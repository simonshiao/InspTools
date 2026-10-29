# ShuntVC 後期訓練研究回收、CCD1 Loss 6 回歸實測

> **2026-10-22 追加：已依需求接續訓練至總共 500 epoch，Loss 目標 1e-5 已達成，保留驗證 IoU 卻下降。** 500 epoch 的模型 R IoU 0.5769，300 epoch 的 R IoU 0.7771，原廠 0.9406。新候選 **不部署**，留 300 epoch 做目前自研比較基準。詳見下方第六節，`AuditShuntLoss6_500.ps1` 與品質檔 `ShuntLoss6_CCD1_500E_20261022_R1/iou500_quality_gate.json`。

2026-10-22 / @CHAO2 / InspTools二次解譯。**此份報告優先於先前的 Loss 1 / 舊 IoU 摘要**。沒有覆蓋 ShuntVC、ProjectHub、DIA 原始資料，沒有對正式 Recipe、模型、PLC 或 Camera 寫入。

## 一、為何已在 ShuntVC 解決，InspTools 又遇到同樣問題

實際比較三份原始 `train_compat.py`：

| 路線 | 原始檔 SHA-256 | 差異 |
|---|---|---|
| `E:\...\ShuntVC_Chao\Runtime\Runtime\TrainingRebuild\train_compat.py` | `4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E` | 2026-09-14 版本，直接呼叫 `RecoveredDAVS.DAVSTrain_v3`，沒有 AgentVisionLab 分流 |
| `D:\delta\ShuntSensor\外觀機\Runtime\ShuntVC\Runtime\TrainingRebuild\train_compat.py` | `2028C94D788E93B3822D9CA8D6650D27BD3B983F019EF783D17DFA984DEB2C32` | 2026-09-30 後更新版，新增 AgentVisionLab 訓練分流、巢狀類別路徑攤平、RGB Y 保留 |
| `InspTools...\output\decoded-ui\Release\Runtime\TrainingRebuild\train_compat.py` | `4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E` | **錯拿 9/14 的舊版本** |

舊 300 epoch `20261022T050341Z-33c441fa` 的 X/Y 正確計數與 GPU PASS 只能證明訓練有完成，**不是沿用最新 ShuntVC 訓練實作**。

此外，舊 General 300 epoch 使用原廠 CCD1 `LossFunction=1`；而 Shunt 後期成功解決稀疏瑕疵像素的主要實作是在 `Runtime/AgentVisionLab/trainers/temp_unet_trainer.py` 的 `LossFunction=6`：

```python
pos_mask = tf.cast(y_true > (180.0 / 255.0), tf.float32)
neg_mask = tf.cast(y_true < (5.0 / 255.0), tf.float32)
loss = mean((y_true - y_pred)**2 * pos_mask * 10
            + (y_true - y_pred)**2 * neg_mask)
```

它忽略中間不確定色階，正樣本像素損失加權 10 倍。Loss 1 是 MSE，當正常背景像素遠大於瑕疵像素時可能把輸出推向全黑。

**此前的 IoU 評估器也不是照搬 Shunt：** 我們最初使用 `cv2.INTER_LINEAR`、`max(R,G,B)` 以及 `Y>127`。Shunt 的 `render_model_preview.py` 用 `cv2.INTER_AREA`、`prediction[...,0]`、`Y>0`；更完整的 `AgentVisionLab/quality_gate.py` 則以訓練相同的 RGB resize、三通道分別輸出 precision/recall/IoU、並回報 NG/OK 影像級統計。本次已將這兩份**實際 Shunt 原版檔案**分層複製到 InspTools 自有 Runtime，並實際執行，不再只使用自編的簡化統計。

## 二、ShuntVC 9/30 現成成果直接驗證

以下兩組來源均為 `ShuntVC/Runtime/TrainingRuns/3923-R002-S1-AgentVisionLab*` 的 **300 epoch** 模型。讀取同一批 55 組 Shunt X/Y（50 OK、5 NG），透過 app-owned 原版 `quality_gate.py` 以 **220/255** 重新測試：

| 模型 | R channel IoU | R Recall | R Precision | NG 偵測 / OK 誤報 |
|---|---:|---:|---:|---:|
| 300 epoch Loss 1 | 0 | 0% | 0% | 0/5 NG，0/50 OK 誤報 |
| **300 epoch Loss 6** | **0.9130** | **96.65%** | **94.29%** | **5/5 NG，0/50 OK 誤報** |

這是**訓練集成果**，不能冒稱跨設備獨立驗證。它確認當時 Loss 6 方向確實有效，也解釋了為何不應回去只看 MSE Loss。

原始品質實證保存在自有 `TrainingEvaluations/Shunt20260930_Source55_MSE_220.json` 與 `...LOSS6_220.json`。

## 三、真實移植與 CCD1 重新訓練

- `StageShuntTrainer20260930.ps1`：從 9/30 Shunt 官方工作樹**唯讀**複製、驗證 6 份 SHA-256 固定檔案進新版 `output/decoded-ui/Release/Runtime/`。包括 `AgentVisionLab/trainers/temp_unet_trainer.py`、`quality_gate.py`、`dia_model_tools.py`、`render_model_preview.py`、`threshold_sweep.py` 與**獨立命名**的 `train_compat_shunt_20260930.py`。沒有替換歷史 `train_compat.py`，避免舊候選失去可重現性。
- `StageDiaCcd1Holdout4.ps1`：保留四個有 X/Y 的原始影像家族；以 X/Y SHA 校驗，拒絕和 43 組訓練家族重疊。
- `RunShuntCcd1Loss6.ps1`：明確走 app-owned AgentVisionLab 而不是舊版 RecoveredDAVS；**真正使用 `--resume-weights` 指向原廠 CCD1 seed**、Loss 6、UnetDiv 4、Batch 8、LR 0.001、AugRatio=1（Shunt 原版 Sequence 每輪重複兩次，且無額外強化設定）、固定 43 組、GPU 訓練 300 epoch、最佳訓練 Loss 權重、只在隔離候選根目錄產生檔案。具限時、stop.request、seed/資料 SHA 防呆。
- 注意 Shunt 最新 `train_compat.py` 的 `resume_candidate` 只在 **seed 路徑與輸出 model 路徑完全相同**時才傳 `--resume-weights`；此條件在我們「原廠 seed 與候選輸出分離」的安全設計下不成立。**因此不能不加思考就複製新 adapter 並直接當續訓。** 新獨立指令改為顯式傳入 seed，日誌已驗證 `TEMP_UNET_RESUME_UNWRAP`，確認不是無意從隨機初始權重重練。
- `1 epoch` 驗證 PASS：原廠 seed 續訓、43 pairs、RGB R 通道 24 NG、19 OK 空遮罩、產物 SHA/最佳權重均 PASS。
- **300 epoch 驗證 PASS**：工作 `ShuntLoss6_CCD1_300E_20261022`，Best epoch **298**、Best train loss `2.67694485955872e-06`、純 H5 SHA-256 `3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634`。只留候選 H5，不曾轉成正式 H5D/PBD 或部署。

## 四、CCD1 最新真實成績

以 ShuntVC 自有 `quality_gate.py`，固定同一批 **4 個從未訓練的家族**（2 NG + 2 OK，來源 X/Y 與 SHA 均固定），閾值 **220/255**、最小瑕疵面積 10 像素，全部用 R 通道：

| CCD1 模型 | 保留集 R IoU | NG 瑕疵像素 recall | OK 誤報 / NG 漏檢 |
|---|---:|---:|---|
| 原廠模型 | **0.9406** | 96.20% | 0 / 0 |
| 舊版 DAVS / Loss 1 / 300 epoch | 0.2495 | 25.20% | 0 / 0 |
| **Shunt 9/30 AgentVisionLab / Loss 6 / 300 epoch** | **0.7771** | **80.83%** | 0 / 0 |

新版 Loss 6 的 **訓練集 R IoU = 0.9742**。技術改善很明顯，但在這四張 holdout 下仍落後原廠 0.9406，不可直接升版，尤其不能把 `NG=2/2` 當作小瑕疵像素完整分割的證明。

所有計算輸出保存在 `TrainingEvaluations/ShuntOriginalQuality_*.json` 和 `TrainingEvaluations/ShuntFixedLoss6_*.json`。本次 `AuditShuntLoss6Recovery.ps1` 會核對 backend/model SHA、300 epoch loss6 記錄、真實 43 paired manifest、R mask label、同一 holdout、門檻，產生：

```text
SHUNT_RECOVERY_GATE=IMPROVED_LOSS6_STILL_NOT_QUALIFIED
VENDOR_R_IOU=0.9406 LEGACY_R_IOU=0.2495
LOSS6_R_IOU=0.7771 LOSS6_TRAIN_IOU=0.9742
BEST_EPOCH=298 DEPLOYED=False
```

不可覆蓋的候選品質記錄：`TrainingCandidates/ShuntLoss6_CCD1_300E_20261022/quality_recovery_result.json`。

## 五、防止再重複犯錯

1. **停用舊版 `--train-general-plan-long300`**：它只會走 9/14 RecoveredDAVS 與 Loss 1，已由 CLI 明確拒絕；歷史工作仍保留可讀。
2. 今後優先使用 app-owned **Shunt 9/30 AgentVisionLab + Loss 6 + RGB 原色保留 + 原廠 seed 顯式續訓 + 配對 manifest + 品質報告**，避免退回舊版訓練鏈。
3. **Shunt 當時的 best checkpoint 監看的是訓練 Loss，不是驗證 IoU。** 後續要補以獨立家族 IoU 選 checkpoint／早停的功能，避免第 300 輪的權重只是更會記住訓練圖。
4. 現在只 2 NG holdout，仍須擴充獨立瑕疵標註，並用不同日期／批次分組；維持 `PRODUCTION_QUALIFIED=False`。

以上所有流程均無 PLC、Camera 或正式 Model 寫入，ProjectHub、ShuntVC 與 DIA 原版全程唯讀。 

## 六、接續 300 → 500 epoch、目標 Loss <= 1e-5（已完成）

延續 `RunShuntCcd1Loss6.ps1` 的 AgentVisionLab / LossFunction=6，這次明確指定 `--initial-epoch 300 --epochs 500`，**總共訓練 500 輪，不是 300+500=800**；`--resume-weights` 指向 300 epoch 已選出的最佳權重（Best Train Loss Epoch 298），`--resume-progress` 指向從 300 epoch 鎖定 SHA 複製進新 job 的 `resume_progress_300.json`。原廠 seed 在第一階段已引用，500 延訓未覆寫正式任何檔案。

第一個 500 epoch 工作 `ShuntLoss6_CCD1_500E_20261022` 因 Windows PowerShell 對 `--resume-progress` 路徑引號夾帶空白，`Path.resolve()` 報 WinError 123，工作標記 FAILED，模型未完成產生，保留原始 log 追溯。修正為工作根目錄內相對進度檔後，另建新工作 **`ShuntLoss6_CCD1_500E_20261022_R1`**，不覆蓋失敗紀錄。

新工作 200 個續訓 Epoch GPU PASS，最終 `epochs_completed=500`、`initial_epoch=300`、`epochs_trained_this_run=200`，共 43 組 X/Y；最佳 Train Loss 第 **499** 輪，**`2.11821247830812e-06`，低於需求的 `1e-5`**。H5 SHA256 `84D97A39705ADFE59DE14DB5715F293F4897B64E5CF32769644CBBB76D850410`，模型只在隔離候選資料夾。**注意：續訓載入模型權重但重新建立 Adam optimizer，不是逐位元保留原來的 optimizer 動量狀態**；若需對比完全連續訓練的精確 Loss 曲線，需要額外保存 optimizer state。

使用同一版原始 Shunt `quality_gate.py`、R 通道、220/255、最小面積 10、同一個 4 原始家族 holdout：

| CCD1 模型 | 最佳訓練 Loss | 訓練集 R IoU | 保留集 R IoU | 保留 R Recall | NG 漏檢／OK 誤報 |
|---|---:|---:|---:|---:|---|
| 原廠 | 未重測 | 未重測 | **0.9406** | 96.20% | 0／0 |
| Loss 6，300 epoch | 2.6769e-6 | 0.9742 | **0.7771** | **80.83%** | 0／0 |
| Loss 6，500 epoch | **2.1182e-6** | 0.9741 | **0.5769** | 60.66% | 0／0 |

**明確結論：** 500 epoch 達到 1e-5 甚至更低，但 holdout R IoU 相對 300 epoch 下降約 **0.2002**；NG 雖仍以影像級偵測到，瑕疵遮罩召回率降低，顯示品質退步。低 Loss 不是高 IoU 的充分條件。保留 `300 epoch` 自研候選作比較，**500 epoch 的正式品質 Gate = HOLDOUT_IOU_REGRESSED_KEEP_300**；兩個候選都尚未達原廠水準，`production_qualified=false`。

`AuditShuntLoss6_500.ps1` 已 PASS，並用原始模型 SHA、resume progress 的 300 epoch 歷史、總計 500 輪、相同訓練與保留集、Loss 1e-5 閾值、每個模型 H5 hash 檢查以產生不可覆寫的 `iou500_quality_gate.json`。目前 2 個 NG family 的驗證仍偏少，下一步優先擴充獨立批次，並依獨立驗證 IoU 監看 checkpoint，而不是盲目延長到更大 epoch。
