namespace InspTools.AOI;

public struct INSP_SELECTFACETOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool TrackerON;

	public bool EnableMultiFace;

	public bool RowSelectFace;

	public bool RowMinSelectFace;

	public int SearchRectT;

	public int SearchRectB;

	public int SearchRectL;

	public int SearchRectR;

	public int SearchRectWidth;

	public int SearchRectHeight;

	public COLOR_CHANNEL THD1_ChannelIndex;

	public COLOR_CHANNEL THD2_ChannelIndex;

	public COLOR_CHANNEL THD3_ChannelIndex;

	public bool THD1_ChannelON;

	public bool THD2_ChannelON;

	public bool THD3_ChannelON;

	public int THD1_Min;

	public int THD2_Min;

	public int THD3_Min;

	public int THD1_Max;

	public int THD2_Max;

	public int THD3_Max;

	public bool THD2_ORMode;

	public bool THD3_ORMode;

	public int DE_Start1_Count;

	public int DE_Start2_Count;

	public int DE_Start3_Count;

	public int DE_End1_Count;

	public int DE_End2_Count;

	public int DE_End3_Count;

	public USE_DILATE_ERODE DE_Start1_Index;

	public USE_DILATE_ERODE DE_Start2_Index;

	public USE_DILATE_ERODE DE_Start3_Index;

	public USE_DILATE_ERODE DE_End1_Index;

	public USE_DILATE_ERODE DE_End2_Index;

	public USE_DILATE_ERODE DE_End3_Index;

	public BlobCondition InspRule1_Index;

	public BlobCondition InspRule2_Index;

	public BlobCondition InspRule3_Index;

	public BlobCondition InspRule4_Index;

	public double InspRule1_Min;

	public double InspRule1_Max;

	public double InspRule2_Min;

	public double InspRule2_Max;

	public double InspRule3_Min;

	public double InspRule3_Max;

	public double InspRule4_Min;

	public double InspRule4_Max;

	public FLOW_BIN ErrorBin;

	public bool InspRuleAND;

	public int RotateMethod;

	public bool UseAutoSize;
}
