# Chao8Side Recipe 選取介面回收｜R3 研究候選（2026-10-22）

## 起因與原版證據

使用者在實際開窗時發現：原版 InspTools 工具區按鍵不回應或造成程式停止回應，而且當初要求移植 Chao8Side Recipe 選擇介面，現有版本卻只在右鍵選單放了簡化功能。

已找到真正來源：

- D:\delta\Chao8Side\src\Chao8Side.Main\SystemSource\UI\RecipeWindow.xaml
- D:\delta\Chao8Side\src\Chao8Side.Main\SystemSource\UI\RecipeWindow.RecoveryBehavior.cs

Chao8Side 的 UI 有 Recipe List、搜尋、Current Recipe、Recipe Path、New／Save As、Barcode 配對及操作。原版依賴 MainWindow、MaterialDesign、FileSystem 與機台權限流程，**不能直接把該視窗及狀態類別載入到獨立檢測工具**，否則會連帶引入機台操作、全域路徑與寫入風險。

## 本次完成的第一階段移植

新增 src/InspTools.Standalone/Chao8SideRecipePicker.cs，移植原版 Recipe 清單搜尋、目前 Recipe、Chao8Side 唯讀來源清單、匯入到獨立 AppBase、另存複製、選取及 S1～S8 切換操作；啟動時直接開啟 Recipe 管理視窗。另加上方可見的 Recipe 入口、站別切換、R3 原版定位設定入口、儲存入口，不再只能用隱藏右鍵選單。

只在 SHA 固定的 R3 候選版進行真實八站初始化與原版定位編輯，使用既有 NativeR3RecipeSession / R3IsolatedEditor，不重寫 CVInspection。未經允許不寫入 Chao8Side 原始 Recipe；來源若選取，先匯入獨立副本。透過原版 StandaloneRecipeStore.Validate 檢查必要的 Setting.ini / station1~8.ini。

空白 Shell 的原版工具區先全部阻止滑鼠事件，防止對 FormatterServices.GetUninitializedObject() 未初始化的 CVInspection 執行原生回呼造成當機。原版面板不是已恢復互動功能；本次僅先開放**安全的 Recipe 選取／站別切換／R3 定位設定／備份驗證存檔入口**，其他工具需逐項接回真實物件後才可解除限制。

## 編譯與開窗驗證

- Build.ps1 -OutputSubdir recipe-ui-r2-20261022 -> PASS（net472-x64，89.97 MiB，原版 DLL SHA pin 未變）。
- BuildStandaloneR3.ps1 -LauncherDirectory output\recipe-ui-r2-20261022 -> PASS（產生全新候選，不覆蓋既有）。
- 最新候選：output\standalone-r3\InspTools-Portable-R3-20261022-153927\InspToolsStandalone.exe
- 原生 R3 InspTools.dll SHA256：ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B。
- @CHAO2 新版程序 PID 37700，實際存在 INSPECTION TOOLS 與 Recipe 管理｜Chao8Side 選擇介面兩個視窗，Responding=True。
- 尚未取得使用者對 Recipe 選擇、來源匯入、真實原版定位編輯及關閉重啟的端到端 GUI 驗收；**不可聲稱全部按鍵已恢復，也不可標示完整機台功能 PASS**。完整 AI/Inspect、Barcode 寫入／綁定、真正 1:1 RecipeWindow 還未移植。

## 安全與後續優先順序

1. 在此全新候選上用 Chao8Side 3923-R002 的獨立匯入副本驗證：清單搜尋、載入八站、切站、原版 LocateWindow、備份式儲存與重開回讀（絕不改動來源）。
2. 逐項確認 Binary／ROI／Mask 等原版工具的依賴、回呼及設定對話框，再逐步解除原版 GUI 的不可點擊限制。切勿一鍵解除所有 IsHitTestVisible 防護。
3. 補 Chao8Side 原版條碼搜尋及配對資料的**唯讀**顯示，實際條碼編寫／刪除需有隔離及還原 Gate，不能直接照搬機台 FileSystem。
4. 已載入的 Recipe 預設不在同一工作階段切換；必須先落實模型／buffer／worker 釋放與多次選配方的安全生命週期。
5. 不更新 release/current、不改原廠 Shunt/DIA/Chao8Side 程式或量產模型；原 Git 未提交項目全保留。
