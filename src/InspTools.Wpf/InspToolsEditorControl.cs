using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InspTools.SDK;

namespace InspTools.Wpf
{
    public sealed class InspToolsEditorControl : UserControl
    {
        private readonly TextBlock sessionText = new TextBlock();
        private readonly TextBlock pageText = new TextBlock();
        private readonly TextBlock resultText = new TextBlock { TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock statusText = new TextBlock();
        private readonly TextBlock dirtyText = new TextBlock();
        private readonly Image previewImage = new Image { Stretch = Stretch.Uniform };
        private readonly ComboBox sectionBox = new ComboBox();
        private readonly DataGrid parameterGrid = new DataGrid();
        private readonly StackPanel recoveredPanel = new StackPanel();
        private readonly TabControl editorTabs = new TabControl();
        private readonly ObservableCollection<ToolChoice> tools = new ObservableCollection<ToolChoice>();
        private readonly ObservableCollection<ParameterRow> parameters = new ObservableCollection<ParameterRow>();
        private IInspToolsSession session;
        private string currentSection = string.Empty;
        private RecoveredToolPolicy currentPolicy;
        private string lastEditedParameterKey = string.Empty;
        private bool loading;

        public InspToolsEditorControl()
        {
            MinWidth = 980;
            MinHeight = 600;
            sectionBox.ItemsSource = tools;
            sectionBox.DisplayMemberPath = "Display";
            sectionBox.SelectionChanged += OnSectionChanged;
            AutomationProperties.SetName(sectionBox, "Recovered Recipe tool section");

            parameterGrid.ItemsSource = parameters;
            parameterGrid.AutoGenerateColumns = false;
            parameterGrid.CanUserAddRows = false;
            parameterGrid.CanUserDeleteRows = false;
            parameterGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
            parameterGrid.Columns.Add(new DataGridTextColumn { Header = "Parameter", IsReadOnly = true, Width = new DataGridLength(1.2, DataGridLengthUnitType.Star), Binding = new Binding("Key") });
            parameterGrid.Columns.Add(new DataGridTextColumn { Header = "Value", Width = new DataGridLength(1.4, DataGridLengthUnitType.Star), Binding = new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged } });
            AutomationProperties.SetName(parameterGrid, "Recipe section parameters");

            Content = BuildLayout();
            RefreshSession();
        }

        public event EventHandler SaveRequested;
        public event EventHandler CancelRequested;

        public IInspToolsSession Session
        {
            get { return session; }
            set { session = value; currentSection = string.Empty; RefreshSession(); }
        }

        public string CurrentSection => currentSection;
        public int VisibleParameterCount => parameters.Count;
        public bool HasRecoveredParityPolicy => currentPolicy != null;
        public bool HasDedicatedRecoveredEditor => currentPolicy != null && currentPolicy.DedicatedEditor;
        public int EditableRecoveredParameterCount => currentPolicy == null
            ? 0
            : parameters.Count(x => currentPolicy.CanEdit(x.Key));

        public bool IsVisibleParameterEditable(string key)
        {
            if (session == null || !session.Editable || currentPolicy == null || string.IsNullOrWhiteSpace(key))
                return false;
            return currentPolicy.CanEdit(key) && parameters.Any(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        public bool SelectToolSection(string section)
        {
            RequireSession();
            ToolChoice choice = tools.FirstOrDefault(x => string.Equals(x.Section, section, StringComparison.OrdinalIgnoreCase));
            if (choice == null) return false;
            loading = true;
            sectionBox.SelectedItem = choice;
            LoadSection(choice);
            loading = false;
            return true;
        }

        public string GetVisibleParameterValue(string key)
        {
            ParameterRow row = parameters.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (row == null) throw new KeyNotFoundException("Visible parameter was not found: " + key);
            return row.Value;
        }

        public void SetVisibleParameterValue(string key, string value)
        {
            RequireSession();
            if (!session.Editable) throw new InvalidOperationException("Session is read-only.");
            if (currentPolicy == null)
                throw new InvalidOperationException("This Tool is a read-only generic fallback until parity evidence is available: " + currentSection);
            if (!currentPolicy.CanEdit(key))
                throw new InvalidOperationException("Recovered parity policy keeps this parameter read-only: " + key);
            ParameterRow row = parameters.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (row == null) throw new KeyNotFoundException("Visible parameter was not found: " + key);
            row.Value = value ?? string.Empty;
            MarkEdited(row);
            parameterGrid.Items.Refresh();
        }

        public ToolPreviewResult TestCurrentTool()
        {
            RequireSession();
            ApplyRows();
            ParameterRow row = !string.IsNullOrWhiteSpace(lastEditedParameterKey)
                ? parameters.FirstOrDefault(x => string.Equals(x.Key, lastEditedParameterKey, StringComparison.OrdinalIgnoreCase))
                : null;
            row = row ?? parameterGrid.SelectedItem as ParameterRow ?? parameters.FirstOrDefault();
            if (row == null) throw new InvalidOperationException("Selected section has no parameters.");
            ToolPreviewResult result = session.TestTool(currentSection + "/" + row.Key);
            resultText.Text = "PARAMETER PREVIEW / NOT AOI TEST · " + (result.Success ? "PASS" : "INFO") + " · " + currentSection + "/" + row.Key + " · " + result.Message;
            UpdateDirty();
            return result;
        }

        public void SaveChanges()
        {
            RequireSession();
            ApplyRows();
            session.Save();
            statusText.Text = "已保存 Session；production Recipe 未寫回。";
            ReloadCurrent();
            UpdateDirty();
        }

        public void CancelChanges()
        {
            RequireSession();
            session.Cancel();
            statusText.Text = "已取消未保存修改。";
            ReloadCurrent();
            UpdateDirty();
        }

        private UIElement BuildLayout()
        {
            var root = new Grid { Background = Brush("#F4F6F8") };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel();
            header.Children.Add(new TextBlock { Text = "InspTools Editor · Recovered Tool Workspace", FontSize = 20, FontWeight = FontWeights.SemiBold });
            header.Children.Add(sessionText);
            header.Children.Add(pageText);
            var headerCard = Card(header, 16);
            Grid.SetRow(headerCard, 0);
            root.Children.Add(headerCard);

            var body = new Grid { Margin = new Thickness(14) };
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });

            var nav = new StackPanel();
            nav.Children.Add(new TextBlock { Text = "Tool / Section", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 7) });
            nav.Children.Add(sectionBox);
            nav.Children.Add(new TextBlock { Text = "來源為前一期已解譯的 Recipe section；不載入 Replica。", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0) });
            var navCard = Card(nav, 14);
            Grid.SetColumn(navCard, 0);
            body.Children.Add(navCard);

            var imageCard = new Border { Background = Brush("#111820"), CornerRadius = new CornerRadius(8), Padding = new Thickness(10), Child = previewImage };
            Grid.SetColumn(imageCard, 2);
            body.Children.Add(imageCard);

            var edit = new Grid();
            edit.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            edit.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            edit.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            edit.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var title = new TextBlock { Text = "Recovered Tool editor", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 7) };
            recoveredPanel.Margin = new Thickness(2);
            var recoveredScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = recoveredPanel };
            editorTabs.Items.Add(new TabItem { Header = "Recovered controls", Content = recoveredScroll });
            editorTabs.Items.Add(new TabItem { Header = "Raw parameters", Content = parameterGrid });
            var test = new Button { Content = "Parameter preview (NOT AOI Test)", Height = 42, Margin = new Thickness(0, 10, 0, 0) };
            test.Click += delegate { try { TestCurrentTool(); } catch (Exception ex) { resultText.Text = ex.GetBaseException().Message; } };
            var resultCard = new Border { Background = Brush("#F4F6F8"), CornerRadius = new CornerRadius(6), Padding = new Thickness(9), Margin = new Thickness(0, 8, 0, 0), Child = resultText };
            Grid.SetRow(title, 0); Grid.SetRow(editorTabs, 1); Grid.SetRow(test, 2); Grid.SetRow(resultCard, 3);
            edit.Children.Add(title); edit.Children.Add(editorTabs); edit.Children.Add(test); edit.Children.Add(resultCard);
            var editCard = Card(edit, 14);
            Grid.SetColumn(editCard, 4);
            body.Children.Add(editCard);
            Grid.SetRow(body, 1);
            root.Children.Add(body);

            var cancel = new Button { Content = "取消修改", Width = 100, Height = 42, Margin = new Thickness(0, 0, 8, 0) };
            cancel.Click += delegate { try { CancelChanges(); CancelRequested?.Invoke(this, EventArgs.Empty); } catch (Exception ex) { statusText.Text = ex.GetBaseException().Message; } };
            var save = new Button { Content = "保存設定", Width = 100, Height = 42 };
            save.Click += delegate { try { SaveChanges(); SaveRequested?.Invoke(this, EventArgs.Empty); } catch (Exception ex) { statusText.Text = ex.GetBaseException().Message; } };
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            buttons.Children.Add(cancel); buttons.Children.Add(save);
            var state = new StackPanel(); state.Children.Add(statusText); state.Children.Add(dirtyText);
            var footer = new DockPanel(); DockPanel.SetDock(state, Dock.Left); DockPanel.SetDock(buttons, Dock.Right); footer.Children.Add(state); footer.Children.Add(buttons);
            var footerCard = Card(footer, 12); Grid.SetRow(footerCard, 2); root.Children.Add(footerCard);
            return root;
        }

        private void RefreshSession()
        {
            tools.Clear(); parameters.Clear();
            if (session == null) { sessionText.Text = "尚未綁定 Session"; previewImage.Source = null; return; }
            RecipeParameterSnapshot snapshot = session.GetRecipeSnapshot();
            sessionText.Text = "Recipe=" + session.RecipeName + " · S" + session.Station + " · " + (session.Editable ? "SESSION EDIT · MEMORY ONLY" : "READ ONLY") + " · SHA=" + ShortHash(snapshot.Sha256);
            parameterGrid.IsReadOnly = !session.Editable;
            LoadImage(session.ImagePath);
            foreach (ToolDescriptor item in session.GetToolCatalog()) tools.Add(new ToolChoice(item.Family, item.Section, item.ParameterCount));
            if (!SelectToolSection("Locate") && tools.Count > 0) SelectToolSection(tools[0].Section);
            UpdateDirty();
        }

        private void OnSectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading || session == null) return;
            ToolChoice choice = sectionBox.SelectedItem as ToolChoice;
            if (choice == null) return;
            if (currentSection.Length > 0 && session.Editable) ApplyRows();
            LoadSection(choice);
        }

        private void LoadSection(ToolChoice choice)
        {
            currentSection = choice.Section;
            lastEditedParameterKey = string.Empty;
            parameters.Clear();
            foreach (RecipeParameterEntry entry in session.GetSectionParameters(choice.Section)) parameters.Add(new ParameterRow(entry.Key, entry.Value));
            if (parameters.Count > 0) parameterGrid.SelectedIndex = 0;
            currentPolicy = RecoveredToolPolicies.Resolve(choice.Family, choice.Section);
            // Raw Recipe is an evidence/debug viewer from IR2-04 onward.  Writable
            // controls exist only in an evidence-backed recovered policy panel.
            parameterGrid.IsReadOnly = true;
            BuildRecoveredPanel(choice);
            pageText.Text = "Recovered mapping: " + RecoveredPage(choice.Family) + " · [" + choice.Section + "] · " + parameters.Count + " parameters" +
                            (currentPolicy == null ? " · GENERIC FALLBACK" : " · IR2-04 PARITY POLICY");
            resultText.Text = currentPolicy == null
                ? "此頁沿用 IR2-03 generic SDK/Core fallback；Raw parameters 僅供查閱，沒有 parity evidence 不開寫入。"
                : currentPolicy.Warning;
        }

        private void ApplyRows()
        {
            if (session == null || !session.Editable || currentSection.Length == 0) return;
            if (currentPolicy == null) return;
            parameterGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            parameterGrid.CommitEdit(DataGridEditingUnit.Row, true);
            foreach (ParameterRow row in parameters)
            {
                if (!currentPolicy.CanEdit(row.Key)) continue;
                session.SetParameter(currentSection, row.Key, row.Value ?? string.Empty);
            }
        }

        private void BuildRecoveredPanel(ToolChoice choice)
        {
            recoveredPanel.Children.Clear();
            if (currentPolicy == null)
            {
                recoveredPanel.Children.Add(new TextBlock
                {
                    Text = "此 Tool 尚未進入 IR2-04 專用 parity slice。Raw parameters 僅供查閱；目前仍屬 IR2-03 generic read-only fallback。",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(4)
                });
                editorTabs.SelectedIndex = 1;
                return;
            }

            recoveredPanel.Children.Add(new TextBlock
            {
                Text = currentPolicy.RecoveredPage + " · " + currentPolicy.Evidence,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(4, 4, 4, 8)
            });
            recoveredPanel.Children.Add(new TextBlock
            {
                Text = currentPolicy.Warning,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brush("#6A4B00"),
                Margin = new Thickness(4, 0, 4, 10)
            });

            if (!currentPolicy.DedicatedEditor)
            {
                recoveredPanel.Children.Add(new Border
                {
                    Background = Brush("#F4F6F8"),
                    Padding = new Thickness(10),
                    Child = new TextBlock
                    {
                        Text = "READ ONLY GATE · 尚未建立有證據的專用 editor，因此不允許從 generic raw grid 推測寫入 lifecycle。",
                        TextWrapping = TextWrapping.Wrap
                    }
                });
                editorTabs.SelectedIndex = 0;
                return;
            }

            foreach (RecoveredToolFieldGroup group in currentPolicy.Groups)
            {
                var groupPanel = new StackPanel { Margin = new Thickness(0, 2, 0, 8) };
                foreach (RecoveredToolField field in group.Fields)
                {
                    ParameterRow row = parameters.FirstOrDefault(x => string.Equals(x.Key, field.Key, StringComparison.OrdinalIgnoreCase));
                    if (row == null) continue;
                    groupPanel.Children.Add(BuildRecoveredField(field, row));
                }
                if (groupPanel.Children.Count == 0) continue;
                recoveredPanel.Children.Add(new GroupBox
                {
                    Header = group.Title,
                    Padding = new Thickness(8),
                    Margin = new Thickness(2, 0, 2, 8),
                    Content = groupPanel
                });
            }
            editorTabs.SelectedIndex = 0;
        }

        private UIElement BuildRecoveredField(RecoveredToolField field, ParameterRow row)
        {
            var line = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            line.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            line.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var label = new TextBlock { Text = field.Label, VerticalAlignment = VerticalAlignment.Center, ToolTip = field.Key };
            Grid.SetColumn(label, 0);
            line.Children.Add(label);

            bool enabled = session != null && session.Editable && currentPolicy != null && currentPolicy.CanEdit(field.Key);
            if (field.Kind == RecoveredFieldKind.Boolean)
            {
                bool value;
                var check = new CheckBox
                {
                    IsChecked = bool.TryParse(row.Value, out value) && value,
                    IsEnabled = enabled,
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = field.Key
                };
                check.Checked += delegate { row.Value = "True"; MarkEdited(row); };
                check.Unchecked += delegate { row.Value = "False"; MarkEdited(row); };
                Grid.SetColumn(check, 1);
                line.Children.Add(check);
            }
            else
            {
                var input = new TextBox { Text = row.Value, IsReadOnly = !enabled, ToolTip = field.Key };
                input.TextChanged += delegate { row.Value = input.Text ?? string.Empty; MarkEdited(row); };
                Grid.SetColumn(input, 1);
                line.Children.Add(input);
            }
            return line;
        }

        private void MarkEdited(ParameterRow row)
        {
            if (row == null) return;
            lastEditedParameterKey = row.Key ?? string.Empty;
            parameterGrid.SelectedItem = row;
        }

        private void ReloadCurrent()
        {
            ToolChoice choice = tools.FirstOrDefault(x => string.Equals(x.Section, currentSection, StringComparison.OrdinalIgnoreCase));
            if (choice != null) LoadSection(choice);
        }

        private void UpdateDirty() { dirtyText.Text = session == null ? string.Empty : session.IsDirty ? "尚有未保存 Session 修改" : "目前沒有未保存修改"; }

        private void LoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) { previewImage.Source = null; return; }
            using (var stream = File.OpenRead(path))
            {
                var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit(); image.Freeze(); previewImage.Source = image;
            }
        }

        private void RequireSession() { if (session == null) throw new InvalidOperationException("InspTools editor Session is not assigned."); }
        private static string ShortHash(string value) { return string.IsNullOrEmpty(value) || value.Length <= 12 ? value ?? string.Empty : value.Substring(0, 12); }
        private static Border Card(UIElement child, double padding) { return new Border { Background = Brushes.White, BorderBrush = Brush("#DDE3EA"), BorderThickness = new Thickness(1), Padding = new Thickness(padding), Child = child }; }
        private static Brush Brush(string value) { return (Brush)new BrushConverter().ConvertFromString(value); }

        private static string RecoveredPage(string family)
        {
            switch ((family ?? string.Empty).ToLowerInvariant())
            {
                case "locate": return "locatewindow.xaml"; case "advlocate": return "advlocatewindow.xaml"; case "measure": return "measurewindow.xaml";
                case "mask": return "maskwindow.xaml"; case "binary": return "binarywindow.xaml"; case "smooth": return "smoothtoolwindow.xaml";
                case "blockavg": return "blockavgtoolwindow.xaml"; case "difference": return "differencetoolwindow.xaml"; case "lineanalysis": return "lineanalysiswindow.xaml";
                case "match": return "matchtoolwindow.xaml"; case "aienhance": case "aipredict": return "aitoolwindow.xaml"; case "aiprophet": return "aiprophettoolwindow.xaml";
                case "font": return "fonttoolwindow.xaml"; case "ir": return "irtoolwindow.xaml"; default: return "recovered Recipe section";
            }
        }

        private sealed class ToolChoice
        {
            internal ToolChoice(string family, string section, int count) { Family = family ?? string.Empty; Section = section ?? string.Empty; Count = count; }
            public string Family { get; private set; } public string Section { get; private set; } public int Count { get; private set; }
            public string Display => Family + " · " + Section + " (" + Count + ")";
        }

        private sealed class ParameterRow
        {
            internal ParameterRow(string key, string value) { Key = key ?? string.Empty; Value = value ?? string.Empty; }
            public string Key { get; private set; } public string Value { get; set; }
        }
    }
}
