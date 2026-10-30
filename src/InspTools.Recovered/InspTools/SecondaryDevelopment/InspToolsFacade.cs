using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using InspTools.AOI;
using InspTools.Station;
using OpenCvSharp;

namespace InspTools.SecondaryDevelopment;

/// <summary>
/// Stable, additive API surface for custom UI / automation.
/// Vendor/decompiled objects are intentionally kept private to this implementation.
/// </summary>
public sealed class InspToolsFacade
{
	public const int StationCount = 8;

	private readonly object syncRoot = new object();
	private readonly CVInspection[] stations = new CVInspection[StationCount];
	private readonly SecondaryDevelopmentOptions options;
	private readonly Dictionary<string, RecipeEditTransactionState> recipeEditTransactions = new Dictionary<string, RecipeEditTransactionState>(StringComparer.OrdinalIgnoreCase);
	private RecipeSnapshot currentRecipe;

	public string RuntimeRoot => AppDomain.CurrentDomain.BaseDirectory;

	public InspToolsFacade()
		: this(new SecondaryDevelopmentOptions())
	{
	}

	public InspToolsFacade(SecondaryDevelopmentOptions options)
	{
		this.options = options ?? throw new ArgumentNullException(nameof(options));
	}

	public bool RecipeWritesEnabled => options.AllowRecipeWrites && HasRecipeWriteGuard();

	public RecipeSnapshot LoadRecipe(string recipeName)
	{
		if (string.IsNullOrWhiteSpace(recipeName))
		{
			throw new ArgumentException("Recipe name is required.", nameof(recipeName));
		}

		string safeRecipeName = Path.GetFileName(recipeName.Trim());
		if (!string.Equals(safeRecipeName, recipeName.Trim(), StringComparison.Ordinal))
		{
			throw new ArgumentException("Recipe name must be a directory name, not a path.", nameof(recipeName));
		}

		string recipeRoot = Path.Combine(RuntimeRoot, "Recipe", safeRecipeName);
		if (!Directory.Exists(recipeRoot))
		{
			throw new DirectoryNotFoundException("Recipe directory was not found: " + recipeRoot);
		}
		for (int i = 0; i < StationCount; i++)
		{
			string stationFile = Path.Combine(recipeRoot, "station" + (i + 1) + ".ini");
			if (!File.Exists(stationFile))
			{
				throw new FileNotFoundException("Recipe station file was not found.", stationFile);
			}
		}

		lock (syncRoot)
		{
			var snapshots = new List<RecipeStationSnapshot>(StationCount);
			var messages = new List<string>();
			for (int i = 0; i < StationCount; i++)
			{
				CVInspection station = GetOrCreateStation(i);
				station.SetStationIndex(i);
				string copyError = string.Empty;
				station.SetRecipe(safeRecipeName, i, ref copyError);
				if (!string.IsNullOrWhiteSpace(copyError))
				{
					messages.Add("Station " + (i + 1) + ": " + copyError.Trim());
				}
				snapshots.Add(CreateStationSnapshot(station, i + 1, safeRecipeName, copyError));
			}

			currentRecipe = new RecipeSnapshot(
				safeRecipeName,
				new ReadOnlyCollection<RecipeStationSnapshot>(snapshots),
				string.Join(Environment.NewLine, messages));
			return currentRecipe;
		}
	}

	public RecipeSnapshot GetCurrentRecipe()
	{
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			return currentRecipe;
		}
	}

	public RecipeStationSnapshot GetStationSnapshot(int stationNumber)
	{
		int index = ToStationIndex(stationNumber);
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			return currentRecipe.Stations[index];
		}
	}

	public ModelLoadResult LoadConfiguredModel(int stationNumber, ModelFamily family)
	{
		int index = ToStationIndex(stationNumber);
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			CVInspection station = GetOrCreateStation(index);
			RecipeStationSnapshot snapshot = currentRecipe.Stations[index];
			if (family == ModelFamily.Enhance)
			{
				if (!snapshot.EnhanceEnabled)
				{
					return new ModelLoadResult(stationNumber, family, snapshot.EnhanceModelName, false, false,
						snapshot.EnhanceUseTensorRt, "AI Enhance is disabled for this station.");
				}
				if (station.aiEnhanceTools == null)
				{
					return new ModelLoadResult(stationNumber, family, snapshot.EnhanceModelName, false, false,
						snapshot.EnhanceUseTensorRt, "AI Enhance runtime is unavailable.");
				}
				bool success = station.aiEnhanceTools.ReadModel(false, 0, index);
				return new ModelLoadResult(stationNumber, family, snapshot.EnhanceModelName, success,
					station.aiEnhanceTools.ModelLoaded_A, snapshot.EnhanceUseTensorRt, station.aiEnhanceTools.ResultText);
			}

			if (!snapshot.PredictEnabled)
			{
				return new ModelLoadResult(stationNumber, family, snapshot.PredictModelName, false, false,
					snapshot.PredictUseTensorRt, "AI Predict is disabled for this station.");
			}
			if (station.aiPredictTools == null)
			{
				return new ModelLoadResult(stationNumber, family, snapshot.PredictModelName, false, false,
					snapshot.PredictUseTensorRt, "AI Predict runtime is unavailable.");
			}
			bool predictSuccess = station.aiPredictTools.ReadModel(false, 0, index);
			return new ModelLoadResult(stationNumber, family, snapshot.PredictModelName, predictSuccess,
				station.aiPredictTools.ModelLoaded_A, snapshot.PredictUseTensorRt, station.aiPredictTools.ResultText);
		}
	}

	public ModelLoadResult ReloadEnhanceModel(int stationNumber, string modelName)
	{
		int index = ToStationIndex(stationNumber);
		if (string.IsNullOrWhiteSpace(modelName))
		{
			throw new ArgumentException("Model name is required.", nameof(modelName));
		}

		string fileName = Path.GetFileName(modelName.Trim());
		if (!string.Equals(fileName, modelName.Trim(), StringComparison.Ordinal))
		{
			throw new ArgumentException("Model name must be a file name or stem, not a path.", nameof(modelName));
		}
		string stem = Path.GetFileNameWithoutExtension(fileName);

		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			CVInspection station = GetOrCreateStation(index);
			if (station.aiEnhanceTools == null)
			{
				return new ModelLoadResult(stationNumber, ModelFamily.Enhance, fileName, false, false, false,
					"AI Enhance runtime is unavailable.");
			}
			bool success = station.aiEnhanceTools.ReadModel(stem, true, 0, false);
			return new ModelLoadResult(stationNumber, ModelFamily.Enhance, fileName, success,
				station.aiEnhanceTools.ModelLoaded_A, currentRecipe.Stations[index].EnhanceUseTensorRt,
				station.aiEnhanceTools.ResultText);
		}
	}

	public InspectionResult InspectFile(int stationNumber, string imagePath, InspectionOptions options = null)
	{
		int index = ToStationIndex(stationNumber);
		if (string.IsNullOrWhiteSpace(imagePath))
		{
			throw new ArgumentException("Image path is required.", nameof(imagePath));
		}
		string fullPath = Path.GetFullPath(imagePath);
		if (!File.Exists(fullPath))
		{
			throw new FileNotFoundException("Inspection image was not found.", fullPath);
		}
		options = options ?? new InspectionOptions();

		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			CVInspection station = GetOrCreateStation(index);
			Mat mat = CVBuffer.ImRead(fullPath, ImreadModes.Color);
			if (mat == null || mat.Empty())
			{
				mat?.Dispose();
				throw new InvalidDataException("Inspection image could not be decoded: " + fullPath);
			}
			try
			{
				station.SetBuffers(mat);
				INSPRESULT vendorResult = station.Inspect(ref mat, options.Offline, options.UseAi,
					options.ImageId, (int)options.ImageSource, options.ImageListIndex);
				IReadOnlyList<InspectionToolResult> toolResults = BuildInspectionToolResults(station, vendorResult.ParetoIndex);
				return new InspectionResult(
					stationNumber,
					fullPath,
					MapInspectionBin(vendorResult.bin),
					vendorResult.ParetoIndex,
					station.ErrorMsg,
					station.locateTool != null && station.locateTool.LocateSuccess,
					station.InlineResult == null ? 0 : station.InlineResult.Count,
					station.OfflineResult == null ? 0 : station.OfflineResult.Count,
					options.ImageId,
					toolResults);
			}
			finally
			{
				mat.Dispose();
			}
		}
	}

	/// <summary>
	/// Executes one Advanced-Locate-family tool using the same recovered AOI implementation as
	/// the vendor settings window. Only Locate/Enhance and the dependencies required by the
	/// requested section are prepared; downstream inspection families are not executed.
	/// </summary>
	public ToolPreviewResult InspectToolPreview(int stationNumber, string imagePath, string section, int faceIndex = 0)
	{
		int stationIndex = ToStationIndex(stationNumber);
		if (string.IsNullOrWhiteSpace(imagePath)) throw new ArgumentException("Image path is required.", nameof(imagePath));
		if (string.IsNullOrWhiteSpace(section)) throw new ArgumentException("Tool section is required.", nameof(section));
		string fullPath = Path.GetFullPath(imagePath);
		if (!File.Exists(fullPath)) throw new FileNotFoundException("Inspection image was not found.", fullPath);
		ToolPreviewTarget target = ParseToolPreviewTarget(section.Trim());

		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			CVInspection station = GetOrCreateStation(stationIndex);
			Mat mat = CVBuffer.ImRead(fullPath, ImreadModes.Color);
			if (mat == null || mat.Empty())
			{
				mat?.Dispose();
				throw new InvalidDataException("Inspection image could not be decoded: " + fullPath);
			}

			try
			{
				station.SetBuffers(mat);
				bool locateSucceeded = PrepareToolPreview(station);
				TOOL_RESULT state = TOOL_RESULT.EM_NG;
				if (locateSucceeded)
				{
					var completed = new Dictionary<string, TOOL_RESULT>(StringComparer.OrdinalIgnoreCase);
					var active = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					state = ExecuteToolPreview(station, target, faceIndex, completed, active);
				}

				return CreateToolPreviewResult(stationNumber, fullPath, mat.Width, mat.Height,
					station, target, state, locateSucceeded);
			}
			finally
			{
				mat.Dispose();
			}
		}
	}

	public TrainingPlan GetTrainingPlan(int stationNumber, ModelFamily family)
	{
		int index = ToStationIndex(stationNumber);
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			RecipeStationSnapshot selected = currentRecipe.Stations[index];
			string modelName = family == ModelFamily.Enhance ? selected.EnhanceModelName : selected.PredictModelName;
			bool enabled = family == ModelFamily.Enhance ? selected.EnhanceEnabled : selected.PredictEnabled;
			bool useTensorRt = family == ModelFamily.Enhance ? selected.EnhanceUseTensorRt : selected.PredictUseTensorRt;
			if (!enabled)
			{
				throw new InvalidOperationException(family + " is disabled for station " + stationNumber + ".");
			}
			if (string.IsNullOrWhiteSpace(modelName))
			{
				throw new InvalidOperationException(family + " model is not configured for station " + stationNumber + ".");
			}

			var sharedStations = currentRecipe.Stations
				.Where(s => family == ModelFamily.Enhance
					? s.EnhanceEnabled && string.Equals(s.EnhanceModelName, modelName, StringComparison.Ordinal)
					: s.PredictEnabled && string.Equals(s.PredictModelName, modelName, StringComparison.Ordinal))
				.Select(s => s.StationNumber)
				.ToList();

			string modelRoot = Path.Combine(RuntimeRoot, "Model");
			string activeSource = Path.Combine(modelRoot, modelName);
			string extension = Path.GetExtension(modelName);
			string stem = Path.GetFileNameWithoutExtension(modelName);
			string candidateSource = Path.Combine(modelRoot, stem + "-TempFile" + extension);
			string derivativeExtension = useTensorRt ? ".Trt" : family == ModelFamily.Enhance ? ".pbd" : ".pb";
			string activeDerivative = Path.Combine(modelRoot, stem + derivativeExtension);
			string candidateDerivative = Path.Combine(modelRoot, stem + "-TempFile" + derivativeExtension);

			return new TrainingPlan(
				stationNumber,
				family,
				modelName,
				new ReadOnlyCollection<int>(sharedStations),
				activeSource,
				candidateSource,
				activeDerivative,
				candidateDerivative,
				Path.Combine(RuntimeRoot, "Training", "Model", "navs.h5"),
				Path.Combine(RuntimeRoot, "Training", "Model", "navs.ini"),
				useTensorRt ? RuntimeBackend.TensorRt : RuntimeBackend.NativeNoTensorRt);
		}
	}

	public RecipeParameterSnapshot GetRecipeParameters(int stationNumber)
	{
		int index = ToStationIndex(stationNumber);
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			string stationPath = GetCurrentStationRecipePath(index);
			RecipeIniDocument document = RecipeIniDocument.Load(stationPath);
			return new RecipeParameterSnapshot(
				stationNumber,
				currentRecipe.RecipeName,
				stationPath,
				document.EncodingName,
				SecondaryDevelopmentHash.Sha256(document.SourceBytes),
				document.GetEntries());
		}
	}

	public RecipePatchPreview PreviewRecipeParameterPatch(int stationNumber, IEnumerable<RecipeParameterPatch> patches)
	{
		int index = ToStationIndex(stationNumber);
		IReadOnlyList<RecipeParameterPatch> patchList = NormalizePatches(patches);
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			string stationPath = GetCurrentStationRecipePath(index);
			RecipeIniDocument document = RecipeIniDocument.Load(stationPath);
			return new RecipePatchPreview(
				stationNumber,
				currentRecipe.RecipeName,
				stationPath,
				document.EncodingName,
				SecondaryDevelopmentHash.Sha256(document.SourceBytes),
				document.Preview(patchList));
		}
	}

	public RecipePatchResult ApplyRecipeParameterPatch(int stationNumber, IEnumerable<RecipeParameterPatch> patches, bool reloadRecipe = true)
	{
		int index = ToStationIndex(stationNumber);
		IReadOnlyList<RecipeParameterPatch> patchList = NormalizePatches(patches);
		lock (syncRoot)
		{
			EnsureRecipeLoaded();
			EnsureRecipeWriteAllowed();

			string recipeName = currentRecipe.RecipeName;
			string stationPath = GetCurrentStationRecipePath(index);
			RecipeIniDocument document = RecipeIniDocument.Load(stationPath);
			IReadOnlyList<RecipeParameterDiff> differences = document.Preview(patchList);
			string preHash = SecondaryDevelopmentHash.Sha256(document.SourceBytes);
			if (!differences.Any(d => d.Changed))
			{
				return new RecipePatchResult(string.Empty, stationNumber, recipeName, stationPath,
					preHash, preHash, string.Empty, false, differences);
			}

			byte[] postBytes = document.Apply(patchList);
			string postHash = SecondaryDevelopmentHash.Sha256(postBytes);
			string transactionId = Guid.NewGuid().ToString("N");
			string backupRoot = Path.Combine(RuntimeRoot, "RecipeEditBackups");
			Directory.CreateDirectory(backupRoot);
			string backupPath = Path.Combine(backupRoot, transactionId + "_station" + stationNumber + ".ini");
			string tempPath = stationPath + ".secondarydev." + transactionId + ".tmp";

			File.WriteAllBytes(tempPath, postBytes);
			try
			{
				File.Replace(tempPath, stationPath, backupPath, true);
				if (!string.Equals(SecondaryDevelopmentHash.Sha256(stationPath), postHash, StringComparison.OrdinalIgnoreCase))
				{
					throw new IOException("Recipe parameter write hash verification failed.");
				}

				bool reloaded = false;
				try
				{
					if (reloadRecipe)
					{
						LoadRecipe(recipeName);
						reloaded = true;
					}
				}
				catch
				{
					RestoreBackupAtomically(stationPath, backupPath, transactionId + "_applyfail");
					LoadRecipe(recipeName);
					throw;
				}

				var state = new RecipeEditTransactionState
				{
					TransactionId = transactionId,
					StationNumber = stationNumber,
					RecipeName = recipeName,
					StationPath = stationPath,
					BackupPath = backupPath,
					PreSha256 = preHash,
					PostSha256 = postHash,
					RolledBack = false
				};
				recipeEditTransactions.Add(transactionId, state);
				return new RecipePatchResult(transactionId, stationNumber, recipeName, stationPath,
					preHash, postHash, backupPath, reloaded, differences);
			}
			finally
			{
				if (File.Exists(tempPath)) File.Delete(tempPath);
			}
		}
	}

	public RecipeRollbackResult RollbackRecipeParameterPatch(string transactionId, bool reloadRecipe = true)
	{
		if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("Transaction id is required.", nameof(transactionId));
		lock (syncRoot)
		{
			EnsureRecipeWriteAllowed();
			RecipeEditTransactionState state;
			if (!recipeEditTransactions.TryGetValue(transactionId.Trim(), out state))
			{
				throw new KeyNotFoundException("Recipe edit transaction was not found: " + transactionId + ".");
			}
			if (state.RolledBack)
			{
				throw new InvalidOperationException("Recipe edit transaction was already rolled back: " + transactionId + ".");
			}
			if (!File.Exists(state.BackupPath)) throw new FileNotFoundException("Recipe edit backup was not found.", state.BackupPath);
			string currentHash = SecondaryDevelopmentHash.Sha256(state.StationPath);
			if (!string.Equals(currentHash, state.PostSha256, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Recipe edit rollback was refused because the station file changed after the transaction.");
			}

			RestoreBackupAtomically(state.StationPath, state.BackupPath, state.TransactionId + "_rollback");
			string restoredHash = SecondaryDevelopmentHash.Sha256(state.StationPath);
			if (!string.Equals(restoredHash, state.PreSha256, StringComparison.OrdinalIgnoreCase))
			{
				throw new IOException("Recipe edit rollback hash verification failed.");
			}

			bool reloaded = false;
			if (reloadRecipe)
			{
				LoadRecipe(state.RecipeName);
				reloaded = true;
			}
			state.RolledBack = true;
			return new RecipeRollbackResult(state.TransactionId, state.StationNumber, state.RecipeName, restoredHash, reloaded);
		}
	}

	private CVInspection GetOrCreateStation(int index)
	{
		CVInspection station = stations[index];
		if (station == null)
		{
			station = new CVInspection();
			station.SetStationIndex(index);
			stations[index] = station;
		}
		return station;
	}

	private static IReadOnlyList<RecipeParameterPatch> NormalizePatches(IEnumerable<RecipeParameterPatch> patches)
	{
		if (patches == null) throw new ArgumentNullException(nameof(patches));
		List<RecipeParameterPatch> list = patches.ToList();
		if (list.Count == 0) throw new ArgumentException("At least one Recipe parameter patch is required.", nameof(patches));
		if (list.Any(p => p == null)) throw new ArgumentException("Recipe parameter patch collection contains null.", nameof(patches));
		return new ReadOnlyCollection<RecipeParameterPatch>(list);
	}

	private string GetCurrentStationRecipePath(int index)
	{
		string path = Path.Combine(RuntimeRoot, "Recipe", currentRecipe.RecipeName, "station" + (index + 1) + ".ini");
		if (!File.Exists(path)) throw new FileNotFoundException("Recipe station file was not found.", path);
		return path;
	}

	private bool HasRecipeWriteGuard()
	{
		string guardPath = Path.Combine(RuntimeRoot, SecondaryDevelopmentOptions.RecipeWriteGuardFileName);
		if (!File.Exists(guardPath)) return false;
		try
		{
			string content = File.ReadAllText(guardPath, Encoding.UTF8).Trim();
			return string.Equals(content, SecondaryDevelopmentOptions.RecipeWriteGuardContent, StringComparison.Ordinal);
		}
		catch
		{
			return false;
		}
	}

	private void EnsureRecipeWriteAllowed()
	{
		if (!options.AllowRecipeWrites)
		{
			throw new InvalidOperationException("Recipe writes are disabled. Construct InspToolsFacade with SecondaryDevelopmentOptions.AllowRecipeWrites=true only for an isolated writable AppBase.");
		}
		if (!HasRecipeWriteGuard())
		{
			throw new InvalidOperationException("Recipe write guard is missing or invalid: " + Path.Combine(RuntimeRoot, SecondaryDevelopmentOptions.RecipeWriteGuardFileName));
		}
	}

	private static void RestoreBackupAtomically(string stationPath, string backupPath, string suffix)
	{
		byte[] backupBytes = File.ReadAllBytes(backupPath);
		string tempPath = stationPath + ".secondarydev." + suffix + ".tmp";
		File.WriteAllBytes(tempPath, backupBytes);
		try
		{
			File.Replace(tempPath, stationPath, null, true);
		}
		finally
		{
			if (File.Exists(tempPath)) File.Delete(tempPath);
		}
	}

	private sealed class ToolPreviewTarget
	{
		public ToolPreviewTarget(ToolPreviewKind kind, string section, string family, string prefix, int index)
		{
			Kind = kind;
			Section = section;
			Family = family;
			Prefix = prefix;
			Index = index;
		}

		public ToolPreviewKind Kind { get; }
		public string Section { get; }
		public string Family { get; }
		public string Prefix { get; }
		public int Index { get; }
	}

	private static ToolPreviewTarget ParseToolPreviewTarget(string section)
	{
		ToolPreviewTarget target;
		if (TryParseToolPreviewTarget(section, "VLocateTools", ToolPreviewKind.VerticalLocate, "Locate", 2, 19, out target)) return target;
		if (TryParseToolPreviewTarget(section, "HLocateTools", ToolPreviewKind.HorizontalLocate, "Locate", 2, 19, out target)) return target;
		if (TryParseToolPreviewTarget(section, "AdvLocateTools", ToolPreviewKind.AdvancedLocate, "AdvLocate", 1, 19, out target)) return target;
		if (TryParseToolPreviewTarget(section, "AdvMeasure", ToolPreviewKind.AdvancedMeasure, "Measure", 0, 39, out target)) return target;
		throw new NotSupportedException("Tool preview supports VLocateTools2-19, HLocateTools2-19, AdvLocateTools1-19 and AdvMeasure0-39. Section: " + section);
	}

	private static bool TryParseToolPreviewTarget(string section, string prefix, ToolPreviewKind kind,
		string family, int minIndex, int maxIndex, out ToolPreviewTarget target)
	{
		target = null;
		if (!section.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
		int index;
		if (!int.TryParse(section.Substring(prefix.Length), out index) || index < minIndex || index > maxIndex) return false;
		target = new ToolPreviewTarget(kind, prefix + index, family, prefix, index);
		return true;
	}

	private static bool PrepareToolPreview(CVInspection station)
	{
		ParamFile p = station.paramFile;
		bool locateSucceeded;
		if (p.LocateToolON)
		{
			station.locateTool.DoLocate(true);
			locateSucceeded = station.locateTool.LocateSuccess;
			station.LocateRect = station.locateTool.LocateRect;
			station.LocateRectTmp = station.locateTool.LocateRect;
			station.buffers.LocateImageRect = station.locateTool.LocateImageRect;
		}
		else
		{
			locateSucceeded = true;
			station.locateTool.LocateRect = station.buffers.SrcRect;
			station.locateTool.LocateImageRect = station.buffers.SrcRect;
			station.LocateRect = station.buffers.SrcRect;
			station.LocateRectTmp = station.buffers.SrcRect;
			station.buffers.LocateImageRect = station.buffers.SrcRect;
			station.buffers.LocatedRect = station.buffers.SrcRect;
			int[] enhanceFunctions = new int[6];
			for (int i = 0; i < enhanceFunctions.Length; i++) enhanceFunctions[i] = p.EnhanceParam[i].FunctionIndex;
			station.buffers.SetLocatedBuffer(station.buffers.Src, enhanceFunctions, true, false);
		}

		Rect locateRect = station.LocateRect;
		if (!locateSucceeded || locateRect.Width <= 0 || locateRect.Height <= 0) return false;

		if (p.EnahnceToolON)
		{
			for (int i = 0; i < 6; i++) station.enhanceTool.Inspect(i, true);
		}

		p.VSLocateParam[0].TrackerON = true;
		p.VSLocateParam[0].Value = locateRect.Left;
		p.VSLocateParam[1].TrackerON = true;
		p.VSLocateParam[1].Value = locateRect.Right;
		p.HSLocateParam[0].TrackerON = true;
		p.HSLocateParam[0].Value = locateRect.Top;
		p.HSLocateParam[1].TrackerON = true;
		p.HSLocateParam[1].Value = locateRect.Bottom;
		p.AdvLocateParam[0].TrackerON = true;
		p.AdvLocateParam[0].L = locateRect.Left;
		p.AdvLocateParam[0].T = locateRect.Top;
		p.AdvLocateParam[0].R = locateRect.Right;
		p.AdvLocateParam[0].B = locateRect.Bottom;
		p.AdvLocateParam[0].Width = locateRect.Width;
		p.AdvLocateParam[0].Height = locateRect.Height;
		p.AdvLocateParam[0].InsideRect = locateRect;
		return true;
	}

	private static TOOL_RESULT ExecuteToolPreview(CVInspection station, ToolPreviewTarget target, int faceIndex,
		Dictionary<string, TOOL_RESULT> completed, HashSet<string> active)
	{
		switch (target.Kind)
		{
			case ToolPreviewKind.VerticalLocate:
				return EnsureVerticalPreview(station, target.Index, faceIndex, completed, active);
			case ToolPreviewKind.HorizontalLocate:
				return EnsureHorizontalPreview(station, target.Index, faceIndex, completed, active);
			case ToolPreviewKind.AdvancedLocate:
				return EnsureAdvancedLocatePreview(station, target.Index, faceIndex, completed, active);
			case ToolPreviewKind.AdvancedMeasure:
				return EnsureAdvancedMeasurePreview(station, target.Index, faceIndex, completed, active);
			default:
				throw new ArgumentOutOfRangeException(nameof(target.Kind));
		}
	}

	private static TOOL_RESULT EnsureVerticalPreview(CVInspection station, int index, int faceIndex,
		Dictionary<string, TOOL_RESULT> completed, HashSet<string> active)
	{
		if (index < 2) return TOOL_RESULT.EM_OK;
		string key = "VLocateTools" + index;
		TOOL_RESULT cached;
		if (completed.TryGetValue(key, out cached)) return cached;
		EnterPreviewDependency(key, active);
		try
		{
			TOOL_RESULT result = TOOL_RESULT.EM_OK;
			station.advLocateTool.doVScan(index, ref result, faceIndex, true);
			completed[key] = result;
			return result;
		}
		finally { active.Remove(key); }
	}

	private static TOOL_RESULT EnsureHorizontalPreview(CVInspection station, int index, int faceIndex,
		Dictionary<string, TOOL_RESULT> completed, HashSet<string> active)
	{
		if (index < 2) return TOOL_RESULT.EM_OK;
		string key = "HLocateTools" + index;
		TOOL_RESULT cached;
		if (completed.TryGetValue(key, out cached)) return cached;
		EnterPreviewDependency(key, active);
		try
		{
			TOOL_RESULT result = TOOL_RESULT.EM_OK;
			station.advLocateTool.doHScan(index, ref result, faceIndex, true);
			completed[key] = result;
			return result;
		}
		finally { active.Remove(key); }
	}

	private static TOOL_RESULT EnsureAdvancedLocatePreview(CVInspection station, int index, int faceIndex,
		Dictionary<string, TOOL_RESULT> completed, HashSet<string> active)
	{
		if (index <= 0) return TOOL_RESULT.EM_OK;
		string key = "AdvLocateTools" + index;
		TOOL_RESULT cached;
		if (completed.TryGetValue(key, out cached)) return cached;
		EnterPreviewDependency(key, active);
		try
		{
			INSP_ADVLOCATETOOL_PARAM param = station.paramFile.AdvLocateParam[index];
			if (!param.UseBinary)
			{
				EnsureVerticalPreview(station, param.RectIndexL, faceIndex, completed, active);
				EnsureVerticalPreview(station, param.RectIndexR, faceIndex, completed, active);
				EnsureHorizontalPreview(station, param.RectIndexT, faceIndex, completed, active);
				EnsureHorizontalPreview(station, param.RectIndexB, faceIndex, completed, active);
			}
			else if (param.UseAutoSize && param.AutoSizeIndex > 0)
			{
				EnsureAdvancedLocatePreview(station, param.AutoSizeIndex, faceIndex, completed, active);
			}

			TOOL_RESULT result = TOOL_RESULT.EM_OK;
			station.advLocateTool.Inspect(index, ref result, faceIndex, true);
			completed[key] = result;
			return result;
		}
		finally { active.Remove(key); }
	}

	private static TOOL_RESULT EnsureAdvancedMeasurePreview(CVInspection station, int index, int faceIndex,
		Dictionary<string, TOOL_RESULT> completed, HashSet<string> active)
	{
		string key = "AdvMeasure" + index;
		TOOL_RESULT cached;
		if (completed.TryGetValue(key, out cached)) return cached;
		EnterPreviewDependency(key, active);
		try
		{
			INSP_ADVMEASURETOOL_PARAM param = station.paramFile.AdvLocateMParam[index];
			switch (param.MeasureFunction)
			{
				case 0:
					if (param.MStartSelect == 0) EnsureVerticalPreview(station, param.MStartWHIndex, faceIndex, completed, active);
					else EnsureAdvancedLocatePreview(station, param.MStartAIndex, faceIndex, completed, active);
					if (param.MEndSelect == 0) EnsureVerticalPreview(station, param.MEndWHIndex, faceIndex, completed, active);
					else EnsureAdvancedLocatePreview(station, param.MEndAIndex, faceIndex, completed, active);
					break;
				case 1:
					if (param.MStartSelect == 0) EnsureHorizontalPreview(station, param.MStartWHIndex, faceIndex, completed, active);
					else EnsureAdvancedLocatePreview(station, param.MStartAIndex, faceIndex, completed, active);
					if (param.MEndSelect == 0) EnsureHorizontalPreview(station, param.MEndWHIndex, faceIndex, completed, active);
					else EnsureAdvancedLocatePreview(station, param.MEndAIndex, faceIndex, completed, active);
					break;
				case 2:
					EnsureAdvancedLocatePreview(station, param.MStartAIndex, faceIndex, completed, active);
					EnsureAdvancedLocatePreview(station, param.MEndAIndex, faceIndex, completed, active);
					break;
				case 3:
					EnsureAdvancedLocatePreview(station, param.MStartWHIndex, faceIndex, completed, active);
					EnsureAdvancedLocatePreview(station, param.MEndWHIndex, faceIndex, completed, active);
					break;
				case 4:
					EnsureAdvancedMeasurePreview(station, param.MStartWHIndex, faceIndex, completed, active);
					EnsureAdvancedMeasurePreview(station, param.MEndWHIndex, faceIndex, completed, active);
					break;
			}

			TOOL_RESULT result = TOOL_RESULT.EM_OK;
			station.advLocateTool.doAdvMeasure(index, ref result, faceIndex, true);
			completed[key] = result;
			return result;
		}
		finally { active.Remove(key); }
	}

	private static void EnterPreviewDependency(string key, HashSet<string> active)
	{
		if (!active.Add(key)) throw new InvalidOperationException("Advanced Locate preview dependency cycle: " + key);
	}

	private static ToolPreviewResult CreateToolPreviewResult(int stationNumber, string imagePath, int imageWidth,
		int imageHeight, CVInspection station, ToolPreviewTarget target, TOOL_RESULT state, bool locateSucceeded)
	{
		ParamFile p = station.paramFile;
		string trackerName;
		FLOW_BIN vendorErrorBin;
		int paretoIndex;
		Rect inspectionRect = new Rect(0, 0, 0, 0);
		Rect resultRect = new Rect(0, 0, 0, 0);
		Rect referenceRect1 = new Rect(0, 0, 0, 0);
		Rect referenceRect2 = new Rect(0, 0, 0, 0);
		int? scanValue = null;
		double? widthValue = null;
		double? heightValue = null;
		double? measureValue = null;

		switch (target.Kind)
		{
			case ToolPreviewKind.VerticalLocate:
			{
				INSP_SCANLINETOOL_PARAM param = p.VSLocateParam[target.Index];
				trackerName = param.TrackerName;
				vendorErrorBin = param.ErrorBin;
				paretoIndex = param.ParetoIndex;
				inspectionRect = station.advLocateTool.InspeRect;
				scanValue = param.Value;
				if (param.Value >= 0) resultRect = new Rect(param.Value, inspectionRect.Y, 1, inspectionRect.Height);
				break;
			}
			case ToolPreviewKind.HorizontalLocate:
			{
				INSP_SCANLINETOOL_PARAM param = p.HSLocateParam[target.Index];
				trackerName = param.TrackerName;
				vendorErrorBin = param.ErrorBin;
				paretoIndex = param.ParetoIndex;
				inspectionRect = station.advLocateTool.InspeRect;
				scanValue = param.Value;
				if (param.Value >= 0) resultRect = new Rect(inspectionRect.X, param.Value, inspectionRect.Width, 1);
				break;
			}
			case ToolPreviewKind.AdvancedLocate:
			{
				INSP_ADVLOCATETOOL_PARAM param = p.AdvLocateParam[target.Index];
				trackerName = param.TrackerName;
				vendorErrorBin = param.ErrorBin;
				paretoIndex = param.ParetoIndex;
				resultRect = param.InsideRect;
				inspectionRect = param.UseBinary ? station.advLocateTool.InspeRect : resultRect;
				widthValue = station.advLocateTool.AdvAWidth;
				heightValue = station.advLocateTool.AdvAHeight;
				break;
			}
			case ToolPreviewKind.AdvancedMeasure:
			{
				INSP_ADVMEASURETOOL_PARAM param = p.AdvLocateMParam[target.Index];
				trackerName = param.TrackerName;
				vendorErrorBin = param.ErrorBin;
				paretoIndex = param.ParetoIndex;
				referenceRect1 = station.advLocateTool.NGRect;
				referenceRect2 = station.advLocateTool.NGRect2;
				measureValue = station.advLocateTool.MeasureDistance;
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(target.Kind));
		}

		InspectionToolState stableState = locateSucceeded ? MapInspectionToolState(state) : InspectionToolState.Ng;
		InspectionBin configuredErrorBin = MapInspectionBin(vendorErrorBin);
		InspectionBin resultBin = stableState == InspectionToolState.Ng || stableState == InspectionToolState.Retest
			? configuredErrorBin : InspectionBin.Ok;
		var toolResult = new InspectionToolResult(target.Family, target.Section, trackerName, target.Index,
			stableState, resultBin, paretoIndex, stableState == InspectionToolState.Ng ? 1 : 0);
		string errorMessage = locateSucceeded ? station.advLocateTool.ErrorMsg : station.locateTool.ErrorMsg;

		return new ToolPreviewResult(stationNumber, imagePath, target.Section, target.Kind, toolResult,
			configuredErrorBin, errorMessage, locateSucceeded, imageWidth, imageHeight,
			RectSnapshot(station.LocateRect), RectSnapshot(inspectionRect), RectSnapshot(resultRect),
			RectSnapshot(referenceRect1), RectSnapshot(referenceRect2),
			PointSnapshot(station.advLocateTool.NGPt1), PointSnapshot(station.advLocateTool.NGPt2),
			scanValue, widthValue, heightValue, measureValue);
	}

	private static ToolRectSnapshot RectSnapshot(Rect rect)
	{
		return new ToolRectSnapshot(rect.X, rect.Y, rect.Width, rect.Height);
	}

	private static ToolPointSnapshot PointSnapshot(Point point)
	{
		return new ToolPointSnapshot(point.X, point.Y);
	}

	private static RecipeStationSnapshot CreateStationSnapshot(CVInspection station, int stationNumber,
		string recipeName, string message)
	{
		ParamFile param = station.paramFile;
		INSP_AITOOL_PARAM enhance = param.AIEnhanceParam;
		INSP_AITOOL_PARAM predict = param.AIPredictParam;
		return new RecipeStationSnapshot(
			stationNumber,
			recipeName,
			param.StationON,
			param.AIEnhanceToolON,
			param.AIPredictToolON,
			param.AIProphetToolON,
			enhance.ModelName_A,
			enhance.Width_A,
			enhance.Height_A,
			enhance.EnableAsignGPU,
			enhance.GPUID,
			enhance.UseTensorRT,
			predict.ModelName_A,
			predict.Width_A,
			predict.Height_A,
			predict.GPUID,
			predict.UseTensorRT,
			message);
	}

	private static int ToStationIndex(int stationNumber)
	{
		if (stationNumber < 1 || stationNumber > StationCount)
		{
			throw new ArgumentOutOfRangeException(nameof(stationNumber), stationNumber, "Station number must be 1 through 8.");
		}
		return stationNumber - 1;
	}

	private void EnsureRecipeLoaded()
	{
		if (currentRecipe == null)
		{
			throw new InvalidOperationException("LoadRecipe must be called before this operation.");
		}
	}

	private static InspectionBin MapInspectionBin(FLOW_BIN bin)
	{
		switch (bin)
		{
			case FLOW_BIN.EM_FLOW_RETESET:
				return InspectionBin.Retest;
			case FLOW_BIN.EM_FLOW_OK:
				return InspectionBin.Ok;
			case FLOW_BIN.EM_FLOW_NG1:
				return InspectionBin.Ng1;
			case FLOW_BIN.EM_FLOW_NG2:
				return InspectionBin.Ng2;
			case FLOW_BIN.EM_FLOW_NG3:
				return InspectionBin.Ng3;
			case FLOW_BIN.EM_FLOW_NG4:
				return InspectionBin.Ng4;
			default:
				throw new ArgumentOutOfRangeException(nameof(bin), bin, "Unknown vendor inspection bin.");
		}
	}

	private static InspectionToolState MapInspectionToolState(TOOL_RESULT result)
	{
		switch (result)
		{
			case TOOL_RESULT.EM_NOT_USE:
				return InspectionToolState.NotUsed;
			case TOOL_RESULT.EM_OK:
				return InspectionToolState.Ok;
			case TOOL_RESULT.EM_NG:
				return InspectionToolState.Ng;
			case TOOL_RESULT.EM_Retest:
				return InspectionToolState.Retest;
			default:
				throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown vendor tool result.");
		}
	}

	private static IReadOnlyList<InspectionToolResult> BuildInspectionToolResults(CVInspection station, int finalParetoIndex)
	{
		var list = new List<InspectionToolResult>();
		if (station == null || station.paramFile == null || station.iResult == null)
		{
			return list.AsReadOnly();
		}

		ParamFile p = station.paramFile;
		CVInspResult r = station.iResult;

		if (p.LocateToolON)
		{
			AddInspectionToolResult(list, "Locate", "Locate", p.LocateParam.TrackerName, 0,
				r.LocaetResult, r.LocateBIN, p.LocateParam.ParetoIndex, r.LocaetErrorCount);
		}

		if (p.AdvLocateToolON)
		{
			for (int i = 0; i < p.VSLocateParam.Length && i < r.VScanResult.Length; i++)
				if (p.VSLocateParam[i].TrackerON)
					AddInspectionToolResult(list, "Locate", "VLocateTools" + i, p.VSLocateParam[i].TrackerName, i,
						r.VScanResult[i], r.VScanBIN[i], p.VSLocateParam[i].ParetoIndex, r.VScanErrorCount[i]);
			for (int i = 0; i < p.HSLocateParam.Length && i < r.HScanResult.Length; i++)
				if (p.HSLocateParam[i].TrackerON)
					AddInspectionToolResult(list, "Locate", "HLocateTools" + i, p.HSLocateParam[i].TrackerName, i,
						r.HScanResult[i], r.HScanBIN[i], p.HSLocateParam[i].ParetoIndex, r.HScanErrorCount[i]);
			for (int i = 0; i < p.AdvLocateParam.Length && i < r.AdvLocateResult.Length; i++)
				if (p.AdvLocateParam[i].TrackerON)
					AddInspectionToolResult(list, "AdvLocate", "AdvLocateTools" + i, p.AdvLocateParam[i].TrackerName, i,
						r.AdvLocateResult[i], r.AdvLocateBIN[i], p.AdvLocateParam[i].ParetoIndex, r.AdvLocateErrorCount[i]);
			for (int i = 0; i < p.AdvLocateMParam.Length && i < r.AdvMeasureResult.Length; i++)
				if (p.AdvLocateMParam[i].TrackerON)
					AddInspectionToolResult(list, "Measure", "AdvMeasure" + i, p.AdvLocateMParam[i].TrackerName, i,
						r.AdvMeasureResult[i], r.AdvMeasureBIN[i], p.AdvLocateMParam[i].ParetoIndex, r.AdvMeasureErrorCount[i]);
		}

		if (p.MeasureToolON)
		{
			for (int i = 0; i < p.MeaWidthParam.Length && i < r.MeaWidthResult.Length; i++)
				if (p.MeaWidthParam[i].TrackerON)
					AddInspectionToolResult(list, "Measure", "MeaWidthTools" + i, p.MeaWidthParam[i].TrackerName, i,
						r.MeaWidthResult[i], r.MeaWidthBIN[i], p.MeaWidthParam[i].ParetoIndex, r.MeaWidthErrorCount[i]);
			for (int i = 0; i < p.MeaHeightParam.Length && i < r.MeaHeightResult.Length; i++)
				if (p.MeaHeightParam[i].TrackerON)
					AddInspectionToolResult(list, "Measure", "MeaHeightTools" + i, p.MeaHeightParam[i].TrackerName, i,
						r.MeaHeightResult[i], r.MeaHeightBIN[i], p.MeaHeightParam[i].ParetoIndex, r.MeaHeightErrorCount[i]);
		}

		if (p.BinaryToolON)
			for (int i = 0; i < p.BinaryParam.Length && i < r.BinaryResult.Length; i++)
				if (p.BinaryParam[i].TrackerON)
					AddInspectionToolResult(list, "Binary", "BinaryTools" + i, p.BinaryParam[i].TrackerName, i,
						r.BinaryResult[i], r.BinaryBIN[i], p.BinaryParam[i].ParetoIndex, r.BinaryErrorCount[i]);

		if (p.SmoothToolON)
			for (int i = 0; i < p.SmoothParam.Length && i < r.SmoothResult.Length; i++)
				if (p.SmoothParam[i].TrackerON)
					AddInspectionToolResult(list, "Smooth", "SmoothTools" + i, p.SmoothParam[i].TrackerName, i,
						r.SmoothResult[i], r.SmoothBIN[i], p.SmoothParam[i].ParetoIndex, r.SmoothErrorCount[i]);

		if (p.BlockAvgToolON)
			for (int i = 0; i < p.BlockAvgParam.Length && i < r.BlockAvgResult.Length; i++)
				if (p.BlockAvgParam[i].TrackerON)
					AddInspectionToolResult(list, "BlockAvg", "BlockAvgTools" + i, p.BlockAvgParam[i].TrackerName, i,
						r.BlockAvgResult[i], r.BlockAvgBIN[i], p.BlockAvgParam[i].ParetoIndex, r.BlockAvgErrorCount[i]);

		if (p.DifferenceToolON)
			for (int i = 0; i < p.DifferenceParam.Length && i < r.DifferenceResult.Length; i++)
				if (p.DifferenceParam[i].TrackerON)
					AddInspectionToolResult(list, "Difference", "DifferenceTools" + i, p.DifferenceParam[i].TrackerName, i,
						r.DifferenceResult[i], r.DifferenceBIN[i], p.DifferenceParam[i].ParetoIndex, r.DifferenceErrorCount[i]);

		if (p.LineAnalysisToolON)
			for (int i = 0; i < p.LineAnalysisParam.Length && i < r.LineAnalysisResult.Length; i++)
				if (p.LineAnalysisParam[i].TrackerON)
					AddInspectionToolResult(list, "LineAnalysis", "LineAnalysisTool" + i, p.LineAnalysisParam[i].TrackerName, i,
						r.LineAnalysisResult[i], r.LineAnalysisBIN[i], p.LineAnalysisParam[i].ParetoIndex, r.LineAnalysisErrorCount[i]);

		if (p.MatchToolON)
			for (int i = 0; i < p.MatchParam.Length && i < r.MatchResult.Length; i++)
				if (p.MatchParam[i].TrackerON)
					AddInspectionToolResult(list, "Match", "MatchTools" + i, p.MatchParam[i].TrackerName, i,
						r.MatchResult[i], r.MatchBIN[i], p.MatchParam[i].ParetoIndex, r.MatchErrorCount[i]);

		if (p.FontToolON)
			AddInspectionToolResult(list, "Font", "FontTool", "Font", 0,
				r.FontToolResult, r.FontToolBIN, p.FontToolParam.ParetoIndex, r.FontToolErrorCount);

		if (p.IRToolON)
			AddInspectionToolResult(list, "IR", "IRTool", "IR", 0,
				r.IRToolResult, r.IRToolBIN, p.IRToolParam.ParetoIndex, r.IRToolErrorCount);

		if (p.AIPredictToolON)
			AddInspectionToolResult(list, "AIPredict", "AIPredictTool", "AI Predict", 0,
				r.AIPredictResult, r.AIPredictBIN, finalParetoIndex, r.AIPredictErrorCount);

		if (p.AIProphetToolON)
			AddInspectionToolResult(list, "AIProphet", "AIProphetTool", "AI Prophet", 0,
				r.AIProphetResult, r.AIProphetBIN, p.AIProphetParam.ParetoIndex, r.AIProphetErrorCount);

		return list.AsReadOnly();
	}

	private static void AddInspectionToolResult(List<InspectionToolResult> list, string family, string section,
		string trackerName, int index, TOOL_RESULT state, FLOW_BIN bin, int paretoIndex, int errorCount)
	{
		list.Add(new InspectionToolResult(family, section, trackerName, index,
			MapInspectionToolState(state), MapInspectionBin(bin), paretoIndex, errorCount));
	}
}
