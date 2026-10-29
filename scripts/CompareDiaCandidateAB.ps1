param(
    [string]$CandidateGate = 'output\delta-native-gates\Gate-20261022-115857',
    [string]$VendorGate = 'output\delta-native-gates\Gate-20261022-122900',
    [ValidateSet('Candidate','Balanced')][string]$CandidateMode = 'Candidate',
    [switch]$ExtraHoldout
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$jobId = if ($CandidateMode -eq 'Balanced') {
    '20261022T044553Z-1fa06812'
} else { '20261022T033549Z-8e528262' }
$job = Join-Path $app ('TrainingCandidates\' + $jobId)
if ($ExtraHoldout -and $CandidateMode -ne 'Balanced') {
    throw 'Four-image holdout is restricted to CCD1 balanced smoke and the original baseline'
}
$allowed = [IO.Path]::GetFullPath((Join-Path $root 'output\delta-native-gates')).TrimEnd('\') + '\'

function ReadGate([string]$path, [string]$mode) {
    $gate = [IO.Path]::GetFullPath((Join-Path $root $path)).TrimEnd('\')
    if (-not $gate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or
        -not (Split-Path -Leaf $gate).StartsWith('Gate-') -or
        -not (Test-Path -LiteralPath (Join-Path $gate 'delta-runtime-gate-manifest.json'))) {
        throw ('Invalid disposable ' + $mode + ' Gate')
    }
    $stem = if ($mode -eq 'Vendor') { 'vendor-inspect-' } elseif ($mode -eq 'Balanced') { 'balanced-inspect-' } else { 'candidate-inspect-' }
    $name = $stem + $(if ($ExtraHoldout) { 'holdout4-' } else { '' }) + 'result.json'
    $file = Join-Path $gate $name
    if (-not (Test-Path -LiteralPath $file)) { throw ('Missing ' + $name) }
    $data = [IO.File]::ReadAllText($file, [Text.Encoding]::UTF8) | ConvertFrom-Json
    if ($data.status -ne 'PASS' -or $data.model_mode -ne $mode -or
        $data.deployed -or -not $data.model_hashes_preserved -or
        @($data.samples).Count -ne $(if ($ExtraHoldout) { 4 } else { 2 })) {
        throw ('Unaccepted ' + $mode + ' InspectFile evidence')
    }
    if ($mode -ne 'Vendor' -and $data.candidate_job -ne $jobId) {
        throw 'Candidate A/B report refers to a different training job'
    }
    return $data
}

$candidate = ReadGate $CandidateGate $CandidateMode
$vendor = ReadGate $VendorGate 'Vendor'
if ([IO.Path]::GetFullPath($candidate.gate) -eq [IO.Path]::GetFullPath($vendor.gate)) {
    throw 'A/B must use independent runtime Gates'
}
$pairs = @()
$expectedSourceSha = @{
    S1OK = '1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45'
    S1NG = 'E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4'
    S1OK2 = '33C880CBA893658E4CCF1B8141EEBC340352281ECF59BE4640557B337B0421AD'
    S1NG2 = '93EE6DC20F726AE57BE288E71D95B51A382FF3B2EE690DDAF3C0453B44AD49BC'
}
$samplesToCheck = if ($ExtraHoldout) { @('S1OK','S1NG','S1OK2','S1NG2') } else { @('S1OK','S1NG') }
foreach ($sample in $samplesToCheck) {
    $original = @($vendor.samples | Where-Object { $_.sample -eq $sample })
    $trained = @($candidate.samples | Where-Object { $_.sample -eq $sample })
    if ($original.Count -ne 1 -or $trained.Count -ne 1) {
        throw ('Missing paired A/B sample ' + $sample)
    }
    foreach ($test in @($original[0],$trained[0])) {
        if (-not $test.inspected -or -not $test.ai_enhance_output_ready -or
            $test.exit_code -ne 0 -or $test.locate_succeeded -ne 'True' -or
            $test.ai_enhance_r_png_sha256 -notmatch '^[0-9A-F]{64}$') {
            throw ('Incomplete actual AI Enhance evidence: ' + $sample)
        }
    }
    $sourceOk = $true
    foreach ($spec in @(
        @{ gate=$vendor.gate; prefix=('vendor-inspect-' + $(if ($ExtraHoldout) { 'holdout4-' } else { '' })) },
        @{ gate=$candidate.gate; prefix=$(if ($CandidateMode -eq 'Balanced') { 'balanced-inspect-' } else { 'candidate-inspect-' }) + $(if ($ExtraHoldout) { 'holdout4-' } else { '' }) }
    )) {
        $path = Join-Path $spec.gate ($spec.prefix + $sample + '.stdout')
        $output = [IO.File]::ReadAllText($path, [Text.Encoding]::UTF8)
        if (-not $output.Contains('SOURCE_LABEL=' + $sample + ' IMAGE_SHA=' + $expectedSourceSha[$sample])) {
            $sourceOk = $false
        }
    }
    if (-not $sourceOk) { throw ('A/B source picture hash differs: ' + $sample) }
    $expectedBin = if ($sample.StartsWith('S1OK')) { 'Ok' } else { 'Ng2' }
    if ($original[0].actual_bin -ne $expectedBin) {
        throw ('Vendor model did not reproduce expected baseline ' + $sample)
    }
    $pairs += [pscustomobject]@{
        sample=$sample
        expected=$expectedBin
        vendor_bin=$original[0].actual_bin
        candidate_bin=$trained[0].actual_bin
        candidate_matches_reference=($trained[0].actual_bin -eq $original[0].actual_bin)
        enhance_output_changed=($trained[0].ai_enhance_r_png_sha256 -ne $original[0].ai_enhance_r_png_sha256)
        vendor_ai_r_sha256=$original[0].ai_enhance_r_png_sha256
        candidate_ai_r_sha256=$trained[0].ai_enhance_r_png_sha256
        original_image_sha256=$expectedSourceSha[$sample]
    }
}
$wrong = @($pairs|Where-Object { -not $_.candidate_matches_reference }).Count
$changed = @($pairs|Where-Object { $_.enhance_output_changed }).Count
# Two examples can BLOCK an unsafe model, but can never QUALIFY a production model.
$status = if ($wrong -gt 0) { 'BLOCKED' } else { 'SMOKE_MATCH_NOT_QUALIFIED' }
$record = [pscustomobject]@{
    gate=$status
    checked_at_utc=[DateTime]::UtcNow.ToString('o')
    candidate_job=$jobId
    recipe='3923-R002'
    station=1
    reference_model='R002-CCD1.h5d'
    candidate_mode=$CandidateMode
    inspected_count=$pairs.Count
    extra_holdout=[bool]$ExtraHoldout
    mismatched_reference_count=$wrong
    changed_ai_enhance_count=$changed
    source_sha_verified=$true
    vendor_gate=$vendor.gate
    candidate_gate=$candidate.gate
    model_deployed=$false
    samples=$pairs
    limitation=$(if ($CandidateMode -eq 'Balanced') {
        'One epoch, exactly 4 OK + 4 NG CCD1 unaugmented families; 2 held-out samples checked; not production qualification'
    } else { 'One epoch, <=8 CCD4 NG samples trained; S1 regression smoke only; not production qualification' })
}
$report = Join-Path $job $(if ($ExtraHoldout) { 'qualification_holdout4_result.json' } else { 'qualification_result.json' })
if (Test-Path -LiteralPath $report) { throw 'Existing qualification result must not be overwritten' }
[IO.File]::WriteAllText($report, ($record | ConvertTo-Json -Depth 6), [Text.Encoding]::UTF8)
Write-Output ('DIA_PROMOTION_GATE=' + $status + ' DIFFERING_BINS=' + $wrong +
    ' OF=' + $pairs.Count + ' AI_OUTPUT_DIFFERENCES=' + $changed +
    ' DEPLOYED=False')
Write-Output ('DIA_QUALIFICATION_REPORT=' + $report)
