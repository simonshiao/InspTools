using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using InspTools.SDK;
using InspTools.Wpf;

namespace InspTools.ReferenceHost
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            string fixtureRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fixture");
            string imagePath = EnsureFixture(fixtureRoot);

            using (IInspToolsRuntime runtime = InspToolsRuntime.Create(new InspToolsRuntimeOptions
            {
                RuntimeRoot = fixtureRoot,
                RecipeRoot = Path.Combine(fixtureRoot, "Recipe"),
                ModelRoot = Path.Combine(fixtureRoot, "Model"),
                EnableAi = false
            }))
            using (IInspToolsSession session = runtime.OpenSession(new InspToolsSessionOptions
            {
                RecipeName = "IR2-DEMO",
                Station = 1,
                ImagePath = imagePath,
                Editable = true
            }))
            {
                if (args.Any(x => string.Equals(x, "--smoke", StringComparison.OrdinalIgnoreCase)))
                    return RunSmoke(session, args);

                var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
                Window host = BuildHostWindow(session);
                app.MainWindow = host;
                return app.Run(host);
            }
        }

        private static Window BuildHostWindow(IInspToolsSession session)
        {
            var status = new TextBlock { Margin = new Thickness(0, 12, 0, 0), TextWrapping = TextWrapping.Wrap };
            var button = new Button
            {
                Content = "開啟 InspTools 設定",
                Width = 180,
                Height = 46,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var panel = new StackPanel { Margin = new Thickness(24) };
            panel.Children.Add(new TextBlock
            {
                Text = "IR2 Machine-First ReferenceHost",
                FontSize = 22,
                FontWeight = FontWeights.SemiBold
            });
            panel.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 18),
                Text = "Host 只 reference InspTools.SDK + InspTools.Wpf；設定視窗與 Host 在同一 process。"
            });
            panel.Children.Add(button);
            panel.Children.Add(status);

            var window = new Window
            {
                Title = "IR2 ReferenceHost",
                Width = 620,
                Height = 330,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = panel
            };

            Action refresh = () => status.Text =
                $"PID={Process.GetCurrentProcess().Id} · Recipe={session.RecipeName} · S{session.Station} · " +
                $"SavedLocateAngle={session.GetParameter("Locate", "LocateAngleLimit")}";
            button.Click += delegate
            {
                InspToolsEditor.ShowDialog(window, session);
                refresh();
            };
            refresh();
            return window;
        }

        private static int RunSmoke(IInspToolsSession session, string[] args)
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            int pidBefore = Process.GetCurrentProcess().Id;
            var editor = new InspToolsEditorControl { Session = session };
            bool inProcess = ReferenceEquals(editor.Session, session) && Process.GetCurrentProcess().Id == pidBefore;

            bool locateSelected = editor.SelectToolSection("Locate");
            int locateParameters = editor.VisibleParameterCount;
            editor.SetVisibleParameterValue("LocateAngleLimit", "16");
            ToolPreviewResult preview = editor.TestCurrentTool();

            bool binarySelected = editor.SelectToolSection("BinaryTools0");
            int binaryParameters = editor.VisibleParameterCount;
            editor.SetVisibleParameterValue("THD1_Min", "12");
            editor.SaveChanges();
            string saved = session.GetParameter("BinaryTools0", "THD1_Min");

            editor.SetVisibleParameterValue("THD1_Min", "14");
            editor.TestCurrentTool();
            bool dirtyBeforeCancel = session.IsDirty;
            editor.CancelChanges();
            string afterCancel = session.GetParameter("BinaryTools0", "THD1_Min");

            bool maskSelected = editor.SelectToolSection("MaskTools0");
            int maskParameters = editor.VisibleParameterCount;
            editor.SetVisibleParameterValue("THD1_Min", "21");
            editor.SaveChanges();
            string maskSaved = session.GetParameter("MaskTools0", "THD1_Min");
            bool maskProtectedBlocked = false;
            try
            {
                editor.SetVisibleParameterValue("MaskBitmapPath", "should-not-write.bmp");
            }
            catch (InvalidOperationException)
            {
                maskProtectedBlocked = true;
            }
            string maskProtectedValue = session.GetParameter("MaskTools0", "MaskBitmapPath");
            editor.SetVisibleParameterValue("THD1_Min", "22");
            editor.TestCurrentTool();
            editor.CancelChanges();
            string maskAfterCancel = session.GetParameter("MaskTools0", "THD1_Min");
            InspectionResult inspection = session.Inspect();

            bool pass = inProcess && locateSelected && locateParameters > 0 && preview.Success &&
                        binarySelected && binaryParameters > 0 && saved == "12" && dirtyBeforeCancel &&
                        afterCancel == "12" && maskSelected && maskParameters > 0 && maskSaved == "21" &&
                        maskProtectedBlocked && maskProtectedValue == "readonly-mask.bmp" && maskAfterCancel == "21" &&
                        !session.IsDirty && inspection.Bin == "NOT_RUN" && inspection.LocateSucceeded;

            string projectRoot = FindProjectRoot();
            string reportDir = Path.Combine(projectRoot, "output");
            Directory.CreateDirectory(reportDir);
            string reportArg = GetArg(args, "--report");
            string report = string.IsNullOrWhiteSpace(reportArg)
                ? Path.Combine(reportDir, "IR2_CURRENT_EMBEDDED_SMOKE.txt")
                : Path.GetFullPath(reportArg);
            string reportParent = Path.GetDirectoryName(report);
            if (!string.IsNullOrWhiteSpace(reportParent)) Directory.CreateDirectory(reportParent);
            File.WriteAllText(report,
                "IR2_03_EMBEDDED_SMOKE=" + (pass ? "PASS" : "FAIL") + Environment.NewLine +
                "PID=" + pidBefore + Environment.NewLine +
                "IN_PROCESS=" + inProcess + Environment.NewLine +
                "RECIPE=" + session.RecipeName + Environment.NewLine +
                "STATION=" + session.Station + Environment.NewLine +
                "IMAGE=" + session.ImagePath + Environment.NewLine +
                "LOCATE_SELECTED=" + locateSelected + Environment.NewLine +
                "LOCATE_PARAMETER_COUNT=" + locateParameters + Environment.NewLine +
                "BINARY_SELECTED=" + binarySelected + Environment.NewLine +
                "BINARY_PARAMETER_COUNT=" + binaryParameters + Environment.NewLine +
                "PREVIEW=" + preview.Success + Environment.NewLine +
                "SAVED_VALUE=" + saved + Environment.NewLine +
                "DIRTY_BEFORE_CANCEL=" + dirtyBeforeCancel + Environment.NewLine +
                "AFTER_CANCEL=" + afterCancel + Environment.NewLine +
                "MASK_SELECTED=" + maskSelected + Environment.NewLine +
                "MASK_PARAMETER_COUNT=" + maskParameters + Environment.NewLine +
                "MASK_SAVED_VALUE=" + maskSaved + Environment.NewLine +
                "MASK_PROTECTED_BLOCKED=" + maskProtectedBlocked + Environment.NewLine +
                "MASK_PROTECTED_VALUE=" + maskProtectedValue + Environment.NewLine +
                "MASK_AFTER_CANCEL=" + maskAfterCancel + Environment.NewLine +
                "INSPECT_BIN=" + inspection.Bin + Environment.NewLine +
                "LOCATE=" + inspection.LocateSucceeded + Environment.NewLine,
                new UTF8Encoding(false));
            return pass ? 0 : 20;
        }

        private static string GetArg(string[] args, string name)
        {
            if (args == null || string.IsNullOrWhiteSpace(name)) return null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static string EnsureFixture(string root)
        {
            Directory.CreateDirectory(root);
            string recipeFolder = Path.Combine(root, "Recipe", "IR2-DEMO");
            Directory.CreateDirectory(recipeFolder);
            Directory.CreateDirectory(Path.Combine(root, "Model"));
            string stationPath = Path.Combine(recipeFolder, "station1.ini");
            File.WriteAllText(stationPath,
                "[Main]" + Environment.NewLine +
                "StationON = True" + Environment.NewLine +
                "LocateToolON = True" + Environment.NewLine +
                "BinaryToolON = True" + Environment.NewLine +
                Environment.NewLine +
                "[Locate]" + Environment.NewLine +
                "LocateAngleLimit = 15" + Environment.NewLine +
                Environment.NewLine +
                "[BinaryTools0]" + Environment.NewLine +
                "TrackerON = True" + Environment.NewLine +
                "TrackerName = FixtureBinary" + Environment.NewLine +
                "THD1_Min = 10" + Environment.NewLine +
                "THD1_Max = 255" + Environment.NewLine +
                Environment.NewLine +
                "[MaskTools0]" + Environment.NewLine +
                "TrackerON = True" + Environment.NewLine +
                "TrackerName = FixtureMask" + Environment.NewLine +
                "THD1_ChannelIndex = 2" + Environment.NewLine +
                "THD1_ChannelON = True" + Environment.NewLine +
                "THD1_Min = 20" + Environment.NewLine +
                "THD1_Max = 255" + Environment.NewLine +
                "FillHole = True" + Environment.NewLine +
                "doConvexHull = True" + Environment.NewLine +
                "MaskBitmapPath = readonly-mask.bmp" + Environment.NewLine,
                new UTF8Encoding(false));
            string imagePath = Path.Combine(root, "IR2-SYNTHETIC.png");
            if (!File.Exists(imagePath))
            {
                using (var bitmap = new Bitmap(240, 140))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                using (var background = new SolidBrush(Color.FromArgb(35, 45, 58)))
                using (var foreground = new SolidBrush(Color.FromArgb(80, 210, 150)))
                {
                    graphics.FillRectangle(background, 0, 0, bitmap.Width, bitmap.Height);
                    graphics.FillEllipse(foreground, 70, 30, 100, 80);
                    bitmap.Save(imagePath, ImageFormat.Png);
                }
            }
            return imagePath;
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "InspTools.Rebuild2.sln"))) return current.FullName;
                current = current.Parent;
            }
            throw new DirectoryNotFoundException("Cannot locate InspTools.Rebuild2.sln from ReferenceHost AppBase.");
        }
    }
}
