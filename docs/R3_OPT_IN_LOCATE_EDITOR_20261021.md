# R3｜原版定位工具受控編輯入口

日期：2026-10-21
專案：DIA 檢測執行環境／InspTools 二次解譯

## 定位

這不是正式版的全功能 AOI 工具編輯器，而是**限定在 R3 隔離執行環境**的可操作入口，從已經驗證的原版 `LocateWindow` 開始，採「編輯記憶體 → 明確確認存檔 → 原版 SaveAllRecipe → 磁碟重讀」流程。

**正式 `release/current` 不啟用。** R3 仍未呼叫 AI ReadModel／Inspect。

## 手動試用

程式路徑：

`E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯\output\native-lazy\runtime`

在該資料夾執行：

```powershell
.\InspToolsStandalone.exe --recipe-native-r3 R2PROBE-3923-R002 --station 5 --enable-r3-editor
```

- 右鍵「R3｜原版定位工具編輯（隔離測試）」可以選擇 S1～S8，呼叫原版 LocateWindow，不再使用佔位 CVInspection。
- 子視窗關閉僅寫回記憶體 ParamFile；不自動保存。
- 右鍵「R3｜儲存 Recipe（備份＋驗證）」需要確認，先將隔離 Recipe／PublicDB 當前全部檔案保存到 `_r3_backups/<時間戳>/`，再呼叫原版 SaveAllRecipe。
- 儲存後比較八站 `LocateParam.FocusLimit` 和 `BinaryToolON` 與原生 ParamFile 磁碟重新讀取結果；異常即恢復原始 bytes。
- 「R3｜撤回上次儲存」只作用於本次啟動保存前的快照；撤回後必須重啟程式同步記憶體。
- 只有通過 `NativeR3RecipeSession.Open` 的沙盒路徑、R2PROBE Recipe 前綴與 R3 SHA-pinned DLL 才能啟用這些入口。

## 已驗收

- R3 原版 GUI 首頁啟停、真實 CVInspection 八站 SetRecipe：前輪通過。
- 原版 LocateWindow 的 S5 FocusLimit 真實子視窗修改 → ParamFile → 原版 SaveAllRecipe → 新建 CVInspection 重新載入 → bytes rollback：12 PASS／0 FAIL。
- 新增 `--r3-editor-menu-smoke` 搭配 `--enable-r3-editor`：八站原版定位編輯入口存在，未修改前儲存／撤回皆停用，3 PASS、Exit 0。
- `TestNativeLazyInit.ps1` 已將該選單驗證追加為第七階段，在原生六階段均 PASS 才會執行。
- `Build.ps1` 只輸出測試目錄，正式 R1 release、一期來源與 Chao8Side/ShuntVC 生產資料未覆蓋。

## 仍待

- 新選單的**人工滑鼠逐項操作**與「按下儲存／撤回」的完整 E2E 尚未驗證；目前測試分別驗證菜單狀態，以及同底層原生保存流程，不可混稱整個 GUI 交互已驗。
- `Verify` 目前重讀兩項代表性參數，並不是原版所有工具欄位逐欄校驗；保存的完整性與衝突處理仍有擴充空間。
- BinaryWindow 的無圖片 DontSave1 真實保護仍保持，不得清掉旗標硬讓它保存。下一步提供離線測試影像／有效 ROI 再驗該工具。
- 其他原版工具、正式模型、離線 Inspect、OK/NG、真正 AI 推論仍未驗。

## 維護

- `src/InspTools.Standalone/R3IsolatedEditor.cs`：隔離原版定位編輯 + 兩階段存檔 + 快照／復原。
- `src/InspTools.Standalone/DecodedToolsWindowHost.cs`：只在 `--recipe-native-r3` 加 `--enable-r3-editor` 時注入選單。
- `output/native-lazy/runtime/r3-editor-menu-result.txt`：編輯選單冒煙測試。
- 仍以 `Build.ps1` 和 `TestNativeLazyInit.ps1` 做回歸，輸出檔案不進 Git。
