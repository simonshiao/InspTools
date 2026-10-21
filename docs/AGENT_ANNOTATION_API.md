# InspTools AI 標註 Agent API

本文件是 `InspTools.Agent.dll` 的 transport-neutral 使用入口。目的不是再建立一個 InspTools EXE、CLI 或 Named Pipe，而是讓 Chao8Side、MCP、HTTP 或其他 Agent Host 共用同一套標註 command contract。

正式 machine-readable contract：`AgentAnnotation.contract.json`。

## 邊界

正式呼叫鏈：

```text
MCP / Chao8Side / HTTP / Pipe wrapper
        |
        | one JSON request / one JSON response
        v
AnnotationAgentJsonDispatcher
        |
        v
AnnotationAgentController
        |
        v
IAnnotationSession
        |
        +--> Annotation Core / Legacy X-Y
        |
        +--> optional IAnnotationSuggestionBackend
                  |
                  +--> InspTools.SamOnnx.dll
```

Transport 不應知道 `AIMarkWindow`、`CVInspection`、`OpenCvSharp.Mat`、ONNX tensor、embedding、write guard 或 recovered object graph。

## 最小流程

開啟 session：

```json
{
  "action": "annotation.open",
  "trainingImageRoot": "D:\\...\\TrainingImage",
  "modelName": "R002-CCD1",
  "kind": "Ng",
  "imagePath": "D:\\...\\X\\NG\\ALL\\sample.png",
  "editable": true
}
```

如果 `labelPath` 不提供，Core 依「同 Model + 同 OK/NG + 同 filename」尋找既有 Y；X/Y caption 子資料夾不需要一致。

加入 Box：

```json
{"action":"annotation.sam.box","x":10,"y":16,"x2":151,"y2":684}
```

取得 suggestion：

```json
{"action":"annotation.suggestion.get"}
```

JSON 只回 `score / positivePixels / width / height` 等摘要。原始 float mask 保留於 session，避免 MCP/HTTP 傳輸數百萬個浮點數。

Commit：

```json
{
  "action":"annotation.suggestion.commit",
  "classId":1,
  "mergeMode":"FillEmptyOnly"
}
```

Commit 只改記憶體 working label，不寫 disk。需要寫 Legacy TrainingImage 時才明確呼叫：

```json
{"action":"annotation.legacy.save"}
```

不要把 `suggestion.commit` 與 `legacy.save` 合併成一個 MCP tool。

既有 Legacy pair 的 save target 有明確 ownership：若目前 X/Y 已位在
`TrainingImage/<Model>/X|Y/<OK|NG>/...` 下，而且 caller 沒有明確指定新的
`XSubfolder/YSubfolder`，`annotation.legacy.save` 必須寫回目前 session 已載入的
X/Y 路徑。這包含 `X/NG/ALL` 對 `Y/NG/ALL`、以及 X/Y caption 子資料夾不同的既有資料。
只有新建 pair，或 caller 明確指定新的 subfolder 時，writer 才重新組 output path。

## Prompt 命令

- `annotation.sam.point.add`：positive point。
- `annotation.sam.point.remove`：negative point。
- `annotation.sam.box`：box prompt。
- `annotation.prompts.clear`：清目前 prompt。
- `annotation.suggestion.get`：以目前 prompt Decode；同 image 會使用 cached embedding。

`annotation.open`、`annotation.image.set`、`annotation.cancel`、`annotation.close` 都會清 transport 層 pending prompts。

## Experimental Auto Proposal

`annotation.auto.propose` 是**唯讀研究能力**，不是自動標註 commit。

它把 recovered `SAMAutoMask` 的實際可確認部分收斂成：

```text
4 x 4 positive-point grid
  -> 16 Decoder calls using one cached embedding
  -> 4 masks / point = 64 candidates
  -> pred IoU > 0.88
  -> stability > 0.95
```

JSON 只回候選的 score / stability / positivePixels / coverageRatio / box / rejectedBySafety；
不回 dense mask，也沒有 `annotation.auto.commit`。

安全層預設 `maxCoverageRatio=0.75`。這是二期 Agent safety，不是 recovered vendor 行為。
代表性 Shunt NG 圖上，recovered filter 留下 6 個候選，但 coverage 全在約 91.3%~91.6%，
因此 6 個全部 `rejectedBySafety=true`，`safeProposalCount=0`。

這項能力刻意保持 experimental，原因：

- recovered `AIMarkWindow` 只建立 `SAMAutoMask` 並注入 embedding/SAM，沒有找到 `Generate()` consumer；
- `box_nms_thresh / crop_nms_thresh / min_mask_region_area / output_mode` 只有欄位，沒有實際 postprocess；
- recovered CLIP prototype 沒有 caller，而且主要 Runtime/證據根找不到 `textual.onnx / visual.onnx`。

因此 Agent 可以拿 proposal 做研究或後續再排序，但不得把這條描述為原廠 AutoMask parity，也不得自動寫 Y。

## Agent 不需要搬大資料

`annotation.class_map.get` 在 JSON transport 預設只回：

```text
width / height
backgroundPixels
classPixelCounts[0..6]
```

完整 `byte[] ClassIds` 仍可從 C# in-process `AnnotationAgentController.GetClassMap()` 或 SDK 取得。這是刻意設計：Agent 做決策通常需要統計與狀態，而不是把整張 label 透過 MCP 往返。

## 錯誤與 fail-closed

- 沒有 SAM backend：`suggestion.get` 回 `ok=false`，Legacy X/Y / erase / validate / save 仍可用。
- 0 prompt：`suggestion.get` 直接回明確錯誤，不送空 tensor 到 Decoder。
- 模型缺失：SAM backend `IsReady=false`，不拖垮 Annotation Core。
- CUDA provider 載入失敗：若允許 fallback，狀態回 CPU fallback；完整 provider 錯誤保留於診斷狀態。
- `image.set` 遇到未儲存 working change 會拒絕切圖。

## 寫入 Gate

目前唯一會真正寫 X/Y 的 command 是 `annotation.legacy.save`。本 API contract 本身**不授權 production write**。正式機台若要開寫，Host/MCP 還需要自己的白名單、Recipe/session ownership 與人工/流程 Gate。

## 已驗收基準

Shunt 真實 `R002-CCD1`：

```text
existing Class1 = 40059
Box = [10,16]-[151,684]
SAM FirstMaskLegacy positive = 11961
score = 0.748690
FillEmptyOnly commit Class1 = 51902
Cancel -> Class1 = 40059
```

驗收程式：

```text
test/AnnotationJsonDispatcherSmoke.cs
test/AnnotationJsonDispatcherSamSmoke.cs
```

正式 Chao MCP `127.0.0.1:1111` 亦已驗證 18 個 `annotation_*` tools 實際載入；
live Agent 完整跑過 `open -> box -> suggest -> commit -> cancel`，並另外在 temp copy
跑過 `commit -> legacy.save -> close -> reopen`。跨子資料夾 Y 寫回 loaded label、重新讀回
class map 與「不得在 X/NG、Y/NG 根層產生 duplicate」均 PASS；Shunt 原始 Y SHA 保持不變。

因此 Chao8Side/MCP 後續只應沿用既有 transport contract，不應再重新解析標註、SAM 或 Legacy X/Y。

`annotation.auto.propose` 加入後的 MCP source 已由 12439 sidecar 驗收：55 tools total / 19 annotation tools，
其中 AutoProposal=`16/64/6/0`。目前 live 1111 是先前 startup catalog，仍只有原 18 個 annotation MCP tool；
等下一次受控 restart 才會出現第 19 個 `annotation_auto_propose`，不為 experimental tool 強制中斷 live MCP。
