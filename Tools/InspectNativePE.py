#!/usr/bin/env python3
"""REA/cra-pe/cra-native preflight: static PE metadata, no DLL execution/writes.

Usage: python Tools/InspectNativePE.py PATH_TO_DLL
Only Python's stdlib is required. Exports/imports/strings are file evidence,
not recovered source or evidence that a function executes successfully.
"""
import hashlib
import json
import re
import struct
import sys
from pathlib import Path


class PE:
    def __init__(self, path):
        self.path = Path(path)
        self.data = self.path.read_bytes()
        self.length = len(self.data)
        if self.data[:2] != b"MZ":
            raise ValueError("missing MZ signature")
        self.pe_offset = self.u32(0x3C)
        if self.at(self.pe_offset, 4) != b"PE\x00\x00":
            raise ValueError("missing PE signature")
        coff = self.pe_offset + 4
        self.machine, count, timestamp, _, _, opt_size, chars = struct.unpack_from(
            "<HHIIIHH", self.data, coff)
        self.timestamp = timestamp
        self.characteristics = chars
        optional = coff + 20
        self.magic = self.u16(optional)
        if self.magic not in (0x10B, 0x20B):
            raise ValueError("unexpected optional header")
        self.bits = 64 if self.magic == 0x20B else 32
        self.image_base = (self.u64(optional + 24) if self.bits == 64
                           else self.u32(optional + 28))
        self.entry = self.u32(optional + 16)
        dd_offset = optional + (112 if self.bits == 64 else 96)
        self.data_dirs = [
            struct.unpack_from("<II", self.data, dd_offset + 8 * i)
            for i in range(16)]
        sec = optional + opt_size
        self.sections = []
        for i in range(count):
            o = sec + i * 40
            name, vsize, va, rawsize, rawptr, _, _, _, _, flags = struct.unpack_from(
                "<8sIIIIIIHHI", self.data, o)
            self.sections.append({
                "name": name.rstrip(b"\x00").decode("ascii", "replace"),
                "rva": va, "virtual_size": vsize,
                "raw_offset": rawptr, "raw_size": rawsize,
                "characteristics": hex(flags),
                "executable": bool(flags & 0x20000000),
            })

    def at(self, offset, size):
        if offset < 0 or offset + size > self.length:
            raise ValueError(f"out-of-file PE reference offset={offset} size={size}")
        return self.data[offset:offset + size]

    def u16(self, o):
        return struct.unpack("<H", self.at(o, 2))[0]

    def u32(self, o):
        return struct.unpack("<I", self.at(o, 4))[0]

    def u64(self, o):
        return struct.unpack("<Q", self.at(o, 8))[0]

    def rva(self, address):
        for s in self.sections:
            if s["rva"] <= address < s["rva"] + max(s["virtual_size"], s["raw_size"]):
                off = s["raw_offset"] + address - s["rva"]
                if off < s["raw_offset"] + s["raw_size"] and off < self.length:
                    return off
        if 0 <= address < min(self.length, 4096):
            return address  # PE headers
        raise ValueError(f"unmapped RVA 0x{address:x}")

    def cstr(self, offset, bound=512):
        blob = self.at(offset, min(bound, self.length - offset))
        return blob.split(b"\x00", 1)[0].decode("ascii", "replace")

    def exports(self):
        rva, size = self.data_dirs[0]
        if not rva or not size:
            return []
        ed = struct.unpack_from("<IIHHIIIIIII", self.data, self.rva(rva))
        _, _, _, _, dll_name, ordinal_base, function_count, named_count, ftab, ntab, otab = ed
        if function_count > 50000 or named_count > 50000:
            raise ValueError("implausible PE export count")
        names = {}
        for i in range(named_count):
            name_rva = self.u32(self.rva(ntab + i * 4))
            idx = self.u16(self.rva(otab + i * 2))
            names[idx] = self.cstr(self.rva(name_rva))
        exports = []
        for i in range(function_count):
            addr = self.u32(self.rva(ftab + i * 4))
            if not addr:
                continue
            item = {"ordinal": ordinal_base + i, "name": names.get(i),
                    "rva": f"0x{addr:08x}"}
            if rva <= addr < rva + size:
                item["forwarder"] = self.cstr(self.rva(addr))
            exports.append(item)
        return exports

    def imports(self):
        addr, size = self.data_dirs[1]
        if not addr or not size:
            return []
        result = []
        stride = 8 if self.bits == 64 else 4
        ordinal_mask = 1 << (self.bits - 1)
        for idx in range(min(size // 20, 250)):
            row = struct.unpack_from("<IIIII", self.data, self.rva(addr + idx * 20))
            original, _, _, lib_rva, first = row
            if not any(row):
                break
            lib = self.cstr(self.rva(lib_rva))
            functions = []
            lookup = original or first
            for j in range(2000):
                offset = self.rva(lookup + j * stride)
                value = self.u64(offset) if self.bits == 64 else self.u32(offset)
                if not value:
                    break
                if value & ordinal_mask:
                    functions.append(f"#{value & 0xffff}")
                else:
                    functions.append(self.cstr(self.rva(value) + 2))
            result.append({"dll": lib, "functions": functions})
        return result


def diagnostic_strings(buf, limit=70):
    terms = ("cuda", "cudnn", "tensor", "trt", "model", "gpu", "opencv",
             "infer", "onnx", "createinstance", "loadlibrary", "setgpu",
             "init", "error", "predict", "nvinfer", "torch", "version")
    found = []
    raw = re.findall(rb"[ -~]{5,200}", buf)
    wide = [x.decode("utf-16le", "replace") for x in
            re.findall(rb"(?:[ -~]\x00){5,200}", buf)]
    for s in [x.decode("ascii", "replace") for x in raw] + wide:
        if any(t in s.lower() for t in terms) and s not in found:
            found.append(s)
    return found[:limit]


def main():
    imports_only = len(sys.argv) == 3 and sys.argv[2] == "--imports-only"
    if (len(sys.argv) not in (2, 3) or
            (len(sys.argv) == 3 and not imports_only)):
        raise SystemExit("Usage: InspectNativePE.py PATH_TO_DLL [--imports-only]")
    pe = PE(sys.argv[1])
    out = {
        "kind": "static-native-PE-evidence",
        "source": str(pe.path),
        "size_bytes": pe.length,
        "sha256": hashlib.sha256(pe.data).hexdigest().upper(),
        "machine": f"0x{pe.machine:04X}",
        "bits": pe.bits,
        "image_base": f"0x{pe.image_base:X}",
        "entry_rva": f"0x{pe.entry:08X}",
        "sections": pe.sections,
        "exports": [] if imports_only else pe.exports(),
        "imports": pe.imports(),
        "relevant_strings_sample": [] if imports_only else diagnostic_strings(pe.data),
        "limitations": [
            "Static PE parsing only; no native code has been executed.",
            "Dynamically loaded DLLs and APIs may not appear in import table.",
            "Export names do not establish function prototypes or success.",
            "Not decompiled pseudocode; use REA/Ghidra/IDA if registered.",
        ],
    }
    print(json.dumps(out, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
