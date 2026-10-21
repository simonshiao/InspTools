param(
    [switch]$Rebuild,
    [switch]$Zip,
    [string]$Configuration = 'Release'
)

# Create a SELF-CONTAINED CANDIDATE from the audited R3 DLL and our standalone
# launcher. Do not change release/current or any machine Shunt/DIA Runtime.
# Never overwrite a previous candidate (which may contain user Recipe edits).
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath($PSScriptRoot)
$App = Join-Path $Root ('output\decoded-ui\' + $Configuration)
$Native = Join-Path $Root ('output\native-lazy\generated-source\bin\' +
    $Configuration + '\net472\InspTools.dll')
$NativeSha = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
$OriginalSha = '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'
$Marker = 'INSPTOOLS_PORTABLE_REVIEW_R3_V1'

if ($Rebuild) {
    & (Join-Path $Root 'Build.ps1') -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'Standalone launcher build failed' }
    & (Join-Path $Root 'BuildNativeLazyInit.ps1') -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'R3 native DLL build failed' }
}
if (-not (Test-Path -LiteralPath (Join-Path $App 'InspToolsStandalone.exe')) -or
    -not (Test-Path -LiteralPath $Native)) {
    throw 'Run Build.ps1 and BuildNativeLazyInit.ps1 before packaging'
}
if ((Get-FileHash -LiteralPath $Native -Algorithm SHA256).Hash -ne $NativeSha) {
    throw 'R3 patched DLL checksum mismatch: refusing candidate'
}
if ((Get-FileHash -LiteralPath (Join-Path $App 'InspTools.dll') -Algorithm SHA256).Hash -ne $OriginalSha) {
    throw 'Launcher source donor must remain the pinned R1, no silent replacement'
}

$OutputRoot = Join-Path $Root 'output\standalone-r3'
[IO.Directory]::CreateDirectory($OutputRoot) | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$candidate = Join-Path $OutputRoot ('InspTools-Portable-R3-' + $stamp)
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
