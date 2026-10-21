# DIA 檢測執行環境｜路線校正後交接

> **2026-10-22 02:18 後續執行：** 已連上 CHAO2，讀取 AGENTS、R1 路線、近期交接與 ProjectHub.Native 三類訓練、Shunt Twin、DIA 相機證據。確認 StageGeneralTrainer.ps1 原預設中文路徑在 Windows PowerShell 5.1 UTF-8 無 BOM 解析會出現 Illegal characters in path，已改用 ASCII 相對兄弟目錄推導；重新執行 staging 被工具安全檢查攔截，**未宣稱 staging PASS**。GeneralTrainingJob.Preflight 新增固定 SHA256 檢查，防止未審核 DAVS 程式被誤用。Build.ps1 Release PASS、原版 INSPECTION TOOLS GUI 正常啟閉、Twin 12/12 PASS、缺 seed 與非法路徑均 exit 74。尚未執行真正 Train。Git 提交操作被工具安全檢查阻擋；下輪先檢查暫存／未提交差異，再處理候選 Train Job，不能將先前 Shunt 的 GPU PASS 冒稱本案驗收。

> **2026-10-22 一般 Train 前置 Gate：** 正式 CLI 新增 --train-general-preflight，強制檢查本案 AppBase/TrainingSeed、TrainingImage X/Y、Python38、recovered DAVS。Build、原版 GUI 啟閉、Twin 12 項、三類資料契約均 PASS；非法路徑／缺 seed 會以 exit 74 拒絕。StageGeneralTrainer.ps1 已寫入並通過語法解析，但執行被工具安全檢查阻擋，尚未完成 staging；5.2GB GPU Python 未複製，**未真正執行 Train**。下一輪先做 staging 與候選 Train job。詳 docs/GENERAL_TRAIN_PREFLIGHT_GATE_20261022.md。

> **2026-10-22 凌晨設備 Twin + Train 合約：** 使用者要求完整獨立 InspTools（包含 Train），已安排每小時自動接手。ProjectHub.Native 唯讀確認 Shunt PLC/Camera Twin、DIA 固定/循序相機與三套 Training。直接在唯一正式 src/InspTools.Standalone 新增 DeviceTwinPorts.cs 與 TrainingContracts.cs，設備 Twin 12 項離線自測 PASS，General/Locate/Prophet 訓練資料契約自測 PASS；尚未掛入原版 GUI 或實體 PLC/相機，亦尚未真正開始 Train。下一輪優先 product-owned General DAVS Trainer，禁用真 PLC 寫入與原廠生產檔覆寫。詳見 docs/INSPTOOLS_DEVICE_TWIN_AND_TRAIN_ROADMAP_20261022.md。

> **2026-10-22 台達 AI 實際 OK/NG Gate 突破：** 確認實際 R3 舊 AIEnhanceTool.ReadModel 在模型讀取前強制要求 train.exe/h5_to_pb.exe；隔離新建 Experiment DLL 僅允許預先已備妥的 .h5d+.pbd 跳過此無關訓練前置檢查。AppBase 原生 Runtime 加入 cuDNN 動態推論 DLL 與相對目錄預載後，真正 S1 Model Success=True/Loaded=True、真實 S1 OK Bin=Ok、S1 NG Bin=Ng2（BinaryTools5/AR溶融），定位皆成功；TestDeltaModelExperiment.ps1 測後已恢復固定 R3 SHA 與模型 bytes。仍只是一站兩張隔離資料，不是八站量產驗收，正式 release/current 與 Shunt 均未修改。完整證據 docs/DELTA_PRECOMPILED_MODEL_INSPECT_GATE_20261022.md。

> **2026-10-21 最新 Delta App-local Gate：** 已在 `output/delta-native-gates/Gate-20261021-234640` 實際複製台達 DLL 和 PE 匯入表要求的 GPU Runtime 子集；補上 `cublasLt64_11.dll` 後 **InspTools 自身 C# CLI `DELTA_NATIVE_RUNTIME=READY`**。S1 真實 Recipe/模型已匯入隔離 Gate，`LoadConfiguredModel(1, Enhance)` 能返回，但 `Success=False Loaded=False`，顯示舊 `Training\Train\train.exe`/PB 轉換工具提示；反查 `.h5d/.pbd` 檔名和檔案都存在。**仍未達模型載入/Predict Gate**，不要反編 NVIDIA。測試與下一步詳 `docs/DELTA_APP_LOCAL_RUNTIME_GATE_20261021.md`；正式 release/current 與 Shunt 均未修改。

> **2026-10-21 台達原生 AI 獨立化：** 使用者指定 `E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime`，已唯讀找到台達 DeltaAIProcessDll、OpenCV、CUDA11、TensorRT 及 ModelConverter，不必反編譯 NVIDIA。用自製純 PE 靜態稽核確認 x64/84 exports，以限時隔離程序對**同一份台達 DLL 真正執行載入 → CreateInstance → DisposeInstance，PASS**。InspTools 新增 `DeltaNativeRuntimeBootstrap`，僅使用本 EXE 的相對 Runtime，預設懶載入；重編譯與無 AI Runtime 的原版 GUI 啟閉 Exit 0。**C# 自帶 AI Runtime 的 READY Gate、模型 ReadModel/Inspect 尚未驗收**，沒有把大型 DLL 打進候選版。完整資料 `docs/DELTA_NATIVE_RUNTIME_INDEPENDENCE_20261021.md`；原 Shunt Runtime 與 release/current 均未修改。

> **2026-10-21 最新獨立候選版（以此為準）：** 使用 `BuildStandaloneR3.ps1 -Zip` 產生獨立的 `output/standalone-r3/InspTools-Portable-R3-YYYYMMDD-HHmmss/` 及同名 ZIP。與可清空的 `output/portable-review` 不同，**每次都新建、不覆蓋使用者 Recipe**。原版 GUI 零參數啟停 Exit 0，獨立 Recipe 匯入／檢查／S5 真實八站、R3 原版定位編輯選單 3 PASS，同機異路徑搬移／載入再次通過。套件內有 25 個 EXE/DLL、約 89.86 MiB、空白 Recipe/Model/PublicDB/TrainingImage，完全不附 Shunt 正式資料。新的打包與操作說明在 `docs/PORTABLE_R3_CURRENT.md`。完整 Inspect、AI Model、OK/NG 尚未驗收；`release/current` 不變，不能把候選版冒稱正式。

> 更新：2026-09-16 00:05 +08:00

> **2026-10-21 產品目標重申與 R3 Portable Gate：** 唯一產品是**獨立 InspToolsStandalone.exe**，ShuntVC 僅提供測試素材。新增 `BuildPortableReview.ps1` 輸出無 Shunt 資料的 89.86 MiB R3 攜帶版，`TestPortableReview.ps1` 已在另一 AppBase 完成 14 PASS、0 FAIL（原版 GUI、匯入 Recipe、真實八站載入與關閉）；詳 `docs/R3_PORTABLE_INSPTOOLS_GATE_20261021.md`。本輪只證明同機搬移，非跨機、AI 推論或 OK/NG 全驗收；正式 `release/current` 未覆蓋。

> **2026-10-21 ProjectHub.Native 歷史成果接入：** 已唯讀核對 `ProjectHub.Native/ProjectData` 的現役 `InspTools-Rebuild2` 與第一期已退役 `InspToolsRebuild`，確認第一期 Stable API（InspectFile/InspectToolPreview/Recipe transaction）與 29 份解譯 XAML、15-dialog 舊驗收等珍貴證據。ProjectHub.Native 中 LocateTool.cs SHA 與 R3 pinned source 一致；**R3 patched DLL 實際含 InspToolsFacade 與九項核心方法**，但只完成 API 存在核查，尚未以此 facade 執行 R3 Inspect。請先讀 `docs/PROJECTHUB_NATIVE_EVIDENCE_AUDIT_20261021.md`，勿複製整包、勿將第一期 ACCEPTED 冒稱新版已通過。ProjectHub.Native 本身未修改。

> **2026-10-21 R3 Shunt 真實資料 Gate：** 已讀取 ShuntVC 目前 Recipe/3923-R002 與 R002-CCD1 原始 OK/NG 圖各一張，隔離匯入 Recipe 21 檔，原版 OpenCvSharp、CVBuffer、BinaryWindow 顯示真圖及 ROI 原廠儲存防呆通過；原版 BinaryWindow 修改 TrackerName → SaveAllRecipe → 新 CVInspection 重新讀取 → byte-for-byte 還原 24 PASS／0 FAIL。詳細紀錄 docs/R3_SHUNT_REAL_IMAGE_BINARY_SAVE_20261021.md。**模型載入及 Inspect 未驗，正式 ShuntVC 與 InspTools release/current 都未覆蓋。**

> **2026-10-21 R3 二值化存讀突破：** 在 Shunt CCD1 真實 NG 圖與有效 ROI 下，原版 `BinaryWindow.txt_TrackerName` 修改可進 S1 ParamFile，呼叫原版 `SaveAllRecipe` 後，全新 `CVInspection.SetRecipe` 讀回相同值；隔離 Recipe 和 PublicDB 逐位元組回復 PASS，無 Shunt/Chao8Side 寫回。見 `docs/R3_SHUNT_OFFLINE_IMAGE_GATE_20261021.md`。並不代表已執行 `Inspect()`。

> **2026-10-21 R3 實際離線影像進度：** 已從 ShuntVC 唯讀取得 CCD1 OK/NG 兩張原始影像，於隔離 R3 使用 OpenCvSharp 解碼，原版 CVBuffer + BinaryWindow 均成功顯示、關閉，原版 ROI 的 DontSave1/2 與 DontSave 均 False，八站 Recipe 原始檔雜湊保持不變。尚未執行 Binary 編輯保存或 Inspect/分類。詳 `docs/R3_SHUNT_OFFLINE_IMAGE_GATE_20261021.md`。

> **2026-10-21 R3 隔離編輯入口：** 新增明確啟用的原版 `LocateWindow` 八站編輯選單 `--enable-r3-editor`，透過原版 `SaveAllRecipe` 加本地完整 Recipe／PublicDB 備份、八站 FocusLimit／BinaryToolON 重讀及異常回復。選單自動化 3 PASS，底層真實定位參數 UI 修改與保存已 12 PASS。尚未驗新選單的人工完整儲存／撤回交互，不可升正式版。見 `docs/R3_OPT_IN_LOCATE_EDITOR_20261021.md`。

> **2026-10-21 R3 工具編輯／存檔 Gate：** 原版 `LocateWindow`、`BinaryWindow` 接真實 S1 CVInspection 的顯示／關閉 **18 PASS**；`LocateWindow.txt_FocusLimit` 真正編輯 → 原版 `SaveAllRecipe()` → 磁碟／新 CVInspection 重讀 → 原 bytes 回滾 **12 PASS**。二值化原版 `DontSave1=True`（缺有效 ROI）會阻止編輯，**未繞過**。測試報告：`docs/R3_ORIGINAL_DIALOG_SAVE_GATE_20261021.md`。未通過 AI Inspect/全部工具，也未開放正式版儲存。

> **2026-10-21 R3 重大進展：** 一期可編輯原始碼已從歷史 ProjectHub 封存尋回。在唯讀供體的隔離編譯副本中將 AI Locate 原生 CreatInst 改為 ReadModel 按需建立，得到 R3 patched DLL；定位 SetBuffers、S1 SetRecipe、八站 CVInspection/SetRecipe 及真正原版 ToolsWindow 八站 GUI 均 PASS。未做模型與 Inspect，正式 release/current 不變。詳 docs/R3_SOURCE_RECOVERY_20261021.md；Git 已啟用，後續修正可追蹤。

> **2026-10-21 R3 最新：** 本案已建立本地 Git main（基準 bc26e73），一期可編輯原始碼已在 D 槽舊 ProjectHub 封存找到。新增隔離 BuildNativeLazyInit.ps1：將 LocateTool.SetBuffers 的原生 AI 定位實例改為按需建立，隔離 DLL 重新編譯 0 errors；真實 CVInspection 八站 SetRecipe **8/8 PASS**，原版 GUI 使用真實八站物件亦能開啟／關閉（Exit 0）。正式 R1 及所有生產 Recipe 未改。見 docs/R3_NATIVE_LAZY_INIT_GATE_20261021.md；**AI ReadModel/Inspect/原版全工具儲存均仍待驗收**。

> **2026-10-21 最新 R2 Gate：** 真實 WPF UI smoke **22 PASS／0 FAIL**，八站站別及基本編輯 modal 均已進行程式內互動測試；最主要阻塞已追至原版 `LocateTool.SetBuffers() -> LocateAIEnhanceTool.CreatInst() -> DeltaAIProcessDll.CreateInstance [-> SetGPU]` 路徑（LocateTool.SetBuffers 4 秒逾時）。新的 `--runtime-preflight` 及 `--recipe` 全功能載入均 fail-closed(exit 46)，不讓未驗的 native 初始化卡住 UI；原版 Recipe 預覽、原生參數讀寫仍可用。完整證據見 `docs/R2_UI_SMOKE_AND_AI_LOCATE_ROOT_CAUSE_20261021.md`。**下一步應找回可重建 source 再做 AI Locate lazy-init 隔離修正，不可覆蓋正式 release。**

> **2026-10-21 R2 最新：** 原版 `ToolsWindow` 的輕量八站 Recipe 參數預覽（零 `CVInspection()` 建構）已完成 S1/S5 GUI 啟停實驗；底層 `ParamFile.WriteAllParam` 真正修改／重讀／恢復、備份／拒絕無效欄位 PASS。GUI 基本開關編輯選單已編譯、滑鼠互動尚待驗。詳 `docs/R2_ORIGINAL_UI_PARAMETER_PREVIEW_AND_SAVE_20261021.md`。完整 AOI `CVInspection / Inspect / AI` 尚未獨立；正式 `release/current` 未動。

> **2026-10-21 R2 核心進度：** 已在獨立版直接呼叫原生 `ParamFile.ReadAllParam / WriteAllParam` 完成八站讀取及 S1 隔離寫入→重讀→恢復 Gate，並新增可由 GUI 右鍵查看的八站原生參數資訊。詳 `docs/R2_NATIVE_PARAMFILE_GATE_20261021.md`。完整 `CVInspection/ToolsWindow` Recipe 實例化仍未通過，不可宣稱 AOI 已獨立檢測。

> **2026-10-21 R2 Recipe 檔案層進度：** 已實際完成並驗證原版 Recipe 目錄匯入／另存／CLI 查詢，空白 Standalone UI 啟停／程序釋放 PASS；最新實作、測試與尚未驗收 Gate 詳 `docs/RECIPE_STORE_R2_20261021.md`。暫未提升正式 release，原生工具參數寫回和真 Recipe/AI/Inspect 尚需驗證。

> **2026-10-21 補充（不改變 R1/R2 正式路線）：** 已比對最新 Shunt Vision / AI Runtime 與本案獨立版的 DLL 差異、Host/Replica 依賴及三條 AI 完整性。先讀 `docs/SHUNT_REUSE_AUDIT_20261021.md`。Shunt AI Runtime status PASS 不等於獨立版 Inspect 已完成；舊 Phase-1 建置來源路徑目前已失效，修正來源前不得覆蓋 R1 release。

## CURRENT TRUTH

本案正式名稱已改為 **DIA 檢測執行環境**。它是 DIA 主線的重大分歧，定位是檢測執行層；一期 `InspToolsRebuild` 是技術基底與逆向證據來源。

目前 ProjectHub 還沒有成熟的母子專案機制，因此只記錄 `parent_project_hint=DIA`，不另外建立假的父專案/子專案結構。內部 ProjectHub ID 暫保留 `InspTools-Rebuild2` 以維持歷史追溯。

本案已從先前 machine-first / generic editor / annotation 擴張線拉回原始目標。

**唯一正式產品：`InspToolsStandalone.exe -> decoded/rebuilt InspTools.ToolsWindow`。**

不要把標註專案、Agent、SAM、ReferenceHost、DIA-like/Shunt-like Host 或白色 `Recovered Tool Workspace` 再接回正式入口。

## 現在從哪裡開

正式交付：

`D:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯\release\current\InspToolsStandalone.exe`

## R1 已驗收

從 `release\current` 零參數啟動：

```text
Alive=True
Title=INSPECTION TOOLS
ErrorFile=False
DIAChip8Side=0
DIAVision=0
ShuntVC=0
Replica=0
ReferenceHost=0
StandaloneCount=1
```

Standalone SHA256：

`668160FF64970778660CB7826E2B146A520E8AFAD7FF0BB19DA998F61B09A11A`

目前 pinned recovered `InspTools.dll` SHA256：

`85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1`

正式 ZIP：

`release\DIA-InspectionRuntime.zip`

ZIP SHA256：

`7145124F46B1A1F62C8AC3A31B5BB5E4B708BC0A1A9ABACAF18A18085BD559E4`

ZIP bytes：`34,043,575`。

## 一期嚴格確認結果

一期 evidence 已重新讀 source 驗證，不只引用舊摘要：

- `ReplicaLoader.LoadCompiledToolsWindow()` 真的是直接 instantiate `InspTools.ToolsWindow`。
- UI-13 明確指定 compiled ToolsWindow 為 production visual reference。
- original AOI child dialogs 是一期成果。
- `InitializeCompiledRecipeLifecycle()` 已有真實 Recipe lifecycle。
- Replica 本身只是 launcher/lifecycle/staging/regression harness。

完整文件：`docs\PHASE1_BASELINE_AUDIT_20260915.md`。

重要：一期 handoff 記錄的 accepted DLL `0B4E...` binary 目前未在一期專案樹中找到；現行 `85C4...` 是可重現、可實跑、已 pin 的 current recovered snapshot，**不可混稱 accepted immutable binary**。

## 正式 source / build

正式 Solution 只剩：

`src\InspTools.Standalone\InspTools.Standalone.csproj`

而該 csproj 只 compile：

- `StandaloneEntry.cs`
- `DecodedToolsWindowHost.cs`

`Build.ps1` 只 build 這一條線，結果：

```text
route=phase1-compiled-toolswindow-standalone
formal_projects=1
off_route_projects_built=0
physical_mb=89.77
```

## 二期偏航已切線

已直接刪除會誤導正式入口的 source：

- 舊白色 Generic Workspace `src\InspTools.Standalone\Program.cs`
- 混線版 `OriginalCompiledUiHost.cs`
- 誤建立的 `InspTools.AnnotationStandalone\Program.cs`

Core / SDK / WPF / ReferenceHost / DIA-like / Shunt-like / Agent / AgentBridge / SamOnnx 等舊成果仍保留在 source tree 作歷史證據，但已從正式 Solution / Build / Release 全部排除。

正式 release forbidden binary audit：`0 hits`。

## 下一步只能照這個順序

1. **R1 已完成，不再重畫 UI。**
2. 下一步若使用者要 Recipe，做 R2：把一期 `InitializeCompiledRecipeLifecycle()` 最小化移植到 Standalone。
3. Recipe 必須是附加載入；空白原版 UI 永遠可直接開。
4. R2 驗收後才做 R3 portable runtime dependency 瘦身。

不要先做 Inspect 重構、Agent、Annotation、MCP、SDK Host 或新的 Tool Editor。

## ProjectHub 狀態說明

ProjectHub 正式顯示名稱改為 `DIA 檢測執行環境`；內部 ID 暫保留 `InspTools-Rebuild2`。歷史 `IR2-01~IR2-06` 仍為 DONE/ACCEPTED；那些卡驗收的是舊 machine-first 路線，**不等於本次 route reset 的驗收**。

新的 R1 route-reset evidence 保存在本專案 `output\ROUTE_RESET_R1_ACCEPTANCE_20260916.md`。後續新工作使用新的 `DIAIR-*` Task，不修改已 ACCEPTED 的 `IR2-*` 歷史卡。

