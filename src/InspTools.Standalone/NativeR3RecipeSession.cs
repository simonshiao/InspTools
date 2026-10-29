using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace InspTools.Standalone
{
    /// <summary>
    /// R3 experimental GUI session with actual CVInspection objects.
    /// Model load, native AI prediction, Inspect and vendor auto-save remain OFF.
    /// Only run with a pinned R3 DLL in an audited engineering sandbox
    /// or a separately packaged portable review directory.
    /// </summary>
    internal static class NativeR3RecipeSession
    {
        private const string PinnedLazyDllSha =
            "ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B";
        // Rebuilt on CHAO3 from SHA-pinned archived LocateTool.cs and R1 DLL.
        // The local compiler emitted a different binary SHA. This alternate
        // candidate requires its own explicit marker; never silently accept
        // an arbitrary rebuilt DLL as the audited CHAO2 R3 release.
        private const string Chao3LocalDllSha =
            "BC56633CD5DD37FAF4274181D9A0100B5A99EAB0D48BDBFDAFDABDFEE71C0223";
        private const string Chao3LocalMarker = "INSPTOOLS_CHAO3_LOCAL_R3_SHA_V1";
        private const string PortableMarker = "INSPTOOLS_PORTABLE_REVIEW_R3_V1";

        internal static bool IsPortableReview()
        {
            string marker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "portable-review.marker");
            if (!File.Exists(marker)) return false;
            try
            {
                string dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "InspTools.dll");
                if (!string.Equals(File.ReadAllText(marker).Trim(),
                    PortableMarker, StringComparison.Ordinal) || !File.Exists(dll))
                    return false;
                string sha = DllSha(dll);
                if (string.Equals(sha, PinnedLazyDllSha,
                    StringComparison.OrdinalIgnoreCase)) return true;
                string localMarker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "portable-chao3-local-r3.marker");
                return string.Equals(sha, Chao3LocalDllSha,
                    StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(localMarker) &&
                    string.Equals(File.ReadAllText(localMarker).Trim(),
                        Chao3LocalMarker, StringComparison.Ordinal);
            }
            catch { return false; }
        }

        private static string DllSha(string dll)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream file = File.OpenRead(dll))
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        }

        internal static void PrepareAiParityStartup()
        {
            if (!IsPortableReview()) return;
            string app = AppDomain.CurrentDomain.BaseDirectory;
            string marker = Path.Combine(app, "chao8side-ai-parity.marker");
            if (!File.Exists(marker)) return;
            if (File.ReadAllText(marker).Trim() != "INSPTOOLS_CHAO8SIDE_AI_PARITY_V1")
                throw new InvalidDataException("Unknown AI parity marker");
            string rootQr = Path.Combine(app, "ThoughtWorks.QRCode.dll");
            string deferred = Path.Combine(app, "AiDeferred", "ThoughtWorks.QRCode.dll");
            if (!File.Exists(rootQr)) return;
            if (!File.Exists(deferred) || DllSha(rootQr) != DllSha(deferred))
                throw new IOException("Unknown QR module in candidate root; refusing to change it");
            // Only delete our own identical staging copy in this isolated
            // candidate. Do not ever touch Chao8Side or production assets.
            // Prevent uninitialized original ToolsWindow.CheckAuth crash.
            File.Delete(rootQr);
        }

        internal static void Open(Window window, string recipeName, int stationNumber)
        {
            RequireIsolatedSandbox(recipeName);
            if (stationNumber < 1 || stationNumber > 8)
                throw new ArgumentOutOfRangeException(nameof(stationNumber));
            StandaloneRecipeStore.Validate(recipeName);

            Type windowType = window.GetType();
            Type stationType = windowType.Assembly.GetType(
                "InspTools.Station.CVInspection", true);
            MethodInfo readRecipe = stationType.GetMethod("SetRecipe",
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setStations = windowType.GetMethod("SetStation",
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo selectStation = windowType.GetMethod("SetStationIndex",
                BindingFlags.Instance | BindingFlags.Public,
                null, new[] { typeof(int) }, null);
            if (readRecipe == null || setStations == null || selectStation == null)
                throw new MissingMethodException("CVInspection/ToolsWindow R3 API 不完整");

            Array stations = Array.CreateInstance(stationType, 8);
            for (int index = 0; index < 8; index++)
            {
                object station = Activator.CreateInstance(stationType);
                object[] args = { recipeName, index, string.Empty };
                readRecipe.Invoke(station, args);
                if (!string.IsNullOrWhiteSpace(Convert.ToString(args[2])))
                    throw new InvalidDataException("S" + (index + 1) +
                        " Recipe 載入警告：" + Convert.ToString(args[2]));
                stations.SetValue(station, index);
            }
            setStations.Invoke(window, new object[] { stations });
            // 原生工具視窗依賴每站的 ThroughputFile；先前僅實驗模式初始化，
            // 造成一般 R3 載入 Recipe 後圖示看得到卻無法正常開窗。
            // 本方法已先完成 RequireIsolatedSandbox，且 Recipe 由 StandaloneRecipeStore 驗證。
            {
                MethodInfo throughput = windowType.GetMethod("SetThroughputRecipe",
                    BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
                if (throughput == null) throw new MissingMethodException("ToolsWindow.SetThroughputRecipe");
                throughput.Invoke(window, new object[] { recipeName });
                for (int i = 0; i < 8; i++)
                {
                    object station = stations.GetValue(i);
                    var param = stationType.GetField("paramFile").GetValue(station);
                    var field = param.GetType().GetField("ThroughputFile");
                    if (field == null || field.GetValue(param) == null)
                        throw new InvalidDataException("S" + (i + 1) + " Throughput 初始化未完成");
                }
            }
            selectStation.Invoke(window, new object[] { stationNumber - 1 });
            string aiStatus = TryInitializeChao8SideAi(window, stations, stationNumber);
            window.Title = "INSPECTION TOOLS - [R3 獨立版／真實八站／AI 未載入] " +
                recipeName + " / S" + stationNumber;
            if (!string.IsNullOrWhiteSpace(aiStatus))
                window.Title = "INSPECTION TOOLS - [R3 Chao8Side AI 對齊：" +
                    aiStatus + "] " + recipeName + " / S" + stationNumber;
        }

        // This opt-in gate is only unlocked by a model/runtime staging manifest
        // in an isolated candidate. Do not turn on a license flag, manufacture
        // Locate success, or start Inspect on behalf of the original application.
        private static string TryInitializeChao8SideAi(Window window, Array stations, int stationNumber)
        {
            string app = AppDomain.CurrentDomain.BaseDirectory;
            string marker = Path.Combine(app, "chao8side-ai-parity.marker");
            if (!IsPortableReview() || !File.Exists(marker) ||
                File.ReadAllText(marker).Trim() != "INSPTOOLS_CHAO8SIDE_AI_PARITY_V1")
                return "";
            var lines = new StringBuilder();
            lines.AppendLine("Chao8Side 1.3.16.28 / R3 native AI parity diagnostic");
            lines.AppendLine("Source models copied to isolated candidate; no machine I/O, training or Inspect.");
            string locateResult = "未檢查";
            string prophetResult = "未檢查";
            try
            {
                string[] dependencies = {
                    "cpsrt.dll", "DeltaAIProcessDll.dll", "OnnxRuntimeEnv.dll"
                };
                foreach (string name in dependencies)
                {
                    if (!File.Exists(Path.Combine(app, name)))
                        throw new FileNotFoundException("Chao8Side AI 相依缺失：" + name);
                }
                // Defer the original QR assembly until *after* Station[] is
                // initialized. If it is present during Window.Loaded, vendor
                // CheckAuth dereferences null Station and the process crashes.
                string qr = Path.Combine(app, "ThoughtWorks.QRCode.dll");
                if (!File.Exists(qr))
                {
                    string deferred = Path.Combine(app, "AiDeferred", "ThoughtWorks.QRCode.dll");
                    if (!File.Exists(deferred))
                        throw new FileNotFoundException("授權元件缺失，不能安全恢復原版 AI 檢查");
                    File.Copy(deferred, qr, false);
                }
                // Restore the vendor's authorization UI and original feature
                // permissions instead of bypassing CheckAuthNoDLG.
                var auth = window.GetType().GetMethod("CheckAuthNoDLG",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (auth == null) throw new MissingMethodException("ToolsWindow.CheckAuthNoDLG");
                auth.Invoke(window, null);

                object station = stations.GetValue(stationNumber - 1);
                Type type = station.GetType();
                object param = type.GetField("paramFile").GetValue(station);
                object loc = param.GetType().GetField("LocateParam").GetValue(param);
                bool wanted = (bool)loc.GetType().GetField("EnableAILocate").GetValue(loc);
                bool allowed = (bool)param.GetType().GetField("AllowEnableAILocate").GetValue(param);
                string model = Convert.ToString(loc.GetType().GetField("AILocateModelName").GetValue(loc));
                lines.AppendLine("S" + stationNumber + " AI Locate: Recipe=" + wanted +
                    ", vendor_authorized=" + allowed + ", model=" + model);
                if (!wanted) locateResult = "Recipe 未啟用";
                else if (!allowed) locateResult = "原版授權未開放";
                else
                {
                    string baseName = Path.GetFileNameWithoutExtension(model);
                    string h5 = Path.Combine(app, "Model", baseName + ".lh5d");
                    string trt = Path.Combine(app, "Model", baseName + ".ltrt");
                    if (!File.Exists(h5) || !File.Exists(trt))
                        locateResult = "缺少原版模型";
                    else
                    {
                        var locate = type.GetField("locateTool").GetValue(station);
                        locate.GetType().GetMethod("ReLoadAIModel").Invoke(locate,
                            new object[] { false });
                        var aiField = locate.GetType().GetField("locateAIEnhanceTool",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        var processor = aiField.GetValue(locate);
                        bool loaded = (bool)processor.GetType().GetField("ModelLoaded")
                            .GetValue(processor);
                        locateResult = loaded ? "原版模型載入成功" : "原版模型載入失敗";
                    }
                }

                bool prophetOn = (bool)param.GetType().GetField("AIProphetToolON").GetValue(param);
                bool prophetAllowed = (bool)param.GetType().GetField("AllowEnableAIProphet").GetValue(param);
                var prophetParam = param.GetType().GetField("AIProphetParam").GetValue(param);
                string prophetName = Convert.ToString(prophetParam.GetType()
                    .GetField("ModelName").GetValue(prophetParam));
                lines.AppendLine("S" + stationNumber + " AI Prophet: Recipe=" + prophetOn +
                    ", vendor_authorized=" + prophetAllowed + ", model=" + prophetName);
                if (!prophetOn) prophetResult = "Recipe 未啟用";
                else if (!prophetAllowed) prophetResult = "原版授權未開放";
                else if (string.IsNullOrWhiteSpace(prophetName) ||
                    !File.Exists(Path.Combine(app, "PModel", prophetName + ".onnx")))
                    prophetResult = "找不到訓練模型";
                else
                {
                    var prophet = type.GetField("aiProphetTools").GetValue(station);
                    bool loaded = (bool)prophet.GetType().GetMethod("ReadModel")
                        .Invoke(prophet, new object[] { "" });
                    prophetResult = loaded ? "原版模型載入成功" : "原版模型載入失敗";
                }
            }
            catch (Exception ex)
            {
                lines.AppendLine("Exception: " + ex.GetBaseException());
                if (locateResult == "未檢查") locateResult = "載入失敗";
                if (prophetResult == "未檢查") prophetResult = "尚未驗證";
            }
            lines.AppendLine("AI Locate result=" + locateResult);
            lines.AppendLine("AI Prophet result=" + prophetResult);
            File.WriteAllText(Path.Combine(app, "chao8side-ai-readiness.log"),
                lines.ToString(), Encoding.UTF8);
            return "定位 " + locateResult + "／先知 " + prophetResult;
        }

        internal static bool OriginalInputExperimentEnabled()
        {
            string root = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string marker = Path.Combine(root, "original-input-experiment.marker");
            return IsPortableReview() &&
                string.Equals(Environment.GetEnvironmentVariable("INSPTOOLS_ORIGINAL_INPUT_EXPERIMENT"), "1", StringComparison.Ordinal) &&
                File.Exists(marker) && File.ReadAllText(marker).Trim() == "ISOLATED_ORIGINAL_INPUT_EXPERIMENT_V1";
        }

        private static void RequireIsolatedSandbox(string recipeName)
        {
            string appBase = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar);
            string suffix = Path.Combine("output", "native-lazy", "runtime");
            bool sandbox = appBase.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                && recipeName.StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase);
            if (!sandbox && !IsPortableReview())
                throw new InvalidOperationException(
                    "R3 真實檢測物件只允許在隔離測試區或獨立攜帶版載入");

            string dll = Path.Combine(appBase, "InspTools.dll");
            if (!File.Exists(dll)) throw new FileNotFoundException("InspTools.dll", dll);
            string hash = DllSha(dll);
            // CHAO3's separately marked candidate is pinned in IsPortableReview.
            // The previous strict CHAO2-only check rejected its valid Recipe
            // even after the marker was verified, leaving all tools disabled.
            bool auditedChao2 = string.Equals(hash, PinnedLazyDllSha,
                StringComparison.OrdinalIgnoreCase);
            bool localChao3 = string.Equals(hash, Chao3LocalDllSha,
                StringComparison.OrdinalIgnoreCase) && IsPortableReview();
            if (!auditedChao2 && !localChao3)
                throw new InvalidOperationException("R3 DLL Hash 不符，拒絕使用原版或未知 DLL");
        }
    }
}
