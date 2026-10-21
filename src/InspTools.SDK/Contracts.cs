using System;
using System.Collections.Generic;

namespace InspTools.SDK
{
    public sealed class InspToolsRuntimeOptions
    {
        public string RuntimeRoot { get; set; }
        public string RecipeRoot { get; set; }
        public string ModelRoot { get; set; }
        public bool EnableAi { get; set; }
    }

    public sealed class InspToolsSessionOptions
    {
        public string RecipeName { get; set; }
        public int Station { get; set; }
        public string ImagePath { get; set; }
        public bool Editable { get; set; }
    }

    public sealed class InspectionResult
    {
        public string Bin { get; internal set; }
        public bool LocateSucceeded { get; internal set; }
        public string Message { get; internal set; }
    }

    public sealed class ToolPreviewResult
    {
        public string ToolId { get; internal set; }
        public bool Success { get; internal set; }
        public double Value { get; internal set; }
        public string Message { get; internal set; }
    }

    public sealed class RecipeParameterEntry
    {
        public string Section { get; internal set; }
        public string Key { get; internal set; }
        public string Value { get; internal set; }
        public int Order { get; internal set; }
    }

    public sealed class RecipeParameterSnapshot
    {
        public string RecipeName { get; internal set; }
        public int Station { get; internal set; }
        public string SourcePath { get; internal set; }
        public string SourceEncoding { get; internal set; }
        public string Sha256 { get; internal set; }
        public IReadOnlyList<RecipeParameterEntry> Entries { get; internal set; }
    }

    public sealed class StationSnapshot
    {
        public int Station { get; internal set; }
        public bool StationEnabled { get; internal set; }
        public bool LocateEnabled { get; internal set; }
        public bool EnhanceEnabled { get; internal set; }
        public bool PredictEnabled { get; internal set; }
        public bool ProphetEnabled { get; internal set; }
        public string InspectionBackendState { get; internal set; }
    }

    public sealed class ToolDescriptor
    {
        public string Family { get; internal set; }
        public string Section { get; internal set; }
        public int ParameterCount { get; internal set; }
    }

    public sealed class ModelDescriptor
    {
        public string Family { get; internal set; }
        public bool Enabled { get; internal set; }
        public string ModelName { get; internal set; }
        public string SourcePath { get; internal set; }
        public string RuntimePath { get; internal set; }
        public bool UseTensorRt { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int GpuId { get; internal set; }
        public bool SourceExists { get; internal set; }
        public bool RuntimeExists { get; internal set; }
    }

    public interface IInspToolsRuntime : IDisposable
    {
        InspToolsRuntimeOptions Options { get; }
        IInspToolsSession OpenSession(InspToolsSessionOptions options);
    }

    public interface IInspToolsSession : IDisposable
    {
        string RecipeName { get; }
        int Station { get; }
        string ImagePath { get; }
        bool Editable { get; }
        bool IsDirty { get; }
        RecipeParameterSnapshot GetRecipeSnapshot();
        StationSnapshot GetStationSnapshot();
        IReadOnlyList<ToolDescriptor> GetToolCatalog();
        IReadOnlyList<RecipeParameterEntry> GetSectionParameters(string section);
        IReadOnlyList<ModelDescriptor> GetModelCatalog();
        string GetParameter(string section, string key);
        string GetParameter(string key);
        void SetParameter(string section, string key, string value);
        void SetParameter(string key, string value);
        void SetImage(string path);
        InspectionResult Inspect();
        ToolPreviewResult TestTool(string toolId);
        void Save();
        void Cancel();
    }
}
