param(
    [Parameter(Mandatory=$true)][string]$GateDirectory,
    [int]$ModelTimeoutMs = 16000
)

# Runtime + model acceptance in disposable independent AppBase only.
# Original Shunt and current official release are read-only.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath($PSScriptRoot)
$base = [IO.Path]::GetFullPath($GateDirectory).TrimEnd('\')
$parent = [IO.Path]::GetFullPath((Join-Path $Root 'output\delta-native-gates')).TrimEnd('\')
if (-not $base.StartsWith($parent + '\', [StringComparison]::OrdinalIgnoreCase) -or
    -not (Split-Path -Leaf $base).StartsWith('Gate-') -or
    -not (Test-Path -LiteralPath (Join-Path $base 'delta-runtime-gate-manifest.json'))) {
    throw 'Refusing non-disposable Delta Gate directory'
}
$native = Join-Path $base 'InspTools.dll'
if ((Get-FileHash -LiteralPath $native -Algorithm SHA256).Hash -ne
    'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B') {
    throw 'R3 native DLL mismatch'
}
Copy-Item -LiteralPath (Join-Path $Root 'output\decoded-ui\Release\InspToolsRecipeCli.exe') -Destination $base -Force
& (Join-Path $Root 'StageShuntFixtures.ps1') -Sandbox $base -IncludeS1Model | Out-Null

function Probe([string]$label, [string[]]$argv, [int]$timeout) {
    $outFile = Join-Path $base ($label + '.out')
    $errFile = Join-Path $base ($label + '.err')
    $p = Start-Process -FilePath (Join-Path $base 'InspToolsRecipeCli.exe') `
        -ArgumentList $argv -WorkingDirectory $base -PassThru `
        -RedirectStandardOutput $outFile -RedirectStandardError $errFile
    $finished = $p.WaitForExit($timeout)
    if (-not $finished) { $p.Kill();$p.WaitForExit() }
    return [pscustomobject]@{
        stage=$label
        finished=$finished
        stdout=$(if (Test-Path $outFile) { [IO.File]::ReadAllText($outFile) } else { '' })
        stderr=$(if (Test-Path $errFile) { [IO.File]::ReadAllText($errFile) } else { '' })
    }
}

$loader = Probe 'csharp-private-runtime' @('--delta-runtime-probe') 13000
$result = [pscustomobject]@{
    kind = 'delta-app-local-model-gate'
    gate = $base
    runtime_ready = $loader.finished -and $loader.stdout.Contains('DELTA_NATIVE_RUNTIME=READY')
    model_test = $null
    delta_loader_stdout = $loader.stdout
    source_unchanged = $true
}
if ($result.runtime_ready) {
    $model = Probe 'delta-native-model-s1' @(
        '--r3-facade-model','R2PROBE-SHUNT-3923-R002','1') $ModelTimeoutMs
    $result.model_test = [pscustomobject]@{
        finished=$model.finished
        loaded=($model.finished -and $model.stdout.Contains('FACADE_MODEL=PASS'))
        status=$(
            if (-not $model.finished) { 'TIMED_OUT_KILLED' }
            elseif ($model.stdout.Contains('FACADE_MODEL=PASS')) { 'PASS' }
            else { 'MODEL_NOT_LOADED' }
        )
        output=$model.stdout
        errors=$model.stderr
    }
}
$result | ConvertTo-Json -Depth 6
