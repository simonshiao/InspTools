# Recipe 側欄候選｜2026-10-28

Chao8SideRecipePicker 改為主視窗左側 Recipe 清單，可搜尋、選取、匯入本機副本、載入，側欄可收合及展開；原版 ToolsWindow 放在可捲動區域，未重畫主介面。載入使用既有 NativeR3RecipeSession 八站隔離流程。

驗證：scripts/Build.ps1 -OutputSubdir recipe-sidebar-check PASS；scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/recipe-sidebar-check PASS。候選 output/standalone-r3/InspTools-Portable-R3-20261028-151955：複製 Recipe 3923-R002 到候選本機，GUI 選取及載入成功，狀態顯示八站離線載入；GUI 收合、展開均成功；輸入不匹配文字時清單清空，清除搜尋後項目恢復。載入後畫面可見原版工具區；窗口不足時可橫向捲動。候選含測試 Recipe 副本。

限制：尚未 GUI 驗證匯入、存檔重讀；同一工作階段不支援切換 Recipe，須重啟。這不是完整一期 Recipe lifecycle：模型／Throughput 尚未完整接回，原版多數工具仍封鎖；AI 先知另待處理。不可宣稱完整獨立檢測已完成。正式 portable/InspTools-R3、原專案和 release/current 未覆蓋。
