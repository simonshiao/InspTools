from __future__ import annotations

import argparse
import json
from pathlib import Path
import re


FORBIDDEN = {
    "core_namespace": r"\bInspTools\.Core\b",
    "recovered_cv_type": r"\bCVInspection\b|\bParamFile\b|OpenCvSharp\.Mat",
    "legacy_host_state": r"ReferenceCache|\bBinder\b|write[-_ ]guard",
    "external_transport": r"NamedPipe|AgentBridge|host-session",
    "legacy_product_path": r"Replica|staging",
    "process_launch": r"Process\.Start",
    "production_hardcode": r"D:\\delta",
}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("project_root")
    parser.add_argument("--report", default="")
    ns = parser.parse_args()

    root = Path(ns.project_root).resolve()
    hosts = [
        root / "src" / "DIALikeHost" / "Program.cs",
        root / "src" / "ShuntLikeHost" / "Program.cs",
    ]
    build = (root / "scripts" / "Build.ps1").read_text(encoding="utf-8")
    results = []
    ok = True

    for host in hosts:
        text = host.read_text(encoding="utf-8")
        matches = {}
        for name, pattern in FORBIDDEN.items():
            found = re.findall(pattern, text, flags=re.IGNORECASE)
            if found:
                matches[name] = found[:5]
        public_usings = "using InspTools.SDK;" in text and "using InspTools.Wpf;" in text
        ok = ok and public_usings and not matches
        results.append(
            {
                "path": str(host),
                "public_usings": public_usings,
                "forbidden_matches": matches,
            }
        )

    compile_lines = [
        line.strip()
        for line in build.splitlines()
        if "DIALikeHost\\Program.cs" in line or "ShuntLikeHost\\Program.cs" in line
    ]
    compile_contract = len(compile_lines) == 2 and all("@($sdk,$wpf)" in line for line in compile_lines)
    no_direct_core_ref = all("$core" not in line for line in compile_lines)
    ok = ok and compile_contract and no_direct_core_ref

    payload = {
        "status": "PASS" if ok else "FAIL",
        "hosts": results,
        "build_compile_lines": compile_lines,
        "build_public_refs_only": compile_contract,
        "build_no_direct_core_ref": no_direct_core_ref,
    }
    rendered = json.dumps(payload, ensure_ascii=False, indent=2)
    print(rendered)
    if ns.report:
        report = Path(ns.report).resolve()
        report.parent.mkdir(parents=True, exist_ok=True)
        report.write_text(rendered + "\n", encoding="utf-8")
    return 0 if ok else 2


if __name__ == "__main__":
    raise SystemExit(main())
