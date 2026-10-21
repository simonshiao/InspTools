from __future__ import annotations

import hashlib
import json
import sys
from pathlib import Path


EXPECTED_MACHINE = {
    "InspTools.Core.dll",
    "InspTools.SDK.dll",
    "InspTools.Wpf.dll",
    "InspToolsStandalone.exe",
}
EXPECTED_AGENT = {"InspTools.Agent.dll", "InspTools.AgentBridge.exe"}
EXPECTED_EXAMPLES = {"DIALikeHost.exe", "ShuntLikeHost.exe"}
FORBIDDEN_NAMES = ("smoke", "fixture", "replica", "referencecache", "dialogbinder")
FORBIDDEN_BINARY_MARKERS = (
    b"D:\\delta",
    b"InspToolsInspectionReplica",
    b"ReferenceCache",
    b"CVInspection",
    b"OpenCvSharp.Mat",
)


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest().upper()


def fail(message: str) -> None:
    raise SystemExit("IR2_06_RELEASE_AUDIT=FAIL\n" + message)


def main() -> int:
    if len(sys.argv) != 2:
        fail("Usage: audit_ir206_release.py <release-current>")

    root = Path(sys.argv[1]).resolve()
    manifest_path = root / "manifest.json"
    if not manifest_path.is_file():
        fail(f"manifest missing: {manifest_path}")

    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    if manifest.get("schema") != "InspTools.Rebuild2.DeliveryManifest.v1":
        fail("unexpected manifest schema")
    if manifest.get("target") != "net472-x64-wpf":
        fail("unexpected target")
    if manifest.get("capability_boundaries", {}).get("inspect") != "NOT_RUN":
        fail("Inspect boundary must remain NOT_RUN")

    machine = {p.name for p in (root / "bin").glob("*") if p.is_file()}
    agent = {p.name for p in (root / "agent").glob("*") if p.is_file()}
    examples = {p.name for p in (root / "examples").glob("*") if p.is_file()}
    if machine != EXPECTED_MACHINE:
        fail(f"machine files mismatch: {sorted(machine)}")
    if agent != EXPECTED_AGENT:
        fail(f"agent files mismatch: {sorted(agent)}")
    if examples != EXPECTED_EXAMPLES:
        fail(f"example files mismatch: {sorted(examples)}")

    for path in root.rglob("*"):
        if not path.is_file():
            continue
        lower = path.name.lower()
        if path.suffix.lower() in {".dll", ".exe"} and any(token in lower for token in FORBIDDEN_NAMES):
            fail(f"forbidden product binary name: {path.relative_to(root)}")

    listed = {}
    for item in manifest.get("files", []):
        rel = str(item.get("path", "")).replace("\\", "/")
        path = root / rel
        if not path.is_file():
            fail(f"manifest file missing: {rel}")
        actual_hash = sha256(path)
        if actual_hash != str(item.get("sha256", "")).upper():
            fail(f"hash mismatch: {rel}")
        if path.stat().st_size != int(item.get("bytes", -1)):
            fail(f"size mismatch: {rel}")
        listed[rel] = actual_hash

    actual = {
        p.relative_to(root).as_posix()
        for p in root.rglob("*")
        if p.is_file() and p.name != "manifest.json"
    }
    if set(listed) != actual:
        fail(f"manifest coverage mismatch: listed={len(listed)} actual={len(actual)}")

    # Machine-facing binaries must not carry known legacy/vendor/path markers.
    for name in EXPECTED_MACHINE | EXPECTED_EXAMPLES:
        folder = "bin" if name in EXPECTED_MACHINE else "examples"
        data = (root / folder / name).read_bytes()
        for marker in FORBIDDEN_BINARY_MARKERS:
            if marker in data:
                fail(f"forbidden marker {marker!r} in {folder}/{name}")

    print(
        "IR2_06_RELEASE_AUDIT=PASS "
        f"files={len(actual)} machine={len(machine)} agent={len(agent)} examples={len(examples)} "
        "inspect=NOT_RUN"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

