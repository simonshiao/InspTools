param(
    [string]$VendorRoot = 'D:\delta\Welding\DIAChip8Side'
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$source = Join-Path $VendorRoot 'TrainingImage\R002-CCD1'
$staged = Join-Path $app 'TrainingImage\VPMGK05500VN0Y0BW-CCD1'
$dataset = 'DIA_CCD1_S1_BalancedSmoke'
$destination = Join-Path $app ('TrainingImage\' + $dataset)
$metadata = Join-Path $app ('TrainingPlans\' + $dataset + '.json')
Add-Type -AssemblyName System.Drawing

function Fingerprint([string]$path) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Source image/mask missing: $path"
    }
    return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
}
function Family([string]$stem) {
    return ($stem -replace '-(H|V|VH)$','')
}
function PairIndex([string]$base, [string]$label) {
    $group = Join-Path $base ('X\' + $label)
    $index = @{}
    foreach ($image in Get-ChildItem -LiteralPath $group -File -Recurse) {
        if ($image.Extension -notin @('.png','.jpg','.jpeg','.bmp')) { continue }
        $relative = $image.FullName.Substring($group.Length).TrimStart('\')
        if ($index.ContainsKey($relative)) { throw "Duplicate X image path: $relative" }
        $mask = Join-Path (Join-Path $base ('Y\'+$label)) $relative
        if (-not (Test-Path -LiteralPath $mask -PathType Leaf)) {
            throw "Missing Y mask with the same relative path: $relative"
        }
        $stem = [IO.Path]::GetFileNameWithoutExtension($image.Name)
        $index[$relative] = [pscustomobject]@{
            label = $label
            relative = $relative
            family = (Family $stem)
            x = $image.FullName
            y = $mask
        }
    }
    $yGroup = Join-Path $base ('Y\' + $label)
    $countY = @(Get-ChildItem -LiteralPath $yGroup -File -Recurse |
        Where-Object { $_.Extension -in @('.png','.jpg','.jpeg','.bmp') }).Count
    if ($index.Count -ne $countY) {
        throw "Unmatched Y images for $label"
    }
    return $index
}
function SafeCopy([string]$from,[string]$to) {
    $sha = Fingerprint $from
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($to)) | Out-Null
    if (Test-Path -LiteralPath $to) {
        if ((Fingerprint $to) -ne $sha) {
            throw "Existing local file differs, refusing overwrite: $to"
        }
    } else {
        Copy-Item -LiteralPath $from -Destination $to
    }
    if ((Fingerprint $to) -ne $sha) { throw "Copy SHA-256 verification failed: $to" }
    return $sha
}

foreach ($folder in @($source,$staged)) {
    if (-not (Test-Path -LiteralPath $folder -PathType Container)) {
        throw "Training source is not staged: $folder"
    }
}

$all = @{}
foreach ($group in @('OK','NG')) {
    $vendorPairs = PairIndex $source $group
    $localPairs = PairIndex $staged $group
    if ($vendorPairs.Count -ne $localPairs.Count) {
        throw "Source vs staged image counts differ: $group"
    }
    foreach ($key in $vendorPairs.Keys) {
        if (-not $localPairs.ContainsKey($key)) {
            throw "Source vs staged relative path differs: $key"
        }
        $vp = $vendorPairs[$key]
        $lp = $localPairs[$key]
        if ((Fingerprint $vp.x) -ne (Fingerprint $lp.x) -or
            (Fingerprint $vp.y) -ne (Fingerprint $lp.y)) {
            throw "Local training copy is not byte-for-byte the DIA vendor original: $key"
        }
        $xBitmap = [Drawing.Image]::FromFile($vp.x)
        try {
            $yBitmap = [Drawing.Image]::FromFile($vp.y)
            try {
                if ($xBitmap.Width -ne $yBitmap.Width -or
                    $xBitmap.Height -ne $yBitmap.Height) {
                    throw "X/Y mask dimension mismatch: $key"
                }
            } finally { $yBitmap.Dispose() }
        } finally { $xBitmap.Dispose() }
        $id = $group + '/' + $key
        $all[$id] = $vp
    }
}

$ok = @($all.Values | Where-Object { $_.label -eq 'OK' })
$ng = @($all.Values | Where-Object { $_.label -eq 'NG' })
$okFamilies = @($ok | Select-Object -ExpandProperty family -Unique)
$ngFamilies = @($ng | Select-Object -ExpandProperty family -Unique)
if ($ok.Count -ne 21 -or $ng.Count -ne 32 -or
    $okFamilies.Count -ne 21 -or $ngFamilies.Count -ne 8) {
    throw "Unexpected vendor CCD1 family structure: OK=$($ok.Count) NG=$($ng.Count)"
}
foreach ($pair in $all.Values) {
    $ext = if ($pair.label -eq 'OK') { '.jpg' } else { '.png' }
    $original = Join-Path $source ('Origional\1\'+$pair.label+'\'+$pair.family+$ext)
    if (-not (Test-Path -LiteralPath $original -PathType Leaf)) {
        throw "Labeled crop has no identifiable original image: $($pair.family)"
    }
}

$holdout = @(
    'OK-1-2025-06-27-16-54-46',
    'OK-1-2025-06-27-16-58-19',
    'NG-1-2025-06-27-16-55-12',
    'NG-1-2025-06-27-16-55-24'
)
$chosen = @(
    'OK-1-2025-06-27-16-58-20',
    'OK-1-2025-06-27-16-58-24',
    'OK-1-2025-06-27-16-58-35',
    'OK-1-2025-06-27-17-21-35',
    'NG-1-2025-06-27-17-12-31',
    'NG-1-2025-06-27-17-13-00',
    'NG-1-2025-06-27-17-13-26',
    'NG-1-2025-06-27-17-16-45'
)
if (@($holdout | Where-Object { $chosen -contains $_ }).Count -ne 0 -or
    @($chosen | Select-Object -Unique).Count -ne 8) {
    throw 'Train/validation family leakage or duplicate smoke family'
}
$smokePairs = @()
foreach ($family in $chosen) {
    $matches = @($all.Values | Where-Object { $_.family -eq $family })
    # NG H/V/VH augmentations must all stay with the same base family;
    # for a smoke test use only the unmodified original crop.
    $pair = @($matches | Where-Object {
        [IO.Path]::GetFileNameWithoutExtension($_.relative) -eq $family
    })
    if ($pair.Count -ne 1) {
        throw "Exactly one unaugmented X/Y pair required for $family"
    }
    $item = $pair[0]
    $fileName = [IO.Path]::GetFileName($item.relative)
    $xSha = SafeCopy $item.x (Join-Path $destination ('X\'+$item.label+'\'+$fileName))
    $ySha = SafeCopy $item.y (Join-Path $destination ('Y\'+$item.label+'\'+$fileName))
    $smokePairs += [pscustomobject]@{
        label=$item.label; family=$family; name=$fileName
        x_sha256=$xSha; y_sha256=$ySha
    }
}

$holdoutImages = @()
foreach ($family in $holdout) {
    $label = if ($family.StartsWith('OK-')) { 'OK' } else { 'NG' }
    $suffix = if ($label -eq 'OK') { '.jpg' } else { '.png' }
    $path = Join-Path $source ('Origional\1\'+$label+'\'+$family+$suffix)
    $holdoutImages += [pscustomobject]@{
        family=$family; label=$label; original=$path; original_sha256=(Fingerprint $path)
    }
}
$record = [ordered]@{
    dataset=$dataset
    source_read_only=$true
    source_full_pair_count=$all.Count
    source_ok_pairs=$ok.Count
    source_ng_pairs=$ng.Count
    source_unique_ok_families=$okFamilies.Count
    source_unique_ng_families=$ngFamilies.Count
    augmented_ng_variants=$($ng.Count - $ngFamilies.Count)
    train_smoke_only=$true
    not_approved_for_quality_or_deployment=$true
    training_pairs=$smokePairs
    holdout_originals=$holdoutImages
    rule='Never train on holdout family OR its H/V/VH variants; evaluate using true original images'
}
$json = $record | ConvertTo-Json -Depth 7
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($metadata)) | Out-Null
if (Test-Path -LiteralPath $metadata) {
    $existing = [IO.File]::ReadAllText($metadata)
    if ($existing.Trim() -ne $json.Trim()) { throw 'Existing smoke manifest differs; refusing overwrite' }
} else {
    [IO.File]::WriteAllText($metadata, $json, [Text.Encoding]::UTF8)
}
Write-Output ('DIA_CCD1_AUDIT=PASS X_Y_PAIRS='+$all.Count+' OK='+$ok.Count+' NG='+$ng.Count+
    ' UNIQUE_OK='+$okFamilies.Count+' UNIQUE_NG='+$ngFamilies.Count)
Write-Output ('DIA_CCD1_SMOKE_STAGE=PASS DATASET='+$dataset+
    ' TRAIN_OK=4 TRAIN_NG=4 RESERVED_HOLDOUT_FAMILIES='+$holdout.Count+
    ' NG_AUGMENTATION_LEAKAGE=False DEPLOYMENT=NEVER')
Write-Output ('DIA_CCD1_MANIFEST='+$metadata)
