# DIA 檢測執行環境｜Decoded InspTools Runtime

目前正式入口只有：

`InspToolsStandalone.exe`

這個交付包只做一件事：**獨立直接建立一期 decoded/rebuilt `InspTools.ToolsWindow`，顯示原版 `INSPECTION TOOLS` 介面。**

## 目前 Gate

- 不需要先開 DIA。
- 不需要先開 Shunt。
- 不需要 `InspToolsInspectionReplica.exe`。
- 不需要 `ReferenceHost.exe`。
- 不需要先選 Recipe。
- 不出現二期自行重畫的白色 Generic Editor。
- 不把 Annotation / Agent / SAM 當成本案正式入口。

## 目前交付邊界

Base bundle 只帶起原版 UI 所需的最小 managed/native dependency；不包含完整 Training、CUDA、相機、PLC 或整套 DIA/Shunt 程式。

Recipe / Model / 真正檢測 Runtime 尚未納入目前 R1 base Gate。下一階段若加入 Recipe，只允許沿用一期已驗證的 compiled Recipe lifecycle，不重新設計一套 RuntimeRoot / ModelRoot / Station 表單。

## 一期基準

請先讀：

- `PHASE1_BASELINE_AUDIT.md`
- `ROUTE_RESET.md`

目前 build pin 的 recovered UI DLL SHA256：

`85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1`

一期歷史交接記錄的 accepted DLL SHA256 為：

`0B4E8C671779B8F33290F6101C5D69105D3182C60BCE30D8B544725183A9F1F2`

但該 accepted binary 目前未在一期專案樹中找到，所以不能把現行 `85C4...` 冒充成 immutable accepted binary；現行 build 會鎖 hash，任何來源漂移都先 fail closed、重新稽核。
