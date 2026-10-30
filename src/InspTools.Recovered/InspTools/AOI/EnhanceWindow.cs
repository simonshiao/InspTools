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
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using OpenCvSharp;

namespace InspTools.AOI;

public class EnhanceWindow : System.Windows.Window, IComponentConnector
{
	private bool THD_Preview;

	private bool ShowMessage = true;

	private byte[] CurrentColor = new byte[13];

	private OpenCvSharp.Rect LocateRect;

	private bool UpdateUI;

	private Mat ShowBitmap = new Mat();

	private int LanguageType = 1;

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private int TrackIndex;

	private int FunctionIndex;

	private CVInspection inspection;

	private RadioButton[] FunctionSelect;

	private Grid[] FunctionGrid;

	private TextBox[] CustomInput;

	private const int MaxChannelSelect = 7;

	private const int MaxEnhanceTool = 11;

	private ComboBox[] ChannelSelect;

	private Button[] ChannelBtn;

	private string[] ChannelStr;

	internal Canvas cnv_Image;

	internal Border bor_LocateRect;

	internal Label Label_ResultError;

	internal Label Label_Result;

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

	internal TextBox txt_AllResult;

	internal Grid grd_CB;

	internal ComboBox cb_Brightness;

	internal ComboBox cb_Contrast;

	internal Button btn_Test_BC;

	internal ComboBox cb_ChannelBC;

	internal ComboBox cb_Gamma;

	internal RadioButton rad_BC;

	internal Grid grd_SS;

	internal ComboBox cb_SS;

	internal Button btn_Test_SS;

	internal ComboBox cb_ChannelSS;

	internal RadioButton rad_SS;

	internal Grid grd_Edge;

	internal ComboBox cb_Edge;

	internal Button btn_Test_Edge;

	internal ComboBox cb_ChannelEdge;

	internal TextBox txt_CannyThd1;

	internal TextBox txt_CannyThd2;

	internal RadioButton rad_Edge;

	internal Grid grd_ColorMix;

	internal Button btn_Test_ColorMix;

	internal ComboBox cb_ChannelColorMix1;

	internal ComboBox cb_ChannelColorMix2;

	internal RadioButton rad_UseCombine;

	internal RadioButton rad_UseSubtract;

	internal RadioButton rad_ColorMix;

	internal Grid grd_Custom;

	internal Button btn_Test_Custom;

	internal ComboBox cb_ChannelCustom;

	internal TextBox txt_c1;

	internal TextBox txt_c2;

	internal TextBox txt_c3;

	internal TextBox txt_c4;

	internal TextBox txt_c5;

	internal TextBox txt_c6;

	internal TextBox txt_c7;

	internal TextBox txt_c8;

	internal TextBox txt_c9;

	internal RadioButton rad_Custom;

	internal Grid grd_None;

	internal RadioButton rad_None;

	internal Grid grd_LAB;

	internal ComboBox cb_ChannelLAB;

	internal Button btn_Test_LAB;

	internal RadioButton rad_LAB;

	internal Grid grd_LUV;

	internal ComboBox cb_ChannelLUV;

	internal Button btn_Test_LAB1;

	internal RadioButton rad_LUV;

	internal Grid grd_BlockAvg;

	internal Button btn_Test_BlockAvg;

	internal ComboBox cb_ChannelBlockAvg;

	internal ComboBox cb_ChannelBlockAvgFun;

	internal NumericUpDown nu_BlockAvgThd;

	internal ComboBox cb_DoBlockMorph;

	internal ComboBox cb_DoBlockMorphCount;

	internal RadioButton rad_BlockAvg;

	internal Grid grd_BM;

	internal ComboBox cb_ChannelBM;

	internal NumericUpDown nu_BrightnessValue;

	internal CheckBox chk_EnableVMirror;

	internal CheckBox chk_EnableHMirror;

	internal Button btn_Test_BM;

	internal NumericUpDown nu_ContrastValue;

	internal RadioButton rad_BM;

	internal Grid grd_Binary;

	internal CheckBox chk_EnableVMirror1;

	internal CheckBox chk_EnableHMirror1;

	internal Button btn_Test_Binary;

	internal RangeSlider rs_Thd_R;

	internal RangeSlider rs_Thd_G;

	internal RangeSlider rs_Thd_B;

	internal RadioButton rad_Binary;

	internal Button btn_CustomImage1;

	internal Button btn_CustomImage2;

	internal Button btn_CustomImage3;

	internal Button btn_CustomImage4;

	internal Button btn_CustomImage5;

	internal Button btn_CustomImage6;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public EnhanceWindow()
	{
		InitializeComponent();
		THD_Preview = false;
		Label_ResultError.Visibility = Visibility.Hidden;
		FunctionSelect = new RadioButton[11]
		{
			rad_None, rad_BC, rad_SS, rad_Edge, rad_ColorMix, rad_Custom, rad_LAB, rad_LUV, rad_BlockAvg, rad_BM,
			rad_Binary
		};
		FunctionGrid = new Grid[11]
		{
			grd_None, grd_CB, grd_SS, grd_Edge, grd_ColorMix, grd_Custom, grd_LAB, grd_LUV, grd_BlockAvg, grd_BM,
			grd_Binary
		};
		CustomInput = new TextBox[9] { txt_c1, txt_c2, txt_c3, txt_c4, txt_c5, txt_c6, txt_c7, txt_c8, txt_c9 };
		ChannelSelect = new ComboBox[7] { cb_ChannelBC, cb_ChannelSS, cb_ChannelEdge, cb_ChannelColorMix1, cb_ChannelColorMix2, cb_ChannelCustom, cb_ChannelBlockAvg };
		ChannelStr = new string[12]
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
			"C3",
			"C4",
			"C5"
		};
		ChannelBtn = new Button[6] { btn_CustomImage1, btn_CustomImage2, btn_CustomImage3, btn_CustomImage4, btn_CustomImage5, btn_CustomImage6 };
		Label_Result.Visibility = Visibility.Hidden;
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

	public void SetInspection(int index, CVInspection station)
	{
		TrackIndex = index;
		inspection = station;
		for (int i = 0; i < 6; i++)
		{
			if (inspection.paramFile.EnhanceParam[i].FunctionIndex == 0)
			{
				ChannelBtn[i].Background = Brushes.White;
			}
			else
			{
				ChannelBtn[i].Background = Brushes.Gold;
			}
		}
	}

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
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

	private void Window_Closed(object sender, EventArgs e)
	{
		WriteUIValueToParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImgSrc_Click(object sender, RoutedEventArgs e)
	{
		string uid = ((Button)sender).Uid;
		Mat image;
		if (uid != null)
		{
			int length = uid.Length;
			if (length == 1)
			{
				switch (uid[0])
				{
				case '0':
					break;
				case '1':
					goto IL_0117;
				case '2':
					goto IL_012e;
				case '3':
					goto IL_0145;
				case '4':
					goto IL_015c;
				case '5':
					goto IL_0173;
				case '6':
					goto IL_018a;
				case '7':
					goto IL_01a1;
				case '8':
					goto IL_01b7;
				case '9':
					goto IL_01cb;
				default:
					goto IL_022f;
				}
				image = inspection.buffers.LocatedSrc;
				goto IL_0242;
			}
			if (length == 2)
			{
				switch (uid[1])
				{
				case '0':
					break;
				case '1':
					goto IL_00ad;
				case '2':
					goto IL_00c9;
				case '3':
					goto IL_00e5;
				default:
					goto IL_022f;
				}
				if (uid == "10")
				{
					image = inspection.buffers.GetLocatedCustomMat(2);
					goto IL_0242;
				}
			}
		}
		goto IL_022f;
		IL_00c9:
		if (!(uid == "12"))
		{
			goto IL_022f;
		}
		image = inspection.buffers.GetLocatedCustomMat(4);
		goto IL_0242;
		IL_00ad:
		if (!(uid == "11"))
		{
			goto IL_022f;
		}
		image = inspection.buffers.GetLocatedCustomMat(3);
		goto IL_0242;
		IL_0242:
		THD_Preview = false;
		ImageSource imageSource = inspection.buffers.Mat2ImageSource(image);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
		return;
		IL_0117:
		image = inspection.buffers.GetLocatedBGR(0);
		goto IL_0242;
		IL_012e:
		image = inspection.buffers.GetLocatedBGR(1);
		goto IL_0242;
		IL_0145:
		image = inspection.buffers.GetLocatedBGR(2);
		goto IL_0242;
		IL_015c:
		image = inspection.buffers.GetLocatedHSV(0);
		goto IL_0242;
		IL_0173:
		image = inspection.buffers.GetLocatedHSV(1);
		goto IL_0242;
		IL_018a:
		image = inspection.buffers.GetLocatedHSV(2);
		goto IL_0242;
		IL_01a1:
		image = inspection.buffers.LocateGray;
		goto IL_0242;
		IL_01b7:
		image = inspection.buffers.GetLocatedCustomMat(0);
		goto IL_0242;
		IL_01cb:
		image = inspection.buffers.GetLocatedCustomMat(1);
		goto IL_0242;
		IL_00e5:
		if (!(uid == "13"))
		{
			goto IL_022f;
		}
		image = inspection.buffers.GetLocatedCustomMat(5);
		goto IL_0242;
		IL_022f:
		image = (image = inspection.buffers.LocatedSrc);
		goto IL_0242;
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
			THD_Preview = true;
		}
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

	private void Bor_SearchRect1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Btn_ReDraw_Click(object sender, RoutedEventArgs e)
	{
	}

	private void UpdateBinary()
	{
		if (UpdateUI && rad_Binary.IsChecked != false)
		{
			Btn_Test_Click(null, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		try
		{
			UpdateUI = false;
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = inspection.paramFile.EnhanceParam[TrackIndex];
			Button button = btn_CustomImage1;
			Button button2 = btn_CustomImage2;
			Brush brush = (btn_CustomImage3.BorderBrush = Brushes.Gray);
			Brush borderBrush = (button2.BorderBrush = brush);
			button.BorderBrush = borderBrush;
			Button button3 = btn_CustomImage4;
			Button button4 = btn_CustomImage5;
			brush = (btn_CustomImage6.BorderBrush = Brushes.Gray);
			borderBrush = (button4.BorderBrush = brush);
			button3.BorderBrush = borderBrush;
			switch (TrackIndex)
			{
			case 0:
				btn_CustomImage1.BorderBrush = Brushes.LimeGreen;
				break;
			case 1:
				btn_CustomImage2.BorderBrush = Brushes.LimeGreen;
				break;
			case 2:
				btn_CustomImage3.BorderBrush = Brushes.LimeGreen;
				break;
			case 3:
				btn_CustomImage4.BorderBrush = Brushes.LimeGreen;
				break;
			case 4:
				btn_CustomImage5.BorderBrush = Brushes.LimeGreen;
				break;
			case 5:
				btn_CustomImage6.BorderBrush = Brushes.LimeGreen;
				break;
			default:
				btn_CustomImage1.BorderBrush = Brushes.LimeGreen;
				break;
			}
			FunctionSelect[iNSP_ENHANCE_PARAM.FunctionIndex].IsChecked = true;
			FunctionIndex = iNSP_ENHANCE_PARAM.FunctionIndex;
			for (int i = 0; i < 11; i++)
			{
				if (iNSP_ENHANCE_PARAM.FunctionIndex == i)
				{
					FunctionGrid[i].IsEnabled = true;
				}
				else
				{
					FunctionGrid[i].IsEnabled = false;
				}
			}
			cb_ChannelBC.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelSS.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelEdge.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelColorMix1.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelCustom.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelLAB.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelLUV.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelBlockAvg.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_ChannelBM.SelectedIndex = iNSP_ENHANCE_PARAM.ChannelIndex;
			cb_Brightness.SelectedIndex = iNSP_ENHANCE_PARAM.BrightnessIndex;
			cb_Contrast.SelectedIndex = iNSP_ENHANCE_PARAM.ContrastIndex;
			cb_Gamma.SelectedIndex = iNSP_ENHANCE_PARAM.GammaIndex;
			cb_ChannelBlockAvgFun.SelectedIndex = iNSP_ENHANCE_PARAM.BlockAvgFun;
			cb_SS.SelectedIndex = iNSP_ENHANCE_PARAM.SSIndex;
			cb_Edge.SelectedIndex = iNSP_ENHANCE_PARAM.EdgeIndex;
			if (cb_Edge.SelectedIndex == 9)
			{
				txt_CannyThd1.Visibility = Visibility.Visible;
				txt_CannyThd2.Visibility = Visibility.Visible;
			}
			else
			{
				txt_CannyThd1.Visibility = Visibility.Hidden;
				txt_CannyThd2.Visibility = Visibility.Hidden;
			}
			if (iNSP_ENHANCE_PARAM.UseSubtract)
			{
				rad_UseSubtract.IsChecked = true;
			}
			else
			{
				rad_UseCombine.IsChecked = true;
			}
			txt_CannyThd1.Text = iNSP_ENHANCE_PARAM.CannyThd1.ToString();
			txt_CannyThd2.Text = iNSP_ENHANCE_PARAM.CannyThd2.ToString();
			cb_ChannelColorMix2.SelectedIndex = iNSP_ENHANCE_PARAM.MixedIndex;
			txt_c1.Text = iNSP_ENHANCE_PARAM.Custom1.ToString();
			txt_c2.Text = iNSP_ENHANCE_PARAM.Custom2.ToString();
			txt_c3.Text = iNSP_ENHANCE_PARAM.Custom3.ToString();
			txt_c4.Text = iNSP_ENHANCE_PARAM.Custom4.ToString();
			txt_c5.Text = iNSP_ENHANCE_PARAM.Custom5.ToString();
			txt_c6.Text = iNSP_ENHANCE_PARAM.Custom6.ToString();
			txt_c7.Text = iNSP_ENHANCE_PARAM.Custom7.ToString();
			txt_c8.Text = iNSP_ENHANCE_PARAM.Custom8.ToString();
			txt_c9.Text = iNSP_ENHANCE_PARAM.Custom9.ToString();
			nu_BlockAvgThd.Value = iNSP_ENHANCE_PARAM.BlockAvgThd;
			nu_BrightnessValue.Value = iNSP_ENHANCE_PARAM.BrightnessValue;
			nu_ContrastValue.Value = iNSP_ENHANCE_PARAM.ContrastValue;
			cb_DoBlockMorph.SelectedIndex = iNSP_ENHANCE_PARAM.MorphType;
			cb_DoBlockMorphCount.SelectedIndex = iNSP_ENHANCE_PARAM.MorphCount;
			rs_Thd_R.LowerValue = iNSP_ENHANCE_PARAM.Thd_R_Min;
			rs_Thd_R.UpperValue = iNSP_ENHANCE_PARAM.Thd_R_Max;
			rs_Thd_G.LowerValue = iNSP_ENHANCE_PARAM.Thd_G_Min;
			rs_Thd_G.UpperValue = iNSP_ENHANCE_PARAM.Thd_G_Max;
			rs_Thd_B.LowerValue = iNSP_ENHANCE_PARAM.Thd_B_Min;
			rs_Thd_B.UpperValue = iNSP_ENHANCE_PARAM.Thd_B_Max;
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "EW01-EnhanceWindow-CopyValueToUI()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
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
		for (int i = 0; i < 9; i++)
		{
			if (!IsNumeric(CustomInput[i].Text))
			{
				CustomInput[i].Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				CustomInput[i].Background = Brushes.White;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI || !inspection.buffers.LocatedReady)
		{
			return;
		}
		WriteUIValueToParam();
		try
		{
			for (int i = 0; i < 6; i++)
			{
				inspection.enhanceTool.Inspect(i);
			}
			Mat image = TrackIndex switch
			{
				0 => inspection.buffers.GetLocatedCustomMat(0), 
				1 => inspection.buffers.GetLocatedCustomMat(1), 
				2 => inspection.buffers.GetLocatedCustomMat(2), 
				3 => inspection.buffers.GetLocatedCustomMat(3), 
				4 => inspection.buffers.GetLocatedCustomMat(4), 
				5 => inspection.buffers.GetLocatedCustomMat(5), 
				_ => image = inspection.buffers.LocatedSrc, 
			};
			THD_Preview = false;
			ImageSource imageSource = inspection.buffers.Mat2ImageSource(image);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
		}
		catch (Exception ex)
		{
			string text = "EW01-EnhanceWindow-Btn_Test_Click()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Rad_BC_Click(object sender, RoutedEventArgs e)
	{
		int num = (FunctionIndex = Convert.ToInt32(((RadioButton)sender).Uid));
		for (int i = 0; i < 11; i++)
		{
			if (num == i)
			{
				FunctionGrid[i].IsEnabled = true;
			}
			else
			{
				FunctionGrid[i].IsEnabled = false;
			}
		}
		if (FunctionIndex == 0)
		{
			ChannelBtn[TrackIndex].Background = Brushes.White;
		}
		else
		{
			ChannelBtn[TrackIndex].Background = Brushes.Gold;
		}
	}

	private void Btn_CustomImage1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		WriteUIValueToParam();
		TrackIndex = num;
		Button button = btn_CustomImage1;
		Button button2 = btn_CustomImage2;
		Brush brush = (btn_CustomImage3.BorderBrush = Brushes.Gray);
		Brush borderBrush = (button2.BorderBrush = brush);
		button.BorderBrush = borderBrush;
		Button button3 = btn_CustomImage4;
		Button button4 = btn_CustomImage5;
		brush = (btn_CustomImage6.BorderBrush = Brushes.Gray);
		borderBrush = (button4.BorderBrush = brush);
		button3.BorderBrush = borderBrush;
		switch (num)
		{
		case 0:
			btn_CustomImage1.BorderBrush = Brushes.LimeGreen;
			break;
		case 1:
			btn_CustomImage2.BorderBrush = Brushes.LimeGreen;
			break;
		case 2:
			btn_CustomImage3.BorderBrush = Brushes.LimeGreen;
			break;
		case 3:
			btn_CustomImage4.BorderBrush = Brushes.LimeGreen;
			break;
		case 4:
			btn_CustomImage5.BorderBrush = Brushes.LimeGreen;
			break;
		case 5:
			btn_CustomImage6.BorderBrush = Brushes.LimeGreen;
			break;
		default:
			btn_CustomImage1.BorderBrush = Brushes.LimeGreen;
			break;
		}
		UpdateComboBox(TrackIndex);
		CopyValueToUI();
		Mat locatedSrc = inspection.buffers.LocatedSrc;
		ImageSource imageSource = inspection.buffers.MatToBitmapImage(locatedSrc);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
		Btn_Test_Click(sender, null);
	}

	private void UpdateComboBox(int index)
	{
		for (int i = 0; i < 7; i++)
		{
			ChannelSelect[i].Items.Clear();
		}
		int num = 7;
		num = index switch
		{
			0 => 7, 
			1 => 8, 
			2 => 9, 
			3 => 10, 
			4 => 11, 
			5 => 12, 
			_ => 7, 
		};
		for (int j = 0; j < 7; j++)
		{
			for (int k = 0; k < num; k++)
			{
				ChannelSelect[j].Items.Add(ChannelStr[k]);
			}
		}
	}

	private void Cb_Edge_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			if (cb_Edge.SelectedIndex == 9)
			{
				txt_CannyThd1.Visibility = Visibility.Visible;
				txt_CannyThd2.Visibility = Visibility.Visible;
			}
			else
			{
				txt_CannyThd1.Visibility = Visibility.Hidden;
				txt_CannyThd2.Visibility = Visibility.Hidden;
			}
			Btn_Test_Click(this, null);
		}
	}

	private void Nu_BlockAvgThd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (UpdateUI)
		{
			Btn_Test_Click(sender, null);
		}
	}

	private void Nu_Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (UpdateUI)
		{
			Btn_Test_Click(sender, null);
		}
	}

	private void Cb_THD1_DEStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			Btn_Test_Click(sender, null);
		}
	}

	private void UpdateBinary(object sender, DragDeltaEventArgs e)
	{
		UpdateBinary();
	}

	private void UpdateBinary(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		UpdateBinary();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		try
		{
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = inspection.paramFile.EnhanceParam[TrackIndex];
			iNSP_ENHANCE_PARAM.ValueChangeFlag = true;
			iNSP_ENHANCE_PARAM.FunctionIndex = FunctionIndex;
			if (FunctionIndex == 1)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelBC.SelectedIndex;
			}
			else if (FunctionIndex == 2)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelSS.SelectedIndex;
			}
			else if (FunctionIndex == 3)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelEdge.SelectedIndex;
			}
			else if (FunctionIndex == 4)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelColorMix1.SelectedIndex;
			}
			else if (FunctionIndex == 5)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelCustom.SelectedIndex;
			}
			else if (FunctionIndex == 6)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelLAB.SelectedIndex;
			}
			else if (FunctionIndex == 7)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelLUV.SelectedIndex;
			}
			else if (FunctionIndex == 8)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelBlockAvg.SelectedIndex;
			}
			else if (FunctionIndex == 9)
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = cb_ChannelBM.SelectedIndex;
			}
			else
			{
				iNSP_ENHANCE_PARAM.ChannelIndex = 1;
			}
			iNSP_ENHANCE_PARAM.BrightnessIndex = cb_Brightness.SelectedIndex;
			iNSP_ENHANCE_PARAM.ContrastIndex = cb_Contrast.SelectedIndex;
			iNSP_ENHANCE_PARAM.GammaIndex = cb_Gamma.SelectedIndex;
			iNSP_ENHANCE_PARAM.CannyThd1 = Convert.ToInt32(txt_CannyThd1.Text);
			iNSP_ENHANCE_PARAM.CannyThd2 = Convert.ToInt32(txt_CannyThd2.Text);
			iNSP_ENHANCE_PARAM.SSIndex = cb_SS.SelectedIndex;
			iNSP_ENHANCE_PARAM.EdgeIndex = cb_Edge.SelectedIndex;
			iNSP_ENHANCE_PARAM.MixedIndex = cb_ChannelColorMix2.SelectedIndex;
			if (rad_UseSubtract.IsChecked == true)
			{
				iNSP_ENHANCE_PARAM.UseSubtract = true;
			}
			else
			{
				iNSP_ENHANCE_PARAM.UseSubtract = false;
			}
			iNSP_ENHANCE_PARAM.Custom1 = Convert.ToDouble(txt_c1.Text);
			iNSP_ENHANCE_PARAM.Custom2 = Convert.ToDouble(txt_c2.Text);
			iNSP_ENHANCE_PARAM.Custom3 = Convert.ToDouble(txt_c3.Text);
			iNSP_ENHANCE_PARAM.Custom4 = Convert.ToDouble(txt_c4.Text);
			iNSP_ENHANCE_PARAM.Custom5 = Convert.ToDouble(txt_c5.Text);
			iNSP_ENHANCE_PARAM.Custom6 = Convert.ToDouble(txt_c6.Text);
			iNSP_ENHANCE_PARAM.Custom7 = Convert.ToDouble(txt_c7.Text);
			iNSP_ENHANCE_PARAM.Custom8 = Convert.ToDouble(txt_c8.Text);
			iNSP_ENHANCE_PARAM.Custom9 = Convert.ToDouble(txt_c9.Text);
			iNSP_ENHANCE_PARAM.BlockAvgFun = cb_ChannelBlockAvgFun.SelectedIndex;
			iNSP_ENHANCE_PARAM.BlockAvgThd = (int)nu_BlockAvgThd.Value.Value;
			iNSP_ENHANCE_PARAM.BrightnessValue = nu_BrightnessValue.Value.Value;
			iNSP_ENHANCE_PARAM.ContrastValue = nu_ContrastValue.Value.Value;
			iNSP_ENHANCE_PARAM.MorphType = cb_DoBlockMorph.SelectedIndex;
			iNSP_ENHANCE_PARAM.MorphCount = cb_DoBlockMorphCount.SelectedIndex;
			iNSP_ENHANCE_PARAM.Thd_R_Min = (int)rs_Thd_R.LowerValue;
			iNSP_ENHANCE_PARAM.Thd_R_Max = (int)rs_Thd_R.UpperValue;
			iNSP_ENHANCE_PARAM.Thd_G_Min = (int)rs_Thd_G.LowerValue;
			iNSP_ENHANCE_PARAM.Thd_G_Max = (int)rs_Thd_G.UpperValue;
			iNSP_ENHANCE_PARAM.Thd_B_Min = (int)rs_Thd_B.LowerValue;
			iNSP_ENHANCE_PARAM.Thd_B_Max = (int)rs_Thd_B.UpperValue;
			inspection.paramFile.EnhanceParam[TrackIndex] = iNSP_ENHANCE_PARAM;
		}
		catch (Exception ex)
		{
			string text = "EW01-EnhanceWindow-WriteUIValueToParam()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void DrawBackGround()
	{
		ShowBitmap = inspection.buffers.LocatedSrc.Clone();
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/enhancewindow.xaml", UriKind.Relative);
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
			((EnhanceWindow)target).Closed += Window_Closed;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 3:
			bor_LocateRect = (Border)target;
			break;
		case 4:
			Label_ResultError = (Label)target;
			break;
		case 5:
			Label_Result = (Label)target;
			break;
		case 6:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 7:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 8:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 9:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 10:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 11:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 12:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 13:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 14:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 15:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 16:
			btn_ImageNum = (Button)target;
			break;
		case 17:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 18:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 19:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 20:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 21:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 22:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 23:
			lab_RValue = (Label)target;
			break;
		case 24:
			lab_GValue = (Label)target;
			break;
		case 25:
			lab_BValue = (Label)target;
			break;
		case 26:
			lab_HValue = (Label)target;
			break;
		case 27:
			lab_SValue = (Label)target;
			break;
		case 28:
			lab_VValue = (Label)target;
			break;
		case 29:
			lab_GrayValue = (Label)target;
			break;
		case 30:
			lab_XValue = (Label)target;
			break;
		case 31:
			lab_YValue = (Label)target;
			break;
		case 32:
			lab_C1Value = (Label)target;
			break;
		case 33:
			lab_C2Value = (Label)target;
			break;
		case 34:
			lab_C3Value = (Label)target;
			break;
		case 35:
			lab_C4Value = (Label)target;
			break;
		case 36:
			lab_C5Value = (Label)target;
			break;
		case 37:
			lab_C6Value = (Label)target;
			break;
		case 38:
			txt_AllResult = (TextBox)target;
			break;
		case 39:
			grd_CB = (Grid)target;
			break;
		case 40:
			cb_Brightness = (ComboBox)target;
			break;
		case 41:
			cb_Contrast = (ComboBox)target;
			break;
		case 42:
			btn_Test_BC = (Button)target;
			btn_Test_BC.Click += Btn_Test_Click;
			break;
		case 43:
			cb_ChannelBC = (ComboBox)target;
			break;
		case 44:
			cb_Gamma = (ComboBox)target;
			break;
		case 45:
			rad_BC = (RadioButton)target;
			rad_BC.Click += Rad_BC_Click;
			break;
		case 46:
			grd_SS = (Grid)target;
			break;
		case 47:
			cb_SS = (ComboBox)target;
			break;
		case 48:
			btn_Test_SS = (Button)target;
			btn_Test_SS.Click += Btn_Test_Click;
			break;
		case 49:
			cb_ChannelSS = (ComboBox)target;
			break;
		case 50:
			rad_SS = (RadioButton)target;
			rad_SS.Click += Rad_BC_Click;
			break;
		case 51:
			grd_Edge = (Grid)target;
			break;
		case 52:
			cb_Edge = (ComboBox)target;
			cb_Edge.SelectionChanged += Cb_Edge_SelectionChanged;
			break;
		case 53:
			btn_Test_Edge = (Button)target;
			btn_Test_Edge.Click += Btn_Test_Click;
			break;
		case 54:
			cb_ChannelEdge = (ComboBox)target;
			break;
		case 55:
			txt_CannyThd1 = (TextBox)target;
			break;
		case 56:
			txt_CannyThd2 = (TextBox)target;
			break;
		case 57:
			rad_Edge = (RadioButton)target;
			rad_Edge.Click += Rad_BC_Click;
			break;
		case 58:
			grd_ColorMix = (Grid)target;
			break;
		case 59:
			btn_Test_ColorMix = (Button)target;
			btn_Test_ColorMix.Click += Btn_Test_Click;
			break;
		case 60:
			cb_ChannelColorMix1 = (ComboBox)target;
			break;
		case 61:
			cb_ChannelColorMix2 = (ComboBox)target;
			break;
		case 62:
			rad_UseCombine = (RadioButton)target;
			break;
		case 63:
			rad_UseSubtract = (RadioButton)target;
			break;
		case 64:
			rad_ColorMix = (RadioButton)target;
			rad_ColorMix.Click += Rad_BC_Click;
			break;
		case 65:
			grd_Custom = (Grid)target;
			break;
		case 66:
			btn_Test_Custom = (Button)target;
			btn_Test_Custom.Click += Btn_Test_Click;
			break;
		case 67:
			cb_ChannelCustom = (ComboBox)target;
			break;
		case 68:
			txt_c1 = (TextBox)target;
			break;
		case 69:
			txt_c2 = (TextBox)target;
			break;
		case 70:
			txt_c3 = (TextBox)target;
			break;
		case 71:
			txt_c4 = (TextBox)target;
			break;
		case 72:
			txt_c5 = (TextBox)target;
			break;
		case 73:
			txt_c6 = (TextBox)target;
			break;
		case 74:
			txt_c7 = (TextBox)target;
			break;
		case 75:
			txt_c8 = (TextBox)target;
			break;
		case 76:
			txt_c9 = (TextBox)target;
			break;
		case 77:
			rad_Custom = (RadioButton)target;
			rad_Custom.Click += Rad_BC_Click;
			break;
		case 78:
			grd_None = (Grid)target;
			break;
		case 79:
			rad_None = (RadioButton)target;
			rad_None.Click += Rad_BC_Click;
			break;
		case 80:
			grd_LAB = (Grid)target;
			break;
		case 81:
			cb_ChannelLAB = (ComboBox)target;
			break;
		case 82:
			btn_Test_LAB = (Button)target;
			btn_Test_LAB.Click += Btn_Test_Click;
			break;
		case 83:
			rad_LAB = (RadioButton)target;
			rad_LAB.Click += Rad_BC_Click;
			break;
		case 84:
			grd_LUV = (Grid)target;
			break;
		case 85:
			cb_ChannelLUV = (ComboBox)target;
			break;
		case 86:
			btn_Test_LAB1 = (Button)target;
			btn_Test_LAB1.Click += Btn_Test_Click;
			break;
		case 87:
			rad_LUV = (RadioButton)target;
			rad_LUV.Click += Rad_BC_Click;
			break;
		case 88:
			grd_BlockAvg = (Grid)target;
			break;
		case 89:
			btn_Test_BlockAvg = (Button)target;
			btn_Test_BlockAvg.Click += Btn_Test_Click;
			break;
		case 90:
			cb_ChannelBlockAvg = (ComboBox)target;
			break;
		case 91:
			cb_ChannelBlockAvgFun = (ComboBox)target;
			break;
		case 92:
			nu_BlockAvgThd = (NumericUpDown)target;
			nu_BlockAvgThd.ValueChanged += Nu_BlockAvgThd_ValueChanged;
			break;
		case 93:
			cb_DoBlockMorph = (ComboBox)target;
			cb_DoBlockMorph.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 94:
			cb_DoBlockMorphCount = (ComboBox)target;
			cb_DoBlockMorphCount.SelectionChanged += Cb_THD1_DEStart_SelectionChanged;
			break;
		case 95:
			rad_BlockAvg = (RadioButton)target;
			rad_BlockAvg.Click += Rad_BC_Click;
			break;
		case 96:
			grd_BM = (Grid)target;
			break;
		case 97:
			cb_ChannelBM = (ComboBox)target;
			break;
		case 98:
			nu_BrightnessValue = (NumericUpDown)target;
			nu_BrightnessValue.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 99:
			chk_EnableVMirror = (CheckBox)target;
			break;
		case 100:
			chk_EnableHMirror = (CheckBox)target;
			break;
		case 101:
			btn_Test_BM = (Button)target;
			btn_Test_BM.Click += Btn_Test_Click;
			break;
		case 102:
			nu_ContrastValue = (NumericUpDown)target;
			nu_ContrastValue.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 103:
			rad_BM = (RadioButton)target;
			rad_BM.Click += Rad_BC_Click;
			break;
		case 104:
			grd_Binary = (Grid)target;
			break;
		case 105:
			chk_EnableVMirror1 = (CheckBox)target;
			break;
		case 106:
			chk_EnableHMirror1 = (CheckBox)target;
			break;
		case 107:
			btn_Test_Binary = (Button)target;
			btn_Test_Binary.Click += Btn_Test_Click;
			break;
		case 108:
			rs_Thd_R = (RangeSlider)target;
			rs_Thd_R.CentralThumbDragDelta += UpdateBinary;
			rs_Thd_R.LowerThumbDragDelta += UpdateBinary;
			rs_Thd_R.UpperThumbDragDelta += UpdateBinary;
			rs_Thd_R.ValueChanged += UpdateBinary;
			break;
		case 109:
			rs_Thd_G = (RangeSlider)target;
			rs_Thd_G.CentralThumbDragDelta += UpdateBinary;
			rs_Thd_G.LowerThumbDragDelta += UpdateBinary;
			rs_Thd_G.UpperThumbDragDelta += UpdateBinary;
			rs_Thd_G.ValueChanged += UpdateBinary;
			break;
		case 110:
			rs_Thd_B = (RangeSlider)target;
			rs_Thd_B.CentralThumbDragDelta += UpdateBinary;
			rs_Thd_B.LowerThumbDragDelta += UpdateBinary;
			rs_Thd_B.UpperThumbDragDelta += UpdateBinary;
			rs_Thd_B.ValueChanged += UpdateBinary;
			break;
		case 111:
			rad_Binary = (RadioButton)target;
			rad_Binary.Click += Rad_BC_Click;
			break;
		case 112:
			btn_CustomImage1 = (Button)target;
			btn_CustomImage1.Click += Btn_CustomImage1_Click;
			break;
		case 113:
			btn_CustomImage2 = (Button)target;
			btn_CustomImage2.Click += Btn_CustomImage1_Click;
			break;
		case 114:
			btn_CustomImage3 = (Button)target;
			btn_CustomImage3.Click += Btn_CustomImage1_Click;
			break;
		case 115:
			btn_CustomImage4 = (Button)target;
			btn_CustomImage4.Click += Btn_CustomImage1_Click;
			break;
		case 116:
			btn_CustomImage5 = (Button)target;
			btn_CustomImage5.Click += Btn_CustomImage1_Click;
			break;
		case 117:
			btn_CustomImage6 = (Button)target;
			btn_CustomImage6.Click += Btn_CustomImage1_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
