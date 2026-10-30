using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DIAChip8Side.TemplateHost
{
    /// <summary>
    /// Template-owned clean-room reconstruction of the vendor AIPreviewWindow.
    /// It intentionally stays on the post-license/model-preview side: one Station,
    /// one image source, old/new model comparison, and no hardware control.
    /// </summary>
    internal sealed class RebuiltTrainingPreviewWindow : Window
    {
        private readonly string _projectRoot;
        private readonly string _xRoot;
        private readonly string _yRoot;
        private readonly string _oldModel;
        private readonly string _newModel;
        private readonly int _width;
        private readonly int _height;
        private readonly int _station;
        private readonly Action<string> _trace;

        private readonly List<string> _allImages = new List<string>();
        private readonly List<string> _visibleImages = new List<string>();
        private int _imageIndex;
        private Process _renderProcess;

        private ComboBox _sourceCombo;
        private ComboBox _previewModeCombo;
        private TextBlock _fileText;
        private TextBlock _indexText;
        private TextBlock _statusText;
        private TextBlock _oldStatsText;
        private TextBlock _newStatsText;
        private Image _inputImage;
        private Image _labelImage;
        private Image _oldImage;
        private Image _newImage;
        private Button _prevButton;
        private Button _nextButton;
        private Button _refreshButton;

        internal RebuiltTrainingPreviewWindow(
            string projectRoot,
            string xRoot,
            string yRoot,
            string oldModel,
            string newModel,
            int width,
            int height,
            int station,
            Action<string> trace)
        {
            _projectRoot = projectRoot ?? throw new ArgumentNullException("projectRoot");
            _xRoot = xRoot ?? string.Empty;
            _yRoot = yRoot ?? string.Empty;
            _oldModel = oldModel ?? string.Empty;
            _newModel = newModel ?? string.Empty;
            _width = width;
            _height = height;
            _station = station;
            _trace = trace ?? delegate { };

            Title = "AI影像預覽";
            Width = 1083;
            Height = 1003;
            MinWidth = 1083;
            MinHeight = 900;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));
            Content = BuildUi();

            Loaded += delegate
            {
                LoadImageList();
                RefreshCurrentImage();
            };
            Closing += delegate(object sender, System.ComponentModel.CancelEventArgs args)
            {
                if (_renderProcess != null && !_renderProcess.HasExited)
                {
                    try
                    {
                        _renderProcess.Kill();
                    }
                    catch
                    {
                    }
                }
            };
        }

        private UIElement BuildUi()
        {
            Grid root = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(242, 242, 242))
            };
            Canvas canvas = new Canvas();
            root.Children.Add(canvas);

            Button okMark = PreviewButton("OK標註", 74, 36, Color.FromRgb(29, 122, 8));
            okMark.IsEnabled = false;
            okMark.ToolTip = "原廠為標註寫入操作；寫回 Training image contract 尚未驗證，因此暫不開放。";
            Place(canvas, okMark, 20, 13);
            Button ngMark = PreviewButton("NG標註", 74, 36, Color.FromRgb(178, 28, 35));
            ngMark.IsEnabled = false;
            ngMark.ToolTip = "原廠為標註寫入操作；寫回 Training image contract 尚未驗證，因此暫不開放。";
            Place(canvas, ngMark, 99, 13);

            _sourceCombo = new ComboBox
            {
                Width = 211,
                Height = 26,
                SelectedIndex = 2,
                Background = new SolidColorBrush(Color.FromRgb(255, 252, 184))
            };
            _sourceCombo.Items.Add("全部 Training Image");
            _sourceCombo.Items.Add("OK Training Image");
            _sourceCombo.Items.Add("NG Training Image");
            _sourceCombo.SelectionChanged += delegate
            {
                RefilterImages();
                RefreshCurrentImage();
            };
            Place(canvas, _sourceCombo, 20, 55);

            Button allButton = PreviewButton("ALL", 52, 36, Color.FromRgb(28, 137, 178));
            allButton.Click += delegate { _sourceCombo.SelectedIndex = 0; };
            Place(canvas, allButton, 277, 42);
            Button okButton = PreviewButton("OK", 52, 36, Color.FromRgb(29, 122, 8));
            okButton.Click += delegate { _sourceCombo.SelectedIndex = 1; };
            Place(canvas, okButton, 334, 42);
            Button ngButton = PreviewButton("NG", 52, 36, Color.FromRgb(178, 28, 35));
            ngButton.Click += delegate { _sourceCombo.SelectedIndex = 2; };
            Place(canvas, ngButton, 277, 82);
            Button fileButton = PreviewButton("File", 52, 36, Color.FromRgb(89, 28, 178));
            fileButton.IsEnabled = false;
            fileButton.ToolTip = "原廠 Buffer File 入口；目前尚未完成 genuine Buffer source contract。";
            Place(canvas, fileButton, 334, 82);

            _previewModeCombo = new ComboBox
            {
                Width = 101,
                Height = 26,
                SelectedIndex = 0,
                Background = Brushes.White
            };
            _previewModeCombo.Items.Add("0 - Overlay");
            _previewModeCombo.Items.Add("1 - Mask");
            _previewModeCombo.SelectionChanged += delegate { ApplyPreviewMode(); };
            Place(canvas, _previewModeCombo, 505, 10);

            _indexText = new TextBlock
            {
                Text = "0 / 0",
                Width = 64,
                Height = 38,
                Background = Brushes.White,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 9, 0, 0)
            };
            Place(canvas, _indexText, 0, 78);

            _prevButton = PreviewButton("◀", 48, 38, Color.FromRgb(247, 247, 247));
            _prevButton.Foreground = Brushes.Black;
            _prevButton.Click += delegate
            {
                if (_visibleImages.Count == 0) return;
                _imageIndex = (_imageIndex - 1 + _visibleImages.Count) % _visibleImages.Count;
                RefreshCurrentImage();
            };
            Place(canvas, _prevButton, 416, 78);
            _nextButton = PreviewButton("▶", 48, 38, Color.FromRgb(247, 247, 247));
            _nextButton.Foreground = Brushes.Black;
            _nextButton.Click += delegate
            {
                if (_visibleImages.Count == 0) return;
                _imageIndex = (_imageIndex + 1) % _visibleImages.Count;
                RefreshCurrentImage();
            };
            Place(canvas, _nextButton, 558, 78);

            _fileText = new TextBlock
            {
                Text = "--",
                Width = 248,
                Height = 33,
                Background = new SolidColorBrush(Color.FromRgb(183, 183, 193)),
                TextTrimming = TextTrimming.CharacterEllipsis,
                Padding = new Thickness(6, 8, 6, 0)
            };
            Place(canvas, _fileText, 647, 22);
            Button deleteButton = PreviewButton("刪除本張影像", 98, 78, Colors.Red);
            deleteButton.IsEnabled = false;
            deleteButton.ToolTip = "原廠控制項位置已同步；刪除 Training image 尚未開放。";
            Place(canvas, deleteButton, 913, 0);

            Border current = new Border
            {
                Width = 190,
                Height = 32,
                Background = new SolidColorBrush(Color.FromRgb(233, 255, 133)),
                Child = new TextBlock
                {
                    Text = "Training Image",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold
                }
            };
            Place(canvas, current, 0, 40);

            _inputImage = AddAbsolutePreviewPanel(canvas, "原始影像", 14, 125, 520, 330, Color.FromRgb(104, 104, 104));
            _labelImage = AddAbsolutePreviewPanel(canvas, "Y 標註", 14, 500, 520, 330, Color.FromRgb(104, 104, 104));
            _oldImage = AddAbsolutePreviewPanel(canvas, "AI標註(舊模型)", 558, 35, 480, 340, Color.FromRgb(104, 104, 104));
            _newImage = AddAbsolutePreviewPanel(canvas, "AI標註(新模型)", 558, 417, 480, 340, Color.FromRgb(117, 236, 255));

            Button newModelButton = PreviewButton("新模型", 99, 48, Colors.Blue);
            newModelButton.IsEnabled = false;
            newModelButton.ToolTip = "genuine handler 需要完整 AIPreview runtime state；切換語意尚未證實，避免假功能。";
            Place(canvas, newModelButton, 568, 765);
            Button oldModelButton = PreviewButton("舊模型", 102, 48, Colors.Gray);
            oldModelButton.IsEnabled = false;
            oldModelButton.ToolTip = "genuine handler 需要完整 AIPreview runtime state；切換語意尚未證實，避免假功能。";
            Place(canvas, oldModelButton, 687, 765);

            _refreshButton = PreviewButton("重新檢測", 92, 36, Color.FromRgb(251, 16, 251));
            _refreshButton.Click += delegate { RefreshCurrentImage(); };
            Place(canvas, _refreshButton, 14, 840);

            Button fixIgnore = PreviewButton("修正關注區域", 87, 35, Color.FromRgb(251, 251, 144));
            fixIgnore.IsEnabled = false;
            fixIgnore.ToolTip = "原廠控制項位置已同步；寫回標註前尚未開放。";
            Place(canvas, fixIgnore, 105, 840);
            Button loadOld = PreviewButton("載入模型", 86, 36, Color.FromRgb(255, 220, 178));
            loadOld.IsEnabled = false;
            Place(canvas, loadOld, 952, 840);
            Button writeTrain = PreviewButton("寫入訓練設定", 160, 26, Color.FromRgb(255, 155, 10));
            writeTrain.IsEnabled = false;
            Place(canvas, writeTrain, 878, 885);
            Button writePreprocess = PreviewButton("寫入前處理設定", 160, 26, Color.FromRgb(10, 177, 255));
            writePreprocess.IsEnabled = false;
            Place(canvas, writePreprocess, 878, 916);

            _oldStatsText = new TextBlock
            {
                Text = "舊模型：--",
                Width = 330,
                Height = 24,
                FontSize = 11
            };
            Place(canvas, _oldStatsText, 558, 810);
            _newStatsText = new TextBlock
            {
                Text = "新模型：--",
                Width = 330,
                Height = 24,
                FontSize = 11
            };
            Place(canvas, _newStatsText, 558, 835);

            _statusText = new TextBlock
            {
                Text = "待命",
                Width = 500,
                Height = 44,
                TextWrapping = TextWrapping.Wrap
            };
            Place(canvas, _statusText, 14, 890);
            Button backButton = PreviewButton("返回", 58, 30, Color.FromRgb(255, 10, 190));
            backButton.Click += delegate { Close(); };
            Place(canvas, backButton, 495, 930);
            return root;
        }

        private static Image AddAbsolutePreviewPanel(
            Canvas parent,
            string caption,
            double left,
            double top,
            double width,
            double height,
            Color headerColor)
        {
            Border border = new Border
            {
                Width = width,
                Height = height,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(165, 165, 165)),
                BorderThickness = new Thickness(1)
            };
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            grid.RowDefinitions.Add(new RowDefinition());
            Border header = new Border
            {
                Background = new SolidColorBrush(headerColor),
                Child = new TextBlock
                {
                    Text = caption,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 0, 0)
                }
            };
            grid.Children.Add(header);
            Image image = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(4)
            };
            Grid.SetRow(image, 1);
            grid.Children.Add(image);
            border.Child = grid;
            Place(parent, border, left, top);
            return image;
        }

        private static Button PreviewButton(
            string caption,
            double width,
            double height,
            Color color)
        {
            return new Button
            {
                Content = caption,
                Width = width,
                Height = height,
                Background = new SolidColorBrush(color),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold
            };
        }

        private static void Place(Canvas canvas, UIElement element, double left, double top)
        {
            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
            canvas.Children.Add(element);
        }

        private static Image AddPreviewPanel(Grid parent, string caption, int column, int row)
        {
            Border border = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(195, 195, 195)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4)
            };
            Grid panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(28) });
            panel.RowDefinitions.Add(new RowDefinition());
            panel.Children.Add(new TextBlock
            {
                Text = caption,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(8, 5, 0, 0)
            });
            Image image = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(6)
            };
            Grid.SetRow(image, 1);
            panel.Children.Add(image);
            border.Child = panel;
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);
            parent.Children.Add(border);
            return image;
        }

        private static Button MakeButton(string caption, double width)
        {
            return new Button
            {
                Content = caption,
                Width = width,
                Height = 28,
                Margin = new Thickness(0, 0, 6, 0),
                Background = new SolidColorBrush(Color.FromRgb(239, 153, 58)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold
            };
        }

        private void LoadImageList()
        {
            _allImages.Clear();
            if (!Directory.Exists(_xRoot))
            {
                _statusText.Text = "找不到 Training X 路徑：" + _xRoot;
                return;
            }
            string[] extensions = { ".bmp", ".png", ".jpg", ".jpeg", ".tif", ".tiff" };
            foreach (string file in Directory.GetFiles(_xRoot, "*.*", SearchOption.AllDirectories))
            {
                if (extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                {
                    _allImages.Add(file);
                }
            }
            _allImages.Sort(StringComparer.OrdinalIgnoreCase);
            RefilterImages();
        }

        private void RefilterImages()
        {
            _visibleImages.Clear();
            int source = _sourceCombo != null ? _sourceCombo.SelectedIndex : 0;
            foreach (string file in _allImages)
            {
                string normalized = file.Replace('/', '\\');
                bool isOk = normalized.IndexOf("\\OK\\", StringComparison.OrdinalIgnoreCase) >= 0;
                bool isNg = normalized.IndexOf("\\NG\\", StringComparison.OrdinalIgnoreCase) >= 0;
                if (source == 1 && !isOk) continue;
                if (source == 2 && !isNg) continue;
                _visibleImages.Add(file);
            }
            _imageIndex = 0;
            UpdateIndexText();
        }

        private void RefreshCurrentImage()
        {
            if (_renderProcess != null && !_renderProcess.HasExited)
            {
                return;
            }
            if (_visibleImages.Count == 0)
            {
                _fileText.Text = "沒有可預覽影像";
                _statusText.Text = "影像清單為空";
                UpdateIndexText();
                return;
            }
            if (!File.Exists(_oldModel) || !File.Exists(_newModel))
            {
                _statusText.Text = "舊模型或新模型不存在";
                return;
            }

            string image = _visibleImages[_imageIndex];
            string label = ResolveLabelPath(image);
            _fileText.Text = Path.GetFileName(image);
            _fileText.ToolTip = image;
            UpdateIndexText();
            SetBusy(true, "模型預覽運算中...");

            string outputRoot = Path.Combine(
                _projectRoot,
                "scratch",
                "TrainingReverse",
                "PreviewUi",
                "S" + _station.ToString(CultureInfo.InvariantCulture));
            Directory.CreateDirectory(outputRoot);
            string runRoot = Path.Combine(outputRoot, "current");
            Directory.CreateDirectory(runRoot);

            string python = ProductRuntimePaths.GetTrainingPython(_projectRoot);
            string script = ProductRuntimePaths.GetTrainingScript(_projectRoot, "render_model_preview.py");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = python,
                WorkingDirectory = Path.GetDirectoryName(script),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments =
                    Quote(script) +
                    " --image " + Quote(image) +
                    (string.IsNullOrWhiteSpace(label) ? string.Empty : " --label " + Quote(label)) +
                    " --old-model " + Quote(_oldModel) +
                    " --new-model " + Quote(_newModel) +
                    " --width " + _width.ToString(CultureInfo.InvariantCulture) +
                    " --height " + _height.ToString(CultureInfo.InvariantCulture) +
                    " --output-dir " + Quote(runRoot)
            };
            ProductRuntimePaths.ApplyTrainingEnvironment(psi, _projectRoot, 0);

            try
            {
                Process process = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };
                process.Exited += delegate
                {
                    string stdout = string.Empty;
                    string stderr = string.Empty;
                    try
                    {
                        stdout = process.StandardOutput.ReadToEnd();
                        stderr = process.StandardError.ReadToEnd();
                    }
                    catch
                    {
                    }
                    int exitCode = process.ExitCode;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _renderProcess = null;
                        if (exitCode != 0)
                        {
                            SetBusy(false, "預覽失敗 exit=" + exitCode);
                            _trace("RebuiltAIPreview: FAIL " + stderr);
                            return;
                        }
                        LoadRenderResult(runRoot, stdout);
                        SetBusy(false, "預覽完成");
                    }));
                };
                _renderProcess = process;
                if (!process.Start())
                {
                    _renderProcess = null;
                    throw new InvalidOperationException("無法啟動模型預覽程序。");
                }
                _trace("RebuiltAIPreview: start S" + _station + " image=" + image);
            }
            catch (Exception exception)
            {
                _renderProcess = null;
                SetBusy(false, "預覽失敗：" + exception.Message);
                _trace("RebuiltAIPreview: start FAIL " + exception);
            }
        }

        private void LoadRenderResult(string runRoot, string stdout)
        {
            string jsonPath = Path.Combine(runRoot, "preview.json");
            if (!File.Exists(jsonPath))
            {
                _statusText.Text = "preview.json 不存在";
                return;
            }
            _inputImage.Source = LoadBitmap(Path.Combine(runRoot, "input.png"));
            _labelImage.Source = LoadBitmap(Path.Combine(runRoot, "label.png"));
            ApplyPreviewMode();

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            double? oldIou = ExtractNestedNumber(json, "old", "iou");
            double? newIou = ExtractNestedNumber(json, "new", "iou");
            double? oldPixels = ExtractNestedNumber(json, "old", "mask_pixels");
            double? newPixels = ExtractNestedNumber(json, "new", "mask_pixels");
            _oldStatsText.Text =
                "舊模型：IoU " + FormatNumber(oldIou) +
                " / Mask " + FormatInteger(oldPixels);
            _newStatsText.Text =
                "新模型：IoU " + FormatNumber(newIou) +
                " / Mask " + FormatInteger(newPixels);
            _trace("RebuiltAIPreview: PASS " + Path.GetFileName(_visibleImages[_imageIndex]));
        }

        private void ApplyPreviewMode()
        {
            if (_oldImage == null || _newImage == null)
            {
                return;
            }
            string runRoot = Path.Combine(
                _projectRoot,
                "scratch",
                "TrainingReverse",
                "PreviewUi",
                "S" + _station.ToString(CultureInfo.InvariantCulture),
                "current");
            bool maskOnly = _previewModeCombo != null && _previewModeCombo.SelectedIndex == 1;
            _oldImage.Source = LoadBitmap(Path.Combine(runRoot, maskOnly ? "old_mask.png" : "old_model.png"));
            _newImage.Source = LoadBitmap(Path.Combine(runRoot, maskOnly ? "new_mask.png" : "new_model.png"));
        }

        private string ResolveLabelPath(string image)
        {
            if (!Directory.Exists(_yRoot))
            {
                return string.Empty;
            }
            string stem = Path.GetFileNameWithoutExtension(image);
            string match = Directory.GetFiles(_yRoot, stem + ".*", SearchOption.AllDirectories).FirstOrDefault();
            return match ?? string.Empty;
        }

        private void UpdateIndexText()
        {
            _indexText.Text = _visibleImages.Count == 0
                ? "0 / 0"
                : (_imageIndex + 1).ToString(CultureInfo.InvariantCulture) +
                  " / " + _visibleImages.Count.ToString(CultureInfo.InvariantCulture);
        }

        private void SetBusy(bool busy, string status)
        {
            _prevButton.IsEnabled = !busy;
            _nextButton.IsEnabled = !busy;
            _refreshButton.IsEnabled = !busy;
            _sourceCombo.IsEnabled = !busy;
            _statusText.Text = status;
        }

        private static BitmapSource LoadBitmap(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }

        private static double? ExtractNestedNumber(string json, string objectName, string propertyName)
        {
            string objectToken = "\"" + objectName + "\"";
            int objectIndex = json.IndexOf(objectToken, StringComparison.OrdinalIgnoreCase);
            if (objectIndex < 0) return null;
            int objectStart = json.IndexOf('{', objectIndex + objectToken.Length);
            if (objectStart < 0) return null;
            int objectEnd = json.IndexOf('}', objectStart + 1);
            if (objectEnd < 0) return null;
            string segment = json.Substring(objectStart, objectEnd - objectStart + 1);
            string propertyToken = "\"" + propertyName + "\"";
            int propertyIndex = segment.IndexOf(propertyToken, StringComparison.OrdinalIgnoreCase);
            if (propertyIndex < 0) return null;
            int colon = segment.IndexOf(':', propertyIndex + propertyToken.Length);
            if (colon < 0) return null;
            int end = colon + 1;
            while (end < segment.Length && char.IsWhiteSpace(segment[end])) end++;
            int start = end;
            while (end < segment.Length &&
                   (char.IsDigit(segment[end]) || segment[end] == '-' || segment[end] == '+' ||
                    segment[end] == '.' || segment[end] == 'e' || segment[end] == 'E'))
            {
                end++;
            }
            double value;
            return double.TryParse(
                segment.Substring(start, end - start),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value)
                ? (double?)value
                : null;
        }

        private static string FormatNumber(double? value)
        {
            return value.HasValue
                ? value.Value.ToString("0.0000", CultureInfo.InvariantCulture)
                : "--";
        }

        private static string FormatInteger(double? value)
        {
            return value.HasValue
                ? Math.Round(value.Value).ToString(CultureInfo.InvariantCulture)
                : "--";
        }
    }
}
