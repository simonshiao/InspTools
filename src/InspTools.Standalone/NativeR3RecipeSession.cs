using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
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
                return string.Equals(File.ReadAllText(marker).Trim(),
                    PortableMarker, StringComparison.Ordinal) &&
                    File.Exists(dll) && string.Equals(DllSha(dll),
                    PinnedLazyDllSha, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static string DllSha(string dll)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream file = File.OpenRead(dll))
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
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
            selectStation.Invoke(window, new object[] { stationNumber - 1 });
            window.Title = "INSPECTION TOOLS - [R3 獨立版／真實八站／AI 未載入] " +
                recipeName + " / S" + stationNumber;
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
            if (!string.Equals(hash, PinnedLazyDllSha, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("R3 DLL Hash 不符，拒絕使用原版或未知 DLL");
        }
    }
}
