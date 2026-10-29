param(
    [string]$Configuration = 'Release',
    [switch]$Rebuild,
    [switch]$Zip
)

$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$BuildRoot = Join-Path $Root ("output\decoded-ui\" + $Configuration)
$ReleaseRoot = Join-Path $Root 'release'
$Current = Join-Path $ReleaseRoot 'current'
$ZipPath = Join-Path $ReleaseRoot 'DIA-InspectionRuntime.zip'
$OldDecodedZip = Join-Path $ReleaseRoot 'InspTools-Rebuild2-DecodedUI.zip'
$OldIr206Zip = Join-Path $ReleaseRoot 'InspTools-Rebuild2-IR2-06.zip'

if ($Rebuild -or -not (Test-Path -LiteralPath (Join-Path $BuildRoot 'InspToolsStandalone.exe'))) {
    & (Join-Path $Root 'scripts\Build.ps1') -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'Decoded UI build failed.' }
}

if (-not (Test-Path -LiteralPath (Join-Path $BuildRoot 'build-manifest.json'))) {
    throw "Decoded UI build manifest missing: $BuildRoot"
}

if (-not (Test-Path -LiteralPath $ReleaseRoot)) {
    [IO.Directory]::CreateDirectory($ReleaseRoot) | Out-Null
}
if (Test-Path -LiteralPath $Current) {
    Remove-Item -LiteralPath $Current -Recurse -Force
}
[IO.Directory]::CreateDirectory($Current) | Out-Null

Copy-Item -Path (Join-Path $BuildRoot '*') -Destination $Current -Recurse -Force
Copy-Item -LiteralPath (Join-Path $Root 'docs\ROUTE_RESET_20260915.md') -Destination (Join-Path $Current 'ROUTE_RESET.md') -Force
Copy-Item -LiteralPath (Join-Path $Root 'docs\PHASE1_BASELINE_AUDIT_20260915.md') -Destination (Join-Path $Current 'PHASE1_BASELINE_AUDIT.md') -Force
Copy-Item -LiteralPath (Join-Path $Root 'docs\DELIVERY_CURRENT.md') -Destination (Join-Path $Current 'README.md') -Force

# Old IR2-06 bundle belonged to the superseded machine-first/generic-editor line.
# Remove it from the formal local release area so it is not mistaken for current.
if (Test-Path -LiteralPath $OldIr206Zip) {
    Remove-Item -LiteralPath $OldIr206Zip -Force
}
if (Test-Path -LiteralPath $OldDecodedZip) {
    Remove-Item -LiteralPath $OldDecodedZip -Force
}

if ($Zip) {
    if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
    Compress-Archive -Path (Join-Path $Current '*') -DestinationPath $ZipPath -CompressionLevel Optimal
}

$files = Get-ChildItem -LiteralPath $Current -File -Recurse
[pscustomobject]@{
    status = 'PASS'
    route = 'phase1-compiled-toolswindow-standalone'
    current = $Current
    file_count = $files.Count
    physical_mb = [Math]::Round((($files | Measure-Object Length -Sum).Sum) / 1MB, 2)
    zip = $(if ($Zip) { $ZipPath } else { $null })
    zip_sha256 = $(if ($Zip) { (Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash } else { $null })
} | ConvertTo-Json -Depth 4
