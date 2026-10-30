# 接手交接：恢復原版 InspTools 工具操作

交接日期：2026-10-28（台灣時間）

> **Codex 最新交接 2026-10-28：** 先讀 `docs/CODEX_HANDOFF_CHAO_AI_20261028.md`。最新候選 `InspTools-Portable-R3-20261028-215835` 已有自研定位 300E 模型和單張 S1 NG 的 CPU Mask/ROI 推論（CLI Smoke PASS），但 CHAO AI GUI 預覽尚不成功：顯示 S1/S4 站別不一致，且 `RunAsync(dialog, root)` 誤傳 CHAO AI 面板導致找不到站別。先修這兩個 P0；先知 e1 ONNX 也僅 smoke、非量產。以下 195627 等候選描述屬歷史。

> **2026-10-28 CHAO 自研 AI 先知 ONNX 真圖推論進度：** 最新候選 `output/standalone-r3/InspTools-Portable-R3-20261028-195627` 已從原有 1-epoch DAVS-wrapped ONNX 唯讀去標頭、用 CHAO 現有 ORT 解析 I/O 並以 Shunt S1 NG 真圖推論生成 256×256 Mask/兩個原始位元組值，Smoke PASS；未完成正式模型品質及 GUI 判定回填。詳 `docs/CHAO_SELF_AI_PROPHET_E1_INFERENCE_20261028.md`。

> **最新 CHAO 自研 AI 入口（2026-10-28）：** 在原版 48×48 `btn_AI_Function` 機器人圖示直接接 CHAO 自研 Runtime 面板，與 Recipe 選擇視窗共存，原版 AI 授權不再作為 CHAO 自研 Gate。隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-194415` 實際 GUI 點擊、`--chao-ai-health` PASS，原生 Runtime 回報 CUDA 可用；定位/先知真正推論尚未完成。見 `docs/CHAO_SELF_AI_BRIDGE_GATE_20261028.md`。

> **2026-10-28 Chao8Side 原版 AI 對齊最新進度：** `output/standalone-r3/InspTools-Portable-R3-20261028-185252` 已只複製 Recipe 所引用的 AI 模型和 Chao8Side 必要相依、恢復授權後原生模型初始化路由，原版 WPF 重啟及二次 Recipe GUI 載入 PASS。原版授權回報 S1 `AllowEnableAILocate=False`，故尚未執行 AI 定位推論；AI 先知 Recipe OFF、無 PModel ONNX，尚未能推論。更重要的是 QR DLL 不可在 Standalone 初始化 Station 前放於根目錄，舊候選 `185028` 會當機，使用 `185252` 延後載入版本。詳 `docs/CHAO8SIDE_AI_PARITY_GATE_20261028.md`。

> **最新 Recipe 尺寸修正（2026-10-28）：** 直接把原版 `grd_Main` 裡的 Recipe 圖示改成 48×48，資料夾圖案 30×30，使用原版八站隱藏的第 10 站保留區，維持與「複製參數」10 px 間距；AI、複製參數與站別按鈕完全不移動。新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-182547`，GUI 尺寸、實際滑鼠點擊開啟 Recipe 視窗 PASS。詳 `docs/RECIPE_ICON_48PX_ORIGINAL_GRID_20261028.md`。下方 23×23 的描述屬歷史候選。

> **Recipe 可選根目錄最新 Gate（2026-10-28）：** 新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-174832`，Recipe 彈窗預設軟體旁 `Recipe`，也可「瀏覽…」或貼路徑＋「套用」切到 DIA 原廠母本 `D:\0_軟體開發區\6.模板與共用資源區\原廠母本\DIAChip8Side_Welding\Recipe`。實測列出四個子 Recipe，選 `3923-R002` 成功匯入本機 21 檔並載入 S1，母本 Setting/S1 SHA 保持不變；詳 `docs/RECIPE_SELECTABLE_ROOT_20261028.md`。不直接在原廠母本上儲存參數。

> **最新二值化真圖保存 Gate（2026-10-28）：** 新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-173228` 已從原版 Recipe 圖示載入隔離 Shunt Recipe、`Ctrl+O` 原生載圖、真實滑鼠點原版二值化、改 `txt_TrackerName`、`Ctrl+S` 呼叫原版 SaveAllRecipe，關閉後重啟新進程再次從原版 GUI 讀回新名稱 PASS。存檔前備份及正式來源 SHA 比對亦 PASS，詳 `docs/ORIGINAL_GUI_BINARY_SAVE_READBACK_20261028.md`。僅 S1 二值化 Gate，模型/Inspect/AI 仍未完成；下方舊文件所稱二值化不能存檔已由此新 Gate 部分解決。

> **最新修正（2026-10-28）：** 已撤回外層 Recipe 浮層與 AI 等按鈕右移，改直接將 23×23 Recipe 圖示加入原版 `grd_Main` 的站別 10 與複製參數空隙。新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-170328` 已驗 GUI Recipe 載入、原生二值化、定位開窗及站別 1→2 操作 PASS；其他工具、修改保存與 AI 先知未驗。詳 `docs/RECIPE_NATIVE_GRID_FIX_20261028.md`。下方舊的 48×48 浮層候選是歷史版本，請勿採用。

> **Recipe 入口位置更新（2026-10-28）：** 已移除頂部 Recipe 橫列，改在原版 `btn_ParamCopy` 左側增加 48×48 資料夾圖示，點擊後仍開原有 Recipe 彈窗。隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-163834`，建置與 GUI 點擊 PASS；詳 `docs/RECIPE_ICON_LEFT_PARAMCOPY_20261028.md`。工具參數保存回讀仍未完成。

> **2026-10-28 追加 Gate：** 新隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-162611` 已以真正的滑鼠點圖示中央成功開啟原版二值化與定位視窗，且不再以反射手動呼叫原廠 callback。只在 marker+環境變數實驗模式生效，**尚未驗影像、修改、保存、重啟回讀、其他工具與 AI 先知**。詳 `docs/ORIGINAL_NATIVE_CLICK_GATE_20261028.md`。下方「二值化未開啟」是修正前 154452 候選的歷史結果。

## 目前使用者目標

以獨立的 `InspToolsStandalone.exe` 顯示一期 decoded/rebuilt 原版 `InspTools.ToolsWindow`。只新增一個「選擇 Recipe」按鈕，按下後開選擇視窗；載入 Recipe 後，使用者要**直接點原版定位、二值化及其他工具圖示進行編輯和保存**，包含未來恢復 AI 先知。不要以右鍵選單或另外重做的工具視窗取代原版操作。

使用者曾考慮類似 ChaoMcpHub 的可摺疊側欄，但後來明確修正為「Recipe 視窗跳出來而已」。側欄 `docs/RECIPE_SIDEBAR_20261028.md` 是已撤回的歷史實驗，不是目前方向。

## 接手位置與規則

- 本輪開發工作在 `E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯-乾淨開發版`。原專案 `E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯` 留有較完整的歷史 output、訓練資料和模型；兩份是不同目錄，不會自動同步。
- 先讀本副本 `AGENTS.md`、`docs/ROUTE_RESET_20260915.md`、`docs/PHASE1_BASELINE_AUDIT_20260915.md`、本文件和 `docs/ORIGINAL_INPUT_EXPERIMENT_20261028.md`。一期 source 位於 ProjectHub.Native 的 `ProjectData/Projects/InspToolsRebuild`；視為唯讀證據。
- 專案 PowerShell 已由根目錄搬到 `scripts/`，功能與命令見 `scripts/README.md`。舊文件中的根目錄指令需要加 `scripts/`。
- 禁止動一期來源、原廠 production Recipe／Model／Runtime／Data；正式 `release/current` 不可用未驗收候選覆蓋。不要接回離線主線的 Generic Editor、SDK、Agent、Annotation 或 Host。

## 目前事實與未完成項

- 現行程式可顯示原版 `INSPECTION TOOLS`，主視窗新增單一 Recipe 按鈕，點後顯示 Recipe 選擇視窗；這不代表整個軟體完成。
- 一般候選在未載入 Recipe 時用未初始化的站別顯示 Shell，並封鎖原版互動。正常候選載入後，目前僅定位圖示經暫行攔截開啟原版定位視窗，其他原版圖示仍受限制；不能聲稱已恢復完整工具操作。
- 一期經驗證的載入順序是 `SetRecipe ×8 → SetStation → SetThroughputRecipe → ReadModel ×8 → SetStationIndex`（其他初始化參照一期 `ReplicaLoader.InitializeCompiledRecipeLifecycle()`）。本案已有部分移植，但正常 R3 路徑尚未完整接回。`SetThroughputRecipe` 可能將 `Setting.ini` 轉成 Unicode 並寫回，應在可寫隔離 Recipe 副本驗證，不能改 production 來源。
- 使用者要求的功能包含真實 GUI 點擊、工具設定、修改保存並重啟回讀；不接受僅有 Window 存在、只讀畫面、右鍵代用操作或 Build PASS 當作完成。

## 2026-10-28 實驗結果

在 `output/standalone-r3/InspTools-Portable-R3-20261028-154452` 的隔離 Recipe `3923-R002` 副本，使用 `INSPTOOLS_ORIGINAL_INPUT_EXPERIMENT=1`、固定 R3 DLL hash 和 `original-input-experiment.marker` 啟用原版輸入實驗。共用設定初始化後，八站 `paramFile.ThroughputFile` 非 null。用 GUI 點原版定位圖示，實際開啟原版 LocateWindow；同目錄 `original-input-experiment.log` 有 `clicked Btn_LocateTool_Click`。未載影像時視窗處於原廠預覽模式，**未測修改／保存／重讀**。

另一隔離實驗 `.../InspTools-Portable-R3-20261028-153053` 實測主畫面原版站別 1 → 2 切換。`154452` 候選中 GUI 點二值化圖示沒有開啟視窗，也沒有記錄 callback；其他工具未完成驗收。定位成功不代表全部按鈕成功。文字工具 `Btn_FontTool_Click` 在該 DLL 中反射找不到，需核對實際 DLL metadata 與事件綁定。

偵錯發現，對所有 UIElement 強行設 `IsHitTestVisible=true` 會改變原版裝飾層的命中設定；實驗改為只恢復啟動器封鎖過的 `ButtonBase`。目前 `Chao8SideRecipePicker.cs` 的實驗事件路由仍是研究代碼，只在 marker 和環境變數生效，**尚不可當正式解法或普遍放行所有按鈕**。實驗包內含 Recipe 副本，也未完成資料寫入及 AI 風險驗證。

## 下一步建議

1. 先核對現有工作樹，閱讀 `Chao8SideRecipePicker.cs`、`NativeR3RecipeSession.cs` 和一期 loader。保留目前未提交成果，不要 reset、clean 或刪除 output 模型。
2. 在新建的隔離候選查清二值化圖示的實際 WPF 命中元素與事件路由；校對 R3 DLL 的方法 metadata，避免猜名稱。確認是原版 callback 真正開窗，而不只檢查 UIA enabled 或呼叫替代 dialog。
3. 在隔離 Recipe 與實際影像副本上，按原廠流程完成工具參數修改 → 原版保存 → 關閉重啟 → 讀回，記錄相關 Recipe、PublicDB/PrivateDB 與 Throughput 寫入範圍。繼續逐項驗其他工具、原版站別和圖片載入。
4. 釐清同一工作階段 Recipe 切換時資源釋放，現行限制是載入一次後須重啟。
5. AI 先知最新狀態見 `docs/CHAO8SIDE_ORIGINAL_AI_PROPHET_BRIDGE_20261030.md`。已從成品 Chao8Side Runtime 複製自家 trainer、AIRuntime、GUI shim、訓練腳本與 backbone，並依 Chao8Side Host 的 marker-gated capability bridge 只在隔離候選的記憶體內接回原版控制項；原版按鈕 GUI 可見且 enabled。Recipe 仍 OFF、PModel 沒有 ONNX、尚未載入影像／定位結果，模型視窗和推論未驗。未改授權檔、正式 DLL、成品 Recipe 或 release；不要把這個隔離開發 bridge 當正式授權或部署完成。

## 工作樹與資料

- 交接時 branch `main`，HEAD `109efc9`；已有大量修改、刪除和未追蹤檔案，**均未提交**。其中根目錄腳本顯示 deleted、`scripts/` 顯示 untracked，是本輪移動後的 Git 狀態，不代表可以清掉。下一位先執行 `git status --short`。
- `output/`、`release/`、部分 DLL／模型受 `.gitignore` 排除；Git 不是 Runtime 或模型的完整備份。乾淨版的 `output/decoded-ui/Release/Runtime` 含訓練 Runtime；`runtime-extra/DeltaAI` 是獨立 GPU Runtime 快照；`portable/InspTools-R3` 是較舊候選，不是本輪已驗的正式成果。
- 每個候選目錄可能含用戶或測試 Recipe；不要覆蓋或刪除。候選已在本機打開多個實驗視窗，操作前確認實際執行路徑，不要結束使用者正在操作的程式。
- `Build.ps1` 在乾淨副本可用 `powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1 -OutputSubdir <新名稱>`。R3 打包用 `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/<新名稱>`。建置與打包 PASS 僅代表產出，不是互動驗收。
