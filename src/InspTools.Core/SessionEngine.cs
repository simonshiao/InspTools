using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace InspTools.Core
{
    public sealed class SessionEngine : IDisposable
    {
        private readonly EngineRecipeSnapshot recipeSnapshot;
        private readonly string modelRoot;
        private Dictionary<string, string> committed;
        private Dictionary<string, string> working;
        private bool disposed;

        public SessionEngine(string recipeRoot, string modelRoot, string recipeName, int station, string imagePath, bool editable)
        {
            if (string.IsNullOrWhiteSpace(recipeRoot)) throw new ArgumentException("Recipe root is required.", nameof(recipeRoot));
            if (string.IsNullOrWhiteSpace(recipeName)) throw new ArgumentException("Recipe name is required.", nameof(recipeName));
            if (station < 1 || station > 99) throw new ArgumentOutOfRangeException(nameof(station));

            string stationPath = ResolveStationPath(recipeRoot, recipeName.Trim(), station);
            RecipeDocument document = RecipeDocument.Load(stationPath);

            RecipeName = recipeName.Trim();
            Station = station;
            Editable = editable;
            this.modelRoot = string.IsNullOrWhiteSpace(modelRoot) ? string.Empty : Path.GetFullPath(modelRoot);
            recipeSnapshot = new EngineRecipeSnapshot(
                RecipeName,
                Station,
                stationPath,
                document.EncodingName,
                CoreHash.Sha256(document.SourceBytes),
                document.Entries);

            committed = BuildState(document.Entries);
            working = Clone(committed);
            ImagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(imagePath)) SetImage(imagePath);
        }

        public string RecipeName { get; private set; }
        public int Station { get; private set; }
        public bool Editable { get; private set; }
        public string ImagePath { get; private set; }
        public bool IsDirty => !DictionaryEqual(committed, working);

        public EngineRecipeSnapshot GetRecipeSnapshot()
        {
            ThrowIfDisposed();
            return recipeSnapshot;
        }

        public EngineStationSnapshot GetStationSnapshot()
        {
            ThrowIfDisposed();
            return new EngineStationSnapshot(
                Station,
                ReadBoolean("Main", "StationON"),
                ReadBoolean("Main", "LocateToolON"),
                ReadBoolean("Main", "AIEnhanceToolON"),
                ReadBoolean("Main", "AIPredictToolON"),
                ReadBoolean("Main", "AIProphetToolON"),
                "Deferred: Recipe/Tool/Model Core ready; recovered inspection execution is not connected in IR2-02.");
        }

        public IReadOnlyList<EngineToolDescriptor> GetToolCatalog()
        {
            ThrowIfDisposed();
            var result = recipeSnapshot.Entries
                .GroupBy(e => e.Section ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .Select(g => new EngineToolDescriptor(MapToolFamily(g.Key), g.Key, g.Count()))
                .Where(x => !string.Equals(x.Family, "RecipeMeta", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Family, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Section, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return result.AsReadOnly();
        }

        public IReadOnlyList<EngineRecipeEntry> GetSectionParameters(string section)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(section)) throw new ArgumentException("Section is required.", nameof(section));

            string target = section.Trim();
            var result = recipeSnapshot.Entries
                .Where(e => string.Equals(e.Section, target, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Order)
                .Select(e => new EngineRecipeEntry(e.Section, e.Key, GetParameter(e.Section, e.Key), e.Order))
                .ToList();

            if (result.Count == 0)
                throw new KeyNotFoundException("Recipe section was not found: [" + target + "].");

            return result.AsReadOnly();
        }

        public IReadOnlyList<EngineModelDescriptor> GetModelCatalog()
        {
            ThrowIfDisposed();
            var result = new List<EngineModelDescriptor>();

            string enhanceName = GetOptionalParameter("AIEnhanceTool", "ModelName_A");
            bool enhanceTensorRt = ReadBoolean("AIEnhanceTool", "UseTensorRT");
            result.Add(CreateModelDescriptor(
                "Enhance",
                ReadBoolean("Main", "AIEnhanceToolON"),
                enhanceName,
                enhanceTensorRt ? ".Trt" : ".pbd",
                enhanceTensorRt,
                ReadInt32("AIEnhanceTool", "Width_A"),
                ReadInt32("AIEnhanceTool", "Height_A"),
                ReadInt32("AIEnhanceTool", "GPUID")));

            string locateStem = GetOptionalParameter("Locate", "AILocateModelName");
            bool locateTensorRt = ReadBoolean("Locate", "EnableTensorRT");
            result.Add(CreateModelDescriptor(
                "Locate",
                ReadBoolean("Locate", "EnableAILocate"),
                locateStem,
                locateTensorRt ? ".ltrt" : ".lh5d",
                locateTensorRt,
                ReadInt32("Locate", "ModelWidth"),
                ReadInt32("Locate", "ModelHeight"),
                0,
                ".lh5d"));

            string prophetName = GetOptionalParameter("AIProphetTool", "ModelName");
            result.Add(CreateModelDescriptor(
                "Prophet",
                ReadBoolean("Main", "AIProphetToolON"),
                prophetName,
                ".trt",
                true,
                0,
                0,
                0));

            return result.AsReadOnly();
        }

        public void SetImage(string path)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Image path is required.", nameof(path));
            string full = Path.GetFullPath(path);
            if (!File.Exists(full)) throw new FileNotFoundException("Inspection image not found.", full);
            ImagePath = full;
        }

        public string GetParameter(string section, string key)
        {
            ThrowIfDisposed();
            string identity = Identity(section, key);
            string value;
            if (!working.TryGetValue(identity, out value))
                throw new KeyNotFoundException("Recipe parameter was not found: [" + section + "] " + key + ".");
            return value;
        }

        // IR2-01 相容入口。只有 key 在整份 Recipe 唯一時才允許使用。
        public string GetParameter(string key)
        {
            ThrowIfDisposed();
            EngineRecipeEntry entry = FindUniqueByKey(key);
            return GetParameter(entry.Section, entry.Key);
        }

        public void SetParameter(string section, string key, string value)
        {
            ThrowIfDisposed();
            if (!Editable) throw new InvalidOperationException("Session is read-only.");
            string identity = Identity(section, key);
            if (!working.ContainsKey(identity))
                throw new KeyNotFoundException("Recipe parameter was not found: [" + section + "] " + key + ".");
            if ((value ?? string.Empty).IndexOfAny(new[] { '\r', '\n' }) >= 0)
                throw new ArgumentException("Recipe parameter value must be a single line.", nameof(value));
            working[identity] = value ?? string.Empty;
        }

        public void SetParameter(string key, string value)
        {
            ThrowIfDisposed();
            EngineRecipeEntry entry = FindUniqueByKey(key);
            SetParameter(entry.Section, entry.Key, value);
        }

        public EnginePreview TestTool(string toolId)
        {
            ThrowIfDisposed();
            string section;
            string key;
            ResolveToolParameter(toolId, out section, out key);
            string raw = GetParameter(section, key);

            double value;
            bool numeric = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                           double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);

            return new EnginePreview(
                section + "/" + key,
                numeric,
                numeric ? value : 0,
                numeric
                    ? "IR2-02 Recipe Core parameter preview PASS; recovered inspection backend is not connected yet."
                    : "IR2-02 Recipe Core loaded the parameter, but this value is not numeric; inspection backend is not connected yet.");
        }

        public EngineInspection Inspect()
        {
            ThrowIfDisposed();
            bool locateEnabled = ReadBoolean("Main", "LocateToolON");
            return new EngineInspection(
                "NOT_RUN",
                locateEnabled,
                "IR2-02 Recipe Core is active. Recovered inspection execution is intentionally deferred to the next Core parity slice.");
        }

        public void Save()
        {
            ThrowIfDisposed();
            if (!Editable) throw new InvalidOperationException("Session is read-only.");
            // IR2-02 僅提交 session 記憶體工作副本，不寫回 production Recipe。
            committed = Clone(working);
        }

        public void Cancel()
        {
            ThrowIfDisposed();
            working = Clone(committed);
        }

        public void Dispose()
        {
            if (disposed) return;
            if (Editable) working = Clone(committed);
            disposed = true;
        }

        private EngineRecipeEntry FindUniqueByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Parameter key is required.", nameof(key));
            List<EngineRecipeEntry> matches = recipeSnapshot.Entries
                .Where(e => string.Equals(e.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count == 0) throw new KeyNotFoundException("Recipe parameter was not found: " + key + ".");
            if (matches.Count != 1) throw new InvalidDataException("Recipe parameter key is ambiguous across sections: " + key + ". Use section + key.");
            return matches[0];
        }

        private void ResolveToolParameter(string toolId, out string section, out string key)
        {
            string value = (toolId ?? string.Empty).Trim();
            int split = value.IndexOf('/');
            if (split > 0 && split < value.Length - 1)
            {
                section = value.Substring(0, split).Trim();
                key = value.Substring(split + 1).Trim();
                GetParameter(section, key);
                return;
            }

            if (working.ContainsKey(Identity("Locate", "LocateAngleLimit")))
            {
                section = "Locate";
                key = "LocateAngleLimit";
                return;
            }

            EngineRecipeEntry first = recipeSnapshot.Entries.FirstOrDefault();
            if (first == null) throw new InvalidDataException("Recipe contains no parameter entries.");
            section = first.Section;
            key = first.Key;
        }

        private bool ReadBoolean(string section, string key)
        {
            string value;
            if (!working.TryGetValue(Identity(section, key), out value)) return false;
            bool parsed;
            return bool.TryParse(value, out parsed) && parsed;
        }

        private int ReadInt32(string section, string key)
        {
            string value = GetOptionalParameter(section, key);
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : 0;
        }

        private string GetOptionalParameter(string section, string key)
        {
            string value;
            return working.TryGetValue(Identity(section, key), out value) ? value : string.Empty;
        }

        private EngineModelDescriptor CreateModelDescriptor(
            string family,
            bool enabled,
            string modelName,
            string runtimeExtension,
            bool useTensorRt,
            int width,
            int height,
            int gpuId,
            string sourceExtensionWhenMissing = null)
        {
            string safeName = (modelName ?? string.Empty).Trim();
            if (safeName.Length == 0)
                return new EngineModelDescriptor(family, enabled, string.Empty, string.Empty, string.Empty, useTensorRt, width, height, gpuId);

            string sourceFile = Path.GetFileName(safeName);
            if (!string.IsNullOrEmpty(sourceExtensionWhenMissing) && string.IsNullOrEmpty(Path.GetExtension(sourceFile)))
                sourceFile += sourceExtensionWhenMissing;

            string stem = Path.GetFileNameWithoutExtension(sourceFile);
            string sourcePath = string.IsNullOrEmpty(modelRoot) ? string.Empty : Path.Combine(modelRoot, sourceFile);
            string runtimePath = string.IsNullOrEmpty(modelRoot) ? string.Empty : Path.Combine(modelRoot, stem + runtimeExtension);
            return new EngineModelDescriptor(family, enabled, safeName, sourcePath, runtimePath, useTensorRt, width, height, gpuId);
        }

        private static string MapToolFamily(string section)
        {
            if (string.Equals(section, "Locate", StringComparison.OrdinalIgnoreCase) ||
                section.StartsWith("VLocateTools", StringComparison.OrdinalIgnoreCase) ||
                section.StartsWith("HLocateTools", StringComparison.OrdinalIgnoreCase)) return "Locate";
            if (section.StartsWith("AdvLocateTools", StringComparison.OrdinalIgnoreCase)) return "AdvLocate";
            if (section.StartsWith("AdvMeasure", StringComparison.OrdinalIgnoreCase) ||
                section.StartsWith("MeaWidthTools", StringComparison.OrdinalIgnoreCase) ||
                section.StartsWith("MeaHeightTools", StringComparison.OrdinalIgnoreCase)) return "Measure";
            if (section.StartsWith("BinaryTools", StringComparison.OrdinalIgnoreCase)) return "Binary";
            if (section.StartsWith("SmoothTools", StringComparison.OrdinalIgnoreCase)) return "Smooth";
            if (section.StartsWith("BlockAvgTools", StringComparison.OrdinalIgnoreCase)) return "BlockAvg";
            if (section.StartsWith("DifferenceTools", StringComparison.OrdinalIgnoreCase)) return "Difference";
            if (section.StartsWith("LineAnalysisTool", StringComparison.OrdinalIgnoreCase)) return "LineAnalysis";
            if (section.StartsWith("MatchTools", StringComparison.OrdinalIgnoreCase)) return "Match";
            if (section.StartsWith("MaskTools", StringComparison.OrdinalIgnoreCase)) return "Mask";
            if (string.Equals(section, "AIEnhanceTool", StringComparison.OrdinalIgnoreCase)) return "AIEnhance";
            if (string.Equals(section, "AIProphetTool", StringComparison.OrdinalIgnoreCase)) return "AIProphet";
            if (string.Equals(section, "AIPredictTool", StringComparison.OrdinalIgnoreCase)) return "AIPredict";
            if (string.Equals(section, "FontTool", StringComparison.OrdinalIgnoreCase)) return "Font";
            if (string.Equals(section, "IRTool", StringComparison.OrdinalIgnoreCase)) return "IR";
            return "RecipeMeta";
        }

        private static string ResolveStationPath(string recipeRoot, string recipeName, int station)
        {
            string root = Path.GetFullPath(recipeRoot);
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("Recipe root was not found: " + root);

            string direct = Path.Combine(root, "station" + station + ".ini");
            string recipeFolder = string.Equals(new DirectoryInfo(root).Name, recipeName, StringComparison.OrdinalIgnoreCase)
                ? root
                : Path.Combine(root, recipeName);
            string nested = Path.Combine(recipeFolder, "station" + station + ".ini");

            if (File.Exists(nested)) return Path.GetFullPath(nested);
            if (File.Exists(direct)) return Path.GetFullPath(direct);
            throw new FileNotFoundException("Recipe station file was not found.", nested);
        }

        private static Dictionary<string, string> BuildState(IReadOnlyList<EngineRecipeEntry> entries)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (EngineRecipeEntry entry in entries)
            {
                string identity = Identity(entry.Section, entry.Key);
                if (result.ContainsKey(identity))
                    throw new InvalidDataException("Recipe parameter is ambiguous because it appears more than once: [" + entry.Section + "] " + entry.Key + ".");
                result.Add(identity, entry.Value);
            }
            return result;
        }

        private static string Identity(string section, string key)
        {
            if (string.IsNullOrWhiteSpace(section)) throw new ArgumentException("Parameter section is required.", nameof(section));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Parameter key is required.", nameof(key));
            return section.Trim() + "\u001f" + key.Trim();
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SessionEngine));
        }

        private static Dictionary<string, string> Clone(Dictionary<string, string> source)
            => new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);

        private static bool DictionaryEqual(Dictionary<string, string> left, Dictionary<string, string> right)
        {
            if (left.Count != right.Count) return false;
            foreach (KeyValuePair<string, string> pair in left)
            {
                string value;
                if (!right.TryGetValue(pair.Key, out value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }
    }

    public sealed class EnginePreview
    {
        public EnginePreview(string toolId, bool success, double value, string message)
        { ToolId = toolId; Success = success; Value = value; Message = message; }
        public string ToolId { get; private set; }
        public bool Success { get; private set; }
        public double Value { get; private set; }
        public string Message { get; private set; }
    }

    public sealed class EngineInspection
    {
        public EngineInspection(string bin, bool locateSucceeded, string message)
        { Bin = bin; LocateSucceeded = locateSucceeded; Message = message; }
        public string Bin { get; private set; }
        public bool LocateSucceeded { get; private set; }
        public string Message { get; private set; }
    }
}
