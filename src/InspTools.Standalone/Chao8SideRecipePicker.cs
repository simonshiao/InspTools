using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace InspTools.Standalone
{
    // The selectable/searchable RecipeWindow flow is ported from
    // D:\delta\Chao8Side\src\Chao8Side.Main\SystemSource\UI\RecipeWindow.
    // This standalone version intentionally does not load the machine MainWindow,
    // barcode writers, PLC or the original machine's mutable FileSystem singleton.
    internal sealed class Chao8SideRecipePicker
    {
        private readonly Window _host;
        private readonly TextBlock _status;
        private readonly ComboBox _station;
        private readonly Button _locate;
        private readonly Button _save;
        private readonly bool _portable;
        private UIElement _original;
        private Button _originalLocate;
        private R3IsolatedEditor _editor;
        private LightweightRecipePreview _preview;
        private string _activeRecipe;
        private bool _loaded;
        private bool _loadFailed;
        // 保留原版裝飾按鈕的命中設定，避免實驗模式誤開啟遮罩層。
        private readonly Dictionary<ButtonBase, bool> _originalHitTestStates =
            new Dictionary<ButtonBase, bool>();

        private Chao8SideRecipePicker(Window host, TextBlock status,
            ComboBox station, Button locate, Button save)
        {
            _host = host;
            _status = status;
            _station = station;
            _locate = locate;
            _save = save;
            _portable = NativeR3RecipeSession.IsPortableReview();
        }

        internal static void Attach(Window host)
        {
            Grid original = host.Content as Grid;
            if (original == null)
                throw new InvalidOperationException("找不到原版 InspTools 的 Grid 根節點");

            // 直接擴充原版 grd_Main，保留所有原版按鈕位置與事件。
            var choose = RecipeIconButton();
            var station = new ComboBox { IsEnabled = false };
            for (int i = 1; i <= 8; i++) station.Items.Add("S" + i);
            station.SelectedIndex = 0;
            var locate = Button("定位設定", 98);
            var save = Button("儲存 Recipe", 115);
            var status = new TextBlock();

            var picker = new Chao8SideRecipePicker(host, status, station, locate, save);
            picker._original = original;
            picker._originalLocate = host.FindName("btn_LocateTool") as Button;
            if (picker._originalLocate == null)
            {
                FieldInfo field = host.GetType().GetField("btn_LocateTool",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) picker._originalLocate = field.GetValue(host) as Button;
            }
            choose.Click += delegate { picker.ShowPicker(); };
            // CHAO owns the AI runtime. The original robot button is kept in
            // the original WPF Grid, but routes to our independent AI panel
            // rather than the vendor's licensed AI sidebar.
            ChaoSelfAiBridge.Attach(host, GetNativeButton(host, "btn_AI_Function"),
                () => picker._loaded);
            host.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                    return;
                if (!picker._loaded || picker._editor == null)
                    return;
                if (e.Key == Key.O)
                {
                    // The original "Open" button is hidden in RemoteMode.
                    // Route the shortcut to its compiled WPF Click handler,
                    // which opens the vendor's own image picker and CVBuffer path.
                    var nativeOpen = GetNativeButton(host, "btn_UseOpenBitmap");
                    if (nativeOpen != null)
                    {
                        e.Handled = true;
                        nativeOpen.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, nativeOpen));
                    }
                    return;
                }
                if (e.Key != Key.S) return;
                e.Handled = true;
                // Keep the original SaveAllRecipe, backup and disk reread;
                // original dialog close events already updated native ParamFile.
                picker._editor.SaveNativeChanges();
            };
            // The original stage buttons own StationIndex; the window title must
            // follow their completed Click rather than preserve the initial S1.
            for (int i = 1; i <= 8; i++)
            {
                var stageButton = GetNativeButton(host, "btn_Stage" + i);
                if (stageButton == null) continue;
                stageButton.Click += delegate
                {
                    host.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ChaoSelfAiBridge.SynchronizeStationTitle(host, picker._loaded);
                    }));
                };
            }
            station.SelectionChanged += delegate { picker.ChangeStation(); };
            locate.Click += delegate
            {
                if (picker._editor != null)
                {
                    picker._editor.OpenStation(picker._station.SelectedIndex + 1);
                    picker._save.IsEnabled = picker._editor.CanSave;
                }
            };
            save.Click += delegate
            {
                if (picker._editor != null)
                {
                    picker._editor.SaveChanges();
                    picker._save.IsEnabled = picker._editor.CanSave;
                }
            };
            // Keep original WPF Click -> OpenBinaryTool -> BinaryWindow,
            // regardless of whether input comes from a mouse or UI Automation.
            // The vendor dialog itself gets a full-frame offline buffer only
            // after Loaded; no location/inspect success flags are overridden.
            EventManager.RegisterClassHandler(typeof(Window),
                FrameworkElement.LoadedEvent, new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    var nativeDialog = sender as Window;
                    if (nativeDialog == null ||
                        nativeDialog.GetType().FullName != "InspTools.AOI.BinaryWindow" ||
                        !picker._loaded || picker._editor == null || !picker._portable)
                        return;
                    host.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new Action(delegate { picker.PrepareNativeBinaryOffline(2); }));
                }), true);
            // The original decoded GUI assumes fully initialized station/tool objects.
            // Its visible controls must not execute against an uninitialized shell.
            host.Loaded += delegate
            {
                CaptureOriginalInputs(original, picker._originalHitTestStates);
                LightweightRecipePreview.RestrictOriginalEditing(original);
                var copy = GetNativeButton(host, "btn_ParamCopy");
                var stage8 = GetNativeButton(host, "btn_Stage8");
                var stage9 = GetNativeButton(host, "btn_Stage9");
                var stage10 = GetNativeButton(host, "btn_Stage10");
                if (copy == null || stage8 == null || stage9 == null || stage10 == null)
                    throw new InvalidOperationException("原版站別或複製參數按鈕不存在，無法安全插入 Recipe");
                // 8 站版的原版 Stage9/Stage10 固定 Hidden，正式主按鈕
                // 可以使用其保留區域；不移動任何原版 Button 或其 Click。
                if (stage9.Visibility == Visibility.Visible || stage10.Visibility == Visibility.Visible)
                    throw new InvalidOperationException("第 9／10 站目前可見，停止放置 Recipe 以免重疊");
                double x = copy.Margin.Left - choose.Width - 10;
                if (x < stage8.Margin.Left + stage8.Width + 8)
                    throw new InvalidOperationException("Recipe 和第 8 站間距不足，不允許覆蓋原版按鈕");
                choose.Margin = new Thickness(x,
                    copy.Margin.Top + (copy.Height - choose.Height) / 2, 0, 0);
                original.Children.Add(choose);
            };
        }

        private static Button Button(string text, double width)
        {
            return new Button { Content = text, Width = width, Height = 32,
                Margin = new Thickness(0, 0, 6, 0), Padding = new Thickness(8, 3, 8, 3) };
        }

        private static Button GetNativeButton(Window host, string name)
        {
            var field = host.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(host) as Button;
        }

        private void PrepareNativeBinaryOffline(int retries)
        {
            var dialog = Application.Current.Windows.Cast<Window>()
                .FirstOrDefault(w => w.IsVisible &&
                    w.GetType().FullName == "InspTools.AOI.BinaryWindow");
            if (dialog == null)
            {
                if (retries > 0)
                    _host.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new Action(delegate { PrepareNativeBinaryOffline(retries - 1); }));
                return;
            }
            try
            {
                const BindingFlags flags = BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic;
                int stationIndex = Convert.ToInt32(_host.GetType()
                    .GetField("StationIndex", flags).GetValue(_host));
                Array stations = _host.GetType().GetField("Station", flags)
                    .GetValue(_host) as Array;
                if (stations == null || stationIndex < 0 || stationIndex >= 8)
                    return;
                object station = stations.GetValue(stationIndex);
                object buffers = station.GetType().GetField("buffers", flags)
                    .GetValue(station);
                object src = buffers.GetType().GetProperty("Src", flags)
                    .GetValue(buffers, null);
                if ((bool)src.GetType().GetMethod("Empty").Invoke(src, null))
                    return; // No image: retain the native preview behavior.
                object located = src.GetType().GetMethod("Clone", Type.EmptyTypes)
                    .Invoke(src, null);
                buffers.GetType().GetProperty("LocatedSrc", flags)
                    .SetValue(buffers, located, null);
                int width = (int)src.GetType().GetProperty("Width").GetValue(src, null);
                int height = (int)src.GetType().GetProperty("Height").GetValue(src, null);
                Type rect = src.GetType().Assembly.GetType("OpenCvSharp.Rect", true);
                object fullFrame = Activator.CreateInstance(rect,
                    new object[] { 0, 0, width, height });
                dialog.GetType().GetMethod("SetPreviewMode", new[] { typeof(bool) })
                    .Invoke(dialog, new object[] { false });
                dialog.GetType().GetMethods()
                    .First(m => m.Name == "SetBuffer" && m.GetParameters().Length == 3)
                    .Invoke(dialog, new object[] { src, fullFrame, true });
                // The original editor's ROI guards remain authoritative.
                foreach (string name in new[] { "DontSave", "DontSave1", "DontSave2" })
                {
                    FieldInfo guard = dialog.GetType().GetField(name, flags);
                    if (guard != null && Convert.ToBoolean(guard.GetValue(dialog)))
                        throw new InvalidOperationException(
                            "原版二值化離線 ROI 防呆未通過：" + name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(dialog, ex.GetBaseException().Message,
                    "原版二值化離線影像未就緒",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static Button RecipeIconButton()
        {
            var folder = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M3,6 L9,6 L11,9 L21,9 L21,20 L3,20 Z"),
                Stroke = Brush("#2563A7"),
                StrokeThickness = 1.6,
                Fill = Brush("#D8E8F8"),
                StrokeLineJoin = PenLineJoin.Round
            };
            var icon = new Viewbox { Width = 30, Height = 30, Child = folder };
            var button = new Button
            {
                Name = "btn_StandaloneRecipe",
                Content = icon,
                Width = 48,
                Height = 48,
                Padding = new Thickness(0),
                Background = Brush("#FFFFFFFF"),
                BorderBrush = Brush("#FF9B9B9B"),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ToolTip = "選擇 Recipe"
            };
            AutomationProperties.SetName(button, "選擇 Recipe");
            return button;
        }

        private static SolidColorBrush Brush(string value)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        private void ShowPicker()
        {
            var dialog = new Window { Title = "Recipe 管理｜Chao8Side 選擇介面",
                Owner = _host, Width = 1080, Height = 530, MinWidth = 860, MinHeight = 470,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brush("#F5F7FA"), ShowInTaskbar = false };
            var root = new Grid { Margin = new Thickness(15) };
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            var left = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
            var import = Button("匯入 Recipe", 143);
            var duplicate = Button("新增／另存", 143);
            var reload = Button("重新整理", 143);
            foreach (Button b in new[] { import, duplicate, reload })
            {
                b.Margin = new Thickness(0, 0, 0, 10);
                b.Height = 42;
                left.Children.Add(b);
            }
            left.Children.Add(new TextBlock { Text = "來源 Recipe 只讀\n匯入後保存於本軟體\n不會變更原廠檔案",
                TextWrapping = TextWrapping.Wrap, Foreground = Brush("#667085"),
                Margin = new Thickness(2, 10, 0, 0) });
            root.Children.Add(left);

            var center = new Grid { Margin = new Thickness(0, 0, 12, 0) };
            center.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            center.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            center.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            center.RowDefinitions.Add(new RowDefinition());
            center.Children.Add(new TextBlock { Text = "Recipe 資料夾", FontSize = 19,
                FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 10) });
            var sourceBar = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            sourceBar.ColumnDefinitions.Add(new ColumnDefinition());
            sourceBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            sourceBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            sourceBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var rootPathInput = new TextBox { Height = 34, FontSize = 12,
                Padding = new Thickness(7, 7, 4, 3), Text = StandaloneRecipeStore.Root,
                ToolTip = "Recipe 根資料夾；預設為軟體旁的 Recipe，可貼上其他路徑" };
            sourceBar.Children.Add(rootPathInput);
            var browseRoot = Button("瀏覽…", 65);
            browseRoot.Height = 34;
            Grid.SetColumn(browseRoot, 1); sourceBar.Children.Add(browseRoot);
            var applyRoot = Button("套用", 55);
            applyRoot.Height = 34;
            Grid.SetColumn(applyRoot, 2); sourceBar.Children.Add(applyRoot);
            var defaultRoot = Button("預設", 55);
            defaultRoot.Height = 34;
            Grid.SetColumn(defaultRoot, 3); sourceBar.Children.Add(defaultRoot);
            Grid.SetRow(sourceBar, 1); center.Children.Add(sourceBar);
            var search = new TextBox { Height = 36, FontSize = 15, Padding = new Thickness(8, 5, 8, 5),
                ToolTip = "搜尋 Recipe 名稱", Margin = new Thickness(0, 0, 0, 8) };
            Grid.SetRow(search, 2); center.Children.Add(search);
            var list = new ListBox { FontSize = 16, Padding = new Thickness(6) };
            Grid.SetRow(list, 3); center.Children.Add(list);
            Grid.SetColumn(center, 1); root.Children.Add(center);

            var details = new Grid { Margin = new Thickness(6, 0, 0, 0) };
            details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            details.RowDefinitions.Add(new RowDefinition());
            details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var title = new TextBlock { Text = "Current Recipe", FontSize = 19,
                FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 10) };
            details.Children.Add(title);
            var current = new TextBlock { Text = _activeRecipe ?? "尚未選擇", FontSize = 15,
                Margin = new Thickness(0, 0, 0, 12), Foreground = Brush("#334155") };
            Grid.SetRow(current, 1); details.Children.Add(current);
            var sourceKind = new TextBlock { Margin = new Thickness(0, 0, 0, 12),
                FontSize = 13, Foreground = Brush("#57677C"),
                Text = "來源：本軟體 Recipe（可編輯）" };
            Grid.SetRow(sourceKind, 2); details.Children.Add(sourceKind);
            var info = new TextBlock { Text = "請在清單選擇 Recipe", TextWrapping = TextWrapping.Wrap,
                FontSize = 13, Foreground = Brush("#57677C") };
            Grid.SetRow(info, 3); details.Children.Add(info);
            var footer = new StackPanel { Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = Button("取消", 78);
            var load = Button("選取並載入", 118);
            load.IsEnabled = false;
            footer.Children.Add(cancel);
            footer.Children.Add(load);
            Grid.SetRow(footer, 4); details.Children.Add(footer);
            Grid.SetColumn(details, 2); root.Children.Add(details);
            dialog.Content = root;

            string selectedRoot = Path.GetFullPath(StandaloneRecipeStore.Root);
            string localRoot = Path.GetFullPath(StandaloneRecipeStore.Root);
            Func<bool> localSelected = () => string.Equals(
                selectedRoot.TrimEnd('\\'), localRoot.TrimEnd('\\'),
                StringComparison.OrdinalIgnoreCase);
            Func<string[]> filtered = () => StandaloneRecipeStore.ListFromRoot(selectedRoot)
                .Where(n => n.IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            Action refresh = delegate
            {
                list.ItemsSource = filtered();
                load.IsEnabled = false;
                sourceKind.Text = localSelected() ? "來源：本軟體 Recipe（可編輯）" :
                    "來源：外部資料夾（唯讀，載入時複製）";
                info.Text = "Recipe 根目錄：\n" + selectedRoot +
                    "\n\n找到 " + StandaloneRecipeStore.ListFromRoot(selectedRoot).Length +
                    " 個有效 Recipe。\n選擇後可查看詳情。" +
                    (localSelected() ? "" : "\n原廠母本不會直接修改。");
            };
            Action<string> changeRoot = delegate(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentException("請指定 Recipe 根資料夾");
                string candidate = Path.GetFullPath(path.Trim().Trim('"'));
                if (!Directory.Exists(candidate))
                    throw new DirectoryNotFoundException("Recipe 根目錄不存在：" + candidate);
                if ((File.GetAttributes(candidate) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("不允許使用連結類型的 Recipe 來源目錄。");
                if (File.Exists(Path.Combine(candidate, "Setting.ini")))
                    throw new InvalidOperationException(
                        "選到的是單一 Recipe，請選取上一層的 Recipe 根資料夾。");
                selectedRoot = candidate;
                rootPathInput.Text = selectedRoot;
                refresh();
            };
            applyRoot.Click += delegate
            {
                try { changeRoot(rootPathInput.Text); }
                catch (Exception ex) { Warn(dialog, ex); }
            };
            defaultRoot.Click += delegate { changeRoot(localRoot); };
            browseRoot.Click += delegate
            {
                try
                {
                    using (var folder = new Forms.FolderBrowserDialog())
                    {
                        folder.Description = "選取包含多個 Recipe 的根資料夾";
                        folder.SelectedPath = Directory.Exists(selectedRoot) ? selectedRoot :
                            localRoot;
                        folder.ShowNewFolderButton = false;
                        if (folder.ShowDialog() == Forms.DialogResult.OK)
                            changeRoot(folder.SelectedPath);
                    }
                }
                catch (Exception ex) { Warn(dialog, ex); }
            };
            search.TextChanged += delegate { refresh(); };
            list.SelectionChanged += delegate
            {
                var name = list.SelectedItem as string;
                load.IsEnabled = !string.IsNullOrWhiteSpace(name) && !_loaded && !_loadFailed;
                info.Text = name == null ? "請選擇 Recipe" :
                    "Recipe:\n" + name + "\n\nRecipe Path:\n" +
                    Path.Combine(selectedRoot, name) +
                    "\n\n" + (localSelected() ? "本地副本，可載入。" :
                    "來源僅供選擇。按「選取並載入」會先匯入本機副本。");
            };
            cancel.Click += delegate { dialog.Close(); };
            reload.Click += delegate { refresh(); };
            import.Click += delegate
            {
                try
                {
                    var file = new OpenFileDialog { Title = "選取 Recipe 的 Setting.ini",
                        Filter = "Setting.ini|Setting.ini", CheckFileExists = true };
                    if (file.ShowDialog(dialog) != true) return;
                    string source = Path.GetDirectoryName(file.FileName);
                    string name = Path.GetFileName(source);
                    StandaloneRecipeStore.Import(source, name);
                    selectedRoot = localRoot;
                    rootPathInput.Text = localRoot;
                    refresh();
                    list.SelectedItem = name;
                }
                catch (Exception ex) { Warn(dialog, ex); }
            };
            duplicate.Click += delegate
            {
                try
                {
                    string selected = list.SelectedItem as string;
                    if (!localSelected() || selected == null)
                        throw new InvalidOperationException("請先選取本軟體內的 Recipe 再複製新增");
                    string name = PromptName(dialog, selected + "-Copy");
                    if (name == null) return;
                    StandaloneRecipeStore.Duplicate(selected, name);
                    refresh();
                    list.SelectedItem = name;
                }
                catch (Exception ex) { Warn(dialog, ex); }
            };
            Action activate = delegate
            {
                try
                {
                    string selected = list.SelectedItem as string;
                    if (selected == null) return;
                    if (!localSelected())
                    {
                        string importedName = selected;
                        if (Directory.Exists(StandaloneRecipeStore.LocalPath(importedName)))
                        {
                            importedName = PromptName(dialog, selected + "-Imported");
                            if (importedName == null) return;
                        }
                        StandaloneRecipeStore.Import(
                            Path.Combine(selectedRoot, selected), importedName);
                        selectedRoot = localRoot;
                        rootPathInput.Text = localRoot;
                        refresh();
                        list.SelectedItem = importedName;
                        selected = importedName;
                        MessageBox.Show(dialog, "已複製到本軟體 Recipe；來源維持唯讀。",
                            "Recipe 匯入", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    StandaloneRecipeStore.Validate(selected);
                    if (MessageBox.Show(dialog, "確定載入「" + selected +
                        "」？\n僅在本軟體離線載入；不連接相機／PLC、不執行 AI。",
                        "選擇 Recipe", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        != MessageBoxResult.Yes) return;
                    Load(selected);
                    dialog.Close();
                }
                catch (Exception ex) { Warn(dialog, ex); }
            };
            load.Click += delegate { activate(); };
            list.MouseDoubleClick += delegate { if (load.IsEnabled) activate(); };
            refresh();
            dialog.ShowDialog();
        }

        private void Load(string recipe)
        {
            if (_loaded) throw new InvalidOperationException("已載入 Recipe，請重啟後切換");
            if (_loadFailed) throw new InvalidOperationException("上次原生 Recipe 初始化失敗，請重新啟動程式後再載入");
            try
            {
                if (_portable)
                {
                    NativeR3RecipeSession.Open(_host, recipe, _station.SelectedIndex + 1);
                    _editor = R3IsolatedEditor.Attach(_host, recipe);
                    if (_originalLocate == null)
                        throw new InvalidOperationException("找不到原版定位按鈕");
                    // Recipe 與 Throughput 初始化成功後，恢復原生事件綁定；
                    // 不能再只放行定位，其餘圖示因 IsHitTestVisible 被永久封鎖。
                    RestoreOriginalInputs(_original, _originalHitTestStates);
                    FixNestedOriginalToolButtons();
                    _locate.IsEnabled = true;
                    _save.IsEnabled = false;
                }
                else
                {
                    _preview = LightweightRecipePreview.Load(_host, recipe, _station.SelectedIndex + 1);
                    _preview.AttachStationMenu();
                }
            }
            catch
            {
                _loadFailed = true;
                _status.Text = "Recipe 初始化未完成，請關閉程式後重新啟動，不要在同一工作階段重試";
                throw;
            }
            _loaded = true;
            _activeRecipe = recipe;
            _station.IsEnabled = true;
            _status.Text = recipe + (_portable ?
                "｜八站離線設定已載入；AI／Inspect 尚未驗收" :
                "｜唯讀預覽；請使用 R3 攜帶版編輯");
        }

        private void FixNestedOriginalToolButtons()
        {
            bool trace = NativeR3RecipeSession.OriginalInputExperimentEnabled();
            string[,] mapping = {
                { "btn_LocateTool", "Btn_LocateTool_Click" },
                { "btn_SelectWayTool", "Btn_SelectWayTool_Click" },
                { "btn_SelectFaceTool", "Btn_SelectFaceTool_Click" },
                { "btn_EnhanceTool", "Btn_EnhanceTool_Click" },
                { "btn_AIProphet", "btn_AIProphet_Click" },
                { "btn_AIEnhanceTool", "Btn_AIEnhanceTool_Click" },
                { "btn_SmoothTool", "Btn_SmoothTool_Click" },
                { "btn_BinaryTool", "Btn_BinaryTool_Click" },
                { "btn_MaskTool", "Btn_MaskTool_Click" },
                { "btn_MeasureTool", "Btn_MeasureTool_Click" },
                { "btn_AdvancedLocate", "Btn_AdvancedLocate_Click" },
                { "btn_BlockAvgTool", "Btn_BlockAvgTool_Click" },
                { "btn_DifferenceTool", "Btn_DifferenceTool_Click" },
                { "btn_LineAnalysis", "Btn_LineAnalysis_Click" },
                { "btn_MatchTool", "Btn_MatchTool_Click" },
                { "btn_AIPredictTool", "Btn_AIPredictTool_Click" },
                { "btn_IRTool", "btn_IRTool_Click" },
                { "btn_FontTool", "Btn_FontTool_Click" }
            };
            string[] innerNames = { "btn_BitmapSave_Copy", "btn_BitmapSave_Copy1", "btn_BitmapSave_Copy6",
                "btn_BitmapSave_Copy2", "btn_BitmapSave_Copy17", "btn_BitmapSave_Copy15", "btn_BitmapSave_Copy5",
                "btn_BitmapSave_Copy4", "btn_BitmapSave_Copy3", "btn_BitmapSave_Copy12", "btn_BitmapSave_Copy7",
                "btn_BitmapSave_Copy8", "btn_BitmapSave_Copy9", "btn_BitmapSave_Copy11", "btn_BitmapSave_Copy10",
                "btn_BitmapSave_Copy13", "btn_BitmapSave_Copy14", "btn_BitmapSave_Copy16" };
            for (int i = 0; i < mapping.GetLength(0); i++)
            {
                FieldInfo field = _host.GetType().GetField(mapping[i, 0],
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo handler = _host.GetType().GetMethod(mapping[i, 1],
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var button = field == null ? null : field.GetValue(_host) as Button;
                if (trace)
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original-input-experiment.log"),
                        mapping[i, 0] + " field=" + (button != null) + " handler=" + (handler != null) + Environment.NewLine);
                if (button == null || handler == null) continue;
                FieldInfo innerField = _host.GetType().GetField(innerNames[i],
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                // Copy* 是蓋在原版工具按鈕中央的裝飾層，沒有原版 Click 回呼。
                // 讓滑鼠穿透裝飾層，由原版 Button 自己執行已綁定的 Click。
                var decoration = innerField == null ? null : innerField.GetValue(_host) as Button;
                if (decoration != null && !ReferenceEquals(decoration, button))
                    decoration.IsHitTestVisible = false;
                if (!trace) continue;
                string originalHandler = handler.Name;
                button.PreviewMouseLeftButtonDown += delegate(object sender, System.Windows.Input.MouseButtonEventArgs e)
                {
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original-input-experiment.log"),
                        "hit native " + originalHandler + Environment.NewLine);
                };
            }
        }

        private static void CaptureOriginalInputs(DependencyObject node,
            IDictionary<ButtonBase, bool> states)
        {
            var button = node as ButtonBase;
            if (button != null && !states.ContainsKey(button))
                states.Add(button, button.IsHitTestVisible);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                CaptureOriginalInputs(VisualTreeHelper.GetChild(node, i), states);
        }

        private static void RestoreOriginalInputs(DependencyObject node,
            IDictionary<ButtonBase, bool> states)
        {
            var button = node as ButtonBase;
            bool enabled;
            if (button != null && states.TryGetValue(button, out enabled))
                button.IsHitTestVisible = enabled;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                RestoreOriginalInputs(VisualTreeHelper.GetChild(node, i), states);
        }

        private static bool RestrictToLocate(DependencyObject node, DependencyObject target)
        {
            if (ReferenceEquals(node, target))
            {
                RestoreHitTesting(node);
                return true;
            }
            bool containsTarget = false;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                containsTarget |= RestrictToLocate(VisualTreeHelper.GetChild(node, i), target);
            var element = node as UIElement;
            if (element != null) element.IsHitTestVisible = containsTarget;
            return containsTarget;
        }

        private static void RestoreHitTesting(DependencyObject node)
        {
            var element = node as UIElement;
            if (element != null) element.IsHitTestVisible = true;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
                RestoreHitTesting(VisualTreeHelper.GetChild(node, i));
        }

        private void ChangeStation()
        {
            if (!_loaded || _station.SelectedIndex < 0) return;
            int number = _station.SelectedIndex + 1;
            try
            {
                if (_preview != null) _preview.SelectStation(number);
                else _host.GetType().GetMethod("SetStationIndex",
                    BindingFlags.Public | BindingFlags.Instance).Invoke(_host, new object[] { number - 1 });
                ChaoSelfAiBridge.SynchronizeStationTitle(_host, _loaded);
                _status.Text = _activeRecipe + " / S" + number +
                    "｜離線設定；AI／Inspect 尚未驗收";
            }
            catch (Exception ex) { Warn(_host, ex); }
        }

        private static string PromptName(Window owner, string value)
        {
            var w = new Window { Title = "新增／另存 Recipe", Owner = owner,
                Width = 390, Height = 165, ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowInTaskbar = false };
            var panel = new StackPanel { Margin = new Thickness(15) };
            panel.Children.Add(new TextBlock { Text = "新的 Recipe 名稱（不覆蓋現有檔案）" });
            var input = new TextBox { Text = value, Margin = new Thickness(0, 7, 0, 10) };
            panel.Children.Add(input);
            var ok = Button("建立", 78);
            ok.Click += delegate { w.DialogResult = true; };
            panel.Children.Add(ok); w.Content = panel;
            w.Loaded += delegate { input.Focus(); input.SelectAll(); };
            return w.ShowDialog() == true ? input.Text.Trim() : null;
        }

        private static void Warn(Window owner, Exception ex)
        {
            MessageBox.Show(owner, ex.GetBaseException().Message,
                "Recipe 操作失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
