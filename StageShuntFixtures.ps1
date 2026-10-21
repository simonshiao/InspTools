param([string]$Sandbox = '')

# Offline data only. ShuntVC is never launched or modified.
$ErrorActionPreference = 'Stop'
$ProjectRoot = [IO.Path]::GetFullPath($PSScriptRoot)
if ([string]::IsNullOrWhiteSpace($Sandbox)) {
    $Sandbox = Join-Path $ProjectRoot 'output\native-lazy\runtime'
}
$Sandbox = [IO.Path]::GetFullPath($Sandbox)
$allowed = [IO.Path]::GetFullPath((Join-Path $ProjectRoot 'output\native-lazy\runtime')).TrimEnd('\')
if ($Sandbox.TrimEnd('\') -ne $allowed) { throw 'Fixture staging is permitted only in the R3 sandbox.' }

$roots = @(Get-Item -Path 'D:\0_*\1.*\Shunt*\Runtime\ShuntVC' -ErrorAction SilentlyContinue |
    Where-Object { $_.PSIsContainer })
if ($roots.Count -ne 1) { throw ('Expected one Shunt product AppBase, got ' + $roots.Count) }
$Shunt = $roots[0].FullName
$SourceRecipe = Join-Path $Shunt 'Recipe\3923-R002'
$FixtureRecipe = 'R2PROBE-SHUNT-3923-R002'
$DestRecipe = Join-Path (Join-Path $Sandbox 'Recipe') $FixtureRecipe
$cli = Join-Path $Sandbox 'InspToolsRecipeCli.exe'
if (-not (Test-Path -LiteralPath $cli)) { throw 'R3 recipe CLI missing' }

# Import through the same independent Recipe system (no overwrite).
if (-not (Test-Path -LiteralPath $DestRecipe)) {
    Push-Location $Sandbox
    try {
        & $cli --recipe-import $SourceRecipe $FixtureRecipe
        if ($LASTEXITCODE -ne 0) { throw ('Recipe import failed: ' + $LASTEXITCODE) }
    } finally { Pop-Location }
}
Push-Location $Sandbox
try {
    & $cli --recipe-check $FixtureRecipe
    if ($LASTEXITCODE -ne 0) { throw ('Recipe validation failed: ' + $LASTEXITCODE) }
} finally { Pop-Location }

$samples = @(
    @{ name='OK'; rel='TrainingImage\R002-CCD1\Origional\1\OK\OK-1-2025-06-27-16-54-46.jpg' },
    @{ name='NG'; rel='TrainingImage\R002-CCD1\Origional\1\NG\NG-1-2025-06-27-16-55-12.png' }
)
$imgDir = Join-Path $Sandbox 'TrainingImage\R3-Shunt-CCD1'
[IO.Directory]::CreateDirectory($imgDir) | Out-Null
$manifestSamples = @()
foreach ($s in $samples) {
    $source = Join-Path $Shunt $s.rel
    if (-not (Test-Path -LiteralPath $source)) { throw ('Sample missing: ' + $source) }
    $hashBefore = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
    $ext = [IO.Path]::GetExtension($source).ToLowerInvariant()
    $dest = Join-Path $imgDir ($s.name + $ext)
    Copy-Item -LiteralPath $source -Destination $dest -Force
    $hashAfter = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
    $copiedHash = (Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash
    if ($hashBefore -ne $hashAfter -or $hashBefore -ne $copiedHash) {
        throw ('Source/copy SHA mismatch: ' + $s.name)
    }
    $manifestSamples += [pscustomobject]@{
        label=$s.name
        source=$source
        copy=$dest
        bytes=(Get-Item -LiteralPath $source).Length
        sha256=$copiedHash
    }
}
$manifest = [pscustomobject]@{
    kind='shunt-fixtures-readonly'
    recipe_source=$SourceRecipe
    recipe_local=$DestRecipe
    recipe_file_count=@(Get-ChildItem -LiteralPath $DestRecipe -File -Recurse).Count
    images=$manifestSamples
    production_untouched=$true
}
$manifest | ConvertTo-Json -Depth 6 | Out-File -LiteralPath (Join-Path $Sandbox 'shunt-fixtures-manifest.json') -Encoding utf8
$manifest | ConvertTo-Json -Depth 6
