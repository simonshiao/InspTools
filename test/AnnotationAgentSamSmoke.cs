using System;
using System.IO;
using System.Linq;
using InspTools.Agent;
using InspTools.SDK;
using InspTools.SamOnnx;

internal static class AnnotationAgentSamSmoke
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3)
                throw new ArgumentException("Usage: AnnotationAgentSamSmoke.exe <TrainingImageRoot> <encoder.onnx> <decoder.onnx>");

            string root = Path.GetFullPath(args[0]);
            string encoder = Path.GetFullPath(args[1]);
            string decoder = Path.GetFullPath(args[2]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);

            using (var agent = new AnnotationAgentController(() => new SamOnnxSuggestionBackend(new SamOnnxOptions
            {
                EncoderModelPath = encoder,
                DecoderModelPath = decoder,
                PreferCuda = false,
                MaskSelection = SamMaskSelection.FirstMaskLegacy
            })))
            {
                AgentResult<AnnotationAgentStatus> opened = agent.Open(new AnnotationAgentOpenRequest
                {
                    TrainingImageRoot = root,
                    ModelName = model,
                    Kind = AnnotationSampleKind.Ng,
                    ImagePath = x,
                    LabelPath = y,
                    Editable = true
                });
                Require(opened.Success && opened.Value.SamReady, "Agent SAM open failed: " + opened.Message + " / " + opened.Value?.SamState);

                AgentResult<AnnotationSuggestion> suggestion = agent.Suggest(new[]
                {
                    new AnnotationPrompt
                    {
                        Type = AnnotationPromptType.Box,
                        X = 10,
                        Y = 16,
                        X2 = 151,
                        Y2 = 684
                    }
                });
                Require(suggestion.Success, "Agent SAM suggest failed: " + suggestion.Message);
                int positive = suggestion.Value.Mask.Count(v => v > 0f);
                Require(positive == 11961, "Agent SAM parity mismatch. positive=" + positive);
                Require(Math.Abs(suggestion.Value.Score - 0.748689591884613) <= 0.02,
                    "Agent SAM score mismatch: " + suggestion.Value.Score);

                AgentResult<AnnotationAgentClassSummary> before = agent.GetClassSummary();
                Require(before.Success && before.Value.ClassPixelCounts[1] == 40059, "Agent pre-commit summary mismatch.");

                AgentResult<AnnotationAgentStatus> committed = agent.CommitSuggestion(1, AnnotationMergeMode.FillEmptyOnly);
                Require(committed.Success && committed.Value.IsDirty, "Agent commit did not dirty session.");
                AgentResult<AnnotationAgentClassSummary> after = agent.GetClassSummary();
                Require(after.Success && after.Value.ClassPixelCounts[1] > before.Value.ClassPixelCounts[1],
                    "Agent SAM commit did not add class pixels.");

                AgentResult<AnnotationAgentStatus> cancelled = agent.Cancel();
                Require(cancelled.Success && !cancelled.Value.IsDirty, "Agent SAM cancel failed.");
                AgentResult<AnnotationAgentClassSummary> restored = agent.GetClassSummary();
                Require(restored.Success && restored.Value.ClassPixelCounts[1] == 40059,
                    "Agent SAM cancel did not restore class map.");

                Console.WriteLine("ANNOTATION_AGENT_SAM=PASS positive=" + positive +
                    " score=" + suggestion.Value.Score.ToString("0.000000") +
                    " before=" + before.Value.ClassPixelCounts[1] +
                    " after=" + after.Value.ClassPixelCounts[1] +
                    " restored=" + restored.Value.ClassPixelCounts[1]);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_AGENT_SAM=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
