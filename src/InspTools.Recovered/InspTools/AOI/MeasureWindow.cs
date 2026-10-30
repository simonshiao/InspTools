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
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using OpenCvSharp;

namespace InspTools.AOI;

public class MeasureWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private const int MaxThdTool = 2;

	private int LanguageType = 1;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool[] ColorDropper = new bool[4];

	private byte[] CurrentColor = new byte[13];

	private bool ShowMessage;

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect DefaultSearchRect1 = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect DefaultSearchRect2 = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect[] MaskRect = new OpenCvSharp.Rect[2];

	private int SelectFace;

	private int FaceIndex;

	private bool TrackerA_Changed = true;

	private int[] TrackIndex = new int[4];

	private int CurrentFunction;

	private bool ThdAPress;

	private bool ThdBPress;

	private INSP_MEASURETOOL_PARAM CopyParamW;

	private INSP_MEASURETOOL_PARAM CopyParamH;

	private INSP_MEASURETOOL_PARAM BackupParamW;

	private INSP_MEASURETOOL_PARAM BackupParamH;

	private bool CopyParamSetedW;

	private bool CopyParamSetedH;

	private bool TrackerAdjShow;

	private int CurrentTracker;

	private Stopwatch sw = new Stopwatch();

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat[] BinaryBlob = new Mat[2];

	private Mat ScanBinary = new Mat();

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private MeasureTool measureTool;

	private Button[] ToolsButton;

	private ToggleSwitch[] TrackON;

	private ComboBox[] TrackSelect;

	private TextBox[] TrackName;

	private RadioButton[,] RadScanDir1;

	private RadioButton[,] RadScanDir2;

	private ComboBox[] ErrorBin;

	private RangeSlider[,] THD_Sliders;

	private RangeSlider[,] THD_Sliders_R;

	private RangeSlider[,] THD_Sliders_G;

	private RangeSlider[,] THD_Sliders_B;

	private NumericUpDown[,] THD_TextLow;

	private NumericUpDown[,] THD_TextUp;

	private CheckBox[,] CHK_UseRGBChannel;

	private Grid[,] GRD_RGBThd;

	private ComboBox[,] THD_Channel;

	private ComboBox[,] THD_DEStart;

	private ComboBox[,] THD_DEEnd;

	private ComboBox[,] THD_DEStartCount;

	private ComboBox[,] THD_DEEndCount;

	private Label[,] LabTHD_RMin;

	private Label[,] LabTHD_RMax;

	private Label[,] LabTHD_GMin;

	private Label[,] LabTHD_GMax;

	private Label[,] LabTHD_BMin;

	private Label[,] LabTHD_BMax;

	private bool EnableModifyLog;

	private string ModifyLog = "";

	private string RecipeName = "";

	private string UserID = "123456";

	private bool PreviewMode;

	private User_Permissions User = User_Permissions.USER_Administrator;

	internal MeasureWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Label Label_Result;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Border bor_SearchRect2;

	internal TextBlock txt_Search1Label1;

	internal Rectangle bor_CorrectRect;

	internal Label lab_FaceIndex;

	internal TextBox Label_ResultError;

	internal TextBox txt_PreviewMode;

	internal TabControl tab_AdvMeasureFun;

	internal TabItem tab_W;

	internal Grid grd_MainW;

	internal ComboBox cb_TrackerSelect_W;

	internal TextBox txt_TrackerName_W;

	internal ScrollBar scr_TrackerSwich_W;

	internal ToggleSwitch ts_TrackerON_W;

	internal Button btn_ReverseW;

	internal Button btn_RecipeCopyW;

	internal Button btn_RecipePasteW;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChartW;

	internal Label lab_Time1;

	internal GroupBox gb_WTHD1;

	internal Grid grd_THD_W1;

	internal RangeSlider rs_ThdW1;

	internal Grid grd_SIMP_DilateErode4;

	internal ComboBox cb_ThdDEStartW1;

	internal ComboBox cb_ThdDEEndW1;

	internal ComboBox cb_ThdDEEndCountW1;

	internal ComboBox cb_ThdDEStartCountW1;

	internal ComboBox cb_ChannelW1;

	internal RadioButton rad_ScanL2RW1;

	internal RadioButton rad_ScanR2LW1;

	internal Button btn_ScanL2RW1;

	internal Button btn_ScanR2LW1;

	internal NumericUpDown nu_ThdW1LimitMin;

	internal NumericUpDown nu_ThdW1LimitMax;

	internal Grid grd_RGBThd_WA;

	internal RangeSlider rs_THD_WA_R;

	internal RangeSlider rs_THD_WA_G;

	internal RangeSlider rs_THD_WA_B;

	internal Label lab_THD_WA_R_Min;

	internal Label lab_THD_WA_R_Max;

	internal Label lab_THD_WA_G_Min;

	internal Label lab_THD_WA_G_Max;

	internal Label lab_THD_WA_B_Min;

	internal Label lab_THD_WA_B_Max;

	internal CheckBox chk_EnableRGBThd_WA;

	internal Button btn_ColorDropperW1;

	internal GroupBox gb_WTHD2;

	internal Grid grd_THD_W2;

	internal RangeSlider rs_ThdW2;

	internal Grid grd_SIMP_DilateErode1;

	internal ComboBox cb_ThdDEStartW2;

	internal ComboBox cb_ThdDEEndW2;

	internal ComboBox cb_ThdDEEndCountW2;

	internal ComboBox cb_ThdDEStartCountW2;

	internal ComboBox cb_ChannelW2;

	internal NumericUpDown nu_ThdW2LimitMin;

	internal NumericUpDown nu_ThdW2LimitMax;

	internal RadioButton rad_ScanL2RW2;

	internal RadioButton rad_ScanR2LW2;

	internal Button btn_ScanL2RW2;

	internal Button btn_ScanR2LW2;

	internal Grid grd_RGBThd_WB;

	internal RangeSlider rs_THD_WB_R;

	internal RangeSlider rs_THD_WB_G;

	internal RangeSlider rs_THD_WB_B;

	internal Label lab_THD_WB_R_Min;

	internal Label lab_THD_WB_R_Max;

	internal Label lab_THD_WB_G_Min;

	internal Label lab_THD_WB_G_Max;

	internal Label lab_THD_WB_B_Min;

	internal Label lab_THD_WB_B_Max;

	internal Button btn_ColorDropperW2;

	internal CheckBox chk_EnableRGBThd_WB;

	internal TabItem tab_H;

	internal Grid grd_MainH;

	internal ComboBox cb_TrackerSelect_H;

	internal TextBox txt_TrackerName_H;

	internal ScrollBar scr_TrackerSwich_H;

	internal ToggleSwitch ts_TrackerON_H;

	internal Button btn_ReverseH;

	internal Button btn_RecipeCopyH;

	internal Button btn_RecipePasteH;

	internal Label lab_Pareto_Copy;

	internal ComboBox cb_ParetoChartH;

	internal Label lab_Time2;

	internal GroupBox gb_HTHD1;

	internal Grid grd_THD_H1;

	internal RangeSlider rs_ThdH1;

	internal Grid grd_SIMP_DilateErode2;

	internal ComboBox cb_ThdDEStartH1;

	internal ComboBox cb_ThdDEEndH1;

	internal ComboBox cb_ThdDEEndCountH1;

	internal ComboBox cb_ThdDEStartCountH1;

	internal ComboBox cb_ChannelH1;

	internal NumericUpDown nu_ThdH1LimitMin;

	internal NumericUpDown nu_ThdH1LimitMax;

	internal RadioButton rad_ScanL2RH1;

	internal RadioButton rad_ScanR2LH1;

	internal Button btn_ScanL2RH1;

	internal Button btn_ScanR2LH1;

	internal Grid grd_RGBThd_HA;

	internal RangeSlider rs_THD_HA_R;

	internal RangeSlider rs_THD_HA_G;

	internal RangeSlider rs_THD_HA_B;

	internal Label lab_THD_HA_R_Min;

	internal Label lab_THD_HA_R_Max;

	internal Label lab_THD_HA_G_Min;

	internal Label lab_THD_HA_G_Max;

	internal Label lab_THD_HA_B_Min;

	internal Label lab_THD_HA_B_Max;

	internal Button btn_ColorDropperH1;

	internal CheckBox chk_EnableRGBThd_HA;

	internal GroupBox gb_HTHD2;

	internal Grid grd_THD_H2;

	internal RangeSlider rs_ThdH2;

	internal Grid grd_SIMP_DilateErode3;

	internal ComboBox cb_ThdDEStartH2;

	internal ComboBox cb_ThdDEEndH2;

	internal ComboBox cb_ThdDEEndCountH2;

	internal ComboBox cb_ThdDEStartCountH2;

	internal ComboBox cb_ChannelH2;

	internal NumericUpDown nu_ThdH2LimitMin;

	internal NumericUpDown nu_ThdH2LimitMax;

	internal RadioButton rad_ScanL2RH2;

	internal RadioButton rad_ScanR2LH2;

	internal Button btn_ScanL2RH2;

	internal Button btn_ScanR2LH2;

	internal Grid grd_RGBThd_HB;

	internal RangeSlider rs_THD_HB_R;

	internal RangeSlider rs_THD_HB_G;

	internal RangeSlider rs_THD_HB_B;

	internal Label lab_THD_HB_R_Min;

	internal Label lab_THD_HB_R_Max;

	internal Label lab_THD_HB_G_Min;

	internal Label lab_THD_HB_G_Max;

	internal Label lab_THD_HB_B_Min;

	internal Label lab_THD_HB_B_Max;

	internal Button btn_ColorDropperH2;

	internal CheckBox chk_EnableRGBThd_HB;

	internal TabItem tab_C;

	internal Button btn_CorrectTrackerReset;

	internal TextBox txt_CorrectWidth;

	internal TextBox txt_CorrectHeight;

	internal Label lab_unit1;

	internal Label lab_unit2;

	internal Button btn_CorrectSet;

	internal TextBox txt_PixelperUmW;

	internal Label lab_unit3;

	internal TextBox txt_PixelperUmH;

	internal Label lab_unit4;

	internal Rectangle rt_CorrectResult;

	internal RadioButton rad_um;

	internal RadioButton rad_mm;

	internal Button btn_CorrectReSet;

	internal CheckBox chk_UseFloat;

	internal TabItem tab_A;

	internal ComboBox cb_TrackerSelect_A;

	internal TextBox txt_TrackerName_A;

	internal ScrollBar scr_TrackerSwich_A;

	internal ToggleSwitch ts_TrackerON_A;

	internal Grid grd_ParamA;

	internal TextBox txt_AdvInspRule3Min;

	internal TextBox txt_AdvInspRule3Max;

	internal ComboBox cb_AdvErrorBin;

	internal Button btn_Test;

	internal TextBox txt_AdvInspRule3Result;

	internal TextBox txt_AdvInspRule1Min;

	internal TextBox txt_AdvInspRule1Max;

	internal TextBox txt_AdvInspRule2Min;

	internal TextBox txt_AdvInspRule2Max;

	internal TextBox txt_AdvInspRule1Result;

	internal TextBox txt_AdvInspRule2Result;

	internal ComboBox cb_AdvInspRule1;

	internal ComboBox cb_AdvInspRule2;

	internal ComboBox cb_AdvInspRule3;

	internal ComboBox cb_AdvSelect1W;

	internal ComboBox cb_AdvSelect1H;

	internal RadioButton rad_Select1UseWidth;

	internal RadioButton rad_Select1UseHeight;

	internal ComboBox cb_Select1ValueIndex;

	internal ComboBox cb_AdvSelect2W;

	internal ComboBox cb_AdvSelect2H;

	internal RadioButton rad_Select2UseWidth;

	internal RadioButton rad_Select2UseHeight;

	internal ComboBox cb_Select2ValueIndex;

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

	internal Label txt_Tracker1Pos;

	internal Button btn_ImgC1;

	internal Button btn_ImgC2;

	internal Button btn_ImgC3;

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

	internal Label txt_Tracker2Pos;

	internal Label lab_C1Value;

	internal Label lab_C2Value;

	internal Label lab_C3Value;

	internal Label lab_C4Value;

	internal Label lab_C5Value;

	internal Label lab_C6Value;

	internal TextBox txt_AllResult;

	internal Button btn_ReDraw;

	internal Grid grd_ParamWH;

	internal Grid grd_SIMP_Setup;

	internal ComboBox cb_ErrorBin;

	internal Button btn_Test1;

	internal ComboBox cb_Scan1Rule;

	internal TextBox txt_ContError;

	internal TextBox txt_MaxError;

	internal TextBox txt_ContErrorResult;

	internal TextBox txt_MaxErrorResult;

	internal Button btn_Tracker1Reset;

	internal Button btn_Tracker2Reset;

	internal RadioButton rad_UseAllPt;

	internal RadioButton rad_UseSinglePt;

	internal TextBox txt_LengthMin;

	internal TextBox txt_LengthMax;

	internal CheckBox chk_ShowScanEdge;

	internal CheckBox chk_ShowScanOK;

	internal CheckBox chk_ShowScanNG;

	internal ComboBox cb_Scan2Rule;

	internal TextBox txt_SinglePtLengthMin;

	internal TextBox txt_SinglePtLengthMax;

	internal TextBox txt_SinglePtLengthResult;

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal CheckBox chk_CheckEveryOne;

	internal Button btn_ChartM;

	internal Button btn_ChartM_Copy;

	internal TextBox txt_LengthDiffMax;

	internal TextBox txt_LengthMaxDiffResult;

	internal ComboBox cb_MaskSelect;

	internal ComboBox cb_Mask_DEStart;

	internal ComboBox cb_Mask_DEStartCount;

	internal Button btn_TrackerAdjShow;

	internal Button btn_PreviewSwitch_Copy;

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

	internal Label Label_Index;

	internal TextBox lab_LockMode;

	internal Rectangle rt_Mask1;

	internal Rectangle rt_Mask2;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public MeasureWindow(User_Permissions permissions)
	{
		inited = false;
		User = permissions;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		bool[] colorDropper = ColorDropper;
		bool[] colorDropper2 = ColorDropper;
		bool flag;
		ColorDropper[2] = (flag = (ColorDropper[3] = false));
		colorDropper2[1] = (flag = flag);
		colorDropper[0] = flag;
		ToolsButton = new Button[20]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20
		};
		TrackSelect = new ComboBox[3] { cb_TrackerSelect_W, cb_TrackerSelect_H, cb_TrackerSelect_A };
		TrackON = new ToggleSwitch[3] { ts_TrackerON_W, ts_TrackerON_H, ts_TrackerON_A };
		TrackName = new TextBox[3] { txt_TrackerName_W, txt_TrackerName_H, txt_TrackerName_A };
		RadScanDir1 = new RadioButton[2, 2]
		{
			{ rad_ScanL2RW1, rad_ScanR2LW1 },
			{ rad_ScanL2RH1, rad_ScanR2LH1 }
		};
		RadScanDir2 = new RadioButton[2, 2]
		{
			{ rad_ScanL2RW2, rad_ScanR2LW2 },
			{ rad_ScanL2RH2, rad_ScanR2LH2 }
		};
		CHK_UseRGBChannel = new CheckBox[2, 2]
		{
			{ chk_EnableRGBThd_WA, chk_EnableRGBThd_WB },
			{ chk_EnableRGBThd_HA, chk_EnableRGBThd_HB }
		};
		GRD_RGBThd = new Grid[2, 2]
		{
			{ grd_RGBThd_WA, grd_RGBThd_WB },
			{ grd_RGBThd_HA, grd_RGBThd_HB }
		};
		THD_Sliders = new RangeSlider[2, 2]
		{
			{ rs_ThdW1, rs_ThdW2 },
			{ rs_ThdH1, rs_ThdH2 }
		};
		THD_Sliders_R = new RangeSlider[2, 2]
		{
			{ rs_THD_WA_R, rs_THD_WB_R },
			{ rs_THD_HA_R, rs_THD_HB_R }
		};
		THD_Sliders_G = new RangeSlider[2, 2]
		{
			{ rs_THD_WA_G, rs_THD_WB_G },
			{ rs_THD_HA_G, rs_THD_HB_G }
		};
		THD_Sliders_B = new RangeSlider[2, 2]
		{
			{ rs_THD_WA_B, rs_THD_WB_B },
			{ rs_THD_HA_B, rs_THD_HB_B }
		};
		LabTHD_RMin = new Label[2, 2]
		{
			{ lab_THD_WA_R_Min, lab_THD_WB_R_Min },
			{ lab_THD_HA_R_Min, lab_THD_HB_R_Min }
		};
		LabTHD_GMin = new Label[2, 2]
		{
			{ lab_THD_WA_G_Min, lab_THD_WB_G_Min },
			{ lab_THD_HA_G_Min, lab_THD_HB_G_Min }
		};
		LabTHD_BMin = new Label[2, 2]
		{
			{ lab_THD_WA_B_Min, lab_THD_WB_B_Min },
			{ lab_THD_HA_B_Min, lab_THD_HB_B_Min }
		};
		LabTHD_RMax = new Label[2, 2]
		{
			{ lab_THD_WA_R_Max, lab_THD_WB_R_Max },
			{ lab_THD_HA_R_Max, lab_THD_HB_R_Max }
		};
		LabTHD_GMax = new Label[2, 2]
		{
			{ lab_THD_WA_G_Max, lab_THD_WB_G_Max },
			{ lab_THD_HA_G_Max, lab_THD_HB_G_Max }
		};
		LabTHD_BMax = new Label[2, 2]
		{
			{ lab_THD_WA_B_Max, lab_THD_WB_B_Max },
			{ lab_THD_HA_B_Max, lab_THD_HB_B_Max }
		};
		THD_TextLow = new NumericUpDown[2, 2]
		{
			{ nu_ThdW1LimitMin, nu_ThdW2LimitMin },
			{ nu_ThdH1LimitMin, nu_ThdH2LimitMin }
		};
		THD_TextUp = new NumericUpDown[2, 2]
		{
			{ nu_ThdW1LimitMax, nu_ThdW2LimitMax },
			{ nu_ThdH1LimitMax, nu_ThdH2LimitMax }
		};
		THD_Channel = new ComboBox[2, 2]
		{
			{ cb_ChannelW1, cb_ChannelW2 },
			{ cb_ChannelH1, cb_ChannelH2 }
		};
		THD_DEStart = new ComboBox[2, 2]
		{
			{ cb_ThdDEStartW1, cb_ThdDEStartW2 },
			{ cb_ThdDEStartH1, cb_ThdDEStartH2 }
		};
		THD_DEEnd = new ComboBox[2, 2]
		{
			{ cb_ThdDEEndW1, cb_ThdDEEndW2 },
			{ cb_ThdDEEndH1, cb_ThdDEEndH2 }
		};
		THD_DEStartCount = new ComboBox[2, 2]
		{
			{ cb_ThdDEStartCountW1, cb_ThdDEStartCountW2 },
			{ cb_ThdDEStartCountH1, cb_ThdDEStartCountH2 }
		};
		THD_DEEndCount = new ComboBox[2, 2]
		{
			{ cb_ThdDEEndCountW1, cb_ThdDEEndCountW2 },
			{ cb_ThdDEEndCountH1, cb_ThdDEEndCountH2 }
		};
		ErrorBin = new ComboBox[2] { cb_ErrorBin, cb_AdvErrorBin };
		Label_Result.Visibility = Visibility.Hidden;
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		txt_PreviewMode.Visibility = Visibility.Hidden;
		grd_RGBThd_HA.Visibility = Visibility.Hidden;
		grd_RGBThd_HB.Visibility = Visibility.Hidden;
		grd_RGBThd_WA.Visibility = Visibility.Hidden;
		grd_RGBThd_WB.Visibility = Visibility.Hidden;
		rt_Mask1.Visibility = Visibility.Hidden;
		rt_Mask2.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(int functionindex, int index, CVInspection station, int faceIndex = 0)
	{
		inspection = station;
		measureTool = new MeasureTool(inspection.paramFile);
		measureTool.SetBuffers(inspection.buffers);
		UpdateUI = false;
		FaceIndex = faceIndex;
		SetFaceIndex(faceIndex);
		cb_TrackerSelect_W.Items.Clear();
		cb_TrackerSelect_H.Items.Clear();
		cb_TrackerSelect_A.Items.Clear();
		cb_AdvSelect1W.Items.Clear();
		cb_AdvSelect2W.Items.Clear();
		cb_AdvSelect1H.Items.Clear();
		cb_AdvSelect2H.Items.Clear();
		cb_MaskSelect.Items.Clear();
		cb_MaskSelect.Items.Add("-----");
		try
		{
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
			txt_CorrectWidth.Text = inspection.paramFile.ObjSizeUM_W.ToString();
			txt_CorrectHeight.Text = inspection.paramFile.ObjSizeUM_H.ToString();
			for (int j = 0; j < 20; j++)
			{
				cb_TrackerSelect_W.Items.Add(j + 1 + " : " + inspection.paramFile.MeaWidthParam[j].TrackerName);
				cb_TrackerSelect_H.Items.Add(j + 1 + " : " + inspection.paramFile.MeaHeightParam[j].TrackerName);
				cb_TrackerSelect_A.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateMParam[j].TrackerName);
				cb_AdvSelect1W.Items.Add(j + 1 + " : " + inspection.paramFile.MeaWidthParam[j].TrackerName);
				cb_AdvSelect2W.Items.Add(j + 1 + " : " + inspection.paramFile.MeaWidthParam[j].TrackerName);
				cb_AdvSelect1H.Items.Add(j + 1 + " : " + inspection.paramFile.MeaHeightParam[j].TrackerName);
				cb_AdvSelect2H.Items.Add(j + 1 + " : " + inspection.paramFile.MeaHeightParam[j].TrackerName);
			}
			TrackSelect[0].SelectedIndex = 0;
			TrackSelect[1].SelectedIndex = 0;
			TrackSelect[2].SelectedIndex = 0;
			TrackIndex[0] = 0;
			TrackIndex[1] = 0;
			TrackIndex[2] = 0;
			TrackIndex[3] = 0;
			tab_AdvMeasureFun.SelectedIndex = functionindex;
			TrackIndex[functionindex] = index;
			TrackSelect[functionindex].SelectedIndex = index;
			CurrentFunction = functionindex;
			bor_CorrectRect.Visibility = Visibility.Hidden;
			UpdateUI = true;
			UpdateToolsState(functionindex);
			UpdateToolBox(functionindex);
		}
		catch
		{
			string text = "MW01-MeasureWindow-SetInspection()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void UpdateToolBox(int functionindex)
	{
		for (int i = 0; i < 20; i++)
		{
			ToolsButton[i].Visibility = Visibility.Visible;
			switch (functionindex)
			{
			case 0:
				if (inspection.paramFile.MeaWidthParam[i].TrackerON)
				{
					if (inspection.iResult.MeaWidthResult[i] == TOOL_RESULT.EM_OK)
					{
						if (inspection.paramFile.MeaWidthParam[i].EnableSelectFace)
						{
							if (inspection.paramFile.MeaWidthParam[i].FaceIndex == FaceIndex)
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
					else if (inspection.iResult.MeaWidthResult[i] == TOOL_RESULT.EM_NG)
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
				ToolsButton[i].ToolTip = inspection.paramFile.MeaWidthParam[i].TrackerName;
				break;
			case 1:
				if (inspection.paramFile.MeaHeightParam[i].TrackerON)
				{
					if (inspection.iResult.MeaHeightResult[i] == TOOL_RESULT.EM_OK)
					{
						if (inspection.paramFile.MeaHeightParam[i].EnableSelectFace)
						{
							if (inspection.paramFile.MeaHeightParam[i].FaceIndex == FaceIndex)
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
					else if (inspection.iResult.MeaWidthResult[i] == TOOL_RESULT.EM_NG)
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
				ToolsButton[i].ToolTip = inspection.paramFile.MeaHeightParam[i].TrackerName;
				break;
			default:
				ToolsButton[i].Visibility = Visibility.Hidden;
				break;
			}
		}
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		bool updateUI = UpdateUI;
		UpdateUI = false;
		ThroughputFile = file;
		cb_ParetoChartW.Items.Clear();
		cb_ParetoChartH.Items.Clear();
		for (int i = 0; i < 30; i++)
		{
			if (ThroughputFile.param.AllParetoItems[i] != "")
			{
				cb_ParetoChartW.Items.Add(ThroughputFile.param.AllParetoItems[i]);
				cb_ParetoChartH.Items.Add(ThroughputFile.param.AllParetoItems[i]);
			}
		}
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateToolsState(int functionindex)
	{
		if (!UpdateUI)
		{
			return;
		}
		tab_W.Background = Brushes.White;
		tab_H.Background = Brushes.White;
		tab_C.Background = Brushes.White;
		switch (functionindex)
		{
		case 0:
		{
			tab_W.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3DD5E2"));
			ToolsButton[0].Background = Brushes.White;
			ToolsButton[1].Background = Brushes.White;
			for (int j = 0; j < 20; j++)
			{
				if (inspection.iResult.MeaWidthResult[j] == TOOL_RESULT.EM_OK)
				{
					ToolsButton[j].Background = Brushes.Lime;
				}
				else if (inspection.iResult.MeaWidthResult[j] == TOOL_RESULT.EM_NG)
				{
					ToolsButton[j].Background = Brushes.Red;
				}
				else
				{
					ToolsButton[j].Background = Brushes.White;
				}
				ToolsButton[j].ToolTip = inspection.paramFile.MeaWidthParam[j].TrackerName;
			}
			break;
		}
		case 1:
		{
			tab_H.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9E167"));
			for (int i = 2; i < 20; i++)
			{
				if (inspection.iResult.MeaHeightResult[i] == TOOL_RESULT.EM_OK)
				{
					ToolsButton[i].Background = Brushes.Lime;
				}
				else if (inspection.iResult.MeaHeightResult[i] == TOOL_RESULT.EM_NG)
				{
					ToolsButton[i].Background = Brushes.Red;
				}
				else
				{
					ToolsButton[i].Background = Brushes.White;
				}
				ToolsButton[i].ToolTip = inspection.paramFile.MeaHeightParam[i].TrackerName;
			}
			break;
		}
		}
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

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect)
	{
		cnv_Image.SetValue(FrameworkElement.WidthProperty, 800.0);
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
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
		LocateRect = locateRect;
		CopyValueToUI(CurrentFunction);
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI(0);
	}

	public void SetModifyLogEnable(bool IsEnable, string recipename, string userID)
	{
		EnableModifyLog = IsEnable;
		RecipeName = recipename;
		UserID = userID;
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		inspection.paramFile.UseFloat = chk_UseFloat.IsChecked.Value;
		WriteUIValueToParam(CurrentFunction, TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
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
		Button button = (Button)sender;
		Mat offlineMatNoAI = inspection.buffers.GetOfflineMatNoAI(button.Uid);
		THD_Preview = false;
		ImageSource imageSource = inspection.buffers.Mat2ImageSource(offlineMatNoAI);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetTracker(OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2)
	{
		if (!PreviewMode)
		{
			OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			rect1 &= rect3;
			if (rect1.Width <= 0 || rect1.Height <= 0)
			{
				rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			rect2 &= rect3;
			if (rect2.Width <= 0 || rect2.Height <= 0)
			{
				rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
			bor_SearchRect2.SetValue(Canvas.LeftProperty, (double)rect2.X * xRatio);
			bor_SearchRect2.SetValue(Canvas.TopProperty, (double)rect2.Y * yRatio);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, (double)rect2.Width * xRatio);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, (double)rect2.Height * yRatio);
			txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			txt_Tracker2Pos.Content = string.Format("[2] X:{0}, Y:{1}, W:{2}, H:{3}", rect2.X, rect2.Y, rect2.Width, rect2.Height);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTracker(ref OpenCvSharp.Rect rect1, ref OpenCvSharp.Rect rect2)
	{
		if (PreviewMode || xRatio == 0.0 || yRatio == 0.0)
		{
			return;
		}
		OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
		rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
		rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
		rect2.X = (int)(Canvas.GetLeft(bor_SearchRect2) / xRatio);
		rect2.Y = (int)(Canvas.GetTop(bor_SearchRect2) / yRatio);
		rect2.Width = (int)(bor_SearchRect2.Width / xRatio);
		rect2.Height = (int)(bor_SearchRect2.Height / yRatio);
		if (rad_UseSinglePt.IsChecked == false)
		{
			if (CurrentFunction == 0)
			{
				rect2.Y = rect1.Y;
				rect2.Height = rect1.Height;
				bor_SearchRect2.SetValue(Canvas.TopProperty, (double)rect2.Y * yRatio);
				bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, (double)rect2.Height * yRatio);
			}
			else
			{
				rect2.X = rect1.X;
				rect2.Width = rect1.Width;
				bor_SearchRect2.SetValue(Canvas.LeftProperty, (double)rect2.X * xRatio);
				bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, (double)rect2.Width * xRatio);
			}
		}
		rect1 &= rect3;
		rect2 &= rect3;
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		txt_Tracker2Pos.Content = string.Format("[2] X:{0}, Y:{1}, W:{2}, H:{3}", rect2.X, rect2.Y, rect2.Width, rect2.Height);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Rs_SIMP_THD1_ThumbDragCompleted(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			if (((RangeSlider)sender).Uid == "0")
			{
				TrackerA_Changed = true;
			}
			else
			{
				TrackerA_Changed = false;
			}
			doThdImage();
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
			doThdImage();
			THD_Preview = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetResult(bool result)
	{
		Label_Result.Visibility = Visibility.Visible;
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = Brushes.Lime;
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Red;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void doThdImage()
	{
		if (PreviewMode || CurrentFunction > 2 || !UpdateUI)
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
		bor_SearchRect1.Visibility = Visibility.Visible;
		bor_SearchRect2.Visibility = Visibility.Visible;
		if (TrackON[CurrentFunction].IsChecked == false)
		{
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
		try
		{
			Label_Result.Visibility = Visibility.Hidden;
			Mat[] array = new Mat[2];
			Mat[] array2 = new Mat[2];
			Mat[] array3 = new Mat[2];
			int[] array4 = new int[2];
			int[] array5 = new int[2];
			int[] array6 = new int[2];
			int[] array7 = new int[2];
			int[] array8 = new int[2];
			int[] array9 = new int[2];
			int[] array10 = new int[2];
			int[] array11 = new int[2];
			int[] array12 = new int[2];
			_ = new bool[2];
			int[] array13 = new int[2];
			int[] array14 = new int[2];
			int[] array15 = new int[2];
			int[] array16 = new int[2];
			int currentFunction = CurrentFunction;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
			GetTracker(ref rect, ref rect2);
			Mat[] array17 = new Mat[2];
			for (int i = 0; i < 2; i++)
			{
				array[i] = new Mat();
				array2[i] = new Mat();
				array4[i] = (int)THD_Sliders[currentFunction, i].LowerValue;
				array5[i] = (int)THD_Sliders[currentFunction, i].UpperValue;
				array12[i] = THD_Channel[currentFunction, i].SelectedIndex;
				array13[i] = THD_DEStart[currentFunction, i].SelectedIndex;
				array14[i] = THD_DEEnd[currentFunction, i].SelectedIndex;
				array15[i] = THD_DEStartCount[currentFunction, i].SelectedIndex;
				array16[i] = THD_DEEndCount[currentFunction, i].SelectedIndex;
				THD_TextLow[currentFunction, i].Value = array4[i];
				THD_TextUp[currentFunction, i].Value = array5[i];
				array6[i] = (int)THD_Sliders_R[currentFunction, i].LowerValue;
				array7[i] = (int)THD_Sliders_R[currentFunction, i].UpperValue;
				array8[i] = (int)THD_Sliders_G[currentFunction, i].LowerValue;
				array9[i] = (int)THD_Sliders_G[currentFunction, i].UpperValue;
				array10[i] = (int)THD_Sliders_B[currentFunction, i].LowerValue;
				array11[i] = (int)THD_Sliders_B[currentFunction, i].UpperValue;
				array[i] = inspection.buffers.GetChannelMat(array12[i]).Clone();
				array2[i] = array[i].Clone();
				if (i == 0)
				{
					array17[i] = new Mat(array[i], rect);
				}
				else
				{
					array17[i] = new Mat(array[i], rect2);
				}
				if (CHK_UseRGBChannel[currentFunction, i].IsChecked == false)
				{
					Cv2.InRange(array17[i], array4[i], array5[i], array17[i]);
				}
				else
				{
					if (i == 0)
					{
						array3[i] = new Mat(inspection.buffers.LocatedSrc, rect);
					}
					else
					{
						array3[i] = new Mat(inspection.buffers.LocatedSrc, rect2);
					}
					Cv2.InRange(array3[i], new Scalar(array10[i], array8[i], array6[i]), new Scalar(array11[i], array9[i], array7[i]), array17[i]);
				}
				if (array13[i] != 0)
				{
					doMorph(ref array17[i], array13[i], array15[i]);
				}
				if (array14[i] != 0)
				{
					doMorph(ref array17[i], array14[i], array16[i]);
				}
			}
			array2[0].SetTo(0.0);
			array2[1].SetTo(0.0);
			Mat m = new Mat(array2[0], rect);
			if (TrackerA_Changed)
			{
				array17[0].CopyTo(m);
			}
			Mat m2 = new Mat(array2[1], rect2);
			if (!TrackerA_Changed)
			{
				array17[1].CopyTo(m2);
			}
			Mat mat = inspection.buffers.LocateGray.Clone();
			mat.SetTo(0.0);
			for (int j = 0; j < 2; j++)
			{
				if (ThdAPress && j == 0)
				{
					Cv2.BitwiseOr(mat, array2[j], mat);
				}
				else if (ThdBPress && j == 1)
				{
					Cv2.BitwiseOr(mat, array2[j], mat);
				}
				else if (!ThdBPress && !ThdAPress)
				{
					Cv2.BitwiseOr(mat, array2[j], mat);
				}
			}
			Mat m3 = new Mat();
			if (cb_MaskSelect.SelectedIndex > 0)
			{
				try
				{
					int num = cb_MaskSelect.SelectedIndex - 1;
					Mat mat2 = mat.Clone();
					mat2.SetTo(0.0);
					bool flag = false;
					if (MaskRect[0].Width > 0 && MaskRect[0].Height > 0 && inspection.paramFile.MaskParam[num].Mask1 != null)
					{
						Mat m4 = new Mat(mat2, MaskRect[0]);
						inspection.paramFile.MaskParam[num].Mask1.Clone().CopyTo(m4);
						flag = true;
					}
					if (MaskRect[1].Width > 0 && MaskRect[1].Height > 0 && inspection.paramFile.MaskParam[num].Mask2 != null)
					{
						Mat m5 = new Mat(mat2, MaskRect[1]);
						inspection.paramFile.MaskParam[num].Mask2.Clone().CopyTo(m5);
						flag = true;
					}
					if (flag)
					{
						doMorph(ref mat2, cb_Mask_DEStart.SelectedIndex, cb_Mask_DEStartCount.SelectedIndex);
						Cv2.BitwiseNot(mat2, mat2);
						Cv2.BitwiseAnd(mat, mat2, mat);
						mat2.CopyTo(m3);
					}
				}
				catch (Exception ex)
				{
					string text = "BW02-BinaryWindow-DoThdImage()-Do Mask!-";
					text += ex.ToString();
					CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
					if (ShowMessage)
					{
						MessageBox.Show(text, "錯誤");
					}
					return;
				}
			}
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			Mat mat3 = new Mat();
			Cv2.Merge(ThdShow, mat3);
			mat3.CopyTo(ShowBitmap, mat);
			ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush2 = new ImageBrush();
			imageBrush2.ImageSource = imageSource2;
			cnv_Image.Background = imageBrush2;
			imageSource2 = null;
			THD_Preview = true;
		}
		catch
		{
			string text2 = "MW02-MeasureWindow-doThdImage()";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
	{
		if (count != 0)
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
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Tracker value will set to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			if (((Button)sender).Uid == "1")
			{
				bor_SearchRect1.SetValue(Canvas.LeftProperty, 0.0);
				bor_SearchRect1.SetValue(Canvas.TopProperty, 0.0);
				bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, 100.0);
				bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, 100.0);
			}
			else
			{
				bor_SearchRect2.SetValue(Canvas.LeftProperty, 100.0);
				bor_SearchRect2.SetValue(Canvas.TopProperty, 0.0);
				bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, 100.0);
				bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, 100.0);
			}
		}
	}

	private void Cnv_Image_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		System.Windows.Point position = e.GetPosition(this);
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			int num = (int)(position.X / xRatio);
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
				lab_C4Value.Content = (CurrentColor[10] = inspection.buffers.GetLocatedCustomMat(3).At<byte>(num2, num));
				lab_C5Value.Content = (CurrentColor[11] = inspection.buffers.GetLocatedCustomMat(4).At<byte>(num2, num));
				lab_C6Value.Content = (CurrentColor[12] = inspection.buffers.GetLocatedCustomMat(5).At<byte>(num2, num));
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

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 1 && User < User_Permissions.USER_Administrator && inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Locked)
		{
			return;
		}
		if (!PreviewMode)
		{
			ToggleSwitch toggleSwitch = (ToggleSwitch)sender;
			Visibility visibility = ((!toggleSwitch.IsChecked.Value) ? Visibility.Hidden : Visibility.Visible);
			bor_SearchRect1.Visibility = visibility;
			bor_SearchRect2.Visibility = visibility;
			switch (CurrentFunction)
			{
			case 0:
				inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].TrackerON = toggleSwitch.IsChecked.Value;
				inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].ValueChangeFlag = true;
				gb_WTHD1.Visibility = visibility;
				gb_WTHD2.Visibility = visibility;
				grd_ParamWH.Visibility = visibility;
				break;
			case 1:
				inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].TrackerON = toggleSwitch.IsChecked.Value;
				inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].ValueChangeFlag = true;
				gb_HTHD1.Visibility = visibility;
				gb_HTHD2.Visibility = visibility;
				grd_ParamWH.Visibility = visibility;
				break;
			}
			if (!toggleSwitch.IsChecked.Value)
			{
				ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
			}
		}
	}

	private void Btn_ReDraw_Click(object sender, RoutedEventArgs e)
	{
		doThdImage();
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect, ref rect2);
	}

	private void Btn_ScanL2R_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanL2RH1.IsChecked = true;
	}

	private void Btn_ScanR2L_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LH1.IsChecked = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_TrackerSelect_W_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
		WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
		}
		UpdateUI = false;
		TrackIndex[CurrentFunction] = cb_TrackerSelect_W.SelectedIndex;
		scr_TrackerSwich_W.Value = cb_TrackerSelect_W.SelectedIndex;
		UpdateUI = true;
		CopyValueToUI(CurrentFunction);
		Btn_Test_Click(sender, e);
		UpdateToolBox(CurrentFunction);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_TrackerSelect_A_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
		WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
		}
		TrackIndex[CurrentFunction] = cb_TrackerSelect_A.SelectedIndex;
		CopyValueToUI(CurrentFunction);
		Btn_Test_Click(sender, e);
	}

	private void Scr_TrackerSwich_H_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	private void Scr_TrackerSwich_A_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_AdvMeasureFun_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited || !UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
		}
		else
		{
			if (!(e.AddedItems[0] is TabItem tabItem))
			{
				return;
			}
			tab_W.Background = Brushes.White;
			tab_H.Background = Brushes.White;
			tab_C.Background = Brushes.White;
			if (tabItem.Name == "tab_W")
			{
				tab_W.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3DD5E2"));
				WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
				if (EnableModifyLog)
				{
					WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
				}
				CurrentFunction = 0;
				CopyValueToUI(CurrentFunction);
				UpdateToolBox(CurrentFunction);
				bor_SearchRect1.Visibility = Visibility.Visible;
				bor_SearchRect2.Visibility = Visibility.Visible;
				bor_CorrectRect.Visibility = Visibility.Hidden;
				if (ts_TrackerON_W.IsChecked == true)
				{
					grd_ParamWH.Visibility = Visibility.Visible;
				}
				else
				{
					grd_ParamWH.Visibility = Visibility.Hidden;
				}
			}
			else if (tabItem.Name == "tab_H")
			{
				tab_H.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9E167"));
				WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
				if (EnableModifyLog)
				{
					WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
				}
				CurrentFunction = 1;
				CopyValueToUI(CurrentFunction);
				UpdateToolBox(CurrentFunction);
				bor_SearchRect1.Visibility = Visibility.Visible;
				bor_SearchRect2.Visibility = Visibility.Visible;
				bor_CorrectRect.Visibility = Visibility.Hidden;
				if (ts_TrackerON_H.IsChecked == true)
				{
					grd_ParamWH.Visibility = Visibility.Visible;
				}
				else
				{
					grd_ParamWH.Visibility = Visibility.Hidden;
				}
			}
			else if (tabItem.Name == "tab_C")
			{
				tab_C.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5"));
				WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
				if (EnableModifyLog)
				{
					WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
				}
				CurrentFunction = 3;
				CopyValueToUI(CurrentFunction);
				UpdateToolBox(CurrentFunction);
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_SearchRect2.Visibility = Visibility.Hidden;
				bor_CorrectRect.Visibility = Visibility.Visible;
				grd_ParamWH.Visibility = Visibility.Hidden;
				bor_CorrectRect.SetValue(Canvas.LeftProperty, (double)LocateRect.X * xRatio);
				bor_CorrectRect.SetValue(Canvas.TopProperty, (double)LocateRect.Y * yRatio);
				bor_CorrectRect.SetValue(FrameworkElement.WidthProperty, (double)LocateRect.Width * xRatio);
				bor_CorrectRect.SetValue(FrameworkElement.HeightProperty, (double)LocateRect.Height * yRatio);
				txt_PixelperUmW.Text = inspection.paramFile.PixelPerUM_X.ToString();
				txt_PixelperUmH.Text = inspection.paramFile.PixelPerUM_Y.ToString();
				chk_UseFloat.IsChecked = inspection.paramFile.UseFloat;
				if (inspection.paramFile.MeasureUnit == 0)
				{
					rad_um.IsChecked = true;
					lab_unit1.Content = "um";
					lab_unit2.Content = "um";
					lab_unit3.Content = "um";
					lab_unit4.Content = "um";
				}
				else
				{
					rad_mm.IsChecked = true;
					lab_unit1.Content = "mm";
					lab_unit2.Content = "mm";
					lab_unit3.Content = "mm";
					lab_unit4.Content = "mm";
				}
			}
		}
	}

	private void Rad_ScanL2RW1_Unchecked(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LW1.IsChecked = true;
	}

	private void Rad_ScanL2RW2_Unloaded(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LW2.IsChecked = true;
	}

	private void Rad_ScanL2RH1_Unchecked(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LH1.IsChecked = true;
	}

	private void Rad_ScanL2RH2_Unchecked(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LH2.IsChecked = true;
	}

	private void Btn_ScanL2RH2_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanL2RH2.IsChecked = true;
	}

	private void Btn_ScanR2LH2_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LH2.IsChecked = true;
	}

	private void Scr_TrackerSwich_H_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_TrackerSelect_H_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
		WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex[tab_AdvMeasureFun.SelectedIndex]);
		}
		UpdateUI = false;
		TrackIndex[CurrentFunction] = cb_TrackerSelect_H.SelectedIndex;
		UpdateUI = true;
		CopyValueToUI(CurrentFunction);
		Btn_Test_Click(sender, e);
		UpdateToolBox(CurrentFunction);
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
		if (!txt_LengthMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_LengthMin.Text))
			{
				txt_LengthMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_LengthMin.Background = Brushes.White;
			}
		}
		if (!txt_LengthMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_LengthMax.Text))
			{
				txt_LengthMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_LengthMax.Background = Brushes.White;
			}
		}
		if (!IsNumeric(txt_ContError.Text))
		{
			txt_ContError.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ContError.Background = Brushes.White;
		}
		if (!IsNumeric(txt_MaxError.Text))
		{
			txt_MaxError.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_MaxError.Background = Brushes.White;
		}
		if (!txt_SinglePtLengthMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_SinglePtLengthMin.Text))
			{
				txt_SinglePtLengthMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_SinglePtLengthMin.Background = Brushes.White;
			}
		}
		if (!txt_SinglePtLengthMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_SinglePtLengthMax.Text))
			{
				txt_SinglePtLengthMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_SinglePtLengthMax.Background = Brushes.White;
			}
		}
		if (!IsNumeric(txt_CorrectWidth.Text))
		{
			txt_CorrectWidth.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_CorrectWidth.Background = Brushes.White;
		}
		if (!IsNumeric(txt_CorrectHeight.Text))
		{
			txt_CorrectHeight.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_CorrectHeight.Background = Brushes.White;
		}
		return result;
	}

	private void DrawBackGround()
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam(int findex, int trackindex)
	{
		switch (findex)
		{
		case 0:
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.MeaWidthParam[trackindex].Locked)
			{
				return;
			}
			break;
		case 1:
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.MeaHeightParam[trackindex].Locked)
			{
				return;
			}
			break;
		}
		if (PreviewMode || findex >= 2)
		{
			return;
		}
		try
		{
			UpdateUI = false;
			if (findex < 2)
			{
				INSP_MEASURETOOL_PARAM iNSP_MEASURETOOL_PARAM = default(INSP_MEASURETOOL_PARAM);
				iNSP_MEASURETOOL_PARAM = ((findex != 0) ? inspection.paramFile.MeaHeightParam[trackindex] : inspection.paramFile.MeaWidthParam[trackindex]);
				iNSP_MEASURETOOL_PARAM.ValueChangeFlag = true;
				iNSP_MEASURETOOL_PARAM.TrackerON = TrackON[findex].IsChecked.Value;
				iNSP_MEASURETOOL_PARAM.TrackerName = TrackName[findex].Text;
				if (findex == 0)
				{
					iNSP_MEASURETOOL_PARAM.ParetoIndex = cb_ParetoChartW.SelectedIndex;
				}
				else
				{
					iNSP_MEASURETOOL_PARAM.ParetoIndex = cb_ParetoChartH.SelectedIndex;
				}
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
				OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
				GetTracker(ref rect, ref rect2);
				if (rect != DefaultSearchRect1 || rect2 != DefaultSearchRect2)
				{
					iNSP_MEASURETOOL_PARAM.SearchRect1L = rect.X - LocateRect.X;
					iNSP_MEASURETOOL_PARAM.SearchRect1T = rect.Y - LocateRect.Y;
					iNSP_MEASURETOOL_PARAM.SearchRect1R = LocateRect.Width - (iNSP_MEASURETOOL_PARAM.SearchRect1L + rect.Width);
					iNSP_MEASURETOOL_PARAM.SearchRect1B = LocateRect.Height - (iNSP_MEASURETOOL_PARAM.SearchRect1T + rect.Height);
					iNSP_MEASURETOOL_PARAM.SearchRect2L = rect2.X - LocateRect.X;
					iNSP_MEASURETOOL_PARAM.SearchRect2T = rect2.Y - LocateRect.Y;
					iNSP_MEASURETOOL_PARAM.SearchRect2R = LocateRect.Width - (iNSP_MEASURETOOL_PARAM.SearchRect2L + rect2.Width);
					iNSP_MEASURETOOL_PARAM.SearchRect2B = LocateRect.Height - (iNSP_MEASURETOOL_PARAM.SearchRect2T + rect2.Height);
					DefaultSearchRect1 = rect;
					DefaultSearchRect2 = rect2;
				}
				iNSP_MEASURETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)THD_Channel[findex, 0].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)THD_Channel[findex, 1].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.THD1_Min = (int)THD_Sliders[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD1_Max = (int)THD_Sliders[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD2_Min = (int)THD_Sliders[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD2_Max = (int)THD_Sliders[findex, 1].UpperValue;
				iNSP_MEASURETOOL_PARAM.EnableRGBThdA = CHK_UseRGBChannel[findex, 0].IsChecked.Value;
				iNSP_MEASURETOOL_PARAM.EnableRGBThdB = CHK_UseRGBChannel[findex, 1].IsChecked.Value;
				iNSP_MEASURETOOL_PARAM.THD_AR_Min = (int)THD_Sliders_R[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_AR_Max = (int)THD_Sliders_R[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_AG_Min = (int)THD_Sliders_G[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_AG_Max = (int)THD_Sliders_G[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_AB_Min = (int)THD_Sliders_B[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_AB_Max = (int)THD_Sliders_B[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_BR_Min = (int)THD_Sliders_R[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_BR_Max = (int)THD_Sliders_R[findex, 1].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_BG_Min = (int)THD_Sliders_G[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_BG_Max = (int)THD_Sliders_G[findex, 1].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_BB_Min = (int)THD_Sliders_B[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_BB_Max = (int)THD_Sliders_B[findex, 1].UpperValue;
				iNSP_MEASURETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)THD_DEStart[findex, 0].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)THD_DEEnd[findex, 0].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)THD_DEStart[findex, 1].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)THD_DEEnd[findex, 1].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_Start1_Count = THD_DEStartCount[findex, 0].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_End1_Count = THD_DEEndCount[findex, 0].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_Start2_Count = THD_DEStartCount[findex, 1].SelectedIndex;
				iNSP_MEASURETOOL_PARAM.DE_End2_Count = THD_DEEndCount[findex, 1].SelectedIndex;
				if (RadScanDir1[findex, 0].IsChecked.Value)
				{
					iNSP_MEASURETOOL_PARAM.Direction1 = (ScanDirection)(findex * 2);
				}
				else
				{
					iNSP_MEASURETOOL_PARAM.Direction1 = (ScanDirection)(1 + findex * 2);
				}
				if (RadScanDir2[findex, 0].IsChecked.Value)
				{
					iNSP_MEASURETOOL_PARAM.Direction2 = (ScanDirection)(findex * 2);
				}
				else
				{
					iNSP_MEASURETOOL_PARAM.Direction2 = (ScanDirection)(1 + findex * 2);
				}
				iNSP_MEASURETOOL_PARAM.Scan1Rule = (MeasureCondition)cb_Scan1Rule.SelectedIndex;
				iNSP_MEASURETOOL_PARAM.Scan2Rule = (MeasureCondition)cb_Scan2Rule.SelectedIndex;
				iNSP_MEASURETOOL_PARAM.SinglePtLengthMin = ConvertComDigits(txt_SinglePtLengthMin);
				iNSP_MEASURETOOL_PARAM.SinglePtLengthMax = ConvertComDigits(txt_SinglePtLengthMax);
				iNSP_MEASURETOOL_PARAM.LengthMin = ConvertComDigits(txt_LengthMin);
				iNSP_MEASURETOOL_PARAM.LengthMax = ConvertComDigits(txt_LengthMax);
				iNSP_MEASURETOOL_PARAM.LengthDiffMax = ConvertComDigits(txt_LengthDiffMax);
				iNSP_MEASURETOOL_PARAM.CheckEveryone = chk_CheckEveryOne.IsChecked.Value;
				iNSP_MEASURETOOL_PARAM.ContinueError = Convert.ToInt32(txt_ContError.Text);
				iNSP_MEASURETOOL_PARAM.MaxError = Convert.ToInt32(txt_MaxError.Text);
				iNSP_MEASURETOOL_PARAM.UseSinglePt = rad_UseSinglePt.IsChecked.Value;
				iNSP_MEASURETOOL_PARAM.EnableSelectFace = chk_SelectFace.IsChecked.Value;
				iNSP_MEASURETOOL_PARAM.FaceIndex = SelectFace;
				iNSP_MEASURETOOL_PARAM.MaskSelectIndex = cb_MaskSelect.SelectedIndex;
				iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Index = (USE_DILATE_ERODE)cb_Mask_DEStart.SelectedIndex;
				iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Count = cb_Mask_DEStartCount.SelectedIndex;
				iNSP_MEASURETOOL_PARAM.ErrorBin = (FLOW_BIN)ErrorBin[0].SelectedIndex;
				if (findex == 0)
				{
					iNSP_MEASURETOOL_PARAM.ValueChangeFlag = false;
					if (!iNSP_MEASURETOOL_PARAM.Equals(inspection.paramFile_Backup.MeaWidthParam[trackindex]))
					{
						iNSP_MEASURETOOL_PARAM.ValueChangeFlag = true;
						inspection.paramFile.MeaWidthParam[trackindex] = iNSP_MEASURETOOL_PARAM;
					}
				}
				else
				{
					iNSP_MEASURETOOL_PARAM.ValueChangeFlag = false;
					if (!iNSP_MEASURETOOL_PARAM.Equals(inspection.paramFile_Backup.MeaHeightParam[trackindex]))
					{
						iNSP_MEASURETOOL_PARAM.ValueChangeFlag = true;
						inspection.paramFile.MeaHeightParam[trackindex] = iNSP_MEASURETOOL_PARAM;
					}
					inspection.paramFile.MeaHeightParam[trackindex] = iNSP_MEASURETOOL_PARAM;
				}
				int selectedIndex = TrackSelect[findex].SelectedIndex;
				TrackSelect[findex].Items.Clear();
				for (int i = 0; i < 20; i++)
				{
					if (findex == 0)
					{
						TrackSelect[findex].Items.Add(i + 1 + " : " + inspection.paramFile.MeaWidthParam[i].TrackerName);
					}
					else
					{
						TrackSelect[findex].Items.Add(i + 1 + " : " + inspection.paramFile.MeaHeightParam[i].TrackerName);
					}
				}
				cb_AdvSelect1W.Items.Clear();
				cb_AdvSelect2W.Items.Clear();
				cb_AdvSelect1H.Items.Clear();
				cb_AdvSelect2H.Items.Clear();
				for (int j = 0; j < 20; j++)
				{
					cb_AdvSelect1W.Items.Add(j + 1 + " : " + inspection.paramFile.MeaWidthParam[j].TrackerName);
					cb_AdvSelect2W.Items.Add(j + 1 + " : " + inspection.paramFile.MeaWidthParam[j].TrackerName);
					cb_AdvSelect1H.Items.Add(j + 1 + " : " + inspection.paramFile.MeaHeightParam[j].TrackerName);
					cb_AdvSelect2H.Items.Add(j + 1 + " : " + inspection.paramFile.MeaHeightParam[j].TrackerName);
				}
				TrackSelect[findex].SelectedIndex = selectedIndex;
			}
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "MW03-MeasureWindow-WriteUIValueToParam()";
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
	private void CopyValueToUI(int findex)
	{
		try
		{
			UpdateUI = false;
			Label_ResultError.Visibility = Visibility.Hidden;
			Label_Index.Content = (TrackIndex[findex] + 1).ToString();
			if (PreviewMode)
			{
				btn_Test1.Visibility = Visibility.Hidden;
				btn_CorrectReSet.Visibility = Visibility.Hidden;
				btn_CorrectSet.Visibility = Visibility.Hidden;
			}
			if (findex < 2)
			{
				INSP_MEASURETOOL_PARAM iNSP_MEASURETOOL_PARAM;
				if (findex == 0)
				{
					iNSP_MEASURETOOL_PARAM = (BackupParamW = inspection.paramFile.MeaWidthParam[TrackIndex[findex]]);
					scr_TrackerSwich_W.Value = TrackIndex[findex];
					if (iNSP_MEASURETOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
					{
						lab_LockMode.Visibility = Visibility.Visible;
					}
					else
					{
						lab_LockMode.Visibility = Visibility.Hidden;
					}
					if (iNSP_MEASURETOOL_PARAM.TrackerON)
					{
						gb_WTHD1.Visibility = Visibility.Visible;
						gb_WTHD2.Visibility = Visibility.Visible;
						bor_SearchRect1.Visibility = Visibility.Visible;
						bor_SearchRect2.Visibility = Visibility.Visible;
					}
					else
					{
						gb_WTHD1.Visibility = Visibility.Hidden;
						gb_WTHD2.Visibility = Visibility.Hidden;
						bor_SearchRect1.Visibility = Visibility.Hidden;
						bor_SearchRect2.Visibility = Visibility.Hidden;
					}
					cb_ParetoChartW.SelectedIndex = iNSP_MEASURETOOL_PARAM.ParetoIndex;
				}
				else
				{
					iNSP_MEASURETOOL_PARAM = (BackupParamH = inspection.paramFile.MeaHeightParam[TrackIndex[findex]]);
					if (iNSP_MEASURETOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
					{
						lab_LockMode.Visibility = Visibility.Visible;
					}
					else
					{
						lab_LockMode.Visibility = Visibility.Hidden;
					}
					if (iNSP_MEASURETOOL_PARAM.TrackerON)
					{
						gb_HTHD1.Visibility = Visibility.Visible;
						gb_HTHD2.Visibility = Visibility.Visible;
						bor_SearchRect1.Visibility = Visibility.Visible;
						bor_SearchRect2.Visibility = Visibility.Visible;
					}
					else
					{
						gb_HTHD1.Visibility = Visibility.Hidden;
						gb_HTHD2.Visibility = Visibility.Hidden;
						bor_SearchRect1.Visibility = Visibility.Hidden;
						bor_SearchRect2.Visibility = Visibility.Hidden;
					}
					cb_ParetoChartH.SelectedIndex = iNSP_MEASURETOOL_PARAM.ParetoIndex;
				}
				TrackON[findex].IsChecked = iNSP_MEASURETOOL_PARAM.TrackerON;
				if (iNSP_MEASURETOOL_PARAM.TrackerON)
				{
					grd_ParamWH.Visibility = Visibility.Visible;
				}
				else
				{
					grd_ParamWH.Visibility = Visibility.Hidden;
				}
				TrackSelect[findex].SelectedIndex = TrackIndex[findex];
				TrackName[findex].Text = iNSP_MEASURETOOL_PARAM.TrackerName;
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
				OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
				rect.X = LocateRect.X + iNSP_MEASURETOOL_PARAM.SearchRect1L;
				rect.Y = LocateRect.Y + iNSP_MEASURETOOL_PARAM.SearchRect1T;
				rect.Width = LocateRect.Width - (iNSP_MEASURETOOL_PARAM.SearchRect1L + iNSP_MEASURETOOL_PARAM.SearchRect1R);
				rect.Height = LocateRect.Height - (iNSP_MEASURETOOL_PARAM.SearchRect1T + iNSP_MEASURETOOL_PARAM.SearchRect1B);
				rect2.X = LocateRect.X + iNSP_MEASURETOOL_PARAM.SearchRect2L;
				rect2.Y = LocateRect.Y + iNSP_MEASURETOOL_PARAM.SearchRect2T;
				rect2.Width = LocateRect.Width - (iNSP_MEASURETOOL_PARAM.SearchRect2L + iNSP_MEASURETOOL_PARAM.SearchRect2R);
				rect2.Height = LocateRect.Height - (iNSP_MEASURETOOL_PARAM.SearchRect2T + iNSP_MEASURETOOL_PARAM.SearchRect2B);
				SetTracker(rect, rect2);
				GetTracker(ref DefaultSearchRect1, ref DefaultSearchRect2);
				THD_Channel[findex, 0].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.THD1_ChannelIndex;
				THD_Channel[findex, 1].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.THD2_ChannelIndex;
				THD_Sliders[findex, 0].LowerValue = iNSP_MEASURETOOL_PARAM.THD1_Min;
				THD_Sliders[findex, 0].UpperValue = iNSP_MEASURETOOL_PARAM.THD1_Max;
				THD_Sliders[findex, 1].LowerValue = iNSP_MEASURETOOL_PARAM.THD2_Min;
				THD_Sliders[findex, 1].UpperValue = iNSP_MEASURETOOL_PARAM.THD2_Max;
				THD_TextLow[findex, 0].Value = iNSP_MEASURETOOL_PARAM.THD1_Min;
				THD_TextLow[findex, 1].Value = iNSP_MEASURETOOL_PARAM.THD2_Min;
				THD_TextUp[findex, 0].Value = iNSP_MEASURETOOL_PARAM.THD1_Max;
				THD_TextUp[findex, 1].Value = iNSP_MEASURETOOL_PARAM.THD2_Max;
				CHK_UseRGBChannel[findex, 0].IsChecked = iNSP_MEASURETOOL_PARAM.EnableRGBThdA;
				CHK_UseRGBChannel[findex, 1].IsChecked = iNSP_MEASURETOOL_PARAM.EnableRGBThdB;
				if (iNSP_MEASURETOOL_PARAM.EnableRGBThdA)
				{
					GRD_RGBThd[findex, 0].Visibility = Visibility.Visible;
				}
				else
				{
					GRD_RGBThd[findex, 0].Visibility = Visibility.Hidden;
				}
				if (iNSP_MEASURETOOL_PARAM.EnableRGBThdB)
				{
					GRD_RGBThd[findex, 1].Visibility = Visibility.Visible;
				}
				else
				{
					GRD_RGBThd[findex, 1].Visibility = Visibility.Hidden;
				}
				THD_Sliders_R[findex, 0].LowerValue = iNSP_MEASURETOOL_PARAM.THD_AR_Min;
				THD_Sliders_R[findex, 0].UpperValue = iNSP_MEASURETOOL_PARAM.THD_AR_Max;
				THD_Sliders_G[findex, 0].LowerValue = iNSP_MEASURETOOL_PARAM.THD_AG_Min;
				THD_Sliders_G[findex, 0].UpperValue = iNSP_MEASURETOOL_PARAM.THD_AG_Max;
				THD_Sliders_B[findex, 0].LowerValue = iNSP_MEASURETOOL_PARAM.THD_AB_Min;
				THD_Sliders_B[findex, 0].UpperValue = iNSP_MEASURETOOL_PARAM.THD_AB_Max;
				THD_Sliders_R[findex, 1].LowerValue = iNSP_MEASURETOOL_PARAM.THD_BR_Min;
				THD_Sliders_R[findex, 1].UpperValue = iNSP_MEASURETOOL_PARAM.THD_BR_Max;
				THD_Sliders_G[findex, 1].LowerValue = iNSP_MEASURETOOL_PARAM.THD_BG_Min;
				THD_Sliders_G[findex, 1].UpperValue = iNSP_MEASURETOOL_PARAM.THD_BG_Max;
				THD_Sliders_B[findex, 1].LowerValue = iNSP_MEASURETOOL_PARAM.THD_BB_Min;
				THD_Sliders_B[findex, 1].UpperValue = iNSP_MEASURETOOL_PARAM.THD_BB_Max;
				LabTHD_RMin[findex, 0].Content = iNSP_MEASURETOOL_PARAM.THD_AR_Min;
				LabTHD_RMax[findex, 0].Content = iNSP_MEASURETOOL_PARAM.THD_AR_Max;
				LabTHD_RMin[findex, 1].Content = iNSP_MEASURETOOL_PARAM.THD_BR_Min;
				LabTHD_RMax[findex, 1].Content = iNSP_MEASURETOOL_PARAM.THD_BR_Max;
				LabTHD_GMin[findex, 0].Content = iNSP_MEASURETOOL_PARAM.THD_AG_Min;
				LabTHD_GMax[findex, 0].Content = iNSP_MEASURETOOL_PARAM.THD_AG_Max;
				LabTHD_GMin[findex, 1].Content = iNSP_MEASURETOOL_PARAM.THD_BG_Min;
				LabTHD_GMax[findex, 1].Content = iNSP_MEASURETOOL_PARAM.THD_BG_Max;
				LabTHD_BMin[findex, 0].Content = iNSP_MEASURETOOL_PARAM.THD_AB_Min;
				LabTHD_BMax[findex, 0].Content = iNSP_MEASURETOOL_PARAM.THD_AB_Max;
				LabTHD_BMin[findex, 1].Content = iNSP_MEASURETOOL_PARAM.THD_BB_Min;
				LabTHD_BMax[findex, 1].Content = iNSP_MEASURETOOL_PARAM.THD_BB_Max;
				iNSP_MEASURETOOL_PARAM.THD_AR_Min = (int)THD_Sliders_R[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_AR_Max = (int)THD_Sliders_R[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_AG_Min = (int)THD_Sliders_G[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_AG_Max = (int)THD_Sliders_G[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_AB_Min = (int)THD_Sliders_B[findex, 0].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_AB_Max = (int)THD_Sliders_B[findex, 0].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_BR_Min = (int)THD_Sliders_R[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_BR_Max = (int)THD_Sliders_R[findex, 1].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_BG_Min = (int)THD_Sliders_G[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_BG_Max = (int)THD_Sliders_G[findex, 1].UpperValue;
				iNSP_MEASURETOOL_PARAM.THD_BB_Min = (int)THD_Sliders_B[findex, 1].LowerValue;
				iNSP_MEASURETOOL_PARAM.THD_BB_Max = (int)THD_Sliders_B[findex, 1].UpperValue;
				THD_DEStart[findex, 0].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.DE_Start1_Index;
				THD_DEStart[findex, 1].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.DE_Start2_Index;
				THD_DEEnd[findex, 0].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.DE_End1_Index;
				THD_DEEnd[findex, 1].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.DE_End2_Index;
				THD_DEStartCount[findex, 0].SelectedIndex = iNSP_MEASURETOOL_PARAM.DE_Start1_Count;
				THD_DEStartCount[findex, 1].SelectedIndex = iNSP_MEASURETOOL_PARAM.DE_Start2_Count;
				THD_DEEndCount[findex, 0].SelectedIndex = iNSP_MEASURETOOL_PARAM.DE_End1_Count;
				THD_DEEndCount[findex, 1].SelectedIndex = iNSP_MEASURETOOL_PARAM.DE_End2_Count;
				txt_ContError.Text = iNSP_MEASURETOOL_PARAM.ContinueError.ToString();
				txt_MaxError.Text = iNSP_MEASURETOOL_PARAM.MaxError.ToString();
				if ((int)iNSP_MEASURETOOL_PARAM.Direction1 % 2 == 0)
				{
					RadScanDir1[findex, 0].IsChecked = true;
				}
				else
				{
					RadScanDir1[findex, 1].IsChecked = true;
				}
				if ((int)iNSP_MEASURETOOL_PARAM.Direction2 % 2 == 0)
				{
					RadScanDir2[findex, 0].IsChecked = true;
				}
				else
				{
					RadScanDir2[findex, 1].IsChecked = true;
				}
				cb_Scan1Rule.SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.Scan1Rule;
				cb_Scan2Rule.SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.Scan2Rule;
				txt_SinglePtLengthMin.Text = GetComDigitsStr(iNSP_MEASURETOOL_PARAM.SinglePtLengthMin, txt_SinglePtLengthMin);
				txt_SinglePtLengthMax.Text = GetComDigitsStr(iNSP_MEASURETOOL_PARAM.SinglePtLengthMax, txt_SinglePtLengthMax);
				txt_LengthMin.Text = GetComDigitsStr(iNSP_MEASURETOOL_PARAM.LengthMin, txt_LengthMin);
				txt_LengthMax.Text = GetComDigitsStr(iNSP_MEASURETOOL_PARAM.LengthMax, txt_LengthMax);
				txt_LengthDiffMax.Text = GetComDigitsStr(iNSP_MEASURETOOL_PARAM.LengthDiffMax, txt_LengthDiffMax);
				chk_CheckEveryOne.IsChecked = iNSP_MEASURETOOL_PARAM.CheckEveryone;
				if (iNSP_MEASURETOOL_PARAM.UseSinglePt)
				{
					rad_UseSinglePt.IsChecked = true;
				}
				else
				{
					rad_UseAllPt.IsChecked = true;
				}
				cb_MaskSelect.SelectedIndex = iNSP_MEASURETOOL_PARAM.MaskSelectIndex;
				cb_Mask_DEStart.SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Index;
				cb_Mask_DEStartCount.SelectedIndex = iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Count;
				chk_SelectFace.IsChecked = iNSP_MEASURETOOL_PARAM.EnableSelectFace;
				SetSelectFace(iNSP_MEASURETOOL_PARAM.FaceIndex);
				ErrorBin[0].SelectedIndex = (int)iNSP_MEASURETOOL_PARAM.ErrorBin;
			}
			else
			{
				_ = 2;
			}
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
			UpdateUI = true;
		}
		catch
		{
			string text = "MW04-MeasureWindow-WriteUIValueToParam()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ScanWidth()
	{
		try
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			TimeStart();
			bool result = measureTool.MeasureWidth(TrackIndex[CurrentFunction], ref iresult);
			double num = TimeStop();
			lab_Time1.Content = string.Format("{0:N3}", num);
			int num2 = ((measureTool.ScanPtL.Count <= measureTool.ScanPtR.Count) ? measureTool.ScanPtL.Count : measureTool.ScanPtR.Count);
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
			GetTracker(ref rect, ref rect2);
			if (rad_UseAllPt.IsChecked == true)
			{
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				txt_AllResult.Clear();
				string text = "";
				text = ((LanguageType == 0) ? "Legth" : ((LanguageType != 1) ? "长度" : "長度"));
				string text2 = "";
				string text3 = "";
				for (int i = 0; i < num2; i++)
				{
					OpenCvSharp.Point pt = measureTool.ScanPtL[i];
					OpenCvSharp.Point pt2 = measureTool.ScanPtR[i];
					if (pt.Y == -1 || pt2.Y == -1 || pt.X == -1 || pt2.X == -1)
					{
						if (chk_CheckEveryOne.IsChecked == true)
						{
							num5++;
							num3++;
							if (num5 > num4)
							{
								num4 = num5;
							}
							text3 = "[NG]";
							text2 += string.Format("[{0}] {1} = {2}    {3}\n", i + 1, text, 0, text3);
						}
						continue;
					}
					double num6 = (double)Math.Abs(pt.X - pt2.X) * inspection.paramFile.PixelPerUM_X;
					double num7 = ConvertComDigits(txt_LengthMin, ForSave: false);
					double num8 = ConvertComDigits(txt_LengthMax, ForSave: false);
					if (num6 < num7 || num6 > num8)
					{
						num5++;
						num3++;
						if (chk_ShowScanNG.IsChecked == true)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Red);
						}
						if (num5 > num4)
						{
							num4 = num5;
						}
						text3 = "[NG]";
					}
					else
					{
						if (chk_ShowScanOK.IsChecked == true)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Lime);
						}
						num5 = 0;
						text3 = "";
					}
					text2 += string.Format("[{0}] {1} = {2}    {3}\n", i + 1, text, num6, text3);
				}
				text2 += string.Format("\nMin = {0:N3}\nMax = {1:N3}\nAvg = {2:N3}\nMax Diff = {3:N3}\n", measureTool.MinWidth, measureTool.MaxWidth, measureTool.AvgWidth, measureTool.MaxWidth - measureTool.MinWidth);
				txt_AllResult.Text += text2;
				txt_AllResult.ScrollToEnd();
				txt_ContErrorResult.Text = num4.ToString();
				txt_MaxErrorResult.Text = num3.ToString();
				if (num4 > Convert.ToInt32(txt_ContError.Text))
				{
					txt_ContError.Background = Brushes.Red;
				}
				else
				{
					txt_ContError.Background = Brushes.White;
				}
				if (num3 > Convert.ToInt32(txt_MaxError.Text))
				{
					txt_MaxError.Background = Brushes.Red;
				}
				else
				{
					txt_MaxError.Background = Brushes.White;
				}
				SetResult(result);
				if (chk_ShowScanEdge.IsChecked == true)
				{
					for (int j = 0; j < measureTool.ScanPtL.Count - 1; j++)
					{
						OpenCvSharp.Point pt = measureTool.ScanPtL[j];
						OpenCvSharp.Point pt2 = measureTool.ScanPtL[j + 1];
						if (pt.Y != -1 && pt2.Y != -1 && pt.X != -1 && pt2.X != -1)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Blue);
						}
					}
					for (int k = 0; k < measureTool.ScanPtR.Count - 1; k++)
					{
						OpenCvSharp.Point pt = measureTool.ScanPtR[k];
						OpenCvSharp.Point pt2 = measureTool.ScanPtR[k + 1];
						if (pt.Y != -1 && pt2.Y != -1 && pt.X != -1 && pt2.X != -1)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Blue);
						}
					}
				}
				if (measureTool.ScanPtL.Count <= 0)
				{
					txt_AllResult.Clear();
					if (LanguageType == 0)
					{
						txt_AllResult.Text = "Left Scan pt = 0";
					}
					else if (LanguageType == 1)
					{
						txt_AllResult.Text = "左邊掃描點為0!";
					}
					else
					{
						txt_AllResult.Text = "左边扫描点为0!";
					}
					return;
				}
				if (measureTool.ScanPtR.Count <= 0)
				{
					txt_AllResult.Clear();
					if (LanguageType == 0)
					{
						txt_AllResult.Text = "Right Scan pt = 0";
					}
					else if (LanguageType == 1)
					{
						txt_AllResult.Text = "右邊掃描點為0!";
					}
					else
					{
						txt_AllResult.Text = "右边扫描点为0!";
					}
					return;
				}
				double num9 = ConvertComDigits(txt_LengthDiffMax, ForSave: false);
				double num10 = measureTool.MaxWidth - measureTool.MinWidth;
				txt_LengthMaxDiffResult.Text = num10.ToString();
				txt_LengthDiffMax.Background = Brushes.White;
				if (num10 > num9 && num9 != 0.0)
				{
					string text4 = "";
					txt_LengthDiffMax.Background = Brushes.Red;
					text4 = ((LanguageType == 0) ? string.Format("[Measure Width][{0}:{1}] Width min max diff {2:N3} > {3:N3}.\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num10, num9) : ((LanguageType != 1) ? string.Format("[量测宽度][{0}:{1}] 宽度高低差{2:N3}大于设定值{3:N3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num10, num9) : string.Format("[量測寬度][{0}:{1}] 寬度高低差{2:N3}大於設定值{3:N3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num10, num9)));
					txt_AllResult.Text += text4;
				}
			}
			else
			{
				int num11 = 0;
				num11 = ((inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan1Rule == MeasureCondition.EM_AVG_LENGTH) ? inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan1AvgValue : ((inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan1Rule != MeasureCondition.EM_MIN_LENGTH) ? inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan1MaxValue : inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan1MinValue));
				int num12 = 0;
				num12 = ((inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan2Rule == MeasureCondition.EM_AVG_LENGTH) ? inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan2AvgValue : ((inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan2Rule != MeasureCondition.EM_MIN_LENGTH) ? inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan2MaxValue : inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Scan2MinValue));
				double num13 = (double)Math.Abs(num11 - num12) * inspection.paramFile.PixelPerUM_X;
				txt_AllResult.Clear();
				if (LanguageType == 0)
				{
					txt_AllResult.Text = string.Format("[Measure Width][{0}:{1}] Width = {2}.\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13);
				}
				else if (LanguageType == 1)
				{
					txt_AllResult.Text = string.Format("[量測寬度][{0}:{1}] 寬度 = {2}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13);
				}
				else
				{
					txt_AllResult.Text = string.Format("[量测宽度][{0}:{1}] 宽度 = {2}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13);
				}
				txt_SinglePtLengthResult.Text = num13.ToString();
				double num14 = ConvertComDigits(txt_SinglePtLengthMin, ForSave: false);
				double num15 = ConvertComDigits(txt_SinglePtLengthMax, ForSave: false);
				if (num13 < num14)
				{
					txt_SinglePtLengthMin.Background = Brushes.Red;
					string text5 = ((LanguageType == 0) ? string.Format("[Measure Width][{0}:{1}] Width {2} < {3}.\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num14) : ((LanguageType != 1) ? string.Format("[量测宽度][{0}:{1}] 宽度{2}小于设定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num14) : string.Format("[量測寬度][{0}:{1}] 寬度{2}小於設定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num14)));
					txt_AllResult.Text += text5;
				}
				if (num13 > num15)
				{
					txt_SinglePtLengthMax.Background = Brushes.Red;
					string text6 = ((LanguageType == 0) ? string.Format("[Measure Width][{0}:{1}] Width {2} > {3}.\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num15) : ((LanguageType != 1) ? string.Format("[量测宽度][{0}:{1}] 宽度{2}大于设定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num15) : string.Format("[量測寬度][{0}:{1}] 寬度{2}大於設定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num15)));
					txt_AllResult.Text += text6;
				}
				OpenCvSharp.Point pt3 = default(OpenCvSharp.Point);
				pt3.X = num11;
				pt3.Y = 0;
				OpenCvSharp.Point pt4 = default(OpenCvSharp.Point);
				pt4.X = num11;
				pt4.Y = ShowBitmap.Height;
				ShowBitmap.Line(pt3, pt4, Scalar.Fuchsia);
				pt3.X = num12;
				pt3.Y = 0;
				pt4.X = num12;
				pt4.Y = ShowBitmap.Height;
				ShowBitmap.Line(pt3, pt4, Scalar.Fuchsia);
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_SearchRect2.Visibility = Visibility.Hidden;
				SetResult(result);
			}
		}
		catch
		{
			string text7 = "MW04-MeasureWindow-ScanWidth()";
			CVBuffer.SaveLog(text7, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text7, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ScanHeight()
	{
		try
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			TimeStart();
			bool result = measureTool.MeasureHeight(TrackIndex[CurrentFunction], ref iresult);
			double num = TimeStop();
			lab_Time2.Content = string.Format("{0:N3}", num);
			int num2 = ((measureTool.ScanPtT.Count <= measureTool.ScanPtB.Count) ? measureTool.ScanPtT.Count : measureTool.ScanPtB.Count);
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
			GetTracker(ref rect, ref rect2);
			if (rad_UseAllPt.IsChecked == true)
			{
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				txt_AllResult.Clear();
				string text = "";
				string text2 = "";
				string text3 = "";
				text3 = ((LanguageType == 0) ? "Legth" : ((LanguageType != 1) ? "长度" : "長度"));
				for (int i = 0; i < num2; i++)
				{
					OpenCvSharp.Point pt = measureTool.ScanPtT[i];
					OpenCvSharp.Point pt2 = measureTool.ScanPtB[i];
					if (pt.Y == -1 || pt2.Y == -1 || pt.X == -1 || pt2.X == -1)
					{
						if (chk_CheckEveryOne.IsChecked == true)
						{
							num5++;
							num3++;
							if (num5 > num4)
							{
								num4 = num5;
							}
							text2 = "[NG]";
							text += string.Format("[{0}] {1} = {2}    {3}\n", i + 1, text3, 0, text2);
						}
						continue;
					}
					double num6 = (double)Math.Abs(pt.Y - pt2.Y) * inspection.paramFile.PixelPerUM_Y;
					double num7 = ConvertComDigits(txt_LengthMin, ForSave: false);
					double num8 = ConvertComDigits(txt_LengthMax, ForSave: false);
					if (num6 < num7 || num6 > num8)
					{
						num5++;
						num3++;
						if (chk_ShowScanNG.IsChecked == true)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Red);
						}
						if (num5 > num4)
						{
							num4 = num5;
						}
						text2 = "[NG]";
					}
					else
					{
						if (chk_ShowScanOK.IsChecked == true)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Lime);
						}
						num5 = 0;
						text2 = "";
					}
					text += string.Format("[{0}] {1} = {2}    {3}\n", i + 1, text3, num6, text2);
				}
				text += string.Format("\nMin = {0:N3}\n Max = {1:N3}\nAvg = {2:N3}\nMax Diff = {3:N3}\n", measureTool.MinHeight, measureTool.MaxHeight, measureTool.AvgHeight, measureTool.MaxHeight - measureTool.MinHeight);
				txt_AllResult.Text = text;
				txt_AllResult.ScrollToEnd();
				txt_ContErrorResult.Text = num4.ToString();
				txt_MaxErrorResult.Text = num3.ToString();
				if (num4 > Convert.ToInt32(txt_ContError.Text))
				{
					txt_ContError.Background = Brushes.Red;
				}
				else
				{
					txt_ContError.Background = Brushes.White;
				}
				if (num3 > Convert.ToInt32(txt_MaxError.Text))
				{
					txt_MaxError.Background = Brushes.Red;
				}
				else
				{
					txt_MaxError.Background = Brushes.White;
				}
				SetResult(result);
				if (chk_ShowScanEdge.IsChecked == true)
				{
					for (int j = 0; j < measureTool.ScanPtT.Count - 1; j++)
					{
						OpenCvSharp.Point pt = measureTool.ScanPtT[j];
						OpenCvSharp.Point pt2 = measureTool.ScanPtT[j + 1];
						if (pt.Y != -1 && pt2.Y != -1 && pt.X != -1 && pt2.X != -1)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Blue);
						}
					}
					for (int k = 0; k < measureTool.ScanPtB.Count - 1; k++)
					{
						OpenCvSharp.Point pt = measureTool.ScanPtB[k];
						OpenCvSharp.Point pt2 = measureTool.ScanPtB[k + 1];
						if (pt.Y != -1 && pt2.Y != -1 && pt.X != -1 && pt2.X != -1)
						{
							ShowBitmap.Line(pt, pt2, Scalar.Blue);
						}
					}
				}
				if (measureTool.ScanPtT.Count <= 0)
				{
					txt_AllResult.Clear();
					if (LanguageType == 0)
					{
						txt_AllResult.Text = "Top Side scan pt = 0!";
					}
					else if (LanguageType == 1)
					{
						txt_AllResult.Text = "上邊掃描點為0!";
					}
					else
					{
						txt_AllResult.Text = "上边扫描点为0!";
					}
					return;
				}
				if (measureTool.ScanPtB.Count <= 0)
				{
					txt_AllResult.Clear();
					if (LanguageType == 0)
					{
						txt_AllResult.Text = "bottom Side scan pt = 0!";
					}
					else if (LanguageType == 1)
					{
						txt_AllResult.Text = "下邊掃描點為0!";
					}
					else
					{
						txt_AllResult.Text = "下边扫描点为0!";
					}
					return;
				}
				double num9 = ConvertComDigits(txt_LengthDiffMax, ForSave: false);
				double num10 = measureTool.MaxHeight - measureTool.MinHeight;
				txt_LengthMaxDiffResult.Text = num10.ToString();
				txt_LengthDiffMax.Background = Brushes.White;
				if (num10 > num9 && num9 != 0.0)
				{
					txt_LengthDiffMax.Background = Brushes.Red;
					text = "";
					text = ((LanguageType == 0) ? string.Format("[Measure Height][{0}:{1}] Height min max diff {2:N3} > {3:N3}.\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num10, num9) : ((LanguageType != 1) ? string.Format("[量测高度][{0}:{1}] 高度高低差{2:N3}大于设定值{3:N3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num10, num9) : string.Format("[量測高度][{0}:{1}] 高度高低差{2:N3}大於設定值{3:N3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num10, num9)));
					txt_AllResult.Text += text;
				}
			}
			else
			{
				int num11 = 0;
				num11 = ((inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan1Rule == MeasureCondition.EM_AVG_LENGTH) ? inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan1AvgValue : ((inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan1Rule != MeasureCondition.EM_MIN_LENGTH) ? inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan1MaxValue : inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan1MinValue));
				int num12 = 0;
				num12 = ((inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan2Rule == MeasureCondition.EM_AVG_LENGTH) ? inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan2AvgValue : ((inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan2Rule != MeasureCondition.EM_MIN_LENGTH) ? inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan2MaxValue : inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Scan2MinValue));
				double num13 = (double)Math.Abs(num11 - num12) * inspection.paramFile.PixelPerUM_Y;
				txt_AllResult.Clear();
				if (LanguageType == 0)
				{
					txt_AllResult.Text = string.Format("[Meausre Height][{0}:{1}] Height = {2}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13);
				}
				else if (LanguageType == 1)
				{
					txt_AllResult.Text = string.Format("[量測高度][{0}:{1}] 高度 = {2}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13);
				}
				else
				{
					txt_AllResult.Text = string.Format("[量测高度][{0}:{1}] 高度 = {2}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13);
				}
				txt_SinglePtLengthResult.Text = num13.ToString();
				double num14 = ConvertComDigits(txt_SinglePtLengthMin, ForSave: false);
				double num15 = ConvertComDigits(txt_SinglePtLengthMax, ForSave: false);
				if (num13 < num14)
				{
					txt_SinglePtLengthMin.Background = Brushes.Red;
					string text4 = ((LanguageType == 0) ? string.Format("[Measure Height][{0}:{1}] Height {2} < {3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num14) : ((LanguageType != 1) ? string.Format("[量测高度][{0}:{1}] 高度{2}小于设定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num14) : string.Format("[量測高度][{0}:{1}] 高度{2}小於設定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num14)));
					txt_AllResult.Text += text4;
				}
				if (num13 > num15)
				{
					txt_SinglePtLengthMax.Background = Brushes.Red;
					string text5 = ((LanguageType == 0) ? string.Format("[Measure Height][{0}:{1}] Height {2} > {3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num15) : ((LanguageType != 1) ? string.Format("[量测高度][{0}:{1}] 高度{2}大于设定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num15) : string.Format("[量測高度][{0}:{1}] 高度{2}大於設定值{3}。\n", TrackIndex[CurrentFunction] + 1, txt_TrackerName_W.Text, num13, num15)));
					txt_AllResult.Text += text5;
				}
				OpenCvSharp.Point pt3 = default(OpenCvSharp.Point);
				pt3.X = 0;
				pt3.Y = num11;
				OpenCvSharp.Point pt4 = default(OpenCvSharp.Point);
				pt4.X = ShowBitmap.Width;
				pt4.Y = num11;
				ShowBitmap.Line(pt3, pt4, Scalar.Fuchsia);
				pt3.X = 0;
				pt3.Y = num12;
				pt4.X = ShowBitmap.Width;
				pt4.Y = num12;
				ShowBitmap.Line(pt3, pt4, Scalar.Fuchsia);
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_SearchRect2.Visibility = Visibility.Hidden;
				SetResult(result);
			}
		}
		catch
		{
			string text6 = "MW05-MeasureWindow-ScanHeight()";
			CVBuffer.SaveLog(text6, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text6, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_CorrectTrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Reset Correct Tracker！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("校正区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("校正區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			bor_CorrectRect.Visibility = Visibility.Visible;
			bor_CorrectRect.SetValue(Canvas.LeftProperty, 0.0);
			bor_CorrectRect.SetValue(Canvas.TopProperty, 0.0);
			bor_CorrectRect.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_CorrectRect.SetValue(FrameworkElement.HeightProperty, 100.0);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_CorrectSet_Click(object sender, RoutedEventArgs e)
	{
		bool flag = true;
		if (!IsNumeric(txt_CorrectWidth.Text))
		{
			txt_CorrectWidth.Background = Brushes.Fuchsia;
			flag = false;
		}
		else
		{
			txt_CorrectWidth.Background = Brushes.White;
		}
		if (!IsNumeric(txt_CorrectHeight.Text))
		{
			txt_CorrectHeight.Background = Brushes.Fuchsia;
			flag = false;
		}
		else
		{
			txt_CorrectHeight.Background = Brushes.White;
		}
		if (!flag)
		{
			MessageBox.Show(LanguageManager.Instance["InputError"], "Error");
			rt_CorrectResult.Fill = Brushes.Red;
			return;
		}
		double num = Convert.ToDouble(txt_CorrectWidth.Text);
		double num2 = Convert.ToDouble(txt_CorrectHeight.Text);
		txt_AllResult.Clear();
		if (num <= 0.0 || num2 <= 0.0)
		{
			if (LanguageType == 0)
			{
				txt_AllResult.Text = "Correction Error，Input value  = 0!";
			}
			else if (LanguageType == 1)
			{
				txt_AllResult.Text = "校正失敗，輸入實際量測的寬度高度不得為0!";
			}
			else
			{
				txt_AllResult.Text = "校正失败，输入实际量测的宽度高度不得为0!";
			}
			SetResult(result: false);
			rt_CorrectResult.Fill = Brushes.Red;
			return;
		}
		OpenCvSharp.Rect rect = default(OpenCvSharp.Rect);
		getCorrectRect(ref rect);
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			if (LanguageType == 0)
			{
				txt_AllResult.Text = "Correction Error，Correct Tracker Width and Height = 0!";
			}
			else if (LanguageType == 1)
			{
				txt_AllResult.Text = "校正失敗，校正區域錯誤，寬度高度為0!";
			}
			else
			{
				txt_AllResult.Text = "校正失败，校正区域错误，宽度高度为0!";
			}
			rt_CorrectResult.Fill = Brushes.Red;
			SetResult(result: false);
			return;
		}
		inspection.paramFile.PixelPerUM_X = num / (double)rect.Width;
		inspection.paramFile.PixelPerUM_Y = num2 / (double)rect.Height;
		if (rad_um.IsChecked == true)
		{
			inspection.paramFile.MeasureUnit = 0;
		}
		else
		{
			inspection.paramFile.MeasureUnit = 1;
		}
		rt_CorrectResult.Fill = Brushes.Lime;
		txt_PixelperUmW.Text = string.Format("{0:N5}", inspection.paramFile.PixelPerUM_X);
		txt_PixelperUmH.Text = string.Format("{0:N5}", inspection.paramFile.PixelPerUM_Y);
		inspection.paramFile.PixelPerUM_X = Convert.ToDouble(txt_PixelperUmW.Text);
		inspection.paramFile.PixelPerUM_Y = Convert.ToDouble(txt_PixelperUmH.Text);
		inspection.paramFile.ObjSizeUM_W = Convert.ToDouble(txt_CorrectWidth.Text);
		inspection.paramFile.ObjSizeUM_H = Convert.ToDouble(txt_CorrectHeight.Text);
	}

	private void Bor_SearchRect1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			grd_THD_W1.Background = Brushes.Yellow;
			grd_THD_W2.Background = Brushes.White;
		}
		else if (CurrentFunction == 1)
		{
			grd_THD_H1.Background = Brushes.Yellow;
			grd_THD_H2.Background = Brushes.White;
		}
	}

	private void Bor_SearchRect2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			grd_THD_W1.Background = Brushes.White;
			grd_THD_W2.Background = Brushes.Yellow;
		}
		else if (CurrentFunction == 1)
		{
			grd_THD_H1.Background = Brushes.White;
			grd_THD_H2.Background = Brushes.Yellow;
		}
	}

	private void Btn_ColorDropperL_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (num % 2 == 0)
		{
			TrackerA_Changed = true;
		}
		else
		{
			TrackerA_Changed = false;
		}
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

	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			if (User >= User_Permissions.USER_Administrator || !inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].Locked)
			{
				btn_RecipePasteW.IsEnabled = true;
				CopyParamSetedW = true;
				CopyParamW = inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]];
				btn_RecipeCopyW.Background = Brushes.PaleGreen;
			}
		}
		else if (CurrentFunction == 1 && (User >= User_Permissions.USER_Administrator || !inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].Locked))
		{
			btn_RecipePasteH.IsEnabled = true;
			CopyParamSetedH = true;
			CopyParamH = inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]];
			btn_RecipeCopyH.Background = Brushes.PaleGreen;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			if (CopyParamSetedW)
			{
				MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
				if (messageBoxResult == MessageBoxResult.Yes)
				{
					string trackerName = inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].TrackerName;
					inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]] = CopyParamW;
					inspection.paramFile.MeaWidthParam[TrackIndex[CurrentFunction]].TrackerName = trackerName;
					CopyValueToUI(0);
				}
			}
		}
		else if (CurrentFunction == 1 && CopyParamSetedH)
		{
			MessageBoxResult messageBoxResult2 = ((LanguageType == 0) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult2 == MessageBoxResult.Yes)
			{
				string trackerName2 = inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].TrackerName;
				inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]] = CopyParamH;
				inspection.paramFile.MeaHeightParam[TrackIndex[CurrentFunction]].TrackerName = trackerName2;
				CopyValueToUI(1);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Reverse_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Parameters will Revert to Previous save！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测参数将还原至上次储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測參數將還原至上次儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			if (CurrentFunction == 0)
			{
				inspection.paramFile.MeaWidthParam[TrackIndex[0]] = inspection.paramFile_Backup.MeaWidthParam[TrackIndex[0]];
				CopyValueToUI(0);
			}
			else if (CurrentFunction == 1)
			{
				inspection.paramFile.MeaHeightParam[TrackIndex[1]] = inspection.paramFile_Backup.MeaHeightParam[TrackIndex[0]];
				CopyValueToUI(1);
			}
		}
	}

	private void Cnv_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		cnv_Image.Cursor = Cursors.Arrow;
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				if (CHK_UseRGBChannel[i / 2, i % 2].IsChecked == false)
				{
					num = CurrentColor[THD_Channel[i / 2, i % 2].SelectedIndex];
					num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
					num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
					THD_Sliders[i / 2, i % 2].LowerValue = num2;
					THD_Sliders[i / 2, i % 2].UpperValue = num3;
				}
				else
				{
					num = CurrentColor[0];
					num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
					num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
					THD_Sliders_R[i / 2, i % 2].LowerValue = num2;
					THD_Sliders_R[i / 2, i % 2].UpperValue = num3;
					num = CurrentColor[1];
					num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
					num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
					THD_Sliders_G[i / 2, i % 2].LowerValue = num2;
					THD_Sliders_G[i / 2, i % 2].UpperValue = num3;
					num = CurrentColor[2];
					num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
					num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
					THD_Sliders_B[i / 2, i % 2].LowerValue = num2;
					THD_Sliders_B[i / 2, i % 2].UpperValue = num3;
				}
				ColorDropper[i] = false;
				doThdImage();
			}
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

	private void Rs_ThdW1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 0; i < 2; i++)
		{
			if (ColorDropper[i])
			{
				THD_Channel[0, i].SelectedIndex = THD_Channel[0, num].SelectedIndex;
				THD_Sliders[0, i].LowerValue = THD_Sliders[0, num].LowerValue;
				THD_Sliders[0, i].UpperValue = THD_Sliders[0, num].UpperValue;
				THD_TextLow[0, i].Value = THD_TextLow[0, num].Value;
				THD_TextUp[0, i].Value = THD_TextUp[0, num].Value;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[i] = false;
			}
		}
	}

	private void Rs_ThdH1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((RangeSlider)sender).Uid);
		for (int i = 0; i < 2; i++)
		{
			if (ColorDropper[2 + i])
			{
				THD_Channel[1, i].SelectedIndex = THD_Channel[1, num].SelectedIndex;
				THD_Sliders[1, i].LowerValue = THD_Sliders[1, num].LowerValue;
				THD_Sliders[1, i].UpperValue = THD_Sliders[1, num].UpperValue;
				THD_TextLow[1, i].Value = THD_TextLow[1, num].Value;
				THD_TextUp[1, i].Value = THD_TextUp[1, num].Value;
				cnv_Image.Cursor = Cursors.Arrow;
				ColorDropper[2 + i] = false;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_CorrectReSet_Click(object sender, RoutedEventArgs e)
	{
		bool flag = true;
		if (!IsNumeric(txt_PixelperUmW.Text))
		{
			txt_PixelperUmW.Background = Brushes.Fuchsia;
			flag = false;
		}
		else
		{
			txt_PixelperUmW.Background = Brushes.Yellow;
		}
		if (!IsNumeric(txt_PixelperUmH.Text))
		{
			txt_PixelperUmH.Background = Brushes.Fuchsia;
			flag = false;
		}
		else
		{
			txt_PixelperUmH.Background = Brushes.Yellow;
		}
		if (!flag)
		{
			MessageBox.Show(LanguageManager.Instance["InputError"], "Error");
			rt_CorrectResult.Fill = Brushes.Red;
			return;
		}
		inspection.paramFile.PixelPerUM_X = Convert.ToDouble(txt_PixelperUmW.Text);
		inspection.paramFile.PixelPerUM_Y = Convert.ToDouble(txt_PixelperUmH.Text);
		inspection.paramFile.ObjSizeUM_W = Convert.ToDouble(txt_CorrectWidth.Text);
		inspection.paramFile.ObjSizeUM_H = Convert.ToDouble(txt_CorrectHeight.Text);
		rt_CorrectResult.Fill = Brushes.Lime;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Rad_um_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			if (rad_um.IsChecked == true)
			{
				inspection.paramFile.MeasureUnit = 0;
			}
			else
			{
				inspection.paramFile.MeasureUnit = 1;
			}
			lab_unit1.Content = "um";
			lab_unit2.Content = "um";
			lab_unit3.Content = "um";
			lab_unit4.Content = "um";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Rad_mm_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			if (rad_um.IsChecked == true)
			{
				inspection.paramFile.MeasureUnit = 0;
			}
			else
			{
				inspection.paramFile.MeasureUnit = 1;
			}
			lab_unit1.Content = "mm";
			lab_unit2.Content = "mm";
			lab_unit3.Content = "mm";
			lab_unit4.Content = "mm";
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
		TrackerA_Changed = true;
	}

	private void Bor_SearchRect2_GotFocus(object sender, RoutedEventArgs e)
	{
		CurrentTracker = 1;
		TrackerA_Changed = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerMoveUp_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		string uid;
		if (CurrentTracker == 0)
		{
			int num = (int)Canvas.GetLeft(bor_SearchRect1);
			int num2 = (int)Canvas.GetTop(bor_SearchRect1);
			int num3 = (int)bor_SearchRect1.Width;
			int num4 = (int)bor_SearchRect1.Height;
			uid = button.Uid;
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
							num3 = (int)bor_SearchRect2.Width;
							num4 = (int)bor_SearchRect2.Height;
						}
						break;
					case '3':
						if (uid == "13")
						{
							num2 = (int)Canvas.GetTop(bor_SearchRect2);
						}
						break;
					case '4':
						if (uid == "14")
						{
							num2 = (int)Canvas.GetTop(bor_SearchRect2) + (int)bor_SearchRect2.Height - num4;
						}
						break;
					case '5':
						if (uid == "15")
						{
							num = (int)Canvas.GetLeft(bor_SearchRect2);
						}
						break;
					case '6':
						if (uid == "16")
						{
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
			return;
		}
		int num5 = (int)Canvas.GetLeft(bor_SearchRect2);
		int num6 = (int)Canvas.GetTop(bor_SearchRect2);
		int num7 = (int)bor_SearchRect2.Width;
		int num8 = (int)bor_SearchRect2.Height;
		uid = button.Uid;
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
						num7 = (int)bor_SearchRect1.Width;
						num8 = (int)bor_SearchRect1.Height;
					}
					break;
				case '3':
					if (uid == "13")
					{
						num6 = (int)Canvas.GetTop(bor_SearchRect1);
					}
					break;
				case '4':
					if (uid == "14")
					{
						num6 = (int)Canvas.GetTop(bor_SearchRect1) + (int)bor_SearchRect1.Height - num8;
					}
					break;
				case '5':
					if (uid == "15")
					{
						num5 = (int)Canvas.GetLeft(bor_SearchRect1);
					}
					break;
				case '6':
					if (uid == "16")
					{
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
	}

	private void Nu_ThdW1LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdW1.LowerValue = (int)nu_ThdW1LimitMin.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdW1LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdW1.UpperValue = (int)nu_ThdW1LimitMax.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdW2LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdW2.LowerValue = (int)nu_ThdW2LimitMin.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdW2LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdW2.UpperValue = (int)nu_ThdW2LimitMax.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdH1LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdH1.LowerValue = (int)nu_ThdH1LimitMin.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdH1LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdH1.UpperValue = (int)nu_ThdH1LimitMax.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdH2LimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdH2.LowerValue = (int)nu_ThdH2LimitMin.Value.Value;
			doThdImage();
		}
	}

	private void Nu_ThdH2LimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited)
		{
			rs_ThdH2.UpperValue = (int)nu_ThdH2LimitMax.Value.Value;
			doThdImage();
		}
	}

	private void Txt_LengthMin_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
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
		hKeyboard.SetInputNegative(state: false);
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

	private void Txt_ContError_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
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

	private void Cnv_Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < 4; i++)
		{
			if (ColorDropper[i])
			{
				num = CurrentColor[THD_Channel[i / 2, i % 2].SelectedIndex];
				num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
				num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
				THD_Sliders[i / 2, i % 2].LowerValue = num2;
				THD_Sliders[i / 2, i % 2].UpperValue = num3;
				doThdImage();
			}
		}
		if (sfr.ScaleX != 1.0)
		{
			System.Windows.Point position = e.GetPosition(cnv_Image);
			sfr.CenterX = position.X;
			sfr.CenterY = position.Y;
		}
	}

	private void Btn_Tools1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (CurrentFunction == 0)
		{
			cb_TrackerSelect_W.SelectedIndex = num;
		}
		else
		{
			cb_TrackerSelect_H.SelectedIndex = num;
		}
		for (int i = 0; i < 20; i++)
		{
			ToolsButton[i].BorderBrush = Brushes.Gray;
		}
		ToolsButton[num].BorderBrush = Brushes.Blue;
	}

	private void Btn_ChartM_Click(object sender, RoutedEventArgs e)
	{
		if (tab_AdvMeasureFun.SelectedIndex == 0)
		{
			if (cb_TrackerSelect_W.SelectedIndex >= 0 && cb_TrackerSelect_W.SelectedIndex < 20)
			{
				_ = (Button)sender;
				ChartWindow chartWindow = new ChartWindow();
				chartWindow.SetInspection(inspection);
				chartWindow.LoadLanguage(LanguageType);
				chartWindow.SetStatistics(3, 0, cb_TrackerSelect_W.SelectedIndex);
				chartWindow.ShowDialog();
			}
		}
		else if (tab_AdvMeasureFun.SelectedIndex == 1 && cb_TrackerSelect_H.SelectedIndex >= 0 && cb_TrackerSelect_H.SelectedIndex < 20)
		{
			_ = (Button)sender;
			ChartWindow chartWindow2 = new ChartWindow();
			chartWindow2.SetInspection(inspection);
			chartWindow2.LoadLanguage(LanguageType);
			chartWindow2.SetStatistics(3, 1, cb_TrackerSelect_H.SelectedIndex);
			chartWindow2.ShowDialog();
		}
	}

	private void getCorrectRect(ref OpenCvSharp.Rect rect1)
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			rect1.X = (int)(Canvas.GetLeft(bor_CorrectRect) / xRatio);
			rect1.Y = (int)(Canvas.GetTop(bor_CorrectRect) / yRatio);
			rect1.Width = (int)(bor_CorrectRect.Width / xRatio);
			rect1.Height = (int)(bor_CorrectRect.Height / yRatio);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		WriteUIValueToParam(tab_AdvMeasureFun.SelectedIndex, TrackIndex[CurrentFunction]);
		if (chk_SelectFace.IsChecked == true && FaceIndex != 0 && FaceIndex != SelectFace)
		{
			Label_ResultError.Visibility = Visibility.Visible;
			txt_AllResult.Clear();
			if (LanguageType == 0)
			{
				Label_ResultError.Text = "Select Face Doesn't Match! ByPass!";
			}
			else if (LanguageType == 1)
			{
				Label_ResultError.Text = "選面不符合，ByPass!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Text = "选面不符合，ByPass!";
			}
			lab_Time1.Content = "0";
			lab_Time2.Content = "0";
			return;
		}
		Label_ResultError.Visibility = Visibility.Hidden;
		if (CurrentFunction == 0)
		{
			if (ts_TrackerON_W.IsChecked != true)
			{
				DrawBackGround();
				return;
			}
			ScanWidth();
		}
		else if (CurrentFunction == 1)
		{
			if (ts_TrackerON_H.IsChecked != true)
			{
				DrawBackGround();
				return;
			}
			ScanHeight();
		}
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
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
			CVBuffer.SaveLog(string.Concat(string.Format("MeasureWindows-Type{0}", tab_AdvMeasureFun.SelectedIndex) + "MT12-MeasureWindows-GetComDigitsStr()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
			CVBuffer.SaveLog(string.Concat(string.Format("MeasureWindows-Type{0}", tab_AdvMeasureFun.SelectedIndex) + "MT12-MeasureWindows-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
			CVBuffer.SaveLog(string.Concat(string.Format("MeasureWindows-Type{0}", tab_AdvMeasureFun.SelectedIndex) + "MT13-MeasureWindows-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Rs_ThdW1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		ThdAPress = true;
	}

	private void Rs_ThdW1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		ThdAPress = false;
	}

	private void Rs_ThdW2_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		ThdBPress = true;
	}

	private void Rs_ThdW2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		ThdBPress = false;
	}

	private void Btn_ScanL2RW1_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanL2RW1.IsChecked = true;
	}

	private void Btn_ScanR2LW1_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LW1.IsChecked = true;
	}

	private void Btn_ScanL2RW2_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanL2RW2.IsChecked = true;
	}

	private void Btn_ScanR2LW2_Click(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LW2.IsChecked = true;
	}

	private void Btn_ScanL2RH2_Click_1(object sender, RoutedEventArgs e)
	{
		rad_ScanL2RH2.IsChecked = true;
	}

	private void Btn_ScanR2LH2_Click_1(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LH2.IsChecked = true;
	}

	private void Chk_EnableRGBThd_HA_Click(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		int selectedIndex = tab_AdvMeasureFun.SelectedIndex;
		if (selectedIndex < 2)
		{
			if (CHK_UseRGBChannel[selectedIndex, 0].IsChecked == true)
			{
				GRD_RGBThd[selectedIndex, 0].Visibility = Visibility.Visible;
			}
			else
			{
				GRD_RGBThd[selectedIndex, 0].Visibility = Visibility.Hidden;
			}
			if (CHK_UseRGBChannel[selectedIndex, 1].IsChecked == true)
			{
				GRD_RGBThd[selectedIndex, 1].Visibility = Visibility.Visible;
			}
			else
			{
				GRD_RGBThd[selectedIndex, 1].Visibility = Visibility.Hidden;
			}
		}
	}

	private void Rad_ScanL2RW2_Unchecked(object sender, RoutedEventArgs e)
	{
		rad_ScanR2LW2.IsChecked = true;
	}

	private void Rs_THD_WA_R_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			TrackerA_Changed = true;
			doThdImage();
		}
	}

	private void Rs_THD_HB_R_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			TrackerA_Changed = false;
			doThdImage();
		}
	}

	private void Cnv_Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (inited && tab_AdvMeasureFun.SelectedIndex < 2)
		{
			doThdImage();
		}
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
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteModifyLog(int index)
	{
		ModifyLog = "";
		INSP_MEASURETOOL_PARAM param;
		INSP_MEASURETOOL_PARAM backupParam;
		if (CurrentFunction == 0)
		{
			param = inspection.paramFile.MeaWidthParam[index];
			backupParam = BackupParamW;
		}
		else
		{
			param = inspection.paramFile.MeaHeightParam[index];
			backupParam = BackupParamH;
		}
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
			string text4 = ((CurrentFunction != 0) ? string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["MeasureTool2"], index + 1, param.TrackerName) : string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["MeasureTool1"], index + 1, param.TrackerName));
			if (param.TrackerON != backupParam.TrackerON)
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
			if (param.THD1_ChannelIndex != backupParam.THD1_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)backupParam.THD1_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD1_ChannelIndex));
			}
			if (param.THD1_Min != backupParam.THD1_Min)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], backupParam.THD1_Min, LanguageManager.Instance["ChangedTo"], param.THD1_Min);
			}
			if (param.THD1_Max != backupParam.THD1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], backupParam.THD1_Max, LanguageManager.Instance["ChangedTo"], param.THD1_Max);
			}
			if (param.THD2_ChannelIndex != backupParam.THD2_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)backupParam.THD2_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD2_ChannelIndex));
			}
			if (param.THD2_Min != backupParam.THD2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], backupParam.THD2_Min, LanguageManager.Instance["ChangedTo"], param.THD2_Min);
			}
			if (param.THD2_Max != backupParam.THD2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], backupParam.THD2_Max, LanguageManager.Instance["ChangedTo"], param.THD2_Max);
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start1_Index, (int)backupParam.DE_Start1_Index, param.DE_Start1_Count, backupParam.DE_Start1_Count, 1, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End1_Index, (int)backupParam.DE_End1_Index, param.DE_End1_Count, backupParam.DE_End1_Count, 1, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start2_Index, (int)backupParam.DE_Start2_Index, param.DE_Start2_Count, backupParam.DE_Start2_Count, 2, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End2_Index, (int)backupParam.DE_End2_Index, param.DE_End2_Count, backupParam.DE_End2_Count, 2, 2, text4);
			GetTrackModifyText(text4, param, backupParam);
			if (param.Direction1 != backupParam.Direction1)
			{
				if (param.Direction1 == ScanDirection.EM_LEFT_RIGHT)
				{
					ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Right2Left"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Left2Right"]);
				}
				else if (param.Direction1 == ScanDirection.EM_RIGHT_LEFT)
				{
					ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Left2Right"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Right2Left"]);
				}
				else if (param.Direction1 == ScanDirection.EM_TOP_BOTTOM)
				{
					ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Bottom2Top"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Top2Bottom"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Top2Bottom"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Bottom2Top"]);
				}
			}
			if (param.Direction2 != backupParam.Direction2)
			{
				if (param.Direction2 == ScanDirection.EM_LEFT_RIGHT)
				{
					ModifyLog += string.Format("{0} [({1}A) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Right2Left"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Left2Right"]);
				}
				else if (param.Direction2 == ScanDirection.EM_RIGHT_LEFT)
				{
					ModifyLog += string.Format("{0} [({1}A) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Left2Right"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Right2Left"]);
				}
				else if (param.Direction2 == ScanDirection.EM_TOP_BOTTOM)
				{
					ModifyLog += string.Format("{0} [({1}B) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Bottom2Top"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Top2Bottom"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}B) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], LanguageManager.Instance["Top2Bottom"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Bottom2Top"]);
				}
			}
			if (param.UseSinglePt != backupParam.UseSinglePt)
			{
				if (!param.UseSinglePt)
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["CheckOnePtMode"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["FullCheckMode"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["FullCheckMode"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["CheckOnePtMode"]);
				}
			}
			if (param.CheckEveryone != backupParam.CheckEveryone)
			{
				if (!param.CheckEveryone)
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["CheckEveryOne"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["CheckEveryOne"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
				}
			}
			if (param.LengthMin != backupParam.LengthMin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["MinLength"], GetComDigitsStr(backupParam.LengthMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.LengthMin));
			}
			if (param.LengthMax != backupParam.LengthMax)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["MaxLength"], GetComDigitsStr(backupParam.LengthMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.LengthMax));
			}
			if (param.LengthDiffMax != backupParam.LengthDiffMax)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["LengthMaxDiff"], GetComDigitsStr(backupParam.LengthDiffMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.LengthDiffMax));
			}
			if (param.ContinueError != backupParam.ContinueError)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["ContinueError"], backupParam.ContinueError, LanguageManager.Instance["ChangedTo"], param.ContinueError);
			}
			if (param.MaxError != backupParam.MaxError)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["TotalError"], backupParam.MaxError, LanguageManager.Instance["ChangedTo"], param.MaxError);
			}
			if (param.Scan1Rule != backupParam.Scan1Rule)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectScanPtA"], modifyLogFun.GetScanRuleText((int)backupParam.Scan1Rule), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetScanRuleText((int)param.Scan1Rule));
			}
			if (param.Scan2Rule != backupParam.Scan2Rule)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectScanPtA"], modifyLogFun.GetScanRuleText((int)backupParam.Scan2Rule), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetScanRuleText((int)param.Scan2Rule));
			}
			if (param.SinglePtLengthMin != backupParam.SinglePtLengthMin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SingleMinLength"], GetComDigitsStr(backupParam.SinglePtLengthMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.SinglePtLengthMin));
			}
			if (param.SinglePtLengthMax != backupParam.SinglePtLengthMax)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SingleMaxLength"], GetComDigitsStr(backupParam.SinglePtLengthMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.SinglePtLengthMax));
			}
			if (param.ErrorBin != backupParam.ErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)backupParam.ErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)param.ErrorBin));
			}
			if (param.EnableSelectFace != backupParam.EnableSelectFace)
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
			if (param.FaceIndex != backupParam.FaceIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectFaceTool"], modifyLogFun.GetSelectFaceText(backupParam.FaceIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetSelectFaceText(param.FaceIndex));
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
			string text5 = "MW10-MeasureWindow-WriteModifyLog()-";
			text5 += ex.ToString();
			CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text5, "Log寫入錯誤");
			}
		}
	}

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			doThdImage();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTrackModifyText(string Title, INSP_MEASURETOOL_PARAM param, INSP_MEASURETOOL_PARAM BackupParam)
	{
		if (param.SearchRect1L != BackupParam.SearchRect1L || param.SearchRect1R != BackupParam.SearchRect1R || param.SearchRect1T != BackupParam.SearchRect1T || param.SearchRect1B != BackupParam.SearchRect1B)
		{
			ModifyLog += string.Format("{0} [(A{1}) L:{2} R:{3} T:{4} B:{5} {6} L:{7} R:{8} T:{9} B:{10}] \n", Title, LanguageManager.Instance["TrackPos"], BackupParam.SearchRect1L, BackupParam.SearchRect1R, BackupParam.SearchRect1T, BackupParam.SearchRect1B, LanguageManager.Instance["ChangedTo"], param.SearchRect1L, param.SearchRect1R, param.SearchRect1T, param.SearchRect1B);
		}
		if (param.SearchRect2L != BackupParam.SearchRect2L || param.SearchRect2R != BackupParam.SearchRect2R || param.SearchRect2T != BackupParam.SearchRect2T || param.SearchRect2B != BackupParam.SearchRect2B)
		{
			ModifyLog += string.Format("{0} [(B{1}) L:{2} R:{3} T:{4} B:{5} {6} L:{7} R:{8} T:{9} B:{10}] \n", Title, LanguageManager.Instance["TrackPos"], BackupParam.SearchRect2L, BackupParam.SearchRect2R, BackupParam.SearchRect2T, BackupParam.SearchRect2B, LanguageManager.Instance["ChangedTo"], param.SearchRect2L, param.SearchRect2R, param.SearchRect2T, param.SearchRect2B);
		}
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/measurewindow.xaml", UriKind.Relative);
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
			MainW = (MeasureWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			cnv_Image.PreviewMouseRightButtonDown += Cnv_Image_PreviewMouseRightButtonDown;
			cnv_Image.MouseLeftButtonUp += Cnv_Image_MouseLeftButtonUp;
			break;
		case 3:
			sfr = (ScaleTransform)target;
			break;
		case 4:
			Label_Result = (Label)target;
			break;
		case 5:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect1.GotFocus += Bor_SearchRect1_GotFocus;
			break;
		case 6:
			txt_Search1Label = (TextBlock)target;
			break;
		case 7:
			bor_SearchRect2 = (Border)target;
			bor_SearchRect2.PreviewMouseLeftButtonDown += Bor_SearchRect2_PreviewMouseLeftButtonDown;
			bor_SearchRect2.GotFocus += Bor_SearchRect2_GotFocus;
			break;
		case 8:
			txt_Search1Label1 = (TextBlock)target;
			break;
		case 9:
			bor_CorrectRect = (Rectangle)target;
			break;
		case 10:
			lab_FaceIndex = (Label)target;
			break;
		case 11:
			Label_ResultError = (TextBox)target;
			break;
		case 12:
			txt_PreviewMode = (TextBox)target;
			break;
		case 13:
			tab_AdvMeasureFun = (TabControl)target;
			tab_AdvMeasureFun.SelectionChanged += Tab_AdvMeasureFun_SelectionChanged;
			break;
		case 14:
			tab_W = (TabItem)target;
			break;
		case 15:
			grd_MainW = (Grid)target;
			break;
		case 16:
			cb_TrackerSelect_W = (ComboBox)target;
			cb_TrackerSelect_W.SelectionChanged += Cb_TrackerSelect_W_SelectionChanged;
			break;
		case 17:
			txt_TrackerName_W = (TextBox)target;
			break;
		case 18:
			scr_TrackerSwich_W = (ScrollBar)target;
			scr_TrackerSwich_W.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 19:
			ts_TrackerON_W = (ToggleSwitch)target;
			ts_TrackerON_W.Click += Ts_TrackerON_Click;
			break;
		case 20:
			btn_ReverseW = (Button)target;
			btn_ReverseW.Click += Btn_Reverse_Click;
			break;
		case 21:
			btn_RecipeCopyW = (Button)target;
			btn_RecipeCopyW.Click += Btn_RecipeCopy_Click;
			break;
		case 22:
			btn_RecipePasteW = (Button)target;
			btn_RecipePasteW.Click += Btn_RecipePaste_Click;
			break;
		case 23:
			lab_Pareto = (Label)target;
			break;
		case 24:
			cb_ParetoChartW = (ComboBox)target;
			break;
		case 25:
			lab_Time1 = (Label)target;
			break;
		case 26:
			gb_WTHD1 = (GroupBox)target;
			break;
		case 27:
			grd_THD_W1 = (Grid)target;
			break;
		case 28:
			rs_ThdW1 = (RangeSlider)target;
			rs_ThdW1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdW1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdW1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdW1.PreviewMouseLeftButtonUp += Rs_ThdW1_PreviewMouseLeftButtonUp;
			rs_ThdW1.PreviewMouseDown += Rs_ThdW1_PreviewMouseDown;
			rs_ThdW1.PreviewMouseUp += Rs_ThdW1_PreviewMouseUp;
			break;
		case 29:
			grd_SIMP_DilateErode4 = (Grid)target;
			break;
		case 30:
			cb_ThdDEStartW1 = (ComboBox)target;
			break;
		case 31:
			cb_ThdDEEndW1 = (ComboBox)target;
			break;
		case 32:
			cb_ThdDEEndCountW1 = (ComboBox)target;
			break;
		case 33:
			cb_ThdDEStartCountW1 = (ComboBox)target;
			break;
		case 34:
			cb_ChannelW1 = (ComboBox)target;
			break;
		case 35:
			rad_ScanL2RW1 = (RadioButton)target;
			rad_ScanL2RW1.Unchecked += Rad_ScanL2RW1_Unchecked;
			break;
		case 36:
			rad_ScanR2LW1 = (RadioButton)target;
			break;
		case 37:
			btn_ScanL2RW1 = (Button)target;
			btn_ScanL2RW1.Click += Btn_ScanL2RW1_Click;
			break;
		case 38:
			btn_ScanR2LW1 = (Button)target;
			btn_ScanR2LW1.Click += Btn_ScanR2LW1_Click;
			break;
		case 39:
			nu_ThdW1LimitMin = (NumericUpDown)target;
			nu_ThdW1LimitMin.ValueChanged += Nu_ThdW1LimitMin_ValueChanged;
			break;
		case 40:
			nu_ThdW1LimitMax = (NumericUpDown)target;
			nu_ThdW1LimitMax.ValueChanged += Nu_ThdW1LimitMax_ValueChanged;
			break;
		case 41:
			grd_RGBThd_WA = (Grid)target;
			break;
		case 42:
			rs_THD_WA_R = (RangeSlider)target;
			rs_THD_WA_R.CentralThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_WA_R.UpperThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_WA_R.LowerThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			break;
		case 43:
			rs_THD_WA_G = (RangeSlider)target;
			rs_THD_WA_G.CentralThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_WA_G.UpperThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_WA_G.LowerThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			break;
		case 44:
			rs_THD_WA_B = (RangeSlider)target;
			rs_THD_WA_B.CentralThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_WA_B.UpperThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_WA_B.LowerThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			break;
		case 45:
			lab_THD_WA_R_Min = (Label)target;
			break;
		case 46:
			lab_THD_WA_R_Max = (Label)target;
			break;
		case 47:
			lab_THD_WA_G_Min = (Label)target;
			break;
		case 48:
			lab_THD_WA_G_Max = (Label)target;
			break;
		case 49:
			lab_THD_WA_B_Min = (Label)target;
			break;
		case 50:
			lab_THD_WA_B_Max = (Label)target;
			break;
		case 51:
			chk_EnableRGBThd_WA = (CheckBox)target;
			chk_EnableRGBThd_WA.Click += Chk_EnableRGBThd_HA_Click;
			break;
		case 52:
			btn_ColorDropperW1 = (Button)target;
			btn_ColorDropperW1.Click += Btn_ColorDropperL_Click;
			break;
		case 53:
			gb_WTHD2 = (GroupBox)target;
			break;
		case 54:
			grd_THD_W2 = (Grid)target;
			break;
		case 55:
			rs_ThdW2 = (RangeSlider)target;
			rs_ThdW2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdW2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdW2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdW2.PreviewMouseLeftButtonUp += Rs_ThdW1_PreviewMouseLeftButtonUp;
			rs_ThdW2.PreviewMouseDown += Rs_ThdW2_PreviewMouseDown;
			rs_ThdW2.PreviewMouseUp += Rs_ThdW2_PreviewMouseUp;
			break;
		case 56:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 57:
			cb_ThdDEStartW2 = (ComboBox)target;
			break;
		case 58:
			cb_ThdDEEndW2 = (ComboBox)target;
			break;
		case 59:
			cb_ThdDEEndCountW2 = (ComboBox)target;
			break;
		case 60:
			cb_ThdDEStartCountW2 = (ComboBox)target;
			break;
		case 61:
			cb_ChannelW2 = (ComboBox)target;
			break;
		case 62:
			nu_ThdW2LimitMin = (NumericUpDown)target;
			nu_ThdW2LimitMin.ValueChanged += Nu_ThdW2LimitMin_ValueChanged;
			break;
		case 63:
			nu_ThdW2LimitMax = (NumericUpDown)target;
			nu_ThdW2LimitMax.ValueChanged += Nu_ThdW2LimitMax_ValueChanged;
			break;
		case 64:
			rad_ScanL2RW2 = (RadioButton)target;
			rad_ScanL2RW2.Unchecked += Rad_ScanL2RW2_Unchecked;
			break;
		case 65:
			rad_ScanR2LW2 = (RadioButton)target;
			break;
		case 66:
			btn_ScanL2RW2 = (Button)target;
			btn_ScanL2RW2.Click += Btn_ScanL2RW2_Click;
			break;
		case 67:
			btn_ScanR2LW2 = (Button)target;
			btn_ScanR2LW2.Click += Btn_ScanR2LW2_Click;
			break;
		case 68:
			grd_RGBThd_WB = (Grid)target;
			break;
		case 69:
			rs_THD_WB_R = (RangeSlider)target;
			rs_THD_WB_R.CentralThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_WB_R.UpperThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_WB_R.LowerThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			break;
		case 70:
			rs_THD_WB_G = (RangeSlider)target;
			rs_THD_WB_G.CentralThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_WB_G.UpperThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_WB_G.LowerThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			break;
		case 71:
			rs_THD_WB_B = (RangeSlider)target;
			rs_THD_WB_B.CentralThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_WB_B.UpperThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_WB_B.LowerThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			break;
		case 72:
			lab_THD_WB_R_Min = (Label)target;
			break;
		case 73:
			lab_THD_WB_R_Max = (Label)target;
			break;
		case 74:
			lab_THD_WB_G_Min = (Label)target;
			break;
		case 75:
			lab_THD_WB_G_Max = (Label)target;
			break;
		case 76:
			lab_THD_WB_B_Min = (Label)target;
			break;
		case 77:
			lab_THD_WB_B_Max = (Label)target;
			break;
		case 78:
			btn_ColorDropperW2 = (Button)target;
			btn_ColorDropperW2.Click += Btn_ColorDropperL_Click;
			break;
		case 79:
			chk_EnableRGBThd_WB = (CheckBox)target;
			chk_EnableRGBThd_WB.Click += Chk_EnableRGBThd_HA_Click;
			break;
		case 80:
			tab_H = (TabItem)target;
			break;
		case 81:
			grd_MainH = (Grid)target;
			break;
		case 82:
			cb_TrackerSelect_H = (ComboBox)target;
			cb_TrackerSelect_H.SelectionChanged += Cb_TrackerSelect_H_SelectionChanged;
			break;
		case 83:
			txt_TrackerName_H = (TextBox)target;
			break;
		case 84:
			scr_TrackerSwich_H = (ScrollBar)target;
			scr_TrackerSwich_H.ValueChanged += Scr_TrackerSwich_H_ValueChanged_1;
			break;
		case 85:
			ts_TrackerON_H = (ToggleSwitch)target;
			ts_TrackerON_H.Click += Ts_TrackerON_Click;
			break;
		case 86:
			btn_ReverseH = (Button)target;
			btn_ReverseH.Click += Btn_Reverse_Click;
			break;
		case 87:
			btn_RecipeCopyH = (Button)target;
			btn_RecipeCopyH.Click += Btn_RecipeCopy_Click;
			break;
		case 88:
			btn_RecipePasteH = (Button)target;
			btn_RecipePasteH.Click += Btn_RecipePaste_Click;
			break;
		case 89:
			lab_Pareto_Copy = (Label)target;
			break;
		case 90:
			cb_ParetoChartH = (ComboBox)target;
			break;
		case 91:
			lab_Time2 = (Label)target;
			break;
		case 92:
			gb_HTHD1 = (GroupBox)target;
			break;
		case 93:
			grd_THD_H1 = (Grid)target;
			break;
		case 94:
			rs_ThdH1 = (RangeSlider)target;
			rs_ThdH1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdH1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdH1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdH1.PreviewMouseLeftButtonUp += Rs_ThdH1_PreviewMouseLeftButtonUp;
			rs_ThdH1.PreviewMouseDown += Rs_ThdW1_PreviewMouseDown;
			rs_ThdH1.PreviewMouseUp += Rs_ThdW1_PreviewMouseUp;
			break;
		case 95:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 96:
			cb_ThdDEStartH1 = (ComboBox)target;
			break;
		case 97:
			cb_ThdDEEndH1 = (ComboBox)target;
			break;
		case 98:
			cb_ThdDEEndCountH1 = (ComboBox)target;
			break;
		case 99:
			cb_ThdDEStartCountH1 = (ComboBox)target;
			break;
		case 100:
			cb_ChannelH1 = (ComboBox)target;
			break;
		case 101:
			nu_ThdH1LimitMin = (NumericUpDown)target;
			nu_ThdH1LimitMin.ValueChanged += Nu_ThdH1LimitMin_ValueChanged;
			break;
		case 102:
			nu_ThdH1LimitMax = (NumericUpDown)target;
			nu_ThdH1LimitMax.ValueChanged += Nu_ThdH1LimitMax_ValueChanged;
			break;
		case 103:
			rad_ScanL2RH1 = (RadioButton)target;
			rad_ScanL2RH1.Unchecked += Rad_ScanL2RH1_Unchecked;
			break;
		case 104:
			rad_ScanR2LH1 = (RadioButton)target;
			break;
		case 105:
			btn_ScanL2RH1 = (Button)target;
			btn_ScanL2RH1.Click += Btn_ScanL2R_Click;
			break;
		case 106:
			btn_ScanR2LH1 = (Button)target;
			btn_ScanR2LH1.Click += Btn_ScanR2L_Click;
			break;
		case 107:
			grd_RGBThd_HA = (Grid)target;
			break;
		case 108:
			rs_THD_HA_R = (RangeSlider)target;
			rs_THD_HA_R.CentralThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_HA_R.UpperThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_HA_R.LowerThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			break;
		case 109:
			rs_THD_HA_G = (RangeSlider)target;
			rs_THD_HA_G.CentralThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_HA_G.UpperThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_HA_G.LowerThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			break;
		case 110:
			rs_THD_HA_B = (RangeSlider)target;
			rs_THD_HA_B.CentralThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_HA_B.UpperThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			rs_THD_HA_B.LowerThumbDragDelta += Rs_THD_WA_R_CentralThumbDragDelta;
			break;
		case 111:
			lab_THD_HA_R_Min = (Label)target;
			break;
		case 112:
			lab_THD_HA_R_Max = (Label)target;
			break;
		case 113:
			lab_THD_HA_G_Min = (Label)target;
			break;
		case 114:
			lab_THD_HA_G_Max = (Label)target;
			break;
		case 115:
			lab_THD_HA_B_Min = (Label)target;
			break;
		case 116:
			lab_THD_HA_B_Max = (Label)target;
			break;
		case 117:
			btn_ColorDropperH1 = (Button)target;
			btn_ColorDropperH1.Click += Btn_ColorDropperL_Click;
			break;
		case 118:
			chk_EnableRGBThd_HA = (CheckBox)target;
			chk_EnableRGBThd_HA.Click += Chk_EnableRGBThd_HA_Click;
			break;
		case 119:
			gb_HTHD2 = (GroupBox)target;
			break;
		case 120:
			grd_THD_H2 = (Grid)target;
			break;
		case 121:
			rs_ThdH2 = (RangeSlider)target;
			rs_ThdH2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdH2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdH2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdH2.PreviewMouseLeftButtonUp += Rs_ThdH1_PreviewMouseLeftButtonUp;
			rs_ThdH2.PreviewMouseDown += Rs_ThdW2_PreviewMouseDown;
			rs_ThdH2.PreviewMouseUp += Rs_ThdW2_PreviewMouseUp;
			break;
		case 122:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 123:
			cb_ThdDEStartH2 = (ComboBox)target;
			break;
		case 124:
			cb_ThdDEEndH2 = (ComboBox)target;
			break;
		case 125:
			cb_ThdDEEndCountH2 = (ComboBox)target;
			break;
		case 126:
			cb_ThdDEStartCountH2 = (ComboBox)target;
			break;
		case 127:
			cb_ChannelH2 = (ComboBox)target;
			break;
		case 128:
			nu_ThdH2LimitMin = (NumericUpDown)target;
			nu_ThdH2LimitMin.ValueChanged += Nu_ThdH2LimitMin_ValueChanged;
			break;
		case 129:
			nu_ThdH2LimitMax = (NumericUpDown)target;
			nu_ThdH2LimitMax.ValueChanged += Nu_ThdH2LimitMax_ValueChanged;
			break;
		case 130:
			rad_ScanL2RH2 = (RadioButton)target;
			rad_ScanL2RH2.Unchecked += Rad_ScanL2RH2_Unchecked;
			break;
		case 131:
			rad_ScanR2LH2 = (RadioButton)target;
			break;
		case 132:
			btn_ScanL2RH2 = (Button)target;
			btn_ScanL2RH2.Click += Btn_ScanL2RH2_Click_1;
			break;
		case 133:
			btn_ScanR2LH2 = (Button)target;
			btn_ScanR2LH2.Click += Btn_ScanR2LH2_Click_1;
			break;
		case 134:
			grd_RGBThd_HB = (Grid)target;
			break;
		case 135:
			rs_THD_HB_R = (RangeSlider)target;
			rs_THD_HB_R.CentralThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_HB_R.LowerThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_HB_R.UpperThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			break;
		case 136:
			rs_THD_HB_G = (RangeSlider)target;
			rs_THD_HB_G.CentralThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_HB_G.LowerThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_HB_G.UpperThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			break;
		case 137:
			rs_THD_HB_B = (RangeSlider)target;
			rs_THD_HB_B.CentralThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_HB_B.LowerThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			rs_THD_HB_B.UpperThumbDragDelta += Rs_THD_HB_R_CentralThumbDragDelta;
			break;
		case 138:
			lab_THD_HB_R_Min = (Label)target;
			break;
		case 139:
			lab_THD_HB_R_Max = (Label)target;
			break;
		case 140:
			lab_THD_HB_G_Min = (Label)target;
			break;
		case 141:
			lab_THD_HB_G_Max = (Label)target;
			break;
		case 142:
			lab_THD_HB_B_Min = (Label)target;
			break;
		case 143:
			lab_THD_HB_B_Max = (Label)target;
			break;
		case 144:
			btn_ColorDropperH2 = (Button)target;
			btn_ColorDropperH2.Click += Btn_ColorDropperL_Click;
			break;
		case 145:
			chk_EnableRGBThd_HB = (CheckBox)target;
			chk_EnableRGBThd_HB.Click += Chk_EnableRGBThd_HA_Click;
			break;
		case 146:
			tab_C = (TabItem)target;
			break;
		case 147:
			btn_CorrectTrackerReset = (Button)target;
			btn_CorrectTrackerReset.Click += Btn_CorrectTrackerReset_Click;
			break;
		case 148:
			txt_CorrectWidth = (TextBox)target;
			txt_CorrectWidth.PreviewMouseDoubleClick += Txt_LengthMin_PreviewMouseDoubleClick;
			break;
		case 149:
			txt_CorrectHeight = (TextBox)target;
			txt_CorrectHeight.PreviewMouseDoubleClick += Txt_LengthMin_PreviewMouseDoubleClick;
			break;
		case 150:
			lab_unit1 = (Label)target;
			break;
		case 151:
			lab_unit2 = (Label)target;
			break;
		case 152:
			btn_CorrectSet = (Button)target;
			btn_CorrectSet.Click += Btn_CorrectSet_Click;
			break;
		case 153:
			txt_PixelperUmW = (TextBox)target;
			txt_PixelperUmW.PreviewMouseDoubleClick += Txt_LengthMin_PreviewMouseDoubleClick;
			break;
		case 154:
			lab_unit3 = (Label)target;
			break;
		case 155:
			txt_PixelperUmH = (TextBox)target;
			txt_PixelperUmH.PreviewMouseDoubleClick += Txt_LengthMin_PreviewMouseDoubleClick;
			break;
		case 156:
			lab_unit4 = (Label)target;
			break;
		case 157:
			rt_CorrectResult = (Rectangle)target;
			break;
		case 158:
			rad_um = (RadioButton)target;
			rad_um.Checked += Rad_um_Checked;
			break;
		case 159:
			rad_mm = (RadioButton)target;
			rad_mm.Checked += Rad_mm_Checked;
			break;
		case 160:
			btn_CorrectReSet = (Button)target;
			btn_CorrectReSet.Click += Btn_CorrectReSet_Click;
			break;
		case 161:
			chk_UseFloat = (CheckBox)target;
			break;
		case 162:
			tab_A = (TabItem)target;
			break;
		case 163:
			cb_TrackerSelect_A = (ComboBox)target;
			cb_TrackerSelect_A.SelectionChanged += Cb_TrackerSelect_A_SelectionChanged;
			break;
		case 164:
			txt_TrackerName_A = (TextBox)target;
			break;
		case 165:
			scr_TrackerSwich_A = (ScrollBar)target;
			scr_TrackerSwich_A.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 166:
			ts_TrackerON_A = (ToggleSwitch)target;
			ts_TrackerON_A.Click += Ts_TrackerON_Click;
			break;
		case 167:
			grd_ParamA = (Grid)target;
			break;
		case 168:
			txt_AdvInspRule3Min = (TextBox)target;
			break;
		case 169:
			txt_AdvInspRule3Max = (TextBox)target;
			break;
		case 170:
			cb_AdvErrorBin = (ComboBox)target;
			break;
		case 171:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 172:
			txt_AdvInspRule3Result = (TextBox)target;
			break;
		case 173:
			txt_AdvInspRule1Min = (TextBox)target;
			break;
		case 174:
			txt_AdvInspRule1Max = (TextBox)target;
			break;
		case 175:
			txt_AdvInspRule2Min = (TextBox)target;
			break;
		case 176:
			txt_AdvInspRule2Max = (TextBox)target;
			break;
		case 177:
			txt_AdvInspRule1Result = (TextBox)target;
			break;
		case 178:
			txt_AdvInspRule2Result = (TextBox)target;
			break;
		case 179:
			cb_AdvInspRule1 = (ComboBox)target;
			break;
		case 180:
			cb_AdvInspRule2 = (ComboBox)target;
			break;
		case 181:
			cb_AdvInspRule3 = (ComboBox)target;
			break;
		case 182:
			cb_AdvSelect1W = (ComboBox)target;
			break;
		case 183:
			cb_AdvSelect1H = (ComboBox)target;
			break;
		case 184:
			rad_Select1UseWidth = (RadioButton)target;
			break;
		case 185:
			rad_Select1UseHeight = (RadioButton)target;
			break;
		case 186:
			cb_Select1ValueIndex = (ComboBox)target;
			break;
		case 187:
			cb_AdvSelect2W = (ComboBox)target;
			break;
		case 188:
			cb_AdvSelect2H = (ComboBox)target;
			break;
		case 189:
			rad_Select2UseWidth = (RadioButton)target;
			break;
		case 190:
			rad_Select2UseHeight = (RadioButton)target;
			break;
		case 191:
			cb_Select2ValueIndex = (ComboBox)target;
			break;
		case 192:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 193:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 194:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 195:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 196:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 197:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 198:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 199:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 200:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 201:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 202:
			txt_Tracker1Pos = (Label)target;
			break;
		case 203:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 204:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 205:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 206:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 207:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 208:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 209:
			lab_RValue = (Label)target;
			break;
		case 210:
			lab_GValue = (Label)target;
			break;
		case 211:
			lab_BValue = (Label)target;
			break;
		case 212:
			lab_HValue = (Label)target;
			break;
		case 213:
			lab_SValue = (Label)target;
			break;
		case 214:
			lab_VValue = (Label)target;
			break;
		case 215:
			lab_GrayValue = (Label)target;
			break;
		case 216:
			lab_XValue = (Label)target;
			break;
		case 217:
			lab_YValue = (Label)target;
			break;
		case 218:
			txt_Tracker2Pos = (Label)target;
			break;
		case 219:
			lab_C1Value = (Label)target;
			break;
		case 220:
			lab_C2Value = (Label)target;
			break;
		case 221:
			lab_C3Value = (Label)target;
			break;
		case 222:
			lab_C4Value = (Label)target;
			break;
		case 223:
			lab_C5Value = (Label)target;
			break;
		case 224:
			lab_C6Value = (Label)target;
			break;
		case 225:
			txt_AllResult = (TextBox)target;
			break;
		case 226:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 227:
			grd_ParamWH = (Grid)target;
			break;
		case 228:
			grd_SIMP_Setup = (Grid)target;
			break;
		case 229:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 230:
			btn_Test1 = (Button)target;
			btn_Test1.Click += Btn_Test_Click;
			break;
		case 231:
			cb_Scan1Rule = (ComboBox)target;
			break;
		case 232:
			txt_ContError = (TextBox)target;
			txt_ContError.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 233:
			txt_MaxError = (TextBox)target;
			txt_MaxError.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 234:
			txt_ContErrorResult = (TextBox)target;
			break;
		case 235:
			txt_MaxErrorResult = (TextBox)target;
			break;
		case 236:
			btn_Tracker1Reset = (Button)target;
			btn_Tracker1Reset.Click += Btn_TrackerReset_Click;
			break;
		case 237:
			btn_Tracker2Reset = (Button)target;
			btn_Tracker2Reset.Click += Btn_TrackerReset_Click;
			break;
		case 238:
			rad_UseAllPt = (RadioButton)target;
			break;
		case 239:
			rad_UseSinglePt = (RadioButton)target;
			break;
		case 240:
			txt_LengthMin = (TextBox)target;
			txt_LengthMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 241:
			txt_LengthMax = (TextBox)target;
			txt_LengthMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 242:
			chk_ShowScanEdge = (CheckBox)target;
			break;
		case 243:
			chk_ShowScanOK = (CheckBox)target;
			break;
		case 244:
			chk_ShowScanNG = (CheckBox)target;
			break;
		case 245:
			cb_Scan2Rule = (ComboBox)target;
			break;
		case 246:
			txt_SinglePtLengthMin = (TextBox)target;
			txt_SinglePtLengthMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 247:
			txt_SinglePtLengthMax = (TextBox)target;
			txt_SinglePtLengthMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 248:
			txt_SinglePtLengthResult = (TextBox)target;
			break;
		case 249:
			grd_SelectFace = (Grid)target;
			break;
		case 250:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 251:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 252:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 253:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 254:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 255:
			chk_CheckEveryOne = (CheckBox)target;
			chk_CheckEveryOne.Click += Chk_SelectFace_Click;
			break;
		case 256:
			btn_ChartM = (Button)target;
			btn_ChartM.Click += Btn_ChartM_Click;
			break;
		case 257:
			btn_ChartM_Copy = (Button)target;
			btn_ChartM_Copy.Click += Btn_ChartM_Click;
			break;
		case 258:
			txt_LengthDiffMax = (TextBox)target;
			txt_LengthDiffMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 259:
			txt_LengthMaxDiffResult = (TextBox)target;
			break;
		case 260:
			cb_MaskSelect = (ComboBox)target;
			cb_MaskSelect.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 261:
			cb_Mask_DEStart = (ComboBox)target;
			cb_Mask_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 262:
			cb_Mask_DEStartCount = (ComboBox)target;
			cb_Mask_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 263:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 264:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 265:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 266:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 267:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 268:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 269:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 270:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 271:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 272:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 273:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 274:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 275:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 276:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 277:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 278:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 279:
			grd_TrackerAdj = (Grid)target;
			break;
		case 280:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 281:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 282:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 283:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 284:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 285:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 286:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 287:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 288:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 289:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 290:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 291:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 292:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 293:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 294:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 295:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 296:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 297:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 298:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 299:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 300:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 301:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 302:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 303:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 304:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 305:
			Label_Index = (Label)target;
			break;
		case 306:
			lab_LockMode = (TextBox)target;
			break;
		case 307:
			rt_Mask1 = (Rectangle)target;
			break;
		case 308:
			rt_Mask2 = (Rectangle)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
