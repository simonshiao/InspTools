using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using InspTools.SDK;
using InspTools.Wpf;

namespace InspTools.ShuntLikeHost
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                HostOptions options = Parse(args);
                using (IInspToolsRuntime runtime = InspToolsRuntime.Create(new InspToolsRuntimeOptions
                {
                    RuntimeRoot = options.RuntimeRoot,
                    RecipeRoot = options.RecipeRoot,
                    ModelRoot = options.ModelRoot,
                    EnableAi = false
                }))
                using (IInspToolsSession session = runtime.OpenSession(new InspToolsSessionOptions
                {
                    RecipeName = options.RecipeName,
                    Station = options.Station,
                    ImagePath = options.ImagePath,
                    Editable = false
                }))
                {
                    if (options.Smoke) return RunSmoke(session, options);

                    var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
                    var window = new Window
                    {
                        Title = "Shunt-like Machine Host · InspTools SDK/WPF",
                        Width = 1180,
                        Height = 760,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = new InspToolsEditorControl { Session = session }
                    };
                    app.MainWindow = window;
                    return app.Run(window);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 41;
            }
        }

        private static int RunSmoke(IInspToolsSession session, HostOptions options)
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            string stationFile = Path.Combine(options.RecipeFolder, "station" + options.Station + ".ini");
            string before = Sha256(stationFile);
            int pid = Process.GetCurrentProcess().Id;
            var editor = new InspToolsEditorControl { Session = session };
            bool inProcess = ReferenceEquals(editor.Session, session) && Process.GetCurrentProcess().Id == pid;

            RecipeParameterSnapshot snapshot = session.GetRecipeSnapshot();
            StationSnapshot station = session.GetStationSnapshot();
            int tools = session.GetToolCatalog().Count;

            bool locateSelected = editor.SelectToolSection("Locate");
            bool locatePolicy = editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor;
            bool locateReadOnly = !editor.IsVisibleParameterEditable("LocateAngleLimit");
            ToolPreviewResult locatePreview = editor.TestCurrentTool();

            ToolDescriptor binary = session.GetToolCatalog().FirstOrDefault(x =>
                string.Equals(x.Family, "Binary", StringComparison.OrdinalIgnoreCase));
            bool binarySelected = binary != null && editor.SelectToolSection(binary.Section);
            bool binaryPolicy = binarySelected && editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor;
            bool binaryReadOnly = binarySelected && !editor.IsVisibleParameterEditable("THD1_Min");
            ToolPreviewResult binaryPreview = binary == null ? null : session.TestTool(binary.Section + "/THD1_Min");

            ToolDescriptor mask = session.GetToolCatalog().FirstOrDefault(x =>
                string.Equals(x.Family, "Mask", StringComparison.OrdinalIgnoreCase));
            bool maskSelected = mask != null && editor.SelectToolSection(mask.Section);
            bool maskPolicy = maskSelected && editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor;
            bool maskReadOnly = maskSelected && !editor.IsVisibleParameterEditable("THD1_Min");

            InspectionResult inspection = session.Inspect();
            string after = Sha256(stationFile);
            bool unchanged = string.Equals(before, after, StringComparison.OrdinalIgnoreCase) &&
                             string.Equals(before, snapshot.Sha256, StringComparison.OrdinalIgnoreCase);

            bool pass = inProcess && !session.Editable && snapshot.Entries.Count > 0 && tools > 0 &&
                        locateSelected && locatePolicy && locateReadOnly && locatePreview.Success &&
                        binarySelected && binaryPolicy && binaryReadOnly && binaryPreview != null && binaryPreview.Success &&
                        maskSelected && maskPolicy && maskReadOnly &&
                        inspection.Bin == "NOT_RUN" && unchanged;

            if (!string.IsNullOrWhiteSpace(options.ReportPath))
            {
                string full = Path.GetFullPath(options.ReportPath);
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full,
                    "IR2_05_SHUNT_LIKE_HOST=" + (pass ? "PASS" : "FAIL") + Environment.NewLine +
                    "PID=" + pid + Environment.NewLine +
                    "IN_PROCESS=" + inProcess + Environment.NewLine +
                    "PUBLIC_HOST=SDK+WPF" + Environment.NewLine +
                    "EDITABLE=" + session.Editable + Environment.NewLine +
                    "RECIPE=" + session.RecipeName + Environment.NewLine +
                    "STATION=" + session.Station + Environment.NewLine +
                    "SOURCE_ENCODING=" + snapshot.SourceEncoding + Environment.NewLine +
                    "ENTRY_COUNT=" + snapshot.Entries.Count + Environment.NewLine +
                    "TOOL_COUNT=" + tools + Environment.NewLine +
                    "STATION_ENABLED=" + station.StationEnabled + Environment.NewLine +
                    "LOCATE_POLICY=" + locatePolicy + Environment.NewLine +
                    "LOCATE_READ_ONLY=" + locateReadOnly + Environment.NewLine +
                    "LOCATE_PREVIEW=" + locatePreview.Success + Environment.NewLine +
                    "BINARY_SECTION=" + (binary == null ? string.Empty : binary.Section) + Environment.NewLine +
                    "BINARY_POLICY=" + binaryPolicy + Environment.NewLine +
                    "BINARY_READ_ONLY=" + binaryReadOnly + Environment.NewLine +
                    "BINARY_PREVIEW=" + (binaryPreview != null && binaryPreview.Success) + Environment.NewLine +
                    "MASK_SECTION=" + (mask == null ? string.Empty : mask.Section) + Environment.NewLine +
                    "MASK_POLICY=" + maskPolicy + Environment.NewLine +
                    "MASK_READ_ONLY=" + maskReadOnly + Environment.NewLine +
                    "INSPECT_BIN=" + inspection.Bin + Environment.NewLine +
                    "SHA_BEFORE=" + before + Environment.NewLine +
                    "SHA_AFTER=" + after + Environment.NewLine +
                    "SOURCE_UNCHANGED=" + unchanged + Environment.NewLine,
                    new UTF8Encoding(false));
            }

            Console.WriteLine("IR2_05_SHUNT_LIKE_HOST=" + (pass ? "PASS" : "FAIL"));
            return pass ? 0 : 42;
        }

        private static HostOptions Parse(string[] args)
        {
            string runtimeRoot = Require(args, "--runtime-root");
            string recipeFolder = Path.GetFullPath(Require(args, "--recipe-folder"));
            string modelRoot = Require(args, "--model-root");
            string image = Require(args, "--image");
            int station;
            if (!int.TryParse(GetArg(args, "--station") ?? "1", out station) || station < 1)
                throw new ArgumentException("--station must be a positive integer.");

            return new HostOptions
            {
                RuntimeRoot = runtimeRoot,
                RecipeFolder = recipeFolder,
                RecipeRoot = Directory.GetParent(recipeFolder).FullName,
                RecipeName = new DirectoryInfo(recipeFolder).Name,
                ModelRoot = modelRoot,
                ImagePath = image,
                Station = station,
                ReportPath = GetArg(args, "--report"),
                Smoke = Has(args, "--smoke")
            };
        }

        private static string Sha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(stream).Select(x => x.ToString("X2")));
        }

        private static string Require(string[] args, string name)
            => GetArg(args, name) ?? throw new ArgumentException(name + " is required.");

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        private static bool Has(string[] args, string name)
            => args.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));

        private sealed class HostOptions
        {
            internal string RuntimeRoot { get; set; }
            internal string RecipeRoot { get; set; }
            internal string RecipeFolder { get; set; }
            internal string RecipeName { get; set; }
            internal string ModelRoot { get; set; }
            internal string ImagePath { get; set; }
            internal int Station { get; set; }
            internal string ReportPath { get; set; }
            internal bool Smoke { get; set; }
        }
    }
}
