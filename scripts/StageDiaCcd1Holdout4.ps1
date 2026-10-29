$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
$src = Join-Path $app 'TrainingImage\VPMGK05500VN0Y0BW-CCD1'
$dest = Join-Path $app 'TrainingImage\DIA_CCD1_S1_Holdout4'
$train = Join-Path $app 'TrainingPlans\DIA_CCD1_S1_Train300.json'
if (-not (Test-Path -LiteralPath $train)) {
    throw 'CCD1 long training manifest missing'
}
$manifest = [IO.File]::ReadAllText($train, [Text.Encoding]::UTF8) |
    ConvertFrom-Json
$holdout = @($manifest.heldout_families)
$trainingFamilies = @($manifest.entries | ForEach-Object { $_.family } |
    Sort-Object -Unique)
if ($holdout.Count -ne 4 -or
    @($holdout | Where-Object {$trainingFamilies -contains $_}).Count -ne 0) {
    throw 'CCD1 original-source family leakage'
}
$samples = @(
    @{category='OK';name='OK-1-2025-06-27-16-54-46.png';x='42E956850A6B8C0CED5B61EF13B07721E89676703EC3B345B41C7337590A70AE';y='11F43A478AB7551D4119DBAC67E1BD31EE9FE6C4C0979C6050DC9B40D8A77C39'},
    @{category='OK';name='OK-1-2025-06-27-16-58-19.png';x='9CA7818D6E1906F7E6E9E19C102F098426EEF64F6F44D9D95409C6D7FE57F14D';y='33AC6D362A3B12044ABB10F6C0F517EF40DC5FE6D083CBCC734669F0CBA646CE'},
    @{category='NG';name='NG-1-2025-06-27-16-55-12.png';x='46C3FBDA62B31E7B04969BED7AB010B25DECFA2E4DE3E92A17D34BA6629052D6';y='A701AFC4E5177493654F4E2D351AEB981E4B0FC54BD24CA09C7556ADC50C106A'},
    @{category='NG';name='NG-1-2025-06-27-16-55-24.png';x='A33E32830E77080CC8035F0D10AB260BD5C6A60A6D440135620652AE14F28440';y='81BB8C18093CD307069D0D6490E18434052B95C928FE138AC587B7556FEED4CA'}
)
$hashTable = @{}
foreach ($entry in $samples) {
    $family = [IO.Path]::GetFileNameWithoutExtension($entry.name)
    if ($holdout -notcontains $family -or $hashTable.ContainsKey($family)) {
        throw 'Unexpected or duplicate CCD1 holdout family'
    }
    $hashTable[$family] = $true
    foreach ($axis in @('X','Y')) {
        $path = Join-Path $src ($axis+'\'+$entry.category+'\ALL\'+$entry.name)
        $target = Join-Path $dest ($axis+'\'+$entry.category+'\'+$entry.name)
        $expected = if ($axis -eq 'X') {$entry.x} else {$entry.y}
        if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or
            (Get-FileHash -LiteralPath $path).Hash -ne $expected) {
            throw ('Holdout source X/Y bytes changed: '+$path)
        }
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) |
            Out-Null
        if (-not (Test-Path -LiteralPath $target)) {
            Copy-Item -LiteralPath $path -Destination $target
        }
        if ((Get-FileHash -LiteralPath $target).Hash -ne $expected) {
            throw ('Held-out training image differs: '+$target)
        }
    }
}
$fileCount = @(Get-ChildItem -LiteralPath $dest -File -Recurse).Count
if ($fileCount -ne 8) {
    throw ('Holdout folder has unexpected extra X/Y files: '+$fileCount)
}
Write-Output 'DIA_CCD1_HOLDOUT4_STAGE=PASS SAMPLES=4 X_Y_FILES=8 LEAKAGE=False'
