param(
    [ValidateSet(1,300,500)][int]$Epochs = 1,
    [ValidateSet('','R1')][string]$RetryTag = ''
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$data = Join-Path $app 'TrainingImage\DIA_CCD1_S1_Train300'
$seed = Join-Path $app 'TrainingSeed\DIA_R002_vendor.h5'
$initialEpoch = if ($Epochs -eq 500) { 300 } else { 0 }
$previousJob = Join-Path $app 'TrainingCandidates\ShuntLoss6_CCD1_300E_20261022'
$previousResultPath = Join-Path $previousJob 'training_result.json'
$previousProgressPath = Join-Path $previousJob 'progress.json'
$expectedPreviousModel = '3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634'
$previousSeed = Join-Path $previousJob 'temp_unet_US_plain.h5'
if ($Epochs -eq 500) {
    if (-not (Test-Path -LiteralPath $previousResultPath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $previousProgressPath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $previousSeed -PathType Leaf) -or
        (Get-FileHash -LiteralPath $previousSeed).Hash -ne $expectedPreviousModel) {
        throw 'Verified 300-epoch Loss6 candidate and progress required for continuation'
    }
    $previous = [IO.File]::ReadAllText($previousResultPath,[Text.Encoding]::UTF8) |
        ConvertFrom-Json
    $previousProgress = [IO.File]::ReadAllText($previousProgressPath,[Text.Encoding]::UTF8) |
        ConvertFrom-Json
    if ($previous.status -ne 'PASS' -or
        $previous.epochs_completed -ne 300 -or
        $previous.best_epoch -ne 298 -or
        $previous.initial_epoch -ne 0 -or
        $previous.batch_size -ne 8 -or
        $previous.learning_rate -ne 0.001 -or
        $previous.loss_function -notlike '*LossFunction=6*' -or
        $previous.plain_h5_sha256 -ne $expectedPreviousModel -or
        $previousProgress.epoch -ne 300 -or
        @($previousProgress.loss_history).Count -ne 300) {
        throw '300-epoch source provenance/history differs; refusing invalid continuation'
    }
    $seed = $previousSeed
}
$trainer = Join-Path $app 'Runtime\AgentVisionLab\trainers\temp_unet_trainer.py'
$python = Join-Path $app 'Runtime\TrainingPython\Python38\python.exe'
$manifest = Join-Path $app 'TrainingPlans\DIA_CCD1_S1_Train300.json'
$job = Join-Path $app ('TrainingCandidates\ShuntLoss6_CCD1_'+$Epochs+'E_20261022')
if ($RetryTag) {
    if ($Epochs -ne 500) { throw 'RetryTag is supported only for 500-epoch continuation' }
    $job = $job + '_' + $RetryTag
}
$timeoutSeconds = if ($Epochs -eq 1) { 180 } else { 1800 }
$expectedSeed = '99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73'
$expectedTrainer = '7B0E07187928F410922D93EF19D052D6FEB7E6C641A05D8A24574A2FEF104913'
$fileHashes = @(
    @{file=$seed; sha=$(if ($Epochs -eq 500) {$expectedPreviousModel} else {$expectedSeed})},
    @{file=$trainer; sha=$expectedTrainer}
)
foreach ($entry in $fileHashes) {
    if (-not (Test-Path -LiteralPath $entry.file -PathType Leaf) -or
        (Get-FileHash -LiteralPath $entry.file).Hash -ne $entry.sha) {
        throw ('Pinned CCD1 seed or Shunt 2026-09-30 trainer changed: '+$entry.file)
    }
}
if (-not (Test-Path -LiteralPath $python -PathType Leaf) -or
    -not (Test-Path -LiteralPath $manifest -PathType Leaf)) {
    throw 'Missing product-local Python or CCD1 training manifest'
}
$plan = [IO.File]::ReadAllText($manifest,[Text.Encoding]::UTF8) | ConvertFrom-Json
if ($plan.dataset -ne 'DIA_CCD1_S1_Train300' -or
    $plan.training_pairs -ne 43 -or $plan.training_ok_pairs -ne 19 -or
    $plan.training_ng_pairs -ne 24 -or $plan.unique_training_families -ne 25 -or
    $plan.epoch_target -ne 300 -or $plan.deployed -or
    @($plan.heldout_families).Count -ne 4 -or @($plan.entries).Count -ne 43) {
    throw 'Training manifest no longer matches CCD1 family-isolated X/Y dataset'
}
$holdout = @($plan.heldout_families)
foreach ($item in $plan.entries) {
    if ($holdout -contains $item.family) {
        throw ('Training sample leaked reserved holdout family: '+$item.family)
    }
    if ($item.label -notin @('OK','NG') -or
        $item.relative -match '[\/]\.\.|[\/:*?"<>|]' -or
        $item.relative.StartsWith('.')) {
        throw 'Unexpected CCD1 training source relative path'
    }
    foreach ($axis in @('X','Y')) {
        $file = Join-Path $data ($axis+'\'+$item.label+'\'+$item.relative)
        $sha = if ($axis -eq 'X') {$item.x_sha256} else {$item.y_sha256}
        if (-not (Test-Path -LiteralPath $file -PathType Leaf) -or
            (Get-FileHash -LiteralPath $file).Hash -ne $sha) {
            throw ('Training input modified: '+$file)
        }
    }
}
if (Test-Path -LiteralPath $job) {
    throw ('Job exists, refusing to overwrite or restart: '+$job)
}
[IO.Directory]::CreateDirectory($job)|Out-Null
$localProgress = $null
if ($Epochs -eq 500) {
    $localProgress = Join-Path $job 'resume_progress_300.json'
    Copy-Item -LiteralPath $previousProgressPath -Destination $localProgress
    if ((Get-FileHash -LiteralPath $localProgress).Hash -ne
        (Get-FileHash -LiteralPath $previousProgressPath).Hash) {
        throw '300-epoch resume progress local staging hash mismatch'
    }
}
[IO.File]::WriteAllText((Join-Path $job 'input_provenance.json'),(
    [ordered]@{
        backend='SHUNT_20260930_AGENTVISIONLAB_LOSS6'
        backend_sha256=$expectedTrainer
        original_seed_sha256=$expectedSeed
        dataset_manifest_sha256=(Get-FileHash $manifest).Hash
        epoch_count=$Epochs
        initial_epoch=$initialEpoch
        epochs_trained_this_run=($Epochs - $initialEpoch)
        resumed_from_300_job=$(if ($Epochs -eq 500) {$previousJob} else {$null})
        resumed_model_sha256=$(if ($Epochs -eq 500) {$expectedPreviousModel} else {$null})
        target_loss=0.00001
        batch=8
        lr=0.001
        loss=6
        source_pairs=43
        reserved_families=$holdout
        allowed_write_root=$job
        deployed=$false
    } | ConvertTo-Json -Depth 5),[Text.Encoding]::UTF8)

$args = @(
    '"'+$trainer+'"', '--epochs', $Epochs, '--initial-epoch', $initialEpoch,
    '--batch', '8',
    '--learning-rate','0.001','--loss-function','6',
    '--data-root','"'+$data+'"', '--output-dir','"'+$job+'"',
    '--resume-weights','"'+$seed+'"', '--variant','US',
    '--unet-div','4','--width','256','--height','256',
    '--cache-dataset'
)
if ($Epochs -eq 500) {
    # Avoid Windows PowerShell string concatenation within an array literal
    # producing a path with leading/trailing whitespace inside quotes.
    $args += '--resume-progress'
    $args += 'resume_progress_300.json'
}
$start = New-Object System.Diagnostics.ProcessStartInfo
$start.FileName = $python
$start.Arguments = ($args -join ' ')
$start.WorkingDirectory = $job
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$cuda = Join-Path $app 'Runtime\CUDA11_118'
$gpu = Join-Path $app 'Runtime\TrainingPython\GpuRuntime'
$pythonRoot = Join-Path $app 'Runtime\TrainingPython\Python38'
if (-not (Test-Path (Join-Path $cuda 'cudnn64_8.dll')) -or
    -not (Test-Path -LiteralPath $gpu)) {
    throw 'Self-owned CUDA11/cuDNN GPU backend missing'
}
$start.EnvironmentVariables['PATH'] = ($gpu+';'+$cuda+';'+$pythonRoot+';'+
    (Join-Path $pythonRoot 'Scripts')+';'+$start.EnvironmentVariables['PATH'])
$start.EnvironmentVariables['CUDA_VISIBLE_DEVICES'] = '0'
$start.EnvironmentVariables['TF_FORCE_GPU_ALLOW_GROWTH'] = 'true'
$start.EnvironmentVariables['PYTHONNOUSERSITE'] = '1'
$start.EnvironmentVariables['PYTHONDONTWRITEBYTECODE'] = '1'
$start.EnvironmentVariables['PYTHONUTF8'] = '1'
$start.EnvironmentVariables['PYTHONIOENCODING'] = 'utf-8'
$start.EnvironmentVariables['TF_CPP_MIN_LOG_LEVEL'] = '2'
$process = New-Object System.Diagnostics.Process
$process.StartInfo = $start
$stop = Join-Path $job 'stop.request'
[IO.File]::WriteAllText((Join-Path $job 'state.txt'),'STATE=RUNNING')
Write-Output ('SHUNT_LOSS6_JOB='+$job+' EPOCHS='+$Epochs+
    ' INITIAL_EPOCH='+$initialEpoch+' SEED_RESUME=PINNED DEPLOYED=False')
$timer = [Diagnostics.Stopwatch]::StartNew()
$state = 'FAILED'
try {
    if (-not $process.Start()) { throw 'Pinned Shunt AgentVisionLab trainer did not start' }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    while (-not $process.WaitForExit(500)) {
        if (Test-Path -LiteralPath $stop) {
            $state = 'STOPPED'
            break
        }
        if ($timer.Elapsed.TotalSeconds -ge $timeoutSeconds) {
            $state = 'TIMED_OUT'
            break
        }
    }
    if (-not $process.HasExited) { $process.Kill() }
    $process.WaitForExit()
    $process.Refresh()
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    [IO.File]::WriteAllText((Join-Path $job 'stdout.log'),$stdout,[Text.Encoding]::UTF8)
    [IO.File]::WriteAllText((Join-Path $job 'stderr.log'),$stderr,[Text.Encoding]::UTF8)
    $resultFile = Join-Path $job 'training_result.json'
    if ($state -ne 'STOPPED' -and $state -ne 'TIMED_OUT' -and
        $process.ExitCode -eq 0 -and (Test-Path -LiteralPath $resultFile)) {
        $result = [IO.File]::ReadAllText($resultFile,[Text.Encoding]::UTF8) |
            ConvertFrom-Json
        $model = Join-Path $job 'temp_unet_US_plain.h5'
        $bestWeights = Join-Path $job 'best.weights.h5'
        $contract = Join-Path $job 'dataset_contract.json'
        $resultDataset = Join-Path $job 'dataset_manifest.json'
        if ($result.status -ne 'PASS' -or
            $result.epochs_completed -ne $Epochs -or
            $result.initial_epoch -ne $initialEpoch -or
            $result.epochs_trained_this_run -ne ($Epochs - $initialEpoch) -or
            $result.pair_count -ne 43 -or
            $result.unet_div -ne 4 -or
            $result.batch_size -ne 8 -or
            $result.loss_function -notlike '*LossFunction=6*' -or
            $result.production_write_performed -or
            -not (Test-Path -LiteralPath $bestWeights) -or
            -not (Test-Path -LiteralPath $contract) -or
            -not (Test-Path -LiteralPath $resultDataset) -or
            @( $result.gpu ).Count -lt 1 -or
            @($result.loss_history).Count -ne $Epochs -or
            @( $result.label_contract.active_channels ) -notcontains 'R' -or
            (Get-FileHash -LiteralPath $model).Hash -ne $result.plain_h5_sha256 -or
            ($Epochs -ne 500 -and -not $stdout.Contains('TEMP_UNET_RESUME_UNWRAP')) -or
            ($Epochs -eq 500 -and $result.plain_h5_sha256 -eq $expectedPreviousModel)) {
            throw 'Shunt trainer produced unexpected model/seed/dataset result'
        }
        $state = 'SUCCEEDED'
        Write-Output ('SHUNT_LOSS6_RESULT=PASS EPOCHS='+$Epochs+
            ' INPUT_PAIRS=43 LOSS='+$result.best_loss+
            ' TARGET_1E5_MET='+($result.best_loss -le 0.00001)+
            ' BEST_EPOCH='+$result.best_epoch+
            ' MODEL_SHA='+$result.plain_h5_sha256+
            ' DEPLOYED=False')
    } elseif ($state -ne 'STOPPED' -and $state -ne 'TIMED_OUT') {
        Write-Output ('SHUNT_LOSS6_RESULT=FAILED EXIT='+$process.ExitCode)
    }
} finally {
    if (-not $process.HasExited) {
        $process.Kill()
        $process.WaitForExit()
    }
    $process.Dispose()
    [IO.File]::WriteAllText((Join-Path $job 'state.txt'),('STATE='+$state+
        [Environment]::NewLine+'ELAPSED_SECONDS='+[math]::Round($timer.Elapsed.TotalSeconds,2)))
}
if ($state -ne 'SUCCEEDED') { throw ('Shunt AgentVisionLab run not accepted: '+$state) }
