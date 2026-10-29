# Recipe 圖示位置校正｜2026-10-28

## 本輪修改

依使用者要求，已移除頂部「選擇 Recipe」橫列，將入口移至原版「複製參數」左側，圖示採 48×48 資料夾向量圖（滑鼠提示為「選擇 Recipe」）。

修改檔案：`src/InspTools.Standalone/Chao8SideRecipePicker.cs`。

- 原版主 UI 與新按鈕放在同一個 Grid 的不同層。原版 Shell 尚未載入 Recipe 時仍會被限制互動，新按鈕不受此封鎖。
- 透過原版 `btn_ParamCopy` 動態定位入口；原版 `btn_ParamCopy`、`btn_AI_Function`、`btn_AddToOK`、`btn_AddToNG` 向右平移 58 px，避免與原版站別 10 擠在一起。
- 不更動原版按鈕的 Click 處理、Recipe 彈窗內容或原廠 DLL。正式 `release/current` 未更新。

## 建置與 GUI 驗證

- `scripts/Build.ps1 -OutputSubdir recipe-icon-left-paramcopy-v2-20261028`：PASS，formal_projects=1，off_route_projects_built=0。
- SHA256：`AFD4B237E1DF1F4B9C62ED23337C2CA6628D51E569F5AAAB7637952BC00E4AAD`。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/recipe-icon-left-paramcopy-v2-20261028`：PASS。
- R3 新候選：`output/standalone-r3/InspTools-Portable-R3-20261028-163834`。
- Windows UI Automation：Recipe 圖示座標 `(800,691)`，大小 `48×48`；複製參數 `(858,691)`，大小 `48×48`；AI、OK、NG 均維持可見，沒有疊在圖示上。
- Computer Use 滑鼠實際點擊 `(823,713)`，成功開啟 `Recipe 管理｜Chao8Side 選擇介面` 彈窗。

## 限制

第一次 V1 測試因 original 尚未離開舊父節點即加入 Grid 而啟動失敗；已修正先 `host.Content=null` 再重掛，僅以 V2 的成功實測為準。

本輪只驗證 UI 入口；不等於二值化參數保存回讀、AI 先知或完整 Inspect 通過。
