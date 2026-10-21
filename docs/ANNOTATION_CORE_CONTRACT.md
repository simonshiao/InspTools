# InspTools 二期｜AI 標註 Core 契約

> 狀態：Recovered contract / IR2-03、IR2-04 的 architecture input  
> 日期：2026-09-15  
> 來源：InspTools 1.3.16.28 原始反編譯 + 第一期 editable source + AITrainingWIndow 訓練入口交叉驗證

## 1. 結論

AI 標註不是 Chao8Side 主程式自己的功能，也不是另一支獨立標註 EXE。

原始責任鏈如下：

```text
ToolsWindow
  -> AIMarkWindow
      -> SAMViewer.SAM
          -> encoder.onnx / decoder.onnx
      -> TrainingImage/<Model>/X|Y/OK|NG
```

二期應把它重構為：

```text
Machine Host / Agent / WPF
             |
             v
        InspTools.SDK
             |
             v
      AnnotationSession
       |            |
       v            v
 AnnotationStore   SamAnnotationEngine
       |            |
       v            v
 Legacy X/Y      ONNX Runtime
```

Machine Host 與 Agent 不得直接依賴 `AIMarkWindow`、`CVInspection`、`OpenCvSharp.Mat`、`SAMViewer.SAM`。

## 2. Legacy TrainingImage 契約

### 2.1 模型資料夾

`ToolsWindow.Btn_DrawMask_Click()` 由 AI Enhance 模型名稱去副檔名後建立資料夾：

```text
TrainingImage/<ModelStem>/
  X/
    OK/
    NG/
  Y/
    OK/
    NG/
  Origional/
    <Station>/
      OK/
      NG/
```

`X` 是模型實際輸入影像，`Y` 是同名標註影像；`Origional` 保留較上游的原始 station 影像。

### 2.2 NG 標註寫檔

`AIMarkWindow.SetNGMarkedFolder(xNgPath, yNgPath)` 的實際欄位語意：

```text
NGFolder       = X/NG
NGMarkedFolder = Y/NG
```

`SaveAIMarkedImage()` 以同一個 timestamp 檔名寫入 X 與 Y：

```text
NG-<Station>-yyyy-MM-dd-HH-mm-ss.png
```

因此 X/Y pair 的主要 identity 是「相同檔名」，不是另一份 index database。

### 2.3 影像責任

Recovered source 中：

- `OrgBitmap`：`SetMat()` 傳入的 AI 使用影像，寫入 `X/NG`。
- `FinalMat`：標註合成結果，寫入 `Y/NG`。
- `OrgImage`：`SetOrgImage()` 傳入的較上游 station buffer，在非 CropedMode 時另存 `Origional/<station>/NG`。

二期 SDK 不應用模糊的 `OrgBitmap/OrgImage` 名稱；建議明確改成：

```text
ModelInputImage
SourceStationImage
TrainingLabelImage
```

## 3. Y 標註不是單純 binary mask

### 3.1 六色 class

標註器有 6 個可選類別；內部 `ColorID = SelectedIndex + 1`，所以 class id 為 1..6：

| Class ID | UI 色 | Legacy visual RGB |
|---:|---|---|
| 1 | Red | 255,0,0 |
| 2 | Lime/Green | 0,255,0 |
| 3 | Blue | 0,0,255 |
| 4 | Yellow | 255,255,0 |
| 5 | Fuchsia/Magenta | 255,0,255 |
| 6 | Aqua/Cyan | 0,255,255 |

`mDataListSave` 保存的是 class id，不是 RGB 值。

### 3.2 Ignore / Maybe 強度

Recovered source 明確存在：

```text
IgnoreColor = 20
MaybeColor  = 125
```

Ignore rectangle 依目前選定 class 色相，以約 125 強度寫入：

```text
125,0,0
0,125,0
0,0,125
125,125,0
125,0,125
0,125,125
```

Auto-ignore 則會使用 `20,20,20` 類型的暗灰值。

因此二期不得把 Legacy Y 在載入時先二值化，否則會破壞 class 與 ignore/maybe 語意。

### 3.3 訓練端佐證

`AITrainingWIndow` 寫給 DAVS 訓練 Runtime 的 `navs.ini` 包含：

```text
XPath      = ../../TrainingImage/<Model>/X/
YPath      = ../../TrainingImage/<Model>/Y/
Threshold  = 220
MaybeArea  = 0.3
NGIgnore   = 0
OKBlank    = 1
ModelType  = US
```

其中 `MaybeArea` 在 recovered UI 預設為 `0.3`。

這表示 Y 的像素強度與訓練 Runtime 之間存在 Legacy protocol；在完全驗證 train runtime 前，20/125/255 必須原樣保留。

## 4. SAM prompt 契約

### 4.1 Prompt label

Recovered `PointPromotion` / `BoxPromotion` 已能直接還原：

```text
Positive point = 1
Negative point = 0
Box left/top   = 2
Box right/btm  = 3
```

Agent 不需要建立 WPF `PointAnnotation` / `RectAnnotation`；SDK 應直接接受 domain prompt。

建議 public contract：

```csharp
public enum AnnotationPromptType
{
    PositivePoint,
    NegativePoint,
    Box
}

public sealed class AnnotationPrompt
{
    public AnnotationPromptType Type { get; set; }
    public float X1 { get; set; }
    public float Y1 { get; set; }
    public float X2 { get; set; }
    public float Y2 { get; set; }
}
```

### 4.2 Encoder input

`Transforms(1024)`：

- 保持長寬比，長邊 resize 到 1024。
- Point / Box 座標使用相同比例縮放。
- 影像轉 float。
- recovered 實作使用該張影像自己的 `MeanStdDev` 做 normalization。
- 打包為 CHW，未使用區域維持 zero padding。

Encoder tensor：

```text
name  = x
shape = [1, 3, 1024, 1024]
```

Encoder output 在 Decoder 端以：

```text
[1, 256, 64, 64]
```

使用。

### 4.3 Decoder input

Recovered Decoder contract：

```text
image_embeddings [1,256,64,64]
point_coords      [1,N,2]
point_labels      [1,N]
mask_input        [1,1,256,256] = 0
has_mask_input    [1]           = 0
orig_im_size      [2]           = [height,width]
```

目前 recovered path 沒有使用前一輪 mask 作 iterative mask prior；每次 prompt update 都以同一個 image embedding + 完整 prompt list 重新 Decode。

## 5. Embedding lifecycle

`AIMarkWindow.OpenAIModelAndImage()` 的真正 lifecycle：

```text
New image
  -> SAM.Encode(image)       // 一次
  -> cache mImgEmbedding
  -> Point/Box changes
       -> SAM.Decode(prompts, cached embedding) // 多次
```

所以二期 Core 必須把 embedding cache 放在 image/session scope，而不是每個 Agent command 都重新 Encode。

建議 invalidation key 至少包含：

```text
image identity/hash
image width/height
encoder model identity/hash
preprocess contract version
```

Prompt 改變不得使 embedding 失效。

## 6. SAM 模型路徑與 Runtime

原始 recovered contract 指向：

```text
C:\Temp\encoder.onnx
C:\Temp\decoder.onnx
```

曾於第一期/Shunt 實機驗證的模型：

```text
encoder.onnx
  size   = 99,827,815 bytes
  SHA256 = CF99A3B79EEA0AC4F7C9B06CDCE5B2763FF41A3F631213C748D497CBA233DF6A

decoder.onnx
  size   = 8,738,967 bytes
  SHA256 = D2118487645C5F2B5808FB89A02BC29E13021A1DEE8332C68E371C5AD2BE5676
```

二期產品不得再硬編碼 `C:\Temp`。目前正式實作為：

```csharp
public sealed class SamOnnxOptions
{
    public string EncoderModelPath { get; set; }
    public string DecoderModelPath { get; set; }
    public bool PreferCuda { get; set; }
    public int CudaDeviceId { get; set; }
    public SamMaskSelection MaskSelection { get; set; }
}
```

路徑由 `RuntimeRoot/ModelRoot` 或明確 options 注入。

正式 backend：

```text
InspTools.SDK.IAnnotationSuggestionBackend
    -> InspTools.SamOnnx.SamOnnxSuggestionBackend
```

`InspTools.Core.dll` 不 reference ONNX Runtime / OpenCV；SAM 是 optional plugin。沒有 SAM bundle 時 Legacy 標註 Core 仍可獨立運作。

2026-09-15 實證：

```text
Shunt R002-CCD1 / NG-1-2025-06-27-16-55-12-H.png
Box (10,16) -> (151,684)
Legacy first mask positive pixels = 11961
IoU = 0.748690
C# ORT 1.16 == Python ORT 1.25 reference : PASS
```

同圖 Box / Positive Point / Positive+Negative Point 共四次 Decode 時 `EncodeCount=1`，embedding cache PASS。

## 7. 原廠 recovered 行為 vs 第一期 patch

這點必須永久分開，避免把我們修過的 code 誤認成 vendor 原始 contract。

### 原始反編譯可確認

- `SAM.LoadONNXModel(bool UseCuda=true)`。
- hardcode `C:\Temp\encoder.onnx` / `decoder.onnx`。
- UseCuda 時直接 append CUDA provider。
- 模型存在才建立 InferenceSession。
- 沒有完整的 CUDA fail -> CPU fallback lifecycle。
- `AIMarkWindow` 每張影像 Encode 一次、prompt 多次 Decode。

### 第一期 editable source 後補

- `IsModelReady` / `LastLoadError`。
- load lock / lifecycle 防重入。
- CUDA session 建立失敗時清理 partial session。
- CPU fallback。
- 缺 encoder/decoder 時 optional skip，避免整個 GUI crash。
- `AIMarkWindow.SetSAM()` 額外做 readiness/load 保護。

二期可以保留這些修正概念，但應以正式 `SamRuntime` abstraction 重寫，而不是把 patch 原樣暴露給 Host。

## 8. Current suggestion 與 committed class mask

Recovered `SaveAITempDraw()` 名稱容易誤導；它並沒有寫檔。

真正作用：

```text
current SAM decoder mask (mDataList)
   -> 以目前 ColorID 寫進 persistent class map (mDataListSave)
   -> 只填目前為空的 pixel
   -> Reset current prompts/suggestion
```

第一筆 suggestion：

```text
mask > 0 -> class id
else     -> 0
```

後續 suggestion：

```text
if suggestion > 0 AND committed pixel <= 0
    committed pixel = class id
```

建議二期 API 名稱改為：

```text
CommitSuggestion(classId, mergeMode)
```

而不是 `SaveAITempDraw()`。

## 9. Binary export 是另一種用途

Recovered `OutPutAIDrawMat()`：

- 將 `mDataListSave > 0` 全部壓成單通道 255。
- 做 Morphology Open。
- 同步把被 open 移除的 class pixel 清掉。
- export 後會 clear `mDataListSave`。

所以它是「破壞性 binary export」，不是一般的 `GetMask()`。

二期必須把 API 分開：

```text
GetClassMap()            // 不丟 class，不改 state
GetLegacyLabelImage()    // 20/125/255 + 六色，兼容訓練
ExportBinaryMask()       // 明確 binary，必要時 morphology
```

Agent 預設不得呼叫 destructive export。

## 10. 建議二期 SDK Surface

```csharp
public interface IAnnotationSession : IDisposable
{
    AnnotationImageInfo Image { get; }
    bool SamReady { get; }

    void SetImage(string path);
    AnnotationSuggestion Suggest(IReadOnlyList<AnnotationPrompt> prompts);

    void CommitSuggestion(int classId, AnnotationMergeMode mergeMode);
    void Erase(AnnotationRect rect);
    void ClearClass(int classId);
    void ClearAll();

    AnnotationClassMap GetClassMap();
    AnnotationLabelImage GetLegacyLabelImage();
    AnnotationBinaryMask ExportBinaryMask(AnnotationBinaryExportOptions options);

    AnnotationValidationResult ValidateLegacyPair();
    AnnotationSaveResult SaveLegacyTrainingPair();
    void Cancel();
}
```

建議 merge mode：

```text
FillEmptyOnly  // recovered SaveAITempDraw parity
Overwrite
UnionSameClass
ReplaceRegion
```

IR2 parity Gate 先只開 `FillEmptyOnly`；其餘模式有測試後再開。

## 11. Agent Surface

建議 MCP/Agent 層只包 SDK，不碰 WPF：

```text
annotation.open
annotation.image.set
annotation.sam.status
annotation.sam.point.add
annotation.sam.point.remove
annotation.sam.box
annotation.suggestion.get
annotation.suggestion.commit
annotation.erase
annotation.class_map.get
annotation.legacy.validate
annotation.legacy.save
annotation.cancel
```

典型 Agent 工作流：

```text
open image
  -> encode/cache embedding
  -> box or positive point
  -> inspect suggestion score/mask
  -> optional negative points
  -> commit to class N
  -> repeat
  -> validate X/Y pair
  -> save
```

## 12. 二期 Gate

標註子系統完成至少需滿足：

1. Agent / ReferenceHost 不 reference `AIMarkWindow`。
2. Agent / ReferenceHost 不接觸 `OpenCvSharp.Mat` public API。
3. SAM 路徑可注入，無 production `C:\Temp` hardcode。
4. 同一張 image 多 prompt 不重跑 Encoder。
5. Legacy X/Y pair 可完全 round-trip：檔名、尺寸、class 色、ignore/maybe 強度不變。
   對既有 caption/subfolder pair，未指定新的 `XSubfolder/YSubfolder` 時 Save 必須寫回 loaded X/Y，
   不得在 `X/OK|NG` 或 `Y/OK|NG` 根層額外產生 duplicate。
6. `GetClassMap()` 不可有 destructive side effect。
7. binary export 與 Legacy training label export 分離。
8. CPU/CUDA capability 由 Runtime preflight 回報，不由 UI 猜。

## 13. 尚未宣告完成的部分

- DAVS/train.exe 對 20/125/255 的精確 loss/ignore 解讀仍在外部訓練 Runtime；目前只確認 InspTools 生產與 navs.ini contract。
- `Y/OK` 已由 DIA / Shunt 真實 TrainingImage 抽樣確認為純黑，且二期 `SaveLegacyTrainingPair()` 已通過新建 X + blank-Y save parity smoke。
- 既有 `X/NG/ALL + Y/NG/ALL` pair 已補 Save round-trip regression：loaded Y 原地更新、reopen 後 class map 持久化、根層無 duplicate；這同時修正 Agent `annotation.legacy.save` 在 caption subfolder dataset 的舊 target 漂移問題。
- 六色 class 在特定模型是否代表 6 個獨立語意類別，還是同一 NG 類別的 caption 分類，需由現有 Recipe/Training sample 再驗證。
- AutoMask recovered point-grid / IoU / stability 部分已納入 optional `IAnnotationAutoProposalBackend` 與 `annotation.auto.propose`，但只允許 read-only proposal metadata；沒有 dense mask 回傳、沒有 auto commit/save。這不是 vendor UI parity：`AIMarkWindow` 未找到 `Generate()` consumer，NMS/region fields 亦未實作使用。
- CUDA provider 完整 bundle 尚未驗收；目前已確認 PreferCuda 失敗可保留底層 provider error 並安全 fallback CPU。

## 14. 真實 TrainingImage 資料驗證

2026-09-15 已把 recovered source contract 與 DIA / Shunt 真實資料交叉驗證，正式證據：

```text
output/ANNOTATION_DATASET_AUDIT_20260915.md
test/audit_legacy_annotation_labels.py
```

關鍵結論：

1. DIA：`Y/OK=3514`、`Y/NG=1806`；依「同 Model + 同 OK/NG + 同 filename」可配 3526 組，另有 1794 個歷史 Y-only orphan，已確認不是尺寸 mismatch 或 ambiguous pair。
2. Shunt：`Y/OK=356`、`Y/NG=711`，1067/1067 全部 X/Y 成對且尺寸一致，可作乾淨 baseline。
3. Y/OK 抽樣均為純 `(0,0,0)`。
4. 真實 Y/NG 已命中 `(20,20,20)` ignore、125 maybe 與 255 full-class；Shunt 樣本實際命中 Red / Green / Blue 三個 class family。
5. X/Y caption 子資料夾可以不同：`SaveFolderWindow` 本來就能分別選 pathY/pathX；因此 pair contract 是 filename identity，不是完整相對路徑 identity。
6. `SaveAIMarkedImage()` 會 `Resize()` IgnoreMat / DrawingMat，真實 label 邊界會產生 254/253/73 等 interpolation 色；production Gate 不可硬限制 palette 只能是 `{0,20,125,255}`。

二期 `SaveLegacyTrainingPair()` 對「新寫入資料」仍必須 strict：永遠產生 pair、尺寸一致、OK 也要有 blank Y；DIA 既有 1794 orphan 只列 legacy debt，不應反過來放寬新 writer contract。

