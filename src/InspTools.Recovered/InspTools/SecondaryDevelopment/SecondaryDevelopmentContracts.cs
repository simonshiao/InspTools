using System;
using System.Collections.Generic;

namespace InspTools.SecondaryDevelopment;

/// <summary>
/// Stable inspection result values for custom UI / automation consumers.
/// Numeric values intentionally match the verified InspTools 1.3.16.28 FLOW_BIN contract,
/// but callers never need to reference FLOW_BIN itself.
/// </summary>
public enum InspectionBin
{
	Retest = 0,
	Ok = 1,
	Ng1 = 2,
	Ng2 = 3,
	Ng3 = 4,
	Ng4 = 5
}

/// <summary>
/// Stable per-tool execution state. Numeric values intentionally match the verified
/// InspTools 1.3.16.28 TOOL_RESULT contract without exposing the vendor enum.
/// </summary>
public enum InspectionToolState
{
	NotUsed = 0,
	Ok = 1,
	Ng = 2,
	Retest = 3
}

/// <summary>
/// Read-only snapshot of one executed inspection tool. This is diagnostic telemetry only;
/// it does not expose mutable vendor AOI objects to secondary-development callers.
/// </summary>
public sealed class InspectionToolResult
{
	internal InspectionToolResult(string family, string section, string trackerName, int index,
		InspectionToolState state, InspectionBin bin, int paretoIndex, int errorCount)
	{
		Family = family ?? string.Empty;
		Section = section ?? string.Empty;
		TrackerName = trackerName ?? string.Empty;
		Index = index;
		State = state;
		Bin = bin;
		ParetoIndex = paretoIndex;
		ErrorCount = errorCount;
	}

	public string Family { get; }
	public string Section { get; }
	public string TrackerName { get; }
	public int Index { get; }
	public InspectionToolState State { get; }
	public InspectionBin Bin { get; }
	public int ParetoIndex { get; }
	public int ErrorCount { get; }
}

public enum ModelFamily
{
	Enhance = 0,
	Predict = 1
}

public enum RuntimeBackend
{
	NativeNoTensorRt = 0,
	TensorRt = 1
}

/// <summary>
/// Stable names for the verified ToolsWindow offline/history image-source mapping.
/// This avoids exposing vendor magic integers to custom UI code.
/// </summary>
public enum InspectionImageSource
{
	None = 0,
	FileList = 1,
	Recent = 2,
	Ok = 3,
	NgPareto = 4,
	Retest = 5
}

public sealed class InspectionOptions
{
	public bool Offline { get; set; } = true;

	public bool UseAi { get; set; }

	public int ImageId { get; set; } = -1;

	public InspectionImageSource ImageSource { get; set; }

	public int ImageListIndex { get; set; }
}

public sealed class InspectionResult
{
	internal InspectionResult(int stationNumber, string imagePath, InspectionBin bin, int paretoIndex,
		string errorMessage, bool locateSucceeded, int inlineHistoryCount, int offlineHistoryCount, int imageId,
		IReadOnlyList<InspectionToolResult> toolResults = null)
	{
		StationNumber = stationNumber;
		ImagePath = imagePath ?? string.Empty;
		Bin = bin;
		ParetoIndex = paretoIndex;
		ErrorMessage = errorMessage ?? string.Empty;
		LocateSucceeded = locateSucceeded;
		InlineHistoryCount = inlineHistoryCount;
		OfflineHistoryCount = offlineHistoryCount;
		ImageId = imageId;
		ToolResults = toolResults ?? new List<InspectionToolResult>().AsReadOnly();
	}

	public int StationNumber { get; }

	public string ImagePath { get; }

	public InspectionBin Bin { get; }

	public int ParetoIndex { get; }

	public string ErrorMessage { get; }

	public bool LocateSucceeded { get; }

	public int InlineHistoryCount { get; }

	public int OfflineHistoryCount { get; }

	public int ImageId { get; }

	/// <summary>
	/// Per-tool diagnostic snapshot from the same inspection call. Only tools enabled by the
	/// loaded recipe are included. Consumers can use State == Ng/Retest to identify the actual
	/// runtime tool result instead of inferring a cause from final Bin/Pareto alone.
	/// </summary>
	public IReadOnlyList<InspectionToolResult> ToolResults { get; }
}

public sealed class RecipeStationSnapshot
{
	internal RecipeStationSnapshot(int stationNumber, string recipeName, bool stationEnabled,
		bool enhanceEnabled, bool predictEnabled, bool prophetEnabled,
		string enhanceModelName, int enhanceWidth, int enhanceHeight,
		bool enhanceGpuAssignmentEnabled, int enhanceGpuId, bool enhanceUseTensorRt,
		string predictModelName, int predictWidth, int predictHeight, int predictGpuId,
		bool predictUseTensorRt, string message)
	{
		StationNumber = stationNumber;
		RecipeName = recipeName ?? string.Empty;
		StationEnabled = stationEnabled;
		EnhanceEnabled = enhanceEnabled;
		PredictEnabled = predictEnabled;
		ProphetEnabled = prophetEnabled;
		EnhanceModelName = enhanceModelName ?? string.Empty;
		EnhanceWidth = enhanceWidth;
		EnhanceHeight = enhanceHeight;
		EnhanceGpuAssignmentEnabled = enhanceGpuAssignmentEnabled;
		EnhanceGpuId = enhanceGpuId;
		EnhanceUseTensorRt = enhanceUseTensorRt;
		PredictModelName = predictModelName ?? string.Empty;
		PredictWidth = predictWidth;
		PredictHeight = predictHeight;
		PredictGpuId = predictGpuId;
		PredictUseTensorRt = predictUseTensorRt;
		Message = message ?? string.Empty;
	}

	public int StationNumber { get; }
	public string RecipeName { get; }
	public bool StationEnabled { get; }
	public bool EnhanceEnabled { get; }
	public bool PredictEnabled { get; }
	public bool ProphetEnabled { get; }
	public string EnhanceModelName { get; }
	public int EnhanceWidth { get; }
	public int EnhanceHeight { get; }
	public bool EnhanceGpuAssignmentEnabled { get; }
	public int EnhanceGpuId { get; }
	public bool EnhanceUseTensorRt { get; }
	public string PredictModelName { get; }
	public int PredictWidth { get; }
	public int PredictHeight { get; }
	public int PredictGpuId { get; }
	public bool PredictUseTensorRt { get; }
	public string Message { get; }
}

public sealed class RecipeSnapshot
{
	internal RecipeSnapshot(string recipeName, IReadOnlyList<RecipeStationSnapshot> stations, string message)
	{
		RecipeName = recipeName ?? string.Empty;
		Stations = stations ?? throw new ArgumentNullException(nameof(stations));
		Message = message ?? string.Empty;
	}

	public string RecipeName { get; }

	public IReadOnlyList<RecipeStationSnapshot> Stations { get; }

	public string Message { get; }

	public bool Success => string.IsNullOrEmpty(Message);
}

public sealed class ModelLoadResult
{
	internal ModelLoadResult(int stationNumber, ModelFamily family, string modelName, bool success,
		bool loaded, bool useTensorRt, string message)
	{
		StationNumber = stationNumber;
		Family = family;
		ModelName = modelName ?? string.Empty;
		Success = success;
		Loaded = loaded;
		UseTensorRt = useTensorRt;
		Message = message ?? string.Empty;
	}

	public int StationNumber { get; }
	public ModelFamily Family { get; }
	public string ModelName { get; }
	public bool Success { get; }
	public bool Loaded { get; }
	public bool UseTensorRt { get; }
	public string Message { get; }
}

/// <summary>
/// Read-only lifecycle plan. It deliberately does not expose a method that commits production artifacts.
/// </summary>
public sealed class TrainingPlan
{
	internal TrainingPlan(int stationNumber, ModelFamily family, string modelName,
		IReadOnlyList<int> sharedStationNumbers, string activeSourcePath, string candidateSourcePath,
		string activeDerivativePath, string candidateDerivativePath, string trainingWorkingModelPath,
		string trainingParameterPath, RuntimeBackend runtimeBackend)
	{
		StationNumber = stationNumber;
		Family = family;
		ModelName = modelName ?? string.Empty;
		SharedStationNumbers = sharedStationNumbers ?? throw new ArgumentNullException(nameof(sharedStationNumbers));
		ActiveSourcePath = activeSourcePath ?? string.Empty;
		CandidateSourcePath = candidateSourcePath ?? string.Empty;
		ActiveDerivativePath = activeDerivativePath ?? string.Empty;
		CandidateDerivativePath = candidateDerivativePath ?? string.Empty;
		TrainingWorkingModelPath = trainingWorkingModelPath ?? string.Empty;
		TrainingParameterPath = trainingParameterPath ?? string.Empty;
		RuntimeBackend = runtimeBackend;
	}

	public int StationNumber { get; }
	public ModelFamily Family { get; }
	public string ModelName { get; }
	public IReadOnlyList<int> SharedStationNumbers { get; }
	public string ActiveSourcePath { get; }
	public string CandidateSourcePath { get; }
	public string ActiveDerivativePath { get; }
	public string CandidateDerivativePath { get; }
	public string TrainingWorkingModelPath { get; }
	public string TrainingParameterPath { get; }
	public RuntimeBackend RuntimeBackend { get; }

	public bool RequiresTraining => true;
	public bool RequiresCandidateValidation => true;
	public bool RequiresExplicitCommit => true;
	public bool ProductionCommitPerformed => false;
}

