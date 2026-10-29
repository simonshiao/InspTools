# DIA 候選模型 H5D / PBD 轉換及原版 ReadModel 驗收

日期：2026-10-22。只修改「InspTools二次解譯」，唯讀參考 ProjectHub.Native / ShuntVC_Chao / 原廠 DIA；**沒有部署**候選模型。

## ProjectHub 已驗證研究的直接沿用

- `ProjectHub.Native/ProjectData/SharedKnowledge/AITraining/THREE_TRAINING_RUNTIME_PLAYBOOK_20260913.md`：General DAVS 正式契約為 `.h5d -> .pbd -> ReadModel`，訓練成功不等於推論可用。
- `DIA_AI_GUI_RUNTIME_REUSE_PLAYBOOK_20260914.md`：`ModelConverterWorker.exe` 為正式 PBD 轉換入口；透過 UTF-8/Base64 處理中文路徑、`transform_graph.exe` 放 ASCII 臨時目錄，原始 H5/H5D 只可讀，不得原地移除 header。
- `UNIFIED_AI_RUNTIME_CONTROL_LAYER_20260914.md`：`PBD` server protocol 已在 Shunt 驗證，不再走舊的 `h5_to_pb.exe` License consumer。

## 本案可攜 Runtime

- `StageModelConverter.ps1`：只複製經 SHA-256 固定的 `ModelConverterWorker.exe`、`h5_to_pb_118.py`、`transform_graph.exe`、`h52intOnnx.py`、`trt_build_861.py` 和 `PyFrozenLib`（5,770 files / 約 84.93 MiB）；沿用本 AppBase 的 `Runtime/TrainingPython/Python38` 與 `Runtime/CUDA11_118`。
- 不複製 `TensorRT861` 1.3 GB、來源 `Work` 與 `__pycache__`：這一關只處理 General Enhance PBD，**不能把 Locate TensorRT 宣稱完成**。
- 直接執行本地 Worker `--server --prewarm`：`SERVER_READY PBD=READY TRT=BUILDER_MISSING`、`SERVER_PONG`、`MODEL_CONVERTER_FINALIZE=0`，GPU:0 被 TensorFlow 偵測。

## 模型候選工作的實測證據

來源 `TrainingCandidates/20261022T033549Z-8e528262`：已經 `TRAIN_RESULT=PASS`、`epochs_completed=1`。

新增 CLI：`--train-general-convert 20261022T033549Z-8e528262`

1. 僅接受已成功訓練且原始 `training_result.json`、種子 SHA、候選 H5 SHA、success marker 全部驗過的工作；不得轉換 FAILED / 未訓練資料。
2. 原始 `candidate.h5` **本來就有原廠 16-byte H5D header**，後面為 HDF5 signature。複製成獨立 `candidate.h5d`，不可再額外 prepend 16 bytes。
3. C# 有限時 Worker 子程序以 `PBD<TAB>candidate.h5d<TAB>candidate.pbd` 交給本地 `ModelConverterWorker.exe`。保留 `convert_stdout.log`、`convert_stderr.log`、`conversion_result.json`；不使用任何正式 `Model` 目錄作輸出。
4. 實際 Worker 輸出：`FrozenGraphNodes=354`、`RawPB.Size=7838859`、`transform_graph exit=0`、`OpenCV47Compat folded Const-only binary nodes=4`、`FinalPB.Size=7827704`、`SERVER_RESULT PASS PBD`。
5. `TRAIN_CONVERT=PASS EXIT=0 DEPLOYED=False`。H5D = **7,988,672 bytes**、SHA-256 `7460AC816B12E6E58A647E92F9CCEB56787BFDD87A26FE9E5D6D3A7F28A7FF89`；PBD = **7,827,704 bytes**、SHA-256 `F88D6F19A67C535A739490A6CC65BEC76383A7179498B7803C2427CBF81D207F`。

## 原版 C# / 台達 native ReadModel 真實驗收

新增 `TestDiaCandidateReadModel.ps1`：在全新 `output/delta-native-gates/Gate-*` 隔離環境，以原廠 `3923-R002` Recipe 的本地副本 `R2PROBE-DIA-3923-R002`、並將 candidate `.h5d/.pbd` 以 S1 模型檔名 `R002-CCD1` **只複製到 Gate**；同時只使用先前已經驗證過的 C# `ModelPrecompiledExperiment` DLL 和由 Runtime Gate 腳本整理的原生 cuDNN 依賴。新版 `R3FacadeProbe` 加入嚴格限定 Recipe、站別、實驗 DLL marker 與兩份固定 SHA 的 `--r3-facade-model-candidate`，不影響原本 Shunt 測試入口。

隔離目錄：`output/delta-native-gates/Gate-20261022-115857`。

首次檢查中 PowerShell `Start-Process` 未回傳 ExitCode（`null`），所以測試結果為 NOT_ACCEPTED，**並非模型讀取失敗**；改以 `.NET ProcessStartInfo / ReadToEndAsync / WaitForExit / ExitCode` 取得可信結果並重新驗證。

真實最終結果：

```text
DIA_CANDIDATE_READMODEL=PASS
DIA_CANDIDATE_MODEL_LOADED=True
DIA_CANDIDATE_MODEL_HASHES_PRESERVED=True
APP_LOCAL_DELTA_RUNTIME=READY
CONFIG_MODEL_NAME=R002-CCD1.h5d
SOURCE_PRESENT=True PBD_PRESENT=True
ReadModel Start
New net
Use CUDA
MODEL_NAME=R002-CCD1.h5d SUCCESS=True LOADED=True TENSOR_RT=False
FACADE_MODEL=PASS
```

獨立直接執行 `InspToolsRecipeCli.exe --r3-facade-model-candidate R2PROBE-DIA-3923-R002 1` 再次返回 **exit 0**、stderr 空白。測試記錄保留 `candidate-readmodel-result.json`、stdout/stderr；來源候選模型與原廠正式檔均保持原 hash。

## 結論及剩餘 Gate

**General DAVS 真實 GPU 訓練 → H5D 包裝 → PBD 真轉換 → 台達原生 C# ReadModel 完整串接 PASS。**

本次原版 `ReadModel` 只在 disposable Gate 用實驗 DLL 執行；**沒有完成新模型的真實離線 `InspectFile`、準確率對照、原版 UI 的完整重載，也不代表量產升版**。暫不碰正式模型與 PLC / Camera。
