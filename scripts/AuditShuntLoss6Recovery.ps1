$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$eval = Join-Path $app 'TrainingEvaluations'
$job = Join-Path $app 'TrainingCandidates\ShuntLoss6_CCD1_300E_20261022'
$holdout = Join-Path $app 'TrainingImage\DIA_CCD1_S1_Holdout4'
$sha = @{
    vendor='99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73'
    legacy='BD88B85FFD883DFC5A01DBDA4CA19A647D1EA3B8B07A7CF960CA05EFB694CBE4'
    modern='3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634'
    trainer='7B0E07187928F410922D93EF19D052D6FEB7E6C641A05D8A24574A2FEF104913'
}
function Json([string]$p) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        throw ('Required training evidence is missing: '+$p)
    }
    return ([IO.File]::ReadAllText($p,[Text.Encoding]::UTF8)|ConvertFrom-Json)
}
function CheckMetric([string]$name,[string]$file,[string]$expectedHash,[int]$count,[string]$data) {
    $m = Json (Join-Path $eval $file)
    if ($m.status -ne 'PASS' -or $m.pair_count -ne $count -or
        [Math]::Abs($m.threshold - 220.0/255.0) -gt 0.0000001 -or
        $m.min_area -ne 10 -or
        [IO.Path]::GetFullPath($m.data_root) -ne [IO.Path]::GetFullPath($data)) {
        throw ('Shunt original quality metric uses different dataset or parameters: '+$name)
    }
    if ((Get-FileHash -LiteralPath $m.model_path).Hash -ne $expectedHash) {
        throw ('Model SHA-256 changed since quality evaluation: '+$name)
    }
    $r=@($m.channels|Where-Object {$_.channel -eq 'R'})
    if ($r.Count -ne 1) { throw ('R channel missing from quality report: '+$name) }
    return [pscustomobject]@{
        name=$name; report=$m; red=$r[0]
    }
}

$original=CheckMetric 'vendor' 'ShuntOriginalQuality_vendor_holdout4.json' $sha.vendor 4 $holdout
$old=CheckMetric 'legacy' 'ShuntOriginalQuality_ccd1-long300_holdout4.json' $sha.legacy 4 $holdout
$new=CheckMetric 'modern' 'ShuntFixedLoss6_300E_holdout4_220.json' $sha.modern 4 $holdout
$train=CheckMetric 'train300' 'ShuntFixedLoss6_300E_training43_220.json' $sha.modern 43 (Join-Path $app 'TrainingImage\DIA_CCD1_S1_Train300')
$result=Json (Join-Path $job 'training_result.json')
$input=Json (Join-Path $job 'input_provenance.json')
$contract=Json (Join-Path $job 'dataset_contract.json')
$files=Json (Join-Path $job 'dataset_manifest.json')
$state=[IO.File]::ReadAllText((Join-Path $job 'state.txt'),[Text.Encoding]::UTF8)
$trainer=Join-Path $app 'Runtime\AgentVisionLab\trainers\temp_unet_trainer.py'
if ((Get-FileHash -LiteralPath $trainer).Hash -ne $sha.trainer -or
    $input.backend_sha256 -ne $sha.trainer -or
    $input.original_seed_sha256 -ne $sha.vendor -or
    $input.loss -ne 6 -or $input.epoch_count -ne 300 -or
    $result.status -ne 'PASS' -or $result.epochs_completed -ne 300 -or
    $result.pair_count -ne 43 -or $result.batch_size -ne 8 -or
    $result.best_epoch -lt 1 -or $result.best_epoch -gt 300 -or
    $result.loss_function -notlike '*LossFunction=6*' -or
    $result.production_write_performed -or $result.plain_h5_sha256 -ne $sha.modern -or
    -not $state.StartsWith('STATE=SUCCEEDED') -or
    $contract.pair_count -ne 43 -or
    $contract.active_images.R -ne 24 -or $contract.active_images.G -ne 0 -or
    $contract.active_images.B -ne 0 -or
    @($files).Count -ne 43) {
    throw 'Pinned Shunt 300 epoch Loss6 training source, labels or result differs'
}
$delta = [double]$new.red.iou - [double]$old.red.iou
$vendorGap = [double]$original.red.iou - [double]$new.red.iou
$improved = $delta -gt 0.3
$belowVendor = $vendorGap -gt 0.05
$gate = if ($improved -and $belowVendor) {
    'IMPROVED_LOSS6_STILL_NOT_QUALIFIED'
} elseif ($improved) {
    'PROMISING_SMOKE_NOT_QUALIFIED'
} else {
    'BLOCKED_NO_SIGNIFICANT_IMPROVEMENT'
}
$report = [ordered]@{
    gate=$gate
    framework='ShuntVC product-owned 2026-09-30 AgentVisionLab'
    train_backend_sha256=$sha.trainer
    epochs=300
    batch=8
    loss_function=6
    seed_continued_from_vendor=$true
    training_pairs=43
    train_original_families=25
    independent_holdout_original_families=4
    holdout_ng_count=2
    channel='R'
    threshold='220/255'
    metric='ShuntVC original quality_gate.py channel IoU'
    original_vendor_r_iou=$original.red.iou
    legacy_mse300_holdout_r_iou=$old.red.iou
    shunt_loss6_300_holdout_r_iou=$new.red.iou
    shunt_loss6_300_training_r_iou=$train.red.iou
    loss6_improvement=$delta
    still_below_vendor=$belowVendor
    loss6_holdout_pixel_recall=$new.red.recall
    loss6_holdout_pixel_precision=$new.red.precision
    loss6_holdout_false_positive_ok_images=$new.report.image_decision.fp_ng
    loss6_holdout_false_negative_ng_images=$new.report.image_decision.fn_ng
    best_epoch_by_training_loss=$result.best_epoch
    best_epoch_by_validation_iou=$null
    independent_sample_size_sufficient=$false
    production_qualified=$false
    deployed=$false
}
$target=Join-Path $job 'quality_recovery_result.json'
$encoded=$report|ConvertTo-Json -Depth 6
if (Test-Path -LiteralPath $target) {
    if ([IO.File]::ReadAllText($target,[Text.Encoding]::UTF8) -ne $encoded) {
        throw 'Existing protected Loss6 recovery result differs'
    }
} else {
    [IO.File]::WriteAllText($target,$encoded,[Text.Encoding]::UTF8)
}
Write-Output ('SHUNT_RECOVERY_GATE='+$gate+
    ' VENDOR_R_IOU='+[math]::Round($original.red.iou,4)+
    ' LEGACY_R_IOU='+[math]::Round($old.red.iou,4)+
    ' LOSS6_R_IOU='+[math]::Round($new.red.iou,4)+
    ' LOSS6_TRAIN_IOU='+[math]::Round($train.red.iou,4)+
    ' BEST_EPOCH='+$result.best_epoch+' DEPLOYED=False')
