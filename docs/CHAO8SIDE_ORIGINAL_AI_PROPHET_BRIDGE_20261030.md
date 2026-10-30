# Chao8Side 原版 AI 先知接入隔離候選｜2026-10-30

## 結論

Chao8Side 使用原版 `InspTools.AOI.AIProphetToolWindow` 與自家 self-contained trainer。控制項被隱藏的缺口不是少了重寫的 AI 頁面，而是：

1. 編譯版 GUI 固定檢查 `PModel\train\dist\AIProphet\AIProphet.exe`。
2. 此路徑是輕量相容入口；實際鏈路為 `ChaoAIRuntime.exe prophet` → `Runtime\AIProphet\Trainer\AIProphet.exe`。
3. 原版 `ToolsWindow.CheckAuth` 會依 AI 權限決定 `ParamFile.AllowEnableAIProphet` 和 UI 顯示；未授權時另有 `btn_UnLockAuth` 覆蓋先知按鈕。
4. Chao8Side 的 Template Host 只在完整自家 runtime 存在時，以 runtime bridge 將八站 `AllowEnableAIProphet` 設為 true、隱藏解鎖覆蓋層，再呼叫原版 `SetAIProphetON(true)`。Recipe 的 `AIProphetToolON` 保持原值。

本候選採用同一種隔離 Host 接法。它不修改 `AuthWindow`、授權檔、`InspTools.dll` 或成品資料；只會在候選程序記憶體內改 capability 欄位。接線需同時具備 portable marker、`chao8side-ai-prophet-ui.marker` 與完整 runtime。

## ProjectHub 線索

- `ProjectData\SharedKnowledge\AITraining\DIA_AI_GUI_RUNTIME_REUSE_PLAYBOOK_20260914.md`：定義跨產品接回 AI Prophet 原版 GUI 的 capability gate：`AllowEnableAIProphet=True` 並呼叫 `ToolsWindow.SetAIProphetON(true)`，同時保留 Recipe `AIProphetToolON` 原值。
- `ProjectData\Projects\Shunt-VisionTools\01_最新狀態\HANDOFF.md`：記錄相同 bridge 實測八站 `AllowEnableAIProphet changed=8`，且 `AIProphetToolON` 不被強制修改。
- `ProjectData\Projects\DIAChip8Side\Output\Handoff\AIProphet-Rebuild-20260912.md`：記錄原版工具窗、self-contained trainer、backbone、固定 launcher 與 ONNX/TensorRT 資產契約。
- `E:\0_軟體開發區\1.成品區\Chao8Side\src\Chao8Side.Host\OriginalAssemblyHost.AIProphetBridge.cs`：Chao8Side 實際啟用條件與 overlay 處理來源。

## Chao8Side Runtime 資產

唯讀來源：`E:\0_軟體開發區\1.成品區\Chao8Side`。

已依來源 manifest 將以下資產原樣複製到隔離候選：

- `Runtime\AIProphet`（包含自包含 PyTorch / CUDA 12 / cuDNN 9 trainer）
- `Runtime\AIRuntime`（`ChaoAIRuntime.exe` 與 runtime manifest）
- `PModel\train\dist\AIProphet\AIProphet.exe` 相容入口
- `PModel\train\AIProphetRebuild` 訓練程式
- `PModel\train\weights\backbone.pth`

來源與候選 SHA、檔案數、位元組數記錄於候選的 `chao8side-ai-prophet-runtime.json`。正式來源保持唯讀。

## 實作位置

- `src\InspTools.Standalone\Chao8SideAiProphetUiBridge.cs`：只在 marker 與完整 runtime 都存在時，設定八站記憶體 capability、隱藏解鎖遮罩、呼叫原版顯示方法。
- `src\InspTools.Standalone\Chao8SideRecipePicker.cs`：在原版 Recipe 完成載入後呼叫 bridge；不改 Recipe 欄位。
- `scripts\StageChao8SideAiProphetRuntime.ps1`：只允許往新的 `output\standalone-r3` 隔離候選分層複製，來源版本與核心 DLL SHA 不符時拒絕。

## 驗證

- 最終 Launcher Build PASS；輸出 SHA256：`0FC40AD4746E191A099189D6B8A17EEB4870C28BD14768C052B1735C129C361A`。
- 候選核心 R3 DLL SHA256 維持 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。
- 候選 `ChaoAIRuntime.exe prophet --status` 回報 `PROPHET_STATUS=READY`、`TORCH_CUDA=true`、`CUDA12=true`、`CUDNN9=true`。
- 原版 GUI 載入隔離 Recipe `3923-R002` 後，bridge log 記錄八站 capability 由 false 改成 true；UI Automation 確認原版 `btn_AIProphet` 可見且 enabled。
- 未執行訓練、Inspect 或模型推論；沒有保存 Recipe。

## 未完成條件

- 成品 Chao8Side 的 `PModel` 沒有 `.onnx` 模型，且 `3923-R002` 的 `AIProphetToolON=False`。所以先知控制項可見，但尚不能做原版模型載入或推論。
- 原版 `btn_AIProphet_Click` 會先走 `CheckBufferAndRef()`；本候選尚未載入有效影像與定位結果，因此未把「按鈕可見」宣稱為完整 AIProphetToolWindow／推論通過。
- 不以未收斂研究模型冒充量產模型；後續需要選定隔離 Recipe、合法測試影像與可用模型後，再做原版 `ReadModel`、Score、HeatMap 驗證。

## 候選

`output\standalone-r3\InspTools-Portable-R3-20261030-AIProphet-OriginalRuntime`

狀態仍為隔離研究候選，不是正式部署包。
