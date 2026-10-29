"""Isolated CCD1 segmentation IoU benchmark, never a deployment authority.

Uses 4 manually selected held-out X/Y cropped pairs and the original vendor
HDF5/segmentation model or a candidate HDF5. Does not consume full-size
Origional camera images as segmentation ground truth.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import tempfile

import cv2
import numpy as np


HOLDOUT = (
    ("OK", "OK-1-2025-06-27-16-54-46.png"),
    ("OK", "OK-1-2025-06-27-16-58-19.png"),
    ("NG", "NG-1-2025-06-27-16-55-12.png"),
    ("NG", "NG-1-2025-06-27-16-55-24.png"),
)
MODEL_PATHS = {
    "vendor": "TrainingSeed/DIA_R002_vendor.h5",
    "ccd1-smoke": "TrainingCandidates/20261022T044553Z-1fa06812/candidate.h5",
    "ccd1-long300": "TrainingCandidates/20261022T050341Z-33c441fa/candidate.h5",
}
TRAIN300_MANIFEST = "TrainingPlans/DIA_CCD1_S1_Train300.json"
SOURCE_DATA = "TrainingImage/VPMGK05500VN0Y0BW-CCD1"
MAGIC = bytes((1, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 64))
HDF5 = bytes((137, 72, 68, 70, 13, 10, 26, 10))


def digest(path):
    h = hashlib.sha256()
    with open(path, "rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest().upper()


def load_segmentation_model(path):
    # Work with a stripped *temporary copy*; source H5 is SHA-pinned and
    # never edited in place. This matches ModelConverterWorker's H5 logic.
    import tensorflow as tf

    if not path.is_file():
        raise RuntimeError("Chosen candidate or vendor model is missing")
    with open(path, "rb") as stream:
        header = stream.read(24)
    if header != MAGIC + HDF5:
        raise RuntimeError("Expected original Delta wrapped HDF5 model")
    with tempfile.TemporaryDirectory(prefix="dia_ccd1_iou_") as temp:
        raw = Path(temp) / "model.h5"
        with open(path, "rb") as source, open(raw, "wb") as target:
            source.seek(16)
            shutil.copyfileobj(source, target)
        model = tf.keras.models.load_model(str(raw), compile=False)
    shape = model.input_shape
    if not isinstance(shape, (tuple, list)) or len(shape) != 4:
        raise RuntimeError("Unsupported model input shape: " + str(shape))
    if shape[1] != 256 or shape[2] != 256 or shape[3] != 3:
        raise RuntimeError("Expected original DAVS input 256x256x3, got " + str(shape))
    outshape = model.output_shape
    if not isinstance(outshape, (tuple, list)) or len(outshape) != 4:
        raise RuntimeError("Unsupported model output shape")
    if outshape[1] != 256 or outshape[2] != 256 or outshape[3] not in (1, 3):
        raise RuntimeError("Unsupported output shape: " + str(outshape))
    return model, shape, outshape


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--app", required=True)
    parser.add_argument("--model", choices=tuple(MODEL_PATHS), required=True)
    parser.add_argument("--split", choices=("holdout", "training"), default="holdout")
    parser.add_argument(
        "--metric-method", choices=("legacy-union", "shunt-preview-v2"),
        default="legacy-union"
    )
    parser.add_argument("--threshold", type=float, default=0.5)
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args()
    vendor_threshold = 220.0 / 255.0
    if not (abs(args.threshold - 0.5) < 1e-12 or
            abs(args.threshold - vendor_threshold) < 1e-12):
        raise RuntimeError("Only historical 0.5 or ProjectHub documented 220/255 thresholds are allowed")

    os.environ["CUDA_VISIBLE_DEVICES"] = "-1"  # GPU is reserved for DAVS trainer
    os.environ["TF_CPP_MIN_LOG_LEVEL"] = "2"
    root = Path(args.app).resolve()
    if not (root / "InspToolsRecipeCli.exe").is_file():
        raise RuntimeError("Expected standalone AppBase, not donor runtime")
    long_plan = json.loads((root / TRAIN300_MANIFEST).read_text(encoding="utf-8-sig"))
    if long_plan["dataset"] != "DIA_CCD1_S1_Train300":
        raise RuntimeError("Pinned training family manifest not found")
    reserved = set(long_plan["heldout_families"])
    families = {
        os.path.splitext(name)[0] for label, name in HOLDOUT
    }
    training = {entry["family"] for entry in long_plan["entries"]}
    if len(families) != 4 or not families.issubset(reserved) or families & training:
        raise RuntimeError("Leakage between IoU validation and real training set")
    if len(long_plan["entries"]) != 43:
        raise RuntimeError("Unexpected long training sample count")

    source_specs = []
    if args.split == "holdout":
        for label, filename in HOLDOUT:
            source_specs.append((
                label, filename,
                root / SOURCE_DATA / "X" / label / "ALL" / filename,
                root / SOURCE_DATA / "Y" / label / "ALL" / filename,
                None,
            ))
    else:
        for item in long_plan["entries"]:
            label, rel = item["label"], item["relative"]
            if label not in ("OK", "NG") or item["family"] in reserved:
                raise RuntimeError("Unexpected training family in manifest")
            source_specs.append((
                label, rel,
                root / "TrainingImage" / "DIA_CCD1_S1_Train300" / "X" / label / rel,
                root / "TrainingImage" / "DIA_CCD1_S1_Train300" / "Y" / label / rel,
                item,
            ))
        if len(source_specs) != 43:
            raise RuntimeError("Expected exactly 43 train X/Y pairs")
    rows = []
    for label, filename, x, y, manifest_item in source_specs:
        if not x.is_file() or not y.is_file():
            raise RuntimeError("Annotated X/Y pair missing: " + filename)
        if manifest_item and (
            digest(x) != manifest_item["x_sha256"] or
            digest(y) != manifest_item["y_sha256"]
        ):
            raise RuntimeError("Training data SHA-256 differs from locked manifest")
        # Windows OpenCV imread() does not reliably support Unicode paths.
        # imdecode from bytes preserves the genuine local X/Y source.
        x_raw = cv2.imdecode(np.fromfile(str(x), dtype=np.uint8), cv2.IMREAD_COLOR)
        y_raw = cv2.imdecode(np.fromfile(str(y), dtype=np.uint8), cv2.IMREAD_COLOR)
        if x_raw is None or y_raw is None or x_raw.shape != y_raw.shape:
            raise RuntimeError("Corrupt or mismatched held-out X/Y: " + filename)
        rows.append({
            "label": label,
            "filename": filename,
            "x_sha256": digest(x),
            "y_sha256": digest(y),
            "x_image": x_raw,
            "y_mask": y_raw,
        })

    model_path = root / MODEL_PATHS[args.model]
    sha_before = digest(model_path)
    model, input_shape, output_shape = load_segmentation_model(model_path)
    intersections = 0
    unions = 0
    samples = []
    for entry in rows:
        # ShuntVC's render_model_preview.py (2026-09-30) uses INTER_AREA,
        # prediction[..., 0], and non-zero label in any channel. The old
        # exploratory benchmark used INTER_LINEAR and channel-wise max;
        # preserve it as a separate method so earlier JSON is not rewritten.
        original_shunt = args.metric_method == "shunt-preview-v2"
        interpolation = cv2.INTER_AREA if original_shunt else cv2.INTER_LINEAR
        x = cv2.resize(entry["x_image"], (256, 256), interpolation=interpolation)
        x = cv2.cvtColor(x, cv2.COLOR_BGR2RGB).astype(np.float32) / 255.0
        y = cv2.resize(entry["y_mask"], (256, 256), interpolation=cv2.INTER_NEAREST)
        target = np.any(y > (0 if original_shunt else 127), axis=2)
        prediction = model.predict(x[np.newaxis, ...], verbose=0)[0]
        if not np.isfinite(prediction).all():
            raise RuntimeError("Model returned non-finite output: " + entry["filename"])
        if prediction.ndim == 3:
            prob = prediction[..., 0] if original_shunt else prediction.max(axis=2)
        else:
            raise RuntimeError("Unexpected tensor rank from model.predict")
        mask = prob >= args.threshold
        intersection = int(np.logical_and(mask, target).sum())
        union = int(np.logical_or(mask, target).sum())
        # Empty-empty is defined as IoU=1, but these samples are reported
        # separately from NG and never used to inflate the NG-only mean.
        iou = float(intersection / union) if union else 1.0
        samples.append({
            "label": entry["label"],
            "filename": entry["filename"],
            "x_sha256": entry["x_sha256"],
            "y_sha256": entry["y_sha256"],
            "iou": iou,
            "target_foreground_pixels": int(target.sum()),
            "predicted_foreground_pixels": int(mask.sum()),
            "intersection": intersection,
            "union": union,
            "prediction_min": float(prediction.min()),
            "prediction_max": float(prediction.max()),
        })
        if entry["label"] == "NG":
            intersections += intersection
            unions += union
    if digest(model_path) != sha_before:
        raise RuntimeError("Input model checksum changed during IoU evaluation")
    ng_rows = [item for item in samples if item["label"] == "NG"]
    ok_rows = [item for item in samples if item["label"] == "OK"]
    result = {
        "gate": "IOU_DIAGNOSTIC_ONLY_NOT_PRODUCTION_QUALIFIED",
        "model": args.model,
        "model_sha256": sha_before,
        "input_shape": str(input_shape),
        "output_shape": str(output_shape),
        "threshold": args.threshold,
        "threshold_provenance": (
            "ProjectHub Shunt-Training 2026-09-30, 220/255"
            if abs(args.threshold - vendor_threshold) < 1e-12
            else "initial simplified diagnostic 0.5"
        ),
        "metric_method": args.metric_method,
        "preprocessing": (
            "ShuntVC render_model_preview.py 2026-09-30: AREA resize RGB float32/255; nearest Y resize; any Y channel>0; prediction[...,0]>=threshold"
            if args.metric_method == "shunt-preview-v2"
            else "Legacy exploratory: OpenCV linear input 256x256 RGB float32/255; nearest Y resize; any Y channel>127; max predicted channel>=threshold"
        ),
        "preprocessing_verified_against_vendor_davs": False,
        "split": args.split,
        "holdout_only": args.split == "holdout",
        "ng_mean_iou": float(np.mean([v["iou"] for v in ng_rows])),
        "ng_micro_iou": float(intersections / unions) if unions else 1.0,
        "ok_mean_empty_mask_iou": float(np.mean([v["iou"] for v in ok_rows])),
        "all_mean_iou": float(np.mean([v["iou"] for v in samples])),
        "deployed": False,
        "samples": samples,
    }
    output = json.dumps(result, ensure_ascii=False, indent=2)
    print(output)
    if args.write:
        suffix = "_threshold220" if abs(args.threshold - vendor_threshold) < 1e-12 else ""
        if args.metric_method == "shunt-preview-v2":
            suffix += "_shunt_preview_v2"
        split_name = "holdout4" if args.split == "holdout" else "training43"
        dst = root / "TrainingEvaluations" / (
            "IoU_" + args.model + "_" + split_name + suffix + ".json"
        )
        dst.parent.mkdir(parents=True, exist_ok=True)
        if dst.exists():
            if dst.read_text(encoding="utf-8-sig") != output:
                raise RuntimeError("Existing IoU evidence differs; not overwritten")
        else:
            dst.write_text(output, encoding="utf-8")


if __name__ == "__main__":
    main()
