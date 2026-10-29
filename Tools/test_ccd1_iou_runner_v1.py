"""Small CPU-only contract tests. Does not start training or write models."""

import unittest
import numpy as np

from ccd1_iou_runner_v1 import calculate_metrics, is_better, THRESHOLD


class IoUCheckpointTests(unittest.TestCase):
    def test_rgb_metrics_and_min_area(self):
        truth = np.zeros((2, 16, 16, 3), dtype=np.float32)
        pred = np.zeros_like(truth)
        truth[0, 2:6, 2:6, 0] = 1.0
        pred[0, 2:6, 2:5, 0] = 1.0  # 12 TP, 4 FN
        pred[1, 0:2, 0:2, 0] = 1.0  # 4 FP but no >=10 component
        result = calculate_metrics(pred, truth, THRESHOLD, 10)
        red = result["channels"][0]
        self.assertEqual((red["tp"], red["fp"], red["fn"]), (12, 4, 4))
        self.assertAlmostEqual(red["iou"], 0.6)
        self.assertAlmostEqual(red["precision"], 0.75)
        self.assertAlmostEqual(red["recall"], 0.75)
        self.assertEqual(result["channels"][1]["iou"], 0.0)
        self.assertEqual(
            result["image_decision"],
            {"tp_ng": 1, "fp_ng": 0, "fn_ng": 0, "tn_ok": 1},
        )

    def test_selection_prefers_iou_then_recall_then_earliest(self):
        def row(epoch, iou, recall):
            return {"epoch": epoch, "channels": [{"iou": iou, "recall": recall}]}
        best = row(20, 0.78, 0.80)
        self.assertFalse(is_better(row(40, 0.57, 0.99), best))
        self.assertTrue(is_better(row(40, 0.79, 0.70), best))
        self.assertTrue(is_better(row(30, 0.78, 0.81), best))
        self.assertFalse(is_better(row(40, 0.78, 0.80), best))
        self.assertTrue(is_better(row(10, 0.78, 0.80), best))


if __name__ == "__main__":
    unittest.main()
