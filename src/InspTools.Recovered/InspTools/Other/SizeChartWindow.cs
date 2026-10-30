using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;

namespace InspTools.Other;

public class SizeChartWindow : Window, IComponentConnector
{
	private CVInspection[] Station;

	private CVInspection StationL;

	private CVInspection StationW;

	private CVInspection StationT;

	private CVInspection StationLPad;

	private CVInspection StationRPad;

	private CVInspection StationGap;

	private CVInspection StationC1;

	private CVInspection StationC2;

	private CVInspection StationC3;

	private CVInspection StationC4;

	private CVInspection StationC5;

	private CVInspection StationC6;

	private CVInspection StationC7;

	private CVInspection StationC8;

	private CVInspection StationC9;

	private CVInspection StationC10;

	private CVInspection StationC11;

	private CVInspection StationC12;

	private CVInspection StationC13;

	private CVInspection StationC14;

	private TOOLSIZERECORDSETUP LSize;

	private TOOLSIZERECORDSETUP WSize;

	private TOOLSIZERECORDSETUP TSize;

	private TOOLSIZERECORDSETUP LPadSize;

	private TOOLSIZERECORDSETUP RPadSize;

	private TOOLSIZERECORDSETUP GapSize;

	private TOOLSIZERECORDSETUP C1Size;

	private TOOLSIZERECORDSETUP C2Size;

	private TOOLSIZERECORDSETUP C3Size;

	private TOOLSIZERECORDSETUP C4Size;

	private TOOLSIZERECORDSETUP C5Size;

	private TOOLSIZERECORDSETUP C6Size;

	private TOOLSIZERECORDSETUP C7Size;

	private TOOLSIZERECORDSETUP C8Size;

	private TOOLSIZERECORDSETUP C9Size;

	private TOOLSIZERECORDSETUP C10Size;

	private TOOLSIZERECORDSETUP C11Size;

	private TOOLSIZERECORDSETUP C12Size;

	private TOOLSIZERECORDSETUP C13Size;

	private TOOLSIZERECORDSETUP C14Size;

	private ThroughputParamFile ThroughputFile;

	public List<OBJSIZE> OBJSizeStatistics;

	private const int MaxMeasureTools = 40;

	private const int MaxTools = 20;

	private const int MaxCustomItem = 20;

	private bool init;

	private int Index;

	private int ID;

	private int DefalutValueL;

	private int DefalutValueW;

	private int DefalutValueT;

	private int DefalutValueLW;

	private int DefalutValueRW;

	private int DefalutValueGap;

	private int BlobType;

	private int LanguageType = 1;

	private int TrackNo = 1;

	public double ValueMin;

	public double ValueMax;

	private string WidthStr = "Width";

	private string HeightStr = "Height";

	private string DistanceStr = "Distance";

	private int CurrentClient;

	private ComboBox[] StationSelect;

	private ComboBox[] ToolSelect;

	private ComboBox[] SubSelect;

	private ComboBox[] ItemSelect;

	internal SizeChartWindow MainW;

	internal MetroTabControl tab_SizeFun;

	internal TabItem tab_Chart;

	internal CartesianChart cc_ChartL;

	internal TextBox txt_MinL;

	internal TextBox txt_MaxL;

	internal TextBox txt_SigmaL;

	internal TextBox txt_AvgL;

	internal TextBox txt_RangeL;

	internal Label lb_ChartName;

	internal Button btn_NextL;

	internal Button btn_ForwordL;

	internal CartesianChart cc_ChartW;

	internal TextBox txt_MinW;

	internal TextBox txt_MaxW;

	internal TextBox txt_SigmaW;

	internal TextBox txt_AvgW;

	internal TextBox txt_RangeW;

	internal Label lb_ChartName1;

	internal Button btn_NextW;

	internal Button btn_ForwordW;

	internal CartesianChart cc_ChartT;

	internal TextBox txt_MinT;

	internal TextBox txt_MaxT;

	internal TextBox txt_SigmaT;

	internal TextBox txt_AvgT;

	internal TextBox txt_RangeT;

	internal Label lb_ChartName2;

	internal Button btn_NextT;

	internal Button btn_ForwordT;

	internal TabItem tab_Custom;

	internal CartesianChart cc_ChartLW;

	internal TextBox txt_MinLW;

	internal TextBox txt_MaxLW;

	internal TextBox txt_SigmaLW;

	internal TextBox txt_AvgLW;

	internal TextBox txt_RangeLW;

	internal Label lb_ChartNameLW;

	internal Button btn_NextLW;

	internal Button btn_ForwordLW;

	internal CartesianChart cc_ChartRW;

	internal TextBox txt_MinRW;

	internal TextBox txt_MaxRW;

	internal TextBox txt_SigmaRW;

	internal TextBox txt_AvgRW;

	internal TextBox txt_RangeRW;

	internal Label lb_ChartNameRW;

	internal Button btn_NextRW;

	internal Button btn_ForwordRW;

	internal CartesianChart cc_ChartGap;

	internal TextBox txt_MinGap;

	internal TextBox txt_MaxGap;

	internal TextBox txt_SigmaGap;

	internal TextBox txt_AvgGap;

	internal TextBox txt_RangeGap;

	internal Label lb_ChartNameGap;

	internal Button btn_NextGap;

	internal Button btn_ForwordGap;

	internal TabItem tab_Setup;

	internal ComboBox cb_StationSelectL;

	internal ComboBox cb_ToolsSelectL;

	internal ComboBox cb_SubSelectL;

	internal ComboBox cb_ItemSelectL;

	internal Button btn_AlarmL;

	internal CheckBox chk_LSizeEnable;

	internal ComboBox cb_StationSelectW;

	internal ComboBox cb_ToolsSelectW;

	internal ComboBox cb_SubSelectW;

	internal ComboBox cb_ItemSelectW;

	internal Button btn_AlarmW;

	internal CheckBox chk_WSizeEnable;

	internal ComboBox cb_StationSelectT;

	internal ComboBox cb_ToolsSelectT;

	internal ComboBox cb_SubSelectT;

	internal ComboBox cb_ItemSelectT;

	internal Button btn_AlarmT;

	internal CheckBox chk_TSizeEnable;

	internal Button btn_QuickSetupSave;

	internal TextBox txt_ComDigitsName0;

	internal TextBox txt_ComDigits0;

	internal TextBox txt_ComDigitsName1;

	internal TextBox txt_ComDigits1;

	internal TextBox txt_ComDigitsName2;

	internal TextBox txt_ComDigits2;

	internal TextBox txt_ComDigitsName3;

	internal TextBox txt_ComDigits3;

	internal TextBox txt_ComDigitsName4;

	internal TextBox txt_ComDigits4;

	internal TextBox txt_ComDigitsName5;

	internal TextBox txt_ComDigits5;

	internal TextBox txt_ComDigitsName6;

	internal TextBox txt_ComDigits6;

	internal TextBox txt_ComDigitsName7;

	internal TextBox txt_ComDigits7;

	internal TextBox txt_ComDigitsName10;

	internal TextBox txt_ComDigits10;

	internal TextBox txt_ComDigitsName11;

	internal TextBox txt_ComDigits11;

	internal TextBox txt_ComDigitsName8;

	internal TextBox txt_ComDigits8;

	internal TextBox txt_ComDigitsName9;

	internal TextBox txt_ComDigits9;

	internal Grid grd_Custom;

	internal TextBox txt_CustomRatio;

	internal TextBox txt_Custom1;

	internal TextBox txt_Custom2;

	internal Button btn_Calc;

	internal CheckBox chk_EnableCustomRatio;

	internal ComboBox cb_StationSelectRW;

	internal ComboBox cb_ToolsSelectRW;

	internal ComboBox cb_SubSelectRW;

	internal ComboBox cb_ItemSelectRW;

	internal Button btn_AlarmRW;

	internal TextBox txt_RPadStr;

	internal CheckBox chk_RPadSizeEnable;

	internal ComboBox cb_StationSelectGap;

	internal ComboBox cb_ToolsSelectGap;

	internal ComboBox cb_SubSelectGap;

	internal ComboBox cb_ItemSelectGap;

	internal Button btn_AlarmGap;

	internal TextBox txt_GapStr;

	internal CheckBox chk_GapSizeEnable;

	internal ComboBox cb_StationSelectLW;

	internal ComboBox cb_ToolsSelectLW;

	internal ComboBox cb_SubSelectLW;

	internal ComboBox cb_ItemSelectLW;

	internal Button btn_AlarmLW;

	internal TextBox txt_LPadStr;

	internal CheckBox chk_LPadSizeEnable;

	internal TabItem tab_Setup2;

	internal ComboBox cb_StationSelectC5;

	internal ComboBox cb_ToolsSelectC5;

	internal ComboBox cb_SubSelectC5;

	internal ComboBox cb_ItemSelectC5;

	internal Button btn_AlarmC5;

	internal TextBox txt_C5Str;

	internal CheckBox chk_C5Enable;

	internal ComboBox cb_StationSelectC6;

	internal ComboBox cb_ToolsSelectC6;

	internal ComboBox cb_SubSelectC6;

	internal ComboBox cb_ItemSelectC6;

	internal Button btn_AlarmC6;

	internal TextBox txt_C6Str;

	internal CheckBox chk_C6Enable;

	internal ComboBox cb_StationSelectC4;

	internal ComboBox cb_ToolsSelectC4;

	internal ComboBox cb_SubSelectC4;

	internal ComboBox cb_ItemSelectC4;

	internal Button btn_AlarmC4;

	internal TextBox txt_C4Str;

	internal CheckBox chk_C4Enable;

	internal ComboBox cb_StationSelectC2;

	internal ComboBox cb_ToolsSelectC2;

	internal ComboBox cb_SubSelectC2;

	internal ComboBox cb_ItemSelectC2;

	internal Button btn_AlarmC2;

	internal TextBox txt_C2Str;

	internal CheckBox chk_C2Enable;

	internal ComboBox cb_StationSelectC3;

	internal ComboBox cb_ToolsSelectC3;

	internal ComboBox cb_SubSelectC3;

	internal ComboBox cb_ItemSelectC3;

	internal Button btn_AlarmC3;

	internal TextBox txt_C3Str;

	internal CheckBox chk_C3Enable;

	internal ComboBox cb_StationSelectC1;

	internal ComboBox cb_ToolsSelectC1;

	internal ComboBox cb_SubSelectC1;

	internal ComboBox cb_ItemSelectC1;

	internal Button btn_AlarmC1;

	internal TextBox txt_C1Str;

	internal CheckBox chk_C1Enable;

	internal Button btn_QuickSetupSave_Copy;

	internal TextBox txt_ComDigitsName12;

	internal TextBox txt_ComDigits12;

	internal TextBox txt_ComDigitsName13;

	internal TextBox txt_ComDigits13;

	internal TextBox txt_ComDigitsName14;

	internal TextBox txt_ComDigits14;

	internal TextBox txt_ComDigitsName15;

	internal TextBox txt_ComDigits15;

	internal TextBox txt_ComDigitsName16;

	internal TextBox txt_ComDigits16;

	internal TextBox txt_ComDigitsName17;

	internal TextBox txt_ComDigits17;

	internal TextBox txt_ComDigitsName18;

	internal TextBox txt_ComDigits18;

	internal TextBox txt_ComDigitsName19;

	internal TextBox txt_ComDigits19;

	internal TextBox txt_ComDigitsName20;

	internal TextBox txt_ComDigits20;

	internal TextBox txt_ComDigitsName21;

	internal TextBox txt_ComDigits21;

	internal TextBox txt_ComDigitsName22;

	internal TextBox txt_ComDigits22;

	internal TextBox txt_ComDigitsName23;

	internal TextBox txt_ComDigits23;

	internal Grid grd_C7;

	internal ComboBox cb_StationSelectC7;

	internal ComboBox cb_ToolsSelectC7;

	internal ComboBox cb_SubSelectC7;

	internal ComboBox cb_ItemSelectC7;

	internal Button btn_AlarmC7;

	internal TextBox txt_C7Str;

	internal CheckBox chk_C7Enable;

	internal TabItem tab_Setup3;

	internal ComboBox cb_StationSelectC8;

	internal ComboBox cb_ToolsSelectC8;

	internal ComboBox cb_SubSelectC8;

	internal ComboBox cb_ItemSelectC8;

	internal Button btn_AlarmC8;

	internal TextBox txt_C8Str;

	internal CheckBox chk_C8Enable;

	internal ComboBox cb_StationSelectC9;

	internal ComboBox cb_ToolsSelectC9;

	internal ComboBox cb_SubSelectC9;

	internal ComboBox cb_ItemSelectC9;

	internal Button btn_AlarmC9;

	internal TextBox txt_C9Str;

	internal CheckBox chk_C9Enable;

	internal ComboBox cb_StationSelectC10;

	internal ComboBox cb_ToolsSelectC10;

	internal ComboBox cb_SubSelectC10;

	internal ComboBox cb_ItemSelectC10;

	internal Button btn_AlarmC10;

	internal TextBox txt_C10Str;

	internal CheckBox chk_C10Enable;

	internal ComboBox cb_StationSelectC11;

	internal ComboBox cb_ToolsSelectC11;

	internal ComboBox cb_SubSelectC11;

	internal ComboBox cb_ItemSelectC11;

	internal Button btn_AlarmC11;

	internal TextBox txt_C11Str;

	internal CheckBox chk_C11Enable;

	internal ComboBox cb_StationSelectC12;

	internal ComboBox cb_ToolsSelectC12;

	internal ComboBox cb_SubSelectC12;

	internal ComboBox cb_ItemSelectC12;

	internal Button btn_AlarmC12;

	internal TextBox txt_C12Str;

	internal CheckBox chk_C12Enable;

	internal ComboBox cb_StationSelectC13;

	internal ComboBox cb_ToolsSelectC13;

	internal ComboBox cb_SubSelectC13;

	internal ComboBox cb_ItemSelectC13;

	internal Button btn_AlarmC13;

	internal TextBox txt_C13Str;

	internal CheckBox chk_C13Enable;

	internal ComboBox cb_StationSelectC14;

	internal ComboBox cb_ToolsSelectC14;

	internal ComboBox cb_SubSelectC14;

	internal ComboBox cb_ItemSelectC14;

	internal Button btn_AlarmC14;

	internal TextBox txt_C14Str;

	internal CheckBox chk_C14Enable;

	internal Button btn_QuickSetupSave_Copy1;

	private bool _contentLoaded;

	private ChartValues<ObservablePoint> ChartLValues { get; set; }

	private ChartValues<ObservablePoint> ChartWValues { get; set; }

	private ChartValues<ObservablePoint> ChartTValues { get; set; }

	private ChartValues<ObservablePoint> ChartLWValues { get; set; }

	private ChartValues<ObservablePoint> ChartRWValues { get; set; }

	private ChartValues<ObservablePoint> ChartGapValues { get; set; }

	[MethodImpl(MethodImplOptions.NoInlining)]
	public SizeChartWindow()
	{
		InitializeComponent();
		StationSelect = new ComboBox[20]
		{
			cb_StationSelectL, cb_StationSelectW, cb_StationSelectT, cb_StationSelectLW, cb_StationSelectRW, cb_StationSelectGap, cb_StationSelectC1, cb_StationSelectC2, cb_StationSelectC3, cb_StationSelectC4,
			cb_StationSelectC5, cb_StationSelectC6, cb_StationSelectC7, cb_StationSelectC8, cb_StationSelectC9, cb_StationSelectC10, cb_StationSelectC11, cb_StationSelectC12, cb_StationSelectC13, cb_StationSelectC14
		};
		ToolSelect = new ComboBox[20]
		{
			cb_ToolsSelectL, cb_ToolsSelectW, cb_ToolsSelectT, cb_ToolsSelectLW, cb_ToolsSelectRW, cb_ToolsSelectGap, cb_ToolsSelectC1, cb_ToolsSelectC2, cb_ToolsSelectC3, cb_ToolsSelectC4,
			cb_ToolsSelectC5, cb_ToolsSelectC6, cb_ToolsSelectC7, cb_ToolsSelectC8, cb_ToolsSelectC9, cb_ToolsSelectC10, cb_ToolsSelectC11, cb_ToolsSelectC12, cb_ToolsSelectC13, cb_ToolsSelectC14
		};
		SubSelect = new ComboBox[20]
		{
			cb_SubSelectL, cb_SubSelectW, cb_SubSelectT, cb_SubSelectLW, cb_SubSelectRW, cb_SubSelectGap, cb_SubSelectC1, cb_SubSelectC2, cb_SubSelectC3, cb_SubSelectC4,
			cb_SubSelectC5, cb_SubSelectC6, cb_SubSelectC7, cb_SubSelectC8, cb_SubSelectC9, cb_SubSelectC10, cb_SubSelectC11, cb_SubSelectC12, cb_SubSelectC13, cb_SubSelectC14
		};
		ItemSelect = new ComboBox[20]
		{
			cb_ItemSelectL, cb_ItemSelectW, cb_ItemSelectT, cb_ItemSelectLW, cb_ItemSelectRW, cb_ItemSelectGap, cb_ItemSelectC1, cb_ItemSelectC2, cb_ItemSelectC3, cb_ItemSelectC4,
			cb_ItemSelectC5, cb_ItemSelectC6, cb_ItemSelectC7, cb_ItemSelectC8, cb_ItemSelectC9, cb_ItemSelectC10, cb_ItemSelectC11, cb_ItemSelectC12, cb_ItemSelectC13, cb_ItemSelectC14
		};
		init = true;
		btn_AlarmL.Visibility = Visibility.Hidden;
		btn_AlarmW.Visibility = Visibility.Hidden;
		btn_AlarmT.Visibility = Visibility.Hidden;
		btn_AlarmLW.Visibility = Visibility.Hidden;
		btn_AlarmRW.Visibility = Visibility.Hidden;
		btn_AlarmGap.Visibility = Visibility.Hidden;
		btn_AlarmC1.Visibility = Visibility.Hidden;
		btn_AlarmC2.Visibility = Visibility.Hidden;
		btn_AlarmC3.Visibility = Visibility.Hidden;
		btn_AlarmC4.Visibility = Visibility.Hidden;
		btn_AlarmC5.Visibility = Visibility.Hidden;
		btn_AlarmC6.Visibility = Visibility.Hidden;
		btn_AlarmC7.Visibility = Visibility.Hidden;
		btn_AlarmC8.Visibility = Visibility.Hidden;
		btn_AlarmC9.Visibility = Visibility.Hidden;
		btn_AlarmC10.Visibility = Visibility.Hidden;
		btn_AlarmC11.Visibility = Visibility.Hidden;
		btn_AlarmC12.Visibility = Visibility.Hidden;
		btn_AlarmC13.Visibility = Visibility.Hidden;
		btn_AlarmC14.Visibility = Visibility.Hidden;
		tab_Chart.Background = Brushes.Yellow;
		grd_Custom.Visibility = Visibility.Hidden;
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
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		LanguageType = language;
		switch (language)
		{
		case 1:
			_ = baseDirectory + "StringResources.zh-TW.xaml";
			WidthStr = "寬度";
			HeightStr = "高度";
			DistanceStr = "距離";
			break;
		case 2:
			_ = baseDirectory + "StringResources.zh-CN.xaml";
			WidthStr = "宽度";
			HeightStr = "高度";
			DistanceStr = "距离";
			break;
		case 3:
			_ = baseDirectory + "StringResources.ja-JP.xaml";
			WidthStr = "幅";
			HeightStr = "高さ";
			DistanceStr = "距離";
			break;
		default:
			_ = baseDirectory + "StringResources.en-US.xaml";
			WidthStr = "Width";
			HeightStr = "Height";
			DistanceStr = "Distance";
			break;
		}
	}

	public void SetInspection(CVInspection[] station)
	{
		Station = station;
	}

	public void SetRecipe(ThroughputParamFile file)
	{
		ThroughputFile = file;
		CopyValueToUI();
		CreatChartSeries();
	}

	public ThroughputParamFile GetRecipe()
	{
		return ThroughputFile;
	}

	public void SetOJBSizes(List<OBJSIZE> OBJSizes)
	{
		OBJSizeStatistics = OBJSizes;
	}

	public void SetClientCode(int code)
	{
		CurrentClient = code;
		if (CurrentClient == 3)
		{
			tab_Setup3.Visibility = Visibility.Hidden;
			grd_C7.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CreatChartSeries()
	{
		CartesianMapper<ObservablePoint> configuration = Mappers.Xy<ObservablePoint>().X((ObservablePoint point) => point.X).Y((ObservablePoint point) => point.Y);
		ChartLValues = new ChartValues<ObservablePoint>();
		ChartWValues = new ChartValues<ObservablePoint>();
		ChartTValues = new ChartValues<ObservablePoint>();
		ChartLWValues = new ChartValues<ObservablePoint>();
		ChartRWValues = new ChartValues<ObservablePoint>();
		ChartGapValues = new ChartValues<ObservablePoint>();
		cc_ChartL.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = LanguageManager.Instance["SizeL"],
				Values = ChartLValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		cc_ChartW.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = LanguageManager.Instance["SizeW"],
				Values = ChartWValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		cc_ChartT.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = LanguageManager.Instance["SizeT"],
				Values = ChartTValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		cc_ChartLW.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = txt_LPadStr.Text,
				Values = ChartLWValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		cc_ChartRW.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = txt_RPadStr.Text,
				Values = ChartRWValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		cc_ChartGap.Series = new SeriesCollection(configuration)
		{
			new LineSeries
			{
				Title = txt_GapStr.Text,
				Values = ChartGapValues,
				PointGeometrySize = 5.0,
				Fill = Brushes.Transparent,
				Stroke = Brushes.Blue,
				LineSmoothness = 5.0
			}
		};
		ChartLValues.Clear();
		ChartWValues.Clear();
		ChartTValues.Clear();
		ChartLWValues.Clear();
		ChartRWValues.Clear();
		ChartGapValues.Clear();
		CalcSizeStatistics(updateUI: true);
		for (int num = 0; num < OBJSizeStatistics.Count; num++)
		{
			ChartLValues.Add(new ObservablePoint(num, OBJSizeStatistics[num].LSize));
			ChartWValues.Add(new ObservablePoint(num, OBJSizeStatistics[num].WSize));
			ChartTValues.Add(new ObservablePoint(num, OBJSizeStatistics[num].TSize));
			ChartLWValues.Add(new ObservablePoint(num, OBJSizeStatistics[num].LPadWidth));
			ChartRWValues.Add(new ObservablePoint(num, OBJSizeStatistics[num].RPadWidth));
			ChartGapValues.Add(new ObservablePoint(num, OBJSizeStatistics[num].Gap));
		}
		cc_ChartL.AxisX[0].MaxValue = 200.0;
		cc_ChartL.AxisX[0].MinValue = 0.0;
		cc_ChartW.AxisX[0].MaxValue = 200.0;
		cc_ChartW.AxisX[0].MinValue = 0.0;
		cc_ChartT.AxisX[0].MaxValue = 200.0;
		cc_ChartT.AxisX[0].MinValue = 0.0;
		cc_ChartLW.AxisX[0].MaxValue = 200.0;
		cc_ChartLW.AxisX[0].MinValue = 0.0;
		cc_ChartRW.AxisX[0].MaxValue = 200.0;
		cc_ChartRW.AxisX[0].MinValue = 0.0;
		cc_ChartGap.AxisX[0].MaxValue = 200.0;
		cc_ChartGap.AxisX[0].MinValue = 0.0;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private double GetSizeValue(CVInspection insp, TOOLSIZERECORDSETUP ss, int index)
	{
		try
		{
			switch (ss.ToolSelect)
			{
			case 0:
				if (ss.SubSelect == 0)
				{
					return insp.SizeStatics[index].LocateWidth;
				}
				return insp.SizeStatics[index].LocateHeight;
			case 1:
				if (ss.ItemSelect >= 0 && ss.ItemSelect < 20)
				{
					if (ss.SubSelect == 0)
					{
						return insp.SizeStatics[index].AdvAWidth[ss.ItemSelect];
					}
					return insp.SizeStatics[index].AdvAHeight[ss.ItemSelect];
				}
				return 0.0;
			case 2:
				if (ss.ItemSelect >= 0 && ss.ItemSelect < 40)
				{
					return insp.SizeStatics[index].AdvM[ss.ItemSelect];
				}
				return 0.0;
			case 3:
				if (ss.ItemSelect >= 0 && ss.ItemSelect < 20)
				{
					if (ss.SubSelect == 0)
					{
						return insp.SizeStatics[index].MeasureW[ss.ItemSelect];
					}
					return insp.SizeStatics[index].MeasureH[ss.ItemSelect];
				}
				return 0.0;
			default:
				return 0.0;
			}
		}
		catch (Exception ex)
		{
			string text = "SW03-SizeChartWindow-GetSizeValue()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return 0.0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CalcSizeStatistics(bool updateUI = false)
	{
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		int num8 = 0;
		double num9 = 0.0;
		double num10 = 0.0;
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		double num15 = 0.0;
		double num16 = 0.0;
		double num17 = 0.0;
		double num18 = 0.0;
		double num19 = 0.0;
		double num20 = 0.0;
		double num21 = 0.0;
		double num22 = 0.0;
		double num23 = 10000000.0;
		double num24 = 10000000.0;
		double num25 = 10000000.0;
		double num26 = 10000000.0;
		double num27 = 10000000.0;
		double num28 = 10000000.0;
		double num29 = 0.0;
		double num30 = 0.0;
		double num31 = 0.0;
		double num32 = 0.0;
		double num33 = 0.0;
		double num34 = 0.0;
		double num35 = 0.0;
		double num36 = 0.0;
		double num37 = 0.0;
		double num38 = 0.0;
		double num39 = 0.0;
		double num40 = 0.0;
		double num41 = 0.0;
		double num42 = 0.0;
		double num43 = 0.0;
		double num44 = 0.0;
		double num45 = 0.0;
		double num46 = 0.0;
		double num47 = 0.0;
		double num48 = 0.0;
		double num49 = 0.0;
		double num50 = 0.0;
		double num51 = 0.0;
		double num52 = 0.0;
		double num53 = 0.0;
		double num54 = 0.0;
		double num55 = 0.0;
		double num56 = 0.0;
		double num57 = 0.0;
		double num58 = 0.0;
		double num59 = 10000000.0;
		double num60 = 0.0;
		double num61 = 0.0;
		double num62 = 0.0;
		double num63 = 10000000.0;
		double num64 = 0.0;
		double num65 = 0.0;
		double num66 = 0.0;
		double num67 = 10000000.0;
		double num68 = 0.0;
		double num69 = 0.0;
		double num70 = 0.0;
		double num71 = 10000000.0;
		double num72 = 0.0;
		double num73 = 0.0;
		double num74 = 0.0;
		double num75 = 10000000.0;
		double num76 = 0.0;
		double num77 = 0.0;
		double num78 = 0.0;
		double num79 = 10000000.0;
		double num80 = 0.0;
		double num81 = 0.0;
		double num82 = 0.0;
		double num83 = 10000000.0;
		double num84 = 0.0;
		double num85 = 0.0;
		double num86 = 0.0;
		double num87 = 10000000.0;
		double num88 = 0.0;
		double num89 = 0.0;
		double num90 = 0.0;
		double num91 = 10000000.0;
		double num92 = 0.0;
		double num93 = 0.0;
		double num94 = 0.0;
		double num95 = 10000000.0;
		double num96 = 0.0;
		double num97 = 0.0;
		double num98 = 0.0;
		double num99 = 10000000.0;
		double num100 = 0.0;
		double num101 = 0.0;
		double num102 = 0.0;
		double num103 = 10000000.0;
		double num104 = 0.0;
		double num105 = 0.0;
		double num106 = 0.0;
		double num107 = 10000000.0;
		double num108 = 0.0;
		double num109 = 0.0;
		double num110 = 0.0;
		double num111 = 10000000.0;
		double num112 = 0.0;
		double num113 = 0.0;
		double num114 = 0.0;
		try
		{
			if (OBJSizeStatistics == null)
			{
				return;
			}
			if (!updateUI)
			{
				LSize.StationNo = ThroughputFile.param.LSizeStation;
				LSize.ToolSelect = ThroughputFile.param.LSizeToolSelect;
				LSize.SubSelect = ThroughputFile.param.LSizeSubSelect;
				LSize.ItemSelect = ThroughputFile.param.LSizeItemSelect;
				StationL = Station[LSize.StationNo];
				WSize.StationNo = ThroughputFile.param.WSizeStation;
				WSize.ToolSelect = ThroughputFile.param.WSizeToolSelect;
				WSize.SubSelect = ThroughputFile.param.WSizeSubSelect;
				WSize.ItemSelect = ThroughputFile.param.WSizeItemSelect;
				StationW = Station[WSize.StationNo];
				TSize.StationNo = ThroughputFile.param.TSizeStation;
				TSize.ToolSelect = ThroughputFile.param.TSizeToolSelect;
				TSize.SubSelect = ThroughputFile.param.TSizeSubSelect;
				TSize.ItemSelect = ThroughputFile.param.TSizeItemSelect;
				StationT = Station[TSize.StationNo];
				LPadSize.StationNo = ThroughputFile.param.LPadSizeStation;
				LPadSize.ToolSelect = ThroughputFile.param.LPadSizeToolSelect;
				LPadSize.SubSelect = ThroughputFile.param.LPadSizeSubSelect;
				LPadSize.ItemSelect = ThroughputFile.param.LPadSizeItemSelect;
				StationLPad = Station[LPadSize.StationNo];
				RPadSize.StationNo = ThroughputFile.param.RPadSizeStation;
				RPadSize.ToolSelect = ThroughputFile.param.RPadSizeToolSelect;
				RPadSize.SubSelect = ThroughputFile.param.RPadSizeSubSelect;
				RPadSize.ItemSelect = ThroughputFile.param.RPadSizeItemSelect;
				StationRPad = Station[RPadSize.StationNo];
				GapSize.StationNo = ThroughputFile.param.GapSizeStation;
				GapSize.ToolSelect = ThroughputFile.param.GapSizeToolSelect;
				GapSize.SubSelect = ThroughputFile.param.GapSizeSubSelect;
				GapSize.ItemSelect = ThroughputFile.param.GapSizeItemSelect;
				StationGap = Station[GapSize.StationNo];
				C1Size.StationNo = ThroughputFile.param.C1SizeStation;
				C1Size.ToolSelect = ThroughputFile.param.C1SizeToolSelect;
				C1Size.SubSelect = ThroughputFile.param.C1SizeSubSelect;
				C1Size.ItemSelect = ThroughputFile.param.C1SizeItemSelect;
				StationC1 = Station[C1Size.StationNo];
				C2Size.StationNo = ThroughputFile.param.C2SizeStation;
				C2Size.ToolSelect = ThroughputFile.param.C2SizeToolSelect;
				C2Size.SubSelect = ThroughputFile.param.C2SizeSubSelect;
				C2Size.ItemSelect = ThroughputFile.param.C2SizeItemSelect;
				StationC2 = Station[C2Size.StationNo];
				C3Size.StationNo = ThroughputFile.param.C3SizeStation;
				C3Size.ToolSelect = ThroughputFile.param.C3SizeToolSelect;
				C3Size.SubSelect = ThroughputFile.param.C3SizeSubSelect;
				C3Size.ItemSelect = ThroughputFile.param.C3SizeItemSelect;
				StationC3 = Station[C3Size.StationNo];
				C4Size.StationNo = ThroughputFile.param.C4SizeStation;
				C4Size.ToolSelect = ThroughputFile.param.C4SizeToolSelect;
				C4Size.SubSelect = ThroughputFile.param.C4SizeSubSelect;
				C4Size.ItemSelect = ThroughputFile.param.C4SizeItemSelect;
				StationC4 = Station[C4Size.StationNo];
				C5Size.StationNo = ThroughputFile.param.C5SizeStation;
				C5Size.ToolSelect = ThroughputFile.param.C5SizeToolSelect;
				C5Size.SubSelect = ThroughputFile.param.C5SizeSubSelect;
				C5Size.ItemSelect = ThroughputFile.param.C5SizeItemSelect;
				StationC5 = Station[C5Size.StationNo];
				C6Size.StationNo = ThroughputFile.param.C6SizeStation;
				C6Size.ToolSelect = ThroughputFile.param.C6SizeToolSelect;
				C6Size.SubSelect = ThroughputFile.param.C6SizeSubSelect;
				C6Size.ItemSelect = ThroughputFile.param.C6SizeItemSelect;
				StationC6 = Station[C6Size.StationNo];
				C7Size.StationNo = ThroughputFile.param.C7SizeStation;
				C7Size.ToolSelect = ThroughputFile.param.C7SizeToolSelect;
				C7Size.SubSelect = ThroughputFile.param.C7SizeSubSelect;
				C7Size.ItemSelect = ThroughputFile.param.C7SizeItemSelect;
				StationC7 = Station[C7Size.StationNo];
				C8Size.StationNo = ThroughputFile.param.C8SizeStation;
				C8Size.ToolSelect = ThroughputFile.param.C8SizeToolSelect;
				C8Size.SubSelect = ThroughputFile.param.C8SizeSubSelect;
				C8Size.ItemSelect = ThroughputFile.param.C8SizeItemSelect;
				StationC8 = Station[C8Size.StationNo];
				C9Size.StationNo = ThroughputFile.param.C9SizeStation;
				C9Size.ToolSelect = ThroughputFile.param.C9SizeToolSelect;
				C9Size.SubSelect = ThroughputFile.param.C9SizeSubSelect;
				C9Size.ItemSelect = ThroughputFile.param.C9SizeItemSelect;
				StationC9 = Station[C9Size.StationNo];
				C10Size.StationNo = ThroughputFile.param.C10SizeStation;
				C10Size.ToolSelect = ThroughputFile.param.C10SizeToolSelect;
				C10Size.SubSelect = ThroughputFile.param.C10SizeSubSelect;
				C10Size.ItemSelect = ThroughputFile.param.C10SizeItemSelect;
				StationC10 = Station[C10Size.StationNo];
				C11Size.StationNo = ThroughputFile.param.C11SizeStation;
				C11Size.ToolSelect = ThroughputFile.param.C11SizeToolSelect;
				C11Size.SubSelect = ThroughputFile.param.C11SizeSubSelect;
				C11Size.ItemSelect = ThroughputFile.param.C11SizeItemSelect;
				StationC11 = Station[C11Size.StationNo];
				C12Size.StationNo = ThroughputFile.param.C12SizeStation;
				C12Size.ToolSelect = ThroughputFile.param.C12SizeToolSelect;
				C12Size.SubSelect = ThroughputFile.param.C12SizeSubSelect;
				C12Size.ItemSelect = ThroughputFile.param.C12SizeItemSelect;
				StationC12 = Station[C12Size.StationNo];
				C13Size.StationNo = ThroughputFile.param.C13SizeStation;
				C13Size.ToolSelect = ThroughputFile.param.C13SizeToolSelect;
				C13Size.SubSelect = ThroughputFile.param.C13SizeSubSelect;
				C13Size.ItemSelect = ThroughputFile.param.C13SizeItemSelect;
				StationC13 = Station[C13Size.StationNo];
				C14Size.StationNo = ThroughputFile.param.C14SizeStation;
				C14Size.ToolSelect = ThroughputFile.param.C14SizeToolSelect;
				C14Size.SubSelect = ThroughputFile.param.C14SizeSubSelect;
				C14Size.ItemSelect = ThroughputFile.param.C14SizeItemSelect;
				StationC14 = Station[C14Size.StationNo];
			}
			OBJSizeStatistics.Clear();
			OBJSIZE item = default(OBJSIZE);
			for (int i = 0; i < StationL.SizeStatics.Count; i++)
			{
				num3 = (num7 = (num2 = -1.0));
				num4 = (num5 = (num6 = 0.0));
				num2 = GetSizeValue(StationL, LSize, i);
				num = StationL.SizeStatics[i].ID;
				for (int j = 0; j < StationW.SizeStatics.Count; j++)
				{
					if (num == StationW.SizeStatics[j].ID)
					{
						num3 = GetSizeValue(StationW, WSize, j);
						break;
					}
				}
				for (int k = 0; k < StationT.SizeStatics.Count; k++)
				{
					if (num == StationT.SizeStatics[k].ID)
					{
						num7 = GetSizeValue(StationT, TSize, k);
						break;
					}
				}
				for (int l = 0; l < StationLPad.SizeStatics.Count; l++)
				{
					if (num == StationLPad.SizeStatics[l].ID)
					{
						num4 = GetSizeValue(StationLPad, LPadSize, l);
						break;
					}
				}
				for (int m = 0; m < StationRPad.SizeStatics.Count; m++)
				{
					if (num == StationRPad.SizeStatics[m].ID)
					{
						num5 = GetSizeValue(StationRPad, RPadSize, m);
						break;
					}
				}
				for (int n = 0; n < StationGap.SizeStatics.Count; n++)
				{
					if (num == StationGap.SizeStatics[n].ID)
					{
						num6 = GetSizeValue(StationGap, GapSize, n);
						break;
					}
				}
				for (int num115 = 0; num115 < StationC1.SizeStatics.Count; num115++)
				{
					if (num == StationC1.SizeStatics[num115].ID)
					{
						num9 = GetSizeValue(StationC1, C1Size, num115);
						break;
					}
				}
				for (int num116 = 0; num116 < StationC2.SizeStatics.Count; num116++)
				{
					if (num == StationC2.SizeStatics[num116].ID)
					{
						num10 = GetSizeValue(StationC2, C2Size, num116);
						break;
					}
				}
				for (int num117 = 0; num117 < StationC3.SizeStatics.Count; num117++)
				{
					if (num == StationC3.SizeStatics[num117].ID)
					{
						num11 = GetSizeValue(StationC3, C3Size, num117);
						break;
					}
				}
				for (int num118 = 0; num118 < StationC4.SizeStatics.Count; num118++)
				{
					if (num == StationC4.SizeStatics[num118].ID)
					{
						num12 = GetSizeValue(StationC4, C4Size, num118);
						break;
					}
				}
				for (int num119 = 0; num119 < StationC5.SizeStatics.Count; num119++)
				{
					if (num == StationC5.SizeStatics[num119].ID)
					{
						num13 = GetSizeValue(StationC5, C5Size, num119);
						break;
					}
				}
				for (int num120 = 0; num120 < StationC6.SizeStatics.Count; num120++)
				{
					if (num == StationC6.SizeStatics[num120].ID)
					{
						num14 = GetSizeValue(StationC6, C6Size, num120);
						break;
					}
				}
				for (int num121 = 0; num121 < StationC7.SizeStatics.Count; num121++)
				{
					if (num == StationC7.SizeStatics[num121].ID)
					{
						num15 = GetSizeValue(StationC7, C7Size, num121);
						break;
					}
				}
				for (int num122 = 0; num122 < StationC8.SizeStatics.Count; num122++)
				{
					if (num == StationC8.SizeStatics[num122].ID)
					{
						num16 = GetSizeValue(StationC8, C8Size, num122);
						break;
					}
				}
				for (int num123 = 0; num123 < StationC9.SizeStatics.Count; num123++)
				{
					if (num == StationC9.SizeStatics[num123].ID)
					{
						num17 = GetSizeValue(StationC9, C9Size, num123);
						break;
					}
				}
				for (int num124 = 0; num124 < StationC10.SizeStatics.Count; num124++)
				{
					if (num == StationC10.SizeStatics[num124].ID)
					{
						num18 = GetSizeValue(StationC10, C10Size, num124);
						break;
					}
				}
				for (int num125 = 0; num125 < StationC11.SizeStatics.Count; num125++)
				{
					if (num == StationC11.SizeStatics[num125].ID)
					{
						num19 = GetSizeValue(StationC11, C11Size, num125);
						break;
					}
				}
				for (int num126 = 0; num126 < StationC12.SizeStatics.Count; num126++)
				{
					if (num == StationC12.SizeStatics[num126].ID)
					{
						num20 = GetSizeValue(StationC12, C12Size, num126);
						break;
					}
				}
				for (int num127 = 0; num127 < StationC13.SizeStatics.Count; num127++)
				{
					if (num == StationC13.SizeStatics[num127].ID)
					{
						num21 = GetSizeValue(StationC13, C13Size, num127);
						break;
					}
				}
				for (int num128 = 0; num128 < StationC14.SizeStatics.Count; num128++)
				{
					if (num == StationC14.SizeStatics[num128].ID)
					{
						num22 = GetSizeValue(StationC14, C14Size, num128);
						break;
					}
				}
				if (num2 > 0.0 && num3 > 0.0 && num7 > 0.0)
				{
					item.ID = num;
					item.LSize = num2;
					item.WSize = num3;
					item.TSize = num7;
					item.LPadWidth = num4;
					item.RPadWidth = num5;
					item.Gap = num6;
					item.C1 = num9;
					item.C2 = num10;
					item.C3 = num11;
					item.C4 = num12;
					item.C5 = num13;
					item.C6 = num14;
					item.C7 = num15;
					item.C8 = num16;
					item.C9 = num17;
					item.C10 = num18;
					item.C11 = num19;
					item.C12 = num20;
					item.C13 = num21;
					item.C14 = num22;
					if (OBJSizeStatistics.Count >= 1000)
					{
						OBJSizeStatistics.RemoveAt(OBJSizeStatistics.Count - 1);
					}
					OBJSizeStatistics.Add(item);
					num8++;
					if (num2 > num29)
					{
						num29 = num2;
					}
					if (num2 < num23)
					{
						num23 = num2;
					}
					num35 += num2;
					if (num3 > num30)
					{
						num30 = num3;
					}
					if (num3 < num24)
					{
						num24 = num3;
					}
					num36 += num3;
					if (num7 > num31)
					{
						num31 = num7;
					}
					if (num7 < num25)
					{
						num25 = num7;
					}
					num37 += num7;
					if (num4 > num32)
					{
						num32 = num4;
					}
					if (num4 < num26)
					{
						num26 = num4;
					}
					num38 += num4;
					if (num5 > num33)
					{
						num33 = num5;
					}
					if (num5 < num27)
					{
						num27 = num5;
					}
					num39 += num5;
					if (num6 > num34)
					{
						num34 = num6;
					}
					if (num6 < num28)
					{
						num28 = num6;
					}
					num40 += num6;
					if (num9 > num60)
					{
						num60 = num9;
					}
					if (num9 < num59)
					{
						num59 = num9;
					}
					num61 += num9;
					if (num10 > num64)
					{
						num64 = num10;
					}
					if (num10 < num63)
					{
						num63 = num10;
					}
					num65 += num10;
					if (num11 > num68)
					{
						num68 = num11;
					}
					if (num11 < num67)
					{
						num67 = num11;
					}
					num69 += num11;
					if (num12 > num72)
					{
						num72 = num12;
					}
					if (num12 < num71)
					{
						num71 = num12;
					}
					num73 += num12;
					if (num13 > num76)
					{
						num76 = num13;
					}
					if (num13 < num75)
					{
						num75 = num13;
					}
					num77 += num13;
					if (num14 > num80)
					{
						num80 = num14;
					}
					if (num14 < num79)
					{
						num79 = num14;
					}
					num81 += num14;
					if (num15 > num84)
					{
						num84 = num15;
					}
					if (num15 < num83)
					{
						num83 = num15;
					}
					num85 += num15;
					if (num16 > num88)
					{
						num88 = num16;
					}
					if (num16 < num87)
					{
						num87 = num16;
					}
					num89 += num16;
					if (num17 > num92)
					{
						num92 = num17;
					}
					if (num17 < num91)
					{
						num91 = num17;
					}
					num93 += num17;
					if (num18 > num96)
					{
						num96 = num18;
					}
					if (num18 < num95)
					{
						num95 = num18;
					}
					num97 += num18;
					if (num19 > num100)
					{
						num100 = num19;
					}
					if (num19 < num99)
					{
						num99 = num19;
					}
					num101 += num19;
					if (num20 > num104)
					{
						num104 = num20;
					}
					if (num20 < num103)
					{
						num103 = num20;
					}
					num105 += num20;
					if (num21 > num108)
					{
						num108 = num21;
					}
					if (num21 < num107)
					{
						num107 = num21;
					}
					num109 += num21;
					if (num22 > num112)
					{
						num112 = num22;
					}
					if (num22 < num111)
					{
						num111 = num22;
					}
					num113 += num22;
				}
			}
			if (num8 <= 0 || OBJSizeStatistics.Count <= 0)
			{
				return;
			}
			num41 = num35 / (double)num8;
			num42 = num36 / (double)num8;
			num43 = num37 / (double)num8;
			num44 = num38 / (double)num8;
			num45 = num39 / (double)num8;
			num46 = num40 / (double)num8;
			num53 = Math.Abs(num29 - num23);
			num54 = Math.Abs(num30 - num24);
			num55 = Math.Abs(num31 - num25);
			num56 = Math.Abs(num32 - num26);
			num57 = Math.Abs(num33 - num27);
			num58 = Math.Abs(num34 - num28);
			num62 = num61 / (double)num8;
			Math.Abs(num60 - num59);
			num66 = num65 / (double)num8;
			Math.Abs(num64 - num63);
			num70 = num69 / (double)num8;
			Math.Abs(num68 - num67);
			num74 = num73 / (double)num8;
			Math.Abs(num72 - num71);
			num78 = num77 / (double)num8;
			Math.Abs(num76 - num75);
			num82 = num81 / (double)num8;
			Math.Abs(num80 - num79);
			num86 = num85 / (double)num8;
			Math.Abs(num84 - num83);
			num90 = num89 / (double)num8;
			Math.Abs(num88 - num87);
			num94 = num93 / (double)num8;
			Math.Abs(num92 - num91);
			num98 = num97 / (double)num8;
			Math.Abs(num96 - num95);
			num102 = num101 / (double)num8;
			Math.Abs(num100 - num99);
			num106 = num105 / (double)num8;
			Math.Abs(num104 - num103);
			num110 = num109 / (double)num8;
			Math.Abs(num108 - num107);
			num114 = num113 / (double)num8;
			Math.Abs(num112 - num111);
			double num129 = 0.0;
			double num130 = 0.0;
			double num131 = 0.0;
			double num132 = 0.0;
			double num133 = 0.0;
			double num134 = 0.0;
			double num135 = 0.0;
			double num136 = 0.0;
			double num137 = 0.0;
			double num138 = 0.0;
			double num139 = 0.0;
			double num140 = 0.0;
			double num141 = 0.0;
			double num142 = 0.0;
			double num143 = 0.0;
			double num144 = 0.0;
			double num145 = 0.0;
			double num146 = 0.0;
			double num147 = 0.0;
			double num148 = 0.0;
			for (int num149 = 0; num149 < OBJSizeStatistics.Count; num149++)
			{
				num129 += Math.Pow(OBJSizeStatistics[num149].LSize - num41, 2.0);
				num130 += Math.Pow(OBJSizeStatistics[num149].WSize - num42, 2.0);
				num131 += Math.Pow(OBJSizeStatistics[num149].TSize - num43, 2.0);
				num132 += Math.Pow(OBJSizeStatistics[num149].LPadWidth - num44, 2.0);
				num133 += Math.Pow(OBJSizeStatistics[num149].RPadWidth - num45, 2.0);
				num134 += Math.Pow(OBJSizeStatistics[num149].Gap - num46, 2.0);
				num135 += Math.Pow(OBJSizeStatistics[num149].C1 - num62, 2.0);
				num136 += Math.Pow(OBJSizeStatistics[num149].C2 - num66, 2.0);
				num137 += Math.Pow(OBJSizeStatistics[num149].C3 - num70, 2.0);
				num138 += Math.Pow(OBJSizeStatistics[num149].C4 - num74, 2.0);
				num139 += Math.Pow(OBJSizeStatistics[num149].C5 - num78, 2.0);
				num140 += Math.Pow(OBJSizeStatistics[num149].C6 - num82, 2.0);
				num141 += Math.Pow(OBJSizeStatistics[num149].C7 - num86, 2.0);
				num142 += Math.Pow(OBJSizeStatistics[num149].C8 - num90, 2.0);
				num143 += Math.Pow(OBJSizeStatistics[num149].C9 - num94, 2.0);
				num144 += Math.Pow(OBJSizeStatistics[num149].C10 - num98, 2.0);
				num145 += Math.Pow(OBJSizeStatistics[num149].C11 - num102, 2.0);
				num146 += Math.Pow(OBJSizeStatistics[num149].C12 - num106, 2.0);
				num147 += Math.Pow(OBJSizeStatistics[num149].C13 - num110, 2.0);
				num148 += Math.Pow(OBJSizeStatistics[num149].C14 - num114, 2.0);
			}
			num47 = Math.Sqrt(num129 / (double)OBJSizeStatistics.Count);
			num48 = Math.Sqrt(num130 / (double)OBJSizeStatistics.Count);
			num49 = Math.Sqrt(num131 / (double)OBJSizeStatistics.Count);
			num50 = Math.Sqrt(num132 / (double)OBJSizeStatistics.Count);
			num51 = Math.Sqrt(num133 / (double)OBJSizeStatistics.Count);
			num52 = Math.Sqrt(num134 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num135 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num136 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num137 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num138 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num139 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num140 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num141 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num142 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num143 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num144 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num145 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num146 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num147 / (double)OBJSizeStatistics.Count);
			Math.Sqrt(num148 / (double)OBJSizeStatistics.Count);
			if (updateUI)
			{
				if (num23 >= 1.0)
				{
					txt_MinL.Text = string.Format("{0:N2}", num23);
				}
				else
				{
					txt_MinL.Text = string.Format("{0:N5}", num23);
				}
				if (num24 >= 1.0)
				{
					txt_MinW.Text = string.Format("{0:N2}", num24);
				}
				else
				{
					txt_MinW.Text = string.Format("{0:N5}", num24);
				}
				if (num25 >= 1.0)
				{
					txt_MinT.Text = string.Format("{0:N2}", num25);
				}
				else
				{
					txt_MinT.Text = string.Format("{0:N5}", num25);
				}
				if (num26 >= 1.0)
				{
					txt_MinLW.Text = string.Format("{0:N2}", num26);
				}
				else
				{
					txt_MinLW.Text = string.Format("{0:N5}", num26);
				}
				if (num27 >= 1.0)
				{
					txt_MinRW.Text = string.Format("{0:N2}", num27);
				}
				else
				{
					txt_MinRW.Text = string.Format("{0:N5}", num27);
				}
				if (num28 >= 1.0)
				{
					txt_MinGap.Text = string.Format("{0:N2}", num28);
				}
				else
				{
					txt_MinGap.Text = string.Format("{0:N5}", num28);
				}
				if (num29 >= 1.0)
				{
					txt_MaxL.Text = string.Format("{0:N2}", num29);
				}
				else
				{
					txt_MaxL.Text = string.Format("{0:N5}", num29);
				}
				if (num30 >= 1.0)
				{
					txt_MaxW.Text = string.Format("{0:N2}", num30);
				}
				else
				{
					txt_MaxW.Text = string.Format("{0:N5}", num30);
				}
				if (num31 >= 1.0)
				{
					txt_MaxT.Text = string.Format("{0:N2}", num31);
				}
				else
				{
					txt_MaxT.Text = string.Format("{0:N5}", num31);
				}
				if (num32 >= 1.0)
				{
					txt_MaxLW.Text = string.Format("{0:N2}", num32);
				}
				else
				{
					txt_MaxLW.Text = string.Format("{0:N5}", num32);
				}
				if (num33 >= 1.0)
				{
					txt_MaxRW.Text = string.Format("{0:N2}", num33);
				}
				else
				{
					txt_MaxRW.Text = string.Format("{0:N5}", num33);
				}
				if (num34 >= 1.0)
				{
					txt_MaxGap.Text = string.Format("{0:N2}", num34);
				}
				else
				{
					txt_MaxGap.Text = string.Format("{0:N5}", num34);
				}
				if (num41 >= 1.0)
				{
					txt_AvgL.Text = string.Format("{0:N2}", num41);
				}
				else
				{
					txt_AvgL.Text = string.Format("{0:N5}", num41);
				}
				if (num42 >= 1.0)
				{
					txt_AvgW.Text = string.Format("{0:N2}", num42);
				}
				else
				{
					txt_AvgW.Text = string.Format("{0:N5}", num42);
				}
				if (num43 >= 1.0)
				{
					txt_AvgT.Text = string.Format("{0:N2}", num43);
				}
				else
				{
					txt_AvgT.Text = string.Format("{0:N5}", num43);
				}
				if (num44 >= 1.0)
				{
					txt_AvgLW.Text = string.Format("{0:N2}", num44);
				}
				else
				{
					txt_AvgLW.Text = string.Format("{0:N5}", num44);
				}
				if (num45 >= 1.0)
				{
					txt_AvgRW.Text = string.Format("{0:N2}", num45);
				}
				else
				{
					txt_AvgRW.Text = string.Format("{0:N5}", num45);
				}
				if (num46 >= 1.0)
				{
					txt_AvgGap.Text = string.Format("{0:N2}", num46);
				}
				else
				{
					txt_AvgGap.Text = string.Format("{0:N5}", num46);
				}
				if (num53 >= 1.0)
				{
					txt_RangeL.Text = string.Format("{0:N2}", num53);
				}
				else
				{
					txt_RangeL.Text = string.Format("{0:N5}", num53);
				}
				if (num54 >= 1.0)
				{
					txt_RangeW.Text = string.Format("{0:N2}", num54);
				}
				else
				{
					txt_RangeW.Text = string.Format("{0:N5}", num54);
				}
				if (num55 >= 1.0)
				{
					txt_RangeT.Text = string.Format("{0:N2}", num55);
				}
				else
				{
					txt_RangeT.Text = string.Format("{0:N5}", num55);
				}
				if (num56 >= 1.0)
				{
					txt_RangeLW.Text = string.Format("{0:N2}", num56);
				}
				else
				{
					txt_RangeLW.Text = string.Format("{0:N5}", num56);
				}
				if (num57 >= 1.0)
				{
					txt_RangeRW.Text = string.Format("{0:N2}", num57);
				}
				else
				{
					txt_RangeRW.Text = string.Format("{0:N5}", num57);
				}
				if (num58 >= 1.0)
				{
					txt_RangeGap.Text = string.Format("{0:N2}", num58);
				}
				else
				{
					txt_RangeGap.Text = string.Format("{0:N5}", num58);
				}
				if (num47 >= 1.0)
				{
					txt_SigmaL.Text = string.Format("{0:N2}", num47);
				}
				else
				{
					txt_SigmaL.Text = string.Format("{0:N5}", num47);
				}
				if (num48 >= 1.0)
				{
					txt_SigmaW.Text = string.Format("{0:N2}", num48);
				}
				else
				{
					txt_SigmaW.Text = string.Format("{0:N5}", num48);
				}
				if (num49 >= 1.0)
				{
					txt_SigmaT.Text = string.Format("{0:N2}", num49);
				}
				else
				{
					txt_SigmaT.Text = string.Format("{0:N5}", num49);
				}
				if (num50 >= 1.0)
				{
					txt_SigmaLW.Text = string.Format("{0:N2}", num50);
				}
				else
				{
					txt_SigmaLW.Text = string.Format("{0:N5}", num50);
				}
				if (num51 >= 1.0)
				{
					txt_SigmaRW.Text = string.Format("{0:N2}", num51);
				}
				else
				{
					txt_SigmaRW.Text = string.Format("{0:N5}", num51);
				}
				if (num52 >= 1.0)
				{
					txt_SigmaGap.Text = string.Format("{0:N2}", num52);
				}
				else
				{
					txt_SigmaGap.Text = string.Format("{0:N5}", num52);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "SW04-SizeChartWindow-DrawChart()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void Btn_Next_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueL += 200;
		if (DefalutValueL >= 900)
		{
			DefalutValueL = 1000;
		}
		cc_ChartL.AxisX[0].MaxValue = DefalutValueL + 200;
		cc_ChartL.AxisX[0].MinValue = DefalutValueL;
	}

	private void Btn_Forword_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueL -= 200;
		if (DefalutValueL <= 0)
		{
			DefalutValueL = 0;
		}
		cc_ChartL.AxisX[0].MaxValue = DefalutValueL + 200;
		cc_ChartL.AxisX[0].MinValue = DefalutValueL;
	}

	private void Chart_OnDataHover(object sender, ChartPoint p)
	{
	}

	private void cc_ChartL_DataClick(object sender, ChartPoint chartPoint)
	{
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_StationSelectL_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!init)
		{
			return;
		}
		ComboBox obj = (ComboBox)sender;
		int num = 0;
		string uid = obj.Uid;
		if (uid != null)
		{
			switch (uid.Length)
			{
			case 1:
				switch (uid[0])
				{
				case 'L':
					StationL = Station[cb_StationSelectL.SelectedIndex];
					num = 0;
					break;
				case 'W':
					StationW = Station[cb_StationSelectW.SelectedIndex];
					num = 1;
					break;
				case 'T':
					StationT = Station[cb_StationSelectT.SelectedIndex];
					num = 2;
					break;
				case 'G':
					StationGap = Station[cb_StationSelectGap.SelectedIndex];
					num = 5;
					break;
				}
				break;
			case 2:
				switch (uid[1])
				{
				case 'W':
					if (!(uid == "LW"))
					{
						if (uid == "RW")
						{
							StationRPad = Station[cb_StationSelectRW.SelectedIndex];
							num = 4;
						}
					}
					else
					{
						StationLPad = Station[cb_StationSelectLW.SelectedIndex];
						num = 3;
					}
					break;
				case '1':
					if (uid == "C1")
					{
						StationC1 = Station[cb_StationSelectC1.SelectedIndex];
						num = 6;
					}
					break;
				case '2':
					if (uid == "C2")
					{
						StationC2 = Station[cb_StationSelectC2.SelectedIndex];
						num = 7;
					}
					break;
				case '3':
					if (uid == "C3")
					{
						StationC3 = Station[cb_StationSelectC3.SelectedIndex];
						num = 8;
					}
					break;
				case '4':
					if (uid == "C4")
					{
						StationC4 = Station[cb_StationSelectC4.SelectedIndex];
						num = 9;
					}
					break;
				case '5':
					if (uid == "C5")
					{
						StationC5 = Station[cb_StationSelectC5.SelectedIndex];
						num = 10;
					}
					break;
				case '6':
					if (uid == "C6")
					{
						StationC6 = Station[cb_StationSelectC6.SelectedIndex];
						num = 11;
					}
					break;
				case '7':
					if (uid == "C7")
					{
						StationC7 = Station[cb_StationSelectC7.SelectedIndex];
						num = 12;
					}
					break;
				case '8':
					if (uid == "C8")
					{
						StationC8 = Station[cb_StationSelectC8.SelectedIndex];
						num = 13;
					}
					break;
				case '9':
					if (uid == "C9")
					{
						StationC9 = Station[cb_StationSelectC9.SelectedIndex];
						num = 14;
					}
					break;
				}
				break;
			case 3:
				switch (uid[2])
				{
				case '0':
					if (uid == "C10")
					{
						StationC10 = Station[cb_StationSelectC10.SelectedIndex];
						num = 15;
					}
					break;
				case '1':
					if (uid == "C11")
					{
						StationC11 = Station[cb_StationSelectC11.SelectedIndex];
						num = 16;
					}
					break;
				case '2':
					if (uid == "C12")
					{
						StationC12 = Station[cb_StationSelectC12.SelectedIndex];
						num = 17;
					}
					break;
				case '3':
					if (uid == "C13")
					{
						StationC13 = Station[cb_StationSelectC13.SelectedIndex];
						num = 18;
					}
					break;
				case '4':
					if (uid == "C14")
					{
						StationC14 = Station[cb_StationSelectC14.SelectedIndex];
						num = 19;
					}
					break;
				}
				break;
			}
		}
		ToolSelect[num].SelectedIndex = 0;
		SubSelect[num].Items.Clear();
		SubSelect[num].Items.Add(WidthStr);
		SubSelect[num].Items.Add(HeightStr);
		SubSelect[num].SelectedIndex = 0;
		ItemSelect[num].Items.Clear();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_ToolsSelectL_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!init)
		{
			return;
		}
		try
		{
			ComboBox obj = (ComboBox)sender;
			CVInspection cVInspection = null;
			int selectedIndex = obj.SelectedIndex;
			int num = 0;
			string uid = obj.Uid;
			if (uid != null)
			{
				switch (uid.Length)
				{
				case 1:
					switch (uid[0])
					{
					case 'L':
						if (StationL == null)
						{
							return;
						}
						num = 0;
						cVInspection = StationL;
						break;
					case 'W':
						if (StationW == null)
						{
							return;
						}
						num = 1;
						cVInspection = StationW;
						break;
					case 'T':
						if (StationT == null)
						{
							return;
						}
						num = 2;
						cVInspection = StationT;
						break;
					case 'G':
						if (StationGap == null)
						{
							return;
						}
						num = 5;
						cVInspection = StationGap;
						break;
					}
					break;
				case 2:
					switch (uid[1])
					{
					case 'W':
						if (!(uid == "LW"))
						{
							if (uid == "RW")
							{
								if (StationRPad == null)
								{
									return;
								}
								num = 4;
								cVInspection = StationRPad;
							}
						}
						else
						{
							if (StationLPad == null)
							{
								return;
							}
							num = 3;
							cVInspection = StationLPad;
						}
						break;
					case '1':
						if (uid == "C1")
						{
							if (StationC1 == null)
							{
								return;
							}
							num = 6;
							cVInspection = StationC1;
						}
						break;
					case '2':
						if (uid == "C2")
						{
							if (StationC2 == null)
							{
								return;
							}
							num = 7;
							cVInspection = StationC2;
						}
						break;
					case '3':
						if (uid == "C3")
						{
							if (StationC3 == null)
							{
								return;
							}
							num = 8;
							cVInspection = StationC3;
						}
						break;
					case '4':
						if (uid == "C4")
						{
							if (StationC4 == null)
							{
								return;
							}
							num = 9;
							cVInspection = StationC4;
						}
						break;
					case '5':
						if (uid == "C5")
						{
							if (StationC5 == null)
							{
								return;
							}
							num = 10;
							cVInspection = StationC5;
						}
						break;
					case '6':
						if (uid == "C6")
						{
							if (StationC6 == null)
							{
								return;
							}
							num = 11;
							cVInspection = StationC6;
						}
						break;
					case '7':
						if (uid == "C7")
						{
							if (StationC7 == null)
							{
								return;
							}
							num = 12;
							cVInspection = StationC7;
						}
						break;
					case '8':
						if (uid == "C8")
						{
							if (StationC8 == null)
							{
								return;
							}
							num = 13;
							cVInspection = StationC8;
						}
						break;
					case '9':
						if (uid == "C9")
						{
							if (StationC9 == null)
							{
								return;
							}
							num = 14;
							cVInspection = StationC9;
						}
						break;
					}
					break;
				case 3:
					switch (uid[2])
					{
					case '0':
						if (uid == "C10")
						{
							if (StationC10 == null)
							{
								return;
							}
							num = 15;
							cVInspection = StationC10;
						}
						break;
					case '1':
						if (uid == "C11")
						{
							if (StationC11 == null)
							{
								return;
							}
							num = 16;
							cVInspection = StationC11;
						}
						break;
					case '2':
						if (uid == "C12")
						{
							if (StationC12 == null)
							{
								return;
							}
							num = 17;
							cVInspection = StationC12;
						}
						break;
					case '3':
						if (uid == "C13")
						{
							if (StationC13 == null)
							{
								return;
							}
							num = 18;
							cVInspection = StationC13;
						}
						break;
					case '4':
						if (uid == "C14")
						{
							if (StationC14 == null)
							{
								return;
							}
							num = 19;
							cVInspection = StationC14;
						}
						break;
					}
					break;
				}
			}
			switch (selectedIndex)
			{
			case 0:
				SubSelect[num].Items.Clear();
				SubSelect[num].Items.Add(WidthStr);
				SubSelect[num].Items.Add(HeightStr);
				SubSelect[num].SelectedIndex = 0;
				ItemSelect[num].Items.Clear();
				break;
			case 1:
			{
				SubSelect[num].Items.Clear();
				SubSelect[num].Items.Add(WidthStr);
				SubSelect[num].Items.Add(HeightStr);
				SubSelect[num].SelectedIndex = 0;
				ItemSelect[num].Items.Clear();
				for (int j = 0; j < 20; j++)
				{
					ComboBoxItem comboBoxItem2 = new ComboBoxItem();
					comboBoxItem2.Content = j + 1 + " : " + cVInspection.paramFile.AdvLocateParam[j].TrackerName;
					comboBoxItem2.IsEnabled = cVInspection.paramFile.AdvLocateParam[j].TrackerON;
					ItemSelect[num].Items.Add(comboBoxItem2);
				}
				break;
			}
			case 2:
			{
				SubSelect[num].Items.Clear();
				SubSelect[num].Items.Add(DistanceStr);
				SubSelect[num].SelectedIndex = 0;
				ItemSelect[num].Items.Clear();
				for (int k = 0; k < 40; k++)
				{
					ComboBoxItem comboBoxItem3 = new ComboBoxItem();
					comboBoxItem3.Content = k + 1 + " : " + cVInspection.paramFile.AdvLocateMParam[k].TrackerName;
					comboBoxItem3.IsEnabled = cVInspection.paramFile.AdvLocateMParam[k].TrackerON;
					ItemSelect[num].Items.Add(comboBoxItem3);
				}
				break;
			}
			case 3:
			{
				SubSelect[num].Items.Clear();
				SubSelect[num].Items.Add(WidthStr);
				SubSelect[num].Items.Add(HeightStr);
				SubSelect[num].SelectedIndex = 0;
				ItemSelect[num].Items.Clear();
				for (int i = 0; i < 20; i++)
				{
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = i + 1 + " : " + cVInspection.paramFile.MeaWidthParam[i].TrackerName;
					comboBoxItem.IsEnabled = cVInspection.paramFile.MeaWidthParam[i].TrackerON;
					ItemSelect[num].Items.Add(comboBoxItem);
				}
				break;
			}
			}
			ItemSelect[num].SelectedIndex = -1;
		}
		catch (Exception ex)
		{
			string text = "SW01-SizeChartWindow-cb_ToolsSelectL_SelectionChanged()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_SubSelectL_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!init)
		{
			return;
		}
		try
		{
			ComboBox comboBox = (ComboBox)sender;
			CVInspection cVInspection = null;
			int selectedIndex = comboBox.SelectedIndex;
			int num = 0;
			if (comboBox.Uid == "L")
			{
				if (StationL == null)
				{
					return;
				}
				num = 0;
				cVInspection = StationL;
			}
			else if (comboBox.Uid == "W")
			{
				if (StationW == null)
				{
					return;
				}
				num = 1;
				cVInspection = StationW;
			}
			else if (comboBox.Uid == "T")
			{
				if (StationT == null)
				{
					return;
				}
				num = 2;
				cVInspection = StationT;
			}
			else if (comboBox.Uid == "LW")
			{
				if (StationLPad == null)
				{
					return;
				}
				num = 3;
				cVInspection = StationLPad;
			}
			else if (comboBox.Uid == "RW")
			{
				if (StationRPad == null)
				{
					return;
				}
				num = 4;
				cVInspection = StationRPad;
			}
			else if (comboBox.Uid == "G")
			{
				if (StationGap == null)
				{
					return;
				}
				num = 5;
				cVInspection = StationGap;
			}
			else if (comboBox.Uid == "C1")
			{
				if (StationC1 == null)
				{
					return;
				}
				num = 6;
				cVInspection = StationC1;
			}
			else if (comboBox.Uid == "C2")
			{
				if (StationC2 == null)
				{
					return;
				}
				num = 7;
				cVInspection = StationC2;
			}
			else if (comboBox.Uid == "C3")
			{
				if (StationC3 == null)
				{
					return;
				}
				num = 8;
				cVInspection = StationC3;
			}
			else if (comboBox.Uid == "C4")
			{
				if (StationC4 == null)
				{
					return;
				}
				num = 9;
				cVInspection = StationC4;
			}
			else if (comboBox.Uid == "C5")
			{
				if (StationC5 == null)
				{
					return;
				}
				num = 10;
				cVInspection = StationC5;
			}
			else if (comboBox.Uid == "C6")
			{
				if (StationC6 == null)
				{
					return;
				}
				num = 11;
				cVInspection = StationC6;
			}
			else if (comboBox.Uid == "C7")
			{
				if (StationC7 == null)
				{
					return;
				}
				num = 12;
				cVInspection = StationC7;
			}
			else if (comboBox.Uid == "C8")
			{
				if (StationC8 == null)
				{
					return;
				}
				num = 13;
				cVInspection = StationC8;
			}
			else if (comboBox.Uid == "C9")
			{
				if (StationC9 == null)
				{
					return;
				}
				num = 14;
				cVInspection = StationC9;
			}
			else if (comboBox.Uid == "C10")
			{
				if (StationC10 == null)
				{
					return;
				}
				num = 15;
				cVInspection = StationC10;
			}
			else if (comboBox.Uid == "C11")
			{
				if (StationC11 == null)
				{
					return;
				}
				num = 16;
				cVInspection = StationC11;
			}
			else if (comboBox.Uid == "C12")
			{
				if (StationC12 == null)
				{
					return;
				}
				num = 17;
				cVInspection = StationC12;
			}
			else if (comboBox.Uid == "C13")
			{
				if (StationC13 == null)
				{
					return;
				}
				num = 18;
				cVInspection = StationC13;
			}
			else
			{
				if (StationC14 == null)
				{
					return;
				}
				num = 19;
				cVInspection = StationC14;
			}
			if (ToolSelect[num].SelectedIndex == 1 || ToolSelect[num].SelectedIndex == 2 || ToolSelect[num].SelectedIndex == 0 || ToolSelect[num].SelectedIndex != 3)
			{
				return;
			}
			switch (selectedIndex)
			{
			case 0:
			{
				ItemSelect[num].Items.Clear();
				for (int j = 0; j < 20; j++)
				{
					ComboBoxItem comboBoxItem2 = new ComboBoxItem();
					comboBoxItem2.Content = j + 1 + " : " + cVInspection.paramFile.MeaWidthParam[j].TrackerName;
					comboBoxItem2.IsEnabled = cVInspection.paramFile.MeaWidthParam[j].TrackerON;
					ItemSelect[num].Items.Add(comboBoxItem2);
				}
				break;
			}
			case 1:
			{
				ItemSelect[num].Items.Clear();
				for (int i = 0; i < 20; i++)
				{
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = i + 1 + " : " + cVInspection.paramFile.MeaHeightParam[i].TrackerName;
					comboBoxItem.IsEnabled = cVInspection.paramFile.MeaHeightParam[i].TrackerON;
					ItemSelect[num].Items.Add(comboBoxItem);
				}
				break;
			}
			}
			ItemSelect[num].SelectedIndex = -1;
		}
		catch (Exception ex)
		{
			string text = "SW01-SizeChartWindow-Cb_SubSelectL_SelectionChanged()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void CopyValueToUI()
	{
		if (Station != null && ThroughputFile != null)
		{
			chk_LSizeEnable.IsChecked = ThroughputFile.param.LSizeEnable;
			LSize.StationNo = ThroughputFile.param.LSizeStation;
			LSize.ToolSelect = ThroughputFile.param.LSizeToolSelect;
			LSize.SubSelect = ThroughputFile.param.LSizeSubSelect;
			LSize.ItemSelect = ThroughputFile.param.LSizeItemSelect;
			StationL = Station[LSize.StationNo];
			UpdateComboBox(0, LSize.StationNo, LSize.ToolSelect, LSize.SubSelect, LSize.ItemSelect);
			chk_WSizeEnable.IsChecked = ThroughputFile.param.WSizeEnable;
			WSize.StationNo = ThroughputFile.param.WSizeStation;
			WSize.ToolSelect = ThroughputFile.param.WSizeToolSelect;
			WSize.SubSelect = ThroughputFile.param.WSizeSubSelect;
			WSize.ItemSelect = ThroughputFile.param.WSizeItemSelect;
			StationW = Station[WSize.StationNo];
			UpdateComboBox(1, WSize.StationNo, WSize.ToolSelect, WSize.SubSelect, WSize.ItemSelect);
			chk_TSizeEnable.IsChecked = ThroughputFile.param.TSizeEnable;
			TSize.StationNo = ThroughputFile.param.TSizeStation;
			TSize.ToolSelect = ThroughputFile.param.TSizeToolSelect;
			TSize.SubSelect = ThroughputFile.param.TSizeSubSelect;
			TSize.ItemSelect = ThroughputFile.param.TSizeItemSelect;
			StationT = Station[TSize.StationNo];
			UpdateComboBox(2, TSize.StationNo, TSize.ToolSelect, TSize.SubSelect, TSize.ItemSelect);
			chk_LPadSizeEnable.IsChecked = ThroughputFile.param.LPadSizeEnable;
			LPadSize.StationNo = ThroughputFile.param.LPadSizeStation;
			LPadSize.ToolSelect = ThroughputFile.param.LPadSizeToolSelect;
			LPadSize.SubSelect = ThroughputFile.param.LPadSizeSubSelect;
			LPadSize.ItemSelect = ThroughputFile.param.LPadSizeItemSelect;
			StationLPad = Station[LPadSize.StationNo];
			UpdateComboBox(3, LPadSize.StationNo, LPadSize.ToolSelect, LPadSize.SubSelect, LPadSize.ItemSelect);
			chk_RPadSizeEnable.IsChecked = ThroughputFile.param.RPadSizeEnable;
			RPadSize.StationNo = ThroughputFile.param.RPadSizeStation;
			RPadSize.ToolSelect = ThroughputFile.param.RPadSizeToolSelect;
			RPadSize.SubSelect = ThroughputFile.param.RPadSizeSubSelect;
			RPadSize.ItemSelect = ThroughputFile.param.RPadSizeItemSelect;
			StationRPad = Station[RPadSize.StationNo];
			UpdateComboBox(4, RPadSize.StationNo, RPadSize.ToolSelect, RPadSize.SubSelect, RPadSize.ItemSelect);
			chk_GapSizeEnable.IsChecked = ThroughputFile.param.GapSizeEnable;
			GapSize.StationNo = ThroughputFile.param.GapSizeStation;
			GapSize.ToolSelect = ThroughputFile.param.GapSizeToolSelect;
			GapSize.SubSelect = ThroughputFile.param.GapSizeSubSelect;
			GapSize.ItemSelect = ThroughputFile.param.GapSizeItemSelect;
			StationGap = Station[GapSize.StationNo];
			UpdateComboBox(5, GapSize.StationNo, GapSize.ToolSelect, GapSize.SubSelect, GapSize.ItemSelect);
			chk_C1Enable.IsChecked = ThroughputFile.param.C1Enable;
			C1Size.StationNo = ThroughputFile.param.C1SizeStation;
			C1Size.ToolSelect = ThroughputFile.param.C1SizeToolSelect;
			C1Size.SubSelect = ThroughputFile.param.C1SizeSubSelect;
			C1Size.ItemSelect = ThroughputFile.param.C1SizeItemSelect;
			StationC1 = Station[C1Size.StationNo];
			UpdateComboBox(6, C1Size.StationNo, C1Size.ToolSelect, C1Size.SubSelect, C1Size.ItemSelect);
			txt_C1Str.Text = ThroughputFile.param.C1Str;
			chk_C2Enable.IsChecked = ThroughputFile.param.C2Enable;
			C2Size.StationNo = ThroughputFile.param.C2SizeStation;
			C2Size.ToolSelect = ThroughputFile.param.C2SizeToolSelect;
			C2Size.SubSelect = ThroughputFile.param.C2SizeSubSelect;
			C2Size.ItemSelect = ThroughputFile.param.C2SizeItemSelect;
			StationC2 = Station[C2Size.StationNo];
			UpdateComboBox(7, C2Size.StationNo, C2Size.ToolSelect, C2Size.SubSelect, C2Size.ItemSelect);
			txt_C2Str.Text = ThroughputFile.param.C2Str;
			chk_C3Enable.IsChecked = ThroughputFile.param.C3Enable;
			C3Size.StationNo = ThroughputFile.param.C3SizeStation;
			C3Size.ToolSelect = ThroughputFile.param.C3SizeToolSelect;
			C3Size.SubSelect = ThroughputFile.param.C3SizeSubSelect;
			C3Size.ItemSelect = ThroughputFile.param.C3SizeItemSelect;
			StationC3 = Station[C3Size.StationNo];
			UpdateComboBox(8, C3Size.StationNo, C3Size.ToolSelect, C3Size.SubSelect, C3Size.ItemSelect);
			txt_C3Str.Text = ThroughputFile.param.C3Str;
			chk_C4Enable.IsChecked = ThroughputFile.param.C4Enable;
			C4Size.StationNo = ThroughputFile.param.C4SizeStation;
			C4Size.ToolSelect = ThroughputFile.param.C4SizeToolSelect;
			C4Size.SubSelect = ThroughputFile.param.C4SizeSubSelect;
			C4Size.ItemSelect = ThroughputFile.param.C4SizeItemSelect;
			StationC4 = Station[C4Size.StationNo];
			UpdateComboBox(9, C4Size.StationNo, C4Size.ToolSelect, C4Size.SubSelect, C4Size.ItemSelect);
			txt_C4Str.Text = ThroughputFile.param.C4Str;
			chk_C5Enable.IsChecked = ThroughputFile.param.C5Enable;
			C5Size.StationNo = ThroughputFile.param.C5SizeStation;
			C5Size.ToolSelect = ThroughputFile.param.C5SizeToolSelect;
			C5Size.SubSelect = ThroughputFile.param.C5SizeSubSelect;
			C5Size.ItemSelect = ThroughputFile.param.C5SizeItemSelect;
			StationC5 = Station[C5Size.StationNo];
			UpdateComboBox(10, C5Size.StationNo, C5Size.ToolSelect, C5Size.SubSelect, C5Size.ItemSelect);
			txt_C5Str.Text = ThroughputFile.param.C5Str;
			chk_C6Enable.IsChecked = ThroughputFile.param.C6Enable;
			C6Size.StationNo = ThroughputFile.param.C6SizeStation;
			C6Size.ToolSelect = ThroughputFile.param.C6SizeToolSelect;
			C6Size.SubSelect = ThroughputFile.param.C6SizeSubSelect;
			C6Size.ItemSelect = ThroughputFile.param.C6SizeItemSelect;
			StationC6 = Station[C6Size.StationNo];
			UpdateComboBox(11, C6Size.StationNo, C6Size.ToolSelect, C6Size.SubSelect, C6Size.ItemSelect);
			txt_C6Str.Text = ThroughputFile.param.C6Str;
			chk_C7Enable.IsChecked = ThroughputFile.param.C7Enable;
			C7Size.StationNo = ThroughputFile.param.C7SizeStation;
			C7Size.ToolSelect = ThroughputFile.param.C7SizeToolSelect;
			C7Size.SubSelect = ThroughputFile.param.C7SizeSubSelect;
			C7Size.ItemSelect = ThroughputFile.param.C7SizeItemSelect;
			StationC7 = Station[C7Size.StationNo];
			UpdateComboBox(12, C7Size.StationNo, C7Size.ToolSelect, C7Size.SubSelect, C7Size.ItemSelect);
			txt_C7Str.Text = ThroughputFile.param.C7Str;
			chk_C8Enable.IsChecked = ThroughputFile.param.C8Enable;
			C8Size.StationNo = ThroughputFile.param.C8SizeStation;
			C8Size.ToolSelect = ThroughputFile.param.C8SizeToolSelect;
			C8Size.SubSelect = ThroughputFile.param.C8SizeSubSelect;
			C8Size.ItemSelect = ThroughputFile.param.C8SizeItemSelect;
			StationC8 = Station[C8Size.StationNo];
			UpdateComboBox(13, C8Size.StationNo, C8Size.ToolSelect, C8Size.SubSelect, C8Size.ItemSelect);
			txt_C8Str.Text = ThroughputFile.param.C8Str;
			chk_C9Enable.IsChecked = ThroughputFile.param.C9Enable;
			C9Size.StationNo = ThroughputFile.param.C9SizeStation;
			C9Size.ToolSelect = ThroughputFile.param.C9SizeToolSelect;
			C9Size.SubSelect = ThroughputFile.param.C9SizeSubSelect;
			C9Size.ItemSelect = ThroughputFile.param.C9SizeItemSelect;
			StationC9 = Station[C9Size.StationNo];
			UpdateComboBox(14, C9Size.StationNo, C9Size.ToolSelect, C9Size.SubSelect, C9Size.ItemSelect);
			txt_C9Str.Text = ThroughputFile.param.C9Str;
			chk_C10Enable.IsChecked = ThroughputFile.param.C10Enable;
			C10Size.StationNo = ThroughputFile.param.C10SizeStation;
			C10Size.ToolSelect = ThroughputFile.param.C10SizeToolSelect;
			C10Size.SubSelect = ThroughputFile.param.C10SizeSubSelect;
			C10Size.ItemSelect = ThroughputFile.param.C10SizeItemSelect;
			StationC10 = Station[C10Size.StationNo];
			UpdateComboBox(15, C10Size.StationNo, C10Size.ToolSelect, C10Size.SubSelect, C10Size.ItemSelect);
			txt_C10Str.Text = ThroughputFile.param.C10Str;
			chk_C11Enable.IsChecked = ThroughputFile.param.C11Enable;
			C11Size.StationNo = ThroughputFile.param.C11SizeStation;
			C11Size.ToolSelect = ThroughputFile.param.C11SizeToolSelect;
			C11Size.SubSelect = ThroughputFile.param.C11SizeSubSelect;
			C11Size.ItemSelect = ThroughputFile.param.C11SizeItemSelect;
			StationC11 = Station[C11Size.StationNo];
			UpdateComboBox(16, C11Size.StationNo, C11Size.ToolSelect, C11Size.SubSelect, C11Size.ItemSelect);
			txt_C11Str.Text = ThroughputFile.param.C11Str;
			chk_C12Enable.IsChecked = ThroughputFile.param.C12Enable;
			C12Size.StationNo = ThroughputFile.param.C12SizeStation;
			C12Size.ToolSelect = ThroughputFile.param.C12SizeToolSelect;
			C12Size.SubSelect = ThroughputFile.param.C12SizeSubSelect;
			C12Size.ItemSelect = ThroughputFile.param.C12SizeItemSelect;
			StationC12 = Station[C12Size.StationNo];
			UpdateComboBox(17, C12Size.StationNo, C12Size.ToolSelect, C12Size.SubSelect, C12Size.ItemSelect);
			txt_C12Str.Text = ThroughputFile.param.C12Str;
			chk_C13Enable.IsChecked = ThroughputFile.param.C13Enable;
			C13Size.StationNo = ThroughputFile.param.C13SizeStation;
			C13Size.ToolSelect = ThroughputFile.param.C13SizeToolSelect;
			C13Size.SubSelect = ThroughputFile.param.C13SizeSubSelect;
			C13Size.ItemSelect = ThroughputFile.param.C13SizeItemSelect;
			StationC13 = Station[C13Size.StationNo];
			UpdateComboBox(18, C13Size.StationNo, C13Size.ToolSelect, C13Size.SubSelect, C13Size.ItemSelect);
			txt_C13Str.Text = ThroughputFile.param.C13Str;
			chk_C14Enable.IsChecked = ThroughputFile.param.C14Enable;
			C14Size.StationNo = ThroughputFile.param.C14SizeStation;
			C14Size.ToolSelect = ThroughputFile.param.C14SizeToolSelect;
			C14Size.SubSelect = ThroughputFile.param.C14SizeSubSelect;
			C14Size.ItemSelect = ThroughputFile.param.C14SizeItemSelect;
			StationC14 = Station[C14Size.StationNo];
			UpdateComboBox(19, C14Size.StationNo, C14Size.ToolSelect, C14Size.SubSelect, C14Size.ItemSelect);
			txt_C14Str.Text = ThroughputFile.param.C14Str;
			txt_LPadStr.Text = ThroughputFile.param.LPadStr;
			txt_RPadStr.Text = ThroughputFile.param.RPadStr;
			txt_GapStr.Text = ThroughputFile.param.GapStr;
			lb_ChartNameLW.Content = ThroughputFile.param.LPadStr;
			lb_ChartNameRW.Content = ThroughputFile.param.RPadStr;
			lb_ChartNameGap.Content = ThroughputFile.param.GapStr;
			txt_ComDigitsName0.Text = ThroughputFile.param.ComDigitsName0;
			txt_ComDigitsName1.Text = ThroughputFile.param.ComDigitsName1;
			txt_ComDigitsName2.Text = ThroughputFile.param.ComDigitsName2;
			txt_ComDigitsName3.Text = ThroughputFile.param.ComDigitsName3;
			txt_ComDigitsName4.Text = ThroughputFile.param.ComDigitsName4;
			txt_ComDigitsName5.Text = ThroughputFile.param.ComDigitsName5;
			txt_ComDigitsName6.Text = ThroughputFile.param.ComDigitsName6;
			txt_ComDigitsName7.Text = ThroughputFile.param.ComDigitsName7;
			txt_ComDigitsName8.Text = ThroughputFile.param.ComDigitsName8;
			txt_ComDigitsName9.Text = ThroughputFile.param.ComDigitsName9;
			txt_ComDigitsName10.Text = ThroughputFile.param.ComDigitsName10;
			txt_ComDigitsName11.Text = ThroughputFile.param.ComDigitsName11;
			txt_ComDigitsName12.Text = ThroughputFile.param.ComDigitsName12;
			txt_ComDigitsName13.Text = ThroughputFile.param.ComDigitsName13;
			txt_ComDigitsName14.Text = ThroughputFile.param.ComDigitsName14;
			txt_ComDigitsName15.Text = ThroughputFile.param.ComDigitsName15;
			txt_ComDigitsName16.Text = ThroughputFile.param.ComDigitsName16;
			txt_ComDigitsName17.Text = ThroughputFile.param.ComDigitsName17;
			txt_ComDigitsName18.Text = ThroughputFile.param.ComDigitsName18;
			txt_ComDigitsName19.Text = ThroughputFile.param.ComDigitsName19;
			txt_ComDigitsName20.Text = ThroughputFile.param.ComDigitsName20;
			txt_ComDigitsName21.Text = ThroughputFile.param.ComDigitsName21;
			txt_ComDigitsName22.Text = ThroughputFile.param.ComDigitsName22;
			txt_ComDigitsName23.Text = ThroughputFile.param.ComDigitsName23;
			txt_ComDigits0.Text = ThroughputFile.param.ComDigits0.ToString();
			txt_ComDigits1.Text = ThroughputFile.param.ComDigits1.ToString();
			txt_ComDigits2.Text = ThroughputFile.param.ComDigits2.ToString();
			txt_ComDigits3.Text = ThroughputFile.param.ComDigits3.ToString();
			txt_ComDigits4.Text = ThroughputFile.param.ComDigits4.ToString();
			txt_ComDigits5.Text = ThroughputFile.param.ComDigits5.ToString();
			txt_ComDigits6.Text = ThroughputFile.param.ComDigits6.ToString();
			txt_ComDigits7.Text = ThroughputFile.param.ComDigits7.ToString();
			txt_ComDigits8.Text = ThroughputFile.param.ComDigits8.ToString();
			txt_ComDigits9.Text = ThroughputFile.param.ComDigits9.ToString();
			txt_ComDigits10.Text = ThroughputFile.param.ComDigits10.ToString();
			txt_ComDigits11.Text = ThroughputFile.param.ComDigits11.ToString();
			txt_ComDigits12.Text = ThroughputFile.param.ComDigits12.ToString();
			txt_ComDigits13.Text = ThroughputFile.param.ComDigits13.ToString();
			txt_ComDigits14.Text = ThroughputFile.param.ComDigits14.ToString();
			txt_ComDigits15.Text = ThroughputFile.param.ComDigits15.ToString();
			txt_ComDigits16.Text = ThroughputFile.param.ComDigits16.ToString();
			txt_ComDigits17.Text = ThroughputFile.param.ComDigits17.ToString();
			txt_ComDigits18.Text = ThroughputFile.param.ComDigits18.ToString();
			txt_ComDigits19.Text = ThroughputFile.param.ComDigits19.ToString();
			txt_ComDigits20.Text = ThroughputFile.param.ComDigits20.ToString();
			txt_ComDigits21.Text = ThroughputFile.param.ComDigits21.ToString();
			txt_ComDigits22.Text = ThroughputFile.param.ComDigits22.ToString();
			txt_ComDigits23.Text = ThroughputFile.param.ComDigits23.ToString();
			txt_CustomRatio.Text = ThroughputFile.param.CustomRatio.ToString();
			chk_EnableCustomRatio.IsChecked = ThroughputFile.param.EnableCustomRatio;
			grd_Custom.Visibility = Visibility.Hidden;
		}
	}

	private bool CheckTextBox()
	{
		bool result = true;
		if (!IsNumeric(txt_ComDigits1.Text))
		{
			txt_ComDigits1.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits1.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits2.Text))
		{
			txt_ComDigits2.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits2.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits3.Text))
		{
			txt_ComDigits3.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits3.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits4.Text))
		{
			txt_ComDigits4.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits4.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits5.Text))
		{
			txt_ComDigits5.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits5.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits6.Text))
		{
			txt_ComDigits6.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits6.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits7.Text))
		{
			txt_ComDigits7.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits7.Background = Brushes.White;
		}
		if (!IsNumeric(txt_ComDigits8.Text))
		{
			txt_ComDigits8.Background = Brushes.Fuchsia;
			result = false;
		}
		else
		{
			txt_ComDigits8.Background = Brushes.White;
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
			string text = "輸入格式錯誤-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool WriteUIValueToParam()
	{
		try
		{
			bool result = true;
			btn_AlarmL.Visibility = Visibility.Hidden;
			btn_AlarmW.Visibility = Visibility.Hidden;
			btn_AlarmT.Visibility = Visibility.Hidden;
			btn_AlarmLW.Visibility = Visibility.Hidden;
			btn_AlarmRW.Visibility = Visibility.Hidden;
			btn_AlarmGap.Visibility = Visibility.Hidden;
			btn_AlarmC1.Visibility = Visibility.Hidden;
			btn_AlarmC2.Visibility = Visibility.Hidden;
			btn_AlarmC3.Visibility = Visibility.Hidden;
			btn_AlarmC4.Visibility = Visibility.Hidden;
			btn_AlarmC5.Visibility = Visibility.Hidden;
			btn_AlarmC6.Visibility = Visibility.Hidden;
			btn_AlarmC7.Visibility = Visibility.Hidden;
			btn_AlarmC8.Visibility = Visibility.Hidden;
			btn_AlarmC9.Visibility = Visibility.Hidden;
			btn_AlarmC10.Visibility = Visibility.Hidden;
			btn_AlarmC11.Visibility = Visibility.Hidden;
			btn_AlarmC12.Visibility = Visibility.Hidden;
			btn_AlarmC13.Visibility = Visibility.Hidden;
			btn_AlarmC14.Visibility = Visibility.Hidden;
			cb_ItemSelectL.Background = Brushes.White;
			cb_ItemSelectW.Background = Brushes.White;
			cb_ItemSelectT.Background = Brushes.White;
			cb_ItemSelectLW.Background = Brushes.White;
			cb_ItemSelectRW.Background = Brushes.White;
			cb_ItemSelectGap.Background = Brushes.White;
			for (int i = 0; i < 20; i++)
			{
				ItemSelect[i].Background = Brushes.White;
			}
			LSize.StationNo = cb_StationSelectL.SelectedIndex;
			LSize.ToolSelect = cb_ToolsSelectL.SelectedIndex;
			LSize.SubSelect = cb_SubSelectL.SelectedIndex;
			LSize.ItemSelect = cb_ItemSelectL.SelectedIndex;
			WSize.StationNo = cb_StationSelectW.SelectedIndex;
			WSize.ToolSelect = cb_ToolsSelectW.SelectedIndex;
			WSize.SubSelect = cb_SubSelectW.SelectedIndex;
			WSize.ItemSelect = cb_ItemSelectW.SelectedIndex;
			TSize.StationNo = cb_StationSelectT.SelectedIndex;
			TSize.ToolSelect = cb_ToolsSelectT.SelectedIndex;
			TSize.SubSelect = cb_SubSelectT.SelectedIndex;
			TSize.ItemSelect = cb_ItemSelectT.SelectedIndex;
			LPadSize.StationNo = cb_StationSelectLW.SelectedIndex;
			LPadSize.ToolSelect = cb_ToolsSelectLW.SelectedIndex;
			LPadSize.SubSelect = cb_SubSelectLW.SelectedIndex;
			LPadSize.ItemSelect = cb_ItemSelectLW.SelectedIndex;
			RPadSize.StationNo = cb_StationSelectRW.SelectedIndex;
			RPadSize.ToolSelect = cb_ToolsSelectRW.SelectedIndex;
			RPadSize.SubSelect = cb_SubSelectRW.SelectedIndex;
			RPadSize.ItemSelect = cb_ItemSelectRW.SelectedIndex;
			GapSize.StationNo = cb_StationSelectGap.SelectedIndex;
			GapSize.ToolSelect = cb_ToolsSelectGap.SelectedIndex;
			GapSize.SubSelect = cb_SubSelectGap.SelectedIndex;
			GapSize.ItemSelect = cb_ItemSelectGap.SelectedIndex;
			C1Size.StationNo = cb_StationSelectC1.SelectedIndex;
			C1Size.ToolSelect = cb_ToolsSelectC1.SelectedIndex;
			C1Size.SubSelect = cb_SubSelectC1.SelectedIndex;
			C1Size.ItemSelect = cb_ItemSelectC1.SelectedIndex;
			C2Size.StationNo = cb_StationSelectC2.SelectedIndex;
			C2Size.ToolSelect = cb_ToolsSelectC2.SelectedIndex;
			C2Size.SubSelect = cb_SubSelectC2.SelectedIndex;
			C2Size.ItemSelect = cb_ItemSelectC2.SelectedIndex;
			C3Size.StationNo = cb_StationSelectC3.SelectedIndex;
			C3Size.ToolSelect = cb_ToolsSelectC3.SelectedIndex;
			C3Size.SubSelect = cb_SubSelectC3.SelectedIndex;
			C3Size.ItemSelect = cb_ItemSelectC3.SelectedIndex;
			C4Size.StationNo = cb_StationSelectC4.SelectedIndex;
			C4Size.ToolSelect = cb_ToolsSelectC4.SelectedIndex;
			C4Size.SubSelect = cb_SubSelectC4.SelectedIndex;
			C4Size.ItemSelect = cb_ItemSelectC4.SelectedIndex;
			C5Size.StationNo = cb_StationSelectC5.SelectedIndex;
			C5Size.ToolSelect = cb_ToolsSelectC5.SelectedIndex;
			C5Size.SubSelect = cb_SubSelectC5.SelectedIndex;
			C5Size.ItemSelect = cb_ItemSelectC5.SelectedIndex;
			C6Size.StationNo = cb_StationSelectC6.SelectedIndex;
			C6Size.ToolSelect = cb_ToolsSelectC6.SelectedIndex;
			C6Size.SubSelect = cb_SubSelectC6.SelectedIndex;
			C6Size.ItemSelect = cb_ItemSelectC6.SelectedIndex;
			C7Size.StationNo = cb_StationSelectC7.SelectedIndex;
			C7Size.ToolSelect = cb_ToolsSelectC7.SelectedIndex;
			C7Size.SubSelect = cb_SubSelectC7.SelectedIndex;
			C7Size.ItemSelect = cb_ItemSelectC7.SelectedIndex;
			C8Size.StationNo = cb_StationSelectC8.SelectedIndex;
			C8Size.ToolSelect = cb_ToolsSelectC8.SelectedIndex;
			C8Size.SubSelect = cb_SubSelectC8.SelectedIndex;
			C8Size.ItemSelect = cb_ItemSelectC8.SelectedIndex;
			C9Size.StationNo = cb_StationSelectC9.SelectedIndex;
			C9Size.ToolSelect = cb_ToolsSelectC9.SelectedIndex;
			C9Size.SubSelect = cb_SubSelectC9.SelectedIndex;
			C9Size.ItemSelect = cb_ItemSelectC9.SelectedIndex;
			C10Size.StationNo = cb_StationSelectC10.SelectedIndex;
			C10Size.ToolSelect = cb_ToolsSelectC10.SelectedIndex;
			C10Size.SubSelect = cb_SubSelectC10.SelectedIndex;
			C10Size.ItemSelect = cb_ItemSelectC10.SelectedIndex;
			C11Size.StationNo = cb_StationSelectC11.SelectedIndex;
			C11Size.ToolSelect = cb_ToolsSelectC11.SelectedIndex;
			C11Size.SubSelect = cb_SubSelectC11.SelectedIndex;
			C11Size.ItemSelect = cb_ItemSelectC11.SelectedIndex;
			C12Size.StationNo = cb_StationSelectC12.SelectedIndex;
			C12Size.ToolSelect = cb_ToolsSelectC12.SelectedIndex;
			C12Size.SubSelect = cb_SubSelectC12.SelectedIndex;
			C12Size.ItemSelect = cb_ItemSelectC12.SelectedIndex;
			C13Size.StationNo = cb_StationSelectC13.SelectedIndex;
			C13Size.ToolSelect = cb_ToolsSelectC13.SelectedIndex;
			C13Size.SubSelect = cb_SubSelectC13.SelectedIndex;
			C13Size.ItemSelect = cb_ItemSelectC13.SelectedIndex;
			C14Size.StationNo = cb_StationSelectC14.SelectedIndex;
			C14Size.ToolSelect = cb_ToolsSelectC14.SelectedIndex;
			C14Size.SubSelect = cb_SubSelectC14.SelectedIndex;
			C14Size.ItemSelect = cb_ItemSelectC14.SelectedIndex;
			ThroughputFile.param.CustomRatio = Convert.ToDouble(txt_CustomRatio.Text);
			ThroughputFile.param.EnableCustomRatio = chk_EnableCustomRatio.IsChecked.Value;
			if (CheckTextBox())
			{
				ThroughputFile.param.ComDigits0 = Convert.ToDouble(txt_ComDigits0.Text);
				ThroughputFile.param.ComDigits1 = Convert.ToDouble(txt_ComDigits1.Text);
				ThroughputFile.param.ComDigits2 = Convert.ToDouble(txt_ComDigits2.Text);
				ThroughputFile.param.ComDigits3 = Convert.ToDouble(txt_ComDigits3.Text);
				ThroughputFile.param.ComDigits4 = Convert.ToDouble(txt_ComDigits4.Text);
				ThroughputFile.param.ComDigits5 = Convert.ToDouble(txt_ComDigits5.Text);
				ThroughputFile.param.ComDigits6 = Convert.ToDouble(txt_ComDigits6.Text);
				ThroughputFile.param.ComDigits7 = Convert.ToDouble(txt_ComDigits7.Text);
				ThroughputFile.param.ComDigits8 = Convert.ToDouble(txt_ComDigits8.Text);
				ThroughputFile.param.ComDigits9 = Convert.ToDouble(txt_ComDigits9.Text);
				ThroughputFile.param.ComDigits10 = Convert.ToDouble(txt_ComDigits10.Text);
				ThroughputFile.param.ComDigits11 = Convert.ToDouble(txt_ComDigits11.Text);
				ThroughputFile.param.ComDigits12 = Convert.ToDouble(txt_ComDigits12.Text);
				ThroughputFile.param.ComDigits13 = Convert.ToDouble(txt_ComDigits13.Text);
				ThroughputFile.param.ComDigits14 = Convert.ToDouble(txt_ComDigits14.Text);
				ThroughputFile.param.ComDigits15 = Convert.ToDouble(txt_ComDigits15.Text);
				ThroughputFile.param.ComDigits16 = Convert.ToDouble(txt_ComDigits16.Text);
				ThroughputFile.param.ComDigits17 = Convert.ToDouble(txt_ComDigits17.Text);
				ThroughputFile.param.ComDigits18 = Convert.ToDouble(txt_ComDigits18.Text);
				ThroughputFile.param.ComDigits19 = Convert.ToDouble(txt_ComDigits19.Text);
				ThroughputFile.param.ComDigits20 = Convert.ToDouble(txt_ComDigits20.Text);
				ThroughputFile.param.ComDigits21 = Convert.ToDouble(txt_ComDigits21.Text);
				ThroughputFile.param.ComDigits22 = Convert.ToDouble(txt_ComDigits22.Text);
				ThroughputFile.param.ComDigits23 = Convert.ToDouble(txt_ComDigits23.Text);
				ThroughputFile.param.ComDigitsName0 = txt_ComDigitsName0.Text;
				ThroughputFile.param.ComDigitsName1 = txt_ComDigitsName1.Text;
				ThroughputFile.param.ComDigitsName2 = txt_ComDigitsName2.Text;
				ThroughputFile.param.ComDigitsName3 = txt_ComDigitsName3.Text;
				ThroughputFile.param.ComDigitsName4 = txt_ComDigitsName4.Text;
				ThroughputFile.param.ComDigitsName5 = txt_ComDigitsName5.Text;
				ThroughputFile.param.ComDigitsName6 = txt_ComDigitsName6.Text;
				ThroughputFile.param.ComDigitsName7 = txt_ComDigitsName7.Text;
				ThroughputFile.param.ComDigitsName8 = txt_ComDigitsName8.Text;
				ThroughputFile.param.ComDigitsName9 = txt_ComDigitsName9.Text;
				ThroughputFile.param.ComDigitsName10 = txt_ComDigitsName10.Text;
				ThroughputFile.param.ComDigitsName11 = txt_ComDigitsName11.Text;
				ThroughputFile.param.ComDigitsName12 = txt_ComDigitsName12.Text;
				ThroughputFile.param.ComDigitsName13 = txt_ComDigitsName13.Text;
				ThroughputFile.param.ComDigitsName14 = txt_ComDigitsName14.Text;
				ThroughputFile.param.ComDigitsName15 = txt_ComDigitsName15.Text;
				ThroughputFile.param.ComDigitsName16 = txt_ComDigitsName16.Text;
				ThroughputFile.param.ComDigitsName17 = txt_ComDigitsName17.Text;
				ThroughputFile.param.ComDigitsName18 = txt_ComDigitsName18.Text;
				ThroughputFile.param.ComDigitsName19 = txt_ComDigitsName19.Text;
				ThroughputFile.param.ComDigitsName20 = txt_ComDigitsName20.Text;
				ThroughputFile.param.ComDigitsName21 = txt_ComDigitsName21.Text;
				ThroughputFile.param.ComDigitsName22 = txt_ComDigitsName22.Text;
				ThroughputFile.param.ComDigitsName23 = txt_ComDigitsName23.Text;
				ThroughputFile.UpdateComDigis();
			}
			ThroughputFile.param.LSizeEnable = chk_LSizeEnable.IsChecked.Value;
			ThroughputFile.param.LSizeStation = LSize.StationNo;
			ThroughputFile.param.LSizeToolSelect = LSize.ToolSelect;
			ThroughputFile.param.LSizeSubSelect = LSize.SubSelect;
			if (LSize.ToolSelect == 0)
			{
				ThroughputFile.param.LSizeItemSelect = -1;
			}
			else if (LSize.ToolSelect != 0 && LSize.ItemSelect < 0)
			{
				ThroughputFile.param.LSizeItemSelect = 0;
				btn_AlarmL.Visibility = Visibility.Visible;
				cb_ItemSelectL.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.LSizeItemSelect = LSize.ItemSelect;
			}
			ThroughputFile.param.WSizeEnable = chk_WSizeEnable.IsChecked.Value;
			ThroughputFile.param.WSizeStation = WSize.StationNo;
			ThroughputFile.param.WSizeToolSelect = WSize.ToolSelect;
			ThroughputFile.param.WSizeSubSelect = WSize.SubSelect;
			if (WSize.ToolSelect == 0)
			{
				ThroughputFile.param.WSizeItemSelect = -1;
			}
			else if (WSize.ToolSelect != 0 && WSize.ItemSelect < 0)
			{
				ThroughputFile.param.WSizeItemSelect = 0;
				btn_AlarmW.Visibility = Visibility.Visible;
				cb_ItemSelectW.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.WSizeItemSelect = WSize.ItemSelect;
			}
			ThroughputFile.param.TSizeEnable = chk_TSizeEnable.IsChecked.Value;
			ThroughputFile.param.TSizeStation = TSize.StationNo;
			ThroughputFile.param.TSizeToolSelect = TSize.ToolSelect;
			ThroughputFile.param.TSizeSubSelect = TSize.SubSelect;
			if (TSize.ToolSelect == 0)
			{
				ThroughputFile.param.TSizeItemSelect = -1;
			}
			else if (TSize.ToolSelect != 0 && TSize.ItemSelect < 0)
			{
				ThroughputFile.param.TSizeItemSelect = 0;
				btn_AlarmT.Visibility = Visibility.Visible;
				cb_ItemSelectT.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.TSizeItemSelect = TSize.ItemSelect;
			}
			ThroughputFile.param.LPadSizeEnable = chk_LPadSizeEnable.IsChecked.Value;
			ThroughputFile.param.LPadSizeStation = LPadSize.StationNo;
			ThroughputFile.param.LPadSizeToolSelect = LPadSize.ToolSelect;
			ThroughputFile.param.LPadSizeSubSelect = LPadSize.SubSelect;
			if (LPadSize.ToolSelect == 0)
			{
				ThroughputFile.param.LPadSizeItemSelect = -1;
			}
			else if (LPadSize.ToolSelect != 0 && LPadSize.ItemSelect < 0)
			{
				ThroughputFile.param.LPadSizeItemSelect = 0;
				btn_AlarmLW.Visibility = Visibility.Visible;
				cb_ItemSelectLW.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.LPadSizeItemSelect = LPadSize.ItemSelect;
			}
			ThroughputFile.param.RPadSizeEnable = chk_RPadSizeEnable.IsChecked.Value;
			ThroughputFile.param.RPadSizeStation = RPadSize.StationNo;
			ThroughputFile.param.RPadSizeToolSelect = RPadSize.ToolSelect;
			ThroughputFile.param.RPadSizeSubSelect = RPadSize.SubSelect;
			if (RPadSize.ToolSelect == 0)
			{
				ThroughputFile.param.RPadSizeItemSelect = -1;
			}
			else if (RPadSize.ToolSelect != 0 && RPadSize.ItemSelect < 0)
			{
				ThroughputFile.param.RPadSizeItemSelect = 0;
				btn_AlarmRW.Visibility = Visibility.Visible;
				cb_ItemSelectRW.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.RPadSizeItemSelect = RPadSize.ItemSelect;
			}
			ThroughputFile.param.GapSizeEnable = chk_GapSizeEnable.IsChecked.Value;
			ThroughputFile.param.GapSizeStation = GapSize.StationNo;
			ThroughputFile.param.GapSizeToolSelect = GapSize.ToolSelect;
			ThroughputFile.param.GapSizeSubSelect = GapSize.SubSelect;
			if (GapSize.ToolSelect == 0)
			{
				ThroughputFile.param.GapSizeItemSelect = -1;
			}
			else if (GapSize.ToolSelect != 0 && GapSize.ItemSelect < 0)
			{
				ThroughputFile.param.GapSizeItemSelect = 0;
				btn_AlarmGap.Visibility = Visibility.Visible;
				cb_ItemSelectGap.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.GapSizeItemSelect = GapSize.ItemSelect;
			}
			ThroughputFile.param.LPadStr = txt_LPadStr.Text;
			ThroughputFile.param.RPadStr = txt_RPadStr.Text;
			ThroughputFile.param.GapStr = txt_GapStr.Text;
			ThroughputFile.param.C1Enable = chk_C1Enable.IsChecked.Value;
			ThroughputFile.param.C1SizeStation = C1Size.StationNo;
			ThroughputFile.param.C1SizeToolSelect = C1Size.ToolSelect;
			ThroughputFile.param.C1SizeSubSelect = C1Size.SubSelect;
			if (C1Size.ToolSelect == 0)
			{
				ThroughputFile.param.C1SizeItemSelect = -1;
			}
			else if (C1Size.ToolSelect != 0 && C1Size.ItemSelect < 0)
			{
				ThroughputFile.param.C1SizeItemSelect = 0;
				btn_AlarmC1.Visibility = Visibility.Visible;
				cb_ItemSelectC1.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C1SizeItemSelect = C1Size.ItemSelect;
			}
			ThroughputFile.param.C1Str = txt_C1Str.Text;
			ThroughputFile.param.C2Enable = chk_C2Enable.IsChecked.Value;
			ThroughputFile.param.C2SizeStation = C2Size.StationNo;
			ThroughputFile.param.C2SizeToolSelect = C2Size.ToolSelect;
			ThroughputFile.param.C2SizeSubSelect = C2Size.SubSelect;
			if (C2Size.ToolSelect == 0)
			{
				ThroughputFile.param.C2SizeItemSelect = -1;
			}
			else if (C2Size.ToolSelect != 0 && C2Size.ItemSelect < 0)
			{
				ThroughputFile.param.C2SizeItemSelect = 0;
				btn_AlarmC2.Visibility = Visibility.Visible;
				cb_ItemSelectC2.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C2SizeItemSelect = C2Size.ItemSelect;
			}
			ThroughputFile.param.C2Str = txt_C2Str.Text;
			ThroughputFile.param.C3Enable = chk_C3Enable.IsChecked.Value;
			ThroughputFile.param.C3SizeStation = C3Size.StationNo;
			ThroughputFile.param.C3SizeToolSelect = C3Size.ToolSelect;
			ThroughputFile.param.C3SizeSubSelect = C3Size.SubSelect;
			if (C3Size.ToolSelect == 0)
			{
				ThroughputFile.param.C3SizeItemSelect = -1;
			}
			else if (C3Size.ToolSelect != 0 && C3Size.ItemSelect < 0)
			{
				ThroughputFile.param.C3SizeItemSelect = 0;
				btn_AlarmC3.Visibility = Visibility.Visible;
				cb_ItemSelectC3.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C3SizeItemSelect = C3Size.ItemSelect;
			}
			ThroughputFile.param.C3Str = txt_C3Str.Text;
			ThroughputFile.param.C4Enable = chk_C4Enable.IsChecked.Value;
			ThroughputFile.param.C4SizeStation = C4Size.StationNo;
			ThroughputFile.param.C4SizeToolSelect = C4Size.ToolSelect;
			ThroughputFile.param.C4SizeSubSelect = C4Size.SubSelect;
			if (C4Size.ToolSelect == 0)
			{
				ThroughputFile.param.C4SizeItemSelect = -1;
			}
			else if (C4Size.ToolSelect != 0 && C4Size.ItemSelect < 0)
			{
				ThroughputFile.param.C4SizeItemSelect = 0;
				btn_AlarmC4.Visibility = Visibility.Visible;
				cb_ItemSelectC4.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C4SizeItemSelect = C4Size.ItemSelect;
			}
			ThroughputFile.param.C4Str = txt_C4Str.Text;
			ThroughputFile.param.C5Enable = chk_C5Enable.IsChecked.Value;
			ThroughputFile.param.C5SizeStation = C5Size.StationNo;
			ThroughputFile.param.C5SizeToolSelect = C5Size.ToolSelect;
			ThroughputFile.param.C5SizeSubSelect = C5Size.SubSelect;
			if (C5Size.ToolSelect == 0)
			{
				ThroughputFile.param.C5SizeItemSelect = -1;
			}
			else if (C5Size.ToolSelect != 0 && C5Size.ItemSelect < 0)
			{
				ThroughputFile.param.C5SizeItemSelect = 0;
				btn_AlarmC5.Visibility = Visibility.Visible;
				cb_ItemSelectC5.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C5SizeItemSelect = C5Size.ItemSelect;
			}
			ThroughputFile.param.C5Str = txt_C5Str.Text;
			ThroughputFile.param.C6Enable = chk_C6Enable.IsChecked.Value;
			ThroughputFile.param.C6SizeStation = C6Size.StationNo;
			ThroughputFile.param.C6SizeToolSelect = C6Size.ToolSelect;
			ThroughputFile.param.C6SizeSubSelect = C6Size.SubSelect;
			if (C6Size.ToolSelect == 0)
			{
				ThroughputFile.param.C6SizeItemSelect = -1;
			}
			else if (C6Size.ToolSelect != 0 && C6Size.ItemSelect < 0)
			{
				ThroughputFile.param.C6SizeItemSelect = 0;
				btn_AlarmC6.Visibility = Visibility.Visible;
				cb_ItemSelectC6.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C6SizeItemSelect = C6Size.ItemSelect;
			}
			ThroughputFile.param.C6Str = txt_C6Str.Text;
			ThroughputFile.param.C7Enable = chk_C7Enable.IsChecked.Value;
			ThroughputFile.param.C7SizeStation = C7Size.StationNo;
			ThroughputFile.param.C7SizeToolSelect = C7Size.ToolSelect;
			ThroughputFile.param.C7SizeSubSelect = C7Size.SubSelect;
			if (C7Size.ToolSelect == 0)
			{
				ThroughputFile.param.C7SizeItemSelect = -1;
			}
			else if (C7Size.ToolSelect != 0 && C7Size.ItemSelect < 0)
			{
				ThroughputFile.param.C7SizeItemSelect = 0;
				btn_AlarmC7.Visibility = Visibility.Visible;
				cb_ItemSelectC7.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C7SizeItemSelect = C7Size.ItemSelect;
			}
			ThroughputFile.param.C7Str = txt_C7Str.Text;
			ThroughputFile.param.C8Enable = chk_C8Enable.IsChecked.Value;
			ThroughputFile.param.C8SizeStation = C8Size.StationNo;
			ThroughputFile.param.C8SizeToolSelect = C8Size.ToolSelect;
			ThroughputFile.param.C8SizeSubSelect = C8Size.SubSelect;
			if (C8Size.ToolSelect == 0)
			{
				ThroughputFile.param.C8SizeItemSelect = -1;
			}
			else if (C8Size.ToolSelect != 0 && C8Size.ItemSelect < 0)
			{
				ThroughputFile.param.C8SizeItemSelect = 0;
				btn_AlarmC8.Visibility = Visibility.Visible;
				cb_ItemSelectC8.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C8SizeItemSelect = C8Size.ItemSelect;
			}
			ThroughputFile.param.C8Str = txt_C8Str.Text;
			ThroughputFile.param.C9Enable = chk_C9Enable.IsChecked.Value;
			ThroughputFile.param.C9SizeStation = C9Size.StationNo;
			ThroughputFile.param.C9SizeToolSelect = C9Size.ToolSelect;
			ThroughputFile.param.C9SizeSubSelect = C9Size.SubSelect;
			if (C9Size.ToolSelect == 0)
			{
				ThroughputFile.param.C9SizeItemSelect = -1;
			}
			else if (C9Size.ToolSelect != 0 && C9Size.ItemSelect < 0)
			{
				ThroughputFile.param.C9SizeItemSelect = 0;
				btn_AlarmC9.Visibility = Visibility.Visible;
				cb_ItemSelectC9.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C9SizeItemSelect = C9Size.ItemSelect;
			}
			ThroughputFile.param.C9Str = txt_C9Str.Text;
			ThroughputFile.param.C10Enable = chk_C10Enable.IsChecked.Value;
			ThroughputFile.param.C10SizeStation = C10Size.StationNo;
			ThroughputFile.param.C10SizeToolSelect = C10Size.ToolSelect;
			ThroughputFile.param.C10SizeSubSelect = C10Size.SubSelect;
			if (C10Size.ToolSelect == 0)
			{
				ThroughputFile.param.C10SizeItemSelect = -1;
			}
			else if (C10Size.ToolSelect != 0 && C10Size.ItemSelect < 0)
			{
				ThroughputFile.param.C10SizeItemSelect = 0;
				btn_AlarmC10.Visibility = Visibility.Visible;
				cb_ItemSelectC10.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C10SizeItemSelect = C10Size.ItemSelect;
			}
			ThroughputFile.param.C10Str = txt_C10Str.Text;
			ThroughputFile.param.C11Enable = chk_C11Enable.IsChecked.Value;
			ThroughputFile.param.C11SizeStation = C11Size.StationNo;
			ThroughputFile.param.C11SizeToolSelect = C11Size.ToolSelect;
			ThroughputFile.param.C11SizeSubSelect = C11Size.SubSelect;
			if (C11Size.ToolSelect == 0)
			{
				ThroughputFile.param.C11SizeItemSelect = -1;
			}
			else if (C11Size.ToolSelect != 0 && C11Size.ItemSelect < 0)
			{
				ThroughputFile.param.C11SizeItemSelect = 0;
				btn_AlarmC11.Visibility = Visibility.Visible;
				cb_ItemSelectC11.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C11SizeItemSelect = C11Size.ItemSelect;
			}
			ThroughputFile.param.C11Str = txt_C11Str.Text;
			ThroughputFile.param.C12Enable = chk_C12Enable.IsChecked.Value;
			ThroughputFile.param.C12SizeStation = C12Size.StationNo;
			ThroughputFile.param.C12SizeToolSelect = C12Size.ToolSelect;
			ThroughputFile.param.C12SizeSubSelect = C12Size.SubSelect;
			if (C12Size.ToolSelect == 0)
			{
				ThroughputFile.param.C12SizeItemSelect = -1;
			}
			else if (C12Size.ToolSelect != 0 && C12Size.ItemSelect < 0)
			{
				ThroughputFile.param.C12SizeItemSelect = 0;
				btn_AlarmC12.Visibility = Visibility.Visible;
				cb_ItemSelectC12.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C12SizeItemSelect = C12Size.ItemSelect;
			}
			ThroughputFile.param.C12Str = txt_C12Str.Text;
			ThroughputFile.param.C13Enable = chk_C13Enable.IsChecked.Value;
			ThroughputFile.param.C13SizeStation = C13Size.StationNo;
			ThroughputFile.param.C13SizeToolSelect = C13Size.ToolSelect;
			ThroughputFile.param.C13SizeSubSelect = C13Size.SubSelect;
			if (C13Size.ToolSelect == 0)
			{
				ThroughputFile.param.C13SizeItemSelect = -1;
			}
			else if (C13Size.ToolSelect != 0 && C13Size.ItemSelect < 0)
			{
				ThroughputFile.param.C13SizeItemSelect = 0;
				btn_AlarmC13.Visibility = Visibility.Visible;
				cb_ItemSelectC13.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C13SizeItemSelect = C13Size.ItemSelect;
			}
			ThroughputFile.param.C13Str = txt_C13Str.Text;
			ThroughputFile.param.C14Enable = chk_C14Enable.IsChecked.Value;
			ThroughputFile.param.C14SizeStation = C14Size.StationNo;
			ThroughputFile.param.C14SizeToolSelect = C14Size.ToolSelect;
			ThroughputFile.param.C14SizeSubSelect = C14Size.SubSelect;
			if (C14Size.ToolSelect == 0)
			{
				ThroughputFile.param.C14SizeItemSelect = -1;
			}
			else if (C14Size.ToolSelect != 0 && C14Size.ItemSelect < 0)
			{
				ThroughputFile.param.C14SizeItemSelect = 0;
				btn_AlarmC14.Visibility = Visibility.Visible;
				cb_ItemSelectC14.Background = Brushes.Red;
				result = false;
			}
			else
			{
				ThroughputFile.param.C14SizeItemSelect = C14Size.ItemSelect;
			}
			ThroughputFile.param.C14Str = txt_C14Str.Text;
			ThroughputFile.WriteSizeRecordSetupParam();
			return result;
		}
		catch (Exception ex)
		{
			string text = "SC01-SizeChartWindow-WriteUIValueToParam()";
			CVBuffer.SaveLog(text);
			_ = text + ex.ToString();
			return false;
		}
	}

	private void UpdateComboBox(int sizeindex, int station, int tools, int sub, int item)
	{
		StationSelect[sizeindex].SelectedIndex = station;
		ToolSelect[sizeindex].SelectedIndex = tools;
		SubSelect[sizeindex].SelectedIndex = sub;
		ItemSelect[sizeindex].SelectedIndex = item;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_QuickSetupSave_Click(object sender, RoutedEventArgs e)
	{
		if (!WriteUIValueToParam())
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Setup Error, Check Selection value", "錯誤");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("參數設定錯誤，請重新確認", "錯誤");
			}
			else
			{
				MessageBox.Show("参数设定错误，请重新确认", "错误");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_ItemSelectL_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!init)
		{
			return;
		}
		try
		{
			ComboBox comboBox = (ComboBox)sender;
			int num = 0;
			string uid = comboBox.Uid;
			if (uid != null)
			{
				switch (uid.Length)
				{
				case 1:
					switch (uid[0])
					{
					case 'L':
						num = 0;
						break;
					case 'W':
						num = 1;
						break;
					case 'T':
						num = 2;
						break;
					}
					break;
				case 2:
					switch (uid[1])
					{
					case 'W':
						if (!(uid == "LW"))
						{
							if (uid == "RW")
							{
								num = 4;
							}
						}
						else
						{
							num = 3;
						}
						break;
					case '1':
						if (uid == "C1")
						{
							num = 6;
						}
						break;
					case '2':
						if (uid == "C2")
						{
							num = 7;
						}
						break;
					case '3':
						if (uid == "C3")
						{
							num = 8;
						}
						break;
					case '4':
						if (uid == "C4")
						{
							num = 9;
						}
						break;
					case '5':
						if (uid == "C5")
						{
							num = 10;
						}
						break;
					case '6':
						if (uid == "C6")
						{
							num = 11;
						}
						break;
					case '7':
						if (uid == "C7")
						{
							num = 12;
						}
						break;
					case '8':
						if (uid == "C8")
						{
							num = 13;
						}
						break;
					case '9':
						if (uid == "C9")
						{
							num = 14;
						}
						break;
					}
					break;
				case 3:
					switch (uid[2])
					{
					case 'p':
						if (uid == "Gap")
						{
							num = 5;
						}
						break;
					case '0':
						if (uid == "C10")
						{
							num = 15;
						}
						break;
					case '1':
						if (uid == "C11")
						{
							num = 16;
						}
						break;
					case '2':
						if (uid == "C12")
						{
							num = 17;
						}
						break;
					case '3':
						if (uid == "C13")
						{
							num = 18;
						}
						break;
					case '4':
						if (uid == "C14")
						{
							num = 19;
						}
						break;
					}
					break;
				}
			}
			if (num >= 0)
			{
				ItemSelect[num].Background = Brushes.White;
				if (comboBox.Uid == "L")
				{
					btn_AlarmL.Visibility = Visibility.Hidden;
				}
				else if (comboBox.Uid == "W")
				{
					btn_AlarmW.Visibility = Visibility.Hidden;
				}
				else
				{
					btn_AlarmT.Visibility = Visibility.Hidden;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "SW05-SizeChartWindow-Cb_ItemSelectL_SelectionChanged()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_SizeFun_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (init && e.Source is TabControl && e.AddedItems[0] is TabItem tabItem)
		{
			tab_Chart.Background = Brushes.White;
			tab_Setup.Background = Brushes.White;
			tab_Custom.Background = Brushes.White;
			tab_Setup2.Background = Brushes.White;
			tab_Setup3.Background = Brushes.White;
			if (tabItem.Name == "tab_Chart")
			{
				tab_Chart.Background = Brushes.Yellow;
			}
			else if (tabItem.Name == "tab_Setup")
			{
				tab_Setup.Background = Brushes.Yellow;
			}
			else if (tabItem.Name == "tab_Setup2")
			{
				tab_Setup2.Background = Brushes.Yellow;
			}
			else if (tabItem.Name == "tab_Setup3")
			{
				tab_Setup3.Background = Brushes.Yellow;
			}
			else
			{
				tab_Custom.Background = Brushes.Yellow;
			}
		}
	}

	private void Btn_NextT_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueT += 200;
		if (DefalutValueT >= 900)
		{
			DefalutValueT = 1000;
		}
		cc_ChartT.AxisX[0].MaxValue = DefalutValueT + 200;
		cc_ChartT.AxisX[0].MinValue = DefalutValueT;
	}

	private void Btn_ForwordT_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueT -= 200;
		if (DefalutValueT <= 0)
		{
			DefalutValueT = 0;
		}
		cc_ChartT.AxisX[0].MaxValue = DefalutValueT + 200;
		cc_ChartT.AxisX[0].MinValue = DefalutValueT;
	}

	private void Btn_NextW_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueW += 200;
		if (DefalutValueW >= 900)
		{
			DefalutValueW = 1000;
		}
		cc_ChartW.AxisX[0].MaxValue = DefalutValueW + 200;
		cc_ChartW.AxisX[0].MinValue = DefalutValueW;
	}

	private void Btn_ForwordW_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueW -= 200;
		if (DefalutValueW <= 0)
		{
			DefalutValueW = 0;
		}
		cc_ChartW.AxisX[0].MaxValue = DefalutValueW + 200;
		cc_ChartW.AxisX[0].MinValue = DefalutValueW;
	}

	private void Cc_ChartL_MouseWheel(object sender, MouseWheelEventArgs e)
	{
	}

	private void Txt_InspRule1Min_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
	}

	private void Txt_ComDigitsName0_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		TextBox textBox = (TextBox)sender;
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr != "")
		{
			textBox.Text = itemInputWindow.InputStr;
		}
	}

	private void Txt_ComDigitsLButtonDown(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		Point position = e.GetPosition(this);
		SetKeyboard(tbox, position);
	}

	private void SetKeyboard(TextBox tbox, Point p)
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

	private void Btn_ForwordLW_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueLW -= 200;
		if (DefalutValueLW <= 0)
		{
			DefalutValueLW = 0;
		}
		cc_ChartLW.AxisX[0].MaxValue = DefalutValueLW + 200;
		cc_ChartLW.AxisX[0].MinValue = DefalutValueLW;
	}

	private void Btn_NextLW_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueLW += 200;
		if (DefalutValueLW >= 900)
		{
			DefalutValueLW = 1000;
		}
		cc_ChartLW.AxisX[0].MaxValue = DefalutValueLW + 200;
		cc_ChartLW.AxisX[0].MinValue = DefalutValueLW;
	}

	private void Btn_ForwordRW_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueRW -= 200;
		if (DefalutValueRW <= 0)
		{
			DefalutValueRW = 0;
		}
		cc_ChartRW.AxisX[0].MaxValue = DefalutValueRW + 200;
		cc_ChartRW.AxisX[0].MinValue = DefalutValueRW;
	}

	private void Btn_ForwordGap_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueGap -= 200;
		if (DefalutValueGap <= 0)
		{
			DefalutValueGap = 0;
		}
		cc_ChartGap.AxisX[0].MaxValue = DefalutValueGap + 200;
		cc_ChartGap.AxisX[0].MinValue = DefalutValueGap;
	}

	private void Btn_NextRW_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueRW += 200;
		if (DefalutValueRW >= 900)
		{
			DefalutValueRW = 1000;
		}
		cc_ChartRW.AxisX[0].MaxValue = DefalutValueRW + 200;
		cc_ChartRW.AxisX[0].MinValue = DefalutValueRW;
	}

	private void Btn_NextGap_Click(object sender, RoutedEventArgs e)
	{
		DefalutValueGap += 200;
		if (DefalutValueGap >= 900)
		{
			DefalutValueGap = 1000;
		}
		cc_ChartGap.AxisX[0].MaxValue = DefalutValueGap + 200;
		cc_ChartGap.AxisX[0].MinValue = DefalutValueGap;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_ItemSelectLW_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!init)
		{
			return;
		}
		try
		{
			ComboBox comboBox = (ComboBox)sender;
			int num = 0;
			string uid = comboBox.Uid;
			if (uid != null)
			{
				switch (uid.Length)
				{
				case 1:
					switch (uid[0])
					{
					case 'L':
						num = 0;
						break;
					case 'W':
						num = 1;
						break;
					case 'T':
						num = 2;
						break;
					}
					break;
				case 2:
					switch (uid[1])
					{
					case 'W':
						if (!(uid == "LW"))
						{
							if (uid == "RW")
							{
								num = 4;
							}
						}
						else
						{
							num = 3;
						}
						break;
					case '1':
						if (uid == "C1")
						{
							num = 6;
						}
						break;
					case '2':
						if (uid == "C2")
						{
							num = 7;
						}
						break;
					case '3':
						if (uid == "C3")
						{
							num = 8;
						}
						break;
					case '4':
						if (uid == "C4")
						{
							num = 9;
						}
						break;
					case '5':
						if (uid == "C5")
						{
							num = 10;
						}
						break;
					case '6':
						if (uid == "C6")
						{
							num = 11;
						}
						break;
					case '7':
						if (uid == "C7")
						{
							num = 12;
						}
						break;
					case '8':
						if (uid == "C8")
						{
							num = 13;
						}
						break;
					case '9':
						if (uid == "C9")
						{
							num = 14;
						}
						break;
					}
					break;
				case 3:
					switch (uid[2])
					{
					case 'p':
						if (uid == "Gap")
						{
							num = 5;
						}
						break;
					case '0':
						if (uid == "C10")
						{
							num = 15;
						}
						break;
					case '1':
						if (uid == "C11")
						{
							num = 16;
						}
						break;
					case '2':
						if (uid == "C12")
						{
							num = 17;
						}
						break;
					case '3':
						if (uid == "C13")
						{
							num = 18;
						}
						break;
					case '4':
						if (uid == "C14")
						{
							num = 19;
						}
						break;
					}
					break;
				}
			}
			if (num >= 0)
			{
				ItemSelect[num].Background = Brushes.White;
				if (comboBox.Uid == "LW")
				{
					btn_AlarmLW.Visibility = Visibility.Hidden;
				}
				else if (comboBox.Uid == "RW")
				{
					btn_AlarmRW.Visibility = Visibility.Hidden;
				}
				else
				{
					btn_AlarmGap.Visibility = Visibility.Hidden;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "SW05-SizeChartWindow-Cb_ItemSelectLW_SelectionChanged()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Calc_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (txt_Custom1.Text != "" && txt_Custom2.Text != "")
			{
				double num = Convert.ToDouble(txt_Custom1.Text);
				double num2 = Convert.ToDouble(txt_Custom2.Text);
				if (num > 0.0)
				{
					double num3 = num2 / num;
					txt_CustomRatio.Text = string.Format("{0:N3}", num3);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "SW05-SizeChartWindow-Btn_Calc_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void Btn_QuickSetupSave_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Right)
		{
			grd_Custom.Visibility = Visibility.Visible;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/sizechartwindow.xaml", UriKind.Relative);
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
			MainW = (SizeChartWindow)target;
			MainW.Loaded += Window_Loaded;
			break;
		case 2:
			tab_SizeFun = (MetroTabControl)target;
			tab_SizeFun.SelectionChanged += Tab_SizeFun_SelectionChanged;
			break;
		case 3:
			tab_Chart = (TabItem)target;
			break;
		case 4:
			cc_ChartL = (CartesianChart)target;
			cc_ChartL.DataHover += Chart_OnDataHover;
			cc_ChartL.MouseWheel += Cc_ChartL_MouseWheel;
			break;
		case 5:
			txt_MinL = (TextBox)target;
			break;
		case 6:
			txt_MaxL = (TextBox)target;
			break;
		case 7:
			txt_SigmaL = (TextBox)target;
			break;
		case 8:
			txt_AvgL = (TextBox)target;
			break;
		case 9:
			txt_RangeL = (TextBox)target;
			break;
		case 10:
			lb_ChartName = (Label)target;
			break;
		case 11:
			btn_NextL = (Button)target;
			btn_NextL.Click += Btn_Next_Click;
			break;
		case 12:
			btn_ForwordL = (Button)target;
			btn_ForwordL.Click += Btn_Forword_Click;
			break;
		case 13:
			cc_ChartW = (CartesianChart)target;
			break;
		case 14:
			txt_MinW = (TextBox)target;
			break;
		case 15:
			txt_MaxW = (TextBox)target;
			break;
		case 16:
			txt_SigmaW = (TextBox)target;
			break;
		case 17:
			txt_AvgW = (TextBox)target;
			break;
		case 18:
			txt_RangeW = (TextBox)target;
			break;
		case 19:
			lb_ChartName1 = (Label)target;
			break;
		case 20:
			btn_NextW = (Button)target;
			btn_NextW.Click += Btn_NextW_Click;
			break;
		case 21:
			btn_ForwordW = (Button)target;
			btn_ForwordW.Click += Btn_ForwordW_Click;
			break;
		case 22:
			cc_ChartT = (CartesianChart)target;
			break;
		case 23:
			txt_MinT = (TextBox)target;
			break;
		case 24:
			txt_MaxT = (TextBox)target;
			break;
		case 25:
			txt_SigmaT = (TextBox)target;
			break;
		case 26:
			txt_AvgT = (TextBox)target;
			break;
		case 27:
			txt_RangeT = (TextBox)target;
			break;
		case 28:
			lb_ChartName2 = (Label)target;
			break;
		case 29:
			btn_NextT = (Button)target;
			btn_NextT.Click += Btn_NextT_Click;
			break;
		case 30:
			btn_ForwordT = (Button)target;
			btn_ForwordT.Click += Btn_ForwordT_Click;
			break;
		case 31:
			tab_Custom = (TabItem)target;
			break;
		case 32:
			cc_ChartLW = (CartesianChart)target;
			cc_ChartLW.DataHover += Chart_OnDataHover;
			cc_ChartLW.MouseWheel += Cc_ChartL_MouseWheel;
			break;
		case 33:
			txt_MinLW = (TextBox)target;
			break;
		case 34:
			txt_MaxLW = (TextBox)target;
			break;
		case 35:
			txt_SigmaLW = (TextBox)target;
			break;
		case 36:
			txt_AvgLW = (TextBox)target;
			break;
		case 37:
			txt_RangeLW = (TextBox)target;
			break;
		case 38:
			lb_ChartNameLW = (Label)target;
			break;
		case 39:
			btn_NextLW = (Button)target;
			btn_NextLW.Click += Btn_NextLW_Click;
			break;
		case 40:
			btn_ForwordLW = (Button)target;
			btn_ForwordLW.Click += Btn_ForwordLW_Click;
			break;
		case 41:
			cc_ChartRW = (CartesianChart)target;
			break;
		case 42:
			txt_MinRW = (TextBox)target;
			break;
		case 43:
			txt_MaxRW = (TextBox)target;
			break;
		case 44:
			txt_SigmaRW = (TextBox)target;
			break;
		case 45:
			txt_AvgRW = (TextBox)target;
			break;
		case 46:
			txt_RangeRW = (TextBox)target;
			break;
		case 47:
			lb_ChartNameRW = (Label)target;
			break;
		case 48:
			btn_NextRW = (Button)target;
			btn_NextRW.Click += Btn_NextRW_Click;
			break;
		case 49:
			btn_ForwordRW = (Button)target;
			btn_ForwordRW.Click += Btn_ForwordRW_Click;
			break;
		case 50:
			cc_ChartGap = (CartesianChart)target;
			break;
		case 51:
			txt_MinGap = (TextBox)target;
			break;
		case 52:
			txt_MaxGap = (TextBox)target;
			break;
		case 53:
			txt_SigmaGap = (TextBox)target;
			break;
		case 54:
			txt_AvgGap = (TextBox)target;
			break;
		case 55:
			txt_RangeGap = (TextBox)target;
			break;
		case 56:
			lb_ChartNameGap = (Label)target;
			break;
		case 57:
			btn_NextGap = (Button)target;
			btn_NextGap.Click += Btn_NextGap_Click;
			break;
		case 58:
			btn_ForwordGap = (Button)target;
			btn_ForwordGap.Click += Btn_ForwordGap_Click;
			break;
		case 59:
			tab_Setup = (TabItem)target;
			break;
		case 60:
			cb_StationSelectL = (ComboBox)target;
			cb_StationSelectL.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 61:
			cb_ToolsSelectL = (ComboBox)target;
			cb_ToolsSelectL.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 62:
			cb_SubSelectL = (ComboBox)target;
			cb_SubSelectL.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 63:
			cb_ItemSelectL = (ComboBox)target;
			cb_ItemSelectL.SelectionChanged += Cb_ItemSelectL_SelectionChanged;
			break;
		case 64:
			btn_AlarmL = (Button)target;
			break;
		case 65:
			chk_LSizeEnable = (CheckBox)target;
			break;
		case 66:
			cb_StationSelectW = (ComboBox)target;
			cb_StationSelectW.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 67:
			cb_ToolsSelectW = (ComboBox)target;
			cb_ToolsSelectW.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 68:
			cb_SubSelectW = (ComboBox)target;
			cb_SubSelectW.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 69:
			cb_ItemSelectW = (ComboBox)target;
			cb_ItemSelectW.SelectionChanged += Cb_ItemSelectL_SelectionChanged;
			break;
		case 70:
			btn_AlarmW = (Button)target;
			break;
		case 71:
			chk_WSizeEnable = (CheckBox)target;
			break;
		case 72:
			cb_StationSelectT = (ComboBox)target;
			cb_StationSelectT.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 73:
			cb_ToolsSelectT = (ComboBox)target;
			cb_ToolsSelectT.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 74:
			cb_SubSelectT = (ComboBox)target;
			cb_SubSelectT.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 75:
			cb_ItemSelectT = (ComboBox)target;
			cb_ItemSelectT.SelectionChanged += Cb_ItemSelectL_SelectionChanged;
			break;
		case 76:
			btn_AlarmT = (Button)target;
			break;
		case 77:
			chk_TSizeEnable = (CheckBox)target;
			break;
		case 78:
			btn_QuickSetupSave = (Button)target;
			btn_QuickSetupSave.Click += Btn_QuickSetupSave_Click;
			btn_QuickSetupSave.PreviewMouseDoubleClick += Btn_QuickSetupSave_PreviewMouseDoubleClick;
			break;
		case 79:
			txt_ComDigitsName0 = (TextBox)target;
			txt_ComDigitsName0.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 80:
			txt_ComDigits0 = (TextBox)target;
			txt_ComDigits0.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 81:
			txt_ComDigitsName1 = (TextBox)target;
			txt_ComDigitsName1.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 82:
			txt_ComDigits1 = (TextBox)target;
			txt_ComDigits1.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 83:
			txt_ComDigitsName2 = (TextBox)target;
			txt_ComDigitsName2.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 84:
			txt_ComDigits2 = (TextBox)target;
			txt_ComDigits2.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 85:
			txt_ComDigitsName3 = (TextBox)target;
			txt_ComDigitsName3.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 86:
			txt_ComDigits3 = (TextBox)target;
			txt_ComDigits3.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 87:
			txt_ComDigitsName4 = (TextBox)target;
			txt_ComDigitsName4.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 88:
			txt_ComDigits4 = (TextBox)target;
			txt_ComDigits4.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 89:
			txt_ComDigitsName5 = (TextBox)target;
			txt_ComDigitsName5.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 90:
			txt_ComDigits5 = (TextBox)target;
			txt_ComDigits5.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 91:
			txt_ComDigitsName6 = (TextBox)target;
			txt_ComDigitsName6.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 92:
			txt_ComDigits6 = (TextBox)target;
			txt_ComDigits6.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 93:
			txt_ComDigitsName7 = (TextBox)target;
			txt_ComDigitsName7.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 94:
			txt_ComDigits7 = (TextBox)target;
			txt_ComDigits7.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 95:
			txt_ComDigitsName10 = (TextBox)target;
			txt_ComDigitsName10.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 96:
			txt_ComDigits10 = (TextBox)target;
			txt_ComDigits10.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 97:
			txt_ComDigitsName11 = (TextBox)target;
			txt_ComDigitsName11.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 98:
			txt_ComDigits11 = (TextBox)target;
			txt_ComDigits11.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 99:
			txt_ComDigitsName8 = (TextBox)target;
			txt_ComDigitsName8.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 100:
			txt_ComDigits8 = (TextBox)target;
			txt_ComDigits8.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 101:
			txt_ComDigitsName9 = (TextBox)target;
			txt_ComDigitsName9.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 102:
			txt_ComDigits9 = (TextBox)target;
			txt_ComDigits9.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 103:
			grd_Custom = (Grid)target;
			break;
		case 104:
			txt_CustomRatio = (TextBox)target;
			txt_CustomRatio.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 105:
			txt_Custom1 = (TextBox)target;
			txt_Custom1.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 106:
			txt_Custom2 = (TextBox)target;
			txt_Custom2.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 107:
			btn_Calc = (Button)target;
			btn_Calc.Click += Btn_Calc_Click;
			break;
		case 108:
			chk_EnableCustomRatio = (CheckBox)target;
			break;
		case 109:
			cb_StationSelectRW = (ComboBox)target;
			cb_StationSelectRW.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 110:
			cb_ToolsSelectRW = (ComboBox)target;
			cb_ToolsSelectRW.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 111:
			cb_SubSelectRW = (ComboBox)target;
			cb_SubSelectRW.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 112:
			cb_ItemSelectRW = (ComboBox)target;
			cb_ItemSelectRW.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 113:
			btn_AlarmRW = (Button)target;
			break;
		case 114:
			txt_RPadStr = (TextBox)target;
			break;
		case 115:
			chk_RPadSizeEnable = (CheckBox)target;
			break;
		case 116:
			cb_StationSelectGap = (ComboBox)target;
			cb_StationSelectGap.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 117:
			cb_ToolsSelectGap = (ComboBox)target;
			cb_ToolsSelectGap.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 118:
			cb_SubSelectGap = (ComboBox)target;
			cb_SubSelectGap.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 119:
			cb_ItemSelectGap = (ComboBox)target;
			cb_ItemSelectGap.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 120:
			btn_AlarmGap = (Button)target;
			break;
		case 121:
			txt_GapStr = (TextBox)target;
			break;
		case 122:
			chk_GapSizeEnable = (CheckBox)target;
			break;
		case 123:
			cb_StationSelectLW = (ComboBox)target;
			cb_StationSelectLW.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 124:
			cb_ToolsSelectLW = (ComboBox)target;
			cb_ToolsSelectLW.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 125:
			cb_SubSelectLW = (ComboBox)target;
			cb_SubSelectLW.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 126:
			cb_ItemSelectLW = (ComboBox)target;
			cb_ItemSelectLW.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 127:
			btn_AlarmLW = (Button)target;
			break;
		case 128:
			txt_LPadStr = (TextBox)target;
			break;
		case 129:
			chk_LPadSizeEnable = (CheckBox)target;
			break;
		case 130:
			tab_Setup2 = (TabItem)target;
			break;
		case 131:
			cb_StationSelectC5 = (ComboBox)target;
			cb_StationSelectC5.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 132:
			cb_ToolsSelectC5 = (ComboBox)target;
			cb_ToolsSelectC5.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 133:
			cb_SubSelectC5 = (ComboBox)target;
			cb_SubSelectC5.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 134:
			cb_ItemSelectC5 = (ComboBox)target;
			cb_ItemSelectC5.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 135:
			btn_AlarmC5 = (Button)target;
			break;
		case 136:
			txt_C5Str = (TextBox)target;
			break;
		case 137:
			chk_C5Enable = (CheckBox)target;
			break;
		case 138:
			cb_StationSelectC6 = (ComboBox)target;
			cb_StationSelectC6.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 139:
			cb_ToolsSelectC6 = (ComboBox)target;
			cb_ToolsSelectC6.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 140:
			cb_SubSelectC6 = (ComboBox)target;
			cb_SubSelectC6.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 141:
			cb_ItemSelectC6 = (ComboBox)target;
			cb_ItemSelectC6.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 142:
			btn_AlarmC6 = (Button)target;
			break;
		case 143:
			txt_C6Str = (TextBox)target;
			break;
		case 144:
			chk_C6Enable = (CheckBox)target;
			break;
		case 145:
			cb_StationSelectC4 = (ComboBox)target;
			cb_StationSelectC4.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 146:
			cb_ToolsSelectC4 = (ComboBox)target;
			cb_ToolsSelectC4.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 147:
			cb_SubSelectC4 = (ComboBox)target;
			cb_SubSelectC4.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 148:
			cb_ItemSelectC4 = (ComboBox)target;
			cb_ItemSelectC4.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 149:
			btn_AlarmC4 = (Button)target;
			break;
		case 150:
			txt_C4Str = (TextBox)target;
			break;
		case 151:
			chk_C4Enable = (CheckBox)target;
			break;
		case 152:
			cb_StationSelectC2 = (ComboBox)target;
			cb_StationSelectC2.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 153:
			cb_ToolsSelectC2 = (ComboBox)target;
			cb_ToolsSelectC2.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 154:
			cb_SubSelectC2 = (ComboBox)target;
			cb_SubSelectC2.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 155:
			cb_ItemSelectC2 = (ComboBox)target;
			cb_ItemSelectC2.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 156:
			btn_AlarmC2 = (Button)target;
			break;
		case 157:
			txt_C2Str = (TextBox)target;
			break;
		case 158:
			chk_C2Enable = (CheckBox)target;
			break;
		case 159:
			cb_StationSelectC3 = (ComboBox)target;
			cb_StationSelectC3.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 160:
			cb_ToolsSelectC3 = (ComboBox)target;
			cb_ToolsSelectC3.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 161:
			cb_SubSelectC3 = (ComboBox)target;
			cb_SubSelectC3.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 162:
			cb_ItemSelectC3 = (ComboBox)target;
			cb_ItemSelectC3.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 163:
			btn_AlarmC3 = (Button)target;
			break;
		case 164:
			txt_C3Str = (TextBox)target;
			break;
		case 165:
			chk_C3Enable = (CheckBox)target;
			break;
		case 166:
			cb_StationSelectC1 = (ComboBox)target;
			cb_StationSelectC1.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 167:
			cb_ToolsSelectC1 = (ComboBox)target;
			cb_ToolsSelectC1.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 168:
			cb_SubSelectC1 = (ComboBox)target;
			cb_SubSelectC1.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 169:
			cb_ItemSelectC1 = (ComboBox)target;
			cb_ItemSelectC1.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 170:
			btn_AlarmC1 = (Button)target;
			break;
		case 171:
			txt_C1Str = (TextBox)target;
			break;
		case 172:
			chk_C1Enable = (CheckBox)target;
			break;
		case 173:
			btn_QuickSetupSave_Copy = (Button)target;
			btn_QuickSetupSave_Copy.Click += Btn_QuickSetupSave_Click;
			btn_QuickSetupSave_Copy.PreviewMouseDoubleClick += Btn_QuickSetupSave_PreviewMouseDoubleClick;
			break;
		case 174:
			txt_ComDigitsName12 = (TextBox)target;
			txt_ComDigitsName12.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 175:
			txt_ComDigits12 = (TextBox)target;
			txt_ComDigits12.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 176:
			txt_ComDigitsName13 = (TextBox)target;
			txt_ComDigitsName13.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 177:
			txt_ComDigits13 = (TextBox)target;
			txt_ComDigits13.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 178:
			txt_ComDigitsName14 = (TextBox)target;
			txt_ComDigitsName14.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 179:
			txt_ComDigits14 = (TextBox)target;
			txt_ComDigits14.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 180:
			txt_ComDigitsName15 = (TextBox)target;
			txt_ComDigitsName15.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 181:
			txt_ComDigits15 = (TextBox)target;
			txt_ComDigits15.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 182:
			txt_ComDigitsName16 = (TextBox)target;
			txt_ComDigitsName16.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 183:
			txt_ComDigits16 = (TextBox)target;
			txt_ComDigits16.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 184:
			txt_ComDigitsName17 = (TextBox)target;
			txt_ComDigitsName17.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 185:
			txt_ComDigits17 = (TextBox)target;
			txt_ComDigits17.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 186:
			txt_ComDigitsName18 = (TextBox)target;
			txt_ComDigitsName18.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 187:
			txt_ComDigits18 = (TextBox)target;
			txt_ComDigits18.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 188:
			txt_ComDigitsName19 = (TextBox)target;
			txt_ComDigitsName19.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 189:
			txt_ComDigits19 = (TextBox)target;
			txt_ComDigits19.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 190:
			txt_ComDigitsName20 = (TextBox)target;
			txt_ComDigitsName20.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 191:
			txt_ComDigits20 = (TextBox)target;
			txt_ComDigits20.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 192:
			txt_ComDigitsName21 = (TextBox)target;
			txt_ComDigitsName21.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 193:
			txt_ComDigits21 = (TextBox)target;
			txt_ComDigits21.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 194:
			txt_ComDigitsName22 = (TextBox)target;
			txt_ComDigitsName22.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 195:
			txt_ComDigits22 = (TextBox)target;
			txt_ComDigits22.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 196:
			txt_ComDigitsName23 = (TextBox)target;
			txt_ComDigitsName23.PreviewMouseDoubleClick += Txt_ComDigitsName0_PreviewMouseDoubleClick;
			break;
		case 197:
			txt_ComDigits23 = (TextBox)target;
			txt_ComDigits23.PreviewMouseLeftButtonDown += Txt_ComDigitsLButtonDown;
			break;
		case 198:
			grd_C7 = (Grid)target;
			break;
		case 199:
			cb_StationSelectC7 = (ComboBox)target;
			cb_StationSelectC7.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 200:
			cb_ToolsSelectC7 = (ComboBox)target;
			cb_ToolsSelectC7.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 201:
			cb_SubSelectC7 = (ComboBox)target;
			cb_SubSelectC7.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 202:
			cb_ItemSelectC7 = (ComboBox)target;
			cb_ItemSelectC7.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 203:
			btn_AlarmC7 = (Button)target;
			break;
		case 204:
			txt_C7Str = (TextBox)target;
			break;
		case 205:
			chk_C7Enable = (CheckBox)target;
			break;
		case 206:
			tab_Setup3 = (TabItem)target;
			break;
		case 207:
			cb_StationSelectC8 = (ComboBox)target;
			cb_StationSelectC8.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 208:
			cb_ToolsSelectC8 = (ComboBox)target;
			cb_ToolsSelectC8.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 209:
			cb_SubSelectC8 = (ComboBox)target;
			cb_SubSelectC8.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 210:
			cb_ItemSelectC8 = (ComboBox)target;
			cb_ItemSelectC8.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 211:
			btn_AlarmC8 = (Button)target;
			break;
		case 212:
			txt_C8Str = (TextBox)target;
			break;
		case 213:
			chk_C8Enable = (CheckBox)target;
			break;
		case 214:
			cb_StationSelectC9 = (ComboBox)target;
			cb_StationSelectC9.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 215:
			cb_ToolsSelectC9 = (ComboBox)target;
			cb_ToolsSelectC9.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 216:
			cb_SubSelectC9 = (ComboBox)target;
			cb_SubSelectC9.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 217:
			cb_ItemSelectC9 = (ComboBox)target;
			cb_ItemSelectC9.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 218:
			btn_AlarmC9 = (Button)target;
			break;
		case 219:
			txt_C9Str = (TextBox)target;
			break;
		case 220:
			chk_C9Enable = (CheckBox)target;
			break;
		case 221:
			cb_StationSelectC10 = (ComboBox)target;
			cb_StationSelectC10.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 222:
			cb_ToolsSelectC10 = (ComboBox)target;
			cb_ToolsSelectC10.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 223:
			cb_SubSelectC10 = (ComboBox)target;
			cb_SubSelectC10.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 224:
			cb_ItemSelectC10 = (ComboBox)target;
			cb_ItemSelectC10.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 225:
			btn_AlarmC10 = (Button)target;
			break;
		case 226:
			txt_C10Str = (TextBox)target;
			break;
		case 227:
			chk_C10Enable = (CheckBox)target;
			break;
		case 228:
			cb_StationSelectC11 = (ComboBox)target;
			cb_StationSelectC11.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 229:
			cb_ToolsSelectC11 = (ComboBox)target;
			cb_ToolsSelectC11.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 230:
			cb_SubSelectC11 = (ComboBox)target;
			cb_SubSelectC11.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 231:
			cb_ItemSelectC11 = (ComboBox)target;
			cb_ItemSelectC11.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 232:
			btn_AlarmC11 = (Button)target;
			break;
		case 233:
			txt_C11Str = (TextBox)target;
			break;
		case 234:
			chk_C11Enable = (CheckBox)target;
			break;
		case 235:
			cb_StationSelectC12 = (ComboBox)target;
			cb_StationSelectC12.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 236:
			cb_ToolsSelectC12 = (ComboBox)target;
			cb_ToolsSelectC12.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 237:
			cb_SubSelectC12 = (ComboBox)target;
			cb_SubSelectC12.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 238:
			cb_ItemSelectC12 = (ComboBox)target;
			cb_ItemSelectC12.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 239:
			btn_AlarmC12 = (Button)target;
			break;
		case 240:
			txt_C12Str = (TextBox)target;
			break;
		case 241:
			chk_C12Enable = (CheckBox)target;
			break;
		case 242:
			cb_StationSelectC13 = (ComboBox)target;
			cb_StationSelectC13.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 243:
			cb_ToolsSelectC13 = (ComboBox)target;
			cb_ToolsSelectC13.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 244:
			cb_SubSelectC13 = (ComboBox)target;
			cb_SubSelectC13.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 245:
			cb_ItemSelectC13 = (ComboBox)target;
			cb_ItemSelectC13.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 246:
			btn_AlarmC13 = (Button)target;
			break;
		case 247:
			txt_C13Str = (TextBox)target;
			break;
		case 248:
			chk_C13Enable = (CheckBox)target;
			break;
		case 249:
			cb_StationSelectC14 = (ComboBox)target;
			cb_StationSelectC14.SelectionChanged += Cb_StationSelectL_SelectionChanged;
			break;
		case 250:
			cb_ToolsSelectC14 = (ComboBox)target;
			cb_ToolsSelectC14.SelectionChanged += Cb_ToolsSelectL_SelectionChanged;
			break;
		case 251:
			cb_SubSelectC14 = (ComboBox)target;
			cb_SubSelectC14.SelectionChanged += Cb_SubSelectL_SelectionChanged;
			break;
		case 252:
			cb_ItemSelectC14 = (ComboBox)target;
			cb_ItemSelectC14.SelectionChanged += Cb_ItemSelectLW_SelectionChanged;
			break;
		case 253:
			btn_AlarmC14 = (Button)target;
			break;
		case 254:
			txt_C14Str = (TextBox)target;
			break;
		case 255:
			chk_C14Enable = (CheckBox)target;
			break;
		case 256:
			btn_QuickSetupSave_Copy1 = (Button)target;
			btn_QuickSetupSave_Copy1.Click += Btn_QuickSetupSave_Click;
			btn_QuickSetupSave_Copy1.PreviewMouseDoubleClick += Btn_QuickSetupSave_PreviewMouseDoubleClick;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
