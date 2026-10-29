param([string]$Dataset = 'DIA_CCD1_S1_Train300')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$vendor = [IO.Path]::GetFullPath('D:\delta\Welding\DIAChip8Side\TrainingImage\R002-CCD1')
$staged = Join-Path $app 'TrainingImage\VPMGK05500VN0Y0BW-CCD1'
$plan = Join-Path $app 'TrainingPlans\DIA_CCD1_S1_BalancedSmoke.json'
$dest = Join-Path $app ('TrainingImage\' + $Dataset)
$out = Join-Path $app ('TrainingPlans\' + $Dataset + '.json')
$record = [IO.File]::ReadAllText($plan, [Text.Encoding]::UTF8) | ConvertFrom-Json
if ($record.dataset -ne 'DIA_CCD1_S1_BalancedSmoke' -or
    -not $record.source_read_only -or @($record.holdout_originals).Count -ne 4) {
    throw 'The audited CCD1 holdout manifest is required'
}
$holdout = @($record.holdout_originals | ForEach-Object { $_.family })
function Family([string]$name) { return ($name -replace '-(H|V|VH)$','') }
function Sha([string]$file) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Missing original: $file" }
    return (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
}
function CopyVerified([string]$src,[string]$target) {
    $hash = Sha $src
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) | Out-Null
    if (Test-Path -LiteralPath $target) {
        if ((Sha $target) -ne $hash) { throw "Existing training target differs: $target" }
    } else {
        Copy-Item -LiteralPath $src -Destination $target
    }
    if ((Sha $target) -ne $hash) { throw "Copy check failed: $target" }
    return $hash
}

$entries = @()
$families = @{}
foreach ($class in @('OK','NG')) {
    $xbase = Join-Path $staged ('X\'+$class)
    $ybase = Join-Path $staged ('Y\'+$class)
    $vxbase = Join-Path $vendor ('X\'+$class)
    $vybase = Join-Path $vendor ('Y\'+$class)
    foreach ($file in Get-ChildItem -LiteralPath $xbase -File -Recurse) {
        if ($file.Extension.ToLowerInvariant() -notin @('.png','.jpg','.jpeg','.bmp')) { continue }
        $rel = $file.FullName.Substring($xbase.Length).TrimStart('\')
        $stem = [IO.Path]::GetFileNameWithoutExtension($file.Name)
        $family = Family $stem
        if ($holdout -contains $family) { continue }
        $localY = Join-Path $ybase $rel
        $vendorX = Join-Path $vxbase $rel
        $vendorY = Join-Path $vybase $rel
        if ((Sha $file.FullName) -ne (Sha $vendorX) -or
            (Sha $localY) -ne (Sha $vendorY)) {
            throw "Staged CCD1 source differs from the original: $rel"
        }
        $xhash = CopyVerified $file.FullName (Join-Path $dest ('X\'+$class+'\'+$rel))
        $yhash = CopyVerified $localY (Join-Path $dest ('Y\'+$class+'\'+$rel))
        $families[$class+':'+$family] = $true
        $entries += [pscustomobject]@{
            label=$class
            family=$family
            relative=$rel
            x_sha256=$xhash
            y_sha256=$yhash
        }
    }
}
$ok = @($entries|Where-Object{$_.label -eq 'OK'}).Count
$ng = @($entries|Where-Object{$_.label -eq 'NG'}).Count
if ($ok -ne 19 -or $ng -ne 24 -or $families.Count -ne 25 -or
    $entries.Count -ne 43) {
    throw "Expected CCD1 19 OK + 24 NG augmentation pairs, 25 families; got OK=$ok NG=$ng families=$($families.Count)"
}
# Training data excludes ALL four original-image holdout families, including
# every mirrored H/V/VH version; no validation originals are copied here.
$manifest = [ordered]@{
    dataset=$Dataset
    source='DIAChip8Side/R002-CCD1'
    source_read_only=$true
    training_pairs=43
    training_ok_pairs=19
    training_ng_pairs=24
    unique_training_families=25
    heldout_families=$holdout
    heldout_count=4
    epoch_target=300
    training_only=$true
    deployed=$false
    entries=@($entries|Sort-Object label,relative)
}
$json = $manifest|ConvertTo-Json -Depth 7
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($out))|Out-Null
if (Test-Path -LiteralPath $out) {
    if ([IO.File]::ReadAllText($out, [Text.Encoding]::UTF8).Trim() -ne $json.Trim()) {
        throw 'Long training manifest differs; refusing overwrite'
    }
} else { [IO.File]::WriteAllText($out,$json,[Text.Encoding]::UTF8) }
Write-Output "DIA_CCD1_LONG_STAGE=PASS OK=$ok NG=$ng FAMILIES=$($families.Count) HELDOUT=4 DEPLOYED=False"
