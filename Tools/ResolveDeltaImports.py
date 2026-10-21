#!/usr/bin/env python3
"""Find recursively imported runtime DLL names absent from a disposable gate.

This is *dependency metadata only*, not NVIDIA code decompilation.
Inspects existing PE import directories to avoid blindly bundling CUDA.
"""
import gc
import json
import sys
from pathlib import Path

from InspectNativePE import PE


def main():
    if len(sys.argv) != 3:
        raise SystemExit("Usage: ResolveDeltaImports.py ORIGINAL_RUNTIME GATE_RUNTIME")
    source = Path(sys.argv[1])
    gate = Path(sys.argv[2])
    folders = ("DeltaAIProcess", "CUDA11_118", "ModelConverter/TensorRT861")
    original = {}
    installed = {}
    for directory in folders:
        for f in (source / directory).glob("*.dll"):
            original[f.name.lower()] = f
        for f in (gate / directory).glob("*.dll"):
            installed[f.name.lower()] = f
    queue = ["deltaaiprocessdll.dll", "opencv_world480.dll", "nvinfer.dll",
             "cudart64_110.dll"]
    visited = set()
    missing = {}
    system = set()
    graph = {}
    while queue:
        name = queue.pop(0)
        if name in visited:
            continue
        visited.add(name)
        target = installed.get(name) or original.get(name)
        if target is None:
            system.add(name)
            continue
        try:
            dll = PE(target)
            imports = [i["dll"].lower() for i in dll.imports()]
            # Release in each pass: OpenCV can exceed 450 MB.
            del dll
            gc.collect()
        except Exception as error:
            graph[name] = {"error": str(error)}
            continue
        graph[name] = imports
        for dep in imports:
            if dep in original and dep not in installed:
                missing[dep] = {
                    "source": str(original[dep]),
                    "bytes": original[dep].stat().st_size,
                    "required_by": name,
                }
            if dep in original:
                queue.append(dep)
            else:
                system.add(dep)
    print(json.dumps({
        "kind": "PE-import-metadata-only",
        "inspected": len(graph),
        "required_but_unstaged": missing,
        "required_unstaged_mb": round(
            sum(v["bytes"] for v in missing.values()) / 1048576, 2),
        "external_or_system": sorted(system),
        "limitations": [
            "Direct static imports only; delay-load/dynamic dependencies not included.",
            "No third-party machine code disassembly is performed.",
        ],
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
