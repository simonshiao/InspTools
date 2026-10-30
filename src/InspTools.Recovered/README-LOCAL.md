# 本機重建來源

此目錄由一期 `InspTools-1.3.16.28-Editable` 封存來源完整複製而來；複製後逐檔比對 199 個檔案的 SHA-256 均一致。封存來源保留不變。

- `InspTools.unpacked.csproj` 的 `TemplateRuntimeRoot` 預設改為本目錄的 `Runtime`，可使用同名 MSBuild 屬性覆寫。
- `Runtime` 的 DLL 來自本機 `output/InspToolsV2`；DLL 被此儲存庫的 `.gitignore` 忽略，僅提交程式碼不足以重現建置，須另外保管或準備相同相依檔案。
- `InspTools.g.resources` 是從 `release/current/InspTools.dll`（SHA-256 `85C4D87E59FB42520DE89B8C2AC04351E4701B809F21F8302F6266F4D96071D1`）擷取的 WPF 編譯資源；封存來源內並無此檔。
- 建置：`dotnet build src/InspTools.Recovered/InspTools.unpacked.csproj -c Release -p:Platform=x64`。
- 這份來源是封存基準，`LocateTool.SetBuffers` 仍包含原本的 AI 初始化。既有 R3 DLL 使用延遲初始化修補；本目錄建置出的 DLL 不等同已驗證的 R3 DLL，不可直接替換現行部署。
- BAML 是已編譯的 WPF 介面資源；本目錄的視窗 C# 可編輯，並不表示所有介面已有可直接編輯的 XAML。
