using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InspTools.Standalone
{
    // All outputs remain under the standalone AppBase; never deploy a model here.
    internal static class GeneralCandidateRunner
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private static string Hash(string file)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static int Number(Dictionary<string, string> values, string name,
            int min, int max)
        {
            string raw;
            int parsed;
            if (!values.TryGetValue(name, out raw) ||
                !int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out parsed) ||
                parsed < min || parsed > max)
                throw new InvalidDataException("Vendor training profile has invalid " + name);
            return parsed;
        }

        // Read only safe, numeric training parameters from an APP-LOCAL snapshot.
        // Paths, epoch count, GPU, and deployment settings in vendor INI are ignored.
        private static string ProfileSettings(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Vendor profile missing", path);
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool section = false;
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.StartsWith("[", StringComparison.Ordinal) &&
                    line.EndsWith("]", StringComparison.Ordinal))
                {
                    section = line.Equals("[DAVS_V3]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                if (!section || line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal) ||
                    line.StartsWith("#", StringComparison.Ordinal)) continue;
                int split = line.IndexOf('=');
                if (split > 0) values[line.Substring(0, split).Trim()] =
                    line.Substring(split + 1).Trim();
            }
            int width = Number(values, "ImgWidth", 64, 2048);
            int height = Number(values, "ImgHeight", 64, 2048);
            int div = Number(values, "UnetDiv", 1, 32);
            int loss = Number(values, "LossFunction", 1, 12);
            int okBlank = Number(values, "OKBlank", 0, 1);
            int ngIgnore = Number(values, "NGIgnore", 0, 1);
            if (width % 16 != 0 || height % 16 != 0)
                throw new InvalidDataException("Vendor image size must be divisible by 16");
            string modelType;
            if (!values.TryGetValue("ModelType", out modelType) || modelType != "US")
                throw new InvalidDataException("Only validated US vendor model type is supported");
            string lrText;
            double lr;
            if (!values.TryGetValue("LR", out lrText) ||
                !double.TryParse(lrText, NumberStyles.Float, CultureInfo.InvariantCulture, out lr) ||
                double.IsNaN(lr) || lr < 0.000001 || lr > 0.1)
                throw new InvalidDataException("Vendor LR is outside allowed range");
            return "\r\nImgWidth=" + width + "\r\nImgHeight=" + height +
                "\r\nUnetDiv=" + div + "\r\nLossFunction=" + loss +
                "\r\nModelType=US\r\nLR=" + lr.ToString("R", CultureInfo.InvariantCulture) +
                "\r\nOKBlank=" + okBlank + "\r\nNGIgnore=" + ngIgnore;
        }
        internal static void State(string job, string value)
        {
            File.WriteAllText(Path.Combine(job, "state.txt"),
                "STATE=" + value + "\r\nUTC=" + DateTime.UtcNow.ToString("o"), Utf8);
        }
        internal static string Prepare(string root, string dataset, string seed,
            string profile = null, int epochs = 1, int maxSamples = 8,
            int batchSize = 1, int maxSeconds = 900)
        {
            GeneralTrainingJob.LocalName(dataset);
            GeneralTrainingJob.LocalName(seed);
            bool longTrain = epochs == 300 && maxSamples == 0 &&
                batchSize == 4 && maxSeconds == 7200 &&
                dataset == "DIA_CCD1_S1_Train300" &&
                profile == "DIA_R002_vendor.ini" &&
                seed == "DIA_R002_vendor.h5";
            if (!longTrain && (epochs != 1 || maxSamples != 8 ||
                batchSize != 1 || maxSeconds != 900))
                throw new ArgumentException("Only 1-epoch smoke or pinned 300-epoch CCD1 mode supported");
            string longManifest = null;
            if (longTrain)
                longManifest = GeneralTrainingJob.VerifyLongDataset(root);
            if (!seed.EndsWith(".h5", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("DAVS seed must be .h5");
            TrainingContracts.Check(root, dataset, TrainingFamily.General);
            string seedPath = Path.Combine(root, "TrainingSeed", seed);
            if (!File.Exists(seedPath) || new FileInfo(seedPath).Length < 4096)
                throw new FileNotFoundException("Candidate seed missing", seedPath);
            string profilePath = null;
            string profileSettings = "\r\nLossFunction=6\r\nUnetDiv=8" +
                "\r\nImgWidth=256\r\nImgHeight=512\r\nLR=0.001" +
                "\r\nModelType=US";
            if (profile != null)
            {
                GeneralTrainingJob.LocalName(profile);
                if (!profile.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Profile must be an app-local .ini");
                profilePath = Path.Combine(root, "TrainingProfiles", profile);
                profileSettings = ProfileSettings(profilePath);
            }
            string jobs = Path.Combine(root, "TrainingCandidates");
            Directory.CreateDirectory(jobs);
            string id = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" +
                Guid.NewGuid().ToString("N").Substring(0, 8);
            string job = Path.Combine(jobs, id);
            Directory.CreateDirectory(job);
            string x = Path.Combine(root, "TrainingImage", dataset, "X");
            string y = Path.Combine(root, "TrainingImage", dataset, "Y");
            string ini = "[DAVS_V3]\r\nXPath=" + x + "\r\nYPath=" + y +
                "\r\nEpoch=" + epochs + "\r\nBatchSize=" + batchSize +
                "\r\nAugRatio=0" + profileSettings +
                "\r\nMaybeArea=0.3\r\nTempName=standalone" +
                "\r\nProcDir=" + Path.Combine(job, "progress") +
                "\r\nOutPath=" + Path.Combine(job, "predict_out") + "\r\n";
            Directory.CreateDirectory(Path.Combine(job, "progress"));
            File.WriteAllText(Path.Combine(job, "candidate.ini"), ini, Utf8);
            if (profilePath != null)
                File.Copy(profilePath, Path.Combine(job, "source-profile.ini"));
            File.WriteAllText(Path.Combine(job, "input.txt"),
                "DATASET=" + dataset + "\r\nSEED=" + seed +
                "\r\nSEED_SHA=" + Hash(Path.Combine(root, "TrainingSeed", seed)) +
                (profilePath == null ? "" : "\r\nPROFILE=" + profile +
                    "\r\nPROFILE_SHA=" + Hash(profilePath)) +
                "\r\nEPOCHS=" + epochs +
                "\r\nMAX_SAMPLES=" + maxSamples +
                "\r\nBATCH_SIZE=" + batchSize +
                "\r\nMAX_SECONDS=" + maxSeconds +
                (longManifest == null ? "" : "\r\nLONG_MANIFEST_SHA=" + longManifest) +
                "\r\nDEPLOYMENT=NEVER\r\n", Utf8);
            State(job, "PREPARED");
            return job;
        }

        internal static string ExistingJob(string root, string id)
        {
            GeneralTrainingJob.LocalName(id);
            if (!Regex.IsMatch(id, @"^\d{8}T\d{6}Z-[0-9a-f]{8}$"))
                throw new ArgumentException("Invalid candidate job ID");
            string job = Path.Combine(root, "TrainingCandidates", id);
            if (!Directory.Exists(job) || !File.Exists(Path.Combine(job, "input.txt")))
                throw new DirectoryNotFoundException("Candidate job does not exist");
            return job;
        }

        internal static string Status(string root, string id)
        {
            string job = ExistingJob(root, id);
            string state = Path.Combine(job, "state.txt");
            if (!File.Exists(state)) throw new InvalidDataException("Job state missing");
            return "TRAIN_GENERAL_JOB=" + id + " " +
                File.ReadAllText(state).Replace("\r", "").Replace("\n", " ");
        }

        internal static string RequestStop(string root, string id)
        {
            string job = ExistingJob(root, id);
            string state = File.ReadAllText(Path.Combine(job, "state.txt"));
            if (!state.StartsWith("STATE=RUNNING", StringComparison.Ordinal))
                throw new InvalidOperationException("Only RUNNING candidate can be stopped");
            File.WriteAllText(Path.Combine(job, "stop.request"), "STOP", Utf8);
            return "TRAIN_GENERAL_STOP=REQUESTED JOB=" + id;
        }


        internal static void SelfTest()
        {
            string root = Path.Combine(Path.GetTempPath(),
                "InspToolsCandidate-" + Guid.NewGuid().ToString("N"));
            try
            {
                foreach (string group in new[] { "X/OK", "X/NG", "Y/OK", "Y/NG" })
                {
                    string folder = Path.Combine(root, "TrainingImage", "test",
                        group.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(folder);
                    File.WriteAllBytes(Path.Combine(folder, "item.png"),
                        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                }
                Directory.CreateDirectory(Path.Combine(root, "TrainingSeed"));
                File.WriteAllBytes(Path.Combine(root, "TrainingSeed", "seed.h5"),
                    new byte[4096]);
                string job = Prepare(root, "test", "seed.h5");
                string id = Path.GetFileName(job);
                if (!File.ReadAllText(Path.Combine(job, "candidate.ini")).Contains("Epoch=1") ||
                    !File.ReadAllText(Path.Combine(job, "input.txt")).Contains("DEPLOYMENT=NEVER") ||
                    !Status(root, id).Contains("STATE=PREPARED"))
                    throw new InvalidDataException("Candidate plan or state invalid");
                bool rejected = false;
                try { RequestStop(root, id); }
                catch (InvalidOperationException) { rejected = true; }
                if (!rejected) throw new InvalidDataException("Unstarted stop was accepted");
                rejected = false;
                try { Status(root, ".."); }
                catch (ArgumentException) { rejected = true; }
                if (!rejected) throw new InvalidDataException("Job traversal accepted");
                string profileDir = Path.Combine(root, "TrainingProfiles");
                Directory.CreateDirectory(profileDir);
                File.WriteAllText(Path.Combine(profileDir, "valid.ini"),
                    "[DAVS_V3]\nImgWidth=256\nImgHeight=256\nUnetDiv=4\n" +
                    "LossFunction=1\nLR=0.001\nModelType=US\nOKBlank=1\nNGIgnore=0\n", Utf8);
                string profiled = Prepare(root, "test", "seed.h5", "valid.ini");
                string content = File.ReadAllText(Path.Combine(profiled, "candidate.ini"));
                if (!content.Contains("ImgHeight=256") ||
                    !content.Contains("UnetDiv=4") ||
                    !content.Contains("OKBlank=1") ||
                    !File.Exists(Path.Combine(profiled, "source-profile.ini")))
                    throw new InvalidDataException("Vendor profile not applied correctly");
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
