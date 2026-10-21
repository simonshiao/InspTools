using System;
using System.IO;
using System.Linq;
using InspTools.SDK;

internal static class AnnotationContractSmoke
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length < 1 || args.Length > 2)
                throw new ArgumentException("Usage: AnnotationContractSmoke.exe <ShuntTrainingImageRoot> [DiaTrainingImageRoot]");

            string root = Path.GetFullPath(args[0]);
            string model = "R002-CCD1";
            string modelRoot = Path.Combine(root, model);
            if (!Directory.Exists(modelRoot))
                throw new DirectoryNotFoundException("Smoke model root not found: " + modelRoot);

            string ngName = "NG-1-2025-06-27-16-55-12-H.png";
            string ngX = Path.Combine(modelRoot, "X", "NG", "ALL", ngName);
            string ngY = Path.Combine(modelRoot, "Y", "NG", "ALL", ngName);
            string okName = "OK-1-2025-06-27-16-54-46.png";
            string okX = Path.Combine(modelRoot, "X", "OK", "ALL", okName);
            string okY = Path.Combine(modelRoot, "Y", "OK", "ALL", okName);

            RequireFile(ngX);
            RequireFile(ngY);
            RequireFile(okX);
            RequireFile(okY);

            TestExistingNg(root, model, ngX, ngY);
            TestExistingOk(root, model, okX, okY);
            TestExistingSubfolderWriter(ngX, ngY);
            TestNewOkWriter(okX);
            if (args.Length == 2) TestDiaCrossFolderLookup(Path.GetFullPath(args[1]));

            Console.WriteLine("ANNOTATION_CONTRACT_SMOKE=PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_CONTRACT_SMOKE=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void TestExistingNg(string root, string model, string x, string y)
    {
        using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
        {
            TrainingImageRoot = root,
            ModelName = model,
            Kind = AnnotationSampleKind.Ng,
            ImagePath = x,
            LabelPath = y,
            Editable = true
        }))
        {
            AnnotationValidationResult validation = session.ValidateLegacyPair();
            Require(validation.IsValid, "Existing NG pair validation failed: " + validation.Message);
            Require(!session.SamReady, "SAM must not report ready before backend is connected.");

            AnnotationLabelImage before = session.GetLegacyLabelImage();
            byte[] beforeBytes = Clone(before.Rgb24);
            AnnotationClassMap classes = session.GetClassMap();
            int classPixels = classes.ClassIds.Count(v => v > 0);
            Require(classPixels > 0, "Existing NG class map should contain class pixels.");
            int firstClassIndex = Array.FindIndex(classes.ClassIds, v => v > 0);
            Require(firstClassIndex >= 0, "Could not locate a class pixel for erase/rollback test.");

            AnnotationBinaryMask mask = session.ExportBinaryMask(new AnnotationBinaryExportOptions
            {
                Threshold = 220,
                IncludeMaybe = false,
                IncludeIgnore = false
            });
            int binaryPixels = mask.Data.Count(v => v > 0);
            Require(binaryPixels > 0, "Existing NG binary mask should contain foreground.");
            Require(BytesEqual(beforeBytes, session.GetLegacyLabelImage().Rgb24), "Binary export mutated legacy label state.");
            Require(!session.IsDirty, "Read/export operations must not dirty the session.");

            AnnotationSuggestion suggestion = session.Suggest(new[]
            {
                new AnnotationPrompt { Type = AnnotationPromptType.PositivePoint, X = 10, Y = 10 }
            });
            Require(!suggestion.Success, "SAM Suggest must fail closed until backend is connected.");
            Require(!session.IsDirty, "Failed SAM suggestion must not mutate state.");

            int eraseX = firstClassIndex % classes.Width;
            int eraseY = firstClassIndex / classes.Width;
            session.Erase(new AnnotationRect { X = eraseX, Y = eraseY, Width = 1, Height = 1 });
            Require(session.IsDirty, "Erase on a known class pixel did not dirty the session.");
            Require(session.GetClassMap().ClassIds[firstClassIndex] == 0, "Erase did not clear the selected class pixel.");
            session.Cancel();
            Require(!session.IsDirty, "Cancel did not restore committed state.");
            Require(BytesEqual(beforeBytes, session.GetLegacyLabelImage().Rgb24), "Cancel did not restore legacy label bytes.");

            Console.WriteLine("EXISTING_NG_PAIR=PASS class_pixels=" + classPixels + " binary_pixels=" + binaryPixels);
        }
    }

    private static void TestExistingOk(string root, string model, string x, string y)
    {
        using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
        {
            TrainingImageRoot = root,
            ModelName = model,
            Kind = AnnotationSampleKind.Ok,
            ImagePath = x,
            LabelPath = y,
            Editable = false
        }))
        {
            AnnotationValidationResult validation = session.ValidateLegacyPair();
            Require(validation.IsValid, "Existing OK pair validation failed: " + validation.Message);
            Require(session.GetLegacyLabelImage().Rgb24.All(v => v == 0), "Existing Y/OK is not blank black.");
            Require(session.GetClassMap().ClassIds.All(v => v == 0), "Existing Y/OK class map is not empty.");
            Console.WriteLine("EXISTING_OK_PAIR=PASS");
        }
    }

    private static void TestNewOkWriter(string sourceX)
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), "InspTools-Rebuild2-AnnotationSmoke-" + Guid.NewGuid().ToString("N"));
        string sourceFolder = Path.Combine(tempRoot, "source");
        string trainingRoot = Path.Combine(tempRoot, "TrainingImage");
        Directory.CreateDirectory(sourceFolder);
        Directory.CreateDirectory(trainingRoot);
        string source = Path.Combine(sourceFolder, "SMOKE-OK.png");
        File.Copy(sourceX, source, true);

        try
        {
            using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
            {
                TrainingImageRoot = trainingRoot,
                ModelName = "SmokeModel",
                Kind = AnnotationSampleKind.Ok,
                ImagePath = source,
                Editable = true,
                AllowOverwrite = false
            }))
            {
                Require(string.IsNullOrEmpty(session.Image.LabelPath), "New session unexpectedly resolved a Y label.");
                Require(session.GetLegacyLabelImage().Rgb24.All(v => v == 0), "New OK label must initialize blank black.");

                AnnotationSaveResult saved = session.SaveLegacyTrainingPair();
                Require(File.Exists(saved.XPath), "New writer did not create X file.");
                Require(File.Exists(saved.YPath), "New writer did not create Y file.");
                Require(saved.Width == session.Image.Width && saved.Height == session.Image.Height, "Saved dimensions changed.");

                AnnotationValidationResult validation = session.ValidateLegacyPair();
                Require(validation.IsValid, "New writer pair validation failed: " + validation.Message);
                Require(session.GetLegacyLabelImage().Rgb24.All(v => v == 0), "Saved Y/OK is not blank black.");
                Require(!session.IsDirty, "Successful save must commit state.");

                Console.WriteLine("NEW_OK_WRITER=PASS x=" + saved.XPath + " y=" + saved.YPath);
            }
        }
        finally
        {
            try { Directory.Delete(tempRoot, true); } catch { }
        }
    }

    private static void TestExistingSubfolderWriter(string sourceX, string sourceY)
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), "InspTools-Rebuild2-ExistingSubfolder-" + Guid.NewGuid().ToString("N"));
        string trainingRoot = Path.Combine(tempRoot, "TrainingImage");
        string model = "R002-CCD1";
        string name = Path.GetFileName(sourceX);
        string x = Path.Combine(trainingRoot, model, "X", "NG", "ALL", name);
        string y = Path.Combine(trainingRoot, model, "Y", "NG", "ALL", name);
        Directory.CreateDirectory(Path.GetDirectoryName(x));
        Directory.CreateDirectory(Path.GetDirectoryName(y));
        File.Copy(sourceX, x, true);
        File.Copy(sourceY, y, true);

        try
        {
            int beforeClassPixels;
            using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
            {
                TrainingImageRoot = trainingRoot,
                ModelName = model,
                Kind = AnnotationSampleKind.Ng,
                ImagePath = x,
                LabelPath = y,
                Editable = true,
                AllowOverwrite = false
            }))
            {
                AnnotationClassMap map = session.GetClassMap();
                beforeClassPixels = map.ClassIds.Count(v => v > 0);
                int first = Array.FindIndex(map.ClassIds, v => v > 0);
                Require(first >= 0, "Existing subfolder writer fixture has no class pixel.");
                session.Erase(new AnnotationRect { X = first % map.Width, Y = first / map.Width, Width = 1, Height = 1 });

                AnnotationSaveResult saved = session.SaveLegacyTrainingPair();
                Require(string.Equals(Path.GetFullPath(saved.XPath), Path.GetFullPath(x), StringComparison.OrdinalIgnoreCase),
                    "Existing X subfolder save target moved unexpectedly: " + saved.XPath);
                Require(string.Equals(Path.GetFullPath(saved.YPath), Path.GetFullPath(y), StringComparison.OrdinalIgnoreCase),
                    "Existing Y subfolder save target moved unexpectedly: " + saved.YPath);
                Require(!File.Exists(Path.Combine(trainingRoot, model, "X", "NG", name)),
                    "Existing X subfolder save created an unintended duplicate at X/NG root.");
                Require(!File.Exists(Path.Combine(trainingRoot, model, "Y", "NG", name)),
                    "Existing Y subfolder save created an unintended duplicate at Y/NG root.");
                Require(!session.IsDirty, "Existing subfolder save did not commit state.");
            }

            using (IAnnotationSession reopened = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
            {
                TrainingImageRoot = trainingRoot,
                ModelName = model,
                Kind = AnnotationSampleKind.Ng,
                ImagePath = x,
                LabelPath = y,
                Editable = false
            }))
            {
                int afterClassPixels = reopened.GetClassMap().ClassIds.Count(v => v > 0);
                Require(afterClassPixels == beforeClassPixels - 1,
                    "Existing subfolder save did not persist the edited loaded Y label.");
                Console.WriteLine("EXISTING_SUBFOLDER_WRITER=PASS y=" + reopened.Image.LabelPath);
            }
        }
        finally
        {
            try { Directory.Delete(tempRoot, true); } catch { }
        }
    }

    private static void TestDiaCrossFolderLookup(string root)
    {
        string model = "0816T4F-TF-4";
        string name = "T4F-C2-NG-CD4-0236.bmp";
        string x = Path.Combine(root, model, "X", "NG", name);
        string expectedY = Path.Combine(root, model, "Y", "NG", "TF", name);
        RequireFile(x);
        RequireFile(expectedY);

        using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
        {
            TrainingImageRoot = root,
            ModelName = model,
            Kind = AnnotationSampleKind.Ng,
            ImagePath = x,
            Editable = false
        }))
        {
            Require(string.Equals(Path.GetFullPath(session.Image.LabelPath), Path.GetFullPath(expectedY), StringComparison.OrdinalIgnoreCase),
                "Cross-folder Y lookup did not resolve the expected TF label.");
            AnnotationValidationResult validation = session.ValidateLegacyPair();
            Require(validation.IsValid, "DIA cross-folder pair validation failed: " + validation.Message);
            Console.WriteLine("DIA_CROSS_FOLDER_PAIR=PASS y=" + session.Image.LabelPath);
        }
    }

    private static void RequireFile(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Smoke fixture not found.", path);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static byte[] Clone(byte[] source)
    {
        var result = new byte[source.Length];
        Buffer.BlockCopy(source, 0, result, 0, source.Length);
        return result;
    }

    private static bool BytesEqual(byte[] left, byte[] right)
    {
        if (left == null || right == null || left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
        return true;
    }
}
