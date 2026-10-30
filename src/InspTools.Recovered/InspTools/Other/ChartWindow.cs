using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace InspTools.Other;

public class ChartWindow : Window, IComponentConnector
{
	private CVInspection Inspection;

	private bool init;

	private int Index;

	private int ID;

	private int LanguageType = 1;

	private int DefalutValue = 1;

	public double ValueMin;

	public double ValueMax;

	public List<SIZESTATIC> SizeStatics;

	private string unitstr = "";

	internal CartesianChart cc_Chart;

	internal TextBox txt_Min;

	internal TextBox txt_Max;

	internal TextBox txt_Sigma;

	internal TextBox txt_Avg;

	internal TextBox txt_Range;

	internal Label lb_ChartName2;

	internal Button btn_NextT;

	internal Button btn_ForwordT;

	private bool _contentLoaded;

	public ChartValues<ObservablePoint> ChartValues { get; set; }

	public ChartWindow()
	{
		InitializeComponent();
		ChartValues = new ChartValues<ObservablePoint>();
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

	public void SetInspection(CVInspection inspection)
	{
		Inspection = inspection;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetStatistics(int tools, int subitem, int item)
	{
		ChartValues.Clear();
		switch (tools)
		{
		case 0:
			switch (subitem)
			{
			case 0:
			{
				for (int m = 0; m < Inspection.SizeStatics.Count; m++)
				{
					ChartValues.Add(new ObservablePoint(m + 1, Inspection.SizeStatics[m].LocateWidth));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Width3"];
				break;
			}
			case 1:
			{
				for (int num2 = 0; num2 < Inspection.SizeStatics.Count; num2++)
				{
					ChartValues.Add(new ObservablePoint(num2 + 1, Inspection.SizeStatics[num2].LocateHeight));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Height3"];
				break;
			}
			case 2:
			{
				for (int n = 0; n < Inspection.SizeStatics.Count; n++)
				{
					ChartValues.Add(new ObservablePoint(n + 1, Inspection.SizeStatics[n].LocateAngle));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Angle"];
				break;
			}
			case 3:
			{
				for (int num = 0; num < Inspection.SizeStatics.Count; num++)
				{
					ChartValues.Add(new ObservablePoint(num + 1, (int)((double)Inspection.SizeStatics[num].LocateCenterX * Inspection.paramFile.PixelPerUM_X)));
				}
				lb_ChartName2.Content = LanguageManager.Instance["CenterX"];
				break;
			}
			case 4:
			{
				for (int l = 0; l < Inspection.SizeStatics.Count; l++)
				{
					ChartValues.Add(new ObservablePoint(l + 1, (int)((double)Inspection.SizeStatics[l].LocateCenterY * Inspection.paramFile.PixelPerUM_Y)));
				}
				lb_ChartName2.Content = LanguageManager.Instance["CenterY"];
				break;
			}
			}
			break;
		case 1:
			if (subitem == 0)
			{
				for (int num4 = 0; num4 < Inspection.SizeStatics.Count; num4++)
				{
					ChartValues.Add(new ObservablePoint(num4 + 1, Inspection.SizeStatics[num4].AdvAWidth[item]));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Width3"];
			}
			else
			{
				for (int num5 = 0; num5 < Inspection.SizeStatics.Count; num5++)
				{
					ChartValues.Add(new ObservablePoint(num5 + 1, Inspection.SizeStatics[num5].AdvAHeight[item]));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Height3"];
			}
			break;
		case 2:
		{
			for (int num6 = 0; num6 < Inspection.SizeStatics.Count; num6++)
			{
				ChartValues.Add(new ObservablePoint(num6 + 1, Inspection.SizeStatics[num6].AdvM[item]));
				lb_ChartName2.Content = LanguageManager.Instance["Distance"];
			}
			break;
		}
		case 3:
			if (subitem == 0)
			{
				for (int j = 0; j < Inspection.SizeStatics.Count; j++)
				{
					ChartValues.Add(new ObservablePoint(j + 1, Inspection.SizeStatics[j].MeasureW[item]));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Width3"];
			}
			else
			{
				for (int k = 0; k < Inspection.SizeStatics.Count; k++)
				{
					ChartValues.Add(new ObservablePoint(k + 1, Inspection.SizeStatics[k].MeasureH[item]));
				}
				lb_ChartName2.Content = LanguageManager.Instance["Height3"];
			}
			break;
		case 4:
		{
			for (int num3 = 0; num3 < Inspection.lightmonitorTool.GetLightMonitorRGB(subitem).Count; num3++)
			{
				ChartValues.Add(new ObservablePoint(num3 + 1, Inspection.lightmonitorTool.GetLightMonitorRGB(subitem)[num3]));
			}
			switch (subitem)
			{
			case 0:
				lb_ChartName2.Content = "R";
				break;
			case 1:
				lb_ChartName2.Content = "G";
				break;
			default:
				lb_ChartName2.Content = "B";
				break;
			}
			break;
		}
		case 5:
		{
			for (int i = 0; i < Inspection.SizeStatics.Count; i++)
			{
				ChartValues.Add(new ObservablePoint(i + 1, Inspection.SizeStatics[i].AIProphetScore));
			}
			lb_ChartName2.Content = LanguageManager.Instance["AIProphetScore"];
			break;
		}
		}
		unitstr = (string)lb_ChartName2.Content;
		CreatChartSeries();
	}

	public void SetStaticValue(List<double> Value, string itemname)
	{
		for (int i = 0; i < Value.Count; i++)
		{
			ChartValues.Add(new ObservablePoint(i + 1, Value[i]));
		}
		lb_ChartName2.Content = itemname;
		CreatChartSeries();
	}

	private void CreatChartSeries()
	{
		CartesianMapper<ObservablePoint> configuration = Mappers.Xy<ObservablePoint>().X((ObservablePoint point) => point.X).Y((ObservablePoint point) => point.Y);
		cc_Chart.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = unitstr,
				Values = ChartValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		CalcSizeStatistics(updateUI: true);
		cc_Chart.AxisX[0].MaxValue = 200.0;
		cc_Chart.AxisX[0].MinValue = 1.0;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CalcSizeStatistics(bool updateUI = false)
	{
		if (ChartValues.Count <= 0)
		{
			return;
		}
		double num = 0.0;
		double num2 = 10000000.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		try
		{
			for (int i = 0; i < ChartValues.Count; i++)
			{
				num = ChartValues[i].Y;
				if (num > num3)
				{
					num3 = num;
				}
				if (num < num2)
				{
					num2 = num;
				}
				num4 += num;
			}
			num5 = num4 / (double)ChartValues.Count;
			num7 = Math.Abs(num3 - num2);
			double num8 = 0.0;
			for (int j = 0; j < ChartValues.Count; j++)
			{
				num8 += Math.Pow(ChartValues[j].Y - num5, 2.0);
			}
			num6 = Math.Sqrt(num8 / (double)ChartValues.Count);
			if (num2 >= 1.0)
			{
				txt_Min.Text = string.Format("{0:N2}", num2);
			}
			else
			{
				txt_Min.Text = string.Format("{0:N5}", num2);
			}
			if (num3 >= 1.0)
			{
				txt_Max.Text = string.Format("{0:N2}", num3);
			}
			else
			{
				txt_Max.Text = string.Format("{0:N5}", num3);
			}
			if (num5 >= 1.0)
			{
				txt_Avg.Text = string.Format("{0:N2}", num5);
			}
			else
			{
				txt_Avg.Text = string.Format("{0:N5}", num5);
			}
			if (num7 >= 1.0)
			{
				txt_Range.Text = string.Format("{0:N2}", num7);
			}
			else
			{
				txt_Range.Text = string.Format("{0:N5}", num7);
			}
			if (num6 >= 1.0)
			{
				txt_Sigma.Text = string.Format("{0:N2}", num6);
			}
			else
			{
				txt_Sigma.Text = string.Format("{0:N5}", num6);
			}
		}
		catch (Exception ex)
		{
			string text = "SW04-ChartWindow-DrawChart()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void Btn_NextT_Click(object sender, RoutedEventArgs e)
	{
		DefalutValue += 200;
		if (DefalutValue >= 900)
		{
			DefalutValue = 1000;
		}
		cc_Chart.AxisX[0].MaxValue = DefalutValue + 200;
		cc_Chart.AxisX[0].MinValue = DefalutValue;
	}

	private void Btn_ForwordT_Click(object sender, RoutedEventArgs e)
	{
		DefalutValue -= 200;
		if (DefalutValue <= 1)
		{
			DefalutValue = 1;
		}
		cc_Chart.AxisX[0].MaxValue = DefalutValue + 200;
		cc_Chart.AxisX[0].MinValue = DefalutValue;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/chartwindow.xaml", UriKind.Relative);
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
			((ChartWindow)target).Loaded += Window_Loaded;
			break;
		case 2:
			cc_Chart = (CartesianChart)target;
			break;
		case 3:
			txt_Min = (TextBox)target;
			break;
		case 4:
			txt_Max = (TextBox)target;
			break;
		case 5:
			txt_Sigma = (TextBox)target;
			break;
		case 6:
			txt_Avg = (TextBox)target;
			break;
		case 7:
			txt_Range = (TextBox)target;
			break;
		case 8:
			lb_ChartName2 = (Label)target;
			break;
		case 9:
			btn_NextT = (Button)target;
			btn_NextT.Click += Btn_NextT_Click;
			break;
		case 10:
			btn_ForwordT = (Button)target;
			btn_ForwordT.Click += Btn_ForwordT_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
