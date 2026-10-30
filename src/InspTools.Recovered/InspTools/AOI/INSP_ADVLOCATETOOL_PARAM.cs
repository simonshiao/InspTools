using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_ADVLOCATETOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public int T;

	public int B;

	public int L;

	public int R;

	public int Width;

	public int Height;

	public Rect InsideRect;

	public int RectIndexT;

	public int RectIndexB;

	public int RectIndexL;

	public int RectIndexR;

	public double WidthMax;

	public double WidthMin;

	public double HeightMax;

	public double HeightMin;

	public bool EnableFakeWidthRatio;

	public bool EnableFakeHeightRatio;

	public double WidthRatio;

	public double HeightRatio;

	public bool UseBinary;

	public int SearchRectT;

	public int SearchRectL;

	public int SearchRectB;

	public int SearchRectR;

	public int SearchRectW;

	public int SearchRectH;

	public double SearchRectShiftT;

	public double SearchRectShiftB;

	public double SearchRectShiftL;

	public double SearchRectShiftR;

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

	public bool UseAutoSize;

	public int AutoSizeIndex;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public FLOW_BIN ErrorBin;
}
