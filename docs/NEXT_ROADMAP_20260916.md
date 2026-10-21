# DIA 檢測執行環境｜後續工作盤點

更新：2026-09-16

## 專案定位

- 正式名稱：`DIA 檢測執行環境`
- 來源主線：`DIA`
- 技術基底：一期 `InspToolsRebuild`
- 目前唯一正式 UI：decoded/rebuilt `InspTools.ToolsWindow`
- ProjectHub 內部 ID 暫保留 `InspTools-Rebuild2`，避免破壞 IR2-01~06 歷史追溯。
- 正式攜帶包技術檔名使用 `DIA-InspectionRuntime.zip`；專案/產品顯示名稱仍為中文，避免舊 Windows PowerShell 對中文 Archive 路徑的相容性問題。
- 母子專案管理尚未成熟；目前只寫 `parent_project_hint=DIA`，不自行發明新的父子資料結構。

## 已完成基線

### R1｜原版 UI 獨立直啟

狀態：PASS。

正式 `release/current/InspToolsStandalone.exe` 可在 DIA / Shunt / Replica / ReferenceHost 全未啟動時直接顯示 `INSPECTION TOOLS`。

這個 Gate 不再重做。

## 下一步必做

### R2｜原版 Recipe Lifecycle

目標：空白 UI 仍可直接開；需要時只選一個 Recipe，其他資訊由原版 lifecycle 自己帶入。

必須沿用一期已驗證順序：

`8x SetRecipe -> SetStation -> SetThroughputRecipe -> 8x ReadModel -> SetStationIndex`

規則：

- 不做 RuntimeRoot / ModelRoot / Station / Image 一排手填欄位。
- Recipe 內保存 Model 名稱與設定。
- Model 實體沿用原版 `AppBase\Model` 規則。
- 真實 production Recipe 只讀；R2 寫入測試用隔離 fixture。
- 驗收至少含一份真實 Recipe read-only 載入與 station 切換。

### R2.1｜Recipe 選取 UX

目標：不破壞原版主畫面。

候選入口：右鍵選單、快捷鍵或極薄啟動參數。正式介面不得再包一層自製 Workspace。

### R2.2｜Model / PublicDB / Throughput 完整性

確認 Recipe 載入後：

- AIEnhance / AIPredict / AILocate 模型名稱由 Recipe 讀出。
- `AppBase\Model` 模型存在性與 `ReadModel()` 行為一致。
- Throughput lifecycle 不為 null。
- 原版各 Tool child dialog 能正常進入。

## R2 完成後

### R3｜最小可攜 Runtime

目標：把目前約 90 MB 的 R1 base bundle 再做依賴盤點，分成：

- UI 必要 managed DLL
- OpenCV/native 必要項
- Recipe/Model 啟用後才需要的項目
- AI/CUDA/TensorRT 選配項
- Training / Camera / PLC / Machine Host 明確排除項

驗收：新電腦解壓後能開 UI；缺少選配 Runtime 時要明確 fail/降級，不可靜默依賴 `D:\delta`。

### R4｜影像與 Tool Preview

只在 R2/R3 穩定後做。

目標：沿用一期 `ApplyCompiledImage` / 已驗證 Tool preview 能力，讓原版介面能載入離線影像並操作工具頁。

這不是重做 AOI execution。

### R5｜真正 Inspect 執行

最後才進。

先盤一期 `InspectFile()` / `InspectToolPreview()` 的真實 dependency，再決定能否把 `Inspect()` 從 `NOT_RUN` 移到 recovered execution。

若不能乾淨抽離，維持 `NOT_RUN`，不得假裝完成。

## 暫時禁止

- 不重畫 InspTools 主介面。
- 不重新接回 Generic Editor。
- 不重開 Core/SDK/WPF product line。
- 不混入「標註學習」專案。
- 不做 Agent / MCP / SAM 自動標註。
- 不先做 DIA/Shunt machine Host。
- 不碰 Camera / PLC / Motion。
- 不修改 production Recipe/Model/Data。
- 不因為 ProjectHub 尚未有母子專案就自行大改平台資料結構。

## 建議 ProjectHub 新工作卡

後續新卡使用新前綴，與已接受的 `IR2-*` 歷史分開：

1. `DIAIR-01`｜Recipe Lifecycle 回接與 read-only 驗收
2. `DIAIR-02`｜Model / PublicDB / Throughput 完整性驗收
3. `DIAIR-03`｜最小可攜 Runtime 與依賴瘦身
4. `DIAIR-04`｜離線影像 / Tool Preview 回接
5. `DIAIR-05`｜Recovered Inspect 執行可行性與 Gate

目前應先做 `DIAIR-01`，其他任務不得提前擴 scope。
