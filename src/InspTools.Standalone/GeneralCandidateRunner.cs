using System;
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
        private static void State(string job, string value)
        {
            File.WriteAllText(Path.Combine(job, "state.txt"),
                "STATE=" + value + "\r\nUTC=" + DateTime.UtcNow.ToString("o"), Utf8);
        }
        internal static string Prepare(string root, string dataset, string seed)
        {
            GeneralTrainingJob.LocalName(dataset);
            GeneralTrainingJob.LocalName(seed);
            if (!seed.EndsWith(".h5", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("DAVS seed must be .h5");
            TrainingContracts.Check(root, dataset, TrainingFamily.General);
            string seedPath = Path.Combine(root, "TrainingSeed", seed);
            if (!File.Exists(seedPath) || new FileInfo(seedPath).Length < 4096)
                throw new FileNotFoundException("Candidate seed missing", seedPath);
            string jobs = Path.Combine(root, "TrainingCandidates");
            Directory.CreateDirectory(jobs);
            string id = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" +
                Guid.NewGuid().ToString("N").Substring(0, 8);
            string job = Path.Combine(jobs, id);
            Directory.CreateDirectory(job);
            string x = Path.Combine(root, "TrainingImage", dataset, "X");
            string y = Path.Combine(root, "TrainingImage", dataset, "Y");
            string ini = "[DAVS_V3]\r\nXPath=" + x + "\r\nYPath=" + y +
                "\r\nEpoch=1\r\nBatchSize=1\r\nLossFunction=6\r\nAugRatio=0" +
                "\r\nUnetDiv=8\r\nImgWidth=256\r\nImgHeight=512\r\nLR=0.001" +
                "\r\nMaybeArea=0.3\r\nModelType=US\r\nTempName=standalone" +
                "\r\nProcDir=" + Path.Combine(job, "progress") +
                "\r\nOutPath=" + Path.Combine(job, "predict_out") + "\r\n";
            Directory.CreateDirectory(Path.Combine(job, "progress"));
            File.WriteAllText(Path.Combine(job, "candidate.ini"), ini, Utf8);
            File.WriteAllText(Path.Combine(job, "input.txt"),
                "DATASET=" + dataset + "\r\nSEED=" + seed +
                "\r\nSEED_SHA=" + Hash(Path.Combine(root, "TrainingSeed", seed)) +
                "\r\nEPOCHS=1\r\nMAX_SAMPLES=8\r\nMAX_SECONDS=900" +
                "\r\nDEPLOYMENT=NEVER\r\n", Utf8);
            State(job, "PREPARED");
            return job;
        }

        private static string ExistingJob(string root, string id)
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
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
