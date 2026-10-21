# DIA 檢測執行環境｜Shunt 完整度與獨立化依賴盤點

日期：2026-10-21
性質：唯讀盤點；不修改正式 EXE、DLL、Recipe、Model 或 Runtime

## 結論

**獨立軟體的基礎應沿用已恢復的 compiled InspTools.ToolsWindow，加上已驗證的 Recipe／影像／檢測生命週期，最後導入 Shunt 的三條 AI 控制層。**

Shunt 已經能透過 replacement InspTools 跑到檢測結果與 PLC Twin，但這不等於全部程式碼都已反編譯成可完全獨立建置的 C# 原始碼。不能把「AI 執行環境存在」、「GUI 有功能」、「完全還原 Inspect() 原始碼」混成同一個 Gate。

## 正式來源

1. 本案主線：`E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯`
2. Shunt 正式 canonical：`D:\0_軟體開發區\1.成品區\Shunt外觀機`
3. Shunt 真正的 AppBase：`D:\0_軟體開發區\1.成品區\Shunt外觀機\Runtime\ShuntVC`
4. Shunt 最新介面／功能證據：`docs\PROJECT_HISTORY_README.md`（2026-10-02 進度）及 `docs\CHAO_VISION_UI_FUNCTION_AUDIT.md`
5. Shunt 視覺架構真相：`docs\SHUNT_VISION_CURRENT_TRUTH.md`

注意：E 槽 `E:\0_軟體開發區\1.成品區\Shunt外觀機\Runtime` 不是 D 槽正式 Runtime 的可用替代；D 槽 Runtime 是指向既有唯一 physical backing 的 junction。不要複製 14+ GiB Runtime 來解決此問題。

## 版本差異，2026-10-21 實機查證

| 標的 | Bytes | SHA256 |
|---|---:|---|
 Shunt 正式 AppBase `InspTools.dll` | 6,079,488 | `E8E1AF43900525EB5B73AB918B010FC30F092F890BD8E587F861A592F3B24931` |
 本案正式 release `InspTools.dll` | 6,078,976 | `85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1` |
 Chao8Side 成品 `InspTools.dll` | 6,078,976 | `85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1` |

**這是不同的編譯版本；不能直接覆蓋，也不能僅由 Hash 推定哪些演算法已解譯。** 先找回可重建來源、比較功能與行為，再決定是否升版。

## 已有能力與真正缺口

| 能力 | Shunt 正式成果 | 目前 InspToolsStandalone | 獨立化要求 |
|---|---|---|---|
| 原版 ToolsWindow／工具設定窗 | compiled UI + Shunt Apple Shell V3 | R1 PASS，原版 compiled UI | 保留原版，不重畫 |
| Recipe / S1-S8 | `ReplicaLoader.InitializeCompiledRecipeLifecycle` + `CompiledReplicaHost` | 原始碼有 `--recipe / --station` 與八站生命週期；release 尚未驗證 | 完成 R2 的實際載入與站別測試 |
| 影像、Recent、回測 | `ApplyCompiledImage` / `InspectionHistoryUiBridge` | 尚無完整可用離線回測 Gate | R4，使用原版 buffer/工具事件 |
| 真的 AOI Inspect / OK-NG | Shunt `BackendBridge`、`InspToolsInspectionReplica`、compiled `CVInspection.Inspect()` 已與 PLC Twin 實測 | 尚未回接；舊 SDK `IInspToolsSession.Inspect()` 是 `NOT_RUN` | R5，獨立進程真圖檢測；不靠 ShuntVC |
| 一般 AI／AI Locate／AI Prophet | `ChaoAIRuntime` + 三組 backend 已備齊 | 未導入 | 採用 Runtime control plane，不複製 Training 機台大包 |
| AI 前處理、SAM、預分類 UI | 2026-10-02 Shunt Host 與原生 UI Gate 已有證據 | 未進入目前 R1 Gate | 在獨立 UI 確認功能、模型載入、失敗 fallback |
| PLC／Camera／Light | Shunt Software Twin COMPLETE；真機 parity 待現場 | 刻意排除 | 不要納入獨立檢測工具 |

## AI 環境只讀實測

在 Shunt 正式 AppBase 執行：

`Runtime\AIRuntime\ChaoAIRuntime.exe status --root <Shunt AppBase>`

結果：`exit 0`；`DIA_COMPAT=true`、`PROPHET=true`、`slots=true`。

這是檔案與 Runtime slot 的就緒檢查，不是 GPU 實算、模型準確率或 InspTools 自動重建的證據。General/Locate 使用 CUDA11/cuDNN8；Prophet 使用 CUDA12/cuDNN9。不可把 ABI 混在同一 PATH。

Shunt Runtime 中已找到 `Recipe\3923-R002\station1.ini` 到 `station8.ini`、模型 `.h5d/.pbd/.lh5d/.ltrt`。獨立版 release 的 Recipe 與 Model 目錄仍為空。不得直接修改 production 設定；首次 R2 應使用唯讀來源與隔離測試設定。

## 已確認的依賴／來源風險

1. `ShuntInspectionToolsHost\CompiledReplicaHost.cs` **仍載入** `InspToolsInspectionReplica.exe` 的 `ReplicaLoader`，調用 `LoadCompiledToolsWindow / InitializeCompiledRecipeLifecycle / ApplyCompiledImage`。此 Host 不是完全移除 Replica 依賴的獨立實作。
2. `ShuntVisionBackendShim\BackendBridge.cs` 用 Named Pipe `ShuntVisionToolsBackend` 與 `InspToolsInspectionReplica.exe --backend-service` 互動；它還有機台 Recipe / PLC 的 ownership，不能直接當獨立引擎。
3. 本案 `Build.ps1` 會從 `D:\delta` 尋找 `Projects\InspToolsRebuild`；2026-10-21 該舊專案根沒有找到。**不要執行破壞性重建／清掉已驗收 release；須先找回當前 Phase-1 Source of Truth 或制定明確 donor 與 hash gate。**
4. R1 的原始碼修改時間晚於正式 release EXE，`--recipe / --station` 功能不能直接宣稱已進到 release。

## 建議最小交付順序（保持本案路線）

1. **DIAIR-01 / R2**：定位可再編譯的 Phase-1 `InspTools.dll` 正式 source，修正失效 build donor；保持空白 UI 可開，驗真實 Recipe read-only 及 S1/S8 切換。
2. **DIAIR-02**：比對 Shunt 的 `InspTools.dll` 差異，確認模型名稱、AppBase Model、AIEnhance/AILocate/AIProphet 的初始化與錯誤退化。
3. **DIAIR-03 / R3**：將必要 Runtime 畫成分層部署表，先 GUI/Recipe 輕量包，再按功能選用推論與訓練；不可強制依賴 ShuntVC/Chao8Side 主程式或生產機台。
4. **DIAIR-04 / R4**：還原離線影像與工具預覽、Recent/OK/NG 工作台。
5. **DIAIR-05 / R5**：真實 `CVInspection.Inspect()` 鏈路的獨立驗收，再考慮 CLI-Anything／Chao Agent 接入。

## 驗收紅線

- 必須「只啟動 InspToolsStandalone.exe」即可操作工具；不能依賴 ShuntVC.exe、Chao8Side.exe、PLC 或相機存在。
- 不把 Shunt 的 Host/Backend 名稱改一改，就宣稱檢測引擎獨立化。
- 真檢測要有指定影像、站別、Recipe、回傳 Bin／Tool timing／可重現 log 的實證。
- 先建立完整性與差異的最小 Gate，無證據的 Inspect 狀態仍標示 NOT_RUN。
