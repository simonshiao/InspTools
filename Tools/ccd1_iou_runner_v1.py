"""DIA CCD1 / AgentVisionLab Loss6: independent IoU-selected checkpoint runner.

Never touches original ShuntVC scripts, existing 300/500 jobs, or release/current.
Historical holdout is used for development selection only; separate test batches
are still required before production qualification.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import time
from pathlib import Path, PureWindowsPath

TRAINER_SHA = "7B0E07187928F410922D93EF19D052D6FEB7E6C641A05D8A24574A2FEF104913"
VENDOR_SEED_SHA = "99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73"
THRESHOLD = 220.0 / 255.0
HOLDOUT_PINS = {
    "OK-1-2025-06-27-16-54-46": (
        "42E956850A6B8C0CED5B61EF13B07721E89676703EC3B345B41C7337590A70AE",
        "11F43A478AB7551D4119DBAC67E1BD31EE9FE6C4C0979C6050DC9B40D8A77C39",
    ),
    "OK-1-2025-06-27-16-58-19": (
        "9CA7818D6E1906F7E6E9E19C102F098426EEF64F6F44D9D95409C6D7FE57F14D",
        "33AC6D362A3B12044ABB10F6C0F517EF40DC5FE6D083CBCC734669F0CBA646CE",
    ),
    "NG-1-2025-06-27-16-55-12": (
        "46C3FBDA62B31E7B04969BED7AB010B25DECFA2E4DE3E92A17D34BA6629052D6",
        "A701AFC4E5177493654F4E2D351AEB981E4B0FC54BD24CA09C7556ADC50C106A",
    ),
    "NG-1-2025-06-27-16-55-24": (
        "A33E32830E77080CC8035F0D10AB260BD5C6A60A6D440135620652AE14F28440",
        "81BB8C18093CD307069D0D6490E18434052B95C928FE138AC587B7556FEED4CA",
    ),
}


def sha256(file: Path) -> str:
    digest = hashlib.sha256()
    with file.open("rb") as stream:
        for piece in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(piece)
    return digest.hexdigest().upper()


def checked_hash(file: Path, expected: str) -> None:
    if not file.is_file() or sha256(file) != expected.upper():
        raise ValueError(f"SHA-256 mismatch or missing file: {file}")


def check_data(app: Path) -> dict:
    """Fail closed: source bytes, X/Y count, original families, no leakage."""
    plan_file = app / "TrainingPlans" / "DIA_CCD1_S1_Train300.json"
    plan = json.loads(plan_file.read_text(encoding="utf-8-sig"))
    train_root = app / "TrainingImage" / "DIA_CCD1_S1_Train300"
    val_root = app / "TrainingImage" / "DIA_CCD1_S1_Holdout4"
    entries = plan["entries"]
    families = {row["family"] for row in entries}
    holdout = set(plan["heldout_families"])
    if (plan["dataset"] != "DIA_CCD1_S1_Train300"
            or plan["deployed"] is not False
            or len(entries) != 43
            or sum(row["label"] == "NG" for row in entries) != 24
            or sum(row["label"] == "OK" for row in entries) != 19
            or len(families) != 25
            or families & holdout
            or holdout != set(HOLDOUT_PINS)):
        raise ValueError("CCD1 family-isolated training manifest changed")
    expected_paths = set()
    train_x_hashes = set()
    for row in entries:
        label = row["label"]
        if label not in {"OK", "NG"}:
            raise ValueError("Unknown label in plan")
        parts = PureWindowsPath(row["relative"]).parts
        if (not parts or any(part in {".", ".."} for part in parts)
                or any(":" in part for part in parts)):
            raise ValueError("Unsafe X/Y relative path")
        for axis in ("X", "Y"):
            file = train_root.joinpath(axis, label, *parts)
            checked_hash(file, row[axis.lower() + "_sha256"])
            expected_paths.add(file.resolve())
            if axis == "X":
                train_x_hashes.add(row["x_sha256"].upper())
    actual_paths = {
        file.resolve()
        for axis in ("X", "Y")
        for file in (train_root / axis).rglob("*")
        if file.is_file()
    }
    if actual_paths != expected_paths:
        raise ValueError("Unexpected or missing training X/Y files")

    validation_paths = []
    for family, hashes in HOLDOUT_PINS.items():
        label = family[:2]
        for axis, expected in zip(("X", "Y"), hashes):
            file = val_root / axis / label / (family + ".png")
            checked_hash(file, expected)
            validation_paths.append(file)
            if axis == "X" and expected in train_x_hashes:
                raise ValueError(f"Train/validation bytes overlap: {family}")
    actual_validation = {
        file.resolve()
        for axis in ("X", "Y")
        for file in (val_root / axis).rglob("*")
        if file.is_file()
    }
    if actual_validation != {file.resolve() for file in validation_paths}:
        raise ValueError("Unexpected holdout files")
    digest_rows = [(row["family"], row["relative"],
                    row["x_sha256"], row["y_sha256"]) for row in entries]
    return {
        "training_root": str(train_root),
        "holdout_root": str(val_root),
        "training_pairs": len(entries),
        "training_families": len(families),
        "training_ng_families": len({r["family"] for r in entries if r["label"] == "NG"}),
        "holdout_pairs": len(HOLDOUT_PINS),
        "holdout_ng_families": 2,
        "manifest_sha256": sha256(plan_file),
        "training_contract_sha256": hashlib.sha256(
            json.dumps(digest_rows, ensure_ascii=False).encode("utf-8")
        ).hexdigest().upper(),
        "holdout_contract_sha256": hashlib.sha256(
            json.dumps(HOLDOUT_PINS, sort_keys=True).encode("ascii")
        ).hexdigest().upper(),
        "overlapping_families": 0,
        "production_qualified": False,
    }


def calculate_metrics(pred, truth, threshold: float = THRESHOLD,
                      min_area: int = 10) -> dict:
    """Same RGB threshold / raw-pixel IoU as Shunt 9/30 quality_gate.py."""
    import cv2
    import numpy as np

    pbin, tbin = np.asarray(pred) >= threshold, np.asarray(truth) >= threshold
    if pbin.shape != tbin.shape or pbin.ndim != 4 or pbin.shape[-1] != 3:
        raise ValueError("Expected matching NHWC RGB predictions and masks")
    channels = []
    for index, channel in enumerate("RGB"):
        p = pbin[..., index]
        t = tbin[..., index]
        tp = int(np.logical_and(p, t).sum())
        fp = int(np.logical_and(p, ~t).sum())
        fn = int(np.logical_and(~p, t).sum())
        channels.append({
            "channel": channel, "tp": tp, "fp": fp, "fn": fn,
            "iou": tp / (tp + fp + fn) if tp + fp + fn else 0.0,
            "precision": tp / (tp + fp) if tp + fp else 0.0,
            "recall": tp / (tp + fn) if tp + fn else 0.0,
        })
    pred_ng = []
    for mask in np.any(pbin, axis=3):
        count, _, stats, _ = cv2.connectedComponentsWithStats(
            mask.astype(np.uint8), connectivity=8)
        pred_ng.append(any(int(stats[i, cv2.CC_STAT_AREA]) >= min_area
                           for i in range(1, count)))
    true_ng = np.any(tbin, axis=(1, 2, 3))
    pred_ng = np.asarray(pred_ng, dtype=bool)
    return {
        "channels": channels,
        "image_decision": {
            "tp_ng": int(np.logical_and(pred_ng, true_ng).sum()),
            "fp_ng": int(np.logical_and(pred_ng, ~true_ng).sum()),
            "fn_ng": int(np.logical_and(~pred_ng, true_ng).sum()),
            "tn_ok": int(np.logical_and(~pred_ng, ~true_ng).sum()),
        },
    }


def is_better(row: dict, best: dict | None) -> bool:
    """Prefer higher holdout R IoU, then recall, then earlier epoch."""
    if best is None:
        return True
    def rank(item):
        red = item["channels"][0]
        return (red["iou"], red["recall"], -item["epoch"])
    return rank(row) > rank(best)


def run_training(app: Path, arguments, provenance: dict) -> dict:
    """Only an explicitly named new candidate directory can receive writes."""
    if not re.fullmatch(r"[A-Za-z0-9_-]{8,90}", arguments.job_name):
        raise ValueError("Supply unique ASCII --job-name (8..90 chars)")
    if not 1 <= arguments.checkpoint_every <= arguments.epochs:
        raise ValueError("Invalid checkpoint cadence")
    if not 1 <= arguments.batch <= 128 or arguments.epochs > 1000:
        raise ValueError("Invalid training batch or epoch count")
    job = app / "TrainingCandidates" / arguments.job_name
    if job.exists():
        raise FileExistsError(f"Job exists, refusing overwrite: {job}")
    trainer_file = app / "Runtime" / "AgentVisionLab" / "trainers" / "temp_unet_trainer.py"
    checked_hash(trainer_file, TRAINER_SHA)
    weights = Path(arguments.resume_weights).resolve()
    checked_hash(weights, arguments.expected_resume_sha)
    # Reuse pinned vendor trainer as a *library*; do not patch the source or
    # override its historical loss-based checkpoints.
    sys.path.insert(0, str(trainer_file.parent))
    os.environ.setdefault("TF_FORCE_GPU_ALLOW_GROWTH", "true")
    os.environ.setdefault("CUDA_VISIBLE_DEVICES", "0")
    import numpy as np
    import tensorflow as tf
    import temp_unet_trainer as vendor

    np.random.seed(arguments.seed)
    tf.random.set_seed(arguments.seed)
    if not tf.config.list_physical_devices("GPU"):
        raise RuntimeError("GPU required for reproducible CCD1 training")
    job.mkdir(parents=True, exist_ok=False)
    state = job / "state.txt"
    state.write_text("STATE=RUNNING\n", encoding="utf-8")
    try:
        train_root = Path(provenance["training_root"])
        val_root = Path(provenance["holdout_root"])
        seq, manifest, labels, cache, count = vendor.prepare_training_data(
            train_root, 256, 256, 0, arguments.batch, True)
        xval, yval, val_manifest = vendor.load_dataset(val_root, 256, 256, 0)
        if count != 43 or len(xval) != 4:
            raise ValueError("Train/validation cardinality mismatch")
        xval = xval.astype(np.float32) / 255.0
        yval = yval.astype(np.float32) / 255.0
        tf.keras.backend.clear_session()
        checkpoint, _ = vendor.prepare_keras_resume_checkpoint(weights, job)
        model = vendor.build_temp_unet_us(256, 256, unet_div=4)
        # The original DAVS can be either full Keras H5 or weights-only H5.
        try:
            reference = tf.keras.models.load_model(str(checkpoint), compile=False)
        except (OSError, ValueError, TypeError):
            model.load_weights(str(checkpoint))
        else:
            if (reference.input_shape != model.input_shape
                    or reference.output_shape != model.output_shape):
                raise ValueError("Seed checkpoint architecture mismatch")
            model.set_weights(reference.get_weights())
            del reference
        model.compile(
            optimizer=tf.keras.optimizers.Adam(
                learning_rate=arguments.learning_rate, amsgrad=True),
            loss=vendor.davs_loss6)
        progress = vendor.Progress(
            arguments.epochs, job, 0.0, unet_div=4)
        best_path = job / "best_holdout_r_iou.weights.h5"

        class IoUCheckpoint(tf.keras.callbacks.Callback):
            def __init__(self):
                super().__init__()
                self.history = []
                self.best = None

            def on_epoch_end(self, epoch, logs=None):
                number = int(epoch) + 1
                if number % arguments.checkpoint_every and number != arguments.epochs:
                    return
                preds = []
                for index in range(0, len(xval), arguments.batch):
                    block = tf.convert_to_tensor(xval[index:index + arguments.batch])
                    preds.append(self.model(block, training=False).numpy())
                score = calculate_metrics(
                    np.concatenate(preds, axis=0), yval,
                    threshold=THRESHOLD, min_area=10)
                row = {
                    "epoch": number,
                    "training_loss": float((logs or {})["loss"]),
                    **score,
                }
                if is_better(row, self.best):
                    # Do not corrupt the previous best if interrupted mid-write.
                    staging = job / "new_best.tmp.weights.h5"
                    self.model.save_weights(str(staging))
                    os.replace(staging, best_path)
                    self.best = row
                    print(f"CCD1_IOU_BEST epoch={number} "
                          f"r_iou={row['channels'][0]['iou']:.6f}", flush=True)
                self.history.append(row)
                (job / "iou_progress.json").write_text(json.dumps({
                    "metric": "independent_holdout_R_IoU",
                    "threshold": THRESHOLD,
                    "min_area": 10,
                    "checkpoint_every": arguments.checkpoint_every,
                    "best_epoch": self.best["epoch"],
                    "best_r_iou": self.best["channels"][0]["iou"],
                    "epochs_evaluated": len(self.history),
                    "history": self.history,
                    "production_qualified": False,
                }, ensure_ascii=False, indent=2), encoding="utf-8")

        observer = IoUCheckpoint()
        class SafetyStop(tf.keras.callbacks.Callback):
            def __init__(self):
                super().__init__()
                self.started = time.monotonic()

            def verify(self):
                if (job / "stop.request").exists():
                    raise RuntimeError("Job stopped by stop.request")
                if time.monotonic() - self.started > arguments.max_seconds:
                    raise TimeoutError("Job exceeded maximum training time")

            def on_epoch_begin(self, epoch, logs=None):
                self.verify()

            def on_epoch_end(self, epoch, logs=None):
                self.verify()

        (job / "input_provenance.json").write_text(json.dumps({
            **provenance,
            "source_trainer_sha256": TRAINER_SHA,
            "resume_weights_sha256": arguments.expected_resume_sha.upper(),
            "seed": arguments.seed,
            "loss": 6,
            "learning_rate": arguments.learning_rate,
            "batch": arguments.batch,
            "epochs": arguments.epochs,
            "checkpoint_every": arguments.checkpoint_every,
            "max_seconds": arguments.max_seconds,
            "checkpoint_metric": "holdout_R_iou",
            "threshold": THRESHOLD,
            "min_area": 10,
            "code_version": "ccd1_iou_runner_v1",
            "runner_sha256": sha256(Path(__file__)),
            "deployed": False,
        }, ensure_ascii=False, indent=2), encoding="utf-8")
        model.fit(seq, epochs=arguments.epochs, verbose=0,
                  workers=1, use_multiprocessing=False, max_queue_size=2,
                  callbacks=[SafetyStop(), progress, observer])
        if observer.best is None or not best_path.is_file():
            raise RuntimeError("No validated checkpoint, refusing model export")
        model.load_weights(str(best_path))
        selected = job / "selected_by_holdout_iou.h5"
        model.save(str(selected), include_optimizer=False)
        result = {
            "status": "PASS_RESEARCH_ONLY",
            "epochs_completed": progress.current_epoch,
            "best_epoch_by_training_loss": vendor.global_best_epoch(
                progress.losses, progress.loss_history_start_epoch),
            "best_training_loss": min(progress.losses),
            "selected_epoch_by_holdout_iou": observer.best["epoch"],
            "selected_holdout_metrics": observer.best,
            "selected_h5_sha256": sha256(selected),
            "selected_weights_sha256": sha256(best_path),
            "epochs_evaluated": len(observer.history),
            "source_trainer_sha256": TRAINER_SHA,
            "training_manifest_sha256": provenance["manifest_sha256"],
            "holdout_contract_sha256": provenance["holdout_contract_sha256"],
            "production_qualified": False, "deployed": False,
            "qualification": "BLOCKED_INDEPENDENT_TEST_NOT_AVAILABLE",
        }
        (job / "training_result.json").write_text(
            json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
        state.write_text("STATE=SUCCEEDED_RESEARCH_ONLY\n", encoding="utf-8")
        return result
    except BaseException:
        state.write_text("STATE=FAILED_NO_DEPLOY\n", encoding="utf-8")
        raise


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--app", required=True)
    parser.add_argument("--audit-only", action="store_true")
    parser.add_argument("--job-name", default="")
    parser.add_argument("--resume-weights", default="")
    parser.add_argument("--expected-resume-sha", default=VENDOR_SEED_SHA)
    parser.add_argument("--epochs", type=int, default=300)
    parser.add_argument("--checkpoint-every", type=int, default=20)
    parser.add_argument("--batch", type=int, default=8)
    parser.add_argument("--learning-rate", type=float, default=0.001)
    parser.add_argument("--seed", type=int, default=260910)
    parser.add_argument("--max-seconds", type=int, default=1800)
    args = parser.parse_args()
    if args.max_seconds < 1:
        parser.error("--max-seconds must be positive")
    app = Path(args.app).resolve()
    provenance = check_data(app)
    if args.audit_only:
        print(json.dumps({"status": "DATA_AUDIT_PASS", **provenance},
                         ensure_ascii=False, indent=2), flush=True)
        return 0
    if not args.resume_weights or not args.job_name:
        parser.error("Training requires --job-name and --resume-weights")
    result = run_training(app, args, provenance)
    print(json.dumps(result, ensure_ascii=False, indent=2), flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
