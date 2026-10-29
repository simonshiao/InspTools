# Recipe 根目錄可選｜2026-10-28

## 需求與行為

Recipe 選取視窗應以執行中的 `InspToolsStandalone.exe` 旁邊 `Recipe` 為預設根目錄，也允許使用者切換到任何合規的外部 `Recipe` 根目錄，例如 `D:\0_軟體開發區\6.模板與共用資源區\原廠母本\DIAChip8Side_Welding\Recipe`。

- 原版主畫面上的 Recipe 23×23 圖示位置不變，AI／複製參數／OK／NG 不位移。
- Recipe 彈窗中增加根目錄路徑輸入框（可貼上）、`瀏覽…`、`套用`、`預設`。每次開啟初始仍選軟體旁本機 `Recipe`，不強制寫死來源路徑、不永久修改選擇設定。
- 清單顯示選定根目錄下有效的子資料夾（需 `Setting.ini` 及 `station1.ini` 至 `station8.ini`），並保留搜尋。
- 直接選外部母本的 Recipe 並按「選取並載入」，會透過原有 `StandaloneRecipeStore.Import` 複製到軟體旁 `Recipe`，之後從本機隔離副本載入；不直接把來源丟給可能寫入參數的原版 SaveAllRecipe。
- 若本機已有同名 Recipe，不覆蓋；要求使用者另取本機新名稱後再匯入。若選到單一 Recipe 子資料夾而非根目錄，會提示改選上一層。
- 在本次安全隔離流程中，載入 Recipe 後要更換 Recipe，仍需重啟應用程式，不強制重建或清理既有 CVInspection。

## 原始碼與建置

- `src/InspTools.Standalone/Chao8SideRecipePicker.cs`：Recipe 根目錄 UI、資料夾瀏覽、外部來源匯入、已存在命名保護。
- `src/InspTools.Standalone/StandaloneRecipeStore.cs`：唯讀 `ListFromRoot()`，僅列有效且非 reparse point 的 Recipe 子目錄。
- `src/InspTools.Standalone/InspTools.Standalone.csproj`、`scripts/Build.ps1`：加入 .NET Framework 4.7.2 `System.Windows.Forms` 參考以使用 Windows 原生資料夾選擇對話框。
- `scripts/Build.ps1 -OutputSubdir recipe-root-picker-v2-20261028` PASS，standalone SHA256 `F32C282255F86A7C20B44C0297C2CF55259CC7E9AF9AF14F46F4C51AD67434EC`。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/recipe-root-picker-v2-20261028` PASS。
- 隔離候選 `output/standalone-r3/InspTools-Portable-R3-20261028-174832`，R3 原版復原 DLL SHA256 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`；正式 `release/current` 未更新。

## GUI 實測

1. 新候選開啟 Recipe 選擇視窗時，路徑預設為 `<目前 EXE 目錄>\Recipe`；按「預設」會回復同一路徑，PASS。
2. 將路徑輸入框填入 DIA 原廠母本 `...\DIAChip8Side_Welding\Recipe`，按「套用」，清單正確顯示 **4 個**有效 Recipe：`3923-0M50-1`、`3923-0M50-WK`、`3923-R002`、`3923-R002-WK`，PASS。
3. 選取外部 `3923-R002` → 「選取並載入」，GUI 成功匯入 21 份原始檔案至新候選的本機 `Recipe\3923-R002`，主畫面顯示 `3923-R002 / S1`，PASS。
4. 比對 DIA 母本與本機副本的 `Setting.ini` SHA256 均為 `13DA694E041DC7EAAB5662B2C45AE68EC30E87D434093C62FD899B5406FE9474`、`station1.ini` 均為 `153650676BC2E1901620F1077D8AA0A00C654699B9443CB366F3555A9D539E4C`，PASS。
5. 在新空白工作階段點「瀏覽…」實際出現 Windows `瀏覽資料夾` 系統視窗（PID=25920）；取消後可回到 Recipe 彈窗，PASS。
6. 最終保留 PID=25920 的 Recipe 選取視窗，已套用 DIA 母本且顯示 4 個子資料夾；之前的測試進程 PID=12412 已關閉。

## 範圍限制

本輪只驗根目錄切換、列舉、匯入與 S1 Recipe 載入；沒有證明 DIA 母本的 AI Model/Inspect 可執行，原版某些 AI 元件缺失時仍可能提示 `DeltaAIProcess` 載入錯誤。上一輪獨立版 Shunt S1 原版二值化真圖修改保存回讀 PASS 的 Gate 不因本輪 UI 改動而重新宣稱已逐項回歸。母本參數不寫入，也沒有大量複製影像與模型。
