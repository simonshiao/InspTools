# 專案腳本索引

這些腳本綁定本案路徑、固定雜湊、Recipe 或實驗流程，應隨專案一起移轉；不是全域 Agent 工具。
2026-10-28 從根目錄移至 scripts，腳本以其父目錄為專案根目錄。歷史文件中的根目錄命令請改成 scripts/檔名。

從專案根目錄執行基本建置：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1
```

R3 打包使用 scripts/BuildStandaloneR3.ps1；先查看其固定 DLL、輸入與 output 需求。Train、Stage 和 Test 可能寫入 output 或耗用 GPU，先讀對應腳本及歷史紀錄；production 資料維持唯讀。

## PowerShell 腳本

- `Build.ps1`：編譯 Standalone 與 CLI，使用 release/current 固定依賴。
- `BuildDeliver.ps1`：建立歷史正式交付包；執行前核對目的地與 release。
- `BuildNativeLazyInit.ps1`：從一期唯讀來源建立隔離 R3 修補 DLL。
- `BuildPortableReview.ps1`：建立舊 portable-review 候選；涉及既有測試輸出，先查看腳本。
- `BuildStandaloneR3.ps1`：以固定 R3 DLL 建立不覆蓋的攜帶候選，可選 ZIP。
- `AuditDiaCcd1Iou.ps1`：核對 CCD1 IoU 實驗資料與結果。
- `AuditShuntLoss6_500.ps1`：核對 500 epoch 實驗品質 Gate。
- `AuditShuntLoss6Recovery.ps1`：核對 Loss6 300 epoch 品質恢復結果。
- `CompareDiaCandidateAB.ps1`：比較原廠與候選隔離 Inspect 結果。
- `ExtendDeltaRuntimeGate.ps1`：在隔離 Gate 增補固定版本 GPU 推論依賴。
- `RunCcd1IoUSelection.ps1`：IoU 選 checkpoint 研究入口；訓練需明確參數。
- `RunShuntCcd1Loss6.ps1`：執行 CCD1 Loss6 研究訓練。
- `StageDeltaRuntimeGate.ps1`：建立 Delta AI 隔離 Runtime Gate。
- `StageDiaCcd1BalancedSmoke.ps1`：準備 CCD1 平衡資料 smoke。
- `StageDiaCcd1Holdout4.ps1`：準備 CCD1 四家族保留集。
- `StageDiaCcd1LongTrain.ps1`：準備 CCD1 長訓練配對資料。
- `StageDiaVendorBaselineGate.ps1`：準備原廠基準隔離檢測 Gate。
- `StageDiaVendorSample.ps1`：從原廠唯讀來源準備 Recipe、模型和樣本副本。
- `StageGeneralTrainer.ps1`：準備 General Trainer 依賴。
- `PackageTrainingReviewCandidate.ps1`：以已驗證 R3 DLL 封裝新的 AI 訓練隔離候選。
- `StageChao8SideTrainingRuntime.ps1`：由 Chao8Side Runtime 封裝 DIA_COMPAT 訓練執行環境；只接受隔離候選。
- `StageModelConverter.ps1`：準備模型轉換 Runtime。
- `StageShuntFixtures.ps1`：準備 Shunt 隔離測試資料。
- `StageShuntTrainer20260930.ps1`：準備固定版本的 Shunt 訓練器。
- `StageTrainingData.ps1`：準備訓練影像資料。
- `StageTrainingSeed.ps1`：準備訓練 seed。
- `TestDeltaGateModel.ps1`：在隔離 Gate 驗證 Delta 模型載入。
- `TestDeltaModelExperiment.ps1`：驗證隔離實驗 DLL 的模型載入及檢測。
- `TestDiaCandidateInspect.ps1`：在隔離 Gate 對原廠或候選模型執行 Inspect。
- `TestDiaCandidateReadModel.ps1`：在隔離 Gate 驗證候選模型 ReadModel。
- `TestNativeLazyInit.ps1`：驗證 R3 原生延遲初始化與隔離 Recipe。
- `TestPortableReview.ps1`：驗證攜帶候選的搬移與操作；會建立測試輸出。
- `VerifyDiaCcd1Provenance.ps1`：核對 CCD1 資料、模型與實驗來源。

## Tools 中的 Python 工具

- `Tools/InspectNativePE.py`：靜態檢查 PE 匯入與匯出。
- `Tools/ResolveDeltaImports.py`：分析 Delta Runtime 原生匯入依賴。
- `Tools/ProbeDeltaRuntime.py`：在子程序檢查 Delta Runtime。
- `Tools/EvaluateDiaCcd1Iou.py`：CCD1 隔離 IoU 評估；原根目錄的同名檔已移入 Tools。
- `Tools/ccd1_iou_runner_v1.py`、`Tools/test_ccd1_iou_runner_v1.py`：IoU checkpoint 研究與其單元測試。

歷史驗收紀錄為當時證據，不代表搬移後所有研究工作已重新執行；部分資料、GPU、外部來源和模型不在此副本內。
