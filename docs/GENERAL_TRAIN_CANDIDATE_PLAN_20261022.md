# 一般 Train：獨立候選工作規劃 Gate

日期：2026-10-22 04:18 +08。僅修改 InspTools二次解譯，來源 ProjectHub.Native、ShuntVC_Chao、正式 release/current 均未寫入。

## 本輪完成

- GeneralCandidateRunner.cs 原本是未追蹤、未編譯的草稿；現已納入唯一 Standalone 專案、Build.ps1 的 GUI 與 CLI 編譯來源。
- --train-general-plan DATASET SEED.h5：完整前置檢查成功後，在 AppBase/TrainingCandidates/<UTC-GUID> 建立不重複的候選工作。產生 candidate.ini、input.txt、progress 目錄、state.txt=PREPARED。沒有啟動 Python，也沒有覆寫 Model/Recipe。
- 計畫固定 1 epoch、batch 1、最多 8 samples、900 秒上限規格；模型仍必須使用獨立訓練 Runtime，尚未實作啟動執行器。
- 候選工作建立前，會重新檢查資料集 X/Y、seed 檔名及大小，拒絕路徑跳脫；後續 job ID 必須符合 UTC+8 hex 字元格式。
- --train-general-status JOB_ID 可查詢已存在的工作狀態。
- --train-general-stop JOB_ID 僅在狀態為 RUNNING 時建立 stop.request。目前沒有 GPU runner 消費 stop.request；不得宣稱可停止真正訓練。
- --train-general-selftest 使用暫存假資料測試計畫生成、PREPARED 狀態、拒絕未開始的停止要求及非法 job ID；測後刪除自己建立的暫存資料。

## 真正執行的最小 Gate

| 測試 | 結果 |
|---|---|
| Build.ps1 Release | PASS；1 個正式專案，0 off-route |
| 原版 INSPECTION TOOLS GUI 啟閉 | PASS，無 error file |
| --train-general-selftest | PASS，GPU_RUN=False |
| --device-twin-selftest | PASS，12 checks，0 external connections |
| --train-contract-selftest | PASS，3 families，TRAIN_EXECUTED=False |
| --train-general-plan .. seed.h5 | 拒絕，exit 74 |
| 真正 AppBase dataset+seed+Python preflight | 尚缺實際 Runtime/TrainingSeed |
| 真正 GPU Train / Stop / 產物 | NOT_RUN |

## 重要限制

原始 donor train_compat.py 介面已唯讀核對：-i、-m、-v DAVS_V3、-r Train、-e、-b、--compat-seed-model、--compat-max-samples。實際回傳的 training_result.json 包含 status、epochs_completed、physical_gpus、model_sha256、success_marker。後續 runner 必須逐項驗證，不能僅依 Exit 0 判斷訓練成功。

目前 AppBase 沒有 Runtime/TrainingPython、Runtime/TrainingRebuild、TrainingSeed；StageGeneralTrainer.ps1 執行此前被工具安全檢查阻擋，這輪沒有重複嘗試繞過。候選工作規劃的自測是合約測試，不等於實際 GPU 訓練。

下一輪：先在允許的權限下完成 SHA-pinned 小型 DAVS staging，再確認自有 Python/GPU runtime 的合法封裝方式；之後實作真正可逾時、可停止、只輸出 candidate 的 runner，做 1 epoch 小樣本，驗 H5D/PBD 與原版 ReadModel/Inspect。
