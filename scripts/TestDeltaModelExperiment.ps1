param(
    [Parameter(Mandatory=$true)][string]$GateDirectory,
    [int]$TimeoutMs = 16000,
    [switch]$InspectSamples,
    [ValidateSet('S1OK','S1NG','Both')][string]$Sample = 'Both'
)

# Disposable and reversible A/B test of a single vendor C# trainer precheck.
# No source Runtime, official release/current or ProjectHub files are written.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$Gate = [IO.Path]::GetFullPath($GateDirectory).TrimEnd('\')
$allowed = [IO.Path]::GetFullPath((Join-Path $Root 'output\delta-native-gates')).TrimEnd('\') + '\'
if (-not $Gate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or
    -not (Split-Path -Leaf $Gate).StartsWith('Gate-') -or
    -not (Test-Path -LiteralPath (Join-Path $Gate 'delta-runtime-gate-manifest.json'))) {
    throw 'This experiment only accepts an existing disposable Delta Gate'
}
$OriginalHash = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
$ExperimentHash = '6A6E842F53D7C88674289CD85A2BD65C1A1D71870ED6A565127FE5DB7ACBEBC7'
$original = Join-Path $Gate 'InspTools.dll'
$experiment = Join-Path $Root 'output\delta-model-experiment\generated-source\bin\Release\net472\InspTools.dll'
$backup = Join-Path $Gate '_temporary_InspTools_precompiled_restore.dll'
$marker = Join-Path $Gate 'delta-model-experiment.marker'
$cli = Join-Path $Root 'output\decoded-ui\Release\InspToolsRecipeCli.exe'
$output = Join-Path $Gate 'model-precompiled-experiment.stdout'
$errors = Join-Path $Gate 'model-precompiled-experiment.stderr'
if ((Get-FileHash -LiteralPath $original).Hash -ne $OriginalHash) { throw 'Original R3 SHA drift' }
if ((Get-FileHash -LiteralPath $experiment).Hash -ne $ExperimentHash) { throw 'Experiment DLL SHA drift' }
if (Test-Path -LiteralPath $backup) { throw 'Stale model experiment restore backup exists' }
if (Test-Path -LiteralPath $marker) { throw 'Stale model experiment marker exists' }
if (-not (Test-Path -LiteralPath (Join-Path $Gate 'Model\VSMA8420AYM050JEW_A1.pbd')) -or
    -not (Test-Path -LiteralPath (Join-Path $Gate 'Recipe\R2PROBE-SHUNT-3923-R002'))) {
    throw 'Stage Shunt Recipe and exact S1 model pair inside the Gate first'
}
Copy-Item -LiteralPath $cli -Destination (Join-Path $Gate 'InspToolsRecipeCli.exe') -Force
$filesToCheck = @(
    (Join-Path $Gate 'Model\VSMA8420AYM050JEW_A1.h5d'),
    (Join-Path $Gate 'Model\VSMA8420AYM050JEW_A1.pbd')
)
$hashes = @($filesToCheck | ForEach-Object { (Get-FileHash -LiteralPath $_).Hash })
$savedModels = @($filesToCheck | ForEach-Object { [IO.File]::ReadAllBytes($_) })
$finished = $false
$exitStatus = $null
$stdOut = ''
$stdErr = ''
$modelsChanged = $false
$inspectOutcomes = @()
try {
    Copy-Item -LiteralPath $original -Destination $backup
    Copy-Item -LiteralPath $experiment -Destination $original -Force
    [IO.File]::WriteAllText($marker, 'MODEL_PRECOMPILED_EXPERIMENT_V1')
    if ((Get-FileHash -LiteralPath $original).Hash -ne $ExperimentHash) {
        throw 'Experiment DLL replacement checksum mismatch'
    }
    $p = Start-Process -FilePath (Join-Path $Gate 'InspToolsRecipeCli.exe') `
        -ArgumentList @('--r3-facade-model','R2PROBE-SHUNT-3923-R002','1') `
        -WorkingDirectory $Gate -RedirectStandardOutput $output `
        -RedirectStandardError $errors -PassThru
    $finished = $p.WaitForExit($TimeoutMs)
    if (-not $finished) { $p.Kill();$p.WaitForExit() }
    $p.Refresh()
    $exitStatus = $p.ExitCode
    if (Test-Path -LiteralPath $output) { $stdOut = [IO.File]::ReadAllText($output) }
    if (Test-Path -LiteralPath $errors) { $stdErr = [IO.File]::ReadAllText($errors) }
    if ($InspectSamples -and $finished -and $stdOut.Contains('FACADE_MODEL=PASS')) {
        $labels = if ($Sample -eq 'Both') { @('S1OK','S1NG') } else { @($Sample) }
        foreach ($label in $labels) {
            $inspectOut = Join-Path $Gate ('model-enhance-inspect-' + $label + '.stdout')
            $inspectErr = Join-Path $Gate ('model-enhance-inspect-' + $label + '.stderr')
            $imageProc = Start-Process -FilePath (Join-Path $Gate 'InspToolsRecipeCli.exe') `
                -ArgumentList @('--r3-facade-inspect-enhance','R2PROBE-SHUNT-3923-R002','1',$label) `
                -WorkingDirectory $Gate -RedirectStandardOutput $inspectOut `
                -RedirectStandardError $inspectErr -PassThru
            $imageDone = $imageProc.WaitForExit($TimeoutMs)
            if (-not $imageDone) { $imageProc.Kill();$imageProc.WaitForExit() }
            $imageText = if (Test-Path $inspectOut) { [IO.File]::ReadAllText($inspectOut) } else { '' }
            $imageError = if (Test-Path $inspectErr) { [IO.File]::ReadAllText($inspectErr) } else { '' }
            $expectedBin = if ($label -eq 'S1OK') { 'Ok' } else { 'Ng2' }
            $observedBin = if ($imageText -match 'ACTUAL_BIN=([^\s]+)') {
                $Matches[1]
            } else {
                'NOT_RETURNED'
            }
            $inspectOutcomes += [pscustomobject]@{
                image=$label
                finished=$imageDone
                model_loaded=$imageText.Contains('FACADE_MODEL=PASS')
                inspect_returned=$imageText.Contains('FACADE_INSPECT_RETURNED')
                expected_bin=$expectedBin
                observed_bin=$observedBin
                bin_matches_reference=($imageDone -and $imageText.Contains('FACADE_INSPECT=PASS') -and
                    $observedBin -eq $expectedBin)
                actual_result=$(if ($imageText -match 'ACTUAL_BIN=[^\r\n]*') { $Matches[0] } else { 'NOT_RETURNED' })
                log=$imageText
                stderr=$imageError
            }
        }
    }
}
finally {
    foreach ($index in 0..1) {
        if (-not (Test-Path -LiteralPath $filesToCheck[$index]) -or
            (Get-FileHash -LiteralPath $filesToCheck[$index]).Hash -ne $hashes[$index]) {
            $modelsChanged = $true
            [IO.File]::WriteAllBytes($filesToCheck[$index], $savedModels[$index])
        }
    }
    if (Test-Path -LiteralPath $backup) {
        Copy-Item -LiteralPath $backup -Destination $original -Force
        Remove-Item -LiteralPath $backup -Force
    }
    if (Test-Path -LiteralPath $marker) { Remove-Item -LiteralPath $marker -Force }
    if ((Get-FileHash -LiteralPath $original).Hash -ne $OriginalHash) {
        throw 'FATAL: failed to restore original pinned R3 DLL'
    }
}
$result = [pscustomobject]@{
    kind = 'delta-precompiled-model-branch-A-B-test'
    finished = $finished
    exit_code = $exitStatus
    model_loaded = ($stdOut -match 'FACADE_MODEL=PASS')
    runtime_ready = ($stdOut -match 'APP_LOCAL_DELTA_RUNTIME=READY')
    result = $(if (-not $finished) { 'TIMEOUT_KILLED' }
               elseif ($stdOut -notmatch 'FACADE_MODEL=PASS') { 'NOT_LOADED' }
               elseif ($InspectSamples -and @($inspectOutcomes|Where-Object {
                   -not $_.bin_matches_reference
               }).Count -gt 0) { 'INSPECTION_NOT_ACCEPTED' }
               else { 'PASS' })
    model_files_modified_during_trial = $modelsChanged
    pinned_r3_dll_restored = $true
    inspections = $inspectOutcomes
    stdout = $stdOut
    stderr = $stdErr
}
$reportPath = Join-Path $Root 'output\delta-model-experiment\model-gate-result.json'
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $reportPath -Encoding UTF8
$result | ConvertTo-Json -Depth 5
