using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_LOCATE_PARAM
{
	public bool ValueChangeFlag;

	public string TrackerName;

	public int LocateType;

	public double LocateAngleLimit;

	public double LocateHeightMin;

	public double LocateHeightMax;

	public double LocateWidthMin;

	public double LocateWidthMax;

	public double doRotateMinAngle;

	public FLOW_BIN LocateErrorBin;

	public FLOW_BIN LocateSizeErrorBin;

	public int ParetoIndex;

	public bool LocateSpeedUp;

	public bool CreateHSVChannel;

	public Rect MaxSearchRect;

	public bool EnableFocusCheck;

	public double FocusLimit;

	public Rect SIMPSearchRect;

	public int SIMP_THD1_Channel;

	public int SIMP_THD2_Channel;

	public int SIMP_THD3_Channel;

	public int SIMP_THD1_Min;

	public int SIMP_THD1_Max;

	public int SIMP_THD2_Min;

	public int SIMP_THD2_Max;

	public int SIMP_THD3_Min;

	public int SIMP_THD3_Max;

	public bool SIMP_AdvancdeMode;

	public bool SIMP_SideLocate;

	public bool SIMP_THD2_Enabled;

	public bool SIMP_THD3_Enabled;

	public int SIMP_DilateErode1StartCount;

	public int SIMP_DilateErode1EndCount;

	public int SIMP_DilateErode2StartCount;

	public int SIMP_DilateErode2EndCount;

	public int SIMP_DilateErode3StartCount;

	public int SIMP_DilateErode3EndCount;

	public USE_DILATE_ERODE SIMP_DilateErode1Start;

	public USE_DILATE_ERODE SIMP_DilateErode1End;

	public USE_DILATE_ERODE SIMP_DilateErode2Start;

	public USE_DILATE_ERODE SIMP_DilateErode2End;

	public USE_DILATE_ERODE SIMP_DilateErode3Start;

	public USE_DILATE_ERODE SIMP_DilateErode3End;

	public Rect QuadSearchRectL;

	public Rect QuadSearchRectR;

	public Rect QuadSearchRectT;

	public Rect QuadSearchRectB;

	public int Quad_Channel_L;

	public int Quad_Channel_R;

	public int Quad_Channel_T;

	public int Quad_Channel_B;

	public int Quad_THD_Min_L;

	public int Quad_THD_Max_L;

	public int Quad_THD_Min_R;

	public int Quad_THD_Max_R;

	public int Quad_THD_Min_T;

	public int Quad_THD_Max_T;

	public int Quad_THD_Min_B;

	public int Quad_THD_Max_B;

	public int Quad_THDR_Min_L;

	public int Quad_THDR_Max_L;

	public int Quad_THDR_Min_R;

	public int Quad_THDR_Max_R;

	public int Quad_THDR_Min_T;

	public int Quad_THDR_Max_T;

	public int Quad_THDR_Min_B;

	public int Quad_THDR_Max_B;

	public int Quad_THDG_Min_L;

	public int Quad_THDG_Max_L;

	public int Quad_THDG_Min_R;

	public int Quad_THDG_Max_R;

	public int Quad_THDG_Min_T;

	public int Quad_THDG_Max_T;

	public int Quad_THDG_Min_B;

	public int Quad_THDG_Max_B;

	public int Quad_THDB_Min_L;

	public int Quad_THDB_Max_L;

	public int Quad_THDB_Min_R;

	public int Quad_THDB_Max_R;

	public int Quad_THDB_Min_T;

	public int Quad_THDB_Max_T;

	public int Quad_THDB_Min_B;

	public int Quad_THDB_Max_B;

	public int Quad_DEStartCount_L;

	public int Quad_DEEndCount_L;

	public int Quad_DEStartCount_R;

	public int Quad_DEEndCount_R;

	public int Quad_DEStartCount_T;

	public int Quad_DEEndCount_T;

	public int Quad_DEStartCount_B;

	public int Quad_DEEndCount_B;

	public bool QUAD_SideLocate;

	public bool Quad_EnableRGBThd_L;

	public bool Quad_EnableRGBThd_R;

	public bool Quad_EnableRGBThd_T;

	public bool Quad_EnableRGBThd_B;

	public bool Quad_FilterPoint;

	public bool Quad_UseStdFilter;

	public bool Quad_UseSideLocate;

	public bool Quad_FixSideLocateLR;

	public int Quad_LocateAngleSelect;

	public USE_DILATE_ERODE Quad_DEStart_L;

	public USE_DILATE_ERODE Quad_DEEnd_L;

	public USE_DILATE_ERODE Quad_DEStart_R;

	public USE_DILATE_ERODE Quad_DEEnd_R;

	public USE_DILATE_ERODE Quad_DEStart_T;

	public USE_DILATE_ERODE Quad_DEEnd_T;

	public USE_DILATE_ERODE Quad_DEStart_B;

	public USE_DILATE_ERODE Quad_DEEnd_B;

	public ScanDirection Direction_L;

	public ScanDirection Direction_R;

	public ScanDirection Direction_T;

	public ScanDirection Direction_B;

	public Rect Quad_Mask_T;

	public Rect Quad_Mask_B;

	public Rect Quad_Mask_L;

	public Rect Quad_Mask_R;

	public bool Quad_EnableMask_T;

	public bool Quad_EnableMask_B;

	public bool Quad_EnableMask_L;

	public bool Quad_EnableMask_R;

	public bool Quad_EnableAvgEdge_T;

	public bool Quad_EnableAvgEdge_B;

	public bool Quad_EnableAvgEdge_L;

	public bool Quad_EnableAvgEdge_R;

	public bool UseMelfMode;

	public Rect PadSearchRectL;

	public Rect PadSearchRectR;

	public int Pad_Channel_L1;

	public int Pad_Channel_L2;

	public int Pad_Channel_R1;

	public int Pad_Channel_R2;

	public int Pad_THD_Min_L1;

	public int Pad_THD_Max_L1;

	public int Pad_THD_Min_L2;

	public int Pad_THD_Max_L2;

	public int Pad_THD_Min_R1;

	public int Pad_THD_Max_R1;

	public int Pad_THD_Min_R2;

	public int Pad_THD_Max_R2;

	public int Pad_DEStartCount_L1;

	public int Pad_DEEndCount_L1;

	public int Pad_DEStartCount_L2;

	public int Pad_DEEndCount_L2;

	public int Pad_DEStartCount_R1;

	public int Pad_DEEndCount_R1;

	public int Pad_DEStartCount_R2;

	public int Pad_DEEndCount_R2;

	public USE_DILATE_ERODE Pad_DEStart_L1;

	public USE_DILATE_ERODE Pad_DEEnd_L1;

	public USE_DILATE_ERODE Pad_DEStart_L2;

	public USE_DILATE_ERODE Pad_DEEnd_L2;

	public USE_DILATE_ERODE Pad_DEStart_R1;

	public USE_DILATE_ERODE Pad_DEEnd_R1;

	public USE_DILATE_ERODE Pad_DEStart_R2;

	public USE_DILATE_ERODE Pad_DEEnd_R2;

	public bool EnableAILocate;

	public string AILocateModelName;

	public bool EnableTensorRT;

	public int AILocateTrainCount;

	public int ModelWidth;

	public int ModelHeight;

	public int ModelDiv;

	public int ModelBatchSize;
}
