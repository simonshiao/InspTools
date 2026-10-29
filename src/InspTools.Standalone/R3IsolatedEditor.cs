using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace InspTools.Standalone
{
    /// <summary>
    /// Opt-in R3-only access to original LocateWindow. Editing remains in
    /// memory until an explicit snapshot-and-verify SaveAllRecipe action.
    /// Do not attach to R1, production Recipes or model-enabled AOI.
    /// </summary>
    internal sealed class R3IsolatedEditor
    {
        private readonly Window _main;
        private readonly string _recipe;
        private readonly MenuItem _save;
        private readonly MenuItem _undo;
        private Dictionary<string, byte[]> _prior;
        private bool _dirty;

        private R3IsolatedEditor(Window main, string recipe)
        {
            _main = main;
            _recipe = recipe;
            _save = new MenuItem { Header = "R3｜儲存 Recipe（備份＋驗證）", IsEnabled = false };
            _undo = new MenuItem { Header = "R3｜撤回上次儲存", IsEnabled = false };
        }

        internal static R3IsolatedEditor Attach(Window main, string recipe)
        {
            // The caller must first succeed in NativeR3RecipeSession.Open(),
            // which verifies AppBase, recipe prefix and exact patched DLL SHA.
            var editor = new R3IsolatedEditor(main, recipe);
            editor.RouteMainLocateButtonToIsolatedEditor();
            var menu = main.ContextMenu;
            if (menu == null) throw new InvalidOperationException("R3 ContextMenu 不存在");
            var edit = new MenuItem { Header = "R3｜原版定位工具編輯（隔離測試）" };
            for (int i = 1; i <= 8; i++)
            {
                int s = i;
                var item = new MenuItem { Header = "S" + s + " 定位設定" };
                item.Click += delegate { editor.SafeInvoke(() => editor.Edit(s)); };
                edit.Items.Add(item);
            }
            menu.Items.Insert(0, edit);
            menu.Items.Insert(1, editor._save);
            menu.Items.Insert(2, editor._undo);
            editor._save.Click += delegate { editor.SafeInvoke(editor.Save); };
            editor._undo.Click += delegate { editor.SafeInvoke(editor.Undo); };
            return editor;
        }

        private void RouteMainLocateButtonToIsolatedEditor()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic;
            FieldInfo buttonField = _main.GetType().GetField("btn_LocateTool", flags);
            Button button = buttonField == null ? null : buttonField.GetValue(_main) as Button;
            MethodInfo originalHandler = _main.GetType().GetMethod("Btn_LocateTool_Click", flags);
            if (button == null || originalHandler == null)
                throw new MissingMemberException("ToolsWindow.btn_LocateTool / Btn_LocateTool_Click");

            var original = (RoutedEventHandler)Delegate.CreateDelegate(
                typeof(RoutedEventHandler), _main, originalHandler);
            button.Click -= original;
            button.Click += delegate
            {
                SafeInvoke(delegate
                {
                    FieldInfo stationIndexField = _main.GetType().GetField("StationIndex", flags);
                    if (stationIndexField == null)
                        throw new MissingFieldException("ToolsWindow.StationIndex");
                    int station = Convert.ToInt32(stationIndexField.GetValue(_main)) + 1;
                    Edit(station);
                });
            };
        }

        internal void OpenStation(int station) { SafeInvoke(() => Edit(station)); }
        internal void OpenStation(int station, string[] imagePaths, int imageIndex)
        {
            SafeInvoke(() => Edit(station, imagePaths, imageIndex));
        }
        internal void OpenNativeTool(int station, string tool, string[] imagePaths, int imageIndex)
        {
            SafeInvoke(() => EditNativeTool(station, tool, imagePaths, imageIndex));
        }

        private void EditNativeTool(int station, string tool, string[] files, int index)
        {
            if (tool != "way" && tool != "face") throw new ArgumentException("Unknown tool");
            if (files == null || files.Length == 0) throw new InvalidDataException("請先在 Luma 載入圖庫。");
            files = files.Where(File.Exists).ToArray();
            if (files.Length == 0) throw new FileNotFoundException("找不到 Luma 圖片。");
            index = Math.Max(0, Math.Min(index, files.Length - 1));
            Array stations = _main.GetType().GetField("Station").GetValue(_main) as Array;
            object target = stations.GetValue(station - 1);
            Type dialogType = _main.GetType().Assembly.GetType(
                tool == "way" ? "InspTools.AOI.SelectWayWindow" : "InspTools.AOI.SelectFaceWindow", true);
            Window dialog = Activator.CreateInstance(dialogType) as Window;
            if (dialog == null) throw new InvalidOperationException("原廠工具視窗建立失敗。");
            var cvAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenCvSharp.dll"));
            Type matType = cvAssembly.GetType("OpenCvSharp.Mat", true);
            Type rectType = cvAssembly.GetType("OpenCvSharp.Rect", true);
            Type cvType = cvAssembly.GetType("OpenCvSharp.Cv2", true);
            Type modeType = cvAssembly.GetType("OpenCvSharp.ImreadModes", true);
            var imread = cvType.GetMethod("ImRead", new[] { typeof(string), modeType });
            var buffer = dialogType.GetMethod("SetBuffer", new[] { matType, rectType });
            if (buffer == null) throw new MissingMethodException("Native SelectWay/Face.SetBuffer");
            dialogType.GetMethod("LoadLanguage", new[] { typeof(int) })?.Invoke(dialog, new object[] { 1 });
            Type stationType = target.GetType();
            if (tool == "way")
                dialogType.GetMethod("SetInspection", new[] { stationType, typeof(int) })
                    .Invoke(dialog, new object[] { target, station - 1 });
            else
                dialogType.GetMethod("SetInspection", new[] { typeof(int), stationType })
                    .Invoke(dialog, new object[] { 0, target });
            object param = stationType.GetField("paramFile").GetValue(target);
            FieldInfo enabledField = param.GetType().GetField(tool == "way" ? "SelectWayON" : "SelectFaceON");
            if (enabledField == null || enabledField.FieldType != typeof(bool))
                throw new MissingFieldException("SelectWayON / SelectFaceON");
            var enabledCheck = new CheckBox
            {
                Content = tool == "way" ? "啟用選向" : "啟用選面",
                IsChecked = (bool)enabledField.GetValue(param),
                Margin = new Thickness(12, 7, 12, 7)
            };
            var saveButton = new Button { Content = "儲存 Recipe", Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(4) };
            var header = new DockPanel();
            DockPanel.SetDock(saveButton, Dock.Right);
            header.Children.Add(saveButton);
            header.Children.Add(enabledCheck);
            UIElement originalContent = dialog.Content as UIElement;
            if (originalContent == null) throw new InvalidOperationException("原版視窗內容遺失");
            dialog.Content = null;
            var wrapper = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            wrapper.Children.Add(header);
            wrapper.Children.Add(originalContent);
            dialog.Content = wrapper;
            enabledCheck.Checked += (sender, e) => enabledField.SetValue(param, true);
            enabledCheck.Unchecked += (sender, e) => enabledField.SetValue(param, false);
            saveButton.Click += (sender, e) => SaveNativeChanges();
            Action showImage = () =>
            {
                object mat = imread.Invoke(null, new[] { (object)files[index], Enum.Parse(modeType, "Color") });
                try
                {
                    int width = (int)matType.GetProperty("Width").GetValue(mat);
                    int height = (int)matType.GetProperty("Height").GetValue(mat);
                    if (width <= 0 || height <= 0) throw new InvalidDataException("圖片無效：" + files[index]);
                    object rect = Activator.CreateInstance(rectType, new object[] { 0, 0, width, height });
                    buffer.Invoke(dialog, new[] { mat, rect });
                    dialog.Title = (tool == "way" ? "選向" : "選面") +
                        " - " + (index + 1) + " / " + files.Length + "（PgUp／PgDn 切換）";
                }
                finally { (mat as IDisposable)?.Dispose(); }
            };
            dialog.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.PageDown || e.Key == System.Windows.Input.Key.PageUp)
                {
                    index = (index + (e.Key == System.Windows.Input.Key.PageDown ? 1 : -1) + files.Length) % files.Length;
                    showImage();
                    e.Handled = true;
                }
            };
            showImage();
            dialog.Owner = _main;
            dialog.ShowDialog();
        }
        internal void SaveChanges() { SafeInvoke(Save); }
        // Original dialogs commit their changes into ParamFile when closed.
        // Ctrl+S on the native host persists those edits without requiring
        // the alternative context-menu editor to mark its own _dirty bit.
        internal void SaveNativeChanges() { SafeInvoke(() => Save(true)); }
        internal bool CanSave { get { return _save.IsEnabled; } }

        private void SafeInvoke(Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                MessageBox.Show(_main, ex.GetBaseException().Message,
                    "R3 隔離 Recipe 操作失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Edit(int station) { Edit(station, null, 0); }

        private void Edit(int station, string[] imagePaths, int imageIndex)
        {
            if (_dirty)
            {
                MessageBox.Show(_main, "前一次修改尚未儲存，請先儲存或重啟程式捨棄修改。");
                return;
            }
            Array stations = _main.GetType().GetField("Station").GetValue(_main) as Array;
            if (stations == null || stations.Length != 8)
                throw new InvalidDataException("真實八站檢測物件不完整");
            object target = stations.GetValue(station - 1);

            // User-approved R3-only override: expose the original AI Locate
            // controls in the isolated editor. Keep inference off on open so
            // preview images cannot start model work implicitly. These are
            // in-memory values on the copied test station, never saved here.
            FieldInfo paramField = target.GetType().GetField("paramFile",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object paramFile = paramField == null ? null : paramField.GetValue(target);
            FieldInfo aiLocatePermission = paramFile == null ? null : paramFile.GetType().GetField(
                "AllowEnableAILocate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (aiLocatePermission == null || aiLocatePermission.FieldType != typeof(bool))
                throw new MissingFieldException("ParamFile.AllowEnableAILocate");
            aiLocatePermission.SetValue(paramFile, true);
            FieldInfo locateField = paramFile.GetType().GetField("LocateParam",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object locateParam = locateField == null ? null : locateField.GetValue(paramFile);
            FieldInfo aiEnabled = locateParam == null ? null : locateParam.GetType().GetField(
                "EnableAILocate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (aiEnabled == null || aiEnabled.FieldType != typeof(bool))
                throw new MissingFieldException("LocateParam.EnableAILocate");
            aiEnabled.SetValue(locateParam, false);
            if (locateParam.GetType().IsValueType)
                locateField.SetValue(paramFile, locateParam);

            Type dialogType = _main.GetType().Assembly.GetType("InspTools.AOI.LocateWindow", true);
            Window dialog = Activator.CreateInstance(dialogType) as Window;
            if (dialog == null) throw new InvalidOperationException("原版 LocateWindow 建構失敗");
            dialogType.GetMethod("SetInspection", new[] { target.GetType() })
                .Invoke(dialog, new[] { target });
            if (imagePaths != null && imagePaths.Length > 0)
            {
                imagePaths = imagePaths.Where(File.Exists).ToArray();
                if (imagePaths.Length > 0)
                {
                    imageIndex = Math.Max(0, Math.Min(imageIndex, imagePaths.Length - 1));
                    // Use the original LocateWindow image-source navigation rather
                    // than duplicating previous/next UI in the Luma host.
                    dialogType.GetMethod("SetImageSource").Invoke(dialog,
                        new object[] { 1, imagePaths, imageIndex, 0, 0, 0 });
                    var cvAssembly = Assembly.LoadFrom(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "OpenCvSharp.dll"));
                    Type matType = cvAssembly.GetType("OpenCvSharp.Mat", true);
                    Type cvType = cvAssembly.GetType("OpenCvSharp.Cv2", true);
                    Type modeType = cvAssembly.GetType("OpenCvSharp.ImreadModes", true);
                    var imread = cvType.GetMethod("ImRead", new[] { typeof(string), modeType });
                    object mat = imread.Invoke(null, new[] { (object)imagePaths[imageIndex],
                        Enum.Parse(modeType, "Color") });
                    try
                    {
                        dialogType.GetMethod("SetBuffer", new[] { matType })
                            .Invoke(dialog, new[] { mat });
                    }
                    finally { (mat as IDisposable)?.Dispose(); }
                }
            }
            dialog.Owner = _main;
            dialog.ShowDialog();
            _dirty = true; // The vendor dialog writes to ParamFile, not disk.
            _save.IsEnabled = true;
            MessageBox.Show(_main, "定位設定目前只修改記憶體；若要保存，使用右鍵 R3 儲存 Recipe。");
        }

        private IEnumerable<string> EditableRoots()
        {
            yield return StandaloneRecipeStore.Validate(_recipe);
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PublicDB");
        }

        private Dictionary<string, byte[]> Snapshot()
        {
            var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string dir in EditableRoots())
            {
                if (!Directory.Exists(dir)) continue;
                foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                        throw new IOException("Recipe 含連結：" + file);
                    files.Add(file, File.ReadAllBytes(file));
                }
            }
            return files;
        }

        private string PersistBackup(Dictionary<string, byte[]> original)
        {
            string backup = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_r3_backups",
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff"));
            foreach (var pair in original)
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!pair.Key.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("不在獨立 Runtime 下：" + pair.Key);
                string path = Path.Combine(backup, pair.Key.Substring(basePath.Length));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, pair.Value);
            }
            return backup;
        }

        private void Restore(Dictionary<string, byte[]> files)
        {
            foreach (string dir in EditableRoots())
            {
                if (!Directory.Exists(dir)) continue;
                foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    if (!files.ContainsKey(file)) File.Delete(file);
            }
            foreach (var pair in files) File.WriteAllBytes(pair.Key, pair.Value);
            if (files.Any(pair => !File.ReadAllBytes(pair.Key).SequenceEqual(pair.Value)))
                throw new IOException("Recipe 還原後檔案比對失敗");
        }

        private void Verify()
        {
            Array stations = _main.GetType().GetField("Station").GetValue(_main) as Array;
            Type paramType = stations.GetValue(0).GetType().GetField("paramFile")
                .GetValue(stations.GetValue(0)).GetType();
            Type iniType = paramType.Assembly.GetType("InspTools.HiniFile", true);
            var paramField = stations.GetValue(0).GetType().GetField("paramFile");
            var focusField = paramType.GetField("LocateParam");
            var binaryField = paramType.GetField("BinaryToolON");
            for (int i = 0; i < 8; i++)
            {
                object memory = paramField.GetValue(stations.GetValue(i));
                object saved = NativeRecipeProbe.OpenNativeParamFile(paramType, iniType, _recipe, i);
                if (!Equals(binaryField.GetValue(memory), binaryField.GetValue(saved)))
                    throw new InvalidDataException("S" + (i + 1) + " BinaryToolON 重讀不一致");
                object a = focusField.GetValue(memory), b = focusField.GetValue(saved);
                FieldInfo focus = a.GetType().GetField("FocusLimit");
                if (!Equals(focus.GetValue(a), focus.GetValue(b)))
                    throw new InvalidDataException("S" + (i + 1) + " FocusLimit 重讀不一致");
            }
        }

        private void Save()
        {
            Save(false);
        }

        private void Save(bool explicitlyRequested)
        {
            if (!_dirty && !explicitlyRequested) return;
            if (MessageBox.Show(_main,
                "確定寫回 R3 隔離 Recipe？將先備份，使用原版 SaveAllRecipe，再重新讀取驗證。",
                "確認隔離存檔", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;
            Dictionary<string, byte[]> snapshot = Snapshot();
            string backup = PersistBackup(snapshot);
            try
            {
                _main.GetType().GetMethod("SaveAllRecipe").Invoke(_main, null);
                Verify();
                _prior = snapshot;
                _dirty = false;
                _save.IsEnabled = false;
                _undo.IsEnabled = true;
                MessageBox.Show(_main, "存檔並重讀成功。備份在：" + backup);
            }
            catch
            {
                Restore(snapshot);
                throw;
            }
        }

        private void Undo()
        {
            if (_prior == null) return;
            if (MessageBox.Show(_main, "恢復本次啟動上次儲存之前的檔案？",
                "撤回存檔", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes) return;
            Restore(_prior);
            _prior = null;
            _undo.IsEnabled = false;
            MessageBox.Show(_main, "已恢復磁碟內容，請重啟 R3 程式讓記憶體同步。");
        }
    }
}
