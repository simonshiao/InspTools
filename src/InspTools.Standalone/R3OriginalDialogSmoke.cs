using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace InspTools.Standalone
{
    /// <summary>
    /// TEST ONLY: open original Locate/Binary child windows on a real R3
    /// CVInspection. No model, Inspect, machine I/O, SaveAllRecipe or DB writes.
    /// Invoked in a bounded isolated process only.
    /// </summary>
    internal sealed class R3OriginalDialogSmoke
    {
        private readonly Window _main;
        private readonly string _recipe;
        private readonly StringBuilder _log = new StringBuilder();
        internal int ExitCode { get; private set; }

        private R3OriginalDialogSmoke(Window main, string recipe)
        {
            _main = main;
            _recipe = recipe;
            ExitCode = 58;
        }

        internal static R3OriginalDialogSmoke Attach(Window main, string recipe)
        {
            if (!recipe.StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("R3 dialog test requires R2PROBE recipe");
            // Requires sandbox path and SHA-pinned patched DLL.
            NativeR3RecipeSession.Open(main, recipe, 1);
            var smoke = new R3OriginalDialogSmoke(main, recipe);
            main.Loaded += delegate
            {
                main.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(smoke.Run));
            };
            return smoke;
        }

        private void Assert(bool value, string label)
        {
            if (!value) throw new InvalidOperationException(label);
            _log.AppendLine("PASS " + label);
        }

        private void Run()
        {
            string recipeRoot = StandaloneRecipeStore.Validate(_recipe);
            string[] stations = Enumerable.Range(1, 8)
                .Select(i => Path.Combine(recipeRoot, "station" + i + ".ini")).ToArray();
            string[] hashes = stations.Select(FileHash).ToArray();
            try
            {
                Assembly asm = _main.GetType().Assembly;
                FieldInfo stationField = _main.GetType().GetField("Station",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert(stationField != null, "Original ToolsWindow.Station exists");
                Array actualStations = stationField.GetValue(_main) as Array;
                Assert(actualStations != null && actualStations.Length == 8,
                    "Original UI bound to eight real stations");
                object station = actualStations.GetValue(0);
                Assert(station != null, "Original S1 CVInspection exists");

                Type locateType = asm.GetType("InspTools.AOI.LocateWindow", true);
                Window locate = Activator.CreateInstance(locateType) as Window;
                Assert(locate != null, "Original LocateWindow constructed");
                locateType.GetMethod("SetInspection", new[] { station.GetType() })
                    .Invoke(locate, new[] { station });
                locateType.GetMethod("SetPreviewMode", new[] { typeof(bool) })
                    .Invoke(locate, new object[] { true });
                locate.Owner = _main;
                locate.Show();
                locate.UpdateLayout();
                Assert(locate.IsVisible, "Original LocateWindow shown in preview");
                locate.Close();
                Assert(!locate.IsVisible, "Original LocateWindow closed");

                Type binaryType = asm.GetType("InspTools.AOI.BinaryWindow", true);
                ConstructorInfo binaryConstructor = binaryType.GetConstructors()
                    .FirstOrDefault(x => x.GetParameters().Length == 1);
                Assert(binaryConstructor != null, "Original BinaryWindow constructor exists");
                Type permissionType = binaryConstructor.GetParameters()[0].ParameterType;
                object engineer = Enum.GetNames(permissionType).Contains("USER_Engineer")
                    ? Enum.Parse(permissionType, "USER_Engineer")
                    : Enum.ToObject(permissionType, 0);
                Window binary = binaryConstructor.Invoke(new[] { engineer }) as Window;
                Assert(binary != null, "Original BinaryWindow constructed");
                MethodInfo bind = binaryType.GetMethods().First(x =>
                    x.Name == "SetInspection" && x.GetParameters().Length == 3);
                bind.Invoke(binary, new object[] { 0, station, 0 });
                binaryType.GetMethod("SetPreviewMode", new[] { typeof(bool) })
                    .Invoke(binary, new object[] { true });
                binary.Owner = _main;
                binary.Show();
                binary.UpdateLayout();
                Assert(binary.IsVisible, "Original BinaryWindow shown in preview");
                binary.Close();
                Assert(!binary.IsVisible, "Original BinaryWindow closed");

                for (int i = 0; i < stations.Length; i++)
                    Assert(hashes[i] == FileHash(stations[i]),
                        "Recipe S" + (i + 1) + " INI unchanged");
                ExitCode = 0;
            }
            catch (Exception ex)
            {
                _log.AppendLine("FAIL " + ex.GetBaseException());
                ExitCode = 58;
            }
            finally
            {
                try
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "r3-dialog-smoke-result.txt"), _log.ToString(), new UTF8Encoding(false));
                }
                catch { }
                _main.Close();
            }
        }

        private static string FileHash(string file)
        {
            using (SHA256 sha = SHA256.Create())
            using (Stream stream = File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
    }
}
