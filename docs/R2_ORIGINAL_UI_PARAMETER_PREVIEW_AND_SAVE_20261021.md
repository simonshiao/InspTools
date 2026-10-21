# R2｜原版 GUI 參數預覽與獨立 Recipe 保存

日期：2026-10-21
實體：`E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯`
開發產物：`output\decoded-ui\Release\InspToolsStandalone.exe`、`InspToolsRecipeCli.exe`
正式 `release\current`：**未覆蓋**

## 本次完成

### 1. 原版 GUI 的輕量參數模式

新增 `LightweightRecipePreview.cs`，接受原生 `ParamFile`、建立 8 個不呼叫建構函式的 `CVInspection` 型別佔位物件，填入 `CVInspection.paramFile` 後呼叫原版 `ToolsWindow.SetStation / SetStationIndex`。

這個模式沿用 **原版 ToolsWindow 和原版 Checkbox**，不是重新做一套 Tool 編輯器。

- 不呼叫重型 `CVInspection()` 建構函式。
- 不呼叫 `CVInspection.ReadModel()`、`Inspect()`。
- 原版控件在參數預覽狀態禁用直接點擊，避免觸發不存在的 Native Tool 執行個體。
- 右鍵選單提供「原版 GUI 預覽 Recipe（唯讀）」及「預覽切換 S1～S8」。
- 預覽模式的標題始終提示「參數預覽／不可檢測」。
- 完整檢測 Recipe 載入在右鍵選單顯示待 Runtime 驗收、**禁用**；仍保留 `--recipe` 原有工程入口但它不是驗收通過功能。
- 選站時比較原版 `chk_BinaryToolON.IsChecked` 與該站原生 `ParamFile.BinaryToolON`；不一致直接報錯，不能默默顯示錯誤狀態。

現場已驗：

```powershell
.\InspToolsStandalone.exe --recipe-preview R2PROBE-3923-R002 --station 1
.\InspToolsStandalone.exe --recipe-preview R2PROBE-3923-R002 --station 5
```

S1 / S5 均 `ALIVE=True`，標題包含正確的 `/ S1`／`/ S5`，關閉均正常，無 error file；S5 二值化 False、S1 True。

> 右鍵清單、站別切換與基本編輯視窗已編譯，**尚未用人工或 GUI 自動點擊逐項驗收**。

### 2. 真正的原生參數寫回

`NativeRecipeProbe.WriteEditableFlags` 使用原版 `InspTools.AOI.ParamFile.WriteAllParam()`，支持四項明確白名單：`LocateToolON`、`BinaryToolON`、`AIEnhanceToolON`、`AIProphetToolON`。

處理順序：
本機 Recipe 檢查 → 原版讀取 → 比對改動 → `Recipe\_BackupRecipe\<name>\stationN-timestamp.ini` 備份 → 原廠寫入 → 原廠重新讀取驗證；任何寫入／驗證失敗，回復原始 INI bytes。

新增 CLI：

```powershell
.\InspToolsRecipeCli.exe --recipe-set-flag R2PROBE-3923-R002 5 BinaryToolON on --commit
.\InspToolsRecipeCli.exe --recipe-params R2PROBE-3923-R002
.\InspToolsRecipeCli.exe --recipe-set-flag R2PROBE-3923-R002 5 BinaryToolON off --commit
```

S5 實測 True/False 重讀比對成功，寫入後恢復原值；exit 0；產生 2 份備份；Chao8Side 供體 `station5.ini` SHA256 前後一致。任意不存在的設定欄位被拒絕，exit 5。

原版視窗右鍵新增「編輯八站 Recipe 基本開關（原生存檔）」入口，能選 Recipe、S1～S8、四項開關，儲存前顯示確認視窗，並複用上述原生寫入／驗證／備份流程。**GUI 滑鼠操作仍待驗證**。

### 3. 啟停、Build Gate

- 零參數 GUI：`INSPECTION TOOLS`，關閉正常。
- S5 預覽 GUI：`INSPECTION TOOLS - [參數預覽／不可檢測] R2PROBE-3923-R002 / S5`，關閉正常。
- `InspToolsStandalone.error.txt` 未產生，均無殘留進程。
- `Build.ps1` 成功，`formal_projects=1`、`off_route_projects_built=0`。
- 不再將未知的 `--recipe-*` 命令全部攔截：`--recipe-preview` GUI 可以正常分流。
- 重建保留 Recipe、Model、PublicDB、TrainingImage，不覆蓋正式 `release\current`。

## 已確認的真正邊界

- 現有成品已能不啟動 DIA/Shunt 主程式完成 Recipe 匯入、八站原生讀取、局部原生寫回、隔離備份，並能在原版 ToolsWindow 預覽參數。
- 本輪未解決完整 `CVInspection()` native/AI 初始化；此前完整初始化曾 timeout，不可宣稱全功能獨立或 GUI 原生 Tool dialog 保存成功。
- 原版工具完整編輯、模型載入、離線圖片、`Inspect()`、AI 推論、三套訓練 Runtime 均待後續 R3～R5。
- 四個基本設定的原生寫入 **不等於** `ToolsWindow.SaveAllRecipe()` 已整合；寫入使用 `ParamFile.WriteAllParam()`。

## 下一個 Gate

1. 透過隔離測試驗證 GUI 右鍵基本編輯視窗的人工操作與 S1～S8 站別切換。
2. 補齊原版工具完整編輯所需的最小 native/runtime，優先以獨立進程／可超時的 probe 找到 `CVInspection()` 真正阻塞點。
3. 測試一份實際模型的離線圖片與 `Inspect()`；通過後才升版 `release\current`。
