# ProjectHub.Native｜InspTools 歷史成果與 R3 重用對照

盤點：2026-10-21。**本次僅讀取 ProjectHub.Native，未修改它的 ProjectData、資料庫或二進位。**

## 身份、專案與資訊優先順序

ProjectHub.Native 實際路徑：

`E:\0_軟體開發區\3.開發中專案區\1.進行中\ProjectHub.Native`

1. `ProjectData/Projects/InspTools-Rebuild2/project.json`：正式名稱「DIA 檢測執行環境」、母案 DIA、技術基底 InspToolsRebuild，生命週期 active。**與本案為同一工作流**，不必重建第三個專案。
2. `ProjectData/Projects/InspTools-Rebuild2/01_最新狀態/LATEST.md`：自動產生於 2026-10-02，當時無 ACTIVE，候選 DIAIR-01。此紀錄**早於本案 2026-10-21 R2/R3 的真實實驗**，不能覆蓋目前 `HANDOFF.md` 的執行 Gate。
3. `ProjectData/Projects/InspToolsRebuild/project.json`：第一期 archived/retired，successor=InspTools-Rebuild2。第一期為來源、已驗證 API 與歷史證據，不應恢復 Replica/Generic SDK 當二期正式產品入口。
4. 本案最新 `HANDOFF.md` + `docs/R3_*.md`：當日實際獨立程式執行與版本庫證據；正式 R1 release/current 仍不得覆蓋。

## 高價值素材：只重用需要的技術

| 位置（均以 ProjectHub.Native 為根） | 找到的東西 | 在 R3 的正確用途 |
|---|---|---|
| `ProjectData/Projects/InspToolsRebuild/Source/InspTools-1.3.16.28-Editable/` | 可重建一期 C# source，含 `InspTools/SecondaryDevelopment/*.cs` | 查 `CVInspection`、既有 API；只在隔離版建置 |
| `ProjectData/Projects/InspToolsRebuild/01_最新狀態/HANDOFF.md` | DEV-04 曾 ACCEPTED、Stable API/工具結果/寫入還原/17 個 XAML Dialog 歷史成果 | 當成第一期驗收證據與回歸需求，不等於目前 R3 已驗證 |
| `ProjectData/SharedKnowledge/InspToolsAPI/INDEX.md`、`stable-api.json` | `InspToolsFacade.LoadRecipe / InspectFile / InspectToolPreview / PreviewRecipeParameterPatch / Apply / Rollback` 等穩定入口 | 後續 R3 走已有 Stable Facade 的隔離檢測，**不要重造獨立 SDK/工具編輯器** |
| `ProjectData/SharedKnowledge/InspToolsAPI/tool-parameters.json`、`recovered-internal-api.json` | Recipe section/key、Tracker、原版 WPF GUI 與內部型別對照 | 原版 Dialog 的跨欄位保存與安全核對 |
| `ProjectHubUi-ax2pzlp0/ProjectData/Other/InspTools-1.3.16.28-Decompiled/src/` | 已展開 **29 個 XAML** | 僅作 GUI 控件、名稱、視覺與 callback 對照；目前沒有看到說明所稱的 `src-rawbaml` 在這個複本中，**不可直接當編譯母本** |
| `ProjectData/SharedKnowledge/InspToolsGUI/NON_AI_DIALOG_CALLBACK_CAPABILITY_20260913.md` | 第一期 15 類非 AI 工具 callback 能力矩陣及未完成項 | R3 各原版窗口逐項驗收，不能把 UI 存在當成 Test/Preview 已完成 |
| `ProjectData/Projects/Shunt-VisionTools/Output/SVT02_LOCATE_BASELINE_ACCEPTANCE_20260913.md` | 117/117 歷史 Locate 實驗、native 初始化問題、通道映射 `0=R/1=G/2=B` | 真正 `InspectFile` 與定位調機的參考測試案例；不套用舊版 Recipe SHA |
| `ProjectData/SharedKnowledge/AIModelRuntime/DIA_LOCATE_TRT_RUNTIME_PLAYBOOK_20260912.md` | TRT capable DeltaAIProcess DLL SHA、CUDA/cuDNN/TensorRT/PE export 與黑圖問題 | 先驗 native ABI，再分站讀模型、真 Predict；不靠模型檔存在判定成功 |
| `ProjectData/SharedKnowledge/AITraining/DIA_AI_GUI_RUNTIME_REUSE_PLAYBOOK_20260914.md` | AI Locate/預分類訓練交換契約、Capability 與 Recipe ON 分離 | 後續接三條訓練 Runtime；不讓 AI Trainer 存在與否阻擋 Recipe/GUI |

## 已直接核對而非推測

- ProjectHub.Native 的 `InspToolsRebuild/Source/.../InspTools/AOI/LocateTool.cs` SHA256 =
  `64D52BC4F04340C67D6CC74F239A6BC781AA47A57261E974913614F8FE623322`。
- 與目前 R3 `BuildNativeLazyInit.ps1` 鎖定的 Phase-1 source **相同**，所以沒有必要再複製一份數百 MB/GB 的專案，也不需要偷偷切換 DLL 版本。
- 2026-10-21 R3 隔離 patched DLL SHA256 =
  `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。
- 已直接從該 R3 DLL 的 .NET metadata 檢查 `InspTools.SecondaryDevelopment.InspToolsFacade` 存在，且有 `LoadRecipe`、`GetCurrentRecipe`、`GetStationSnapshot`、`LoadConfiguredModel`、`InspectFile`、`InspectToolPreview`、`PreviewRecipeParameterPatch`、`ApplyRecipeParameterPatch`、`RollbackRecipeParameterPatch`。**這是 ABI 存在測試，不是 R3 執行成功測試。**
- 第一期報告中的歷史 Stable API DLL `0B4E8C...`、原廠保護 DLL `6BBF3E...`、目前 R1 recovered `85C4D8...`、R3 patched `ACD6DB...`、Shunt production DLL `E8E1AF...` **不能直接視為同一 binary**。API 兼容、native runtime 與模型需要分開驗收。

## 下一輪精準 Gate（保持一個正式 Standalone Project）

1. 在 `output/native-lazy/runtime` 的 **R2PROBE- 隔離 Recipe** 上呼叫 R3 DLL 既有 `InspToolsFacade.LoadRecipe`，比對八站 snapshot；禁止新建通用 SDK/Host 作為正式產品入口。
2. 對同一站真實 Shunt CCD1 OK/NG 樣本試 `InspectFile` 的 **無 AI／明確隔離、限時** 路徑，記錄各工具真實 Bin、Error、LocateSucceeded；沒有結果就報 NOT_RUN 或 FAILED，不把檔名 OK/NG 當推論。
3. 調用穩定交易 API 的 Preview/Apply/Rollback 僅在滿足 guard marker / write opt-in 的 R3 sandbox，對照目前原版 SaveAllRecipe/byte rollback Gate，不使用生產 Recipe。
4. 模型問題再查 `DIA_LOCATE_TRT_RUNTIME_PLAYBOOK` 的 CUDA 11.8、TensorRT 8.6.1、DeltaAIProcess DLL export / ABI，按站最小複製所需檔案，絕不搬整套 ShuntVC。
5. GUI 擴展查 29 個 XAML + 15-dialog callback matrix；以目前 compiled 原版 ToolsWindow 為真實 GUI，禁止回頭做 Generic Editor。

## 治理和儲存原則

- **Git 只追蹤本份索引／必要原始碼補丁與可重現 Gate；不把 ProjectHub.Native、舊解譯 Source、整包 Shunt/模型、ProjectHub.db 當子目錄匯入。**
- ProjectHub.Native 的 ProjectData 是獨立專案與歷史資料，未經明確需求不得搬移、覆寫、回填進度或修改它的 DB。
- 歷史 handoff 有多種互相衝突 SHA、路徑與先前時間點的狀態；遇衝突優先依目前 R3 實際 hash、檢測結果、Current Truth，舊驗收只當實驗假說與參考。
