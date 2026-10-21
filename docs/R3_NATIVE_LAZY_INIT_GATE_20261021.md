# R3｜找到一期原始碼並修復 AI 定位提前初始化

日期：2026-10-21
狀態：**R3 隔離建置、真實八站載入、原版 GUI 啟停 PASS；AI 模型／Inspect 尚未驗收。**

## 來源與版本控管

- 二期本地 Git：main，第一次基準提交 bc26e73。
- 一期 C# 原始碼找回於 D 槽舊 ProjectHub 封存：
  D:\0_軟體開發區\9.封存區\歷史專案\ChaoMCPHub_Python_Full_20260916\ProjectHUB\Projects\InspToolsRebuild\Source\InspTools-1.3.16.28-Editable
- Source LocateTool.cs SHA256：64D52BC4F04340C67D6CC74F239A6BC781AA47A57261E974913614F8FE623322
- R1 DLL SHA256：85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1
- **R3 隔離重建 DLL SHA256：ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B**
- 封存原始碼和原本 release/current 的 DLL 都維持只讀，沒有覆蓋。

## 修復內容

原版 LocateTool.SetBuffers(CVBuffer) 無條件呼叫 LocateAIEnhanceTool.CreatInst()，再進入原生 DeltaAIProcessDll.CreateInstance / SetGPU，導致沒有模型／RTX 的測試環境連 CVInspection 建構都卡住。

解決方式：只在產生的來源副本中移除 **SetBuffers 階段的 CreatInst 呼叫**。原版 LocateAIEnhanceTool.ReadModel() 已在未建立 native instance 時呼叫 CreatInst()，所以真正需要 AI Locate 時仍保留按需初始化路徑。

BuildNativeLazyInit.ps1：

1. 驗證原始碼和 R1 DLL 的 SHA256，失敗就停止。
2. 唯讀取用 ProjectHub 封存來源，產生 output/native-lazy/generated-source 測試副本，只對副本套用一行修改。
3. 從 SHA-pinned R1 DLL 抽取缺少的 InspTools.g.resources，保持原廠 WPF GUI 資源。
4. 使用 .NET Framework 4.7.2 / x64 編譯，輸出僅在 ignored output/native-lazy。
5. 編譯 **0 錯誤，157 警告**。警告與二進位版本差異尚需進一步稽核，不得直接升版正式 DLL。

## 隔離原生 Gate

TestNativeLazyInit.ps1 只建立 output/native-lazy/runtime 執行副本，裡面有新的 DLL、本案最新 Launcher、以及複製的 R2PROBE Recipe。

| 項目 | R3 結果 |
| --- | --- |
| LocateTool.SetBuffers | PASS，無原生卡死 |
| CVInspection() + S1 SetRecipe | PASS，約 0.57 秒 |
| 真實 CVInspection() × 8 + SetRecipe S1～S8 | **8/8 PASS，約 1.27 秒** |
| 原版 GUI + 真實八站 S5 | PASS，視窗正常開啟 |
| 原版 GUI 關閉 | Exit 0，無 error file |
| AI ReadModel / Inspect / OK-NG | **NOT_RUN** |

新 R3 GUI 入口（只能在 output/native-lazy/runtime 目錄）：

    .\InspToolsStandalone.exe --recipe-native-r3 R2PROBE-3923-R002 --station 5

NativeR3RecipeSession.cs 的安全約束：

- 強制確認 AppBase 尾段 output/native-lazy/runtime。
- 僅接受 R2PROBE- 開頭的隔離 Recipe。
- 僅允許已驗證 Hash ACD6... 的 R3 DLL。
- 不呼叫 ReadModel、Inspect 或 PLC/Camera；原版工具按鈕在該模式先停用；原生 Closing 自動儲存仍未啟用。
- 正式 R1 執行環境的 --recipe 保持 fail-closed（Exit 46）。

## 使用順序

在專案 Git 根目錄：

    powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
    powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildNativeLazyInit.ps1
    powershell -NoProfile -ExecutionPolicy Bypass -File .\TestNativeLazyInit.ps1

這些命令只更新開發產物。TestNativeLazyInit 會清空重建 **output/native-lazy/runtime** 專用測試區，切勿放入使用者 Recipe。

## 後續 Gate

1. 原版完整工具 Child Dialog 與 SaveAllRecipe() 的隔離寫回、重新讀取驗證（需要單獨備份／回滾）。
2. 各站 ReadModel() 分模型、分 Runtime 的限時驗證，避免再無條件載入 native AI。
3. 離線影像、真的 Inspect()、工具結果與 OK/NG 逐步實證。
4. 將 R3 依賴由本機封存進一步整理成可被授權部署的 build source manifest；Git 目前只有補丁與建置腳本，**未包含一期反編譯全部 source 或 proprietary DLL**。
