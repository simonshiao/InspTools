# R3 既有檢測 API 回歸：獨立候選版未開放的能力

日期：2026-10-21。此紀錄屬研發 Gate，不代表獨立 GUI 的完整 Inspect 已交付。

## 來源

ProjectHub.Native 第一期原始碼的 `InspTools.SecondaryDevelopment.InspToolsFacade`
已在 R3 patched DLL 中。測試只在
`output/native-lazy/runtime` 搭配 SHA-pinned DLL／R2PROBE Recipe 執行；
ShuntVC 與 ProjectHub.Native 都只讀。

`src/InspTools.Standalone/R3FacadeProbe.cs` 由獨立 Recipe CLI 啟用，
提供 `--r3-facade-load`、`--r3-facade-inspect`、
`--r3-facade-model` 限定診斷。使用父程序超時終止，避免 native 初始化卡死。
這些入口不是正式 GUI 的替代 SDK 或新 Host。

## 經驗證

- `LoadRecipe`：真實載入 Shunt 3923-R002 隔離副本並返回完整 8 站 Snapshot。
- `InspectFile`：在離線模式 `Offline=true, UseAi=false` 實際返回
  `InspectionResult` 和 7 項原版 `ToolResults`；不是依照檔名模擬的結果。
- 原本取用的 `TrainingImage/R002-CCD1` 尺寸為 1292×964，定位失敗；
  此 Recipe S1 的 `OKPath/NGPath` 實際指向
  `TrainingImage/VSMA8420AYM050JEW_A1/Origional/1`，影像為 4112×3008。
- 改用 Recipe 指定位置的 S1 原始 OK 與 NG 樣本後，
  **兩張 LocateSucceeded=true、Locate state=Ok、Locate errors=0**。
  兩張最終 Bin 都仍是 Retest，不能宣稱 OK/NG 已經正確判讀。
- S1 的 AI 前處理需要 `VSMA8420AYM050JEW_A1.h5d` 及
  `VSMA8420AYM050JEW_A1.pbd`。
  在 R3 隔離版分別只複製 .pbd、以及 h5d+pbd（每檔確認 SHA），
  `LoadConfiguredModel(1, Enhance)` 仍返回
  `Success=false, Loaded=false`，提及舊
  `Training/Train/train.exe` 和 `Training/Model/h5_to_pb.exe`。
  此失敗不能靠複製整份 ShuntVC 或宣稱模型就緒解決；
  下一輪需隔離追蹤原版 AIEnhanceTool.ReadModel/ConvertToPb 的實際依賴。

## 產品交付界線

獨立產品仍是 **InspToolsStandalone.exe 的原版 ToolsWindow**。
R3 portable 候選版只有經驗收的原版介面、真實八站／Recipe 存讀、
部分原版工具設定編輯；完整 AI 推論、有效 OK/NG、機台連線均尚未通過。

Shunt 影像／Recipe／模型只是工程測試供體，**沒有附在 portable ZIP**。
請勿在正式 Recipe 或運轉中的 ShuntVC 上執行以上實驗。
