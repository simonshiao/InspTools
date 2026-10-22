# InspTools 獨立一般 AI 訓練｜候選執行 Gate

日期：2026-10-22。本專案只交付 InspToolsStandalone.exe；ProjectHub.Native／Shunt／Chao8Side 是唯讀參考。

## 新增真正的候選工作執行器

- src/InspTools.Standalone/GeneralTrainerExecutor.cs 已納入正式 Build.ps1 與 GUI、CLI 專案。
- --train-general-run JOB_ID 僅執行 --train-general-plan 所建立、狀態 PREPARED 的候選工作。
- 重新檢查資料集 X/Y 配對、種子 SHA、原始 DAVS train_compat.py/pyc 雜湊、AppBase 下 Python38/GpuRuntime；不改原版 Recipe/Model，也不接實體 PLC。
- 訓練限定 1 epoch、batch=1、最多 8 samples、900 秒；所有產物限於 TrainingCandidates/JOB_ID。
- 非同步記錄 stdout.log / stderr.log；支援 --train-general-stop JOB_ID 提交 stop.request，中止執行器自己開的程序。
- 子程序 Exit 0 不等於 Train PASS：再次核查 training_result.json 的 PASS、DAVS contract、epoch、physical_gpus、原始 seed SHA、candidate.h5 的檔案大小與 SHA、成功標記，以及所有回報的輸出路徑。
- 不會自動部署候選模型，不改 active Model/Recipe。
- --train-general-executor-selftest 以無害子程序測逾時／停止／正常輸出／假陽性結果防護，絕不啟動 GPU Train。

## 此輪確認的 Gate

| 項目 | 結果 |
|---|---|
| Build.ps1 Release | PASS，正式 1 project，off-route 0 |
| --train-general-executor-selftest | PASS，逾時、停止、退出、假陽性防護 |
| --train-general-selftest | PASS，候選工作規劃 |
| --train-contract-selftest | PASS，三條資料契約 |
| --device-twin-selftest | PASS，12/12，零外部連線 |
| --train-general-run .. | 正確拒絕，Exit 74 |
| --train-general-gpu-probe | **PASS：TensorFlow 2.7.0 / GPU=1** |
| 原版 ToolsWindow 啟停 | PASS，Exit 0，無錯誤檔 |
| 真正 DAVS GPU 1 epoch | **NOT_RUN** |
| 新模型 .h5d/.pbd/原版 Reload | **NOT_RUN** |

實查 AppBase 內 Runtime/TrainingRebuild/train_compat.py 與 recovered DAVSTrain_v3.pyc 均存在、SHA 分別符合既有 pinned 值：
4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E 與
8645361D666E06A80E0BD52C1C91A63F3D5AFE90414297F001ADAAAE44F27522。
TrainingPython/Python38/python.exe 也存在，GPU probe 確實可讀到單張 GPU。

## 剩餘阻塞及下一步

目前 AppBase 沒有 TrainingSeed 目錄，尚未有受控的有效 .h5 seed＋完整 X/Y 標註小樣本組合，不能將 Shunt 歷史成功直接當作本案成功。

下一輪需從 ProjectHub 已驗證訓練案件尋回 seed 的來源與 SHA，只將授權測試副本納入 InspTools 隔離區，才可依序執行：

    InspToolsRecipeCli.exe --train-general-preflight DATASET SEED.h5
    InspToolsRecipeCli.exe --train-general-plan DATASET SEED.h5
    InspToolsRecipeCli.exe --train-general-run JOB_ID
    InspToolsRecipeCli.exe --train-general-status JOB_ID

同時驗真正 GPU 運作期間的 --train-general-stop JOB_ID、900 秒上限、候選模型合法性及原版 Inspect 驗收。

本輪開始前已有 StageGeneralTrainer.ps1 的大型 Python/CUDA staging 未提交變更；這部分本輪沒有再執行，也沒有宣稱完整 Runtime 封裝通過。保留原差異待獨立稽核。
