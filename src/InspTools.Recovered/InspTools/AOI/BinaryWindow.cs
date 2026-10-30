using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
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
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.AOI;

public class BinaryWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 35;

	private const int MaxThdTool = 4;

	private int LanguageType = 1;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private byte[] CurrentColor = new byte[16];

	private bool[] SearchRectResult = new bool[2];

	private BlobStruct[] Blobs = new BlobStruct[2];

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private OpenCvSharp.Rect LastSearchRect2;

	private OpenCvSharp.Rect DefaultSearchRect1 = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect DefaultSearchRect2 = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect[] MaskRect = new OpenCvSharp.Rect[2];

	private INSP_BINARYTOOL_PARAM CopyParam;

	private INSP_BINARYTOOL_PARAM BackupParam;

	private INSP_COLORREFERENCE_PARAM CopyColorParam;

	private bool CopyParamSeted;

	private bool DontSave;

	private bool DontSave1;

	private bool DontSave2;

	private bool ReferenceLock = true;

	private int SelectFace;

	private int FaceIndex;

	private bool TrackerAdjShow;

	private bool TrackerFillletShow;

	private int CurrentTracker;

	private string Tracker1Str;

	private string Tracker2Str;

	private bool SwitchToTempImage;

	private bool ShowBinaryCompare;

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat[] BinaryBlob = new Mat[2];

	private Mat BinaryT2 = new Mat();

	private Mat ImageSrc = new Mat();

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private double CanvasWidth;

	private double CanvasHeight;

	private int[] channelShiftMin = new int[4];

	private int[] channelShiftMax = new int[4];

	private int ImageListSource;

	private string[] filelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int TrackIndex;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private bool TrackerShow = true;

	private bool ShowMessage = true;

	private bool ReferenceShow = true;

	private RangeSlider[] THD_Sliders;

	private NumericUpDown[] THD_TextLow;

	private NumericUpDown[] THD_TextUp;

	private ComboBox[] THD_Channel;

	private ComboBox[] THD_DEStart;

	private ComboBox[] THD_DEEnd;

	private ComboBox[] THD_DEStartCount;

	private ComboBox[] THD_DEEndCount;

	private TextBox[] Result1Txt;

	private TextBox[] Result2Txt;

	private TextBox[] ValueMinTxt;

	private TextBox[] ValueMaxTxt;

	private Expander[] THD_Expender;

	private Button[] ToolsButton;

	private ComboBox[] InspRule;

	private ComboBox[] MB_InspRule;

	private bool EnableModifyLog;

	private string ModifyLog = "";

	private string RecipeName = "";

	private string UserID = "123456";

	private bool PreviewMode;

	private User_Permissions User = User_Permissions.USER_Engineer;

	private bool OTSU_White = true;

	private DispatcherTimer UpdateUITImer = new DispatcherTimer();

	private int updateCount;

	private Stopwatch sw = new Stopwatch();

	internal BinaryWindow MainW;

	internal Grid grd_SIMP_Setup;

	internal ComboBox cb_BinaryErrorBin;

	internal Button btn_Test;

	internal Rectangle rt_Result;

	internal TabControl tab_BlobAnalysis;

	internal TabItem tb_MaxBlob;

	internal TextBox txt_InspRule3Min;

	internal TextBox txt_InspRule3Max;

	internal TextBox txt_InspRule4Min;

	internal TextBox txt_InspRule4Max;

	internal TextBox txt_InspRule13Result;

	internal TextBox txt_InspRule14Result;

	internal TextBox txt_InspRule1Min;

	internal TextBox txt_InspRule1Max;

	internal TextBox txt_InspRule2Min;

	internal TextBox txt_InspRule2Max;

	internal TextBox txt_InspRule11Result;

	internal TextBox txt_InspRule12Result;

	internal ComboBox cb_InspRule1;

	internal ComboBox cb_InspRule2;

	internal ComboBox cb_InspRule3;

	internal ComboBox cb_InspRule4;

	internal TextBox txt_InspRule23Result;

	internal TextBox txt_InspRule24Result;

	internal TextBox txt_InspRule21Result;

	internal TextBox txt_InspRule22Result;

	internal Label lab_Tracker1Result;

	internal Label lab_Tracker2Result;

	internal RadioButton rad_RuleAND;

	internal RadioButton rad_RuleOR;

	internal Button btn_Chart;

	internal Button btn_Chart_Copy;

	internal Button btn_Chart_Copy1;

	internal Button btn_Chart_Copy2;

	internal TabItem tb_BlobLine;

	internal Grid ___沒有名稱_;

	internal TextBox txt_BlobLineAreaMin;

	internal TextBox txt_BlobLineAreaMax;

	internal TextBox txt_BlobLineMin;

	internal TextBox txt_OKLineResultA;

	internal TextBox txt_BlobLineHeightMin;

	internal TextBox txt_BlobLineHeightMax;

	internal ComboBox cb_BlobLineSort;

	internal TextBox txt_OKLineResultB;

	internal TextBox txt_BlobLineWidthMin;

	internal TextBox txt_BlobLineWidthMax;

	internal TextBox txt_BlobLineMax;

	internal TextBox txt_BlobLineNum;

	internal Label lab_Tracker1Result2;

	internal Label lab_Tracker2Result2;

	internal CheckBox cb_BlobValueUsePixelValue;

	internal TabItem tb_MultiBlob;

	internal TextBox txt_MB_InspRule3Min;

	internal TextBox txt_MB_InspRule3Max;

	internal TextBox txt_MB_InspRule4Min;

	internal TextBox txt_MB_InspRule4Max;

	internal TextBox txt_MB_InspRule1Min;

	internal TextBox txt_MB_InspRule1Max;

	internal TextBox txt_MB_InspRule2Min;

	internal TextBox txt_MB_InspRule2Max;

	internal ComboBox cb_MB_InspRule1;

	internal ComboBox cb_MB_InspRule2;

	internal ComboBox cb_MB_InspRule3;

	internal ComboBox cb_MB_InspRule4;

	internal RadioButton rad_MB_RuleAND;

	internal RadioButton rad_MB_RuleOR;

	internal ComboBox cb_MB_BlobSort;

	internal TextBox txt_MB_BlobNum;

	internal TextBox txt_MB_NGCount;

	internal TabItem tb_LineWidth;

	internal TextBox txt_LineWidthMinArea;

	internal TextBox txt_LineWidthMinW;

	internal TextBox txt_LineWidthMinH;

	internal CheckBox cb_LineWidthA;

	internal Rectangle rt_LineWidthAA;

	internal TextBox txt_AllowLineWidth;

	internal TextBox txt_AllowErrorW;

	internal CheckBox cb_LineWidthW;

	internal Rectangle rt_LineWidthWA;

	internal TextBox txt_AllowLineHeight;

	internal TextBox txt_AllowErrorH;

	internal CheckBox cb_LineWidthH;

	internal Rectangle rt_LineWidthHA;

	internal TextBox txt_AllowErrorWResultA;

	internal TextBox txt_AllowErrorHResultA;

	internal TextBox txt_AllowErrorWResultB;

	internal TextBox txt_AllowErrorHResultB;

	internal Rectangle rt_LineWidthWB;

	internal Rectangle rt_LineWidthHB;

	internal Rectangle rt_LineWidthAB;

	internal TextBox txt_AllowLineWidthError;

	internal TextBox txt_AllowLineErrorResultA;

	internal TextBox txt_AllowLineErrorResultB;

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal Border bor_Image;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Line line_X1;

	internal Line line_Y1;

	internal Line line_X2;

	internal Line line_Y2;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect2;

	internal TextBlock txt_Search2Label;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Rectangle rt_Mask1;

	internal Rectangle rt_Mask2;

	internal Rectangle rt_ReferenceRect1;

	internal Rectangle rt_ReferenceRect2;

	internal Label lab_FaceIndex;

	internal TextBox Label_ResultError;

	internal Label Label_Result;

	internal TextBox txt_PreviewMode;

	internal TextBox txt_BinaryCompareMode;

	internal TabControl tab_Function;

	internal ComboBox cb_TrackerSelect;

	internal ScrollBar scr_TrackerSwich;

	internal ToggleSwitch ts_TrackerON;

	internal Button btn_Reverse;

	internal Button btn_RecipeCopy;

	internal Button btn_RecipePaste;

	internal Label lab_TrackIndex;

	internal TextBox txt_TrackerName;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal Label lab_Time;

	internal Expander exp_THD1;

	internal Expander exp_THD2;

	internal Grid grd_SIMP_THD_2;

	internal RangeSlider rs_THD2;

	internal Grid grd_SIMP_DilateErode2;

	internal ComboBox cb_THD2_DEStart;

	internal ComboBox cb_THD2_DEEnd;

	internal ComboBox cb_THD2_DEEndCount;

	internal ComboBox cb_THD2_DEStartCount;

	internal ComboBox cb_Channel_THD2;

	internal NumericUpDown nu_Thd2LimitMin;

	internal NumericUpDown nu_Thd2LimitMax;

	internal Button btn_ChannelBinaryPreview2;

	internal Button btn_ChannelPreview2;

	internal Button btn_HistogramPreview2;

	internal Button btn_THD2Undo;

	internal Button btn_SoftCorrectionTHD2;

	internal Label lab_SoftCorrectionShiftTHD2;

	internal Expander exp_THD3;

	internal Grid grd_SIMP_THD_3;

	internal RangeSlider rs_THD3;

	internal Grid grd_SIMP_DilateErode3;

	internal ComboBox cb_THD3_DEStart;

	internal ComboBox cb_THD3_DEEnd;

	internal ComboBox cb_THD3_DEEndCount;

	internal ComboBox cb_THD3_DEStartCount;

	internal ComboBox cb_Channel_THD3;

	internal NumericUpDown nu_Thd3LimitMin;

	internal NumericUpDown nu_Thd3LimitMax;

	internal Button btn_ChannelBinaryPreview3;

	internal Button btn_ChannelPreview3;

	internal Button btn_HistogramPreview3;

	internal Button btn_THD3Undo;

	internal Button btn_SoftCorrectionTHD3;

	internal Label lab_SoftCorrectionShiftTHD3;

	internal Expander exp_THD4;

	internal Grid grd_SIMP_THD_4;

	internal RangeSlider rs_THD4;

	internal ComboBox cb_Channel_THD4;

	internal Grid grd_SIMP_DilateErode4;

	internal ComboBox cb_THD4_DEStart;

	internal ComboBox cb_THD4_DEEnd;

	internal ComboBox cb_THD4_DEEndCount;

	internal ComboBox cb_THD4_DEStartCount;

	internal NumericUpDown nu_Thd4LimitMin;

	internal NumericUpDown nu_Thd4LimitMax;

	internal Button btn_ChannelBinaryPreview4;

	internal Button btn_ChannelPreview4;

	internal Button btn_HistogramPreview1_Copy;

	internal Button btn_THD4Undo;

	internal Button btn_SoftCorrectionTHD4;

	internal Label lab_SoftCorrectionShiftTHD4;

	internal CheckBox chk_Thd_ORMode;

	internal Grid grd_SIMP_THD_1;

	internal RangeSlider rs_THD1;

	internal Grid grd_SIMP_DilateErode1;

	internal ComboBox cb_THD1_DEStart;

	internal ComboBox cb_THD1_DEEnd;

	internal ComboBox cb_THD1_DEEndCount;

	internal ComboBox cb_THD1_DEStartCount;

	internal CheckBox chk_EnableSoftCorrection;

	internal ComboBox cb_Channel_THD1;

	internal Button btn_ColorCopy;

	internal Button btn_ColorPaste;

	internal NumericUpDown nu_Thd1LimitMin;

	internal NumericUpDown nu_Thd1LimitMax;

	internal Button btn_ChannelBinaryPreview1;

	internal Button btn_ChannelPreview1;

	internal Button btn_HistogramPreview1;

	internal Button btn_THD1Undo;

	internal Button btn_OTSU;

	internal Button btn_SoftCorrectionTHD1;

	internal Label lab_SoftCorrectionShiftTHD1;

	internal Grid grd_QuickAIColor;

	internal Button btn_QuickAIColor_R;

	internal Button btn_QuickAIColor_G;

	internal Button btn_QuickAIColor_B;

	internal Button btn_QuickAIColor;

	internal Button btn_QuickAIColor_Y;

	internal Button btn_QuickAIColor_P;

	internal Button btn_QuickAIColor_LB;

	internal Grid grd_SIMP_RefSetup;

	internal ComboBox cb_ColorReference;

	internal Button btn_ReferenceSave;

	internal TextBox txt_ReferenceName;

	internal Button btn_ReferenceLock;

	internal ComboBox cb_Track1AlignWayX;

	internal RadioButton rad_Tracker1AutoSize;

	internal ComboBox cb_Track1AutoSize;

	internal RadioButton rad_Tracker1AlignSelect;

	internal Button btn_TrackerReset;

	internal ComboBox cb_Track1AlignWayY;

	internal Button btn_Tracker1SizeChange;

	internal Label txt_Tracker1Pos;

	internal CheckBox chk_TrackErrorSetNG;

	internal Grid grd_Track2Setup;

	internal ComboBox cb_Track2AlignWayX;

	internal RadioButton rad_Tracker2AutoSize;

	internal ComboBox cb_Track2AutoSize;

	internal RadioButton rad_Tracker2AlignSelect;

	internal ComboBox cb_Track2AlignWayY;

	internal Button btn_TrackerReset_Copy;

	internal Button btn_Tracker2SizeChange;

	internal Label txt_Tracker2Pos;

	internal CheckBox ts_Tracker2ON;

	internal Grid grd_Mask;

	internal ComboBox cb_MaskSelect;

	internal Button btn_MaskEdit;

	internal CheckBox chk_UseMaskAsInspArea;

	internal ComboBox cb_MatchMaskSelect1;

	internal ComboBox cb_MatchMaskSelect2;

	internal ComboBox cb_MatchMaskSelect3;

	internal ComboBox cb_MatchText_DEStart1;

	internal ComboBox cb_MatchText_DEStartCount1;

	internal ComboBox cb_MatchText_DEStart2;

	internal ComboBox cb_MatchText_DEStartCount2;

	internal ComboBox cb_MatchText_DEStart3;

	internal ComboBox cb_MatchText_DEStartCount3;

	internal ComboBox cb_Mask_DEStart;

	internal ComboBox cb_Mask_DEStartCount;

	internal Button btn_MaskPreView;

	internal CheckBox chk_UseFontMask;

	internal Grid grd_AdvFunction;

	internal ComboBox cb_JudgeWithAdvMeasureA;

	internal CheckBox chk_EnableJudgeWithAdvMeasure;

	internal ComboBox cb_JudgeWithAdvMeasureB;

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

	internal Button btn_ImageNum;

	internal Button btn_ImgC1;

	internal Button btn_ImgC2;

	internal Button btn_ImgC3;

	internal Button btn_ImgAI;

	internal Button btn_ImgAIR;

	internal Button btn_ImgAIG;

	internal Button btn_ImgAIB;

	internal Button btn_ImgC4;

	internal Button btn_ImgC5;

	internal Button btn_ImgC6;

	internal Label lab_RValue;

	internal Label lab_GValue;

	internal Label lab_BValue;

	internal Label lab_HValue;

	internal Label lab_SValue;

	internal Label lab_VValue;

	internal Label lab_GrayValue;

	internal Label lab_XValue;

	internal Label lab_YValue;

	internal Label lab_C1Value;

	internal Label lab_C2Value;

	internal Label lab_C3Value;

	internal Button btn_ImgT1;

	internal Button btn_ImgT2;

	internal Button btn_ImgT3;

	internal Label lab_AIRValue;

	internal Label lab_AIGValue;

	internal Label lab_AIBValue;

	internal Button btn_ColorDropper;

	internal TextBox txt_AllResult;

	internal Button btn_ReDraw;

	internal Button btn_TestNextImg;

	internal Button btn_TestPrevImg;

	internal Button btn_TrackerShow;

	internal Button btn_ReferenceShow;

	internal Button btn_PrevTracker;

	internal Button btn_NextTracker;

	internal Button btn_TrackerAdjShow;

	internal Button btn_PreviewSwitch;

	internal Button btn_Tools1;

	internal Button btn_Tools2;

	internal Button btn_Tools3;

	internal Button btn_Tools4;

	internal Button btn_Tools5;

	internal Button btn_Tools6;

	internal Button btn_Tools7;

	internal Button btn_Tools8;

	internal Button btn_Tools9;

	internal Button btn_Tools10;

	internal Button btn_Tools11;

	internal Button btn_Tools12;

	internal Button btn_Tools13;

	internal Button btn_Tools14;

	internal Button btn_Tools15;

	internal Button btn_Tools16;

	internal Button btn_Tools17;

	internal Button btn_Tools18;

	internal Button btn_Tools19;

	internal Button btn_Tools20;

	internal Button btn_Tools21;

	internal Button btn_Tools22;

	internal Button btn_Tools23;

	internal Button btn_Tools24;

	internal Button btn_Tools25;

	internal Button btn_Tools26;

	internal Button btn_Tools27;

	internal Button btn_Tools28;

	internal Button btn_Tools29;

	internal Button btn_Tools30;

	internal Expander exp_TrackerAdj;

	internal Button btn_TrackerZoomInUp;

	internal Button btn_TrackerZoomOutUp;

	internal Button btn_TrackerZoomOutDown;

	internal Button btn_TrackerZoomDown;

	internal Button btn_TrackerZoomLeft;

	internal Button btn_TrackerZoomLeft1;

	internal Button btn_TrackerZoomRight;

	internal Button btn_TrackerZoomInRight;

	internal Button btn_TrackerMoveUp;

	internal Button btn_TrackerMoveDown;

	internal Button btn_TestNextImg_Copy4;

	internal Button btn_TrackerMoveRight;

	internal Button btn_TestNextImg2_Copy;

	internal Grid grd_TrackerAdj;

	internal Button btn_TrackerSetSameSize;

	internal Button btn_TrackerAlignTop;

	internal Button TrackerAlignBottom;

	internal Button TrackerAlignLeft;

	internal Button TrackerAlignRight;

	internal Label lab_Tracker1;

	internal Label lab_Tracker2;

	internal Button btn_ResetA;

	internal Button btn_ResetB;

	internal Grid grd_Histogram;

	internal Canvas cnv_Histogram;

	internal Button btn_Fillet;

	internal Expander exp_TrackerFilllet;

	internal ComboBox cb_T1Filllet0;

	internal ComboBox cb_T1Filllet1;

	internal ComboBox cb_T1Filllet2;

	internal ComboBox cb_T1Filllet3;

	internal Grid grd_Tracker2Filllet;

	internal ComboBox cb_T2Filllet0;

	internal ComboBox cb_T2Filllet1;

	internal ComboBox cb_T2Filllet2;

	internal ComboBox cb_T2Filllet3;

	internal Label Label_Index;

	internal TextBox lab_LockMode;

	internal Button btn_BinaryCompare;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public BinaryWindow(User_Permissions permissions)
	{
		inited = false;
		User = permissions;
		InitializeComponent();
		exp_THD4.Visibility = Visibility.Visible;
		inited = true;
		THD_Preview = false;
		Label_ResultError.Visibility = Visibility.Hidden;
		rt_Mask1.Visibility = Visibility.Hidden;
		rt_Mask2.Visibility = Visibility.Hidden;
		CanvasWidth = cnv_Image.Width;
		CanvasHeight = cnv_Image.Height;
		THD_Sliders = new RangeSlider[4] { rs_THD1, rs_THD2, rs_THD3, rs_THD4 };
		THD_TextLow = new NumericUpDown[4] { nu_Thd1LimitMin, nu_Thd2LimitMin, nu_Thd3LimitMin, nu_Thd4LimitMin };
		THD_TextUp = new NumericUpDown[4] { nu_Thd1LimitMax, nu_Thd2LimitMax, nu_Thd3LimitMax, nu_Thd4LimitMax };
		THD_Channel = new ComboBox[4] { cb_Channel_THD1, cb_Channel_THD2, cb_Channel_THD3, cb_Channel_THD4 };
		THD_DEStart = new ComboBox[4] { cb_THD1_DEStart, cb_THD2_DEStart, cb_THD3_DEStart, cb_THD4_DEStart };
		THD_DEEnd = new ComboBox[4] { cb_THD1_DEEnd, cb_THD2_DEEnd, cb_THD3_DEEnd, cb_THD4_DEEnd };
		THD_DEStartCount = new ComboBox[4] { cb_THD1_DEStartCount, cb_THD2_DEStartCount, cb_THD3_DEStartCount, cb_THD4_DEStartCount };
		THD_DEEndCount = new ComboBox[4] { cb_THD1_DEEndCount, cb_THD2_DEEndCount, cb_THD3_DEEndCount, cb_THD4_DEEndCount };
		Result1Txt = new TextBox[4] { txt_InspRule11Result, txt_InspRule12Result, txt_InspRule13Result, txt_InspRule14Result };
		Result2Txt = new TextBox[4] { txt_InspRule21Result, txt_InspRule22Result, txt_InspRule23Result, txt_InspRule24Result };
		ValueMinTxt = new TextBox[4] { txt_InspRule1Min, txt_InspRule2Min, txt_InspRule3Min, txt_InspRule4Min };
		ValueMaxTxt = new TextBox[4] { txt_InspRule1Max, txt_InspRule2Max, txt_InspRule3Max, txt_InspRule4Max };
		THD_Expender = new Expander[4] { exp_THD1, exp_THD2, exp_THD3, exp_THD4 };
		ToolsButton = new Button[30]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20,
			btn_Tools21, btn_Tools22, btn_Tools23, btn_Tools24, btn_Tools25, btn_Tools26, btn_Tools27, btn_Tools28, btn_Tools29, btn_Tools30
		};
		InspRule = new ComboBox[4] { cb_InspRule1, cb_InspRule2, cb_InspRule3, cb_InspRule4 };
		MB_InspRule = new ComboBox[4] { cb_MB_InspRule1, cb_MB_InspRule2, cb_MB_InspRule3, cb_MB_InspRule4 };
		Label_Result.Visibility = Visibility.Hidden;
		BinaryBlob[0] = new Mat();
		BinaryBlob[1] = new Mat();
		rt_ReferenceRect1.Visibility = Visibility.Hidden;
		rt_ReferenceRect2.Visibility = Visibility.Hidden;
		line_X1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		line_Y1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		btn_ResetA.Visibility = Visibility.Hidden;
		btn_ResetB.Visibility = Visibility.Hidden;
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		exp_TrackerFilllet.Visibility = Visibility.Hidden;
		grd_Histogram.Visibility = Visibility.Hidden;
		txt_BinaryCompareMode.Visibility = Visibility.Hidden;
		txt_PreviewMode.Visibility = Visibility.Hidden;
		grd_QuickAIColor.Visibility = Visibility.Hidden;
		UpdateUITImer.Interval = TimeSpan.FromMilliseconds(500.0);
		exp_THD4.Visibility = Visibility.Hidden;
		UpdateUITImer.Tick += UpdateUITImer_Tick;
		UpdateUITImer.Start();
	}

	private void UpdateUITImer_Tick(object sender, EventArgs e)
	{
		if (updateCount > 2)
		{
			if (cb_ColorReference.SelectedIndex <= 0)
			{
				exp_THD4.IsEnabled = true;
			}
			UpdateUITImer.Stop();
		}
		updateCount++;
	}

	private void TimeStart()
	{
		sw.Start();
	}

	private double TimeStop()
	{
		sw.Stop();
		double totalMilliseconds = sw.Elapsed.TotalMilliseconds;
		sw.Reset();
		return totalMilliseconds;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(int index, CVInspection station, int faceIndex = 0)
	{
		try
		{
			TrackIndex = index;
			inspection = station;
			FaceIndex = faceIndex;
			SetFaceIndex(faceIndex);
			lab_TrackIndex.Content = (TrackIndex + 1).ToString();
			UpdateUI = false;
			cb_TrackerSelect.Items.Clear();
			cb_Track1AlignWayX.Items.Clear();
			cb_Track1AlignWayY.Items.Clear();
			cb_Track2AlignWayX.Items.Clear();
			cb_Track2AlignWayY.Items.Clear();
			cb_Track1AutoSize.Items.Clear();
			cb_Track2AutoSize.Items.Clear();
			cb_MaskSelect.Items.Clear();
			cb_MaskSelect.Items.Add("-----");
			cb_MatchMaskSelect1.Items.Clear();
			cb_MatchMaskSelect1.Items.Add("-----");
			cb_MatchMaskSelect2.Items.Clear();
			cb_MatchMaskSelect2.Items.Add("-----");
			cb_MatchMaskSelect3.Items.Clear();
			cb_MatchMaskSelect3.Items.Add("-----");
			cb_JudgeWithAdvMeasureA.Items.Clear();
			cb_JudgeWithAdvMeasureA.Items.Add("-----");
			cb_JudgeWithAdvMeasureB.Items.Clear();
			cb_JudgeWithAdvMeasureB.Items.Add("-----");
			for (int i = 0; i < 30; i++)
			{
				if (inspection.paramFile.MaskParam[i].TrackerON)
				{
					cb_MaskSelect.Items.Add(i + 1 + " : " + inspection.paramFile.MaskParam[i].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.MaskParam[i].TrackerName;
				comboBoxItem.IsEnabled = false;
				cb_MaskSelect.Items.Add(comboBoxItem);
			}
			if (inspection.paramFile.FontToolON)
			{
				chk_UseFontMask.Visibility = Visibility.Visible;
			}
			else
			{
				chk_UseFontMask.Visibility = Visibility.Hidden;
			}
			for (int j = 0; j < 40; j++)
			{
				if (inspection.paramFile.AdvLocateMParam[j].TrackerON)
				{
					cb_JudgeWithAdvMeasureA.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateMParam[j].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem2 = new ComboBoxItem();
				comboBoxItem2.Content = j + 1 + " : " + inspection.paramFile.AdvLocateMParam[j].TrackerName;
				comboBoxItem2.IsEnabled = false;
				cb_JudgeWithAdvMeasureA.Items.Add(comboBoxItem2);
			}
			for (int k = 0; k < 40; k++)
			{
				if (inspection.paramFile.AdvLocateMParam[k].TrackerON)
				{
					cb_JudgeWithAdvMeasureB.Items.Add(k + 1 + " : " + inspection.paramFile.AdvLocateMParam[k].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem3 = new ComboBoxItem();
				comboBoxItem3.Content = k + 1 + " : " + inspection.paramFile.AdvLocateMParam[k].TrackerName;
				comboBoxItem3.IsEnabled = false;
				cb_JudgeWithAdvMeasureB.Items.Add(comboBoxItem3);
			}
			for (int l = 0; l < 20; l++)
			{
				if (inspection.paramFile.MatchParam[l].TrackerON)
				{
					cb_MatchMaskSelect1.Items.Add(l + 1 + " : " + inspection.paramFile.MatchParam[l].TrackerName);
					cb_MatchMaskSelect2.Items.Add(l + 1 + " : " + inspection.paramFile.MatchParam[l].TrackerName);
					cb_MatchMaskSelect3.Items.Add(l + 1 + " : " + inspection.paramFile.MatchParam[l].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem4 = new ComboBoxItem();
					comboBoxItem4.Content = l + 1 + " : " + inspection.paramFile.MatchParam[l].TrackerName;
					comboBoxItem4.IsEnabled = false;
					cb_MatchMaskSelect1.Items.Add(comboBoxItem4);
					ComboBoxItem comboBoxItem5 = new ComboBoxItem();
					comboBoxItem5.Content = l + 1 + " : " + inspection.paramFile.MatchParam[l].TrackerName;
					comboBoxItem5.IsEnabled = false;
					cb_MatchMaskSelect2.Items.Add(comboBoxItem5);
					ComboBoxItem comboBoxItem6 = new ComboBoxItem();
					comboBoxItem6.Content = l + 1 + " : " + inspection.paramFile.MatchParam[l].TrackerName;
					comboBoxItem6.IsEnabled = false;
					cb_MatchMaskSelect3.Items.Add(comboBoxItem6);
				}
				if (!inspection.paramFile.AdvLocateToolON)
				{
					if (l < 2)
					{
						cb_Track1AlignWayX.Items.Add(l + 1 + " : " + inspection.paramFile.VSLocateParam[l].TrackerName);
						cb_Track2AlignWayX.Items.Add(l + 1 + " : " + inspection.paramFile.VSLocateParam[l].TrackerName);
						cb_Track1AlignWayY.Items.Add(l + 1 + " : " + inspection.paramFile.HSLocateParam[l].TrackerName);
						cb_Track2AlignWayY.Items.Add(l + 1 + " : " + inspection.paramFile.HSLocateParam[l].TrackerName);
						if (l < 1)
						{
							cb_Track1AutoSize.Items.Add(l + 1 + " : " + inspection.paramFile.AdvLocateParam[l].TrackerName);
							cb_Track2AutoSize.Items.Add(l + 1 + " : " + inspection.paramFile.AdvLocateParam[l].TrackerName);
						}
					}
					continue;
				}
				if (inspection.paramFile.VSLocateParam[l].TrackerON)
				{
					cb_Track1AlignWayX.Items.Add(l + 1 + " : " + inspection.paramFile.VSLocateParam[l].TrackerName);
					cb_Track2AlignWayX.Items.Add(l + 1 + " : " + inspection.paramFile.VSLocateParam[l].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem7 = new ComboBoxItem();
					comboBoxItem7.Content = l + 1 + " : " + inspection.paramFile.VSLocateParam[l].TrackerName;
					comboBoxItem7.IsEnabled = false;
					cb_Track1AlignWayX.Items.Add(comboBoxItem7);
					ComboBoxItem comboBoxItem8 = new ComboBoxItem();
					comboBoxItem8.Content = l + 1 + " : " + inspection.paramFile.VSLocateParam[l].TrackerName;
					comboBoxItem8.IsEnabled = false;
					cb_Track2AlignWayX.Items.Add(comboBoxItem8);
				}
				if (inspection.paramFile.HSLocateParam[l].TrackerON)
				{
					cb_Track1AlignWayY.Items.Add(l + 1 + " : " + inspection.paramFile.HSLocateParam[l].TrackerName);
					cb_Track2AlignWayY.Items.Add(l + 1 + " : " + inspection.paramFile.HSLocateParam[l].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem9 = new ComboBoxItem();
					comboBoxItem9.Content = l + 1 + " : " + inspection.paramFile.HSLocateParam[l].TrackerName;
					comboBoxItem9.IsEnabled = false;
					cb_Track1AlignWayY.Items.Add(comboBoxItem9);
					ComboBoxItem comboBoxItem10 = new ComboBoxItem();
					comboBoxItem10.Content = l + 1 + " : " + inspection.paramFile.HSLocateParam[l].TrackerName;
					comboBoxItem10.IsEnabled = false;
					cb_Track2AlignWayY.Items.Add(comboBoxItem10);
				}
				if (inspection.paramFile.AdvLocateParam[l].TrackerON)
				{
					cb_Track1AutoSize.Items.Add(l + 1 + " : " + inspection.paramFile.AdvLocateParam[l].TrackerName);
					cb_Track2AutoSize.Items.Add(l + 1 + " : " + inspection.paramFile.AdvLocateParam[l].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem11 = new ComboBoxItem();
				comboBoxItem11.Content = l + 1 + " : " + inspection.paramFile.AdvLocateParam[l].TrackerName;
				comboBoxItem11.IsEnabled = false;
				cb_Track1AutoSize.Items.Add(comboBoxItem11);
				ComboBoxItem comboBoxItem12 = new ComboBoxItem();
				comboBoxItem12.Content = l + 1 + " : " + inspection.paramFile.AdvLocateParam[l].TrackerName;
				comboBoxItem12.IsEnabled = false;
				cb_Track2AutoSize.Items.Add(comboBoxItem12);
			}
			for (int m = 0; m < 30; m++)
			{
				cb_TrackerSelect.Items.Add(m + 1 + " : " + inspection.paramFile.BinaryParam[m].TrackerName);
			}
			if (!station.paramFile.AIEnhanceToolON && !station.paramFile.AIPredictToolON)
			{
				btn_ImgAI.Visibility = Visibility.Hidden;
				btn_ImgAIR.Visibility = Visibility.Hidden;
				btn_ImgAIG.Visibility = Visibility.Hidden;
				btn_ImgAIB.Visibility = Visibility.Hidden;
				lab_AIRValue.Visibility = Visibility.Hidden;
				lab_AIGValue.Visibility = Visibility.Hidden;
				lab_AIBValue.Visibility = Visibility.Hidden;
			}
			UpdateToolBox();
			cb_TrackerSelect.SelectedIndex = index;
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "BW05-BinaryWindow-SetInspection()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void UpdateToolBox()
	{
		for (int i = 0; i < 30; i++)
		{
			if (inspection.paramFile.BinaryParam[i].TrackerON)
			{
				if (inspection.iResult.BinaryResult[i] == TOOL_RESULT.EM_OK)
				{
					if (inspection.paramFile.BinaryParam[i].EnableSelectFace)
					{
						if (inspection.paramFile.BinaryParam[i].FaceIndex == FaceIndex)
						{
							ToolsButton[i].Background = Brushes.Lime;
						}
						else
						{
							ToolsButton[i].Background = Brushes.DarkGray;
						}
					}
					else
					{
						ToolsButton[i].Background = Brushes.Lime;
					}
				}
				else if (inspection.iResult.BinaryResult[i] == TOOL_RESULT.EM_NG)
				{
					ToolsButton[i].Background = Brushes.Red;
				}
				else
				{
					ToolsButton[i].Background = Brushes.Cyan;
				}
			}
			else
			{
				ToolsButton[i].Background = Brushes.White;
			}
			ToolsButton[i].ToolTip = inspection.paramFile.BinaryParam[i].TrackerName;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetFaceIndex(int index)
	{
		switch (index)
		{
		case 0:
			lab_FaceIndex.Visibility = Visibility.Hidden;
			break;
		case 1:
			lab_FaceIndex.Content = "A";
			break;
		case 2:
			lab_FaceIndex.Content = "B";
			break;
		case 3:
			lab_FaceIndex.Content = "C";
			break;
		case 4:
			lab_FaceIndex.Content = "D";
			break;
		default:
			lab_FaceIndex.Visibility = Visibility.Hidden;
			break;
		}
	}

	private void SetSelectFace(int index)
	{
		SelectFace = index;
		if (index > 0)
		{
			chk_SelectFace.IsChecked = true;
			grd_SelectFace.Visibility = Visibility.Visible;
		}
		btn_FaceA.Background = Brushes.White;
		btn_FaceB.Background = Brushes.White;
		btn_FaceC.Background = Brushes.White;
		btn_FaceD.Background = Brushes.White;
		switch (index)
		{
		case 0:
			btn_FaceA.Background = Brushes.White;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			break;
		case 1:
			btn_FaceA.Background = Brushes.Yellow;
			break;
		case 2:
			btn_FaceB.Background = Brushes.Yellow;
			break;
		case 3:
			btn_FaceC.Background = Brushes.Yellow;
			break;
		case 4:
			btn_FaceD.Background = Brushes.Yellow;
			break;
		default:
			btn_FaceA.Background = Brushes.White;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
	{
		try
		{
			ImageWidth = mat.Cols;
			ImageHeight = mat.Rows;
			ImageBand = mat.Channels();
			xRatio = CanvasWidth / (double)ImageWidth;
			yRatio = CanvasHeight / (double)ImageHeight;
			if (xRatio > yRatio)
			{
				xRatio = yRatio;
			}
			else
			{
				yRatio = xRatio;
			}
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)ImageWidth * xRatio);
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)ImageHeight * yRatio);
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc.Clone(), ShowBitmap);
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
			LocateRect = locateRect;
			bor_LocateRect.SetValue(Canvas.LeftProperty, (double)locateRect.X * xRatio);
			bor_LocateRect.SetValue(Canvas.TopProperty, (double)locateRect.Y * yRatio);
			bor_LocateRect.SetValue(FrameworkElement.WidthProperty, (double)locateRect.Width * xRatio);
			bor_LocateRect.SetValue(FrameworkElement.HeightProperty, (double)locateRect.Height * yRatio);
			BackupParam = inspection.paramFile.BinaryParam[TrackIndex];
			if (CopyValue)
			{
				CopyValueToUI();
			}
			if (!SwitchToTempImage)
			{
				ImageSrc = inspection.buffers.Src.Clone();
				if (!inspection.TempImage[0].Empty())
				{
					btn_ImgT1.Background = Brushes.LightSkyBlue;
				}
				if (!inspection.TempImage[1].Empty())
				{
					btn_ImgT2.Background = Brushes.LightSkyBlue;
				}
				if (!inspection.TempImage[2].Empty())
				{
					btn_ImgT3.Background = Brushes.LightSkyBlue;
				}
			}
		}
		catch
		{
			string text = "BW01-BinaryWindow-SetBuffer()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-BinaryWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI();
	}

	public void SetModifyLogEnable(bool IsEnable, string recipename, string userID)
	{
		EnableModifyLog = IsEnable;
		RecipeName = recipename;
		UserID = userID;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		if (!ReferenceLock)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("顏色參考數值已被更動，是否儲存?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("颜色参考数值已被更动，是否储存?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Color Reference Parameters has been modified， Recipe will be save!", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("色の参照値が変更されました。保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				Btn_ReferenceSave_Click(sender, null);
			}
			ReferenceLock = true;
		}
		WriteUIValueToParam();
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
		}
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

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImgSrc_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		Mat mat = new Mat();
		if (!SwitchToTempImage || button.Uid != "0")
		{
			mat = inspection.buffers.GetOfflineMat(button.Uid);
			THD_Preview = false;
			ImageSource imageSource = inspection.buffers.Mat2ImageSource(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
			return;
		}
		inspection.SetBuffers(ImageSrc);
		inspection.Inspect(ref mat);
		if (!inspection.LocaetResult || !inspection.AdvlocateResult)
		{
			cnv_Image.Background = Brushes.LightGray;
			THD_Preview = true;
			DontSave = true;
			btn_Test.IsEnabled = false;
			if (!inspection.LocaetResult)
			{
				SetResult(2, LanguageManager.Instance["LocateFail"]);
			}
			else if (!inspection.AdvlocateResult)
			{
				SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
			}
		}
		else
		{
			FaceIndex = inspection.SelectFaceResult;
			SetFaceIndex(FaceIndex);
			DontSave = false;
			btn_Test.IsEnabled = true;
			SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
			Btn_Test_Click(this, null);
		}
		SwitchToTempImage = false;
	}

	private void SetMaskTracker(OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2)
	{
		OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		if (rect1.Width > 0 || rect1.Height > 0)
		{
			rect1 &= rect3;
			rt_Mask1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			rt_Mask1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			rt_Mask1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			rt_Mask1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
		}
		if (rect2.Width > 0 || rect2.Height > 0)
		{
			rect2 &= rect3;
			rt_Mask2.SetValue(Canvas.LeftProperty, (double)rect2.X * xRatio);
			rt_Mask2.SetValue(Canvas.TopProperty, (double)rect2.Y * yRatio);
			rt_Mask2.SetValue(FrameworkElement.WidthProperty, (double)rect2.Width * xRatio);
			rt_Mask2.SetValue(FrameworkElement.HeightProperty, (double)rect2.Height * yRatio);
		}
	}

	private void SetReferenceRect(Rectangle rt, OpenCvSharp.Rect rect)
	{
		if (rect.Width > 0 || rect.Height > 0)
		{
			rt.Visibility = Visibility.Visible;
			rt.SetValue(Canvas.LeftProperty, (double)rect.X * xRatio);
			rt.SetValue(Canvas.TopProperty, (double)rect.Y * yRatio);
			rt.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
			rt.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * yRatio);
		}
	}

	private void SetReferenceLine(Line x, Line y, int xindex, int yindex)
	{
		x.Visibility = Visibility.Visible;
		double x2 = (x.X2 = (double)inspection.paramFile.VSLocateParam[xindex].Value * xRatio);
		x.X1 = x2;
		x.Y1 = 0.0;
		x.Y2 = (double)ImageHeight * yRatio;
		y.Visibility = Visibility.Visible;
		x2 = (y.Y2 = (double)inspection.paramFile.HSLocateParam[yindex].Value * yRatio);
		y.Y1 = x2;
		y.X1 = 0.0;
		y.X2 = (double)ImageWidth * xRatio;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool SetTracker(OpenCvSharp.Rect rect1, int TrackerID)
	{
		if (PreviewMode)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		rect1 &= rect2;
		bool result = true;
		if (TrackerID == 0)
		{
			if (rect1.Width <= 0 || rect1.Height <= 0)
			{
				if (LanguageType == 1)
				{
					Label_ResultError.Text = "檢測區域A寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域A!";
				}
				else if (LanguageType == 2)
				{
					Label_ResultError.Text = "检测区域A宽高小于0，请检查对齐、自动内缩设定或是重设检测区域A!";
				}
				else if (LanguageType == 3)
				{
					Label_ResultError.Text = "検査領域Aの幅と高さが0未満です。アライメント、自動縮小設定を確認するか、検査領域Aをリセットしてください。";
				}
				else
				{
					Label_ResultError.Text = "Tracker A value < 0，Please check Aligment and Autofit value, or reset Tracker A! ";
				}
				Label_ResultError.Visibility = Visibility.Visible;
				btn_ResetA.Visibility = Visibility.Visible;
				txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
				result = false;
				bor_SearchRect1.Uid = "1";
				DontSave1 = true;
				btn_Test.IsEnabled = false;
				bor_SearchRect1.Visibility = Visibility.Hidden;
			}
			else
			{
				bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
				bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
				bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
				bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
				bor_SearchRect1.Uid = "0";
				DontSave1 = false;
				txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
				if (TrackerShow)
				{
					bor_SearchRect1.Visibility = Visibility.Visible;
				}
			}
		}
		else if ((rect1.Width == 0 || rect1.Height == 0) && ts_Tracker2ON.IsChecked == true)
		{
			if (LanguageType == 1)
			{
				Label_ResultError.Text = "檢測區域B寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域B!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Text = "检测区域B宽高小于0，请检查对齐、自动内缩设定或是重设检测区域B!";
			}
			else if (LanguageType == 3)
			{
				Label_ResultError.Text = "檢測區域B寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域B!";
			}
			else
			{
				Label_ResultError.Text = "検査領域Bの幅と高さが0未満です。アライメント、自動縮小設定を確認するか、検査領域Bをリセットしてください。";
			}
			Label_ResultError.Visibility = Visibility.Visible;
			btn_ResetB.Visibility = Visibility.Visible;
			txt_Tracker2Pos.Content = string.Format("[B] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
			result = false;
			bor_SearchRect2.Uid = "1";
			DontSave2 = true;
			btn_Test.IsEnabled = false;
			bor_SearchRect2.Visibility = Visibility.Hidden;
		}
		else
		{
			bor_SearchRect2.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			bor_SearchRect2.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
			bor_SearchRect2.Uid = "0";
			DontSave2 = false;
			txt_Tracker2Pos.Content = string.Format("[B] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			if (TrackerShow)
			{
				bor_SearchRect2.Visibility = Visibility.Visible;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetTracker(ref OpenCvSharp.Rect rect1, int TrackerID)
	{
		if (PreviewMode)
		{
			return false;
		}
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		if (TrackerID == 0)
		{
			rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
			rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
			rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
			rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
			rect1 &= rect2;
			if (rect1.Width <= 0 || rect1.Height <= 0)
			{
				if (LanguageType == 1)
				{
					Label_ResultError.Text = "檢測區域A超出範圍，請檢查對齊、自動內縮設定或是重設檢測區域A!";
				}
				else if (LanguageType == 2)
				{
					Label_ResultError.Text = "检测区域A超出范围，请检查对齐、自动内缩设定或是重设检测区域A!";
				}
				else if (LanguageType == 3)
				{
					Label_ResultError.Text = "検査領域Aは範囲外です。アライメント、自動縮小設定を確認するか、検査領域Aをリセットしてください。";
				}
				else
				{
					Label_ResultError.Text = "Tracker A value over range，Please check Aligment and Autofit value, or reset Tracker A! ";
				}
				Label_ResultError.Visibility = Visibility.Visible;
				txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
				return false;
			}
			txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			if (rad_Tracker1AlignSelect.IsChecked == true)
			{
				if (LanguageType == 1)
				{
					Tracker1Str = string.Format("[A] X:{0}, Y:{1}, 寬:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
				}
				else if (LanguageType == 2)
				{
					Tracker1Str = string.Format("[A] X:{0}, Y:{1}, 宽:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
				}
				else if (LanguageType == 3)
				{
					Tracker1Str = string.Format("[A] X:{0}, Y:{1}, 幅:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
				}
				else
				{
					Tracker1Str = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
				}
			}
			else if (LanguageType == 1)
			{
				Tracker1Str = string.Format("[A] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 寬:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
			}
			else if (LanguageType == 2)
			{
				Tracker1Str = string.Format("[A] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 宽:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
			}
			else if (LanguageType == 3)
			{
				Tracker1Str = string.Format("[A] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 幅:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
			}
			else
			{
				Tracker1Str = string.Format("[A] L:{0}, T:{1}, R:{2}, B:{3}, W:{4}, H:{5}", inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
			}
			txt_Search1Label.Text = Tracker1Str;
			lab_Tracker1.Content = Tracker1Str;
		}
		else
		{
			rect1.X = (int)(Canvas.GetLeft(bor_SearchRect2) / xRatio);
			rect1.Y = (int)(Canvas.GetTop(bor_SearchRect2) / yRatio);
			rect1.Width = (int)(bor_SearchRect2.Width / xRatio);
			rect1.Height = (int)(bor_SearchRect2.Height / yRatio);
			rect1 &= rect2;
			if (ts_Tracker2ON.IsChecked == true)
			{
				if (rect1.Width == 0 || rect1.Height == 0)
				{
					if (LanguageType == 1)
					{
						Label_ResultError.Text = "檢測區域B超出範圍，請檢查對齊、自動內縮設定或是重設檢測區域B!";
					}
					else if (LanguageType == 2)
					{
						Label_ResultError.Text = "检测区域B超出范围，请检查对齐、自动内缩设定或是重设检测区域B!";
					}
					else if (LanguageType == 3)
					{
						Label_ResultError.Text = "検査領域Bは範囲外です。アライメント、自動縮小設定を確認するか、検査領域Bをリセットしてください。";
					}
					else
					{
						Label_ResultError.Text = "Tracker B value over range，Please check Aligment and Autofit value, or reset Tracker B! ";
					}
					Label_ResultError.Visibility = Visibility.Visible;
					txt_Tracker2Pos.Content = string.Format("[B] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
					return false;
				}
				txt_Tracker2Pos.Content = string.Format("[B] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
				if (rad_Tracker2AlignSelect.IsChecked == true)
				{
					if (LanguageType == 1)
					{
						Tracker2Str = string.Format("[B] X:{0}, Y:{1}, 寬:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
					}
					else if (LanguageType == 2)
					{
						Tracker2Str = string.Format("[B] X:{0}, Y:{1}, 宽:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
					}
					else if (LanguageType == 3)
					{
						Tracker2Str = string.Format("[B] X:{0}, Y:{1}, 幅:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
					}
					else
					{
						Tracker2Str = string.Format("[B] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
					}
				}
				else if (LanguageType == 1)
				{
					Tracker2Str = string.Format("[B] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 寬:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
				}
				else if (LanguageType == 2)
				{
					Tracker2Str = string.Format("[B] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 宽:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
				}
				else if (LanguageType == 3)
				{
					Tracker2Str = string.Format("[B] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 幅:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
				}
				else
				{
					Tracker2Str = string.Format("[B] L:{0}, T:{1}, R:{2}, B:{3}, W:{4}, H:{5}", inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
				}
				txt_Search2Label.Text = Tracker2Str;
				lab_Tracker2.Content = Tracker2Str;
			}
		}
		return true;
	}

	private void Rs_SIMP_THD1_ThumbDragCompleted(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			DoThdImage();
			Btn_Test_Click(this, null);
		}
	}

	private void Btn_PreviewSwitch_Click(object sender, RoutedEventArgs e)
	{
		SwitchView();
	}

	private void SwitchView()
	{
		if (THD_Preview)
		{
			Mat locatedSrc = inspection.buffers.LocatedSrc;
			ImageSource imageSource = inspection.buffers.Mat2ImageSource(locatedSrc);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			THD_Preview = false;
		}
		else
		{
			DoThdImage();
			THD_Preview = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetResult(bool result)
	{
		Label_Result.Visibility = Visibility.Visible;
		Label_ResultError.Visibility = Visibility.Hidden;
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = Brushes.Lime;
			ToolsButton[TrackIndex].Background = Brushes.Lime;
			rt_Result.Fill = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
			ToolsButton[TrackIndex].Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
		}
	}

	private void SetResult(int result, string msg)
	{
		if (result == 2)
		{
			Label_Result.Visibility = Visibility.Hidden;
			Label_ResultError.Visibility = Visibility.Visible;
			Label_ResultError.Text = msg;
			Label_ResultError.Background = Brushes.Red;
			ToolsButton[TrackIndex].Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
		}
	}

	private void SetErrorResult(string str)
	{
		Label_Result.Visibility = Visibility.Visible;
		Label_Result.Content = str;
		Label_Result.Background = Brushes.Red;
		ToolsButton[TrackIndex].Background = Brushes.Red;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckResult()
	{
		try
		{
			OpenCvSharp.Rect[] array = new OpenCvSharp.Rect[2]
			{
				inspection.binaryTool[TrackIndex].Blobs[0].NGRect,
				inspection.binaryTool[TrackIndex].Blobs[1].NGRect
			};
			double[] array2 = new double[2]
			{
				inspection.binaryTool[TrackIndex].Blobs[0].Area,
				inspection.binaryTool[TrackIndex].Blobs[1].Area
			};
			bool[] array3 = new bool[4];
			for (int i = 0; i < 2; i++)
			{
				TextBox[] array4 = ((i != 0) ? Result2Txt : Result1Txt);
				if (i == 1 && ts_Tracker2ON.IsChecked == false)
				{
					TextBox obj = array4[0];
					TextBox obj2 = array4[1];
					TextBox obj3 = array4[2];
					string text = (array4[3].Text = "");
					string text3 = (obj3.Text = text);
					string text5 = (obj2.Text = text3);
					obj.Text = text5;
					SearchRectResult[i] = true;
					break;
				}
				array3[0] = GetRule(cb_InspRule1.SelectedIndex, txt_InspRule1Min, txt_InspRule1Max, array4[0], array[i], array2[i], i);
				array3[1] = GetRule(cb_InspRule2.SelectedIndex, txt_InspRule2Min, txt_InspRule2Max, array4[1], array[i], array2[i], i);
				array3[2] = GetRule(cb_InspRule3.SelectedIndex, txt_InspRule3Min, txt_InspRule3Max, array4[2], array[i], array2[i], i);
				array3[3] = GetRule(cb_InspRule4.SelectedIndex, txt_InspRule4Min, txt_InspRule4Max, array4[3], array[i], array2[i], i);
				if (rad_RuleAND.IsChecked == true)
				{
					if (array3[0] && array3[1] && array3[2] && array3[3])
					{
						SearchRectResult[i] = true;
					}
					else
					{
						SearchRectResult[i] = false;
					}
					continue;
				}
				SearchRectResult[i] = false;
				for (int j = 0; j < 4; j++)
				{
					if (array3[j] && InspRule[j].SelectedIndex > 0)
					{
						SearchRectResult[i] = true;
						break;
					}
				}
			}
			if (SearchRectResult[0] && SearchRectResult[1])
			{
				SetResult(result: true);
				return true;
			}
			if (TrackIndex == 0 && inspection.paramFile.EnableJudgeWithAdvMeasure && (inspection.paramFile.JudgeWithAdvMeasureIndexA > 0 || inspection.paramFile.JudgeWithAdvMeasureIndexB > 0) && inspection.paramFile.JudgeWithAdvMeasureResult)
			{
				return true;
			}
			SetResult(result: false);
			return false;
		}
		catch (Exception ex)
		{
			string text7 = "BW04-BinaryWindow-CheckResult()-";
			text7 += ex.ToString();
			CVBuffer.SaveLog(text7, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text7, "錯誤");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, TextBox min, TextBox max, TextBox resultbox, OpenCvSharp.Rect rect, double area, int TrackerABIndex)
	{
		bool result = true;
		resultbox.Foreground = Brushes.Black;
		double num = ConvertComDigits(min, ForSave: false);
		double num2 = ConvertComDigits(max, ForSave: false);
		try
		{
			switch (rule)
			{
			case 0:
				resultbox.Text = "0";
				break;
			case 1:
			{
				double value9 = (double)rect.Width * inspection.paramFile.PixelPerUM_X;
				value9 = ((!inspection.paramFile.UseFloat) ? Math.Round(value9, 0) : Math.Round(value9, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value9);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value9);
				}
				if (value9 < num || value9 > num2)
				{
					result = false;
					if (value9 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value9 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 2:
			{
				double value6 = (double)rect.Height * inspection.paramFile.PixelPerUM_Y;
				value6 = ((!inspection.paramFile.UseFloat) ? Math.Round(value6, 0) : Math.Round(value6, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value6);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value6);
				}
				if (value6 < num || value6 > num2)
				{
					result = false;
					if (value6 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value6 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 3:
			{
				double value8 = Convert.ToDouble(area);
				value8 = ((!inspection.paramFile.UseFloat) ? Math.Round(value8, 0) : Math.Round(value8, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value8);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value8);
				}
				if (value8 < num || value8 > num2)
				{
					result = false;
					if (value8 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value8 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 4:
			{
				double value5 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				value5 = Math.Round(value5, 3);
				resultbox.Text = string.Format("{0:N3}", value5);
				if (value5 < num || value5 > num2)
				{
					result = false;
					if (value5 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value5 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 5:
			{
				double value2 = ((rect.Width * rect.Height > 0) ? (area / ((double)(rect.Width * rect.Height) * inspection.paramFile.PixelPerUM_X * inspection.paramFile.PixelPerUM_Y)) : 0.0);
				value2 = Math.Round(value2, 3);
				resultbox.Text = string.Format("{0:N3}", value2);
				Blobs[TrackerABIndex].AreaRatio = value2;
				if (value2 < num || value2 > num2)
				{
					result = false;
					if (value2 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value2 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 6:
			{
				if (TrackerABIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num3 = 0.0;
				resultbox.Text = string.Format("{0:N3}", num3);
				if (!(Blobs[0].Area > 0.0) || !(Blobs[1].Area > 0.0))
				{
					return false;
				}
				num3 = ((!(Blobs[0].Area > Blobs[1].Area)) ? (Blobs[1].Area / Blobs[0].Area) : (Blobs[0].Area / Blobs[1].Area));
				resultbox.Text = string.Format("{0:N3}", num3);
				num3 = Math.Round(num3, 3);
				Blobs[1].TwoAreaRatio = num3;
				if (num3 < num || num3 > num2)
				{
					result = false;
					if (num3 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num3 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 7:
			{
				if (TrackerABIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double value3 = Math.Abs(Blobs[0].Height - Blobs[1].Height);
				value3 = ((!inspection.paramFile.UseFloat) ? Math.Round(value3, 0) : Math.Round(value3, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value3);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value3);
				}
				if (value3 < num || value3 > num2)
				{
					result = false;
					if (value3 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value3 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 8:
			{
				if (TrackerABIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double value11 = Math.Abs(Blobs[0].Width - Blobs[1].Width);
				value11 = ((!inspection.paramFile.UseFloat) ? Math.Round(value11, 0) : Math.Round(value11, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value11);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value11);
				}
				if (value11 < num || value11 > num2)
				{
					result = false;
					if (value11 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value11 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 9:
			{
				if (TrackerABIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num4 = 0.0;
				if (Blobs[0].Width > 0.0 && Blobs[1].Width > 0.0)
				{
					num4 = ((!(Blobs[0].Width > Blobs[1].Width)) ? (Blobs[1].Width / Blobs[0].Width) : (Blobs[0].Width / Blobs[1].Width));
					num4 = Math.Round(num4, 3);
					Blobs[1].WidthRatio = num4;
					if (num4 < num || num4 > num2)
					{
						result = false;
						if (num4 < num)
						{
							min.Background = Brushes.Fuchsia;
						}
						if (num4 > num2)
						{
							max.Background = Brushes.Fuchsia;
						}
						resultbox.Foreground = Brushes.Red;
					}
				}
				resultbox.Text = string.Format("{0:N3}", num4);
				break;
			}
			case 10:
			{
				if (TrackerABIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num5 = 0.0;
				if (Blobs[0].Height > 0.0 && Blobs[1].Height > 0.0)
				{
					num5 = ((!(Blobs[0].Height > Blobs[1].Height)) ? (Blobs[1].Height / Blobs[0].Height) : (Blobs[0].Height / Blobs[1].Height));
					num5 = Math.Round(num5, 3);
					Blobs[1].HeightRatio = num5;
					if (num5 < num || num5 > num2)
					{
						result = false;
						if (num5 < num)
						{
							min.Background = Brushes.Fuchsia;
						}
						if (num5 > num2)
						{
							max.Background = Brushes.Fuchsia;
						}
						resultbox.Foreground = Brushes.Red;
					}
				}
				resultbox.Text = string.Format("{0:N3}", num5);
				break;
			}
			case 11:
			{
				double value7 = inspection.binaryTool[TrackIndex].AllArea[TrackerABIndex];
				value7 = ((!inspection.paramFile.UseFloat) ? Math.Round(value7, 0) : Math.Round(value7, 3));
				if (value7 < num || value7 > num2)
				{
					result = false;
					if (value7 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value7 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value7);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value7);
				}
				break;
			}
			case 12:
			{
				double value4 = inspection.binaryTool[TrackIndex].AllAreaRatio[TrackerABIndex];
				value4 = Math.Round(value4, 3);
				if (value4 < num || value4 > num2)
				{
					result = false;
					if (value4 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value4 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				resultbox.Text = string.Format("{0:N3}", value4);
				break;
			}
			case 13:
			{
				if (TrackerABIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double value10 = Math.Abs(inspection.binaryTool[TrackIndex].AllArea[1] - inspection.binaryTool[TrackIndex].AllArea[0]);
				value10 = ((!inspection.paramFile.UseFloat) ? Math.Round(value10, 0) : Math.Round(value10, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value10);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value10);
				}
				if (value10 < num || value10 > num2)
				{
					result = false;
					if (value10 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value10 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 14:
			{
				double val = (double)rect.Width * inspection.paramFile.PixelPerUM_X;
				double val2 = (double)rect.Height * inspection.paramFile.PixelPerUM_Y;
				double value = Math.Max(val, val2);
				value = ((!inspection.paramFile.UseFloat) ? Math.Round(value, 0) : Math.Round(value, 3));
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", value);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)value);
				}
				if (value < num || value > num2)
				{
					result = false;
					if (value < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (value > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BW03-BinaryWindow-GetRule()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DoThdImage(bool DrawAll = false)
	{
		if (PreviewMode || !inspection.buffers.LocatedReady || !UpdateUI)
		{
			return;
		}
		if (ShowBinaryCompare)
		{
			WriteUIValueToParam();
			RunShowBinaryCompare();
			return;
		}
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		if (ts_TrackerON.IsChecked == false)
		{
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
			return;
		}
		if (bor_SearchRect1.Uid == "1")
		{
			if (LanguageType == 1)
			{
				Label_ResultError.Text = "檢測區域A寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域A!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Text = "检测区域A宽高小于0，请检查对齐、自动内缩设定或是重设检测区域A!";
			}
			else if (LanguageType == 3)
			{
				Label_ResultError.Text = "検査領域Aの幅と高さが0未満です。アライメント、自動縮小設定を確認するか、検査領域Aをリセットしてください。";
			}
			else
			{
				Label_ResultError.Text = "Tracker A value < 0，Please check Aligment and Autofit value, or reset Tracker A! ";
			}
			Label_ResultError.Visibility = Visibility.Visible;
			return;
		}
		if (ts_Tracker2ON.IsChecked == true && bor_SearchRect2.Uid == "1")
		{
			if (LanguageType == 1)
			{
				Label_ResultError.Text = "檢測區域B寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域B!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Text = "检测区域B宽高小于0，请检查对齐、自动内缩设定或是重设检测区域B!";
			}
			else if (LanguageType == 3)
			{
				Label_ResultError.Text = "検査領域Bの幅と高さが0未満です。アライメント、自動縮小設定を確認するか、検査領域Bをリセットしてください。";
			}
			else
			{
				Label_ResultError.Text = "Tracker B value < 0，Please check Aligment and Autofit value, or reset Tracker B! ";
			}
			Label_ResultError.Visibility = Visibility.Visible;
			return;
		}
		Label_ResultError.Visibility = Visibility.Hidden;
		Mat[] array = new Mat[4];
		Mat[] array2 = new Mat[4];
		int[] array3 = new int[4];
		int[] array4 = new int[4];
		int[] array5 = new int[4];
		bool[] array6 = new bool[4];
		int[] array7 = new int[4];
		int[] array8 = new int[4];
		int[] array9 = new int[4];
		int[] array10 = new int[4];
		array6[0] = true;
		array6[1] = exp_THD2.IsExpanded;
		array6[2] = exp_THD3.IsExpanded;
		array6[3] = exp_THD4.IsExpanded;
		int num = 0;
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
		Mat mat = new Mat();
		try
		{
			if (!GetTracker(ref rect, 0) || (ts_Tracker2ON.IsChecked == true && !GetTracker(ref rect2, 1)))
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				array[i] = new Mat();
				array2[i] = new Mat();
				if (DrawAll)
				{
					array3[i] = 0;
					array4[i] = 255;
				}
				else
				{
					array3[i] = (int)THD_Sliders[i].LowerValue;
					array4[i] = (int)THD_Sliders[i].UpperValue;
				}
				array5[i] = THD_Channel[i].SelectedIndex;
				array7[i] = THD_DEStart[i].SelectedIndex;
				array8[i] = THD_DEEnd[i].SelectedIndex;
				array9[i] = THD_DEStartCount[i].SelectedIndex;
				array10[i] = THD_DEEndCount[i].SelectedIndex;
				if (!DrawAll)
				{
					THD_TextLow[i].Value = array3[i];
					THD_TextUp[i].Value = array4[i];
				}
				if (!array6[i])
				{
					continue;
				}
				int num2 = array5[i];
				if (num2 >= 16)
				{
					num2 += 5;
				}
				array[i] = inspection.buffers.GetChannelMat(num2).Clone();
				Cv2.InRange(array[i], array3[i], array4[i], array2[i]);
				Mat mat2 = new Mat(array2[i], rect);
				if (array7[i] != 0)
				{
					DoMorph(ref mat2, array7[i], array9[i]);
				}
				if (array8[i] != 0)
				{
					DoMorph(ref mat2, array8[i], array10[i]);
				}
				if (ts_Tracker2ON.IsChecked == true)
				{
					Mat mat3 = new Mat(array2[i], rect2);
					if (array7[i] != 0)
					{
						DoMorph(ref mat3, array7[i], array9[i]);
					}
					if (array8[i] != 0)
					{
						DoMorph(ref mat3, array8[i], array10[i]);
					}
				}
				num++;
			}
			if (num == 0)
			{
				return;
			}
			mat = array2[0].Clone();
			for (int j = 1; j < 4; j++)
			{
				if (array6[j])
				{
					if (j != 3 || chk_Thd_ORMode.IsChecked == false)
					{
						Cv2.BitwiseAnd(mat, array2[j], mat);
					}
					else
					{
						Cv2.BitwiseOr(mat, array2[j], mat);
					}
				}
			}
			BinaryBlob[0] = new Mat(mat, rect);
			if (ts_Tracker2ON.IsChecked == true)
			{
				BinaryBlob[1] = new Mat(mat, rect2);
			}
		}
		catch (Exception ex)
		{
			string text = "BW01-BinaryWindow-DoThdImage()-Threshold-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		Mat m = new Mat();
		if (cb_MaskSelect.SelectedIndex > 0)
		{
			try
			{
				int num3 = cb_MaskSelect.SelectedIndex - 1;
				Mat mat4 = mat.Clone();
				mat4.SetTo(0.0);
				bool flag = false;
				if (MaskRect[0].Width > 0 && MaskRect[0].Height > 0 && inspection.paramFile.MaskParam[num3].Mask1 != null)
				{
					Mat m2 = new Mat(mat4, MaskRect[0]);
					inspection.paramFile.MaskParam[num3].Mask1.Clone().CopyTo(m2);
					flag = true;
				}
				if (MaskRect[1].Width > 0 && MaskRect[1].Height > 0 && inspection.paramFile.MaskParam[num3].Mask2 != null)
				{
					Mat m3 = new Mat(mat4, MaskRect[1]);
					inspection.paramFile.MaskParam[num3].Mask2.Clone().CopyTo(m3);
					flag = true;
				}
				if (flag)
				{
					if (chk_UseMaskAsInspArea.IsChecked == true)
					{
						DoMorph(ref mat4, cb_Mask_DEStart.SelectedIndex, cb_Mask_DEStartCount.SelectedIndex);
						Cv2.BitwiseAnd(mat, mat4, mat);
					}
					else
					{
						DoMorph(ref mat4, cb_Mask_DEStart.SelectedIndex, cb_Mask_DEStartCount.SelectedIndex);
						Cv2.BitwiseNot(mat4, mat4);
						Cv2.BitwiseAnd(mat, mat4, mat);
					}
					mat4.CopyTo(m);
				}
			}
			catch (Exception ex2)
			{
				string text2 = "BW02-BinaryWindow-DoThdImage()-Do Mask!-";
				text2 += ex2.ToString();
				CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text2, "錯誤");
				}
				return;
			}
		}
		if (chk_UseFontMask.IsChecked == true && inspection.paramFile.FontToolON)
		{
			try
			{
				Mat mat5 = inspection.buffers.LocateGray.Clone();
				OpenCvSharp.Rect foundMatchTextRect = inspection.paramFile.FontToolParam.FoundMatchTextRect;
				mat5.SetTo(0.0);
				bool flag2 = false;
				if (foundMatchTextRect.Width > 0 && foundMatchTextRect.Height > 0)
				{
					Mat m4 = new Mat(mat5, foundMatchTextRect);
					Mat mat6 = inspection.paramFile.FontToolParam.TextMat.Clone();
					DoMorph(ref mat6, cb_Mask_DEStart.SelectedIndex, cb_Mask_DEStartCount.SelectedIndex);
					mat6.CopyTo(m4);
					flag2 = true;
				}
				if (flag2)
				{
					Cv2.BitwiseNot(mat5, mat5);
					Cv2.BitwiseAnd(mat, mat5, mat);
					mat5.CopyTo(m);
				}
			}
			catch (Exception ex3)
			{
				string text3 = "BW02-BinaryWindow-DoThdImage()-Do Mask!-";
				text3 += ex3.ToString();
				CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text3, "錯誤");
				}
				return;
			}
		}
		inspection.buffers.DoFilllet(ref mat, rect, cb_T1Filllet0.SelectedIndex, cb_T1Filllet1.SelectedIndex, cb_T1Filllet2.SelectedIndex, cb_T1Filllet3.SelectedIndex);
		if (ts_Tracker2ON.IsChecked == true)
		{
			inspection.buffers.DoFilllet(ref mat, rect2, cb_T2Filllet0.SelectedIndex, cb_T2Filllet1.SelectedIndex, cb_T2Filllet2.SelectedIndex, cb_T2Filllet3.SelectedIndex);
		}
		if (cb_MatchMaskSelect1.SelectedIndex > 0 && inspection.paramFile.MaskToolON)
		{
			try
			{
				if (inspection.paramFile.MaskToolON && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TrackerON && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextMatResult && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextRect.Width > 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextRect.Height > 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextMat != null)
				{
					OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
					OpenCvSharp.Rect rect4 = inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextRect & rect3;
					if (rect4.Width == inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextRect.Width && rect4.Height == inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextRect.Height && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextMat.Width >= 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextMat.Height >= 0)
					{
						using Mat mat7 = mat.Clone();
						mat7.SetTo(255.0);
						Mat m5 = new Mat(mat7, inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextRect);
						Mat mat8 = inspection.paramFile.MatchParam[cb_MatchMaskSelect1.SelectedIndex - 1].TextMat.Clone();
						DoMorph(ref mat8, cb_MatchText_DEStart1.SelectedIndex, cb_MatchText_DEStartCount1.SelectedIndex);
						Cv2.BitwiseNot(mat8, mat8);
						mat8.CopyTo(m5);
						Cv2.BitwiseAnd(mat, mat7, mat);
					}
				}
			}
			catch (Exception ex4)
			{
				string text4 = "BW02-BinaryWindow-DoThdImage()-Do MatchMask1!-";
				text4 += ex4.ToString();
				CVBuffer.SaveLog(text4, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text4, "錯誤");
				}
				return;
			}
		}
		if (cb_MatchMaskSelect2.SelectedIndex > 0)
		{
			try
			{
				if (inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TrackerON && inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextMatResult && inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextRect.Width > 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextRect.Height > 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextMat != null)
				{
					OpenCvSharp.Rect rect5 = new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
					OpenCvSharp.Rect rect6 = inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextRect & rect5;
					if (rect6.Width == inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextRect.Width && rect6.Height == inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextRect.Height && inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextMat.Width >= 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextMat.Height >= 0)
					{
						using Mat mat9 = mat.Clone();
						mat9.SetTo(255.0);
						Mat m6 = new Mat(mat9, inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextRect);
						Mat mat10 = inspection.paramFile.MatchParam[cb_MatchMaskSelect2.SelectedIndex - 1].TextMat.Clone();
						DoMorph(ref mat10, cb_MatchText_DEStart2.SelectedIndex, cb_MatchText_DEStartCount2.SelectedIndex);
						Cv2.BitwiseNot(mat10, mat10);
						mat10.CopyTo(m6);
						Cv2.BitwiseAnd(mat, mat9, mat);
					}
				}
			}
			catch (Exception ex5)
			{
				string text5 = "BW02-BinaryWindow-DoThdImage()-Do MatchMask2!-";
				text5 += ex5.ToString();
				CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text5, "錯誤");
				}
				return;
			}
		}
		if (cb_MatchMaskSelect3.SelectedIndex > 0)
		{
			try
			{
				if (inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TrackerON && inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextMatResult && inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextRect.Width > 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextRect.Height > 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextMat != null)
				{
					OpenCvSharp.Rect rect7 = new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
					OpenCvSharp.Rect rect8 = inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextRect & rect7;
					if (rect8.Width == inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextRect.Width && rect8.Height == inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextRect.Height && inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextMat.Width >= 0 && inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextMat.Height >= 0)
					{
						using Mat mat11 = mat.Clone();
						mat11.SetTo(255.0);
						Mat m7 = new Mat(mat11, inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextRect);
						Mat mat12 = inspection.paramFile.MatchParam[cb_MatchMaskSelect3.SelectedIndex - 1].TextMat.Clone();
						DoMorph(ref mat12, cb_MatchText_DEStart3.SelectedIndex, cb_MatchText_DEStartCount3.SelectedIndex);
						Cv2.BitwiseNot(mat12, mat12);
						mat12.CopyTo(m7);
						Cv2.BitwiseAnd(mat, mat11, mat);
					}
				}
			}
			catch (Exception ex6)
			{
				string text6 = "BW02-BinaryWindow-DoThdImage()-Do MatchMask3!-";
				text6 += ex6.ToString();
				CVBuffer.SaveLog(text6, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text6, "錯誤");
				}
				return;
			}
		}
		try
		{
			Mat mat13 = mat.Clone();
			mat13.SetTo(0.0);
			new Mat(mat13, rect).SetTo(255.0);
			if (ts_Tracker2ON.IsChecked.Value)
			{
				new Mat(mat13, rect2).SetTo(255.0);
			}
			Cv2.BitwiseAnd(mat, mat13, mat);
			Mat mat14 = new Mat();
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			Cv2.Merge(ThdShow, mat14);
			if (DrawAll)
			{
				_ = new Point2f[4];
				Cv2.FindContours(mat, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
				Cv2.DrawContours(ShowBitmap, contours, -1, new Scalar(255.0, 0.0, 255.0), 2);
			}
			else
			{
				mat14.CopyTo(ShowBitmap, mat);
			}
			ImageSource imageSource2 = ShowBitmap.ToBitmapSource();
			ImageBrush imageBrush2 = new ImageBrush();
			imageBrush2.ImageSource = imageSource2;
			cnv_Image.Background = imageBrush2;
			imageSource2 = null;
			THD_Preview = true;
		}
		catch (Exception ex7)
		{
			string text7 = "BW03-BinaryWindow-DoThdImage()-Draw to Screen-";
			text7 += ex7.ToString();
			CVBuffer.SaveLog(text7, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text7, "錯誤");
			}
		}
	}

	private void DoMorph(ref Mat mat, int index, int count)
	{
		if (count != 0 || index == 10)
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
				Cv2.MorphologyEx(mat, mat, MorphTypes.Gradient, new Mat(), null, count);
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
	private void Cb_TrackerSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		if (!ReferenceLock)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("顏色參考數值已被更動，是否儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("颜色参考数值已被更动，是否储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Color Reference Parameters has been modified, Save Recipe?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("カラー参照値が変更されました。保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				Btn_ReferenceSave_Click(sender, null);
			}
			ReferenceLock = true;
			btn_ReferenceSave.IsEnabled = false;
		}
		ShowBinaryCompare = false;
		btn_BinaryCompare.Background = Brushes.White;
		Label_ResultError.Visibility = Visibility.Hidden;
		WriteUIValueToParam();
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		BackupParam = inspection.paramFile.BinaryParam[TrackIndex];
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		CopyValueToUI();
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
		else
		{
			DrawBackGround();
		}
		UpdateToolBox();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("檢測區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("检测区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Tracker value will set to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("検査領域の位置がデフォルト値にリセットされます。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		if (((Button)sender).Uid == "0")
		{
			bor_SearchRect1.SetValue(Canvas.LeftProperty, 0.0);
			bor_SearchRect1.SetValue(Canvas.TopProperty, 0.0);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, 100.0);
			bor_SearchRect1.Visibility = Visibility.Visible;
			bor_SearchRect1.Uid = "0";
			btn_ResetA.Visibility = Visibility.Hidden;
			DontSave1 = false;
			if (bor_SearchRect2.Uid == "0")
			{
				btn_Test.IsEnabled = true;
				DontSave2 = false;
				Label_ResultError.Visibility = Visibility.Hidden;
			}
		}
		else
		{
			bor_SearchRect2.SetValue(Canvas.LeftProperty, 100.0);
			bor_SearchRect2.SetValue(Canvas.TopProperty, 0.0);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, 100.0);
			bor_SearchRect2.Visibility = Visibility.Visible;
			bor_SearchRect2.Uid = "0";
			btn_ResetB.Visibility = Visibility.Hidden;
			DontSave2 = false;
			if (bor_SearchRect1.Uid == "0")
			{
				btn_Test.IsEnabled = true;
				DontSave1 = false;
				Label_ResultError.Visibility = Visibility.Hidden;
			}
		}
	}

	private void Cnv_Image_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (cb_ColorReference.SelectedIndex > 0 && !btn_ReferenceSave.IsEnabled)
		{
			return;
		}
		System.Windows.Point position = e.GetPosition(this);
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			int num = (int)((position.X - cnv_Image.Margin.Left) / xRatio);
			int num2 = (int)(position.Y / yRatio);
			if (num < ImageWidth && num >= 0 && num2 < ImageHeight && num2 >= 0)
			{
				lab_RValue.Content = (CurrentColor[0] = inspection.buffers.GetLocatedBGR(0).At<byte>(num2, num));
				lab_GValue.Content = (CurrentColor[1] = inspection.buffers.GetLocatedBGR(1).At<byte>(num2, num));
				lab_BValue.Content = (CurrentColor[2] = inspection.buffers.GetLocatedBGR(2).At<byte>(num2, num));
				lab_HValue.Content = (CurrentColor[3] = inspection.buffers.GetLocatedHSV(0).At<byte>(num2, num));
				lab_SValue.Content = (CurrentColor[4] = inspection.buffers.GetLocatedHSV(1).At<byte>(num2, num));
				lab_VValue.Content = (CurrentColor[5] = inspection.buffers.GetLocatedHSV(2).At<byte>(num2, num));
				lab_GrayValue.Content = (CurrentColor[6] = inspection.buffers.LocateGray.At<byte>(num2, num));
				lab_C1Value.Content = (CurrentColor[7] = inspection.buffers.GetLocatedCustomMat(0).At<byte>(num2, num));
				lab_C2Value.Content = (CurrentColor[8] = inspection.buffers.GetLocatedCustomMat(1).At<byte>(num2, num));
				lab_C3Value.Content = (CurrentColor[9] = inspection.buffers.GetLocatedCustomMat(2).At<byte>(num2, num));
				lab_AIRValue.Content = (CurrentColor[10] = inspection.buffers.GetChannelMat(10).At<byte>(num2, num));
				lab_AIGValue.Content = (CurrentColor[11] = inspection.buffers.GetChannelMat(11).At<byte>(num2, num));
				lab_AIBValue.Content = (CurrentColor[12] = inspection.buffers.GetChannelMat(12).At<byte>(num2, num));
				CurrentColor[13] = inspection.buffers.GetLocatedCustomMat(3).At<byte>(num2, num);
				CurrentColor[14] = inspection.buffers.GetLocatedCustomMat(4).At<byte>(num2, num);
				CurrentColor[15] = inspection.buffers.GetLocatedCustomMat(5).At<byte>(num2, num);
				Label label = lab_XValue;
				int num3 = num;
				label.Content = num3.ToString();
				Label label2 = lab_YValue;
				num3 = num2;
				label2.Content = num3.ToString();
			}
		}
	}

	private void Scr_TrackerSwich_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	private void Scr_TrackerSwich_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if ((User > User_Permissions.USER_Engineer || !inspection.paramFile.BinaryParam[TrackIndex].Locked) && !PreviewMode)
		{
			if (ts_TrackerON.IsChecked.Value)
			{
				inspection.paramFile.BinaryParam[TrackIndex].TrackerON = true;
				grd_SIMP_THD_1.Visibility = Visibility.Visible;
				grd_SIMP_THD_2.Visibility = Visibility.Visible;
				grd_SIMP_THD_3.Visibility = Visibility.Visible;
				grd_SIMP_THD_4.Visibility = Visibility.Visible;
				CopyValueToUI();
				return;
			}
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_SearchRect2.Visibility = Visibility.Hidden;
			inspection.paramFile.BinaryParam[TrackIndex].TrackerON = false;
			inspection.paramFile.BinaryParam[TrackIndex].ValueChangeFlag = true;
			grd_SIMP_THD_1.Visibility = Visibility.Hidden;
			grd_SIMP_THD_2.Visibility = Visibility.Hidden;
			grd_SIMP_THD_3.Visibility = Visibility.Hidden;
			grd_SIMP_THD_4.Visibility = Visibility.Hidden;
			grd_SIMP_Setup.Visibility = Visibility.Hidden;
			grd_SIMP_RefSetup.Visibility = Visibility.Hidden;
			DoThdImage();
			ToolsButton[TrackIndex].Background = Brushes.White;
		}
	}

	private void Bor_SearchRect1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Ts_Tracker2ON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_Tracker2ON.IsChecked.Value)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
			txt_Tracker2Pos.Visibility = Visibility.Visible;
			grd_Track2Setup.Visibility = Visibility.Visible;
			SetConditionText(tracker2ON: true);
			grd_Tracker2Filllet.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect2.Uid = "0";
			bor_SearchRect2.Visibility = Visibility.Hidden;
			txt_Tracker2Pos.Visibility = Visibility.Hidden;
			grd_Track2Setup.Visibility = Visibility.Hidden;
			SetConditionText(tracker2ON: false);
			grd_Tracker2Filllet.Visibility = Visibility.Hidden;
		}
	}

	private void SetConditionText(bool tracker2ON)
	{
		int[] array = new int[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = InspRule[i].SelectedIndex;
			InspRule[i].Items.Clear();
		}
		int num = 15;
		string text = "";
		for (int j = 0; j < num; j++)
		{
			text = GetRuleText(j);
			if (tracker2ON)
			{
				cb_InspRule1.Items.Add(text);
				cb_InspRule2.Items.Add(text);
				cb_InspRule3.Items.Add(text);
				cb_InspRule4.Items.Add(text);
			}
			else if ((j >= 6 && j <= 10) || j == 13)
			{
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = text;
				comboBoxItem.IsEnabled = false;
				cb_InspRule1.Items.Add(comboBoxItem);
				ComboBoxItem comboBoxItem2 = new ComboBoxItem();
				comboBoxItem2.Content = text;
				comboBoxItem2.IsEnabled = false;
				cb_InspRule2.Items.Add(comboBoxItem2);
				ComboBoxItem comboBoxItem3 = new ComboBoxItem();
				comboBoxItem3.Content = text;
				comboBoxItem3.IsEnabled = false;
				cb_InspRule3.Items.Add(comboBoxItem3);
				ComboBoxItem comboBoxItem4 = new ComboBoxItem();
				comboBoxItem4.Content = text;
				comboBoxItem4.IsEnabled = false;
				cb_InspRule4.Items.Add(comboBoxItem4);
			}
			else
			{
				cb_InspRule1.Items.Add(text);
				cb_InspRule2.Items.Add(text);
				cb_InspRule3.Items.Add(text);
				cb_InspRule4.Items.Add(text);
			}
		}
		for (int k = 0; k < 4; k++)
		{
			InspRule[k].SelectedIndex = array[k];
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetRuleText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["Disable"], 
			1 => LanguageManager.Instance["Width"], 
			2 => LanguageManager.Instance["Height"], 
			3 => LanguageManager.Instance["Area"], 
			4 => LanguageManager.Instance["WHRatio"], 
			5 => LanguageManager.Instance["AreaRatio"], 
			6 => LanguageManager.Instance["ABAreaRatio"], 
			7 => LanguageManager.Instance["ABHDiff"], 
			8 => LanguageManager.Instance["ABWDiff"], 
			9 => LanguageManager.Instance["ABWRatio"], 
			10 => LanguageManager.Instance["ABHRatio"], 
			11 => LanguageManager.Instance["AllArea"], 
			12 => LanguageManager.Instance["AllAreaRatio"], 
			13 => LanguageManager.Instance["ABAllAreaDiff"], 
			14 => LanguageManager.Instance["LongestSide"], 
			_ => "-----", 
		};
	}

	private void Btn_ColorDropper_Click(object sender, RoutedEventArgs e)
	{
		if (!ColorDropper)
		{
			if (ts_TrackerON.IsChecked == true)
			{
				cnv_Image.Cursor = Cursors.Cross;
				ColorDropper = true;
			}
		}
		else
		{
			cnv_Image.Cursor = Cursors.Arrow;
			ColorDropper = false;
		}
	}

	private void Cnv_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if ((cb_ColorReference.SelectedIndex > 0 && !btn_ReferenceSave.IsEnabled) || !ColorDropper)
		{
			return;
		}
		cnv_Image.Cursor = Cursors.Arrow;
		for (int i = 0; i < 4; i++)
		{
			if (THD_Expender[i].IsExpanded || i == 0)
			{
				int num = CurrentColor[THD_Channel[i].SelectedIndex];
				int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				THD_Sliders[i].LowerValue = num2;
				THD_Sliders[i].UpperValue = num3;
			}
		}
		DoThdImage();
		ColorDropper = false;
	}

	private void Btn_ReDraw_Click(object sender, RoutedEventArgs e)
	{
		DoThdImage();
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
			if (!inspection.LocaetResult || !inspection.AdvlocateResult)
			{
				cnv_Image.Background = Brushes.LightGray;
				THD_Preview = true;
				DontSave = true;
				btn_Test.IsEnabled = false;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				FaceIndex = inspection.SelectFaceResult;
				SetFaceIndex(FaceIndex);
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-BinaryWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
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
			if (!inspection.LocaetResult || !inspection.AdvlocateResult)
			{
				DontSave = true;
				cnv_Image.Background = Brushes.LightGray;
				THD_Preview = true;
				btn_Test.IsEnabled = false;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				FaceIndex = inspection.SelectFaceResult;
				SetFaceIndex(FaceIndex);
				DontSave = false;
				mat = new Mat();
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				btn_Test.IsEnabled = true;
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-BinaryWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Btn_Tools1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		cb_TrackerSelect.SelectedIndex = num;
		for (int i = 0; i < 30; i++)
		{
			ToolsButton[i].BorderBrush = Brushes.Gray;
		}
		ToolsButton[num].BorderBrush = Brushes.Blue;
	}

	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		btn_RecipePaste.IsEnabled = true;
		CopyParamSeted = true;
		CopyParam = inspection.paramFile.BinaryParam[TrackIndex];
		btn_RecipeCopy.Background = Brushes.PaleGreen;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if ((User > User_Permissions.USER_Engineer || !inspection.paramFile.BinaryParam[TrackIndex].Locked) && CopyParamSeted)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在の検査領域の数値は上書きされます。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes && messageBoxResult == MessageBoxResult.Yes)
			{
				string trackerName = inspection.paramFile.BinaryParam[TrackIndex].TrackerName;
				inspection.paramFile.BinaryParam[TrackIndex] = CopyParam;
				inspection.paramFile.BinaryParam[TrackIndex].TrackerName = trackerName;
				CopyValueToUI();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Reverse_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("檢測參數將還原至上次儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("检测参数将还原至上次储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Parameters will Revert to Previous save！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("検査パラメータは、前回の保存内容に復元されます。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes && messageBoxResult == MessageBoxResult.Yes)
		{
			inspection.paramFile.BinaryParam[TrackIndex] = inspection.paramFile_Backup.BinaryParam[TrackIndex];
			CopyValueToUI();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_PrevTracker(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		if (!ReferenceLock)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("顏色參考數值已被更動，是否儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("颜色参考数值已被更动，是否储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Color Reference Parameters has been modified, Save Recipe ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("カラー参照値が変更されました。保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				Btn_ReferenceSave_Click(sender, null);
			}
			ReferenceLock = true;
			btn_ReferenceSave.IsEnabled = false;
		}
		if (ts_TrackerON.IsChecked == true)
		{
			WriteUIValueToParam();
		}
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
		}
		UpdateUI = false;
		if (TrackIndex == 0)
		{
			cb_TrackerSelect.SelectedIndex = 29;
		}
		else
		{
			cb_TrackerSelect.SelectedIndex--;
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		BackupParam = inspection.paramFile.BinaryParam[TrackIndex];
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		UpdateUI = true;
		CopyValueToUI();
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
		else
		{
			DrawBackGround();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NextTracker_Click(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		if (!ReferenceLock)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("顏色參考數值已被更動，是否儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("颜色参考数值已被更动，是否储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Color Reference Parameters has been modified, Save Recipe ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("カラー参照値が変更されました。保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				Btn_ReferenceSave_Click(sender, null);
			}
			ReferenceLock = true;
			btn_ReferenceSave.IsEnabled = false;
		}
		if (ts_TrackerON.IsChecked == true)
		{
			WriteUIValueToParam();
		}
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
		}
		UpdateUI = false;
		if (TrackIndex == 29)
		{
			cb_TrackerSelect.SelectedIndex = 0;
		}
		else
		{
			cb_TrackerSelect.SelectedIndex++;
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		UpdateUI = true;
		BackupParam = inspection.paramFile.BinaryParam[TrackIndex];
		CopyValueToUI();
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
		else
		{
			DrawBackGround();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Tracker1SizeChange_Click(object sender, RoutedEventArgs e)
	{
		Edit2InputWindow edit2InputWindow = new Edit2InputWindow();
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect, 0);
		GetTracker(ref rect2, 1);
		Button obj = (Button)sender;
		if (obj.Uid == "1")
		{
			edit2InputWindow.SetMinMaxValue(0, ImageWidth, 0, ImageHeight);
			edit2InputWindow.SetValue(rect.Width, rect.Height);
			edit2InputWindow.ShowDialog();
			if (edit2InputWindow.SetSuccess)
			{
				rect.Width = edit2InputWindow.Value1;
				rect.Height = edit2InputWindow.Value2;
				SetTracker(rect, 0);
			}
		}
		if (obj.Uid == "2" && ts_Tracker2ON.IsChecked == true)
		{
			edit2InputWindow.SetMinMaxValue(0, ImageWidth, 0, ImageHeight);
			edit2InputWindow.SetValue(rect2.Width, rect2.Height);
			edit2InputWindow.ShowDialog();
			if (edit2InputWindow.SetSuccess)
			{
				rect2.Width = edit2InputWindow.Value1;
				rect2.Height = edit2InputWindow.Value2;
				SetTracker(rect2, 1);
			}
		}
	}

	private void Btn_ReferenceLock_Click(object sender, RoutedEventArgs e)
	{
		if (cb_ColorReference.SelectedIndex <= 0 || (ThroughputFile.CurrentClient == 6 && User < User_Permissions.USER_Administrator))
		{
			return;
		}
		if (ReferenceLock)
		{
			ReferenceLock = false;
			btn_ReferenceSave.IsEnabled = true;
			txt_ReferenceName.IsEnabled = true;
			for (int i = 0; i < 4; i++)
			{
				THD_Sliders[i].IsEnabled = true;
				THD_Expender[i].IsEnabled = true;
			}
			grd_SIMP_THD_1.IsEnabled = true;
		}
		else
		{
			ReferenceLock = true;
			btn_ReferenceSave.IsEnabled = false;
			txt_ReferenceName.IsEnabled = false;
			for (int j = 0; j < 4; j++)
			{
				THD_Sliders[j].IsEnabled = false;
				THD_Expender[j].IsEnabled = false;
			}
			grd_SIMP_THD_1.IsEnabled = false;
		}
	}

	private void Cb_ColorReference_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (cb_ColorReference.SelectedIndex > 0)
		{
			grd_SIMP_RefSetup.Background = Brushes.LightGoldenrodYellow;
			ReferenceLock = true;
			btn_ReferenceSave.IsEnabled = false;
			txt_ReferenceName.IsEnabled = false;
			for (int i = 0; i < 4; i++)
			{
				THD_Sliders[i].IsEnabled = false;
				THD_Expender[i].IsEnabled = false;
			}
			grd_SIMP_THD_1.IsEnabled = false;
			UpdateUI = false;
			INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = inspection.paramFile.ColorReferenceParam[cb_ColorReference.SelectedIndex - 1];
			txt_ReferenceName.Text = iNSP_COLORREFERENCE_PARAM.TrackerName;
			cb_Channel_THD1.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD1_ChannelIndex;
			cb_Channel_THD2.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD2_ChannelIndex;
			cb_Channel_THD3.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD3_ChannelIndex;
			cb_Channel_THD4.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD4_ChannelIndex;
			exp_THD1.IsExpanded = true;
			exp_THD2.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD2_ChannelON;
			exp_THD3.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD3_ChannelON;
			exp_THD4.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD4_ChannelON;
			rs_THD1.LowerValue = iNSP_COLORREFERENCE_PARAM.THD1_Min;
			rs_THD1.UpperValue = iNSP_COLORREFERENCE_PARAM.THD1_Max;
			nu_Thd1LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD1_Min;
			nu_Thd1LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD1_Max;
			rs_THD2.LowerValue = iNSP_COLORREFERENCE_PARAM.THD2_Min;
			rs_THD2.UpperValue = iNSP_COLORREFERENCE_PARAM.THD2_Max;
			nu_Thd2LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD2_Min;
			nu_Thd2LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD2_Max;
			rs_THD3.LowerValue = iNSP_COLORREFERENCE_PARAM.THD3_Min;
			rs_THD3.UpperValue = iNSP_COLORREFERENCE_PARAM.THD3_Max;
			nu_Thd3LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD3_Min;
			nu_Thd3LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD3_Max;
			rs_THD4.LowerValue = iNSP_COLORREFERENCE_PARAM.THD4_Min;
			rs_THD4.UpperValue = iNSP_COLORREFERENCE_PARAM.THD4_Max;
			nu_Thd4LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD4_Min;
			nu_Thd4LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD4_Max;
			cb_THD1_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End3_Index;
			cb_THD4_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start4_Index;
			cb_THD4_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End4_Index;
			cb_THD1_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End3_Count;
			cb_THD4_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start4_Count;
			cb_THD4_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End4_Count;
			UpdateUI = true;
		}
		else if (ts_TrackerON.IsChecked != false)
		{
			grd_SIMP_RefSetup.Background = Brushes.White;
			txt_ReferenceName.Text = "";
			UpdateUI = false;
			for (int j = 0; j < 4; j++)
			{
				THD_Sliders[j].IsEnabled = true;
				THD_Expender[j].IsEnabled = true;
			}
			grd_SIMP_THD_1.IsEnabled = true;
			INSP_BINARYTOOL_PARAM iNSP_BINARYTOOL_PARAM = inspection.paramFile.BinaryParam[TrackIndex];
			cb_Channel_THD1.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.THD1_ChannelIndex;
			cb_Channel_THD2.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.THD2_ChannelIndex;
			cb_Channel_THD3.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.THD3_ChannelIndex;
			cb_Channel_THD4.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.THD4_ChannelIndex;
			THD_Expender[0].IsExpanded = iNSP_BINARYTOOL_PARAM.THD1_ChannelON;
			THD_Expender[1].IsExpanded = iNSP_BINARYTOOL_PARAM.THD2_ChannelON;
			THD_Expender[2].IsExpanded = iNSP_BINARYTOOL_PARAM.THD3_ChannelON;
			THD_Expender[3].IsExpanded = iNSP_BINARYTOOL_PARAM.THD4_ChannelON;
			chk_Thd_ORMode.IsChecked = iNSP_BINARYTOOL_PARAM.THD_ORMode;
			rs_THD1.LowerValue = iNSP_BINARYTOOL_PARAM.THD1_Min;
			rs_THD1.UpperValue = iNSP_BINARYTOOL_PARAM.THD1_Max;
			nu_Thd1LimitMin.Value = iNSP_BINARYTOOL_PARAM.THD1_Min;
			nu_Thd1LimitMax.Value = iNSP_BINARYTOOL_PARAM.THD1_Max;
			rs_THD2.LowerValue = iNSP_BINARYTOOL_PARAM.THD2_Min;
			rs_THD2.UpperValue = iNSP_BINARYTOOL_PARAM.THD2_Max;
			nu_Thd2LimitMin.Value = iNSP_BINARYTOOL_PARAM.THD2_Min;
			nu_Thd2LimitMax.Value = iNSP_BINARYTOOL_PARAM.THD2_Max;
			rs_THD3.LowerValue = iNSP_BINARYTOOL_PARAM.THD3_Min;
			rs_THD3.UpperValue = iNSP_BINARYTOOL_PARAM.THD3_Max;
			nu_Thd3LimitMin.Value = iNSP_BINARYTOOL_PARAM.THD3_Min;
			nu_Thd3LimitMax.Value = iNSP_BINARYTOOL_PARAM.THD3_Max;
			rs_THD4.LowerValue = iNSP_BINARYTOOL_PARAM.THD4_Min;
			rs_THD4.UpperValue = iNSP_BINARYTOOL_PARAM.THD4_Max;
			nu_Thd4LimitMin.Value = iNSP_BINARYTOOL_PARAM.THD4_Min;
			nu_Thd4LimitMax.Value = iNSP_BINARYTOOL_PARAM.THD4_Max;
			cb_THD1_DEStart.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_End3_Index;
			cb_THD4_DEStart.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_Start4_Index;
			cb_THD4_DEEnd.SelectedIndex = (int)iNSP_BINARYTOOL_PARAM.DE_End4_Index;
			cb_THD1_DEStartCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_End3_Count;
			cb_THD4_DEStartCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_Start4_Count;
			cb_THD4_DEEndCount.SelectedIndex = iNSP_BINARYTOOL_PARAM.DE_End4_Count;
			UpdateUI = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ReferenceSave_Click(object sender, RoutedEventArgs e)
	{
		if (ReferenceLock || cb_ColorReference.SelectedIndex <= 0)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		UpdateUI = false;
		INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = new INSP_COLORREFERENCE_PARAM
		{
			TrackerName = txt_ReferenceName.Text,
			ValueChangeFlag = true,
			THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex,
			THD1_ChannelON = true,
			THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex,
			THD2_ChannelON = exp_THD2.IsExpanded,
			THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex,
			THD3_ChannelON = exp_THD3.IsExpanded,
			THD4_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD4.SelectedIndex,
			THD4_ChannelON = exp_THD4.IsExpanded,
			THD1_Min = (int)rs_THD1.LowerValue,
			THD1_Max = (int)rs_THD1.UpperValue,
			THD2_Min = (int)rs_THD2.LowerValue,
			THD2_Max = (int)rs_THD2.UpperValue,
			THD3_Min = (int)rs_THD3.LowerValue,
			THD3_Max = (int)rs_THD3.UpperValue,
			THD4_Min = (int)rs_THD4.LowerValue,
			THD4_Max = (int)rs_THD4.UpperValue,
			DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex,
			DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex,
			DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex,
			DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex,
			DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex,
			DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex,
			DE_Start4_Index = (USE_DILATE_ERODE)cb_THD4_DEStart.SelectedIndex,
			DE_End4_Index = (USE_DILATE_ERODE)cb_THD4_DEEnd.SelectedIndex,
			DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex,
			DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex,
			DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex,
			DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex,
			DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex,
			DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex,
			DE_Start4_Count = cb_THD4_DEStartCount.SelectedIndex,
			DE_End4_Count = cb_THD4_DEEndCount.SelectedIndex
		};
		inspection.paramFile.ColorReferenceParam[cb_ColorReference.SelectedIndex - 1] = iNSP_COLORREFERENCE_PARAM;
		int selectedIndex = cb_ColorReference.SelectedIndex;
		cb_ColorReference.Items.Clear();
		for (int i = 0; i <= 20; i++)
		{
			if (i != 0)
			{
				cb_ColorReference.Items.Add(i + " : " + inspection.paramFile.ColorReferenceParam[i - 1].TrackerName);
			}
			else
			{
				cb_ColorReference.Items.Add("-----");
			}
		}
		cb_ColorReference.SelectedIndex = selectedIndex;
		ReferenceLock = true;
		btn_ReferenceSave.IsEnabled = false;
		txt_ReferenceName.IsEnabled = false;
		for (int j = 0; j < 4; j++)
		{
			THD_Sliders[j].IsEnabled = false;
			THD_Expender[j].IsEnabled = false;
		}
		grd_SIMP_THD_1.IsEnabled = false;
		WriteUIValueToParam();
		UpdateUI = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ColorPaste_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("目前二值化數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("目前二值化数值将被覆盖！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Binary Threshold value will Overwrite！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在の二値化数値は上書きされます。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			cb_Channel_THD1.SelectedIndex = (int)CopyColorParam.THD1_ChannelIndex;
			cb_Channel_THD2.SelectedIndex = (int)CopyColorParam.THD2_ChannelIndex;
			cb_Channel_THD3.SelectedIndex = (int)CopyColorParam.THD3_ChannelIndex;
			cb_Channel_THD4.SelectedIndex = (int)CopyColorParam.THD4_ChannelIndex;
			exp_THD1.IsExpanded = true;
			exp_THD2.IsExpanded = CopyColorParam.THD2_ChannelON;
			exp_THD3.IsExpanded = CopyColorParam.THD3_ChannelON;
			exp_THD4.IsExpanded = CopyColorParam.THD4_ChannelON;
			rs_THD1.LowerValue = CopyColorParam.THD1_Min;
			rs_THD1.UpperValue = CopyColorParam.THD1_Max;
			nu_Thd1LimitMin.Value = CopyColorParam.THD1_Min;
			nu_Thd1LimitMax.Value = CopyColorParam.THD1_Max;
			rs_THD2.LowerValue = CopyColorParam.THD2_Min;
			rs_THD2.UpperValue = CopyColorParam.THD2_Max;
			nu_Thd2LimitMin.Value = CopyColorParam.THD2_Min;
			nu_Thd2LimitMax.Value = CopyColorParam.THD2_Max;
			rs_THD3.LowerValue = CopyColorParam.THD3_Min;
			rs_THD3.UpperValue = CopyColorParam.THD3_Max;
			nu_Thd3LimitMin.Value = CopyColorParam.THD3_Min;
			nu_Thd3LimitMax.Value = CopyColorParam.THD3_Max;
			rs_THD4.LowerValue = CopyColorParam.THD4_Min;
			rs_THD4.UpperValue = CopyColorParam.THD4_Max;
			nu_Thd4LimitMin.Value = CopyColorParam.THD4_Min;
			nu_Thd4LimitMax.Value = CopyColorParam.THD4_Max;
			cb_THD1_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End3_Index;
			cb_THD4_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start4_Index;
			cb_THD4_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End4_Index;
			cb_THD1_DEStartCount.SelectedIndex = CopyColorParam.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = CopyColorParam.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = CopyColorParam.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = CopyColorParam.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = CopyColorParam.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = CopyColorParam.DE_End3_Count;
			cb_THD4_DEStartCount.SelectedIndex = CopyColorParam.DE_Start4_Count;
			cb_THD4_DEEndCount.SelectedIndex = CopyColorParam.DE_End4_Count;
		}
	}

	private void Btn_ColorCopy_Click(object sender, RoutedEventArgs e)
	{
		btn_ColorPaste.IsEnabled = true;
		CopyColorParam.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
		CopyColorParam.THD1_ChannelON = true;
		CopyColorParam.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
		CopyColorParam.THD2_ChannelON = exp_THD2.IsExpanded;
		CopyColorParam.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
		CopyColorParam.THD3_ChannelON = exp_THD3.IsExpanded;
		CopyColorParam.THD4_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD4.SelectedIndex;
		CopyColorParam.THD4_ChannelON = exp_THD4.IsExpanded;
		CopyColorParam.THD1_Min = (int)rs_THD1.LowerValue;
		CopyColorParam.THD1_Max = (int)rs_THD1.UpperValue;
		CopyColorParam.THD2_Min = (int)rs_THD2.LowerValue;
		CopyColorParam.THD2_Max = (int)rs_THD2.UpperValue;
		CopyColorParam.THD3_Min = (int)rs_THD3.LowerValue;
		CopyColorParam.THD3_Max = (int)rs_THD3.UpperValue;
		CopyColorParam.THD4_Min = (int)rs_THD4.LowerValue;
		CopyColorParam.THD4_Max = (int)rs_THD4.UpperValue;
		CopyColorParam.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
		CopyColorParam.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
		CopyColorParam.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
		CopyColorParam.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start4_Index = (USE_DILATE_ERODE)cb_THD4_DEStart.SelectedIndex;
		CopyColorParam.DE_End4_Index = (USE_DILATE_ERODE)cb_THD4_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
		CopyColorParam.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
		CopyColorParam.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
		CopyColorParam.DE_Start4_Count = cb_THD4_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End4_Count = cb_THD4_DEEndCount.SelectedIndex;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_MaskSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited || !UpdateUI)
		{
			return;
		}
		if (cb_MaskSelect.SelectedIndex > 0)
		{
			grd_Mask.Background = Brushes.LightYellow;
			int num = cb_MaskSelect.SelectedIndex - 1;
			if (!inspection.paramFile.MaskParam[num].TrackerON)
			{
				if (LanguageType == 1)
				{
					MessageBox.Show("所選擇的遮罩區域尚未啟用！", "警告", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
				}
				else if (LanguageType == 2)
				{
					MessageBox.Show("所选择的遮罩区域尚未启用！", "警告", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
				}
				else if (LanguageType == 3)
				{
					MessageBox.Show("選択したマスク領域はまだ有効になっていません。", "警告", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
				}
				else
				{
					MessageBox.Show("Selected Mask area has been disabled！", "Warning", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
				}
				UpdateUI = false;
				cb_MaskSelect.SelectedIndex = 0;
				UpdateUI = true;
				return;
			}
			rt_Mask1.Visibility = Visibility.Visible;
			if (inspection.paramFile.MaskParam[num].SearchRect2ON)
			{
				rt_Mask2.Visibility = Visibility.Visible;
			}
			if (inspection.paramFile.MaskParam[num].Mask1 != null)
			{
				MaskRect[0] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[num].Mask1Pt.X, inspection.paramFile.MaskParam[num].Mask1Pt.Y, inspection.paramFile.MaskParam[num].Mask1.Cols, inspection.paramFile.MaskParam[num].Mask1.Rows);
			}
			if (inspection.paramFile.MaskParam[num].SearchRect2ON && inspection.paramFile.MaskParam[num].Mask2 != null)
			{
				MaskRect[1] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[num].Mask2Pt.X, inspection.paramFile.MaskParam[num].Mask2Pt.Y, inspection.paramFile.MaskParam[num].Mask2.Cols, inspection.paramFile.MaskParam[num].Mask2.Rows);
			}
			else
			{
				MaskRect[1] = new OpenCvSharp.Rect(0, 0, 0, 0);
			}
			SetMaskTracker(MaskRect[0], MaskRect[1]);
		}
		else
		{
			rt_Mask1.Visibility = Visibility.Hidden;
			rt_Mask2.Visibility = Visibility.Hidden;
			grd_Mask.Background = Brushes.White;
		}
	}

	private void Btn_MaskEdit_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = cb_MaskSelect.SelectedIndex;
		if (selectedIndex > 0)
		{
			MaskWindow maskWindow = new MaskWindow();
			maskWindow.SetInspection(selectedIndex - 1, inspection);
			maskWindow.SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
			maskWindow.ShowDialog();
			Mat mat = new Mat();
			inspection.Inspect(ref mat);
			CopyValueToUI();
		}
	}

	private void Btn_TrackerShow_Click(object sender, RoutedEventArgs e)
	{
		if (!TrackerShow)
		{
			bor_SearchRect1.Visibility = Visibility.Visible;
			if (ts_Tracker2ON.IsChecked == true)
			{
				bor_SearchRect2.Visibility = Visibility.Visible;
			}
			TrackerShow = true;
			bor_LocateRect.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_SearchRect2.Visibility = Visibility.Hidden;
			bor_LocateRect.Visibility = Visibility.Hidden;
			TrackerShow = false;
		}
	}

	private void DrawBackGround()
	{
		if (!PreviewMode)
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool IsNumeric(string TextBoxValue)
	{
		try
		{
			Convert.ToDouble(TextBoxValue);
			return true;
		}
		catch (Exception ex)
		{
			string text = "輸入格式錯誤-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckTextBox()
	{
		bool result = true;
		if (!txt_InspRule1Min.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule1Min.Text))
			{
				txt_InspRule1Min.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule1Min.Background = Brushes.White;
			}
		}
		if (!txt_InspRule2Min.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule2Min.Text))
			{
				txt_InspRule2Min.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule2Min.Background = Brushes.White;
			}
		}
		if (!txt_InspRule3Min.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule3Min.Text))
			{
				txt_InspRule3Min.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule3Min.Background = Brushes.White;
			}
		}
		if (!txt_InspRule4Min.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule4Min.Text))
			{
				txt_InspRule4Min.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule4Min.Background = Brushes.White;
			}
		}
		if (!txt_InspRule1Max.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule1Max.Text))
			{
				txt_InspRule1Max.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule1Max.Background = Brushes.White;
			}
		}
		if (!txt_InspRule2Max.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule2Max.Text))
			{
				txt_InspRule2Max.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule2Max.Background = Brushes.White;
			}
		}
		if (!txt_InspRule3Max.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule3Max.Text))
			{
				txt_InspRule3Max.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule3Max.Background = Brushes.White;
			}
		}
		if (!txt_InspRule4Max.Text.Contains(":"))
		{
			if (!IsNumeric(txt_InspRule4Max.Text))
			{
				txt_InspRule4Max.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_InspRule4Max.Background = Brushes.White;
			}
		}
		if (!IsNumeric(txt_BlobLineWidthMin.Text))
		{
			txt_BlobLineWidthMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineWidthMin.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineWidthMax.Text))
		{
			txt_BlobLineWidthMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineWidthMax.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineHeightMin.Text))
		{
			txt_BlobLineHeightMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineHeightMin.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineHeightMax.Text))
		{
			txt_BlobLineHeightMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineHeightMax.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineAreaMin.Text))
		{
			txt_BlobLineAreaMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineAreaMin.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineAreaMax.Text))
		{
			txt_BlobLineAreaMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineAreaMax.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineNum.Text))
		{
			txt_BlobLineNum.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineNum.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineMin.Text))
		{
			txt_BlobLineMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineMin.Background = Brushes.White;
		}
		if (!IsNumeric(txt_BlobLineMax.Text))
		{
			txt_BlobLineMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_BlobLineMax.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule1Min.Text))
		{
			txt_MB_InspRule1Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule1Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule2Min.Text))
		{
			txt_MB_InspRule2Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule2Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule3Min.Text))
		{
			txt_MB_InspRule3Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule3Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule4Min.Text))
		{
			txt_MB_InspRule4Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule4Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule1Max.Text))
		{
			txt_MB_InspRule1Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule1Max.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule2Max.Text))
		{
			txt_MB_InspRule2Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule2Max.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule3Max.Text))
		{
			txt_MB_InspRule3Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule3Max.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MB_InspRule4Max.Text))
		{
			txt_MB_InspRule4Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MB_InspRule4Max.Background = Brushes.White;
		}
		return result;
	}

	private void Chk_UseMaskAsInspArea_Click(object sender, RoutedEventArgs e)
	{
		Btn_ReDraw_Click(this, null);
	}

	private void Cb_Track1AutoSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && rad_Tracker1AutoSize.IsChecked == true)
		{
			SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect);
			rt_ReferenceRect1.Visibility = Visibility.Visible;
			Line line = line_X1;
			Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
			line.Visibility = visibility;
		}
	}

	private void Cb_Track2AutoSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && rad_Tracker2AutoSize.IsChecked == true)
		{
			SetReferenceRect(rt_ReferenceRect2, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect);
			rt_ReferenceRect2.Visibility = Visibility.Visible;
			Line line = line_X2;
			Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
			line.Visibility = visibility;
		}
	}

	private void Cb_Track1AlignWayX_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && rad_Tracker1AlignSelect.IsChecked == true)
		{
			SetReferenceLine(line_X1, line_Y1, cb_Track1AlignWayX.SelectedIndex, cb_Track1AlignWayY.SelectedIndex);
			rt_ReferenceRect1.Visibility = Visibility.Hidden;
			Line line = line_X1;
			Visibility visibility = (line_Y1.Visibility = Visibility.Visible);
			line.Visibility = visibility;
		}
	}

	private void Cb_Track2AlignWayX_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && rad_Tracker2AlignSelect.IsChecked == true)
		{
			SetReferenceLine(line_X2, line_Y2, cb_Track2AlignWayX.SelectedIndex, cb_Track2AlignWayY.SelectedIndex);
			rt_ReferenceRect2.Visibility = Visibility.Hidden;
			Line line = line_X2;
			Visibility visibility = (line_Y2.Visibility = Visibility.Visible);
			line.Visibility = visibility;
		}
	}

	private void Btn_ReferenceShow_Click(object sender, RoutedEventArgs e)
	{
		if (ReferenceShow)
		{
			Line line = line_X1;
			Line line2 = line_X2;
			Line line3 = line_Y1;
			Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
			Visibility visibility3 = (line3.Visibility = visibility);
			Visibility visibility5 = (line2.Visibility = visibility3);
			line.Visibility = visibility5;
			Rectangle rectangle = rt_ReferenceRect1;
			visibility5 = (rt_ReferenceRect2.Visibility = Visibility.Hidden);
			rectangle.Visibility = visibility5;
			Rectangle rectangle2 = rt_Mask1;
			visibility5 = (rt_Mask2.Visibility = Visibility.Hidden);
			rectangle2.Visibility = visibility5;
			ReferenceShow = false;
			return;
		}
		ReferenceShow = true;
		if (rad_Tracker1AlignSelect.IsChecked == true)
		{
			Line line4 = line_X1;
			Visibility visibility5 = (line_Y1.Visibility = Visibility.Visible);
			line4.Visibility = visibility5;
		}
		else
		{
			rt_ReferenceRect1.Visibility = Visibility.Visible;
		}
		if (ts_Tracker2ON.IsChecked == true)
		{
			if (rad_Tracker2AlignSelect.IsChecked == true)
			{
				Line line5 = line_X2;
				Visibility visibility5 = (line_Y2.Visibility = Visibility.Visible);
				line5.Visibility = visibility5;
			}
			else
			{
				rt_ReferenceRect2.Visibility = Visibility.Visible;
			}
		}
		if (cb_MaskSelect.SelectedIndex > 0)
		{
			rt_Mask1.Visibility = Visibility.Visible;
			if (inspection.paramFile.MaskParam[cb_MaskSelect.SelectedIndex - 1].SearchRect2ON)
			{
				rt_Mask2.Visibility = Visibility.Visible;
			}
		}
	}

	private void Rad_Tracker1AlignSelect_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track1AlignWayX_SelectionChanged(this, null);
	}

	private void Rad_Tracker1AutoSize_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track1AutoSize_SelectionChanged(this, null);
	}

	private void Rad_Tracker2AlignSelect_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track2AlignWayX_SelectionChanged(this, null);
	}

	private void Rad_Tracker2AutoSize_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track2AutoSize_SelectionChanged(this, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_FaceA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		btn_FaceA.Background = Brushes.White;
		btn_FaceB.Background = Brushes.White;
		btn_FaceC.Background = Brushes.White;
		btn_FaceD.Background = Brushes.White;
		string uid = obj.Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (!(uid == "3"))
				{
					if (uid == "4")
					{
						btn_FaceD.Background = Brushes.Yellow;
						SelectFace = 4;
					}
				}
				else
				{
					btn_FaceC.Background = Brushes.Yellow;
					SelectFace = 3;
				}
			}
			else
			{
				btn_FaceB.Background = Brushes.Yellow;
				SelectFace = 2;
			}
		}
		else
		{
			btn_FaceA.Background = Brushes.Yellow;
			SelectFace = 1;
		}
	}

	private void Chk_SelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (chk_SelectFace.IsChecked == true)
		{
			grd_SelectFace.IsEnabled = true;
			btn_FaceA.Background = Brushes.Yellow;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			SelectFace = 1;
		}
		else
		{
			grd_SelectFace.IsEnabled = false;
			btn_FaceA.Background = Brushes.White;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			SelectFace = 0;
		}
	}

	private void Nu_ThdLimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD1.LowerValue = (int)nu_Thd1LimitMin.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd1LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD1.UpperValue = (int)nu_Thd1LimitMax.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd4LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD4.LowerValue = (int)nu_Thd4LimitMin.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd4LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD4.UpperValue = (int)nu_Thd4LimitMax.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd3LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD3.LowerValue = (int)nu_Thd3LimitMin.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd3LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD3.UpperValue = (int)nu_Thd3LimitMax.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd2LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD2.UpperValue = (int)nu_Thd2LimitMax.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Nu_Thd2LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_THD2.LowerValue = (int)nu_Thd2LimitMin.Value.Value;
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Exp_TrackerAdj_Collapsed(object sender, RoutedEventArgs e)
	{
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		TrackerAdjShow = false;
	}

	private void Btn_TrackerAdjShow_Click(object sender, RoutedEventArgs e)
	{
		if (!TrackerAdjShow)
		{
			exp_TrackerAdj.Visibility = Visibility.Visible;
			exp_TrackerAdj.IsExpanded = true;
			TrackerAdjShow = true;
		}
		else
		{
			exp_TrackerAdj.Visibility = Visibility.Hidden;
			TrackerAdjShow = false;
		}
	}

	private void Bor_SearchRect1_GotFocus(object sender, RoutedEventArgs e)
	{
		CurrentTracker = 0;
	}

	private void Bor_SearchRect2_GotFocus(object sender, RoutedEventArgs e)
	{
		CurrentTracker = 1;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerMoveUp_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (CurrentTracker == 0)
		{
			int num = (int)Canvas.GetLeft(bor_SearchRect1);
			int num2 = (int)Canvas.GetTop(bor_SearchRect1);
			int num3 = (int)bor_SearchRect1.Width;
			int num4 = (int)bor_SearchRect1.Height;
			string uid = button.Uid;
			if (uid != null)
			{
				switch (uid.Length)
				{
				case 1:
					switch (uid[0])
					{
					case '0':
						num2--;
						break;
					case '1':
						num2++;
						break;
					case '2':
						num--;
						break;
					case '3':
						num++;
						break;
					case '4':
						num2--;
						num4++;
						break;
					case '5':
						num4++;
						break;
					case '6':
						num--;
						num3++;
						break;
					case '7':
						num3++;
						break;
					case '8':
						num2++;
						num4--;
						break;
					case '9':
						num4--;
						break;
					}
					break;
				case 2:
					switch (uid[1])
					{
					case '0':
						if (uid == "10")
						{
							num++;
							num3--;
						}
						break;
					case '1':
						if (uid == "11")
						{
							num3--;
						}
						break;
					case '2':
						if (uid == "12")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num3 = (int)bor_SearchRect2.Width;
							num4 = (int)bor_SearchRect2.Height;
						}
						break;
					case '3':
						if (uid == "13")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num2 = (int)Canvas.GetTop(bor_SearchRect2);
						}
						break;
					case '4':
						if (uid == "14")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num2 = (int)Canvas.GetTop(bor_SearchRect2) + (int)bor_SearchRect2.Height - num4;
						}
						break;
					case '5':
						if (uid == "15")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num = (int)Canvas.GetLeft(bor_SearchRect2);
						}
						break;
					case '6':
						if (uid == "16")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num = (int)Canvas.GetLeft(bor_SearchRect2) + (int)bor_SearchRect2.Width - num3;
						}
						break;
					}
					break;
				}
			}
			bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)num);
			bor_SearchRect1.SetValue(Canvas.TopProperty, (double)num2);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)num3);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)num4);
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetTracker(ref rect, 0);
		}
		else
		{
			if (ts_Tracker2ON.IsChecked == false)
			{
				return;
			}
			int num5 = (int)Canvas.GetLeft(bor_SearchRect2);
			int num6 = (int)Canvas.GetTop(bor_SearchRect2);
			int num7 = (int)bor_SearchRect2.Width;
			int num8 = (int)bor_SearchRect2.Height;
			string uid = button.Uid;
			if (uid != null)
			{
				switch (uid.Length)
				{
				case 1:
					switch (uid[0])
					{
					case '0':
						num6--;
						break;
					case '1':
						num6++;
						break;
					case '2':
						num5--;
						break;
					case '3':
						num5++;
						break;
					case '4':
						num6--;
						num8++;
						break;
					case '5':
						num8++;
						break;
					case '6':
						num5--;
						num7++;
						break;
					case '7':
						num7++;
						break;
					case '8':
						num6++;
						num8--;
						break;
					case '9':
						num8--;
						break;
					}
					break;
				case 2:
					switch (uid[1])
					{
					case '0':
						if (uid == "10")
						{
							num5++;
							num7--;
						}
						break;
					case '1':
						if (uid == "11")
						{
							num7--;
						}
						break;
					case '2':
						if (uid == "12")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num7 = (int)bor_SearchRect1.Width;
							num8 = (int)bor_SearchRect1.Height;
						}
						break;
					case '3':
						if (uid == "13")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num6 = (int)Canvas.GetTop(bor_SearchRect1);
						}
						break;
					case '4':
						if (uid == "14")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num6 = (int)Canvas.GetTop(bor_SearchRect1) + (int)bor_SearchRect1.Height - num8;
						}
						break;
					case '5':
						if (uid == "15")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num5 = (int)Canvas.GetLeft(bor_SearchRect1);
						}
						break;
					case '6':
						if (uid == "16")
						{
							if (ts_Tracker2ON.IsChecked == false)
							{
								return;
							}
							num5 = (int)Canvas.GetLeft(bor_SearchRect1) + (int)bor_SearchRect1.Width - num7;
						}
						break;
					}
					break;
				}
			}
			bor_SearchRect2.SetValue(Canvas.LeftProperty, (double)num5);
			bor_SearchRect2.SetValue(Canvas.TopProperty, (double)num6);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, (double)num7);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, (double)num8);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetTracker(ref rect2, 1);
		}
	}

	private void Txt_ComDigits_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetComDigitsKeyboard(tbox, position);
	}

	private void Txt_InspRule1Min_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetKeyboard(tbox, position);
	}

	private void Txt_BlobLineNum_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT13-BinaryWindow-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
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
		tbox.IsReadOnly = true;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		if (PreviewMode || (User <= User_Permissions.USER_Engineer && inspection.paramFile.BinaryParam[TrackIndex].Locked))
		{
			return;
		}
		try
		{
			if (ColorDropper)
			{
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper = false;
			}
			if (DontSave || DontSave1 || DontSave2)
			{
				Label_ResultError.Visibility = Visibility.Visible;
				if (LanguageType == 1)
				{
					Label_ResultError.Text = "錯誤，檢測區域錯誤，自動儲存取消。";
				}
				else if (LanguageType == 2)
				{
					Label_ResultError.Text = "错误，检测区域错误，自动储存取消。";
				}
				else if (LanguageType == 3)
				{
					Label_ResultError.Text = "検査領域にエラーがあります。自動保存をキャンセルしました。";
				}
				else
				{
					Label_ResultError.Text = "Tracker value error, Can't be Save!";
				}
				return;
			}
			INSP_BINARYTOOL_PARAM iNSP_BINARYTOOL_PARAM = inspection.paramFile.BinaryParam[TrackIndex];
			iNSP_BINARYTOOL_PARAM.ValueChangeFlag = true;
			iNSP_BINARYTOOL_PARAM.TrackerON = ts_TrackerON.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.TrackerName = txt_TrackerName.Text;
			iNSP_BINARYTOOL_PARAM.MaskSelectIndex = cb_MaskSelect.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.UseMaskAsInspArea = chk_UseMaskAsInspArea.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.MaskDE_Start1_Index = (USE_DILATE_ERODE)cb_Mask_DEStart.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MaskDE_Start1_Count = cb_Mask_DEStartCount.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.ColorReferenceIndex = cb_ColorReference.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.EnableSoftCorrection = chk_EnableSoftCorrection.IsChecked.Value;
			if (iNSP_BINARYTOOL_PARAM.ColorReferenceIndex == 0)
			{
				iNSP_BINARYTOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.THD1_ChannelON = true;
				iNSP_BINARYTOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.THD2_ChannelON = exp_THD2.IsExpanded;
				iNSP_BINARYTOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.THD3_ChannelON = exp_THD3.IsExpanded;
				iNSP_BINARYTOOL_PARAM.THD4_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD4.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.THD4_ChannelON = exp_THD4.IsExpanded;
				iNSP_BINARYTOOL_PARAM.THD_ORMode = chk_Thd_ORMode.IsChecked.Value;
				if (iNSP_BINARYTOOL_PARAM.EnableSoftCorrection && inspection.paramFile.SoftCorrectionON && inspection.paramFile.LightMonitorParam.EnableSoftCorrection)
				{
					if (iNSP_BINARYTOOL_PARAM.THD1_ChannelIndex >= COLOR_CHANNEL.EM_RED || iNSP_BINARYTOOL_PARAM.THD1_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int num = (int)rs_THD1.LowerValue + channelShiftMin[0];
						int num2 = (int)rs_THD1.UpperValue + channelShiftMax[0];
						if (num < 0)
						{
							num = 0;
						}
						if (num > 254)
						{
							num = 254;
						}
						if (num2 < 1)
						{
							num2 = 1;
						}
						if (num2 > 255)
						{
							num2 = 255;
						}
						iNSP_BINARYTOOL_PARAM.THD1_Min = num;
						iNSP_BINARYTOOL_PARAM.THD1_Max = num2;
					}
					if (iNSP_BINARYTOOL_PARAM.THD2_ChannelIndex >= COLOR_CHANNEL.EM_RED || iNSP_BINARYTOOL_PARAM.THD2_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int num3 = (int)rs_THD2.LowerValue + channelShiftMin[1];
						int num4 = (int)rs_THD2.UpperValue + channelShiftMax[1];
						if (num3 < 0)
						{
							num3 = 0;
						}
						if (num3 > 254)
						{
							num3 = 254;
						}
						if (num4 < 1)
						{
							num4 = 1;
						}
						if (num4 > 255)
						{
							num4 = 255;
						}
						iNSP_BINARYTOOL_PARAM.THD2_Min = num3;
						iNSP_BINARYTOOL_PARAM.THD2_Max = num4;
					}
					if (iNSP_BINARYTOOL_PARAM.THD3_ChannelIndex >= COLOR_CHANNEL.EM_RED || iNSP_BINARYTOOL_PARAM.THD3_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int num5 = (int)rs_THD3.LowerValue + channelShiftMin[2];
						int num6 = (int)rs_THD3.UpperValue + channelShiftMax[2];
						if (num5 < 0)
						{
							num5 = 0;
						}
						if (num5 > 254)
						{
							num5 = 254;
						}
						if (num6 < 1)
						{
							num6 = 1;
						}
						if (num6 > 255)
						{
							num6 = 255;
						}
						iNSP_BINARYTOOL_PARAM.THD3_Min = num5;
						iNSP_BINARYTOOL_PARAM.THD3_Max = num6;
					}
					if (iNSP_BINARYTOOL_PARAM.THD4_ChannelIndex >= COLOR_CHANNEL.EM_RED || iNSP_BINARYTOOL_PARAM.THD4_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int num7 = (int)rs_THD4.LowerValue + channelShiftMin[3];
						int num8 = (int)rs_THD4.UpperValue + channelShiftMax[3];
						if (num7 < 0)
						{
							num7 = 0;
						}
						if (num7 > 254)
						{
							num7 = 254;
						}
						if (num8 < 1)
						{
							num8 = 1;
						}
						if (num8 > 255)
						{
							num8 = 255;
						}
						iNSP_BINARYTOOL_PARAM.THD4_Min = num7;
						iNSP_BINARYTOOL_PARAM.THD4_Max = num8;
					}
				}
				else
				{
					iNSP_BINARYTOOL_PARAM.THD1_Min = (int)rs_THD1.LowerValue;
					iNSP_BINARYTOOL_PARAM.THD1_Max = (int)rs_THD1.UpperValue;
					iNSP_BINARYTOOL_PARAM.THD2_Min = (int)rs_THD2.LowerValue;
					iNSP_BINARYTOOL_PARAM.THD2_Max = (int)rs_THD2.UpperValue;
					iNSP_BINARYTOOL_PARAM.THD3_Min = (int)rs_THD3.LowerValue;
					iNSP_BINARYTOOL_PARAM.THD3_Max = (int)rs_THD3.UpperValue;
					iNSP_BINARYTOOL_PARAM.THD4_Min = (int)rs_THD4.LowerValue;
					iNSP_BINARYTOOL_PARAM.THD4_Max = (int)rs_THD4.UpperValue;
				}
				iNSP_BINARYTOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start4_Index = (USE_DILATE_ERODE)cb_THD4_DEStart.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End4_Index = (USE_DILATE_ERODE)cb_THD4_DEEnd.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_Start4_Count = cb_THD4_DEStartCount.SelectedIndex;
				iNSP_BINARYTOOL_PARAM.DE_End4_Count = cb_THD4_DEEndCount.SelectedIndex;
			}
			iNSP_BINARYTOOL_PARAM.InspRuleAND = rad_RuleAND.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.InspRule1_Index = (BlobCondition)cb_InspRule1.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.InspRule2_Index = (BlobCondition)cb_InspRule2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.InspRule3_Index = (BlobCondition)cb_InspRule3.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.InspRule4_Index = (BlobCondition)cb_InspRule4.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.InspRule1_Min = ConvertComDigits(txt_InspRule1Min);
			iNSP_BINARYTOOL_PARAM.InspRule2_Min = ConvertComDigits(txt_InspRule2Min);
			iNSP_BINARYTOOL_PARAM.InspRule3_Min = ConvertComDigits(txt_InspRule3Min);
			iNSP_BINARYTOOL_PARAM.InspRule4_Min = ConvertComDigits(txt_InspRule4Min);
			iNSP_BINARYTOOL_PARAM.InspRule1_Max = ConvertComDigits(txt_InspRule1Max);
			iNSP_BINARYTOOL_PARAM.InspRule2_Max = ConvertComDigits(txt_InspRule2Max);
			iNSP_BINARYTOOL_PARAM.InspRule3_Max = ConvertComDigits(txt_InspRule3Max);
			iNSP_BINARYTOOL_PARAM.InspRule4_Max = ConvertComDigits(txt_InspRule4Max);
			if (cb_MB_InspRule1.SelectedIndex == 6)
			{
				iNSP_BINARYTOOL_PARAM.MB_InspRule1_Index = BlobCondition.EM_ALLAREA;
			}
			else
			{
				iNSP_BINARYTOOL_PARAM.MB_InspRule1_Index = (BlobCondition)cb_MB_InspRule1.SelectedIndex;
			}
			iNSP_BINARYTOOL_PARAM.MB_InspRule2_Index = (BlobCondition)cb_MB_InspRule2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MB_InspRule3_Index = (BlobCondition)cb_MB_InspRule3.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MB_InspRule4_Index = (BlobCondition)cb_MB_InspRule4.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MB_InspRule1_Min = Convert.ToDouble(txt_MB_InspRule1Min.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule2_Min = Convert.ToDouble(txt_MB_InspRule2Min.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule3_Min = Convert.ToDouble(txt_MB_InspRule3Min.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule4_Min = Convert.ToDouble(txt_MB_InspRule4Min.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule1_Max = Convert.ToDouble(txt_MB_InspRule1Max.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule2_Max = Convert.ToDouble(txt_MB_InspRule2Max.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule3_Max = Convert.ToDouble(txt_MB_InspRule3Max.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRule4_Max = Convert.ToDouble(txt_MB_InspRule4Max.Text);
			iNSP_BINARYTOOL_PARAM.MB_InspRuleAND = rad_MB_RuleAND.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.MB_BlobSort = cb_MB_BlobSort.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MB_BlobNum = Convert.ToInt32(txt_MB_BlobNum.Text);
			iNSP_BINARYTOOL_PARAM.MB_AllowNGCount = Convert.ToInt32(txt_MB_NGCount.Text);
			iNSP_BINARYTOOL_PARAM.BlobAnalysisSelect = tab_BlobAnalysis.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.BlobLineSort = cb_BlobLineSort.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.BlobLineNum = Convert.ToInt32(txt_BlobLineNum.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineMin = Convert.ToInt32(txt_BlobLineMin.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineMax = Convert.ToInt32(txt_BlobLineMax.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineWidthMin = Convert.ToInt32(txt_BlobLineWidthMin.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineWidthMax = Convert.ToInt32(txt_BlobLineWidthMax.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineHeightMin = Convert.ToInt32(txt_BlobLineHeightMin.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineHeightMax = Convert.ToInt32(txt_BlobLineHeightMax.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineAreaMin = Convert.ToInt32(txt_BlobLineAreaMin.Text);
			iNSP_BINARYTOOL_PARAM.BlobLineAreaMax = Convert.ToInt32(txt_BlobLineAreaMax.Text);
			iNSP_BINARYTOOL_PARAM.LineWidthMinW = Convert.ToInt32(txt_LineWidthMinW.Text);
			iNSP_BINARYTOOL_PARAM.LineWidthMinH = Convert.ToInt32(txt_LineWidthMinH.Text);
			iNSP_BINARYTOOL_PARAM.LineWidthMinArea = Convert.ToInt32(txt_LineWidthMinArea.Text);
			iNSP_BINARYTOOL_PARAM.EnableLineWidthA = cb_LineWidthA.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.EnableLineWidthW = cb_LineWidthW.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.EnableLineWidthH = cb_LineWidthH.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.UsePixelValue = cb_BlobValueUsePixelValue.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.AllowLineWidth = Convert.ToInt32(txt_AllowLineWidth.Text);
			iNSP_BINARYTOOL_PARAM.AllowLineHeight = Convert.ToInt32(txt_AllowLineHeight.Text);
			iNSP_BINARYTOOL_PARAM.AllowLineError = Convert.ToInt32(txt_AllowLineWidthError.Text);
			iNSP_BINARYTOOL_PARAM.AllowErrorW = Convert.ToInt32(txt_AllowErrorW.Text);
			iNSP_BINARYTOOL_PARAM.AllowErrorH = Convert.ToInt32(txt_AllowErrorH.Text);
			iNSP_BINARYTOOL_PARAM.TrackErrorSetNG = chk_TrackErrorSetNG.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.ErrorBin = (FLOW_BIN)cb_BinaryErrorBin.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
			bool flag = false;
			if (rad_Tracker1AutoSize.IsChecked.Value != iNSP_BINARYTOOL_PARAM.Tracker1UseAutoSize)
			{
				flag = true;
			}
			if (iNSP_BINARYTOOL_PARAM.Tracker1AligmentXIndex != cb_Track1AlignWayX.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_BINARYTOOL_PARAM.Tracker2AligmentXIndex != cb_Track2AlignWayX.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_BINARYTOOL_PARAM.Tracker1AligmentYIndex != cb_Track1AlignWayY.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_BINARYTOOL_PARAM.Tracker2AligmentYIndex != cb_Track2AlignWayY.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex != cb_Track1AutoSize.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_BINARYTOOL_PARAM.AutoSize2_SelectIndex != cb_Track2AutoSize.SelectedIndex)
			{
				flag = true;
			}
			iNSP_BINARYTOOL_PARAM.Tracker1UseAutoSize = rad_Tracker1AutoSize.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.Tracker1AligmentXIndex = cb_Track1AlignWayX.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.Tracker2AligmentXIndex = cb_Track2AlignWayX.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.Tracker1AligmentYIndex = cb_Track1AlignWayY.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.Tracker2AligmentYIndex = cb_Track2AlignWayY.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex = cb_Track1AutoSize.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.AutoSize2_SelectIndex = cb_Track2AutoSize.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (!GetTracker(ref rect, 0))
			{
				return;
			}
			bool flag2 = false;
			if (iNSP_BINARYTOOL_PARAM.SearchRect2ON != ts_Tracker2ON.IsChecked)
			{
				flag2 = true;
			}
			if (iNSP_BINARYTOOL_PARAM.Tracker2UseAutoSize != rad_Tracker2AutoSize.IsChecked.Value)
			{
				flag2 = true;
			}
			iNSP_BINARYTOOL_PARAM.SearchRect2ON = ts_Tracker2ON.IsChecked.Value;
			if (iNSP_BINARYTOOL_PARAM.SearchRect2ON && !GetTracker(ref rect2, 1))
			{
				return;
			}
			iNSP_BINARYTOOL_PARAM.Tracker2UseAutoSize = rad_Tracker2AutoSize.IsChecked.Value;
			if ((LastSearchRect1 != rect) | flag)
			{
				if (rad_Tracker1AutoSize.IsChecked.Value)
				{
					iNSP_BINARYTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex].L;
					iNSP_BINARYTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex].T;
					iNSP_BINARYTOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex].Width - iNSP_BINARYTOOL_PARAM.Track1_L - rect.Width;
					iNSP_BINARYTOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex].Height - iNSP_BINARYTOOL_PARAM.Track1_T - rect.Height;
				}
				else
				{
					iNSP_BINARYTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[iNSP_BINARYTOOL_PARAM.Tracker1AligmentXIndex].Value;
					iNSP_BINARYTOOL_PARAM.Track1_R = iNSP_BINARYTOOL_PARAM.Track1_L + rect.Width;
					iNSP_BINARYTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[iNSP_BINARYTOOL_PARAM.Tracker1AligmentYIndex].Value;
					iNSP_BINARYTOOL_PARAM.Track1_B = iNSP_BINARYTOOL_PARAM.Track1_T + rect.Height;
				}
			}
			if ((LastSearchRect2 != rect2) | flag | flag2)
			{
				if (rad_Tracker2AutoSize.IsChecked.Value)
				{
					iNSP_BINARYTOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize2_SelectIndex].L;
					iNSP_BINARYTOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize2_SelectIndex].T;
					iNSP_BINARYTOOL_PARAM.Track2_R = inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize2_SelectIndex].Width - iNSP_BINARYTOOL_PARAM.Track2_L - rect2.Width;
					iNSP_BINARYTOOL_PARAM.Track2_B = inspection.paramFile.AdvLocateParam[iNSP_BINARYTOOL_PARAM.AutoSize2_SelectIndex].Height - iNSP_BINARYTOOL_PARAM.Track2_T - rect2.Height;
				}
				else
				{
					iNSP_BINARYTOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.VSLocateParam[iNSP_BINARYTOOL_PARAM.Tracker2AligmentXIndex].Value;
					iNSP_BINARYTOOL_PARAM.Track2_R = iNSP_BINARYTOOL_PARAM.Track2_L + rect2.Width;
					iNSP_BINARYTOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.HSLocateParam[iNSP_BINARYTOOL_PARAM.Tracker2AligmentYIndex].Value;
					iNSP_BINARYTOOL_PARAM.Track2_B = iNSP_BINARYTOOL_PARAM.Track2_T + rect2.Height;
				}
			}
			iNSP_BINARYTOOL_PARAM.T1FillletCount0 = cb_T1Filllet0.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T1FillletCount1 = cb_T1Filllet1.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T1FillletCount2 = cb_T1Filllet2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T1FillletCount3 = cb_T1Filllet3.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T2FillletCount0 = cb_T2Filllet0.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T2FillletCount1 = cb_T2Filllet1.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T2FillletCount2 = cb_T2Filllet2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.T2FillletCount3 = cb_T2Filllet3.SelectedIndex;
			if (iNSP_BINARYTOOL_PARAM.T1FillletCount0 > 0 || iNSP_BINARYTOOL_PARAM.T1FillletCount1 > 0 || iNSP_BINARYTOOL_PARAM.T1FillletCount2 > 0 || iNSP_BINARYTOOL_PARAM.T1FillletCount3 > 0 || iNSP_BINARYTOOL_PARAM.T2FillletCount0 > 0 || iNSP_BINARYTOOL_PARAM.T2FillletCount1 > 0 || iNSP_BINARYTOOL_PARAM.T2FillletCount2 > 0 || iNSP_BINARYTOOL_PARAM.T2FillletCount3 > 0)
			{
				btn_Fillet.Background = Brushes.Gold;
			}
			else
			{
				btn_Fillet.Background = Brushes.White;
			}
			iNSP_BINARYTOOL_PARAM.EnableSelectFace = chk_SelectFace.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.FaceIndex = SelectFace;
			iNSP_BINARYTOOL_PARAM.UseFontMask = chk_UseFontMask.IsChecked.Value;
			iNSP_BINARYTOOL_PARAM.UseMatchTextMask1 = cb_MatchMaskSelect1.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.UseMatchTextMask2 = cb_MatchMaskSelect2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.UseMatchTextMask3 = cb_MatchMaskSelect3.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MatchTextDE_Start1_Index = (USE_DILATE_ERODE)cb_MatchText_DEStart1.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MatchTextDE_Start2_Index = (USE_DILATE_ERODE)cb_MatchText_DEStart2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MatchTextDE_Start3_Index = (USE_DILATE_ERODE)cb_MatchText_DEStart3.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MatchTextDE_Start1_Count = cb_MatchText_DEStartCount1.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MatchTextDE_Start2_Count = cb_MatchText_DEStartCount2.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.MatchTextDE_Start3_Count = cb_MatchText_DEStartCount3.SelectedIndex;
			if (TrackIndex == 0)
			{
				inspection.paramFile.EnableJudgeWithAdvMeasure = chk_EnableJudgeWithAdvMeasure.IsChecked.Value;
				int num9 = cb_JudgeWithAdvMeasureA.SelectedIndex - 1;
				int num10 = cb_JudgeWithAdvMeasureB.SelectedIndex - 1;
				if (num9 >= 0 && num9 < 40)
				{
					if (inspection.paramFile.AdvLocateMParam[num9].TrackerON)
					{
						inspection.paramFile.JudgeWithAdvMeasureIndexA = num9 + 1;
					}
					else
					{
						inspection.paramFile.JudgeWithAdvMeasureIndexA = 0;
					}
				}
				else
				{
					inspection.paramFile.JudgeWithAdvMeasureIndexA = 0;
				}
				if (num10 >= 0 && num10 < 40)
				{
					if (inspection.paramFile.AdvLocateMParam[num10].TrackerON)
					{
						inspection.paramFile.JudgeWithAdvMeasureIndexB = num10 + 1;
					}
					else
					{
						inspection.paramFile.JudgeWithAdvMeasureIndexB = 0;
					}
				}
				else
				{
					inspection.paramFile.JudgeWithAdvMeasureIndexB = 0;
				}
			}
			if (!iNSP_BINARYTOOL_PARAM.Equals(inspection.paramFile_Backup.BinaryParam[TrackIndex]))
			{
				iNSP_BINARYTOOL_PARAM.ValueChangeFlag = true;
			}
			else
			{
				iNSP_BINARYTOOL_PARAM.ValueChangeFlag = false;
			}
			inspection.paramFile.BinaryParam[TrackIndex] = iNSP_BINARYTOOL_PARAM;
			UpdateUI = false;
			int selectedIndex = cb_TrackerSelect.SelectedIndex;
			cb_TrackerSelect.Items.Clear();
			for (int i = 0; i < 30; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.BinaryParam[i].TrackerName);
			}
			cb_TrackerSelect.SelectedIndex = selectedIndex;
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "BW06-BinaryWindow-WriteUIValueToParam()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string CheckImg(string ext)
	{
		string text = ext.ToLower();
		if (text == ".gif" || text == ".jpg" || text == ".png" || text == ".bmp" || text == ".tif")
		{
			return "1";
		}
		return "0";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void MainW_Drop(object sender, DragEventArgs e)
	{
		string[] array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
		if (array.Length < 1)
		{
			return;
		}
		string text = array[0];
		if (string.IsNullOrEmpty(text) || !File.Exists(text))
		{
			return;
		}
		string extension = System.IO.Path.GetExtension(text);
		if (CheckImg(extension).Equals("0"))
		{
			return;
		}
		try
		{
			Mat mat = CVBuffer.ImRead(text);
			inspection.SetBuffers(mat);
			inspection.Inspect(ref mat);
			if (!inspection.LocaetResult || !inspection.AdvlocateResult)
			{
				cnv_Image.Background = Brushes.LightGray;
				THD_Preview = true;
				DontSave = true;
				btn_Test.IsEnabled = false;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				FaceIndex = inspection.SelectFaceResult;
				SetFaceIndex(FaceIndex);
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
			btn_ImageNum.Content = string.Format("{0} / {1}", 1, 1);
			ImageListSource = 0;
		}
		catch
		{
			MessageBox.Show("不是有效的圖檔格式", LanguageManager.Instance["Error"]);
		}
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_BlobAnalysis_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !(e.Source is TabControl) || !(e.AddedItems[0] is TabItem tabItem))
		{
			return;
		}
		if (tabItem.Name == "tb_MaxBlob")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("檢測特徵方式將更換為檢測區塊！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("检测特征方式将更换为检测区块！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("検査特徴方式が検査ブロックに置き換えられます！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Inspect Functio will change to Inspect Blob！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
		}
		else if (tabItem.Name == "tb_BlobLine")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("檢測特徵方式將更換為檢測線條！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("检测特征方式将更换为检测线条！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("検査特徴方式は、検査線で置き換えられます。", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Inspect Functio will change to Inspect Blob Line！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
		}
		else if (tabItem.Name == "tb_MultiBlob")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("檢測特徵方式將更換為檢測多區塊！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("检测特征方式将更换为检测多区块！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("検査特徴方式は、検査多区画に置き換えられます！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Inspect Functio will change to Inspect Multi-Blobs！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
		}
		else if (tabItem.Name == "tb_LineWidth")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("檢測特徵方式將更換為檢測線寬！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("检测特征方式将更换为检测线宽！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("検査特徴方式は、検査線幅に置き換えられます。", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Inspect Functio will change to Inspect Line Width！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
		}
	}

	private void Btn_Chart_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (InspRule[num].SelectedIndex > 0 && (InspRule[num].SelectedIndex < 6 || InspRule[num].SelectedIndex > 10))
		{
			BlobStatisticsWindow blobStatisticsWindow = new BlobStatisticsWindow();
			blobStatisticsWindow.LoadLanguage(LanguageType);
			blobStatisticsWindow.SetImageSource(ImageListSource, filelist, fileIndex, RecentBufferIndex, OKBufferIndex, NGBufferIndex);
			if (ts_Tracker2ON.IsChecked == true)
			{
				blobStatisticsWindow.SetInspection(TrackIndex, 0, inspection, InspRule[num].SelectedIndex - 1, 2, Convert.ToDouble(ValueMinTxt[num].Text), Convert.ToDouble(ValueMaxTxt[num].Text));
			}
			else
			{
				blobStatisticsWindow.SetInspection(TrackIndex, 0, inspection, InspRule[num].SelectedIndex - 1, 1, Convert.ToDouble(ValueMinTxt[num].Text), Convert.ToDouble(ValueMaxTxt[num].Text));
			}
			blobStatisticsWindow.ShowDialog();
			ValueMinTxt[num].Text = blobStatisticsWindow.ValueMin.ToString();
			ValueMaxTxt[num].Text = blobStatisticsWindow.ValueMax.ToString();
			if (blobStatisticsWindow.LocateRect.Width > 0 && blobStatisticsWindow.LocateRect.Height > 0 && !blobStatisticsWindow.LocatBuf.Empty())
			{
				SetBuffer(blobStatisticsWindow.LocatBuf, blobStatisticsWindow.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
	}

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
		}
	}

	private void Cnv_Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (cb_ColorReference.SelectedIndex > 0 && !btn_ReferenceSave.IsEnabled)
		{
			return;
		}
		if (ColorDropper)
		{
			for (int i = 0; i < 4; i++)
			{
				if (THD_Expender[i].IsExpanded || i == 0)
				{
					int num = CurrentColor[THD_Channel[i].SelectedIndex];
					int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
					int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
					THD_Sliders[i].LowerValue = num2;
					THD_Sliders[i].UpperValue = num3;
				}
			}
			DoThdImage();
		}
		if (sfr.ScaleX != 1.0)
		{
			System.Windows.Point position = e.GetPosition(cnv_Image);
			sfr.CenterX = position.X;
			sfr.CenterY = position.Y;
		}
	}

	private void Btn_ChannelPreview1_MouseEnter(object sender, MouseEventArgs e)
	{
		if (!PreviewMode && !ShowBinaryCompare)
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			new Mat();
			int num2 = THD_Channel[num].SelectedIndex;
			if (num2 >= 16)
			{
				num2 += 5;
			}
			Mat image = inspection.buffers.GetChannelMat(num2).Clone();
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(image);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Btn_ChannelPreview1_MouseLeave(object sender, MouseEventArgs e)
	{
		if (!PreviewMode && !ShowBinaryCompare)
		{
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Btn_ChannelBinaryPreview1_MouseEnter(object sender, MouseEventArgs e)
	{
		if (!PreviewMode && !ShowBinaryCompare)
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			Mat mat = new Mat();
			int num2 = THD_Channel[num].SelectedIndex;
			if (num2 >= 16)
			{
				num2 += 5;
			}
			Mat mat2 = inspection.buffers.GetChannelMat(num2).Clone();
			Cv2.InRange(mat2, THD_Sliders[num].LowerValue, THD_Sliders[num].UpperValue, mat);
			DoMorph(ref mat2, THD_DEStart[num].SelectedIndex, THD_DEStartCount[num].SelectedIndex);
			DoMorph(ref mat2, THD_DEEnd[num].SelectedIndex, THD_DEEndCount[num].SelectedIndex);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Btn_HistogramPreview1_MouseEnter(object sender, MouseEventArgs e)
	{
		if (!PreviewMode)
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			new Mat();
			int num2 = THD_Channel[num].SelectedIndex;
			if (num2 >= 16)
			{
				num2 += 5;
			}
			Mat source = inspection.buffers.GetChannelMat(num2).Clone();
			Mat histogram = GetHistogram(num, source);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(histogram);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Histogram.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
			grd_Histogram.Visibility = Visibility.Visible;
		}
	}

	public Mat GetHistogram(int id, Mat source)
	{
		Mat[] images = new Mat[1] { source };
		Mat mat = new Mat();
		int[] channels = new int[1];
		int[] histSize = new int[1] { 256 };
		Rangef[] ranges = new Rangef[1]
		{
			new Rangef(0f, 256f)
		};
		Mat mat2 = new Mat();
		Cv2.CalcHist(images, channels, mat2, mat, 1, histSize, ranges);
		Mat mat3 = new Mat(160, 256, MatType.CV_8UC3, Scalar.All(255.0));
		for (int i = 0; i < 256; i++)
		{
			int pt2Y = (int)(mat.Get<float>(i) / 3000f * (float)mat3.Rows);
			Cv2.Line(mat3, i, 0, i, pt2Y, Scalar.Blue);
		}
		Cv2.Line(mat3, (int)THD_Sliders[id].LowerValue, 0, (int)THD_Sliders[id].LowerValue, 360, Scalar.Fuchsia);
		Cv2.Line(mat3, (int)THD_Sliders[id].UpperValue, 0, (int)THD_Sliders[id].UpperValue, 360, Scalar.Red);
		Cv2.Flip(mat3, mat3, FlipMode.X);
		return mat3;
	}

	private void Btn_HistogramPreview1_MouseLeave(object sender, MouseEventArgs e)
	{
		grd_Histogram.Visibility = Visibility.Hidden;
	}

	private void Exp_TrackerFilllet_Collapsed(object sender, RoutedEventArgs e)
	{
		exp_TrackerFilllet.Visibility = Visibility.Hidden;
		TrackerFillletShow = false;
		bor_SearchRect1.Visibility = Visibility.Visible;
		if (ts_Tracker2ON.IsChecked == true)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
		}
		DoThdImage();
	}

	private void Btn_Fillet_Click(object sender, RoutedEventArgs e)
	{
		if (!TrackerFillletShow)
		{
			exp_TrackerFilllet.Visibility = Visibility.Visible;
			exp_TrackerFilllet.IsExpanded = true;
			TrackerFillletShow = true;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_SearchRect2.Visibility = Visibility.Hidden;
			DoThdImage(DrawAll: true);
			return;
		}
		exp_TrackerFilllet.Visibility = Visibility.Hidden;
		TrackerFillletShow = false;
		bor_SearchRect1.Visibility = Visibility.Visible;
		if (ts_Tracker2ON.IsChecked == true)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
		}
		DoThdImage();
	}

	private void Cb_T1Filllet0_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			DoThdImage(DrawAll: true);
		}
	}

	private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (inited)
		{
			DoThdImage(DrawAll: true);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RunShowBinaryCompare()
	{
		if (!ShowBinaryCompare)
		{
			return;
		}
		try
		{
			Mat[] array = new Mat[4];
			Mat[] array2 = new Mat[4];
			array[0] = inspection.buffers.Src;
			for (int i = 0; i < 4; i++)
			{
				array2[i] = new Mat();
				if (i > 0)
				{
					if (inspection.TempImage[i - 1].Width <= 0 || inspection.TempImage[i - 1].Height <= 0)
					{
						continue;
					}
					array[i] = inspection.TempImage[i - 1].Clone();
				}
				inspection.SetBuffers(array[i]);
				if (inspection.Inspect(ref array[i]).bin != FLOW_BIN.EM_FLOW_RETESET)
				{
					Mat finalBinary = inspection.binaryTool[TrackIndex].GetFinalBinary(0);
					OpenCvSharp.Rect rect = default(OpenCvSharp.Rect);
					rect = inspection.binaryTool[TrackIndex].GetInspectRect(0);
					array2[i] = new Mat(inspection.buffers.LocatedSrc, rect).Clone();
					Mat mat = new Mat();
					Mat[] mv = new Mat[3];
					Cv2.Split(array2[i], out mv);
					mv[0].SetTo(255.0);
					mv[1].SetTo(0.0);
					mv[2].SetTo(0.0);
					Cv2.Merge(mv, mat);
					if (finalBinary.Width != array2[i].Width || finalBinary.Height != array2[i].Height)
					{
						finalBinary.Resize(new OpenCvSharp.Size(array2[i].Width, array2[i].Height));
					}
					Mat mat2 = array2[i].Clone();
					mat.CopyTo(mat2, finalBinary);
					Mat mat3 = new Mat();
					Cv2.AddWeighted(array2[i], 0.65, mat2, 0.35, 0.0, mat3);
					array2[i] = mat3.Clone();
				}
			}
			int width = array2[0].Width;
			int height = array2[0].Height;
			Mat mat4 = new Mat(height * 2, width * 2, array2[0].Type());
			mat4.SetTo(0.0);
			for (int j = 0; j < 4; j++)
			{
				if (array2[j].Width > 0 && array2[j].Height > 0)
				{
					OpenCvSharp.Rect roi = new OpenCvSharp.Rect(width * (j / 2), height * (j % 2), width, height);
					array2[j] = array2[j].Resize(new OpenCvSharp.Size(array2[0].Width, array2[0].Height));
					Mat m = new Mat(mat4, roi);
					array2[j].CopyTo(m);
					if (j > 0)
					{
						mat4.PutText("#" + j, new OpenCvSharp.Point(width * (j / 2), height * (j % 2)), HersheyFonts.HersheyComplex, 0.0, Scalar.White);
					}
				}
			}
			ImageSource imageSource = mat4.ToBitmapSource();
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-BinaryWindow-RunShowBinaryCompare()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImgT1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (inspection.TempImage[num].Empty())
		{
			return;
		}
		SwitchToTempImage = true;
		Mat mat = inspection.TempImage[num].Clone();
		inspection.SetBuffers(mat);
		inspection.Inspect(ref mat);
		if (!inspection.LocaetResult || !inspection.AdvlocateResult)
		{
			cnv_Image.Background = Brushes.LightGray;
			THD_Preview = true;
			DontSave = true;
			btn_Test.IsEnabled = false;
			if (!inspection.LocaetResult)
			{
				SetResult(2, LanguageManager.Instance["LocateFail"]);
			}
			else if (!inspection.AdvlocateResult)
			{
				SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
			}
		}
		else
		{
			FaceIndex = inspection.SelectFaceResult;
			SetFaceIndex(FaceIndex);
			DontSave = false;
			btn_Test.IsEnabled = true;
			SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
			Btn_Test_Click(this, null);
		}
	}

	private void Btn_MaskPreView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (PreviewMode)
		{
			return;
		}
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
		if (!GetTracker(ref rect, 0) || (ts_Tracker2ON.IsChecked == true && !GetTracker(ref rect2, 1)) || cb_MaskSelect.SelectedIndex <= 0)
		{
			return;
		}
		int num = cb_MaskSelect.SelectedIndex - 1;
		Mat mat = inspection.buffers.LocateGray.Clone();
		mat.SetTo(0.0);
		bool flag = false;
		if (MaskRect[0].Width > 0 && MaskRect[0].Height > 0 && inspection.paramFile.MaskParam[num].Mask1 != null)
		{
			Mat m = new Mat(mat, MaskRect[0]);
			inspection.paramFile.MaskParam[num].Mask1.Clone().CopyTo(m);
			flag = true;
		}
		if (MaskRect[1].Width > 0 && MaskRect[1].Height > 0 && inspection.paramFile.MaskParam[num].Mask2 != null)
		{
			Mat m2 = new Mat(mat, MaskRect[1]);
			inspection.paramFile.MaskParam[num].Mask2.Clone().CopyTo(m2);
			flag = true;
		}
		if (flag)
		{
			if (chk_UseMaskAsInspArea.IsChecked == false)
			{
				DoMorph(ref mat, cb_Mask_DEStart.SelectedIndex, cb_Mask_DEStartCount.SelectedIndex);
			}
			else
			{
				DoMorph(ref mat, cb_Mask_DEStart.SelectedIndex, cb_Mask_DEStartCount.SelectedIndex);
				Cv2.BitwiseNot(mat, mat);
			}
			Mat mat2 = new Mat(ShowBitmap.Height, ShowBitmap.Width, MatType.CV_8UC1);
			mat2.SetTo(0.0);
			mat2.Rectangle(rect, Scalar.White, -1);
			if (ts_Tracker2ON.IsChecked == true)
			{
				mat2.Rectangle(rect2, Scalar.White, -1);
			}
			Cv2.BitwiseAnd(mat2, mat, mat);
			Mat mat3 = new Mat();
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(0.0);
			ThdShow[1].SetTo(255.0);
			ThdShow[2].SetTo(255.0);
			Cv2.Merge(ThdShow, mat3);
			mat3.CopyTo(ShowBitmap, mat);
		}
		ImageSource imageSource = ShowBitmap.ToBitmapSource();
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	private void Btn_MaskPreView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		ImageSource imageSource = ShowBitmap.ToBitmapSource();
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	private void Bor_SearchRect1_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueSetReference(INSP_BINARYTOOL_PARAM param, OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2)
	{
		try
		{
			Rectangle rectangle = rt_ReferenceRect1;
			Visibility visibility = (rt_ReferenceRect2.Visibility = Visibility.Hidden);
			rectangle.Visibility = visibility;
			if (param.Tracker1UseAutoSize)
			{
				if (inspection.paramFile.AdvLocateParam[param.AutoSize1_SelectIndex].TrackerON)
				{
					rad_Tracker1AutoSize.IsChecked = true;
					rect1.X = inspection.paramFile.AdvLocateParam[param.AutoSize1_SelectIndex].L + param.Track1_L;
					rect1.Y = inspection.paramFile.AdvLocateParam[param.AutoSize1_SelectIndex].T + param.Track1_T;
					rect1.Width = inspection.paramFile.AdvLocateParam[param.AutoSize1_SelectIndex].Width - (param.Track1_L + param.Track1_R);
					rect1.Height = inspection.paramFile.AdvLocateParam[param.AutoSize1_SelectIndex].Height - (param.Track1_T + param.Track1_B);
					SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[param.AutoSize1_SelectIndex].InsideRect);
					Line line = line_X1;
					visibility = (line_Y1.Visibility = Visibility.Hidden);
					line.Visibility = visibility;
				}
				else
				{
					rad_Tracker1AutoSize.IsChecked = true;
					rect1.X = inspection.paramFile.AdvLocateParam[0].L + param.Track1_L;
					rect1.Y = inspection.paramFile.AdvLocateParam[0].T + param.Track1_T;
					rect1.Width = inspection.paramFile.AdvLocateParam[0].Width - (param.Track1_L + param.Track1_R);
					rect1.Height = inspection.paramFile.AdvLocateParam[0].Height - (param.Track1_T + param.Track1_B);
					cb_Track1AutoSize.SelectedIndex = (param.AutoSize1_SelectIndex = 0);
					SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[0].InsideRect);
					Line line2 = line_X1;
					visibility = (line_Y1.Visibility = Visibility.Hidden);
					line2.Visibility = visibility;
				}
			}
			else if (inspection.paramFile.VSLocateParam[param.Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[param.Tracker1AligmentYIndex].TrackerON)
			{
				rad_Tracker1AlignSelect.IsChecked = true;
				int width = param.Track1_R - param.Track1_L;
				int height = param.Track1_B - param.Track1_T;
				rect1.X = inspection.paramFile.VSLocateParam[param.Tracker1AligmentXIndex].Value + param.Track1_L;
				rect1.Y = inspection.paramFile.HSLocateParam[param.Tracker1AligmentYIndex].Value + param.Track1_T;
				rect1.Width = width;
				rect1.Height = height;
				SetReferenceLine(line_X1, line_Y1, param.Tracker1AligmentXIndex, param.Tracker1AligmentYIndex);
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
			else
			{
				rad_Tracker1AlignSelect.IsChecked = true;
				int width2 = param.Track1_R - param.Track1_L;
				int height2 = param.Track1_B - param.Track1_T;
				rect1.X = inspection.paramFile.VSLocateParam[0].Value + param.Track1_L;
				rect1.Y = inspection.paramFile.HSLocateParam[0].Value + param.Track1_T;
				rect1.Width = width2;
				rect1.Height = height2;
				cb_Track1AlignWayX.SelectedIndex = 0;
				cb_Track1AlignWayY.SelectedIndex = 0;
				SetReferenceLine(line_X1, line_Y1, 0, 0);
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
			if (param.Tracker2UseAutoSize)
			{
				if (inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].TrackerON)
				{
					rad_Tracker2AutoSize.IsChecked = true;
					rect2.X = inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].L + param.Track2_L;
					rect2.Y = inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].T + param.Track2_T;
					rect2.Width = inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].Width - (param.Track2_L + param.Track2_R);
					rect2.Height = inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].Height - (param.Track2_T + param.Track2_B);
					if (param.SearchRect2ON)
					{
						SetReferenceRect(rt_ReferenceRect2, inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].InsideRect);
					}
					Line line3 = line_X2;
					visibility = (line_Y2.Visibility = Visibility.Hidden);
					line3.Visibility = visibility;
				}
				else
				{
					rad_Tracker2AutoSize.IsChecked = true;
					rect2.X = inspection.paramFile.AdvLocateParam[0].L + param.Track2_L;
					rect2.Y = inspection.paramFile.AdvLocateParam[0].T + param.Track2_T;
					rect2.Width = inspection.paramFile.AdvLocateParam[0].Width - (param.Track2_L + param.Track2_R);
					rect2.Height = inspection.paramFile.AdvLocateParam[0].Height - (param.Track2_T + param.Track2_B);
					cb_Track2AutoSize.SelectedIndex = (param.AutoSize2_SelectIndex = 0);
					if (param.SearchRect2ON)
					{
						SetReferenceRect(rt_ReferenceRect2, inspection.paramFile.AdvLocateParam[0].InsideRect);
					}
					Line line4 = line_X2;
					visibility = (line_Y2.Visibility = Visibility.Hidden);
					line4.Visibility = visibility;
				}
			}
			else if (inspection.paramFile.VSLocateParam[param.Tracker2AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[param.Tracker2AligmentYIndex].TrackerON)
			{
				rad_Tracker2AlignSelect.IsChecked = true;
				int width3 = param.Track2_R - param.Track2_L;
				int height3 = param.Track2_B - param.Track2_T;
				rect2.X = inspection.paramFile.VSLocateParam[param.Tracker2AligmentXIndex].Value + param.Track2_L;
				rect2.Y = inspection.paramFile.HSLocateParam[param.Tracker2AligmentYIndex].Value + param.Track2_T;
				rect2.Width = width3;
				rect2.Height = height3;
				if (param.SearchRect2ON)
				{
					SetReferenceLine(line_X2, line_Y2, param.Tracker2AligmentXIndex, param.Tracker2AligmentYIndex);
				}
				rt_ReferenceRect2.Visibility = Visibility.Hidden;
			}
			else
			{
				rad_Tracker2AlignSelect.IsChecked = true;
				int width4 = param.Track2_R - param.Track2_L;
				int height4 = param.Track2_B - param.Track2_T;
				rect2.X = inspection.paramFile.VSLocateParam[0].Value + param.Track2_L;
				rect2.Y = inspection.paramFile.HSLocateParam[0].Value + param.Track2_T;
				rect2.Width = width4;
				rect2.Height = height4;
				cb_Track2AlignWayX.SelectedIndex = 0;
				cb_Track2AlignWayY.SelectedIndex = 0;
				if (param.SearchRect2ON)
				{
					SetReferenceLine(line_X2, line_Y2, 0, 0);
				}
				rt_ReferenceRect2.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex)
		{
			string text = "BW07-BinaryWindow-CopyValueToUI()-05-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		if (param.T1FillletCount0 > 0 || param.T1FillletCount1 > 0 || param.T1FillletCount2 > 0 || param.T1FillletCount3 > 0 || param.T2FillletCount0 > 0 || param.T2FillletCount1 > 0 || param.T2FillletCount2 > 0 || param.T2FillletCount3 > 0)
		{
			btn_Fillet.Background = Brushes.Gold;
		}
		else
		{
			btn_Fillet.Background = Brushes.White;
		}
		cb_T1Filllet0.SelectedIndex = param.T1FillletCount0;
		cb_T1Filllet1.SelectedIndex = param.T1FillletCount1;
		cb_T1Filllet2.SelectedIndex = param.T1FillletCount2;
		cb_T1Filllet3.SelectedIndex = param.T1FillletCount3;
		cb_T2Filllet0.SelectedIndex = param.T2FillletCount0;
		cb_T2Filllet1.SelectedIndex = param.T2FillletCount1;
		cb_T2Filllet2.SelectedIndex = param.T2FillletCount2;
		cb_T2Filllet3.SelectedIndex = param.T2FillletCount3;
		if (!param.SearchRect2ON)
		{
			grd_Tracker2Filllet.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Tracker2Filllet.Visibility = Visibility.Visible;
		}
		try
		{
			if (SetTracker(rect1, 0))
			{
				GetTracker(ref rect1, 0);
				LastSearchRect1 = rect1;
			}
			ts_Tracker2ON.IsChecked = param.SearchRect2ON;
			if (SetTracker(rect2, 1))
			{
				GetTracker(ref rect2, 1);
				LastSearchRect2 = rect2;
			}
			if (param.SearchRect2ON)
			{
				grd_Track2Setup.Visibility = Visibility.Visible;
				bor_SearchRect2.Visibility = Visibility.Visible;
			}
			else
			{
				txt_Tracker2Pos.Visibility = Visibility.Hidden;
				grd_Track2Setup.Visibility = Visibility.Hidden;
				bor_SearchRect2.Visibility = Visibility.Hidden;
			}
			if (!param.TrackerON)
			{
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_SearchRect2.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex2)
		{
			string text2 = "BW07-BinaryWindow-CopyValueToUI()-06-";
			text2 += ex2.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return;
		}
		try
		{
			cb_MaskSelect.SelectedIndex = param.MaskSelectIndex;
			chk_UseMaskAsInspArea.IsChecked = param.UseMaskAsInspArea;
			cb_Mask_DEStart.SelectedIndex = (int)param.MaskDE_Start1_Index;
			cb_Mask_DEStartCount.SelectedIndex = param.MaskDE_Start1_Count;
			if (param.MaskSelectIndex > 0)
			{
				if (!inspection.paramFile.MaskToolON || !inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].TrackerON)
				{
					SetResult(2, LanguageManager.Instance["MaskLinkError"]);
					rt_Mask1.Visibility = Visibility.Hidden;
					rt_Mask2.Visibility = Visibility.Hidden;
					cb_MaskSelect.SelectedIndex = 0;
					return;
				}
				rt_Mask1.Visibility = Visibility.Visible;
				if (inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].SearchRect2ON)
				{
					rt_Mask2.Visibility = Visibility.Visible;
				}
				if (inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask1 != null)
				{
					MaskRect[0] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask1Pt.X, inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask1Pt.Y, inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask1.Cols, inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask1.Rows);
				}
				else
				{
					MaskRect[0] = new OpenCvSharp.Rect(0, 0, 0, 0);
				}
				if (inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].SearchRect2ON && inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask2 != null)
				{
					MaskRect[1] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask2Pt.X, inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask2Pt.Y, inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask2.Cols, inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].Mask2.Rows);
				}
				else
				{
					MaskRect[1] = new OpenCvSharp.Rect(0, 0, 0, 0);
				}
				SetMaskTracker(MaskRect[0], MaskRect[1]);
			}
			else
			{
				rt_Mask1.Visibility = Visibility.Hidden;
				rt_Mask2.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex3)
		{
			string text3 = "BW07-BinaryWindow-CopyValueToUI()-02-";
			text3 += ex3.ToString();
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		UpdateUI = false;
		DontSave = false;
		DontSave1 = (DontSave2 = false);
		btn_ResetA.Visibility = Visibility.Hidden;
		btn_ResetB.Visibility = Visibility.Hidden;
		bor_SearchRect1.Uid = "0";
		bor_SearchRect2.Uid = "0";
		Label_ResultError.Visibility = Visibility.Hidden;
		btn_Test.IsEnabled = true;
		rt_Result.Fill = Brushes.White;
		if (PreviewMode)
		{
			btn_Test.Visibility = Visibility.Hidden;
			btn_ColorDropper.Visibility = Visibility.Hidden;
			btn_TestNextImg.Visibility = Visibility.Hidden;
			btn_TestPrevImg.Visibility = Visibility.Hidden;
			btn_PreviewSwitch.Visibility = Visibility.Hidden;
			btn_MaskPreView.Visibility = Visibility.Hidden;
			btn_MaskEdit.Visibility = Visibility.Hidden;
		}
		BackupParam = inspection.paramFile.BinaryParam[TrackIndex];
		INSP_BINARYTOOL_PARAM param = inspection.paramFile.BinaryParam[TrackIndex];
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
		txt_AllResult.Clear();
		if (param.TrackerON)
		{
			grd_SIMP_Setup.Visibility = Visibility.Visible;
			grd_SIMP_RefSetup.Visibility = Visibility.Visible;
			grd_SIMP_THD_1.Visibility = Visibility.Visible;
			grd_SIMP_THD_1.IsEnabled = true;
			grd_SIMP_THD_1.Visibility = Visibility.Visible;
			grd_SIMP_THD_2.Visibility = Visibility.Visible;
			grd_SIMP_THD_3.Visibility = Visibility.Visible;
			grd_SIMP_THD_4.Visibility = Visibility.Visible;
		}
		else
		{
			grd_SIMP_Setup.Visibility = Visibility.Hidden;
			grd_SIMP_RefSetup.Visibility = Visibility.Hidden;
			grd_SIMP_THD_1.Visibility = Visibility.Hidden;
			grd_SIMP_THD_2.Visibility = Visibility.Hidden;
			grd_SIMP_THD_3.Visibility = Visibility.Hidden;
			grd_SIMP_THD_4.Visibility = Visibility.Hidden;
		}
		try
		{
			Label_Index.Content = (TrackIndex + 1).ToString();
			ts_TrackerON.IsChecked = param.TrackerON;
			if (param.TrackerON)
			{
				grd_SIMP_Setup.Visibility = Visibility.Visible;
				grd_SIMP_RefSetup.Visibility = Visibility.Visible;
			}
			else
			{
				grd_SIMP_Setup.Visibility = Visibility.Hidden;
				grd_SIMP_RefSetup.Visibility = Visibility.Hidden;
			}
			if (TrackIndex == 0)
			{
				grd_AdvFunction.Visibility = Visibility.Visible;
				chk_EnableJudgeWithAdvMeasure.IsChecked = inspection.paramFile.EnableJudgeWithAdvMeasure;
				int num = inspection.paramFile.JudgeWithAdvMeasureIndexA - 1;
				int num2 = inspection.paramFile.JudgeWithAdvMeasureIndexB - 1;
				if (num >= 0 && num < 40)
				{
					if (inspection.paramFile.AdvLocateMParam[num].TrackerON)
					{
						cb_JudgeWithAdvMeasureA.SelectedIndex = num + 1;
					}
					else
					{
						cb_JudgeWithAdvMeasureA.SelectedIndex = 0;
					}
				}
				else
				{
					cb_JudgeWithAdvMeasureA.SelectedIndex = 0;
				}
				if (num2 >= 0 && num2 < 40)
				{
					if (inspection.paramFile.AdvLocateMParam[num2].TrackerON)
					{
						cb_JudgeWithAdvMeasureB.SelectedIndex = num2 + 1;
					}
					else
					{
						cb_JudgeWithAdvMeasureB.SelectedIndex = 0;
					}
				}
				else
				{
					cb_JudgeWithAdvMeasureB.SelectedIndex = 0;
				}
			}
			else
			{
				grd_AdvFunction.Visibility = Visibility.Hidden;
			}
			cb_TrackerSelect.SelectedIndex = TrackIndex;
			txt_TrackerName.Text = param.TrackerName;
			lab_TrackIndex.Content = (TrackIndex + 1).ToString();
			if (param.Locked && User <= User_Permissions.USER_Engineer)
			{
				lab_LockMode.Visibility = Visibility.Visible;
			}
			else
			{
				lab_LockMode.Visibility = Visibility.Hidden;
			}
			SetConditionText(param.SearchRect2ON);
		}
		catch (Exception ex)
		{
			UpdateUI = true;
			string text = "BW07-BinaryWindow-CopyValueToUI()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		chk_TrackErrorSetNG.IsChecked = param.TrackErrorSetNG;
		cb_Track1AlignWayX.SelectedIndex = param.Tracker1AligmentXIndex;
		cb_Track2AlignWayX.SelectedIndex = param.Tracker2AligmentXIndex;
		cb_Track1AlignWayY.SelectedIndex = param.Tracker1AligmentYIndex;
		cb_Track2AlignWayY.SelectedIndex = param.Tracker2AligmentYIndex;
		cb_Track1AutoSize.SelectedIndex = param.AutoSize1_SelectIndex;
		cb_Track2AutoSize.SelectedIndex = param.AutoSize2_SelectIndex;
		if (!PreviewMode)
		{
			CopyValueSetReference(param, rect, rect2);
		}
		try
		{
			cb_ColorReference.Items.Clear();
			for (int i = 0; i <= 20; i++)
			{
				if (i != 0)
				{
					cb_ColorReference.Items.Add(i + " : " + inspection.paramFile.ColorReferenceParam[i - 1].TrackerName);
				}
				else
				{
					cb_ColorReference.Items.Add("-----");
				}
			}
			cb_ColorReference.SelectedIndex = param.ColorReferenceIndex;
			if (param.ColorReferenceIndex == 0)
			{
				grd_SIMP_RefSetup.Background = Brushes.White;
				btn_ReferenceSave.IsEnabled = false;
				txt_ReferenceName.IsEnabled = false;
				txt_ReferenceName.Text = "";
				for (int j = 0; j < 4; j++)
				{
					THD_Sliders[j].IsEnabled = true;
					THD_Expender[j].IsEnabled = true;
					if (j == 3 && updateCount <= 2)
					{
						THD_Expender[j].IsEnabled = false;
					}
					channelShiftMin[j] = 0;
					channelShiftMax[j] = 0;
				}
				cb_Channel_THD1.SelectedIndex = (int)param.THD1_ChannelIndex;
				cb_Channel_THD2.SelectedIndex = (int)param.THD2_ChannelIndex;
				cb_Channel_THD3.SelectedIndex = (int)param.THD3_ChannelIndex;
				cb_Channel_THD4.SelectedIndex = (int)param.THD4_ChannelIndex;
				chk_Thd_ORMode.IsChecked = param.THD_ORMode;
				if (param.THD_ORMode)
				{
					grd_SIMP_THD_4.Background = Brushes.LightGoldenrodYellow;
				}
				else
				{
					grd_SIMP_THD_4.Background = Brushes.White;
				}
				exp_THD2.IsExpanded = false;
				exp_THD3.IsExpanded = false;
				exp_THD4.IsExpanded = false;
				if (param.TrackerON)
				{
					exp_THD1.IsExpanded = true;
					exp_THD2.IsExpanded = param.THD2_ChannelON;
					exp_THD3.IsExpanded = param.THD3_ChannelON;
					exp_THD4.IsExpanded = param.THD4_ChannelON;
				}
				chk_EnableSoftCorrection.IsChecked = param.EnableSoftCorrection;
				if (inspection.paramFile.LightMonitorParam.EnableSoftCorrection)
				{
					chk_EnableSoftCorrection.Visibility = Visibility.Visible;
				}
				else
				{
					chk_EnableSoftCorrection.Visibility = Visibility.Hidden;
				}
				Button button = btn_SoftCorrectionTHD1;
				Button button2 = btn_SoftCorrectionTHD2;
				Button button3 = btn_SoftCorrectionTHD3;
				Visibility visibility = (btn_SoftCorrectionTHD4.Visibility = Visibility.Hidden);
				Visibility visibility3 = (button3.Visibility = visibility);
				Visibility visibility5 = (button2.Visibility = visibility3);
				button.Visibility = visibility5;
				Label label = lab_SoftCorrectionShiftTHD1;
				Label label2 = lab_SoftCorrectionShiftTHD2;
				Label label3 = lab_SoftCorrectionShiftTHD3;
				visibility = (lab_SoftCorrectionShiftTHD4.Visibility = Visibility.Hidden);
				visibility3 = (label3.Visibility = visibility);
				visibility5 = (label2.Visibility = visibility3);
				label.Visibility = visibility5;
				if (param.EnableSoftCorrection && inspection.paramFile.SoftCorrectionON && inspection.paramFile.LightMonitorParam.EnableSoftCorrection)
				{
					if (param.THD1_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD1_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min = param.THD1_Min;
						int max = param.THD1_Max;
						GetSoftCorrectionShiftValue((int)param.THD1_ChannelIndex, ref min, ref max, ref channelShiftMin[0], ref channelShiftMax[0]);
						rs_THD1.LowerValue = min;
						rs_THD1.UpperValue = max;
						nu_Thd1LimitMin.Value = min;
						nu_Thd1LimitMax.Value = max;
						btn_SoftCorrectionTHD1.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD1.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD1.Content = GetSoftShift((int)param.THD1_ChannelIndex);
					}
					else
					{
						rs_THD1.LowerValue = param.THD1_Min;
						rs_THD1.UpperValue = param.THD1_Max;
						nu_Thd1LimitMin.Value = param.THD1_Min;
						nu_Thd1LimitMax.Value = param.THD1_Max;
					}
					if (param.THD2_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD2_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min2 = param.THD2_Min;
						int max2 = param.THD2_Max;
						GetSoftCorrectionShiftValue((int)param.THD2_ChannelIndex, ref min2, ref max2, ref channelShiftMin[1], ref channelShiftMax[1]);
						rs_THD2.LowerValue = min2;
						rs_THD2.UpperValue = max2;
						nu_Thd2LimitMin.Value = min2;
						nu_Thd2LimitMax.Value = max2;
						btn_SoftCorrectionTHD2.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD2.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD2.Content = GetSoftShift((int)param.THD2_ChannelIndex);
					}
					else
					{
						rs_THD2.LowerValue = param.THD2_Min;
						rs_THD2.UpperValue = param.THD2_Max;
						nu_Thd2LimitMin.Value = param.THD2_Min;
						nu_Thd2LimitMax.Value = param.THD2_Max;
					}
					if (param.THD3_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD3_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min3 = param.THD3_Min;
						int max3 = param.THD3_Max;
						GetSoftCorrectionShiftValue((int)param.THD3_ChannelIndex, ref min3, ref max3, ref channelShiftMin[2], ref channelShiftMax[2]);
						rs_THD3.LowerValue = min3;
						rs_THD3.UpperValue = max3;
						nu_Thd3LimitMin.Value = min3;
						nu_Thd3LimitMax.Value = max3;
						btn_SoftCorrectionTHD3.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD3.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD3.Content = GetSoftShift((int)param.THD3_ChannelIndex);
					}
					else
					{
						rs_THD3.LowerValue = param.THD3_Min;
						rs_THD3.UpperValue = param.THD3_Max;
						nu_Thd3LimitMin.Value = param.THD3_Min;
						nu_Thd3LimitMax.Value = param.THD3_Max;
					}
					if (param.THD4_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD4_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min4 = param.THD4_Min;
						int max4 = param.THD4_Max;
						GetSoftCorrectionShiftValue((int)param.THD4_ChannelIndex, ref min4, ref max4, ref channelShiftMin[3], ref channelShiftMax[3]);
						rs_THD4.LowerValue = min4;
						rs_THD4.UpperValue = max4;
						nu_Thd4LimitMin.Value = min4;
						nu_Thd4LimitMax.Value = max4;
						btn_SoftCorrectionTHD4.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD4.Visibility = Visibility.Visible;
						lab_SoftCorrectionShiftTHD4.Content = GetSoftShift((int)param.THD4_ChannelIndex);
					}
					else
					{
						rs_THD4.LowerValue = param.THD4_Min;
						rs_THD4.UpperValue = param.THD4_Max;
						nu_Thd4LimitMin.Value = param.THD4_Min;
						nu_Thd4LimitMax.Value = param.THD4_Max;
					}
				}
				else
				{
					rs_THD1.LowerValue = param.THD1_Min;
					rs_THD1.UpperValue = param.THD1_Max;
					nu_Thd1LimitMin.Value = param.THD1_Min;
					nu_Thd1LimitMax.Value = param.THD1_Max;
					rs_THD2.LowerValue = param.THD2_Min;
					rs_THD2.UpperValue = param.THD2_Max;
					nu_Thd2LimitMin.Value = param.THD2_Min;
					nu_Thd2LimitMax.Value = param.THD2_Max;
					rs_THD3.LowerValue = param.THD3_Min;
					rs_THD3.UpperValue = param.THD3_Max;
					nu_Thd3LimitMin.Value = param.THD3_Min;
					nu_Thd3LimitMax.Value = param.THD3_Max;
					rs_THD4.LowerValue = param.THD4_Min;
					rs_THD4.UpperValue = param.THD4_Max;
					nu_Thd4LimitMin.Value = param.THD4_Min;
					nu_Thd4LimitMax.Value = param.THD4_Max;
				}
				cb_THD1_DEStart.SelectedIndex = (int)param.DE_Start1_Index;
				cb_THD1_DEEnd.SelectedIndex = (int)param.DE_End1_Index;
				cb_THD2_DEStart.SelectedIndex = (int)param.DE_Start2_Index;
				cb_THD2_DEEnd.SelectedIndex = (int)param.DE_End2_Index;
				cb_THD3_DEStart.SelectedIndex = (int)param.DE_Start3_Index;
				cb_THD3_DEEnd.SelectedIndex = (int)param.DE_End3_Index;
				cb_THD4_DEStart.SelectedIndex = (int)param.DE_Start4_Index;
				cb_THD4_DEEnd.SelectedIndex = (int)param.DE_End4_Index;
				cb_THD1_DEStartCount.SelectedIndex = param.DE_Start1_Count;
				cb_THD1_DEEndCount.SelectedIndex = param.DE_End1_Count;
				cb_THD2_DEStartCount.SelectedIndex = param.DE_Start2_Count;
				cb_THD2_DEEndCount.SelectedIndex = param.DE_End2_Count;
				cb_THD3_DEStartCount.SelectedIndex = param.DE_Start3_Count;
				cb_THD3_DEEndCount.SelectedIndex = param.DE_End3_Count;
				cb_THD4_DEStartCount.SelectedIndex = param.DE_Start4_Count;
				cb_THD4_DEEndCount.SelectedIndex = param.DE_End4_Count;
			}
			else
			{
				ReferenceLock = true;
				btn_ReferenceSave.IsEnabled = false;
				txt_ReferenceName.IsEnabled = false;
				grd_SIMP_RefSetup.Background = Brushes.LightGoldenrodYellow;
				for (int k = 0; k < 4; k++)
				{
					THD_Sliders[k].IsEnabled = false;
					THD_Expender[k].IsEnabled = false;
				}
				grd_SIMP_THD_1.IsEnabled = false;
				INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = inspection.paramFile.ColorReferenceParam[param.ColorReferenceIndex - 1];
				txt_ReferenceName.Text = iNSP_COLORREFERENCE_PARAM.TrackerName;
				cb_Channel_THD1.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD1_ChannelIndex;
				cb_Channel_THD2.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD2_ChannelIndex;
				cb_Channel_THD3.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD3_ChannelIndex;
				cb_Channel_THD4.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD4_ChannelIndex;
				exp_THD1.IsExpanded = true;
				exp_THD2.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD2_ChannelON;
				exp_THD3.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD3_ChannelON;
				exp_THD4.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD4_ChannelON;
				rs_THD1.LowerValue = iNSP_COLORREFERENCE_PARAM.THD1_Min;
				rs_THD1.UpperValue = iNSP_COLORREFERENCE_PARAM.THD1_Max;
				nu_Thd1LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD1_Min;
				nu_Thd1LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD1_Max;
				rs_THD2.LowerValue = iNSP_COLORREFERENCE_PARAM.THD2_Min;
				rs_THD2.UpperValue = iNSP_COLORREFERENCE_PARAM.THD2_Max;
				nu_Thd2LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD2_Min;
				nu_Thd2LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD2_Max;
				rs_THD3.LowerValue = iNSP_COLORREFERENCE_PARAM.THD3_Min;
				rs_THD3.UpperValue = iNSP_COLORREFERENCE_PARAM.THD3_Max;
				nu_Thd3LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD3_Min;
				nu_Thd3LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD3_Max;
				rs_THD4.LowerValue = iNSP_COLORREFERENCE_PARAM.THD4_Min;
				rs_THD4.UpperValue = iNSP_COLORREFERENCE_PARAM.THD4_Max;
				nu_Thd4LimitMin.Value = iNSP_COLORREFERENCE_PARAM.THD4_Min;
				nu_Thd4LimitMax.Value = iNSP_COLORREFERENCE_PARAM.THD4_Max;
				cb_THD1_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start1_Index;
				cb_THD1_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End1_Index;
				cb_THD2_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start2_Index;
				cb_THD2_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End2_Index;
				cb_THD3_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start3_Index;
				cb_THD3_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End3_Index;
				cb_THD4_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start4_Index;
				cb_THD4_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End4_Index;
				cb_THD1_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start1_Count;
				cb_THD1_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End1_Count;
				cb_THD2_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start2_Count;
				cb_THD2_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End2_Count;
				cb_THD3_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start3_Count;
				cb_THD3_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End3_Count;
				cb_THD4_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start4_Count;
				cb_THD4_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End4_Count;
			}
		}
		catch (Exception ex2)
		{
			string text2 = "BW07-BinaryWindow-CopyValueToUI()-03-";
			text2 += ex2.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return;
		}
		try
		{
			if (param.SearchRect2ON)
			{
				cb_InspRule1.SelectedIndex = (int)param.InspRule1_Index;
				cb_InspRule2.SelectedIndex = (int)param.InspRule2_Index;
				cb_InspRule3.SelectedIndex = (int)param.InspRule3_Index;
				cb_InspRule4.SelectedIndex = (int)param.InspRule4_Index;
			}
			else
			{
				if (param.InspRule1_Index >= BlobCondition.EM_2BLOB_Ratio && param.InspRule1_Index <= BlobCondition.EM_2BLOB_WRatio)
				{
					cb_InspRule1.SelectedIndex = 0;
				}
				else
				{
					cb_InspRule1.SelectedIndex = (int)param.InspRule1_Index;
				}
				if (param.InspRule2_Index >= BlobCondition.EM_2BLOB_Ratio && param.InspRule2_Index <= BlobCondition.EM_2BLOB_WRatio)
				{
					cb_InspRule2.SelectedIndex = 0;
				}
				else
				{
					cb_InspRule2.SelectedIndex = (int)param.InspRule2_Index;
				}
				if (param.InspRule3_Index >= BlobCondition.EM_2BLOB_Ratio && param.InspRule3_Index <= BlobCondition.EM_2BLOB_WRatio)
				{
					cb_InspRule3.SelectedIndex = 0;
				}
				else
				{
					cb_InspRule3.SelectedIndex = (int)param.InspRule3_Index;
				}
				if (param.InspRule4_Index >= BlobCondition.EM_2BLOB_Ratio && param.InspRule4_Index <= BlobCondition.EM_2BLOB_WRatio)
				{
					cb_InspRule4.SelectedIndex = 0;
				}
				else
				{
					cb_InspRule4.SelectedIndex = (int)param.InspRule4_Index;
				}
			}
			if (param.MB_InspRule1_Index == BlobCondition.EM_ALLAREA)
			{
				cb_MB_InspRule1.SelectedIndex = 6;
			}
			else
			{
				cb_MB_InspRule1.SelectedIndex = (int)param.MB_InspRule1_Index;
			}
			cb_MB_InspRule2.SelectedIndex = (int)param.MB_InspRule2_Index;
			cb_MB_InspRule3.SelectedIndex = (int)param.MB_InspRule3_Index;
			cb_MB_InspRule4.SelectedIndex = (int)param.MB_InspRule4_Index;
			if (param.MB_InspRuleAND)
			{
				rad_MB_RuleAND.IsChecked = true;
			}
			else
			{
				rad_MB_RuleOR.IsChecked = true;
			}
			cb_MB_BlobSort.SelectedIndex = param.MB_BlobSort;
			txt_MB_BlobNum.Text = param.MB_BlobNum.ToString();
			txt_MB_NGCount.Text = param.MB_AllowNGCount.ToString();
			txt_MB_InspRule1Min.Text = param.MB_InspRule1_Min.ToString();
			txt_MB_InspRule2Min.Text = param.MB_InspRule2_Min.ToString();
			txt_MB_InspRule3Min.Text = param.MB_InspRule3_Min.ToString();
			txt_MB_InspRule4Min.Text = param.MB_InspRule4_Min.ToString();
			txt_MB_InspRule1Max.Text = param.MB_InspRule1_Max.ToString();
			txt_MB_InspRule2Max.Text = param.MB_InspRule2_Max.ToString();
			txt_MB_InspRule3Max.Text = param.MB_InspRule3_Max.ToString();
			txt_MB_InspRule4Max.Text = param.MB_InspRule4_Max.ToString();
			if (param.InspRuleAND)
			{
				rad_RuleAND.IsChecked = true;
			}
			else
			{
				rad_RuleOR.IsChecked = true;
			}
			txt_InspRule1Min.Text = GetComDigitsStr(param.InspRule1_Min, txt_InspRule1Min);
			txt_InspRule2Min.Text = GetComDigitsStr(param.InspRule2_Min, txt_InspRule2Min);
			txt_InspRule3Min.Text = GetComDigitsStr(param.InspRule3_Min, txt_InspRule3Min);
			txt_InspRule4Min.Text = GetComDigitsStr(param.InspRule4_Min, txt_InspRule4Min);
			txt_InspRule1Max.Text = GetComDigitsStr(param.InspRule1_Max, txt_InspRule1Max);
			txt_InspRule2Max.Text = GetComDigitsStr(param.InspRule2_Max, txt_InspRule2Max);
			txt_InspRule3Max.Text = GetComDigitsStr(param.InspRule3_Max, txt_InspRule3Max);
			txt_InspRule4Max.Text = GetComDigitsStr(param.InspRule4_Max, txt_InspRule4Max);
			cb_BinaryErrorBin.SelectedIndex = (int)param.ErrorBin;
			cb_ParetoChart.SelectedIndex = param.ParetoIndex;
			_ = cb_Track1AlignWayX.Items.Count;
			_ = cb_Track2AlignWayX.Items.Count;
			tab_BlobAnalysis.SelectedIndex = param.BlobAnalysisSelect;
			cb_BlobLineSort.SelectedIndex = param.BlobLineSort;
			txt_BlobLineNum.Text = param.BlobLineNum.ToString();
			txt_BlobLineMin.Text = param.BlobLineMin.ToString();
			txt_BlobLineMax.Text = param.BlobLineMax.ToString();
			txt_BlobLineWidthMin.Text = param.BlobLineWidthMin.ToString();
			txt_BlobLineWidthMax.Text = param.BlobLineWidthMax.ToString();
			txt_BlobLineHeightMin.Text = param.BlobLineHeightMin.ToString();
			txt_BlobLineHeightMax.Text = param.BlobLineHeightMax.ToString();
			txt_BlobLineAreaMin.Text = param.BlobLineAreaMin.ToString();
			txt_BlobLineAreaMax.Text = param.BlobLineAreaMax.ToString();
			txt_LineWidthMinW.Text = param.LineWidthMinW.ToString();
			txt_LineWidthMinH.Text = param.LineWidthMinH.ToString();
			txt_LineWidthMinArea.Text = param.LineWidthMinArea.ToString();
			cb_LineWidthA.IsChecked = param.EnableLineWidthA;
			cb_LineWidthW.IsChecked = param.EnableLineWidthW;
			cb_LineWidthH.IsChecked = param.EnableLineWidthH;
			cb_BlobValueUsePixelValue.IsChecked = param.UsePixelValue;
			txt_AllowLineWidth.Text = param.AllowLineWidth.ToString();
			txt_AllowLineHeight.Text = param.AllowLineHeight.ToString();
			txt_AllowLineWidthError.Text = param.AllowLineError.ToString();
			txt_AllowErrorW.Text = param.AllowErrorW.ToString();
			txt_AllowErrorH.Text = param.AllowErrorH.ToString();
			chk_SelectFace.IsChecked = param.EnableSelectFace;
			SetSelectFace(param.FaceIndex);
			chk_UseFontMask.IsChecked = param.UseFontMask;
			cb_MatchMaskSelect1.SelectedIndex = param.UseMatchTextMask1;
			cb_MatchMaskSelect2.SelectedIndex = param.UseMatchTextMask2;
			cb_MatchMaskSelect3.SelectedIndex = param.UseMatchTextMask3;
			cb_MatchText_DEStart1.SelectedIndex = (int)param.MatchTextDE_Start1_Index;
			cb_MatchText_DEStart2.SelectedIndex = (int)param.MatchTextDE_Start2_Index;
			cb_MatchText_DEStart3.SelectedIndex = (int)param.MatchTextDE_Start3_Index;
			cb_MatchText_DEStartCount1.SelectedIndex = param.MatchTextDE_Start1_Count;
			cb_MatchText_DEStartCount2.SelectedIndex = param.MatchTextDE_Start2_Count;
			cb_MatchText_DEStartCount3.SelectedIndex = param.MatchTextDE_Start3_Count;
		}
		catch (Exception ex3)
		{
			string text3 = "BW07-BinaryWindow-CopyValueToUI()-04-";
			text3 += ex3.ToString();
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
			return;
		}
		if (param.EnableSelectFace && FaceIndex != 0 && FaceIndex != SelectFace)
		{
			rt_Result.Fill = Brushes.White;
			Label_ResultError.Visibility = Visibility.Visible;
			if (LanguageType == 1)
			{
				Label_ResultError.Text = "選面不符合，ByPass!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Text = "选面不符合，ByPass!";
			}
			else if (LanguageType == 3)
			{
				Label_ResultError.Text = "選択面が一致しません，ByPass。";
			}
			else
			{
				Label_ResultError.Text = "Select Face Doesn't Match! ByPass!";
			}
			lab_Time.Content = "0";
		}
		UpdateUI = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode || !inited || !UpdateUI || ShowBinaryCompare)
		{
			return;
		}
		exp_TrackerFilllet.Visibility = Visibility.Hidden;
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
		}
		else
		{
			if (!inspection.buffers.LocatedReady || DontSave || DontSave1 || DontSave2)
			{
				return;
			}
			Label_ResultError.Visibility = Visibility.Hidden;
			OpenCvSharp.Rect[] array = new OpenCvSharp.Rect[2];
			if (!GetTracker(ref array[0], 0) || (ts_Tracker2ON.IsChecked == true && !GetTracker(ref array[1], 1)))
			{
				return;
			}
			if (array[0].Width <= 0 || array[0].Height <= 0)
			{
				SetResult(result: false);
				txt_AllResult.Clear();
				if (LanguageType == 1)
				{
					txt_AllResult.Text = "檢測區域A寬度高度為0!，請重設檢測區域A";
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text = "检测区域A宽度高度为0!，请重设检测区域A";
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text = "検査領域Aの幅と高さが0です！検査領域Aを再設定してください。";
				}
				else
				{
					txt_AllResult.Text = "Tracker A Width and Height < 0!，Please Reset Tracker A!";
				}
				DrawBackGround();
				return;
			}
			if (ts_Tracker2ON.IsChecked == true && (array[1].Width <= 0 || array[1].Height <= 0))
			{
				SetResult(result: false);
				txt_AllResult.Clear();
				if (LanguageType == 1)
				{
					txt_AllResult.Text = "檢測區域B寬度高度為0!，請重設檢測區域B";
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text = "检测区域B宽度高度为0!，请重设检测区域B";
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text = "検査領域Bの幅と高さが0です！検査領域Bを再設定してください。";
				}
				else
				{
					txt_AllResult.Text = "Tracker B Width and Height < 0!，Please Reset Tracker B!";
				}
				DrawBackGround();
				return;
			}
			WriteUIValueToParam();
			if (chk_SelectFace.IsChecked == true && FaceIndex != 0 && FaceIndex != SelectFace)
			{
				rt_Result.Fill = Brushes.White;
				Label_ResultError.Visibility = Visibility.Visible;
				txt_AllResult.Clear();
				if (LanguageType == 1)
				{
					Label_ResultError.Text = "選面不符合，ByPass!";
				}
				else if (LanguageType == 2)
				{
					Label_ResultError.Text = "选面不符合，ByPass!";
				}
				else if (LanguageType == 3)
				{
					Label_ResultError.Text = "選択面が一致しません，ByPass。";
				}
				else
				{
					Label_ResultError.Text = "Select Face Doesn't Match! ByPass!";
				}
				lab_Time.Content = "0";
				return;
			}
			Label_ResultError.Visibility = Visibility.Hidden;
			SearchRectResult[0] = (SearchRectResult[1] = true);
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			TimeStart();
			bool flag = inspection.binaryTool[TrackIndex].Inspect(TrackIndex, ref iresult, FaceIndex);
			double num = TimeStop();
			lab_Time.Content = string.Format("{0:N3}", num);
			SetResult(flag);
			if (tab_BlobAnalysis.SelectedIndex == 0)
			{
				OpenCvSharp.Rect[] array2 = new OpenCvSharp.Rect[2];
				Blobs[0] = inspection.binaryTool[TrackIndex].Blobs[0];
				Blobs[1] = inspection.binaryTool[TrackIndex].Blobs[1];
				array2[0] = Blobs[0].NGRect;
				array2[1] = Blobs[1].NGRect;
				if (!Blobs[0].result)
				{
					lab_Tracker1Result.Background = Brushes.Red;
				}
				else
				{
					lab_Tracker1Result.Background = Brushes.Lime;
				}
				if (ts_Tracker2ON.IsChecked == true)
				{
					if (!Blobs[1].result)
					{
						lab_Tracker2Result.Background = Brushes.Red;
					}
					else
					{
						lab_Tracker2Result.Background = Brushes.Lime;
					}
				}
				else
				{
					lab_Tracker2Result.Background = Brushes.White;
				}
				CheckResult();
				if (inspection.buffers.LocatedSrc.Channels() == 3)
				{
					ShowBitmap = inspection.buffers.LocatedSrc.Clone();
				}
				else
				{
					Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
				}
				DoThdImage();
				bool flag2 = false;
				for (int i = 0; i < 4; i++)
				{
					if (InspRule[i].SelectedIndex == 11)
					{
						flag2 = true;
					}
				}
				for (int j = 0; j < 2; j++)
				{
					if (!SearchRectResult[j])
					{
						ShowBitmap.Rectangle(array2[j], Scalar.Red);
						if (flag2)
						{
							ShowBitmap.Rectangle(array[j], Scalar.Red, 3);
						}
					}
					else
					{
						ShowBitmap.Rectangle(array2[j], Scalar.Green);
					}
				}
				txt_AllResult.Clear();
				txt_AllResult.Text += inspection.binaryTool[TrackIndex].Tracker1ResultText;
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += inspection.binaryTool[TrackIndex].Tracker2ResultText;
				}
				if (TrackIndex == 0)
				{
					int num2 = 0;
					num2 = ((!inspection.paramFile.SelectFaceON) ? cb_JudgeWithAdvMeasureA.SelectedIndex : ((FaceIndex != 1) ? cb_JudgeWithAdvMeasureB.SelectedIndex : cb_JudgeWithAdvMeasureA.SelectedIndex));
					if (chk_EnableJudgeWithAdvMeasure.IsChecked == true && num2 > 0 && inspection.paramFile.JudgeWithAdvMeasureResult)
					{
						string text = string.Format("\n特殊判斷模式開啟，進階量測[{0}]結果為OK，本次判定結果皆為皆為OK\n", inspection.paramFile.AdvLocateMParam[num2 - 1].TrackerName);
						txt_AllResult.Text += text;
					}
					else if (chk_EnableJudgeWithAdvMeasure.IsChecked == true && num2 > 0 && !inspection.paramFile.JudgeWithAdvMeasureResult)
					{
						string text2 = string.Format("\n特殊判斷模式開啟，進階量測[{0}]結果為NG，不影響本次判定結果\n", inspection.paramFile.AdvLocateMParam[num2 - 1].TrackerName);
						txt_AllResult.Text += text2;
					}
				}
			}
			else if (tab_BlobAnalysis.SelectedIndex == 1)
			{
				DoThdImage();
				lab_Tracker1Result2.Background = Brushes.White;
				lab_Tracker2Result2.Background = Brushes.White;
				Blobs[0] = inspection.binaryTool[TrackIndex].Blobs[0];
				Blobs[1] = inspection.binaryTool[TrackIndex].Blobs[1];
				txt_AllResult.Clear();
				int num3 = Convert.ToInt32(txt_BlobLineMin.Text);
				int num4 = Convert.ToInt32(txt_BlobLineMax.Text);
				int num5 = Convert.ToInt32(txt_BlobLineNum.Text);
				int num6 = 0;
				if (num3 > 0 && num4 > 0 && num5 > 0 && inspection.binaryTool[TrackIndex].BlobLine1.Count > 0)
				{
					for (int k = 0; k < inspection.binaryTool[TrackIndex].BlobLine1.Count && k < num5; k++)
					{
						if (!inspection.binaryTool[TrackIndex].BlobLine1[k].result)
						{
							if (inspection.paramFile.MeasureUnit == 0)
							{
								if (inspection.paramFile.UseFloat)
								{
									if (LanguageType == 1)
									{
										txt_AllResult.Text += string.Format("[A]線條寬度={0:N3}um, 線條高度={1:N3}um, 線條面積={2:N3}um [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
									}
									else if (LanguageType == 2)
									{
										txt_AllResult.Text += string.Format("[A]线条宽度={0:N3}um, 线条高度={1:N3}um, 线条面积={2:N3}um [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
									}
									else if (LanguageType == 3)
									{
										txt_AllResult.Text += string.Format("[A]線幅={0:N3}μm、線高={1:N3}μm、面積={2:N3}μm [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
									}
									else
									{
										txt_AllResult.Text += string.Format("[A]Line Width={0:N3}um, Line Height={1:N3}um, Line Area={2:N3}um [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
									}
								}
								else if (LanguageType == 1)
								{
									txt_AllResult.Text += string.Format("[A]線條寬度={0}um, 線條高度={1}um, 線條面積={2}um [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else if (LanguageType == 2)
								{
									txt_AllResult.Text += string.Format("[A]线条宽度={0}um, 线条高度={1}um, 线条面积={2}um [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else if (LanguageType == 3)
								{
									txt_AllResult.Text += string.Format("[A]線幅={0}μm、線高={1}μm、面積={2}μm [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else
								{
									txt_AllResult.Text += string.Format("[A]Line Width={0}um, Line Height={1}um, Line Area={2}um [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
							}
							else if (inspection.paramFile.UseFloat)
							{
								if (LanguageType == 1)
								{
									txt_AllResult.Text += string.Format("[A]線條寬度={0:N3}mm, 線條高度={1:N3}mm, 線條面積={2:N3}mm [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else if (LanguageType == 2)
								{
									txt_AllResult.Text += string.Format("[A]线条宽度={0:N3}mm, 线条高度={1:N3}mm, 线条面积={2:N3}mm [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else if (LanguageType == 3)
								{
									txt_AllResult.Text += string.Format("[A]線幅={0:N3}mm、線高={1:N3}mm、面積={2:N3}mm [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else
								{
									txt_AllResult.Text += string.Format("[A]Line Width={0:N3}mm, Line Height={1:N3}mm, Line Area={2:N3}mm [NG]\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
							}
							else if (LanguageType == 1)
							{
								txt_AllResult.Text += string.Format("[A]線條寬度={0}mm, 線條高度={1}mm, 線條面積={2}mm [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else if (LanguageType == 2)
							{
								txt_AllResult.Text += string.Format("[A]线条宽度={0}mm, 线条高度={1}mm, 线条面积={2}mm [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else if (LanguageType == 3)
							{
								txt_AllResult.Text += string.Format("[A]線幅={0}mm、線高={1}mm、面積={2}mm [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else
							{
								txt_AllResult.Text += string.Format("[A]Line Width={0}mm, Line Height={1}mm, Line Area={2}mm [NG]\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].BlobLine1[k].NGRect, Scalar.Red);
							continue;
						}
						if (inspection.paramFile.MeasureUnit == 0)
						{
							if (inspection.paramFile.UseFloat)
							{
								if (LanguageType == 1)
								{
									txt_AllResult.Text += string.Format("[A]線條寬度={0:N3}um, 線條高度={1:N3}um, 線條面積={2:N3}um\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else if (LanguageType == 2)
								{
									txt_AllResult.Text += string.Format("[A]线条宽度={0:N3}um, 线条高度={1:N3}um, 线条面积={2:N3}um \n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else if (LanguageType == 3)
								{
									txt_AllResult.Text += string.Format("[A]線幅={0:N3}μm、線高={1:N3}μm、線面積={2:N3}μm \n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
								else
								{
									txt_AllResult.Text += string.Format("[A]Line Width={0:N3}um, Line Height={1:N3}um, Line Area={2:N3}um\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
								}
							}
							else if (LanguageType == 1)
							{
								txt_AllResult.Text += string.Format("[A]線條寬度={0}um, 線條高度={1}um, 線條面積={2}um\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else if (LanguageType == 2)
							{
								txt_AllResult.Text += string.Format("[A]线条宽度={0}um, 线条高度={1}um, 线条面积={2}um \n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else if (LanguageType == 3)
							{
								txt_AllResult.Text += string.Format("[A]線幅={0}μm、線高={1}μm、線面積={2}μm \n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else
							{
								txt_AllResult.Text += string.Format("[A]Line Width={0}um, Line Height={1}um, Line Area={2}um\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
						}
						else if (inspection.paramFile.UseFloat)
						{
							if (LanguageType == 1)
							{
								txt_AllResult.Text += string.Format("[A]線條寬度={0:N3}mm, 線條高度={1:N3}mm, 線條面積={2:N3}mm\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else if (LanguageType == 2)
							{
								txt_AllResult.Text += string.Format("[A]线条宽度={0:N3}mm, 线条高度={1:N3}mm, 线条面积={2:N3}mm \n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else if (LanguageType == 3)
							{
								txt_AllResult.Text += string.Format("[A]線幅={0:N3}μm、線高={1:N3}μm、線面積={2:N3}μm \n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
							else
							{
								txt_AllResult.Text += string.Format("[A]Line Width={0:N3}mm, Line Height={1:N3}mm, Line Area={2:N3}mm\n", inspection.binaryTool[TrackIndex].BlobLine1[k].Width, inspection.binaryTool[TrackIndex].BlobLine1[k].Height, inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
							}
						}
						else if (LanguageType == 1)
						{
							txt_AllResult.Text += string.Format("[A]線條寬度={0}mm, 線條高度={1}mm, 線條面積={2}mm\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text += string.Format("[A]线条宽度={0}mm, 线条高度={1}mm, 线条面积={2}mm \n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text += string.Format("[A]線幅={0}μm、線高={1}μm、線面積={2}μm \n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
						}
						else
						{
							txt_AllResult.Text += string.Format("[A]Line Width={0}mm, Line Height={1}mm, Line Area={2}mm\n", (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Width, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Height, (int)inspection.binaryTool[TrackIndex].BlobLine1[k].Area);
						}
						ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].BlobLine1[k].NGRect, Scalar.Green);
						num6++;
					}
					txt_OKLineResultA.Text = num6.ToString();
				}
				if (LanguageType == 1)
				{
					txt_AllResult.Text += string.Format("[A]線條合格數量為{0}，允許最小合格數為{1}，允許最大合格數為{2}\n", num6, num3, num4);
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text += string.Format("[A]线条合格数量为{0}，允许最小合格数为{1}，允许最大合格数为{2}\n", num6, num3, num4);
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text += string.Format("[A]線の合格数は{0}、許容最小合格数は{1}、許容最大合格数は{2}です。\n", num6, num3, num4);
				}
				else
				{
					txt_AllResult.Text += string.Format("[A]Passed Line={0}，Min Passs Line={1}，Max Pass Line=\n", num6, num3, num4);
				}
				txt_BlobLineMin.Background = Brushes.White;
				txt_BlobLineMax.Background = Brushes.White;
				if (num6 < num3)
				{
					txt_BlobLineMin.Background = Brushes.Red;
				}
				if (num6 > num4)
				{
					txt_BlobLineMax.Background = Brushes.Red;
				}
				if (!Blobs[0].result)
				{
					lab_Tracker1Result2.Background = Brushes.Red;
				}
				else
				{
					lab_Tracker1Result2.Background = Brushes.Lime;
				}
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += "\n";
					num6 = 0;
					if (num3 > 0 && num4 > 0 && num5 > 0 && inspection.binaryTool[TrackIndex].BlobLine2.Count > 0)
					{
						for (int l = 0; l < inspection.binaryTool[TrackIndex].BlobLine2.Count && l < num5; l++)
						{
							if (!inspection.binaryTool[TrackIndex].BlobLine2[l].result)
							{
								if (LanguageType == 1)
								{
									txt_AllResult.Text += string.Format("[B]線條寬度{0}, 線條高度{1}, 線條面積{2} [NG]\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
								}
								else if (LanguageType == 2)
								{
									txt_AllResult.Text += string.Format("[B]线条宽度{0}, 线条高度{1}, 线条面积{2} [NG]\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
								}
								else if (LanguageType == 3)
								{
									txt_AllResult.Text += string.Format("[B]線の幅は{0}、高さは{1}、面積は{2}です。[NG]\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
								}
								else
								{
									txt_AllResult.Text += string.Format("[B]Line Width{0}, Line Height{1}, Line Area{2} [NG]\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
								}
								ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].BlobLine2[l].NGRect, Scalar.Red);
								continue;
							}
							if (LanguageType == 1)
							{
								txt_AllResult.Text += string.Format("[B]線條寬度{0}, 線條高度{1}, 線條面積{2}\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
							}
							else if (LanguageType == 2)
							{
								txt_AllResult.Text += string.Format("[B]线条宽度{0}, 线条高度{1}, 线条面积{2}\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
							}
							if (LanguageType == 3)
							{
								txt_AllResult.Text += string.Format("[B]線の幅は{0}、高さは{1}、面積は{2}です。[NG]\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
							}
							else
							{
								txt_AllResult.Text += string.Format("[B]Line Width{0}, Line Height{1}, Line Area{2}\n", inspection.binaryTool[TrackIndex].BlobLine2[l].Width, inspection.binaryTool[TrackIndex].BlobLine2[l].Height, inspection.binaryTool[TrackIndex].BlobLine2[l].Area);
							}
							ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].BlobLine2[l].NGRect, Scalar.Green);
							num6++;
						}
						txt_OKLineResultB.Text = num6.ToString();
					}
					if (LanguageType == 1)
					{
						txt_AllResult.Text += string.Format("[B]線條合格數量為{0}，允許最小合格數為{1}，允許最大合格數為{2}\n", num6, num3, num4);
					}
					else if (LanguageType == 2)
					{
						txt_AllResult.Text += string.Format("[B]线条合格数量为{0}，允许最小合格数为{1}，允许最大合格数为{2}\n", num6, num3, num4);
					}
					else if (LanguageType == 3)
					{
						txt_AllResult.Text += string.Format("[B]線の合格数は{0}、許容最小合格数は{1}、許容最大合格数は{2}です。\n", num6, num3, num4);
					}
					else
					{
						txt_AllResult.Text += string.Format("[B]Passed Line={0}，Min Passs Line={1}，Max Pass Line=\n", num6, num3, num4);
					}
					if (num6 < num3)
					{
						txt_BlobLineMin.Background = Brushes.Red;
					}
					if (num6 > num4)
					{
						txt_BlobLineMax.Background = Brushes.Red;
					}
					if (!Blobs[1].result)
					{
						lab_Tracker2Result2.Background = Brushes.Red;
					}
					else
					{
						lab_Tracker2Result2.Background = Brushes.Lime;
					}
				}
			}
			else if (tab_BlobAnalysis.SelectedIndex == 2)
			{
				DoThdImage();
				txt_AllResult.Clear();
				for (int m = 0; m < inspection.binaryTool[TrackIndex].MultiBlob1.Count && m < Convert.ToInt32(txt_MB_BlobNum.Text); m++)
				{
					if (!inspection.binaryTool[TrackIndex].MultiBlob1[m].result)
					{
						ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].MultiBlob1[m].NGRect, Scalar.Red);
					}
					else
					{
						ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].MultiBlob1[m].NGRect, Scalar.Green);
					}
				}
				for (int n = 0; n < inspection.binaryTool[TrackIndex].MultiBlob2.Count && n < Convert.ToInt32(txt_MB_BlobNum.Text); n++)
				{
					if (!inspection.binaryTool[TrackIndex].MultiBlob2[n].result)
					{
						ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].MultiBlob2[n].NGRect, Scalar.Red);
					}
					else
					{
						ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].MultiBlob2[n].NGRect, Scalar.Green);
					}
				}
				txt_AllResult.Text += inspection.binaryTool[TrackIndex].Tracker1ResultText;
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += inspection.binaryTool[TrackIndex].Tracker2ResultText;
				}
			}
			else
			{
				DoThdImage();
				txt_AllResult.Clear();
				if (cb_LineWidthA.IsChecked == true)
				{
					if (inspection.binaryTool[TrackIndex].LineWidthResultAA)
					{
						rt_LineWidthAA.Fill = Brushes.Lime;
					}
					else
					{
						rt_LineWidthAA.Fill = Brushes.Red;
					}
					if (inspection.binaryTool[TrackIndex].LineWidthResultAB)
					{
						rt_LineWidthAB.Fill = Brushes.Lime;
					}
					else
					{
						rt_LineWidthAB.Fill = Brushes.Red;
					}
					for (int num7 = 0; num7 < inspection.binaryTool[TrackIndex].LineWidthRect.Count; num7++)
					{
						if (!flag)
						{
							ShowBitmap.Rectangle(inspection.binaryTool[TrackIndex].LineWidthRect[num7], Scalar.Red);
						}
					}
				}
				else
				{
					rt_LineWidthAA.Fill = Brushes.White;
					rt_LineWidthAB.Fill = Brushes.White;
				}
				if (cb_LineWidthW.IsChecked == true)
				{
					txt_AllowErrorW.Background = Brushes.White;
					if (inspection.binaryTool[TrackIndex].LineWidthResultWA)
					{
						rt_LineWidthWA.Fill = Brushes.Lime;
					}
					else
					{
						rt_LineWidthWA.Fill = Brushes.Red;
						txt_AllowErrorW.Background = Brushes.Red;
					}
					if (inspection.binaryTool[TrackIndex].LineWidthResultWB)
					{
						rt_LineWidthWB.Fill = Brushes.Lime;
					}
					else
					{
						rt_LineWidthWB.Fill = Brushes.Red;
						txt_AllowErrorW.Background = Brushes.Red;
					}
				}
				else
				{
					rt_LineWidthWA.Fill = Brushes.White;
					rt_LineWidthWB.Fill = Brushes.White;
					txt_AllowErrorW.Background = Brushes.White;
				}
				if (cb_LineWidthH.IsChecked == true)
				{
					txt_AllowErrorH.Background = Brushes.White;
					if (inspection.binaryTool[TrackIndex].LineWidthResultHA)
					{
						rt_LineWidthHA.Fill = Brushes.Lime;
					}
					else
					{
						rt_LineWidthHA.Fill = Brushes.Red;
						txt_AllowErrorH.Background = Brushes.Red;
					}
					txt_AllowErrorH.Background = Brushes.White;
					if (inspection.binaryTool[TrackIndex].LineWidthResultHB)
					{
						rt_LineWidthHB.Fill = Brushes.Lime;
					}
					else
					{
						rt_LineWidthHB.Fill = Brushes.Red;
						txt_AllowErrorH.Background = Brushes.Red;
					}
				}
				else
				{
					rt_LineWidthHA.Fill = Brushes.White;
					txt_AllowErrorH.Background = Brushes.White;
				}
				txt_AllowLineErrorResultA.Text = inspection.binaryTool[TrackIndex].LineWidthErrorCountA.ToString();
				txt_AllowLineErrorResultB.Text = inspection.binaryTool[TrackIndex].LineWidthErrorCountB.ToString();
				txt_AllowErrorWResultA.Text = inspection.binaryTool[TrackIndex].LineWidthErrorA.ToString();
				txt_AllowErrorWResultB.Text = inspection.binaryTool[TrackIndex].LineWidthErrorB.ToString();
				txt_AllowErrorHResultA.Text = inspection.binaryTool[TrackIndex].LineHeightErrorA.ToString();
				txt_AllowErrorHResultB.Text = inspection.binaryTool[TrackIndex].LineHeightErrorB.ToString();
				txt_AllResult.Text += inspection.binaryTool[TrackIndex].Tracker1ResultText;
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += inspection.binaryTool[TrackIndex].Tracker2ResultText;
				}
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Btn_THD1Undo_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		switch (num)
		{
		case 0:
			THD_Sliders[num].LowerValue = BackupParam.THD1_Min;
			THD_Sliders[num].UpperValue = BackupParam.THD1_Max;
			break;
		case 1:
			THD_Sliders[num].LowerValue = BackupParam.THD2_Min;
			THD_Sliders[num].UpperValue = BackupParam.THD2_Max;
			break;
		case 2:
			THD_Sliders[num].LowerValue = BackupParam.THD3_Min;
			THD_Sliders[num].UpperValue = BackupParam.THD3_Max;
			break;
		case 3:
			THD_Sliders[num].LowerValue = BackupParam.THD4_Min;
			THD_Sliders[num].UpperValue = BackupParam.THD4_Max;
			break;
		}
		Btn_Test_Click(this, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteModifyLog(int index)
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
			string text3 = "[" + RecipeName + "][" + UserID + "] " + now.ToString("[yyyy-MM-dd] [HH:mm:ss]");
			INSP_BINARYTOOL_PARAM param = inspection.paramFile.BinaryParam[index];
			string text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["Binary"], index + 1, param.TrackerName);
			if (param.TrackerON != BackupParam.TrackerON)
			{
				if (param.TrackerON)
				{
					ModifyLog += string.Format("{0} [{1}]\n", text4, LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1}]\n", text4, LanguageManager.Instance["Close"]);
				}
			}
			if (param.THD1_ChannelIndex != BackupParam.THD1_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD1_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD1_ChannelIndex));
			}
			if (param.THD1_Min != BackupParam.THD1_Min)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD1_Min, LanguageManager.Instance["ChangedTo"], param.THD1_Min);
			}
			if (param.THD1_Max != BackupParam.THD1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD1_Max, LanguageManager.Instance["ChangedTo"], param.THD1_Max);
			}
			if (param.THD2_ChannelIndex != BackupParam.THD2_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD2_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD2_ChannelIndex));
			}
			if (param.THD2_Min != BackupParam.THD2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD2_Min, LanguageManager.Instance["ChangedTo"], param.THD2_Min);
			}
			if (param.THD2_Max != BackupParam.THD2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD2_Max, LanguageManager.Instance["ChangedTo"], param.THD2_Max);
			}
			if (param.THD2_ChannelON != BackupParam.THD2_ChannelON)
			{
				if (param.THD2_ChannelON)
				{
					ModifyLog += string.Format("{0} [({1}2) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}2) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			if (param.THD3_ChannelIndex != BackupParam.THD3_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD3_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD3_ChannelIndex));
			}
			if (param.THD3_Min != BackupParam.THD3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD3_Min, LanguageManager.Instance["ChangedTo"], param.THD3_Min);
			}
			if (param.THD3_Max != BackupParam.THD3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD3_Max, LanguageManager.Instance["ChangedTo"], param.THD3_Max);
			}
			if (param.THD3_ChannelON != BackupParam.THD3_ChannelON)
			{
				if (param.THD3_ChannelON)
				{
					ModifyLog += string.Format("{0} [({1}3) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}3) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			if (param.THD4_ChannelIndex != BackupParam.THD4_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}4) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD4_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD4_ChannelIndex));
			}
			if (param.THD4_Min != BackupParam.THD4_Min)
			{
				ModifyLog += string.Format("{0} [({1}4) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD4_Min, LanguageManager.Instance["ChangedTo"], param.THD4_Min);
			}
			if (param.THD4_Max != BackupParam.THD4_Max)
			{
				ModifyLog += string.Format("{0} [({1}4) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD4_Max, LanguageManager.Instance["ChangedTo"], param.THD4_Max);
			}
			if (param.THD4_ChannelON != BackupParam.THD4_ChannelON)
			{
				if (param.THD4_ChannelON)
				{
					ModifyLog += string.Format("{0} [({1}4) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}4) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start1_Index, (int)BackupParam.DE_Start1_Index, param.DE_Start1_Count, BackupParam.DE_Start1_Count, 1, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End1_Index, (int)BackupParam.DE_End1_Index, param.DE_End1_Count, BackupParam.DE_End1_Count, 1, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start2_Index, (int)BackupParam.DE_Start2_Index, param.DE_Start2_Count, BackupParam.DE_Start2_Count, 2, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End2_Index, (int)BackupParam.DE_End2_Index, param.DE_End2_Count, BackupParam.DE_End2_Count, 2, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start3_Index, (int)BackupParam.DE_Start3_Index, param.DE_Start3_Count, BackupParam.DE_Start3_Count, 3, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End3_Index, (int)BackupParam.DE_End3_Index, param.DE_End3_Count, BackupParam.DE_End3_Count, 3, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start4_Index, (int)BackupParam.DE_Start4_Index, param.DE_Start4_Count, BackupParam.DE_Start4_Count, 4, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End4_Index, (int)BackupParam.DE_End4_Index, param.DE_End4_Count, BackupParam.DE_End4_Count, 4, 2, text4);
			GetTrackModifyText(text4, param);
			GetMaskModifyText(text4, param);
			if (param.BlobAnalysisSelect != BackupParam.BlobAnalysisSelect)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule"], modifyLogFun.GetBlobAnalysisText(BackupParam.BlobAnalysisSelect), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobAnalysisText(param.BlobAnalysisSelect));
			}
			if (param.InspRuleAND != BackupParam.InspRuleAND)
			{
				if (param.InspRuleAND)
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["AadOrRule"], "OR", LanguageManager.Instance["ChangedTo"], "AND");
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["AadOrRule"], "AND", LanguageManager.Instance["ChangedTo"], "OR");
				}
			}
			if (param.InspRule1_Index != BackupParam.InspRule1_Index)
			{
				ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule1_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule1_Index));
			}
			if (param.InspRule1_Min != BackupParam.InspRule1_Min)
			{
				ModifyLog += string.Format("{0} [({1}1-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule1_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule1_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule1_Min));
			}
			if (param.InspRule1_Max != BackupParam.InspRule1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule1_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule1_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule1_Max));
			}
			if (param.InspRule2_Index != BackupParam.InspRule2_Index)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule2_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule2_Index));
			}
			if (param.InspRule2_Min != BackupParam.InspRule2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule2_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule2_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule2_Min));
			}
			if (param.InspRule2_Max != BackupParam.InspRule2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule2_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule2_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule2_Max));
			}
			if (param.InspRule3_Index != BackupParam.InspRule3_Index)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule3_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule3_Index));
			}
			if (param.InspRule3_Min != BackupParam.InspRule3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule3_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule3_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule3_Min));
			}
			if (param.InspRule3_Max != BackupParam.InspRule3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule3_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule3_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule3_Max));
			}
			if (param.InspRule4_Index != BackupParam.InspRule4_Index)
			{
				ModifyLog += string.Format("{0} [({1}4) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule4_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule4_Index));
			}
			if (param.InspRule4_Min != BackupParam.InspRule4_Min)
			{
				ModifyLog += string.Format("{0} [({1}4-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule4_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule4_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule4_Min));
			}
			if (param.InspRule4_Max != BackupParam.InspRule4_Max)
			{
				ModifyLog += string.Format("{0} [({1}4-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule4_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule4_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.InspRule4_Max));
			}
			if (param.ErrorBin != BackupParam.ErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)BackupParam.ErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)param.ErrorBin));
			}
			if (param.EnableSelectFace != BackupParam.EnableSelectFace)
			{
				if (!param.EnableSelectFace)
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
				}
			}
			if (param.FaceIndex != BackupParam.FaceIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectFaceTool"], modifyLogFun.GetSelectFaceText(BackupParam.FaceIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetSelectFaceText(param.FaceIndex));
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
			string text5 = "BW10-BinaryWindow-WriteModifyLog()-";
			text5 += ex.ToString();
			CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text5, "Log寫入錯誤");
			}
		}
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
	}

	private void MainW_ContentRendered(object sender, EventArgs e)
	{
		exp_THD4.Visibility = Visibility.Visible;
	}

	private void Btn_QuickAIColor_Click(object sender, RoutedEventArgs e)
	{
		if (grd_QuickAIColor.Width > 100.0)
		{
			grd_QuickAIColor.Width = 30.0;
		}
		else
		{
			grd_QuickAIColor.Width = 230.0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_QuickAIColor_R_Click(object sender, RoutedEventArgs e)
	{
		string uid = ((Button)sender).Uid;
		if (!(uid == "0") && !(uid == "1") && !(uid == "2") && !(uid == "3") && !(uid == "4"))
		{
			_ = uid == "5";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OTSU_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = cb_Channel_THD1.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect, 0);
			Mat mat = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
			double num = Cv2.Threshold(dst: mat.Clone(), src: mat, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
			if (OTSU_White)
			{
				rs_THD1.LowerValue = num;
				rs_THD1.UpperValue = 255.0;
			}
			else
			{
				rs_THD1.LowerValue = 0.0;
				rs_THD1.UpperValue = num;
			}
			if (exp_THD2.IsExpanded)
			{
				selectedIndex = cb_Channel_THD2.SelectedIndex;
				Mat mat2 = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
				num = Cv2.Threshold(dst: mat2.Clone(), src: mat2, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
				if (OTSU_White)
				{
					rs_THD2.LowerValue = num;
					rs_THD2.UpperValue = 255.0;
				}
				else
				{
					rs_THD2.LowerValue = 0.0;
					rs_THD2.UpperValue = num;
				}
			}
			if (exp_THD3.IsExpanded)
			{
				selectedIndex = cb_Channel_THD3.SelectedIndex;
				Mat mat3 = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
				num = Cv2.Threshold(dst: mat3.Clone(), src: mat3, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
				if (OTSU_White)
				{
					rs_THD3.LowerValue = num;
					rs_THD3.UpperValue = 255.0;
				}
				else
				{
					rs_THD3.LowerValue = 0.0;
					rs_THD3.UpperValue = num;
				}
			}
			if (exp_THD4.IsExpanded)
			{
				selectedIndex = cb_Channel_THD4.SelectedIndex;
				Mat mat4 = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
				num = Cv2.Threshold(dst: mat4.Clone(), src: mat4, thresh: 0.0, maxval: 255.0, type: ThresholdTypes.Otsu);
				if (OTSU_White)
				{
					rs_THD4.LowerValue = num;
					rs_THD4.UpperValue = 255.0;
				}
				else
				{
					rs_THD4.LowerValue = 0.0;
					rs_THD4.UpperValue = num;
				}
			}
			OTSU_White = !OTSU_White;
			DoThdImage();
		}
		catch (Exception ex)
		{
			string text = "BW10-BinaryWindow-Btn_OTSUL_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetMaskModifyText(string Title, INSP_BINARYTOOL_PARAM param)
	{
		if (param.MaskSelectIndex != BackupParam.MaskSelectIndex)
		{
			ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["UseMask"], cb_MaskSelect.Items[BackupParam.MaskSelectIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_MaskSelect.Items[param.MaskSelectIndex].ToString());
		}
		if (param.UseMaskAsInspArea != BackupParam.UseMaskAsInspArea)
		{
			if (param.UseMaskAsInspArea)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["UseMaskInsp"], LanguageManager.Instance["Close"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
			}
			else
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["UseMaskInsp"], LanguageManager.Instance["Open"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
			}
		}
		if (param.UseMatchTextMask1 != BackupParam.UseMatchTextMask1)
		{
			ModifyLog += string.Format("{0} [({1})[1] {2} {3} {4}]  \n", Title, LanguageManager.Instance["UseMatchToolMask"], cb_MatchMaskSelect1.Items[BackupParam.UseMatchTextMask1].ToString(), LanguageManager.Instance["ChangedTo"], cb_MatchMaskSelect1.Items[param.UseMatchTextMask1].ToString());
		}
		if (param.UseMatchTextMask2 != BackupParam.UseMatchTextMask2)
		{
			ModifyLog += string.Format("{0} [({1})[2] {2} {3} {4}]  \n", Title, LanguageManager.Instance["UseMatchToolMask"], cb_MatchMaskSelect2.Items[BackupParam.UseMatchTextMask2].ToString(), LanguageManager.Instance["ChangedTo"], cb_MatchMaskSelect2.Items[param.UseMatchTextMask2].ToString());
		}
		if (param.UseMatchTextMask3 != BackupParam.UseMatchTextMask3)
		{
			ModifyLog += string.Format("{0} [({1})[3] {2} {3} {4}]  \n", Title, LanguageManager.Instance["UseMatchToolMask"], cb_MatchMaskSelect3.Items[BackupParam.UseMatchTextMask3].ToString(), LanguageManager.Instance["ChangedTo"], cb_MatchMaskSelect3.Items[param.UseMatchTextMask3].ToString());
		}
	}

	private void Chk_EnableSoftCorrection_Click(object sender, RoutedEventArgs e)
	{
		inspection.paramFile.BinaryParam[TrackIndex].EnableSoftCorrection = chk_EnableSoftCorrection.IsChecked.Value;
		CopyValueToUI();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTrackModifyText(string Title, INSP_BINARYTOOL_PARAM param)
	{
		if (param.Tracker1UseAutoSize != BackupParam.Tracker1UseAutoSize)
		{
			if (param.Tracker1UseAutoSize)
			{
				ModifyLog += string.Format("{0} [(A{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["AutoFit"]);
			}
			else
			{
				ModifyLog += string.Format("{0} [(A{1}) {2} {3} {4}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["AutoFit"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Alignment"]);
			}
		}
		if (param.Tracker1AligmentXIndex != BackupParam.Tracker1AligmentXIndex)
		{
			ModifyLog += string.Format("{0} [(A{1}-X{2}) {3} {4} {5}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], cb_Track1AlignWayX.Items[BackupParam.Tracker1AligmentXIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_Track1AlignWayX.Items[param.Tracker1AligmentXIndex].ToString());
		}
		if (param.Tracker1AligmentYIndex != BackupParam.Tracker1AligmentYIndex)
		{
			ModifyLog += string.Format("{0} [(A{1}-Y{2}) {3} {4} {5}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], cb_Track1AlignWayY.Items[BackupParam.Tracker1AligmentYIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_Track1AlignWayY.Items[param.Tracker1AligmentYIndex].ToString());
		}
		if (param.AutoSize1_SelectIndex != BackupParam.AutoSize1_SelectIndex)
		{
			ModifyLog += string.Format("{0} [(A{1}-{2}) {3} {4} {5}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["AutoFit"], cb_Track1AutoSize.Items[BackupParam.AutoSize1_SelectIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_Track1AutoSize.Items[param.AutoSize1_SelectIndex].ToString());
		}
		if (param.Track1_L != BackupParam.Track1_L || param.Track1_R != BackupParam.Track1_R || param.Track1_T != BackupParam.Track1_T || param.Track1_B != BackupParam.Track1_B)
		{
			ModifyLog += string.Format("{0} [(A{1}) L:{2} R:{3} T:{4} B:{5} {6} L:{7} R:{8} T:{9} B:{10}] \n", Title, LanguageManager.Instance["TrackPos"], BackupParam.Track1_L, BackupParam.Track1_R, BackupParam.Track1_T, BackupParam.Track1_B, LanguageManager.Instance["ChangedTo"], param.Track1_L, param.Track1_R, param.Track1_T, param.Track1_B);
		}
		if (param.SearchRect2ON != BackupParam.SearchRect2ON)
		{
			if (param.SearchRect2ON)
			{
				ModifyLog += string.Format("{0} [(B{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Close"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
			}
			else
			{
				ModifyLog += string.Format("{0} [(B{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Open"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
			}
		}
		if (param.Tracker2UseAutoSize != BackupParam.Tracker2UseAutoSize)
		{
			if (param.Tracker2UseAutoSize)
			{
				ModifyLog += string.Format("{0} [(B{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["AutoFit"]);
			}
			else
			{
				ModifyLog += string.Format("{0} [(B{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["AutoFit"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Alignment"]);
			}
		}
		if (param.Tracker2AligmentXIndex != BackupParam.Tracker2AligmentXIndex)
		{
			ModifyLog += string.Format("{0} [(B{1}-X{2}) {3} {4} {5}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], cb_Track1AlignWayX.Items[BackupParam.Tracker2AligmentXIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_Track1AlignWayX.Items[param.Tracker2AligmentXIndex].ToString());
		}
		if (param.Tracker2AligmentYIndex != BackupParam.Tracker2AligmentYIndex)
		{
			ModifyLog += string.Format("{0} [(B{1}-Y{2}) {3} {4} {5}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], cb_Track1AlignWayY.Items[BackupParam.Tracker2AligmentYIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_Track1AlignWayY.Items[param.Tracker2AligmentYIndex].ToString());
		}
		if (param.AutoSize1_SelectIndex != BackupParam.AutoSize1_SelectIndex)
		{
			ModifyLog += string.Format("{0} [(B{1}-{2}) {3} {4} {5}] \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["AutoFit"], cb_Track1AutoSize.Items[BackupParam.AutoSize1_SelectIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_Track1AutoSize.Items[param.AutoSize1_SelectIndex].ToString());
		}
		if (param.Track2_L != BackupParam.Track2_L || param.Track2_R != BackupParam.Track2_R || param.Track2_T != BackupParam.Track2_T || param.Track2_B != BackupParam.Track2_B)
		{
			ModifyLog += string.Format("{0} [(B{1}) L:{2} R:{3} T:{4} B:{5} {6} L:{7} R:{8} T:{9} B:{10}] \n", Title, LanguageManager.Instance["TrackPos"], BackupParam.Track2_L, BackupParam.Track2_R, BackupParam.Track2_T, BackupParam.Track2_B, LanguageManager.Instance["ChangedTo"], param.Track2_L, param.Track2_R, param.Track2_T, param.Track2_B);
		}
	}

	private void Chk_Thd_ORMode_Click(object sender, RoutedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			if (chk_Thd_ORMode.IsChecked == true)
			{
				grd_SIMP_THD_4.Background = Brushes.LightGoldenrodYellow;
			}
			else
			{
				grd_SIMP_THD_4.Background = Brushes.White;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_BinaryCompare_Click(object sender, RoutedEventArgs e)
	{
		ShowBinaryCompare = !ShowBinaryCompare;
		if (ShowBinaryCompare)
		{
			btn_BinaryCompare.Background = Brushes.Yellow;
			txt_BinaryCompareMode.Visibility = Visibility.Visible;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_SearchRect2.Visibility = Visibility.Hidden;
			bor_LocateRect.Visibility = Visibility.Hidden;
			Line line = line_X1;
			Line line2 = line_X2;
			Line line3 = line_Y1;
			Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
			Visibility visibility3 = (line3.Visibility = visibility);
			Visibility visibility5 = (line2.Visibility = visibility3);
			line.Visibility = visibility5;
			Rectangle rectangle = rt_ReferenceRect1;
			visibility5 = (rt_ReferenceRect2.Visibility = Visibility.Hidden);
			rectangle.Visibility = visibility5;
			Rectangle rectangle2 = rt_Mask1;
			visibility5 = (rt_Mask2.Visibility = Visibility.Hidden);
			rectangle2.Visibility = visibility5;
			WriteUIValueToParam();
			RunShowBinaryCompare();
			txt_AllResult.Text = "快速調整模式";
			return;
		}
		btn_BinaryCompare.Background = Brushes.White;
		txt_BinaryCompareMode.Visibility = Visibility.Hidden;
		bor_SearchRect1.Visibility = Visibility.Visible;
		if (ts_Tracker2ON.IsChecked == true)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
		}
		bor_LocateRect.Visibility = Visibility.Visible;
		if (rad_Tracker1AlignSelect.IsChecked == true)
		{
			Line line4 = line_X1;
			Visibility visibility5 = (line_Y1.Visibility = Visibility.Visible);
			line4.Visibility = visibility5;
		}
		else
		{
			rt_ReferenceRect1.Visibility = Visibility.Visible;
		}
		if (ts_Tracker2ON.IsChecked == true)
		{
			if (rad_Tracker2AlignSelect.IsChecked == true)
			{
				Line line5 = line_X2;
				Visibility visibility5 = (line_Y2.Visibility = Visibility.Visible);
				line5.Visibility = visibility5;
			}
			else
			{
				rt_ReferenceRect2.Visibility = Visibility.Visible;
			}
		}
		if (cb_MaskSelect.SelectedIndex > 0)
		{
			rt_Mask1.Visibility = Visibility.Visible;
			if (inspection.paramFile.MaskParam[cb_MaskSelect.SelectedIndex - 1].SearchRect2ON)
			{
				rt_Mask2.Visibility = Visibility.Visible;
			}
		}
		DoThdImage();
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-BinaryWindow-GetComDigitsStr()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return "0";
		}
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-BinaryWindow-ConvertComDigits()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return -1.0;
		}
	}

	private int GetSoftShift(int channel)
	{
		int num = 0;
		return channel switch
		{
			0 => inspection.paramFile.SoftShift_R, 
			1 => inspection.paramFile.SoftShift_G, 
			_ => inspection.paramFile.SoftShift_B, 
		};
	}

	private void GetSoftCorrectionShiftValue(int channel, ref int min, ref int max, ref int minShift, ref int maxShfit)
	{
		int num = min;
		int num2 = max;
		switch (channel)
		{
		case 0:
			if (min != 0)
			{
				min += inspection.paramFile.SoftShift_R;
			}
			if (max != 255)
			{
				max += inspection.paramFile.SoftShift_R;
			}
			break;
		case 1:
			if (min != 0)
			{
				min += inspection.paramFile.SoftShift_G;
			}
			if (max != 255)
			{
				max += inspection.paramFile.SoftShift_G;
			}
			break;
		default:
			if (min != 0)
			{
				min += inspection.paramFile.SoftShift_B;
			}
			if (max != 255)
			{
				max += inspection.paramFile.SoftShift_B;
			}
			break;
		}
		if (min < 0)
		{
			min = 0;
		}
		if (min > 254)
		{
			min = 254;
		}
		if (max < 1)
		{
			max = 1;
		}
		if (max > 255)
		{
			max = 255;
		}
		minShift = num - min;
		maxShfit = num2 - max;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/binarywindow.xaml", UriKind.Relative);
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
			MainW = (BinaryWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			MainW.Drop += MainW_Drop;
			MainW.ContentRendered += MainW_ContentRendered;
			break;
		case 2:
			grd_SIMP_Setup = (Grid)target;
			break;
		case 3:
			cb_BinaryErrorBin = (ComboBox)target;
			break;
		case 4:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 5:
			rt_Result = (Rectangle)target;
			break;
		case 6:
			tab_BlobAnalysis = (TabControl)target;
			tab_BlobAnalysis.SelectionChanged += Tab_BlobAnalysis_SelectionChanged;
			break;
		case 7:
			tb_MaxBlob = (TabItem)target;
			break;
		case 8:
			txt_InspRule3Min = (TextBox)target;
			txt_InspRule3Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 9:
			txt_InspRule3Max = (TextBox)target;
			txt_InspRule3Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 10:
			txt_InspRule4Min = (TextBox)target;
			txt_InspRule4Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 11:
			txt_InspRule4Max = (TextBox)target;
			txt_InspRule4Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 12:
			txt_InspRule13Result = (TextBox)target;
			break;
		case 13:
			txt_InspRule14Result = (TextBox)target;
			break;
		case 14:
			txt_InspRule1Min = (TextBox)target;
			txt_InspRule1Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 15:
			txt_InspRule1Max = (TextBox)target;
			txt_InspRule1Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 16:
			txt_InspRule2Min = (TextBox)target;
			txt_InspRule2Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 17:
			txt_InspRule2Max = (TextBox)target;
			txt_InspRule2Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 18:
			txt_InspRule11Result = (TextBox)target;
			break;
		case 19:
			txt_InspRule12Result = (TextBox)target;
			break;
		case 20:
			cb_InspRule1 = (ComboBox)target;
			break;
		case 21:
			cb_InspRule2 = (ComboBox)target;
			break;
		case 22:
			cb_InspRule3 = (ComboBox)target;
			break;
		case 23:
			cb_InspRule4 = (ComboBox)target;
			break;
		case 24:
			txt_InspRule23Result = (TextBox)target;
			break;
		case 25:
			txt_InspRule24Result = (TextBox)target;
			break;
		case 26:
			txt_InspRule21Result = (TextBox)target;
			break;
		case 27:
			txt_InspRule22Result = (TextBox)target;
			break;
		case 28:
			lab_Tracker1Result = (Label)target;
			break;
		case 29:
			lab_Tracker2Result = (Label)target;
			break;
		case 30:
			rad_RuleAND = (RadioButton)target;
			break;
		case 31:
			rad_RuleOR = (RadioButton)target;
			break;
		case 32:
			btn_Chart = (Button)target;
			btn_Chart.Click += Btn_Chart_Click;
			break;
		case 33:
			btn_Chart_Copy = (Button)target;
			btn_Chart_Copy.Click += Btn_Chart_Click;
			break;
		case 34:
			btn_Chart_Copy1 = (Button)target;
			btn_Chart_Copy1.Click += Btn_Chart_Click;
			break;
		case 35:
			btn_Chart_Copy2 = (Button)target;
			btn_Chart_Copy2.Click += Btn_Chart_Click;
			break;
		case 36:
			tb_BlobLine = (TabItem)target;
			break;
		case 37:
			___沒有名稱_ = (Grid)target;
			break;
		case 38:
			txt_BlobLineAreaMin = (TextBox)target;
			break;
		case 39:
			txt_BlobLineAreaMax = (TextBox)target;
			txt_BlobLineAreaMax.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 40:
			txt_BlobLineMin = (TextBox)target;
			txt_BlobLineMin.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 41:
			txt_OKLineResultA = (TextBox)target;
			break;
		case 42:
			txt_BlobLineHeightMin = (TextBox)target;
			txt_BlobLineHeightMin.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 43:
			txt_BlobLineHeightMax = (TextBox)target;
			txt_BlobLineHeightMax.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 44:
			cb_BlobLineSort = (ComboBox)target;
			break;
		case 45:
			txt_OKLineResultB = (TextBox)target;
			break;
		case 46:
			txt_BlobLineWidthMin = (TextBox)target;
			txt_BlobLineWidthMin.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 47:
			txt_BlobLineWidthMax = (TextBox)target;
			txt_BlobLineWidthMax.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 48:
			txt_BlobLineMax = (TextBox)target;
			txt_BlobLineMax.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 49:
			txt_BlobLineNum = (TextBox)target;
			txt_BlobLineNum.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 50:
			lab_Tracker1Result2 = (Label)target;
			break;
		case 51:
			lab_Tracker2Result2 = (Label)target;
			break;
		case 52:
			cb_BlobValueUsePixelValue = (CheckBox)target;
			break;
		case 53:
			tb_MultiBlob = (TabItem)target;
			break;
		case 54:
			txt_MB_InspRule3Min = (TextBox)target;
			txt_MB_InspRule3Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 55:
			txt_MB_InspRule3Max = (TextBox)target;
			txt_MB_InspRule3Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 56:
			txt_MB_InspRule4Min = (TextBox)target;
			txt_MB_InspRule4Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 57:
			txt_MB_InspRule4Max = (TextBox)target;
			txt_MB_InspRule4Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 58:
			txt_MB_InspRule1Min = (TextBox)target;
			txt_MB_InspRule1Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 59:
			txt_MB_InspRule1Max = (TextBox)target;
			txt_MB_InspRule1Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 60:
			txt_MB_InspRule2Min = (TextBox)target;
			txt_MB_InspRule2Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 61:
			txt_MB_InspRule2Max = (TextBox)target;
			txt_MB_InspRule2Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 62:
			cb_MB_InspRule1 = (ComboBox)target;
			break;
		case 63:
			cb_MB_InspRule2 = (ComboBox)target;
			break;
		case 64:
			cb_MB_InspRule3 = (ComboBox)target;
			break;
		case 65:
			cb_MB_InspRule4 = (ComboBox)target;
			break;
		case 66:
			rad_MB_RuleAND = (RadioButton)target;
			break;
		case 67:
			rad_MB_RuleOR = (RadioButton)target;
			break;
		case 68:
			cb_MB_BlobSort = (ComboBox)target;
			break;
		case 69:
			txt_MB_BlobNum = (TextBox)target;
			txt_MB_BlobNum.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 70:
			txt_MB_NGCount = (TextBox)target;
			txt_MB_NGCount.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 71:
			tb_LineWidth = (TabItem)target;
			break;
		case 72:
			txt_LineWidthMinArea = (TextBox)target;
			txt_LineWidthMinArea.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 73:
			txt_LineWidthMinW = (TextBox)target;
			txt_LineWidthMinW.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 74:
			txt_LineWidthMinH = (TextBox)target;
			txt_LineWidthMinH.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 75:
			cb_LineWidthA = (CheckBox)target;
			break;
		case 76:
			rt_LineWidthAA = (Rectangle)target;
			break;
		case 77:
			txt_AllowLineWidth = (TextBox)target;
			txt_AllowLineWidth.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 78:
			txt_AllowErrorW = (TextBox)target;
			txt_AllowErrorW.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 79:
			cb_LineWidthW = (CheckBox)target;
			break;
		case 80:
			rt_LineWidthWA = (Rectangle)target;
			break;
		case 81:
			txt_AllowLineHeight = (TextBox)target;
			txt_AllowLineHeight.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 82:
			txt_AllowErrorH = (TextBox)target;
			txt_AllowErrorH.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 83:
			cb_LineWidthH = (CheckBox)target;
			break;
		case 84:
			rt_LineWidthHA = (Rectangle)target;
			break;
		case 85:
			txt_AllowErrorWResultA = (TextBox)target;
			break;
		case 86:
			txt_AllowErrorHResultA = (TextBox)target;
			break;
		case 87:
			txt_AllowErrorWResultB = (TextBox)target;
			break;
		case 88:
			txt_AllowErrorHResultB = (TextBox)target;
			break;
		case 89:
			rt_LineWidthWB = (Rectangle)target;
			break;
		case 90:
			rt_LineWidthHB = (Rectangle)target;
			break;
		case 91:
			rt_LineWidthAB = (Rectangle)target;
			break;
		case 92:
			txt_AllowLineWidthError = (TextBox)target;
			txt_AllowLineWidthError.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 93:
			txt_AllowLineErrorResultA = (TextBox)target;
			break;
		case 94:
			txt_AllowLineErrorResultB = (TextBox)target;
			break;
		case 95:
			grd_SelectFace = (Grid)target;
			break;
		case 96:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 97:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 98:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 99:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 100:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 101:
			bor_Image = (Border)target;
			break;
		case 102:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			cnv_Image.PreviewMouseRightButtonDown += Cnv_Image_PreviewMouseRightButtonDown;
			break;
		case 103:
			sfr = (ScaleTransform)target;
			break;
		case 104:
			line_X1 = (Line)target;
			break;
		case 105:
			line_Y1 = (Line)target;
			break;
		case 106:
			line_X2 = (Line)target;
			break;
		case 107:
			line_Y2 = (Line)target;
			break;
		case 108:
			bor_LocateRect = (Border)target;
			break;
		case 109:
			bor_SearchRect2 = (Border)target;
			bor_SearchRect2.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect2.GotFocus += Bor_SearchRect2_GotFocus;
			bor_SearchRect2.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 110:
			txt_Search2Label = (TextBlock)target;
			break;
		case 111:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect1.GotFocus += Bor_SearchRect1_GotFocus;
			bor_SearchRect1.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 112:
			txt_Search1Label = (TextBlock)target;
			break;
		case 113:
			rt_Mask1 = (Rectangle)target;
			break;
		case 114:
			rt_Mask2 = (Rectangle)target;
			break;
		case 115:
			rt_ReferenceRect1 = (Rectangle)target;
			break;
		case 116:
			rt_ReferenceRect2 = (Rectangle)target;
			break;
		case 117:
			lab_FaceIndex = (Label)target;
			break;
		case 118:
			Label_ResultError = (TextBox)target;
			break;
		case 119:
			Label_Result = (Label)target;
			break;
		case 120:
			txt_PreviewMode = (TextBox)target;
			break;
		case 121:
			txt_BinaryCompareMode = (TextBox)target;
			break;
		case 122:
			tab_Function = (TabControl)target;
			break;
		case 123:
			cb_TrackerSelect = (ComboBox)target;
			cb_TrackerSelect.SelectionChanged += Cb_TrackerSelect_SelectionChanged;
			break;
		case 124:
			scr_TrackerSwich = (ScrollBar)target;
			scr_TrackerSwich.SizeChanged += Scr_TrackerSwich_SizeChanged;
			scr_TrackerSwich.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 125:
			ts_TrackerON = (ToggleSwitch)target;
			ts_TrackerON.Click += Ts_TrackerON_Click;
			break;
		case 126:
			btn_Reverse = (Button)target;
			btn_Reverse.Click += Btn_Reverse_Click;
			break;
		case 127:
			btn_RecipeCopy = (Button)target;
			btn_RecipeCopy.Click += Btn_RecipeCopy_Click;
			break;
		case 128:
			btn_RecipePaste = (Button)target;
			btn_RecipePaste.Click += Btn_RecipePaste_Click;
			break;
		case 129:
			lab_TrackIndex = (Label)target;
			break;
		case 130:
			txt_TrackerName = (TextBox)target;
			break;
		case 131:
			lab_Pareto = (Label)target;
			break;
		case 132:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 133:
			lab_Time = (Label)target;
			break;
		case 134:
			exp_THD1 = (Expander)target;
			break;
		case 135:
			exp_THD2 = (Expander)target;
			break;
		case 136:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 137:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 138:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 139:
			cb_THD2_DEStart = (ComboBox)target;
			cb_THD2_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 140:
			cb_THD2_DEEnd = (ComboBox)target;
			cb_THD2_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 141:
			cb_THD2_DEEndCount = (ComboBox)target;
			cb_THD2_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 142:
			cb_THD2_DEStartCount = (ComboBox)target;
			cb_THD2_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 143:
			cb_Channel_THD2 = (ComboBox)target;
			break;
		case 144:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 145:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 146:
			btn_ChannelBinaryPreview2 = (Button)target;
			btn_ChannelBinaryPreview2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview2.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 147:
			btn_ChannelPreview2 = (Button)target;
			btn_ChannelPreview2.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 148:
			btn_HistogramPreview2 = (Button)target;
			btn_HistogramPreview2.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview2.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 149:
			btn_THD2Undo = (Button)target;
			btn_THD2Undo.Click += Btn_THD1Undo_Click;
			break;
		case 150:
			btn_SoftCorrectionTHD2 = (Button)target;
			break;
		case 151:
			lab_SoftCorrectionShiftTHD2 = (Label)target;
			break;
		case 152:
			exp_THD3 = (Expander)target;
			break;
		case 153:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 154:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 155:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 156:
			cb_THD3_DEStart = (ComboBox)target;
			cb_THD3_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 157:
			cb_THD3_DEEnd = (ComboBox)target;
			cb_THD3_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 158:
			cb_THD3_DEEndCount = (ComboBox)target;
			cb_THD3_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 159:
			cb_THD3_DEStartCount = (ComboBox)target;
			cb_THD3_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 160:
			cb_Channel_THD3 = (ComboBox)target;
			break;
		case 161:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 162:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 163:
			btn_ChannelBinaryPreview3 = (Button)target;
			btn_ChannelBinaryPreview3.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview3.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 164:
			btn_ChannelPreview3 = (Button)target;
			btn_ChannelPreview3.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview3.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 165:
			btn_HistogramPreview3 = (Button)target;
			btn_HistogramPreview3.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview3.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 166:
			btn_THD3Undo = (Button)target;
			btn_THD3Undo.Click += Btn_THD1Undo_Click;
			break;
		case 167:
			btn_SoftCorrectionTHD3 = (Button)target;
			break;
		case 168:
			lab_SoftCorrectionShiftTHD3 = (Label)target;
			break;
		case 169:
			exp_THD4 = (Expander)target;
			break;
		case 170:
			grd_SIMP_THD_4 = (Grid)target;
			break;
		case 171:
			rs_THD4 = (RangeSlider)target;
			rs_THD4.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD4.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD4.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 172:
			cb_Channel_THD4 = (ComboBox)target;
			break;
		case 173:
			grd_SIMP_DilateErode4 = (Grid)target;
			break;
		case 174:
			cb_THD4_DEStart = (ComboBox)target;
			cb_THD4_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 175:
			cb_THD4_DEEnd = (ComboBox)target;
			cb_THD4_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 176:
			cb_THD4_DEEndCount = (ComboBox)target;
			cb_THD4_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 177:
			cb_THD4_DEStartCount = (ComboBox)target;
			cb_THD4_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 178:
			nu_Thd4LimitMin = (NumericUpDown)target;
			nu_Thd4LimitMin.ValueChanged += Nu_Thd4LimitMin_ValueChanged;
			break;
		case 179:
			nu_Thd4LimitMax = (NumericUpDown)target;
			nu_Thd4LimitMax.ValueChanged += Nu_Thd4LimitMax_ValueChanged;
			break;
		case 180:
			btn_ChannelBinaryPreview4 = (Button)target;
			btn_ChannelBinaryPreview4.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview4.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 181:
			btn_ChannelPreview4 = (Button)target;
			btn_ChannelPreview4.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview4.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 182:
			btn_HistogramPreview1_Copy = (Button)target;
			btn_HistogramPreview1_Copy.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview1_Copy.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 183:
			btn_THD4Undo = (Button)target;
			btn_THD4Undo.Click += Btn_THD1Undo_Click;
			break;
		case 184:
			btn_SoftCorrectionTHD4 = (Button)target;
			break;
		case 185:
			lab_SoftCorrectionShiftTHD4 = (Label)target;
			break;
		case 186:
			chk_Thd_ORMode = (CheckBox)target;
			chk_Thd_ORMode.Click += Chk_Thd_ORMode_Click;
			break;
		case 187:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 188:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 189:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 190:
			cb_THD1_DEStart = (ComboBox)target;
			cb_THD1_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 191:
			cb_THD1_DEEnd = (ComboBox)target;
			cb_THD1_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 192:
			cb_THD1_DEEndCount = (ComboBox)target;
			cb_THD1_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 193:
			cb_THD1_DEStartCount = (ComboBox)target;
			cb_THD1_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 194:
			chk_EnableSoftCorrection = (CheckBox)target;
			chk_EnableSoftCorrection.Click += Chk_EnableSoftCorrection_Click;
			break;
		case 195:
			cb_Channel_THD1 = (ComboBox)target;
			break;
		case 196:
			btn_ColorCopy = (Button)target;
			btn_ColorCopy.Click += Btn_ColorCopy_Click;
			break;
		case 197:
			btn_ColorPaste = (Button)target;
			btn_ColorPaste.Click += Btn_ColorPaste_Click;
			break;
		case 198:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 199:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 200:
			btn_ChannelBinaryPreview1 = (Button)target;
			btn_ChannelBinaryPreview1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview1.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 201:
			btn_ChannelPreview1 = (Button)target;
			btn_ChannelPreview1.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 202:
			btn_HistogramPreview1 = (Button)target;
			btn_HistogramPreview1.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview1.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 203:
			btn_THD1Undo = (Button)target;
			btn_THD1Undo.Click += Btn_THD1Undo_Click;
			break;
		case 204:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSU_Click;
			break;
		case 205:
			btn_SoftCorrectionTHD1 = (Button)target;
			break;
		case 206:
			lab_SoftCorrectionShiftTHD1 = (Label)target;
			break;
		case 207:
			grd_QuickAIColor = (Grid)target;
			break;
		case 208:
			btn_QuickAIColor_R = (Button)target;
			btn_QuickAIColor_R.Click += Btn_QuickAIColor_R_Click;
			break;
		case 209:
			btn_QuickAIColor_G = (Button)target;
			btn_QuickAIColor_G.Click += Btn_QuickAIColor_R_Click;
			break;
		case 210:
			btn_QuickAIColor_B = (Button)target;
			btn_QuickAIColor_B.Click += Btn_QuickAIColor_R_Click;
			break;
		case 211:
			btn_QuickAIColor = (Button)target;
			btn_QuickAIColor.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_QuickAIColor.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			btn_QuickAIColor.Click += Btn_QuickAIColor_Click;
			break;
		case 212:
			btn_QuickAIColor_Y = (Button)target;
			btn_QuickAIColor_Y.Click += Btn_QuickAIColor_R_Click;
			break;
		case 213:
			btn_QuickAIColor_P = (Button)target;
			btn_QuickAIColor_P.Click += Btn_QuickAIColor_R_Click;
			break;
		case 214:
			btn_QuickAIColor_LB = (Button)target;
			btn_QuickAIColor_LB.Click += Btn_QuickAIColor_R_Click;
			break;
		case 215:
			grd_SIMP_RefSetup = (Grid)target;
			break;
		case 216:
			cb_ColorReference = (ComboBox)target;
			cb_ColorReference.SelectionChanged += Cb_ColorReference_SelectionChanged;
			break;
		case 217:
			btn_ReferenceSave = (Button)target;
			btn_ReferenceSave.Click += Btn_ReferenceSave_Click;
			break;
		case 218:
			txt_ReferenceName = (TextBox)target;
			break;
		case 219:
			btn_ReferenceLock = (Button)target;
			btn_ReferenceLock.Click += Btn_ReferenceLock_Click;
			break;
		case 220:
			cb_Track1AlignWayX = (ComboBox)target;
			cb_Track1AlignWayX.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 221:
			rad_Tracker1AutoSize = (RadioButton)target;
			rad_Tracker1AutoSize.Click += Rad_Tracker1AutoSize_Click;
			break;
		case 222:
			cb_Track1AutoSize = (ComboBox)target;
			cb_Track1AutoSize.SelectionChanged += Cb_Track1AutoSize_SelectionChanged;
			break;
		case 223:
			rad_Tracker1AlignSelect = (RadioButton)target;
			rad_Tracker1AlignSelect.Click += Rad_Tracker1AlignSelect_Click;
			break;
		case 224:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 225:
			cb_Track1AlignWayY = (ComboBox)target;
			cb_Track1AlignWayY.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 226:
			btn_Tracker1SizeChange = (Button)target;
			btn_Tracker1SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 227:
			txt_Tracker1Pos = (Label)target;
			break;
		case 228:
			chk_TrackErrorSetNG = (CheckBox)target;
			chk_TrackErrorSetNG.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 229:
			grd_Track2Setup = (Grid)target;
			break;
		case 230:
			cb_Track2AlignWayX = (ComboBox)target;
			cb_Track2AlignWayX.SelectionChanged += Cb_Track2AlignWayX_SelectionChanged;
			break;
		case 231:
			rad_Tracker2AutoSize = (RadioButton)target;
			rad_Tracker2AutoSize.Click += Rad_Tracker2AutoSize_Click;
			break;
		case 232:
			cb_Track2AutoSize = (ComboBox)target;
			cb_Track2AutoSize.SelectionChanged += Cb_Track2AutoSize_SelectionChanged;
			break;
		case 233:
			rad_Tracker2AlignSelect = (RadioButton)target;
			rad_Tracker2AlignSelect.Click += Rad_Tracker2AlignSelect_Click;
			break;
		case 234:
			cb_Track2AlignWayY = (ComboBox)target;
			cb_Track2AlignWayY.SelectionChanged += Cb_Track2AlignWayX_SelectionChanged;
			break;
		case 235:
			btn_TrackerReset_Copy = (Button)target;
			btn_TrackerReset_Copy.Click += Btn_TrackerReset_Click;
			break;
		case 236:
			btn_Tracker2SizeChange = (Button)target;
			btn_Tracker2SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 237:
			txt_Tracker2Pos = (Label)target;
			break;
		case 238:
			ts_Tracker2ON = (CheckBox)target;
			ts_Tracker2ON.Click += Ts_Tracker2ON_Click;
			break;
		case 239:
			grd_Mask = (Grid)target;
			break;
		case 240:
			cb_MaskSelect = (ComboBox)target;
			cb_MaskSelect.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 241:
			btn_MaskEdit = (Button)target;
			btn_MaskEdit.Click += Btn_MaskEdit_Click;
			break;
		case 242:
			chk_UseMaskAsInspArea = (CheckBox)target;
			chk_UseMaskAsInspArea.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 243:
			cb_MatchMaskSelect1 = (ComboBox)target;
			cb_MatchMaskSelect1.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 244:
			cb_MatchMaskSelect2 = (ComboBox)target;
			cb_MatchMaskSelect2.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 245:
			cb_MatchMaskSelect3 = (ComboBox)target;
			cb_MatchMaskSelect3.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 246:
			cb_MatchText_DEStart1 = (ComboBox)target;
			cb_MatchText_DEStart1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 247:
			cb_MatchText_DEStartCount1 = (ComboBox)target;
			cb_MatchText_DEStartCount1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 248:
			cb_MatchText_DEStart2 = (ComboBox)target;
			cb_MatchText_DEStart2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 249:
			cb_MatchText_DEStartCount2 = (ComboBox)target;
			cb_MatchText_DEStartCount2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 250:
			cb_MatchText_DEStart3 = (ComboBox)target;
			cb_MatchText_DEStart3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 251:
			cb_MatchText_DEStartCount3 = (ComboBox)target;
			cb_MatchText_DEStartCount3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 252:
			cb_Mask_DEStart = (ComboBox)target;
			cb_Mask_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 253:
			cb_Mask_DEStartCount = (ComboBox)target;
			cb_Mask_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 254:
			btn_MaskPreView = (Button)target;
			btn_MaskPreView.PreviewMouseLeftButtonDown += Btn_MaskPreView_PreviewMouseLeftButtonDown;
			btn_MaskPreView.PreviewMouseLeftButtonUp += Btn_MaskPreView_PreviewMouseLeftButtonUp;
			break;
		case 255:
			chk_UseFontMask = (CheckBox)target;
			chk_UseFontMask.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 256:
			grd_AdvFunction = (Grid)target;
			break;
		case 257:
			cb_JudgeWithAdvMeasureA = (ComboBox)target;
			cb_JudgeWithAdvMeasureA.SelectionChanged += Cb_ColorReference_SelectionChanged;
			break;
		case 258:
			chk_EnableJudgeWithAdvMeasure = (CheckBox)target;
			chk_EnableJudgeWithAdvMeasure.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 259:
			cb_JudgeWithAdvMeasureB = (ComboBox)target;
			cb_JudgeWithAdvMeasureB.SelectionChanged += Cb_ColorReference_SelectionChanged;
			break;
		case 260:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 261:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 262:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 263:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 264:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 265:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 266:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 267:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 268:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 269:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 270:
			btn_ImageNum = (Button)target;
			break;
		case 271:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 272:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 273:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 274:
			btn_ImgAI = (Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 275:
			btn_ImgAIR = (Button)target;
			btn_ImgAIR.Click += Btn_ImgSrc_Click;
			break;
		case 276:
			btn_ImgAIG = (Button)target;
			btn_ImgAIG.Click += Btn_ImgSrc_Click;
			break;
		case 277:
			btn_ImgAIB = (Button)target;
			btn_ImgAIB.Click += Btn_ImgSrc_Click;
			break;
		case 278:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 279:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 280:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 281:
			lab_RValue = (Label)target;
			break;
		case 282:
			lab_GValue = (Label)target;
			break;
		case 283:
			lab_BValue = (Label)target;
			break;
		case 284:
			lab_HValue = (Label)target;
			break;
		case 285:
			lab_SValue = (Label)target;
			break;
		case 286:
			lab_VValue = (Label)target;
			break;
		case 287:
			lab_GrayValue = (Label)target;
			break;
		case 288:
			lab_XValue = (Label)target;
			break;
		case 289:
			lab_YValue = (Label)target;
			break;
		case 290:
			lab_C1Value = (Label)target;
			break;
		case 291:
			lab_C2Value = (Label)target;
			break;
		case 292:
			lab_C3Value = (Label)target;
			break;
		case 293:
			btn_ImgT1 = (Button)target;
			btn_ImgT1.Click += Btn_ImgT1_Click;
			break;
		case 294:
			btn_ImgT2 = (Button)target;
			btn_ImgT2.Click += Btn_ImgT1_Click;
			break;
		case 295:
			btn_ImgT3 = (Button)target;
			btn_ImgT3.Click += Btn_ImgT1_Click;
			break;
		case 296:
			lab_AIRValue = (Label)target;
			break;
		case 297:
			lab_AIGValue = (Label)target;
			break;
		case 298:
			lab_AIBValue = (Label)target;
			break;
		case 299:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 300:
			txt_AllResult = (TextBox)target;
			break;
		case 301:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 302:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 303:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 304:
			btn_TrackerShow = (Button)target;
			btn_TrackerShow.Click += Btn_TrackerShow_Click;
			break;
		case 305:
			btn_ReferenceShow = (Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 306:
			btn_PrevTracker = (Button)target;
			btn_PrevTracker.Click += Btn_PrevTracker;
			break;
		case 307:
			btn_NextTracker = (Button)target;
			btn_NextTracker.Click += Btn_NextTracker_Click;
			break;
		case 308:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 309:
			btn_PreviewSwitch = (Button)target;
			btn_PreviewSwitch.Click += Btn_PreviewSwitch_Click;
			break;
		case 310:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 311:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 312:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 313:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 314:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 315:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 316:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 317:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 318:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 319:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 320:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 321:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 322:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 323:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 324:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 325:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 326:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 327:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 328:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 329:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 330:
			btn_Tools21 = (Button)target;
			btn_Tools21.Click += Btn_Tools1_Click;
			break;
		case 331:
			btn_Tools22 = (Button)target;
			btn_Tools22.Click += Btn_Tools1_Click;
			break;
		case 332:
			btn_Tools23 = (Button)target;
			btn_Tools23.Click += Btn_Tools1_Click;
			break;
		case 333:
			btn_Tools24 = (Button)target;
			btn_Tools24.Click += Btn_Tools1_Click;
			break;
		case 334:
			btn_Tools25 = (Button)target;
			btn_Tools25.Click += Btn_Tools1_Click;
			break;
		case 335:
			btn_Tools26 = (Button)target;
			btn_Tools26.Click += Btn_Tools1_Click;
			break;
		case 336:
			btn_Tools27 = (Button)target;
			btn_Tools27.Click += Btn_Tools1_Click;
			break;
		case 337:
			btn_Tools28 = (Button)target;
			btn_Tools28.Click += Btn_Tools1_Click;
			break;
		case 338:
			btn_Tools29 = (Button)target;
			btn_Tools29.Click += Btn_Tools1_Click;
			break;
		case 339:
			btn_Tools30 = (Button)target;
			btn_Tools30.Click += Btn_Tools1_Click;
			break;
		case 340:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 341:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 342:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 343:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 344:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 345:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 346:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 347:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 348:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 349:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 350:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 351:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 352:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 353:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 354:
			grd_TrackerAdj = (Grid)target;
			break;
		case 355:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 356:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 357:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 358:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 359:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 360:
			lab_Tracker1 = (Label)target;
			break;
		case 361:
			lab_Tracker2 = (Label)target;
			break;
		case 362:
			btn_ResetA = (Button)target;
			btn_ResetA.Click += Btn_TrackerReset_Click;
			break;
		case 363:
			btn_ResetB = (Button)target;
			btn_ResetB.Click += Btn_TrackerReset_Click;
			break;
		case 364:
			grd_Histogram = (Grid)target;
			break;
		case 365:
			cnv_Histogram = (Canvas)target;
			break;
		case 366:
			btn_Fillet = (Button)target;
			btn_Fillet.Click += Btn_Fillet_Click;
			break;
		case 367:
			exp_TrackerFilllet = (Expander)target;
			exp_TrackerFilllet.Collapsed += Exp_TrackerFilllet_Collapsed;
			break;
		case 368:
			((Grid)target).PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 369:
			cb_T1Filllet0 = (ComboBox)target;
			cb_T1Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 370:
			cb_T1Filllet1 = (ComboBox)target;
			cb_T1Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 371:
			cb_T1Filllet2 = (ComboBox)target;
			cb_T1Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 372:
			cb_T1Filllet3 = (ComboBox)target;
			cb_T1Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 373:
			grd_Tracker2Filllet = (Grid)target;
			grd_Tracker2Filllet.PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 374:
			cb_T2Filllet0 = (ComboBox)target;
			cb_T2Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 375:
			cb_T2Filllet1 = (ComboBox)target;
			cb_T2Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 376:
			cb_T2Filllet2 = (ComboBox)target;
			cb_T2Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 377:
			cb_T2Filllet3 = (ComboBox)target;
			cb_T2Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 378:
			Label_Index = (Label)target;
			break;
		case 379:
			lab_LockMode = (TextBox)target;
			break;
		case 380:
			btn_BinaryCompare = (Button)target;
			btn_BinaryCompare.Click += btn_BinaryCompare_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
