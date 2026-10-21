# Agent 規則

本案正式名稱是 **DIA 檢測執行環境**；內部 ProjectHub ID 暫保留 `InspTools-Rebuild2`。它是 DIA 主線的檢測執行層重大分歧，技術基底為一期 `InspToolsRebuild`。2026-09-15 已做正式路線校正；舊的 machine-first / SDK / Agent / Annotation 路線全部降為歷史證據，不得再當目前主線。

## 開工順序

1. 讀 `docs/ROUTE_RESET_20260915.md`。
2. 讀 `docs/PHASE1_BASELINE_AUDIT_20260915.md`。
3. 讀本目錄 `README.md` / `HANDOFF.md`。
4. 需要一期 source-level evidence 時才讀 `D:\delta\專案進度\Projects\InspToolsRebuild` 指定檔案。

## 唯一目標

`InspToolsStandalone.exe` 必須可獨立直接建立 decoded/rebuilt `InspTools.ToolsWindow`，顯示原版 `INSPECTION TOOLS` 介面。

零參數啟動不得要求：

- DIA / Shunt 主程式
- InspToolsInspectionReplica.exe
- Recipe
- Model path
- RuntimeRoot 表單
- ReferenceHost
- Agent / Annotation / SAM

## 正式路線

1. R1：零參數直接叫出 compiled `ToolsWindow`。
2. R2：若要載 Recipe，只沿用一期 `ReplicaLoader.InitializeCompiledRecipeLifecycle()` 已驗證生命週期。
3. R3：再整理最小可攜 Runtime；不先複製整套 DIA/Shunt。

## 凍結支線

`InspTools.Core / SDK / Wpf / ReferenceHost / DIALikeHost / ShuntLikeHost / Agent / AgentBridge / SamOnnx / Annotation` 目前全部 OFF_ROUTE。

檔案可保留作歷史證據，但不得被正式 Build / Solution / Release 引用，也不得作下一步依據。

## 禁止

- 不再自己重畫 InspTools 主介面。
- 不再用 generic `Recovered Tool Workspace` 當產品入口。
- 不把標註專案混進本案。
- 不把「能開 demo Host」當成二期完成。
- 不修改一期 source 來遷就二期 launcher；一期專案視為 read-only evidence。
- production Recipe / Runtime / Data 保持 read-only。

