# 原版定位圖示接回候選｜2026-10-28

本輪使用乾淨開發副本。Chao8SideRecipePicker 在 R3 portable 成功載入本地 Recipe、建立真實八站後，僅解鎖原版主畫面的定位圖示，攔截點擊並呼叫已存在的 R3IsolatedEditor 打開原版 LocateWindow；其他原版控制項仍封鎖。上方站別與保存入口目前仍保留。這不是原廠 ToolsWindow 原生定位回呼，原生回呼依賴 Throughput／影像生命週期，仍需接回及驗收。

驗證：scripts/Build.ps1 -OutputSubdir locate-original-check PASS；BuildStandaloneR3.ps1 -LauncherDirectory output/locate-original-check PASS。候選 output/standalone-r3/InspTools-Portable-R3-20261028-150132，以 portable/InspTools-R3/Recipe/3923-R002 複製到候選隔離資料夾；GUI 選取並載入、實際點擊原版定位圖示成功顯示原版定位視窗。未改參數、未驗存檔回讀，未驗其他工具、影像及 Inspect。候選中有 Recipe 副本；保留並避免當成空白包。

未完成：原版站別切換、其他工具按鈕與原生 callbacks、完整圖像/Throughput 初始化、Recipe 切換釋放、完整保存回讀與多站操作。不得聲稱完整獨立作業完成；不得直接解除所有原版控制項的保護。後續應依一期已驗證生命週期逐項完成。
