#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""InspTools Legacy TrainingImage X/Y 標註契約稽核。

驗證 Y/OK blank label、X/Y pair、尺寸，以及 Y/NG 的 class/ignore/maybe 色彩語意。
預設採樣；--full 才逐張掃描全部 Y label 像素。
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Dict, List, Sequence, Tuple

try:
    from PIL import Image
except ImportError as exc:
    raise SystemExit("缺少 Pillow；請使用專案既有 Python 環境或安裝 Pillow。") from exc


RGB = Tuple[int, int, int]
影像副檔名 = {".png", ".bmp", ".jpg", ".jpeg", ".tif", ".tiff"}

標準色: Dict[str, RGB] = {
    "black": (0, 0, 0),
    "ignore": (20, 20, 20),
    "red": (255, 0, 0),
    "green": (0, 255, 0),
    "blue": (0, 0, 255),
    "yellow": (255, 255, 0),
    "magenta": (255, 0, 255),
    "cyan": (0, 255, 255),
    "maybe_red": (125, 0, 0),
    "maybe_green": (0, 125, 0),
    "maybe_blue": (0, 0, 125),
    "maybe_yellow": (125, 125, 0),
    "maybe_magenta": (125, 0, 125),
    "maybe_cyan": (0, 125, 125),
}
標準色集合 = set(標準色.values())


def 建立參數() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="稽核 InspTools TrainingImage Legacy X/Y 標註契約")
    parser.add_argument("--root", action="append", required=True, help="TrainingImage 根目錄，可重複指定")
    parser.add_argument("--sample-ok", type=int, default=20, help="每個 root 抽樣 Y/OK 張數")
    parser.add_argument("--sample-ng", type=int, default=80, help="每個 root 抽樣 Y/NG 張數")
    parser.add_argument("--full", action="store_true", help="掃描全部 Y/OK + Y/NG 像素")
    parser.add_argument("--require-pairs", action="store_true", help="X/Y 缺 pair 或尺寸不一致時回傳失敗")
    parser.add_argument(
        "--strict-palette",
        action="store_true",
        help="非標準 RGB 出現即失敗；一般資料不建議，Resize 會產生插值色",
    )
    return parser.parse_args()


def 等距抽樣(items: Sequence[Path], 數量: int) -> List[Path]:
    if 數量 <= 0 or not items:
        return []
    if len(items) <= 數量:
        return list(items)
    if 數量 == 1:
        return [items[len(items) // 2]]
    return [items[(i * (len(items) - 1)) // (數量 - 1)] for i in range(數量)]


def 找Y檔(root: Path) -> Tuple[List[Path], List[Path]]:
    ok: List[Path] = []
    ng: List[Path] = []
    for path in root.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in 影像副檔名:
            continue
        parts = path.relative_to(root).parts
        if "Y" not in parts:
            continue
        y_index = parts.index("Y")
        if y_index + 1 >= len(parts):
            continue
        類別 = parts[y_index + 1].upper()
        if 類別 == "OK":
            ok.append(path)
        elif 類別 == "NG":
            ng.append(path)
    return sorted(ok), sorted(ng)


def 對應X(root: Path, y_path: Path) -> Path:
    parts = list(y_path.relative_to(root).parts)
    parts[parts.index("Y")] = "X"
    return root.joinpath(*parts)


def 建立X索引(root: Path) -> Dict[Tuple[Tuple[str, ...], str, str], List[Path]]:
    """依 Model path + OK/NG + filename 建立索引。

    Legacy SaveAsFolder 允許 X/Y 選不同子資料夾，因此不能要求 Y 子目錄結構
    一定與 X 完全相同；真正可追的 pair identity 是同 Model、同 OK/NG、同檔名。
    """
    index: Dict[Tuple[Tuple[str, ...], str, str], List[Path]] = defaultdict(list)
    for path in root.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in 影像副檔名:
            continue
        parts = path.relative_to(root).parts
        if "X" not in parts:
            continue
        x_index = parts.index("X")
        if x_index + 1 >= len(parts):
            continue
        kind = parts[x_index + 1].upper()
        if kind not in {"OK", "NG"}:
            continue
        key = (tuple(parts[:x_index]), kind, path.name.lower())
        index[key].append(path)
    return index


def 尋找XPair(root: Path, y_path: Path, x_index: Dict[Tuple[Tuple[str, ...], str, str], List[Path]]) -> List[Path]:
    exact = 對應X(root, y_path)
    if exact.is_file():
        return [exact]
    parts = y_path.relative_to(root).parts
    y_index = parts.index("Y")
    kind = parts[y_index + 1].upper()
    key = (tuple(parts[:y_index]), kind, y_path.name.lower())
    return x_index.get(key, [])


def 讀尺寸(path: Path) -> Tuple[int, int]:
    with Image.open(path) as img:
        return img.size


def 取色彩統計(path: Path) -> Counter:
    with Image.open(path) as img:
        rgb = img.convert("RGB")
        colors = rgb.getcolors(maxcolors=20_000_000)
        if colors is None:
            return Counter(rgb.getdata())
        return Counter({color: count for count, color in colors})


def 色彩家族(rgb: RGB) -> str:
    r, g, b = rgb
    if r == g == b:
        return "gray"
    if g <= 10 and b <= 10 and r > 0:
        return "red_axis"
    if r <= 10 and b <= 10 and g > 0:
        return "green_axis"
    if r <= 10 and g <= 10 and b > 0:
        return "blue_axis"
    if b <= 10 and r > 0 and g > 0 and abs(r - g) <= 10:
        return "yellow_axis"
    if g <= 10 and r > 0 and b > 0 and abs(r - b) <= 10:
        return "magenta_axis"
    if r <= 10 and g > 0 and b > 0 and abs(g - b) <= 10:
        return "cyan_axis"
    return "mixed_or_interpolated"


def 掃描一個Root(root: Path, args: argparse.Namespace) -> Dict[str, object]:
    if not root.is_dir():
        return {"root": str(root), "status": "FAIL", "errors": ["TrainingImage root does not exist"]}

    ok_files, ng_files = 找Y檔(root)
    全Y = ok_files + ng_files
    x_index = 建立X索引(root)
    missing_pairs: List[str] = []
    ambiguous_pairs: List[Dict[str, object]] = []
    dimension_mismatch: List[Dict[str, object]] = []
    missing_pair_count = 0
    ambiguous_pair_count = 0
    dimension_mismatch_count = 0
    pair_checked = 0

    for y_path in 全Y:
        candidates = 尋找XPair(root, y_path, x_index)
        if not candidates:
            missing_pair_count += 1
            if len(missing_pairs) < 50:
                missing_pairs.append(str(y_path))
            continue
        if len(candidates) > 1:
            ambiguous_pair_count += 1
            if len(ambiguous_pairs) < 50:
                ambiguous_pairs.append({"y": str(y_path), "x_candidates": [str(p) for p in candidates[:10]]})
            continue
        x_path = candidates[0]
        pair_checked += 1
        try:
            y_size = 讀尺寸(y_path)
            x_size = 讀尺寸(x_path)
            if y_size != x_size:
                dimension_mismatch_count += 1
                if len(dimension_mismatch) < 50:
                    dimension_mismatch.append(
                        {"y": str(y_path), "x": str(x_path), "y_size": y_size, "x_size": x_size}
                    )
        except Exception as exc:
            dimension_mismatch_count += 1
            if len(dimension_mismatch) < 50:
                dimension_mismatch.append({"y": str(y_path), "x": str(x_path), "error": repr(exc)})

    pixel_files = 全Y if args.full else 等距抽樣(ok_files, args.sample_ok) + 等距抽樣(ng_files, args.sample_ng)
    palette = Counter()
    family = Counter()
    canonical = Counter()
    canonical_examples: Dict[str, List[str]] = defaultdict(list)
    ok_nonblank: List[str] = []
    ng_blank: List[str] = []
    pixel_errors: List[Dict[str, str]] = []
    ok_set = set(ok_files)
    ng_set = set(ng_files)
    reverse_name = {value: name for name, value in 標準色.items()}

    for path in pixel_files:
        try:
            colors = 取色彩統計(path)
        except Exception as exc:
            pixel_errors.append({"path": str(path), "error": repr(exc)})
            continue

        palette.update(colors)
        non_black = sum(count for rgb, count in colors.items() if rgb != (0, 0, 0))
        if path in ok_set and non_black > 0 and len(ok_nonblank) < 50:
            ok_nonblank.append(str(path))
        if path in ng_set and non_black == 0 and len(ng_blank) < 50:
            ng_blank.append(str(path))

        for rgb, count in colors.items():
            family[色彩家族(rgb)] += count
            name = reverse_name.get(rgb)
            if name:
                canonical[name] += count
                if len(canonical_examples[name]) < 3:
                    canonical_examples[name].append(str(path))

    noncanonical_pixels = sum(count for rgb, count in palette.items() if rgb not in 標準色集合)
    total_pixels = sum(palette.values())
    errors: List[str] = []
    warnings: List[str] = []

    if not 全Y:
        errors.append("No Y/OK or Y/NG images found")
    if ok_nonblank:
        errors.append(f"Sampled Y/OK contains non-black pixels: {len(ok_nonblank)} file(s)")
    if pixel_errors:
        errors.append(f"Pixel decode failed: {len(pixel_errors)} file(s)")
    if args.require_pairs and missing_pair_count:
        errors.append(f"Missing X pair: {missing_pair_count} file(s)")
    if args.require_pairs and ambiguous_pair_count:
        errors.append(f"Ambiguous X pair: {ambiguous_pair_count} file(s)")
    if args.require_pairs and dimension_mismatch_count:
        errors.append(f"X/Y dimension mismatch: {dimension_mismatch_count} file(s)")
    if args.strict_palette and noncanonical_pixels > 0:
        errors.append(f"Strict palette violation: {noncanonical_pixels} non-canonical pixel(s)")

    if missing_pairs and not args.require_pairs:
        warnings.append("Missing X pair exists; rerun with --require-pairs to gate it")
    if ambiguous_pairs and not args.require_pairs:
        warnings.append("Ambiguous X pair exists; rerun with --require-pairs to gate it")
    if dimension_mismatch and not args.require_pairs:
        warnings.append("X/Y dimension mismatch exists; rerun with --require-pairs to gate it")
    if noncanonical_pixels > 0 and not args.strict_palette:
        warnings.append(
            "Non-canonical colors exist; expected at resized/anti-aliased label edges. Do not use hard palette as production gate."
        )

    status = "FAIL" if errors else ("WARN" if warnings else "PASS")
    return {
        "root": str(root),
        "status": status,
        "counts": {
            "y_ok": len(ok_files),
            "y_ng": len(ng_files),
            "pair_checked": pair_checked,
            "missing_x": missing_pair_count,
            "ambiguous_x": ambiguous_pair_count,
            "dimension_mismatch": dimension_mismatch_count,
            "pixel_scanned": len(pixel_files),
            "pixel_total": total_pixels,
            "noncanonical_pixels": noncanonical_pixels,
        },
        "legacy_pair": {
            "missing_x_examples": missing_pairs,
            "ambiguous_x_examples": ambiguous_pairs,
            "dimension_mismatch_examples": dimension_mismatch,
        },
        "label_semantics": {
            "canonical_pixel_counts": dict(canonical),
            "canonical_examples": dict(canonical_examples),
            "color_family_counts": dict(family),
            "ok_nonblank_examples": ok_nonblank,
            "ng_blank_examples": ng_blank,
            "palette_top30": [
                {"rgb": list(rgb), "count": count} for rgb, count in palette.most_common(30)
            ],
        },
        "errors": errors,
        "warnings": warnings,
    }


def main() -> int:
    args = 建立參數()
    results = [掃描一個Root(Path(root), args) for root in args.root]
    fail = any(item.get("status") == "FAIL" for item in results)
    warn = any(item.get("status") == "WARN" for item in results)
    payload = {
        "schema_version": 1,
        "tool": "audit_legacy_annotation_labels.py",
        "mode": "full" if args.full else "sample",
        "contract": {
            "y_ok": "blank black label",
            "y_ng": "multi-class RGB label with ignore/maybe semantics",
            "canonical": {name: list(rgb) for name, rgb in 標準色.items()},
            "note": "OpenCV Resize can create interpolated edge colors; non-canonical RGB is reported but not failed by default.",
        },
        "results": results,
        "overall": "FAIL" if fail else ("WARN" if warn else "PASS"),
    }
    print(json.dumps(payload, ensure_ascii=True, indent=2))
    return 1 if fail else 0


if __name__ == "__main__":
    sys.exit(main())
