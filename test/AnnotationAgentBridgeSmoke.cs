using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

internal static class AnnotationAgentBridgeSmoke
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

    private static int Main(string[] args)
    {
        Process process = null;
        try
        {
            if (args.Length != 2 && args.Length != 4)
                throw new ArgumentException("Usage: AnnotationAgentBridgeSmoke.exe <bridge.exe> <TrainingImageRoot> [encoder.onnx decoder.onnx]");

            string bridge = Path.GetFullPath(args[0]);
            string root = Path.GetFullPath(args[1]);
            bool withSam = args.Length == 4;
            string pipeName = "InspTools.AnnotationAgent.Smoke." + Guid.NewGuid().ToString("N");
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);

            string bridgeArgs = "--pipe \"" + pipeName + "\"";
            if (withSam)
                bridgeArgs += " --sam-encoder \"" + Path.GetFullPath(args[2]) + "\" --sam-decoder \"" + Path.GetFullPath(args[3]) + "\" --prefer-cuda false";

            process = Process.Start(new ProcessStartInfo
            {
                FileName = bridge,
                Arguments = bridgeArgs,
                WorkingDirectory = Path.GetDirectoryName(bridge),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            using (var pipe = Connect(pipeName, 15000))
            using (var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 4096, true))
            using (var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, true) { AutoFlush = true })
            {
                Require(Ok(Call(writer, reader, "{\"action\":\"annotation.capabilities\"}")), "Bridge capabilities failed.");
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
                Require(Ok(Call(writer, reader, open)), "Bridge open failed.");
                int before = Class1(Call(writer, reader, "{\"action\":\"annotation.class_map.get\"}"));
                Require(before == 40059, "Bridge class1 mismatch: " + before);

                if (withSam)
                {
                    Require(Ok(Call(writer, reader, "{\"action\":\"annotation.sam.box\",\"x\":10,\"y\":16,\"x2\":151,\"y2\":684}")), "Bridge Box failed.");
                    IDictionary<string, object> suggestion = Parse(Call(writer, reader, "{\"action\":\"annotation.suggestion.get\"}"));
                    Require(GetBool(suggestion, "ok"), "Bridge SAM suggestion failed: " + GetString(suggestion, "message"));
                    var v = (IDictionary<string, object>)suggestion["value"];
                    Require(Convert.ToInt32(v["positivePixels"]) == 11961, "Bridge SAM positive mismatch.");
                    Require(Ok(Call(writer, reader, "{\"action\":\"annotation.suggestion.commit\",\"classId\":1,\"mergeMode\":\"FillEmptyOnly\"}")), "Bridge commit failed.");
                    Require(Class1(Call(writer, reader, "{\"action\":\"annotation.class_map.get\"}")) == 51902, "Bridge commit class1 mismatch.");
                    Require(Ok(Call(writer, reader, "{\"action\":\"annotation.cancel\"}")), "Bridge cancel failed.");
                    Require(Class1(Call(writer, reader, "{\"action\":\"annotation.class_map.get\"}")) == 40059, "Bridge rollback mismatch.");
                }
                else
                {
                    Require(Ok(Call(writer, reader, "{\"action\":\"annotation.sam.point.add\",\"x\":10,\"y\":10}")), "Bridge point failed.");
                    Require(!Ok(Call(writer, reader, "{\"action\":\"annotation.suggestion.get\"}")), "Bridge without SAM must fail closed.");
                }

                Require(Ok(Call(writer, reader, "{\"action\":\"annotation.close\"}")), "Bridge close failed.");
            }

            Console.WriteLine(withSam
                ? "ANNOTATION_AGENT_BRIDGE_SAM=PASS class1=40059->51902->40059 positive=11961"
                : "ANNOTATION_AGENT_BRIDGE=PASS class1=40059 fail_closed_no_sam=PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ANNOTATION_AGENT_BRIDGE_SMOKE=FAIL");
            Console.Error.WriteLine(ex.ToString());
            if (process != null && process.HasExited)
            {
                Console.Error.WriteLine(process.StandardOutput.ReadToEnd());
                Console.Error.WriteLine(process.StandardError.ReadToEnd());
            }
            return 1;
        }
        finally
        {
            if (process != null)
            {
                try { if (!process.HasExited) process.Kill(); }
                catch { }
                process.Dispose();
            }
        }
    }

    private static NamedPipeClientStream Connect(string pipeName, int timeoutMs)
    {
        Exception last = null;
        DateTime until = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < until)
        {
            var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);
            try
            {
                pipe.Connect(500);
                return pipe;
            }
            catch (Exception ex)
            {
                last = ex;
                pipe.Dispose();
                Thread.Sleep(100);
            }
        }
        throw new IOException("Could not connect to AgentBridge pipe.", last);
    }

    private static string Call(StreamWriter writer, StreamReader reader, string request)
    {
        writer.WriteLine(request);
        string response = reader.ReadLine();
        if (response == null) throw new EndOfStreamException("AgentBridge closed without a response.");
        return response;
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
