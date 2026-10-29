param(
    [switch]$IncludeModelPair
)

# Creates a NEW disposable independent InspTools AppBase for one native Gate.
# Does not edit source Runtime, current release, or existing user Recipe.
# Copies NVIDIA libraries as unmodified dependencies, never decompiles them.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$SourceApp = Join-Path $Root 'output\decoded-ui\Release'
$SourceNative = Join-Path $Root 'output\native-lazy\generated-source\bin\Release\net472\InspTools.dll'
$Donor = @(Get-Item -Path 'E:\0_*\3.*\1.*\ShuntVC_Chao\Runtime\Runtime' -ErrorAction SilentlyContinue)
if ($Donor.Count -ne 1) { throw 'Expected one ShuntVC_Chao Runtime donor' }
$SrcRoot = $Donor[0].FullName
$ExpectedR3 = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
$ExpectedDelta = '3B056989D395E83B73AA2CD82EF3CB37DDA6DD8C50541DFD051636CA36DB5B38'
if ((Get-FileHash -LiteralPath $SourceNative -Algorithm SHA256).Hash -ne $ExpectedR3) {
    throw 'Pinned R3 source DLL mismatch'
}
$dir = Join-Path $Root ('output\delta-native-gates\Gate-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
if (Test-Path -LiteralPath $dir) { throw 'Gate output already exists; refusing overwrite' }
[IO.Directory]::CreateDirectory($dir) | Out-Null

$fileMap = @(
    @{src=(Join-Path $SrcRoot 'DeltaAIProcess\DeltaAIProcessDll.dll'); dst='Runtime\DeltaAIProcess\DeltaAIProcessDll.dll'; hash=$ExpectedDelta},
    @{src=(Join-Path $SrcRoot 'DeltaAIProcess\opencv_world480.dll'); dst='Runtime\DeltaAIProcess\opencv_world480.dll'; hash='10B1130AB8F0F1DBAFB8EF8D499A993D4274EF09825D2A9A34070615A3B21810'},
    @{src=(Join-Path $SrcRoot 'DeltaAIProcess\nvinfer.dll'); dst='Runtime\DeltaAIProcess\nvinfer.dll'; hash='96A5E25EF7BCF3F885863372B1EB66C879BAD845CACDD35064C7D48FA62638CA'},
    @{src=(Join-Path $SrcRoot 'CUDA11_118\cudart64_110.dll'); dst='Runtime\CUDA11_118\cudart64_110.dll'; hash='BA5C2FB526C4EE4BB218CEB3FA5E8BFDE89CE474F38711FDCCE802549BF9FC6F'}
)
if (-not (Test-Path -LiteralPath (Join-Path $SourceApp 'InspToolsRecipeCli.exe'))) {
    throw 'Standalone C# CLI not built'
}
# Copy only binaries already proven necessary for current standalone AppBase.
Get-ChildItem -LiteralPath $SourceApp -File | Where-Object {
    $_.Extension -in @('.exe','.dll','.config') -and $_.Name -ne 'InspTools.dll'
} | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $dir }
Copy-Item -LiteralPath $SourceNative -Destination (Join-Path $dir 'InspTools.dll')
foreach ($folder in @('Recipe','Model','PublicDB','TrainingImage')) {
    [IO.Directory]::CreateDirectory((Join-Path $dir $folder)) | Out-Null
}
$manifest = @()
foreach ($file in $fileMap) {
    if ((Get-FileHash -LiteralPath $file.src -Algorithm SHA256).Hash -ne $file.hash) {
        throw ('Source native dependency drift: ' + $file.src)
    }
    $destination = Join-Path $dir $file.dst
    [IO.Directory]::CreateDirectory((Split-Path -Parent $destination)) | Out-Null
    Copy-Item -LiteralPath $file.src -Destination $destination
    if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $file.hash) {
        throw ('Copied native dependency drift: ' + $destination)
    }
    $manifest += [pscustomobject]@{
        relative_path=$file.dst
        bytes=(Get-Item -LiteralPath $destination).Length
        sha256=$file.hash
    }
}
$model = $null
if ($IncludeModelPair) {
    # Deliberately do not add any model until the app-local loader Gate
    # has succeeded on its own. The model pair is a separate future Gate.
    throw 'IncludeModelPair is reserved: use model staging only after native load acceptance'
}
$state = [pscustomobject]@{
    status='STAGED_NOT_TESTED'
    app_base=$dir
    delta_source_readonly=$SrcRoot
    native_files=$manifest
    model=$model
    source_unchanged=$true
}
$state | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $dir 'delta-runtime-gate-manifest.json') -Encoding UTF8
$state | ConvertTo-Json -Depth 6
