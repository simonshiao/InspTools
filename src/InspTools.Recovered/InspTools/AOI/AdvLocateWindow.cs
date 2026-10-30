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

public class AdvLocateWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private const int MaxThdTool = 3;

	private const int MaxTools = 20;

	private const int MaxMeasureTools = 40;

	private int MeasureFunction;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private bool ShowMessage = true;

	private byte[] CurrentColor = new byte[13];

	private OpenCvSharp.Rect DefaultSearchRect = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private OpenCvSharp.Rect BlobRect;

	private int LanguageType = 1;

	private int[] TrackIndex = new int[4];

	private int[] SelectFace = new int[4];

	private int FaceIndex;

	private int CurrentFunction;

	private double CopyMinW;

	private double CopyMaxW;

	private double CopyMinH;

	private double CopyMaxH;

	private INSP_SCANLINETOOL_PARAM CopyParamV;

	private INSP_SCANLINETOOL_PARAM CopyParamH;

	private INSP_ADVLOCATETOOL_PARAM CopyParamA;

	private INSP_ADVMEASURETOOL_PARAM CopyParamM;

	private INSP_SCANLINETOOL_PARAM BackupParamV;

	private INSP_SCANLINETOOL_PARAM BackupParamH;

	private INSP_ADVLOCATETOOL_PARAM BackupParamA;

	private INSP_ADVMEASURETOOL_PARAM BackupParamM;

	private INSP_COLORREFERENCE_PARAM CopyColorParam;

	private bool CopyParamVSeted;

	private bool CopyParamHSeted;

	private bool CopyParamASeted;

	private bool CopyParamMSeted;

	private bool TrackerAdjShow;

	private Stopwatch sw = new Stopwatch();

	private int[] ThdLow = new int[3];

	private int[] ThdHi = new int[3];

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat BinaryBlob = new Mat();

	private Mat ScanBinary = new Mat();

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private AdvLocateTool advLocateTool;

	private ToggleSwitch[] TrackON;

	private ComboBox[] TrackSelect;

	private TextBox[] TrackName;

	private RadioButton[] ScanDirectionBtn;

	private RadioButton[] ScanDirection2Btn;

	private ComboBox[] FindEdgeMethod;

	private ComboBox[] ErrorBin;

	private Grid[] ScanBlock;

	private Button[] ToolsButton;

	private RangeSlider[] THD_Sliders;

	private NumericUpDown[] THD_TextLow;

	private NumericUpDown[] THD_TextUp;

	private ComboBox[] THD_Channel;

	private ComboBox[] THD_DEStart;

	private ComboBox[] THD_DEEnd;

	private ComboBox[] THD_DEStartCount;

	private ComboBox[] THD_DEEndCount;

	private Expander[] THD_Expender;

	private Rectangle[] rtResult;

	private bool EnableModifyLog;

	private string ModifyLog = "";

	private string RecipeName = "";

	private string UserID = "123456";

	private bool PreviewMode;

	private User_Permissions User = User_Permissions.USER_Administrator;

	private bool OTSU_White = true;

	internal AdvLocateWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Line line_1;

	internal Line line_2;

	internal Label Label_Result;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Rectangle bor_Rect1;

	internal Rectangle bor_Rect2;

	internal Rectangle bor_PtRect1;

	internal Rectangle bor_PtRect2;

	internal TextBox Label_ResultError;

	internal TextBox txt_PreviewMode;

	internal TabControl tab_AdvLocateFun;

	internal TabItem tab_V;

	internal ComboBox cb_TrackerSelect_V;

	internal TextBox txt_TrackerName_V;

	internal ScrollBar scr_TrackerSwich_V;

	internal ToggleSwitch ts_TrackerON_V;

	internal Button btn_ReverseV;

	internal Button btn_RecipeCopyV;

	internal Button btn_RecipePasteV;

	internal Grid grd_ScanV_Block;

	internal ComboBox cb_ErrorBin_V;

	internal Button btn_Test_V;

	internal RadioButton rad_Direction_V;

	internal RadioButton rad_Direction_V2;

	internal Button btn_ScanL2R;

	internal Button btn_ScanR2L;

	internal RadioButton rad_L2R1;

	internal RadioButton rad_R2L1;

	internal ComboBox cb_FindEdgeMethod_V;

	internal Button btn_TrackerReset_Copy;

	internal Rectangle rt_ResultV;

	internal CheckBox chk_UseStdFilterV;

	internal Grid grd_VSelectFace;

	internal Button btn_VFaceA;

	internal Button btn_VFaceB;

	internal Button btn_VFaceC;

	internal Button btn_VFaceD;

	internal CheckBox chk_VSelectFace;

	internal CheckBox chk_UseColorInverseV;

	internal Button btn_PrevTracker;

	internal Button btn_NextTracker;

	internal RadioButton rad_FollowV;

	internal RadioButton rad_AutoSizeV;

	internal ComboBox cb_FollowVMode;

	internal TabItem tab_H;

	internal Grid grd_ScanH_Block;

	internal ComboBox cb_ErrorBin_H;

	internal Button btn_Test_H;

	internal RadioButton rad_Direction_H;

	internal RadioButton rad_Direction_H2;

	internal Button btn_ScanL2R1;

	internal Button btn_ScanR2L1;

	internal RadioButton rad_Direction_H3;

	internal RadioButton rad_Direction_H4;

	internal RadioButton rad_L2R3;

	internal RadioButton rad_R2L3;

	internal ComboBox cb_FindEdgeMethod_H;

	internal Button btn_TrackerReset;

	internal Rectangle rt_ResultH;

	internal CheckBox chk_UseStdFilterH;

	internal Grid grd_HSelectFace;

	internal Button btn_HFaceA;

	internal Button btn_HFaceB;

	internal Button btn_HFaceC;

	internal Button btn_HFaceD;

	internal CheckBox chk_HSelectFace;

	internal CheckBox chk_UseColorInverseH;

	internal ComboBox cb_TrackerSelect_H;

	internal TextBox txt_TrackerName_H;

	internal ScrollBar scr_TrackerSwich_H;

	internal ToggleSwitch ts_TrackerON_H;

	internal Button btn_ReverseH;

	internal Button btn_RecipeCopyH;

	internal Button btn_RecipePasteH;

	internal Button btn_PrevTrackerH;

	internal Button btn_NextTrackerH;

	internal RadioButton rad_FollowH;

	internal RadioButton rad_AutoSizeH;

	internal TabItem tab_A;

	internal Grid grd_ScanA_Block;

	internal Rectangle rt_UseBinary;

	internal Rectangle rt_UseAdvLocate;

	internal ComboBox cb_RectIndexT;

	internal ComboBox cb_RectIndexB;

	internal ComboBox cb_RectIndexL;

	internal ComboBox cb_RectIndexR;

	internal TextBox txt_WidthMin;

	internal TextBox txt_WidthMax;

	internal TextBox txt_WidthValue;

	internal TextBox txt_HeightMin;

	internal TextBox txt_HeightMax;

	internal TextBox txt_HeightValue;

	internal Button btn_Test1__A;

	internal ComboBox cb_ErrorBin_A;

	internal Rectangle rt_ResultA;

	internal Grid grd_ASelectFace;

	internal Button btn_AFaceA;

	internal Button btn_AFaceB;

	internal Button btn_AFaceC;

	internal Button btn_AFaceD;

	internal Button btn_ValueCopy;

	internal Button btn_ValuePaste;

	internal CheckBox chk_ASelectFace;

	internal Button btn_ChartW;

	internal Button btn_ChartH;

	internal CheckBox chk_EnableWidthRatio;

	internal TextBox txt_WidthRatio;

	internal CheckBox chk_EnableHeightRatio;

	internal TextBox txt_HeightRatio;

	internal Rectangle rt_EnabledFake;

	internal RadioButton rad_UseAdvLocate;

	internal RadioButton rad_UseBinary;

	internal RadioButton rad_FollowA;

	internal RadioButton rad_AutoSizeA;

	internal ComboBox cb_FollowA;

	internal TextBox txt_RectShiftT;

	internal TextBox txt_RectShiftB;

	internal TextBox txt_RectShiftL;

	internal TextBox txt_RectShiftR;

	internal ComboBox cb_TrackerSelect_A;

	internal TextBox txt_TrackerName_A;

	internal ScrollBar scr_TrackerSwich_A;

	internal ToggleSwitch ts_TrackerON_A;

	internal Button btn_ReverseA;

	internal Button btn_RecipeCopyA;

	internal Button btn_RecipePasteA;

	internal Label lab_Time3;

	internal Button btn_PrevTrackerA;

	internal Button btn_NextTrackerA;

	internal TabItem tab_M;

	internal ComboBox cb_TrackerSelect_M;

	internal TextBox txt_TrackerName_M;

	internal ScrollBar scr_TrackerSwich_A1;

	internal ToggleSwitch ts_TrackerON_M;

	internal Button btn_ReverseM;

	internal Button btn_RecipeCopyM;

	internal Button btn_RecipePasteM;

	internal Label lab_Time4;

	internal Grid grd_ScanM_Block;

	internal Button btn_Test_V1;

	internal RadioButton rad_MeasureW;

	internal RadioButton rad_MeasureH;

	internal RadioButton rad_MeasureD;

	internal RadioButton rad_MeasureK;

	internal RadioButton rad_MeasureS;

	internal RadioButton rad_L2R2;

	internal RadioButton rad_R2L2;

	internal Rectangle rt_ResultM;

	internal Grid grd_MSelectFace;

	internal Button btn_MFaceA;

	internal Button btn_MFaceB;

	internal Button btn_MFaceC;

	internal Button btn_MFaceD;

	internal RadioButton rad_MeasreStartWH;

	internal RadioButton rad_MeasreStartA;

	internal ComboBox cb_MeasureStartWHIndex;

	internal ComboBox cb_MeasureStartAIndex;

	internal ComboBox cb_MeasureStartAPt;

	internal Button btn_MStartEdit;

	internal RadioButton rad_MeasreEndWH;

	internal ComboBox cb_MeasureEndWHIndex;

	internal RadioButton rad_MeasreEndA;

	internal ComboBox cb_MeasureEndAIndex;

	internal ComboBox cb_MeasureEndAPt;

	internal Button btn_MEndEdit;

	internal Grid grd_PtPreview;

	internal TextBox txt_ValueMin;

	internal TextBox txt_ValueMax;

	internal TextBox txt_Distance;

	internal ComboBox cb_ErrorBin_M;

	internal Button btn_ChartM;

	internal CheckBox chk_MeasureAllX;

	internal CheckBox chk_MeasureAllY;

	internal Grid grd_CustomRatio;

	internal Grid grd_KValueSetup;

	internal TextBox txt_ScanExtend;

	internal TextBox txt_ScanShrink;

	internal CheckBox chk_MSelectFace;

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

	internal Label lab_C1Value;

	internal Label lab_C2Value;

	internal Label lab_C3Value;

	internal Label lab_C4Value;

	internal Label lab_C5Value;

	internal Label lab_C6Value;

	internal Button btn_ColorDropper;

	internal TextBox txt_AllResult;

	internal Button btn_ReDraw;

	internal Grid grd_BinaryBlock;

	internal Expander exp_THD1;

	internal Grid grd_SIMP_THD_1;

	internal RangeSlider rs_THD1;

	internal Grid grd_SIMP_DilateErode1;

	internal ComboBox cb_THD1_DEStart;

	internal ComboBox cb_THD1_DEEnd;

	internal ComboBox cb_THD1_DEEndCount;

	internal ComboBox cb_THD1_DEStartCount;

	internal ComboBox cb_Channel_THD1;

	internal NumericUpDown nu_Thd1LimitMin;

	internal NumericUpDown nu_Thd1LimitMax;

	internal Button btn_THD1Undo;

	internal Button btn_OTSU;

	internal Expander exp_THD2;

	internal Grid grd_SIMP_THD_1_Copy;

	internal RangeSlider rs_THD2;

	internal Grid grd_SIMP_DilateErode2;

	internal ComboBox cb_THD2_DEStart;

	internal ComboBox cb_THD2_DEEnd;

	internal ComboBox cb_THD2_DEEndCount;

	internal ComboBox cb_THD2_DEStartCount;

	internal ComboBox cb_Channel_THD2;

	internal NumericUpDown nu_Thd2LimitMin;

	internal NumericUpDown nu_Thd2LimitMax;

	internal Button btn_THD2Undo;

	internal Expander exp_THD3;

	internal Grid grd_SIMP_THD_1_Copy1;

	internal RangeSlider rs_THD3;

	internal Grid grd_SIMP_DilateErode3;

	internal ComboBox cb_THD3_DEStart;

	internal ComboBox cb_THD3_DEEnd;

	internal ComboBox cb_THD3_DEEndCount;

	internal ComboBox cb_THD3_DEStartCount;

	internal ComboBox cb_Channel_THD3;

	internal NumericUpDown nu_Thd3LimitMin;

	internal NumericUpDown nu_Thd3LimitMax;

	internal Button btn_THD3Undo;

	internal Button btn_ColorCopy;

	internal Button btn_ColorPaste;

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

	internal Label lab_FaceIndex;

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

	internal Button btn_Tools31;

	internal Button btn_Tools32;

	internal Button btn_Tools33;

	internal Button btn_Tools34;

	internal Button btn_Tools35;

	internal Button btn_Tools36;

	internal Button btn_Tools37;

	internal Button btn_Tools38;

	internal Button btn_Tools39;

	internal Button btn_Tools40;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal Label Label_Index;

	internal TextBox lab_LockMode;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public AdvLocateWindow(User_Permissions permissions)
	{
		inited = false;
		User = permissions;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		TrackIndex[0] = 2;
		TrackIndex[1] = 2;
		TrackIndex[2] = 1;
		TrackIndex[3] = 0;
		TrackSelect = new ComboBox[4] { cb_TrackerSelect_V, cb_TrackerSelect_H, cb_TrackerSelect_A, cb_TrackerSelect_M };
		TrackON = new ToggleSwitch[4] { ts_TrackerON_V, ts_TrackerON_H, ts_TrackerON_A, ts_TrackerON_M };
		TrackName = new TextBox[4] { txt_TrackerName_V, txt_TrackerName_H, txt_TrackerName_A, txt_TrackerName_M };
		ScanDirectionBtn = new RadioButton[2] { rad_Direction_V, rad_Direction_H };
		ScanDirection2Btn = new RadioButton[2] { rad_Direction_V2, rad_Direction_H2 };
		FindEdgeMethod = new ComboBox[2] { cb_FindEdgeMethod_V, cb_FindEdgeMethod_H };
		ErrorBin = new ComboBox[4] { cb_ErrorBin_V, cb_ErrorBin_H, cb_ErrorBin_A, cb_ErrorBin_M };
		ScanBlock = new Grid[4] { grd_ScanV_Block, grd_ScanH_Block, grd_ScanA_Block, grd_ScanM_Block };
		THD_Sliders = new RangeSlider[3] { rs_THD1, rs_THD2, rs_THD3 };
		THD_TextLow = new NumericUpDown[3] { nu_Thd1LimitMin, nu_Thd2LimitMin, nu_Thd3LimitMin };
		THD_TextUp = new NumericUpDown[3] { nu_Thd1LimitMax, nu_Thd2LimitMax, nu_Thd3LimitMax };
		THD_Channel = new ComboBox[3] { cb_Channel_THD1, cb_Channel_THD2, cb_Channel_THD3 };
		THD_DEStart = new ComboBox[3] { cb_THD1_DEStart, cb_THD2_DEStart, cb_THD3_DEStart };
		THD_DEEnd = new ComboBox[3] { cb_THD1_DEEnd, cb_THD2_DEEnd, cb_THD3_DEEnd };
		THD_DEStartCount = new ComboBox[3] { cb_THD1_DEStartCount, cb_THD2_DEStartCount, cb_THD3_DEStartCount };
		THD_DEEndCount = new ComboBox[3] { cb_THD1_DEEndCount, cb_THD2_DEEndCount, cb_THD3_DEEndCount };
		THD_Expender = new Expander[3] { exp_THD1, exp_THD2, exp_THD3 };
		rtResult = new Rectangle[4] { rt_ResultV, rt_ResultH, rt_ResultA, rt_ResultM };
		Label_Result.Visibility = Visibility.Hidden;
		ToolsButton = new Button[40]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20,
			btn_Tools21, btn_Tools22, btn_Tools23, btn_Tools24, btn_Tools25, btn_Tools26, btn_Tools27, btn_Tools28, btn_Tools29, btn_Tools30,
			btn_Tools31, btn_Tools32, btn_Tools33, btn_Tools34, btn_Tools35, btn_Tools36, btn_Tools37, btn_Tools38, btn_Tools39, btn_Tools40
		};
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		line_1.Visibility = (line_2.Visibility = Visibility.Hidden);
		bor_Rect1.Visibility = (bor_Rect2.Visibility = Visibility.Hidden);
		bor_PtRect1.Visibility = (bor_PtRect2.Visibility = Visibility.Hidden);
		bor_SearchRect1.Visibility = Visibility.Hidden;
		chk_MeasureAllX.Visibility = Visibility.Hidden;
		chk_MeasureAllY.Visibility = Visibility.Hidden;
		txt_PreviewMode.Visibility = Visibility.Hidden;
		grd_CustomRatio.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(int functionindex, int index, CVInspection station, int faceIndex = 0)
	{
		inspection = station;
		advLocateTool = new AdvLocateTool(inspection.paramFile);
		FaceIndex = faceIndex;
		SetFaceIndex(faceIndex);
		SetSelectFace(faceIndex, functionindex);
		UpdateUI = false;
		cb_TrackerSelect_V.Items.Clear();
		cb_TrackerSelect_H.Items.Clear();
		cb_TrackerSelect_A.Items.Clear();
		cb_RectIndexT.Items.Clear();
		cb_RectIndexB.Items.Clear();
		cb_RectIndexL.Items.Clear();
		cb_RectIndexR.Items.Clear();
		cb_TrackerSelect_M.Items.Clear();
		inspection.paramFile.VSLocateParam[0].TrackerName = LanguageManager.Instance["LocateLeft"];
		inspection.paramFile.VSLocateParam[0].Value = inspection.buffers.LocatedRect.Left;
		inspection.paramFile.VSLocateParam[0].TrackerON = true;
		inspection.paramFile.VSLocateParam[1].TrackerName = LanguageManager.Instance["LocateRight"];
		inspection.paramFile.VSLocateParam[1].Value = inspection.buffers.LocatedRect.Left + inspection.buffers.LocatedRect.Width;
		inspection.paramFile.VSLocateParam[1].TrackerON = true;
		inspection.paramFile.HSLocateParam[0].TrackerName = LanguageManager.Instance["LocateTop"];
		inspection.paramFile.HSLocateParam[0].Value = inspection.buffers.LocatedRect.Top;
		inspection.paramFile.HSLocateParam[0].TrackerON = true;
		inspection.paramFile.HSLocateParam[1].TrackerName = LanguageManager.Instance["LocateBottom"];
		inspection.paramFile.HSLocateParam[1].Value = inspection.buffers.LocatedRect.Top + inspection.buffers.LocatedRect.Height;
		inspection.paramFile.HSLocateParam[1].TrackerON = true;
		inspection.paramFile.AdvLocateParam[0].TrackerON = true;
		inspection.paramFile.AdvLocateParam[0].TrackerName = LanguageManager.Instance["LocateRect"];
		for (int i = 0; i < 20; i++)
		{
			if (i < 2)
			{
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + (i + 1) + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
				comboBoxItem.IsEnabled = false;
				ComboBoxItem newItem = comboBoxItem;
				cb_TrackerSelect_V.Items.Add(newItem);
				comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + (i + 1) + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
				comboBoxItem.IsEnabled = false;
				ComboBoxItem newItem2 = comboBoxItem;
				cb_TrackerSelect_H.Items.Add(newItem2);
			}
			else
			{
				cb_TrackerSelect_V.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
				cb_TrackerSelect_H.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
			}
		}
		for (int j = 0; j < 40; j++)
		{
			cb_TrackerSelect_M.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateMParam[j].TrackerName);
		}
		for (int k = 0; k < 20; k++)
		{
			if (k < 1)
			{
				ComboBoxItem newItem3 = new ComboBoxItem
				{
					Content = k + 1 + " : " + inspection.paramFile.AdvLocateParam[k].TrackerName,
					IsEnabled = false
				};
				cb_TrackerSelect_A.Items.Add(newItem3);
			}
			else
			{
				cb_TrackerSelect_A.Items.Add(k + 1 + " : " + inspection.paramFile.AdvLocateParam[k].TrackerName);
			}
			if (inspection.paramFile.VSLocateParam[k].TrackerON)
			{
				cb_RectIndexL.Items.Add(k + 1 + " : " + inspection.paramFile.VSLocateParam[k].TrackerName);
				cb_RectIndexR.Items.Add(k + 1 + " : " + inspection.paramFile.VSLocateParam[k].TrackerName);
			}
			else
			{
				ComboBoxItem newItem4 = new ComboBoxItem
				{
					Content = k + 1 + " : " + inspection.paramFile.VSLocateParam[k].TrackerName,
					IsEnabled = false
				};
				cb_RectIndexL.Items.Add(newItem4);
				ComboBoxItem newItem5 = new ComboBoxItem
				{
					Content = k + 1 + " : " + inspection.paramFile.VSLocateParam[k].TrackerName,
					IsEnabled = false
				};
				cb_RectIndexR.Items.Add(newItem5);
			}
			if (inspection.paramFile.HSLocateParam[k].TrackerON)
			{
				cb_RectIndexT.Items.Add(k + 1 + " : " + inspection.paramFile.HSLocateParam[k].TrackerName);
				cb_RectIndexB.Items.Add(k + 1 + " : " + inspection.paramFile.HSLocateParam[k].TrackerName);
				continue;
			}
			ComboBoxItem newItem6 = new ComboBoxItem
			{
				Content = k + 1 + " : " + inspection.paramFile.HSLocateParam[k].TrackerName,
				IsEnabled = false
			};
			cb_RectIndexT.Items.Add(newItem6);
			ComboBoxItem newItem7 = new ComboBoxItem
			{
				Content = k + 1 + " : " + inspection.paramFile.HSLocateParam[k].TrackerName,
				IsEnabled = false
			};
			cb_RectIndexB.Items.Add(newItem7);
		}
		TrackSelect[0].SelectedIndex = 2;
		TrackSelect[1].SelectedIndex = 2;
		TrackSelect[2].SelectedIndex = 1;
		tab_AdvLocateFun.SelectedIndex = functionindex;
		TrackIndex[functionindex] = index;
		TrackSelect[functionindex].SelectedIndex = index;
		CurrentFunction = functionindex;
		UpdateToolsState(functionindex);
		UpdateUI = true;
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

	private void SetSelectFace(int index, int findex)
	{
		switch (findex)
		{
		case 0:
			SelectFace[0] = index;
			if (index > 0)
			{
				chk_VSelectFace.IsChecked = true;
				grd_VSelectFace.Visibility = Visibility.Visible;
			}
			btn_VFaceA.Background = Brushes.White;
			btn_VFaceB.Background = Brushes.White;
			btn_VFaceC.Background = Brushes.White;
			btn_VFaceD.Background = Brushes.White;
			switch (index)
			{
			case 0:
				btn_VFaceA.Background = Brushes.White;
				btn_VFaceB.Background = Brushes.White;
				btn_VFaceC.Background = Brushes.White;
				btn_VFaceD.Background = Brushes.White;
				break;
			case 1:
				btn_VFaceA.Background = Brushes.Yellow;
				break;
			case 2:
				btn_VFaceB.Background = Brushes.Yellow;
				break;
			case 3:
				btn_VFaceC.Background = Brushes.Yellow;
				break;
			case 4:
				btn_VFaceD.Background = Brushes.Yellow;
				break;
			default:
				btn_VFaceA.Background = Brushes.White;
				btn_VFaceB.Background = Brushes.White;
				btn_VFaceC.Background = Brushes.White;
				btn_VFaceD.Background = Brushes.White;
				break;
			}
			break;
		case 1:
			SelectFace[1] = index;
			if (index > 0)
			{
				chk_HSelectFace.IsChecked = true;
				grd_HSelectFace.Visibility = Visibility.Visible;
			}
			btn_HFaceA.Background = Brushes.White;
			btn_HFaceB.Background = Brushes.White;
			btn_HFaceC.Background = Brushes.White;
			btn_HFaceD.Background = Brushes.White;
			switch (index)
			{
			case 0:
				btn_HFaceA.Background = Brushes.White;
				btn_HFaceB.Background = Brushes.White;
				btn_HFaceC.Background = Brushes.White;
				btn_HFaceD.Background = Brushes.White;
				break;
			case 1:
				btn_HFaceA.Background = Brushes.Yellow;
				break;
			case 2:
				btn_HFaceB.Background = Brushes.Yellow;
				break;
			case 3:
				btn_HFaceC.Background = Brushes.Yellow;
				break;
			case 4:
				btn_HFaceD.Background = Brushes.Yellow;
				break;
			default:
				btn_HFaceA.Background = Brushes.White;
				btn_HFaceB.Background = Brushes.White;
				btn_HFaceC.Background = Brushes.White;
				btn_HFaceD.Background = Brushes.White;
				break;
			}
			break;
		case 2:
			SelectFace[2] = index;
			if (index > 0)
			{
				chk_ASelectFace.IsChecked = true;
				grd_ASelectFace.Visibility = Visibility.Visible;
			}
			btn_AFaceA.Background = Brushes.White;
			btn_AFaceB.Background = Brushes.White;
			btn_AFaceC.Background = Brushes.White;
			btn_AFaceD.Background = Brushes.White;
			switch (index)
			{
			case 0:
				btn_AFaceA.Background = Brushes.White;
				btn_AFaceB.Background = Brushes.White;
				btn_AFaceC.Background = Brushes.White;
				btn_AFaceD.Background = Brushes.White;
				break;
			case 1:
				btn_AFaceA.Background = Brushes.Yellow;
				break;
			case 2:
				btn_AFaceB.Background = Brushes.Yellow;
				break;
			case 3:
				btn_AFaceC.Background = Brushes.Yellow;
				break;
			case 4:
				btn_AFaceD.Background = Brushes.Yellow;
				break;
			default:
				btn_AFaceA.Background = Brushes.White;
				btn_AFaceB.Background = Brushes.White;
				btn_AFaceC.Background = Brushes.White;
				btn_AFaceD.Background = Brushes.White;
				break;
			}
			break;
		default:
			SelectFace[3] = index;
			if (index > 0)
			{
				chk_MSelectFace.IsChecked = true;
				grd_MSelectFace.Visibility = Visibility.Visible;
			}
			btn_MFaceA.Background = Brushes.White;
			btn_MFaceB.Background = Brushes.White;
			btn_MFaceC.Background = Brushes.White;
			btn_MFaceD.Background = Brushes.White;
			switch (index)
			{
			case 0:
				btn_MFaceA.Background = Brushes.White;
				btn_MFaceB.Background = Brushes.White;
				btn_MFaceC.Background = Brushes.White;
				btn_MFaceD.Background = Brushes.White;
				break;
			case 1:
				btn_MFaceA.Background = Brushes.Yellow;
				break;
			case 2:
				btn_MFaceB.Background = Brushes.Yellow;
				break;
			case 3:
				btn_MFaceC.Background = Brushes.Yellow;
				break;
			case 4:
				btn_MFaceD.Background = Brushes.Yellow;
				break;
			default:
				btn_MFaceA.Background = Brushes.White;
				btn_MFaceB.Background = Brushes.White;
				btn_MFaceC.Background = Brushes.White;
				btn_MFaceD.Background = Brushes.White;
				break;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateToolsState(int functionindex)
	{
		btn_Tools1.IsEnabled = false;
		btn_Tools2.IsEnabled = false;
		btn_Tools2.Background = Brushes.White;
		tab_V.Background = Brushes.White;
		tab_H.Background = Brushes.White;
		tab_A.Background = Brushes.White;
		tab_M.Background = Brushes.White;
		switch (functionindex)
		{
		case 0:
		{
			tab_V.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA8DA90"));
			ToolsButton[0].Background = Brushes.White;
			ToolsButton[1].Background = Brushes.White;
			for (int m = 2; m < 20; m++)
			{
				if (inspection.paramFile.VSLocateParam[m].TrackerON)
				{
					if (inspection.iResult.VScanResult[m] == TOOL_RESULT.EM_OK)
					{
						if (inspection.paramFile.VSLocateParam[m].EnableSelectFace)
						{
							if (inspection.paramFile.VSLocateParam[m].FaceIndex == FaceIndex)
							{
								ToolsButton[m].Background = Brushes.Lime;
							}
							else
							{
								ToolsButton[m].Background = Brushes.DarkGray;
							}
						}
						else
						{
							ToolsButton[m].Background = Brushes.Lime;
						}
					}
					else if (inspection.iResult.VScanResult[m] == TOOL_RESULT.EM_NG)
					{
						ToolsButton[m].Background = Brushes.Red;
					}
					else
					{
						ToolsButton[m].Background = Brushes.Cyan;
					}
				}
				else
				{
					ToolsButton[m].Background = Brushes.White;
				}
				ToolsButton[m].ToolTip = inspection.paramFile.VSLocateParam[m].TrackerName;
			}
			for (int n = 20; n < 40; n++)
			{
				ToolsButton[n].Visibility = Visibility.Hidden;
			}
			return;
		}
		case 1:
		{
			tab_H.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEEA4B7"));
			ToolsButton[0].Background = Brushes.White;
			ToolsButton[1].Background = Brushes.White;
			for (int i = 2; i < 20; i++)
			{
				if (inspection.paramFile.HSLocateParam[i].TrackerON)
				{
					if (inspection.iResult.HScanResult[i] == TOOL_RESULT.EM_OK)
					{
						if (inspection.paramFile.HSLocateParam[i].EnableSelectFace)
						{
							if (inspection.paramFile.HSLocateParam[i].FaceIndex == FaceIndex)
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
					else if (inspection.iResult.HScanResult[i] == TOOL_RESULT.EM_NG)
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
				ToolsButton[i].ToolTip = inspection.paramFile.HSLocateParam[i].TrackerName;
			}
			for (int j = 20; j < 40; j++)
			{
				ToolsButton[j].Visibility = Visibility.Hidden;
			}
			return;
		}
		case 2:
		{
			tab_A.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6DDA2"));
			ToolsButton[0].Background = Brushes.White;
			btn_Tools2.IsEnabled = true;
			for (int k = 1; k < 20; k++)
			{
				if (inspection.paramFile.AdvLocateParam[k].TrackerON)
				{
					if (inspection.iResult.AdvLocateResult[k] == TOOL_RESULT.EM_OK)
					{
						if (inspection.paramFile.AdvLocateParam[k].EnableSelectFace)
						{
							if (inspection.paramFile.AdvLocateParam[k].FaceIndex == FaceIndex)
							{
								ToolsButton[k].Background = Brushes.Lime;
							}
							else
							{
								ToolsButton[k].Background = Brushes.DarkGray;
							}
						}
						else
						{
							ToolsButton[k].Background = Brushes.Lime;
						}
					}
					else if (inspection.iResult.AdvLocateResult[k] == TOOL_RESULT.EM_NG)
					{
						ToolsButton[k].Background = Brushes.Red;
					}
					else
					{
						ToolsButton[k].Background = Brushes.Cyan;
					}
				}
				else
				{
					ToolsButton[k].Background = Brushes.White;
				}
				ToolsButton[k].ToolTip = inspection.paramFile.AdvLocateParam[k].TrackerName;
			}
			for (int l = 20; l < 40; l++)
			{
				ToolsButton[l].Visibility = Visibility.Hidden;
			}
			return;
		}
		}
		tab_M.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA2EEF6"));
		btn_Tools1.IsEnabled = true;
		btn_Tools2.IsEnabled = true;
		for (int num = 0; num < 40; num++)
		{
			ToolsButton[num].Visibility = Visibility.Visible;
			if (inspection.paramFile.AdvLocateMParam[num].TrackerON)
			{
				if (inspection.iResult.AdvMeasureResult[num] == TOOL_RESULT.EM_OK)
				{
					if (inspection.paramFile.AdvLocateMParam[num].EnableSelectFace)
					{
						if (inspection.paramFile.AdvLocateMParam[num].FaceIndex == FaceIndex)
						{
							ToolsButton[num].Background = Brushes.Lime;
						}
						else
						{
							ToolsButton[num].Background = Brushes.DarkGray;
						}
					}
					else
					{
						ToolsButton[num].Background = Brushes.Lime;
					}
				}
				else if (inspection.iResult.AdvMeasureResult[num] == TOOL_RESULT.EM_NG)
				{
					ToolsButton[num].Background = Brushes.Red;
				}
				else
				{
					ToolsButton[num].Background = Brushes.Cyan;
				}
			}
			else
			{
				ToolsButton[num].Background = Brushes.White;
			}
			ToolsButton[num].ToolTip = inspection.paramFile.AdvLocateMParam[num].TrackerName;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void UpdateAdvMeasureComobox(int FunctionSelect)
	{
		if (FunctionSelect == 3)
		{
			grd_PtPreview.Visibility = Visibility.Hidden;
			grd_KValueSetup.Visibility = Visibility.Visible;
		}
		else
		{
			grd_PtPreview.Visibility = Visibility.Visible;
			grd_KValueSetup.Visibility = Visibility.Hidden;
		}
		UpdateUI = false;
		if (FunctionSelect == 0 || FunctionSelect == 1)
		{
			MeasureFunction = FunctionSelect;
		}
		else
		{
			MeasureFunction = FunctionSelect - 1;
		}
		cb_MeasureStartWHIndex.Items.Clear();
		cb_MeasureEndWHIndex.Items.Clear();
		cb_MeasureStartAIndex.Items.Clear();
		cb_MeasureEndAIndex.Items.Clear();
		chk_MeasureAllX.Visibility = Visibility.Hidden;
		chk_MeasureAllY.Visibility = Visibility.Hidden;
		for (int i = 0; i < 20; i++)
		{
			switch (FunctionSelect)
			{
			case 0:
				if (!inspection.paramFile.VSLocateParam[i].TrackerON)
				{
					ComboBoxItem comboBoxItem5 = new ComboBoxItem();
					comboBoxItem5.Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
					comboBoxItem5.IsEnabled = false;
					cb_MeasureStartWHIndex.Items.Add(comboBoxItem5);
					ComboBoxItem comboBoxItem6 = new ComboBoxItem();
					comboBoxItem6.Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
					comboBoxItem6.IsEnabled = false;
					cb_MeasureEndWHIndex.Items.Add(comboBoxItem6);
				}
				else
				{
					cb_MeasureStartWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
					cb_MeasureEndWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
				}
				break;
			case 1:
				if (!inspection.paramFile.HSLocateParam[i].TrackerON)
				{
					ComboBoxItem comboBoxItem3 = new ComboBoxItem();
					comboBoxItem3.Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
					comboBoxItem3.IsEnabled = false;
					cb_MeasureStartWHIndex.Items.Add(comboBoxItem3);
					ComboBoxItem comboBoxItem4 = new ComboBoxItem();
					comboBoxItem4.Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
					comboBoxItem4.IsEnabled = false;
					cb_MeasureEndWHIndex.Items.Add(comboBoxItem4);
				}
				else
				{
					cb_MeasureStartWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
					cb_MeasureEndWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
				}
				break;
			case 3:
				chk_MeasureAllX.Visibility = Visibility.Visible;
				chk_MeasureAllY.Visibility = Visibility.Visible;
				if (!inspection.paramFile.AdvLocateParam[i].TrackerON)
				{
					ComboBoxItem comboBoxItem7 = new ComboBoxItem();
					comboBoxItem7.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
					comboBoxItem7.IsEnabled = false;
					cb_MeasureStartWHIndex.Items.Add(comboBoxItem7);
					ComboBoxItem comboBoxItem8 = new ComboBoxItem();
					comboBoxItem8.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
					comboBoxItem8.IsEnabled = false;
					cb_MeasureEndWHIndex.Items.Add(comboBoxItem8);
				}
				else
				{
					cb_MeasureStartWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
					cb_MeasureEndWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
				}
				break;
			case 4:
			{
				int selectedIndex = cb_TrackerSelect_M.SelectedIndex;
				if (inspection.paramFile.AdvLocateMParam[i].TrackerON && i < selectedIndex)
				{
					cb_MeasureStartWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateMParam[i].TrackerName);
					cb_MeasureEndWHIndex.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateMParam[i].TrackerName);
					break;
				}
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.AdvLocateMParam[i].TrackerName;
				comboBoxItem.IsEnabled = false;
				cb_MeasureStartWHIndex.Items.Add(comboBoxItem);
				ComboBoxItem comboBoxItem2 = new ComboBoxItem();
				comboBoxItem2.Content = i + 1 + " : " + inspection.paramFile.AdvLocateMParam[i].TrackerName;
				comboBoxItem2.IsEnabled = false;
				cb_MeasureEndWHIndex.Items.Add(comboBoxItem2);
				break;
			}
			}
			if (!inspection.paramFile.AdvLocateParam[i].TrackerON && FunctionSelect != 3 && FunctionSelect != 4)
			{
				ComboBoxItem comboBoxItem9 = new ComboBoxItem();
				comboBoxItem9.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem9.IsEnabled = false;
				cb_MeasureStartAIndex.Items.Add(comboBoxItem9);
				ComboBoxItem comboBoxItem10 = new ComboBoxItem();
				comboBoxItem10.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem10.IsEnabled = false;
				cb_MeasureEndAIndex.Items.Add(comboBoxItem10);
			}
			else
			{
				cb_MeasureStartAIndex.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
				cb_MeasureEndAIndex.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
			}
		}
		cb_MeasureStartWHIndex.SelectedIndex = -1;
		cb_MeasureEndWHIndex.SelectedIndex = -1;
		cb_MeasureStartAIndex.SelectedIndex = -1;
		cb_MeasureEndAIndex.SelectedIndex = -1;
		if (FunctionSelect < 2)
		{
			cb_MeasureStartWHIndex.IsEnabled = true;
			cb_MeasureEndWHIndex.IsEnabled = true;
			rad_MeasreStartWH.IsEnabled = true;
			rad_MeasreEndWH.IsEnabled = true;
			rad_MeasreStartA.IsEnabled = true;
			rad_MeasreEndA.IsEnabled = true;
			cb_MeasureStartAIndex.IsEnabled = true;
			cb_MeasureEndAIndex.IsEnabled = true;
			cb_MeasureStartAPt.IsEnabled = true;
			cb_MeasureEndAPt.IsEnabled = true;
			return;
		}
		switch (FunctionSelect)
		{
		case 2:
			cb_MeasureStartWHIndex.IsEnabled = false;
			cb_MeasureEndWHIndex.IsEnabled = false;
			rad_MeasreStartWH.IsEnabled = false;
			rad_MeasreEndWH.IsEnabled = false;
			rad_MeasreStartA.IsEnabled = true;
			rad_MeasreEndA.IsEnabled = true;
			cb_MeasureStartAIndex.IsEnabled = true;
			cb_MeasureEndAIndex.IsEnabled = true;
			cb_MeasureStartAPt.IsEnabled = true;
			cb_MeasureEndAPt.IsEnabled = true;
			break;
		case 3:
			cb_MeasureStartWHIndex.IsEnabled = true;
			cb_MeasureEndWHIndex.IsEnabled = true;
			cb_MeasureStartWHIndex.SelectedIndex = -1;
			cb_MeasureEndWHIndex.SelectedIndex = -1;
			rad_MeasreStartWH.IsEnabled = true;
			rad_MeasreEndWH.IsEnabled = true;
			rad_MeasreStartWH.IsChecked = true;
			rad_MeasreEndWH.IsChecked = true;
			rad_MeasreStartA.IsEnabled = false;
			rad_MeasreEndA.IsEnabled = false;
			cb_MeasureStartAIndex.IsEnabled = false;
			cb_MeasureEndAIndex.IsEnabled = false;
			cb_MeasureStartAPt.IsEnabled = false;
			cb_MeasureEndAPt.IsEnabled = false;
			break;
		case 4:
			cb_MeasureStartWHIndex.IsEnabled = true;
			cb_MeasureEndWHIndex.IsEnabled = true;
			cb_MeasureStartWHIndex.SelectedIndex = -1;
			cb_MeasureEndWHIndex.SelectedIndex = -1;
			rad_MeasreStartWH.IsEnabled = true;
			rad_MeasreEndWH.IsEnabled = true;
			rad_MeasreStartWH.IsChecked = true;
			rad_MeasreEndWH.IsChecked = true;
			rad_MeasreStartA.IsEnabled = false;
			rad_MeasreEndA.IsEnabled = false;
			cb_MeasureStartAIndex.IsEnabled = false;
			cb_MeasureEndAIndex.IsEnabled = false;
			cb_MeasureStartAPt.IsEnabled = false;
			cb_MeasureEndAPt.IsEnabled = false;
			break;
		}
	}

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect)
	{
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
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc, ShowBitmap);
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
		CopyValueToUI(CurrentFunction);
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

	public void SetModifyLogEnable(bool IsEnable, string recipename, string userID)
	{
		EnableModifyLog = IsEnable;
		RecipeName = recipename;
		UserID = userID;
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI(0);
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex[CurrentFunction]);
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
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
	private void SetTracker(OpenCvSharp.Rect rect1)
	{
		if (!PreviewMode)
		{
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			rect1 &= rect2;
			if (rect1.Width <= 0 || rect1.Height <= 0)
			{
				rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
			txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTracker(ref OpenCvSharp.Rect rect1)
	{
		if (!PreviewMode && xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
			rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
			rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
			rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
			rect1 &= rect2;
			txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		}
	}

	public void GetInspeRect(int index, ref OpenCvSharp.Rect rect)
	{
		int autoSizeIndex = inspection.paramFile.AdvLocateParam[index].AutoSizeIndex;
		rect.X = LocateRect.X + inspection.paramFile.AdvLocateParam[index].SearchRectL;
		rect.Y = LocateRect.Y + inspection.paramFile.AdvLocateParam[index].SearchRectT;
		if (inspection.paramFile.AdvLocateParam[index].UseAutoSize)
		{
			rect.X = inspection.paramFile.AdvLocateParam[autoSizeIndex].InsideRect.X + inspection.paramFile.AdvLocateParam[index].SearchRectL;
			rect.Y = inspection.paramFile.AdvLocateParam[autoSizeIndex].InsideRect.Y + inspection.paramFile.AdvLocateParam[index].SearchRectT;
			rect.Width = inspection.paramFile.AdvLocateParam[autoSizeIndex].InsideRect.Width;
			rect.Height = inspection.paramFile.AdvLocateParam[autoSizeIndex].InsideRect.Height;
		}
		else
		{
			rect.Width = inspection.paramFile.AdvLocateParam[index].SearchRectWidth;
			rect.Height = inspection.paramFile.AdvLocateParam[index].SearchRectHeight;
		}
	}

	private void Rs_SIMP_THD1_ThumbDragCompleted(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			DoThdImage();
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
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = Brushes.Lime;
			rtResult[CurrentFunction].Fill = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
			rtResult[CurrentFunction].Fill = Brushes.Red;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DoThdImage()
	{
		if (!UpdateUI || PreviewMode)
		{
			return;
		}
		switch (CurrentFunction)
		{
		case 0:
			if (ts_TrackerON_V.IsChecked == false)
			{
				return;
			}
			break;
		case 1:
			if (ts_TrackerON_H.IsChecked == false)
			{
				return;
			}
			break;
		case 2:
			if (ts_TrackerON_A.IsChecked == false)
			{
				return;
			}
			break;
		}
		if (CurrentFunction == 2 && rad_UseAdvLocate.IsChecked == true)
		{
			return;
		}
		try
		{
			Label_Result.Visibility = Visibility.Hidden;
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc, ShowBitmap);
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			Mat[] array = new Mat[3];
			Mat[] array2 = new Mat[3];
			int[] array3 = new int[3];
			int[] array4 = new int[3];
			int[] array5 = new int[3];
			bool[] array6 = new bool[3];
			int[] array7 = new int[3];
			int[] array8 = new int[3];
			int[] array9 = new int[3];
			int[] array10 = new int[3];
			array6[0] = exp_THD1.IsExpanded;
			array6[1] = exp_THD2.IsExpanded;
			array6[2] = exp_THD3.IsExpanded;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			new OpenCvSharp.Rect(0, 0, 100, 100);
			if (CurrentFunction != 2 || rad_UseBinary.IsChecked == true)
			{
				GetTracker(ref rect);
			}
			else
			{
				GetInspeRect(cb_TrackerSelect_A.SelectedIndex, ref rect);
			}
			Mat mat = MM.CloneFrom(inspection.buffers.LocateGray);
			mat.SetTo(255.0);
			rect &= new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
			int num = 0;
			for (int i = 0; i < 3; i++)
			{
				array[i] = new Mat();
				array2[i] = new Mat();
				array3[i] = (int)THD_Sliders[i].LowerValue;
				array4[i] = (int)THD_Sliders[i].UpperValue;
				array5[i] = THD_Channel[i].SelectedIndex;
				array7[i] = THD_DEStart[i].SelectedIndex;
				array8[i] = THD_DEEnd[i].SelectedIndex;
				array9[i] = THD_DEStartCount[i].SelectedIndex;
				array10[i] = THD_DEEndCount[i].SelectedIndex;
				THD_TextLow[i].Value = array3[i];
				THD_TextUp[i].Value = array4[i];
				if (array6[i])
				{
					array[i] = MM.CloneFrom(inspection.buffers.GetChannelMat(array5[i]), array[i]);
					if (array7[i] != 9)
					{
						Cv2.InRange(array[i], array3[i], array4[i], array2[i]);
					}
					else
					{
						array2[i] = MM.CloneFrom(array[i], array2[i]);
					}
					Mat mat2 = new Mat(array2[i], rect);
					if (array7[i] != 0)
					{
						DoMorph(ref mat2, array7[i], array9[i]);
					}
					if (array8[i] != 0)
					{
						DoMorph(ref mat2, array8[i], array10[i]);
					}
					num++;
				}
			}
			if (num == 0)
			{
				return;
			}
			for (int j = 0; j < 3; j++)
			{
				if (array6[j])
				{
					Cv2.BitwiseAnd(mat, array2[j], mat);
				}
			}
			if (CurrentFunction == 0)
			{
				if (chk_UseColorInverseV.IsChecked.Value)
				{
					Cv2.BitwiseNot(mat, mat);
				}
			}
			else if (CurrentFunction == 1 && chk_UseColorInverseH.IsChecked.Value)
			{
				Cv2.BitwiseNot(mat, mat);
			}
			ScanBinary = MM.CloneFrom(mat, ScanBinary);
			BinaryBlob = new Mat(mat, rect).Clone();
			if (tab_AdvLocateFun.SelectedIndex == 2 && rad_UseBinary.IsChecked == true)
			{
				findBlob(BinaryBlob);
			}
			Mat mat3 = MM.CloneFrom(mat);
			mat3.SetTo(0.0);
			new Mat(mat3, rect).SetTo(255.0);
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
			THD_Preview = true;
			MM.GiveBack(mat);
			MM.GiveBack(mat3);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ALW01-AdvLocateWindow-DoThdImage()" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			_ = ShowMessage;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlob(Mat binary)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
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
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
			new OpenCvSharp.Rect(0, 0, 100, 100);
			GetTracker(ref rect2);
			BlobRect.Left = rect.Left + rect2.Left;
			BlobRect.Top = rect.Top + rect2.Top;
			BlobRect.Width = rect.Width;
			BlobRect.Height = rect.Height;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Adv02-AdvLocateTool-findBlob()-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return true;
		}
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
		if (!txt_WidthMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_WidthMin.Text))
			{
				txt_WidthMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_WidthMin.Background = Brushes.White;
			}
		}
		if (!txt_WidthMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_WidthMax.Text))
			{
				txt_WidthMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_WidthMax.Background = Brushes.White;
			}
		}
		if (!txt_WidthRatio.Text.Contains(":") && !IsNumeric(txt_WidthRatio.Text))
		{
			txt_WidthRatio.Text = "1.0";
		}
		if (!txt_HeightRatio.Text.Contains(":") && !IsNumeric(txt_HeightRatio.Text))
		{
			txt_HeightRatio.Text = "1.0";
		}
		if (!txt_HeightMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_HeightMin.Text))
			{
				txt_HeightMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_HeightMin.Background = Brushes.White;
			}
		}
		if (!txt_HeightMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_HeightMax.Text))
			{
				txt_HeightMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_HeightMax.Background = Brushes.White;
			}
		}
		if (!txt_ValueMin.Text.Contains(":"))
		{
			if (!IsNumeric(txt_ValueMin.Text))
			{
				txt_ValueMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_ValueMin.Background = Brushes.White;
			}
		}
		if (!txt_ValueMax.Text.Contains(":"))
		{
			if (!IsNumeric(txt_ValueMax.Text))
			{
				txt_ValueMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_ValueMax.Background = Brushes.White;
			}
		}
		if (!txt_RectShiftT.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RectShiftT.Text))
			{
				txt_RectShiftT.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RectShiftT.Background = Brushes.White;
			}
		}
		if (!txt_RectShiftB.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RectShiftB.Text))
			{
				txt_RectShiftB.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RectShiftB.Background = Brushes.White;
			}
		}
		if (!txt_RectShiftL.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RectShiftL.Text))
			{
				txt_RectShiftL.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RectShiftL.Background = Brushes.White;
			}
		}
		if (!txt_RectShiftR.Text.Contains(":"))
		{
			if (!IsNumeric(txt_RectShiftR.Text))
			{
				txt_RectShiftR.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_RectShiftR.Background = Brushes.White;
			}
		}
		return result;
	}

	private void DoMorph(ref Mat mat, int index, int count)
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
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(LanguageManager.Instance["ResetTrackerMsg"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			_ = (Button)sender;
			bor_SearchRect1.SetValue(Canvas.LeftProperty, 0.0);
			bor_SearchRect1.SetValue(Canvas.TopProperty, 0.0);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, 100.0);
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckVUsed(int index)
	{
		if (LanguageType == 0)
		{
			MessageBoxResult messageBoxResult = MessageBox.Show("Parameters will Revert to Previous save！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
		}
		for (int i = 1; i < 20; i++)
		{
			if (inspection.paramFile.AdvLocateParam[i].TrackerON && (inspection.paramFile.AdvLocateParam[i].RectIndexL == index || inspection.paramFile.AdvLocateParam[i].RectIndexR == index))
			{
				MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show(string.Format("內部定位工具[{0}]使用中，是否關閉?", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show(string.Format("内部定位工具[{0}]使用中，是否关闭?", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show(string.Format("Inside Locate[{0}] has been Used。Will be Close?", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show(string.Format("内部配置[{0}] が使用中です。閉じてよろしいですか？", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
				if (messageBoxResult != MessageBoxResult.Yes)
				{
					return false;
				}
				inspection.paramFile.AdvLocateParam[i].TrackerON = false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckHUsed(int index)
	{
		for (int i = 1; i < 20; i++)
		{
			if (inspection.paramFile.AdvLocateParam[i].TrackerON && (inspection.paramFile.AdvLocateParam[i].RectIndexT == index || inspection.paramFile.AdvLocateParam[i].RectIndexB == index))
			{
				MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show(string.Format("內部定位工具[{0}]使用中，是否關閉?", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show(string.Format("内部定位工具[{0}]使用中，是否关闭?", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show(string.Format("Inside Locate[{0}] has been Used。Will be Close?", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show(string.Format("内部配置[{0}] が使用中です。閉じてよろしいですか？", i + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
				if (messageBoxResult != MessageBoxResult.Yes)
				{
					return false;
				}
				inspection.paramFile.AdvLocateParam[i].TrackerON = false;
			}
		}
		return true;
	}

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 1)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 2)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 3 && User < User_Permissions.USER_Administrator && inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]].Locked)
		{
			return;
		}
		if (PreviewMode)
		{
			return;
		}
		if (TrackON[CurrentFunction].IsChecked.Value)
		{
			ScanBlock[CurrentFunction].Visibility = Visibility.Visible;
			THD_Expender[0].IsExpanded = true;
			if (CurrentFunction < 2)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
				grd_BinaryBlock.Visibility = Visibility.Visible;
			}
			else if (CurrentFunction == 2)
			{
				if (rad_UseBinary.IsChecked == true)
				{
					bor_SearchRect1.Visibility = Visibility.Visible;
					grd_BinaryBlock.Visibility = Visibility.Visible;
				}
			}
			else
			{
				bor_SearchRect1.Visibility = Visibility.Hidden;
				grd_BinaryBlock.Visibility = Visibility.Hidden;
			}
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
			return;
		}
		if (CurrentFunction == 0)
		{
			if (!CheckVUsed(TrackIndex[CurrentFunction]))
			{
				TrackON[CurrentFunction].IsChecked = true;
				return;
			}
		}
		else if (CurrentFunction == 1 && !CheckHUsed(TrackIndex[CurrentFunction]))
		{
			TrackON[CurrentFunction].IsChecked = true;
			return;
		}
		ScanBlock[CurrentFunction].Visibility = Visibility.Hidden;
		bor_SearchRect1.Visibility = Visibility.Hidden;
		grd_BinaryBlock.Visibility = Visibility.Hidden;
		if (CurrentFunction == 0)
		{
			inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].TrackerON = false;
			inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].ValueChangeFlag = true;
			inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
			inspection.iResult.VScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NOT_USE;
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
		}
		else if (CurrentFunction == 1)
		{
			inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].TrackerON = false;
			inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].ValueChangeFlag = true;
			inspection.iResult.HScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NOT_USE;
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
		}
		else if (CurrentFunction == 2)
		{
			inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].TrackerON = false;
			inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].ValueChangeFlag = true;
			inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NOT_USE;
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
		}
		else if (CurrentFunction == 3)
		{
			inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]].TrackerON = false;
			inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]].ValueChangeFlag = true;
			ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
		}
		UpdateToolsState(CurrentFunction);
		ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
	}

	private void Btn_ColorDropper_Click(object sender, RoutedEventArgs e)
	{
		cnv_Image.Cursor = Cursors.Cross;
		ColorDropper = true;
	}

	private void Cnv_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!ColorDropper)
		{
			return;
		}
		cnv_Image.Cursor = Cursors.Arrow;
		for (int i = 0; i < 3; i++)
		{
			if (THD_Expender[i].IsExpanded)
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
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect);
	}

	private void Btn_ScanL2R_Click(object sender, RoutedEventArgs e)
	{
		rad_Direction_V.IsChecked = true;
	}

	private void Btn_ScanR2L_Click(object sender, RoutedEventArgs e)
	{
		rad_Direction_V2.IsChecked = true;
	}

	private void Cb_TrackerSelect_V_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			TrackIndex[CurrentFunction] = cb_TrackerSelect_V.SelectedIndex;
			Label_Index.Content = (cb_TrackerSelect_V.SelectedIndex + 1).ToString();
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			UpdateToolsState(CurrentFunction);
		}
	}

	private void Cb_TrackerSelect_H_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			TrackIndex[CurrentFunction] = cb_TrackerSelect_H.SelectedIndex;
			Label_Index.Content = (cb_TrackerSelect_H.SelectedIndex + 1).ToString();
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			UpdateToolsState(CurrentFunction);
		}
	}

	private void Cb_TrackerSelect_A_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			TrackIndex[CurrentFunction] = cb_TrackerSelect_A.SelectedIndex;
			Label_Index.Content = (cb_TrackerSelect_A.SelectedIndex + 1).ToString();
			CopyValueToUI(CurrentFunction);
			if (ts_TrackerON_A.IsChecked == true)
			{
				Btn_Test_Click(sender, e);
			}
			UpdateToolsState(CurrentFunction);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_AdvLocateFun_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !(e.Source is TabControl) || !(e.AddedItems[0] is TabItem tabItem))
		{
			return;
		}
		if (tabItem.Name == "tab_V")
		{
			Line line = line_1;
			Visibility visibility = (line_2.Visibility = Visibility.Hidden);
			line.Visibility = visibility;
			Rectangle rectangle = bor_Rect1;
			visibility = (bor_Rect2.Visibility = Visibility.Hidden);
			rectangle.Visibility = visibility;
			Rectangle rectangle2 = bor_PtRect1;
			visibility = (bor_PtRect2.Visibility = Visibility.Hidden);
			rectangle2.Visibility = visibility;
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			CurrentFunction = 0;
			if (TrackIndex[CurrentFunction] < 2)
			{
				TrackIndex[CurrentFunction] = 2;
			}
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			bor_LocateRect.Visibility = Visibility.Visible;
			UpdateToolsState(0);
		}
		else if (tabItem.Name == "tab_H")
		{
			Line line2 = line_1;
			Visibility visibility = (line_2.Visibility = Visibility.Hidden);
			line2.Visibility = visibility;
			Rectangle rectangle3 = bor_Rect1;
			visibility = (bor_Rect2.Visibility = Visibility.Hidden);
			rectangle3.Visibility = visibility;
			Rectangle rectangle4 = bor_PtRect1;
			visibility = (bor_PtRect2.Visibility = Visibility.Hidden);
			rectangle4.Visibility = visibility;
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			CurrentFunction = 1;
			if (TrackIndex[CurrentFunction] < 2)
			{
				TrackIndex[CurrentFunction] = 2;
			}
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			bor_LocateRect.Visibility = Visibility.Visible;
			UpdateToolsState(1);
		}
		else if (tabItem.Name == "tab_A")
		{
			Line line3 = line_1;
			Visibility visibility = (line_2.Visibility = Visibility.Hidden);
			line3.Visibility = visibility;
			Rectangle rectangle5 = bor_Rect1;
			visibility = (bor_Rect2.Visibility = Visibility.Hidden);
			rectangle5.Visibility = visibility;
			Rectangle rectangle6 = bor_PtRect1;
			visibility = (bor_PtRect2.Visibility = Visibility.Hidden);
			rectangle6.Visibility = visibility;
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			CurrentFunction = 2;
			if (TrackIndex[CurrentFunction] < 1)
			{
				TrackIndex[CurrentFunction] = 1;
			}
			if (rad_UseBinary.IsChecked == false)
			{
				bor_SearchRect1.Visibility = Visibility.Hidden;
				grd_BinaryBlock.Visibility = Visibility.Hidden;
			}
			else
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
				grd_BinaryBlock.Visibility = Visibility.Visible;
			}
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			bor_LocateRect.Visibility = Visibility.Visible;
			UpdateToolsState(2);
		}
		else if (tabItem.Name == "tab_M")
		{
			Mat mat = new Mat();
			inspection.SetBuffers(inspection.buffers.Src);
			inspection.Inspect(ref mat);
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			CurrentFunction = 3;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			grd_BinaryBlock.Visibility = Visibility.Hidden;
			bor_LocateRect.Visibility = Visibility.Hidden;
			UpdateToolsState(3);
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
		}
	}

	private void Cb_RectIndexT_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc, ShowBitmap);
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			int num = (int)ConvertComDigits(txt_RectShiftT, ForSave: false);
			int num2 = (int)ConvertComDigits(txt_RectShiftB, ForSave: false);
			int num3 = (int)ConvertComDigits(txt_RectShiftL, ForSave: false);
			int num4 = (int)ConvertComDigits(txt_RectShiftR, ForSave: false);
			int num5 = inspection.paramFile.VSLocateParam[cb_RectIndexL.SelectedIndex].Value + num3;
			int num6 = inspection.paramFile.VSLocateParam[cb_RectIndexR.SelectedIndex].Value - num4;
			int num7 = inspection.paramFile.HSLocateParam[cb_RectIndexT.SelectedIndex].Value + num;
			int num8 = inspection.paramFile.HSLocateParam[cb_RectIndexB.SelectedIndex].Value - num2;
			if (num7 != -1)
			{
				ShowBitmap.Line(new OpenCvSharp.Point(0, num7), new OpenCvSharp.Point(ShowBitmap.Cols, num7), Scalar.Yellow);
			}
			if (num8 != -1)
			{
				ShowBitmap.Line(new OpenCvSharp.Point(0, num8), new OpenCvSharp.Point(ShowBitmap.Cols, num8), Scalar.Yellow);
			}
			if (num5 != -1)
			{
				ShowBitmap.Line(new OpenCvSharp.Point(num5, 0), new OpenCvSharp.Point(num5, ShowBitmap.Rows), Scalar.Yellow);
			}
			if (num6 != -1)
			{
				ShowBitmap.Line(new OpenCvSharp.Point(num6, 0), new OpenCvSharp.Point(num6, ShowBitmap.Rows), Scalar.Yellow);
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Scr_TrackerSwich_H_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	private void Scr_TrackerSwich_A_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
	}

	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		switch (CurrentFunction)
		{
		case 0:
			btn_RecipePasteV.IsEnabled = true;
			CopyParamVSeted = true;
			CopyParamV = inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]];
			btn_RecipeCopyV.Background = Brushes.PaleGreen;
			break;
		case 1:
			btn_RecipePasteH.IsEnabled = true;
			CopyParamHSeted = true;
			CopyParamH = inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]];
			btn_RecipeCopyH.Background = Brushes.PaleGreen;
			break;
		case 2:
			btn_RecipePasteA.IsEnabled = true;
			CopyParamASeted = true;
			CopyParamA = inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]];
			btn_RecipeCopyA.Background = Brushes.PaleGreen;
			break;
		case 3:
			btn_RecipePasteM.IsEnabled = true;
			CopyParamMSeted = true;
			CopyParamM = inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]];
			btn_RecipeCopyM.Background = Brushes.PaleGreen;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentFunction == 0)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 1)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 2)
		{
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].Locked)
			{
				return;
			}
		}
		else if (CurrentFunction == 3 && User < User_Permissions.USER_Administrator && inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]].Locked)
		{
			return;
		}
		switch (CurrentFunction)
		{
		case 0:
			if (CopyParamVSeted && MessageBox.Show(LanguageManager.Instance["ValueOverWrite"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				string trackerName3 = inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].TrackerName;
				inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]] = CopyParamV;
				inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].TrackerName = trackerName3;
				CopyValueToUI(CurrentFunction);
			}
			break;
		case 1:
			if (CopyParamHSeted && MessageBox.Show(LanguageManager.Instance["ValueOverWrite"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				string trackerName2 = inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].TrackerName;
				inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]] = CopyParamH;
				inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].TrackerName = trackerName2;
				CopyValueToUI(CurrentFunction);
			}
			break;
		case 2:
			if (CopyParamASeted && MessageBox.Show(LanguageManager.Instance["ValueOverWrite"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				string trackerName4 = inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].TrackerName;
				inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]] = CopyParamA;
				inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].TrackerName = trackerName4;
				CopyValueToUI(CurrentFunction);
			}
			break;
		case 3:
			if (CopyParamMSeted && MessageBox.Show(LanguageManager.Instance["ValueOverWrite"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				string trackerName = inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]].TrackerName;
				inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]] = CopyParamM;
				inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]].TrackerName = trackerName;
				CopyValueToUI(CurrentFunction);
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Reverse_Click(object sender, RoutedEventArgs e)
	{
		switch (CurrentFunction)
		{
		case 0:
			if (MessageBox.Show(LanguageManager.Instance["ValueRevert"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]] = inspection.paramFile_Backup.VSLocateParam[TrackIndex[CurrentFunction]];
				CopyValueToUI(CurrentFunction);
			}
			break;
		case 1:
			if (MessageBox.Show(LanguageManager.Instance["ValueRevert"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]] = inspection.paramFile_Backup.HSLocateParam[TrackIndex[CurrentFunction]];
				CopyValueToUI(CurrentFunction);
			}
			break;
		case 2:
			if (MessageBox.Show(LanguageManager.Instance["ValueRevert"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]] = inspection.paramFile_Backup.AdvLocateParam[TrackIndex[CurrentFunction]];
				CopyValueToUI(CurrentFunction);
			}
			break;
		case 3:
			if (MessageBox.Show(LanguageManager.Instance["ValueRevert"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				inspection.paramFile.AdvLocateMParam[TrackIndex[CurrentFunction]] = inspection.paramFile_Backup.AdvLocateMParam[TrackIndex[CurrentFunction]];
				CopyValueToUI(CurrentFunction);
			}
			break;
		}
	}

	private void Ts_TrackerON_V_Unchecked(object sender, RoutedEventArgs e)
	{
	}

	private void DrawBackGround()
	{
		if (!PreviewMode)
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc, ShowBitmap);
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
	private void CopyValueToUI(int findex)
	{
		try
		{
			UpdateUI = false;
			Label_ResultError.Visibility = Visibility.Hidden;
			chk_EnableWidthRatio.Visibility = Visibility.Hidden;
			chk_EnableHeightRatio.Visibility = Visibility.Hidden;
			txt_HeightRatio.Visibility = Visibility.Hidden;
			txt_WidthRatio.Visibility = Visibility.Hidden;
			if (PreviewMode)
			{
				btn_Test_V.Visibility = Visibility.Hidden;
				btn_Test_H.Visibility = Visibility.Hidden;
				btn_Test_V1.Visibility = Visibility.Hidden;
				btn_Test1__A.Visibility = Visibility.Hidden;
			}
			if (findex < 2)
			{
				if (TrackIndex[findex] < 2)
				{
					TrackIndex[findex] = 2;
				}
				INSP_SCANLINETOOL_PARAM iNSP_SCANLINETOOL_PARAM;
				if (findex == 0)
				{
					iNSP_SCANLINETOOL_PARAM = (BackupParamV = inspection.paramFile.VSLocateParam[TrackIndex[findex]]);
					if (iNSP_SCANLINETOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
					{
						lab_LockMode.Visibility = Visibility.Visible;
					}
					else
					{
						lab_LockMode.Visibility = Visibility.Hidden;
					}
				}
				else
				{
					iNSP_SCANLINETOOL_PARAM = (BackupParamH = inspection.paramFile.HSLocateParam[TrackIndex[findex]]);
					if (iNSP_SCANLINETOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
					{
						lab_LockMode.Visibility = Visibility.Visible;
					}
					else
					{
						lab_LockMode.Visibility = Visibility.Hidden;
					}
				}
				Label_Index.Content = (TrackIndex[findex] + 1).ToString();
				TrackON[findex].IsChecked = iNSP_SCANLINETOOL_PARAM.TrackerON;
				if (iNSP_SCANLINETOOL_PARAM.TrackerON)
				{
					ScanBlock[findex].Visibility = Visibility.Visible;
					bor_SearchRect1.Visibility = Visibility.Visible;
					grd_BinaryBlock.Visibility = Visibility.Visible;
				}
				else
				{
					ScanBlock[findex].Visibility = Visibility.Hidden;
					bor_SearchRect1.Visibility = Visibility.Hidden;
					grd_BinaryBlock.Visibility = Visibility.Hidden;
				}
				TrackSelect[findex].SelectedIndex = TrackIndex[findex];
				TrackName[findex].Text = iNSP_SCANLINETOOL_PARAM.TrackerName;
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
				cb_FollowVMode.SelectedIndex = iNSP_SCANLINETOOL_PARAM.FollowMode;
				if (iNSP_SCANLINETOOL_PARAM.UseAutoSize)
				{
					rect.X = LocateRect.X + iNSP_SCANLINETOOL_PARAM.SearchRectL;
					rect.Y = LocateRect.Y + iNSP_SCANLINETOOL_PARAM.SearchRectT;
					rect.Width = LocateRect.Width - (iNSP_SCANLINETOOL_PARAM.SearchRectL + iNSP_SCANLINETOOL_PARAM.SearchRectR);
					rect.Height = LocateRect.Height - (iNSP_SCANLINETOOL_PARAM.SearchRectT + iNSP_SCANLINETOOL_PARAM.SearchRectB);
					if (findex == 0)
					{
						rad_AutoSizeV.IsChecked = true;
					}
					else
					{
						rad_AutoSizeH.IsChecked = true;
					}
				}
				else
				{
					if (findex == 0)
					{
						if (iNSP_SCANLINETOOL_PARAM.FollowMode == 0)
						{
							rect.X = LocateRect.X + iNSP_SCANLINETOOL_PARAM.SearchRectL;
							rect.Y = LocateRect.Y + iNSP_SCANLINETOOL_PARAM.SearchRectT;
						}
						else if (iNSP_SCANLINETOOL_PARAM.FollowMode == 1)
						{
							rect.X = LocateRect.X + iNSP_SCANLINETOOL_PARAM.SearchRectL;
							rect.Y = LocateRect.Y + LocateRect.Height + iNSP_SCANLINETOOL_PARAM.SearchRectT;
						}
						else if (iNSP_SCANLINETOOL_PARAM.FollowMode == 2)
						{
							rect.X = LocateRect.X + LocateRect.Width + iNSP_SCANLINETOOL_PARAM.SearchRectL;
							rect.Y = LocateRect.Y + iNSP_SCANLINETOOL_PARAM.SearchRectT;
						}
						else
						{
							rect.X = LocateRect.X + LocateRect.Width + iNSP_SCANLINETOOL_PARAM.SearchRectL;
							rect.Y = LocateRect.Y + LocateRect.Height + iNSP_SCANLINETOOL_PARAM.SearchRectT;
						}
						rad_FollowV.IsChecked = true;
					}
					else
					{
						rect.X = LocateRect.X + iNSP_SCANLINETOOL_PARAM.SearchRectL;
						rect.Y = LocateRect.Y + iNSP_SCANLINETOOL_PARAM.SearchRectT;
						rad_FollowH.IsChecked = true;
					}
					rect.Width = iNSP_SCANLINETOOL_PARAM.SearchRectWidth;
					rect.Height = iNSP_SCANLINETOOL_PARAM.SearchRectHeight;
				}
				SetTracker(rect);
				GetTracker(ref DefaultSearchRect);
				LastSearchRect1 = rect;
				cb_Channel_THD1.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.THD1_ChannelIndex;
				cb_Channel_THD2.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.THD2_ChannelIndex;
				cb_Channel_THD3.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.THD3_ChannelIndex;
				exp_THD1.IsExpanded = iNSP_SCANLINETOOL_PARAM.THD1_ChannelON;
				exp_THD2.IsExpanded = iNSP_SCANLINETOOL_PARAM.THD2_ChannelON;
				exp_THD3.IsExpanded = iNSP_SCANLINETOOL_PARAM.THD3_ChannelON;
				rs_THD1.LowerValue = iNSP_SCANLINETOOL_PARAM.THD1_Min;
				rs_THD1.UpperValue = iNSP_SCANLINETOOL_PARAM.THD1_Max;
				nu_Thd1LimitMin.Value = iNSP_SCANLINETOOL_PARAM.THD1_Min;
				nu_Thd1LimitMax.Value = iNSP_SCANLINETOOL_PARAM.THD1_Max;
				ThdLow[0] = iNSP_SCANLINETOOL_PARAM.THD1_Min;
				ThdHi[0] = iNSP_SCANLINETOOL_PARAM.THD1_Max;
				rs_THD2.LowerValue = iNSP_SCANLINETOOL_PARAM.THD2_Min;
				rs_THD2.UpperValue = iNSP_SCANLINETOOL_PARAM.THD2_Max;
				nu_Thd2LimitMin.Value = iNSP_SCANLINETOOL_PARAM.THD2_Min;
				nu_Thd2LimitMax.Value = iNSP_SCANLINETOOL_PARAM.THD2_Max;
				ThdLow[1] = iNSP_SCANLINETOOL_PARAM.THD2_Min;
				ThdHi[1] = iNSP_SCANLINETOOL_PARAM.THD2_Max;
				rs_THD3.LowerValue = iNSP_SCANLINETOOL_PARAM.THD3_Min;
				rs_THD3.UpperValue = iNSP_SCANLINETOOL_PARAM.THD3_Max;
				nu_Thd3LimitMin.Value = iNSP_SCANLINETOOL_PARAM.THD3_Min;
				nu_Thd3LimitMax.Value = iNSP_SCANLINETOOL_PARAM.THD3_Max;
				ThdLow[2] = iNSP_SCANLINETOOL_PARAM.THD3_Min;
				ThdHi[2] = iNSP_SCANLINETOOL_PARAM.THD3_Max;
				cb_THD1_DEStart.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.DE_Start1_Index;
				cb_THD1_DEEnd.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.DE_End1_Index;
				cb_THD2_DEStart.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.DE_Start2_Index;
				cb_THD2_DEEnd.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.DE_End2_Index;
				cb_THD3_DEStart.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.DE_Start3_Index;
				cb_THD3_DEEnd.SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.DE_End3_Index;
				cb_THD1_DEStartCount.SelectedIndex = iNSP_SCANLINETOOL_PARAM.DE_Start1_Count;
				cb_THD1_DEEndCount.SelectedIndex = iNSP_SCANLINETOOL_PARAM.DE_End1_Count;
				cb_THD2_DEStartCount.SelectedIndex = iNSP_SCANLINETOOL_PARAM.DE_Start2_Count;
				cb_THD2_DEEndCount.SelectedIndex = iNSP_SCANLINETOOL_PARAM.DE_End2_Count;
				cb_THD3_DEStartCount.SelectedIndex = iNSP_SCANLINETOOL_PARAM.DE_Start3_Count;
				cb_THD3_DEEndCount.SelectedIndex = iNSP_SCANLINETOOL_PARAM.DE_End3_Count;
				if (findex == 1)
				{
					if (iNSP_SCANLINETOOL_PARAM.Direction < ScanDirection.EM_LEFT_RIGHT_HOLE)
					{
						if ((int)iNSP_SCANLINETOOL_PARAM.Direction % 2 == 0)
						{
							ScanDirectionBtn[findex].IsChecked = true;
						}
						else
						{
							ScanDirection2Btn[findex].IsChecked = true;
						}
					}
					else if (iNSP_SCANLINETOOL_PARAM.Direction == ScanDirection.EM_LEFT_RIGHT_HOLE)
					{
						rad_Direction_H3.IsChecked = true;
					}
					else if (iNSP_SCANLINETOOL_PARAM.Direction == ScanDirection.EM_RIGHT_LEFT_HOLE)
					{
						rad_Direction_H4.IsChecked = true;
					}
				}
				else if ((int)iNSP_SCANLINETOOL_PARAM.Direction % 2 == 0)
				{
					ScanDirectionBtn[findex].IsChecked = true;
				}
				else
				{
					ScanDirection2Btn[findex].IsChecked = true;
				}
				FindEdgeMethod[findex].SelectedIndex = iNSP_SCANLINETOOL_PARAM.FindEdgeMethod;
				if (findex == 0)
				{
					chk_UseStdFilterV.IsChecked = iNSP_SCANLINETOOL_PARAM.UseStdFilter;
					chk_VSelectFace.IsChecked = iNSP_SCANLINETOOL_PARAM.EnableSelectFace;
					chk_UseColorInverseV.IsChecked = iNSP_SCANLINETOOL_PARAM.UseColorInverse;
				}
				else
				{
					chk_UseStdFilterH.IsChecked = iNSP_SCANLINETOOL_PARAM.UseStdFilter;
					chk_HSelectFace.IsChecked = iNSP_SCANLINETOOL_PARAM.EnableSelectFace;
					chk_UseColorInverseH.IsChecked = iNSP_SCANLINETOOL_PARAM.UseColorInverse;
				}
				SetSelectFace(iNSP_SCANLINETOOL_PARAM.FaceIndex, findex);
				ErrorBin[findex].SelectedIndex = (int)iNSP_SCANLINETOOL_PARAM.ErrorBin;
				cb_ParetoChart.SelectedIndex = iNSP_SCANLINETOOL_PARAM.ParetoIndex;
			}
			else if (findex == 2)
			{
				if (TrackIndex[findex] < 1)
				{
					TrackIndex[findex] = 1;
				}
				INSP_ADVLOCATETOOL_PARAM iNSP_ADVLOCATETOOL_PARAM = (BackupParamA = inspection.paramFile.AdvLocateParam[TrackIndex[findex]]);
				if (iNSP_ADVLOCATETOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
				{
					lab_LockMode.Visibility = Visibility.Visible;
				}
				else
				{
					lab_LockMode.Visibility = Visibility.Hidden;
				}
				Label_Index.Content = (TrackIndex[findex] + 1).ToString();
				TrackON[findex].IsChecked = iNSP_ADVLOCATETOOL_PARAM.TrackerON;
				if (iNSP_ADVLOCATETOOL_PARAM.TrackerON)
				{
					ScanBlock[findex].Visibility = Visibility.Visible;
					grd_BinaryBlock.Visibility = Visibility.Visible;
				}
				else
				{
					ScanBlock[findex].Visibility = Visibility.Hidden;
					grd_BinaryBlock.Visibility = Visibility.Hidden;
				}
				TrackSelect[findex].SelectedIndex = TrackIndex[findex];
				TrackName[findex].Text = iNSP_ADVLOCATETOOL_PARAM.TrackerName;
				int num = TrackIndex[findex];
				cb_FollowA.Items.Clear();
				cb_FollowA.Items.Add("定位");
				for (int i = 1; i < num; i++)
				{
					bool trackerON = inspection.paramFile.AdvLocateParam[i].TrackerON;
					ComboBoxItem newItem = new ComboBoxItem
					{
						Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName,
						IsEnabled = trackerON
					};
					cb_FollowA.Items.Add(newItem);
				}
				if (inspection.paramFile.AdvLocateParam[num].TrackerON)
				{
					cb_FollowA.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex;
				}
				else
				{
					cb_FollowA.SelectedIndex = 0;
				}
				if (iNSP_ADVLOCATETOOL_PARAM.RectIndexL > 0 && iNSP_ADVLOCATETOOL_PARAM.RectIndexL < 20)
				{
					if (inspection.paramFile.VSLocateParam[iNSP_ADVLOCATETOOL_PARAM.RectIndexL].TrackerON)
					{
						cb_RectIndexL.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.RectIndexL;
					}
					else
					{
						cb_RectIndexL.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexL = 0);
					}
				}
				else
				{
					cb_RectIndexL.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexL = 0);
				}
				if (iNSP_ADVLOCATETOOL_PARAM.RectIndexR > 0 && iNSP_ADVLOCATETOOL_PARAM.RectIndexR < 20)
				{
					if (inspection.paramFile.VSLocateParam[iNSP_ADVLOCATETOOL_PARAM.RectIndexR].TrackerON)
					{
						cb_RectIndexR.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.RectIndexR;
					}
					else
					{
						cb_RectIndexR.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexR = 0);
					}
				}
				else
				{
					cb_RectIndexR.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexR = 0);
				}
				if (iNSP_ADVLOCATETOOL_PARAM.RectIndexT > 0 && iNSP_ADVLOCATETOOL_PARAM.RectIndexT < 20)
				{
					if (inspection.paramFile.HSLocateParam[iNSP_ADVLOCATETOOL_PARAM.RectIndexT].TrackerON)
					{
						cb_RectIndexT.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.RectIndexT;
					}
					else
					{
						cb_RectIndexT.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexT = 0);
					}
				}
				else
				{
					cb_RectIndexT.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexT = 0);
				}
				if (iNSP_ADVLOCATETOOL_PARAM.RectIndexB > 0 && iNSP_ADVLOCATETOOL_PARAM.RectIndexB < 20)
				{
					if (inspection.paramFile.HSLocateParam[iNSP_ADVLOCATETOOL_PARAM.RectIndexB].TrackerON)
					{
						cb_RectIndexB.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.RectIndexB;
					}
					else
					{
						cb_RectIndexB.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexB = 0);
					}
				}
				else
				{
					cb_RectIndexB.SelectedIndex = (iNSP_ADVLOCATETOOL_PARAM.RectIndexB = 0);
				}
				txt_RectShiftT.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftT, txt_RectShiftT);
				txt_RectShiftB.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftB, txt_RectShiftB);
				txt_RectShiftL.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftL, txt_RectShiftL);
				txt_RectShiftR.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftR, txt_RectShiftR);
				OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
				rect2.X = LocateRect.X + iNSP_ADVLOCATETOOL_PARAM.SearchRectL;
				rect2.Y = LocateRect.Y + iNSP_ADVLOCATETOOL_PARAM.SearchRectT;
				if (iNSP_ADVLOCATETOOL_PARAM.UseAutoSize)
				{
					if (iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex == 0)
					{
						rect2.Width = LocateRect.Width - (iNSP_ADVLOCATETOOL_PARAM.SearchRectL + iNSP_ADVLOCATETOOL_PARAM.SearchRectR);
						rect2.Height = LocateRect.Height - (iNSP_ADVLOCATETOOL_PARAM.SearchRectT + iNSP_ADVLOCATETOOL_PARAM.SearchRectB);
					}
					else
					{
						rect2.X = inspection.paramFile.AdvLocateParam[iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex].InsideRect.X + iNSP_ADVLOCATETOOL_PARAM.SearchRectL;
						rect2.Y = inspection.paramFile.AdvLocateParam[iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex].InsideRect.Y + iNSP_ADVLOCATETOOL_PARAM.SearchRectT;
						rect2.Width = inspection.paramFile.AdvLocateParam[iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex].InsideRect.Width - (iNSP_ADVLOCATETOOL_PARAM.SearchRectL + iNSP_ADVLOCATETOOL_PARAM.SearchRectR);
						rect2.Height = inspection.paramFile.AdvLocateParam[iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex].InsideRect.Height - (iNSP_ADVLOCATETOOL_PARAM.SearchRectT + iNSP_ADVLOCATETOOL_PARAM.SearchRectB);
					}
					rad_AutoSizeA.IsChecked = true;
				}
				else
				{
					rect2.Width = iNSP_ADVLOCATETOOL_PARAM.SearchRectWidth;
					rect2.Height = iNSP_ADVLOCATETOOL_PARAM.SearchRectHeight;
					rad_FollowA.IsChecked = true;
				}
				SetTracker(rect2);
				GetTracker(ref DefaultSearchRect);
				LastSearchRect1 = rect2;
				cb_Channel_THD1.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelIndex;
				cb_Channel_THD2.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelIndex;
				cb_Channel_THD3.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelIndex;
				exp_THD1.IsExpanded = iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelON;
				exp_THD2.IsExpanded = iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelON;
				exp_THD3.IsExpanded = iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelON;
				rs_THD1.LowerValue = iNSP_ADVLOCATETOOL_PARAM.THD1_Min;
				rs_THD1.UpperValue = iNSP_ADVLOCATETOOL_PARAM.THD1_Max;
				nu_Thd1LimitMin.Value = iNSP_ADVLOCATETOOL_PARAM.THD1_Min;
				nu_Thd1LimitMax.Value = iNSP_ADVLOCATETOOL_PARAM.THD1_Max;
				ThdLow[0] = iNSP_ADVLOCATETOOL_PARAM.THD1_Min;
				ThdHi[0] = iNSP_ADVLOCATETOOL_PARAM.THD1_Max;
				rs_THD2.LowerValue = iNSP_ADVLOCATETOOL_PARAM.THD2_Min;
				rs_THD2.UpperValue = iNSP_ADVLOCATETOOL_PARAM.THD2_Max;
				nu_Thd2LimitMin.Value = iNSP_ADVLOCATETOOL_PARAM.THD2_Min;
				nu_Thd2LimitMax.Value = iNSP_ADVLOCATETOOL_PARAM.THD2_Max;
				ThdLow[1] = iNSP_ADVLOCATETOOL_PARAM.THD2_Min;
				ThdHi[1] = iNSP_ADVLOCATETOOL_PARAM.THD2_Max;
				rs_THD3.LowerValue = iNSP_ADVLOCATETOOL_PARAM.THD3_Min;
				rs_THD3.UpperValue = iNSP_ADVLOCATETOOL_PARAM.THD3_Max;
				nu_Thd3LimitMin.Value = iNSP_ADVLOCATETOOL_PARAM.THD3_Min;
				nu_Thd3LimitMax.Value = iNSP_ADVLOCATETOOL_PARAM.THD3_Max;
				ThdLow[2] = iNSP_ADVLOCATETOOL_PARAM.THD3_Min;
				ThdHi[2] = iNSP_ADVLOCATETOOL_PARAM.THD3_Max;
				cb_THD1_DEStart.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Index;
				cb_THD1_DEEnd.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.DE_End1_Index;
				cb_THD2_DEStart.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Index;
				cb_THD2_DEEnd.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.DE_End2_Index;
				cb_THD3_DEStart.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Index;
				cb_THD3_DEEnd.SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.DE_End3_Index;
				cb_THD1_DEStartCount.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Count;
				cb_THD1_DEEndCount.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.DE_End1_Count;
				cb_THD2_DEStartCount.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Count;
				cb_THD2_DEEndCount.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.DE_End2_Count;
				cb_THD3_DEStartCount.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Count;
				cb_THD3_DEEndCount.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.DE_End3_Count;
				if (!iNSP_ADVLOCATETOOL_PARAM.UseBinary)
				{
					rad_UseAdvLocate.IsChecked = true;
					bor_SearchRect1.Visibility = Visibility.Hidden;
					grd_BinaryBlock.Visibility = Visibility.Hidden;
					rt_UseAdvLocate.Fill = Brushes.LightGreen;
					rt_UseBinary.Fill = Brushes.White;
				}
				else
				{
					rad_UseBinary.IsChecked = true;
					bor_SearchRect1.Visibility = Visibility.Visible;
					grd_BinaryBlock.Visibility = Visibility.Visible;
					rt_UseAdvLocate.Fill = Brushes.White;
					rt_UseBinary.Fill = Brushes.LightGreen;
				}
				txt_WidthMin.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.WidthMin, txt_WidthMin);
				txt_WidthMax.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.WidthMax, txt_WidthMax);
				txt_HeightMin.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.HeightMin, txt_HeightMin);
				txt_HeightMax.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.HeightMax, txt_HeightMax);
				chk_EnableWidthRatio.IsChecked = iNSP_ADVLOCATETOOL_PARAM.EnableFakeWidthRatio;
				chk_EnableHeightRatio.IsChecked = iNSP_ADVLOCATETOOL_PARAM.EnableFakeHeightRatio;
				txt_WidthRatio.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.WidthRatio, txt_WidthRatio);
				txt_HeightRatio.Text = GetComDigitsStr(iNSP_ADVLOCATETOOL_PARAM.HeightRatio, txt_HeightRatio);
				if (iNSP_ADVLOCATETOOL_PARAM.EnableFakeWidthRatio || iNSP_ADVLOCATETOOL_PARAM.EnableFakeHeightRatio)
				{
					rt_EnabledFake.Visibility = Visibility.Visible;
				}
				else
				{
					rt_EnabledFake.Visibility = Visibility.Hidden;
				}
				if (iNSP_ADVLOCATETOOL_PARAM.UseAutoSize)
				{
					rad_AutoSizeA.IsChecked = true;
				}
				else
				{
					rad_FollowA.IsChecked = true;
				}
				chk_ASelectFace.IsChecked = iNSP_ADVLOCATETOOL_PARAM.EnableSelectFace;
				SetSelectFace(iNSP_ADVLOCATETOOL_PARAM.FaceIndex, findex);
				ErrorBin[findex].SelectedIndex = (int)iNSP_ADVLOCATETOOL_PARAM.ErrorBin;
				cb_ParetoChart.SelectedIndex = iNSP_ADVLOCATETOOL_PARAM.ParetoIndex;
			}
			else
			{
				INSP_ADVMEASURETOOL_PARAM iNSP_ADVMEASURETOOL_PARAM = (BackupParamM = inspection.paramFile.AdvLocateMParam[TrackIndex[findex]]);
				if (iNSP_ADVMEASURETOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
				{
					lab_LockMode.Visibility = Visibility.Visible;
				}
				else
				{
					lab_LockMode.Visibility = Visibility.Hidden;
				}
				Label_Index.Content = (TrackIndex[findex] + 1).ToString();
				TrackON[findex].IsChecked = iNSP_ADVMEASURETOOL_PARAM.TrackerON;
				if (iNSP_ADVMEASURETOOL_PARAM.TrackerON)
				{
					ScanBlock[findex].Visibility = Visibility.Visible;
					grd_BinaryBlock.Visibility = Visibility.Hidden;
				}
				else
				{
					ScanBlock[findex].Visibility = Visibility.Hidden;
					grd_BinaryBlock.Visibility = Visibility.Hidden;
				}
				TrackSelect[findex].SelectedIndex = TrackIndex[findex];
				TrackName[findex].Text = iNSP_ADVMEASURETOOL_PARAM.TrackerName;
				chk_MeasureAllX.Visibility = Visibility.Hidden;
				chk_MeasureAllY.Visibility = Visibility.Hidden;
				grd_PtPreview.Visibility = Visibility.Visible;
				grd_KValueSetup.Visibility = Visibility.Hidden;
				txt_ScanExtend.Text = iNSP_ADVMEASURETOOL_PARAM.ScanExtend.ToString();
				txt_ScanShrink.Text = iNSP_ADVMEASURETOOL_PARAM.ScanShrink.ToString();
				if (iNSP_ADVMEASURETOOL_PARAM.MeasureFunction < 2)
				{
					if (iNSP_ADVMEASURETOOL_PARAM.MeasureFunction == 0)
					{
						rad_MeasureW.IsChecked = true;
					}
					else
					{
						rad_MeasureH.IsChecked = true;
					}
					rad_MeasreStartWH.IsEnabled = true;
					rad_MeasreEndWH.IsEnabled = true;
					cb_MeasureStartWHIndex.IsEnabled = true;
					cb_MeasureEndWHIndex.IsEnabled = true;
				}
				else if (iNSP_ADVMEASURETOOL_PARAM.MeasureFunction == 2)
				{
					rad_MeasureD.IsChecked = true;
					rad_MeasreStartWH.IsEnabled = false;
					rad_MeasreEndWH.IsEnabled = false;
					cb_MeasureStartWHIndex.IsEnabled = false;
					cb_MeasureEndWHIndex.IsEnabled = false;
				}
				else if (iNSP_ADVMEASURETOOL_PARAM.MeasureFunction == 3)
				{
					grd_PtPreview.Visibility = Visibility.Hidden;
					grd_KValueSetup.Visibility = Visibility.Visible;
					rad_MeasureK.IsChecked = true;
					chk_MeasureAllX.Visibility = Visibility.Visible;
					chk_MeasureAllY.Visibility = Visibility.Visible;
					if (inspection.paramFile.ThroughputFile.param.EnableCustomRatio)
					{
						grd_CustomRatio.Visibility = Visibility.Visible;
					}
				}
				else
				{
					rad_MeasureS.IsChecked = true;
				}
				chk_MeasureAllX.IsChecked = iNSP_ADVMEASURETOOL_PARAM.MeasureAllX;
				chk_MeasureAllY.IsChecked = iNSP_ADVMEASURETOOL_PARAM.MeasureAllY;
				if (rad_MeasureW.IsChecked == true)
				{
					UpdateAdvMeasureComobox(0);
				}
				else if (rad_MeasureH.IsChecked == true)
				{
					UpdateAdvMeasureComobox(1);
				}
				else if (rad_MeasureD.IsChecked == true)
				{
					UpdateAdvMeasureComobox(2);
				}
				else if (rad_MeasureK.IsChecked == true)
				{
					UpdateAdvMeasureComobox(3);
				}
				else
				{
					UpdateAdvMeasureComobox(4);
				}
				if (iNSP_ADVMEASURETOOL_PARAM.MStartSelect == 0)
				{
					rad_MeasreStartWH.IsChecked = true;
				}
				else
				{
					rad_MeasreStartA.IsChecked = true;
				}
				if (iNSP_ADVMEASURETOOL_PARAM.MEndSelect == 0)
				{
					rad_MeasreEndWH.IsChecked = true;
				}
				else
				{
					rad_MeasreEndA.IsChecked = true;
				}
				cb_MeasureStartWHIndex.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex;
				cb_MeasureStartAIndex.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.MStartAIndex;
				cb_MeasureStartAPt.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.MStartAPt;
				if (cb_MeasureStartWHIndex.SelectedIndex < 0)
				{
					cb_MeasureStartWHIndex.SelectedIndex = 0;
				}
				if (cb_MeasureStartAIndex.SelectedIndex < 0)
				{
					cb_MeasureStartAIndex.SelectedIndex = 0;
				}
				if (cb_MeasureStartAPt.SelectedIndex < 0)
				{
					cb_MeasureStartAPt.SelectedIndex = 0;
				}
				cb_MeasureEndWHIndex.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex;
				cb_MeasureEndAIndex.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.MEndAIndex;
				cb_MeasureEndAPt.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.MEndAPt;
				if (cb_MeasureEndWHIndex.SelectedIndex < 0)
				{
					cb_MeasureEndWHIndex.SelectedIndex = 0;
				}
				if (cb_MeasureEndAIndex.SelectedIndex < 0)
				{
					cb_MeasureEndAIndex.SelectedIndex = 0;
				}
				if (cb_MeasureEndAPt.SelectedIndex < 0)
				{
					cb_MeasureEndAPt.SelectedIndex = 0;
				}
				txt_ValueMin.Text = GetComDigitsStr(iNSP_ADVMEASURETOOL_PARAM.ValueMin, txt_ValueMin);
				txt_ValueMax.Text = GetComDigitsStr(iNSP_ADVMEASURETOOL_PARAM.ValueMax, txt_ValueMax);
				chk_MSelectFace.IsChecked = iNSP_ADVMEASURETOOL_PARAM.EnableSelectFace;
				SetSelectFace(iNSP_ADVMEASURETOOL_PARAM.FaceIndex, findex);
				ErrorBin[findex].SelectedIndex = (int)iNSP_ADVMEASURETOOL_PARAM.ErrorBin;
				cb_ParetoChart.SelectedIndex = iNSP_ADVMEASURETOOL_PARAM.ParetoIndex;
			}
			UpdateUI = true;
		}
		catch
		{
			string text = "ALW02-AdvLocateWindow-CopyValueToUI()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Btn_NextTracker_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			UpdateUI = false;
			if (cb_TrackerSelect_V.SelectedIndex + 1 < 20)
			{
				cb_TrackerSelect_V.SelectedIndex++;
			}
			else
			{
				cb_TrackerSelect_V.SelectedIndex = 2;
			}
			TrackIndex[CurrentFunction] = cb_TrackerSelect_V.SelectedIndex;
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			UpdateUI = true;
		}
	}

	private void Btn_PrevTracker(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			UpdateUI = false;
			if (cb_TrackerSelect_V.SelectedIndex - 1 >= 2)
			{
				cb_TrackerSelect_V.SelectedIndex--;
				TrackIndex[CurrentFunction] = cb_TrackerSelect_V.SelectedIndex;
				CopyValueToUI(CurrentFunction);
				Btn_Test_Click(sender, e);
				UpdateUI = true;
			}
			else
			{
				UpdateUI = true;
			}
		}
	}

	private void Btn_NextTrackerH_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			UpdateUI = false;
			if (cb_TrackerSelect_H.SelectedIndex + 1 < 20)
			{
				cb_TrackerSelect_H.SelectedIndex++;
			}
			else
			{
				cb_TrackerSelect_H.SelectedIndex = 2;
			}
			TrackIndex[CurrentFunction] = cb_TrackerSelect_H.SelectedIndex;
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			UpdateUI = true;
		}
	}

	private void Btn_PrevTrackerH_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			UpdateUI = false;
			if (cb_TrackerSelect_H.SelectedIndex - 1 >= 2)
			{
				cb_TrackerSelect_H.SelectedIndex--;
				TrackIndex[CurrentFunction] = cb_TrackerSelect_H.SelectedIndex;
				CopyValueToUI(CurrentFunction);
				Btn_Test_Click(sender, e);
				UpdateUI = true;
			}
			else
			{
				UpdateUI = true;
			}
		}
	}

	private void Btn_PrevTrackerA_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			UpdateUI = false;
			if (cb_TrackerSelect_A.SelectedIndex - 1 >= 1)
			{
				cb_TrackerSelect_A.SelectedIndex--;
				TrackIndex[CurrentFunction] = cb_TrackerSelect_A.SelectedIndex;
				CopyValueToUI(CurrentFunction);
				Btn_Test_Click(sender, e);
				UpdateUI = true;
			}
			else
			{
				UpdateUI = true;
			}
		}
	}

	private void Btn_NextTrackerA_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			UpdateUI = false;
			if (cb_TrackerSelect_A.SelectedIndex + 1 < 20)
			{
				cb_TrackerSelect_A.SelectedIndex++;
			}
			else
			{
				cb_TrackerSelect_A.SelectedIndex = 2;
			}
			TrackIndex[CurrentFunction] = cb_TrackerSelect_A.SelectedIndex;
			CopyValueToUI(CurrentFunction);
			Btn_Test_Click(sender, e);
			UpdateUI = true;
		}
	}

	private void Btn_Tools1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		TrackSelect[CurrentFunction].SelectedIndex = num;
		if (CurrentFunction <= 2)
		{
			for (int i = 0; i < 20; i++)
			{
				ToolsButton[i].BorderBrush = Brushes.Gray;
			}
			ToolsButton[num].BorderBrush = Brushes.Blue;
			return;
		}
		for (int j = 0; j < 40; j++)
		{
			ToolsButton[j].BorderBrush = Brushes.Gray;
			ToolsButton[j].Foreground = Brushes.Black;
		}
		ToolsButton[num].BorderBrush = Brushes.Blue;
		ToolsButton[num].Foreground = Brushes.Red;
	}

	private void Rad_UseAdvLocate_Checked(object sender, RoutedEventArgs e)
	{
		bor_SearchRect1.Visibility = Visibility.Hidden;
		grd_BinaryBlock.Visibility = Visibility.Hidden;
		rt_UseAdvLocate.Fill = Brushes.LightGreen;
		rt_UseBinary.Fill = Brushes.White;
	}

	private void Rad_UseBinary_Checked(object sender, RoutedEventArgs e)
	{
		bor_SearchRect1.Visibility = Visibility.Visible;
		grd_BinaryBlock.Visibility = Visibility.Visible;
		rt_UseAdvLocate.Fill = Brushes.White;
		rt_UseBinary.Fill = Brushes.LightGreen;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerMoveUp_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		int num = (int)Canvas.GetLeft(bor_SearchRect1);
		int num2 = (int)Canvas.GetTop(bor_SearchRect1);
		int num3 = (int)bor_SearchRect1.Width;
		int num4 = (int)bor_SearchRect1.Height;
		string uid = obj.Uid;
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
				}
				break;
			}
		}
		bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)num);
		bor_SearchRect1.SetValue(Canvas.TopProperty, (double)num2);
		bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)num3);
		bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)num4);
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

	private void Txt_WidthMin_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
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
		hKeyboard.SetDouble(state: true);
		hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
		hKeyboard.Left = p.X;
		hKeyboard.Top = p.Y - hKeyboard.Height + MainW.Top;
		tbox.Background = Brushes.Gold;
		hKeyboard.ShowDialog();
		tbox.Background = Brushes.White;
	}

	private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
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
		if (ColorDropper)
		{
			for (int i = 0; i < 3; i++)
			{
				if (THD_Expender[i].IsExpanded)
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

	private void Chk_VSelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (chk_VSelectFace.IsChecked == true)
		{
			grd_VSelectFace.IsEnabled = true;
			btn_VFaceA.Background = Brushes.Yellow;
			btn_VFaceB.Background = Brushes.White;
			btn_VFaceC.Background = Brushes.White;
			btn_VFaceD.Background = Brushes.White;
			SelectFace[0] = 1;
		}
		else
		{
			grd_VSelectFace.IsEnabled = false;
			btn_VFaceA.Background = Brushes.White;
			btn_VFaceB.Background = Brushes.White;
			btn_VFaceC.Background = Brushes.White;
			btn_VFaceD.Background = Brushes.White;
			SelectFace[0] = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_VFaceA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		btn_VFaceA.Background = Brushes.White;
		btn_VFaceB.Background = Brushes.White;
		btn_VFaceC.Background = Brushes.White;
		btn_VFaceD.Background = Brushes.White;
		string uid = obj.Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (!(uid == "3"))
				{
					if (uid == "4")
					{
						btn_VFaceD.Background = Brushes.Yellow;
						SelectFace[0] = 4;
					}
				}
				else
				{
					btn_VFaceC.Background = Brushes.Yellow;
					SelectFace[0] = 3;
				}
			}
			else
			{
				btn_VFaceB.Background = Brushes.Yellow;
				SelectFace[0] = 2;
			}
		}
		else
		{
			btn_VFaceA.Background = Brushes.Yellow;
			SelectFace[0] = 1;
		}
	}

	private void Chk_HSelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (chk_HSelectFace.IsChecked == true)
		{
			grd_HSelectFace.IsEnabled = true;
			btn_HFaceA.Background = Brushes.Yellow;
			btn_HFaceB.Background = Brushes.White;
			btn_HFaceC.Background = Brushes.White;
			btn_HFaceD.Background = Brushes.White;
			SelectFace[0] = 1;
		}
		else
		{
			grd_HSelectFace.IsEnabled = false;
			btn_HFaceA.Background = Brushes.White;
			btn_HFaceB.Background = Brushes.White;
			btn_HFaceC.Background = Brushes.White;
			btn_HFaceD.Background = Brushes.White;
			SelectFace[0] = 0;
		}
	}

	private void Chk_ASelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (chk_ASelectFace.IsChecked == true)
		{
			grd_ASelectFace.IsEnabled = true;
			btn_AFaceA.Background = Brushes.Yellow;
			btn_AFaceB.Background = Brushes.White;
			btn_AFaceC.Background = Brushes.White;
			btn_AFaceD.Background = Brushes.White;
			SelectFace[0] = 1;
		}
		else
		{
			grd_ASelectFace.IsEnabled = false;
			btn_AFaceA.Background = Brushes.White;
			btn_AFaceB.Background = Brushes.White;
			btn_AFaceC.Background = Brushes.White;
			btn_AFaceD.Background = Brushes.White;
			SelectFace[0] = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_HFaceA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		btn_HFaceA.Background = Brushes.White;
		btn_HFaceB.Background = Brushes.White;
		btn_HFaceC.Background = Brushes.White;
		btn_HFaceD.Background = Brushes.White;
		string uid = obj.Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (!(uid == "3"))
				{
					if (uid == "4")
					{
						btn_HFaceD.Background = Brushes.Yellow;
						SelectFace[1] = 4;
					}
				}
				else
				{
					btn_HFaceC.Background = Brushes.Yellow;
					SelectFace[1] = 3;
				}
			}
			else
			{
				btn_HFaceB.Background = Brushes.Yellow;
				SelectFace[1] = 2;
			}
		}
		else
		{
			btn_HFaceA.Background = Brushes.Yellow;
			SelectFace[1] = 1;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AFaceA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		btn_AFaceA.Background = Brushes.White;
		btn_AFaceB.Background = Brushes.White;
		btn_AFaceC.Background = Brushes.White;
		btn_AFaceD.Background = Brushes.White;
		string uid = obj.Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (!(uid == "3"))
				{
					if (uid == "4")
					{
						btn_AFaceD.Background = Brushes.Yellow;
						SelectFace[2] = 4;
					}
				}
				else
				{
					btn_AFaceC.Background = Brushes.Yellow;
					SelectFace[2] = 3;
				}
			}
			else
			{
				btn_AFaceB.Background = Brushes.Yellow;
				SelectFace[2] = 2;
			}
		}
		else
		{
			btn_AFaceA.Background = Brushes.Yellow;
			SelectFace[2] = 1;
		}
	}

	private void Btn_ValueCopy_Click(object sender, RoutedEventArgs e)
	{
		btn_ValuePaste.IsEnabled = true;
		CopyMinW = ConvertComDigits(txt_WidthMin);
		CopyMaxW = ConvertComDigits(txt_WidthMax);
		CopyMinH = ConvertComDigits(txt_HeightMin);
		CopyMaxH = ConvertComDigits(txt_HeightMax);
	}

	private void Btn_ValuePaste_Click(object sender, RoutedEventArgs e)
	{
		txt_WidthMin.Text = GetComDigitsStr(CopyMinW, txt_WidthMin);
		txt_WidthMax.Text = GetComDigitsStr(CopyMaxW, txt_WidthMax);
		txt_HeightMin.Text = GetComDigitsStr(CopyMinH, txt_HeightMin);
		txt_HeightMax.Text = GetComDigitsStr(CopyMaxH, txt_HeightMax);
	}

	private void Chk_MSelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (chk_MSelectFace.IsChecked == true)
		{
			grd_MSelectFace.IsEnabled = true;
			btn_MFaceA.Background = Brushes.Yellow;
			btn_MFaceB.Background = Brushes.White;
			btn_MFaceC.Background = Brushes.White;
			btn_MFaceD.Background = Brushes.White;
			SelectFace[3] = 1;
		}
		else
		{
			grd_MSelectFace.IsEnabled = false;
			btn_MFaceA.Background = Brushes.White;
			btn_MFaceB.Background = Brushes.White;
			btn_MFaceC.Background = Brushes.White;
			btn_MFaceD.Background = Brushes.White;
			SelectFace[3] = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_MFaceA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		btn_MFaceA.Background = Brushes.White;
		btn_MFaceB.Background = Brushes.White;
		btn_MFaceC.Background = Brushes.White;
		btn_MFaceD.Background = Brushes.White;
		string uid = obj.Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (!(uid == "3"))
				{
					if (uid == "4")
					{
						btn_MFaceD.Background = Brushes.Yellow;
						SelectFace[3] = 4;
					}
				}
				else
				{
					btn_MFaceC.Background = Brushes.Yellow;
					SelectFace[3] = 3;
				}
			}
			else
			{
				btn_MFaceB.Background = Brushes.Yellow;
				SelectFace[3] = 2;
			}
		}
		else
		{
			btn_MFaceA.Background = Brushes.Yellow;
			SelectFace[3] = 1;
		}
	}

	private void Rad_MeasureW_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			UpdateAdvMeasureComobox(0);
		}
	}

	private void Rad_MeasureH_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			UpdateAdvMeasureComobox(1);
		}
	}

	private void Rad_MeasureD_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			rad_MeasreStartA.IsChecked = true;
			rad_MeasreEndA.IsChecked = true;
			UpdateAdvMeasureComobox(2);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam(int findex, int trackindex)
	{
		switch (findex)
		{
		case 0:
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.VSLocateParam[trackindex].Locked)
			{
				return;
			}
			break;
		case 1:
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.HSLocateParam[trackindex].Locked)
			{
				return;
			}
			break;
		case 2:
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.AdvLocateParam[trackindex].Locked)
			{
				return;
			}
			break;
		case 3:
			if (User < User_Permissions.USER_Administrator && inspection.paramFile.AdvLocateMParam[trackindex].Locked)
			{
				return;
			}
			break;
		}
		if (PreviewMode)
		{
			return;
		}
		try
		{
			UpdateUI = false;
			if (findex < 2)
			{
				if (trackindex < 2)
				{
					return;
				}
				INSP_SCANLINETOOL_PARAM iNSP_SCANLINETOOL_PARAM = default(INSP_SCANLINETOOL_PARAM);
				iNSP_SCANLINETOOL_PARAM = ((findex != 0) ? inspection.paramFile.HSLocateParam[trackindex] : inspection.paramFile.VSLocateParam[trackindex]);
				iNSP_SCANLINETOOL_PARAM.ValueChangeFlag = true;
				iNSP_SCANLINETOOL_PARAM.TrackerON = TrackON[findex].IsChecked.Value;
				iNSP_SCANLINETOOL_PARAM.TrackerName = TrackName[findex].Text;
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
				GetTracker(ref rect);
				if (findex == 0)
				{
					if (rad_FollowV.IsChecked == true)
					{
						iNSP_SCANLINETOOL_PARAM.UseAutoSize = false;
					}
					else
					{
						iNSP_SCANLINETOOL_PARAM.UseAutoSize = true;
					}
				}
				else if (rad_FollowH.IsChecked == true)
				{
					iNSP_SCANLINETOOL_PARAM.UseAutoSize = false;
				}
				else
				{
					iNSP_SCANLINETOOL_PARAM.UseAutoSize = true;
				}
				if (DefaultSearchRect != rect || iNSP_SCANLINETOOL_PARAM.FollowMode != cb_FollowVMode.SelectedIndex)
				{
					iNSP_SCANLINETOOL_PARAM.FollowMode = cb_FollowVMode.SelectedIndex;
					if (iNSP_SCANLINETOOL_PARAM.UseAutoSize)
					{
						iNSP_SCANLINETOOL_PARAM.SearchRectL = rect.X - LocateRect.X;
						iNSP_SCANLINETOOL_PARAM.SearchRectT = rect.Y - LocateRect.Y;
						iNSP_SCANLINETOOL_PARAM.SearchRectR = LocateRect.Width - iNSP_SCANLINETOOL_PARAM.SearchRectL - rect.Width;
						iNSP_SCANLINETOOL_PARAM.SearchRectB = LocateRect.Height - iNSP_SCANLINETOOL_PARAM.SearchRectT - rect.Height;
						iNSP_SCANLINETOOL_PARAM.SearchRectWidth = rect.Width;
						iNSP_SCANLINETOOL_PARAM.SearchRectHeight = rect.Height;
						DefaultSearchRect = rect;
					}
					else
					{
						if (iNSP_SCANLINETOOL_PARAM.FollowMode == 0)
						{
							iNSP_SCANLINETOOL_PARAM.SearchRectL = rect.X - LocateRect.X;
							iNSP_SCANLINETOOL_PARAM.SearchRectT = rect.Y - LocateRect.Y;
						}
						else if (iNSP_SCANLINETOOL_PARAM.FollowMode == 1)
						{
							iNSP_SCANLINETOOL_PARAM.SearchRectL = rect.X - LocateRect.X;
							iNSP_SCANLINETOOL_PARAM.SearchRectT = rect.Y - LocateRect.Y - LocateRect.Height;
						}
						else if (iNSP_SCANLINETOOL_PARAM.FollowMode == 2)
						{
							iNSP_SCANLINETOOL_PARAM.SearchRectL = rect.X - LocateRect.X - LocateRect.Width;
							iNSP_SCANLINETOOL_PARAM.SearchRectT = rect.Y - LocateRect.Y;
						}
						else
						{
							iNSP_SCANLINETOOL_PARAM.SearchRectL = rect.X - LocateRect.X - LocateRect.Width;
							iNSP_SCANLINETOOL_PARAM.SearchRectT = rect.Y - LocateRect.Y - LocateRect.Height;
						}
						iNSP_SCANLINETOOL_PARAM.SearchRectR = iNSP_SCANLINETOOL_PARAM.SearchRectL + rect.Width;
						iNSP_SCANLINETOOL_PARAM.SearchRectB = iNSP_SCANLINETOOL_PARAM.SearchRectT + rect.Height;
						iNSP_SCANLINETOOL_PARAM.SearchRectWidth = rect.Width;
						iNSP_SCANLINETOOL_PARAM.SearchRectHeight = rect.Height;
						DefaultSearchRect = rect;
					}
				}
				iNSP_SCANLINETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.THD1_ChannelON = exp_THD1.IsExpanded;
				iNSP_SCANLINETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.THD2_ChannelON = exp_THD2.IsExpanded;
				iNSP_SCANLINETOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.THD3_ChannelON = exp_THD3.IsExpanded;
				iNSP_SCANLINETOOL_PARAM.THD1_Min = (int)rs_THD1.LowerValue;
				iNSP_SCANLINETOOL_PARAM.THD1_Max = (int)rs_THD1.UpperValue;
				iNSP_SCANLINETOOL_PARAM.THD2_Min = (int)rs_THD2.LowerValue;
				iNSP_SCANLINETOOL_PARAM.THD2_Max = (int)rs_THD2.UpperValue;
				iNSP_SCANLINETOOL_PARAM.THD3_Min = (int)rs_THD3.LowerValue;
				iNSP_SCANLINETOOL_PARAM.THD3_Max = (int)rs_THD3.UpperValue;
				iNSP_SCANLINETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
				if (findex == 0)
				{
					if (ScanDirectionBtn[findex].IsChecked.Value)
					{
						iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)(findex * 2);
					}
					else
					{
						iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)(1 + findex * 2);
					}
				}
				else if (rad_Direction_H3.IsChecked == true)
				{
					iNSP_SCANLINETOOL_PARAM.Direction = ScanDirection.EM_LEFT_RIGHT_HOLE;
				}
				else if (rad_Direction_H4.IsChecked == true)
				{
					iNSP_SCANLINETOOL_PARAM.Direction = ScanDirection.EM_RIGHT_LEFT_HOLE;
				}
				else if (ScanDirectionBtn[findex].IsChecked.Value)
				{
					iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)(findex * 2);
				}
				else
				{
					iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)(1 + findex * 2);
				}
				iNSP_SCANLINETOOL_PARAM.FindEdgeMethod = FindEdgeMethod[findex].SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.ErrorBin = (FLOW_BIN)ErrorBin[findex].SelectedIndex;
				iNSP_SCANLINETOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
				if (findex == 0)
				{
					iNSP_SCANLINETOOL_PARAM.UseStdFilter = chk_UseStdFilterV.IsChecked.Value;
					iNSP_SCANLINETOOL_PARAM.EnableSelectFace = chk_VSelectFace.IsChecked.Value;
					iNSP_SCANLINETOOL_PARAM.UseColorInverse = chk_UseColorInverseV.IsChecked.Value;
				}
				else
				{
					iNSP_SCANLINETOOL_PARAM.UseStdFilter = chk_UseStdFilterH.IsChecked.Value;
					iNSP_SCANLINETOOL_PARAM.EnableSelectFace = chk_HSelectFace.IsChecked.Value;
					iNSP_SCANLINETOOL_PARAM.UseColorInverse = chk_UseColorInverseH.IsChecked.Value;
				}
				iNSP_SCANLINETOOL_PARAM.FaceIndex = SelectFace[findex];
				if (!iNSP_SCANLINETOOL_PARAM.EnableSelectFace)
				{
					iNSP_SCANLINETOOL_PARAM.FaceIndex = 0;
				}
				if (findex == 0)
				{
					iNSP_SCANLINETOOL_PARAM.ValueChangeFlag = false;
					if (!iNSP_SCANLINETOOL_PARAM.Equals(inspection.paramFile_Backup.VSLocateParam[trackindex]))
					{
						iNSP_SCANLINETOOL_PARAM.ValueChangeFlag = true;
						inspection.paramFile.VSLocateParam[trackindex] = iNSP_SCANLINETOOL_PARAM;
					}
				}
				else
				{
					iNSP_SCANLINETOOL_PARAM.ValueChangeFlag = false;
					if (!iNSP_SCANLINETOOL_PARAM.Equals(inspection.paramFile_Backup.HSLocateParam[trackindex]))
					{
						iNSP_SCANLINETOOL_PARAM.ValueChangeFlag = true;
						inspection.paramFile.HSLocateParam[trackindex] = iNSP_SCANLINETOOL_PARAM;
					}
				}
				int selectedIndex = TrackSelect[findex].SelectedIndex;
				TrackSelect[findex].Items.Clear();
				for (int i = 0; i < 20; i++)
				{
					if (i < 2)
					{
						if (findex == 0)
						{
							ComboBoxItem comboBoxItem = new ComboBoxItem();
							comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
							comboBoxItem.IsEnabled = false;
							TrackSelect[findex].Items.Add(comboBoxItem);
						}
						else
						{
							ComboBoxItem comboBoxItem2 = new ComboBoxItem();
							comboBoxItem2.Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
							comboBoxItem2.IsEnabled = false;
							TrackSelect[findex].Items.Add(comboBoxItem2);
						}
					}
					else if (findex == 0)
					{
						TrackSelect[findex].Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
					}
					else
					{
						TrackSelect[findex].Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
					}
				}
				cb_RectIndexL.Items.Clear();
				cb_RectIndexR.Items.Clear();
				cb_RectIndexT.Items.Clear();
				cb_RectIndexB.Items.Clear();
				for (int j = 0; j < 20; j++)
				{
					if (j < 2)
					{
						inspection.paramFile.VSLocateParam[j].TrackerON = true;
						inspection.paramFile.HSLocateParam[j].TrackerON = true;
					}
					if (inspection.paramFile.VSLocateParam[j].TrackerON)
					{
						cb_RectIndexL.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
						cb_RectIndexR.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
					}
					else
					{
						ComboBoxItem comboBoxItem3 = new ComboBoxItem();
						comboBoxItem3.Content = j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName;
						comboBoxItem3.IsEnabled = false;
						cb_RectIndexL.Items.Add(comboBoxItem3);
						ComboBoxItem comboBoxItem4 = new ComboBoxItem();
						comboBoxItem4.Content = j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName;
						comboBoxItem4.IsEnabled = false;
						cb_RectIndexR.Items.Add(comboBoxItem4);
					}
					if (inspection.paramFile.HSLocateParam[j].TrackerON)
					{
						cb_RectIndexT.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
						cb_RectIndexB.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
						continue;
					}
					ComboBoxItem comboBoxItem5 = new ComboBoxItem();
					comboBoxItem5.Content = j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName;
					comboBoxItem5.IsEnabled = false;
					cb_RectIndexT.Items.Add(comboBoxItem5);
					ComboBoxItem comboBoxItem6 = new ComboBoxItem();
					comboBoxItem6.Content = j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName;
					comboBoxItem6.IsEnabled = false;
					cb_RectIndexB.Items.Add(comboBoxItem6);
				}
				TrackSelect[findex].SelectedIndex = selectedIndex;
			}
			else if (findex == 2)
			{
				if (!CheckTextBox())
				{
					txt_AllResult.Clear();
					txt_AllResult.Text = LanguageManager.Instance["InputError"];
					return;
				}
				if (trackindex < 1)
				{
					return;
				}
				INSP_ADVLOCATETOOL_PARAM iNSP_ADVLOCATETOOL_PARAM = default(INSP_ADVLOCATETOOL_PARAM);
				iNSP_ADVLOCATETOOL_PARAM = inspection.paramFile.AdvLocateParam[trackindex];
				iNSP_ADVLOCATETOOL_PARAM.ValueChangeFlag = true;
				iNSP_ADVLOCATETOOL_PARAM.TrackerON = TrackON[findex].IsChecked.Value;
				iNSP_ADVLOCATETOOL_PARAM.TrackerName = TrackName[findex].Text;
				iNSP_ADVLOCATETOOL_PARAM.RectIndexL = cb_RectIndexL.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.RectIndexR = cb_RectIndexR.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.RectIndexT = cb_RectIndexT.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.RectIndexB = cb_RectIndexB.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftT = ConvertComDigits(txt_RectShiftT);
				iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftB = ConvertComDigits(txt_RectShiftB);
				iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftL = ConvertComDigits(txt_RectShiftL);
				iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftR = ConvertComDigits(txt_RectShiftR);
				iNSP_ADVLOCATETOOL_PARAM.WidthMin = ConvertComDigits(txt_WidthMin);
				iNSP_ADVLOCATETOOL_PARAM.WidthMax = ConvertComDigits(txt_WidthMax);
				iNSP_ADVLOCATETOOL_PARAM.HeightMin = ConvertComDigits(txt_HeightMin);
				iNSP_ADVLOCATETOOL_PARAM.HeightMax = ConvertComDigits(txt_HeightMax);
				iNSP_ADVLOCATETOOL_PARAM.EnableFakeHeightRatio = chk_EnableHeightRatio.IsChecked.Value;
				iNSP_ADVLOCATETOOL_PARAM.EnableFakeWidthRatio = chk_EnableWidthRatio.IsChecked.Value;
				iNSP_ADVLOCATETOOL_PARAM.WidthRatio = ConvertComDigits(txt_WidthRatio);
				iNSP_ADVLOCATETOOL_PARAM.HeightRatio = ConvertComDigits(txt_HeightRatio);
				iNSP_ADVLOCATETOOL_PARAM.ErrorBin = (FLOW_BIN)ErrorBin[findex].SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
				int selectedIndex2 = TrackSelect[findex].SelectedIndex;
				TrackSelect[findex].Items.Clear();
				for (int k = 0; k < 20; k++)
				{
					if (k == 0)
					{
						ComboBoxItem comboBoxItem7 = new ComboBoxItem();
						comboBoxItem7.Content = k + 1 + " : " + inspection.paramFile.AdvLocateParam[k].TrackerName;
						comboBoxItem7.IsEnabled = false;
						TrackSelect[findex].Items.Add(comboBoxItem7);
					}
					else
					{
						TrackSelect[findex].Items.Add(k + 1 + " : " + inspection.paramFile.AdvLocateParam[k].TrackerName);
					}
				}
				TrackSelect[findex].SelectedIndex = selectedIndex2;
				if (rad_UseBinary.IsChecked == true)
				{
					iNSP_ADVLOCATETOOL_PARAM.UseBinary = true;
				}
				else
				{
					iNSP_ADVLOCATETOOL_PARAM.UseBinary = false;
				}
				if (rad_AutoSizeA.IsChecked == true)
				{
					iNSP_ADVLOCATETOOL_PARAM.UseAutoSize = true;
				}
				else
				{
					iNSP_ADVLOCATETOOL_PARAM.UseAutoSize = false;
				}
				iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex = cb_FollowA.SelectedIndex;
				if (iNSP_ADVLOCATETOOL_PARAM.UseBinary)
				{
					OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
					GetTracker(ref rect2);
					int selectedIndex3 = cb_FollowA.SelectedIndex;
					if (selectedIndex3 == 0 || !iNSP_ADVLOCATETOOL_PARAM.UseAutoSize)
					{
						iNSP_ADVLOCATETOOL_PARAM.SearchRectL = rect2.X - LocateRect.X;
						iNSP_ADVLOCATETOOL_PARAM.SearchRectT = rect2.Y - LocateRect.Y;
						iNSP_ADVLOCATETOOL_PARAM.SearchRectR = LocateRect.Width - iNSP_ADVLOCATETOOL_PARAM.SearchRectL - rect2.Width;
						iNSP_ADVLOCATETOOL_PARAM.SearchRectB = LocateRect.Height - iNSP_ADVLOCATETOOL_PARAM.SearchRectT - rect2.Height;
					}
					else
					{
						iNSP_ADVLOCATETOOL_PARAM.SearchRectL = rect2.X - inspection.paramFile.AdvLocateParam[selectedIndex3].InsideRect.Left;
						iNSP_ADVLOCATETOOL_PARAM.SearchRectT = rect2.Y - inspection.paramFile.AdvLocateParam[selectedIndex3].InsideRect.Top;
						iNSP_ADVLOCATETOOL_PARAM.SearchRectR = inspection.paramFile.AdvLocateParam[selectedIndex3].InsideRect.Width - iNSP_ADVLOCATETOOL_PARAM.SearchRectL - rect2.Width;
						iNSP_ADVLOCATETOOL_PARAM.SearchRectB = inspection.paramFile.AdvLocateParam[selectedIndex3].InsideRect.Height - iNSP_ADVLOCATETOOL_PARAM.SearchRectT - rect2.Height;
					}
					iNSP_ADVLOCATETOOL_PARAM.SearchRectW = rect2.Width;
					iNSP_ADVLOCATETOOL_PARAM.SearchRectH = rect2.Height;
					iNSP_ADVLOCATETOOL_PARAM.SearchRectWidth = rect2.Width;
					iNSP_ADVLOCATETOOL_PARAM.SearchRectHeight = rect2.Height;
					DefaultSearchRect = rect2;
				}
				iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelON = exp_THD1.IsExpanded;
				iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelON = exp_THD2.IsExpanded;
				iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelON = exp_THD3.IsExpanded;
				iNSP_ADVLOCATETOOL_PARAM.THD1_Min = (int)rs_THD1.LowerValue;
				iNSP_ADVLOCATETOOL_PARAM.THD1_Max = (int)rs_THD1.UpperValue;
				iNSP_ADVLOCATETOOL_PARAM.THD2_Min = (int)rs_THD2.LowerValue;
				iNSP_ADVLOCATETOOL_PARAM.THD2_Max = (int)rs_THD2.UpperValue;
				iNSP_ADVLOCATETOOL_PARAM.THD3_Min = (int)rs_THD3.LowerValue;
				iNSP_ADVLOCATETOOL_PARAM.THD3_Max = (int)rs_THD3.UpperValue;
				iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
				iNSP_ADVLOCATETOOL_PARAM.EnableSelectFace = chk_ASelectFace.IsChecked.Value;
				iNSP_ADVLOCATETOOL_PARAM.FaceIndex = SelectFace[findex];
				if (!iNSP_ADVLOCATETOOL_PARAM.EnableSelectFace)
				{
					iNSP_ADVLOCATETOOL_PARAM.FaceIndex = 0;
				}
				iNSP_ADVLOCATETOOL_PARAM.ValueChangeFlag = false;
				if (!iNSP_ADVLOCATETOOL_PARAM.Equals(inspection.paramFile_Backup.AdvLocateParam[trackindex]))
				{
					iNSP_ADVLOCATETOOL_PARAM.ValueChangeFlag = true;
					inspection.paramFile.AdvLocateParam[trackindex] = iNSP_ADVLOCATETOOL_PARAM;
				}
			}
			else
			{
				if (!CheckTextBox())
				{
					txt_AllResult.Clear();
					txt_AllResult.Text = LanguageManager.Instance["InputError"];
					return;
				}
				INSP_ADVMEASURETOOL_PARAM iNSP_ADVMEASURETOOL_PARAM = default(INSP_ADVMEASURETOOL_PARAM);
				iNSP_ADVMEASURETOOL_PARAM = inspection.paramFile.AdvLocateMParam[trackindex];
				iNSP_ADVMEASURETOOL_PARAM.ValueChangeFlag = true;
				iNSP_ADVMEASURETOOL_PARAM.TrackerON = TrackON[findex].IsChecked.Value;
				iNSP_ADVMEASURETOOL_PARAM.TrackerName = TrackName[findex].Text;
				if (rad_MeasureW.IsChecked == true)
				{
					iNSP_ADVMEASURETOOL_PARAM.MeasureFunction = 0;
				}
				else if (rad_MeasureH.IsChecked == true)
				{
					iNSP_ADVMEASURETOOL_PARAM.MeasureFunction = 1;
				}
				else if (rad_MeasureD.IsChecked == true)
				{
					iNSP_ADVMEASURETOOL_PARAM.MeasureFunction = 2;
				}
				else if (rad_MeasureK.IsChecked == true)
				{
					iNSP_ADVMEASURETOOL_PARAM.MeasureFunction = 3;
				}
				else
				{
					iNSP_ADVMEASURETOOL_PARAM.MeasureFunction = 4;
				}
				if (rad_MeasreStartWH.IsChecked == true)
				{
					iNSP_ADVMEASURETOOL_PARAM.MStartSelect = 0;
				}
				else
				{
					iNSP_ADVMEASURETOOL_PARAM.MStartSelect = 1;
				}
				if (rad_MeasreEndWH.IsChecked == true)
				{
					iNSP_ADVMEASURETOOL_PARAM.MEndSelect = 0;
				}
				else
				{
					iNSP_ADVMEASURETOOL_PARAM.MEndSelect = 1;
				}
				if (cb_MeasureStartWHIndex.SelectedIndex < 0)
				{
					cb_MeasureStartWHIndex.SelectedIndex = 0;
				}
				if (cb_MeasureStartAIndex.SelectedIndex < 0)
				{
					cb_MeasureStartAIndex.SelectedIndex = 0;
				}
				if (cb_MeasureStartAPt.SelectedIndex < 0)
				{
					cb_MeasureStartAPt.SelectedIndex = 0;
				}
				iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex = cb_MeasureStartWHIndex.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.MStartAIndex = cb_MeasureStartAIndex.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.MStartAPt = cb_MeasureStartAPt.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.ScanExtend = Convert.ToInt32(txt_ScanExtend.Text);
				iNSP_ADVMEASURETOOL_PARAM.ScanShrink = Convert.ToInt32(txt_ScanShrink.Text);
				if (cb_MeasureEndWHIndex.SelectedIndex < 0)
				{
					cb_MeasureEndWHIndex.SelectedIndex = 0;
				}
				if (cb_MeasureEndAIndex.SelectedIndex < 0)
				{
					cb_MeasureEndAIndex.SelectedIndex = 0;
				}
				if (cb_MeasureEndAPt.SelectedIndex < 0)
				{
					cb_MeasureEndAPt.SelectedIndex = 0;
				}
				iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex = cb_MeasureEndWHIndex.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.MEndAIndex = cb_MeasureEndAIndex.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.MEndAPt = cb_MeasureEndAPt.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.ValueMin = ConvertComDigits(txt_ValueMin);
				iNSP_ADVMEASURETOOL_PARAM.ValueMax = ConvertComDigits(txt_ValueMax);
				iNSP_ADVMEASURETOOL_PARAM.MeasureAllX = chk_MeasureAllX.IsChecked.Value;
				iNSP_ADVMEASURETOOL_PARAM.MeasureAllY = chk_MeasureAllY.IsChecked.Value;
				iNSP_ADVMEASURETOOL_PARAM.ErrorBin = (FLOW_BIN)ErrorBin[findex].SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
				iNSP_ADVMEASURETOOL_PARAM.EnableSelectFace = chk_MSelectFace.IsChecked.Value;
				iNSP_ADVMEASURETOOL_PARAM.FaceIndex = SelectFace[findex];
				if (!iNSP_ADVMEASURETOOL_PARAM.EnableSelectFace)
				{
					iNSP_ADVMEASURETOOL_PARAM.FaceIndex = 0;
				}
				iNSP_ADVMEASURETOOL_PARAM.ValueChangeFlag = false;
				inspection.paramFile_Backup.AdvLocateMParam[trackindex].MeasureValue = iNSP_ADVMEASURETOOL_PARAM.MeasureValue;
				if (!iNSP_ADVMEASURETOOL_PARAM.Equals(inspection.paramFile_Backup.AdvLocateMParam[trackindex]))
				{
					iNSP_ADVMEASURETOOL_PARAM.ValueChangeFlag = true;
					inspection.paramFile.AdvLocateMParam[trackindex] = iNSP_ADVMEASURETOOL_PARAM;
				}
			}
			UpdateUI = true;
		}
		catch
		{
			string text = "ALW03-AdvLocateWindow-WriteUIValue()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode)
		{
			return;
		}
		if (CurrentFunction == 0)
		{
			if (ts_TrackerON_V.IsChecked == false)
			{
				DrawBackGround();
				return;
			}
		}
		else if (CurrentFunction == 1)
		{
			if (ts_TrackerON_H.IsChecked == false)
			{
				DrawBackGround();
				return;
			}
		}
		else if (CurrentFunction == 2)
		{
			if (ts_TrackerON_A.IsChecked == false)
			{
				DrawBackGround();
				return;
			}
			if (!CheckTextBox())
			{
				txt_AllResult.Clear();
				txt_AllResult.Text = LanguageManager.Instance["InputError"];
				return;
			}
		}
		else
		{
			if (ts_TrackerON_M.IsChecked == false)
			{
				DrawBackGround();
				return;
			}
			if (!CheckTextBox())
			{
				txt_AllResult.Clear();
				txt_AllResult.Text = LanguageManager.Instance["InputError"];
				return;
			}
		}
		WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
		inspection.advLocateTool.ErrorMsg = "";
		bool flag = false;
		if (CurrentFunction == 0)
		{
			flag = chk_VSelectFace.IsChecked.Value;
			rt_ResultV.Fill = Brushes.White;
		}
		else if (CurrentFunction == 1)
		{
			flag = chk_HSelectFace.IsChecked.Value;
			rt_ResultH.Fill = Brushes.White;
		}
		else if (CurrentFunction == 2)
		{
			flag = chk_ASelectFace.IsChecked.Value;
			rt_ResultA.Fill = Brushes.White;
		}
		else
		{
			flag = chk_MSelectFace.IsChecked.Value;
			rt_ResultM.Fill = Brushes.White;
		}
		if (flag && FaceIndex != 0 && FaceIndex != SelectFace[CurrentFunction])
		{
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
			lab_Time3.Content = "0";
			lab_Time4.Content = "0";
			return;
		}
		Label_ResultError.Visibility = Visibility.Hidden;
		new OpenCvSharp.Rect(0, 0, 0, 0);
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect);
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc, ShowBitmap);
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		try
		{
			if (CurrentFunction == 0)
			{
				DoThdImage();
				new Mat(ScanBinary, rect).Clone();
				if (BinaryBlob.Empty())
				{
					return;
				}
				TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
				inspection.advLocateTool.doVScan(TrackIndex[CurrentFunction], ref iresult, FaceIndex);
				int value = inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].Value;
				if (iresult == TOOL_RESULT.EM_OK)
				{
					ShowBitmap.Line(new OpenCvSharp.Point(value, rect.Y), new OpenCvSharp.Point(value, rect.Y + rect.Height), Scalar.Red);
					SetResult(result: true);
					txt_AllResult.Clear();
					if (LanguageType == 1)
					{
						txt_AllResult.Text = string.Format("[垂直邊][{0}] X = {1} \n", TrackIndex[CurrentFunction] + 1, value);
					}
					else if (LanguageType == 2)
					{
						txt_AllResult.Text = string.Format("[垂直边][{0}] X = {1} \n", TrackIndex[CurrentFunction] + 1, value);
					}
					else if (LanguageType == 3)
					{
						txt_AllResult.Text = string.Format("[垂直な辺][{0}] X = {1} \n", TrackIndex[CurrentFunction] + 1, value);
					}
					else
					{
						txt_AllResult.Text = string.Format("[Vertical][{0}] X = {1} \n", TrackIndex[CurrentFunction] + 1, value);
					}
					inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].Value = value;
					inspection.iResult.VScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_OK;
					UpdateToolsState(CurrentFunction);
					ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
				}
				else
				{
					SetResult(result: false);
					txt_AllResult.Clear();
					if (LanguageType == 1)
					{
						txt_AllResult.Text = string.Format("[垂直邊][{0}] 邊緣合格點數小於2 \n", TrackIndex[CurrentFunction] + 1);
					}
					else if (LanguageType == 2)
					{
						txt_AllResult.Text = string.Format("[垂直边][{0}] 边缘合格点数小于2 \n", TrackIndex[CurrentFunction] + 1);
					}
					else if (LanguageType == 3)
					{
						txt_AllResult.Text = string.Format("[垂直な辺] [{0}] のエッジ合格点数が 2 未満です。 \n", TrackIndex[CurrentFunction] + 1);
					}
					else
					{
						txt_AllResult.Text = string.Format("[Vertical][{0}] Pass edge point < 2 \n", TrackIndex[CurrentFunction] + 1);
					}
					inspection.paramFile.VSLocateParam[TrackIndex[CurrentFunction]].Value = -1;
					inspection.iResult.VScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
					UpdateToolsState(CurrentFunction);
					ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Red;
				}
			}
			else if (CurrentFunction == 1)
			{
				DoThdImage();
				Mat binary = MM.newMat(ScanBinary, rect).Clone();
				if (BinaryBlob.Empty())
				{
					return;
				}
				if (rad_Direction_H3.IsChecked == true || rad_Direction_H4.IsChecked == true)
				{
					bool flag2 = false;
					flag2 = rad_Direction_H3.IsChecked == true;
					if (advLocateTool.ScanVLineHole(binary, rect, out var ResultY, flag2, cb_FindEdgeMethod_H.SelectedIndex))
					{
						ShowBitmap.Line(new OpenCvSharp.Point(rect.X, ResultY), new OpenCvSharp.Point(rect.X + rect.Width, ResultY), Scalar.Red);
						SetResult(result: true);
						txt_AllResult.Clear();
						if (LanguageType == 1)
						{
							txt_AllResult.Text = string.Format("[水平邊][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, ResultY);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text = string.Format("[水平边][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, ResultY);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text = string.Format("[水平な辺][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, ResultY);
						}
						else
						{
							txt_AllResult.Text = string.Format("[Horizontal][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, ResultY);
						}
						inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Value = ResultY;
						inspection.iResult.HScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_OK;
						UpdateToolsState(CurrentFunction);
						ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
					}
					else
					{
						SetResult(result: false);
						txt_AllResult.Clear();
						if (LanguageType == 1)
						{
							txt_AllResult.Text = string.Format("[水平邊][{0}] 邊緣合格點數小於2 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text = string.Format("[水平边][{0}] 边缘合格点数小于2 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text = string.Format("[水平な辺] [{0}] のエッジ合格点数が 2 未満です。 \n", TrackIndex[CurrentFunction] + 1);
						}
						else
						{
							txt_AllResult.Text = string.Format("[Horizontal][{0}] Pass edge point < 2 \n", TrackIndex[CurrentFunction] + 1);
						}
						inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Value = -1;
						inspection.iResult.HScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
						UpdateToolsState(CurrentFunction);
						ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Red;
					}
				}
				else
				{
					TOOL_RESULT iresult2 = TOOL_RESULT.EM_OK;
					inspection.advLocateTool.doHScan(TrackIndex[CurrentFunction], ref iresult2, FaceIndex);
					int value2 = inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Value;
					if (iresult2 == TOOL_RESULT.EM_OK)
					{
						ShowBitmap.Line(new OpenCvSharp.Point(rect.X, value2), new OpenCvSharp.Point(rect.X + rect.Width, value2), Scalar.Red);
						SetResult(result: true);
						txt_AllResult.Clear();
						if (LanguageType == 1)
						{
							txt_AllResult.Text = string.Format("[水平邊][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, value2);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text = string.Format("[水平边][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, value2);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text = string.Format("[水平な辺][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, value2);
						}
						else
						{
							txt_AllResult.Text = string.Format("[Horizontal][{0}] Y = {1} \n", TrackIndex[CurrentFunction] + 1, value2);
						}
						inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Value = value2;
						inspection.iResult.HScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_OK;
						UpdateToolsState(CurrentFunction);
						ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
					}
					else
					{
						SetResult(result: false);
						txt_AllResult.Clear();
						if (LanguageType == 1)
						{
							txt_AllResult.Text = string.Format("[水平邊][{0}] 邊緣合格點數小於2 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text = string.Format("[水平边][{0}] 边缘合格点数小于2 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text = string.Format("[水平な辺] [{0}] のエッジ合格点数が 2 未満です。 \n", TrackIndex[CurrentFunction] + 1);
						}
						else
						{
							txt_AllResult.Text = string.Format("[Horizontal][{0}] Pass edge point < 2 \n", TrackIndex[CurrentFunction] + 1);
						}
						inspection.paramFile.HSLocateParam[TrackIndex[CurrentFunction]].Value = -1;
						inspection.iResult.HScanResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
						UpdateToolsState(CurrentFunction);
						ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Red;
					}
				}
			}
			else if (CurrentFunction == 2)
			{
				if (rad_UseBinary.IsChecked == false)
				{
					int num = (int)(ConvertComDigits(txt_RectShiftL, ForSave: false) / inspection.paramFile.PixelPerUM_X);
					int num2 = (int)(ConvertComDigits(txt_RectShiftR, ForSave: false) / inspection.paramFile.PixelPerUM_X);
					int num3 = (int)(ConvertComDigits(txt_RectShiftT, ForSave: false) / inspection.paramFile.PixelPerUM_Y);
					int num4 = (int)(ConvertComDigits(txt_RectShiftB, ForSave: false) / inspection.paramFile.PixelPerUM_Y);
					if (num > ImageWidth)
					{
						txt_RectShiftL.Background = Brushes.Fuchsia;
						if (LanguageType == 1)
						{
							MessageBox.Show("內部定位左內縮超過影像寬度");
						}
						else if (LanguageType == 2)
						{
							MessageBox.Show("内部定位左内缩超过影像宽度");
						}
						else if (LanguageType == 3)
						{
							MessageBox.Show("内部配置の左インセットが画像幅を超えています。");
						}
						else
						{
							MessageBox.Show("Inside Locate left offset exceeds image width.");
						}
						return;
					}
					txt_RectShiftL.Background = Brushes.White;
					if (num2 > ImageWidth)
					{
						txt_RectShiftR.Background = Brushes.Fuchsia;
						if (LanguageType == 1)
						{
							MessageBox.Show("內部定位右內縮超過影像寬度");
						}
						else if (LanguageType == 2)
						{
							MessageBox.Show("内部定位右内缩超过影像宽度");
						}
						else if (LanguageType == 3)
						{
							MessageBox.Show("内部配置の右インセットが画像幅を超えています。");
						}
						else
						{
							MessageBox.Show("Inside Locate right offset exceeds image width.");
						}
						return;
					}
					txt_RectShiftR.Background = Brushes.White;
					if (num3 > ImageHeight)
					{
						txt_RectShiftT.Background = Brushes.Fuchsia;
						if (LanguageType == 1)
						{
							MessageBox.Show("內部定位上內縮超過影像高度");
						}
						else if (LanguageType == 2)
						{
							MessageBox.Show("内部定位上内缩超过影像高度");
						}
						else if (LanguageType == 3)
						{
							MessageBox.Show("内部配置の上インセットが画像高さを超えています。");
						}
						else
						{
							MessageBox.Show("Inside Locate top offset exceeds image height.");
						}
						return;
					}
					txt_RectShiftT.Background = Brushes.White;
					if (num4 > ImageHeight)
					{
						txt_RectShiftB.Background = Brushes.Fuchsia;
						if (LanguageType == 1)
						{
							MessageBox.Show("內部定位下內縮超過影像高度");
						}
						else if (LanguageType == 2)
						{
							MessageBox.Show("内部定位下内缩超过影像高度");
						}
						else if (LanguageType == 3)
						{
							MessageBox.Show("内部配置の下インセットが画像高さを超えています。");
						}
						else
						{
							MessageBox.Show("Inside Locate bottom offset exceeds image height.");
						}
						return;
					}
					txt_RectShiftB.Background = Brushes.White;
					int num5 = inspection.paramFile.VSLocateParam[cb_RectIndexL.SelectedIndex].Value + num;
					int num6 = inspection.paramFile.VSLocateParam[cb_RectIndexR.SelectedIndex].Value - num2;
					int num7 = inspection.paramFile.HSLocateParam[cb_RectIndexT.SelectedIndex].Value + num3;
					int num8 = inspection.paramFile.HSLocateParam[cb_RectIndexB.SelectedIndex].Value - num4;
					if (num7 != -1)
					{
						ShowBitmap.Line(new OpenCvSharp.Point(0, num7), new OpenCvSharp.Point(ShowBitmap.Cols, num7), Scalar.Yellow);
					}
					if (num8 != -1)
					{
						ShowBitmap.Line(new OpenCvSharp.Point(0, num8), new OpenCvSharp.Point(ShowBitmap.Cols, num8), Scalar.Yellow);
					}
					if (num5 != -1)
					{
						ShowBitmap.Line(new OpenCvSharp.Point(num5, 0), new OpenCvSharp.Point(num5, ShowBitmap.Rows), Scalar.Yellow);
					}
					if (num6 != -1)
					{
						ShowBitmap.Line(new OpenCvSharp.Point(num6, 0), new OpenCvSharp.Point(num6, ShowBitmap.Rows), Scalar.Yellow);
					}
					if (num7 >= 0 && num8 >= 0 && num5 >= 0 && num6 >= 0)
					{
						int num9 = num6 - num5;
						int num10 = num8 - num7;
						double num11 = (double)num9 * inspection.paramFile.PixelPerUM_X;
						double num12 = (double)num10 * inspection.paramFile.PixelPerUM_Y;
						if (inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].EnableFakeWidthRatio)
						{
							num11 *= inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].WidthRatio;
						}
						if (inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].EnableFakeHeightRatio)
						{
							num12 *= inspection.paramFile.AdvLocateParam[TrackIndex[CurrentFunction]].HeightRatio;
						}
						txt_AllResult.Clear();
						if (LanguageType == 1)
						{
							txt_AllResult.Text = string.Format("[內部定位][{0}] 寬度={1:N3}，高度={2:N3}。 \n", TrackIndex[CurrentFunction] + 1, num11, num12);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text = string.Format("[内部定位][{0}] 宽度={1:N3}，高度={2:N3}。 \n", TrackIndex[CurrentFunction] + 1, num11, num12);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text = string.Format("内部配置[{0}] の幅は {1:N3} ピクセル、高さは {2:N3} ピクセルです。 \n", TrackIndex[CurrentFunction] + 1, num11, num12);
						}
						else
						{
							txt_AllResult.Text = string.Format("[Inside Locate][{0}] Width={1:N3}，Height={2:N3}。 \n", TrackIndex[CurrentFunction] + 1, num11, num12);
						}
						txt_WidthValue.Text = num11.ToString();
						txt_HeightValue.Text = num12.ToString();
						OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(num5, num7, num9, num10);
						double num13 = ConvertComDigits(txt_WidthMin, ForSave: false);
						double num14 = ConvertComDigits(txt_WidthMax, ForSave: false);
						double num15 = ConvertComDigits(txt_HeightMin, ForSave: false);
						double num16 = ConvertComDigits(txt_HeightMax, ForSave: false);
						if (num11 < num13 || num11 > num14 || num12 < num15 || num12 > num16 || num11 < 0.0 || num12 < 0.0)
						{
							if (num9 > 0 && num10 > 0)
							{
								ShowBitmap.Rectangle(rect2, Scalar.Fuchsia);
							}
							if (num11 < num13)
							{
								txt_WidthMin.Background = Brushes.Fuchsia;
							}
							if (num11 > num14)
							{
								txt_WidthMax.Background = Brushes.Fuchsia;
							}
							if (num12 < num15)
							{
								txt_HeightMin.Background = Brushes.Fuchsia;
							}
							if (num12 > num16)
							{
								txt_HeightMax.Background = Brushes.Fuchsia;
							}
							SetResult(result: false);
							inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
							UpdateToolsState(CurrentFunction);
							ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Red;
						}
						else
						{
							if (num9 > 0 && num10 > 0)
							{
								ShowBitmap.Rectangle(rect2, Scalar.Yellow);
							}
							SetResult(result: true);
							inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_OK;
							UpdateToolsState(CurrentFunction);
							ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
						}
					}
					else
					{
						txt_AllResult.Clear();
						if (LanguageType == 1)
						{
							if (num7 < 0)
							{
								txt_AllResult.Text = string.Format("[內部定位][{0}] 上邊錯誤。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num8 < 0)
							{
								txt_AllResult.Text = string.Format("[內部定位][{0}] 下邊錯誤。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num5 < 0)
							{
								txt_AllResult.Text = string.Format("[內部定位][{0}] 左邊錯誤。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num6 < 0)
							{
								txt_AllResult.Text = string.Format("[內部定位][{0}] 右邊錯誤。 \n", TrackIndex[CurrentFunction] + 1);
							}
						}
						else if (LanguageType == 2)
						{
							if (num7 < 0)
							{
								txt_AllResult.Text = string.Format("[内部定位][{0}] 上边错误。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num8 < 0)
							{
								txt_AllResult.Text = string.Format("[内部定位][{0}] 下边错误。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num5 < 0)
							{
								txt_AllResult.Text = string.Format("[内部定位][{0}] 左边错误。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num6 < 0)
							{
								txt_AllResult.Text = string.Format("[内部定位][{0}] 右边错误。 \n", TrackIndex[CurrentFunction] + 1);
							}
						}
						else if (LanguageType == 3)
						{
							if (num7 < 0)
							{
								txt_AllResult.Text = string.Format("内部配置 [{0}] の上部にエラーがあります。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num8 < 0)
							{
								txt_AllResult.Text = string.Format("内部配置 [{0}] の下部にエラーがあります。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num5 < 0)
							{
								txt_AllResult.Text = string.Format("内部配置 [{0}] の左部にエラーがあります。 \n", TrackIndex[CurrentFunction] + 1);
							}
							else if (num6 < 0)
							{
								txt_AllResult.Text = string.Format("内部配置 [{0}] の右部にエラーがあります。 \n", TrackIndex[CurrentFunction] + 1);
							}
						}
						else if (num7 < 0)
						{
							txt_AllResult.Text = string.Format("[Inside Locate][{0}] Found Top Side value error。 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (num8 < 0)
						{
							txt_AllResult.Text = string.Format("[Inside Locate][{0}] Found Bottom Side value error。 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (num5 < 0)
						{
							txt_AllResult.Text = string.Format("[Inside Locate][{0}] Found Left Side value error。 \n", TrackIndex[CurrentFunction] + 1);
						}
						else if (num6 < 0)
						{
							txt_AllResult.Text = string.Format("[Inside Locate][{0}] Found Right Side value error。 \n", TrackIndex[CurrentFunction] + 1);
						}
						inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
						UpdateToolsState(CurrentFunction);
						SetResult(result: false);
					}
				}
				else
				{
					DoThdImage();
					TOOL_RESULT iresult3 = TOOL_RESULT.EM_OK;
					TimeStart();
					inspection.advLocateTool.Inspect(cb_TrackerSelect_A.SelectedIndex, ref iresult3, SelectFace[2]);
					double num17 = TimeStop();
					lab_Time3.Content = string.Format("{0:N3}", num17);
					double advAWidth = inspection.advLocateTool.AdvAWidth;
					double advAHeight = inspection.advLocateTool.AdvAHeight;
					txt_AllResult.Clear();
					if (LanguageType == 1)
					{
						txt_AllResult.Text = string.Format("[內部定位][{0}] 寬度={1:N3}，高度={2:N3}。 \n", TrackIndex[CurrentFunction] + 1, advAWidth, advAHeight);
					}
					else if (LanguageType == 2)
					{
						txt_AllResult.Text = string.Format("[内部定位][{0}] 宽度={1:N3}，高度={2:N3}。 \n", TrackIndex[CurrentFunction] + 1, advAWidth, advAHeight);
					}
					else if (LanguageType == 3)
					{
						txt_AllResult.Text = string.Format("内部配置 [{0}] の幅は {1:N3} ピクセル、高さは {2:N3} ピクセルです。 \n", TrackIndex[CurrentFunction] + 1, advAWidth, advAHeight);
					}
					else
					{
						txt_AllResult.Text = string.Format("[Inside Locate][{0}] Width={1:N3}，Height={2:N3}。 \n", TrackIndex[CurrentFunction] + 1, advAWidth, advAHeight);
					}
					txt_WidthValue.Text = advAWidth.ToString();
					txt_HeightValue.Text = advAHeight.ToString();
					OpenCvSharp.Rect blobRect = BlobRect;
					double num18 = ConvertComDigits(txt_WidthMin, ForSave: false);
					double num19 = ConvertComDigits(txt_WidthMax, ForSave: false);
					double num20 = ConvertComDigits(txt_HeightMin, ForSave: false);
					double num21 = ConvertComDigits(txt_HeightMax, ForSave: false);
					if (advAWidth < num18 || advAWidth > num19 || advAHeight < num20 || advAHeight > num21 || advAWidth < 0.0 || advAHeight < 0.0)
					{
						if (advAWidth > 0.0 && advAHeight > 0.0)
						{
							ShowBitmap.Rectangle(blobRect, Scalar.Fuchsia);
						}
						if (advAWidth < num18)
						{
							txt_WidthMin.Background = Brushes.Fuchsia;
						}
						if (advAWidth > num19)
						{
							txt_WidthMax.Background = Brushes.Fuchsia;
						}
						if (advAHeight < num20)
						{
							txt_HeightMin.Background = Brushes.Fuchsia;
						}
						if (advAHeight > num21)
						{
							txt_HeightMax.Background = Brushes.Fuchsia;
						}
						SetResult(result: false);
						inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_NG;
						UpdateToolsState(CurrentFunction);
					}
					else
					{
						if (advAWidth > 0.0 && advAHeight > 0.0)
						{
							ShowBitmap.Rectangle(blobRect, Scalar.Yellow);
						}
						SetResult(result: true);
						inspection.iResult.AdvLocateResult[TrackIndex[CurrentFunction]] = TOOL_RESULT.EM_OK;
						UpdateToolsState(CurrentFunction);
					}
				}
			}
			else
			{
				TOOL_RESULT iresult4 = TOOL_RESULT.EM_OK;
				inspection.advLocateTool.ErrorMsg = "";
				TimeStart();
				inspection.advLocateTool.doAdvMeasure(cb_TrackerSelect_M.SelectedIndex, ref iresult4, FaceIndex);
				double num22 = TimeStop();
				num22 -= inspection.advLocateTool.OfflineTime;
				lab_Time4.Content = string.Format("{0:N3}", num22);
				switch (iresult4)
				{
				case TOOL_RESULT.EM_OK:
					ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Lime;
					break;
				case TOOL_RESULT.EM_NOT_USE:
					ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.White;
					break;
				default:
					ToolsButton[TrackIndex[CurrentFunction]].Background = Brushes.Red;
					break;
				}
				double num23 = ConvertComDigits(txt_ValueMin, ForSave: false);
				double num24 = ConvertComDigits(txt_ValueMax, ForSave: false);
				txt_Distance.Text = inspection.advLocateTool.MeasureDistance.ToString();
				Scalar color = Scalar.LimeGreen;
				txt_AllResult.Clear();
				if (inspection.advLocateTool.MeasureDistance < num23)
				{
					txt_ValueMin.Background = Brushes.Fuchsia;
					rt_ResultM.Fill = Brushes.Red;
					SetResult(result: false);
					color = Scalar.Red;
				}
				else if (inspection.advLocateTool.MeasureDistance > num24)
				{
					txt_ValueMax.Background = Brushes.Fuchsia;
					rt_ResultM.Fill = Brushes.Red;
					SetResult(result: false);
					color = Scalar.Red;
				}
				else
				{
					rt_ResultM.Fill = Brushes.Lime;
					SetResult(result: true);
				}
				txt_AllResult.Text = inspection.advLocateTool.ErrorMsg;
				if (inspection.buffers.LocatedSrc.Channels() == 3)
				{
					ShowBitmap = MM.CloneFrom(inspection.buffers.LocatedSrc, ShowBitmap);
				}
				else
				{
					Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
				}
				bor_PtRect1.Visibility = Visibility.Hidden;
				bor_PtRect2.Visibility = Visibility.Hidden;
				if (rad_MeasreStartA.IsChecked == true && cb_MeasureStartAIndex.SelectedIndex > 0)
				{
					OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[cb_MeasureStartAIndex.SelectedIndex].InsideRect;
					SetReferenceRect(bor_Rect1, insideRect);
				}
				if (rad_MeasreEndA.IsChecked == true && cb_MeasureEndAIndex.SelectedIndex > 0)
				{
					OpenCvSharp.Rect insideRect2 = inspection.paramFile.AdvLocateParam[cb_MeasureEndAIndex.SelectedIndex].InsideRect;
					SetReferenceRect(bor_Rect2, insideRect2);
				}
				if (rad_MeasureW.IsChecked == true)
				{
					if (inspection.advLocateTool.Line1 >= 0 && inspection.advLocateTool.Line2 >= 0)
					{
						OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(inspection.advLocateTool.NGPt1.X - 3, inspection.advLocateTool.NGPt1.Y - 3, 6, 6);
						OpenCvSharp.Rect rect4 = new OpenCvSharp.Rect(inspection.advLocateTool.NGPt2.X - 3, inspection.advLocateTool.NGPt2.Y - 3, 6, 6);
						ShowBitmap.Rectangle(rect3, Scalar.Gold, -1);
						ShowBitmap.Rectangle(rect4, Scalar.Gold, -1);
						ShowBitmap.Rectangle(rect3, Scalar.Black);
						ShowBitmap.Rectangle(rect4, Scalar.Black);
						ShowBitmap.Line(inspection.advLocateTool.NGPt1, inspection.advLocateTool.NGPt2, color);
						SetWidthLine(line_1, inspection.advLocateTool.Line1);
						SetWidthLine(line_2, inspection.advLocateTool.Line2);
					}
				}
				else if (rad_MeasureH.IsChecked == true)
				{
					if (inspection.advLocateTool.Line1 >= 0 && inspection.advLocateTool.Line2 >= 0)
					{
						OpenCvSharp.Rect rect5 = new OpenCvSharp.Rect(inspection.advLocateTool.NGPt1.X - 3, inspection.advLocateTool.NGPt1.Y - 3, 6, 6);
						OpenCvSharp.Rect rect6 = new OpenCvSharp.Rect(inspection.advLocateTool.NGPt2.X - 3, inspection.advLocateTool.NGPt2.Y - 3, 6, 6);
						ShowBitmap.Rectangle(rect5, Scalar.Gold, -1);
						ShowBitmap.Rectangle(rect6, Scalar.Gold, -1);
						ShowBitmap.Rectangle(rect5, Scalar.Black);
						ShowBitmap.Rectangle(rect6, Scalar.Black);
						ShowBitmap.Line(inspection.advLocateTool.NGPt1, inspection.advLocateTool.NGPt2, color);
						SetHeightLine(line_1, inspection.advLocateTool.Line1);
						SetHeightLine(line_2, inspection.advLocateTool.Line2);
					}
				}
				else
				{
					if (rad_MeasureK.IsChecked == true)
					{
						if (inspection.paramFile.AdvLocateParam[cb_MeasureStartWHIndex.SelectedIndex].UseBinary)
						{
							for (int i = 0; i < inspection.advLocateTool.Line1Pt.Count - 1; i++)
							{
								ShowBitmap.Line(inspection.advLocateTool.Line1Pt[i], inspection.advLocateTool.Line1Pt[i + 1], color);
							}
						}
						else
						{
							ShowBitmap.Rectangle(inspection.paramFile.AdvLocateParam[cb_MeasureStartWHIndex.SelectedIndex].InsideRect, color);
						}
						if (inspection.paramFile.AdvLocateParam[cb_MeasureEndWHIndex.SelectedIndex].UseBinary)
						{
							for (int j = 0; j < inspection.advLocateTool.Line2Pt.Count - 1; j++)
							{
								if (Math.Abs(inspection.advLocateTool.Line2Pt[j].X - inspection.advLocateTool.Line2Pt[j + 1].X) < 100 && Math.Abs(inspection.advLocateTool.Line2Pt[j].Y - inspection.advLocateTool.Line2Pt[j + 1].Y) < 100)
								{
									ShowBitmap.Line(inspection.advLocateTool.Line2Pt[j], inspection.advLocateTool.Line2Pt[j + 1], color);
								}
							}
						}
						else
						{
							ShowBitmap.Rectangle(inspection.paramFile.AdvLocateParam[cb_MeasureEndWHIndex.SelectedIndex].InsideRect, color);
						}
						bor_Rect1.Visibility = Visibility.Hidden;
						bor_Rect2.Visibility = Visibility.Hidden;
					}
					if (rad_MeasureS.IsChecked == false)
					{
						line_1.Visibility = Visibility.Hidden;
						line_2.Visibility = Visibility.Hidden;
						OpenCvSharp.Rect rect7 = new OpenCvSharp.Rect(inspection.advLocateTool.PT1.X - 3, inspection.advLocateTool.PT1.Y - 3, 6, 6);
						OpenCvSharp.Rect rect8 = new OpenCvSharp.Rect(inspection.advLocateTool.PT2.X - 3, inspection.advLocateTool.PT2.Y - 3, 6, 6);
						if (THD_Preview)
						{
							ShowBitmap.Rectangle(rect7, Scalar.Gold, -1);
							ShowBitmap.Rectangle(rect8, Scalar.Gold, -1);
							ShowBitmap.Rectangle(rect7, Scalar.Black);
							ShowBitmap.Rectangle(rect8, Scalar.Black);
						}
						ShowBitmap.Line(inspection.advLocateTool.PT1, inspection.advLocateTool.PT2, Scalar.Fuchsia);
					}
				}
				int selectedIndex = cb_TrackerSelect_M.SelectedIndex;
				UpdateUI = false;
				cb_TrackerSelect_M.Items.Clear();
				for (int k = 0; k < 40; k++)
				{
					cb_TrackerSelect_M.Items.Add(k + 1 + " : " + inspection.paramFile.AdvLocateMParam[k].TrackerName);
				}
				cb_TrackerSelect_M.SelectedIndex = selectedIndex;
				UpdateUI = true;
			}
		}
		catch
		{
			string text = "ALW05-AdvLocateWindow-Btn_Test_Click()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	private void Cb_MeasureStartWHIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && rad_MeasreStartWH.IsChecked != false && cb_MeasureStartWHIndex.SelectedIndex >= 0)
		{
			if (rad_MeasureW.IsChecked == true)
			{
				int value = inspection.paramFile.VSLocateParam[cb_MeasureStartWHIndex.SelectedIndex].Value;
				SetWidthLine(line_1, value);
			}
			else if (rad_MeasureH.IsChecked == true)
			{
				int value2 = inspection.paramFile.HSLocateParam[cb_MeasureStartWHIndex.SelectedIndex].Value;
				SetHeightLine(line_1, value2);
			}
			else if (rad_MeasureK.IsChecked == true)
			{
				UpdateKValueRect(0);
			}
			else
			{
				line_1.Visibility = Visibility.Hidden;
			}
		}
	}

	private void Cb_MeasureEndWHIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && rad_MeasreEndWH.IsChecked != false && cb_MeasureEndWHIndex.SelectedIndex >= 0)
		{
			if (rad_MeasureW.IsChecked == true)
			{
				int value = inspection.paramFile.VSLocateParam[cb_MeasureEndWHIndex.SelectedIndex].Value;
				SetWidthLine(line_2, value);
			}
			else if (rad_MeasureH.IsChecked == true)
			{
				int value2 = inspection.paramFile.HSLocateParam[cb_MeasureEndWHIndex.SelectedIndex].Value;
				SetHeightLine(line_2, value2);
			}
			else if (rad_MeasureK.IsChecked == true)
			{
				UpdateKValueRect(1);
			}
			else
			{
				line_2.Visibility = Visibility.Hidden;
			}
		}
	}

	private void UpdateKValueRect(int index)
	{
		if (index == 0)
		{
			if (cb_MeasureStartWHIndex.SelectedIndex >= 0)
			{
				line_1.Visibility = Visibility.Hidden;
				OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[cb_MeasureStartWHIndex.SelectedIndex].InsideRect;
				bor_PtRect1.Visibility = Visibility.Hidden;
				SetReferenceRect(bor_Rect1, insideRect);
			}
		}
		else if (cb_MeasureEndWHIndex.SelectedIndex >= 0)
		{
			line_2.Visibility = Visibility.Hidden;
			OpenCvSharp.Rect insideRect2 = inspection.paramFile.AdvLocateParam[cb_MeasureEndWHIndex.SelectedIndex].InsideRect;
			bor_PtRect2.Visibility = Visibility.Hidden;
			SetReferenceRect(bor_Rect2, insideRect2);
		}
	}

	private void UpdateMeasurePt1()
	{
		if (cb_MeasureStartAIndex.SelectedIndex >= 0)
		{
			line_1.Visibility = Visibility.Hidden;
			OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[cb_MeasureStartAIndex.SelectedIndex].InsideRect;
			SetReferenceRect(bor_Rect1, insideRect);
			bor_PtRect1.Visibility = Visibility.Visible;
			OpenCvSharp.Point pt = (OpenCvSharp.Point)inspection.advLocateTool.GetAdvLocatePt(cb_MeasureStartAIndex.SelectedIndex, cb_MeasureStartAPt.SelectedIndex);
			SetReferencePtRect(bor_PtRect1, pt);
		}
	}

	private void UpdateMeasurePt2()
	{
		if (cb_MeasureEndAIndex.SelectedIndex >= 0)
		{
			line_2.Visibility = Visibility.Hidden;
			OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[cb_MeasureEndAIndex.SelectedIndex].InsideRect;
			SetReferenceRect(bor_Rect2, insideRect);
			bor_PtRect2.Visibility = Visibility.Visible;
			OpenCvSharp.Point pt = (OpenCvSharp.Point)inspection.advLocateTool.GetAdvLocatePt(cb_MeasureEndAIndex.SelectedIndex, cb_MeasureEndAPt.SelectedIndex);
			SetReferencePtRect(bor_PtRect2, pt);
		}
	}

	private void Cb_MeasureStartAIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited || rad_MeasreStartA.IsChecked == false || cb_MeasureStartAIndex.SelectedIndex < 0 || cb_MeasureStartAPt.SelectedIndex < 0)
		{
			return;
		}
		DrawBackGround();
		if (rad_MeasureW.IsChecked == true)
		{
			int advLocateX = inspection.advLocateTool.GetAdvLocateX(cb_MeasureStartAIndex.SelectedIndex, cb_MeasureStartAPt.SelectedIndex);
			SetWidthLine(line_1, advLocateX);
			bor_PtRect1.Visibility = Visibility.Hidden;
			return;
		}
		if (rad_MeasureH.IsChecked == true)
		{
			int advLocateY = inspection.advLocateTool.GetAdvLocateY(cb_MeasureStartAIndex.SelectedIndex, cb_MeasureStartAPt.SelectedIndex);
			SetHeightLine(line_1, advLocateY);
			bor_PtRect1.Visibility = Visibility.Hidden;
			return;
		}
		UpdateMeasurePt1();
		if (rad_MeasreEndA.IsChecked == true)
		{
			UpdateMeasurePt2();
		}
	}

	private void Cb_MeasureEndAIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited || rad_MeasreEndA.IsChecked == false || cb_MeasureEndAIndex.SelectedIndex < 0 || cb_MeasureEndAPt.SelectedIndex < 0)
		{
			return;
		}
		DrawBackGround();
		if (rad_MeasureW.IsChecked == true)
		{
			int advLocateX = inspection.advLocateTool.GetAdvLocateX(cb_MeasureEndAIndex.SelectedIndex, cb_MeasureEndAPt.SelectedIndex);
			SetWidthLine(line_2, advLocateX);
			bor_PtRect2.Visibility = Visibility.Hidden;
			return;
		}
		if (rad_MeasureH.IsChecked == true)
		{
			int advLocateY = inspection.advLocateTool.GetAdvLocateY(cb_MeasureEndAIndex.SelectedIndex, cb_MeasureEndAPt.SelectedIndex);
			SetHeightLine(line_2, advLocateY);
			bor_PtRect2.Visibility = Visibility.Hidden;
			return;
		}
		UpdateMeasurePt2();
		if (rad_MeasreStartA.IsChecked == true)
		{
			UpdateMeasurePt1();
		}
	}

	private void SetWidthLine(Line x, int value)
	{
		x.Visibility = Visibility.Visible;
		double x2 = (x.X2 = (double)value * xRatio);
		x.X1 = x2;
		x.Y1 = 0.0;
		x.Y2 = (double)ImageHeight * yRatio;
	}

	private void Rad_MeasreStartWH_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			bor_Rect1.Visibility = Visibility.Hidden;
		}
	}

	private void Rad_MeasreEndWH_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			bor_Rect2.Visibility = Visibility.Hidden;
		}
	}

	private void Rad_MeasreStartA_Checked_1(object sender, RoutedEventArgs e)
	{
		if (!inited || cb_MeasureStartAIndex.SelectedIndex < 0 || rad_MeasreStartA.IsChecked == false)
		{
			return;
		}
		DrawBackGround();
		OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[cb_MeasureStartAIndex.SelectedIndex].InsideRect;
		if (insideRect.Width > 0 && insideRect.Height > 0)
		{
			bor_Rect1.Visibility = Visibility.Visible;
			SetReferenceRect(bor_Rect1, insideRect);
		}
		else
		{
			bor_Rect1.Visibility = Visibility.Hidden;
		}
		if (rad_MeasureD.IsChecked == true)
		{
			UpdateMeasurePt1();
			if (rad_MeasreEndA.IsChecked == true)
			{
				UpdateMeasurePt2();
			}
		}
	}

	private void Rad_MeasreEndA_Checked(object sender, RoutedEventArgs e)
	{
		if (!inited || cb_MeasureEndAIndex.SelectedIndex < 0 || rad_MeasreEndA.IsChecked == false)
		{
			return;
		}
		DrawBackGround();
		OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[cb_MeasureEndAIndex.SelectedIndex].InsideRect;
		if (insideRect.Width > 0 && insideRect.Height > 0)
		{
			bor_Rect2.Visibility = Visibility.Visible;
			SetReferenceRect(bor_Rect2, insideRect);
		}
		else
		{
			bor_Rect2.Visibility = Visibility.Hidden;
		}
		if (rad_MeasureD.IsChecked == true)
		{
			UpdateMeasurePt2();
			if (rad_MeasreStartA.IsChecked == true)
			{
				UpdateMeasurePt1();
			}
		}
	}

	private void SetHeightLine(Line y, int value)
	{
		y.Visibility = Visibility.Visible;
		double y2 = (y.Y2 = (double)value * yRatio);
		y.Y1 = y2;
		y.X1 = 0.0;
		y.X2 = (double)ImageWidth * xRatio;
	}

	private void Cb_TrackerSelect_M_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam(CurrentFunction, TrackIndex[CurrentFunction]);
			TrackIndex[CurrentFunction] = cb_TrackerSelect_M.SelectedIndex;
			Label_Index.Content = (cb_TrackerSelect_M.SelectedIndex + 1).ToString();
			CopyValueToUI(CurrentFunction);
			if (ts_TrackerON_M.IsChecked == true)
			{
				Btn_Test_Click(sender, e);
			}
			UpdateToolsState(CurrentFunction);
		}
	}

	private void Rad_MeasureK_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			rad_MeasreStartA.IsChecked = true;
			rad_MeasreEndA.IsChecked = true;
			UpdateAdvMeasureComobox(3);
			chk_MeasureAllX.Visibility = Visibility.Visible;
			chk_MeasureAllY.Visibility = Visibility.Visible;
		}
	}

	private void Btn_Chart_Click(object sender, RoutedEventArgs e)
	{
		if (cb_TrackerSelect_A.SelectedIndex >= 0 && cb_TrackerSelect_A.SelectedIndex < 20)
		{
			int subitem = Convert.ToInt32(((Button)sender).Uid);
			ChartWindow chartWindow = new ChartWindow();
			chartWindow.SetInspection(inspection);
			chartWindow.LoadLanguage(LanguageType);
			chartWindow.SetStatistics(1, subitem, cb_TrackerSelect_A.SelectedIndex);
			chartWindow.ShowDialog();
		}
	}

	private void Btn_ChartM_Click(object sender, RoutedEventArgs e)
	{
		if (cb_TrackerSelect_M.SelectedIndex >= 0 && cb_TrackerSelect_M.SelectedIndex < 40)
		{
			_ = (Button)sender;
			ChartWindow chartWindow = new ChartWindow();
			chartWindow.SetInspection(inspection);
			chartWindow.LoadLanguage(LanguageType);
			chartWindow.SetStatistics(2, 0, cb_TrackerSelect_M.SelectedIndex);
			chartWindow.ShowDialog();
		}
	}

	private void Rad_MeasureS_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			rad_MeasreStartA.IsChecked = true;
			rad_MeasreEndA.IsChecked = true;
			UpdateAdvMeasureComobox(4);
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
		else
		{
			rt.Visibility = Visibility.Hidden;
		}
	}

	private void SetReferencePtRect(Rectangle rt, OpenCvSharp.Point pt)
	{
		rt.Visibility = Visibility.Visible;
		rt.SetValue(Canvas.LeftProperty, (double)(pt.X - 2) * xRatio);
		rt.SetValue(Canvas.TopProperty, (double)(pt.Y - 2) * yRatio);
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
			CVBuffer.SaveLog(string.Concat(string.Format("AdvLocateTool-Type{0}", tab_AdvLocateFun.SelectedIndex) + "LT11-AdvLocate-ConvertComDigits()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
			CVBuffer.SaveLog(string.Concat(string.Format("AdvLocateTool-Type{0}", tab_AdvLocateFun.SelectedIndex) + "LT12-AdvLocate-ConvertComDigits()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
			if (hKeyboard.Top < 0.0)
			{
				hKeyboard.Top = 0.0;
			}
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
			CVBuffer.SaveLog(string.Concat(string.Format("AdvLocateTool-Type{0}", tab_AdvLocateFun.SelectedIndex) + "LT13-AdvLocate-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void WriteModifyLog(int index)
	{
		if (CurrentFunction == 0 || CurrentFunction == 1)
		{
			WriteVHModifyLog(index);
		}
		else if (CurrentFunction == 2)
		{
			WriteAModifyLog(index);
		}
		else
		{
			WriteMModifyLog(index);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteVHModifyLog(int index)
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
			INSP_SCANLINETOOL_PARAM param;
			INSP_SCANLINETOOL_PARAM backupParam;
			string text4;
			if (CurrentFunction == 0)
			{
				param = inspection.paramFile.VSLocateParam[index];
				backupParam = BackupParamV;
				text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["AdvLocateTool1"], index + 1, param.TrackerName);
			}
			else
			{
				param = inspection.paramFile.HSLocateParam[index];
				backupParam = BackupParamH;
				text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["AdvLocateTool2"], index + 1, param.TrackerName);
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
			if (param.THD2_ChannelON != backupParam.THD2_ChannelON)
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
			if (param.THD3_ChannelIndex != backupParam.THD3_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)backupParam.THD3_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD3_ChannelIndex));
			}
			if (param.THD3_Min != backupParam.THD3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], backupParam.THD3_Min, LanguageManager.Instance["ChangedTo"], param.THD3_Min);
			}
			if (param.THD3_Max != backupParam.THD3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], backupParam.THD3_Max, LanguageManager.Instance["ChangedTo"], param.THD3_Max);
			}
			if (param.THD3_ChannelON != backupParam.THD3_ChannelON)
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
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start1_Index, (int)backupParam.DE_Start1_Index, param.DE_Start1_Count, backupParam.DE_Start1_Count, 1, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End1_Index, (int)backupParam.DE_End1_Index, param.DE_End1_Count, backupParam.DE_End1_Count, 1, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start2_Index, (int)backupParam.DE_Start2_Index, param.DE_Start2_Count, backupParam.DE_Start2_Count, 2, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End2_Index, (int)backupParam.DE_End2_Index, param.DE_End2_Count, backupParam.DE_End2_Count, 2, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start3_Index, (int)backupParam.DE_Start3_Index, param.DE_Start3_Count, backupParam.DE_Start3_Count, 3, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End3_Index, (int)backupParam.DE_End3_Index, param.DE_End3_Count, backupParam.DE_End3_Count, 3, 2, text4);
			GetVHTrackModifyText(text4, param, backupParam);
			if (param.Direction != backupParam.Direction)
			{
				string secanDirectionText = modifyLogFun.GetSecanDirectionText((int)param.Direction);
				string secanDirectionText2 = modifyLogFun.GetSecanDirectionText((int)backupParam.Direction);
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["ScanDirection"], secanDirectionText2, LanguageManager.Instance["ChangedTo"], secanDirectionText);
			}
			if (param.UseStdFilter != backupParam.UseStdFilter)
			{
				if (param.UseStdFilter)
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["UseStdFilter"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["UseStdFilter"], LanguageManager.Instance["Close"]);
				}
			}
			if (param.UseAutoSize != backupParam.UseAutoSize)
			{
				if (param.UseAutoSize)
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["TrackPos"], LanguageManager.Instance["TrackerUseFollow"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["TrackerUseAutoSize"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["TrackPos"], LanguageManager.Instance["TrackerUseAutoSize"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["TrackerUseFollow"]);
				}
			}
			if (param.FindEdgeMethod != backupParam.FindEdgeMethod)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectScanRule"], modifyLogFun.GetScanRuleText(backupParam.FindEdgeMethod), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetScanRuleText(param.FindEdgeMethod));
			}
			if (param.ErrorBin != backupParam.ErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)backupParam.ErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)param.ErrorBin));
			}
			if (param.EnableSelectFace != backupParam.EnableSelectFace)
			{
				if (!param.EnableSelectFace)
				{
					ModifyLog += string.Format("{0} {1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
				}
				else
				{
					ModifyLog += string.Format("{0} {1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
				}
			}
			if (param.FaceIndex != backupParam.FaceIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectFaceTool"], modifyLogFun.GetSelectFaceText(backupParam.FaceIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetSelectFaceText(param.FaceIndex));
			}
			if (param.TrackerON != backupParam.TrackerON)
			{
				if (param.TrackerON)
				{
					ModifyLog += string.Format("{0} {1}]\n", text4, LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} {1}]\n", text4, LanguageManager.Instance["Close"]);
				}
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
			string text5 = "BW10-AdvLocateWindow-WriteVHModifyLog()-";
			text5 += ex.ToString();
			CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text5, "Log寫入錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteAModifyLog(int index)
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
			INSP_ADVLOCATETOOL_PARAM param = inspection.paramFile.AdvLocateParam[index];
			INSP_ADVLOCATETOOL_PARAM backupParamA = BackupParamA;
			string text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["AdvLocateTool3"], index + 1, param.TrackerName);
			if (param.TrackerON != backupParamA.TrackerON)
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
			if (param.THD1_ChannelIndex != backupParamA.THD1_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)backupParamA.THD1_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD1_ChannelIndex));
			}
			if (param.THD1_Min != backupParamA.THD1_Min)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], backupParamA.THD1_Min, LanguageManager.Instance["ChangedTo"], param.THD1_Min);
			}
			if (param.THD1_Max != backupParamA.THD1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], backupParamA.THD1_Max, LanguageManager.Instance["ChangedTo"], param.THD1_Max);
			}
			if (param.THD2_ChannelIndex != backupParamA.THD2_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)backupParamA.THD2_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD2_ChannelIndex));
			}
			if (param.THD2_Min != backupParamA.THD2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], backupParamA.THD2_Min, LanguageManager.Instance["ChangedTo"], param.THD2_Min);
			}
			if (param.THD2_Max != backupParamA.THD2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], backupParamA.THD2_Max, LanguageManager.Instance["ChangedTo"], param.THD2_Max);
			}
			if (param.THD2_ChannelON != backupParamA.THD2_ChannelON)
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
			if (param.THD3_ChannelIndex != backupParamA.THD3_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)backupParamA.THD3_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.THD3_ChannelIndex));
			}
			if (param.THD3_Min != backupParamA.THD3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], backupParamA.THD3_Min, LanguageManager.Instance["ChangedTo"], param.THD3_Min);
			}
			if (param.THD3_Max != backupParamA.THD3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], backupParamA.THD3_Max, LanguageManager.Instance["ChangedTo"], param.THD3_Max);
			}
			if (param.THD3_ChannelON != backupParamA.THD3_ChannelON)
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
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start1_Index, (int)backupParamA.DE_Start1_Index, param.DE_Start1_Count, backupParamA.DE_Start1_Count, 1, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End1_Index, (int)backupParamA.DE_End1_Index, param.DE_End1_Count, backupParamA.DE_End1_Count, 1, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start2_Index, (int)backupParamA.DE_Start2_Index, param.DE_Start2_Count, backupParamA.DE_Start2_Count, 2, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End2_Index, (int)backupParamA.DE_End2_Index, param.DE_End2_Count, backupParamA.DE_End2_Count, 2, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start3_Index, (int)backupParamA.DE_Start3_Index, param.DE_Start3_Count, backupParamA.DE_Start3_Count, 3, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End3_Index, (int)backupParamA.DE_End3_Index, param.DE_End3_Count, backupParamA.DE_End3_Count, 3, 2, text4);
			GetATrackModifyText(text4, param, backupParamA);
			if (param.UseBinary != backupParamA.UseBinary)
			{
				if (param.UseBinary)
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["AdvLocateMode"], LanguageManager.Instance["UseAdvLocate"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["UseBinaryLocate"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["AdvLocateMode"], LanguageManager.Instance["UseBinaryLocate"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["UseAdvLocate"]);
				}
			}
			if (param.UseAutoSize != backupParamA.UseAutoSize)
			{
				if (param.UseAutoSize)
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["TrackPos"], LanguageManager.Instance["TrackerUseFollow"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["TrackerUseAutoSize"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["TrackPos"], LanguageManager.Instance["TrackerUseAutoSize"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["TrackerUseFollow"]);
				}
			}
			if (param.RectIndexT != backupParamA.RectIndexT && param.RectIndexT >= 0 && backupParamA.RectIndexT >= 0)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Adv.Inside"] + LanguageManager.Instance["Top2"], cb_RectIndexT.Items[backupParamA.RectIndexT].ToString(), LanguageManager.Instance["ChangedTo"], cb_RectIndexT.Items[param.RectIndexT].ToString());
			}
			if (param.RectIndexB != backupParamA.RectIndexB && param.RectIndexB >= 0 && backupParamA.RectIndexB >= 0)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Adv.Inside"] + LanguageManager.Instance["Bottom2"], cb_RectIndexB.Items[backupParamA.RectIndexB].ToString(), LanguageManager.Instance["ChangedTo"], cb_RectIndexB.Items[param.RectIndexB].ToString());
			}
			if (param.RectIndexL != backupParamA.RectIndexL && param.RectIndexL >= 0 && backupParamA.RectIndexL >= 0)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Adv.Inside"] + LanguageManager.Instance["Left2"], cb_RectIndexL.Items[backupParamA.RectIndexL].ToString(), LanguageManager.Instance["ChangedTo"], cb_RectIndexL.Items[param.RectIndexL].ToString());
			}
			if (param.RectIndexR != backupParamA.RectIndexR && param.RectIndexR >= 0 && backupParamA.RectIndexR >= 0)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Adv.Inside"] + LanguageManager.Instance["Right2"], cb_RectIndexR.Items[backupParamA.RectIndexR].ToString(), LanguageManager.Instance["ChangedTo"], cb_RectIndexR.Items[param.RectIndexR].ToString());
			}
			if (param.WidthMin != backupParamA.WidthMin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Width3"] + LanguageManager.Instance["Min2"], GetComDigitsStr(backupParamA.WidthMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.WidthMin));
			}
			if (param.WidthMax != backupParamA.WidthMax)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Width3"] + LanguageManager.Instance["Max2"], GetComDigitsStr(backupParamA.WidthMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.WidthMax));
			}
			if (param.HeightMin != backupParamA.HeightMin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Height3"] + LanguageManager.Instance["Min2"], GetComDigitsStr(backupParamA.HeightMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.HeightMin));
			}
			if (param.HeightMax != backupParamA.HeightMax)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Height3"] + LanguageManager.Instance["Max2"], GetComDigitsStr(backupParamA.HeightMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(param.HeightMax));
			}
			if (param.ErrorBin != backupParamA.ErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)backupParamA.ErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)param.ErrorBin));
			}
			if (param.EnableSelectFace != backupParamA.EnableSelectFace)
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
			if (param.FaceIndex != backupParamA.FaceIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectFaceTool"], modifyLogFun.GetSelectFaceText(backupParamA.FaceIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetSelectFaceText(param.FaceIndex));
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
			string text5 = "BW10-AdvLocateWindow-WriteAModifyLog()-";
			text5 += ex.ToString();
			CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text5, "Log寫入錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteMModifyLog(int index)
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
			INSP_ADVMEASURETOOL_PARAM iNSP_ADVMEASURETOOL_PARAM = inspection.paramFile.AdvLocateMParam[index];
			INSP_ADVMEASURETOOL_PARAM backupParamM = BackupParamM;
			string text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["Adv.Measure"], index + 1, iNSP_ADVMEASURETOOL_PARAM.TrackerName);
			string measureFunctionText = modifyLogFun.GetMeasureFunctionText(iNSP_ADVMEASURETOOL_PARAM.MeasureFunction);
			string measureFunctionText2 = modifyLogFun.GetMeasureFunctionText(backupParamM.MeasureFunction);
			if (iNSP_ADVMEASURETOOL_PARAM.MeasureFunction != backupParamM.MeasureFunction)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["MeasureObj"], measureFunctionText2, LanguageManager.Instance["ChangedTo"], measureFunctionText);
			}
			if (iNSP_ADVMEASURETOOL_PARAM.MStartSelect != backupParamM.MStartSelect || iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex != backupParamM.MStartWHIndex || iNSP_ADVMEASURETOOL_PARAM.MStartAIndex != backupParamM.MStartAIndex || iNSP_ADVMEASURETOOL_PARAM.MStartAPt != backupParamM.MStartAPt)
			{
				if (iNSP_ADVMEASURETOOL_PARAM.MStartSelect == 0)
				{
					if (backupParamM.MStartWHIndex < 0 || backupParamM.MStartWHIndex >= cb_MeasureStartWHIndex.Items.Count)
					{
						backupParamM.MStartWHIndex = 0;
					}
					if (iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex < 0 || iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex >= cb_MeasureStartWHIndex.Items.Count)
					{
						iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex = 0;
					}
					ModifyLog += string.Format("{0} [({1}-1) {2} {3} {4}]\n", text4, measureFunctionText, cb_MeasureStartWHIndex.Items[backupParamM.MStartWHIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_MeasureStartWHIndex.Items[iNSP_ADVMEASURETOOL_PARAM.MStartWHIndex].ToString());
				}
				else
				{
					if (backupParamM.MStartAIndex < 0 || backupParamM.MStartAIndex >= cb_MeasureStartAIndex.Items.Count)
					{
						backupParamM.MStartAIndex = 0;
					}
					if (iNSP_ADVMEASURETOOL_PARAM.MStartAIndex < 0 || iNSP_ADVMEASURETOOL_PARAM.MStartAIndex >= cb_MeasureStartAIndex.Items.Count)
					{
						iNSP_ADVMEASURETOOL_PARAM.MStartAIndex = 0;
					}
					ModifyLog += string.Format("{0} [({1}-1) {2}-{3} {4} {5}-{6}]\n", text4, measureFunctionText, cb_MeasureStartAIndex.Items[backupParamM.MStartAIndex].ToString(), modifyLogFun.GetInsideLocatePtText(backupParamM.MStartAPt), LanguageManager.Instance["ChangedTo"], cb_MeasureStartAIndex.Items[iNSP_ADVMEASURETOOL_PARAM.MStartAIndex].ToString(), modifyLogFun.GetInsideLocatePtText(iNSP_ADVMEASURETOOL_PARAM.MStartAPt));
				}
			}
			if (iNSP_ADVMEASURETOOL_PARAM.MEndSelect != backupParamM.MEndSelect || iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex != backupParamM.MEndWHIndex || iNSP_ADVMEASURETOOL_PARAM.MEndAIndex != backupParamM.MEndAIndex || iNSP_ADVMEASURETOOL_PARAM.MEndAPt != backupParamM.MEndAPt)
			{
				if (iNSP_ADVMEASURETOOL_PARAM.MEndSelect == 0)
				{
					if (backupParamM.MEndWHIndex < 0 || backupParamM.MEndWHIndex >= cb_MeasureEndWHIndex.Items.Count)
					{
						backupParamM.MEndWHIndex = 0;
					}
					if (iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex < 0 || iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex >= cb_MeasureEndWHIndex.Items.Count)
					{
						iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex = 0;
					}
					ModifyLog += string.Format("{0} [({1}-2) {2} {3} {4}]\n", text4, measureFunctionText, cb_MeasureEndWHIndex.Items[backupParamM.MEndWHIndex].ToString(), LanguageManager.Instance["ChangedTo"], cb_MeasureEndWHIndex.Items[iNSP_ADVMEASURETOOL_PARAM.MEndWHIndex].ToString());
				}
				else
				{
					if (backupParamM.MEndAIndex < 0 || backupParamM.MEndAIndex >= cb_MeasureEndAIndex.Items.Count)
					{
						backupParamM.MEndAIndex = 0;
					}
					if (iNSP_ADVMEASURETOOL_PARAM.MEndAIndex < 0 || iNSP_ADVMEASURETOOL_PARAM.MEndAIndex >= cb_MeasureEndAIndex.Items.Count)
					{
						iNSP_ADVMEASURETOOL_PARAM.MEndAIndex = 0;
					}
					ModifyLog += string.Format("{0} [({1}-2) {2}-{3} {4} {5}-{6}]\n", text4, measureFunctionText, cb_MeasureEndAIndex.Items[backupParamM.MEndAIndex].ToString(), modifyLogFun.GetInsideLocatePtText(backupParamM.MEndAPt), LanguageManager.Instance["ChangedTo"], cb_MeasureEndAIndex.Items[iNSP_ADVMEASURETOOL_PARAM.MEndAIndex].ToString(), modifyLogFun.GetInsideLocatePtText(iNSP_ADVMEASURETOOL_PARAM.MEndAPt));
				}
			}
			if (iNSP_ADVMEASURETOOL_PARAM.ValueMin != backupParamM.ValueMin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, measureFunctionText + LanguageManager.Instance["Min2"], GetComDigitsStr(backupParamM.ValueMin), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_ADVMEASURETOOL_PARAM.ValueMin));
			}
			if (iNSP_ADVMEASURETOOL_PARAM.ValueMax != backupParamM.ValueMax)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, measureFunctionText + LanguageManager.Instance["Max2"], GetComDigitsStr(backupParamM.ValueMax), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_ADVMEASURETOOL_PARAM.ValueMax));
			}
			if (iNSP_ADVMEASURETOOL_PARAM.ErrorBin != backupParamM.ErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)backupParamM.ErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)iNSP_ADVMEASURETOOL_PARAM.ErrorBin));
			}
			if (iNSP_ADVMEASURETOOL_PARAM.EnableSelectFace != backupParamM.EnableSelectFace)
			{
				if (!iNSP_ADVMEASURETOOL_PARAM.EnableSelectFace)
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
				}
			}
			if (iNSP_ADVMEASURETOOL_PARAM.FaceIndex != backupParamM.FaceIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectFaceTool"], modifyLogFun.GetSelectFaceText(backupParamM.FaceIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetSelectFaceText(iNSP_ADVMEASURETOOL_PARAM.FaceIndex));
			}
			if (iNSP_ADVMEASURETOOL_PARAM.TrackerON != backupParamM.TrackerON)
			{
				if (iNSP_ADVMEASURETOOL_PARAM.TrackerON)
				{
					ModifyLog += string.Format("{0} [{1}]\n", text4, LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1}]\n", text4, LanguageManager.Instance["Close"]);
				}
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
			string text5 = "BW10-AdvLocateWindow-WriteAModifyLog()-";
			text5 += ex.ToString();
			CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text5, "Log寫入錯誤");
			}
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
		CopyColorParam.THD1_Min = (int)rs_THD1.LowerValue;
		CopyColorParam.THD1_Max = (int)rs_THD1.UpperValue;
		CopyColorParam.THD2_Min = (int)rs_THD2.LowerValue;
		CopyColorParam.THD2_Max = (int)rs_THD2.UpperValue;
		CopyColorParam.THD3_Min = (int)rs_THD3.LowerValue;
		CopyColorParam.THD3_Max = (int)rs_THD3.UpperValue;
		CopyColorParam.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
		CopyColorParam.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
		CopyColorParam.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
		CopyColorParam.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
		CopyColorParam.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
		CopyColorParam.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
		CopyColorParam.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
		CopyColorParam.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ColorPaste_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("目前二值化數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("目前二值化数值将被覆盖！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Binary Threshold value will Overwrite！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在の二値化値が上書きされます！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			cb_Channel_THD1.SelectedIndex = (int)CopyColorParam.THD1_ChannelIndex;
			cb_Channel_THD2.SelectedIndex = (int)CopyColorParam.THD2_ChannelIndex;
			cb_Channel_THD3.SelectedIndex = (int)CopyColorParam.THD3_ChannelIndex;
			exp_THD1.IsExpanded = true;
			exp_THD2.IsExpanded = CopyColorParam.THD2_ChannelON;
			exp_THD3.IsExpanded = CopyColorParam.THD3_ChannelON;
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
			cb_THD1_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)CopyColorParam.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)CopyColorParam.DE_End3_Index;
			cb_THD1_DEStartCount.SelectedIndex = CopyColorParam.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = CopyColorParam.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = CopyColorParam.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = CopyColorParam.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = CopyColorParam.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = CopyColorParam.DE_End3_Count;
		}
	}

	private void Btn_THD1Undo_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		switch (num)
		{
		case 0:
			THD_Sliders[num].LowerValue = ThdLow[0];
			THD_Sliders[num].UpperValue = ThdHi[0];
			break;
		case 1:
			THD_Sliders[num].LowerValue = ThdLow[1];
			THD_Sliders[num].UpperValue = ThdHi[1];
			break;
		case 2:
			THD_Sliders[num].LowerValue = ThdLow[2];
			THD_Sliders[num].UpperValue = ThdHi[2];
			break;
		}
		Btn_Test_Click(this, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_MStartEdit_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (MeasureFunction >= 0 && MeasureFunction < 4 && cb_MeasureStartWHIndex.SelectedIndex >= 0)
			{
				tab_AdvLocateFun.SelectedIndex = MeasureFunction;
				TrackIndex[MeasureFunction] = cb_MeasureStartWHIndex.SelectedIndex;
				CopyValueToUI(MeasureFunction);
			}
		}
		catch (Exception ex)
		{
			string text = "BW10-AdvLocateWindow-Btn_MStartEdit_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "Btn_MStartEdit_Click");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_MEndEdit_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (MeasureFunction >= 0 && MeasureFunction < 4 && cb_MeasureEndWHIndex.SelectedIndex >= 0)
			{
				tab_AdvLocateFun.SelectedIndex = MeasureFunction;
				TrackIndex[MeasureFunction] = cb_MeasureEndWHIndex.SelectedIndex;
				CopyValueToUI(MeasureFunction);
			}
		}
		catch (Exception ex)
		{
			string text = "BW10-AdvLocateWindow-Btn_MEndEdit_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "Btn_MEndEdit_Click");
			}
		}
	}

	private void Rad_Direction_H3_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			cb_FindEdgeMethod_H.SelectedIndex = 2;
		}
	}

	private void Rad_Direction_H4_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			cb_FindEdgeMethod_H.SelectedIndex = 1;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OTSUL_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = cb_Channel_THD1.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect);
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
			OTSU_White = !OTSU_White;
			DoThdImage();
		}
		catch (Exception ex)
		{
			string text = "BW10-AdvLocateWindow-Btn_OTSUL_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "Error");
			}
		}
	}

	private void Cnv_Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Btn_Test1__A_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Right)
		{
			chk_EnableWidthRatio.Visibility = Visibility.Visible;
			chk_EnableHeightRatio.Visibility = Visibility.Visible;
			txt_HeightRatio.Visibility = Visibility.Visible;
			txt_WidthRatio.Visibility = Visibility.Visible;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetVHTrackModifyText(string Title, INSP_SCANLINETOOL_PARAM param, INSP_SCANLINETOOL_PARAM BackupParam)
	{
		if (param.SearchRectL != BackupParam.SearchRectL || param.SearchRectR != BackupParam.SearchRectR || param.SearchRectT != BackupParam.SearchRectT || param.SearchRectB != BackupParam.SearchRectB)
		{
			ModifyLog += string.Format("{0} [({1}) L:{2} R:{3} T:{4} B:{5} {6} L:{7} R:{8} T:{9} B:{10}] \n", Title, LanguageManager.Instance["TrackPos"], BackupParam.SearchRectL, BackupParam.SearchRectR, BackupParam.SearchRectT, BackupParam.SearchRectB, LanguageManager.Instance["ChangedTo"], param.SearchRectL, param.SearchRectR, param.SearchRectT, param.SearchRectB);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetATrackModifyText(string Title, INSP_ADVLOCATETOOL_PARAM param, INSP_ADVLOCATETOOL_PARAM BackupParam)
	{
		if (param.UseAutoSize != BackupParam.UseAutoSize)
		{
			if (param.UseAutoSize)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["AutoFit"]);
			}
			else
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["AutoFit"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Alignment"]);
			}
		}
		int num;
		int num2;
		int num3;
		int num4;
		if (param.UseAutoSize)
		{
			num = param.SearchRectW;
			num2 = param.SearchRectH;
			num3 = BackupParam.SearchRectW;
			num4 = BackupParam.SearchRectH;
		}
		else
		{
			num = param.SearchRectWidth;
			num2 = param.SearchRectHeight;
			num3 = BackupParam.SearchRectWidth;
			num4 = BackupParam.SearchRectHeight;
		}
		if (param.SearchRectL != BackupParam.SearchRectL || num != num3 || param.SearchRectT != BackupParam.SearchRectT || num2 != num4)
		{
			ModifyLog += string.Format("{0} [({1}) L:{2} Width:{3} T:{4} Height:{5} {6} Width:{7} R:{8} T:{9} Height:{10}] \n", Title, LanguageManager.Instance["TrackPos"], BackupParam.SearchRectL, num3, BackupParam.SearchRectT, num4, LanguageManager.Instance["ChangedTo"], param.SearchRectL, num, param.SearchRectT, num2);
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/advlocatewindow.xaml", UriKind.Relative);
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
			MainW = (AdvLocateWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			break;
		case 2:
			((Grid)target).MouseWheel += Grid_MouseWheel;
			break;
		case 3:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown;
			cnv_Image.PreviewMouseRightButtonDown += Cnv_Image_PreviewMouseRightButtonDown;
			cnv_Image.MouseLeftButtonUp += Cnv_Image_MouseLeftButtonUp;
			break;
		case 4:
			sfr = (ScaleTransform)target;
			break;
		case 5:
			line_1 = (Line)target;
			break;
		case 6:
			line_2 = (Line)target;
			break;
		case 7:
			Label_Result = (Label)target;
			break;
		case 8:
			bor_LocateRect = (Border)target;
			break;
		case 9:
			bor_SearchRect1 = (Border)target;
			break;
		case 10:
			txt_Search1Label = (TextBlock)target;
			break;
		case 11:
			bor_Rect1 = (Rectangle)target;
			break;
		case 12:
			bor_Rect2 = (Rectangle)target;
			break;
		case 13:
			bor_PtRect1 = (Rectangle)target;
			break;
		case 14:
			bor_PtRect2 = (Rectangle)target;
			break;
		case 15:
			Label_ResultError = (TextBox)target;
			break;
		case 16:
			txt_PreviewMode = (TextBox)target;
			break;
		case 17:
			tab_AdvLocateFun = (TabControl)target;
			tab_AdvLocateFun.SelectionChanged += Tab_AdvLocateFun_SelectionChanged;
			break;
		case 18:
			tab_V = (TabItem)target;
			break;
		case 19:
			cb_TrackerSelect_V = (ComboBox)target;
			cb_TrackerSelect_V.SelectionChanged += Cb_TrackerSelect_V_SelectionChanged;
			break;
		case 20:
			txt_TrackerName_V = (TextBox)target;
			break;
		case 21:
			scr_TrackerSwich_V = (ScrollBar)target;
			scr_TrackerSwich_V.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 22:
			ts_TrackerON_V = (ToggleSwitch)target;
			ts_TrackerON_V.Click += Ts_TrackerON_Click;
			ts_TrackerON_V.Unchecked += Ts_TrackerON_V_Unchecked;
			break;
		case 23:
			btn_ReverseV = (Button)target;
			btn_ReverseV.Click += Btn_Reverse_Click;
			break;
		case 24:
			btn_RecipeCopyV = (Button)target;
			btn_RecipeCopyV.Click += Btn_RecipeCopy_Click;
			break;
		case 25:
			btn_RecipePasteV = (Button)target;
			btn_RecipePasteV.Click += Btn_RecipePaste_Click;
			break;
		case 26:
			grd_ScanV_Block = (Grid)target;
			break;
		case 27:
			cb_ErrorBin_V = (ComboBox)target;
			break;
		case 28:
			btn_Test_V = (Button)target;
			btn_Test_V.Click += Btn_Test_Click;
			break;
		case 29:
			rad_Direction_V = (RadioButton)target;
			break;
		case 30:
			rad_Direction_V2 = (RadioButton)target;
			break;
		case 31:
			btn_ScanL2R = (Button)target;
			btn_ScanL2R.Click += Btn_ScanL2R_Click;
			break;
		case 32:
			btn_ScanR2L = (Button)target;
			btn_ScanR2L.Click += Btn_ScanR2L_Click;
			break;
		case 33:
			rad_L2R1 = (RadioButton)target;
			break;
		case 34:
			rad_R2L1 = (RadioButton)target;
			break;
		case 35:
			cb_FindEdgeMethod_V = (ComboBox)target;
			break;
		case 36:
			btn_TrackerReset_Copy = (Button)target;
			btn_TrackerReset_Copy.Click += Btn_TrackerReset_Click;
			break;
		case 37:
			rt_ResultV = (Rectangle)target;
			break;
		case 38:
			chk_UseStdFilterV = (CheckBox)target;
			break;
		case 39:
			grd_VSelectFace = (Grid)target;
			break;
		case 40:
			btn_VFaceA = (Button)target;
			btn_VFaceA.Click += Btn_VFaceA_Click;
			break;
		case 41:
			btn_VFaceB = (Button)target;
			btn_VFaceB.Click += Btn_VFaceA_Click;
			break;
		case 42:
			btn_VFaceC = (Button)target;
			btn_VFaceC.Click += Btn_VFaceA_Click;
			break;
		case 43:
			btn_VFaceD = (Button)target;
			btn_VFaceD.Click += Btn_VFaceA_Click;
			break;
		case 44:
			chk_VSelectFace = (CheckBox)target;
			chk_VSelectFace.Click += Chk_VSelectFace_Click;
			break;
		case 45:
			chk_UseColorInverseV = (CheckBox)target;
			break;
		case 46:
			btn_PrevTracker = (Button)target;
			btn_PrevTracker.Click += Btn_PrevTracker;
			break;
		case 47:
			btn_NextTracker = (Button)target;
			btn_NextTracker.Click += Btn_NextTracker_Click;
			break;
		case 48:
			rad_FollowV = (RadioButton)target;
			break;
		case 49:
			rad_AutoSizeV = (RadioButton)target;
			break;
		case 50:
			cb_FollowVMode = (ComboBox)target;
			break;
		case 51:
			tab_H = (TabItem)target;
			break;
		case 52:
			grd_ScanH_Block = (Grid)target;
			break;
		case 53:
			cb_ErrorBin_H = (ComboBox)target;
			break;
		case 54:
			btn_Test_H = (Button)target;
			btn_Test_H.Click += Btn_Test_Click;
			break;
		case 55:
			rad_Direction_H = (RadioButton)target;
			break;
		case 56:
			rad_Direction_H2 = (RadioButton)target;
			break;
		case 57:
			btn_ScanL2R1 = (Button)target;
			btn_ScanL2R1.Click += Btn_ScanL2R_Click;
			break;
		case 58:
			btn_ScanR2L1 = (Button)target;
			btn_ScanR2L1.Click += Btn_ScanR2L_Click;
			break;
		case 59:
			rad_Direction_H3 = (RadioButton)target;
			rad_Direction_H3.Checked += Rad_Direction_H3_Checked;
			break;
		case 60:
			rad_Direction_H4 = (RadioButton)target;
			rad_Direction_H4.Checked += Rad_Direction_H4_Checked;
			break;
		case 61:
			rad_L2R3 = (RadioButton)target;
			break;
		case 62:
			rad_R2L3 = (RadioButton)target;
			break;
		case 63:
			cb_FindEdgeMethod_H = (ComboBox)target;
			break;
		case 64:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 65:
			rt_ResultH = (Rectangle)target;
			break;
		case 66:
			chk_UseStdFilterH = (CheckBox)target;
			break;
		case 67:
			grd_HSelectFace = (Grid)target;
			break;
		case 68:
			btn_HFaceA = (Button)target;
			btn_HFaceA.Click += Btn_HFaceA_Click;
			break;
		case 69:
			btn_HFaceB = (Button)target;
			btn_HFaceB.Click += Btn_HFaceA_Click;
			break;
		case 70:
			btn_HFaceC = (Button)target;
			btn_HFaceC.Click += Btn_HFaceA_Click;
			break;
		case 71:
			btn_HFaceD = (Button)target;
			btn_HFaceD.Click += Btn_HFaceA_Click;
			break;
		case 72:
			chk_HSelectFace = (CheckBox)target;
			chk_HSelectFace.Click += Chk_HSelectFace_Click;
			break;
		case 73:
			chk_UseColorInverseH = (CheckBox)target;
			break;
		case 74:
			cb_TrackerSelect_H = (ComboBox)target;
			cb_TrackerSelect_H.SelectionChanged += Cb_TrackerSelect_H_SelectionChanged;
			break;
		case 75:
			txt_TrackerName_H = (TextBox)target;
			break;
		case 76:
			scr_TrackerSwich_H = (ScrollBar)target;
			scr_TrackerSwich_H.ValueChanged += Scr_TrackerSwich_H_ValueChanged;
			break;
		case 77:
			ts_TrackerON_H = (ToggleSwitch)target;
			ts_TrackerON_H.Click += Ts_TrackerON_Click;
			break;
		case 78:
			btn_ReverseH = (Button)target;
			btn_ReverseH.Click += Btn_Reverse_Click;
			break;
		case 79:
			btn_RecipeCopyH = (Button)target;
			btn_RecipeCopyH.Click += Btn_RecipeCopy_Click;
			break;
		case 80:
			btn_RecipePasteH = (Button)target;
			btn_RecipePasteH.Click += Btn_RecipePaste_Click;
			break;
		case 81:
			btn_PrevTrackerH = (Button)target;
			btn_PrevTrackerH.Click += Btn_PrevTrackerH_Click;
			break;
		case 82:
			btn_NextTrackerH = (Button)target;
			btn_NextTrackerH.Click += Btn_NextTrackerH_Click;
			break;
		case 83:
			rad_FollowH = (RadioButton)target;
			break;
		case 84:
			rad_AutoSizeH = (RadioButton)target;
			break;
		case 85:
			tab_A = (TabItem)target;
			break;
		case 86:
			grd_ScanA_Block = (Grid)target;
			break;
		case 87:
			rt_UseBinary = (Rectangle)target;
			break;
		case 88:
			rt_UseAdvLocate = (Rectangle)target;
			break;
		case 89:
			cb_RectIndexT = (ComboBox)target;
			cb_RectIndexT.SelectionChanged += Cb_RectIndexT_SelectionChanged;
			break;
		case 90:
			cb_RectIndexB = (ComboBox)target;
			cb_RectIndexB.SelectionChanged += Cb_RectIndexT_SelectionChanged;
			break;
		case 91:
			cb_RectIndexL = (ComboBox)target;
			cb_RectIndexL.SelectionChanged += Cb_RectIndexT_SelectionChanged;
			break;
		case 92:
			cb_RectIndexR = (ComboBox)target;
			cb_RectIndexR.SelectionChanged += Cb_RectIndexT_SelectionChanged;
			break;
		case 93:
			txt_WidthMin = (TextBox)target;
			txt_WidthMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 94:
			txt_WidthMax = (TextBox)target;
			txt_WidthMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 95:
			txt_WidthValue = (TextBox)target;
			break;
		case 96:
			txt_HeightMin = (TextBox)target;
			txt_HeightMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 97:
			txt_HeightMax = (TextBox)target;
			txt_HeightMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 98:
			txt_HeightValue = (TextBox)target;
			break;
		case 99:
			btn_Test1__A = (Button)target;
			btn_Test1__A.Click += Btn_Test_Click;
			btn_Test1__A.PreviewMouseDoubleClick += Btn_Test1__A_PreviewMouseDoubleClick;
			break;
		case 100:
			cb_ErrorBin_A = (ComboBox)target;
			break;
		case 101:
			rt_ResultA = (Rectangle)target;
			break;
		case 102:
			grd_ASelectFace = (Grid)target;
			break;
		case 103:
			btn_AFaceA = (Button)target;
			btn_AFaceA.Click += Btn_AFaceA_Click;
			break;
		case 104:
			btn_AFaceB = (Button)target;
			btn_AFaceB.Click += Btn_AFaceA_Click;
			break;
		case 105:
			btn_AFaceC = (Button)target;
			btn_AFaceC.Click += Btn_AFaceA_Click;
			break;
		case 106:
			btn_AFaceD = (Button)target;
			btn_AFaceD.Click += Btn_AFaceA_Click;
			break;
		case 107:
			btn_ValueCopy = (Button)target;
			btn_ValueCopy.Click += Btn_ValueCopy_Click;
			break;
		case 108:
			btn_ValuePaste = (Button)target;
			btn_ValuePaste.Click += Btn_ValuePaste_Click;
			break;
		case 109:
			chk_ASelectFace = (CheckBox)target;
			chk_ASelectFace.Click += Chk_ASelectFace_Click;
			break;
		case 110:
			btn_ChartW = (Button)target;
			btn_ChartW.Click += Btn_Chart_Click;
			break;
		case 111:
			btn_ChartH = (Button)target;
			btn_ChartH.Click += Btn_Chart_Click;
			break;
		case 112:
			chk_EnableWidthRatio = (CheckBox)target;
			break;
		case 113:
			txt_WidthRatio = (TextBox)target;
			txt_WidthRatio.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 114:
			chk_EnableHeightRatio = (CheckBox)target;
			break;
		case 115:
			txt_HeightRatio = (TextBox)target;
			txt_HeightRatio.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 116:
			rt_EnabledFake = (Rectangle)target;
			break;
		case 117:
			rad_UseAdvLocate = (RadioButton)target;
			rad_UseAdvLocate.Checked += Rad_UseAdvLocate_Checked;
			break;
		case 118:
			rad_UseBinary = (RadioButton)target;
			rad_UseBinary.Checked += Rad_UseBinary_Checked;
			break;
		case 119:
			rad_FollowA = (RadioButton)target;
			break;
		case 120:
			rad_AutoSizeA = (RadioButton)target;
			break;
		case 121:
			cb_FollowA = (ComboBox)target;
			cb_FollowA.SelectionChanged += Cb_RectIndexT_SelectionChanged;
			break;
		case 122:
			txt_RectShiftT = (TextBox)target;
			txt_RectShiftT.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 123:
			txt_RectShiftB = (TextBox)target;
			txt_RectShiftB.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 124:
			txt_RectShiftL = (TextBox)target;
			txt_RectShiftL.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 125:
			txt_RectShiftR = (TextBox)target;
			txt_RectShiftR.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 126:
			cb_TrackerSelect_A = (ComboBox)target;
			cb_TrackerSelect_A.SelectionChanged += Cb_TrackerSelect_A_SelectionChanged;
			break;
		case 127:
			txt_TrackerName_A = (TextBox)target;
			break;
		case 128:
			scr_TrackerSwich_A = (ScrollBar)target;
			scr_TrackerSwich_A.ValueChanged += Scr_TrackerSwich_A_ValueChanged;
			break;
		case 129:
			ts_TrackerON_A = (ToggleSwitch)target;
			ts_TrackerON_A.Click += Ts_TrackerON_Click;
			break;
		case 130:
			btn_ReverseA = (Button)target;
			btn_ReverseA.Click += Btn_Reverse_Click;
			break;
		case 131:
			btn_RecipeCopyA = (Button)target;
			btn_RecipeCopyA.Click += Btn_RecipeCopy_Click;
			break;
		case 132:
			btn_RecipePasteA = (Button)target;
			btn_RecipePasteA.Click += Btn_RecipePaste_Click;
			break;
		case 133:
			lab_Time3 = (Label)target;
			break;
		case 134:
			btn_PrevTrackerA = (Button)target;
			btn_PrevTrackerA.Click += Btn_PrevTrackerA_Click;
			break;
		case 135:
			btn_NextTrackerA = (Button)target;
			btn_NextTrackerA.Click += Btn_NextTrackerA_Click;
			break;
		case 136:
			tab_M = (TabItem)target;
			break;
		case 137:
			cb_TrackerSelect_M = (ComboBox)target;
			cb_TrackerSelect_M.SelectionChanged += Cb_TrackerSelect_M_SelectionChanged;
			break;
		case 138:
			txt_TrackerName_M = (TextBox)target;
			break;
		case 139:
			scr_TrackerSwich_A1 = (ScrollBar)target;
			scr_TrackerSwich_A1.ValueChanged += Scr_TrackerSwich_A_ValueChanged;
			break;
		case 140:
			ts_TrackerON_M = (ToggleSwitch)target;
			ts_TrackerON_M.Click += Ts_TrackerON_Click;
			break;
		case 141:
			btn_ReverseM = (Button)target;
			btn_ReverseM.Click += Btn_Reverse_Click;
			break;
		case 142:
			btn_RecipeCopyM = (Button)target;
			btn_RecipeCopyM.Click += Btn_RecipeCopy_Click;
			break;
		case 143:
			btn_RecipePasteM = (Button)target;
			btn_RecipePasteM.Click += Btn_RecipePaste_Click;
			break;
		case 144:
			lab_Time4 = (Label)target;
			break;
		case 145:
			grd_ScanM_Block = (Grid)target;
			break;
		case 146:
			btn_Test_V1 = (Button)target;
			btn_Test_V1.Click += Btn_Test_Click;
			break;
		case 147:
			rad_MeasureW = (RadioButton)target;
			rad_MeasureW.Checked += Rad_MeasureW_Checked;
			break;
		case 148:
			rad_MeasureH = (RadioButton)target;
			rad_MeasureH.Checked += Rad_MeasureH_Checked;
			break;
		case 149:
			rad_MeasureD = (RadioButton)target;
			rad_MeasureD.Checked += Rad_MeasureD_Checked;
			break;
		case 150:
			rad_MeasureK = (RadioButton)target;
			rad_MeasureK.Checked += Rad_MeasureK_Checked;
			break;
		case 151:
			rad_MeasureS = (RadioButton)target;
			rad_MeasureS.Checked += Rad_MeasureS_Checked;
			break;
		case 152:
			rad_L2R2 = (RadioButton)target;
			break;
		case 153:
			rad_R2L2 = (RadioButton)target;
			break;
		case 154:
			rt_ResultM = (Rectangle)target;
			break;
		case 155:
			grd_MSelectFace = (Grid)target;
			break;
		case 156:
			btn_MFaceA = (Button)target;
			btn_MFaceA.Click += Btn_MFaceA_Click;
			break;
		case 157:
			btn_MFaceB = (Button)target;
			btn_MFaceB.Click += Btn_MFaceA_Click;
			break;
		case 158:
			btn_MFaceC = (Button)target;
			btn_MFaceC.Click += Btn_MFaceA_Click;
			break;
		case 159:
			btn_MFaceD = (Button)target;
			btn_MFaceD.Click += Btn_MFaceA_Click;
			break;
		case 160:
			rad_MeasreStartWH = (RadioButton)target;
			rad_MeasreStartWH.Checked += Rad_MeasreStartWH_Checked;
			break;
		case 161:
			rad_MeasreStartA = (RadioButton)target;
			rad_MeasreStartA.Checked += Rad_MeasreStartA_Checked_1;
			break;
		case 162:
			cb_MeasureStartWHIndex = (ComboBox)target;
			cb_MeasureStartWHIndex.SelectionChanged += Cb_MeasureStartWHIndex_SelectionChanged;
			break;
		case 163:
			cb_MeasureStartAIndex = (ComboBox)target;
			cb_MeasureStartAIndex.SelectionChanged += Cb_MeasureStartAIndex_SelectionChanged;
			break;
		case 164:
			cb_MeasureStartAPt = (ComboBox)target;
			cb_MeasureStartAPt.SelectionChanged += Cb_MeasureStartAIndex_SelectionChanged;
			break;
		case 165:
			btn_MStartEdit = (Button)target;
			btn_MStartEdit.Click += Btn_MStartEdit_Click;
			break;
		case 166:
			rad_MeasreEndWH = (RadioButton)target;
			rad_MeasreEndWH.Checked += Rad_MeasreEndWH_Checked;
			break;
		case 167:
			cb_MeasureEndWHIndex = (ComboBox)target;
			cb_MeasureEndWHIndex.SelectionChanged += Cb_MeasureEndWHIndex_SelectionChanged;
			break;
		case 168:
			rad_MeasreEndA = (RadioButton)target;
			rad_MeasreEndA.Checked += Rad_MeasreEndA_Checked;
			break;
		case 169:
			cb_MeasureEndAIndex = (ComboBox)target;
			cb_MeasureEndAIndex.SelectionChanged += Cb_MeasureEndAIndex_SelectionChanged;
			break;
		case 170:
			cb_MeasureEndAPt = (ComboBox)target;
			cb_MeasureEndAPt.SelectionChanged += Cb_MeasureEndAIndex_SelectionChanged;
			break;
		case 171:
			btn_MEndEdit = (Button)target;
			btn_MEndEdit.Click += Btn_MEndEdit_Click;
			break;
		case 172:
			grd_PtPreview = (Grid)target;
			break;
		case 173:
			txt_ValueMin = (TextBox)target;
			txt_ValueMin.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 174:
			txt_ValueMax = (TextBox)target;
			txt_ValueMax.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 175:
			txt_Distance = (TextBox)target;
			break;
		case 176:
			cb_ErrorBin_M = (ComboBox)target;
			break;
		case 177:
			btn_ChartM = (Button)target;
			btn_ChartM.Click += Btn_ChartM_Click;
			break;
		case 178:
			chk_MeasureAllX = (CheckBox)target;
			break;
		case 179:
			chk_MeasureAllY = (CheckBox)target;
			break;
		case 180:
			grd_CustomRatio = (Grid)target;
			break;
		case 181:
			grd_KValueSetup = (Grid)target;
			break;
		case 182:
			txt_ScanExtend = (TextBox)target;
			txt_ScanExtend.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 183:
			txt_ScanShrink = (TextBox)target;
			txt_ScanShrink.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 184:
			chk_MSelectFace = (CheckBox)target;
			chk_MSelectFace.Click += Chk_MSelectFace_Click;
			break;
		case 185:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 186:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 187:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 188:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 189:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 190:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 191:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 192:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 193:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 194:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 195:
			txt_Tracker1Pos = (Label)target;
			break;
		case 196:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 197:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 198:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 199:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 200:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 201:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 202:
			lab_RValue = (Label)target;
			break;
		case 203:
			lab_GValue = (Label)target;
			break;
		case 204:
			lab_BValue = (Label)target;
			break;
		case 205:
			lab_HValue = (Label)target;
			break;
		case 206:
			lab_SValue = (Label)target;
			break;
		case 207:
			lab_VValue = (Label)target;
			break;
		case 208:
			lab_GrayValue = (Label)target;
			break;
		case 209:
			lab_XValue = (Label)target;
			break;
		case 210:
			lab_YValue = (Label)target;
			break;
		case 211:
			lab_C1Value = (Label)target;
			break;
		case 212:
			lab_C2Value = (Label)target;
			break;
		case 213:
			lab_C3Value = (Label)target;
			break;
		case 214:
			lab_C4Value = (Label)target;
			break;
		case 215:
			lab_C5Value = (Label)target;
			break;
		case 216:
			lab_C6Value = (Label)target;
			break;
		case 217:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 218:
			txt_AllResult = (TextBox)target;
			break;
		case 219:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 220:
			grd_BinaryBlock = (Grid)target;
			break;
		case 221:
			exp_THD1 = (Expander)target;
			break;
		case 222:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 223:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 224:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 225:
			cb_THD1_DEStart = (ComboBox)target;
			break;
		case 226:
			cb_THD1_DEEnd = (ComboBox)target;
			break;
		case 227:
			cb_THD1_DEEndCount = (ComboBox)target;
			break;
		case 228:
			cb_THD1_DEStartCount = (ComboBox)target;
			break;
		case 229:
			cb_Channel_THD1 = (ComboBox)target;
			break;
		case 230:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 231:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 232:
			btn_THD1Undo = (Button)target;
			btn_THD1Undo.Click += Btn_THD1Undo_Click;
			break;
		case 233:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSUL_Click;
			break;
		case 234:
			exp_THD2 = (Expander)target;
			break;
		case 235:
			grd_SIMP_THD_1_Copy = (Grid)target;
			break;
		case 236:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 237:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 238:
			cb_THD2_DEStart = (ComboBox)target;
			break;
		case 239:
			cb_THD2_DEEnd = (ComboBox)target;
			break;
		case 240:
			cb_THD2_DEEndCount = (ComboBox)target;
			break;
		case 241:
			cb_THD2_DEStartCount = (ComboBox)target;
			break;
		case 242:
			cb_Channel_THD2 = (ComboBox)target;
			break;
		case 243:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 244:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 245:
			btn_THD2Undo = (Button)target;
			btn_THD2Undo.Click += Btn_THD1Undo_Click;
			break;
		case 246:
			exp_THD3 = (Expander)target;
			break;
		case 247:
			grd_SIMP_THD_1_Copy1 = (Grid)target;
			break;
		case 248:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 249:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 250:
			cb_THD3_DEStart = (ComboBox)target;
			break;
		case 251:
			cb_THD3_DEEnd = (ComboBox)target;
			break;
		case 252:
			cb_THD3_DEEndCount = (ComboBox)target;
			break;
		case 253:
			cb_THD3_DEStartCount = (ComboBox)target;
			break;
		case 254:
			cb_Channel_THD3 = (ComboBox)target;
			break;
		case 255:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 256:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 257:
			btn_THD3Undo = (Button)target;
			btn_THD3Undo.Click += Btn_THD1Undo_Click;
			break;
		case 258:
			btn_ColorCopy = (Button)target;
			btn_ColorCopy.Click += Btn_ColorCopy_Click;
			break;
		case 259:
			btn_ColorPaste = (Button)target;
			btn_ColorPaste.Click += Btn_ColorPaste_Click;
			break;
		case 260:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 261:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 262:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 263:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 264:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 265:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 266:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 267:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 268:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 269:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 270:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 271:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 272:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 273:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 274:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 275:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 276:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 277:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 278:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 279:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 280:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 281:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 282:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 283:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 284:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 285:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 286:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 287:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 288:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 289:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 290:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 291:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 292:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 293:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 294:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 295:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 296:
			grd_TrackerAdj = (Grid)target;
			break;
		case 297:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 298:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 299:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 300:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 301:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 302:
			lab_FaceIndex = (Label)target;
			break;
		case 303:
			btn_Tools21 = (Button)target;
			btn_Tools21.Click += Btn_Tools1_Click;
			break;
		case 304:
			btn_Tools22 = (Button)target;
			btn_Tools22.Click += Btn_Tools1_Click;
			break;
		case 305:
			btn_Tools23 = (Button)target;
			btn_Tools23.Click += Btn_Tools1_Click;
			break;
		case 306:
			btn_Tools24 = (Button)target;
			btn_Tools24.Click += Btn_Tools1_Click;
			break;
		case 307:
			btn_Tools25 = (Button)target;
			btn_Tools25.Click += Btn_Tools1_Click;
			break;
		case 308:
			btn_Tools26 = (Button)target;
			btn_Tools26.Click += Btn_Tools1_Click;
			break;
		case 309:
			btn_Tools27 = (Button)target;
			btn_Tools27.Click += Btn_Tools1_Click;
			break;
		case 310:
			btn_Tools28 = (Button)target;
			btn_Tools28.Click += Btn_Tools1_Click;
			break;
		case 311:
			btn_Tools29 = (Button)target;
			btn_Tools29.Click += Btn_Tools1_Click;
			break;
		case 312:
			btn_Tools30 = (Button)target;
			btn_Tools30.Click += Btn_Tools1_Click;
			break;
		case 313:
			btn_Tools31 = (Button)target;
			btn_Tools31.Click += Btn_Tools1_Click;
			break;
		case 314:
			btn_Tools32 = (Button)target;
			btn_Tools32.Click += Btn_Tools1_Click;
			break;
		case 315:
			btn_Tools33 = (Button)target;
			btn_Tools33.Click += Btn_Tools1_Click;
			break;
		case 316:
			btn_Tools34 = (Button)target;
			btn_Tools34.Click += Btn_Tools1_Click;
			break;
		case 317:
			btn_Tools35 = (Button)target;
			btn_Tools35.Click += Btn_Tools1_Click;
			break;
		case 318:
			btn_Tools36 = (Button)target;
			btn_Tools36.Click += Btn_Tools1_Click;
			break;
		case 319:
			btn_Tools37 = (Button)target;
			btn_Tools37.Click += Btn_Tools1_Click;
			break;
		case 320:
			btn_Tools38 = (Button)target;
			btn_Tools38.Click += Btn_Tools1_Click;
			break;
		case 321:
			btn_Tools39 = (Button)target;
			btn_Tools39.Click += Btn_Tools1_Click;
			break;
		case 322:
			btn_Tools40 = (Button)target;
			btn_Tools40.Click += Btn_Tools1_Click;
			break;
		case 323:
			lab_Pareto = (Label)target;
			break;
		case 324:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 325:
			Label_Index = (Label)target;
			break;
		case 326:
			lab_LockMode = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
