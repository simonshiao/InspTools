param(
    [Parameter(Mandatory=$true)][string]$Candidate,
    [Parameter(Mandatory=$true)][string]$Image,
    [string]$RuntimeRoot = ''
)
$ErrorActionPreference = 'Stop'
$root = (Split-Path -Parent $PSScriptRoot)
if (-not $RuntimeRoot) {
    $RuntimeRoot = Join-Path (Split-Path -Parent $root) 'ShuntVC_Chao\Runtime'
}
$candidate = [IO.Path]::GetFullPath($Candidate)
if (-not $candidate.StartsWith(
    (Join-Path $root 'output\standalone-r3') + '\',
    [StringComparison]::OrdinalIgnoreCase)) { throw 'Only isolated candidate permitted' }
$model = Join-Path $candidate 'ChaoAI\Models\prophet-real-e1.onnx'
$evidence = Join-Path $candidate 'ChaoAI\prophet-e1-provenance.json'
if (-not (Test-Path $evidence)) { throw 'Missing 1-epoch provenance guard' }
$manifest = Get-Content -LiteralPath $evidence -Raw | ConvertFrom-Json
if ($manifest.deployment_allowed -or
    $manifest.unwrapped_sha256 -ne (Get-FileHash $model -Algorithm SHA256).Hash) {
    throw 'Smoke model provenance / SHA mismatch'
}
$ort = Join-Path $RuntimeRoot 'Microsoft.ML.OnnxRuntime.dll'
$memory = Join-Path $RuntimeRoot 'System.Memory.dll'
$buffers = Join-Path $RuntimeRoot 'System.Buffers.dll'
$netstandard = $null
foreach ($u in Get-ChildItem -LiteralPath 'C:\Users' -Directory -ErrorAction SilentlyContinue) {
    $ref = Join-Path $u.FullName '.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.3\build\.NETFramework\v4.7.2\Facades\netstandard.dll'
    if (Test-Path -LiteralPath $ref) { $netstandard = $ref; break }
}
if (-not $netstandard) { throw 'Missing NET472 netstandard facade' }
$env:PATH = $RuntimeRoot + ';' + $env:PATH
foreach($assembly in @($memory,$buffers,$ort)) {
    [void][Reflection.Assembly]::LoadFrom($assembly)
}
Add-Type -Path (Join-Path $PSScriptRoot 'ChaoProphetOnnxSmoke.cs') -ReferencedAssemblies @(
    $ort, $memory, $buffers, $netstandard, 'System.Drawing.dll', 'System.dll'
)
$out = Join-Path $candidate 'ChaoAI\SmokeResults\real-e1-mask.png'
$result = [ChaoProphetOnnxSmoke]::Run($model, [IO.Path]::GetFullPath($Image), $out)
$result | Set-Content -LiteralPath (Join-Path $candidate 'ChaoAI\SmokeResults\real-e1-report.txt') -Encoding UTF8
Write-Output $result
