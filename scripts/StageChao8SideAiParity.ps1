param(
    [Parameter(Mandatory=$true)][string]$Candidate,
    [string]$RecipeName = '3923-R002',
    [string]$Donor = 'D:\delta\Chao8Side'
)

# Only populate a NEW, isolated R3 portable candidate. Source assets are read-only.
# Never update an already staged model with different bytes or production folders.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$Candidate = [IO.Path]::GetFullPath($Candidate).TrimEnd('\')
$Donor = [IO.Path]::GetFullPath($Donor).TrimEnd('\')
$allowed = (Join-Path $Root 'output\standalone-r3') + '\'
if (-not $Candidate.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'AI parity staging only allowed inside project output/standalone-r3'
}
if ((Get-Content -LiteralPath (Join-Path $Candidate 'portable-review.marker') -Raw).Trim() -ne 'INSPTOOLS_PORTABLE_REVIEW_R3_V1') {
    throw 'Not a portable R3 candidate'
}
$dllhash = (Get-FileHash (Join-Path $Candidate 'InspTools.dll') -Algorithm SHA256).Hash
if ($dllhash -ne 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B') {
    throw 'Unexpected rebuilt R3 DLL'
}
if ((Get-FileHash (Join-Path $Donor 'InspTools.dll') -Algorithm SHA256).Hash -ne '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1') {
    throw 'Donor is not the audited Chao8Side 1.3.16.28'
}
if ($RecipeName -notmatch '^[A-Za-z0-9_-]+$') { throw 'Unsafe Recipe name' }
$sourceRecipe = Join-Path $Donor ('Recipe\' + $RecipeName)
if (-not (Test-Path -LiteralPath (Join-Path $sourceRecipe 'Setting.ini'))) {
    throw 'Donor Recipe missing'
}
if (-not (Test-Path -LiteralPath (Join-Path $Candidate ('Recipe\' + $RecipeName + '\Setting.ini')))) {
    throw 'Import the donor Recipe with InspToolsRecipeCli.exe before staging'
}
$files = New-Object System.Collections.Generic.List[object]
function CopyVerified([string]$src, [string]$dest) {
    if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { throw "Missing Chao8Side AI asset: $src" }
    if ((Get-Item -LiteralPath $src).Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw "Refusing donor symlink: $src"
    }
    $sh=(Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash
    if (Test-Path -LiteralPath $dest) {
        if ((Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash -ne $sh) {
            throw "Refusing to overwrite existing AI asset: $dest"
        }
    } else {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $dest)) | Out-Null
        Copy-Item -LiteralPath $src -Destination $dest
    }
    if ((Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash -ne $sh) {
        throw "AI staging integrity mismatch: $dest"
    }
    $script:files.Add([ordered]@{name=$dest.Substring($Candidate.Length + 1);sha256=$sh;bytes=(Get-Item $dest).Length})
}

# These are exact Chao8Side managed/native dependencies, NOT license files.
foreach ($name in @('cpsrt.dll','onnxruntime_providers_shared.dll')) {
    CopyVerified (Join-Path $Donor $name) (Join-Path $Candidate $name)
}
# The vendor invokes ToolsWindow.CheckAuth on Window.Loaded, before our
# Standalone has created its eight Station objects. Having the QR assembly
# in the app root at boot crashes the original CheckAuth with NullReference.
# Hold it until after NativeR3RecipeSession.Open initializes Station[].
$qr='ThoughtWorks.QRCode.dll'
if (Test-Path (Join-Path $Candidate $qr)) {
    throw 'Unsafe candidate: QR auth DLL already in app root before station initialization'
}
CopyVerified (Join-Path $Donor $qr) (Join-Path $Candidate ('AiDeferred\'+$qr))
$trainer='PModel\train\dist\AIProphet\AIProphet.exe'
CopyVerified (Join-Path $Donor $trainer) (Join-Path $Candidate $trainer)

# Only models named by this Recipe. No complete Model tree or training images.
$models=New-Object System.Collections.Generic.HashSet[string] ([StringComparer]::OrdinalIgnoreCase)
$required=New-Object System.Collections.Generic.HashSet[string] ([StringComparer]::OrdinalIgnoreCase)
$stations=@()
for ($s=1;$s -le 8;$s++) {
    $ini=Join-Path $sourceRecipe ('station' + $s + '.ini')
    if (-not (Test-Path -LiteralPath $ini)) { throw "Missing station $s" }
    $body=Get-Content -LiteralPath $ini
    $ai=$body|Where-Object{$_ -match '^\s*EnableAILocate\s*=\s*True\s*$'}|Select-Object -First 1
    $prophet=$body|Where-Object{$_ -match '^\s*AIProphetToolON\s*=\s*True\s*$'}|Select-Object -First 1
    $modelLine=$body|Where-Object{$_ -match '^\s*AILocateModelName\s*='}|Select-Object -First 1
    $model=if($modelLine){($modelLine -split '=',2)[1].Trim()}else{''}
    if ($ai) {
        if ($model -notmatch '^[a-zA-Z0-9_.-]+$') { throw "Invalid AI Locate model in S$s" }
        $baseName=[IO.Path]::GetFileNameWithoutExtension($model)
        foreach($ext in @('.lh5d','.ltrt')) {
            $name=$baseName+$ext
            [void]$required.Add($name)
            [void]$models.Add($name)
        }
    }
    # Match Chao8Side's main AIEnhance model path without importing unused models.
    if($body|Where-Object{$_ -match '^\s*AIEnhanceToolON\s*=\s*True\s*$'}|Select-Object -First 1) {
        foreach($line in $body|Where-Object{$_ -match '^\s*ModelName_A\s*='}) {
            $v=($line -split '=',2)[1].Trim()
            if ($v -match '^[a-zA-Z0-9_.-]+\.h5d$') {
                [void]$models.Add($v)
                [void]$models.Add(([IO.Path]::GetFileNameWithoutExtension($v)+'.pbd'))
            }
        }
    }
    $stations+= [ordered]@{station=$s;ai_locate=[bool]$ai;locate_model=$model;ai_prophet=[bool]$prophet}
}
foreach($name in $models) {
    $src=Join-Path (Join-Path $Donor 'Model') $name
    if (-not (Test-Path -LiteralPath $src)) {
        if ($required.Contains($name)) { throw "Required Chao8Side AI Locate model missing: $name" }
        continue
    }
    CopyVerified $src (Join-Path (Join-Path $Candidate 'Model') $name)
}
$onnx=@(Get-ChildItem -LiteralPath (Join-Path $Donor 'PModel') -File -Filter '*.onnx' -ErrorAction SilentlyContinue)
foreach($m in $onnx){CopyVerified $m.FullName (Join-Path $Candidate ('PModel\'+$m.Name))}

$manifest=[ordered]@{
    schema='InspTools.Chao8SideAiParity.v1'
    source='D:\delta\Chao8Side'
    recipe=$RecipeName
    source_dll_sha256='85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'
    status='ASSETS_STAGED_NOT_INFERENCE_VERIFIED'
    ai_prophet_onnx_count=$onnx.Count
    stations=$stations
    files=$files.ToArray()
}
$manifest|ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $Candidate 'chao8side-ai-assets.json') -Encoding UTF8
[IO.File]::WriteAllText((Join-Path $Candidate 'chao8side-ai-parity.marker'),'INSPTOOLS_CHAO8SIDE_AI_PARITY_V1')
[pscustomobject]@{status='STAGED';candidate=$Candidate;model_count=$models.Count;copied=$files.Count;prophet_onnx_count=$onnx.Count;note='AI permission, ReadModel, inference and Inspect not yet verified'}|ConvertTo-Json
