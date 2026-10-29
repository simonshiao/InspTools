param([ValidateSet('Vendor','Balanced')][string]$ModelMode = 'Vendor')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$expectedDll = '6A6E842F53D7C88674289CD85A2BD65C1A1D71870ED6A565127FE5DB7ACBEBC7'
$balancedJob = Join-Path $app 'TrainingCandidates\20261022T044553Z-1fa06812'
$vendorFiles = if ($ModelMode -eq 'Balanced') {
    @(
        @{name='R002-CCD1.h5d'; source=(Join-Path $balancedJob 'candidate.h5d'); hash='9405C60991EFA3D6D08A688F7D946DF2276C9C3AA0200F08A31196483DFBEAF0'},
        @{name='R002-CCD1.pbd'; source=(Join-Path $balancedJob 'candidate.pbd'); hash='F734108ECCA953715F6CFCE56402556DA9AF1983643D9584A58D42CC20E71ADF'}
    )
} else {
    @(
        @{name='R002-CCD1.h5d'; source=(Join-Path $app 'Model\R002-CCD1.h5d'); hash='99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73'},
        @{name='R002-CCD1.pbd'; source=(Join-Path $app 'Model\R002-CCD1.pbd'); hash='47533BA5CDAFBA4ADC1961F417BD64CFBC572EEF59125293893900F5C831F48B'}
    )
}
$experiment = Join-Path $root 'output\delta-model-experiment\generated-source\bin\Release\net472\InspTools.dll'
if ((Get-FileHash -LiteralPath $experiment).Hash -ne $expectedDll) {
    throw 'Experimental ReadModel DLL changed'
}
foreach ($entry in $vendorFiles) {
    if ((Get-FileHash -LiteralPath $entry.source).Hash -ne $entry.hash) {
        throw ('Pinned '+$ModelMode+' model differs: ' + $entry.name)
    }
}
if ($ModelMode -eq 'Balanced') {
    $conversion = [IO.File]::ReadAllText((Join-Path $balancedJob 'conversion_result.json')) |
        ConvertFrom-Json
    if ($conversion.status -ne 'PASS' -or $conversion.deployed -or
        $conversion.h5d_sha256 -ne $vendorFiles[0].hash -or
        $conversion.pbd_sha256 -ne $vendorFiles[1].hash) {
        throw 'Balanced CCD1 conversion proof missing'
    }
}

# Create a NEW AppBase. Never swap model bytes in a running or prior Gate.
$stageOutput = & (Join-Path $root 'scripts\StageDeltaRuntimeGate.ps1')
if (-not $stageOutput) { throw 'Disposable Delta Gate could not be created' }
$stage = ($stageOutput | Out-String | ConvertFrom-Json)
$gate = [IO.Path]::GetFullPath($stage.app_base)
$allowed = [IO.Path]::GetFullPath((Join-Path $root 'output\delta-native-gates')).TrimEnd('\') + '\'
if (-not $gate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or
    -not (Test-Path -LiteralPath (Join-Path $gate 'delta-runtime-gate-manifest.json'))) {
    throw 'Unexpected Gate staging location'
}
Write-Output ('DIA_VENDOR_GATE=' + $gate)
& (Join-Path $root 'scripts\ExtendDeltaRuntimeGate.ps1') -GateDirectory $gate -IncludeCudnnInference | Out-Null

$cli = Join-Path $gate 'InspToolsRecipeCli.exe'
& $cli --recipe-import (Join-Path $app 'Recipe\3923-R002') 'R2PROBE-DIA-3923-R002'
if ($LASTEXITCODE -ne 0) { throw 'Vendor baseline Recipe import failed' }
& $cli --recipe-check 'R2PROBE-DIA-3923-R002'
if ($LASTEXITCODE -ne 0) { throw 'Vendor baseline Recipe invalid' }
foreach ($entry in $vendorFiles) {
    $source = $entry.source
    $dest = Join-Path $gate ('Model\' + $entry.name)
    if (Test-Path -LiteralPath $dest) { throw 'Gate model already exists' }
    Copy-Item -LiteralPath $source -Destination $dest
    if ((Get-FileHash -LiteralPath $dest).Hash -ne $entry.hash) {
        throw ('Gate original model copy differs: ' + $entry.name)
    }
}
$native = Join-Path $gate 'InspTools.dll'
$expectedOriginalR3 = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
if ((Get-FileHash -LiteralPath $native).Hash -ne $expectedOriginalR3) {
    throw 'Unexpected R3 sandbox DLL before isolated experiment'
}
Copy-Item -LiteralPath $experiment -Destination $native -Force
if ((Get-FileHash -LiteralPath $native).Hash -ne $expectedDll) { throw 'Baseline Gate experimental DLL mismatch' }
[IO.File]::WriteAllText((Join-Path $gate 'delta-model-experiment.marker'), 'MODEL_PRECOMPILED_EXPERIMENT_V1')
Write-Output ('DIA_'+$ModelMode.ToUpperInvariant()+'_BASELINE_STAGE=PASS SOURCE_READ_ONLY=True DEPLOYED=False')
Write-Output ('DIA_'+$ModelMode.ToUpperInvariant()+'_GATE=' + $gate)
