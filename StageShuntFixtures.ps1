param([string]$Sandbox = '', [switch]$IncludeS1Model)

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
    @{ name='NG'; rel='TrainingImage\R002-CCD1\Origional\1\NG\NG-1-2025-06-27-16-55-12.png' },
    # These correspond to the OKPath/NGPath in Recipe/3923-R002/station1.ini.
    # They are NOT the smaller 1292x964 R002-CCD1 samples.
    @{ name='S1OK'; rel='TrainingImage\VSMA8420AYM050JEW_A1\Origional\1\ok\OK (1).png' },
    @{ name='S1NG'; rel='TrainingImage\VSMA8420AYM050JEW_A1\Origional\1\ng\2026-10-01-21-04-34.png' }
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
    if ($s.name -eq 'S1OK' -and $copiedHash -ne 'A7AFF9D716CD394DE072E84564F916BA58744133A52C5E1B9D2B8B287AD3F5CA') {
        throw 'Recipe-matched S1 OK SHA drift'
    }
    if ($s.name -eq 'S1NG' -and $copiedHash -ne 'DA43AAC9BE43B84E1B3AE9366468260EF368917E79B370E93628E9254B0E5F48') {
        throw 'Recipe-matched S1 NG SHA drift'
    }
    $manifestSamples += [pscustomobject]@{
        label=$s.name
        source=$source
        copy=$dest
        bytes=(Get-Item -LiteralPath $source).Length
        sha256=$copiedHash
    }
}
$modelManifest = $null
if ($IncludeS1Model) {
    # Optional, opt-in model pair. The full Shunt Model and Training
    # trees remain untouched and are never copied into this product.
    $models = @(
        @{ name='VSMA8420AYM050JEW_A1.h5d'; sha='BB36AAA4017D154F161895E63CFB2B97C3CBF1F549676F0D3CB12A138D3C6476' },
        @{ name='VSMA8420AYM050JEW_A1.pbd'; sha='B5B8759891FA93CED197347C95F038724F620F10E0969AD7D7F6278D8651AEA5' }
    )
    $modelDir = Join-Path $Sandbox 'Model'
    [IO.Directory]::CreateDirectory($modelDir) | Out-Null
    $modelManifest = @()
    foreach ($m in $models) {
        $modelSource = Join-Path (Join-Path $Shunt 'Model') $m.name
        if ((Get-FileHash -LiteralPath $modelSource -Algorithm SHA256).Hash -ne $m.sha) {
            throw ('Shunt S1 original model drift: ' + $m.name)
        }
        $modelCopy = Join-Path $modelDir $m.name
        if (Test-Path -LiteralPath $modelCopy) {
            if ((Get-FileHash -LiteralPath $modelCopy -Algorithm SHA256).Hash -ne $m.sha) {
                throw ('R3 sandbox model conflicts with pinned S1 asset: ' + $m.name)
            }
        } else {
            Copy-Item -LiteralPath $modelSource -Destination $modelCopy
        }
        $sourceAfter = (Get-FileHash -LiteralPath $modelSource -Algorithm SHA256).Hash
        $copiedAfter = (Get-FileHash -LiteralPath $modelCopy -Algorithm SHA256).Hash
        if ($sourceAfter -ne $m.sha -or $copiedAfter -ne $m.sha) {
            throw ('S1 model source/copy hash mismatch: ' + $m.name)
        }
        $modelManifest += [pscustomobject]@{
            source=$modelSource
            copy=$modelCopy
            bytes=(Get-Item -LiteralPath $modelCopy).Length
            sha256=$copiedAfter
        }
    }
}
$manifest = [pscustomobject]@{
    kind='shunt-fixtures-readonly'
    recipe_source=$SourceRecipe
    recipe_local=$DestRecipe
    recipe_file_count=@(Get-ChildItem -LiteralPath $DestRecipe -File -Recurse).Count
    images=$manifestSamples
    optional_s1_model=$modelManifest
    production_untouched=$true
}
$manifest | ConvertTo-Json -Depth 6 | Out-File -LiteralPath (Join-Path $Sandbox 'shunt-fixtures-manifest.json') -Encoding utf8
$manifest | ConvertTo-Json -Depth 6
