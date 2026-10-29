"""CHAO 300E research-candidate offline mask / ROI preview.

Preprocessing matches AgentVisionLab.trainors.temp_unet_trainer.read_rgb:
cv2.imdecode BGR -> RGB, resize INTER_LINEAR, float32 /255.
Never writes original InspTools CVBuffer, Recipe, ROI parameter or Inspect flags.
"""
import argparse
import hashlib
import json
import time
from pathlib import Path

import cv2
import numpy as np
import tensorflow as tf


PINNED = "3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634"


def verify_model(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    if digest.hexdigest().upper() != PINNED:
        raise RuntimeError("Model SHA mismatch; refuse inference")


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--model", required=True)
    p.add_argument("--image", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--threshold", type=int, default=220)
    p.add_argument("--min-area", type=int, default=10)
    args = p.parse_args()
    model_path = Path(args.model).resolve(strict=True)
    image_path = Path(args.image).resolve(strict=True)
    output_dir = Path(args.output).resolve()
    if not (1 <= args.threshold <= 255) or args.min_area < 1:
        raise ValueError("Invalid segmentation threshold / minimum area")
    if output_dir.exists():
        if any(output_dir.iterdir()):
            raise RuntimeError("Refuse to overwrite existing preview artifacts")
    output_dir.mkdir(parents=True, exist_ok=True)
    verify_model(model_path)

    raw = np.fromfile(str(image_path), dtype=np.uint8)
    bgr = cv2.imdecode(raw, cv2.IMREAD_COLOR)
    if bgr is None:
        raise ValueError("Cannot decode source image")
    original_h, original_w = bgr.shape[:2]
    model = tf.keras.models.load_model(str(model_path), compile=False)
    shape = model.input_shape
    output_shape = model.output_shape
    if tuple(shape[1:]) != (256, 256, 3) or tuple(output_shape[1:]) != (256, 256, 3):
        raise RuntimeError("Unexpected 300E model shape: %r -> %r" % (shape, output_shape))
    resized = cv2.resize(bgr, (256, 256), interpolation=cv2.INTER_LINEAR)
    rgb = resized[..., [2, 1, 0]].copy()
    started = time.perf_counter()
    prob = model(np.asarray(rgb[None, ...], dtype=np.float32) / 255., training=False).numpy()[0]
    elapsed_ms = (time.perf_counter() - started) * 1000.
    mask = np.clip(prob[..., 0] * 255., 0, 255).astype(np.uint8)
    binary = np.asarray(mask >= args.threshold, dtype=np.uint8)
    count, labels, stats, centroids = cv2.connectedComponentsWithStats(binary, 8)
    areas = [(int(stats[i, cv2.CC_STAT_AREA]), i) for i in range(1, count)
             if int(stats[i, cv2.CC_STAT_AREA]) >= args.min_area]
    areas.sort(reverse=True)
    boxes = []
    overlay = bgr.copy()
    for area, i in areas:
        x, y, w, h = [int(v) for v in stats[i, :4]]
        # Inference is 256x256; ROI must be transformed back to original pixels.
        xywh = [int(round(x * original_w / 256.)),
                int(round(y * original_h / 256.)),
                int(round(w * original_w / 256.)),
                int(round(h * original_h / 256.))]
        boxes.append({"area_256": area, "xywh_256": [x, y, w, h],
                      "xywh_original": xywh})
        x0, y0, ow, oh = xywh
        cv2.rectangle(overlay, (x0, y0), (x0 + ow, y0 + oh), (0, 0, 255), 2)
    # Separate preview-only sidecar files; never set native Inspect/OK/NG.
    if not cv2.imencode(".png", mask)[0]:
        raise RuntimeError("Mask PNG encoding failed")
    np.asarray(cv2.imencode(".png", mask)[1]).tofile(str(output_dir / "locate-mask.png"))
    np.asarray(cv2.imencode(".png", overlay)[1]).tofile(str(output_dir / "locate-overlay.png"))
    report = {
        "status": "INFERENCE_SMOKE_PASS",
        "model_sha256": PINNED,
        "model_300e_r_iou_reference": 0.7770970782280867,
        "source_image": str(image_path),
        "image_original_wh": [original_w, original_h],
        "model_wh": [256, 256],
        "elapsed_ms_first_forward": elapsed_ms,
        "threshold_uint8": args.threshold,
        "min_area": args.min_area,
        "r_mask_nonzero_thresholded": int(np.count_nonzero(binary)),
        "roi_count": len(boxes),
        "rois": boxes,
        "source_isolated": True,
        "native_cvbuffer_updated": False,
        "inspect_result_updated": False,
        "production_qualified": False,
    }
    (output_dir / "locate-report.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print("CHAO_LOCATE_SMOKE=PASS ROI_COUNT=%d THRESHOLD=%d MS=%.1f" %
          (len(boxes), args.threshold, elapsed_ms), flush=True)


if __name__ == "__main__":
    main()
