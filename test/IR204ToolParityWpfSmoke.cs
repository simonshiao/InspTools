using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using InspTools.SDK;
using InspTools.Wpf;

internal static class IR204ToolParityWpfSmoke
{
    private const string ExpectedProductionSha = "153650676BC2E1901620F1077D8AA0A00C654699B9443CB366F3555A9D539E4C";

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2)
                throw new ArgumentException("Usage: IR204ToolParityWpfSmoke.exe <project-root> <production-station1.ini>");

            string projectRoot = Path.GetFullPath(args[0]);
            string productionStation = Path.GetFullPath(args[1]);
            Require(Directory.Exists(projectRoot), "Project root missing.");
            Require(File.Exists(productionStation), "Production station file missing.");

            string productionBefore = Sha256(productionStation);
            Require(string.Equals(productionBefore, ExpectedProductionSha, StringComparison.OrdinalIgnoreCase),
                "Production recipe baseline SHA changed before smoke: " + productionBefore);

            string fixtureRoot = Path.Combine(projectRoot, "output", "IR2-04-ToolParityFixture");
            if (Directory.Exists(fixtureRoot)) Directory.Delete(fixtureRoot, true);
            string recipeFolder = Path.Combine(fixtureRoot, "Recipe", "IR204-FIXTURE");
            Directory.CreateDirectory(recipeFolder);
            Directory.CreateDirectory(Path.Combine(fixtureRoot, "Model"));
            string stationPath = Path.Combine(recipeFolder, "station1.ini");
            File.WriteAllText(stationPath, FixtureRecipe(), new UTF8Encoding(false));
            string imagePath = Path.Combine(fixtureRoot, "fixture.png");
            File.WriteAllBytes(imagePath, Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));
            string fixtureBefore = Sha256(stationPath);

            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            using (IInspToolsRuntime runtime = InspToolsRuntime.Create(new InspToolsRuntimeOptions
            {
                RuntimeRoot = fixtureRoot,
                RecipeRoot = Path.Combine(fixtureRoot, "Recipe"),
                ModelRoot = Path.Combine(fixtureRoot, "Model"),
                EnableAi = false
            }))
            using (IInspToolsSession session = runtime.OpenSession(new InspToolsSessionOptions
            {
                RecipeName = "IR204-FIXTURE",
                Station = 1,
                ImagePath = imagePath,
                Editable = true
            }))
            {
                var editor = new InspToolsEditorControl { Session = session };

                Require(editor.SelectToolSection("Locate"), "Locate section missing.");
                Require(editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor,
                    "Locate recovered editor policy missing.");
                Require(editor.EditableRecoveredParameterCount == 10,
                    "Locate allowlist must expose exactly 10 fixture keys; actual=" + editor.EditableRecoveredParameterCount);
                Require(editor.IsVisibleParameterEditable("LocateAngleLimit"), "LocateAngleLimit must be editable.");
                Require(!editor.IsVisibleParameterEditable("LocateUnsafeInternal"), "Unknown Locate key must be protected.");
                Require(ThrowsInvalidOperation(() => editor.SetVisibleParameterValue("LocateUnsafeInternal", "MUTATED")),
                    "Locate protected key write was not blocked.");

                editor.SetVisibleParameterValue("LocateAngleLimit", "16");
                ToolPreviewResult locatePreview = editor.TestCurrentTool();
                Require(locatePreview.Success, "Locate parameter preview failed.");
                editor.SaveChanges();
                Require(session.GetParameter("Locate", "LocateAngleLimit") == "16", "Locate save mismatch.");
                Require(session.GetParameter("Locate", "LocateUnsafeInternal") == "KEEP", "Protected Locate key changed.");

                editor.SetVisibleParameterValue("LocateAngleLimit", "17");
                editor.TestCurrentTool();
                Require(session.IsDirty, "Locate edit did not dirty session before cancel.");
                editor.CancelChanges();
                Require(session.GetParameter("Locate", "LocateAngleLimit") == "16", "Locate cancel rollback failed.");

                Require(editor.SelectToolSection("BinaryTools0"), "BinaryTools0 section missing.");
                Require(editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor,
                    "Binary recovered editor policy missing.");
                Require(editor.EditableRecoveredParameterCount >= 12,
                    "Binary fixture did not expose enough recovered safe scalar keys: " + editor.EditableRecoveredParameterCount);
                Require(editor.IsVisibleParameterEditable("THD1_Min"), "Binary THD1_Min must be editable.");
                Require(editor.IsVisibleParameterEditable("EnableLineWidthA"), "Binary line rule scalar must be editable.");
                Require(editor.IsVisibleParameterEditable("InspRule3_Index"), "Binary rule scalar must be editable.");
                Require(editor.IsVisibleParameterEditable("UseMatchTextMask1"), "Binary text-mask scalar must be editable.");
                Require(!editor.IsVisibleParameterEditable("PatternDB0"), "Pattern DB must remain protected.");
                Require(ThrowsInvalidOperation(() => editor.SetVisibleParameterValue("PatternDB0", "MUTATED")),
                    "Pattern DB write was not blocked.");

                editor.SetVisibleParameterValue("THD1_Min", "12");
                editor.SetVisibleParameterValue("EnableLineWidthA", "True");
                editor.SetVisibleParameterValue("InspRule3_Index", "11");
                editor.SaveChanges();
                Require(session.GetParameter("BinaryTools0", "THD1_Min") == "12", "Binary save mismatch.");
                Require(session.GetParameter("BinaryTools0", "PatternDB0") == "KEEP", "Protected Pattern DB changed.");

                editor.SetVisibleParameterValue("THD1_Min", "14");
                editor.TestCurrentTool();
                Require(session.IsDirty, "Binary edit did not dirty session before cancel.");
                editor.CancelChanges();
                Require(session.GetParameter("BinaryTools0", "THD1_Min") == "12", "Binary cancel rollback failed.");

                Require(editor.SelectToolSection("MaskTools0"), "MaskTools0 section missing.");
                Require(editor.HasRecoveredParityPolicy && editor.HasDedicatedRecoveredEditor,
                    "Mask recovered scalar editor policy missing.");
                Require(editor.EditableRecoveredParameterCount >= 8,
                    "Mask fixture did not expose enough recovered safe scalar keys: " + editor.EditableRecoveredParameterCount);
                Require(editor.IsVisibleParameterEditable("THD1_Min"), "Mask THD1_Min must be editable.");
                Require(editor.IsVisibleParameterEditable("FillHole"), "Mask FillHole must be editable.");
                Require(!editor.IsVisibleParameterEditable("MaskBitmapPath"), "Mask bitmap path must stay protected.");
                Require(ThrowsInvalidOperation(() => editor.SetVisibleParameterValue("MaskBitmapPath", "MUTATED")),
                    "Mask bitmap write was not blocked.");

                editor.SetVisibleParameterValue("THD1_Min", "21");
                editor.SetVisibleParameterValue("FillHole", "False");
                editor.SaveChanges();
                Require(session.GetParameter("MaskTools0", "THD1_Min") == "21", "Mask scalar save mismatch.");
                Require(session.GetParameter("MaskTools0", "FillHole") == "False", "Mask boolean save mismatch.");
                Require(session.GetParameter("MaskTools0", "MaskBitmapPath") == "KEEP", "Mask protected value changed.");

                editor.SetVisibleParameterValue("THD1_Min", "22");
                ToolPreviewResult maskPreview = editor.TestCurrentTool();
                Require(maskPreview.Success, "Mask parameter preview failed.");
                Require(session.IsDirty, "Mask edit did not dirty session before cancel.");
                editor.CancelChanges();
                Require(session.GetParameter("MaskTools0", "THD1_Min") == "21", "Mask cancel rollback failed.");

                Require(editor.SelectToolSection("SmoothTools0"), "SmoothTools0 section missing.");
                Require(!editor.HasRecoveredParityPolicy && !editor.HasDedicatedRecoveredEditor,
                    "Smooth fixture should remain generic fallback in this slice.");
                Require(editor.EditableRecoveredParameterCount == 0, "Generic fallback must expose zero writable recovered keys.");
                Require(ThrowsInvalidOperation(() => editor.SetVisibleParameterValue("Kernel", "99")),
                    "Generic fallback write bypassed parity gate.");
                Require(session.GetParameter("SmoothTools0", "Kernel") == "3", "Generic fallback value changed.");

                InspectionResult inspection = session.Inspect();
                Require(inspection.Bin == "NOT_RUN", "IR2-04 must not claim recovered AOI execution parity.");
            }

            string fixtureAfter = Sha256(stationPath);
            string productionAfter = Sha256(productionStation);
            Require(fixtureBefore == fixtureAfter, "Session Save wrote the project-local recipe file unexpectedly.");
            Require(productionBefore == productionAfter, "Production station1.ini changed during IR2-04 smoke.");

            string report = Path.Combine(projectRoot, "output", "IR2-04_TOOL_SPECIFIC_WPF_SMOKE.txt");
            File.WriteAllText(report,
                "IR2_04_TOOL_SPECIFIC_WPF=PASS" + Environment.NewLine +
                "LOCATE_ALLOWLIST=10" + Environment.NewLine +
                "BINARY_SAFE_SCALAR=PASS" + Environment.NewLine +
                "MASK_SAFE_SCALAR=PASS" + Environment.NewLine +
                "MASK_BITMAP_ROI=READ_ONLY" + Environment.NewLine +
                "PATTERN_DB=READ_ONLY" + Environment.NewLine +
                "GENERIC_FALLBACK=READ_ONLY" + Environment.NewLine +
                "AOI_EXECUTION=NOT_RUN" + Environment.NewLine +
                "FIXTURE_SHA_BEFORE=" + fixtureBefore + Environment.NewLine +
                "FIXTURE_SHA_AFTER=" + fixtureAfter + Environment.NewLine +
                "PRODUCTION_SHA_BEFORE=" + productionBefore + Environment.NewLine +
                "PRODUCTION_SHA_AFTER=" + productionAfter + Environment.NewLine,
                new UTF8Encoding(false));

            Console.WriteLine("IR2_04_TOOL_SPECIFIC_WPF=PASS locate=10 binary=SAFE_SCALAR mask=SAFE_SCALAR+BITMAP_ROI_READ_ONLY pattern=READ_ONLY generic=READ_ONLY production_sha=UNCHANGED");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("IR2_04_TOOL_SPECIFIC_WPF=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static bool ThrowsInvalidOperation(Action action)
    {
        try { action(); return false; }
        catch (InvalidOperationException) { return true; }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static string Sha256(string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            byte[] hash = sha.ComputeHash(stream);
            var text = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash) text.Append(value.ToString("X2"));
            return text.ToString();
        }
    }

    private static string FixtureRecipe()
    {
        return
            "[Main]\r\n" +
            "StationON = True\r\n" +
            "LocateToolON = True\r\n" +
            "BinaryToolON = True\r\n\r\n" +
            "[Locate]\r\n" +
            "LocateAngleLimit = 15\r\n" +
            "LocateWidthMin = 2000\r\n" +
            "LocateWidthMax = 2800\r\n" +
            "LocateHeightMin = 1000\r\n" +
            "LocateHeightMax = 1800\r\n" +
            "LocateErrorBin = 2\r\n" +
            "LocateSizeErrorBin = 4\r\n" +
            "ParetoIndex = 1\r\n" +
            "LocateSpeedUp = True\r\n" +
            "doRotateMinAngle = 2\r\n" +
            "LocateUnsafeInternal = KEEP\r\n\r\n" +
            "[BinaryTools0]\r\n" +
            "TrackerName = FixtureBinary\r\n" +
            "TrackerON = True\r\n" +
            "ErrorBin = 3\r\n" +
            "ParetoIndex = 2\r\n" +
            "MaskSelectIndex = 1\r\n" +
            "UseMaskAsInspArea = True\r\n" +
            "Tracker1UseAutoSize = True\r\n" +
            "AutoSize1_SelectIndex = 2\r\n" +
            "Tracker1AligmentXIndex = 1\r\n" +
            "Tracker1AligmentYIndex = 2\r\n" +
            "THD1_ChannelIndex = 10\r\n" +
            "THD1_ChannelON = True\r\n" +
            "THD1_Min = 10\r\n" +
            "THD1_Max = 255\r\n" +
            "BlobLineSort = 2\r\n" +
            "BlobLineNum = 3\r\n" +
            "EnableLineWidthA = False\r\n" +
            "InspRuleAND = True\r\n" +
            "InspRule3_Index = 9\r\n" +
            "InspRule3_Min = 1\r\n" +
            "InspRule3_Max = 9\r\n" +
            "UseMatchTextMask1 = 0\r\n" +
            "MatchTextDE_Start1_Index = 0\r\n" +
            "MatchTextDE_Start1_Count = 1\r\n" +
            "PatternDB0 = KEEP\r\n\r\n" +
            "[MaskTools0]\r\n" +
            "TrackerON = True\r\n" +
            "TrackerName = FixtureMask\r\n" +
            "THD1_ChannelIndex = 2\r\n" +
            "THD1_ChannelON = True\r\n" +
            "THD1_Min = 20\r\n" +
            "THD1_Max = 255\r\n" +
            "DE_Start1_Index = 0\r\n" +
            "DE_End1_Index = 0\r\n" +
            "DE_Start1_Count = 0\r\n" +
            "DE_End1_Count = 0\r\n" +
            "FillHole = True\r\n" +
            "doConvexHull = True\r\n" +
            "DE_FinalStart_Index = 0\r\n" +
            "DE_FinalStart_Count = 0\r\n" +
            "DE_FinalEnd_Index = 0\r\n" +
            "DE_FinalEnd_Count = 0\r\n" +
            "MaxBlob = 0\r\n" +
            "MaskBitmapPath = KEEP\r\n\r\n" +
            "[SmoothTools0]\r\n" +
            "Kernel = 3\r\n";
    }
}
