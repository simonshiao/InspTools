param(
    [switch]$Train,
    [string]$VerifyJob = '',
    [string]$JobName = '',
    [ValidateRange(1,1000)][int]$Epochs = 300,
    [ValidateRange(1,1000)][int]$CheckpointEvery = 20,
    [ValidateRange(1,128)][int]$Batch = 8,
    [ValidateRange(1,60000)][int]$MaxSeconds = 1800,
    [string]$ResumeWeights = '',
    [string]$ExpectedResumeSha = '99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73'
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$script = Join-Path $root 'Tools\ccd1_iou_runner_v1.py'
$python = Join-Path $app 'Runtime\TrainingPython\Python38\python.exe'
$gpu = Join-Path $app 'Runtime\TrainingPython\GpuRuntime'
$cuda = Join-Path $app 'Runtime\CUDA11_118'
$pythonRoot = Join-Path $app 'Runtime\TrainingPython\Python38'
if (-not (Test-Path -LiteralPath $python -PathType Leaf) -or
    -not (Test-Path -LiteralPath $script -PathType Leaf)) {
    throw 'Local app-owned Python or IoU trainer missing'
}
$env:PYTHONNOUSERSITE = '1'
$env:PYTHONDONTWRITEBYTECODE = '1'
$env:PYTHONUTF8 = '1'
$env:PYTHONIOENCODING = 'utf-8'
$env:TF_FORCE_GPU_ALLOW_GROWTH = 'true'
$env:CUDA_VISIBLE_DEVICES = '0'
$env:TF_CPP_MIN_LOG_LEVEL = '2'
$env:PATH = $gpu + ';' + $cuda + ';' + $pythonRoot + ';' +
    (Join-Path $pythonRoot 'Scripts') + ';' + $env:PATH
if ($VerifyJob) {
    if ($Train -or $VerifyJob -notmatch '^[A-Za-z0-9_-]{8,90}$') {
        throw '-VerifyJob accepts exactly one existing candidate name and no -Train'
    }
    $candidate = Join-Path $app ('TrainingCandidates\' + $VerifyJob)
    $resultFile = Join-Path $candidate 'training_result.json'
    $selected = Join-Path $candidate 'selected_by_holdout_iou.h5'
    $quality = Join-Path $app 'Runtime\AgentVisionLab\quality_gate.py'
    $validation = Join-Path $app 'TrainingImage\DIA_CCD1_S1_Holdout4'
    $independent = Join-Path $candidate 'shunt_quality_parity.json'
    if (-not (Test-Path -LiteralPath $resultFile -PathType Leaf) -or
        -not (Test-Path -LiteralPath $selected -PathType Leaf) -or
        -not (Test-Path -LiteralPath $quality -PathType Leaf)) {
        throw 'Missing complete research output or Shunt quality evaluator'
    }
    $result = [IO.File]::ReadAllText($resultFile,[Text.Encoding]::UTF8) |
        ConvertFrom-Json
    if ($result.status -ne 'PASS_RESEARCH_ONLY' -or
        $result.production_qualified -or $result.deployed -or
        (Get-FileHash -LiteralPath $selected).Hash -ne $result.selected_h5_sha256) {
        throw 'Research output SHA or deployment Gate mismatch'
    }
    $checkArgs = @(
        $quality, '--model', $selected,
        '--data-root', $validation,
        '--batch', '4', '--threshold', '0.8627450980392157',
        '--min-area', '10', '--output', $independent
    )
    & $python -B @checkArgs | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Independent Shunt quality evaluator failed' }
    $report = [IO.File]::ReadAllText($independent,[Text.Encoding]::UTF8) |
        ConvertFrom-Json
    $expected = $result.selected_holdout_metrics
    if ($report.status -ne 'PASS' -or $report.pair_count -ne 4 -or
        [math]::Abs($report.threshold - 220.0/255.0) -gt 1e-10 -or
        $report.min_area -ne 10) {
        throw 'Independent quality contract differs from selected checkpoint'
    }
    foreach ($ch in @('R','G','B')) {
        $a = @($report.channels | Where-Object {$_.channel -eq $ch})[0]
        $b = @($expected.channels | Where-Object {$_.channel -eq $ch})[0]
        if ([math]::Abs($a.iou - $b.iou) -gt 0.000001 -or
            [math]::Abs($a.precision - $b.precision) -gt 0.000001 -or
            [math]::Abs($a.recall - $b.recall) -gt 0.000001 -or
            $a.tp -ne $b.tp -or $a.fp -ne $b.fp -or $a.fn -ne $b.fn) {
            throw ('IoU checkpoint metrics do not match original Shunt evaluator: '+$ch)
        }
    }
    foreach ($key in @('tp_ng','fp_ng','fn_ng','tn_ok')) {
        if ($report.image_decision.$key -ne $expected.image_decision.$key) {
            throw ('Image-level Shunt evaluation mismatch: ' + $key)
        }
    }
    $red = @($report.channels | Where-Object {$_.channel -eq 'R'})[0]
    Write-Output ('CCD1_IOU_QUALITY_PARITY=PASS JOB='+$VerifyJob+
        ' R_IOU='+[math]::Round($red.iou,6)+
        ' QUALIFIED=False DEPLOYED=False')
    exit 0
}
if (-not $Train) {
    # Default read-only family/hash audit. No model or job is created.
    & $python -B $script --app $app --audit-only
    if ($LASTEXITCODE -ne 0) { throw 'CCD1 IoU data audit FAILED' }
    Write-Output 'CCD1_IOU_V1_AUDIT=PASS TRAINING=NOT_STARTED DEPLOYED=False'
    exit 0
}
if ($JobName -notmatch '^[A-Za-z0-9_-]{8,90}$') {
    throw '-Train requires a new, explicit ASCII -JobName (8..90 chars)'
}
if ($CheckpointEvery -gt $Epochs) {
    throw 'CheckpointEvery cannot exceed Epochs'
}
$candidate = Join-Path $app ('TrainingCandidates\' + $JobName)
if (Test-Path -LiteralPath $candidate) {
    throw ('Refusing overwrite of existing candidate: ' + $candidate)
}
if (-not $ResumeWeights) {
    $ResumeWeights = Join-Path $app 'TrainingSeed\DIA_R002_vendor.h5'
}
if ((Get-FileHash -LiteralPath $ResumeWeights).Hash -ne $ExpectedResumeSha) {
    throw 'Resume checkpoint SHA does not match requested seed'
}
if (-not (Test-Path -LiteralPath (Join-Path $cuda 'cudnn64_8.dll') -PathType Leaf) -or
    -not (Test-Path -LiteralPath $gpu -PathType Container)) {
    throw 'GPU Runtime missing; refusing to start training'
}
Write-Output ('CCD1_IOU_V1_START JOB=' + $JobName +
    ' EPOCHS=' + $Epochs + ' EVERY=' + $CheckpointEvery +
    ' RESEARCH_ONLY=True DEPLOYED=False')
$cliArgs = @(
    $script, '--app', $app,
    '--job-name', $JobName,
    '--resume-weights', $ResumeWeights,
    '--expected-resume-sha', $ExpectedResumeSha,
    '--epochs', [string]$Epochs,
    '--checkpoint-every', [string]$CheckpointEvery,
    '--batch', [string]$Batch,
    '--max-seconds', [string]$MaxSeconds
)
& $python -B @cliArgs
if ($LASTEXITCODE -ne 0) {
    throw ('CCD1 IoU research training FAILED; inspect candidate state: ' + $candidate)
}
Write-Output ('CCD1_IOU_V1_RESULT=PASS_RESEARCH_ONLY JOB=' + $candidate +
    ' PRODUCTION_QUALIFIED=False DEPLOYED=False')
