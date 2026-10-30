using OpenCvSharp;

namespace InspTools.AOI;

public struct BlobStruct
{
	public double Width;

	public double Height;

	public double Area;

	public double HeightDiff;

	public double WidthDiff;

	public double WidthRatio;

	public double HeightRatio;

	public double WHRatio;

	public double AreaRatio;

	public double TwoAreaRatio;

	public double TotalArea;

	public double TotalAreaRatio;

	public double TotalAreaDiff;

	public Rect NGRect;

	public bool result;

	public int ImageListSource;

	public int ImageAddr;

	public int TotalWidth;

	public int TotalHeight;
}
