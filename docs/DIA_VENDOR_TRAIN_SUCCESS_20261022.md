# DIA 原廠 Recipe / 模型隔離訓練實證（2026-10-22）

## 結論

**DAVS General 訓練已在 InspTools 自有 AppBase 真正完成 1 epoch**；`TRAIN_RESULT=PASS`、`state.txt=SUCCEEDED`，模型檔案及回傳 JSON SHA-256 經 C# 執行器核對。這是**最多 8 組圖片的功能性 smoke test**，不是模型準確率驗收，也不是正式部署。

## Source of truth（全部唯讀）

- `D:\delta\Welding\DIAChip8Side\Recipe\3923-R002`：完整 8 站 Recipe，21 個檔案，約 2.32 MB。
- `D:\delta\Welding\DIAChip8Side\Training\Model\navs.ini`：原廠 DAVS_V3 profile：`ImgWidth=256`、`ImgHeight=256`、`UnetDiv=4`、`LossFunction=1`、`ModelType=US`、`OKBlank=1`、`NGIgnore=0`。
- `D:\delta\Welding\DIAChip8Side\Training\Model\navs.h5`：原廠包裝 H5，31,551,392 bytes，SHA-256 `99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73`。
- `D:\delta\Welding\DIAChip8Side\TrainingImage\3923-R002-4`：X/NG 20 張、Y/NG 20 張，依相同檔名一一配對。
- `D:\delta\Welding\DIAChip8Side\Model`：Recipe 有 8 站模型引用，其中 S1/S2/S3/S4/S6/S7/S8 的 `.h5d`/`.pbd` 均可取得。**S5 引用 `06077-15.h5d`，供體本身沒有此檔**；本案不得冒充其已存在。

## 獨立 AppBase

正式測試根為本專案 `output\decoded-ui\Release`，**不是** D 槽原廠。

- `StageDiaVendorSample.ps1`：只複製上述訓練種子、profile、完整 Recipe、7 站模型與 40 張訓練檔；原廠唯讀、SHA-256 比對、不覆蓋已有且不同的目標。
- 本地 `Recipe\3923-R002`：經 StandaloneRecipeStore.Import 建立，`--recipe-check 3923-R002` 通過。模型在本地 `Model`，缺 S5 原廠檔。
- `TrainingSeed\DIA_R002_vendor.h5`、`TrainingProfiles\DIA_R002_vendor.ini`、`TrainingImage\DIA_3923_R002_4`、`VendorReference\3923-R002` 都在本地。
- `--train-general-plan-profile DATASET SEED.h5 PROFILE.ini`：只讀本地 profile 的數值白名單，不允許原廠 INI 絕對路徑或進行正式部署；`Epoch=1`、`BatchSize=1`、`AugRatio=0`、`--compat-max-samples 8`、超時 900 秒仍強制生效。`source-profile.ini` 與 SHA 被封存至 job。

## 真正測試

先前誤配：8,236,960 bytes 的不同 `navs.h5` 搭配 `256x512/UnetDiv=8`，在 Keras concat 報 `[1,64,64,32]` 對 `[1,64,16,16]`；結果 FAIL、epochs_completed=0。

新工作 ID：`20261022T033549Z-8e528262`。

1. `StageDiaVendorSample.ps1` → `DIA_VENDOR_STAGE=PASS`，40/40 檔 SHA 核對。
2. `--train-general-selftest` → PASS；`--train-contract-check General DIA_3923_R002_4` → 20 images/20 masks PASS。
3. `--train-general-preflight DIA_3923_R002_4 DIA_R002_vendor.h5` → READY。
4. `--train-general-plan-profile DIA_3923_R002_4 DIA_R002_vendor.h5 DIA_R002_vendor.ini` → 建立 PREPARED 工作。
5. `--train-general-run 20261022T033549Z-8e528262` → `TRAIN_RESULT=PASS STATE=SUCCEEDED EXIT=0 ELAPSED_MS=9868 DEPLOYED=False`。
6. `training_result.json` → `status=PASS`、`epochs_completed=1`、`tensorflow=2.7.0`、`physical_gpus` 有 GPU:0、`final_loss=0.07862711697816849`；`stdout.log` 記錄 100% progress，C# 核對結果 JSON、成功標記和模型雜湊。
7. 候選模型 `TrainingCandidates\20261022T033549Z-8e528262\candidate.h5`，大小 7,988,672 bytes，SHA-256 `7460AC816B12E6E58A647E92F9CCEB56787BFDD87A26FE9E5D6D3A7F28A7FF89`，與 seed 不同。

## 界線與下一個 Gate

- 原廠 `D:\delta\Welding\DIAChip8Side`、ShuntVC_Chao、`release/current` 及所有正式模型無寫入；無相機/PLC；不曾部署候選模型。
- 本輪只確認訓練契約及輸出真的完成。**沒有**驗證重新訓練時權重繼承比例、模型品質／良率、每個 TensorFlow 算子的實際裝置配置、H5D/PBD 轉換或原版 `ReadModel/Inspect` 載入新候選檔。
- 下個 Gate：先在 AppBase 自有模型轉換測試區，確認候選 H5 可轉原版 `.h5d`/`.pbd`，再用離線圖片進行原版 `ReadModel/Inspect` 比較；不可直接取代正式 `Model`。
