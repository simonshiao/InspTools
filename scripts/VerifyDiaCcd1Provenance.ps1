$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$jobId = '20261022T044553Z-1fa06812'
$job = Join-Path $app ('TrainingCandidates\' + $jobId)
$dataset = 'DIA_CCD1_S1_BalancedSmoke'

function ReadJson([string]$p) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { throw "Evidence file missing: $p" }
    return ([IO.File]::ReadAllText($p,[Text.Encoding]::UTF8) | ConvertFrom-Json)
}
function Digest([string]$p) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { throw "Evidence source missing: $p" }
    return (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash
}
$manifest = ReadJson (Join-Path $app ('TrainingPlans\' + $dataset + '.json'))
$train = ReadJson (Join-Path $job 'training_result.json')
$conversion = ReadJson (Join-Path $job 'conversion_result.json')
$quality = ReadJson (Join-Path $job 'qualification_holdout4_result.json')
$subsetPath = Join-Path $job 'compat_dataset_temp\pair_manifest.json'
$subset = @(ReadJson $subsetPath)
$validParent = [IO.Path]::GetFullPath((Join-Path $app ('TrainingImage\' + $dataset))).TrimEnd('\') + '\'

if ($manifest.dataset -ne $dataset -or
    -not $manifest.source_read_only -or -not $manifest.train_smoke_only -or
    -not $manifest.not_approved_for_quality_or_deployment -or
    $manifest.source_full_pair_count -ne 53 -or
    $manifest.source_ok_pairs -ne 21 -or $manifest.source_ng_pairs -ne 32 -or
    $manifest.source_unique_ng_families -ne 8 -or
    @($manifest.training_pairs).Count -ne 8 -or
    @($manifest.holdout_originals).Count -ne 4) {
    throw 'CCD1 dataset manifest does not describe the independently audited 53 pairs'
}
$trainFamilies = @($manifest.training_pairs | ForEach-Object { $_.family })
$holdoutFamilies = @($manifest.holdout_originals | ForEach-Object { $_.family })
if (@($trainFamilies | Select-Object -Unique).Count -ne 8 -or
    @($holdoutFamilies | Select-Object -Unique).Count -ne 4) {
    throw 'Duplicate family found in training or reserved original holdout'
}
foreach ($family in $trainFamilies) {
    if ($holdoutFamilies -contains $family) { throw "Train/holdout family leakage: $family" }
}
if (@($manifest.training_pairs | Where-Object { $_.label -eq 'OK' }).Count -ne 4 -or
    @($manifest.training_pairs | Where-Object { $_.label -eq 'NG' }).Count -ne 4 -or
    @($manifest.holdout_originals | Where-Object { $_.label -eq 'OK' }).Count -ne 2 -or
    @($manifest.holdout_originals | Where-Object { $_.label -eq 'NG' }).Count -ne 2) {
    throw 'Training/holdout class balance has changed'
}

$expected = @{}
foreach ($pair in $manifest.training_pairs) {
    $rel = $pair.label + '\' + $pair.name
    if ($expected.ContainsKey($rel)) { throw 'Duplicated training relative path' }
    $expected[$rel] = $pair
}
if ($subset.Count -ne 8) {
    throw "Real recovered DAVS subset did not consume exactly 8 X/Y pairs"
}
if ([IO.Path]::GetFullPath($train.subset_manifest) -ne [IO.Path]::GetFullPath($subsetPath)) {
    throw 'Actual training-result subset manifest path changed'
}
foreach ($entry in $subset) {
    $rel = [string]$entry.relative
    if (-not $expected.ContainsKey($rel)) { throw "Unexpected image trained: $rel" }
    $pair = $expected[$rel]
    $expectedX = [IO.Path]::GetFullPath((Join-Path $validParent ('X\' + $rel)))
    $expectedY = [IO.Path]::GetFullPath((Join-Path $validParent ('Y\' + $rel)))
    if ([IO.Path]::GetFullPath($entry.x) -ne $expectedX -or
        [IO.Path]::GetFullPath($entry.y) -ne $expectedY -or
        -not $expectedX.StartsWith($validParent,[StringComparison]::OrdinalIgnoreCase) -or
        -not $expectedY.StartsWith($validParent,[StringComparison]::OrdinalIgnoreCase)) {
        throw "Unexpected training sample source path: $rel"
    }
    if ((Digest $entry.x) -ne $pair.x_sha256 -or
        (Digest $entry.y) -ne $pair.y_sha256) {
        throw "Training image or mask changed after execution: $rel"
    }
}
$vendorOriginalRoot = [IO.Path]::GetFullPath('D:\delta\Welding\DIAChip8Side\TrainingImage\R002-CCD1\Origional\1').TrimEnd('\') + '\'
foreach ($holdout in $manifest.holdout_originals) {
    $path = [IO.Path]::GetFullPath([string]$holdout.original)
    if (-not $path.StartsWith($vendorOriginalRoot,[StringComparison]::OrdinalIgnoreCase) -or
        (Digest $path) -ne $holdout.original_sha256) {
        throw "Reserved validation original changed: $($holdout.family)"
    }
}
if ($train.status -ne 'PASS' -or $train.epochs_completed -ne 1 -or
    @( $train.physical_gpus ).Count -lt 1 -or
    $train.seed_sha256 -ne '99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73' -or
    (Digest (Join-Path $job 'candidate.h5')) -ne $train.model_sha256) {
    throw 'Real GPU training result did not satisfy this pinned CCD1 smoke'
}
if ($conversion.status -ne 'PASS' -or $conversion.deployed -or
    $conversion.source_h5_sha256 -ne $train.model_sha256 -or
    (Digest (Join-Path $job 'candidate.h5d')) -ne $conversion.h5d_sha256 -or
    (Digest (Join-Path $job 'candidate.pbd')) -ne $conversion.pbd_sha256) {
    throw 'Candidate H5D/PBD conversion provenance failed'
}
if ($quality.gate -ne 'SMOKE_MATCH_NOT_QUALIFIED' -or
    $quality.candidate_job -ne $jobId -or $quality.inspected_count -ne 4 -or
    $quality.mismatched_reference_count -ne 0 -or
    $quality.model_deployed -or
    -not $quality.source_sha_verified) {
    throw 'Four-sample non-production A/B evidence does not match'
}
if (@($quality.samples).Count -ne 4) { throw 'Missing four holdout result rows' }
foreach ($item in $quality.samples) {
    $family = switch ($item.sample) {
        'S1OK' { 'OK-1-2025-06-27-16-54-46' }
        'S1NG' { 'NG-1-2025-06-27-16-55-12' }
        'S1OK2' { 'OK-1-2025-06-27-16-58-19' }
        'S1NG2' { 'NG-1-2025-06-27-16-55-24' }
        default { throw 'Unknown validation sample' }
    }
    $held = @($manifest.holdout_originals | Where-Object { $_.family -eq $family })
    $expectedBin = if ($family.StartsWith('OK-')) { 'Ok' } else { 'Ng2' }
    if ($held.Count -ne 1 -or $item.original_image_sha256 -ne $held[0].original_sha256 -or
        $item.vendor_bin -ne $expectedBin -or $item.candidate_bin -ne $expectedBin -or
        -not $item.candidate_matches_reference -or -not $item.enhance_output_changed) {
        throw "Held-out A/B truth, source or model image differs: $family"
    }
}
$result = [ordered]@{
    gate='SMOKE_PROVENANCE_PASS_NOT_QUALIFIED'
    job=$jobId
    data_pair_count=8
    classes='OK4_NG4'
    holdout_families=4
    original_full_pairs=53
    original_ng_augmentations=24
    train_holdout_leakage=$false
    paired_sha256_verified=$true
    real_gpu_train_verified=$true
    h5d_pbd_verified=$true
    ab_holdout4_match='4_OF_4'
    production_qualification=$false
    deployed=$false
}
$out = Join-Path $job 'ccd1_provenance_result.json'
$json = $result | ConvertTo-Json -Depth 5
if (Test-Path -LiteralPath $out) {
    if ([IO.File]::ReadAllText($out, [Text.Encoding]::UTF8) -ne $json) {
        throw 'Existing immutable CCD1 provenance witness differs'
    }
} else {
    [IO.File]::WriteAllText($out, $json, [Text.Encoding]::UTF8)
}
Write-Output 'DIA_CCD1_PROVENANCE=PASS TRAIN=OK4_NG4 HOLDOUT_FAMILIES=4'
Write-Output 'DIA_CCD1_QUALITY=SMOKE_MATCH_NOT_QUALIFIED AB=4/4 TRAIN_HOLDOUT_LEAKAGE=False DEPLOYED=False'
