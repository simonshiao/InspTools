# R3｜使用 ShuntVC 正式 Recipe 與真實影像驗證二值化

日期：2026-10-21。所有操作僅在 InspTools 的 E 槽 output/native-lazy/runtime 測試區；ShuntVC 正式 AppBase 唯讀。

## 來源

- 正式 ShuntVC：D:\0_軟體開發區\1.成品區\Shunt外觀機\Runtime\ShuntVC
- InspTools Recipe：Recipe/3923-R002（21 個檔案，約 2.32 MB）
- 機台 Recipe 名稱 VPMGK05500VN0Y0BW，與 InspTools Recipe 不是同一個識別。
- CCD1 OK 圖：TrainingImage/R002-CCD1/Origional/1/OK/OK-1-2025-06-27-16-54-46.jpg（1292 × 964）
- CCD1 NG 圖：TrainingImage/R002-CCD1/Origional/1/NG/NG-1-2025-06-27-16-55-12.png（1292 × 964）
- OK SHA256: 1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45
- NG SHA256: E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4

## 隔離測試建置

- StageShuntFixtures.ps1 會用本案的 Recipe CLI 將原始 Recipe 匯入為 R2PROBE-SHUNT-3923-R002；只複製兩張 CCD1 原圖至 TrainingImage/R3-Shunt-CCD1，逐檔比對供體、測試副本 SHA。
- shunt-fixtures-manifest.json 只放在 ignored 測試輸出，不提交供體圖片、Recipe、模型或二進位。
- TestNativeLazyInit.ps1 自動清空重建 R3 測試 AppBase，重新匯入上述精簡素材、執行原生 Gate。
- 須用 R3 修正版、SHA 鎖定的 InspTools.dll；不修改 ShuntVC 的正式 DLL。

## 實測

- 在 R3 隔離 AppBase 用 --r3-shunt-image-smoke R2PROBE-SHUNT-3923-R002：原版 OpenCvSharp 解碼 OK/NG，CVBuffer 導入真實圖片，原版 BinaryWindow 顯示與關閉均 PASS。兩張影像均使原版 DontSave1 / DontSave2 / DontSave 都是 False，沒有關閉或繞過檢測區域防呆。
- 使用 --r3-shunt-binary-save-smoke R2PROBE-SHUNT-3923-R002：在原版 BinaryWindow 修改 TrackerName → 原版 Window_Closed 寫入 ParamFile → 原版 ToolsWindow.SaveAllRecipe → 建立新的 CVInspection.SetRecipe 重新從磁碟載入 → 驗證名稱一致 → 本機 Recipe/PublicDB 按原 bytes 還原。
- **二值化寫入測試：24 PASS／0 FAIL、Exit 0**。
- Shunt 正式 Recipe/3923-R002/station1.ini 雜湊仍是 3A09D9514F3661D8E634F19962DDAB583BEFCE69C603CD48D8F2681AD0E95A03，供體不變。
- 測試前在本機 _r3_shunt_binary_backup 保存檔案副本，避免中途結束時無法復原。若此備份已存在，重新執行會拒絕覆蓋。需重新由 TestNativeLazyInit.ps1 建立可拋棄的 sandbox 才能重新測試。

## 明確限制

此測試僅證明二值化工具 UI、ROI、參數保存、重讀、回復，未執行實際 Inspect、AI 推論與 OK/NG 判定。圖片所在 OK/NG 資料夾是既有標籤，不是本次演算法推論結果。

本次沒有複製整包 Runtime，也沒有搬入 Model/*.h5d、*.pbd、*.lh5d、*.ltrt 等模型。下一關是按站配對真正的模型及最小原生依賴，限時測 ReadModel，再驗證 Inspect。
