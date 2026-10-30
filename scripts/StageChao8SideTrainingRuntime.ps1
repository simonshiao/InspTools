param(
    [string]$Candidate = '',
    [string]$DonorRuntime = ''
)
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$OutputRoot = Join-Path $Root 'output'
if ([string]::IsNullOrWhiteSpace($Candidate)) {
    $Candidate = Join-Path $OutputRoot 'InspToolsV11'
}
if ([string]::IsNullOrWhiteSpace($DonorRuntime)) {
    $DonorRuntime = 'E:\0_軟體開發區\1.成品區\Chao8Side\Runtime'
}
$Candidate = [IO.Path]::GetFullPath($Candidate)
$DonorRuntime = [IO.Path]::GetFullPath($DonorRuntime)
if (-not $Candidate.StartsWith($OutputRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Candidate must stay under this project output directory.'
}
if (-not (Test-Path (Join-Path $Candidate 'portable-review.marker')) -or
    (Get-Content (Join-Path $Candidate 'portable-review.marker') -Raw).Trim() -ne
        'INSPTOOLS_PORTABLE_REVIEW_R3_V1') {
    throw 'Candidate is not an explicitly marked portable review build.'
}
if (-not (Test-Path (Join-Path $Candidate 'chao8side-ai-training-ui.marker'))) {
    throw 'Training bridge marker is missing.'
}
$runtimeRoot = Join-Path $Candidate 'Runtime'
if (Test-Path -LiteralPath $runtimeRoot) { throw 'Candidate Runtime already exists; refusing overwrite.' }
foreach ($relative in @(
    'TrainingPython\Python38\python.exe',
    'TrainingRebuild\train_compat.py',
    'ModelConverter\PyFrozenLib\DAVS\DAVSTrain_v3.pyc',
    'CUDA11_118\cudnn64_8.dll',
    'AIRuntime\ChaoAIRuntime.exe',
    'ModelConverter\PyFrozenLib\DAVS\DAVSTrain_v3.pyc',
    'ModelConverter\TensorRT861\nvinfer.dll'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $DonorRuntime $relative))) {
        throw "Runtime donor is incomplete: $relative"
    }
}
$pinned = @{
    'TrainingRebuild\train_compat.py' = 'F003B62FC2084721B69227550419BBE017EEF0DC92CF7E3ABC92FE9CF081A62D'
    'ModelConverter\PyFrozenLib\DAVS\DAVSTrain_v3.pyc' = '8645361D666E06A80E0BD52C1C91A63F3D5AFE90414297F001ADAAAE44F27522'
}
foreach ($relative in $pinned.Keys) {
    if ((Get-FileHash -LiteralPath (Join-Path $DonorRuntime $relative) -Algorithm SHA256).Hash -ne $pinned[$relative]) {
        throw "Pinned Chao8Side training backend changed: $relative"
    }
}
New-Item -ItemType Directory -Path $runtimeRoot | Out-Null
$folders = @('TrainingPython','TrainingRebuild','CUDA11_118','AIRuntime','ModelConverter')
foreach ($name in $folders) {
    $source = Join-Path $DonorRuntime $name
    $target = Join-Path $runtimeRoot $name
    New-Item -ItemType Directory -Path $target | Out-Null
    if ($name -eq 'TrainingPython') {
        & robocopy $source $target /E /XJ /XD (Join-Path $source 'GpuRuntime') /R:1 /W:1 /NFL /NDL /NP
    } else {
        & robocopy $source $target /E /XJ /R:1 /W:1 /NFL /NDL /NP
    }
    if ($LASTEXITCODE -ge 8) { throw "Runtime copy failed for ${name}: $LASTEXITCODE" }
}
$gpuAlias = Join-Path $runtimeRoot 'TrainingPython\GpuRuntime'
New-Item -ItemType Junction -Path $gpuAlias -Target (Join-Path $runtimeRoot 'CUDA11_118') | Out-Null

foreach ($relative in $pinned.Keys) {
    $target = Join-Path $runtimeRoot $relative
    if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $pinned[$relative]) {
        throw "Staged training backend checksum mismatch: $relative"
    }
}
foreach ($relative in @(
    'TrainingPython\Python38\python.exe',
    'TrainingPython\Python38\python38.dll',
    'TrainingPython\GpuRuntime\cudnn64_8.dll',
    'TrainingRebuild\train_compat.py',
    'AIRuntime\ChaoAIRuntime.exe',
    'ModelConverter\PyFrozenLib\DAVS\DAVSTrain_v3.pyc',
    'ModelConverter\TensorRT861\nvinfer.dll'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $runtimeRoot $relative))) {
        throw "Staged runtime missing: $relative"
    }
}

$manifest = [ordered]@{
    schema='InspTools.Standalone.Chao8SideTrainingRuntime.v1'
    status='STAGED_ISOLATED_CANDIDATE'
    source=$DonorRuntime
    staged_at=(Get-Date).ToString('o')
    profiles=@('DIA_COMPAT')
    runtime_directories=$folders
    pinned_files=$pinned
    runtime_gpu_alias='Runtime\TrainingPython\GpuRuntime -> Runtime\CUDA11_118'
    production_model_deployment='NOT_RUN'
}
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath `
    (Join-Path $Candidate 'training-runtime-manifest.json') -Encoding UTF8
$appManifestPath = Join-Path $Candidate 'standalone-manifest.json'
$appManifest = Get-Content -LiteralPath $appManifestPath -Raw | ConvertFrom-Json
$appManifest.training_runtime = 'STAGED_ISOLATED_CANDIDATE'
$appManifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $appManifestPath -Encoding UTF8
[pscustomobject]@{status='PASS';candidate=$Candidate;runtime_source=$DonorRuntime;
    training_backend_sha256=$pinned['TrainingRebuild\train_compat.py'];
    davs_sha256=$pinned['ModelConverter\PyFrozenLib\DAVS\DAVSTrain_v3.pyc'];
    runtime_directories=$folders} | ConvertTo-Json -Depth 4
