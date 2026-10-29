param(
    [Parameter(Mandatory=$true)][string]$GateDirectory,
    [int]$TimeoutMs = 18000,
    [ValidateSet('Candidate','Vendor','Balanced')][string]$ModelMode = 'Candidate',
    [switch]$ExtraHoldout
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$gate = [IO.Path]::GetFullPath($GateDirectory).TrimEnd('\')
$allowed = [IO.Path]::GetFullPath((Join-Path $root 'output\delta-native-gates')).TrimEnd('\') + '\'
if (-not $gate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or
    -not (Split-Path -Leaf $gate).StartsWith('Gate-') -or
    -not (Test-Path -LiteralPath (Join-Path $gate 'delta-runtime-gate-manifest.json'))) {
    throw 'Only an existing disposable Delta Gate is allowed'
}
$expectedDll = '6A6E842F53D7C88674289CD85A2BD65C1A1D71870ED6A565127FE5DB7ACBEBC7'
$expectedH5d = if ($ModelMode -eq 'Balanced') {
    '9405C60991EFA3D6D08A688F7D946DF2276C9C3AA0200F08A31196483DFBEAF0'
} elseif ($ModelMode -eq 'Vendor') {
    '99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73'
} else { '7460AC816B12E6E58A647E92F9CCEB56787BFDD87A26FE9E5D6D3A7F28A7FF89' }
$expectedPbd = if ($ModelMode -eq 'Balanced') {
    'F734108ECCA953715F6CFCE56402556DA9AF1983643D9584A58D42CC20E71ADF'
} elseif ($ModelMode -eq 'Vendor') {
    '47533BA5CDAFBA4ADC1961F417BD64CFBC572EEF59125293893900F5C831F48B'
} else { 'F88D6F19A67C535A739490A6CC65BEC76383A7179498B7803C2427CBF81D207F' }
foreach ($item in @(
    @{path='InspTools.dll'; hash=$expectedDll},
    @{path='Model\R002-CCD1.h5d'; hash=$expectedH5d},
    @{path='Model\R002-CCD1.pbd'; hash=$expectedPbd}
)) {
    $p = Join-Path $gate $item.path
    if (-not (Test-Path -LiteralPath $p -PathType Leaf) -or
        (Get-FileHash -LiteralPath $p).Hash -ne $item.hash) {
        throw ('Candidate Gate artifact does not match pinned SHA: ' + $item.path)
    }
}
$marker = Join-Path $gate 'delta-model-experiment.marker'
if (-not (Test-Path -LiteralPath $marker) -or
    [IO.File]::ReadAllText($marker) -ne 'MODEL_PRECOMPILED_EXPERIMENT_V1') {
    throw 'Precompiled model Gate marker missing or invalid'
}
$recipe = Join-Path $gate 'Recipe\R2PROBE-DIA-3923-R002'
if (-not (Test-Path -LiteralPath (Join-Path $recipe 'Setting.ini'))) {
    throw 'Pinned candidate Recipe missing'
}

$donor = 'D:\delta\Welding\DIAChip8Side\TrainingImage\R002-CCD1\Origional\1'
$samples = @(
    @{label='S1OK'; source='OK\OK-1-2025-06-27-16-54-46.jpg'; file='S1OK.jpg'; hash='1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45'},
    @{label='S1NG'; source='NG\NG-1-2025-06-27-16-55-12.png'; file='S1NG.png'; hash='E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4'}
)
if ($ExtraHoldout) {
    if ($ModelMode -eq 'Candidate') { throw 'Extra holdout is for CCD1 and original vendor only' }
    $samples += @(
        @{label='S1OK2'; source='OK\OK-1-2025-06-27-16-58-19.jpg'; file='S1OK2.jpg'; hash='33C880CBA893658E4CCF1B8141EEBC340352281ECF59BE4640557B337B0421AD'},
        @{label='S1NG2'; source='NG\NG-1-2025-06-27-16-55-24.png'; file='S1NG2.png'; hash='93EE6DC20F726AE57BE288E71D95B51A382FF3B2EE690DDAF3C0453B44AD49BC'}
    )
}
$samplesDir = Join-Path $gate 'TrainingImage\DIA-Candidate-S1'
[IO.Directory]::CreateDirectory($samplesDir) | Out-Null
foreach ($item in $samples) {
    $source = Join-Path $donor $item.source
    $dest = Join-Path $samplesDir $item.file
    if ((Get-FileHash -LiteralPath $source).Hash -ne $item.hash) {
        throw ('Original vendor sample SHA differs: ' + $item.label)
    }
    if (-not (Test-Path -LiteralPath $dest)) {
        Copy-Item -LiteralPath $source -Destination $dest
    }
    if ((Get-FileHash -LiteralPath $dest).Hash -ne $item.hash) {
        throw ('Isolated sample copy SHA differs: ' + $item.label)
    }
}

$cliOriginal = Join-Path $root 'output\decoded-ui\Release\InspToolsRecipeCli.exe'
$cli = Join-Path $gate 'InspToolsRecipeCli.exe'
if (-not (Test-Path -LiteralPath $cli) -or
    (Get-FileHash -LiteralPath $cli).Hash -ne (Get-FileHash -LiteralPath $cliOriginal).Hash) {
    # Replacement affects only the known disposable Gate, never product release.
    Copy-Item -LiteralPath $cliOriginal -Destination $cli -Force
}

$results = @()
$prefix = if ($ModelMode -eq 'Vendor') { 'vendor-inspect-' } elseif ($ModelMode -eq 'Balanced') { 'balanced-inspect-' } else { 'candidate-inspect-' }
$prefix = if ($ExtraHoldout) { $prefix + 'holdout4-' } else { $prefix }
$command = if ($ModelMode -eq 'Vendor') { '--r3-facade-inspect-vendor' } elseif ($ModelMode -eq 'Balanced') { '--r3-facade-inspect-balanced' } else { '--r3-facade-inspect-candidate' }
foreach ($item in $samples) {
    $start = New-Object System.Diagnostics.ProcessStartInfo
    $start.FileName = $cli
    $start.Arguments = $command + ' R2PROBE-DIA-3923-R002 1 ' + $item.label
    $start.WorkingDirectory = $gate
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $start
    if (-not $process.Start()) { throw ('Failed to launch InspectFile sample: ' + $item.label) }
    $outTask = $process.StandardOutput.ReadToEndAsync()
    $errTask = $process.StandardError.ReadToEndAsync()
    $finished = $process.WaitForExit($TimeoutMs)
    if (-not $finished) { $process.Kill();$process.WaitForExit() }
    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $stdout = $outTask.GetAwaiter().GetResult()
    $stderr = $errTask.GetAwaiter().GetResult()
    [IO.File]::WriteAllText((Join-Path $gate ($prefix + $item.label + '.stdout')), $stdout, [Text.Encoding]::UTF8)
    [IO.File]::WriteAllText((Join-Path $gate ($prefix + $item.label + '.stderr')), $stderr, [Text.Encoding]::UTF8)
    $bin = if ($stdout -match 'ACTUAL_BIN=([^\s]+)') { $Matches[1] } else { 'NOT_RETURNED' }
    $locate = if ($stdout -match 'LOCATE_SUCCEEDED=([^\s]+)') { $Matches[1] } else { 'NOT_RETURNED' }
    $aiR = if ($stdout -match 'AI_ENHANCE_OUTPUT_R=READY[^\r\n]*PNG_SHA256=([A-F0-9]{64})') { $Matches[1] } else { '' }
    $passed = $finished -and $exitCode -eq 0 -and
        $stdout.Contains('FACADE_MODEL=PASS') -and
        $stdout.Contains('AI_ENHANCE_OUTPUT=READY') -and
        $stdout.Contains('FACADE_INSPECT_RETURNED') -and
        $stdout.Contains('FACADE_INSPECT=PASS')
    $results += [pscustomobject]@{
        sample=$item.label
        expected_label=$(if ($item.label.StartsWith('S1OK')) { 'OK' } else { 'NG' })
        model_loaded=$stdout.Contains('FACADE_MODEL=PASS')
        inspect_returned=$stdout.Contains('FACADE_INSPECT_RETURNED')
        inspected=$passed
        actual_bin=$bin
        locate_succeeded=$locate
        ai_enhance_output_ready=$stdout.Contains('AI_ENHANCE_OUTPUT=READY')
        ai_enhance_r_png_sha256=$aiR
        exit_code=$exitCode
        finished=$finished
        stderr=$stderr
    }
    Write-Output ('DIA_' + $ModelMode.ToUpperInvariant() + '_INSPECT_SAMPLE=' + $item.label +
        ' STATUS=' + $(if ($passed) { 'PASS' } else { 'NOT_ACCEPTED' }) +
        ' BIN=' + $bin + ' LOCATE=' + $locate + ' EXIT=' + $exitCode +
        ' AI_R_SHA=' + $(if ($aiR) { $aiR.Substring(0,16) } else { 'MISSING' }))
}
$hashesPreserved = (Get-FileHash (Join-Path $gate 'Model\R002-CCD1.h5d')).Hash -eq $expectedH5d -and
    (Get-FileHash (Join-Path $gate 'Model\R002-CCD1.pbd')).Hash -eq $expectedPbd
$accepted = @($results|Where-Object { -not $_.inspected }).Count -eq 0 -and $hashesPreserved
$record = [pscustomobject]@{
    status=$(if ($accepted) { 'PASS' } else { 'NOT_ACCEPTED' })
    gate=$gate
    model_mode=$ModelMode
    extra_holdout=[bool]$ExtraHoldout
    candidate_job=$(if ($ModelMode -eq 'Vendor') { $null } elseif ($ModelMode -eq 'Balanced') { '20261022T044553Z-1fa06812' } else { '20261022T033549Z-8e528262' })
    purpose='Functional offline InspectFile smoke, not model accuracy qualification'
    model_hashes_preserved=$hashesPreserved
    deployed=$false
    samples=$results
}
$record | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $gate ($prefix + 'result.json')) -Encoding UTF8
Write-Output ('DIA_' + $ModelMode.ToUpperInvariant() + '_INSPECT=' + $record.status + ' DEPLOYED=False HASHES_PRESERVED=' + $hashesPreserved)
