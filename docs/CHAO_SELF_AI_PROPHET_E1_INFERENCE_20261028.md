# CHAO 自研 AI 先知｜ONNX 真圖推論 Smoke｜2026-10-28

## 目的與範圍

承接 `CHAO_SELF_AI_BRIDGE_GATE_20261028.md`，不依賴原版 InspTools 的 AI 授權。這次是 CHAO 自研先知 **1 epoch 測試模型**的輸入／輸出介面真實執行 Gate，**不代表 AI 先知正式模型品質、OK/NG 判定或量產**。

## 原始模型與封裝修復

- CHAO 自研來源：`E:\0_軟體開發區\3.開發中專案區\1.進行中\ProjectHub.Native\ProjectData\Projects\Delta-AIProphet\Output\real-e1\model.onnx`，12,347,154 bytes，SHA256 `230C78FF5CF6518AEACCA0081768B449C657E9009E763ABEDF0B7C9CD295E903`。
- 直接以 `Microsoft.ML.OnnxRuntime.InferenceSession` 開啟原檔會回報 `InvalidProtobuf`，但**不是權重損壞**：檔案前 16 bytes 為 `01 00 00 00 02 00 00 00 00 00 00 00 00 00 08 40`，即 DAVS `struct.pack('ihd', 1, 2, 3.0)` 容器標頭，之後緊接有效 ONNX protobuf。
- 新增 `scripts/StageChaoProphetOnnx.ps1`：只接受指向專案 `output\standalone-r3` 中釘選 DLL 的隔離候選，要求來源 SHA 與正確 16-byte header，**唯讀來源**、不覆寫現有模型，複製時去掉標頭並寫 `ChaoAI\prophet-e1-provenance.json`。`deployment_allowed=false`。
- 成功去封裝副本：`ChaoAI\Models\prophet-real-e1.onnx`，12,347,138 bytes，SHA256 `1212DCE4100DCC159B7C4DE55DE773BAECE69598E5E52F35BE2B7984E4B22B16`。
- 去封裝後既有 `E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Microsoft.ML.OnnxRuntime.dll` 真正建立 `InferenceSession`，`MODEL_SESSION=PASS`。

## 已確認 ONNX 契約

| 張量 | 型別 | Shape |
|---|---|---|
| input | uint8 | [1, 256, 256, 3] |
| output1 | uint8 | [1, 256, 256] |
| output2 | uint8 | [1] |
| output3 | uint8 | [1] |

這是 **輸入影像 → Mask + 兩個額外輸出**的真實 ONNX 合約。測試以 RGB uint8、256x256 的基本等比例縮放導入，但正式模型的色序／保留長寬比／正規化／Score 定義仍待對照 CHAO 自研訓練原始碼，不得直接拿 `output2`、`output3` 當 NG 分數。

## 真圖煙測

- 新增 `scripts/ChaoProphetOnnxSmoke.cs`（C#，呼叫既有 Microsoft.ML.OnnxRuntime）和 `scripts/TestChaoProphetOnnx.ps1`（限定隔離候選、雜湊比對與已知 e1 provenance）。
- 從既有隔離 Shunt S1 NG 圖片 `output\standalone-r3\InspTools-Portable-R3-20261028-172406\TrainingImage\R3-Shunt-CCD1\NG.png` 讀圖；交給 CPU ONNX Runtime 推論，結果：
  - `INFERENCE_SMOKE=PASS`，ExitCode 0
  - Mask 大小 256x256
  - `MASK_NONZERO=65406`、`MASK_MIN=0`、`MASK_MAX=255`
  - `RAW_OUTPUT2=136`、`RAW_OUTPUT3=3`
  - `QUALITY_GATE=NOT_TESTED`、`MODEL_PROVENANCE=real-e1, one epoch; NOT DEPLOYABLE`
- PNG：`output\standalone-r3\InspTools-Portable-R3-20261028-195627\ChaoAI\SmokeResults\real-e1-mask.png`
- Log：`output\standalone-r3\InspTools-Portable-R3-20261028-195627\ChaoAI\SmokeResults\real-e1-report.txt`
- 推論通過是在**獨立 Smoke 工具**；InspTools 主畫面尚未實際回填 Mask、ROI 或 NG Score，不可宣稱完整 AI 先知功能完成。

## 新候選／建置與 GUI

- `scripts/Build.ps1 -OutputSubdir chao-self-ai-prophet-smoke-20261028` PASS，standalone SHA256 `12F403BC08788D417D29B480463B2BB465F4D56D6553EE5641951F12A5EA3135`；正式專案 1，off-route 0。
- `scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/chao-self-ai-prophet-smoke-20261028` PASS，production_untouched=true。
- **最新隔離候選：** `output\standalone-r3\InspTools-Portable-R3-20261028-195627`。使用 Recipe CLI 唯讀匯入 `3923-R002`，獨立 staged+smoke `INFERENCE_SMOKE=PASS`。
- `InspToolsStandalone.exe --chao-ai-health` 再驗 Exit 0：`PROPHET_STATUS=READY`、CUDA12/cuDNN9/TorchCUDA true，`RUNTIME_HEALTH=PASS; INFERENCE=NOT_TESTED`；此命令**只驗 Runtime**，不涵蓋上面的獨立模型 smoke，屬兩個不同 Gate。
- CHAO 48×48 機器人按鈕／獨立 CHAO AI 面板上一候選已做真實滑鼠驗證；最新候選新增對 `ChaoAI\SmokeResults\real-e1-report.txt` 的檢查，顯示「先知真圖 smoke PASS（1 epoch，不可量產）」。
- 候選沒有原廠 AI marker／QR 授權元件，不依賴 Chao8Side 授權，也沒有覆蓋原版 InspTools.dll。

## 執行方式

在 PowerShell 中由專案根目錄呼叫，**使用 `-Command` 執行腳本（在本次遠端工作區 `-File` 曾卡住）**：

```powershell
$candidate = Join-Path (Get-Location) 'output\standalone-r3\InspTools-Portable-R3-20261028-195627'
& '.\scripts\StageChaoProphetOnnx.ps1' -Candidate $candidate  # 首次且目標不存在才執行
& '.\scripts\TestChaoProphetOnnx.ps1' -Candidate $candidate -Image '<absolute test image path>'
```

## 後續真實功能 Gate

1. 先修正與自研訓練契約完全一致的前後處理與輸出分數解譯，在多張 OK/NG 上比較品質；1 epoch 不能作正式模型。
2. 將已驗證模型預測 Adapter 與 InspTools 選取站別的 `CVBuffer` 對接；離線回填 Mask 和 Score（不修改原版 `Inspect` 成功旗標），做 GUI 驗收和保存回讀。
3. AI 定位仍缺已驗證 300E 自研 checkpoint 的實際路徑／前處理與 IoU Gate。已檢查 `ShuntVC_Chao\Runtime\Training` 及 ProjectHub `DIAChip8Side`、`Shunt-VisionTools` 目錄，未找到可確認的 checkpoint，不應借用原廠或未標註來源的模型假稱自研定位。
