using System;
using System.IO;

namespace InspTools.Standalone
{
    internal static class TrainingRuntimePaths
    {
        internal static string LocalGpuPath(string appBase, string originalPath)
        {
            string runtime = Path.Combine(appBase, "Runtime", "TrainingPython");
            string gpu = Path.Combine(runtime, "GpuRuntime");
            string python = Path.Combine(runtime, "Python38");
            if (!Directory.Exists(gpu) || !File.Exists(Path.Combine(python, "python.exe")))
                throw new DirectoryNotFoundException("App-local GPU Python runtime missing");
            return string.Join(Path.PathSeparator.ToString(), new[] {
                gpu, python, Path.Combine(python, "Scripts"), originalPath ?? ""
            });
        }
    }
}
