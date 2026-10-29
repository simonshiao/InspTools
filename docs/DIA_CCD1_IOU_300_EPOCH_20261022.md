# CCD1 原廠配對資料 300 epoch / IoU 品質 Gate 實驗紀錄

> **後續校正（2026-10-22）：** 此文件是 **9/14 舊版 RecoveredDAVS／Loss 1** 的歷史實驗，並非 ShuntVC 最新解法。後來找到 ShuntVC 9/30 版 `AgentVisionLab` 的 Loss 6，已在 CCD1 真正重跑 300 epoch，原版 Shunt 品質工具測得 R IoU 由舊候選 0.2495 提升至 0.7771（原廠 0.9406）。最新交接以 `SHUNTVC_TRAINING_20260930_RECOVERY_20261022.md` 為準，不要把下方簡化 IoU 或 Loss 1 結果引用成最新版訓練績效。

日期：2026-10-22，專案：InspTools二次解譯。完整工程實驗在 @CHAO2、獨立 AppBase 執行，來源 `D:\delta\Welding\DIAChip8Side` 及舊 ProjectHub/Shunt 皆**唯讀**。

## 使用者校正後的正確評估方向

- `Ng2` 是完整 Recipe 的二值化工具 `BinaryTools5 / AC-壓傷` 等後段規則的最終判定，**不是**新 AI 模型的 IoU，也不能當作 segmentation 模型是否訓練好的單獨指標。
- 訓練至少 300 epoch；本次實際跑滿 300 epoch。原廠 CCD1 `Training/Model/navs.ini` 設定的是 `Epoch=500`，ProjectHub 的 300 epoch 成功案例另包括 **Locate** 訓練，不能混稱為相同模型。
- `LossFunction=1` 沿用原廠 CCD1 設定；目前 recovered DAVS .pyc 的損失函數內部語意未完全確認，因此**不能宣稱本次已採 IoU loss 訓練**。本次「IoU」是獨立驗證計算。

## 資料防洩漏、訓練實證

- `StageDiaCcd1LongTrain.ps1`：將 SHA-256 與原廠對齊的 CCD1 配對資料放入產品自有 `TrainingImage/DIA_CCD1_S1_Train300`；總計 **19 組 OK + 24 組 NG = 43** 個 X/Y 配對，合計 25 個原始家族。
- 24 張 NG 來自 **6 個原始 NG 家族**，含 H/V/VH 翻轉；其餘 2 個 NG 家族與 2 個 OK 家族保留做 holdout，訓練完全排除相關 X/Y 配對與翻轉版本。
- `TrainingPlans/DIA_CCD1_S1_Train300.json` 保存配對名、家族、每個 X/Y SHA256、明確 holdout 家族。
- `TrainingContracts.Check` 已改成**遞迴且依相對路徑配對**，包含 `ALL`，拒絕 X/Y 不同檔名和 reparse point。
- `GeneralCandidateRunner.Prepare` 新增僅允許**固定 CCD1/原廠 seed/原廠 profile**的長訓練模式：`--train-general-plan-long300 DIA_CCD1_S1_Train300 DIA_R002_vendor.h5 DIA_R002_vendor.ini`。既有 1 epoch 保留為 smoke test。
- 長訓練 CLI 強制 `Epoch=300`、`BatchSize=4`、`MAX_SAMPLES=0`（全部 43 組，**不再只取前 8 組**）、本地 7200 秒超時、來源 hash 不變才執行、`DEPLOYMENT=NEVER`。拒絕其他未核准參數。
- 已執行的完整工作 ID：`20261022T050341Z-33c441fa`。
- 真 GPU：`TRAIN_RESULT=PASS STATE=SUCCEEDED`、`epochs_completed=300`、`stdout.log` 100% progress、`final_loss=0.000004406047082738951`、本地 `candidate.h5` 7,988,672 bytes、SHA256 `BD88B85FFD883DFC5A01DBDA4CA19A647D1EA3B8B07A7CF960CA05EFB694CBE4`。未做正式部署、未連 PLC 或相機。

## IoU 計算來源與方法

使用 `EvaluateDiaCcd1Iou.py`：**真正 TensorFlow 載入原版 H5 權重**，只對暫存副本移除原廠 16-byte header，不修改來源；從自有 `TrainingImage/VPMGK05500VN0Y0BW-CCD1/X,Y` 讀入 4 個 **未訓練** 原始家族的 X/Y 裁切標註，並與 43 組訓練資料分開計算。

- 模型 input/output shape 均為 `(None,256,256,3)`。
- 明確診斷前處理：X 使用 OpenCV linear resize、BGR→RGB、float32/255；Y 使用 nearest-neighbor resize、任一色彩通道 >127 當瑕疵；輸出任一通道 >= 閾值視為瑕疵。
- 評估門檻依 ProjectHub.Native `Shunt-Training/01_最新狀態/HANDOFF.md` 既有證據**220/255**；另外保留初版 0.5 指標，未覆蓋舊紀錄。
- NG 報告獨立 IoU，不把大量 OK 全黑區域與背景 pixel accuracy 混入 NG 平均。空遮罩/無預測在 OK 才記為 1。
- **限制：** 此 IoU 前處理仍未經 recovered DAVS 內部資料路徑逐像素對照；holdout 只有 2 NG + 2 OK，無法推估母體準確率。這是**小樣本相對比較及問題定位**，不代表已經通過原廠正式品質認證。

## 真實 IoU 結果（220/255）

| 模型 | 訓練集 NG Mean IoU | 4 張保留影像中 NG Mean IoU | 保留 NG Micro IoU | 保留 OK 空遮罩 |
|---|---:|---:|---:|---:|
| 原廠 CCD1 seed | 0.9004 | **0.9381** | 0.9406 | 2/2 |
| CCD1 1 epoch 候選（4 OK+4 NG） | 未測 | 0.8526 | 0.8252 | 1/2 |
| CCD1 300 epoch 候選（19 OK+24 NG） | **0.9987** | **0.3170** | 0.2495 | 2/2 |

可見在相同算式、holdout、閾值條件下，300 epoch 模型記熟已訓練瑕疵，但在獨立瑕疵樣本明顯退步：**高度疑似過擬合**。不是單純延長到 500 次就會解決。

IoU source records 均在自有 `output/decoded-ui/Release/TrainingEvaluations/IoU_*_threshold220.json`；`AuditDiaCcd1Iou.ps1` 以原始模型 SHA、300 epoch job、訓練/保留影像來源 SHA 交叉核對，輸出：

```text
DIA_IOU_GATE=BLOCKED_OVERFIT_DIAGNOSTIC EPOCH=300 TRAIN_NG=0.9987
HOLDOUT_NG=0.3170 VENDOR_HOLDOUT_NG=0.9381 DEPLOYED=False
```

不可覆蓋的品質紀錄：`TrainingCandidates/20261022T050341Z-33c441fa/iou_quality_gate.json`。

## 下一個實作方向

1. 補更多**獨立來源**的 CCD1 NG 缺陷影像及人工/原廠 X/Y 標註，確保 holdout 不是只有 2 張 NG，不可把同張 NG 的 H/V/VH 翻轉版本拆進不同組。
2. 在訓練過程建立中途 checkpoint（例如每 25 epoch 保存候選），每個 checkpoint 做**驗證 IoU**而不是只看訓練 Loss；若驗證 IoU 降低，必須拒絕「最後 300 epoch 即最佳模型」的假設。
3. 與原廠 `MyDatasetXY` 前處理逐像素比對（RGB Y 標註與 nearest resize 已有 ProjectHub 證據），驗證 IoU 算式後再制定品質門檻。
4. **此 300 epoch 候選品質 Gate = BLOCKED**，不得轉換/部署為正式 S1 模型；所有實驗成果仍保留可追溯，原廠原始模型、Recipe 不變。
