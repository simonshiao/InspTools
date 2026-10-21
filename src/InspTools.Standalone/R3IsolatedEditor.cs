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

        internal static void Attach(Window main, string recipe)
        {
            // The caller must first succeed in NativeR3RecipeSession.Open(),
            // which verifies AppBase, recipe prefix and exact patched DLL SHA.
            var editor = new R3IsolatedEditor(main, recipe);
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
        }

        private void SafeInvoke(Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                MessageBox.Show(_main, ex.GetBaseException().Message,
                    "R3 隔離 Recipe 操作失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Edit(int station)
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
            Type dialogType = _main.GetType().Assembly.GetType("InspTools.AOI.LocateWindow", true);
            Window dialog = Activator.CreateInstance(dialogType) as Window;
            if (dialog == null) throw new InvalidOperationException("原版 LocateWindow 建構失敗");
            dialogType.GetMethod("SetInspection", new[] { target.GetType() })
                .Invoke(dialog, new[] { target });
            dialogType.GetMethod("SetPreviewMode", new[] { typeof(bool) })
                .Invoke(dialog, new object[] { false });
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
            if (!_dirty) return;
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
