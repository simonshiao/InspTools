using OpenCvSharp;

namespace InspTools.AOI;

public class SimpleBlobStruct
{
	public double Width;

	public double Height;

	public double Area;

	public Rect NGRect;

	public bool result;

	public bool AreaMinNG;

	public bool AreaMaxNG;
}
