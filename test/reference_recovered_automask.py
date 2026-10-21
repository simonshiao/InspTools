from __future__ import annotations

import argparse
import json
import math
from pathlib import Path
import time

import cv2
import numpy as np
import onnxruntime as ort


TARGET = 1024
POINTS_PER_SIDE = 4
PRED_IOU_THRESHOLD = 0.88
STABILITY_THRESHOLD = 0.95
STABILITY_OFFSET = 1.0
MASK_THRESHOLD = 0.0


def preprocess(image: np.ndarray) -> tuple[np.ndarray, int, int]:
    old_h, old_w = image.shape[:2]
    scale = TARGET / max(old_h, old_w)
    new_w = int(old_w * scale + 0.5)
    new_h = int(old_h * scale + 0.5)
    resized = cv2.resize(image, (new_w, new_h)).astype(np.float32)
    mean, stddev = cv2.meanStdDev(resized)
    mean = mean.reshape(1, 1, 3).astype(np.float32)
    stddev = stddev.reshape(1, 1, 3).astype(np.float32)
    normalized = (resized - mean) / stddev
    chw = np.zeros((1, 3, TARGET, TARGET), dtype=np.float32)
    chw[0, :, :new_h, :new_w] = normalized.transpose(2, 0, 1)
    return chw, new_w, new_h


def point_grid(n: int) -> list[tuple[float, float]]:
    offset = 1.0 / (2 * n)
    values = [offset + i * (1.0 - 2.0 * offset) / (n - 1) for i in range(n)]
    return [(values[row], values[col]) for row in range(n) for col in range(n)]


def stability(mask: np.ndarray) -> float:
    upper = int(np.count_nonzero(mask > MASK_THRESHOLD + STABILITY_OFFSET))
    lower = int(np.count_nonzero(mask > MASK_THRESHOLD - STABILITY_OFFSET))
    return float(upper / lower) if lower else 0.0


def bbox(mask: np.ndarray) -> list[int]:
    ys, xs = np.where(mask > MASK_THRESHOLD)
    if xs.size == 0:
        return [0, 0, 0, 0]
    return [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())]


def full_class_mask(label_bgr: np.ndarray) -> np.ndarray:
    rgb = cv2.cvtColor(label_bgr, cv2.COLOR_BGR2RGB)
    r = rgb[:, :, 0]
    g = rgb[:, :, 1]
    b = rgb[:, :, 2]
    return (
        ((r >= 128) & (g <= 10) & (b <= 10))
        | ((g >= 128) & (r <= 10) & (b <= 10))
        | ((b >= 128) & (r <= 10) & (g <= 10))
        | ((r >= 128) & (g >= 128) & (b <= 10))
        | ((r >= 128) & (b >= 128) & (g <= 10))
        | ((g >= 128) & (b >= 128) & (r <= 10))
    )


def overlap_metrics(candidate: np.ndarray, truth: np.ndarray) -> dict[str, float | int]:
    intersection = int(np.count_nonzero(candidate & truth))
    candidate_pixels = int(np.count_nonzero(candidate))
    truth_pixels = int(np.count_nonzero(truth))
    union = candidate_pixels + truth_pixels - intersection
    return {
        "labelIntersection": intersection,
        "labelIou": float(intersection / union) if union else 0.0,
        "labelPrecision": float(intersection / candidate_pixels) if candidate_pixels else 0.0,
        "labelRecall": float(intersection / truth_pixels) if truth_pixels else 0.0,
    }


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("image")
    ap.add_argument("encoder")
    ap.add_argument("decoder")
    ap.add_argument("--label", default="")
    args = ap.parse_args()

    image_path = Path(args.image).resolve()
    encoder_path = Path(args.encoder).resolve()
    decoder_path = Path(args.decoder).resolve()
    for path in (image_path, encoder_path, decoder_path):
        if not path.is_file():
            raise FileNotFoundError(path)

    # Unicode-safe load, matching the rebuilt C# backend's ImDecode byte path.
    raw = np.fromfile(str(image_path), dtype=np.uint8)
    image = cv2.imdecode(raw, cv2.IMREAD_COLOR)
    if image is None:
        raise RuntimeError(f"cannot decode image: {image_path}")
    height, width = image.shape[:2]
    input_tensor, new_w, new_h = preprocess(image)

    truth = None
    truth_path = None
    if args.label:
        truth_path = Path(args.label).resolve()
        if not truth_path.is_file():
            raise FileNotFoundError(truth_path)
        label_raw = np.fromfile(str(truth_path), dtype=np.uint8)
        label = cv2.imdecode(label_raw, cv2.IMREAD_COLOR)
        if label is None or label.shape[:2] != image.shape[:2]:
            raise RuntimeError("label cannot be decoded or dimensions do not match image")
        truth = full_class_mask(label)

    sess_opts = ort.SessionOptions()
    sess_opts.enable_mem_pattern = False
    sess_opts.enable_cpu_mem_arena = False
    encoder = ort.InferenceSession(str(encoder_path), sess_options=sess_opts, providers=["CPUExecutionProvider"])
    decoder = ort.InferenceSession(str(decoder_path), sess_options=sess_opts, providers=["CPUExecutionProvider"])

    t0 = time.perf_counter()
    embedding = encoder.run(["image_embeddings"], {"x": input_tensor})[0]
    encode_ms = (time.perf_counter() - t0) * 1000.0

    accepted: list[dict] = []
    candidate_rows: list[dict] = []
    decode_ms = 0.0
    points = point_grid(POINTS_PER_SIDE)
    for point_index, (nx, ny) in enumerate(points):
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

        masks = masks[0]
        ious = ious[0]
        for mask_index in range(masks.shape[0]):
            mask = masks[mask_index]
            score = float(ious[mask_index])
            stable = stability(mask)
            positive = int(np.count_nonzero(mask > MASK_THRESHOLD))
            row = {
                "pointIndex": point_index,
                "point": [x, y],
                "decoderPoint": [tx, ty],
                "maskIndex": mask_index,
                "iou": score,
                "stability": stable,
                "positivePixels": positive,
                "box": bbox(mask),
                "accepted": score > PRED_IOU_THRESHOLD and stable > STABILITY_THRESHOLD,
            }
            if truth is not None:
                row.update(overlap_metrics(mask > MASK_THRESHOLD, truth))
            candidate_rows.append(row)
            if row["accepted"]:
                accepted.append(row)

    result = {
        "status": "PASS",
        "contract": "Recovered SAMAutoMask defaults; no NMS/crop postprocess inferred",
        "image": str(image_path),
        "shape": [width, height],
        "preprocessShape": [new_w, new_h],
        "pointsPerSide": POINTS_PER_SIDE,
        "pointCount": len(points),
        "candidateCount": len(candidate_rows),
        "thresholds": {
            "predIou": PRED_IOU_THRESHOLD,
            "stability": STABILITY_THRESHOLD,
            "stabilityOffset": STABILITY_OFFSET,
            "mask": MASK_THRESHOLD,
        },
        "acceptedCount": len(accepted),
        "accepted": accepted,
        "encodeMs": encode_ms,
        "decodeTotalMs": decode_ms,
        "decodeAverageMs": decode_ms / len(points),
        "allCandidates": candidate_rows,
    }
    if truth is not None:
        ranked = sorted(candidate_rows, key=lambda item: item["labelIou"], reverse=True)
        result["groundTruth"] = {
            "label": str(truth_path),
            "fullClassPixels": int(np.count_nonzero(truth)),
            "bestCandidateLabelIou": float(ranked[0]["labelIou"]) if ranked else 0.0,
            "bestCandidateLabelPrecision": float(ranked[0]["labelPrecision"]) if ranked else 0.0,
            "bestCandidateLabelRecall": float(ranked[0]["labelRecall"]) if ranked else 0.0,
            "topCandidates": ranked[:10],
        }
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
