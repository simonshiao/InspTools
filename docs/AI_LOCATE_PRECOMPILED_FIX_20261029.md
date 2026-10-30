# AI 定位預編譯模型修復驗證（2026-10-29）

## 修改
scripts/BuildNativeLazyInit.ps1 新增 -LocatePrecompiledExperiment，只在 generated-source 建立實驗 DLL。當未要求重新轉換，且 Model/<name>.lh5d 與對應 .ltrt/.lpbd 都至少4096 bytes，允許略過舊 Training/Train/train.exe 與 Training/Model/h5_to_pb.exe 的存在檢查；原版原生載入、解碼與推論仍實際執行。來源與正式 R3 DLL未修改。

## 結果
- 新 DLL SHA256：BDC5B070AAF95703834E405A3341B3EAAF59B00D1836A4D709041F91069708B0。
- 隔離 probe：output/locate-model-experiment/probe。
- 執行環境：隔離複製 E:/0_軟體開發區/1.成品區/Chao8Side 的 Runtime/DeltaAIProcess、Runtime/CUDA11_118。
- 原廠 Model R002-CCD7DW / Recipe 3923-R002，檢測中沒有訓練程式；BOOTSTRAP=True、ModelLoaded=True、ModelCreatInst=True，非零TensorRT handle。
- CCD1 OK樣本：AI_PRELOCATE=True、QUAD_LOCATE=True，框 x9 y129 w1209 h681，寬10.047mm、高5.673mm、角度0。
- CCD1 NG樣本：AI_PRELOCATE=True、QUAD_LOCATE=True，框 x13 y127 w1216 h680，寬10.105mm、高5.664mm、角度0.047。stdout保存於 probe/locate-ng-result.txt。
- Probe分別直接呼叫 UseAIPreLocate 和 QuadLocate，未修改AllowEnableAILocate或原版授權狀態；這是低階定位演算法驗證，尚非 ToolsWindow 主流程的完整 Inspect。
- 原正式 R3與Chao8Side DLL SHA保持原值。

## 尚待工作
Standalone GUI 需以清楚標記的 CHAO整合入口使用修復版本與執行庫；原版授權路線仍回報未開放，不可把低階測試等同原廠授權已通過。尚未驗證其他站、Recipe保存、完整Inspect或Luma量測銜接。
