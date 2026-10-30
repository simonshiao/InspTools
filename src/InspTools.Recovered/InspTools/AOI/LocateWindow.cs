using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using OpenCvSharp;
using SAMViewer;

namespace InspTools.AOI;

public class LocateWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private bool inited;

	private bool THD_Preview;

	private int LocateType;

	private bool UseAILocate;

	private bool ShowMessage = true;

	private int QuadSelectIndex;

	private int PadSelectIndex;

	private int LanguageType = 1;

	private OpenCvSharp.Rect[] QuadSearchRect;

	private OpenCvSharp.Rect[] QuadSearchMask;

	private OpenCvSharp.Rect[] PadSearchRect;

	private OpenCvSharp.Rect SIMPSearchRect = new OpenCvSharp.Rect(0, 0, 50, 50);

	private OpenCvSharp.Rect AIInfluenceRect = new OpenCvSharp.Rect(0, 0, 50, 50);

	private OpenCvSharp.Rect MaxSearchRect = new OpenCvSharp.Rect(0, 0, 320, 240);

	private bool RulerMode;

	private Point2f Pt1;

	private Point2f Pt2;

	private SAM mSam;

	private int ImageListSource;

	private string[] filelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private string PreviewIndex = "0";

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private INSP_LOCATE_PARAM BackupParam;

	private CVInspection inspection;

	private byte[] buffer;

	private ThroughputParamFile ThroughputFile;

	private byte[] CurrentColor = new byte[3];

	private bool[] ColorDropper = new bool[7];

	private ComboBox[] QuadChannel;

	private RangeSlider[] QuadThdValue;

	private RangeSlider[] QuadThdRValue;

	private RangeSlider[] QuadThdGValue;

	private RangeSlider[] QuadThdBValue;

	private TextBox[] QuadThdMin;

	private TextBox[] QuadThdMax;

	private Label[] Quad_THDR_Min;

	private Label[] Quad_THDR_Max;

	private Label[] Quad_THDG_Min;

	private Label[] Quad_THDG_Max;

	private Label[] Quad_THDB_Min;

	private Label[] Quad_THDB_Max;

	private CheckBox[] Quad_EnableRGBThd;

	private RadioButton[] QuadScanDir1;

	private RadioButton[] QuadScanDir2;

	private ComboBox[] QuadDEStart;

	private ComboBox[] QuadDEEnd;

	private ComboBox[] QuadDEStartCount;

	private ComboBox[] QuadDEEndCount;

	private Border[] borQuadSearchRect;

	private Border[] borQuadSearchMask;

	private CheckBox[] CBEnableMask;

	private ComboBox[,] PadChannel;

	private RangeSlider[,] PadThdValue;

	private TextBox[,] PadThdMin;

	private TextBox[,] PadThdMax;

	private ComboBox[,] PadDEStart;

	private ComboBox[,] PadDEEnd;

	private ComboBox[,] PadDEStartCount;

	private ComboBox[,] PadDEEndCount;

	private Border[] borPadSearchRect;

	private bool UpdateUI;

	private bool[] OTSU_White = new bool[4];

	private bool OTSU_Easy = true;

	private string[] DBfilelist;

	private int MarkingImgCount;

	private int DBIndex;

	private bool EnableModifyLog;

	private string ModifyLog = "";

	private string RecipeName = "";

	private string UserID = "123456";

	private bool PreviewMode;

	private int AutoLocateChID = 3;

	private int CurrentClient;

	private bool LocateResult;

	private DispatcherTimer LoadingTimer = new DispatcherTimer();

	private int loadingstatus;

	internal LocateWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Border br_LocateRect;

	internal TextBlock txt_LocateRect;

	internal Label Label_Result;

	internal Rectangle bor_AILocateRect;

	internal Border bor_SearchRectL;

	internal TextBlock txt_Search1Label1;

	internal Border bor_SearchRectT;

	internal TextBlock txt_Search1Label2;

	internal Border bor_SearchRectR;

	internal TextBlock txt_Search1Label3;

	internal Border bor_SearchRectB;

	internal TextBlock txt_Search1Label4;

	internal Border bor_MaxSearchRect;

	internal TextBlock txt_Search1Label5;

	internal Border bor_SearchMaskT;

	internal TextBlock txt_Search1Label6;

	internal Border bor_SearchMaskB;

	internal TextBlock txt_Search1Label7;

	internal Border bor_SearchMaskL;

	internal TextBlock txt_Search1Label8;

	internal Border bor_SearchMaskR;

	internal TextBlock txt_Search1Label9;

	internal TextBox txt_PreviewMode;

	internal Button btn_PreviewSwitch_Copy;

	internal Label lab_AutoLocateHint;

	internal Button btn_Ruler;

	internal Border bor_SIMPSearchRect;

	internal TextBlock txt_Search1Label;

	internal TabControl tab_LocateType;

	internal TabItem tab_SIMP;

	internal Grid grd_SIMP_THD_1;

	internal RangeSlider rs_SIMP_THD1;

	internal ComboBox cb_SIMP_THD1_Channel;

	internal Grid grd_SIMP_DilateErode1;

	internal ComboBox cb_THD1_DEStart;

	internal ComboBox cb_THD1_DEEnd;

	internal ComboBox cb_THD1_DEEndCount;

	internal ComboBox cb_THD1_DEStartCount;

	internal Button btn_ColorDropperSIMP;

	internal NumericUpDown nu_Thd1LimitMin;

	internal NumericUpDown nu_Thd1LimitMax;

	internal Button btn_OTSU_Easy;

	internal Button btn_TrackerReset;

	internal CheckBox cb_SIMP_SideLocate;

	internal Expander exp_THD2;

	internal Grid grd_SIMP_THD_3;

	internal RangeSlider rs_SIMP_THD2;

	internal Grid grd_SIMP_DilateErode9;

	internal ComboBox cb_THD2_DEStart;

	internal ComboBox cb_THD2_DEEnd;

	internal ComboBox cb_THD2_DEEndCount;

	internal ComboBox cb_THD2_DEStartCount;

	internal NumericUpDown nu_Thd2LimitMin;

	internal NumericUpDown nu_Thd2LimitMax;

	internal ComboBox cb_SIMP_THD2_Channel;

	internal Expander exp_THD3;

	internal Grid grd_SIMP_THD_4;

	internal RangeSlider rs_SIMP_THD3;

	internal Grid grd_SIMP_DilateErode10;

	internal ComboBox cb_THD3_DEStart;

	internal ComboBox cb_THD3_DEEnd;

	internal ComboBox cb_THD3_DEEndCount;

	internal ComboBox cb_THD3_DEStartCount;

	internal NumericUpDown nu_Thd3LimitMin;

	internal NumericUpDown nu_Thd3LimitMax;

	internal ComboBox cb_SIMP_THD3_Channel;

	internal TabItem tab_QUAD;

	internal Grid grd_QUAD_THD_L;

	internal RangeSlider rs_QuadTHDValueL;

	internal TextBox txt_QuadTHDMinL;

	internal Grid grd_SIMP_DilateErode3;

	internal ComboBox cb_Quad_DEStartL;

	internal ComboBox cb_Quad_DEEndL;

	internal ComboBox cb_Quad_DEEndCountL;

	internal ComboBox cb_Quad_DEStartCountL;

	internal TextBox txt_QuadTHDMaxL;

	internal ComboBox cb_QuadChannelL;

	internal RadioButton rad_QuadScanL2R_L;

	internal RadioButton rad_QuadScanR2L_L;

	internal Button btn_ScanL2RW1;

	internal Button btn_ScanR2LW1;

	internal Button btn_QuadTrackerReset;

	internal Button btn_PreviewScanL;

	internal Button btn_PreviewQuadThdL;

	internal Button btn_ColorDropperL;

	internal CheckBox cb_EnableMaskL;

	internal CheckBox cb_EnableAvgEdgeL;

	internal CheckBox cb_Quad_EnableRGBThd_L;

	internal Grid grd_Quad_RGBThd_L;

	internal RangeSlider rs_Quad_THDR_L;

	internal RangeSlider rs_Quad_THDG_L;

	internal RangeSlider rs_Quad_THDB_L;

	internal Label lab_Quad_THDR_Min_L;

	internal Label lab_Quad_THDR_Max_L;

	internal Label lab_Quad_THDG_Min_L;

	internal Label lab_Quad_THDG_Max_L;

	internal Label lab_Quad_THDB_Min_L;

	internal Label lab_Quad_THDB_Max_L;

	internal Button btn_OTSU_L;

	internal Grid grd_QUAD_THD_R;

	internal RangeSlider rs_QuadTHDValueR;

	internal TextBox txt_QuadTHDMinR;

	internal Grid grd_SIMP_DilateErode4;

	internal ComboBox cb_Quad_DEStartR;

	internal ComboBox cb_Quad_DEEndR;

	internal ComboBox cb_Quad_DEEndCountR;

	internal ComboBox cb_Quad_DEStartCountR;

	internal TextBox txt_QuadTHDMaxR;

	internal ComboBox cb_QuadChannelR;

	internal RadioButton rad_QuadScanL2R_R;

	internal RadioButton rad_QuadScanR2L_R;

	internal Button btn_ScanL2RW2;

	internal Button btn_ScanR2LW2;

	internal Button btn_QuadTrackerReset_Copy;

	internal Button btn_PreviewScanL_Copy;

	internal Button btn_PreviewQuadThdL_Copy;

	internal Button btn_ColorDropperR;

	internal CheckBox cb_EnableMaskR;

	internal CheckBox cb_EnableAvgEdgeR;

	internal CheckBox cb_Quad_EnableRGBThd_R;

	internal Grid grd_Quad_RGBThd_R;

	internal Label lab_Quad_THDR_Min_R;

	internal Label lab_Quad_THDR_Max_R;

	internal Label lab_Quad_THDG_Min_R;

	internal Label lab_Quad_THDG_Max_R;

	internal Label lab_Quad_THDB_Min_R;

	internal Label lab_Quad_THDB_Max_R;

	internal RangeSlider rs_Quad_THDR_R;

	internal RangeSlider rs_Quad_THDG_R;

	internal RangeSlider rs_Quad_THDB_R;

	internal Button btn_OTSU_R;

	internal Grid grd_QUAD_THD_T;

	internal RangeSlider rs_QuadTHDValueT;

	internal TextBox txt_QuadTHDMinT;

	internal Grid grd_SIMP_DilateErode5;

	internal ComboBox cb_Quad_DEStartT;

	internal ComboBox cb_Quad_DEEndT;

	internal ComboBox cb_Quad_DEEndCountT;

	internal ComboBox cb_Quad_DEStartCountT;

	internal TextBox txt_QuadTHDMaxT;

	internal ComboBox cb_QuadChannelT;

	internal RadioButton rad_QuadScanT2B_T;

	internal RadioButton rad_QuadScanB2T_T;

	internal Button btn_ScanL2RW3;

	internal Button btn_ScanR2LW3;

	internal Button btn_QuadTrackerReset_Copy1;

	internal Button btn_PreviewScanL_Copy1;

	internal Button btn_PreviewQuadThdL_Copy1;

	internal Button btn_ColorDropperT;

	internal CheckBox cb_EnableMaskT;

	internal CheckBox cb_EnableAvgEdgeT;

	internal CheckBox cb_Quad_EnableRGBThd_T;

	internal Grid grd_Quad_RGBThd_T;

	internal Label lab_Quad_THDR_Min_T;

	internal Label lab_Quad_THDR_Max_T;

	internal Label lab_Quad_THDG_Min_T;

	internal Label lab_Quad_THDG_Max_T;

	internal Label lab_Quad_THDB_Min_T;

	internal Label lab_Quad_THDB_Max_T;

	internal RangeSlider rs_Quad_THDR_T;

	internal RangeSlider rs_Quad_THDG_T;

	internal RangeSlider rs_Quad_THDB_T;

	internal Button btn_OTSU_T;

	internal Grid grd_QUAD_THD_B;

	internal RangeSlider rs_QuadTHDValueB;

	internal TextBox txt_QuadTHDMinB;

	internal Grid grd_SIMP_DilateErode6;

	internal ComboBox cb_Quad_DEStartB;

	internal ComboBox cb_Quad_DEEndB;

	internal ComboBox cb_Quad_DEEndCountB;

	internal ComboBox cb_Quad_DEStartCountB;

	internal TextBox txt_QuadTHDMaxB;

	internal ComboBox cb_QuadChannelB;

	internal RadioButton rad_QuadScanT2B_B;

	internal RadioButton rad_QuadScanB2T_B;

	internal Button btn_ScanL2RW4;

	internal Button btn_ScanR2LW4;

	internal Button btn_QuadTrackerReset_Copy2;

	internal Button btn_PreviewScanL_Copy2;

	internal Button btn_PreviewQuadThdL_Copy2;

	internal Button btn_ColorDropperB;

	internal CheckBox cb_EnableMaskB;

	internal CheckBox cb_EnableAvgEdgeB;

	internal CheckBox cb_Quad_EnableRGBThd_B;

	internal Grid grd_Quad_RGBThd_B;

	internal Label lab_Quad_THDR_Min_B;

	internal Label lab_Quad_THDR_Max_B;

	internal Label lab_Quad_THDG_Min_B;

	internal Label lab_Quad_THDG_Max_B;

	internal Label lab_Quad_THDB_Min_B;

	internal Label lab_Quad_THDB_Max_B;

	internal RangeSlider rs_Quad_THDR_B;

	internal RangeSlider rs_Quad_THDG_B;

	internal RangeSlider rs_Quad_THDB_B;

	internal Button btn_OTSU_B;

	internal CheckBox chk_UseMelfLocate;

	internal CheckBox chk_UseFilterPoint;

	internal CheckBox chk_UseStdFilter;

	internal CheckBox chk_UseSideLocate;

	internal ComboBox cb_Quad_LocateAngleSelect;

	internal Label lab_LocateExtend_Copy;

	internal CheckBox chk_FixSideLocateLR;

	internal CheckBox cb_QUAD_SideLocate;

	internal Label lab_LocateExtend_Copy1;

	internal TabItem tab_PAD;

	internal Grid grd_Pad_THD_L;

	internal RangeSlider rs_PadTHDValueL1;

	internal TextBox txt_PadTHDMinL1;

	internal Grid grd_SIMP_DilateErode7;

	internal ComboBox cb_Pad_DEStartL1;

	internal ComboBox cb_Pad_DEEndL1;

	internal ComboBox cb_Pad_DEEndCountL1;

	internal ComboBox cb_Pad_DEStartCountL1;

	internal TextBox txt_PadTHDMaxL1;

	internal ComboBox cb_PadChannelL1;

	internal Button btn_QuadTrackerReset1;

	internal Button btn_PreviewPadThdL;

	internal Button btn_ColorDropperL1;

	internal RangeSlider rs_PadTHDValueL2;

	internal TextBox txt_PadTHDMinL2;

	internal Grid grd_SIMP_DilateErode7_Copy;

	internal ComboBox cb_Pad_DEStartL2;

	internal ComboBox cb_Pad_DEEndL2;

	internal ComboBox cb_Pad_DEEndCountL2;

	internal ComboBox cb_Pad_DEStartCountL2;

	internal TextBox txt_PadTHDMaxL2;

	internal ComboBox cb_PadChannelL2;

	internal Grid grd_Pad_THD_R;

	internal RangeSlider rs_PadTHDValueR1;

	internal TextBox txt_PadTHDMinR1;

	internal Grid grd_SIMP_DilateErode8;

	internal ComboBox cb_Pad_DEStartR1;

	internal ComboBox cb_Pad_DEEndR1;

	internal ComboBox cb_Pad_DEEndCountR1;

	internal ComboBox cb_Pad_DEStartCountR1;

	internal TextBox txt_PadTHDMaxR1;

	internal ComboBox cb_PadChannelR1;

	internal Button btn_QuadTrackerReset2;

	internal Button btn_PreviewPadThdR;

	internal Button btn_ColorDropperL2;

	internal RangeSlider rs_PadTHDValueR2;

	internal TextBox txt_PadTHDMinR2;

	internal Grid grd_SIMP_DilateErode7_Copy1;

	internal ComboBox cb_Pad_DEStartR2;

	internal ComboBox cb_Pad_DEEndR2;

	internal ComboBox cb_Pad_DEEndCountR2;

	internal ComboBox cb_Pad_DEStartCountR2;

	internal TextBox txt_PadTHDMaxR2;

	internal ComboBox cb_PadChannelR2;

	internal TabItem tab_MELF;

	internal Button btn_ImgSrc;

	internal Button btn_ImgR;

	internal Button btn_ImgG;

	internal Button btn_ImgB;

	internal Button btn_ImgH;

	internal Button btn_ImgS;

	internal Button btn_ImgV;

	internal Button btn_ImgGray;

	internal Button btn_ImgS_Copy;

	internal Button btn_ImgV_Copy;

	internal Rectangle rt_AutoLocateHint;

	internal Button btn_Img_AILR;

	internal Button btn_Img_AILG;

	internal Button btn_Img_AILB;

	internal Button btn_ImageNum;

	internal Button btn_Img_AILRGB;

	internal Label lab_RValue;

	internal Label lab_GValue;

	internal Label lab_BValue;

	internal Label lab_XValue;

	internal Label lab_YValue;

	internal Label lab_HValue;

	internal Label lab_SValue;

	internal Label lab_VValue;

	internal Label lab_GrayValue;

	internal Button btn_AutoLocate;

	internal TextBox txt_AllResult;

	internal Grid grd_SIMP_Setup;

	internal TextBox txt_RotatLimitAngle;

	internal TextBox txt_RotatWidthMin;

	internal TextBox txt_RotatWidthMax;

	internal TextBox txt_RotatHeightMin;

	internal TextBox txt_RotatHeightMax;

	internal ComboBox cb_RotatErrorBin;

	internal Label lab_doRotateMinAngle;

	internal TextBox txt_doRotateMinAngle;

	internal Button btn_Test;

	internal TextBox txt_RotatAngleResult;

	internal TextBox txt_RotatWidthResult;

	internal TextBox txt_RotatHeightResult;

	internal Rectangle rt_Result;

	internal CheckBox cb_LocateSpeedUp;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal Button btn_ChartW;

	internal Button btn_ChartH;

	internal Button btn_ChartA;

	internal CheckBox chk_CreateHSVChannel;

	internal ComboBox cb_SizeErrorBin;

	internal CheckBox chk_EnableFocusCheck;

	internal TextBox txt_FocusLimit;

	internal TextBox txt_FocusLimitResult;

	internal Button btn_Reverse;

	internal Button btn_AILocate;

	internal CheckBox cb_EnableAILocate;

	internal Label lab_Ruler;

	internal Grid grd_AILocate;

	internal Button btn_SetModel;

	internal TextBox txt_ModeName;

	internal CheckBox chk_EnableTensorRT;

	internal Button btn_Draw;

	internal TextBox txt_TrainCount;

	internal Button btn_Train;

	internal Canvas cnv_SourceImg;

	internal Canvas cnv_AIImg;

	internal Button btn_TrainImageNum;

	internal Button btn_NextDBImage;

	internal Button btn_PrevDBImage;

	internal Button btn_DeleteDBImage;

	internal TextBox txt_ModeName_Copy;

	internal Canvas cnv_AIMarkImg;

	internal Button btn_ChartY;

	internal Button btn_ChartExport;

	internal Button btn_ChartX;

	internal Button btn_TestNextImg;

	internal Button btn_TestPrevImg;

	internal Grid grd_Loading;

	internal Label lab_Loading;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public LocateWindow()
	{
		inited = false;
		InitializeComponent();
		rs_SIMP_THD1.LowerValue = 0.0;
		rs_SIMP_THD1.UpperValue = 255.0;
		inited = true;
		THD_Preview = false;
		br_LocateRect.Visibility = Visibility.Hidden;
		QuadSearchRect = new OpenCvSharp.Rect[4];
		QuadSearchRect[0] = new OpenCvSharp.Rect(0, 0, 100, 100);
		QuadSearchRect[1] = new OpenCvSharp.Rect(0, 0, 100, 100);
		QuadSearchRect[2] = new OpenCvSharp.Rect(0, 0, 100, 100);
		QuadSearchRect[3] = new OpenCvSharp.Rect(0, 0, 100, 100);
		QuadSearchMask = new OpenCvSharp.Rect[4];
		QuadSearchMask[0] = new OpenCvSharp.Rect(0, 0, 50, 50);
		QuadSearchMask[1] = new OpenCvSharp.Rect(0, 0, 50, 50);
		QuadSearchMask[2] = new OpenCvSharp.Rect(0, 0, 50, 50);
		QuadSearchMask[3] = new OpenCvSharp.Rect(0, 0, 50, 50);
		PadSearchRect = new OpenCvSharp.Rect[4];
		PadSearchRect[0] = new OpenCvSharp.Rect(0, 0, 50, 50);
		PadSearchRect[1] = new OpenCvSharp.Rect(0, 0, 50, 50);
		bool[] colorDropper = ColorDropper;
		bool[] colorDropper2 = ColorDropper;
		bool[] colorDropper3 = ColorDropper;
		bool[] colorDropper4 = ColorDropper;
		bool[] colorDropper5 = ColorDropper;
		bool flag;
		ColorDropper[5] = (flag = (ColorDropper[6] = false));
		colorDropper5[4] = (flag = flag);
		colorDropper4[3] = (flag = flag);
		colorDropper3[2] = (flag = flag);
		colorDropper2[1] = (flag = flag);
		colorDropper[0] = flag;
		QuadChannel = new ComboBox[4] { cb_QuadChannelL, cb_QuadChannelR, cb_QuadChannelT, cb_QuadChannelB };
		Quad_EnableRGBThd = new CheckBox[4] { cb_Quad_EnableRGBThd_L, cb_Quad_EnableRGBThd_R, cb_Quad_EnableRGBThd_T, cb_Quad_EnableRGBThd_B };
		QuadThdValue = new RangeSlider[4] { rs_QuadTHDValueL, rs_QuadTHDValueR, rs_QuadTHDValueT, rs_QuadTHDValueB };
		QuadThdRValue = new RangeSlider[4] { rs_Quad_THDR_L, rs_Quad_THDR_R, rs_Quad_THDR_T, rs_Quad_THDR_B };
		QuadThdGValue = new RangeSlider[4] { rs_Quad_THDG_L, rs_Quad_THDG_R, rs_Quad_THDG_T, rs_Quad_THDG_B };
		QuadThdBValue = new RangeSlider[4] { rs_Quad_THDB_L, rs_Quad_THDB_R, rs_Quad_THDB_T, rs_Quad_THDB_B };
		QuadThdMin = new TextBox[4] { txt_QuadTHDMinL, txt_QuadTHDMinR, txt_QuadTHDMinT, txt_QuadTHDMinB };
		QuadThdMax = new TextBox[4] { txt_QuadTHDMaxL, txt_QuadTHDMaxR, txt_QuadTHDMaxT, txt_QuadTHDMaxB };
		Quad_THDR_Min = new Label[4] { lab_Quad_THDR_Min_L, lab_Quad_THDR_Min_R, lab_Quad_THDR_Min_T, lab_Quad_THDR_Min_B };
		Quad_THDR_Max = new Label[4] { lab_Quad_THDR_Max_L, lab_Quad_THDR_Max_R, lab_Quad_THDR_Max_T, lab_Quad_THDR_Max_B };
		Quad_THDG_Min = new Label[4] { lab_Quad_THDG_Min_L, lab_Quad_THDG_Min_R, lab_Quad_THDG_Min_T, lab_Quad_THDG_Min_B };
		Quad_THDG_Max = new Label[4] { lab_Quad_THDG_Max_L, lab_Quad_THDG_Max_R, lab_Quad_THDG_Max_T, lab_Quad_THDG_Max_B };
		Quad_THDB_Min = new Label[4] { lab_Quad_THDB_Min_L, lab_Quad_THDB_Min_R, lab_Quad_THDB_Min_T, lab_Quad_THDB_Min_B };
		Quad_THDB_Max = new Label[4] { lab_Quad_THDB_Max_L, lab_Quad_THDB_Max_R, lab_Quad_THDB_Max_T, lab_Quad_THDB_Max_B };
		QuadScanDir1 = new RadioButton[4] { rad_QuadScanL2R_L, rad_QuadScanL2R_R, rad_QuadScanT2B_T, rad_QuadScanT2B_B };
		QuadScanDir2 = new RadioButton[4] { rad_QuadScanR2L_L, rad_QuadScanR2L_R, rad_QuadScanB2T_T, rad_QuadScanB2T_B };
		QuadDEStart = new ComboBox[4] { cb_Quad_DEStartL, cb_Quad_DEStartR, cb_Quad_DEStartT, cb_Quad_DEStartB };
		QuadDEEnd = new ComboBox[4] { cb_Quad_DEEndL, cb_Quad_DEEndR, cb_Quad_DEEndT, cb_Quad_DEEndB };
		QuadDEStartCount = new ComboBox[4] { cb_Quad_DEStartCountL, cb_Quad_DEStartCountR, cb_Quad_DEStartCountT, cb_Quad_DEStartCountB };
		QuadDEEndCount = new ComboBox[4] { cb_Quad_DEEndCountL, cb_Quad_DEEndCountR, cb_Quad_DEEndCountT, cb_Quad_DEEndCountB };
		borQuadSearchRect = new Border[4] { bor_SearchRectL, bor_SearchRectR, bor_SearchRectT, bor_SearchRectB };
		borQuadSearchMask = new Border[4] { bor_SearchMaskL, bor_SearchMaskR, bor_SearchMaskT, bor_SearchMaskB };
		CBEnableMask = new CheckBox[4] { cb_EnableMaskL, cb_EnableMaskR, cb_EnableMaskT, cb_EnableMaskB };
		PadChannel = new ComboBox[2, 2]
		{
			{ cb_PadChannelL1, cb_PadChannelL2 },
			{ cb_PadChannelR1, cb_PadChannelR2 }
		};
		PadThdValue = new RangeSlider[2, 2]
		{
			{ rs_PadTHDValueL1, rs_PadTHDValueL2 },
			{ rs_PadTHDValueR1, rs_PadTHDValueR2 }
		};
		PadThdMin = new TextBox[2, 2]
		{
			{ txt_PadTHDMinL1, txt_PadTHDMinL2 },
			{ txt_PadTHDMinR1, txt_PadTHDMinR2 }
		};
		PadThdMax = new TextBox[2, 2]
		{
			{ txt_PadTHDMaxL1, txt_PadTHDMaxL2 },
			{ txt_PadTHDMaxR1, txt_PadTHDMaxR2 }
		};
		PadDEStart = new ComboBox[2, 2]
		{
			{ cb_Pad_DEStartL1, cb_Pad_DEStartL2 },
			{ cb_Pad_DEStartR1, cb_Pad_DEStartR2 }
		};
		PadDEEnd = new ComboBox[2, 2]
		{
			{ cb_Pad_DEEndL1, cb_Pad_DEEndL2 },
			{ cb_Pad_DEEndR1, cb_Pad_DEEndR2 }
		};
		PadDEStartCount = new ComboBox[2, 2]
		{
			{ cb_Pad_DEStartCountL1, cb_Pad_DEStartCountL2 },
			{ cb_Pad_DEStartCountR1, cb_Pad_DEStartCountR2 }
		};
		PadDEEndCount = new ComboBox[2, 2]
		{
			{ cb_Pad_DEEndCountL1, cb_Pad_DEEndCountL2 },
			{ cb_Pad_DEEndCountR1, cb_Pad_DEEndCountR2 }
		};
		borPadSearchRect = new Border[2] { bor_SearchRectL, bor_SearchRectR };
		bor_SearchMaskL.Visibility = Visibility.Hidden;
		bor_SearchMaskR.Visibility = Visibility.Hidden;
		bor_SearchMaskT.Visibility = Visibility.Hidden;
		bor_SearchMaskB.Visibility = Visibility.Hidden;
		txt_PreviewMode.Visibility = Visibility.Hidden;
		for (int i = 0; i < 4; i++)
		{
			OTSU_White[i] = true;
		}
		lab_AutoLocateHint.Visibility = Visibility.Hidden;
		rt_AutoLocateHint.Visibility = Visibility.Hidden;
		lab_Ruler.Visibility = Visibility.Hidden;
		Pt1 = new OpenCvSharp.Point(0, 0);
		Pt2 = new OpenCvSharp.Point(0, 0);
		grd_AILocate.Visibility = Visibility.Hidden;
		bor_AILocateRect.Visibility = Visibility.Hidden;
		bor_AILocateRect.IsEnabled = false;
		grd_Loading.Visibility = Visibility.Hidden;
		LoadingTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
		LoadingTimer.Tick += LoadingTimer_Tick;
		LoadingTimer.Stop();
	}

	private void LoadingTimer_Tick(object sender, EventArgs e)
	{
		LoadingTimer.Stop();
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
		{
			grd_Loading.Visibility = Visibility.Visible;
			inspection.locateTool.ReLoadAIModel(ReCreatePB: true);
			if (loadingstatus == 0)
			{
				Btn_Test_Click(this, null);
				string text = "";
				text = ((LanguageType == 1) ? "訓練完成!" : ((LanguageType == 2) ? "训练完成!" : ((LanguageType != 3) ? "Training completed!" : "AIの訓練が成功しました。")));
				MessageBox.Show(text);
			}
			grd_Loading.Visibility = Visibility.Hidden;
		}));
	}

	public void SetInspection(CVInspection station)
	{
		inspection = station;
		if (inspection.paramFile.AllowEnableAILocate)
		{
			btn_Img_AILR.Visibility = Visibility.Visible;
			btn_Img_AILG.Visibility = Visibility.Visible;
			btn_Img_AILB.Visibility = Visibility.Visible;
			btn_Img_AILRGB.Visibility = Visibility.Visible;
			btn_AILocate.Visibility = Visibility.Visible;
		}
		else
		{
			btn_Img_AILR.Visibility = Visibility.Hidden;
			btn_Img_AILG.Visibility = Visibility.Hidden;
			btn_Img_AILB.Visibility = Visibility.Hidden;
			btn_Img_AILRGB.Visibility = Visibility.Hidden;
			btn_AILocate.Visibility = Visibility.Hidden;
		}
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		bool updateUI = UpdateUI;
		UpdateUI = false;
		ThroughputFile = file;
		cb_ParetoChart.Items.Clear();
		for (int i = 0; i < 30; i++)
		{
			if (ThroughputFile.param.AllParetoItems[i] != "")
			{
				cb_ParetoChart.Items.Add(ThroughputFile.param.AllParetoItems[i]);
			}
		}
		UpdateUI = updateUI;
	}

	public void SetBuffer(Mat mat)
	{
		cnv_Image.SetValue(FrameworkElement.WidthProperty, 700.0);
		cnv_Image.SetValue(FrameworkElement.HeightProperty, 600.0);
		ImageWidth = mat.Cols;
		ImageHeight = mat.Rows;
		ImageBand = mat.Channels();
		xRatio = cnv_Image.Width / (double)ImageWidth;
		yRatio = cnv_Image.Height / (double)ImageHeight;
		if (xRatio > yRatio)
		{
			xRatio = yRatio;
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)ImageWidth * xRatio);
		}
		else
		{
			yRatio = xRatio;
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)ImageHeight * yRatio);
		}
		inspection.buffers.SetBuffer(mat);
		if (inspection.paramFile.LocateParam.EnableAILocate)
		{
			inspection.locateTool.UseAIPreLocate(inspection.buffers.Src, offline: true);
		}
		PreviewMode = false;
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
		BackupParam = inspection.paramFile.LocateParam;
		CopyValueToUI();
	}

	public void SetBuffer(byte[] buf, int width, int height, int band)
	{
		buffer = buf;
		ImageWidth = width;
		ImageHeight = height;
		ImageBand = band;
		if (cnv_Image.Width < (double)ImageWidth && ImageWidth > 0)
		{
			xRatio = cnv_Image.Width / (double)ImageWidth;
		}
		else
		{
			xRatio = 1.0;
		}
		if (cnv_Image.Height < (double)ImageHeight && ImageHeight > 0)
		{
			yRatio = cnv_Image.Height / (double)ImageHeight;
		}
		else
		{
			yRatio = 1.0;
		}
		ShowBitmap.Release();
		switch (band)
		{
		case 3:
			ShowBitmap = inspection.buffers.Src.Clone();
			break;
		case 1:
			Cv2.CvtColor(inspection.buffers.Src, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			break;
		default:
			return;
		}
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImageSource(int imagelistsource, string[] flist, int findex, int recentindex, int okindex, int ngindex)
	{
		try
		{
			ImageListSource = imagelistsource;
			filelist = flist;
			fileIndex = findex;
			RecentBufferIndex = recentindex;
			OKBufferIndex = okindex;
			NGBufferIndex = ngindex;
			switch (ImageListSource)
			{
			case 0:
				btn_ImageNum.Content = string.Format("1 / 1");
				break;
			case 1:
				if (filelist != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
				}
				break;
			case 2:
				if (inspection.RecentBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				}
				break;
			case 3:
				if (inspection.OKBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				}
				break;
			case 4:
				if (inspection.NGBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				}
				break;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("LT01-LocateWindow-SetImageSource()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public void SetModifyLogEnable(bool IsEnable, string recipename, string userID)
	{
		EnableModifyLog = IsEnable;
		RecipeName = recipename;
		UserID = userID;
	}

	public void SetSAM(SAM sam)
	{
		mSam = sam;
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI();
	}

	public void SetClientCode(int code)
	{
		CurrentClient = code;
		if (CurrentClient == 3)
		{
			btn_AutoLocate.Visibility = Visibility.Hidden;
		}
		else
		{
			btn_AutoLocate.Visibility = Visibility.Visible;
		}
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		if (CheckTextBox())
		{
			WriteUIValueToParam();
		}
		if (EnableModifyLog)
		{
			WriteModifyLog();
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LoadLanguage(int language)
	{
		switch (language)
		{
		case 1:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-TW"));
			break;
		case 2:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-CN"));
			break;
		case 3:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("ja-JP"));
			break;
		default:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("en-US"));
			break;
		}
		_ = AppDomain.CurrentDomain.BaseDirectory;
		LanguageType = language;
	}

	private void Btn_ImgSrc_Click(object sender, RoutedEventArgs e)
	{
		if (!PreviewMode)
		{
			Button button = (Button)sender;
			PreviewIndex = button.Uid;
			Mat image;
			switch (button.Uid)
			{
			case "0":
				image = inspection.buffers.Src;
				AutoLocateChID = 3;
				break;
			case "1":
				image = inspection.buffers.GetSrcBGR(0);
				AutoLocateChID = 0;
				break;
			case "2":
				image = inspection.buffers.GetSrcBGR(1);
				AutoLocateChID = 1;
				break;
			case "3":
				image = inspection.buffers.GetSrcBGR(2);
				AutoLocateChID = 2;
				break;
			case "4":
				image = inspection.buffers.GetChannelMat(18);
				break;
			case "5":
				image = inspection.buffers.GetChannelMat(19);
				break;
			case "6":
				image = inspection.buffers.GetChannelMat(20);
				break;
			case "7":
				image = DrawAIOutside(inspection.buffers.Src, inspection.paramFile.LocateParam.MaxSearchRect);
				break;
			default:
				image = (image = inspection.buffers.Src);
				AutoLocateChID = 3;
				break;
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(image);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
	}

	private Mat DrawAIOutside(Mat src, OpenCvSharp.Rect rect)
	{
		Mat mat = new Mat(src, rect).Clone();
		if (!inspection.locateTool.PredictMat(mat))
		{
			return src;
		}
		Mat destMatColor = inspection.locateTool.GetDestMatColor();
		Cv2.CvtColor(destMatColor, mat, ColorConversionCodes.RGB2GRAY);
		Cv2.InRange(mat, 20.0, 255.0, mat);
		Cv2.FindContours(mat, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
		Mat mat2 = new Mat(src, rect).Clone();
		mat2.SetTo(0.0);
		for (int i = 0; i < contours.Length; i++)
		{
			Cv2.DrawContours(mat2, contours, i, Scalar.White, 2);
		}
		if (destMatColor.Width > 0 && destMatColor.Height > 0)
		{
			Cv2.CvtColor(mat2, mat2, ColorConversionCodes.BGR2GRAY);
			Mat mat3 = src.Clone();
			Mat m = new Mat(mat3, rect);
			destMatColor.CopyTo(m, mat2);
			return mat3;
		}
		return src;
	}

	private void Cnv_Image_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		try
		{
			System.Windows.Point position = e.GetPosition(this);
			if (xRatio != 0.0 && yRatio != 0.0)
			{
				int num = (int)(position.X / xRatio);
				int num2 = (int)(position.Y / yRatio);
				if (num < ImageWidth && num >= 0 && num2 < ImageHeight && num2 >= 0)
				{
					lab_RValue.Content = (CurrentColor[0] = inspection.buffers.GetSrcBGR(0).At<byte>(num2, num));
					lab_GValue.Content = (CurrentColor[1] = inspection.buffers.GetSrcBGR(1).At<byte>(num2, num));
					lab_BValue.Content = (CurrentColor[2] = inspection.buffers.GetSrcBGR(2).At<byte>(num2, num));
					lab_XValue.Content = num.ToString();
					lab_YValue.Content = num2.ToString();
				}
			}
		}
		catch
		{
		}
	}

	private void SetAILocateRect(OpenCvSharp.Rect rect)
	{
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			rect = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		rect &= rect2;
		bor_AILocateRect.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
		bor_AILocateRect.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * xRatio);
		Canvas.SetLeft(bor_AILocateRect, (double)rect.X * xRatio);
		Canvas.SetTop(bor_AILocateRect, (double)rect.Y * yRatio);
	}

	private void SetSIMPSearchRect(OpenCvSharp.Rect rect)
	{
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			rect = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		rect &= rect2;
		bor_SIMPSearchRect.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
		bor_SIMPSearchRect.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * xRatio);
		Canvas.SetLeft(bor_SIMPSearchRect, (double)rect.X * xRatio);
		Canvas.SetTop(bor_SIMPSearchRect, (double)rect.Y * yRatio);
		bor_SIMPSearchRect.Visibility = Visibility.Visible;
		bor_SearchRectL.Visibility = Visibility.Hidden;
		bor_SearchRectR.Visibility = Visibility.Hidden;
		bor_SearchRectT.Visibility = Visibility.Hidden;
		bor_SearchRectB.Visibility = Visibility.Hidden;
	}

	private void SetMaxSearchRect()
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		if (MaxSearchRect.Width <= 0 || MaxSearchRect.Height <= 0)
		{
			MaxSearchRect = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		MaxSearchRect &= rect;
		bor_MaxSearchRect.SetValue(FrameworkElement.WidthProperty, (double)MaxSearchRect.Width * xRatio);
		bor_MaxSearchRect.SetValue(FrameworkElement.HeightProperty, (double)MaxSearchRect.Height * yRatio);
		Canvas.SetLeft(bor_MaxSearchRect, (double)MaxSearchRect.X * xRatio);
		Canvas.SetTop(bor_MaxSearchRect, (double)MaxSearchRect.Y * yRatio);
		bor_MaxSearchRect.Visibility = Visibility.Visible;
		bor_SearchRectL.Visibility = Visibility.Hidden;
		bor_SearchRectR.Visibility = Visibility.Hidden;
		bor_SearchRectT.Visibility = Visibility.Hidden;
		bor_SearchRectB.Visibility = Visibility.Hidden;
	}

	private void SetQuadSearchRect()
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		QuadSearchRect[0] &= rect;
		QuadSearchRect[1] &= rect;
		QuadSearchRect[2] &= rect;
		QuadSearchRect[3] &= rect;
		for (int i = 0; i < 4; i++)
		{
			if (QuadSearchRect[i].Width <= 0 || QuadSearchRect[i].Height <= 0)
			{
				QuadSearchRect[i] = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			borQuadSearchRect[i].SetValue(FrameworkElement.WidthProperty, (double)QuadSearchRect[i].Width * xRatio);
			borQuadSearchRect[i].SetValue(FrameworkElement.HeightProperty, (double)QuadSearchRect[i].Height * xRatio);
			Canvas.SetLeft(borQuadSearchRect[i], (double)QuadSearchRect[i].X * xRatio);
			Canvas.SetTop(borQuadSearchRect[i], (double)QuadSearchRect[i].Y * yRatio);
		}
		bor_SIMPSearchRect.Visibility = Visibility.Hidden;
		bor_SearchRectL.Visibility = Visibility.Visible;
		bor_SearchRectR.Visibility = Visibility.Visible;
		bor_SearchRectT.Visibility = Visibility.Visible;
		bor_SearchRectB.Visibility = Visibility.Visible;
	}

	private void SetQuadSearchMask()
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		QuadSearchMask[0] &= rect;
		QuadSearchMask[1] &= rect;
		QuadSearchMask[2] &= rect;
		QuadSearchMask[3] &= rect;
		for (int i = 0; i < 4; i++)
		{
			if (QuadSearchMask[i].Width <= 0 || QuadSearchMask[i].Height <= 0)
			{
				QuadSearchMask[i] = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			borQuadSearchMask[i].SetValue(FrameworkElement.WidthProperty, (double)QuadSearchMask[i].Width * xRatio);
			borQuadSearchMask[i].SetValue(FrameworkElement.HeightProperty, (double)QuadSearchMask[i].Height * xRatio);
			Canvas.SetLeft(borQuadSearchMask[i], (double)QuadSearchMask[i].X * xRatio);
			Canvas.SetTop(borQuadSearchMask[i], (double)QuadSearchMask[i].Y * yRatio);
		}
		bor_SIMPSearchRect.Visibility = Visibility.Hidden;
		bor_SearchMaskL.Visibility = Visibility.Visible;
		bor_SearchMaskR.Visibility = Visibility.Visible;
		bor_SearchMaskT.Visibility = Visibility.Visible;
		bor_SearchMaskB.Visibility = Visibility.Visible;
	}

	private void SetPadSearchRect()
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		PadSearchRect[0] &= rect;
		PadSearchRect[1] &= rect;
		for (int i = 0; i < 2; i++)
		{
			if (PadSearchRect[i].Width <= 0 || PadSearchRect[i].Height <= 0)
			{
				PadSearchRect[i] = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			borQuadSearchRect[i].SetValue(FrameworkElement.WidthProperty, (double)PadSearchRect[i].Width * xRatio);
			borQuadSearchRect[i].SetValue(FrameworkElement.HeightProperty, (double)PadSearchRect[i].Height * xRatio);
			Canvas.SetLeft(borQuadSearchRect[i], (double)PadSearchRect[i].X * xRatio);
			Canvas.SetTop(borQuadSearchRect[i], (double)PadSearchRect[i].Y * yRatio);
		}
		bor_SIMPSearchRect.Visibility = Visibility.Hidden;
		bor_SearchRectL.Visibility = Visibility.Visible;
		bor_SearchRectR.Visibility = Visibility.Visible;
		bor_SearchRectT.Visibility = Visibility.Hidden;
		bor_SearchRectB.Visibility = Visibility.Hidden;
	}

	private void SetLocateRect(OpenCvSharp.Rect rect, string axis)
	{
		br_LocateRect.Visibility = Visibility.Visible;
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		if (rect.Width == 0 || rect.Height == 0)
		{
			rect = new OpenCvSharp.Rect(0, 0, 100, 100);
		}
		rect &= rect2;
		br_LocateRect.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
		br_LocateRect.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * xRatio);
		Canvas.SetLeft(br_LocateRect, (double)rect.X * xRatio);
		Canvas.SetTop(br_LocateRect, (double)rect.Y * yRatio);
		txt_LocateRect.Text = axis;
	}

	private OpenCvSharp.Rect GetAILocateRect()
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return rect;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect.X = (int)(Canvas.GetLeft(bor_AILocateRect) / xRatio);
		rect.Y = (int)(Canvas.GetTop(bor_AILocateRect) / yRatio);
		rect.Width = (int)(bor_AILocateRect.Width / xRatio);
		rect.Height = (int)(bor_AILocateRect.Height / yRatio);
		return rect & rect2;
	}

	private OpenCvSharp.Rect GetSIMPSearchRect()
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return rect;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect.X = (int)(Canvas.GetLeft(bor_SIMPSearchRect) / xRatio);
		rect.Y = (int)(Canvas.GetTop(bor_SIMPSearchRect) / yRatio);
		rect.Width = (int)(bor_SIMPSearchRect.Width / xRatio);
		rect.Height = (int)(bor_SIMPSearchRect.Height / yRatio);
		return rect & rect2;
	}

	private void GetMaxSearchRect()
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			MaxSearchRect.X = (int)(Canvas.GetLeft(bor_MaxSearchRect) / xRatio);
			MaxSearchRect.Y = (int)(Canvas.GetTop(bor_MaxSearchRect) / yRatio);
			MaxSearchRect.Width = (int)(bor_MaxSearchRect.Width / xRatio);
			MaxSearchRect.Height = (int)(bor_MaxSearchRect.Height / yRatio);
			MaxSearchRect &= rect;
		}
	}

	private void GetQuadSearchRect()
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			for (int i = 0; i < 4; i++)
			{
				QuadSearchRect[i].X = (int)(Canvas.GetLeft(borQuadSearchRect[i]) / xRatio);
				QuadSearchRect[i].Y = (int)(Canvas.GetTop(borQuadSearchRect[i]) / yRatio);
				QuadSearchRect[i].Width = (int)(borQuadSearchRect[i].Width / xRatio);
				QuadSearchRect[i].Height = (int)(borQuadSearchRect[i].Height / yRatio);
				QuadSearchRect[i] &= rect;
			}
		}
	}

	private void GetQuadSearchMask()
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			for (int i = 0; i < 4; i++)
			{
				QuadSearchMask[i].X = (int)(Canvas.GetLeft(borQuadSearchMask[i]) / xRatio);
				QuadSearchMask[i].Y = (int)(Canvas.GetTop(borQuadSearchMask[i]) / yRatio);
				QuadSearchMask[i].Width = (int)(borQuadSearchMask[i].Width / xRatio);
				QuadSearchMask[i].Height = (int)(borQuadSearchMask[i].Height / yRatio);
				QuadSearchMask[i] &= rect;
			}
		}
	}

	private void GetPadSearchRect()
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			for (int i = 0; i < 2; i++)
			{
				PadSearchRect[i].X = (int)(Canvas.GetLeft(borQuadSearchRect[i]) / xRatio);
				PadSearchRect[i].Y = (int)(Canvas.GetTop(borQuadSearchRect[i]) / yRatio);
				PadSearchRect[i].Width = (int)(borQuadSearchRect[i].Width / xRatio);
				PadSearchRect[i].Height = (int)(borQuadSearchRect[i].Height / yRatio);
				PadSearchRect[i] &= rect;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetResult(bool result)
	{
		Label_Result.Visibility = Visibility.Visible;
		LocateResult = result;
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = Brushes.Lime;
			rt_Result.Fill = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
		}
	}

	private void Rs_SIMP_THD1_ThumbDragCompleted(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			nu_Thd1LimitMin.Value = Convert.ToInt32(rs_SIMP_THD1.LowerValue);
			nu_Thd1LimitMax.Value = Convert.ToInt32(rs_SIMP_THD1.UpperValue);
			nu_Thd2LimitMin.Value = Convert.ToInt32(rs_SIMP_THD2.LowerValue);
			nu_Thd2LimitMax.Value = Convert.ToInt32(rs_SIMP_THD2.UpperValue);
			nu_Thd3LimitMin.Value = Convert.ToInt32(rs_SIMP_THD3.LowerValue);
			nu_Thd3LimitMax.Value = Convert.ToInt32(rs_SIMP_THD3.UpperValue);
			doThdImage();
		}
	}

	private void Btn_PreviewSwitch_Click(object sender, RoutedEventArgs e)
	{
		if (THD_Preview)
		{
			Mat src = inspection.buffers.Src;
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(src);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
			return;
		}
		if (tab_LocateType.SelectedIndex == 0)
		{
			doThdImage();
		}
		else if (tab_LocateType.SelectedIndex == 1)
		{
			doQUADThdImage(QuadSelectIndex);
		}
		else if (tab_LocateType.SelectedIndex == 2)
		{
			doPADThdImage(PadSelectIndex);
		}
		THD_Preview = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void doThdImage()
	{
		if (!UpdateUI || PreviewMode)
		{
			return;
		}
		try
		{
			Label_Result.Visibility = Visibility.Hidden;
			br_LocateRect.Visibility = Visibility.Hidden;
			if (inspection.buffers.Src.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.Src.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.Src, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			int num = (int)rs_SIMP_THD1.LowerValue;
			int num2 = (int)rs_SIMP_THD1.UpperValue;
			int selectedIndex = cb_SIMP_THD1_Channel.SelectedIndex;
			Mat srcBGR = inspection.buffers.GetSrcBGR(selectedIndex);
			Mat mat = srcBGR.Clone();
			Cv2.InRange(srcBGR, num, num2, mat);
			doMorph(ref mat, cb_THD1_DEStart.SelectedIndex, cb_THD1_DEStartCount.SelectedIndex);
			doMorph(ref mat, cb_THD1_DEEnd.SelectedIndex, cb_THD1_DEEndCount.SelectedIndex);
			if (exp_THD2.IsExpanded)
			{
				int num3 = (int)rs_SIMP_THD2.LowerValue;
				int num4 = (int)rs_SIMP_THD2.UpperValue;
				int selectedIndex2 = cb_SIMP_THD2_Channel.SelectedIndex;
				Mat srcBGR2 = inspection.buffers.GetSrcBGR(selectedIndex2);
				Mat mat2 = srcBGR2.Clone();
				Cv2.InRange(srcBGR2, num3, num4, mat2);
				doMorph(ref mat2, cb_THD2_DEStart.SelectedIndex, cb_THD2_DEStartCount.SelectedIndex);
				doMorph(ref mat2, cb_THD2_DEEnd.SelectedIndex, cb_THD2_DEEndCount.SelectedIndex);
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			if (exp_THD3.IsExpanded)
			{
				int num5 = (int)rs_SIMP_THD3.LowerValue;
				int num6 = (int)rs_SIMP_THD3.UpperValue;
				int selectedIndex3 = cb_SIMP_THD3_Channel.SelectedIndex;
				Mat srcBGR3 = inspection.buffers.GetSrcBGR(selectedIndex3);
				Mat mat3 = srcBGR3.Clone();
				Cv2.InRange(srcBGR3, num5, num6, mat3);
				doMorph(ref mat3, cb_THD3_DEStart.SelectedIndex, cb_THD3_DEStartCount.SelectedIndex);
				doMorph(ref mat3, cb_THD3_DEEnd.SelectedIndex, cb_THD3_DEEndCount.SelectedIndex);
				Cv2.BitwiseAnd(mat, mat3, mat);
			}
			SIMPSearchRect = GetSIMPSearchRect();
			SIMPSearchRect &= new OpenCvSharp.Rect(0, 0, mat.Cols, mat.Rows);
			if (SIMPSearchRect.Width <= 0 || SIMPSearchRect.Height <= 0)
			{
				txt_AllResult.Clear();
				if (LanguageType == 1)
				{
					txt_AllResult.Text = "[定位失敗] 定位檢測框參數錯誤，寬度高度為0!";
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text = "[定位失败] 定位检测框参数错误，宽度高度为0!";
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text = "[失敗しました] 位置合わせめ検出ボックスの幅と高さのパラメータが誤っており、0になっています";
				}
				else
				{
					txt_AllResult.Text = "[Locate Error] Tracker error! Tracker width and height = 0!";
				}
				return;
			}
			Mat mat4 = mat.Clone();
			mat4.SetTo(0.0);
			new Mat(mat4, SIMPSearchRect).SetTo(255.0);
			Cv2.BitwiseAnd(mat, mat4, mat);
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			Mat mat5 = new Mat();
			Cv2.Merge(ThdShow, mat5);
			mat5.CopyTo(ShowBitmap, mat);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
		catch
		{
			string text = "LW01-LocateWindow-doThdImage()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
	{
		if (count != 0 || index == 9)
		{
			switch (index)
			{
			case 1:
				Cv2.Dilate(mat, mat, new Mat(), null, count);
				break;
			case 2:
				Cv2.Erode(mat, mat, new Mat(), null, count);
				break;
			case 3:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, count);
				break;
			case 4:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, count);
				break;
			case 5:
			{
				Mat structuringElement4 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, count));
				Cv2.Dilate(mat, mat, structuringElement4, null, count);
				break;
			}
			case 6:
			{
				Mat structuringElement3 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, count));
				Cv2.Erode(mat, mat, structuringElement3, null, count);
				break;
			}
			case 7:
			{
				Mat structuringElement2 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(count, 1));
				Cv2.Dilate(mat, mat, structuringElement2, null, count);
				break;
			}
			case 8:
			{
				Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(count, 1));
				Cv2.Erode(mat, mat, structuringElement, null, count);
				break;
			}
			case 9:
				Cv2.Canny(mat, mat, 0.0, 130.0);
				break;
			case 10:
				Cv2.BitwiseNot(mat, mat);
				break;
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("定位區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("定位区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Tracker will reset to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("定位区域の位置は初期値に戻されます！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			bor_SIMPSearchRect.SetValue(Canvas.LeftProperty, 0.0);
			bor_SIMPSearchRect.SetValue(Canvas.TopProperty, 0.0);
			bor_SIMPSearchRect.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SIMPSearchRect.SetValue(FrameworkElement.HeightProperty, 100.0);
		}
	}

	private void Rs_QuadTHDValueL_ThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				QuadThdMin[i].Text = ((int)QuadThdValue[i].LowerValue).ToString();
				QuadThdMax[i].Text = ((int)QuadThdValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void doQUADThdImage(int index)
	{
		try
		{
			Label_Result.Visibility = Visibility.Hidden;
			br_LocateRect.Visibility = Visibility.Hidden;
			if (inspection.buffers.Src.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.Src.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.Src, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			if (Quad_EnableRGBThd[index].IsChecked == false)
			{
				int num = (int)QuadThdValue[index].LowerValue;
				int num2 = (int)QuadThdValue[index].UpperValue;
				int selectedIndex = QuadChannel[index].SelectedIndex;
				mat = inspection.buffers.GetSrcBGR(selectedIndex);
				mat2 = mat.Clone();
				if (QuadDEStart[index].SelectedIndex != 9)
				{
					Cv2.InRange(mat, num, num2, mat2);
				}
			}
			else
			{
				int num3 = (int)QuadThdRValue[index].LowerValue;
				int num4 = (int)QuadThdRValue[index].UpperValue;
				int num5 = (int)QuadThdGValue[index].LowerValue;
				int num6 = (int)QuadThdGValue[index].UpperValue;
				int num7 = (int)QuadThdBValue[index].LowerValue;
				int num8 = (int)QuadThdBValue[index].UpperValue;
				mat = inspection.buffers.Src;
				Cv2.InRange(mat, new Scalar(num7, num5, num3), new Scalar(num8, num6, num4), mat2);
			}
			doMorph(ref mat2, QuadDEStart[index].SelectedIndex, QuadDEStartCount[index].SelectedIndex);
			doMorph(ref mat2, QuadDEEnd[index].SelectedIndex, QuadDEEndCount[index].SelectedIndex);
			GetQuadSearchMask();
			if (CBEnableMask[index].IsChecked == true)
			{
				using Mat mat3 = new Mat(mat2, QuadSearchMask[index]);
				mat3.SetTo(0.0);
			}
			GetQuadSearchRect();
			QuadSearchRect[index] &= new OpenCvSharp.Rect(0, 0, mat2.Cols, mat2.Rows);
			if (QuadSearchRect[index].Width <= 0 || QuadSearchRect[index].Height <= 0)
			{
				txt_AllResult.Clear();
				if (LanguageType == 1)
				{
					txt_AllResult.Text = "[定位失敗] 定位檢測框參數錯誤，寬度高度為0!";
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text = "[定位失败] 定位检测框参数错误，宽度高度为0!";
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text = "[失敗しました] 検出範囲が正しく設定されていません。!";
				}
				else
				{
					txt_AllResult.Text = "[Locate Error] Tracker error! Tracker width and height = 0!";
				}
				return;
			}
			Mat mat4 = mat2.Clone();
			mat4.SetTo(0.0);
			new Mat(mat4, QuadSearchRect[index]).SetTo(255.0);
			Cv2.BitwiseAnd(mat2, mat4, mat2);
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			Mat mat5 = new Mat();
			Cv2.Merge(ThdShow, mat5);
			mat5.CopyTo(ShowBitmap, mat2);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
		catch (Exception ex)
		{
			string text = "LW02-LocateWindow-doQUADThdImage()";
			string text2 = text + " | index=" + index + " | " + ex;
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void doPADThdImage(int index)
	{
		try
		{
			Label_Result.Visibility = Visibility.Hidden;
			br_LocateRect.Visibility = Visibility.Hidden;
			if (inspection.buffers.Src.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.Src.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.Src, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			int num = (int)PadThdValue[index, 0].LowerValue;
			int num2 = (int)PadThdValue[index, 0].UpperValue;
			int num3 = (int)PadThdValue[index, 1].LowerValue;
			int num4 = (int)PadThdValue[index, 1].UpperValue;
			int selectedIndex = PadChannel[index, 0].SelectedIndex;
			Mat srcBGR = inspection.buffers.GetSrcBGR(selectedIndex);
			Mat mat = srcBGR.Clone();
			Cv2.InRange(srcBGR, num, num2, mat);
			doMorph(ref mat, PadDEStart[index, 0].SelectedIndex, PadDEStartCount[index, 0].SelectedIndex);
			doMorph(ref mat, PadDEEnd[index, 0].SelectedIndex, PadDEEndCount[index, 0].SelectedIndex);
			int selectedIndex2 = PadChannel[index, 1].SelectedIndex;
			Mat srcBGR2 = inspection.buffers.GetSrcBGR(selectedIndex2);
			Mat mat2 = srcBGR2.Clone();
			Cv2.InRange(srcBGR2, num3, num4, mat2);
			doMorph(ref mat2, PadDEStart[index, 1].SelectedIndex, PadDEStartCount[index, 1].SelectedIndex);
			doMorph(ref mat2, PadDEEnd[index, 1].SelectedIndex, PadDEEndCount[index, 1].SelectedIndex);
			Cv2.BitwiseAnd(mat, mat2, mat);
			GetPadSearchRect();
			PadSearchRect[0] &= new OpenCvSharp.Rect(0, 0, mat.Cols, mat.Rows);
			PadSearchRect[1] &= new OpenCvSharp.Rect(0, 0, mat2.Cols, mat2.Rows);
			if (PadSearchRect[index].Width <= 0 || PadSearchRect[index].Height <= 0)
			{
				txt_AllResult.Clear();
				if (LanguageType == 1)
				{
					txt_AllResult.Text = "[定位失敗] 定位檢測框參數錯誤，寬度高度為0!";
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text = "[定位失败] 定位检测框参数错误，宽度高度为0!";
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text = "[失敗しました] 検出範囲が正しく設定されていません。";
				}
				else
				{
					txt_AllResult.Text = "[Locate Error] Tracker error! Tracker width and height = 0!";
				}
				return;
			}
			Mat mat3 = mat.Clone();
			mat3.SetTo(0.0);
			new Mat(mat3, PadSearchRect[index]).SetTo(255.0);
			Cv2.BitwiseAnd(mat, mat3, mat);
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			Mat mat4 = new Mat();
			Cv2.Merge(ThdShow, mat4);
			mat4.CopyTo(ShowBitmap, mat);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
		catch
		{
			string text = "LW02-LocateWindow-doQUADThdImage()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_LocateType_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !(e.Source is TabControl) || !(e.AddedItems[0] is TabItem tabItem))
		{
			return;
		}
		tab_SIMP.Background = Brushes.White;
		tab_QUAD.Background = Brushes.White;
		tab_PAD.Background = Brushes.White;
		grd_SIMP_Setup.Visibility = Visibility.Visible;
		if (tabItem.Name == "tab_SIMP")
		{
			tab_SIMP.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF82D4CD"));
			btn_Ruler.Visibility = Visibility.Visible;
			if (LanguageType == 1)
			{
				MessageBox.Show("定位方式將更換為簡單定位！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("定位方式将更换为简单定位！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("位置特定方法が簡単な位置特定に切り替わります！", "ご注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Locate Functio will change to Easy Locate！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			inspection.paramFile.LocateParam.LocateType = 0;
			LocateType = 0;
			CopyValueToUI();
			bor_SearchMaskL.Visibility = Visibility.Hidden;
			bor_SearchMaskR.Visibility = Visibility.Hidden;
			bor_SearchMaskT.Visibility = Visibility.Hidden;
			bor_SearchMaskB.Visibility = Visibility.Hidden;
			Btn_Test_Click(sender, e);
		}
		else if (tabItem.Name == "tab_QUAD")
		{
			tab_QUAD.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF90C3DA"));
			btn_Ruler.Visibility = Visibility.Hidden;
			if (LanguageType == 1)
			{
				MessageBox.Show("定位方式將更換為四邊定位！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("定位方式将更换为四边定位！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("位置決め方式が四辺位置決めへ変更されます！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Locate Functio will change to Quad Locate！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			inspection.paramFile.LocateParam.LocateType = 1;
			LocateType = 1;
			CopyValueToUI();
			Btn_Test_Click(sender, e);
		}
		else if (tabItem.Name == "tab_PAD")
		{
			tab_PAD.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEEA4B7"));
			btn_Ruler.Visibility = Visibility.Hidden;
			if (LanguageType == 1)
			{
				MessageBox.Show("定位方式將更換為電極定位！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("定位方式将更换为电极定位！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("位置特定方法は電極位置特定に変更されます！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Locate Functio will change to Pad Locate！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			inspection.paramFile.LocateParam.LocateType = 2;
			LocateType = 2;
			CopyValueToUI();
			bor_SearchMaskL.Visibility = Visibility.Hidden;
			bor_SearchMaskR.Visibility = Visibility.Hidden;
			bor_SearchMaskT.Visibility = Visibility.Hidden;
			bor_SearchMaskB.Visibility = Visibility.Hidden;
			Btn_Test_Click(sender, e);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_QuadTrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("定位區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("定位区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Tracker will reset to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("位置合わせはデフォルト値にリセットされます！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			borQuadSearchRect[num].SetValue(Canvas.LeftProperty, 0.0);
			borQuadSearchRect[num].SetValue(Canvas.TopProperty, 0.0);
			borQuadSearchRect[num].SetValue(FrameworkElement.WidthProperty, 100.0);
			borQuadSearchRect[num].SetValue(FrameworkElement.HeightProperty, 100.0);
		}
	}

	private void Btn_PreviewScanL_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		inspection.DoLocate();
		if (!inspection.locateTool.QuadScanStatus[num])
		{
			return;
		}
		List<OpenCvSharp.Point> scanPt = inspection.locateTool.GetScanPt(num);
		if (scanPt.Count > 0)
		{
			if (inspection.buffers.Src.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.Src.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.Src, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			for (int i = 0; i < scanPt.Count - 1; i++)
			{
				ShowBitmap.Line(scanPt[i], scanPt[i + 1], Scalar.Lime, 2);
			}
			ShowBitmap.Line(inspection.locateTool.Drawlines[num].P1, inspection.locateTool.Drawlines[num].P2, Scalar.Fuchsia);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
	}

	private void Btn_PreviewQuadThdL_Click(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			Button button = (Button)sender;
			for (int i = 0; i < 4; i++)
			{
				QuadThdMin[i].Text = ((int)QuadThdValue[i].LowerValue).ToString();
				QuadThdMax[i].Text = ((int)QuadThdValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(button.Uid));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cnv_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		cnv_Image.Cursor = Cursors.Arrow;
		if (ColorDropper[4])
		{
			int num = CurrentColor[cb_SIMP_THD1_Channel.SelectedIndex];
			int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
			int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
			rs_SIMP_THD1.LowerValue = num2;
			rs_SIMP_THD1.UpperValue = num3;
			nu_Thd1LimitMin.Value = num2;
			nu_Thd1LimitMax.Value = num3;
			if (exp_THD2.IsExpanded)
			{
				num = CurrentColor[cb_SIMP_THD2_Channel.SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				rs_SIMP_THD2.LowerValue = num2;
				rs_SIMP_THD2.UpperValue = num3;
				nu_Thd2LimitMin.Value = num2;
				nu_Thd2LimitMax.Value = num3;
			}
			if (exp_THD3.IsExpanded)
			{
				num = CurrentColor[cb_SIMP_THD3_Channel.SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				rs_SIMP_THD3.LowerValue = num2;
				rs_SIMP_THD3.UpperValue = num3;
				nu_Thd3LimitMin.Value = num2;
				nu_Thd3LimitMax.Value = num3;
			}
			ColorDropper[4] = false;
			doThdImage();
		}
		for (int i = 0; i < 4; i++)
		{
			if (!ColorDropper[i])
			{
				continue;
			}
			if (Quad_EnableRGBThd[i].IsChecked == false)
			{
				int num = CurrentColor[QuadChannel[i].SelectedIndex];
				int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				QuadThdValue[i].LowerValue = num2;
				QuadThdValue[i].UpperValue = num3;
				ColorDropper[i] = false;
			}
			else
			{
				int num4 = CurrentColor[0];
				int num5 = CurrentColor[1];
				int num6 = CurrentColor[2];
				if (num4 - AutoThdValue >= 0)
				{
					QuadThdRValue[i].LowerValue = num4 - AutoThdValue;
				}
				else
				{
					QuadThdRValue[i].LowerValue = 0.0;
				}
				Quad_THDR_Min[i].Content = (int)QuadThdRValue[i].LowerValue;
				if (num4 + AutoThdValue <= 255)
				{
					QuadThdRValue[i].UpperValue = num4 + AutoThdValue;
				}
				else
				{
					QuadThdRValue[i].UpperValue = 255.0;
				}
				Quad_THDR_Max[i].Content = (int)QuadThdRValue[i].UpperValue;
				if (num5 - AutoThdValue >= 0)
				{
					QuadThdGValue[i].LowerValue = num5 - AutoThdValue;
				}
				else
				{
					QuadThdGValue[i].LowerValue = 0.0;
				}
				Quad_THDG_Min[i].Content = (int)QuadThdGValue[i].LowerValue;
				if (num5 + AutoThdValue <= 255)
				{
					QuadThdGValue[i].UpperValue = num5 + AutoThdValue;
				}
				else
				{
					QuadThdGValue[i].UpperValue = 255.0;
				}
				Quad_THDG_Max[i].Content = (int)QuadThdGValue[i].UpperValue;
				if (num6 - AutoThdValue >= 0)
				{
					QuadThdBValue[i].LowerValue = num6 - AutoThdValue;
				}
				else
				{
					QuadThdBValue[i].LowerValue = 0.0;
				}
				Quad_THDB_Min[i].Content = (int)QuadThdBValue[i].LowerValue;
				if (num6 + AutoThdValue <= 255)
				{
					QuadThdBValue[i].UpperValue = num6 + AutoThdValue;
				}
				else
				{
					QuadThdBValue[i].UpperValue = 255.0;
				}
				Quad_THDB_Max[i].Content = (int)QuadThdBValue[i].UpperValue;
				ColorDropper[i] = false;
			}
			doQUADThdImage(i);
		}
		for (int j = 5; j < 7; j++)
		{
			if (ColorDropper[j])
			{
				int num = CurrentColor[PadChannel[j - 5, 0].SelectedIndex];
				int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				PadThdValue[j - 5, 0].LowerValue = num2;
				PadThdValue[j - 5, 0].UpperValue = num3;
				num = CurrentColor[PadChannel[j - 5, 1].SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				PadThdValue[j - 5, 1].LowerValue = num2;
				PadThdValue[j - 5, 1].UpperValue = num3;
				ColorDropper[j] = false;
				doPADThdImage(j - 5);
			}
		}
		if (!RulerMode)
		{
			return;
		}
		System.Windows.Point position = e.GetPosition(this);
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return;
		}
		int num7 = (int)((position.X - cnv_Image.Margin.Left) / xRatio);
		int num8 = (int)(position.Y / yRatio);
		if (Pt1.X == 0f && Pt1.Y == 0f)
		{
			Pt1.X = num7;
			Pt1.Y = num8;
			cnv_Image.Cursor = Cursors.Cross;
			Mat mat = inspection.buffers.Src.Clone();
			mat.Line((int)Pt1.X - 5, (int)Pt1.Y - 5, (int)Pt1.X + 5, (int)Pt1.Y + 5, Scalar.Yellow);
			mat.Line((int)Pt1.X + 5, (int)Pt1.Y - 5, (int)Pt1.X - 5, (int)Pt1.Y + 5, Scalar.Yellow);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
			if (LanguageType == 1)
			{
				lab_Ruler.Content = string.Format("尺寸量測模式  請點選第二點!");
			}
			else if (LanguageType == 2)
			{
				lab_Ruler.Content = string.Format("尺寸量测模式  请点选第二点!");
			}
			else if (LanguageType == 3)
			{
				lab_Ruler.Content = string.Format("サイズ計測モード: 第二点を選択してください。");
			}
			else
			{
				lab_Ruler.Content = string.Format("Measure size mode, plase set second pt!");
			}
			return;
		}
		Pt2.X = num7;
		Pt2.Y = num8;
		Mat mat2 = inspection.buffers.Src.Clone();
		mat2.Line((int)Pt1.X - 5, (int)Pt1.Y - 5, (int)Pt1.X + 5, (int)Pt1.Y + 5, Scalar.Yellow);
		mat2.Line((int)Pt1.X + 5, (int)Pt1.Y - 5, (int)Pt1.X - 5, (int)Pt1.Y + 5, Scalar.Yellow);
		mat2.Line((int)Pt2.X - 5, (int)Pt2.Y - 5, (int)Pt2.X + 5, (int)Pt2.Y + 5, Scalar.Yellow);
		mat2.Line((int)Pt2.X + 5, (int)Pt2.Y - 5, (int)Pt2.X - 5, (int)Pt2.Y + 5, Scalar.Yellow);
		mat2.Line((int)Pt1.X, (int)Pt1.Y, (int)Pt2.X, (int)Pt2.Y, Scalar.Red);
		ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(mat2);
		ImageBrush imageBrush2 = new ImageBrush();
		imageBrush2.ImageSource = imageSource2;
		cnv_Image.Background = imageBrush2;
		imageSource2 = null;
		THD_Preview = false;
		Point2f point2f = Pt1 - Pt2;
		if (LanguageType == 1)
		{
			if (inspection.paramFile.MeasureUnit == 0)
			{
				lab_Ruler.Content = string.Format("尺寸量測模式 長度 = {0:N1}um 。 ({1} pixel)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
			}
			else
			{
				lab_Ruler.Content = string.Format("尺寸量測模式 長度 = {0:N3}mm 。 ({1} pixel)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
			}
		}
		else if (LanguageType == 2)
		{
			if (inspection.paramFile.MeasureUnit == 0)
			{
				lab_Ruler.Content = string.Format("尺寸量测模式 长度 = {0:N1}um 。 ({1} pixel)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
			}
			else
			{
				lab_Ruler.Content = string.Format("尺寸量测模式 长度 = {0:N3}mm 。 ({1} pixel)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
			}
		}
		else if (LanguageType == 3)
		{
			if (inspection.paramFile.MeasureUnit == 0)
			{
				lab_Ruler.Content = string.Format("寸法測定モード 長さ = {0:N1}um 。 ({1} pixel)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
			}
			else
			{
				lab_Ruler.Content = string.Format("寸法測定モード 長さ = {0:N3}mm 。 ({1} pixel)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
			}
		}
		else if (inspection.paramFile.MeasureUnit == 0)
		{
			lab_Ruler.Content = string.Format("Dimension measurement mode Length = {0:N1}um. ({1} pixels)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y));
		}
		else
		{
			lab_Ruler.Content = string.Format("Dimension measurement mode Length = {0:N3}mm. ({1} pixels)", CalcDistance(Pt1, Pt2), (int)Math.Sqrt(point2f.X * point2f.X + point2f.Y));
		}
		Pt1 = new Point2f(0f, 0f);
		Pt2 = new Point2f(0f, 0f);
	}

	private double CalcDistance(Point2f a, Point2f b)
	{
		Point2f point2f = a - b;
		point2f.X *= (float)inspection.paramFile.PixelPerUM_X;
		point2f.Y *= (float)inspection.paramFile.PixelPerUM_Y;
		return Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y);
	}

	public static bool IsNumeric(string TextBoxValue)
	{
		try
		{
			Convert.ToDouble(TextBoxValue);
			return true;
		}
		catch
		{
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckTextBox()
	{
		bool result = true;
		if (!IsNumeric(txt_doRotateMinAngle.Text))
		{
			txt_doRotateMinAngle.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_doRotateMinAngle.Background = Brushes.White;
		}
		if (!IsNumeric(txt_RotatLimitAngle.Text))
		{
			txt_RotatLimitAngle.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_RotatLimitAngle.Background = Brushes.White;
		}
		if (!txt_RotatWidthMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RotatWidthMin.Text))
			{
				txt_RotatWidthMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RotatWidthMin.Background = Brushes.White;
			}
		}
		if (!txt_RotatWidthMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RotatWidthMax.Text))
			{
				txt_RotatWidthMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RotatWidthMax.Background = Brushes.White;
			}
		}
		if (!txt_RotatHeightMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RotatHeightMin.Text))
			{
				txt_RotatHeightMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RotatHeightMin.Background = Brushes.White;
			}
		}
		if (!txt_RotatHeightMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RotatHeightMax.Text))
			{
				txt_RotatHeightMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RotatHeightMax.Background = Brushes.White;
			}
		}
		return result;
	}

	private void Bor_SearchRectR_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (LocateType == 1)
		{
			grd_QUAD_THD_L.Background = Brushes.White;
			grd_QUAD_THD_R.Background = Brushes.Yellow;
			grd_QUAD_THD_T.Background = Brushes.White;
			grd_QUAD_THD_B.Background = Brushes.White;
			QuadSelectIndex = 1;
		}
		else
		{
			grd_Pad_THD_L.Background = Brushes.White;
			grd_Pad_THD_R.Background = Brushes.Yellow;
			PadSelectIndex = 1;
		}
	}

	private void Bor_SearchRectL_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (LocateType == 1)
		{
			grd_QUAD_THD_L.Background = Brushes.Yellow;
			grd_QUAD_THD_R.Background = Brushes.White;
			grd_QUAD_THD_T.Background = Brushes.White;
			grd_QUAD_THD_B.Background = Brushes.White;
			QuadSelectIndex = 0;
		}
		else
		{
			grd_Pad_THD_L.Background = Brushes.Yellow;
			grd_Pad_THD_R.Background = Brushes.White;
			PadSelectIndex = 0;
		}
	}

	private void Bor_SearchRectT_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		grd_QUAD_THD_L.Background = Brushes.White;
		grd_QUAD_THD_R.Background = Brushes.White;
		grd_QUAD_THD_T.Background = Brushes.Yellow;
		grd_QUAD_THD_B.Background = Brushes.White;
		QuadSelectIndex = 2;
	}

	private void Bor_SearchRectB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		grd_QUAD_THD_L.Background = Brushes.White;
		grd_QUAD_THD_R.Background = Brushes.White;
		grd_QUAD_THD_T.Background = Brushes.White;
		grd_QUAD_THD_B.Background = Brushes.Yellow;
		QuadSelectIndex = 3;
	}

	private void Rs_PadTHDValueL1_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			for (int i = 0; i < 2; i++)
			{
				PadThdMin[0, i].Text = ((int)PadThdValue[0, i].LowerValue).ToString();
				PadThdMax[0, i].Text = ((int)PadThdValue[0, i].UpperValue).ToString();
			}
			doPADThdImage(0);
		}
	}

	private void Rs_PadTHDValueR1_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			for (int i = 0; i < 2; i++)
			{
				PadThdMin[1, i].Text = ((int)PadThdValue[1, i].LowerValue).ToString();
				PadThdMax[1, i].Text = ((int)PadThdValue[1, i].UpperValue).ToString();
			}
			doPADThdImage(1);
		}
	}

	private void Btn_PreviewPadThdL_Click(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			for (int i = 0; i < 2; i++)
			{
				PadThdMin[0, i].Text = ((int)PadThdValue[0, i].LowerValue).ToString();
				PadThdMax[0, i].Text = ((int)PadThdValue[0, i].UpperValue).ToString();
			}
			doPADThdImage(0);
		}
	}

	private void Btn_PreviewPadThdR_Click(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			for (int i = 0; i < 2; i++)
			{
				PadThdMin[1, i].Text = ((int)PadThdValue[1, i].LowerValue).ToString();
				PadThdMax[1, i].Text = ((int)PadThdValue[1, i].UpperValue).ToString();
			}
			doPADThdImage(1);
		}
	}

	private void Btn_ColorDropperL_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (!ColorDropper[num])
		{
			cnv_Image.Cursor = Cursors.Cross;
			ColorDropper[num] = true;
		}
		else
		{
			cnv_Image.Cursor = Cursors.Arrow;
			ColorDropper[num] = false;
		}
	}

	private void Rs_QuadTHDValueL_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				QuadChannel[i].SelectedIndex = QuadChannel[num].SelectedIndex;
				QuadThdValue[i].LowerValue = QuadThdValue[num].LowerValue;
				QuadThdValue[i].UpperValue = QuadThdValue[num].UpperValue;
				QuadThdMin[i].Text = QuadThdMin[num].Text;
				QuadThdMax[i].Text = QuadThdMax[num].Text;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[i] = false;
			}
		}
	}

	private void Rs_PadTHDValueL1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 5; i < 7; i++)
		{
			if (ColorDropper[i])
			{
				PadChannel[i - 5, 0].SelectedIndex = PadChannel[num, 0].SelectedIndex;
				PadThdValue[i - 5, 0].LowerValue = PadThdValue[num, 0].LowerValue;
				PadThdValue[i - 5, 0].UpperValue = PadThdValue[num, 0].UpperValue;
				PadThdMin[i - 5, 0].Text = PadThdMin[num, 0].Text;
				PadThdMax[i - 5, 0].Text = PadThdMax[num, 0].Text;
				PadChannel[i - 5, 1].SelectedIndex = PadChannel[num, 1].SelectedIndex;
				PadThdValue[i - 5, 1].LowerValue = PadThdValue[num, 1].LowerValue;
				PadThdValue[i - 5, 1].UpperValue = PadThdValue[num, 1].UpperValue;
				PadThdMin[i - 5, 1].Text = PadThdMin[num, 1].Text;
				PadThdMax[i - 5, 1].Text = PadThdMax[num, 1].Text;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[i] = false;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CopyValueToUI()
	{
		try
		{
			INSP_LOCATE_PARAM locateParam = inspection.paramFile.LocateParam;
			if (PreviewMode)
			{
				btn_Test.Visibility = Visibility.Hidden;
			}
			UpdateUI = false;
			if (!UseAILocate)
			{
				tab_LocateType.SelectedIndex = (LocateType = locateParam.LocateType);
			}
			tab_SIMP.Background = Brushes.White;
			tab_QUAD.Background = Brushes.White;
			tab_PAD.Background = Brushes.White;
			btn_Ruler.Visibility = Visibility.Hidden;
			chk_EnableFocusCheck.IsChecked = locateParam.EnableFocusCheck;
			txt_FocusLimit.Text = locateParam.FocusLimit.ToString();
			if (LocateType == 0 || UseAILocate)
			{
				btn_Ruler.Visibility = Visibility.Visible;
				tab_SIMP.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF82D4CD"));
				bor_SearchMaskL.Visibility = Visibility.Hidden;
				bor_SearchMaskR.Visibility = Visibility.Hidden;
				bor_SearchMaskT.Visibility = Visibility.Hidden;
				bor_SearchMaskB.Visibility = Visibility.Hidden;
			}
			else if (LocateType == 1)
			{
				tab_QUAD.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF90C3DA"));
			}
			else
			{
				tab_PAD.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEEA4B7"));
				bor_SearchMaskL.Visibility = Visibility.Hidden;
				bor_SearchMaskR.Visibility = Visibility.Hidden;
				bor_SearchMaskT.Visibility = Visibility.Hidden;
				bor_SearchMaskB.Visibility = Visibility.Hidden;
			}
			if (inspection.paramFile.AllowEnableAILocate)
			{
				cb_EnableAILocate.IsChecked = locateParam.EnableAILocate;
				cb_EnableAILocate.Visibility = Visibility.Visible;
				bor_AILocateRect.Visibility = Visibility.Visible;
			}
			else
			{
				cb_EnableAILocate.IsChecked = false;
				cb_EnableAILocate.Visibility = Visibility.Hidden;
				bor_AILocateRect.Visibility = Visibility.Hidden;
			}
			int num = 3;
			_ = inspection.paramFile.AllowEnableAILocate;
			if (locateParam.EnableAILocate)
			{
				UpdateColorChannelItem(cb_SIMP_THD1_Channel, usedAI: true);
				UpdateColorChannelItem(cb_SIMP_THD2_Channel, usedAI: true);
				UpdateColorChannelItem(cb_SIMP_THD3_Channel, usedAI: true);
				for (int i = 0; i < 4; i++)
				{
					UpdateColorChannelItem(QuadChannel[i], usedAI: true);
				}
				UpdateColorChannelItem(cb_PadChannelL1, usedAI: true);
				UpdateColorChannelItem(cb_PadChannelL2, usedAI: true);
				num = 6;
			}
			else
			{
				UpdateColorChannelItem(cb_SIMP_THD1_Channel, usedAI: false);
				UpdateColorChannelItem(cb_SIMP_THD2_Channel, usedAI: false);
				UpdateColorChannelItem(cb_SIMP_THD3_Channel, usedAI: false);
				for (int j = 0; j < 4; j++)
				{
					UpdateColorChannelItem(QuadChannel[j], usedAI: false);
				}
				UpdateColorChannelItem(cb_PadChannelL1, usedAI: false);
				UpdateColorChannelItem(cb_PadChannelL2, usedAI: false);
			}
			txt_RotatLimitAngle.Text = locateParam.LocateAngleLimit.ToString();
			chk_CreateHSVChannel.IsChecked = locateParam.CreateHSVChannel;
			txt_RotatWidthMin.Text = GetComDigitsStr(locateParam.LocateWidthMin, txt_RotatWidthMin);
			txt_RotatWidthMax.Text = GetComDigitsStr(locateParam.LocateWidthMax, txt_RotatWidthMax);
			txt_RotatHeightMin.Text = GetComDigitsStr(locateParam.LocateHeightMin, txt_RotatHeightMin);
			txt_RotatHeightMax.Text = GetComDigitsStr(locateParam.LocateHeightMax, txt_RotatHeightMax);
			txt_doRotateMinAngle.Text = locateParam.doRotateMinAngle.ToString();
			cb_RotatErrorBin.SelectedIndex = (int)locateParam.LocateErrorBin;
			cb_SizeErrorBin.SelectedIndex = (int)locateParam.LocateSizeErrorBin;
			if (cb_ParetoChart.Items.Count > locateParam.ParetoIndex)
			{
				cb_ParetoChart.SelectedIndex = locateParam.ParetoIndex;
			}
			else
			{
				cb_ParetoChart.SelectedIndex = -1;
			}
			cb_LocateSpeedUp.IsChecked = locateParam.LocateSpeedUp;
			if (locateParam.LocateSpeedUp)
			{
				txt_doRotateMinAngle.Visibility = Visibility.Visible;
				lab_doRotateMinAngle.Visibility = Visibility.Visible;
			}
			else
			{
				txt_doRotateMinAngle.Visibility = Visibility.Hidden;
				lab_doRotateMinAngle.Visibility = Visibility.Hidden;
			}
			SIMPSearchRect = locateParam.SIMPSearchRect;
			OpenCvSharp.Rect maxrect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			inspection.locateTool.ReCalcMaxSearchRect(locateParam.LocateType, maxrect);
			AIInfluenceRect = locateParam.MaxSearchRect;
			if (locateParam.LocateType == 0)
			{
				SIMPSearchRect = locateParam.SIMPSearchRect;
				SetSIMPSearchRect(SIMPSearchRect);
			}
			SetAILocateRect(AIInfluenceRect);
			chk_EnableTensorRT.IsChecked = locateParam.EnableTensorRT;
			txt_ModeName.Text = locateParam.AILocateModelName;
			txt_TrainCount.Text = locateParam.AILocateTrainCount.ToString();
			cb_SIMP_THD1_Channel.SelectedIndex = locateParam.SIMP_THD1_Channel % num;
			rs_SIMP_THD1.LowerValue = locateParam.SIMP_THD1_Min;
			rs_SIMP_THD1.UpperValue = locateParam.SIMP_THD1_Max;
			nu_Thd1LimitMin.Value = locateParam.SIMP_THD1_Min;
			nu_Thd1LimitMax.Value = locateParam.SIMP_THD1_Max;
			cb_SIMP_SideLocate.IsChecked = locateParam.SIMP_SideLocate;
			if (locateParam.SIMP_THD2_Enabled)
			{
				exp_THD2.IsExpanded = true;
			}
			else
			{
				exp_THD2.IsExpanded = false;
			}
			if (locateParam.SIMP_THD3_Enabled)
			{
				exp_THD3.IsExpanded = true;
			}
			else
			{
				exp_THD3.IsExpanded = false;
			}
			cb_SIMP_THD2_Channel.SelectedIndex = locateParam.SIMP_THD2_Channel % num;
			rs_SIMP_THD2.LowerValue = locateParam.SIMP_THD2_Min;
			rs_SIMP_THD2.UpperValue = locateParam.SIMP_THD2_Max;
			nu_Thd2LimitMin.Value = locateParam.SIMP_THD2_Min;
			nu_Thd2LimitMax.Value = locateParam.SIMP_THD2_Max;
			cb_SIMP_THD3_Channel.SelectedIndex = locateParam.SIMP_THD3_Channel % num;
			rs_SIMP_THD3.LowerValue = locateParam.SIMP_THD3_Min;
			rs_SIMP_THD3.UpperValue = locateParam.SIMP_THD3_Max;
			nu_Thd3LimitMin.Value = locateParam.SIMP_THD3_Min;
			nu_Thd3LimitMax.Value = locateParam.SIMP_THD3_Max;
			cb_THD1_DEStart.SelectedIndex = (int)locateParam.SIMP_DilateErode1Start;
			cb_THD1_DEStartCount.SelectedIndex = locateParam.SIMP_DilateErode1StartCount;
			cb_THD1_DEEnd.SelectedIndex = (int)locateParam.SIMP_DilateErode1End;
			cb_THD1_DEEndCount.SelectedIndex = locateParam.SIMP_DilateErode1EndCount;
			cb_THD2_DEStart.SelectedIndex = (int)locateParam.SIMP_DilateErode2Start;
			cb_THD2_DEStartCount.SelectedIndex = locateParam.SIMP_DilateErode2StartCount;
			cb_THD2_DEEnd.SelectedIndex = (int)locateParam.SIMP_DilateErode2End;
			cb_THD2_DEEndCount.SelectedIndex = locateParam.SIMP_DilateErode2EndCount;
			cb_THD3_DEStart.SelectedIndex = (int)locateParam.SIMP_DilateErode3Start;
			cb_THD3_DEStartCount.SelectedIndex = locateParam.SIMP_DilateErode3StartCount;
			cb_THD3_DEEnd.SelectedIndex = (int)locateParam.SIMP_DilateErode3End;
			cb_THD3_DEEndCount.SelectedIndex = locateParam.SIMP_DilateErode3EndCount;
			QuadSearchRect[0] = locateParam.QuadSearchRectL;
			QuadSearchRect[1] = locateParam.QuadSearchRectR;
			QuadSearchRect[2] = locateParam.QuadSearchRectT;
			QuadSearchRect[3] = locateParam.QuadSearchRectB;
			QuadSearchMask[0] = locateParam.Quad_Mask_L;
			QuadSearchMask[1] = locateParam.Quad_Mask_R;
			QuadSearchMask[2] = locateParam.Quad_Mask_T;
			QuadSearchMask[3] = locateParam.Quad_Mask_B;
			if (locateParam.LocateType == 1)
			{
				SetQuadSearchRect();
				SetQuadSearchMask();
				if (locateParam.Quad_EnableMask_L)
				{
					cb_EnableMaskL.IsChecked = true;
					bor_SearchMaskL.Visibility = Visibility.Visible;
				}
				else
				{
					cb_EnableMaskL.IsChecked = false;
					bor_SearchMaskL.Visibility = Visibility.Hidden;
				}
				if (locateParam.Quad_EnableMask_R)
				{
					cb_EnableMaskR.IsChecked = true;
					bor_SearchMaskR.Visibility = Visibility.Visible;
				}
				else
				{
					cb_EnableMaskR.IsChecked = false;
					bor_SearchMaskR.Visibility = Visibility.Hidden;
				}
				if (locateParam.Quad_EnableMask_T)
				{
					cb_EnableMaskT.IsChecked = true;
					bor_SearchMaskT.Visibility = Visibility.Visible;
				}
				else
				{
					cb_EnableMaskT.IsChecked = false;
					bor_SearchMaskT.Visibility = Visibility.Hidden;
				}
				if (locateParam.Quad_EnableMask_B)
				{
					cb_EnableMaskB.IsChecked = true;
					bor_SearchMaskB.Visibility = Visibility.Visible;
				}
				else
				{
					cb_EnableMaskB.IsChecked = false;
					bor_SearchMaskB.Visibility = Visibility.Hidden;
				}
			}
			cb_QUAD_SideLocate.IsChecked = locateParam.QUAD_SideLocate;
			cb_EnableAvgEdgeL.IsChecked = locateParam.Quad_EnableAvgEdge_L;
			cb_EnableAvgEdgeT.IsChecked = locateParam.Quad_EnableAvgEdge_T;
			cb_EnableAvgEdgeR.IsChecked = locateParam.Quad_EnableAvgEdge_R;
			cb_EnableAvgEdgeB.IsChecked = locateParam.Quad_EnableAvgEdge_B;
			cb_QuadChannelL.SelectedIndex = locateParam.Quad_Channel_L % num;
			cb_QuadChannelR.SelectedIndex = locateParam.Quad_Channel_R % num;
			cb_QuadChannelT.SelectedIndex = locateParam.Quad_Channel_T % num;
			cb_QuadChannelB.SelectedIndex = locateParam.Quad_Channel_B % num;
			txt_QuadTHDMinL.Text = locateParam.Quad_THD_Min_L.ToString();
			txt_QuadTHDMinR.Text = locateParam.Quad_THD_Min_R.ToString();
			txt_QuadTHDMinT.Text = locateParam.Quad_THD_Min_T.ToString();
			txt_QuadTHDMinB.Text = locateParam.Quad_THD_Min_B.ToString();
			txt_QuadTHDMaxL.Text = locateParam.Quad_THD_Max_L.ToString();
			txt_QuadTHDMaxR.Text = locateParam.Quad_THD_Max_R.ToString();
			txt_QuadTHDMaxT.Text = locateParam.Quad_THD_Max_T.ToString();
			txt_QuadTHDMaxB.Text = locateParam.Quad_THD_Max_B.ToString();
			cb_Quad_EnableRGBThd_L.IsChecked = locateParam.Quad_EnableRGBThd_L;
			if (locateParam.Quad_EnableRGBThd_L)
			{
				cb_QuadChannelL.Visibility = Visibility.Hidden;
				grd_Quad_RGBThd_L.Visibility = Visibility.Visible;
				lab_Quad_THDR_Min_L.Content = locateParam.Quad_THDR_Min_L.ToString();
				lab_Quad_THDR_Max_L.Content = locateParam.Quad_THDR_Max_L.ToString();
				lab_Quad_THDG_Min_L.Content = locateParam.Quad_THDG_Min_L.ToString();
				lab_Quad_THDG_Max_L.Content = locateParam.Quad_THDG_Max_L.ToString();
				lab_Quad_THDB_Min_L.Content = locateParam.Quad_THDB_Min_L.ToString();
				lab_Quad_THDB_Max_L.Content = locateParam.Quad_THDB_Max_L.ToString();
				rs_Quad_THDR_L.LowerValue = locateParam.Quad_THDR_Min_L;
				rs_Quad_THDR_L.UpperValue = locateParam.Quad_THDR_Max_L;
				rs_Quad_THDG_L.LowerValue = locateParam.Quad_THDG_Min_L;
				rs_Quad_THDG_L.UpperValue = locateParam.Quad_THDG_Max_L;
				rs_Quad_THDB_L.LowerValue = locateParam.Quad_THDB_Min_L;
				rs_Quad_THDB_L.UpperValue = locateParam.Quad_THDB_Max_L;
			}
			else
			{
				cb_QuadChannelL.Visibility = Visibility.Visible;
				grd_Quad_RGBThd_L.Visibility = Visibility.Hidden;
			}
			cb_Quad_EnableRGBThd_R.IsChecked = locateParam.Quad_EnableRGBThd_R;
			if (locateParam.Quad_EnableRGBThd_R)
			{
				cb_QuadChannelR.Visibility = Visibility.Hidden;
				grd_Quad_RGBThd_R.Visibility = Visibility.Visible;
				lab_Quad_THDR_Min_R.Content = locateParam.Quad_THDR_Min_R.ToString();
				lab_Quad_THDR_Max_R.Content = locateParam.Quad_THDR_Max_R.ToString();
				lab_Quad_THDG_Min_R.Content = locateParam.Quad_THDG_Min_R.ToString();
				lab_Quad_THDG_Max_R.Content = locateParam.Quad_THDG_Max_R.ToString();
				lab_Quad_THDB_Min_R.Content = locateParam.Quad_THDB_Min_R.ToString();
				lab_Quad_THDB_Max_R.Content = locateParam.Quad_THDB_Max_R.ToString();
				rs_Quad_THDR_R.LowerValue = locateParam.Quad_THDR_Min_R;
				rs_Quad_THDR_R.UpperValue = locateParam.Quad_THDR_Max_R;
				rs_Quad_THDG_R.LowerValue = locateParam.Quad_THDG_Min_R;
				rs_Quad_THDG_R.UpperValue = locateParam.Quad_THDG_Max_R;
				rs_Quad_THDB_R.LowerValue = locateParam.Quad_THDB_Min_R;
				rs_Quad_THDB_R.UpperValue = locateParam.Quad_THDB_Max_R;
			}
			else
			{
				cb_QuadChannelR.Visibility = Visibility.Visible;
				grd_Quad_RGBThd_R.Visibility = Visibility.Hidden;
			}
			cb_Quad_EnableRGBThd_T.IsChecked = locateParam.Quad_EnableRGBThd_T;
			if (locateParam.Quad_EnableRGBThd_T)
			{
				cb_QuadChannelT.Visibility = Visibility.Hidden;
				grd_Quad_RGBThd_T.Visibility = Visibility.Visible;
				lab_Quad_THDR_Min_T.Content = locateParam.Quad_THDR_Min_T.ToString();
				lab_Quad_THDR_Max_T.Content = locateParam.Quad_THDR_Max_T.ToString();
				lab_Quad_THDG_Min_T.Content = locateParam.Quad_THDG_Min_T.ToString();
				lab_Quad_THDG_Max_T.Content = locateParam.Quad_THDG_Max_T.ToString();
				lab_Quad_THDB_Min_T.Content = locateParam.Quad_THDB_Min_T.ToString();
				lab_Quad_THDB_Max_T.Content = locateParam.Quad_THDB_Max_T.ToString();
				rs_Quad_THDR_T.LowerValue = locateParam.Quad_THDR_Min_T;
				rs_Quad_THDR_T.UpperValue = locateParam.Quad_THDR_Max_T;
				rs_Quad_THDG_T.LowerValue = locateParam.Quad_THDG_Min_T;
				rs_Quad_THDG_T.UpperValue = locateParam.Quad_THDG_Max_T;
				rs_Quad_THDB_T.LowerValue = locateParam.Quad_THDB_Min_T;
				rs_Quad_THDB_T.UpperValue = locateParam.Quad_THDB_Max_T;
			}
			else
			{
				cb_QuadChannelT.Visibility = Visibility.Visible;
				grd_Quad_RGBThd_T.Visibility = Visibility.Hidden;
			}
			cb_Quad_EnableRGBThd_B.IsChecked = locateParam.Quad_EnableRGBThd_B;
			if (locateParam.Quad_EnableRGBThd_B)
			{
				cb_QuadChannelB.Visibility = Visibility.Hidden;
				grd_Quad_RGBThd_B.Visibility = Visibility.Visible;
				lab_Quad_THDR_Min_B.Content = locateParam.Quad_THDR_Min_B.ToString();
				lab_Quad_THDR_Max_B.Content = locateParam.Quad_THDR_Max_B.ToString();
				lab_Quad_THDG_Min_B.Content = locateParam.Quad_THDG_Min_B.ToString();
				lab_Quad_THDG_Max_B.Content = locateParam.Quad_THDG_Max_B.ToString();
				lab_Quad_THDB_Min_B.Content = locateParam.Quad_THDB_Min_B.ToString();
				lab_Quad_THDB_Max_B.Content = locateParam.Quad_THDB_Max_B.ToString();
				rs_Quad_THDR_B.LowerValue = locateParam.Quad_THDR_Min_B;
				rs_Quad_THDR_B.UpperValue = locateParam.Quad_THDR_Max_B;
				rs_Quad_THDG_B.LowerValue = locateParam.Quad_THDG_Min_B;
				rs_Quad_THDG_B.UpperValue = locateParam.Quad_THDG_Max_B;
				rs_Quad_THDB_B.LowerValue = locateParam.Quad_THDB_Min_B;
				rs_Quad_THDB_B.UpperValue = locateParam.Quad_THDB_Max_B;
			}
			else
			{
				cb_QuadChannelB.Visibility = Visibility.Visible;
				grd_Quad_RGBThd_B.Visibility = Visibility.Hidden;
			}
			rs_QuadTHDValueL.LowerValue = locateParam.Quad_THD_Min_L;
			rs_QuadTHDValueL.UpperValue = locateParam.Quad_THD_Max_L;
			rs_QuadTHDValueR.LowerValue = locateParam.Quad_THD_Min_R;
			rs_QuadTHDValueR.UpperValue = locateParam.Quad_THD_Max_R;
			rs_QuadTHDValueT.LowerValue = locateParam.Quad_THD_Min_T;
			rs_QuadTHDValueT.UpperValue = locateParam.Quad_THD_Max_T;
			rs_QuadTHDValueB.LowerValue = locateParam.Quad_THD_Min_B;
			rs_QuadTHDValueB.UpperValue = locateParam.Quad_THD_Max_B;
			cb_Quad_DEStartL.SelectedIndex = (int)locateParam.Quad_DEStart_L;
			cb_Quad_DEStartR.SelectedIndex = (int)locateParam.Quad_DEStart_R;
			cb_Quad_DEStartT.SelectedIndex = (int)locateParam.Quad_DEStart_T;
			cb_Quad_DEStartB.SelectedIndex = (int)locateParam.Quad_DEStart_B;
			cb_Quad_DEEndL.SelectedIndex = (int)locateParam.Quad_DEEnd_L;
			cb_Quad_DEEndR.SelectedIndex = (int)locateParam.Quad_DEEnd_R;
			cb_Quad_DEEndT.SelectedIndex = (int)locateParam.Quad_DEEnd_T;
			cb_Quad_DEEndB.SelectedIndex = (int)locateParam.Quad_DEEnd_B;
			cb_Quad_DEStartCountL.SelectedIndex = locateParam.Quad_DEStartCount_L;
			cb_Quad_DEStartCountR.SelectedIndex = locateParam.Quad_DEStartCount_R;
			cb_Quad_DEStartCountT.SelectedIndex = locateParam.Quad_DEStartCount_T;
			cb_Quad_DEStartCountB.SelectedIndex = locateParam.Quad_DEStartCount_B;
			cb_Quad_DEEndCountL.SelectedIndex = locateParam.Quad_DEEndCount_L;
			cb_Quad_DEEndCountR.SelectedIndex = locateParam.Quad_DEEndCount_R;
			cb_Quad_DEEndCountT.SelectedIndex = locateParam.Quad_DEEndCount_T;
			cb_Quad_DEEndCountB.SelectedIndex = locateParam.Quad_DEEndCount_B;
			chk_UseFilterPoint.IsChecked = locateParam.Quad_FilterPoint;
			chk_UseStdFilter.IsChecked = locateParam.Quad_UseStdFilter;
			chk_UseSideLocate.IsChecked = locateParam.Quad_UseSideLocate;
			chk_FixSideLocateLR.IsChecked = locateParam.Quad_FixSideLocateLR;
			cb_Quad_LocateAngleSelect.SelectedIndex = locateParam.Quad_LocateAngleSelect;
			if ((int)locateParam.Direction_L % 2 == 0)
			{
				QuadScanDir1[0].IsChecked = true;
			}
			else
			{
				QuadScanDir2[0].IsChecked = true;
			}
			if ((int)locateParam.Direction_R % 2 == 0)
			{
				QuadScanDir1[1].IsChecked = true;
			}
			else
			{
				QuadScanDir2[1].IsChecked = true;
			}
			if ((int)locateParam.Direction_T % 2 == 0)
			{
				QuadScanDir1[2].IsChecked = true;
			}
			else
			{
				QuadScanDir2[2].IsChecked = true;
			}
			if ((int)locateParam.Direction_B % 2 == 0)
			{
				QuadScanDir1[3].IsChecked = true;
			}
			else
			{
				QuadScanDir2[3].IsChecked = true;
			}
			if (locateParam.UseMelfMode)
			{
				chk_UseMelfLocate.IsChecked = true;
			}
			else
			{
				chk_UseMelfLocate.IsChecked = false;
			}
			PadSearchRect[0] = locateParam.PadSearchRectL;
			PadSearchRect[1] = locateParam.PadSearchRectR;
			if (locateParam.LocateType == 2)
			{
				SetPadSearchRect();
			}
			cb_PadChannelL1.SelectedIndex = locateParam.Pad_Channel_L1 % num;
			cb_PadChannelR1.SelectedIndex = locateParam.Pad_Channel_R1 % num;
			cb_PadChannelL2.SelectedIndex = locateParam.Pad_Channel_L2 % num;
			cb_PadChannelR2.SelectedIndex = locateParam.Pad_Channel_R2 % num;
			txt_PadTHDMinL1.Text = locateParam.Pad_THD_Min_L1.ToString();
			txt_PadTHDMinR1.Text = locateParam.Pad_THD_Min_R1.ToString();
			txt_PadTHDMinL2.Text = locateParam.Pad_THD_Min_L2.ToString();
			txt_PadTHDMinR2.Text = locateParam.Pad_THD_Min_R2.ToString();
			txt_PadTHDMaxL1.Text = locateParam.Pad_THD_Max_L1.ToString();
			txt_PadTHDMaxR1.Text = locateParam.Pad_THD_Max_R1.ToString();
			txt_PadTHDMaxL2.Text = locateParam.Pad_THD_Max_L2.ToString();
			txt_PadTHDMaxR2.Text = locateParam.Pad_THD_Max_R2.ToString();
			rs_PadTHDValueL1.LowerValue = locateParam.Pad_THD_Min_L1;
			rs_PadTHDValueL1.UpperValue = locateParam.Pad_THD_Max_L1;
			rs_PadTHDValueR1.LowerValue = locateParam.Pad_THD_Min_R1;
			rs_PadTHDValueR1.UpperValue = locateParam.Pad_THD_Max_R1;
			rs_PadTHDValueL2.LowerValue = locateParam.Pad_THD_Min_L2;
			rs_PadTHDValueL2.UpperValue = locateParam.Pad_THD_Max_L2;
			rs_PadTHDValueR2.LowerValue = locateParam.Pad_THD_Min_R2;
			rs_PadTHDValueR2.UpperValue = locateParam.Pad_THD_Max_R2;
			cb_Pad_DEStartL1.SelectedIndex = (int)locateParam.Pad_DEStart_L1;
			cb_Pad_DEStartR1.SelectedIndex = (int)locateParam.Pad_DEStart_R1;
			cb_Pad_DEStartL2.SelectedIndex = (int)locateParam.Pad_DEStart_L2;
			cb_Pad_DEStartR2.SelectedIndex = (int)locateParam.Pad_DEStart_R2;
			cb_Pad_DEEndL1.SelectedIndex = (int)locateParam.Pad_DEEnd_L1;
			cb_Pad_DEEndR1.SelectedIndex = (int)locateParam.Pad_DEEnd_R1;
			cb_Pad_DEEndL2.SelectedIndex = (int)locateParam.Pad_DEEnd_L2;
			cb_Pad_DEEndR2.SelectedIndex = (int)locateParam.Pad_DEEnd_R2;
			cb_Pad_DEStartCountL1.SelectedIndex = locateParam.Pad_DEStartCount_L1;
			cb_Pad_DEStartCountR1.SelectedIndex = locateParam.Pad_DEStartCount_R1;
			cb_Pad_DEStartCountL2.SelectedIndex = locateParam.Pad_DEStartCount_L2;
			cb_Pad_DEStartCountR2.SelectedIndex = locateParam.Pad_DEStartCount_R2;
			cb_Pad_DEEndCountL1.SelectedIndex = locateParam.Pad_DEEndCount_L1;
			cb_Pad_DEEndCountR1.SelectedIndex = locateParam.Pad_DEEndCount_R1;
			cb_Pad_DEEndCountL2.SelectedIndex = locateParam.Pad_DEEndCount_L2;
			cb_Pad_DEEndCountR2.SelectedIndex = locateParam.Pad_DEEndCount_R2;
			UpdateUI = true;
			return true;
		}
		catch
		{
			string text = "LW03-LocateWindow-CopyValueToUI()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
			return false;
		}
	}

	private void Txt_RotatLimitAngle_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetKeyboard(tbox, position);
	}

	private void SetKeyboard(TextBox tbox, System.Windows.Point p)
	{
		HKeyboard hKeyboard = new HKeyboard();
		hKeyboard.setTxtbox(tbox);
		hKeyboard.SetInput(tbox.Text);
		hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
		hKeyboard.Left = p.X;
		if (hKeyboard.Left + hKeyboard.Width > MainW.Left + MainW.Width)
		{
			hKeyboard.Left = MainW.Left + MainW.Width - hKeyboard.Width;
		}
		hKeyboard.Top = p.Y - hKeyboard.Height + MainW.Top;
		tbox.Background = Brushes.Gold;
		hKeyboard.ShowDialog();
		tbox.Background = Brushes.White;
	}

	private void SetUnsignedINTKeyboard(TextBox tbox, System.Windows.Point p)
	{
		HKeyboard hKeyboard = new HKeyboard();
		hKeyboard.setTxtbox(tbox);
		hKeyboard.SetInput(tbox.Text);
		hKeyboard.SetInputNegative(state: false);
		hKeyboard.SetDouble(state: false);
		hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
		hKeyboard.Left = p.X;
		hKeyboard.Top = p.Y - hKeyboard.Height + MainW.Top;
		tbox.Background = Brushes.Gold;
		hKeyboard.ShowDialog();
		tbox.Background = Brushes.White;
	}

	private void Cnv_Image_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		System.Windows.Point position = e.GetPosition(cnv_Image);
		sfr.CenterX = position.X;
		sfr.CenterY = position.Y;
		if (!(sfr.ScaleX < 0.3) || !(sfr.ScaleY < 0.3) || e.Delta >= 0)
		{
			sfr.ScaleX += (double)e.Delta / 500.0;
			sfr.ScaleY += (double)e.Delta / 500.0;
			if (sfr.ScaleX <= 1.0)
			{
				sfr.ScaleX = 1.0;
			}
			if (sfr.ScaleY <= 1.0)
			{
				sfr.ScaleY = 1.0;
			}
		}
	}

	private void Nu_ThdLimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_SIMP_THD1.LowerValue = (int)nu_Thd1LimitMin.Value.Value;
		if (inited)
		{
			doThdImage();
		}
	}

	private void Nu_Thd1LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_SIMP_THD1.UpperValue = (int)nu_Thd1LimitMax.Value.Value;
		if (inited)
		{
			doThdImage();
		}
	}

	private void Nu_Thd3LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_SIMP_THD3.LowerValue = (int)nu_Thd3LimitMin.Value.Value;
		if (inited)
		{
			doThdImage();
		}
	}

	private void Nu_Thd3LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_SIMP_THD3.UpperValue = (int)nu_Thd3LimitMax.Value.Value;
		if (inited)
		{
			doThdImage();
		}
	}

	private void Nu_Thd2LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_SIMP_THD2.UpperValue = (int)nu_Thd2LimitMax.Value.Value;
		if (inited)
		{
			doThdImage();
		}
	}

	private void Nu_Thd2LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_SIMP_THD2.LowerValue = (int)nu_Thd2LimitMin.Value.Value;
		if (inited)
		{
			doThdImage();
		}
	}

	private void Cnv_Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (ColorDropper[4])
		{
			int num = CurrentColor[cb_SIMP_THD1_Channel.SelectedIndex];
			int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
			int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
			rs_SIMP_THD1.LowerValue = num2;
			rs_SIMP_THD1.UpperValue = num3;
			nu_Thd1LimitMin.Value = num2;
			nu_Thd1LimitMax.Value = num3;
			if (exp_THD2.IsExpanded)
			{
				num = CurrentColor[cb_SIMP_THD2_Channel.SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				rs_SIMP_THD2.LowerValue = num2;
				rs_SIMP_THD2.UpperValue = num3;
				nu_Thd2LimitMin.Value = num2;
				nu_Thd2LimitMax.Value = num3;
			}
			if (exp_THD3.IsExpanded)
			{
				num = CurrentColor[cb_SIMP_THD3_Channel.SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				rs_SIMP_THD3.LowerValue = num2;
				rs_SIMP_THD3.UpperValue = num3;
				nu_Thd3LimitMin.Value = num2;
				nu_Thd3LimitMax.Value = num3;
			}
			doThdImage();
		}
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				int num = CurrentColor[QuadChannel[i].SelectedIndex];
				int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				QuadThdValue[i].LowerValue = num2;
				QuadThdValue[i].UpperValue = num3;
				doQUADThdImage(i);
			}
		}
		for (int j = 5; j < 7; j++)
		{
			if (ColorDropper[j])
			{
				int num = CurrentColor[PadChannel[j - 5, 0].SelectedIndex];
				int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				PadThdValue[j - 5, 0].LowerValue = num2;
				PadThdValue[j - 5, 0].UpperValue = num3;
				num = CurrentColor[PadChannel[j - 5, 1].SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				PadThdValue[j - 5, 1].LowerValue = num2;
				PadThdValue[j - 5, 1].UpperValue = num3;
				doPADThdImage(j - 5);
			}
		}
		if (sfr.ScaleX != 1.0)
		{
			System.Windows.Point position = e.GetPosition(cnv_Image);
			sfr.CenterX = position.X;
			sfr.CenterY = position.Y;
		}
	}

	private void Cb_LocateSpeedUp_Click(object sender, RoutedEventArgs e)
	{
		if (cb_LocateSpeedUp.IsChecked == true)
		{
			txt_doRotateMinAngle.Visibility = Visibility.Visible;
			lab_doRotateMinAngle.Visibility = Visibility.Visible;
		}
		else
		{
			txt_doRotateMinAngle.Visibility = Visibility.Hidden;
			lab_doRotateMinAngle.Visibility = Visibility.Hidden;
		}
	}

	private void Btn_Chart_Click(object sender, RoutedEventArgs e)
	{
		int subitem = Convert.ToInt32(((Button)sender).Uid);
		ChartWindow chartWindow = new ChartWindow();
		chartWindow.SetInspection(inspection);
		chartWindow.LoadLanguage(LanguageType);
		chartWindow.SetStatistics(0, subitem, 0);
		chartWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteUIValueToParam()
	{
		if (PreviewMode)
		{
			return false;
		}
		try
		{
			INSP_LOCATE_PARAM locateParam = inspection.paramFile.LocateParam;
			locateParam.ValueChangeFlag = true;
			locateParam.LocateType = tab_LocateType.SelectedIndex;
			locateParam.LocateAngleLimit = Convert.ToDouble(txt_RotatLimitAngle.Text);
			locateParam.CreateHSVChannel = chk_CreateHSVChannel.IsChecked.Value;
			locateParam.LocateWidthMin = ConvertComDigits(txt_RotatWidthMin);
			locateParam.LocateWidthMax = ConvertComDigits(txt_RotatWidthMax);
			locateParam.LocateHeightMin = ConvertComDigits(txt_RotatHeightMin);
			locateParam.LocateHeightMax = ConvertComDigits(txt_RotatHeightMax);
			locateParam.doRotateMinAngle = Convert.ToDouble(txt_doRotateMinAngle.Text);
			locateParam.LocateErrorBin = (FLOW_BIN)cb_RotatErrorBin.SelectedIndex;
			locateParam.LocateSizeErrorBin = (FLOW_BIN)cb_SizeErrorBin.SelectedIndex;
			locateParam.ParetoIndex = cb_ParetoChart.SelectedIndex;
			locateParam.LocateSpeedUp = cb_LocateSpeedUp.IsChecked.Value;
			locateParam.EnableFocusCheck = chk_EnableFocusCheck.IsChecked.Value;
			locateParam.FocusLimit = Convert.ToDouble(txt_FocusLimit.Text);
			_ = locateParam.LocateSpeedUp;
			SIMPSearchRect = GetSIMPSearchRect();
			locateParam.SIMPSearchRect = SIMPSearchRect;
			locateParam.EnableAILocate = cb_EnableAILocate.IsChecked.Value;
			locateParam.EnableTensorRT = chk_EnableTensorRT.IsChecked.Value;
			locateParam.AILocateModelName = txt_ModeName.Text;
			locateParam.AILocateTrainCount = Convert.ToInt32(txt_TrainCount.Text);
			locateParam.SIMP_THD1_Channel = cb_SIMP_THD1_Channel.SelectedIndex;
			locateParam.SIMP_THD1_Min = (int)rs_SIMP_THD1.LowerValue;
			locateParam.SIMP_THD1_Max = (int)rs_SIMP_THD1.UpperValue;
			locateParam.SIMP_SideLocate = cb_SIMP_SideLocate.IsChecked.Value;
			locateParam.SIMP_THD2_Enabled = exp_THD2.IsExpanded;
			locateParam.SIMP_THD2_Channel = cb_SIMP_THD2_Channel.SelectedIndex;
			locateParam.SIMP_THD2_Min = (int)rs_SIMP_THD2.LowerValue;
			locateParam.SIMP_THD2_Max = (int)rs_SIMP_THD2.UpperValue;
			locateParam.SIMP_THD3_Enabled = exp_THD3.IsExpanded;
			locateParam.SIMP_THD3_Channel = cb_SIMP_THD3_Channel.SelectedIndex;
			locateParam.SIMP_THD3_Min = (int)rs_SIMP_THD3.LowerValue;
			locateParam.SIMP_THD3_Max = (int)rs_SIMP_THD3.UpperValue;
			locateParam.SIMP_DilateErode1Start = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
			locateParam.SIMP_DilateErode1StartCount = cb_THD1_DEStartCount.SelectedIndex;
			locateParam.SIMP_DilateErode1End = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
			locateParam.SIMP_DilateErode1EndCount = cb_THD1_DEEndCount.SelectedIndex;
			locateParam.SIMP_DilateErode2Start = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
			locateParam.SIMP_DilateErode2StartCount = cb_THD2_DEStartCount.SelectedIndex;
			locateParam.SIMP_DilateErode2End = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
			locateParam.SIMP_DilateErode2EndCount = cb_THD2_DEEndCount.SelectedIndex;
			locateParam.SIMP_DilateErode3Start = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
			locateParam.SIMP_DilateErode3StartCount = cb_THD3_DEStartCount.SelectedIndex;
			locateParam.SIMP_DilateErode3End = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
			locateParam.SIMP_DilateErode3EndCount = cb_THD3_DEEndCount.SelectedIndex;
			if (LocateType == 1)
			{
				GetQuadSearchRect();
				locateParam.QuadSearchRectL = QuadSearchRect[0];
				locateParam.QuadSearchRectR = QuadSearchRect[1];
				locateParam.QuadSearchRectT = QuadSearchRect[2];
				locateParam.QuadSearchRectB = QuadSearchRect[3];
				if (locateParam.QuadSearchRectR.Left < locateParam.QuadSearchRectL.Left)
				{
					string text = "";
					text = ((LanguageType == 1) ? "請檢查檢測區域左右邊檢測框設定是否相反。" : ((LanguageType == 2) ? "请检查检测区域左右边设定是否相反。" : ((LanguageType != 3) ? "Please check Left and Right Tracker setting!" : "検出領域の左右の設定が逆になっている可能性があります。")));
					MessageBox.Show(text, "Alarm");
				}
				if (locateParam.QuadSearchRectB.Top < locateParam.QuadSearchRectT.Top)
				{
					string text2 = "";
					text2 = ((LanguageType == 1) ? "請檢查檢測區域上下邊檢測框設定是否相反。" : ((LanguageType == 2) ? "请检查检测区域上下边设定是否相反。" : ((LanguageType != 3) ? "Please check Top and Bottom Tracker setting!" : "検出領域の上下の設定が逆になっている可能性があります。")));
					MessageBox.Show(text2, "Alarm");
				}
				GetQuadSearchMask();
				locateParam.Quad_Mask_L = QuadSearchMask[0];
				locateParam.Quad_Mask_R = QuadSearchMask[1];
				locateParam.Quad_Mask_T = QuadSearchMask[2];
				locateParam.Quad_Mask_B = QuadSearchMask[3];
			}
			locateParam.QUAD_SideLocate = cb_QUAD_SideLocate.IsChecked.Value;
			locateParam.Quad_EnableAvgEdge_L = cb_EnableAvgEdgeL.IsChecked.Value;
			locateParam.Quad_EnableAvgEdge_R = cb_EnableAvgEdgeR.IsChecked.Value;
			locateParam.Quad_EnableAvgEdge_T = cb_EnableAvgEdgeT.IsChecked.Value;
			locateParam.Quad_EnableAvgEdge_B = cb_EnableAvgEdgeB.IsChecked.Value;
			locateParam.Quad_EnableMask_L = cb_EnableMaskL.IsChecked.Value;
			locateParam.Quad_EnableMask_R = cb_EnableMaskR.IsChecked.Value;
			locateParam.Quad_EnableMask_T = cb_EnableMaskT.IsChecked.Value;
			locateParam.Quad_EnableMask_B = cb_EnableMaskB.IsChecked.Value;
			locateParam.Quad_Channel_L = cb_QuadChannelL.SelectedIndex;
			locateParam.Quad_Channel_R = cb_QuadChannelR.SelectedIndex;
			locateParam.Quad_Channel_T = cb_QuadChannelT.SelectedIndex;
			locateParam.Quad_Channel_B = cb_QuadChannelB.SelectedIndex;
			locateParam.Quad_THD_Min_L = (int)rs_QuadTHDValueL.LowerValue;
			locateParam.Quad_THD_Max_L = (int)rs_QuadTHDValueL.UpperValue;
			locateParam.Quad_THD_Min_R = (int)rs_QuadTHDValueR.LowerValue;
			locateParam.Quad_THD_Max_R = (int)rs_QuadTHDValueR.UpperValue;
			locateParam.Quad_THD_Min_T = (int)rs_QuadTHDValueT.LowerValue;
			locateParam.Quad_THD_Max_T = (int)rs_QuadTHDValueT.UpperValue;
			locateParam.Quad_THD_Min_B = (int)rs_QuadTHDValueB.LowerValue;
			locateParam.Quad_THD_Max_B = (int)rs_QuadTHDValueB.UpperValue;
			locateParam.Quad_EnableRGBThd_L = cb_Quad_EnableRGBThd_L.IsChecked.Value;
			locateParam.Quad_THDR_Min_L = (int)rs_Quad_THDR_L.LowerValue;
			locateParam.Quad_THDR_Max_L = (int)rs_Quad_THDR_L.UpperValue;
			locateParam.Quad_THDG_Min_L = (int)rs_Quad_THDG_L.LowerValue;
			locateParam.Quad_THDG_Max_L = (int)rs_Quad_THDG_L.UpperValue;
			locateParam.Quad_THDB_Min_L = (int)rs_Quad_THDB_L.LowerValue;
			locateParam.Quad_THDB_Max_L = (int)rs_Quad_THDB_L.UpperValue;
			locateParam.Quad_EnableRGBThd_R = cb_Quad_EnableRGBThd_R.IsChecked.Value;
			locateParam.Quad_THDR_Min_R = (int)rs_Quad_THDR_R.LowerValue;
			locateParam.Quad_THDR_Max_R = (int)rs_Quad_THDR_R.UpperValue;
			locateParam.Quad_THDG_Min_R = (int)rs_Quad_THDG_R.LowerValue;
			locateParam.Quad_THDG_Max_R = (int)rs_Quad_THDG_R.UpperValue;
			locateParam.Quad_THDB_Min_R = (int)rs_Quad_THDB_R.LowerValue;
			locateParam.Quad_THDB_Max_R = (int)rs_Quad_THDB_R.UpperValue;
			locateParam.Quad_EnableRGBThd_T = cb_Quad_EnableRGBThd_T.IsChecked.Value;
			locateParam.Quad_THDR_Min_T = (int)rs_Quad_THDR_T.LowerValue;
			locateParam.Quad_THDR_Max_T = (int)rs_Quad_THDR_T.UpperValue;
			locateParam.Quad_THDG_Min_T = (int)rs_Quad_THDG_T.LowerValue;
			locateParam.Quad_THDG_Max_T = (int)rs_Quad_THDG_T.UpperValue;
			locateParam.Quad_THDB_Min_T = (int)rs_Quad_THDB_T.LowerValue;
			locateParam.Quad_THDB_Max_T = (int)rs_Quad_THDB_T.UpperValue;
			locateParam.Quad_EnableRGBThd_B = cb_Quad_EnableRGBThd_B.IsChecked.Value;
			locateParam.Quad_THDR_Min_B = (int)rs_Quad_THDR_B.LowerValue;
			locateParam.Quad_THDR_Max_B = (int)rs_Quad_THDR_B.UpperValue;
			locateParam.Quad_THDG_Min_B = (int)rs_Quad_THDG_B.LowerValue;
			locateParam.Quad_THDG_Max_B = (int)rs_Quad_THDG_B.UpperValue;
			locateParam.Quad_THDB_Min_B = (int)rs_Quad_THDB_B.LowerValue;
			locateParam.Quad_THDB_Max_B = (int)rs_Quad_THDB_B.UpperValue;
			locateParam.Quad_DEStart_L = (USE_DILATE_ERODE)cb_Quad_DEStartL.SelectedIndex;
			locateParam.Quad_DEStart_R = (USE_DILATE_ERODE)cb_Quad_DEStartR.SelectedIndex;
			locateParam.Quad_DEStart_T = (USE_DILATE_ERODE)cb_Quad_DEStartT.SelectedIndex;
			locateParam.Quad_DEStart_B = (USE_DILATE_ERODE)cb_Quad_DEStartB.SelectedIndex;
			locateParam.Quad_DEEnd_L = (USE_DILATE_ERODE)cb_Quad_DEEndL.SelectedIndex;
			locateParam.Quad_DEEnd_R = (USE_DILATE_ERODE)cb_Quad_DEEndR.SelectedIndex;
			locateParam.Quad_DEEnd_T = (USE_DILATE_ERODE)cb_Quad_DEEndT.SelectedIndex;
			locateParam.Quad_DEEnd_B = (USE_DILATE_ERODE)cb_Quad_DEEndB.SelectedIndex;
			locateParam.Quad_DEStartCount_L = cb_Quad_DEStartCountL.SelectedIndex;
			locateParam.Quad_DEStartCount_R = cb_Quad_DEStartCountR.SelectedIndex;
			locateParam.Quad_DEStartCount_T = cb_Quad_DEStartCountT.SelectedIndex;
			locateParam.Quad_DEStartCount_B = cb_Quad_DEStartCountB.SelectedIndex;
			locateParam.Quad_DEEndCount_L = cb_Quad_DEEndCountL.SelectedIndex;
			locateParam.Quad_DEEndCount_R = cb_Quad_DEEndCountR.SelectedIndex;
			locateParam.Quad_DEEndCount_T = cb_Quad_DEEndCountT.SelectedIndex;
			locateParam.Quad_DEEndCount_B = cb_Quad_DEEndCountB.SelectedIndex;
			locateParam.Quad_FilterPoint = chk_UseFilterPoint.IsChecked.Value;
			locateParam.Quad_UseStdFilter = chk_UseStdFilter.IsChecked.Value;
			locateParam.Quad_UseSideLocate = chk_UseSideLocate.IsChecked.Value;
			locateParam.Quad_FixSideLocateLR = chk_FixSideLocateLR.IsChecked.Value;
			locateParam.Quad_LocateAngleSelect = cb_Quad_LocateAngleSelect.SelectedIndex;
			if (QuadScanDir1[0].IsChecked == true)
			{
				locateParam.Direction_L = ScanDirection.EM_LEFT_RIGHT;
			}
			else
			{
				locateParam.Direction_L = ScanDirection.EM_RIGHT_LEFT;
			}
			if (QuadScanDir1[1].IsChecked == true)
			{
				locateParam.Direction_R = ScanDirection.EM_LEFT_RIGHT;
			}
			else
			{
				locateParam.Direction_R = ScanDirection.EM_RIGHT_LEFT;
			}
			if (QuadScanDir1[2].IsChecked == true)
			{
				locateParam.Direction_T = ScanDirection.EM_TOP_BOTTOM;
			}
			else
			{
				locateParam.Direction_T = ScanDirection.EM_BOTTOM_TOP;
			}
			if (QuadScanDir1[3].IsChecked == true)
			{
				locateParam.Direction_B = ScanDirection.EM_TOP_BOTTOM;
			}
			else
			{
				locateParam.Direction_B = ScanDirection.EM_BOTTOM_TOP;
			}
			if (chk_UseMelfLocate.IsChecked == true)
			{
				locateParam.UseMelfMode = true;
			}
			else
			{
				locateParam.UseMelfMode = false;
			}
			if (LocateType == 2)
			{
				GetPadSearchRect();
				locateParam.PadSearchRectL = PadSearchRect[0];
				locateParam.PadSearchRectR = PadSearchRect[1];
			}
			locateParam.Pad_Channel_L1 = cb_PadChannelL1.SelectedIndex;
			locateParam.Pad_Channel_R1 = cb_PadChannelR1.SelectedIndex;
			locateParam.Pad_Channel_L2 = cb_PadChannelL2.SelectedIndex;
			locateParam.Pad_Channel_R2 = cb_PadChannelR2.SelectedIndex;
			locateParam.Pad_THD_Min_L1 = (int)rs_PadTHDValueL1.LowerValue;
			locateParam.Pad_THD_Max_L1 = (int)rs_PadTHDValueL1.UpperValue;
			locateParam.Pad_THD_Min_R1 = (int)rs_PadTHDValueR1.LowerValue;
			locateParam.Pad_THD_Max_R1 = (int)rs_PadTHDValueR1.UpperValue;
			locateParam.Pad_THD_Min_L2 = (int)rs_PadTHDValueL2.LowerValue;
			locateParam.Pad_THD_Max_L2 = (int)rs_PadTHDValueL2.UpperValue;
			locateParam.Pad_THD_Min_R2 = (int)rs_PadTHDValueR2.LowerValue;
			locateParam.Pad_THD_Max_R2 = (int)rs_PadTHDValueR2.UpperValue;
			locateParam.Pad_DEStart_L1 = (USE_DILATE_ERODE)cb_Pad_DEStartL1.SelectedIndex;
			locateParam.Pad_DEStart_R1 = (USE_DILATE_ERODE)cb_Pad_DEStartR1.SelectedIndex;
			locateParam.Pad_DEStart_L2 = (USE_DILATE_ERODE)cb_Pad_DEStartL2.SelectedIndex;
			locateParam.Pad_DEStart_R2 = (USE_DILATE_ERODE)cb_Pad_DEStartR2.SelectedIndex;
			locateParam.Pad_DEEnd_L1 = (USE_DILATE_ERODE)cb_Pad_DEEndL1.SelectedIndex;
			locateParam.Pad_DEEnd_R1 = (USE_DILATE_ERODE)cb_Pad_DEEndR1.SelectedIndex;
			locateParam.Pad_DEEnd_L2 = (USE_DILATE_ERODE)cb_Pad_DEEndL2.SelectedIndex;
			locateParam.Pad_DEEnd_R2 = (USE_DILATE_ERODE)cb_Pad_DEEndR2.SelectedIndex;
			locateParam.Pad_DEStartCount_L1 = cb_Pad_DEStartCountL1.SelectedIndex;
			locateParam.Pad_DEStartCount_R1 = cb_Pad_DEStartCountR1.SelectedIndex;
			locateParam.Pad_DEStartCount_L2 = cb_Pad_DEStartCountL2.SelectedIndex;
			locateParam.Pad_DEStartCount_R2 = cb_Pad_DEStartCountR2.SelectedIndex;
			locateParam.Pad_DEEndCount_L1 = cb_Pad_DEEndCountL1.SelectedIndex;
			locateParam.Pad_DEEndCount_R1 = cb_Pad_DEEndCountR1.SelectedIndex;
			locateParam.Pad_DEEndCount_L2 = cb_Pad_DEEndCountL2.SelectedIndex;
			locateParam.Pad_DEEndCount_R2 = cb_Pad_DEEndCountL2.SelectedIndex;
			inspection.paramFile.LocateParam = locateParam;
			OpenCvSharp.Rect maxrect = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			inspection.locateTool.ReCalcMaxSearchRect(locateParam.LocateType, maxrect);
			SetAILocateRect(locateParam.MaxSearchRect);
			return true;
		}
		catch
		{
			string text3 = "LW04-LocateWindow-WriteUIValueToParam()";
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text3, "Error");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode)
		{
			return;
		}
		try
		{
			if (!CheckTextBox())
			{
				txt_AllResult.Clear();
				txt_AllResult.Text = LanguageManager.Instance["InputError"];
				return;
			}
			WriteUIValueToParam();
			bool flag = true;
			inspection.locateTool.LocateExtendRect = new OpenCvSharp.Rect(0, 0, 1, 1);
			flag = inspection.DoLocate();
			Mat mat = inspection.buffers.Dst.Clone();
			if (chk_EnableFocusCheck.IsChecked == true)
			{
				txt_FocusLimitResult.Text = inspection.locateTool.FocusScore.ToString();
				try
				{
					if (inspection.locateTool.FocusResult)
					{
						txt_FocusLimit.Background = Brushes.White;
					}
					else
					{
						txt_FocusLimit.Background = Brushes.Fuchsia;
					}
				}
				catch
				{
				}
			}
			double locateAngle = inspection.locateTool.LocateAngle;
			OpenCvSharp.Rect locateRect = inspection.locateTool.LocateRect;
			OpenCvSharp.Rect locateImageRect = inspection.locateTool.LocateImageRect;
			locateRect.X += locateImageRect.X;
			locateRect.Y += locateImageRect.Y;
			string axis = string.Format("({0}, {1}, {2}, {3})", locateRect.X, locateRect.Y, locateRect.X + locateRect.Width, locateRect.Y + locateRect.Height);
			SetLocateRect(locateRect, axis);
			axis = string.Format("{0:0.0}", locateAngle);
			txt_RotatAngleResult.Text = axis;
			txt_RotatWidthResult.Text = string.Format("{0:N3}", ((double)locateRect.Width * inspection.paramFile.PixelPerUM_X).ToString());
			txt_RotatHeightResult.Text = string.Format("{0:N3}", ((double)locateRect.Height * inspection.paramFile.PixelPerUM_Y).ToString());
			double num = (double)locateRect.Width * inspection.paramFile.PixelPerUM_X;
			double num2 = (double)locateRect.Height * inspection.paramFile.PixelPerUM_Y;
			double num3 = ConvertComDigits(txt_RotatWidthMin, ForSave: false);
			double num4 = ConvertComDigits(txt_RotatWidthMax, ForSave: false);
			double num5 = ConvertComDigits(txt_RotatHeightMin, ForSave: false);
			double num6 = ConvertComDigits(txt_RotatHeightMax, ForSave: false);
			if (num < num3)
			{
				txt_RotatWidthMin.Background = Brushes.Fuchsia;
			}
			if (num > num4)
			{
				txt_RotatWidthMax.Background = Brushes.Fuchsia;
			}
			if (num2 < num5)
			{
				txt_RotatHeightMin.Background = Brushes.Fuchsia;
			}
			if (num2 > num6)
			{
				txt_RotatHeightMax.Background = Brushes.Fuchsia;
			}
			if (Math.Abs(locateAngle) > Convert.ToDouble(txt_RotatLimitAngle.Text))
			{
				txt_RotatLimitAngle.Background = Brushes.Fuchsia;
			}
			else
			{
				txt_RotatLimitAngle.Background = Brushes.White;
			}
			SetResult(flag);
			txt_AllResult.Clear();
			txt_AllResult.Text = inspection.locateTool.ErrorMsg;
			if (tab_LocateType.SelectedIndex == 1)
			{
				if (!inspection.locateTool.QuadScanStatus[0])
				{
					grd_QUAD_THD_L.Background = Brushes.MediumVioletRed;
				}
				else
				{
					grd_QUAD_THD_L.Background = Brushes.White;
				}
				if (!inspection.locateTool.QuadScanStatus[1])
				{
					grd_QUAD_THD_R.Background = Brushes.MediumVioletRed;
				}
				else
				{
					grd_QUAD_THD_R.Background = Brushes.White;
				}
				if (!inspection.locateTool.QuadScanStatus[2])
				{
					grd_QUAD_THD_T.Background = Brushes.MediumVioletRed;
				}
				else
				{
					grd_QUAD_THD_T.Background = Brushes.White;
				}
				if (!inspection.locateTool.QuadScanStatus[3])
				{
					grd_QUAD_THD_B.Background = Brushes.MediumVioletRed;
				}
				else
				{
					grd_QUAD_THD_B.Background = Brushes.White;
				}
			}
			Mat image;
			switch (PreviewIndex)
			{
			case "0":
				image = mat;
				break;
			case "1":
				image = inspection.buffers.GetSrcBGR(0);
				AutoLocateChID = 0;
				break;
			case "2":
				image = inspection.buffers.GetSrcBGR(1);
				AutoLocateChID = 1;
				break;
			case "3":
				image = inspection.buffers.GetSrcBGR(2);
				AutoLocateChID = 2;
				break;
			case "4":
				image = inspection.buffers.GetChannelMat(18);
				break;
			case "5":
				image = inspection.buffers.GetChannelMat(19);
				break;
			case "6":
				image = inspection.buffers.GetChannelMat(20);
				break;
			case "7":
				image = DrawAIOutside(inspection.buffers.Src, inspection.paramFile.LocateParam.MaxSearchRect);
				break;
			default:
				image = (image = inspection.buffers.Src);
				AutoLocateChID = 3;
				break;
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(image);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("LocateTool-Type{0}", tab_LocateType.SelectedIndex) + "LT12-LocateWindow-Btn_test_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetComDigitsStr(double value, TextBox tbox = null)
	{
		if (value > -98980.0 || value < -99004.0 || ThroughputFile == null)
		{
			if (tbox != null)
			{
				tbox.Background = Brushes.White;
				tbox.ToolTip = "";
				tbox.IsReadOnly = false;
			}
			return value.ToString();
		}
		try
		{
			int num = (int)Math.Abs(-98980.0 - value);
			string result = string.Format("{0}:{1}", num + 1, ThroughputFile.ComDigitsName[num]);
			if (tbox != null)
			{
				tbox.Background = Brushes.GreenYellow;
				tbox.ToolTip = string.Format("{0} = {1}", ThroughputFile.ComDigitsName[num], ThroughputFile.ComDigits[num]);
				tbox.IsReadOnly = true;
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("LocateTool-Type{0}", tab_LocateType.SelectedIndex) + "LT12-LocateWindow-GetComDigitsStr()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return "0";
		}
	}

	private void Txt_ComDigits_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetComDigitsKeyboard(tbox, position);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private double ConvertComDigits(TextBox txt, bool ForSave = true)
	{
		double num = 0.0;
		try
		{
			if (txt.Text.Contains(":"))
			{
				int length = txt.Text.IndexOf(":");
				int num2 = Convert.ToInt32(txt.Text.Substring(0, length)) - 1;
				num = ((!ForSave) ? ThroughputFile.ComDigits[num2] : (-98980.0 + (double)(num2 * -1)));
				txt.Background = Brushes.GreenYellow;
				return num;
			}
			if (!IsNumeric(txt.Text))
			{
				txt.Background = Brushes.Red;
				return -1.0;
			}
			txt.Background = Brushes.White;
			return Convert.ToDouble(txt.Text);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("LocateTool-Type{0}", tab_LocateType.SelectedIndex) + "LT12-LocateWindow-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return -1.0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetComDigitsKeyboard(TextBox tbox, System.Windows.Point p)
	{
		try
		{
			HKeyboard hKeyboard = new HKeyboard();
			if (ThroughputFile != null)
			{
				hKeyboard.SetComDigits(ThroughputFile);
			}
			hKeyboard.setTxtbox(tbox);
			hKeyboard.SetInput(tbox.Text);
			hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
			hKeyboard.Left = p.X;
			hKeyboard.Top = p.Y - hKeyboard.Height + MainW.Top;
			tbox.Background = Brushes.Gold;
			hKeyboard.ShowDialog();
			tbox.Background = Brushes.White;
			tbox.ToolTip = "";
			tbox.IsReadOnly = false;
			tbox.Text = hKeyboard.GetInput();
			if (hKeyboard.UseCommonDigits && hKeyboard.CommonDigitsIndex >= 0)
			{
				tbox.Text = string.Format("{0}:{1}", hKeyboard.CommonDigitsIndex + 1, ThroughputFile.ComDigitsName[hKeyboard.CommonDigitsIndex]);
				tbox.Background = Brushes.GreenYellow;
				tbox.ToolTip = string.Format("{0} = {1}", ThroughputFile.ComDigitsName[hKeyboard.CommonDigitsIndex], ThroughputFile.ComDigits[hKeyboard.CommonDigitsIndex]);
				tbox.IsReadOnly = true;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("LocateTool-Type{0}", tab_LocateType.SelectedIndex) + "LT13-LocateWindow-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Cb_EnableMaskL_Click(object sender, RoutedEventArgs e)
	{
		if (cb_EnableMaskL.IsChecked == true)
		{
			bor_SearchMaskL.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchMaskL.Visibility = Visibility.Hidden;
		}
	}

	private void Cb_EnableMaskR_Click(object sender, RoutedEventArgs e)
	{
		if (cb_EnableMaskR.IsChecked == true)
		{
			bor_SearchMaskR.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchMaskR.Visibility = Visibility.Hidden;
		}
	}

	private void Cb_EnableMaskT_Click(object sender, RoutedEventArgs e)
	{
		if (cb_EnableMaskT.IsChecked == true)
		{
			bor_SearchMaskT.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchMaskT.Visibility = Visibility.Hidden;
		}
	}

	private void Cb_EnableMaskB_Click(object sender, RoutedEventArgs e)
	{
		if (cb_EnableMaskB.IsChecked == true)
		{
			bor_SearchMaskB.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchMaskB.Visibility = Visibility.Hidden;
		}
	}

	private void Rs_Quad_THDR_L_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDR_Min[i].Content = ((int)QuadThdRValue[i].LowerValue).ToString();
				Quad_THDR_Max[i].Content = ((int)QuadThdRValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	private void Rs_Quad_THDR_L_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				QuadThdRValue[i].LowerValue = QuadThdRValue[num].LowerValue;
				QuadThdRValue[i].UpperValue = QuadThdRValue[num].UpperValue;
				Quad_THDR_Min[i].Content = QuadThdRValue[num].LowerValue;
				Quad_THDR_Max[i].Content = QuadThdRValue[num].UpperValue;
				QuadThdGValue[i].LowerValue = QuadThdGValue[num].LowerValue;
				QuadThdGValue[i].UpperValue = QuadThdGValue[num].UpperValue;
				Quad_THDG_Min[i].Content = QuadThdGValue[num].LowerValue;
				Quad_THDG_Max[i].Content = QuadThdGValue[num].UpperValue;
				QuadThdBValue[i].LowerValue = QuadThdBValue[num].LowerValue;
				QuadThdBValue[i].UpperValue = QuadThdBValue[num].UpperValue;
				Quad_THDB_Min[i].Content = QuadThdBValue[num].LowerValue;
				Quad_THDB_Max[i].Content = QuadThdBValue[num].UpperValue;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[i] = false;
			}
		}
	}

	private void Rs_Quad_THDG_L_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDG_Min[i].Content = ((int)QuadThdGValue[i].LowerValue).ToString();
				Quad_THDG_Max[i].Content = ((int)QuadThdGValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	private void Rs_Quad_THDB_L_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDB_Min[i].Content = ((int)QuadThdBValue[i].LowerValue).ToString();
				Quad_THDB_Max[i].Content = ((int)QuadThdBValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	private void Rs_Quad_THDG_L_LowerThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDG_Min[i].Content = ((int)QuadThdGValue[i].LowerValue).ToString();
				Quad_THDG_Max[i].Content = ((int)QuadThdGValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	private void Rs_Quad_THDG_L_UpperThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDG_Min[i].Content = ((int)QuadThdGValue[i].LowerValue).ToString();
				Quad_THDG_Max[i].Content = ((int)QuadThdGValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	private void Rs_Quad_THDG_L_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				QuadThdRValue[i].LowerValue = QuadThdRValue[num].LowerValue;
				QuadThdRValue[i].UpperValue = QuadThdRValue[num].UpperValue;
				Quad_THDR_Min[i].Content = QuadThdRValue[num].LowerValue;
				Quad_THDR_Max[i].Content = QuadThdRValue[num].UpperValue;
				QuadThdGValue[i].LowerValue = QuadThdGValue[num].LowerValue;
				QuadThdGValue[i].UpperValue = QuadThdGValue[num].UpperValue;
				Quad_THDG_Min[i].Content = QuadThdGValue[num].LowerValue;
				Quad_THDG_Max[i].Content = QuadThdGValue[num].UpperValue;
				QuadThdBValue[i].LowerValue = QuadThdBValue[num].LowerValue;
				QuadThdBValue[i].UpperValue = QuadThdBValue[num].UpperValue;
				Quad_THDB_Min[i].Content = QuadThdBValue[num].LowerValue;
				Quad_THDB_Max[i].Content = QuadThdBValue[num].UpperValue;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[i] = false;
			}
		}
	}

	private void Rs_Quad_THDB_L_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				QuadThdRValue[i].LowerValue = QuadThdRValue[num].LowerValue;
				QuadThdRValue[i].UpperValue = QuadThdRValue[num].UpperValue;
				Quad_THDR_Min[i].Content = QuadThdRValue[num].LowerValue;
				Quad_THDR_Max[i].Content = QuadThdRValue[num].UpperValue;
				QuadThdGValue[i].LowerValue = QuadThdGValue[num].LowerValue;
				QuadThdGValue[i].UpperValue = QuadThdGValue[num].UpperValue;
				Quad_THDG_Min[i].Content = QuadThdGValue[num].LowerValue;
				Quad_THDG_Max[i].Content = QuadThdGValue[num].UpperValue;
				QuadThdBValue[i].LowerValue = QuadThdBValue[num].LowerValue;
				QuadThdBValue[i].UpperValue = QuadThdBValue[num].UpperValue;
				Quad_THDB_Min[i].Content = QuadThdBValue[num].LowerValue;
				Quad_THDB_Max[i].Content = QuadThdBValue[num].UpperValue;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[i] = false;
			}
		}
	}

	private void Cb_Quad_EnableRGBThd_L_Click(object sender, RoutedEventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		if (cb_Quad_EnableRGBThd_L.IsChecked == true)
		{
			grd_Quad_RGBThd_L.Visibility = Visibility.Visible;
			cb_QuadChannelL.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Quad_RGBThd_L.Visibility = Visibility.Hidden;
			cb_QuadChannelL.Visibility = Visibility.Visible;
		}
		doQUADThdImage(Convert.ToInt32(checkBox.Uid));
	}

	private void Cb_Quad_EnableRGBThd_R_Click(object sender, RoutedEventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		if (cb_Quad_EnableRGBThd_R.IsChecked == true)
		{
			grd_Quad_RGBThd_R.Visibility = Visibility.Visible;
			cb_QuadChannelR.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Quad_RGBThd_R.Visibility = Visibility.Hidden;
			cb_QuadChannelR.Visibility = Visibility.Visible;
		}
		doQUADThdImage(Convert.ToInt32(checkBox.Uid));
	}

	private void Cb_Quad_EnableRGBThd_T_Click(object sender, RoutedEventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		if (cb_Quad_EnableRGBThd_T.IsChecked == true)
		{
			grd_Quad_RGBThd_T.Visibility = Visibility.Visible;
			cb_QuadChannelT.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Quad_RGBThd_T.Visibility = Visibility.Hidden;
			cb_QuadChannelT.Visibility = Visibility.Visible;
		}
		doQUADThdImage(Convert.ToInt32(checkBox.Uid));
	}

	private void Cb_Quad_EnableRGBThd_B_Click(object sender, RoutedEventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		if (cb_Quad_EnableRGBThd_B.IsChecked == true)
		{
			grd_Quad_RGBThd_B.Visibility = Visibility.Visible;
			cb_QuadChannelB.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Quad_RGBThd_B.Visibility = Visibility.Hidden;
			cb_QuadChannelB.Visibility = Visibility.Visible;
		}
		doQUADThdImage(Convert.ToInt32(checkBox.Uid));
	}

	private void Cb_Quad_EnableRGBThd_L_Click_1(object sender, RoutedEventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		if (cb_Quad_EnableRGBThd_L.IsChecked == true)
		{
			grd_Quad_RGBThd_L.Visibility = Visibility.Visible;
			cb_QuadChannelL.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Quad_RGBThd_L.Visibility = Visibility.Hidden;
			cb_QuadChannelL.Visibility = Visibility.Visible;
		}
		doQUADThdImage(Convert.ToInt32(checkBox.Uid));
	}

	private void Rs_Quad_THDR_R_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (inited)
		{
			_ = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDR_Min[i].Content = ((int)QuadThdRValue[i].LowerValue).ToString();
				Quad_THDR_Max[i].Content = ((int)QuadThdRValue[i].UpperValue).ToString();
			}
		}
	}

	private void Rs_Quad_THDR_L_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (inited)
		{
			RangeSlider rangeSlider = (RangeSlider)sender;
			for (int i = 0; i < 4; i++)
			{
				Quad_THDR_Min[i].Content = ((int)QuadThdRValue[i].LowerValue).ToString();
				Quad_THDR_Max[i].Content = ((int)QuadThdRValue[i].UpperValue).ToString();
			}
			doQUADThdImage(Convert.ToInt32(rangeSlider.Uid));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OTSUL_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			GetQuadSearchRect();
			if (Quad_EnableRGBThd[num].IsChecked == false)
			{
				int selectedIndex = QuadChannel[num].SelectedIndex;
				using Mat mat = new Mat(inspection.buffers.GetSrcBGR(selectedIndex).Clone(), QuadSearchRect[num]);
				using Mat mat2 = mat.Clone();
				double num2 = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
				if (OTSU_White[num])
				{
					QuadThdValue[num].LowerValue = num2;
					QuadThdValue[num].UpperValue = 255.0;
				}
				else
				{
					QuadThdValue[num].LowerValue = 0.0;
					QuadThdValue[num].UpperValue = num2;
				}
				QuadThdMin[num].Text = ((int)QuadThdValue[num].LowerValue).ToString();
				QuadThdMax[num].Text = ((int)QuadThdValue[num].UpperValue).ToString();
			}
			else
			{
				using Mat mat3 = new Mat(inspection.buffers.GetSrcBGR(0).Clone(), QuadSearchRect[num]);
				using Mat mat4 = new Mat(inspection.buffers.GetSrcBGR(1).Clone(), QuadSearchRect[num]);
				using Mat mat5 = new Mat(inspection.buffers.GetSrcBGR(2).Clone(), QuadSearchRect[num]);
				using Mat mat6 = mat3.Clone();
				using Mat mat7 = mat4.Clone();
				using Mat mat8 = mat5.Clone();
				double num3 = Cv2.Threshold(mat3, mat6, 0.0, 255.0, ThresholdTypes.Otsu);
				double num4 = Cv2.Threshold(mat4, mat7, 0.0, 255.0, ThresholdTypes.Otsu);
				double num5 = Cv2.Threshold(mat5, mat8, 0.0, 255.0, ThresholdTypes.Otsu);
				if (OTSU_White[num])
				{
					QuadThdRValue[num].LowerValue = 0.0;
					QuadThdRValue[num].UpperValue = num3;
					QuadThdGValue[num].LowerValue = 0.0;
					QuadThdGValue[num].UpperValue = num4;
					QuadThdBValue[num].LowerValue = 0.0;
					QuadThdBValue[num].UpperValue = num5;
				}
				else
				{
					QuadThdRValue[num].LowerValue = num3;
					QuadThdRValue[num].UpperValue = 255.0;
					QuadThdGValue[num].LowerValue = num4;
					QuadThdGValue[num].UpperValue = 255.0;
					QuadThdBValue[num].LowerValue = num5;
					QuadThdBValue[num].UpperValue = 255.0;
				}
				Quad_THDR_Min[num].Content = ((int)QuadThdRValue[num].LowerValue).ToString();
				Quad_THDR_Max[num].Content = ((int)QuadThdRValue[num].UpperValue).ToString();
				Quad_THDG_Min[num].Content = ((int)QuadThdGValue[num].LowerValue).ToString();
				Quad_THDG_Max[num].Content = ((int)QuadThdGValue[num].UpperValue).ToString();
				Quad_THDB_Min[num].Content = ((int)QuadThdBValue[num].LowerValue).ToString();
				Quad_THDB_Max[num].Content = ((int)QuadThdBValue[num].UpperValue).ToString();
			}
			OTSU_White[num] = !OTSU_White[num];
			doQUADThdImage(num);
		}
		catch (Exception ex)
		{
			string text = "BW10-LocateWindow-Btn_OTSUL_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OTSU_Easy_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = cb_SIMP_THD1_Channel.SelectedIndex;
			SIMPSearchRect = GetSIMPSearchRect();
			Mat mat = new Mat(inspection.buffers.GetSrcBGR(selectedIndex), SIMPSearchRect);
			double num = Cv2.Threshold(dst: mat.Clone(), src: mat, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
			if (OTSU_Easy)
			{
				rs_SIMP_THD1.LowerValue = num;
				rs_SIMP_THD1.UpperValue = 255.0;
			}
			else
			{
				rs_SIMP_THD1.LowerValue = 0.0;
				rs_SIMP_THD1.UpperValue = num;
			}
			nu_Thd1LimitMin.Value = rs_SIMP_THD1.LowerValue;
			nu_Thd1LimitMax.Value = rs_SIMP_THD1.UpperValue;
			if (exp_THD2.IsExpanded)
			{
				selectedIndex = cb_SIMP_THD2_Channel.SelectedIndex;
				Mat mat2 = new Mat(inspection.buffers.GetSrcBGR(selectedIndex), SIMPSearchRect);
				num = Cv2.Threshold(dst: mat2.Clone(), src: mat2, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
				if (OTSU_Easy)
				{
					rs_SIMP_THD2.LowerValue = num;
					rs_SIMP_THD2.UpperValue = 255.0;
				}
				else
				{
					rs_SIMP_THD2.LowerValue = 0.0;
					rs_SIMP_THD2.UpperValue = num;
				}
				nu_Thd2LimitMin.Value = rs_SIMP_THD2.LowerValue;
				nu_Thd2LimitMax.Value = rs_SIMP_THD2.UpperValue;
			}
			if (exp_THD3.IsExpanded)
			{
				selectedIndex = cb_SIMP_THD3_Channel.SelectedIndex;
				Mat mat3 = new Mat(inspection.buffers.GetSrcBGR(selectedIndex), SIMPSearchRect);
				num = Cv2.Threshold(dst: mat3.Clone(), src: mat3, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
				if (OTSU_Easy)
				{
					rs_SIMP_THD3.LowerValue = num;
					rs_SIMP_THD3.UpperValue = 255.0;
				}
				else
				{
					rs_SIMP_THD3.LowerValue = 0.0;
					rs_SIMP_THD3.UpperValue = num;
				}
				nu_Thd3LimitMin.Value = rs_SIMP_THD3.LowerValue;
				nu_Thd3LimitMax.Value = rs_SIMP_THD3.UpperValue;
			}
			OTSU_Easy = !OTSU_Easy;
			doThdImage();
		}
		catch (Exception ex)
		{
			string text = "BW10-LocateWindow-Btn_OTSU_Easy_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ChartExport_Click(object sender, RoutedEventArgs e)
	{
		if (inspection.SizeStatics.Count <= 0)
		{
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Csv files (*.Csv;)|*.csv;|All files (*.*)|*.*";
		if (saveFileDialog.ShowDialog() == true)
		{
			if (!Directory.Exists(System.IO.Path.GetDirectoryName(saveFileDialog.FileName)))
			{
				MessageBox.Show(LanguageManager.Instance["Error"], LanguageManager.Instance["Error"]);
			}
			else
			{
				SaveCSV(saveFileDialog.FileName);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SaveCSV(string FileName)
	{
		using StreamWriter streamWriter = new StreamWriter(FileName, append: true);
		streamWriter.Write("定位點左邊X \n");
		for (int i = 0; i < inspection.SizeStatics.Count; i++)
		{
			string value = string.Format("{0},", (int)((double)inspection.SizeStatics[i].LocateCenterX * inspection.paramFile.PixelPerUM_X));
			streamWriter.Write(value);
		}
		streamWriter.WriteLine();
		streamWriter.Write("\n定位點中心Y \n");
		for (int j = 0; j < inspection.SizeStatics.Count; j++)
		{
			string value = string.Format("{0},", (int)((double)inspection.SizeStatics[j].LocateCenterY * inspection.paramFile.PixelPerUM_Y));
			streamWriter.Write(value);
		}
		streamWriter.WriteLine();
	}

	private void Btn_AutoLocate_Click(object sender, RoutedEventArgs e)
	{
		if (AutoLocateChID == 3)
		{
			lab_AutoLocateHint.Visibility = Visibility.Visible;
			rt_AutoLocateHint.Visibility = Visibility.Visible;
			return;
		}
		lab_AutoLocateHint.Visibility = Visibility.Hidden;
		rt_AutoLocateHint.Visibility = Visibility.Hidden;
		Mat mat = new Mat();
		bool flag = true;
		mat = inspection.buffers.GetSrcBGR(AutoLocateChID).Clone();
		if (new Mat(mat, new OpenCvSharp.Rect(0, 0, mat.Width / 7, mat.Height)).Mean().Val0 > 128.0)
		{
			flag = false;
		}
		Mat mat2 = new Mat();
		double num = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
		if (flag)
		{
			Cv2.InRange(mat, num + 15.0, 255.0, mat2);
		}
		else
		{
			Cv2.InRange(mat, 0.0, num - 15.0, mat2);
		}
		if (LocateType == 1)
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
			OpenCvSharp.Rect rect2 = FindMaxBlob(mat2);
			OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(rect2.Left + rect2.Width / 6, rect2.Top / 2, rect2.Width / 6 * 4, rect2.Top / 2 + rect2.Height / 2);
			OpenCvSharp.Rect rect4 = new OpenCvSharp.Rect(rect2.Left + rect2.Width / 6, rect2.Bottom - rect2.Height / 2, rect2.Width / 6 * 4, (rect.Bottom - rect2.Bottom) / 2 + rect2.Height / 2);
			OpenCvSharp.Rect rect5 = new OpenCvSharp.Rect(rect2.Left / 2, rect2.Top + rect2.Height / 6, rect2.Left / 2 + rect2.Width / 3, rect2.Height / 6 * 4);
			OpenCvSharp.Rect rect6 = new OpenCvSharp.Rect(rect2.Right - rect2.Width / 3, rect2.Top + rect2.Height / 6, (rect.Right - rect2.Right) / 2 + rect2.Width / 3, rect2.Height / 6 * 4);
			Cv2.CvtColor(mat2, mat2, ColorConversionCodes.GRAY2BGR);
			mat2.Rectangle(rect2, Scalar.Red);
			mat2.Rectangle(rect3, Scalar.Blue);
			mat2.Rectangle(rect4, Scalar.Green);
			mat2.Rectangle(rect5, Scalar.Yellow);
			mat2.Rectangle(rect6, Scalar.Fuchsia);
			QuadSearchRect[0] = rect5;
			QuadSearchRect[1] = rect6;
			QuadSearchRect[2] = rect3;
			QuadSearchRect[3] = rect4;
			SetQuadSearchRect();
			chk_UseSideLocate.IsChecked = false;
			chk_FixSideLocateLR.IsChecked = false;
			chk_UseStdFilter.IsChecked = true;
			chk_UseFilterPoint.IsChecked = false;
			chk_UseMelfLocate.IsChecked = false;
			cb_Quad_LocateAngleSelect.SelectedIndex = 0;
			chk_CreateHSVChannel.IsChecked = true;
			bor_SearchMaskL.Visibility = Visibility.Hidden;
			bor_SearchMaskR.Visibility = Visibility.Hidden;
			bor_SearchMaskT.Visibility = Visibility.Hidden;
			bor_SearchMaskB.Visibility = Visibility.Hidden;
			for (int i = 0; i < 4; i++)
			{
				QuadChannel[i].SelectedIndex = AutoLocateChID;
				QuadDEStartCount[i].SelectedIndex = 0;
				QuadDEEndCount[i].SelectedIndex = 0;
				using (Mat mat3 = new Mat(mat, QuadSearchRect[0]))
				{
					using Mat mat4 = mat3.Clone();
					double num2 = Cv2.Threshold(mat3, mat4, 0.0, 255.0, ThresholdTypes.Otsu);
					if (flag)
					{
						QuadThdValue[i].LowerValue = num2;
						QuadThdValue[i].UpperValue = 255.0;
					}
					else
					{
						QuadThdValue[i].LowerValue = 0.0;
						QuadThdValue[i].UpperValue = num2;
					}
					QuadThdMin[i].Text = ((int)QuadThdValue[i].LowerValue).ToString();
					QuadThdMax[i].Text = ((int)QuadThdValue[i].UpperValue).ToString();
				}
				switch (i)
				{
				case 0:
					rad_QuadScanL2R_L.IsChecked = true;
					cb_EnableMaskL.IsChecked = false;
					cb_Quad_EnableRGBThd_L.IsChecked = false;
					break;
				case 1:
					rad_QuadScanR2L_R.IsChecked = true;
					cb_EnableMaskR.IsChecked = false;
					cb_Quad_EnableRGBThd_R.IsChecked = false;
					break;
				case 2:
					rad_QuadScanT2B_T.IsChecked = true;
					cb_EnableMaskT.IsChecked = false;
					cb_Quad_EnableRGBThd_T.IsChecked = false;
					break;
				case 3:
					rad_QuadScanB2T_B.IsChecked = true;
					cb_EnableMaskB.IsChecked = false;
					cb_Quad_EnableRGBThd_B.IsChecked = false;
					break;
				}
			}
		}
		else if (LocateType == 0)
		{
			OpenCvSharp.Rect rect7 = new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
			OpenCvSharp.Rect rect8 = FindMaxBlob(mat2);
			OpenCvSharp.Rect rect9 = new OpenCvSharp.Rect(rect8.Left / 2, rect8.Top / 2, rect8.Width + rect8.Left / 2 + (rect7.Right - rect8.Right) / 2, rect8.Height + rect8.Top / 2 + (rect7.Bottom - rect8.Bottom) / 2);
			Cv2.CvtColor(mat2, mat2, ColorConversionCodes.GRAY2BGR);
			mat2.Rectangle(rect8, Scalar.Red);
			mat2.Rectangle(rect9, Scalar.Yellow);
			SIMPSearchRect = rect9;
			SetSIMPSearchRect(SIMPSearchRect);
			chk_CreateHSVChannel.IsChecked = true;
			cb_THD1_DEStartCount.SelectedIndex = 0;
			cb_THD2_DEStartCount.SelectedIndex = 0;
			cb_THD3_DEStartCount.SelectedIndex = 0;
			cb_THD1_DEEndCount.SelectedIndex = 0;
			cb_THD2_DEEndCount.SelectedIndex = 0;
			cb_THD3_DEEndCount.SelectedIndex = 0;
			cb_SIMP_THD1_Channel.SelectedIndex = AutoLocateChID;
			exp_THD2.IsExpanded = false;
			exp_THD3.IsExpanded = false;
			Mat mat5 = new Mat(inspection.buffers.GetSrcBGR(AutoLocateChID), SIMPSearchRect);
			double num3 = Cv2.Threshold(dst: mat5.Clone(), src: mat5, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
			if (flag)
			{
				rs_SIMP_THD1.LowerValue = num3 + 15.0;
				rs_SIMP_THD1.UpperValue = 255.0;
			}
			else
			{
				rs_SIMP_THD1.LowerValue = 0.0;
				rs_SIMP_THD1.UpperValue = num3 - 15.0;
			}
			nu_Thd1LimitMin.Value = rs_SIMP_THD1.LowerValue;
			nu_Thd1LimitMax.Value = rs_SIMP_THD1.UpperValue;
		}
		Btn_Test_Click(null, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteModifyLog()
	{
		ModifyLog = "";
		try
		{
			ModifyLogFun modifyLogFun = new ModifyLogFun();
			DateTime now = DateTime.Now;
			string arg = now.ToString("yyyyMM");
			string text = string.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory, "Log\\", arg);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arg2 = now.ToString("yyyyMMdd");
			string text2 = string.Format("{0}\\{1}-Modify.log", text, arg2);
			string arg3 = "[" + RecipeName + "][" + UserID + "] " + now.ToString("[yyyy-MM-dd] [HH:mm:ss]");
			INSP_LOCATE_PARAM locateParam = inspection.paramFile.LocateParam;
			string text3 = string.Format("{0}\t[{1}][{2}] ", arg3, inspection.StationTxt, LanguageManager.Instance["LocateTool"]);
			if (locateParam.LocateType != BackupParam.LocateType)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text3, LanguageManager.Instance["LocateType"], modifyLogFun.GetLocateTypeText(BackupParam.LocateType), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetLocateTypeText(locateParam.LocateType));
			}
			if (locateParam.SIMP_THD1_Channel != BackupParam.SIMP_THD1_Channel)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3} {4} {5}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.SIMP_THD1_Channel), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.SIMP_THD1_Channel));
			}
			if (locateParam.SIMP_THD1_Min != BackupParam.SIMP_THD1_Min)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.SIMP_THD1_Min, LanguageManager.Instance["ChangedTo"], locateParam.SIMP_THD1_Min);
			}
			if (locateParam.SIMP_THD1_Max != BackupParam.SIMP_THD1_Max)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.SIMP_THD1_Max, LanguageManager.Instance["ChangedTo"], locateParam.SIMP_THD1_Max);
			}
			if (locateParam.SIMP_THD2_Enabled != BackupParam.SIMP_THD2_Enabled)
			{
				if (locateParam.SIMP_THD2_Enabled)
				{
					ModifyLog += string.Format("{0} [({1}-{2}2) {3}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}2) {3}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.SIMP_THD2_Channel != BackupParam.SIMP_THD2_Channel)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3} {4} {5}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.SIMP_THD2_Channel), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.SIMP_THD2_Channel));
			}
			if (locateParam.SIMP_THD2_Min != BackupParam.SIMP_THD2_Min)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.SIMP_THD2_Min, LanguageManager.Instance["ChangedTo"], locateParam.SIMP_THD2_Min);
			}
			if (locateParam.SIMP_THD2_Max != BackupParam.SIMP_THD2_Max)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.SIMP_THD2_Max, LanguageManager.Instance["ChangedTo"], locateParam.SIMP_THD2_Max);
			}
			if (locateParam.SIMP_THD3_Enabled != BackupParam.SIMP_THD3_Enabled)
			{
				if (locateParam.SIMP_THD3_Enabled)
				{
					ModifyLog += string.Format("{0} [({1}-{2}3) {3}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}3) {3}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.SIMP_THD3_Channel != BackupParam.SIMP_THD3_Channel)
			{
				ModifyLog += string.Format("{0} [({1}-{2}3) {3} {4} {5}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.SIMP_THD3_Channel), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.SIMP_THD3_Channel));
			}
			if (locateParam.SIMP_THD3_Min != BackupParam.SIMP_THD3_Min)
			{
				ModifyLog += string.Format("{0} [({1}-{2}3) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.SIMP_THD3_Min, LanguageManager.Instance["ChangedTo"], locateParam.SIMP_THD3_Min);
			}
			if (locateParam.SIMP_THD3_Max != BackupParam.SIMP_THD3_Max)
			{
				ModifyLog += string.Format("{0} [({1}-{2}3) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.SIMP_THD3_Max, LanguageManager.Instance["ChangedTo"], locateParam.SIMP_THD3_Max);
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.SIMP_DilateErode1Start, (int)BackupParam.SIMP_DilateErode1Start, locateParam.SIMP_DilateErode1StartCount, BackupParam.SIMP_DilateErode1StartCount, 1, 1, text3, LanguageManager.Instance["EasyLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.SIMP_DilateErode1End, (int)BackupParam.SIMP_DilateErode1End, locateParam.SIMP_DilateErode1EndCount, BackupParam.SIMP_DilateErode1EndCount, 1, 2, text3, LanguageManager.Instance["EasyLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.SIMP_DilateErode2Start, (int)BackupParam.SIMP_DilateErode2Start, locateParam.SIMP_DilateErode2StartCount, BackupParam.SIMP_DilateErode2StartCount, 2, 1, text3, LanguageManager.Instance["EasyLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.SIMP_DilateErode2End, (int)BackupParam.SIMP_DilateErode2End, locateParam.SIMP_DilateErode2EndCount, BackupParam.SIMP_DilateErode2EndCount, 2, 2, text3, LanguageManager.Instance["EasyLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.SIMP_DilateErode3Start, (int)BackupParam.SIMP_DilateErode3Start, locateParam.SIMP_DilateErode3StartCount, BackupParam.SIMP_DilateErode3StartCount, 3, 1, text3, LanguageManager.Instance["EasyLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.SIMP_DilateErode3End, (int)BackupParam.SIMP_DilateErode3End, locateParam.SIMP_DilateErode3EndCount, BackupParam.SIMP_DilateErode3EndCount, 3, 2, text3, LanguageManager.Instance["EasyLocate"] + "-");
			if (locateParam.Quad_Channel_L != BackupParam.Quad_Channel_L)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Quad_Channel_L), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Quad_Channel_L));
			}
			if (locateParam.Quad_THD_Min_L != BackupParam.Quad_THD_Min_L)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Quad_THD_Min_L, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Min_L);
			}
			if (locateParam.Quad_THD_Max_L != BackupParam.Quad_THD_Max_L)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Quad_THD_Max_L, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Max_L);
			}
			if (locateParam.Quad_Channel_R != BackupParam.Quad_Channel_R)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Quad_Channel_R), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Quad_Channel_R));
			}
			if (locateParam.Quad_THD_Min_R != BackupParam.Quad_THD_Min_R)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Quad_THD_Min_R, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Min_R);
			}
			if (locateParam.Quad_THD_Max_R != BackupParam.Quad_THD_Max_R)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Quad_THD_Max_R, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Max_R);
			}
			if (locateParam.Quad_Channel_T != BackupParam.Quad_Channel_T)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Top2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Quad_Channel_T), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Quad_Channel_T));
			}
			if (locateParam.Quad_THD_Min_T != BackupParam.Quad_THD_Min_T)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Top2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Quad_THD_Min_T, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Min_T);
			}
			if (locateParam.Quad_THD_Max_T != BackupParam.Quad_THD_Max_T)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Top2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Quad_THD_Max_T, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Max_T);
			}
			if (locateParam.Quad_Channel_B != BackupParam.Quad_Channel_B)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Bottom2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Quad_Channel_B), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Quad_Channel_B));
			}
			if (locateParam.Quad_THD_Min_B != BackupParam.Quad_THD_Min_B)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Bottom2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Quad_THD_Min_B, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Min_B);
			}
			if (locateParam.Quad_THD_Max_B != BackupParam.Quad_THD_Max_B)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Bottom2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Quad_THD_Max_B, LanguageManager.Instance["ChangedTo"], locateParam.Quad_THD_Max_B);
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEStart_L, (int)BackupParam.Quad_DEStart_L, locateParam.Quad_DEStartCount_L, BackupParam.Quad_DEStartCount_L, 1, 1, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEEnd_L, (int)BackupParam.Quad_DEEnd_L, locateParam.Quad_DEEndCount_L, BackupParam.Quad_DEEndCount_L, 1, 2, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEStart_R, (int)BackupParam.Quad_DEStart_R, locateParam.Quad_DEStartCount_R, BackupParam.Quad_DEStartCount_R, 2, 1, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEEnd_R, (int)BackupParam.Quad_DEEnd_R, locateParam.Quad_DEEndCount_R, BackupParam.Quad_DEEndCount_R, 2, 2, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEStart_T, (int)BackupParam.Quad_DEStart_T, locateParam.Quad_DEStartCount_T, BackupParam.Quad_DEStartCount_T, 3, 1, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEEnd_T, (int)BackupParam.Quad_DEEnd_T, locateParam.Quad_DEEndCount_T, BackupParam.Quad_DEEndCount_T, 3, 2, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEStart_B, (int)BackupParam.Quad_DEStart_B, locateParam.Quad_DEStartCount_B, BackupParam.Quad_DEStartCount_B, 4, 1, text3, LanguageManager.Instance["QuadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Quad_DEEnd_B, (int)BackupParam.Quad_DEEnd_B, locateParam.Quad_DEEndCount_B, BackupParam.Quad_DEEndCount_B, 4, 2, text3, LanguageManager.Instance["QuadLocate"] + "-");
			if (locateParam.Direction_L != BackupParam.Direction_L)
			{
				if (locateParam.Direction_L == ScanDirection.EM_LEFT_RIGHT)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Right2Left"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Left2Right"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Left2Right"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Right2Left"]);
				}
			}
			if (locateParam.Direction_R != BackupParam.Direction_R)
			{
				if (locateParam.Direction_R == ScanDirection.EM_LEFT_RIGHT)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Right2Left"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Left2Right"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Left2Right"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Right2Left"]);
				}
			}
			if (locateParam.Direction_T != BackupParam.Direction_T)
			{
				if (locateParam.Direction_T == ScanDirection.EM_TOP_BOTTOM)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Top2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Bottom2Top"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Top2Bottom"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Top2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Top2Bottom"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Bottom2Top"]);
				}
			}
			if (locateParam.Direction_B != BackupParam.Direction_B)
			{
				if (locateParam.Direction_B == ScanDirection.EM_TOP_BOTTOM)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Bottom2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Bottom2Top"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Top2Bottom"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Bottom2"], LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Top2Bottom"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Bottom2Top"]);
				}
			}
			if (locateParam.Quad_FilterPoint != BackupParam.Quad_FilterPoint)
			{
				if (locateParam.Quad_FilterPoint)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseEdgeDirFilter"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseEdgeDirFilter"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.Quad_UseStdFilter != BackupParam.Quad_UseStdFilter)
			{
				if (locateParam.Quad_UseStdFilter)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseStdFilter"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseStdFilter"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.Quad_LocateAngleSelect != BackupParam.Quad_LocateAngleSelect)
			{
				ModifyLog += string.Format("{0} [({1}-{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["RotateAngleUse"], modifyLogFun.GetRotateAngleUseText(BackupParam.Quad_LocateAngleSelect), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetRotateAngleUseText(locateParam.Quad_LocateAngleSelect));
			}
			if (locateParam.Quad_UseSideLocate != BackupParam.Quad_UseSideLocate)
			{
				if (locateParam.Quad_UseSideLocate)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseSideLocate"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseSideLocate"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.UseMelfMode != BackupParam.UseMelfMode)
			{
				if (locateParam.UseMelfMode)
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseMelfLocate"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}-{2}) {3}]\n", text3, LanguageManager.Instance["QuadLocate"], LanguageManager.Instance["UseMelfLocate"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.Pad_Channel_L1 != BackupParam.Pad_Channel_L1)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3} {4} {5}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Pad_Channel_L1), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Pad_Channel_L1));
			}
			if (locateParam.Pad_Channel_L2 != BackupParam.Pad_Channel_L2)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3} {4} {5}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Pad_Channel_L2), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Pad_Channel_L2));
			}
			if (locateParam.Pad_THD_Min_L1 != BackupParam.Pad_THD_Min_L1)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Pad_THD_Min_L1, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Min_L1);
			}
			if (locateParam.Pad_THD_Max_L1 != BackupParam.Pad_THD_Max_L1)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Pad_THD_Max_L1, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Max_L1);
			}
			if (locateParam.Pad_THD_Min_L2 != BackupParam.Pad_THD_Min_L2)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Pad_THD_Min_L2, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Min_L2);
			}
			if (locateParam.Pad_THD_Max_L2 != BackupParam.Pad_THD_Max_L2)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Pad_THD_Max_L2, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Max_L2);
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEStart_L1, (int)BackupParam.Pad_DEStart_L1, locateParam.Pad_DEStartCount_L1, BackupParam.Pad_DEStartCount_L1, 1, 1, text3, LanguageManager.Instance["PadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEEnd_L1, (int)BackupParam.Pad_DEEnd_L1, locateParam.Pad_DEEndCount_L1, BackupParam.Pad_DEEndCount_L1, 1, 2, text3, LanguageManager.Instance["PadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEStart_L2, (int)BackupParam.Pad_DEStart_L2, locateParam.Pad_DEStartCount_L2, BackupParam.Pad_DEStartCount_L2, 2, 1, text3, LanguageManager.Instance["PadLocate"] + "-");
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEEnd_L2, (int)BackupParam.Pad_DEEnd_L2, locateParam.Pad_DEEndCount_L2, BackupParam.Pad_DEEndCount_L2, 2, 2, text3, LanguageManager.Instance["PadLocate"] + "-");
			if (locateParam.Pad_Channel_R1 != BackupParam.Pad_Channel_R1)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3} {4} {5}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Pad_Channel_R1), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Pad_Channel_R1));
			}
			if (locateParam.Pad_Channel_R2 != BackupParam.Pad_Channel_R2)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3} {4} {5}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText(BackupParam.Pad_Channel_R2), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText(locateParam.Pad_Channel_R2));
			}
			if (locateParam.Pad_THD_Min_R1 != BackupParam.Pad_THD_Min_R1)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Pad_THD_Min_R1, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Min_R1);
			}
			if (locateParam.Pad_THD_Max_R1 != BackupParam.Pad_THD_Max_R1)
			{
				ModifyLog += string.Format("{0} [({1}-{2}1) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Pad_THD_Max_R1, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Max_R1);
			}
			if (locateParam.Pad_THD_Min_R2 != BackupParam.Pad_THD_Min_R2)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.Pad_THD_Min_R2, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Min_R2);
			}
			if (locateParam.Pad_THD_Max_R2 != BackupParam.Pad_THD_Max_R2)
			{
				ModifyLog += string.Format("{0} [({1}-{2}2) {3}: {4} {5} {6}]\n", text3, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.Pad_THD_Max_R2, LanguageManager.Instance["ChangedTo"], locateParam.Pad_THD_Max_R2);
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEStart_R1, (int)BackupParam.Pad_DEStart_R1, locateParam.Pad_DEStartCount_R1, BackupParam.Pad_DEStartCount_R1, 1, 1, text3 + "-" + LanguageManager.Instance["PadLocate"]);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEEnd_R1, (int)BackupParam.Pad_DEEnd_R1, locateParam.Pad_DEEndCount_R1, BackupParam.Pad_DEEndCount_R1, 1, 2, text3 + "-" + LanguageManager.Instance["PadLocate"]);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEStart_R2, (int)BackupParam.Pad_DEStart_R2, locateParam.Pad_DEStartCount_R2, BackupParam.Pad_DEStartCount_R2, 2, 1, text3 + "-" + LanguageManager.Instance["PadLocate"]);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)locateParam.Pad_DEEnd_R2, (int)BackupParam.Pad_DEEnd_R2, locateParam.Pad_DEEndCount_R2, BackupParam.Pad_DEEndCount_R2, 2, 2, text3 + "-" + LanguageManager.Instance["PadLocate"]);
			GetTrackModifyText(text3, locateParam);
			if (locateParam.LocateSpeedUp != BackupParam.LocateSpeedUp)
			{
				if (locateParam.LocateSpeedUp)
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text3, LanguageManager.Instance["LocateSpeedUp"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text3, LanguageManager.Instance["LocateSpeedUp"], LanguageManager.Instance["Close"]);
				}
			}
			if (locateParam.LocateAngleLimit != BackupParam.LocateAngleLimit)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text3, LanguageManager.Instance["RotateAngleLimit"], BackupParam.LocateAngleLimit, LanguageManager.Instance["ChangedTo"], locateParam.LocateAngleLimit);
			}
			if (locateParam.LocateWidthMin != BackupParam.LocateWidthMin)
			{
				ModifyLog += string.Format("{0} [({1}{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["LocateWidth"], LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.LocateWidthMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(locateParam.LocateWidthMin));
			}
			if (locateParam.LocateWidthMax != BackupParam.LocateWidthMax)
			{
				ModifyLog += string.Format("{0} [({1}{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["LocateWidth"], LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.LocateWidthMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(locateParam.LocateWidthMax));
			}
			if (locateParam.LocateHeightMin != BackupParam.LocateHeightMin)
			{
				ModifyLog += string.Format("{0} [({1}{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["LocateHeight"], LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.LocateHeightMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(locateParam.LocateHeightMin));
			}
			if (locateParam.LocateHeightMax != BackupParam.LocateHeightMax)
			{
				ModifyLog += string.Format("{0} [({1}{2}) {3} {4} {5}]\n", text3, LanguageManager.Instance["LocateHeight"], LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.LocateHeightMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(locateParam.LocateHeightMax));
			}
			if (locateParam.LocateErrorBin != BackupParam.LocateErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text3, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)BackupParam.LocateErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)locateParam.LocateErrorBin));
			}
			if (locateParam.LocateSizeErrorBin != BackupParam.LocateSizeErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text3, LanguageManager.Instance["LocateSizeErrorBin"], modifyLogFun.GetErrorBinText((int)BackupParam.LocateSizeErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)locateParam.LocateSizeErrorBin));
			}
			if (ModifyLog == "")
			{
				modifyLogFun.Close();
				modifyLogFun = null;
				return;
			}
			if (CVBuffer.IsFileOpen(text2))
			{
				modifyLogFun.Close();
				modifyLogFun = null;
				return;
			}
			using (StreamWriter streamWriter = new StreamWriter(text2, append: true))
			{
				streamWriter.Write("{0}", ModifyLog);
				streamWriter.WriteLine();
			}
			modifyLogFun.Close();
			modifyLogFun = null;
		}
		catch (Exception ex)
		{
			string text4 = "BW10-LocateWindow-WriteModifyLog()-";
			text4 += ex.ToString();
			CVBuffer.SaveLog(text4, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text4, "Log Output Error");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Reverse_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("檢測參數將還原至上次儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("检测参数将还原至上次储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Parameters will Revert to Previous save！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("検査パラメータは、前回の保存内容に復元されます。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				inspection.paramFile.LocateParam = inspection.paramFile_Backup.LocateParam;
				CopyValueToUI();
			}
			Btn_Test_Click(null, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Chk_CreateHSVChannel_Click(object sender, RoutedEventArgs e)
	{
		if (chk_CreateHSVChannel.IsChecked == false)
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("警告，取消產生HSV色階，會造成後面二值化等工具無法使用HSV色階。");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("警告，取消产生HSV色阶，会造成后面二值化等工具无法使用HSV色阶。");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("警告: HSV色相環を生成しない場合、後続の二値化などのツールでHSV色相環を使用できなくなります。");
			}
			else
			{
				MessageBox.Show("Warning: Disabling HSV color generation will prevent subsequent tools such as binarization from using HSV color scales.");
			}
		}
	}

	private void Bor_SearchRectT_GotFocus(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			doQUADThdImage(2);
		}
	}

	private void Bor_SearchRectB_GotFocus(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			doQUADThdImage(3);
		}
	}

	private void Bor_SearchRectL_GotFocus(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			doQUADThdImage(0);
		}
	}

	private void Bor_SearchRectR_GotFocus(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			doQUADThdImage(1);
		}
	}

	private void Btn_SetModel_Click(object sender, RoutedEventArgs e)
	{
		LocateModelsWindow locateModelsWindow = new LocateModelsWindow();
		locateModelsWindow.LoadLanguage(LanguageType);
		if (txt_ModeName.Text != "")
		{
			locateModelsWindow.SetCurrentModelName(txt_ModeName.Text);
		}
		locateModelsWindow.SetInspection(inspection);
		locateModelsWindow.ShowDialog();
		string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(locateModelsWindow.GetSelectModelName());
		txt_ModeName.Text = fileNameWithoutExtension;
		if (txt_ModeName.Text != "")
		{
			UpdateAIDB();
		}
		WriteUIValueToParam();
	}

	private void btn_Train_Click(object sender, RoutedEventArgs e)
	{
		if (!(txt_ModeName.Text == ""))
		{
			Training(txt_ModeName.Text);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Draw_Click(object sender, RoutedEventArgs e)
	{
		string text = txt_ModeName.Text;
		if (text == "")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("請先設定及選擇AI模型。", "警告");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("请先设定及选择AI模型。", "警告");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("AIモデルを設定し、選択してください。", "警告");
			}
			else
			{
				MessageBox.Show("Please set and select the AI model first.", "Warning");
			}
			return;
		}
		string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string ngpath = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string markpath = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			WriteUIValueToParam();
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, inspection.buffers.Src.Width, inspection.buffers.Src.Height);
			inspection.locateTool.ReCalcMaxSearchRect(inspection.paramFile.LocateParam.LocateType, rect);
			AIInfluenceRect = inspection.paramFile.LocateParam.MaxSearchRect;
			AIInfluenceRect &= rect;
			Mat mat = new Mat(inspection.buffers.Src, AIInfluenceRect);
			if (mat.Width > 0 && mat.Height > 0)
			{
				AIMarkWindow aIMarkWindow = new AIMarkWindow();
				int stationIndex = inspection.paramFile.GetStationIndex();
				aIMarkWindow.SetInspection(inspection, stationIndex);
				aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
				if (mSam != null)
				{
					aIMarkWindow.SetSAM(mSam);
				}
				aIMarkWindow.LoadLanguage(LanguageType);
				aIMarkWindow.SetOrgImage(inspection.buffers.Dst);
				aIMarkWindow.SetMat(mat, inspection.paramFile.ZoomMode);
				aIMarkWindow.SetNGMarkedFolder(ngpath, markpath);
				aIMarkWindow.SetColorSelect(inspection.paramFile.ColorSelect);
				aIMarkWindow.SetPreviewMode(inspection.paramFile.PreviewMode);
				aIMarkWindow.SetPenSize(inspection.paramFile.PenSize);
				aIMarkWindow.SetContinueWrite(inspection.paramFile.EnableContinueWrite);
				aIMarkWindow.SetAutoDrawIgnore(inspection.paramFile.EnableAutoIgnoreDraw, inspection.paramFile.AutoIgnoreSize);
				aIMarkWindow.SetCaption(inspection.paramFile.DrawCaition, inspection.paramFile.SaveAsFolder);
				aIMarkWindow.ShowDialog();
				inspection.paramFile.PreviewMode = aIMarkWindow.GetPreviewMode();
				inspection.paramFile.ZoomMode = aIMarkWindow.GetZoomMode();
				inspection.paramFile.ColorSelect = aIMarkWindow.GetColorSelect();
				inspection.paramFile.PenSize = aIMarkWindow.GetPenSize();
				inspection.paramFile.EnableContinueWrite = aIMarkWindow.GetContinueWrite();
				inspection.paramFile.EnableAutoIgnoreDraw = aIMarkWindow.GetEnableAutoDrawIgnore();
				inspection.paramFile.AutoIgnoreSize = aIMarkWindow.GetAutoDrawIgnoreSize();
			}
			UpdateAIDB();
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_DrawMask_Click()" + ex.ToString());
		}
	}

	private void chk_EnableTensorRT_Click(object sender, RoutedEventArgs e)
	{
		inspection.paramFile.LocateParam.EnableTensorRT = chk_EnableTensorRT.IsChecked.Value;
		grd_Loading.Visibility = Visibility.Visible;
		loadingstatus = 1;
		LoadingTimer.Start();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Ruler_Click(object sender, RoutedEventArgs e)
	{
		if (!RulerMode)
		{
			RulerMode = true;
			lab_Ruler.Visibility = Visibility.Visible;
			Pt1 = new OpenCvSharp.Point(0, 0);
			Pt2 = new OpenCvSharp.Point(0, 0);
			cnv_Image.Cursor = Cursors.Cross;
			bor_SIMPSearchRect.Visibility = Visibility.Hidden;
			Mat image = inspection.buffers.Src.Clone();
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(image);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
			btn_Ruler.Background = Brushes.Yellow;
			if (LanguageType == 1)
			{
				lab_Ruler.Content = string.Format("尺寸量測模式  請點選第一點!");
			}
			else if (LanguageType == 2)
			{
				lab_Ruler.Content = string.Format("尺寸量测模式  请点选第一点!");
			}
			else if (LanguageType == 3)
			{
				lab_Ruler.Content = string.Format("サイズ計測モード: 第一点を選択してください。");
			}
			else
			{
				lab_Ruler.Content = string.Format("Measure size mode, plase set First pt!");
			}
		}
		else
		{
			RulerMode = false;
			lab_Ruler.Visibility = Visibility.Hidden;
			cnv_Image.Cursor = Cursors.Arrow;
			bor_SIMPSearchRect.Visibility = Visibility.Visible;
			Mat image2 = inspection.buffers.Src.Clone();
			ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(image2);
			ImageBrush imageBrush2 = new ImageBrush();
			imageBrush2.ImageSource = imageSource2;
			cnv_Image.Background = imageBrush2;
			imageSource2 = null;
			THD_Preview = false;
			btn_Ruler.Background = Brushes.White;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTrackModifyText(string Title, INSP_LOCATE_PARAM param)
	{
		if (param.LocateType == 0)
		{
			if (param.SIMPSearchRect != BackupParam.SIMPSearchRect)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["EasyLocate"], LanguageManager.Instance["TrackPos"], BackupParam.SIMPSearchRect.Left, BackupParam.SIMPSearchRect.Right, BackupParam.SIMPSearchRect.Top, BackupParam.SIMPSearchRect.Bottom, LanguageManager.Instance["ChangedTo"], param.SIMPSearchRect.Left, param.SIMPSearchRect.Right, param.SIMPSearchRect.Top, param.SIMPSearchRect.Bottom);
			}
		}
		else if (param.LocateType == 1)
		{
			if (param.QuadSearchRectT != BackupParam.QuadSearchRectT)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Top2"], LanguageManager.Instance["TrackPos"], BackupParam.QuadSearchRectT.Left, BackupParam.QuadSearchRectT.Right, BackupParam.QuadSearchRectT.Top, BackupParam.QuadSearchRectT.Bottom, LanguageManager.Instance["ChangedTo"], param.QuadSearchRectT.Left, param.QuadSearchRectT.Right, param.QuadSearchRectT.Top, param.QuadSearchRectT.Bottom);
			}
			if (param.QuadSearchRectB != BackupParam.QuadSearchRectB)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Bottom2"], LanguageManager.Instance["TrackPos"], BackupParam.QuadSearchRectB.Left, BackupParam.QuadSearchRectB.Right, BackupParam.QuadSearchRectB.Top, BackupParam.QuadSearchRectB.Bottom, LanguageManager.Instance["ChangedTo"], param.QuadSearchRectB.Left, param.QuadSearchRectB.Right, param.QuadSearchRectB.Top, param.QuadSearchRectB.Bottom);
			}
			if (param.QuadSearchRectL != BackupParam.QuadSearchRectL)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["TrackPos"], BackupParam.QuadSearchRectL.Left, BackupParam.QuadSearchRectL.Right, BackupParam.QuadSearchRectL.Top, BackupParam.QuadSearchRectL.Bottom, LanguageManager.Instance["ChangedTo"], param.QuadSearchRectL.Left, param.QuadSearchRectL.Right, param.QuadSearchRectL.Top, param.QuadSearchRectL.Bottom);
			}
			if (param.QuadSearchRectR != BackupParam.QuadSearchRectR)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["QuadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["TrackPos"], BackupParam.QuadSearchRectR.Left, BackupParam.QuadSearchRectR.Right, BackupParam.QuadSearchRectR.Top, BackupParam.QuadSearchRectR.Bottom, LanguageManager.Instance["ChangedTo"], param.QuadSearchRectR.Left, param.QuadSearchRectR.Right, param.QuadSearchRectR.Top, param.QuadSearchRectR.Bottom);
			}
		}
		else
		{
			if (param.PadSearchRectL != BackupParam.PadSearchRectL)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Left2"], LanguageManager.Instance["TrackPos"], BackupParam.PadSearchRectL.Left, BackupParam.PadSearchRectL.Right, BackupParam.PadSearchRectL.Top, BackupParam.PadSearchRectL.Bottom, LanguageManager.Instance["ChangedTo"], param.PadSearchRectL.Left, param.PadSearchRectL.Right, param.PadSearchRectL.Top, param.PadSearchRectL.Bottom);
			}
			if (param.PadSearchRectR != BackupParam.PadSearchRectR)
			{
				ModifyLog += string.Format("{0} [({1}) ({2}) L:{3} R:{4} T:{5} B:{6} {7} L:{8} R:{9} T:{10} B:{11}] \n", Title, LanguageManager.Instance["PadLocate"] + LanguageManager.Instance["Right2"], LanguageManager.Instance["TrackPos"], BackupParam.PadSearchRectR.Left, BackupParam.PadSearchRectR.Right, BackupParam.PadSearchRectR.Top, BackupParam.PadSearchRectR.Bottom, LanguageManager.Instance["ChangedTo"], param.PadSearchRectR.Left, param.PadSearchRectR.Right, param.PadSearchRectR.Top, param.PadSearchRectR.Bottom);
			}
		}
	}

	private OpenCvSharp.Rect FindMaxBlob(Mat binary)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		Mat mat = new Mat();
		Mat mat2 = new Mat();
		Mat mat3 = new Mat();
		int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
		Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
		int num2 = 0;
		int i = 0;
		if (num > 1)
		{
			for (int j = 1; j < num; j++)
			{
				if (genericIndexer[j, 4] > num2)
				{
					i = j;
					num2 = genericIndexer[j, 4];
				}
			}
			rect = new OpenCvSharp.Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
		}
		else
		{
			rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		}
		return rect;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CheckTrainingFile()
	{
		string text = "";
		string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\train.exe");
		string text3 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\h5_to_pb.exe");
		if (!File.Exists(text2))
		{
			if (!File.Exists(text2))
			{
				text = ((inspection.paramFile.LanguageType == 1) ? (text + string.Format("[定位前處理] 找不到訓練主程式 {0}。\n", text2)) : ((inspection.paramFile.LanguageType == 2) ? (text + string.Format("[定位前处理] 找不到训练主程式 {0}。\n", text2)) : ((inspection.paramFile.LanguageType != 3) ? (text + string.Format("[Locate Enhence] Cannot find the training program {0}。\n", text2)) : (text + string.Format("[位置合わせ前処理] トレーニング主プログラム {0} が見つかりません。\n", text2)))));
			}
			if (!File.Exists(text3))
			{
				text = ((inspection.paramFile.LanguageType == 1) ? (text + string.Format("[定位前處理] 找不到PB轉換程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text3)) : ((inspection.paramFile.LanguageType == 2) ? (text + string.Format("[定位前处理] 找不到PB转换程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text3)) : ((inspection.paramFile.LanguageType != 3) ? (text + string.Format("[Locate Enhence] Cannot find the PB conversion program (Training\\Model\\h5_to_pb.exe)。\n", text3)) : (text + string.Format("[位置合わせ前処理] PB 変換プログラム (Training\\Model\\h5_to_pb.exe) {0} が見つかりません。\n", text3)))));
			}
			MessageBox.Show(text);
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Training(string modelname)
	{
		try
		{
			string text = string.Format("{0}TrainingImage\\{1}\\X\\", AppDomain.CurrentDomain.BaseDirectory, modelname);
			string.Format("{0}TrainingImage\\{1}\\Y\\", AppDomain.CurrentDomain.BaseDirectory, modelname);
			string arg = string.Format("{0}-TempFile", System.IO.Path.GetFileNameWithoutExtension(modelname));
			string path = string.Format("{0}{1}{2}.lh5d", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
			string path2 = string.Format("{0}{1}{2}.lpbd", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			string path3 = text + "NG\\";
			string path4 = text + "OK\\";
			RunDuplicateImageCheck(path3, path4, extramode: false);
			if (GetTrainingImageCount(text) <= 0)
			{
				return false;
			}
			if (!CreateIni())
			{
				return false;
			}
			CopyModel(modelname);
			RunTrainingScript();
			DelTempProcFile(ClearRemote: false);
			CopyNewModel(modelname);
			grd_Loading.Visibility = Visibility.Visible;
			loadingstatus = 0;
			LoadingTimer.Start();
			return true;
		}
		catch (Exception ex)
		{
			string text2 = string.Format("AIP01-LocateAIEnhanceTool-Training(DebugCode-{0})", "") + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool RunTrainingScript()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		string text = "";
		double num = 0.0;
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "TrainningNew.bat");
		string path2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Trainning.bat");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if (File.Exists(path2))
		{
			File.Delete(path2);
		}
		string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training.bat");
		processStartInfo.FileName = "Training.bat";
		string text3 = txt_TrainCount.Text;
		string.Format("{0}Training\\train\\train.exe", AppDomain.CurrentDomain.BaseDirectory);
		int num2 = 6;
		text = string.Format("cd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V3 -r Train -e {0} -l {1} -a {2}", text3, num2, num);
		if (File.Exists(text2))
		{
			FileInfo fileInfo = new FileInfo(text2);
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
			File.Delete(text2);
		}
		using (StreamWriter streamWriter = new StreamWriter(text2, append: true))
		{
			streamWriter.Write("{0}", text);
			streamWriter.WriteLine();
		}
		if (!File.Exists(text2))
		{
			return false;
		}
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = false;
		Process process = Process.Start(processStartInfo);
		process.WaitForExit();
		process.Close();
		DelTempProcFile();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DelTempProcFile(bool ClearRemote = true)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path, "*.proc", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
		path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Proc\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files2 = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
		for (int j = 0; j < files2.Length; j++)
		{
			File.Delete(files2[j]);
		}
		path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\Proc\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files3 = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
		for (int k = 0; k < files3.Length; k++)
		{
			File.Delete(files3[k]);
		}
		string path2 = string.Format("{0}RemoteTraining\\Proc\\", AppDomain.CurrentDomain.BaseDirectory);
		if (!Directory.Exists(path2))
		{
			Directory.CreateDirectory(path2);
		}
		string[] files4 = Directory.GetFiles(path2, "*.txt", SearchOption.AllDirectories);
		for (int l = 0; l < files4.Length; l++)
		{
			File.Delete(files4[l]);
		}
		if (ClearRemote)
		{
			path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Comm\\");
			string[] files5 = Directory.GetFiles(path, "*.finish", SearchOption.AllDirectories);
			for (int m = 0; m < files5.Length; m++)
			{
				File.Delete(files5[m]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string RunDuplicateImageCheck(string path1, string path2, bool extramode)
	{
		string text = "";
		if (!Directory.Exists(path1))
		{
			return "Directory Path1 Error!";
		}
		if (!Directory.Exists(path2))
		{
			return "Directory Path2 Error!";
		}
		try
		{
			if (path1 == "")
			{
				return text;
			}
			string[] files = Directory.GetFiles(path1, "*.*", SearchOption.AllDirectories);
			string[] array = null;
			if (path2 != "")
			{
				array = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
			}
			if (files.Length == 0 && array.Length == 0)
			{
				return text;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < files.Length; i++)
			{
				list.Add(files[i]);
			}
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					list.Add(array[j]);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				list2.Add(GetFileHash(list[k]));
			}
			for (int l = 0; l < list2.Count; l++)
			{
				if (list2[l] == "")
				{
					continue;
				}
				for (int m = 0; m < list2.Count; m++)
				{
					if (list[l] == list[m] || !(list2[l] == list2[m]))
					{
						continue;
					}
					string fileName = System.IO.Path.GetFileName(list[l]);
					string fileName2 = System.IO.Path.GetFileName(list[m]);
					string text2 = list[m];
					string text3 = "";
					if (text2.Contains("\\X\\NG\\"))
					{
						text3 = text2.Replace("\\X\\NG\\", "\\Y\\NG\\");
					}
					else if (text2.Contains("\\X\\OK\\"))
					{
						text3 = text2.Replace("\\X\\OK\\", "\\Y\\OK\\");
					}
					if (File.Exists(text2))
					{
						FileInfo fileInfo = new FileInfo(text2);
						if (fileInfo.IsReadOnly)
						{
							fileInfo.IsReadOnly = false;
						}
						File.Delete(text2);
						text = ((LanguageType == 1) ? (text + string.Format("{0}影像與{1}影像相同，系統自動刪除原影像{2}\n", fileName, fileName2, fileName2)) : ((LanguageType == 2) ? (text + string.Format("{0}影像与{1}影像相同，系统自动删除原影像{2}\n", fileName, fileName2, fileName2)) : ((LanguageType != 3) ? (text + string.Format("Image {0} is identical to image {1}. The original image {2} has been automatically deleted.\n", fileName, fileName2, fileName2)) : (text + string.Format("{0}画像と{1}画像が同じため、システムは元の画像{2}を自動的に削除します\n", fileName, fileName2, fileName2)))));
					}
					if (File.Exists(text3) && text3 != "")
					{
						FileInfo fileInfo2 = new FileInfo(text3);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
						File.Delete(text3);
						text = ((LanguageType == 1) ? (text + string.Format("{0}影像與{1}影像相同，系統自動刪除原影像{2}\n", fileName, fileName2, System.IO.Path.GetFileName(text3))) : ((LanguageType == 2) ? (text + string.Format("{0}影像与{1}影像相同，系统自动删除原影像{2}\n", fileName, fileName2, System.IO.Path.GetFileName(text3))) : ((LanguageType != 3) ? (text + string.Format("Image {0} is identical to image {1}. The original image {2} has been automatically deleted.\n", fileName, fileName2, System.IO.Path.GetFileName(text3))) : (text + string.Format("{0}画像と{1}画像が同じため、システムは元の画像{2}を自動的に削除します\n", fileName, fileName2, System.IO.Path.GetFileName(text3))))));
					}
					list2[m] = "";
				}
			}
			return text;
		}
		catch (Exception ex)
		{
			string text4 = "AIP01-AITrainingWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text4);
			MessageBox.Show(text4, LanguageManager.Instance["Error"]);
			return text;
		}
	}

	private string GetFileHash(string file)
	{
		string result = "";
		if (!File.Exists(file))
		{
			return result;
		}
		HashAlgorithm hashAlgorithm = HashAlgorithm.Create();
		FileStream fileStream = new FileStream(file, FileMode.Open);
		result = BitConverter.ToString(hashAlgorithm.ComputeHash(fileStream));
		fileStream.Close();
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int GetTrainingImageCount(string X_Path)
	{
		if (!Directory.Exists(X_Path))
		{
			return -1;
		}
		string[] array = Directory.GetFiles(X_Path, "*.*", SearchOption.AllDirectories);
		for (int i = 0; i < array.Length; i++)
		{
			string extension = System.IO.Path.GetExtension(array[i]);
			extension = extension.ToLower();
			if (extension != ".bmp" && extension != ".jpg" && extension != ".tif" && extension != ".png")
			{
				array = array.Take(i).Concat(array.Skip(i + 1)).ToArray();
				i--;
			}
		}
		return array.Length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CreateIni()
	{
		if (txt_ModeName.Text == "")
		{
			return false;
		}
		try
		{
			string text = System.IO.Path.Combine(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\"), "navs.ini");
			string value = string.Format("{0}TrainingImage\\{1}\\X\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			string value2 = string.Format("{0}TrainingImage\\{1}\\Y\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			HiniFile hiniFile = new HiniFile(text, ";");
			hiniFile.WriteString("DAVS_V3", "SegModelName", "Training\\Model\\navs.h5");
			hiniFile.WriteInt("DAVS_V3", "ImgWidth", inspection.paramFile.LocateParam.ModelWidth);
			hiniFile.WriteInt("DAVS_V3", "ImgHeight", inspection.paramFile.LocateParam.ModelHeight);
			hiniFile.WriteDouble("DAVS_V3", "AugRatio", 1.0);
			hiniFile.WriteInt("DAVS_V3", "Epoch", inspection.paramFile.LocateParam.AILocateTrainCount);
			hiniFile.WriteString("DAVS_V3", "PredPath", "");
			hiniFile.WriteString("DAVS_V3", "OutPath", "OutPath");
			hiniFile.WriteInt("DAVS_V3", "LossFunction", 6);
			hiniFile.WriteDouble("DAVS_V3", "MaybeArea", 0.3);
			hiniFile.WriteDouble("DAVS_V3", "LR", 0.001);
			hiniFile.WriteInt("DAVS_V3", "NGIgnore", 0);
			hiniFile.WriteInt("DAVS_V3", "OKBlank", 1);
			hiniFile.WriteInt("DAVS_V3", "Threshold", 220);
			hiniFile.WriteString("DAVS_V3", "XPath", value);
			hiniFile.WriteString("DAVS_V3", "YPath", value2);
			hiniFile.WriteString("DAVS_V3", "ModelType", "US");
			hiniFile.WriteInt("DAVS_V3", "UnetDiv", inspection.paramFile.LocateParam.ModelDiv);
			hiniFile.WriteInt("DAVS_V3", "BatchSize", inspection.paramFile.LocateParam.ModelBatchSize);
			hiniFile.WriteDouble("Augmentation", "horizontalFlip", 0.0);
			hiniFile.WriteDouble("Augmentation", "verticalFlip", 0.0);
			hiniFile.WriteDouble("Augmentation", "brightness_limit", 0.0);
			hiniFile.WriteDouble("Augmentation", "contrast_limit", 0.0);
			hiniFile.WriteDouble("Augmentation", "scale_limit", 0.0);
			hiniFile.WriteDouble("Augmentation", "rotate_limit", 0.0);
			hiniFile.WriteDouble("Augmentation", "shift_limit", 0.0);
			hiniFile.WriteInt("Augmentation", "hue_shift_limit", 0);
			hiniFile.WriteInt("Augmentation", "sat_shift_limit", 0);
			hiniFile.WriteInt("Augmentation", "val_shift_limit", 0);
			hiniFile.WriteInt("Augmentation", "r_shift_limit", 0);
			hiniFile.WriteInt("Augmentation", "g_shift_limit", 0);
			hiniFile.WriteInt("Augmentation", "b_shift_limit", 0);
			if (GetFileEncodeType(text) != Encoding.UTF8)
			{
				string value3 = File.ReadAllText(text, Encoding.Default);
				File.Delete(text);
				FileStream fileStream = File.Create(text);
				using (StreamWriter streamWriter = new StreamWriter(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
				{
					streamWriter.Write(value3);
					streamWriter.WriteLine();
				}
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CreateIni()" + ex.ToString());
			return false;
		}
	}

	private void cb_EnableAILocate_Click(object sender, RoutedEventArgs e)
	{
		if (cb_EnableAILocate.IsChecked == true)
		{
			btn_Img_AILR.Visibility = Visibility.Visible;
			btn_Img_AILG.Visibility = Visibility.Visible;
			btn_Img_AILB.Visibility = Visibility.Visible;
			btn_Img_AILRGB.Visibility = Visibility.Visible;
			btn_AILocate.Visibility = Visibility.Visible;
			bor_AILocateRect.Visibility = Visibility.Visible;
			switch (tab_LocateType.SelectedIndex)
			{
			case 0:
				UpdateColorChannelItem(cb_SIMP_THD1_Channel, usedAI: true);
				UpdateColorChannelItem(cb_SIMP_THD2_Channel, usedAI: true);
				UpdateColorChannelItem(cb_SIMP_THD3_Channel, usedAI: true);
				break;
			case 1:
			{
				for (int i = 0; i < 4; i++)
				{
					UpdateColorChannelItem(QuadChannel[i], usedAI: true);
				}
				break;
			}
			case 2:
				UpdateColorChannelItem(cb_PadChannelL1, usedAI: true);
				UpdateColorChannelItem(cb_PadChannelL2, usedAI: true);
				UpdateColorChannelItem(cb_PadChannelR1, usedAI: true);
				UpdateColorChannelItem(cb_PadChannelR2, usedAI: true);
				break;
			}
		}
		else
		{
			btn_Img_AILR.Visibility = Visibility.Hidden;
			btn_Img_AILG.Visibility = Visibility.Hidden;
			btn_Img_AILB.Visibility = Visibility.Hidden;
			btn_Img_AILRGB.Visibility = Visibility.Hidden;
			btn_AILocate.Visibility = Visibility.Hidden;
			grd_AILocate.Visibility = Visibility.Hidden;
			bor_AILocateRect.Visibility = Visibility.Hidden;
			switch (tab_LocateType.SelectedIndex)
			{
			case 0:
				UpdateColorChannelItem(cb_SIMP_THD1_Channel, usedAI: false);
				UpdateColorChannelItem(cb_SIMP_THD2_Channel, usedAI: false);
				UpdateColorChannelItem(cb_SIMP_THD3_Channel, usedAI: false);
				break;
			case 1:
			{
				for (int j = 0; j < 4; j++)
				{
					UpdateColorChannelItem(QuadChannel[j], usedAI: false);
				}
				break;
			}
			case 2:
				UpdateColorChannelItem(cb_PadChannelL1, usedAI: false);
				UpdateColorChannelItem(cb_PadChannelL2, usedAI: false);
				UpdateColorChannelItem(cb_PadChannelR1, usedAI: false);
				UpdateColorChannelItem(cb_PadChannelR2, usedAI: false);
				break;
			}
		}
		WriteUIValueToParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateColorChannelItem(ComboBox box, bool usedAI)
	{
		if (usedAI)
		{
			int selectedIndex = box.SelectedIndex;
			box.Items.Clear();
			box.Items.Add("R");
			box.Items.Add("G");
			box.Items.Add("B");
			box.Items.Add("AI LR");
			box.Items.Add("AI LG");
			box.Items.Add("AI LB");
			box.SelectedIndex = selectedIndex;
		}
		else
		{
			int selectedIndex2 = box.SelectedIndex;
			box.Items.Clear();
			box.Items.Add("R");
			box.Items.Add("G");
			box.Items.Add("B");
			box.SelectedIndex = selectedIndex2 % 2;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateAIDB(int index = 1)
	{
		string path = string.Format("{0}TrainingImage\\{1}\\X\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
		DBfilelist = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		MarkingImgCount = DBfilelist.Length;
		if (MarkingImgCount > 0)
		{
			btn_TrainImageNum.Content = string.Format("{0} / {1}", index, MarkingImgCount);
		}
		else
		{
			btn_TrainImageNum.Content = string.Format("{0} / {1}", 0, MarkingImgCount);
		}
		if (MarkingImgCount >= 0)
		{
			PredictDBMat();
		}
	}

	private void btn_AILocate_Click(object sender, RoutedEventArgs e)
	{
		if (grd_AILocate.Visibility == Visibility.Hidden)
		{
			grd_AILocate.Visibility = Visibility.Visible;
			if (txt_ModeName.Text != "")
			{
				UpdateAIDB();
			}
		}
		else
		{
			grd_AILocate.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void PredictDBMat()
	{
		if (DBfilelist == null || DBfilelist.Length == 0)
		{
			return;
		}
		try
		{
			if (DBIndex >= DBfilelist.Length)
			{
				DBIndex = 0;
			}
			if (DBIndex < 0)
			{
				DBIndex = DBfilelist.Length - 1;
			}
			Mat mat = Cv2.ImRead(DBfilelist[DBIndex]);
			inspection.locateTool.PredictDBMat(mat);
			Mat destDBMatColor = inspection.locateTool.GetDestDBMatColor();
			ImageSource imageSource = inspection.buffers.Mat2ImageSource(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_SourceImg.Background = imageBrush;
			ImageSource imageSource2 = inspection.buffers.Mat2ImageSource(destDBMatColor);
			ImageBrush imageBrush2 = new ImageBrush();
			imageBrush2.ImageSource = imageSource2;
			cnv_AIImg.Background = imageBrush2;
			Mat image = Cv2.ImRead(DBfilelist[DBIndex].Replace("\\X\\", "\\Y\\"));
			ImageSource imageSource3 = inspection.buffers.Mat2ImageSource(image);
			ImageBrush imageBrush3 = new ImageBrush();
			imageBrush3.ImageSource = imageSource3;
			cnv_AIMarkImg.Background = imageBrush3;
		}
		catch (Exception ex)
		{
			string text = "MW02-AIProphetToolWindow-PredictDBMat()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_DeleteDBImage_Click(object sender, RoutedEventArgs e)
	{
		if (DBfilelist == null || DBfilelist.Length == 0)
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("當前的AI資料庫影像將被刪除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("当前的AI资料库影像将被删除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Delete current AI database image ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在のAIデータベースの画像が削除されます!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		try
		{
			string text = DBfilelist[DBIndex];
			if (!File.Exists(text))
			{
				return;
			}
			string text2 = "";
			if (text.Contains("\\X\\NG\\"))
			{
				text2 = text.Replace("\\X\\NG\\", "\\Y\\NG\\");
			}
			else if (text.Contains("\\X\\OK\\"))
			{
				text2 = text.Replace("\\X\\OK\\", "\\Y\\OK\\");
			}
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text);
				DBIndex--;
			}
			if (File.Exists(text2) && text2 != "")
			{
				FileInfo fileInfo2 = new FileInfo(text2);
				if (fileInfo2.IsReadOnly)
				{
					fileInfo2.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			if (DBIndex >= DBfilelist.Length)
			{
				DBIndex = DBfilelist.Length - 1;
			}
			if (DBIndex <= 0)
			{
				DBIndex = 0;
			}
			UpdateAIDB(DBIndex + 1);
			btn_NextDBImage_Click(this, null);
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIPreviewWindow-Btn_Delete_Click()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
		}
	}

	private void btn_NextDBImage_Click(object sender, RoutedEventArgs e)
	{
		DBIndex++;
		PredictDBMat();
		UpdateAIDB(DBIndex + 1);
	}

	private void btn_PrevDBImage_Click(object sender, RoutedEventArgs e)
	{
		DBIndex--;
		PredictDBMat();
		UpdateAIDB(DBIndex + 1);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TestPrevImg_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			default:
				return;
			case 0:
				return;
			case 1:
				if (filelist == null || filelist.Length == 0)
				{
					return;
				}
				if (fileIndex - 1 > 0)
				{
					fileIndex--;
				}
				else
				{
					fileIndex = filelist.Length - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
				mat = CVBuffer.ImRead(filelist[fileIndex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (RecentBufferIndex < 0 || inspection.RecentBufferList.Count <= 0)
				{
					return;
				}
				if (RecentBufferIndex - 1 > 0)
				{
					RecentBufferIndex--;
				}
				else
				{
					RecentBufferIndex = inspection.RecentBufferList.Count - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				inspection.SetBuffers(inspection.RecentBufferList[RecentBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (OKBufferIndex < 0 || inspection.OKBufferList.Count <= 0)
				{
					return;
				}
				if (OKBufferIndex - 1 > 0)
				{
					OKBufferIndex--;
				}
				else
				{
					OKBufferIndex = inspection.OKBufferList.Count - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				inspection.SetBuffers(inspection.OKBufferList[OKBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (NGBufferIndex < 0 || inspection.NGBufferList.Count <= 0)
				{
					return;
				}
				if (NGBufferIndex - 1 > 0)
				{
					NGBufferIndex--;
				}
				else
				{
					NGBufferIndex = inspection.NGBufferList.Count - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat);
			SetBuffer(inspection.buffers.Src);
			Btn_Test_Click(this, null);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-BinaryWindow-Btn_TestPrevImg_Click()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TestNextImg_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			default:
				return;
			case 0:
				return;
			case 1:
				if (filelist == null || filelist.Length == 0)
				{
					return;
				}
				if (fileIndex < 0)
				{
					fileIndex = 0;
				}
				if (fileIndex + 1 < filelist.Length)
				{
					fileIndex++;
				}
				else
				{
					fileIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
				mat = CVBuffer.ImRead(filelist[fileIndex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (RecentBufferIndex < 0 || inspection.RecentBufferList.Count <= 0)
				{
					return;
				}
				if (RecentBufferIndex + 1 < inspection.RecentBufferList.Count)
				{
					RecentBufferIndex++;
				}
				else
				{
					RecentBufferIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				inspection.SetBuffers(inspection.RecentBufferList[RecentBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (OKBufferIndex < 0 || inspection.OKBufferList.Count <= 0)
				{
					return;
				}
				if (OKBufferIndex + 1 < inspection.OKBufferList.Count)
				{
					OKBufferIndex++;
				}
				else
				{
					OKBufferIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				inspection.SetBuffers(inspection.OKBufferList[OKBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (NGBufferIndex < 0 || inspection.NGBufferList.Count <= 0)
				{
					return;
				}
				if (NGBufferIndex + 1 < inspection.NGBufferList.Count)
				{
					NGBufferIndex++;
				}
				else
				{
					NGBufferIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat);
			SetBuffer(inspection.buffers.Src);
			Btn_Test_Click(this, null);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-BinaryWindow-Btn_TestNextImg_Click()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public Encoding GetFileEncodeType(string filename)
	{
		using FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
		byte[] array = new BinaryReader(input).ReadBytes(2);
		if (array[0] >= 239)
		{
			if (array[0] == 239 && array[1] == 187)
			{
				return Encoding.UTF8;
			}
			if (array[0] == 254 && array[1] == byte.MaxValue)
			{
				return Encoding.BigEndianUnicode;
			}
			if (array[0] == byte.MaxValue && array[1] == 254)
			{
				return Encoding.Unicode;
			}
			return Encoding.Default;
		}
		return Encoding.Default;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CopyModel(string ModelName)
	{
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string path2 = ModelName + ".lh5d";
			string text2 = System.IO.Path.Combine(path, path2);
			string text3 = System.IO.Path.Combine(text, "navs.h5");
			if (File.Exists(text3))
			{
				FileInfo fileInfo = new FileInfo(text3);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text3);
			}
			if (!File.Exists(text2))
			{
				return false;
			}
			File.Copy(text2, text3, overwrite: true);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("LocateWindow-CopyModel()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CopyNewModel(string ModelName)
	{
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arg = string.Format("{0}", System.IO.Path.GetFileNameWithoutExtension(ModelName));
			string path2 = string.Format("{0}.lh5d", arg);
			string text2 = System.IO.Path.Combine(path, path2);
			string text3 = System.IO.Path.Combine(text, "navs.h5");
			if (!File.Exists(text3))
			{
				return false;
			}
			FileInfo fileInfo = new FileInfo(text3);
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
			if (File.Exists(text2))
			{
				FileInfo fileInfo2 = new FileInfo(text2);
				if (fileInfo2.IsReadOnly)
				{
					fileInfo2.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			File.Copy(text3, text2, overwrite: true);
			string path3 = string.Format("{0}.ltrt", arg);
			string path4 = System.IO.Path.Combine(path, path3);
			string path5 = string.Format("{0}.lpbd", arg);
			string path6 = System.IO.Path.Combine(path, path5);
			if (File.Exists(path4))
			{
				File.Delete(path4);
			}
			if (File.Exists(path6))
			{
				File.Delete(path6);
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CopyNewModel()" + ex.ToString());
			return false;
		}
	}

	public void SetAuthLevel(int AuthLevel)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/locatewindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	void IComponentConnector.InitializeComponent()
	{
		//ILSpy generated this explicit interface implementation from .override directive in InitializeComponent
		this.InitializeComponent();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			MainW = (LocateWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			cnv_Image.PreviewMouseRightButtonDown += Cnv_Image_PreviewMouseRightButtonDown;
			break;
		case 3:
			sfr = (ScaleTransform)target;
			break;
		case 4:
			br_LocateRect = (Border)target;
			break;
		case 5:
			txt_LocateRect = (TextBlock)target;
			break;
		case 6:
			Label_Result = (Label)target;
			break;
		case 7:
			bor_AILocateRect = (Rectangle)target;
			break;
		case 8:
			bor_SearchRectL = (Border)target;
			bor_SearchRectL.PreviewMouseLeftButtonDown += Bor_SearchRectL_PreviewMouseLeftButtonDown;
			bor_SearchRectL.GotFocus += Bor_SearchRectL_GotFocus;
			break;
		case 9:
			txt_Search1Label1 = (TextBlock)target;
			break;
		case 10:
			bor_SearchRectT = (Border)target;
			bor_SearchRectT.PreviewMouseLeftButtonDown += Bor_SearchRectT_PreviewMouseLeftButtonDown;
			bor_SearchRectT.GotFocus += Bor_SearchRectT_GotFocus;
			break;
		case 11:
			txt_Search1Label2 = (TextBlock)target;
			break;
		case 12:
			bor_SearchRectR = (Border)target;
			bor_SearchRectR.PreviewMouseLeftButtonDown += Bor_SearchRectR_PreviewMouseLeftButtonDown;
			bor_SearchRectR.GotFocus += Bor_SearchRectR_GotFocus;
			break;
		case 13:
			txt_Search1Label3 = (TextBlock)target;
			break;
		case 14:
			bor_SearchRectB = (Border)target;
			bor_SearchRectB.PreviewMouseLeftButtonDown += Bor_SearchRectB_PreviewMouseLeftButtonDown;
			bor_SearchRectB.GotFocus += Bor_SearchRectB_GotFocus;
			break;
		case 15:
			txt_Search1Label4 = (TextBlock)target;
			break;
		case 16:
			bor_MaxSearchRect = (Border)target;
			break;
		case 17:
			txt_Search1Label5 = (TextBlock)target;
			break;
		case 18:
			bor_SearchMaskT = (Border)target;
			bor_SearchMaskT.PreviewMouseLeftButtonDown += Bor_SearchRectT_PreviewMouseLeftButtonDown;
			break;
		case 19:
			txt_Search1Label6 = (TextBlock)target;
			break;
		case 20:
			bor_SearchMaskB = (Border)target;
			bor_SearchMaskB.PreviewMouseLeftButtonDown += Bor_SearchRectT_PreviewMouseLeftButtonDown;
			break;
		case 21:
			txt_Search1Label7 = (TextBlock)target;
			break;
		case 22:
			bor_SearchMaskL = (Border)target;
			bor_SearchMaskL.PreviewMouseLeftButtonDown += Bor_SearchRectT_PreviewMouseLeftButtonDown;
			break;
		case 23:
			txt_Search1Label8 = (TextBlock)target;
			break;
		case 24:
			bor_SearchMaskR = (Border)target;
			bor_SearchMaskR.PreviewMouseLeftButtonDown += Bor_SearchRectT_PreviewMouseLeftButtonDown;
			break;
		case 25:
			txt_Search1Label9 = (TextBlock)target;
			break;
		case 26:
			txt_PreviewMode = (TextBox)target;
			break;
		case 27:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 28:
			lab_AutoLocateHint = (Label)target;
			break;
		case 29:
			btn_Ruler = (Button)target;
			btn_Ruler.Click += btn_Ruler_Click;
			break;
		case 30:
			bor_SIMPSearchRect = (Border)target;
			break;
		case 31:
			txt_Search1Label = (TextBlock)target;
			break;
		case 32:
			tab_LocateType = (TabControl)target;
			tab_LocateType.SelectionChanged += Tab_LocateType_SelectionChanged;
			break;
		case 33:
			tab_SIMP = (TabItem)target;
			break;
		case 34:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 35:
			rs_SIMP_THD1 = (RangeSlider)target;
			rs_SIMP_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_SIMP_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_SIMP_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 36:
			cb_SIMP_THD1_Channel = (ComboBox)target;
			break;
		case 37:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 38:
			cb_THD1_DEStart = (ComboBox)target;
			break;
		case 39:
			cb_THD1_DEEnd = (ComboBox)target;
			break;
		case 40:
			cb_THD1_DEEndCount = (ComboBox)target;
			break;
		case 41:
			cb_THD1_DEStartCount = (ComboBox)target;
			break;
		case 42:
			btn_ColorDropperSIMP = (Button)target;
			btn_ColorDropperSIMP.Click += Btn_ColorDropperL_Click;
			break;
		case 43:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 44:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 45:
			btn_OTSU_Easy = (Button)target;
			btn_OTSU_Easy.Click += Btn_OTSU_Easy_Click;
			break;
		case 46:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 47:
			cb_SIMP_SideLocate = (CheckBox)target;
			cb_SIMP_SideLocate.Click += Cb_LocateSpeedUp_Click;
			break;
		case 48:
			exp_THD2 = (Expander)target;
			break;
		case 49:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 50:
			rs_SIMP_THD2 = (RangeSlider)target;
			rs_SIMP_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_SIMP_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_SIMP_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 51:
			grd_SIMP_DilateErode9 = (Grid)target;
			break;
		case 52:
			cb_THD2_DEStart = (ComboBox)target;
			break;
		case 53:
			cb_THD2_DEEnd = (ComboBox)target;
			break;
		case 54:
			cb_THD2_DEEndCount = (ComboBox)target;
			break;
		case 55:
			cb_THD2_DEStartCount = (ComboBox)target;
			break;
		case 56:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 57:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 58:
			cb_SIMP_THD2_Channel = (ComboBox)target;
			break;
		case 59:
			exp_THD3 = (Expander)target;
			break;
		case 60:
			grd_SIMP_THD_4 = (Grid)target;
			break;
		case 61:
			rs_SIMP_THD3 = (RangeSlider)target;
			rs_SIMP_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_SIMP_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_SIMP_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 62:
			grd_SIMP_DilateErode10 = (Grid)target;
			break;
		case 63:
			cb_THD3_DEStart = (ComboBox)target;
			break;
		case 64:
			cb_THD3_DEEnd = (ComboBox)target;
			break;
		case 65:
			cb_THD3_DEEndCount = (ComboBox)target;
			break;
		case 66:
			cb_THD3_DEStartCount = (ComboBox)target;
			break;
		case 67:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 68:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 69:
			cb_SIMP_THD3_Channel = (ComboBox)target;
			break;
		case 70:
			tab_QUAD = (TabItem)target;
			break;
		case 71:
			grd_QUAD_THD_L = (Grid)target;
			break;
		case 72:
			rs_QuadTHDValueL = (RangeSlider)target;
			rs_QuadTHDValueL.LowerThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueL.UpperThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueL.CentralThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueL.PreviewMouseLeftButtonUp += Rs_QuadTHDValueL_PreviewMouseLeftButtonUp;
			break;
		case 73:
			txt_QuadTHDMinL = (TextBox)target;
			break;
		case 74:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 75:
			cb_Quad_DEStartL = (ComboBox)target;
			break;
		case 76:
			cb_Quad_DEEndL = (ComboBox)target;
			break;
		case 77:
			cb_Quad_DEEndCountL = (ComboBox)target;
			break;
		case 78:
			cb_Quad_DEStartCountL = (ComboBox)target;
			break;
		case 79:
			txt_QuadTHDMaxL = (TextBox)target;
			break;
		case 80:
			cb_QuadChannelL = (ComboBox)target;
			break;
		case 81:
			rad_QuadScanL2R_L = (RadioButton)target;
			break;
		case 82:
			rad_QuadScanR2L_L = (RadioButton)target;
			break;
		case 83:
			btn_ScanL2RW1 = (Button)target;
			break;
		case 84:
			btn_ScanR2LW1 = (Button)target;
			break;
		case 85:
			btn_QuadTrackerReset = (Button)target;
			btn_QuadTrackerReset.Click += Btn_QuadTrackerReset_Click;
			break;
		case 86:
			btn_PreviewScanL = (Button)target;
			btn_PreviewScanL.Click += Btn_PreviewScanL_Click;
			break;
		case 87:
			btn_PreviewQuadThdL = (Button)target;
			btn_PreviewQuadThdL.Click += Btn_PreviewQuadThdL_Click;
			break;
		case 88:
			btn_ColorDropperL = (Button)target;
			btn_ColorDropperL.Click += Btn_ColorDropperL_Click;
			break;
		case 89:
			cb_EnableMaskL = (CheckBox)target;
			cb_EnableMaskL.Click += Cb_EnableMaskL_Click;
			break;
		case 90:
			cb_EnableAvgEdgeL = (CheckBox)target;
			break;
		case 91:
			cb_Quad_EnableRGBThd_L = (CheckBox)target;
			cb_Quad_EnableRGBThd_L.Click += Cb_Quad_EnableRGBThd_L_Click_1;
			break;
		case 92:
			grd_Quad_RGBThd_L = (Grid)target;
			break;
		case 93:
			rs_Quad_THDR_L = (RangeSlider)target;
			rs_Quad_THDR_L.CentralThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_L.PreviewMouseLeftButtonUp += Rs_Quad_THDR_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDR_L.UpperThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_L.LowerThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_L.ValueChanged += Rs_Quad_THDR_L_ValueChanged;
			break;
		case 94:
			rs_Quad_THDG_L = (RangeSlider)target;
			rs_Quad_THDG_L.LowerThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_L.CentralThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_L.PreviewMouseLeftButtonUp += Rs_Quad_THDG_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDG_L.UpperThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			break;
		case 95:
			rs_Quad_THDB_L = (RangeSlider)target;
			rs_Quad_THDB_L.LowerThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_L.UpperThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_L.CentralThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_L.PreviewMouseLeftButtonUp += Rs_Quad_THDB_L_PreviewMouseLeftButtonUp;
			break;
		case 96:
			lab_Quad_THDR_Min_L = (Label)target;
			break;
		case 97:
			lab_Quad_THDR_Max_L = (Label)target;
			break;
		case 98:
			lab_Quad_THDG_Min_L = (Label)target;
			break;
		case 99:
			lab_Quad_THDG_Max_L = (Label)target;
			break;
		case 100:
			lab_Quad_THDB_Min_L = (Label)target;
			break;
		case 101:
			lab_Quad_THDB_Max_L = (Label)target;
			break;
		case 102:
			btn_OTSU_L = (Button)target;
			btn_OTSU_L.Click += Btn_OTSUL_Click;
			break;
		case 103:
			grd_QUAD_THD_R = (Grid)target;
			break;
		case 104:
			rs_QuadTHDValueR = (RangeSlider)target;
			rs_QuadTHDValueR.LowerThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueR.UpperThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueR.CentralThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueR.PreviewMouseLeftButtonUp += Rs_QuadTHDValueL_PreviewMouseLeftButtonUp;
			break;
		case 105:
			txt_QuadTHDMinR = (TextBox)target;
			break;
		case 106:
			grd_SIMP_DilateErode4 = (Grid)target;
			break;
		case 107:
			cb_Quad_DEStartR = (ComboBox)target;
			break;
		case 108:
			cb_Quad_DEEndR = (ComboBox)target;
			break;
		case 109:
			cb_Quad_DEEndCountR = (ComboBox)target;
			break;
		case 110:
			cb_Quad_DEStartCountR = (ComboBox)target;
			break;
		case 111:
			txt_QuadTHDMaxR = (TextBox)target;
			break;
		case 112:
			cb_QuadChannelR = (ComboBox)target;
			break;
		case 113:
			rad_QuadScanL2R_R = (RadioButton)target;
			break;
		case 114:
			rad_QuadScanR2L_R = (RadioButton)target;
			break;
		case 115:
			btn_ScanL2RW2 = (Button)target;
			break;
		case 116:
			btn_ScanR2LW2 = (Button)target;
			break;
		case 117:
			btn_QuadTrackerReset_Copy = (Button)target;
			btn_QuadTrackerReset_Copy.Click += Btn_QuadTrackerReset_Click;
			break;
		case 118:
			btn_PreviewScanL_Copy = (Button)target;
			btn_PreviewScanL_Copy.Click += Btn_PreviewScanL_Click;
			break;
		case 119:
			btn_PreviewQuadThdL_Copy = (Button)target;
			btn_PreviewQuadThdL_Copy.Click += Btn_PreviewQuadThdL_Click;
			break;
		case 120:
			btn_ColorDropperR = (Button)target;
			btn_ColorDropperR.Click += Btn_ColorDropperL_Click;
			break;
		case 121:
			cb_EnableMaskR = (CheckBox)target;
			cb_EnableMaskR.Click += Cb_EnableMaskR_Click;
			break;
		case 122:
			cb_EnableAvgEdgeR = (CheckBox)target;
			break;
		case 123:
			cb_Quad_EnableRGBThd_R = (CheckBox)target;
			cb_Quad_EnableRGBThd_R.Click += Cb_Quad_EnableRGBThd_R_Click;
			break;
		case 124:
			grd_Quad_RGBThd_R = (Grid)target;
			break;
		case 125:
			lab_Quad_THDR_Min_R = (Label)target;
			break;
		case 126:
			lab_Quad_THDR_Max_R = (Label)target;
			break;
		case 127:
			lab_Quad_THDG_Min_R = (Label)target;
			break;
		case 128:
			lab_Quad_THDG_Max_R = (Label)target;
			break;
		case 129:
			lab_Quad_THDB_Min_R = (Label)target;
			break;
		case 130:
			lab_Quad_THDB_Max_R = (Label)target;
			break;
		case 131:
			rs_Quad_THDR_R = (RangeSlider)target;
			rs_Quad_THDR_R.CentralThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_R.PreviewMouseLeftButtonUp += Rs_Quad_THDR_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDR_R.UpperThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_R.LowerThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			break;
		case 132:
			rs_Quad_THDG_R = (RangeSlider)target;
			rs_Quad_THDG_R.LowerThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_R.CentralThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_R.PreviewMouseLeftButtonUp += Rs_Quad_THDG_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDG_R.UpperThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			break;
		case 133:
			rs_Quad_THDB_R = (RangeSlider)target;
			rs_Quad_THDB_R.LowerThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_R.UpperThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_R.CentralThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_R.PreviewMouseLeftButtonUp += Rs_Quad_THDB_L_PreviewMouseLeftButtonUp;
			break;
		case 134:
			btn_OTSU_R = (Button)target;
			btn_OTSU_R.Click += Btn_OTSUL_Click;
			break;
		case 135:
			grd_QUAD_THD_T = (Grid)target;
			break;
		case 136:
			rs_QuadTHDValueT = (RangeSlider)target;
			rs_QuadTHDValueT.LowerThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueT.UpperThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueT.CentralThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueT.PreviewMouseLeftButtonUp += Rs_QuadTHDValueL_PreviewMouseLeftButtonUp;
			break;
		case 137:
			txt_QuadTHDMinT = (TextBox)target;
			break;
		case 138:
			grd_SIMP_DilateErode5 = (Grid)target;
			break;
		case 139:
			cb_Quad_DEStartT = (ComboBox)target;
			break;
		case 140:
			cb_Quad_DEEndT = (ComboBox)target;
			break;
		case 141:
			cb_Quad_DEEndCountT = (ComboBox)target;
			break;
		case 142:
			cb_Quad_DEStartCountT = (ComboBox)target;
			break;
		case 143:
			txt_QuadTHDMaxT = (TextBox)target;
			break;
		case 144:
			cb_QuadChannelT = (ComboBox)target;
			break;
		case 145:
			rad_QuadScanT2B_T = (RadioButton)target;
			break;
		case 146:
			rad_QuadScanB2T_T = (RadioButton)target;
			break;
		case 147:
			btn_ScanL2RW3 = (Button)target;
			break;
		case 148:
			btn_ScanR2LW3 = (Button)target;
			break;
		case 149:
			btn_QuadTrackerReset_Copy1 = (Button)target;
			btn_QuadTrackerReset_Copy1.Click += Btn_QuadTrackerReset_Click;
			break;
		case 150:
			btn_PreviewScanL_Copy1 = (Button)target;
			btn_PreviewScanL_Copy1.Click += Btn_PreviewScanL_Click;
			break;
		case 151:
			btn_PreviewQuadThdL_Copy1 = (Button)target;
			btn_PreviewQuadThdL_Copy1.Click += Btn_PreviewQuadThdL_Click;
			break;
		case 152:
			btn_ColorDropperT = (Button)target;
			btn_ColorDropperT.Click += Btn_ColorDropperL_Click;
			break;
		case 153:
			cb_EnableMaskT = (CheckBox)target;
			cb_EnableMaskT.Click += Cb_EnableMaskT_Click;
			break;
		case 154:
			cb_EnableAvgEdgeT = (CheckBox)target;
			break;
		case 155:
			cb_Quad_EnableRGBThd_T = (CheckBox)target;
			cb_Quad_EnableRGBThd_T.Click += Cb_Quad_EnableRGBThd_T_Click;
			break;
		case 156:
			grd_Quad_RGBThd_T = (Grid)target;
			break;
		case 157:
			lab_Quad_THDR_Min_T = (Label)target;
			break;
		case 158:
			lab_Quad_THDR_Max_T = (Label)target;
			break;
		case 159:
			lab_Quad_THDG_Min_T = (Label)target;
			break;
		case 160:
			lab_Quad_THDG_Max_T = (Label)target;
			break;
		case 161:
			lab_Quad_THDB_Min_T = (Label)target;
			break;
		case 162:
			lab_Quad_THDB_Max_T = (Label)target;
			break;
		case 163:
			rs_Quad_THDR_T = (RangeSlider)target;
			rs_Quad_THDR_T.CentralThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_T.PreviewMouseLeftButtonUp += Rs_Quad_THDR_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDR_T.UpperThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_T.LowerThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			break;
		case 164:
			rs_Quad_THDG_T = (RangeSlider)target;
			rs_Quad_THDG_T.LowerThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_T.CentralThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_T.PreviewMouseLeftButtonUp += Rs_Quad_THDG_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDG_T.UpperThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			break;
		case 165:
			rs_Quad_THDB_T = (RangeSlider)target;
			rs_Quad_THDB_T.LowerThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_T.UpperThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_T.CentralThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_T.PreviewMouseLeftButtonUp += Rs_Quad_THDB_L_PreviewMouseLeftButtonUp;
			break;
		case 166:
			btn_OTSU_T = (Button)target;
			btn_OTSU_T.Click += Btn_OTSUL_Click;
			break;
		case 167:
			grd_QUAD_THD_B = (Grid)target;
			break;
		case 168:
			rs_QuadTHDValueB = (RangeSlider)target;
			rs_QuadTHDValueB.LowerThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueB.UpperThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueB.CentralThumbDragDelta += Rs_QuadTHDValueL_ThumbDragDelta;
			rs_QuadTHDValueB.PreviewMouseLeftButtonUp += Rs_QuadTHDValueL_PreviewMouseLeftButtonUp;
			break;
		case 169:
			txt_QuadTHDMinB = (TextBox)target;
			break;
		case 170:
			grd_SIMP_DilateErode6 = (Grid)target;
			break;
		case 171:
			cb_Quad_DEStartB = (ComboBox)target;
			break;
		case 172:
			cb_Quad_DEEndB = (ComboBox)target;
			break;
		case 173:
			cb_Quad_DEEndCountB = (ComboBox)target;
			break;
		case 174:
			cb_Quad_DEStartCountB = (ComboBox)target;
			break;
		case 175:
			txt_QuadTHDMaxB = (TextBox)target;
			break;
		case 176:
			cb_QuadChannelB = (ComboBox)target;
			break;
		case 177:
			rad_QuadScanT2B_B = (RadioButton)target;
			break;
		case 178:
			rad_QuadScanB2T_B = (RadioButton)target;
			break;
		case 179:
			btn_ScanL2RW4 = (Button)target;
			break;
		case 180:
			btn_ScanR2LW4 = (Button)target;
			break;
		case 181:
			btn_QuadTrackerReset_Copy2 = (Button)target;
			btn_QuadTrackerReset_Copy2.Click += Btn_QuadTrackerReset_Click;
			break;
		case 182:
			btn_PreviewScanL_Copy2 = (Button)target;
			btn_PreviewScanL_Copy2.Click += Btn_PreviewScanL_Click;
			break;
		case 183:
			btn_PreviewQuadThdL_Copy2 = (Button)target;
			btn_PreviewQuadThdL_Copy2.Click += Btn_PreviewQuadThdL_Click;
			break;
		case 184:
			btn_ColorDropperB = (Button)target;
			btn_ColorDropperB.Click += Btn_ColorDropperL_Click;
			break;
		case 185:
			cb_EnableMaskB = (CheckBox)target;
			cb_EnableMaskB.Click += Cb_EnableMaskB_Click;
			break;
		case 186:
			cb_EnableAvgEdgeB = (CheckBox)target;
			break;
		case 187:
			cb_Quad_EnableRGBThd_B = (CheckBox)target;
			cb_Quad_EnableRGBThd_B.Click += Cb_Quad_EnableRGBThd_B_Click;
			break;
		case 188:
			grd_Quad_RGBThd_B = (Grid)target;
			break;
		case 189:
			lab_Quad_THDR_Min_B = (Label)target;
			break;
		case 190:
			lab_Quad_THDR_Max_B = (Label)target;
			break;
		case 191:
			lab_Quad_THDG_Min_B = (Label)target;
			break;
		case 192:
			lab_Quad_THDG_Max_B = (Label)target;
			break;
		case 193:
			lab_Quad_THDB_Min_B = (Label)target;
			break;
		case 194:
			lab_Quad_THDB_Max_B = (Label)target;
			break;
		case 195:
			rs_Quad_THDR_B = (RangeSlider)target;
			rs_Quad_THDR_B.CentralThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_B.PreviewMouseLeftButtonUp += Rs_Quad_THDR_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDR_B.UpperThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			rs_Quad_THDR_B.LowerThumbDragDelta += Rs_Quad_THDR_L_CentralThumbDragDelta;
			break;
		case 196:
			rs_Quad_THDG_B = (RangeSlider)target;
			rs_Quad_THDG_B.LowerThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_B.CentralThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			rs_Quad_THDG_B.PreviewMouseLeftButtonUp += Rs_Quad_THDG_L_PreviewMouseLeftButtonUp;
			rs_Quad_THDG_B.UpperThumbDragDelta += Rs_Quad_THDG_L_CentralThumbDragDelta;
			break;
		case 197:
			rs_Quad_THDB_B = (RangeSlider)target;
			rs_Quad_THDB_B.LowerThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_B.UpperThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_B.CentralThumbDragDelta += Rs_Quad_THDB_L_CentralThumbDragDelta;
			rs_Quad_THDB_B.PreviewMouseLeftButtonUp += Rs_Quad_THDB_L_PreviewMouseLeftButtonUp;
			break;
		case 198:
			btn_OTSU_B = (Button)target;
			btn_OTSU_B.Click += Btn_OTSUL_Click;
			break;
		case 199:
			chk_UseMelfLocate = (CheckBox)target;
			break;
		case 200:
			chk_UseFilterPoint = (CheckBox)target;
			break;
		case 201:
			chk_UseStdFilter = (CheckBox)target;
			break;
		case 202:
			chk_UseSideLocate = (CheckBox)target;
			break;
		case 203:
			cb_Quad_LocateAngleSelect = (ComboBox)target;
			break;
		case 204:
			lab_LocateExtend_Copy = (Label)target;
			break;
		case 205:
			chk_FixSideLocateLR = (CheckBox)target;
			break;
		case 206:
			cb_QUAD_SideLocate = (CheckBox)target;
			cb_QUAD_SideLocate.Click += Cb_LocateSpeedUp_Click;
			break;
		case 207:
			lab_LocateExtend_Copy1 = (Label)target;
			break;
		case 208:
			tab_PAD = (TabItem)target;
			break;
		case 209:
			grd_Pad_THD_L = (Grid)target;
			break;
		case 210:
			rs_PadTHDValueL1 = (RangeSlider)target;
			rs_PadTHDValueL1.LowerThumbDragDelta += Rs_PadTHDValueL1_CentralThumbDragDelta;
			rs_PadTHDValueL1.UpperThumbDragDelta += Rs_PadTHDValueL1_CentralThumbDragDelta;
			rs_PadTHDValueL1.CentralThumbDragDelta += Rs_PadTHDValueL1_CentralThumbDragDelta;
			rs_PadTHDValueL1.PreviewMouseLeftButtonUp += Rs_PadTHDValueL1_PreviewMouseLeftButtonUp;
			break;
		case 211:
			txt_PadTHDMinL1 = (TextBox)target;
			break;
		case 212:
			grd_SIMP_DilateErode7 = (Grid)target;
			break;
		case 213:
			cb_Pad_DEStartL1 = (ComboBox)target;
			break;
		case 214:
			cb_Pad_DEEndL1 = (ComboBox)target;
			break;
		case 215:
			cb_Pad_DEEndCountL1 = (ComboBox)target;
			break;
		case 216:
			cb_Pad_DEStartCountL1 = (ComboBox)target;
			break;
		case 217:
			txt_PadTHDMaxL1 = (TextBox)target;
			break;
		case 218:
			cb_PadChannelL1 = (ComboBox)target;
			break;
		case 219:
			btn_QuadTrackerReset1 = (Button)target;
			btn_QuadTrackerReset1.Click += Btn_QuadTrackerReset_Click;
			break;
		case 220:
			btn_PreviewPadThdL = (Button)target;
			btn_PreviewPadThdL.Click += Btn_PreviewPadThdL_Click;
			break;
		case 221:
			btn_ColorDropperL1 = (Button)target;
			btn_ColorDropperL1.Click += Btn_ColorDropperL_Click;
			break;
		case 222:
			rs_PadTHDValueL2 = (RangeSlider)target;
			rs_PadTHDValueL2.LowerThumbDragDelta += Rs_PadTHDValueL1_CentralThumbDragDelta;
			rs_PadTHDValueL2.UpperThumbDragDelta += Rs_PadTHDValueL1_CentralThumbDragDelta;
			rs_PadTHDValueL2.CentralThumbDragDelta += Rs_PadTHDValueL1_CentralThumbDragDelta;
			rs_PadTHDValueL2.PreviewMouseLeftButtonUp += Rs_PadTHDValueL1_PreviewMouseLeftButtonUp;
			break;
		case 223:
			txt_PadTHDMinL2 = (TextBox)target;
			break;
		case 224:
			grd_SIMP_DilateErode7_Copy = (Grid)target;
			break;
		case 225:
			cb_Pad_DEStartL2 = (ComboBox)target;
			break;
		case 226:
			cb_Pad_DEEndL2 = (ComboBox)target;
			break;
		case 227:
			cb_Pad_DEEndCountL2 = (ComboBox)target;
			break;
		case 228:
			cb_Pad_DEStartCountL2 = (ComboBox)target;
			break;
		case 229:
			txt_PadTHDMaxL2 = (TextBox)target;
			break;
		case 230:
			cb_PadChannelL2 = (ComboBox)target;
			break;
		case 231:
			grd_Pad_THD_R = (Grid)target;
			break;
		case 232:
			rs_PadTHDValueR1 = (RangeSlider)target;
			rs_PadTHDValueR1.LowerThumbDragDelta += Rs_PadTHDValueR1_CentralThumbDragDelta;
			rs_PadTHDValueR1.UpperThumbDragDelta += Rs_PadTHDValueR1_CentralThumbDragDelta;
			rs_PadTHDValueR1.CentralThumbDragDelta += Rs_PadTHDValueR1_CentralThumbDragDelta;
			rs_PadTHDValueR1.PreviewMouseLeftButtonUp += Rs_PadTHDValueL1_PreviewMouseLeftButtonUp;
			break;
		case 233:
			txt_PadTHDMinR1 = (TextBox)target;
			break;
		case 234:
			grd_SIMP_DilateErode8 = (Grid)target;
			break;
		case 235:
			cb_Pad_DEStartR1 = (ComboBox)target;
			break;
		case 236:
			cb_Pad_DEEndR1 = (ComboBox)target;
			break;
		case 237:
			cb_Pad_DEEndCountR1 = (ComboBox)target;
			break;
		case 238:
			cb_Pad_DEStartCountR1 = (ComboBox)target;
			break;
		case 239:
			txt_PadTHDMaxR1 = (TextBox)target;
			break;
		case 240:
			cb_PadChannelR1 = (ComboBox)target;
			break;
		case 241:
			btn_QuadTrackerReset2 = (Button)target;
			btn_QuadTrackerReset2.Click += Btn_QuadTrackerReset_Click;
			break;
		case 242:
			btn_PreviewPadThdR = (Button)target;
			btn_PreviewPadThdR.Click += Btn_PreviewPadThdR_Click;
			break;
		case 243:
			btn_ColorDropperL2 = (Button)target;
			btn_ColorDropperL2.Click += Btn_ColorDropperL_Click;
			break;
		case 244:
			rs_PadTHDValueR2 = (RangeSlider)target;
			rs_PadTHDValueR2.LowerThumbDragDelta += Rs_PadTHDValueR1_CentralThumbDragDelta;
			rs_PadTHDValueR2.UpperThumbDragDelta += Rs_PadTHDValueR1_CentralThumbDragDelta;
			rs_PadTHDValueR2.CentralThumbDragDelta += Rs_PadTHDValueR1_CentralThumbDragDelta;
			rs_PadTHDValueR2.PreviewMouseLeftButtonUp += Rs_PadTHDValueL1_PreviewMouseLeftButtonUp;
			break;
		case 245:
			txt_PadTHDMinR2 = (TextBox)target;
			break;
		case 246:
			grd_SIMP_DilateErode7_Copy1 = (Grid)target;
			break;
		case 247:
			cb_Pad_DEStartR2 = (ComboBox)target;
			break;
		case 248:
			cb_Pad_DEEndR2 = (ComboBox)target;
			break;
		case 249:
			cb_Pad_DEEndCountR2 = (ComboBox)target;
			break;
		case 250:
			cb_Pad_DEStartCountR2 = (ComboBox)target;
			break;
		case 251:
			txt_PadTHDMaxR2 = (TextBox)target;
			break;
		case 252:
			cb_PadChannelR2 = (ComboBox)target;
			break;
		case 253:
			tab_MELF = (TabItem)target;
			break;
		case 254:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 255:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 256:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 257:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 258:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 259:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 260:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 261:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 262:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 263:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 264:
			rt_AutoLocateHint = (Rectangle)target;
			break;
		case 265:
			btn_Img_AILR = (Button)target;
			btn_Img_AILR.Click += Btn_ImgSrc_Click;
			break;
		case 266:
			btn_Img_AILG = (Button)target;
			btn_Img_AILG.Click += Btn_ImgSrc_Click;
			break;
		case 267:
			btn_Img_AILB = (Button)target;
			btn_Img_AILB.Click += Btn_ImgSrc_Click;
			break;
		case 268:
			btn_ImageNum = (Button)target;
			break;
		case 269:
			btn_Img_AILRGB = (Button)target;
			btn_Img_AILRGB.Click += Btn_ImgSrc_Click;
			break;
		case 270:
			lab_RValue = (Label)target;
			break;
		case 271:
			lab_GValue = (Label)target;
			break;
		case 272:
			lab_BValue = (Label)target;
			break;
		case 273:
			lab_XValue = (Label)target;
			break;
		case 274:
			lab_YValue = (Label)target;
			break;
		case 275:
			lab_HValue = (Label)target;
			break;
		case 276:
			lab_SValue = (Label)target;
			break;
		case 277:
			lab_VValue = (Label)target;
			break;
		case 278:
			lab_GrayValue = (Label)target;
			break;
		case 279:
			btn_AutoLocate = (Button)target;
			btn_AutoLocate.Click += Btn_AutoLocate_Click;
			break;
		case 280:
			txt_AllResult = (TextBox)target;
			break;
		case 281:
			grd_SIMP_Setup = (Grid)target;
			break;
		case 282:
			txt_RotatLimitAngle = (TextBox)target;
			txt_RotatLimitAngle.PreviewMouseDoubleClick += Txt_RotatLimitAngle_PreviewMouseDoubleClick;
			break;
		case 283:
			txt_RotatWidthMin = (TextBox)target;
			txt_RotatWidthMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 284:
			txt_RotatWidthMax = (TextBox)target;
			txt_RotatWidthMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 285:
			txt_RotatHeightMin = (TextBox)target;
			txt_RotatHeightMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 286:
			txt_RotatHeightMax = (TextBox)target;
			txt_RotatHeightMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 287:
			cb_RotatErrorBin = (ComboBox)target;
			break;
		case 288:
			lab_doRotateMinAngle = (Label)target;
			break;
		case 289:
			txt_doRotateMinAngle = (TextBox)target;
			break;
		case 290:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 291:
			txt_RotatAngleResult = (TextBox)target;
			break;
		case 292:
			txt_RotatWidthResult = (TextBox)target;
			break;
		case 293:
			txt_RotatHeightResult = (TextBox)target;
			break;
		case 294:
			rt_Result = (Rectangle)target;
			break;
		case 295:
			cb_LocateSpeedUp = (CheckBox)target;
			cb_LocateSpeedUp.Click += Cb_LocateSpeedUp_Click;
			break;
		case 296:
			lab_Pareto = (Label)target;
			break;
		case 297:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 298:
			btn_ChartW = (Button)target;
			btn_ChartW.Click += Btn_Chart_Click;
			break;
		case 299:
			btn_ChartH = (Button)target;
			btn_ChartH.Click += Btn_Chart_Click;
			break;
		case 300:
			btn_ChartA = (Button)target;
			btn_ChartA.Click += Btn_Chart_Click;
			break;
		case 301:
			chk_CreateHSVChannel = (CheckBox)target;
			chk_CreateHSVChannel.Click += Chk_CreateHSVChannel_Click;
			break;
		case 302:
			cb_SizeErrorBin = (ComboBox)target;
			break;
		case 303:
			chk_EnableFocusCheck = (CheckBox)target;
			break;
		case 304:
			txt_FocusLimit = (TextBox)target;
			txt_FocusLimit.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 305:
			txt_FocusLimitResult = (TextBox)target;
			break;
		case 306:
			btn_Reverse = (Button)target;
			btn_Reverse.Click += Btn_Reverse_Click;
			break;
		case 307:
			btn_AILocate = (Button)target;
			btn_AILocate.Click += btn_AILocate_Click;
			break;
		case 308:
			cb_EnableAILocate = (CheckBox)target;
			cb_EnableAILocate.Click += cb_EnableAILocate_Click;
			break;
		case 309:
			lab_Ruler = (Label)target;
			break;
		case 310:
			grd_AILocate = (Grid)target;
			break;
		case 311:
			btn_SetModel = (Button)target;
			btn_SetModel.Click += Btn_SetModel_Click;
			break;
		case 312:
			txt_ModeName = (TextBox)target;
			break;
		case 313:
			chk_EnableTensorRT = (CheckBox)target;
			chk_EnableTensorRT.Click += chk_EnableTensorRT_Click;
			break;
		case 314:
			btn_Draw = (Button)target;
			btn_Draw.Click += btn_Draw_Click;
			break;
		case 315:
			txt_TrainCount = (TextBox)target;
			break;
		case 316:
			btn_Train = (Button)target;
			btn_Train.Click += btn_Train_Click;
			break;
		case 317:
			cnv_SourceImg = (Canvas)target;
			cnv_SourceImg.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 318:
			cnv_AIImg = (Canvas)target;
			cnv_AIImg.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 319:
			btn_TrainImageNum = (Button)target;
			break;
		case 320:
			btn_NextDBImage = (Button)target;
			btn_NextDBImage.Click += btn_NextDBImage_Click;
			break;
		case 321:
			btn_PrevDBImage = (Button)target;
			btn_PrevDBImage.Click += btn_PrevDBImage_Click;
			break;
		case 322:
			btn_DeleteDBImage = (Button)target;
			btn_DeleteDBImage.Click += btn_DeleteDBImage_Click;
			break;
		case 323:
			txt_ModeName_Copy = (TextBox)target;
			break;
		case 324:
			cnv_AIMarkImg = (Canvas)target;
			cnv_AIMarkImg.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 325:
			btn_ChartY = (Button)target;
			btn_ChartY.Click += Btn_Chart_Click;
			break;
		case 326:
			btn_ChartExport = (Button)target;
			btn_ChartExport.Click += Btn_ChartExport_Click;
			break;
		case 327:
			btn_ChartX = (Button)target;
			btn_ChartX.Click += Btn_Chart_Click;
			break;
		case 328:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 329:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 330:
			grd_Loading = (Grid)target;
			break;
		case 331:
			lab_Loading = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
