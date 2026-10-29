using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InspTools.Standalone
{
    // CHAO-owned AI runtime integration. This never calls vendor CheckAuth,
    // writes vendor ParamFile, claims Inspect success, or treats demo weights
    // as a deployable model. Keep native ToolsWindow and station controls.
    internal static class ChaoSelfAiBridge
    {
        private static readonly string KnownDevRoot =
            @"E:\0_軟體開發區\3.開發中專案區\1.進行中\ShuntVC_Chao\Runtime";
        private static readonly string KnownProductRoot =
            @"D:\0_軟體開發區\1.成品區\Chao8Side";
        private static readonly string TestProphetModel =
            @"E:\0_軟體開發區\3.開發中專案區\1.進行中\ProjectHub.Native\ProjectData\Projects\Delta-AIProphet\Output\real-e1\model.onnx";

        internal static bool TryRunHealth(string[] args, out int exitCode)
        {
            exitCode = 0;
            if (args == null || args.Length != 1 ||
                !string.Equals(args[0], "--chao-ai-health", StringComparison.Ordinal))
                return false;
            string root = RuntimeRoot();
            string output;
            try
            {
                output = Preflight(root, "CLI 獨立檢查（無 Recipe）", false) +
                    Environment.NewLine + "【CHAO Runtime 真實健康檢查】" +
                    Environment.NewLine + ProphetHealth(root);
            }
            catch (Exception ex)
            {
                output = "CHAO AI 健康檢查例外：" + ex.GetBaseException();
            }
            bool ready = output.Contains("PROPHET_STATUS=READY") &&
                output.Contains("TORCH_CUDA=true") &&
                output.Contains("CUDA12=true") &&
                output.Contains("CUDNN9=true");
            output += Environment.NewLine + (ready
                ? "RUNTIME_HEALTH=PASS; INFERENCE=NOT_TESTED"
                : "RUNTIME_HEALTH=FAIL; INFERENCE=NOT_TESTED");
            Console.WriteLine(output);
            // This file belongs to the isolated candidate only.
            File.WriteAllText(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "chao-self-ai-health.log"), output,
                new UTF8Encoding(false));
            exitCode = ready ? 0 : 7;
            return true;
        }

        internal static void Attach(Window host, ButtonBase robot, Func<bool> recipeLoaded)
        {
            if (robot == null) return;
            robot.ToolTip = "CHAO AI｜自研定位與 AI 先知";
            robot.PreviewMouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                e.Handled = true; // prevent original licensed AI sidebar path
                Show(host, recipeLoaded());
            };
            robot.PreviewKeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key != Key.Enter && e.Key != Key.Space) return;
                e.Handled = true;
                Show(host, recipeLoaded());
            };
        }

        private static string RuntimeRoot()
        {
            string configured = Environment.GetEnvironmentVariable("CHAO_AI_RUNTIME_ROOT");
            foreach (string path in new[] { configured, KnownDevRoot, KnownProductRoot })
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) continue;
                if (File.Exists(Path.Combine(path, @"Runtime\AIRuntime\ChaoAIRuntime.exe")) &&
                    File.Exists(Path.Combine(path, @"Runtime\AIRuntime\runtime.json")))
                    return Path.GetFullPath(path);
            }
            return "";
        }

        private static string OriginalStationInfo(Window host)
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                int index = Convert.ToInt32(host.GetType().GetField("StationIndex", flags)
                    .GetValue(host));
                Array all = (Array)host.GetType().GetField("Station", flags).GetValue(host);
                if (index < 0 || index >= all.Length) return "站別尚未選定";
                object station = all.GetValue(index);
                object param = station.GetType().GetField("paramFile", flags)
                    .GetValue(station);
                object locate = param.GetType().GetField("LocateParam", flags)
                    .GetValue(param);
                bool enabled = Convert.ToBoolean(locate.GetType()
                    .GetField("EnableAILocate", flags).GetValue(locate));
                bool prophet = Convert.ToBoolean(param.GetType()
                    .GetField("AIProphetToolON", flags).GetValue(param));
                string model = Convert.ToString(locate.GetType()
                    .GetField("AILocateModelName", flags).GetValue(locate));
                // Recipe is only a reference contract, not an inference model.
                return "目前 S" + (index + 1) + "｜Recipe 定位=" +
                    (enabled ? "ON" : "OFF") + "／原版模型名稱=" +
                    (string.IsNullOrWhiteSpace(model) ? "未設定" : model) +
                    "｜先知=" + (prophet ? "ON" : "OFF");
            }
            catch (Exception ex)
            {
                return "站別參數讀取失敗：" + ex.GetBaseException().Message;
            }
        }

        internal static void SynchronizeStationTitle(Window host, bool recipeLoaded)
        {
            if (!recipeLoaded || host == null) return;
            int station = ChaoLocatePreviewUi.Station(host);
            if (station < 1 || station > 8) return;
            host.Title = System.Text.RegularExpressions.Regex.Replace(host.Title,
                @" / S[1-8]$", " / S" + station);
        }

        private static string Preflight(string root, string stationInfo, bool recipeLoaded)
        {
            var log = new StringBuilder();
            log.AppendLine(stationInfo);
            log.AppendLine("來源：CHAO 自研 Runtime（不經原廠 AI 授權）");
            if (!recipeLoaded)
                log.AppendLine("Recipe：尚未載入，禁止推論與參數回填");
            log.AppendLine("Runtime 路徑：" + (string.IsNullOrEmpty(root) ? "未找到" : root));
            if (!string.IsNullOrEmpty(root))
            {
                string runtime = Path.Combine(root, @"Runtime\AIRuntime\ChaoAIRuntime.exe");
                string config = Path.Combine(root, @"Runtime\AIRuntime\runtime.json");
                log.AppendLine("CHAO 控制層：" +
                    (File.Exists(runtime) && File.Exists(config) ? "存在" : "缺失"));
                log.AppendLine("自研定位訓練後端：" +
                    (File.Exists(Path.Combine(root,
                        @"Runtime\TrainingRebuild\locate_train_backend.py"))
                        ? "存在（訓練用途）" : "缺失"));
            }
            log.AppendLine("定位正式模型：尚未指定及通過獨立驗證的 CHAO checkpoint");
            log.AppendLine("AI 先知 ONNX 測試模型：" +
                (File.Exists(TestProphetModel)
                    ? "real-e1 已找到（僅 1 epoch smoke，不可量產）"
                    : "目前找不到"));
            string smokeModel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"ChaoAI\Models\prophet-real-e1.onnx");
            string smokeReport = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"ChaoAI\SmokeResults\real-e1-report.txt");
            bool smokePassed = File.Exists(smokeModel) && File.Exists(smokeReport) &&
                File.ReadAllText(smokeReport).Contains("INFERENCE_SMOKE=PASS");
            log.AppendLine("本候選先知真圖 smoke：" +
                (smokePassed ? "PASS（1 epoch，僅驗證 I/O，不可量產）" : "尚未執行"));
            log.AppendLine("CHAO 定位 300E：" + (ChaoLocatePreviewUi.Staged
                ? "模型及隔離預覽已就緒（保留集 R IoU 0.7771，非量產）"
                : "尚未安全匯入 checkpoint"));
            log.AppendLine("推論適配器：尚未通過真圖推論／ROI 回填 Gate");
            return log.ToString();
        }

        private static string ProphetHealth(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
                return "CHAO Runtime 未找到，未執行檢查。";
            string exe = Path.Combine(root, @"Runtime\AIRuntime\ChaoAIRuntime.exe");
            if (!File.Exists(exe)) return "缺少 ChaoAIRuntime.exe";
            using (var process = new Process())
            {
                process.StartInfo.FileName = exe;
                process.StartInfo.Arguments = "prophet --root \"" + root + "\" --status";
                process.StartInfo.WorkingDirectory = root;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                // Read asynchronously to avoid blocked pipes.
                Task<string> stdout = process.StandardOutput.ReadToEndAsync();
                Task<string> stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(18000))
                {
                    try { process.Kill(); } catch { }
                    return "CHAO 先知 Runtime 健康檢查逾時（18 秒）。";
                }
                string output = stdout.Result + Environment.NewLine + stderr.Result;
                if (process.ExitCode != 0)
                    return "Runtime 健康檢查失敗，ExitCode=" + process.ExitCode +
                        Environment.NewLine + output;
                return output;
            }
        }

        private static void Show(Window host, bool recipeLoaded)
        {
            string root = RuntimeRoot();
            SynchronizeStationTitle(host, recipeLoaded);
            int stationNumber = recipeLoaded ? ChaoLocatePreviewUi.Station(host) : 0;
            string station = recipeLoaded ? OriginalStationInfo(host) : "Recipe 尚未載入";
            var dialog = new Window
            {
                Title = "CHAO AI｜自研定位與 AI 先知",
                Width = 740,
                Height = 500,
                MinWidth = 590,
                MinHeight = 400,
                Owner = host,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252))
            };
            var frame = new DockPanel { Margin = new Thickness(22) };
            var header = new TextBlock
            {
                Text = "CHAO AI · 自研 Runtime",
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            };
            DockPanel.SetDock(header, Dock.Top);
            frame.Children.Add(header);
            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 14, 0, 0)
            };
            var probe = new Button { Content = "檢查 AI 先知 Runtime",
                Width = 190, Height = 36, Margin = new Thickness(0, 0, 10, 0) };
            var previewLocate = new Button {
                Content = "定位 300E｜選圖預覽",
                Width = 180, Height = 36, Margin = new Thickness(0, 0, 10, 0),
                IsEnabled = recipeLoaded && stationNumber == 1 && ChaoLocatePreviewUi.Staged
            };
            if (stationNumber != 1)
                previewLocate.ToolTip = "此定位模型僅適用 S1／CCD1";
            var previewProphet = new Button {
                Content = "先知煙測 Mask",
                Width = 140, Height = 36, Margin = new Thickness(0, 0, 10, 0)
            };
            var close = new Button { Content = "關閉", Width = 85, Height = 36 };
            footer.Children.Add(previewLocate);
            footer.Children.Add(previewProphet);
            footer.Children.Add(probe);
            footer.Children.Add(close);
            DockPanel.SetDock(footer, Dock.Bottom);
            frame.Children.Add(footer);
            var info = new TextBox
            {
                Text = Preflight(root, station, recipeLoaded),
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsReadOnly = true,
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 228, 235)),
                Background = Brushes.White,
                Padding = new Thickness(16)
            };
            frame.Children.Add(info);
            close.Click += delegate { dialog.Close(); };
            previewLocate.Click += async delegate
            {
                previewLocate.IsEnabled = false;
                try
                {
                    string output = await ChaoLocatePreviewUi.RunAsync(host, dialog, root);
                    info.Text = Preflight(root, station, recipeLoaded) +
                        Environment.NewLine + (string.IsNullOrEmpty(output)
                            ? "已取消選圖。"
                            : "離線定位預覽完成，原版 Inspect／Recipe 未修改：" + output);
                }
                catch (Exception ex)
                {
                    info.Text = Preflight(root, station, recipeLoaded) +
                        Environment.NewLine + "定位失敗：" + ex.GetBaseException().Message;
                }
                finally { previewLocate.IsEnabled = recipeLoaded && ChaoLocatePreviewUi.Station(host) == 1 && ChaoLocatePreviewUi.Staged; }
            };
            previewProphet.Click += delegate
            {
                string image = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    @"ChaoAI\SmokeResults\real-e1-mask.png");
                if (!File.Exists(image))
                {
                    MessageBox.Show(dialog, "尚未完成先知 smoke，不可顯示空結果。");
                    return;
                }
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(image, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                new Window {
                    Owner = dialog,
                    Title = "CHAO AI 先知｜1 epoch Mask（非量產）",
                    Width = 540, Height = 580,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new Image { Source = bitmap, Stretch = Stretch.Uniform }
                }.ShowDialog();
            };
            probe.Click += async delegate
            {
                probe.IsEnabled = false;
                info.Text = Preflight(root, station, recipeLoaded) +
                    Environment.NewLine + "正在檢查 CHAO Runtime...";
                string result;
                try { result = await Task.Run(() => ProphetHealth(root)); }
                catch (Exception ex) { result = ex.GetBaseException().Message; }
                info.Text = Preflight(root, station, recipeLoaded) +
                    Environment.NewLine + "【健康檢查（僅 Runtime，非推論）】" +
                    Environment.NewLine + result;
                probe.IsEnabled = true;
            };
            dialog.Content = frame;
            dialog.ShowDialog();
        }
    }
}
