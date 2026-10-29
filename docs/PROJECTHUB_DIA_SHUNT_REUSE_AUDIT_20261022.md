# DIA / ShuntVC / ProjectHub 複用稽核（2026-10-22）

執行節點：@CHAO2。使用者指出 InspTools 正重複實作既有訓練功能；此份是唯讀查核結果，**暫停新增通用 Trainer / Quality Gate / Runtime / Model Converter**，先重用現有模組。

## 已查的 Source of Truth

1. E:\0_軟體開發區\3.開發中專案區\1.進行中\ProjectHub.Native\ProjectData：Projects/DIAChip8Side、Shunt-Training、Shunt-VisionTools、AgentVisionLab 之 LATEST/INDEX/HANDOFF；SharedKnowledge/AITraining/INDEX.md、THREE_TRAINING_RUNTIME_PLAYBOOK_20260913.md、UNIFIED_AI_RUNTIME_CONTROL_LAYER_20260914.md。
2. D:\delta\DIAChip8Side-Template\tools\DIABiSeNetV2\engine：trainers/bisenetv2_trainer.py、trainers/temp_unet_trainer.py、quality_gate.py、threshold_sweep.py。
3. D:\delta\ShuntSensor\外觀機\Runtime\ShuntVC\Runtime\AgentVisionLab：現存 quality_gate.py 與 trainers；InspTools 自有 AppBase 已固定 Shunt 9/30 Loss6 Trainer SHA，來源為唯讀。
4. AgentVisionLab：Output/Baseline/AVL_BASELINE_2026-09-11.md 與 03_知識治理/Cases.json 的 CASE-TASK-AVL-03「雙模型公平比較與實驗登錄」。

**新舊版本界線**：ProjectHub 的 LATEST/INDEX 多於 10/02 生成，共用 Playbook 多為 9/13~9/14；InspTools 10/22 實際完成的 Shunt 9/30 Loss6、300/500 epoch holdout Gate 為較新事實，不能用舊模板覆蓋。源碼與固定 SHA 優先於歷史指標。

## 必須直接沿用，避免重做

| 能力 | 已有來源 | 結論 |
|---|---|---|
| AOI/Locate/AI先知三套不同 Training 契約 | ProjectHub AITraining 共用 Playbook | CCD1 是 X/Y 的 General AOI，勿改作 AI先知 |
| Loss6 / RGB 標註 / Temp U-Net US | Shunt 9/30 AgentVisionLab + InspTools 自有 pinned staging | 不回頭用舊 RecoveredDAVS，也不再發明新 Trainer |
| X/Y 配對、資料檢查、Resume、prior-best | AVL_BASELINE_2026-09-11.md、Temp U-Net trainer | 用原本 dataset preflight、progress/Resume，僅在 CCD1 補家族 SHA 的特定 Gate |
| RGB IoU/Precision/Recall、影像 OK/NG | AgentVisionLab quality_gate.py | 固定 CCD1 的 220/255 與 min_area=10，不能另造通用品質工具 |
| 模型 A/B 隔離比較與可追溯實驗 | AVL-03：child 評估、comparison.json、list/get API | 不另建 Model Comparison/Experiment Registry；656 圖是 AVL 資料，不能與 CCD1 四圖成績混用 |
| Threshold Sweep | AgentVisionLab threshold_sweep.py | 可研究，但候選比較需相同閾值 |
| GPU Runtime/ABI 控制 | ProjectHub UNIFIED_AI_RUNTIME_CONTROL_LAYER | DIA_COMPAT 與 PROPHET 分開；不可多拷 Runtime 或混 CUDA |
| H5D→PBD、ReadModel、部署隔離 | ProjectHub AITraining + InspTools 10/22 轉換/InspectFile Gate | 僅研究候選，禁止自動替換 active Model |

## 真正的差異功能

已查原始碼：DIA Template 的 bisenetv2_trainer.py 和 temp_unet_trainer.py，checkpoint 都 monitor="loss"、save_best_only=True；本案 pinned Shunt Loss6 trainer 也只選 training-loss best。ProjectHub AVL-03 是**訓練後模型 A/B 品質比較**，未找到訓練中每 N epoch 依 holdout R IoU 保存最佳權重的已完成實作。**不要把「有品質評估」誤認為「有 IoU 驅動 Checkpoint」。**

因此可保留 2026-10-22 新增的 Tools/ccd1_iou_runner_v1.py / RunCcd1IoUSelection.ps1 作隔離原型，但不能平行養一整套 Trainer。新程式已有 43 組 X/Y SHA、25 原圖家族、4 組保留/零洩漏稽核、2 個單元測試，以及**一輪 GPU smoke**，且原版 Shunt quality_gate.py 在同一 4 組保留圖的重新推論與 callback 數值一致（R IoU 0.444620，smoke 模型 2 OK 全部誤報）。證據 output/decoded-ui/Release/TrainingCandidates/CCD1_IoU_V1_Smoke_20261022/training_result.json 和 shunt_quality_parity.json；只是功能測試，**未跑 300 epoch IoU 選模**，不能宣稱品質改善。

## 下一步（小改、最小驗證）

1. 先固定並比對本案 staged 9/30 Shunt 訓練器與品質工具的 SHA、RGB/resize/threshold、seed/model width/height，確認不誤用 DIA 舊 MSE 或 AVL 0.60 threshold。
2. **已完成、不要重做**：1 epoch GPU smoke 與原版 Shunt quality_gate 的 RGB IoU/precision/recall、NG/OK parity PASS；現有 smoke 結果不具量產品質。若未變更計分契約，不必重跑此 smoke。
3. 新增內容收斂到既有 Trainer 的最小 Checkpoint callback/adapter，保留原有資料審核、Resume、工作生命週期與品質 Gate，不再獨立建立 Experiment DB / GPU backend。
4. 尋找其他日期/批次真正獨立的 CCD1 NG 家族 X/Y，按原圖 family 分組；現有 2 個 NG holdout 一旦拿來選 checkpoint，便是調參驗證集，不再能當盲測。
5. 完成以上才可考慮一個新的 IoU-driven 研究 job。所有新候選 production_qualified=false、deployed=false；原廠及舊模型不動。

本輪只寫本案稽核與交接文字，**未修改 ProjectHub、DIA、ShuntVC 原始檔**，也沒有新 GPU 長訓練。1 epoch smoke 是本案同一工作樹已完成的其他實作進度，本稽核經實際 result/parity JSON 讀回核對。
