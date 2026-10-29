# Recipe 圖示大小回歸原版工具列｜2026-10-28

## 需求

使用者提供八站工具列截圖，要求原本偏小的 Recipe 資料夾圖示與站別 1～8、機器人及複製參數按鈕同一大小、同一排且不得推動其他原版按鈕。

## 修改

- 修改 `src/InspTools.Standalone/Chao8SideRecipePicker.cs` 的原版 `grd_Main` Recipe 按鈕（非外掛 Grid、不是一條新工具列）。
- `btn_StandaloneRecipe` 改成 48×48，置中的向量資料夾圖示 30×30，白底、灰框，保留 tooltip「選擇 Recipe」及原有 Click → Recipe 彈窗行為。
- 原版八站介面 Stage9 和 Stage10 在原廠 `ToolsWindow.cs` 均設為 `Visibility.Hidden`，因此使用第 10 站保留區域放置 48×48 Recipe 按鈕，左上座標 `X=copy.Margin.Left-48-10`、`Y=copy.Margin.Top`。
- 保留原版 Stage1～8、`btn_ParamCopy`、AI 機器人原始位置與事件。執行時如果 Stage9/Stage10 意外變成可見，拒絕安置新按鈕以免覆蓋。
- `Recipe` 路徑選擇、匯入本機副本、參數保存等既有流程未修改。

## 建置、隔離 GUI 驗證

- `scripts/Build.ps1 -OutputSubdir recipe-48px-original-row-20261028`：PASS，formal_projects=1，off_route_projects_built=0。
- Standalone SHA256：`54B0142B079953753B0FF619B15D3859FAB7A0D3D1CCA5804CE6DECC0E6F8335`。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/recipe-48px-original-row-20261028`：PASS，production_untouched=true。
- 新隔離候選：`output/standalone-r3/InspTools-Portable-R3-20261028-182547`，原版 R3 DLL SHA256 不變（`ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`）。
- Windows UI Automation 實測：
  - Recipe `btn_StandaloneRecipe`：`(742,691) 48×48`
  - 複製參數 `btn_ParamCopy`：`(800,691) 48×48`
  - AI 機器人 `btn_AI_Function`：`(865,691) 48×48`
  - Recipe 與複製參數實際間距 10 px；原版按鈕沒有位移。
- 實際 Windows 畫面截圖核對圖示大小與底部工具列對齊。以 Win32 頂層視窗保持可見後，在 Recipe 中央 `(766,715)` 發送原生滑鼠點擊，成功開啟 `Recipe 管理｜Chao8Side 選擇介面`（非僅反射呼叫事件或 UI Automation Invoke）。PASS。
- 本輪屬按鈕尺寸／版面 Gate，不代表新候選已重新驗完整 Recipe 參數保存、AI／Inspect；之前功能 Gate 依各自紀錄保留。

## 重要提醒

先前 `20261028-163834` 是 48×48 *外層浮層* 且把 AI 往右移的歷史失敗版本，勿混淆。**本次 48×48 是直接放進原版 Grid，其他原版按鈕未挪動。**
