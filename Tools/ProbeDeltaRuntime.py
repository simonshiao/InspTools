#!/usr/bin/env python3
"""Read-only, bounded DeltaAIProcess native loader test (NOT a CUDA analysis).

Only the proprietary Delta DLL is the test target. NVIDIA / OpenCV are treated
as ordinary runtime dependencies. Existing ShuntVC_Chao files are read-only.

  python Tools/ProbeDeltaRuntime.py ROOT
  python Tools/ProbeDeltaRuntime.py ROOT --create

The parent process times out and terminates the child. No PATH registry or
system settings are changed; no Model/Training/Recipe files are modified.
"""
import argparse
import ctypes
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

PIN = "3B056989D395E83B73AA2CD82EF3CB37DDA6DD8C50541DFD051636CA36DB5B38"
LIBRARY = "DeltaAIProcessDll.dll"
IMPORTS = (
    ("DeltaAIProcess", "opencv_world480.dll"),
    ("DeltaAIProcess", "nvinfer.dll"),
    ("CUDA11_118", "cudart64_110.dll"),
)
DIRS = ("DeltaAIProcess", "CUDA11_118", "ModelConverter/TensorRT861")


def digest(path):
    h = hashlib.sha256()
    with path.open("rb") as fp:
        for buf in iter(lambda: fp.read(1024 * 1024), b""):
            h.update(buf)
    return h.hexdigest().upper()


def preflight(root):
    root = root.resolve()
    lib = root / "DeltaAIProcess" / LIBRARY
    if not lib.is_file():
        raise FileNotFoundError(lib)
    actual = digest(lib)
    if actual != PIN:
        raise ValueError(f"DeltaAIProcess DLL mismatch: {actual}")
    deps = []
    for rel, filename in IMPORTS:
        full = root / rel / filename
        deps.append({"name": filename, "relative_path": f"{rel}/{filename}",
                     "exists": full.is_file(), "bytes": full.stat().st_size if full.is_file() else 0})
    return {"source": str(lib), "sha256": actual, "deps": deps,
            "all_direct_imports_present": all(x["exists"] for x in deps)}


def child(root, create):
    if os.name != "nt":
        raise RuntimeError("This native loader probe requires Windows")
    extra_dirs = []
    for sub in DIRS:
        folder = root / sub
        if folder.is_dir():
            extra_dirs.append(os.add_dll_directory(str(folder)))
    target = root / "DeltaAIProcess" / LIBRARY
    print("CHILD_STAGE=LOAD_BEGIN", flush=True)
    api = ctypes.WinDLL(str(target))
    print("CHILD_STAGE=LOAD_OK", flush=True)
    for name in ("CreateInstance", "DisposeInstance", "SetGPU", "ReadModel"):
        getattr(api, name)
    print("CHILD_STAGE=EXPORTS_OK", flush=True)
    if create:
        print("CHILD_STAGE=DELTA_CREATE_BEGIN", flush=True)
        creator = api.CreateInstance
        creator.argtypes = []
        creator.restype = ctypes.c_void_p
        obj = creator()
        if not obj:
            raise RuntimeError("Delta CreateInstance returned null")
        print("CHILD_STAGE=DELTA_CREATE_OK", flush=True)
        disposer = api.DisposeInstance
        disposer.argtypes = [ctypes.c_void_p]
        disposer.restype = None
        disposer(obj)
        print("CHILD_STAGE=DELTA_DISPOSE_OK", flush=True)
    print("CHILD_STAGE=PASS", flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("root", help="Existing ShuntVC_Chao/Runtime/Runtime")
    parser.add_argument("--create", action="store_true",
                        help="Also test Delta CreateInstance + DisposeInstance")
    parser.add_argument("--child", action="store_true", help=argparse.SUPPRESS)
    parser.add_argument("--timeout", type=int, default=12)
    args = parser.parse_args()
    root = Path(args.root).resolve()
    info = preflight(root)
    if args.child:
        child(root, args.create)
        return
    argv = [sys.executable, str(Path(__file__).resolve()), str(root), "--child"]
    if args.create:
        argv.append("--create")
    try:
        outcome = subprocess.run(argv, capture_output=True, text=True,
                                 timeout=min(max(args.timeout, 3), 20),
                                 cwd=str(root))
        info["loader_exit_code"] = outcome.returncode
        info["loader_stdout"] = outcome.stdout[-3500:]
        info["loader_stderr"] = outcome.stderr[-3500:]
        info["timed_out"] = False
        info["passed"] = outcome.returncode == 0 and "CHILD_STAGE=PASS" in outcome.stdout
    except subprocess.TimeoutExpired as ex:
        info.update(loader_exit_code=None, timed_out=True, passed=False,
                    loader_stdout=(ex.stdout or b"").decode("utf8", "replace")[-3500:],
                    loader_stderr=(ex.stderr or b"").decode("utf8", "replace")[-3500:])
    print(json.dumps(info, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
