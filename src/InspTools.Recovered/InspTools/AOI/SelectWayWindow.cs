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

public class SelectWayWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private const int MaxThdTool = 3;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private bool ShowMessage = true;

	private byte[] CurrentColor = new byte[10];

	private OpenCvSharp.Rect DefaultSearchRect1 = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect DefaultSearchRect2 = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private int LanguageType = 1;

	private int PatternChannelSelect;

	private int StationIndex;

	private int SelectWayFunction;

	private bool TrackerAdjShow;

	private Mat ShowBitmap = new Mat();

	private Mat[] ThdShow = new Mat[3];

	private Mat BinaryBlob = new Mat();

	private Mat ScanBinary = new Mat();

	private Mat[] PatternMat = new Mat[20];

	private Mat OrgMat = new Mat();

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private CVInspection inspection;

	private AdvLocateTool advLocateTool;

	private RangeSlider[] THD_Sliders;

	private NumericUpDown[] THD_TextLow;

	private NumericUpDown[] THD_TextUp;

	private ComboBox[] THD_Channel;

	private ComboBox[] THD_DEStart;

	private ComboBox[] THD_DEEnd;

	private ComboBox[] THD_DEStartCount;

	private ComboBox[] THD_DEEndCount;

	private Expander[] THD_Expender;

	private TextBox[] Result1Txt;

	private ComboBox[] InspRule;

	private TextBox[] PatternName;

	private Button[] ChannelBtn;

	private Label[] PatternInfo;

	private string[] ChannelStr;

	private Image[] PatternImage;

	private bool OTSU_White = true;

	private bool ReferenceLock;

	internal SelectWayWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Label Label_Result;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Border bor_PatternSet;

	internal TextBlock txt_Search2Label;

	internal Border bor_PatternSearch;

	internal TextBlock txt_Search1Label1;

	internal Button btn_Result_FlipResult;

	internal Border bor_SearchRect2;

	internal TextBlock txt_Search2Label1;

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

	internal Button btn_Result_Flip;

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

	internal Button btn_ColorDropper;

	internal TextBox txt_AllResult;

	internal Button btn_ReDraw;

	internal TabControl tab_Function;

	internal TabItem tab_Binary;

	internal ScrollBar scr_TrackerSwich_V;

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

	internal RadioButton rad_Follow;

	internal RadioButton rad_AutoSize;

	internal Grid grd_SIMP_RefSetup;

	internal ComboBox cb_ColorReference;

	internal Button btn_ReferenceSave;

	internal TextBox txt_ReferenceName;

	internal Button btn_ReferenceLock;

	internal Grid grd_Test;

	internal Grid grd_ScanV_Block;

	internal Button btn_Test_Binary;

	internal ComboBox cb_RotateMethod;

	internal Button btn_TrackerReset_Copy;

	internal Rectangle rt_Result;

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

	internal ComboBox cb_SuccessDoFlip;

	internal CheckBox chk_Select2WayMode;

	internal ComboBox cb_RotateMethodB;

	internal ComboBox cb_SuccessDoFlipB;

	internal Label lab_A;

	internal Label lab_B;

	internal Button btn_Result_Flip_Copy;

	internal TabItem tab_PatternMatch;

	internal GroupBox gb_BasicSetup;

	internal Grid grd_SIMP_THD_2;

	internal Grid grd_Found;

	internal TextBox txt_PatternName21;

	internal Image img_PatternFound;

	internal Label lab_PatternFoundInfo;

	internal Grid grd_Pattern;

	internal Image img_PatternSet;

	internal Label lab_PatternSetInfo;

	internal ComboBox cb_Pattern;

	internal Ellipse ep_Result;

	internal TextBox txt_MatchScoreMin;

	internal TextBox txt_MatchScoreMax;

	internal Rectangle rt_Result1;

	internal Button btn_Test;

	internal Label lab_MatchScore;

	internal Button btn_ReDraw_Copy;

	internal Button btn_ReDraw_Copy1;

	internal Button btn_ReDraw_Copy2;

	internal ComboBox cb_RotateMethod2;

	internal ComboBox cb_SuccessDoFlip2;

	internal RadioButton rad_AutoSize2;

	internal RadioButton rad_Follow2;

	internal Button btn_PatternSearchTrackerReset;

	internal TabItem tab_PatternDB;

	internal GroupBox gb_BasicSetup_Copy;

	internal Grid grd_SIMP_THD_3;

	internal Button btn_ChannelR;

	internal Button btn_ChannelG;

	internal Button btn_ChannelB;

	internal Button btn_ChannelH;

	internal Button btn_ChannelS;

	internal Button btn_ChannelV;

	internal Button btn_ChannelGray;

	internal Button btn_ChannelC1;

	internal Button btn_ChannelC2;

	internal Button btn_ChannelC3;

	internal Button btn_ChannelRemove;

	internal TextBox txt_PatternName1;

	internal Image img_PatternImg1;

	internal Button btn_SetPattern;

	internal Label lab_PatternInfo1;

	internal TextBox txt_PatternName6;

	internal Image img_PatternImg6;

	internal Label lab_PatternInfo6;

	internal TextBox txt_PatternName11;

	internal Image img_PatternImg11;

	internal Label lab_PatternInfo11;

	internal TextBox txt_PatternName16;

	internal Image img_PatternImg16;

	internal Label lab_PatternInfo16;

	internal TextBox txt_PatternName2;

	internal Image img_PatternImg2;

	internal Label lab_PatternInfo2;

	internal TextBox txt_PatternName7;

	internal Image img_PatternImg7;

	internal Label lab_PatternInfo7;

	internal TextBox txt_PatternName12;

	internal Image img_PatternImg12;

	internal Label lab_PatternInfo12;

	internal TextBox txt_PatternName17;

	internal Image img_PatternImg17;

	internal Label lab_PatternInfo17;

	internal TextBox txt_PatternName3;

	internal Image img_PatternImg3;

	internal Label lab_PatternInfo3;

	internal TextBox txt_PatternName8;

	internal Image img_PatternImg8;

	internal Label lab_PatternInfo8;

	internal TextBox txt_PatternName13;

	internal Image img_PatternImg13;

	internal Label lab_PatternInfo13;

	internal TextBox txt_PatternName18;

	internal Image img_PatternImg18;

	internal Label lab_PatternInfo18;

	internal TextBox txt_PatternName4;

	internal Image img_PatternImg4;

	internal Label lab_PatternInfo4;

	internal TextBox txt_PatternName9;

	internal Image img_PatternImg9;

	internal Label lab_PatternInfo9;

	internal TextBox txt_PatternName14;

	internal Image img_PatternImg14;

	internal Label lab_PatternInfo14;

	internal TextBox txt_PatternName19;

	internal Image img_PatternImg19;

	internal Label lab_PatternInfo19;

	internal TextBox txt_PatternName5;

	internal Image img_PatternImg5;

	internal Label lab_PatternInfo5;

	internal TextBox txt_PatternName10;

	internal Image img_PatternImg10;

	internal Label lab_PatternInfo10;

	internal TextBox txt_PatternName15;

	internal Image img_PatternImg15;

	internal Label lab_PatternInfo15;

	internal TextBox txt_PatternName20;

	internal Image img_PatternImg20;

	internal Label lab_PatternInfo20;

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

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public SelectWayWindow()
	{
		inited = false;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		THD_Sliders = new RangeSlider[3] { rs_THD1, rs_THD2, rs_THD3 };
		THD_TextLow = new NumericUpDown[3] { nu_Thd1LimitMin, nu_Thd2LimitMin, nu_Thd3LimitMin };
		THD_TextUp = new NumericUpDown[3] { nu_Thd1LimitMax, nu_Thd2LimitMax, nu_Thd3LimitMax };
		THD_Channel = new ComboBox[3] { cb_Channel_THD1, cb_Channel_THD2, cb_Channel_THD3 };
		THD_DEStart = new ComboBox[3] { cb_THD1_DEStart, cb_THD2_DEStart, cb_THD3_DEStart };
		THD_DEEnd = new ComboBox[3] { cb_THD1_DEEnd, cb_THD2_DEEnd, cb_THD3_DEEnd };
		THD_DEStartCount = new ComboBox[3] { cb_THD1_DEStartCount, cb_THD2_DEStartCount, cb_THD3_DEStartCount };
		THD_DEEndCount = new ComboBox[3] { cb_THD1_DEEndCount, cb_THD2_DEEndCount, cb_THD3_DEEndCount };
		THD_Expender = new Expander[3] { exp_THD1, exp_THD2, exp_THD3 };
		InspRule = new ComboBox[4] { cb_InspRule1, cb_InspRule2, cb_InspRule3, cb_InspRule4 };
		Result1Txt = new TextBox[4] { txt_InspRule11Result, txt_InspRule12Result, txt_InspRule13Result, txt_InspRule14Result };
		PatternName = new TextBox[20]
		{
			txt_PatternName1, txt_PatternName2, txt_PatternName3, txt_PatternName4, txt_PatternName5, txt_PatternName6, txt_PatternName7, txt_PatternName8, txt_PatternName9, txt_PatternName10,
			txt_PatternName11, txt_PatternName12, txt_PatternName13, txt_PatternName14, txt_PatternName15, txt_PatternName16, txt_PatternName17, txt_PatternName18, txt_PatternName19, txt_PatternName20
		};
		PatternImage = new Image[20]
		{
			img_PatternImg1, img_PatternImg2, img_PatternImg3, img_PatternImg4, img_PatternImg5, img_PatternImg6, img_PatternImg7, img_PatternImg8, img_PatternImg9, img_PatternImg10,
			img_PatternImg11, img_PatternImg12, img_PatternImg13, img_PatternImg14, img_PatternImg15, img_PatternImg16, img_PatternImg17, img_PatternImg18, img_PatternImg19, img_PatternImg20
		};
		PatternInfo = new Label[20]
		{
			lab_PatternInfo1, lab_PatternInfo2, lab_PatternInfo3, lab_PatternInfo4, lab_PatternInfo5, lab_PatternInfo6, lab_PatternInfo7, lab_PatternInfo8, lab_PatternInfo9, lab_PatternInfo10,
			lab_PatternInfo11, lab_PatternInfo12, lab_PatternInfo13, lab_PatternInfo14, lab_PatternInfo15, lab_PatternInfo16, lab_PatternInfo17, lab_PatternInfo18, lab_PatternInfo19, lab_PatternInfo20
		};
		ChannelBtn = new Button[11]
		{
			btn_ChannelR, btn_ChannelG, btn_ChannelB, btn_ChannelH, btn_ChannelS, btn_ChannelV, btn_ChannelGray, btn_ChannelC1, btn_ChannelC2, btn_ChannelC3,
			btn_ChannelRemove
		};
		ChannelStr = new string[10]
		{
			"R",
			"G",
			"B",
			"H",
			"S",
			"V",
			"Gray",
			"C1",
			"C2",
			"C3"
		};
		Label_Result.Visibility = Visibility.Hidden;
		btn_Result_FlipResult.Visibility = Visibility.Hidden;
		exp_TrackerAdj.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(CVInspection station, int stationIndex)
	{
		inspection = station;
		StationIndex = stationIndex;
		advLocateTool = new AdvLocateTool(inspection.paramFile);
		UpdateUI = false;
		cb_Pattern.Items.Clear();
		try
		{
			for (int i = 0; i < 20; i++)
			{
				if (inspection.paramFile.patternDB[i].PatternON)
				{
					cb_Pattern.Items.Add(i + 1 + " : " + inspection.paramFile.patternDB[i].PatternName);
					PatternInfo[i].Content = "";
					PatternName[i].Text = inspection.paramFile.patternDB[i].PatternName;
					if (!inspection.paramFile.patternDB[i].PatternImage.Empty())
					{
						PatternMat[i] = inspection.paramFile.patternDB[i].PatternImage;
						ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[i]);
						PatternImage[i].Source = source;
						PatternInfo[i].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[i].PatternChannel], PatternMat[i].Width, PatternMat[i].Height);
					}
				}
				else
				{
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.patternDB[i].PatternName;
					comboBoxItem.IsEnabled = false;
					cb_Pattern.Items.Add(comboBoxItem);
				}
			}
			cb_Pattern.SelectedIndex = -1;
			UpdateUI = true;
		}
		catch
		{
			string text = "MTW01-SelectWayToolWindow-SetInspection()";
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect)
	{
		ImageWidth = mat.Cols;
		ImageHeight = mat.Rows;
		ImageBand = mat.Channels();
		mat.CopyTo(OrgMat);
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
		bor_LocateRect.SetValue(Canvas.LeftProperty, (double)locateRect.X * xRatio);
		bor_LocateRect.SetValue(Canvas.TopProperty, (double)locateRect.Y * yRatio);
		bor_LocateRect.SetValue(FrameworkElement.WidthProperty, (double)locateRect.Width * xRatio);
		bor_LocateRect.SetValue(FrameworkElement.HeightProperty, (double)locateRect.Height * yRatio);
		CopyValueToUI();
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

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		WriteUIValueToParam();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImgSrc_Click(object sender, RoutedEventArgs e)
	{
		string uid = ((Button)sender).Uid;
		if (uid == null)
		{
			goto IL_01ba;
		}
		int length = uid.Length;
		Mat image;
		if (length != 1)
		{
			if (length != 2)
			{
				goto IL_01ba;
			}
			char c = uid[1];
			if (c != '0')
			{
				if (c != '1' || !(uid == "11"))
				{
					goto IL_01ba;
				}
				image = inspection.buffers.LocatedSrc;
			}
			else
			{
				if (!(uid == "10"))
				{
					goto IL_01ba;
				}
				image = inspection.buffers.GetLocatedCustomMat(2);
			}
		}
		else
		{
			switch (uid[0])
			{
			case '0':
				break;
			case '1':
				goto IL_00d1;
			case '2':
				goto IL_00e8;
			case '3':
				goto IL_00ff;
			case '4':
				goto IL_0116;
			case '5':
				goto IL_012d;
			case '6':
				goto IL_0144;
			case '7':
				goto IL_0158;
			case '8':
				goto IL_016b;
			case '9':
				goto IL_017f;
			default:
				goto IL_01ba;
			}
			image = inspection.buffers.LocatedSrcTmp;
		}
		goto IL_01cd;
		IL_017f:
		image = inspection.buffers.GetLocatedCustomMat(1);
		goto IL_01cd;
		IL_01ba:
		image = (image = inspection.buffers.LocatedSrcTmp);
		goto IL_01cd;
		IL_01cd:
		THD_Preview = false;
		ImageSource imageSource = inspection.buffers.Mat2ImageSource(image);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
		return;
		IL_00d1:
		image = inspection.buffers.GetLocatedBGR(0);
		goto IL_01cd;
		IL_00e8:
		image = inspection.buffers.GetLocatedBGR(1);
		goto IL_01cd;
		IL_00ff:
		image = inspection.buffers.GetLocatedBGR(2);
		goto IL_01cd;
		IL_0116:
		image = inspection.buffers.GetLocatedHSV(0);
		goto IL_01cd;
		IL_012d:
		image = inspection.buffers.GetLocatedHSV(1);
		goto IL_01cd;
		IL_0144:
		image = inspection.buffers.GetLocatedHSV(2);
		goto IL_01cd;
		IL_0158:
		image = inspection.buffers.LocateGray;
		goto IL_01cd;
		IL_016b:
		image = inspection.buffers.GetLocatedCustomMat(0);
		goto IL_01cd;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetTracker(OpenCvSharp.Rect rect1, OpenCvSharp.Rect rect2, bool UseRect2)
	{
		bor_SearchRect1.Visibility = Visibility.Visible;
		bor_PatternSearch.Visibility = Visibility.Hidden;
		bor_PatternSet.Visibility = Visibility.Hidden;
		OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect1 &= rect3;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
		bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
		bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
		bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		if (UseRect2)
		{
			bor_SearchRect2.Visibility = Visibility.Visible;
			rect3 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			rect2 &= rect3;
			if (rect2.Width <= 0 || rect2.Height <= 0)
			{
				rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
			}
			bor_SearchRect2.SetValue(Canvas.LeftProperty, (double)rect2.X * xRatio);
			bor_SearchRect2.SetValue(Canvas.TopProperty, (double)rect2.Y * yRatio);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, (double)rect2.Width * xRatio);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, (double)rect2.Height * yRatio);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetPatternSearchTracker(OpenCvSharp.Rect rect1)
	{
		bor_SearchRect1.Visibility = Visibility.Hidden;
		bor_SearchRect2.Visibility = Visibility.Hidden;
		bor_PatternSearch.Visibility = Visibility.Visible;
		bor_PatternSet.Visibility = Visibility.Hidden;
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect1 &= rect2;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		bor_PatternSearch.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
		bor_PatternSearch.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
		bor_PatternSearch.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
		bor_PatternSearch.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetPatternSetTracker(OpenCvSharp.Rect rect1)
	{
		bor_SearchRect1.Visibility = Visibility.Hidden;
		bor_SearchRect2.Visibility = Visibility.Hidden;
		bor_PatternSearch.Visibility = Visibility.Hidden;
		bor_PatternSet.Visibility = Visibility.Visible;
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect1 &= rect2;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		bor_PatternSet.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
		bor_PatternSet.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
		bor_PatternSet.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
		bor_PatternSet.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTracker(ref OpenCvSharp.Rect rect1, bool GetRect2)
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
			if (!GetRect2)
			{
				rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
				rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
				rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
				rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
				rect1 &= rect2;
				txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
			}
			else
			{
				rect1.X = (int)(Canvas.GetLeft(bor_SearchRect2) / xRatio);
				rect1.Y = (int)(Canvas.GetTop(bor_SearchRect2) / yRatio);
				rect1.Width = (int)(bor_SearchRect2.Width / xRatio);
				rect1.Height = (int)(bor_SearchRect2.Height / yRatio);
				rect1 &= rect2;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetPatternSearchTracker(ref OpenCvSharp.Rect rect1)
	{
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect1.X = (int)(Canvas.GetLeft(bor_PatternSearch) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_PatternSearch) / yRatio);
		rect1.Width = (int)(bor_PatternSearch.Width / xRatio);
		rect1.Height = (int)(bor_PatternSearch.Height / yRatio);
		rect1 &= rect2;
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetPatternSetTracker(ref OpenCvSharp.Rect rect1)
	{
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect1.X = (int)(Canvas.GetLeft(bor_PatternSet) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_PatternSet) / yRatio);
		rect1.Width = (int)(bor_PatternSet.Width / xRatio);
		rect1.Height = (int)(bor_PatternSet.Height / yRatio);
		rect1 &= rect2;
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
		return true;
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
			Mat locatedSrcTmp = inspection.buffers.LocatedSrcTmp;
			ImageSource imageSource = inspection.buffers.Mat2ImageSource(locatedSrcTmp);
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
			rt_Result.Fill = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DoThdImage()
	{
		if (!UpdateUI)
		{
			return;
		}
		try
		{
			int[] array = new int[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = inspection.paramFile.EnhanceParam[i].FunctionIndex;
			}
			inspection.buffers.SetLocatedBuffer(OrgMat, array, inspection.paramFile.AIEnhanceToolON);
			Label_Result.Visibility = Visibility.Hidden;
			if (OrgMat.Channels() == 3)
			{
				ShowBitmap = OrgMat.Clone();
			}
			else
			{
				Cv2.CvtColor(OrgMat, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			Mat[] array2 = new Mat[3];
			Mat[] array3 = new Mat[3];
			int[] array4 = new int[3];
			int[] array5 = new int[3];
			int[] array6 = new int[3];
			bool[] array7 = new bool[3];
			int[] array8 = new int[3];
			int[] array9 = new int[3];
			int[] array10 = new int[3];
			int[] array11 = new int[3];
			array7[0] = exp_THD1.IsExpanded;
			array7[1] = exp_THD2.IsExpanded;
			array7[2] = exp_THD3.IsExpanded;
			int num = 0;
			for (int j = 0; j < 3; j++)
			{
				array2[j] = new Mat();
				array3[j] = new Mat();
				array4[j] = (int)THD_Sliders[j].LowerValue;
				array5[j] = (int)THD_Sliders[j].UpperValue;
				array6[j] = THD_Channel[j].SelectedIndex;
				array8[j] = THD_DEStart[j].SelectedIndex;
				array9[j] = THD_DEEnd[j].SelectedIndex;
				array10[j] = THD_DEStartCount[j].SelectedIndex;
				array11[j] = THD_DEEndCount[j].SelectedIndex;
				THD_TextLow[j].Value = array4[j];
				THD_TextUp[j].Value = array5[j];
				if (array7[j])
				{
					array2[j] = inspection.buffers.GetChannelMat(array6[j]).Clone();
					Cv2.InRange(array2[j], array4[j], array5[j], array3[j]);
					if (array8[j] != 0)
					{
						DoMorph(ref array3[j], array8[j], array10[j]);
					}
					if (array9[j] != 0)
					{
						DoMorph(ref array3[j], array9[j], array11[j]);
					}
					num++;
				}
			}
			if (num == 0)
			{
				return;
			}
			Mat mat = inspection.buffers.LocateGray.Clone();
			mat.SetTo(255.0);
			for (int k = 0; k < 3; k++)
			{
				if (array7[k])
				{
					Cv2.BitwiseAnd(mat, array3[k], mat);
				}
			}
			ScanBinary = mat.Clone();
			Mat mat2 = mat.Clone();
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 100, 100);
			GetTracker(ref rect, GetRect2: false);
			BinaryBlob = new Mat(mat, rect).Clone();
			Mat mat3 = mat.Clone();
			mat3.SetTo(0.0);
			new Mat(mat3, rect).SetTo(255.0);
			Cv2.BitwiseAnd(mat, mat3, mat);
			if (chk_Select2WayMode.IsChecked.Value)
			{
				GetTracker(ref rect2, GetRect2: true);
				BinaryBlob = new Mat(mat2, rect2).Clone();
				mat3 = mat2.Clone();
				mat3.SetTo(0.0);
				new Mat(mat3, rect2).SetTo(255.0);
				Cv2.BitwiseAnd(mat2, mat3, mat2);
				Cv2.BitwiseOr(mat, mat2, mat);
			}
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
			CVBuffer.SaveLog("SWTW01-SelectWayWindow-CheckResult()", inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			_ = ShowMessage;
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

	private bool CheckTextBox()
	{
		bool result = true;
		if (SelectWayFunction == 0)
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
		}
		else
		{
			if (!IsNumeric(txt_MatchScoreMin.Text))
			{
				txt_MatchScoreMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MatchScoreMin.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MatchScoreMax.Text))
			{
				txt_MatchScoreMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MatchScoreMax.Background = Brushes.White;
			}
		}
		return result;
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
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(LanguageManager.Instance["ResetTrackerMsg"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			_ = (Button)sender;
			bor_SearchRect1.SetValue(Canvas.LeftProperty, 0.0);
			bor_SearchRect1.SetValue(Canvas.TopProperty, 0.0);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, 100.0);
			bor_SearchRect2.SetValue(Canvas.LeftProperty, 50.0);
			bor_SearchRect2.SetValue(Canvas.TopProperty, 50.0);
			bor_SearchRect2.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect2.SetValue(FrameworkElement.HeightProperty, 100.0);
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

	private void Btn_ColorDropper_Click(object sender, RoutedEventArgs e)
	{
		cnv_Image.Cursor = Cursors.Cross;
		ColorDropper = true;
	}

	private void Btn_ReDraw_Click(object sender, RoutedEventArgs e)
	{
		DoThdImage();
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect, GetRect2: false);
		if (chk_Select2WayMode.IsChecked == true)
		{
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetTracker(ref rect2, GetRect2: true);
		}
	}

	private void DrawBackGround()
	{
		ShowBitmap = inspection.buffers.LocatedSrcTmp.Clone();
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	private void DrawBackGround(int channel)
	{
		ShowBitmap = inspection.buffers.GetChannelMat(channel);
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		try
		{
			UpdateUI = false;
			INSP_SELECTWAYTOOL_PARAM iNSP_SELECTWAYTOOL_PARAM = default(INSP_SELECTWAYTOOL_PARAM);
			iNSP_SELECTWAYTOOL_PARAM = inspection.paramFile.SelectWayParam;
			tab_Function.SelectedIndex = (SelectWayFunction = iNSP_SELECTWAYTOOL_PARAM.SelelctWayFunction);
			tab_Binary.Background = Brushes.White;
			tab_PatternMatch.Background = Brushes.White;
			tab_PatternDB.Background = Brushes.White;
			if (SelectWayFunction == 0)
			{
				tab_Binary.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCFD6FF"));
			}
			else
			{
				tab_PatternMatch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDAE4E"));
			}
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
			rect.X = LocateRect.X + iNSP_SELECTWAYTOOL_PARAM.SearchRectL;
			rect.Y = LocateRect.Y + iNSP_SELECTWAYTOOL_PARAM.SearchRectT;
			rect2.X = LocateRect.X + iNSP_SELECTWAYTOOL_PARAM.SearchRect2L;
			rect2.Y = LocateRect.Y + iNSP_SELECTWAYTOOL_PARAM.SearchRect2T;
			if (iNSP_SELECTWAYTOOL_PARAM.UseAutoSize)
			{
				rect.Width = LocateRect.Width - (iNSP_SELECTWAYTOOL_PARAM.SearchRectL + iNSP_SELECTWAYTOOL_PARAM.SearchRectR);
				rect.Height = LocateRect.Height - (iNSP_SELECTWAYTOOL_PARAM.SearchRectT + iNSP_SELECTWAYTOOL_PARAM.SearchRectB);
				rect2.Width = LocateRect.Width - (iNSP_SELECTWAYTOOL_PARAM.SearchRect2L + iNSP_SELECTWAYTOOL_PARAM.SearchRect2R);
				rect2.Height = LocateRect.Height - (iNSP_SELECTWAYTOOL_PARAM.SearchRect2T + iNSP_SELECTWAYTOOL_PARAM.SearchRect2B);
			}
			else
			{
				rect.Width = iNSP_SELECTWAYTOOL_PARAM.SearchRectWidth;
				rect.Height = iNSP_SELECTWAYTOOL_PARAM.SearchRectHeight;
				rect2.Width = iNSP_SELECTWAYTOOL_PARAM.SearchRect2Width;
				rect2.Height = iNSP_SELECTWAYTOOL_PARAM.SearchRect2Height;
			}
			if (SelectWayFunction == 0)
			{
				cb_RotateMethod.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.RotateMethod;
				cb_SuccessDoFlip.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.SuccessDoFlip;
				cb_RotateMethodB.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.RotateMethodB;
				cb_SuccessDoFlipB.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.SuccessDoFlipB;
				if (iNSP_SELECTWAYTOOL_PARAM.UseAutoSize)
				{
					rad_AutoSize.IsChecked = true;
				}
				else
				{
					rad_Follow.IsChecked = true;
				}
				chk_Select2WayMode.IsChecked = iNSP_SELECTWAYTOOL_PARAM.Use2WayMode;
				SetTracker(rect, rect2, iNSP_SELECTWAYTOOL_PARAM.Use2WayMode);
				GetTracker(ref DefaultSearchRect1, GetRect2: false);
				if (iNSP_SELECTWAYTOOL_PARAM.Use2WayMode)
				{
					GetTracker(ref DefaultSearchRect2, GetRect2: true);
					bor_SearchRect2.Visibility = Visibility.Visible;
					cb_RotateMethodB.Visibility = Visibility.Visible;
					cb_SuccessDoFlipB.Visibility = Visibility.Visible;
					lab_A.Visibility = Visibility.Visible;
					lab_B.Visibility = Visibility.Visible;
				}
				else
				{
					bor_SearchRect2.Visibility = Visibility.Hidden;
					cb_RotateMethodB.Visibility = Visibility.Hidden;
					cb_SuccessDoFlipB.Visibility = Visibility.Hidden;
					lab_A.Visibility = Visibility.Hidden;
					lab_B.Visibility = Visibility.Hidden;
				}
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
				cb_ColorReference.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.ColorReferenceIndex;
				if (iNSP_SELECTWAYTOOL_PARAM.ColorReferenceIndex <= 0)
				{
					cb_ColorReference.SelectedIndex = -1;
					ReferenceLock = false;
					grd_SIMP_RefSetup.Background = Brushes.White;
					btn_ReferenceSave.IsEnabled = false;
					txt_ReferenceName.IsEnabled = false;
					txt_ReferenceName.Text = "";
					for (int j = 0; j < 3; j++)
					{
						THD_Sliders[j].IsEnabled = true;
						THD_Expender[j].IsEnabled = true;
					}
					cb_Channel_THD1.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.THD1_ChannelIndex;
					cb_Channel_THD2.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.THD2_ChannelIndex;
					cb_Channel_THD3.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.THD3_ChannelIndex;
					exp_THD1.IsExpanded = iNSP_SELECTWAYTOOL_PARAM.THD1_ChannelON;
					exp_THD2.IsExpanded = iNSP_SELECTWAYTOOL_PARAM.THD2_ChannelON;
					exp_THD3.IsExpanded = iNSP_SELECTWAYTOOL_PARAM.THD3_ChannelON;
					rs_THD1.LowerValue = iNSP_SELECTWAYTOOL_PARAM.THD1_Min;
					rs_THD1.UpperValue = iNSP_SELECTWAYTOOL_PARAM.THD1_Max;
					nu_Thd1LimitMin.Value = iNSP_SELECTWAYTOOL_PARAM.THD1_Min;
					nu_Thd1LimitMax.Value = iNSP_SELECTWAYTOOL_PARAM.THD1_Max;
					rs_THD2.LowerValue = iNSP_SELECTWAYTOOL_PARAM.THD2_Min;
					rs_THD2.UpperValue = iNSP_SELECTWAYTOOL_PARAM.THD2_Max;
					nu_Thd2LimitMin.Value = iNSP_SELECTWAYTOOL_PARAM.THD2_Min;
					nu_Thd2LimitMax.Value = iNSP_SELECTWAYTOOL_PARAM.THD2_Max;
					rs_THD3.LowerValue = iNSP_SELECTWAYTOOL_PARAM.THD3_Min;
					rs_THD3.UpperValue = iNSP_SELECTWAYTOOL_PARAM.THD3_Max;
					nu_Thd3LimitMin.Value = iNSP_SELECTWAYTOOL_PARAM.THD3_Min;
					nu_Thd3LimitMax.Value = iNSP_SELECTWAYTOOL_PARAM.THD3_Max;
					cb_THD1_DEStart.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.DE_Start1_Index;
					cb_THD1_DEEnd.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.DE_End1_Index;
					cb_THD2_DEStart.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.DE_Start2_Index;
					cb_THD2_DEEnd.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.DE_End2_Index;
					cb_THD3_DEStart.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.DE_Start3_Index;
					cb_THD3_DEEnd.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.DE_End3_Index;
					cb_THD1_DEStartCount.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.DE_Start1_Count;
					cb_THD1_DEEndCount.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.DE_End1_Count;
					cb_THD2_DEStartCount.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.DE_Start2_Count;
					cb_THD2_DEEndCount.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.DE_End2_Count;
					cb_THD3_DEStartCount.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.DE_Start3_Count;
					cb_THD3_DEEndCount.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.DE_End3_Count;
				}
				else
				{
					cb_ColorReference.SelectedIndex = -1;
					ReferenceLock = true;
					btn_ReferenceSave.IsEnabled = false;
					txt_ReferenceName.IsEnabled = false;
					grd_SIMP_RefSetup.Background = Brushes.LightGoldenrodYellow;
					for (int k = 0; k < 3; k++)
					{
						THD_Sliders[k].IsEnabled = false;
						THD_Expender[k].IsEnabled = false;
					}
					grd_SIMP_THD_1.IsEnabled = false;
					INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = inspection.paramFile.ColorReferenceParam[iNSP_SELECTWAYTOOL_PARAM.ColorReferenceIndex - 1];
					txt_ReferenceName.Text = iNSP_COLORREFERENCE_PARAM.TrackerName;
					cb_Channel_THD1.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD1_ChannelIndex;
					cb_Channel_THD2.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD2_ChannelIndex;
					cb_Channel_THD3.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.THD3_ChannelIndex;
					exp_THD1.IsExpanded = true;
					exp_THD2.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD2_ChannelON;
					exp_THD3.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD3_ChannelON;
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
					cb_THD1_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start1_Index;
					cb_THD1_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End1_Index;
					cb_THD2_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start2_Index;
					cb_THD2_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End2_Index;
					cb_THD3_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start3_Index;
					cb_THD3_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End3_Index;
					cb_THD1_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start1_Count;
					cb_THD1_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End1_Count;
					cb_THD2_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start2_Count;
					cb_THD2_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End2_Count;
					cb_THD3_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start3_Count;
					cb_THD3_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End3_Count;
				}
				cb_InspRule1.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.InspRule1_Index;
				cb_InspRule2.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.InspRule2_Index;
				cb_InspRule3.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.InspRule3_Index;
				cb_InspRule4.SelectedIndex = (int)iNSP_SELECTWAYTOOL_PARAM.InspRule4_Index;
				if (iNSP_SELECTWAYTOOL_PARAM.InspRuleAND)
				{
					rad_RuleAND.IsChecked = true;
				}
				else
				{
					rad_RuleOR.IsChecked = true;
				}
				txt_InspRule1Min.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule1_Min.ToString();
				txt_InspRule2Min.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule2_Min.ToString();
				txt_InspRule3Min.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule3_Min.ToString();
				txt_InspRule4Min.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule4_Min.ToString();
				txt_InspRule1Max.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule1_Max.ToString();
				txt_InspRule2Max.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule2_Max.ToString();
				txt_InspRule3Max.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule3_Max.ToString();
				txt_InspRule4Max.Text = iNSP_SELECTWAYTOOL_PARAM.InspRule4_Max.ToString();
			}
			else
			{
				cb_RotateMethod2.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.RotateMethod;
				cb_SuccessDoFlip2.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.SuccessDoFlip;
				if (iNSP_SELECTWAYTOOL_PARAM.UseAutoSize)
				{
					rad_AutoSize2.IsChecked = true;
				}
				else
				{
					rad_Follow2.IsChecked = true;
				}
				SetPatternSearchTracker(rect);
				GetPatternSearchTracker(ref DefaultSearchRect1);
				txt_MatchScoreMin.Text = iNSP_SELECTWAYTOOL_PARAM.MatchScoreMin.ToString();
				txt_MatchScoreMax.Text = iNSP_SELECTWAYTOOL_PARAM.MatchScoreMax.ToString();
				if (iNSP_SELECTWAYTOOL_PARAM.PatternSelect < 0)
				{
					cb_Pattern.SelectedIndex = -1;
				}
				else if (inspection.paramFile.patternDB[iNSP_SELECTWAYTOOL_PARAM.PatternSelect].PatternON && inspection.paramFile.patternDB[iNSP_SELECTWAYTOOL_PARAM.PatternSelect].PatternPath != null && !inspection.paramFile.patternDB[iNSP_SELECTWAYTOOL_PARAM.PatternSelect].PatternImage.Empty())
				{
					cb_Pattern.SelectedIndex = iNSP_SELECTWAYTOOL_PARAM.PatternSelect;
					ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_SELECTWAYTOOL_PARAM.PatternSelect]);
					img_PatternSet.Source = source;
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_SELECTWAYTOOL_PARAM.PatternSelect].PatternChannel], PatternMat[iNSP_SELECTWAYTOOL_PARAM.PatternSelect].Width, PatternMat[iNSP_SELECTWAYTOOL_PARAM.PatternSelect].Height);
				}
				else
				{
					cb_Pattern.SelectedIndex = -1;
				}
				if (cb_Pattern.SelectedIndex == -1)
				{
					img_PatternSet.Source = null;
					lab_PatternSetInfo.Content = "";
				}
				img_PatternFound.Source = null;
				lab_PatternFoundInfo.Content = "";
				ep_Result.Fill = Brushes.Gray;
				lab_MatchScore.Content = "0";
			}
			LastSearchRect1 = rect;
			UpdateUI = true;
		}
		catch
		{
			string text = "SWTW02-SelectWayWindow-CopyValueToUI()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckResult()
	{
		try
		{
			OpenCvSharp.Rect rect = default(OpenCvSharp.Rect);
			rect = inspection.selectwayTool.Blobs.NGRect;
			OpenCvSharp.Rect rect2 = default(OpenCvSharp.Rect);
			rect2 = inspection.selectwayTool.Blobs2.NGRect;
			double num = 0.0;
			num = inspection.selectwayTool.Blobs.Area;
			bool[] array = new bool[4];
			bool flag = true;
			TextBox[] result1Txt = Result1Txt;
			array[0] = GetRule(cb_InspRule1.SelectedIndex, txt_InspRule1Min, txt_InspRule1Max, result1Txt[0], rect, num);
			array[1] = GetRule(cb_InspRule2.SelectedIndex, txt_InspRule2Min, txt_InspRule2Max, result1Txt[1], rect, num);
			if (chk_Select2WayMode.IsChecked == false)
			{
				array[2] = GetRule(cb_InspRule3.SelectedIndex, txt_InspRule3Min, txt_InspRule3Max, result1Txt[2], rect, num);
				array[3] = GetRule(cb_InspRule4.SelectedIndex, txt_InspRule4Min, txt_InspRule4Max, result1Txt[3], rect, num);
			}
			else
			{
				array[2] = GetRule(cb_InspRule3.SelectedIndex, txt_InspRule3Min, txt_InspRule3Max, result1Txt[2], rect2, inspection.selectwayTool.Blobs2.Area);
				array[3] = GetRule(cb_InspRule4.SelectedIndex, txt_InspRule4Min, txt_InspRule4Max, result1Txt[3], rect2, inspection.selectwayTool.Blobs2.Area);
			}
			if (rad_RuleAND.IsChecked == true)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				for (int i = 0; i < 4; i++)
				{
					if (array[i] && InspRule[i].SelectedIndex > 0)
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
			SetResult(result: false);
			return false;
		}
		catch (Exception ex)
		{
			string text = "BW04-BinaryWindow-CheckResult()-";
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
				double num2 = Convert.ToDouble(area) * inspection.paramFile.PixelPerUM_X * inspection.paramFile.PixelPerUM_Y;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", num2);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)num2);
				}
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
			case 4:
			{
				double num = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				resultbox.Text = string.Format("{0:N3}", num);
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
				break;
			}
			case 5:
			{
				double num4 = ((rect.Width * rect.Height > 0) ? (area / (double)(rect.Width * rect.Height)) : 0.0);
				resultbox.Text = string.Format("{0:N3}", num4);
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
			case 6:
			{
				double allArea = inspection.selectwayTool.AllArea;
				if (inspection.paramFile.UseFloat)
				{
					resultbox.Text = string.Format("{0:N3}", allArea);
				}
				else
				{
					resultbox.Text = string.Format("{0}", (int)allArea);
				}
				if (allArea < Convert.ToDouble(min.Text) || allArea > Convert.ToDouble(max.Text))
				{
					result = false;
					if (allArea < Convert.ToDouble(min.Text))
					{
						min.Background = Brushes.Fuchsia;
					}
					if (allArea > Convert.ToDouble(max.Text))
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

	private void Btn_ChannelR_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		for (int i = 0; i < 11; i++)
		{
			ChannelBtn[i].Background = Brushes.White;
		}
		PatternChannelSelect = Convert.ToInt32(button.Uid);
		ChannelBtn[PatternChannelSelect].Background = Brushes.YellowGreen;
		if (PatternChannelSelect == 10)
		{
			cnv_Image.Background = Brushes.Gray;
		}
		else
		{
			DrawBackGround(PatternChannelSelect);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetPattern_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		if (PatternChannelSelect == 10)
		{
			PatternMat[num] = new Mat();
			PatternImage[num].Source = null;
			PatternInfo[num].Content = "";
			inspection.paramFile.patternDB[num].PatternON = false;
			inspection.paramFile.patternDB[num].ValueChangeFlag = true;
		}
		else if (GetPatternSetTracker(ref rect))
		{
			PatternMat[num] = new Mat(inspection.buffers.GetChannelMat(PatternChannelSelect), rect).Clone();
			WritePattern(num);
			ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num]);
			PatternImage[num].Source = source;
			PatternInfo[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num].PatternChannel], PatternMat[num].Width, PatternMat[num].Height);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WritePattern(int index)
	{
		try
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + inspection.RecipeName + "\\Pattern\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}S{1}P{2}.bmp", text, StationIndex + 1, index + 1);
			MatchPattern matchPattern = inspection.paramFile.patternDB[index];
			matchPattern.ValueChangeFlag = true;
			if (!PatternMat[index].Empty())
			{
				Cv2.ImWrite(text2, PatternMat[index]);
				matchPattern.PatternON = true;
				matchPattern.PatternName = PatternName[index].Text;
				matchPattern.PatternChannel = PatternChannelSelect;
				matchPattern.PatternPath = text2;
				matchPattern.PatternImage = PatternMat[index];
			}
		}
		catch
		{
			string text3 = "MTW07-SelectWayWindow-WritePattern()";
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_Function_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !inited || !(e.Source is TabControl))
		{
			return;
		}
		tab_Binary.Background = Brushes.White;
		tab_PatternMatch.Background = Brushes.White;
		tab_PatternDB.Background = Brushes.White;
		if (tab_Function.SelectedIndex == 0)
		{
			tab_Binary.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCFD6FF"));
			WriteUIValueToParam();
			inspection.paramFile.SelectWayParam.SelelctWayFunction = (SelectWayFunction = 0);
			if (LanguageType == 0)
			{
				MessageBox.Show("SelectWay Functio will change to Binary！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("選向方式將更換為二值化選向！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("选向方式将更换为二值化选向！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			bor_SearchRect1.Visibility = Visibility.Visible;
			if (chk_Select2WayMode.IsChecked == true)
			{
				bor_SearchRect2.Visibility = Visibility.Visible;
			}
			bor_PatternSearch.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Hidden;
			DrawBackGround();
			CopyValueToUI();
		}
		else if (tab_Function.SelectedIndex == 1)
		{
			tab_PatternMatch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDAE4E"));
			WriteUIValueToParam();
			int selectWayFunction = SelectWayFunction;
			inspection.paramFile.SelectWayParam.SelelctWayFunction = (SelectWayFunction = 1);
			if (selectWayFunction == 0)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("SelectWay Functio will change to Match Tool！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("選向方式將更換為圖形選向！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
				}
				else
				{
					MessageBox.Show("选向方式将更换为图形选向！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
				}
			}
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_SearchRect2.Visibility = Visibility.Hidden;
			bor_PatternSearch.Visibility = Visibility.Visible;
			bor_PatternSet.Visibility = Visibility.Hidden;
			UpdatePatternComboBox();
			DrawBackGround();
			CopyValueToUI();
		}
		else
		{
			if (tab_Function.SelectedIndex != 2)
			{
				return;
			}
			tab_PatternDB.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDAE4E"));
			if (SelectWayFunction != 1)
			{
				inspection.paramFile.SelectWayParam.SelelctWayFunction = (SelectWayFunction = 1);
				if (LanguageType == 0)
				{
					MessageBox.Show("SelectWay Functio will change to Match Tool！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("選向方式將更換為圖形選向！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
				}
				else
				{
					MessageBox.Show("选向方式将更换为图形选向！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
				}
			}
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_PatternSearch.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Visible;
			int[] array = new int[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = inspection.paramFile.EnhanceParam[i].FunctionIndex;
			}
			inspection.buffers.SetLocatedBuffer(OrgMat, array, inspection.paramFile.AIEnhanceToolON);
			DrawBackGround(PatternChannelSelect);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdatePatternComboBox()
	{
		cb_Pattern.Items.Clear();
		for (int i = 0; i < 20; i++)
		{
			if (inspection.paramFile.patternDB[i].PatternON)
			{
				cb_Pattern.Items.Add(i + 1 + " : " + inspection.paramFile.patternDB[i].PatternName);
				continue;
			}
			ComboBoxItem comboBoxItem = new ComboBoxItem();
			comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.patternDB[i].PatternName;
			comboBoxItem.IsEnabled = false;
			cb_Pattern.Items.Add(comboBoxItem);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_PatternSearchTrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Tracker value will set to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			bor_PatternSearch.Visibility = Visibility.Visible;
			bor_PatternSearch.SetValue(Canvas.LeftProperty, 0.0);
			bor_PatternSearch.SetValue(Canvas.TopProperty, 0.0);
			bor_PatternSearch.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_PatternSearch.SetValue(FrameworkElement.HeightProperty, 100.0);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_Pattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && inited)
		{
			int selectedIndex = cb_Pattern.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < 30 && inspection.paramFile.patternDB[selectedIndex].PatternON && !PatternMat[selectedIndex].Empty())
			{
				ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[selectedIndex]);
				img_PatternSet.Source = source;
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[selectedIndex].PatternChannel], PatternMat[selectedIndex].Width, PatternMat[selectedIndex].Height);
			}
		}
	}

	private void Cnv_Image_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
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

	private void Txt_InspRule1Min_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetKeyboard(tbox, position);
	}

	private void Txt_MatchScoreMin_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
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
		if (!ColorDropper)
		{
			return;
		}
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

	private void Chk_Select2WayMode_Click(object sender, RoutedEventArgs e)
	{
		if (chk_Select2WayMode.IsChecked == true)
		{
			GetTracker(ref DefaultSearchRect2, GetRect2: true);
			bor_SearchRect2.Visibility = Visibility.Visible;
			cb_RotateMethodB.Visibility = Visibility.Visible;
			cb_SuccessDoFlipB.Visibility = Visibility.Visible;
			lab_A.Visibility = Visibility.Visible;
			lab_B.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect2.Visibility = Visibility.Hidden;
			cb_RotateMethodB.Visibility = Visibility.Hidden;
			cb_SuccessDoFlipB.Visibility = Visibility.Hidden;
			lab_A.Visibility = Visibility.Hidden;
			lab_B.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OTSU_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = cb_Channel_THD1.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect, GetRect2: false);
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
			string text = "BW10-SelectWayWindow-Btn_OTSUL_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
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
			for (int i = 0; i < 3; i++)
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
			for (int j = 0; j < 3; j++)
			{
				THD_Sliders[j].IsEnabled = false;
				THD_Expender[j].IsEnabled = false;
			}
			grd_SIMP_THD_1.IsEnabled = false;
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
			THD1_Min = (int)rs_THD1.LowerValue,
			THD1_Max = (int)rs_THD1.UpperValue,
			THD2_Min = (int)rs_THD2.LowerValue,
			THD2_Max = (int)rs_THD2.UpperValue,
			THD3_Min = (int)rs_THD3.LowerValue,
			THD3_Max = (int)rs_THD3.UpperValue,
			DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex,
			DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex,
			DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex,
			DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex,
			DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex,
			DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex,
			DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex,
			DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex,
			DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex,
			DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex,
			DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex,
			DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex
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
		for (int j = 0; j < 3; j++)
		{
			THD_Sliders[j].IsEnabled = false;
			THD_Expender[j].IsEnabled = false;
		}
		grd_SIMP_THD_1.IsEnabled = false;
		WriteUIValueToParam();
		UpdateUI = true;
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
			for (int i = 0; i < 3; i++)
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
			exp_THD1.IsExpanded = true;
			exp_THD2.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD2_ChannelON;
			exp_THD3.IsExpanded = iNSP_COLORREFERENCE_PARAM.THD3_ChannelON;
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
			cb_THD1_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)iNSP_COLORREFERENCE_PARAM.DE_End3_Index;
			cb_THD1_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = iNSP_COLORREFERENCE_PARAM.DE_End3_Count;
			UpdateUI = true;
		}
		else
		{
			grd_SIMP_RefSetup.Background = Brushes.White;
			txt_ReferenceName.Text = "";
			UpdateUI = false;
			for (int j = 0; j < 3; j++)
			{
				THD_Sliders[j].IsEnabled = true;
				THD_Expender[j].IsEnabled = true;
			}
			grd_SIMP_THD_1.IsEnabled = true;
			INSP_SELECTWAYTOOL_PARAM selectWayParam = inspection.paramFile.SelectWayParam;
			cb_Channel_THD1.SelectedIndex = (int)selectWayParam.THD1_ChannelIndex;
			cb_Channel_THD2.SelectedIndex = (int)selectWayParam.THD2_ChannelIndex;
			cb_Channel_THD3.SelectedIndex = (int)selectWayParam.THD3_ChannelIndex;
			THD_Expender[0].IsExpanded = selectWayParam.THD1_ChannelON;
			THD_Expender[1].IsExpanded = selectWayParam.THD2_ChannelON;
			THD_Expender[2].IsExpanded = selectWayParam.THD3_ChannelON;
			rs_THD1.LowerValue = selectWayParam.THD1_Min;
			rs_THD1.UpperValue = selectWayParam.THD1_Max;
			nu_Thd1LimitMin.Value = selectWayParam.THD1_Min;
			nu_Thd1LimitMax.Value = selectWayParam.THD1_Max;
			rs_THD2.LowerValue = selectWayParam.THD2_Min;
			rs_THD2.UpperValue = selectWayParam.THD2_Max;
			nu_Thd2LimitMin.Value = selectWayParam.THD2_Min;
			nu_Thd2LimitMax.Value = selectWayParam.THD2_Max;
			rs_THD3.LowerValue = selectWayParam.THD3_Min;
			rs_THD3.UpperValue = selectWayParam.THD3_Max;
			nu_Thd3LimitMin.Value = selectWayParam.THD3_Min;
			nu_Thd3LimitMax.Value = selectWayParam.THD3_Max;
			cb_THD1_DEStart.SelectedIndex = (int)selectWayParam.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)selectWayParam.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)selectWayParam.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)selectWayParam.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)selectWayParam.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)selectWayParam.DE_End3_Index;
			cb_THD1_DEStartCount.SelectedIndex = selectWayParam.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = selectWayParam.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = selectWayParam.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = selectWayParam.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = selectWayParam.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = selectWayParam.DE_End3_Count;
			UpdateUI = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		try
		{
			UpdateUI = false;
			INSP_SELECTWAYTOOL_PARAM selectWayParam = inspection.paramFile.SelectWayParam;
			selectWayParam.ValueChangeFlag = true;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (SelectWayFunction == 0)
			{
				selectWayParam.SelelctWayFunction = 0;
				GetTracker(ref rect, GetRect2: false);
				selectWayParam.Use2WayMode = chk_Select2WayMode.IsChecked.Value;
				if (selectWayParam.Use2WayMode)
				{
					GetTracker(ref rect2, GetRect2: true);
					if (DefaultSearchRect2 != rect2)
					{
						selectWayParam.SearchRect2L = rect2.X - LocateRect.X;
						selectWayParam.SearchRect2T = rect2.Y - LocateRect.Y;
						selectWayParam.SearchRect2R = LocateRect.Width - selectWayParam.SearchRect2L - rect2.Width;
						selectWayParam.SearchRect2B = LocateRect.Height - selectWayParam.SearchRect2T - rect2.Height;
						selectWayParam.SearchRect2Width = rect2.Width;
						selectWayParam.SearchRect2Height = rect2.Height;
					}
					selectWayParam.RotateMethodB = cb_RotateMethodB.SelectedIndex;
					selectWayParam.SuccessDoFlipB = cb_SuccessDoFlipB.SelectedIndex;
				}
				if (selectWayParam.ColorReferenceIndex <= 0)
				{
					selectWayParam.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
					selectWayParam.THD1_ChannelON = exp_THD1.IsExpanded;
					selectWayParam.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
					selectWayParam.THD2_ChannelON = exp_THD2.IsExpanded;
					selectWayParam.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
					selectWayParam.THD3_ChannelON = exp_THD3.IsExpanded;
					selectWayParam.THD1_Min = (int)rs_THD1.LowerValue;
					selectWayParam.THD1_Max = (int)rs_THD1.UpperValue;
					selectWayParam.THD2_Min = (int)rs_THD2.LowerValue;
					selectWayParam.THD2_Max = (int)rs_THD2.UpperValue;
					selectWayParam.THD3_Min = (int)rs_THD3.LowerValue;
					selectWayParam.THD3_Max = (int)rs_THD3.UpperValue;
					selectWayParam.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
					selectWayParam.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
					selectWayParam.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
					selectWayParam.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
					selectWayParam.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
					selectWayParam.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
					selectWayParam.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
					selectWayParam.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
					selectWayParam.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
					selectWayParam.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
					selectWayParam.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
					selectWayParam.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
				}
				selectWayParam.RotateMethod = cb_RotateMethod.SelectedIndex;
				selectWayParam.InspRuleAND = rad_RuleAND.IsChecked.Value;
				selectWayParam.InspRule1_Index = (BlobCondition)cb_InspRule1.SelectedIndex;
				selectWayParam.InspRule2_Index = (BlobCondition)cb_InspRule2.SelectedIndex;
				selectWayParam.InspRule3_Index = (BlobCondition)cb_InspRule3.SelectedIndex;
				selectWayParam.InspRule4_Index = (BlobCondition)cb_InspRule4.SelectedIndex;
				selectWayParam.InspRule1_Min = Convert.ToDouble(txt_InspRule1Min.Text);
				selectWayParam.InspRule2_Min = Convert.ToDouble(txt_InspRule2Min.Text);
				selectWayParam.InspRule3_Min = Convert.ToDouble(txt_InspRule3Min.Text);
				selectWayParam.InspRule4_Min = Convert.ToDouble(txt_InspRule4Min.Text);
				selectWayParam.InspRule1_Max = Convert.ToDouble(txt_InspRule1Max.Text);
				selectWayParam.InspRule2_Max = Convert.ToDouble(txt_InspRule2Max.Text);
				selectWayParam.InspRule3_Max = Convert.ToDouble(txt_InspRule3Max.Text);
				selectWayParam.InspRule4_Max = Convert.ToDouble(txt_InspRule4Max.Text);
				if (rad_Follow.IsChecked == true)
				{
					selectWayParam.UseAutoSize = false;
				}
				else
				{
					selectWayParam.UseAutoSize = true;
				}
				selectWayParam.SuccessDoFlip = cb_SuccessDoFlip.SelectedIndex;
				selectWayParam.RotateMethod = cb_RotateMethod.SelectedIndex;
			}
			else
			{
				selectWayParam.SelelctWayFunction = 1;
				GetPatternSearchTracker(ref rect);
				selectWayParam.MatchScoreMin = Convert.ToInt32(txt_MatchScoreMin.Text);
				selectWayParam.MatchScoreMax = Convert.ToInt32(txt_MatchScoreMax.Text);
				selectWayParam.PatternSelect = cb_Pattern.SelectedIndex;
				if (rad_Follow2.IsChecked == true)
				{
					selectWayParam.UseAutoSize = false;
				}
				else
				{
					selectWayParam.UseAutoSize = true;
				}
				selectWayParam.SuccessDoFlip = cb_SuccessDoFlip2.SelectedIndex;
				selectWayParam.RotateMethod = cb_RotateMethod2.SelectedIndex;
			}
			if (DefaultSearchRect1 != rect)
			{
				selectWayParam.SearchRectL = rect.X - LocateRect.X;
				selectWayParam.SearchRectT = rect.Y - LocateRect.Y;
				selectWayParam.SearchRectR = LocateRect.Width - selectWayParam.SearchRectL - rect.Width;
				selectWayParam.SearchRectB = LocateRect.Height - selectWayParam.SearchRectT - rect.Height;
				selectWayParam.SearchRectWidth = rect.Width;
				selectWayParam.SearchRectHeight = rect.Height;
			}
			UpdateUI = true;
			inspection.paramFile.SelectWayParam = selectWayParam;
		}
		catch
		{
			string text = "SWTW03-SelectWayWindow-WriteUIValue()";
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
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		int[] array = new int[6];
		for (int i = 0; i < 6; i++)
		{
			array[i] = inspection.paramFile.EnhanceParam[i].FunctionIndex;
		}
		inspection.buffers.SetLocatedBuffer(OrgMat, array, inspection.paramFile.AIEnhanceToolON);
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
		WriteUIValueToParam();
		bool flag = false;
		if (SelectWayFunction == 0)
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
			OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetTracker(ref rect2, GetRect2: false);
			ShowBitmap = OrgMat.Clone();
			try
			{
				DoThdImage();
				flag = inspection.selectwayTool.Inspect();
				rect = inspection.selectwayTool.NGRect;
				bool flag2 = false;
				for (int j = 0; j < 4; j++)
				{
					if (InspRule[j].SelectedIndex == 6)
					{
						flag2 = true;
					}
				}
				if (flag)
				{
					ShowBitmap.Rectangle(rect, Scalar.Red);
					if (flag2)
					{
						ShowBitmap.Rectangle(rect2, Scalar.Red, 3);
						if (chk_Select2WayMode.IsChecked == true)
						{
							GetTracker(ref rect3, GetRect2: true);
							ShowBitmap.Rectangle(rect3, Scalar.Red, 3);
						}
					}
				}
				else
				{
					ShowBitmap.Rectangle(rect, Scalar.Green);
				}
				CheckResult();
				txt_AllResult.Clear();
				txt_AllResult.Text += inspection.selectwayTool.Tracker1ResultText;
			}
			catch
			{
				string text = "SWTW05-SelectWayWindow-Btn_Test_Click()";
				CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				if (ShowMessage)
				{
					MessageBox.Show(text, "錯誤");
				}
				return;
			}
		}
		else
		{
			if (cb_Pattern.SelectedIndex < 0 || cb_Pattern.SelectedIndex >= 20)
			{
				txt_AllResult.Text = LanguageManager.Instance["SelectMatchImage"];
				SetResult(result: false);
				return;
			}
			OpenCvSharp.Rect rect4 = default(OpenCvSharp.Rect);
			if (!GetPatternSearchTracker(ref rect4))
			{
				return;
			}
			if (rect4.Width <= 0 || rect4.Height <= 0)
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
			flag = inspection.selectwayTool.Inspect();
			if (inspection.selectwayTool.PatternFound)
			{
				img_PatternFound.Source = inspection.buffers.MatToBitmapImage(inspection.selectwayTool.FoundMat);
				lab_MatchScore.Content = string.Format("{0}", inspection.selectwayTool.MatchScore);
			}
			else
			{
				img_PatternFound.Source = null;
				lab_MatchScore.Content = string.Format("{0}", 0);
			}
			if (inspection.selectwayTool.MatchScore > Convert.ToInt32(txt_MatchScoreMax.Text))
			{
				txt_MatchScoreMax.Background = Brushes.Fuchsia;
			}
			else
			{
				txt_MatchScoreMax.Background = Brushes.White;
			}
			if (inspection.selectwayTool.MatchScore < Convert.ToInt32(txt_MatchScoreMin.Text))
			{
				txt_MatchScoreMin.Background = Brushes.Fuchsia;
			}
			else
			{
				txt_MatchScoreMin.Background = Brushes.White;
			}
			if (inspection.selectwayTool.SelectWaySuccess)
			{
				ep_Result.Fill = Brushes.Lime;
			}
			else
			{
				ep_Result.Fill = Brushes.Red;
			}
			ShowBitmap = OrgMat.Clone();
			txt_AllResult.Clear();
			txt_AllResult.Text = inspection.selectwayTool.Tracker1ResultText;
			if (inspection.selectwayTool.FoundRect.Width > 0 && inspection.selectwayTool.FoundRect.Height > 0)
			{
				ShowBitmap.Rectangle(inspection.selectwayTool.FoundRect, Scalar.GreenYellow);
			}
		}
		if (flag)
		{
			btn_Result_FlipResult.Visibility = Visibility.Visible;
		}
		else
		{
			btn_Result_FlipResult.Visibility = Visibility.Hidden;
		}
		SetResult(inspection.selectwayTool.SelectWaySuccess);
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/selectwaywindow.xaml", UriKind.Relative);
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
			MainW = (SelectWayWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown_1;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			cnv_Image.PreviewMouseRightButtonDown += Cnv_Image_PreviewMouseRightButtonDown;
			break;
		case 3:
			sfr = (ScaleTransform)target;
			break;
		case 4:
			Label_Result = (Label)target;
			break;
		case 5:
			bor_LocateRect = (Border)target;
			break;
		case 6:
			bor_SearchRect1 = (Border)target;
			break;
		case 7:
			txt_Search1Label = (TextBlock)target;
			break;
		case 8:
			bor_PatternSet = (Border)target;
			break;
		case 9:
			txt_Search2Label = (TextBlock)target;
			break;
		case 10:
			bor_PatternSearch = (Border)target;
			break;
		case 11:
			txt_Search1Label1 = (TextBlock)target;
			break;
		case 12:
			btn_Result_FlipResult = (Button)target;
			break;
		case 13:
			bor_SearchRect2 = (Border)target;
			break;
		case 14:
			txt_Search2Label1 = (TextBlock)target;
			break;
		case 15:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 16:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 17:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 18:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 19:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 20:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 21:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 22:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 23:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 24:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 25:
			txt_Tracker1Pos = (Label)target;
			break;
		case 26:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 27:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 28:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 29:
			btn_Result_Flip = (Button)target;
			btn_Result_Flip.Click += Btn_ImgSrc_Click;
			break;
		case 30:
			lab_RValue = (Label)target;
			break;
		case 31:
			lab_GValue = (Label)target;
			break;
		case 32:
			lab_BValue = (Label)target;
			break;
		case 33:
			lab_HValue = (Label)target;
			break;
		case 34:
			lab_SValue = (Label)target;
			break;
		case 35:
			lab_VValue = (Label)target;
			break;
		case 36:
			lab_GrayValue = (Label)target;
			break;
		case 37:
			lab_XValue = (Label)target;
			break;
		case 38:
			lab_YValue = (Label)target;
			break;
		case 39:
			lab_C1Value = (Label)target;
			break;
		case 40:
			lab_C2Value = (Label)target;
			break;
		case 41:
			lab_C3Value = (Label)target;
			break;
		case 42:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 43:
			txt_AllResult = (TextBox)target;
			break;
		case 44:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 45:
			tab_Function = (TabControl)target;
			tab_Function.SelectionChanged += Tab_Function_SelectionChanged;
			break;
		case 46:
			tab_Binary = (TabItem)target;
			break;
		case 47:
			scr_TrackerSwich_V = (ScrollBar)target;
			scr_TrackerSwich_V.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 48:
			grd_BinaryBlock = (Grid)target;
			break;
		case 49:
			exp_THD1 = (Expander)target;
			break;
		case 50:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 51:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 52:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 53:
			cb_THD1_DEStart = (ComboBox)target;
			break;
		case 54:
			cb_THD1_DEEnd = (ComboBox)target;
			break;
		case 55:
			cb_THD1_DEEndCount = (ComboBox)target;
			break;
		case 56:
			cb_THD1_DEStartCount = (ComboBox)target;
			break;
		case 57:
			cb_Channel_THD1 = (ComboBox)target;
			break;
		case 58:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 59:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 60:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSU_Click;
			break;
		case 61:
			exp_THD2 = (Expander)target;
			break;
		case 62:
			grd_SIMP_THD_1_Copy = (Grid)target;
			break;
		case 63:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 64:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 65:
			cb_THD2_DEStart = (ComboBox)target;
			break;
		case 66:
			cb_THD2_DEEnd = (ComboBox)target;
			break;
		case 67:
			cb_THD2_DEEndCount = (ComboBox)target;
			break;
		case 68:
			cb_THD2_DEStartCount = (ComboBox)target;
			break;
		case 69:
			cb_Channel_THD2 = (ComboBox)target;
			break;
		case 70:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 71:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 72:
			exp_THD3 = (Expander)target;
			break;
		case 73:
			grd_SIMP_THD_1_Copy1 = (Grid)target;
			break;
		case 74:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 75:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 76:
			cb_THD3_DEStart = (ComboBox)target;
			break;
		case 77:
			cb_THD3_DEEnd = (ComboBox)target;
			break;
		case 78:
			cb_THD3_DEEndCount = (ComboBox)target;
			break;
		case 79:
			cb_THD3_DEStartCount = (ComboBox)target;
			break;
		case 80:
			cb_Channel_THD3 = (ComboBox)target;
			break;
		case 81:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 82:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 83:
			rad_Follow = (RadioButton)target;
			break;
		case 84:
			rad_AutoSize = (RadioButton)target;
			break;
		case 85:
			grd_SIMP_RefSetup = (Grid)target;
			break;
		case 86:
			cb_ColorReference = (ComboBox)target;
			cb_ColorReference.SelectionChanged += Cb_ColorReference_SelectionChanged;
			break;
		case 87:
			btn_ReferenceSave = (Button)target;
			btn_ReferenceSave.Click += Btn_ReferenceSave_Click;
			break;
		case 88:
			txt_ReferenceName = (TextBox)target;
			break;
		case 89:
			btn_ReferenceLock = (Button)target;
			btn_ReferenceLock.Click += Btn_ReferenceLock_Click;
			break;
		case 90:
			grd_Test = (Grid)target;
			break;
		case 91:
			grd_ScanV_Block = (Grid)target;
			break;
		case 92:
			btn_Test_Binary = (Button)target;
			btn_Test_Binary.Click += Btn_Test_Click;
			break;
		case 93:
			cb_RotateMethod = (ComboBox)target;
			break;
		case 94:
			btn_TrackerReset_Copy = (Button)target;
			btn_TrackerReset_Copy.Click += Btn_TrackerReset_Click;
			break;
		case 95:
			rt_Result = (Rectangle)target;
			break;
		case 96:
			txt_InspRule3Min = (TextBox)target;
			txt_InspRule3Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 97:
			txt_InspRule3Max = (TextBox)target;
			txt_InspRule3Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 98:
			txt_InspRule4Min = (TextBox)target;
			txt_InspRule4Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 99:
			txt_InspRule4Max = (TextBox)target;
			txt_InspRule4Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 100:
			txt_InspRule13Result = (TextBox)target;
			break;
		case 101:
			txt_InspRule14Result = (TextBox)target;
			break;
		case 102:
			txt_InspRule1Min = (TextBox)target;
			txt_InspRule1Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 103:
			txt_InspRule1Max = (TextBox)target;
			txt_InspRule1Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 104:
			txt_InspRule2Min = (TextBox)target;
			txt_InspRule2Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 105:
			txt_InspRule2Max = (TextBox)target;
			txt_InspRule2Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 106:
			txt_InspRule11Result = (TextBox)target;
			break;
		case 107:
			txt_InspRule12Result = (TextBox)target;
			break;
		case 108:
			cb_InspRule1 = (ComboBox)target;
			break;
		case 109:
			cb_InspRule2 = (ComboBox)target;
			break;
		case 110:
			cb_InspRule3 = (ComboBox)target;
			break;
		case 111:
			cb_InspRule4 = (ComboBox)target;
			break;
		case 112:
			rad_RuleAND = (RadioButton)target;
			break;
		case 113:
			rad_RuleOR = (RadioButton)target;
			break;
		case 114:
			cb_SuccessDoFlip = (ComboBox)target;
			break;
		case 115:
			chk_Select2WayMode = (CheckBox)target;
			chk_Select2WayMode.Click += Chk_Select2WayMode_Click;
			break;
		case 116:
			cb_RotateMethodB = (ComboBox)target;
			break;
		case 117:
			cb_SuccessDoFlipB = (ComboBox)target;
			break;
		case 118:
			lab_A = (Label)target;
			break;
		case 119:
			lab_B = (Label)target;
			break;
		case 120:
			btn_Result_Flip_Copy = (Button)target;
			btn_Result_Flip_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 121:
			tab_PatternMatch = (TabItem)target;
			break;
		case 122:
			gb_BasicSetup = (GroupBox)target;
			break;
		case 123:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 124:
			grd_Found = (Grid)target;
			break;
		case 125:
			txt_PatternName21 = (TextBox)target;
			break;
		case 126:
			img_PatternFound = (Image)target;
			break;
		case 127:
			lab_PatternFoundInfo = (Label)target;
			break;
		case 128:
			grd_Pattern = (Grid)target;
			break;
		case 129:
			img_PatternSet = (Image)target;
			break;
		case 130:
			lab_PatternSetInfo = (Label)target;
			break;
		case 131:
			cb_Pattern = (ComboBox)target;
			cb_Pattern.SelectionChanged += Cb_Pattern_SelectionChanged;
			break;
		case 132:
			ep_Result = (Ellipse)target;
			break;
		case 133:
			txt_MatchScoreMin = (TextBox)target;
			txt_MatchScoreMin.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 134:
			txt_MatchScoreMax = (TextBox)target;
			txt_MatchScoreMax.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 135:
			rt_Result1 = (Rectangle)target;
			break;
		case 136:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 137:
			lab_MatchScore = (Label)target;
			break;
		case 138:
			btn_ReDraw_Copy = (Button)target;
			break;
		case 139:
			btn_ReDraw_Copy1 = (Button)target;
			break;
		case 140:
			btn_ReDraw_Copy2 = (Button)target;
			break;
		case 141:
			cb_RotateMethod2 = (ComboBox)target;
			break;
		case 142:
			cb_SuccessDoFlip2 = (ComboBox)target;
			break;
		case 143:
			rad_AutoSize2 = (RadioButton)target;
			break;
		case 144:
			rad_Follow2 = (RadioButton)target;
			break;
		case 145:
			btn_PatternSearchTrackerReset = (Button)target;
			btn_PatternSearchTrackerReset.Click += Btn_PatternSearchTrackerReset_Click;
			break;
		case 146:
			tab_PatternDB = (TabItem)target;
			break;
		case 147:
			gb_BasicSetup_Copy = (GroupBox)target;
			break;
		case 148:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 149:
			btn_ChannelR = (Button)target;
			btn_ChannelR.Click += Btn_ChannelR_Click;
			break;
		case 150:
			btn_ChannelG = (Button)target;
			btn_ChannelG.Click += Btn_ChannelR_Click;
			break;
		case 151:
			btn_ChannelB = (Button)target;
			btn_ChannelB.Click += Btn_ChannelR_Click;
			break;
		case 152:
			btn_ChannelH = (Button)target;
			btn_ChannelH.Click += Btn_ChannelR_Click;
			break;
		case 153:
			btn_ChannelS = (Button)target;
			btn_ChannelS.Click += Btn_ChannelR_Click;
			break;
		case 154:
			btn_ChannelV = (Button)target;
			btn_ChannelV.Click += Btn_ChannelR_Click;
			break;
		case 155:
			btn_ChannelGray = (Button)target;
			btn_ChannelGray.Click += Btn_ChannelR_Click;
			break;
		case 156:
			btn_ChannelC1 = (Button)target;
			btn_ChannelC1.Click += Btn_ChannelR_Click;
			break;
		case 157:
			btn_ChannelC2 = (Button)target;
			btn_ChannelC2.Click += Btn_ChannelR_Click;
			break;
		case 158:
			btn_ChannelC3 = (Button)target;
			btn_ChannelC3.Click += Btn_ChannelR_Click;
			break;
		case 159:
			btn_ChannelRemove = (Button)target;
			btn_ChannelRemove.Click += Btn_ChannelR_Click;
			break;
		case 160:
			txt_PatternName1 = (TextBox)target;
			break;
		case 161:
			img_PatternImg1 = (Image)target;
			break;
		case 162:
			btn_SetPattern = (Button)target;
			btn_SetPattern.Click += Btn_SetPattern_Click;
			break;
		case 163:
			lab_PatternInfo1 = (Label)target;
			break;
		case 164:
			txt_PatternName6 = (TextBox)target;
			break;
		case 165:
			img_PatternImg6 = (Image)target;
			break;
		case 166:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 167:
			lab_PatternInfo6 = (Label)target;
			break;
		case 168:
			txt_PatternName11 = (TextBox)target;
			break;
		case 169:
			img_PatternImg11 = (Image)target;
			break;
		case 170:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 171:
			lab_PatternInfo11 = (Label)target;
			break;
		case 172:
			txt_PatternName16 = (TextBox)target;
			break;
		case 173:
			img_PatternImg16 = (Image)target;
			break;
		case 174:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 175:
			lab_PatternInfo16 = (Label)target;
			break;
		case 176:
			txt_PatternName2 = (TextBox)target;
			break;
		case 177:
			img_PatternImg2 = (Image)target;
			break;
		case 178:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 179:
			lab_PatternInfo2 = (Label)target;
			break;
		case 180:
			txt_PatternName7 = (TextBox)target;
			break;
		case 181:
			img_PatternImg7 = (Image)target;
			break;
		case 182:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 183:
			lab_PatternInfo7 = (Label)target;
			break;
		case 184:
			txt_PatternName12 = (TextBox)target;
			break;
		case 185:
			img_PatternImg12 = (Image)target;
			break;
		case 186:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 187:
			lab_PatternInfo12 = (Label)target;
			break;
		case 188:
			txt_PatternName17 = (TextBox)target;
			break;
		case 189:
			img_PatternImg17 = (Image)target;
			break;
		case 190:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 191:
			lab_PatternInfo17 = (Label)target;
			break;
		case 192:
			txt_PatternName3 = (TextBox)target;
			break;
		case 193:
			img_PatternImg3 = (Image)target;
			break;
		case 194:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 195:
			lab_PatternInfo3 = (Label)target;
			break;
		case 196:
			txt_PatternName8 = (TextBox)target;
			break;
		case 197:
			img_PatternImg8 = (Image)target;
			break;
		case 198:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 199:
			lab_PatternInfo8 = (Label)target;
			break;
		case 200:
			txt_PatternName13 = (TextBox)target;
			break;
		case 201:
			img_PatternImg13 = (Image)target;
			break;
		case 202:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 203:
			lab_PatternInfo13 = (Label)target;
			break;
		case 204:
			txt_PatternName18 = (TextBox)target;
			break;
		case 205:
			img_PatternImg18 = (Image)target;
			break;
		case 206:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 207:
			lab_PatternInfo18 = (Label)target;
			break;
		case 208:
			txt_PatternName4 = (TextBox)target;
			break;
		case 209:
			img_PatternImg4 = (Image)target;
			break;
		case 210:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 211:
			lab_PatternInfo4 = (Label)target;
			break;
		case 212:
			txt_PatternName9 = (TextBox)target;
			break;
		case 213:
			img_PatternImg9 = (Image)target;
			break;
		case 214:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 215:
			lab_PatternInfo9 = (Label)target;
			break;
		case 216:
			txt_PatternName14 = (TextBox)target;
			break;
		case 217:
			img_PatternImg14 = (Image)target;
			break;
		case 218:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 219:
			lab_PatternInfo14 = (Label)target;
			break;
		case 220:
			txt_PatternName19 = (TextBox)target;
			break;
		case 221:
			img_PatternImg19 = (Image)target;
			break;
		case 222:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 223:
			lab_PatternInfo19 = (Label)target;
			break;
		case 224:
			txt_PatternName5 = (TextBox)target;
			break;
		case 225:
			img_PatternImg5 = (Image)target;
			break;
		case 226:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 227:
			lab_PatternInfo5 = (Label)target;
			break;
		case 228:
			txt_PatternName10 = (TextBox)target;
			break;
		case 229:
			img_PatternImg10 = (Image)target;
			break;
		case 230:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 231:
			lab_PatternInfo10 = (Label)target;
			break;
		case 232:
			txt_PatternName15 = (TextBox)target;
			break;
		case 233:
			img_PatternImg15 = (Image)target;
			break;
		case 234:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 235:
			lab_PatternInfo15 = (Label)target;
			break;
		case 236:
			txt_PatternName20 = (TextBox)target;
			break;
		case 237:
			img_PatternImg20 = (Image)target;
			break;
		case 238:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 239:
			lab_PatternInfo20 = (Label)target;
			break;
		case 240:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 241:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 242:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 243:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 244:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 245:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 246:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 247:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 248:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 249:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 250:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 251:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 252:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 253:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 254:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 255:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 256:
			grd_TrackerAdj = (Grid)target;
			break;
		case 257:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 258:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 259:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 260:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 261:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
