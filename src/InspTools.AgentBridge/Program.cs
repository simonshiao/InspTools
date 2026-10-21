using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Threading;
using InspTools.Agent;
using InspTools.SDK;

namespace InspTools.AgentBridge
{
    internal static class Program
    {
        private static volatile bool stopping;

        private static int Main(string[] args)
        {
            try
            {
                string pipeName = GetArg(args, "--pipe") ??
                    ("InspTools.AnnotationAgent." + Process.GetCurrentProcess().Id);
                string readyPath = GetArg(args, "--ready");
                string encoder = GetArg(args, "--sam-encoder");
                string decoder = GetArg(args, "--sam-decoder");
                bool preferCuda = GetBoolArg(args, "--prefer-cuda", false);

                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    stopping = true;
                    e.Cancel = true;
                };

                Func<IAnnotationSuggestionBackend> backendFactory = BuildBackendFactory(encoder, decoder, preferCuda);
                using (var dispatcher = new AnnotationAgentJsonDispatcher(backendFactory))
                {
                    WriteReadyFile(readyPath, pipeName, backendFactory != null);
                    Console.WriteLine("ANNOTATION_AGENT_BRIDGE_READY pipe=" + pipeName +
                        " protocol=" + AnnotationAgentJsonDispatcher.Protocol +
                        " sam=" + (backendFactory == null ? "optional-not-configured" : "configured"));
                    Console.Out.Flush();

                    while (!stopping)
                    {
                        using (var pipe = new NamedPipeServerStream(
                            pipeName,
                            PipeDirection.InOut,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.None))
                        {
                            pipe.WaitForConnection();
                            using (var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 4096, true))
                            using (var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, true) { AutoFlush = true })
                            {
                                while (!stopping && pipe.IsConnected)
                                {
                                    string line = reader.ReadLine();
                                    if (line == null) break;
                                    writer.WriteLine(dispatcher.Dispatch(line));
                                }
                            }
                        }
                    }
                }

                DeleteReadyFile(readyPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ANNOTATION_AGENT_BRIDGE_FAIL");
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static Func<IAnnotationSuggestionBackend> BuildBackendFactory(string encoder, string decoder, bool preferCuda)
        {
            if (string.IsNullOrWhiteSpace(encoder) && string.IsNullOrWhiteSpace(decoder)) return null;
            if (string.IsNullOrWhiteSpace(encoder) || string.IsNullOrWhiteSpace(decoder))
                throw new ArgumentException("--sam-encoder and --sam-decoder must be supplied together.");

            encoder = Path.GetFullPath(encoder);
            decoder = Path.GetFullPath(decoder);
            if (!File.Exists(encoder)) throw new FileNotFoundException("SAM encoder not found.", encoder);
            if (!File.Exists(decoder)) throw new FileNotFoundException("SAM decoder not found.", decoder);

            string assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.SamOnnx.dll");
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException("Optional SAM backend assembly is not beside AgentBridge.", assemblyPath);

            return () =>
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                Type optionsType = assembly.GetType("InspTools.SamOnnx.SamOnnxOptions", true);
                Type backendType = assembly.GetType("InspTools.SamOnnx.SamOnnxSuggestionBackend", true);
                object options = Activator.CreateInstance(optionsType);
                optionsType.GetProperty("EncoderModelPath").SetValue(options, encoder, null);
                optionsType.GetProperty("DecoderModelPath").SetValue(options, decoder, null);
                optionsType.GetProperty("PreferCuda").SetValue(options, preferCuda, null);
                PropertyInfo selection = optionsType.GetProperty("MaskSelection");
                selection.SetValue(options, Enum.Parse(selection.PropertyType, "FirstMaskLegacy"), null);
                object backend = Activator.CreateInstance(backendType, new[] { options });
                var typed = backend as IAnnotationSuggestionBackend;
                if (typed == null) throw new InvalidCastException("Loaded SAM backend does not implement IAnnotationSuggestionBackend.");
                return typed;
            };
        }

        private static void WriteReadyFile(string path, string pipeName, bool samConfigured)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            path = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            string json = "{\n" +
                "  \"protocol\": \"" + AnnotationAgentJsonDispatcher.Protocol + "\",\n" +
                "  \"pid\": " + Process.GetCurrentProcess().Id + ",\n" +
                "  \"pipe\": \"" + JsonEscape(pipeName) + "\",\n" +
                "  \"samConfigured\": " + (samConfigured ? "true" : "false") + ",\n" +
                "  \"contract\": \"docs/AgentAnnotation.contract.json\"\n" +
                "}\n";
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        private static void DeleteReadyFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                path = Path.GetFullPath(path);
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // Discovery cleanup must never turn a clean bridge shutdown into a failure.
            }
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        private static bool GetBoolArg(string[] args, string name, bool fallback)
        {
            string value = GetArg(args, name);
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            bool result;
            if (bool.TryParse(value, out result)) return result;
            return value == "1" ? true : value == "0" ? false : fallback;
        }

        private static string JsonEscape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }
}
