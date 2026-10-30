namespace InspTools.AOI;

public struct INSP_MEASURETOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public int SearchRect1T;

	public int SearchRect1B;

	public int SearchRect1L;

	public int SearchRect1R;

	public int SearchRect2T;

	public int SearchRect2B;

	public int SearchRect2L;

	public int SearchRect2R;

	public COLOR_CHANNEL THD1_ChannelIndex;

	public COLOR_CHANNEL THD2_ChannelIndex;

	public int THD1_Min;

	public int THD2_Min;

	public int THD1_Max;

	public int THD2_Max;

	public int THD_AR_Min;

	public int THD_AR_Max;

	public int THD_AG_Min;

	public int THD_AG_Max;

	public int THD_AB_Min;

	public int THD_AB_Max;

	public int THD_BR_Min;

	public int THD_BR_Max;

	public int THD_BG_Min;

	public int THD_BG_Max;

	public int THD_BB_Min;

	public int THD_BB_Max;

	public bool EnableRGBThdA;

	public bool EnableRGBThdB;

	public int DE_Start1_Count;

	public int DE_Start2_Count;

	public int DE_End1_Count;

	public int DE_End2_Count;

	public USE_DILATE_ERODE DE_Start1_Index;

	public USE_DILATE_ERODE DE_Start2_Index;

	public USE_DILATE_ERODE DE_End1_Index;

	public USE_DILATE_ERODE DE_End2_Index;

	public ScanDirection Direction1;

	public ScanDirection Direction2;

	public MeasureCondition Scan1Rule;

	public MeasureCondition Scan2Rule;

	public double SinglePtLengthMin;

	public double SinglePtLengthMax;

	public double LengthMin;

	public double LengthMax;

	public double LengthDiffMax;

	public int ContinueError;

	public int MaxError;

	public bool UseSinglePt;

	public int Scan1MinValue;

	public int Scan1MaxValue;

	public int Scan1AvgValue;

	public int Scan2MinValue;

	public int Scan2MaxValue;

	public int Scan2AvgValue;

	public double LengthMinValue;

	public double LengthMaxValue;

	public double LengthAvgValue;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public bool CheckEveryone;

	public int MaskSelectIndex;

	public USE_DILATE_ERODE MaskDE_Start1_Index;

	public int MaskDE_Start1_Count;

	public FLOW_BIN ErrorBin;
}
