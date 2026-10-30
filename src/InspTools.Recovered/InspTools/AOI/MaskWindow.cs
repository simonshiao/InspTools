using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

public class MaskWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private const int MaxThdTool = 4;

	private const int MaxMaskCombine = 8;

	private int LanguageType = 1;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private byte[] CurrentColor = new byte[13];

	private bool[] SearchRectResult = new bool[2];

	private BlobStruct[] Blobs = new BlobStruct[2];

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private OpenCvSharp.Rect LastSearchRect2;

	private INSP_MASKTOOL_PARAM CopyParam;

	private INSP_MASKTOOL_PARAM BackupParam;

	private INSP_COLORREFERENCE_PARAM CopyColorParam;

	private bool CopyParamSeted;

	private bool DontSave;

	private bool ShowMessage = true;

	private bool ReferenceShow = true;

	private bool TrackerShow = true;

	private bool TrackerAdjShow;

	private bool TrackerFillletShow;

	private bool AdvLocateMaskShow;

	private int CurrentTracker;

	private string Tracker1Str;

	private string Tracker2Str;

	private int SelectFace;

	private int FaceIndex;

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat[] BinaryBlob = new Mat[2];

	private Mat BinaryT2 = new Mat();

	private Mat ImageSourceMat;

	private int ImageListSource;

	private Mat[] MaskPreview = new Mat[8];

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

	private RangeSlider[] THD_Sliders;

	private NumericUpDown[] THD_TextLow;

	private NumericUpDown[] THD_TextUp;

	private ComboBox[] THD_Channel;

	private ComboBox[] THD_DEStart;

	private ComboBox[] THD_DEEnd;

	private ComboBox[] THD_DEStartCount;

	private ComboBox[] THD_DEEndCount;

	private Expander[] THD_Expender;

	private Button[] ToolsButton;

	private ComboBox[] CombineSelect;

	private ComboBox[] CombineRule;

	private ComboBox[] CombineDEStart;

	private ComboBox[] CombineDEStartCount;

	private bool PreviewMode;

	private bool OTSU_White = true;

	private Stopwatch sw = new Stopwatch();

	internal MaskWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Line line_X1;

	internal Line line_Y1;

	internal Line line_X2;

	internal Line line_Y2;

	internal Label Label_Result;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect2;

	internal TextBlock txt_Search2Label;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Label Label_ResultError;

	internal Rectangle rt_ReferenceRect1;

	internal Rectangle rt_ReferenceRect2;

	internal TextBox txt_PreviewMode;

	internal TabControl tab_Function;

	internal TabItem tab_Mask;

	internal Grid grd_SIMP_Setup;

	internal CheckBox chk_FillHole;

	internal CheckBox chk_doConvexHull;

	internal ComboBox cb_MaxBlob;

	internal ComboBox cb_TrackerSelect;

	internal ScrollBar scr_TrackerSwich;

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

	internal Grid grd_SIMP_THD_1;

	internal RangeSlider rs_THD1;

	internal Grid grd_SIMP_DilateErode1;

	internal ComboBox cb_THD1_DEStart;

	internal ComboBox cb_THD1_DEEnd;

	internal ComboBox cb_THD1_DEEndCount;

	internal ComboBox cb_THD1_DEStartCount;

	internal ComboBox cb_Channel_THD1;

	internal Button btn_ColorCopy;

	internal Button btn_ColorPaste;

	internal NumericUpDown nu_Thd1LimitMin;

	internal NumericUpDown nu_Thd1LimitMax;

	internal Button btn_OTSU;

	internal Grid grd_Aligment;

	internal ComboBox cb_Track1AlignWayX;

	internal RadioButton rad_Tracker1AutoSize;

	internal ComboBox cb_Track1AutoSize;

	internal RadioButton rad_Tracker1AlignSelect;

	internal Button btn_TrackerReset;

	internal ComboBox cb_Track1AlignWayY;

	internal Button btn_Tracker1SizeChange;

	internal Label txt_Tracker1Pos;

	internal Grid grd_Track2Setup;

	internal ComboBox cb_Track2AlignWayX;

	internal RadioButton rad_Tracker2AutoSize;

	internal ComboBox cb_Track2AutoSize;

	internal RadioButton rad_Tracker2AlignSelect;

	internal ComboBox cb_Track2AlignWayY;

	internal Button btn_TrackerReset_Copy;

	internal Button btn_Tracker2SizeChange;

	internal Label txt_Tracker2Pos;

	internal CheckBox ts_Tracker2AsMask;

	internal CheckBox ts_Tracker2ON;

	internal TabItem tab_MaskCombine;

	internal ComboBox cb_TrackerSelectCombine;

	internal ScrollBar scr_TrackerSwich1;

	internal Grid grd_Combine;

	internal ComboBox cb_CombineSelect1;

	internal Label lab_TrackIndex0;

	internal Button btn_BinaryPreview1;

	internal Button btn_MaskPreview1;

	internal ComboBox cb_CombineDEStart1;

	internal ComboBox cb_CombineDEStartCount1;

	internal ComboBox cb_CombineRule1;

	internal ComboBox cb_CombineSelect2;

	internal Label lab_TrackIndex1;

	internal ComboBox cb_CombineRule2;

	internal Button btn_BinaryPreview2;

	internal Button btn_MaskPreview1_Copy;

	internal ComboBox cb_CombineDEStart2;

	internal ComboBox cb_CombineDEStartCount2;

	internal ComboBox cb_CombineSelect3;

	internal Label lab_TrackIndex2;

	internal ComboBox cb_CombineRule3;

	internal Button btn_BinaryPreview1_Copy1;

	internal Button btn_MaskPreview1_Copy1;

	internal ComboBox cb_CombineDEStart3;

	internal ComboBox cb_CombineDEStartCount3;

	internal ComboBox cb_CombineSelect4;

	internal Label lab_TrackIndex3;

	internal ComboBox cb_CombineRule4;

	internal Button btn_BinaryPreview1_Copy2;

	internal Button btn_MaskPreview1_Copy2;

	internal ComboBox cb_CombineDEStart4;

	internal ComboBox cb_CombineDEStartCount4;

	internal ComboBox cb_CombineSelect5;

	internal Label lab_TrackIndex4;

	internal ComboBox cb_CombineRule5;

	internal Button btn_BinaryPreview1_Copy3;

	internal Button btn_MaskPreview1_Copy3;

	internal ComboBox cb_CombineDEStart5;

	internal ComboBox cb_CombineDEStartCount5;

	internal ComboBox cb_CombineSelect6;

	internal Label lab_TrackIndex5;

	internal ComboBox cb_CombineRule6;

	internal Button btn_BinaryPreview1_Copy4;

	internal Button btn_MaskPreview1_Copy4;

	internal ComboBox cb_CombineDEStart6;

	internal ComboBox cb_CombineDEStartCount6;

	internal ComboBox cb_CombineSelect7;

	internal Label lab_TrackIndex6;

	internal ComboBox cb_CombineRule7;

	internal Button btn_BinaryPreview1_Copy5;

	internal Button btn_MaskPreview1_Copy5;

	internal ComboBox cb_CombineDEStart7;

	internal ComboBox cb_CombineDEStartCount7;

	internal ComboBox cb_CombineSelect8;

	internal Label lab_TrackIndex7;

	internal ComboBox cb_CombineRule8;

	internal Button btn_BinaryPreview1_Copy6;

	internal Button btn_MaskPreview1_Copy6;

	internal ComboBox cb_CombineDEStart8;

	internal ComboBox cb_CombineDEStartCount8;

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

	internal Button btn_AdvLocateMaskShow;

	internal Label lab_FaceIndex;

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

	internal Button btn_Fillet;

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

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal Button btn_Test;

	internal ComboBox cb_FinalDEStart;

	internal ComboBox cb_FinalDEEnd;

	internal ComboBox cb_FinalDEEndCount;

	internal ComboBox cb_FinalDEStartCount;

	internal TextBox txt_TrackerName;

	internal ToggleSwitch ts_TrackerON;

	internal Button btn_Reverse;

	internal Button btn_RecipeCopy;

	internal Button btn_RecipePaste;

	internal Label lab_TrackIndex;

	internal Label Label_Index;

	internal Expander exp_UseAdvLocateMask;

	internal CheckBox chk_UseAdvLocateMask;

	internal ComboBox cb_UseAdvLocateMaskStart;

	internal ComboBox cb_UseAdvLocateMaskEnd;

	internal ComboBox cb_AdvLocateBorder1;

	internal ComboBox cb_AdvLocateExpand1;

	internal ComboBox cb_AdvLocateExpandCount1;

	internal ComboBox cb_AdvLocateBorder2;

	internal ComboBox cb_AdvLocateExpand2;

	internal ComboBox cb_AdvLocateExpandCount2;

	internal ComboBox cb_AdvLocateBorderFillet;

	internal RadioButton rad_AdvLocateAsInsp;

	internal RadioButton rad_AdvLocateAsMask;

	private bool _contentLoaded;

	public MaskWindow()
	{
		inited = false;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		Label_ResultError.Visibility = Visibility.Hidden;
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
		THD_Expender = new Expander[4] { exp_THD1, exp_THD2, exp_THD3, exp_THD4 };
		ToolsButton = new Button[30]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20,
			btn_Tools21, btn_Tools22, btn_Tools23, btn_Tools24, btn_Tools25, btn_Tools26, btn_Tools27, btn_Tools28, btn_Tools29, btn_Tools30
		};
		CombineSelect = new ComboBox[8] { cb_CombineSelect1, cb_CombineSelect2, cb_CombineSelect3, cb_CombineSelect4, cb_CombineSelect5, cb_CombineSelect6, cb_CombineSelect7, cb_CombineSelect8 };
		CombineRule = new ComboBox[8] { cb_CombineRule1, cb_CombineRule2, cb_CombineRule3, cb_CombineRule4, cb_CombineRule5, cb_CombineRule6, cb_CombineRule7, cb_CombineRule8 };
		CombineDEStart = new ComboBox[8] { cb_CombineDEStart1, cb_CombineDEStart2, cb_CombineDEStart3, cb_CombineDEStart4, cb_CombineDEStart5, cb_CombineDEStart6, cb_CombineDEStart7, cb_CombineDEStart8 };
		CombineDEStartCount = new ComboBox[8] { cb_CombineDEStartCount1, cb_CombineDEStartCount2, cb_CombineDEStartCount3, cb_CombineDEStartCount4, cb_CombineDEStartCount5, cb_CombineDEStartCount6, cb_CombineDEStartCount7, cb_CombineDEStartCount8 };
		Label_Result.Visibility = Visibility.Hidden;
		rt_ReferenceRect1.Visibility = Visibility.Hidden;
		rt_ReferenceRect2.Visibility = Visibility.Hidden;
		line_X1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		line_Y1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		BinaryBlob[0] = new Mat();
		BinaryBlob[1] = new Mat();
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		exp_UseAdvLocateMask.Visibility = Visibility.Hidden;
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
		UpdateUI = false;
		cb_TrackerSelect.Items.Clear();
		cb_TrackerSelectCombine.Items.Clear();
		cb_Track1AlignWayX.Items.Clear();
		cb_Track1AlignWayY.Items.Clear();
		cb_Track2AlignWayX.Items.Clear();
		cb_Track2AlignWayY.Items.Clear();
		cb_Track1AutoSize.Items.Clear();
		cb_Track2AutoSize.Items.Clear();
		cb_UseAdvLocateMaskStart.Items.Clear();
		cb_UseAdvLocateMaskEnd.Items.Clear();
		try
		{
			for (int i = 0; i < 20; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.MaskParam[i].TrackerName);
				if (!inspection.paramFile.AdvLocateToolON)
				{
					if (i < 2)
					{
						cb_Track1AlignWayX.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
						cb_Track2AlignWayX.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
						cb_Track1AlignWayY.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
						cb_Track2AlignWayY.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
						if (i < 1)
						{
							cb_Track1AutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
							cb_Track2AutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
						}
					}
					continue;
				}
				if (inspection.paramFile.VSLocateParam[i].TrackerON)
				{
					cb_Track1AlignWayX.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
					cb_Track2AlignWayX.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
					comboBoxItem.IsEnabled = false;
					cb_Track1AlignWayX.Items.Add(comboBoxItem);
					ComboBoxItem comboBoxItem2 = new ComboBoxItem();
					comboBoxItem2.Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
					comboBoxItem2.IsEnabled = false;
					cb_Track2AlignWayX.Items.Add(comboBoxItem2);
				}
				if (inspection.paramFile.HSLocateParam[i].TrackerON)
				{
					cb_Track1AlignWayY.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
					cb_Track2AlignWayY.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem3 = new ComboBoxItem();
					comboBoxItem3.Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
					comboBoxItem3.IsEnabled = false;
					cb_Track1AlignWayY.Items.Add(comboBoxItem3);
					ComboBoxItem comboBoxItem4 = new ComboBoxItem();
					comboBoxItem4.Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
					comboBoxItem4.IsEnabled = false;
					cb_Track2AlignWayY.Items.Add(comboBoxItem4);
				}
				if (i == 0)
				{
					cb_UseAdvLocateMaskStart.Items.Add("---");
					cb_UseAdvLocateMaskEnd.Items.Add("---");
				}
				if (inspection.paramFile.AdvLocateParam[i].TrackerON)
				{
					cb_Track1AutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
					cb_Track2AutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
					if (i > 0)
					{
						cb_UseAdvLocateMaskStart.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
						cb_UseAdvLocateMaskEnd.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
					}
					continue;
				}
				ComboBoxItem comboBoxItem5 = new ComboBoxItem();
				comboBoxItem5.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem5.IsEnabled = false;
				cb_Track1AutoSize.Items.Add(comboBoxItem5);
				ComboBoxItem comboBoxItem6 = new ComboBoxItem();
				comboBoxItem6.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem6.IsEnabled = false;
				cb_Track2AutoSize.Items.Add(comboBoxItem6);
				ComboBoxItem comboBoxItem7 = new ComboBoxItem();
				comboBoxItem7.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem7.IsEnabled = false;
				cb_UseAdvLocateMaskStart.Items.Add(comboBoxItem7);
				ComboBoxItem comboBoxItem8 = new ComboBoxItem();
				comboBoxItem8.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem8.IsEnabled = false;
				cb_UseAdvLocateMaskEnd.Items.Add(comboBoxItem8);
			}
			for (int j = 0; j < 10; j++)
			{
				cb_TrackerSelectCombine.Items.Add(j + 21 + " : " + inspection.paramFile.MaskParam[j + 20].TrackerName);
			}
			UpdateUI = true;
			if (index < 20)
			{
				ChangeMaskFunction(0, TrackIndex, write: false);
			}
			else
			{
				ChangeMaskFunction(1, TrackIndex, write: false);
			}
			UpdateToolBox();
		}
		catch
		{
			string text = "MW01-MaskWindow-SetInspection()";
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
			if (inspection.paramFile.MaskParam[i].TrackerON)
			{
				if (inspection.paramFile.MaskParam[i].EnableSelectFace)
				{
					if (inspection.paramFile.MaskParam[i].FaceIndex == FaceIndex)
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
			else
			{
				ToolsButton[i].Background = Brushes.White;
			}
			ToolsButton[i].ToolTip = inspection.paramFile.MaskParam[i].TrackerName;
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

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
	{
		ImageWidth = mat.Cols;
		ImageHeight = mat.Rows;
		ImageBand = mat.Channels();
		ImageSourceMat = mat;
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.MaskParam[TrackIndex].TrackerName) + "BT01-MaskWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI();
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		WriteUIValueToParam();
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
	private void SetTracker(OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2)
	{
		if (!PreviewMode)
		{
			OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, ImageWidth - 1, ImageHeight - 1);
			rect1 &= rect3;
			if (rect1.Width <= 0 || rect1.Height <= 0)
			{
				rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
			rect2 &= rect3;
			if (rect2.Width <= 0 || rect2.Height <= 0)
			{
				rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			bor_SearchRect2.SetValue(Canvas.LeftProperty, (double)rect2.X * xRatio);
			bor_SearchRect2.SetValue(Canvas.TopProperty, (double)rect2.Y * yRatio);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, (double)rect2.Width * xRatio);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, (double)rect2.Height * yRatio);
			txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			txt_Tracker2Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect2.X, rect2.Y, rect2.Width, rect2.Height);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTracker(ref OpenCvSharp.Rect rect1, ref OpenCvSharp.Rect rect2)
	{
		if (PreviewMode || xRatio == 0.0 || yRatio == 0.0)
		{
			return;
		}
		OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, ImageWidth - 1, ImageHeight - 1);
		rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
		rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
		rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
		rect1 &= rect3;
		rect2.X = (int)(Canvas.GetLeft(bor_SearchRect2) / xRatio);
		rect2.Y = (int)(Canvas.GetTop(bor_SearchRect2) / yRatio);
		rect2.Width = (int)(bor_SearchRect2.Width / xRatio);
		rect2.Height = (int)(bor_SearchRect2.Height / yRatio);
		rect2 &= rect3;
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		if (rad_Tracker1AlignSelect.IsChecked == true)
		{
			if (LanguageType == 0)
			{
				Tracker1Str = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			}
			else
			{
				Tracker1Str = string.Format("[A] X:{0}, Y:{1}, 寬:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			}
		}
		else if (LanguageType == 0)
		{
			Tracker1Str = string.Format("[A] L:{0}, T:{1}, R:{2}, B:{3}, W:{4}, H:{5}", inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
		}
		else
		{
			Tracker1Str = string.Format("[A] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 寬:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.X - rect1.X, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Y - rect1.Y, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Right - rect1.Right, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect.Bottom - rect1.Bottom, rect1.Width, rect1.Height);
		}
		txt_Search1Label.Text = Tracker1Str;
		lab_Tracker1.Content = Tracker1Str;
		txt_Tracker2Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect2.X, rect2.Y, rect2.Width, rect2.Height);
		if (rad_Tracker2AlignSelect.IsChecked == true)
		{
			if (LanguageType == 0)
			{
				Tracker2Str = string.Format("[B] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			}
			else
			{
				Tracker2Str = string.Format("[B] X:{0}, Y:{1}, 寬:{2}, 高:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			}
		}
		else if (LanguageType == 0)
		{
			Tracker2Str = string.Format("[B] L:{0}, T:{1}, R:{2}, B:{3}, W:{4}, H:{5}", inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.X - rect2.X, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Y - rect2.Y, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Right - rect2.Right, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Bottom - rect2.Bottom, rect2.Width, rect2.Height);
		}
		else
		{
			Tracker2Str = string.Format("[B] 左:{0}, 上:{1}, 右:{2}, 下:{3}, 寬:{4}, 高:{5}", inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.X - rect2.X, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Y - rect2.Y, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Right - rect2.Right, inspection.paramFile.AdvLocateParam[cb_Track2AutoSize.SelectedIndex].InsideRect.Bottom - rect2.Bottom, rect2.Width, rect2.Height);
		}
		txt_Search2Label.Text = Tracker2Str;
		lab_Tracker2.Content = Tracker2Str;
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

	private void SetResultError(int index, int errorno)
	{
	}

	private void SetResult(bool result)
	{
	}

	private void SetResult(int result, string msg)
	{
		_ = 2;
	}

	private void SetErrorResult(string str)
	{
		Label_Result.Visibility = Visibility.Visible;
		Label_Result.Content = str;
		Label_Result.Background = Brushes.Red;
		ToolsButton[TrackIndex].Background = Brushes.Red;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DoThdImage()
	{
		if (PreviewMode || !UpdateUI || !inspection.buffers.LocatedReady)
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
			ImageSource imageSource = ShowBitmap.ToBitmapSource();
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
			return;
		}
		try
		{
			Label_Result.Visibility = Visibility.Hidden;
			Mat mat = inspection.buffers.GetChannelMat(0).Clone();
			if (tab_Function.SelectedIndex == 0)
			{
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
				OpenCvSharp.Rect[] array11 = new OpenCvSharp.Rect[2]
				{
					new OpenCvSharp.Rect(0, 0, 100, 100),
					new OpenCvSharp.Rect(0, 0, 100, 100)
				};
				GetTracker(ref array11[0], ref array11[1]);
				int num = 0;
				for (int i = 0; i < 2; i++)
				{
					if (i == 1 && ts_Tracker2ON.IsChecked == false)
					{
						continue;
					}
					for (int j = 0; j < 4; j++)
					{
						array[j] = new Mat();
						array2[j] = new Mat();
						array3[j] = (int)THD_Sliders[j].LowerValue;
						array4[j] = (int)THD_Sliders[j].UpperValue;
						array5[j] = THD_Channel[j].SelectedIndex;
						array7[j] = THD_DEStart[j].SelectedIndex;
						array8[j] = THD_DEEnd[j].SelectedIndex;
						array9[j] = THD_DEStartCount[j].SelectedIndex;
						array10[j] = THD_DEEndCount[j].SelectedIndex;
						THD_TextLow[j].Value = array3[j];
						THD_TextUp[j].Value = array4[j];
						if (!array6[j])
						{
							continue;
						}
						array[j] = new Mat(inspection.buffers.GetChannelMat(array5[j]), array11[i]).Clone();
						Cv2.InRange(array[j], array3[j], array4[j], array2[j]);
						if (array7[j] != 0)
						{
							DoMorph(ref array2[j], array7[j], array9[j]);
						}
						if (array8[j] != 0)
						{
							DoMorph(ref array2[j], array8[j], array10[j]);
						}
						if (ts_Tracker2ON.IsChecked == true)
						{
							if (array7[j] != 0)
							{
								DoMorph(ref array2[j], array7[j], array9[j]);
							}
							if (array8[j] != 0)
							{
								DoMorph(ref array2[j], array8[j], array10[j]);
							}
						}
						num++;
					}
					BinaryBlob[i] = array2[0];
					for (int k = 1; k < 4; k++)
					{
						if (array6[k])
						{
							Cv2.BitwiseAnd(BinaryBlob[i], array2[k], BinaryBlob[i]);
						}
					}
					if (i == 0)
					{
						inspection.buffers.DoFilllet(ref BinaryBlob[i], new OpenCvSharp.Rect(0, 0, array11[i].Width, array11[i].Height), cb_T1Filllet0.SelectedIndex, cb_T1Filllet1.SelectedIndex, cb_T1Filllet2.SelectedIndex, cb_T1Filllet3.SelectedIndex);
					}
					else
					{
						inspection.buffers.DoFilllet(ref BinaryBlob[i], new OpenCvSharp.Rect(0, 0, array11[i].Width, array11[i].Height), cb_T2Filllet0.SelectedIndex, cb_T2Filllet1.SelectedIndex, cb_T2Filllet2.SelectedIndex, cb_T2Filllet3.SelectedIndex);
					}
					if (num == 0)
					{
						return;
					}
					if (cb_MaxBlob.SelectedIndex > 0)
					{
						inspection.maskTool.FindMaxBlob(ref BinaryBlob[i], cb_MaxBlob.SelectedIndex, chk_doConvexHull.IsChecked.Value);
					}
					else if (chk_doConvexHull.IsChecked == true)
					{
						inspection.maskTool.doFillConvexHull(ref BinaryBlob[i]);
					}
					if (chk_FillHole.IsChecked == true)
					{
						FillHole(ref BinaryBlob[i]);
					}
					if (chk_UseAdvLocateMask.IsChecked == true && cb_UseAdvLocateMaskStart.SelectedIndex > 0 && cb_UseAdvLocateMaskEnd.SelectedIndex > 0)
					{
						Mat channelMat = inspection.buffers.GetChannelMat(0);
						OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, channelMat.Cols, channelMat.Rows);
						Mat Mat = new Mat(channelMat.Rows, channelMat.Cols, MatType.CV_8UC1);
						Mat.SetTo(0.0);
						int num2 = cb_UseAdvLocateMaskStart.SelectedIndex;
						int num3 = cb_UseAdvLocateMaskEnd.SelectedIndex;
						if (num2 > num3)
						{
							int num4 = num2;
							num2 = num3;
							num3 = num4;
						}
						for (int l = num2; l <= num3; l++)
						{
							OpenCvSharp.Rect rect2 = inspection.paramFile.AdvLocateParam[l].InsideRect;
							DoBorderExpend(ref rect2, cb_AdvLocateBorder1.SelectedIndex, cb_AdvLocateExpand1.SelectedIndex, cb_AdvLocateExpandCount1.SelectedIndex);
							DoBorderExpend(ref rect2, cb_AdvLocateBorder2.SelectedIndex, cb_AdvLocateExpand2.SelectedIndex, cb_AdvLocateExpandCount2.SelectedIndex);
							rect2 &= rect;
							if (rect2.Width > 0 && rect2.Height > 0)
							{
								using (Mat mat2 = new Mat(Mat, rect2))
								{
									mat2.SetTo(255.0);
								}
								if (cb_AdvLocateBorderFillet.SelectedIndex != 0)
								{
									int selectedIndex = cb_AdvLocateBorderFillet.SelectedIndex;
									inspection.buffers.DoFilllet(ref Mat, rect2, selectedIndex, selectedIndex, selectedIndex, selectedIndex);
								}
							}
						}
						if (rad_AdvLocateAsMask.IsChecked == true)
						{
							Cv2.BitwiseNot(Mat, Mat);
							using Mat mat3 = new Mat(Mat, array11[i]);
							Cv2.BitwiseAnd(BinaryBlob[i], mat3, BinaryBlob[i]);
						}
						else
						{
							using Mat mat4 = new Mat(Mat, array11[i]);
							BinaryBlob[i].SetTo(255.0);
							Cv2.BitwiseAnd(BinaryBlob[i], mat4, BinaryBlob[i]);
						}
					}
					if (cb_FinalDEStart.SelectedIndex != 0)
					{
						DoMorph(ref BinaryBlob[i], cb_FinalDEStart.SelectedIndex, cb_FinalDEStartCount.SelectedIndex);
					}
					if (cb_FinalDEEnd.SelectedIndex != 0)
					{
						DoMorph(ref BinaryBlob[i], cb_FinalDEEnd.SelectedIndex, cb_FinalDEEndCount.SelectedIndex);
					}
				}
				inspection.paramFile.MaskParam[TrackIndex].Mask1Pt = array11[0].TopLeft;
				inspection.paramFile.MaskParam[TrackIndex].Mask1 = BinaryBlob[0].Clone();
				if (ts_Tracker2ON.IsChecked.Value)
				{
					inspection.paramFile.MaskParam[TrackIndex].Mask2Pt = array11[1].TopLeft;
					inspection.paramFile.MaskParam[TrackIndex].Mask2 = BinaryBlob[1].Clone();
				}
				mat.SetTo(0.0);
				using (Mat mat5 = new Mat(mat, array11[0]))
				{
					Cv2.BitwiseOr(mat5, BinaryBlob[0], mat5);
				}
				if (ts_Tracker2ON.IsChecked == true)
				{
					using Mat mat6 = new Mat(mat, array11[1]);
					Cv2.BitwiseOr(mat6, BinaryBlob[1], mat6);
				}
				Mat mat7 = mat.Clone();
				mat7.SetTo(0.0);
				new Mat(mat7, array11[0]).SetTo(255.0);
				if (ts_Tracker2ON.IsChecked.Value)
				{
					Mat mat8 = new Mat(mat7, array11[1]);
					if (ts_Tracker2AsMask.IsChecked == false)
					{
						mat8.SetTo(255.0);
					}
					else
					{
						mat8.SetTo(0.0);
					}
				}
				Cv2.BitwiseAnd(mat, mat7, mat);
			}
			else
			{
				OpenCvSharp.Rect[] array12 = new OpenCvSharp.Rect[2];
				mat.SetTo(0.0);
				for (int m = 0; m < 8; m++)
				{
					int num5 = CombineSelect[m].SelectedIndex - 1;
					array12[0] = new OpenCvSharp.Rect(0, 0, 0, 0);
					array12[1] = new OpenCvSharp.Rect(0, 0, 0, 0);
					MaskPreview[m] = mat.Clone();
					MaskPreview[m].SetTo(0.0);
					if (num5 >= 0 && num5 < 20 && inspection.paramFile.MaskParam[num5].TrackerON)
					{
						if (inspection.paramFile.MaskParam[num5].Mask1 != null)
						{
							array12[0] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[num5].Mask1Pt.X, inspection.paramFile.MaskParam[num5].Mask1Pt.Y, inspection.paramFile.MaskParam[num5].Mask1.Cols, inspection.paramFile.MaskParam[num5].Mask1.Rows);
						}
						if (inspection.paramFile.MaskParam[num5].SearchRect2ON && inspection.paramFile.MaskParam[num5].Mask2 != null)
						{
							array12[1] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[num5].Mask2Pt.X, inspection.paramFile.MaskParam[num5].Mask2Pt.Y, inspection.paramFile.MaskParam[num5].Mask2.Cols, inspection.paramFile.MaskParam[num5].Mask2.Rows);
						}
						if (array12[0].Width > 0 && array12[0].Height > 0 && inspection.paramFile.MaskParam[num5].Mask1 != null)
						{
							Mat m2 = new Mat(MaskPreview[m], array12[0]);
							Mat mat9 = inspection.paramFile.MaskParam[num5].Mask1.Clone();
							DoMorph(ref mat9, CombineDEStart[m].SelectedIndex, CombineDEStartCount[m].SelectedIndex);
							mat9.CopyTo(m2);
						}
						if (array12[1].Width > 0 && array12[1].Height > 0 && inspection.paramFile.MaskParam[num5].Mask2 != null)
						{
							Mat m3 = new Mat(MaskPreview[m], array12[1]);
							Mat mat10 = inspection.paramFile.MaskParam[num5].Mask2.Clone();
							DoMorph(ref mat10, CombineDEStart[m].SelectedIndex, CombineDEStartCount[m].SelectedIndex);
							mat10.CopyTo(m3);
						}
						if (m == 0)
						{
							Cv2.BitwiseOr(mat, MaskPreview[m], mat);
						}
						else if (CombineRule[m].SelectedIndex == 0)
						{
							Cv2.BitwiseOr(mat, MaskPreview[m], mat);
						}
						else if (CombineRule[m].SelectedIndex == 1)
						{
							Cv2.BitwiseAnd(mat, MaskPreview[m], mat);
						}
						else if (CombineRule[m].SelectedIndex == 2)
						{
							Cv2.BitwiseXor(mat, MaskPreview[m], mat);
						}
					}
				}
				if (cb_FinalDEStart.SelectedIndex != 0)
				{
					DoMorph(ref mat, cb_FinalDEStart.SelectedIndex, cb_FinalDEStartCount.SelectedIndex);
				}
				if (cb_FinalDEEnd.SelectedIndex != 0)
				{
					DoMorph(ref mat, cb_FinalDEEnd.SelectedIndex, cb_FinalDEEndCount.SelectedIndex);
				}
				inspection.paramFile.MaskParam[TrackIndex].Mask1Pt = new OpenCvSharp.Point(0, 0);
				inspection.paramFile.MaskParam[TrackIndex].Mask1 = mat.Clone();
				inspection.paramFile.MaskParam[TrackIndex].Mask2 = null;
			}
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			using (Mat mat11 = new Mat())
			{
				Cv2.Merge(ThdShow, mat11);
				mat11.CopyTo(ShowBitmap, mat);
			}
			ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush2 = new ImageBrush();
			imageBrush2.ImageSource = imageSource2;
			cnv_Image.Background = imageBrush2;
			imageSource2 = null;
			THD_Preview = true;
		}
		catch (Exception ex)
		{
			string text = "MW02-MaskWindow-DoThdImage()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void FillHole(ref Mat Src)
	{
		using Mat mat = new Mat(Src.Height + 2, Src.Width + 2, MatType.CV_8UC1);
		mat.SetTo(0.0);
		Mat mat2 = new Mat(mat, new OpenCvSharp.Rect(1, 1, Src.Width, Src.Height));
		Src.CopyTo(mat2);
		Cv2.FloodFill(mat, new OpenCvSharp.Point(0, 0), new Scalar(255.0));
		Cv2.BitwiseNot(mat2, mat2);
		Cv2.BitwiseOr(Src, mat2, Src);
	}

	private void DoMorph(ref Mat mat, int index, int count)
	{
		if (count != 0 || index == 10)
		{
			switch (index)
			{
			case 0:
				break;
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
			}
		}
	}

	private void Cb_TrackerSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!PreviewMode && UpdateUI)
		{
			WriteUIValueToParam();
			TrackIndex = cb_TrackerSelect.SelectedIndex;
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
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
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Tracker value will set to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			DontSave = false;
			btn_Test.IsEnabled = true;
			if (((Button)sender).Uid == "0")
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

	private void Scr_TrackerSwich_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_TrackerON.IsChecked.Value)
		{
			inspection.paramFile.MaskParam[TrackIndex].TrackerON = true;
			CopyValueToUI();
			return;
		}
		inspection.paramFile.MaskParam[TrackIndex].TrackerON = false;
		inspection.paramFile.MaskParam[TrackIndex].ValueChangeFlag = true;
		bor_SearchRect1.Visibility = Visibility.Hidden;
		bor_SearchRect2.Visibility = Visibility.Hidden;
		grd_SIMP_Setup.Visibility = Visibility.Hidden;
		grd_Aligment.Visibility = Visibility.Hidden;
		THD_Expender[0].Visibility = Visibility.Hidden;
		THD_Expender[1].Visibility = Visibility.Hidden;
		THD_Expender[2].Visibility = Visibility.Hidden;
		THD_Expender[3].Visibility = Visibility.Hidden;
		DoThdImage();
		ToolsButton[TrackIndex].Background = Brushes.White;
		WriteUIValueToParam();
	}

	private void Bor_SearchRect1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Ts_Tracker2ON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_Tracker2ON.IsChecked.Value)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
			txt_Tracker1Pos.Visibility = Visibility.Visible;
			grd_Track2Setup.Visibility = Visibility.Visible;
			if (TrackIndex != 0)
			{
				txt_Tracker2Pos.Visibility = Visibility.Visible;
			}
			else
			{
				txt_Tracker2Pos.Visibility = Visibility.Hidden;
			}
		}
		else
		{
			bor_SearchRect2.Visibility = Visibility.Hidden;
			txt_Tracker1Pos.Visibility = Visibility.Hidden;
			grd_Track2Setup.Visibility = Visibility.Hidden;
			txt_Tracker2Pos.Visibility = Visibility.Hidden;
		}
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
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect, ref rect2);
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
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.MaskParam[TrackIndex].TrackerName) + "BT01-MaskWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
				btn_Test.IsEnabled = true;
				DontSave = false;
				mat = new Mat();
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.MaskParam[TrackIndex].TrackerName) + "BT01-MaskWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Btn_Tools1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (num < 20)
		{
			cb_TrackerSelect.SelectedIndex = num;
		}
		else
		{
			cb_TrackerSelectCombine.SelectedIndex = num - 20;
		}
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
		CopyParam = inspection.paramFile.MaskParam[TrackIndex];
		btn_RecipeCopy.Background = Brushes.PaleGreen;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if (CopyParamSeted)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				inspection.paramFile.MaskParam[TrackIndex] = CopyParam;
				CopyValueToUI();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Reverse_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Parameters will Revert to Previous save！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测参数将还原至上次储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測參數將還原至上次儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			inspection.paramFile.MaskParam[TrackIndex] = inspection.paramFile_Backup.MaskParam[TrackIndex];
			CopyValueToUI();
		}
	}

	private void Btn_PrevTracker(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam();
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
	}

	private void Btn_NextTracker_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam();
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
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Tracker1SizeChange_Click(object sender, RoutedEventArgs e)
	{
		Edit2InputWindow edit2InputWindow = new Edit2InputWindow();
		GetTracker(ref LastSearchRect1, ref LastSearchRect2);
		Button obj = (Button)sender;
		if (obj.Uid == "1")
		{
			edit2InputWindow.SetMinMaxValue(0, ImageWidth, 0, ImageHeight);
			edit2InputWindow.SetValue(LastSearchRect1.Width, LastSearchRect1.Height);
			edit2InputWindow.ShowDialog();
			if (edit2InputWindow.SetSuccess)
			{
				LastSearchRect1.Width = edit2InputWindow.Value1;
				LastSearchRect1.Height = edit2InputWindow.Value2;
				SetTracker(LastSearchRect1, LastSearchRect2);
			}
		}
		if (obj.Uid == "2" && ts_Tracker2ON.IsChecked == true)
		{
			edit2InputWindow.SetMinMaxValue(0, ImageWidth, 0, ImageHeight);
			edit2InputWindow.SetValue(LastSearchRect1.Width, LastSearchRect1.Height);
			edit2InputWindow.ShowDialog();
			if (edit2InputWindow.SetSuccess)
			{
				LastSearchRect2.Width = edit2InputWindow.Value1;
				LastSearchRect2.Height = edit2InputWindow.Value2;
				SetTracker(LastSearchRect1, LastSearchRect2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		try
		{
			UpdateUI = false;
			DontSave = false;
			Label_ResultError.Visibility = Visibility.Hidden;
			if (PreviewMode)
			{
				btn_Test.Visibility = Visibility.Hidden;
				btn_ColorDropper.Visibility = Visibility.Hidden;
				btn_TestNextImg.Visibility = Visibility.Hidden;
				btn_TestPrevImg.Visibility = Visibility.Hidden;
			}
			BackupParam = inspection.paramFile.MaskParam[TrackIndex];
			INSP_MASKTOOL_PARAM iNSP_MASKTOOL_PARAM = inspection.paramFile.MaskParam[TrackIndex];
			Label_Index.Content = (TrackIndex + 1).ToString();
			lab_TrackIndex.Content = (TrackIndex + 1).ToString();
			ts_TrackerON.IsChecked = iNSP_MASKTOOL_PARAM.TrackerON;
			txt_AllResult.Clear();
			btn_Test.IsEnabled = true;
			txt_TrackerName.Text = iNSP_MASKTOOL_PARAM.TrackerName;
			chk_SelectFace.IsChecked = iNSP_MASKTOOL_PARAM.EnableSelectFace;
			SetSelectFace(iNSP_MASKTOOL_PARAM.FaceIndex);
			cb_FinalDEStart.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_FinalStart_Index;
			cb_FinalDEStartCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_FinalStart_Count;
			cb_FinalDEEnd.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_FinalEnd_Index;
			cb_FinalDEEndCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_FinalEnd_Count;
			if (TrackIndex < 20)
			{
				tab_Function.SelectedIndex = 0;
				if (iNSP_MASKTOOL_PARAM.TrackerON)
				{
					grd_SIMP_Setup.Visibility = Visibility.Visible;
					exp_THD1.Visibility = Visibility.Visible;
					exp_THD2.Visibility = Visibility.Visible;
					exp_THD3.Visibility = Visibility.Visible;
					exp_THD4.Visibility = Visibility.Visible;
					grd_Aligment.Visibility = Visibility.Visible;
					grd_SIMP_THD_1.Visibility = Visibility.Visible;
				}
				else
				{
					grd_SIMP_Setup.Visibility = Visibility.Hidden;
					exp_THD1.Visibility = Visibility.Hidden;
					exp_THD2.Visibility = Visibility.Hidden;
					exp_THD3.Visibility = Visibility.Hidden;
					exp_THD4.Visibility = Visibility.Hidden;
					grd_Aligment.Visibility = Visibility.Hidden;
					grd_SIMP_THD_1.Visibility = Visibility.Hidden;
				}
				cb_TrackerSelect.SelectedIndex = TrackIndex;
				for (int i = 0; i < 4; i++)
				{
					THD_Sliders[i].IsEnabled = false;
					THD_Expender[i].IsEnabled = false;
				}
				grd_SIMP_THD_1.IsEnabled = false;
				for (int j = 0; j < 4; j++)
				{
					THD_Sliders[j].IsEnabled = true;
					THD_Expender[j].IsEnabled = true;
				}
				grd_SIMP_THD_1.IsEnabled = true;
				cb_Channel_THD1.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.THD1_ChannelIndex;
				cb_Channel_THD2.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.THD2_ChannelIndex;
				cb_Channel_THD3.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.THD3_ChannelIndex;
				cb_Channel_THD4.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.THD4_ChannelIndex;
				if (iNSP_MASKTOOL_PARAM.TrackerON)
				{
					exp_THD1.IsExpanded = true;
					exp_THD2.IsExpanded = iNSP_MASKTOOL_PARAM.THD2_ChannelON;
					exp_THD3.IsExpanded = iNSP_MASKTOOL_PARAM.THD3_ChannelON;
					exp_THD4.IsExpanded = iNSP_MASKTOOL_PARAM.THD4_ChannelON;
				}
				else
				{
					exp_THD1.IsExpanded = false;
					exp_THD2.IsExpanded = false;
					exp_THD3.IsExpanded = false;
					exp_THD4.IsExpanded = false;
				}
				rs_THD1.LowerValue = iNSP_MASKTOOL_PARAM.THD1_Min;
				rs_THD1.UpperValue = iNSP_MASKTOOL_PARAM.THD1_Max;
				nu_Thd1LimitMin.Value = iNSP_MASKTOOL_PARAM.THD1_Min;
				nu_Thd1LimitMax.Value = iNSP_MASKTOOL_PARAM.THD1_Max;
				rs_THD2.LowerValue = iNSP_MASKTOOL_PARAM.THD2_Min;
				rs_THD2.UpperValue = iNSP_MASKTOOL_PARAM.THD2_Max;
				nu_Thd2LimitMin.Value = iNSP_MASKTOOL_PARAM.THD2_Min;
				nu_Thd2LimitMax.Value = iNSP_MASKTOOL_PARAM.THD2_Max;
				rs_THD3.LowerValue = iNSP_MASKTOOL_PARAM.THD3_Min;
				rs_THD3.UpperValue = iNSP_MASKTOOL_PARAM.THD3_Max;
				nu_Thd3LimitMin.Value = iNSP_MASKTOOL_PARAM.THD3_Min;
				nu_Thd3LimitMax.Value = iNSP_MASKTOOL_PARAM.THD3_Max;
				rs_THD4.LowerValue = iNSP_MASKTOOL_PARAM.THD4_Min;
				rs_THD4.UpperValue = iNSP_MASKTOOL_PARAM.THD4_Max;
				nu_Thd4LimitMin.Value = iNSP_MASKTOOL_PARAM.THD4_Min;
				nu_Thd4LimitMax.Value = iNSP_MASKTOOL_PARAM.THD4_Max;
				cb_THD1_DEStart.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_Start1_Index;
				cb_THD1_DEEnd.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_End1_Index;
				cb_THD2_DEStart.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_Start2_Index;
				cb_THD2_DEEnd.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_End2_Index;
				cb_THD3_DEStart.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_Start3_Index;
				cb_THD3_DEEnd.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_End3_Index;
				cb_THD4_DEStart.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_Start4_Index;
				cb_THD4_DEEnd.SelectedIndex = (int)iNSP_MASKTOOL_PARAM.DE_End4_Index;
				cb_THD1_DEStartCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_Start1_Count;
				cb_THD1_DEEndCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_End1_Count;
				cb_THD2_DEStartCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_Start2_Count;
				cb_THD2_DEEndCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_End2_Count;
				cb_THD3_DEStartCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_Start3_Count;
				cb_THD3_DEEndCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_End3_Count;
				cb_THD4_DEStartCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_Start4_Count;
				cb_THD4_DEEndCount.SelectedIndex = iNSP_MASKTOOL_PARAM.DE_End4_Count;
				chk_FillHole.IsChecked = iNSP_MASKTOOL_PARAM.FillHole;
				chk_doConvexHull.IsChecked = iNSP_MASKTOOL_PARAM.doConvexHull;
				cb_Track1AlignWayX.SelectedIndex = iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex;
				cb_Track2AlignWayX.SelectedIndex = iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex;
				cb_Track1AlignWayY.SelectedIndex = iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex;
				cb_Track2AlignWayY.SelectedIndex = iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex;
				cb_Track1AutoSize.SelectedIndex = iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex;
				cb_Track2AutoSize.SelectedIndex = iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex;
				if (TrackIndex == 0 && inspection.paramFile.AIEnhanceToolON)
				{
					cb_Track1AlignWayX.SelectedIndex = 0;
					cb_Track2AlignWayX.SelectedIndex = 0;
					cb_Track1AlignWayY.SelectedIndex = 0;
					cb_Track2AlignWayY.SelectedIndex = 0;
					cb_Track1AutoSize.SelectedIndex = 0;
					cb_Track2AutoSize.SelectedIndex = 0;
				}
				cb_MaxBlob.SelectedIndex = iNSP_MASKTOOL_PARAM.MaxBlob;
				chk_UseAdvLocateMask.IsChecked = iNSP_MASKTOOL_PARAM.EnableAdvLocateMask;
				if (iNSP_MASKTOOL_PARAM.EnableAdvLocateMask)
				{
					btn_AdvLocateMaskShow.Background = Brushes.Gold;
				}
				else
				{
					btn_AdvLocateMaskShow.Background = Brushes.White;
				}
				cb_UseAdvLocateMaskStart.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateStartIndex;
				cb_UseAdvLocateMaskEnd.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateEndIndex;
				cb_AdvLocateBorder1.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateBorder1;
				cb_AdvLocateBorder2.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateBorder2;
				cb_AdvLocateExpand1.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateExpand1;
				cb_AdvLocateExpand2.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateExpand2;
				cb_AdvLocateExpandCount1.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateExpandCount1;
				cb_AdvLocateExpandCount2.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateExpandCount2;
				cb_AdvLocateBorderFillet.SelectedIndex = iNSP_MASKTOOL_PARAM.AdvLocateBorderFilllet;
				if (iNSP_MASKTOOL_PARAM.AdvLocateAsInsp)
				{
					rad_AdvLocateAsInsp.IsChecked = true;
				}
				else
				{
					rad_AdvLocateAsMask.IsChecked = true;
				}
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
				OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
				if (iNSP_MASKTOOL_PARAM.Tracker1UseAutoSize)
				{
					if (!inspection.paramFile.AdvLocateToolON && iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex >= 1)
					{
						DontSave = true;
						btn_Test.IsEnabled = false;
						MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
						SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
						txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
					}
					else if (inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].TrackerON)
					{
						rad_Tracker1AutoSize.IsChecked = true;
						rect.X = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].L + iNSP_MASKTOOL_PARAM.Track1_L;
						rect.Y = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].T + iNSP_MASKTOOL_PARAM.Track1_T;
						rect.Width = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].Width - (iNSP_MASKTOOL_PARAM.Track1_L + iNSP_MASKTOOL_PARAM.Track1_R);
						rect.Height = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].Height - (iNSP_MASKTOOL_PARAM.Track1_T + iNSP_MASKTOOL_PARAM.Track1_B);
						SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].InsideRect);
						Line line = line_X1;
						Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
						line.Visibility = visibility;
					}
					else
					{
						DontSave = true;
						btn_Test.IsEnabled = false;
						MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
						SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
						txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
					}
				}
				else if ((iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex >= 2 || iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex >= 2) && !inspection.paramFile.AdvLocateToolON)
				{
					DontSave = true;
					btn_Test.IsEnabled = false;
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
				}
				else if (inspection.paramFile.VSLocateParam[iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex].TrackerON)
				{
					rad_Tracker1AlignSelect.IsChecked = true;
					int width = iNSP_MASKTOOL_PARAM.Track1_R - iNSP_MASKTOOL_PARAM.Track1_L;
					int height = iNSP_MASKTOOL_PARAM.Track1_B - iNSP_MASKTOOL_PARAM.Track1_T;
					rect.X = inspection.paramFile.VSLocateParam[iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex].Value + iNSP_MASKTOOL_PARAM.Track1_L;
					rect.Y = inspection.paramFile.HSLocateParam[iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex].Value + iNSP_MASKTOOL_PARAM.Track1_T;
					rect.Width = width;
					rect.Height = height;
					SetReferenceLine(line_X1, line_Y1, iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex, iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex);
					rt_ReferenceRect1.Visibility = Visibility.Hidden;
				}
				else
				{
					DontSave = true;
					btn_Test.IsEnabled = false;
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
				}
				if (iNSP_MASKTOOL_PARAM.Tracker2UseAutoSize)
				{
					if (!inspection.paramFile.AdvLocateToolON && iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex >= 1)
					{
						DontSave = true;
						btn_Test.IsEnabled = false;
						MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorB"], LanguageManager.Instance["Error"]);
						SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorB"]);
						txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorB"];
					}
					else if (inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].TrackerON)
					{
						rad_Tracker2AutoSize.IsChecked = true;
						rect2.X = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].L + iNSP_MASKTOOL_PARAM.Track2_L;
						rect2.Y = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].T + iNSP_MASKTOOL_PARAM.Track2_T;
						rect2.Width = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].Width - (iNSP_MASKTOOL_PARAM.Track2_L + iNSP_MASKTOOL_PARAM.Track2_R);
						rect2.Height = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].Height - (iNSP_MASKTOOL_PARAM.Track2_T + iNSP_MASKTOOL_PARAM.Track2_B);
						SetReferenceRect(rt_ReferenceRect2, inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].InsideRect);
						Line line2 = line_X2;
						Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
						line2.Visibility = visibility;
					}
					else
					{
						DontSave = true;
						btn_Test.IsEnabled = false;
						MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorB"], LanguageManager.Instance["Error"]);
						SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorB"]);
						txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorB"];
					}
				}
				else if ((iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex >= 2 || iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex >= 2) && !inspection.paramFile.AdvLocateToolON)
				{
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorB"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorB"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorB"];
				}
				else if (inspection.paramFile.VSLocateParam[iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex].TrackerON)
				{
					rad_Tracker2AlignSelect.IsChecked = true;
					int width2 = iNSP_MASKTOOL_PARAM.Track2_R - iNSP_MASKTOOL_PARAM.Track2_L;
					int height2 = iNSP_MASKTOOL_PARAM.Track2_B - iNSP_MASKTOOL_PARAM.Track2_T;
					rect2.X = inspection.paramFile.VSLocateParam[iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex].Value + iNSP_MASKTOOL_PARAM.Track2_L;
					rect2.Y = inspection.paramFile.HSLocateParam[iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex].Value + iNSP_MASKTOOL_PARAM.Track2_T;
					rect2.Width = width2;
					rect2.Height = height2;
					if (iNSP_MASKTOOL_PARAM.SearchRect2ON)
					{
						SetReferenceLine(line_X2, line_Y2, iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex, iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex);
						rt_ReferenceRect2.Visibility = Visibility.Hidden;
					}
				}
				else
				{
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorB"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorB"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorB"];
				}
				if (iNSP_MASKTOOL_PARAM.T1FillletCount0 > 0 || iNSP_MASKTOOL_PARAM.T1FillletCount1 > 0 || iNSP_MASKTOOL_PARAM.T1FillletCount2 > 0 || iNSP_MASKTOOL_PARAM.T1FillletCount3 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount0 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount1 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount2 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount3 > 0)
				{
					btn_Fillet.Background = Brushes.Gold;
				}
				else
				{
					btn_Fillet.Background = Brushes.White;
				}
				cb_T1Filllet0.SelectedIndex = iNSP_MASKTOOL_PARAM.T1FillletCount0;
				cb_T1Filllet1.SelectedIndex = iNSP_MASKTOOL_PARAM.T1FillletCount1;
				cb_T1Filllet2.SelectedIndex = iNSP_MASKTOOL_PARAM.T1FillletCount2;
				cb_T1Filllet3.SelectedIndex = iNSP_MASKTOOL_PARAM.T1FillletCount3;
				cb_T2Filllet0.SelectedIndex = iNSP_MASKTOOL_PARAM.T2FillletCount0;
				cb_T2Filllet1.SelectedIndex = iNSP_MASKTOOL_PARAM.T2FillletCount1;
				cb_T2Filllet2.SelectedIndex = iNSP_MASKTOOL_PARAM.T2FillletCount2;
				cb_T2Filllet3.SelectedIndex = iNSP_MASKTOOL_PARAM.T2FillletCount3;
				SetTracker(rect, rect2);
				LastSearchRect1 = rect;
				LastSearchRect2 = rect2;
				if (iNSP_MASKTOOL_PARAM.TrackerON)
				{
					if (TrackIndex == 0)
					{
						ts_Tracker2AsMask.Visibility = Visibility.Visible;
						ts_Tracker2AsMask.IsChecked = iNSP_MASKTOOL_PARAM.SearchRect2AsMask;
					}
					else
					{
						ts_Tracker2AsMask.Visibility = Visibility.Hidden;
					}
					bor_SearchRect1.Visibility = Visibility.Visible;
					if (iNSP_MASKTOOL_PARAM.SearchRect2ON)
					{
						bor_SearchRect2.Visibility = Visibility.Visible;
						if (TrackIndex != 0)
						{
							txt_Tracker2Pos.Visibility = Visibility.Visible;
						}
						else
						{
							txt_Tracker2Pos.Visibility = Visibility.Hidden;
						}
						ts_Tracker2ON.IsChecked = true;
					}
					else
					{
						bor_SearchRect2.Visibility = Visibility.Hidden;
						txt_Tracker2Pos.Visibility = Visibility.Hidden;
						ts_Tracker2ON.IsChecked = false;
					}
				}
				else
				{
					bor_SearchRect1.Visibility = Visibility.Hidden;
					bor_SearchRect2.Visibility = Visibility.Hidden;
				}
			}
			else
			{
				tab_Function.SelectedIndex = 1;
				if (iNSP_MASKTOOL_PARAM.TrackerON)
				{
					grd_Combine.Visibility = Visibility.Visible;
				}
				else
				{
					grd_Combine.Visibility = Visibility.Hidden;
				}
				cb_TrackerSelectCombine.SelectedIndex = TrackIndex;
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_SearchRect2.Visibility = Visibility.Hidden;
				cb_CombineSelect1.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine1Select;
				cb_CombineDEStart1.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine1DE_Start_Index;
				cb_CombineDEStartCount1.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine1DE_Start_Count;
				cb_CombineRule1.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine1Rule;
				cb_CombineSelect2.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine2Select;
				cb_CombineDEStart2.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine2DE_Start_Index;
				cb_CombineDEStartCount2.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine2DE_Start_Count;
				cb_CombineRule2.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine2Rule;
				cb_CombineSelect3.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine3Select;
				cb_CombineDEStart3.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine3DE_Start_Index;
				cb_CombineDEStartCount3.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine3DE_Start_Count;
				cb_CombineRule3.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine3Rule;
				cb_CombineSelect4.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine4Select;
				cb_CombineDEStart4.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine4DE_Start_Index;
				cb_CombineDEStartCount4.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine4DE_Start_Count;
				cb_CombineRule4.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine4Rule;
				cb_CombineSelect5.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine5Select;
				cb_CombineDEStart5.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine5DE_Start_Index;
				cb_CombineDEStartCount5.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine5DE_Start_Count;
				cb_CombineRule5.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine5Rule;
				cb_CombineSelect6.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine6Select;
				cb_CombineDEStart6.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine6DE_Start_Index;
				cb_CombineDEStartCount6.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine6DE_Start_Count;
				cb_CombineRule6.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine6Rule;
				cb_CombineSelect7.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine7Select;
				cb_CombineDEStart7.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine7DE_Start_Index;
				cb_CombineDEStartCount7.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine7DE_Start_Count;
				cb_CombineRule7.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine7Rule;
				cb_CombineSelect8.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine8Select;
				cb_CombineDEStart8.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine8DE_Start_Index;
				cb_CombineDEStartCount8.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine8DE_Start_Count;
				cb_CombineRule8.SelectedIndex = iNSP_MASKTOOL_PARAM.Combine8Rule;
			}
			UpdateUI = true;
		}
		catch
		{
			string text = "MW03-MaskWindow-CopyValueToUI()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		if (PreviewMode || DontSave)
		{
			return;
		}
		try
		{
			INSP_MASKTOOL_PARAM iNSP_MASKTOOL_PARAM = new INSP_MASKTOOL_PARAM
			{
				ValueChangeFlag = true,
				TrackerON = ts_TrackerON.IsChecked.Value,
				TrackerName = txt_TrackerName.Text
			};
			if (TrackIndex < 20)
			{
				iNSP_MASKTOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.THD1_ChannelON = true;
				iNSP_MASKTOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.THD2_ChannelON = exp_THD2.IsExpanded;
				iNSP_MASKTOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
				iNSP_MASKTOOL_PARAM.THD3_ChannelON = exp_THD3.IsExpanded;
				iNSP_MASKTOOL_PARAM.THD4_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD4.SelectedIndex;
				iNSP_MASKTOOL_PARAM.THD4_ChannelON = exp_THD4.IsExpanded;
				iNSP_MASKTOOL_PARAM.THD1_Min = (int)rs_THD1.LowerValue;
				iNSP_MASKTOOL_PARAM.THD1_Max = (int)rs_THD1.UpperValue;
				iNSP_MASKTOOL_PARAM.THD2_Min = (int)rs_THD2.LowerValue;
				iNSP_MASKTOOL_PARAM.THD2_Max = (int)rs_THD2.UpperValue;
				iNSP_MASKTOOL_PARAM.THD3_Min = (int)rs_THD3.LowerValue;
				iNSP_MASKTOOL_PARAM.THD3_Max = (int)rs_THD3.UpperValue;
				iNSP_MASKTOOL_PARAM.THD4_Min = (int)rs_THD4.LowerValue;
				iNSP_MASKTOOL_PARAM.THD4_Max = (int)rs_THD4.UpperValue;
				iNSP_MASKTOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start4_Index = (USE_DILATE_ERODE)cb_THD4_DEStart.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End4_Index = (USE_DILATE_ERODE)cb_THD4_DEEnd.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_Start4_Count = cb_THD4_DEStartCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_End4_Count = cb_THD4_DEEndCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.FillHole = chk_FillHole.IsChecked.Value;
				iNSP_MASKTOOL_PARAM.doConvexHull = chk_doConvexHull.IsChecked.Value;
				iNSP_MASKTOOL_PARAM.DE_FinalStart_Index = (USE_DILATE_ERODE)cb_FinalDEStart.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_FinalStart_Count = cb_FinalDEStartCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_FinalEnd_Index = (USE_DILATE_ERODE)cb_FinalDEEnd.SelectedIndex;
				iNSP_MASKTOOL_PARAM.DE_FinalEnd_Count = cb_FinalDEEndCount.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex = cb_Track1AlignWayX.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex = cb_Track2AlignWayX.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex = cb_Track1AlignWayY.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex = cb_Track2AlignWayY.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex = cb_Track1AutoSize.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex = cb_Track2AutoSize.SelectedIndex;
				iNSP_MASKTOOL_PARAM.MaxBlob = cb_MaxBlob.SelectedIndex;
				iNSP_MASKTOOL_PARAM.EnableSelectFace = chk_SelectFace.IsChecked.Value;
				iNSP_MASKTOOL_PARAM.FaceIndex = SelectFace;
				iNSP_MASKTOOL_PARAM.EnableAdvLocateMask = chk_UseAdvLocateMask.IsChecked.Value;
				iNSP_MASKTOOL_PARAM.AdvLocateStartIndex = cb_UseAdvLocateMaskStart.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateEndIndex = cb_UseAdvLocateMaskEnd.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateBorder1 = cb_AdvLocateBorder1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateBorder2 = cb_AdvLocateBorder2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateExpand1 = cb_AdvLocateExpand1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateExpand2 = cb_AdvLocateExpand2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateExpandCount1 = cb_AdvLocateExpandCount1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateExpandCount2 = cb_AdvLocateExpandCount2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.AdvLocateBorderFilllet = cb_AdvLocateBorderFillet.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T1FillletCount0 = cb_T1Filllet0.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T1FillletCount1 = cb_T1Filllet1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T1FillletCount2 = cb_T1Filllet2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T1FillletCount3 = cb_T1Filllet3.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T2FillletCount0 = cb_T2Filllet0.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T2FillletCount1 = cb_T2Filllet1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T2FillletCount2 = cb_T2Filllet2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.T2FillletCount3 = cb_T2Filllet3.SelectedIndex;
				if (iNSP_MASKTOOL_PARAM.T1FillletCount0 > 0 || iNSP_MASKTOOL_PARAM.T1FillletCount1 > 0 || iNSP_MASKTOOL_PARAM.T1FillletCount2 > 0 || iNSP_MASKTOOL_PARAM.T1FillletCount3 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount0 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount1 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount2 > 0 || iNSP_MASKTOOL_PARAM.T2FillletCount3 > 0)
				{
					btn_Fillet.Background = Brushes.Gold;
				}
				else
				{
					btn_Fillet.Background = Brushes.White;
				}
				if (rad_AdvLocateAsInsp.IsChecked == true)
				{
					iNSP_MASKTOOL_PARAM.AdvLocateAsInsp = true;
				}
				else
				{
					iNSP_MASKTOOL_PARAM.AdvLocateAsInsp = false;
				}
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
				OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
				GetTracker(ref rect, ref rect2);
				iNSP_MASKTOOL_PARAM.SearchRect2AsMask = ts_Tracker2AsMask.IsChecked.Value;
				iNSP_MASKTOOL_PARAM.SearchRect2ON = ts_Tracker2ON.IsChecked.Value;
				if (rad_Tracker1AutoSize.IsChecked.Value)
				{
					iNSP_MASKTOOL_PARAM.Tracker1UseAutoSize = true;
					rad_Tracker1AutoSize.IsChecked = true;
					iNSP_MASKTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].L;
					iNSP_MASKTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].T;
					iNSP_MASKTOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].Width - iNSP_MASKTOOL_PARAM.Track1_L - rect.Width;
					iNSP_MASKTOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize1_SelectIndex].Height - iNSP_MASKTOOL_PARAM.Track1_T - rect.Height;
				}
				else
				{
					iNSP_MASKTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[iNSP_MASKTOOL_PARAM.Tracker1AligmentXIndex].Value;
					iNSP_MASKTOOL_PARAM.Track1_R = iNSP_MASKTOOL_PARAM.Track1_L + rect.Width;
					iNSP_MASKTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[iNSP_MASKTOOL_PARAM.Tracker1AligmentYIndex].Value;
					iNSP_MASKTOOL_PARAM.Track1_B = iNSP_MASKTOOL_PARAM.Track1_T + rect.Height;
					iNSP_MASKTOOL_PARAM.Tracker1UseAutoSize = false;
				}
				if (rad_Tracker2AutoSize.IsChecked.Value)
				{
					iNSP_MASKTOOL_PARAM.Tracker2UseAutoSize = true;
					rad_Tracker2AutoSize.IsChecked = true;
					iNSP_MASKTOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].L;
					iNSP_MASKTOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].T;
					iNSP_MASKTOOL_PARAM.Track2_R = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].Width - iNSP_MASKTOOL_PARAM.Track2_L - rect2.Width;
					iNSP_MASKTOOL_PARAM.Track2_B = inspection.paramFile.AdvLocateParam[iNSP_MASKTOOL_PARAM.AutoSize2_SelectIndex].Height - iNSP_MASKTOOL_PARAM.Track2_T - rect2.Height;
				}
				else
				{
					iNSP_MASKTOOL_PARAM.Track2_L = rect2.X - inspection.paramFile.VSLocateParam[iNSP_MASKTOOL_PARAM.Tracker2AligmentXIndex].Value;
					iNSP_MASKTOOL_PARAM.Track2_R = iNSP_MASKTOOL_PARAM.Track2_L + rect2.Width;
					iNSP_MASKTOOL_PARAM.Track2_T = rect2.Y - inspection.paramFile.HSLocateParam[iNSP_MASKTOOL_PARAM.Tracker2AligmentYIndex].Value;
					iNSP_MASKTOOL_PARAM.Track2_B = iNSP_MASKTOOL_PARAM.Track2_T + rect2.Height;
					iNSP_MASKTOOL_PARAM.Tracker2UseAutoSize = false;
				}
			}
			else
			{
				iNSP_MASKTOOL_PARAM.SearchRect2ON = false;
				iNSP_MASKTOOL_PARAM.Combine1Select = cb_CombineSelect1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine1DE_Start_Index = cb_CombineDEStart1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine1DE_Start_Count = cb_CombineDEStartCount1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine1Rule = cb_CombineRule1.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine2Select = cb_CombineSelect2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine2DE_Start_Index = cb_CombineDEStart2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine2DE_Start_Count = cb_CombineDEStartCount2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine2Rule = cb_CombineRule2.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine3Select = cb_CombineSelect3.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine3DE_Start_Index = cb_CombineDEStart3.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine3DE_Start_Count = cb_CombineDEStartCount3.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine3Rule = cb_CombineRule3.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine4Select = cb_CombineSelect4.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine4DE_Start_Index = cb_CombineDEStart4.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine4DE_Start_Count = cb_CombineDEStartCount4.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine4Rule = cb_CombineRule4.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine5Select = cb_CombineSelect5.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine5DE_Start_Index = cb_CombineDEStart5.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine5DE_Start_Count = cb_CombineDEStartCount5.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine5Rule = cb_CombineRule5.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine6Select = cb_CombineSelect6.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine6DE_Start_Index = cb_CombineDEStart6.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine6DE_Start_Count = cb_CombineDEStartCount6.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine6Rule = cb_CombineRule6.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine7Select = cb_CombineSelect7.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine7DE_Start_Index = cb_CombineDEStart7.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine7DE_Start_Count = cb_CombineDEStartCount7.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine7Rule = cb_CombineRule7.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine8Select = cb_CombineSelect8.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine8DE_Start_Index = cb_CombineDEStart8.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine8DE_Start_Count = cb_CombineDEStartCount8.SelectedIndex;
				iNSP_MASKTOOL_PARAM.Combine8Rule = cb_CombineRule8.SelectedIndex;
			}
			UpdateUI = false;
			int selectedIndex = cb_TrackerSelect.SelectedIndex;
			cb_TrackerSelect.Items.Clear();
			for (int i = 0; i < 20; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.MaskParam[i].TrackerName);
			}
			cb_TrackerSelect.SelectedIndex = selectedIndex;
			UpdateUI = true;
			iNSP_MASKTOOL_PARAM.ValueChangeFlag = false;
			if (!iNSP_MASKTOOL_PARAM.Equals(inspection.paramFile_Backup.MaskParam[TrackIndex]))
			{
				iNSP_MASKTOOL_PARAM.ValueChangeFlag = true;
				inspection.paramFile.MaskParam[TrackIndex] = iNSP_MASKTOOL_PARAM;
			}
		}
		catch (Exception ex)
		{
			UpdateUI = true;
			string text = "MW05-MaskWindow-WriteValueToParam()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ColorPaste_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Binary value will be overwrite！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("二值化色阶的数值将被覆盖！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("二值化色階的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
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
			ReferenceShow = false;
		}
		else
		{
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
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetTracker(ref rect, ref rect2);
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
			OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, 0, 0);
			OpenCvSharp.Rect rect4 = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetTracker(ref rect3, ref rect4);
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

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Bor_SearchRect1_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
		}
	}

	private void Btn_AdvLocateMaskShow_Click(object sender, RoutedEventArgs e)
	{
		if (!AdvLocateMaskShow)
		{
			exp_UseAdvLocateMask.Visibility = Visibility.Visible;
			exp_UseAdvLocateMask.IsExpanded = true;
			AdvLocateMaskShow = true;
		}
		else
		{
			exp_UseAdvLocateMask.Visibility = Visibility.Hidden;
			AdvLocateMaskShow = false;
		}
	}

	private void Exp_UseAdvLocateMask_Collapsed(object sender, RoutedEventArgs e)
	{
		if (chk_UseAdvLocateMask.IsChecked == true)
		{
			btn_AdvLocateMaskShow.Background = Brushes.Gold;
		}
		else
		{
			btn_AdvLocateMaskShow.Background = Brushes.White;
		}
		exp_UseAdvLocateMask.Visibility = Visibility.Hidden;
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

	private void Cnv_Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
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

	private void Chk_UseAdvLocateMask_Checked(object sender, RoutedEventArgs e)
	{
		btn_AdvLocateMaskShow.Background = Brushes.Gold;
	}

	private void Chk_UseAdvLocateMask_Unchecked(object sender, RoutedEventArgs e)
	{
		btn_AdvLocateMaskShow.Background = Brushes.White;
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
			DoThdImage();
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
			DoThdImage();
		}
	}

	private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (inited)
		{
			DoThdImage();
		}
	}

	private void Tab_Function_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && e.Source is TabControl)
		{
			tab_Mask.Background = Brushes.White;
			tab_MaskCombine.Background = Brushes.White;
			if (tab_Function.SelectedIndex == 0)
			{
				ChangeMaskFunction(tab_Function.SelectedIndex, TrackIndex);
			}
			else
			{
				ChangeMaskFunction(tab_Function.SelectedIndex, 20);
			}
		}
	}

	private void ChangeMaskFunction(int function, int index, bool write = true)
	{
		if (function == 0)
		{
			if (write)
			{
				WriteUIValueToParam();
			}
			tab_Mask.Background = Brushes.Gold;
			for (int i = 0; i < 30; i++)
			{
				if (i < 20)
				{
					ToolsButton[i].IsEnabled = true;
				}
				else
				{
					ToolsButton[i].IsEnabled = false;
				}
			}
			UpdateUI = false;
			if (write)
			{
				TrackIndex = 0;
			}
			CopyValueToUI();
			UpdateUI = true;
			if (write)
			{
				Btn_Test_Click(this, null);
			}
			return;
		}
		Mat mat = new Mat();
		if (write)
		{
			WriteUIValueToParam();
		}
		inspection.Inspect(ref mat);
		tab_MaskCombine.Background = Brushes.Gold;
		for (int j = 0; j < 30; j++)
		{
			if (j < 20)
			{
				ToolsButton[j].IsEnabled = false;
			}
			else
			{
				ToolsButton[j].IsEnabled = true;
			}
			if (inspection.paramFile.MaskParam[j].TrackerON)
			{
				ToolsButton[j].Background = Brushes.Lime;
			}
			else
			{
				ToolsButton[j].Background = Brushes.White;
			}
		}
		UpdateUI = false;
		UpdateMaskComboBox();
		TrackIndex = index;
		ToolsButton[20].BorderBrush = Brushes.Blue;
		cb_TrackerSelectCombine.SelectedIndex = index - 20;
		CopyValueToUI();
		UpdateUI = true;
		if (write)
		{
			Btn_Test_Click(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateMaskComboBox()
	{
		for (int i = 0; i < 8; i++)
		{
			CombineSelect[i].Items.Clear();
			CombineSelect[i].Items.Add("-----");
			for (int j = 0; j < 20; j++)
			{
				if (inspection.paramFile.MaskParam[j].TrackerON)
				{
					CombineSelect[i].Items.Add(j + 1 + " : " + inspection.paramFile.MaskParam[j].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = j + 1 + " : " + inspection.paramFile.MaskParam[j].TrackerName;
				comboBoxItem.IsEnabled = false;
				CombineSelect[i].Items.Add(comboBoxItem);
			}
		}
	}

	private void Btn_ChannelPreview1_MouseEnter(object sender, MouseEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (MaskPreview[num] != null && MaskPreview[num].Width > 0 && MaskPreview[num].Height > 0)
		{
			new Mat();
			Mat image = MaskPreview[num].Clone();
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
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	private void Btn_ChannelBinaryPreview1_MouseEnter(object sender, MouseEventArgs e)
	{
	}

	private void Cb_CombineSelect1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			DoThdImage();
		}
	}

	private void Cb_TrackerSelectCombine_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam();
			TrackIndex = cb_TrackerSelectCombine.SelectedIndex + 20;
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
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
	private void Btn_OTSU_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = cb_Channel_THD1.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect, ref rect2);
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
			string text = "BW10-MaskWindow-Btn_OTSUL_Click()-";
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
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode)
		{
			return;
		}
		lab_Time.Content = "0";
		if (!inspection.buffers.LocatedReady)
		{
			return;
		}
		exp_TrackerFilllet.Visibility = Visibility.Hidden;
		txt_AllResult.Clear();
		if (DontSave)
		{
			return;
		}
		OpenCvSharp.Rect[] array = new OpenCvSharp.Rect[2];
		GetTracker(ref array[0], ref array[1]);
		TimeStart();
		inspection.maskTool.Inspect(TrackIndex);
		double num = TimeStop();
		lab_Time.Content = string.Format("{0:N3}", num);
		WriteUIValueToParam();
		if (chk_SelectFace.IsChecked == true && FaceIndex != 0 && FaceIndex != SelectFace)
		{
			Label_ResultError.Visibility = Visibility.Visible;
			if (LanguageType == 0)
			{
				Label_ResultError.Content = "Select Face Doesn't Match! ByPass!";
			}
			else if (LanguageType == 1)
			{
				Label_ResultError.Content = "選面不符合，ByPass!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Content = "选面不符合，ByPass!";
			}
		}
		else
		{
			Label_ResultError.Visibility = Visibility.Hidden;
			DoThdImage();
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void DoBorderExpend(ref OpenCvSharp.Rect rect, int BorderType, int ExpendType, int count)
	{
		OpenCvSharp.Rect rect2 = rect;
		if (ExpendType != 0)
		{
			count *= -1;
		}
		switch (BorderType)
		{
		case 0:
			rect.Left -= count;
			rect.Top -= count;
			rect.Width += count * 2;
			rect.Height += count * 2;
			break;
		case 1:
			rect.Top -= count;
			rect.Height += count * 2;
			break;
		default:
			rect.Left -= count;
			rect.Width += count * 2;
			break;
		}
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			rect = rect2;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/maskwindow.xaml", UriKind.Relative);
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
			MainW = (MaskWindow)target;
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
			Label_Result = (Label)target;
			break;
		case 9:
			bor_LocateRect = (Border)target;
			break;
		case 10:
			bor_SearchRect2 = (Border)target;
			bor_SearchRect2.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect2.GotFocus += Bor_SearchRect2_GotFocus;
			bor_SearchRect2.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 11:
			txt_Search2Label = (TextBlock)target;
			break;
		case 12:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.GotFocus += Bor_SearchRect1_GotFocus;
			bor_SearchRect1.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 13:
			txt_Search1Label = (TextBlock)target;
			break;
		case 14:
			Label_ResultError = (Label)target;
			break;
		case 15:
			rt_ReferenceRect1 = (Rectangle)target;
			break;
		case 16:
			rt_ReferenceRect2 = (Rectangle)target;
			break;
		case 17:
			txt_PreviewMode = (TextBox)target;
			break;
		case 18:
			tab_Function = (TabControl)target;
			tab_Function.SelectionChanged += Tab_Function_SelectionChanged;
			break;
		case 19:
			tab_Mask = (TabItem)target;
			break;
		case 20:
			grd_SIMP_Setup = (Grid)target;
			break;
		case 21:
			chk_FillHole = (CheckBox)target;
			break;
		case 22:
			chk_doConvexHull = (CheckBox)target;
			break;
		case 23:
			cb_MaxBlob = (ComboBox)target;
			cb_MaxBlob.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 24:
			cb_TrackerSelect = (ComboBox)target;
			cb_TrackerSelect.SelectionChanged += Cb_TrackerSelect_SelectionChanged;
			break;
		case 25:
			scr_TrackerSwich = (ScrollBar)target;
			scr_TrackerSwich.SizeChanged += Scr_TrackerSwich_SizeChanged;
			scr_TrackerSwich.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 26:
			lab_Time = (Label)target;
			break;
		case 27:
			exp_THD1 = (Expander)target;
			break;
		case 28:
			exp_THD2 = (Expander)target;
			break;
		case 29:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 30:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 31:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 32:
			cb_THD2_DEStart = (ComboBox)target;
			cb_THD2_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 33:
			cb_THD2_DEEnd = (ComboBox)target;
			cb_THD2_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 34:
			cb_THD2_DEEndCount = (ComboBox)target;
			cb_THD2_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 35:
			cb_THD2_DEStartCount = (ComboBox)target;
			cb_THD2_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 36:
			cb_Channel_THD2 = (ComboBox)target;
			break;
		case 37:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 38:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 39:
			exp_THD3 = (Expander)target;
			break;
		case 40:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 41:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 42:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 43:
			cb_THD3_DEStart = (ComboBox)target;
			cb_THD3_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 44:
			cb_THD3_DEEnd = (ComboBox)target;
			cb_THD3_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 45:
			cb_THD3_DEEndCount = (ComboBox)target;
			cb_THD3_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 46:
			cb_THD3_DEStartCount = (ComboBox)target;
			cb_THD3_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 47:
			cb_Channel_THD3 = (ComboBox)target;
			break;
		case 48:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 49:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 50:
			exp_THD4 = (Expander)target;
			break;
		case 51:
			grd_SIMP_THD_4 = (Grid)target;
			break;
		case 52:
			rs_THD4 = (RangeSlider)target;
			rs_THD4.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD4.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD4.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 53:
			cb_Channel_THD4 = (ComboBox)target;
			break;
		case 54:
			grd_SIMP_DilateErode4 = (Grid)target;
			break;
		case 55:
			cb_THD4_DEStart = (ComboBox)target;
			cb_THD4_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 56:
			cb_THD4_DEEnd = (ComboBox)target;
			cb_THD4_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 57:
			cb_THD4_DEEndCount = (ComboBox)target;
			cb_THD4_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 58:
			cb_THD4_DEStartCount = (ComboBox)target;
			cb_THD4_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 59:
			nu_Thd4LimitMin = (NumericUpDown)target;
			nu_Thd4LimitMin.ValueChanged += Nu_Thd4LimitMin_ValueChanged;
			break;
		case 60:
			nu_Thd4LimitMax = (NumericUpDown)target;
			nu_Thd4LimitMax.ValueChanged += Nu_Thd4LimitMax_ValueChanged;
			break;
		case 61:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 62:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 63:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 64:
			cb_THD1_DEStart = (ComboBox)target;
			cb_THD1_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 65:
			cb_THD1_DEEnd = (ComboBox)target;
			cb_THD1_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 66:
			cb_THD1_DEEndCount = (ComboBox)target;
			cb_THD1_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 67:
			cb_THD1_DEStartCount = (ComboBox)target;
			cb_THD1_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 68:
			cb_Channel_THD1 = (ComboBox)target;
			break;
		case 69:
			btn_ColorCopy = (Button)target;
			btn_ColorCopy.Click += Btn_ColorCopy_Click;
			break;
		case 70:
			btn_ColorPaste = (Button)target;
			btn_ColorPaste.Click += Btn_ColorPaste_Click;
			break;
		case 71:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 72:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 73:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSU_Click;
			break;
		case 74:
			grd_Aligment = (Grid)target;
			break;
		case 75:
			cb_Track1AlignWayX = (ComboBox)target;
			cb_Track1AlignWayX.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 76:
			rad_Tracker1AutoSize = (RadioButton)target;
			rad_Tracker1AutoSize.Click += Rad_Tracker1AutoSize_Click;
			break;
		case 77:
			cb_Track1AutoSize = (ComboBox)target;
			cb_Track1AutoSize.SelectionChanged += Cb_Track1AutoSize_SelectionChanged;
			break;
		case 78:
			rad_Tracker1AlignSelect = (RadioButton)target;
			rad_Tracker1AlignSelect.Click += Rad_Tracker1AlignSelect_Click;
			break;
		case 79:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 80:
			cb_Track1AlignWayY = (ComboBox)target;
			cb_Track1AlignWayY.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 81:
			btn_Tracker1SizeChange = (Button)target;
			btn_Tracker1SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 82:
			txt_Tracker1Pos = (Label)target;
			break;
		case 83:
			grd_Track2Setup = (Grid)target;
			break;
		case 84:
			cb_Track2AlignWayX = (ComboBox)target;
			cb_Track2AlignWayX.SelectionChanged += Cb_Track2AlignWayX_SelectionChanged;
			break;
		case 85:
			rad_Tracker2AutoSize = (RadioButton)target;
			rad_Tracker2AutoSize.Click += Rad_Tracker2AutoSize_Click;
			break;
		case 86:
			cb_Track2AutoSize = (ComboBox)target;
			cb_Track2AutoSize.SelectionChanged += Cb_Track2AutoSize_SelectionChanged;
			break;
		case 87:
			rad_Tracker2AlignSelect = (RadioButton)target;
			rad_Tracker2AlignSelect.Click += Rad_Tracker2AlignSelect_Click;
			break;
		case 88:
			cb_Track2AlignWayY = (ComboBox)target;
			break;
		case 89:
			btn_TrackerReset_Copy = (Button)target;
			btn_TrackerReset_Copy.Click += Btn_TrackerReset_Click;
			break;
		case 90:
			btn_Tracker2SizeChange = (Button)target;
			btn_Tracker2SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 91:
			txt_Tracker2Pos = (Label)target;
			break;
		case 92:
			ts_Tracker2AsMask = (CheckBox)target;
			break;
		case 93:
			ts_Tracker2ON = (CheckBox)target;
			ts_Tracker2ON.Click += Ts_Tracker2ON_Click;
			break;
		case 94:
			tab_MaskCombine = (TabItem)target;
			break;
		case 95:
			cb_TrackerSelectCombine = (ComboBox)target;
			cb_TrackerSelectCombine.SelectionChanged += Cb_TrackerSelectCombine_SelectionChanged;
			break;
		case 96:
			scr_TrackerSwich1 = (ScrollBar)target;
			scr_TrackerSwich1.SizeChanged += Scr_TrackerSwich_SizeChanged;
			scr_TrackerSwich1.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 97:
			grd_Combine = (Grid)target;
			break;
		case 98:
			cb_CombineSelect1 = (ComboBox)target;
			cb_CombineSelect1.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 99:
			lab_TrackIndex0 = (Label)target;
			break;
		case 100:
			btn_BinaryPreview1 = (Button)target;
			btn_BinaryPreview1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 101:
			btn_MaskPreview1 = (Button)target;
			btn_MaskPreview1.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 102:
			cb_CombineDEStart1 = (ComboBox)target;
			cb_CombineDEStart1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 103:
			cb_CombineDEStartCount1 = (ComboBox)target;
			cb_CombineDEStartCount1.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 104:
			cb_CombineRule1 = (ComboBox)target;
			cb_CombineRule1.SelectionChanged += Cb_TrackerSelect_SelectionChanged;
			break;
		case 105:
			cb_CombineSelect2 = (ComboBox)target;
			cb_CombineSelect2.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 106:
			lab_TrackIndex1 = (Label)target;
			break;
		case 107:
			cb_CombineRule2 = (ComboBox)target;
			cb_CombineRule2.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 108:
			btn_BinaryPreview2 = (Button)target;
			btn_BinaryPreview2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview2.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 109:
			btn_MaskPreview1_Copy = (Button)target;
			btn_MaskPreview1_Copy.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 110:
			cb_CombineDEStart2 = (ComboBox)target;
			cb_CombineDEStart2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 111:
			cb_CombineDEStartCount2 = (ComboBox)target;
			cb_CombineDEStartCount2.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 112:
			cb_CombineSelect3 = (ComboBox)target;
			cb_CombineSelect3.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 113:
			lab_TrackIndex2 = (Label)target;
			break;
		case 114:
			cb_CombineRule3 = (ComboBox)target;
			cb_CombineRule3.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 115:
			btn_BinaryPreview1_Copy1 = (Button)target;
			btn_BinaryPreview1_Copy1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1_Copy1.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 116:
			btn_MaskPreview1_Copy1 = (Button)target;
			btn_MaskPreview1_Copy1.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 117:
			cb_CombineDEStart3 = (ComboBox)target;
			cb_CombineDEStart3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 118:
			cb_CombineDEStartCount3 = (ComboBox)target;
			cb_CombineDEStartCount3.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 119:
			cb_CombineSelect4 = (ComboBox)target;
			cb_CombineSelect4.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 120:
			lab_TrackIndex3 = (Label)target;
			break;
		case 121:
			cb_CombineRule4 = (ComboBox)target;
			cb_CombineRule4.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 122:
			btn_BinaryPreview1_Copy2 = (Button)target;
			btn_BinaryPreview1_Copy2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1_Copy2.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 123:
			btn_MaskPreview1_Copy2 = (Button)target;
			btn_MaskPreview1_Copy2.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 124:
			cb_CombineDEStart4 = (ComboBox)target;
			cb_CombineDEStart4.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 125:
			cb_CombineDEStartCount4 = (ComboBox)target;
			cb_CombineDEStartCount4.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 126:
			cb_CombineSelect5 = (ComboBox)target;
			cb_CombineSelect5.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 127:
			lab_TrackIndex4 = (Label)target;
			break;
		case 128:
			cb_CombineRule5 = (ComboBox)target;
			cb_CombineRule5.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 129:
			btn_BinaryPreview1_Copy3 = (Button)target;
			btn_BinaryPreview1_Copy3.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1_Copy3.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 130:
			btn_MaskPreview1_Copy3 = (Button)target;
			btn_MaskPreview1_Copy3.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy3.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 131:
			cb_CombineDEStart5 = (ComboBox)target;
			cb_CombineDEStart5.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 132:
			cb_CombineDEStartCount5 = (ComboBox)target;
			cb_CombineDEStartCount5.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 133:
			cb_CombineSelect6 = (ComboBox)target;
			cb_CombineSelect6.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 134:
			lab_TrackIndex5 = (Label)target;
			break;
		case 135:
			cb_CombineRule6 = (ComboBox)target;
			cb_CombineRule6.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 136:
			btn_BinaryPreview1_Copy4 = (Button)target;
			btn_BinaryPreview1_Copy4.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1_Copy4.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 137:
			btn_MaskPreview1_Copy4 = (Button)target;
			btn_MaskPreview1_Copy4.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy4.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 138:
			cb_CombineDEStart6 = (ComboBox)target;
			cb_CombineDEStart6.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 139:
			cb_CombineDEStartCount6 = (ComboBox)target;
			cb_CombineDEStartCount6.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 140:
			cb_CombineSelect7 = (ComboBox)target;
			cb_CombineSelect7.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 141:
			lab_TrackIndex6 = (Label)target;
			break;
		case 142:
			cb_CombineRule7 = (ComboBox)target;
			cb_CombineRule7.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 143:
			btn_BinaryPreview1_Copy5 = (Button)target;
			btn_BinaryPreview1_Copy5.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1_Copy5.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 144:
			btn_MaskPreview1_Copy5 = (Button)target;
			btn_MaskPreview1_Copy5.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy5.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 145:
			cb_CombineDEStart7 = (ComboBox)target;
			cb_CombineDEStart7.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 146:
			cb_CombineDEStartCount7 = (ComboBox)target;
			cb_CombineDEStartCount7.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 147:
			cb_CombineSelect8 = (ComboBox)target;
			cb_CombineSelect8.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 148:
			lab_TrackIndex7 = (Label)target;
			break;
		case 149:
			cb_CombineRule8 = (ComboBox)target;
			cb_CombineRule8.SelectionChanged += Cb_CombineSelect1_SelectionChanged;
			break;
		case 150:
			btn_BinaryPreview1_Copy6 = (Button)target;
			btn_BinaryPreview1_Copy6.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_BinaryPreview1_Copy6.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 151:
			btn_MaskPreview1_Copy6 = (Button)target;
			btn_MaskPreview1_Copy6.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_MaskPreview1_Copy6.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 152:
			cb_CombineDEStart8 = (ComboBox)target;
			cb_CombineDEStart8.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 153:
			cb_CombineDEStartCount8 = (ComboBox)target;
			cb_CombineDEStartCount8.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 154:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 155:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 156:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 157:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 158:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 159:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 160:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 161:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 162:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 163:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 164:
			btn_ImageNum = (Button)target;
			break;
		case 165:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 166:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 167:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 168:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 169:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 170:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 171:
			lab_RValue = (Label)target;
			break;
		case 172:
			lab_GValue = (Label)target;
			break;
		case 173:
			lab_BValue = (Label)target;
			break;
		case 174:
			lab_HValue = (Label)target;
			break;
		case 175:
			lab_SValue = (Label)target;
			break;
		case 176:
			lab_VValue = (Label)target;
			break;
		case 177:
			lab_GrayValue = (Label)target;
			break;
		case 178:
			lab_XValue = (Label)target;
			break;
		case 179:
			lab_YValue = (Label)target;
			break;
		case 180:
			lab_C1Value = (Label)target;
			break;
		case 181:
			lab_C2Value = (Label)target;
			break;
		case 182:
			lab_C3Value = (Label)target;
			break;
		case 183:
			lab_C4Value = (Label)target;
			break;
		case 184:
			lab_C5Value = (Label)target;
			break;
		case 185:
			lab_C6Value = (Label)target;
			break;
		case 186:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 187:
			txt_AllResult = (TextBox)target;
			break;
		case 188:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 189:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 190:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 191:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 192:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 193:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 194:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 195:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 196:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 197:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 198:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 199:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 200:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 201:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 202:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 203:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 204:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 205:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 206:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 207:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 208:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 209:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 210:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 211:
			btn_ReferenceShow = (Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 212:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 213:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 214:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 215:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 216:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 217:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 218:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 219:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 220:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 221:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 222:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 223:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 224:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 225:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 226:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 227:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 228:
			grd_TrackerAdj = (Grid)target;
			break;
		case 229:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 230:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 231:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 232:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 233:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 234:
			lab_Tracker1 = (Label)target;
			break;
		case 235:
			lab_Tracker2 = (Label)target;
			break;
		case 236:
			btn_AdvLocateMaskShow = (Button)target;
			btn_AdvLocateMaskShow.Click += Btn_AdvLocateMaskShow_Click;
			break;
		case 237:
			lab_FaceIndex = (Label)target;
			break;
		case 238:
			exp_TrackerFilllet = (Expander)target;
			exp_TrackerFilllet.Collapsed += Exp_TrackerFilllet_Collapsed;
			break;
		case 239:
			((Grid)target).PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 240:
			cb_T1Filllet0 = (ComboBox)target;
			cb_T1Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 241:
			cb_T1Filllet1 = (ComboBox)target;
			cb_T1Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 242:
			cb_T1Filllet2 = (ComboBox)target;
			cb_T1Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 243:
			cb_T1Filllet3 = (ComboBox)target;
			cb_T1Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 244:
			grd_Tracker2Filllet = (Grid)target;
			grd_Tracker2Filllet.PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 245:
			cb_T2Filllet0 = (ComboBox)target;
			cb_T2Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 246:
			cb_T2Filllet1 = (ComboBox)target;
			cb_T2Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 247:
			cb_T2Filllet2 = (ComboBox)target;
			cb_T2Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 248:
			cb_T2Filllet3 = (ComboBox)target;
			cb_T2Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 249:
			btn_Fillet = (Button)target;
			btn_Fillet.Click += Btn_Fillet_Click;
			break;
		case 250:
			btn_Tools21 = (Button)target;
			btn_Tools21.Click += Btn_Tools1_Click;
			break;
		case 251:
			btn_Tools22 = (Button)target;
			btn_Tools22.Click += Btn_Tools1_Click;
			break;
		case 252:
			btn_Tools23 = (Button)target;
			btn_Tools23.Click += Btn_Tools1_Click;
			break;
		case 253:
			btn_Tools24 = (Button)target;
			btn_Tools24.Click += Btn_Tools1_Click;
			break;
		case 254:
			btn_Tools25 = (Button)target;
			btn_Tools25.Click += Btn_Tools1_Click;
			break;
		case 255:
			btn_Tools26 = (Button)target;
			btn_Tools26.Click += Btn_Tools1_Click;
			break;
		case 256:
			btn_Tools27 = (Button)target;
			btn_Tools27.Click += Btn_Tools1_Click;
			break;
		case 257:
			btn_Tools28 = (Button)target;
			btn_Tools28.Click += Btn_Tools1_Click;
			break;
		case 258:
			btn_Tools29 = (Button)target;
			btn_Tools29.Click += Btn_Tools1_Click;
			break;
		case 259:
			btn_Tools30 = (Button)target;
			btn_Tools30.Click += Btn_Tools1_Click;
			break;
		case 260:
			grd_SelectFace = (Grid)target;
			break;
		case 261:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 262:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 263:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 264:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 265:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 266:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 267:
			cb_FinalDEStart = (ComboBox)target;
			cb_FinalDEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 268:
			cb_FinalDEEnd = (ComboBox)target;
			cb_FinalDEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 269:
			cb_FinalDEEndCount = (ComboBox)target;
			cb_FinalDEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 270:
			cb_FinalDEStartCount = (ComboBox)target;
			cb_FinalDEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 271:
			txt_TrackerName = (TextBox)target;
			break;
		case 272:
			ts_TrackerON = (ToggleSwitch)target;
			ts_TrackerON.Click += Ts_TrackerON_Click;
			break;
		case 273:
			btn_Reverse = (Button)target;
			btn_Reverse.Click += Btn_Reverse_Click;
			break;
		case 274:
			btn_RecipeCopy = (Button)target;
			btn_RecipeCopy.Click += Btn_RecipeCopy_Click;
			break;
		case 275:
			btn_RecipePaste = (Button)target;
			btn_RecipePaste.Click += Btn_RecipePaste_Click;
			break;
		case 276:
			lab_TrackIndex = (Label)target;
			break;
		case 277:
			Label_Index = (Label)target;
			break;
		case 278:
			exp_UseAdvLocateMask = (Expander)target;
			exp_UseAdvLocateMask.Collapsed += Exp_UseAdvLocateMask_Collapsed;
			break;
		case 279:
			chk_UseAdvLocateMask = (CheckBox)target;
			chk_UseAdvLocateMask.Checked += Chk_UseAdvLocateMask_Checked;
			chk_UseAdvLocateMask.Unchecked += Chk_UseAdvLocateMask_Unchecked;
			break;
		case 280:
			cb_UseAdvLocateMaskStart = (ComboBox)target;
			break;
		case 281:
			cb_UseAdvLocateMaskEnd = (ComboBox)target;
			break;
		case 282:
			cb_AdvLocateBorder1 = (ComboBox)target;
			break;
		case 283:
			cb_AdvLocateExpand1 = (ComboBox)target;
			break;
		case 284:
			cb_AdvLocateExpandCount1 = (ComboBox)target;
			break;
		case 285:
			cb_AdvLocateBorder2 = (ComboBox)target;
			break;
		case 286:
			cb_AdvLocateExpand2 = (ComboBox)target;
			break;
		case 287:
			cb_AdvLocateExpandCount2 = (ComboBox)target;
			break;
		case 288:
			cb_AdvLocateBorderFillet = (ComboBox)target;
			break;
		case 289:
			rad_AdvLocateAsInsp = (RadioButton)target;
			break;
		case 290:
			rad_AdvLocateAsMask = (RadioButton)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
