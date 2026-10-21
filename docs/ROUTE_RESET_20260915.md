# InspTools 二次解譯｜路線重置

更新：2026-09-15 23:4x +08:00

## 唯一目標

把一期已解譯重建完成的 **InspTools 原版介面** 做成可獨立直接啟動的正式入口。

唯一正式入口：

`InspToolsStandalone.exe`

預期行為：

```text
雙擊 InspToolsStandalone.exe
  -> 不需要 DIA / Shunt / Replica / ReferenceHost
  -> 不需要先選 Recipe
  -> 直接建立 decoded/rebuilt InspTools.ToolsWindow
  -> 顯示原版 INSPECTION TOOLS UI
```

## 不是本案目標

以下全部從正式主線移除：

- 新寫 generic `InspToolsEditorControl / Recovered Tool Workspace`
- Core / SDK / WPF machine-first 產品化作為本案主目標
- DIA-like / Shunt-like / ReferenceHost demo 作為正式產品入口
- Annotation Core / Annotation Standalone
- Agent / AgentBridge / MCP annotation protocol
- SAM/AutoMask/CLIP 包裝
- 重新設計 Tool UI
- 自己重做原廠標註器

上述檔案可保留作歷史研究證據，但**不得再被正式 Build、Solution、Release、README、Handoff 當作本案下一步**。

## 正式三階段路線

### R1｜Independent UI Shell

目標：零參數直接叫出原版 compiled `ToolsWindow`。

Gate：

- process 獨立存活
- Window title=`INSPECTION TOOLS`
- 無 DIA/Shunt/Replica/ReferenceHost dependency
- 無 Recipe requirement
- 無 generic editor
- 無 Annotation/Agent UI
- 不因空白啟動載入大型 AI Runtime

### R2｜Recipe Lifecycle

只在 R1 穩定後做。

直接移植一期已驗證：

`ReplicaLoader.InitializeCompiledRecipeLifecycle()`

禁止另造一套 RuntimeRoot/ModelRoot/Station 表單。

Recipe 選取後，應由 Recipe / AppBase convention 自動取得 station/model/tool 設定。

### R3｜Portable Runtime Packaging

只整理讓原版介面可攜所需的最小 managed/native dependency。

目標：

- 不依賴開發機絕對路徑
- 不複製完整 DIA/Shunt 程式
- 不把 Training/CUDA/相機/PLC 等非必要內容塞進 base bundle
- Recipe/AI 需要的額外 Runtime 再依功能分層

## 二期偏航分類

### KEEP｜正式主線

- `src\InspTools.Standalone\OriginalCompiledUiHost.cs`（需簡化成一期 UI launcher）
- `InspToolsStandalone.exe`
- decoded/rebuilt `InspTools.dll`
- 必要 WPF/runtime dependency staging
- 一期 UI13 evidence

### FREEZE｜保留證據但不再 Build

- `src\InspTools.Core`
- `src\InspTools.SDK`
- `src\InspTools.Wpf`
- `src\ReferenceHost`
- `src\DIALikeHost`
- `src\ShuntLikeHost`
- `src\InspTools.Agent`
- `src\InspTools.AgentBridge`
- `src\InspTools.SamOnnx`
- Annotation 相關 docs/tests

### REMOVE FROM FORMAL LINE｜直接切線

- `src\InspTools.AnnotationStandalone`
- `Program.cs` 內 generic white workspace 作正式入口的路徑
- `--diagnostic-workspace` 作正式產品能力的宣稱
- 舊 `release\current` / `IR2-06.zip` 作為「目前正式交付」的宣稱

## 驗收規則

任何新功能都先問：

> 這件事是否讓 decoded InspTools 原版介面更接近「可直接獨立開啟」？

若答案不是，移出本案主線。

## 當前真實狀態

2026-09-15 現場已成功讓二期 `InspToolsStandalone.exe`：

- `ALIVE=True`
- `TITLE=INSPECTION TOOLS`
- `ERROR_FILE=False`
- 顯示原版 ToolsWindow 視覺

但目前 Build/Solution/README 仍混有早先 machine-first / annotation 支線，尚未完成主線清理；本文件即為清理依據。

