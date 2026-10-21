# InspTools｜DeltaAIProcess 完全本機 Runtime 實測

日期：2026-10-21。唯一產品目標：**InspToolsStandalone.exe**。

## 新的實驗證據

來源唯讀：
`E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime\Runtime`

本機隔離 Gate：
`output\delta-native-gates\Gate-20261021-234640`

- `StageDeltaRuntimeGate.ps1` 建立新的、不可覆蓋的獨立 AppBase；放入既有 InspTools 原版 UI 和 SHA 鎖定的 R3 `InspTools.dll`、原生台達 DLL、OpenCV、TensorRT、CUDA Runtime 基礎 DLL。
- 首次四檔不足；使用 `Tools/InspectNativePE.py --imports-only` 只讀 PE import table，發現 OpenCV 需要 `npp*`、cuBLAS、cuDNN、cuFFT 等執行期 DLL。這是部署依賴盤點，**不是 NVIDIA 反編譯**。
- `ExtendDeltaRuntimeGate.ps1` 核驗來源/副本 SHA 後加入 13 個必要執行期 DLL。再以 `Tools/ResolveDeltaImports.py` 檢查遞迴 import table，補得 `cublasLt64_11.dll`（由 `cublas64_11.dll` 要求）。
- 只在新 Gate 副本中執行 `InspToolsRecipeCli.exe --delta-runtime-probe`：
  **`DELTA_NATIVE_RUNTIME=READY`**。這是實際 C# 自有 AppBase 測試，不是從 ShuntVC 原始目錄執行 Python。
- 獨立 AI Runtime 的直接 DLL 預載約 1.7 GiB；**不應讓單純 Recipe 編輯版強制包含這些 GPU DLL**。

## 單站真實模型測試

`TestDeltaGateModel.ps1` 僅允許 `output/delta-native-gates/Gate-*`，必須有明確的 Gate manifest，且 R3 InspTools.dll SHA 符合：
`ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。

執行程序：

1. 測試 CLI 從自身 AppBase 執行 `--delta-runtime-probe`，得到 READY。
2. 唯讀供體 ShuntVC Recipe 3923-R002、S1 OK/NG 圖片、`VSMA8420AYM050JEW_A1.h5d` + `.pbd` 僅匯入 Gate 副本；個別 SHA 驗證。
3. 從 Gate 獨立進程呼叫原版第一期 `InspToolsFacade.LoadRecipe`，八站成功。
4. `InspToolsFacade.LoadConfiguredModel(1, Enhance)` 真正執行，程序正常返回；輸出 `Set Gpu 0`，未逾時。
5. 實際結果 **`SUCCESS=False LOADED=False`**，錯誤文字提示缺失 `Training\Train\train.exe` 以及 `Training\Model\h5_to_pb.exe`。
6. 用 R3 Reflection 再取台達 `AIEnhanceParam.ModelName_A`：`VSMA8420AYM050JEW_A1.h5d`；推導的 `VSMA8420AYM050JEW_A1.pbd` **確實存在**，兩份檔案均 SHA 匹配。實測後 PBD 與 H5D 仍在 Gate Model 中。

結論：**台達 Runtime 的獨立載入 Gate 已通過，模型完整載入 Gate 尚未通過。**
不可把缺舊訓練檔的回傳直接解釋成 NVIDIA 依賴缺失，也不能宣稱已達成真實 Predict、OK/NG。

## 代碼交接

- `src/InspTools.Standalone/DeltaNativeRuntimeBootstrap.cs` 只註冊 AppBase 自己的 Runtime 子目錄，預設懶載入；空 Runtime 下正常 GUI 無依賴。
- `src/InspTools.Standalone/R3FacadeProbe.cs` 的 `--r3-facade-model` 在 Gate 目錄透過 `DeltaNativeRuntimeBootstrap` 先確保 READY；再使用同一個第一期 Facade，固定 S1 獨立測試。
- `StageShuntFixtures.ps1` 增加特定 Gate 標記允許的唯讀來源測試資料；正式成品不包含 Shunt 素材。
- `StageDeltaRuntimeGate.ps1` 只產生 AppBase + 四個已鎖定 SHA 的基本 native DLL。
- `ExtendDeltaRuntimeGate.ps1` 補齊原始 OpenCV 載入需要的 CUDA runtime 子集。
- `Tools/ResolveDeltaImports.py` 只解析 PE import metadata。
- `TestDeltaGateModel.ps1` 限時執行 CLI Model Gate，保留 stdout/stderr，若未能載入會明確標示 MODEL_NOT_LOADED。

## 下一張必做 Gate

不再補整套 Trainer 或動 NVIDIA 原始碼。應對隔離的 **台達 AIEnhanceTool.ReadModel(bool)** 走過的分支加最小化診斷，重點：

- `ReadModel` 運行時取得的實際 `.pbd` 路徑、存在/檔案大小、`CheckEncrypt()` 結果；
- `CreatInst/SetGPU` 是否有隱藏異常，`ReadModel` PInvoke 實際返回與例外；
- 分辨 `ResultText` 的舊 `train.exe/h5_to_pb.exe` 提示是否從初始化／轉換預檢保留，不能未確認就改程式。

不覆蓋 `release/current`、ShuntVC、ProjectHub.Native，不變更 DLL 當前 SHA-pinning，先獨立測試證明再動正式程式。所有模型、GPU runtime DLL、臨時輸出保留在 Git 忽略區。
