using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using OpenCvSharp;

namespace InspTools.Other;

public class BlobStatisticsWindow : System.Windows.Window, IComponentConnector
{
	private CVInspection inspection;

	private int Index;

	private int ID;

	private int DefalutValue;

	private int BlobType;

	private int LanguageType = 1;

	private int TrackNo = 1;

	public double ValueMin;

	public double ValueMax;

	private int ImageListSource;

	private string[] filelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int OKCounter;

	private int NGCounter;

	public Mat LocatBuf = new Mat();

	public OpenCvSharp.Rect LocateRect = new OpenCvSharp.Rect(0, 0, 0, 0);

	internal CartesianChart ChartWidth;

	internal TextBox txt_Min;

	internal TextBox txt_Max;

	internal TextBox txt_Std;

	internal TextBox txt_Avg;

	internal TextBox txt_Range;

	internal Label lb_ChartName;

	internal Button btn_Next;

	internal Button btn_Forword;

	internal TextBox txt_ValueMin;

	internal TextBox txt_ValueMax;

	internal Button btn_SetMinMax;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Line line_X1;

	internal Line line_Y1;

	internal Line line_X2;

	internal Line line_Y2;

	internal Label lab_Message;

	internal Button btn_Close;

	internal TextBox txt_OK;

	internal TextBox txt_NG;

	internal TextBox txt_Total;

	internal TextBox txt_OKRatio;

	internal TextBox txt_NGRatio;

	private bool _contentLoaded;

	public ChartValues<ObservablePoint> Chart1ValuesA { get; set; }

	public ChartValues<ObservablePoint> Chart1ValuesB { get; set; }

	public ChartValues<ObservablePoint> Chart1ValuesMin { get; set; }

	public ChartValues<ObservablePoint> Chart1ValuesMax { get; set; }

	public BlobStatisticsWindow()
	{
		InitializeComponent();
		lab_Message.Visibility = Visibility.Hidden;
	}

	public void SetImageSource(int imagelistsource, string[] flist, int findex, int recentindex, int okindex, int ngindex)
	{
		ImageListSource = imagelistsource;
		filelist = flist;
		fileIndex = findex;
		RecentBufferIndex = recentindex;
		OKBufferIndex = okindex;
		NGBufferIndex = ngindex;
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

	public void SetInspection(int index, int id, CVInspection station, int type, int trackno, double min, double max)
	{
		inspection = station;
		Index = index;
		TrackNo = trackno;
		ValueMin = min;
		ValueMax = max;
		txt_ValueMin.Text = min.ToString();
		txt_ValueMax.Text = max.ToString();
		CreatChartSeries(type);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CreatChartSeries(int type)
	{
		BlobType = type;
		string text = "";
		string text2 = "";
		switch (type)
		{
		default:
			return;
		case 0:
			text = "A Width";
			text2 = "B Width";
			lb_ChartName.Content = LanguageManager.Instance["Width"];
			break;
		case 1:
			text = "A Height";
			text2 = "B Height";
			lb_ChartName.Content = LanguageManager.Instance["Height"];
			break;
		case 2:
			text = "A Area";
			text2 = "B Area";
			lb_ChartName.Content = LanguageManager.Instance["Area"];
			break;
		case 3:
			text = "A WH Ratio";
			text2 = "B WH Ratio";
			lb_ChartName.Content = LanguageManager.Instance["WHRatio"];
			break;
		case 4:
			text = "A AreaRatio";
			text2 = "B AreaRatio";
			lb_ChartName.Content = LanguageManager.Instance["AreaRatio"];
			break;
		case 10:
			text = "A All Area";
			text2 = "B All Area";
			lb_ChartName.Content = LanguageManager.Instance["AllArea"];
			break;
		case 11:
			text = "A All Area Ratio";
			text2 = "B All Area Ratio";
			lb_ChartName.Content = LanguageManager.Instance["AllAreaRatio"];
			break;
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
			return;
		}
		if (ChartWidth.Series != null)
		{
			ChartWidth.Series.Clear();
		}
		CartesianMapper<ObservablePoint> configuration = Mappers.Xy<ObservablePoint>().X((ObservablePoint point) => point.X).Y((ObservablePoint point) => point.Y);
		Chart1ValuesA = new ChartValues<ObservablePoint>();
		Chart1ValuesB = new ChartValues<ObservablePoint>();
		Chart1ValuesMin = new ChartValues<ObservablePoint>();
		Chart1ValuesMax = new ChartValues<ObservablePoint>();
		ChartWidth.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = text,
				Values = Chart1ValuesA,
				PointGeometrySize = 10.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			},
			new LineSeries
			{
				Title = text2,
				Values = Chart1ValuesB,
				PointGeometrySize = 10.0,
				Stroke = Brushes.Aqua,
				Fill = Brushes.Transparent,
				LineSmoothness = 5.0
			}
		};
		DrawChart();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DrawChart()
	{
		try
		{
			Chart1ValuesA.Clear();
			Chart1ValuesB.Clear();
			Chart1ValuesMin.Clear();
			Chart1ValuesMax.Clear();
			OKCounter = (NGCounter = 0);
			if (inspection != null)
			{
				double num = 10000000.0;
				double num2 = 0.0;
				double num3 = 0.0;
				double num4 = 0.0;
				double num5 = 0.0;
				if (inspection.binaryTool[Index].BlobStatistics[1].Count < inspection.binaryTool[Index].BlobStatistics[0].Count)
				{
					TrackNo = 1;
				}
				for (int i = 0; i < inspection.binaryTool[Index].BlobStatistics[ID].Count; i++)
				{
					double num6 = 0.0;
					double num7 = 0.0;
					bool flag = true;
					Chart1ValuesMin.Add(new ObservablePoint(i, ValueMin));
					Chart1ValuesMax.Add(new ObservablePoint(i, ValueMax));
					switch (BlobType)
					{
					case 0:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].Width));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].Width;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].Width));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].Width;
						}
						break;
					case 1:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].Height));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].Height;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].Height));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].Height;
						}
						break;
					case 2:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].Area));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].Area;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].Area));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].Area;
						}
						break;
					case 3:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].WHRatio));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].WHRatio;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].WHRatio));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].WHRatio;
						}
						break;
					case 4:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].AreaRatio));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].AreaRatio;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].AreaRatio));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].AreaRatio;
						}
						break;
					case 10:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].TotalArea));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].TotalArea;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].TotalArea));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].TotalArea;
						}
						break;
					case 11:
						Chart1ValuesA.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[0][i].TotalAreaRatio));
						num6 = inspection.binaryTool[Index].BlobStatistics[0][i].TotalAreaRatio;
						if (TrackNo == 2)
						{
							Chart1ValuesB.Add(new ObservablePoint(i, inspection.binaryTool[Index].BlobStatistics[1][i].TotalAreaRatio));
							num7 = inspection.binaryTool[Index].BlobStatistics[1][i].TotalAreaRatio;
						}
						break;
					}
					if (num6 > num2)
					{
						num2 = num6;
					}
					if (num6 < num)
					{
						num = num6;
					}
					if (num6 < ValueMin || num6 > ValueMax)
					{
						flag = false;
					}
					if (TrackNo == 2)
					{
						if (num7 > num2)
						{
							num2 = num7;
						}
						if (num7 < num)
						{
							num = num7;
						}
						num3 += num6 + num7;
						if (num7 < ValueMin || num7 > ValueMax)
						{
							flag = false;
						}
					}
					else
					{
						num3 += num6;
					}
					if (flag)
					{
						OKCounter++;
					}
					else
					{
						NGCounter++;
					}
				}
				txt_OK.Text = OKCounter.ToString();
				txt_NG.Text = NGCounter.ToString();
				txt_Total.Text = (OKCounter + NGCounter).ToString();
				if (OKCounter + NGCounter > 0)
				{
					txt_OKRatio.Text = string.Format("{0:N2}%", (double)OKCounter / (double)(OKCounter + NGCounter) * 100.0);
					txt_NGRatio.Text = string.Format("{0:N2}%", (double)NGCounter / (double)(OKCounter + NGCounter) * 100.0);
				}
				txt_Max.Text = num2.ToString();
				txt_Min.Text = num.ToString();
				num4 = ((TrackNo != 2) ? (num3 / (double)inspection.binaryTool[Index].BlobStatistics[ID].Count) : (num3 / (double)(inspection.binaryTool[Index].BlobStatistics[ID].Count * 2)));
				if (num4 >= 1.0)
				{
					txt_Avg.Text = string.Format("{0:N2}", num4);
				}
				else
				{
					txt_Avg.Text = string.Format("{0:N5}", num4);
				}
				double num8 = Math.Abs(num2 - num);
				if (num8 >= 1.0)
				{
					txt_Range.Text = string.Format("{0:N2}", num8);
				}
				else
				{
					txt_Range.Text = string.Format("{0:N5}", num8);
				}
				double num9 = 0.0;
				double num10 = 0.0;
				for (int j = 0; j < inspection.binaryTool[Index].BlobStatistics[ID].Count; j++)
				{
					double num11 = 0.0;
					double num12 = 0.0;
					switch (BlobType)
					{
					case 0:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].Width;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].Width;
						}
						break;
					case 1:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].Height;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].Height;
						}
						break;
					case 2:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].Area;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].Area;
						}
						break;
					case 3:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].WHRatio;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].WHRatio;
						}
						break;
					case 4:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].AreaRatio;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].AreaRatio;
						}
						break;
					case 10:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].TotalArea;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].TotalArea;
						}
						break;
					case 11:
						num11 = inspection.binaryTool[Index].BlobStatistics[0][j].TotalAreaRatio;
						if (TrackNo == 2)
						{
							num12 = inspection.binaryTool[Index].BlobStatistics[1][j].TotalAreaRatio;
						}
						break;
					}
					num9 += Math.Pow(num11 - num4, 2.0);
					num10++;
					if (TrackNo == 2)
					{
						num9 += Math.Pow(num12 - num4, 2.0);
						num10++;
					}
				}
				num5 = Math.Sqrt(num9 / num10);
				if (num5 >= 1.0)
				{
					txt_Std.Text = string.Format("{0:N2}", num5);
				}
				else
				{
					txt_Std.Text = string.Format("{0:N5}", num5);
				}
			}
			ChartWidth.AxisX[0].MaxValue = 100.0;
			ChartWidth.AxisX[0].MinValue = DefalutValue;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("BSW-BlobStatisticsWindows-DrawChart()") + ex.ToString());
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void ChartWidth_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta > 0)
		{
			DefalutValue -= 100;
			if (DefalutValue <= 0)
			{
				DefalutValue = 0;
			}
			ChartWidth.AxisX[0].MaxValue = DefalutValue + 100;
			ChartWidth.AxisX[0].MinValue = DefalutValue;
		}
		else
		{
			DefalutValue += 100;
			if (DefalutValue >= 400)
			{
				DefalutValue = 400;
			}
			ChartWidth.AxisX[0].MaxValue = DefalutValue + 100;
			ChartWidth.AxisX[0].MinValue = DefalutValue;
		}
	}

	private void Btn_Next_Click(object sender, RoutedEventArgs e)
	{
		DefalutValue += 100;
		if (DefalutValue >= 400)
		{
			DefalutValue = 400;
		}
		ChartWidth.AxisX[0].MaxValue = DefalutValue + 100;
		ChartWidth.AxisX[0].MinValue = DefalutValue;
	}

	private void Btn_Forword_Click(object sender, RoutedEventArgs e)
	{
		DefalutValue -= 100;
		if (DefalutValue <= 0)
		{
			DefalutValue = 0;
		}
		ChartWidth.AxisX[0].MaxValue = DefalutValue + 100;
		ChartWidth.AxisX[0].MinValue = DefalutValue;
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		int type = Convert.ToInt32(((Button)sender).Uid);
		CreatChartSeries(type);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ShowImage(int fileindex, OpenCvSharp.Rect r1, OpenCvSharp.Rect r2)
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
				if (filelist == null || fileindex < 0 || fileindex >= filelist.Length || !File.Exists(filelist[fileindex]))
				{
					return;
				}
				mat = CVBuffer.ImRead(filelist[fileindex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (inspection.RecentBufferList == null || fileindex < 0 || inspection.RecentBufferList.Count <= 0 || fileindex >= inspection.RecentBufferList.Count)
				{
					return;
				}
				inspection.SetBuffers(inspection.RecentBufferList[fileindex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (inspection.OKBufferList == null || fileindex < 0 || inspection.OKBufferList.Count <= 0 || fileindex >= inspection.OKBufferList.Count)
				{
					return;
				}
				inspection.SetBuffers(inspection.OKBufferList[fileindex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (inspection.NGBufferList == null || fileindex < 0 || inspection.NGBufferList.Count <= 0 || fileindex >= inspection.NGBufferList.Count)
				{
					return;
				}
				NGBufferIndex = 0;
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat);
			if (inspection.LocaetResult && inspection.AdvlocateResult)
			{
				int cols = mat.Cols;
				int rows = mat.Rows;
				mat.Channels();
				double width = cnv_Image.Width;
				double height = cnv_Image.Height;
				double num = width / (double)cols;
				double num2 = height / (double)rows;
				if (num > num2)
				{
					num = num2;
				}
				else
				{
					num2 = num;
				}
				cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)cols * num);
				cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)rows * num2);
				mat = inspection.buffers.LocatedSrc;
				if (r1.Width > 0 && r1.Height > 0)
				{
					mat.Rectangle(r1, Scalar.Red);
				}
				if (r2.Width > 0 && r2.Height > 0)
				{
					mat.Rectangle(r2, Scalar.Red);
				}
				ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
				ImageBrush imageBrush = new ImageBrush();
				imageBrush.ImageSource = imageSource;
				cnv_Image.Background = imageBrush;
				imageSource = null;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("BSW-BlobStatisticsWindows-ShowImage()-ImageListSource = {0}, fileindex = {1}", ImageListSource, fileindex) + ex.ToString());
		}
	}

	private void Btn_SetMinMax_Click(object sender, RoutedEventArgs e)
	{
		if (CheckTextBox())
		{
			ValueMin = Convert.ToDouble(txt_ValueMin.Text);
			ValueMax = Convert.ToDouble(txt_ValueMax.Text);
			DrawChart();
		}
	}

	private bool CheckTextBox()
	{
		bool result = true;
		if (!IsNumeric(txt_ValueMin.Text))
		{
			txt_ValueMin.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ValueMin.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ValueMax.Text))
		{
			txt_ValueMax.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ValueMin.Background = Brushes.White;
		}
		return result;
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
			MessageBox.Show("輸入格式錯誤-" + ex.ToString(), "Error");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Chart_OnDataHover(object sender, ChartPoint p)
	{
		try
		{
			int num = (int)p.X;
			if (num >= 0 && num <= inspection.binaryTool[Index].BlobStatistics[0].Count)
			{
				int imageAddr = inspection.binaryTool[Index].BlobStatistics[0][num].ImageAddr;
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
				rect = inspection.binaryTool[Index].BlobStatistics[0][num].NGRect;
				OpenCvSharp.Rect r = new OpenCvSharp.Rect(0, 0, 0, 0);
				if (TrackNo == 2)
				{
					r = inspection.binaryTool[Index].BlobStatistics[1][num].NGRect;
				}
				if (LocateRect.Width <= 0 || LocateRect.Height <= 0)
				{
					ShowImage(imageAddr, rect, r);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("BSW-BlobStatisticsWindows-Chart_OnDataHover()-id = {0}, Index = {1}, fileIndex = {2}", ID, Index, fileIndex) + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ChartWidth_DataClick(object sender, ChartPoint chartPoint)
	{
		try
		{
			int num = (int)chartPoint.X;
			if (num >= 0 && num <= inspection.binaryTool[Index].BlobStatistics[0].Count)
			{
				int imageAddr = inspection.binaryTool[Index].BlobStatistics[0][num].ImageAddr;
				if (LocateRect.Width <= 0 || LocateRect.Height <= 0)
				{
					LoadImage(imageAddr);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("BSW-BlobStatisticsWindows-ChartWidth_DataClick()") + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LoadImage(int fileindex)
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
				if (filelist == null || fileindex < 0 || fileindex >= filelist.Length || !File.Exists(filelist[fileindex]))
				{
					return;
				}
				mat = CVBuffer.ImRead(filelist[fileindex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (inspection.RecentBufferList == null || fileindex < 0 || inspection.RecentBufferList.Count <= 0 || fileindex >= inspection.RecentBufferList.Count)
				{
					return;
				}
				inspection.SetBuffers(inspection.RecentBufferList[fileindex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (inspection.OKBufferList == null || fileindex < 0 || inspection.OKBufferList.Count <= 0 || fileindex >= inspection.OKBufferList.Count)
				{
					return;
				}
				inspection.SetBuffers(inspection.OKBufferList[fileindex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (inspection.NGBufferList == null || fileindex < 0 || inspection.NGBufferList.Count <= 0 || fileindex >= inspection.NGBufferList.Count)
				{
					return;
				}
				NGBufferIndex = 0;
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat);
			if (inspection.LocaetResult && inspection.AdvlocateResult)
			{
				LocatBuf = inspection.buffers.LocatedSrc;
				LocateRect = inspection.LocateRect;
				lab_Message.Visibility = Visibility.Visible;
				if (LanguageType == 0)
				{
					ShowLabMessage("Loaded Finish!");
				}
				else if (LanguageType == 1)
				{
					ShowLabMessage("載入完成!");
				}
				else if (LanguageType == 2)
				{
					ShowLabMessage("载入完成!");
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("BSW-BlobStatisticsWindows-LoadImage()") + ex.ToString());
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

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/blobstatisticswindow.xaml", UriKind.Relative);
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
			((BlobStatisticsWindow)target).Loaded += Window_Loaded;
			break;
		case 2:
			ChartWidth = (CartesianChart)target;
			ChartWidth.DataHover += Chart_OnDataHover;
			ChartWidth.MouseWheel += ChartWidth_MouseWheel;
			ChartWidth.DataClick += ChartWidth_DataClick;
			break;
		case 3:
			txt_Min = (TextBox)target;
			break;
		case 4:
			txt_Max = (TextBox)target;
			break;
		case 5:
			txt_Std = (TextBox)target;
			break;
		case 6:
			txt_Avg = (TextBox)target;
			break;
		case 7:
			txt_Range = (TextBox)target;
			break;
		case 8:
			lb_ChartName = (Label)target;
			break;
		case 9:
			btn_Next = (Button)target;
			btn_Next.Click += Btn_Next_Click;
			break;
		case 10:
			btn_Forword = (Button)target;
			btn_Forword.Click += Btn_Forword_Click;
			break;
		case 11:
			((Button)target).Click += Button_Click;
			break;
		case 12:
			((Button)target).Click += Button_Click;
			break;
		case 13:
			((Button)target).Click += Button_Click;
			break;
		case 14:
			((Button)target).Click += Button_Click;
			break;
		case 15:
			((Button)target).Click += Button_Click;
			break;
		case 16:
			((Button)target).Click += Button_Click;
			break;
		case 17:
			((Button)target).Click += Button_Click;
			break;
		case 18:
			txt_ValueMin = (TextBox)target;
			break;
		case 19:
			txt_ValueMax = (TextBox)target;
			break;
		case 20:
			btn_SetMinMax = (Button)target;
			btn_SetMinMax.Click += Btn_SetMinMax_Click;
			break;
		case 21:
			cnv_Image = (Canvas)target;
			break;
		case 22:
			sfr = (ScaleTransform)target;
			break;
		case 23:
			line_X1 = (Line)target;
			break;
		case 24:
			line_Y1 = (Line)target;
			break;
		case 25:
			line_X2 = (Line)target;
			break;
		case 26:
			line_Y2 = (Line)target;
			break;
		case 27:
			lab_Message = (Label)target;
			break;
		case 28:
			btn_Close = (Button)target;
			btn_Close.Click += Btn_Close_Click;
			break;
		case 29:
			txt_OK = (TextBox)target;
			break;
		case 30:
			txt_NG = (TextBox)target;
			break;
		case 31:
			txt_Total = (TextBox)target;
			break;
		case 32:
			txt_OKRatio = (TextBox)target;
			break;
		case 33:
			txt_NGRatio = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
