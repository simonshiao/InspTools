using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DIAChip8Side.TemplateHost;

namespace InspTools.Standalone
{
    /// <summary>
    /// Portable-review-only replacement for the original protected Training
    /// click route. Mirrors Chao8Side's PreviewMouse interception and opens
    /// its rebuilt DAVS training window against this candidate's own runtime.
    /// </summary>
    internal static class Chao8SideAiTrainingUiBridge
    {
        private const string MarkerName = "chao8side-ai-training-ui.marker";
        private const string MarkerValue = "INSPTOOLS_CHAO8SIDE_AI_TRAINING_UI_V1";
        private static UIElement _button;
        private static RebuiltTrainingWindow _trainingWindow;

        internal static bool Attach(Window toolsWindow)
        {
            if (toolsWindow == null || !NativeR3RecipeSession.IsPortableReview())
                return false;

            string root = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string marker = Path.Combine(root, MarkerName);
            if (!File.Exists(marker) ||
                !string.Equals(File.ReadAllText(marker).Trim(), MarkerValue,
                    StringComparison.Ordinal))
                return false;

            object control = toolsWindow.FindName("btn_AITrain");
            _button = control as UIElement;
            if (_button == null)
            {
                WriteLog(root, "bridge skipped; btn_AITrain unavailable");
                return false;
            }

            _button.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            _button.PreviewKeyDown += OnPreviewKeyDown;
            FrameworkElement element = _button as FrameworkElement;
            if (element != null)
                element.ToolTip = "CHAO 重建版 AI 訓練（隔離 Runtime）";
            WriteLog(root, "bridge attached to btn_AITrain");
            return true;
        }

        private static void OnPreviewMouseLeftButtonDown(object sender,
            MouseButtonEventArgs args)
        {
            if (args.ChangedButton != MouseButton.Left) return;
            args.Handled = true;
            Show(sender as FrameworkElement);
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key != Key.Enter && args.Key != Key.Space) return;
            args.Handled = true;
            Show(sender as FrameworkElement);
        }

        private static void Show(FrameworkElement button)
        {
            Window owner = Window.GetWindow(button);
            string root = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                if (_trainingWindow != null && _trainingWindow.IsVisible)
                {
                    _trainingWindow.Activate();
                    return;
                }

                string recipe = NativeR3RecipeSession.CurrentRecipePath;
                if (string.IsNullOrWhiteSpace(recipe) || !Directory.Exists(recipe))
                    recipe = string.Empty;

                RequireFile(root, @"Runtime\TrainingPython\Python38\python.exe");
                RequireFile(root, @"Runtime\TrainingRebuild\train_compat.py");
                RequireFile(root, @"Runtime\ModelConverter\PyFrozenLib\DAVS\DAVSTrain_v3.pyc");
                RequireFile(root, @"Runtime\AIRuntime\ChaoAIRuntime.exe");

                // ProductRuntimePaths honors these process variables. Pin them
                // to this isolated AppBase so training/deploy helpers cannot
                // resolve into a machine Shunt or Chao8Side installation.
                Environment.SetEnvironmentVariable("CHAO8SIDE_ROOT", root);
                Environment.SetEnvironmentVariable("DIA_PRODUCT_ROOT", root);

                _trainingWindow = new RebuiltTrainingWindow(
                    root,
                    recipe,
                    message => WriteLog(root, message),
                    1,
                    null,
                    null);
                _trainingWindow.Owner = owner;
                _trainingWindow.Closed += delegate { _trainingWindow = null; };
                _trainingWindow.Show();
                WriteLog(root, "rebuilt training window shown recipe=" + recipe);
            }
            catch (Exception error)
            {
                WriteLog(root, "training window failed: " + error);
                MessageBox.Show(owner,
                    "重建版 AI 訓練無法開啟：" + error.GetBaseException().Message,
                    "CHAO AI 訓練", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void RequireFile(string root, string relative)
        {
            string path = Path.Combine(root, relative);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    "隔離候選缺少 General Training Runtime。請先完成 Runtime 封裝：" + relative,
                    path);
        }

        private static void WriteLog(string root, string message)
        {
            try
            {
                File.AppendAllText(Path.Combine(root, "chao8side-ai-training-ui.log"),
                    DateTime.Now.ToString("O") + " " + message + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }
    }
}
