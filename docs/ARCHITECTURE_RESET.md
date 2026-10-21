# InspTools 二期｜架構重置與失敗經驗

> **歷史文件／已被 2026-09-15 Route Reset supersede。**  
> 本文記錄先前 machine-first / Core / SDK / WPF 架構探索，仍可作失敗經驗參考，但不再代表目前正式產品路線。現在先讀 `ROUTE_RESET_20260915.md` 與 `PHASE1_BASELINE_AUDIT_20260915.md`。

## 為什麼要重開二期

第一期逆向本身成功，但「可用於其他機台」的拆層不夠好。ShuntVC 實際套用後，為了使用 InspTools GUI/Runtime，需要理解或承擔太多 InspTools 專案內部細節，因此移植成本仍高。

這代表第一期的 Gate 偏向：

> 能恢復、能載入、能 patch、能 rollback、能 preview。

而二期 Gate 必須改成：

> 一般機台工程師只看 SDK 文件與幾個 C# 類別，就能接進既有 WPF 主程式。

## 第一期間接暴露給 Host 的複雜度

從現有 source 可確認舊整合有以下不適合作為產品主契約的特徵：

1. `InspToolsInspectionReplica.exe --host-session` 以 process/CLI 作 GUI 主入口。
2. `BackendPipeService` 使用 Named Pipe 承接 Inspect，適合隔離，但不適合同 process 機台 GUI 的預設方式。
3. `Stage.ps1` 建 fresh AppBase，複製 Recipe、PublicDB、Runtime、Model、XAML 與大量 DLL；適合作 Gate，不應成為每台機器整合的必要步驟。
4. `ReplicaLoader` / loader fallback 曾綁定 DIA Template / editable source 的絕對路徑。
5. `ReplicaRuntimeAdapter` 有固定 Recipe 預設與 GUI 專案內部 transaction 管理；外部 Host 很難只靠一個乾淨 SDK 理解生命週期。
6. Replica 編譯曾以 net481 Reference Assemblies 建 Host，而核心 DLL 為 net472；二期需把機台相容性放前面。

## 新的 public surface 草案

名稱仍可調整，但責任邊界先固定：

```csharp
public sealed class InspToolsRuntimeOptions
{
    public string RuntimeRoot { get; set; }
    public string RecipeRoot { get; set; }
    public string ModelRoot { get; set; }
    public bool EnableAi { get; set; }
}

public sealed class InspToolsSessionOptions
{
    public string RecipeName { get; set; }
    public int Station { get; set; }
    public string ImagePath { get; set; }
    public bool Editable { get; set; }
}

public interface IInspToolsRuntime : IDisposable
{
    IInspToolsSession OpenSession(InspToolsSessionOptions options);
}

public interface IInspToolsSession : IDisposable
{
    int Station { get; }
    string RecipeName { get; }
    void SetImage(string path);
    InspectionResult Inspect();
    ToolPreviewResult TestTool(string toolId);
    void Save();
    void Cancel();
}
```

WPF 層應再提供：

```csharp
public sealed class InspToolsEditorControl : UserControl
{
    public IInspToolsSession Session { get; set; }
}

public static class InspToolsEditor
{
    public static bool? ShowDialog(Window owner, IInspToolsSession session);
}
```

這樣機台端只知道 Runtime/Session/Editor，不知道 ParamFile、CVInspection、Binder、XAML sanitizer 或 transaction file。

## 編輯生命週期

正式語意應是：

```text
OpenSession(editable=true)
  -> SDK 建立內部 snapshot / transaction context
  -> GUI 多次修改 / Test
  -> Save()   => commit
     或
     Cancel() / Dispose without Save => rollback
```

Write guard 若仍需存在，只能留在 Core 內部，不可以要求 Host 工程師自己建立 marker 檔。

## Runtime 原則

機台正式 AppBase 本來就會有自己的 Runtime。二期 SDK 應接受 `RuntimeRoot`，並檢查/回報缺件；不應要求每次開 GUI 先複製一份 Runtime。

必要 native 相依仍可保持 locality，但需有 machine-readable manifest / preflight：

```text
CheckRuntime()
  -> ManagedDependencies
  -> NativeDependencies
  -> CUDA/TensorRT/DeltaAI capability
  -> Recipe/Model availability
```

## 二期拆解

### IR2-01｜Machine-First Contract

先完成 API/生命週期/Runtime contract 與 ReferenceHost，禁止一開始就重做全部 Tool UI。

### IR2-02｜Recovered Core 正規化

把第一期已解出的 Recipe、Inspection、Tool、Model 行為整理成 internal Core，消除 UI/Host 對 recovered object graph 的依賴。

### IR2-03｜Embeddable WPF GUI

將檢測設定做成可直接 reference 的 WPF control/window library；每個 Tool editor 綁 SDK session，而不是另啟 EXE。

### IR2-04｜Tool Parity / Test

依工具族逐一收斂 scalar、ROI、Mask bitmap、Pattern DB、OCR 與 Test/Preview。每項只在有 parity evidence 時開 write。

### IR2-05｜Machine Integration Gate

至少建立兩個極薄 ReferenceHost：DIA-like、Shunt-like。Host 只准使用 public SDK；若需要碰內部檔案/CLI/Pipe 才能工作，就判 Gate FAIL。

## 成功判定

二期不以「解出多少類別」判成功，而以「新機台整合成本」判成功。

核心 KPI：

- Host 新增 InspTools 設定入口所需自有程式碼越少越好。
- 不需要額外 InspTools process 即可用完整 GUI。
- 不需要複製另一份大型 Runtime。
- 不需要知道舊逆向專案路徑。
- Recipe 編輯可取消、可回復、可測試。
- 同一套 SDK 能同時服務 DIA-like 與 Shunt-like host。

