param([string]$VendorRoot = 'D:\delta\Welding\DIAChip8Side')
$ErrorActionPreference = 'Stop'
$app = Join-Path (Split-Path -Parent $PSScriptRoot) 'output\decoded-ui\Release'
if (-not (Test-Path -LiteralPath (Join-Path $app 'InspToolsRecipeCli.exe'))) {
    throw 'Standalone CLI missing; build first'
}
if (-not (Test-Path -LiteralPath (Join-Path $VendorRoot 'Training\Model\navs.h5'))) {
    throw 'Original DIA vendor training model is missing'
}

function CopyPinned([string]$source, [string]$target) {
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Missing original: $source" }
    $folder = Split-Path -Parent $target
    [IO.Directory]::CreateDirectory($folder) | Out-Null
    if (Test-Path -LiteralPath $target) {
        if ((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash) {
            throw "Existing local content differs; refusing overwrite: $target"
        }
    } else {
        Copy-Item -LiteralPath $source -Destination $target
    }
    if ((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash) {
        throw "SHA-256 verification failed: $target"
    }
}

CopyPinned (Join-Path $VendorRoot 'Training\Model\navs.h5') `
    (Join-Path $app 'TrainingSeed\DIA_R002_vendor.h5')
CopyPinned (Join-Path $VendorRoot 'Training\Model\navs.ini') `
    (Join-Path $app 'TrainingProfiles\DIA_R002_vendor.ini')
## Stage only the inference models named by this recipe. Station 5 has a
## reference to 06077-15.h5d which is absent even in the original vendor Model.
$recipeModels = @(
    'R002-CCD1', 'R002-CCD2-2-0624', 'R002-CCD3-0624', 'R002-CCD4',
    'R002-CCD6-0624', 'R002-CCD7-0627', 'R002-CCD8-0624'
)
foreach ($model in $recipeModels) {
    foreach ($extension in @('.h5d', '.pbd')) {
        $name = $model + $extension
        CopyPinned (Join-Path $VendorRoot ('Model\' + $name)) `
            (Join-Path $app ('Model\' + $name))
    }
}

$recipeSource = Join-Path $VendorRoot 'Recipe\3923-R002'
$recipeDestination = Join-Path $app 'Recipe\3923-R002'
$cli = Join-Path $app 'InspToolsRecipeCli.exe'
if (-not (Test-Path -LiteralPath $recipeDestination)) {
    & $cli --recipe-import $recipeSource '3923-R002'
    if ($LASTEXITCODE -ne 0) { throw 'Standalone vendor Recipe import failed' }
}
& $cli --recipe-check '3923-R002'
if ($LASTEXITCODE -ne 0) { throw 'Standalone Recipe validation failed' }
foreach ($name in @('Setting.ini', 'station4.ini')) {
    CopyPinned (Join-Path $VendorRoot ('Recipe\3923-R002\' + $name)) `
        (Join-Path $app ('VendorReference\3923-R002\' + $name))
}

$dataset = 'DIA_3923_R002_4'
$count = 0
foreach ($axis in @('X','Y')) {
    foreach ($group in @('OK','NG')) {
        $source = Join-Path $VendorRoot ('TrainingImage\3923-R002-4\' + $axis + '\' + $group)
        if (-not (Test-Path -LiteralPath $source -PathType Container)) { continue }
        foreach ($file in Get-ChildItem -LiteralPath $source -File) {
            if ($file.Extension.ToLowerInvariant() -notin @('.png','.jpg','.jpeg','.bmp')) { continue }
            $target = Join-Path $app ('TrainingImage\' + $dataset + '\' + $axis + '\' + $group + '\' + $file.Name)
            CopyPinned $file.FullName $target
            $count++
        }
    }
}
if ($count -ne 40) { throw "Expected 20 source and 20 masks; verified $count files" }
Write-Output "DIA_VENDOR_STAGE=PASS DATASET=$dataset FILES=$count SOURCE_READ_ONLY=True VERIFIED_SHA256=True"
Write-Output 'DIA_VENDOR_REFERENCES=7/8 S5_ORIGINAL_MODEL_MISSING=06077-15.h5d'
Write-Output ('DIA_VENDOR_MODEL_SHA=' + (Get-FileHash -LiteralPath (Join-Path $app 'TrainingSeed\DIA_R002_vendor.h5')).Hash)
