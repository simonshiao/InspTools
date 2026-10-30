using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_LINEANALYSISTOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public Rect SearchRect1;

	public bool Tracker1UseAutoSize;

	public int Tracker1AligmentXIndex;

	public int Tracker1AligmentYIndex;

	public int Tracker1AligmentX;

	public int Tracker1AligmentY;

	public int MaskSelectIndex;

	public bool UseMaskAsInspArea;

	public int AutoSize1_SelectIndex;

	public int Track1_T;

	public int Track1_B;

	public int Track1_L;

	public int Track1_R;

	public COLOR_CHANNEL THD1_ChannelIndex;

	public COLOR_CHANNEL THD2_ChannelIndex;

	public COLOR_CHANNEL THD3_ChannelIndex;

	public bool THD23_ChannelON;

	public int THD1_Min;

	public int THD2_Min;

	public int THD3_Min;

	public int THD1_Max;

	public int THD2_Max;

	public int THD3_Max;

	public int THD1_DEStart;

	public int THD1_DEStartCount;

	public int THD2_DEStart;

	public int THD2_DEStartCount;

	public int THD3_DEStart;

	public int THD3_DEStartCount;

	public int THD1_DEEnd;

	public int THD1_DEEndCount;

	public int THD2_DEEnd;

	public int THD2_DEEndCount;

	public int THD3_DEEnd;

	public int THD3_DEEndCount;

	public bool UseStdFilter;

	public bool UseLineFilter;

	public bool EnableDistanceCheck;

	public double AllowDistance;

	public int AllowDistanceErrorCount;

	public int AllowDistanceContErrorCount;

	public int DistanceLineSegment;

	public int FindEdgeMethod;

	public ScanDirection Direction;

	public int P2PInterval;

	public int P2PAllowValue;

	public int ContinueError;

	public int TotalError;

	public double LineAngleMin;

	public double LineAngleMax;

	public int LineSegment;

	public double LineAngleDiff;

	public bool EnableAngleCheck;

	public bool EnablePointCheck;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public FLOW_BIN ErrorBin;
}
