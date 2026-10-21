using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using InspTools.SDK;
using InspTools.Wpf;

namespace InspTools.DIALikeHost
{
    internal static class Program
    {
        private const string RecipeName = "IR205-DIA-LIKE";

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                string fixtureRoot = Path.GetFullPath(GetArg(args, "--fixture-root") ??
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DIALikeFixture"));
                string report = GetArg(args, "--report");
                string image = EnsureFixture(fixtureRoot);

                using (IInspToolsRuntime runtime = InspToolsRuntime.Create(new InspToolsRuntimeOptions
                {
                    RuntimeRoot = fixtureRoot,
                    RecipeRoot = Path.Combine(fixtureRoot, "Recipe"),
                    ModelRoot = Path.Combine(fixtureRoot, "Model"),
                    EnableAi = false
                }))
                using (IInspToolsSession session = runtime.OpenSession(new InspToolsSessionOptions
                {
                    RecipeName = RecipeName,
                    Station = 1,
                    ImagePath = image,
                    Editable = true
                }))
                {
                    if (Has(args, "--smoke")) return RunSmoke(session, report);

                    var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
                    var editor = new InspToolsEditorControl { Session = session };
                    var window = new Window
                    {
                        Title = "DIA-like Machine Host · InspTools SDK/WPF",
                        Width = 1180,
                        Height = 760,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = editor
                    };
                    app.MainWindow = window;
                    return app.Run(window);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 31;
            }
        }

        private static int RunSmoke(IInspToolsSession session, string reportPath)
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            int pid = Process.GetCurrentProcess().Id;
            var editor = new InspToolsEditorControl { Session = session };
            bool inProcess = ReferenceEquals(editor.Session, session) && Process.GetCurrentProcess().Id == pid;

            bool locateSelected = editor.SelectToolSection("Locate");
            bool locatePolicy = editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor;
            bool locateAngleEditable = editor.IsVisibleParameterEditable("LocateAngleLimit");
            bool locateRawProtected = !editor.IsVisibleParameterEditable("UnverifiedLocateKey");
            editor.SetVisibleParameterValue("LocateAngleLimit", "16");
            ToolPreviewResult locatePreview = editor.TestCurrentTool();
            editor.SaveChanges();
            string locateSaved = session.GetParameter("Locate", "LocateAngleLimit");
            editor.SetVisibleParameterValue("LocateAngleLimit", "17");
            editor.CancelChanges();
            string locateAfterCancel = session.GetParameter("Locate", "LocateAngleLimit");

            bool binarySelected = editor.SelectToolSection("BinaryTools0");
            bool binaryPolicy = editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor;
            bool binaryEditable = editor.IsVisibleParameterEditable("THD1_Min");
            editor.SetVisibleParameterValue("THD1_Min", "22");
            ToolPreviewResult binaryPreview = editor.TestCurrentTool();
            editor.SaveChanges();
            string binarySaved = session.GetParameter("BinaryTools0", "THD1_Min");
            editor.SetVisibleParameterValue("THD1_Min", "23");
            editor.CancelChanges();
            string binaryAfterCancel = session.GetParameter("BinaryTools0", "THD1_Min");

            bool maskSelected = editor.SelectToolSection("MaskTools0");
            bool maskPolicy = editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor;
            bool maskScalarEditable = editor.IsVisibleParameterEditable("THD1_Min");
            bool maskAssetProtected = !editor.IsVisibleParameterEditable("MaskBitmapPath") &&
                                      ThrowsInvalidOperation(() => editor.SetVisibleParameterValue("MaskBitmapPath", "blocked.bmp"));

            bool genericSelected = editor.SelectToolSection("SmoothTools0");
            bool genericReadOnly = !editor.HasRecoveredParityPolicy &&
                                   !editor.IsVisibleParameterEditable("Kernel") &&
                                   ThrowsInvalidOperation(() => editor.SetVisibleParameterValue("Kernel", "9"));

            InspectionResult inspection = session.Inspect();
            bool pass = inProcess && locateSelected && locatePolicy && locateAngleEditable && locateRawProtected &&
                        locatePreview.Success && locateSaved == "16" && locateAfterCancel == "16" &&
                        binarySelected && binaryPolicy && binaryEditable && binaryPreview.Success &&
                        binarySaved == "22" && binaryAfterCancel == "22" &&
                        maskSelected && maskPolicy && maskScalarEditable && maskAssetProtected &&
                        genericSelected && genericReadOnly && inspection.Bin == "NOT_RUN" && !session.IsDirty;

            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                string full = Path.GetFullPath(reportPath);
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full,
                    "IR2_05_DIA_LIKE_HOST=" + (pass ? "PASS" : "FAIL") + Environment.NewLine +
                    "PID=" + pid + Environment.NewLine +
                    "IN_PROCESS=" + inProcess + Environment.NewLine +
                    "PUBLIC_HOST=SDK+WPF" + Environment.NewLine +
                    "RECIPE=" + session.RecipeName + Environment.NewLine +
                    "STATION=" + session.Station + Environment.NewLine +
                    "LOCATE_POLICY=" + locatePolicy + Environment.NewLine +
                    "LOCATE_PREVIEW=" + locatePreview.Success + Environment.NewLine +
                    "LOCATE_SAVE_CANCEL=" + (locateSaved == "16" && locateAfterCancel == "16") + Environment.NewLine +
                    "BINARY_POLICY=" + binaryPolicy + Environment.NewLine +
                    "BINARY_PREVIEW=" + binaryPreview.Success + Environment.NewLine +
                    "BINARY_SAVE_CANCEL=" + (binarySaved == "22" && binaryAfterCancel == "22") + Environment.NewLine +
                    "MASK_SAFE_SCALAR=" + maskScalarEditable + Environment.NewLine +
                    "MASK_COMPLEX_ASSET_PROTECTED=" + maskAssetProtected + Environment.NewLine +
                    "GENERIC_FALLBACK_READ_ONLY=" + genericReadOnly + Environment.NewLine +
                    "INSPECT_BIN=" + inspection.Bin + Environment.NewLine,
                    new UTF8Encoding(false));
            }

            Console.WriteLine("IR2_05_DIA_LIKE_HOST=" + (pass ? "PASS" : "FAIL"));
            return pass ? 0 : 32;
        }

        private static string EnsureFixture(string root)
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, "Model"));
            string recipeFolder = Path.Combine(root, "Recipe", RecipeName);
            Directory.CreateDirectory(recipeFolder);
            string station = Path.Combine(recipeFolder, "station1.ini");
            File.WriteAllText(station, FixtureRecipe(), new UTF8Encoding(false));

            string image = Path.Combine(root, "fixture.png");
            File.WriteAllBytes(image, Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));
            return image;
        }

        private static string FixtureRecipe()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "[Main]",
                "StationON = True",
                "LocateToolON = True",
                "BinaryToolON = True",
                "",
                "[Locate]",
                "LocateAngleLimit = 15",
                "LocateWidthMin = 2000",
                "LocateWidthMax = 2800",
                "LocateHeightMin = 1000",
                "LocateHeightMax = 1800",
                "LocateErrorBin = 1",
                "LocateSizeErrorBin = 4",
                "ParetoIndex = 0",
                "LocateSpeedUp = True",
                "doRotateMinAngle = 1",
                "UnverifiedLocateKey = keep-read-only",
                "",
                "[BinaryTools0]",
                "TrackerName = DIA-Like",
                "TrackerON = True",
                "THD1_ChannelIndex = 0",
                "THD1_ChannelON = True",
                "THD1_Min = 20",
                "THD1_Max = 255",
                "",
                "[MaskTools0]",
                "TrackerName = DIA-Mask",
                "TrackerON = True",
                "THD1_ChannelIndex = 0",
                "THD1_ChannelON = True",
                "THD1_Min = 10",
                "THD1_Max = 255",
                "FillHole = True",
                "doConvexHull = False",
                "MaskBitmapPath = readonly-mask.bmp",
                "",
                "[SmoothTools0]",
                "Kernel = 3",
                ""
            });
        }

        private static bool ThrowsInvalidOperation(Action action)
        {
            try { action(); return false; }
            catch (InvalidOperationException) { return true; }
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        private static bool Has(IEnumerable<string> args, string name)
            => args.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
    }
}
