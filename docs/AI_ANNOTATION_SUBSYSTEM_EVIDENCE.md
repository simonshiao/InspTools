# InspTools AI 標註子系統｜二期解譯證據與 Agent 邊界

日期：2026-09-14  
狀態：Evidence / Architecture Input  
用途：提供 InspTools-Rebuild2 後續 Core / SDK / WPF / Agent 化時使用。這份文件描述已確認的舊版行為，不代表二期 public API 已完成。

## 結論

目前可確認：DIA / Chao8Side 看到的 AI 標註功能，其主要 owner 不是 Host 主程式，也不是另一支獨立標註 EXE。

舊版實際責任鏈如下：

```text
Machine Host / DIA / Chao8Side
        |
        v
InspTools.ToolsWindow
        |
        v
InspTools.Other.AIMarkWindow
        |
        +-- 手動畫筆 / 擦除 / Mask 編輯
        +-- AI 標註影像讀寫
        +-- Inspection / Station context
        +-- SAM Point / Box 輔助標註
        |
        v
SAMViewer.SAM + SAMViewer/*Promotion
        |
        v
Microsoft.ML.OnnxRuntime
        |
        +-- SAM encoder ONNX
        +-- SAM decoder ONNX
```

因此二期應把這套能力視為 `InspTools` 的標註子系統，再從 recovered UI object graph 中抽出乾淨 Core / SDK contract；不要讓 Machine Host 或未來 Agent 直接操控 `AIMarkWindow` 內部欄位。

## 已確認的來源

第一期 editable evidence：

```text
D:\delta\專案進度\Projects\InspToolsRebuild\Source\InspTools-1.3.16.28-Editable
```

主要檔案：

```text
InspTools\ToolsWindow.cs
InspTools\Other\AIMarkWindow.cs
SAMViewer\SAM.cs
SAMViewer\SAMAutoMask.cs
SAMViewer\MaskData.cs
SAMViewer\Promotion.cs
SAMViewer\PointPromotion.cs
SAMViewer\BoxPromotion.cs
SAMViewer\MaskPromotion.cs
SAMViewer\PointAnnotation.cs
SAMViewer\RectAnnotation.cs
SAMViewer\Transforms.cs
SAMViewer\CLIP.cs
SAMViewer\SimpleTokenizer.cs
```

另有原始反編譯證據：

```text
D:\delta\專案進度\Other\InspTools-1.3.16.28-Decompiled
```

二期目前只引用這些檔案作 evidence；未經 IR2-02 正規化前，不直接把整棵第一期 source tree 視為新產品架構。

## ToolsWindow -> AIMarkWindow 的直接證據

在第一期 editable `InspTools\ToolsWindow.cs` 可直接找到 `AIMarkWindow` 建立與注入流程。

2026-09-14 查核時的重要位置：

```text
6361  AIMarkWindow aIMarkWindow = new AIMarkWindow();
6362  SetInspection(...)
6364  SetSAM(mSam)
6367  SetOrgImage(...)
6368  SetMat(...)
6369  SetNGMarkedFolder(...)
6370  SetColorSelect(...)
6372  SetPenSize(...)
```

另有多組標註 / AI 流程重用同一個 `AIMarkWindow`，例如約 7247、7353、7935 行附近，包含：

```text
OpenAIModelAndImage(...)
SetSamAIRect(...)
OutPutAIDrawMat()
```

這證明 `ToolsWindow` 是舊版 UI orchestration / entry owner，而真正的標註視窗與大部分標註行為位於 `AIMarkWindow`。

## AIMarkWindow 已確認能力

第一期 editable：

```text
InspTools\Other\AIMarkWindow.cs
```

已確認 public / internal callable surface 包含：

```text
SetOrgImage(Mat)
SetMat(Mat, zoom)
SetInspection(CVInspection, stationIndex)
SetSAM(SAM)
SetNGMarkedFolder(ngPath, markPath)
SetColorSelect(...)
SetPenSize(...)
SetSamAIRect(...)
OpenAIModelAndImage(...)
OutPutAIDrawMat()
```

已確認內部保存行為包含：

```text
SaveAIMarkedImage()
SaveAITempDraw()
```

這些名稱可作為 IR2-02 拆分責任的證據，但禁止原樣全部升格為二期 public SDK。

原因：舊 surface 直接暴露 `CVInspection`、`OpenCvSharp.Mat`、StationIndex 與 UI state，違反二期 `Host 不理解 recovered type` 的原則。

## SAM 子系統

`SAMViewer\SAM.cs` 已確認直接持有 ONNX Runtime session：

```text
private InferenceSession mEncoder;
private InferenceSession mDecoder;
```

並提供：

```text
LoadONNXModel(bool UseCuda = true)
```

2026-09-14 查核：

```text
SAM.cs 約 90-91 行：CUDA path 建立 encoder / decoder InferenceSession
SAM.cs 約 117-118 行：CPU path 建立 encoder / decoder InferenceSession
```

`ToolsWindow.cs` 約 1167 行可看到：

```text
mSam.LoadONNXModel(GPUExist)
```

因此要區分：

- `SAMViewer/*.cs`：SAM 操作、prompt、decode 與整合邏輯，屬 InspTools recovered C#。
- `encoder/decoder *.onnx`：外部模型資產。
- `Microsoft.ML.OnnxRuntime` / CUDA provider：Runtime dependency。

換句話說，SAM 並不是一支獨立標註程式；它是由 InspTools C# 邏輯載入外部 ONNX 模型。

## 二期 Ownership 建議

二期不要以舊 class name 決定 public architecture，建議責任邊界固定為：

```text
InspTools.Core
  Annotation/
    AnnotationSession
    AnnotationDocument
    AnnotationMask
    AnnotationStore
    SamAnnotationEngine

InspTools.SDK
  IAnnotationSession
  AnnotationSessionOptions
  AnnotationImage
  AnnotationMaskResult
  AnnotationPrompt

InspTools.Wpf
  AnnotationEditorControl
  AnnotationEditorWindow

Machine Host / Agent
  只依賴 SDK contract
```

其中 recovered type mapping 可留在 Core internal：

```text
AIMarkWindow            -> WPF editor + Core annotation session
CVInspection            -> Core adapter，不暴露給 Host
OpenCvSharp.Mat         -> Core implementation detail 或受控 image adapter
SAMViewer.SAM           -> SamAnnotationEngine internal implementation
PointPromotion          -> Point prompt implementation
BoxPromotion            -> Box prompt implementation
SaveAIMarkedImage       -> AnnotationStore commit
SaveAITempDraw          -> working/snapshot persistence
```

## 未來 Agent 不應操作 GUI

Agent 化時，不應以滑鼠自動化 AIMarkWindow 作正式契約。

理想呼叫方式：

```text
Agent
  -> InspTools.SDK
      -> OpenAnnotationSession(...)
      -> SetImage(...)
      -> SuggestMask(PointPrompt / BoxPrompt)
      -> MergeMask(...)
      -> EraseMask(...)
      -> ValidatePair()
      -> Save()
```

建議的 Agent-safe API 草案：

```csharp
public interface IAnnotationSession : IDisposable
{
    AnnotationImageInfo Image { get; }
    AnnotationMaskResult SuggestPoint(int x, int y, bool foreground);
    AnnotationMaskResult SuggestBox(int x1, int y1, int x2, int y2);
    AnnotationMaskResult GetMask();
    void ApplyMask(AnnotationMaskResult mask);
    void EraseMask(AnnotationMaskResult mask);
    AnnotationValidationResult Validate();
    void Save();
    void Cancel();
}
```

這只是 contract direction，不代表二期已實作。

## Agent 工具層可以進一步包裝

SDK 穩定後，才在上層提供 Agent / MCP tool，例如：

```text
annotation.open
annotation.image.set
annotation.sam.point
annotation.sam.box
annotation.mask.get
annotation.mask.apply
annotation.mask.erase
annotation.validate
annotation.save
annotation.cancel
```

這樣未來 Agent 可以做：

```text
載入一批 NG 影像
-> SAM 自動提出 mask
-> 依規則計算可信度 / 面積 / 邊界
-> 高可信度自動接受
-> 低可信度送人工 AnnotationEditorControl 覆核
-> 保存 X/Y
-> 接續 Training workflow
```

Agent 層仍不得直接知道：

```text
CVInspection
ParamFile
SAMViewer.SAM
InferenceSession
AIMarkWindow
OpenCvSharp.Mat ownership
Recipe write guard
Replica Binder
ReferenceCache
```

## 與 IR2 主線的關係

這份證據不應打斷 IR2-01 Machine-First contract。

建議納入順序：

```text
IR2-01 先證明 Runtime / Session / WPF 同 process contract
IR2-02 正規化 Core 時，把 AIMarkWindow/SAM 的非 UI 行為拆入 Annotation Core
IR2-03 WPF 化時，將標註 GUI 改為只綁 IAnnotationSession
IR2-04 parity 階段補 Mask / AI annotation / SAM prompt 的實際 Gate
IR2-05 用 DIA-like / Shunt-like Host 驗證 Host 不碰 recovered type
```

若未來要新增專門工作卡，建議名稱使用「AI Annotation / SAM Core 正規化」，不要建立另一支獨立標註 EXE 專案。

## 下一輪解譯重點

後續針對標註子系統應優先確認：

1. `SaveAIMarkedImage()` 的 X/Y 實際檔名、色碼、尺寸、metadata 與覆寫規則。
2. `SaveAITempDraw()` 的 temporary/snapshot lifecycle。
3. Point / Box prompt 到 decoder input tensor 的完整轉換。
4. SAM encoder embedding 是否可跨多次 prompt 重用，以及切換影像時的 invalidate 條件。
5. CPU / CUDA fallback 與模型檔實際搜尋規則。
6. OK / NG / NGMarkedFolder 與 Training workspace 的真正 ownership。
7. `OutPutAIDrawMat()` 回傳的是 UI preview、training mask 還是兩者共用 representation。
8. 標註資料與 DIA Inspection / Locate / AIProphet 三條 Training 線的邊界是否一致。

完成上述查核後，才適合把標註 Core public contract 定稿。

