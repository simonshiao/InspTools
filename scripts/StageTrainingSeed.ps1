$ErrorActionPreference = 'Stop'
$root = (Split-Path -Parent $PSScriptRoot)
$source = Join-Path (Split-Path -Parent $root) 'ShuntVC_Chao\Runtime\Runtime\ModelConverter\Work\91f0527a25984ed29d3a9e90c311a5b1\navs.h5'
$targetDir = Join-Path $root 'output\decoded-ui\Release\TrainingSeed'
$target = Join-Path $targetDir 'navs.h5'
if (-not (Test-Path -LiteralPath $source)) { throw 'Seed candidate missing' }
$header = [IO.File]::ReadAllBytes($source)
if ($header.Length -lt 4096 -or [BitConverter]::ToString($header, 0, 4) -ne '01-00-00-00') { throw 'Unexpected DAVS seed header' }
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
if (Test-Path -LiteralPath $target) {
    if ((Get-FileHash $target).Hash -ne (Get-FileHash $source).Hash) { throw 'Existing seed differs; refusing overwrite' }
} else {
    Copy-Item -LiteralPath $source -Destination $target
}
if ((Get-FileHash $target).Hash -ne (Get-FileHash $source).Hash) { throw 'Seed copy verification failed' }
Write-Output 'TRAINING_SEED_STAGE=PASS DAVS_WRAPPED=True DONOR_READ_ONLY=True'
