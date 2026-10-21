param(
    [Parameter(Mandatory=$true)][string]$GateDirectory,
    [switch]$IncludeCudnnInference
)

# Only adds OpenCV's directly imported, unmodified runtime DLL dependencies
# to an existing disposable gate. Never writes ShuntVC, source, user Recipe.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath($PSScriptRoot)
$allowed = [IO.Path]::GetFullPath((Join-Path $Root 'output\delta-native-gates')).
    TrimEnd('\') + '\'
$target = [IO.Path]::GetFullPath($GateDirectory).TrimEnd('\')
if (-not $target.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase) -or
    -not (Split-Path -Leaf $target).StartsWith('Gate-') -or
    -not (Test-Path (Join-Path $target 'delta-runtime-gate-manifest.json'))) {
    throw 'Refusing to modify a directory that is not an existing disposable Delta Gate'
}

$donors = @(Get-Item -Path 'E:\0_*\3.*\1.*\ShuntVC_Chao\Runtime\Runtime\CUDA11_118' -ErrorAction SilentlyContinue)
if ($donors.Count -ne 1) { throw 'Expected one original CUDA runtime dependency folder' }
$srcRoot = $donors[0].FullName
$destRoot = Join-Path $target 'Runtime\CUDA11_118'
$direct = @(
    'nppc64_11.dll',
    'nppial64_11.dll',
    'nppicc64_11.dll',
    'nppidei64_11.dll',
    'nppif64_11.dll',
    'nppig64_11.dll',
    'nppim64_11.dll',
    'nppist64_11.dll',
    'nppitc64_11.dll',
    'cublas64_11.dll',
    'cublasLt64_11.dll',
    'cudnn64_8.dll',
    'cufft64_10.dll'
)
if ($IncludeCudnnInference) {
    # cuDNN loads these at runtime; they will not appear as static imports
    # of DeltaAIProcessDll. Never include train-only libs in inference Gate.
    $direct += @('cudnn_ops_infer64_8.dll',
                 'cudnn_cnn_infer64_8.dll',
                 'cudnn_adv_infer64_8.dll')
}
$status = @()
foreach ($name in $direct) {
    $src = Join-Path $srcRoot $name
    $dst = Join-Path $destRoot $name
    if (-not (Test-Path -LiteralPath $src)) { throw ('Missing original DLL: ' + $src) }
    $sourceHash = (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash
    if (Test-Path -LiteralPath $dst) {
        if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $sourceHash) {
            throw ('Hash mismatch on existing gate DLL: ' + $dst)
        }
    } else {
        Copy-Item -LiteralPath $src -Destination $dst
        if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $sourceHash) {
            throw ('Hash mismatch on copied gate DLL: ' + $dst)
        }
    }
    $status += [pscustomobject]@{
        name=$name; bytes=(Get-Item -LiteralPath $dst).Length; sha256=$sourceHash
    }
}
[pscustomobject]@{
    status='DIRECT_DEPENDENCIES_STAGED_NOT_TESTED'
    gate=$target
    added_or_checked=$status.Count
    added_mb=[Math]::Round((($status|Measure-Object bytes -Sum).Sum)/1MB,1)
    donor_unchanged=$true
} | ConvertTo-Json -Depth 3
