using System;
using System.IO;
using System.Linq;
using InspTools.Agent;
using InspTools.SDK;

internal static class AnnotationAgentSmoke
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1)
                throw new ArgumentException("Usage: AnnotationAgentSmoke.exe <TrainingImageRoot>");

            string root = Path.GetFullPath(args[0]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);
            Require(File.Exists(x) && File.Exists(y), "Agent smoke fixture missing.");

            using (var agent = new AnnotationAgentController())
            {
                Require(agent.Capabilities.Contains("annotation.open"), "annotation.open capability missing.");
                Require(agent.Capabilities.Contains("annotation.suggestion.commit"), "annotation.suggestion.commit capability missing.");

                AgentResult<AnnotationAgentStatus> opened = agent.Open(new AnnotationAgentOpenRequest
                {
                    TrainingImageRoot = root,
                    ModelName = model,
                    Kind = AnnotationSampleKind.Ng,
                    ImagePath = x,
                    LabelPath = y,
                    Editable = true
                });
                Require(opened.Success && opened.Value.SessionOpen, "Agent open failed: " + opened.Message);
                Require(!opened.Value.SamReady, "Agent without backend must not report SAM ready.");

                AgentResult<AnnotationValidationResult> validation = agent.ValidateLegacy();
                Require(validation.Success && validation.Value.IsValid, "Agent validate failed: " + validation.Message);

                AgentResult<AnnotationAgentClassSummary> summary = agent.GetClassSummary();
                Require(summary.Success, "Agent class summary failed: " + summary.Message);
                Require(summary.Value.ClassPixelCounts[1] == 40059,
                    "Agent class summary parity failed. class1=" + summary.Value.ClassPixelCounts[1]);

                AgentResult<AnnotationSuggestion> noSam = agent.Suggest(new[]
                {
                    new AnnotationPrompt { Type = AnnotationPromptType.PositivePoint, X = 10, Y = 10 }
                });
                Require(!noSam.Success, "Agent without backend must fail suggestion closed.");

                AgentResult<AnnotationAgentStatus> erased = agent.Erase(new AnnotationRect { X = 0, Y = 0, Width = 64, Height = 64 });
                Require(erased.Success, "Agent erase failed: " + erased.Message);
                AgentResult<AnnotationAgentStatus> cancelled = agent.Cancel();
                Require(cancelled.Success && !cancelled.Value.IsDirty, "Agent cancel rollback failed.");

                AgentResult<bool> closed = agent.Close();
                Require(closed.Success, "Agent close failed.");
                Require(!agent.Status().Value.SessionOpen, "Agent status still reports open after close.");
            }

            Console.WriteLine("ANNOTATION_AGENT_SMOKE=PASS class1=40059 fail_closed_no_sam=PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_AGENT_SMOKE=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
