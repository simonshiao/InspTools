using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ellipse = System.Windows.Shapes.Ellipse;
using Line = System.Windows.Shapes.Line;
using Polyline = System.Windows.Shapes.Polyline;

namespace DIAChip8Side.TemplateHost
{
    /// <summary>
    /// Template-owned clean-room Training UI.
    ///
    /// This window never calls the original protected AITrainingWindow and does
    /// not implement, consume, or bypass its authentication mechanism. It only
    /// drives the reconstructed V3 trainer from the product-owned TrainingRebuild runtime.
    /// </summary>
    internal sealed class RebuiltTrainingWindow : Window
    {
        // UI-only rollout: production gets the Delta visual shell, while all
        // standalone queue/keyboard/lifecycle behavior stays disabled.
        private static readonly bool DeltaVisualDesign = true;
#if TRAINING_STANDALONE
        private static readonly bool StandaloneBehavior = true;
#else
        private static readonly bool StandaloneBehavior = false;
#endif
        // Queue/scheduler is now a shared Training feature.  Production keeps
        // the proven TemplateHost callbacks/lifecycle, while reusing the
        // ChaoTraining queue UI/flow.  Standalone-only keyboard/window behavior
        // remains controlled exclusively by StandaloneBehavior.
        private static readonly bool TrainingQueueFeature = true;
        private static readonly bool StandaloneDesign = DeltaVisualDesign;
        private static readonly Color ChaoCream = Color.FromRgb(241, 247, 253);
        private static readonly Color ChaoSurface = Color.FromRgb(255, 255, 255);
        private static readonly Color ChaoOrange = Color.FromRgb(90, 185, 244);
        private static readonly Color ChaoOrangeDeep = Color.FromRgb(36, 104, 161);
        private static readonly Color ChaoOrangeSoft = Color.FromRgb(231, 242, 255);
        private static readonly Color ChaoInk = Color.FromRgb(20, 42, 66);
        private static readonly Color ChaoMuted = Color.FromRgb(113, 144, 173);
        private static readonly Color ChaoLine = Color.FromRgb(215, 230, 244);
        private static readonly Color ChaoGreen = Color.FromRgb(34, 184, 143);
        private static readonly Color ChaoGreenSoft = Color.FromRgb(227, 248, 241);
        private static readonly Color ChaoBlue = Color.FromRgb(90, 185, 244);
        private static readonly Color ChaoBlueSoft = Color.FromRgb(231, 242, 255);
        private static readonly Color DeltaBackground = Color.FromRgb(241, 247, 253);
        private static readonly Color DeltaInk = Color.FromRgb(20, 42, 66);
        private static readonly Color DeltaTitle = Color.FromRgb(18, 57, 94);
        private static readonly Color DeltaMuted = Color.FromRgb(113, 144, 173);
        private static readonly Color DeltaLine = Color.FromRgb(215, 230, 244);
        private static readonly Color DeltaLineStrong = Color.FromRgb(207, 225, 241);
        private static readonly Color DeltaBlue = Color.FromRgb(90, 185, 244);
        private static readonly Color DeltaGreen = Color.FromRgb(34, 184, 143);
        private static readonly Color DeltaGreenSoft = Color.FromRgb(227, 248, 241);
        private const string DeltaLauncherRoot = @"D:\delta\DeltaCodexLauncher-122B正常";
        private readonly string _projectRoot;
        private readonly string _recipePath;
        private readonly Action<string> _trace;
        private readonly Func<int, string> _reloadStationModel;
        private readonly Func<bool> _hasLiveInspectionStations;
        private readonly DispatcherTimer _progressTimer;

        private TextBox _stationBox;
        private TextBox _xPathBox;
        private TextBox _yPathBox;
        private TextBox _outputRootBox;
        private TextBox _widthBox;
        private TextBox _heightBox;
        private TextBox _epochBox;
        private TextBox _batchBox;
        private TextBox _lossBox;
        private TextBox _learningRateBox;
        private TextBox _unetDivBox;
        private TextBox _augRatioBox;
        private TextBox _gpuBox;
        private TextBox _maxSamplesBox;
        private TextBox _logBox;
        private ProgressBar _progressBar;
        private TextBlock _statusText;
        private TextBlock _stationInfoText;
        private TextBlock _timeText;
        private TextBlock _standaloneHeaderModelText;
        private TextBlock _standaloneHeaderStationText;
        private TextBlock _standaloneHeaderRecipeText;
        private TextBlock _standaloneQueueSummaryText;
        private ListBox _standaloneQueueList;
        private Button _standaloneAddQueueButton;
        private Button _standaloneStartQueueButton;
        private Button _standaloneClearQueueButton;
        private readonly List<StandaloneQueueItem> _standaloneQueue = new List<StandaloneQueueItem>();
        private bool _standaloneQueueRunning;
        private StandaloneQueueItem _standaloneCurrentQueueItem;
        private CheckBox _retestAfterTrainingCheck;
        private CheckBox _hideTrainingWindowCheck;
        private CheckBox _autoUpdateModelCheck;
        private ComboBox _testImageSetCombo;
        private Button _startButton;
        private Button _stopButton;
        private Button _terminateButton;
        private Button _convertButton;
        private Button _readModelButton;
        private Button _previewButton;
        private Button _deployCheckButton;
        private Button _replaceAllButton;
        private Button _lossCurveButton;
        private Button _agentInfoButton;
        private readonly Border[] _stationCards = new Border[8];
        private readonly TextBlock[] _stationModelTexts = new TextBlock[8];
        private readonly TextBlock[] _stationOkTexts = new TextBlock[8];
        private readonly TextBlock[] _stationNgTexts = new TextBlock[8];
        private readonly TextBox[] _stationEpochBoxes = new TextBox[8];
        private readonly CheckBox[] _stationTrainChecks = new CheckBox[8];
        private readonly ProgressBar[] _stationTrainProgressBars = new ProgressBar[8];
        private readonly ProgressBar[] _stationTestProgressBars = new ProgressBar[8];
        private readonly TextBlock[] _stationTrainProgressTexts = new TextBlock[8];
        private readonly TextBlock[] _stationTestProgressTexts = new TextBlock[8];
        private readonly TextBlock[] _stationAccuracyTexts = new TextBlock[8];
        private readonly TextBlock[] _stationYieldTexts = new TextBlock[8];
        private readonly TextBlock[] _stationNewYieldTexts = new TextBlock[8];
        private readonly Button[] _stationPreviewButtons = new Button[8];
        private readonly Button[] _stationUpdateButtons = new Button[8];
        private Grid _mainTrainingLayer;
        private Grid _resultTrainingLayer;
        private TextBox _resultInspLog;
        private TextBox _resultCommandLog;
        private readonly string[] _stationRunStates = new string[8];
        private readonly string[] _stationPrimaryOverrides = new string[8];
        private readonly string[] _stationSecondaryOverrides = new string[8];
        private readonly SortedDictionary<int, double>[] _stationLossHistory =
            new SortedDictionary<int, double>[8];
        private Expander _advancedExpander;
        private int _selectedStation = 2;

        private sealed class StandaloneQueueItem
        {
            internal int Station;
            internal int Epoch;
            internal string SeedPath;
            internal string SeedSha256;
            internal string Status;

            public override string ToString()
            {
                return "S" + Station.ToString(CultureInfo.InvariantCulture) +
                    "   " + Path.GetFileName(SeedPath ?? string.Empty) +
                    "   " + Epoch.ToString(CultureInfo.InvariantCulture) + " epochs" +
                    "   " + (Status ?? "QUEUED");
            }
        }

        private Process _activeProcess;
        private Process _previewProcess;
        private string _previewOutputPath;
        private DateTime _previewStartedAt;
        private string _activeProcDir;
        private string _activeModelPath;
        private string _activeIniPath;
        private string _workspaceModelType = "US";
        private double _workspaceMaybeArea = 0.3;
        private double _workspaceThreshold = 220.0;
        private DateTime _trainingStartedAt;
        private int _lastProgressPercent;
        private DateTime _lastProgressAt;
        private string _lastLossText = string.Empty;
        private int _trainingStdoutLineCount;
        private int _trainingStderrLineCount;
        private bool _stopRequested;
        private bool _stopEscalated;
        private DateTime _stopRequestedAt;
        private bool _postTrainingFinalizeInProgress;
        private volatile bool _agentPipeStopping;
        private string _agentPipeName;
        private string _agentReadyPath;
        private TrainingRun _activeRun;
        private const int SoftStopTimeoutSeconds = 30;
        private const int MaxTrainingStdoutUiLines = 200;
        private const int PbConversionTimeoutMs = 120000;
        private const int DeploymentCheckTimeoutMs = 60000;
        private static readonly object LiveSeedSync = new object();
        private static readonly Dictionary<string, string> LiveSelectedSeeds =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string LiveSeedKey(string recipePath, int station, int slot)
        {
            string recipe = string.IsNullOrWhiteSpace(recipePath)
                ? string.Empty
                : Path.GetFullPath(recipePath).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            return recipe + "|S" +
                station.ToString(CultureInfo.InvariantCulture) + "|" +
                (slot == 0 ? "A" : "B");
        }

        internal static void SetLiveSelectedSeed(
            string recipePath,
            int station,
            int slot,
            string modelPath)
        {
            if (station < 1 || station > 8 ||
                (slot != 0 && slot != 1) ||
                string.IsNullOrWhiteSpace(recipePath) ||
                string.IsNullOrWhiteSpace(modelPath))
            {
                return;
            }
            lock (LiveSeedSync)
            {
                LiveSelectedSeeds[LiveSeedKey(recipePath, station, slot)] =
                    Path.GetFullPath(modelPath);
            }
        }

        private static bool TryGetLiveSelectedSeed(
            string recipePath,
            int station,
            int slot,
            out string modelPath)
        {
            modelPath = string.Empty;
            if (station < 1 || station > 8 ||
                (slot != 0 && slot != 1) ||
                string.IsNullOrWhiteSpace(recipePath))
            {
                return false;
            }
            lock (LiveSeedSync)
            {
                string value;
                if (!LiveSelectedSeeds.TryGetValue(
                        LiveSeedKey(recipePath, station, slot),
                        out value) ||
                    string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
                modelPath = value;
                return true;
            }
        }

        internal RebuiltTrainingWindow(
            string projectRoot,
            string recipePath,
            Action<string> trace,
            int initialStation = 2,
            Func<int, string> reloadStationModel = null,
            Func<bool> hasLiveInspectionStations = null)
        {
            _projectRoot = projectRoot ?? throw new ArgumentNullException("projectRoot");
            _recipePath = recipePath ?? string.Empty;
            _trace = trace ?? delegate { };
            _reloadStationModel = reloadStationModel;
            _hasLiveInspectionStations = hasLiveInspectionStations;
            if (initialStation >= 1 && initialStation <= 8)
            {
                _selectedStation = initialStation;
            }

            for (int i = 0; i < _stationLossHistory.Length; i++)
            {
                _stationLossHistory[i] = new SortedDictionary<int, double>();
            }

            Title = "DELTA AI Training — Rebuilt V3";
            Width = 980;
            Height = 760;
            MinWidth = 900;
            MinHeight = 650;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.WhiteSmoke;

            Content = BuildUi();
            if (StandaloneBehavior)
            {
                PreviewKeyDown += HandleStandaloneGameNavigation;
            }

            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(700)
            };
            _progressTimer.Tick += delegate
            {
                RefreshProgress();
                CheckTrainingStopTimeout();
            };

            Loaded += delegate
            {
                ApplyRecipeDefaults();
                _progressTimer.Start();
                StartTrainingAgentPipe();
            };
            Closing += delegate(object sender, CancelEventArgs args)
            {
                if ((_activeProcess != null && !_activeProcess.HasExited) ||
                    (_previewProcess != null && !_previewProcess.HasExited))
                {
                    args.Cancel = true;
                    MessageBox.Show(
                        this,
                        "目前仍有 Training 執行中。請先按「停止」等待程序結束。",
                        "Training 執行中",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                _agentPipeStopping = true;
                try
                {
                    if (!string.IsNullOrWhiteSpace(_agentReadyPath) &&
                        File.Exists(_agentReadyPath))
                    {
                        File.Delete(_agentReadyPath);
                    }
                }
                catch
                {
                    // Agent discovery cleanup must never block Training shutdown.
                }
                _progressTimer.Stop();
            };
        }

        private UIElement BuildUi()
        {
            Background = new SolidColorBrush(
                StandaloneDesign
                    ? ChaoCream
                    : Color.FromRgb(126, 126, 126));
            if (StandaloneDesign)
            {
                FontFamily = new FontFamily("Segoe UI, Microsoft JhengHei UI");
            }
            Title = StandaloneDesign ? "DELTA AI Training Console" : "AI模型訓練程式";
            Width = StandaloneDesign ? 1680 : 1603;
            Height = StandaloneDesign ? 940 : 781;
            MinWidth = 1500;
            MinHeight = StandaloneDesign ? 820 : 740;

            Grid root = new Grid
            {
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? DeltaBackground
                        : Color.FromRgb(126, 126, 126))
            };
            if (StandaloneDesign)
            {
                root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(104) });
                root.RowDefinitions.Add(new RowDefinition());
                UIElement header = BuildStandaloneHeader();
                Grid.SetRow(header, 0);
                root.Children.Add(header);

                UIElement backdrop = BuildStandaloneGameBackdrop();
                Grid.SetRow(backdrop, 1);
                root.Children.Add(backdrop);
            }
            _mainTrainingLayer = BuildOriginalMainLayer();
            _resultTrainingLayer = BuildOriginalResultLayer();
            _resultTrainingLayer.Visibility = Visibility.Hidden;
            if (StandaloneDesign)
            {
                Grid.SetRow(_mainTrainingLayer, 1);
                Grid.SetRow(_resultTrainingLayer, 1);
            }
            root.Children.Add(_mainTrainingLayer);
            root.Children.Add(_resultTrainingLayer);

            BuildHiddenAdvancedControls(root);
            UpdateStationSelectionVisual();
            return root;
        }

        private UIElement BuildStandaloneGameBackdrop()
        {
            Grid backdrop = new Grid
            {
                IsHitTestVisible = false,
                ClipToBounds = true,
                Background = new LinearGradientBrush(
                    DeltaBackground,
                    Color.FromRgb(230, 243, 254),
                    24)
            };

            Canvas ambient = new Canvas
            {
                IsHitTestVisible = false,
                ClipToBounds = true
            };

            Ellipse cyanGlow = new Ellipse
            {
                Width = 760,
                Height = 760,
                Opacity = 0.16,
                Fill = new RadialGradientBrush(
                    Color.FromArgb(180, 80, 216, 238),
                    Color.FromArgb(0, 80, 216, 238)),
                Effect = new BlurEffect { Radius = 90 }
            };
            TranslateTransform cyanMove = new TranslateTransform();
            cyanGlow.RenderTransform = cyanMove;
            Canvas.SetLeft(cyanGlow, -250);
            Canvas.SetTop(cyanGlow, -300);
            ambient.Children.Add(cyanGlow);
            cyanMove.BeginAnimation(
                TranslateTransform.XProperty,
                new DoubleAnimation
                {
                    From = 0,
                    To = 190,
                    Duration = TimeSpan.FromSeconds(10),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                });
            cyanMove.BeginAnimation(
                TranslateTransform.YProperty,
                new DoubleAnimation
                {
                    From = 0,
                    To = 85,
                    Duration = TimeSpan.FromSeconds(13),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                });

            Ellipse blueGlow = new Ellipse
            {
                Width = 680,
                Height = 680,
                Opacity = 0.13,
                Fill = new RadialGradientBrush(
                    Color.FromArgb(175, 109, 157, 244),
                    Color.FromArgb(0, 109, 157, 244)),
                Effect = new BlurEffect { Radius = 110 }
            };
            TranslateTransform blueMove = new TranslateTransform();
            blueGlow.RenderTransform = blueMove;
            Canvas.SetLeft(blueGlow, 1120);
            Canvas.SetTop(blueGlow, 80);
            ambient.Children.Add(blueGlow);
            blueMove.BeginAnimation(
                TranslateTransform.XProperty,
                new DoubleAnimation
                {
                    From = 0,
                    To = -170,
                    Duration = TimeSpan.FromSeconds(12),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                });
            blueMove.BeginAnimation(
                TranslateTransform.YProperty,
                new DoubleAnimation
                {
                    From = 0,
                    To = 120,
                    Duration = TimeSpan.FromSeconds(16),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                });

            for (int i = 0; i < 14; i++)
            {
                Ellipse spark = new Ellipse
                {
                    Width = i % 3 == 0 ? 4 : 2,
                    Height = i % 3 == 0 ? 4 : 2,
                    Fill = new SolidColorBrush(Color.FromArgb(180, 71, 121, 174)),
                    Opacity = 0.10
                };
                Canvas.SetLeft(spark, 90 + ((i * 137) % 1450));
                Canvas.SetTop(spark, 40 + ((i * 89) % 690));
                spark.BeginAnimation(
                    UIElement.OpacityProperty,
                    new DoubleAnimation
                    {
                        From = 0.06,
                        To = 0.34,
                        Duration = TimeSpan.FromSeconds(2.4 + (i % 5) * 0.65),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        BeginTime = TimeSpan.FromMilliseconds(i * 140)
                    });
                ambient.Children.Add(spark);
            }

            backdrop.Children.Add(ambient);
            return backdrop;
        }

        private UIElement BuildStandaloneHeader()
        {
            Border header = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(238, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(DeltaLine),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Margin = new Thickness(20, 14, 20, 0),
                Padding = new Thickness(18, 12, 16, 12)
            };
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            FrameworkElement brandMark;
            string deltaLogoPath = Path.Combine(
                DeltaLauncherRoot,
                "assets",
                "delta_logo.png");
            if (File.Exists(deltaLogoPath))
            {
                brandMark = new Image
                {
                    Source = LoadBitmap(deltaLogoPath),
                    Width = 180,
                    Height = 46,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 16, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                brandMark = new Border
                {
                    Width = 58,
                    Height = 58,
                    CornerRadius = new CornerRadius(16),
                    Background = new SolidColorBrush(DeltaBlue),
                    Margin = new Thickness(0, 0, 16, 0),
                    Child = new TextBlock
                    {
                        Text = "△",
                        Foreground = Brushes.White,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
            }
            grid.Children.Add(brandMark);

            StackPanel titleStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStack.Children.Add(new TextBlock
            {
                Text = "AI TRAINING",
                Foreground = new SolidColorBrush(DeltaTitle),
                FontSize = 24,
                FontWeight = FontWeights.Bold
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = "8-STATION TRAINING ORCHESTRATOR  ·  IDENTITY GUARDED",
                Foreground = new SolidColorBrush(DeltaMuted),
                FontSize = 10,
                Margin = new Thickness(0, 2, 0, 0)
            });
            _standaloneHeaderRecipeText = new TextBlock
            {
                Text = "Recipe  --",
                Foreground = new SolidColorBrush(ChaoOrangeDeep),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 3, 0, 0)
            };
            titleStack.Children.Add(_standaloneHeaderRecipeText);
            Grid.SetColumn(titleStack, 1);
            grid.Children.Add(titleStack);

            StackPanel identity = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            identity.Children.Add(MakeDeltaChip("● READY", false, true));
            TextBlock navChip = MakeDeltaChip("← →  A / D  SELECT", true);
            navChip.Foreground = new SolidColorBrush(Color.FromRgb(36, 116, 188));
            navChip.Background = new SolidColorBrush(Color.FromRgb(231, 242, 255));
            identity.Children.Add(navChip);
            _standaloneHeaderStationText = MakeStandaloneChip("S" + _selectedStation.ToString(CultureInfo.InvariantCulture));
            _standaloneHeaderModelText = MakeStandaloneChip("Model --", true);
            identity.Children.Add(_standaloneHeaderStationText);
            identity.Children.Add(_standaloneHeaderModelText);
            Grid.SetColumn(identity, 2);
            grid.Children.Add(identity);
            header.Child = grid;
            return header;
        }

        private static TextBlock MakeStandaloneChip(string text, bool mono = false, bool positive = false)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(positive ? ChaoGreen : ChaoInk),
                Background = new SolidColorBrush(positive ? ChaoGreenSoft : ChaoOrangeSoft),
                Padding = new Thickness(11, 7, 11, 7),
                Margin = new Thickness(8, 0, 0, 0),
                FontFamily = mono ? new FontFamily("Consolas") : new FontFamily("Segoe UI, Microsoft JhengHei UI"),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
        }

        private static BitmapImage LoadBitmap(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static TextBlock MakeDeltaChip(string text, bool mono = false, bool positive = false)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(positive ? Color.FromRgb(22, 129, 101) : DeltaInk),
                Background = new SolidColorBrush(positive ? DeltaGreenSoft : Color.FromRgb(248, 251, 255)),
                Padding = new Thickness(11, 7, 11, 7),
                Margin = new Thickness(8, 0, 0, 0),
                FontFamily = mono
                    ? new FontFamily("Cascadia Mono, Consolas")
                    : new FontFamily("Segoe UI, Microsoft JhengHei UI"),
                FontSize = 10.5,
                FontWeight = FontWeights.SemiBold
            };
        }

        private Grid BuildOriginalMainLayer()
        {
            Grid layer = new Grid
            {
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? Colors.Transparent
                        : Color.FromRgb(126, 126, 126))
            };
            layer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(456) });
            layer.RowDefinitions.Add(new RowDefinition());

            Grid stationGrid = new Grid
            {
                Width = 1569,
                Height = 452,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            stationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(392) });
            stationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(393) });
            stationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(392) });
            stationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(392) });
            stationGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(226) });
            stationGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(226) });
            for (int station = 1; station <= 8; station++)
            {
                Border card = BuildStationCard(station);
                int column = (station - 1) % 4;
                int row = (station - 1) / 4;
                card.Margin = new Thickness(column == 0 ? 8 : 10, 8, 0, 0);
                card.HorizontalAlignment = HorizontalAlignment.Left;
                card.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetColumn(card, column);
                Grid.SetRow(card, row);
                stationGrid.Children.Add(card);
            }
            Grid.SetRow(stationGrid, 0);
            layer.Children.Add(stationGrid);

            Grid controls = new Grid
            {
                Margin = new Thickness(28, 8, 28, 18),
                Background = StandaloneDesign
                    ? Brushes.Transparent
                    : new SolidColorBrush(Color.FromRgb(238, 238, 238))
            };
            controls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(520) });
            controls.ColumnDefinitions.Add(new ColumnDefinition());
            controls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(142) });

            StackPanel settings = new StackPanel { Margin = new Thickness(18, 14, 18, 14) };
            _hideTrainingWindowCheck = new CheckBox
            {
                Content = "隱藏訓練程式視窗",
                IsChecked = true,
                Margin = new Thickness(2, 2, 0, 8),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                    : Brushes.Black
            };
            _retestAfterTrainingCheck = new CheckBox
            {
                Content = "訓練完後，回測影像",
                IsChecked = false,
                Margin = new Thickness(2, 2, 0, 8),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                    : Brushes.Black
            };
            _autoUpdateModelCheck = new CheckBox
            {
                Content = "訓練完後，自動更新模型",
                IsChecked = false,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(2, 2, 0, 8),
                ToolTip = "原廠 AutoUpdateModel 對應項；實際部署仍受更換前檢查、live Station 與 rollback 保護。"
            };
            Grid testImageSetRow = new Grid { Margin = new Thickness(2, 1, 0, 8) };
            testImageSetRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            testImageSetRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            testImageSetRow.Children.Add(new TextBlock
            {
                Text = "回測影像來源  ",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(156, 163, 175))
                    : Brushes.Black
            });
            _testImageSetCombo = new ComboBox
            {
                Width = 138,
                Height = 27,
                HorizontalAlignment = HorizontalAlignment.Left,
                SelectedIndex = 0,
                Background = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(255, 251, 242))
                    : Brushes.White,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(55, 48, 43))
                    : Brushes.Black
            };
            _testImageSetCombo.Items.Add("0 - Recent Image");
            _testImageSetCombo.Items.Add("1 - Recent OK Image");
            _testImageSetCombo.Items.Add("2 - Recent NG Image");
            _testImageSetCombo.Items.Add("3 - OK Image DB");
            _testImageSetCombo.Items.Add("4 - NG Image DB");
            _testImageSetCombo.Items.Add("5 - OK Training Image");
            _testImageSetCombo.Items.Add("6 - NG Training Image");
            Grid.SetColumn(_testImageSetCombo, 1);
            testImageSetRow.Children.Add(_testImageSetCombo);
            settings.Children.Add(_hideTrainingWindowCheck);
            settings.Children.Add(_retestAfterTrainingCheck);
            settings.Children.Add(testImageSetRow);
            settings.Children.Add(_autoUpdateModelCheck);
            if (TrainingQueueFeature)
            {
                Grid queueHeader = new Grid
                {
                    Margin = new Thickness(2, 4, 0, 6)
                };
                queueHeader.ColumnDefinitions.Add(new ColumnDefinition());
                queueHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                queueHeader.Children.Add(new TextBlock
                {
                    Text = "排程佇列  /  SCHEDULER",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(ChaoInk),
                    VerticalAlignment = VerticalAlignment.Center
                });
                _standaloneQueueSummaryText = new TextBlock
                {
                    Text = "0 QUEUED",
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(ChaoOrangeDeep),
                    Background = new SolidColorBrush(ChaoOrangeSoft),
                    Padding = new Thickness(8, 3, 8, 3),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(_standaloneQueueSummaryText, 1);
                queueHeader.Children.Add(_standaloneQueueSummaryText);
                settings.Children.Add(queueHeader);
                _standaloneQueueList = new ListBox
                {
                    Height = 82,
                    Background = new SolidColorBrush(ChaoCream),
                    Foreground = new SolidColorBrush(ChaoInk),
                    BorderBrush = new SolidColorBrush(ChaoLine),
                    BorderThickness = new Thickness(1),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 11,
                    Padding = new Thickness(6)
                };
                settings.Children.Add(_standaloneQueueList);
                _standaloneClearQueueButton = new Button
                {
                    Content = "清空排程",
                    Width = 86,
                    Height = 26,
                    IsEnabled = false,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 5, 0, 0),
                    Background = new SolidColorBrush(ChaoSurface),
                    Foreground = new SolidColorBrush(ChaoMuted),
                    BorderBrush = new SolidColorBrush(ChaoLine),
                    BorderThickness = new Thickness(1),
                    Template = CreateRoundedButtonTemplate(7)
                };
                _standaloneClearQueueButton.Click += delegate { ResetStandaloneQueue(); };
                settings.Children.Add(_standaloneClearQueueButton);
            }
            Border settingsCard = new Border
            {
                Background = new SolidColorBrush(ChaoSurface),
                BorderBrush = new SolidColorBrush(ChaoLine),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Margin = new Thickness(0, 0, 12, 0),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 14,
                    ShadowDepth = 2,
                    Opacity = 0.08,
                    Color = ChaoInk
                },
                Child = settings
            };
            Grid.SetColumn(settingsCard, 0);
            controls.Children.Add(settingsCard);

            StackPanel training = new StackPanel { Margin = new Thickness(18, 14, 18, 14) };
            if (DeltaVisualDesign)
            {
                training.Children.Add(BuildStandaloneTrainingHero());
            }
            StackPanel presets = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (int count in new[] { 150, 300, 500, 1000, 2000, 3000 })
            {
                int captured = count;
                Button preset = new Button
                {
                    Content = count.ToString(CultureInfo.InvariantCulture),
                    Width = 80,
                    Height = 28,
                    Margin = new Thickness(0, 0, 5, 8),
                    Background = StandaloneDesign
                        ? new SolidColorBrush(ChaoOrangeSoft)
                        : SystemColors.ControlBrush,
                    Foreground = StandaloneDesign
                        ? new SolidColorBrush(ChaoInk)
                        : Brushes.Black,
                    BorderBrush = StandaloneDesign
                        ? new SolidColorBrush(ChaoLine)
                        : SystemColors.ControlDarkBrush,
                    Template = StandaloneDesign ? CreateRoundedButtonTemplate(6) : null
                };
                preset.Click += delegate { ApplyTrainingPreset(captured); };
                presets.Children.Add(preset);
            }
            training.Children.Add(presets);
            StackPanel trainButtons = new StackPanel { Orientation = Orientation.Horizontal };
            _startButton = MakeOriginalButton("本機訓練開始", 150, false);
            SetAgentAutomationId(_startButton, "dia.training.start");
            _startButton.Height = 45;
            if (DeltaVisualDesign)
            {
                _startButton.Background = new SolidColorBrush(ChaoOrange);
            }
            _startButton.Click += delegate { StartTraining(); };
            Button remoteButton = MakeOriginalButton(
                TrainingQueueFeature ? "加入排程" : "遠端訓練開始",
                150,
                false);
            SetAgentAutomationId(remoteButton, "dia.training.queue.add");
            remoteButton.Height = 45;
            remoteButton.IsEnabled = TrainingQueueFeature;
            if (DeltaVisualDesign)
            {
                remoteButton.Background = new SolidColorBrush(ChaoOrangeSoft);
                remoteButton.Foreground = new SolidColorBrush(ChaoOrangeDeep);
                remoteButton.BorderBrush = new SolidColorBrush(ChaoLine);
                remoteButton.BorderThickness = new Thickness(1);
            }
            remoteButton.ToolTip = TrainingQueueFeature
                ? "將勾選站別的模型身份、SHA256 與 Epoch 凍結後加入 sequential queue。"
                : "RemoteTraining 通訊契約尚未完成重建。";
            if (TrainingQueueFeature)
            {
                _standaloneAddQueueButton = remoteButton;
                remoteButton.Click += delegate { AddSelectedStationsToStandaloneQueue(); };
            }
            Button hybridButton = MakeOriginalButton(
                TrainingQueueFeature ? "開始排程" : "本地+遠端訓練開始",
                150,
                false);
            SetAgentAutomationId(hybridButton, "dia.training.queue.start");
            hybridButton.Height = 45;
            hybridButton.IsEnabled = TrainingQueueFeature;
            if (DeltaVisualDesign)
            {
                hybridButton.Background = new SolidColorBrush(ChaoGreen);
                hybridButton.Foreground = Brushes.White;
            }
            hybridButton.ToolTip = TrainingQueueFeature
                ? "依序執行 Queue；每站完成 Training → PB → deployment dry-run 後才進下一站。"
                : "RemoteTraining 通訊契約尚未完成重建。";
            if (TrainingQueueFeature)
            {
                _standaloneStartQueueButton = hybridButton;
                hybridButton.Click += delegate { StartStandaloneQueue(); };
            }
            trainButtons.Children.Add(_startButton);
            trainButtons.Children.Add(remoteButton);
            trainButtons.Children.Add(hybridButton);
            _agentInfoButton = MakeOriginalButton("Agent 接口", 110, false);
            _agentInfoButton.Height = 45;
            _agentInfoButton.Margin = new Thickness(8, 0, 0, 0);
            _agentInfoButton.ToolTip = "顯示智慧體控制 Pipe、AutomationId 與操作文件位置。";
            SetAgentAutomationId(_agentInfoButton, "dia.training.agent.info");
            _agentInfoButton.Click += delegate { ShowAgentInterfaceInfo(); };
            trainButtons.Children.Add(_agentInfoButton);
            training.Children.Add(trainButtons);
            Border trainingCard = new Border
            {
                Background = new SolidColorBrush(ChaoSurface),
                BorderBrush = new SolidColorBrush(ChaoLine),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Margin = new Thickness(0, 0, 12, 0),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 14,
                    ShadowDepth = 2,
                    Opacity = 0.08,
                    Color = ChaoInk
                },
                Child = training
            };
            Grid.SetColumn(trainingCard, 1);
            controls.Children.Add(trainingCard);

            Button closeButton = MakeOriginalButton("結束", 126, true);
            SetAgentAutomationId(closeButton, "dia.training.close");
            closeButton.Height = 45;
            closeButton.Margin = new Thickness(8, 10, 8, 0);
            if (DeltaVisualDesign)
            {
                closeButton.Background = new SolidColorBrush(ChaoSurface);
                closeButton.Foreground = new SolidColorBrush(ChaoMuted);
                closeButton.BorderBrush = new SolidColorBrush(ChaoLine);
                closeButton.BorderThickness = new Thickness(1);
            }
            closeButton.Click += delegate { Close(); };
            Grid.SetColumn(closeButton, 2);
            controls.Children.Add(closeButton);

            _progressBar = new ProgressBar { Visibility = Visibility.Collapsed };
            _statusText = new TextBlock
            {
                Text = "待命",
                FontWeight = FontWeights.SemiBold,
                Margin = StandaloneDesign
                    ? new Thickness(18, 226, 0, 0)
                    : new Thickness(16, 172, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(55, 48, 43))
                    : Brushes.Black
            };
            _timeText = new TextBlock
            {
                Text = "訓練時間 -- / 剩餘 --",
                FontSize = 11,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(Color.FromRgb(107, 114, 128))
                    : Brushes.DimGray,
                Margin = StandaloneDesign
                    ? new Thickness(18, 250, 0, 0)
                    : new Thickness(16, 196, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(_statusText, 1);
            Grid.SetColumn(_timeText, 1);
            controls.Children.Add(_statusText);
            controls.Children.Add(_timeText);
            _logBox = new TextBox
            {
                Visibility = Visibility.Collapsed,
                FontFamily = new FontFamily("Consolas"),
                IsReadOnly = true
            };
            controls.Children.Add(_logBox);

            _readModelButton = MakeOriginalButton("讀取模型", 132, false);
            _readModelButton.Click += delegate { ReadSelectedModel(); };
            _readModelButton.Visibility = Visibility.Collapsed;
            _convertButton = MakeOriginalButton("H5 → PB", 132, false);
            SetAgentAutomationId(_convertButton, "dia.training.convert.pb");
            _convertButton.Click += delegate { ConvertToPb(); };
            _convertButton.Visibility = Visibility.Collapsed;
            _previewButton = MakeOriginalButton("預覽", 132, false);
            SetAgentAutomationId(_previewButton, "dia.training.preview");
            _previewButton.Click += delegate { ShowOriginalStylePreview(); };
            _previewButton.Visibility = Visibility.Collapsed;
            _lossCurveButton = MakeOriginalButton("Loss 曲線", 132, false);
            SetAgentAutomationId(_lossCurveButton, "dia.training.loss.curve");
            _lossCurveButton.IsEnabled = false;
            _lossCurveButton.Click += delegate { ShowLossCurve(); };
            _lossCurveButton.Visibility = Visibility.Collapsed;
            _deployCheckButton = MakeOriginalButton("更換前檢查", 132, false);
            SetAgentAutomationId(_deployCheckButton, "dia.training.deploy.check");
            _deployCheckButton.Click += delegate { RunDeploymentCheck(); };
            _deployCheckButton.Visibility = Visibility.Collapsed;
            _replaceAllButton = MakeOriginalButton("全部更換", 132, false);
            SetAgentAutomationId(_replaceAllButton, "dia.training.deploy.apply");
            _replaceAllButton.IsEnabled = false;
            _replaceAllButton.ToolTip = "完成 Training、H5→PB 與更換前檢查後才會解鎖。";
            _replaceAllButton.Click += delegate { ApplyDeployment(); };
            _replaceAllButton.Visibility = Visibility.Collapsed;
            _terminateButton = MakeOriginalButton("終止訓練", 100, true);
            SetAgentAutomationId(_terminateButton, "dia.training.terminate");
            _terminateButton.IsEnabled = false;
            _terminateButton.Visibility = Visibility.Collapsed;
            _terminateButton.Click += delegate { TerminateActiveOperation(); };
            controls.Children.Add(_readModelButton);
            controls.Children.Add(_convertButton);
            controls.Children.Add(_previewButton);
            controls.Children.Add(_lossCurveButton);
            controls.Children.Add(_deployCheckButton);
            controls.Children.Add(_terminateButton);
            Grid.SetRow(controls, 1);
            layer.Children.Add(controls);
            return layer;
        }

        private Grid BuildOriginalResultLayer()
        {
            Grid layer = new Grid
            {
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? Colors.Transparent
                        : Color.FromRgb(126, 126, 126))
            };
            Canvas canvas = new Canvas();
            layer.Children.Add(canvas);
            if (StandaloneDesign)
            {
                canvas.Children.Add(BuildTrainingNeuralVisual());
            }
            for (int station = 1; station <= 8; station++)
            {
                int local = (station - 1) % 4;
                double x = station <= 4 ? 58 : 854;
                double y = new[] { 67d, 196d, 328d, 457d }[local];
                Border row = BuildOriginalResultRow(station);
                Canvas.SetLeft(row, x);
                Canvas.SetTop(row, y);
                canvas.Children.Add(row);
                Border stationIndex = new Border
                {
                    Width = 50,
                    Height = 106,
                    Background = new SolidColorBrush(
                        StandaloneDesign ? ChaoOrange : Color.FromRgb(62, 62, 62)),
                    CornerRadius = StandaloneDesign
                        ? new CornerRadius(12, 0, 0, 12)
                        : new CornerRadius(0),
                    Child = new TextBlock
                    {
                        Text = station.ToString(CultureInfo.InvariantCulture),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        FontSize = 22,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Canvas.SetLeft(stationIndex, station <= 4 ? 8 : 804);
                Canvas.SetTop(stationIndex, y);
                canvas.Children.Add(stationIndex);
            }

            _replaceAllButton = MakeOriginalButton("全部更換", 126, false);
            _replaceAllButton.Height = 45;
            _replaceAllButton.IsEnabled = false;
            if (StandaloneDesign)
            {
                _replaceAllButton.Background = new SolidColorBrush(ChaoOrange);
            }
            _replaceAllButton.Click += delegate { ApplyDeployment(); };
            Canvas.SetLeft(_replaceAllButton, 707);
            Canvas.SetTop(_replaceAllButton, 585);
            canvas.Children.Add(_replaceAllButton);

            _stopButton = MakeOriginalButton("中斷訓練", 112, true);
            SetAgentAutomationId(_stopButton, "dia.training.stop");
            _stopButton.Height = 38;
            _stopButton.IsEnabled = false;
            _stopButton.Click += delegate { RequestStop(); };
            Canvas.SetLeft(_stopButton, 713);
            Canvas.SetTop(_stopButton, 635);
            canvas.Children.Add(_stopButton);

            Button closeButton = MakeOriginalButton("關閉", 126, true);
            closeButton.Height = 45;
            if (StandaloneDesign)
            {
                closeButton.Background = new SolidColorBrush(ChaoSurface);
                closeButton.Foreground = new SolidColorBrush(ChaoMuted);
                closeButton.BorderBrush = new SolidColorBrush(ChaoLine);
                closeButton.BorderThickness = new Thickness(1);
            }
            closeButton.Click += delegate { Close(); };
            Canvas.SetLeft(closeButton, 1383);
            Canvas.SetTop(closeButton, 585);
            canvas.Children.Add(closeButton);

            _resultInspLog = new TextBox
            {
                Width = 675,
                Height = 107,
                IsReadOnly = true,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoSurface)
                    : new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoInk)
                    : Brushes.Black,
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : SystemColors.ControlDarkBrush
            };
            Canvas.SetLeft(_resultInspLog, 10);
            Canvas.SetTop(_resultInspLog, 630);
            canvas.Children.Add(_resultInspLog);
            _resultCommandLog = new TextBox
            {
                Width = 507,
                Height = 107,
                IsReadOnly = true,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoSurface)
                    : new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoInk)
                    : Brushes.Black,
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : SystemColors.ControlDarkBrush
            };
            Canvas.SetLeft(_resultCommandLog, 851);
            Canvas.SetTop(_resultCommandLog, 630);
            canvas.Children.Add(_resultCommandLog);
            return layer;
        }

        private UIElement BuildTrainingNeuralVisual()
        {
            Canvas visual = new Canvas { Width = 250, Height = 105, IsHitTestVisible = false, Opacity = 0.9 };
            Canvas.SetLeft(visual, 675);
            Canvas.SetTop(visual, 600);
            double[][] columns = { new[] { 16d, 50d, 84d }, new[] { 8d, 34d, 60d, 86d }, new[] { 20d, 52d, 84d }, new[] { 36d, 68d } };
            double[] xs = { 10d, 80d, 150d, 220d };
            for (int c = 0; c < columns.Length - 1; c++)
            {
                foreach (double y1 in columns[c])
                foreach (double y2 in columns[c + 1])
                    visual.Children.Add(new Line { X1 = xs[c] + 5, Y1 = y1 + 5, X2 = xs[c + 1] + 5, Y2 = y2 + 5, Stroke = new SolidColorBrush(Color.FromArgb(64, 255, 138, 52)), StrokeThickness = 1 });
            }
            for (int c = 0; c < columns.Length; c++)
            foreach (double y in columns[c])
            {
                Ellipse node = new Ellipse { Width = 10, Height = 10, Fill = new SolidColorBrush(ChaoSurface), Stroke = new SolidColorBrush(ChaoOrange), StrokeThickness = 2 };
                Canvas.SetLeft(node, xs[c]); Canvas.SetTop(node, y); visual.Children.Add(node);
            }
            visual.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { From = 0.55, To = 1.0, Duration = TimeSpan.FromSeconds(1.25), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
            return visual;
        }

        private UIElement BuildStandaloneTrainingHero()
        {
            Border hero = new Border
            {
                Height = 102,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(14, 6, 16, 6),
                Background = new LinearGradientBrush(
                    Color.FromRgb(255, 255, 255),
                    Color.FromRgb(230, 243, 254),
                    24),
                BorderBrush = new SolidColorBrush(DeltaLineStrong),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(154) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Canvas core = new Canvas
            {
                Width = 138,
                Height = 88,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            Ellipse aura = new Ellipse
            {
                Width = 94,
                Height = 94,
                Fill = new RadialGradientBrush(
                    Color.FromArgb(78, 90, 185, 244),
                    Color.FromArgb(0, 90, 185, 244)),
                Effect = new BlurEffect { Radius = 10 }
            };
            Canvas.SetLeft(aura, 22);
            Canvas.SetTop(aura, -3);
            core.Children.Add(aura);

            double[] ringSizes = { 82, 68, 54 };
            double[] ringWidths = { 2.2, 1.5, 1.0 };
            double[] ringSeconds = { 8.0, 6.2, 4.8 };
            for (int i = 0; i < ringSizes.Length; i++)
            {
                double size = ringSizes[i];
                Ellipse ring = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Stroke = new SolidColorBrush(
                        Color.FromArgb(
                            (byte)(i == 0 ? 130 : i == 1 ? 92 : 65),
                            DeltaBlue.R,
                            DeltaBlue.G,
                            DeltaBlue.B)),
                    StrokeThickness = ringWidths[i],
                    StrokeDashArray = new DoubleCollection
                    {
                        i == 0 ? 16 : 10,
                        i == 0 ? 8 : 7,
                        4,
                        6
                    },
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                RotateTransform rotate = new RotateTransform();
                ring.RenderTransform = rotate;
                Canvas.SetLeft(ring, (138 - size) / 2);
                Canvas.SetTop(ring, (88 - size) / 2);
                core.Children.Add(ring);
                rotate.BeginAnimation(
                    RotateTransform.AngleProperty,
                    new DoubleAnimation
                    {
                        From = i == 1 ? 360 : 0,
                        To = i == 1 ? 0 : 360,
                        Duration = TimeSpan.FromSeconds(ringSeconds[i]),
                        RepeatBehavior = RepeatBehavior.Forever
                    });
            }

            Border glassCore = new Border
            {
                Width = 54,
                Height = 46,
                CornerRadius = new CornerRadius(13),
                Background = new LinearGradientBrush(
                    Color.FromArgb(246, 255, 255, 255),
                    Color.FromArgb(238, 233, 245, 255),
                    35),
                BorderBrush = new SolidColorBrush(Color.FromArgb(165, 172, 209, 239)),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(glassCore, 42);
            Canvas.SetTop(glassCore, 21);
            core.Children.Add(glassCore);

            string trianglePath = Path.Combine(
                DeltaLauncherRoot,
                "assets",
                "delta_triangle.png");
            if (File.Exists(trianglePath))
            {
                Image triangle = new Image
                {
                    Source = LoadBitmap(trianglePath),
                    Width = 37,
                    Height = 37,
                    Stretch = Stretch.Uniform
                };
                Canvas.SetLeft(triangle, 50.5);
                Canvas.SetTop(triangle, 25.5);
                core.Children.Add(triangle);
            }

            Border scanner = new Border
            {
                Width = 16,
                Height = 42,
                Opacity = 0.58,
                Background = new LinearGradientBrush(
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb(160, 255, 255, 255),
                    0)
            };
            TranslateTransform scanMove = new TranslateTransform(-8, 0);
            scanner.RenderTransform = scanMove;
            Canvas.SetLeft(scanner, 43);
            Canvas.SetTop(scanner, 23);
            core.Children.Add(scanner);
            scanMove.BeginAnimation(
                TranslateTransform.XProperty,
                new DoubleAnimation
                {
                    From = -12,
                    To = 46,
                    Duration = TimeSpan.FromSeconds(2.2),
                    RepeatBehavior = RepeatBehavior.Forever
                });

            Ellipse pulse = new Ellipse
            {
                Width = 9,
                Height = 9,
                Fill = new SolidColorBrush(DeltaGreen),
                Effect = new DropShadowEffect
                {
                    Color = DeltaGreen,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.65
                }
            };
            Canvas.SetLeft(pulse, 64.5);
            Canvas.SetTop(pulse, 72);
            pulse.BeginAnimation(
                UIElement.OpacityProperty,
                new DoubleAnimation
                {
                    From = 0.4,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1.1),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                });
            core.Children.Add(pulse);
            grid.Children.Add(core);

            StackPanel copy = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            copy.Children.Add(new TextBlock
            {
                Text = "DELTA TRAINING CORE",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(DeltaTitle)
            });
            copy.Children.Add(new TextBlock
            {
                Text = "MODEL LOCK  →  TRAIN  →  PB  →  VERIFY  →  NEXT",
                FontFamily = new FontFamily("Cascadia Mono, Consolas"),
                FontSize = 10.5,
                Foreground = new SolidColorBrush(Color.FromRgb(35, 104, 161)),
                Margin = new Thickness(0, 4, 0, 0)
            });
            copy.Children.Add(new TextBlock
            {
                Text = "每站先鎖定模型身份與 SHA256，驗證完成才放行下一站。",
                FontSize = 11,
                Foreground = new SolidColorBrush(DeltaMuted),
                Margin = new Thickness(0, 3, 0, 0)
            });
            Grid.SetColumn(copy, 1);
            grid.Children.Add(copy);

            hero.Child = grid;
            return hero;
        }

        private Border BuildOriginalResultRow(int station)
        {
            Border border = new Border
            {
                Width = 685,
                Height = 106,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoSurface)
                    : Brushes.White,
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : Brushes.Transparent,
                BorderThickness = StandaloneDesign
                    ? new Thickness(1)
                    : new Thickness(0),
                CornerRadius = StandaloneDesign
                    ? new CornerRadius(0, 12, 12, 0)
                    : new CornerRadius(0)
            };
            Canvas canvas = new Canvas();
            border.Child = canvas;
            int index = station - 1;
            ProgressBar trainProgress = new ProgressBar
            {
                Width = 284,
                Height = 40,
                Minimum = 0,
                Maximum = 100,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoOrangeSoft)
                    : new SolidColorBrush(Color.FromRgb(199, 180, 180))
            };
            _stationTrainProgressBars[index] = trainProgress;
            Canvas.SetLeft(trainProgress, 15);
            Canvas.SetTop(trainProgress, 12);
            canvas.Children.Add(trainProgress);
            TextBlock trainText = ResultValueText("0", 59, 28);
            _stationTrainProgressTexts[index] = trainText;
            Canvas.SetLeft(trainText, 137);
            Canvas.SetTop(trainText, 18);
            canvas.Children.Add(trainText);

            ProgressBar testProgress = new ProgressBar
            {
                Width = 284,
                Height = 40,
                Minimum = 0,
                Maximum = 100,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoBlueSoft)
                    : new SolidColorBrush(Color.FromRgb(164, 164, 164))
            };
            _stationTestProgressBars[index] = testProgress;
            Canvas.SetLeft(testProgress, 15);
            Canvas.SetTop(testProgress, 57);
            canvas.Children.Add(testProgress);
            TextBlock testText = ResultValueText("0", 59, 28);
            _stationTestProgressTexts[index] = testText;
            Canvas.SetLeft(testText, 136);
            Canvas.SetTop(testText, 63);
            canvas.Children.Add(testText);

            TextBlock accuracy = ResultMetricText(
                "0",
                StandaloneDesign ? ChaoOrangeSoft : Color.FromRgb(255, 252, 218));
            _stationAccuracyTexts[index] = accuracy;
            accuracy.ToolTip = "原廠欄位名為 LabAcc，但 genuine train.exe 寫入的是 logs['loss']。";
            Canvas.SetLeft(accuracy, 314);
            Canvas.SetTop(accuracy, 31);
            canvas.Children.Add(accuracy);
            TextBlock yield = ResultMetricText(
                "0",
                StandaloneDesign ? ChaoCream : Colors.White);
            _stationYieldTexts[index] = yield;
            yield.ToolTip = "舊模型 OfflineBufTest(TestImageSet, false, true) 回傳的 FLOW_OK 百分比。";
            Canvas.SetLeft(yield, 413);
            Canvas.SetTop(yield, 31);
            canvas.Children.Add(yield);
            TextBlock newYield = ResultMetricText(
                "0",
                StandaloneDesign ? ChaoGreenSoft : Color.FromRgb(251, 251, 251));
            _stationNewYieldTexts[index] = newYield;
            newYield.ToolTip = "新模型 OfflineBufTest(TestImageSet, false, true) 回傳的 FLOW_OK 百分比。";
            Canvas.SetLeft(newYield, 513);
            Canvas.SetTop(newYield, 31);
            canvas.Children.Add(newYield);

            Button preview = MakeOriginalButton("預覽", 53, false);
            preview.Height = 39;
            preview.Background = new SolidColorBrush(
                StandaloneDesign ? ChaoBlue : Color.FromRgb(26, 251, 251));
            preview.IsEnabled = false;
            preview.Margin = new Thickness(0);
            preview.Click += delegate
            {
                SelectStation(station);
                ShowOriginalStylePreview();
            };
            _stationPreviewButtons[index] = preview;
            Canvas.SetLeft(preview, 616);
            Canvas.SetTop(preview, 10);
            canvas.Children.Add(preview);

            Button update = MakeOriginalButton("更換", 53, false);
            update.Height = 39;
            update.Background = new SolidColorBrush(
                StandaloneDesign ? ChaoOrange : Color.FromRgb(249, 193, 4));
            update.IsEnabled = false;
            update.Margin = new Thickness(0);
            update.Click += delegate
            {
                SelectStation(station);
                ApplyDeployment();
            };
            _stationUpdateButtons[index] = update;
            Canvas.SetLeft(update, 616);
            Canvas.SetTop(update, 58);
            canvas.Children.Add(update);
            return border;
        }

        private static TextBlock ResultValueText(string value, double width, double height)
        {
            return new TextBlock
            {
                Text = value,
                Width = width,
                Height = height,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoInk)
                    : Brushes.Black
            };
        }

        private static TextBlock ResultMetricText(string value, Color background)
        {
            return new TextBlock
            {
                Text = value,
                Width = 85,
                Height = 38,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 9, 0, 0),
                Background = new SolidColorBrush(background),
                FontWeight = FontWeights.SemiBold,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoInk)
                    : Brushes.Black
            };
        }

        private void ShowResultTrainingLayer()
        {
            if (_mainTrainingLayer != null)
            {
                _mainTrainingLayer.Visibility = Visibility.Hidden;
            }
            if (_resultTrainingLayer != null)
            {
                _resultTrainingLayer.Visibility = Visibility.Visible;
            }
        }

        private void ShowMainTrainingLayer()
        {
            if (_resultTrainingLayer != null)
            {
                _resultTrainingLayer.Visibility = Visibility.Hidden;
            }
            if (_mainTrainingLayer != null)
            {
                _mainTrainingLayer.Visibility = Visibility.Visible;
            }
        }

        private Border BuildStationCard(int station)
        {
            Border card = new Border
            {
                Width = 371,
                Height = 215,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoSurface)
                    : new SolidColorBrush(GetOriginalStationColor(station)),
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : Brushes.Transparent,
                BorderThickness = StandaloneDesign ? new Thickness(1) : new Thickness(0),
                CornerRadius = StandaloneDesign ? new CornerRadius(16) : new CornerRadius(0),
                Effect = StandaloneDesign
                    ? (Effect)new DropShadowEffect
                    {
                        BlurRadius = 14,
                        ShadowDepth = 2,
                        Opacity = 0.08,
                        Color = ChaoInk
                    }
                    : null,
                Cursor = System.Windows.Input.Cursors.Hand,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0)
            };
            Canvas panel = new Canvas();

            Border stationBadge = new Border
            {
                Width = 55,
                Height = 55,
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoOrange
                        : Color.FromRgb(65, 65, 65)),
                CornerRadius = StandaloneDesign
                    ? new CornerRadius(15, 0, 15, 0)
                    : new CornerRadius(0),
                Child = new TextBlock
                {
                    Text = station.ToString(CultureInfo.InvariantCulture),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Canvas.SetLeft(stationBadge, 0);
            Canvas.SetTop(stationBadge, 0);
            panel.Children.Add(stationBadge);

            Border dnetBadge = new Border
            {
                Width = 55,
                Height = 30,
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoOrangeSoft
                        : Color.FromRgb(221, 255, 3)),
                CornerRadius = StandaloneDesign
                    ? new CornerRadius(0, 0, 14, 0)
                    : new CornerRadius(0),
                Child = new TextBlock
                {
                    Text = "DNET",
                    Foreground = StandaloneDesign
                        ? new SolidColorBrush(ChaoOrangeDeep)
                        : Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold
                }
            };
            Canvas.SetLeft(dnetBadge, 0);
            Canvas.SetTop(dnetBadge, 55);
            panel.Children.Add(dnetBadge);

            Border checkPanel = new Border
            {
                Width = 55,
                Height = 55,
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoCream)
                    : Brushes.White,
                CornerRadius = StandaloneDesign
                    ? new CornerRadius(0, 15, 0, 15)
                    : new CornerRadius(0)
            };
            CheckBox trainCheck = new CheckBox
            {
                IsChecked = station == _selectedStation,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            SetAgentAutomationId(
                trainCheck,
                "dia.training.station." + station.ToString(CultureInfo.InvariantCulture) + ".select");
            _stationTrainChecks[station - 1] = trainCheck;
            checkPanel.Child = trainCheck;
            Canvas.SetLeft(checkPanel, 316);
            Canvas.SetTop(checkPanel, 0);
            panel.Children.Add(checkPanel);

            if (StandaloneDesign)
            {
                Border modelBand = new Border
                {
                    Width = 261,
                    Height = 55,
                    Background = new SolidColorBrush(ChaoSurface),
                    BorderBrush = new SolidColorBrush(ChaoLine),
                    BorderThickness = new Thickness(0, 0, 0, 1)
                };
                Canvas.SetLeft(modelBand, 55);
                Canvas.SetTop(modelBand, 0);
                panel.Children.Add(modelBand);

                Border accentLine = new Border
                {
                    Width = 261,
                    Height = 4,
                    Background = new SolidColorBrush(ChaoOrange)
                };
                Canvas.SetLeft(accentLine, 55);
                Canvas.SetTop(accentLine, 0);
                panel.Children.Add(accentLine);
            }

            _stationModelTexts[station - 1] = new TextBlock
            {
                Text = "--",
                Width = StandaloneDesign ? 245 : 240,
                Height = 30,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoInk
                        : Color.FromRgb(70, 70, 70)),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 6, 0, 0)
            };
            Canvas.SetLeft(_stationModelTexts[station - 1], StandaloneDesign ? 65 : 10);
            Canvas.SetTop(_stationModelTexts[station - 1], 13);
            panel.Children.Add(_stationModelTexts[station - 1]);

            TextBlock okLabel = OriginalStationLabel("OK 訓練影像");
            Canvas.SetLeft(okLabel, 40);
            Canvas.SetTop(okLabel, 77);
            panel.Children.Add(okLabel);
            _stationOkTexts[station - 1] = OriginalStationCounter("0");
            Canvas.SetLeft(_stationOkTexts[station - 1], 160);
            Canvas.SetTop(_stationOkTexts[station - 1], 73);
            panel.Children.Add(_stationOkTexts[station - 1]);
            Button openOkButton = new Button
            {
                Content = "開啟",
                Width = 36,
                Height = 36,
                FontSize = 9,
                Margin = new Thickness(0),
                Padding = new Thickness(2, 0, 2, 0),
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoOrangeSoft)
                    : SystemColors.ControlBrush,
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : SystemColors.ControlDarkBrush,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoOrangeDeep)
                    : Brushes.Black,
                Template = StandaloneDesign ? CreateRoundedButtonTemplate(7) : null
            };
            SetAgentAutomationId(
                openOkButton,
                "dia.training.station." + station.ToString(CultureInfo.InvariantCulture) + ".open.ok");
            openOkButton.Click += delegate(object sender, RoutedEventArgs args)
            {
                args.Handled = true;
                OpenTrainingImageFolder(station, "OK");
            };
            Canvas.SetLeft(openOkButton, 245);
            Canvas.SetTop(openOkButton, 72);
            panel.Children.Add(openOkButton);

            TextBlock ngLabel = OriginalStationLabel("NG 訓練影像");
            Canvas.SetLeft(ngLabel, 40);
            Canvas.SetTop(ngLabel, 118);
            panel.Children.Add(ngLabel);
            _stationNgTexts[station - 1] = OriginalStationCounter("0");
            Canvas.SetLeft(_stationNgTexts[station - 1], 160);
            Canvas.SetTop(_stationNgTexts[station - 1], 114);
            panel.Children.Add(_stationNgTexts[station - 1]);
            Button openNgButton = new Button
            {
                Content = "開啟",
                Width = 36,
                Height = 36,
                FontSize = 9,
                Margin = new Thickness(0),
                Padding = new Thickness(2, 0, 2, 0),
                Background = StandaloneDesign
                    ? new SolidColorBrush(ChaoOrangeSoft)
                    : SystemColors.ControlBrush,
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : SystemColors.ControlDarkBrush,
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoOrangeDeep)
                    : Brushes.Black,
                Template = StandaloneDesign ? CreateRoundedButtonTemplate(7) : null
            };
            SetAgentAutomationId(
                openNgButton,
                "dia.training.station." + station.ToString(CultureInfo.InvariantCulture) + ".open.ng");
            openNgButton.Click += delegate(object sender, RoutedEventArgs args)
            {
                args.Handled = true;
                OpenTrainingImageFolder(station, "NG");
            };
            Canvas.SetLeft(openNgButton, 245);
            Canvas.SetTop(openNgButton, 113);
            panel.Children.Add(openNgButton);

            TextBlock trainingCountLabel = OriginalStationLabel("訓練次數");
            Canvas.SetLeft(trainingCountLabel, 40);
            Canvas.SetTop(trainingCountLabel, 159);
            panel.Children.Add(trainingCountLabel);
            TextBox epoch = new TextBox
            {
                Text = "50",
                Width = 77,
                Height = 33,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoCream
                        : Color.FromRgb(241, 236, 120)),
                BorderBrush = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoOrange
                        : Color.FromRgb(215, 205, 120)),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoInk)
                    : Brushes.Black
            };
            SetAgentAutomationId(
                epoch,
                "dia.training.station." + station.ToString(CultureInfo.InvariantCulture) + ".epochs");
            _stationEpochBoxes[station - 1] = epoch;
            Canvas.SetLeft(epoch, 160);
            Canvas.SetTop(epoch, 155);
            panel.Children.Add(epoch);
            Button paramsButton = new Button
            {
                Content = "影像參數設定",
                Width = 104,
                Height = 34,
                FontSize = 10,
                Padding = new Thickness(5, 1, 5, 1),
                Margin = new Thickness(0),
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoOrangeSoft
                        : Color.FromRgb(255, 166, 91)),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoOrangeDeep)
                    : Brushes.White,
                BorderBrush = StandaloneDesign
                    ? new SolidColorBrush(ChaoLine)
                    : Brushes.Transparent,
                BorderThickness = StandaloneDesign ? new Thickness(1) : new Thickness(0),
                Template = StandaloneDesign ? CreateRoundedButtonTemplate(8) : null
            };
            paramsButton.Click += delegate(object sender, RoutedEventArgs args)
            {
                args.Handled = true;
                SelectStation(station);
                ShowImageParameterDialog();
            };
            Canvas.SetLeft(paramsButton, 245);
            Canvas.SetTop(paramsButton, 155);
            panel.Children.Add(paramsButton);

            card.Child = panel;
            int capturedStation = station;
            card.MouseEnter += delegate
            {
                if (!StandaloneDesign)
                {
                    return;
                }
                bool selected = capturedStation == _selectedStation;
                AnimateStandaloneStationCard(
                    card,
                    selected ? 1.05 : 0.995,
                    selected ? 1.0 : 0.94,
                    150);
            };
            card.MouseLeave += delegate
            {
                if (!StandaloneDesign)
                {
                    return;
                }
                bool selected = capturedStation == _selectedStation;
                AnimateStandaloneStationCard(
                    card,
                    selected ? 1.035 : 0.965,
                    selected ? 1.0 : 0.80,
                    180);
            };
            card.MouseLeftButtonDown += delegate
            {
                SelectStation(capturedStation);
            };
            _stationCards[station - 1] = card;
            return card;
        }

        private static Color GetOriginalStationColor(int station)
        {
            Color[] colors =
            {
                Color.FromRgb(126, 208, 255),
                Color.FromRgb(253, 137, 137),
                Color.FromRgb(211, 199, 253),
                Color.FromRgb(34, 222, 214),
                Color.FromRgb(208, 126, 255),
                Color.FromRgb(253, 195, 137),
                Color.FromRgb(253, 199, 199),
                Color.FromRgb(58, 191, 100)
            };
            return station >= 1 && station <= colors.Length
                ? colors[station - 1]
                : Colors.White;
        }

        private static TextBlock OriginalStationLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Width = 115,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 5, 0, 0),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoMuted)
                    : Brushes.Black
            };
        }

        private static TextBlock OriginalStationCounter(string text)
        {
            return new TextBlock
            {
                Text = text,
                Width = 77,
                Height = 33,
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? ChaoCream
                        : Color.FromRgb(212, 212, 212)),
                Foreground = StandaloneDesign
                    ? new SolidColorBrush(ChaoInk)
                    : Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 7, 0, 0)
            };
        }

        private Grid BuildStationProgressRow(
            int station,
            string caption,
            ProgressBar[] progressBars,
            TextBlock[] progressTexts)
        {
            Grid row = new Grid { Margin = new Thickness(8, 1, 8, 1) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            row.Children.Add(new TextBlock
            {
                Text = caption,
                FontSize = 10,
                Foreground = Brushes.DimGray,
                VerticalAlignment = VerticalAlignment.Center
            });
            ProgressBar progress = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Height = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            progressBars[station - 1] = progress;
            Grid.SetColumn(progress, 1);
            row.Children.Add(progress);
            TextBlock text = new TextBlock
            {
                Text = "0%",
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            progressTexts[station - 1] = text;
            Grid.SetColumn(text, 2);
            row.Children.Add(text);
            return row;
        }

        private static TextBlock MakeStationCountText(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(10, 3, 54, 2),
                Foreground = new SolidColorBrush(Color.FromRgb(75, 75, 75)),
                FontSize = 11
            };
        }

        private void OpenTrainingImageFolder(int station, string category)
        {
            try
            {
                if (station < 1 || station > 8)
                {
                    throw new ArgumentOutOfRangeException("station");
                }
                if (string.IsNullOrWhiteSpace(_recipePath) || !Directory.Exists(_recipePath))
                {
                    throw new DirectoryNotFoundException("目前沒有有效 Recipe 路徑。");
                }

                DirectoryInfo recipe = new DirectoryInfo(_recipePath);
                DirectoryInfo recipeContainer = recipe.Parent;
                DirectoryInfo machineRoot = recipeContainer != null ? recipeContainer.Parent : null;
                if (machineRoot == null)
                {
                    throw new InvalidOperationException("無法由 Recipe 推導機台根目錄。");
                }

                string stationIni = Path.Combine(
                    _recipePath,
                    "station" + station.ToString(CultureInfo.InvariantCulture) + ".ini");
                Dictionary<string, string> aiConfig = ReadIniSection(stationIni, "AIEnhanceTool");
                string modelName = GetDictionaryValue(aiConfig, "ModelName_A", string.Empty);
                string datasetRoot = ResolveDatasetRoot(
                    _projectRoot,
                    machineRoot.FullName,
                    recipe.Name,
                    modelName,
                    station);
                string folder = Path.Combine(datasetRoot, "X", category);
                if (!Directory.Exists(folder))
                {
                    throw new DirectoryNotFoundException("找不到訓練影像資料夾：" + folder);
                }

                Process.Start("explorer.exe", "\"" + folder + "\"");
                AppendLog("OPEN TRAINING IMAGE FOLDER S" + station + " " + category + "=" + folder);
            }
            catch (Exception exception)
            {
                HandleFailure("開啟訓練影像資料夾失敗", exception);
            }
        }

        private static void SetAgentAutomationId(FrameworkElement element, string automationId)
        {
            if (element == null || string.IsNullOrWhiteSpace(automationId))
            {
                return;
            }
            System.Windows.Automation.AutomationProperties.SetAutomationId(
                element,
                automationId);
        }

        private void ShowAgentInterfaceInfo()
        {
            string doc = Path.Combine(_projectRoot, "docs", "AGENT_CONTROL.md");
            string contract = Path.Combine(_projectRoot, "docs", "AgentControl.contract.json");
            string ready = string.IsNullOrWhiteSpace(_agentReadyPath)
                ? "<尚未發布>"
                : _agentReadyPath;
            MessageBox.Show(
                this,
                "Agent 控制介面已啟用。\n\n" +
                "Pipe: " + (_agentPipeName ?? "<尚未啟動>") + "\n" +
                "Discovery: " + ready + "\n" +
                "文件: " + doc + "\n" +
                "契約: " + contract + "\n\n" +
                "UI AutomationId 與 Named Pipe command 都使用固定 ID，" +
                "智慧體不需要依賴按鈕座標。",
                "DIA Training Agent Interface",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void StartTrainingAgentPipe()
        {
            if (!string.IsNullOrWhiteSpace(_agentPipeName))
            {
                return;
            }

            int pid = Process.GetCurrentProcess().Id;
            _agentPipeName = "DIAChip8Side.TemplateHost.Training." +
                pid.ToString(CultureInfo.InvariantCulture);
            string agentDirectory = Path.Combine(_projectRoot, "logs", "AgentBridge");
            Directory.CreateDirectory(agentDirectory);
            _agentReadyPath = Path.Combine(agentDirectory, "training.ready.json");
            File.WriteAllText(
                _agentReadyPath,
                "{\n" +
                "  \"protocol\": \"DIAHOST-TRAINING/1\",\n" +
                "  \"pid\": " + pid.ToString(CultureInfo.InvariantCulture) + ",\n" +
                "  \"pipe\": \"" + JsonEscape(_agentPipeName) + "\",\n" +
                "  \"recipe\": \"" + JsonEscape(_recipePath) + "\",\n" +
                "  \"station\": " + _selectedStation.ToString(CultureInfo.InvariantCulture) + ",\n" +
                "  \"windowTitle\": \"" + JsonEscape(Title) + "\",\n" +
                "  \"contract\": \"docs/AgentControl.contract.json\"\n" +
                "}\n",
                new UTF8Encoding(false));
            _trace("TrainingAgent: READY pipe=" + _agentPipeName + " discovery=" + _agentReadyPath);

            Task.Run(() =>
            {
                while (!_agentPipeStopping)
                {
                    try
                    {
                        using (NamedPipeServerStream server = new NamedPipeServerStream(
                            _agentPipeName,
                            PipeDirection.InOut,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.None))
                        {
                            server.WaitForConnection();
                            string command;
                            using (StreamReader reader = new StreamReader(
                                server,
                                new UTF8Encoding(false),
                                false,
                                1024,
                                true))
                            {
                                command = reader.ReadLine();
                            }

                            string response = Dispatcher.Invoke(
                                new Func<string>(() => ExecuteTrainingAgentCommand(command)),
                                DispatcherPriority.Normal);
                            using (StreamWriter writer = new StreamWriter(
                                server,
                                new UTF8Encoding(false),
                                1024,
                                true))
                            {
                                writer.AutoFlush = true;
                                writer.WriteLine(response);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!_agentPipeStopping)
                        {
                            _trace("TrainingAgent: pipe error " + exception.GetType().Name + ": " + exception.Message);
                        }
                    }
                }
            });
        }

        private string ExecuteTrainingAgentCommand(string line)
        {
            try
            {
                string[] tokens = (line ?? string.Empty).Split('|');
                string action = tokens.Length > 0
                    ? tokens[0].Trim().ToUpperInvariant()
                    : string.Empty;
                Dictionary<string, string> values = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                for (int i = 1; i < tokens.Length; i++)
                {
                    int equals = tokens[i].IndexOf('=');
                    if (equals > 0)
                    {
                        values[tokens[i].Substring(0, equals).Trim()] =
                            tokens[i].Substring(equals + 1).Trim();
                    }
                }

                if (action == "PING" || action == "STATE")
                {
                    bool running = _activeProcess != null && !_activeProcess.HasExited;
                    return "OK|PROTO=DIAHOST-TRAINING/1|ACTION=" + action +
                        "|STATION=" + _selectedStation.ToString(CultureInfo.InvariantCulture) +
                        "|EPOCH=" + (_epochBox != null ? _epochBox.Text : string.Empty) +
                        "|RUNNING=" + (running ? "1" : "0") +
                        "|PID=" + Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture) +
                        "|MODELTYPE=" + (_workspaceModelType ?? string.Empty) +
                        "|STATUS=" + SanitizeAgentValue(_statusText != null ? _statusText.Text : string.Empty);
                }

                if (action == "SELECT_STATION")
                {
                    int station;
                    if (!values.ContainsKey("station") ||
                        !int.TryParse(values["station"], out station) ||
                        station < 1 || station > 8)
                    {
                        return "ERROR|BAD_STATION";
                    }
                    SelectStation(station);
                    return "OK|ACTION=SELECT_STATION|STATION=" + station.ToString(CultureInfo.InvariantCulture);
                }

                if (action == "SET_EPOCHS")
                {
                    int epochs;
                    if (!values.ContainsKey("value") ||
                        !int.TryParse(values["value"], out epochs) ||
                        epochs < 1 || epochs > 100000)
                    {
                        return "ERROR|BAD_EPOCHS";
                    }
                    _epochBox.Text = epochs.ToString(CultureInfo.InvariantCulture);
                    if (_stationEpochBoxes[_selectedStation - 1] != null)
                    {
                        _stationEpochBoxes[_selectedStation - 1].Text = _epochBox.Text;
                    }
                    return "OK|ACTION=SET_EPOCHS|VALUE=" + epochs.ToString(CultureInfo.InvariantCulture);
                }

                if (action == "START_TRAINING")
                {
                    StartTraining();
                    return "OK|ACTION=START_TRAINING|STATION=" + _selectedStation.ToString(CultureInfo.InvariantCulture);
                }
                if (action == "STOP_TRAINING")
                {
                    RequestStop();
                    return "OK|ACTION=STOP_TRAINING";
                }
                if (action == "ADD_QUEUE")
                {
                    AddSelectedStationsToStandaloneQueue();
                    return "OK|ACTION=ADD_QUEUE|COUNT=" + _standaloneQueue.Count.ToString(CultureInfo.InvariantCulture);
                }
                if (action == "START_QUEUE")
                {
                    StartStandaloneQueue();
                    return "OK|ACTION=START_QUEUE|COUNT=" + _standaloneQueue.Count.ToString(CultureInfo.InvariantCulture);
                }
                if (action == "OPEN_OK_FOLDER")
                {
                    OpenTrainingImageFolder(_selectedStation, "OK");
                    return "OK|ACTION=OPEN_OK_FOLDER";
                }
                if (action == "OPEN_NG_FOLDER")
                {
                    OpenTrainingImageFolder(_selectedStation, "NG");
                    return "OK|ACTION=OPEN_NG_FOLDER";
                }
                if (action == "SHOW_LOSS")
                {
                    ShowLossCurve();
                    return "OK|ACTION=SHOW_LOSS";
                }
                if (action == "DEPLOY_CHECK")
                {
                    RunDeploymentCheck();
                    return "OK|ACTION=DEPLOY_CHECK";
                }
                if (action == "APPLY_DEPLOYMENT")
                {
                    string confirm;
                    if (!values.TryGetValue("confirm", out confirm) ||
                        !string.Equals(confirm, "APPLY", StringComparison.Ordinal))
                    {
                        return "ERROR|CONFIRM_REQUIRED|EXPECTED=confirm=APPLY";
                    }
                    ApplyDeployment();
                    return "OK|ACTION=APPLY_DEPLOYMENT";
                }
                if (action == "SHOW")
                {
                    if (!IsVisible) Show();
                    if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
                    Activate();
                    return "OK|ACTION=SHOW";
                }
                if (action == "CLOSE")
                {
                    bool running = _activeProcess != null && !_activeProcess.HasExited;
                    if (running)
                    {
                        return "ERROR|TRAINING_RUNNING|USE=STOP_TRAINING";
                    }
                    Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Background);
                    return "OK|ACTION=CLOSE";
                }

                return "ERROR|UNKNOWN_ACTION|ACTION=" + SanitizeAgentValue(action);
            }
            catch (Exception exception)
            {
                _trace("TrainingAgent: command failed " + exception);
                return "ERROR|EXCEPTION|TYPE=" + exception.GetType().Name +
                    "|MESSAGE=" + SanitizeAgentValue(exception.Message);
            }
        }

        private static string JsonEscape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private static string SanitizeAgentValue(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string FormatLossScientific(string value)
        {
            double parsed;
            if (!double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out parsed) ||
                double.IsNaN(parsed) ||
                double.IsInfinity(parsed))
            {
                return value ?? string.Empty;
            }
            return parsed.ToString("0.00000000E+00", CultureInfo.InvariantCulture);
        }

        private static Button MakeOriginalButton(string text, double width, bool danger)
        {
            return new Button
            {
                Content = text,
                Width = width,
                Height = 30,
                Margin = new Thickness(0, 0, 6, 6),
                Padding = new Thickness(6, 2, 6, 2),
                Background = new SolidColorBrush(
                    StandaloneDesign
                        ? (danger
                            ? Color.FromRgb(220, 38, 38)
                            : Color.FromRgb(37, 99, 235))
                        : (danger
                            ? Color.FromRgb(224, 45, 45)
                            : Color.FromRgb(239, 153, 58))),
                Foreground = Brushes.White,
                BorderBrush = StandaloneDesign ? Brushes.Transparent : SystemColors.ControlDarkBrush,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Template = StandaloneDesign ? CreateRoundedButtonTemplate(8) : null
            };
        }

        private static ControlTemplate CreateRoundedButtonTemplate(double radius)
        {
            FrameworkElementFactory border =
                new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
            border.SetBinding(
                Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(
                Border.BorderBrushProperty,
                new System.Windows.Data.Binding("BorderBrush")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(
                Border.BorderThicknessProperty,
                new System.Windows.Data.Binding("BorderThickness")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });

            FrameworkElementFactory presenter =
                new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(
                FrameworkElement.HorizontalAlignmentProperty,
                HorizontalAlignment.Center);
            presenter.SetValue(
                FrameworkElement.VerticalAlignmentProperty,
                VerticalAlignment.Center);
            presenter.SetBinding(
                ContentPresenter.ContentProperty,
                new System.Windows.Data.Binding("Content")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.AppendChild(presenter);

            return new ControlTemplate(typeof(Button))
            {
                VisualTree = border
            };
        }

        private void BuildHiddenAdvancedControls(Grid root)
        {
            _stationBox = MakeTextBox(_selectedStation.ToString(CultureInfo.InvariantCulture));
            _gpuBox = MakeTextBox("0");
            _xPathBox = MakeTextBox(string.Empty);
            _yPathBox = MakeTextBox(string.Empty);
            _outputRootBox = MakeTextBox(string.Empty);
            _widthBox = MakeTextBox("256");
            _heightBox = MakeTextBox("256");
            _epochBox = _stationEpochBoxes[_selectedStation - 1];
            _batchBox = MakeTextBox("8");
            _lossBox = MakeTextBox("6");
            _learningRateBox = MakeTextBox("0.001");
            _unetDivBox = MakeTextBox("8");
            _augRatioBox = MakeTextBox("1");
            _maxSamplesBox = MakeTextBox("0");
            _stationInfoText = new TextBlock
            {
                Text = "Station 資訊尚未載入",
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 6, 0, 0)
            };

            Grid form = new Grid { Margin = new Thickness(8) };
            for (int i = 0; i < 4; i++)
            {
                form.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = i % 2 == 0
                        ? new GridLength(100)
                        : new GridLength(1, GridUnitType.Star)
                });
            }
            int row = 0;
            AddPairExisting(form, ref row, "Station", _stationBox, "GPU", _gpuBox);
            AddWideExisting(form, ref row, "X 影像", _xPathBox);
            AddWideExisting(form, ref row, "Y 標註", _yPathBox);
            AddWideExisting(form, ref row, "輸出", _outputRootBox);
            AddPairExisting(form, ref row, "Width", _widthBox, "Height", _heightBox);
            AddPairExisting(form, ref row, "Batch", _batchBox, "Loss", _lossBox);
            AddPairExisting(form, ref row, "LR", _learningRateBox, "UnetDiv", _unetDivBox);
            AddPairExisting(form, ref row, "AugRatio", _augRatioBox, "驗證上限", _maxSamplesBox);
            EnsureRow(form, row);
            Grid.SetRow(_stationInfoText, row);
            Grid.SetColumn(_stationInfoText, 0);
            Grid.SetColumnSpan(_stationInfoText, 4);
            form.Children.Add(_stationInfoText);

            _advancedExpander = new Expander
            {
                Header = "重建診斷 / 進階參數",
                Content = form,
                IsExpanded = false,
                Visibility = Visibility.Collapsed
            };
            root.Children.Add(_advancedExpander);
        }

        private static void AddPairExisting(
            Grid grid,
            ref int row,
            string label1,
            TextBox box1,
            string label2,
            TextBox box2)
        {
            EnsureRow(grid, row);
            TextBlock l1 = MakeLabel(label1);
            TextBlock l2 = MakeLabel(label2);
            Grid.SetRow(l1, row);
            Grid.SetColumn(l1, 0);
            Grid.SetRow(box1, row);
            Grid.SetColumn(box1, 1);
            Grid.SetRow(l2, row);
            Grid.SetColumn(l2, 2);
            Grid.SetRow(box2, row);
            Grid.SetColumn(box2, 3);
            grid.Children.Add(l1);
            grid.Children.Add(box1);
            grid.Children.Add(l2);
            grid.Children.Add(box2);
            row++;
        }

        private static void AddWideExisting(
            Grid grid,
            ref int row,
            string label,
            TextBox box)
        {
            EnsureRow(grid, row);
            TextBlock l = MakeLabel(label);
            Grid.SetRow(l, row);
            Grid.SetColumn(l, 0);
            Grid.SetRow(box, row);
            Grid.SetColumn(box, 1);
            Grid.SetColumnSpan(box, 3);
            grid.Children.Add(l);
            grid.Children.Add(box);
            row++;
        }

        private void SelectStation(int station)
        {
            if (station < 1 || station > 8)
            {
                return;
            }

            if (_activeProcess != null && !_activeProcess.HasExited)
            {
                MessageBox.Show(
                    this,
                    "Training 執行中不可切換站別。",
                    "模型訓練工具",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _selectedStation = station;
            _stationBox.Text = station.ToString(CultureInfo.InvariantCulture);
            _epochBox = _stationEpochBoxes[station - 1];
            UpdateStationSelectionVisual();
            ApplyRecipeDefaults();
            UpdateLossCurveAvailability();
        }

        private void HandleStandaloneGameNavigation(
            object sender,
            System.Windows.Input.KeyEventArgs args)
        {
            if (!StandaloneBehavior)
            {
                return;
            }

            object focused = System.Windows.Input.Keyboard.FocusedElement;
            if (focused is TextBox || focused is ComboBox)
            {
                return;
            }

            int target = _selectedStation;
            switch (args.Key)
            {
                case System.Windows.Input.Key.Left:
                case System.Windows.Input.Key.A:
                    target = _selectedStation <= 1 ? 8 : _selectedStation - 1;
                    break;
                case System.Windows.Input.Key.Right:
                case System.Windows.Input.Key.D:
                    target = _selectedStation >= 8 ? 1 : _selectedStation + 1;
                    break;
                case System.Windows.Input.Key.D1:
                case System.Windows.Input.Key.NumPad1:
                    target = 1;
                    break;
                case System.Windows.Input.Key.D2:
                case System.Windows.Input.Key.NumPad2:
                    target = 2;
                    break;
                case System.Windows.Input.Key.D3:
                case System.Windows.Input.Key.NumPad3:
                    target = 3;
                    break;
                case System.Windows.Input.Key.D4:
                case System.Windows.Input.Key.NumPad4:
                    target = 4;
                    break;
                case System.Windows.Input.Key.D5:
                case System.Windows.Input.Key.NumPad5:
                    target = 5;
                    break;
                case System.Windows.Input.Key.D6:
                case System.Windows.Input.Key.NumPad6:
                    target = 6;
                    break;
                case System.Windows.Input.Key.D7:
                case System.Windows.Input.Key.NumPad7:
                    target = 7;
                    break;
                case System.Windows.Input.Key.D8:
                case System.Windows.Input.Key.NumPad8:
                    target = 8;
                    break;
                default:
                    return;
            }

            if (target != _selectedStation)
            {
                SelectStation(target);
            }
            args.Handled = true;
        }

        private static void AnimateStandaloneStationCard(
            Border card,
            double scale,
            double opacity,
            int durationMs)
        {
            if (card == null)
            {
                return;
            }

            ScaleTransform transform = card.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform(1.0, 1.0);
                card.RenderTransform = transform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            QuadraticEase ease = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            };
            Duration duration = new Duration(TimeSpan.FromMilliseconds(durationMs));
            transform.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                new DoubleAnimation(scale, duration) { EasingFunction = ease });
            transform.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                new DoubleAnimation(scale, duration) { EasingFunction = ease });
            card.BeginAnimation(
                UIElement.OpacityProperty,
                new DoubleAnimation(opacity, duration) { EasingFunction = ease });
        }

        private void UpdateStationSelectionVisual()
        {
            for (int i = 0; i < _stationCards.Length; i++)
            {
                Border card = _stationCards[i];
                if (card == null)
                {
                    continue;
                }
                bool selected = i == _selectedStation - 1;
                if (StandaloneDesign)
                {
                    card.BorderThickness = selected ? new Thickness(2) : new Thickness(1);
                    card.BorderBrush = new SolidColorBrush(
                        selected
                            ? ChaoOrange
                            : ChaoLine);
                    card.Background = new SolidColorBrush(
                        selected
                            ? Color.FromRgb(255, 247, 238)
                            : ChaoSurface);
                    card.Effect = new DropShadowEffect
                    {
                        BlurRadius = selected ? 28 : 14,
                        ShadowDepth = selected ? 0 : 2,
                        Opacity = selected ? 0.34 : 0.08,
                        Color = selected ? ChaoOrange : ChaoInk
                    };
                    AnimateStandaloneStationCard(
                        card,
                        selected ? 1.035 : 0.965,
                        selected ? 1.0 : 0.80,
                        220);
                    continue;
                }
                card.BorderThickness = selected ? new Thickness(2) : new Thickness(0);
                card.BorderBrush = selected
                    ? Brushes.White
                    : Brushes.Transparent;
                card.Background = new SolidColorBrush(GetOriginalStationColor(i + 1));
            }
        }

        private void RefreshStationCards()
        {
            if (string.IsNullOrWhiteSpace(_recipePath) || !Directory.Exists(_recipePath))
            {
                return;
            }

            DirectoryInfo recipe = new DirectoryInfo(_recipePath);
            DirectoryInfo recipeContainer = recipe.Parent;
            DirectoryInfo machineRoot = recipeContainer != null ? recipeContainer.Parent : null;
            if (machineRoot == null)
            {
                return;
            }

            for (int station = 1; station <= 8; station++)
            {
                string stationIni = Path.Combine(
                    _recipePath,
                    "station" + station.ToString(CultureInfo.InvariantCulture) + ".ini");
                Dictionary<string, string> aiConfig = ReadIniSection(stationIni, "AIEnhanceTool");
                string persistedModelName =
                    GetDictionaryValue(aiConfig, "ModelName_A", "");
                string modelName = persistedModelName;
                string liveModelPath;
                if (TryGetLiveSelectedSeed(_recipePath, station, 0, out liveModelPath) &&
                    File.Exists(liveModelPath))
                {
                    modelName = Path.GetFileName(liveModelPath);
                }
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    modelName = "--";
                }
                if (_stationModelTexts[station - 1] != null)
                {
                    _stationModelTexts[station - 1].Text = modelName;
                    _stationModelTexts[station - 1].ToolTip = modelName;
                }
                if (StandaloneDesign && station == _selectedStation)
                {
                    if (_standaloneHeaderModelText != null)
                    {
                        _standaloneHeaderModelText.Text = "Model  " + modelName;
                        _standaloneHeaderModelText.ToolTip = modelName;
                    }
                    if (_standaloneHeaderStationText != null)
                    {
                        _standaloneHeaderStationText.Text = "Station  " + station.ToString(CultureInfo.InvariantCulture);
                    }
                    if (_standaloneHeaderRecipeText != null)
                    {
                        _standaloneHeaderRecipeText.Text = "Recipe  " + recipe.Name + "   ·   S" + station.ToString(CultureInfo.InvariantCulture);
                    }
                }

                string datasetRoot = ResolveDatasetRoot(
                    _projectRoot,
                    machineRoot.FullName,
                    recipe.Name,
                    persistedModelName,
                    station);
                int okTotal = CountImageFiles(Path.Combine(datasetRoot, "X", "OK"));
                int ngTotal = CountImageFiles(Path.Combine(datasetRoot, "X", "NG"));
                if (_stationOkTexts[station - 1] != null)
                {
                    _stationOkTexts[station - 1].Text =
                        okTotal.ToString(CultureInfo.InvariantCulture);
                }
                if (_stationNgTexts[station - 1] != null)
                {
                    _stationNgTexts[station - 1].Text =
                        ngTotal.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void SetSelectedStationRunVisual(string state)
        {
            int index = _selectedStation - 1;
            if (index < 0 || index >= _stationCards.Length || _stationCards[index] == null)
            {
                return;
            }
            _stationRunStates[index] = state ?? string.Empty;
            UpdateStationSelectionVisual();
        }

        private void SetSelectedStationResultText(string primary, string secondary)
        {
            int index = _selectedStation - 1;
            if (index < 0 || index >= _stationOkTexts.Length)
            {
                return;
            }
            _stationPrimaryOverrides[index] = primary ?? string.Empty;
            _stationSecondaryOverrides[index] = secondary ?? string.Empty;
            if (_resultCommandLog != null)
            {
                _resultCommandLog.AppendText(
                    "S" + _selectedStation + " " +
                    _stationPrimaryOverrides[index] + " | " +
                    _stationSecondaryOverrides[index] + Environment.NewLine);
                _resultCommandLog.ScrollToEnd();
            }
        }

        private void UpdateLossCurveAvailability()
        {
            if (_lossCurveButton == null)
            {
                return;
            }
            int index = _selectedStation - 1;
            int count =
                index >= 0 && index < _stationLossHistory.Length
                    ? _stationLossHistory[index].Count
                    : 0;
            _lossCurveButton.IsEnabled = count > 0;
            _lossCurveButton.ToolTip = count > 0
                ? "Station " + _selectedStation + "：" + count + " 個實際 Loss 點"
                : "目前 Station 尚無 Training Loss 資料。";
        }

        private void ShowLossCurve()
        {
            int index = _selectedStation - 1;
            if (index < 0 || index >= _stationLossHistory.Length)
            {
                return;
            }
            List<KeyValuePair<int, double>> points =
                _stationLossHistory[index].ToList();
            if (points.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "目前 Station 尚無可顯示的 Loss 資料。",
                    "Loss 曲線",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            Window dialog = new Window
            {
                Title = "Loss 曲線 - Station " + _selectedStation,
                Width = 780,
                Height = 500,
                MinWidth = 650,
                MinHeight = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(242, 242, 242))
            };

            Grid root = new Grid { Margin = new Thickness(14) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition());
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            double minLoss = points.Min(p => p.Value);
            double maxLoss = points.Max(p => p.Value);
            double latestLoss = points[points.Count - 1].Value;
            int latestProgress = points[points.Count - 1].Key;
            TextBlock header = new TextBlock
            {
                Text =
                    "Station " + _selectedStation +
                    "  Loss 點=" + points.Count +
                    "  最新=" + latestLoss.ToString("G8", CultureInfo.InvariantCulture) +
                    " @ " + latestProgress + "%",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            root.Children.Add(header);

            Canvas chart = new Canvas
            {
                Width = 720,
                Height = 360,
                Background = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(chart, 1);
            root.Children.Add(chart);

            const double left = 62;
            const double top = 22;
            const double right = 24;
            const double bottom = 48;
            double plotWidth = chart.Width - left - right;
            double plotHeight = chart.Height - top - bottom;

            chart.Children.Add(new Line
            {
                X1 = left,
                Y1 = top,
                X2 = left,
                Y2 = top + plotHeight,
                Stroke = Brushes.DimGray,
                StrokeThickness = 1
            });
            chart.Children.Add(new Line
            {
                X1 = left,
                Y1 = top + plotHeight,
                X2 = left + plotWidth,
                Y2 = top + plotHeight,
                Stroke = Brushes.DimGray,
                StrokeThickness = 1
            });

            double yMin = minLoss;
            double yMax = maxLoss;
            if (Math.Abs(yMax - yMin) < 1e-12)
            {
                double pad = Math.Max(Math.Abs(yMin) * 0.05, 1e-9);
                yMin -= pad;
                yMax += pad;
            }

            Polyline polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(35, 154, 217)),
                StrokeThickness = 2
            };
            foreach (KeyValuePair<int, double> point in points)
            {
                double x = left + (point.Key / 100.0) * plotWidth;
                double y = top + ((yMax - point.Value) / (yMax - yMin)) * plotHeight;
                polyline.Points.Add(new Point(x, y));

                Ellipse marker = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromRgb(35, 154, 217)),
                    ToolTip =
                        point.Key + "%  loss=" +
                        point.Value.ToString("G10", CultureInfo.InvariantCulture)
                };
                Canvas.SetLeft(marker, x - 3);
                Canvas.SetTop(marker, y - 3);
                chart.Children.Add(marker);
            }
            chart.Children.Add(polyline);

            AddCanvasText(
                chart,
                yMax.ToString("G6", CultureInfo.InvariantCulture),
                4,
                top - 8);
            AddCanvasText(
                chart,
                yMin.ToString("G6", CultureInfo.InvariantCulture),
                4,
                top + plotHeight - 8);
            AddCanvasText(chart, "0%", left - 10, top + plotHeight + 8);
            AddCanvasText(chart, "50%", left + plotWidth * 0.5 - 14, top + plotHeight + 8);
            AddCanvasText(chart, "100%", left + plotWidth - 22, top + plotHeight + 8);

            TextBlock note = new TextBlock
            {
                Text =
                    "資料來源：Training ProcDir/<percent>.txt 第一行 Loss；" +
                    "圖上沒有補值或平滑。",
                FontSize = 11,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(note, 2);
            root.Children.Add(note);

            dialog.Content = root;
            dialog.ShowDialog();
        }

        private static void AddCanvasText(
            Canvas canvas,
            string text,
            double left,
            double top)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = Brushes.DimGray
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }

        private void ShowImageParameterDialog()
        {
            Window dialog = new Window
            {
                Title = "影像參數設定",
                Width = 650,
                Height = 390,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(242, 242, 242))
            };

            Grid root = new Grid { Margin = new Thickness(14) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition());
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.Children.Add(new TextBlock
            {
                Text = "影像參數設定",
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            Grid form = new Grid
            {
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            for (int i = 0; i < 4; i++)
            {
                form.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = i % 2 == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star)
                });
            }

            TextBox width = MakeTextBox(_widthBox.Text);
            TextBox height = MakeTextBox(_heightBox.Text);
            TextBox batch = MakeTextBox(_batchBox.Text);
            TextBox aug = MakeTextBox(_augRatioBox.Text);
            TextBox loss = MakeTextBox(_lossBox.Text);
            TextBox lr = MakeTextBox(_learningRateBox.Text);
            TextBox unet = MakeTextBox(_unetDivBox.Text);
            TextBox maxSamples = MakeTextBox(_maxSamplesBox.Text);
            int row = 0;
            AddPairExisting(form, ref row, "AI模型寬", width, "AI模型高", height);
            AddPairExisting(form, ref row, "Batch Size", batch, "影像擴增倍率", aug);
            AddPairExisting(form, ref row, "LossFunction", loss, "Learning Rate", lr);
            AddPairExisting(form, ref row, "UnetDiv", unet, "驗證樣本上限", maxSamples);
            EnsureRow(form, row);
            TextBlock note = new TextBlock
            {
                Text = "手冊建議：影像擴增倍率設為 0；Batch Size 使用 8。",
                Foreground = Brushes.DimGray,
                Margin = new Thickness(10, 8, 10, 8)
            };
            Grid.SetRow(note, row);
            Grid.SetColumn(note, 0);
            Grid.SetColumnSpan(note, 4);
            form.Children.Add(note);
            Grid.SetRow(form, 1);
            root.Children.Add(form);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Button save = MakeOriginalButton("確定", 90, false);
            Button cancel = MakeOriginalButton("取消", 90, true);
            save.Click += delegate
            {
                _widthBox.Text = width.Text;
                _heightBox.Text = height.Text;
                _batchBox.Text = batch.Text;
                _augRatioBox.Text = aug.Text;
                _lossBox.Text = loss.Text;
                _learningRateBox.Text = lr.Text;
                _unetDivBox.Text = unet.Text;
                _maxSamplesBox.Text = maxSamples.Text;
                dialog.DialogResult = true;
            };
            cancel.Click += delegate { dialog.DialogResult = false; };
            buttons.Children.Add(save);
            buttons.Children.Add(cancel);
            Grid.SetRow(buttons, 2);
            root.Children.Add(buttons);
            dialog.Content = root;
            dialog.ShowDialog();
        }

        private static Button MakeButton(string text, double width)
        {
            return new Button
            {
                Content = text,
                Width = width,
                Height = 34,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(8, 2, 8, 2)
            };
        }

        private static void EnsureRow(Grid grid, int row)
        {
            while (grid.RowDefinitions.Count <= row)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
        }

        private static TextBlock MakeLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 5, 8, 5),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static TextBox MakeTextBox(string value)
        {
            return new TextBox
            {
                Text = value ?? string.Empty,
                Margin = new Thickness(0, 3, 16, 3),
                MinHeight = 27,
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private static void AddPair(
            Grid grid,
            ref int row,
            string label1,
            out TextBox box1,
            string default1,
            string label2,
            out TextBox box2,
            string default2)
        {
            EnsureRow(grid, row);
            TextBlock l1 = MakeLabel(label1);
            box1 = MakeTextBox(default1);
            TextBlock l2 = MakeLabel(label2);
            box2 = MakeTextBox(default2);

            Grid.SetRow(l1, row);
            Grid.SetColumn(l1, 0);
            Grid.SetRow(box1, row);
            Grid.SetColumn(box1, 1);
            Grid.SetRow(l2, row);
            Grid.SetColumn(l2, 2);
            Grid.SetRow(box2, row);
            Grid.SetColumn(box2, 3);

            grid.Children.Add(l1);
            grid.Children.Add(box1);
            grid.Children.Add(l2);
            grid.Children.Add(box2);
            row++;
        }

        private static void AddWide(
            Grid grid,
            ref int row,
            string label,
            out TextBox box)
        {
            EnsureRow(grid, row);
            TextBlock l = MakeLabel(label);
            box = MakeTextBox(string.Empty);
            Grid.SetRow(l, row);
            Grid.SetColumn(l, 0);
            Grid.SetRow(box, row);
            Grid.SetColumn(box, 1);
            Grid.SetColumnSpan(box, 3);
            grid.Children.Add(l);
            grid.Children.Add(box);
            row++;
        }

        private void ApplyTrainingPreset(int epochs)
        {
            _epochBox.Text = epochs.ToString(CultureInfo.InvariantCulture);
            _batchBox.Text = "8";
            _augRatioBox.Text = "0";
            _maxSamplesBox.Text = "0";
            AppendLog(
                "套用說明書建議 preset: Epoch=" +
                epochs +
                " Batch=8 AugRatio=0 full-dataset");
        }

        private void ApplyRecipeDefaults()
        {
            try
            {
                int station = ParseInt(_stationBox.Text, "Station", 1, 8);
                if (string.IsNullOrWhiteSpace(_recipePath) || !Directory.Exists(_recipePath))
                {
                    SetStatus("目前沒有有效 Recipe 路徑", false);
                    return;
                }

                // Genuine AITrainingWIndow.CopyValueToUI() reads these from
                // ThroughputFile.param.  The persisted values are stored in
                // Setting.ini [AITraining].  Recovered recipes without that
                // file fall back to the genuine XAML defaults: retest off and
                // Recent Image (index 0).
                Dictionary<string, string> trainingUiConfig = ReadIniSection(
                    Path.Combine(_recipePath, "Setting.ini"),
                    "AITraining");
                string bufferRetestText = GetDictionaryValue(
                    trainingUiConfig,
                    "EnableTrainingBufferTest",
                    null);
                bool persistedBufferRetest;
                if (_retestAfterTrainingCheck != null)
                {
                    _retestAfterTrainingCheck.IsChecked =
                        bool.TryParse(bufferRetestText, out persistedBufferRetest) &&
                        persistedBufferRetest;
                }
                int persistedBufferIndex = ParseOptionalNonNegativeInt(
                    GetDictionaryValue(
                        trainingUiConfig,
                        "TrainingBufferIndex",
                        null));
                if (_testImageSetCombo != null)
                {
                    _testImageSetCombo.SelectedIndex =
                        persistedBufferIndex >= 0 && persistedBufferIndex <= 6
                            ? persistedBufferIndex
                            : 0;
                }

                DirectoryInfo recipe = new DirectoryInfo(_recipePath);
                DirectoryInfo recipeContainer = recipe.Parent;
                DirectoryInfo machineRoot = recipeContainer != null ? recipeContainer.Parent : null;
                if (machineRoot == null)
                {
                    throw new InvalidOperationException("無法由 Recipe 推導機台根目錄。");
                }

                string stationIni = Path.Combine(
                    _recipePath,
                    "station" + station.ToString(CultureInfo.InvariantCulture) + ".ini");
                Dictionary<string, string> aiConfig =
                    ReadIniSection(stationIni, "AIEnhanceTool");
                string persistedModelName = GetDictionaryValue(
                    aiConfig,
                    "ModelName_A",
                    "<未設定>");
                string modelName = persistedModelName;
                string liveModelPath;
                bool usingLiveModel =
                    TryGetLiveSelectedSeed(_recipePath, station, 0, out liveModelPath) &&
                    File.Exists(liveModelPath);
                if (usingLiveModel)
                {
                    modelName = Path.GetFileName(liveModelPath);
                }
                // Selecting a different Training seed must not silently switch
                // the station's training image set. Dataset identity remains
                // anchored to the persisted Recipe/Station contract.
                string datasetRoot = ResolveDatasetRoot(
                    _projectRoot,
                    machineRoot.FullName,
                    recipe.Name,
                    persistedModelName,
                    station);
                _xPathBox.Text = Path.Combine(datasetRoot, "X");
                _yPathBox.Text = Path.Combine(datasetRoot, "Y");

                string outputRoot = Path.Combine(
                    ProductRuntimePaths.ResolveProductRoot(_projectRoot),
                    "Runtime",
                    "TrainingRuns",
                    recipe.Name + "-S" + station.ToString(CultureInfo.InvariantCulture));
                _outputRootBox.Text = outputRoot;
                AppendLog("Recipe=" + _recipePath);
                AppendLog("Dataset=" + datasetRoot);
                AppendLog("Output=" + outputRoot);
                int configuredWidth = ParseOptionalPositiveInt(
                    GetDictionaryValue(aiConfig, "Width_A", null));
                int configuredHeight = ParseOptionalPositiveInt(
                    GetDictionaryValue(aiConfig, "Height_A", null));
                if (configuredWidth > 0)
                {
                    _widthBox.Text = configuredWidth.ToString(CultureInfo.InvariantCulture);
                }
                if (configuredHeight > 0)
                {
                    _heightBox.Text = configuredHeight.ToString(CultureInfo.InvariantCulture);
                }

                int trainingCount = ParseOptionalPositiveInt(
                    GetDictionaryValue(aiConfig, "TrainingCount", null));
                ApplyOriginalTrainingWorkspaceDefaults(recipe.Name, station, modelName);

                int xOkCount = CountImageFiles(Path.Combine(datasetRoot, "X", "OK"));
                int xNgCount = CountImageFiles(Path.Combine(datasetRoot, "X", "NG"));
                string originalStationRoot = Path.Combine(
                    datasetRoot,
                    "Origional",
                    station.ToString(CultureInfo.InvariantCulture));
                int originalOkCount = CountImageFiles(Path.Combine(originalStationRoot, "OK"));
                int originalNgCount = CountImageFiles(Path.Combine(originalStationRoot, "NG"));
                string configuredModelPath = usingLiveModel
                    ? Path.GetFullPath(liveModelPath)
                    : ResolveModelPath(
                        machineRoot.FullName,
                        modelName);
                bool configuredModelExists =
                    !string.IsNullOrWhiteSpace(configuredModelPath) &&
                    File.Exists(configuredModelPath);

                _stationInfoText.Text =
                    "S" + station +
                    "  模型=" + modelName +
                    (usingLiveModel ? " [目前選取]" : " [Recipe]") +
                    (modelName != "<未設定>"
                        ? (configuredModelExists
                            ? " [可讀取]"
                            : " [找不到模型]")
                        : string.Empty) +
                    "  尺寸=" + _widthBox.Text + "x" + _heightBox.Text +
                    "  |  訓練集 X：OK " + xOkCount +
                    " / NG " + xNgCount +
                    " / Total " + (xOkCount + xNgCount) +
                    (trainingCount > 0
                        ? "  |  Recipe TrainingCount=" + trainingCount
                        : string.Empty) +
                    "  |  原圖：OK " + originalOkCount +
                    " / NG " + originalNgCount;
                AppendLog(_stationInfoText.Text);

                bool xOk = Directory.Exists(_xPathBox.Text);
                bool yOk = Directory.Exists(_yPathBox.Text);
                SetStatus(
                    xOk && yOk
                        ? "Recipe/Station 路徑已套用"
                        : "已套用，但 X/Y 資料夾尚不存在",
                    xOk && yOk);
                RefreshStationCards();
                UpdateStationSelectionVisual();
            }
            catch (Exception exception)
            {
                HandleFailure("套用 Recipe 失敗", exception);
            }
        }

        private void ApplyOriginalTrainingWorkspaceDefaults(
            string recipeName,
            int station,
            string configuredModelName)
        {
            string stationFileName =
                recipeName + "-" + station.ToString(CultureInfo.InvariantCulture) + ".ini";
            string globalWorkspace = Path.Combine(
                _projectRoot,
                "Training",
                "Model",
                "navs.ini");

            string liveSeedPath;
            bool liveSeedIsEmptyPlaceholder =
                TryGetLiveSelectedSeed(_recipePath, station, 0, out liveSeedPath) &&
                IsObservedEmptyTrainingPlaceholder(liveSeedPath);

            List<string> candidates = new List<string>
            {
                Path.Combine(
                    _projectRoot,
                    "Training",
                    "Train-Recipe",
                    recipeName,
                    stationFileName),
                Path.Combine(
                    _projectRoot,
                    "Train-Recipe",
                    recipeName,
                    stationFileName)
            };

            // TF-7 is the preserved machine donor used by this reconstruction.
            // It is a read-only evidence source; product operation does not write it.
            string goldenTf7 = Path.Combine(
                @"E:\Ai-Report\davs3\TF-7\Train-Recipe",
                recipeName,
                stationFileName);
            candidates.Add(goldenTf7);

            // ModelsWindow's newly-created tiny .h5d is only a placeholder.
            // Genuine StartTrainDnet does not inherit weights/config from that
            // file; it falls through load_model failure into CreateUnetModel
            // using the current Training workspace parameters.  Therefore a
            // fresh live placeholder must prefer the current global navs.ini
            // over preserved per-station Train-Recipe history, which may be
            // stale relative to the model currently in use.
            if (liveSeedIsEmptyPlaceholder && File.Exists(globalWorkspace))
            {
                candidates.Remove(globalWorkspace);
                candidates.Insert(0, globalWorkspace);
                AppendLog(
                    "Fresh model placeholder detected; prefer current Training workspace=" +
                    globalWorkspace +
                    " liveSeed=" + liveSeedPath);
            }
            else
            {
                // Last-resort compatibility for non-placeholder selection.
                candidates.Add(globalWorkspace);
            }

            string configuredStem = string.IsNullOrWhiteSpace(configuredModelName)
                ? string.Empty
                : Path.GetFileNameWithoutExtension(configuredModelName.Trim());
            string navsIni = null;
            foreach (string candidate in candidates)
            {
                if (!File.Exists(candidate))
                {
                    continue;
                }

                Dictionary<string, string> candidateValues =
                    ReadIniSection(candidate, "DAVS_V3");
                string candidateModel =
                    GetDictionaryValue(candidateValues, "SegModelName", string.Empty);
                string candidateStem = string.IsNullOrWhiteSpace(candidateModel)
                    ? string.Empty
                    : Path.GetFileNameWithoutExtension(candidateModel.Trim());

                bool isGlobalWorkspace = string.Equals(
                    candidate,
                    globalWorkspace,
                    StringComparison.OrdinalIgnoreCase);
                if (isGlobalWorkspace ||
                    string.IsNullOrWhiteSpace(configuredStem) ||
                    string.Equals(
                        candidateStem,
                        configuredStem,
                        StringComparison.OrdinalIgnoreCase))
                {
                    navsIni = candidate;
                    break;
                }

                AppendLog(
                    "Skip stale Training parameter source=" + candidate +
                    " SegModelName=" + candidateModel +
                    " currentModel=" + configuredModelName);
            }
            if (!File.Exists(navsIni))
            {
                return;
            }

            Dictionary<string, string> values =
                ReadIniSection(navsIni, "DAVS_V3");
            int epoch = ParseOptionalPositiveInt(
                GetDictionaryValue(values, "Epoch", null));
            int batch = ParseOptionalPositiveInt(
                GetDictionaryValue(values, "BatchSize", null));
            int loss = ParseOptionalPositiveInt(
                GetDictionaryValue(values, "LossFunction", null));
            int unetDiv = ParseOptionalPositiveInt(
                GetDictionaryValue(values, "UnetDiv", null));
            int augRatio = ParseOptionalNonNegativeInt(
                GetDictionaryValue(values, "AugRatio", null));
            double learningRate = ParseOptionalPositiveDouble(
                GetDictionaryValue(values, "LR", null));
            double maybeArea = ParseOptionalNonNegativeDouble(
                GetDictionaryValue(values, "MaybeArea", null));
            double threshold = ParseOptionalPositiveDouble(
                GetDictionaryValue(values, "Threshold", null));
            string modelType = GetDictionaryValue(values, "ModelType", "US");

            if (epoch > 0) _epochBox.Text = epoch.ToString(CultureInfo.InvariantCulture);
            if (batch > 0) _batchBox.Text = batch.ToString(CultureInfo.InvariantCulture);
            if (loss > 0) _lossBox.Text = loss.ToString(CultureInfo.InvariantCulture);
            if (unetDiv > 0) _unetDivBox.Text = unetDiv.ToString(CultureInfo.InvariantCulture);
            if (augRatio >= 0) _augRatioBox.Text = augRatio.ToString(CultureInfo.InvariantCulture);
            if (learningRate > 0)
            {
                _learningRateBox.Text = learningRate.ToString(CultureInfo.InvariantCulture);
            }
            if (maybeArea >= 0) _workspaceMaybeArea = maybeArea;
            if (threshold > 0) _workspaceThreshold = threshold;
            if (modelType == "STD" ||
                modelType == "US" ||
                modelType == "US-" ||
                modelType == "US--")
            {
                _workspaceModelType = modelType;
            }

            AppendLog(
                "Training parameter source=" + navsIni +
                " ModelType=" + _workspaceModelType +
                " Epoch=" + _epochBox.Text +
                " Loss=" + _lossBox.Text +
                " Batch=" + _batchBox.Text +
                " UnetDiv=" + _unetDivBox.Text +
                " MaybeArea=" + _workspaceMaybeArea.ToString(CultureInfo.InvariantCulture) +
                " Threshold=" + _workspaceThreshold.ToString(CultureInfo.InvariantCulture));
        }

        private static bool IsObservedEmptyTrainingPlaceholder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            FileInfo info = new FileInfo(path);
            if (info.Length != 2)
            {
                return false;
            }

            byte[] bytes = File.ReadAllBytes(path);
            return bytes.Length == 2 && bytes[0] == 0x0D && bytes[1] == 0x0A;
        }

        private static int ParseOptionalNonNegativeInt(string text)
        {
            int value;
            return int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value) && value >= 0
                ? value
                : -1;
        }

        private static double ParseOptionalPositiveDouble(string text)
        {
            double value;
            return double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value) && value > 0
                ? value
                : 0.0;
        }

        private static double ParseOptionalNonNegativeDouble(string text)
        {
            double value;
            return double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value) && value >= 0
                ? value
                : -1.0;
        }

        private static string ResolveDatasetRoot(
            string projectRoot,
            string machineRoot,
            string recipeName,
            string modelName,
            int station)
        {
            string stationText = station.ToString(CultureInfo.InvariantCulture);
            string modelStem = string.IsNullOrWhiteSpace(modelName) || modelName == "<未設定>"
                ? string.Empty
                : Path.GetFileNameWithoutExtension(modelName);
            string templateTrainingRoot = Path.Combine(projectRoot, "TrainingImage");
            string machineTrainingRoot = Path.Combine(machineRoot, "TrainingImage");
            List<string> candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(modelStem))
            {
                candidates.Add(Path.Combine(templateTrainingRoot, modelStem));
                candidates.Add(Path.Combine(machineTrainingRoot, modelStem));
            }
            candidates.Add(Path.Combine(templateTrainingRoot, recipeName + "-" + stationText));
            candidates.Add(Path.Combine(machineTrainingRoot, recipeName + "-" + stationText));

            if (recipeName.EndsWith("-TF", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(
                    Path.Combine(
                        templateTrainingRoot,
                        recipeName.Substring(0, recipeName.Length - 3) + "-" + stationText));
                candidates.Add(
                    Path.Combine(
                        machineTrainingRoot,
                        recipeName.Substring(0, recipeName.Length - 3) + "-" + stationText));
            }

            string found = candidates.FirstOrDefault(Directory.Exists);
            return found ?? candidates[0];
        }

        private string ResolveModelPath(string machineRoot, string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName) || modelName == "<未設定>")
            {
                return string.Empty;
            }

            string fileName = Path.GetFileName(modelName);
            string[] candidates =
            {
                Path.Combine(
                    ProductRuntimePaths.ResolveProductRoot(_projectRoot),
                    "Model",
                    fileName),
                Path.Combine(machineRoot, "Model", fileName),
                Path.Combine(_recipePath, fileName)
            };

            string found = candidates.FirstOrDefault(File.Exists);
            return found ?? candidates[0];
        }

        private static Dictionary<string, string> ReadIniSection(
            string path,
            string wantedSection)
        {
            Dictionary<string, string> result =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
            {
                return result;
            }

            string section = string.Empty;
            using (StreamReader reader = new StreamReader(path, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("[", StringComparison.Ordinal) &&
                        trimmed.EndsWith("]", StringComparison.Ordinal))
                    {
                        section = trimmed.Substring(1, trimmed.Length - 2);
                        continue;
                    }
                    if (!string.Equals(
                        section,
                        wantedSection,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    int equals = trimmed.IndexOf('=');
                    if (equals <= 0)
                    {
                        continue;
                    }
                    string key = trimmed.Substring(0, equals).Trim();
                    string value = trimmed.Substring(equals + 1).Trim();
                    result[key] = value;
                }
            }

            return result;
        }

        private static string GetDictionaryValue(
            IDictionary<string, string> values,
            string key,
            string fallback)
        {
            string value;
            return values != null && values.TryGetValue(key, out value)
                ? value
                : fallback;
        }

        private static int ParseOptionalPositiveInt(string text)
        {
            int value;
            return int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value) && value > 0
                    ? value
                    : 0;
        }

        private static int CountImageFiles(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return 0;
            }

            HashSet<string> extensions = new HashSet<string>(
                new[] { ".bmp", ".png", ".jpg", ".jpeg", ".tif", ".tiff" },
                StringComparer.OrdinalIgnoreCase);
            return Directory.GetFiles(
                    directory,
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Count(path => extensions.Contains(Path.GetExtension(path)));
        }

        private void AddSelectedStationsToStandaloneQueue()
        {
            if (!TrainingQueueFeature)
            {
                return;
            }
            try
            {
                List<int> stations = GetCheckedTrainingStations();
                if (stations.Count == 0)
                {
                    throw new InvalidOperationException("請先勾選至少一個站別再加入排程。");
                }

                foreach (int station in stations)
                {
                    string seed = ResolveConfiguredTrainingSeed(station);
                    if (!File.Exists(seed))
                    {
                        throw new FileNotFoundException("排程模型不存在。", seed);
                    }
                    int epoch = 50;
                    TextBox epochBox = _stationEpochBoxes[station - 1];
                    if (epochBox != null)
                    {
                        int parsed;
                        if (int.TryParse(epochBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && parsed > 0)
                        {
                            epoch = parsed;
                        }
                    }
                    string fullSeed = Path.GetFullPath(seed);
                    string sha = ComputeSha256(fullSeed);
                    StandaloneQueueItem existing = _standaloneQueue.FirstOrDefault(item => item.Station == station);
                    if (existing == null)
                    {
                        _standaloneQueue.Add(new StandaloneQueueItem
                        {
                            Station = station,
                            Epoch = epoch,
                            SeedPath = fullSeed,
                            SeedSha256 = sha,
                            Status = "QUEUED"
                        });
                    }
                    else if (!_standaloneQueueRunning)
                    {
                        existing.Epoch = epoch;
                        existing.SeedPath = fullSeed;
                        existing.SeedSha256 = sha;
                        existing.Status = "QUEUED";
                    }
                    AppendLog(
                        "QUEUE SNAPSHOT S" + station +
                        " model=" + fullSeed +
                        " sha256=" + sha +
                        " epoch=" + epoch);
                }
                RefreshStandaloneQueueList();
                SetStatus("已加入 Training Queue，共 " + _standaloneQueue.Count + " 站", true);
            }
            catch (Exception exception)
            {
                HandleFailure("加入 Training Queue 失敗", exception);
            }
        }

        private void RefreshStandaloneQueueList()
        {
            if (!TrainingQueueFeature || _standaloneQueueList == null)
            {
                return;
            }
            _standaloneQueueList.Items.Clear();
            foreach (StandaloneQueueItem item in _standaloneQueue)
            {
                _standaloneQueueList.Items.Add(item.ToString());
            }
            if (_standaloneQueueSummaryText != null)
            {
                int queued = _standaloneQueue.Count(item =>
                    string.Equals(item.Status, "QUEUED", StringComparison.OrdinalIgnoreCase));
                int running = _standaloneQueue.Count(item =>
                    string.Equals(item.Status, "RUNNING", StringComparison.OrdinalIgnoreCase));
                int done = _standaloneQueue.Count(item =>
                    string.Equals(item.Status, "DONE", StringComparison.OrdinalIgnoreCase));
                _standaloneQueueSummaryText.Text =
                    running > 0
                        ? running + " RUN · " + queued + " WAIT · " + done + " DONE"
                        : queued + " QUEUED · " + done + " DONE";
            }
            if (_standaloneClearQueueButton != null)
            {
                _standaloneClearQueueButton.IsEnabled =
                    !_standaloneQueueRunning && _standaloneQueue.Count > 0;
            }
        }

        private void ResetStandaloneQueue()
        {
            if (!TrainingQueueFeature || _standaloneQueueRunning)
            {
                return;
            }
            _standaloneQueue.Clear();
            _standaloneCurrentQueueItem = null;
            RefreshStandaloneQueueList();
            SetStatus("Training Queue 已清空", true);
        }

        private void StartStandaloneQueue()
        {
            if (!TrainingQueueFeature)
            {
                return;
            }
            if (_standaloneQueueRunning)
            {
                return;
            }
            if (_standaloneQueue.Count == 0)
            {
                AddSelectedStationsToStandaloneQueue();
                if (_standaloneQueue.Count == 0)
                {
                    return;
                }
            }
            _standaloneQueueRunning = true;
            if (_standaloneAddQueueButton != null) _standaloneAddQueueButton.IsEnabled = false;
            if (_standaloneStartQueueButton != null) _standaloneStartQueueButton.IsEnabled = false;
            StartNextStandaloneQueueItem();
        }

        private void StartNextStandaloneQueueItem()
        {
            if (!TrainingQueueFeature || !_standaloneQueueRunning)
            {
                return;
            }
            StandaloneQueueItem next = _standaloneQueue.FirstOrDefault(item => string.Equals(item.Status, "QUEUED", StringComparison.OrdinalIgnoreCase));
            if (next == null)
            {
                _standaloneQueueRunning = false;
                _standaloneCurrentQueueItem = null;
                if (_standaloneAddQueueButton != null) _standaloneAddQueueButton.IsEnabled = true;
                if (_standaloneStartQueueButton != null) _standaloneStartQueueButton.IsEnabled = true;
                RefreshStandaloneQueueList();
                SetStatus("Training Queue 全部完成", true);
                return;
            }

            try
            {
                if (!File.Exists(next.SeedPath))
                {
                    throw new FileNotFoundException("Queue seed 已不存在。", next.SeedPath);
                }
                string actualSha = ComputeSha256(next.SeedPath);
                if (!string.Equals(actualSha, next.SeedSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "IDENTITY MISMATCH：S" + next.Station + " seed SHA256 已改變。\n" +
                        "Queue=" + next.SeedSha256 + "\nActual=" + actualSha);
                }

                for (int i = 0; i < _stationTrainChecks.Length; i++)
                {
                    if (_stationTrainChecks[i] != null)
                    {
                        _stationTrainChecks[i].IsChecked = i == next.Station - 1;
                    }
                }
                SetLiveSelectedSeed(_recipePath, next.Station, 0, next.SeedPath);
                if (_selectedStation != next.Station)
                {
                    SelectStation(next.Station);
                }
                if (_stationEpochBoxes[next.Station - 1] != null)
                {
                    _stationEpochBoxes[next.Station - 1].Text = next.Epoch.ToString(CultureInfo.InvariantCulture);
                    _epochBox = _stationEpochBoxes[next.Station - 1];
                }
                next.Status = "RUNNING";
                _standaloneCurrentQueueItem = next;
                RefreshStandaloneQueueList();
                AppendLog(
                    "QUEUE START S" + next.Station +
                    " model=" + next.SeedPath +
                    " sha256=" + next.SeedSha256 +
                    " epoch=" + next.Epoch);
                StartTraining();
            }
            catch (Exception exception)
            {
                next.Status = "BLOCKED";
                _standaloneQueueRunning = false;
                if (_standaloneAddQueueButton != null) _standaloneAddQueueButton.IsEnabled = true;
                if (_standaloneStartQueueButton != null) _standaloneStartQueueButton.IsEnabled = true;
                RefreshStandaloneQueueList();
                HandleFailure("Training Queue identity 驗證失敗", exception);
            }
        }

        private void FinishStandaloneQueueItem(bool success, string status)
        {
            if (!TrainingQueueFeature || _standaloneCurrentQueueItem == null)
            {
                return;
            }
            _standaloneCurrentQueueItem.Status = success ? "DONE" : (string.IsNullOrWhiteSpace(status) ? "FAILED" : status);
            RefreshStandaloneQueueList();
            if (!success)
            {
                _standaloneQueueRunning = false;
                if (_standaloneAddQueueButton != null) _standaloneAddQueueButton.IsEnabled = true;
                if (_standaloneStartQueueButton != null) _standaloneStartQueueButton.IsEnabled = true;
                return;
            }
            _standaloneCurrentQueueItem = null;
            Dispatcher.BeginInvoke(new Action(StartNextStandaloneQueueItem));
        }

        private void StartTraining()
        {
            if (_activeProcess != null && !_activeProcess.HasExited)
            {
                MessageBox.Show(this, "Training 已在執行中。", "Training");
                return;
            }
            if (_postTrainingFinalizeInProgress)
            {
                MessageBox.Show(
                    this,
                    "上一輪 Training 正在背景完成 PB / 部署檢查，請等收尾完成後再開始下一輪。",
                    "Training 收尾中");
                return;
            }

            try
            {
                List<int> selectedForTraining = GetCheckedTrainingStations();
                if (selectedForTraining.Count == 0)
                {
                    throw new InvalidOperationException("請先勾選至少一個「本次訓練站別」。");
                }
                if (selectedForTraining.Count > 1)
                {
                    throw new InvalidOperationException(
                        "目前已完成原廠多站選取 UI，但多站 sequential/remote orchestration 尚未通過 SYS-01。\n" +
                        "現階段請先只勾選一站執行本機訓練，避免假裝支援尚未驗證的多站流程。");
                }
                int trainStation = selectedForTraining[0];
                if (trainStation != _selectedStation)
                {
                    SelectStation(trainStation);
                }
                ShowResultTrainingLayer();
                // Chao8Side: runtime paths are resolved from the product root.

                string seedModelPath = ResolveConfiguredTrainingSeed(trainStation);
                TrainingRun run = BuildRun();
                WriteIni(run);

                string python = ProductRuntimePaths.GetAIRuntimeControl(_projectRoot);
                if (!File.Exists(python))
                {
                    throw new FileNotFoundException("Unified AI Runtime control layer missing.", python);
                }

                Directory.CreateDirectory(run.OutputRoot);
                Directory.CreateDirectory(run.ProcDir);
                foreach (string file in Directory.GetFiles(run.ProcDir, "*.txt"))
                {
                    File.Delete(file);
                }

                StringBuilder arguments = new StringBuilder();
                arguments.Append("inspection --root ").Append(Quote(ProductRuntimePaths.ResolveProductRoot(_projectRoot)))
                    .Append(" --slot local --workdir ").Append(Quote(run.OutputRoot))
                    .Append(" --gpu ").Append(run.Gpu).Append(" --");
                arguments.Append(" -s 0 -g ").Append(run.Gpu);
                arguments.Append(" -i ").Append(Quote(run.IniPath));
                arguments.Append(" -m ").Append(Quote(run.ModelPath));
                arguments.Append(" -v DAVS_V3 -r Train");
                arguments.Append(" -e ").Append(run.Epoch);
                arguments.Append(" -b ").Append(run.BatchSize);
                arguments.Append(" -l ").Append(run.LossFunction);
                arguments.Append(" -a ").Append(run.AugRatio);
                arguments.Append(" -d ").Append(run.UnetDiv);
                arguments.Append(" --ImgWidth ").Append(run.Width);
                arguments.Append(" --ImgHeight ").Append(run.Height);
                arguments.Append(" -lr ").Append(run.LearningRate.ToString(CultureInfo.InvariantCulture));
                arguments.Append(" --MaybeArea ").Append(
                    run.MaybeArea.ToString(CultureInfo.InvariantCulture));
                arguments.Append(" --ProcDir ").Append(Quote(run.ProcDir));
                arguments.Append(" --TempName temp");
                arguments.Append(" -t ").Append(run.ModelType);
                arguments.Append(" --compat-seed-model ").Append(Quote(seedModelPath));
                if (run.MaxSamples > 0)
                {
                    arguments.Append(" --compat-max-samples ").Append(run.MaxSamples);
                }

                _activeProcDir = run.ProcDir;
                _activeModelPath = run.ModelPath;
                _activeIniPath = run.IniPath;
                _activeRun = run;
                _stopRequested = false;
                _stopEscalated = false;
                _stopRequestedAt = default(DateTime);
                _trainingStartedAt = DateTime.Now;
                _lastProgressAt = _trainingStartedAt;
                _lastProgressPercent = 0;
                _trainingStdoutLineCount = 0;
                _trainingStderrLineCount = 0;
                _stationLossHistory[_selectedStation - 1].Clear();
                _progressBar.Value = 0;
                _startButton.IsEnabled = false;
                _stopButton.IsEnabled = true;
                _terminateButton.IsEnabled = true;
                _convertButton.IsEnabled = false;
                _previewButton.IsEnabled = false;
                UpdateLossCurveAvailability();
                _timeText.Text = "訓練時間 00:00:00 / 剩餘 --";
                SetStatus("Training 執行中", true);
                SetSelectedStationRunVisual("running");
                _lastLossText = string.Empty;
                SetSelectedStationResultText("訓練進度      0%", "Loss            --");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = arguments.ToString(),
                    WorkingDirectory = run.OutputRoot,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(startInfo, _projectRoot, run.Gpu);

                Process process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };
                process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        QueueTrainingChildOutput(e.Data, false);
                    }
                };
                process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        QueueTrainingChildOutput(e.Data, true);
                    }
                };
                process.Exited += delegate
                {
                    DateTime childExitObservedAt = DateTime.Now;
                    int childPid = process.Id;
                    try
                    {
                        // Exited may fire before asynchronous stdout/stderr
                        // readers have consumed EOF.  This callback is not on
                        // the WPF Dispatcher thread, so waiting here cannot
                        // freeze the UI.
                        process.WaitForExit();
                    }
                    catch
                    {
                    }
                    int exitCode = process.ExitCode;
                    double cpuSeconds = 0.0;
                    try
                    {
                        cpuSeconds = process.TotalProcessorTime.TotalSeconds;
                    }
                    catch
                    {
                    }
                    _trace(
                        "RebuiltTraining: child EXIT observed PID=" + childPid +
                        " exit=" + exitCode +
                        " cpuSec=" + cpuSeconds.ToString("F3", CultureInfo.InvariantCulture) +
                        " stdoutLines=" + _trainingStdoutLineCount +
                        " stderrLines=" + _trainingStderrLineCount +
                        " at=" + childExitObservedAt.ToString("HH:mm:ss.fff"));
                    Dispatcher.BeginInvoke(
                        new Action(() => OnTrainingExited(
                            exitCode,
                            childPid,
                            childExitObservedAt,
                            cpuSeconds)),
                        DispatcherPriority.Send);
                };

                _activeProcess = process;
                AppendLog("START " + python + " " + arguments);
                long seedBytes = new FileInfo(seedModelPath).Length;
                string seedSha256 = ComputeSha256(seedModelPath);
                AppendLog(
                    "SEED MODEL=" + seedModelPath +
                    " bytes=" + seedBytes +
                    " sha256=" + seedSha256);
                _trace(
                    "RebuiltTraining: seed=" + seedModelPath +
                    " bytes=" + seedBytes +
                    " sha256=" + seedSha256);
                _trace("RebuiltTraining: start " + arguments);
                process.Start();
                _trace(
                    "RebuiltTraining: child PID=" + process.Id +
                    " exe=" + python +
                    " seed=" + seedModelPath +
                    " seedSha256=" + seedSha256 +
                    " args=" + arguments);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception exception)
            {
                _startButton.IsEnabled = true;
                _stopButton.IsEnabled = false;
                _terminateButton.IsEnabled = false;
                _convertButton.IsEnabled = true;
                _previewButton.IsEnabled = true;
                SetSelectedStationRunVisual("failed");
                HandleFailure("Training 啟動失敗", exception);
            }
        }

        private void QueueTrainingChildOutput(string line, bool isError)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            int seen = isError
                ? System.Threading.Interlocked.Increment(ref _trainingStderrLineCount)
                : System.Threading.Interlocked.Increment(ref _trainingStdoutLineCount);
            bool important =
                isError ||
                line.StartsWith("DIA_PROGRESS ", StringComparison.Ordinal) ||
                line.StartsWith("DIA_RESULT ", StringComparison.Ordinal) ||
                line.StartsWith("Epoch ", StringComparison.OrdinalIgnoreCase);

            if (!important && seen > MaxTrainingStdoutUiLines)
            {
                if (!isError && seen == MaxTrainingStdoutUiLines + 1)
                {
                    Dispatcher.BeginInvoke(
                        new Action(() => AppendLog(
                            "[trainer stdout 已節流；完整結果請查看 training_result.json]")),
                        DispatcherPriority.Background);
                }
                return;
            }

            string uiLine = isError ? "ERR> " + line : line;
            Dispatcher.BeginInvoke(
                new Action(() => AppendLog(uiLine)),
                DispatcherPriority.Background);
        }

        private string ResolveConfiguredTrainingSeed(int station)
        {
            if (station < 1 || station > 8)
            {
                throw new ArgumentOutOfRangeException("station");
            }
            if (string.IsNullOrWhiteSpace(_recipePath) || !Directory.Exists(_recipePath))
            {
                throw new DirectoryNotFoundException("目前沒有有效 Recipe 路徑。");
            }

            DirectoryInfo recipe = new DirectoryInfo(_recipePath);
            DirectoryInfo recipeContainer = recipe.Parent;
            DirectoryInfo machineRoot = recipeContainer != null
                ? recipeContainer.Parent
                : null;
            if (machineRoot == null)
            {
                throw new InvalidOperationException("無法由 Recipe 推導機台根目錄。");
            }

            string liveSeed;
            if (TryGetLiveSelectedSeed(_recipePath, station, 0, out liveSeed) &&
                File.Exists(liveSeed))
            {
                AppendLog(
                    "TRAIN SEED live-selection S" + station +
                    " slot=A = " + liveSeed +
                    " bytes=" + new FileInfo(liveSeed).Length +
                    " sha256=" + ComputeSha256(liveSeed));
                return Path.GetFullPath(liveSeed);
            }

            string stationIni = Path.Combine(
                _recipePath,
                "station" + station.ToString(CultureInfo.InvariantCulture) + ".ini");
            Dictionary<string, string> aiConfig =
                ReadIniSection(stationIni, "AIEnhanceTool");
            string modelName = GetDictionaryValue(aiConfig, "ModelName_A", "");
            if (string.IsNullOrWhiteSpace(modelName))
            {
                throw new InvalidOperationException(
                    "S" + station + " Recipe 未設定 ModelName_A，禁止用未知模型開始訓練。");
            }

            string modelPath = ResolveModelPath(machineRoot.FullName, modelName);
            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                throw new FileNotFoundException(
                    "S" + station + " 找不到 Recipe 指定的 Training seed 模型。",
                    modelPath);
            }
            AppendLog(
                "TRAIN SEED recipe-fallback S" + station +
                " = " + modelPath +
                " bytes=" + new FileInfo(modelPath).Length +
                " sha256=" + ComputeSha256(modelPath));
            return Path.GetFullPath(modelPath);
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
            }
        }

        private List<int> GetCheckedTrainingStations()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < _stationTrainChecks.Length; i++)
            {
                CheckBox check = _stationTrainChecks[i];
                if (check != null && check.IsChecked == true)
                {
                    result.Add(i + 1);
                }
            }
            return result;
        }

        private void RequestStop()
        {
            try
            {
                Process process = _activeProcess;
                if (process == null || process.HasExited)
                {
                    _startButton.IsEnabled = true;
                    _stopButton.IsEnabled = false;
                    _terminateButton.IsEnabled = false;
                    SetStatus("目前沒有執行中的 Training", false);
                    return;
                }
                if (_stopRequested)
                {
                    SetStatus("停止要求已送出，等待 trainer 結束", false);
                    return;
                }
                if (string.IsNullOrWhiteSpace(_activeProcDir))
                {
                    throw new InvalidOperationException("目前 Training 沒有有效 ProcDir。");
                }
                Directory.CreateDirectory(_activeProcDir);
                string interrupt = Path.Combine(_activeProcDir, "interrupt.txt");
                File.WriteAllText(interrupt, string.Empty, Encoding.UTF8);
                _stopRequested = true;
                _stopEscalated = false;
                _stopRequestedAt = DateTime.Now;
                _stopButton.IsEnabled = false;
                _terminateButton.IsEnabled = true;
                AppendLog(
                    "STOP requested PID=" + process.Id +
                    " interrupt=" + interrupt);
                _trace(
                    "RebuiltTraining: stop requested PID=" + process.Id +
                    " interrupt=" + interrupt);
                SetStatus(
                    "已要求停止；等待目前 batch 結束，30 秒未退出將自動終止",
                    false);
            }
            catch (Exception exception)
            {
                HandleFailure("停止要求失敗", exception);
            }
        }

        private void CheckTrainingStopTimeout()
        {
            if (!_stopRequested ||
                _stopEscalated ||
                _stopRequestedAt == default(DateTime))
            {
                return;
            }

            Process process = _activeProcess;
            if (process == null || process.HasExited)
            {
                return;
            }
            if ((DateTime.Now - _stopRequestedAt).TotalSeconds < SoftStopTimeoutSeconds)
            {
                return;
            }

            try
            {
                _stopEscalated = true;
                AppendLog(
                    "STOP timeout after " + SoftStopTimeoutSeconds +
                    "s; force terminate PID=" + process.Id);
                _trace(
                    "RebuiltTraining: stop timeout force terminate PID=" + process.Id);
                process.Kill();
                SetStatus("停止逾時，已強制終止 Training", false);
            }
            catch (Exception exception)
            {
                _stopEscalated = true;
                _startButton.IsEnabled = true;
                _stopButton.IsEnabled = false;
                _terminateButton.IsEnabled = true;
                HandleFailure("停止逾時後終止 Training 失敗", exception);
            }
        }

        private void TerminateActiveOperation()
        {
            try
            {
                if (_previewProcess != null && !_previewProcess.HasExited)
                {
                    Process preview = _previewProcess;
                    AppendLog("PREVIEW TERMINATE requested PID=" + preview.Id);
                    _trace("RebuiltTraining: preview terminate PID=" + preview.Id);
                    preview.Kill();
                    SetStatus("預覽回測停止中", false);
                    return;
                }
            }
            catch (Exception exception)
            {
                HandleFailure("停止預覽回測失敗", exception);
                return;
            }

            TerminateTraining();
        }

        private void TerminateTraining()
        {
            try
            {
                Process process = _activeProcess;
                if (process == null || process.HasExited)
                {
                    return;
                }

                MessageBoxResult confirm = MessageBox.Show(
                    this,
                    "這會立即終止本次 Training，不等待目前 batch 完成。\n" +
                    "已產生的 scratch 候選檔仍會保留供除錯。確定終止？",
                    "終止訓練",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }

                AppendLog("TERMINATE requested PID=" + process.Id);
                _trace("RebuiltTraining: terminate PID=" + process.Id);
                process.Kill();
                SetStatus("Training 已強制終止", false);
            }
            catch (Exception exception)
            {
                HandleFailure("終止 Training 失敗", exception);
            }
        }

        private void ReadSelectedModel()
        {
            try
            {
                int station = ParseInt(_stationBox.Text, "Station", 1, 8);
                if (string.IsNullOrWhiteSpace(_recipePath) || !Directory.Exists(_recipePath))
                {
                    throw new DirectoryNotFoundException("目前沒有有效 Recipe 路徑。");
                }

                string modelPath = ResolveConfiguredTrainingSeed(station);
                if (!File.Exists(modelPath))
                {
                    throw new FileNotFoundException("找不到模型。", modelPath);
                }
                string modelName = Path.GetFileName(modelPath);

                string python = ProductRuntimePaths.GetTrainingPython(_projectRoot);
                string script = ProductRuntimePaths.GetTrainingScript(_projectRoot, "training_read_model.py");
                if (!File.Exists(python))
                {
                    throw new FileNotFoundException("找不到 Training Python runtime。", python);
                }
                if (!File.Exists(script))
                {
                    throw new FileNotFoundException("找不到 Training ReadModel 腳本。", script);
                }

                SetStatus("正在讀取模型 " + modelName, true);
                _readModelButton.IsEnabled = false;
                AppendLog("READMODEL START " + modelPath);
                _trace("RebuiltTraining: ReadModel begin model=" + modelPath);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = Quote(script) + " --model " + Quote(modelPath),
                    WorkingDirectory = Path.GetDirectoryName(modelPath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(psi, _projectRoot);
                Process process = Process.Start(psi);
                if (process == null)
                {
                    throw new InvalidOperationException("無法啟動 Training ReadModel。");
                }

                System.Threading.Tasks.Task<string> stdoutTask =
                    process.StandardOutput.ReadToEndAsync();
                System.Threading.Tasks.Task<string> stderrTask =
                    process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(30000))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException(
                        "Training ReadModel 超過 30 秒，已終止子程序。");
                }
                string stdout = stdoutTask.GetAwaiter().GetResult();
                string stderr = stderrTask.GetAwaiter().GetResult();

                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    AppendLog(stdout.Trim());
                }
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    AppendLog("READMODEL STDERR " + stderr.Trim());
                }
                if (process.ExitCode != 0 ||
                    stdout.IndexOf("STATUS=PASS", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw new InvalidOperationException(
                        "Training ReadModel 失敗，exit=" + process.ExitCode);
                }

                _activeModelPath = modelPath;
                SetSelectedStationRunVisual("success");
                SetStatus("模型讀取成功 — " + modelName, true);
                _trace("RebuiltTraining: ReadModel PASS model=" + modelPath);
            }
            catch (Exception exception)
            {
                SetSelectedStationRunVisual("failed");
                HandleFailure("讀取模型失敗", exception);
            }
            finally
            {
                if (_readModelButton != null)
                {
                    _readModelButton.IsEnabled = true;
                }
            }
        }

        private void ConvertToPb()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_activeModelPath) || !File.Exists(_activeModelPath))
                {
                    throw new FileNotFoundException("尚未產生 H5 模型。", _activeModelPath);
                }
                if (string.IsNullOrWhiteSpace(_activeIniPath) || !File.Exists(_activeIniPath))
                {
                    throw new FileNotFoundException("找不到本次 navs.ini。", _activeIniPath);
                }

                string python = ProductRuntimePaths.GetAIRuntimeControl(_projectRoot);
                string args =
                    "inspection --root " + Quote(ProductRuntimePaths.ResolveProductRoot(_projectRoot)) +
                    " --slot local --workdir " + Quote(Path.GetDirectoryName(_activeModelPath)) +
                    " --gpu " + _gpuBox.Text.Trim() + " --" +
                    " -s 0 -g " + _gpuBox.Text.Trim() +
                    " -i " + Quote(_activeIniPath) +
                    " -m " + Quote(_activeModelPath) +
                    " -v DAVS_V3 -r SaveAsPB" +
                    " -o " + Quote(Path.GetDirectoryName(_activeModelPath));

                SetStatus("PB 轉換中", true);
                _convertButton.IsEnabled = false;
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(_activeModelPath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(
                    psi,
                    _projectRoot,
                    ParseInt(_gpuBox.Text, "GPU", 0, 32));
                ProcessCaptureResult conversion = RunProcessCapture(
                    psi,
                    PbConversionTimeoutMs,
                    "PB 轉換");
                AppendProcessCapture("PB", conversion);

                string pb = Path.ChangeExtension(_activeModelPath, ".pb");
                if (!File.Exists(pb))
                {
                    throw new FileNotFoundException("converter 未產生 PB。", pb);
                }
                SetStatus("PB 已產生", true);
                AppendLog("PB=" + pb + " size=" + new FileInfo(pb).Length);
                _previewButton.IsEnabled = true;
            }
            catch (Exception exception)
            {
                HandleFailure("PB 轉換失敗", exception);
            }
            finally
            {
                _convertButton.IsEnabled = true;
            }
        }

        private void ShowOriginalStylePreview()
        {
            try
            {
                string oldModel = ResolveConfiguredTrainingSeed(_selectedStation);
                string newModel =
                    !string.IsNullOrWhiteSpace(_activeModelPath) && File.Exists(_activeModelPath)
                        ? _activeModelPath
                        : oldModel;

                int width;
                int height;
                if (!int.TryParse(_widthBox.Text, out width) || width <= 0)
                {
                    width = 256;
                }
                if (!int.TryParse(_heightBox.Text, out height) || height <= 0)
                {
                    height = 256;
                }

                string xRoot = _xPathBox != null ? _xPathBox.Text.Trim() : string.Empty;
                string yRoot = _yPathBox != null ? _yPathBox.Text.Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(xRoot) || !Directory.Exists(xRoot))
                {
                    throw new DirectoryNotFoundException("找不到目前站別的 Training X 路徑：" + xRoot);
                }

                RebuiltTrainingPreviewWindow preview = new RebuiltTrainingPreviewWindow(
                    _projectRoot,
                    xRoot,
                    yRoot,
                    oldModel,
                    newModel,
                    width,
                    height,
                    _selectedStation,
                    _trace);
                preview.Owner = this;
                preview.Show();
                AppendLog(
                    "AIPREVIEW OPEN S" + _selectedStation +
                    " old=" + oldModel +
                    " new=" + newModel);
                _trace(
                    "RebuiltTraining: AIPreviewWindow shown S" +
                    _selectedStation +
                    " old=" + oldModel +
                    " new=" + newModel);
            }
            catch (Exception exception)
            {
                HandleFailure("模型預覽視窗開啟失敗", exception);
            }
        }

        private void RunCandidatePreview()
        {
            try
            {
                if (_previewProcess != null && !_previewProcess.HasExited)
                {
                    MessageBox.Show(this, "預覽回測已在執行中。", "預覽");
                    return;
                }

                TrainingRun run = BuildRun();
                string pb = Path.ChangeExtension(run.ModelPath, ".pb");
                if (!File.Exists(run.ModelPath))
                {
                    throw new FileNotFoundException(
                        "尚未產生候選模型。",
                        run.ModelPath);
                }

                DirectoryInfo xDirectory = new DirectoryInfo(run.XPath);
                DirectoryInfo datasetRoot = xDirectory.Parent;
                if (datasetRoot == null)
                {
                    throw new InvalidOperationException("無法由 X 路徑推導 TrainingImage 根目錄。");
                }

                string originalRoot = Path.Combine(
                    datasetRoot.FullName,
                    "Origional",
                    run.Station.ToString(CultureInfo.InvariantCulture));
                string okDir = Path.Combine(originalRoot, "OK");
                string ngDir = Path.Combine(originalRoot, "NG");
                if (!Directory.Exists(okDir) || !Directory.Exists(ngDir))
                {
                    throw new DirectoryNotFoundException(
                        "預覽回測需要 Origional\\<Station>\\OK/NG。\n" +
                        "OK=" + okDir + "\nNG=" + ngDir);
                }

                string python = ProductRuntimePaths.GetTrainingPython(_projectRoot);
                string script = ProductRuntimePaths.GetTrainingScript(
                    _projectRoot,
                    "run_candidate_quality_preview.py");
                if (!File.Exists(script))
                {
                    throw new FileNotFoundException("找不到候選模型回測腳本。", script);
                }

                string output = Path.Combine(run.OutputRoot, "quality_preview.json");
                int previewMax = run.MaxSamples > 0
                    ? Math.Min(run.MaxSamples, 100)
                    : 20;
                string args =
                    Quote(script) +
                    " --pb " + Quote(pb) +
                    " --h5 " + Quote(run.ModelPath) +
                    " --recipe " + Quote(_recipePath) +
                    " --station " + run.Station +
                    " --ok-dir " + Quote(okDir) +
                    " --ng-dir " + Quote(ngDir) +
                    " --max-images " + previewMax +
                    " --output " + Quote(output);

                SetStatus("預覽回測準備中 0%", true);
                _previewButton.IsEnabled = false;
                _startButton.IsEnabled = false;
                _readModelButton.IsEnabled = false;
                _terminateButton.IsEnabled = true;
                _terminateButton.Content = "停止預覽";
                _progressBar.Value = 0;
                _previewStartedAt = DateTime.Now;
                _previewOutputPath = output;
                AppendLog("PREVIEW START " + args);
                _trace("RebuiltTraining: preview start pb=" + pb);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = args,
                    WorkingDirectory = ProductRuntimePaths.GetTrainingRebuildRoot(_projectRoot),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(psi, _projectRoot, run.Gpu);
                Process process = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };
                process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs eventArgs)
                {
                    if (string.IsNullOrWhiteSpace(eventArgs.Data))
                    {
                        return;
                    }
                    string line = eventArgs.Data;
                    Dispatcher.BeginInvoke(new Action(() => HandlePreviewOutput(line)));
                };
                process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs eventArgs)
                {
                    if (string.IsNullOrWhiteSpace(eventArgs.Data))
                    {
                        return;
                    }
                    string line = eventArgs.Data;
                    Dispatcher.BeginInvoke(new Action(() => AppendLog("ERR> " + line)));
                };
                process.Exited += delegate
                {
                    try
                    {
                        process.WaitForExit();
                    }
                    catch
                    {
                    }
                    int exitCode = process.ExitCode;
                    Dispatcher.BeginInvoke(new Action(() => OnPreviewExited(process, exitCode)));
                };

                _previewProcess = process;
                if (!process.Start())
                {
                    _previewProcess = null;
                    throw new InvalidOperationException("無法啟動候選模型回測程序。");
                }
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception exception)
            {
                _previewProcess = null;
                _previewButton.IsEnabled = true;
                _startButton.IsEnabled = true;
                _readModelButton.IsEnabled = true;
                _terminateButton.IsEnabled = false;
                _terminateButton.Content = "終止訓練";
                HandleFailure("候選模型預覽回測失敗", exception);
            }
        }

        private void HandlePreviewOutput(string line)
        {
            AppendLog(line);
            if (!line.StartsWith("PREVIEW_PROGRESS ", StringComparison.Ordinal))
            {
                return;
            }

            double? percentValue = ExtractJsonNumber(line, "percent");
            int percent = percentValue.HasValue
                ? Math.Max(0, Math.Min(100, (int)Math.Round(percentValue.Value)))
                : 0;
            string stage = ExtractJsonString(line, "stage") ?? "running";
            string detail = ExtractJsonString(line, "detail") ?? string.Empty;

            _progressBar.Value = percent;
            int selectedIndex = _selectedStation - 1;
            if (selectedIndex >= 0 && selectedIndex < _stationTestProgressBars.Length)
            {
                if (_stationTestProgressBars[selectedIndex] != null)
                {
                    _stationTestProgressBars[selectedIndex].Value = percent;
                }
                if (_stationTestProgressTexts[selectedIndex] != null)
                {
                    _stationTestProgressTexts[selectedIndex].Text =
                        percent.ToString(CultureInfo.InvariantCulture) + "%";
                }
            }
            _statusText.Text = "預覽回測 " + percent + "%  " + detail;
            _timeText.Text =
                "回測時間 " + FormatDuration(DateTime.Now - _previewStartedAt) +
                " / 階段 " + stage;
            SetSelectedStationResultText(
                "預覽回測        " + percent + "%",
                string.IsNullOrWhiteSpace(detail) ? "處理中" : detail);
        }

        private void OnPreviewExited(Process process, int exitCode)
        {
            if (!ReferenceEquals(_previewProcess, process))
            {
                return;
            }

            _previewProcess = null;
            _previewButton.IsEnabled = true;
            _startButton.IsEnabled = true;
            _readModelButton.IsEnabled = true;
            _terminateButton.IsEnabled = false;
            _terminateButton.Content = "終止訓練";

            bool success =
                exitCode == 0 &&
                !string.IsNullOrWhiteSpace(_previewOutputPath) &&
                File.Exists(_previewOutputPath);
            if (!success)
            {
                _progressBar.Value = 0;
                SetStatus("預覽回測失敗 exit=" + exitCode, false);
                SetSelectedStationRunVisual("failed");
                AppendLog("PREVIEW EXIT=" + exitCode);
                return;
            }

            _progressBar.Value = 100;
            int selectedIndex = _selectedStation - 1;
            if (selectedIndex >= 0 && selectedIndex < _stationTestProgressBars.Length)
            {
                if (_stationTestProgressBars[selectedIndex] != null)
                {
                    _stationTestProgressBars[selectedIndex].Value = 100;
                }
                if (_stationTestProgressTexts[selectedIndex] != null)
                {
                    _stationTestProgressTexts[selectedIndex].Text = "100%";
                }
            }
            string summary = ReadPreviewSummary(_previewOutputPath);
            SetStatus("預覽回測完成 — " + summary, true);
            _timeText.Text =
                "回測時間 " + FormatDuration(DateTime.Now - _previewStartedAt) +
                " / 完成";
            AppendLog("PREVIEW RESULT " + summary + " file=" + _previewOutputPath);
            _trace("RebuiltTraining: preview PASS " + summary);
            string previewJson = File.ReadAllText(_previewOutputPath, Encoding.UTF8);
            double? okRate = ExtractJsonNumber(previewJson, "ok_pass_rate");
            double? ngRate = ExtractJsonNumber(previewJson, "ng_catch_rate");
            double? balanced = ExtractJsonNumber(previewJson, "balanced_accuracy");
            SetSelectedStationResultText(
                "整站 OK率      " + FormatPercent(okRate),
                "整站 NG命中    " + FormatPercent(ngRate) +
                " / Balanced " + FormatPercent(balanced));
            SetSelectedStationRunVisual("success");
        }

        private static string ReadPreviewSummary(string path)
        {
            if (!File.Exists(path))
            {
                return "result file missing";
            }

            string text = File.ReadAllText(path, Encoding.UTF8);
            double? okRate = ExtractJsonNumber(text, "ok_pass_rate");
            double? ngRate = ExtractJsonNumber(text, "ng_catch_rate");
            double? balanced = ExtractJsonNumber(text, "balanced_accuracy");
            return
                "OK pass=" + FormatPercent(okRate) +
                " / NG catch=" + FormatPercent(ngRate) +
                " / Balanced=" + FormatPercent(balanced);
        }

        private static double? ExtractJsonNumber(string text, string key)
        {
            string marker = "\"" + key + "\"";
            int keyIndex = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return null;
            }
            int colon = text.IndexOf(':', keyIndex + marker.Length);
            if (colon < 0)
            {
                return null;
            }
            int start = colon + 1;
            while (start < text.Length && char.IsWhiteSpace(text[start]))
            {
                start++;
            }
            if (start + 4 <= text.Length &&
                string.Equals(
                    text.Substring(start, 4),
                    "null",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            int end = start;
            while (end < text.Length &&
                (char.IsDigit(text[end]) ||
                 text[end] == '.' ||
                 text[end] == '-' ||
                 text[end] == '+' ||
                 text[end] == 'e' ||
                 text[end] == 'E'))
            {
                end++;
            }
            double value;
            return double.TryParse(
                text.Substring(start, end - start),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value)
                    ? (double?)value
                    : null;
        }

        private static string FormatPercent(double? value)
        {
            return value.HasValue
                ? (value.Value * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%"
                : "--";
        }

        private void RunDeploymentCheck()
        {
            try
            {
                TrainingRun run = BuildRun();
                string pb = Path.ChangeExtension(run.ModelPath, ".pb");
                if (!File.Exists(run.ModelPath) || !File.Exists(pb))
                {
                    throw new FileNotFoundException(
                        "部署檢查需要本次候選 H5/PB。請先完成 Training 與 H5 → PB。",
                        !File.Exists(run.ModelPath) ? run.ModelPath : pb);
                }

                DirectoryInfo recipe = new DirectoryInfo(_recipePath);
                DirectoryInfo recipeContainer = recipe.Parent;
                DirectoryInfo machineRoot = recipeContainer != null
                    ? recipeContainer.Parent
                    : null;
                if (machineRoot == null)
                {
                    throw new InvalidOperationException("無法由目前 Recipe 推導機台根目錄。");
                }

                string python = ProductRuntimePaths.GetTrainingPython(_projectRoot);
                string planner = ProductRuntimePaths.GetTrainingScript(
                    _projectRoot,
                    "plan_model_deployment.py");
                if (!File.Exists(planner))
                {
                    throw new FileNotFoundException("找不到部署檢查器。", planner);
                }

                string output = Path.Combine(run.OutputRoot, "deployment_plan.json");
                string args =
                    Quote(planner) +
                    " --machine-root " + Quote(machineRoot.FullName) +
                    " --recipe " + Quote(_recipePath) +
                    " --station " + run.Station +
                    " --candidate-h5 " + Quote(run.ModelPath) +
                    " --candidate-pb " + Quote(pb) +
                    " --candidate-ini " + Quote(run.IniPath) +
                    " --output " + Quote(output);

                _deployCheckButton.IsEnabled = false;
                SetStatus("部署契約檢查中", true);
                AppendLog("DEPLOY CHECK START " + args);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = args,
                    WorkingDirectory = ProductRuntimePaths.GetTrainingRebuildRoot(_projectRoot),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(psi, _projectRoot, run.Gpu);
                ProcessCaptureResult deployment = RunProcessCapture(
                    psi,
                    DeploymentCheckTimeoutMs,
                    "部署檢查");
                AppendProcessCapture("DEPLOY", deployment);

                string json = File.Exists(output)
                    ? File.ReadAllText(output, Encoding.UTF8)
                    : string.Empty;
                string status = ExtractJsonString(json, "status") ?? "UNKNOWN";
                bool ready = string.Equals(
                    status,
                    "READY_DRY_RUN",
                    StringComparison.OrdinalIgnoreCase);
                bool liveRuntimeReady =
                    _hasLiveInspectionStations != null &&
                    _hasLiveInspectionStations();
                _replaceAllButton.IsEnabled = ready && liveRuntimeReady;
                int resultIndex = _selectedStation - 1;
                if (resultIndex >= 0 &&
                    resultIndex < _stationUpdateButtons.Length &&
                    _stationUpdateButtons[resultIndex] != null)
                {
                    _stationUpdateButtons[resultIndex].IsEnabled =
                        ready && liveRuntimeReady;
                }
                SetStatus(
                    ready && liveRuntimeReady
                        ? "部署檢查 READY — Live Runtime 可 Reload"
                        : ready
                            ? "部署檢查 READY，但目前沒有 persistent live Stations"
                        : "部署檢查 " + status + "（未寫入）",
                    ready && liveRuntimeReady);
                AppendLog("DEPLOY CHECK RESULT status=" + status + " file=" + output);
                AppendLog("DEPLOY CHECK LIVE_RUNTIME=" + liveRuntimeReady);
                _trace("RebuiltTraining: deployment check status=" + status);
            }
            catch (Exception exception)
            {
                HandleFailure("部署檢查失敗", exception);
            }
            finally
            {
                _deployCheckButton.IsEnabled = true;
            }
        }

        private void ApplyDeployment()
        {
            try
            {
                if (_hasLiveInspectionStations == null ||
                    !_hasLiveInspectionStations())
                {
                    throw new InvalidOperationException(
                        "目前沒有 persistent CVInspection[8]；禁止先寫 active model 再嘗試 Reload。請從檢測設定內開啟 Training 後再執行全部更換。");
                }

                TrainingRun run = BuildRun();
                string pb = Path.ChangeExtension(run.ModelPath, ".pb");
                string plan = Path.Combine(run.OutputRoot, "deployment_plan.json");
                if (!File.Exists(run.ModelPath) || !File.Exists(pb) || !File.Exists(plan))
                {
                    throw new FileNotFoundException(
                        "全部更換需要候選 H5、PB 與 READY deployment_plan.json。請先執行更換前檢查。");
                }

                string planJson = File.ReadAllText(plan, Encoding.UTF8);
                string planStatus = ExtractJsonString(planJson, "status") ?? "UNKNOWN";
                if (!string.Equals(planStatus, "READY_DRY_RUN", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "部署計畫尚未 READY，禁止更換。status=" + planStatus);
                }

                MessageBoxResult confirm = MessageBox.Show(
                    this,
                    "將先備份目前 active 模型，再以本次 Training 的 H5/PB 取代，並更新 LastModelFile_A。\n" +
                    "若 transaction 中途失敗會從 Backup 回滾。\n\n確定執行「全部更換」？",
                    "模型全部更換",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }

                DirectoryInfo recipe = new DirectoryInfo(_recipePath);
                DirectoryInfo recipeContainer = recipe.Parent;
                DirectoryInfo machineRoot = recipeContainer != null ? recipeContainer.Parent : null;
                if (machineRoot == null)
                {
                    throw new InvalidOperationException("無法由目前 Recipe 推導機台根目錄。");
                }

                string python = ProductRuntimePaths.GetTrainingPython(_projectRoot);
                string applier = ProductRuntimePaths.GetTrainingScript(
                    _projectRoot,
                    "apply_model_deployment.py");
                if (!File.Exists(applier))
                {
                    throw new FileNotFoundException("找不到模型部署 transaction。", applier);
                }

                string output = Path.Combine(run.OutputRoot, "deployment_result.json");
                string args =
                    Quote(applier) +
                    " --machine-root " + Quote(machineRoot.FullName) +
                    " --recipe " + Quote(_recipePath) +
                    " --station " + run.Station +
                    " --candidate-h5 " + Quote(run.ModelPath) +
                    " --candidate-pb " + Quote(pb) +
                    " --plan " + Quote(plan) +
                    " --output " + Quote(output) +
                    " --allow-write";

                _replaceAllButton.IsEnabled = false;
                SetStatus("模型備份 / 更換中", true);
                AppendLog("DEPLOY APPLY START " + args);
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = args,
                    WorkingDirectory = ProductRuntimePaths.GetTrainingRebuildRoot(_projectRoot),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(psi, _projectRoot, run.Gpu);
                Process process = Process.Start(psi);
                if (process == null)
                {
                    throw new InvalidOperationException("無法啟動模型部署 transaction。");
                }
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (!string.IsNullOrWhiteSpace(stdout)) AppendLog(stdout);
                if (!string.IsNullOrWhiteSpace(stderr)) AppendLog("ERR> " + stderr);
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException("模型部署 transaction exit=" + process.ExitCode);
                }

                string resultJson = File.Exists(output)
                    ? File.ReadAllText(output, Encoding.UTF8)
                    : string.Empty;
                string status = ExtractJsonString(resultJson, "status") ?? "UNKNOWN";
                bool deployed = string.Equals(
                    status,
                    "DEPLOYED_PENDING_RUNTIME_RELOAD",
                    StringComparison.OrdinalIgnoreCase);
                AppendLog("DEPLOY APPLY RESULT status=" + status + " file=" + output);
                _trace("RebuiltTraining: deployment apply status=" + status);
                if (!deployed)
                {
                    SetStatus("模型更換結果 " + status, false);
                    throw new InvalidOperationException(
                        "模型部署 transaction 未進入 runtime reload 階段。status=" + status);
                }

                if (_reloadStationModel == null)
                {
                    throw new InvalidOperationException(
                        "目前 Training 沒有 persistent Host runtime reload callback；部署必須回滾。");
                }

                try
                {
                    SetStatus("模型已更換，正在 Reload S" + run.Station, true);
                    string liveState = _reloadStationModel(run.Station);
                    AppendLog("RUNTIME RELOAD PASS " + liveState);
                    _trace("RebuiltTraining: runtime reload PASS " + liveState);
                    SetStatus("模型更換完成 — Runtime Ready", true);
                    _replaceAllButton.IsEnabled = false;
                    MessageBox.Show(
                        this,
                        "模型已備份、更換並由 genuine CVInspection.ReadModel() 重新載入。\n\n" +
                        liveState,
                        "模型更換完成",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception reloadException)
                {
                    AppendLog("RUNTIME RELOAD FAIL " + reloadException);
                    _trace("RebuiltTraining: runtime reload FAIL " + reloadException);
                    SetStatus("Runtime Reload 失敗，正在回滾模型", false);

                    string rollbackScript = ProductRuntimePaths.GetTrainingScript(
                        _projectRoot,
                        "rollback_model_deployment.py");
                    if (!File.Exists(rollbackScript))
                    {
                        throw new InvalidOperationException(
                            "Runtime Reload 失敗，且找不到 rollback script。",
                            reloadException);
                    }

                    string rollbackOutput = Path.Combine(
                        run.OutputRoot,
                        "deployment_rollback_result.json");
                    string rollbackArgs =
                        Quote(rollbackScript) +
                        " --deployment-result " + Quote(output) +
                        " --output " + Quote(rollbackOutput);
                    ProcessStartInfo rollbackPsi = new ProcessStartInfo
                    {
                        FileName = python,
                        Arguments = rollbackArgs,
                        WorkingDirectory = ProductRuntimePaths.GetTrainingRebuildRoot(_projectRoot),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    ProductRuntimePaths.ApplyTrainingEnvironment(rollbackPsi, _projectRoot, run.Gpu);
                    Process rollbackProcess = Process.Start(rollbackPsi);
                    if (rollbackProcess == null)
                    {
                        throw new InvalidOperationException(
                            "Runtime Reload 失敗，且無法啟動 rollback process。",
                            reloadException);
                    }
                    string rollbackStdout = rollbackProcess.StandardOutput.ReadToEnd();
                    string rollbackStderr = rollbackProcess.StandardError.ReadToEnd();
                    rollbackProcess.WaitForExit();
                    if (!string.IsNullOrWhiteSpace(rollbackStdout))
                    {
                        AppendLog(rollbackStdout);
                    }
                    if (!string.IsNullOrWhiteSpace(rollbackStderr))
                    {
                        AppendLog("ROLLBACK ERR> " + rollbackStderr);
                    }
                    if (rollbackProcess.ExitCode != 0)
                    {
                        throw new InvalidOperationException(
                            "Runtime Reload 失敗，rollback 也失敗，exit=" +
                            rollbackProcess.ExitCode,
                            reloadException);
                    }

                    string rollbackState = _reloadStationModel(run.Station);
                    AppendLog("ROLLBACK RUNTIME RELOAD PASS " + rollbackState);
                    _trace("RebuiltTraining: rollback runtime reload PASS " + rollbackState);
                    SetStatus("新模型 Reload 失敗；已回滾並恢復舊模型", false);
                    throw new InvalidOperationException(
                        "新模型已部署但 Runtime Reload 驗證失敗；系統已從 Backup 回滾，舊模型已重新載入。",
                        reloadException);
                }
            }
            catch (Exception exception)
            {
                _replaceAllButton.IsEnabled = true;
                HandleFailure("模型全部更換失敗", exception);
            }
        }

        private static string ExtractJsonString(string text, string key)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            string marker = "\"" + key + "\"";
            int keyIndex = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return null;
            }
            int colon = text.IndexOf(':', keyIndex + marker.Length);
            if (colon < 0)
            {
                return null;
            }
            int quote1 = text.IndexOf('"', colon + 1);
            if (quote1 < 0)
            {
                return null;
            }
            int quote2 = text.IndexOf('"', quote1 + 1);
            if (quote2 <= quote1)
            {
                return null;
            }
            return text.Substring(quote1 + 1, quote2 - quote1 - 1);
        }

        private TrainingRun BuildRun()
        {
            TrainingRun run = new TrainingRun();
            run.Station = ParseInt(_stationBox.Text, "Station", 1, 8);
            run.Gpu = ParseInt(_gpuBox.Text, "GPU", 0, 2);
            run.Width = ParseInt(_widthBox.Text, "Width", 16, 8192);
            run.Height = ParseInt(_heightBox.Text, "Height", 16, 8192);
            run.Epoch = ParseInt(_epochBox.Text, "Epoch", 1, 100000);
            run.BatchSize = ParseInt(_batchBox.Text, "Batch", 1, 1024);
            run.LossFunction = ParseInt(_lossBox.Text, "LossFunction", 1, 9);
            if (run.LossFunction != 1 &&
                run.LossFunction != 2 &&
                run.LossFunction != 3 &&
                run.LossFunction != 5 &&
                run.LossFunction != 6 &&
                run.LossFunction != 7 &&
                run.LossFunction != 8 &&
                run.LossFunction != 9)
            {
                throw new InvalidOperationException(
                    "原 DAVS V3 已重建的 LossFunction 為 1/2/3/5/6/7/8/9。收到: " +
                    run.LossFunction.ToString(CultureInfo.InvariantCulture));
            }
            run.UnetDiv = ParseInt(_unetDivBox.Text, "UnetDiv", 1, 64);
            run.AugRatio = ParseInt(_augRatioBox.Text, "AugRatio", 0, 100);
            run.MaxSamples = ParseInt(_maxSamplesBox.Text, "驗證樣本上限", 0, 1000000);
            run.LearningRate = ParseDouble(_learningRateBox.Text, "Learning Rate", 0.0000001, 10.0);
            run.ModelType = _workspaceModelType;
            run.MaybeArea = _workspaceMaybeArea;
            run.Threshold = _workspaceThreshold;

            run.XPath = Path.GetFullPath(_xPathBox.Text.Trim());
            run.YPath = Path.GetFullPath(_yPathBox.Text.Trim());
            run.OutputRoot = Path.GetFullPath(_outputRootBox.Text.Trim());
            if (!Directory.Exists(run.XPath))
            {
                throw new DirectoryNotFoundException("X 影像資料夾不存在: " + run.XPath);
            }
            if (!Directory.Exists(run.YPath))
            {
                throw new DirectoryNotFoundException("Y 標註資料夾不存在: " + run.YPath);
            }
            Directory.CreateDirectory(run.OutputRoot);
            run.ModelPath = Path.Combine(run.OutputRoot, "navs.h5");
            run.IniPath = Path.Combine(run.OutputRoot, "navs.ini");
            run.ProcDir = Path.Combine(run.OutputRoot, "proc");
            run.PredictOutputRoot = Path.Combine(run.OutputRoot, "predict_out");
            return run;
        }

        private static int ParseInt(string text, string name, int min, int max)
        {
            int value;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ||
                value < min || value > max)
            {
                throw new InvalidOperationException(
                    name + " 必須介於 " + min + " ~ " + max + "。收到: " + text);
            }
            return value;
        }

        private static double ParseDouble(string text, string name, double min, double max)
        {
            double value;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                value < min || value > max)
            {
                throw new InvalidOperationException(
                    name + " 必須介於 " + min.ToString(CultureInfo.InvariantCulture) +
                    " ~ " + max.ToString(CultureInfo.InvariantCulture) + "。收到: " + text);
            }
            return value;
        }

        private void WriteIni(TrainingRun run)
        {
            StringBuilder ini = new StringBuilder();
            ini.AppendLine("[DAVS_V3]");
            ini.AppendLine("SegModelName=" + run.ModelPath);
            ini.AppendLine("Epoch=" + run.Epoch);
            ini.AppendLine("LossFunction=" + run.LossFunction);
            // 原 DAVS Predict 會先刪除整個 OutPath。模型與 Predict 輸出不可
            // 共用同一層，否則回測會把剛訓練完成的 navs.h5 / navs.pb 一起刪除。
            ini.AppendLine("OutPath=" + run.PredictOutputRoot);
            ini.AppendLine("ImgWidth=" + run.Width);
            ini.AppendLine("ImgHeight=" + run.Height);
            ini.AppendLine("XPath=" + run.XPath);
            ini.AppendLine("YPath=" + run.YPath);
            ini.AppendLine("NGIgnore=0");
            ini.AppendLine("OKBlank=1");
            ini.AppendLine("AugRatio=" + run.AugRatio);
            ini.AppendLine("BatchSize=" + run.BatchSize);
            ini.AppendLine("LR=" + run.LearningRate.ToString(CultureInfo.InvariantCulture));
            ini.AppendLine("MaybeArea=" + run.MaybeArea.ToString(CultureInfo.InvariantCulture));
            ini.AppendLine("SplitTrain=0");
            ini.AppendLine("ProcDir=" + run.ProcDir);
            ini.AppendLine("TempName=temp");
            ini.AppendLine("PredPath=");
            ini.AppendLine("Threshold=" + run.Threshold.ToString(CultureInfo.InvariantCulture));
            ini.AppendLine("ModelType=" + run.ModelType);
            ini.AppendLine("UnetDiv=" + run.UnetDiv);
            ini.AppendLine();
            ini.AppendLine("[Augmentation]");
            ini.AppendLine("horizontalFlip=0");
            ini.AppendLine("verticalFlip=0");
            ini.AppendLine("brightness_limit=0");
            ini.AppendLine("contrast_limit=0");
            ini.AppendLine("scale_limit=0");
            ini.AppendLine("rotate_limit=0");
            ini.AppendLine("shift_limit=0");
            ini.AppendLine("hue_shift_limit=0");
            ini.AppendLine("sat_shift_limit=0");
            ini.AppendLine("val_shift_limit=0");
            ini.AppendLine("r_shift_limit=0");
            ini.AppendLine("g_shift_limit=0");
            ini.AppendLine("b_shift_limit=0");

            File.WriteAllText(run.IniPath, ini.ToString(), new UTF8Encoding(false));
            AppendLog("INI=" + run.IniPath);
        }

        private void RefreshProgress()
        {
            if (string.IsNullOrWhiteSpace(_activeProcDir) || !Directory.Exists(_activeProcDir))
            {
                return;
            }

            int best = 0;
            string bestFile = null;
            int selectedIndex = _selectedStation - 1;
            foreach (string file in Directory.GetFiles(_activeProcDir, "*.txt"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                int value;
                if (!int.TryParse(name, out value) || value < 0 || value > 100)
                {
                    continue;
                }

                try
                {
                    string line = File.ReadLines(file).FirstOrDefault() ?? string.Empty;
                    double parsedLoss;
                    if (double.TryParse(
                        line,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out parsedLoss) &&
                        !double.IsNaN(parsedLoss) &&
                        !double.IsInfinity(parsedLoss) &&
                        selectedIndex >= 0 && selectedIndex < _stationLossHistory.Length)
                    {
                        _stationLossHistory[selectedIndex][value] = parsedLoss;
                    }
                }
                catch (IOException)
                {
                }

                if (value >= best)
                {
                    best = value;
                    bestFile = file;
                }
            }
            UpdateLossCurveAvailability();
            if (bestFile != null)
            {
                _progressBar.Value = best;
                if (selectedIndex >= 0 && selectedIndex < _stationTrainProgressBars.Length)
                {
                    if (_stationTrainProgressBars[selectedIndex] != null)
                    {
                        _stationTrainProgressBars[selectedIndex].Value = best;
                    }
                    if (_stationTrainProgressTexts[selectedIndex] != null)
                    {
                        _stationTrainProgressTexts[selectedIndex].Text =
                            best.ToString(CultureInfo.InvariantCulture) + "%";
                    }
                }
                string loss = string.Empty;
                try
                {
                    loss = File.ReadLines(bestFile).FirstOrDefault() ?? string.Empty;
                }
                catch (IOException)
                {
                }
                if (!string.IsNullOrWhiteSpace(loss))
                {
                    loss = FormatLossScientific(loss);
                    _lastLossText = loss;
                    if (selectedIndex >= 0 && selectedIndex < _stationAccuracyTexts.Length &&
                        _stationAccuracyTexts[selectedIndex] != null)
                    {
                        // Genuine AITrainingWIndow.GetProgress() reads the
                        // progress txt body into accstr and displays it in
                        // LabAcc. Vendor SaveTxt writes logs['loss'] there.
                        _stationAccuracyTexts[selectedIndex].Text = loss;
                    }
                }
                bool processRunning =
                    _activeProcess != null && !_activeProcess.HasExited;
                if (processRunning)
                {
                    _statusText.Text = best >= 100
                        ? "Training 100% — 正在儲存模型並等待 trainer 結束"
                        : "Training " + best + "%  loss=" + loss;
                    SetSelectedStationResultText(
                        "訓練進度      " + best + "%",
                        "Loss            " +
                        (string.IsNullOrWhiteSpace(loss) ? "--" : loss));

                    DateTime now = DateTime.Now;
                    TimeSpan elapsed = now - _trainingStartedAt;
                    string remainingText = "--";
                    if (best > 0)
                    {
                        double remainingSeconds =
                            elapsed.TotalSeconds * (100.0 - best) / best;
                        if (!double.IsNaN(remainingSeconds) &&
                            !double.IsInfinity(remainingSeconds) &&
                            remainingSeconds >= 0)
                        {
                            remainingText = FormatDuration(
                                TimeSpan.FromSeconds(remainingSeconds));
                        }
                    }
                    _timeText.Text =
                        "訓練時間 " + FormatDuration(elapsed) +
                        " / 剩餘 " + remainingText;

                    if (best != _lastProgressPercent)
                    {
                        _lastProgressPercent = best;
                        _lastProgressAt = now;
                    }
                }
            }
        }

        private void OnTrainingExited(
            int exitCode,
            int childPid,
            DateTime childExitObservedAt,
            double childCpuSeconds)
        {
            double dispatcherDelayMs =
                Math.Max(0.0, (DateTime.Now - childExitObservedAt).TotalMilliseconds);
            _trace(
                "RebuiltTraining: child EXIT UI handled PID=" + childPid +
                " exit=" + exitCode +
                " dispatcherDelayMs=" + dispatcherDelayMs.ToString("F1", CultureInfo.InvariantCulture) +
                " cpuSec=" + childCpuSeconds.ToString("F3", CultureInfo.InvariantCulture) +
                " stdoutLines=" + _trainingStdoutLineCount +
                " stderrLines=" + _trainingStderrLineCount);
            RefreshProgress();
            bool stopRequested = _stopRequested;
            bool stopEscalated = _stopEscalated;
            TrainingRun completedRun = _activeRun;
            _activeProcess = null;
            _stopRequested = false;
            _stopEscalated = false;
            _stopRequestedAt = default(DateTime);
            _startButton.IsEnabled = true;
            _stopButton.IsEnabled = false;
            _terminateButton.IsEnabled = false;
            _convertButton.IsEnabled = true;
            bool modelExists = !string.IsNullOrWhiteSpace(_activeModelPath) && File.Exists(_activeModelPath);
            bool success = !stopRequested && exitCode == 0 && modelExists;
            _previewButton.IsEnabled = success;
            int resultIndex = _selectedStation - 1;
            if (resultIndex >= 0 &&
                resultIndex < _stationPreviewButtons.Length &&
                _stationPreviewButtons[resultIndex] != null)
            {
                _stationPreviewButtons[resultIndex].IsEnabled = success;
            }
            if (_trainingStartedAt != default(DateTime))
            {
                _timeText.Text =
                    "訓練時間 " + FormatDuration(DateTime.Now - _trainingStartedAt) +
                    " / 剩餘 00:00:00";
            }
            if (stopRequested)
            {
                SetStatus(
                    stopEscalated
                        ? "Training 已停止（軟停止逾時後強制終止）"
                        : "Training 已停止",
                    false);
                SetSelectedStationRunVisual(string.Empty);
                SetSelectedStationResultText(
                    stopEscalated ? "訓練已強制停止" : "訓練已停止",
                    "Loss            " +
                    (string.IsNullOrWhiteSpace(_lastLossText) ? "--" : _lastLossText));
            }
            else
            {
                SetStatus(
                    success ? "Training PASS — H5 已產生" : "Training FAIL exit=" + exitCode,
                    success);
                SetSelectedStationRunVisual(success ? "success" : "failed");
                SetSelectedStationResultText(
                    success
                        ? "訓練完成      100%"
                        : "訓練失敗      exit=" + exitCode,
                    "Loss            " +
                    (string.IsNullOrWhiteSpace(_lastLossText) ? "--" : _lastLossText));
            }
            AppendLog(
                "EXIT=" + exitCode +
                " model=" + (_activeModelPath ?? "<null>") +
                " exists=" + modelExists +
                " stopRequested=" + stopRequested +
                " stopEscalated=" + stopEscalated);
            _trace(
                "RebuiltTraining: exit=" + exitCode +
                " modelExists=" + modelExists +
                " stopRequested=" + stopRequested +
                " stopEscalated=" + stopEscalated +
                " model=" + (_activeModelPath ?? "<null>"));

            if (success)
            {
                BeginPostTrainingFinalize(completedRun);
            }
            else if (TrainingQueueFeature && _standaloneQueueRunning)
            {
                FinishStandaloneQueueItem(false, stopRequested ? "STOPPED" : "FAILED");
            }
        }

        private void BeginPostTrainingFinalize(TrainingRun run)
        {
            try
            {
                if (run == null)
                {
                    throw new InvalidOperationException("Training 完成後找不到本次 run context。");
                }
                if (!File.Exists(run.ModelPath) || !File.Exists(run.IniPath))
                {
                    throw new FileNotFoundException(
                        "Training 完成後缺少 H5 或 navs.ini。",
                        !File.Exists(run.ModelPath) ? run.ModelPath : run.IniPath);
                }

                string python = ProductRuntimePaths.GetAIRuntimeControl(_projectRoot);
                string pb = Path.ChangeExtension(run.ModelPath, ".pb");
                string convertArguments =
                    "inspection --root " + Quote(ProductRuntimePaths.ResolveProductRoot(_projectRoot)) +
                    " --slot local --workdir " + Quote(Path.GetDirectoryName(run.ModelPath)) +
                    " --gpu " + run.Gpu.ToString(CultureInfo.InvariantCulture) + " --" +
                    " -s 0 -g " + run.Gpu.ToString(CultureInfo.InvariantCulture) +
                    " -i " + Quote(run.IniPath) +
                    " -m " + Quote(run.ModelPath) +
                    " -v DAVS_V3 -r SaveAsPB" +
                    " -o " + Quote(Path.GetDirectoryName(run.ModelPath));

                DirectoryInfo recipe = new DirectoryInfo(_recipePath);
                DirectoryInfo recipeContainer = recipe.Parent;
                DirectoryInfo machineRoot = recipeContainer != null
                    ? recipeContainer.Parent
                    : null;
                if (machineRoot == null)
                {
                    throw new InvalidOperationException("無法由目前 Recipe 推導機台根目錄。");
                }
                string planner = ProductRuntimePaths.GetTrainingScript(
                    _projectRoot,
                    "plan_model_deployment.py");
                if (!File.Exists(planner))
                {
                    throw new FileNotFoundException("找不到部署檢查器。", planner);
                }
                string deploymentOutput = Path.Combine(
                    run.OutputRoot,
                    "deployment_plan.json");
                string deploymentArguments =
                    Quote(planner) +
                    " --machine-root " + Quote(machineRoot.FullName) +
                    " --recipe " + Quote(_recipePath) +
                    " --station " + run.Station.ToString(CultureInfo.InvariantCulture) +
                    " --candidate-h5 " + Quote(run.ModelPath) +
                    " --candidate-pb " + Quote(pb) +
                    " --candidate-ini " + Quote(run.IniPath) +
                    " --output " + Quote(deploymentOutput);

                ProcessStartInfo convertPsi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = convertArguments,
                    WorkingDirectory = Path.GetDirectoryName(run.ModelPath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(convertPsi, _projectRoot, run.Gpu);

                ProcessStartInfo deploymentPsi = new ProcessStartInfo
                {
                    FileName = python,
                    Arguments = deploymentArguments,
                    WorkingDirectory = ProductRuntimePaths.GetTrainingRebuildRoot(_projectRoot),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ProductRuntimePaths.ApplyTrainingEnvironment(deploymentPsi, _projectRoot, run.Gpu);

                _postTrainingFinalizeInProgress = true;
                _startButton.IsEnabled = false;
                _convertButton.IsEnabled = false;
                _deployCheckButton.IsEnabled = false;
                SetStatus("Training 完成 — 背景轉換 PB / 部署檢查中", true);
                AppendLog("POST FINALIZE START model=" + run.ModelPath);
                _trace("RebuiltTraining: post-finalize background start model=" + run.ModelPath);

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate(object sender, DoWorkEventArgs eventArgs)
                {
                    ProcessCaptureResult convertResult = RunProcessCapture(
                        convertPsi,
                        PbConversionTimeoutMs,
                        "PB 轉換");
                    if (!File.Exists(pb))
                    {
                        throw new FileNotFoundException("converter 未產生 PB。", pb);
                    }

                    ProcessCaptureResult deploymentResult = RunProcessCapture(
                        deploymentPsi,
                        DeploymentCheckTimeoutMs,
                        "部署檢查");
                    string json = File.Exists(deploymentOutput)
                        ? File.ReadAllText(deploymentOutput, Encoding.UTF8)
                        : string.Empty;
                    eventArgs.Result = new PostFinalizeResult
                    {
                        PbPath = pb,
                        Conversion = convertResult,
                        Deployment = deploymentResult,
                        DeploymentOutputPath = deploymentOutput,
                        DeploymentStatus = ExtractJsonString(json, "status") ?? "UNKNOWN"
                    };
                };
                worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs eventArgs)
                {
                    _postTrainingFinalizeInProgress = false;
                    _startButton.IsEnabled = true;
                    _convertButton.IsEnabled = true;
                    _deployCheckButton.IsEnabled = true;

                    if (eventArgs.Error != null)
                    {
                        _trace(
                            "RebuiltTraining: post-finalize failed " +
                            eventArgs.Error.GetType().Name + ": " + eventArgs.Error.Message);
                        HandleFailure("Training 後處理失敗", eventArgs.Error);
                        if (TrainingQueueFeature && _standaloneQueueRunning)
                        {
                            FinishStandaloneQueueItem(false, "FINALIZE FAILED");
                        }
                        return;
                    }

                    PostFinalizeResult result = (PostFinalizeResult)eventArgs.Result;
                    AppendProcessCapture("PB", result.Conversion);
                    AppendLog(
                        "PB=" + result.PbPath +
                        " size=" + new FileInfo(result.PbPath).Length);
                    AppendProcessCapture("DEPLOY", result.Deployment);

                    bool ready = string.Equals(
                        result.DeploymentStatus,
                        "READY_DRY_RUN",
                        StringComparison.OrdinalIgnoreCase);
                    bool liveRuntimeReady =
                        _hasLiveInspectionStations != null &&
                        _hasLiveInspectionStations();
                    _replaceAllButton.IsEnabled = ready && liveRuntimeReady;
                    int stationIndex = run.Station - 1;
                    if (stationIndex >= 0 &&
                        stationIndex < _stationUpdateButtons.Length &&
                        _stationUpdateButtons[stationIndex] != null)
                    {
                        _stationUpdateButtons[stationIndex].IsEnabled =
                            ready && liveRuntimeReady;
                    }
                    _previewButton.IsEnabled = true;
                    SetStatus(
                        ready && liveRuntimeReady
                            ? "Training 完成 — PB / 部署檢查 READY"
                            : ready
                                ? "Training 完成 — 部署檢查 READY，目前無 persistent live Stations"
                                : "Training 完成 — 部署檢查 " + result.DeploymentStatus,
                        ready && liveRuntimeReady);
                    AppendLog(
                        "POST FINALIZE RESULT status=" + result.DeploymentStatus +
                        " file=" + result.DeploymentOutputPath);
                    _trace(
                        "RebuiltTraining: post-finalize PASS deployment=" +
                        result.DeploymentStatus);

                    if (_retestAfterTrainingCheck != null &&
                        _retestAfterTrainingCheck.IsChecked == true)
                    {
                        RunCandidatePreview();
                    }

                    if (TrainingQueueFeature && _standaloneQueueRunning)
                    {
                        FinishStandaloneQueueItem(true, "DONE");
                    }
                };
                worker.RunWorkerAsync();
            }
            catch (Exception exception)
            {
                _postTrainingFinalizeInProgress = false;
                _startButton.IsEnabled = true;
                _convertButton.IsEnabled = true;
                _deployCheckButton.IsEnabled = true;
                HandleFailure("Training 後處理啟動失敗", exception);
                if (TrainingQueueFeature && _standaloneQueueRunning)
                {
                    FinishStandaloneQueueItem(false, "FINALIZE FAILED");
                }
            }
        }

        private static ProcessCaptureResult RunProcessCapture(
            ProcessStartInfo startInfo,
            int timeoutMs,
            string stageName)
        {
            using (Process process = new Process { StartInfo = startInfo })
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                if (!process.Start())
                {
                    throw new InvalidOperationException("無法啟動 " + stageName + " 子程序。");
                }
                int processId = process.Id;
                System.Threading.Tasks.Task<string> stdoutTask =
                    process.StandardOutput.ReadToEndAsync();
                System.Threading.Tasks.Task<string> stderrTask =
                    process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { }
                    try { process.WaitForExit(5000); } catch { }
                    stopwatch.Stop();
                    throw new TimeoutException(
                        stageName + " PID=" + processId + " 超過 " +
                        Math.Ceiling(timeoutMs / 1000.0).ToString(CultureInfo.InvariantCulture) +
                        " 秒，已終止子程序。");
                }
                stopwatch.Stop();
                string stdout = stdoutTask.GetAwaiter().GetResult();
                string stderr = stderrTask.GetAwaiter().GetResult();
                double cpuSeconds = 0.0;
                try
                {
                    cpuSeconds = process.TotalProcessorTime.TotalSeconds;
                }
                catch
                {
                }
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        stageName + " PID=" + processId +
                        " exit=" + process.ExitCode +
                        " elapsedMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture) +
                        " stdout=" + TailText(stdout, 1500) +
                        " stderr=" + TailText(stderr, 1500));
                }
                return new ProcessCaptureResult
                {
                    ProcessId = processId,
                    ExitCode = process.ExitCode,
                    ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                    CpuSeconds = cpuSeconds,
                    Stdout = stdout,
                    Stderr = stderr
                };
            }
        }

        private void AppendProcessCapture(string prefix, ProcessCaptureResult result)
        {
            if (result == null)
            {
                return;
            }
            AppendLog(
                prefix + " PROCESS pid=" + result.ProcessId +
                " exit=" + result.ExitCode +
                " elapsedMs=" + result.ElapsedMilliseconds.ToString("F1", CultureInfo.InvariantCulture) +
                " cpuSec=" + result.CpuSeconds.ToString("F3", CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(result.Stdout))
            {
                AppendLog(prefix + "> " + result.Stdout.Trim());
            }
            if (!string.IsNullOrWhiteSpace(result.Stderr))
            {
                AppendLog(prefix + " ERR> " + result.Stderr.Trim());
            }
        }

        private static string TailText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text ?? string.Empty;
            }
            return text.Substring(text.Length - maxLength, maxLength);
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                duration = TimeSpan.Zero;
            }
            int hours = (int)Math.Floor(duration.TotalHours);
            return
                hours.ToString("00", CultureInfo.InvariantCulture) + ":" +
                duration.Minutes.ToString("00", CultureInfo.InvariantCulture) + ":" +
                duration.Seconds.ToString("00", CultureInfo.InvariantCulture);
        }

        private void OpenOutputDirectory()
        {
            try
            {
                string output = Path.GetFullPath(_outputRootBox.Text.Trim());
                Directory.CreateDirectory(output);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = Quote(output),
                    UseShellExecute = true
                });
            }
            catch (Exception exception)
            {
                HandleFailure("開啟輸出目錄失敗", exception);
            }
        }

        private void AppendLog(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }
            string rendered =
                DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                " " + line.TrimEnd() + Environment.NewLine;
            if (_logBox != null)
            {
                _logBox.AppendText(rendered);
                _logBox.ScrollToEnd();
            }
            if (_resultCommandLog != null)
            {
                _resultCommandLog.AppendText(rendered);
                _resultCommandLog.ScrollToEnd();
            }
        }

        private void SetStatus(string text, bool good)
        {
            _statusText.Text = text;
            _statusText.Foreground = good ? Brushes.DarkGreen : Brushes.DarkOrange;
        }

        private void HandleFailure(string title, Exception exception)
        {
            AppendLog(title + ": " + exception);
            SetStatus(title, false);
            _trace("RebuiltTraining: " + title + " " + exception);
            MessageBox.Show(
                this,
                exception.Message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private static string Quote(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private sealed class ProcessCaptureResult
        {
            internal int ProcessId;
            internal int ExitCode;
            internal double ElapsedMilliseconds;
            internal double CpuSeconds;
            internal string Stdout;
            internal string Stderr;
        }

        private sealed class PostFinalizeResult
        {
            internal string PbPath;
            internal ProcessCaptureResult Conversion;
            internal ProcessCaptureResult Deployment;
            internal string DeploymentOutputPath;
            internal string DeploymentStatus;
        }

        private sealed class TrainingRun
        {
            internal int Station;
            internal int Gpu;
            internal int Width;
            internal int Height;
            internal int Epoch;
            internal int BatchSize;
            internal int LossFunction;
            internal int UnetDiv;
            internal int AugRatio;
            internal int MaxSamples;
            internal double LearningRate;
            internal double MaybeArea;
            internal double Threshold;
            internal string ModelType;
            internal string XPath;
            internal string YPath;
            internal string OutputRoot;
            internal string ModelPath;
            internal string IniPath;
            internal string ProcDir;
            internal string PredictOutputRoot;
        }
    }
}
