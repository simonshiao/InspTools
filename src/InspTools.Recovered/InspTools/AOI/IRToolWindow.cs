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

public class IRToolWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 35;

	private const int MaxThdTool = 4;

	private const int MaxRings = 4;

	private int LanguageType = 1;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private bool RotateStatus;

	private byte[] CurrentColor = new byte[16];

	private BlobStruct Blobs;

	private OpenCvSharp.Rect LocateRect;

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

	private int RingIndex;

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

	private ComboBox[] InspRule;

	private ComboBox[] MB_InspRule;

	private Button[] BTNRings;

	private Grid[] GRDRings;

	private Rectangle[] RTRings;

	private Label[] LABRings;

	private Label[] LABResult;

	private ComboBox[] CBRings;

	private Ellipse[] ELRingsResult;

	private Border[] BDTrackers;

	private bool EnableModifyLog;

	private string ModifyLog = "";

	private string RecipeName = "";

	private string UserID = "123456";

	private bool PreviewMode;

	private User_Permissions User = User_Permissions.USER_Administrator;

	private bool OTSU_White = true;

	private Stopwatch sw = new Stopwatch();

	internal IRToolWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect2;

	internal TextBlock txt_Search2Label;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal TextBox Label_ResultError;

	internal Label Label_Result;

	internal TextBox txt_PreviewMode;

	internal Border bor_SearchRect3;

	internal TextBlock txt_Search1Label1;

	internal Border bor_SearchRect4;

	internal TextBlock txt_Search1Label2;

	internal TabControl tab_Function;

	internal TabItem tab_Main;

	internal Grid grd_Main;

	internal Rectangle rt_Ring1;

	internal Rectangle rt_Ring2;

	internal Rectangle rt_Ring3;

	internal Rectangle rt_Ring4;

	internal Label lab_Ring1;

	internal Label lab_Ring2;

	internal Label lab_Ring3;

	internal Label lab_Ring4;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal ComboBox cb_RingCount;

	internal Label lab_Pareto_Copy;

	internal Label ico_ResultAlart1;

	internal Label ico_ResultAlart2;

	internal Label ico_ResultAlart3;

	internal Label ico_ResultAlart4;

	internal ComboBox cb_BinaryErrorBin;

	internal Button btn_Test;

	internal Rectangle rt_Result;

	internal Button btn_RotateImage2;

	internal Grid grd_Tool;

	internal Grid grd_Ring1;

	internal Ellipse es_RingResult1;

	internal ComboBox cb_Ring1;

	internal Grid grd_Ring2;

	internal Ellipse es_RingResult2;

	internal ComboBox cb_Ring2;

	internal Grid grd_Ring3;

	internal Ellipse es_RingResult3;

	internal ComboBox cb_Ring3;

	internal Grid grd_Ring4;

	internal Ellipse es_RingResult4;

	internal ComboBox cb_Ring4;

	internal TabItem tab_Setup;

	internal ScrollBar scr_TrackerSwich;

	internal Label lab_Time;

	internal Button btn_ResetTracker;

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

	internal ComboBox cb_Channel_THD1;

	internal NumericUpDown nu_Thd1LimitMin;

	internal NumericUpDown nu_Thd1LimitMax;

	internal Button btn_ChannelBinaryPreview1;

	internal Button btn_ChannelPreview1;

	internal Button btn_HistogramPreview1;

	internal Button btn_THD1Undo;

	internal Button btn_OTSU;

	internal Label lab_SoftCorrectionShiftTHD1;

	internal Grid grd_SIMP_RefSetup;

	internal ComboBox cb_ColorReference;

	internal Button btn_ReferenceSave;

	internal TextBox txt_ReferenceName;

	internal Button btn_ReferenceLock;

	internal Grid grd_SIMP_Setup;

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

	internal RadioButton rad_RuleAND;

	internal RadioButton rad_RuleOR;

	internal Button btn_BinaryTest;

	internal Rectangle rt_RingTestResult;

	internal Button btn_RotateImage;

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

	internal Button btn_PreviewSwitch;

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

	internal Button btn_Ring1;

	internal Button btn_Ring2;

	internal Button btn_Ring3;

	internal Button btn_Ring4;

	internal Button btn_Result_FlipResult;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public IRToolWindow(User_Permissions permissions)
	{
		inited = false;
		User = permissions;
		InitializeComponent();
		exp_THD4.Visibility = Visibility.Visible;
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
		Result1Txt = new TextBox[4] { txt_InspRule11Result, txt_InspRule12Result, txt_InspRule13Result, txt_InspRule14Result };
		ValueMinTxt = new TextBox[4] { txt_InspRule1Min, txt_InspRule2Min, txt_InspRule3Min, txt_InspRule4Min };
		ValueMaxTxt = new TextBox[4] { txt_InspRule1Max, txt_InspRule2Max, txt_InspRule3Max, txt_InspRule4Max };
		THD_Expender = new Expander[4] { exp_THD1, exp_THD2, exp_THD3, exp_THD4 };
		InspRule = new ComboBox[4] { cb_InspRule1, cb_InspRule2, cb_InspRule3, cb_InspRule4 };
		BTNRings = new Button[4] { btn_Ring1, btn_Ring2, btn_Ring3, btn_Ring4 };
		GRDRings = new Grid[4] { grd_Ring1, grd_Ring2, grd_Ring3, grd_Ring4 };
		LABRings = new Label[4] { lab_Ring1, lab_Ring2, lab_Ring3, lab_Ring4 };
		LABResult = new Label[4] { ico_ResultAlart1, ico_ResultAlart2, ico_ResultAlart3, ico_ResultAlart4 };
		RTRings = new Rectangle[4] { rt_Ring1, rt_Ring2, rt_Ring3, rt_Ring4 };
		CBRings = new ComboBox[4] { cb_Ring1, cb_Ring2, cb_Ring3, cb_Ring4 };
		ELRingsResult = new Ellipse[4] { es_RingResult1, es_RingResult2, es_RingResult3, es_RingResult4 };
		BDTrackers = new Border[4] { bor_SearchRect1, bor_SearchRect2, bor_SearchRect3, bor_SearchRect4 };
		Label_Result.Visibility = Visibility.Hidden;
		BinaryBlob[0] = new Mat();
		BinaryBlob[1] = new Mat();
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		exp_TrackerFilllet.Visibility = Visibility.Hidden;
		grd_Histogram.Visibility = Visibility.Hidden;
		txt_PreviewMode.Visibility = Visibility.Hidden;
		for (int i = 0; i < 4; i++)
		{
			LABResult[i].Visibility = Visibility.Hidden;
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
	public void SetInspection(int index, CVInspection station, int faceIndex = 0)
	{
		try
		{
			TrackIndex = index;
			inspection = station;
			FaceIndex = faceIndex;
			UpdateUI = false;
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
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "BW05-IRToolWindow-SetInspection()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
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
				CopyValueToUI(0);
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
			string text = "BW01-IRToolWindow-SetBuffer()";
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-IRToolWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
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
		if (!ReferenceLock)
		{
			Btn_ReferenceSave_Click(sender, null);
			ReferenceLock = true;
		}
		WriteUIValueToParam(-1);
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

	private bool SetTracker(OpenCvSharp.Rect rect, int id, bool show = true)
	{
		if (PreviewMode)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect &= rect2;
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			rect = new OpenCvSharp.Rect(0, 0, rect2.Width / 2, rect2.Height / 2);
		}
		BDTrackers[id].SetValue(Canvas.LeftProperty, (double)rect.X * xRatio);
		BDTrackers[id].SetValue(Canvas.TopProperty, (double)rect.Y * yRatio);
		BDTrackers[id].SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
		BDTrackers[id].SetValue(FrameworkElement.HeightProperty, (double)rect.Height * yRatio);
		return true;
	}

	private bool GetTracker(ref OpenCvSharp.Rect rect, int id)
	{
		if (PreviewMode)
		{
			return false;
		}
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect.X = (int)(Canvas.GetLeft(BDTrackers[id]) / xRatio);
		rect.Y = (int)(Canvas.GetTop(BDTrackers[id]) / yRatio);
		rect.Width = (int)(BDTrackers[id].Width / xRatio);
		rect.Height = (int)(BDTrackers[id].Height / yRatio);
		rect &= rect2;
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			rect = new OpenCvSharp.Rect(0, 0, rect2.Width / 2, rect2.Height / 2);
			SetTracker(rect, id);
		}
		return true;
	}

	private void Rs_SIMP_THD1_ThumbDragCompleted(object sender, DragDeltaEventArgs e)
	{
		if (inited)
		{
			DoThdImage();
			btn_BinaryTest_Click(this, null);
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
			rt_Result.Fill = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
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
			rt_Result.Fill = Brushes.Red;
		}
	}

	private void SetErrorResult(string str)
	{
		Label_Result.Visibility = Visibility.Visible;
		Label_Result.Content = str;
		Label_Result.Background = Brushes.Red;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckResult()
	{
		try
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			rect = inspection.irTool.Blobs.NGRect;
			double num = 0.0;
			num = inspection.irTool.Blobs.Area;
			bool flag = true;
			bool[] array = new bool[4];
			for (int i = 0; i < 2; i++)
			{
				array[0] = GetRule(cb_InspRule1.SelectedIndex, txt_InspRule1Min, txt_InspRule1Max, txt_InspRule11Result, rect, num);
				array[1] = GetRule(cb_InspRule2.SelectedIndex, txt_InspRule2Min, txt_InspRule2Max, txt_InspRule12Result, rect, num);
				array[2] = GetRule(cb_InspRule3.SelectedIndex, txt_InspRule3Min, txt_InspRule3Max, txt_InspRule13Result, rect, num);
				array[3] = GetRule(cb_InspRule4.SelectedIndex, txt_InspRule4Min, txt_InspRule4Max, txt_InspRule14Result, rect, num);
				if (rad_RuleAND.IsChecked == true)
				{
					flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
					continue;
				}
				flag = false;
				for (int j = 0; j < 4; j++)
				{
					if (array[j] && InspRule[j].SelectedIndex > 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
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
			string text = "BW04-IRToolWindow-CheckResult()-";
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
	private bool GetRule(int rule, TextBox min, TextBox max, TextBox resultbox, OpenCvSharp.Rect rect, double area)
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
				double num5 = (double)rect.Width * inspection.paramFile.PixelPerUM_X;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num5);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num5);
				}
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
				break;
			}
			case 2:
			{
				double num4 = (double)rect.Height * inspection.paramFile.PixelPerUM_Y;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num4);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num4);
				}
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
				break;
			}
			case 3:
			{
				double num8 = Convert.ToDouble(area);
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num8);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num8);
				}
				if (num8 < num || num8 > num2)
				{
					result = false;
					if (num8 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num8 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 4:
			{
				double num7 = (double)inspection.irTool.Blobs.TotalWidth * inspection.paramFile.PixelPerUM_X;
				if (num7 < num || num7 > num2)
				{
					result = false;
					if (num7 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num7 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 5:
			{
				double num6 = (double)inspection.irTool.Blobs.TotalHeight * inspection.paramFile.PixelPerUM_Y;
				if (num6 < num || num6 > num2)
				{
					result = false;
					if (num6 < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (num6 > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				break;
			}
			case 6:
				resultbox.Text = "";
				return true;
			case 7:
				resultbox.Text = "";
				return true;
			case 8:
				resultbox.Text = "";
				return true;
			case 9:
				resultbox.Text = "";
				return true;
			case 10:
				resultbox.Text = "";
				return true;
			case 11:
			{
				double allArea = inspection.irTool.AllArea;
				if (allArea < num || allArea > num2)
				{
					result = false;
					if (allArea < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (allArea > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", allArea);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)allArea);
				}
				break;
			}
			case 12:
			{
				double allAreaRatio = inspection.irTool.AllAreaRatio;
				if (allAreaRatio < num || allAreaRatio > num2)
				{
					result = false;
					if (allAreaRatio < num)
					{
						min.Background = Brushes.Fuchsia;
					}
					if (allAreaRatio > num2)
					{
						max.Background = Brushes.Fuchsia;
					}
					resultbox.Foreground = Brushes.Red;
				}
				resultbox.Text = string.Format("{0:N3}", allAreaRatio);
				break;
			}
			case 13:
				resultbox.Text = "";
				return true;
			case 14:
			{
				double val = (double)rect.Width * inspection.paramFile.PixelPerUM_X;
				double val2 = (double)rect.Height * inspection.paramFile.PixelPerUM_Y;
				double num3 = Math.Max(val, val2);
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num3);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num3);
				}
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
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BW03-IRToolWindow-GetRule()-";
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
	public void DoThdImage(bool DrawAll = false)
	{
		if (PreviewMode || !inspection.buffers.LocatedReady || !UpdateUI)
		{
			return;
		}
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			if (RotateStatus)
			{
				Mat mat = new Mat(ShowBitmap, LocateRect);
				Cv2.Flip(mat, mat, FlipMode.XY);
			}
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		Label_ResultError.Visibility = Visibility.Hidden;
		Label_Result.Visibility = Visibility.Hidden;
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
		new OpenCvSharp.Rect(0, 0, 100, 100);
		Mat mat2 = new Mat();
		try
		{
			if (!GetTracker(ref rect, RingIndex))
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
				if (array6[i])
				{
					array[i] = inspection.buffers.GetChannelMat(array5[i]).Clone();
					if (RotateStatus)
					{
						Mat mat3 = new Mat(array[i], LocateRect);
						Cv2.Flip(mat3, mat3, FlipMode.XY);
					}
					Cv2.InRange(array[i], array3[i], array4[i], array2[i]);
					Mat mat4 = new Mat(array2[i], rect);
					if (array7[i] != 0)
					{
						DoMorph(ref mat4, array7[i], array9[i]);
					}
					if (array8[i] != 0)
					{
						DoMorph(ref mat4, array8[i], array10[i]);
					}
					num++;
				}
			}
			if (num == 0)
			{
				return;
			}
			mat2 = inspection.buffers.LocateGray.Clone();
			mat2.SetTo(255.0);
			for (int j = 0; j < 4; j++)
			{
				if (array6[j])
				{
					if (j != 3 || chk_Thd_ORMode.IsChecked == false)
					{
						Cv2.BitwiseAnd(mat2, array2[j], mat2);
					}
					else
					{
						Cv2.BitwiseOr(mat2, array2[j], mat2);
					}
				}
			}
			BinaryBlob[0] = new Mat(mat2, rect);
		}
		catch (Exception ex)
		{
			string text = "BW01-IRToolWindow-DoThdImage()-Threshold-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		try
		{
			Mat mat5 = mat2.Clone();
			mat5.SetTo(0.0);
			new Mat(mat5, rect).SetTo(255.0);
			Cv2.BitwiseAnd(mat2, mat5, mat2);
			Mat mat6 = new Mat();
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
			Cv2.Merge(ThdShow, mat6);
			if (DrawAll)
			{
				_ = new Point2f[4];
				Cv2.FindContours(mat2, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
				Cv2.DrawContours(ShowBitmap, contours, -1, new Scalar(255.0, 0.0, 255.0), 2);
			}
			else
			{
				mat6.CopyTo(ShowBitmap, mat2);
			}
			ImageSource imageSource = ShowBitmap.ToBitmapSource();
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
		catch (Exception ex2)
		{
			string text2 = "BW03-IRToolWindow-DoThdImage()-Draw to Screen-";
			text2 += ex2.ToString();
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

	private void Bor_SearchRect1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void SetConditionText()
	{
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
			cnv_Image.Cursor = Cursors.Cross;
			ColorDropper = true;
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
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				SetRotateStatus(inspection.irTool.ResultIsA);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-IRToolWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
				DontSave = false;
				mat = new Mat();
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				SetRotateStatus(inspection.irTool.ResultIsA);
				btn_Test.IsEnabled = true;
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-IRToolWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Btn_Tracker1SizeChange_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Btn_ReferenceLock_Click(object sender, RoutedEventArgs e)
	{
		if (cb_ColorReference.SelectedIndex <= 0)
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
		else
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
		WriteUIValueToParam(RingIndex);
		UpdateUI = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ColorPaste_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Binary Threshold value will Overwrite！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("目前二值化数值将被覆盖！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("目前二值化數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
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
		return result;
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

	private void Btn_TrackerMoveUp_Click(object sender, RoutedEventArgs e)
	{
		_ = (Button)sender;
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT13-IRToolWindow-SetComDigitsKeyboard()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
	private void WriteBinaryParam(int ringid)
	{
		try
		{
			INSP_BINARYTOOL_PARAM iNSP_BINARYTOOL_PARAM = inspection.paramFile.IRToolParam.ColorRings[ringid];
			iNSP_BINARYTOOL_PARAM.ValueChangeFlag = true;
			iNSP_BINARYTOOL_PARAM.TrackerON = true;
			iNSP_BINARYTOOL_PARAM.SearchRect2ON = false;
			iNSP_BINARYTOOL_PARAM.MaskSelectIndex = 0;
			iNSP_BINARYTOOL_PARAM.UseMaskAsInspArea = false;
			iNSP_BINARYTOOL_PARAM.MaskDE_Start1_Index = USE_DILATE_ERODE.EM_NOTUSE;
			iNSP_BINARYTOOL_PARAM.MaskDE_Start1_Count = 0;
			iNSP_BINARYTOOL_PARAM.ColorReferenceIndex = cb_ColorReference.SelectedIndex;
			iNSP_BINARYTOOL_PARAM.EnableSoftCorrection = false;
			_ = cb_Ring1.Text;
			iNSP_BINARYTOOL_PARAM.TrackerName = string.Format("{0}", CBRings[ringid].Text);
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
				iNSP_BINARYTOOL_PARAM.THD1_Min = (int)rs_THD1.LowerValue;
				iNSP_BINARYTOOL_PARAM.THD1_Max = (int)rs_THD1.UpperValue;
				iNSP_BINARYTOOL_PARAM.THD2_Min = (int)rs_THD2.LowerValue;
				iNSP_BINARYTOOL_PARAM.THD2_Max = (int)rs_THD2.UpperValue;
				iNSP_BINARYTOOL_PARAM.THD3_Min = (int)rs_THD3.LowerValue;
				iNSP_BINARYTOOL_PARAM.THD3_Max = (int)rs_THD3.UpperValue;
				iNSP_BINARYTOOL_PARAM.THD4_Min = (int)rs_THD4.LowerValue;
				iNSP_BINARYTOOL_PARAM.THD4_Max = (int)rs_THD4.UpperValue;
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
			iNSP_BINARYTOOL_PARAM.BlobAnalysisSelect = 0;
			iNSP_BINARYTOOL_PARAM.Tracker1UseAutoSize = false;
			iNSP_BINARYTOOL_PARAM.Tracker1AligmentXIndex = 0;
			iNSP_BINARYTOOL_PARAM.Tracker1AligmentYIndex = 0;
			iNSP_BINARYTOOL_PARAM.AutoSize1_SelectIndex = 0;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (GetTracker(ref rect, ringid))
			{
				iNSP_BINARYTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[0].Value;
				iNSP_BINARYTOOL_PARAM.Track1_R = iNSP_BINARYTOOL_PARAM.Track1_L + rect.Width;
				iNSP_BINARYTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[0].Value;
				iNSP_BINARYTOOL_PARAM.Track1_B = iNSP_BINARYTOOL_PARAM.Track1_T + rect.Height;
				iNSP_BINARYTOOL_PARAM.SearchRect1 = rect;
				iNSP_BINARYTOOL_PARAM.UseFontMask = false;
				inspection.paramFile.IRToolParam.ColorRings[ringid] = iNSP_BINARYTOOL_PARAM;
			}
		}
		catch (Exception ex)
		{
			string text = "BW06-IRToolWindow-WriteBinaryParam()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void WriteTrackParam(int ringid)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		if (GetTracker(ref rect, ringid))
		{
			inspection.paramFile.IRToolParam.ColorRings[ringid].ValueChangeFlag = true;
			inspection.paramFile.IRToolParam.ColorRings[ringid].TrackerON = true;
			inspection.paramFile.IRToolParam.ColorRings[ringid].Track1_L = rect.X - inspection.paramFile.VSLocateParam[0].Value;
			inspection.paramFile.IRToolParam.ColorRings[ringid].Track1_R = inspection.paramFile.IRToolParam.ColorRings[ringid].Track1_L + rect.Width;
			inspection.paramFile.IRToolParam.ColorRings[ringid].Track1_T = rect.Y - inspection.paramFile.HSLocateParam[0].Value;
			inspection.paramFile.IRToolParam.ColorRings[ringid].Track1_B = inspection.paramFile.IRToolParam.ColorRings[ringid].Track1_T + rect.Height;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam(int ringid)
	{
		if (PreviewMode || (User < User_Permissions.USER_Administrator && inspection.paramFile.BinaryParam[TrackIndex].Locked))
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
			_ = new int[4]
			{
				(inspection.paramFile.IRToolParam.RingsColor1 = cb_Ring1.SelectedIndex),
				(inspection.paramFile.IRToolParam.RingsColor2 = cb_Ring2.SelectedIndex),
				(inspection.paramFile.IRToolParam.RingsColor3 = cb_Ring3.SelectedIndex),
				(inspection.paramFile.IRToolParam.RingsColor4 = cb_Ring4.SelectedIndex)
			};
			inspection.paramFile.IRToolParam.RingsType = cb_RingCount.SelectedIndex;
			inspection.paramFile.IRToolParam.ParetoIndex = cb_ParetoChart.SelectedIndex;
			inspection.paramFile.IRToolParam.ErrorBin = (FLOW_BIN)cb_BinaryErrorBin.SelectedIndex;
			inspection.paramFile.IRToolParam.ColorRings[0].ColorReferenceIndex = cb_Ring1.SelectedIndex;
			inspection.paramFile.IRToolParam.ColorRings[1].ColorReferenceIndex = cb_Ring2.SelectedIndex;
			inspection.paramFile.IRToolParam.ColorRings[2].ColorReferenceIndex = cb_Ring3.SelectedIndex;
			inspection.paramFile.IRToolParam.ColorRings[3].ColorReferenceIndex = cb_Ring4.SelectedIndex;
			switch (cb_RingCount.SelectedIndex)
			{
			case 0:
				inspection.paramFile.IRToolParam.ColorRings[1].TrackerON = false;
				inspection.paramFile.IRToolParam.ColorRings[2].TrackerON = false;
				inspection.paramFile.IRToolParam.ColorRings[3].TrackerON = false;
				break;
			case 1:
				inspection.paramFile.IRToolParam.ColorRings[1].TrackerON = true;
				inspection.paramFile.IRToolParam.ColorRings[2].TrackerON = false;
				inspection.paramFile.IRToolParam.ColorRings[3].TrackerON = false;
				break;
			case 2:
				inspection.paramFile.IRToolParam.ColorRings[1].TrackerON = true;
				inspection.paramFile.IRToolParam.ColorRings[2].TrackerON = true;
				inspection.paramFile.IRToolParam.ColorRings[3].TrackerON = false;
				break;
			case 3:
				inspection.paramFile.IRToolParam.ColorRings[1].TrackerON = true;
				inspection.paramFile.IRToolParam.ColorRings[2].TrackerON = true;
				inspection.paramFile.IRToolParam.ColorRings[3].TrackerON = true;
				break;
			}
			if (ringid >= 0)
			{
				WriteBinaryParam(ringid);
			}
			switch (cb_RingCount.SelectedIndex)
			{
			case 0:
				WriteTrackParam(0);
				break;
			case 1:
				WriteTrackParam(0);
				WriteTrackParam(1);
				break;
			case 2:
				WriteTrackParam(0);
				WriteTrackParam(1);
				WriteTrackParam(2);
				break;
			case 3:
				WriteTrackParam(0);
				WriteTrackParam(1);
				WriteTrackParam(2);
				WriteTrackParam(3);
				break;
			}
		}
		catch (Exception ex)
		{
			string text = "BW06-IRToolWindow-WriteUIValueToParam()-";
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
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
				btn_BinaryTest_Click(this, null);
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

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
		}
	}

	private void Cnv_Image_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if ((cb_ColorReference.SelectedIndex > 0 && !btn_ReferenceSave.IsEnabled) || !ColorDropper)
		{
			return;
		}
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

	private void Btn_ChannelPreview1_MouseEnter(object sender, MouseEventArgs e)
	{
		if (!PreviewMode)
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			new Mat();
			Mat mat = inspection.buffers.GetChannelMat(THD_Channel[num].SelectedIndex).Clone();
			if (RotateStatus)
			{
				new Mat(mat, LocateRect);
				Cv2.Flip(mat, mat, FlipMode.XY);
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Btn_ChannelPreview1_MouseLeave(object sender, MouseEventArgs e)
	{
		if (!PreviewMode)
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
		if (!PreviewMode)
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			Mat mat = new Mat();
			Mat mat2 = inspection.buffers.GetChannelMat(THD_Channel[num].SelectedIndex).Clone();
			if (RotateStatus)
			{
				new Mat(mat2, LocateRect);
				Cv2.Flip(mat2, mat2, FlipMode.XY);
			}
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
			Mat mat = inspection.buffers.GetChannelMat(THD_Channel[num].SelectedIndex).Clone();
			if (RotateStatus)
			{
				new Mat(mat, LocateRect);
				Cv2.Flip(mat, mat, FlipMode.XY);
			}
			Mat histogram = GetHistogram(num, mat);
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
		}
		else
		{
			exp_TrackerFilllet.Visibility = Visibility.Hidden;
			TrackerFillletShow = false;
			bor_SearchRect1.Visibility = Visibility.Visible;
			DoThdImage();
		}
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
			DontSave = false;
			btn_Test.IsEnabled = true;
			SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
			Btn_Test_Click(this, null);
		}
	}

	private void Bor_SearchRect1_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			DoThdImage();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyBinaryValueToUI(int ringid)
	{
		TrackIndex = ringid;
		INSP_BINARYTOOL_PARAM backupParam = inspection.paramFile.IRToolParam.ColorRings[ringid];
		new OpenCvSharp.Rect(0, 0, 50, 50);
		txt_AllResult.Clear();
		if (backupParam.TrackerON)
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
			inspection.paramFile.EnableJudgeWithAdvMeasure = false;
			lab_LockMode.Visibility = Visibility.Hidden;
			SetConditionText();
		}
		catch (Exception ex)
		{
			UpdateUI = true;
			string text = "BW07-IRToolWindow-CopyValueToUI()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[ringid], ringid, CBRings[ringid].SelectedIndex);
		try
		{
			bool updateUI = UpdateUI;
			updateUI = UpdateUI;
			UpdateUI = false;
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
			cb_ColorReference.SelectedIndex = backupParam.ColorReferenceIndex;
			UpdateUI = updateUI;
			if (backupParam.ColorReferenceIndex == 0)
			{
				grd_SIMP_RefSetup.Background = Brushes.White;
				btn_ReferenceSave.IsEnabled = false;
				txt_ReferenceName.IsEnabled = false;
				txt_ReferenceName.Text = "";
				for (int j = 0; j < 4; j++)
				{
					THD_Sliders[j].IsEnabled = true;
					THD_Expender[j].IsEnabled = true;
					channelShiftMin[j] = 0;
					channelShiftMax[j] = 0;
				}
				cb_Channel_THD1.SelectedIndex = (int)backupParam.THD1_ChannelIndex;
				cb_Channel_THD2.SelectedIndex = (int)backupParam.THD2_ChannelIndex;
				cb_Channel_THD3.SelectedIndex = (int)backupParam.THD3_ChannelIndex;
				cb_Channel_THD4.SelectedIndex = (int)backupParam.THD4_ChannelIndex;
				chk_Thd_ORMode.IsChecked = backupParam.THD_ORMode;
				if (backupParam.THD_ORMode)
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
				if (backupParam.TrackerON)
				{
					exp_THD1.IsExpanded = true;
					exp_THD2.IsExpanded = backupParam.THD2_ChannelON;
					exp_THD3.IsExpanded = backupParam.THD3_ChannelON;
					exp_THD4.IsExpanded = backupParam.THD4_ChannelON;
				}
				backupParam.EnableSoftCorrection = false;
				Button button = btn_SoftCorrectionTHD2;
				Button button2 = btn_SoftCorrectionTHD3;
				Visibility visibility = (btn_SoftCorrectionTHD4.Visibility = Visibility.Hidden);
				Visibility visibility3 = (button2.Visibility = visibility);
				button.Visibility = visibility3;
				Label label = lab_SoftCorrectionShiftTHD1;
				Label label2 = lab_SoftCorrectionShiftTHD2;
				Label label3 = lab_SoftCorrectionShiftTHD3;
				Visibility visibility5 = (lab_SoftCorrectionShiftTHD4.Visibility = Visibility.Hidden);
				visibility = (label3.Visibility = visibility5);
				visibility3 = (label2.Visibility = visibility);
				label.Visibility = visibility3;
				rs_THD1.LowerValue = backupParam.THD1_Min;
				rs_THD1.UpperValue = backupParam.THD1_Max;
				nu_Thd1LimitMin.Value = backupParam.THD1_Min;
				nu_Thd1LimitMax.Value = backupParam.THD1_Max;
				rs_THD2.LowerValue = backupParam.THD2_Min;
				rs_THD2.UpperValue = backupParam.THD2_Max;
				nu_Thd2LimitMin.Value = backupParam.THD2_Min;
				nu_Thd2LimitMax.Value = backupParam.THD2_Max;
				rs_THD3.LowerValue = backupParam.THD3_Min;
				rs_THD3.UpperValue = backupParam.THD3_Max;
				nu_Thd3LimitMin.Value = backupParam.THD3_Min;
				nu_Thd3LimitMax.Value = backupParam.THD3_Max;
				rs_THD4.LowerValue = backupParam.THD4_Min;
				rs_THD4.UpperValue = backupParam.THD4_Max;
				nu_Thd4LimitMin.Value = backupParam.THD4_Min;
				nu_Thd4LimitMax.Value = backupParam.THD4_Max;
				cb_THD1_DEStart.SelectedIndex = (int)backupParam.DE_Start1_Index;
				cb_THD1_DEEnd.SelectedIndex = (int)backupParam.DE_End1_Index;
				cb_THD2_DEStart.SelectedIndex = (int)backupParam.DE_Start2_Index;
				cb_THD2_DEEnd.SelectedIndex = (int)backupParam.DE_End2_Index;
				cb_THD3_DEStart.SelectedIndex = (int)backupParam.DE_Start3_Index;
				cb_THD3_DEEnd.SelectedIndex = (int)backupParam.DE_End3_Index;
				cb_THD4_DEStart.SelectedIndex = (int)backupParam.DE_Start4_Index;
				cb_THD4_DEEnd.SelectedIndex = (int)backupParam.DE_End4_Index;
				cb_THD1_DEStartCount.SelectedIndex = backupParam.DE_Start1_Count;
				cb_THD1_DEEndCount.SelectedIndex = backupParam.DE_End1_Count;
				cb_THD2_DEStartCount.SelectedIndex = backupParam.DE_Start2_Count;
				cb_THD2_DEEndCount.SelectedIndex = backupParam.DE_End2_Count;
				cb_THD3_DEStartCount.SelectedIndex = backupParam.DE_Start3_Count;
				cb_THD3_DEEndCount.SelectedIndex = backupParam.DE_End3_Count;
				cb_THD4_DEStartCount.SelectedIndex = backupParam.DE_Start4_Count;
				cb_THD4_DEEndCount.SelectedIndex = backupParam.DE_End4_Count;
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
				INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = inspection.paramFile.ColorReferenceParam[backupParam.ColorReferenceIndex - 1];
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
			string text2 = "BW07-IRToolWindow-CopyValueToUI()-03-";
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
			if (backupParam.InspRule1_Index >= BlobCondition.EM_2BLOB_Ratio && backupParam.InspRule1_Index <= BlobCondition.EM_2BLOB_WRatio)
			{
				cb_InspRule1.SelectedIndex = 0;
			}
			else
			{
				cb_InspRule1.SelectedIndex = (int)backupParam.InspRule1_Index;
			}
			if (backupParam.InspRule2_Index >= BlobCondition.EM_2BLOB_Ratio && backupParam.InspRule2_Index <= BlobCondition.EM_2BLOB_WRatio)
			{
				cb_InspRule2.SelectedIndex = 0;
			}
			else
			{
				cb_InspRule2.SelectedIndex = (int)backupParam.InspRule2_Index;
			}
			if (backupParam.InspRule3_Index >= BlobCondition.EM_2BLOB_Ratio && backupParam.InspRule3_Index <= BlobCondition.EM_2BLOB_WRatio)
			{
				cb_InspRule3.SelectedIndex = 0;
			}
			else
			{
				cb_InspRule3.SelectedIndex = (int)backupParam.InspRule3_Index;
			}
			if (backupParam.InspRule4_Index >= BlobCondition.EM_2BLOB_Ratio && backupParam.InspRule4_Index <= BlobCondition.EM_2BLOB_WRatio)
			{
				cb_InspRule4.SelectedIndex = 0;
			}
			else
			{
				cb_InspRule4.SelectedIndex = (int)backupParam.InspRule4_Index;
			}
			if (backupParam.InspRuleAND)
			{
				rad_RuleAND.IsChecked = true;
			}
			else
			{
				rad_RuleOR.IsChecked = true;
			}
			txt_InspRule1Min.Text = GetComDigitsStr(backupParam.InspRule1_Min, txt_InspRule1Min);
			txt_InspRule2Min.Text = GetComDigitsStr(backupParam.InspRule2_Min, txt_InspRule2Min);
			txt_InspRule3Min.Text = GetComDigitsStr(backupParam.InspRule3_Min, txt_InspRule3Min);
			txt_InspRule4Min.Text = GetComDigitsStr(backupParam.InspRule4_Min, txt_InspRule4Min);
			txt_InspRule1Max.Text = GetComDigitsStr(backupParam.InspRule1_Max, txt_InspRule1Max);
			txt_InspRule2Max.Text = GetComDigitsStr(backupParam.InspRule2_Max, txt_InspRule2Max);
			txt_InspRule3Max.Text = GetComDigitsStr(backupParam.InspRule3_Max, txt_InspRule3Max);
			txt_InspRule4Max.Text = GetComDigitsStr(backupParam.InspRule4_Max, txt_InspRule4Max);
			cb_BinaryErrorBin.SelectedIndex = (int)backupParam.ErrorBin;
			cb_ParetoChart.SelectedIndex = backupParam.ParetoIndex;
			tab_BlobAnalysis.SelectedIndex = backupParam.BlobAnalysisSelect;
			BackupParam = backupParam;
		}
		catch (Exception ex3)
		{
			string text3 = "BW07-IRToolWindow-CopyValueToUI()-04-";
			text3 += ex3.ToString();
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	private void CopyValueToUI(int ringid)
	{
		UpdateUI = false;
		DontSave = false;
		DontSave1 = (DontSave2 = false);
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
			btn_BinaryTest.Visibility = Visibility.Hidden;
		}
		int[] array = new int[4];
		int num = (cb_Ring1.SelectedIndex = inspection.paramFile.IRToolParam.RingsColor1);
		array[0] = num;
		num = (cb_Ring2.SelectedIndex = inspection.paramFile.IRToolParam.RingsColor2);
		array[1] = num;
		num = (cb_Ring3.SelectedIndex = inspection.paramFile.IRToolParam.RingsColor3);
		array[2] = num;
		num = (cb_Ring4.SelectedIndex = inspection.paramFile.IRToolParam.RingsColor4);
		array[3] = num;
		for (int i = 0; i < 4; i++)
		{
			SetRingColor(i, array[i]);
		}
		num = (cb_RingCount.SelectedIndex = inspection.paramFile.IRToolParam.RingsType);
		int ringtype = num;
		UpdateRingType(ringtype);
		cb_BinaryErrorBin.SelectedIndex = (int)inspection.paramFile.IRToolParam.ErrorBin;
		cb_ParetoChart.SelectedIndex = inspection.paramFile.IRToolParam.ParetoIndex;
		UpdateUI = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetBinaryTracker(INSP_BINARYTOOL_PARAM param, int ringid, int ringcolor)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		try
		{
			int width = param.Track1_R - param.Track1_L;
			int height = param.Track1_B - param.Track1_T;
			rect.X = inspection.paramFile.VSLocateParam[0].Value + param.Track1_L;
			rect.Y = inspection.paramFile.HSLocateParam[0].Value + param.Track1_T;
			rect.Width = width;
			rect.Height = height;
		}
		catch (Exception ex)
		{
			string text = "BW07-IRToolWindow-CopyValueToUI()-05-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		try
		{
			SetTracker(rect, ringid);
		}
		catch (Exception ex2)
		{
			string text2 = "BW07-IRToolWindow-CopyValueToUI()-06-";
			text2 += ex2.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode || !inited || !UpdateUI || DontSave || DontSave1 || DontSave2)
		{
			return;
		}
		Label_ResultError.Visibility = Visibility.Hidden;
		WriteUIValueToParam(-1);
		TimeStart();
		TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
		bool num = inspection.irTool.Inspect(ref iresult);
		double num2 = TimeStop();
		lab_Time.Content = string.Format("{0:N3}", num2);
		txt_AllResult.Clear();
		txt_AllResult.Text += inspection.irTool.ErrorStr;
		int num3 = cb_RingCount.SelectedIndex + 1;
		if (num)
		{
			for (int i = 0; i < num3; i++)
			{
				ELRingsResult[i].Fill = Brushes.Lime;
				LABResult[i].Visibility = Visibility.Hidden;
			}
			SetResult(result: true);
		}
		else
		{
			for (int j = 0; j < num3; j++)
			{
				if (!inspection.irTool.FinalRingResult[j])
				{
					ELRingsResult[j].Fill = Brushes.Red;
					LABResult[j].Visibility = Visibility.Visible;
				}
				else
				{
					ELRingsResult[j].Fill = Brushes.Lime;
					LABResult[j].Visibility = Visibility.Hidden;
				}
			}
			SetResult(result: false);
		}
		Mat mat = ShowBitmap.Clone();
		if (!RotateStatus)
		{
			Mat mat2 = new Mat(mat, LocateRect);
			Cv2.Flip(mat2, mat2, FlipMode.XY);
		}
		if (RotateStatus)
		{
			btn_Result_FlipResult.Visibility = Visibility.Visible;
		}
		else
		{
			btn_Result_FlipResult.Visibility = Visibility.Hidden;
		}
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
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
		btn_BinaryTest_Click(this, null);
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
			INSP_BINARYTOOL_PARAM iNSP_BINARYTOOL_PARAM = inspection.paramFile.BinaryParam[index];
			string text4 = string.Format("{0}\t[{1}][{2}{3}-{4}] ", text3, inspection.StationTxt, LanguageManager.Instance["Binary"], index + 1, iNSP_BINARYTOOL_PARAM.TrackerName);
			if (iNSP_BINARYTOOL_PARAM.TrackerON != BackupParam.TrackerON)
			{
				if (iNSP_BINARYTOOL_PARAM.TrackerON)
				{
					ModifyLog += string.Format("{0} [{1}]\n", text4, LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1}]\n", text4, LanguageManager.Instance["Close"]);
				}
			}
			if (iNSP_BINARYTOOL_PARAM.THD1_ChannelIndex != BackupParam.THD1_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD1_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)iNSP_BINARYTOOL_PARAM.THD1_ChannelIndex));
			}
			if (iNSP_BINARYTOOL_PARAM.THD1_Min != BackupParam.THD1_Min)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD1_Min, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD1_Min);
			}
			if (iNSP_BINARYTOOL_PARAM.THD1_Max != BackupParam.THD1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD1_Max, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD1_Max);
			}
			if (iNSP_BINARYTOOL_PARAM.THD2_ChannelIndex != BackupParam.THD2_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD2_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)iNSP_BINARYTOOL_PARAM.THD2_ChannelIndex));
			}
			if (iNSP_BINARYTOOL_PARAM.THD2_Min != BackupParam.THD2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD2_Min, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD2_Min);
			}
			if (iNSP_BINARYTOOL_PARAM.THD2_Max != BackupParam.THD2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD2_Max, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD2_Max);
			}
			if (iNSP_BINARYTOOL_PARAM.THD2_ChannelON != BackupParam.THD2_ChannelON)
			{
				if (iNSP_BINARYTOOL_PARAM.THD2_ChannelON)
				{
					ModifyLog += string.Format("{0} [({1}2) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}2) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			if (iNSP_BINARYTOOL_PARAM.THD3_ChannelIndex != BackupParam.THD3_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD3_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)iNSP_BINARYTOOL_PARAM.THD3_ChannelIndex));
			}
			if (iNSP_BINARYTOOL_PARAM.THD3_Min != BackupParam.THD3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD3_Min, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD3_Min);
			}
			if (iNSP_BINARYTOOL_PARAM.THD3_Max != BackupParam.THD3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD3_Max, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD3_Max);
			}
			if (iNSP_BINARYTOOL_PARAM.THD3_ChannelON != BackupParam.THD3_ChannelON)
			{
				if (iNSP_BINARYTOOL_PARAM.THD3_ChannelON)
				{
					ModifyLog += string.Format("{0} [({1}3) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}3) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			if (iNSP_BINARYTOOL_PARAM.THD4_ChannelIndex != BackupParam.THD4_ChannelIndex)
			{
				ModifyLog += string.Format("{0} [({1}4) {2} {3} {4}]\n", text4, LanguageManager.Instance["Channel"], modifyLogFun.GetChannelText((int)BackupParam.THD4_ChannelIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetChannelText((int)iNSP_BINARYTOOL_PARAM.THD4_ChannelIndex));
			}
			if (iNSP_BINARYTOOL_PARAM.THD4_Min != BackupParam.THD4_Min)
			{
				ModifyLog += string.Format("{0} [({1}4) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdUp"], BackupParam.THD4_Min, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD4_Min);
			}
			if (iNSP_BINARYTOOL_PARAM.THD4_Max != BackupParam.THD4_Max)
			{
				ModifyLog += string.Format("{0} [({1}4) {2}: {3} {4} {5}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["ThdDown"], BackupParam.THD4_Max, LanguageManager.Instance["ChangedTo"], iNSP_BINARYTOOL_PARAM.THD4_Max);
			}
			if (iNSP_BINARYTOOL_PARAM.THD4_ChannelON != BackupParam.THD4_ChannelON)
			{
				if (iNSP_BINARYTOOL_PARAM.THD4_ChannelON)
				{
					ModifyLog += string.Format("{0} [({1}4) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Open"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}4) {2}]\n", text4, LanguageManager.Instance["Channel"], LanguageManager.Instance["Close"]);
				}
			}
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_Start1_Index, (int)BackupParam.DE_Start1_Index, iNSP_BINARYTOOL_PARAM.DE_Start1_Count, BackupParam.DE_Start1_Count, 1, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_End1_Index, (int)BackupParam.DE_End1_Index, iNSP_BINARYTOOL_PARAM.DE_End1_Count, BackupParam.DE_End1_Count, 1, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_Start2_Index, (int)BackupParam.DE_Start2_Index, iNSP_BINARYTOOL_PARAM.DE_Start2_Count, BackupParam.DE_Start2_Count, 2, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_End2_Index, (int)BackupParam.DE_End2_Index, iNSP_BINARYTOOL_PARAM.DE_End2_Count, BackupParam.DE_End2_Count, 2, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_Start3_Index, (int)BackupParam.DE_Start3_Index, iNSP_BINARYTOOL_PARAM.DE_Start3_Count, BackupParam.DE_Start3_Count, 3, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_End3_Index, (int)BackupParam.DE_End3_Index, iNSP_BINARYTOOL_PARAM.DE_End3_Count, BackupParam.DE_End3_Count, 3, 2, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_Start4_Index, (int)BackupParam.DE_Start4_Index, iNSP_BINARYTOOL_PARAM.DE_Start4_Count, BackupParam.DE_Start4_Count, 4, 1, text4);
			ModifyLog += modifyLogFun.GetMorphModifyText((int)iNSP_BINARYTOOL_PARAM.DE_End4_Index, (int)BackupParam.DE_End4_Index, iNSP_BINARYTOOL_PARAM.DE_End4_Count, BackupParam.DE_End4_Count, 4, 2, text4);
			if (iNSP_BINARYTOOL_PARAM.BlobAnalysisSelect != BackupParam.BlobAnalysisSelect)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule"], modifyLogFun.GetBlobAnalysisText(BackupParam.BlobAnalysisSelect), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobAnalysisText(iNSP_BINARYTOOL_PARAM.BlobAnalysisSelect));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRuleAND != BackupParam.InspRuleAND)
			{
				if (iNSP_BINARYTOOL_PARAM.InspRuleAND)
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["AadOrRule"], "OR", LanguageManager.Instance["ChangedTo"], "AND");
				}
				else
				{
					ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["AadOrRule"], "AND", LanguageManager.Instance["ChangedTo"], "OR");
				}
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule1_Index != BackupParam.InspRule1_Index)
			{
				ModifyLog += string.Format("{0} [({1}1) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule1_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule1_Index));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule1_Min != BackupParam.InspRule1_Min)
			{
				ModifyLog += string.Format("{0} [({1}1-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule1_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule1_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule1_Min));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule1_Max != BackupParam.InspRule1_Max)
			{
				ModifyLog += string.Format("{0} [({1}1-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule1_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule1_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule1_Max));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule2_Index != BackupParam.InspRule2_Index)
			{
				ModifyLog += string.Format("{0} [({1}2) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule2_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule2_Index));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule2_Min != BackupParam.InspRule2_Min)
			{
				ModifyLog += string.Format("{0} [({1}2-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule2_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule2_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule2_Min));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule2_Max != BackupParam.InspRule2_Max)
			{
				ModifyLog += string.Format("{0} [({1}2-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule2_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule2_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule2_Max));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule3_Index != BackupParam.InspRule3_Index)
			{
				ModifyLog += string.Format("{0} [({1}3) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule3_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule3_Index));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule3_Min != BackupParam.InspRule3_Min)
			{
				ModifyLog += string.Format("{0} [({1}3-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule3_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule3_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule3_Min));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule3_Max != BackupParam.InspRule3_Max)
			{
				ModifyLog += string.Format("{0} [({1}3-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule3_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule3_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule3_Max));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule4_Index != BackupParam.InspRule4_Index)
			{
				ModifyLog += string.Format("{0} [({1}4) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)BackupParam.InspRule4_Index), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule4_Index));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule4_Min != BackupParam.InspRule4_Min)
			{
				ModifyLog += string.Format("{0} [({1}4-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule4_Index), LanguageManager.Instance["Min2"], GetComDigitsStr(BackupParam.InspRule4_Min), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule4_Min));
			}
			if (iNSP_BINARYTOOL_PARAM.InspRule4_Max != BackupParam.InspRule4_Max)
			{
				ModifyLog += string.Format("{0} [({1}4-{2}) {3}: {4} {5} {6}]\n", text4, LanguageManager.Instance["InspRule2"], modifyLogFun.GetBlobRuleText((int)iNSP_BINARYTOOL_PARAM.InspRule4_Index), LanguageManager.Instance["Max2"], GetComDigitsStr(BackupParam.InspRule4_Max), LanguageManager.Instance["ChangedTo"], GetComDigitsStr(iNSP_BINARYTOOL_PARAM.InspRule4_Max));
			}
			if (iNSP_BINARYTOOL_PARAM.ErrorBin != BackupParam.ErrorBin)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["InspErrorBin"], modifyLogFun.GetErrorBinText((int)BackupParam.ErrorBin), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetErrorBinText((int)iNSP_BINARYTOOL_PARAM.ErrorBin));
			}
			if (iNSP_BINARYTOOL_PARAM.EnableSelectFace != BackupParam.EnableSelectFace)
			{
				if (!iNSP_BINARYTOOL_PARAM.EnableSelectFace)
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Close"]);
				}
				else
				{
					ModifyLog += string.Format("{0} [{1} {2} {3}]\n", text4, LanguageManager.Instance["EnableSelectFace"], LanguageManager.Instance["ChangedTo"], LanguageManager.Instance["Open"]);
				}
			}
			if (iNSP_BINARYTOOL_PARAM.FaceIndex != BackupParam.FaceIndex)
			{
				ModifyLog += string.Format("{0} [({1}) {2} {3} {4}]\n", text4, LanguageManager.Instance["SelectFaceTool"], modifyLogFun.GetSelectFaceText(BackupParam.FaceIndex), LanguageManager.Instance["ChangedTo"], modifyLogFun.GetSelectFaceText(iNSP_BINARYTOOL_PARAM.FaceIndex));
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
			string text5 = "BW10-IRToolWindow-WriteModifyLog()-";
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
			if (RotateStatus)
			{
				new Mat(mat, LocateRect);
				Cv2.Flip(mat, mat, FlipMode.XY);
			}
			Mat mat2 = mat.Clone();
			double num = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
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
				mat = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
				mat2 = mat.Clone();
				num = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
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
				mat = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
				mat2 = mat.Clone();
				num = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
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
				mat = new Mat(inspection.buffers.GetLocatedBGR(selectedIndex), rect);
				mat2 = mat.Clone();
				num = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
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
			string text = "BW10-IRToolWindow-Btn_OTSUL_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
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
	private void tab_Function_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !(e.AddedItems[0] is TabItem tabItem))
		{
			return;
		}
		if (tabItem.Name == "tab_Main")
		{
			for (int i = 0; i < 4; i++)
			{
				BTNRings[i].Background = Brushes.White;
			}
			UpdateRingType(cb_RingCount.SelectedIndex);
		}
		else if (UpdateUI)
		{
			bool updateUI = UpdateUI;
			UpdateUI = false;
			BTNRings[0].Background = Brushes.LightGoldenrodYellow;
			CopyBinaryValueToUI(0);
			BDTrackers[0].IsEnabled = true;
			UpdateUI = updateUI;
		}
	}

	private void btn_Ring1_Click(object sender, RoutedEventArgs e)
	{
		int num = (RingIndex = Convert.ToInt32(((Button)sender).Uid));
		for (int i = 0; i < 4; i++)
		{
			if (i == num)
			{
				BTNRings[i].Background = Brushes.LightGoldenrodYellow;
			}
			else
			{
				BTNRings[i].Background = Brushes.White;
			}
			BDTrackers[i].Visibility = Visibility.Hidden;
		}
		BDTrackers[num].Visibility = Visibility.Visible;
		BDTrackers[num].IsEnabled = true;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		tab_Function.SelectedIndex = 1;
		CopyBinaryValueToUI(num);
		UpdateUI = updateUI;
		btn_BinaryTest_Click(this, null);
	}

	private void UpdateRingType(int ringtype)
	{
		switch (ringtype)
		{
		case 0:
			GRDRings[1].Visibility = Visibility.Hidden;
			GRDRings[2].Visibility = Visibility.Hidden;
			GRDRings[3].Visibility = Visibility.Hidden;
			RTRings[1].Visibility = Visibility.Hidden;
			RTRings[2].Visibility = Visibility.Hidden;
			RTRings[3].Visibility = Visibility.Hidden;
			LABRings[1].Visibility = Visibility.Hidden;
			LABRings[2].Visibility = Visibility.Hidden;
			LABRings[3].Visibility = Visibility.Hidden;
			BDTrackers[0].Visibility = Visibility.Visible;
			BDTrackers[1].Visibility = Visibility.Hidden;
			BDTrackers[2].Visibility = Visibility.Hidden;
			BDTrackers[3].Visibility = Visibility.Hidden;
			BTNRings[1].Visibility = Visibility.Hidden;
			BTNRings[2].Visibility = Visibility.Hidden;
			BTNRings[3].Visibility = Visibility.Hidden;
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[0], 0, CBRings[0].SelectedIndex);
			break;
		case 1:
			GRDRings[1].Visibility = Visibility.Visible;
			GRDRings[2].Visibility = Visibility.Hidden;
			GRDRings[3].Visibility = Visibility.Hidden;
			RTRings[1].Visibility = Visibility.Visible;
			RTRings[2].Visibility = Visibility.Hidden;
			RTRings[3].Visibility = Visibility.Hidden;
			LABRings[1].Visibility = Visibility.Visible;
			LABRings[2].Visibility = Visibility.Hidden;
			LABRings[3].Visibility = Visibility.Hidden;
			BDTrackers[0].Visibility = Visibility.Visible;
			BDTrackers[1].Visibility = Visibility.Visible;
			BDTrackers[2].Visibility = Visibility.Hidden;
			BDTrackers[3].Visibility = Visibility.Hidden;
			BTNRings[1].Visibility = Visibility.Visible;
			BTNRings[2].Visibility = Visibility.Hidden;
			BTNRings[3].Visibility = Visibility.Hidden;
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[0], 0, CBRings[0].SelectedIndex);
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[1], 1, CBRings[1].SelectedIndex);
			break;
		case 2:
			GRDRings[1].Visibility = Visibility.Visible;
			GRDRings[2].Visibility = Visibility.Visible;
			GRDRings[3].Visibility = Visibility.Hidden;
			RTRings[1].Visibility = Visibility.Visible;
			RTRings[2].Visibility = Visibility.Visible;
			RTRings[3].Visibility = Visibility.Hidden;
			LABRings[1].Visibility = Visibility.Visible;
			LABRings[2].Visibility = Visibility.Visible;
			LABRings[3].Visibility = Visibility.Hidden;
			BDTrackers[0].Visibility = Visibility.Visible;
			BDTrackers[1].Visibility = Visibility.Visible;
			BDTrackers[2].Visibility = Visibility.Visible;
			BDTrackers[3].Visibility = Visibility.Hidden;
			BTNRings[1].Visibility = Visibility.Visible;
			BTNRings[2].Visibility = Visibility.Visible;
			BTNRings[3].Visibility = Visibility.Hidden;
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[0], 0, CBRings[0].SelectedIndex);
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[1], 1, CBRings[1].SelectedIndex);
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[2], 2, CBRings[2].SelectedIndex);
			break;
		case 3:
			GRDRings[1].Visibility = Visibility.Visible;
			GRDRings[2].Visibility = Visibility.Visible;
			GRDRings[3].Visibility = Visibility.Visible;
			RTRings[1].Visibility = Visibility.Visible;
			RTRings[2].Visibility = Visibility.Visible;
			RTRings[3].Visibility = Visibility.Visible;
			LABRings[1].Visibility = Visibility.Visible;
			LABRings[2].Visibility = Visibility.Visible;
			LABRings[3].Visibility = Visibility.Visible;
			BDTrackers[0].Visibility = Visibility.Visible;
			BDTrackers[1].Visibility = Visibility.Visible;
			BDTrackers[2].Visibility = Visibility.Visible;
			BDTrackers[3].Visibility = Visibility.Visible;
			BTNRings[1].Visibility = Visibility.Visible;
			BTNRings[2].Visibility = Visibility.Visible;
			BTNRings[3].Visibility = Visibility.Visible;
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[0], 0, CBRings[0].SelectedIndex);
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[1], 1, CBRings[1].SelectedIndex);
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[2], 2, CBRings[2].SelectedIndex);
			GetBinaryTracker(inspection.paramFile.IRToolParam.ColorRings[3], 3, CBRings[3].SelectedIndex);
			break;
		}
	}

	private void cb_RingCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			for (int i = 0; i < 4; i++)
			{
				LABResult[i].Visibility = Visibility.Hidden;
			}
			UpdateRingType(cb_RingCount.SelectedIndex);
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-IRToolWindow-GetComDigitsStr()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return "0";
		}
	}

	private void cb_Ring1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			ComboBox comboBox = (ComboBox)sender;
			int id = Convert.ToInt32(comboBox.Uid);
			SetRingColor(id, comboBox.SelectedIndex);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_BinaryTest_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode || !inited || !UpdateUI)
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
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (!GetTracker(ref rect, RingIndex))
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
			if (cb_ColorReference.SelectedIndex > 0)
			{
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
			}
			WriteBinaryParam(RingIndex);
			Label_ResultError.Visibility = Visibility.Hidden;
			TimeStart();
			string TrackErrorStrA = "";
			bool flag = inspection.irTool.BinaryInspect(TrackIndex, ref TrackErrorStrA, 0, 0, 0, RotateStatus);
			double num = TimeStop();
			lab_Time.Content = string.Format("{0:N3}", num);
			SetResult(flag);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
			Blobs = inspection.irTool.Blobs;
			rect2 = Blobs.NGRect;
			CheckResult();
			DoThdImage();
			bool flag2 = false;
			for (int i = 0; i < 4; i++)
			{
				if (InspRule[i].SelectedIndex == 11)
				{
					flag2 = true;
				}
			}
			if (!flag)
			{
				rt_RingTestResult.Fill = Brushes.Red;
				ShowBitmap.Rectangle(rect2, Scalar.Red);
				if (flag2)
				{
					ShowBitmap.Rectangle(rect, Scalar.Red, 3);
				}
			}
			else
			{
				ShowBitmap.Rectangle(rect2, Scalar.Green);
				rt_RingTestResult.Fill = Brushes.Lime;
			}
			txt_AllResult.Clear();
			txt_AllResult.Text += TrackErrorStrA;
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void btn_ResetTracker_Click(object sender, RoutedEventArgs e)
	{
		BDTrackers[RingIndex].SetValue(Canvas.LeftProperty, 50.0);
		BDTrackers[RingIndex].SetValue(Canvas.TopProperty, 50.0);
		BDTrackers[RingIndex].SetValue(FrameworkElement.WidthProperty, 100.0);
		BDTrackers[RingIndex].SetValue(FrameworkElement.HeightProperty, 100.0);
		BDTrackers[RingIndex].IsEnabled = true;
		BDTrackers[RingIndex].Visibility = Visibility.Visible;
	}

	private void btn_RotateImage_Click(object sender, RoutedEventArgs e)
	{
		RotateStatus = !RotateStatus;
		btn_BinaryTest_Click(sender, e);
	}

	private void btn_RotateImage2_Click(object sender, RoutedEventArgs e)
	{
		RotateStatus = !RotateStatus;
		Btn_Test_Click(sender, e);
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.BinaryParam[TrackIndex].TrackerName) + "BT01-IRToolWindow-ConvertComDigits()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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

	private void SetRingColor(int id, int color)
	{
		SolidColorBrush fill = color switch
		{
			0 => Brushes.Transparent, 
			1 => Brushes.Black, 
			2 => Brushes.Brown, 
			3 => Brushes.Red, 
			4 => Brushes.Orange, 
			5 => Brushes.Yellow, 
			6 => Brushes.LimeGreen, 
			7 => Brushes.Blue, 
			8 => Brushes.BlueViolet, 
			9 => Brushes.Gray, 
			10 => Brushes.White, 
			11 => Brushes.Gold, 
			12 => Brushes.Silver, 
			_ => Brushes.Transparent, 
		};
		RTRings[id].Fill = fill;
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

	public void SetRotateStatus(bool status)
	{
		RotateStatus = !status;
		if (RotateStatus)
		{
			btn_Result_FlipResult.Visibility = Visibility.Visible;
		}
		else
		{
			btn_Result_FlipResult.Visibility = Visibility.Hidden;
		}
		DoThdImage();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/irtoolwindow.xaml", UriKind.Relative);
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
			MainW = (IRToolWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			MainW.Drop += MainW_Drop;
			MainW.ContentRendered += MainW_ContentRendered;
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
			bor_LocateRect = (Border)target;
			break;
		case 5:
			bor_SearchRect2 = (Border)target;
			bor_SearchRect2.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect2.GotFocus += Bor_SearchRect2_GotFocus;
			bor_SearchRect2.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 6:
			txt_Search2Label = (TextBlock)target;
			break;
		case 7:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect1.GotFocus += Bor_SearchRect1_GotFocus;
			bor_SearchRect1.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 8:
			txt_Search1Label = (TextBlock)target;
			break;
		case 9:
			Label_ResultError = (TextBox)target;
			break;
		case 10:
			Label_Result = (Label)target;
			break;
		case 11:
			txt_PreviewMode = (TextBox)target;
			break;
		case 12:
			bor_SearchRect3 = (Border)target;
			bor_SearchRect3.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect3.GotFocus += Bor_SearchRect1_GotFocus;
			bor_SearchRect3.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 13:
			txt_Search1Label1 = (TextBlock)target;
			break;
		case 14:
			bor_SearchRect4 = (Border)target;
			bor_SearchRect4.PreviewMouseLeftButtonDown += Bor_SearchRect1_PreviewMouseLeftButtonDown;
			bor_SearchRect4.GotFocus += Bor_SearchRect1_GotFocus;
			bor_SearchRect4.SizeChanged += Bor_SearchRect1_SizeChanged;
			break;
		case 15:
			txt_Search1Label2 = (TextBlock)target;
			break;
		case 16:
			tab_Function = (TabControl)target;
			tab_Function.SelectionChanged += tab_Function_SelectionChanged;
			break;
		case 17:
			tab_Main = (TabItem)target;
			break;
		case 18:
			grd_Main = (Grid)target;
			break;
		case 19:
			rt_Ring1 = (Rectangle)target;
			break;
		case 20:
			rt_Ring2 = (Rectangle)target;
			break;
		case 21:
			rt_Ring3 = (Rectangle)target;
			break;
		case 22:
			rt_Ring4 = (Rectangle)target;
			break;
		case 23:
			lab_Ring1 = (Label)target;
			break;
		case 24:
			lab_Ring2 = (Label)target;
			break;
		case 25:
			lab_Ring3 = (Label)target;
			break;
		case 26:
			lab_Ring4 = (Label)target;
			break;
		case 27:
			lab_Pareto = (Label)target;
			break;
		case 28:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 29:
			cb_RingCount = (ComboBox)target;
			cb_RingCount.SelectionChanged += cb_RingCount_SelectionChanged;
			break;
		case 30:
			lab_Pareto_Copy = (Label)target;
			break;
		case 31:
			ico_ResultAlart1 = (Label)target;
			break;
		case 32:
			ico_ResultAlart2 = (Label)target;
			break;
		case 33:
			ico_ResultAlart3 = (Label)target;
			break;
		case 34:
			ico_ResultAlart4 = (Label)target;
			break;
		case 35:
			cb_BinaryErrorBin = (ComboBox)target;
			break;
		case 36:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 37:
			rt_Result = (Rectangle)target;
			break;
		case 38:
			btn_RotateImage2 = (Button)target;
			btn_RotateImage2.Click += btn_RotateImage2_Click;
			break;
		case 39:
			grd_Tool = (Grid)target;
			break;
		case 40:
			grd_Ring1 = (Grid)target;
			break;
		case 41:
			es_RingResult1 = (Ellipse)target;
			break;
		case 42:
			cb_Ring1 = (ComboBox)target;
			cb_Ring1.SelectionChanged += cb_Ring1_SelectionChanged;
			break;
		case 43:
			grd_Ring2 = (Grid)target;
			break;
		case 44:
			es_RingResult2 = (Ellipse)target;
			break;
		case 45:
			cb_Ring2 = (ComboBox)target;
			cb_Ring2.SelectionChanged += cb_Ring1_SelectionChanged;
			break;
		case 46:
			grd_Ring3 = (Grid)target;
			break;
		case 47:
			es_RingResult3 = (Ellipse)target;
			break;
		case 48:
			cb_Ring3 = (ComboBox)target;
			cb_Ring3.SelectionChanged += cb_Ring1_SelectionChanged;
			break;
		case 49:
			grd_Ring4 = (Grid)target;
			break;
		case 50:
			es_RingResult4 = (Ellipse)target;
			break;
		case 51:
			cb_Ring4 = (ComboBox)target;
			cb_Ring4.SelectionChanged += cb_Ring1_SelectionChanged;
			break;
		case 52:
			tab_Setup = (TabItem)target;
			break;
		case 53:
			scr_TrackerSwich = (ScrollBar)target;
			scr_TrackerSwich.SizeChanged += Scr_TrackerSwich_SizeChanged;
			scr_TrackerSwich.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 54:
			lab_Time = (Label)target;
			break;
		case 55:
			btn_ResetTracker = (Button)target;
			btn_ResetTracker.Click += btn_ResetTracker_Click;
			break;
		case 56:
			exp_THD1 = (Expander)target;
			break;
		case 57:
			exp_THD2 = (Expander)target;
			break;
		case 58:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 59:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 60:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 61:
			cb_THD2_DEStart = (ComboBox)target;
			cb_THD2_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 62:
			cb_THD2_DEEnd = (ComboBox)target;
			cb_THD2_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 63:
			cb_THD2_DEEndCount = (ComboBox)target;
			cb_THD2_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 64:
			cb_THD2_DEStartCount = (ComboBox)target;
			cb_THD2_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 65:
			cb_Channel_THD2 = (ComboBox)target;
			break;
		case 66:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 67:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 68:
			btn_ChannelBinaryPreview2 = (Button)target;
			btn_ChannelBinaryPreview2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview2.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 69:
			btn_ChannelPreview2 = (Button)target;
			btn_ChannelPreview2.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview2.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 70:
			btn_HistogramPreview2 = (Button)target;
			btn_HistogramPreview2.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview2.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 71:
			btn_THD2Undo = (Button)target;
			btn_THD2Undo.Click += Btn_THD1Undo_Click;
			break;
		case 72:
			btn_SoftCorrectionTHD2 = (Button)target;
			break;
		case 73:
			lab_SoftCorrectionShiftTHD2 = (Label)target;
			break;
		case 74:
			exp_THD3 = (Expander)target;
			break;
		case 75:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 76:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 77:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 78:
			cb_THD3_DEStart = (ComboBox)target;
			cb_THD3_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 79:
			cb_THD3_DEEnd = (ComboBox)target;
			cb_THD3_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 80:
			cb_THD3_DEEndCount = (ComboBox)target;
			cb_THD3_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 81:
			cb_THD3_DEStartCount = (ComboBox)target;
			cb_THD3_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 82:
			cb_Channel_THD3 = (ComboBox)target;
			break;
		case 83:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 84:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 85:
			btn_ChannelBinaryPreview3 = (Button)target;
			btn_ChannelBinaryPreview3.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview3.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 86:
			btn_ChannelPreview3 = (Button)target;
			btn_ChannelPreview3.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview3.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 87:
			btn_HistogramPreview3 = (Button)target;
			btn_HistogramPreview3.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview3.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 88:
			btn_THD3Undo = (Button)target;
			btn_THD3Undo.Click += Btn_THD1Undo_Click;
			break;
		case 89:
			btn_SoftCorrectionTHD3 = (Button)target;
			break;
		case 90:
			lab_SoftCorrectionShiftTHD3 = (Label)target;
			break;
		case 91:
			exp_THD4 = (Expander)target;
			break;
		case 92:
			grd_SIMP_THD_4 = (Grid)target;
			break;
		case 93:
			rs_THD4 = (RangeSlider)target;
			rs_THD4.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD4.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD4.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 94:
			cb_Channel_THD4 = (ComboBox)target;
			break;
		case 95:
			grd_SIMP_DilateErode4 = (Grid)target;
			break;
		case 96:
			cb_THD4_DEStart = (ComboBox)target;
			cb_THD4_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 97:
			cb_THD4_DEEnd = (ComboBox)target;
			cb_THD4_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 98:
			cb_THD4_DEEndCount = (ComboBox)target;
			cb_THD4_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 99:
			cb_THD4_DEStartCount = (ComboBox)target;
			cb_THD4_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 100:
			nu_Thd4LimitMin = (NumericUpDown)target;
			nu_Thd4LimitMin.ValueChanged += Nu_Thd4LimitMin_ValueChanged;
			break;
		case 101:
			nu_Thd4LimitMax = (NumericUpDown)target;
			nu_Thd4LimitMax.ValueChanged += Nu_Thd4LimitMax_ValueChanged;
			break;
		case 102:
			btn_ChannelBinaryPreview4 = (Button)target;
			btn_ChannelBinaryPreview4.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview4.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 103:
			btn_ChannelPreview4 = (Button)target;
			btn_ChannelPreview4.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview4.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 104:
			btn_HistogramPreview1_Copy = (Button)target;
			btn_HistogramPreview1_Copy.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview1_Copy.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 105:
			btn_THD4Undo = (Button)target;
			btn_THD4Undo.Click += Btn_THD1Undo_Click;
			break;
		case 106:
			btn_SoftCorrectionTHD4 = (Button)target;
			break;
		case 107:
			lab_SoftCorrectionShiftTHD4 = (Label)target;
			break;
		case 108:
			chk_Thd_ORMode = (CheckBox)target;
			chk_Thd_ORMode.Click += Chk_Thd_ORMode_Click;
			break;
		case 109:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 110:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 111:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 112:
			cb_THD1_DEStart = (ComboBox)target;
			cb_THD1_DEStart.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 113:
			cb_THD1_DEEnd = (ComboBox)target;
			cb_THD1_DEEnd.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 114:
			cb_THD1_DEEndCount = (ComboBox)target;
			cb_THD1_DEEndCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 115:
			cb_THD1_DEStartCount = (ComboBox)target;
			cb_THD1_DEStartCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 116:
			cb_Channel_THD1 = (ComboBox)target;
			break;
		case 117:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 118:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 119:
			btn_ChannelBinaryPreview1 = (Button)target;
			btn_ChannelBinaryPreview1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			btn_ChannelBinaryPreview1.MouseEnter += Btn_ChannelBinaryPreview1_MouseEnter;
			break;
		case 120:
			btn_ChannelPreview1 = (Button)target;
			btn_ChannelPreview1.MouseEnter += Btn_ChannelPreview1_MouseEnter;
			btn_ChannelPreview1.MouseLeave += Btn_ChannelPreview1_MouseLeave;
			break;
		case 121:
			btn_HistogramPreview1 = (Button)target;
			btn_HistogramPreview1.MouseLeave += Btn_HistogramPreview1_MouseLeave;
			btn_HistogramPreview1.MouseEnter += Btn_HistogramPreview1_MouseEnter;
			break;
		case 122:
			btn_THD1Undo = (Button)target;
			btn_THD1Undo.Click += Btn_THD1Undo_Click;
			break;
		case 123:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSU_Click;
			break;
		case 124:
			lab_SoftCorrectionShiftTHD1 = (Label)target;
			break;
		case 125:
			grd_SIMP_RefSetup = (Grid)target;
			break;
		case 126:
			cb_ColorReference = (ComboBox)target;
			cb_ColorReference.SelectionChanged += Cb_ColorReference_SelectionChanged;
			break;
		case 127:
			btn_ReferenceSave = (Button)target;
			btn_ReferenceSave.Click += Btn_ReferenceSave_Click;
			break;
		case 128:
			txt_ReferenceName = (TextBox)target;
			break;
		case 129:
			btn_ReferenceLock = (Button)target;
			btn_ReferenceLock.Click += Btn_ReferenceLock_Click;
			break;
		case 130:
			grd_SIMP_Setup = (Grid)target;
			break;
		case 131:
			tab_BlobAnalysis = (TabControl)target;
			break;
		case 132:
			tb_MaxBlob = (TabItem)target;
			break;
		case 133:
			txt_InspRule3Min = (TextBox)target;
			txt_InspRule3Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 134:
			txt_InspRule3Max = (TextBox)target;
			txt_InspRule3Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 135:
			txt_InspRule4Min = (TextBox)target;
			txt_InspRule4Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 136:
			txt_InspRule4Max = (TextBox)target;
			txt_InspRule4Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 137:
			txt_InspRule13Result = (TextBox)target;
			break;
		case 138:
			txt_InspRule14Result = (TextBox)target;
			break;
		case 139:
			txt_InspRule1Min = (TextBox)target;
			txt_InspRule1Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 140:
			txt_InspRule1Max = (TextBox)target;
			txt_InspRule1Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 141:
			txt_InspRule2Min = (TextBox)target;
			txt_InspRule2Min.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 142:
			txt_InspRule2Max = (TextBox)target;
			txt_InspRule2Max.PreviewMouseDoubleClick += Txt_ComDigits_PreviewMouseDoubleClick;
			break;
		case 143:
			txt_InspRule11Result = (TextBox)target;
			break;
		case 144:
			txt_InspRule12Result = (TextBox)target;
			break;
		case 145:
			cb_InspRule1 = (ComboBox)target;
			break;
		case 146:
			cb_InspRule2 = (ComboBox)target;
			break;
		case 147:
			cb_InspRule3 = (ComboBox)target;
			break;
		case 148:
			cb_InspRule4 = (ComboBox)target;
			break;
		case 149:
			rad_RuleAND = (RadioButton)target;
			break;
		case 150:
			rad_RuleOR = (RadioButton)target;
			break;
		case 151:
			btn_BinaryTest = (Button)target;
			btn_BinaryTest.Click += btn_BinaryTest_Click;
			break;
		case 152:
			rt_RingTestResult = (Rectangle)target;
			break;
		case 153:
			btn_RotateImage = (Button)target;
			btn_RotateImage.Click += btn_RotateImage_Click;
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
			btn_ImgAI = (Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 169:
			btn_ImgAIR = (Button)target;
			btn_ImgAIR.Click += Btn_ImgSrc_Click;
			break;
		case 170:
			btn_ImgAIG = (Button)target;
			btn_ImgAIG.Click += Btn_ImgSrc_Click;
			break;
		case 171:
			btn_ImgAIB = (Button)target;
			btn_ImgAIB.Click += Btn_ImgSrc_Click;
			break;
		case 172:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 173:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 174:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 175:
			lab_RValue = (Label)target;
			break;
		case 176:
			lab_GValue = (Label)target;
			break;
		case 177:
			lab_BValue = (Label)target;
			break;
		case 178:
			lab_HValue = (Label)target;
			break;
		case 179:
			lab_SValue = (Label)target;
			break;
		case 180:
			lab_VValue = (Label)target;
			break;
		case 181:
			lab_GrayValue = (Label)target;
			break;
		case 182:
			lab_XValue = (Label)target;
			break;
		case 183:
			lab_YValue = (Label)target;
			break;
		case 184:
			lab_C1Value = (Label)target;
			break;
		case 185:
			lab_C2Value = (Label)target;
			break;
		case 186:
			lab_C3Value = (Label)target;
			break;
		case 187:
			btn_ImgT1 = (Button)target;
			btn_ImgT1.Click += Btn_ImgT1_Click;
			break;
		case 188:
			btn_ImgT2 = (Button)target;
			btn_ImgT2.Click += Btn_ImgT1_Click;
			break;
		case 189:
			btn_ImgT3 = (Button)target;
			btn_ImgT3.Click += Btn_ImgT1_Click;
			break;
		case 190:
			lab_AIRValue = (Label)target;
			break;
		case 191:
			lab_AIGValue = (Label)target;
			break;
		case 192:
			lab_AIBValue = (Label)target;
			break;
		case 193:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 194:
			txt_AllResult = (TextBox)target;
			break;
		case 195:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 196:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 197:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 198:
			btn_TrackerShow = (Button)target;
			btn_TrackerShow.Click += Btn_TrackerShow_Click;
			break;
		case 199:
			btn_PreviewSwitch = (Button)target;
			btn_PreviewSwitch.Click += Btn_PreviewSwitch_Click;
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
			grd_Histogram = (Grid)target;
			break;
		case 223:
			cnv_Histogram = (Canvas)target;
			break;
		case 224:
			btn_Fillet = (Button)target;
			btn_Fillet.Click += Btn_Fillet_Click;
			break;
		case 225:
			exp_TrackerFilllet = (Expander)target;
			exp_TrackerFilllet.Collapsed += Exp_TrackerFilllet_Collapsed;
			break;
		case 226:
			((Grid)target).PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 227:
			cb_T1Filllet0 = (ComboBox)target;
			cb_T1Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 228:
			cb_T1Filllet1 = (ComboBox)target;
			cb_T1Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 229:
			cb_T1Filllet2 = (ComboBox)target;
			cb_T1Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 230:
			cb_T1Filllet3 = (ComboBox)target;
			cb_T1Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 231:
			grd_Tracker2Filllet = (Grid)target;
			grd_Tracker2Filllet.PreviewMouseDown += Grid_PreviewMouseDown;
			break;
		case 232:
			cb_T2Filllet0 = (ComboBox)target;
			cb_T2Filllet0.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 233:
			cb_T2Filllet1 = (ComboBox)target;
			cb_T2Filllet1.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 234:
			cb_T2Filllet2 = (ComboBox)target;
			cb_T2Filllet2.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 235:
			cb_T2Filllet3 = (ComboBox)target;
			cb_T2Filllet3.SelectionChanged += Cb_T1Filllet0_SelectionChanged;
			break;
		case 236:
			Label_Index = (Label)target;
			break;
		case 237:
			lab_LockMode = (TextBox)target;
			break;
		case 238:
			btn_Ring1 = (Button)target;
			btn_Ring1.Click += btn_Ring1_Click;
			break;
		case 239:
			btn_Ring2 = (Button)target;
			btn_Ring2.Click += btn_Ring1_Click;
			break;
		case 240:
			btn_Ring3 = (Button)target;
			btn_Ring3.Click += btn_Ring1_Click;
			break;
		case 241:
			btn_Ring4 = (Button)target;
			btn_Ring4.Click += btn_Ring1_Click;
			break;
		case 242:
			btn_Result_FlipResult = (Button)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
