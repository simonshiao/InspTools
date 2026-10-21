# InspTools 一般 AI Train｜前置 Gate

2026-10-22：本輪僅完成前置驗證，尚未啟動 GPU Train。

## 來源與可重用成果

ProjectHub.Native 的 THREE_TRAINING_RUNTIME_PLAYBOOK_20260913.md、SVT05_THREE_TRAINING_RUNTIME_HANDOFF_20260913.md 記錄：一般 DAVS 使用 X/Y 成對資料，模型 .h5d → .pbd，需 candidate 驗證與 rollback。Shunt 歷史 1 epoch GPU PASS 不代表本案已通過。

ShuntVC_Chao/Runtime/Runtime/TrainingRebuild 的 train_compat.py SHA：4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E。

RecoveredDAVS/DAVS/DAVSTrain_v3.pyc SHA：8645361D666E06A80E0BD52C1C91A63F3D5AFE90414297F001ADAAAE44F27522。

## 已實作

InspToolsRecipeCli.exe 新增 --train-general-preflight DATASET_NAME SEED.h5；檢查 AppBase/TrainingSeed 本地種子、Runtime/TrainingPython/Python38/python.exe、Runtime/TrainingRebuild/train_compat.py、RecoveredDAVS 主 pyc，以及 TrainingImage 的 X/Y 成對資料。只讀，不修改原廠 Model、Recipe。

StageGeneralTrainer.ps1 可從唯讀 donor 按 SHA 複製約 250KB 的 Python/DAVS backend 到 ignored AppBase Runtime。大型 GPU Python Runtime 約 5.2GB，這輪不複製；IncludePython 目前明確拒絕。

## 真正驗證

- Build.ps1 Release：PASS；正式產品 1 個、off-route 0 個。
- 原版 GUI INSPECTION TOOLS：正常開啟、關閉。
- Device Twin：12/12 PASS、外部連線 0。
- 三類 Train dataset contract：PASS；TRAIN_EXECUTED=False。
- --train-general-preflight missing-fixture seed.h5：BLOCKED，exit 74。
- --train-general-preflight .. seed.h5：路徑拒絕，exit 74。
- StageGeneralTrainer.ps1：PowerShell 語法解析 0 error；執行呼叫被工具安全檢查阻擋，**尚未完成 staging 實測**。
- Git：本輪變更已 git add、git diff --cached --check PASS；commit 命令被工具安全檢查阻擋，**尚未提交**。

## 下一輪

1. 先完成 SHA-pinned 小型 backend staging 實測，確認沒有修改 donor。
2. 以獨立封裝策略處理 Python38/GpuRuntime，避免每次重複複製 5.2GB；確認完整本地相依。
3. 只用本案 AppBase/TrainingImage 和 TrainingSeed，建立限時、可停止的候選 Train job，1 epoch 小樣本真 GPU 驗收。
4. 驗證 .h5 → .h5d → .pbd → 原版 ReadModel/Inspect；只發布 candidate，不自動覆寫正式 Model。
5. 之後才把 Train 入口接回原版 ToolsWindow；不得另畫主 UI。
