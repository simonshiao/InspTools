using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using InspTools.Agent;

internal static class AnnotationJsonDispatcherSmoke
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1)
                throw new ArgumentException("Usage: AnnotationJsonDispatcherSmoke.exe <TrainingImageRoot>");

            string root = Path.GetFullPath(args[0]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);
            Require(File.Exists(x) && File.Exists(y), "JSON dispatcher smoke fixture missing.");

            using (var dispatcher = new AnnotationAgentJsonDispatcher())
            {
                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.capabilities\"}")), "Capabilities failed.");

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
                Require(Ok(dispatcher.Dispatch(open)), "Open failed.");

                IDictionary<string, object> classMap = Parse(dispatcher.Dispatch("{\"action\":\"annotation.class_map.get\"}"));
                Require(GetBool(classMap, "ok"), "Class map failed.");
                var value = (IDictionary<string, object>)classMap["value"];
                var counts = (IList)value["classPixelCounts"];
                Require(Convert.ToInt32(counts[1]) == 40059, "Class 1 parity failed: " + counts[1]);

                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.sam.point.add\",\"x\":10,\"y\":10}")), "Point add failed.");
                IDictionary<string, object> noSam = Parse(dispatcher.Dispatch("{\"action\":\"annotation.suggestion.get\"}"));
                Require(!GetBool(noSam, "ok"), "No-SAM suggestion must fail closed.");

                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.erase\",\"x\":0,\"y\":0,\"width\":64,\"height\":64}")), "Erase failed.");
                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.cancel\"}")), "Cancel failed.");

                IDictionary<string, object> restored = Parse(dispatcher.Dispatch("{\"action\":\"annotation.class_map.get\"}"));
                var restoredCounts = (IList)((IDictionary<string, object>)restored["value"])["classPixelCounts"];
                Require(Convert.ToInt32(restoredCounts[1]) == 40059, "Cancel restore failed.");
                Require(Ok(dispatcher.Dispatch("{\"action\":\"annotation.close\"}")), "Close failed.");
            }

            Console.WriteLine("ANNOTATION_JSON_DISPATCHER=PASS protocol=" + AnnotationAgentJsonDispatcher.Protocol + " class1=40059 fail_closed_no_sam=PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_JSON_DISPATCHER=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static IDictionary<string, object> Parse(string text)
        => (IDictionary<string, object>)Json.DeserializeObject(text);

    private static bool Ok(string text) => GetBool(Parse(text), "ok");

    private static bool GetBool(IDictionary<string, object> value, string key)
        => value.ContainsKey(key) && Convert.ToBoolean(value[key]);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
