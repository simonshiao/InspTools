using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_BINARYTOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public Rect SearchRect1;

	public Rect SearchRect2;

	public bool SearchRect2ON;

	public bool Tracker1UseAutoSize;

	public bool Tracker2UseAutoSize;

	public int Tracker1AligmentXIndex;

	public int Tracker1AligmentYIndex;

	public int Tracker2AligmentXIndex;

	public int Tracker2AligmentYIndex;

	public int Tracker1AligmentX;

	public int Tracker2AligmentX;

	public int Tracker1AligmentY;

	public int Tracker2AligmentY;

	public int MaskSelectIndex;

	public bool UseMaskAsInspArea;

	public int AutoSize1_SelectIndex;

	public int AutoSize2_SelectIndex;

	public int Track1_T;

	public int Track1_B;

	public int Track1_L;

	public int Track1_R;

	public int Track2_T;

	public int Track2_B;

	public int Track2_L;

	public int Track2_R;

	public int ColorReferenceIndex;

	public COLOR_CHANNEL THD1_ChannelIndex;

	public COLOR_CHANNEL THD2_ChannelIndex;

	public COLOR_CHANNEL THD3_ChannelIndex;

	public COLOR_CHANNEL THD4_ChannelIndex;

	public bool THD1_ChannelON;

	public bool THD2_ChannelON;

	public bool THD3_ChannelON;

	public bool THD4_ChannelON;

	public bool THD_ORMode;

	public int THD1_Min;

	public int THD2_Min;

	public int THD3_Min;

	public int THD4_Min;

	public int THD1_Max;

	public int THD2_Max;

	public int THD3_Max;

	public int THD4_Max;

	public int DE_Start1_Count;

	public int DE_Start2_Count;

	public int DE_Start3_Count;

	public int DE_Start4_Count;

	public int DE_End1_Count;

	public int DE_End2_Count;

	public int DE_End3_Count;

	public int DE_End4_Count;

	public USE_DILATE_ERODE DE_Start1_Index;

	public USE_DILATE_ERODE DE_Start2_Index;

	public USE_DILATE_ERODE DE_Start3_Index;

	public USE_DILATE_ERODE DE_Start4_Index;

	public USE_DILATE_ERODE DE_End1_Index;

	public USE_DILATE_ERODE DE_End2_Index;

	public USE_DILATE_ERODE DE_End3_Index;

	public USE_DILATE_ERODE DE_End4_Index;

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

	public BlobCondition MB_InspRule1_Index;

	public BlobCondition MB_InspRule2_Index;

	public BlobCondition MB_InspRule3_Index;

	public BlobCondition MB_InspRule4_Index;

	public double MB_InspRule1_Min;

	public double MB_InspRule1_Max;

	public double MB_InspRule2_Min;

	public double MB_InspRule2_Max;

	public double MB_InspRule3_Min;

	public double MB_InspRule3_Max;

	public double MB_InspRule4_Min;

	public double MB_InspRule4_Max;

	public int MB_BlobSort;

	public int MB_BlobNum;

	public bool MB_InspRuleAND;

	public int MB_AllowNGCount;

	public int Use_THD_ReferenceIndex;

	public int Use_Rule_ReferenceIndex;

	public bool InspRuleAND;

	public int BlobAnalysisSelect;

	public int BlobLineSort;

	public int BlobLineNum;

	public int BlobLineMin;

	public int BlobLineMax;

	public double BlobLineWidthMin;

	public double BlobLineWidthMax;

	public double BlobLineHeightMin;

	public double BlobLineHeightMax;

	public double BlobLineAreaMin;

	public double BlobLineAreaMax;

	public int LineWidthMinW;

	public int LineWidthMinH;

	public int LineWidthMinArea;

	public bool EnableLineWidthA;

	public bool EnableLineWidthW;

	public bool EnableLineWidthH;

	public bool UsePixelValue;

	public int AllowLineWidth;

	public int AllowLineHeight;

	public int AllowLineError;

	public int AllowErrorW;

	public int AllowErrorH;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public bool EnableSoftCorrection;

	public bool UseFontMask;

	public int UseMatchTextMask1;

	public int UseMatchTextMask2;

	public int UseMatchTextMask3;

	public int MatchTextDE_Start1_Count;

	public int MatchTextDE_Start2_Count;

	public int MatchTextDE_Start3_Count;

	public USE_DILATE_ERODE MatchTextDE_Start1_Index;

	public USE_DILATE_ERODE MatchTextDE_Start2_Index;

	public USE_DILATE_ERODE MatchTextDE_Start3_Index;

	public USE_DILATE_ERODE MaskDE_Start1_Index;

	public int MaskDE_Start1_Count;

	public int T1FillletCount0;

	public int T1FillletCount1;

	public int T1FillletCount2;

	public int T1FillletCount3;

	public int T2FillletCount0;

	public int T2FillletCount1;

	public int T2FillletCount2;

	public int T2FillletCount3;

	public bool TrackErrorSetNG;

	public FLOW_BIN ErrorBin;
}
