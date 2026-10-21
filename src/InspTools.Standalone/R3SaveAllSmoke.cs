using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InspTools.Standalone
{
    /// <summary>
    /// Destructive TEST ONLY in a disposable sandbox; every Recipe file is
    /// snapshotted before SaveAllRecipe and restored in finally.
    /// Never use on the phase-1 archive, R1 release or production Recipes.
    /// </summary>
    internal sealed class R3SaveAllSmoke
    {
        private readonly Window _window;
        private readonly string _recipe;
        private readonly StringBuilder _log = new StringBuilder();
        internal int ExitCode { get; private set; }

        private R3SaveAllSmoke(Window window, string recipe)
        {
            _window = window;
            _recipe = recipe;
            ExitCode = 59;
        }

        internal static R3SaveAllSmoke Attach(Window window, string recipe)
        {
            NativeR3RecipeSession.Open(window, recipe, 5); // Gate path + exact R3 SHA
            var smoke = new R3SaveAllSmoke(window, recipe);
            window.Loaded += delegate {
                window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(smoke.Run));
            };
            return smoke;
        }

        private void Check(bool ok, string label)
        {
            if (!ok) throw new InvalidDataException(label);
            _log.AppendLine("PASS " + label);
        }

        private void Run()
        {
            string baseFolder = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string recipeFolder = StandaloneRecipeStore.Validate(_recipe);
            string backupFolder = Path.Combine(baseFolder, "_r3_saveall_backup");
            var originals = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            // Record preexisting Recipe/DB files. No other folders may be modified.
            foreach (string folder in new[] { recipeFolder,
                Path.Combine(baseFolder, "PublicDB") })
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                    originals[file] = File.ReadAllBytes(file);
            }
            Directory.CreateDirectory(backupFolder);
            foreach (KeyValuePair<string, byte[]> item in originals)
            {
                // Copy only to the local sandbox in case the process terminates.
                string dest = Path.Combine(backupFolder,
                    (item.Key.StartsWith(recipeFolder, StringComparison.OrdinalIgnoreCase)
                        ? "Recipe" : "PublicDB"),
                    Path.GetFileName(item.Key));
                if (File.Exists(dest)) throw new IOException("Backup filename collision: " + dest);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.WriteAllBytes(dest, item.Value);
            }

            bool verified = false;
            try
            {
                FieldInfo stationsField = _window.GetType().GetField("Station");
                Array stations = stationsField.GetValue(_window) as Array;
                Check(stations != null && stations.Length == 8,
                    "Original ToolsWindow has eight CVInspection objects");
                object fifth = stations.GetValue(4);
                Type cv = fifth.GetType();
                FieldInfo field = cv.GetField("paramFile");
                object param = field.GetValue(fifth);
                FieldInfo binary = param.GetType().GetField("BinaryToolON");
                bool initial = Convert.ToBoolean(binary.GetValue(param));
                binary.SetValue(param, !initial);
                Check(Convert.ToBoolean(binary.GetValue(param)) != initial,
                    "S5 native ParamFile edited in memory");

                // First verified real editor: LocateWindow's FocusLimit textbox.
                // BinaryWindow's DontSave1 safety guard is active with no image;
                // do not bypass that guard to fabricate an edit success.
                Type locateType = _window.GetType().Assembly.GetType(
                    "InspTools.AOI.LocateWindow", true);
                Window dialog = Activator.CreateInstance(locateType) as Window;
                Check(dialog != null, "Original LocateWindow editor constructed");
                locateType.GetMethod("SetInspection", new[] { cv })
                    .Invoke(dialog, new[] { fifth });
                locateType.GetMethod("SetPreviewMode", new[] { typeof(bool) })
                    .Invoke(dialog, new object[] { false });
                FieldInfo focusField = locateType.GetField("txt_FocusLimit",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                TextBox focusBox = focusField == null ? null : focusField.GetValue(dialog) as TextBox;
                Check(focusBox != null, "Original LocateWindow FocusLimit textbox bound");
                double originalFocus = GetLocateFocusLimit(param);
                double changedFocus = originalFocus + 1.0;
                dialog.Owner = _window;
                dialog.Show();
                focusBox.Text = changedFocus.ToString(CultureInfo.InvariantCulture);
                Check(focusBox.Text == changedFocus.ToString(CultureInfo.InvariantCulture),
                    "Original LocateWindow textbox accepts edit");
                dialog.Close(); // Vendor Window_Closed validates UI and writes ParamFile.
                Check(Math.Abs(GetLocateFocusLimit(param) - changedFocus) < 0.00001,
                    "Original LocateWindow UI edit reached ParamFile");

                MethodInfo save = _window.GetType().GetMethod("SaveAllRecipe");
                Check(save != null, "Original ToolsWindow.SaveAllRecipe exists");
                save.Invoke(_window, null);
                Check(true, "Original SaveAllRecipe returned");

                var reread = NativeRecipeProbe.ReadEditableFlags(_recipe, 5);
                Check(reread["BinaryToolON"] == !initial,
                    "S5 independent disk reread contains changed flag");

                // Recreate an actual inspection object and read the saved Recipe.
                object reopened = Activator.CreateInstance(cv);
                object[] args = { _recipe, 4, string.Empty };
                cv.GetMethod("SetRecipe").Invoke(reopened, args);
                bool loaded = Convert.ToBoolean(binary.GetValue(field.GetValue(reopened)));
                Check(loaded == !initial, "new CVInspection loads saved S5 flag");
                Check(Math.Abs(GetLocateFocusLimit(field.GetValue(reopened)) - changedFocus) < 0.00001,
                    "new CVInspection reloads original LocateWindow edit");
                verified = true;
            }
            catch (Exception ex)
            {
                _log.AppendLine("FAIL " + ex.GetBaseException());
            }
            finally
            {
                bool rollbackOk = true;
                try
                {
                    foreach (KeyValuePair<string, byte[]> item in originals)
                        File.WriteAllBytes(item.Key, item.Value);
                    foreach (string folder in new[] { recipeFolder,
                        Path.Combine(baseFolder, "PublicDB") })
                    {
                        if (!Directory.Exists(folder)) continue;
                        foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                            if (!originals.ContainsKey(file))
                                File.Delete(file); // Only this disposable sandbox.
                    }
                    foreach (KeyValuePair<string, byte[]> item in originals)
                    {
                        if (!File.ReadAllBytes(item.Key).SequenceEqual(item.Value))
                            throw new InvalidDataException("Byte-for-byte rollback failed: " + item.Key);
                    }
                    _log.AppendLine("PASS exact rollback of Recipe and PublicDB files");
                }
                catch (Exception ex)
                {
                    rollbackOk = false;
                    _log.AppendLine("FAIL ROLLBACK: " + ex);
                }
                ExitCode = verified && rollbackOk ? 0 : 59;
                try
                {
                    File.WriteAllText(Path.Combine(baseFolder, "r3-saveall-result.txt"),
                        _log.ToString(), new UTF8Encoding(false));
                }
                catch { }
                _window.Close();
            }
        }

        private static double GetLocateFocusLimit(object param)
        {
            FieldInfo locateField = param.GetType().GetField("LocateParam",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (locateField == null) throw new MissingFieldException("ParamFile.LocateParam");
            object locate = locateField.GetValue(param);
            FieldInfo focus = locate.GetType().GetField("FocusLimit",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (focus == null) throw new MissingFieldException("LocateParam.FocusLimit");
            return Convert.ToDouble(focus.GetValue(locate), CultureInfo.InvariantCulture);
        }
    }
}
