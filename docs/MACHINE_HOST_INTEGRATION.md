# InspTools 二期｜Machine Host Integration

> **歷史文件／OFF_ROUTE。** 這是舊 machine-first 支線的整合證據，不是目前 `InspToolsStandalone.exe` 正式路線。除非另開 Machine Integration 專案，否則不要依本文繼續本案。

更新：2026-09-15  
適用：DIA-like / Shunt-like / 一般 .NET Framework 4.7.2 x64 WPF 機台 Host

## Host 只需要知道的邊界

機台端正式 contract 是：

```text
Machine Host
   -> InspTools.SDK
   -> InspTools.Wpf
```

部署 bundle 內會包含 `InspTools.Core.dll` 作為 SDK 的內部 implementation dependency，但 **Host source 不直接 reference Core namespace 或 Core type**。

機台 Host 不需要知道任何 recovered vendor object graph、內部視窗 binder、寫入 guard 或舊的 process bridge。

## 最小引用

Host compile-time 只引用：

```text
InspTools.SDK.dll
InspTools.Wpf.dll
```

WPF/BCL reference 依一般 net472 WPF 程式加入即可。

## 建立 Runtime

所有路徑都由機台程式注入，不由 InspTools 猜 production 路徑：

```csharp
using InspTools.SDK;
using InspTools.Wpf;

IInspToolsRuntime runtime = InspToolsRuntime.Create(new InspToolsRuntimeOptions
{
    RuntimeRoot = runtimeRoot,
    RecipeRoot = recipeRoot,
    ModelRoot = modelRoot,
    EnableAi = true
});
```

`runtimeRoot / recipeRoot / modelRoot` 應來自機台自己的設定、Recipe 選擇或啟動參數，不應寫死成開發機路徑。

## 開 Session

```csharp
IInspToolsSession session = runtime.OpenSession(new InspToolsSessionOptions
{
    RecipeName = recipeName,
    Station = station,
    ImagePath = imagePath,
    Editable = editable
});
```

建議 production 診斷/驗證先用 `Editable=false`。需要工程設定時，才由上層機台權限決定是否建立 editable session。

目前 `Save()` 是 session commit，不直接寫回 production Recipe；production persistence 若未來開放，必須另立 write policy/Gate，不能由 Host 自行繞過。

## 嵌入設定 GUI

直接嵌入同 process：

```csharp
var editor = new InspToolsEditorControl
{
    Session = session
};

machinePanel.Content = editor;
```

若機台需要彈出設定視窗，可用同一套 WPF wrapper：

```csharp
InspToolsEditor.ShowDialog(ownerWindow, session);
```

不需要啟動另一支 InspTools EXE。

## Tool / Recipe API

常用 public API：

```csharp
RecipeParameterSnapshot recipe = session.GetRecipeSnapshot();
StationSnapshot stationState = session.GetStationSnapshot();
IReadOnlyList<ToolDescriptor> tools = session.GetToolCatalog();
IReadOnlyList<ModelDescriptor> models = session.GetModelCatalog();

string value = session.GetParameter("Locate", "LocateAngleLimit");
ToolPreviewResult preview = session.TestTool("Locate/LocateAngleLimit");
```

只有已建立 parity policy 的欄位會由 WPF 專用頁開放修改。Raw Recipe grid 是查閱介面，不應被 Host 當成任意寫入 API。

## Save / Cancel

```csharp
session.SetParameter("Locate", "LocateAngleLimit", "16");
session.Save();

session.SetParameter("Locate", "LocateAngleLimit", "17");
session.Cancel(); // 回到已 commit 的 16
```

若要依 recovered parity policy 操作，建議讓使用者透過 `InspToolsEditorControl` 完成，而不是讓 Machine Host 重複實作各 Tool 的 allowlist。

## Test / Inspect 的目前語意

目前：

- `TestTool(...)` = Core/SDK 參數 preview contract。
- WPF 明確顯示 `PARAMETER PREVIEW / NOT AOI TEST`。
- `Inspect()` = recovered AOI execution 尚未接回時回傳 `Bin=NOT_RUN`。

所以機台整合者不得把 `TestTool()` 成功解讀成完整 AOI inspection 已完成。

## 已驗寫入政策

- Locate：第一期 recovered binder 已確認的 10 個 scalar 欄位。
- Binary：第一期 binder 可 round-trip 的 safe scalar set。
- Mask：已確認 threshold / morphology scalar；bitmap / complex ROI 保持 read-only。
- Pattern DB：read-only。
- Font/OCR DB / training / pattern lifecycle：read-only。
- 未進 parity slice 的 generic Tool：raw parameters read-only。

規則固定：**沒有 parity evidence，不開 write**。

## 兩個可執行範例

Build 後：

```text
output\bin\Release\DIALikeHost.exe
output\bin\Release\ShuntLikeHost.exe
```

`DIALikeHost` 使用 project-local fixture，示範 editable session、Tool policy、preview、Save/Cancel。

`ShuntLikeHost` 完全由參數注入真實 Runtime/Recipe/Model/Image，示範 production read-only integration。它本身不包含任何 production 路徑 fallback。

## Agent / 異語言整合

.NET 機台 Host 優先使用上述 DLL / in-process contract。

Agent 或非 .NET Host 若確實需要跨 process 隔離，可使用既有 `InspTools.Agent.dll` / optional `InspTools.AgentBridge.exe` transport；那是外部整合選項，不是 Machine Host 的必要依賴。

## 永久 Gate

後續改版至少重跑：

1. `Build.ps1` net472 x64 offline build。
2. `test/audit_ir205_hosts.py` public dependency audit。
3. `DIALikeHost.exe --smoke`。
4. `ShuntLikeHost.exe --smoke ...`，production SHA before/after 相同。
5. `ReferenceHost.exe --smoke` same-process regression。
6. `InspToolsStandalone.exe` 無參數獨立啟動 regression。

任何一項重新要求機台 Host 理解 recovered internal state、外部 process/staging 或硬編碼 production 路徑，都視為 integration contract regression。
