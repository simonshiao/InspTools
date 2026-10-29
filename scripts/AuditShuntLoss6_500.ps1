$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app=Join-Path $root 'output\decoded-ui\Release'
$eval=Join-Path $app 'TrainingEvaluations'
$jobs=Join-Path $app 'TrainingCandidates'
$job300=Join-Path $jobs 'ShuntLoss6_CCD1_300E_20261022'
$job500=Join-Path $jobs 'ShuntLoss6_CCD1_500E_20261022_R1'
function Json([string]$file) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw ('Missing 300/500 experiment evidence: '+$file)
    }
    return ([IO.File]::ReadAllText($file,[Text.Encoding]::UTF8)|
        ConvertFrom-Json)
}
$result300=Json (Join-Path $job300 'training_result.json')
$result500=Json (Join-Path $job500 'training_result.json')
$input500=Json (Join-Path $job500 'input_provenance.json')
$original=Json (Join-Path $eval 'ShuntOriginalQuality_vendor_holdout4.json')
$quality300=Json (Join-Path $eval 'ShuntFixedLoss6_300E_holdout4_220.json')
$quality500=Json (Join-Path $eval 'ShuntFixedLoss6_500E_holdout4_220.json')
$training300=Json (Join-Path $eval 'ShuntFixedLoss6_300E_training43_220.json')
$training500=Json (Join-Path $eval 'ShuntFixedLoss6_500E_training43_220.json')
$history=Json (Join-Path $job500 'resume_progress_300.json')
$hash300='3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634'
$hash500='84D97A39705ADFE59DE14DB5715F293F4897B64E5CF32769644CBBB76D850410'
if ($result300.status -ne 'PASS' -or
    $result300.epochs_completed -ne 300 -or
    $result300.plain_h5_sha256 -ne $hash300 -or
    $result500.status -ne 'PASS' -or
    $result500.epochs_requested -ne 500 -or
    $result500.epochs_completed -ne 500 -or
    $result500.epochs_trained_this_run -ne 200 -or
    $result500.initial_epoch -ne 300 -or
    $result500.pair_count -ne 43 -or
    $result500.batch_size -ne 8 -or
    $result500.loss_function -notlike '*LossFunction=6*' -or
    $result500.production_write_performed -or
    $result500.plain_h5_sha256 -ne $hash500 -or
    $result500.best_epoch -lt 301 -or
    $result500.best_epoch -gt 500 -or
    $result500.best_loss -gt 1e-5 -or
    @($result500.loss_history).Count -ne 500 -or
    $history.epoch -ne 300 -or
    @($history.loss_history).Count -ne 300 -or
    $input500.resumed_model_sha256 -ne $hash300 -or
    $input500.initial_epoch -ne 300 -or
    $input500.epochs_trained_this_run -ne 200) {
    throw '500 epoch Loss6 resume, target, or full history contract failed'
}
if ((Get-FileHash -LiteralPath (Join-Path $job300 'temp_unet_US_plain.h5')).Hash -ne $hash300 -or
    (Get-FileHash -LiteralPath (Join-Path $job500 'temp_unet_US_plain.h5')).Hash -ne $hash500) {
    throw 'Candidate model SHA mismatch after 500 epoch continuation'
}
foreach ($check in @($original,$quality300,$quality500,$training300,$training500)) {
    if ($check.status -ne 'PASS' -or
        [Math]::Abs($check.threshold - 220.0/255.0) -gt 1e-10 -or
        $check.min_area -ne 10 -or
        @($check.channels).Count -ne 3) {
        throw 'Shunt original quality evaluator configuration differs'
    }
}
$holdout=Join-Path $app 'TrainingImage\DIA_CCD1_S1_Holdout4'
$training=Join-Path $app 'TrainingImage\DIA_CCD1_S1_Train300'
foreach ($check in @($original,$quality300,$quality500)) {
    if ($check.pair_count -ne 4 -or
        [IO.Path]::GetFullPath($check.data_root) -ne [IO.Path]::GetFullPath($holdout)) {
        throw 'Validation cohort changed between vendor/300/500'
    }
}
foreach ($check in @($training300,$training500)) {
    if ($check.pair_count -ne 43 -or
        [IO.Path]::GetFullPath($check.data_root) -ne [IO.Path]::GetFullPath($training)) {
        throw 'Training cohort changed between 300/500'
    }
}
$r0=@($original.channels|Where-Object {$_.channel -eq 'R'})[0]
$r3=@($quality300.channels|Where-Object {$_.channel -eq 'R'})[0]
$r5=@($quality500.channels|Where-Object {$_.channel -eq 'R'})[0]
$t3=@($training300.channels|Where-Object {$_.channel -eq 'R'})[0]
$t5=@($training500.channels|Where-Object {$_.channel -eq 'R'})[0]
$regression=[double]$r5.iou -lt ([double]$r3.iou - 0.05)
$status=if($regression){'HOLDOUT_IOU_REGRESSED_KEEP_300'}else{'HOLDOUT_IMPROVED_STILL_UNQUALIFIED'}
$record=[ordered]@{
    gate=$status
    threshold='220/255'
    metric='ShuntVC 2026-09-30 original quality_gate.py / R channel'
    source_model_sha256=$hash300
    continued_model_sha256=$hash500
    total_epochs=500
    resumed_from_epoch=300
    trained_epochs_in_this_job=200
    best_epoch_by_training_loss=$result500.best_epoch
    train_loss_target=1e-5
    target_met=$true
    train_best_loss_300=$result300.best_loss
    train_best_loss_500=$result500.best_loss
    vendor_holdout_r_iou=$r0.iou
    model300_holdout_r_iou=$r3.iou
    model500_holdout_r_iou=$r5.iou
    model300_train_r_iou=$t3.iou
    model500_train_r_iou=$t5.iou
    model300_holdout_r_recall=$r3.recall
    model500_holdout_r_recall=$r5.recall
    holdout_iou_delta_500_vs_300=([double]$r5.iou-[double]$r3.iou)
    holdout_ng_families=2
    holdout_ok_families=2
    quality_qualified=$false
    selected_candidate_epoch=300
    deployed=$false
}
$path=Join-Path $job500 'iou500_quality_gate.json'
$json=$record|ConvertTo-Json -Depth 5
if (Test-Path -LiteralPath $path) {
    if ([IO.File]::ReadAllText($path,[Text.Encoding]::UTF8) -ne $json) {
        throw 'Existing immutable 500-epoch quality evidence differs'
    }
} else {
    [IO.File]::WriteAllText($path,$json,[Text.Encoding]::UTF8)
}
Write-Output ('CCD1_500_GATE='+$status+
    ' LOSS500='+$result500.best_loss+
    ' RIOU300='+[Math]::Round($r3.iou,4)+
    ' RIOU500='+[Math]::Round($r5.iou,4)+
    ' VENDOR='+[Math]::Round($r0.iou,4)+
    ' TARGET_1E5=PASS DEPLOYED=False')
