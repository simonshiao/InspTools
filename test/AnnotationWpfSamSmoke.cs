using System;
using System.IO;
using System.Linq;
using InspTools.SDK;
using InspTools.SamOnnx;
using InspTools.Wpf;

internal static class AnnotationWpfSamSmoke
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3)
                throw new ArgumentException("Usage: AnnotationWpfSamSmoke.exe <TrainingImageRoot> <encoder.onnx> <decoder.onnx>");

            string root = Path.GetFullPath(args[0]);
            string encoder = Path.GetFullPath(args[1]);
            string decoder = Path.GetFullPath(args[2]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);

            using (var backend = new SamOnnxSuggestionBackend(new SamOnnxOptions
            {
                EncoderModelPath = encoder,
                DecoderModelPath = decoder,
                PreferCuda = false,
                MaskSelection = SamMaskSelection.FirstMaskLegacy
            }))
            using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
            {
                TrainingImageRoot = root,
                ModelName = model,
                Kind = AnnotationSampleKind.Ng,
                ImagePath = x,
                LabelPath = y,
                Editable = true,
                SuggestionBackend = backend
            }))
            {
                var control = new AnnotationEditorControl { Session = session };
                Require(control.SamReady, "WPF control does not report SAM ready.");
                Require(control.GetClassPixelCount(1) == 40059, "WPF pre-SAM class parity failed.");

                control.SetBoxPrompt(10, 16, 151, 684);
                AnnotationSuggestion suggestion = control.RunSuggestion();
                Require(suggestion.Success, "WPF SAM suggestion failed: " + suggestion.Message);
                int positive = suggestion.Mask.Count(v => v > 0f);
                Require(positive == 11961, "WPF SAM parity mismatch. positive=" + positive);
                Require(Math.Abs(suggestion.Score - 0.748689591884613) <= 0.02,
                    "WPF SAM score mismatch: " + suggestion.Score);

                control.CommitSuggestion();
                int after = control.GetClassPixelCount(1);
                Require(after == 51902, "WPF SAM commit parity mismatch. class1=" + after);
                Require(session.IsDirty, "WPF SAM commit must dirty session.");

                control.CancelChanges();
                Require(!session.IsDirty, "WPF SAM cancel did not clear dirty state.");
                Require(control.GetClassPixelCount(1) == 40059, "WPF SAM cancel did not restore label.");

                Console.WriteLine("ANNOTATION_WPF_SAM=PASS positive=" + positive +
                    " score=" + suggestion.Score.ToString("0.000000") +
                    " after=" + after +
                    " restored=40059");
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_WPF_SAM=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
