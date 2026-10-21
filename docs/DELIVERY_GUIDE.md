# InspTools 二期｜交付與部署指南

> **歷史文件／OFF_ROUTE。** 本文屬舊 IR2-06 machine-first 交付線，不再代表目前正式產品。2026-09-16 起請讀 `DELIVERY_CURRENT.md`、`ROUTE_RESET_20260915.md`。

版本：IR2-06  
更新：2026-09-15

## 1. 交付包結構

執行 `BuildDeliver.ps1` 後會產生：

```text
release\current\
├─ README.md
├─ manifest.json
├─ bin\
│  ├─ InspTools.Core.dll
│  ├─ InspTools.SDK.dll
│  ├─ InspTools.Wpf.dll
│  └─ InspToolsStandalone.exe
├─ agent\
│  ├─ InspTools.Agent.dll
│  └─ InspTools.AgentBridge.exe
├─ examples\
│  ├─ DIALikeHost.exe
│  └─ ShuntLikeHost.exe
└─ docs\
   ├─ MACHINE_HOST_INTEGRATION.md
   ├─ DELIVERY_GUIDE.md
   ├─ PRODUCT_BOUNDARIES.md
   ├─ AGENT_ANNOTATION_API.md
   ├─ AgentAnnotation.contract.json
   ├─ IR2-04_TOOL_PARITY_MATRIX.md
   └─ ir2-04-tool-parity-matrix.json
```

基本包刻意不放 smoke EXE、Fixture、舊 Replica、ReferenceCache 或大型 SAM/CUDA runtime。

## 2. 機台端最小部署

Machine Host compile-time reference：

```text
InspTools.SDK.dll
InspTools.Wpf.dll
```

Machine Host 執行目錄至少要有：

```text
InspTools.Core.dll
InspTools.SDK.dll
InspTools.Wpf.dll
```

若需要獨立工程設定工作台，再部署 `InspToolsStandalone.exe`。

Host 不需要 reference `InspTools.Core`；Core 是 SDK 的 runtime implementation dependency。

## 3. 機台程式接線

正式 C# 接法只走：

```text
Machine Host -> InspTools.SDK -> InspTools.Wpf
```

RuntimeRoot / RecipeRoot / ModelRoot / RecipeName / Station / ImagePath 全由機台上層設定注入。不要在 InspTools 或 Host 裡寫死開發機/產線路徑。

完整 C# 範例請讀 `MACHINE_HOST_INTEGRATION.md`。

## 4. Standalone

`InspToolsStandalone.exe` 是工程工具入口，可以在 DIA / Shunt / Replica / ReferenceHost 全部沒啟動時無參數獨立啟動。

它不是 Machine Host 的必要 subprocess；正式機台整合仍應直接 reference SDK/WPF。

## 5. Agent 選配

`agent\InspTools.Agent.dll` 提供 transport-neutral Annotation Agent contract。

`agent\InspTools.AgentBridge.exe` 是非 .NET / 隔離場景的選配 transport。Machine Host 不需要它。

SAM backend 與 ONNX/OpenCV runtime 不在基本交付包。若要啟用 SAM，沿用已驗收 optional `InspTools.SamOnnx.dll` contract，但目前完整 CUDA provider co-location 仍未列為正式 packaging PASS。

## 6. Release 重建

使用既有 accepted build 產生交付包：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildDeliver.ps1
```

需要先重建 net472 binaries：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildDeliver.ps1 -Rebuild
```

需要額外 zip：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildDeliver.ps1 -Zip
```

每次產生 `release\current` 都會重新計算 `manifest.json` SHA256，不應手工修改 manifest。

## 7. 交付前必查

- `manifest.json` 存在，且沒有 smoke/Fixture/Replica binary。
- Host compile-time 只 reference SDK/Wpf。
- `InspToolsStandalone.exe` 無參數可獨立開啟。
- ReferenceHost same-process regression PASS。
- 真實 Shunt `station1.ini` SHA before/after 不變。
- `Inspect()` 若仍回 `NOT_RUN`，文件與 Host UI 都不得宣稱完整 AOI execution 已完成。

