param(
    [switch]$Rebuild,
    [switch]$Zip,
    [switch]$Chao3LocalR3,
    [int]$Version = 0,
    [string]$Configuration = 'Release',
    [string]$LauncherDirectory = ''
)

# Create a SELF-CONTAINED CANDIDATE from the audited R3 DLL and our standalone
# launcher. Do not change release/current or any machine Shunt/DIA Runtime.
# Never overwrite a previous candidate (which may contain user Recipe edits).
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$App = if ($LauncherDirectory) { [IO.Path]::GetFullPath($LauncherDirectory) } else {
    Join-Path $Root ('output\decoded-ui\' + $Configuration)
}
if (-not $App.StartsWith((Join-Path $Root 'output') + [IO.Path]::DirectorySeparatorChar,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw 'LauncherDirectory must stay under this project output directory'
}
$Native = Join-Path $Root ('output\native-lazy\generated-source\bin\' +
    $Configuration + '\net472\InspTools.dll')
$NativeSha = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
$LocalR3Sha = 'BC56633CD5DD37FAF4274181D9A0100B5A99EAB0D48BDBFDAFDABDFEE71C0223'
$SelectedSha = if ($Chao3LocalR3) { $LocalR3Sha } else { $NativeSha }
$OriginalSha = '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'
$Marker = 'INSPTOOLS_PORTABLE_REVIEW_R3_V1'

if ($Rebuild) {
    & (Join-Path $Root 'scripts\Build.ps1') -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'Standalone launcher build failed' }
    & (Join-Path $Root 'scripts\BuildNativeLazyInit.ps1') -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'R3 native DLL build failed' }
}
if (-not (Test-Path -LiteralPath (Join-Path $App 'InspToolsStandalone.exe')) -or
    -not (Test-Path -LiteralPath $Native)) {
    throw 'Run Build.ps1 and BuildNativeLazyInit.ps1 before packaging'
}
if ((Get-FileHash -LiteralPath $Native -Algorithm SHA256).Hash -ne $SelectedSha) {
    throw 'R3 patched DLL checksum mismatch: refusing candidate'
}
if ((Get-FileHash -LiteralPath (Join-Path $App 'InspTools.dll') -Algorithm SHA256).Hash -ne $OriginalSha) {
    throw 'Launcher source donor must remain the pinned R1, no silent replacement'
}

$OutputRoot = Join-Path $Root 'output'
[IO.Directory]::CreateDirectory($OutputRoot) | Out-Null
if ($Version -lt 0) { throw 'Version must be >= 1 (or 0 for auto next)' }
if ($Version -eq 0) {
    $previous = @(Get-ChildItem -LiteralPath $OutputRoot -Directory |
        Where-Object { $_.Name -match '^InspToolsV[1-9][0-9]*$' } |
        ForEach-Object { [int]($_.Name.Substring('InspToolsV'.Length)) })
    $Version = if ($previous.Count -gt 0) {
        [int](($previous | Measure-Object -Maximum).Maximum) + 1
    } else { 1 }
}
$candidate = Join-Path $OutputRoot ('InspToolsV' + $Version)
if (Test-Path -LiteralPath $candidate) { throw 'Package folder already exists; refusing overwrite' }
[IO.Directory]::CreateDirectory($candidate) | Out-Null

try {
    # Binary runtime is shipped with its own app. At execution time neither
    # ShuntVC nor Chao8Side nor ProjectHub.Native needs to be present.
    $allowedExtensions = @('.dll', '.exe')
    foreach ($file in Get-ChildItem -LiteralPath $App -File) {
        if ($file.Extension -notin $allowedExtensions -or
            $file.Name -eq 'InspTools.dll') { continue }
        Copy-Item -LiteralPath $file.FullName -Destination $candidate
    }
    Copy-Item -LiteralPath $Native -Destination (Join-Path $candidate 'InspTools.dll')
    foreach ($folder in @('Recipe','Model','PublicDB','TrainingImage')) {
        [IO.Directory]::CreateDirectory((Join-Path $candidate $folder)) | Out-Null
    }
    [IO.File]::WriteAllText((Join-Path $candidate 'portable-review.marker'),
        $Marker + [Environment]::NewLine, [Text.Encoding]::ASCII)
    if ($Chao3LocalR3) {
        [IO.File]::WriteAllText(
            (Join-Path $candidate 'portable-chao3-local-r3.marker'),
            'INSPTOOLS_CHAO3_LOCAL_R3_SHA_V1' + [Environment]::NewLine,
            [Text.Encoding]::ASCII)
    }
    $instructions = Join-Path $Root 'docs\PORTABLE_R3_CURRENT.md'
    if (-not (Test-Path -LiteralPath $instructions)) {
        throw 'Portable R3 user-facing instructions are missing'
    }
    Copy-Item -LiteralPath $instructions -Destination (Join-Path $candidate 'README.md')

    $binFiles = @(Get-ChildItem -LiteralPath $candidate -File |
        Where-Object { $_.Extension -in $allowedExtensions })
    $manifest = [ordered]@{
        schema = 'InspTools.Standalone.R3.PortableCandidate.v1'
        status = 'CANDIDATE_NOT_PRODUCTION'
        version = 'InspToolsV' + $Version
        packaged_at = (Get-Date).ToString('yyyy-MM-ddTHH:mm:sszzz')
        local_chao3_rebuild = [bool]$Chao3LocalR3
        local_sha_not_chao2_audited = [bool]$Chao3LocalR3
        entry = 'InspToolsStandalone.exe'
        architecture = 'net472-x64-wpf'
        native_dll_sha256 = (Get-FileHash -LiteralPath (Join-Path $candidate 'InspTools.dll')).Hash
        build_input = 'audited-standalone-launcher-and-pinned-R3-core'
        standalone_recipe_folder = 'Recipe'
        model_runtime_status = 'NOT_ACCEPTED'
        inspect_classification_status = 'NOT_ACCEPTED'
        depends_on_shunt_or_chao8side_at_launch = $false
        files = @($binFiles | ForEach-Object {
            [ordered]@{name=$_.Name; size=$_.Length;
                sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}
        })
    }
    $manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (
        Join-Path $candidate 'standalone-manifest.json') -Encoding UTF8
    $archivePath = $null
    if ($Zip) {
        $archivePath = $candidate + '.zip'
        if (Test-Path -LiteralPath $archivePath) {
            throw 'Unique portable ZIP destination already exists'
        }
        Compress-Archive -Path (Join-Path $candidate '*') -DestinationPath $archivePath -CompressionLevel Optimal
        if (-not (Test-Path -LiteralPath $archivePath)) {
            throw 'Portable ZIP archive was not created'
        }
    }
    [pscustomobject]@{
        status='PASS'
        kind='portable-r3-candidate'
        version='InspToolsV' + $Version
        path=$candidate
        native_sha256=$manifest.native_dll_sha256
        binary_mb=[math]::Round(($binFiles|Measure-Object Length -Sum).Sum/1MB,2)
        binaries=$binFiles.Count
        recipe_count=0
        zip=$archivePath
        zip_sha256=$(if ($archivePath) {
            (Get-FileHash -LiteralPath $archivePath).Hash
        } else { $null })
        production_untouched=$true
    } | ConvertTo-Json -Depth 4
}
catch {
    # Only this newly generated candidate is disposable. NEVER remove an
    # existing candidate or any source Recipe/Model/release folder.
    if (Test-Path -LiteralPath $candidate) {
        Remove-Item -LiteralPath $candidate -Recurse -Force
    }
    throw
}
