using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InspTools.Standalone
{
    /// <summary>
    /// Thin independent launcher for the phase-1 decoded/rebuilt InspTools UI.
    /// No generic editor, SDK host, Agent or Annotation product belongs here.
    /// Recipe loading is optional and reuses the phase-1 compiled lifecycle.
    /// </summary>
    internal static class DecodedToolsWindowHost
    {
        internal static int Run(string[] args)
        {
            // Never accidentally call the eager native CreateInstance path.
            // Full model-enabled initialization must pass a separate R3 gate.
            if (NativeRuntimeGate.FullRecipeRequested(args))
            {
                NativeRuntimeGate.SaveBlockedNotice();
                return NativeRuntimeGate.BlockedExitCode;
            }
            ConfigureAssemblyResolution();

            var app = new Application
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };

            Window toolsWindow = CreateToolsWindow();
            InitializeBlankStations(toolsWindow);
            var closeGuard = BlankUiLifecycleGuard.Attach(toolsWindow);
            AttachOptionalRecipeMenu(toolsWindow, closeGuard.Restore);
            bool uiSmoke = Array.Exists(args ?? new string[0],
                a => string.Equals(a, "--ui-smoke", StringComparison.OrdinalIgnoreCase));
            StandaloneUiSmoke smoke = null;
            if (uiSmoke)
            {
                string targetRecipe = GetArg(args, "--recipe-preview");
                if (string.IsNullOrWhiteSpace(targetRecipe))
                    throw new ArgumentException("--ui-smoke 需要 --recipe-preview <隔離 Recipe>");
                if (!targetRecipe.StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("--ui-smoke 僅允許 R2PROBE- 隔離 Recipe");
                smoke = new StandaloneUiSmoke(toolsWindow, targetRecipe);
                smoke.Attach();
            }

            string recipeName = GetArg(args, "--recipe");
            string previewName = GetArg(args, "--recipe-preview");
            int station = ParseStation(GetArg(args, "--station"));
            if (!string.IsNullOrWhiteSpace(previewName))
            {
                LightweightRecipePreview preview =
                    LightweightRecipePreview.Load(toolsWindow, previewName, station);
                preview.AttachStationMenu();
                toolsWindow.Loaded += delegate
                {
                    LightweightRecipePreview.RestrictOriginalEditing(toolsWindow);
                };
                // Keep original closing handlers detached: preview CVInspection
                // holders do not own locateTool / AI workers.
            }
            else if (!string.IsNullOrWhiteSpace(recipeName))
            {
                LoadCompiledRecipeLifecycle(toolsWindow, recipeName, station);
                closeGuard.Restore();
            }

            app.MainWindow = toolsWindow;
            int exitCode = app.Run(toolsWindow);
            return smoke == null ? exitCode : smoke.ExitCode;
        }

        private static Window CreateToolsWindow()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll");
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    "Phase-1 decoded/rebuilt InspTools.dll is required beside InspToolsStandalone.exe.",
                    path);

            // Same preload set proven by phase-1 ReplicaLoader.LoadCompiledToolsWindow().
            Preload("ControlzEx.dll");
            Preload("MahApps.Metro.dll");
            Preload("MahApps.Metro.IconPacks.dll");
            Preload("UICommon.dll");
            Preload("System.Windows.Interactivity.dll");

            Assembly assembly = Assembly.LoadFrom(path);
            Type toolsWindowType = assembly.GetType("InspTools.ToolsWindow", true);
            var window = Activator.CreateInstance(toolsWindowType) as Window;
            if (window == null)
                throw new InvalidDataException("Decoded InspTools.ToolsWindow could not be constructed.");

            InvokeIfPresent(toolsWindowType, window, "SetRemoteMode");
            InvokeIfPresent(toolsWindowType, window, "LoadLanguage", 1);
            return window;
        }

        private static void InitializeBlankStations(Window window)
        {
            // ToolsWindow.Main_IsVisibleChanged expects Station[StationIndex] to exist.
            // A normal CVInspection constructor probes AI/native runtime, which must not
            // become a prerequisite merely to show the decoded UI.  Use zero-state
            // station objects only for the blank shell.  Real Recipe loading is R2 and
            // must reuse phase-1 InitializeCompiledRecipeLifecycle instead.
            Type toolsWindowType = window.GetType();
            Assembly assembly = toolsWindowType.Assembly;
            Type inspectionType = assembly.GetType("InspTools.Station.CVInspection", true);
            Array stations = Array.CreateInstance(inspectionType, 8);

            for (int i = 0; i < 8; i++)
            {
                object station = FormatterServices.GetUninitializedObject(inspectionType);
                stations.SetValue(station, i);
            }

            MethodInfo setStation = toolsWindowType.GetMethod(
                "SetStation",
                BindingFlags.Instance | BindingFlags.Public);
            if (setStation == null)
                throw new MissingMethodException("InspTools.ToolsWindow", "SetStation");

            setStation.Invoke(window, new object[] { stations });
        }

        private static void AttachOptionalRecipeMenu(Window window, Action onRecipeLoaded)
        {
            var menu = new ContextMenu();
            var recipeMenu = new MenuItem { Header = "原版 GUI 預覽 Recipe（唯讀）" };
            PopulateRecipeMenu(recipeMenu, window, onRecipeLoaded);
            menu.Items.Add(recipeMenu);
            menu.Items.Add(new MenuItem
            {
                Header = "完整檢測 Recipe 載入：等待 Runtime 驗收",
                IsEnabled = false
            });

            var refresh = new MenuItem { Header = "重新整理 Recipe 清單" };
            refresh.Click += delegate
            {
                PopulateRecipeMenu(recipeMenu, window, onRecipeLoaded);
            };
            menu.Items.Add(refresh);

            // Chao8Side-compatible, independent file store. No ShuntVC/DIA host.
            StandaloneRecipeUi.AddMenuItems(menu, window, delegate
            {
                PopulateRecipeMenu(recipeMenu, window, onRecipeLoaded);
            });

            window.ContextMenu = menu;
            window.PreviewKeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
                {
                    PopulateRecipeMenu(recipeMenu, window, onRecipeLoaded);
                    menu.IsOpen = true;
                    e.Handled = true;
                }
            };
        }

        private static void PopulateRecipeMenu(MenuItem recipeMenu, Window window, Action onRecipeLoaded)
        {
            recipeMenu.Items.Clear();
            string[] names = StandaloneRecipeStore.List();
            if (names.Length == 0)
            {
                recipeMenu.Items.Add(new MenuItem
                {
                    Header = "(Recipe 資料夾目前是空的)",
                    IsEnabled = false
                });
                return;
            }

            foreach (string name in names)
            {
                string recipeName = name;
                var item = new MenuItem { Header = recipeName, Tag = recipeName };
                item.Click += delegate(object sender, RoutedEventArgs e)
                {
                    string selected = Convert.ToString(((MenuItem)sender).Tag);
                    TryPreviewRecipe(window, selected, 1);
                };
                recipeMenu.Items.Add(item);
            }
        }

        private static void TryPreviewRecipe(Window window, string recipeName, int stationNumber)
        {
            try
            {
                LightweightRecipePreview preview =
                    LightweightRecipePreview.Load(window, recipeName, stationNumber);
                preview.AttachStationMenu();
                LightweightRecipePreview.RestrictOriginalEditing(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show(window, ex.GetBaseException().Message,
                    "Recipe 參數預覽失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void TryLoadRecipe(Window window, string recipeName, int stationNumber, Action onRecipeLoaded)
        {
            try
            {
                string messages = LoadCompiledRecipeLifecycle(window, recipeName, stationNumber);
                if (onRecipeLoaded != null) onRecipeLoaded();
                window.Title = "INSPECTION TOOLS - [" + recipeName + "]";
                if (!string.IsNullOrWhiteSpace(messages))
                {
                    // Keep the original UI as the main product.  Lifecycle notes are
                    // available on demand instead of adding another status shell.
                    window.Tag = messages;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.GetBaseException().Message,
                    "Recipe 載入失敗",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Direct port of phase-1 ReplicaLoader.InitializeCompiledRecipeLifecycle().
        /// Recipe owns station/tool/model names; model binaries follow the original
        /// AppBase\\Model convention.  There is intentionally no ModelRoot picker.
        /// </summary>
        private static string LoadCompiledRecipeLifecycle(Window window, string recipeName, int stationNumber)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            if (string.IsNullOrWhiteSpace(recipeName)) throw new ArgumentException("Recipe name is required.", nameof(recipeName));
            if (stationNumber < 1 || stationNumber > 8) throw new ArgumentOutOfRangeException(nameof(stationNumber));

            string recipePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipe", recipeName);
            if (!Directory.Exists(recipePath))
                throw new DirectoryNotFoundException("Recipe not found under AppBase\\Recipe: " + recipePath);

            Type toolsWindowType = window.GetType();
            Assembly assembly = toolsWindowType.Assembly;
            Type cvInspectionType = assembly.GetType("InspTools.Station.CVInspection", true);
            Array stations = Array.CreateInstance(cvInspectionType, 8);
            MethodInfo stationSetRecipe = cvInspectionType.GetMethod("SetRecipe", BindingFlags.Instance | BindingFlags.Public);
            if (stationSetRecipe == null)
                throw new MissingMethodException("InspTools.Station.CVInspection", "SetRecipe");

            var messages = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                object station = Activator.CreateInstance(cvInspectionType);
                object[] stationArgs = { recipeName, i, string.Empty };
                stationSetRecipe.Invoke(station, stationArgs);
                stations.SetValue(station, i);
                string message = stationArgs[2] as string;
                if (!string.IsNullOrWhiteSpace(message))
                    messages.Add("S" + (i + 1) + ":" + message.Trim());
            }

            MethodInfo setStation = toolsWindowType.GetMethod("SetStation", BindingFlags.Instance | BindingFlags.Public);
            if (setStation == null)
                throw new MissingMethodException("InspTools.ToolsWindow", "SetStation");
            setStation.Invoke(window, new object[] { stations });

            MethodInfo setThroughputRecipe = toolsWindowType.GetMethod(
                "SetThroughputRecipe",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string) },
                null);
            if (setThroughputRecipe == null)
                throw new MissingMethodException("InspTools.ToolsWindow", "SetThroughputRecipe(string)");
            try
            {
                setThroughputRecipe.Invoke(window, new object[] { recipeName });
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException ?? ex;
                messages.Add("throughput lifecycle failed:" + inner.Message);
            }

            MethodInfo stationReadModel = cvInspectionType.GetMethod(
                "ReadModel",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(int) },
                null);
            if (stationReadModel == null)
                throw new MissingMethodException("InspTools.Station.CVInspection", "ReadModel(int)");

            FieldInfo stationErrorMsg = cvInspectionType.GetField(
                "ErrorMsg",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < 8; i++)
            {
                object station = stations.GetValue(i);
                try
                {
                    stationReadModel.Invoke(station, new object[] { i });
                    if (stationErrorMsg != null)
                    {
                        string loadMessage = Convert.ToString(stationErrorMsg.GetValue(station));
                        if (!string.IsNullOrWhiteSpace(loadMessage))
                            messages.Add("S" + (i + 1) + " model:" + loadMessage.Trim());
                    }
                }
                catch (TargetInvocationException ex)
                {
                    Exception inner = ex.InnerException ?? ex;
                    messages.Add("S" + (i + 1) + " model load failed:" + inner.Message);
                }
            }

            FieldInfo samModelLoadField = toolsWindowType.GetField(
                "SAMModelLoad",
                BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo aiLoadingPauseEventField = toolsWindowType.GetField(
                "AILoadingPauseEvent",
                BindingFlags.Instance | BindingFlags.NonPublic);
            bool samModelLoaded = samModelLoadField != null && Convert.ToBoolean(samModelLoadField.GetValue(window));
            if (!samModelLoaded && aiLoadingPauseEventField != null)
            {
                var aiLoadingPauseEvent = aiLoadingPauseEventField.GetValue(window) as EventWaitHandle;
                if (aiLoadingPauseEvent != null)
                {
                    aiLoadingPauseEvent.Set();
                    messages.Add("SAM background preload signaled");
                }
            }

            MethodInfo setStationIndex = toolsWindowType.GetMethod(
                "SetStationIndex",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(int) },
                null);
            if (setStationIndex == null)
                throw new MissingMethodException("InspTools.ToolsWindow", "SetStationIndex(int)");
            setStationIndex.Invoke(window, new object[] { stationNumber - 1 });

            window.Title = "INSPECTION TOOLS - [" + recipeName + "]";
            return string.Join(" | ", messages.ToArray());
        }

        private static void ConfigureAssemblyResolution()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e)
            {
                string name = new AssemblyName(e.Name).Name + ".dll";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }

        private static void Preload(string name)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(path))
                Assembly.LoadFrom(path);
        }

        private static void InvokeIfPresent(Type type, object instance, string method, params object[] args)
        {
            MethodInfo info = type.GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
            if (info != null)
                info.Invoke(instance, args);
        }

        private static string GetArg(string[] args, string name)
        {
            if (args == null) return null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static int ParseStation(string value)
        {
            int station;
            return int.TryParse(value, out station) && station >= 1 && station <= 8 ? station : 1;
        }
    }
}
