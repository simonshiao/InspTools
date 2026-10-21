param(
    [string]$Configuration = 'Release',
    [switch]$Zip
)

# A separate, relocatable Windows x64 .NET Framework 4.7.2 WPF review build.
# Does not touch release/current and never embeds ShuntVC or DIA machine data.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath($PSScriptRoot)
$Build = Join-Path $Root ("output\decoded-ui\" + $Configuration)
$Patched = Join-Path $Root 'output\native-lazy\generated-source\bin\Release\net472\InspTools.dll'
$Review = Join-Path $Root 'output\portable-review'
$Archive = Join-Path $Root 'output\InspTools-Portable-Review-R3.zip'
$Expected = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'

foreach ($name in @('InspToolsStandalone.exe','InspToolsRecipeCli.exe','build-manifest.json')) {
    if (-not (Test-Path -LiteralPath (Join-Path $Build $name))) {
        throw ('Missing standalone build: ' + $name + '; run Build.ps1 first')
    }
}
if (-not (Test-Path -LiteralPath $Patched)) {
    throw 'Missing isolated native R3 DLL; run BuildNativeLazyInit.ps1 first'
}
if ((Get-FileHash -LiteralPath $Patched -Algorithm SHA256).Hash -ne $Expected) {
    throw 'R3 native DLL changed; inspect and update the acceptance gate first'
}
if (Test-Path -LiteralPath $Review) {
    $path = [IO.Path]::GetFullPath($Review).TrimEnd('\')
    $expectedFolder = [IO.Path]::GetFullPath((Join-Path $Root 'output\portable-review')).TrimEnd('\')
    if (-not $path.Equals($expectedFolder, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Unsafe portable build location'
    }
    # Dedicated disposable output only; not user Recipes or saved settings.
    Remove-Item -LiteralPath $Review -Recurse -Force
}
[IO.Directory]::CreateDirectory($Review) | Out-Null

# Copy the standalone EXEs and all pinned local managed/native dependencies.
# No machine runtime fallback, no source path embedded in the runtime marker.
Get-ChildItem -LiteralPath $Build -File |
    Where-Object { $_.Extension -in @('.dll', '.exe', '.config') -and $_.Name -ne 'InspTools.dll' } |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $Review }
Copy-Item -LiteralPath $Patched -Destination (Join-Path $Review 'InspTools.dll')
foreach ($folder in @('Recipe','Model','PublicDB','TrainingImage')) {
    [IO.Directory]::CreateDirectory((Join-Path $Review $folder)) | Out-Null
}
[IO.File]::WriteAllText((Join-Path $Review 'portable-review.marker'),
    'INSPTOOLS_PORTABLE_REVIEW_R3_V1', (New-Object Text.UTF8Encoding($false)))
@'
InspTools - R3 standalone review (x64 / Windows / .NET Framework 4.7.2)

Start: InspToolsStandalone.exe
Recipe: right-click inside the original ToolsWindow -> Import Recipe;
        then Load 8-station Recipe (standalone offline preview).
All Recipe / Model / PublicDB / TrainingImage paths are next to the EXE.
The package contains NO ShuntVC data and NO external machine paths.

This build verifies portable launch, original UI and native Recipe loading.
Full Inspect / AI Model / machine-camera / PLC are NOT accepted or enabled.
For now, use only copied/authorized Recipes, never production folders.
This is a test build, not the approved release/current distribution.
'@ | Set-Content -LiteralPath (Join-Path $Review 'README-REVIEW.txt') -Encoding ASCII

$published = @(Get-ChildItem -LiteralPath $Review -File)
$meta = [ordered]@{
    kind = 'portable-independent-insp-tools-review'
    version = 'R3'
    architecture = 'x64'
    framework = '.NET Framework 4.7.2'
    recipe_mode = 'real-8-station-offline-preview'
    full_inspection = 'NOT_ACCEPTED'
    machine_dependency = 'NONE'
    r3_dll_sha256 = (Get-FileHash -LiteralPath (Join-Path $Review 'InspTools.dll')).Hash
    file_count = $published.Count
    files = @($published | ForEach-Object {
        [ordered]@{ name = $_.Name; bytes = $_.Length;
                    sha256 = (Get-FileHash -LiteralPath $_.FullName).Hash }
    })
}
$meta | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $Review 'portable-manifest.json') -Encoding UTF8
if ($Zip) {
    if (Test-Path -LiteralPath $Archive) { Remove-Item -LiteralPath $Archive -Force }
    Compress-Archive -Path (Join-Path $Review '*') -DestinationPath $Archive -CompressionLevel Optimal
}
[pscustomobject]@{
    status = 'PASS'
    portable_directory = $Review
    exe = (Join-Path $Review 'InspToolsStandalone.exe')
    files = $published.Count
    mb = [Math]::Round((($published | Measure-Object Length -Sum).Sum)/1MB,2)
    zip = $(if ($Zip) { $Archive } else { $null })
    source_data_copied = $false
    official_release_untouched = $true
} | ConvertTo-Json -Depth 3
