$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$job = Join-Path $app 'TrainingCandidates\20261022T050341Z-33c441fa'
$expectedVendorSha = '99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73'
$evaluation = Join-Path $app 'TrainingEvaluations'
function LoadJson([string]$path) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing evaluation: $path" }
    return ([IO.File]::ReadAllText($path,[Text.Encoding]::UTF8)|ConvertFrom-Json)
}
$vendor = LoadJson (Join-Path $evaluation 'IoU_vendor_holdout4_threshold220.json')
$smoke = LoadJson (Join-Path $evaluation 'IoU_ccd1-smoke_holdout4_threshold220.json')
$long = LoadJson (Join-Path $evaluation 'IoU_ccd1-long300_holdout4_threshold220.json')
$training = LoadJson (Join-Path $evaluation 'IoU_ccd1-long300_training43_threshold220.json')
$seed = Join-Path $app 'TrainingSeed\DIA_R002_vendor.h5'
$model = Join-Path $job 'candidate.h5'
$outcome = LoadJson (Join-Path $job 'training_result.json')
$plan = LoadJson (Join-Path $app 'TrainingPlans\DIA_CCD1_S1_Train300.json')
$recorded = (Get-FileHash -LiteralPath $model).Hash
if ((Get-FileHash -LiteralPath $seed).Hash -ne $expectedVendorSha -or
    $vendor.model_sha256 -ne $expectedVendorSha -or
    $outcome.status -ne 'PASS' -or $outcome.epochs_completed -ne 300 -or
    @($outcome.physical_gpus).Count -lt 1 -or
    $outcome.model_sha256 -ne $recorded -or
    $long.model_sha256 -ne $recorded -or
    $training.model_sha256 -ne $recorded -or
    $plan.epoch_target -ne 300 -or $plan.training_pairs -ne 43 -or
    @($plan.heldout_families).Count -ne 4) {
    throw '300-epoch model, original seed or training contract evidence differs'
}
foreach ($metric in @($vendor,$smoke,$long,$training)) {
    if ($metric.gate -ne 'IOU_DIAGNOSTIC_ONLY_NOT_PRODUCTION_QUALIFIED' -or
        $metric.deployed -or
        $metric.preprocessing_verified_against_vendor_davs -or
        [Math]::Abs($metric.threshold - 220.0 / 255.0) -gt 1e-12 -or
        $metric.input_shape -ne '(None, 256, 256, 3)' -or
        $metric.output_shape -ne '(None, 256, 256, 3)') {
        throw 'IoU evaluator setup, threshold or qualification flag differs'
    }
}
if (-not $vendor.holdout_only -or -not $smoke.holdout_only -or
    -not $long.holdout_only -or $training.holdout_only -or
    $vendor.split -notin @($null,'','holdout') -or
    $long.split -notin @($null,'','holdout') -or
    $training.split -ne 'training' -or
    @($vendor.samples).Count -ne 4 -or
    @($smoke.samples).Count -ne 4 -or
    @($long.samples).Count -ne 4 -or
    @($training.samples).Count -ne 43) {
    throw 'Independent IoU holdout/training split labels differ'
}
for ($i=0; $i -lt 4; $i++) {
    foreach ($other in @($smoke,$long)) {
        if ($vendor.samples[$i].label -ne $other.samples[$i].label -or
            $vendor.samples[$i].filename -ne $other.samples[$i].filename -or
            $vendor.samples[$i].x_sha256 -ne $other.samples[$i].x_sha256 -or
            $vendor.samples[$i].y_sha256 -ne $other.samples[$i].y_sha256) {
            throw 'Different held-out ground-truth image was used between models'
        }
    }
}
$margin = $long.ng_mean_iou - $vendor.ng_mean_iou
$generalizationGap = $training.ng_mean_iou - $long.ng_mean_iou
$regression = $margin -lt -0.15 -and $generalizationGap -gt 0.20
$quality = if ($regression) { 'BLOCKED_OVERFIT_DIAGNOSTIC' } else { 'UNQUALIFIED_NEEDS_MORE_HOLDOUT' }
$report = [ordered]@{
    gate=$quality
    candidate_job='20261022T050341Z-33c441fa'
    epochs=300
    training_pairs=43
    independent_holdout_pairs=4
    independent_holdout_ng_pairs=2
    threshold='220/255'
    preprocessing_vendor_exact_match_unverified=$true
    vendor_holdout_ng_iou=$vendor.ng_mean_iou
    smoke1_holdout_ng_iou=$smoke.ng_mean_iou
    candidate300_holdout_ng_iou=$long.ng_mean_iou
    candidate300_train_ng_iou=$training.ng_mean_iou
    candidate300_generalization_gap=$generalizationGap
    candidate300_delta_vs_original=$margin
    model_sha256=$recorded
    iou_regressed=$regression
    production_qualified=$false
    deployed=$false
}
$dst = Join-Path $job 'iou_quality_gate.json'
$json = $report|ConvertTo-Json -Depth 5
if (Test-Path -LiteralPath $dst) {
    if ([IO.File]::ReadAllText($dst, [Text.Encoding]::UTF8) -ne $json) {
        throw 'Existing IoU quality gate may not be overwritten'
    }
} else { [IO.File]::WriteAllText($dst,$json,[Text.Encoding]::UTF8) }
Write-Output ('DIA_IOU_GATE='+$quality+' EPOCH=300 TRAIN_NG='+[math]::Round($training.ng_mean_iou,4)+
    ' HOLDOUT_NG='+[math]::Round($long.ng_mean_iou,4)+
    ' VENDOR_HOLDOUT_NG='+[math]::Round($vendor.ng_mean_iou,4)+' DEPLOYED=False')
