using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace InspTools.Standalone
{
    // General DAVS candidate-only training remains isolated from production.
    internal static class GeneralTrainingJob
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private const string TrainerSha =
            "4988283839A194DCF18E0B914060AC62D1A617D8923F4872DC729269FD09011E";
        private const string DavsSha =
            "8645361D666E06A80E0BD52C1C91A63F3D5AFE90414297F001ADAAAE44F27522";
        internal static string LocalName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == ".." ||
                name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                name.Contains("/") || name.Contains("\\") || name.Contains(":") ||
                name.EndsWith(" ") || name.EndsWith("."))
                throw new ArgumentException("Expected a single local filename");
            return name;
        }
        private static string Sha256(string path)
        {
            using (var hash = SHA256.Create())
            using (var file = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(file)).Replace("-", "");
        }
        internal static string Preflight(string root, string dataset, string seedName)
        {
            LocalName(dataset);
            LocalName(seedName);
            if (!seedName.EndsWith(".h5", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("DAVS seed must be .h5");
            string seed = Path.Combine(root, "TrainingSeed", seedName);
            if (!File.Exists(seed) || new FileInfo(seed).Length < 4096)
                throw new FileNotFoundException("Local TrainingSeed missing", seed);
            foreach (string path in new[] {
                Path.Combine(root, "Runtime", "TrainingPython", "Python38", "python.exe"),
                Path.Combine(root, "Runtime", "TrainingRebuild", "train_compat.py"),
                Path.Combine(root, "Runtime", "TrainingRebuild", "RecoveredDAVS",
                    "DAVS", "DAVSTrain_v3.pyc")
            })
                if (!File.Exists(path))
                    throw new FileNotFoundException("App-local trainer missing", path);
            string trainer = Path.Combine(root, "Runtime", "TrainingRebuild", "train_compat.py");
            string davs = Path.Combine(root, "Runtime", "TrainingRebuild",
                "RecoveredDAVS", "DAVS", "DAVSTrain_v3.pyc");
            if (!string.Equals(Sha256(trainer), TrainerSha, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(Sha256(davs), DavsSha, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Pinned training backend SHA mismatch");
            TrainingDatasetReport data = TrainingContracts.Check(
                root, dataset, TrainingFamily.General);
            return "DATASET=" + dataset + " IMAGES=" + data.SourceImages +
                " MASKS=" + data.Labels + " SEED_SHA=" + Sha256(seed);
        }
        internal static string GpuProbe(string root)
        {
            string runtime = Path.Combine(root, "Runtime", "TrainingPython");
            string python = Path.Combine(runtime, "Python38", "python.exe");
            string gpu = Path.Combine(runtime, "GpuRuntime");
            if (!File.Exists(python) || !Directory.Exists(gpu))
                throw new FileNotFoundException("App-local Python/GpuRuntime missing", python);
            var start = new ProcessStartInfo(python,
                "-c \"import tensorflow as tf; print('TF=' + tf.__version__); print('GPU=' + str(len(tf.config.list_physical_devices('GPU'))))\"");
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.WorkingDirectory = root;
            start.EnvironmentVariables["PATH"] = gpu + ";" + Path.Combine(runtime, "Python38") +
                ";" + Path.Combine(runtime, "Python38", "Scripts") + ";" +
                start.EnvironmentVariables["PATH"];
            start.EnvironmentVariables["TF_FORCE_GPU_ALLOW_GROWTH"] = "true";
            using (var process = Process.Start(start))
            {
                if (!process.WaitForExit(60000))
                {
                    process.Kill();
                    throw new TimeoutException("GPU probe exceeded 60 seconds");
                }
                string output = process.StandardOutput.ReadToEnd().Trim();
                if (process.ExitCode != 0 || !output.Contains("GPU=1"))
                    throw new InvalidOperationException("GPU probe failed: " + output);
                return output.Replace("\r", " ").Replace("\n", " ");
            }
        }
        internal static bool TryRun(string[] args, out int exit)
        {
            exit = 0;
            if (args == null || args.Length == 0 ||
                !args[0].StartsWith("--train-general-", StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                Console.OutputEncoding = Utf8;
                if (args.Length == 1 && args[0] == "--train-general-selftest")
                {
                    GeneralCandidateRunner.SelfTest();
                    Console.WriteLine("TRAIN_GENERAL_CANDIDATE_SELFTEST=PASS GPU_RUN=False");
                    return true;
                }
                if (args.Length == 1 && args[0] == "--train-general-executor-selftest")
                {
                    GeneralTrainerExecutor.SelfTest();
                    Console.WriteLine("TRAIN_EXECUTOR=PASS PROCESS_TIMEOUT=PASS STOP_REQUEST=PASS GPU_RUN=False");
                    return true;
                }
                if (args.Length == 2 && args[0] == "--train-general-run")
                {
                    string report = GeneralTrainerExecutor.Run(
                        AppDomain.CurrentDomain.BaseDirectory, args[1]);
                    Console.WriteLine(report);
                    exit = report.Contains("TRAIN_RESULT=PASS") ? 0 : 75;
                    return true;
                }
                if (args.Length == 1 && args[0] == "--train-general-gpu-probe")
                {
                    Console.WriteLine("TRAIN_GENERAL_GPU_PROBE=PASS " +
                        GpuProbe(AppDomain.CurrentDomain.BaseDirectory));
                    return true;
                }
                if (args.Length == 2 && args[0] == "--train-general-status")
                {
                    Console.WriteLine(GeneralCandidateRunner.Status(
                        AppDomain.CurrentDomain.BaseDirectory, args[1]));
                    return true;
                }
                if (args.Length == 2 && args[0] == "--train-general-stop")
                {
                    Console.WriteLine(GeneralCandidateRunner.RequestStop(
                        AppDomain.CurrentDomain.BaseDirectory, args[1]));
                    return true;
                }
                if (args.Length != 3 ||
                    (args[0] != "--train-general-preflight" &&
                     args[0] != "--train-general-plan"))
                    throw new ArgumentException(
                        "Usage: --train-general-preflight|--train-general-plan DATASET_NAME SEED.h5");
                string result = Preflight(AppDomain.CurrentDomain.BaseDirectory,
                    args[1], args[2]);
                if (args[0] == "--train-general-plan")
                    Console.WriteLine("CANDIDATE_JOB=" + Path.GetFileName(
                        GeneralCandidateRunner.Prepare(AppDomain.CurrentDomain.BaseDirectory,
                            args[1], args[2])));
                Console.WriteLine("TRAIN_GENERAL_PREFLIGHT=READY " + result +
                    " TRAIN_EXECUTED=False");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("TRAIN_GENERAL_PREFLIGHT=BLOCKED " +
                    ex.GetBaseException().Message);
                exit = 74;
            }
            return true;
        }
    }
}
