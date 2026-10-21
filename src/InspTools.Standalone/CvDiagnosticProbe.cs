using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace InspTools.Standalone
{
    /// <summary>
    /// One-station diagnostic run in a disposable bounded child process.
    /// The parent runner MUST set a timeout and terminate if native code hangs.
    /// No model load, AI training, image inspection or PLC operation.
    /// </summary>
    internal static class CvDiagnosticProbe
    {
        internal static bool TryRun(string[] args, out int code)
        {
            code = 0;
            if (args != null && args.Length > 0 &&
                string.Equals(args[0], "--cv-component-probe", StringComparison.OrdinalIgnoreCase))
            {
                code = RunComponent(args);
                return true;
            }
            if (args == null || args.Length == 0 ||
                !string.Equals(args[0], "--cv-native-diagnostic", StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                if (args.Length < 2 || args.Length > 3 ||
                    !args[1].StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("只允許 --cv-native-diagnostic R2PROBE-<隔離 Recipe> [--all-stations]");
                bool allStations = args.Length == 3 &&
                    string.Equals(args[2], "--all-stations", StringComparison.OrdinalIgnoreCase);
                if (args.Length == 3 && !allStations)
                    throw new ArgumentException("未知診斷參數：" + args[2]);
                string name = args[1];
                StandaloneRecipeStore.Validate(name);
                string dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll");
                Assembly asm = Assembly.LoadFrom(dll);
                Type cvType = asm.GetType("InspTools.Station.CVInspection", true);
                var watch = Stopwatch.StartNew();
                FieldInfo param = cvType.GetField("paramFile",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo setRecipe = cvType.GetMethod("SetRecipe",
                    BindingFlags.Public | BindingFlags.Instance);
                if (setRecipe == null) throw new MissingMethodException("CVInspection.SetRecipe");
                int count = allStations ? 8 : 1;
                for (int index = 0; index < count; index++)
                {
                    int number = index + 1;
                    Mark("S" + number + "_CONSTRUCT_BEGIN", watch);
                    object station = Activator.CreateInstance(cvType);
                    Mark("S" + number + "_CONSTRUCT_END", watch);
                    if (param == null || param.GetValue(station) == null)
                        throw new InvalidDataException("S" + number + " paramFile 為空");
                    object[] call = { name, index, "" };
                    Mark("S" + number + "_SETRECIPE_BEGIN", watch);
                    setRecipe.Invoke(station, call);
                    Mark("S" + number + "_SETRECIPE_END", watch);
                    Mark("S" + number + "_WARNING=" + Convert.ToString(call[2]), watch);
                }
                Mark("STATIONS_LOADED=" + count, watch);
                Mark("CV_DIAGNOSTIC=PASS", watch);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CV_DIAGNOSTIC=FAIL " + ex.GetBaseException());
                Console.Error.Flush();
                code = 44;
            }
            return true;
        }

        private static int RunComponent(string[] args)
        {
            try
            {
                if (args.Length < 2 || args.Length > 3)
                    throw new ArgumentException("Usage: --cv-component-probe ToolName [--withbuffers]");
                bool withBuffers = args.Length == 3 &&
                    string.Equals(args[2], "--withbuffers", StringComparison.OrdinalIgnoreCase);
                if (args.Length == 3 && !withBuffers)
                    throw new ArgumentException("未知參數：" + args[2]);
                string[] allowed = { "CVBuffer", "LocateTool", "BinaryTool", "MaskTool",
                    "AIProphetTool", "AIEnhanceTool", "AIPredictTool" };
                if (Array.IndexOf(allowed, args[1]) < 0)
                    throw new ArgumentException("未知原廠元件：" + args[1]);
                var sw = Stopwatch.StartNew();
                Mark("COMPONENT_BEGIN=" + args[1], sw);
                Assembly asm = Assembly.LoadFrom(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll"));
                Type toolType = asm.GetType("InspTools.AOI." + args[1], true);
                object tool;
                if (args[1] == "CVBuffer")
                {
                    tool = Activator.CreateInstance(toolType);
                }
                else
                {
                    Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
                    object parameters = Activator.CreateInstance(paramType);
                    Mark("PARAMFILE_CREATED", sw);
                    ConstructorInfo ctor = toolType.GetConstructor(new[] { paramType });
                    if (ctor == null)
                        throw new MissingMethodException(toolType.FullName, ".ctor(ParamFile)");
                    tool = ctor.Invoke(new object[] { parameters });
                }
                if (tool == null) throw new InvalidDataException("工具未建立");
                Mark("COMPONENT_END=" + args[1], sw);
                if (withBuffers)
                {
                    Type bufferType = asm.GetType("InspTools.AOI.CVBuffer", true);
                    object buffer = Activator.CreateInstance(bufferType);
                    MethodInfo assign = toolType.GetMethod("SetBuffers",
                        BindingFlags.Instance | BindingFlags.Public,
                        null, new[] { bufferType }, null);
                    if (assign == null)
                        throw new MissingMethodException(toolType.FullName, "SetBuffers(CVBuffer)");
                    Mark("SETBUFFERS_BEGIN=" + args[1], sw);
                    assign.Invoke(tool, new[] { buffer });
                    Mark("SETBUFFERS_END=" + args[1], sw);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("COMPONENT_FAIL " + ex.GetBaseException());
                Console.Error.Flush();
                return 45;
            }
        }

        private static void Mark(string what, Stopwatch watch)
        {
            Console.WriteLine(what + " elapsed_ms=" + watch.ElapsedMilliseconds);
            Console.Out.Flush();
        }
    }
}
