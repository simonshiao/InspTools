param(
    [string]$DonorRuntime = '',
    [switch]$IncludePython
)
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$AppBase = Join-Path $Root 'output\decoded-ui\Release'
if ([string]::IsNullOrWhiteSpace($DonorRuntime)) {
    # PS 5.1 can decode UTF-8-without-BOM scripts as ANSI; keep literals ASCII.
    $DonorRuntime = Join-Path (Split-Path -Parent $Root) 'ShuntVC_Chao\Runtime\Runtime'
}
$DonorRuntime = [IO.Path]::GetFullPath($DonorRuntime)
if (-not (Test-Path (Join-Path $AppBase 'InspToolsRecipeCli.exe'))) {
    throw 'Build.ps1 must finish before staging the optional Trainer'
}
$DonorTrainer = Join-Path $DonorRuntime 'TrainingRebuild'
$TargetTrainer = Join-Path $AppBase 'Runtime\TrainingRebuild'
$Pinned = @{
    'train_compat.py' = '4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E'
    'RecoveredDAVS\DAVS\DAVSTrain_v3.pyc' = '8645361D666E06A80E0BD52C1C91A63F3D5AFE90414297F001ADAAAE44F27522'
}
foreach ($name in $Pinned.Keys) {
    $file = Join-Path $DonorTrainer $name
    if (-not (Test-Path -LiteralPath $file) -or
        (Get-FileHash -LiteralPath $file).Hash -ne $Pinned[$name]) {
        throw "Training donor changed or missing: $name"
    }
}
New-Item -ItemType Directory -Path $TargetTrainer -Force | Out-Null
$sourceScript = Join-Path $DonorTrainer 'train_compat.py'
$targetScript = Join-Path $TargetTrainer 'train_compat.py'
if (Test-Path $targetScript) {
    if ((Get-FileHash $targetScript).Hash -ne $Pinned['train_compat.py']) {
        throw 'Staged Trainer script differs; refusing to overwrite'
    }
} else {
    Copy-Item -LiteralPath $sourceScript -Destination $targetScript
}
$package = Join-Path $TargetTrainer 'RecoveredDAVS'
if (-not (Test-Path $package)) {
    Copy-Item -LiteralPath (Join-Path $DonorTrainer 'RecoveredDAVS') -Destination $package -Recurse
}
if ((Get-FileHash (Join-Path $package 'DAVS\DAVSTrain_v3.pyc')).Hash -ne
    $Pinned['RecoveredDAVS\DAVS\DAVSTrain_v3.pyc']) {
    throw 'Recovered DAVS package SHA mismatch'
}
Write-Output 'DAVS_STAGE=PASS PYTHON=NOT_STAGED'
if ($IncludePython) {
    $targetPython = Join-Path $AppBase 'Runtime\TrainingPython'
    $donorPython = Join-Path $DonorRuntime 'TrainingPython'
    $donorCuda = Join-Path $DonorRuntime 'CUDA11_118'
    $targetCuda = Join-Path $AppBase 'Runtime\CUDA11_118'
    $driveName = [IO.Path]::GetPathRoot($AppBase).Substring(0,1)
    if ((Get-PSDrive $driveName).Free -lt 12GB) {
        throw 'At least 12GB free required for isolated GPU Python Runtime'
    }
    foreach ($source in @($donorPython, $donorCuda)) {
        if (-not (Test-Path -LiteralPath $source)) { throw "Missing runtime: $source" }
    }
    foreach ($name in @('python.exe', 'python38.dll')) {
        if (-not (Test-Path -LiteralPath (Join-Path $donorPython "Python38\$name"))) {
            throw "Missing portable Python component: $name"
        }
    }
    if (-not (Test-Path -LiteralPath (Join-Path $donorCuda 'cudnn64_8.dll'))) {
        throw 'Missing CUDA11 cuDNN runtime'
    }
    # Robocopy handles long paths and resumes a partial staging copy.
    New-Item -ItemType Directory -Path $targetCuda -Force | Out-Null
    New-Item -ItemType Directory -Path $targetPython -Force | Out-Null
    & robocopy $donorCuda $targetCuda /E /XJ /R:1 /W:1 /NFL /NDL /NP
    if ($LASTEXITCODE -ge 8) { throw "CUDA robocopy failed: $LASTEXITCODE" }
    & robocopy $donorPython $targetPython /E /XJ /XD (Join-Path $donorPython 'GpuRuntime') /R:1 /W:1 /NFL /NDL /NP
    if ($LASTEXITCODE -ge 8) { throw "Python robocopy failed: $LASTEXITCODE" }
    $gpuAlias = Join-Path $targetPython 'GpuRuntime'
    if (-not (Test-Path -LiteralPath $gpuAlias)) {
        New-Item -ItemType Junction -Path $gpuAlias -Target $targetCuda | Out-Null
    } elseif (-not ((Get-Item -LiteralPath $gpuAlias -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw 'GpuRuntime exists but is not a junction; refusing overwrite'
    }
    if ((Get-FileHash (Join-Path $targetCuda 'cudnn64_8.dll')).Hash -ne
        (Get-FileHash (Join-Path $donorCuda 'cudnn64_8.dll')).Hash) {
        throw 'CUDA runtime copy integrity check failed'
    }
    Write-Output 'PORTABLE_GPU_STAGE=PASS CUDA11=ISOLATED GPU_ALIAS=JUNCTION'
}
