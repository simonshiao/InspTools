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

namespace InspTools.AOI;

public class SelectFaceWindow : System.Windows.Window, IComponentConnector
{
	private int AutoThdValue = 50;

	private const int MaxThdTool = 3;

	private bool inited;

	private bool THD_Preview;

	private bool UpdateUI;

	private bool ColorDropper;

	private bool ShowMessage = true;

	private byte[] CurrentColor = new byte[10];

	private OpenCvSharp.Rect DefaultSearchRect = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private int LanguageType = 1;

	private int FaceIndex;

	private bool TrackerAdjShow;

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

	private bool OTSU_White = true;

	internal SelectFaceWindow MainW;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Label Label_Result;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

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

	internal CheckBox chk_Thd2_ORMode;

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

	internal CheckBox chk_Thd3_ORMode;

	internal RadioButton rad_Follow;

	internal RadioButton rad_AutoSize;

	internal CheckBox chk_RowSelectFace;

	internal CheckBox chk_RowMinSelectFace;

	internal Grid grd_Test;

	internal Grid grd_ScanV_Block;

	internal Button btn_Test_V;

	internal Button btn_TrackerReset_Copy;

	internal Rectangle rt_Result;

	internal TextBox txt_InspRule3Min;

	internal TextBox txt_InspRule3Max;

	internal TextBox txt_InspRule4Min;

	internal TextBox txt_InspRule4Max;

	internal TextBox txt_InspRule13Result;

	internal TextBox txt_InspRule14Result;

	internal TextBox txt_InspRule1Min;

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

	internal TextBox txt_InspRule1Max;

	internal ComboBox cb_ErrorBin;

	internal ScrollBar scr_TrackerSwich_V;

	internal ToggleSwitch ts_TrackerON;

	internal Label lab_TrackIndex;

	internal CheckBox chk_EnableMultiFace;

	internal TabControl tab_FACE;

	internal TabItem tab_FaceA;

	internal TabItem tab_FaceB;

	internal TabItem tab_FaceC;

	internal TabItem tab_FaceD;

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

	public SelectFaceWindow()
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
		Label_Result.Visibility = Visibility.Hidden;
		exp_TrackerAdj.Visibility = Visibility.Hidden;
		chk_EnableMultiFace.Visibility = Visibility.Hidden;
	}

	public void SetInspection(int index, CVInspection station)
	{
		inspection = station;
		advLocateTool = new AdvLocateTool(inspection.paramFile);
		UpdateUI = false;
		FaceIndex = index;
		tab_FaceA.Background = Brushes.Yellow;
		UpdateUI = true;
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
			image = inspection.buffers.LocatedSrc;
		}
		goto IL_01cd;
		IL_017f:
		image = inspection.buffers.GetLocatedCustomMat(1);
		goto IL_01cd;
		IL_01ba:
		image = (image = inspection.buffers.LocatedSrc);
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
	private void SetTracker(OpenCvSharp.Rect rect1)
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTracker(ref OpenCvSharp.Rect rect1)
	{
		if (xRatio != 0.0 && yRatio != 0.0)
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
		try
		{
			int[] array = new int[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = inspection.paramFile.EnhanceParam[i].FunctionIndex;
			}
			inspection.buffers.SetLocatedBuffer(inspection.buffers.LocatedSrc, array, inspection.paramFile.AIEnhanceToolON);
			Label_Result.Visibility = Visibility.Hidden;
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
			bool[] array12 = new bool[3]
			{
				false,
				chk_Thd2_ORMode.IsChecked.Value,
				chk_Thd3_ORMode.IsChecked.Value
			};
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
					if (k == 0 || !array12[k])
					{
						Cv2.BitwiseAnd(mat, array3[k], mat);
					}
					else
					{
						Cv2.BitwiseOr(mat, array3[k], mat);
					}
				}
			}
			ScanBinary = mat.Clone();
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 100, 100);
			new OpenCvSharp.Rect(0, 0, 100, 100);
			GetTracker(ref rect);
			BinaryBlob = new Mat(mat, rect).Clone();
			Mat mat2 = mat.Clone();
			mat2.SetTo(0.0);
			new Mat(mat2, rect).SetTo(255.0);
			Cv2.BitwiseAnd(mat, mat2, mat);
			Cv2.Split(ShowBitmap, out ThdShow);
			ThdShow[0].SetTo(255.0);
			ThdShow[1].SetTo(0.0);
			ThdShow[2].SetTo(0.0);
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
			CVBuffer.SaveLog("SFTW01-SelectFaceWindow-CheckResult()", inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_TrackerON.IsChecked.Value)
		{
			bor_SearchRect1.Visibility = Visibility.Visible;
			grd_BinaryBlock.Visibility = Visibility.Visible;
			grd_Test.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect1.Visibility = Visibility.Hidden;
			grd_BinaryBlock.Visibility = Visibility.Hidden;
			grd_Test.Visibility = Visibility.Visible;
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
	private void CopyValueToUI()
	{
		try
		{
			UpdateUI = false;
			INSP_SELECTFACETOOL_PARAM iNSP_SELECTFACETOOL_PARAM = default(INSP_SELECTFACETOOL_PARAM);
			iNSP_SELECTFACETOOL_PARAM = inspection.paramFile.SelectFaceParam[FaceIndex];
			ts_TrackerON.IsChecked = iNSP_SELECTFACETOOL_PARAM.TrackerON;
			chk_EnableMultiFace.IsChecked = inspection.paramFile.SelectFaceParam[0].EnableMultiFace;
			if (iNSP_SELECTFACETOOL_PARAM.TrackerON)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
				grd_BinaryBlock.Visibility = Visibility.Visible;
				grd_Test.Visibility = Visibility.Visible;
			}
			else
			{
				bor_SearchRect1.Visibility = Visibility.Hidden;
				grd_BinaryBlock.Visibility = Visibility.Hidden;
				grd_Test.Visibility = Visibility.Visible;
			}
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			rect.X = LocateRect.X + iNSP_SELECTFACETOOL_PARAM.SearchRectL;
			rect.Y = LocateRect.Y + iNSP_SELECTFACETOOL_PARAM.SearchRectT;
			if (iNSP_SELECTFACETOOL_PARAM.UseAutoSize)
			{
				rect.Width = LocateRect.Width - (iNSP_SELECTFACETOOL_PARAM.SearchRectL + iNSP_SELECTFACETOOL_PARAM.SearchRectR);
				rect.Height = LocateRect.Height - (iNSP_SELECTFACETOOL_PARAM.SearchRectT + iNSP_SELECTFACETOOL_PARAM.SearchRectB);
				rad_AutoSize.IsChecked = true;
			}
			else
			{
				rect.Width = iNSP_SELECTFACETOOL_PARAM.SearchRectWidth;
				rect.Height = iNSP_SELECTFACETOOL_PARAM.SearchRectHeight;
				rad_Follow.IsChecked = true;
			}
			chk_RowSelectFace.IsChecked = iNSP_SELECTFACETOOL_PARAM.RowSelectFace;
			if (iNSP_SELECTFACETOOL_PARAM.RowSelectFace)
			{
				chk_RowMinSelectFace.IsChecked = false;
			}
			else
			{
				chk_RowMinSelectFace.IsChecked = iNSP_SELECTFACETOOL_PARAM.RowMinSelectFace;
			}
			SetTracker(rect);
			GetTracker(ref DefaultSearchRect);
			LastSearchRect1 = rect;
			cb_Channel_THD1.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.THD1_ChannelIndex;
			cb_Channel_THD2.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.THD2_ChannelIndex;
			cb_Channel_THD3.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.THD3_ChannelIndex;
			exp_THD1.IsExpanded = iNSP_SELECTFACETOOL_PARAM.THD1_ChannelON;
			exp_THD2.IsExpanded = iNSP_SELECTFACETOOL_PARAM.THD2_ChannelON;
			exp_THD3.IsExpanded = iNSP_SELECTFACETOOL_PARAM.THD3_ChannelON;
			chk_Thd2_ORMode.IsChecked = iNSP_SELECTFACETOOL_PARAM.THD2_ORMode;
			chk_Thd3_ORMode.IsChecked = iNSP_SELECTFACETOOL_PARAM.THD3_ORMode;
			if (chk_Thd2_ORMode.IsChecked == true)
			{
				grd_SIMP_THD_2.Background = Brushes.LightGoldenrodYellow;
			}
			else
			{
				grd_SIMP_THD_2.Background = Brushes.White;
			}
			if (chk_Thd3_ORMode.IsChecked == true)
			{
				grd_SIMP_THD_3.Background = Brushes.LightGoldenrodYellow;
			}
			else
			{
				grd_SIMP_THD_3.Background = Brushes.White;
			}
			rs_THD1.LowerValue = iNSP_SELECTFACETOOL_PARAM.THD1_Min;
			rs_THD1.UpperValue = iNSP_SELECTFACETOOL_PARAM.THD1_Max;
			nu_Thd1LimitMin.Value = iNSP_SELECTFACETOOL_PARAM.THD1_Min;
			nu_Thd1LimitMax.Value = iNSP_SELECTFACETOOL_PARAM.THD1_Max;
			rs_THD2.LowerValue = iNSP_SELECTFACETOOL_PARAM.THD2_Min;
			rs_THD2.UpperValue = iNSP_SELECTFACETOOL_PARAM.THD2_Max;
			nu_Thd2LimitMin.Value = iNSP_SELECTFACETOOL_PARAM.THD2_Min;
			nu_Thd2LimitMax.Value = iNSP_SELECTFACETOOL_PARAM.THD2_Max;
			rs_THD3.LowerValue = iNSP_SELECTFACETOOL_PARAM.THD3_Min;
			rs_THD3.UpperValue = iNSP_SELECTFACETOOL_PARAM.THD3_Max;
			nu_Thd3LimitMin.Value = iNSP_SELECTFACETOOL_PARAM.THD3_Min;
			nu_Thd3LimitMax.Value = iNSP_SELECTFACETOOL_PARAM.THD3_Max;
			cb_THD1_DEStart.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.DE_Start1_Index;
			cb_THD1_DEEnd.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.DE_End1_Index;
			cb_THD2_DEStart.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.DE_Start2_Index;
			cb_THD2_DEEnd.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.DE_End2_Index;
			cb_THD3_DEStart.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.DE_Start3_Index;
			cb_THD3_DEEnd.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.DE_End3_Index;
			cb_THD1_DEStartCount.SelectedIndex = iNSP_SELECTFACETOOL_PARAM.DE_Start1_Count;
			cb_THD1_DEEndCount.SelectedIndex = iNSP_SELECTFACETOOL_PARAM.DE_End1_Count;
			cb_THD2_DEStartCount.SelectedIndex = iNSP_SELECTFACETOOL_PARAM.DE_Start2_Count;
			cb_THD2_DEEndCount.SelectedIndex = iNSP_SELECTFACETOOL_PARAM.DE_End2_Count;
			cb_THD3_DEStartCount.SelectedIndex = iNSP_SELECTFACETOOL_PARAM.DE_Start3_Count;
			cb_THD3_DEEndCount.SelectedIndex = iNSP_SELECTFACETOOL_PARAM.DE_End3_Count;
			cb_InspRule1.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.InspRule1_Index;
			cb_InspRule2.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.InspRule2_Index;
			cb_InspRule3.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.InspRule3_Index;
			cb_InspRule4.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.InspRule4_Index;
			if (iNSP_SELECTFACETOOL_PARAM.InspRuleAND)
			{
				rad_RuleAND.IsChecked = true;
			}
			else
			{
				rad_RuleOR.IsChecked = true;
			}
			txt_InspRule1Min.Text = iNSP_SELECTFACETOOL_PARAM.InspRule1_Min.ToString();
			txt_InspRule2Min.Text = iNSP_SELECTFACETOOL_PARAM.InspRule2_Min.ToString();
			txt_InspRule3Min.Text = iNSP_SELECTFACETOOL_PARAM.InspRule3_Min.ToString();
			txt_InspRule4Min.Text = iNSP_SELECTFACETOOL_PARAM.InspRule4_Min.ToString();
			txt_InspRule1Max.Text = iNSP_SELECTFACETOOL_PARAM.InspRule1_Max.ToString();
			txt_InspRule2Max.Text = iNSP_SELECTFACETOOL_PARAM.InspRule2_Max.ToString();
			txt_InspRule3Max.Text = iNSP_SELECTFACETOOL_PARAM.InspRule3_Max.ToString();
			txt_InspRule4Max.Text = iNSP_SELECTFACETOOL_PARAM.InspRule4_Max.ToString();
			cb_ErrorBin.SelectedIndex = (int)iNSP_SELECTFACETOOL_PARAM.ErrorBin;
			UpdateUI = true;
		}
		catch
		{
			string text = "SFTW02-SelectFaceWindow-CopyValueToUI()";
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
			rect = inspection.selectfaceTool.Blobs.NGRect;
			double num = 0.0;
			num = inspection.selectfaceTool.Blobs.Area;
			bool[] array = new bool[4];
			bool flag = true;
			TextBox[] result1Txt = Result1Txt;
			array[0] = GetRule(cb_InspRule1.SelectedIndex, txt_InspRule1Min, txt_InspRule1Max, result1Txt[0], rect, num);
			array[1] = GetRule(cb_InspRule2.SelectedIndex, txt_InspRule2Min, txt_InspRule2Max, result1Txt[1], rect, num);
			array[2] = GetRule(cb_InspRule3.SelectedIndex, txt_InspRule3Min, txt_InspRule3Max, result1Txt[2], rect, num);
			array[3] = GetRule(cb_InspRule4.SelectedIndex, txt_InspRule4Min, txt_InspRule4Max, result1Txt[3], rect, num);
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
				double allArea = inspection.selectfaceTool.AllArea;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_FACE_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && e.AddedItems[0] is TabItem tabItem)
		{
			tab_FaceA.Background = Brushes.White;
			tab_FaceB.Background = Brushes.White;
			tab_FaceC.Background = Brushes.White;
			tab_FaceD.Background = Brushes.White;
			if (tabItem.Name == "tab_FaceA")
			{
				tab_FaceA.Background = Brushes.Yellow;
				lab_TrackIndex.Content = "A";
				WriteUIValueToParam();
				FaceIndex = 0;
				CopyValueToUI();
			}
			else if (tabItem.Name == "tab_FaceB")
			{
				tab_FaceB.Background = Brushes.Yellow;
				lab_TrackIndex.Content = "B";
				WriteUIValueToParam();
				FaceIndex = 1;
				CopyValueToUI();
			}
			else if (tabItem.Name == "tab_FaceC")
			{
				tab_FaceC.Background = Brushes.Yellow;
				lab_TrackIndex.Content = "C";
				WriteUIValueToParam();
				FaceIndex = 2;
				CopyValueToUI();
			}
			else if (tabItem.Name == "tab_FaceD")
			{
				tab_FaceD.Background = Brushes.Yellow;
				lab_TrackIndex.Content = "D";
				WriteUIValueToParam();
				FaceIndex = 3;
				CopyValueToUI();
			}
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
			string text = "BW10-SelectFaceWindow-Btn_OTSUL_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Chk_RowSelectFace_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			chk_RowMinSelectFace.IsChecked = false;
		}
	}

	private void Chk_MinRowSelectFace_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			chk_RowSelectFace.IsChecked = false;
		}
	}

	private void Chk_Thd2_ORMode_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			if (chk_Thd2_ORMode.IsChecked == true)
			{
				grd_SIMP_THD_2.Background = Brushes.LightGoldenrodYellow;
			}
			else
			{
				grd_SIMP_THD_2.Background = Brushes.White;
			}
			if (chk_Thd3_ORMode.IsChecked == true)
			{
				grd_SIMP_THD_3.Background = Brushes.LightGoldenrodYellow;
			}
			else
			{
				grd_SIMP_THD_3.Background = Brushes.White;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		try
		{
			UpdateUI = false;
			INSP_SELECTFACETOOL_PARAM iNSP_SELECTFACETOOL_PARAM = inspection.paramFile.SelectFaceParam[FaceIndex];
			inspection.paramFile.SelectFaceParam[0].EnableMultiFace = chk_EnableMultiFace.IsChecked.Value;
			iNSP_SELECTFACETOOL_PARAM.ValueChangeFlag = true;
			iNSP_SELECTFACETOOL_PARAM.TrackerON = ts_TrackerON.IsChecked.Value;
			iNSP_SELECTFACETOOL_PARAM.RowSelectFace = chk_RowSelectFace.IsChecked.Value;
			iNSP_SELECTFACETOOL_PARAM.RowMinSelectFace = chk_RowMinSelectFace.IsChecked.Value;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect);
			if (DefaultSearchRect != rect)
			{
				iNSP_SELECTFACETOOL_PARAM.SearchRectL = rect.X - LocateRect.X;
				iNSP_SELECTFACETOOL_PARAM.SearchRectT = rect.Y - LocateRect.Y;
				iNSP_SELECTFACETOOL_PARAM.SearchRectR = LocateRect.Width - iNSP_SELECTFACETOOL_PARAM.SearchRectL - rect.Width;
				iNSP_SELECTFACETOOL_PARAM.SearchRectB = LocateRect.Height - iNSP_SELECTFACETOOL_PARAM.SearchRectT - rect.Height;
				iNSP_SELECTFACETOOL_PARAM.SearchRectWidth = rect.Width;
				iNSP_SELECTFACETOOL_PARAM.SearchRectHeight = rect.Height;
			}
			iNSP_SELECTFACETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD1.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.THD1_ChannelON = exp_THD1.IsExpanded;
			iNSP_SELECTFACETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD2.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.THD2_ChannelON = exp_THD2.IsExpanded;
			iNSP_SELECTFACETOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)cb_Channel_THD3.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.THD3_ChannelON = exp_THD3.IsExpanded;
			iNSP_SELECTFACETOOL_PARAM.THD2_ORMode = chk_Thd2_ORMode.IsChecked.Value;
			iNSP_SELECTFACETOOL_PARAM.THD3_ORMode = chk_Thd3_ORMode.IsChecked.Value;
			iNSP_SELECTFACETOOL_PARAM.THD1_Min = (int)rs_THD1.LowerValue;
			iNSP_SELECTFACETOOL_PARAM.THD1_Max = (int)rs_THD1.UpperValue;
			iNSP_SELECTFACETOOL_PARAM.THD2_Min = (int)rs_THD2.LowerValue;
			iNSP_SELECTFACETOOL_PARAM.THD2_Max = (int)rs_THD2.UpperValue;
			iNSP_SELECTFACETOOL_PARAM.THD3_Min = (int)rs_THD3.LowerValue;
			iNSP_SELECTFACETOOL_PARAM.THD3_Max = (int)rs_THD3.UpperValue;
			iNSP_SELECTFACETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)cb_THD1_DEStart.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)cb_THD1_DEEnd.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)cb_THD2_DEStart.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)cb_THD2_DEEnd.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)cb_THD3_DEStart.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)cb_THD3_DEEnd.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_Start1_Count = cb_THD1_DEStartCount.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_End1_Count = cb_THD1_DEEndCount.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_Start2_Count = cb_THD2_DEStartCount.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_End2_Count = cb_THD2_DEEndCount.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_Start3_Count = cb_THD3_DEStartCount.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.DE_End3_Count = cb_THD3_DEEndCount.SelectedIndex;
			if (rad_Follow.IsChecked == true)
			{
				iNSP_SELECTFACETOOL_PARAM.UseAutoSize = false;
			}
			else
			{
				iNSP_SELECTFACETOOL_PARAM.UseAutoSize = true;
			}
			iNSP_SELECTFACETOOL_PARAM.InspRuleAND = rad_RuleAND.IsChecked.Value;
			iNSP_SELECTFACETOOL_PARAM.InspRule1_Index = (BlobCondition)cb_InspRule1.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.InspRule2_Index = (BlobCondition)cb_InspRule2.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.InspRule3_Index = (BlobCondition)cb_InspRule3.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.InspRule4_Index = (BlobCondition)cb_InspRule4.SelectedIndex;
			iNSP_SELECTFACETOOL_PARAM.InspRule1_Min = Convert.ToDouble(txt_InspRule1Min.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule2_Min = Convert.ToDouble(txt_InspRule2Min.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule3_Min = Convert.ToDouble(txt_InspRule3Min.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule4_Min = Convert.ToDouble(txt_InspRule4Min.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule1_Max = Convert.ToDouble(txt_InspRule1Max.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule2_Max = Convert.ToDouble(txt_InspRule2Max.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule3_Max = Convert.ToDouble(txt_InspRule3Max.Text);
			iNSP_SELECTFACETOOL_PARAM.InspRule4_Max = Convert.ToDouble(txt_InspRule4Max.Text);
			iNSP_SELECTFACETOOL_PARAM.ErrorBin = (FLOW_BIN)cb_ErrorBin.SelectedIndex;
			UpdateUI = true;
			inspection.paramFile.SelectFaceParam[FaceIndex] = iNSP_SELECTFACETOOL_PARAM;
		}
		catch
		{
			string text = "SFTW03-SelectFaceWindow-WriteUIValue()";
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
		if (ts_TrackerON.IsChecked == false)
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
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect2);
		if (inspection.buffers.LocatedSrc.Channels() == 3)
		{
			ShowBitmap = inspection.buffers.LocatedSrc.Clone();
		}
		else
		{
			Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
		}
		try
		{
			DoThdImage();
			bool flag = inspection.selectfaceTool.Inspect(FaceIndex);
			rect = inspection.selectfaceTool.NGRect;
			bool flag2 = false;
			for (int i = 0; i < 4; i++)
			{
				if (InspRule[i].SelectedIndex == 6)
				{
					flag2 = true;
				}
			}
			if (!flag)
			{
				ShowBitmap.Rectangle(rect, Scalar.Red);
				if (flag2)
				{
					ShowBitmap.Rectangle(rect2, Scalar.Red, 3);
				}
			}
			else
			{
				ShowBitmap.Rectangle(rect, Scalar.Green);
			}
			CheckResult();
			SetResult(flag);
			txt_AllResult.Clear();
			txt_AllResult.Text += inspection.selectfaceTool.Tracker1ResultText;
		}
		catch
		{
			string text = "SFTW05-SelectFaceWindow-Btn_Test_Click()";
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/selectfacewindow.xaml", UriKind.Relative);
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
			MainW = (SelectFaceWindow)target;
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
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 9:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 10:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 11:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 12:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 13:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 14:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 15:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 16:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 17:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 18:
			txt_Tracker1Pos = (Label)target;
			break;
		case 19:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 20:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 21:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 22:
			lab_RValue = (Label)target;
			break;
		case 23:
			lab_GValue = (Label)target;
			break;
		case 24:
			lab_BValue = (Label)target;
			break;
		case 25:
			lab_HValue = (Label)target;
			break;
		case 26:
			lab_SValue = (Label)target;
			break;
		case 27:
			lab_VValue = (Label)target;
			break;
		case 28:
			lab_GrayValue = (Label)target;
			break;
		case 29:
			lab_XValue = (Label)target;
			break;
		case 30:
			lab_YValue = (Label)target;
			break;
		case 31:
			lab_C1Value = (Label)target;
			break;
		case 32:
			lab_C2Value = (Label)target;
			break;
		case 33:
			lab_C3Value = (Label)target;
			break;
		case 34:
			btn_ColorDropper = (Button)target;
			btn_ColorDropper.Click += Btn_ColorDropper_Click;
			break;
		case 35:
			txt_AllResult = (TextBox)target;
			break;
		case 36:
			btn_ReDraw = (Button)target;
			btn_ReDraw.Click += Btn_ReDraw_Click;
			break;
		case 37:
			grd_BinaryBlock = (Grid)target;
			break;
		case 38:
			exp_THD1 = (Expander)target;
			break;
		case 39:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 40:
			rs_THD1 = (RangeSlider)target;
			rs_THD1.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD1.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 41:
			grd_SIMP_DilateErode1 = (Grid)target;
			break;
		case 42:
			cb_THD1_DEStart = (ComboBox)target;
			break;
		case 43:
			cb_THD1_DEEnd = (ComboBox)target;
			break;
		case 44:
			cb_THD1_DEEndCount = (ComboBox)target;
			break;
		case 45:
			cb_THD1_DEStartCount = (ComboBox)target;
			break;
		case 46:
			cb_Channel_THD1 = (ComboBox)target;
			break;
		case 47:
			nu_Thd1LimitMin = (NumericUpDown)target;
			nu_Thd1LimitMin.ValueChanged += Nu_ThdLimitMin_ValueChanged;
			break;
		case 48:
			nu_Thd1LimitMax = (NumericUpDown)target;
			nu_Thd1LimitMax.ValueChanged += Nu_Thd1LimitMax_ValueChanged;
			break;
		case 49:
			btn_OTSU = (Button)target;
			btn_OTSU.Click += Btn_OTSU_Click;
			break;
		case 50:
			exp_THD2 = (Expander)target;
			break;
		case 51:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 52:
			rs_THD2 = (RangeSlider)target;
			rs_THD2.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD2.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 53:
			grd_SIMP_DilateErode2 = (Grid)target;
			break;
		case 54:
			cb_THD2_DEStart = (ComboBox)target;
			break;
		case 55:
			cb_THD2_DEEnd = (ComboBox)target;
			break;
		case 56:
			cb_THD2_DEEndCount = (ComboBox)target;
			break;
		case 57:
			cb_THD2_DEStartCount = (ComboBox)target;
			break;
		case 58:
			cb_Channel_THD2 = (ComboBox)target;
			break;
		case 59:
			nu_Thd2LimitMin = (NumericUpDown)target;
			nu_Thd2LimitMin.ValueChanged += Nu_Thd2LimitMin_ValueChanged;
			break;
		case 60:
			nu_Thd2LimitMax = (NumericUpDown)target;
			nu_Thd2LimitMax.ValueChanged += Nu_Thd2LimitMax_ValueChanged;
			break;
		case 61:
			chk_Thd2_ORMode = (CheckBox)target;
			chk_Thd2_ORMode.Click += Chk_Thd2_ORMode_Click;
			break;
		case 62:
			exp_THD3 = (Expander)target;
			break;
		case 63:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 64:
			rs_THD3 = (RangeSlider)target;
			rs_THD3.LowerThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.UpperThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			rs_THD3.CentralThumbDragDelta += Rs_SIMP_THD1_ThumbDragCompleted;
			break;
		case 65:
			grd_SIMP_DilateErode3 = (Grid)target;
			break;
		case 66:
			cb_THD3_DEStart = (ComboBox)target;
			break;
		case 67:
			cb_THD3_DEEnd = (ComboBox)target;
			break;
		case 68:
			cb_THD3_DEEndCount = (ComboBox)target;
			break;
		case 69:
			cb_THD3_DEStartCount = (ComboBox)target;
			break;
		case 70:
			cb_Channel_THD3 = (ComboBox)target;
			break;
		case 71:
			nu_Thd3LimitMin = (NumericUpDown)target;
			nu_Thd3LimitMin.ValueChanged += Nu_Thd3LimitMin_ValueChanged;
			break;
		case 72:
			nu_Thd3LimitMax = (NumericUpDown)target;
			nu_Thd3LimitMax.ValueChanged += Nu_Thd3LimitMax_ValueChanged;
			break;
		case 73:
			chk_Thd3_ORMode = (CheckBox)target;
			chk_Thd3_ORMode.Click += Chk_Thd2_ORMode_Click;
			break;
		case 74:
			rad_Follow = (RadioButton)target;
			break;
		case 75:
			rad_AutoSize = (RadioButton)target;
			break;
		case 76:
			chk_RowSelectFace = (CheckBox)target;
			chk_RowSelectFace.Checked += Chk_RowSelectFace_Checked;
			break;
		case 77:
			chk_RowMinSelectFace = (CheckBox)target;
			chk_RowMinSelectFace.Checked += Chk_MinRowSelectFace_Checked;
			break;
		case 78:
			grd_Test = (Grid)target;
			break;
		case 79:
			grd_ScanV_Block = (Grid)target;
			break;
		case 80:
			btn_Test_V = (Button)target;
			btn_Test_V.Click += Btn_Test_Click;
			break;
		case 81:
			btn_TrackerReset_Copy = (Button)target;
			btn_TrackerReset_Copy.Click += Btn_TrackerReset_Click;
			break;
		case 82:
			rt_Result = (Rectangle)target;
			break;
		case 83:
			txt_InspRule3Min = (TextBox)target;
			txt_InspRule3Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 84:
			txt_InspRule3Max = (TextBox)target;
			txt_InspRule3Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 85:
			txt_InspRule4Min = (TextBox)target;
			txt_InspRule4Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 86:
			txt_InspRule4Max = (TextBox)target;
			txt_InspRule4Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 87:
			txt_InspRule13Result = (TextBox)target;
			break;
		case 88:
			txt_InspRule14Result = (TextBox)target;
			break;
		case 89:
			txt_InspRule1Min = (TextBox)target;
			txt_InspRule1Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 90:
			txt_InspRule2Min = (TextBox)target;
			txt_InspRule2Min.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 91:
			txt_InspRule2Max = (TextBox)target;
			txt_InspRule2Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 92:
			txt_InspRule11Result = (TextBox)target;
			break;
		case 93:
			txt_InspRule12Result = (TextBox)target;
			break;
		case 94:
			cb_InspRule1 = (ComboBox)target;
			break;
		case 95:
			cb_InspRule2 = (ComboBox)target;
			break;
		case 96:
			cb_InspRule3 = (ComboBox)target;
			break;
		case 97:
			cb_InspRule4 = (ComboBox)target;
			break;
		case 98:
			rad_RuleAND = (RadioButton)target;
			break;
		case 99:
			rad_RuleOR = (RadioButton)target;
			break;
		case 100:
			txt_InspRule1Max = (TextBox)target;
			txt_InspRule1Max.PreviewMouseDoubleClick += Txt_InspRule1Min_PreviewMouseDoubleClick;
			break;
		case 101:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 102:
			scr_TrackerSwich_V = (ScrollBar)target;
			scr_TrackerSwich_V.ValueChanged += Scr_TrackerSwich_ValueChanged;
			break;
		case 103:
			ts_TrackerON = (ToggleSwitch)target;
			ts_TrackerON.Click += Ts_TrackerON_Click;
			break;
		case 104:
			lab_TrackIndex = (Label)target;
			break;
		case 105:
			chk_EnableMultiFace = (CheckBox)target;
			break;
		case 106:
			tab_FACE = (TabControl)target;
			tab_FACE.SelectionChanged += Tab_FACE_SelectionChanged;
			break;
		case 107:
			tab_FaceA = (TabItem)target;
			break;
		case 108:
			tab_FaceB = (TabItem)target;
			break;
		case 109:
			tab_FaceC = (TabItem)target;
			break;
		case 110:
			tab_FaceD = (TabItem)target;
			break;
		case 111:
			btn_TrackerAdjShow = (Button)target;
			btn_TrackerAdjShow.Click += Btn_TrackerAdjShow_Click;
			break;
		case 112:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 113:
			exp_TrackerAdj = (Expander)target;
			exp_TrackerAdj.Collapsed += Exp_TrackerAdj_Collapsed;
			break;
		case 114:
			btn_TrackerZoomInUp = (Button)target;
			btn_TrackerZoomInUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 115:
			btn_TrackerZoomOutUp = (Button)target;
			btn_TrackerZoomOutUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 116:
			btn_TrackerZoomOutDown = (Button)target;
			btn_TrackerZoomOutDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 117:
			btn_TrackerZoomDown = (Button)target;
			btn_TrackerZoomDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 118:
			btn_TrackerZoomLeft = (Button)target;
			btn_TrackerZoomLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 119:
			btn_TrackerZoomLeft1 = (Button)target;
			btn_TrackerZoomLeft1.Click += Btn_TrackerMoveUp_Click;
			break;
		case 120:
			btn_TrackerZoomRight = (Button)target;
			btn_TrackerZoomRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 121:
			btn_TrackerZoomInRight = (Button)target;
			btn_TrackerZoomInRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 122:
			btn_TrackerMoveUp = (Button)target;
			btn_TrackerMoveUp.Click += Btn_TrackerMoveUp_Click;
			break;
		case 123:
			btn_TrackerMoveDown = (Button)target;
			btn_TrackerMoveDown.Click += Btn_TrackerMoveUp_Click;
			break;
		case 124:
			btn_TestNextImg_Copy4 = (Button)target;
			btn_TestNextImg_Copy4.Click += Btn_TrackerMoveUp_Click;
			break;
		case 125:
			btn_TrackerMoveRight = (Button)target;
			btn_TrackerMoveRight.Click += Btn_TrackerMoveUp_Click;
			break;
		case 126:
			btn_TestNextImg2_Copy = (Button)target;
			break;
		case 127:
			grd_TrackerAdj = (Grid)target;
			break;
		case 128:
			btn_TrackerSetSameSize = (Button)target;
			btn_TrackerSetSameSize.Click += Btn_TrackerMoveUp_Click;
			break;
		case 129:
			btn_TrackerAlignTop = (Button)target;
			btn_TrackerAlignTop.Click += Btn_TrackerMoveUp_Click;
			break;
		case 130:
			TrackerAlignBottom = (Button)target;
			TrackerAlignBottom.Click += Btn_TrackerMoveUp_Click;
			break;
		case 131:
			TrackerAlignLeft = (Button)target;
			TrackerAlignLeft.Click += Btn_TrackerMoveUp_Click;
			break;
		case 132:
			TrackerAlignRight = (Button)target;
			TrackerAlignRight.Click += Btn_TrackerMoveUp_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
