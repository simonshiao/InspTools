using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

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
        private static string LocalName(string name)
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
        private static string Preflight(string root, string dataset, string seedName)
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
        internal static bool TryRun(string[] args, out int exit)
        {
            exit = 0;
            if (args == null || args.Length == 0 ||
                !args[0].StartsWith("--train-general-", StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                Console.OutputEncoding = Utf8;
                if (args.Length != 3 || args[0] != "--train-general-preflight")
                    throw new ArgumentException(
                        "Usage: --train-general-preflight DATASET_NAME SEED.h5");
                string result = Preflight(AppDomain.CurrentDomain.BaseDirectory,
                    args[1], args[2]);
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
