# Chao8Side AI 定位／AI 先知對齊｜2026-10-28

## 對齊基準

- 只比對 `D:\delta\Chao8Side` 的原版 `InspTools.dll` 1.3.16.28（SHA256 `85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1`），不與 DIAChip8Side 1.3.16.56 混用。
- 以 `D:\delta\Chao8Side\Recipe\3923-R002` 為隔離測試 Recipe；來源唯讀，不連相機／PLC、不訓練、不呼叫 Inspect。
- 本案核心 `InspTools.dll` 繼續釘選 R3 rebuilt SHA256 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`，不能偷偷更換其他版本。

## 查明的真正缺口

1. 現有 Standalone `NativeR3RecipeSession.Open()` 只呼叫 `SetRecipe`、`SetStation`、`SetThroughputRecipe`、`SetStationIndex`，原廠 `ToolsWindow.SetRecipe()` 額外執行 `ReadToolONStatus`、`CheckAuthNoDLG()`、各站 `ReadModel`，所以先前 AI 定位/先知未初始化。
2. R3 携帶版原本缺少 Chao8Side 的 AI 定位模型（`*.lh5d`／`*.ltrt`）及執行相依 `cpsrt.dll`、`ThoughtWorks.QRCode.dll`、`onnxruntime_providers_shared.dll` 和 `PModel\train\dist\AIProphet\AIProphet.exe`。
3. Chao8Side `3923-R002`：S1 `EnableAILocate=True`、`AILocateModelName=R002-CCD7DW`；S1 至 S4、S7、S8 的定位模型已在 donor Model 可找到，S6 沒有啟用。
4. 原版 AI 先知各站 `AIProphetToolON=False`，`ModelName` 未設定。遍歷 Chao8Side 的 PModel、Runtime/AIProphet、Training 後找不到任何 `*.onnx`，因此**目前不存在可推論的 AI 先知模型**。PModel 的 `backbone.pth` 是訓練權重，不可直接當原版 ONNX。
5. 原版 AI 定位受 `ParamFile.AllowEnableAILocate` 控制，原版 AI 先知受 `AllowEnableAIProphet` 控制；必須由原版 `CheckAuthNoDLG` 設定，不得偽造授權旗標。

## 本輪代碼改動

- `scripts/StageChao8SideAiParity.ps1`：新候選專用，確認原版及 R3 DLL 雜湊、限定輸出至 `output\standalone-r3`，根據 Recipe 的站別欄位選擇並逐檔 SHA256 驗證模型及必要 native/managed DLL。建立 `chao8side-ai-assets.json` 與 opt-in `chao8side-ai-parity.marker`。不覆蓋與既有檔案雜湊不同者，原廠檔案保持唯讀。
- `src/InspTools.Standalone/NativeR3RecipeSession.cs`：只有釘選 R3 攜帶版且有 marker 才會在八站參數與 Throughput 初始化後執行原版授權檢查。授權通過時才嘗試原版 `locateTool.ReLoadAIModel()` 或 `aiProphetTools.ReadModel()`，透過原版 `ModelLoaded`／`ModelReaded` 判定，不強制更改授權、Locate 成功旗標或 Inspect 結果。將結果寫入 `chao8side-ai-readiness.log`，視窗標題也標示狀態。
- `src/InspTools.Standalone/StandaloneEntry.cs`：加上候選啟動防護，避免原版授權元件提早觸發還未初始化的 `ToolsWindow.CheckAuth`。
- 特別注意：`ThoughtWorks.QRCode.dll` **不可直接放於候選根目錄開機**。實測最初 `185028` 候選因原版 `ToolsWindow.CheckAuth` 在 `Station[]` 未建立前讀取而發生 `NullReferenceException`／APPCRASH。已改將它放於 `AiDeferred`，完成 Station 初始化才暫時複製到候選根目錄；下次啟動前只刪除由本流程建立、與 `AiDeferred` SHA 相符的候選根檔。**185028 舊候選不可採用**，不可刪除此隔離修正。

## 隔離驗證／觀察

- `scripts/Build.ps1 -OutputSubdir chao8side-ai-deferred-20261028` PASS，Launcher SHA256 `919779761502E97F75EBAF74EA58316747F56A65F45AC8B01DF2770A12D04DC2`，formal_projects=1、off_route_projects_built=0。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/chao8side-ai-deferred-20261028` PASS，production_untouched=true。
- **最新隔離候選** `output/standalone-r3/InspTools-Portable-R3-20261028-185252`。以 RecipeCLI 匯入 `3923-R002`；StageChao8SideAiParity 實際復原 24 個 AI 對應模型 + 4 個相依檔案，檔案雜湊驗證通過。這是 assets staged，不是模型推論通過。
- GUI 首次啟動 PID=8828 → 原版 Recipe 彈窗 → 選 `3923-R002` 載入，程式仍活著，診斷檔成功產生，沒有再因 QR DLL 提前載入而當機。
- `chao8side-ai-readiness.log` 真實結果：S1 AI Locate Recipe=True，model=`R002-CCD7DW`，`vendor_authorized=False` → **原版授權未開放**。AI Prophet Recipe=False，model 空值，`vendor_authorized=False` → **Recipe 未啟用**。
- 關閉 PID=8828 → 候選根目錄暫存 QR DLL 曾存在 → 新啟動 PID=38516 時啟動防護安全清除暫存檔 → WPF 開窗 PASS。第二次再次 GUI 選取 Recipe、執行原版授權檢查、診斷結果一致，未當機，Restart PASS。
- **未進行 AI 推論**：原版授權沒有放行，這是應遵守的結果，不可透過改 `AllowEnableAILocate` / `AllowEnableAIProphet` 欺騙功能 Gate。AI 先知尚無 ONNX 也不可能完成真正預分類。

## 下一個 Gate

1. 在具合法原版 AI 權限的環境中確認 `CheckAuthNoDLG` 可讓 S1 `AllowEnableAILocate=True`；如仍是 False，稽核原版授權與工具環境／授權版本，不要改私有欄位繞過。
2. 才以隔離真實 CCD1 影像運行原版 `ReadModel → AI PreLocate → DoLocate`，比較定位框／信心度，保存成功/失敗及耗時；不可只檢查 `ModelLoaded` 就稱為定位成功。
3. AI 先知先指定正確 Recipe 的 `AIProphetParam.ModelName`、提供合法的 `PModel\<模型>.onnx` 與對應 OK/NG 資料，驗證原版 `ReadModel`、熱力圖與 NG 分數。禁止用不存在或未訓練的模型假 PASS。
4. 仍需確認標準 GUI 原版視窗可實際編輯保存，以及全部八站、跨機、Inspect。正式 `release/current` 不變。
