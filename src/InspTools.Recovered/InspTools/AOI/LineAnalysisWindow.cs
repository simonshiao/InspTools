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

public class LineAnalysisWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private const int MaxThdTool = 3;

	private int LanguageType = 1;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private bool ShowMessage = true;

	private bool TrackerShow = true;

	private byte[] CurrentColor = new byte[16];

	private bool[] SearchRectResult = new bool[2];

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private OpenCvSharp.Rect[] MaskRect = new OpenCvSharp.Rect[2];

	private BlobStruct[] Blobs = new BlobStruct[2];

	private INSP_LINEANALYSISTOOL_PARAM CopyParam;

	private INSP_LINEANALYSISTOOL_PARAM BackupParam;

	private bool CopyParamSeted;

	private bool DontSave;

	private bool ReferenceShow = true;

	private int SelectFace;

	private int FaceIndex;

	private bool TrackerAdjShow;

	private Stopwatch sw = new Stopwatch();

	private ComboBox[] DEStart;

	private ComboBox[] DEStartCount;

	private ComboBox[] DEEnd;

	private ComboBox[] DEEndCount;

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat[] BinaryBlob = new Mat[2];

	private Mat BinaryT2 = new Mat();

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

	private RangeSlider[] THD_Sliders;

	private NumericUpDown[] THD_Min;

	private NumericUpDown[] THD_Max;

	private ComboBox[] THD_Channel;

	private int TrackIndex;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private Button[] ToolsButton;

	private bool PreviewMode;

	private User_Permissions User = User_Permissions.USER_Administrator;

	private bool OTSU_White = true;

	internal LineAnalysisWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Line line_X1;

	internal Line line_Y1;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Label Label_Result;

	internal Rectangle rt_Mask1;

	internal Rectangle rt_ReferenceRect1;

	internal Rectangle rt_Mask2;

	internal Label Label_ResultError;

	internal Label lab_FaceIndex;

	internal TextBox txt_PreviewMode;

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

	internal GroupBox gb_ConditionSetup;

	internal Grid grd_ConditionSetup;

	internal ComboBox cb_ErrorBin;

	internal Rectangle rt_Result;

	internal Button btn_Test;

	internal TextBox txt_ContError;

	internal TextBox txt_TotalError;

	internal TextBox txt_ContErrorResult;

	internal TextBox txt_TotalErrorResult;

	internal CheckBox chk_ShowScanOK;

	internal CheckBox chk_ShowScanNG;

	internal TextBox txt_AngleMin;

	internal TextBox txt_AngleMax;

	internal TextBox txt_AngleResult;

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal ComboBox cb_LineSegment;

	internal CheckBox chk_EnableAngleCheck;

	internal CheckBox chk_EnablePointCheck;

	internal NumericUpDown nu_P2PInterval;

	internal NumericUpDown nu_P2PAllowValue;

	internal TextBox txt_LineAngleDiff;

	internal CheckBox chk_EnableDistanceCheck;

	internal ComboBox cb_DistanceLineSegment;

	internal TextBox txt_AllowDistance;

	internal TextBox txt_AllowDistanceErrorCount;

	internal TextBox txt_AllowDistanceErrorResult;

	internal TextBox txt_DistanceContError;

	internal TextBox txt_DistanceContErrorResult;

	internal CheckBox chk_UseStdFilter;

	internal Expander exp_THD23;

	internal Grid grd_SIMP_THD_1;

	internal RangeSlider rs_THD2;

	internal NumericUpDown nu_THD2Max;

	internal NumericUpDown nu_THD2Min;

	internal ComboBox cb_Channel_THD2;

	internal RangeSlider rs_THD3;

	internal NumericUpDown nu_THD3Max;

	internal NumericUpDown nu_THD3Min;

	internal ComboBox cb_Channel_THD3;

	internal ComboBox cb_THD2_DEStart;

	internal ComboBox cb_THD2_DEStartCount;

	internal ComboBox cb_THD3_DEStart;

	internal ComboBox cb_THD3_DEStartCount;

	internal ComboBox cb_THD2_DEEnd;

	internal ComboBox cb_THD2_DEEndCount;

	internal ComboBox cb_THD3_DEEnd;

	internal ComboBox cb_THD3_DEEndCount;

	internal GroupBox gb_BasicSetup3;

	internal Grid grd_SIMP_DilateErode1_Copy;

	internal ComboBox cb_Track1AlignWayX;

	internal RadioButton rad_Tracker1AutoSize;

	internal ComboBox cb_Track1AutoSize;

	internal RadioButton rad_Tracker1AlignSelect;

	internal Button btn_TrackerReset;

	internal ComboBox cb_Track1AlignWayY;

	internal Button btn_Tracker1SizeChange;

	internal Label txt_Tracker1Pos;

	internal ComboBox cb_MaskSelect;

	internal Button btn_MaskEdit;

	internal CheckBox chk_UseMaskAsInspArea;

	internal GroupBox gb_BasicSetup1;

	internal Grid grd_SIMP_THD_3;

	internal RangeSlider rs_THD1;

	internal NumericUpDown nu_THD1Max;

	internal NumericUpDown nu_THD1Min;

	internal ComboBox cb_Channel_THD1;

	internal RadioButton rad_Direction0;

	internal RadioButton rad_Direction1;

	internal Button btn_ScanL2R;

	internal Button btn_ScanR2L;

	internal RadioButton rad_Direction2;

	internal RadioButton rad_Direction3;

	internal Button btn_ScanL2R_Copy;

	internal Button btn_ScanR2L_Copy;

	internal Button btn_ReDraw_Copy;

	internal ComboBox cb_THD1_DEStart;

	internal ComboBox cb_THD1_DEStartCount;

	internal Label chk_l;

	internal ComboBox cb_THD1_DEEnd;

	internal ComboBox cb_THD1_DEEndCount;

	internal CheckBox chk_LineFilter;

	internal Button btn_OTSU;

	internal Button btn_PrevTracker;

	internal Button btn_NextTracker;

	internal Button btn_ImgS_Copy;

	internal Button btn_ImgV_Copy;

	internal Button btn_ImageNum;

	internal Button btn_ImgSrc;

	internal Button btn_ImgR;

	internal Button btn_ImgG;

	internal Button btn_ImgB;

	internal Button btn_ImgH;

	internal Button btn_ImgS;

	internal Button btn_ImgV;

	internal Button btn_ImgGray;

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

	internal Label lab_AIRValue;

	internal Label lab_AIGValue;

	internal Label lab_AIBValue;

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

	internal Button btn_ReDraw;

	internal Button btn_ColorDropper;

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

	internal Button btn_ResetA;

	internal Label Label_Index;

	internal TextBox lab_LockMode;

	private bool _contentLoaded;

	public LineAnalysisWindow(User_Permissions permissions)
	{
		inited = false;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		User = permissions;
		Label_ResultError.Visibility = Visibility.Hidden;
		rt_Mask1.Visibility = Visibility.Hidden;
		rt_Mask2.Visibility = Visibility.Hidden;
		CanvasWidth = cnv_Image.Width;
		CanvasHeight = cnv_Image.Height;
		ToolsButton = new Button[20]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20
		};
		THD_Sliders = new RangeSlider[3] { rs_THD1, rs_THD2, rs_THD3 };
		THD_Min = new NumericUpDown[3] { nu_THD1Min, nu_THD2Min, nu_THD3Min };
		THD_Max = new NumericUpDown[3] { nu_THD1Max, nu_THD2Max, nu_THD3Max };
		THD_Channel = new ComboBox[3] { cb_Channel_THD1, cb_Channel_THD2, cb_Channel_THD3 };
		DEStart = new ComboBox[3] { cb_THD1_DEStart, cb_THD2_DEStart, cb_THD3_DEStart };
		DEStartCount = new ComboBox[3] { cb_THD1_DEStartCount, cb_THD2_DEStartCount, cb_THD3_DEStartCount };
		DEEnd = new ComboBox[3] { cb_THD1_DEEnd, cb_THD2_DEEnd, cb_THD3_DEEnd };
		DEEndCount = new ComboBox[3] { cb_THD1_DEEndCount, cb_THD2_DEEndCount, cb_THD3_DEEndCount };
		Label_Result.Visibility = Visibility.Hidden;
		BinaryBlob[0] = new Mat();
		BinaryBlob[1] = new Mat();
		rt_ReferenceRect1.Visibility = Visibility.Hidden;
		line_X1.Visibility = (line_Y1.Visibility = Visibility.Hidden);
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		btn_ResetA.Visibility = Visibility.Hidden;
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
		cb_Track1AutoSize.Items.Clear();
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
			for (int j = 0; j < 20; j++)
			{
				cb_TrackerSelect.Items.Add(j + 1 + " : " + inspection.paramFile.LineAnalysisParam[j].TrackerName);
				if (!inspection.paramFile.AdvLocateToolON)
				{
					if (j < 2)
					{
						cb_Track1AlignWayX.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
						cb_Track1AlignWayY.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
						if (j < 1)
						{
							cb_Track1AutoSize.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName);
						}
					}
					continue;
				}
				if (inspection.paramFile.VSLocateParam[j].TrackerON)
				{
					cb_Track1AlignWayX.Items.Add(j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem2 = new ComboBoxItem();
					comboBoxItem2.Content = j + 1 + " : " + inspection.paramFile.VSLocateParam[j].TrackerName;
					comboBoxItem2.IsEnabled = false;
					cb_Track1AlignWayX.Items.Add(comboBoxItem2);
				}
				if (inspection.paramFile.HSLocateParam[j].TrackerON)
				{
					cb_Track1AlignWayY.Items.Add(j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem3 = new ComboBoxItem();
					comboBoxItem3.Content = j + 1 + " : " + inspection.paramFile.HSLocateParam[j].TrackerName;
					comboBoxItem3.IsEnabled = false;
					cb_Track1AlignWayY.Items.Add(comboBoxItem3);
				}
				if (inspection.paramFile.AdvLocateParam[j].TrackerON)
				{
					cb_Track1AutoSize.Items.Add(j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem4 = new ComboBoxItem();
				comboBoxItem4.Content = j + 1 + " : " + inspection.paramFile.AdvLocateParam[j].TrackerName;
				comboBoxItem4.IsEnabled = false;
				cb_Track1AutoSize.Items.Add(comboBoxItem4);
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
			string text = "LAW01-LineAnalysisWindow-SetInspection()";
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
			if (inspection.paramFile.LineAnalysisParam[i].TrackerON)
			{
				if (inspection.iResult.LineAnalysisResult[i] == TOOL_RESULT.EM_OK)
				{
					if (inspection.paramFile.LineAnalysisParam[i].EnableSelectFace)
					{
						if (inspection.paramFile.LineAnalysisParam[i].FaceIndex == FaceIndex)
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
				else if (inspection.iResult.LineAnalysisResult[i] == TOOL_RESULT.EM_NG)
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
			ToolsButton[i].ToolTip = inspection.paramFile.LineAnalysisParam[i].TrackerName;
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerName) + "BT01-LineAnalysisWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
		Mat offlineMat = inspection.buffers.GetOfflineMat(button.Uid);
		THD_Preview = false;
		ImageSource imageSource = inspection.buffers.Mat2ImageSource(offlineMat);
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
	private bool SetTracker(OpenCvSharp.Rect rect1)
	{
		if (PreviewMode)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		rect1 &= rect2;
		bool result = true;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Tracker A value < 0，Please check Aligment and Autofit value, or reset Tracker A! ", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("檢測區域A寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域A!", "錯誤");
			}
			else
			{
				MessageBox.Show("检测区域A宽高小于0，请检查对齐、自动内缩设定或是重设检测区域A!", "错误");
			}
			txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
			result = false;
			bor_SearchRect1.Uid = "1";
			DontSave = true;
			btn_Test.IsEnabled = false;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			btn_ResetA.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
			bor_SearchRect1.Uid = "0";
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
			btn_ResetA.Visibility = Visibility.Hidden;
			DontSave = false;
			btn_Test.IsEnabled = true;
		}
		txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetTracker(ref OpenCvSharp.Rect rect1)
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
		rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
		rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
		rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
		rect1 &= rect2;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Tracker A value over range，Please check Aligment and Autofit value, or reset Tracker A! ", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("檢測區域A超出範圍，請檢查對齊、自動內縮設定或是重設檢測區域A!", "錯誤");
			}
			else
			{
				MessageBox.Show("检测区域A超出范围，请检查对齐、自动内缩设定或是重设检测区域A!", "错误");
			}
			txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
			return false;
		}
		return true;
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
			Label_Result.Content = msg;
			Label_Result.Background = Brushes.Red;
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
				double num2 = rect.Width;
				resultbox.Text = num2.ToString();
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
			case 2:
			{
				double num4 = rect.Height;
				resultbox.Text = num4.ToString();
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
			case 3:
			{
				double num5 = Convert.ToDouble(area);
				resultbox.Text = num5.ToString();
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
			case 4:
			{
				double num3 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				resultbox.Text = num3.ToString();
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
			case 5:
			{
				if (TrackerIndex == 0)
				{
					resultbox.Text = "";
					return true;
				}
				resultbox.Text = 0.0.ToString();
				if (!(Blobs[0].Area > 0.0) || !(Blobs[1].Area > 0.0))
				{
					return false;
				}
				double num = ((!(Blobs[0].Area > Blobs[1].Area)) ? (Blobs[1].Area / Blobs[0].Area) : (Blobs[0].Area / Blobs[1].Area));
				Blobs[1].AreaRatio = num;
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
				resultbox.Text = num.ToString();
				break;
			}
			}
			return result;
		}
		catch
		{
			string text = "LAW03-AnalysisWindow-GetRule()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DoThdImage()
	{
		if (PreviewMode || !UpdateUI || !inspection.buffers.LocatedReady)
		{
			return;
		}
		if (bor_SearchRect1.Uid == "1")
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Tracker A value < 0，Please check Aligment and Autofit value, or reset Tracker A! ", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("檢測區域A寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域A!", "錯誤");
			}
			else
			{
				MessageBox.Show("检测区域A宽高小于0，请检查对齐、自动内缩设定或是重设检测区域A!", "错误");
			}
			return;
		}
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
		if (!GetTracker(ref rect))
		{
			return;
		}
		Label_Result.Visibility = Visibility.Hidden;
		ShowBitmap = inspection.buffers.LocatedSrc.Clone();
		Mat[] array = new Mat[3];
		Mat[] array2 = new Mat[3];
		int[] array3 = new int[3];
		int[] array4 = new int[3];
		int[] array5 = new int[3];
		for (int i = 0; i < 3; i++)
		{
			array[i] = new Mat();
			array2[i] = new Mat();
			array3[i] = (int)THD_Sliders[i].LowerValue;
			array4[i] = (int)THD_Sliders[i].UpperValue;
			array5[i] = THD_Channel[i].SelectedIndex;
			if (i == 0 || exp_THD23.IsExpanded)
			{
				array[i] = inspection.buffers.GetChannelMat(array5[i]).Clone();
				Cv2.InRange(array[i], array3[i], array4[i], array2[i]);
				DoMorph(ref array2[i], DEStart[i].SelectedIndex, DEStartCount[i].SelectedIndex);
				DoMorph(ref array2[i], DEEnd[i].SelectedIndex, DEEndCount[i].SelectedIndex);
			}
		}
		Mat mat = array2[0].Clone();
		if (exp_THD23.IsExpanded)
		{
			Cv2.BitwiseAnd(mat, array2[1], mat);
			Cv2.BitwiseAnd(mat, array2[2], mat);
		}
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
					Mat m = new Mat(mat2, MaskRect[0]);
					inspection.paramFile.MaskParam[num].Mask1.Clone().CopyTo(m);
					flag = true;
				}
				if (MaskRect[1].Width > 0 && MaskRect[1].Height > 0 && inspection.paramFile.MaskParam[num].Mask2 != null)
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
			catch
			{
				string text = "BW02-BinaryWindow-DoThdImage()-Do Mask!";
				CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text, "錯誤");
				}
				return;
			}
		}
		try
		{
			Mat mat3 = mat.Clone();
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
		}
		catch
		{
			string text2 = "BW03-BinaryWindow-DoThdImage()-Draw to Screen";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
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
		Label_ResultError.Visibility = Visibility.Hidden;
		WriteUIValueToParam();
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		scr_TrackerSwich.Value = cb_TrackerSelect.SelectedIndex;
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
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Tracker value will set to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			Button obj = (Button)sender;
			if (obj.Uid == "0")
			{
				bor_SearchRect1.SetValue(Canvas.LeftProperty, 0.0);
				bor_SearchRect1.SetValue(Canvas.TopProperty, 0.0);
				bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, 100.0);
				bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, 100.0);
			}
			obj.Uid = "0";
			bor_SearchRect1.Visibility = Visibility.Visible;
			DontSave = false;
			btn_Test.IsEnabled = true;
			btn_ResetA.Visibility = Visibility.Hidden;
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
		cb_TrackerSelect.SelectedIndex = (int)scr_TrackerSwich.Value;
	}

	private void Scr_TrackerSwich_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if ((User < User_Permissions.USER_Administrator && inspection.paramFile.LineAnalysisParam[TrackIndex].Locked) || PreviewMode)
		{
			return;
		}
		if (ts_TrackerON.IsChecked.Value)
		{
			gb_BasicSetup1.Visibility = Visibility.Visible;
			exp_THD23.Visibility = Visibility.Visible;
			gb_BasicSetup3.Visibility = Visibility.Visible;
			gb_ConditionSetup.Visibility = Visibility.Visible;
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
			inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerON = true;
			CopyValueToUI();
		}
		else
		{
			WriteUIValueToParam();
			inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerON = false;
			inspection.paramFile.LineAnalysisParam[TrackIndex].ValueChangeFlag = true;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			gb_BasicSetup1.Visibility = Visibility.Hidden;
			exp_THD23.Visibility = Visibility.Hidden;
			gb_BasicSetup3.Visibility = Visibility.Hidden;
			gb_ConditionSetup.Visibility = Visibility.Hidden;
			ToolsButton[TrackIndex].Background = Brushes.White;
		}
	}

	private void Bor_SearchRect1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerName) + "BT01-LineAnalysisWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerName) + "BT01-LineAnalysisWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
		CopyParam = inspection.paramFile.LineAnalysisParam[TrackIndex];
		btn_RecipeCopy.Background = Brushes.PaleGreen;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if ((User >= User_Permissions.USER_Administrator || !inspection.paramFile.LineAnalysisParam[TrackIndex].Locked) && CopyParamSeted)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				string trackerName = inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerName;
				inspection.paramFile.LineAnalysisParam[TrackIndex] = CopyParam;
				inspection.paramFile.LineAnalysisParam[TrackIndex].TrackerName = trackerName;
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
			inspection.paramFile.LineAnalysisParam[TrackIndex] = inspection.paramFile_Backup.LineAnalysisParam[TrackIndex];
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
			if (LanguageType == 0)
			{
				MessageBox.Show("Selected Mask area has been disabled！", "Warning", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("所選擇的遮罩區域尚未啟用！", "警告", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("所选择的遮罩区域尚未启用！", "警告", MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.Yes);
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
		if (!IsNumeric(txt_AngleMin.Text))
		{
			txt_AngleMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_AngleMin.Background = Brushes.White;
		}
		if (!IsNumeric(txt_AngleMax.Text))
		{
			txt_AngleMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_AngleMax.Background = Brushes.White;
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
		if (!IsNumeric(txt_TotalError.Text))
		{
			txt_TotalError.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_TotalError.Background = Brushes.White;
		}
		if (!IsNumeric(txt_LineAngleDiff.Text))
		{
			txt_LineAngleDiff.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_LineAngleDiff.Background = Brushes.White;
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
		scr_TrackerSwich.Value = cb_TrackerSelect.SelectedIndex;
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
		scr_TrackerSwich.Value = cb_TrackerSelect.SelectedIndex;
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

	private void Nu_ThdUp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited && UpdateUI)
		{
			UpdateUI = false;
			try
			{
				Convert.ToDouble(nu_P2PInterval.Value);
			}
			catch
			{
				nu_P2PInterval.Value = nu_P2PInterval.Minimum;
			}
			try
			{
				Convert.ToDouble(nu_P2PAllowValue.Value);
			}
			catch
			{
				nu_P2PAllowValue.Value = nu_P2PAllowValue.Minimum;
			}
			UpdateUI = true;
			if (ts_TrackerON.IsChecked == true)
			{
				Btn_Test_Click(sender, e);
			}
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

	private void Btn_Tracker1SizeChange_Click(object sender, RoutedEventArgs e)
	{
		Edit2InputWindow edit2InputWindow = new Edit2InputWindow();
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect);
		_ = (Button)sender;
		edit2InputWindow.SetMinMaxValue(0, ImageWidth, 0, ImageHeight);
		edit2InputWindow.SetValue(rect.Width, rect.Height);
		edit2InputWindow.ShowDialog();
		if (edit2InputWindow.SetSuccess)
		{
			rect.Width = edit2InputWindow.Value1;
			rect.Height = edit2InputWindow.Value2;
			SetTracker(rect);
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
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Btn_ReferenceShow_Click(object sender, RoutedEventArgs e)
	{
		if (ReferenceShow)
		{
			Line line = line_X1;
			Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
			line.Visibility = visibility;
			rt_ReferenceRect1.Visibility = Visibility.Hidden;
			Rectangle rectangle = rt_Mask1;
			visibility = (rt_Mask2.Visibility = Visibility.Hidden);
			rectangle.Visibility = visibility;
			ReferenceShow = false;
			return;
		}
		ReferenceShow = true;
		if (rad_Tracker1AlignSelect.IsChecked == true)
		{
			Line line2 = line_X1;
			Visibility visibility = (line_Y1.Visibility = Visibility.Visible);
			line2.Visibility = visibility;
		}
		else
		{
			rt_ReferenceRect1.Visibility = Visibility.Visible;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode || !inited || !UpdateUI)
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
			if (!inspection.buffers.LocatedReady || DontSave)
			{
				return;
			}
			TextBox textBox = txt_AngleMin;
			TextBox textBox2 = txt_AngleMax;
			TextBox textBox3 = txt_ContError;
			Brush brush = (txt_TotalError.Background = Brushes.White);
			Brush brush2 = (textBox3.Background = brush);
			Brush background = (textBox2.Background = brush2);
			textBox.Background = background;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
			if (!GetTracker(ref rect))
			{
				return;
			}
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				SetResult(result: false);
				txt_AllResult.Clear();
				if (LanguageType == 0)
				{
					txt_AllResult.Text = "Tracker A Width and Height < 0!，Please Reset Tracker A!";
				}
				else if (LanguageType == 1)
				{
					txt_AllResult.Text = "檢測區域A寬度高度為0!，請重設檢測區域A";
				}
				else
				{
					txt_AllResult.Text = "检测区域A宽度高度为0!，请重设检测区域A";
				}
				DrawBackGround();
				return;
			}
			DoThdImage();
			WriteUIValueToParam();
			if (chk_SelectFace.IsChecked == true && FaceIndex != 0 && FaceIndex != SelectFace)
			{
				rt_Result.Fill = Brushes.White;
				Label_ResultError.Visibility = Visibility.Visible;
				txt_AllResult.Clear();
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
				lab_Time.Content = "0";
				return;
			}
			Label_ResultError.Visibility = Visibility.Hidden;
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			TimeStart();
			bool result = inspection.lineanalysisTool.Inspect(TrackIndex, ref iresult);
			double num = TimeStop();
			lab_Time.Content = string.Format("{0:N3}", num);
			SetResult(result);
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			try
			{
				OpenCvSharp.Point[] array = new OpenCvSharp.Point[4];
				if (chk_ShowScanOK.IsChecked == true)
				{
					for (int i = 0; i < inspection.lineanalysisTool.EdgePT.Count - 1; i++)
					{
						array[0].X = inspection.lineanalysisTool.EdgePT[i].X + rect.X;
						array[0].Y = inspection.lineanalysisTool.EdgePT[i].Y + rect.Y;
						array[1].X = inspection.lineanalysisTool.EdgePT[i + 1].X + rect.X;
						array[1].Y = inspection.lineanalysisTool.EdgePT[i + 1].Y + rect.Y;
						ShowBitmap.Line(array[0], array[1], Scalar.Lime);
					}
				}
				if (chk_EnablePointCheck.IsChecked == true)
				{
					if (chk_ShowScanNG.IsChecked == true)
					{
						for (int j = 0; j < inspection.lineanalysisTool.NGPt.Count; j++)
						{
							array[0].X = inspection.lineanalysisTool.NGPt[j].X + rect.X - 2;
							array[0].Y = inspection.lineanalysisTool.NGPt[j].Y + rect.Y - 2;
							array[1].X = inspection.lineanalysisTool.NGPt[j].X + rect.X + 2;
							array[1].Y = inspection.lineanalysisTool.NGPt[j].Y + rect.Y - 2;
							array[2].X = inspection.lineanalysisTool.NGPt[j].X + rect.X - 2;
							array[2].Y = inspection.lineanalysisTool.NGPt[j].Y + rect.Y + 2;
							array[3].X = inspection.lineanalysisTool.NGPt[j].X + rect.X + 2;
							array[3].Y = inspection.lineanalysisTool.NGPt[j].Y + rect.Y + 2;
							ShowBitmap.Line(array[0], array[3], Scalar.Red);
							ShowBitmap.Line(array[1], array[2], Scalar.Red);
						}
					}
					txt_ContErrorResult.Text = inspection.lineanalysisTool.ContinueError.ToString();
					txt_TotalErrorResult.Text = inspection.lineanalysisTool.TotalError.ToString();
				}
				else
				{
					txt_ContErrorResult.Text = "";
					txt_TotalErrorResult.Text = "";
				}
				if (chk_EnableAngleCheck.IsChecked == true)
				{
					txt_AngleResult.Text = string.Format("{0:N3}", inspection.lineanalysisTool.Angle);
					if (inspection.lineanalysisTool.Angle < Convert.ToDouble(txt_AngleMin.Text))
					{
						txt_AngleMin.Background = Brushes.Fuchsia;
					}
					if (inspection.lineanalysisTool.Angle > Convert.ToDouble(txt_AngleMax.Text))
					{
						txt_AngleMax.Background = Brushes.Fuchsia;
					}
					if (inspection.lineanalysisTool.ContinueError > Convert.ToInt32(txt_ContError.Text))
					{
						txt_ContError.Background = Brushes.Fuchsia;
					}
					if (inspection.lineanalysisTool.TotalError > Convert.ToInt32(txt_TotalError.Text))
					{
						txt_TotalError.Background = Brushes.Fuchsia;
					}
				}
				else
				{
					txt_AngleResult.Text = "";
				}
				txt_AllResult.Clear();
				if (chk_EnableAngleCheck.IsChecked == true)
				{
					if (cb_LineSegment.SelectedIndex == 0)
					{
						txt_AllResult.Text += inspection.lineanalysisTool.Tracker1ResultText;
						txt_AllResult.Text += "\n";
					}
					else
					{
						txt_AllResult.Text += inspection.lineanalysisTool.Tracker1ResultText;
						txt_AllResult.Text += "\n";
						for (int k = 1; k <= cb_LineSegment.SelectedIndex; k++)
						{
							if (inspection.lineanalysisTool.EdgePT.Count <= 0)
							{
								break;
							}
							if (rad_Direction2.IsChecked.Value || rad_Direction3.IsChecked.Value)
							{
								array[0].X = rect.X + inspection.lineanalysisTool.LineSegmentCount * k + inspection.lineanalysisTool.EdgePT[0].X;
								array[0].Y = rect.Y;
								array[1].X = rect.X + inspection.lineanalysisTool.LineSegmentCount * k + inspection.lineanalysisTool.EdgePT[0].X;
								array[1].Y = rect.Y + rect.Height - 1;
								ShowBitmap.Line(array[0], array[1], Scalar.Gold);
							}
							else
							{
								array[0].X = rect.X;
								array[0].Y = rect.Y + inspection.lineanalysisTool.LineSegmentCount * k + inspection.lineanalysisTool.EdgePT[0].Y;
								array[1].X = rect.X + rect.Width - 1;
								array[1].Y = rect.Y + inspection.lineanalysisTool.LineSegmentCount * k + inspection.lineanalysisTool.EdgePT[0].Y;
								ShowBitmap.Line(array[0], array[1], Scalar.Gold);
							}
						}
						txt_LineAngleDiff.Background = Brushes.White;
						for (int l = 1; l < inspection.lineanalysisTool.LineAngles.Count; l++)
						{
							if (Math.Abs(inspection.lineanalysisTool.LineAngles[l - 1] - inspection.lineanalysisTool.LineAngles[l]) > Convert.ToDouble(txt_LineAngleDiff.Text))
							{
								txt_LineAngleDiff.Background = Brushes.Fuchsia;
							}
						}
					}
				}
				if (chk_EnableDistanceCheck.IsChecked == true)
				{
					if (cb_DistanceLineSegment.SelectedIndex > 0)
					{
						for (int m = 1; m <= cb_DistanceLineSegment.SelectedIndex; m++)
						{
							if (inspection.lineanalysisTool.EdgePT.Count <= 0)
							{
								break;
							}
							if (rad_Direction2.IsChecked.Value || rad_Direction3.IsChecked.Value)
							{
								array[0].X = rect.X + inspection.lineanalysisTool.DistanceLineSegmentCount * m + inspection.lineanalysisTool.EdgePT[0].X;
								array[0].Y = rect.Y;
								array[1].X = rect.X + inspection.lineanalysisTool.DistanceLineSegmentCount * m + inspection.lineanalysisTool.EdgePT[0].X;
								array[1].Y = rect.Y + rect.Height - 1;
								ShowBitmap.Line(array[0], array[1], Scalar.Gold);
							}
							else
							{
								array[0].X = rect.X;
								array[0].Y = rect.Y + inspection.lineanalysisTool.DistanceLineSegmentCount * m + inspection.lineanalysisTool.EdgePT[0].Y;
								array[1].X = rect.X + rect.Width - 1;
								array[1].Y = rect.Y + inspection.lineanalysisTool.DistanceLineSegmentCount * m + inspection.lineanalysisTool.EdgePT[0].Y;
								ShowBitmap.Line(array[0], array[1], Scalar.Gold);
							}
						}
					}
					if (chk_ShowScanNG.IsChecked == true)
					{
						for (int n = 0; n < inspection.lineanalysisTool.DistanceNGPt.Count; n++)
						{
							array[0].X = inspection.lineanalysisTool.DistanceNGPt[n].X + rect.X - 2;
							array[0].Y = inspection.lineanalysisTool.DistanceNGPt[n].Y + rect.Y - 2;
							array[1].X = inspection.lineanalysisTool.DistanceNGPt[n].X + rect.X + 2;
							array[1].Y = inspection.lineanalysisTool.DistanceNGPt[n].Y + rect.Y - 2;
							array[2].X = inspection.lineanalysisTool.DistanceNGPt[n].X + rect.X - 2;
							array[2].Y = inspection.lineanalysisTool.DistanceNGPt[n].Y + rect.Y + 2;
							array[3].X = inspection.lineanalysisTool.DistanceNGPt[n].X + rect.X + 2;
							array[3].Y = inspection.lineanalysisTool.DistanceNGPt[n].Y + rect.Y + 2;
							ShowBitmap.Line(array[0], array[3], Scalar.Red);
							ShowBitmap.Line(array[1], array[2], Scalar.Red);
						}
					}
					txt_AllowDistanceErrorResult.Text = inspection.lineanalysisTool.DistanceTotalError.ToString();
					txt_DistanceContErrorResult.Text = inspection.lineanalysisTool.DistanceContError.ToString();
					txt_AllResult.Text += inspection.lineanalysisTool.Tracker1ResultText;
					txt_AllResult.Text += "\n";
					int num2 = cb_DistanceLineSegment.SelectedIndex + 1;
					if (num2 < 1)
					{
						num2 = 1;
					}
					int num3 = inspection.lineanalysisTool.PDistance.Count / num2;
					if (num3 < 1)
					{
						num3 = 1;
					}
					for (int num4 = 0; num4 < inspection.lineanalysisTool.PDistance.Count; num4++)
					{
						string arg = "";
						if (inspection.lineanalysisTool.PDistance[num4] > (double)Convert.ToInt32(txt_AllowDistance.Text))
						{
							arg = "[NG]";
						}
						string text = string.Format("[{0}] {1:N2}   {2}\n", num4 + 1, inspection.lineanalysisTool.PDistance[num4], arg);
						if (num4 % num3 == 0 && num4 < inspection.lineanalysisTool.PDistance.Count - 1)
						{
							text += string.Format("\n==分段{0}===\n", num4 / num3 + 1);
						}
						txt_AllResult.Text += text;
					}
					if (inspection.lineanalysisTool.DistanceTotalError > Convert.ToInt32(txt_AllowDistanceErrorCount.Text))
					{
						txt_AllowDistanceErrorCount.Background = Brushes.Fuchsia;
					}
					else
					{
						txt_AllowDistanceErrorCount.Background = Brushes.White;
					}
					if (inspection.lineanalysisTool.DistanceContError > Convert.ToInt32(txt_DistanceContError.Text))
					{
						txt_DistanceContError.Background = Brushes.Fuchsia;
					}
					else
					{
						txt_DistanceContError.Background = Brushes.White;
					}
				}
				if (inspection.lineanalysisTool.EdgePT.Count > (int)nu_P2PInterval.Value.Value && chk_EnablePointCheck.IsChecked == true)
				{
					if (LanguageType == 0)
					{
						txt_AllResult.Text += string.Format("[{0}:{1}] Line Point Continue Error={2}, Total Error={3}。\n\n", TrackIndex + 1, txt_TrackerName.Text, inspection.lineanalysisTool.ContinueError, inspection.lineanalysisTool.TotalError);
					}
					else if (LanguageType == 1)
					{
						txt_AllResult.Text += string.Format("[{0}:{1}] 檢測線段点跟点連續錯誤數={2}, 總錯誤數={3}。\n\n", TrackIndex + 1, txt_TrackerName.Text, inspection.lineanalysisTool.ContinueError, inspection.lineanalysisTool.TotalError);
					}
					else
					{
						txt_AllResult.Text += string.Format("[{0}:{1}] 检测线段点跟点连续错误数={2}, 总错误数={3}。\n\n", TrackIndex + 1, txt_TrackerName.Text, inspection.lineanalysisTool.ContinueError, inspection.lineanalysisTool.TotalError);
					}
					string text2 = "";
					string text3 = ((LanguageType == 0) ? "P2P Distance" : ((LanguageType != 1) ? "点跟点距离" : "點跟點距離"));
					string text4 = "";
					for (int num5 = 0; num5 < inspection.lineanalysisTool.EdgePT.Count - (int)nu_P2PInterval.Value.Value; num5++)
					{
						if (num5 + (int)nu_P2PInterval.Value.Value < 0 || num5 + (int)nu_P2PInterval.Value.Value >= inspection.lineanalysisTool.EdgePT.Count)
						{
							continue;
						}
						text2 = "";
						if (rad_Direction0.IsChecked == true || rad_Direction1.IsChecked == true)
						{
							if (Math.Abs(inspection.lineanalysisTool.EdgePT[num5].X - inspection.lineanalysisTool.EdgePT[num5 + (int)nu_P2PInterval.Value.Value].X) > (int)nu_P2PAllowValue.Value.Value)
							{
								text2 = " [NG]";
							}
							text4 += string.Format("[{0}] x = {1:N3}, {2} = {3:N3}, {4}\n", num5 + 1, inspection.lineanalysisTool.EdgePT[num5].X, text3, Math.Abs(inspection.lineanalysisTool.EdgePT[num5].X - inspection.lineanalysisTool.EdgePT[num5 + (int)nu_P2PInterval.Value.Value].X), text2);
						}
						else
						{
							if (Math.Abs(inspection.lineanalysisTool.EdgePT[num5].Y - inspection.lineanalysisTool.EdgePT[num5 + (int)nu_P2PInterval.Value.Value].Y) > (int)nu_P2PAllowValue.Value.Value)
							{
								text2 = " [NG]";
							}
							text4 += string.Format("[{0}] y = {1:N3}, {2} = {3:N3}, {4}\n", num5 + 1, inspection.lineanalysisTool.EdgePT[num5].Y, text3, Math.Abs(inspection.lineanalysisTool.EdgePT[num5].Y - inspection.lineanalysisTool.EdgePT[num5 + (int)nu_P2PInterval.Value.Value].Y), text2);
						}
					}
					int num6 = inspection.lineanalysisTool.EdgePT.Count - 1;
					while (num6 >= inspection.lineanalysisTool.EdgePT.Count - (int)nu_P2PInterval.Value.Value && num6 >= 0 && inspection.lineanalysisTool.EdgePT.Count > 0)
					{
						if (num6 - (int)nu_P2PInterval.Value.Value >= 0 && num6 - (int)nu_P2PInterval.Value.Value < inspection.lineanalysisTool.EdgePT.Count)
						{
							text2 = "";
							if (rad_Direction0.IsChecked == true || rad_Direction1.IsChecked == true)
							{
								if (Math.Abs(inspection.lineanalysisTool.EdgePT[num6].X - inspection.lineanalysisTool.EdgePT[num6 - (int)nu_P2PInterval.Value.Value].X) > (int)nu_P2PAllowValue.Value.Value)
								{
									text2 = " [NG]";
								}
								text4 += string.Format("[{0}] x = {1:N3}, {2} = {3:N3}, {4}\n", num6 + 1, inspection.lineanalysisTool.EdgePT[num6].X, text3, Math.Abs(inspection.lineanalysisTool.EdgePT[num6].X - inspection.lineanalysisTool.EdgePT[num6 - (int)nu_P2PInterval.Value.Value].X), text2);
							}
							else
							{
								if (Math.Abs(inspection.lineanalysisTool.EdgePT[num6].Y - inspection.lineanalysisTool.EdgePT[num6 - (int)nu_P2PInterval.Value.Value].Y) > (int)nu_P2PAllowValue.Value.Value)
								{
									text2 = " [NG]";
								}
								text4 += string.Format("[{0}] y = {1:N3}, {2} = {3:N3}, {4}\n", num6 + 1, inspection.lineanalysisTool.EdgePT[num6].Y, text3, Math.Abs(inspection.lineanalysisTool.EdgePT[num6].Y - inspection.lineanalysisTool.EdgePT[num6 - (int)nu_P2PInterval.Value.Value].Y), text2);
							}
						}
						num6--;
					}
					txt_AllResult.Text += text4;
				}
			}
			catch (Exception ex)
			{
				string text5 = "Lineanalysis01-LineAnalysisWindow-Btn_Test_Click()";
				text5 += ex.ToString();
				CVBuffer.SaveLog(text5, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text5, "錯誤");
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
	}

	private void Btn_TrackerShow_Click(object sender, RoutedEventArgs e)
	{
		if (!TrackerShow)
		{
			bor_SearchRect1.Visibility = Visibility.Visible;
			TrackerShow = true;
			bor_LocateRect.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect1.Visibility = Visibility.Hidden;
			TrackerShow = false;
			bor_LocateRect.Visibility = Visibility.Hidden;
		}
	}

	private void Rs_THD1_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited && UpdateUI)
		{
			UpdateUI = false;
			nu_THD1Min.Value = (int)rs_THD1.LowerValue;
			nu_THD1Max.Value = (int)rs_THD1.UpperValue;
			UpdateUI = true;
			DoThdImage();
		}
	}

	private void Rs_THD2_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited && UpdateUI)
		{
			UpdateUI = false;
			nu_THD2Min.Value = (int)rs_THD2.LowerValue;
			nu_THD2Max.Value = (int)rs_THD2.UpperValue;
			UpdateUI = true;
			DoThdImage();
		}
	}

	private void Rs_THD3_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (inited && UpdateUI)
		{
			UpdateUI = false;
			nu_THD3Min.Value = (int)rs_THD3.LowerValue;
			nu_THD3Max.Value = (int)rs_THD3.UpperValue;
			UpdateUI = true;
			DoThdImage();
		}
	}

	private void Nu_THD1Min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited && UpdateUI)
		{
			double num = 0.0;
			double num2 = 0.0;
			UpdateUI = false;
			try
			{
				num = Convert.ToDouble(nu_THD1Min.Value);
			}
			catch
			{
				nu_THD1Min.Value = nu_THD1Min.Minimum;
				num = nu_THD1Min.Value.Value;
			}
			try
			{
				num2 = Convert.ToDouble(nu_THD1Max.Value);
			}
			catch
			{
				nu_THD1Max.Value = nu_THD1Max.Maximum;
				num2 = nu_THD1Max.Value.Value;
			}
			rs_THD1.LowerValue = num;
			rs_THD1.UpperValue = num2;
			UpdateUI = true;
			DoThdImage();
		}
	}

	private void Nu_THD2Min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited && UpdateUI)
		{
			double num = 0.0;
			double num2 = 0.0;
			UpdateUI = false;
			try
			{
				num = Convert.ToDouble(nu_THD2Min.Value);
			}
			catch
			{
				nu_THD2Min.Value = nu_THD2Min.Minimum;
				num = nu_THD2Min.Value.Value;
			}
			try
			{
				num2 = Convert.ToDouble(nu_THD2Max.Value);
			}
			catch
			{
				nu_THD2Max.Value = nu_THD2Max.Maximum;
				num2 = nu_THD2Max.Value.Value;
			}
			rs_THD2.LowerValue = num;
			rs_THD2.UpperValue = num2;
			UpdateUI = true;
			DoThdImage();
		}
	}

	private void Nu_THD3Min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (inited && UpdateUI)
		{
			double num = 0.0;
			double num2 = 0.0;
			UpdateUI = false;
			try
			{
				num = Convert.ToDouble(nu_THD3Min.Value);
			}
			catch
			{
				nu_THD3Min.Value = nu_THD3Min.Minimum;
				num = nu_THD3Min.Value.Value;
			}
			try
			{
				num2 = Convert.ToDouble(nu_THD3Max.Value);
			}
			catch
			{
				nu_THD3Max.Value = nu_THD3Max.Maximum;
				num2 = nu_THD3Max.Value.Value;
			}
			rs_THD3.LowerValue = num;
			rs_THD3.UpperValue = num2;
			UpdateUI = true;
			DoThdImage();
		}
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
		if (!ColorDropper)
		{
			return;
		}
		cnv_Image.Cursor = Cursors.Arrow;
		for (int i = 0; i < 3; i++)
		{
			int num = CurrentColor[THD_Channel[i].SelectedIndex];
			int num2 = ((num - AutoThdValue >= 0) ? (num - AutoThdValue) : 0);
			int num3 = ((num + AutoThdValue > 255) ? 255 : (num + AutoThdValue));
			if (i == 0 || exp_THD23.IsExpanded)
			{
				THD_Sliders[i].LowerValue = num2;
				THD_Sliders[i].UpperValue = num3;
			}
		}
		DoThdImage();
		ColorDropper = false;
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

	private void Txt_AngleMin_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

	private void Txt_ContError_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

	private void DrawBackGround()
	{
		if (!PreviewMode)
		{
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		UpdateUI = false;
		DontSave = false;
		btn_Test.IsEnabled = true;
		rt_Result.Fill = Brushes.White;
		Label_ResultError.Visibility = Visibility.Hidden;
		btn_ResetA.Visibility = Visibility.Hidden;
		bor_SearchRect1.Uid = "0";
		Label_ResultError.Visibility = Visibility.Hidden;
		if (PreviewMode)
		{
			btn_Test.Visibility = Visibility.Hidden;
			btn_TestNextImg.Visibility = Visibility.Hidden;
			btn_TestPrevImg.Visibility = Visibility.Hidden;
		}
		BackupParam = inspection.paramFile.LineAnalysisParam[TrackIndex];
		INSP_LINEANALYSISTOOL_PARAM iNSP_LINEANALYSISTOOL_PARAM = inspection.paramFile.LineAnalysisParam[TrackIndex];
		scr_TrackerSwich.Value = TrackIndex;
		ts_TrackerON.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.TrackerON;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		Label_Index.Content = (TrackIndex + 1).ToString();
		txt_AllResult.Clear();
		if (iNSP_LINEANALYSISTOOL_PARAM.TrackerON)
		{
			gb_BasicSetup1.Visibility = Visibility.Visible;
			exp_THD23.Visibility = Visibility.Visible;
			gb_BasicSetup3.Visibility = Visibility.Visible;
			gb_ConditionSetup.Visibility = Visibility.Visible;
		}
		else
		{
			gb_BasicSetup1.Visibility = Visibility.Hidden;
			exp_THD23.Visibility = Visibility.Hidden;
			gb_BasicSetup3.Visibility = Visibility.Hidden;
			gb_ConditionSetup.Visibility = Visibility.Hidden;
		}
		if (iNSP_LINEANALYSISTOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
		{
			lab_LockMode.Visibility = Visibility.Visible;
		}
		else
		{
			lab_LockMode.Visibility = Visibility.Hidden;
		}
		cb_TrackerSelect.SelectedIndex = TrackIndex;
		txt_TrackerName.Text = iNSP_LINEANALYSISTOOL_PARAM.TrackerName;
		cb_MaskSelect.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex;
		chk_UseMaskAsInspArea.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.UseMaskAsInspArea;
		cb_Channel_THD1.SelectedIndex = (int)iNSP_LINEANALYSISTOOL_PARAM.THD1_ChannelIndex;
		cb_Channel_THD2.SelectedIndex = (int)iNSP_LINEANALYSISTOOL_PARAM.THD2_ChannelIndex;
		cb_Channel_THD3.SelectedIndex = (int)iNSP_LINEANALYSISTOOL_PARAM.THD3_ChannelIndex;
		exp_THD23.IsExpanded = iNSP_LINEANALYSISTOOL_PARAM.THD23_ChannelON;
		rs_THD1.LowerValue = iNSP_LINEANALYSISTOOL_PARAM.THD1_Min;
		rs_THD1.UpperValue = iNSP_LINEANALYSISTOOL_PARAM.THD1_Max;
		nu_THD1Min.Value = iNSP_LINEANALYSISTOOL_PARAM.THD1_Min;
		nu_THD1Max.Value = iNSP_LINEANALYSISTOOL_PARAM.THD1_Max;
		rs_THD2.LowerValue = iNSP_LINEANALYSISTOOL_PARAM.THD2_Min;
		rs_THD2.UpperValue = iNSP_LINEANALYSISTOOL_PARAM.THD2_Max;
		nu_THD2Min.Value = iNSP_LINEANALYSISTOOL_PARAM.THD2_Min;
		nu_THD2Max.Value = iNSP_LINEANALYSISTOOL_PARAM.THD2_Max;
		rs_THD3.LowerValue = iNSP_LINEANALYSISTOOL_PARAM.THD3_Min;
		rs_THD3.UpperValue = iNSP_LINEANALYSISTOOL_PARAM.THD3_Max;
		nu_THD3Min.Value = iNSP_LINEANALYSISTOOL_PARAM.THD3_Min;
		nu_THD3Max.Value = iNSP_LINEANALYSISTOOL_PARAM.THD3_Max;
		cb_THD1_DEStart.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD1_DEStart;
		cb_THD1_DEStartCount.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD1_DEStartCount;
		cb_THD2_DEStart.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD2_DEStart;
		cb_THD2_DEStartCount.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD2_DEStartCount;
		cb_THD3_DEStart.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD3_DEStart;
		cb_THD3_DEStartCount.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD3_DEStartCount;
		cb_THD1_DEEnd.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD1_DEEnd;
		cb_THD1_DEEndCount.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD1_DEEndCount;
		cb_THD2_DEEnd.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD2_DEEnd;
		cb_THD2_DEEndCount.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD2_DEEndCount;
		cb_THD3_DEEnd.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD3_DEEnd;
		cb_THD3_DEEndCount.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.THD3_DEEndCount;
		chk_UseStdFilter.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.UseStdFilter;
		chk_LineFilter.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.UseLineFilter;
		chk_EnableDistanceCheck.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.EnableDistanceCheck;
		cb_DistanceLineSegment.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.DistanceLineSegment;
		txt_AllowDistance.Text = iNSP_LINEANALYSISTOOL_PARAM.AllowDistance.ToString();
		txt_AllowDistanceErrorCount.Text = iNSP_LINEANALYSISTOOL_PARAM.AllowDistanceErrorCount.ToString();
		txt_DistanceContError.Text = iNSP_LINEANALYSISTOOL_PARAM.AllowDistanceContErrorCount.ToString();
		nu_P2PInterval.Value = iNSP_LINEANALYSISTOOL_PARAM.P2PInterval;
		nu_P2PAllowValue.Value = iNSP_LINEANALYSISTOOL_PARAM.P2PAllowValue;
		cb_LineSegment.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.LineSegment;
		txt_AngleMin.Text = iNSP_LINEANALYSISTOOL_PARAM.LineAngleMin.ToString();
		txt_AngleMax.Text = iNSP_LINEANALYSISTOOL_PARAM.LineAngleMax.ToString();
		txt_LineAngleDiff.Text = iNSP_LINEANALYSISTOOL_PARAM.LineAngleDiff.ToString();
		chk_EnableAngleCheck.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.EnableAngleCheck;
		chk_EnablePointCheck.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.EnablePointCheck;
		txt_ContError.Text = iNSP_LINEANALYSISTOOL_PARAM.ContinueError.ToString();
		txt_TotalError.Text = iNSP_LINEANALYSISTOOL_PARAM.TotalError.ToString();
		chk_SelectFace.IsChecked = iNSP_LINEANALYSISTOOL_PARAM.EnableSelectFace;
		SetSelectFace(iNSP_LINEANALYSISTOOL_PARAM.FaceIndex);
		switch (iNSP_LINEANALYSISTOOL_PARAM.Direction)
		{
		case ScanDirection.EM_LEFT_RIGHT:
			rad_Direction0.IsChecked = true;
			break;
		case ScanDirection.EM_RIGHT_LEFT:
			rad_Direction1.IsChecked = true;
			break;
		case ScanDirection.EM_TOP_BOTTOM:
			rad_Direction2.IsChecked = true;
			break;
		case ScanDirection.EM_BOTTOM_TOP:
			rad_Direction3.IsChecked = true;
			break;
		default:
			rad_Direction0.IsChecked = true;
			break;
		}
		try
		{
			if (iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex > 0)
			{
				if (!inspection.paramFile.MaskToolON || !inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].TrackerON)
				{
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
					rt_Mask1.Visibility = Visibility.Hidden;
					rt_Mask2.Visibility = Visibility.Hidden;
					cb_MaskSelect.SelectedIndex = 0;
				}
				else
				{
					rt_Mask1.Visibility = Visibility.Visible;
					if (inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].SearchRect2ON)
					{
						rt_Mask2.Visibility = Visibility.Visible;
					}
					if (inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask1 != null)
					{
						MaskRect[0] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask1Pt.X, inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask1Pt.Y, inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask1.Cols, inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask1.Rows);
					}
					else
					{
						MaskRect[0] = new OpenCvSharp.Rect(0, 0, 0, 0);
					}
					if (inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].SearchRect2ON && inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask2 != null)
					{
						MaskRect[1] = new OpenCvSharp.Rect(inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask2Pt.X, inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask2Pt.Y, inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask2.Cols, inspection.paramFile.MaskParam[iNSP_LINEANALYSISTOOL_PARAM.MaskSelectIndex - 1].Mask2.Rows);
					}
					else
					{
						MaskRect[1] = new OpenCvSharp.Rect(0, 0, 0, 0);
					}
					SetMaskTracker(MaskRect[0], MaskRect[1]);
				}
			}
			else
			{
				rt_Mask1.Visibility = Visibility.Hidden;
				rt_Mask2.Visibility = Visibility.Hidden;
			}
		}
		catch
		{
			string text = "LAW05-AnalysisWindow-CopyVlaueToUI()-01";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		cb_ErrorBin.SelectedIndex = (int)iNSP_LINEANALYSISTOOL_PARAM.ErrorBin;
		cb_ParetoChart.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.ParetoIndex;
		cb_Track1AlignWayX.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex;
		cb_Track1AlignWayY.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex;
		cb_Track1AutoSize.SelectedIndex = iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex;
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		new OpenCvSharp.Rect(0, 0, 50, 50);
		try
		{
			if (iNSP_LINEANALYSISTOOL_PARAM.Tracker1UseAutoSize)
			{
				if (inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].TrackerON)
				{
					rad_Tracker1AutoSize.IsChecked = true;
					rect.X = inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].L + iNSP_LINEANALYSISTOOL_PARAM.Track1_L;
					rect.Y = inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].T + iNSP_LINEANALYSISTOOL_PARAM.Track1_T;
					rect.Width = inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].Width - (iNSP_LINEANALYSISTOOL_PARAM.Track1_L + iNSP_LINEANALYSISTOOL_PARAM.Track1_R);
					rect.Height = inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].Height - (iNSP_LINEANALYSISTOOL_PARAM.Track1_T + iNSP_LINEANALYSISTOOL_PARAM.Track1_B);
					SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].InsideRect);
					Line line = line_X1;
					Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
					line.Visibility = visibility;
				}
				else
				{
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
					rad_Tracker1AutoSize.IsChecked = true;
					rect.X = inspection.paramFile.AdvLocateParam[0].L + iNSP_LINEANALYSISTOOL_PARAM.Track1_L;
					rect.Y = inspection.paramFile.AdvLocateParam[0].T + iNSP_LINEANALYSISTOOL_PARAM.Track1_T;
					rect.Width = inspection.paramFile.AdvLocateParam[0].Width - (iNSP_LINEANALYSISTOOL_PARAM.Track1_L + iNSP_LINEANALYSISTOOL_PARAM.Track1_R);
					rect.Height = inspection.paramFile.AdvLocateParam[0].Height - (iNSP_LINEANALYSISTOOL_PARAM.Track1_T + iNSP_LINEANALYSISTOOL_PARAM.Track1_B);
					cb_Track1AutoSize.SelectedIndex = (iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex = 0);
					SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[0].InsideRect);
					Line line2 = line_X1;
					Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
					line2.Visibility = visibility;
				}
			}
			else if (inspection.paramFile.VSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex].TrackerON)
			{
				rad_Tracker1AlignSelect.IsChecked = true;
				int width = iNSP_LINEANALYSISTOOL_PARAM.Track1_R - iNSP_LINEANALYSISTOOL_PARAM.Track1_L;
				int height = iNSP_LINEANALYSISTOOL_PARAM.Track1_B - iNSP_LINEANALYSISTOOL_PARAM.Track1_T;
				rect.X = inspection.paramFile.VSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex].Value + iNSP_LINEANALYSISTOOL_PARAM.Track1_L;
				rect.Y = inspection.paramFile.HSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex].Value + iNSP_LINEANALYSISTOOL_PARAM.Track1_T;
				rect.Width = width;
				rect.Height = height;
				SetReferenceLine(line_X1, line_Y1, iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex, iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex);
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
			else
			{
				MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
				SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
				txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
				rad_Tracker1AlignSelect.IsChecked = true;
				int width2 = iNSP_LINEANALYSISTOOL_PARAM.Track1_R - iNSP_LINEANALYSISTOOL_PARAM.Track1_L;
				int height2 = iNSP_LINEANALYSISTOOL_PARAM.Track1_B - iNSP_LINEANALYSISTOOL_PARAM.Track1_T;
				rect.X = inspection.paramFile.VSLocateParam[0].Value + iNSP_LINEANALYSISTOOL_PARAM.Track1_L;
				rect.Y = inspection.paramFile.HSLocateParam[0].Value + iNSP_LINEANALYSISTOOL_PARAM.Track1_T;
				rect.Width = width2;
				rect.Height = height2;
				cb_Track1AlignWayX.SelectedIndex = 0;
				cb_Track1AlignWayY.SelectedIndex = 0;
				SetReferenceLine(line_X1, line_Y1, 0, 0);
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
		}
		catch
		{
			string text2 = "LAW06-AnalysisWindow-CopyVlaueToUI()-02";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return;
		}
		if (SetTracker(rect))
		{
			GetTracker(ref rect);
			LastSearchRect1 = rect;
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
		}
		if (!iNSP_LINEANALYSISTOOL_PARAM.TrackerON)
		{
			bor_SearchRect1.Visibility = Visibility.Hidden;
		}
		if (iNSP_LINEANALYSISTOOL_PARAM.EnableSelectFace && FaceIndex != 0 && FaceIndex != SelectFace)
		{
			rt_Result.Fill = Brushes.White;
			txt_AllResult.Clear();
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
			lab_Time.Content = "0";
		}
		UpdateUI = true;
	}

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		DoThdImage();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OTSU_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = cb_Channel_THD1.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			new OpenCvSharp.Rect(0, 0, 50, 50);
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
			if (exp_THD23.IsExpanded)
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
			string text = "BW10-LineAnalysisWindow-Btn_OTSUL_Click()-";
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
	private void WriteUIValueToParam()
	{
		if ((User < User_Permissions.USER_Administrator && inspection.paramFile.LineAnalysisParam[TrackIndex].Locked) || PreviewMode)
		{
			return;
		}
		try
		{
			if (DontSave)
			{
				Label_ResultError.Visibility = Visibility.Visible;
				if (LanguageType == 0)
				{
					Label_ResultError.Content = "Tracker value error, Can't be Save!";
				}
				else if (LanguageType == 1)
				{
					Label_ResultError.Content = "錯誤，檢測區域錯誤，自動儲存取消。";
				}
				else
				{
					Label_ResultError.Content = "錯誤，檢測區域錯誤，自動儲存取消。";
				}
				return;
			}
			INSP_LINEANALYSISTOOL_PARAM iNSP_LINEANALYSISTOOL_PARAM = new INSP_LINEANALYSISTOOL_PARAM
			{
				ValueChangeFlag = true,
				TrackerON = ts_TrackerON.IsChecked.Value,
				TrackerName = txt_TrackerName.Text,
				MaskSelectIndex = cb_MaskSelect.SelectedIndex,
				UseMaskAsInspArea = chk_UseMaskAsInspArea.IsChecked.Value,
				THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex,
				THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex,
				THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex,
				THD23_ChannelON = exp_THD23.IsExpanded,
				THD1_Min = (int)rs_THD1.LowerValue,
				THD1_Max = (int)rs_THD1.UpperValue,
				THD2_Min = (int)rs_THD2.LowerValue,
				THD2_Max = (int)rs_THD2.UpperValue,
				THD3_Min = (int)rs_THD3.LowerValue,
				THD3_Max = (int)rs_THD3.UpperValue,
				THD1_DEStart = cb_THD1_DEStart.SelectedIndex,
				THD1_DEStartCount = cb_THD1_DEStartCount.SelectedIndex,
				THD2_DEStart = cb_THD2_DEStart.SelectedIndex,
				THD2_DEStartCount = cb_THD2_DEStartCount.SelectedIndex,
				THD3_DEStart = cb_THD3_DEStart.SelectedIndex,
				THD3_DEStartCount = cb_THD3_DEStartCount.SelectedIndex,
				THD1_DEEnd = cb_THD1_DEEnd.SelectedIndex,
				THD1_DEEndCount = cb_THD1_DEEndCount.SelectedIndex,
				THD2_DEEnd = cb_THD2_DEEnd.SelectedIndex,
				THD2_DEEndCount = cb_THD2_DEEndCount.SelectedIndex,
				THD3_DEEnd = cb_THD3_DEEnd.SelectedIndex,
				THD3_DEEndCount = cb_THD3_DEEndCount.SelectedIndex,
				UseStdFilter = chk_UseStdFilter.IsChecked.Value,
				UseLineFilter = chk_LineFilter.IsChecked.Value,
				EnableDistanceCheck = chk_EnableDistanceCheck.IsChecked.Value,
				DistanceLineSegment = cb_DistanceLineSegment.SelectedIndex,
				AllowDistance = Convert.ToDouble(txt_AllowDistance.Text),
				AllowDistanceErrorCount = Convert.ToInt32(txt_AllowDistanceErrorCount.Text),
				AllowDistanceContErrorCount = Convert.ToInt32(txt_DistanceContError.Text),
				P2PInterval = (int)nu_P2PInterval.Value.Value,
				P2PAllowValue = (int)nu_P2PAllowValue.Value.Value,
				LineSegment = cb_LineSegment.SelectedIndex,
				LineAngleMin = Convert.ToDouble(txt_AngleMin.Text),
				LineAngleMax = Convert.ToDouble(txt_AngleMax.Text),
				LineAngleDiff = Convert.ToDouble(txt_LineAngleDiff.Text),
				EnableAngleCheck = chk_EnableAngleCheck.IsChecked.Value,
				EnablePointCheck = chk_EnablePointCheck.IsChecked.Value,
				ContinueError = Convert.ToInt32(txt_ContError.Text),
				TotalError = Convert.ToInt32(txt_TotalError.Text)
			};
			if (rad_Direction0.IsChecked == true)
			{
				iNSP_LINEANALYSISTOOL_PARAM.Direction = ScanDirection.EM_LEFT_RIGHT;
			}
			else if (rad_Direction1.IsChecked == true)
			{
				iNSP_LINEANALYSISTOOL_PARAM.Direction = ScanDirection.EM_RIGHT_LEFT;
			}
			else if (rad_Direction2.IsChecked == true)
			{
				iNSP_LINEANALYSISTOOL_PARAM.Direction = ScanDirection.EM_TOP_BOTTOM;
			}
			else
			{
				iNSP_LINEANALYSISTOOL_PARAM.Direction = ScanDirection.EM_BOTTOM_TOP;
			}
			iNSP_LINEANALYSISTOOL_PARAM.EnableSelectFace = chk_SelectFace.IsChecked.Value;
			iNSP_LINEANALYSISTOOL_PARAM.FaceIndex = SelectFace;
			iNSP_LINEANALYSISTOOL_PARAM.ErrorBin = (FLOW_BIN)cb_ErrorBin.SelectedIndex;
			iNSP_LINEANALYSISTOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
			iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex = cb_Track1AlignWayX.SelectedIndex;
			iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex = cb_Track1AlignWayY.SelectedIndex;
			iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex = cb_Track1AutoSize.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect);
			if (rad_Tracker1AutoSize.IsChecked.Value)
			{
				iNSP_LINEANALYSISTOOL_PARAM.Tracker1UseAutoSize = true;
				rad_Tracker1AutoSize.IsChecked = true;
				if (inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].TrackerON)
				{
					iNSP_LINEANALYSISTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].L;
					iNSP_LINEANALYSISTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].T;
					iNSP_LINEANALYSISTOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].Width - iNSP_LINEANALYSISTOOL_PARAM.Track1_L - rect.Width;
					iNSP_LINEANALYSISTOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex].Height - iNSP_LINEANALYSISTOOL_PARAM.Track1_T - rect.Height;
				}
				else
				{
					iNSP_LINEANALYSISTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[0].L;
					iNSP_LINEANALYSISTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[0].T;
					iNSP_LINEANALYSISTOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[0].Width - iNSP_LINEANALYSISTOOL_PARAM.Track1_L - rect.Width;
					iNSP_LINEANALYSISTOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[0].Height - iNSP_LINEANALYSISTOOL_PARAM.Track1_T - rect.Height;
					int autoSize1_SelectIndex = (cb_Track1AutoSize.SelectedIndex = 0);
					iNSP_LINEANALYSISTOOL_PARAM.AutoSize1_SelectIndex = autoSize1_SelectIndex;
				}
			}
			else if (inspection.paramFile.VSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex].TrackerON)
			{
				iNSP_LINEANALYSISTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex].Value;
				iNSP_LINEANALYSISTOOL_PARAM.Track1_R = iNSP_LINEANALYSISTOOL_PARAM.Track1_L + rect.Width;
				iNSP_LINEANALYSISTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentYIndex].Value;
				iNSP_LINEANALYSISTOOL_PARAM.Track1_B = iNSP_LINEANALYSISTOOL_PARAM.Track1_T + rect.Height;
				iNSP_LINEANALYSISTOOL_PARAM.Tracker1UseAutoSize = false;
			}
			else
			{
				iNSP_LINEANALYSISTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[0].Value;
				iNSP_LINEANALYSISTOOL_PARAM.Track1_R = iNSP_LINEANALYSISTOOL_PARAM.Track1_L + rect.Width;
				iNSP_LINEANALYSISTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[0].Value;
				iNSP_LINEANALYSISTOOL_PARAM.Track1_B = iNSP_LINEANALYSISTOOL_PARAM.Track1_T + rect.Height;
				iNSP_LINEANALYSISTOOL_PARAM.Tracker1UseAutoSize = false;
				int autoSize1_SelectIndex = (cb_Track1AlignWayX.SelectedIndex = 0);
				iNSP_LINEANALYSISTOOL_PARAM.Tracker1AligmentXIndex = autoSize1_SelectIndex;
			}
			iNSP_LINEANALYSISTOOL_PARAM.ValueChangeFlag = false;
			if (!iNSP_LINEANALYSISTOOL_PARAM.Equals(inspection.paramFile_Backup.LineAnalysisParam[TrackIndex]))
			{
				iNSP_LINEANALYSISTOOL_PARAM.ValueChangeFlag = true;
			}
			else
			{
				iNSP_LINEANALYSISTOOL_PARAM.ValueChangeFlag = false;
			}
			inspection.paramFile.LineAnalysisParam[TrackIndex] = iNSP_LINEANALYSISTOOL_PARAM;
			UpdateUI = false;
			int selectedIndex = cb_TrackerSelect.SelectedIndex;
			cb_TrackerSelect.Items.Clear();
			for (int i = 0; i < 20; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.LineAnalysisParam[i].TrackerName);
			}
			cb_TrackerSelect.SelectedIndex = selectedIndex;
			UpdateUI = true;
		}
		catch
		{
			string text = "LAW07-AnalysisWindow-WriteUIValueToParam()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/lineanalysiswindow.xaml", UriKind.Relative);
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
			MainW = (LineAnalysisWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			MainW.Drop += MainW_Drop;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
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
			bor_LocateRect = (Border)target;
			break;
		case 7:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			break;
		case 8:
			txt_Search1Label = (TextBlock)target;
			break;
		case 9:
			Label_Result = (Label)target;
			break;
		case 10:
			rt_Mask1 = (Rectangle)target;
			break;
		case 11:
			rt_ReferenceRect1 = (Rectangle)target;
			break;
		case 12:
			rt_Mask2 = (Rectangle)target;
			break;
		case 13:
			Label_ResultError = (Label)target;
			break;
		case 14:
			lab_FaceIndex = (Label)target;
			break;
		case 15:
			txt_PreviewMode = (TextBox)target;
			break;
		case 16:
			tab_Function = (TabControl)target;
			break;
		case 17:
			cb_TrackerSelect = (ComboBox)target;
			cb_TrackerSelect.SelectionChanged += Cb_TrackerSelect_SelectionChanged;
			break;
		case 18:
			scr_TrackerSwich = (ScrollBar)target;
			scr_TrackerSwich.SizeChanged += Scr_TrackerSwich_SizeChanged;
			scr_TrackerSwich.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 19:
			ts_TrackerON = (ToggleSwitch)target;
			ts_TrackerON.Click += Ts_TrackerON_Click;
			break;
		case 20:
			btn_Reverse = (Button)target;
			btn_Reverse.Click += Btn_Reverse_Click;
			break;
		case 21:
			btn_RecipeCopy = (Button)target;
			btn_RecipeCopy.Click += Btn_RecipeCopy_Click;
			break;
		case 22:
			btn_RecipePaste = (Button)target;
			btn_RecipePaste.Click += Btn_RecipePaste_Click;
			break;
		case 23:
			lab_TrackIndex = (Label)target;
			break;
		case 24:
			txt_TrackerName = (TextBox)target;
			break;
		case 25:
			lab_Pareto = (Label)target;
			break;
		case 26:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 27:
			lab_Time = (Label)target;
			break;
		case 28:
			gb_ConditionSetup = (GroupBox)target;
			break;
		case 29:
			grd_ConditionSetup = (Grid)target;
			break;
		case 30:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 31:
			rt_Result = (Rectangle)target;
			break;
		case 32:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 33:
			txt_ContError = (TextBox)target;
			txt_ContError.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 34:
			txt_TotalError = (TextBox)target;
			txt_TotalError.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 35:
			txt_ContErrorResult = (TextBox)target;
			break;
		case 36:
			txt_TotalErrorResult = (TextBox)target;
			break;
		case 37:
			chk_ShowScanOK = (CheckBox)target;
			break;
		case 38:
			chk_ShowScanNG = (CheckBox)target;
			break;
		case 39:
			txt_AngleMin = (TextBox)target;
			txt_AngleMin.PreviewMouseDoubleClick += Txt_AngleMin_PreviewMouseDoubleClick;
			break;
		case 40:
			txt_AngleMax = (TextBox)target;
			txt_AngleMax.PreviewMouseDoubleClick += Txt_AngleMin_PreviewMouseDoubleClick;
			break;
		case 41:
			txt_AngleResult = (TextBox)target;
			break;
		case 42:
			grd_SelectFace = (Grid)target;
			break;
		case 43:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 44:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 45:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 46:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 47:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 48:
			cb_LineSegment = (ComboBox)target;
			cb_LineSegment.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 49:
			chk_EnableAngleCheck = (CheckBox)target;
			break;
		case 50:
			chk_EnablePointCheck = (CheckBox)target;
			break;
		case 51:
			nu_P2PInterval = (NumericUpDown)target;
			nu_P2PInterval.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 52:
			nu_P2PAllowValue = (NumericUpDown)target;
			nu_P2PAllowValue.ValueChanged += Nu_ThdUp_ValueChanged;
			break;
		case 53:
			txt_LineAngleDiff = (TextBox)target;
			txt_LineAngleDiff.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 54:
			chk_EnableDistanceCheck = (CheckBox)target;
			break;
		case 55:
			cb_DistanceLineSegment = (ComboBox)target;
			cb_DistanceLineSegment.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 56:
			txt_AllowDistance = (TextBox)target;
			txt_AllowDistance.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 57:
			txt_AllowDistanceErrorCount = (TextBox)target;
			txt_AllowDistanceErrorCount.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 58:
			txt_AllowDistanceErrorResult = (TextBox)target;
			break;
		case 59:
			txt_DistanceContError = (TextBox)target;
			txt_DistanceContError.PreviewMouseDoubleClick += Txt_ContError_PreviewMouseDoubleClick;
			break;
		case 60:
			txt_DistanceContErrorResult = (TextBox)target;
			break;
		case 61:
			chk_UseStdFilter = (CheckBox)target;
			break;
		case 62:
			exp_THD23 = (Expander)target;
			break;
		case 63:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 64:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_THD2_CentralThumbDragDelta;
			rs_THD2.UpperThumbDragDelta += Rs_THD2_CentralThumbDragDelta;
			rs_THD2.CentralThumbDragDelta += Rs_THD2_CentralThumbDragDelta;
			break;
		case 65:
			nu_THD2Max = (NumericUpDown)target;
			nu_THD2Max.ValueChanged += Nu_THD2Min_ValueChanged;
			break;
		case 66:
			nu_THD2Min = (NumericUpDown)target;
			nu_THD2Min.ValueChanged += Nu_THD2Min_ValueChanged;
			break;
		case 67:
			cb_Channel_THD2 = (ComboBox)target;
			cb_Channel_THD2.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 68:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_THD3_CentralThumbDragDelta;
			rs_THD3.UpperThumbDragDelta += Rs_THD3_CentralThumbDragDelta;
			rs_THD3.CentralThumbDragDelta += Rs_THD3_CentralThumbDragDelta;
			break;
		case 69:
			nu_THD3Max = (NumericUpDown)target;
			nu_THD3Max.ValueChanged += Nu_THD3Min_ValueChanged;
			break;
		case 70:
			nu_THD3Min = (NumericUpDown)target;
			nu_THD3Min.ValueChanged += Nu_THD3Min_ValueChanged;
			break;
		case 71:
			cb_Channel_THD3 = (ComboBox)target;
			cb_Channel_THD3.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 72:
			cb_THD2_DEStart = (ComboBox)target;
			cb_THD2_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 73:
			cb_THD2_DEStartCount = (ComboBox)target;
			cb_THD2_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 74:
			cb_THD3_DEStart = (ComboBox)target;
			cb_THD3_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 75:
			cb_THD3_DEStartCount = (ComboBox)target;
			cb_THD3_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 76:
			cb_THD2_DEEnd = (ComboBox)target;
			cb_THD2_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 77:
			cb_THD2_DEEndCount = (ComboBox)target;
			cb_THD2_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 78:
			cb_THD3_DEEnd = (ComboBox)target;
			cb_THD3_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 79:
			cb_THD3_DEEndCount = (ComboBox)target;
			cb_THD3_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 80:
			gb_BasicSetup3 = (GroupBox)target;
			break;
		case 81:
			grd_SIMP_DilateErode1_Copy = (Grid)target;
			break;
		case 82:
			cb_Track1AlignWayX = (ComboBox)target;
			cb_Track1AlignWayX.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 83:
			rad_Tracker1AutoSize = (RadioButton)target;
			rad_Tracker1AutoSize.Click += Rad_Tracker1AutoSize_Click;
			break;
		case 84:
			cb_Track1AutoSize = (ComboBox)target;
			cb_Track1AutoSize.SelectionChanged += Cb_Track1AutoSize_SelectionChanged;
			break;
		case 85:
			rad_Tracker1AlignSelect = (RadioButton)target;
			rad_Tracker1AlignSelect.Click += Rad_Tracker1AlignSelect_Click;
			break;
		case 86:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 87:
			cb_Track1AlignWayY = (ComboBox)target;
			cb_Track1AlignWayY.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 88:
			btn_Tracker1SizeChange = (Button)target;
			btn_Tracker1SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 89:
			txt_Tracker1Pos = (Label)target;
			break;
		case 90:
			cb_MaskSelect = (ComboBox)target;
			cb_MaskSelect.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 91:
			btn_MaskEdit = (Button)target;
			btn_MaskEdit.Click += Btn_MaskEdit_Click;
			break;
		case 92:
			chk_UseMaskAsInspArea = (CheckBox)target;
			chk_UseMaskAsInspArea.Click += Chk_UseMaskAsInspArea_Click;
			break;
		case 93:
			gb_BasicSetup1 = (GroupBox)target;
			break;
		case 94:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 95:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_THD1_CentralThumbDragDelta;
			rs_THD1.UpperThumbDragDelta += Rs_THD1_CentralThumbDragDelta;
			rs_THD1.CentralThumbDragDelta += Rs_THD1_CentralThumbDragDelta;
			break;
		case 96:
			nu_THD1Max = (NumericUpDown)target;
			nu_THD1Max.ValueChanged += Nu_THD1Min_ValueChanged;
			break;
		case 97:
			nu_THD1Min = (NumericUpDown)target;
			nu_THD1Min.ValueChanged += Nu_THD1Min_ValueChanged;
			break;
		case 98:
			cb_Channel_THD1 = (ComboBox)target;
			cb_Channel_THD1.SelectionChanged += Cb_ChannelIndex_SelectionChanged;
			break;
		case 99:
			rad_Direction0 = (RadioButton)target;
			break;
		case 100:
			rad_Direction1 = (RadioButton)target;
			break;
		case 101:
			btn_ScanL2R = (Button)target;
			break;
		case 102:
			btn_ScanR2L = (Button)target;
			break;
		case 103:
			rad_Direction2 = (RadioButton)target;
			break;
		case 104:
			rad_Direction3 = (RadioButton)target;
			break;
		case 105:
			btn_ScanL2R_Copy = (Button)target;
			break;
		case 106:
			btn_ScanR2L_Copy = (Button)target;
			break;
		case 107:
			btn_ReDraw_Copy = (Button)target;
			btn_ReDraw_Copy.Click += Btn_ReDraw_Click;
			break;
		case 108:
			cb_THD1_DEStart = (ComboBox)target;
			cb_THD1_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 109:
			cb_THD1_DEStartCount = (ComboBox)target;
			cb_THD1_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 110:
			chk_l = (Label)target;
			break;
		case 111:
			cb_THD1_DEEnd = (ComboBox)target;
			cb_THD1_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 112:
			cb_THD1_DEEndCount = (ComboBox)target;
			cb_THD1_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 113:
			chk_LineFilter = (CheckBox)target;
			break;
		case 114:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSU_Click;
			break;
		case 115:
			btn_PrevTracker = (Button)target;
			btn_PrevTracker.Click += Btn_PrevTracker;
			break;
		case 116:
			btn_NextTracker = (Button)target;
			btn_NextTracker.Click += Btn_NextTracker_Click;
			break;
		case 117:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 118:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 119:
			btn_ImageNum = (Button)target;
			break;
		case 120:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 121:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 122:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 123:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 124:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 125:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 126:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 127:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 128:
			btn_ImgAIR = (Button)target;
			btn_ImgAIR.Click += Btn_ImgSrc_Click;
			break;
		case 129:
			btn_ImgAIG = (Button)target;
			btn_ImgAIG.Click += Btn_ImgSrc_Click;
			break;
		case 130:
			btn_ImgAIB = (Button)target;
			btn_ImgAIB.Click += Btn_ImgSrc_Click;
			break;
		case 131:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 132:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 133:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 134:
			btn_ImgAI = (Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 135:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 136:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 137:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 138:
			lab_RValue = (Label)target;
			break;
		case 139:
			lab_GValue = (Label)target;
			break;
		case 140:
			lab_BValue = (Label)target;
			break;
		case 141:
			lab_HValue = (Label)target;
			break;
		case 142:
			lab_SValue = (Label)target;
			break;
		case 143:
			lab_VValue = (Label)target;
			break;
		case 144:
			lab_GrayValue = (Label)target;
			break;
		case 145:
			lab_XValue = (Label)target;
			break;
		case 146:
			lab_YValue = (Label)target;
			break;
		case 147:
			lab_C1Value = (Label)target;
			break;
		case 148:
			lab_C2Value = (Label)target;
			break;
		case 149:
			lab_C3Value = (Label)target;
			break;
		case 150:
			lab_AIRValue = (Label)target;
			break;
		case 151:
			lab_AIGValue = (Label)target;
			break;
		case 152:
			lab_AIBValue = (Label)target;
			break;
		case 153:
			txt_AllResult = (TextBox)target;
			break;
		case 154:
			btn_TrackerShow = (Button)target;
			btn_TrackerShow.Click += Btn_TrackerShow_Click;
			break;
		case 155:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 156:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 157:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 158:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 159:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 160:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 161:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 162:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 163:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 164:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 165:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 166:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 167:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 168:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 169:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 170:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 171:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 172:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 173:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 174:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 175:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 176:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 177:
			btn_ReferenceShow = (Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 178:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 179:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 180:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 181:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 182:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 183:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 184:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 185:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 186:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 187:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 188:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 189:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 190:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 191:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 192:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 193:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 194:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 195:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 196:
			grd_TrackerAdj = (Grid)target;
			break;
		case 197:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 198:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 199:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 200:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 201:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 202:
			btn_ResetA = (Button)target;
			btn_ResetA.Click += Btn_TrackerReset_Click;
			break;
		case 203:
			Label_Index = (Label)target;
			break;
		case 204:
			lab_LockMode = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
