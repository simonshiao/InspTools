using OpenCvSharp;

namespace InspTools.AOI;

public class MatchPattern
{
	public bool PatternON;

	public Mat PatternImage;

	public string PatternName;

	public string PatternPath;

	public Rect PatternRect;

	public int PatternChannel;

	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public MatchPattern()
	{
		PatternON = false;
		PatternImage = new Mat();
		PatternName = "";
		PatternPath = "";
		PatternRect = new Rect(0, 0, 50, 50);
		PatternChannel = 0;
		ValueChangeFlag = false;
	}
}
