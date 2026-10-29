# CCD1｜Loss6 驗證 IoU 選模 v1（研究入口，不可部署）

日期：2026-10-22／@CHAO2（HOME-1）／DIA 檢測執行環境。

## 已完成

- 版本化入口：根目錄 RunCcd1IoUSelection.ps1；程式：Tools/ccd1_iou_runner_v1.py。
- 沿用本案 AppBase 裡 SHA 固定的 2026-09-30 Shunt temp_unet_trainer.py，只 import 已有 Loss6 / Temp U-Net US / RGB X/Y 載入函式。原始 ShuntVC 和現有 300/500 epoch 工作一律不修改。
- 訓練 epoch 結束後，每 CheckpointEvery（預設 20）輪與最後一輪，評估全部 4 個保留原圖家族的 RGB 分割品質。以 R channel IoU 最高選模，同分比較 R recall，再同分選較早 epoch。Training Loss 只記錄，不參與選模。
- 與 Shunt quality_gate.py 一樣，閾值 220/255、3-channel RGB 混淆矩陣；影像級 NG 判斷為 RGB 遮罩聯集的相連區域面積至少 10。
- 持續保存 iou_progress.json；選中 checkpoint 只保留最佳權重 best_holdout_r_iou.weights.h5，先存暫存檔再原子取代，避免中斷時損毀舊最佳模型。訓練成功產出 selected_by_holdout_iou.h5 於全新、不允許覆寫的候選工作目錄。
- input_provenance.json 記錄訓練器、執行器、seed、訓練/驗證 SHA、參數；有 stop.request 和最大秒數守門。失敗留下 STATE=FAILED_NO_DEPLOY。
- 全程研究隔離：production_qualified=false、deployed=false；不觸碰 release/current、原廠模型／Recipe、ShuntVC、DIA 原始資料，也不轉換正式 H5D/PBD。

## 執行方式

在專案根目錄的 PowerShell，先只執行資料稽核（預設模式，不建 job）：

~~~
powershell -NoProfile -ExecutionPolicy Bypass -File .\RunCcd1IoUSelection.ps1
~~~

僅在明確決定開始新 GPU 研究實驗時，才使用全新 job 名稱：

~~~
powershell -NoProfile -ExecutionPolicy Bypass -File .\RunCcd1IoUSelection.ps1 -Train -JobName CCD1_IoU_V1_Experiment001 -Epochs 300 -CheckpointEvery 20
~~~

預設從本案唯讀 staging 的原廠 seed 開始，不沿用 Adam optimizer 動量。變更來源權重時必須一起提供 -ResumeWeights 與 -ExpectedResumeSha，拒絕不符 SHA 的權重。新輸出只在 output/decoded-ui/Release/TrainingCandidates/<JobName>。本次沒有啟動以上長訓練。

新候選完成後，可直接用原版 Shunt 品質工具重算並比對 IoU callback 結果：

~~~
powershell -NoProfile -ExecutionPolicy Bypass -File .\RunCcd1IoUSelection.ps1 -VerifyJob CCD1_IoU_V1_Smoke_20261022
~~~

## 最小驗證結果

1. RunCcd1IoUSelection.ps1 不帶 -Train：DATA_AUDIT_PASS。確認 43 對訓練資料、25 原始家族（6 NG）、4 對保留家族（2 NG）、零家族重疊及 X/Y SHA 全符合。
2. Tools/test_ccd1_iou_runner_v1.py：2/2 PASS，包含 R/G/B TP/FP/FN、IoU、最小面積、模型選擇次序。
3. 新 Python 程式編譯：PASS。
4. 嘗試沿用既有 ShuntLoss6_CCD1_300E_20261022 名稱啟動：按設計拒絕（Exit 1），沒有建立新工作、沒有覆寫。
5. AuditShuntLoss6_500.ps1：PASS，原廠 0.9406、300 epoch 0.7771、500 epoch 0.5769，狀態 HOLDOUT_IOU_REGRESSED_KEEP_300。該 Gate 同時核對舊候選實際模型 SHA。
6. 舊 AuditShuntLoss6Recovery.ps1：印出了期望品質 Gate 和數值，但程序未在限制時間內退出（timeout）；本次不能視為完整 PASS。
7. **真實 GPU 1 epoch smoke PASS**，獨立研究 job：CCD1_IoU_V1_Smoke_20261022。Training Loss = 5.26862103e-4；依 holdout R IoU 完成 callback 與 checkpoint 保存，R IoU=0.444620、R precision=0.451436、R recall=0.967157；影像級 NG=2/2、OK 誤報=2/2。輸出 H5 SHA-256=9F3244F35C41B343E3FA5760895AD0032BC86DD4162A4974FD81D053112109C6，僅供功能驗證，品質遠低於原廠 0.9406。
8. **原版 Shunt quality_gate.py 獨立重算比對 PASS**：新入口 -VerifyJob 對相同 4 X/Y 驗證影像重新載入 smoke 選中 H5，R/G/B precision、recall、IoU、TP/FP/FN 及影像判斷全部一致，R IoU=0.444620。比對報告保存在同一 smoke job 的 shunt_quality_parity.json。訓練與獨立重算時 TensorFlow stderr 出現 CreateProcess error code 2，但兩次皆退出碼 0、模型與數值驗證成功；此非正式量產 Runtime 驗收。

## Gate 與後續

- v1 已完成程式、資料稽核、CPU 單測、**1 epoch GPU checkpoint smoke** 及原版 Shunt 品質工具數值一致性核驗；**尚未執行 300 epoch 的 IoU 選模實驗**，也沒有任何新模型通過品質 Gate。1 epoch 僅驗證流程，不可宣稱 IoU 改善。
- 現有 4 張保留圖只包含 2 個真正 NG 家族；一旦被用作 checkpoint 選模，這批影像屬於**調參驗證集**，不再能當獨立最終盲測。必須新增不同來源／日期／批次的 X/Y 原始標註，以供最終評價與正式 Gate。
- 43 張訓練影像只有 25 個原始家族，其中 NG 僅 6 個家族。新增資料必須先核對真實 X/Y 配對、分群、翻轉版本、重複內容及來源 SHA；禁止同一家族跨訓練與驗證，不得拿無 Y 的影像假充已標註 NG。
- 即使研究驗證集 IoU 勝過原廠，也要完成獨立盲測、原廠同條件比較、離線 InspectFile 與 Recipe Gate，才可能討論正式升版。保持全部候選禁止部署。
- 下一步：擴充獨立瑕疵資料、再進行新的 IoU 驅動研究訓練與全新盲測驗收，不要單純加 epoch。
