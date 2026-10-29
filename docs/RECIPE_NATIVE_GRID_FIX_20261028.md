# Recipe 原版 Grid 操作恢復｜2026-10-28

## 需求及問題

使用者指出新版 Recipe 圖示採浮層、AI 圖示被右移、原版 WPF 工具按鈕無法操作。先前版本 `20261028-163834` 不可當成已驗收 UI。

## 本輪修改

- 只在 Standalone 程式碼 `src/InspTools.Standalone/Chao8SideRecipePicker.cs` 中，將 23×23 的 Recipe 資料夾按鈕加入原版 `grd_Main`，位置由原版 `btn_Stage10`、`btn_ParamCopy` 的 Margin 算出，位於兩者空隙。
- 取消外層 Grid Overlay、取消把複製參數／AI／OK／NG 右移的 RenderTransform。
- 不修改一期原始碼及 recovered `InspTools.dll`；這是 Standalone 在執行時擴充**原版 WPF Grid**，不是改寫原廠 BAML/XAML 的 binary。
- 初始仍會禁用未載 Recipe 的原版 Button/ToggleButton；Recipe 按鈕在此禁用動作之後加入，以確保可點。
- Recipe 載入成功後，恢復原版 ButtonBase 的命中狀態、讓裝飾圖示穿透至原版自有 Button Click；移除 launcher 對定位工具的反射/攔截替代操作。
- `NativeR3RecipeSession.Open` 在通過隔離路徑及固定 DLL hash 檢查之後，為可寫隔離 Recipe 副本呼叫 `SetThroughputRecipe` 並驗證八站 ThroughputFile。模型載入/Inspect 仍未開放。
- 曾新增根 Grid 的 PreviewMouseDown 防護，因可能干擾原版事件已移除；最後的 V2 候選沒有這層攔截。

## 驗證紀錄

- `scripts/Build.ps1 -OutputSubdir recipe-native-grid-v2-20261028` PASS；1 個正式專案，0 off-route。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/recipe-native-grid-v2-20261028` PASS。
- 最新候選 `output/standalone-r3/InspTools-Portable-R3-20261028-170328`，固定原生 DLL SHA256 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。
- Launcher SHA256 `21134DB112DF01B01BF09932FFB4C6E3DF97A4CF9F6285445AE59BBFE03EFC8F`。
- 使用候選自己的 `InspToolsRecipeCli.exe --recipe-import` 匯入舊實驗 Recipe `3923-R002` 到新候選，不改原廠正式 Recipe。
- Windows UI Automation：空白時 Recipe 按鈕 `(775,704)`；複製參數 `(800,691)`；AI `(865,691)`。AI 和複製參數與原版未改位置。
- 真實 GUI 點 Recipe 圖示開啟彈窗 → 選取 `3923-R002` → 載入成功。載入後原版 WPF UIA 控制樹有 454 個 descendants。
- Computer Use 真實滑鼠點擊二值化圖示中央 → 原版「二值化」視窗開啟；定位圖示中央 → 原版「定位」視窗開啟；主介面點站別 2 → 畫面站別從 1 變 2，流程圖狀態變更。以上 PASS。
- 已知顯示差異：原版主畫面與流程圖切到 S2，但 Standalone 額外設定的視窗 Title 仍顯示載入當下的 `/ S1`；站別標題同步尚待改善，不影響本次原版按鈕路由 Gate。

## 尚未驗收

- 本次沒有測真實圖片、修改工具參數、原版保存、重啟回讀，也沒有驗證所有工具、AI 先知、AI 模型、Inspect 及跨機。
- `SetThroughputRecipe` 可能改寫候選 Recipe 的 Setting.ini；必須記錄實際寫入範圍，不可套用到 production Recipe。
- 正式 `release/current` 不變；本輪保持工作樹未提交變更，不能把隔離候選當正式量產版。
