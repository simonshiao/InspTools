param([string]$DonorApp = '')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
if (-not $DonorApp) { $DonorApp = Join-Path (Split-Path -Parent $root) 'ShuntVC_Chao\Runtime' }
if (-not (Test-Path -LiteralPath (Join-Path $app 'InspToolsStandalone.exe'))) { throw 'Build standalone first' }
foreach ($name in @('TrainingImage', 'Model', 'Training')) {
    $source = Join-Path $DonorApp $name
    $target = Join-Path $app $name
    if (-not (Test-Path -LiteralPath $source)) { throw "Missing donor: $source" }
    New-Item -ItemType Directory -Path $target -Force | Out-Null
    # /XC /XN /XO preserve every existing local file, even if different.
    # /XJ avoids following junctions; never mirror or delete user data.
    & robocopy $source $target /E /XJ /XC /XN /XO /R:1 /W:1 /NFL /NDL /NP
    if ($LASTEXITCODE -ge 8) { throw "Copy failed: $name code=$LASTEXITCODE" }
    $count = (Get-ChildItem -LiteralPath $target -Recurse -File | Measure-Object).Count
    Write-Output "TRAINING_DATA_STAGE=$name FILES=$count"
}
Write-Output 'TRAINING_DATA_STAGE=PASS SOURCE_READ_ONLY=True EXISTING_PRESERVED=True'
