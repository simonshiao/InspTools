# 原版工具圖示中央原生點擊 Gate｜2026-10-28

## 結論

乾淨開發版新增「保留原版 ButtonBase 命中狀態、讓無原生 Click 回呼的 BitmapSave_Copy* 裝飾層穿透」修正。
在全新隔離 R3 候選載入隔離 Recipe 副本後，以 Computer Use 真正發送滑鼠點擊，實測圖示中央：

- 二值化：原版「二值化」視窗出現，原生 btn_BinaryTool 的輸入日誌為 `hit native Btn_BinaryTool_Click`。
- 定位：原版「定位」視窗出現，原生 btn_LocateTool 的輸入日誌為 `hit native Btn_LocateTool_Click`。
- 新版移除實驗用 `handler.Invoke(...)` 強制呼叫，沒有新建替代工具視窗；原版 DLL 自有的 WPF Click 綁定仍生效。
- 上述日誌只代表原生按鈕收到滑鼠輸入；配合原版工具視窗出現，才構成「原生工具點擊」Gate。**未驗存檔**。

## 修改位置

`src/InspTools.Standalone/Chao8SideRecipePicker.cs`

1. 啟動器限制原版輸入前，保存 ButtonBase 的 IsHitTestVisible 狀態；實驗載入後只還原原始狀態，不再一律設 true。
2. 原版工具圖示中央的 `btn_BitmapSave_Copy*` 裝飾按鈕僅於實驗模式改成 `IsHitTestVisible=false`，讓點擊到達原版按鈕；不再反射呼叫原廠 callback。
3. 一期 `ToolsWindow.cs` 確認文字工具方法為 `Btn_FontTool_Click`，已修正大小寫；文字工具尚未做 GUI 驗收。

## 本輪可重現輸出與實測

- `scripts/Build.ps1 -OutputSubdir original-native-click-20261028`：PASS；1 個正式專案、0 個 off-route project。
- Launcher SHA256：`F099C51855708C892B0327BC41B7836B85A54A7BD7A4901D33D7E248730A397B`。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/original-native-click-20261028`：PASS。
- R3 候選：`output/standalone-r3/InspTools-Portable-R3-20261028-162611`；DLL SHA256：`ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。
- 須同時有 `INSPTOOLS_ORIGINAL_INPUT_EXPERIMENT=1`、`original-input-experiment.marker` 與固定 DLL 才啟用；非一般預設操作。
- 透過 GUI 將舊隔離候選 `20261028-154452/Recipe/3923-R002` 匯入**新候選自己的 Recipe 目錄**，載入後主標題顯示 `3923-R002 / S1`。
- 用滑鼠點擊二值化與定位圖示中央，兩個原版視窗均實際出現。日誌：同候選目錄下 `original-input-experiment.log`。

## 未完成與禁止宣稱

- 未載真實圖片；未修改參數、點原版保存或重啟回讀。不能宣告工具編輯保存完成。
- 未驗全部原版工具、站別與圖片切換的本次回歸、AI 先知、權限、Model/Inspect、跨機。
- 實驗放行仍在 marker/env Gate，尚未成為正式路線；正式 release/current、一期來源、原廠 Recipe/Model/Runtime/Data 未改。
- `SetThroughputRecipe` 可能改寫隔離 Recipe Setting.ini；下一輪需記錄寫入前後差異。

## 下一 Gate

在新隔離候選搭配真實影像副本，優先針對 S1 二值化完成「載入影像 → 原版 GUI 改參數 → 原版保存 → 關閉程式 → 重啟回讀」並記錄 Recipe/PublicDB/PrivateDB/Throughput 寫入及前後 SHA。之後才逐項放行其他按鈕。
