> **2026-10-28 開發版更新：** 新建的 Recipe 側欄候選使用左側清單與收合按鈕，以下右鍵說明屬於歷史候選版本；狀態和限制見 docs/RECIPE_SIDEBAR_20261028.md。

# InspTools 獨立版 R3｜使用說明

狀態：**可操作的獨立候選版，不是完整檢測正式交付版。**

## 如何啟動

在這個資料夾直接開啟 **InspToolsStandalone.exe**。不需要先啟動 ShuntVC、Chao8Side、DIA、ProjectHub、Replica 或本地測試程式；編譯 DLL、原生依賴與原版 ToolsWindow 已放在同一資料夾。

## Recipe 管理

在原版 InspTools 視窗上按右鍵，操作以下選單：

- **匯入 Chao8Side／InspTools Recipe**：選原始 Recipe 內的 `Setting.ini`，輸入新的名稱；資料會複製到此軟體的 `Recipe\名稱`，原始來源不會改動。
- **新增／另存 Recipe（複製）**：獨立建立另一份完整 Recipe。
- **載入 Recipe（真實八站／離線設定）**：在這個程式內建立真實的八站 `CVInspection`，讀取原版 Recipe。
- **R3 原版定位工具編輯**：修改並關閉原版 `LocateWindow` 後，才可按「儲存 Recipe（備份＋驗證）」；有本機備份、重新讀取核對及撤回。
- **八站 Recipe 基本開關**：可存讀原版 `ParamFile` 的定位、二值化、AI 前處理、預分類開關；AI 開關不代表模型已載入。

重要：Recipe 可能包含舊電腦或舊機台的影像、模型絕對路徑。匯入只複製 Recipe 本身；真正的圖片／模型需另行整理到本機，不能宣稱已自動遷移。

## 功能邊界

已驗收：原版獨立 GUI、八站真實檢測物件、Recipe 匯入與存讀、定位參數編輯、隔離的二值化實測、Basic Locate；在工程環境能透過 Stable API 執行離線檢測並回傳結構化診斷。

**尚未驗收：AI 模型完整載入、所有原版工具設定視窗的保存、完整 Inspect 的有效 OK／NG 分類、PLC／相機的機台連線。** 因此完整檢測按鈕暫停使用；不要拿候選版取代運轉中的正式 AOI。

## 檔案安全與版本更新

此資料夾是獨立 AppBase，`Recipe`、`Model`、`PublicDB`、`TrainingImage` 均隨程式放在旁邊。不要將使用中的 Recipe 放回開發區的 `output/native-lazy/runtime`：工程測試會清空該區。

每次建置產生**新的候選資料夾**，不覆蓋舊候選版或已編輯的 Recipe。升級時可自行備份 Recipe，先在新候選版匯入副本驗證，確認相容後再使用。

如需以命令列直接開啟特定 Recipe，使用：

```powershell
.\InspToolsStandalone.exe --recipe-native-r3 "Recipe名稱" --station 1
```

這個版本不需要 Python、舊 ShuntVC 可執行檔或 ProjectHub 在目標電腦上運作；但需要支援 .NET Framework 4.7.2 的 Windows x64 系統。部分 AI/native 功能有未驗證的外部驅動與 Runtime 需求。
