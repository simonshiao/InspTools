param(
    [Parameter(Mandatory=$true)][string]$Candidate
)

# CHAO self-trained (vendor-seeded) 300 epoch checkpoint; independent review only.
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$candidate = [IO.Path]::GetFullPath($Candidate).TrimEnd('\')
$prefix = (Join-Path $project 'output\standalone-r3') + '\'
if (-not $candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Candidate outside isolated output/standalone-r3'
}
if ((Get-Content -LiteralPath (Join-Path $candidate 'portable-review.marker') -Raw).Trim() -ne 'INSPTOOLS_PORTABLE_REVIEW_R3_V1') {
    throw 'Unknown candidate marker'
}
if ((Get-FileHash (Join-Path $candidate 'InspTools.dll') -Algorithm SHA256).Hash -ne 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B') {
    throw 'Unverified native InspTools DLL'
}
$oldProject = @(Get-ChildItem -LiteralPath (Split-Path -Parent $project) -Directory |
    Where-Object { $_.Name -like 'InspTools*' -and $_.FullName -ne $project -and
        (Test-Path -LiteralPath (Join-Path $_.FullName 'output\decoded-ui\Release\TrainingCandidates\ShuntLoss6_CCD1_300E_20261022\temp_unet_US_plain.h5')) } |
    Select-Object -First 1 -ExpandProperty FullName)
if (-not $oldProject) { throw 'Historical CHAO 300E candidate not found' }
$sourceDir = Join-Path $oldProject 'output\decoded-ui\Release\TrainingCandidates\ShuntLoss6_CCD1_300E_20261022'
$sourceModel = Join-Path $sourceDir 'temp_unet_US_plain.h5'
$sourceReport = Join-Path $sourceDir 'quality_recovery_result.json'
$expectedModel = '3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634'
$actualModel = (Get-FileHash -LiteralPath $sourceModel -Algorithm SHA256).Hash
if ($actualModel -ne $expectedModel) {
    throw ('300E self-trained checkpoint SHA mismatch: path=' + $sourceModel + ' actual=' + $actualModel)
}
$quality = Get-Content -LiteralPath $sourceReport -Raw | ConvertFrom-Json
if ($quality.epochs -ne 300 -or $quality.production_qualified -ne $false -or
    [Math]::Abs($quality.shunt_loss6_300_holdout_r_iou - 0.7770970782280867) -gt 0.0000001) {
    throw '300E quality provenance is not the audited reference'
}
$modelTarget = Join-Path $candidate 'ChaoAI\Models\locate-ccd1-loss6-300e.h5'
$scriptTarget = Join-Path $candidate 'ChaoAI\Runtime\ChaoLocateOffline.py'
$reportTarget = Join-Path $candidate 'ChaoAI\locate-300e-provenance.json'
foreach($dest in @($modelTarget, $scriptTarget, $reportTarget)) {
    if (Test-Path -LiteralPath $dest) { throw ('Refusing to overwrite previous stage: ' + $dest) }
    [IO.Directory]::CreateDirectory((Split-Path -Parent $dest)) | Out-Null
}
Copy-Item -LiteralPath $sourceModel -Destination $modelTarget
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ChaoLocateOffline.py') -Destination $scriptTarget
if ((Get-FileHash -LiteralPath $modelTarget -Algorithm SHA256).Hash -ne $expectedModel) {
    throw 'Staged model SHA mismatch'
}
$manifest = [ordered]@{
    schema = 'chao-self-locate-300e-v1'
    source = $sourceModel
    original_sha256 = $expectedModel
    staged_sha256 = (Get-FileHash $modelTarget -Algorithm SHA256).Hash
    offline_script_sha256 = (Get-FileHash $scriptTarget -Algorithm SHA256).Hash
    epochs = 300
    backend = 'SHUNT_20260930_AGENTVISIONLAB_LOSS6_VENDOR_SEEDED'
    source_seeded_from_vendor = $true
    holdout_family_count = 4
    holdout_r_iou = $quality.shunt_loss6_300_holdout_r_iou
    vendor_reference_r_iou = $quality.original_vendor_r_iou
    production_qualified = $false
    deployment_allowed = $false
    target_station_scope = 'S1 Shunt CCD1 smoke only'
}
$manifest | ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath $reportTarget -Encoding UTF8
[pscustomobject]@{
    status='CHAO_LOCATE_300E_STAGED'
    candidate=$candidate
    model_sha256=$expectedModel
    validation_iou=$quality.shunt_loss6_300_holdout_r_iou
    deployment_allowed=$false
} | ConvertTo-Json
