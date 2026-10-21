using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using InspTools.SDK;

namespace InspTools.Wpf
{
    public enum AnnotationInputMode
    {
        PositivePoint,
        NegativePoint,
        Box
    }

    public sealed class AnnotationEditorControl : UserControl
    {
        private readonly Image baseImage = new Image { Stretch = Stretch.Uniform };
        private readonly Image maskImage = new Image { Stretch = Stretch.Uniform, IsHitTestVisible = false };
        private readonly Canvas promptCanvas = new Canvas { Background = Brushes.Transparent };
        private readonly TextBlock statusText = new TextBlock { TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock imageText = new TextBlock { TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock samText = new TextBlock { TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock classText = new TextBlock();
        private readonly ComboBox classBox = new ComboBox { Width = 150 };
        private readonly ComboBox mergeBox = new ComboBox { Width = 160 };
        private readonly Button positiveButton = new Button { Content = "+ Point", MinWidth = 82 };
        private readonly Button negativeButton = new Button { Content = "− Point", MinWidth = 82 };
        private readonly Button boxButton = new Button { Content = "Box", MinWidth = 82 };
        private readonly Button suggestButton = new Button { Content = "Suggest", MinWidth = 90 };
        private readonly Button commitButton = new Button { Content = "Commit", MinWidth = 90 };
        private readonly Button clearPromptsButton = new Button { Content = "Clear prompts", MinWidth = 104 };
        private readonly Button cancelButton = new Button { Content = "取消修改", Width = 100 };
        private readonly Button saveButton = new Button { Content = "儲存 X/Y", Width = 100 };
        private readonly List<AnnotationPrompt> prompts = new List<AnnotationPrompt>();
        private IAnnotationSession session;
        private AnnotationSuggestion suggestion;
        private AnnotationInputMode inputMode = AnnotationInputMode.Box;
        private Point? boxStart;

        public AnnotationEditorControl()
        {
            MinWidth = 980;
            MinHeight = 620;
            classBox.ItemsSource = Enumerable.Range(1, 6).Select(i => new ClassChoice(i)).ToArray();
            classBox.DisplayMemberPath = "Display";
            classBox.SelectedIndex = 0;
            mergeBox.ItemsSource = Enum.GetValues(typeof(AnnotationMergeMode));
            mergeBox.SelectedItem = AnnotationMergeMode.FillEmptyOnly;

            positiveButton.Click += delegate { InputMode = AnnotationInputMode.PositivePoint; };
            negativeButton.Click += delegate { InputMode = AnnotationInputMode.NegativePoint; };
            boxButton.Click += delegate { InputMode = AnnotationInputMode.Box; };
            suggestButton.Click += delegate { SafeAction(delegate { RunSuggestion(); }); };
            commitButton.Click += delegate { SafeAction(CommitSuggestion); };
            clearPromptsButton.Click += delegate { ClearPrompts(); };
            cancelButton.Click += delegate { SafeAction(CancelChanges); };
            saveButton.Click += delegate { SafeAction(delegate { SaveChanges(); }); };

            promptCanvas.MouseLeftButtonDown += OnCanvasMouseDown;
            promptCanvas.MouseLeftButtonUp += OnCanvasMouseUp;
            promptCanvas.SizeChanged += delegate { RenderPrompts(); };

            AutomationProperties.SetName(promptCanvas, "Annotation prompt canvas");
            AutomationProperties.SetName(classBox, "Annotation class selector");
            AutomationProperties.SetName(mergeBox, "Annotation merge mode");
            Content = BuildLayout();
            InputMode = AnnotationInputMode.Box;
            RefreshSession();
        }

        public event EventHandler Saved;
        public event EventHandler Cancelled;

        public IAnnotationSession Session
        {
            get { return session; }
            set
            {
                if (ReferenceEquals(session, value)) return;
                session = value;
                prompts.Clear();
                suggestion = null;
                RefreshSession();
            }
        }

        public AnnotationInputMode InputMode
        {
            get { return inputMode; }
            set
            {
                inputMode = value;
                positiveButton.FontWeight = value == AnnotationInputMode.PositivePoint ? FontWeights.Bold : FontWeights.Normal;
                negativeButton.FontWeight = value == AnnotationInputMode.NegativePoint ? FontWeights.Bold : FontWeights.Normal;
                boxButton.FontWeight = value == AnnotationInputMode.Box ? FontWeights.Bold : FontWeights.Normal;
                statusText.Text = "Prompt mode: " + value;
            }
        }

        public int PromptCount => prompts.Count;
        public bool SamReady => session != null && session.SamReady;
        public string SamState => session == null ? "No annotation session." : session.SamState;
        public bool HasSuggestion => suggestion != null && suggestion.Success;

        public void AddPositivePoint(int x, int y) => AddPoint(AnnotationPromptType.PositivePoint, x, y);
        public void AddNegativePoint(int x, int y) => AddPoint(AnnotationPromptType.NegativePoint, x, y);

        public void SetBoxPrompt(int x1, int y1, int x2, int y2)
        {
            RequireSession();
            prompts.RemoveAll(p => p.Type == AnnotationPromptType.Box);
            prompts.Add(new AnnotationPrompt { Type = AnnotationPromptType.Box, X = x1, Y = y1, X2 = x2, Y2 = y2 });
            suggestion = null;
            RenderPrompts();
            RenderOverlay();
            UpdateStatus("Box prompt 已更新。");
        }

        public AnnotationSuggestion RunSuggestion()
        {
            RequireSession();
            if (prompts.Count == 0) throw new InvalidOperationException("請先加入 Point 或 Box prompt。");
            AnnotationSuggestion result = session.Suggest(prompts.ToArray());
            suggestion = result != null && result.Success ? result : null;
            RenderOverlay();
            UpdateStatus(result == null ? "Suggestion backend returned null." : result.Message);
            return result;
        }

        public void CommitSuggestion()
        {
            RequireSession();
            if (suggestion == null || !suggestion.Success) throw new InvalidOperationException("目前沒有可 Commit 的 suggestion。");
            ClassChoice choice = classBox.SelectedItem as ClassChoice ?? new ClassChoice(1);
            AnnotationMergeMode merge = mergeBox.SelectedItem is AnnotationMergeMode
                ? (AnnotationMergeMode)mergeBox.SelectedItem
                : AnnotationMergeMode.FillEmptyOnly;
            session.CommitSuggestion(choice.Id, merge);
            suggestion = null;
            prompts.Clear();
            RefreshClassSummary();
            RenderPrompts();
            RenderOverlay();
            UpdateStatus("Suggestion 已 Commit 到 Class " + choice.Id + " · " + merge + "。尚未寫檔。");
        }

        public AnnotationSaveResult SaveChanges()
        {
            RequireSession();
            AnnotationSaveResult result = session.SaveLegacyTrainingPair();
            suggestion = null;
            prompts.Clear();
            RefreshSession();
            UpdateStatus("X/Y 已儲存：" + result.YPath);
            Saved?.Invoke(this, EventArgs.Empty);
            return result;
        }

        public void CancelChanges()
        {
            RequireSession();
            session.Cancel();
            suggestion = null;
            prompts.Clear();
            RefreshSession();
            UpdateStatus("未儲存標註已取消。");
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        public void ClearPrompts()
        {
            prompts.Clear();
            suggestion = null;
            RenderPrompts();
            RenderOverlay();
            UpdateStatus("Prompts 已清除；已 Commit 的 class map 不受影響。");
        }

        public int GetClassPixelCount(int classId)
        {
            RequireSession();
            if (classId < 1 || classId > 6) throw new ArgumentOutOfRangeException(nameof(classId));
            return session.GetClassMap().ClassIds.Count(v => v == classId);
        }

        private UIElement BuildLayout()
        {
            var root = new Grid { Background = Brush("#F4F6F8") };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel();
            header.Children.Add(new TextBlock { Text = "InspTools · AI Annotation", FontSize = 20, FontWeight = FontWeights.SemiBold });
            header.Children.Add(imageText);
            header.Children.Add(samText);
            var headerCard = Card(header, 14);
            Grid.SetRow(headerCard, 0);
            root.Children.Add(headerCard);

            var body = new Grid { Margin = new Thickness(14) };
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });

            var imageGrid = new Grid { Background = Brush("#101820"), ClipToBounds = true };
            imageGrid.Children.Add(baseImage);
            imageGrid.Children.Add(maskImage);
            imageGrid.Children.Add(promptCanvas);
            var imageCard = new Border { Background = Brush("#101820"), BorderBrush = Brush("#26323D"), BorderThickness = new Thickness(1), Padding = new Thickness(8), Child = imageGrid };
            Grid.SetColumn(imageCard, 0);
            body.Children.Add(imageCard);

            var tools = new StackPanel();
            tools.Children.Add(new TextBlock { Text = "Prompt", FontWeight = FontWeights.SemiBold });
            var promptButtons = new WrapPanel { Margin = new Thickness(0, 6, 0, 10) };
            promptButtons.Children.Add(positiveButton); promptButtons.Children.Add(negativeButton); promptButtons.Children.Add(boxButton);
            tools.Children.Add(promptButtons);
            tools.Children.Add(new TextBlock { Text = "Class" });
            tools.Children.Add(classBox);
            tools.Children.Add(new TextBlock { Text = "Merge", Margin = new Thickness(0, 8, 0, 0) });
            tools.Children.Add(mergeBox);
            tools.Children.Add(suggestButton);
            tools.Children.Add(commitButton);
            tools.Children.Add(clearPromptsButton);
            tools.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 8) });
            tools.Children.Add(classText);
            suggestButton.Margin = commitButton.Margin = clearPromptsButton.Margin = new Thickness(0, 8, 0, 0);
            var toolCard = Card(tools, 14);
            Grid.SetColumn(toolCard, 2);
            body.Children.Add(toolCard);
            Grid.SetRow(body, 1);
            root.Children.Add(body);

            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            cancelButton.Margin = new Thickness(0, 0, 8, 0);
            actions.Children.Add(cancelButton); actions.Children.Add(saveButton);
            var footer = new DockPanel();
            DockPanel.SetDock(actions, Dock.Right);
            footer.Children.Add(statusText);
            footer.Children.Add(actions);
            var footerCard = Card(footer, 12);
            Grid.SetRow(footerCard, 2);
            root.Children.Add(footerCard);
            return root;
        }

        private void RefreshSession()
        {
            baseImage.Source = null;
            maskImage.Source = null;
            promptCanvas.Children.Clear();
            if (session == null)
            {
                imageText.Text = "尚未綁定 Annotation Session";
                samText.Text = "SAM: unavailable";
                classText.Text = string.Empty;
                return;
            }

            LoadBaseImage(session.Image.ImagePath);
            imageText.Text = session.Image.ModelName + " · " + session.Image.Kind + " · " + session.Image.Width + "×" + session.Image.Height + " · " + (session.Editable ? "EDITABLE" : "READ ONLY");
            string fullSamState = session.SamState ?? string.Empty;
            string displaySamState = fullSamState;
            int errorAt = displaySamState.IndexOf("; CUDA provider load failed:", StringComparison.OrdinalIgnoreCase);
            if (errorAt >= 0) displaySamState = displaySamState.Substring(0, errorAt);
            if (displaySamState.StartsWith("Ready (", StringComparison.OrdinalIgnoreCase) &&
                displaySamState.EndsWith(")", StringComparison.Ordinal))
                displaySamState = displaySamState.Substring(7, displaySamState.Length - 8);
            samText.Text = "SAM: " + (session.SamReady ? "READY" : "NOT READY") +
                           (string.IsNullOrWhiteSpace(displaySamState) ? string.Empty : " · " + displaySamState);
            samText.ToolTip = string.IsNullOrWhiteSpace(fullSamState) ? null : fullSamState;
            suggestButton.IsEnabled = session.SamReady;
            commitButton.IsEnabled = session.Editable;
            saveButton.IsEnabled = session.Editable;
            RefreshClassSummary();
            RenderPrompts();
            RenderOverlay();
            UpdateStatus(session.IsDirty ? "目前有未儲存標註。" : "目前沒有未儲存修改。");
        }

        private void RefreshClassSummary()
        {
            if (session == null) { classText.Text = string.Empty; return; }
            AnnotationClassMap map = session.GetClassMap();
            var counts = new int[7];
            foreach (byte id in map.ClassIds) if (id <= 6) counts[id]++;
            classText.Text = "Class pixels\n" + string.Join("\n", Enumerable.Range(1, 6).Select(i => i + ": " + counts[i]));
        }

        private void AddPoint(AnnotationPromptType type, int x, int y)
        {
            RequireSession();
            prompts.Add(new AnnotationPrompt { Type = type, X = x, Y = y });
            suggestion = null;
            RenderPrompts();
            RenderOverlay();
            UpdateStatus(type + " prompt 已加入。共 " + prompts.Count + " 個 prompt。");
        }

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (session == null) return;
            Point imagePoint;
            if (!TryCanvasToImage(e.GetPosition(promptCanvas), out imagePoint)) return;
            if (inputMode == AnnotationInputMode.Box)
            {
                boxStart = imagePoint;
                return;
            }
            if (inputMode == AnnotationInputMode.PositivePoint) AddPositivePoint((int)imagePoint.X, (int)imagePoint.Y);
            else AddNegativePoint((int)imagePoint.X, (int)imagePoint.Y);
        }

        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (session == null || inputMode != AnnotationInputMode.Box || !boxStart.HasValue) return;
            Point imagePoint;
            if (!TryCanvasToImage(e.GetPosition(promptCanvas), out imagePoint)) { boxStart = null; return; }
            Point start = boxStart.Value;
            boxStart = null;
            SetBoxPrompt((int)start.X, (int)start.Y, (int)imagePoint.X, (int)imagePoint.Y);
        }

        private bool TryCanvasToImage(Point canvasPoint, out Point imagePoint)
        {
            imagePoint = new Point();
            if (session == null || promptCanvas.ActualWidth <= 0 || promptCanvas.ActualHeight <= 0) return false;
            double iw = session.Image.Width;
            double ih = session.Image.Height;
            double scale = Math.Min(promptCanvas.ActualWidth / iw, promptCanvas.ActualHeight / ih);
            if (scale <= 0) return false;
            double shownW = iw * scale;
            double shownH = ih * scale;
            double ox = (promptCanvas.ActualWidth - shownW) / 2d;
            double oy = (promptCanvas.ActualHeight - shownH) / 2d;
            if (canvasPoint.X < ox || canvasPoint.Y < oy || canvasPoint.X > ox + shownW || canvasPoint.Y > oy + shownH) return false;
            imagePoint = new Point((canvasPoint.X - ox) / scale, (canvasPoint.Y - oy) / scale);
            return true;
        }

        private void RenderPrompts()
        {
            promptCanvas.Children.Clear();
            if (session == null || promptCanvas.ActualWidth <= 0 || promptCanvas.ActualHeight <= 0) return;
            double iw = session.Image.Width;
            double ih = session.Image.Height;
            double scale = Math.Min(promptCanvas.ActualWidth / iw, promptCanvas.ActualHeight / ih);
            double ox = (promptCanvas.ActualWidth - iw * scale) / 2d;
            double oy = (promptCanvas.ActualHeight - ih * scale) / 2d;
            foreach (AnnotationPrompt prompt in prompts)
            {
                if (prompt.Type == AnnotationPromptType.Box)
                {
                    double left = ox + Math.Min(prompt.X, prompt.X2) * scale;
                    double top = oy + Math.Min(prompt.Y, prompt.Y2) * scale;
                    var rect = new Rectangle
                    {
                        Width = Math.Abs(prompt.X2 - prompt.X) * scale,
                        Height = Math.Abs(prompt.Y2 - prompt.Y) * scale,
                        Stroke = Brushes.Gold,
                        StrokeThickness = 2,
                        Fill = Brushes.Transparent
                    };
                    Canvas.SetLeft(rect, left); Canvas.SetTop(rect, top); promptCanvas.Children.Add(rect);
                }
                else
                {
                    const double size = 10;
                    var ellipse = new Ellipse
                    {
                        Width = size,
                        Height = size,
                        Fill = prompt.Type == AnnotationPromptType.PositivePoint ? Brushes.Lime : Brushes.Red,
                        Stroke = Brushes.White,
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(ellipse, ox + prompt.X * scale - size / 2d);
                    Canvas.SetTop(ellipse, oy + prompt.Y * scale - size / 2d);
                    promptCanvas.Children.Add(ellipse);
                }
            }
        }

        private void RenderOverlay()
        {
            if (session == null) { maskImage.Source = null; return; }
            AnnotationClassMap classes = session.GetClassMap();
            float[] current = suggestion != null && suggestion.Success ? suggestion.Mask : null;
            int pixels = classes.Width * classes.Height;
            var bgra = new byte[pixels * 4];
            for (int i = 0; i < pixels; i++)
            {
                byte id = classes.ClassIds[i];
                int p = i * 4;
                if (id > 0)
                {
                    Color c = ClassColor(id);
                    bgra[p] = c.B; bgra[p + 1] = c.G; bgra[p + 2] = c.R; bgra[p + 3] = 95;
                }
                else if (current != null && i < current.Length && current[i] > 0f)
                {
                    bgra[p] = 0; bgra[p + 1] = 215; bgra[p + 2] = 255; bgra[p + 3] = 90;
                }
            }
            var bitmap = new WriteableBitmap(classes.Width, classes.Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, classes.Width, classes.Height), bgra, classes.Width * 4, 0);
            bitmap.Freeze();
            maskImage.Source = bitmap;
        }

        private void LoadBaseImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            using (FileStream stream = File.OpenRead(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.StreamSource = stream; bitmap.EndInit(); bitmap.Freeze();
                baseImage.Source = bitmap;
            }
        }

        private void SafeAction(Action action)
        {
            try { action(); }
            catch (Exception ex) { UpdateStatus(ex.GetBaseException().Message); }
        }

        private void UpdateStatus(string text)
        {
            statusText.Text = (text ?? string.Empty) + (session != null && session.IsDirty ? " · UNSAVED" : string.Empty);
        }

        private void RequireSession()
        {
            if (session == null) throw new InvalidOperationException("Annotation Session is not assigned.");
        }

        private static Color ClassColor(byte id)
        {
            switch (id)
            {
                case 1: return Colors.Red;
                case 2: return Colors.Lime;
                case 3: return Colors.Blue;
                case 4: return Colors.Yellow;
                case 5: return Colors.Magenta;
                case 6: return Colors.Cyan;
                default: return Colors.Transparent;
            }
        }

        private static Border Card(UIElement child, double padding)
            => new Border { Background = Brushes.White, BorderBrush = Brush("#DDE3EA"), BorderThickness = new Thickness(1), Padding = new Thickness(padding), Child = child };
        private static Brush Brush(string value) => (Brush)new BrushConverter().ConvertFromString(value);

        private sealed class ClassChoice
        {
            internal ClassChoice(int id) { Id = id; }
            public int Id { get; private set; }
            public string Display => "Class " + Id;
        }
    }
}
