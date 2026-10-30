using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_DIFFERENCETOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public Rect SearchRect1;

	public Rect SearchRect2;

	public bool SearchRect2ON;

	public bool FilterNoise;

	public int NoiseWidth;

	public int NoiseHeight;

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

	public COLOR_CHANNEL ChannelIndex;

	public int InspDirection;

	public int P2PIntervalX;

	public int P2PIntervalY;

	public bool ThdUpON;

	public int ThdUp;

	public bool ThdDownON;

	public int ThdDown;

	public int THDLimit_Min;

	public int THDLimit_Max;

	public int DE_Start_Count;

	public int DE_End_Count;

	public USE_DILATE_ERODE DE_Start_Index;

	public USE_DILATE_ERODE DE_End_Index;

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

	public bool InspRuleAND;

	public bool UseAvgBinary;

	public int AvgBinaryMethod;

	public int AvgShiftValue;

	public int AvgGrayMax;

	public int AvgGrayMin;

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

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

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
