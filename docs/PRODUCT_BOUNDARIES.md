# InspTools 二期｜產品能力邊界

> **歷史文件／OFF_ROUTE。** 本文描述舊 Core/SDK/WPF 產品化線。現在正式邊界以 `ROUTE_RESET_20260915.md`、`PHASE1_BASELINE_AUDIT_20260915.md`、根目錄 `HANDOFF.md` 為準。

版本：IR2-06  
更新：2026-09-15

這份文件是交付時的「可以宣稱什麼 / 不可以宣稱什麼」。後續 Agent、工程師與機台 Host 都以這份邊界為最低安全線。

## 已正式完成

- .NET Framework 4.7.2 x64 的 `InspTools.Core / SDK / Wpf` machine-first 架構。
- Machine Host 只需 compile-time reference `InspTools.SDK.dll + InspTools.Wpf.dll`。
- 同 process `InspToolsEditorControl` / `InspToolsEditor.ShowDialog(...)`。
- `InspToolsStandalone.exe` 可無參數獨立啟動，不依賴 DIA/Shunt 主程式先啟動。
- Recipe / Station / Tool / Model catalog public DTO。
- RuntimeRoot / RecipeRoot / ModelRoot / RecipeName / Station / ImagePath 由 Host 注入，產品不猜 production path。
- Locate recovered safe scalar policy。
- Binary recovered safe scalar policy。
- Mask threshold / morphology safe scalar policy。
- Pattern DB / Font-OCR / complex ROI 等證據不足區域 fail-closed / read-only。
- Session Save / Cancel lifecycle；Save 目前只 commit session state，不直接 persistence 到 production Recipe。
- AI Annotation Core / WPF / Agent JSON contract 與 optional AgentBridge。
- optional SAM backend 的 CPU parity 與 PreferCuda -> CPU fallback policy已驗收。
- DIA-like fixture Host 與真實 Shunt read-only Host integration Gate 已驗收。

## 明確尚未完成

### 1. Recovered AOI execution

`IInspToolsSession.Inspect()` 目前仍明確回：

```text
Bin = NOT_RUN
```

因此不能宣稱「完整原廠 AOI 檢測執行已重建」。`TestTool()` 成功也只代表參數 preview contract，不等於 AOI inspection。

### 2. Production Recipe persistence

目前沒有授權一般 Host 把 session 內容直接寫回 production Recipe。未來若要開，需要獨立 write policy、權限與 rollback Gate。

### 3. Pattern / OCR / complex ROI mutation

以下目前仍 read-only：

- Match Pattern DB / public DB lifecycle。
- Font/OCR DB、training、pattern lifecycle。
- Mask bitmap / complex ROI。
- 沒有 recovered parity evidence 的 generic Tool raw parameter。

固定規則：**沒有 parity evidence，不開 write。**

### 4. 完整 CUDA packaging

SAM optional bundle 已驗 CPU 與 provider fallback，但基本交付包刻意不附大型 CUDA provider。完整 `onnxruntime_providers_cuda.dll` 同目錄部署尚未完成正式 packaging Gate，因此不能把「PreferCuda policy」說成「CUDA release 已完成」。

## 禁止重新引入的依賴

Machine Host 正式產品路徑不得要求：

- InspToolsInspectionReplica / 舊 Replica EXE。
- CLI subprocess。
- Named Pipe 作為 Machine Host 必經路徑。
- staging runtime。
- ReferenceCache。
- 各 Tool `*DialogBinder`。
- `CVInspection`、`ParamFile`、`OpenCvSharp.Mat` ownership。
- hard-coded `D:\delta` production fallback。

AgentBridge 可作 Agent/異語言選配 transport，但不屬於 Machine Host 必要 contract。

## 驗收語意

「二期交付完成」代表：**已完成可嵌入機台的正式 SDK/WPF/Standalone/Annotation 公開邊界與部署包**。

它不等於「InspTools 1.3.16.28 每個 private callback、17 個 Tool 全 1:1、完整 AOI execution、production Recipe persistence、CUDA 大包全部完成」。

