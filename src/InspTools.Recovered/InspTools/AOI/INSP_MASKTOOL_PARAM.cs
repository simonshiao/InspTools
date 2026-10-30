using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_MASKTOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public Point Mask1Pt;

	public Point Mask2Pt;

	public Mat Mask1;

	public Mat Mask2;

	public Rect SearchRect1;

	public Rect SearchRect2;

	public bool SearchRect2ON;

	public bool SearchRect2AsMask;

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

	public COLOR_CHANNEL THD1_ChannelIndex;

	public COLOR_CHANNEL THD2_ChannelIndex;

	public COLOR_CHANNEL THD3_ChannelIndex;

	public COLOR_CHANNEL THD4_ChannelIndex;

	public bool THD1_ChannelON;

	public bool THD2_ChannelON;

	public bool THD3_ChannelON;

	public bool THD4_ChannelON;

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

	public USE_DILATE_ERODE DE_FinalStart_Index;

	public USE_DILATE_ERODE DE_FinalEnd_Index;

	public int DE_FinalStart_Count;

	public int DE_FinalEnd_Count;

	public int T1FillletCount0;

	public int T1FillletCount1;

	public int T1FillletCount2;

	public int T1FillletCount3;

	public int T2FillletCount0;

	public int T2FillletCount1;

	public int T2FillletCount2;

	public int T2FillletCount3;

	public bool FillHole;

	public bool doConvexHull;

	public int MaxBlob;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public bool EnableAdvLocateMask;

	public int AdvLocateStartIndex;

	public int AdvLocateEndIndex;

	public int AdvLocateBorder1;

	public int AdvLocateBorder2;

	public int AdvLocateExpand1;

	public int AdvLocateExpand2;

	public int AdvLocateExpandCount1;

	public int AdvLocateExpandCount2;

	public bool AdvLocateAsInsp;

	public int AdvLocateBorderFilllet;

	public int Combine1Select;

	public int Combine2Select;

	public int Combine3Select;

	public int Combine4Select;

	public int Combine5Select;

	public int Combine6Select;

	public int Combine7Select;

	public int Combine8Select;

	public int Combine1Rule;

	public int Combine2Rule;

	public int Combine3Rule;

	public int Combine4Rule;

	public int Combine5Rule;

	public int Combine6Rule;

	public int Combine7Rule;

	public int Combine8Rule;

	public int Combine1DE_Start_Index;

	public int Combine2DE_Start_Index;

	public int Combine3DE_Start_Index;

	public int Combine4DE_Start_Index;

	public int Combine5DE_Start_Index;

	public int Combine6DE_Start_Index;

	public int Combine7DE_Start_Index;

	public int Combine8DE_Start_Index;

	public int Combine1DE_Start_Count;

	public int Combine2DE_Start_Count;

	public int Combine3DE_Start_Count;

	public int Combine4DE_Start_Count;

	public int Combine5DE_Start_Count;

	public int Combine6DE_Start_Count;

	public int Combine7DE_Start_Count;

	public int Combine8DE_Start_Count;
}
