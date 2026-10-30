using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DIAChip8Side.TemplateHost
{
    internal static class ProductRuntimePaths
    {
        public static string ResolveProductRoot(string projectRoot)
        {
            string env = Environment.GetEnvironmentVariable("CHAO8SIDE_ROOT");
            if (string.IsNullOrWhiteSpace(env))
                env = Environment.GetEnvironmentVariable("DIA_PRODUCT_ROOT");
            if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
                return Path.GetFullPath(env);
            if (!string.IsNullOrWhiteSpace(projectRoot) && Directory.Exists(projectRoot))
                return Path.GetFullPath(projectRoot);
            string appBase = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(appBase) && Directory.Exists(appBase))
                return Path.GetFullPath(appBase.TrimEnd(Path.DirectorySeparatorChar));
            throw new DirectoryNotFoundException("找不到 Chao8Side Product Root。");
        }

        public static string GetAIRuntimeControl(string projectRoot)
        {
            string path = Path.Combine(ResolveProductRoot(projectRoot), "Runtime", "AIRuntime", "ChaoAIRuntime.exe");
            if (!File.Exists(path))
                throw new FileNotFoundException("Unified AI Runtime control layer missing.", path);
            return path;
        }

        public static string GetTrainingPython(string projectRoot)
        {
            string root = ResolveProductRoot(projectRoot);
            string[] candidates =
            {
                Environment.GetEnvironmentVariable("CHAO8SIDE_TRAINING_PYTHON"),
                Path.Combine(root, "Runtime", "TrainingPython", "Python38", "python.exe"),
                Path.Combine(root, "Runtime", "TrainingPython", "python.exe")
            };
            string result = FirstExistingFile(candidates);
            if (!string.IsNullOrWhiteSpace(result)) return result;
            throw new FileNotFoundException("找不到產品內 Training Python。", candidates[1]);
        }

        public static string GetTrainingRebuildRoot(string projectRoot)
        {
            string root = ResolveProductRoot(projectRoot);
            string runtimeRoot = Path.Combine(root, "Runtime", "TrainingRebuild");
            if (Directory.Exists(runtimeRoot)) return runtimeRoot;
            string sourceRoot = Path.Combine(root, "src", "Chao8Side.Host", "TrainingRebuild");
            if (Directory.Exists(sourceRoot)) return sourceRoot;
            return runtimeRoot;
        }

        public static string GetTrainingScript(string projectRoot, string fileName)
        {
            string path = Path.Combine(GetTrainingRebuildRoot(projectRoot), fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("找不到 Chao8Side Training runtime script。", path);
            return path;
        }

        public static void ApplyTrainingEnvironment(ProcessStartInfo startInfo, string projectRoot, int? gpu = null)
        {
            string root = ResolveProductRoot(projectRoot);
            List<string> paths = new List<string>
            {
                Path.Combine(root, "Runtime", "TrainingPython", "Python38"),
                Path.Combine(root, "Runtime", "TrainingPython", "Python38", "DLLs"),
                Path.Combine(root, "Runtime", "CUDA11_118"),
                Path.Combine(root, "Runtime", "ModelConverter", "TensorRT861")
            };
            string ptxas = Environment.GetEnvironmentVariable("CHAO8SIDE_PTXAS_DIR");
            if (!string.IsNullOrWhiteSpace(ptxas)) paths.Add(ptxas);
            string existing = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            IEnumerable<string> merged = paths.Where(Directory.Exists)
                .Concat(existing.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            startInfo.EnvironmentVariables["PATH"] = string.Join(";", merged);
            startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            startInfo.EnvironmentVariables["TF_CPP_MIN_LOG_LEVEL"] = "1";
            startInfo.EnvironmentVariables["TF_FORCE_GPU_ALLOW_GROWTH"] = "true";
            startInfo.EnvironmentVariables["CHAO8SIDE_ROOT"] = root;
            if (gpu.HasValue) startInfo.EnvironmentVariables["CUDA_VISIBLE_DEVICES"] = gpu.Value.ToString();
        }

        private static string FirstExistingFile(IEnumerable<string> candidates)
        {
            foreach (string candidate in candidates.Where(item => !string.IsNullOrWhiteSpace(item)))
            {
                string full = Path.GetFullPath(candidate);
                if (File.Exists(full)) return full;
            }
            return null;
        }
    }
}
