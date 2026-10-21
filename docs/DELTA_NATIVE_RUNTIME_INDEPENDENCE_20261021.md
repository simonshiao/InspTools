# DeltaAIProcess 獨立化｜台達原生 Runtime 第一關

日期：2026-10-21。主目標始終是 **InspToolsStandalone.exe** 的完整獨立檢測能力；
ShuntVC_Chao 僅作為檔案供體和舊相容行為的唯讀證據。

## 來源確認

使用者指定的 Runtime：
`E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime`

找到：

- `DeltaAIProcess/DeltaAIProcessDll.dll`：112 KiB，SHA256 `3B056989D395E83B73AA2CD82EF3CB37DDA6DD8C50541DFD051636CA36DB5B38`。
- `DeltaAIProcess/opencv_world480.dll`：464,696,320 bytes，SHA256 `10B1130AB8F0F1DBAFB8EF8D499A993D4274EF09825D2A9A34070615A3B21810`。
- `DeltaAIProcess/nvinfer.dll`：211,135,528 bytes，SHA256 `96A5E25EF7BCF3F885863372B1EB66C879BAD845CACDD35064C7D48FA62638CA`。
- `CUDA11_118/cudart64_110.dll`：526,848 bytes，SHA256 `BA5C2FB526C4EE4BB218CEB3FA5E8BFDE89CE474F38711FDCCE802549BF9FC6F`。
- `CUDA11_118/cudnn64_8.dll` 等 CUDA/cuDNN 組件。
- `ModelConverter/ModelConverterWorker.exe`、`h5_to_pb_118.py`、`trt_build_861.py`。
- `AIRuntime/ChaoAIRuntime.exe` 與 `runtime.json`；它標示本身只是 control layer，`DIA_COMPAT` 與 `PROPHET` 使用隔離 Runtime。

目前 R3 的 DeltaAIProcessDll.dll、ShuntVC 正式 DLL、上述 DeltaAIProcess 版本**三者 SHA 相同**。

## REA + 冷咖啡的正確使用界線

- 已讀 `20261019/ChaoMcpHub/mcp-node/skills/reverse-engineer-anything/SKILL.md`、`cra-native`、`cra-pe`。
- 本次連線沒有註冊 REA/Ghidra/IDA 工具，所以**尚未產生 native 函式級反編譯偽程式碼**。
- 新增 `Tools/InspectNativePE.py`：純 Python stdlib 的 PE 靜態稽核，讀取匯入/匯出表、節資訊及診斷字串，不執行目標 DLL。
- 實證：原生 x64 PE，84 個匯出（含 `CreateInstance` 51、`DisposeInstance` 52、`ReadModel` 78、`SetGPU` 83）；直接匯入 OpenCV 4.8、CUDA 11 Runtime、TensorRT nvinfer。診斷字串含 `ReadModel Start`、`Library inferbox_shared.dll not found or load failed`。
- **不反編譯、不修改 NVIDIA/Intel/OpenCV 廠商程式，也不從它們編造原始碼。** GPU/推論引擎是部署依賴；只有台達 DLL 的 ABI、模型呼叫契約及 C# 包裝流程是本案逆向的目標。

## 已通過的台達原生生命週期實測

`Tools/ProbeDeltaRuntime.py` 以獨立 Python 子程序，從指定 Runtime 的不同子資料夾註冊**當次程序專用** DLL search directories：

```powershell
python Tools/ProbeDeltaRuntime.py "E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime"
python Tools/ProbeDeltaRuntime.py "E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime" --create --timeout 9
```

測試前檢查台達 DLL 完整 SHA。完成：

- Windows loader 對台達 DLL 的完整動態載入：**PASS**。
- `CreateInstance`、`DisposeInstance`、`SetGPU`、`ReadModel` 四項 export：**PASS**。
- 真實 `CreateInstance() → 非 NULL 物件 → DisposeInstance()`：**PASS、子程序 Exit 0、無逾時**。
- 上述測試**沒有呼叫 SetGPU、ReadModel 或 Predict**；也沒有複製供體 DLL、訓練模型、改機器 PATH 或 GPU 驅動。

這能排除「台達 DLL 無法被載入／建立原生物件」作為本機基本問題，**不能推論模型已成功載入**。

## InspTools 本體整合

新增 `src/InspTools.Standalone/DeltaNativeRuntimeBootstrap.cs`，對獨立產品只承認：

```text
InspToolsStandalone.exe
InspTools.dll
DeltaAIProcessDll.dll          # R1 相容既有拷貝
Recipe/
Model/
Runtime/
  DeltaAIProcess/
    DeltaAIProcessDll.dll     # PINNED SHA ONLY
    opencv_world480.dll
    nvinfer.dll
  CUDA11_118/
    cudart64_110.dll
    ... (按模型/執行期需求逐項確認)
  ModelConverter/
    TensorRT861/               # 僅需轉檔時才附帶
```

`DeltaNativeRuntimeBootstrap.InitializeIfPresent()` 只取本 EXE 所在的 `Runtime`，
先驗台達 DLL SHA，再以 Windows `AddDllDirectory/LoadLibraryEx/GetProcAddress` 做程序內預載。
**不得偷偷透過硬編碼絕對路徑找到 ShuntVC。**

本函式目前**只在工程診斷命令**使用，故零參數啟動絕不提前讀 CUDA：

```powershell
InspToolsRecipeCli.exe --delta-runtime-probe
```

輸出 `DELTA_NATIVE_RUNTIME=READY`（Exit 0）、`NOT_BUNDLED`（Exit 65）或錯誤（Exit 66）。
**沒有將 GPU runtime 附進標準 90 MiB R3 搬移包**，實測標準空包得到 `NOT_BUNDLED`；
同時正常 GUI 啟閉 **Exit 0、無錯誤檔**。C# AppBase 內帶依賴之 `READY`
尚未做實體封裝測試，因此**不能以 Python 供體測試代替 C# 獨立成品的 READY Gate**。

## 下一步（不逆向 NVIDIA，不把舊 ShuntHost 引進來）

1. 依目前 native Windows Loader 的最小依賴集合，做**另外一份候選 Runtime 的空間容量預算**，確認後才選定複製子集。OpenCV+TensorRT 兩個檔案即約 676MB，不能直接合併到 90MiB 的基礎軟體。
2. 在 **新建不覆蓋的獨立測試候選目錄**中，只放台達 DLL 與已核對的執行依賴，執行 CLI `--delta-runtime-probe` 的真實 `READY` Gate。
3. 追舊原版 `AIEnhanceTool.ReadModel` 與 `ModelConverterWorker` 的相依路徑，修正台達**C# 包裝流程**：有可用的 `.pbd` 時，無須為推論額外依賴舊 `Training/Train/train.exe`、`Training/Model/h5_to_pb.exe`。
4. 然後做 pinned `.h5d + .pbd` 的單站 `ReadModel`、真實 Predict、InspectFile (OK/NG) 與應用層 UI，不能把 CreateInstance 成功誤認成推論完成。
5. 最後再測乾淨 Windows / 不同 GPU：不要求 NVIDIA driver 脫殼、也不要求安裝整份 ShuntVC。

## 版本與資料

- 所有來源 Runtime/Recipe/Model 依舊唯讀。
- `Tools` 程式碼與文件進 Git；任何模型、Delta DLL、CUDA、TensorRT、OpenCV 二進位或大量備份皆**不進 Git**。
- 原本 `release/current` 保留作 R1 baseline；R3 candidate 的升版由真正獨立的 Runtime / AI 模型驗收決定。
