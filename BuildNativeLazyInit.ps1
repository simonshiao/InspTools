param(
    [string]$Phase1SourceRoot = '',
    [string]$Configuration = 'Release',
    [switch]$ModelPrecompiledExperiment
)

# Isolated build from read-only archived Phase-1 source. No production writes.
$ErrorActionPreference = 'Stop'
$ProjectRoot = [IO.Path]::GetFullPath($PSScriptRoot)
if ([string]::IsNullOrWhiteSpace($Phase1SourceRoot)) {
    # Wildcard discovery keeps the script ASCII-only for Windows PowerShell 5.1.
    $candidate = @(Get-Item -Path 'D:\0_*\9.*\*\ChaoMCPHub_Python_Full_20260916\ProjectHUB\Projects\InspToolsRebuild\Source\InspTools-1.3.16.28-Editable' -ErrorAction SilentlyContinue)
    if ($candidate.Count -ne 1) {
        throw ('Expected one archived phase-1 tree, got ' + $candidate.Count)
    }
    $Phase1SourceRoot = $candidate[0].FullName
}
$Root = Join-Path $ProjectRoot $(if ($ModelPrecompiledExperiment) {
    'output\delta-model-experiment'
} else {
    'output\native-lazy'
})
$Stage = Join-Path $Root 'generated-source'
$Donor = Join-Path $ProjectRoot 'release\current'
$OriginalDll = Join-Path $Donor 'InspTools.dll'
$SourceLocate = Join-Path $Phase1SourceRoot 'InspTools\AOI\LocateTool.cs'
$SourceCsproj = Join-Path $Phase1SourceRoot 'InspTools.unpacked.csproj'
$PinnedSourceSha = '64D52BC4F04340C67D6CC74F239A6BC781AA47A57261E974913614F8FE623322'
$PinnedBinarySha = '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'

if (-not (Test-Path -LiteralPath $SourceCsproj) -or
    -not (Test-Path -LiteralPath $SourceLocate) -or
    -not (Test-Path -LiteralPath $OriginalDll)) {
    throw ("Missing archived Phase-1 source or pinned R1 DLL: csproj=" +
        (Test-Path -LiteralPath $SourceCsproj) + "; locate=" +
        (Test-Path -LiteralPath $SourceLocate) + "; dll=" +
        (Test-Path -LiteralPath $OriginalDll) + "; root=" + $Phase1SourceRoot)
}
if ((Get-FileHash -LiteralPath $SourceLocate -Algorithm SHA256).Hash -ne $PinnedSourceSha) {
    throw 'Archived LocateTool.cs hash drift - STOP'
}
if ((Get-FileHash -LiteralPath $OriginalDll -Algorithm SHA256).Hash -ne $PinnedBinarySha) {
    throw 'Pinned R1 DLL hash drift - STOP'
}

# This folder contains generated source only; it must never contain Recipe/Model.
if (Test-Path -LiteralPath $Stage) { Remove-Item -LiteralPath $Stage -Recurse -Force }
[IO.Directory]::CreateDirectory($Stage) | Out-Null
& robocopy $Phase1SourceRoot $Stage /E /XD bin obj /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -gt 7) { throw ('Source staging failed: robocopy=' + $LASTEXITCODE) }

$PatchedLocate = Join-Path $Stage 'InspTools\AOI\LocateTool.cs'
$source = [IO.File]::ReadAllText($PatchedLocate, [Text.Encoding]::UTF8)
$regex = '(?m)^[ \t]*locateAIEnhanceTool\.CreatInst\(\);[ \t]*$'
$matchCount = [regex]::Matches($source, $regex).Count
if ($matchCount -ne 1) {
    throw ('SetBuffers native eager call mismatch: expected 1, got ' + $matchCount)
}
$patched = [regex]::Replace($source, $regex,
    '        // R3: native AI instance is created on demand by ReadModel().')
[IO.File]::WriteAllText($PatchedLocate, $patched, (New-Object Text.UTF8Encoding($false)))

if ($ModelPrecompiledExperiment) {
    # Narrow *experimental* change to the vendor C# class only. The original
    # R3 compiled DLL remains intact and hash pinned. For an already prepared
    # .h5d + .pbd pair, no training/conversion executables should be needed.
    $sourceFile = Join-Path $Stage 'InspTools\AOI\AIEnhanceTool.cs'
    $aiSource = [IO.File]::ReadAllText($sourceFile, [Text.Encoding]::UTF8)
    $old = 'if (!File.Exists(text) || !File.Exists(text2))'
    $n = [regex]::Matches($aiSource, [regex]::Escape($old)).Count
    if ($n -ne 1) {
        throw ('AIEnhance old Trainer gate changed; expected 1, got ' + $n)
    }
    $new = @'
string preparedStem = Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A);
            string preparedSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", preparedStem + ".h5d");
            string preparedPbd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", preparedStem + ".pbd");
            bool preparedModel = !param.AIEnhanceParam.UseTensorRT
                && File.Exists(preparedSource) && new FileInfo(preparedSource).Length >= 4096
                && File.Exists(preparedPbd) && new FileInfo(preparedPbd).Length >= 4096;
            if ((!File.Exists(text) || !File.Exists(text2)) && !preparedModel)
'@
    $aiPatched = $aiSource.Replace($old, $new)
    [IO.File]::WriteAllText($sourceFile, $aiPatched,
        (New-Object Text.UTF8Encoding($false)))
}

# The archived sources omit the original packed WPF container.
# Extract the container from hash-pinned R1 DLL into generated build staging.
$assembly = [Reflection.Assembly]::LoadFrom($OriginalDll)
$stream = $assembly.GetManifestResourceStream('InspTools.g.resources')
if ($null -eq $stream) { throw 'Pinned R1 DLL lacks InspTools.g.resources' }
$resourceFile = Join-Path $Stage 'InspTools.g.resources'
try {
    $destination = [IO.File]::Open($resourceFile, [IO.FileMode]::CreateNew)
    try { $stream.CopyTo($destination) } finally { $destination.Dispose() }
} finally { $stream.Dispose() }

$project = Join-Path $Stage 'InspTools.unpacked.csproj'
$arguments = @('build', $project, '-c', $Configuration, '--nologo',
    ('/p:TemplateRuntimeRoot=' + $Donor),
    '/p:GenerateAssemblyInfo=false', '--ignore-failed-sources')
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw 'Isolated R3 native source build failed; R1 DLL untouched' }

$compiledDll = Join-Path $Stage ('bin\' + $Configuration + '\net472\InspTools.dll')
if (-not (Test-Path -LiteralPath $compiledDll)) { throw 'Compiled InspTools.dll missing' }
[pscustomobject]@{
    status = 'PASS'
    kind = $(if ($ModelPrecompiledExperiment) { 'isolated-delta-precompiled-model-experiment' }
             else { 'isolated-R3-lazy-init' })
    donor_read_only = $Phase1SourceRoot
    generated_dll = $compiledDll
    generated_sha256 = (Get-FileHash -LiteralPath $compiledDll -Algorithm SHA256).Hash
    baseline_sha256 = $PinnedBinarySha
    phase1_source_sha256 = $PinnedSourceSha
    production_untouched = $true
} | ConvertTo-Json -Depth 4
