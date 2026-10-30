param(
    [string]$BuildOutput = '',
    [string]$R3Donor = '',
    [int]$Version = 11
)
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if ([string]::IsNullOrWhiteSpace($BuildOutput)) {
    $BuildOutput = Join-Path $Root 'output\ai-training-build-20261030'
}
if ([string]::IsNullOrWhiteSpace($R3Donor)) {
    $R3Donor = Join-Path $Root 'output\standalone-r3\InspTools-Portable-R3-20261030-AIProphet-OriginalRuntime'
}
$BuildOutput = [IO.Path]::GetFullPath($BuildOutput)
$R3Donor = [IO.Path]::GetFullPath($R3Donor)
$OutputRoot = Join-Path $Root 'output'
$Candidate = Join-Path $OutputRoot ('InspToolsV' + $Version)
$ExpectedR3Sha = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
if (-not $BuildOutput.StartsWith($OutputRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase) -or
    -not $R3Donor.StartsWith($OutputRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Build and R3 donor must stay under this project output directory.'
}
if (Test-Path -LiteralPath $Candidate) { throw "Candidate already exists: $Candidate" }
if (-not (Test-Path (Join-Path $BuildOutput 'InspToolsStandalone.exe')) -or
    -not (Test-Path (Join-Path $BuildOutput 'InspTools.dll'))) {
    throw 'Standalone build output is incomplete.'
}
$donorDll = Join-Path $R3Donor 'InspTools.dll'
if (-not (Test-Path -LiteralPath $donorDll) -or
    (Get-FileHash -LiteralPath $donorDll -Algorithm SHA256).Hash -ne $ExpectedR3Sha) {
    throw 'R3 donor is not the previously verified portable candidate DLL.'
}
$created = $false
try {
    New-Item -ItemType Directory -Path $Candidate | Out-Null
    $created = $true
    foreach ($file in Get-ChildItem -LiteralPath $BuildOutput -File -Force |
        Where-Object { $_.Extension -in @('.dll','.exe') -and $_.Name -ne 'InspTools.dll' }) {
        Copy-Item -LiteralPath $file.FullName -Destination $Candidate
    }
    Copy-Item -LiteralPath $donorDll -Destination (Join-Path $Candidate 'InspTools.dll')
    Copy-Item -LiteralPath (Join-Path $Root 'docs\PORTABLE_R3_CURRENT.md') `
        -Destination (Join-Path $Candidate 'README.md')
    foreach ($folder in @('Recipe','Model','PublicDB','TrainingImage','TrainingSeed')) {
        New-Item -ItemType Directory -Path (Join-Path $Candidate $folder) | Out-Null
    }
    Set-Content -LiteralPath (Join-Path $Candidate 'portable-review.marker') `
        -Value 'INSPTOOLS_PORTABLE_REVIEW_R3_V1' -Encoding Ascii
    Set-Content -LiteralPath (Join-Path $Candidate 'chao8side-ai-training-ui.marker') `
        -Value 'INSPTOOLS_CHAO8SIDE_AI_TRAINING_UI_V1' -Encoding Ascii
    $files = @(Get-ChildItem -LiteralPath $Candidate -File -Force |
        Where-Object { $_.Extension -in @('.dll','.exe') } | ForEach-Object {
            [ordered]@{ name=$_.Name; size=$_.Length;
                sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
        })
    $manifest = [ordered]@{
        schema='InspTools.Standalone.TrainingReviewCandidate.v1'
        status='CANDIDATE_NOT_PRODUCTION'
        version=('InspToolsV' + $Version)
        entry='InspToolsStandalone.exe'
        native_dll_sha256=$ExpectedR3Sha
        training_ui='Chao8Side RebuiltTrainingWindow'
        training_runtime='PENDING_STAGE'
        model_deployment='candidate AppBase only'
        files=$files
    }
    $manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath `
        (Join-Path $Candidate 'standalone-manifest.json') -Encoding UTF8
    [pscustomobject]@{status='PASS';path=$Candidate;binary_count=$files.Count;
        native_sha256=$ExpectedR3Sha} | ConvertTo-Json -Depth 3
}
catch {
    if ($created -and (Test-Path -LiteralPath $Candidate)) {
        Remove-Item -LiteralPath $Candidate -Recurse -Force
    }
    throw
}
