using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace InspTools.Standalone
{
    // Offline CHAO-owned Locate adapter: no writes into native CVBuffer,
    // Recipe parameters, vendor authorization, or Inspect success flags.
    internal static class ChaoLocatePreviewUi
    {
        internal static bool Staged
        {
            get
            {
                string app = AppDomain.CurrentDomain.BaseDirectory;
                return File.Exists(Path.Combine(app, @"ChaoAI\Models\locate-ccd1-loss6-300e.h5")) &&
                    File.Exists(Path.Combine(app, @"ChaoAI\locate-300e-provenance.json")) &&
                    File.Exists(Path.Combine(app, @"ChaoAI\Runtime\ChaoLocateOffline.py"));
            }
        }

        internal static int Station(Window host)
        {
            try
            {
                var field = host.GetType().GetField("StationIndex",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                int index = Convert.ToInt32(field.GetValue(host));
                return index >= 0 && index < 8 ? index + 1 : 0;
            }
            catch { return 0; }
        }

        internal static async Task<string> RunAsync(Window host, Window owner, string runtimeRoot)
        {
            int station = Station(host);
            if (station == 0) throw new InvalidOperationException("未選定有效站別");
            if (station != 1) throw new InvalidOperationException("此定位模型僅適用 S1／CCD1；此站尚未配置模型");
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (!Staged) throw new FileNotFoundException("尚未放入 CHAO 300E 模型／推論腳本");
            var dialog = new OpenFileDialog {
                Title = "選取 S" + station + " 離線測試影像（不會寫回 Inspect）",
                Filter = "影像|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff",
                Multiselect = false, CheckFileExists = true
            };
            if (dialog.ShowDialog(owner) != true) return "";
            if (Station(host) != station) throw new InvalidOperationException("選圖期間站別已改變，請重新選圖");
            string image = dialog.FileName;
            string output = await Task.Run(() => Predict(runtimeRoot, image, station));
            ShowPreview(owner, output, station);
            return output;
        }

        private static string Predict(string runtimeRoot, string image, int station)
        {
            if (station != 1) throw new InvalidOperationException("此定位模型僅適用 S1／CCD1");
            string app = AppDomain.CurrentDomain.BaseDirectory;
            string python = Path.Combine(runtimeRoot,
                @"Runtime\TrainingPython\Python38\python.exe");
            string script = Path.Combine(app, @"ChaoAI\Runtime\ChaoLocateOffline.py");
            string model = Path.Combine(app, @"ChaoAI\Models\locate-ccd1-loss6-300e.h5");
            if (!File.Exists(python)) throw new FileNotFoundException("找不到 CHAO Python", python);
            string folder = Path.Combine(app, @"ChaoAI\SmokeResults",
                "preview-s" + station + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff"));
            var psi = new ProcessStartInfo
            {
                FileName = python,
                Arguments = "\"" + script + "\" --model \"" + model +
                    "\" --image \"" + image + "\" --output \"" + folder + "\"",
                WorkingDirectory = app,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.EnvironmentVariables["CUDA_VISIBLE_DEVICES"] = "-1"; // first offline gate: CPU only
            using (var process = new Process { StartInfo = psi })
            {
                process.Start();
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(90000))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException("離線定位超時 90 秒");
                }
                if (process.ExitCode != 0 ||
                    !stdout.Result.Contains("CHAO_LOCATE_SMOKE=PASS"))
                    throw new IOException("定位推論失敗：" + stdout.Result + stderr.Result);
            }
            foreach (string name in new [] { "locate-report.json", "locate-mask.png", "locate-overlay.png" })
                if (!File.Exists(Path.Combine(folder, name)))
                    throw new InvalidDataException("定位輸出不完整：" + name);
            return folder;
        }

        private static BitmapImage ReadBitmap(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static void ShowPreview(Window owner, string folder, int station)
        {
            var window = new Window
            {
                Owner = owner,
                Title = "CHAO 定位 S" + station + "｜離線 Mask／ROI",
                Width = 950, Height = 640,
                MinWidth = 600, MinHeight = 430,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var layout = new DockPanel { Margin = new Thickness(12) };
            var note = new TextBlock {
                Text = "300 epoch 研究模型｜紅框只作預覽，沒有修改原版 Inspect 或 Recipe",
                Margin = new Thickness(6, 6, 6, 12), FontSize = 14
            };
            DockPanel.SetDock(note, Dock.Top);
            layout.Children.Add(note);
            var button = new Button {
                Content = "切換 Mask／定位框", Width = 205, Height = 36,
                Margin = new Thickness(0, 8, 0, 0)
            };
            DockPanel.SetDock(button, Dock.Bottom);
            layout.Children.Add(button);
            var image = new Image {
                Source = ReadBitmap(Path.Combine(folder, "locate-overlay.png")),
                Stretch = Stretch.Uniform
            };
            bool showOverlay = true;
            button.Click += delegate {
                showOverlay = !showOverlay;
                image.Source = ReadBitmap(Path.Combine(folder,
                    showOverlay ? "locate-overlay.png" : "locate-mask.png"));
            };
            layout.Children.Add(image);
            window.Content = layout;
            window.ShowDialog();
        }
    }
}
