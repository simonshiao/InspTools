# InspTools 獨立化｜設備 Twin 與 Train 接入規格

日期：2026-10-22。唯一產品是 InspToolsStandalone.exe。ShuntVC/Chao8Side 是唯讀協定與測試參考；原版 compiled ToolsWindow 是唯一主 GUI，不能重做 Generic Editor 或建立第二個機台 Host。

## Port / Adapter / Twin 決策

一個產品、三個可替換邊界：

1. 原版 GUI → 原版八站 Recipe + CVInspection + 台達 AI Runtime。
2. ICameraFrameSource → TwinCameraReplay（已做）／AVT-Vimba、Hik、iRayple adapter（待接）。
3. IPlcInspectionPort → TwinPlcHandshake（已做）／KEYENCE KV Host Link 和其他 PLC adapter（待接）。
4. 原版 AI Training 子視窗 → General DAVS、AI Locate、預分類 Prophet 訓練工作（待接）。

Twin 是本機開發替身，不是再複製一套 ShuntVC。機台位址、Camera SDK、PLC register、燈控與幾何資料應歸各 adapter profile；原版 AOI Recipe 維持獨立檢測設定。DIA 八站與 Shunt 六邏輯站／五實體相機必須使用不同 profile，絕不假定所有相機一站一台。

## ProjectHub.Native 已有成果（來源全部唯讀）

- ProjectData/Projects/Shunt-Machine/Output/SHUNT_DIGITAL_TWIN_V1_PROGRESS_20260912.md：Shunt 六邏輯站、五實體相機；S3/S4 共用相機，具 replay/lock/reconnect。PLC 握手順序 Trigger → reset 0/0 → Status1 → Result → Status2，Trigger 與 Count 為 PLC-owned。實體 PLC 清 Trigger 和 Count 精確時機未證實，不能聲稱與原 ladder 完全一致。
- ProjectData/Projects/Shunt-Machine/Output/SHUNT_PLC_HMI_V01_20260912.md：KEYENCE Twin 有回路測試及現場唯讀限制，沒有真機自動寫入授權。
- ProjectData/Projects/Shunt-Machine/Output/SHUNT-01_CameraSim_取像模式.md：DIA CameraSim 的 FIXED/SEQUENCE、切換單張與 frame ID 連續遞增可以復用。
- ProjectData/Projects/DIAChip8Side/Output/Handoff/DEVMAIN-01_CAMERA_PARAM_FIX_2026-09-11.md：AVT 的 Exposure、Gain、WB 是 CCDParams.ini 真值，不要拿 Setting.ini placeholder 寫回原廠相機；幾何讀回需實機。
- ProjectData/SharedKnowledge/AITraining/THREE_TRAINING_RUNTIME_PLAYBOOK_20260913.md 及 Projects/Shunt-VisionTools/Output/SVT05_THREE_TRAINING_RUNTIME_HANDOFF_20260913.md：三條訓練應各自有資料契約、Runtime、輸出、candidate 與 rollback。

## 本輪真正完成的程式

位於既有 src/InspTools.Standalone 中，仍然只建置一個正式 WPF 產品，未新增 off-route SDK：

- DeviceTwinPorts.cs 的 ICameraFrameSource、IPlcInspectionPort 是未來真機 adapter 的合約。
- TwinCameraReplay 本機圖檔離線 FIXED/SEQUENCE、每張 FrameId 遞增、S3/S4 可共用實體 CameraId、邏輯站分開；防路徑逸出，無 socket 或真機驅動。
- TwinPlcHandshake 模擬 PLC-owned Trigger 與 AOI Status1、Result、Status2；拒絕 stale cycle、重複與順序錯誤；不實作仍未證明的實機 Count 時機，也不對真 PLC 發送命令。
- CLI：InspToolsRecipeCli.exe --device-twin-selftest，實測 DEVICE_TWIN=PASS CHECKS=12 EXTERNAL_CONNECTIONS=0。
- TrainingContracts.cs 驗證 General/Locate 的 X/OK、X/NG、Y/OK、Y/NG 且 X/Y 同名配對；Prophet 為 OK/PASS/NOPASS，不需要 Y mask。
- CLI：--train-contract-selftest 已通過三 family、缺遮罩拒絕、非法 dataset 路徑拒絕；TRAIN_EXECUTED=False。CLI --train-contract-check General|Locate|Prophet DATASET_NAME 只驗證 AppBase/TrainingImage 的資料，不會啟動訓練。

限制：Twin 與 Train Contract 現已編進同一正式產品與 CLI，但尚未接上原版 ToolsWindow 操作，也尚未對接真實機台和實際訓練器。此類 PASS 只能稱合約自測。

## Train 正確復用路線

General：參考 ProjectHub 已驗證的 product-owned TrainingRebuild/train_compat.py → recovered DAVS.DAVSTrain_v3.StartTrainDnet()，使用自己的 Runtime/TrainingPython/Python38，X/Y 成對，輸出 .h5d → .pbd。Shunt 歷史 1 epoch / 8 samples GPU PASS 是參考，移入 InspTools 仍要重新測，不把舊 Training/train/train.exe 接成產品訓練器。

Locate：X/Y 定位 mask → .lh5d → 對本機 GPU 相容的 .ltrt。既有 ProjectHub 曾發現 compute capability 8.6 engine 不可直接跑 8.9；必須在目標 GPU 重新 build 并驗原版定位輸出。

Prophet（預分類）：OK/PASS/NOPASS → .pt、.onnx、可選 .trt。保留原版 UI/授權邊界，不能靠 patch 解鎖。

三者都必須有工作狀態：資料驗證 → 獨立 job → 中途停止/重啟 → candidate 模型 → 轉檔 → 原生 Runtime Reload/Predict → active 模型備份 → 明確提交或 rollback。共用模型要同步處理多站，錯誤不能覆蓋正在使用的正式模型；不能以 subprocess Exit 0 假定訓練成功。

## 尚未完成與接續優先序

1. 真正掛接相機 Twin → 原版 ToolsWindow 影像輸入與站別檢測，驗證固定／循序取像；設備 adapter 不得偷偷借用 Shunt Runtime。
2. General DAVS：先查完整 TrainingRebuild/TrainingPython、Hash 與依賴，建立 product-owned 版本和限時 job launcher，再用 1 epoch 小樣本跑正式 InspTools 的獨立訓練驗收。
3. Locate/Prophet 三套 Trainer 從 ProjectHub 經驗移入，但保留各自 ABI、UI 與授權限制。
4. KEYENCE/AVT/Hik/iRayple 實機 adapter 只留 API，現階段不得自動對 PLC 下寫入或宣稱真機 PASS。
5. 基本 90MB GUI+Recipe 和大型 GPU 訓練／推論 Runtime 分層發布，限制空間/避免把模型二進位推入 Git。

原始 ShuntVC、Chao8Side、ProjectHub.Native、release/current 未修改。
