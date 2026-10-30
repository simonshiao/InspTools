using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using HiroPChart;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using LiveCharts;
using LiveCharts.Defaults;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using OpenCvSharp;

namespace InspTools.Other;

public class ParetoChartWindow : System.Windows.Window, IComponentConnector
{
	private CVInspection[] Station;

	private int Index;

	private int ID;

	private int DefalutValue;

	private int BlobType;

	private int LanguageType = 1;

	private int TrackNo = 1;

	private bool init;

	public double ValueMin;

	public double ValueMax;

	private List<ParetoItem> PItems = new List<ParetoItem>();

	private List<ParetoSatisicsItem> PSItems = new List<ParetoSatisicsItem>();

	private List<ParetoItemPreview> PVItems = new List<ParetoItemPreview>();

	private ThroughputParamFile ThroughputFile;

	private HPChartPage HiroPChart = new HPChartPage();

	private DataTable dt = new DataTable("ParetoChartData");

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

	internal ParetoChartWindow ParetoMain;

	internal MetroTabControl tab_ParetoFun;

	internal TabItem tab_Chart;

	internal ListView lv_ParetoStatistics;

	internal Button btn_QuickSetupSave;

	internal Frame frame;

	internal Button btn_ShowChartS1;

	internal Button btn_ShowChartS2;

	internal Button btn_ShowChartS3;

	internal Button btn_ShowChartS4;

	internal Button btn_ShowChartS5;

	internal Button btn_ShowChartS6;

	internal Button btn_ShowChartS7;

	internal Button btn_ShowChartS8;

	internal TabItem tab_Setup;

	internal ListView lv_ParetoList;

	internal Button btn_ItemAdd;

	internal Button btn_ItemUp;

	internal Button btn_ItemDowm;

	internal Button btn_ItemDelete;

	internal Button btn_ItemEdit;

	internal CheckBox chk_EnableMultiPareto;

	internal CheckBox chk_EnableThroughputWindow;

	internal CheckBox chk_EnableReverseBin;

	internal TabItem tab_ItemPreview;

	internal ListView lv_ParetoListPreview;

	internal ComboBox cb_ParetoItem;

	internal ComboBox cb_Station;

	private bool _contentLoaded;

	public ChartValues<ObservablePoint> Chart1ValuesA { get; set; }

	public ChartValues<ObservablePoint> Chart1ValuesB { get; set; }

	public ChartValues<ObservablePoint> Chart1ValuesMin { get; set; }

	public ChartValues<ObservablePoint> Chart1ValuesMax { get; set; }

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ParetoChartWindow()
	{
		InitializeComponent();
		tab_ParetoFun.SelectedIndex = 0;
		init = true;
		tab_Chart.Background = Brushes.Yellow;
		loadGridColums();
	}

	public void SetInspection(CVInspection[] station)
	{
		Station = station;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetRecipe(ThroughputParamFile file, bool offline = true)
	{
		ThroughputFile = file;
		if (offline)
		{
			lv_ParetoList.ItemsSource = null;
			lv_ParetoStatistics.ItemsSource = null;
		}
		PItems.Clear();
		PSItems.Clear();
		try
		{
			double[] array = new double[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = 0.0;
				for (int j = 1; j < 30; j++)
				{
					array[i] += Station[i].ParetoChartTable[j];
				}
			}
			chk_EnableMultiPareto.IsChecked = ThroughputFile.GetEnableMultiParetoMode();
			chk_EnableThroughputWindow.IsChecked = ThroughputFile.GetEnableThroughputWindow();
			chk_EnableReverseBin.IsChecked = ThroughputFile.param.BinReverseMode;
			for (int k = 1; k < 30; k++)
			{
				if (!(ThroughputFile.param.AllParetoItems[k] != ""))
				{
					continue;
				}
				string[] array2 = new string[8];
				int[] array3 = new int[8];
				for (int l = 0; l < 8; l++)
				{
					if (array[l] == 0.0)
					{
						array2[l] = "0 (0%)";
						array3[l] = 0;
					}
					else
					{
						array2[l] = string.Format("{0} ({1:N1}%)", Station[l].ParetoChartTable[k], (double)Station[l].ParetoChartTable[k] / array[l] * 100.0);
						array3[l] = Station[l].ParetoChartTable[k];
					}
				}
				PItems.Add(new ParetoItem
				{
					Name = ThroughputFile.param.AllParetoItems[k],
					Priority = PItems.Count + 1
				});
				ParetoSatisicsItem item = new ParetoSatisicsItem(ThroughputFile.param.AllParetoItems[k], array3, array2);
				PSItems.Add(item);
			}
			if (offline)
			{
				UpdatePriority();
				lv_ParetoList.ItemsSource = PItems;
				lv_ParetoStatistics.ItemsSource = PSItems;
			}
		}
		catch (Exception ex)
		{
			string text = "PCW02-ParetoChartWindow-SetRecipe()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	public ThroughputParamFile GetRecipe()
	{
		return ThroughputFile;
	}

	public void UpdatePriority()
	{
		for (int i = 0; i < PItems.Count; i++)
		{
			PItems[i].Priority = i + 1;
		}
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ItemAdd_Click(object sender, RoutedEventArgs e)
	{
		if (PItems.Count >= 29)
		{
			MessageBox.Show("項目已達最大限制！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			return;
		}
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr != "")
		{
			lv_ParetoList.ItemsSource = null;
			PItems.Add(new ParetoItem
			{
				Name = itemInputWindow.InputStr,
				Priority = PItems.Count + 1
			});
			UpdatePriority();
			lv_ParetoList.ItemsSource = PItems;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ItemDelete_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = lv_ParetoList.SelectedIndex;
			if (selectedIndex < 0 || selectedIndex >= PItems.Count)
			{
				return;
			}
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected item will be delete！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("选取的项目即将删除！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("選取的項目即將刪除！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult != MessageBoxResult.Yes || messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
			lv_ParetoList.ItemsSource = null;
			PItems.RemoveAt(selectedIndex);
			ResetParetoIndex(selectedIndex + 1, 0);
			if (selectedIndex + 2 <= PItems.Count)
			{
				for (int i = selectedIndex + 2; i <= PItems.Count + 1; i++)
				{
					ResetParetoIndex(i, i - 1);
				}
			}
			UpdatePriority();
			lv_ParetoList.ItemsSource = PItems;
		}
		catch (Exception ex)
		{
			string text = "PCW03-ParetoChartWindow-Btn_ItemDelete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ItemUp_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = lv_ParetoList.SelectedIndex;
		if (selectedIndex <= 0 || selectedIndex >= PItems.Count)
		{
			return;
		}
		try
		{
			lv_ParetoList.ItemsSource = null;
			ParetoItem value = PItems[selectedIndex];
			ParetoItem value2 = PItems[selectedIndex - 1];
			PItems[selectedIndex] = value2;
			PItems[selectedIndex - 1] = value;
			UpdateParetoIndex(selectedIndex, selectedIndex + 1);
			UpdatePriority();
			lv_ParetoList.ItemsSource = PItems;
			lv_ParetoList.SelectedIndex = selectedIndex - 1;
		}
		catch (Exception ex)
		{
			string text = "PCW04-ParetoChartWindow-Btn_ItemUp_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ItemDowm_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = lv_ParetoList.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < PItems.Count - 1)
			{
				lv_ParetoList.ItemsSource = null;
				ParetoItem value = PItems[selectedIndex];
				ParetoItem value2 = PItems[selectedIndex + 1];
				PItems[selectedIndex] = value2;
				PItems[selectedIndex + 1] = value;
				UpdateParetoIndex(selectedIndex + 1, selectedIndex + 2);
				UpdatePriority();
				lv_ParetoList.ItemsSource = PItems;
				lv_ParetoList.SelectedIndex = selectedIndex + 1;
			}
		}
		catch (Exception ex)
		{
			string text = "PCW04-ParetoChartWindow-Btn_ItemDowm_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void ParetoMain_Closing(object sender, CancelEventArgs e)
	{
		lv_ParetoList.ItemsSource = null;
		for (int i = 1; i < 30; i++)
		{
			ThroughputFile.param.AllParetoItems[i] = "";
		}
		for (int j = 0; j < PItems.Count && j + 1 < 30; j++)
		{
			if (PItems[j].Name != "")
			{
				ThroughputFile.param.AllParetoItems[j + 1] = PItems[j].Name;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Tab_ParetoFun_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (init && e.Source is TabControl && e.AddedItems[0] is TabItem tabItem)
		{
			tab_Chart.Background = Brushes.White;
			tab_Setup.Background = Brushes.White;
			tab_ItemPreview.Background = Brushes.White;
			if (tabItem.Name == "tab_Chart")
			{
				lv_ParetoStatistics.ItemsSource = null;
				GetParetoChart();
				lv_ParetoStatistics.ItemsSource = PSItems;
				tab_Chart.Background = Brushes.Yellow;
			}
			else if (tabItem.Name == "tab_Setup")
			{
				tab_Setup.Background = Brushes.Yellow;
			}
			else
			{
				ReDrawPreview();
				tab_ItemPreview.Background = Brushes.Yellow;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<ParetoSatisicsItem> GetParetoChart()
	{
		try
		{
			PSItems.Clear();
			double[] array = new double[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = 0.0;
				for (int j = 1; j < 30; j++)
				{
					if (j - 1 < PItems.Count && PItems[j - 1].Name != "")
					{
						array[i] += Station[i].ParetoChartTable[j];
					}
				}
			}
			for (int k = 0; k < PItems.Count; k++)
			{
				if (!(PItems[k].Name != ""))
				{
					continue;
				}
				string[] array2 = new string[8];
				int[] array3 = new int[8];
				for (int l = 0; l < 8; l++)
				{
					if (array[l] == 0.0)
					{
						array2[l] = "0 (0%)";
						array3[l] = 0;
					}
					else
					{
						array3[l] = Station[l].ParetoChartTable[k + 1];
						array2[l] = string.Format("{0} ({1:N1}%)", Station[l].ParetoChartTable[k + 1], (double)Station[l].ParetoChartTable[k + 1] / array[l] * 100.0);
					}
				}
				ParetoSatisicsItem item = new ParetoSatisicsItem(PItems[k].Name, array3, array2);
				PSItems.Add(item);
			}
			return PSItems;
		}
		catch (Exception ex)
		{
			string text = "PCW01-ParetoChartWindow-GetParetoChart()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return PSItems;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ReDrawPreview()
	{
		try
		{
			cb_ParetoItem.Items.Clear();
			if (PItems.Count > 0)
			{
				cb_ParetoItem.Items.Add(LanguageManager.Instance["Unused"]);
				for (int i = 0; i < PItems.Count; i++)
				{
					cb_ParetoItem.Items.Add(PItems[i].Name);
				}
				cb_ParetoItem.SelectedIndex = 0;
				UpdatePreviewItem(cb_Station.SelectedIndex, 0);
			}
		}
		catch (Exception ex)
		{
			string text = "PCW05-ParetoChartWindow-ReDrawPreview()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdatePreviewItem(int stationindex, int PretoIndex)
	{
		if (Station == null || stationindex < 0 || stationindex >= 8)
		{
			return;
		}
		try
		{
			lv_ParetoListPreview.ItemsSource = null;
			PVItems.Clear();
			CVInspection cVInspection = Station[stationindex];
			if (cVInspection.paramFile.LocateParam.ParetoIndex == PretoIndex)
			{
				PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["LocateTool"], 0, LanguageManager.Instance["MeasureTool"]));
			}
			for (int i = 2; i < 20; i++)
			{
				if (cVInspection.paramFile.VSLocateParam[i].TrackerON && cVInspection.paramFile.VSLocateParam[i].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["AdvLocateTool1"], i + 1, cVInspection.paramFile.VSLocateParam[i].TrackerName));
				}
			}
			for (int j = 2; j < 20; j++)
			{
				if (cVInspection.paramFile.HSLocateParam[j].TrackerON && cVInspection.paramFile.HSLocateParam[j].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["AdvLocateTool2"], j + 1, cVInspection.paramFile.HSLocateParam[j].TrackerName));
				}
			}
			for (int k = 1; k < 20; k++)
			{
				if (cVInspection.paramFile.AdvLocateParam[k].TrackerON && cVInspection.paramFile.AdvLocateParam[k].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["AdvLocateTool3"], k + 1, cVInspection.paramFile.AdvLocateParam[k].TrackerName));
				}
			}
			for (int l = 0; l < 40; l++)
			{
				if (cVInspection.paramFile.AdvLocateMParam[l].TrackerON && cVInspection.paramFile.AdvLocateMParam[l].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["Adv.Measure"], l + 1, cVInspection.paramFile.AdvLocateMParam[l].TrackerName));
				}
			}
			for (int m = 0; m < 20; m++)
			{
				if (!cVInspection.paramFile.MeasureToolON)
				{
					break;
				}
				if (cVInspection.paramFile.MeaWidthParam[m].TrackerON && cVInspection.paramFile.MeaWidthParam[m].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["MeasureTool1"], m + 1, cVInspection.paramFile.MeaWidthParam[m].TrackerName));
				}
			}
			for (int n = 0; n < 20; n++)
			{
				if (!cVInspection.paramFile.MeasureToolON)
				{
					break;
				}
				if (cVInspection.paramFile.MeaHeightParam[n].TrackerON && cVInspection.paramFile.MeaHeightParam[n].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["MeasureTool2"], n + 1, cVInspection.paramFile.MeaHeightParam[n].TrackerName));
				}
			}
			for (int num = 0; num < 30; num++)
			{
				if (!cVInspection.paramFile.BinaryToolON)
				{
					break;
				}
				if (cVInspection.paramFile.BinaryParam[num].TrackerON && cVInspection.paramFile.BinaryParam[num].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["BinaryTool"], num + 1, cVInspection.paramFile.BinaryParam[num].TrackerName));
				}
			}
			for (int num2 = 0; num2 < 20; num2++)
			{
				if (!cVInspection.paramFile.SmoothToolON)
				{
					break;
				}
				if (cVInspection.paramFile.SmoothParam[num2].TrackerON && cVInspection.paramFile.SmoothParam[num2].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["SmoothTool"], num2 + 1, cVInspection.paramFile.SmoothParam[num2].TrackerName));
				}
			}
			for (int num3 = 0; num3 < 20; num3++)
			{
				if (!cVInspection.paramFile.BlockAvgToolON)
				{
					break;
				}
				if (cVInspection.paramFile.BlockAvgParam[num3].TrackerON && cVInspection.paramFile.BlockAvgParam[num3].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["BlockAvgTool"], num3 + 1, cVInspection.paramFile.BlockAvgParam[num3].TrackerName));
				}
			}
			for (int num4 = 0; num4 < 20; num4++)
			{
				if (!cVInspection.paramFile.DifferenceToolON)
				{
					break;
				}
				if (cVInspection.paramFile.DifferenceParam[num4].TrackerON && cVInspection.paramFile.DifferenceParam[num4].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["DifferenceTool"], num4 + 1, cVInspection.paramFile.DifferenceParam[num4].TrackerName));
				}
			}
			for (int num5 = 0; num5 < 20; num5++)
			{
				if (!cVInspection.paramFile.LineAnalysisToolON)
				{
					break;
				}
				if (cVInspection.paramFile.LineAnalysisParam[num5].TrackerON && cVInspection.paramFile.LineAnalysisParam[num5].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["LineAnalysisTool"], num5 + 1, cVInspection.paramFile.LineAnalysisParam[num5].TrackerName));
				}
			}
			for (int num6 = 0; num6 < 20; num6++)
			{
				if (!cVInspection.paramFile.MaskToolON)
				{
					break;
				}
				if (cVInspection.paramFile.MatchParam[num6].TrackerON && cVInspection.paramFile.MatchParam[num6].ParetoIndex == PretoIndex)
				{
					PVItems.Add(new ParetoItemPreview(LanguageManager.Instance["MatchTool"], num6 + 1, cVInspection.paramFile.MatchParam[num6].TrackerName));
				}
			}
			lv_ParetoListPreview.ItemsSource = PVItems;
		}
		catch (Exception ex)
		{
			string text = "PCW06-ParetoChartWindow-UpdatePreviewItem()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ItemEdit_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = lv_ParetoList.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < PItems.Count)
			{
				ItemInputWindow itemInputWindow = new ItemInputWindow();
				itemInputWindow.ShowDialog();
				if (itemInputWindow.InputStr != "")
				{
					lv_ParetoList.ItemsSource = null;
					PItems[selectedIndex].Name = itemInputWindow.InputStr;
					UpdatePriority();
					lv_ParetoList.ItemsSource = PItems;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "PCW03-ParetoChartWindow-Btn_ItemDelete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Lv_ParetoList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		try
		{
			int selectedIndex = lv_ParetoList.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < PItems.Count)
			{
				ItemInputWindow itemInputWindow = new ItemInputWindow();
				itemInputWindow.ShowDialog();
				if (itemInputWindow.InputStr != "")
				{
					lv_ParetoList.ItemsSource = null;
					PItems[selectedIndex].Name = itemInputWindow.InputStr;
					UpdatePriority();
					lv_ParetoList.ItemsSource = PItems;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "PCW03-ParetoChartWindow-Btn_ItemDelete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void Cb_Station_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (cb_ParetoItem.SelectedIndex >= 0 && PSItems.Count > 0 && cb_Station.SelectedIndex >= 0)
		{
			UpdatePreviewItem(cb_Station.SelectedIndex, cb_ParetoItem.SelectedIndex);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_QuickSetupSave_Click(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Csv files (*.Csv;)|*.csv;|All files (*.*)|*.*";
		if (saveFileDialog.ShowDialog() == true)
		{
			if (!Directory.Exists(Path.GetDirectoryName(saveFileDialog.FileName)))
			{
				MessageBox.Show(LanguageManager.Instance["Error"], LanguageManager.Instance["Error"]);
			}
			else
			{
				SaveCSV(saveFileDialog.FileName);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SaveCSV(string FileName)
	{
		if (PSItems.Count <= 0)
		{
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(FileName, append: true);
		streamWriter.Write(", 1, 2, 3, 4, 5, 6, 7, 8 \n");
		for (int i = 0; i < PSItems.Count; i++)
		{
			string value = string.Format("{0}, {1},{2},{3},{4},{5},{6},{7},{8} \n", PSItems[i].Name, PSItems[i].Station1, PSItems[i].Station2, PSItems[i].Station3, PSItems[i].Station4, PSItems[i].Station5, PSItems[i].Station6, PSItems[i].Station7, PSItems[i].Station8);
			streamWriter.Write(value);
		}
		streamWriter.WriteLine();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void loadGridColums()
	{
		dt.Columns.Add("No", typeof(string));
		dt.Columns.Add("Names", typeof(string));
		dt.Columns.Add("Frequency");
		dt.Columns["Frequency"].DataType = Type.GetType("System.Int32");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CreatParetoTable(int index)
	{
		try
		{
			dt.Clear();
			dt.Rows.Clear();
			for (int i = 0; i < PSItems.Count; i++)
			{
				DataRow dataRow = dt.NewRow();
				dataRow["No"] = i.ToString();
				dataRow["Names"] = PSItems[i].Name;
				dataRow["Frequency"] = PSItems[i].Value[index];
				dt.Rows.Add(dataRow);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("PCW04-ParetoChartWindow-Btn_ShowChartS1_Click()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ShowChartS1_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			CreatParetoTable(num);
			frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
			frame.Content = HiroPChart;
			HiroPChart.ItemFontSize = 11;
			HiroPChart.ChartTitleStr = string.Format("Station {0}", num + 1);
			HiroPChart.SetChartSize(frame.RenderSize.Width, frame.RenderSize.Height);
			HiroPChart.Bindgrid(dt);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("PCW04-ParetoChartWindow-Btn_ShowChartS1_Click()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateParetoIndex(int switch1, int switch2)
	{
		if (Station == null)
		{
			return;
		}
		try
		{
			for (int i = 0; i < 8; i++)
			{
				CVInspection cVInspection = Station[i];
				if (cVInspection.paramFile.LocateParam.ParetoIndex == switch1)
				{
					cVInspection.paramFile.LocateParam.ParetoIndex = switch2;
					cVInspection.paramFile.LocateParam.ValueChangeFlag = true;
				}
				else if (cVInspection.paramFile.LocateParam.ParetoIndex == switch2)
				{
					cVInspection.paramFile.LocateParam.ParetoIndex = switch1;
					cVInspection.paramFile.LocateParam.ValueChangeFlag = true;
				}
				for (int j = 2; j < 20; j++)
				{
					if (cVInspection.paramFile.VSLocateParam[j].ParetoIndex == switch1)
					{
						cVInspection.paramFile.VSLocateParam[j].ParetoIndex = switch2;
						cVInspection.paramFile.VSLocateParam[j].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.VSLocateParam[j].ParetoIndex == switch2)
					{
						cVInspection.paramFile.VSLocateParam[j].ParetoIndex = switch1;
						cVInspection.paramFile.VSLocateParam[j].ValueChangeFlag = true;
					}
				}
				for (int k = 2; k < 20; k++)
				{
					if (cVInspection.paramFile.HSLocateParam[k].ParetoIndex == switch1)
					{
						cVInspection.paramFile.HSLocateParam[k].ParetoIndex = switch2;
						cVInspection.paramFile.HSLocateParam[k].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.HSLocateParam[k].ParetoIndex == switch2)
					{
						cVInspection.paramFile.HSLocateParam[k].ParetoIndex = switch1;
						cVInspection.paramFile.HSLocateParam[k].ValueChangeFlag = true;
					}
				}
				for (int l = 1; l < 20; l++)
				{
					if (cVInspection.paramFile.AdvLocateParam[l].ParetoIndex == switch1)
					{
						cVInspection.paramFile.AdvLocateParam[l].ParetoIndex = switch2;
						cVInspection.paramFile.AdvLocateParam[l].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.AdvLocateParam[l].ParetoIndex == switch2)
					{
						cVInspection.paramFile.AdvLocateParam[l].ParetoIndex = switch1;
						cVInspection.paramFile.AdvLocateParam[l].ValueChangeFlag = true;
					}
				}
				for (int m = 0; m < 40; m++)
				{
					if (cVInspection.paramFile.AdvLocateMParam[m].ParetoIndex == switch1)
					{
						cVInspection.paramFile.AdvLocateMParam[m].ParetoIndex = switch2;
						cVInspection.paramFile.AdvLocateMParam[m].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.AdvLocateMParam[m].ParetoIndex == switch2)
					{
						cVInspection.paramFile.AdvLocateMParam[m].ParetoIndex = switch1;
						cVInspection.paramFile.AdvLocateMParam[m].ValueChangeFlag = true;
					}
				}
				for (int n = 0; n < 20; n++)
				{
					if (!cVInspection.paramFile.MeasureToolON)
					{
						break;
					}
					if (cVInspection.paramFile.MeaWidthParam[n].ParetoIndex == switch1)
					{
						cVInspection.paramFile.MeaWidthParam[n].ParetoIndex = switch2;
						cVInspection.paramFile.MeaWidthParam[n].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.MeaWidthParam[n].ParetoIndex == switch2)
					{
						cVInspection.paramFile.MeaWidthParam[n].ParetoIndex = switch1;
						cVInspection.paramFile.MeaWidthParam[n].ValueChangeFlag = true;
					}
				}
				for (int num = 0; num < 20; num++)
				{
					if (!cVInspection.paramFile.MeasureToolON)
					{
						break;
					}
					if (cVInspection.paramFile.MeaHeightParam[num].ParetoIndex == switch1)
					{
						cVInspection.paramFile.MeaHeightParam[num].ParetoIndex = switch2;
						cVInspection.paramFile.MeaHeightParam[num].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.MeaHeightParam[num].ParetoIndex == switch2)
					{
						cVInspection.paramFile.MeaHeightParam[num].ParetoIndex = switch1;
						cVInspection.paramFile.MeaHeightParam[num].ValueChangeFlag = true;
					}
				}
				for (int num2 = 0; num2 < 30; num2++)
				{
					if (!cVInspection.paramFile.BinaryToolON)
					{
						break;
					}
					if (cVInspection.paramFile.BinaryParam[num2].ParetoIndex == switch1)
					{
						cVInspection.paramFile.BinaryParam[num2].ParetoIndex = switch2;
						cVInspection.paramFile.BinaryParam[num2].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.BinaryParam[num2].ParetoIndex == switch2)
					{
						cVInspection.paramFile.BinaryParam[num2].ParetoIndex = switch1;
						cVInspection.paramFile.BinaryParam[num2].ValueChangeFlag = true;
					}
				}
				for (int num3 = 0; num3 < 20; num3++)
				{
					if (!cVInspection.paramFile.SmoothToolON)
					{
						break;
					}
					if (cVInspection.paramFile.SmoothParam[num3].ParetoIndex == switch1)
					{
						cVInspection.paramFile.SmoothParam[num3].ParetoIndex = switch2;
						cVInspection.paramFile.SmoothParam[num3].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.SmoothParam[num3].ParetoIndex == switch2)
					{
						cVInspection.paramFile.SmoothParam[num3].ParetoIndex = switch1;
						cVInspection.paramFile.SmoothParam[num3].ValueChangeFlag = true;
					}
				}
				for (int num4 = 0; num4 < 20; num4++)
				{
					if (!cVInspection.paramFile.BlockAvgToolON)
					{
						break;
					}
					if (cVInspection.paramFile.BlockAvgParam[num4].ParetoIndex == switch1)
					{
						cVInspection.paramFile.BlockAvgParam[num4].ParetoIndex = switch2;
						cVInspection.paramFile.BlockAvgParam[num4].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.BlockAvgParam[num4].ParetoIndex == switch2)
					{
						cVInspection.paramFile.BlockAvgParam[num4].ParetoIndex = switch1;
						cVInspection.paramFile.BlockAvgParam[num4].ValueChangeFlag = true;
					}
				}
				for (int num5 = 0; num5 < 20; num5++)
				{
					if (!cVInspection.paramFile.DifferenceToolON)
					{
						break;
					}
					if (cVInspection.paramFile.DifferenceParam[num5].ParetoIndex == switch1)
					{
						cVInspection.paramFile.DifferenceParam[num5].ParetoIndex = switch2;
						cVInspection.paramFile.DifferenceParam[num5].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.DifferenceParam[num5].ParetoIndex == switch2)
					{
						cVInspection.paramFile.DifferenceParam[num5].ParetoIndex = switch1;
						cVInspection.paramFile.DifferenceParam[num5].ValueChangeFlag = true;
					}
				}
				for (int num6 = 0; num6 < 20; num6++)
				{
					if (!cVInspection.paramFile.LineAnalysisToolON)
					{
						break;
					}
					if (cVInspection.paramFile.LineAnalysisParam[num6].ParetoIndex == switch1)
					{
						cVInspection.paramFile.LineAnalysisParam[num6].ParetoIndex = switch2;
						cVInspection.paramFile.LineAnalysisParam[num6].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.LineAnalysisParam[num6].ParetoIndex == switch2)
					{
						cVInspection.paramFile.LineAnalysisParam[num6].ParetoIndex = switch1;
						cVInspection.paramFile.LineAnalysisParam[num6].ValueChangeFlag = true;
					}
				}
				for (int num7 = 0; num7 < 20; num7++)
				{
					if (!cVInspection.paramFile.MaskToolON)
					{
						break;
					}
					if (cVInspection.paramFile.MatchParam[num7].ParetoIndex == switch1)
					{
						cVInspection.paramFile.MatchParam[num7].ParetoIndex = switch2;
						cVInspection.paramFile.MatchParam[num7].ValueChangeFlag = true;
					}
					else if (cVInspection.paramFile.MatchParam[num7].ParetoIndex == switch2)
					{
						cVInspection.paramFile.MatchParam[num7].ParetoIndex = switch1;
						cVInspection.paramFile.MatchParam[num7].ValueChangeFlag = true;
					}
				}
			}
		}
		catch (Exception ex)
		{
			string text = "PCW06-ParetoChartWindow-UpdateParetoIndex()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ResetParetoIndex(int switch1, int value)
	{
		if (Station == null)
		{
			return;
		}
		try
		{
			for (int i = 0; i < 8; i++)
			{
				CVInspection cVInspection = Station[i];
				if (cVInspection.paramFile.LocateParam.ParetoIndex == switch1)
				{
					cVInspection.paramFile.LocateParam.ParetoIndex = value;
					cVInspection.paramFile.LocateParam.ValueChangeFlag = true;
				}
				for (int j = 2; j < 20; j++)
				{
					if (cVInspection.paramFile.VSLocateParam[j].ParetoIndex == switch1)
					{
						cVInspection.paramFile.VSLocateParam[j].ParetoIndex = value;
						cVInspection.paramFile.VSLocateParam[j].ValueChangeFlag = true;
					}
				}
				for (int k = 2; k < 20; k++)
				{
					if (cVInspection.paramFile.HSLocateParam[k].ParetoIndex == switch1)
					{
						cVInspection.paramFile.HSLocateParam[k].ParetoIndex = value;
						cVInspection.paramFile.HSLocateParam[k].ValueChangeFlag = true;
					}
				}
				for (int l = 1; l < 20; l++)
				{
					if (cVInspection.paramFile.AdvLocateParam[l].ParetoIndex == switch1)
					{
						cVInspection.paramFile.AdvLocateParam[l].ParetoIndex = value;
						cVInspection.paramFile.AdvLocateParam[l].ValueChangeFlag = true;
					}
				}
				for (int m = value; m < 40; m++)
				{
					if (cVInspection.paramFile.AdvLocateMParam[m].ParetoIndex == switch1)
					{
						cVInspection.paramFile.AdvLocateMParam[m].ParetoIndex = value;
						cVInspection.paramFile.AdvLocateMParam[m].ValueChangeFlag = true;
					}
				}
				for (int n = value; n < 20; n++)
				{
					if (!cVInspection.paramFile.MeasureToolON)
					{
						break;
					}
					if (cVInspection.paramFile.MeaWidthParam[n].ParetoIndex == switch1)
					{
						cVInspection.paramFile.MeaWidthParam[n].ParetoIndex = value;
						cVInspection.paramFile.MeaWidthParam[n].ValueChangeFlag = true;
					}
				}
				for (int num = value; num < 20; num++)
				{
					if (!cVInspection.paramFile.MeasureToolON)
					{
						break;
					}
					if (cVInspection.paramFile.MeaHeightParam[num].ParetoIndex == switch1)
					{
						cVInspection.paramFile.MeaHeightParam[num].ParetoIndex = value;
						cVInspection.paramFile.MeaHeightParam[num].ValueChangeFlag = true;
					}
				}
				for (int num2 = value; num2 < 30; num2++)
				{
					if (!cVInspection.paramFile.BinaryToolON)
					{
						break;
					}
					if (cVInspection.paramFile.BinaryParam[num2].ParetoIndex == switch1)
					{
						cVInspection.paramFile.BinaryParam[num2].ParetoIndex = value;
						cVInspection.paramFile.BinaryParam[num2].ValueChangeFlag = true;
					}
				}
				for (int num3 = value; num3 < 20; num3++)
				{
					if (!cVInspection.paramFile.SmoothToolON)
					{
						break;
					}
					if (cVInspection.paramFile.SmoothParam[num3].ParetoIndex == switch1)
					{
						cVInspection.paramFile.SmoothParam[num3].ParetoIndex = value;
						cVInspection.paramFile.SmoothParam[num3].ValueChangeFlag = true;
					}
				}
				for (int num4 = value; num4 < 20; num4++)
				{
					if (!cVInspection.paramFile.BlockAvgToolON)
					{
						break;
					}
					if (cVInspection.paramFile.BlockAvgParam[num4].ParetoIndex == switch1)
					{
						cVInspection.paramFile.BlockAvgParam[num4].ParetoIndex = value;
						cVInspection.paramFile.BlockAvgParam[num4].ValueChangeFlag = true;
					}
				}
				for (int num5 = value; num5 < 20; num5++)
				{
					if (!cVInspection.paramFile.DifferenceToolON)
					{
						break;
					}
					if (cVInspection.paramFile.DifferenceParam[num5].ParetoIndex == switch1)
					{
						cVInspection.paramFile.DifferenceParam[num5].ParetoIndex = value;
						cVInspection.paramFile.DifferenceParam[num5].ValueChangeFlag = true;
					}
				}
				for (int num6 = value; num6 < 20; num6++)
				{
					if (!cVInspection.paramFile.LineAnalysisToolON)
					{
						break;
					}
					if (cVInspection.paramFile.LineAnalysisParam[num6].ParetoIndex == switch1)
					{
						cVInspection.paramFile.LineAnalysisParam[num6].ParetoIndex = value;
						cVInspection.paramFile.LineAnalysisParam[num6].ValueChangeFlag = true;
					}
				}
				for (int num7 = value; num7 < 20; num7++)
				{
					if (!cVInspection.paramFile.MaskToolON)
					{
						break;
					}
					if (cVInspection.paramFile.MatchParam[num7].ParetoIndex == switch1)
					{
						cVInspection.paramFile.MatchParam[num7].ParetoIndex = value;
						cVInspection.paramFile.MatchParam[num7].ValueChangeFlag = true;
					}
				}
			}
		}
		catch (Exception ex)
		{
			string text = "PCW06-ParetoChartWindow-UpdateParetoIndex()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Chk_EnableMultiPareto_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			bool value = chk_EnableMultiPareto.IsChecked.Value;
			ThroughputFile.SetEnableMultiParetoMode(value);
			for (int i = 0; i < 8; i++)
			{
				Station[i].EnableMultiParetoMode = value;
			}
		}
		catch (Exception ex)
		{
			string text = "PCW03-ParetoChartWindow-Chk_EnableMultiPareto_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void Chk_EnableThroughputWindow_Click(object sender, RoutedEventArgs e)
	{
		bool value = chk_EnableThroughputWindow.IsChecked.Value;
		ThroughputFile.SetEnableThroughputWindow(value);
		ThroughputFile.MainValueChanged = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/paretochartwindow.xaml", UriKind.Relative);
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
			ParetoMain = (ParetoChartWindow)target;
			ParetoMain.Closing += ParetoMain_Closing;
			break;
		case 2:
			tab_ParetoFun = (MetroTabControl)target;
			tab_ParetoFun.SelectionChanged += Tab_ParetoFun_SelectionChanged;
			break;
		case 3:
			tab_Chart = (TabItem)target;
			break;
		case 4:
			lv_ParetoStatistics = (ListView)target;
			break;
		case 5:
			btn_QuickSetupSave = (Button)target;
			btn_QuickSetupSave.Click += Btn_QuickSetupSave_Click;
			break;
		case 6:
			frame = (Frame)target;
			break;
		case 7:
			btn_ShowChartS1 = (Button)target;
			btn_ShowChartS1.Click += Btn_ShowChartS1_Click;
			break;
		case 8:
			btn_ShowChartS2 = (Button)target;
			btn_ShowChartS2.Click += Btn_ShowChartS1_Click;
			break;
		case 9:
			btn_ShowChartS3 = (Button)target;
			btn_ShowChartS3.Click += Btn_ShowChartS1_Click;
			break;
		case 10:
			btn_ShowChartS4 = (Button)target;
			btn_ShowChartS4.Click += Btn_ShowChartS1_Click;
			break;
		case 11:
			btn_ShowChartS5 = (Button)target;
			btn_ShowChartS5.Click += Btn_ShowChartS1_Click;
			break;
		case 12:
			btn_ShowChartS6 = (Button)target;
			btn_ShowChartS6.Click += Btn_ShowChartS1_Click;
			break;
		case 13:
			btn_ShowChartS7 = (Button)target;
			btn_ShowChartS7.Click += Btn_ShowChartS1_Click;
			break;
		case 14:
			btn_ShowChartS8 = (Button)target;
			btn_ShowChartS8.Click += Btn_ShowChartS1_Click;
			break;
		case 15:
			tab_Setup = (TabItem)target;
			break;
		case 16:
			lv_ParetoList = (ListView)target;
			lv_ParetoList.MouseDoubleClick += Lv_ParetoList_MouseDoubleClick;
			break;
		case 17:
			btn_ItemAdd = (Button)target;
			btn_ItemAdd.Click += Btn_ItemAdd_Click;
			break;
		case 18:
			btn_ItemUp = (Button)target;
			btn_ItemUp.Click += Btn_ItemUp_Click;
			break;
		case 19:
			btn_ItemDowm = (Button)target;
			btn_ItemDowm.Click += Btn_ItemDowm_Click;
			break;
		case 20:
			btn_ItemDelete = (Button)target;
			btn_ItemDelete.Click += Btn_ItemDelete_Click;
			break;
		case 21:
			btn_ItemEdit = (Button)target;
			btn_ItemEdit.Click += Btn_ItemEdit_Click;
			break;
		case 22:
			chk_EnableMultiPareto = (CheckBox)target;
			chk_EnableMultiPareto.Click += Chk_EnableMultiPareto_Click;
			break;
		case 23:
			chk_EnableThroughputWindow = (CheckBox)target;
			chk_EnableThroughputWindow.Click += Chk_EnableThroughputWindow_Click;
			break;
		case 24:
			chk_EnableReverseBin = (CheckBox)target;
			break;
		case 25:
			tab_ItemPreview = (TabItem)target;
			break;
		case 26:
			lv_ParetoListPreview = (ListView)target;
			lv_ParetoListPreview.MouseDoubleClick += Lv_ParetoList_MouseDoubleClick;
			break;
		case 27:
			cb_ParetoItem = (ComboBox)target;
			cb_ParetoItem.SelectionChanged += Cb_Station_SelectionChanged;
			break;
		case 28:
			cb_Station = (ComboBox)target;
			cb_Station.SelectionChanged += Cb_Station_SelectionChanged;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
