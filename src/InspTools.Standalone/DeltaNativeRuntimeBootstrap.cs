using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace InspTools.Standalone
{
    /// <summary>
    /// Optional app-local Delta native runtime. Never fetches ShuntVC paths,
    /// installs NVIDIA drivers, changes persistent PATH or converts models.
    /// When absent, original InspTools UI / Recipe features still launch.
    /// </summary>
    internal static class DeltaNativeRuntimeBootstrap
    {
        private const string PinnedDeltaSha =
            "3B056989D395E83B73AA2CD82EF3CB37DDA6DD8C50541DFD051636CA36DB5B38";
        private const uint SearchDllLoadDir = 0x00000100;
        private const uint SearchUserDirs = 0x00000400;
        private const uint SearchDefaultDirs = 0x00001000;
        private static bool _initialized;
        private static readonly List<IntPtr> RegisteredDirs = new List<IntPtr>();
        private static IntPtr _library;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr AddDllDirectory(string path);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string name, IntPtr reserved, uint flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr handle, string procedure);

        /// <summary>
        /// The only supported runtime root is AppBase/Runtime. Do not silently
        /// redirect to machine projects, original Training or global CUDA.
        /// </summary>
        internal static bool InitializeIfPresent()
        {
            if (_initialized) return _library != IntPtr.Zero;
            _initialized = true;
            string appRoot = AppDomain.CurrentDomain.BaseDirectory;
            string runtime = Path.Combine(appRoot, "Runtime");
            string deltaDir = Path.Combine(runtime, "DeltaAIProcess");
            string delta = Path.Combine(deltaDir, "DeltaAIProcessDll.dll");
            if (!File.Exists(delta)) return false;

            string actual;
            using (SHA256 sha = SHA256.Create())
            using (FileStream fs = File.OpenRead(delta))
                actual = BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "");
            if (!actual.Equals(PinnedDeltaSha, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("App-local DeltaAIProcessDll.dll SHA256 不符，停止 AI 載入");

            string cudaDir = Path.Combine(runtime, "CUDA11_118");
            foreach (string path in new[] { deltaDir, cudaDir,
                Path.Combine(runtime, "ModelConverter", "TensorRT861") })
            {
                if (!Directory.Exists(path)) continue;
                IntPtr cookie = AddDllDirectory(path);
                if (cookie == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to add private native DLL directory: " + path);
                RegisteredDirs.Add(cookie);
            }
            // Resolves the DLL and its imported OpenCV/CUDA/TensorRT libraries
            // only through the executable, system and scoped added directories.
            _library = LoadLibraryEx(delta, IntPtr.Zero,
                SearchDllLoadDir | SearchUserDirs | SearchDefaultDirs);
            if (_library == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "台達原生 Runtime 不完整。請檢查 AppBase/Runtime 的 DLL");
            foreach (string api in new[] { "CreateInstance", "DisposeInstance",
                "ReadModel", "SetGPU" })
            {
                if (GetProcAddress(_library, api) == IntPtr.Zero)
                    throw new EntryPointNotFoundException("台達原生 DLL 缺少：" + api);
            }
            return true;
        }
    }
}
