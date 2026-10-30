using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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

public class LightMonitorWindow : System.Windows.Window, IComponentConnector
{
	private bool THD_Preview;

	private bool ShowMessage = true;

	private byte[] CurrentColor = new byte[10];

	private OpenCvSharp.Rect LocateRect;

	private bool UpdateUI;

	private int CurrentAvgThd = -1;

	private int CorrectionAvgThd = -1;

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

	internal Canvas cnv_Image;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Border bor_LocateRect;

	internal Rectangle rt_ReferenceRect1;

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

	internal TextBox txt_AllResult;

	internal TabControl tab_Fun;

	internal ToggleSwitch ts_TrackerMonitorON;

	internal TextBox txt_MoniterValueError;

	internal ToggleSwitch ts_TrackerCorrectionON;

	internal TextBox txt_CorrectionValueError;

	internal TextBox txt_AlarmRatioR;

	internal Button btn_WriteMoniterValue;

	internal TextBox txt_LightMonitotB;

	internal TextBox txt_LightMonitotR;

	internal TextBox txt_LightMonitotG;

	internal TextBox txt_AlarmRatioG;

	internal TextBox txt_AlarmRatioB;

	internal Button btn_WriteCorrectionValue;

	internal TextBox txt_LightCorrectionB;

	internal TextBox txt_LightCorrectionR;

	internal TextBox txt_LightCorrectionG;

	internal TextBox txt_CorrectionRatioR;

	internal TextBox txt_CorrectionRatioG;

	internal TextBox txt_CorrectionRatioB;

	internal Grid grd_Voltage;

	internal TextBox txt_VoltagePerValueR;

	internal TextBox txt_VoltagePerValueG;

	internal TextBox txt_VoltagePerValueB;

	internal Button btn_Test;

	internal ComboBox cb_TracAutoSize;

	internal Button btn_ChartR;

	internal Button btn_ChartG;

	internal Button btn_ChartB;

	internal TabItem tab_SoftCorr;

	internal ToggleSwitch ts_EnableSoftCorrection;

	internal TextBox txt_SoftCorrectionValueError;

	internal ComboBox cb_TracAutoSize1;

	internal TextBox txt_AllowShift;

	internal TextBox txt_SoftSetupB;

	internal TextBox txt_SoftSetupR;

	internal TextBox txt_SoftSetupG;

	internal Button btn_WriteSoftCorrectionValue;

	internal TextBox txt_SoftShiftB;

	internal TextBox txt_SoftShiftR;

	internal TextBox txt_SoftShiftG;

	internal RadioButton rad_SoftCorrectionON;

	private bool _contentLoaded;

	public LightMonitorWindow()
	{
		InitializeComponent();
		THD_Preview = false;
		rt_ReferenceRect1.Visibility = Visibility.Hidden;
		tab_SoftCorr.Visibility = Visibility.Hidden;
		grd_Voltage.Visibility = Visibility.Hidden;
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
	public void SetInspection(int index, CVInspection station)
	{
		TrackIndex = index;
		inspection = station;
		cb_TracAutoSize.Items.Clear();
		for (int i = 0; i < 20; i++)
		{
			if (!inspection.paramFile.AdvLocateToolON)
			{
				if (i < 1)
				{
					cb_TracAutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
				}
			}
			else if (inspection.paramFile.AdvLocateParam[i].TrackerON)
			{
				cb_TracAutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
			}
			else
			{
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem.IsEnabled = false;
				cb_TracAutoSize.Items.Add(comboBoxItem);
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

	private void SetTracker(OpenCvSharp.Rect rect1)
	{
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth - 1, ImageHeight - 1);
		rect1 &= rect2;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			rect1 = new OpenCvSharp.Rect(0, 0, ImageWidth / 4, ImageHeight / 4);
		}
		bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
		bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
		bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
		bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
	}

	private void GetTracker(ref OpenCvSharp.Rect rect1)
	{
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth - 1, ImageHeight - 1);
			rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
			rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
			rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
			rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
			rect1 &= rect2;
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
		if (uid == null)
		{
			goto IL_016e;
		}
		int length = uid.Length;
		Mat image;
		if (length != 1)
		{
			if (length != 2 || !(uid == "10"))
			{
				goto IL_016e;
			}
			image = inspection.buffers.GetLocatedCustomMat(2);
		}
		else
		{
			switch (uid[0])
			{
			case '0':
				break;
			case '1':
				goto IL_009b;
			case '2':
				goto IL_00b2;
			case '3':
				goto IL_00c9;
			case '4':
				goto IL_00e0;
			case '5':
				goto IL_00f7;
			case '6':
				goto IL_010b;
			case '7':
				goto IL_011f;
			case '8':
				goto IL_0132;
			case '9':
				goto IL_0146;
			default:
				goto IL_016e;
			}
			image = inspection.buffers.LocatedSrc;
		}
		goto IL_0181;
		IL_0146:
		image = inspection.buffers.GetLocatedCustomMat(1);
		goto IL_0181;
		IL_016e:
		image = (image = inspection.buffers.LocatedSrc);
		goto IL_0181;
		IL_0181:
		THD_Preview = false;
		ImageSource imageSource = inspection.buffers.Mat2ImageSource(image);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
		return;
		IL_009b:
		image = inspection.buffers.GetLocatedBGR(0);
		goto IL_0181;
		IL_00b2:
		image = inspection.buffers.GetLocatedBGR(1);
		goto IL_0181;
		IL_00c9:
		image = inspection.buffers.GetLocatedBGR(2);
		goto IL_0181;
		IL_00e0:
		image = inspection.buffers.GetLocatedHSV(0);
		goto IL_0181;
		IL_00f7:
		image = inspection.buffers.GetLocatedHSV(1);
		goto IL_0181;
		IL_010b:
		image = inspection.buffers.GetLocatedHSV(2);
		goto IL_0181;
		IL_011f:
		image = inspection.buffers.LocateGray;
		goto IL_0181;
		IL_0132:
		image = inspection.buffers.GetLocatedCustomMat(0);
		goto IL_0181;
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
				Label label = lab_XValue;
				int num3 = num;
				label.Content = num3.ToString();
				Label label2 = lab_YValue;
				num3 = num2;
				label2.Content = num3.ToString();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		try
		{
			UpdateUI = false;
			rad_SoftCorrectionON.IsChecked = inspection.paramFile.SoftCorrectionON;
			txt_SoftShiftR.Text = inspection.paramFile.SoftShift_R.ToString();
			txt_SoftShiftG.Text = inspection.paramFile.SoftShift_G.ToString();
			txt_SoftShiftB.Text = inspection.paramFile.SoftShift_B.ToString();
			INSP_LIGHTMONITOR_PARAM lightMonitorParam = inspection.paramFile.LightMonitorParam;
			OpenCvSharp.Rect tracker = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (!inspection.paramFile.AdvLocateToolON || lightMonitorParam.TrackerAligmentIndex < 0)
			{
				lightMonitorParam.TrackerAligmentIndex = 0;
				tracker.X = inspection.paramFile.AdvLocateParam[0].L + lightMonitorParam.SearchRectL;
				tracker.Y = inspection.paramFile.AdvLocateParam[0].T + lightMonitorParam.SearchRectT;
				tracker.Width = inspection.paramFile.AdvLocateParam[0].Width - (lightMonitorParam.SearchRectL + lightMonitorParam.SearchRectR);
				tracker.Height = inspection.paramFile.AdvLocateParam[0].Height - (lightMonitorParam.SearchRectT + lightMonitorParam.SearchRectB);
			}
			else if (lightMonitorParam.TrackerAligmentIndex > 0 && lightMonitorParam.TrackerAligmentIndex < 20)
			{
				if (inspection.paramFile.AdvLocateParam[lightMonitorParam.TrackerAligmentIndex].TrackerON)
				{
					tracker.X = inspection.paramFile.AdvLocateParam[lightMonitorParam.TrackerAligmentIndex].L + lightMonitorParam.SearchRectL;
					tracker.Y = inspection.paramFile.AdvLocateParam[lightMonitorParam.TrackerAligmentIndex].T + lightMonitorParam.SearchRectT;
					tracker.Width = inspection.paramFile.AdvLocateParam[lightMonitorParam.TrackerAligmentIndex].Width - (lightMonitorParam.SearchRectL + lightMonitorParam.SearchRectR);
					tracker.Height = inspection.paramFile.AdvLocateParam[lightMonitorParam.TrackerAligmentIndex].Height - (lightMonitorParam.SearchRectT + lightMonitorParam.SearchRectB);
				}
				else
				{
					lightMonitorParam.TrackerAligmentIndex = 0;
					tracker.X = inspection.paramFile.AdvLocateParam[0].L + lightMonitorParam.SearchRectL;
					tracker.Y = inspection.paramFile.AdvLocateParam[0].T + lightMonitorParam.SearchRectT;
					tracker.Width = inspection.paramFile.AdvLocateParam[0].Width - (lightMonitorParam.SearchRectL + lightMonitorParam.SearchRectR);
					tracker.Height = inspection.paramFile.AdvLocateParam[0].Height - (lightMonitorParam.SearchRectT + lightMonitorParam.SearchRectB);
				}
			}
			else
			{
				lightMonitorParam.TrackerAligmentIndex = 0;
				tracker.X = inspection.paramFile.AdvLocateParam[0].L + lightMonitorParam.SearchRectL;
				tracker.Y = inspection.paramFile.AdvLocateParam[0].T + lightMonitorParam.SearchRectT;
				tracker.Width = inspection.paramFile.AdvLocateParam[0].Width - (lightMonitorParam.SearchRectL + lightMonitorParam.SearchRectR);
				tracker.Height = inspection.paramFile.AdvLocateParam[0].Height - (lightMonitorParam.SearchRectT + lightMonitorParam.SearchRectB);
			}
			if (lightMonitorParam.ChannelIndex < COLOR_CHANNEL.EM_RED)
			{
				lightMonitorParam.ChannelIndex = COLOR_CHANNEL.EM_RED;
			}
			SetTracker(tracker);
			cb_TracAutoSize.SelectedIndex = lightMonitorParam.TrackerAligmentIndex;
			txt_LightMonitotR.Text = lightMonitorParam.MonitorValueR.ToString();
			txt_LightMonitotG.Text = lightMonitorParam.MonitorValueG.ToString();
			txt_LightMonitotB.Text = lightMonitorParam.MonitorValueB.ToString();
			if (lightMonitorParam.MonitorValueR == -1 || lightMonitorParam.MonitorValueG == -1 || lightMonitorParam.MonitorValueB == -1)
			{
				txt_MoniterValueError.Visibility = Visibility.Visible;
			}
			else
			{
				txt_MoniterValueError.Visibility = Visibility.Hidden;
			}
			txt_AlarmRatioR.Text = inspection.paramFile.LightMonitorParam.AlarmRatioR.ToString();
			txt_AlarmRatioG.Text = inspection.paramFile.LightMonitorParam.AlarmRatioG.ToString();
			txt_AlarmRatioB.Text = inspection.paramFile.LightMonitorParam.AlarmRatioB.ToString();
			if (lightMonitorParam.CorrectionValueR == -1 || lightMonitorParam.CorrectionValueG == -1 || lightMonitorParam.CorrectionValueB == -1)
			{
				txt_CorrectionValueError.Visibility = Visibility.Visible;
			}
			else
			{
				txt_CorrectionValueError.Visibility = Visibility.Hidden;
			}
			txt_LightCorrectionR.Text = lightMonitorParam.CorrectionValueR.ToString();
			txt_LightCorrectionG.Text = lightMonitorParam.CorrectionValueG.ToString();
			txt_LightCorrectionB.Text = lightMonitorParam.CorrectionValueB.ToString();
			txt_CorrectionRatioR.Text = lightMonitorParam.CorrectionRatioR.ToString();
			txt_CorrectionRatioG.Text = lightMonitorParam.CorrectionRatioG.ToString();
			txt_CorrectionRatioB.Text = lightMonitorParam.CorrectionRatioB.ToString();
			txt_VoltagePerValueR.Text = lightMonitorParam.VoltagePerValueR.ToString();
			txt_VoltagePerValueG.Text = lightMonitorParam.VoltagePerValueG.ToString();
			txt_VoltagePerValueB.Text = lightMonitorParam.VoltagePerValueB.ToString();
			ts_EnableSoftCorrection.IsChecked = lightMonitorParam.EnableSoftCorrection;
			txt_SoftSetupR.Text = inspection.paramFile.SoftCorrSetup_R.ToString();
			txt_SoftSetupG.Text = inspection.paramFile.SoftCorrSetup_G.ToString();
			txt_SoftSetupB.Text = inspection.paramFile.SoftCorrSetup_B.ToString();
			if (inspection.paramFile.SoftCorrSetup_R == -1 || inspection.paramFile.SoftCorrSetup_G == -1 || inspection.paramFile.SoftCorrSetup_B == -1)
			{
				txt_SoftCorrectionValueError.Visibility = Visibility.Visible;
			}
			txt_SoftCorrectionValueError.Visibility = Visibility.Hidden;
			txt_AllowShift.Text = inspection.paramFile.SoftCorrectoinAllowDiff.ToString();
			if (lightMonitorParam.EnableSoftCorrection)
			{
				tab_Fun.SelectedIndex = 1;
			}
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "EW01-LightMonitorWindow-CopyValueToUI()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			UpdateUI = true;
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool IsNumeric(TextBox TextBoxValue)
	{
		try
		{
			double num = Convert.ToDouble(TextBoxValue.Text);
			TextBoxValue.Background = Brushes.White;
			if (num < 0.0 || num > 255.0)
			{
				TextBoxValue.Background = Brushes.Fuchsia;
				MessageBox.Show("請輸入合法數字。(0~255)", "錯誤");
				return false;
			}
			return true;
		}
		catch
		{
			TextBoxValue.Background = Brushes.Fuchsia;
			MessageBox.Show("請輸入合法數字。(0~255)", "錯誤");
			return false;
		}
	}

	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Txt_AlarmBinaryValue_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
	}

	private void Txt_GainPerValue_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
	}

	private void Cb_TracAutoSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			if (cb_TracAutoSize.SelectedIndex != 0)
			{
				SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[cb_TracAutoSize.SelectedIndex].InsideRect);
				rt_ReferenceRect1.Visibility = Visibility.Visible;
			}
			else
			{
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_WriteMoniterValue_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect);
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Light Monitor Area Error, Please Check Inspect Rect.", "Warning");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("光源偵測區域異常，請重新調整檢測框", "警告");
				}
				else
				{
					MessageBox.Show("光源侦测区域异常，请重新调整检测框", "警告");
				}
			}
			if (inspection.paramFile.LightMonitorParam.TrackerAligmentIndex < 0 || inspection.paramFile.LightMonitorParam.TrackerAligmentIndex > 20)
			{
				inspection.paramFile.LightMonitorParam.TrackerAligmentIndex = 0;
			}
			inspection.paramFile.LightMonitorParam.SearchRectL = rect.X - inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].L;
			inspection.paramFile.LightMonitorParam.SearchRectT = rect.Y - inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].T;
			inspection.paramFile.LightMonitorParam.SearchRectR = inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].Width - inspection.paramFile.LightMonitorParam.SearchRectL - rect.Width;
			inspection.paramFile.LightMonitorParam.SearchRectB = inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].Height - inspection.paramFile.LightMonitorParam.SearchRectT - rect.Height;
			inspection.buffers.GetChannelMat(0).Clone();
			inspection.buffers.GetChannelMat(1).Clone();
			inspection.buffers.GetChannelMat(2).Clone();
			int AvgR = 0;
			int AvgG = 0;
			int AvgB = 0;
			inspection.lightmonitorTool.Inspect(ref AvgR, ref AvgG, ref AvgB);
			inspection.paramFile.LightMonitorParam.MonitorValueR = AvgR;
			inspection.paramFile.LightMonitorParam.MonitorValueG = AvgG;
			inspection.paramFile.LightMonitorParam.MonitorValueB = AvgB;
			txt_LightMonitotR.Text = AvgR.ToString();
			txt_LightMonitotG.Text = AvgG.ToString();
			txt_LightMonitotB.Text = AvgB.ToString();
			inspection.paramFile.LightMonitorParam.AlarmRatioR = Convert.ToDouble(txt_AlarmRatioR.Text);
			inspection.paramFile.LightMonitorParam.AlarmRatioG = Convert.ToDouble(txt_AlarmRatioG.Text);
			inspection.paramFile.LightMonitorParam.AlarmRatioB = Convert.ToDouble(txt_AlarmRatioB.Text);
			inspection.paramFile.LightMonitorParam.ValueChangeFlag = true;
			txt_MoniterValueError.Visibility = Visibility.Hidden;
			WriteUIValueToParam();
		}
		catch (Exception ex)
		{
			string text = "LW01-LightMonitorWindow-Btn_WriteMoniterValue_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_WriteCorrectionValue_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect);
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Light Monitor Area Error, Please Check Inspect Rect.", "Warning");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("光源偵測區域異常，請重新調整檢測框", "警告");
				}
				else
				{
					MessageBox.Show("光源侦测区域异常，请重新调整检测框", "警告");
				}
			}
			if (inspection.paramFile.LightMonitorParam.TrackerAligmentIndex < 0 || inspection.paramFile.LightMonitorParam.TrackerAligmentIndex > 20)
			{
				inspection.paramFile.LightMonitorParam.TrackerAligmentIndex = 0;
			}
			inspection.paramFile.LightMonitorParam.SearchRectL = rect.X - inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].L;
			inspection.paramFile.LightMonitorParam.SearchRectT = rect.Y - inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].T;
			inspection.paramFile.LightMonitorParam.SearchRectR = inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].Width - inspection.paramFile.LightMonitorParam.SearchRectL - rect.Width;
			inspection.paramFile.LightMonitorParam.SearchRectB = inspection.paramFile.AdvLocateParam[inspection.paramFile.LightMonitorParam.TrackerAligmentIndex].Height - inspection.paramFile.LightMonitorParam.SearchRectT - rect.Height;
			inspection.buffers.GetChannelMat(0).Clone();
			inspection.buffers.GetChannelMat(1).Clone();
			inspection.buffers.GetChannelMat(2).Clone();
			int AvgR = 0;
			int AvgG = 0;
			int AvgB = 0;
			inspection.lightmonitorTool.Inspect(ref AvgR, ref AvgG, ref AvgB);
			inspection.paramFile.LightMonitorParam.CorrectionValueR = AvgR;
			inspection.paramFile.LightMonitorParam.CorrectionValueG = AvgG;
			inspection.paramFile.LightMonitorParam.CorrectionValueB = AvgB;
			txt_LightCorrectionR.Text = AvgR.ToString();
			txt_LightCorrectionG.Text = AvgG.ToString();
			txt_LightCorrectionB.Text = AvgB.ToString();
			inspection.paramFile.LightMonitorParam.CorrectionRatioR = Convert.ToDouble(txt_CorrectionRatioR.Text);
			inspection.paramFile.LightMonitorParam.CorrectionRatioG = Convert.ToDouble(txt_CorrectionRatioG.Text);
			inspection.paramFile.LightMonitorParam.CorrectionRatioB = Convert.ToDouble(txt_CorrectionRatioB.Text);
			inspection.paramFile.LightMonitorParam.VoltagePerValueR = Convert.ToInt32(txt_VoltagePerValueR.Text);
			inspection.paramFile.LightMonitorParam.VoltagePerValueG = Convert.ToInt32(txt_VoltagePerValueG.Text);
			inspection.paramFile.LightMonitorParam.VoltagePerValueB = Convert.ToInt32(txt_VoltagePerValueB.Text);
			inspection.paramFile.LightMonitorParam.ValueChangeFlag = true;
			txt_CorrectionValueError.Visibility = Visibility.Hidden;
			WriteUIValueToParam();
		}
		catch (Exception ex)
		{
			string text = "LW01-LightMonitorWindow-Btn_WriteMoniterValue_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void Ts_TrackerMonitorON_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Ts_TrackerCorrectionON_Click(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_WriteSoftCorrectionValue_Click(object sender, RoutedEventArgs e)
	{
		if (inspection.paramFile.SoftCorrectionON)
		{
			if (MessageBox.Show("目前已在校正模式下，寫入監控值會更新所有校正的二值化參數，是否繼續?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
			{
				return;
			}
			for (int i = 0; i < 30; i++)
			{
				if (inspection.paramFile.BinaryParam[i].EnableSoftCorrection)
				{
					inspection.paramFile.BinaryParam[i].ValueChangeFlag = true;
					if (inspection.paramFile.BinaryParam[i].THD1_ChannelIndex >= COLOR_CHANNEL.EM_RED && inspection.paramFile.BinaryParam[i].THD1_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						GetSoftCorrectionShiftValue((int)inspection.paramFile.BinaryParam[i].THD1_ChannelIndex, ref inspection.paramFile.BinaryParam[i].THD1_Min, ref inspection.paramFile.BinaryParam[i].THD1_Max);
					}
					if (inspection.paramFile.BinaryParam[i].THD2_ChannelIndex >= COLOR_CHANNEL.EM_RED && inspection.paramFile.BinaryParam[i].THD2_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						GetSoftCorrectionShiftValue((int)inspection.paramFile.BinaryParam[i].THD2_ChannelIndex, ref inspection.paramFile.BinaryParam[i].THD2_Min, ref inspection.paramFile.BinaryParam[i].THD2_Max);
					}
					if (inspection.paramFile.BinaryParam[i].THD3_ChannelIndex >= COLOR_CHANNEL.EM_RED && inspection.paramFile.BinaryParam[i].THD3_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						GetSoftCorrectionShiftValue((int)inspection.paramFile.BinaryParam[i].THD3_ChannelIndex, ref inspection.paramFile.BinaryParam[i].THD3_Min, ref inspection.paramFile.BinaryParam[i].THD3_Max);
					}
					if (inspection.paramFile.BinaryParam[i].THD4_ChannelIndex >= COLOR_CHANNEL.EM_RED && inspection.paramFile.BinaryParam[i].THD4_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						GetSoftCorrectionShiftValue((int)inspection.paramFile.BinaryParam[i].THD4_ChannelIndex, ref inspection.paramFile.BinaryParam[i].THD4_Min, ref inspection.paramFile.BinaryParam[i].THD4_Max);
					}
				}
			}
		}
		WriteUIValueToParam();
		ts_EnableSoftCorrection.IsChecked = true;
		int AvgR = 0;
		int AvgG = 0;
		int AvgB = 0;
		string ResultStr = "";
		inspection.lightmonitorTool.InspectRGB(ref AvgR, ref AvgG, ref AvgB, ref ResultStr);
		txt_SoftSetupR.Text = AvgR.ToString();
		txt_SoftSetupG.Text = AvgG.ToString();
		txt_SoftSetupB.Text = AvgB.ToString();
		inspection.paramFile.SoftShift_R = (inspection.paramFile.SoftShift_G = (inspection.paramFile.SoftShift_B = 0));
		inspection.paramFile.SoftCorrection_R = (inspection.paramFile.SoftCorrection_G = (inspection.paramFile.SoftCorrection_B = -1));
		txt_AllResult.Text = ResultStr;
	}

	private void GetSoftCorrectionShiftValue(int channel, ref int min, ref int max)
	{
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
	}

	private void btn_ChartR_Click(object sender, RoutedEventArgs e)
	{
		int subitem = Convert.ToInt32(((Button)sender).Uid);
		ChartWindow chartWindow = new ChartWindow();
		chartWindow.SetInspection(inspection);
		chartWindow.LoadLanguage(LanguageType);
		chartWindow.SetStatistics(4, subitem, 0);
		chartWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		try
		{
			INSP_LIGHTMONITOR_PARAM iNSP_LIGHTMONITOR_PARAM = default(INSP_LIGHTMONITOR_PARAM);
			iNSP_LIGHTMONITOR_PARAM = inspection.paramFile.LightMonitorParam;
			iNSP_LIGHTMONITOR_PARAM.ValueChangeFlag = true;
			iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex = cb_TracAutoSize.SelectedIndex;
			if (iNSP_LIGHTMONITOR_PARAM.ChannelIndex < COLOR_CHANNEL.EM_RED)
			{
				iNSP_LIGHTMONITOR_PARAM.ChannelIndex = COLOR_CHANNEL.EM_RED;
			}
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			GetTracker(ref rect);
			if (iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex < 0 || iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex > 20)
			{
				iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex = 0;
			}
			iNSP_LIGHTMONITOR_PARAM.SearchRectL = rect.X - inspection.paramFile.AdvLocateParam[iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex].L;
			iNSP_LIGHTMONITOR_PARAM.SearchRectT = rect.Y - inspection.paramFile.AdvLocateParam[iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex].T;
			iNSP_LIGHTMONITOR_PARAM.SearchRectR = inspection.paramFile.AdvLocateParam[iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex].Width - iNSP_LIGHTMONITOR_PARAM.SearchRectL - rect.Width;
			iNSP_LIGHTMONITOR_PARAM.SearchRectB = inspection.paramFile.AdvLocateParam[iNSP_LIGHTMONITOR_PARAM.TrackerAligmentIndex].Height - iNSP_LIGHTMONITOR_PARAM.SearchRectT - rect.Height;
			iNSP_LIGHTMONITOR_PARAM.LightMonitor_ON = ts_TrackerMonitorON.IsChecked.Value;
			iNSP_LIGHTMONITOR_PARAM.LightCorrection_ON = ts_TrackerCorrectionON.IsChecked.Value;
			iNSP_LIGHTMONITOR_PARAM.EnableSoftCorrection = ts_EnableSoftCorrection.IsChecked.Value;
			inspection.paramFile.SoftCorrSetup_R = Convert.ToInt32(txt_SoftSetupR.Text);
			inspection.paramFile.SoftCorrSetup_G = Convert.ToInt32(txt_SoftSetupG.Text);
			inspection.paramFile.SoftCorrSetup_B = Convert.ToInt32(txt_SoftSetupB.Text);
			iNSP_LIGHTMONITOR_PARAM.AlarmRatioR = Convert.ToDouble(txt_AlarmRatioR.Text);
			iNSP_LIGHTMONITOR_PARAM.AlarmRatioG = Convert.ToDouble(txt_AlarmRatioG.Text);
			iNSP_LIGHTMONITOR_PARAM.AlarmRatioB = Convert.ToDouble(txt_AlarmRatioB.Text);
			iNSP_LIGHTMONITOR_PARAM.CorrectionRatioR = Convert.ToDouble(txt_CorrectionRatioR.Text);
			iNSP_LIGHTMONITOR_PARAM.CorrectionRatioG = Convert.ToDouble(txt_CorrectionRatioG.Text);
			iNSP_LIGHTMONITOR_PARAM.CorrectionRatioB = Convert.ToDouble(txt_CorrectionRatioB.Text);
			inspection.paramFile.SoftCorrectoinAllowDiff = Convert.ToInt32(txt_AllowShift.Text);
			iNSP_LIGHTMONITOR_PARAM.ValueChangeFlag = true;
			inspection.paramFile.LightMonitorParam = iNSP_LIGHTMONITOR_PARAM;
		}
		catch (Exception ex)
		{
			string text = "EW01-LightMonitorWindow-WriteUIValueToParam()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/lightmonitorwindow.xaml", UriKind.Relative);
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
			((LightMonitorWindow)target).Closed += Window_Closed;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 3:
			bor_SearchRect1 = (Border)target;
			break;
		case 4:
			txt_Search1Label = (TextBlock)target;
			break;
		case 5:
			bor_LocateRect = (Border)target;
			break;
		case 6:
			rt_ReferenceRect1 = (Rectangle)target;
			break;
		case 7:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 8:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 9:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 10:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 11:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 12:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 13:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 14:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 15:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 16:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 17:
			btn_ImageNum = (Button)target;
			break;
		case 18:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 19:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 20:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 21:
			lab_RValue = (Label)target;
			break;
		case 22:
			lab_GValue = (Label)target;
			break;
		case 23:
			lab_BValue = (Label)target;
			break;
		case 24:
			lab_HValue = (Label)target;
			break;
		case 25:
			lab_SValue = (Label)target;
			break;
		case 26:
			lab_VValue = (Label)target;
			break;
		case 27:
			lab_GrayValue = (Label)target;
			break;
		case 28:
			lab_XValue = (Label)target;
			break;
		case 29:
			lab_YValue = (Label)target;
			break;
		case 30:
			lab_C1Value = (Label)target;
			break;
		case 31:
			lab_C2Value = (Label)target;
			break;
		case 32:
			lab_C3Value = (Label)target;
			break;
		case 33:
			txt_AllResult = (TextBox)target;
			break;
		case 34:
			tab_Fun = (TabControl)target;
			break;
		case 35:
			ts_TrackerMonitorON = (ToggleSwitch)target;
			ts_TrackerMonitorON.Click += Ts_TrackerMonitorON_Click;
			break;
		case 36:
			txt_MoniterValueError = (TextBox)target;
			break;
		case 37:
			ts_TrackerCorrectionON = (ToggleSwitch)target;
			ts_TrackerCorrectionON.Click += Ts_TrackerCorrectionON_Click;
			break;
		case 38:
			txt_CorrectionValueError = (TextBox)target;
			break;
		case 39:
			txt_AlarmRatioR = (TextBox)target;
			txt_AlarmRatioR.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 40:
			btn_WriteMoniterValue = (Button)target;
			btn_WriteMoniterValue.Click += Btn_WriteMoniterValue_Click;
			break;
		case 41:
			txt_LightMonitotB = (TextBox)target;
			break;
		case 42:
			txt_LightMonitotR = (TextBox)target;
			break;
		case 43:
			txt_LightMonitotG = (TextBox)target;
			break;
		case 44:
			txt_AlarmRatioG = (TextBox)target;
			txt_AlarmRatioG.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 45:
			txt_AlarmRatioB = (TextBox)target;
			txt_AlarmRatioB.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 46:
			btn_WriteCorrectionValue = (Button)target;
			btn_WriteCorrectionValue.Click += Btn_WriteCorrectionValue_Click;
			break;
		case 47:
			txt_LightCorrectionB = (TextBox)target;
			break;
		case 48:
			txt_LightCorrectionR = (TextBox)target;
			break;
		case 49:
			txt_LightCorrectionG = (TextBox)target;
			break;
		case 50:
			txt_CorrectionRatioR = (TextBox)target;
			txt_CorrectionRatioR.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 51:
			txt_CorrectionRatioG = (TextBox)target;
			txt_CorrectionRatioG.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 52:
			txt_CorrectionRatioB = (TextBox)target;
			txt_CorrectionRatioB.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 53:
			grd_Voltage = (Grid)target;
			break;
		case 54:
			txt_VoltagePerValueR = (TextBox)target;
			txt_VoltagePerValueR.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 55:
			txt_VoltagePerValueG = (TextBox)target;
			txt_VoltagePerValueG.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 56:
			txt_VoltagePerValueB = (TextBox)target;
			txt_VoltagePerValueB.PreviewMouseDoubleClick += Txt_AlarmBinaryValue_PreviewMouseDoubleClick;
			break;
		case 57:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 58:
			cb_TracAutoSize = (ComboBox)target;
			cb_TracAutoSize.SelectionChanged += Cb_TracAutoSize_SelectionChanged;
			break;
		case 59:
			btn_ChartR = (Button)target;
			btn_ChartR.Click += btn_ChartR_Click;
			break;
		case 60:
			btn_ChartG = (Button)target;
			btn_ChartG.Click += btn_ChartR_Click;
			break;
		case 61:
			btn_ChartB = (Button)target;
			btn_ChartB.Click += btn_ChartR_Click;
			break;
		case 62:
			tab_SoftCorr = (TabItem)target;
			break;
		case 63:
			ts_EnableSoftCorrection = (ToggleSwitch)target;
			break;
		case 64:
			txt_SoftCorrectionValueError = (TextBox)target;
			break;
		case 65:
			cb_TracAutoSize1 = (ComboBox)target;
			cb_TracAutoSize1.SelectionChanged += Cb_TracAutoSize_SelectionChanged;
			break;
		case 66:
			txt_AllowShift = (TextBox)target;
			break;
		case 67:
			txt_SoftSetupB = (TextBox)target;
			break;
		case 68:
			txt_SoftSetupR = (TextBox)target;
			break;
		case 69:
			txt_SoftSetupG = (TextBox)target;
			break;
		case 70:
			btn_WriteSoftCorrectionValue = (Button)target;
			btn_WriteSoftCorrectionValue.Click += Btn_WriteSoftCorrectionValue_Click;
			break;
		case 71:
			txt_SoftShiftB = (TextBox)target;
			break;
		case 72:
			txt_SoftShiftR = (TextBox)target;
			break;
		case 73:
			txt_SoftShiftG = (TextBox)target;
			break;
		case 74:
			rad_SoftCorrectionON = (RadioButton)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
