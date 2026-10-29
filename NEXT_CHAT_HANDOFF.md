## 2026-10-29 Codex 進度：P0 修正待完整 GUI 驗證

新的隔離測試候選：`output/standalone-r3/InspTools-Portable-R3-20261028-233327`；承接 `215835`，不代表正式 release。Build.ps1 與 BuildStandaloneR3.ps1 編譯成功，已分別 staging 300E／先知 e1，並只在隔離候選匯入 3923-R002。

已修改：`ChaoLocatePreviewUi.RunAsync` 分開傳入原版 ToolsWindow host 和對話框 owner，讀取原版 StationIndex；定位預覽僅允許 S1，選圖後及推論前再次檢查。`ChaoSelfAiBridge` 依原版站別顯示面板與啟用按鈕；`Chao8SideRecipePicker` 在原版站別按鈕事件後同步標題。主程式原版 WPF 介面保留。

GUI 初步證據：載入隔離 Recipe 3923-R002 後，原版標題 S1、原版按鈕 1 與 AI 面板目前 S1 一致；按「定位 300E｜選圖預覽」已開啟 S1 離線選圖視窗，未在這一步顯示「未選定有效站別」。尚未成功完成選圖、推論、定位框及 Mask 切換；S1→S2→S8→S1 實際回歸及 S2／S8 停用檢查也未完成，**不得標記 GUI 驗收 PASS**。本輪 GUI 自動化遇到其他桌面視窗取得焦點，須重新確認視窗焦點後繼續，避免操作其他應用。

使用者提供唯讀測試圖片：`D:\0_軟體開發區\6.模板與共用資源區\原廠母本\DIAChip8Side_Welding\TrainingImage\R002-CCD1\Origional\1\NG\NG-1-2025-06-27-16-55-12.png`。僅 S1／CCD1 可使用既有定位模型；不能將該模型用於其他站。完整 Inspect／CVBuffer 整合仍未完成，兩條 AI 既有 smoke 不能代表完整檢測驗收。請保留原有所有 dirty worktree 與正式 Recipe／Model／release/current。

> **2026-10-28 Codex 正式交接入口／優先於全部歷史段落：** 使用者要求交接給 Codex。請先讀根目錄 `CODEX_HANDOFF.md` 和 `docs/CODEX_HANDOFF_CHAO_AI_20261028.md`；最新候選 `output/standalone-r3/InspTools-Portable-R3-20261028-215835`。自研 UNet 300E 真實 checkpoint 已找回、SHA 核對、CPU S1 真圖 Mask/ROI Smoke PASS；先知 e1 ONNX Mask Smoke PASS，但均非正式 Inspect。P0 BUG：原版主畫面 S1 與 CHAO 面板 S4 不一致；`ChaoSelfAiBridge` 誤用 `RunAsync(dialog, root)`，將面板代替原版 host，故選圖預覽回報「未選定有效站別」。Codex 先修站別／傳參及 CCD1 S1-only 防呆，再做實際 GUI 選圖→Mask/Overlay 回歸。切勿重訓、清理 dirty Git 或覆蓋正式版。以下 195627 等資訊是歷史版本。

> **2026-10-28 最新 CHAO 自研 AI 先知真圖 Gate：** `Delta-AIProphet\Output\real-e1\model.onnx` 原本有 16-byte DAVS header，直接交 OnnxRuntime 會 InvalidProtobuf；不是模型損壞。新 `scripts/StageChaoProphetOnnx.ps1` 唯讀去封裝到隔離候選（原 SHA `230C78...` → 真 ONNX SHA `1212DCE...`），現有 `ShuntVC_Chao\Runtime\Microsoft.ML.OnnxRuntime.dll` 解析 input uint8[1,256,256,3]、outputs Mask uint8[1,256,256]+兩個 byte scalar 成功。新增 `ChaoProphetOnnxSmoke.cs` 與 `TestChaoProphetOnnx.ps1`，實際 Shunt NG 圖推論 `INFERENCE_SMOKE=PASS`、Mask 非零 65406、RAW_OUTPUT2=136/OUTPUT3=3，PNG/Report 已留存。最新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-195627` Build/Package PASS，先知 Runtime Health 同樣 PASS。**仍是 1 epoch，非品質/NG 判定 Gate、未接到 GUI Mask 回填；AI 定位 300E 真正 checkpoint 未找到。** 完整詳 `docs/CHAO_SELF_AI_PROPHET_E1_INFERENCE_20261028.md`。舊的 CHAO self AI 控制層入口文件依 `docs/CHAO_SELF_AI_BRIDGE_GATE_20261028.md`。

> **2026-10-28 最新 CHAO 自研 AI 接線 Gate：** 使用者決議不要依賴 Chao8Side 原版 AI 授權，改在 InspTools 原版 UI 下使用 **CHAO 自研定位 UNet、AI 先知 Runtime**。新代碼 `src/InspTools.Standalone/ChaoSelfAiBridge.cs`；原版 48×48 機器人按鈕已接到獨立 `CHAO AI｜自研定位與 AI 先知` 面板（GUI 原生滑鼠 PASS），面板可測自研 `ShuntVC_Chao\Runtime\Runtime\AIRuntime\ChaoAIRuntime.exe`。最新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-194415` Build/Package PASS；`InspToolsStandalone.exe --chao-ai-health` 實際 Process Exit 0，`PROPHET_STATUS=READY`、TorchCUDA/CUDA12/cuDNN9=true，`RUNTIME_HEALTH=PASS; INFERENCE=NOT_TESTED`。找到自研先知 `Delta-AIProphet\Output\real-e1\model.onnx`（僅 1 epoch smoke），定位 300E 自研 checkpoint/真圖推論 adapter 仍待驗。舊 `CHAO8SIDE_AI_PARITY_GATE` 原廠授權分支只供研究，不再是此專案主線。詳 `docs/CHAO_SELF_AI_BRIDGE_GATE_20261028.md`。**嚴禁宣告 AI 定位／AI 先知推論已完成。**

> **2026-10-28 Chao8Side AI 定位／先知最新 Gate：** 對齊原版 `D:\delta\Chao8Side\InspTools.dll` 1.3.16.28。已建立獨立候選 `output/standalone-r3/InspTools-Portable-R3-20261028-185252`，將 `3923-R002` Recipe 的 24 個模型和 4 個相依檔案以 SHA 保護方式匯入，加入原版 `CheckAuthNoDLG` 與授權後按需 `locateTool.ReLoadAIModel`／`aiProphetTools.ReadModel` 路由。曾發現根目錄 QR DLL 引發原版 WPF 載入時 NullReference 當機，已做延後載入＋下次開機安全清除；重啟和二次 Recipe 載入 PASS。實際 S1 診斷結果：AI 定位 Recipe=True、Model 具備、**vendor_authorized=False**；AI 先知 Recipe=False、ModelName 空且 Chao8Side 無任何 ONNX。因此**本輪未完成 AI 真實推論**，不可宣稱已修好 AI定位／先知。完整證據 `docs/CHAO8SIDE_AI_PARITY_GATE_20261028.md`。原廠授權不能繞過，無有效先知模型不能假訓練。**切勿使用會當機的舊候選 185028**。

> **2026-10-28 最新 Recipe 按鈕尺寸 Gate：** 新候選 `output/standalone-r3/InspTools-Portable-R3-20261028-182547` 已把原版 Grid 中的 Recipe 資料夾按鈕從 23×23 改為 48×48、圖示 30×30。使用隱藏 Stage10 保留位置，Recipe (742,691) 48×48、複製參數 (800,691) 48×48、AI (865,691) 48×48，三者位置無重疊，原版站別/複製/AI 均未移動；原生滑鼠中央點擊 Recipe 成功開啟原有彈窗。建置和打包 PASS，正式 release/current 未動；詳 `docs/RECIPE_ICON_48PX_ORIGINAL_GRID_20261028.md`。這是尺寸版面 Gate，原先的 DIA Recipe 選目錄與 Shunt S1 Binary 保存 Gate 不重新宣稱全部驗證。

> **2026-10-28 最新 Recipe 可選根目錄 Gate：** 候選 `output/standalone-r3/InspTools-Portable-R3-20261028-174832`。Recipe 預設 `<EXE 目錄>\Recipe`，可按「瀏覽…」或貼路徑＋「套用」改為外部母本根目錄。GUI 已驗 `D:\0_軟體開發區\6.模板與共用資源區\原廠母本\DIAChip8Side_Welding\Recipe` 下四個 Recipe 列舉，選 `3923-R002` 匯入本機 21 檔與八站 S1 初始化 PASS；來源 Setting.ini / station1.ini SHA 沒改。原廠母本唯讀，禁止直接寫回，詳 `docs/RECIPE_SELECTABLE_ROOT_20261028.md`。目前 PID=25920 開啟新候選的 Recipe 選擇視窗，已套用 DIA 母本路徑。

> **2026-10-28 最新原版 GUI 保存／重啟讀回 Gate：** 最新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-173228` 已完成：原版 Recipe 載入 → `Ctrl+O` 原生讀真圖 → 原版二值化視窗改 TrackerName → 關閉 → `Ctrl+S` 原版 SaveAllRecipe（先備份、重讀）→ 結束程式 → 新進程重新載 Recipe → 原版二值化視窗讀回修改值 **PASS**。正式來源 SHA 未變。另 `172406` 自動化真圖二值化 Gate 24 PASS/0 FAIL。詳 `docs/ORIGINAL_GUI_BINARY_SAVE_READBACK_20261028.md`。目前僅 S1 Binary Gate；其他工具、AI 先知及 Inspect 未驗，不可宣稱整套完成。

> **2026-10-28 原版 Grid Gate（前一候選）：** 已撤回 Recipe 48×48 外層浮層和原版 AI/複製參數/OK/NG 按鈕右移，改將 23×23 Recipe 圖示直接加入原版 `grd_Main` 空隙。舊候選 `output/standalone-r3/InspTools-Portable-R3-20261028-170328` 已驗證載入 Recipe、定位與二值化開窗、站別切換；更完整的保存驗證請以頂部最新 Gate 為準。

> **歷史候選，請勿採用：** `20261028-163834` 為外層 48×48 Recipe 浮層與按鈕右移版本，已撤回，原 `docs/RECIPE_ICON_LEFT_PARAMCOPY_20261028.md` 僅供問題追溯。

> **2026-10-28 本輪原生按鈕 Gate：** 新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-162611` 真正滑鼠點圖示中央可開啟原版二值化與定位視窗；不再用反射 Invoke。僅 marker+環境變數實驗模式生效；影像／編輯保存／重啟回讀、其他工具與 AI 先知尚未驗。下步先做二值化真圖保存回讀。詳 `docs/ORIGINAL_NATIVE_CLICK_GATE_20261028.md`。

> **目前接手入口（2026-10-28）：** 先讀 docs/HANDOFF_ORIGINAL_UI_20261028.md。使用乾淨開發版；使用者要求原版按鈕可直接編輯，Recipe 採彈窗，側欄已撤回。定位與站別只有隔離實驗，二值化及完整保存尚未完成。下方舊研究計畫為歷史（本次追加 Gate 的二值化點擊結果以頂部為準）。

> **2026-10-28 最新原版按鈕實驗：** 隔離副本已實測原廠定位回呼、原版站別切換；二值化仍未觸發，其餘及保存回讀未驗。不得宣稱完成；詳 docs/ORIGINAL_INPUT_EXPERIMENT_20261028.md。

> **2026-10-28 最新使用者校正：** 撤回 Recipe 側欄，原版介面只新增 Recipe 彈窗入口；原版工具直接操作仍未完成。以 docs/RECIPE_DIALOG_RESET_20261028.md 為準。

> **2026-10-28 Recipe 側欄候選：** 左側清單、載入及收合已在隔離候選 GUI 驗證；搜尋、匯入、存檔回讀與同工作階段切換仍待完成，詳見 docs/RECIPE_SIDEBAR_20261028.md。原版大多工具仍未恢復。

> **2026-10-28 使用者要求恢復原版工具操作：** 已在隔離 Recipe GUI 實測原版定位圖示可打開定位視窗；仍有原版站別、其他工具、影像與保存回讀未完成，詳見 docs/ORIGINAL_LOCATE_BUTTON_20261028.md。

> **2026-10-28 腳本整理：** PowerShell 已由專案根目錄移到 `scripts/`；功能與新命令見 `scripts/README.md`。歷史文件中的舊路徑需換成 `scripts/`。`EvaluateDiaCcd1Iou.py` 移至 `Tools/`。

# 下一對話交接｜DIA 檢測執行環境（InspTools二次解譯）

> **2026-10-22 最新優先事項已切換為 GUI／Recipe 真正可操作性：** 使用者反映任意按工具可能當機，而且上一輪未完成 Chao8Side Recipe 選取介面移植。原始 RecipeWindow.xaml / RecoveryBehavior.cs 已從 D:\delta\Chao8Side 找到。新 src/InspTools.Standalone/Chao8SideRecipePicker.cs 已新增可見 Recipe／站別／定位／儲存入口；R3 最新候選 output/standalone-r3/InspTools-Portable-R3-20261022-153927，Build PASS、實際 Recipe 選擇視窗出現且程序 Responding=True。**還沒有完成匯入→切換→定位設定→存檔→重啟回讀的 GUI 全鏈驗收，更沒有解封其他原版工具；目前只是候選。** 詳 docs/CHAO8SIDE_RECIPE_PICKER_R3_20261022.md。優先修 GUI，不要跑新的模型訓練。

> **2026-10-22 使用者 GUI 現場驗收發現重大問題：主介面按鈕看似可用、實際多數無效。** 先讀 docs/UI_BUTTON_DIAGNOSIS_20261022.md。零參數只是八個未初始化的 CVInspection 空白 Shell；R3 唯讀/編輯路徑會刻意阻止原版 Button 滑鼠事件，Recipe 入口隱藏於右鍵。**不要再宣告獨立 GUI 功能完成**；優先接回原版 DIA/Shunt 已驗證的 Station/Recipe/Buffer 與子視窗流程，從真實 GUI 點擊及存檔回讀驗收。模型研究暫緩。

> **2026-10-22 ProjectHub 複用稽核完成：** 先讀 docs/PROJECTHUB_DIA_SHUNT_REUSE_AUDIT_20261022.md。DIA/Shunt/AgentVisionLab 已有 X/Y dataset audit、Loss6、best train-loss checkpoint、quality_gate、model compare、GPU Runtime、H5D/PBD 能力；不要重做。唯一尚未找到的是訓練中根據 validation R IoU 選 checkpoint。新 IoU v1 已有 1 epoch GPU smoke + 原版 Shunt quality_gate 指標 parity PASS，**不要再重跑相同 smoke**；尚未有長訓練模型。下一步收斂最小 callback、擴充獨立 NG 家族與盲測資料。所有候選禁部署。

> **2026-10-22 新對話實作增量：** 已另建版本化 IoU checkpoint 選模入口 RunCcd1IoUSelection.ps1 + Tools/ccd1_iou_runner_v1.py，沿用 SHA 固定的原訓練器，依獨立家族驗證 R IoU 選最佳研究權重。預設資料稽核與 CPU 單測 PASS，拒絕覆蓋舊 job PASS；**1 epoch GPU smoke 完成**、R IoU=0.444620、保存研究 checkpoint/H5、以原版 Shunt quality_gate.py 獨立重算一致 PASS。這只是功能測試且品質不足，**沒有新的 300 epoch IoU 驅動模型**。完整證據 docs/CCD1_IOU_CHECKPOINT_V1_20261022.md。下方「第一個實作任務」為原交接時的歷史計畫；下一步改為新選模的長訓練研究、擴增 NG 家族和全新盲測集。原廠與 300/500 epoch 比較結論維持，禁止部署。

> **交接時間：2026-10-22，台灣時間。接手節點：@CHAO2（HOME-1）。**  
> **最重要的下一步：驗證 IoU 來選最佳 checkpoint；不要再直接增加 epoch，也不要把低訓練 Loss 當模型品質 PASS。**  
> 這是**獨立 DIA InspTools** 專案；不要誤接手 ChaoInterior／ShuntVC 原專案。

## 1. 接手入口與必讀順序

Windows 專案絕對路徑：

`E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯`

新 Agent 先在 @CHAO2 依序閱讀：

1. `AGENTS.md`（本案最高優先路線與禁止事項）
2. `docs/ROUTE_RESET_20260915.md`、`docs/PHASE1_BASELINE_AUDIT_20260915.md`
3. **本文件** `NEXT_CHAT_HANDOFF.md`
4. `HANDOFF.md` 頂部「2026-10-22 最新」區段（下方舊 NOT_RUN 是歷史）
5. **核心研究與 500 epoch 結論：** `docs/SHUNTVC_TRAINING_20260930_RECOVERY_20261022.md`（尤其第六節）
6. 視需要讀 `docs/DIA_CCD1_IOU_300_EPOCH_20261022.md`、`docs/DIA_CANDIDATE_CONVERSION_READMODEL_20261022.md`、`docs/DIA_CANDIDATE_INSPECT_AB_20261022.md`。

本案正式程式目標是保留**原版 compiled `InspTools.ToolsWindow`**，零參數由 `InspToolsStandalone.exe` 啟動；不是自行重畫一套 Editor。R2 Recipe / R3 可攜 Runtime 可用，但生產八站和真機連線尚未完成量產驗收。正式 Build 僅 `src/InspTools.Standalone` 一個 project，`Build.ps1`。

## 2. 最後已確認的事實（不可和推測混淆）

### CCD1 資料與評估規則

- 原廠來源 **唯讀**：`D:\delta\Welding\DIAChip8Side`，Recipe `3923-R002`，本站模型 `R002-CCD1`。
- 使用 `TrainingImage/R002-CCD1` 的 X/Y **真正配對標註**：全部 21 組 OK + 32 組 NG；其中 NG 32 張包含 H/V/VH 翻轉，實際僅 8 個原始 NG 家族。
- 訓練集：**19 組 OK + 24 組 NG = 43 X/Y pairs**，共 **25 個原圖家族**；NG 僅 **6 個獨立家族**。
- 完全保留 **2 OK + 2 NG** 原圖家族，不允許同一家族翻轉跨訓練與驗證。資料／SHA manifest：`output/decoded-ui/Release/TrainingPlans/DIA_CCD1_S1_Train300.json`。
- 主要品質評估採 **ShuntVC 2026-09-30 的原版 `AgentVisionLab/quality_gate.py`**、**R 通道 IoU**、閾值 **220/255**、最小面積 10；各候選使用同一份四個家族的 X/Y holdout。
- `Ng2` 是 Recipe 後段 BinaryTools 等工具的最終判定，**不是** AI 分割 IoU。單看 NG2 或低 Loss 都不足以放行模型。

### 真正做過的 300 → 500 epoch 實驗

| 模型 | 訓練輪數 | 最佳訓練 Loss | 訓練集 R IoU | 保留集 R IoU | 保留集 R Recall |
|---|---:|---:|---:|---:|---:|
| 原廠 CCD1（對照） | — | — | — | **0.9406** | 96.20% |
| 舊 9/14 RecoveredDAVS / Loss 1 | 300 | 4.406e-6 | 0.9987（舊簡化評估法） | 0.2495（Shunt 原版品質工具） | 25.20% |
| **Shunt 9/30 AgentVisionLab / Loss 6** | **300** | **2.67694e-6** | **0.9742** | **0.7771** | **80.83%** |
| **Shunt 9/30 AgentVisionLab / Loss 6 續訓** | **總計 500** | **2.11821e-6** | **0.9741** | **0.5769** | **60.66%** |

- 使用者要求 **500 epoch 且 Loss < 1e-5**：**確實完成，且達標**，但 **holdout IoU 下降 0.2002**，未能勝過 300 epoch。
- **300 epoch 工作：** `output/decoded-ui/Release/TrainingCandidates/ShuntLoss6_CCD1_300E_20261022`，狀態 `SUCCEEDED`；Best Train Loss Epoch **298**；模型 `temp_unet_US_plain.h5` SHA-256：
  `3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634`。
- **500 epoch 工作：** `output/decoded-ui/Release/TrainingCandidates/ShuntLoss6_CCD1_500E_20261022_R1`，狀態 `SUCCEEDED`；從 300 epoch 最佳權重續訓 **200 輪**（`initial_epoch=300`，`epochs_completed=500`），Best Train Loss Epoch **499**；SHA-256：
  `84D97A39705ADFE59DE14DB5715F293F4897B64E5CF32769644CBBB76D850410`。
- 500 epoch 接續**只承接模型權重與進度歷史，不承接 Adam optimizer 動量狀態**。不可宣稱是 optimizer 逐位元不中斷的 500 epoch。
- 首次 500 工作 `ShuntLoss6_CCD1_500E_20261022` 是 **FAILED**，原因是 Windows PowerShell 把 `--resume-progress` 中文路徑引號處理錯；已改用相對的本地副本，成功工作是 **帶 `_R1` 後綴**的版本。不要誤讀兩者。
- 已保存訓練 logs、`training_result.json`、`progress.json`、`best.weights.h5`、`input_provenance.json` 與模型，皆在對應 job 目錄。保存期限未指定，不要主動清理。

### 品質 Gate：嚴格禁止誤部署

- 300 epoch：`quality_recovery_result.json` = `IMPROVED_LOSS6_STILL_NOT_QUALIFIED`。
- 500 epoch：`iou500_quality_gate.json` = `HOLDOUT_IOU_REGRESSED_KEEP_300`。
- 結論：**暫時以 300 epoch 模型作較佳自研研究基準，原廠 CCD1 仍是目前品質基準；兩顆自研模型都不可部署**。原廠模型 SHA 未被改動，未連線 PLC/Camera，沒有更改正式 Recipe / release/current Model。
- 4 張驗證圖只有 **2 個真正 NG 家族**；不支援量產良率或召回率的統計推論。

## 3. ShuntVC 當時真正解決的問題，不可重做錯版

- **舊來源（不要再用來長訓練）：** `E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime\TrainingRebuild\train_compat.py`，9/14 舊版 `RecoveredDAVS`。本案原本把它當新版，造成方向偏離。
- **正確來源（唯讀）：** `D:\delta\ShuntSensor\外觀機\Runtime\ShuntVC\Runtime`，9/30 後的 `AgentVisionLab`、RGB 標註保留與 `quality_gate.py`。
- 已執行 `StageShuntTrainer20260930.ps1`，將六個 **固定 SHA** 的 Shunt 版本腳本搬進**本案自有** `output/decoded-ui/Release/Runtime/`，含 `AgentVisionLab/trainers/temp_unet_trainer.py` 與 `quality_gate.py`。絕不可直接在 ShuntVC 原目錄修改程式。
- **Loss 6** 是稀疏瑕疵加權：高信號像素提高約 10 倍權重、略過中間不確定區間；Loss 1 MSE 容易被背景主導。**已實測原本 Shunt 自己的 300 epoch Loss 1 vs Loss 6，在 Shunt 55 筆資料上 R IoU 分別約 0 與 0.913，但那是訓練集，不是 holdout。**
- `RunShuntCcd1Loss6.ps1` 已擴充 `-Epochs 1|300|500` 並支援 500 續訓；**不要立即重跑**，固定 job 名已存在，預設拒絕覆寫。要新實驗請新建獨立 job 並比對來源 manifest / seed。
- 舊的 `--train-general-plan-long300` 入口**已拒絕使用 9/14 RecoveredDAVS 再訓練**，歷史候選可留作回歸。

## 4. 實測通過及限制

- 2026-10-22 最新 `Build.ps1`：**PASS**，輸出 `output/decoded-ui/Release/InspToolsStandalone.exe`（約 89.96 MB staged build、僅一個正式 project）。**不是**正式 `release/current` 自動升版。
- `InspToolsRecipeCli.exe --train-contract-selftest`：PASS（含遞迴 `ALL`、X/Y 相對路徑配對、路徑防護）。
- `--train-general-selftest`：PASS（不含重新 GPU 訓練）。
- `--train-general-executor-selftest`：PASS（停止/逾時）。
- `--device-twin-selftest`：12/12 PASS、外部連線 0。
- `AuditShuntLoss6_500.ps1`：**PASS**，顯示 `HOLDOUT_IOU_REGRESSED_KEEP_300`，含模型 SHA/續訓歷史/IoU 同條件檢查。
- 9/30 `AuditShuntLoss6Recovery.ps1` 曾 PASS 並重跑輸出正確結論；如新對話再次核驗時需要較長讀取時間，避免把工具限時誤判成訓練失敗。
- **未完成**：最佳 checkpoint 由 holdout IoU 選取、真正獨立批次／更多 NG X/Y 標註、八站完整實體設備測試、量產模型資格；切忌宣稱已完成。

## 5. 新對話第一個實作任務（優先序）

1. **P0：接手核驗。** 讀本案 `AGENTS.md`、最新 doc、現有 Git 工作樹和兩個品質 Gate；僅重新執行只讀驗證，不覆寫模型／重跑 GPU。
2. **P1：訓練中監測 holdout IoU + checkpoint 選擇。** 在本案自有 `AgentVisionLab` 派生訓練器中**增加版本化的驗證迴圈**，按原始家族切分、例如每 10～25 epoch 保存 checkpoint，以保留集 R IoU（或獨立 NG IoU）而非 train Loss 選模型；每個結果要記 epoch、seed SHA、train/validation SHA、per-channel IoU/precision/recall 和訓練超參數。不修改原版 ShuntVC。
3. **P2：擴充真正獨立 NG 家族。** 尋找更多 CCD1 原始瑕疵標註、先測資料品質與同源影像洩漏；嚴禁「同原圖翻轉版本」跨 train/validation，也不要拿未標註原圖充當 Y。現在 2 NG 家族太少，不能據此認定任何模型可上線。
4. **P3：驗證改善後才做候選 H5D/PBD 轉換與離線 `InspectFile`。** 原版 worker / ReadModel / Inspect 的隔離 Gate 已有早先成功範例，但未經品質資格不得升版至正式 Model。
5. 接手時先向使用者回報：**500 epoch 確實達 Loss 1e-5，holdout IoU 反而低於 300**，故下一步轉向監控/挑選模型，不再單純堆輪數。

## 6. 重啟核驗指令（在上述專案工作目錄執行）

只讀或不動正式模型的指令，逐項執行：

```powershell
git status --short
powershell -NoProfile -ExecutionPolicy Bypass -File AuditShuntLoss6_500.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File AuditShuntLoss6Recovery.ps1
output\decoded-ui\Release\InspToolsRecipeCli.exe --train-contract-selftest
output\decoded-ui\Release\InspToolsRecipeCli.exe --train-general-executor-selftest
output\decoded-ui\Release\InspToolsRecipeCli.exe --device-twin-selftest
```

必要時 `powershell -NoProfile -ExecutionPolicy Bypass -File Build.ps1` 重新編譯；此命令會更新**開發 AppBase**，不是只讀，需先確認沒有進行中的工作。切勿啟動新的長訓練、實體機台或部署工作來「驗證交接」。

## 7. Git／資料安全與紀錄

- 2026-10-22 交接時 branch：`main`，最新已提交 HEAD：`109efc9`（一般 AI 訓練候選執行與停止驗收）。
- **本次大量新增腳本、`src/InspTools.Standalone` 原碼及報告仍是未提交的 Git 工作樹變更**；未宣稱已 commit/push。接手前先 `git status --short`、檢查差異，**不得** `git clean/reset --hard`、批次刪除或在未核對前覆寫目前成果。
- `output/` 包含大型 AppBase、Python/GPU Runtime、資料、候選模型和品質報告，多數被 Git ignore；**Git commit 不會保護這些證據**。不可把此處模型當成 Git 追蹤的備份；如需永久備份，另設經核對 SHA 的受控目的地。
- 來源唯讀：`D:\delta\Welding\DIAChip8Side`、`D:\delta\ShuntSensor\外觀機\Runtime\ShuntVC`、`ProjectHub.Native` 與正式 `release/current`。不更新 @chao3 或 @delta2。
- 所有模型候選只能在 `output/decoded-ui/Release/TrainingCandidates` 的**獨立 job 目錄**寫入；避免把 300 與 500 兩顆模型互相覆蓋。

## 8. 可直接複製到下一對話的接手要求

> 接手 @CHAO2（HOME-1）上 `E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯` 的「DIA 檢測執行環境」。先讀 `AGENTS.md`、`NEXT_CHAT_HANDOFF.md`、`HANDOFF.md` 及 `docs/SHUNTVC_TRAINING_20260930_RECOVERY_20261022.md`。500 epoch Loss 6 已成功且 Loss=2.118e-6，但 holdout R IoU 從 300 epoch 的 0.7771 降至 0.5769；原廠是 0.9406。兩顆候選都禁止部署。先核驗模型 SHA、兩個品質 Gate 與 Git 未提交變更，再從「依獨立 holdout IoU 選 checkpoint」接手開發。不要重做 ShuntVC 訓練器、不要修改 Shunt/DIA 原廠、不要覆寫正式 Model/Recipe，也不要盲目增加訓練輪數。
