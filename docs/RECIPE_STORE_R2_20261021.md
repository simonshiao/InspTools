# DIA 檢測執行環境｜Recipe 獨立化第一階段

日期：2026-10-21
狀態：**Recipe 檔案層及 R1 空白 GUI Gate PASS；完整參數儲存／讀模／Inspect 尚未驗收。**

## 方向

沿用 Chao8Side 的 Recipe 資料夾格式及管理規則，不能複製 Chao8Side 的 MainWindow、PLC、Camera 或 RTX 流程。
檔案格式至少包含 `Recipe/<name>/Setting.ini` 及 `station1.ini`～`station8.ini`，並完整保留 Pattern、Font、PrivateDB 等資料。

獨立版只讀寫自己 AppBase 下的 `Recipe`，原廠／Chao8Side／Shunt Recipe 皆只作唯讀來源。

## 已實作

- `StandaloneRecipeStore.cs`：本機 Recipe 清單、完整性檢查、資料夾匯入、複製／另存、保留原檔編碼的 Recipe 內部路徑更新。
- 複製前完整檢查 8 站、拒絕路徑連結；目標已存在即拒絕覆蓋；中途失敗只清理本次新建目錄。
- `StandaloneRecipeCli.cs` / `RecipeCliEntry.cs`：獨立 console CLI，不必啟動 WPF。
- `StandaloneRecipeUi.cs`：在原版 `INSPECTION TOOLS` 右鍵選單增加「新增／另存 Recipe」和「匯入 Recipe」，仍用原版 `LoadCompiledRecipeLifecycle` 載入。
- `BlankUiLifecycleGuard.cs`：空白 UI 不允許原廠 `Window_Closing` 對未初始化 CVInspection 的 `locateTool`、`paramFile` 操作；真 Recipe 成功載入後恢復原生 callback。
- `StandaloneEntry.cs`：WPF 視窗退出後顯式結束由原廠 DLL 殘留前景執行緒的行程。
- `Build.ps1`：舊一期來源缺失時，只接受當前 release/current 裡 hash 已鎖定的 `InspTools.dll`，且依賴從本案 R1 已驗收 snapshot 取得，不再從退役的 Chao8Side Template 搬。

## 執行方式

目前僅有開發輸出，**尚未覆蓋正式 `release/current`**：

`output/decoded-ui/Release/InspToolsStandalone.exe`
`output/decoded-ui/Release/InspToolsRecipeCli.exe`

在 `output/decoded-ui/Release` 目錄執行：

```powershell
.\InspToolsRecipeCli.exe --recipe-list
.\InspToolsRecipeCli.exe --recipe-check 3923-R002
.\InspToolsRecipeCli.exe --recipe-copy 3923-R002 NewRecipe
.\InspToolsRecipeCli.exe --recipe-import "D:\0_軟體開發區\1.成品區\Chao8Side\Recipe\3923-R002" ImportedRecipe
```

原版 UI 開啟後在視窗右鍵選單進行匯入、另存、載入。

## 實測 Gate

- 編譯：PASS，`formal_projects=1`、`off_route_projects_built=0`；console CLI 為同批次獨立編譯之工具，不回接舊 AgentBridge。
- 原版 `3923-R002` 由 Chao8Side 唯讀匯入：21/21 檔案；`Setting.ini` SHA256 保持一致。
- 自行複製 Recipe：21/21 檔案；CLI list/check/clone/import exit 0。
- 同名匯入被拒絕：exit 2，無覆蓋。
- 空白 GUI：`INSPECTION TOOLS` window 存活；關閉後 `EXIT_CODE=0`、無 `InspToolsStandalone.error.txt`、無殘留。
- 測試用 `output/decoded-ui/Release/Recipe/3923-R002` 為獨立副本，沒有複製 Model 或 AI Runtime，也未執行 AOI。

## 尚未完成／不可宣稱

1. **原生工具參數寫回**：Chao8Side 源碼已確認 `ToolsWindow.SaveAllRecipe()` 與 `CVInspection.paramFile.WriteAllParam()`，但獨立 Host 還沒證明閉合事件後八站參數真的寫入新 Recipe。不能把「複製另存」等同於工具參數保存。
2. **R2 真 Recipe 載入**：本次只有檔案匯入與 CLI 驗證，未用完整模型 Runtime 跑 `SetRecipe/ReadModel/SetStationIndex`。
3. **已載入 Recipe 關閉流程**：現階段只驗了沒有 Recipe 的空白窗；真 Recipe 模式保留原生關閉事件，需另外驗。
4. **AI/Inspect**：ChaoAIRuntime 在 Shunt 有 status PASS，但獨立版尚未整合；`Inspect()` 仍為 NOT_RUN。
5. **完整建置來源**：本次 build 使用 hash 鎖定的二進位快照，並非重新從一期 InspTools C# source 編出 DLL。尋回一期 source 後才能達到完整 source rebuild。

## 接下來

先用隔離 Recipe 配對獨立 Model/Runtime，完成 R2 真實讀取與八站切換；針對原版 `SaveAllRecipe / WriteAllParam` 做明確的保存／重新啟動／比對測試；通過後才宣告「獨立 Recipe 存讀完成」。其後才接離線影像、檢測與三條 AI Runtime。
