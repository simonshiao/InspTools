param(
    [int]$ComponentTimeoutMs = 5000,
    [int]$ConstructorTimeoutMs = 15000,
    [int]$GuiTimeoutMs = 15000,
    [switch]$StageOnly
)

# This script stages an isolated AppBase and runs bounded child processes.
# It does NOT update release/current, Phase-1 source or production Recipe.
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$Dev = Join-Path $Root 'output\decoded-ui\Release'
$Native = Join-Path $Root 'output\native-lazy\generated-source\bin\Release\net472\InspTools.dll'
$Sandbox = Join-Path $Root 'output\native-lazy\runtime'
$PinnedBaseline = '85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1'

if (-not (Test-Path -LiteralPath $Native) -or
    -not (Test-Path -LiteralPath $Dev)) { throw 'Run BuildNativeLazyInit.ps1 and Build.ps1 first' }
if ((Get-FileHash -LiteralPath $Native -Algorithm SHA256).Hash -eq $PinnedBaseline) {
    throw 'Expected a patched, different DLL; refusing baseline-test ambiguity'
}
if (Test-Path -LiteralPath $Sandbox) { Remove-Item -LiteralPath $Sandbox -Recurse -Force }
[IO.Directory]::CreateDirectory($Sandbox) | Out-Null
Get-ChildItem -LiteralPath $Dev -File | Where-Object {
    $_.Extension -in @('.dll','.exe','.json') -and $_.Name -ne 'InspTools.dll'
} | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $Sandbox
}
Copy-Item -LiteralPath $Native -Destination (Join-Path $Sandbox 'InspTools.dll')
$RecipeName = 'R2PROBE-3923-R002'
$RecipeSource = Join-Path $Dev ('Recipe\' + $RecipeName)
if (-not (Test-Path -LiteralPath $RecipeSource)) { throw 'R2 isolated Recipe not found' }
$RecipeRoot = Join-Path $Sandbox 'Recipe'
[IO.Directory]::CreateDirectory($RecipeRoot) | Out-Null
Copy-Item -LiteralPath $RecipeSource -Destination $RecipeRoot -Recurse
[IO.Directory]::CreateDirectory((Join-Path $Sandbox 'Model')) | Out-Null

# Stage a tiny, read-only selection of the Shunt production Recipe + one
# original CCD1 OK and NG image. Never duplicate the full production Runtime.
& (Join-Path $Root 'scripts\StageShuntFixtures.ps1') -Sandbox $Sandbox | Out-Null
if ($StageOnly) {
    Write-Output 'R3_STAGE_ONLY=PASS (no native/UI regression executed)'
    return
}

function Run-BoundedProbe([string]$Label, [string[]]$Arguments, [int]$LimitMs, [string]$ExpectedMarker) {
    $std = Join-Path $Sandbox ($Label + '.out')
    $err = Join-Path $Sandbox ($Label + '.err')
    $app = Join-Path $Sandbox 'InspToolsRecipeCli.exe'
    $p = Start-Process -FilePath $app -ArgumentList $Arguments -WorkingDirectory $Sandbox -PassThru -RedirectStandardOutput $std -RedirectStandardError $err
    $finished = $p.WaitForExit($LimitMs)
    if (-not $finished) {
        $p.Kill()
        $p.WaitForExit()
    }
    if ($finished) { $p.Refresh() }
    $stdout = if (Test-Path -LiteralPath $std) { [IO.File]::ReadAllText($std) } else { '' }
    $stderr = if (Test-Path -LiteralPath $err) { [IO.File]::ReadAllText($err) } else { '' }
    # C# MCP PowerShell hosts can return a null ExitCode for a terminated
    # console child; never count that as success unless the emitted explicit
    # positive completion marker is present and there is no stderr.
    $nativeExit = if ($finished) { $p.ExitCode } else { -1 }
    $code = if (-not $finished) { -1 }
            elseif ($null -ne $nativeExit) { [int]$nativeExit }
            elseif ($stdout.Contains($ExpectedMarker) -and
                    [string]::IsNullOrWhiteSpace($stderr)) { 0 }
            else { 47 }
    [pscustomobject]@{
        stage = $Label
        exit_code = $code
        timed_out = (-not $finished)
        stdout = $stdout
        stderr = $stderr
    }
}

$results = @()
$results += Run-BoundedProbe 'locate-setbuffers' @('--cv-component-probe','LocateTool','--withbuffers') $ComponentTimeoutMs 'SETBUFFERS_END=LocateTool'
if (-not $results[0].timed_out -and $results[0].exit_code -eq 0) {
    $results += Run-BoundedProbe 'cv-full-constructor' @('--cv-native-diagnostic',$RecipeName) $ConstructorTimeoutMs 'CV_DIAGNOSTIC=PASS'
}
if ($results.Count -ge 2 -and $results[1].exit_code -eq 0) {
    $results += Run-BoundedProbe 'cv-eight-stations' @('--cv-native-diagnostic',$RecipeName,'--all-stations') $ConstructorTimeoutMs 'STATIONS_LOADED=8'
}
if ($results.Count -ge 3 -and $results[2].exit_code -eq 0) {
    $guiResultFile = Join-Path $Sandbox 'r3-native-gui-result.txt'
    $app = Join-Path $Sandbox 'InspToolsStandalone.exe'
    $gui = Start-Process -FilePath $app -ArgumentList @('--r3-native-gui-smoke',$RecipeName) -WorkingDirectory $Sandbox -PassThru
    $finished = $gui.WaitForExit($GuiTimeoutMs)
    if (-not $finished) { $gui.Kill();$gui.WaitForExit() }
    if ($finished) { $gui.Refresh() }
    $guiText = if (Test-Path -LiteralPath $guiResultFile) {
        [IO.File]::ReadAllText($guiResultFile)
    } else { '' }
    $passCount = @($guiText -split "\r?\n" | Where-Object { $_.StartsWith('PASS ') }).Count
    $guiExit = if (-not $finished) { -1 }
        elseif ($gui.ExitCode -eq 0 -and $passCount -eq 10 -and -not $guiText.Contains('FAIL ')) { 0 }
        else { 55 }
    $results += [pscustomobject]@{
        stage = 'r3-original-gui-eight-stations'
        exit_code = $guiExit
        timed_out = (-not $finished)
        stdout = $guiText
        stderr = ''
    }
}
function Run-BoundedGui([string]$Stage, [string]$Option, [string]$ResultFile,
    [int]$ExpectedPasses, [string]$FixtureRecipeName = '') {
    $app = Join-Path $Sandbox 'InspToolsStandalone.exe'
    $log = Join-Path $Sandbox $ResultFile
    if ([string]::IsNullOrWhiteSpace($FixtureRecipeName)) {
        $FixtureRecipeName = $RecipeName
    }
    $proc = Start-Process -FilePath $app -ArgumentList @($Option, $FixtureRecipeName) -WorkingDirectory $Sandbox -PassThru
    $finished = $proc.WaitForExit($GuiTimeoutMs)
    if (-not $finished) { $proc.Kill(); $proc.WaitForExit() }
    if ($finished) { $proc.Refresh() }
    $result = if (Test-Path -LiteralPath $log) { [IO.File]::ReadAllText($log) } else { '' }
    $passes = @($result -split "\r?\n" | Where-Object { $_.StartsWith('PASS ') }).Count
    $exit = if (-not $finished) { -1 }
        elseif ($proc.ExitCode -eq 0 -and $passes -eq $ExpectedPasses -and
                -not $result.Contains('FAIL ')) { 0 }
        else { 59 }
    return [pscustomobject]@{
        stage = $Stage
        exit_code = $exit
        timed_out = (-not $finished)
        stdout = $result
        stderr = ''
    }
}
if ($results.Count -ge 4 -and $results[3].exit_code -eq 0) {
    $results += Run-BoundedGui 'r3-original-dialog-open-close' '--r3-original-dialog-smoke' 'r3-dialog-smoke-result.txt' 18
}
if ($results.Count -ge 5 -and $results[4].exit_code -eq 0) {
    $results += Run-BoundedGui 'r3-original-saveall-and-byte-rollback' '--r3-saveall-smoke' 'r3-saveall-result.txt' 12
}
if ($results.Count -ge 6 -and $results[5].exit_code -eq 0) {
    $app = Join-Path $Sandbox 'InspToolsStandalone.exe'
    $log = Join-Path $Sandbox 'r3-editor-menu-result.txt'
    $args = @('--recipe-native-r3',$RecipeName,'--station','5',
        '--enable-r3-editor','--r3-editor-menu-smoke')
    $proc = Start-Process -FilePath $app -ArgumentList $args -WorkingDirectory $Sandbox -PassThru
    $finished = $proc.WaitForExit($GuiTimeoutMs)
    if (-not $finished) { $proc.Kill(); $proc.WaitForExit() }
    if ($finished) { $proc.Refresh() }
    $result = if (Test-Path -LiteralPath $log) { [IO.File]::ReadAllText($log) } else { '' }
    $passes = @($result -split "\r?\n" | Where-Object { $_.StartsWith('PASS ') }).Count
    $exit = if (-not $finished) { -1 }
        elseif ($proc.ExitCode -eq 0 -and $passes -eq 3 -and
                -not $result.Contains('FAIL ')) { 0 }
        else { 60 }
    $results += [pscustomobject]@{
        stage = 'r3-opt-in-editor-menu'
        exit_code = $exit
        timed_out = (-not $finished)
        stdout = $result
        stderr = ''
    }
}
if ($results.Count -ge 7 -and $results[6].exit_code -eq 0) {
    $results += Run-BoundedGui 'r3-shunt-ccd1-binary-save-and-rollback' '--r3-shunt-binary-save-smoke' 'r3-shunt-binary-save-result.txt' 24 'R2PROBE-SHUNT-3923-R002'
}
$report = [pscustomobject]@{
    kind = 'isolated-native-lazy-init-probe'
    sandbox = $Sandbox
    patched_dll_hash = (Get-FileHash -LiteralPath (Join-Path $Sandbox 'InspTools.dll') -Algorithm SHA256).Hash
    stages = $results
    production_untouched = $true
}
$report | ConvertTo-Json -Depth 7
if ($results | Where-Object { $_.exit_code -ne 0 }) { exit 2 }
