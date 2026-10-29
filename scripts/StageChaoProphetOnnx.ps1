param(
    [Parameter(Mandatory=$true)][string]$Candidate,
    [string]$ModelPath = ''
)

# Copy the CHAO-generated ONNX into a new portable candidate. Some model
# artifacts are wrapped with the 16-byte DAVS (int32=1,int32=2,double=3.0)
# header. ONNX Runtime cannot consume the envelope; preserve donor untouched.
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if (-not $ModelPath) {
    $ModelPath = Join-Path (Split-Path -Parent $root) 'ProjectHub.Native\ProjectData\Projects\Delta-AIProphet\Output\real-e1\model.onnx'
}
$candidate = [IO.Path]::GetFullPath($Candidate).TrimEnd('\')
$allow = (Join-Path $root 'output\standalone-r3') + '\'
if (-not $candidate.StartsWith($allow, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Destination must be an isolated R3 candidate'
}
if ((Get-Content -LiteralPath (Join-Path $candidate 'portable-review.marker') -Raw).Trim() -ne 'INSPTOOLS_PORTABLE_REVIEW_R3_V1') {
    throw 'Candidate has no verified review marker'
}
if ((Get-FileHash -LiteralPath (Join-Path $candidate 'InspTools.dll') -Algorithm SHA256).Hash -ne 'ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B') {
    throw 'Unexpected InspTools DLL'
}
$source = [IO.Path]::GetFullPath($ModelPath)
$expected = '230C78FF5CF6518AEACCA0081768B449C657E9009E763ABEDF0B7C9CD295E903'
if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $expected) {
    throw 'Original real-e1 model hash changed; re-audit first'
}
$dest = Join-Path $candidate 'ChaoAI\Models\prophet-real-e1.onnx'
[IO.Directory]::CreateDirectory((Split-Path -Parent $dest)) | Out-Null
if (Test-Path -LiteralPath $dest) {
    throw 'Refusing to overwrite an existing candidate ONNX'
}
$header = [byte[]](1,0,0,0,2,0,0,0,0,0,0,0,0,0,8,64)
$input = [IO.File]::OpenRead($source)
try {
    if ($input.Length -le 16) { throw 'Empty wrapped model' }
    $actual = New-Object byte[] 16
    if ($input.Read($actual, 0, 16) -ne 16) { throw 'Cannot read DAVS header' }
    for ($i=0;$i -lt 16;$i++) {
        if ($actual[$i] -ne $header[$i]) { throw 'Unknown ONNX envelope; no automatic unwrap' }
    }
    $output = [IO.File]::Open($dest, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
    try { $input.CopyTo($output) } finally { $output.Dispose() }
} finally { $input.Dispose() }
if ((Get-Item -LiteralPath $dest).Length -ne (Get-Item -LiteralPath $source).Length - 16) {
    throw 'Unwrap length mismatch'
}
$manifest = [ordered]@{
    schema = 'chao-prophet-onnx-smoke-v1'
    provenance = 'Delta-AIProphet real-e1, 1 epoch ONLY'
    wrapped_sha256 = $expected
    header = 'DAVS int32(1),int32(2),float64(3.0)'
    stripped_bytes = 16
    output = 'ChaoAI\Models\prophet-real-e1.onnx'
    unwrapped_sha256 = (Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash
    deployment_allowed = $false
    inference_validated = $false
}
$manifest | ConvertTo-Json -Depth 4 |
    Set-Content -LiteralPath (Join-Path $candidate 'ChaoAI\prophet-e1-provenance.json') -Encoding UTF8
[pscustomobject]@{status='UNWRAPPED_ONLY';output=$dest;sha256=$manifest.unwrapped_sha256;bytes=(Get-Item $dest).Length;deployment_allowed=$false} | ConvertTo-Json
