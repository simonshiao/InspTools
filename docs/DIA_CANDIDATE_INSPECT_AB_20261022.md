# DIA 原廠與訓練候選模型｜離線檢測 A/B 回歸 Gate

日期：2026-10-22。專案：InspTools二次解譯。來源唯讀：`D:\delta\Welding\DIAChip8Side`。只操作 `output/delta-native-gates/Gate-*` 兩套可拋棄、互相獨立的 AppBase，**未改原廠、未連 PLC/Camera、未部署任何候選模型**。

## 核心事實

已由上一輪 `20261022T033549Z-8e528262` 完成 General DAVS 真 GPU 訓練（1 epoch、最多 8 組 NG 影像，來源 CCD4）→ `candidate.h5d` → 真正轉換 `candidate.pbd` → 原版 C# `LoadConfiguredModel` ReadModel PASS。

本輪擴充隔離、固定 SHA 的 `R3FacadeProbe.cs`，加入 `--r3-facade-inspect-candidate` 和 `--r3-facade-inspect-vendor`，在原版 Facade `InspectFile` 結束後以反射讀取原廠 `AIEnhanceTool.AIMatR/G/B`（原生 `Mat`），將各通道轉成記憶體 PNG 再 SHA-256，**不寫 AI 影像回 Recipe**。這直接證實 AI 增強預處理產生了非空的影像，而非只宣稱模型載入。

注意：`CVInspection.Inspect` 的原版程式在 `AIEnhanceToolON` 啟用時會把 `UseAI` 設成 `true`；模型成功預載後，實際透過 `aiEnhanceTools.Inspect()` 將結果寫入 `buffers.SetAiEnhanceMat()`。因此 CLI 外部 `InspectionOptions.UseAi=false` 不等於禁用 Enhance；`UseAi` 亦涉及其他預分類與儲存功能。這次確實證明 Enhance 路徑已執行。

## 隔離測試資料

- Recipe：`3923-R002`，Gate 副本 `R2PROBE-DIA-3923-R002`，S1 / `R002-CCD1`。
- 原廠模型：`R002-CCD1.h5d` SHA `99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73`；`R002-CCD1.pbd` SHA `47533BA5CDAFBA4ADC1961F417BD64CFBC572EEF59125293893900F5C831F48B`。
- 候選模型：`.h5d` SHA `7460AC816B12E6E58A647E92F9CCEB56787BFDD87A26FE9E5D6D3A7F28A7FF89`；`.pbd` SHA `F88D6F19A67C535A739490A6CC65BEC76383A7179498B7803C2427CBF81D207F`。
- 圖片 1：DIA `TrainingImage/R002-CCD1/Origional/1/OK/OK-1-2025-06-27-16-54-46.jpg`，1292×964，SHA `1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45`。
- 圖片 2：DIA `TrainingImage/R002-CCD1/Origional/1/NG/NG-1-2025-06-27-16-55-12.png`，1292×964，SHA `E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4`。
- 候選 Gate：`Gate-20261022-115857`；原廠基準 Gate：`Gate-20261022-122900`。兩套原生 Runtime、Recipe、模型、圖樣均分離；每次使用獨立限時程序，原始 SHA 及副本 SHA 有核對。

## 對照實際結果

| 站別 | 樣本 | 原廠 S1 模型 | 新候選模型 | 判定 |
|---|---|---|---|---|
| S1 | OK | **Ok** | **Ng2** | 候選模型誤判 |
| S1 | NG | **Ng2** | **Ng2** | 與原廠一致 |

兩套模型在兩張圖片上都成功 LoadModel、LocateSucceeded=True、AI_ENHANCE_OUTPUT=READY（R/G/B 三通道各有 1 Mat）、InspectFile 返回、ExitCode=0、stderr 空白。**技術流程 PASS，但候選品質 Gate BLOCKED**。

AI Enhance R 通道的 PNG SHA 亦直接不同：

| 圖片 | 原廠增強 R PNG SHA 前 16 字元 | 候選增強 R PNG SHA 前 16 字元 |
|---|---|---|
| OK | `A27131F4B2D8B6CD` | `190B2B95EEA5F3CB` |
| NG | `0ABAE488243662E2` | `8A5FCB148D31E965` |

原廠、候選影像 SHA 相同，模型 SHA 不同，AI 增強圖 SHA 不同，說明**確實使用兩種不同模型運算出不同增強影像**，不是只有 API 載入成功。

最終 NG2 均來自**原版 BinaryTools5「AC－壓傷」**：候選 OK 的 A/B 區域分別為寬 1.487 / 1.438 mm、高 5.614 / 5.581 mm；原廠模型同一 OK 圖中 `BinaryTools5 STATE=Ok`、最終 Ok。因此是模型前處理改變了後段二值化結果，不可直接將 NG2 當成 AI 分類分數。

## 自動回歸與禁止部署

- `StageDiaVendorBaselineGate.ps1`：唯讀複製原廠兩檔至新建的獨立 Gate；使用固定實驗 DLL，不修改正式 GUI DLL。
- `TestDiaCandidateInspect.ps1 -GateDirectory <Gate> [-ModelMode Candidate|Vendor]`：複製兩張精準 SHA 對應的原圖，限時執行原版 `InspectFile`，保存每張 stdout/stderr 和 JSON。對 `AI_ENHANCE_OUTPUT=READY`、ReadModel、InspectFile 返回、退出碼、模型 SHA 做真實 PASS Gate。
- `CompareDiaCandidateAB.ps1`：拒絕資料／來源 SHA 不同、路徑跳出、原廠基準不一致、AI 影像未產生的 A/B。候選結果若任何一張與原廠基準不同，`DIA_PROMOTION_GATE=BLOCKED`；即使兩張一致也只表示 `SMOKE_MATCH_NOT_QUALIFIED`，**不會授權部署**。
- 已實測比較：`DIA_PROMOTION_GATE=BLOCKED DIFFERING_BINS=1 OF=2 AI_OUTPUT_DIFFERENCES=2 DEPLOYED=False`。
- 不可覆寫的候選品質紀錄：`output/decoded-ui/Release/TrainingCandidates/20261022T033549Z-8e528262/qualification_result.json`。

## 後續優先事項

1. 正式訓練必須使用與部署站別、Recipe、成像條件一致的 X/Y 配對資料。此次 **CCD4 NG-only、最多 8 張、1 epoch** 只用於工程驗證，不得用 CCD1 模型上線；CCD1 目前標註 X/Y 很少，不能冒用原始未標註影像當正確遮罩。
2. 保留原廠模型不變；先補足 CCD1 OK/NG 標註和獨立保留的驗證集，再建立新候選並做至少成組離線 A/B 結果、誤判率與遺漏率，最後才能討論部署。
3. 若需進階品質評估，考慮對同一站取得多張獨立時段的影像、追蹤 BinaryTools5 的差異和增強遮罩；不得以這兩張樣本推估量產準確率。

以上為可追溯技術證據，不代表已驗八站 AI 或真機。
