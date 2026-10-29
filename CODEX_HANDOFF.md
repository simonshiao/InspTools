## 2026-10-29 Codex 進度：P0 修正待完整 GUI 驗證

新的隔離測試候選：`output/standalone-r3/InspTools-Portable-R3-20261028-233327`；承接 `215835`，不代表正式 release。Build.ps1 與 BuildStandaloneR3.ps1 編譯成功，已分別 staging 300E／先知 e1，並只在隔離候選匯入 3923-R002。

已修改：`ChaoLocatePreviewUi.RunAsync` 分開傳入原版 ToolsWindow host 和對話框 owner，讀取原版 StationIndex；定位預覽僅允許 S1，選圖後及推論前再次檢查。`ChaoSelfAiBridge` 依原版站別顯示面板與啟用按鈕；`Chao8SideRecipePicker` 在原版站別按鈕事件後同步標題。主程式原版 WPF 介面保留。

GUI 初步證據：載入隔離 Recipe 3923-R002 後，原版標題 S1、原版按鈕 1 與 AI 面板目前 S1 一致；按「定位 300E｜選圖預覽」已開啟 S1 離線選圖視窗，未在這一步顯示「未選定有效站別」。尚未成功完成選圖、推論、定位框及 Mask 切換；S1→S2→S8→S1 實際回歸及 S2／S8 停用檢查也未完成，**不得標記 GUI 驗收 PASS**。本輪 GUI 自動化遇到其他桌面視窗取得焦點，須重新確認視窗焦點後繼續，避免操作其他應用。

使用者提供唯讀測試圖片：`D:\0_軟體開發區\6.模板與共用資源區\原廠母本\DIAChip8Side_Welding\TrainingImage\R002-CCD1\Origional\1\NG\NG-1-2025-06-27-16-55-12.png`。僅 S1／CCD1 可使用既有定位模型；不能將該模型用於其他站。完整 Inspect／CVBuffer 整合仍未完成，兩條 AI 既有 smoke 不能代表完整檢測驗收。請保留原有所有 dirty worktree 與正式 Recipe／Model／release/current。

# Codex 接手入口｜2026-10-28

**請先閱讀完整交接：** [docs/CODEX_HANDOFF_CHAO_AI_20261028.md](docs/CODEX_HANDOFF_CHAO_AI_20261028.md)。

這是 **DIA 檢測執行環境／InspTools 二次解譯**，工作樹是當前這個 `InspTools二次解譯-乾淨開發版`。先遵守 `AGENTS.md`；不要誤改一期 ProjectHub、ShuntVC 或正式 DIA 原廠。請勿重置 dirty worktree。

最新基準 `output/standalone-r3/InspTools-Portable-R3-20261028-215835` 已完成 CHAO 自研 UNet 300E 模型的唯讀 SHA staging／CPU 真圖定位 Mask + ROI smoke，以及 AI 先知 e1 ONNX smoke；原版 48×48 Recipe/AI 圖示可見、Recipe S1 載入、AI 面板實體滑鼠開窗 PASS。

**第一優先 P0 未完成：** CHAO AI 面板在最新候選出現 **原版標題 S1 vs 面板 S4**；且 `ChaoSelfAiBridge` 把 CHAO 面板 `dialog` 傳給 `ChaoLocatePreviewUi.RunAsync`，不是原版 ToolsWindow `host`，故按「定位 300E｜選圖預覽」回報「未選定有效站別」。先修站別來源／參數與 S1-only 模型安全防呆，實際跑 GUI 選圖/Mask/Overlay，才往原版 CVBuffer 和 AI 先知接線推進。

**嚴禁把 smoke 當量產品質、重新訓練、清除歷史候選或覆蓋正式 `release/current`。**
