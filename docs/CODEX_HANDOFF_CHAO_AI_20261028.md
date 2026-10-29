## 2026-10-29 Codex 進度：P0 修正待完整 GUI 驗證

新的隔離測試候選：`output/standalone-r3/InspTools-Portable-R3-20261028-233327`；承接 `215835`，不代表正式 release。Build.ps1 與 BuildStandaloneR3.ps1 編譯成功，已分別 staging 300E／先知 e1，並只在隔離候選匯入 3923-R002。

已修改：`ChaoLocatePreviewUi.RunAsync` 分開傳入原版 ToolsWindow host 和對話框 owner，讀取原版 StationIndex；定位預覽僅允許 S1，選圖後及推論前再次檢查。`ChaoSelfAiBridge` 依原版站別顯示面板與啟用按鈕；`Chao8SideRecipePicker` 在原版站別按鈕事件後同步標題。主程式原版 WPF 介面保留。

GUI 初步證據：載入隔離 Recipe 3923-R002 後，原版標題 S1、原版按鈕 1 與 AI 面板目前 S1 一致；按「定位 300E｜選圖預覽」已開啟 S1 離線選圖視窗，未在這一步顯示「未選定有效站別」。尚未成功完成選圖、推論、定位框及 Mask 切換；S1→S2→S8→S1 實際回歸及 S2／S8 停用檢查也未完成，**不得標記 GUI 驗收 PASS**。本輪 GUI 自動化遇到其他桌面視窗取得焦點，須重新確認視窗焦點後繼續，避免操作其他應用。

使用者提供唯讀測試圖片：`D:\0_軟體開發區\6.模板與共用資源區\原廠母本\DIAChip8Side_Welding\TrainingImage\R002-CCD1\Origional\1\NG\NG-1-2025-06-27-16-55-12.png`。僅 S1／CCD1 可使用既有定位模型；不能將該模型用於其他站。完整 Inspect／CVBuffer 整合仍未完成，兩條 AI 既有 smoke 不能代表完整檢測驗收。請保留原有所有 dirty worktree 與正式 Recipe／Model／release/current。

# CODEX 正式交接｜InspTools 二次解譯／CHAO 自研 AI

**日期：2026-10-28（台灣），接手電腦：@CHAO2 / HOME-1。**

## 0. 一句話指令

**接手 InspTools 乾淨開發版。先修正 CHAO AI 面板站別錯誤及定位預覽傳參，再在隔離候選中完成 CHAO 自研 UNet 定位 / AI 先知與原版 InspTools 站別影像的接線及 GUI Gate。不修改正式版、原廠授權、現有訓練產物，不重新設計原版 ToolsWindow。**

## 1. 絕對路徑／先讀規則

- **唯一工作樹**：`E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯-乾淨開發版`。
- 先讀 `AGENTS.md`、`docs/ROUTE_RESET_20260915.md`、`docs/PHASE1_BASELINE_AUDIT_20260915.md`，再讀**本檔**與 `docs/CHAO_SELF_AI_BRIDGE_GATE_20261028.md`、`docs/CHAO_SELF_AI_PROPHET_E1_INFERENCE_20261028.md`。
- 先前 `NEXT_CHAT_HANDOFF.md` 有大量歷史段落，**請以本文最新版狀態為準**，尤其早期寫「找不到 300E checkpoint」的文字已過時。
- `main` 的 HEAD 核對值 `109efc906208adbf1b4e6a3c66816d4ae829c053`，**worktree 本來就 dirty**，本輪查看到約 63 項變更含原本移到 `scripts/` 的腳本；不 `git reset/clean/checkout -- .`，不自動 commit，不刪候選，不把所有異動當自己剛做的。
- 只修改乾淨版；`ProjectHub.Native`、`ShuntVC_Chao`、`D:\delta\Chao8Side`、原廠 DIA、舊 `InspTools二次解譯` 都是**唯讀參考**。

## 2. 需求與產品決策（不要倒退）

1. `InspToolsStandalone.exe` 零參數直接顯示**真正原版 `InspTools.ToolsWindow` WPF**；.NET Framework 4.7.2、核心 Chao8Side/InspTools.dll 版號 **1.3.16.28**。DIA 原廠的 `1.3.16.56` 暫不混升。
2. 原版工具按鈕要保持原始排列及原生事件；Recipe 是**「複製參數」左邊的 48×48 正式圖示按鈕**，不是側欄、不是外掛 toolbar。機器人、站別 1–8、複製參數、OK/NG 均不可隨便位移。
3. Recipe 視窗預設 `<EXE 資料夾>\Recipe`，允許檔案總管樣式選擇外部 `Recipe` 根目錄，也可貼路徑；只能**複製匯入候選本機 Recipe**，不可直接在外部母本上 Save。
4. **CHAO 自研 AI 定位與 AI 先知**應重用 CHAO／ShuntVC Runtime，而不是再修原廠 `CheckAuthNoDLG`、`AllowEnableAILocate` 或開授權 bypass。原版 UI 可以借用，原廠 AI 權限不能繞過。
5. 可見按鈕 ≠ 檢測成功。不得把 Runtime READY、模型載入 PASS、1 epoch inference smoke、原版二值化保存 PASS 誤報為整套 `Inspect()` / AI先知量產 / 八站驗收完成。

## 3. 最新可開啟隔離候選（2026-10-28 21:58 建置）

```text
E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯-乾淨開發版\output\standalone-r3\InspTools-Portable-R3-20261028-215835\InspToolsStandalone.exe
```

- 建置：`powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1 -OutputSubdir chao-ai-locate300-preview-20261028` → PASS，正式 project 1、off-route 0。
- 封裝：`powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\BuildStandaloneR3.ps1 -LauncherDirectory output/chao-ai-locate300-preview-20261028` → PASS，`production_untouched=true`。
- Standalone EXE SHA256：`3FE7752C278EE5A59F5F3B609D0CC5B2951059FDDD322E76A4F88613C71EAFAF`。
- 乾淨版 R3 原版重建 `InspTools.dll` SHA256：`ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。
- 同一候選已隔離匯入 `D:\delta\Chao8Side\Recipe\3923-R002`，有 `Recipe\3923-R002\Setting.ini`。
- CHAO 自研 AI 對照候選的模型與資料**已經 staged**，不須再次覆寫：`ChaoAI\Models\locate-ccd1-loss6-300e.h5`、`ChaoAI\Models\prophet-real-e1.onnx`。
- **不包含原廠 AI 授權驗證 marker**；歷史候選 `185028` 會因原廠 QR DLL 啟動當機，**不可採用**。
- GUI 實測最新候選原版主視窗載入 `3923-R002 / S1`，實際滑鼠按機器人圖示可開啟 `CHAO AI｜自研定位與 AI 先知` 面板。**面板內的定位選圖預覽仍有 P0 BUG，未驗過完整 GUI 推論**。

## 4. 已取得的 CHAO 自研模型與真實證據

### 4.1 UNet / 定位 300 epoch（已找回！）

- 舊版**唯讀來源**：
  `E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯\output\decoded-ui\Release\TrainingCandidates\ShuntLoss6_CCD1_300E_20261022\temp_unet_US_plain.h5`
- 原始和候選拷貝 SHA256 均為 `3BF34C2691CEFCB4B500F6F86B844D87CEA11B801BDB85CAACD4D3E871006634`（約 7.99 MB）；`best.weights.h5` 也在歷史 job。
- 來源 `quality_recovery_result.json`：300 epoch、Shunt 2026-09-30 AgentVisionLab、**Loss 6、43 組訓練配對、4 個獨立 holdout 家族**；原廠種子續訓。holdout R IoU `0.7770970782`，原廠 `0.9405703331`；`production_qualified=false`。後續 500E holdout `0.5769` 更差，不升版。
- 新增 `scripts/StageChaoLocate300.ps1`，對已釘選原版 DLL 的隔離候選唯讀 staging + SHA 防呆，輸出 `ChaoAI\locate-300e-provenance.json`。
- 新增 `scripts/ChaoLocateOffline.py`（候選內在 `ChaoAI\Runtime\ChaoLocateOffline.py`），沿用既有 AgentVisionLab RGB 前處理：`cv2.imdecode`→BGR 改 RGB、INTER_LINEAR resize 256×256、`float32 / 255`；輸出 R channel 220/255 二值 Mask、connected-component 面積至少 10 的 ROI、還原原圖座標；只輸出 **PNG + JSON**，不寫原版 CVBuffer/Inspect/Recipe。
- **真實 NG 單圖 CPU 推論 PASS**：`ChaoAI\SmokeResults\locate300-GUI-reference\locate-report.json`、`locate-mask.png`、`locate-overlay.png` 已存在；真圖 `1237×701` → 256×256；`CHAO_LOCATE_SMOKE=PASS`、`ROI_COUNT=1`，`r_mask_nonzero_thresholded=2459`，模型 ROI `[232,98,22,153]`，原圖 ROI `[1121,268,106,419]`；本候選觀察到第一輪約 127.6ms，**不是穩定 GPU latency**。
- 注意：目錄 `locate300-GUI-reference` 名稱是**給 GUI 參考的離線命令列輸出**，不是證明 GUI 選圖執行成功。GPU TensorFlow 未載入 CUDA11 DLL，這輪強制 CPU；不應稱 GPU 推論通過。

### 4.2 AI 先知 ONNX 1 epoch（技術煙測，不可量產）

- 來源：`ProjectHub.Native\ProjectData\Projects\Delta-AIProphet\Output\real-e1\model.onnx`，SHA256 `230C78FF5CF6518AEACCA0081768B449C657E9009E763ABEDF0B7C9CD295E903`。
- 原始有 **16-byte DAVS 包裝標頭**，不能直接以 ORT 打開。只透過 `scripts/StageChaoProphetOnnx.ps1` 去封裝到候選；有效 ONNX SHA256 `1212DCE4100DCC159B7C4DE55DE773BAECE69598E5E52F35BE2B7984E4B22B16`（12,347,138 bytes）。
- 原版 CHAO `ShuntVC_Chao\Runtime\Microsoft.ML.OnnxRuntime.dll` 讀入模型 PASS；input `uint8[1,256,256,3]`，outputs `output1: uint8[1,256,256]`、`output2: uint8[1]`、`output3: uint8[1]`。
- `scripts/ChaoProphetOnnxSmoke.cs` + `scripts/TestChaoProphetOnnx.ps1` 真實隔離 Shunt NG 圖 `INFERENCE_SMOKE=PASS`，曾得到 Mask 65,406 非零像素、`RAW_OUTPUT2=136`、`RAW_OUTPUT3=3`（**不是正式 NG 分數**）。
- 最新候選 `ChaoAI\SmokeResults\real-e1-report.txt` 和 `real-e1-mask.png` 均存在；面板按「先知煙測 Mask」只顯示離線輸出，不是正式先知在 Inspect 內執行。
- `ChaoAIRuntime.exe prophet --root <CHAO Runtime> --status` 曾驗 `PROPHET_STATUS=READY`、TorchCUDA/CUDA12/cuDNN9 可用；這是 Runtime 健康檢查，**不能代替 ONNX 內容/品質/GUI Gate**。

## 5. **P0: Codex 第一張卡（已實際重現，立即修）**

### BUG-A：定位預覽收到錯誤的 WPF Window 實例

- 檔案 `src/InspTools.Standalone/ChaoSelfAiBridge.cs`，按鈕處：`previewLocate.Click` 使用 `ChaoLocatePreviewUi.RunAsync(dialog, root)`。
- 此處 `dialog` 是新開的「CHAO AI」面板，**不是** `InspTools.ToolsWindow` 原版主視窗；`ChaoLocatePreviewUi.Station(Window host)` 反射面板上的 `StationIndex` 會失敗、回 0。
- 實際結果：面板「定位 300E｜選圖預覽」enabled=True，但點擊後文字顯示 **`定位失敗：未選定有效站別`**，因此沒有出現選檔案對話框，也沒有證明紅框/Mask GUI 預覽。修法：由已知 `Show(Window host, ...)` 的**原版 host** 傳入，並讓 OpenFileDialog/預覽仍能以 CHAO 面板為 Owner；不要將模型、站別、視窗 Owner 混為同一參數。

### BUG-B：原版畫面標題 S1，但 CHAO AI 面板讀到 S4

- 現場最新候選 GUI：主視窗標題 `3923-R002 / S1`，按機器人後面板內容卻是 `目前 S4｜Recipe 定位=ON／原版模型名稱=R002-CCD4-DW｜先知=OFF`。
- `ChaoSelfAiBridge.OriginalStationInfo()` 反射 `ToolsWindow.StationIndex`，而 `Chao8SideRecipePicker.ChangeStation()` 經 `SetStationIndex(index)` 更新；`NativeR3RecipeSession.Open()` 也先選站並設定主視窗標題。
- **不要猜 S4 是正確站別**。應以真實 WPF 控制項、Picker `_station.SelectedIndex`、原生 `StationIndex`、對應的 `CVInspection.paramFile`、Main Title 和原生站別按鈕選取狀態作同時快照，定位是哪一層在後續事件中切錯。修正後 **S1→S2→S8→S1** 在開 AI 面板時全部一致才 PASS。
- **安全 Gate：** 現在只有 **CCD1 / S1** 的已驗模型；未完成站別對應模型前，必須**禁止** S2–S8 用 S1 300E 模型假裝推論，不能只換一個 S4 的 UI label。

### P0 後應驗的原版 GUI 流程

1. 新啟動隔離 EXE → 48×48 Recipe → 選 `3923-R002`，確認初始 S1 真正同步。
2. 用**實體滑鼠**點原版 48×48 機器人 → CHAO AI 面板讀 S1，點「定位 300E｜選圖預覽」→ Windows 檔案選擇器 → 選下述 holdout 真圖 → CHAO CPU 推論 → 新 `preview-s1-*\locate-report.json/mask/overlay` → 預覽 Window 顯示紅框，切換 Mask 有效。
3. 站別 S2/S8 要顯示正確站別，且在沒有核准的 S2/S8 自研模型時拒絕模型推論；S1 回復後再測。
4. 點「先知煙測 Mask」可看 e1 Mask（不能顯示成正式 OK/NG）；原版 Recipe/Binary/Locate 原生圖示仍可用、不移位。
5. 關閉重啟／Recipe 保存回讀等已有的隔離 Gate 不應被這輪 UI 破壞。不可在正式資料夾執行。

## 6. 相關原始碼入口（不要重寫一套）

| 檔案 | 責任／注意 |
|---|---|
| `src/InspTools.Standalone/Chao8SideRecipePicker.cs` | 原版 Grid 48×48 Recipe／原版機器人 Click 接線、站別 Combo；真正 WPF 控件 |
| `src/InspTools.Standalone/ChaoSelfAiBridge.cs` | CHAO AI 面板、Runtime 健康檢查、兩顆煙測按鈕；**BUG-A 呼叫端** |
| `src/InspTools.Standalone/ChaoLocatePreviewUi.cs` | 反射站別、隔離子進程離線推論、Mask/Overlay Preview；**BUG-A 接收端** |
| `src/InspTools.Standalone/NativeR3RecipeSession.cs` | 八站 Recipe/Throughput 初始化、原版 SetStationIndex；vendor AI parity marker 分支是歷史 |
| `src/InspTools.Standalone/R3IsolatedEditor.cs` | 隔離參數修改/備份/SaveAllRecipe；勿繞過正式資料唯讀 |
| `scripts/StageChaoLocate300.ps1` | SHA 固定的 CHAO 300E 模型隔離複製，不可在已 staged 候選重做 |
| `scripts/ChaoLocateOffline.py` | 真圖 CHAO 300E TensorFlow CPU Mask/ROI JSON/PNG |
| `scripts/StageChaoProphetOnnx.ps1` | 16-byte DAVS wrapped ONNX 唯讀去封裝 |
| `scripts/TestChaoProphetOnnx.ps1`, `scripts/ChaoProphetOnnxSmoke.cs` | 先知獨立 ORT 實際推論，研究候選不可量產 |
| `scripts/Build.ps1`, `scripts/BuildStandaloneR3.ps1` | 只建新 output 子資料夾，勿覆蓋 `release/current` |

## 7. 實際路徑／可重現命令

由**乾淨開發版根目錄** PowerShell 執行：

```powershell
$candidate = Join-Path (Get-Location) 'output\standalone-r3\InspTools-Portable-R3-20261028-215835'
$old = Get-ChildItem (Split-Path -Parent (Get-Location).Path) -Directory |
    Where-Object { $_.Name -like 'InspTools*' -and $_.FullName -ne (Get-Location).Path -and
        (Test-Path (Join-Path $_.FullName 'output\decoded-ui\Release\TrainingImage\DIA_CCD1_S1_Holdout4')) } |
    Select-Object -First 1 -ExpandProperty FullName
$image = Join-Path $old 'output\decoded-ui\Release\TrainingImage\DIA_CCD1_S1_Holdout4\X\NG\NG-1-2025-06-27-16-55-12.png'
$python = 'E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime\TrainingPython\Python38\python.exe'
# 另選新輸出名稱（現有 locate300-GUI-reference 已存在，腳本會拒絕覆寫）
$newOutput = Join-Path $candidate 'ChaoAI\SmokeResults\locate300-codex-unique-test'
$env:CUDA_VISIBLE_DEVICES = '-1'
& $python (Join-Path $candidate 'ChaoAI\Runtime\ChaoLocateOffline.py') `
    --model (Join-Path $candidate 'ChaoAI\Models\locate-ccd1-loss6-300e.h5') `
    --image $image --output $newOutput
```

首次使用**新隔離候選**（不能覆蓋舊 candidate）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1 -OutputSubdir codex-chao-ai-fix-UNIQUE
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\BuildStandaloneR3.ps1 -LauncherDirectory output/codex-chao-ai-fix-UNIQUE
$fresh = '<new output/standalone-r3/InspTools-Portable-R3-... absolute directory>'
# PowerShell 在遠端中文路徑曾有 -File encoding 問題，必要時改用 -Command 並讓腳本由 PS 自動列舉舊專案
& '.\scripts\StageChaoLocate300.ps1' -Candidate $fresh
& '.\scripts\StageChaoProphetOnnx.ps1' -Candidate $fresh
& (Join-Path $fresh 'InspToolsRecipeCli.exe') --recipe-import 'D:\delta\Chao8Side\Recipe\3923-R002' '3923-R002'
```

不要在同一 candidate 上反覆 stage；兩套腳本有 SHA／不覆寫防呆。**不需要重跑 300/500 epoch**。若要更換採用 GPU 或新模型，另開安全 Gate。

## 8. 驗收狀態矩陣（以此為準）

| Gate | 結論 |
|---|---|
| 原版 WPF 主視窗零參數啟動 | PASS |
| Recipe 48×48 原版 Grid、外部 Recipe 目錄匯入 | 舊 Gate PASS；最新候選載入 S1 PASS |
| 原版 S1 二值化真圖修改→保存→重啟回讀 | 之前隔離 Gate PASS；最新 AI 候選未重跑全回歸 |
| 機器人原版圖示開 CHAO 自研 AI 面板 | 最新候選實體滑鼠 PASS |
| CHAO 300E checkpoint 找回／SHA／品質報告 | PASS，研究用，不能部署 |
| CHAO 定位離線真圖 Mask/ROI | **CPU CLI Smoke PASS**（不是 GUI） |
| CHAO 定位「選圖預覽」GUI | **FAIL／P0 BUG-A** |
| CHAO 初始站別一致性 | **FAIL／P0 BUG-B**（主視窗 S1，面板 S4） |
| CHAO AI 先知 e1 ONNX 真圖 Mask | 獨立 CPU Smoke PASS，1 epoch |
| CHAO AI 先知正式分數語義、OK/NG 品質 | NOT_DONE |
| CHAO 定位／先知回填原版 CVBuffer、Inspect | NOT_DONE |
| 八站自研模型正確映射／切換與部署品質 | NOT_DONE |
| 正式版本／原廠資料更新 | **禁止；未變更** |

## 9. 其他風險／下階段，不要假驗收

- 本輪先處理 P0 GUI，再接**離線影像 → CHAO inference → Mask/ROI/Score → 原版緩衝區**；遵守原版主視窗，不新增取代它的自製 Inspector。
- AI 先知 e1 `uint8` 的 `output2=136`、`output3=3` 語義不明；訓練前處理、Scores、熱圖需回到 `Delta-AIProphet` 源碼核對，不能直接轉為 NG 判定。
- 300E UNet 是由原廠種子續訓的 CHAO 自研候選，**不是完全隨機起點**，也不是已驗證量產模型。
- 4 個 holdout 家族太少，且 300E 仍落後原廠；品質 Gate = `IMPROVED_LOSS6_STILL_NOT_QUALIFIED`。
- 原版 `InspTools.dll` 內有已知歷史授權/初始化支線，新的 CHAO AI 不依賴它。不能把歷史原廠 AI 授權候選 `185252` 當成自研正式路線。
- Codex 接手前先 `git status` 看 dirty worktree；不執行帶清理的 build/deploy/commit 指令。不刪舊 output，來源保持 read-only。

## 10. 對接 Codex 第一則任務範本

> 接手 @CHAO2 的 InspTools二次解譯-乾淨開發版。先閱讀 AGENTS.md、CODEX_HANDOFF.md 及 docs/CODEX_HANDOFF_CHAO_AI_20261028.md，不要改其他專案、不清理現有 dirty Git。最新隔離候選 20261028-215835 的 CHAO 300E 定位 CLI 真圖 Mask/ROI PASS、AI 先知 e1 ONNX Smoke PASS；但原版主視窗顯示 S1、CHAO AI 面板曾讀到 S4，且定位預覽呼叫 RunAsync(dialog,root) 導致「未選定有效站別」。先修這兩個 P0，再實際 GUI 選圖→產生新 Mask/Overlay→彈窗查看，S1、S2、S8 站別一致；只有 CCD1 模型，S2–S8 禁止誤推論。採用新版本化隔離 output，通過建置與 GUI 後更新本交接；不要訓練、不要碰正式 Recipe/Model 或原廠授權。
