using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using InspTools.Agent;
using InspTools.SamOnnx;

internal static class AnnotationJsonDispatcherSamSmoke
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3)
                throw new ArgumentException("Usage: AnnotationJsonDispatcherSamSmoke.exe <TrainingImageRoot> <encoder.onnx> <decoder.onnx>");

            string root = Path.GetFullPath(args[0]);
            string encoder = Path.GetFullPath(args[1]);
            string decoder = Path.GetFullPath(args[2]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);

            using (var dispatcher = new AnnotationAgentJsonDispatcher(() => new SamOnnxSuggestionBackend(new SamOnnxOptions
            {
                EncoderModelPath = encoder,
                DecoderModelPath = decoder,
                PreferCuda = false,
                MaskSelection = SamMaskSelection.FirstMaskLegacy
            })))
            {
                string open = Json.Serialize(new Dictionary<string, object>
                {
                    { "action", "annotation.open" },
                    { "trainingImageRoot", root },
                    { "modelName", model },
                    { "kind", "Ng" },
                    { "imagePath", x },
                    { "labelPath", y },
                    { "editable", true }
                });
                Require(Ok(dispatcher.Dispatch(open)), "SAM JSON open failed.");

                IDictionary<string, object> auto = Parse(dispatcher.Dispatch("{\"action\":\"annotation.auto.propose\"}"));
                Require(GetBool(auto, "ok"), "Auto proposal scan failed: " + GetString(auto, "message"));
                var autoValue = (IDictionary<string, object>)auto["value"];
                Require(Convert.ToInt32(autoValue["pointCount"]) == 16, "Auto proposal point count mismatch.");
                Require(Convert.ToInt32(autoValue["candidateCount"]) == 64, "Auto proposal candidate count mismatch.");
                Require(Convert.ToInt32(autoValue["recoveredAcceptedCount"]) == 6, "Auto proposal recovered filter mismatch.");
                Require(Convert.ToInt32(autoValue["safeProposalCount"]) == 0, "Giant recovered masks should be rejected by safety coverage gate.");
                var autoProposals = (IList)autoValue["proposals"];
                Require(autoProposals.Count == 6, "Auto proposal summary count mismatch.");
                foreach (object raw in autoProposals)
                {
                    var item = (IDictionary<string, object>)raw;
                    Require(Convert.ToBoolean(item["rejectedBySafety"]), "Recovered giant mask bypassed safety gate.");
                    Require(Convert.ToDouble(item["coverageRatio"]) > 0.90, "Reference recovered proposal coverage changed unexpectedly.");
                }

                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.sam.box\",\"x\":10,\"y\":16,\"x2\":151,\"y2\":684}")), "Box command failed.");

                IDictionary<string, object> suggested = Parse(dispatcher.Dispatch("{\"action\":\"annotation.suggestion.get\"}"));
                Require(GetBool(suggested, "ok"), "Suggestion failed: " + GetString(suggested, "message"));
                var suggestionValue = (IDictionary<string, object>)suggested["value"];
                int positive = Convert.ToInt32(suggestionValue["positivePixels"]);
                double score = Convert.ToDouble(suggestionValue["score"]);
                Require(positive == 11961, "SAM JSON parity mismatch. positive=" + positive);
                Require(Math.Abs(score - 0.748689591884613) <= 0.02, "SAM JSON score mismatch: " + score);

                int before = Class1(dispatcher.Dispatch("{\"action\":\"annotation.class_map.get\"}"));
                Require(before == 40059, "Pre-commit class1 mismatch: " + before);
                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.suggestion.commit\",\"classId\":1,\"mergeMode\":\"FillEmptyOnly\"}")), "Commit failed.");
                int after = Class1(dispatcher.Dispatch("{\"action\":\"annotation.class_map.get\"}"));
                Require(after > before, "Commit did not add class pixels.");
                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.cancel\"}")), "Cancel failed.");
                int restored = Class1(dispatcher.Dispatch("{\"action\":\"annotation.class_map.get\"}"));
                Require(restored == 40059, "Cancel did not restore class1: " + restored);

                Console.WriteLine("ANNOTATION_JSON_SAM=PASS positive=" + positive +
                    " score=" + score.ToString("0.000000") +
                    " auto=16/64/6/0" +
                    " before=" + before + " after=" + after + " restored=" + restored);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_JSON_SAM=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static int Class1(string text)
    {
        var root = Parse(text);
        Require(GetBool(root, "ok"), "class_map.get failed: " + GetString(root, "message"));
        var value = (IDictionary<string, object>)root["value"];
        var counts = (IList)value["classPixelCounts"];
        return Convert.ToInt32(counts[1]);
    }

    private static IDictionary<string, object> Parse(string text)
        => (IDictionary<string, object>)Json.DeserializeObject(text);

    private static bool Ok(string text) => GetBool(Parse(text), "ok");

    private static bool GetBool(IDictionary<string, object> value, string key)
        => value.ContainsKey(key) && Convert.ToBoolean(value[key]);

    private static string GetString(IDictionary<string, object> value, string key)
        => value.ContainsKey(key) && value[key] != null ? Convert.ToString(value[key]) : string.Empty;

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
