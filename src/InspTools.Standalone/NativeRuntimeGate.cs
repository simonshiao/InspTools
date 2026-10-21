using System;
using System.IO;
using System.Text;

namespace InspTools.Standalone
{
    /// <summary>
    /// Fail-closed gate for untested native model initialization.
    /// In the pinned InspTools.dll, LocateTool.SetBuffers calls
    /// LocateAIEnhanceTool.CreatInst unconditionally, which calls native
    /// DeltaAIProcessDll.CreateInstance even before any recipe/model load.
    /// </summary>
    internal static class NativeRuntimeGate
    {
        internal const int BlockedExitCode = 46;
        internal const string Reason =
            "完整 CVInspection 尚未驗收：LocateTool.SetBuffers 無條件呼叫 " +
            "LocateAIEnhanceTool.CreatInst -> DeltaAIProcessDll.CreateInstance。 " +
            "目前請用 --recipe-preview 及基本 Recipe 儲存功能。";

        internal static bool FullRecipeRequested(string[] args)
        {
            if (args == null) return false;
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], "--recipe", StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        internal static void SaveBlockedNotice()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "RuntimeGate.txt");
            File.WriteAllText(file, "BLOCKED: " + Reason + Environment.NewLine,
                new UTF8Encoding(true));
        }

        internal static bool TryPreflight(string[] args, out int code)
        {
            code = 0;
            if (args == null || args.Length != 1 ||
                !string.Equals(args[0], "--runtime-preflight", StringComparison.OrdinalIgnoreCase))
                return false;
            string root = AppDomain.CurrentDomain.BaseDirectory;
            Console.OutputEncoding = new UTF8Encoding(false);
            Console.WriteLine("INSPTOOLS=" + File.Exists(Path.Combine(root, "InspTools.dll")));
            Console.WriteLine("DELTA_AI_DLL=" + File.Exists(Path.Combine(root, "DeltaAIProcessDll.dll")));
            Console.WriteLine("RECIPE_FOLDER=" + Directory.Exists(Path.Combine(root, "Recipe")));
            Console.WriteLine("MODEL_FOLDER=" + Directory.Exists(Path.Combine(root, "Model")));
            Console.WriteLine("R2_NATIVE_PARAMFILE=AVAILABLE");
            Console.WriteLine("FULL_CVINSPECTION=BLOCKED");
            Console.WriteLine("BLOCK_REASON=" + Reason);
            code = BlockedExitCode;
            return true;
        }
    }
}
