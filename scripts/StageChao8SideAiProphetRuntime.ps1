param(
    [Parameter(Mandatory=$true)][string]$Candidate,
    [string]$SourceRoot = 'E:\0_軟體開發區\1.成品區\Chao8Side'
)

# Stage the user's own packaged Chao8Side AI Prophet into a NEW isolated R3
# candidate. The product tree is read-only evidence and is never modified.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$Candidate = [IO.Path]::GetFullPath($Candidate).TrimEnd('\')
$SourceRoot = [IO.Path]::GetFullPath($SourceRoot).TrimEnd('\')
$allowed = [IO.Path]::GetFullPath((Join-Path $Root 'output\standalone-r3')).TrimEnd('\') + '\'
if (-not $Candidate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'AI Prophet staging is only allowed inside output/standalone-r3'
}
if (-not (Test-Path -LiteralPath $Candidate -PathType Container)) {
    throw "Candidate directory does not exist: $Candidate"
}
$marker = Join-Path $Candidate 'portable-review.marker'
if (-not (Test-Path -LiteralPath $marker -PathType Leaf) -or
    (Get-Content -LiteralPath $marker -Raw).Trim() -ne 'INSPTOOLS_PORTABLE_REVIEW_R3_V1') {
    throw 'Not an isolated portable R3 candidate'
}
if ((Get-FileHash -LiteralPath (Join-Path $Candidate 'InspTools.dll') -Algorithm SHA256).Hash -ne
    'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B') {
    throw 'Unexpected rebuilt R3 DLL'
}
if ((Get-FileHash -LiteralPath (Join-Path $SourceRoot 'InspTools.dll') -Algorithm SHA256).Hash -ne
    '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1') {
    throw 'SourceRoot is not the audited Chao8Side InspTools build'
}

$sourceManifestPath = Join-Path $SourceRoot 'Runtime\AIProphet\trainer_manifest.json'
$controlManifestPath = Join-Path $SourceRoot 'Runtime\AIRuntime\runtime.json'
$trainerManifest = Get-Content -LiteralPath $sourceManifestPath -Raw | ConvertFrom-Json
$controlManifest = Get-Content -LiteralPath $controlManifestPath -Raw | ConvertFrom-Json
if ($trainerManifest.trainer -ne 'Runtime\AIProphet\Trainer\AIProphet.exe' -or
    $controlManifest.profiles.PROPHET.trainer -ne 'Runtime\AIProphet\Trainer\AIProphet.exe' -or
    $controlManifest.profiles.PROPHET.legacy_entry -ne 'PModel\train\dist\AIProphet\AIProphet.exe') {
    throw 'Chao8Side AI Prophet runtime contract changed; refusing to stage'
}

function CopyVerifiedFile([string]$relative) {
    $src = Join-Path $SourceRoot $relative
    $dst = Join-Path $Candidate $relative
    if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { throw "Missing source file: $src" }
    $sourceHash = (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash
    if (Test-Path -LiteralPath $dst) {
        if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $sourceHash) {
            throw "Refusing to overwrite different candidate file: $dst"
        }
    } else {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $dst)) | Out-Null
        Copy-Item -LiteralPath $src -Destination $dst
    }
    if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $sourceHash) {
        throw "Staging hash mismatch: $dst"
    }
    return [ordered]@{path=$relative;bytes=(Get-Item -LiteralPath $dst).Length;sha256=$sourceHash}
}

function CopyVerifiedDirectory([string]$relative) {
    $src = Join-Path $SourceRoot $relative
    $dst = Join-Path $Candidate $relative
    if (-not (Test-Path -LiteralPath $src -PathType Container)) { throw "Missing source directory: $src" }
    if (Test-Path -LiteralPath $dst) { throw "Refusing to reuse existing directory: $dst" }
    $reparse = @(Get-ChildItem -LiteralPath $src -Recurse -Force -ErrorAction Stop |
        Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint })
    if ($reparse.Count -gt 0) { throw "Refusing source directory with reparse points: $src" }
    $sourceFiles = @(Get-ChildItem -LiteralPath $src -Recurse -File -ErrorAction Stop)
    $sourceBytes = [int64](($sourceFiles | Measure-Object Length -Sum).Sum)
    [IO.Directory]::CreateDirectory((Split-Path -Parent $dst)) | Out-Null
    Copy-Item -LiteralPath $src -Destination $dst -Recurse
    $targetFiles = @(Get-ChildItem -LiteralPath $dst -Recurse -File -ErrorAction Stop)
    $targetBytes = [int64](($targetFiles | Measure-Object Length -Sum).Sum)
    if ($targetFiles.Count -ne $sourceFiles.Count -or $targetBytes -ne $sourceBytes) {
        throw "Staged directory inventory mismatch: $relative"
    }
    foreach ($file in @($sourceFiles | Where-Object { $_.Name -in @('AIProphet.exe','trainer_manifest.json','runtime.json','ChaoAIRuntime.exe.config') })) {
        $rel = $file.FullName.Substring($src.Length + 1)
        $target = Join-Path $dst $rel
        if ((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash) {
            throw "Staged critical file hash mismatch: $relative\$rel"
        }
    }
    return [ordered]@{path=$relative;files=$targetFiles.Count;bytes=$targetBytes}
}

$staged = New-Object System.Collections.Generic.List[object]
# Compatibility entry the original InspTools GUI probes before it shows the
# Prophet controls. This is only the small original shim, not a replacement UI.
$staged.Add((CopyVerifiedFile 'PModel\train\dist\AIProphet\AIProphet.exe'))
$staged.Add((CopyVerifiedDirectory 'PModel\train\AIProphetRebuild'))
$staged.Add((CopyVerifiedFile 'PModel\train\weights\backbone.pth'))
$staged.Add((CopyVerifiedDirectory 'Runtime\AIRuntime'))
$staged.Add((CopyVerifiedDirectory 'Runtime\AIProphet'))

$onnx = @(Get-ChildItem -LiteralPath (Join-Path $SourceRoot 'PModel') -Recurse -File -Filter '*.onnx' -ErrorAction SilentlyContinue)
$manifest = [ordered]@{
    schema = 'InspTools.Chao8SideAiProphetRuntime.v1'
    source = $SourceRoot
    source_inspTools_sha256 = '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'
    candidate_r3_sha256 = 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B'
    status = 'RUNTIME_STAGED_MODEL_NOT_PRESENT'
    runtime_trainer = 'Runtime\AIProphet\Trainer\AIProphet.exe'
    gui_compatibility_entry = 'PModel\train\dist\AIProphet\AIProphet.exe'
    working_directory = 'PModel\train'
    model_count = $onnx.Count
    trainer_manifest = $trainerManifest
    runtime_manifest = $controlManifest
    staged = $staged.ToArray()
}
$manifest | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath (Join-Path $Candidate 'chao8side-ai-prophet-runtime.json') -Encoding UTF8
[IO.File]::WriteAllText((Join-Path $Candidate 'chao8side-ai-prophet-ui.marker'),
    'INSPTOOLS_CHAO8SIDE_AI_PROPHET_UI_V1', [Text.Encoding]::ASCII)
[int64]$runtimeBytes = ($staged.ToArray() | ForEach-Object { [int64]$_.bytes } | Measure-Object -Sum).Sum
[pscustomobject]@{
    status = 'STAGED'
    candidate = $Candidate
    runtime_bytes = $runtimeBytes
    model_count = $onnx.Count
    inference_ready = ($onnx.Count -gt 0)
} | ConvertTo-Json
