# 獨立 InspTools｜台達模型與 S1 真實 OK/NG 檢測 Gate

日期：2026-10-22。產品是 InspToolsStandalone.exe。本份結果只對隔離 R3 實驗、S1 Recipe 與兩張核對過的影像成立。

## 根因

R3 實際編譯來源為 output/native-lazy/generated-source/InspTools/AOI/AIEnhanceTool.cs，不等同 ProjectHub.Native 中較新版 AIEnhanceTool.cs。

舊版本的 ReadModel(bool) 在檢查 Model/*.h5d 與 *.pbd 前，先以 if (!File.Exists(text) || !File.Exists(text2)) 強制要求兩個訓練／轉檔程序：

- Training/Train/train.exe
- Training/Model/h5_to_pb.exe

即使模型及預先轉好的 PBD 已存在，仍會提前返回 false。ProjectHub.Native 的較新版 C# 已刪去這個強制前置檢查。

## 獨立實驗補丁

BuildNativeLazyInit.ps1 -ModelPrecompiledExperiment 會從唯讀來源重新建立 output/delta-model-experiment/generated-source。

只修正 AIEnhanceTool.ReadModel(bool) 一處條件：當非 TensorRT 模型、同名 .h5d 和 .pbd 均存在，且各自長度不小於 4096 bytes，才在沒有舊 Trainer/Converter 時繼續。

沒有略過台達原版 CreateInstance、SetGPU、CheckEncrypt、ReadModel，也不改模型格式和 NVIDIA DLL。

實驗 DLL SHA256：
6A6E842F53D7C88674289CD85A2BD65C1A1D71870ED6A565127FE5DB7ACBEBC7

固定 R3 DLL SHA256：
ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B

TestDeltaModelExperiment.ps1 僅允許指定有 manifest 的 output/delta-native-gates/Gate-*，在測試期間暫時換入實驗 DLL、限時呼叫原版 Facade，之後恢復原始 R3 DLL 和模型 bytes，並核對 SHA。

## Native Runtime

cuDNN 推論會在執行期間以檔名動態載入 cudnn_ops_infer64_8.dll、cudnn_cnn_infer64_8.dll、cudnn_adv_infer64_8.dll。它們未必出現在主要 DLL 的 PE import table。

ExtendDeltaRuntimeGate.ps1 -IncludeCudnnInference 只在隔離 Gate 補入核對過 SHA 的推論執行期副本。

DeltaNativeRuntimeBootstrap 使用 LoadLibraryEx 預先從 AppBase 相對 Runtime/CUDA11_118 載入這三個元件，修復 cuDNN 以 LoadLibrary 名稱搜尋不到 AddDllDirectory 子目錄的問題。沒有修改全域 PATH、驅動或登錄表。

GPU Runtime 只應作為選裝推論層，不隨約 90MB 的 Recipe/GUI 基本版一起打包。

## 驗收程序與結果

環境：output/delta-native-gates/Gate-20261021-234640，獨立 AppBase。

測試的資料是 Shunt Recipe 3923-R002 的隔離副本 R2PROBE-SHUNT-3923-R002，及其 S1 的 VSMA8420AYM050JEW_A1.h5d、VSMA8420AYM050JEW_A1.pbd。

影像：Recipe 指定資料夾 VSMA8420AYM050JEW_A1/Origional/1 下的 OK (1).png 與 2026-10-01-21-04-34.png，複製並檢查原始 SHA。

在專案根目錄依序執行：

    powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildNativeLazyInit.ps1 -ModelPrecompiledExperiment
    powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1 -Configuration Release
    powershell -NoProfile -ExecutionPolicy Bypass -File .\TestDeltaModelExperiment.ps1 -GateDirectory ".\output\delta-native-gates\Gate-20261021-234640" -InspectSamples -TimeoutMs 14000

前提：Gate 先經 StageDeltaRuntimeGate.ps1 和 ExtendDeltaRuntimeGate.ps1 -IncludeCudnnInference 建立相同的核對過相依集合，並放入受控 S1 的 Recipe/模型副本。

| Gate | 實際結果 |
|---|---|
| AppBase 自有台達原生 Runtime | READY |
| 第一階 Facade 完整八站 Recipe | PASS |
| LoadConfiguredModel(1, Enhance) | Success=True、Loaded=True |
| S1 OK 真實影像 | Bin=Ok、Locate=True、Binary=Ok |
| S1 NG 真實影像 | Bin=Ng2、Locate=True、Binary=Ng |
| NG 診斷來源 | BinaryTools5 / AR溶融，原版回傳面積 3.895mm |
| 兩張 stderr | 空白 |
| 模型原始 hash | 測後均未變 |
| 隔離 Gate 原 R3 DLL hash | 測後恢復 ACD6... |
| 無 AI Runtime 的零參數原版 GUI | INSPECTION TOOLS 正常開啟／關閉，無 error file |

檢測子程序約 1.9–2 秒／張，包含 Recipe 載入、模型預載、InspectFile，不是跨機效能保證。

測試檔：output/delta-model-experiment/model-gate-result.json 和 Gate 內各次 stdout/stderr。

本次 InspectionOptions.UseAi=false，但 Recipe 的 AI 前處理 Enhance 仍啟用，且已顯式預先載入該模型。這不代表 AI Predict、預分類或訓練都已通過。

## 保留的限制與下一階段

- **這是 S1 一張 OK 加一張 NG 的受控實驗，不是八站、批次精度或跨電腦驗收。**
- 修正版是另外建置的實驗 DLL，未升版至正式 release/current。
- 尚須進行獨立候選版封裝：輕量 GUI/Recipe 主程式與選裝大型 GPU 推論 Runtime 分層。
- 尚須驗證不同 GPU/無 GPU、純乾淨 Windows、使用者實際操作及各站實際 Recipe。
- 不修改 ShuntVC、ProjectHub.Native、原廠 CUDA/NVIDIA DLL。

結論：獨立 InspTools 的首個台達模型與原版 AOI OK/NG 分辨 Gate 已在隔離環境通過，不能誤稱正式量產版全部完成。
