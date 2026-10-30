using OpenCvSharp;

namespace InspTools;

public class MyMat
{
	private Mat img;

	public MyMat(Mat org, Rect MaxRect)
	{
		img = new Mat(org, MaxRect);
	}

	public Mat Clone(Mat GiveBack_Img = null)
	{
		if (GiveBack_Img != null)
		{
			MM.GiveBack(GiveBack_Img);
		}
		return MM.CloneFrom(img);
	}
}
