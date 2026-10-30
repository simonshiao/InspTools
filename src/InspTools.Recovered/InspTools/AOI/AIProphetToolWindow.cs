using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;
using OpenCvSharp;

namespace InspTools.AOI;

public class AIProphetToolWindow : System.Windows.Window, IComponentConnector
{
	private bool THD_Preview;

	private bool ShowMessage = true;

	private byte[] CurrentColor = new byte[13];

	private OpenCvSharp.Rect LocateRect;

	private bool UpdateUI;

	private const int MaxMask = 4;

	private OpenCvSharp.Rect OrgTrack = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect[] OrgMaskTrack = new OpenCvSharp.Rect[4];

	private int ImageListSource;

	private string[] filelist;

	private string[] DBfilelist;

	private int DBIndex;

	private int DBKind = 1;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private Mat ShowBitmap = new Mat();

	private Mat OrgMat = new Mat();

	private float[] TotalScore;

	private int LanguageType = 1;

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private int StationIndex;

	private CVInspection inspection;

	private CVInspection[] Station;

	private ThroughputParamFile ThroughputFile;

	private Rectangle[] MaskTrack;

	private Thread AILoadingThread;

	private ManualResetEvent AILoadingPauseEvent = new ManualResetEvent(initialState: false);

	private bool ContinueRun = true;

	internal Canvas cnv_Image;

	internal Rectangle bor_AIScope;

	internal Label Label_ResultError;

	internal Label Label_Result;

	internal Border bor_LocateRect;

	internal Rectangle bor_AIScopeMask1;

	internal Rectangle bor_AIScopeMask2;

	internal Rectangle bor_AIScopeMask3;

	internal Rectangle bor_AIScopeMask4;

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

	internal Button btn_SetModel;

	internal TextBox txt_ModeName;

	internal TextBox txt_ModelWidth;

	internal TextBox txt_ModelHeight;

	internal Button btn_Train;

	internal TextBox txt_TrainOKImageNum;

	internal Button btn_OpenModelImageFolder;

	internal TextBox txt_TrainCount;

	internal Canvas cnv_SourceImg;

	internal Canvas cnv_HeatMap;

	internal Button btn_AddPTrainImage;

	internal Button btn_AddPTrainNGImage;

	internal Button btn_LoadOKDB;

	internal Button btn_LoadNGDB;

	internal Button btn_TrainImageNum;

	internal Button btn_NextDBImage;

	internal Button btn_PrevDBImage;

	internal Button btn_DeleteDBImage;

	internal Button btn_LoadPASSDB;

	internal TextBox txt_TrainNGlmageNum;

	internal Button btn_AddPTrainPASSImage;

	internal TextBox txt_AllowScore;

	internal TextBox txt_Score;

	internal Button btn_Chart;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal ComboBox cb_ErrorBin;

	internal Rectangle rt_Result;

	internal Label Label_ResultScore;

	internal Button btn_TestPrevImg;

	internal Button btn_Test;

	internal Button btn_TestNextImg;

	internal TextBox txt_TrainResult;

	internal CheckBox chk_Mask1_ON;

	internal CheckBox chk_Mask2_ON;

	internal CheckBox chk_Mask3_ON;

	internal CheckBox chk_Mask4_ON;

	internal CheckBox chk_EnableTensorRT;

	internal ComboBox cb_ModelSize;

	internal Button btn_ResetTracker;

	internal TextBox txt_TrainPassImageNum;

	internal Label lab_Pareto_Copy;

	internal Grid grd_AILoading;

	internal Label lab_Message;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public AIProphetToolWindow()
	{
		InitializeComponent();
		THD_Preview = false;
		Label_ResultError.Visibility = Visibility.Hidden;
		Label_Result.Visibility = Visibility.Hidden;
		bor_AIScope.Visibility = Visibility.Visible;
		bor_AIScope.IsEnabled = true;
		MaskTrack = new Rectangle[4] { bor_AIScopeMask1, bor_AIScopeMask2, bor_AIScopeMask3, bor_AIScopeMask4 };
		for (int i = 0; i < 4; i++)
		{
			MaskTrack[i].Visibility = Visibility.Hidden;
			OrgMaskTrack[i] = new OpenCvSharp.Rect(0, 0, 50, 50);
		}
		AILoadingThread = new Thread(AILoadingRun);
		AILoadingThread.Start();
		grd_AILoading.Visibility = Visibility.Hidden;
		lab_Message.Opacity = 0.0;
		if (!File.Exists(string.Format("{0}PModel\\Train\\dist\\AIProphet\\AIProphet.exe", AppDomain.CurrentDomain.BaseDirectory)))
		{
			MessageBox.Show("AIProphet.exe is missing, please check \\PModel\\Train\\dist\\AIProphet\\AIProphet.exe");
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

	private void AILoadingRun()
	{
		while (true)
		{
			AILoadingPauseEvent.WaitOne(-1);
			if (ContinueRun)
			{
				inspection.aiProphetTools.ResetModelRead();
				inspection.aiProphetTools.ReadModel(inspection.paramFile.AIProphetParam.ModelName);
				Action method = delegate
				{
					grd_AILoading.Visibility = Visibility.Hidden;
				};
				base.Dispatcher.BeginInvoke(method);
				AILoadingPauseEvent.Reset();
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LoadLanguage(int language)
	{
		_ = AppDomain.CurrentDomain.BaseDirectory;
		LanguageType = language;
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
	}

	public void SetInspection(int index, CVInspection[] station)
	{
		Station = station;
		inspection = station[index];
		StationIndex = index;
	}

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
	{
		cnv_Image.SetValue(FrameworkElement.WidthProperty, 700.0);
		cnv_Image.SetValue(FrameworkElement.HeightProperty, 600.0);
		ImageWidth = mat.Cols;
		ImageHeight = mat.Rows;
		ImageBand = mat.Channels();
		OrgMat = mat.Clone();
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
		bor_LocateRect.IsEnabled = false;
		bor_LocateRect.SetValue(Canvas.LeftProperty, (double)locateRect.X * xRatio);
		bor_LocateRect.SetValue(Canvas.TopProperty, (double)locateRect.Y * yRatio);
		bor_LocateRect.SetValue(FrameworkElement.WidthProperty, (double)locateRect.Width * xRatio);
		bor_LocateRect.SetValue(FrameworkElement.HeightProperty, (double)locateRect.Height * yRatio);
		LocateRect = locateRect;
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", StationIndex + 1, inspection.paramFile.BinaryParam[StationIndex].TrackerName) + "BT01-BinaryWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		WriteUIValueToParam();
		ContinueRun = false;
		AILoadingPauseEvent.Set();
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

	private OpenCvSharp.Rect GetTrackRect(Rectangle bor_track)
	{
		OpenCvSharp.Rect result = default(OpenCvSharp.Rect);
		result.X = (int)(Canvas.GetLeft(bor_track) / xRatio);
		result.Y = (int)(Canvas.GetTop(bor_track) / yRatio);
		result.Width = (int)(bor_track.Width / xRatio);
		result.Height = (int)(bor_track.Height / yRatio);
		return result;
	}

	private void UpdateTrack(Rectangle bor_track, int L, int R, int T, int B)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(LocateRect.Left + L, LocateRect.Top + T, LocateRect.Width - (L + R), LocateRect.Height - (T + B));
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect &= rect2;
		bor_track.SetValue(Canvas.LeftProperty, (double)rect.X * xRatio);
		bor_track.SetValue(Canvas.TopProperty, (double)rect.Y * yRatio);
		bor_track.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
		bor_track.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * yRatio);
	}

	private void UpdateMaskTrack(Rectangle bor_track, int L, int R, int T, int B)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		rect = ((L != 5 || R != 5 || T != 5 || B != 5) ? new OpenCvSharp.Rect(LocateRect.Left + L, LocateRect.Top + T, LocateRect.Width - (L + R), LocateRect.Height - (T + B)) : new OpenCvSharp.Rect(LocateRect.Left + L, LocateRect.Top + T, LocateRect.Left + L + 50, LocateRect.Top + T + 50));
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect &= rect2;
		bor_track.SetValue(Canvas.LeftProperty, (double)rect.X * xRatio);
		bor_track.SetValue(Canvas.TopProperty, (double)rect.Y * yRatio);
		bor_track.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
		bor_track.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * yRatio);
	}

	private void WriteTrack(Rectangle bor_track, ref int L, ref int R, ref int T, ref int B)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect
		{
			X = (int)(Canvas.GetLeft(bor_track) / xRatio),
			Y = (int)(Canvas.GetTop(bor_track) / yRatio),
			Width = (int)(bor_track.Width / xRatio),
			Height = (int)(bor_track.Height / yRatio)
		};
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, ImageWidth, ImageHeight);
		rect &= rect2;
		L = rect.X - LocateRect.Left;
		T = rect.Y - LocateRect.Top;
		R = LocateRect.Width - L - rect.Width;
		B = LocateRect.Height - T - rect.Height;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		try
		{
			UpdateUI = false;
			INSP_AIPROPHET_PARAM aIProphetParam = inspection.paramFile.AIProphetParam;
			txt_ModeName.Text = aIProphetParam.ModelName;
			switch (aIProphetParam.ModelImageSize)
			{
			case 128:
				cb_ModelSize.SelectedIndex = 0;
				break;
			case 256:
				cb_ModelSize.SelectedIndex = 1;
				break;
			case 384:
				cb_ModelSize.SelectedIndex = 2;
				break;
			case 512:
				cb_ModelSize.SelectedIndex = 3;
				break;
			}
			if (inspection.aiProphetTools.ModelReaded)
			{
				cb_ModelSize.IsEnabled = false;
			}
			else
			{
				cb_ModelSize.IsEnabled = true;
			}
			txt_TrainCount.Text = aIProphetParam.TrainCount.ToString();
			txt_AllowScore.Text = aIProphetParam.AllowSocreMax.ToString();
			cb_ErrorBin.SelectedIndex = (int)aIProphetParam.ErrorBin;
			cb_ParetoChart.SelectedIndex = aIProphetParam.ParetoIndex;
			UpdateTrack(bor_AIScope, aIProphetParam.Track_L, aIProphetParam.Track_R, aIProphetParam.Track_T, aIProphetParam.Track_B);
			OrgTrack = GetTrackRect(bor_AIScope);
			chk_EnableTensorRT.IsChecked = aIProphetParam.EnableTensorRT;
			if (aIProphetParam.Mask1_ON)
			{
				bor_AIScopeMask1.Visibility = Visibility.Visible;
				chk_Mask1_ON.IsChecked = true;
			}
			else
			{
				bor_AIScopeMask1.Visibility = Visibility.Hidden;
			}
			UpdateMaskTrack(bor_AIScopeMask1, aIProphetParam.Mask1_L, aIProphetParam.Mask1_R, aIProphetParam.Mask1_T, aIProphetParam.Mask1_B);
			OrgMaskTrack[0] = GetTrackRect(bor_AIScopeMask1);
			if (aIProphetParam.Mask2_ON)
			{
				bor_AIScopeMask2.Visibility = Visibility.Visible;
				chk_Mask2_ON.IsChecked = true;
			}
			else
			{
				bor_AIScopeMask2.Visibility = Visibility.Hidden;
			}
			UpdateMaskTrack(bor_AIScopeMask2, aIProphetParam.Mask2_L, aIProphetParam.Mask2_R, aIProphetParam.Mask2_T, aIProphetParam.Mask2_B);
			OrgMaskTrack[1] = GetTrackRect(bor_AIScopeMask2);
			if (aIProphetParam.Mask3_ON)
			{
				bor_AIScopeMask3.Visibility = Visibility.Visible;
				chk_Mask3_ON.IsChecked = true;
			}
			else
			{
				bor_AIScopeMask3.Visibility = Visibility.Hidden;
			}
			UpdateMaskTrack(bor_AIScopeMask3, aIProphetParam.Mask3_L, aIProphetParam.Mask3_R, aIProphetParam.Mask3_T, aIProphetParam.Mask3_B);
			OrgMaskTrack[2] = GetTrackRect(bor_AIScopeMask3);
			if (aIProphetParam.Mask4_ON)
			{
				bor_AIScopeMask4.Visibility = Visibility.Visible;
				chk_Mask4_ON.IsChecked = true;
			}
			else
			{
				bor_AIScopeMask4.Visibility = Visibility.Hidden;
			}
			UpdateMaskTrack(bor_AIScopeMask4, aIProphetParam.Mask4_L, aIProphetParam.Mask4_R, aIProphetParam.Mask4_T, aIProphetParam.Mask4_B);
			OrgMaskTrack[3] = GetTrackRect(bor_AIScopeMask4);
			if (txt_ModeName.Text != "")
			{
				string path = string.Format("{0}PModel\\{1}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
				string path2 = string.Format("{0}PModel\\{1}\\NOPASS\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
				string path3 = string.Format("{0}PModel\\{1}\\PASS\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				if (!Directory.Exists(path2))
				{
					Directory.CreateDirectory(path2);
				}
				if (!Directory.Exists(path3))
				{
					Directory.CreateDirectory(path3);
				}
				int num = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Length;
				int num2 = Directory.GetFiles(path3, "*.*", SearchOption.AllDirectories).Length;
				txt_TrainOKImageNum.Text = (num + num2).ToString();
				string[] files = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
				txt_TrainNGlmageNum.Text = files.Length.ToString();
			}
			else
			{
				txt_TrainOKImageNum.Text = "0";
				txt_TrainNGlmageNum.Text = "0";
			}
			DBKind = 1;
			UpdateDBList();
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "EW01-AIProphetToolWindow-CopyValueToUI()";
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
		try
		{
			INSP_AIPROPHET_PARAM aIProphetParam = inspection.paramFile.AIProphetParam;
			aIProphetParam.ValueChangeFlag = true;
			aIProphetParam.ModelName = txt_ModeName.Text;
			aIProphetParam.TrainCount = Convert.ToInt32(txt_TrainCount.Text);
			aIProphetParam.AllowSocreMax = Convert.ToDouble(txt_AllowScore.Text);
			aIProphetParam.ErrorBin = (FLOW_BIN)cb_ErrorBin.SelectedIndex;
			aIProphetParam.ParetoIndex = cb_ParetoChart.SelectedIndex;
			OpenCvSharp.Rect trackRect = GetTrackRect(bor_AIScope);
			if (OrgTrack != trackRect)
			{
				WriteTrack(bor_AIScope, ref aIProphetParam.Track_L, ref aIProphetParam.Track_R, ref aIProphetParam.Track_T, ref aIProphetParam.Track_B);
			}
			aIProphetParam.EnableTensorRT = chk_EnableTensorRT.IsChecked.Value;
			switch (cb_ModelSize.SelectedIndex)
			{
			case 0:
				aIProphetParam.ModelImageSize = 128;
				break;
			case 1:
				aIProphetParam.ModelImageSize = 256;
				break;
			case 2:
				aIProphetParam.ModelImageSize = 384;
				break;
			}
			aIProphetParam.Mask1_ON = chk_Mask1_ON.IsChecked.Value;
			aIProphetParam.Mask2_ON = chk_Mask2_ON.IsChecked.Value;
			aIProphetParam.Mask3_ON = chk_Mask3_ON.IsChecked.Value;
			aIProphetParam.Mask4_ON = chk_Mask4_ON.IsChecked.Value;
			if (aIProphetParam.Mask1_ON && GetTrackRect(bor_AIScopeMask1) != OrgMaskTrack[0])
			{
				WriteTrack(bor_AIScopeMask1, ref aIProphetParam.Mask1_L, ref aIProphetParam.Mask1_R, ref aIProphetParam.Mask1_T, ref aIProphetParam.Mask1_B);
			}
			if (aIProphetParam.Mask2_ON && GetTrackRect(bor_AIScopeMask2) != OrgMaskTrack[1])
			{
				WriteTrack(bor_AIScopeMask2, ref aIProphetParam.Mask2_L, ref aIProphetParam.Mask2_R, ref aIProphetParam.Mask2_T, ref aIProphetParam.Mask2_B);
			}
			if (aIProphetParam.Mask3_ON && GetTrackRect(bor_AIScopeMask3) != OrgMaskTrack[2])
			{
				WriteTrack(bor_AIScopeMask3, ref aIProphetParam.Mask3_L, ref aIProphetParam.Mask3_R, ref aIProphetParam.Mask3_T, ref aIProphetParam.Mask3_B);
			}
			if (aIProphetParam.Mask4_ON && GetTrackRect(bor_AIScopeMask4) != OrgMaskTrack[3])
			{
				WriteTrack(bor_AIScopeMask4, ref aIProphetParam.Mask4_L, ref aIProphetParam.Mask4_R, ref aIProphetParam.Mask4_T, ref aIProphetParam.Mask4_B);
			}
			inspection.paramFile.AIProphetParam = aIProphetParam;
		}
		catch (Exception ex)
		{
			string text = "EW01-AIProphetToolWindow-WriteUIValueToParam()";
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
		return true;
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
			Mat mat = new Mat();
			inspection.Inspect(ref mat, offLine: true);
			txt_Score.Text = string.Format("{0:N3}", inspection.aiProphetTools.FinalScore);
			Label_ResultScore.Content = txt_Score.Text;
			if (inspection.AIProphetResult)
			{
				txt_AllowScore.Background = Brushes.White;
				rt_Result.Fill = Brushes.Lime;
				SetResult(result: true);
			}
			else
			{
				txt_AllowScore.Background = Brushes.Fuchsia;
				txt_AllResult.Text = inspection.aiProphetTools.ResultText;
				rt_Result.Fill = Brushes.Red;
				SetResult(result: false);
			}
			Mat heatMapImage = inspection.aiProphetTools.GetHeatMapImage();
			int cols = heatMapImage.Cols;
			int rows = heatMapImage.Rows;
			double num = 446.0 / (double)cols;
			double num2 = 320.0 / (double)rows;
			if (num > num2)
			{
				num = num2;
				cnv_HeatMap.SetValue(FrameworkElement.WidthProperty, (double)cols * num);
				cnv_SourceImg.SetValue(FrameworkElement.WidthProperty, (double)cols * num);
			}
			else
			{
				num2 = num;
				cnv_HeatMap.SetValue(FrameworkElement.HeightProperty, (double)rows * num2);
				cnv_SourceImg.SetValue(FrameworkElement.HeightProperty, (double)rows * num2);
			}
			if (heatMapImage.Width > 0 && heatMapImage.Height > 0)
			{
				ImageSource imageSource = inspection.buffers.Mat2ImageSource(heatMapImage);
				ImageBrush imageBrush = new ImageBrush();
				imageBrush.ImageSource = imageSource;
				cnv_HeatMap.Background = imageBrush;
			}
			Mat org = inspection.aiProphetTools.org;
			if (org.Width > 0 && org.Height > 0)
			{
				ImageSource imageSource2 = inspection.buffers.Mat2ImageSource(org);
				ImageBrush imageBrush2 = new ImageBrush();
				imageBrush2.ImageSource = imageSource2;
				cnv_SourceImg.Background = imageBrush2;
			}
		}
		catch (Exception ex)
		{
			string text = "EW01-AIProphetToolWindow-Btn_Test_Click()";
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_OpenModelImageFolder_Click(object sender, RoutedEventArgs e)
	{
		string text = txt_ModeName.Text;
		if (text == "")
		{
			return;
		}
		string text2 = string.Format("{0}PModel\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			string fileName = "c:\\windows\\explorer.exe";
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string arguments = string.Format("/root, {0}", text2);
			Process.Start(fileName, arguments);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AToolsWindow-Btn_OpenFolder_Click()" + ex.ToString());
		}
	}

	private void DirectoryCopy(string sourceDir, string targetDir)
	{
		if (!Directory.Exists(sourceDir))
		{
			return;
		}
		if (!Directory.Exists(targetDir))
		{
			Directory.CreateDirectory(targetDir);
			if (!Directory.Exists(targetDir))
			{
				return;
			}
		}
		string text = sourceDir.Replace(Directory.GetParent(sourceDir).ToString(), "").Replace(System.IO.Path.DirectorySeparatorChar.ToString(), "");
		if (!(sourceDir == targetDir + text))
		{
			string text2 = targetDir + text;
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string[] files = Directory.GetFiles(sourceDir);
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i], text2 + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(files[i]), overwrite: true);
			}
			string[] directories = Directory.GetDirectories(sourceDir);
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryCopy(directories[j], text2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Train_Click(object sender, RoutedEventArgs e)
	{
		if (txt_ModeName.Text == "" || txt_ModeName.Text == "train")
		{
			return;
		}
		if (cb_ModelSize.SelectedIndex == 3)
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("尚未選擇模型尺寸");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("尚未選擇模型尺寸");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("モデルサイズはまだ選択されていません。");
			}
			else
			{
				MessageBox.Show("Model size not yet selected.");
			}
			return;
		}
		WriteUIValueToParam();
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIProphetParam.ModelName == txt_ModeName.Text)
			{
				Station[i].aiProphetTools.ModelReaded = false;
			}
		}
		int traincount = Convert.ToInt32(txt_TrainCount.Text);
		string text = string.Format("{0}PModel\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
		FindSameImage(text, ShowMsg: false);
		string path = string.Format("{0}PModel\\{1}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		if (Directory.GetFiles(text, "*.*", SearchOption.AllDirectories).Length == 0)
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("OK訓練影像張數為0");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("OK训练影像张数为0");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("システムは、OK訓練の影像枚数がゼロであることを検出しました。");
			}
			else
			{
				MessageBox.Show("The number of OK training images is 0.");
			}
			return;
		}
		string text2 = text;
		string text3 = string.Format("{0}PModel\\train\\image\\", AppDomain.CurrentDomain.BaseDirectory);
		if (Directory.Exists(text2))
		{
			if (Directory.Exists(text3))
			{
				Directory.Delete(text3, recursive: true);
			}
			DirectoryCopy(text2, text3);
		}
		Action method = [MethodImpl(MethodImplOptions.NoInlining)] () =>
		{
			if (LanguageType == 1)
			{
				txt_TrainResult.Text = "訓練中!";
			}
			else if (LanguageType == 2)
			{
				txt_TrainResult.Text = "训练中!";
			}
			else if (LanguageType == 3)
			{
				txt_TrainResult.Text = "訓練中";
			}
			else
			{
				txt_TrainResult.Text = "Training ...!";
			}
			int num = RunTrainingScript(traincount, txt_ModeName.Text);
			if (num < 0)
			{
				switch (num)
				{
				case -1:
					if (LanguageType == 1)
					{
						MessageBox.Show("找不到訓練程式。(AIProphet.exe)");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show("找不到训练程式。(AIProphet.exe)");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show("訓練プログラムが見つかりません。(AIProphet.exe)");
					}
					else
					{
						MessageBox.Show("Training program not found. (AIProphet.exe)");
					}
					break;
				case -2:
					if (LanguageType == 1)
					{
						MessageBox.Show("找不到訓練程式。(PTraining.bat)");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show("找不到训练程式。(PTraining.bat)");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show("訓練プログラムが見つかりません。(PTraining.bat)");
					}
					else
					{
						MessageBox.Show("Training program not found. (PTraining.bat)");
					}
					break;
				case -3:
					if (LanguageType == 1)
					{
						MessageBox.Show("訓練失敗，找不到模型檔案!");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show("训练失败，找不到模型档案!");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show("訓練失敗、モデルファイルが見つかりません！");
					}
					else
					{
						MessageBox.Show("Training failed, cannot find the model file!");
					}
					break;
				}
				if (LanguageType == 1)
				{
					txt_TrainResult.Text = "訓練失敗!";
				}
				else if (LanguageType == 2)
				{
					txt_TrainResult.Text = "训练失败!";
				}
				else if (LanguageType == 3)
				{
					txt_TrainResult.Text = "訓練失敗";
				}
				else
				{
					txt_TrainResult.Text = "Training failed!";
				}
			}
			else
			{
				if (LanguageType == 1)
				{
					txt_TrainResult.Text = "訓練完成!";
				}
				else if (LanguageType == 2)
				{
					txt_TrainResult.Text = "训练完成!";
				}
				else if (LanguageType == 3)
				{
					txt_TrainResult.Text = "AIの訓練が成功しました。";
				}
				else
				{
					txt_TrainResult.Text = "Training completed!";
				}
				grd_AILoading.Visibility = Visibility.Visible;
				AILoadingPauseEvent.Set();
				txt_AllResult.Text = "";
			}
		};
		base.Dispatcher.BeginInvoke(method);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyModeltoTrain(string modelname)
	{
		string text = string.Format("{0}PModel\\{1}.pt", AppDomain.CurrentDomain.BaseDirectory, modelname);
		string path = string.Format("{0}PModel\\train\\model.onnx", AppDomain.CurrentDomain.BaseDirectory, modelname);
		string text2 = string.Format("{0}PModel\\train\\model.pt", AppDomain.CurrentDomain.BaseDirectory, modelname);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if (File.Exists(text2))
		{
			File.Delete(text2);
		}
		if (inspection.aiProphetTools.CheckEncrypt(text))
		{
			string path2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
			}
			int num = new Random(Guid.NewGuid().GetHashCode()).Next();
			string text3 = string.Format("{0}{1}{2}.pmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num);
			inspection.aiProphetTools.Decrypt(text, text3);
			if (File.Exists(text3))
			{
				File.Copy(text3, text2, overwrite: true);
				File.Delete(text3);
			}
		}
		else if (File.Exists(text))
		{
			File.Copy(text, text2, overwrite: true);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateTrainedModel(string modelname)
	{
		try
		{
			string text = string.Format("{0}PModel\\{1}.onnx", AppDomain.CurrentDomain.BaseDirectory, modelname);
			string path = string.Format("{0}PModel\\{1}.trt", AppDomain.CurrentDomain.BaseDirectory, modelname);
			string text2 = string.Format("{0}PModel\\{1}.pt", AppDomain.CurrentDomain.BaseDirectory, modelname);
			string text3 = string.Format("{0}PModel\\train\\model.onnx", AppDomain.CurrentDomain.BaseDirectory, modelname);
			string text4 = string.Format("{0}PModel\\train\\model.pt", AppDomain.CurrentDomain.BaseDirectory, modelname);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			inspection.aiProphetTools.Encrypt(text4, text2);
			File.Copy(text3, text, overwrite: true);
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			if (File.Exists(text4))
			{
				File.Delete(text4);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat("" + "BT01-AIProphetToolWindow-UpdateTrainedModel()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int RunTrainingScript(int count, string modelname)
	{
		CopyModeltoTrain(modelname);
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		string text = "";
		string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PTraining.bat");
		processStartInfo.FileName = "PTraining.bat";
		if (!File.Exists(string.Format("{0}PModel\\train\\dist\\AIProphet\\AIProphet.exe", AppDomain.CurrentDomain.BaseDirectory)))
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("找不到訓練程式。(AIProphet.exe)");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("找不到训练程式。(AIProphet.exe)");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("訓練プログラムが見つかりません。(AIProphet.exe)");
			}
			else
			{
				MessageBox.Show("Could not find the training program (AIProphet.exe)");
			}
			return -1;
		}
		text = string.Format("cd PModel\ncd train\ndist\\AIProphet\\AIProphet.exe --train --batch-size 8 --epochs {0} --input-size {1} --data-dir .\\{2}\\ --model-base-path  model", count, inspection.paramFile.AIProphetParam.ModelImageSize, "Image");
		if (File.Exists(text2))
		{
			FileInfo fileInfo = new FileInfo(text2);
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
			File.Delete(text2);
		}
		using (StreamWriter streamWriter = new StreamWriter(text2, append: true))
		{
			streamWriter.Write("{0}", text);
			streamWriter.WriteLine();
		}
		if (!File.Exists(text2))
		{
			return -2;
		}
		bool flag = false;
		if (flag)
		{
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
		}
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = flag;
		Process process = Process.Start(processStartInfo);
		if (flag)
		{
			string eOut = null;
			process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
			{
				eOut += e.Data;
			};
			process.BeginErrorReadLine();
			process.StandardOutput.ReadToEnd();
		}
		process.WaitForExit();
		process.Close();
		if (!File.Exists(string.Format("{0}PModel\\train\\model.onnx", AppDomain.CurrentDomain.BaseDirectory, modelname)))
		{
			return -3;
		}
		UpdateTrainedModel(modelname);
		return 1;
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
				cnv_Image.Background = Brushes.LightGray;
				THD_Preview = true;
				btn_Test.IsEnabled = false;
				if (!inspection.LocaetResult)
				{
					SetResult(2, FindResource("LocateFail").ToString());
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, FindResource("AdvLocateFail").ToString());
				}
			}
			else
			{
				mat = new Mat();
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				btn_Test.IsEnabled = true;
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat("" + "BT01-BinaryWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
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
				btn_Test.IsEnabled = false;
				if (!inspection.LocaetResult)
				{
					SetResult(2, FindResource("LocateFail").ToString());
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, FindResource("AdvLocateFail").ToString());
				}
			}
			else
			{
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat("" + "BT01-BinaryWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void SetResult(int result, string msg)
	{
		if (result == 2)
		{
			Label_Result.Visibility = Visibility.Hidden;
			Label_ResultError.Visibility = Visibility.Visible;
			Label_ResultError.Content = msg;
			Label_ResultError.Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
		}
	}

	private void Btn_Chart_Click(object sender, RoutedEventArgs e)
	{
		ChartWindow chartWindow = new ChartWindow();
		chartWindow.SetInspection(inspection);
		chartWindow.LoadLanguage(LanguageType);
		chartWindow.SetStatistics(5, 0, 0);
		chartWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AddPTrainImage_Click(object sender, RoutedEventArgs e)
	{
		if (txt_ModeName.Text == "")
		{
			return;
		}
		string text = string.Format("{0}PModel\\{1}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!Directory.Exists(text))
		{
			string text2 = string.Format("{0}PModel\\{1}\\Good\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			if (Directory.Exists(text2))
			{
				Directory.Move(text2, text);
			}
			else
			{
				Directory.CreateDirectory(text);
			}
		}
		if (!Directory.Exists(text))
		{
			string text3 = "";
			text3 = ((LanguageType == 1) ? string.Format("AI OK資料夾建立失敗，請重新設定AI影像路徑。({0})", text) : ((LanguageType == 2) ? string.Format("AI OK資料夾建立失敗，请重新设置AI影像路径。({0})", text) : ((LanguageType != 3) ? string.Format("AI OK database cannot be created. ({0})", text) : string.Format("AI OK資料夾を作成できません。({0})", text))));
			MessageBox.Show(text3, "警告");
		}
		else
		{
			AddTrainImage(text);
		}
	}

	private string GetFileHash(string file)
	{
		string result = "";
		if (!File.Exists(file))
		{
			return result;
		}
		HashAlgorithm hashAlgorithm = HashAlgorithm.Create();
		FileStream fileStream = new FileStream(file, FileMode.Open);
		result = BitConverter.ToString(hashAlgorithm.ComputeHash(fileStream));
		fileStream.Close();
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string FindSameImage(string TrainImageFolder, bool ShowMsg = true)
	{
		string text = "";
		if (!Directory.Exists(TrainImageFolder))
		{
			return text;
		}
		string[] files = Directory.GetFiles(TrainImageFolder, "*.*", SearchOption.AllDirectories);
		if (files.Length == 0)
		{
			return text;
		}
		try
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < files.Length; i++)
			{
				list.Add(files[i]);
			}
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(GetFileHash(list[j]));
			}
			for (int k = 0; k < list2.Count; k++)
			{
				if (list2[k] == "")
				{
					continue;
				}
				for (int l = 0; l < list2.Count; l++)
				{
					if (list[k] == list[l] || list2[l] == "" || !(list2[k] == list2[l]))
					{
						continue;
					}
					System.IO.Path.GetFileName(list[k]);
					System.IO.Path.GetFileName(list[l]);
					_ = list[k];
					string text2 = list[l];
					if (File.Exists(text2))
					{
						FileInfo fileInfo = new FileInfo(text2);
						if (fileInfo.IsReadOnly)
						{
							fileInfo.IsReadOnly = false;
						}
						File.Delete(text2);
						text = ((LanguageType == 1) ? "發現重複影像，系統自動刪除重複影像!" : ((LanguageType == 2) ? "发现重复影像，系统自动删除重复影像!" : ((LanguageType != 3) ? "Found duplicate image, system automatically deleted duplicate image." : "重複影像を発見しました。システムは自動的に重複影像を削除しました。")));
						list2[l] = "";
					}
				}
			}
			if ((text != "") & ShowMsg)
			{
				MessageBox.Show(text);
			}
			return text;
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIProphetToolWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, FindResource("Error").ToString());
			return text;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AddPTrainNGImage_Click(object sender, RoutedEventArgs e)
	{
		if (!(txt_ModeName.Text == ""))
		{
			string text = string.Format("{0}PModel\\{1}\\NOPASS\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text))
			{
				string text2 = "";
				text2 = ((LanguageType == 1) ? string.Format("AI OK資料夾建立失敗，請重新設定AI影像路徑。({0})", text) : ((LanguageType == 2) ? string.Format("AI OK資料夾建立失敗，请重新设置AI影像路径。({0})", text) : ((LanguageType != 3) ? string.Format("AI OK database cannot be created. ({0})", text) : string.Format("AI OK資料夾を作成できません。({0})", text))));
				MessageBox.Show(text2, "Warning");
			}
			else
			{
				AddTrainImage(text);
			}
		}
	}

	private void btn_LoadOKDB_Click(object sender, RoutedEventArgs e)
	{
		if (!(txt_ModeName.Text == ""))
		{
			Button button = (Button)sender;
			DBKind = Convert.ToInt32(button.Uid);
			DBfilelist = null;
			btn_LoadOKDB.Background = Brushes.White;
			btn_LoadNGDB.Background = Brushes.White;
			button.Background = Brushes.Gold;
			DBIndex = 0;
			UpdateDBList();
			if (DBfilelist.Length != 0)
			{
				PredictDBMat(ShowResult: true);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateDBList()
	{
		string text = "";
		text = ((DBKind == 0) ? "NOPASS" : ((DBKind != 1) ? "PASS" : "OK"));
		try
		{
			string path = string.Format("{0}PModel\\{1}\\{2}\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text, text);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			DBfilelist = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
			if (DBfilelist.Length == 0)
			{
				btn_TrainImageNum.Content = string.Format("{0} / {1}", 0, 0);
				cnv_SourceImg.Background = null;
				cnv_HeatMap.Background = null;
			}
			else
			{
				btn_TrainImageNum.Content = string.Format("{0} / {1}", DBIndex + 1, DBfilelist.Length);
			}
		}
		catch (Exception ex)
		{
			string text2 = "MW02-AIProphetToolWindow-UpdateDBList()";
			text2 += ex.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
		}
	}

	private void DelDBMat()
	{
		if (DBfilelist.Length != 0)
		{
			if (DBIndex >= DBfilelist.Length)
			{
				DBIndex = 0;
			}
			if (DBIndex < 0)
			{
				DBIndex = DBfilelist.Length - 1;
			}
			if (File.Exists(DBfilelist[DBIndex]))
			{
				File.Delete(DBfilelist[DBIndex]);
			}
			UpdateDBList();
			PredictDBMat(ShowResult: true);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private float PredictDBMat(bool ShowResult = false)
	{
		if (DBfilelist == null)
		{
			return 0f;
		}
		if (DBfilelist.Length == 0)
		{
			return 0f;
		}
		try
		{
			if (DBIndex >= DBfilelist.Length)
			{
				DBIndex = 0;
			}
			if (DBIndex < 0)
			{
				DBIndex = DBfilelist.Length - 1;
			}
			Mat mat = Cv2.ImRead(DBfilelist[DBIndex]);
			if (mat.Width > 0 && mat.Height > 0)
			{
				int cols = mat.Cols;
				int rows = mat.Rows;
				double num = 446.0 / (double)cols;
				double num2 = 320.0 / (double)rows;
				if (num > num2)
				{
					num = num2;
					cnv_HeatMap.SetValue(FrameworkElement.WidthProperty, (double)cols * num);
					cnv_SourceImg.SetValue(FrameworkElement.WidthProperty, (double)cols * num);
				}
				else
				{
					num2 = num;
					cnv_HeatMap.SetValue(FrameworkElement.HeightProperty, (double)rows * num2);
					cnv_SourceImg.SetValue(FrameworkElement.HeightProperty, (double)rows * num2);
				}
				ImageSource imageSource = inspection.buffers.Mat2ImageSource(mat);
				ImageBrush imageBrush = new ImageBrush();
				imageBrush.ImageSource = imageSource;
				cnv_SourceImg.Background = imageBrush;
			}
			float num3 = inspection.aiProphetTools.PredictDB(mat);
			float num4 = inspection.aiProphetTools.GetRealScore(num3);
			double num5 = Convert.ToDouble(txt_AllowScore.Text);
			if (ShowResult)
			{
				bool flag = true;
				string text = "";
				if (num4 != -1f)
				{
					if (!((double)num4 < num5))
					{
						text = ((LanguageType == 1) ? string.Format("[AI 先知] 分數 = {0:N2}\n\n", num4) : ((LanguageType == 2) ? string.Format("[AI 先知] 分数 = {0:N2}\n\n", num4) : ((LanguageType != 3) ? string.Format("[AI Enhance] Score = {0:N2}\n\n", num4) : string.Format("[AI 先知] スコア = {0:N2}\n\n", num4))));
					}
					else
					{
						flag = false;
						text = ((LanguageType == 1) ? string.Format("[AI 先知]檢測失敗，分數 = {0:N2}\n\n", num4) : ((LanguageType == 2) ? string.Format("[AI 先知] 检测失败，分数 = {0:N2}\n\n", num4) : ((LanguageType != 3) ? string.Format("[AI Enhance] Test Failed, Score = {0:N2}\n\n", num4) : string.Format("[AI 先知] チェック失敗、スコア = {0:N2}\n\n", num4))));
					}
				}
				else if (!((double)num3 > num5))
				{
					text = ((LanguageType == 1) ? string.Format("[AI 先知] 分數 = {0:N2}\n\n", num3) : ((LanguageType == 2) ? string.Format("[AI 先知] 分数 = {0:N2}\n\n", num3) : ((LanguageType != 3) ? string.Format("[AI Enhance] Score = {0:N2}\n\n", num3) : string.Format("[AI 先知] スコア = {0:N2}\n\n", num3))));
				}
				else
				{
					flag = false;
					text = ((LanguageType == 1) ? string.Format("[AI 先知]檢測失敗，分數 = {0:N2}\n\n", num3) : ((LanguageType == 2) ? string.Format("[AI 先知] 检测失败，分数 = {0:N2}\n\n", num3) : ((LanguageType != 3) ? string.Format("[AI Enhance] Test Failed, Score = {0:N2}\n\n", num3) : string.Format("[AI 先知] チェック失敗、スコア = {0:N2}\n\n", num3))));
				}
				Mat heatMapImage = inspection.aiProphetTools.GetHeatMapImage();
				if (heatMapImage.Width > 0 && heatMapImage.Height > 0)
				{
					ImageSource imageSource2 = inspection.buffers.Mat2ImageSource(heatMapImage);
					ImageBrush imageBrush2 = new ImageBrush();
					imageBrush2.ImageSource = imageSource2;
					cnv_HeatMap.Background = imageBrush2;
				}
				btn_TrainImageNum.Content = string.Format("{0} / {1}", DBIndex + 1, DBfilelist.Length);
				txt_Score.Text = string.Format("{0:N1}", num4);
				Label_ResultScore.Content = txt_Score.Text;
				if (flag)
				{
					txt_AllowScore.Background = Brushes.White;
					rt_Result.Fill = Brushes.Lime;
					txt_AllResult.Text = "";
					SetResult(result: true);
				}
				else
				{
					txt_AllowScore.Background = Brushes.Fuchsia;
					txt_AllResult.Text = text;
					rt_Result.Fill = Brushes.Red;
					SetResult(result: false);
				}
			}
			if (num4 == -1f)
			{
				return num3;
			}
			return num4;
		}
		catch (Exception ex)
		{
			string text2 = "MW02-AIProphetToolWindow-PredictDBMat()";
			text2 += ex.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return 0f;
		}
	}

	private void btn_PrevDBImage_Click(object sender, RoutedEventArgs e)
	{
		DBIndex--;
		PredictDBMat(ShowResult: true);
	}

	private void btn_NextDBImage_Click(object sender, RoutedEventArgs e)
	{
		DBIndex++;
		PredictDBMat(ShowResult: true);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_DeleteDBImage_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("當前的AI資料庫影像將被刪除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("当前的AI资料库影像将被删除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Delete current AI database image ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在のAIデータベースの画像が削除されます!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			DelDBMat();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddTrainImage(string ImageFolder)
	{
		try
		{
			Btn_Test_Click(this, null);
			if (Station[StationIndex].aiProphetTools.org.Width > 0 && Station[StationIndex].aiProphetTools.org.Height > 0)
			{
				DateTime now = DateTime.Now;
				string text = string.Format("OK-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
				string fileName = string.Format("{0}{1}", ImageFolder, text);
				Station[StationIndex].aiProphetTools.org.SaveImage(fileName);
				if (FindSameImage(ImageFolder) == "")
				{
					string str = ((LanguageType == 1) ? string.Format("{0} AI 訓練影像儲存成功", text) : ((LanguageType == 2) ? string.Format("{0} AI 训练影像储存成功!", text) : ((LanguageType != 3) ? string.Format("{0} AI training image storage successful.", text) : string.Format("{0} AIトレーニング画像の保存が完了しました。", text))));
					ShowLabMessage(str);
				}
				if (Directory.Exists(ImageFolder))
				{
					string[] files = Directory.GetFiles(ImageFolder, "*.*", SearchOption.AllDirectories);
					txt_TrainPassImageNum.Text = files.Length.ToString();
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-btn_AddPTrainImage_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AddPTrainPASSImage_Click(object sender, RoutedEventArgs e)
	{
		if (!(txt_ModeName.Text == ""))
		{
			string text = string.Format("{0}PModel\\{1}\\PASS\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text))
			{
				string text2 = "";
				text2 = ((LanguageType == 1) ? string.Format("AI PASS資料夾建立失敗，請重新設定AI影像路徑。({0})", text) : ((LanguageType == 2) ? string.Format("AI PASS资料夹建立失敗，请重新设置AI影像路径。({0})", text) : ((LanguageType != 3) ? string.Format("AI PASS database cannot be created. ({0})", text) : string.Format("AI PASSデータバースを作成できません。({0})", text))));
				MessageBox.Show(text2, "Alert");
			}
			else
			{
				AddTrainImage(text);
			}
		}
	}

	private void chk_Mask1_ON_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			CheckBox obj = (CheckBox)sender;
			int num = Convert.ToInt32(obj.Uid);
			if (obj.IsChecked.Value)
			{
				MaskTrack[num].Visibility = Visibility.Visible;
			}
			else
			{
				MaskTrack[num].Visibility = Visibility.Hidden;
			}
		}
	}

	private void chk_EnableTensorRT_Click(object sender, RoutedEventArgs e)
	{
		Action method = delegate
		{
			grd_AILoading.Visibility = Visibility.Visible;
			WriteUIValueToParam();
			AILoadingPauseEvent.Set();
		};
		base.Dispatcher.BeginInvoke(method);
	}

	private void btn_ResetTracker_Click(object sender, RoutedEventArgs e)
	{
		bor_AIScope.SetValue(Canvas.LeftProperty, (double)LocateRect.Left * xRatio - 5.0);
		bor_AIScope.SetValue(Canvas.TopProperty, (double)LocateRect.Top * yRatio - 5.0);
		bor_AIScope.SetValue(FrameworkElement.WidthProperty, (double)LocateRect.Width * xRatio + 10.0);
		bor_AIScope.SetValue(FrameworkElement.HeightProperty, (double)LocateRect.Height * yRatio + 10.0);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetModel_Click(object sender, RoutedEventArgs e)
	{
		ProphetOnnxModelsWindow prophetOnnxModelsWindow = new ProphetOnnxModelsWindow();
		prophetOnnxModelsWindow.SetCurrentModelName(txt_ModeName.Text);
		prophetOnnxModelsWindow.SetInspection(Station);
		prophetOnnxModelsWindow.LoadLanguage(LanguageType);
		prophetOnnxModelsWindow.ShowDialog();
		txt_ModeName.Text = prophetOnnxModelsWindow.GetSelectModelName();
		if (!(txt_ModeName.Text == ""))
		{
			inspection.paramFile.AIProphetToolON = true;
			inspection.aiProphetTools.ResetModelRead();
			string path = string.Format("{0}PModel\\{1}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			string path2 = string.Format("{0}PModel\\{1}\\NOPASS\\", AppDomain.CurrentDomain.BaseDirectory, txt_ModeName.Text);
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				txt_TrainOKImageNum.Text = files.Length.ToString();
			}
			else
			{
				Directory.CreateDirectory(path);
				txt_TrainOKImageNum.Text = "0";
			}
			if (Directory.Exists(path2))
			{
				string[] files2 = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
				txt_TrainNGlmageNum.Text = files2.Length.ToString();
			}
			else
			{
				Directory.CreateDirectory(path2);
				txt_TrainNGlmageNum.Text = "0";
			}
			Action method = delegate
			{
				grd_AILoading.Visibility = Visibility.Visible;
				AILoadingPauseEvent.Set();
			};
			base.Dispatcher.BeginInvoke(method);
			cb_ModelSize.IsEnabled = false;
			switch (inspection.aiProphetTools.GetModelSize().Width)
			{
			case 128:
				cb_ModelSize.SelectedIndex = 0;
				break;
			case 256:
				cb_ModelSize.SelectedIndex = 1;
				break;
			case 384:
				cb_ModelSize.SelectedIndex = 2;
				break;
			default:
				cb_ModelSize.SelectedIndex = 3;
				cb_ModelSize.IsEnabled = true;
				break;
			}
			Btn_Test_Click(this, null);
		}
	}

	private void ShowLabMessage(string str)
	{
		DoubleAnimation doubleAnimation = new DoubleAnimation();
		doubleAnimation.From = 0.0;
		doubleAnimation.To = 1.0;
		doubleAnimation.AutoReverse = true;
		doubleAnimation.RepeatBehavior = new RepeatBehavior(1.0);
		lab_Message.Content = str;
		doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.0));
		lab_Message.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/aiprophettoolwindow.xaml", UriKind.Relative);
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
			((AIProphetToolWindow)target).Closed += Window_Closed;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 3:
			bor_AIScope = (Rectangle)target;
			break;
		case 4:
			Label_ResultError = (Label)target;
			break;
		case 5:
			Label_Result = (Label)target;
			break;
		case 6:
			bor_LocateRect = (Border)target;
			break;
		case 7:
			bor_AIScopeMask1 = (Rectangle)target;
			break;
		case 8:
			bor_AIScopeMask2 = (Rectangle)target;
			break;
		case 9:
			bor_AIScopeMask3 = (Rectangle)target;
			break;
		case 10:
			bor_AIScopeMask4 = (Rectangle)target;
			break;
		case 11:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 12:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 13:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 14:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 15:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 16:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 17:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 18:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 19:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 20:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 21:
			btn_ImageNum = (Button)target;
			break;
		case 22:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 23:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 24:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 25:
			btn_ImgC4 = (Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 26:
			btn_ImgC5 = (Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 27:
			btn_ImgC6 = (Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 28:
			lab_RValue = (Label)target;
			break;
		case 29:
			lab_GValue = (Label)target;
			break;
		case 30:
			lab_BValue = (Label)target;
			break;
		case 31:
			lab_HValue = (Label)target;
			break;
		case 32:
			lab_SValue = (Label)target;
			break;
		case 33:
			lab_VValue = (Label)target;
			break;
		case 34:
			lab_GrayValue = (Label)target;
			break;
		case 35:
			lab_XValue = (Label)target;
			break;
		case 36:
			lab_YValue = (Label)target;
			break;
		case 37:
			lab_C1Value = (Label)target;
			break;
		case 38:
			lab_C2Value = (Label)target;
			break;
		case 39:
			lab_C3Value = (Label)target;
			break;
		case 40:
			lab_C4Value = (Label)target;
			break;
		case 41:
			lab_C5Value = (Label)target;
			break;
		case 42:
			lab_C6Value = (Label)target;
			break;
		case 43:
			txt_AllResult = (TextBox)target;
			break;
		case 44:
			btn_SetModel = (Button)target;
			btn_SetModel.Click += Btn_SetModel_Click;
			break;
		case 45:
			txt_ModeName = (TextBox)target;
			break;
		case 46:
			txt_ModelWidth = (TextBox)target;
			break;
		case 47:
			txt_ModelHeight = (TextBox)target;
			break;
		case 48:
			btn_Train = (Button)target;
			btn_Train.Click += btn_Train_Click;
			break;
		case 49:
			txt_TrainOKImageNum = (TextBox)target;
			break;
		case 50:
			btn_OpenModelImageFolder = (Button)target;
			btn_OpenModelImageFolder.Click += btn_OpenModelImageFolder_Click;
			break;
		case 51:
			txt_TrainCount = (TextBox)target;
			break;
		case 52:
			cnv_SourceImg = (Canvas)target;
			cnv_SourceImg.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 53:
			cnv_HeatMap = (Canvas)target;
			cnv_HeatMap.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			break;
		case 54:
			btn_AddPTrainImage = (Button)target;
			btn_AddPTrainImage.Click += btn_AddPTrainImage_Click;
			break;
		case 55:
			btn_AddPTrainNGImage = (Button)target;
			btn_AddPTrainNGImage.Click += btn_AddPTrainNGImage_Click;
			break;
		case 56:
			btn_LoadOKDB = (Button)target;
			btn_LoadOKDB.Click += btn_LoadOKDB_Click;
			break;
		case 57:
			btn_LoadNGDB = (Button)target;
			btn_LoadNGDB.Click += btn_LoadOKDB_Click;
			break;
		case 58:
			btn_TrainImageNum = (Button)target;
			break;
		case 59:
			btn_NextDBImage = (Button)target;
			btn_NextDBImage.Click += btn_NextDBImage_Click;
			break;
		case 60:
			btn_PrevDBImage = (Button)target;
			btn_PrevDBImage.Click += btn_PrevDBImage_Click;
			break;
		case 61:
			btn_DeleteDBImage = (Button)target;
			btn_DeleteDBImage.Click += btn_DeleteDBImage_Click;
			break;
		case 62:
			btn_LoadPASSDB = (Button)target;
			btn_LoadPASSDB.Click += btn_LoadOKDB_Click;
			break;
		case 63:
			txt_TrainNGlmageNum = (TextBox)target;
			break;
		case 64:
			btn_AddPTrainPASSImage = (Button)target;
			btn_AddPTrainPASSImage.Click += btn_AddPTrainPASSImage_Click;
			break;
		case 65:
			txt_AllowScore = (TextBox)target;
			break;
		case 66:
			txt_Score = (TextBox)target;
			break;
		case 67:
			btn_Chart = (Button)target;
			btn_Chart.Click += Btn_Chart_Click;
			break;
		case 68:
			lab_Pareto = (Label)target;
			break;
		case 69:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 70:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 71:
			rt_Result = (Rectangle)target;
			break;
		case 72:
			Label_ResultScore = (Label)target;
			break;
		case 73:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 74:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 75:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 76:
			txt_TrainResult = (TextBox)target;
			break;
		case 77:
			chk_Mask1_ON = (CheckBox)target;
			chk_Mask1_ON.Click += chk_Mask1_ON_Click;
			break;
		case 78:
			chk_Mask2_ON = (CheckBox)target;
			chk_Mask2_ON.Click += chk_Mask1_ON_Click;
			break;
		case 79:
			chk_Mask3_ON = (CheckBox)target;
			chk_Mask3_ON.Click += chk_Mask1_ON_Click;
			break;
		case 80:
			chk_Mask4_ON = (CheckBox)target;
			chk_Mask4_ON.Click += chk_Mask1_ON_Click;
			break;
		case 81:
			chk_EnableTensorRT = (CheckBox)target;
			chk_EnableTensorRT.Click += chk_EnableTensorRT_Click;
			break;
		case 82:
			cb_ModelSize = (ComboBox)target;
			break;
		case 83:
			btn_ResetTracker = (Button)target;
			btn_ResetTracker.Click += btn_ResetTracker_Click;
			break;
		case 84:
			txt_TrainPassImageNum = (TextBox)target;
			break;
		case 85:
			lab_Pareto_Copy = (Label)target;
			break;
		case 86:
			grd_AILoading = (Grid)target;
			break;
		case 87:
			lab_Message = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
