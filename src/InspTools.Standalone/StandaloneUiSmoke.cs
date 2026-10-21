using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace InspTools.Standalone
{
    /// <summary>
    /// Bounded in-process WPF integration test. It deliberately does not
    /// load models, create inspection engines, or write Recipe parameters.
    /// This is only reachable with the explicit --ui-smoke argument.
    /// </summary>
    internal sealed class StandaloneUiSmoke
    {
        private readonly Window _window;
        private readonly string _recipe;
        private readonly StringBuilder _log = new StringBuilder();
        internal int ExitCode { get; private set; }

        internal StandaloneUiSmoke(Window window, string recipe)
        {
            _window = window;
            _recipe = recipe;
            ExitCode = 42;
        }

        internal void Attach()
        {
            _window.Loaded += delegate
            {
                _window.Dispatcher.BeginInvoke(
                    DispatcherPriority.ApplicationIdle, new Action(Run));
            };
        }

        private static MenuItem FindMenu(ContextMenu menu, string prefix)
        {
            var item = menu.Items.OfType<MenuItem>()
                .FirstOrDefault(x => Convert.ToString(x.Header).StartsWith(prefix,
                    StringComparison.Ordinal));
            if (item == null) throw new InvalidDataException("找不到選單：" + prefix);
            return item;
        }

        private void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
            _log.AppendLine("PASS " + message);
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream file = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        }

        private void Run()
        {
            string target = Path.Combine(StandaloneRecipeStore.Validate(_recipe), "station5.ini");
            string before = Hash(target);
            try
            {
                ContextMenu menu = _window.ContextMenu;
                Check(menu != null, "原版視窗附加 Recipe ContextMenu");
                MenuItem viewer = FindMenu(menu, "查看八站原生 Recipe 參數");
                viewer.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent, viewer));
                Check(viewer.Items.OfType<MenuItem>()
                    .Any(x => Equals(x.Header, _recipe)), "八站原生參數檢視選單有測試 Recipe");

                MenuItem recipeSelector = FindMenu(menu, "原版 GUI 預覽 Recipe");
                Check(recipeSelector.Items.OfType<MenuItem>()
                    .Any(x => Equals(x.Header, _recipe)), "原版 Recipe 清單包含隔離副本");

                MenuItem editor = FindMenu(menu, "編輯八站 Recipe 基本開關");
                editor.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent, editor));
                MenuItem entry = editor.Items.OfType<MenuItem>()
                    .FirstOrDefault(x => Equals(x.Header, _recipe));
                Check(entry != null, "Recipe 基本參數編輯選單已綁定");

                LightweightRecipePreview preview =
                    LightweightRecipePreview.Load(_window, _recipe, 1);
                preview.AttachStationMenu();
                MenuItem stations = FindMenu(menu, "預覽切換 S1～S8");
                Check(stations.Items.Count == 8, "八站切換選單共有 8 站");
                for (int station = 1; station <= 8; station++)
                {
                    preview.SelectStation(station);
                    Check(_window.Title.EndsWith("/ S" + station,
                        StringComparison.Ordinal), "原版站別視窗對應 S" + station);
                }
                preview.SelectStation(5);
                LightweightRecipePreview.RestrictOriginalEditing(_window);

                // Enter the actual modal editor through the WPF menu's Click
                // handler; a dispatcher callback inspects S1/S5 and cancels it.
                bool inspected = false;
                _window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(delegate
                    {
                        Window dialog = Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w != _window &&
                                w.Title.StartsWith("Recipe 基本開關", StringComparison.Ordinal));
                        if (dialog == null)
                            throw new InvalidDataException("Recipe 基本參數編輯視窗未出現");
                        try
                        {
                            StackPanel body = dialog.Content as StackPanel;
                            Check(body != null, "編輯視窗具備 WPF 表單");
                            ComboBox selector = body.Children.OfType<ComboBox>().FirstOrDefault();
                            Check(selector != null && selector.Items.Count == 8,
                                "編輯視窗可選擇 S1～S8");
                            Check(selector.SelectedIndex == 0, "預設載入 S1");
                            CheckBoxes(body, true);
                            selector.SelectedIndex = 4;
                            CheckBoxes(body, false);
                            inspected = true;
                        }
                        finally
                        {
                            dialog.Close(); // No save. No production writes.
                        }
                    }));
                entry.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, entry));
                Check(inspected, "編輯視窗開啟、切換站別、取消關閉");
                Check(before == Hash(target), "隔離 station5.ini 保持不變");
                ExitCode = 0;
            }
            catch (Exception ex)
            {
                _log.AppendLine("FAIL " + ex.GetBaseException());
                ExitCode = 42;
            }
            finally
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "ui-smoke-result.txt");
                try { File.WriteAllText(file, _log.ToString(), new UTF8Encoding(false)); }
                catch { }
                _window.Close();
            }
        }

        private void CheckBoxes(StackPanel body, bool binaryExpected)
        {
            CheckBox[] checks = body.Children.OfType<CheckBox>().ToArray();
            Check(checks.Length == 4, "編輯表單具有四項原生旗標");
            Check(checks[1].IsChecked == binaryExpected,
                binaryExpected ? "S1 二值化為開" : "S5 二值化為關");
        }
    }
}
