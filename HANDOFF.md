# DIA 檢測執行環境｜路線校正後交接

> 更新：2026-09-16 00:05 +08:00

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

