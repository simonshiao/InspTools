using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InspTools.Standalone
{
    /// <summary>
    /// TEST ONLY: real CVInspection + ToolsWindow, eight stations, NO ReadModel
    /// and NO Inspect. Guard the exact test AppBase and patched native DLL.
    /// Run only as a timeout-bounded child process.
    /// </summary>
    internal sealed class R3NativeGuiSmoke
    {
        private const string ExpectedDllSha =
            "ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B";
        private readonly Window _window;
        private readonly string _recipe;
        private readonly List<string> _results = new List<string>();
        private readonly bool[] _binary = new bool[8];
        internal int ExitCode { get; private set; }

        private R3NativeGuiSmoke(Window window, string recipe)
        {
            _window = window;
            _recipe = recipe;
            ExitCode = 55;
        }

        internal static R3NativeGuiSmoke Attach(Window window, string recipe)
        {
            string basePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string normalized = basePath.Replace('/', '\\');
            if (!normalized.EndsWith(
                    @"\output\native-lazy\runtime", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("R3 GUI 測試僅允許 output/native-lazy/runtime");
            if (string.IsNullOrWhiteSpace(recipe) ||
                !recipe.StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("R3 GUI 測試需要 R2PROBE- 隔離 Recipe");
            string dll = Path.Combine(basePath, "InspTools.dll");
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(dll))
            {
                string actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
                if (!string.Equals(actual, ExpectedDllSha, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("R3 DLL 與已驗收的 lazy-init binary 不一致");
            }
            StandaloneRecipeStore.Validate(recipe);
            var smoke = new R3NativeGuiSmoke(window, recipe);
            smoke.Prepare();
            smoke._window.Loaded += delegate
            {
                smoke._window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(smoke.Execute));
            };
            return smoke;
        }

        private void Prepare()
        {
            Type windowType = _window.GetType();
            Assembly asm = windowType.Assembly;
            Type inspectionType = asm.GetType("InspTools.Station.CVInspection", true);
            FieldInfo paramField = inspectionType.GetField("paramFile",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
            FieldInfo binaryField = paramType.GetField("BinaryToolON",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo setRecipe = inspectionType.GetMethod("SetRecipe",
                BindingFlags.Instance | BindingFlags.Public);
            if (paramField == null || binaryField == null || setRecipe == null)
                throw new MissingMemberException("原版 CVInspection/ParamFile Recipe 欄位不存在");
            Array stations = Array.CreateInstance(inspectionType, 8);
            for (int i = 0; i < 8; i++)
            {
                object station = Activator.CreateInstance(inspectionType);
                object[] parameters = { _recipe, i, "" };
                setRecipe.Invoke(station, parameters);
                if (!string.IsNullOrWhiteSpace(Convert.ToString(parameters[2])))
                    throw new InvalidDataException("S" + (i+1) + " Recipe warning: " + parameters[2]);
                object param = paramField.GetValue(station);
                if (param == null) throw new InvalidDataException("站別參數為空");
                _binary[i] = Convert.ToBoolean(binaryField.GetValue(param));
                stations.SetValue(station, i);
            }
            MethodInfo setStation = windowType.GetMethod("SetStation",
                BindingFlags.Instance | BindingFlags.Public);
            if (setStation == null) throw new MissingMethodException("ToolsWindow.SetStation");
            setStation.Invoke(_window, new object[] { stations });
            _results.Add("PASS real CVInspection SetRecipe 8/8");
        }

        private void Execute()
        {
            try
            {
                Type windowType = _window.GetType();
                MethodInfo setIndex = windowType.GetMethod("SetStationIndex",
                    BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(int) }, null);
                FieldInfo checkbox = windowType.GetField("chk_BinaryToolON",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (setIndex == null || checkbox == null)
                    throw new MissingMemberException("ToolsWindow SetStationIndex/Checkbox");
                for (int i = 0; i < 8; i++)
                {
                    setIndex.Invoke(_window, new object[] { i });
                    CheckBox cb = checkbox.GetValue(_window) as CheckBox;
                    if (cb == null || cb.IsChecked != _binary[i])
                        throw new InvalidDataException("S" + (i+1) + " 原版 GUI 值與 ParamFile 不一致");
                    _results.Add("PASS S" + (i+1) + " GUI native checkbox");
                }
                _results.Add("PASS no ReadModel or Inspect invoked");
                ExitCode = 0;
            }
            catch (Exception error)
            {
                _results.Add("FAIL " + error.GetBaseException());
                ExitCode = 55;
            }
            finally
            {
                try
                {
                    File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "r3-native-gui-result.txt"), _results.ToArray(), new UTF8Encoding(false));
                }
                catch { }
                _window.Close();
            }
        }
    }
}
