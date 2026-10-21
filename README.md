# DIA 檢測執行環境｜目前正式主線

> 路線校正：2026-09-16 00:05 +08:00

> 正式專案名稱：**DIA 檢測執行環境**  
> 來源主線：**DIA**  
> 技術基底：**InspToolsRebuild**  
> 內部舊路徑 `InspTools二次解譯` 暫保留，等 ProjectHub 正式支援母子專案/路徑遷移後再一次處理，避免現在破壞已驗收證據與腳本。

## 一句話

**本案現在只做一件事：把一期已解譯重建完成的原版 `InspTools.ToolsWindow` 做成可獨立直接啟動的 `InspToolsStandalone.exe`。**

> **2026-10-21 更新：** 現已另有 R3 攜帶測試包 `output/portable-review/`，
> 可在同機另一資料夾執行原版 GUI、匯入與真實載入八站 Recipe，搬移測試 14 PASS。
> 正式 `release/current` 尚未提升至 R3，AI 模型與完整 Inspect 仍待驗證。
> `docs/R3_PORTABLE_INSPTOOLS_GATE_20261021.md` 為最新攜帶版驗收記錄。

不是重畫 InspTools，不是重新做 Tool Editor，也不是 Annotation / Agent 專案。

## 正式入口

開發驗收：

`output\decoded-ui\Release\InspToolsStandalone.exe`

正式交付：

`release\current\InspToolsStandalone.exe`

攜帶包：

`release\DIA-InspectionRuntime.zip`

## 目前狀態

R1｜Independent UI Shell：**PASS**。

已從 `release\current` 直接實測：

- `InspToolsStandalone.exe` 單獨啟動成功。
- `Alive=True`。
- Window title=`INSPECTION TOOLS`。
- `DIAChip8Side=0`。
- `DIAVision=0`。
- `ShuntVC=0`。
- `InspToolsInspectionReplica=0`。
- `ReferenceHost=0`。
- 沒有 `InspToolsStandalone.error.txt`。
- 顯示的是一期 decoded/rebuilt compiled `ToolsWindow` 原版介面，不是二期白色 Generic Editor。

正式 Solution 現在只剩 **1 個 project：`InspTools.Standalone`**。

正式 release 已確認不含：

`InspTools.Core.dll / InspTools.SDK.dll / InspTools.Wpf.dll / InspTools.Agent.dll / AgentBridge / AnnotationStandalone / ReferenceHost / DIALikeHost / ShuntLikeHost / SamOnnx`。

## 一期成果確認

一期真正完成的是：

1. decoded/rebuilt `InspTools.dll` 可直接建立 `InspTools.ToolsWindow`。
2. 原版 AOI child dialogs 已恢復。
3. UI-13 已確認正式視覺基準是 compiled `ToolsWindow` + original child windows。
4. Replica 是 launcher/lifecycle/staging/regression harness，不是 UI 本體。
5. Recipe lifecycle 已在一期 `ReplicaLoader.InitializeCompiledRecipeLifecycle()` 驗證。

詳細稽核：

`docs\PHASE1_BASELINE_AUDIT_20260915.md`

### Binary 注意事項

一期歷史交接記錄 accepted rebuilt DLL SHA256：

`0B4E8C671779B8F33290F6101C5D69105D3182C60BCE30D8B544725183A9F1F2`

但該 binary 現在沒有在一期專案樹中找到。目前可重現並現場驗證的 recovered UI DLL SHA256 為：

`85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1`

正式 `Build.ps1` 已 pin 此 hash；只要一期來源再漂移就直接 fail closed，不靜默換版。不得把 `85C4...` 冒稱為一期 immutable accepted artifact。

## 正式路線

### R1｜Independent UI Shell

已完成。零參數直接叫出原版 compiled `ToolsWindow`。

### R2｜Recipe Lifecycle

下一步候選。只能移植一期已驗證 lifecycle：

`8x SetRecipe -> SetStation -> SetThroughputRecipe -> 8x ReadModel -> SetStationIndex`

不得再做 RuntimeRoot / ModelRoot / Image / Station 一排手填欄位。Recipe 是附加功能，不是開啟 UI 的前置條件。

### R3｜Portable Runtime Packaging

R2 穩定後再做最小可攜依賴瘦身；不先塞完整 Training / CUDA / 相機 / PLC / DIA / Shunt Runtime。

## 已切離主線

下列二期成果目前只保留作歷史研究證據，不再 Build / Solution / Release：

- Core / SDK / WPF Generic Editor
- ReferenceHost
- DIA-like / Shunt-like Host
- Agent / AgentBridge
- Annotation
- SAM / AutoMask

偏航索引：`src\OFF_ROUTE.md`。

舊 `release\InspTools-Rebuild2-IR2-06.zip` 已從正式 local release 區移除，不再視為目前產品。

## 先讀

1. `docs\ROUTE_RESET_20260915.md`
2. `docs\PHASE1_BASELINE_AUDIT_20260915.md`
3. `output\ROUTE_RESET_R1_ACCEPTANCE_20260916.md`

任何後續需求先問：**是否直接讓一期 decoded InspTools 原版介面更接近獨立可用？** 若不是，移出本案。

