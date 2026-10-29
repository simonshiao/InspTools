# @CHAO3 InspTools 原版工具按鈕修復｜2026-10-29

## 用戶回報

GUI 上方 S1–S8、Recipe 資料夾、複製、AI 機器人仍存在，但原版工具區除 Recipe 外多數點擊沒有反應。

## 已確認根因（兩道 Gate＋一個滑鼠事件層）

1. 舊 `output/chao3-github-9b6f431-20261029` 為 **R1 唯讀預覽**，`LightweightRecipePreview.RestrictOriginalEditing` 把原生工具設為 `IsHitTestVisible=false`，避免未初始化 CVInspection 時誤觸造成當機。
2. CHAO3 自己以釘選封存母本重建的 R3 `InspTools.dll` SHA256 為 `BC56633CD5DD37FAF4274181D9A0100B5A99EAB0D48BDBFDAFDABDFEE71C0223`；CHAO2 已驗 R3 為 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`。兩者不同，不可冒稱相同或放寬成任意 R3 DLL。
3. `NativeR3RecipeSession.IsPortableReview` 原已能以專屬 marker 認證 CHAO3 local SHA，但 `RequireIsolatedSandbox()` **仍只接受 CHAO2 雜湊**，導致真實 Recipe 初始化在最後一道檢查遭拒。已修正一致使用 SHA + CHAO3 marker 雙條件。
4. 在 R3 Recipe 成功載入後，原版部分工具的 `btn_BitmapSave_Copy*` 裝飾 Button 位於真正工具 Button 上方，會吞掉實體滑鼠點擊。UIAutomation 直接 Invoke 底下的 `btn_BinaryTool` 可以開窗，但手動點像素坐標不會開窗；修改 `Chao8SideRecipePicker.FixNestedOriginalToolButtons`，將裝飾 Button 的 `Click` **明確轉送到原版 ButtonBase.ClickEvent** 並設 `e.Handled=true`，避免重複 bubbling。**只有 R3 CVInspection/Throughput/Recipe 成功初始化後才接線**。

## 修改檔案

- `src/InspTools.Standalone/NativeR3RecipeSession.cs`：認證 CHAO3 local R3，含雙重 SHA+marker Gate；同時修復 `RequireIsolatedSandbox` 第二道 SHA 認證。
- `src/InspTools.Standalone/Chao8SideRecipePicker.cs`：裝飾 Button 的實際滑鼠事件轉送到原版工具 Click，不覆蓋原生 handler、不更改工具執行邏輯。
- `scripts/BuildStandaloneR3.ps1`：新增 `-Chao3LocalR3`、固定 SHA、`portable-chao3-local-r3.marker`、版本化輸出。
- `scripts/Build.ps1`：找不到 PATH dotnet 時可使用已存在共享 portable SDK `D:\0_軟體開發區\6.模板與共用資源區\dotnet-sdk-8`；不修改系統環境變數。

既有 `scripts/Build.ps1` 與 `scripts/BuildNativeLazyInit.ps1` 在接手前已有未提交修改；未清理／重置／提交 Git，未修改原廠正式 Recipe/Model、`release/current` 或任何既有已部署檔案。

## 最新隔離候選

```text
D:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools\output\standalone-r3\InspTools-Portable-R3-20261029-152043-CHAO3-review\InspToolsStandalone.exe
```

- Build：`Build.ps1 -OutputSubdir chao3-native-clicks-20261029` → PASS，Launcher SHA256 `CE72B8514DE2D8481AA265C0651E83FC78BC0B37B68BBA9DADA296D902FD820E`。
- Package：`BuildStandaloneR3.ps1 -Chao3LocalR3 -LauncherDirectory output/chao3-native-clicks-20261029` → PASS，25 個執行檔／DLL，約 90 MB，`production_untouched=true`。
- Recipe CLI 匯入 `3923-R002` 到此新候選的本機 `Recipe` 目錄，母本維持唯讀。
- 真實 GUI：啟動 PID 51256，選取 `3923-R002` → 確認載入，主視窗顯示 `[R3 獨立版／真實八站／AI 未載入] 3923-R002 / S1`。
- **實際滑鼠** 點 `btn_BinaryTool`（螢幕座標約 1287,553）→ 出現視窗 **「二值化」**，PASS；關閉不儲存。
- **實際滑鼠** 點 `btn_MaskTool`（約 1418,553）→ 出現視窗 **「遮罩」**，PASS；關閉不儲存。
- 上一個本機 R3 候選 `20261029-151418-CHAO3-review` 執行原版獨立對話框 smoke，Exit 0：原版 `LocateWindow` / `BinaryWindow` 建立/顯示/關閉 PASS、八站 CVInspection 存在、**S1–S8 原始 INI SHA 全數不變**。

## 尚未驗收

僅定位／二值化／Mask 原生設定視窗已確認可打開，不代表所有 18 個工具、參數儲存回讀、二值化對真圖、AI 定位/AI 先知正式推論、完整 Inspect OK/NG、相機/PLC 已通過。CHAO3 local rebuilt DLL 為**獨立工程候選**，不可覆寫正式版或不經 Gate 升版。

如果使用者反映仍只有 Recipe 可用，先核對實際執行檔是否為上述 `152043-CHAO3-review`，不要誤開 `chao3-github-9b6f431-20261029` R1 舊候選。
