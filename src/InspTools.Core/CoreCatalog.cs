using System.Collections.Generic;

namespace InspTools.Core
{
    public sealed class EngineStationSnapshot
    {
        internal EngineStationSnapshot(int station, bool stationEnabled, bool locateEnabled,
            bool enhanceEnabled, bool predictEnabled, bool prophetEnabled,
            string inspectionBackendState)
        {
            Station = station;
            StationEnabled = stationEnabled;
            LocateEnabled = locateEnabled;
            EnhanceEnabled = enhanceEnabled;
            PredictEnabled = predictEnabled;
            ProphetEnabled = prophetEnabled;
            InspectionBackendState = inspectionBackendState ?? string.Empty;
        }

        public int Station { get; private set; }
        public bool StationEnabled { get; private set; }
        public bool LocateEnabled { get; private set; }
        public bool EnhanceEnabled { get; private set; }
        public bool PredictEnabled { get; private set; }
        public bool ProphetEnabled { get; private set; }
        public string InspectionBackendState { get; private set; }
    }

    public sealed class EngineToolDescriptor
    {
        internal EngineToolDescriptor(string family, string section, int parameterCount)
        {
            Family = family ?? string.Empty;
            Section = section ?? string.Empty;
            ParameterCount = parameterCount;
        }

        public string Family { get; private set; }
        public string Section { get; private set; }
        public int ParameterCount { get; private set; }
    }

    public sealed class EngineModelDescriptor
    {
        internal EngineModelDescriptor(string family, bool enabled, string modelName,
            string sourcePath, string runtimePath, bool useTensorRt,
            int width, int height, int gpuId)
        {
            Family = family ?? string.Empty;
            Enabled = enabled;
            ModelName = modelName ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
            RuntimePath = runtimePath ?? string.Empty;
            UseTensorRt = useTensorRt;
            Width = width;
            Height = height;
            GpuId = gpuId;
        }

        public string Family { get; private set; }
        public bool Enabled { get; private set; }
        public string ModelName { get; private set; }
        public string SourcePath { get; private set; }
        public string RuntimePath { get; private set; }
        public bool UseTensorRt { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int GpuId { get; private set; }
        public bool SourceExists => !string.IsNullOrWhiteSpace(SourcePath) && System.IO.File.Exists(SourcePath);
        public bool RuntimeExists => !string.IsNullOrWhiteSpace(RuntimePath) && System.IO.File.Exists(RuntimePath);
    }
}
