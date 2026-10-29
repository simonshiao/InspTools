param(
    [switch]$FromZip,
    [string]$PackageDirectory = ''
)

# Portability acceptance: RELOCATE the whole package before launching it.
# All test Recipe files are copied into the moved package through its own CLI.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$Source = if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
    Join-Path $Root 'output\portable-review'
} else {
    [IO.Path]::GetFullPath($PackageDirectory)
}
$Moved = Join-Path $Root 'output\portable-relocation-test\Moved'
$TestRoot = Split-Path -Parent $Moved
$Fixture = Join-Path $Root 'output\decoded-ui\Release\Recipe\R2PROBE-3923-R002'
$Log = New-Object System.Collections.Generic.List[string]

function Check([bool]$ok, [string]$msg) {
    if (-not $ok) { throw ('PORTABLE_TEST_FAIL: ' + $msg) }
    $Log.Add('PASS ' + $msg)
}
function RunCli([string[]]$parameters, [string]$marker) {
    $exe = Join-Path $Moved 'InspToolsRecipeCli.exe'
    $outputFile = Join-Path $Moved 'relocation-cli.stdout'
    $errorFile = Join-Path $Moved 'relocation-cli.stderr'
    if (Test-Path $outputFile) { Remove-Item $outputFile -Force }
    if (Test-Path $errorFile) { Remove-Item $errorFile -Force }
    $p = Start-Process -FilePath $exe -ArgumentList $parameters -WorkingDirectory $Moved -PassThru -RedirectStandardOutput $outputFile -RedirectStandardError $errorFile
    $done = $p.WaitForExit(9000)
    if (-not $done) { $p.Kill();$p.WaitForExit() }
    $out = if (Test-Path $outputFile) { [IO.File]::ReadAllText($outputFile) } else { '' }
    $err = if (Test-Path $errorFile) { [IO.File]::ReadAllText($errorFile) } else { '' }
    Check ($done -and $out.Contains($marker) -and [string]::IsNullOrWhiteSpace($err)) ('CLI ' + $marker)
}
function RunWpf([string[]]$parameters, [string]$title) {
    if ($parameters.Count -gt 0) {
        $p = Start-Process -FilePath (Join-Path $Moved 'InspToolsStandalone.exe') -ArgumentList $parameters -WorkingDirectory $Moved -PassThru
    } else {
        $p = Start-Process -FilePath (Join-Path $Moved 'InspToolsStandalone.exe') -WorkingDirectory $Moved -PassThru
    }
    Start-Sleep -Seconds 4
    $p.Refresh()
    $alive = -not $p.HasExited
    $winTitle = if ($alive) { $p.MainWindowTitle } else { '' }
    try {
        Check ($alive -and $winTitle.Contains($title)) ('WPF window "' + $title + '" opens from relocated folder')
    }
    finally {
        if (-not $p.HasExited) {
            $p.CloseMainWindow() | Out-Null
            if (-not $p.WaitForExit(4500)) {
                $p.Kill()
                $p.WaitForExit()
                throw 'Relocated WPF process failed to close without force'
            }
        }
    }
    Check ((-not (Test-Path (Join-Path $Moved 'InspToolsStandalone.error.txt')))) 'Relocated GUI no error file'
}

Check (Test-Path (Join-Path $Source 'portable-review.marker')) 'Portable marker present'
Check (Test-Path $Fixture) 'Known non-Shunt test Recipe is available'
if (Test-Path $TestRoot) {
    $expected = [IO.Path]::GetFullPath((Join-Path $Root 'output\portable-relocation-test'))
    Check ([IO.Path]::GetFullPath($TestRoot) -eq $expected) 'Disposable relocation target is exact'
    Remove-Item -LiteralPath $TestRoot -Recurse -Force
}
[IO.Directory]::CreateDirectory($Moved) | Out-Null
if ($FromZip) {
    if (-not [string]::IsNullOrWhiteSpace($PackageDirectory)) {
        throw 'FromZip uses the audited default review ZIP; use directory mode for a persistent candidate'
    }
    $archive = Join-Path $Root 'output\InspTools-Portable-Review-R3.zip'
    Check (Test-Path -LiteralPath $archive) 'Distributable review ZIP exists'
    Expand-Archive -LiteralPath $archive -DestinationPath $Moved
    $Log.Add('PASS Portable ZIP extracted at relocated AppBase')
} else {
    & robocopy $Source $Moved /E /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -gt 7) { throw 'Relocated package copy failed' }
}
Check ((Get-FileHash (Join-Path $Moved 'InspTools.dll')).Hash -eq
       (Get-FileHash (Join-Path $Source 'InspTools.dll')).Hash) 'R3 DLL hash preserved after copy'
Check (-not (Test-Path (Join-Path $Moved 'Recipe\R2PROBE-3923-R002'))) 'Portable package ships without sample Recipe'
Check (-not (Test-Path (Join-Path $Moved 'TrainingImage\R3-Shunt-CCD1'))) 'No ShuntVC test images in distributed package'
RunCli @('--recipe-list') '' # no error, empty Recipe
RunWpf @() 'INSPECTION TOOLS'
RunCli @('--recipe-import', $Fixture, 'R2PROBE-3923-R002') 'R2PROBE-3923-R002'
RunCli @('--recipe-check','R2PROBE-3923-R002') 'RECIPE_VALID='
RunCli @('--recipe-params','R2PROBE-3923-R002') 'S5'
RunWpf @('--recipe-native-r3','R2PROBE-3923-R002','--station','5') 'R3 '
$Log.Add('PORTABLE_RELOCATION=PASS')
$Report = Join-Path $TestRoot 'portable-acceptance.txt'
$Log | Set-Content -LiteralPath $Report -Encoding UTF8
$Log
Write-Output ('REPORT=' + $Report)
