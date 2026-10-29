# 原版 GUI 二值化｜真圖修改、原生保存、重啟讀回

日期：2026-10-28。僅於乾淨開發版新建隔離候選內實測；不覆蓋原版 InspTools.dll、正式 release/current 或 ShuntVC/DIA 來源。

## 最新隔離候選

- `output/standalone-r3/InspTools-Portable-R3-20261028-173228`
- 原生 R3 InspTools.dll SHA256：`ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`
- Launcher Build：`scripts/Build.ps1 -OutputSubdir native-binary-loaded-hook-20261028` PASS；standalone SHA256 `6F10EEB768D073FB647E8439EF9BBE55ACF90F4443010DEDEB0DE490A7B8CBD3`。
- Portable package：`scripts/BuildStandaloneR3.ps1 -LauncherDirectory output/native-binary-loaded-hook-20261028` PASS。

## 功能接線（原版 UI 不變）

- `Ctrl+O` 轉送給原廠 `btn_UseOpenBitmap.Click`，使用原版 `OpenBitmap()` 及 `CVBuffer` 載圖，不重畫影像 UI。
- 原版 `btn_BinaryTool.Click` 維持呼叫原版 `OpenBinaryTool` 並顯示原版 `BinaryWindow`。離線時主程式利用原版視窗的 Loaded 事件提供全圖 ROI 到原版 `SetBuffer()`，不造假 Locate/Inspect 成功旗標。原版 `DontSave` 防呆未被直接改寫。
- 原版對話框關閉時由其 `Window_Closed` 寫入記憶體中的 `ParamFile`；主畫面 `Ctrl+S` 呼叫 `R3IsolatedEditor.SaveNativeChanges`，先做隔離 Recipe/PublicDB 檔案備份，再呼叫原版 `ToolsWindow.SaveAllRecipe()` 和隔離重讀驗證。
- Recipe 圖示仍直接置於原版 `grd_Main`，AI、複製參數、OK、NG 都不位移。

## 真實 GUI 驗證（PASS）

1. 使用新候選的 Recipe CLI 將 ShuntVC `Recipe/3923-R002` 唯讀匯入為 `R2PROBE-SHUNT-3923-R002`。
2. 將原廠 NG 影像唯讀複製到候選 `TrainingImage/R3-Shunt-CCD1/NG.png`，原始與副本 SHA256 皆為 `E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4`。
3. 原版 Recipe 彈窗選取載入 → `Ctrl+O` 原版 OpenBitmap 對話框 → 選取 NG 影像 → 主畫面顯示真圖，影像數 1/1。
4. 真實滑鼠點原版二值化圖示，原版「二值化」視窗出現。UI Automation 直接操作原版 `txt_TrackerName`：`刮傷` → `R3-GUI-NATIVE-20261028`。
5. 關閉原版子視窗 → 主畫面 `Ctrl+S` → 「確認隔離存檔」確認 → 磁碟 `station1.ini` 具新名稱。
6. 存檔前備份 SHA256 `3A09D9514F3661D8E634F19962DDAB583BEFCE69C603CD48D8F2681AD0E95A03`，與 Shunt 正式來源 bytes 相同；隔離存檔後 SHA256 `C4B725A42A3551067126D9009BFEBED6474B67DBF1B602BC72A312ACCB0C1E20`，正式來源 SHA 未變。
7. 完整關閉測試 Process PID=25532 後重新啟動 PID=25100，經 GUI Recipe 重載 → 原版二值化視窗 `txt_TrackerName` 顯示 `R3-GUI-NATIVE-20261028`，`RESTART_READBACK=PASS`。
8. 存檔前備份保留於 `173228/_r3_backups/20261028-093810-8061280`。

## 原生 CLI 既有 Gate 重新驗證

前一輪隔離候選 `20261028-172406` 使用 `--r3-shunt-binary-save-smoke`：24 PASS、0 FAIL、Exit 0；包含兩張真圖、原版二值化 ROI 防呆值全部 False、原版 SaveAllRecipe、全新 CVInspection 讀回與測試 Recipe/PublicDB 逐位元組還原。這是獨立測試殼 Gate；上述 GUI Gate 另在 `173228` 人機介面確認。

## 限制／後續

- 全圖 ROI 用於**模型未載入的離線二值化參數編輯**；不能視作 Locate/AdvLocate/Inspect 真正成功，亦不代表 AI 先知已可用。
- 本輪僅驗證二值化的一個參數與站別 S1 存檔回讀，未逐項測所有原版圖示、ROI 拖曳、所有工具欄位及八站各種工具。
- `R3IsolatedEditor.Verify()` 主要比對 BinaryToolON 和 FocusLimit；TrackerName 另由本次**全新進程原版 GUI** 獨立驗證，未誤稱 Verify 本身已涵蓋 TrackerName。
- 舊主介面標題不隨 S1→S2 即時更新，屬另外的 UI 同步缺陷。
- 以上是隔離驗收，不代表可寫入 production Recipe；正式 `release/current` 不變。原始工作樹保留未提交修改。
