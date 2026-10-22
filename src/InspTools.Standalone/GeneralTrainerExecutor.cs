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
        private static RunOutcome Execute(ProcessStartInfo start, string job, int maxMs)
        {
            string stdout = Path.Combine(job, "stdout.log");
            string stderr = Path.Combine(job, "stderr.log");
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

        private static bool ValidateResult(string job, string seedSha, string model)
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
                Convert.ToInt32(count) < 1 ||
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
            string seedPath = Path.Combine(root, "TrainingSeed", seed);
            if (!string.Equals(seedSha, Hash(seedPath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Candidate seed changed since plan");
            string model = Path.Combine(job, "candidate.h5");
            if (File.Exists(model) || File.Exists(Path.Combine(job, "training_result.json")))
                throw new InvalidOperationException("Candidate model already exists; refuse overwrite");
            string pythonDir = Path.Combine(root, "Runtime", "TrainingPython", "Python38");
            string python = Path.Combine(pythonDir, "python.exe");
            string trainer = Path.Combine(root, "Runtime", "TrainingRebuild", "train_compat.py");
            var start = new ProcessStartInfo(python,
                Quote(trainer) + " -i " + Quote(Path.Combine(job, "candidate.ini")) +
                " -m " + Quote(model) + " -v DAVS_V3 -r Train -e 1 -b 1" +
                " --compat-seed-model " + Quote(seedPath) +
                " --compat-max-samples 8 --ProcDir " +
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
                    outcome = Execute(start, job, 900000);
                    bool verified = outcome.Status == "EXITED" &&
                        ValidateResult(job, seedSha, model);
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
