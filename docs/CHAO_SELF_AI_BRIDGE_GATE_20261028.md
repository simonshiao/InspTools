# CHAO 自研 AI 定位／AI 先知接線｜2026-10-28

## 方向決議

使用者要求 CHAO 走自研 AI 路線，InspTools 對齊 Chao8Side 1.3.16.28 的原版 WPF 主介面，但 AI 定位和 AI 先知改用 CHAO 自有 Runtime、模型訓練／轉換／推論，不以原廠 AI 授權檢查為自研能力的 Gate；也不得修改原廠功能授權或偽造 Locate/Inspect 成功。

先前 `CHAO8SIDE_AI_PARITY_GATE_20261028.md` 的原廠授權路線已改為**歷史研究**，不應作為後續自研整合主線。新候選沒有 `chao8side-ai-parity.marker`、`ThoughtWorks.QRCode.dll` 或原版 AI 模型複製包。

## 已定位的自研實體資產

- 工作中的 CHAO Runtime 根目錄：`E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime`
- 控制層：`Runtime\AIRuntime\ChaoAIRuntime.exe`；`Runtime\AIRuntime\runtime.json`（`chao-ai-runtime-v1`）
- 自研 AI 定位訓練實作：`Runtime\TrainingRebuild\locate_train_backend.py`，TensorFlow/DAVS 相容訓練和 CUDA11_118 的 `DIA_COMPAT` profile
- 自研 AI 先知：`Runtime\AIProphet\Trainer\AIProphet.exe`，`PROPHET` profile 及其獨立 CUDA12/cuDNN9／Torch CUDA，與 DIA CUDA11 隔離
- 模型轉換：`Runtime\TrainingRebuild\model_converter.py`，配合已有部署 plan/apply/rollback 腳本，不重做
- 自研先知曾產出的 ONNX 測試模型：`E:\0_軟體開發區\3.開發中專案區\1.進行中\ProjectHub.Native\ProjectData\Projects\Delta-AIProphet\Output\real-e1\model.onnx`，12,347,154 bytes，SHA256 `230C78FF5CF6518AEACCA0081768B449C657E9009E763ABEDF0B7C9CD295E903`；這是 **1 epoch 的 smoke 模型**，不是正式部署權重。
- 過往 Shunt/DIA 自研 UNet 300 epoch 候選（R IoU 0.7771）尚未在本輪確認可部署 checkpoint 路徑與獨立推論品質 Gate，不能誤把 Shunt Runtime 裡的既有 `.lh5d/.ltrt` 當作已驗證 CHAO 自研權重。

## 原始碼修改

- 新增 `src/InspTools.Standalone/ChaoSelfAiBridge.cs`。
- 在原版 `grd_Main` 的 **`btn_AI_Function`（機器人）** 加獨立 CHAO AI 路由。按鈕仍保持 48×48 原位，實際滑鼠開啟 `CHAO AI｜自研定位與 AI 先知` 視窗；自研路由不呼叫原廠 AI 側欄的授權流程。
- 面板顯示當前站別、Recipe 的 AI Locate 開關與模型名稱、AI Prophet 開關、CHAO Runtime 與訓練後端是否存在、1 epoch ONNX 是否找到，以及 `推論適配器尚未通過真圖 Gate`。不會因為模型檔存在就標示 AI 已通過。
- Runtime root 可使用 `CHAO_AI_RUNTIME_ROOT` 環境變數明確指定，或在現有 `ShuntVC_Chao\Runtime`、`D:\0_軟體開發區\1.成品區\Chao8Side` 自動選有效 `runtime.json` 路徑。
- `檢查 AI 先知 Runtime` 以隔離 Process 執行 `ChaoAIRuntime.exe prophet --root <path> --status`，18 秒 timeout。只是檢查 CHAO 控制層與 GPU/依賴，不會訓練或推論、不碰原廠授權。
- `StandaloneEntry.cs` 增加 `--chao-ai-health` 的 headless Gate，寫候選根目錄 `chao-self-ai-health.log`，exit 0 只代表 Runtime 健康檢查通過。編譯入口已在 `scripts/Build.ps1` 加入新檔案，原版 InspTools DLL 不變。

## 實際 Gate

- `scripts/Build.ps1 -OutputSubdir chao-self-ai-health-v2-20261028` PASS，正式建置專案 1、off-route 0；Launcher SHA256 `2FB91C3F25E889AF84A64A195E0DF9C22A9F88ED587520B7BE2D6319DCB4C664`。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/chao-self-ai-health-v2-20261028` PASS，production_untouched=true。
- 最新隔離候選：`output\standalone-r3\InspTools-Portable-R3-20261028-194415`。已用本候選 Recipe CLI 匯入 Chao8Side `3923-R002`；正式 donor 唯讀。
- 以 `InspToolsStandalone.exe --chao-ai-health` 執行實際 Runtime Process，Exit 0 並寫入 `chao-self-ai-health.log`：
  - `PROPHET_STATUS=READY`
  - `TORCH_CUDA=true`、`CUDA12=true`、`CUDNN9=true`
  - `RUNTIME_HEALTH=PASS; INFERENCE=NOT_TESTED`
- 真實 WPF GUI 實測：載入 `3923-R002 / S1` → 實體滑鼠點原版機器人 (48×48, `btn_AI_Function`) → 頂層視窗 `CHAO AI｜自研定位與 AI 先知` 出現（PID 38620，GUI PASS）。原版站別及 Recipe 按鈕未移位。
- 候選內無 `chao8side-ai-parity.marker`、無 `ThoughtWorks.QRCode.dll`；未複製原廠 AI 模型或啟動原版授權檢查。
- 沒有更新 `release/current`、Shunt/DIA 正式 Recipe 或任何來源模型，也沒有啟動重新訓練。

## 下一張工作卡（未完成，禁止混淆）

1. **Locate Adapter**：定位 300 epoch 自研候選 checkpoint 的真實路徑、架構、前處理/後處理；驗證可用 CUDA11/DIA_COMPAT 從真實 CCD 影像得到 Mask + ROI + IoU/耗時，再實作介面 `image → CHAO model → Mask/ROI → InspTools CVBuffer`。不可只因載入既有 `.lh5d/.ltrt` 就宣告自研定位成功。
2. **Prophet Adapter**：先用 `real-e1/model.onnx` 做自研 ONNX 輸入輸出契約與不含量產判定的推論 smoke；取得正式訓練權重後再做 S1 單圖 OK/NG、Score、熱力圖 Gate 與 GUI 回填。
3. 兩條模型線均須做站別 S1→S8、隔離 Recipe 保存/重啟讀回、不更改原廠權限或導入未授權模型。

本輪 **完成 CHAO Runtime 對接、GUI 入口、健康檢查與模型線索定位**，但**未完成推論**。交接時不得把 Runtime READY 寫成 Locate/Prophet PASS。
