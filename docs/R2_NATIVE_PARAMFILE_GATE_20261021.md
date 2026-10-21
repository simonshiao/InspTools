# R2 原版 Recipe 參數讀寫｜2026-10-21

狀態：**原版 ParamFile 八站讀取 PASS；隔離 S1 寫入、重讀、恢復 PASS；完整 GUI Recipe/AI/Inspect 仍 PENDING。**

## 實測來源與隔離

- 唯讀供體：`D:\0_軟體開發區\1.成品區\Chao8Side\Recipe\3923-R002`
- 實際測試副本：`output\decoded-ui\Release\Recipe\R2PROBE-3923-R002`
- 編譯與測試僅在 `E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯`。
- 原生 API：`InspTools.AOI.ParamFile` + `InspTools.HiniFile`，`SetIniFile`、`ReadAllParam`、`WriteAllParam`。

## 完成與驗收

1. 程式依原版的 `Recipe/<name>/station1.ini`～`station8.ini` 路徑載入，直接調用 **原廠 `ParamFile.ReadAllParam()`**。S1～S8 成功，返回碼 0。
2. 真正的站別差異：S1～S4、S6～S8 的 `BinaryToolON=True`；S5 為 `False`。八站的 `LocateToolON=True`。S5 的 `AIEnhanceToolON=False`，其他七站為 `True`。
3. 在隔離副本中，S1 的 `BinaryToolON` 使用 **原廠 `WriteAllParam()`** 翻轉，重新建立 ParamFile 後讀到新值；用同一原生寫入器還原並重讀，顯示 `S1 WRITE_REREAD_RESTORE=PASS`，返回碼 0。
4. 驗證 Chao8Side 正式來源 `station1.ini` 的 SHA256 前後一致，`SOURCE_UNCHANGED=True`。
5. 新增 `InspToolsRecipeCli.exe --recipe-params R2PROBE-3923-R002`：不用 GUI 即可顯示八站定位、二值化、AI 前處理、預分類啟用狀態，返回碼 0。
6. 新增 GUI 原版 `INSPECTION TOOLS` 右鍵選單「查看八站原生 Recipe 參數（不載入 AI）」；與 CLI 共用相同唯讀讀取邏輯。GUI 本輪啟動標題正確、關閉 exit 0、沒有錯誤檔；右鍵選單人工點擊尚未逐項驗收。
7. `Build.ps1` 改成保留 `output\decoded-ui\Release\Recipe/Model/PublicDB/TrainingImage` 用戶資料，重新建置後現有 R2PROBE Recipe 仍在，且原生讀取正常。
8. 最新建置：`formal_projects=1`、`off_route_projects_built=0`；保留正式 `release/current` 不覆蓋。

## 技術發現

- `CVInspection()` 建構函式立即建立 Locate、SelectWay、Measure、Mask、Binary、AIEnhance、AIPredict、AIProphet 等工具；直接執行整體探測在無完整 Runtime 的情境中未於 29 秒限制內完成，不能稱為成功。
- `CVInspection.SetRecipe()` 除讀參數還會建立 Font/缺失檔案、清理重複 INI 區段；所以全原生載入試驗不得指向正式 Recipe。
- `ParamFile` 單獨呼叫即可驗證真實參數存讀，不必啟動 CVInspection / AI Model / 機台。
- `HiniFile(string, string commentDelimiter = ";")` 的 optional parameter 在 .NET reflection 需實際傳入兩個參數。

## 指令

在 `output\decoded-ui\Release`：

```powershell
.\InspToolsRecipeCli.exe --recipe-params R2PROBE-3923-R002
.\InspToolsRecipeCli.exe --recipe-native-probe R2PROBE-3923-R002
.\InspToolsRecipeCli.exe --recipe-native-probe R2PROBE-3923-R002 --roundtrip
```

`--roundtrip` **只接受 R2PROBE- 開頭的本機隔離副本**。不可對其他 Recipe 呼叫。這些命令不做 `ReadModel/Inspect()`。

## 尚未完成

- 在 GUI 正式工具編輯後，`ToolsWindow.SaveAllRecipe()` 的多站生命週期和異常處理尚未整合/回歸；目前獨立驗證的是底層 ParamFile 真實讀寫。
- 原版 `CVInspection` 完整實例化／`ToolsWindow.SetStation`、`SetStationIndex` 的已載 Recipe 視窗尚未通過整合 Gate。
- AI 模型 Runtime、離線影像、`Inspect()` 仍待獨立化；不可把本輪 ParamFile PASS 說成 AOI 全功能完成。
- 下一步優先處理 CVInspection 的完整 Runtime preflight 與隔離初始化，再測 GUI 原生工具編輯保存；仍不修改 ShuntVC 或 Chao8Side 的正式資料。
