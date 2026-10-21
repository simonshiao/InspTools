# R2 原版 GUI 實際整合測試與 AI 定位啟動阻塞根因

日期：2026-10-21
主線：DIA 檢測執行環境／`InspToolsStandalone.exe`
範圍：CHAO2 隔離開發區，**不覆蓋正式 `release/current`，不修改 ShuntVC／Chao8Side 原資料**。

## A. GUI 實際 WPF 行為驗收

建立 `src/InspTools.Standalone/StandaloneUiSmoke.cs`，僅在明確傳入 `--ui-smoke` 時執行；只能指定 `R2PROBE-` 開頭的隔離 Recipe。自動控制本機真實原版 WPF 視窗與右鍵選單，包括 **RaiseEvent 與真正進入編輯 Modal Dialog**，不是單純編譯檢查。

執行：

```powershell
.\InspToolsStandalone.exe --ui-smoke --recipe-preview R2PROBE-3923-R002 --station 5
```

產物：`output/decoded-ui/Release/ui-smoke-result.txt`

結果：

- `ExitCode=0`；**22 PASS／0 FAIL**。
- 原版視窗已綁定 Recipe 清單、原生參數檢視及編輯選單。
- `LightweightRecipePreview` 的 S1～S8 逐站 `SetStationIndex` 皆正常，原版 Checkbox 二值化狀態逐站與原生 `ParamFile` 一致。
- 真正透過 WPF `MenuItem.ClickEvent` 開啟 Modal 基本參數編輯視窗，讀到 8 站選項與 4 個工具開關；S1 二值化 True、S5 False；關閉不儲存，隔離 S5 INI hash 保持不變。
- **界線**：這是程式內的 WPF 自動化互動 Gate，還不是人手滑鼠點擊／GUI 確認儲存完整 E2E Gate。原生 CLI 写入→重讀→恢復則已於前份 R2 報告通過。

## B. native 初始化拆解

`CVInspection()` 本身在隔離的 STA console 子程序內 15 秒無法回傳，輸出只包含 `CONSTRUCT_BEGIN`，父層已 kill 子程序。尚未呼叫 `SetRecipe`。

對單獨模組逐一以短時程隔離程序確認：

| 組件 | 建構 | 單獨 SetBuffers |
|---|---|---|
| `CVBuffer` | 約 90ms | — |
| `LocateTool` | 約 130ms | **4秒逾時** |
| `BinaryTool` | 約 70ms | PASS |
| `MaskTool` | 約 70ms | 未驗 |
| `AIProphetTool` | 約 110ms | PASS |
| `AIEnhanceTool` | 約 120ms | PASS |
| `AIPredictTool` | 約 60ms | 未驗 |

由原版重建 `InspTools.dll` 反編譯實際確認：

```csharp
public void LocateTool.SetBuffers(CVBuffer bufs) {
    buffers = bufs;
    locateAIEnhanceTool.SetBuffers(bufs);
    locateAIEnhanceTool.CreatInst();  // 不論有沒有載入 Recipe
}
```

`LocateAIEnhanceTool.CreatInst()` 在 `ModelCreatInst == false` 時**無條件進入原生 `DeltaAIProcessDll.dll` 的 `CreateInstance()`**，並在 `useCUDA` 為 True 時呼叫 `SetGPU`。因此現在可確定阻塞發生在「AI 定位原生建立路徑」，但尚不能單靠時間紀錄斷言一定是 `CreateInstance` 或 `SetGPU` 中的哪一個 native entry 凍結。

**根因優先順序：原版將 AI 定位實例建立綁在 `LocateTool.SetBuffers`，導致即使只要一般定位／Recipe，完整 `CVInspection()` 仍被原生 AI 初始化阻塞。**

## C. 防呆 Gate

新增 `NativeRuntimeGate.cs`：

- `InspToolsRecipeCli.exe --runtime-preflight` 回傳 `INSPTOOLS=True`、`DELTA_AI_DLL=True`、`R2_NATIVE_PARAMFILE=AVAILABLE`、`FULL_CVINSPECTION=BLOCKED`，exit **46**。這是已知的未完成 Gate，不代表 DLL 缺檔。
- `InspToolsStandalone.exe --recipe R2PROBE-3923-R002` 現在**即時阻擋並 exit 46**，不會誤觸完整 `CVInspection()` 卡死；在 AppBase 輸出 `RuntimeGate.txt`。
- `--recipe-preview` 的 R2 原版 GUI 唯讀預覽不受影響；一般零參數 GUI 同樣不受影響。
- 真正的 native 單站研究 `--cv-native-diagnostic` 僅作工程用途，必須由父程序限時且強制清理。

## D. 接下來真正需要的改動

1. 定位原廠 Phase-1 **可重建 InspTools C# Source of Truth**；若能重建，在**獨立測試 DLL** 裡把 `LocateTool.SetBuffers` 中 `CreatInst()` 延後至需要 AI Locate 時才執行，必須同時考量模型載入、Dispose、重選 Recipe 的資源生命週期。
2. 同一隔離 Recipe 重跑 `CVInspection()` 單站與八站；確認 Constructor PASS 才測 `SetRecipe`、`SetStationIndex` 及原版 Tool dialogs 保存。
3. 最後才導入原版圖片 Buffer／`Inspect()` 與 Shunt 已驗證的 AI Runtime。**不要現在把整套 Shunt Runtime 複製進來**。

## 禁止宣稱

- 未完成 `CVInspection/Inspect` 或 AI model inference，不可稱完整 AOI 或完全獨立。
- 這輪找出的是一條明確阻塞的 native AI 初始化呼叫鏈，尚未修補該 DLL 的邏輯。
- 不得以隨意抽換 `InspTools.dll` 或 native `DeltaAIProcessDll.dll` 假裝修復。
