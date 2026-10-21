using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InspTools.Standalone
{
    /// <summary>
    /// Shunt CCD1 image / R002 recipe compatibility probe, no ReadModel/Inspect.
    /// Uses two copied, SHA-verified samples inside the disposable R3 AppBase.
    /// This is not an OK/NG classification test.
    /// </summary>
    internal sealed class R3ShuntImageSmoke
    {
        private readonly Window _main;
        private readonly string _recipe;
        private readonly bool _saveSmoke;
        private readonly StringBuilder _log = new StringBuilder();
        internal int ExitCode { get; private set; }

        private R3ShuntImageSmoke(Window window, string recipe, bool saveSmoke)
        {
            _main = window; _recipe = recipe; _saveSmoke = saveSmoke; ExitCode = 63;
        }

        internal static R3ShuntImageSmoke Attach(Window window, string recipe,
            bool saveSmoke = false)
        {
            if (!string.Equals(recipe, "R2PROBE-SHUNT-3923-R002",
                    StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Only the isolated Shunt Recipe may be tested");
            NativeR3RecipeSession.Open(window, recipe, 1);
            var smoke = new R3ShuntImageSmoke(window, recipe, saveSmoke);
            window.Loaded += delegate {
                window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(smoke.Run));
            };
            return smoke;
        }

        private void Check(bool okay, string description)
        {
            if (!okay) throw new InvalidDataException(description);
            _log.AppendLine("PASS " + description);
        }

        private static string Hash(string file)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static object GetMember(object obj, string name)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance;
            var type = obj.GetType();
            var prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(obj, null);
            var field = type.GetField(name, flags);
            if (field != null) return field.GetValue(obj);
            throw new MissingMemberException(type.FullName, name);
        }

        private static string FirstTrackerName(object param)
        {
            Array trackers = GetMember(param, "BinaryParam") as Array;
            if (trackers == null || trackers.Length == 0)
                throw new InvalidDataException("Missing binary tracker");
            return Convert.ToString(GetMember(trackers.GetValue(0), "TrackerName"));
        }

        private void Run()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string folder = StandaloneRecipeStore.Validate(_recipe);
            string publicDb = Path.Combine(root, "PublicDB");
            // Snapshot before deliberate native SaveAllRecipe; keep a crash
            // recovery copy under this disposable sandbox, never in ShuntVC.
            var originalFiles = new Dictionary<string, byte[]>(
                StringComparer.OrdinalIgnoreCase);
            if (_saveSmoke)
            {
                foreach (string source in new[] { folder, publicDb })
                {
                    if (!Directory.Exists(source)) continue;
                    foreach (string file in Directory.GetFiles(source, "*",
                        SearchOption.AllDirectories))
                        originalFiles.Add(file, File.ReadAllBytes(file));
                }
                string backup = Path.Combine(root, "_r3_shunt_binary_backup");
                if (Directory.Exists(backup))
                    throw new InvalidOperationException("Stale recovery backup exists");
                foreach (var file in originalFiles)
                {
                    string relative = file.Key.Substring(root.Length)
                        .TrimStart(Path.DirectorySeparatorChar);
                    string path = Path.Combine(backup, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, file.Value);
                }
            }
            var settings = Enumerable.Range(1, 8).Select(i =>
                Path.Combine(folder, "station" + i + ".ini")).ToArray();
            var before = settings.Select(Hash).ToArray();
            try
            {
                Array stations = GetMember(_main, "Station") as Array;
                Check(stations != null && stations.Length == 8, "Shunt R002 eight real stations");
                object station = stations.GetValue(0);
                object nativeParam = GetMember(station, "paramFile");
                string originalTrackerName = FirstTrackerName(nativeParam);
                string editedTrackerName = "R3-SHUNT-CCD1-VALID-ROI";
                if (_saveSmoke)
                    Check(!string.Equals(originalTrackerName, editedTrackerName,
                        StringComparison.Ordinal), "Test edit differs from original tracker");
                object buffers = GetMember(station, "buffers");
                string[] labels = { "OK", "NG" };
                string[] extensions = { ".jpg", ".png" };
                string[] knownHash = {
                    "1A45DE213F72475AC42220B236E72B48913BEA728B44B5899588FB3BBA9C1D45",
                    "E6739A1223201CEF40D2BD2531FD0F4A50DD6CEAD568CC1432C6F286E7EFF2C4"
                };
                Type binaryType = _main.GetType().Assembly.GetType(
                    "InspTools.AOI.BinaryWindow", true);
                ConstructorInfo ctor = binaryType.GetConstructors()
                    .First(x => x.GetParameters().Length == 1);
                Type permission = ctor.GetParameters()[0].ParameterType;
                object role = Enum.GetNames(permission).Contains("USER_Engineer")
                    ? Enum.Parse(permission, "USER_Engineer")
                    : Enum.ToObject(permission, 0);

                for (int i = 0; i < 2; i++)
                {
                    string sample = Path.Combine(root,
                        "TrainingImage\\R3-Shunt-CCD1\\" + labels[i] + extensions[i]);
                    Check(File.Exists(sample), labels[i] + " copied Shunt CCD1 image exists");
                    Check(string.Equals(Hash(sample), knownHash[i],
                        StringComparison.OrdinalIgnoreCase),
                        labels[i] + " copied source SHA verified");

                    // Reuse the exact native OpenCvSharp image decoder that
                    // InspTools already ships with; no System.Drawing loading.
                    Assembly cv = Assembly.LoadFrom(Path.Combine(root, "OpenCvSharp.dll"));
                    Type cv2 = cv.GetType("OpenCvSharp.Cv2", true);
                    Type imreadModes = cv.GetType("OpenCvSharp.ImreadModes", true);
                    object color = Enum.Parse(imreadModes, "Color");
                    MethodInfo imread = cv2.GetMethod("ImRead",
                        new[] { typeof(string), imreadModes });
                    object bmp = imread.Invoke(null, new[] { (object)sample, color });
                    try
                    {
                        Type matType = bmp.GetType();
                        int width = (int)matType.GetProperty("Width").GetValue(bmp, null);
                        int height = (int)matType.GetProperty("Height").GetValue(bmp, null);
                        Check(width == 1292 && height == 964,
                            labels[i] + " original image dimensions 1292x964");
                        MethodInfo set = buffers.GetType().GetMethods().First(x =>
                            x.Name == "SetBuffer" && x.GetParameters().Length == 1 &&
                            x.GetParameters()[0].ParameterType == matType);
                        Check((bool)set.Invoke(buffers, new[] { bmp }),
                            labels[i] + " decoded into original CVBuffer");

                        object src = GetMember(buffers, "Src");
                        object located = src.GetType().GetMethod("Clone",
                            Type.EmptyTypes).Invoke(src, null);
                        buffers.GetType().GetProperty("LocatedSrc")
                            .SetValue(buffers, located, null);
                        Window dialog = ctor.Invoke(new[] { role }) as Window;
                        Check(dialog != null, labels[i] + " original BinaryWindow constructed");
                        binaryType.GetMethod("SetInspection", new[] {
                            typeof(int), station.GetType(), typeof(int)
                        }).Invoke(dialog, new object[] { 0, station, 0 });
                        binaryType.GetMethod("SetPreviewMode", new[] { typeof(bool) })
                            .Invoke(dialog, new object[] { false });
                        Type rectType = src.GetType().Assembly.GetType("OpenCvSharp.Rect", true);
                        object rect = Activator.CreateInstance(rectType,
                            new object[] { 0, 0, width, height });
                        MethodInfo setBuffer = binaryType.GetMethods()
                            .First(x => x.Name == "SetBuffer" &&
                                x.GetParameters().Length == 3);
                        dialog.Owner = _main;
                        dialog.Show();
                        setBuffer.Invoke(dialog, new object[] { src, rect, true });
                        dialog.UpdateLayout();
                        Check(dialog.IsVisible, labels[i] + " original BinaryWindow image opened");
                        _log.AppendLine("INFO " + labels[i] +
                            " DontSave1=" + GetMember(dialog, "DontSave1") +
                            " DontSave2=" + GetMember(dialog, "DontSave2") +
                            " DontSave=" + GetMember(dialog, "DontSave"));
                        if (_saveSmoke && i == 1)
                        {
                            Check(!(bool)GetMember(dialog, "DontSave1") &&
                                !(bool)GetMember(dialog, "DontSave2") &&
                                !(bool)GetMember(dialog, "DontSave"),
                                "Original BinaryWindow ROI save guards all pass");
                            var trackerText = GetMember(dialog, "txt_TrackerName") as TextBox;
                            Check(trackerText != null, "Original tracker name edit field exists");
                            trackerText.Text = editedTrackerName;
                            Check(trackerText.Text == editedTrackerName,
                                "Real Shunt CCD1 editor accepts tracker modification");
                        }
                        dialog.Close();
                        Check(!dialog.IsVisible, labels[i] + " image window closed");
                        if (_saveSmoke && i == 1)
                            Check(FirstTrackerName(nativeParam) == editedTrackerName,
                                "Original BinaryWindow closing writes to ParamFile");
                    }
                    finally { ((IDisposable)bmp).Dispose(); }
                }

                if (_saveSmoke)
                {
                    MethodInfo save = _main.GetType().GetMethod("SaveAllRecipe");
                    Check(save != null, "Original ToolsWindow.SaveAllRecipe exists");
                    save.Invoke(_main, null);
                    Check(true, "Original SaveAllRecipe completed");
                    object reopened = Activator.CreateInstance(station.GetType());
                    object[] recipeArgs = { _recipe, 0, "" };
                    station.GetType().GetMethod("SetRecipe").Invoke(reopened, recipeArgs);
                    Check(FirstTrackerName(GetMember(reopened, "paramFile")) ==
                        editedTrackerName,
                        "New CVInspection reloads binary name from saved Recipe");
                }
                else
                    for (int i = 0; i < 8; i++)
                        Check(before[i] == Hash(settings[i]),
                            "S" + (i + 1) + " Shunt Recipe copy unchanged");
                ExitCode = 0;
            }
            catch (Exception ex)
            {
                _log.AppendLine("FAIL " + ex.GetBaseException());
                ExitCode = 63;
            }
            finally
            {
                if (_saveSmoke)
                {
                    try
                    {
                        foreach (var file in originalFiles)
                            File.WriteAllBytes(file.Key, file.Value);
                        foreach (string path in new[] { folder, publicDb })
                        {
                            if (!Directory.Exists(path)) continue;
                            foreach (string file in Directory.GetFiles(path, "*",
                                SearchOption.AllDirectories))
                                if (!originalFiles.ContainsKey(file)) File.Delete(file);
                        }
                        Check(originalFiles.All(x =>
                            File.ReadAllBytes(x.Key).SequenceEqual(x.Value)),
                            "Shunt isolation Recipe and PublicDB exact byte rollback");
                    }
                    catch (Exception rollbackError)
                    {
                        _log.AppendLine("FAIL ROLLBACK " + rollbackError);
                        ExitCode = 63;
                    }
                }
                try {
                    File.WriteAllText(Path.Combine(root, _saveSmoke ?
                        "r3-shunt-binary-save-result.txt" : "r3-shunt-image-result.txt"),
                        _log.ToString(), new UTF8Encoding(false));
                } catch { }
                _main.Close();
            }
        }
    }
}
