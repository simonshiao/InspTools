using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace InspTools.Standalone
{
    /// <summary>
    /// Product-owned, foreground-only DAVS candidate worker. Never deploys
    /// models and never opens a PLC or camera connection.
    /// </summary>
    internal static class GeneralTrainerExecutor
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private static string Hash(string file)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static string Quote(string value)
        {
            if (value.IndexOf('"') >= 0 || value.IndexOf('\r') >= 0 ||
                value.IndexOf('\n') >= 0)
                throw new ArgumentException("Invalid process argument");
            return "\"" + value + "\"";
        }

        private static string ReadInput(string job, string name)
        {
            string prefix = name + "=";
            foreach (string line in File.ReadAllLines(Path.Combine(job, "input.txt")))
                if (line.StartsWith(prefix, StringComparison.Ordinal))
                    return line.Substring(prefix.Length).Trim();
            throw new InvalidDataException("Candidate input is missing " + name);
        }

        private static void RequireNoLink(string path)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Candidate path is a link: " + path);
        }

        internal sealed class RunOutcome
        {
            internal string Status;
            internal int ExitCode;
            internal long ElapsedMs;
        }

        /// <summary>
        /// Event-driven pipe draining avoids Python stdout/stderr pipe deadlocks.
        /// Only the child that THIS method started may be terminated.
        /// </summary>
        private static RunOutcome Execute(ProcessStartInfo start, string job, int maxMs,
            string logPrefix = "", string standardInput = null)
        {
            string stdout = Path.Combine(job, logPrefix + "stdout.log");
            string stderr = Path.Combine(job, logPrefix + "stderr.log");
            string stop = Path.Combine(job, "stop.request");
            var outcome = new RunOutcome { Status = "FAILED", ExitCode = -1 };
            object sync = new object();
            var watch = Stopwatch.StartNew();
            using (var outLog = new StreamWriter(stdout, false, Utf8))
            using (var errLog = new StreamWriter(stderr, false, Utf8))
            using (var process = new Process())
            {
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = standardInput != null;
                start.CreateNoWindow = true;
                process.StartInfo = start;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null) lock (sync) { outLog.WriteLine(e.Data); outLog.Flush(); }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null) lock (sync) { errLog.WriteLine(e.Data); errLog.Flush(); }
                };
                if (!process.Start())
                    throw new InvalidOperationException("Candidate trainer did not start");
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (standardInput != null)
                {
                    process.StandardInput.Write(standardInput);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();
                }
                try
                {
                    while (true)
                    {
                        if (process.WaitForExit(250))
                        {
                            process.WaitForExit(); // complete async stream callbacks
                            outcome.ExitCode = process.ExitCode;
                            outcome.Status = process.ExitCode == 0 ? "EXITED" : "FAILED";
                            break;
                        }
                        if (File.Exists(stop))
                        {
                            outcome.Status = "STOPPED";
                            break;
                        }
                        if (watch.ElapsedMilliseconds >= maxMs)
                        {
                            outcome.Status = "TIMED_OUT";
                            break;
                        }
                    }
                }
                finally
                {
                    if (!process.HasExited)
                    {
                        process.Kill(); // Only own Python candidate process
                        if (!process.WaitForExit(5000))
                            throw new TimeoutException("Candidate subprocess did not stop");
                        process.WaitForExit();
                    }
                    outcome.ExitCode = process.ExitCode;
                    outcome.ElapsedMs = watch.ElapsedMilliseconds;
                }
            }
            return outcome;
        }

        private static string JsonString(Dictionary<string, object> obj, string name)
        {
            object value;
            if (!obj.TryGetValue(name, out value) || !(value is string))
                throw new InvalidDataException("Training result missing " + name);
            return (string)value;
        }

        private static bool ValidateResult(string job, string seedSha, string model,
            int expectedEpochs = 1)
        {
            string resultFile = Path.Combine(job, "training_result.json");
            if (!File.Exists(resultFile)) return false;
            var json = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(
                File.ReadAllText(resultFile, Encoding.UTF8));
            if (JsonString(json, "status") != "PASS" ||
                JsonString(json, "contract") != "SHUNT_RECOVERED_DAVS_V3_V1" ||
                !string.Equals(JsonString(json, "seed_sha256"), seedSha,
                    StringComparison.OrdinalIgnoreCase))
                return false;
            object count, devices;
            if (!json.TryGetValue("epochs_completed", out count) ||
                Convert.ToInt32(count) != expectedEpochs ||
                !json.TryGetValue("physical_gpus", out devices) ||
                !(devices is ICollection) || ((ICollection)devices).Count < 1)
                return false;
            string marker = Path.Combine(job, "candidate_success");
            if (!string.Equals(Path.GetFullPath(JsonString(json, "model")),
                    Path.GetFullPath(model), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(Path.GetFullPath(JsonString(json, "success_marker")),
                    Path.GetFullPath(marker), StringComparison.OrdinalIgnoreCase))
                return false;
            return File.Exists(marker) && File.Exists(model) &&
                new FileInfo(model).Length >= 4096 &&
                string.Equals(JsonString(json, "model_sha256"), Hash(model),
                    StringComparison.OrdinalIgnoreCase);
        }

        private static void RequireDeltaWrapped(string path, bool hdf5)
        {
            byte[] magic = { 1, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 64 };
            byte[] hdf = { 137, 72, 68, 70, 13, 10, 26, 10 };
            using (var stream = File.OpenRead(path))
            {
                if (stream.Length < 4096)
                    throw new InvalidDataException("Candidate artifact too small: " + path);
                for (int i = 0; i < magic.Length; i++)
                    if (stream.ReadByte() != magic[i])
                        throw new InvalidDataException("Delta 16-byte wrapper mismatch: " + path);
                if (hdf5)
                    for (int i = 0; i < hdf.Length; i++)
                        if (stream.ReadByte() != hdf[i])
                            throw new InvalidDataException("Candidate HDF5 signature mismatch");
            }
        }

        // Convert a successful training result only inside its own candidate directory.
        internal static string ConvertCandidate(string root, string id)
        {
            string job = GeneralCandidateRunner.ExistingJob(root, id);
            RequireNoLink(Path.Combine(root, "TrainingCandidates"));
            RequireNoLink(job);
            string state = File.ReadAllText(Path.Combine(job, "state.txt"));
            if (!state.StartsWith("STATE=SUCCEEDED", StringComparison.Ordinal))
                throw new InvalidOperationException("Only SUCCEEDED training job can convert");
            string h5 = Path.Combine(job, "candidate.h5");
            int expectedEpochs = int.Parse(ReadInput(job, "EPOCHS"));
            if (!ValidateResult(job, ReadInput(job, "SEED_SHA"), h5,
                    expectedEpochs))
                throw new InvalidDataException("Training result failed integrity verification");
            RequireDeltaWrapped(h5, true);
            string h5d = Path.Combine(job, "candidate.h5d");
            string pbd = Path.Combine(job, "candidate.pbd");
            string converter = Path.Combine(root, "Runtime", "ModelConverter");
            string worker = Path.Combine(converter, "ModelConverterWorker.exe");
            foreach (string needed in new[] { worker,
                Path.Combine(converter, "h5_to_pb_118.py"),
                Path.Combine(converter, "transform_graph.exe"),
                Path.Combine(root, "Runtime", "TrainingPython", "Python38", "python38.dll") })
                if (!File.Exists(needed))
                    throw new FileNotFoundException("App-local converter dependency missing", needed);
            if (!Directory.Exists(Path.Combine(converter, "PyFrozenLib")))
                throw new DirectoryNotFoundException("App-local PyFrozenLib missing");
            if (File.Exists(h5d) || File.Exists(pbd) ||
                File.Exists(Path.Combine(job, "conversion_result.json")))
                throw new IOException("Conversion artifacts already exist; refusing overwrite");

            using (var guard = new FileStream(Path.Combine(job, "convert.lock"),
                FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                // Recovered DAVS already produces wrapped HDF5; do not double-wrap.
                File.Copy(h5, h5d, false);
                RequireDeltaWrapped(h5d, true);
                if (!string.Equals(Hash(h5), Hash(h5d), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("H5D candidate copy checksum mismatch");

                var start = new ProcessStartInfo(worker, "--server --prewarm");
                start.WorkingDirectory = job;
                start.EnvironmentVariables["PATH"] = TrainingRuntimePaths.LocalGpuPath(
                    root, start.EnvironmentVariables["PATH"]);
                start.EnvironmentVariables["PYTHONNOUSERSITE"] = "1";
                start.EnvironmentVariables["TF_FORCE_GPU_ALLOW_GROWTH"] = "true";
                start.EnvironmentVariables["CUDA_VISIBLE_DEVICES"] = "0";
                string request = "PBD\t" + h5d + "\t" + pbd + "\nEXIT\n";
                RunOutcome outcome = Execute(start, job, 300000, "convert_", request);
                string stdout = File.ReadAllText(Path.Combine(job, "convert_stdout.log"));
                bool ready = stdout.Contains("SERVER_READY\tPBD=READY");
                bool success = outcome.Status == "EXITED" && outcome.ExitCode == 0 &&
                    ready && stdout.Contains("SERVER_RESULT\tPASS\tPBD\t") &&
                    File.Exists(pbd);
                if (success) RequireDeltaWrapped(pbd, false);
                success = success && !string.Equals(Hash(h5), Hash(pbd),
                    StringComparison.OrdinalIgnoreCase);
                var result = new Dictionary<string, object> {
                    {"status", success ? "PASS" : "FAIL"},
                    {"source_training_job", id},
                    {"source_h5_sha256", Hash(h5)},
                    {"h5d", h5d}, {"h5d_sha256", Hash(h5d)},
                    {"pbd", pbd},
                    {"pbd_sha256", File.Exists(pbd) ? Hash(pbd) : ""},
                    {"converter_exit_code", outcome.ExitCode},
                    {"elapsed_ms", outcome.ElapsedMs},
                    {"deployed", false},
                    {"runtime_readmodel_verified", false}
                };
                File.WriteAllText(Path.Combine(job, "conversion_result.json"),
                    new JavaScriptSerializer().Serialize(result), Utf8);
                return "TRAIN_CONVERT=" + (success ? "PASS" : "NOT_ACCEPTED") +
                    " JOB=" + id + " H5D_SHA=" + Hash(h5d) +
                    " PBD_SHA=" + (File.Exists(pbd) ? Hash(pbd) : "MISSING") +
                    " EXIT=" + outcome.ExitCode + " DEPLOYED=False";
            }
        }

        internal static string Run(string root, string id)
        {
            string job = GeneralCandidateRunner.ExistingJob(root, id);
            string jobs = Path.Combine(root, "TrainingCandidates");
            RequireNoLink(jobs);
            RequireNoLink(job);
            string state = File.ReadAllText(Path.Combine(job, "state.txt"));
            if (!state.StartsWith("STATE=PREPARED", StringComparison.Ordinal))
                throw new InvalidOperationException("Only PREPARED candidate may start once");
            string dataset = ReadInput(job, "DATASET");
            string seed = ReadInput(job, "SEED");
            string seedSha = ReadInput(job, "SEED_SHA");
            string preview = GeneralTrainingJob.Preflight(root, dataset, seed);
            int epochs = int.Parse(ReadInput(job, "EPOCHS"));
            int samples = int.Parse(ReadInput(job, "MAX_SAMPLES"));
            int seconds = int.Parse(ReadInput(job, "MAX_SECONDS"));
            int batch = epochs == 300 ? 4 : 1;
            bool longTrain = epochs == 300 && samples == 0 &&
                seconds == 7200 && batch == 4 &&
                dataset == "DIA_CCD1_S1_Train300" &&
                seed == "DIA_R002_vendor.h5";
            if (longTrain)
                throw new InvalidOperationException(
                    "Legacy 2026-09-14 300-epoch DAVS route is retired; " +
                    "rerun with isolated Shunt 2026-09-30 Loss6 backend");
            if (!(epochs == 1 && samples == 8 && seconds == 900) && !longTrain)
                throw new InvalidDataException("Training work exceeds its pinned epoch/data/time policy");
            if (longTrain && !string.Equals(ReadInput(job, "LONG_MANIFEST_SHA"),
                    GeneralTrainingJob.VerifyLongDataset(root),
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("CCD1 long training input manifest changed");
            if (longTrain && !string.Equals(ReadInput(job, "PROFILE"),
                    "DIA_R002_vendor.ini", StringComparison.Ordinal))
                throw new InvalidDataException("Expected the exact vendor CCD1 profile");
            // BATCH_SIZE added to new jobs; older 1-epoch smoke jobs still
            // explicitly use and enforce batch 1.
            string batchInput = null;
            try { batchInput = ReadInput(job, "BATCH_SIZE"); }
            catch (InvalidDataException) { if (longTrain) throw; }
            if (batchInput != null && int.Parse(batchInput) != batch)
                throw new InvalidDataException("Candidate batch size differs");
            string seedPath = Path.Combine(root, "TrainingSeed", seed);
            if (!string.Equals(seedSha, Hash(seedPath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Candidate seed changed since plan");
            string profileSnapshot = Path.Combine(job, "source-profile.ini");
            if (File.Exists(profileSnapshot) &&
                !string.Equals(ReadInput(job, "PROFILE_SHA"), Hash(profileSnapshot),
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Vendor training profile snapshot changed since plan");
            string model = Path.Combine(job, "candidate.h5");
            if (File.Exists(model) || File.Exists(Path.Combine(job, "training_result.json")))
                throw new InvalidOperationException("Candidate model already exists; refuse overwrite");
            string pythonDir = Path.Combine(root, "Runtime", "TrainingPython", "Python38");
            string python = Path.Combine(pythonDir, "python.exe");
            string trainer = Path.Combine(root, "Runtime", "TrainingRebuild", "train_compat.py");
            var start = new ProcessStartInfo(python,
                Quote(trainer) + " -i " + Quote(Path.Combine(job, "candidate.ini")) +
                " -m " + Quote(model) + " -v DAVS_V3 -r Train -e " + epochs +
                " -b " + batch +
                " --compat-seed-model " + Quote(seedPath) +
                " --compat-max-samples " + samples + " --ProcDir " +
                Quote(Path.Combine(job, "progress")));
            start.WorkingDirectory = job;
            start.EnvironmentVariables["PATH"] = TrainingRuntimePaths.LocalGpuPath(
                root, start.EnvironmentVariables["PATH"]);
            start.EnvironmentVariables["CUDA_VISIBLE_DEVICES"] = "0";
            start.EnvironmentVariables["TF_FORCE_GPU_ALLOW_GROWTH"] = "true";
            start.EnvironmentVariables["PYTHONNOUSERSITE"] = "1";
            start.EnvironmentVariables["PYTHONDONTWRITEBYTECODE"] = "1";
            using (var runLock = new FileStream(Path.Combine(job, "run.lock"),
                FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                File.WriteAllText(Path.Combine(job, "preflight.txt"), preview, Utf8);
                GeneralCandidateRunner.State(job, "RUNNING");
                RunOutcome outcome;
                try
                {
                    outcome = Execute(start, job, checked(seconds * 1000));
                    bool verified = outcome.Status == "EXITED" &&
                        ValidateResult(job, seedSha, model, epochs);
                    if (verified && longTrain)
                        verified = string.Equals(ReadInput(job, "LONG_MANIFEST_SHA"),
                            GeneralTrainingJob.VerifyLongDataset(root),
                            StringComparison.OrdinalIgnoreCase);
                    string finalState = verified ? "SUCCEEDED" :
                        outcome.Status == "EXITED" ? "FAILED" : outcome.Status;
                    GeneralCandidateRunner.State(job, finalState);
                    return "TRAIN_RESULT=" + (verified ? "PASS" : "NOT_ACCEPTED") +
                        " JOB=" + id + " STATE=" + finalState +
                        " EXIT=" + outcome.ExitCode + " ELAPSED_MS=" + outcome.ElapsedMs +
                        " DEPLOYED=False";
                }
                catch
                {
                    GeneralCandidateRunner.State(job, "FAILED");
                    throw;
                }
            }
        }

        internal static void SelfTest()
        {
            string temp = Path.Combine(Path.GetTempPath(),
                "InspToolsExecTest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            try
            {
                string system = Environment.GetEnvironmentVariable("ComSpec");
                if (string.IsNullOrWhiteSpace(system)) system = "cmd.exe";
                Func<ProcessStartInfo> sleeper = () =>
                    new ProcessStartInfo(system, "/d /c ping -n 4 127.0.0.1 >nul")
                    { WorkingDirectory = temp };
                RunOutcome timed = Execute(sleeper(), temp, 350);
                if (timed.Status != "TIMED_OUT")
                    throw new InvalidOperationException("Worker timeout did not kill subprocess");
                File.WriteAllText(Path.Combine(temp, "stop.request"), "STOP", Utf8);
                RunOutcome stopped = Execute(sleeper(), temp, 5000);
                if (stopped.Status != "STOPPED")
                    throw new InvalidOperationException("Stop request was not consumed");
                File.Delete(Path.Combine(temp, "stop.request"));
                RunOutcome cleanExit = Execute(new ProcessStartInfo(system,
                    "/d /c echo candidate-executor-smoke") { WorkingDirectory = temp },
                    temp, 5000);
                if (cleanExit.Status != "EXITED" || cleanExit.ExitCode != 0 ||
                    !File.ReadAllText(Path.Combine(temp, "stdout.log"))
                        .Contains("candidate-executor-smoke"))
                    throw new InvalidOperationException("Worker exit/logging did not pass");
                if (File.Exists(Path.Combine(temp, "candidate.h5")) ||
                    File.Exists(Path.Combine(temp, "training_result.json")))
                    throw new InvalidOperationException("Selftest created fake training output");

                // Synthetic report stays inside temporary selftest. Even a
                // successful process Exit 0 is NOT a trusted Train result.
                string model = Path.Combine(temp, "candidate.h5");
                string marker = Path.Combine(temp, "candidate_success");
                File.WriteAllBytes(model, new byte[4096]);
                var evidence = new Dictionary<string, object>
                {
                    {"status", "PASS"},
                    {"contract", "SHUNT_RECOVERED_DAVS_V3_V1"},
                    {"seed_sha256", new string('A', 64)},
                    {"epochs_completed", 1},
                    {"physical_gpus", new object[] { "Synthetic-Test-Only" }},
                    {"model", model},
                    {"success_marker", marker},
                    {"model_sha256", Hash(model)}
                };
                File.WriteAllText(Path.Combine(temp, "training_result.json"),
                    new JavaScriptSerializer().Serialize(evidence), Utf8);
                if (ValidateResult(temp, new string('A', 64), model))
                    throw new InvalidOperationException("Accepted report without success marker");
                File.WriteAllText(marker, "synthetic", Utf8);
                if (!ValidateResult(temp, new string('A', 64), model))
                    throw new InvalidOperationException("Valid synthetic evidence was rejected");
                File.AppendAllText(model, "modified");
                if (ValidateResult(temp, new string('A', 64), model))
                    throw new InvalidOperationException("Accepted tampered candidate model");
            }
            finally
            {
                if (Directory.Exists(temp)) Directory.Delete(temp, true);
            }
        }
    }
}
