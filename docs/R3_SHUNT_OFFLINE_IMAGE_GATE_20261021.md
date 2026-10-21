# R3｜Shunt 真實 CCD1 離線影像與原版二值化視窗

日期：2026-10-21
狀態：**影像載入／原版視窗顯示 PASS；二值化 TrackerName 實際編輯、原版儲存／重讀／恢復 PASS；Inspect、OK/NG 推論尚未驗。**

## 目標

讓「DIA 檢測執行環境」不依賴執行 ShuntVC、Chao8Side、相機與 PLC，也能使用離線 Recipe 和原廠二值化工具窗口顯示真實影像。

## 唯讀來源、隔離副本

供體：`D:\0_軟體開發區\1.成品區\Shunt外觀機\Runtime\ShuntVC`

`StageShuntFixtures.ps1` 從供體唯讀取得：

- Recipe `3923-R002` → 沙盒 `Recipe\R2PROBE-SHUNT-3923-R002`，21 個檔案。
- CCD1 OK：`OK-1-2025-06-27-16-54-46.jpg`，SHA256 `1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45`。
- CCD1 NG：`NG-1-2025-06-27-16-55-12.png`，SHA256 `E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4`。

目的地：`output\native-lazy\runtime\TrainingImage\R3-Shunt-CCD1`。

所有供體 SHA256 在複製前／複製後一致，沒有回寫來源 Recipe 或圖片。

## 實測步驟

在 Git 根目錄：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\TestNativeLazyInit.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\StageShuntFixtures.ps1
```

在 `output\native-lazy\runtime`：

```powershell
.\InspToolsStandalone.exe --r3-shunt-image-smoke R2PROBE-SHUNT-3923-R002
```

測試輸出：`r3-shunt-image-result.txt`。

獨立檔案存讀閉環驗收（只在沙盒）：

```powershell
.\InspToolsStandalone.exe --r3-shunt-binary-save-smoke R2PROBE-SHUNT-3923-R002
```

輸出：`r3-shunt-binary-save-result.txt`。

## 通過證據

- 8 個真實 `CVInspection` 綁定原版 `ToolsWindow`。
- 兩張實際 Shunt 圖片 SHA256 均驗證，均為 1292 × 964。
- 使用 `OpenCvSharp.Cv2.ImRead(path, ImreadModes.Color)`，直接交給原版 `CVBuffer.SetBuffer(Mat)`，再提供原版 `BinaryWindow.SetBuffer`。
- OK、NG 圖皆成功顯示於原版二值化工具視窗，成功關閉。
- **兩張圖的 `DontSave1=False`、`DontSave2=False`、`DontSave=False`**：具備有效影像尺寸後，原廠影像區域驗證不再攔截；沒有修改或強制跳過原廠的保存檢查。
- Recipe S1～S8 全部 INI SHA256 前後一致。
- 執行結束碼 `0`，無 `InspToolsStandalone.error.txt`。

## 原版 BinaryWindow 真實編輯／存檔 Gate

已在 OK、NG 離線影像完成原版 ImageBuffer／有效區域測試後，用 NG 圖做下面閉環：

1. 確認原版 `DontSave1`、`DontSave2`、`DontSave` 皆為 False，**不繞過原版防呆**。
2. 在原版 `BinaryWindow.txt_TrackerName` 填入 `R3-SHUNT-CCD1-VALID-ROI`。
3. 關閉視窗，確認原版 `Window_Closed` 已將新值寫回 S1 `ParamFile.BinaryParam[0].TrackerName`。
4. 呼叫原版 `ToolsWindow.SaveAllRecipe()`，用**全新的** `CVInspection.SetRecipe` 重新載入 S1。
5. 確認重新讀到新 Tracker 名稱。
6. 從預先保存的副本恢復隔離 Recipe／PublicDB，每一個檔案 bytes 回復一致。

以上全部 PASS，測試程序 `Exit 0`、無錯誤檔。測試來源 ShuntVC 沒有寫回。

這一條和先前 R3 原版 `LocateWindow.FocusLimit` 閉環相互獨立，證明**定位與二值化兩條 Tool 的基本編輯→保存→重讀**均有實測證據。

## 排除的問題

第一版離線樣本測試使用反射載入 `System.Drawing.Bitmap`，此獨立執行環境無法解析該組件。已改為使用**現成 OpenCvSharp.dll** 解碼，不再引入額外 System.Drawing 依賴。

## 尚未完成

- `BinaryWindow` 目前只驗了 `TrackerName` 一個原版欄位，其他 Threshold、ROI 座標及 Tracker 進階參數尚待逐項驗證；仍不能稱整個 BinaryTool 編輯功能完成。
- 未執行原版 `Inspect()`，也沒有實際計算檢測結果；這次的 OK／NG 只表示樣本原始資料夾標籤，不能當成獨立版分類結果。
- AI 模型載入、預分類訓練與推論仍需各自獨立驗收。
- 未覆蓋正式 `release/current`，也沒有修改 ShuntVC 成品。

下一步：用真實影像測試 `BinaryWindow` 的 Threshold／ROI 座標，並開始規劃原始 `Inspect()` 的離線輸入、輸出與 OK／NG 判定驗收。
