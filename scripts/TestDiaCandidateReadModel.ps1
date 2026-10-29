param([int]$TimeoutMs = 20000, [string]$GateDirectory = '')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$job = Join-Path $app 'TrainingCandidates\20261022T033549Z-8e528262'
$conversion = Join-Path $job 'conversion_result.json'
$experiment = Join-Path $root 'output\delta-model-experiment\generated-source\bin\Release\net472\InspTools.dll'
$expectedExperiment = '6A6E842F53D7C88674289CD85A2BD65C1A1D71870ED6A565127FE5DB7ACBEBC7'
$expectedH5d = '7460AC816B12E6E58A647E92F9CCEB56787BFDD87A26FE9E5D6D3A7F28A7FF89'
$expectedPbd = 'F88D6F19A67C535A739490A6CC65BEC76383A7179498B7803C2427CBF81D207F'
if (-not (Test-Path -LiteralPath $conversion)) { throw 'Candidate conversion result is missing' }
$record = ([IO.File]::ReadAllText($conversion, [Text.Encoding]::UTF8) | ConvertFrom-Json)
if ($record.status -ne 'PASS' -or $record.deployed -or
    $record.h5d_sha256 -ne $expectedH5d -or $record.pbd_sha256 -ne $expectedPbd) {
    throw 'Candidate conversion acceptance does not match pinned result'
}
if ((Get-FileHash -LiteralPath $experiment).Hash -ne $expectedExperiment) {
    throw 'Isolated experimental InspTools.dll changed'
}

if ($GateDirectory) {
    $gate = [IO.Path]::GetFullPath($GateDirectory)
} else {
    $stageOutput = & (Join-Path $root 'scripts\StageDeltaRuntimeGate.ps1')
    if (-not $stageOutput) { throw 'Could not create disposable Delta Gate' }
    $stage = ($stageOutput | Out-String | ConvertFrom-Json)
    $gate = [IO.Path]::GetFullPath($stage.app_base)
}
$allowed = [IO.Path]::GetFullPath((Join-Path $root 'output\delta-native-gates')).TrimEnd('\') + '\'
if (-not $gate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or
    -not (Test-Path -LiteralPath (Join-Path $gate 'delta-runtime-gate-manifest.json'))) {
    throw 'A new disposable runtime Gate is required'
}
Write-Output ('DIA_CANDIDATE_GATE=' + $gate)
& (Join-Path $root 'scripts\ExtendDeltaRuntimeGate.ps1') -GateDirectory $gate -IncludeCudnnInference | Out-Null

$cli = Join-Path $gate 'InspToolsRecipeCli.exe'
$sourceRecipe = Join-Path $app 'Recipe\3923-R002'
if (-not (Test-Path -LiteralPath (Join-Path $gate 'Recipe\R2PROBE-DIA-3923-R002'))) {
    & $cli --recipe-import $sourceRecipe 'R2PROBE-DIA-3923-R002'
    if ($LASTEXITCODE -ne 0) { throw 'Copy-only Recipe import into Gate failed' }
}
& $cli --recipe-check 'R2PROBE-DIA-3923-R002'
if ($LASTEXITCODE -ne 0) { throw 'Imported DIA Recipe is invalid' }

foreach ($entry in @(
    @{ source=(Join-Path $job 'candidate.h5d'); dest='Model\R002-CCD1.h5d'; hash=$expectedH5d },
    @{ source=(Join-Path $job 'candidate.pbd'); dest='Model\R002-CCD1.pbd'; hash=$expectedPbd }
)) {
    if ((Get-FileHash -LiteralPath $entry.source).Hash -ne $entry.hash) {
        throw ('Candidate source changed: ' + $entry.source)
    }
    $target = Join-Path $gate $entry.dest
    if (-not (Test-Path -LiteralPath $target)) {
        Copy-Item -LiteralPath $entry.source -Destination $target
    }
    if ((Get-FileHash -LiteralPath $target).Hash -ne $entry.hash) {
        throw ('Gate candidate model checksum failed: ' + $target)
    }
}

# Only this disposable Gate gets the experimental precompiled ReadModel branch.
$gateDll = Join-Path $gate 'InspTools.dll'
$gateDllHash = (Get-FileHash -LiteralPath $gateDll).Hash
if ($gateDllHash -ne $expectedExperiment) {
    if ($gateDllHash -ne 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B') {
        throw 'Unrecognized experiment Gate DLL; refusing overwrite'
    }
    Copy-Item -LiteralPath $experiment -Destination $gateDll -Force
}
if ((Get-FileHash -LiteralPath (Join-Path $gate 'InspTools.dll')).Hash -ne $expectedExperiment) {
    throw 'Gate experimental DLL mismatch'
}
$marker = Join-Path $gate 'delta-model-experiment.marker'
if (Test-Path -LiteralPath $marker) {
    if ([IO.File]::ReadAllText($marker) -ne 'MODEL_PRECOMPILED_EXPERIMENT_V1') {
        throw 'Existing experiment Gate marker mismatch'
    }
} else {
    [IO.File]::WriteAllText($marker, 'MODEL_PRECOMPILED_EXPERIMENT_V1')
}

$out = Join-Path $gate 'candidate-readmodel.stdout'
$err = Join-Path $gate 'candidate-readmodel.stderr'
$start = New-Object System.Diagnostics.ProcessStartInfo
$start.FileName = $cli
$start.Arguments = '--r3-facade-model-candidate R2PROBE-DIA-3923-R002 1'
$start.WorkingDirectory = $gate
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$process = New-Object System.Diagnostics.Process
$process.StartInfo = $start
if (-not $process.Start()) { throw 'Candidate model probe process did not start' }
$outTask = $process.StandardOutput.ReadToEndAsync()
$errTask = $process.StandardError.ReadToEndAsync()
$finished = $process.WaitForExit($TimeoutMs)
if (-not $finished) { $process.Kill();$process.WaitForExit() }
$process.WaitForExit()
$exitCode = $process.ExitCode
$stdout = $outTask.GetAwaiter().GetResult()
$stderr = $errTask.GetAwaiter().GetResult()
[IO.File]::WriteAllText($out, $stdout, [Text.Encoding]::UTF8)
[IO.File]::WriteAllText($err, $stderr, [Text.Encoding]::UTF8)
$allHashesMatch = (Get-FileHash (Join-Path $gate 'Model\R002-CCD1.h5d')).Hash -eq $expectedH5d -and
    (Get-FileHash (Join-Path $gate 'Model\R002-CCD1.pbd')).Hash -eq $expectedPbd
$passed = $finished -and $exitCode -eq 0 -and
    $stdout.Contains('APP_LOCAL_DELTA_RUNTIME=READY') -and
    $stdout.Contains('FACADE_MODEL=PASS') -and $allHashesMatch
$result = [pscustomobject]@{
    status=$(if ($passed) { 'PASS' } else { 'NOT_ACCEPTED' })
    gate=$gate
    candidate_job='20261022T033549Z-8e528262'
    readmodel_loaded=$stdout.Contains('FACADE_MODEL=PASS')
    finished=$finished
    exit_code=$exitCode
    candidate_hashes_preserved=$allHashesMatch
    deployed=$false
    stdout=$stdout
    stderr=$stderr
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $gate 'candidate-readmodel-result.json') -Encoding UTF8
Write-Output ('DIA_CANDIDATE_READMODEL=' + $result.status)
Write-Output ('DIA_CANDIDATE_MODEL_LOADED=' + $result.readmodel_loaded)
Write-Output ('DIA_CANDIDATE_MODEL_HASHES_PRESERVED=' + $allHashesMatch)
Write-Output ('DIA_CANDIDATE_GATE=' + $gate)
if (-not $passed) { Write-Output ('DIA_CANDIDATE_STDERR=' + $stderr.Substring(0,[Math]::Min(800,$stderr.Length))) }
