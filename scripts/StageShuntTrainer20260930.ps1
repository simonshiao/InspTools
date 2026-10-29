param([string]$SourceRuntime = '')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$app = Join-Path $root 'output\decoded-ui\Release'
if (-not $SourceRuntime) {
    # Windows PowerShell 5.1 may decode BOM-less UTF-8 scripts as ANSI.
    # Resolve the vendor's non-ASCII directory without embedding its name.
    $shunt = 'D:\delta\ShuntSensor'
    $matches = @(Get-ChildItem -LiteralPath $shunt -Directory |
        Where-Object {
            Test-Path -LiteralPath (Join-Path $_.FullName 'Runtime\ShuntVC\Runtime\TrainingRebuild\train_compat.py')
        })
    if ($matches.Count -ne 1) {
        throw 'Expected exactly one ShuntVC training donor under ShuntSensor'
    }
    $SourceRuntime = Join-Path $matches[0].FullName 'Runtime\ShuntVC\Runtime'
}
$source = [IO.Path]::GetFullPath($SourceRuntime)
if (-not (Test-Path -LiteralPath (Join-Path $app 'InspToolsRecipeCli.exe'))) {
    throw 'Build standalone before staging Shunt training assets'
}
$files = @(
    @{
        source='TrainingRebuild\train_compat.py'
        target='TrainingRebuild\train_compat_shunt_20260930.py'
        sha='2028C94D788E93B3822D9CA8D6650D27BD3B983F019EF783D17DFA984DEB2C32'
    },
    @{
        source='TrainingRebuild\render_model_preview.py'
        target='TrainingRebuild\render_model_preview.py'
        sha='DB81DA3D3A066B980C9176F6D667A5280CB34E886B181AF430B96E22CD7765BA'
    },
    @{
        source='TrainingRebuild\threshold_sweep.py'
        target='TrainingRebuild\threshold_sweep.py'
        sha='D9A095E69713866E0FB56DBDDBC9CD842FF2682232F87D0251DBA05B192D1B2C'
    },
    @{
        source='AgentVisionLab\trainers\temp_unet_trainer.py'
        target='AgentVisionLab\trainers\temp_unet_trainer.py'
        sha='7B0E07187928F410922D93EF19D052D6FEB7E6C641A05D8A24574A2FEF104913'
    },
    @{
        source='AgentVisionLab\quality_gate.py'
        target='AgentVisionLab\quality_gate.py'
        sha='A6B6F26A4ADB1CE534EE228583BA5D7F98846BFCF4603A0ED2C664B953820F77'
    },
    @{
        source='AgentVisionLab\dia_model_tools.py'
        target='AgentVisionLab\dia_model_tools.py'
        sha='12C43C9E032CF4EB4DF9F41A6DE24D3AA6EF1731A10E7601D8F4AE626328018F'
    }
)
foreach ($item in $files) {
    $s = Join-Path $source $item.source
    $t = Join-Path (Join-Path $app 'Runtime') $item.target
    if (-not (Test-Path -LiteralPath $s -PathType Leaf) -or
        (Get-FileHash -LiteralPath $s).Hash -ne $item.sha) {
        throw ('Pinned Shunt 2026-09-30 source missing or changed: ' + $item.source)
    }
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($t)) | Out-Null
    if (Test-Path -LiteralPath $t -PathType Leaf) {
        if ((Get-FileHash -LiteralPath $t).Hash -ne $item.sha) {
            throw ('Existing standalone Shunt training module differs: ' + $t)
        }
    } else {
        Copy-Item -LiteralPath $s -Destination $t
    }
    if ((Get-FileHash -LiteralPath $t).Hash -ne $item.sha) {
        throw ('Standalone Shunt training module copy hash failed: ' + $t)
    }
}
# Explicitly do not switch the old train_compat.py used by existing candidate
# jobs: the 2026-09-30 adapter has different artifact/result contracts and its
# resume gate requires separate acceptance. This stage adds immutable,
# app-local versioned assets, NOT a live source reference.
$legacy = Join-Path $app 'Runtime\TrainingRebuild\train_compat.py'
if ((Get-FileHash -LiteralPath $legacy).Hash -ne
    '4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E') {
    throw 'Legacy candidate trainer changed while staging alternate backend'
}
Write-Output ('SHUNT_20260930_TRAINER_STAGE=PASS FILES='+$files.Count+
    ' BACKEND=VERSIONED_APP_LOCAL DEFAULT_BACKEND_UNCHANGED=True'+
    ' PRODUCTION_UNCHANGED=True')
