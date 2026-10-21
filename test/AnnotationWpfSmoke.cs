using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using InspTools.SDK;
using InspTools.Wpf;

internal static class AnnotationWpfSmoke
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("Usage: AnnotationWpfSmoke.exe <TrainingImageRoot>");
            string root = Path.GetFullPath(args[0]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);

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
                var control = new AnnotationEditorControl { Session = session };
                Require(control.GetClassPixelCount(1) == 40059, "WPF class overlay parity failed.");
                Require(!control.SamReady, "No-backend WPF smoke must not report SAM ready.");
                control.AddPositivePoint(10, 10);
                control.AddNegativePoint(20, 20);
                control.SetBoxPrompt(10, 16, 151, 684);
                Require(control.PromptCount == 3, "WPF prompt adapter count mismatch: " + control.PromptCount);
                AnnotationSuggestion failClosed = control.RunSuggestion();
                Require(!failClosed.Success, "WPF without backend must fail suggestion closed.");
                control.ClearPrompts();
                Require(control.PromptCount == 0, "WPF clear prompts failed.");
                control.CancelChanges();
                Require(!session.IsDirty && control.GetClassPixelCount(1) == 40059, "WPF cancel parity failed.");
            }

            Console.WriteLine("ANNOTATION_WPF_SMOKE=PASS class1=40059 prompts=PASS fail_closed_no_sam=PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_WPF_SMOKE=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
