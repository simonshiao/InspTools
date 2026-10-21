# InspTools 二期｜一期成果嚴格基準稽核

更新：2026-09-15 23:4x +08:00

## 結論

一期真正完成、且二期應直接繼承的核心不是「另外重畫一套 InspTools」，而是：

1. 原版 `InspTools.ToolsWindow` 已由 decoded/rebuilt `InspTools.dll` 直接實例化。
2. 原版 AOI child dialogs 已恢復；正式視覺基準是 compiled ToolsWindow + original child windows。
3. Replica 的角色是 launcher / lifecycle / staging / regression harness；**Replica 不是 UI 本體**。
4. UI-13 已確認 production path 應優先使用 rebuilt compiled `ToolsWindow`，loose-XAML 只保留 legacy/fallback regression。
5. 真正 Recipe lifecycle 已在一期 `ReplicaLoader.InitializeCompiledRecipeLifecycle()` 中釐清，二期若要接 Recipe 應沿用這條已驗證生命週期，不應再自行發明另一套 Loader。

## 一期 Source of Truth

正式一期專案：

`D:\delta\專案進度\Projects\InspToolsRebuild`

關鍵來源：

- `01_最新狀態\HANDOFF.md`
- `Output\UI13_COMPILED_UI_PARITY_20260913.md`
- `Replica\InspToolsInspectionReplica\ReplicaLoader.cs`
- `Replica\InspToolsInspectionReplica\Program.cs`
- `Source\InspTools-1.3.16.28-Editable`

## 已驗證的 compiled UI 生命週期

UI-13 明確記錄：

```text
SetRemoteMode
-> LoadLanguage(1)
-> 8x CVInspection.SetRecipe
-> ToolsWindow.SetStation
-> ToolsWindow.SetThroughputRecipe
-> 8x CVInspection.ReadModel
-> SAM background preload signal（需要時）
-> ToolsWindow.SetStationIndex
```

影像則走原版：

`ToolsWindow.SetBitmapBuffer(Bitmap)`

這條路徑已由一期 Shunt compiled host-session smoke 驗證，並不是二期猜測。

## 視覺基準

二期正式 UI 只能以一期 compiled `InspTools.ToolsWindow` 為基準。

已驗證的原版狀態包括：

- Stage 1~8 主流程
- Stage 9/10 隱藏
- RemoteMode 顯示
- 主背景 LightYellow
- 原版工具流程圖與圖示
- 原版 Locate child window layout / tabs / collapse state / controls
- Recipe tool checkbox 由原版 `ReadToolONStatus()` 填入

因此下列做法不是正式產品路線：

- 自己重畫白色 `Recovered Tool Workspace`
- 用 generic property grid 代替原版 Tool dialog
- 把 Annotation/Agent UI 當 InspTools 二次解譯正式入口
- 以 ReferenceHost / DIA-like / Shunt-like demo 當最終 UI 本體

## Replica 的正確定位

一期 `InspToolsInspectionReplica.exe` 有兩種主要價值：

1. 直接啟動 rebuilt compiled ToolsWindow。
2. 提供 Recipe lifecycle、Runtime bootstrap、write guard、smoke、host-session 等驗證殼。

二期要做的是**把真正需要的 launcher/lifecycle 能力抽成乾淨 Standalone**，不是把 Replica 的整套 CLI/Pipe/測試架構照搬，也不是重做 UI。

## Binary 基準警告

一期交接曾記錄 accepted rebuilt DLL：

`0B4E8C671779B8F33290F6101C5D69105D3182C60BCE30D8B544725183A9F1F2`

但 2026-09-15 現場重新檢查後，該 accepted binary 已不在一期主目錄；目前 canonical source / Replica bin 的 `InspTools.dll` 為：

`85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1`

目前 Replica EXE 亦已不同於一期交接當時的 accepted hash。

因此：

- 不得宣稱目前磁碟上的 `85C4...` 就是一期 immutable accepted binary。
- 一期**行為/介面/生命週期 evidence 仍成立**，因為 source 與 UI-13 報告存在且現場 compiled UI 可再次叫出。
- 二期正式 build 需 pin 目前採用的 recovered UI DLL hash；若一期 source 再變更，build 必須 fail closed，而不是靜默換版。
- 若之後找回 `0B4E...` accepted binary，再另開 Gate 比對並決定是否切回。

## 二期可以繼承的內容

保留：

- decoded/rebuilt `InspTools.ToolsWindow`
- original AOI child dialogs
- UI-13 lifecycle
- Runtime dependency staging 經驗
- Recipe / Tool / Model ownership evidence
- production read-only / isolated write guard 原則

不要重新開發：

- 另一套 InspTools 主介面
- 另一套 Tool editor
- 另一套標註產品
- 另一套 Agent protocol

## 本輪 Gate

一期基準確認完成後，二期正式路線只允許：

`InspToolsStandalone.exe -> rebuilt InspTools.ToolsWindow`

零參數必須可以直接看到原版介面；Recipe / image / model loading 是後續沿用一期 lifecycle 的附加能力，不是啟動前置條件。

