> **2026-10-28 腳本整理：** PowerShell 已由專案根目錄移到 `scripts/`；功能與新命令見 `scripts/README.md`。歷史文件中的舊路徑需換成 `scripts/`。`EvaluateDiaCcd1Iou.py` 移至 `Tools/`。

# 乾淨開發副本

此副本保留建立時的原始碼、文件、Git 紀錄及未提交變更，並保留 Build.ps1 所需的 release/current 基準依賴。
大型 output、訓練影像、模型、實驗資料及 R3 候選封包仍位於原專案；此副本不包含這些資料。
原專案：E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯
R3 打包、AI 訓練及部分研究腳本仍需要原專案 output 或外部來源，請先核對各腳本的依賴與資料路徑。
此檔為副本說明，建立後請以此副本作為開發工作目錄。

## 可攜版與 Runtime

- portable/InspTools-R3 是 2026-10-22 R3 候選包，包含 GUI 執行所需的隨附 DLL；目標電腦仍需 Windows x64 與 .NET Framework 4.7.2。此候選包未完成跨機驗收。
- output/decoded-ui/Release/Runtime 保留原開發環境的訓練和其他 Runtime；它很大，日常程式碼備份可排除，搬機時須另行備份。它可能還依賴 GPU 驅動、模型、資料或外部路徑，不代表已完成跨機驗證。
- runtime-extra/DeltaAI 是隔離實驗中使用的 GPU Runtime 快照，並未整合或驗證到 portable/InspTools-R3。它不含模型和 Recipe。
- output/native-lazy/generated-source/bin/Release/net472/InspTools.dll 是 R3 打包所需固定版本的 DLL；重新建置它仍需一期來源。
- 原專案中的模型、訓練資料、實驗成果未複製；請按用途另外遷移。
