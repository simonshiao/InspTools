using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using InspTools.Core;

namespace InspTools.SDK
{
    public static class InspToolsRuntime
    {
        public static IInspToolsRuntime Create(InspToolsRuntimeOptions options)
            => new RuntimeAdapter(options ?? throw new ArgumentNullException(nameof(options)));
    }

    internal sealed class RuntimeAdapter : IInspToolsRuntime
    {
        public RuntimeAdapter(InspToolsRuntimeOptions options)
        {
            Options = CloneAndValidate(options);
        }

        public InspToolsRuntimeOptions Options { get; private set; }

        public IInspToolsSession OpenSession(InspToolsSessionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            return new SessionAdapter(new SessionEngine(
                Options.RecipeRoot,
                Options.ModelRoot,
                options.RecipeName,
                options.Station,
                options.ImagePath,
                options.Editable));
        }

        public void Dispose() { }

        private static InspToolsRuntimeOptions CloneAndValidate(InspToolsRuntimeOptions options)
        {
            string recipeRoot = RequireDirectory(options.RecipeRoot, "RecipeRoot");
            string runtimeRoot = NormalizeOptionalDirectory(options.RuntimeRoot, "RuntimeRoot");
            string modelRoot = NormalizeOptionalDirectory(options.ModelRoot, "ModelRoot");

            return new InspToolsRuntimeOptions
            {
                RuntimeRoot = runtimeRoot,
                RecipeRoot = recipeRoot,
                ModelRoot = modelRoot,
                EnableAi = options.EnableAi
            };
        }

        private static string RequireDirectory(string path, string name)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(name + " is required.", name);
            string full = Path.GetFullPath(path);
            if (!Directory.Exists(full)) throw new DirectoryNotFoundException(name + " was not found: " + full);
            return full;
        }

        private static string NormalizeOptionalDirectory(string path, string name)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            string full = Path.GetFullPath(path);
            if (!Directory.Exists(full)) throw new DirectoryNotFoundException(name + " was not found: " + full);
            return full;
        }
    }

    internal sealed class SessionAdapter : IInspToolsSession
    {
        private readonly SessionEngine engine;

        public SessionAdapter(SessionEngine engine)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public string RecipeName => engine.RecipeName;
        public int Station => engine.Station;
        public string ImagePath => engine.ImagePath;
        public bool Editable => engine.Editable;
        public bool IsDirty => engine.IsDirty;

        public RecipeParameterSnapshot GetRecipeSnapshot()
        {
            EngineRecipeSnapshot source = engine.GetRecipeSnapshot();
            var entries = new List<RecipeParameterEntry>(source.Entries.Count);
            foreach (EngineRecipeEntry entry in source.Entries)
            {
                entries.Add(new RecipeParameterEntry
                {
                    Section = entry.Section,
                    Key = entry.Key,
                    Value = entry.Value,
                    Order = entry.Order
                });
            }

            return new RecipeParameterSnapshot
            {
                RecipeName = source.RecipeName,
                Station = source.Station,
                SourcePath = source.SourcePath,
                SourceEncoding = source.SourceEncoding,
                Sha256 = source.Sha256,
                Entries = new ReadOnlyCollection<RecipeParameterEntry>(entries)
            };
        }

        public StationSnapshot GetStationSnapshot()
        {
            EngineStationSnapshot source = engine.GetStationSnapshot();
            return new StationSnapshot
            {
                Station = source.Station,
                StationEnabled = source.StationEnabled,
                LocateEnabled = source.LocateEnabled,
                EnhanceEnabled = source.EnhanceEnabled,
                PredictEnabled = source.PredictEnabled,
                ProphetEnabled = source.ProphetEnabled,
                InspectionBackendState = source.InspectionBackendState
            };
        }

        public IReadOnlyList<ToolDescriptor> GetToolCatalog()
        {
            IReadOnlyList<EngineToolDescriptor> source = engine.GetToolCatalog();
            var result = new List<ToolDescriptor>(source.Count);
            foreach (EngineToolDescriptor item in source)
                result.Add(new ToolDescriptor { Family = item.Family, Section = item.Section, ParameterCount = item.ParameterCount });
            return new ReadOnlyCollection<ToolDescriptor>(result);
        }

        public IReadOnlyList<RecipeParameterEntry> GetSectionParameters(string section)
        {
            IReadOnlyList<EngineRecipeEntry> source = engine.GetSectionParameters(section);
            var result = new List<RecipeParameterEntry>(source.Count);
            foreach (EngineRecipeEntry entry in source)
            {
                result.Add(new RecipeParameterEntry
                {
                    Section = entry.Section,
                    Key = entry.Key,
                    Value = entry.Value,
                    Order = entry.Order
                });
            }
            return new ReadOnlyCollection<RecipeParameterEntry>(result);
        }

        public IReadOnlyList<ModelDescriptor> GetModelCatalog()
        {
            IReadOnlyList<EngineModelDescriptor> source = engine.GetModelCatalog();
            var result = new List<ModelDescriptor>(source.Count);
            foreach (EngineModelDescriptor item in source)
            {
                result.Add(new ModelDescriptor
                {
                    Family = item.Family,
                    Enabled = item.Enabled,
                    ModelName = item.ModelName,
                    SourcePath = item.SourcePath,
                    RuntimePath = item.RuntimePath,
                    UseTensorRt = item.UseTensorRt,
                    Width = item.Width,
                    Height = item.Height,
                    GpuId = item.GpuId,
                    SourceExists = item.SourceExists,
                    RuntimeExists = item.RuntimeExists
                });
            }
            return new ReadOnlyCollection<ModelDescriptor>(result);
        }

        public string GetParameter(string section, string key) => engine.GetParameter(section, key);
        public string GetParameter(string key) => engine.GetParameter(key);
        public void SetParameter(string section, string key, string value) => engine.SetParameter(section, key, value);
        public void SetParameter(string key, string value) => engine.SetParameter(key, value);
        public void SetImage(string path) => engine.SetImage(path);
        public void Save() => engine.Save();
        public void Cancel() => engine.Cancel();
        public void Dispose() => engine.Dispose();

        public InspectionResult Inspect()
        {
            EngineInspection result = engine.Inspect();
            return new InspectionResult
            {
                Bin = result.Bin,
                LocateSucceeded = result.LocateSucceeded,
                Message = result.Message
            };
        }

        public ToolPreviewResult TestTool(string toolId)
        {
            EnginePreview result = engine.TestTool(toolId);
            return new ToolPreviewResult
            {
                ToolId = result.ToolId,
                Success = result.Success,
                Value = result.Value,
                Message = result.Message
            };
        }
    }
}
