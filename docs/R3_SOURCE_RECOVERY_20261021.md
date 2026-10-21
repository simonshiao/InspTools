# R3｜一期來源恢復、延遲初始化與原版八站 GUI

更新：2026-10-21

## 來源與 Git

- 已建立本機 Git main 分支。初始提交為 bc26e73，正式版 release/current 不納入版控。
- 一期可編輯來源已由歷史封存找回，位置：
  D:\0_軟體開發區\9.封存區\歷史專案\ChaoMCPHub_Python_Full_20260916\ProjectHUB\Projects\InspToolsRebuild\Source\InspTools-1.3.16.28-Editable
- 原始 LocateTool.cs SHA-256：64D52BC4F04340C67D6CC74F239A6BC781AA47A57261E974913614F8FE623322。
- 正式 R1 InspTools.dll SHA-256：85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1。

## 修正方式

由 BuildNativeLazyInit.ps1 在 output/native-lazy/generated-source 建立一期原始碼的隔離副本，只在該副本移除 LocateTool.SetBuffers 無條件執行的 locateAIEnhanceTool.CreatInst()；原有 LocateAIEnhanceTool.ReadModel 在需要時才執行 CreatInst 的邏輯仍保留。

由 hash-pinned R1 DLL 提取原版 WPF pack-resource container 到隔離建置目錄，再重建獨立測試 DLL。**沒有修改封存來源，也未更新正式 release/current。**

修正後 DLL SHA-256：ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B。

## 取得的驗收結果

執行 Build.ps1 後執行 TestNativeLazyInit.ps1。此腳本自動建立隔離 AppBase，透過帶逾時與強制清理的子程序測試：

1. LocateTool.SetBuffers()：PASS，約 1ms。
2. 單一 CVInspection()、S1 SetRecipe()：PASS。
3. 真實八個 CVInspection 及 S1～S8 SetRecipe()：8/8 PASS，約 1.2 秒。
4. 真實原版 ToolsWindow，使用八個實際 CVInspection 逐站呼叫 SetStationIndex，核對原版二值化 Checkbox：S1～S8 均 PASS，Exit 0；沒有 ReadModel 或 Inspect。

GUI 測試使用明確參數 --r3-native-gui-smoke R2PROBE-3923-R002，僅接受 output/native-lazy/runtime 下、擁有上述已驗 hash DLL 的測試環境。每次自動關閉並記錄 r3-native-gui-result.txt。

## 尚未通過、仍保持關閉的功能

- 一般正式版的 --recipe 完整載入仍被 NativeRuntimeGate 阻擋，exit 46；不能提前開放。
- 此輪沒有 ReadModel、GPU／CPU 模型推論、圖片載入、Inspect()、OK/NG 結果驗收。
- 也尚未在真正原版工具子視窗驗證修改與 WriteAllParam／SaveAllRecipe 的完整流程。
- 一期可編輯來源已找回，但應繼續比對 WPF 及各個演算法行為；實驗版不能直接替換正式或機台 DLL。

## 後續

先在隔離 AppBase 驗證無模型時的原版工具子視窗操作，再逐步導入輕量影像 Buffer 與離線 Inspect()。需要 AI Locate 時才執行原生初始化，並為模型初始化添加明確失敗與逾時機制，最後再進入正式升版驗收。
