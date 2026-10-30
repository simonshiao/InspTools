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
using OpenCvSharp.Extensions;

namespace InspTools.AOI;

public class BlockAvgToolWindow : System.Windows.Window, IComponentConnector
{
	private const int MaxThdTool = 4;

	private int LanguageType = 1;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ShowMessage = true;

	private bool TrackerShow = true;

	private byte[] CurrentColor = new byte[16];

	private bool[] SearchRectResult = new bool[2];

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private OpenCvSharp.Rect LastSearchRect2;

	private OpenCvSharp.Rect[] MaskRect = new OpenCvSharp.Rect[2];

	private BlobStruct[] Blobs = new BlobStruct[2];

	private INSP_DIFFERENCETOOL_PARAM CopyParam;

	private INSP_DIFFERENCETOOL_PARAM BackupParam;

	private bool CopyParamSeted;

	private bool DontSave;

	private bool ReferenceShow = true;

	private int tracker2DiableRuleCount = 6;

	private int SelectFace;

	private int FaceIndex;

	private bool TrackerAdjShow;

	private bool TrackerFillletShow;

	private int CurrentTracker;

	private string Tracker1Str;

	private string Tracker2Str;

	private bool SwitchToTempImage;

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat[] BinaryBlob = new Mat[2];

	private Mat BinaryT2 = new Mat();

	private Mat ImageSrc = new Mat();

	private int ImageListSource;

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private double CanvasWidth;

	private double CanvasHeight;

	private string[] filelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int TrackIndex;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private Stopwatch sw = new Stopwatch();

	private TextBox[] Result1Txt;

	private TextBox[] Result2Txt;

	private Button[] ToolsButton;

	private ComboBox[] InspRule;

	private bool EnableModifyLog;

	private string ModifyLog = "";

	private string RecipeName = "";

	private string UserID = "123456";

	private bool PreviewMode;

	private User_Permissions User = User_Permissions.USER_Administrator;

	internal BlockAvgToolWindow MainW;

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

	internal Label Label_Result;

	internal Rectangle rt_Mask1;

	internal Rectangle rt_Mask2;

	internal Rectangle rt_ReferenceRect1;

	internal Rectangle rt_ReferenceRect2;

	internal Label lab_FaceIndex;

	internal TextBox Label_ResultError;

	internal Button btn_ResetA;

	internal Button btn_ResetB;

	internal TextBox txt_PreviewMode;

	internal TabControl tab_Function;

	internal Grid grd_Main;

	internal ComboBox cb_TrackerSelect;

	internal ScrollBar scr_TrackerSwich;

	internal Button btn_Reverse;

	internal Button btn_RecipeCopy;

	internal Button btn_RecipePaste;

	internal TextBox txt_TrackerName;

	internal ToggleSwitch ts_TrackerON;

	internal Label lab_TrackIndex;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal Label lab_Time;

	internal GroupBox gb_BasicSetup;

	internal Grid grd_SIMP_THD_2;

	internal Grid grd_SIMP_DilateErode4;

	internal ComboBox cb_ThdDEStart;

	internal ComboBox cb_ThdDEEnd;

	internal ComboBox cb_ThdDEEndCount;

	internal ComboBox cb_ThdDEStartCount;

	internal ComboBox cb_ChannelIndex;

	internal CheckBox chk_ThdUpON;

	internal CheckBox chk_ThdDownON;

	internal NumericUpDown nu_ThdUp;

	internal NumericUpDown nu_ThdDown;

	internal NumericUpDown nu_P2PIntervalX;

	internal NumericUpDown nu_P2PIntervalY;

	internal GroupBox gb_ConditionSetup;

	internal Grid grd_ConditionSetup;

	internal ComboBox cb_ErrorBin;

	internal Rectangle rt_Result;

	internal Button btn_Test;

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

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal Expander exp_ColorLimit;

	internal Grid grd_SIMP_THD_3;

	internal RangeSlider rs_ThdLimit;

	internal NumericUpDown nu_ThdLimitMax;

	internal NumericUpDown nu_ThdLimitMin;

	internal CheckBox cb_FilterNoise;

	internal NumericUpDown nu_NoiseWidth;

	internal NumericUpDown nu_NoiseHeight;

	internal CheckBox cb_UseAvgBinary;

	internal RadioButton rad_AvgBinaryA;

	internal RadioButton rad_AvgBinaryH;

	internal RadioButton rad_AvgBinaryV;

	internal ComboBox cb_AvgShift;

	internal Grid grd_AvgThdFun;

	internal TextBox txt_AvgGray;

	internal TextBox txt_AvgGrayMin;

	internal TextBox txt_AvgGrayMax;

	internal Expander exp_TrackerSetup;

	internal Grid grd_SIMP_DilateErode1_Copy;

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

	internal Button btn_TrackerReset2;

	internal Button btn_Tracker2SizeChange;

	internal Label txt_Tracker2Pos;

	internal CheckBox ts_Tracker2ON;

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

	internal Button btn_MaskPreView;

	internal Button btn_TextMaskPreView;

	internal Button btn_PrevTracker;

	internal Button btn_NextTracker;

	internal TextBox txt_AllResult;

	internal Button btn_TrackerShow;

	internal Button btn_TestNextImg;

	internal Button btn_TestPrevImg;

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

	internal Button btn_ReferenceShow;

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

	internal Label lab_Tracker1;

	internal Label lab_Tracker2;

	internal Button btn_Fillet;

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

	internal Button btn_ImgAIR;

	internal Button btn_ImgAIG;

	internal Button btn_ImgAIB;

	internal Button btn_ImgC1;

	internal Button btn_ImgC2;

	internal Button btn_ImgC3;

	internal Button btn_ImgAI;

	internal Button btn_ImgC4;

	internal Button btn_ImgC5;

	internal Button btn_ImgC6;

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

	internal Label Label_Index;

	internal TextBox lab_LockMode;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public BlockAvgToolWindow(User_Permissions permissions)
	{
		inited = false;
		InitializeComponent();
		User = permissions;
		grd_Main.Visibility = Visibility.Hidden;
		inited = true;
		THD_Preview = false;
		Label_ResultError.Visibility = Visibility.Hidden;
		rt_Mask1.Visibility = Visibility.Hidden;
		rt_Mask2.Visibility = Visibility.Hidden;
		CanvasWidth = cnv_Image.Width;
		CanvasHeight = cnv_Image.Height;
		Result1Txt = new TextBox[4] { txt_InspRule11Result, txt_InspRule12Result, txt_InspRule13Result, txt_InspRule14Result };
		Result2Txt = new TextBox[4] { txt_InspRule21Result, txt_InspRule22Result, txt_InspRule23Result, txt_InspRule24Result };
		ToolsButton = new Button[20]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20
		};
		InspRule = new ComboBox[4] { cb_InspRule1, cb_InspRule2, cb_InspRule3, cb_InspRule4 };
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
		txt_PreviewMode.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(int index, CVInspection station, int faceIndex = 0)
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
			for (int j = 0; j < 20; j++)
			{
				if (inspection.paramFile.MatchParam[j].TrackerON)
				{
					cb_MatchMaskSelect1.Items.Add(j + 1 + " : " + inspection.paramFile.MatchParam[j].TrackerName);
					cb_MatchMaskSelect2.Items.Add(j + 1 + " : " + inspection.paramFile.MatchParam[j].TrackerName);
					cb_MatchMaskSelect3.Items.Add(j + 1 + " : " + inspection.paramFile.MatchParam[j].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem2 = new ComboBoxItem();
					comboBoxItem2.Content = j + 1 + " : " + inspection.paramFile.MatchParam[j].TrackerName;
					comboBoxItem2.IsEnabled = false;
					cb_MatchMaskSelect1.Items.Add(comboBoxItem2);
					ComboBoxItem comboBoxItem3 = new ComboBoxItem();
					comboBoxItem3.Content = j + 1 + " : " + inspection.paramFile.MatchParam[j].TrackerName;
					comboBoxItem3.IsEnabled = false;
					cb_MatchMaskSelect2.Items.Add(comboBoxItem3);
					ComboBoxItem comboBoxItem4 = new ComboBoxItem();
					comboBoxItem4.Content = j + 1 + " : " + inspection.paramFile.MatchParam[j].TrackerName;
					comboBoxItem4.IsEnabled = false;
					cb_MatchMaskSelect3.Items.Add(comboBoxItem4);
				}
				cb_TrackerSelect.Items.Add(j + 1 + " : " + inspection.paramFile.BlockAvgParam[j].TrackerName);
				if (!inspection.paramFile.AdvLocateToolON)
				{
					if (j < 2)
					{
						cb_Track1AlignWayX.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
						cb_Track2AlignWayX.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
						cb_Track1AlignWayY.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
						cb_Track2AlignWayY.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
						if (j < 1)
						{
							cb_Track1AutoSize.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName);
							cb_Track2AutoSize.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName);
						}
					}
					continue;
				}
				if (inspection.paramFile.VSLocateParam[j].TrackerON)
				{
					cb_Track1AlignWayX.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
					cb_Track2AlignWayX.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem5 = new ComboBoxItem();
					comboBoxItem5.Content = j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName;
					comboBoxItem5.IsEnabled = false;
					cb_Track1AlignWayX.Items.Add(comboBoxItem5);
					ComboBoxItem comboBoxItem6 = new ComboBoxItem();
					comboBoxItem6.Content = j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName;
					comboBoxItem6.IsEnabled = false;
					cb_Track2AlignWayX.Items.Add(comboBoxItem6);
				}
				if (inspection.paramFile.HSLocateParam[j].TrackerON)
				{
					cb_Track1AlignWayY.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
					cb_Track2AlignWayY.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem7 = new ComboBoxItem();
					comboBoxItem7.Content = j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName;
					comboBoxItem7.IsEnabled = false;
					cb_Track1AlignWayY.Items.Add(comboBoxItem7);
					ComboBoxItem comboBoxItem8 = new ComboBoxItem();
					comboBoxItem8.Content = j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName;
					comboBoxItem8.IsEnabled = false;
					cb_Track2AlignWayY.Items.Add(comboBoxItem8);
				}
				if (inspection.paramFile.AdvLocateParam[j].TrackerON)
				{
					cb_Track1AutoSize.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName);
					cb_Track2AutoSize.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem9 = new ComboBoxItem();
				comboBoxItem9.Content = j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName;
				comboBoxItem9.IsEnabled = false;
				cb_Track1AutoSize.Items.Add(comboBoxItem9);
				ComboBoxItem comboBoxItem10 = new ComboBoxItem();
				comboBoxItem10.Content = j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName;
				comboBoxItem10.IsEnabled = false;
				cb_Track2AutoSize.Items.Add(comboBoxItem10);
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
		catch
		{
			string text = "BAW01-BlockAvgWindow-SetInspection()";
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void UpdateToolBox()
	{
		for (int i = 0; i < 20; i++)
		{
			if (inspection.paramFile.BlockAvgParam[i].TrackerON)
			{
				if (inspection.iResult.BlockAvgResult[i] == TOOL_RESULT.EM_OK)
				{
					if (inspection.paramFile.BlockAvgParam[i].EnableSelectFace)
					{
						if (inspection.paramFile.BlockAvgParam[i].FaceIndex == FaceIndex)
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
				else if (inspection.iResult.BlockAvgResult[i] == TOOL_RESULT.EM_NG)
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
			ToolsButton[i].ToolTip = inspection.paramFile.BlockAvgParam[i].TrackerName;
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

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
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
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BlockAvgParam[TrackIndex].TrackerName) + "BT01-BlockAvgWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		WriteUIValueToParam();
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
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

	private void SetMaskTracker(OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2)
	{
		OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		if (rect1.Width > 0 && rect1.Height > 0)
		{
			rect1 &= rect3;
			rt_Mask1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			rt_Mask1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			rt_Mask1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			rt_Mask1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
		}
		if (rect2.Width > 0 && rect2.Height > 0)
		{
			rect2 &= rect3;
			rt_Mask2.SetValue(Canvas.LeftProperty, (double)rect2.X * xRatio);
			rt_Mask2.SetValue(Canvas.TopProperty, (double)rect2.Y * yRatio);
			rt_Mask2.SetValue(FrameworkElement.WidthProperty, (double)rect2.Width * xRatio);
			rt_Mask2.SetValue(FrameworkElement.HeightProperty, (double)rect2.Height * yRatio);
		}
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
				DontSave = true;
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
			DontSave = true;
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
			txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
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
				txt_Tracker2Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
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
			nu_ThdLimitMin.Value = (int)rs_ThdLimit.LowerValue;
			nu_ThdLimitMax.Value = (int)rs_ThdLimit.UpperValue;
			if (ts_TrackerON.IsChecked == true)
			{
				Btn_Test_Click(sender, e);
			}
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
			Btn_Test_Click(this, null);
			THD_Preview = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetConditionText(bool tracker2ON)
	{
		int[] array = new int[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = InspRule[i].SelectedIndex;
			InspRule[i].Items.Clear();
		}
		int num = ((!tracker2ON) ? tracker2DiableRuleCount : 10);
		string newItem = "";
		for (int j = 0; j < num; j++)
		{
			switch (j)
			{
			case 0:
				newItem = "-----";
				break;
			case 1:
				newItem = LanguageManager.Instance["Width"];
				break;
			case 2:
				newItem = LanguageManager.Instance["Height"];
				break;
			case 3:
				newItem = LanguageManager.Instance["Area"];
				break;
			case 4:
				newItem = LanguageManager.Instance["WHRatio"];
				break;
			case 5:
				newItem = LanguageManager.Instance["AreaRatio"];
				break;
			case 6:
				newItem = LanguageManager.Instance["ABAreaRatio"];
				break;
			case 7:
				newItem = LanguageManager.Instance["ABHDiff"];
				break;
			case 8:
				newItem = LanguageManager.Instance["ABWDiff"];
				break;
			case 9:
				newItem = LanguageManager.Instance["ABWRatio"];
				break;
			}
			cb_InspRule1.Items.Add(newItem);
			cb_InspRule2.Items.Add(newItem);
			cb_InspRule3.Items.Add(newItem);
			cb_InspRule4.Items.Add(newItem);
		}
		for (int k = 0; k < 4; k++)
		{
			InspRule[k].SelectedIndex = array[k];
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
				inspection.blockavgTool[TrackIndex].Blobs[0].NGRect,
				inspection.blockavgTool[TrackIndex].Blobs[1].NGRect
			};
			double[] array2 = new double[2]
			{
				inspection.blockavgTool[TrackIndex].Blobs[0].Area,
				inspection.blockavgTool[TrackIndex].Blobs[1].Area
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
				return true;
			}
			return false;
		}
		catch
		{
			string text7 = "BAW02-BlockAvgWindow-CheckResult()";
			CVBuffer.SaveLog(text7, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text7, "錯誤");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, TextBox min, TextBox max, TextBox resultbox, OpenCvSharp.Rect rect, double area, int TrackerIndex)
	{
		bool result = true;
		resultbox.Foreground = Brushes.Black;
		try
		{
			switch (rule)
			{
			case 0:
				resultbox.Text = "0";
				break;
			case 1:
			{
				double num4 = (double)rect.Width * inspection.paramFile.PixelPerUM_X;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num4);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num4);
				}
				if (num4 < Convert.ToDouble(min.Text) || num4 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num4 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num4 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 2:
			{
				double num3 = (double)rect.Height * inspection.paramFile.PixelPerUM_Y;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num3);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num3);
				}
				if (num3 < Convert.ToDouble(min.Text) || num3 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num3 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num3 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 3:
			{
				double num8 = Convert.ToDouble(area) * inspection.paramFile.PixelPerUM_X * inspection.paramFile.PixelPerUM_Y;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num8);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num8);
				}
				if (num8 < Convert.ToDouble(min.Text) || num8 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num8 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num8 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 4:
			{
				double num2 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				resultbox.Text = string.Format("{0:N3}", num2);
				if (num2 < Convert.ToDouble(min.Text) || num2 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num2 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num2 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 5:
			{
				double num5 = ((rect.Width * rect.Height > 0) ? (area / (double)(rect.Width * rect.Height)) : 0.0);
				resultbox.Text = string.Format("{0:N3}", num5);
				Blobs[TrackerIndex].AreaRatio = num5;
				if (num5 < Convert.ToDouble(min.Text) || num5 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num5 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num5 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 6:
			{
				if (TrackerIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num6 = 0.0;
				resultbox.Text = string.Format("{0:N3}", num6);
				if (!(Blobs[0].Area > 0.0) || !(Blobs[1].Area > 0.0))
				{
					return false;
				}
				num6 = ((!(Blobs[0].Area > Blobs[1].Area)) ? (Blobs[1].Area / Blobs[0].Area) : (Blobs[0].Area / Blobs[1].Area));
				Blobs[1].TwoAreaRatio = num6;
				if (num6 < Convert.ToDouble(min.Text) || num6 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num6 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num6 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 7:
			{
				if (TrackerIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num9 = Math.Abs(Blobs[0].Height - Blobs[1].Height);
				num9 *= inspection.paramFile.PixelPerUM_Y;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num9);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num9);
				}
				if (num9 < Convert.ToDouble(min.Text) || num9 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num9 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num9 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 8:
			{
				if (TrackerIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num7 = Math.Abs(Blobs[0].Width - Blobs[1].Width);
				num7 *= inspection.paramFile.PixelPerUM_X;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num7);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num7);
				}
				if (num7 < Convert.ToDouble(min.Text) || num7 > Convert.ToDouble(max.Text))
				{
					result = false;
					if (num7 < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num7 > Convert.ToDouble(max.Text))
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 9:
			{
				if (TrackerIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				double num = 0.0;
				if (Blobs[0].Width > 0.0 && Blobs[1].Width > 0.0)
				{
					num = ((!(Blobs[0].Width > Blobs[1].Width)) ? (Blobs[1].Width / Blobs[0].Width) : (Blobs[0].Width / Blobs[1].Width));
					Blobs[1].WidthRatio = num;
					if (num < Convert.ToDouble(min.Text) || num > Convert.ToDouble(max.Text))
					{
						result = false;
						if (num < Convert.ToDouble(min.Text))
						{
							min.Background = Brushes.Fuchsia;
						}
						if (num > Convert.ToDouble(max.Text))
						{
							max.Background = Brushes.Fuchsia;
						}
						resultbox.Foreground = Brushes.Red;
					}
				}
				resultbox.Text = string.Format("{0:N3}", num);
				break;
			}
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BW03-BlockAvgWindow-GetRule()-";
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
				Label_ResultError.Text = "檢測區域B寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域B!";
			}
			else
			{
				Label_ResultError.Text = "検査領域Bの幅と高さが0未満です。アライメント、自動縮小設定を確認するか、検査領域Bをリセットしてください。";
			}
			Label_ResultError.Visibility = Visibility.Visible;
			return;
		}
		try
		{
			_ = new Mat[2];
			Mat[] array = new Mat[2];
			Mat[] array2 = new Mat[2];
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
			if (!GetTracker(ref rect, 0) || (ts_Tracker2ON.IsChecked == true && !GetTracker(ref rect2, 1)))
			{
				return;
			}
			WriteUIValueToParam();
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			inspection.blockavgTool[TrackIndex].Inspect(TrackIndex, ref iresult);
			array2[0] = inspection.blockavgTool[TrackIndex].Binary[0].Clone();
			array2[1] = inspection.blockavgTool[TrackIndex].Binary[1].Clone();
			if (inspection.blockavgTool[TrackIndex].TrackerError)
			{
				txt_AllResult.Clear();
				txt_AllResult.Text += inspection.blockavgTool[TrackIndex].Tracker1ResultText;
				txt_AllResult.Text += inspection.blockavgTool[TrackIndex].Tracker2ResultText;
				return;
			}
			Mat mat = inspection.buffers.LocateGray.Clone();
			rect = new OpenCvSharp.Rect(rect.Left, rect.Top, array2[0].Width, array2[0].Height);
			array[0] = new Mat(mat, rect);
			array[0].SetTo(0.0);
			Cv2.BitwiseOr(array[0], array2[0], array[0]);
			if (ts_Tracker2ON.IsChecked == true)
			{
				rect2 = new OpenCvSharp.Rect(rect2.X, rect2.Y, array2[1].Width, array2[1].Height);
				array[1] = new Mat(mat, rect2);
				array[1].SetTo(0.0);
				Cv2.BitwiseOr(array[1], array2[1], array[1]);
			}
			if (DrawAll)
			{
				mat.SetTo(0.0);
				array[0].SetTo(255.0);
				if (ts_Tracker2ON.IsChecked == true)
				{
					array[1].SetTo(255.0);
				}
			}
			if (cb_MaskSelect.SelectedIndex > 0)
			{
				int num = cb_MaskSelect.SelectedIndex - 1;
				Mat mat2 = mat.Clone();
				mat2.SetTo(0.0);
				bool flag = false;
				if (MaskRect[0].Width > 0 && MaskRect[0].Height > 0)
				{
					Mat m = new Mat(mat2, MaskRect[0]);
					inspection.paramFile.MaskParam[num].Mask1.Clone().CopyTo(m);
					flag = true;
				}
				if (MaskRect[1].Width > 0 && MaskRect[1].Height > 0)
				{
					Mat m2 = new Mat(mat2, MaskRect[1]);
					inspection.paramFile.MaskParam[num].Mask2.Clone().CopyTo(m2);
					flag = true;
				}
				if (flag)
				{
					if (chk_UseMaskAsInspArea.IsChecked == true)
					{
						Cv2.BitwiseAnd(mat, mat2, mat);
					}
					else
					{
						Cv2.BitwiseNot(mat2, mat2);
						Cv2.BitwiseAnd(mat, mat2, mat);
					}
				}
			}
			inspection.buffers.DoFilllet(ref array[0], new OpenCvSharp.Rect(0, 0, array[0].Width, array[0].Height), cb_T1Filllet0.SelectedIndex, cb_T1Filllet1.SelectedIndex, cb_T1Filllet2.SelectedIndex, cb_T1Filllet3.SelectedIndex);
			if (ts_Tracker2ON.IsChecked == true)
			{
				inspection.buffers.DoFilllet(ref array[1], new OpenCvSharp.Rect(0, 0, array[1].Width, array[1].Height), cb_T2Filllet0.SelectedIndex, cb_T2Filllet1.SelectedIndex, cb_T2Filllet2.SelectedIndex, cb_T2Filllet3.SelectedIndex);
			}
			Mat mat3 = mat.Clone();
			mat3.SetTo(0.0);
			new Mat(mat3, rect).SetTo(255.0);
			if (ts_Tracker2ON.IsChecked.Value)
			{
				new Mat(mat3, rect2).SetTo(255.0);
			}
			Cv2.BitwiseAnd(mat, mat3, mat);
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			Mat mat4 = new Mat();
			Cv2.Merge(ThdShow, mat4);
			if (DrawAll)
			{
				_ = new Point2f[4];
				Cv2.FindContours(mat, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
				Cv2.DrawContours(ShowBitmap, contours, -1, new Scalar(255.0, 0.0, 255.0), 2);
			}
			else
			{
				mat4.CopyTo(ShowBitmap, mat);
			}
			ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush2 = new ImageBrush();
			imageBrush2.ImageSource = imageSource2;
			cnv_Image.Background = imageBrush2;
			imageSource2 = null;
			THD_Preview = true;
		}
		catch
		{
			string text = "BAW04-BlockAvgWindow-DoThdImage()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void DoMorph(ref Mat mat, int index, int count)
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
		Label_ResultError.Visibility = Visibility.Hidden;
		WriteUIValueToParam();
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
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
			btn_ResetA.Visibility = Visibility.Hidden;
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
			bor_SearchRect1.Uid = "0";
			if (bor_SearchRect2.Uid == "0")
			{
				btn_Test.IsEnabled = true;
				DontSave = false;
				Label_ResultError.Visibility = Visibility.Hidden;
			}
		}
		else
		{
			bor_SearchRect2.SetValue(Canvas.LeftProperty, 100.0);
			bor_SearchRect2.SetValue(Canvas.TopProperty, 0.0);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, 100.0);
			btn_ResetB.Visibility = Visibility.Hidden;
			if (TrackerShow)
			{
				bor_SearchRect2.Visibility = Visibility.Visible;
			}
			bor_SearchRect2.Uid = "0";
			if (bor_SearchRect1.Uid == "0")
			{
				btn_Test.IsEnabled = true;
				DontSave = false;
				Label_ResultError.Visibility = Visibility.Hidden;
			}
		}
	}

	private void Cnv_Image_PreviewMouseMove(object sender, MouseEventArgs e)
	{
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
		if ((User >= User_Permissions.USER_Administrator || !inspection.paramFile.BlockAvgParam[TrackIndex].Locked) && !PreviewMode)
		{
			if (ts_TrackerON.IsChecked.Value)
			{
				inspection.paramFile.BlockAvgParam[TrackIndex].TrackerON = true;
				CopyValueToUI();
				return;
			}
			inspection.paramFile.BlockAvgParam[TrackIndex].TrackerON = false;
			inspection.paramFile.BlockAvgParam[TrackIndex].ValueChangeFlag = true;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_SearchRect2.Visibility = Visibility.Hidden;
			gb_BasicSetup.Visibility = Visibility.Hidden;
			gb_ConditionSetup.Visibility = Visibility.Hidden;
			exp_ColorLimit.Visibility = Visibility.Hidden;
			exp_TrackerSetup.Visibility = Visibility.Hidden;
			ToolsButton[TrackIndex].Background = Brushes.White;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Ts_Tracker2ON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_Tracker2ON.IsChecked.Value)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
			txt_Tracker1Pos.Visibility = Visibility.Visible;
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
				cnv_Image.Background = Brushes.Black;
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BlockAvgParam[TrackIndex].TrackerName) + "BT01-BlockAvgWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
				cnv_Image.Background = Brushes.Black;
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
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BlockAvgParam[TrackIndex].TrackerName) + "BT01-BlockAvgWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Btn_Tools1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		cb_TrackerSelect.SelectedIndex = num;
		for (int i = 0; i < 20; i++)
		{
			ToolsButton[i].BorderBrush = Brushes.Gray;
		}
		ToolsButton[num].BorderBrush = Brushes.Blue;
	}

	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		CopyParamSeted = true;
		CopyParam = inspection.paramFile.BlockAvgParam[TrackIndex];
		btn_RecipeCopy.Background = Brushes.PaleGreen;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if ((User >= User_Permissions.USER_Administrator || !inspection.paramFile.BlockAvgParam[TrackIndex].Locked) && CopyParamSeted)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在の検査領域の数値は上書きされます。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes && messageBoxResult == MessageBoxResult.Yes)
			{
				string trackerName = inspection.paramFile.BlockAvgParam[TrackIndex].TrackerName;
				inspection.paramFile.BlockAvgParam[TrackIndex] = CopyParam;
				inspection.paramFile.BlockAvgParam[TrackIndex].TrackerName = trackerName;
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
			inspection.paramFile.BlockAvgParam[TrackIndex] = inspection.paramFile_Backup.BlockAvgParam[TrackIndex];
			CopyValueToUI();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_MaskSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited || !UpdateUI || cb_MaskSelect.SelectedIndex <= 0)
		{
			return;
		}
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
		else
		{
			MaskRect[0] = new OpenCvSharp.Rect(0, 0, 0, 0);
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

	private bool CheckTextBox()
	{
		bool result = true;
		if (!IsNumeric(txt_InspRule1Min.Text))
		{
			txt_InspRule1Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule1Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule2Min.Text))
		{
			txt_InspRule2Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule2Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule3Min.Text))
		{
			txt_InspRule3Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule3Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule4Min.Text))
		{
			txt_InspRule4Min.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule4Min.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule1Max.Text))
		{
			txt_InspRule1Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule1Max.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule2Max.Text))
		{
			txt_InspRule2Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule2Max.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule3Max.Text))
		{
			txt_InspRule3Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule3Max.Background = Brushes.White;
		}
		if (!IsNumeric(txt_InspRule4Max.Text))
		{
			txt_InspRule4Max.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_InspRule4Max.Background = Brushes.White;
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
		if (!IsNumeric(txt_AvgGrayMax.Text))
		{
			txt_AvgGrayMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			int num = Convert.ToInt32(txt_AvgGrayMax.Text);
			if (num < 0 || num > 255)
			{
				num = 255;
			}
			txt_AvgGrayMax.Background = Brushes.White;
		}
		if (!IsNumeric(txt_AvgGrayMin.Text))
		{
			txt_AvgGrayMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			int num2 = Convert.ToInt32(txt_AvgGrayMin.Text);
			if (num2 < 0 || num2 >= 255)
			{
				num2 = 0;
			}
			txt_AvgGrayMin.Background = Brushes.White;
		}
		return result;
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
			cb_TrackerSelect.SelectedIndex = 19;
		}
		else
		{
			cb_TrackerSelect.SelectedIndex--;
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
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
		if (ts_TrackerON.IsChecked == true)
		{
			WriteUIValueToParam();
		}
		if (EnableModifyLog)
		{
			WriteModifyLog(TrackIndex);
		}
		UpdateUI = false;
		if (TrackIndex == 19)
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

	private void Nu_ThdLimitMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_ThdLimit.LowerValue = (int)nu_ThdLimitMin.Value.Value;
	}

	private void Nu_ThdLimitMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		rs_ThdLimit.UpperValue = (int)nu_ThdLimitMax.Value.Value;
	}

	private void Nu_ThdUp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Chk_ThdUpON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Cb_ChannelIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Cb_FilterNoise_Click(object sender, RoutedEventArgs e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
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

	private void Chk_UseMaskAsInspArea_Click(object sender, RoutedEventArgs e)
	{
		if (cb_MaskSelect.SelectedIndex <= 0)
		{
			chk_UseMaskAsInspArea.IsChecked = false;
		}
		else if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
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
		if (cb_MaskSelect.SelectedIndex > 0)
		{
			rt_Mask1.Visibility = Visibility.Visible;
			if (inspection.paramFile.MaskParam[cb_MaskSelect.SelectedIndex - 1].SearchRect2ON)
			{
				rt_Mask2.Visibility = Visibility.Visible;
			}
		}
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

	private void Rad_Tracker1AlignSelect_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track1AlignWayX_SelectionChanged(this, null);
	}

	private void Rad_Tracker1AutoSize_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track1AutoSize_SelectionChanged(this, null);
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

	private void Rad_Tracker2AutoSize_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track2AutoSize_SelectionChanged(this, null);
	}

	private void Rad_Tracker2AlignSelect_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track2AlignWayX_SelectionChanged(this, null);
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
			TrackerShow = false;
			bor_LocateRect.Visibility = Visibility.Hidden;
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

	private void Bor_SearchRect2_GotFocus(object sender, RoutedEventArgs e)
	{
		CurrentTracker = 1;
	}

	private void Bor_SearchRect1_GotFocus(object sender, RoutedEventArgs e)
	{
		CurrentTracker = 0;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueSetReference(INSP_DIFFERENCETOOL_PARAM param, OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2)
	{
		try
		{
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
					Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
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
					Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
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
					SetReferenceRect(rt_ReferenceRect2, inspection.paramFile.AdvLocateParam[param.AutoSize2_SelectIndex].InsideRect);
					Line line3 = line_X2;
					Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
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
					SetReferenceRect(rt_ReferenceRect2, inspection.paramFile.AdvLocateParam[0].InsideRect);
					Line line4 = line_X2;
					Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
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
					rt_ReferenceRect2.Visibility = Visibility.Hidden;
				}
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
					rt_ReferenceRect2.Visibility = Visibility.Hidden;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "BW07-BlockAvgWindow-CopyValueSetReference()-01-";
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
			string text2 = "BW07-BlockAvgWindow-CopyValueSetReference()-02-";
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
			if (param.MaskSelectIndex > 0)
			{
				chk_UseMaskAsInspArea.IsChecked = param.UseMaskAsInspArea;
			}
			else
			{
				chk_UseMaskAsInspArea.IsChecked = false;
			}
			if (param.MaskSelectIndex > 0)
			{
				if (!inspection.paramFile.MaskToolON || !inspection.paramFile.MaskParam[param.MaskSelectIndex - 1].TrackerON)
				{
					SetResult(2, LanguageManager.Instance["MaskLinkError"]);
					rt_Mask1.Visibility = Visibility.Hidden;
					rt_Mask2.Visibility = Visibility.Hidden;
					cb_MaskSelect.SelectedIndex = 0;
					chk_UseMaskAsInspArea.IsChecked = false;
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
			string text3 = "BW07-BlockAvgWindow-CopyValueSetReference()-03-";
			text3 += ex3.ToString();
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	private void Txt_InspRule1Min_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

	private void Txt_BlobLineNum_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
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

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
		}
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

	private void Bor_SearchRect1_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
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

	private void Cb_UseAvgBinary_Click(object sender, RoutedEventArgs e)
	{
		if (cb_UseAvgBinary.IsChecked == true)
		{
			rad_AvgBinaryA.IsEnabled = true;
			rad_AvgBinaryH.IsEnabled = true;
			rad_AvgBinaryV.IsEnabled = true;
			nu_P2PIntervalX.IsEnabled = false;
			nu_P2PIntervalY.IsEnabled = false;
		}
		else
		{
			rad_AvgBinaryA.IsEnabled = false;
			rad_AvgBinaryH.IsEnabled = false;
			rad_AvgBinaryV.IsEnabled = false;
			nu_P2PIntervalX.IsEnabled = true;
			nu_P2PIntervalY.IsEnabled = true;
		}
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Cb_T1Filllet0_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			DoThdImage(DrawAll: true);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		UpdateUI = false;
		DontSave = false;
		btn_Test.IsEnabled = true;
		txt_AllResult.Clear();
		rt_Result.Fill = Brushes.White;
		Label_ResultError.Visibility = Visibility.Hidden;
		btn_ResetA.Visibility = Visibility.Hidden;
		btn_ResetB.Visibility = Visibility.Hidden;
		bor_SearchRect1.Uid = "0";
		bor_SearchRect2.Uid = "0";
		Label_ResultError.Visibility = Visibility.Hidden;
		if (PreviewMode)
		{
			btn_Test.Visibility = Visibility.Hidden;
			btn_TestNextImg.Visibility = Visibility.Hidden;
			btn_TestPrevImg.Visibility = Visibility.Hidden;
			btn_MaskPreView.Visibility = Visibility.Hidden;
			btn_MaskEdit.Visibility = Visibility.Hidden;
		}
		BackupParam = inspection.paramFile.BlockAvgParam[TrackIndex];
		INSP_DIFFERENCETOOL_PARAM param = inspection.paramFile.BlockAvgParam[TrackIndex];
		ts_TrackerON.IsChecked = param.TrackerON;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		Label_Index.Content = (TrackIndex + 1).ToString();
		if (param.Locked && User < User_Permissions.USER_Administrator)
		{
			lab_LockMode.Visibility = Visibility.Visible;
		}
		else
		{
			lab_LockMode.Visibility = Visibility.Hidden;
		}
		if (param.TrackerON)
		{
			gb_BasicSetup.Visibility = Visibility.Visible;
			gb_ConditionSetup.Visibility = Visibility.Visible;
			exp_ColorLimit.Visibility = Visibility.Visible;
			exp_TrackerSetup.Visibility = Visibility.Visible;
		}
		else
		{
			gb_BasicSetup.Visibility = Visibility.Hidden;
			gb_ConditionSetup.Visibility = Visibility.Hidden;
			exp_ColorLimit.Visibility = Visibility.Hidden;
			exp_TrackerSetup.Visibility = Visibility.Hidden;
		}
		cb_FilterNoise.IsChecked = param.FilterNoise;
		nu_NoiseWidth.Value = param.NoiseWidth;
		nu_NoiseHeight.Value = param.NoiseHeight;
		SetConditionText(param.SearchRect2ON);
		cb_TrackerSelect.SelectedIndex = TrackIndex;
		txt_TrackerName.Text = param.TrackerName;
		cb_ChannelIndex.SelectedIndex = (int)param.ChannelIndex;
		nu_P2PIntervalX.Value = param.P2PIntervalX;
		nu_P2PIntervalY.Value = param.P2PIntervalY;
		chk_ThdUpON.IsChecked = param.ThdUpON;
		nu_ThdUp.Value = param.ThdUp;
		chk_ThdDownON.IsChecked = param.ThdDownON;
		nu_ThdDown.Value = param.ThdDown;
		rs_ThdLimit.LowerValue = param.THDLimit_Min;
		rs_ThdLimit.UpperValue = param.THDLimit_Max;
		nu_ThdLimitMin.Value = param.THDLimit_Min;
		nu_ThdLimitMax.Value = param.THDLimit_Max;
		cb_ThdDEStart.SelectedIndex = (int)param.DE_Start_Index;
		cb_ThdDEEnd.SelectedIndex = (int)param.DE_End_Index;
		cb_ThdDEStartCount.SelectedIndex = param.DE_Start_Count;
		cb_ThdDEEndCount.SelectedIndex = param.DE_End_Count;
		chk_TrackErrorSetNG.IsChecked = param.TrackErrorSetNG;
		if (param.SearchRect2ON)
		{
			cb_InspRule1.SelectedIndex = (int)param.InspRule1_Index;
			cb_InspRule2.SelectedIndex = (int)param.InspRule2_Index;
			cb_InspRule3.SelectedIndex = (int)param.InspRule3_Index;
			cb_InspRule4.SelectedIndex = (int)param.InspRule4_Index;
		}
		else
		{
			cb_InspRule1.SelectedIndex = (int)param.InspRule1_Index % tracker2DiableRuleCount;
			cb_InspRule2.SelectedIndex = (int)param.InspRule2_Index % tracker2DiableRuleCount;
			cb_InspRule3.SelectedIndex = (int)param.InspRule3_Index % tracker2DiableRuleCount;
			cb_InspRule4.SelectedIndex = (int)param.InspRule4_Index % tracker2DiableRuleCount;
		}
		grd_AvgThdFun.Visibility = Visibility.Hidden;
		if (param.AvgBinaryMethod == 0)
		{
			rad_AvgBinaryA.IsChecked = true;
			grd_AvgThdFun.Visibility = Visibility.Visible;
		}
		else if (param.AvgBinaryMethod == 1)
		{
			rad_AvgBinaryH.IsChecked = true;
		}
		else
		{
			rad_AvgBinaryV.IsChecked = true;
		}
		txt_AvgGrayMax.Text = param.AvgGrayMax.ToString();
		txt_AvgGrayMin.Text = param.AvgGrayMin.ToString();
		if (param.UseAvgBinary)
		{
			cb_UseAvgBinary.IsChecked = true;
			rad_AvgBinaryA.IsEnabled = true;
			rad_AvgBinaryH.IsEnabled = true;
			rad_AvgBinaryV.IsEnabled = true;
			nu_P2PIntervalX.IsEnabled = false;
			nu_P2PIntervalY.IsEnabled = false;
		}
		else
		{
			cb_UseAvgBinary.IsChecked = false;
			rad_AvgBinaryA.IsEnabled = false;
			rad_AvgBinaryH.IsEnabled = false;
			rad_AvgBinaryV.IsEnabled = false;
			nu_P2PIntervalX.IsEnabled = true;
			nu_P2PIntervalY.IsEnabled = true;
		}
		if (param.InspRuleAND)
		{
			rad_RuleAND.IsChecked = true;
		}
		else
		{
			rad_RuleOR.IsChecked = true;
		}
		txt_InspRule1Min.Text = param.InspRule1_Min.ToString();
		txt_InspRule2Min.Text = param.InspRule2_Min.ToString();
		txt_InspRule3Min.Text = param.InspRule3_Min.ToString();
		txt_InspRule4Min.Text = param.InspRule4_Min.ToString();
		txt_InspRule1Max.Text = param.InspRule1_Max.ToString();
		txt_InspRule2Max.Text = param.InspRule2_Max.ToString();
		txt_InspRule3Max.Text = param.InspRule3_Max.ToString();
		txt_InspRule4Max.Text = param.InspRule4_Max.ToString();
		cb_MB_InspRule1.SelectedIndex = (int)param.MB_InspRule1_Index;
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
		cb_ErrorBin.SelectedIndex = (int)param.ErrorBin;
		cb_ParetoChart.SelectedIndex = param.ParetoIndex;
		cb_Track1AlignWayX.SelectedIndex = param.Tracker1AligmentXIndex;
		cb_Track2AlignWayX.SelectedIndex = param.Tracker2AligmentXIndex;
		cb_Track1AlignWayY.SelectedIndex = param.Tracker1AligmentYIndex;
		cb_Track2AlignWayY.SelectedIndex = param.Tracker2AligmentYIndex;
		chk_SelectFace.IsChecked = param.EnableSelectFace;
		SetSelectFace(param.FaceIndex);
		cb_MatchMaskSelect1.SelectedIndex = param.UseMatchTextMask1;
		cb_MatchMaskSelect2.SelectedIndex = param.UseMatchTextMask2;
		cb_MatchMaskSelect3.SelectedIndex = param.UseMatchTextMask3;
		cb_MatchText_DEStart1.SelectedIndex = (int)param.MatchTextDE_Start1_Index;
		cb_MatchText_DEStart2.SelectedIndex = (int)param.MatchTextDE_Start2_Index;
		cb_MatchText_DEStart3.SelectedIndex = (int)param.MatchTextDE_Start3_Index;
		cb_MatchText_DEStartCount1.SelectedIndex = param.MatchTextDE_Start1_Count;
		cb_MatchText_DEStartCount2.SelectedIndex = param.MatchTextDE_Start2_Count;
		cb_MatchText_DEStartCount3.SelectedIndex = param.MatchTextDE_Start3_Count;
		cb_Track1AutoSize.SelectedIndex = param.AutoSize1_SelectIndex;
		cb_Track2AutoSize.SelectedIndex = param.AutoSize2_SelectIndex;
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
		if (!PreviewMode)
		{
			CopyValueSetReference(param, rect, rect2);
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
	private void WriteUIValueToParam()
	{
		if (PreviewMode || (User < User_Permissions.USER_Administrator && inspection.paramFile.BlockAvgParam[TrackIndex].Locked))
		{
			return;
		}
		try
		{
			if (DontSave)
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
			INSP_DIFFERENCETOOL_PARAM iNSP_DIFFERENCETOOL_PARAM = inspection.paramFile.BlockAvgParam[TrackIndex];
			iNSP_DIFFERENCETOOL_PARAM.ValueChangeFlag = true;
			iNSP_DIFFERENCETOOL_PARAM.TrackerON = ts_TrackerON.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.TrackerName = txt_TrackerName.Text;
			iNSP_DIFFERENCETOOL_PARAM.MaskSelectIndex = cb_MaskSelect.SelectedIndex;
			if (iNSP_DIFFERENCETOOL_PARAM.MaskSelectIndex > 0)
			{
				iNSP_DIFFERENCETOOL_PARAM.UseMaskAsInspArea = chk_UseMaskAsInspArea.IsChecked.Value;
			}
			else
			{
				iNSP_DIFFERENCETOOL_PARAM.UseMaskAsInspArea = false;
			}
			iNSP_DIFFERENCETOOL_PARAM.FilterNoise = cb_FilterNoise.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.NoiseWidth = (int)nu_NoiseWidth.Value.Value;
			iNSP_DIFFERENCETOOL_PARAM.NoiseHeight = (int)nu_NoiseHeight.Value.Value;
			iNSP_DIFFERENCETOOL_PARAM.ChannelIndex = (COLOR_CHANNEL)cb_ChannelIndex.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.P2PIntervalX = (int)nu_P2PIntervalX.Value.Value;
			iNSP_DIFFERENCETOOL_PARAM.P2PIntervalY = (int)nu_P2PIntervalY.Value.Value;
			iNSP_DIFFERENCETOOL_PARAM.ThdUpON = chk_ThdUpON.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.ThdUp = (int)nu_ThdUp.Value.Value;
			iNSP_DIFFERENCETOOL_PARAM.ThdDownON = chk_ThdDownON.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.ThdDown = (int)nu_ThdDown.Value.Value;
			iNSP_DIFFERENCETOOL_PARAM.THDLimit_Min = (int)rs_ThdLimit.LowerValue;
			iNSP_DIFFERENCETOOL_PARAM.THDLimit_Max = (int)rs_ThdLimit.UpperValue;
			iNSP_DIFFERENCETOOL_PARAM.DE_Start_Index = (USE_DILATE_ERODE)cb_ThdDEStart.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.DE_End_Index = (USE_DILATE_ERODE)cb_ThdDEEnd.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.DE_Start_Count = cb_ThdDEStartCount.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.DE_End_Count = cb_ThdDEEndCount.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.UseAvgBinary = cb_UseAvgBinary.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.TrackErrorSetNG = chk_TrackErrorSetNG.IsChecked.Value;
			if (cb_AvgShift.SelectedIndex < 0)
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgShiftValue = 9;
			}
			else
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgShiftValue = cb_AvgShift.SelectedIndex;
			}
			iNSP_DIFFERENCETOOL_PARAM.AvgGrayMax = Convert.ToInt32(txt_AvgGrayMax.Text);
			iNSP_DIFFERENCETOOL_PARAM.AvgGrayMin = Convert.ToInt32(txt_AvgGrayMin.Text);
			if (rad_AvgBinaryA.IsChecked == true)
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgBinaryMethod = 0;
			}
			else if (rad_AvgBinaryH.IsChecked == true)
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgBinaryMethod = 1;
			}
			else if (rad_AvgBinaryV.IsChecked == true)
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgBinaryMethod = 2;
			}
			if (cb_AvgShift.SelectedIndex < 0)
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgShiftValue = 9;
			}
			else
			{
				iNSP_DIFFERENCETOOL_PARAM.AvgShiftValue = cb_AvgShift.SelectedIndex;
			}
			iNSP_DIFFERENCETOOL_PARAM.InspRuleAND = rad_RuleAND.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.InspRule1_Index = (BlobCondition)cb_InspRule1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.InspRule2_Index = (BlobCondition)cb_InspRule2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.InspRule3_Index = (BlobCondition)cb_InspRule3.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.InspRule4_Index = (BlobCondition)cb_InspRule4.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.InspRule1_Min = Convert.ToDouble(txt_InspRule1Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule2_Min = Convert.ToDouble(txt_InspRule2Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule3_Min = Convert.ToDouble(txt_InspRule3Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule4_Min = Convert.ToDouble(txt_InspRule4Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule1_Max = Convert.ToDouble(txt_InspRule1Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule2_Max = Convert.ToDouble(txt_InspRule2Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule3_Max = Convert.ToDouble(txt_InspRule3Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.InspRule4_Max = Convert.ToDouble(txt_InspRule4Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule1_Index = (BlobCondition)cb_MB_InspRule1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule2_Index = (BlobCondition)cb_MB_InspRule2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule3_Index = (BlobCondition)cb_MB_InspRule3.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule4_Index = (BlobCondition)cb_MB_InspRule4.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule1_Min = Convert.ToDouble(txt_MB_InspRule1Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule2_Min = Convert.ToDouble(txt_MB_InspRule2Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule3_Min = Convert.ToDouble(txt_MB_InspRule3Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule4_Min = Convert.ToDouble(txt_MB_InspRule4Min.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule1_Max = Convert.ToDouble(txt_MB_InspRule1Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule2_Max = Convert.ToDouble(txt_MB_InspRule2Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule3_Max = Convert.ToDouble(txt_MB_InspRule3Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRule4_Max = Convert.ToDouble(txt_MB_InspRule4Max.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_InspRuleAND = rad_MB_RuleAND.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.MB_BlobSort = cb_MB_BlobSort.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MB_BlobNum = Convert.ToInt32(txt_MB_BlobNum.Text);
			iNSP_DIFFERENCETOOL_PARAM.MB_AllowNGCount = Convert.ToInt32(txt_MB_NGCount.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobAnalysisSelect = tab_BlobAnalysis.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.BlobLineSort = cb_BlobLineSort.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.BlobLineNum = Convert.ToInt32(txt_BlobLineNum.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineMin = Convert.ToInt32(txt_BlobLineMin.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineMax = Convert.ToInt32(txt_BlobLineMax.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineWidthMin = Convert.ToInt32(txt_BlobLineWidthMin.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineWidthMax = Convert.ToInt32(txt_BlobLineWidthMax.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineHeightMin = Convert.ToInt32(txt_BlobLineHeightMin.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineHeightMax = Convert.ToInt32(txt_BlobLineHeightMax.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineAreaMin = Convert.ToInt32(txt_BlobLineAreaMin.Text);
			iNSP_DIFFERENCETOOL_PARAM.BlobLineAreaMax = Convert.ToInt32(txt_BlobLineAreaMax.Text);
			iNSP_DIFFERENCETOOL_PARAM.EnableSelectFace = chk_SelectFace.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.FaceIndex = SelectFace;
			iNSP_DIFFERENCETOOL_PARAM.UseMatchTextMask1 = cb_MatchMaskSelect1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.UseMatchTextMask2 = cb_MatchMaskSelect2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.UseMatchTextMask3 = cb_MatchMaskSelect3.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MatchTextDE_Start1_Index = (USE_DILATE_ERODE)cb_MatchText_DEStart1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MatchTextDE_Start2_Index = (USE_DILATE_ERODE)cb_MatchText_DEStart2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MatchTextDE_Start3_Index = (USE_DILATE_ERODE)cb_MatchText_DEStart3.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MatchTextDE_Start1_Count = cb_MatchText_DEStartCount1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MatchTextDE_Start2_Count = cb_MatchText_DEStartCount2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.MatchTextDE_Start3_Count = cb_MatchText_DEStartCount3.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T1FillletCount0 = cb_T1Filllet0.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T1FillletCount1 = cb_T1Filllet1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T1FillletCount2 = cb_T1Filllet2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T1FillletCount3 = cb_T1Filllet3.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T2FillletCount0 = cb_T2Filllet0.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T2FillletCount1 = cb_T2Filllet1.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T2FillletCount2 = cb_T2Filllet2.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.T2FillletCount3 = cb_T2Filllet3.SelectedIndex;
			if (iNSP_DIFFERENCETOOL_PARAM.T1FillletCount0 > 0 || iNSP_DIFFERENCETOOL_PARAM.T1FillletCount1 > 0 || iNSP_DIFFERENCETOOL_PARAM.T1FillletCount2 > 0 || iNSP_DIFFERENCETOOL_PARAM.T1FillletCount3 > 0 || iNSP_DIFFERENCETOOL_PARAM.T2FillletCount0 > 0 || iNSP_DIFFERENCETOOL_PARAM.T2FillletCount1 > 0 || iNSP_DIFFERENCETOOL_PARAM.T2FillletCount2 > 0 || iNSP_DIFFERENCETOOL_PARAM.T2FillletCount3 > 0)
			{
				btn_Fillet.Background = Brushes.Gold;
			}
			else
			{
				btn_Fillet.Background = Brushes.White;
			}
			iNSP_DIFFERENCETOOL_PARAM.ErrorBin = (FLOW_BIN)cb_ErrorBin.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
			bool flag = false;
			bool flag2 = false;
			if (rad_Tracker1AutoSize.IsChecked.Value != iNSP_DIFFERENCETOOL_PARAM.Tracker1UseAutoSize)
			{
				flag = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentXIndex != cb_Track1AlignWayX.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentXIndex != cb_Track2AlignWayX.SelectedIndex)
			{
				flag2 = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentYIndex != cb_Track1AlignWayY.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentYIndex != cb_Track2AlignWayY.SelectedIndex)
			{
				flag2 = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex != cb_Track1AutoSize.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex != cb_Track2AutoSize.SelectedIndex)
			{
				flag2 = true;
			}
			iNSP_DIFFERENCETOOL_PARAM.Tracker1UseAutoSize = rad_Tracker1AutoSize.IsChecked.Value;
			iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentXIndex = cb_Track1AlignWayX.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentXIndex = cb_Track2AlignWayX.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentYIndex = cb_Track1AlignWayY.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentYIndex = cb_Track2AlignWayY.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex = cb_Track1AutoSize.SelectedIndex;
			iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex = cb_Track2AutoSize.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (!GetTracker(ref rect, 0))
			{
				return;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.SearchRect2ON != ts_Tracker2ON.IsChecked)
			{
				flag2 = true;
			}
			if (iNSP_DIFFERENCETOOL_PARAM.Tracker2UseAutoSize != rad_Tracker2AutoSize.IsChecked.Value)
			{
				flag2 = true;
			}
			iNSP_DIFFERENCETOOL_PARAM.SearchRect2ON = ts_Tracker2ON.IsChecked.Value;
			if (iNSP_DIFFERENCETOOL_PARAM.SearchRect2ON && !GetTracker(ref rect2, 1))
			{
				return;
			}
			iNSP_DIFFERENCETOOL_PARAM.Tracker2UseAutoSize = rad_Tracker2AutoSize.IsChecked.Value;
			if ((LastSearchRect1 != rect) | flag)
			{
				if (rad_Tracker1AutoSize.IsChecked.Value)
				{
					if (inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex].TrackerON)
					{
						iNSP_DIFFERENCETOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex].L;
						iNSP_DIFFERENCETOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex].T;
						iNSP_DIFFERENCETOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex].Width - iNSP_DIFFERENCETOOL_PARAM.Track1_L - rect.Width;
						iNSP_DIFFERENCETOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex].Height - iNSP_DIFFERENCETOOL_PARAM.Track1_T - rect.Height;
					}
					else
					{
						iNSP_DIFFERENCETOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[0].L;
						iNSP_DIFFERENCETOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[0].T;
						iNSP_DIFFERENCETOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[0].Width - iNSP_DIFFERENCETOOL_PARAM.Track1_L - rect.Width;
						iNSP_DIFFERENCETOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[0].Height - iNSP_DIFFERENCETOOL_PARAM.Track1_T - rect.Height;
						int autoSize1_SelectIndex = (cb_Track1AutoSize.SelectedIndex = 0);
						iNSP_DIFFERENCETOOL_PARAM.AutoSize1_SelectIndex = autoSize1_SelectIndex;
					}
				}
				else if (inspection.paramFile.VSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentYIndex].TrackerON)
				{
					iNSP_DIFFERENCETOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentXIndex].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track1_R = iNSP_DIFFERENCETOOL_PARAM.Track1_L + rect.Width;
					iNSP_DIFFERENCETOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentYIndex].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track1_B = iNSP_DIFFERENCETOOL_PARAM.Track1_T + rect.Height;
				}
				else
				{
					iNSP_DIFFERENCETOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[0].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track1_R = iNSP_DIFFERENCETOOL_PARAM.Track1_L + rect.Width;
					iNSP_DIFFERENCETOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[0].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track1_B = iNSP_DIFFERENCETOOL_PARAM.Track1_T + rect.Height;
					iNSP_DIFFERENCETOOL_PARAM.Tracker1UseAutoSize = false;
					int autoSize1_SelectIndex = (cb_Track1AlignWayX.SelectedIndex = 0);
					iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentXIndex = autoSize1_SelectIndex;
					autoSize1_SelectIndex = (cb_Track2AlignWayX.SelectedIndex = 0);
					iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentXIndex = autoSize1_SelectIndex;
				}
			}
			if ((LastSearchRect2 != rect2) | flag | flag2)
			{
				if (rad_Tracker2AutoSize.IsChecked.Value)
				{
					if (inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex].TrackerON)
					{
						iNSP_DIFFERENCETOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex].L;
						iNSP_DIFFERENCETOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex].T;
						iNSP_DIFFERENCETOOL_PARAM.Track2_R = inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex].Width - iNSP_DIFFERENCETOOL_PARAM.Track2_L - rect2.Width;
						iNSP_DIFFERENCETOOL_PARAM.Track2_B = inspection.paramFile.AdvLocateParam[iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex].Height - iNSP_DIFFERENCETOOL_PARAM.Track2_T - rect2.Height;
					}
					else
					{
						iNSP_DIFFERENCETOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.AdvLocateParam[0].L;
						iNSP_DIFFERENCETOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.AdvLocateParam[0].T;
						iNSP_DIFFERENCETOOL_PARAM.Track2_R = inspection.paramFile.AdvLocateParam[0].Width - iNSP_DIFFERENCETOOL_PARAM.Track2_L - rect2.Width;
						iNSP_DIFFERENCETOOL_PARAM.Track2_B = inspection.paramFile.AdvLocateParam[0].Height - iNSP_DIFFERENCETOOL_PARAM.Track2_T - rect2.Height;
						int autoSize1_SelectIndex = (cb_Track2AutoSize.SelectedIndex = 0);
						iNSP_DIFFERENCETOOL_PARAM.AutoSize2_SelectIndex = autoSize1_SelectIndex;
					}
				}
				else if (inspection.paramFile.VSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentYIndex].TrackerON)
				{
					iNSP_DIFFERENCETOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.VSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentXIndex].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track2_R = iNSP_DIFFERENCETOOL_PARAM.Track2_L + rect2.Width;
					iNSP_DIFFERENCETOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.HSLocateParam[iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentYIndex].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track2_B = iNSP_DIFFERENCETOOL_PARAM.Track2_T + rect2.Height;
				}
				else
				{
					iNSP_DIFFERENCETOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.VSLocateParam[0].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track2_R = iNSP_DIFFERENCETOOL_PARAM.Track2_L + rect2.Width;
					iNSP_DIFFERENCETOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.HSLocateParam[0].Value;
					iNSP_DIFFERENCETOOL_PARAM.Track2_B = iNSP_DIFFERENCETOOL_PARAM.Track2_T + rect2.Height;
					int autoSize1_SelectIndex = (cb_Track1AlignWayY.SelectedIndex = 0);
					iNSP_DIFFERENCETOOL_PARAM.Tracker1AligmentYIndex = autoSize1_SelectIndex;
					autoSize1_SelectIndex = (cb_Track2AlignWayY.SelectedIndex = 0);
					iNSP_DIFFERENCETOOL_PARAM.Tracker2AligmentYIndex = autoSize1_SelectIndex;
				}
			}
			iNSP_DIFFERENCETOOL_PARAM.ValueChangeFlag = false;
			if (!iNSP_DIFFERENCETOOL_PARAM.Equals(inspection.paramFile_Backup.BlockAvgParam[TrackIndex]))
			{
				iNSP_DIFFERENCETOOL_PARAM.ValueChangeFlag = true;
				inspection.paramFile.BlockAvgParam[TrackIndex] = iNSP_DIFFERENCETOOL_PARAM;
			}
			UpdateUI = false;
			int selectedIndex = cb_TrackerSelect.SelectedIndex;
			cb_TrackerSelect.Items.Clear();
			for (int i = 0; i < 20; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.BlockAvgParam[i].TrackerName);
			}
			cb_TrackerSelect.SelectedIndex = selectedIndex;
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "BAW07-BlockAvgWindow-WriteUIValueToParam()";
			text += ex.ToString();
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
		if (!inited || !UpdateUI || PreviewMode)
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
			if (!inspection.buffers.LocatedReady || DontSave)
			{
				return;
			}
			TextBox textBox = txt_InspRule1Min;
			TextBox textBox2 = txt_InspRule2Min;
			TextBox textBox3 = txt_InspRule3Min;
			Brush brush = (txt_InspRule4Min.Background = Brushes.White);
			Brush brush2 = (textBox3.Background = brush);
			Brush background = (textBox2.Background = brush2);
			textBox.Background = background;
			TextBox textBox4 = txt_InspRule1Max;
			TextBox textBox5 = txt_InspRule2Max;
			TextBox textBox6 = txt_InspRule3Max;
			brush = (txt_InspRule4Max.Background = Brushes.White);
			brush2 = (textBox6.Background = brush);
			background = (textBox5.Background = brush2);
			textBox4.Background = background;
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
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			TimeStart();
			bool result = inspection.blockavgTool[TrackIndex].Inspect(TrackIndex, ref iresult);
			double num = TimeStop();
			lab_Time.Content = string.Format("{0:N3}", num);
			SetResult(result);
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			if (rad_AvgBinaryA.IsChecked == true && cb_UseAvgBinary.IsChecked == true)
			{
				int num2 = inspection.blockavgTool[TrackIndex].AvgGray[0];
				int num3 = inspection.blockavgTool[TrackIndex].AvgGray[1];
				txt_AvgGrayMin.Background = Brushes.White;
				txt_AvgGrayMax.Background = Brushes.White;
				txt_AvgGray.Text = string.Format("A:{0}, B:{1}", num2, num3);
				if (num2 < inspection.paramFile.BlockAvgParam[TrackIndex].AvgGrayMin)
				{
					txt_AvgGrayMin.Background = Brushes.Fuchsia;
				}
				if (ts_Tracker2ON.IsChecked == true && num3 < inspection.paramFile.BlockAvgParam[TrackIndex].AvgGrayMin)
				{
					txt_AvgGrayMin.Background = Brushes.Fuchsia;
				}
				if (num2 > inspection.paramFile.BlockAvgParam[TrackIndex].AvgGrayMax)
				{
					txt_AvgGrayMax.Background = Brushes.Fuchsia;
				}
				if (ts_Tracker2ON.IsChecked == true && num3 > inspection.paramFile.BlockAvgParam[TrackIndex].AvgGrayMax)
				{
					txt_AvgGrayMax.Background = Brushes.Fuchsia;
				}
			}
			DoThdImage();
			txt_AllResult.Clear();
			if (tab_BlobAnalysis.SelectedIndex == 0)
			{
				OpenCvSharp.Rect[] array2 = new OpenCvSharp.Rect[2];
				Blobs[0] = inspection.blockavgTool[TrackIndex].Blobs[0];
				Blobs[1] = inspection.blockavgTool[TrackIndex].Blobs[1];
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
				for (int i = 0; i < 2; i++)
				{
					if (!SearchRectResult[i])
					{
						ShowBitmap.Rectangle(array2[i], Scalar.Red);
					}
					else
					{
						ShowBitmap.Rectangle(array2[i], Scalar.Green);
					}
				}
				txt_AllResult.Text += inspection.blockavgTool[TrackIndex].Tracker1ResultText;
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += inspection.blockavgTool[TrackIndex].Tracker2ResultText;
				}
			}
			else if (tab_BlobAnalysis.SelectedIndex == 1)
			{
				int num4 = Convert.ToInt32(txt_BlobLineMin.Text);
				int num5 = Convert.ToInt32(txt_BlobLineMax.Text);
				int num6 = Convert.ToInt32(txt_BlobLineNum.Text);
				int num7 = 0;
				if (num4 > 0 && num5 > 0 && inspection.blockavgTool[TrackIndex].BlobLine1.Count > 0)
				{
					for (int j = 0; j < inspection.blockavgTool[TrackIndex].BlobLine1.Count && j < num6; j++)
					{
						if (!inspection.blockavgTool[TrackIndex].BlobLine1[j].result)
						{
							if (LanguageType == 0)
							{
								txt_AllResult.Text += string.Format("[A]Line Width{0}, Line Height{1}, Line Area{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
							}
							else if (LanguageType == 1)
							{
								txt_AllResult.Text += string.Format("[A]線條寬度{0}, 線條高度{1}, 線條面積{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
							}
							else if (LanguageType == 2)
							{
								txt_AllResult.Text += string.Format("[A]线条宽度{0}, 线条高度{1}, 线条面积{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
							}
							else if (LanguageType == 3)
							{
								txt_AllResult.Text += string.Format("[A]線條幅:{0}, 線條高:{1}, 線條面積:{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
							}
							ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].BlobLine1[j].NGRect, Scalar.Red);
							continue;
						}
						if (LanguageType == 0)
						{
							txt_AllResult.Text += string.Format("[A]Line Width{0}, Line Height{1}, Line Area{2} \n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
						}
						else if (LanguageType == 1)
						{
							txt_AllResult.Text += string.Format("[A]線條寬度{0}, 線條高度{1}, 線條面積{2}\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
						}
						else if (LanguageType == 2)
						{
							txt_AllResult.Text += string.Format("[A]线条宽度{0}, 线条高度{1}, 线条面积{2}\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
						}
						else if (LanguageType == 3)
						{
							txt_AllResult.Text += string.Format("[A]線條幅:{0}, 線條高:{1}, 線條面積:{2}\n", inspection.blockavgTool[TrackIndex].BlobLine1[j].Width, inspection.blockavgTool[TrackIndex].BlobLine1[j].Height, inspection.blockavgTool[TrackIndex].BlobLine1[j].Area);
						}
						ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].BlobLine1[j].NGRect, Scalar.Green);
						num7++;
					}
					txt_OKLineResultA.Text = num7.ToString();
				}
				if (LanguageType == 1)
				{
					txt_AllResult.Text += string.Format("[A]線條合格數量為{0}，允許最小合格數為{1}，允許最大合格數為{2}\n", num7, num4, num5);
				}
				else if (LanguageType == 2)
				{
					txt_AllResult.Text += string.Format("[A]线条合格数量为{0}，允许最小合格数为{1}，允许最大合格数为{2}\n", num7, num4, num5);
				}
				else if (LanguageType == 3)
				{
					txt_AllResult.Text += string.Format("[A]線の合格数は{0}、許容最小合格数は{1}、許容最大合格数は{2}です。\n", num7, num4, num5);
				}
				else
				{
					txt_AllResult.Text += string.Format("[A]Passed Line={0}，Min Passs Line={1}，Max Pass Line=\n", num7, num4, num5);
				}
				txt_OKLineResultA.Text = num7.ToString();
				txt_BlobLineMin.Background = Brushes.White;
				txt_BlobLineMax.Background = Brushes.White;
				if (num7 < num4)
				{
					txt_BlobLineMin.Background = Brushes.Red;
				}
				if (num7 > num5)
				{
					txt_BlobLineMax.Background = Brushes.Red;
				}
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += "\n";
					num7 = 0;
					if (num4 > 0 && num5 > 0 && inspection.blockavgTool[TrackIndex].BlobLine2.Count > 0)
					{
						for (int k = 0; k < inspection.blockavgTool[TrackIndex].BlobLine2.Count && k < num6; k++)
						{
							if (!inspection.blockavgTool[TrackIndex].BlobLine2[k].result)
							{
								if (LanguageType == 0)
								{
									txt_AllResult.Text += string.Format("[A]Line Width{0}, Line Height{1}, Line Area{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
								}
								else if (LanguageType == 1)
								{
									txt_AllResult.Text += string.Format("[A]線條寬度{0}, 線條高度{1}, 線條面積{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
								}
								else if (LanguageType == 2)
								{
									txt_AllResult.Text += string.Format("[A]线条宽度{0}, 线条高度{1}, 线条面积{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
								}
								else if (LanguageType == 3)
								{
									txt_AllResult.Text += string.Format("[A]線條幅:{0}, 線條高:{1}, 線條面積:{2} [NG]\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
								}
								ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].BlobLine2[k].NGRect, Scalar.Red);
								continue;
							}
							if (LanguageType == 0)
							{
								txt_AllResult.Text += string.Format("[A]Line Width{0}, Line Height{1}, Line Area{2}\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
							}
							else if (LanguageType == 1)
							{
								txt_AllResult.Text += string.Format("[A]線條寬度{0}, 線條高度{1}, 線條面積{2}\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
							}
							else if (LanguageType == 2)
							{
								txt_AllResult.Text += string.Format("[A]线条宽度{0}, 线条高度{1}, 线条面积{2}\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
							}
							else if (LanguageType == 3)
							{
								txt_AllResult.Text += string.Format("[A]線條幅:{0}, 線條高:{1}, 線條面積:{2}\n", inspection.blockavgTool[TrackIndex].BlobLine2[k].Width, inspection.blockavgTool[TrackIndex].BlobLine2[k].Height, inspection.blockavgTool[TrackIndex].BlobLine2[k].Area);
							}
							ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].BlobLine2[k].NGRect, Scalar.Green);
							num7++;
						}
						txt_OKLineResultB.Text = num7.ToString();
					}
					if (LanguageType == 1)
					{
						txt_AllResult.Text += string.Format("[B]線條合格數量為{0}，允許最小合格數為{1}，允許最大合格數為{2}\n", num7, num4, num5);
					}
					else if (LanguageType == 2)
					{
						txt_AllResult.Text += string.Format("[B]线条合格数量为{0}，允许最小合格数为{1}，允许最大合格数为{2}\n", num7, num4, num5);
					}
					else if (LanguageType == 3)
					{
						txt_AllResult.Text += string.Format("[B]線の合格数は{0}、許容最小合格数は{1}、許容最大合格数は{2}です。\n", num7, num4, num5);
					}
					else
					{
						txt_AllResult.Text += string.Format("[B]Passed Line={0}，Min Passs Line={1}，Max Pass Line=\n", num7, num4, num5);
					}
					if (num7 < num4)
					{
						txt_BlobLineMin.Background = Brushes.Red;
					}
					if (num7 > num5)
					{
						txt_BlobLineMax.Background = Brushes.Red;
					}
				}
			}
			else if (tab_BlobAnalysis.SelectedIndex == 2)
			{
				DoThdImage();
				txt_AllResult.Clear();
				for (int l = 0; l < inspection.blockavgTool[TrackIndex].MultiBlob1.Count && l < Convert.ToInt32(txt_MB_BlobNum.Text); l++)
				{
					if (!inspection.blockavgTool[TrackIndex].MultiBlob1[l].result)
					{
						ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].MultiBlob1[l].NGRect, Scalar.Red);
					}
					else
					{
						ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].MultiBlob1[l].NGRect, Scalar.Green);
					}
				}
				for (int m = 0; m < inspection.blockavgTool[TrackIndex].MultiBlob2.Count && m < Convert.ToInt32(txt_MB_BlobNum.Text); m++)
				{
					if (!inspection.blockavgTool[TrackIndex].MultiBlob2[m].result)
					{
						ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].MultiBlob2[m].NGRect, Scalar.Red);
					}
					else
					{
						ShowBitmap.Rectangle(inspection.blockavgTool[TrackIndex].MultiBlob2[m].NGRect, Scalar.Green);
					}
				}
				txt_AllResult.Text += inspection.blockavgTool[TrackIndex].Tracker1ResultText;
				if (ts_Tracker2ON.IsChecked == true)
				{
					txt_AllResult.Text += inspection.blockavgTool[TrackIndex].Tracker2ResultText;
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

	private void Cb_ThdDEStartCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Btn_MaskPreView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
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
		Cv2.BitwiseNot(mat, mat);
		if (flag)
		{
			Mat mat2 = new Mat(ShowBitmap.Height, ShowBitmap.Width, MatType.CV_8UC1);
			mat2.SetTo(0.0);
			mat2.Rectangle(rect, Scalar.White, -1);
			if (ts_Tracker2ON.IsChecked == true)
			{
				mat2.Rectangle(rect2, Scalar.White, -1);
			}
			Cv2.BitwiseAnd(mat2, mat, mat);
			if (chk_UseMaskAsInspArea.IsChecked == true)
			{
				Cv2.BitwiseNot(mat, mat);
			}
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

	private void MainW_ContentRendered(object sender, EventArgs e)
	{
		grd_Main.Visibility = Visibility.Visible;
	}

	private void Btn_TextMaskPreView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
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
		if (!GetTracker(ref rect, 0) || (ts_Tracker2ON.IsChecked == true && !GetTracker(ref rect2, 1)))
		{
			return;
		}
		Mat mat = inspection.buffers.LocateGray.Clone();
		mat.SetTo(0.0);
		if (rect.Width > 0 && rect.Height > 0)
		{
			Mat mat2 = new Mat(mat, rect);
			if (inspection.blockavgTool[TrackIndex].WholeMask[0].Width == mat2.Width && inspection.blockavgTool[TrackIndex].WholeMask[0].Height == mat2.Height)
			{
				Cv2.BitwiseOr(mat2, inspection.blockavgTool[TrackIndex].WholeMask[0], mat2);
			}
		}
		if (rect2.Width > 0 && rect2.Height > 0)
		{
			Mat mat3 = new Mat(mat, rect2);
			if (inspection.blockavgTool[TrackIndex].WholeMask[1].Width == mat3.Width && inspection.blockavgTool[TrackIndex].WholeMask[1].Height == mat3.Height)
			{
				Cv2.BitwiseOr(mat3, inspection.blockavgTool[TrackIndex].WholeMask[1], mat3);
			}
		}
		Mat mat4 = new Mat(ShowBitmap.Height, ShowBitmap.Width, MatType.CV_8UC1);
		mat4.SetTo(0.0);
		mat4.Rectangle(rect, Scalar.White, -1);
		if (ts_Tracker2ON.IsChecked == true)
		{
			mat4.Rectangle(rect2, Scalar.White, -1);
		}
		Cv2.BitwiseAnd(mat4, mat, mat);
		if (chk_UseMaskAsInspArea.IsChecked == true)
		{
			Cv2.BitwiseNot(mat, mat);
		}
		Mat mat5 = new Mat();
		Cv2.Split(ShowBitmap, out ThdShow);
		ThdShow[0].SetTo(0.0);
		ThdShow[1].SetTo(255.0);
		ThdShow[2].SetTo(255.0);
		Cv2.Merge(ThdShow, mat5);
		mat5.CopyTo(ShowBitmap, mat);
		ImageSource imageSource = ShowBitmap.ToBitmapSource();
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	private void Txt_AvgGrayMax_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
	}

	private void Txt_AvgGrayMin_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
	}

	private void Rad_AvgBinaryA_Checked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			grd_AvgThdFun.Visibility = Visibility.Visible;
		}
	}

	private void Rad_AvgBinaryA_Unchecked(object sender, RoutedEventArgs e)
	{
		if (inited)
		{
			grd_AvgThdFun.Visibility = Visibility.Hidden;
		}
	}

	private void cnv_Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sfr.ScaleX != 1.0)
		{
			System.Windows.Point position = e.GetPosition(cnv_Image);
			sfr.CenterX = position.X;
			sfr.CenterY = position.Y;
		}
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
			INSP_DIFFERENCETOOL_PARAM param = inspection.paramFile.BlockAvgParam[index];
			string text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["BlockAvgTool"], index + 1, param.TrackerName);
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
			if (param.ChannelIndex != BackupParam.ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)param.ChannelIndex));
			}
			if (param.P2PIntervalX != BackupParam.P2PIntervalX)
			{
				ModifyLog += string.Format("{0} [({1}) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["HorizontalAvg"], BackupParam.P2PIntervalX, LanguageManager.Instance["ChangedTo"], param.P2PIntervalX);
			}
			if (param.P2PIntervalY != BackupParam.P2PIntervalY)
			{
				ModifyLog += string.Format("{0} [({1}) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["VerticalAvg"], BackupParam.P2PIntervalY, LanguageManager.Instance["ChangedTo"], param.P2PIntervalY);
			}
			if (param.ThdUpON != BackupParam.ThdUpON)
			{
				if (param.ThdUpON)
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["ThdUp"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["ThdUp"], LanguageManager.Instance["Close"]);
				}
			}
			if (param.ThdDownON != BackupParam.ThdDownON)
			{
				if (param.ThdDownON)
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["ThdDown"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["ThdDown"], LanguageManager.Instance["Close"]);
				}
			}
			if (param.ThdUp != BackupParam.ThdUp)
			{
				ModifyLog += string.Format("{0} [({1}) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.ThdUp, LanguageManager.Instance["ChangedTo"], param.ThdUp);
			}
			if (param.ThdDown != BackupParam.ThdDown)
			{
				ModifyLog += string.Format("{0} [({1}) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.ThdDown, LanguageManager.Instance["ChangedTo"], param.ThdDown);
			}
			if (param.UseAvgBinary != BackupParam.UseAvgBinary)
			{
				if (param.UseAvgBinary)
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["UseAvgBinary"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2}]\n", text4, LanguageManager.Instance["UseAvgBinary"], LanguageManager.Instance["Close"]);
				}
			}
			if (param.AvgBinaryMethod != BackupParam.AvgBinaryMethod)
			{
				ModifyLog += string.Format("{0} [({1}) {2} ({3})]\n", text4, modifyLogFun.GetAvgBinaryText(param.AvgBinaryMethod), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetAvgBinaryText(BackupParam.AvgBinaryMethod));
			}
			if (param.THDLimit_Min != BackupParam.THDLimit_Min)
			{
				ModifyLog += string.Format("{0} [{1}{2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["BinaryTool"], LanguageManager.Instance["Min2"], BackupParam.THDLimit_Min, LanguageManager.Instance["ChangedTo"], param.THDLimit_Min);
			}
			if (param.THDLimit_Max != BackupParam.THDLimit_Max)
			{
				ModifyLog += string.Format("{0} [{1}{2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["BinaryTool"], LanguageManager.Instance["Max2"], BackupParam.THDLimit_Max, LanguageManager.Instance["ChangedTo"], param.THDLimit_Max);
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_Start_Index, (int)BackupParam.DE_Start_Index, param.DE_Start_Count, BackupParam.DE_Start_Count, 1, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)param.DE_End_Index, (int)BackupParam.DE_End_Index, param.DE_End_Count, BackupParam.DE_End_Count, 1, 2, text4);
			GetTrackModifyText(text4, param);
			GetMaskModifyText(text4, param);
			if (param.BlobAnalysisSelect != BackupParam.BlobAnalysisSelect)
			{
				ModifyLog += string.Format("{0}\t[{1}][{2}{3}-{4}] [({5}) {6} {7} {8}]\n", text3, inspection.StationTxt, LanguageManager.Instance["BlockAvgTool"], index + 1, param.TrackerName, LanguageManager.Instance["InspRule"], modifyLogFun.GetBlobAnalysisText(BackupParam.BlobAnalysisSelect), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobAnalysisText(param.BlobAnalysisSelect));
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
				ModifyLog += string.Format("{0} [({1}1-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule1_Index), LanguageManager.Instance["Min2"], BackupParam.InspRule1_Min, LanguageManager.Instance["ChangedTo"], param.InspRule1_Min);
			}
			if (param.InspRule1_Max != BackupParam.InspRule1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule1_Index), LanguageManager.Instance["Max2"], BackupParam.InspRule1_Max, LanguageManager.Instance["ChangedTo"], param.InspRule1_Max);
			}
			if (param.InspRule2_Index != BackupParam.InspRule2_Index)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule2_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule2_Index));
			}
			if (param.InspRule2_Min != BackupParam.InspRule2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule2_Index), LanguageManager.Instance["Min2"], BackupParam.InspRule2_Min, LanguageManager.Instance["ChangedTo"], param.InspRule2_Min);
			}
			if (param.InspRule2_Max != BackupParam.InspRule2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule2_Index), LanguageManager.Instance["Max2"], BackupParam.InspRule2_Max, LanguageManager.Instance["ChangedTo"], param.InspRule2_Max);
			}
			if (param.InspRule3_Index != BackupParam.InspRule3_Index)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule3_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule3_Index));
			}
			if (param.InspRule3_Min != BackupParam.InspRule3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule3_Index), LanguageManager.Instance["Min2"], BackupParam.InspRule3_Min, LanguageManager.Instance["ChangedTo"], param.InspRule3_Min);
			}
			if (param.InspRule3_Max != BackupParam.InspRule3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule3_Index), LanguageManager.Instance["Max2"], BackupParam.InspRule3_Max, LanguageManager.Instance["ChangedTo"], param.InspRule3_Max);
			}
			if (param.InspRule4_Index != BackupParam.InspRule4_Index)
			{
				ModifyLog += string.Format("{0} [({1}4) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule4_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)param.InspRule4_Index));
			}
			if (param.InspRule4_Min != BackupParam.InspRule4_Min)
			{
				ModifyLog += string.Format("{0} [({1}4-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule4_Index), LanguageManager.Instance["Min2"], BackupParam.InspRule4_Min, LanguageManager.Instance["ChangedTo"], param.InspRule4_Min);
			}
			if (param.InspRule4_Max != BackupParam.InspRule4_Max)
			{
				ModifyLog += string.Format("{0} [({1}4-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)param.InspRule4_Index), LanguageManager.Instance["Max2"], BackupParam.InspRule4_Max, LanguageManager.Instance["ChangedTo"], param.InspRule4_Max);
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
			string text5 = "BW10-BlockAvgToolWindow-WriteModifyLog()-";
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
	private void GetMaskModifyText(string Title, INSP_DIFFERENCETOOL_PARAM param)
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTrackModifyText(string Title, INSP_DIFFERENCETOOL_PARAM param)
	{
		if (param.Tracker1UseAutoSize != BackupParam.Tracker1UseAutoSize)
		{
			if (param.Tracker1UseAutoSize)
			{
				ModifyLog += string.Format("{0} [(A{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["Alignment"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["AutoFit"]);
			}
			else
			{
				ModifyLog += string.Format("{0} [(A{1}) {2} {3} {4}]  \n", Title, LanguageManager.Instance["Track"], LanguageManager.Instance["AutoFit"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Alignment"]);
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/blockavgtoolwindow.xaml", UriKind.Relative);
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
			MainW = (BlockAvgToolWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			MainW.Drop += MainW_Drop;
			MainW.ContentRendered += MainW_ContentRendered;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			cnv_Image.PreviewMouseRightButtonDown += cnv_Image_PreviewMouseRightButtonDown;
			break;
		case 3:
			sfr = (ScaleTransform)target;
			break;
		case 4:
			line_X1 = (Line)target;
			break;
		case 5:
			line_Y1 = (Line)target;
			break;
		case 6:
			line_X2 = (Line)target;
			break;
		case 7:
			line_Y2 = (Line)target;
			break;
		case 8:
			bor_LocateRect = (Border)target;
			break;
		case 9:
			bor_SearchRect2 = (Border)target;
			bor_SearchRect2.GotFocus += Bor_SearchRect2_GotFocus;
			bor_SearchRect2.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 10:
			txt_Search2Label = (TextBlock)target;
			break;
		case 11:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.GotFocus += Bor_SearchRect1_GotFocus;
			bor_SearchRect1.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 12:
			txt_Search1Label = (TextBlock)target;
			break;
		case 13:
			Label_Result = (Label)target;
			break;
		case 14:
			rt_Mask1 = (Rectangle)target;
			break;
		case 15:
			rt_Mask2 = (Rectangle)target;
			break;
		case 16:
			rt_ReferenceRect1 = (Rectangle)target;
			break;
		case 17:
			rt_ReferenceRect2 = (Rectangle)target;
			break;
		case 18:
			lab_FaceIndex = (Label)target;
			break;
		case 19:
			Label_ResultError = (TextBox)target;
			break;
		case 20:
			btn_ResetA = (Button)target;
			btn_ResetA.Click += Btn_TrackerReset_Click;
			break;
		case 21:
			btn_ResetB = (Button)target;
			btn_ResetB.Click += Btn_TrackerReset_Click;
			break;
		case 22:
			txt_PreviewMode = (TextBox)target;
			break;
		case 23:
			tab_Function = (TabControl)target;
			break;
		case 24:
			grd_Main = (Grid)target;
			break;
		case 25:
			cb_TrackerSelect = (ComboBox)target;
			cb_TrackerSelect.SelectionChanged += Cb_TrackerSelect_SelectionChanged;
			break;
		case 26:
			scr_TrackerSwich = (ScrollBar)target;
			scr_TrackerSwich.SizeChanged += Scr_TrackerSwich_SizeChanged;
			scr_TrackerSwich.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 27:
			btn_Reverse = (Button)target;
			btn_Reverse.Click += Btn_Reverse_Click;
			break;
		case 28:
			btn_RecipeCopy = (Button)target;
			btn_RecipeCopy.Click += Btn_RecipeCopy_Click;
			break;
		case 29:
			btn_RecipePaste = (Button)target;
			btn_RecipePaste.Click += Btn_RecipePaste_Click;
			break;
		case 30:
			txt_TrackerName = (TextBox)target;
			break;
		case 31:
			ts_TrackerON = (ToggleSwitch)target;
			ts_TrackerON.Click += Ts_TrackerON_Click;
			break;
		case 32:
			lab_TrackIndex = (Label)target;
			break;
		case 33:
			lab_Pareto = (Label)target;
			break;
		case 34:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 35:
			lab_Time = (Label)target;
			break;
		case 36:
			gb_BasicSetup = (GroupBox)target;
			break;
		case 37:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 38:
			grd_SIMP_DilateErode4 = (Grid)target;
			break;
		case 39:
			cb_ThdDEStart = (ComboBox)target;
			cb_ThdDEStart.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 40:
			cb_ThdDEEnd = (ComboBox)target;
			cb_ThdDEEnd.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 41:
			cb_ThdDEEndCount = (ComboBox)target;
			cb_ThdDEEndCount.SelectionChanged += Cb_ThdDEStartCount_SelectionChanged;
			break;
		case 42:
			cb_ThdDEStartCount = (ComboBox)target;
			cb_ThdDEStartCount.SelectionChanged += Cb_ThdDEStartCount_SelectionChanged;
			break;
		case 43:
			cb_ChannelIndex = (ComboBox)target;
			cb_ChannelIndex.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 44:
			chk_ThdUpON = (CheckBox)target;
			chk_ThdUpON.Click += Chk_ThdUpON_Click;
			break;
		case 45:
			chk_ThdDownON = (CheckBox)target;
			chk_ThdDownON.Click += Chk_ThdUpON_Click;
			break;
		case 46:
			nu_ThdUp = (NumericUpDown)target;
			nu_ThdUp.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 47:
			nu_ThdDown = (NumericUpDown)target;
			nu_ThdDown.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 48:
			nu_P2PIntervalX = (NumericUpDown)target;
			nu_P2PIntervalX.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 49:
			nu_P2PIntervalY = (NumericUpDown)target;
			nu_P2PIntervalY.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 50:
			gb_ConditionSetup = (GroupBox)target;
			break;
		case 51:
			grd_ConditionSetup = (Grid)target;
			break;
		case 52:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 53:
			rt_Result = (Rectangle)target;
			break;
		case 54:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 55:
			tab_BlobAnalysis = (TabControl)target;
			tab_BlobAnalysis.SelectionChanged += Tab_BlobAnalysis_SelectionChanged;
			break;
		case 56:
			tb_MaxBlob = (TabItem)target;
			break;
		case 57:
			txt_InspRule3Min = (TextBox)target;
			txt_InspRule3Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 58:
			txt_InspRule3Max = (TextBox)target;
			txt_InspRule3Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 59:
			txt_InspRule4Min = (TextBox)target;
			txt_InspRule4Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 60:
			txt_InspRule4Max = (TextBox)target;
			txt_InspRule4Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 61:
			txt_InspRule13Result = (TextBox)target;
			break;
		case 62:
			txt_InspRule14Result = (TextBox)target;
			break;
		case 63:
			txt_InspRule1Min = (TextBox)target;
			txt_InspRule1Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 64:
			txt_InspRule1Max = (TextBox)target;
			txt_InspRule1Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 65:
			txt_InspRule2Min = (TextBox)target;
			txt_InspRule2Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 66:
			txt_InspRule2Max = (TextBox)target;
			txt_InspRule2Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 67:
			txt_InspRule11Result = (TextBox)target;
			break;
		case 68:
			txt_InspRule12Result = (TextBox)target;
			break;
		case 69:
			cb_InspRule1 = (ComboBox)target;
			break;
		case 70:
			cb_InspRule2 = (ComboBox)target;
			break;
		case 71:
			cb_InspRule3 = (ComboBox)target;
			break;
		case 72:
			cb_InspRule4 = (ComboBox)target;
			break;
		case 73:
			txt_InspRule23Result = (TextBox)target;
			break;
		case 74:
			txt_InspRule24Result = (TextBox)target;
			break;
		case 75:
			txt_InspRule21Result = (TextBox)target;
			break;
		case 76:
			txt_InspRule22Result = (TextBox)target;
			break;
		case 77:
			lab_Tracker1Result = (Label)target;
			break;
		case 78:
			lab_Tracker2Result = (Label)target;
			break;
		case 79:
			rad_RuleAND = (RadioButton)target;
			break;
		case 80:
			rad_RuleOR = (RadioButton)target;
			break;
		case 81:
			tb_BlobLine = (TabItem)target;
			break;
		case 82:
			___沒有名稱_ = (Grid)target;
			break;
		case 83:
			txt_BlobLineAreaMin = (TextBox)target;
			break;
		case 84:
			txt_BlobLineAreaMax = (TextBox)target;
			break;
		case 85:
			txt_BlobLineMin = (TextBox)target;
			txt_BlobLineMin.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 86:
			txt_OKLineResultA = (TextBox)target;
			break;
		case 87:
			txt_BlobLineHeightMin = (TextBox)target;
			break;
		case 88:
			txt_BlobLineHeightMax = (TextBox)target;
			break;
		case 89:
			cb_BlobLineSort = (ComboBox)target;
			break;
		case 90:
			txt_OKLineResultB = (TextBox)target;
			break;
		case 91:
			txt_BlobLineWidthMin = (TextBox)target;
			break;
		case 92:
			txt_BlobLineWidthMax = (TextBox)target;
			break;
		case 93:
			txt_BlobLineMax = (TextBox)target;
			txt_BlobLineMax.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 94:
			txt_BlobLineNum = (TextBox)target;
			txt_BlobLineNum.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 95:
			tb_MultiBlob = (TabItem)target;
			break;
		case 96:
			txt_MB_InspRule3Min = (TextBox)target;
			txt_MB_InspRule3Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 97:
			txt_MB_InspRule3Max = (TextBox)target;
			txt_MB_InspRule3Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 98:
			txt_MB_InspRule4Min = (TextBox)target;
			txt_MB_InspRule4Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 99:
			txt_MB_InspRule4Max = (TextBox)target;
			txt_MB_InspRule4Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 100:
			txt_MB_InspRule1Min = (TextBox)target;
			txt_MB_InspRule1Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 101:
			txt_MB_InspRule1Max = (TextBox)target;
			txt_MB_InspRule1Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 102:
			txt_MB_InspRule2Min = (TextBox)target;
			txt_MB_InspRule2Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 103:
			txt_MB_InspRule2Max = (TextBox)target;
			txt_MB_InspRule2Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 104:
			cb_MB_InspRule1 = (ComboBox)target;
			break;
		case 105:
			cb_MB_InspRule2 = (ComboBox)target;
			break;
		case 106:
			cb_MB_InspRule3 = (ComboBox)target;
			break;
		case 107:
			cb_MB_InspRule4 = (ComboBox)target;
			break;
		case 108:
			rad_MB_RuleAND = (RadioButton)target;
			break;
		case 109:
			rad_MB_RuleOR = (RadioButton)target;
			break;
		case 110:
			cb_MB_BlobSort = (ComboBox)target;
			break;
		case 111:
			txt_MB_BlobNum = (TextBox)target;
			txt_MB_BlobNum.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 112:
			txt_MB_NGCount = (TextBox)target;
			txt_MB_NGCount.PreviewMouseDoubleClick += Txt_BlobLineNum_PreviewMouseDoubleClick;
			break;
		case 113:
			grd_SelectFace = (Grid)target;
			break;
		case 114:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 115:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 116:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 117:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 118:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 119:
			exp_ColorLimit = (Expander)target;
			break;
		case 120:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 121:
			rs_ThdLimit = (RangeSlider)target;
			rs_ThdLimit.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdLimit.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_ThdLimit.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 122:
			nu_ThdLimitMax = (NumericUpDown)target;
			nu_ThdLimitMax.ValueChanged += Nu_ThdLimitMax_ValueChanged;
			break;
		case 123:
			nu_ThdLimitMin = (NumericUpDown)target;
			nu_ThdLimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 124:
			cb_FilterNoise = (CheckBox)target;
			cb_FilterNoise.Click += Cb_FilterNoise_Click;
			break;
		case 125:
			nu_NoiseWidth = (NumericUpDown)target;
			nu_NoiseWidth.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 126:
			nu_NoiseHeight = (NumericUpDown)target;
			nu_NoiseHeight.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 127:
			cb_UseAvgBinary = (CheckBox)target;
			cb_UseAvgBinary.Click += Cb_UseAvgBinary_Click;
			break;
		case 128:
			rad_AvgBinaryA = (RadioButton)target;
			rad_AvgBinaryA.Checked += Rad_AvgBinaryA_Checked;
			rad_AvgBinaryA.Unchecked += Rad_AvgBinaryA_Unchecked;
			break;
		case 129:
			rad_AvgBinaryH = (RadioButton)target;
			break;
		case 130:
			rad_AvgBinaryV = (RadioButton)target;
			break;
		case 131:
			cb_AvgShift = (ComboBox)target;
			cb_AvgShift.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 132:
			grd_AvgThdFun = (Grid)target;
			break;
		case 133:
			txt_AvgGray = (TextBox)target;
			break;
		case 134:
			txt_AvgGrayMin = (TextBox)target;
			txt_AvgGrayMin.PreviewMouseDoubleClick += Txt_AvgGrayMin_PreviewMouseDoubleClick;
			break;
		case 135:
			txt_AvgGrayMax = (TextBox)target;
			txt_AvgGrayMax.PreviewMouseDoubleClick += Txt_AvgGrayMax_PreviewMouseDoubleClick;
			break;
		case 136:
			exp_TrackerSetup = (Expander)target;
			break;
		case 137:
			grd_SIMP_DilateErode1_Copy = (Grid)target;
			break;
		case 138:
			cb_Track1AlignWayX = (ComboBox)target;
			cb_Track1AlignWayX.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 139:
			rad_Tracker1AutoSize = (RadioButton)target;
			rad_Tracker1AutoSize.Click += Rad_Tracker1AutoSize_Click;
			break;
		case 140:
			cb_Track1AutoSize = (ComboBox)target;
			cb_Track1AutoSize.SelectionChanged += Cb_Track1AutoSize_SelectionChanged;
			break;
		case 141:
			rad_Tracker1AlignSelect = (RadioButton)target;
			rad_Tracker1AlignSelect.Click += Rad_Tracker1AlignSelect_Click;
			break;
		case 142:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 143:
			cb_Track1AlignWayY = (ComboBox)target;
			cb_Track1AlignWayY.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 144:
			btn_Tracker1SizeChange = (Button)target;
			btn_Tracker1SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 145:
			txt_Tracker1Pos = (Label)target;
			break;
		case 146:
			chk_TrackErrorSetNG = (CheckBox)target;
			chk_TrackErrorSetNG.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 147:
			grd_Track2Setup = (Grid)target;
			break;
		case 148:
			cb_Track2AlignWayX = (ComboBox)target;
			cb_Track2AlignWayX.SelectionChanged += Cb_Track2AlignWayX_SelectionChanged;
			break;
		case 149:
			rad_Tracker2AutoSize = (RadioButton)target;
			rad_Tracker2AutoSize.Click += Rad_Tracker2AutoSize_Click;
			break;
		case 150:
			cb_Track2AutoSize = (ComboBox)target;
			cb_Track2AutoSize.SelectionChanged += Cb_Track2AutoSize_SelectionChanged;
			break;
		case 151:
			rad_Tracker2AlignSelect = (RadioButton)target;
			rad_Tracker2AlignSelect.Click += Rad_Tracker2AlignSelect_Click;
			break;
		case 152:
			cb_Track2AlignWayY = (ComboBox)target;
			cb_Track2AlignWayY.SelectionChanged += Cb_Track2AlignWayX_SelectionChanged;
			break;
		case 153:
			btn_TrackerReset2 = (Button)target;
			btn_TrackerReset2.Click += Btn_TrackerReset_Click;
			break;
		case 154:
			btn_Tracker2SizeChange = (Button)target;
			btn_Tracker2SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 155:
			txt_Tracker2Pos = (Label)target;
			break;
		case 156:
			ts_Tracker2ON = (CheckBox)target;
			ts_Tracker2ON.Click += Ts_Tracker2ON_Click;
			break;
		case 157:
			cb_MaskSelect = (ComboBox)target;
			cb_MaskSelect.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 158:
			btn_MaskEdit = (Button)target;
			btn_MaskEdit.Click += Btn_MaskEdit_Click;
			break;
		case 159:
			chk_UseMaskAsInspArea = (CheckBox)target;
			chk_UseMaskAsInspArea.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 160:
			cb_MatchMaskSelect1 = (ComboBox)target;
			cb_MatchMaskSelect1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 161:
			cb_MatchMaskSelect2 = (ComboBox)target;
			cb_MatchMaskSelect2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 162:
			cb_MatchMaskSelect3 = (ComboBox)target;
			cb_MatchMaskSelect3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 163:
			cb_MatchText_DEStart1 = (ComboBox)target;
			cb_MatchText_DEStart1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 164:
			cb_MatchText_DEStartCount1 = (ComboBox)target;
			cb_MatchText_DEStartCount1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 165:
			cb_MatchText_DEStart2 = (ComboBox)target;
			cb_MatchText_DEStart2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 166:
			cb_MatchText_DEStartCount2 = (ComboBox)target;
			cb_MatchText_DEStartCount2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 167:
			cb_MatchText_DEStart3 = (ComboBox)target;
			cb_MatchText_DEStart3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 168:
			cb_MatchText_DEStartCount3 = (ComboBox)target;
			cb_MatchText_DEStartCount3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 169:
			btn_MaskPreView = (Button)target;
			btn_MaskPreView.PreviewMouseLeftButtonDown += Btn_MaskPreView_PreviewMouseLeftButtonDown;
			btn_MaskPreView.PreviewMouseLeftButtonUp += Btn_MaskPreView_PreviewMouseLeftButtonUp;
			break;
		case 170:
			btn_TextMaskPreView = (Button)target;
			btn_TextMaskPreView.PreviewMouseLeftButtonDown += Btn_TextMaskPreView_PreviewMouseLeftButtonDown;
			btn_TextMaskPreView.PreviewMouseLeftButtonUp += Btn_MaskPreView_PreviewMouseLeftButtonUp;
			break;
		case 171:
			btn_PrevTracker = (Button)target;
			btn_PrevTracker.Click += Btn_PrevTracker;
			break;
		case 172:
			btn_NextTracker = (Button)target;
			btn_NextTracker.Click += Btn_NextTracker_Click;
			break;
		case 173:
			txt_AllResult = (TextBox)target;
			break;
		case 174:
			btn_TrackerShow = (Button)target;
			btn_TrackerShow.Click += Btn_TrackerShow_Click;
			break;
		case 175:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 176:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 177:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 178:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 179:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 180:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 181:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 182:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 183:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 184:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 185:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 186:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 187:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 188:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 189:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 190:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 191:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 192:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 193:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 194:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 195:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 196:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 197:
			btn_ReferenceShow = (Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 198:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 199:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 200:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 201:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 202:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 203:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 204:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 205:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 206:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 207:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 208:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 209:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 210:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 211:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 212:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 213:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 214:
			grd_TrackerAdj = (Grid)target;
			break;
		case 215:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 216:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 217:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 218:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 219:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 220:
			lab_Tracker1 = (Label)target;
			break;
		case 221:
			lab_Tracker2 = (Label)target;
			break;
		case 222:
			btn_Fillet = (Button)target;
			btn_Fillet.Click += Btn_Fillet_Click;
			break;
		case 223:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 224:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 225:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 226:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 227:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 228:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 229:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 230:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 231:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 232:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 233:
			btn_ImageNum = (Button)target;
			break;
		case 234:
			btn_ImgAIR = (Button)target;
			btn_ImgAIR.Click += Btn_ImgSrc_Click;
			break;
		case 235:
			btn_ImgAIG = (Button)target;
			btn_ImgAIG.Click += Btn_ImgSrc_Click;
			break;
		case 236:
			btn_ImgAIB = (Button)target;
			btn_ImgAIB.Click += Btn_ImgSrc_Click;
			break;
		case 237:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 238:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 239:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 240:
			btn_ImgAI = (Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 241:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 242:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 243:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 244:
			exp_TrackerFilllet = (Expander)target;
			exp_TrackerFilllet.Collapsed += Exp_TrackerFilllet_Collapsed;
			break;
		case 245:
			((Grid)target).PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 246:
			cb_T1Filllet0 = (ComboBox)target;
			cb_T1Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 247:
			cb_T1Filllet1 = (ComboBox)target;
			cb_T1Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 248:
			cb_T1Filllet2 = (ComboBox)target;
			cb_T1Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 249:
			cb_T1Filllet3 = (ComboBox)target;
			cb_T1Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 250:
			grd_Tracker2Filllet = (Grid)target;
			grd_Tracker2Filllet.PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 251:
			cb_T2Filllet0 = (ComboBox)target;
			cb_T2Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 252:
			cb_T2Filllet1 = (ComboBox)target;
			cb_T2Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 253:
			cb_T2Filllet2 = (ComboBox)target;
			cb_T2Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 254:
			cb_T2Filllet3 = (ComboBox)target;
			cb_T2Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 255:
			lab_RValue = (Label)target;
			break;
		case 256:
			lab_GValue = (Label)target;
			break;
		case 257:
			lab_BValue = (Label)target;
			break;
		case 258:
			lab_HValue = (Label)target;
			break;
		case 259:
			lab_SValue = (Label)target;
			break;
		case 260:
			lab_VValue = (Label)target;
			break;
		case 261:
			lab_GrayValue = (Label)target;
			break;
		case 262:
			lab_XValue = (Label)target;
			break;
		case 263:
			lab_YValue = (Label)target;
			break;
		case 264:
			lab_C1Value = (Label)target;
			break;
		case 265:
			lab_C2Value = (Label)target;
			break;
		case 266:
			lab_C3Value = (Label)target;
			break;
		case 267:
			btn_ImgT1 = (Button)target;
			btn_ImgT1.Click += Btn_ImgT1_Click;
			break;
		case 268:
			btn_ImgT2 = (Button)target;
			btn_ImgT2.Click += Btn_ImgT1_Click;
			break;
		case 269:
			btn_ImgT3 = (Button)target;
			btn_ImgT3.Click += Btn_ImgT1_Click;
			break;
		case 270:
			lab_AIRValue = (Label)target;
			break;
		case 271:
			lab_AIGValue = (Label)target;
			break;
		case 272:
			lab_AIBValue = (Label)target;
			break;
		case 273:
			Label_Index = (Label)target;
			break;
		case 274:
			lab_LockMode = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
