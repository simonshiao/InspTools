namespace InspTools.AOI;

public struct INSP_ADVMEASURETOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public int MeasureFunction;

	public int MStartSelect;

	public int MEndSelect;

	public int MStartWHIndex;

	public int MEndWHIndex;

	public int MStartAIndex;

	public int MEndAIndex;

	public int MStartAPt;

	public int MEndAPt;

	public int ScanExtend;

	public int ScanShrink;

	public bool MeasureAllX;

	public bool MeasureAllY;

	public double ValueMax;

	public double ValueMin;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public double MeasureValue;

	public FLOW_BIN ErrorBin;
}
