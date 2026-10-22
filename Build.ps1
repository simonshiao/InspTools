param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath($PSScriptRoot)
$Out = Join-Path $Root ("output\decoded-ui\" + $Configuration)
$Phase1Parent = Get-ChildItem -LiteralPath 'D:\delta' -Directory | Where-Object {
    Test-Path -LiteralPath (Join-Path $_.FullName 'Projects\InspToolsRebuild')
} | Select-Object -First 1 -ExpandProperty FullName
$Phase1Dll = $null
$SourceProvenance = 'phase1_recovered_build'
if (-not [string]::IsNullOrWhiteSpace($Phase1Parent)) {
    $Phase1Root = Join-Path $Phase1Parent 'Projects\InspToolsRebuild'
    $candidate = Join-Path $Phase1Root 'Source\InspTools-1.3.16.28-Editable\bin\Release\net472\InspTools.dll'
    if (Test-Path -LiteralPath $candidate) { $Phase1Dll = $candidate }
}
if (-not $Phase1Dll) {
    # Use the already pinned R1 *binary*, never an arbitrary DIA/Shunt DLL.
    # This restores reproducible builds after the old Phase-1 source moved;
    # it does NOT claim that the recovered DLL source can be rebuilt yet.
    $candidate = Join-Path $Root 'release\current\InspTools.dll'
    if (Test-Path -LiteralPath $candidate) {
        $Phase1Dll = $candidate
        $SourceProvenance = 'pinned_r1_release_binary_fallback'
    }
}
# R1 already contains the audited dependency set. Use this known baseline,
# never silently take dependencies from a retired DIA machine template.
$DependencyDonor = Join-Path $Root 'release\current'

# Current recovered UI snapshot.  This is intentionally NOT labelled as the
# historical accepted 0B4E... binary; that artifact is no longer present in the
# phase-1 project tree.  Pinning prevents silent post-closeout source drift.
$PinnedRecoveredUiSha256 = '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'

if (-not (Test-Path -LiteralPath $Phase1Dll)) {
    throw "Phase-1 decoded/rebuilt InspTools.dll missing: $Phase1Dll"
}
$actualPhase1Hash = (Get-FileHash -LiteralPath $Phase1Dll -Algorithm SHA256).Hash
if ($actualPhase1Hash -ne $PinnedRecoveredUiSha256) {
    throw "Phase-1 recovered UI hash changed. Expected $PinnedRecoveredUiSha256, got $actualPhase1Hash. Re-audit before building."
}

if (Test-Path -LiteralPath $Out) {
    # Keep independent user-owned Recipe/Model/data across rebuilds.
    # Only compiled top-level files are replaced. Never delete recipe edits.
    Get-ChildItem -LiteralPath $Out -File -Force | Remove-Item -Force
}
[IO.Directory]::CreateDirectory($Out) | Out-Null

function Find-Net472Refs {
    $candidates = New-Object System.Collections.Generic.List[string]
    $candidates.Add('C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2')
    foreach ($user in Get-ChildItem -LiteralPath 'C:\Users' -Directory -ErrorAction SilentlyContinue) {
        $candidates.Add((Join-Path $user.FullName '.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.3\build\.NETFramework\v4.7.2'))
    }
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath (Join-Path $candidate 'mscorlib.dll')) { return $candidate }
    }
    throw 'No .NET Framework 4.7.2 reference assemblies found.'
}

$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$sdkLine = dotnet --list-sdks | Select-Object -Last 1
$sdkVersion = ($sdkLine -split ' ')[0]
$csc = Join-Path (Split-Path -Parent $dotnet) ("sdk\" + $sdkVersion + "\Roslyn\bincore\csc.dll")
$RefRoot = Find-Net472Refs
$exe = Join-Path $Out 'InspToolsStandalone.exe'

$compileArgs = @(
    $csc,
    '/nologo',
    '/nostdlib+',
    '/platform:x64',
    '/langversion:latest',
    '/target:winexe',
    ('/out:' + $exe)
)
foreach ($name in @(
    'mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll',
    'WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll'
)) {
    $compileArgs += ('/reference:' + (Join-Path $RefRoot $name))
}
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneEntry.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\DeviceTwinPorts.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\TrainingContracts.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\GeneralTrainingJob.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\GeneralCandidateRunner.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\GeneralTrainerExecutor.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\TrainingRuntimePaths.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\DeltaNativeRuntimeBootstrap.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\DecodedToolsWindowHost.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneRecipeStore.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneRecipeCli.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneRecipeUi.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\BlankUiLifecycleGuard.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\NativeRecipeProbe.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\LightweightRecipePreview.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneUiSmoke.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\NativeRuntimeGate.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\R3NativeGuiSmoke.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\NativeR3RecipeSession.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\R3OriginalDialogSmoke.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\R3SaveAllSmoke.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\R3ShuntImageSmoke.cs')
$compileArgs += (Join-Path $Root 'src\InspTools.Standalone\R3IsolatedEditor.cs')

& $dotnet @compileArgs
if ($LASTEXITCODE -ne 0) { throw 'InspToolsStandalone compile failed.' }

# Recipe commands must also work without starting the WPF process.
$cliArgs = @(
    $csc,
    '/nologo',
    '/nostdlib+',
    '/platform:x64',
    '/langversion:latest',
    '/target:exe',
    ('/out:' + (Join-Path $Out 'InspToolsRecipeCli.exe'))
)
foreach ($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll',
                    'WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll')) {
    $cliArgs += ('/reference:' + (Join-Path $RefRoot $name))
}
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneRecipeStore.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\StandaloneRecipeCli.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\RecipeCliEntry.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\DeviceTwinPorts.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\TrainingContracts.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\GeneralTrainingJob.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\GeneralCandidateRunner.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\GeneralTrainerExecutor.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\TrainingRuntimePaths.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\DeltaNativeRuntimeBootstrap.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\NativeRecipeProbe.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\CvDiagnosticProbe.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\R3FacadeProbe.cs')
$cliArgs += (Join-Path $Root 'src\InspTools.Standalone\NativeRuntimeGate.cs')
& $dotnet @cliArgs
if ($LASTEXITCODE -ne 0) { throw 'InspToolsRecipeCli compile failed.' }

Copy-Item -LiteralPath $Phase1Dll -Destination (Join-Path $Out 'InspTools.dll') -Force

# R1 dependency set: only what the decoded ToolsWindow currently needs to
# construct/show.  No Training tree, CUDA runtime, camera/PLC stack or Agent bits.
$dependencies = @(
    'ControlzEx.dll',
    'MahApps.Metro.dll',
    'MahApps.Metro.IconPacks.dll',
    'MahApps.Metro.IconPacks.Material.dll',
    'MahApps.Metro.IconPacks.FontAwesome.dll',
    'MahApps.Metro.IconPacks.SimpleIcons.dll',
    'System.Windows.Interactivity.dll',
    'UICommon.dll',
    'INIFileParser.dll',
    'OpenCvSharp.dll',
    'OpenCvSharp.Extensions.dll',
    'OpenCvSharpExtern.dll',
    'Microsoft.ML.OnnxRuntime.dll',
    'onnxruntime.dll',
    'LiveCharts.dll',
    'LiveCharts.Wpf.dll',
    'HiroPChart.dll',
    'System.Memory.dll',
    'System.Runtime.CompilerServices.Unsafe.dll',
    'OnnxRuntimeEnv.dll',
    'DeltaAIProcessDll.dll',
    'TRT.dll'
)

foreach ($name in $dependencies) {
    $source = Join-Path $DependencyDonor $name
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Decoded UI dependency missing from donor: $source"
    }
    Copy-Item -LiteralPath $source -Destination (Join-Path $Out $name) -Force
}

# App-owned OpenCV snapshot.  Keep root DLLs for legacy .NET/native loading.
$opencvRuntime = Join-Path $Out 'Runtime\OpenCV'
[IO.Directory]::CreateDirectory($opencvRuntime) | Out-Null
foreach ($name in @('OpenCvSharp.dll', 'OpenCvSharp.Extensions.dll', 'OpenCvSharpExtern.dll')) {
    $source = Join-Path $Out $name
    $target = Join-Path $opencvRuntime $name
    if (Test-Path -LiteralPath $target) {
        if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash) {
            throw "App-local OpenCV runtime differs: $name"
        }
    } else {
        Copy-Item -LiteralPath $source -Destination $target
    }
}

foreach ($folder in @('Recipe','Model','PublicDB','TrainingImage')) {
    [IO.Directory]::CreateDirectory((Join-Path $Out $folder)) | Out-Null
}

$files = Get-ChildItem -LiteralPath $Out -File | Sort-Object Name
$manifest = [ordered]@{
    schema = 'InspTools.Rebuild2.DecodedUiBuild.v1'
    route = 'phase1-compiled-toolswindow-standalone'
    target = 'net472-x64-wpf'
    phase1_source = $Phase1Dll
    source_provenance = $SourceProvenance
    phase1_recovered_ui_sha256 = $actualPhase1Hash
    historical_accepted_sha256 = '0B4E8C671779B8F33290F6101C5D69105D3182C60BCE30D8B544725183A9F1F2'
    historical_accepted_binary_present = $false
    standalone_sha256 = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash
    files = @($files | ForEach-Object {
        [ordered]@{
            name = $_.Name
            bytes = $_.Length
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    })
    physical_bytes = ($files | Measure-Object Length -Sum).Sum
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $Out 'build-manifest.json') -Encoding UTF8

[pscustomobject]@{
    status = 'PASS'
    route = $manifest.route
    output = $Out
    standalone = $exe
    standalone_sha256 = $manifest.standalone_sha256
    phase1_ui_sha256 = $actualPhase1Hash
    physical_mb = [Math]::Round($manifest.physical_bytes / 1MB, 2)
    formal_projects = 1
    off_route_projects_built = 0
} | ConvertTo-Json -Depth 4
