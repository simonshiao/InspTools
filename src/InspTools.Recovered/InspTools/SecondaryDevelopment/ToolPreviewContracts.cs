using System;

namespace InspTools.SecondaryDevelopment;

/// <summary>
/// Stable Advanced-Locate preview families.  These values describe the public API only;
/// callers never need to reference vendor AOI enums or classes.
/// </summary>
public enum ToolPreviewKind
{
	VerticalLocate = 0,
	HorizontalLocate = 1,
	AdvancedLocate = 2,
	AdvancedMeasure = 3
}

/// <summary>
/// Immutable integer image-space rectangle.  This intentionally replaces OpenCvSharp.Rect
/// at the secondary-development boundary.
/// </summary>
public sealed class ToolRectSnapshot
{
	internal ToolRectSnapshot(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public int X { get; }
	public int Y { get; }
	public int Width { get; }
	public int Height { get; }
	public int Right => X + Width;
	public int Bottom => Y + Height;
	public bool IsEmpty => Width <= 0 || Height <= 0;
}

/// <summary>
/// Immutable integer image-space point for measurement/preview overlays.
/// </summary>
public sealed class ToolPointSnapshot
{
	internal ToolPointSnapshot(int x, int y)
	{
		X = x;
		Y = y;
	}

	public int X { get; }
	public int Y { get; }
}

/// <summary>
/// Result of one isolated Advanced-Locate-family preview.  The facade prepares only the
/// required locate/enhance/prerequisite chain and executes the requested tool; downstream
/// Binary/Match/AI inspection stages are not run.
/// </summary>
public sealed class ToolPreviewResult
{
	internal ToolPreviewResult(int stationNumber, string imagePath, string section, ToolPreviewKind kind,
		InspectionToolResult toolResult, InspectionBin configuredErrorBin, string errorMessage,
		bool locateSucceeded, int imageWidth, int imageHeight, ToolRectSnapshot locateRect,
		ToolRectSnapshot inspectionRect, ToolRectSnapshot resultRect, ToolRectSnapshot referenceRect1,
		ToolRectSnapshot referenceRect2, ToolPointSnapshot point1, ToolPointSnapshot point2,
		int? scanValue, double? widthValue, double? heightValue, double? measureValue)
	{
		StationNumber = stationNumber;
		ImagePath = imagePath ?? string.Empty;
		Section = section ?? string.Empty;
		Kind = kind;
		ToolResult = toolResult ?? throw new ArgumentNullException(nameof(toolResult));
		ConfiguredErrorBin = configuredErrorBin;
		ErrorMessage = errorMessage ?? string.Empty;
		LocateSucceeded = locateSucceeded;
		ImageWidth = imageWidth;
		ImageHeight = imageHeight;
		LocateRect = locateRect ?? new ToolRectSnapshot(0, 0, 0, 0);
		InspectionRect = inspectionRect ?? new ToolRectSnapshot(0, 0, 0, 0);
		ResultRect = resultRect ?? new ToolRectSnapshot(0, 0, 0, 0);
		ReferenceRect1 = referenceRect1 ?? new ToolRectSnapshot(0, 0, 0, 0);
		ReferenceRect2 = referenceRect2 ?? new ToolRectSnapshot(0, 0, 0, 0);
		Point1 = point1 ?? new ToolPointSnapshot(0, 0);
		Point2 = point2 ?? new ToolPointSnapshot(0, 0);
		ScanValue = scanValue;
		WidthValue = widthValue;
		HeightValue = heightValue;
		MeasureValue = measureValue;
	}

	public int StationNumber { get; }
	public string ImagePath { get; }
	public string Section { get; }
	public ToolPreviewKind Kind { get; }
	public InspectionToolResult ToolResult { get; }
	public InspectionBin ConfiguredErrorBin { get; }
	public string ErrorMessage { get; }
	public bool LocateSucceeded { get; }
	public int ImageWidth { get; }
	public int ImageHeight { get; }
	public ToolRectSnapshot LocateRect { get; }
	public ToolRectSnapshot InspectionRect { get; }
	public ToolRectSnapshot ResultRect { get; }
	public ToolRectSnapshot ReferenceRect1 { get; }
	public ToolRectSnapshot ReferenceRect2 { get; }
	public ToolPointSnapshot Point1 { get; }
	public ToolPointSnapshot Point2 { get; }
	public int? ScanValue { get; }
	public double? WidthValue { get; }
	public double? HeightValue { get; }
	public double? MeasureValue { get; }

	/// <summary>
	/// True for this API contract: only the requested tool and its prerequisites are executed.
	/// </summary>
	public bool Isolated => true;
}
