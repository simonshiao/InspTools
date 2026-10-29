param([string]$DonorRuntime = '')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
if (-not $DonorRuntime) { $DonorRuntime = Join-Path (Split-Path -Parent $root) 'ShuntVC_Chao\Runtime\Runtime' }
$source = Join-Path $DonorRuntime 'ModelConverter'
$target = Join-Path $app 'Runtime\ModelConverter'
if (-not (Test-Path (Join-Path $app 'Runtime\TrainingPython\Python38\python38.dll'))) {
    throw 'Product-owned TrainingPython runtime missing'
}
if (-not (Test-Path (Join-Path $app 'Runtime\CUDA11_118\cudnn64_8.dll'))) {
    throw 'Product-owned CUDA11_118 runtime missing'
}

# PBD-only packaging. Exclude GPU-specific TensorRT861 (1.3 GB), Work and cache;
# the worker advertises TRT=BUILDER_MISSING while retaining PBD=READY.
$pinned = @{
    'ModelConverterWorker.exe' = 'A2DCA196BDE2E7F67B3702C16C609862984C584752BDBEE1B905AB2E20463833'
    'h5_to_pb_118.py' = '01D68D5DF4B3FD68A2C642EFBA403EBC2E090199400A9E8E40CC79FFCC82E1E0'
    'transform_graph.exe' = '258E55B7E092272E6E1090830229516E514843DDD9600937FB62B18FFDE662B3'
    'h52intOnnx.py' = '287FBA7AAFD3CB4F7C43F40223FDDA33B5A885345FBB7FF2233B46F187A4CE9D'
    'trt_build_861.py' = '2221814F24A5F235B87552D0EB359E46E029A18FCFE7228527B09A341BD53017'
}
[IO.Directory]::CreateDirectory($target) | Out-Null
foreach ($name in $pinned.Keys) {
    $from = Join-Path $source $name
    $to = Join-Path $target $name
    if (-not (Test-Path -LiteralPath $from -PathType Leaf) -or
        (Get-FileHash -LiteralPath $from).Hash -ne $pinned[$name]) {
        throw "Donor converter file changed: $name"
    }
    if (Test-Path -LiteralPath $to) {
        if ((Get-FileHash -LiteralPath $to).Hash -ne $pinned[$name]) {
            throw "Existing product converter file differs: $name"
        }
    } else { Copy-Item -LiteralPath $from -Destination $to }
    if ((Get-FileHash -LiteralPath $to).Hash -ne $pinned[$name]) {
        throw "Converter SHA-256 mismatch: $name"
    }
}

$frozenSource = Join-Path $source 'PyFrozenLib'
$frozenTarget = Join-Path $target 'PyFrozenLib'
if (-not (Test-Path (Join-Path $frozenSource 'tensorflow'))) {
    throw 'Missing vendor PyFrozenLib dependency cache'
}
[IO.Directory]::CreateDirectory($frozenTarget) | Out-Null
& robocopy $frozenSource $frozenTarget /E /XJ /XC /XN /XO /R:1 /W:1 /NFL /NDL /NP
if ($LASTEXITCODE -ge 8) { throw "PyFrozenLib stage failed: $LASTEXITCODE" }
$missing = 0
foreach ($file in Get-ChildItem -LiteralPath $frozenSource -File -Recurse) {
    $rel = $file.FullName.Substring($frozenSource.Length).TrimStart('\')
    if (-not (Test-Path -LiteralPath (Join-Path $frozenTarget $rel) -PathType Leaf)) { $missing++ }
}
if ($missing) { throw "Missing staged PyFrozenLib dependencies: $missing" }
Write-Output 'MODEL_CONVERTER_STAGE=PASS MODE=PBD_ONLY DONOR_READ_ONLY=True PINNED_SHA=True'
