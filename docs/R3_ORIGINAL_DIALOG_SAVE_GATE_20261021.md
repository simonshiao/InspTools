# R3 原版工具子視窗與 Recipe 真實保存 Gate

日期：2026-10-21

## 結論

在 `output\native-lazy\runtime` 的**隔離 R3** 環境中：

1. 真實八站 `CVInspection` 配合原版 `ToolsWindow`：PASS。
2. 原版 `LocateWindow`、`BinaryWindow` 使用真實 S1 Station 建構、預覽、顯示、關閉：**18 PASS／0 FAIL**。
3. 對 S5 真實 `CVInspection` 內的 `BinaryToolON` 變更，呼叫原版 `ToolsWindow.SaveAllRecipe()`，磁碟重讀與全新 `CVInspection.SetRecipe()` 的值一致：PASS。
4. **額外的完整 UI 編輯鏈 PASS：** 在原版 `LocateWindow.txt_FocusLimit` 輸入不同數值，關閉視窗由原廠 `Window_Closed` 將 UI 值寫回 S5 `ParamFile`，調用原廠 `SaveAllRecipe()`，重新建立 `CVInspection` 並載入 Recipe，`FocusLimit` 與編輯值相同。
5. 存檔驗證後在 `finally` 將隔離 Recipe 與 `PublicDB` 原有檔案以原始 bytes 恢復，並比對一致；**12 PASS／0 FAIL，Exit 0**。
6. 測試沒有啟動 ShuntVC、Chao8Side、相機、PLC、AI Model 或 `Inspect()`。正式 `release\current`、供體 Recipe 和一期封存 source 不變。

## 測試指令

在 Git 根目錄：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildNativeLazyInit.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\TestNativeLazyInit.ps1
```

`TestNativeLazyInit.ps1` 現已串接：

- 定位 SetBuffers probe
- 單站 CVInspection + SetRecipe
- 八站 CVInspection + SetRecipe
- 原版八站 GUI Checkbox 相容性 Gate
- 原版定位與二值化子視窗啟停 Gate：`r3-dialog-smoke-result.txt`
- 原版定位視窗實際數值編輯與 `SaveAllRecipe` 重新讀取／恢復 Gate：`r3-saveall-result.txt`

每一段在**獨立子程序**中測試並設逾時，結果都留在 `output\native-lazy\runtime`，大型執行副本不進 Git。

## 重要：二值化防呆沒有被繞過

第一次嘗試在 `BinaryWindow.txt_TrackerName` 修改名稱時，關閉視窗未寫回 S5 的 BinaryParam。進一步確認原廠 `BinaryWindow.WriteUIValueToParam()` 有 `DontSave1=True`，即沒有有效影像／檢測區域，原版刻意取消自動儲存。

因此**沒有強制清除 DontSave1，也沒有謊稱二值化參數可以無圖片任意保存**。目前只是證實 BinaryWindow 可開關；真正編輯須先加入合法影像、ROI，並驗證 Binary 的檢測約束。

## 已驗／未驗邊界

- 已驗：原版二個 Tool Dialog 啟停；定位的 `FocusLimit` 使用原版 UI 真實編輯；原版 `SaveAllRecipe`；磁碟重讀；新 CVInspection 載入；資料回復。
- 未驗：其他所有定位欄位、Binary 參數有 ROI 的修改，其他 16 個工具 Dialog、`ThroughputINI` 初始化後的擴充 DB 寫回、真 AI 模型與 `Inspect()`。
- **此輪只在顯式測試入口解鎖保存，常規 R3 GUI 仍限制原版工具操作，沒有開放到正式版。**
- `SaveAllRecipe` 除了八站還可能修改 Throughput、Public/Private DB 與快速配方；若日後初始化這些依賴，必須重新擴大交易快照驗證範圍，不能拿目前 PASS 直接當通用存檔保證。

## 接下來

1. 在 R3 GUI 加入受控、隔離 Recipe 專用的原版定位編輯入口與備份／提交／撤回。
2. 導入離線圖片、有效 ROI 後解除 BinaryWindow 的原生 `DontSave1` 前置條件（靠資料滿足原版驗證，不改 guard）。
3. 再跑其他原版工具的 dialog/parity/SaveAllRecipe。等 AI Runtime 準備完成才進 `ReadModel` 和 `Inspect()`。
