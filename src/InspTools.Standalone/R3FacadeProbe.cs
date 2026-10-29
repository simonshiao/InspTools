using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace InspTools.Standalone
{
    /// <summary>
    /// Read-only engineering probes for the existing Phase-1 Stable Facade.
    /// Execute ONLY as a bounded disposable child process in the R3 sandbox.
    /// This does not add a second SDK/Host or change the normal GUI route.
    /// </summary>
    internal static class R3FacadeProbe
    {
        private const string ExpectedDllSha =
            "ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B";
        private const string ExperimentalModelDllSha =
            "6A6E842F53D7C88674289CD85A2BD65C1A1D71870ED6A565127FE5DB7ACBEBC7";
        private const string ShuntName = "R2PROBE-SHUNT-3923-R002";
        private const string OkSha =
            "1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45";
        private const string NgSha =
            "E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4";
        private const string S1OkSha =
            "A7AFF9D716CD394DE072E84564F916BA58744133A52C5E1B9D2B8B287AD3F5CA";
        private const string S1NgSha =
            "DA43AAC9BE43B84E1B3AE9366468260EF368917E79B370E93628E9254B0E5F48";
        private const string S1PbdSha =
            "B5B8759891FA93CED197347C95F038724F620F10E0969AD7D7F6278D8651AEA5";
        private const string S1H5dSha =
            "BB36AAA4017D154F161895E63CFB2B97C3CBF1F549676F0D3CB12A138D3C6476";
        // Fixed engineering witness for a single candidate in a disposable Gate.
        // Never use these hashes to authorize production deployment.
        private const string DiaCandidateRecipe = "R2PROBE-DIA-3923-R002";
        private const string DiaCandidateH5dSha =
            "7460AC816B12E6E58A647E92F9CCEB56787BFDD87A26FE9E5D6D3A7F28A7FF89";
        private const string DiaCandidatePbdSha =
            "F88D6F19A67C535A739490A6CC65BEC76383A7179498B7803C2427CBF81D207F";
        private const string DiaBalancedH5dSha =
            "9405C60991EFA3D6D08A688F7D946DF2276C9C3AA0200F08A31196483DFBEAF0";
        private const string DiaBalancedPbdSha =
            "F734108ECCA953715F6CFCE56402556DA9AF1983643D9584A58D42CC20E71ADF";
        private const string DiaSecondHoldoutOkSha =
            "33C880CBA893658E4CCF1B8141EEBC340352281ECF59BE4640557B337B0421AD";
        private const string DiaSecondHoldoutNgSha =
            "93EE6DC20F726AE57BE288E71D95B51A382FF3B2EE690DDAF3C0453B44AD49BC";
        private const string DiaVendorH5dSha =
            "99286716F6A1AD9EB28F70CFE0FA122521DC71B99C13800CF391C2AB890AEF73";
        private const string DiaVendorPbdSha =
            "47533BA5CDAFBA4ADC1961F417BD64CFBC572EEF59125293893900F5C831F48B";

        internal static bool TryRun(string[] args, out int code)
        {
            code = 0;
            if (args == null || args.Length == 0 ||
                !args[0].StartsWith("--r3-facade-", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                Console.OutputEncoding = new UTF8Encoding(false);
                bool loading = string.Equals(args[0], "--r3-facade-load",
                    StringComparison.OrdinalIgnoreCase);
                bool inspecting = string.Equals(args[0], "--r3-facade-inspect",
                    StringComparison.OrdinalIgnoreCase);
                bool modeling = string.Equals(args[0], "--r3-facade-model",
                    StringComparison.OrdinalIgnoreCase);
                bool candidateModel = string.Equals(args[0], "--r3-facade-model-candidate",
                    StringComparison.OrdinalIgnoreCase);
                bool candidateInspect = string.Equals(args[0], "--r3-facade-inspect-candidate",
                    StringComparison.OrdinalIgnoreCase);
                bool vendorInspect = string.Equals(args[0], "--r3-facade-inspect-vendor",
                    StringComparison.OrdinalIgnoreCase);
                bool balancedInspect = string.Equals(args[0], "--r3-facade-inspect-balanced",
                    StringComparison.OrdinalIgnoreCase);
                bool candidateMode = candidateModel || candidateInspect || vendorInspect ||
                    balancedInspect;
                modeling = modeling || candidateModel;
                bool inspectingWithModel = string.Equals(args[0],
                    "--r3-facade-inspect-enhance", StringComparison.OrdinalIgnoreCase) ||
                    candidateInspect || vendorInspect || balancedInspect;
                if ((!loading && !inspecting && !modeling && !inspectingWithModel) ||
                    args.Length != (loading ? 2 : modeling ? 3 : 4))
                    throw new ArgumentException(
                        "Usage: --r3-facade-load RECIPE | --r3-facade-inspect RECIPE STATION OK/NG/S1OK/S1NG | --r3-facade-model RECIPE STATION | --r3-facade-inspect-enhance RECIPE STATION S1OK/S1NG");
                string recipeName = args[1];
                VerifySandbox(recipeName, modeling || inspectingWithModel);
                var watch = Stopwatch.StartNew();
                string currentRoot = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory)
                    .TrimEnd(Path.DirectorySeparatorChar);
                bool isDeltaGate = Path.GetFileName(currentRoot).StartsWith("Gate-",
                    StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(Path.Combine(currentRoot, "delta-runtime-gate-manifest.json"));
                if ((modeling || inspecting || inspectingWithModel) && isDeltaGate)
                {
                    if (!DeltaNativeRuntimeBootstrap.InitializeIfPresent())
                        throw new InvalidOperationException("Delta gate has no private Runtime");
                    Mark("APP_LOCAL_DELTA_RUNTIME=READY", watch);
                }
                Assembly asm = Assembly.LoadFrom(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll"));
                Type facadeType = asm.GetType(
                    "InspTools.SecondaryDevelopment.InspToolsFacade", true);
                object facade = Activator.CreateInstance(facadeType);
                Mark("FACADE_CONSTRUCTED", watch);
                object snapshot = facadeType.GetMethod("LoadRecipe").Invoke(
                    facade, new object[] { recipeName });
                Mark("FACADE_LOAD_END", watch);
                if (!string.Equals(Get(snapshot, "RecipeName").ToString(), recipeName,
                    StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Facade returned unexpected Recipe");
                var stations = Get(snapshot, "Stations") as IEnumerable;
                if (stations == null) throw new InvalidDataException("Missing station snapshots");
                int count = 0;
                foreach (object item in stations)
                {
                    count++;
                    int number = Convert.ToInt32(Get(item, "StationNumber"));
                    if (number != count)
                        throw new InvalidDataException("Facade station mapping differs at S" + count);
                    Console.WriteLine("STATION=" + number +
                        " ENABLED=" + Get(item, "StationEnabled") +
                        " ENHANCE=" + Get(item, "EnhanceEnabled") +
                        " PREDICT=" + Get(item, "PredictEnabled") +
                        " PROPHET=" + Get(item, "ProphetEnabled"));
                }
                if (count != 8) throw new InvalidDataException("Facade expected 8 stations, got " + count);
                Mark("FACADE_STATIONS=8", watch);
                if (loading)
                {
                    Mark("FACADE_LOAD=PASS", watch);
                    return true;
                }

                if (!(candidateMode ?
                    string.Equals(recipeName, DiaCandidateRecipe, StringComparison.Ordinal) :
                    string.Equals(recipeName, ShuntName, StringComparison.Ordinal)))
                    throw new ArgumentException("Inspect/model requires its pinned isolated Recipe");
                int station = int.Parse(args[2]);
                if (station != 1)
                    throw new ArgumentException("Initial focused Inspect/model gate allows only CCD1/S1");
                if (modeling || inspectingWithModel)
                {
                    MethodInfo stationFactory = facadeType.GetMethod("GetOrCreateStation",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (stationFactory == null)
                        throw new MissingMethodException(facadeType.FullName, "GetOrCreateStation");
                    object actualStation = stationFactory.Invoke(facade, new object[] { station - 1 });
                    FieldInfo nativeParam = actualStation.GetType().GetField("paramFile");
                    object paramObject = nativeParam.GetValue(actualStation);
                    FieldInfo enhanceParam = paramObject.GetType().GetField("AIEnhanceParam");
                    object enhanceObject = enhanceParam.GetValue(paramObject);
                    string configuredName = Convert.ToString(
                        enhanceObject.GetType().GetField("ModelName_A").GetValue(enhanceObject));
                    string candidateStem = Path.GetFileNameWithoutExtension(configuredName);
                    string modelRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model");
                    Console.WriteLine("CONFIG_MODEL_NAME=" + configuredName +
                        " EXPECTED_PBD=" + candidateStem + ".pbd" +
                        " SOURCE_PRESENT=" + File.Exists(Path.Combine(modelRoot, candidateStem + ".h5d")) +
                        " PBD_PRESENT=" + File.Exists(Path.Combine(modelRoot, candidateStem + ".pbd")));
                    string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "Model", candidateMode ? "R002-CCD1.pbd" : "VSMA8420AYM050JEW_A1.pbd");
                    string modelSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "Model", candidateMode ? "R002-CCD1.h5d" : "VSMA8420AYM050JEW_A1.h5d");
                    if (candidateMode && configuredName != "R002-CCD1.h5d")
                        throw new InvalidDataException("Candidate Recipe model filename differs");
                    string expectedPbdSha = balancedInspect ? DiaBalancedPbdSha :
                        vendorInspect ? DiaVendorPbdSha :
                        candidateMode ? DiaCandidatePbdSha : S1PbdSha;
                    string expectedH5dSha = balancedInspect ? DiaBalancedH5dSha :
                        vendorInspect ? DiaVendorH5dSha :
                        candidateMode ? DiaCandidateH5dSha : S1H5dSha;
                    if (!File.Exists(modelPath) ||
                        !string.Equals(Hash(modelPath), expectedPbdSha,
                            StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Pinned S1 model missing or hash mismatch");
                    if (!File.Exists(modelSource) ||
                        !string.Equals(Hash(modelSource), expectedH5dSha,
                            StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Pinned S1 .h5d source missing or hash mismatch");
                    Type family = asm.GetType(
                        "InspTools.SecondaryDevelopment.ModelFamily", true);
                    object enhance = Enum.Parse(family, "Enhance");
                    MethodInfo loadModel = facadeType.GetMethod("LoadConfiguredModel",
                        new[] { typeof(int), family });
                    if (loadModel == null) throw new MissingMethodException(
                        facadeType.FullName, "LoadConfiguredModel");
                    Console.WriteLine("FACADE_MODEL_BEGIN FAMILY=Enhance STATION=1");
                    Console.Out.Flush();
                    object model = loadModel.Invoke(facade,
                        new[] { (object)station, enhance });
                    Mark("FACADE_MODEL_RETURNED", watch);
                    bool success = Convert.ToBoolean(Get(model, "Success"));
                    bool loaded = Convert.ToBoolean(Get(model, "Loaded"));
                    Console.WriteLine("MODEL_NAME=" + Get(model, "ModelName") +
                        " SUCCESS=" + success + " LOADED=" + loaded +
                        " TENSOR_RT=" + Get(model, "UseTensorRt") +
                        " MESSAGE=" + Clean(Convert.ToString(Get(model, "Message"))));
                    if (success && loaded)
                        Mark("FACADE_MODEL=PASS", watch);
                    else
                    {
                        Console.WriteLine("FACADE_MODEL=UNAVAILABLE");
                        code = 66;
                    }
                    if (modeling || code != 0) return true;
                }
                string label = args[3].ToUpperInvariant();
                if (label != "OK" && label != "NG" &&
                    label != "S1OK" && label != "S1NG" &&
                    !(candidateMode && (label == "S1OK2" || label == "S1NG2")))
                    throw new ArgumentException("Unrecognized pinned image label");
                if (inspectingWithModel && label != "S1OK" && label != "S1NG" &&
                    !(candidateMode && (label == "S1OK2" || label == "S1NG2")))
                    throw new ArgumentException("Enhanced Inspect requires Recipe-matched S1OK/S1NG image");
                string relative = (candidateInspect || vendorInspect || balancedInspect)
                    ? Path.Combine("TrainingImage", "DIA-Candidate-S1",
                        label + (label == "S1OK" || label == "S1OK2" ? ".jpg" : ".png"))
                    : Path.Combine("TrainingImage", "R3-Shunt-CCD1",
                        label + (label == "OK" ? ".jpg" : ".png"));
                string image = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
                if (!File.Exists(image)) throw new FileNotFoundException("Test image missing", image);
                string sha = Hash(image);
                string expected = (candidateInspect || vendorInspect || balancedInspect) ?
                    label == "S1OK2" ? DiaSecondHoldoutOkSha :
                    label == "S1NG2" ? DiaSecondHoldoutNgSha :
                    label == "S1OK" ? OkSha : NgSha :
                    label == "OK" ? OkSha :
                    label == "NG" ? NgSha : label == "S1OK" ? S1OkSha : S1NgSha;
                if (!string.Equals(sha, expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Pinned test image SHA changed");
                Console.WriteLine("SOURCE_LABEL=" + label + " IMAGE_SHA=" + sha);
                Type optionsType = asm.GetType(
                    "InspTools.SecondaryDevelopment.InspectionOptions", true);
                object options = Activator.CreateInstance(optionsType);
                optionsType.GetProperty("Offline").SetValue(options, true, null);
                optionsType.GetProperty("UseAi").SetValue(options, false, null);
                Console.WriteLine("FACADE_INSPECT_BEGIN STATION=" + station +
                    " OFFLINE=True USE_AI=False");
                Console.Out.Flush();
                MethodInfo inspect = facadeType.GetMethod("InspectFile",
                    new[] { typeof(int), typeof(string), optionsType });
                if (inspect == null)
                    throw new MissingMethodException(facadeType.FullName, "InspectFile");
                object result = inspect.Invoke(facade, new[] { (object)station, image, options });
                Mark("FACADE_INSPECT_RETURNED", watch);
                if (candidateInspect || vendorInspect || balancedInspect)
                {
                    MethodInfo factory = facadeType.GetMethod("GetOrCreateStation",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    object actualStation = factory.Invoke(facade, new object[] { station - 1 });
                    ReportEnhanceImages(actualStation);
                }
                Console.WriteLine("ACTUAL_BIN=" + Get(result, "Bin") +
                    " LOCATE_SUCCEEDED=" + Get(result, "LocateSucceeded") +
                    " PARETO=" + Get(result, "ParetoIndex") +
                    " ERROR=" + Clean(Convert.ToString(Get(result, "ErrorMessage"))));
                int toolCount = 0;
                var toolResults = Get(result, "ToolResults") as IEnumerable;
                if (toolResults != null)
                {
                    foreach (object tool in toolResults)
                    {
                        toolCount++;
                        if (toolCount <= 30)
                            Console.WriteLine("TOOL=" + Get(tool, "Family") +
                                "/" + Get(tool, "Section") +
                                " TRACKER=" + Clean(Convert.ToString(Get(tool, "TrackerName"))) +
                                " STATE=" + Get(tool, "State") +
                                " BIN=" + Get(tool, "Bin") +
                                " ERRORS=" + Get(tool, "ErrorCount"));
                    }
                }
                Console.WriteLine("TOOL_RESULTS=" + toolCount);
                Mark("FACADE_INSPECT=PASS", watch);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FACADE_PROBE=FAIL " + ex.GetBaseException());
                Console.Error.Flush();
                code = 64;
            }
            return true;
        }

        private static void ReportEnhanceImages(object station)
        {
            FieldInfo enhancerField = station.GetType().GetField("aiEnhanceTools");
            object enhancer = enhancerField == null ? null : enhancerField.GetValue(station);
            if (enhancer == null)
            {
                Console.WriteLine("AI_ENHANCE_OUTPUT=UNAVAILABLE");
                return;
            }
            int populatedChannels = 0;
            foreach (string channel in new[] { "R", "G", "B" })
            {
                FieldInfo field = enhancer.GetType().GetField("AIMat" + channel,
                    BindingFlags.Instance | BindingFlags.Public);
                object channelValue = field == null ? null : field.GetValue(enhancer);
                if (channelValue == null) continue;
                // Recovered AIEnhanceTool exposes one Mat; AIEnhanceCenter has
                // an array. Support both without depending on the native ABI.
                var mats = new System.Collections.Generic.List<object>();
                if (channelValue is Array)
                    foreach (object item in (Array)channelValue) mats.Add(item);
                else
                    mats.Add(channelValue);
                int filled = 0;
                string firstSha = "NONE";
                foreach (object mat in mats)
                {
                    if (mat == null) continue;
                    Type matType = mat.GetType();
                    if (Convert.ToBoolean(matType.GetMethod("Empty").Invoke(mat, null)))
                        continue;
                    filled++;
                    if (firstSha == "NONE")
                    {
                        MethodInfo png = matType.GetMethod("ToBytes",
                            new[] { typeof(string), typeof(int[]) });
                        if (png == null) throw new MissingMethodException("Mat.ToBytes PNG");
                        byte[] bytes = (byte[])png.Invoke(mat, new object[] { ".png", new int[0] });
                        using (var sha = SHA256.Create())
                            firstSha = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
                    }
                }
                if (filled > 0) populatedChannels++;
                Console.WriteLine("AI_ENHANCE_OUTPUT_" + channel + "=" +
                    (filled > 0 ? "READY" : "EMPTY") + " MATS=" + filled +
                    " PNG_SHA256=" + firstSha);
            }
            Console.WriteLine("AI_ENHANCE_OUTPUT=" +
                (populatedChannels == 3 ? "READY" : "INCOMPLETE"));
        }

        private static void VerifySandbox(string recipeName, bool modelExperiment)
        {
            string root = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar);
            bool native = root.EndsWith(Path.Combine("output", "native-lazy", "runtime"),
                StringComparison.OrdinalIgnoreCase);
            string parent = Path.GetFullPath(Path.GetDirectoryName(root))
                .TrimEnd(Path.DirectorySeparatorChar);
            bool deltaGate = parent.EndsWith(Path.Combine("output", "delta-native-gates"),
                StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(root).StartsWith("Gate-", StringComparison.OrdinalIgnoreCase) &&
                File.Exists(Path.Combine(root, "delta-runtime-gate-manifest.json"));
            if ((!native && !deltaGate) ||
                !recipeName.StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Facade test requires R3 isolated AppBase/Recipe");
            StandaloneRecipeStore.Validate(recipeName);
            string dll = Path.Combine(root, "InspTools.dll");
            string actual = Hash(dll);
            bool experimental = deltaGate && modelExperiment &&
                File.Exists(Path.Combine(root, "delta-model-experiment.marker")) &&
                string.Equals(File.ReadAllText(Path.Combine(root,
                    "delta-model-experiment.marker")).Trim(),
                    "MODEL_PRECOMPILED_EXPERIMENT_V1", StringComparison.Ordinal) &&
                string.Equals(actual, ExperimentalModelDllSha,
                    StringComparison.OrdinalIgnoreCase);
            if (!string.Equals(actual, ExpectedDllSha, StringComparison.OrdinalIgnoreCase) &&
                !experimental)
                throw new InvalidDataException("Patched R3 DLL SHA mismatch");
        }

        private static string Hash(string file)
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream fs = File.OpenRead(file))
                return BitConverter.ToString(hash.ComputeHash(fs)).Replace("-", "");
        }

        private static object Get(object obj, string name)
        {
            PropertyInfo field = obj.GetType().GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public);
            if (field == null) throw new MissingMemberException(obj.GetType().FullName, name);
            return field.GetValue(obj, null);
        }

        private static string Clean(string value)
        {
            value = (value ?? "").Replace("\r", " ").Replace("\n", " ");
            return value.Length > 220 ? value.Substring(0, 220) : value;
        }

        private static void Mark(string message, Stopwatch watch)
        {
            Console.WriteLine(message + " ELAPSED_MS=" + watch.ElapsedMilliseconds);
            Console.Out.Flush();
        }
    }
}
