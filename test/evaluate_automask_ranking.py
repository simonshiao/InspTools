from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys
import time

import cv2
import numpy as np
import onnxruntime as ort

sys.path.insert(0, str(Path(__file__).resolve().parent))
import reference_recovered_automask as ref


IMAGE_EXTS = {".png", ".bmp", ".jpg", ".jpeg", ".tif", ".tiff"}


def find_pairs(root: Path) -> list[tuple[str, Path, Path]]:
    result: list[tuple[str, Path, Path]] = []
    for model_dir in sorted(p for p in root.iterdir() if p.is_dir()):
        xroot = model_dir / "X" / "NG"
        yroot = model_dir / "Y" / "NG"
        if not xroot.is_dir() or not yroot.is_dir():
            continue
        y_by_name: dict[str, list[Path]] = {}
        for y in yroot.rglob("*"):
            if y.is_file() and y.suffix.lower() in IMAGE_EXTS:
                y_by_name.setdefault(y.name.lower(), []).append(y)
        for x in xroot.rglob("*"):
            if not x.is_file() or x.suffix.lower() not in IMAGE_EXTS:
                continue
            ys = y_by_name.get(x.name.lower(), [])
            if len(ys) == 1:
                result.append((model_dir.name, x, ys[0]))
    return result


def load_image(path: Path) -> np.ndarray:
    raw = np.fromfile(str(path), dtype=np.uint8)
    image = cv2.imdecode(raw, cv2.IMREAD_COLOR)
    if image is None:
        raise RuntimeError(f"cannot decode: {path}")
    return image


def scan_candidates(
    image: np.ndarray,
    truth: np.ndarray,
    encoder: ort.InferenceSession,
    decoder: ort.InferenceSession,
) -> tuple[list[dict], float, float]:
    height, width = image.shape[:2]
    input_tensor, new_w, new_h = ref.preprocess(image)
    t0 = time.perf_counter()
    embedding = encoder.run(["image_embeddings"], {"x": input_tensor})[0]
    encode_ms = (time.perf_counter() - t0) * 1000.0

    rows: list[dict] = []
    decode_ms = 0.0
    pixels = width * height
    for point_index, (nx, ny) in enumerate(ref.point_grid(ref.POINTS_PER_SIDE)):
        x = int(nx * width)
        y = int(ny * height)
        tx = int(x * new_w / width)
        ty = int(y * new_h / height)
        inputs = {
            "image_embeddings": embedding,
            "point_coords": np.asarray([[[tx, ty]]], dtype=np.float32),
            "point_labels": np.asarray([[1.0]], dtype=np.float32),
            "mask_input": np.zeros((1, 1, 256, 256), dtype=np.float32),
            "has_mask_input": np.zeros((1,), dtype=np.float32),
            "orig_im_size": np.asarray([height, width], dtype=np.float32),
        }
        t1 = time.perf_counter()
        masks, ious, _ = decoder.run(["masks", "iou_predictions", "low_res_masks"], inputs)
        decode_ms += (time.perf_counter() - t1) * 1000.0
        for mask_index, mask in enumerate(masks[0]):
            candidate = mask > ref.MASK_THRESHOLD
            positive = int(np.count_nonzero(candidate))
            coverage = positive / pixels if pixels else 0.0
            row = {
                "pointIndex": point_index,
                "point": [x, y],
                "maskIndex": mask_index,
                "score": float(ious[0][mask_index]),
                "stability": ref.stability(mask),
                "coverage": float(coverage),
                "positivePixels": positive,
                "box": ref.bbox(mask),
            }
            row.update(ref.overlap_metrics(candidate, truth))
            row["recoveredAccepted"] = (
                row["score"] > ref.PRED_IOU_THRESHOLD
                and row["stability"] > ref.STABILITY_THRESHOLD
            )
            rows.append(row)
    return rows, encode_ms, decode_ms


def pick(rows: list[dict], mode: str) -> dict | None:
    nonempty = [row for row in rows if row["positivePixels"] > 0]
    if mode == "oracle":
        pool = nonempty
        key = lambda row: row["labelIou"]
    elif mode == "recovered":
        pool = [row for row in nonempty if row["recoveredAccepted"]]
        key = lambda row: row["score"]
    elif mode == "safe_score":
        pool = [row for row in nonempty if 0.001 <= row["coverage"] <= 0.50]
        key = lambda row: row["score"]
    elif mode == "safe_score_stability":
        pool = [row for row in nonempty if 0.001 <= row["coverage"] <= 0.50]
        key = lambda row: row["score"] * row["stability"]
    elif mode == "safe_compact":
        pool = [row for row in nonempty if 0.001 <= row["coverage"] <= 0.50]
        key = lambda row: row["score"] * row["stability"] * (1.0 - row["coverage"])
    else:
        raise ValueError(mode)
    return max(pool, key=key) if pool else None


def metric_summary(values: list[dict], mode: str) -> dict:
    selected = [row["selected"][mode] for row in values if row["selected"][mode] is not None]
    if not selected:
        return {"count": 0}
    return {
        "count": len(selected),
        "meanLabelIou": float(np.mean([row["labelIou"] for row in selected])),
        "meanPrecision": float(np.mean([row["labelPrecision"] for row in selected])),
        "meanRecall": float(np.mean([row["labelRecall"] for row in selected])),
        "meanCoverage": float(np.mean([row["coverage"] for row in selected])),
    }


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("training_root")
    ap.add_argument("encoder")
    ap.add_argument("decoder")
    ap.add_argument("--sample", type=int, default=6)
    args = ap.parse_args()

    root = Path(args.training_root).resolve()
    pairs = find_pairs(root)
    if not pairs:
        raise RuntimeError("no unambiguous NG X/Y pairs found")
    count = min(max(args.sample, 1), len(pairs))
    if count == 1:
        chosen = [pairs[0]]
    else:
        # Deterministic spread through the available pair list instead of only
        # sampling one caption folder / acquisition burst.
        indexes = np.linspace(0, len(pairs) - 1, count, dtype=int)
        chosen = [pairs[int(i)] for i in indexes]

    opts = ort.SessionOptions()
    opts.enable_mem_pattern = False
    opts.enable_cpu_mem_arena = False
    encoder = ort.InferenceSession(str(Path(args.encoder).resolve()), sess_options=opts, providers=["CPUExecutionProvider"])
    decoder = ort.InferenceSession(str(Path(args.decoder).resolve()), sess_options=opts, providers=["CPUExecutionProvider"])

    modes = ["oracle", "recovered", "safe_score", "safe_score_stability", "safe_compact"]
    rows: list[dict] = []
    for model, x_path, y_path in chosen:
        image = load_image(x_path)
        label = load_image(y_path)
        if label.shape[:2] != image.shape[:2]:
            continue
        truth = ref.full_class_mask(label)
        truth_pixels = int(np.count_nonzero(truth))
        if truth_pixels == 0:
            continue
        candidates, encode_ms, decode_ms = scan_candidates(image, truth, encoder, decoder)
        selected = {mode: pick(candidates, mode) for mode in modes}
        rows.append(
            {
                "model": model,
                "x": str(x_path),
                "y": str(y_path),
                "truthPixels": truth_pixels,
                "encodeMs": encode_ms,
                "decodeMs": decode_ms,
                "selected": selected,
            }
        )

    output = {
        "status": "PASS",
        "availablePairs": len(pairs),
        "requestedSample": count,
        "evaluated": len(rows),
        "heuristics": {mode: metric_summary(rows, mode) for mode in modes},
        "images": rows,
    }
    print(json.dumps(output, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
