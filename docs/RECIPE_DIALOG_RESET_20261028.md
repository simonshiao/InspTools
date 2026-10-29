# Recipe 彈窗路線校正｜2026-10-28

依使用者要求取消側欄，回到原版 ToolsWindow 主畫面及單一「選擇 Recipe」按鈕；點擊開啟 Recipe 選擇視窗。不得再把側欄候選當作最新路線。

本次 Build.ps1 -OutputSubdir recipe-dialog-check 與 BuildStandaloneR3.ps1 PASS。候選 output/standalone-r3/InspTools-Portable-R3-20261028-152649（包含隔離 Recipe 3923-R002 副本），實際 GUI 確認按鈕能開啟 Recipe 彈窗；尚未以本候選重新測載入、存檔、切站。這次只恢復彈窗外觀與入口，不代表原版工具恢復。

核心未完成：原版多數圖示仍被啟動器限制；應沿用一期 ReplicaLoader.InitializeCompiledRecipeLifecycle，接回 Throughput、站別、影像、模型生命週期，再在隔離副本逐項驗原版回呼、保存和切站。SetThroughputRecipe 會將 Setting.ini 改寫為 Unicode，必須確保處理使用者可寫副本、釐清寫入及回復。既有定位圖示仍是攔截並打開原版 LocateWindow 的暫行接法，不是原生完整 callback。使用者要求原本介面直接操作，禁止把此候選當完成。
