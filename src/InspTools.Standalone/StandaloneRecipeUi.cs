using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace InspTools.Standalone
{
    /// <summary>Local recipe package commands added to the original ToolsWindow menu.</summary>
    internal static class StandaloneRecipeUi
    {
        internal static void AddMenuItems(ContextMenu menu, Window owner, Action refresh)
        {
            var nativeParams = new MenuItem { Header = "查看八站原生 Recipe 參數（不載入 AI）" };
            nativeParams.SubmenuOpened += delegate
            {
                nativeParams.Items.Clear();
                foreach (string recipe in StandaloneRecipeStore.List())
                {
                    string selected = recipe;
                    var item = new MenuItem { Header = selected };
                    item.Click += delegate
                    {
                        RunWithErrorDialog(owner, delegate
                        {
                            MessageBox.Show(owner, NativeRecipeProbe.ReadOnlySummary(selected),
                                "Recipe 參數資訊", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    };
                    nativeParams.Items.Add(item);
                }
                if (nativeParams.Items.Count == 0)
                    nativeParams.Items.Add(new MenuItem { Header = "尚未匯入 Recipe", IsEnabled = false });
            };
            menu.Items.Add(nativeParams);

            var editBasic = new MenuItem { Header = "編輯八站 Recipe 基本開關（原生存檔）" };
            editBasic.SubmenuOpened += delegate
            {
                editBasic.Items.Clear();
                foreach (string recipeName in StandaloneRecipeStore.List())
                {
                    string selected = recipeName;
                    var item = new MenuItem { Header = selected };
                    item.Click += delegate { RunWithErrorDialog(owner, delegate
                    {
                        ShowBasicFlagEditor(owner, selected);
                    }); };
                    editBasic.Items.Add(item);
                }
                if (editBasic.Items.Count == 0)
                    editBasic.Items.Add(new MenuItem
                    {
                        Header = "請先匯入 Recipe",
                        IsEnabled = false
                    });
            };
            menu.Items.Add(editBasic);

            var copyMenu = new MenuItem { Header = "新增／另存 Recipe（複製）" };
            copyMenu.SubmenuOpened += delegate
            {
                copyMenu.Items.Clear();
                foreach (string recipeName in StandaloneRecipeStore.List())
                {
                    string selected = recipeName;
                    var item = new MenuItem { Header = selected };
                    item.Click += delegate
                    {
                        string destination = PromptName(owner, selected + "-Copy");
                        if (destination == null) return;
                        RunWithErrorDialog(owner, delegate
                        {
                            StandaloneRecipeStore.Duplicate(selected, destination);
                            refresh();
                            MessageBox.Show(owner,
                                "新 Recipe 已保存在本軟體的 Recipe 資料夾。\n尚未載入模型或執行檢測。",
                                "Recipe 已建立");
                        });
                    };
                    copyMenu.Items.Add(item);
                }
                if (copyMenu.Items.Count == 0)
                    copyMenu.Items.Add(new MenuItem { Header = "請先匯入一份完整 Recipe", IsEnabled = false });
            };
            menu.Items.Add(copyMenu);

            var import = new MenuItem { Header = "匯入 Chao8Side／InspTools Recipe..." };
            import.Click += delegate
            {
                var chooser = new OpenFileDialog
                {
                    Title = "選取來源 Recipe 的 Setting.ini（來源不會被修改）",
                    Filter = "Recipe Setting.ini|Setting.ini",
                    CheckFileExists = true,
                    Multiselect = false
                };
                if (chooser.ShowDialog(owner) != true) return;
                string sourceDir = Path.GetDirectoryName(chooser.FileName);
                string destination = PromptName(owner, Path.GetFileName(sourceDir));
                if (destination == null) return;
                RunWithErrorDialog(owner, delegate
                {
                    StandaloneRecipeStore.Import(sourceDir, destination);
                    refresh();
                    MessageBox.Show(owner, "Recipe 已匯入本機，不會修改來源。", "Recipe 匯入完成");
                });
            };
            menu.Items.Add(import);
        }

        private static void RunWithErrorDialog(Window owner, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                MessageBox.Show(owner, ex.GetBaseException().Message,
                    "Recipe 操作失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void ShowBasicFlagEditor(Window owner, string recipe)
        {
            // This is an intentionally small Recipe settings dialog, NOT a
            // replacement for the vendor's original AOI tool child windows.
            var dialog = new Window
            {
                Title = "Recipe 基本開關 - " + recipe,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 470,
                Height = 390,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false
            };
            var stack = new StackPanel { Margin = new Thickness(20) };
            stack.Children.Add(new TextBlock
            {
                Text = "修改本程式的本地 Recipe，儲存時使用原廠 ParamFile。",
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = "AI 勾選只代表設定旗標，不代表模型已載入。",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 14)
            });
            stack.Children.Add(new TextBlock { Text = "站別" });
            var station = new ComboBox { Margin = new Thickness(0, 5, 0, 12), Width = 180,
                HorizontalAlignment = HorizontalAlignment.Left };
            for (int i = 1; i <= 8; i++) station.Items.Add("S" + i);
            stack.Children.Add(station);

            string[] keys = { "LocateToolON", "BinaryToolON", "AIEnhanceToolON", "AIProphetToolON" };
            string[] labels = { "定位工具", "二值化", "AI 前處理", "預分類" };
            var checks = new CheckBox[4];
            for (int i = 0; i < checks.Length; i++)
            {
                checks[i] = new CheckBox { Content = labels[i], Margin = new Thickness(0, 5, 0, 5) };
                stack.Children.Add(checks[i]);
            }
            Action loadStation = delegate
            {
                int index = station.SelectedIndex + 1;
                if (index < 1) return;
                Dictionary<string, bool> original = NativeRecipeProbe.ReadEditableFlags(recipe, index);
                for (int i = 0; i < checks.Length; i++)
                    checks[i].IsChecked = original[keys[i]];
            };
            station.SelectionChanged += delegate { RunWithErrorDialog(dialog, loadStation); };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 14, 0, 0)
            };
            var cancel = new Button { Content = "取消", Width = 80,
                Margin = new Thickness(0, 0, 10, 0) };
            var save = new Button { Content = "儲存參數", Width = 100, IsDefault = true };
            cancel.Click += delegate { dialog.DialogResult = false; };
            save.Click += delegate
            {
                RunWithErrorDialog(dialog, delegate
                {
                    int index = station.SelectedIndex + 1;
                    if (index < 1) throw new InvalidOperationException("請先選擇站別");
                    if (MessageBox.Show(dialog,
                        "寫回本機 Recipe「" + recipe + "」的 S" + index +
                        " 設定？儲存前會備份原始 station.ini。",
                        "確認儲存 Recipe", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        != MessageBoxResult.Yes) return;
                    var desired = new Dictionary<string, bool>(StringComparer.Ordinal);
                    for (int i = 0; i < checks.Length; i++)
                        desired.Add(keys[i], checks[i].IsChecked == true);
                    string backup = NativeRecipeProbe.WriteEditableFlags(recipe, index, desired);
                    MessageBox.Show(dialog,
                        backup == "UNCHANGED" ? "設定沒有變更。" :
                        "已使用原版 ParamFile 寫入並重新讀取驗證。\n備份：" + backup +
                        "\n請重新選擇 Recipe 預覽以更新主視窗。",
                        "Recipe 儲存結果", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.DialogResult = true;
                });
            };
            buttons.Children.Add(cancel);
            buttons.Children.Add(save);
            stack.Children.Add(buttons);
            dialog.Content = stack;
            station.SelectedIndex = 0;
            dialog.ShowDialog();
        }

        private static string PromptName(Window owner, string suggested)
        {
            var dialog = new Window
            {
                Title = "Recipe 名稱",
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 420,
                Height = 170,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false
            };
            var stack = new StackPanel { Margin = new Thickness(16) };
            stack.Children.Add(new TextBlock { Text = "另存為（不覆蓋已有 Recipe）：" });
            var input = new TextBox { Text = suggested, Margin = new Thickness(0, 8, 0, 12) };
            stack.Children.Add(input);
            var actions = new StackPanel { Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "取消", Width = 72, Margin = new Thickness(0, 0, 8, 0) };
            var ok = new Button { Content = "建立", Width = 72, IsDefault = true };
            cancel.Click += delegate { dialog.DialogResult = false; };
            ok.Click += delegate { dialog.DialogResult = true; };
            actions.Children.Add(cancel);
            actions.Children.Add(ok);
            stack.Children.Add(actions);
            dialog.Content = stack;
            dialog.Loaded += delegate { input.Focus(); input.SelectAll(); };
            return dialog.ShowDialog() == true ? input.Text : null;
        }
    }
}
