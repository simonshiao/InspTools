using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_FONTTOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool Locked;

	public Rect SearchRect1;

	public bool Tracker1UseAutoSize;

	public int Track1_T;

	public int Track1_B;

	public int Track1_L;

	public int Track1_R;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public bool EnableRotateInspect;

	public int PatternSelect;

	public int MaskSelectIndex;

	public int MinMatchScore;

	public string AllFont;

	public string FontChar1;

	public string FontChar2;

	public string FontChar3;

	public string FontChar4;

	public string FontChar5;

	public string FontChar6;

	public string FontChar7;

	public string FontChar8;

	public string FontChar9;

	public int FontIndex1;

	public int FontIndex2;

	public int FontIndex3;

	public int FontIndex4;

	public int FontIndex5;

	public int FontIndex6;

	public int FontIndex7;

	public int FontIndex8;

	public int FontIndex9;

	public bool DoubleWidthFont1;

	public bool DoubleWidthFont2;

	public bool DoubleWidthFont3;

	public bool DoubleWidthFont4;

	public bool DoubleWidthFont5;

	public bool DoubleWidthFont6;

	public bool DoubleWidthFont7;

	public bool DoubleWidthFont8;

	public bool DoubleWidthFont9;

	public bool EnableAI_Font1;

	public bool EnableAI_Font2;

	public bool EnableAI_Font3;

	public bool EnableAI_Font4;

	public bool EnableAI_Font5;

	public bool EnableAI_Font6;

	public bool EnableAI_Font7;

	public bool EnableAI_Font8;

	public bool EnableAI_Font9;

	public bool EnableFontCheck1;

	public bool EnableFontCheck2;

	public bool EnableFontCheck3;

	public bool EnableFontCheck4;

	public bool EnableFontCheck5;

	public bool EnableFontCheck6;

	public bool EnableFontCheck7;

	public bool EnableFontCheck8;

	public bool EnableFontCheck9;

	public bool BypassOCR1;

	public bool BypassOCR2;

	public bool BypassOCR3;

	public bool BypassOCR4;

	public bool BypassOCR5;

	public bool BypassOCR6;

	public bool BypassOCR7;

	public bool BypassOCR8;

	public bool BypassOCR9;

	public string AIModel_Font1;

	public string AIModel_Font2;

	public string AIModel_Font3;

	public string AIModel_Font4;

	public string AIModel_Font5;

	public string AIModel_Font6;

	public string AIModel_Font7;

	public string AIModel_Font8;

	public string AIModel_Font9;

	public int MinScore_Font1;

	public int MinScore_Font2;

	public int MinScore_Font3;

	public int MinScore_Font4;

	public int MinScore_Font5;

	public int MinScore_Font6;

	public int MinScore_Font7;

	public int MinScore_Font8;

	public int MinScore_Font9;

	public int MinScore_ALL;

	public int FontExtend;

	public int WFontExtend1;

	public int WFontExtend2;

	public int WFontExtend3;

	public int WFontExtend4;

	public int WFontExtend5;

	public int WFontExtend6;

	public int WFontExtend7;

	public int WFontExtend8;

	public int WFontExtend9;

	public int HFontExtend1;

	public int HFontExtend2;

	public int HFontExtend3;

	public int HFontExtend4;

	public int HFontExtend5;

	public int HFontExtend6;

	public int HFontExtend7;

	public int HFontExtend8;

	public int HFontExtend9;

	public bool UsePublicDB;

	public bool UseSameScore;

	public bool FontDBUsePublicMode;

	public bool PublicDBUsePublicMode;

	public bool FontAlignVertical;

	public bool EnabledOCR;

	public bool FontIsBlack;

	public bool AutoLoadAI;

	public bool FontConnected;

	public int PatternChannel;

	public bool doAutoCombineFontToDB;

	public bool AutoCombineFontToDBSet;

	public bool AutoCombineFontCreate;

	public Point Font9Center1;

	public Point Font9Center2;

	public Point Font9Center3;

	public Point Font9Center4;

	public Point Font9Center5;

	public Point Font9Center6;

	public Point Font9Center7;

	public Point Font9Center8;

	public Point Font9Center9;

	public Mat TextMat;

	public Rect FoundMatchTextRect;

	public bool R_LT_H;

	public bool R_LT_W;

	public bool R_RT_H;

	public bool R_RT_W;

	public bool R_LB_H;

	public bool R_RB_H;

	public bool R_Area;

	public bool R_T_W;

	public bool R_B_W;

	public bool R_L_H;

	public bool R_R_H;

	public bool R_C;

	public bool R_C_H_MIN;

	public bool R_LR_H_Diff;

	public bool R_Break;

	public double R_LT_H_P;

	public double R_LT_W_P;

	public double R_RT_H_P;

	public double R_RT_W_P;

	public double R_LB_H_P;

	public double R_RB_H_P;

	public double R_Area_P;

	public double R_T_W_P;

	public double R_B_W_P;

	public double R_L_H_P;

	public double R_R_H_P;

	public double R_C_P;

	public double R_C_H_MIN_P;

	public double R_C_H_MAX_P;

	public double R_LR_H_Diff_P;

	public double R_Break_PMin;

	public double R_Break_PMax;

	public int R_BinaryShift;

	public int R_OpenClose;

	public int R_OpenCloseCount;

	public int R_OpenClose2;

	public int R_OpenCloseCount2;

	public bool R_UseMorphFirst;

	public bool R_UseFontDBMask;

	public bool M_T_W;

	public bool M_B_W;

	public bool M_W;

	public bool M_Break;

	public bool M_Leg1;

	public bool M_Leg2;

	public bool M_Leg3;

	public bool M_Connected;

	public int M_BinaryShift;

	public int M_OpenClose;

	public int M_OpenCloseCount;

	public int M_OpenClose2;

	public int M_OpenCloseCount2;

	public bool M_UseMorphFirst;

	public bool M_UseFontDBMask;

	public bool C_T_W;

	public bool C_B_W;

	public bool C_L_H;

	public bool C_R_H;

	public bool C_H;

	public bool C_W;

	public bool C_Break;

	public int C_BinaryShift;

	public int C_OpenClose;

	public int C_OpenCloseCount;

	public int C_OpenClose2;

	public int C_OpenCloseCount2;

	public bool C_UseMorphFirst;

	public bool C_UseFontDBMask;

	public bool U_T_W;

	public bool U_B_W;

	public bool U_L_H;

	public bool U_R_H;

	public bool U_H;

	public bool U_W;

	public bool U_Break;

	public int U_BinaryShift;

	public int U_OpenClose;

	public int U_OpenCloseCount;

	public int U_OpenClose2;

	public int U_OpenCloseCount2;

	public bool U_UseMorphFirst;

	public bool U_UseFontDBMask;

	public bool X_T_W;

	public bool X_B_W;

	public bool X_L_H;

	public bool X_R_H;

	public bool X_H;

	public bool X_W;

	public bool X_Break;

	public int X_BinaryShift;

	public int X_OpenClose;

	public int X_OpenCloseCount;

	public int X_OpenClose2;

	public int X_OpenCloseCount2;

	public bool X_UseMorphFirst;

	public bool X_UseFontDBMask;

	public bool TB_LT_H;

	public bool TB_LT_W;

	public bool TB_RT_H;

	public bool TB_RT_W;

	public bool TB_LB_H;

	public bool TB_LB_W;

	public bool TB_RB_H;

	public bool TB_RB_W;

	public bool TB_B_W;

	public bool TB_C;

	public bool TB_C_H_MIN;

	public bool TB_C_H_MAX;

	public bool TB_Break;

	public int TB_BinaryShift;

	public int TB_OpenClose;

	public int TB_OpenCloseCount;

	public int TB_OpenClose2;

	public int TB_OpenCloseCount2;

	public bool TB_UseMorphFirst;

	public bool TB_UseFontDBMask;

	public bool TE_LT_H;

	public bool TE_LT_W;

	public bool TE_RT_H;

	public bool TE_RT_W;

	public bool TE_LB_H;

	public bool TE_LB_W;

	public bool TE_RB_H;

	public bool TE_RB_W;

	public bool TE_Break;

	public int TE_BinaryShift;

	public int TE_OpenClose;

	public int TE_OpenCloseCount;

	public int TE_OpenClose2;

	public int TE_OpenCloseCount2;

	public bool TE_UseMorphFirst;

	public bool TE_UseFontDBMask;

	public bool Dash_H;

	public bool Dash_W;

	public bool Dash_Break;

	public int Dash_BinaryShift;

	public int Dash_OpenClose;

	public int Dash_OpenCloseCount;

	public int Dash_OpenClose2;

	public int Dash_OpenCloseCount2;

	public bool Dash_UseMorphFirst;

	public bool Dash_UseFontDBMask;

	public bool D_LT_H;

	public bool D_LT_W;

	public bool D_RT_H;

	public bool D_RT_W;

	public bool D_LB_H;

	public bool D_LB_W;

	public bool D_RB_H;

	public bool D_RB_W;

	public bool D_T_W;

	public bool D_B_W;

	public bool D_C;

	public bool D_C_H_MIN;

	public bool D_C_H_MAX;

	public bool D_Break;

	public int D_BinaryShift;

	public int D_OpenClose;

	public int D_OpenCloseCount;

	public int D_OpenClose2;

	public int D_OpenCloseCount2;

	public bool D_UseMorphFirst;

	public bool D_UseFontDBMask;

	public bool A_T_W;

	public bool A_B_W;

	public bool A_L_H;

	public bool A_R_H;

	public bool A_H;

	public bool A_W;

	public bool A_Break;

	public int A_BinaryShift;

	public int A_OpenClose;

	public int A_OpenCloseCount;

	public int A_OpenClose2;

	public int A_OpenCloseCount2;

	public bool A_UseMorphFirst;

	public bool A_UseFontDBMask;

	public bool T0_LT_H;

	public bool T0_LT_W;

	public bool T0_RT_H;

	public bool T0_RT_W;

	public bool T0_LB_H;

	public bool T0_LB_W;

	public bool T0_RB_H;

	public bool T0_RB_W;

	public bool T0_T_W;

	public bool T0_B_W;

	public bool T0_C;

	public bool T0_C_H_MIN;

	public bool T0_C_H_MAX;

	public bool T0_Break;

	public bool T0_L_H;

	public bool T0_R_H;

	public int T0_BinaryShift;

	public int T0_OpenClose;

	public int T0_OpenCloseCount;

	public int T0_OpenClose2;

	public int T0_OpenCloseCount2;

	public bool T0_UseMorphFirst;

	public bool T0_UseFontDBMask;

	public bool T1_H;

	public bool T1_W;

	public bool T1_Break;

	public int T1_BinaryShift;

	public int T1_OpenClose;

	public int T1_OpenCloseCount;

	public int T1_OpenClose2;

	public int T1_OpenCloseCount2;

	public bool T1_UseMorphFirst;

	public bool T1_UseFontDBMask;

	public bool T2_LT_H;

	public bool T2_LT_W;

	public bool T2_RT_H;

	public bool T2_RT_W;

	public bool T2_LB_H;

	public bool T2_LB_W;

	public bool T2_RB_H;

	public bool T2_RB_W;

	public bool T2_Break;

	public int T2_BinaryShift;

	public int T2_OpenClose;

	public int T2_OpenCloseCount;

	public int T2_OpenClose2;

	public int T2_OpenCloseCount2;

	public bool T2_UseMorphFirst;

	public bool T2_UseFontDBMask;

	public bool T3_LT_H;

	public bool T3_LT_W;

	public bool T3_RT_H;

	public bool T3_RT_W;

	public bool T3_LB_H;

	public bool T3_LB_W;

	public bool T3_RB_H;

	public bool T3_RB_W;

	public bool T3_Break;

	public bool T3_T_W;

	public bool T3_B_W;

	public bool T3_C_W;

	public int T3_BinaryShift;

	public int T3_OpenClose;

	public int T3_OpenCloseCount;

	public int T3_OpenClose2;

	public int T3_OpenCloseCount2;

	public bool T3_UseMorphFirst;

	public bool T3_UseFontDBMask;

	public bool T4_LT_H;

	public bool T4_LT_W;

	public bool T4_RT_H;

	public bool T4_RT_W;

	public bool T4_T_W;

	public bool T4_B_W;

	public bool T4_Break;

	public bool T4_W;

	public bool T4_H;

	public int T4_BinaryShift;

	public int T4_OpenClose;

	public int T4_OpenCloseCount;

	public int T4_OpenClose2;

	public int T4_OpenCloseCount2;

	public bool T4_UseMorphFirst;

	public bool T4_UseFontDBMask;

	public bool T5_LT_H;

	public bool T5_LT_W;

	public bool T5_RT_H;

	public bool T5_RT_W;

	public bool T5_LB_H;

	public bool T5_LB_W;

	public bool T5_RB_H;

	public bool T5_RB_W;

	public bool T5_Break;

	public bool T5_TT_W;

	public bool T5_TB_W;

	public int T5_BinaryShift;

	public int T5_OpenClose;

	public int T5_OpenCloseCount;

	public int T5_OpenClose2;

	public int T5_OpenCloseCount2;

	public bool T5_UseMorphFirst;

	public bool T5_UseFontDBMask;

	public bool T6_LT_H;

	public bool T6_LT_W;

	public bool T6_RT_H;

	public bool T6_RT_W;

	public bool T6_T_W;

	public bool T6_B_W;

	public bool T6_Break;

	public bool T6_C;

	public int T6_BinaryShift;

	public int T6_OpenClose;

	public int T6_OpenCloseCount;

	public int T6_OpenClose2;

	public int T6_OpenCloseCount2;

	public bool T6_UseMorphFirst;

	public bool T6_UseFontDBMask;

	public bool T7_T_W;

	public bool T7_B_W;

	public bool T7_W;

	public bool T7_H;

	public bool T7_Break;

	public int T7_BinaryShift;

	public int T7_OpenClose;

	public int T7_OpenCloseCount;

	public int T7_OpenClose2;

	public int T7_OpenCloseCount2;

	public bool T7_UseMorphFirst;

	public bool T7_UseFontDBMask;

	public bool T8_LT_H;

	public bool T8_LT_W;

	public bool T8_RT_H;

	public bool T8_RT_W;

	public bool T8_LB_H;

	public bool T8_LB_W;

	public bool T8_RB_H;

	public bool T8_RB_W;

	public bool T8_B_W;

	public bool T8_C;

	public bool T8_C_H_MIN;

	public bool T8_C_H_MAX;

	public bool T8_Break;

	public bool T8_L_H;

	public bool T8_R_H;

	public int T8_BinaryShift;

	public int T8_OpenClose;

	public int T8_OpenCloseCount;

	public int T8_OpenClose2;

	public int T8_OpenCloseCount2;

	public bool T8_UseMorphFirst;

	public bool T8_UseFontDBMask;

	public bool T9_LT_H;

	public bool T9_LT_W;

	public bool T9_RT_H;

	public bool T9_RT_W;

	public bool T9_B_H;

	public bool T9_B_W;

	public bool T9_Break;

	public bool T9_C;

	public int T9_BinaryShift;

	public int T9_OpenClose;

	public int T9_OpenCloseCount;

	public int T9_OpenClose2;

	public int T9_OpenCloseCount2;

	public bool T9_UseMorphFirst;

	public bool T9_UseFontDBMask;

	public int ConnectBinaryShift;

	public int ConnectOpenClose;

	public int ConnectOpenCloseCount;

	public int ConnectOpenClose2;

	public int ConnectOpenCloseCount2;

	public bool ConnectCountAlarm;

	public bool ConnectWidthAlarm;

	public bool UseFastMatch;

	public bool AutoSetFontImage;

	public INSP_FONTTOOL_OTHERFONT_PARAM[] OtherFont;

	public FLOW_BIN ErrorBin;
}
