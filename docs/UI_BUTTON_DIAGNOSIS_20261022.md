# 獨立 InspTools UI｜按鍵無效診斷（2026-10-22）

## 使用者現場反饋

視窗可開啟，但原版 GUI 上許多按鈕點擊沒有預期反應，故「開窗 PASS」不能代表獨立工作站已完成。

## 已核對的來源事實

- 起初於 @CHAO2 開啟 output/decoded-ui/Release/InspToolsStandalone.exe，視窗 INSPECTION TOOLS / PID 25472 啟動成功，但這是工程產物，不是 10/21 獨立 R3 已封包版本。
- src/InspTools.Standalone/DecodedToolsWindowHost.cs 的零參數入口直接呼叫 InitializeBlankStations()：八個 CVInspection 以 FormatterServices.GetUninitializedObject() 建立，不含真實工具、影像、Recipe、模型與 runtime。這只是一個可顯示的 Shell。
- LightweightRecipePreview.RestrictOriginalEditing() 在參數唯讀／R3 載入後，巡覽視覺樹將原版 Button/ToggleButton 的 IsHitTestVisible 設為 false，故那些控制項無法接收實際滑鼠點擊；這是原本避免缺 Native 物件發生例外的保護，不是完整使用者操作介面。
- 原有設計把 Recipe 匯入、真實八站載入與定位編輯藏在 ContextMenu（主窗右鍵），不是直接從可見按鈕導航；這也是操作性問題。
- R3 交接文件明確註明 AI 模型載入、完整 Inspect/OK/NG 未驗收，正式檢測按鈕故意不開放。
- 已另開真正的 R3 候選：output/standalone-r3/InspTools-Portable-R3-20261021-231504/InspToolsStandalone.exe，啟動後 Windows UIA 找到約 450 個元件，許多 Button 的 IsEnabled=true；然而此旗標只代表 UIA 的 enabled，**不代表有有效的 CVInspection 及 Click 執行流程**。此 R3 包內 Recipe 目錄目前空白。

## 開發判斷

這不是簡單補 Click handler 可解決，而是把「空白 Shell/唯讀 Preview」包裝成看似完整的原版工作站。不可粗暴移除 IsHitTestVisible 防護，否則 Native 子視窗在缺少實體 Station/ParamFile/Buffer 時可能當掉。

## 後續實作與驗收 Gate

1. 建立明確的可見狀態（未載 Recipe／離線可編輯／AI 未載入／Inspect 可用），不要無提示地顯示大量看似可按的控制。
2. 將既有 R3 Recipe 選擇與載入入口搬到明顯的操作列，初始化真實八站 CVInspection、ParamFile、Throughput、當前 S1～S8 與影像 Buffer；沿用 ProjectHub 的 DIA/Shunt 已驗證 lifecycle，避免重建架構。
3. 對每個原版工具按鈕建立「對應原廠 dialog / 初始化依賴 / 是否允許離線編輯 / 保存及回讀」矩陣；已驗證的定位、二值化工具優先恢復，未驗收的 AI/完整 Inspect 明確禁用並顯示原因。
4. 在**隔離 Recipe 與影像副本**實測：主視窗選 S1、點工具能開真實設定視窗、改一個參數→存檔→關閉重啟→回讀，操作全程不得改原廠／正式模型。再用真圖驗證基礎 Inspect，最後才處理 AI。
5. 驗收至少包含手動或 GUI Automation 的真實按鍵操作，不再使用視窗存在、Build PASS 或可執行檔 SHA 當作 UI 完成證據。

本診斷未修改 GUI 原始碼、原廠 Recipe、模型或 runtime；R3 仍只是功能受限的候選。
