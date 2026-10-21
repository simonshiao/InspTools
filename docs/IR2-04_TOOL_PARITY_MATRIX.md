# IR2-04 Tool Parity Matrix

更新：2026-09-15  
狀態：ACCEPTED / DONE 100%（ProjectHub IR2-04）

## 目的

本矩陣把第一期 `InspToolsRebuild` 已驗證的 recovered evidence，轉成二期可直接執行的 Tool 編輯與 Test/Preview 邊界。

這不是重新逆向，也不代表 17 個原廠 Dialog 已全部 1:1 完成。二期只把 **有 parity evidence 的能力** 拉進 `InspTools.Core / InspTools.SDK / InspTools.Wpf`；沒有證據的 complex asset lifecycle 必須保持唯讀或 UNKNOWN。

## Evidence source of truth

- `D:\delta\專案進度\SharedKnowledge\InspToolsAPI\tool-parameters.json`
- `D:\delta\專案進度\SharedKnowledge\InspToolsAPI\stable-api.json`
- `D:\delta\專案進度\SharedKnowledge\InspToolsGUI\NON_AI_DIALOG_CALLBACK_CAPABILITY_20260913.md`
- `D:\delta\專案進度\Projects\InspToolsRebuild\Replica\InspToolsInspectionReplica\LocateDialogBinder.cs`
- `D:\delta\專案進度\Projects\InspToolsRebuild\Replica\InspToolsInspectionReplica\BinaryDialogBinder.cs`
- `D:\delta\專案進度\Projects\InspToolsRebuild\Replica\InspToolsInspectionReplica\MaskDialogBinder.cs`
- `D:\delta\專案進度\Projects\InspToolsRebuild\Replica\InspToolsInspectionReplica\MatchDialogBinder.cs`

## Parity matrix

| Slice | Recovered owner / section | 一期證據 | 二期寫入政策 | Test / Preview 政策 | IR2-04 決策 |
|---|---|---|---|---|---|
| Locate | `locatewindow.xaml` / `[Locate]` | `read_write_scalar_verified`, `verified_runtime`; binder 明確綁 10 個 scalar keys | **ALLOWLIST scalar only**；production Recipe 仍 read-only | 一期一般 Dialog Test 走 real non-AI `InspectFile`; 二期尚未移植 recovered execution | 先做 tool-specific WPF slice；不得把參數預覽稱為 AOI Test |
| Binary | `binarywindow.xaml` / `BinaryTools0..29` | `read_write_scalar_verified`, `verified_runtime`; binder 有 tracker / threshold / blob / rule 等可表達 scalar binding | **ALLOWLIST scalar only**；production Recipe 仍 read-only | 一期一般 Dialog Test 走 real non-AI inspection + `ToolResults` | 先做 tool-specific WPF slice；保留 raw viewer fallback |
| ROI | 多個 Tool 的 recovered rect/ROI object | `complex_roi_pattern_ocr = read_only_until_parity_evidence` | **READ_ONLY / NO PRODUCTION WRITE** | 只可顯示已知 geometry snapshot；不可偽造 editor lifecycle | IR2-04 不開 complex ROI write |
| Mask scalar | `maskwindow.xaml` / `MaskTools0..29` | `scalar_read_write_verified_bitmap_roi_read_only`, `verified_runtime`; binder 有 threshold / morphology scalar allowlist | **ALLOWLIST scalar only** | 一期一般 Dialog Test 可安全 non-AI inspection | scalar 可進下一 slice；bitmap/ROI 不可寫 |
| Mask bitmap / ROI | `maskwindow.xaml` complex asset | 第一期間明確標 `bitmap_roi_read_only` | **READ_ONLY** | 無完整 editor lifecycle parity | 保持保護狀態 |
| Match safe scalar | `matchtoolwindow.xaml` / `MatchTools0..19` | `safe_scalar_read_write_verified_pattern_db_roi_ocr_read_only`; binder Capture 明確列 safe scalar | **ALLOWLIST scalar only** | 一般 non-AI inspection 可 Test | 可顯示/後續接 scalar；不得外推 Pattern/OCR lifecycle |
| Pattern DB | `PatternDB0..19` + Match preview | 一期可讀 DB / preview；`MatchDialogBinder` 明確禁止 public DB / DB mutation 推論 | **READ_ONLY** | 可 preview 既有 pattern；不得新增/刪除/訓練/commit | 本輪 production write CLOSED |
| Font / OCR | `fonttoolwindow.xaml` / `FontTool` | `READ_ONLY+TEST`; 字庫 / Training / Pattern lifecycle 未開放寫入 | **READ_ONLY** | 一期可跑安全 non-AI Test，但不是 OCR DB editor parity | 本輪 production write CLOSED |
| Advanced Locate preview | `advlocatewindow.xaml`; `VLocateTools2..19`, `HLocateTools2..19`, `AdvLocateTools1..19`, `AdvMeasure0..39` | `InspectToolPreview` verified runtime；V=182 / H=148 / AdvMeasure=1243.55883 examples | 只沿用各自已驗 scalar 範圍 | **isolated ToolPreview verified in phase 1** | 後續 true Preview port 的第一優先 evidence |
| General Test | 多數 non-AI Dialog | 一期 `InspectFile` + `ToolResults` 已驗證 | 無 Recipe production write | 真實 state / Bin / Error；未啟用允許 `NOTUSED` / `NO_TOOL_RESULT` | 二期未接回前 `Inspect()` 必須維持 `NOT_RUN` |

## Locate allowlist

第一期 `LocateDialogBinder` 已驗證以下 scalar ownership：

`LocateAngleLimit`, `LocateWidthMin`, `LocateWidthMax`, `LocateHeightMin`, `LocateHeightMax`, `LocateErrorBin`, `LocateSizeErrorBin`, `ParetoIndex`, `LocateSpeedUp`, `doRotateMinAngle`。

IR2-04 專用 Locate editor 只把以上欄位當成 recovered tool-specific controls。其他 `[Locate]` keys 保留 raw viewer，不因存在於 INI 就自動升格成 1:1 UI parity。

## Binary allowlist policy

`BinaryDialogBinder` 已能 round-trip 的欄位範圍包括：

- tracker / enable / error bin / pareto / mask selection
- Tracker1/2 auto-size 與 alignment scalar
- THD1..4 channel / enable / min / max / DE scalar
- blob line / line width scalar
- inspection rule scalar
- multi-blob rule scalar
- font-mask / select-face / pixel-value 等安全 flags

二期可以依這個 recovered binder 範圍建立專用控制；沒有出現在 binder representable set 的 Binary key 不應只因 raw INI 可見就宣稱 UI parity。

## 永久 Gate

1. `InspToolsStandalone.exe` 必須在 DIA / Shunt / Replica / ReferenceHost 都未執行時無參數獨立啟動。
2. 同一套 `InspTools.Wpf` 必須可由 Machine Host same-process embed。
3. `D:\delta\ShuntSensor\外觀機\Runtime\ShuntVC\Recipe\3923-R002\station1.ini` 在 IR2-04 前後 SHA256 必須保持：
   `153650676BC2E1901620F1077D8AA0A00C654699B9443CB366F3555A9D539E4C`。
4. recovered execution 尚未接入二期前，`Inspect()` 固定回報 `NOT_RUN`；參數 preview 不得冒充 AOI Test。
5. Pattern DB、Mask bitmap/complex ROI、Font/OCR DB/model/training 無新 parity evidence 時一律不開 production write。

