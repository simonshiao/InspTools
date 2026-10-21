# R3｜InspTools 獨立軟體搬移驗收

> **後續獨立產品候選版更新：** 新增 `BuildStandaloneR3.ps1 -Zip`，
> 每次建立不同時間戳的乾淨版本，避免 `BuildPortableReview.ps1` 的一次性
> 工程測試資料夾被清空後誤傷 Recipe。此版本已驗證 25 EXE/DLL、約 89.86 MiB，
> 原版零參數啟動、CLI 獨立 Recipe 匯入和檢查、S5 真實 CVInspection
> 原版 GUI、定位編輯選單 3/3、移動到其他 AppBase 再開啟，全 PASS。
> `release/current` 不變。新的候選版使用指南見
> `docs/PORTABLE_R3_CURRENT.md`；AI 與完整 Inspect 均未驗收。

> **本次乾淨候選版實物：**
> `output/standalone-r3/InspTools-Portable-R3-20261021-231504/`，
> ZIP 為同名 `.zip`；ZIP SHA256 為
> `73A47288AC2C21FE0978E0B96DC110067598FE5C665080B78F4D58A6C3013EB8`。
> 使用 `TestPortableReview.ps1 -PackageDirectory <該候選版路徑>`
> 移動到新的資料夾驗收 **14 PASS／0 FAIL**。
> 此包的 `Recipe`、`Model`、`TrainingImage` 都是空白，
> 未放 ShuntVC 正式資料，正式 release/current 亦未修改。

日期：2026-10-21

## 唯一產品目標

**InspToolsStandalone.exe 是獨立應用程式。**

不重新開發 ShuntVC、DIA 機台 Host、Generic Editor 或 SDK；ShuntVC 僅是 Recipe/圖片/模型的**唯讀測試供體**。離線 Recipe 管理、原版 GUI、八站真實 CVInspection 與後續 Inspect/AI 都由獨立 InspTools 的 AppBase 執行。正式 release/current 維持 R1 baseline，不得以 R3 預覽覆蓋。

## 已完成的獨立攜帶版本

`BuildPortableReview.ps1` 從既有 Build.ps1 產生的 standalone launcher 及 Hash-pinned R3 recovered InspTools.dll 打包到：

`output/portable-review/InspToolsStandalone.exe`

壓縮檔：

`output/InspTools-Portable-Review-R3.zip`

套件約 89.86 MiB，共 27 個頂層程式／DLL；內含空白 `Recipe/Model/PublicDB/TrainingImage`，沒有 ShuntVC 的 Recipe、模型、圖片，也沒有 Camera、PLC 或整套 Training。

`portable-review.marker` 啟用獨立 R3 **原生八站離線預覽**。除了標記外，也必須由執行器驗證 R3 DLL 的完整 SHA256 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`，防止把 R1 或 Shunt 的不同 DLL 當作同一版本。

獨立應用程式可直接雙擊 EXE 開啟原版 `INSPECTION TOOLS`，右鍵匯入完整 Recipe，再選「載入八站 Recipe（獨立離線預覽）」。原版檢測控制項目前禁用，因為完整 AI 模型／Inspect 尚未驗證；不能把預覽模式宣稱為正式檢測。

## 可重現的真正搬移 Gate

`TestPortableReview.ps1` 先將套件複製到另一位置 `output/portable-relocation-test/Moved`，然後只從搬移後 AppBase 啟動／呼叫：

- 檢查整包 DLL SHA 不變、未附 Shunt fixture。
- 空 Recipe 的獨立 CLI 與原版 WPF 啟動／正常關閉。
- 使用本案先前隔離的非 Shunt `R2PROBE-3923-R002` Recipe 作為**測試時另外匯入**的副本。
- CLI 匯入、結構檢查、八站真實原生 ParamFile 讀取。
- 由搬移位置執行 `InspToolsStandalone.exe --recipe-native-r3 R2PROBE-3923-R002 --station 5`；出現真正的 R3 八站 GUI 且正常關閉、無 error file。

2026-10-21 實測：**14 PASS、0 FAIL**，含 `PORTABLE_RELOCATION=PASS`。

又針對**實際 ZIP** 執行 `TestPortableReview.ps1 -FromZip`：
`Expand-Archive` 解壓到新的 AppBase，完成空 Recipe GUI、Recipe 匯入、
八站參數讀取及 R3 真實八站 GUI 啟閉，**16 PASS／0 FAIL**。
ZIP 為 `output/InspTools-Portable-Review-R3.zip`，
大小 34,078,780 bytes，SHA256：
`643954BA1C5EFAA1972AD31943B8251ED708B5762A3F433856B4BC65563B2AF9`。

本份 `output/portable-review` 是**會重新建置的測試包資料夾**，
不可在其 Recipe 放長期正式資料。若需永久保存獨立候選版，
另有 `BuildStandaloneR3.ps1` 會以時間戳建立不覆蓋的版本資料夾，
需各自做驗收；兩個打包入口不得混稱正式 release。

測試記錄：

`output/portable-relocation-test/portable-acceptance.txt`

以上證明同一台 Windows 開發機**更換位置仍可運作**；尚未跨不同電腦、無 .NET Framework、不同 GPU、未安裝 VC Runtime 的乾淨系統測試，因此不可宣稱完全免安裝或已在所有環境通過。

## 驗收分層

| 獨立軟體功能 | 現況 |
| --- | --- |
| Windows x64 原版 GUI 直接啟動 | PASS |
| 另一目錄搬移／正常關閉 | PASS |
| 本機 Recipe 匯入、複製、八站讀取 | PASS |
| 真實 CVInspection 八站 SetRecipe | PASS |
| 原版 Locate/Binary 子視窗及 SaveAllRecipe | 隔離測試 PASS，正式操作尚未放行 |
| Shunt 完整 Recipe／真實影像測試 | 僅測試資源，不隨軟體發布 |
| 正確影像 Locate | 實驗 PASS，仍需整合驗收 |
| AI Enhancement 模型原生載入 | 未完成 |
| 真正 OK/NG／Inspect 完整能力 | 未完成 |
| 非開發機獨立安裝／使用者驗收 | 未完成 |

## 下一個階段

1. 加入獨立版自己的影像開檔與顯示入口（不依賴 Shunt 模擬器），允許使用者選擇 S1～S8，顯示各工具真實結果，不能以檔名判定 OK/NG。
2. 將已在隔離試驗通過的原版工具參數編輯、SaveAllRecipe 加入正式可回復、可管理的獨立 UI 流程；務必先建立全檔備份並驗證成功才能宣告儲存。
3. 原生 AI Runtime 以**獨立產品內相對路徑和最低依賴集合**部署；只在需要模型時載入，避免過早初始化。不要為了成功加載模型而依賴 ShuntVC 或舊訓練工具的外部路徑。
4. 提供乾淨 Windows 機器的可攜性驗收、必要的 .NET Framework 4.7.2 與 VC Runtime 條件，再升版正式交付包。
