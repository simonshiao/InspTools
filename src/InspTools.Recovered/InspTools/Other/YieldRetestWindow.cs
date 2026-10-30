using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using OpenCvSharp;

namespace InspTools.Other;

public class YieldRetestWindow : System.Windows.Window, IComponentConnector
{
	private CVInspection[] Station;

	private const int MaxStation = 8;

	private const int MaxBin = 6;

	public string YieldRetestPath = "";

	private bool[] StationEnable = new bool[8];

	private Mat[] StationMat = new Mat[8];

	private string[][] filelist = new string[8][];

	private int[] ImageWidth = new int[8];

	private int[] ImageHeight = new int[8];

	private int[] ImageBand = new int[8];

	private int TotalTestImage;

	private int EnableStationNum;

	private int LanguageType = 1;

	private bool UseRecentImg = true;

	private bool UpdateUI;

	private double OK_Ratio;

	private double Param1Ratio;

	private double Param2Ratio;

	private double Param3Ratio;

	private bool InspectCancel;

	private BIN_COUNT[] BinCount = new BIN_COUNT[8];

	private BIN_COUNT BinResult = new BIN_COUNT();

	private BIN_COUNT[] OldBinCount = new BIN_COUNT[8];

	private BIN_COUNT TotalResult = new BIN_COUNT();

	private BIN_COUNT OldTotalResult = new BIN_COUNT();

	private DispatcherTimer TestTimer = new DispatcherTimer();

	private Label[] BSOKLabel;

	private Label[] BSOK2Label;

	private Label[] BSNG1Label;

	private Label[] BSNG2Label;

	private Label[] BSNG3Label;

	private Label[] BSNG4Label;

	private Label[] BSRetestLabel;

	private Label[] ASOKLabel;

	private Label[] ASOK2Label;

	private Label[] ASNG1Label;

	private Label[] ASNG2Label;

	private Label[] ASNG3Label;

	private Label[] ASNG4Label;

	private Label[] ASRetestLabel;

	private Grid[] ASGrid;

	private Grid[] BSGrid;

	private CheckBox[] CBStation;

	internal YieldRetestWindow wnd_main;

	internal Grid grd_Main;

	internal Rectangle rt_RecentImage;

	internal Rectangle rt_LocalImage;

	internal Label lab_LocalImage;

	internal Label lab_RecentImage;

	internal TabControl tab_Main;

	internal TabItem tab_MainYield;

	internal Label lab_TotalOKB;

	internal Label lab_TotalOKRatioB;

	internal Label lab_TotalNG1B;

	internal Label lab_TotalNG1RatioB;

	internal Label lab_TotalNG2B;

	internal Label lab_TotalNG2RatioB;

	internal Label lab_TotalNG3B;

	internal Label lab_TotalNG3RatioB;

	internal Label lab_TotalNG4B;

	internal Label lab_TotalNG4RatioB;

	internal Label lab_TotalRetestB;

	internal Label lab_TotalRetestRatioB;

	internal Label lab_TotalOK2RatioB;

	internal Label lab_TotalOKA;

	internal Label lab_TotalOKRatioA;

	internal Label lab_TotalNG1A;

	internal Label lab_TotalNG1RatioA;

	internal Label lab_TotalNG2A;

	internal Label lab_TotalNG2RatioA;

	internal Label lab_TotalNG3A;

	internal Label lab_TotalNG3RatioA;

	internal Label lab_TotalNG4A;

	internal Label lab_TotalNG4RatioA;

	internal Label lab_TotalRetestA;

	internal Label lab_TotalRetestRatioA;

	internal Label lab_TotalOK2RatioA;

	internal Button btn_CopyTo;

	internal TabItem tab_StationYield;

	internal Grid grd_bs1;

	internal Label lab_ok_bs1;

	internal Label lab_ng1_bs1;

	internal Label lab_ng2_bs1;

	internal Label lab_ng3_bs1;

	internal Label lab_ng4_bs1;

	internal Label lab_retest_bs1;

	internal Label lab_ok2_bs1;

	internal Grid grd_bs2;

	internal Label lab_ok_bs2;

	internal Label lab_ng1_bs2;

	internal Label lab_ng2_bs2;

	internal Label lab_ng3_bs2;

	internal Label lab_ng4_bs2;

	internal Label lab_retest_bs2;

	internal Label lab_ok2_bs2;

	internal Grid grd_bs3;

	internal Label lab_ok_bs3;

	internal Label lab_ng1_bs3;

	internal Label lab_ng2_bs3;

	internal Label lab_ng3_bs3;

	internal Label lab_ng4_bs3;

	internal Label lab_retest_bs3;

	internal Label lab_ok2_bs3;

	internal Grid grd_bs4;

	internal Label lab_ok_bs4;

	internal Label lab_ng1_bs4;

	internal Label lab_ng2_bs4;

	internal Label lab_ng3_bs4;

	internal Label lab_ng4_bs4;

	internal Label lab_retest_bs4;

	internal Label lab_ok2_bs4;

	internal Grid grd_bs5;

	internal Label lab_ok_bs5;

	internal Label lab_ng1_bs5;

	internal Label lab_ng2_bs5;

	internal Label lab_ng3_bs5;

	internal Label lab_ng4_bs5;

	internal Label lab_retest_bs5;

	internal Label lab_ok2_bs5;

	internal Grid grd_bs6;

	internal Label lab_ok_bs6;

	internal Label lab_ng1_bs6;

	internal Label lab_ng2_bs6;

	internal Label lab_ng3_bs6;

	internal Label lab_ng4_bs6;

	internal Label lab_retest_bs6;

	internal Label lab_ok2_bs6;

	internal Grid grd_bs7;

	internal Label lab_ok_bs7;

	internal Label lab_ng1_bs7;

	internal Label lab_ng2_bs7;

	internal Label lab_ng3_bs7;

	internal Label lab_ng4_bs7;

	internal Label lab_retest_bs7;

	internal Label lab_ok2_bs7;

	internal Grid grd_bs8;

	internal Label lab_ok_bs8;

	internal Label lab_ng1_bs8;

	internal Label lab_ng2_bs8;

	internal Label lab_ng3_bs8;

	internal Label lab_ng4_bs8;

	internal Label lab_retest_bs8;

	internal Label lab_ok2_bs8;

	internal Grid grd_as1;

	internal Label lab_ok_as1;

	internal Label lab_ng1_as1;

	internal Label lab_ng2_as1;

	internal Label lab_ng3_as1;

	internal Label lab_ng4_as1;

	internal Label lab_retest_as1;

	internal Label lab_ok2_as1;

	internal Grid grd_as2;

	internal Label lab_ok_as2;

	internal Label lab_ng1_as2;

	internal Label lab_ng2_as2;

	internal Label lab_ng3_as2;

	internal Label lab_ng4_as2;

	internal Label lab_retest_as2;

	internal Label lab_ok2_as2;

	internal Grid grd_as3;

	internal Label lab_ok_as3;

	internal Label lab_ng1_as3;

	internal Label lab_ng2_as3;

	internal Label lab_ng3_as3;

	internal Label lab_ng4_as3;

	internal Label lab_retest_as3;

	internal Label lab_ok2_as3;

	internal Grid grd_as4;

	internal Label lab_ok_as4;

	internal Label lab_ng1_as4;

	internal Label lab_ng2_as4;

	internal Label lab_ng3_as4;

	internal Label lab_ng4_as4;

	internal Label lab_retest_as4;

	internal Label lab_ok2_as4;

	internal Grid grd_as5;

	internal Label lab_ok_as5;

	internal Label lab_ng1_as5;

	internal Label lab_ng2_as5;

	internal Label lab_ng3_as5;

	internal Label lab_ng4_as5;

	internal Label lab_retest_as5;

	internal Label lab_ok2_as5;

	internal Grid grd_as6;

	internal Label lab_ok_as6;

	internal Label lab_ng1_as6;

	internal Label lab_ng2_as6;

	internal Label lab_ng3_as6;

	internal Label lab_ng4_as6;

	internal Label lab_retest_as6;

	internal Label lab_ok2_as6;

	internal Grid grd_as7;

	internal Label lab_ok_as7;

	internal Label lab_ng1_as7;

	internal Label lab_ng2_as7;

	internal Label lab_ng3_as7;

	internal Label lab_ng4_as7;

	internal Label lab_retest_as7;

	internal Label lab_ok2_as7;

	internal Grid grd_as8;

	internal Label lab_ok_as8;

	internal Label lab_ng1_as8;

	internal Label lab_ng2_as8;

	internal Label lab_ng3_as8;

	internal Label lab_ng4_as8;

	internal Label lab_retest_as8;

	internal Label lab_ok2_as8;

	internal TabItem tab_StationNGCount;

	internal TextBox lab_path;

	internal Button btn_SetPath;

	internal Button btn_Test;

	internal TextBox lab_Total;

	internal DockPanel dp_Station;

	internal CheckBox cb_st1;

	internal CheckBox cb_st2;

	internal CheckBox cb_st3;

	internal CheckBox cb_st4;

	internal CheckBox cb_st5;

	internal CheckBox cb_st6;

	internal CheckBox cb_st7;

	internal CheckBox cb_st8;

	internal Grid grd_Loading;

	internal Label lab_Loading;

	internal ProgressBar pb_TestProgress;

	internal Button btn_cancel;

	internal Grid grd_Temp1;

	internal Button btn_SaveParam1;

	internal Button btn_LoadParam1;

	internal Label lab_ParamRatio1;

	internal Grid grd_Temp2;

	internal Button btn_SaveParam2;

	internal Button btn_LoadParam2;

	internal Label lab_ParamRatio2;

	internal Grid grd_Temp3;

	internal Button btn_SaveParam3;

	internal Button btn_LoadParam3;

	internal Label lab_ParamRatio3;

	internal RadioButton rad_RecentImage;

	internal RadioButton rad_Localmage;

	internal Label lab_RecentImage2;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public YieldRetestWindow()
	{
		InitializeComponent();
		CBStation = new CheckBox[8] { cb_st1, cb_st2, cb_st3, cb_st4, cb_st5, cb_st6, cb_st7, cb_st8 };
		ASOKLabel = new Label[8] { lab_ok_as1, lab_ok_as2, lab_ok_as3, lab_ok_as4, lab_ok_as5, lab_ok_as6, lab_ok_as7, lab_ok_as8 };
		ASOK2Label = new Label[8] { lab_ok2_as1, lab_ok2_as2, lab_ok2_as3, lab_ok2_as4, lab_ok2_as5, lab_ok2_as6, lab_ok2_as7, lab_ok2_as8 };
		ASNG1Label = new Label[8] { lab_ng1_as1, lab_ng1_as2, lab_ng1_as3, lab_ng1_as4, lab_ng1_as5, lab_ng1_as6, lab_ng1_as7, lab_ng1_as8 };
		ASNG2Label = new Label[8] { lab_ng2_as2, lab_ng2_as2, lab_ng2_as3, lab_ng2_as4, lab_ng2_as5, lab_ng2_as6, lab_ng2_as7, lab_ng2_as8 };
		ASNG3Label = new Label[8] { lab_ng3_as3, lab_ng3_as2, lab_ng3_as3, lab_ng3_as4, lab_ng3_as5, lab_ng3_as6, lab_ng3_as7, lab_ng3_as8 };
		ASNG4Label = new Label[8] { lab_ng4_as4, lab_ng4_as2, lab_ng4_as3, lab_ng4_as4, lab_ng4_as5, lab_ng4_as6, lab_ng4_as7, lab_ng4_as8 };
		ASRetestLabel = new Label[8] { lab_retest_as1, lab_retest_as2, lab_retest_as3, lab_retest_as4, lab_retest_as5, lab_retest_as6, lab_retest_as7, lab_retest_as8 };
		ASGrid = new Grid[8] { grd_as1, grd_as2, grd_as3, grd_as4, grd_as5, grd_as6, grd_as7, grd_as8 };
		BSOKLabel = new Label[8] { lab_ok_bs1, lab_ok_bs2, lab_ok_bs3, lab_ok_bs4, lab_ok_bs5, lab_ok_bs6, lab_ok_bs7, lab_ok_bs8 };
		BSOK2Label = new Label[8] { lab_ok2_bs1, lab_ok2_bs2, lab_ok2_bs3, lab_ok2_bs4, lab_ok2_bs5, lab_ok2_bs6, lab_ok2_bs7, lab_ok2_bs8 };
		BSNG1Label = new Label[8] { lab_ng1_bs1, lab_ng1_bs2, lab_ng1_bs3, lab_ng1_bs4, lab_ng1_bs5, lab_ng1_bs6, lab_ng1_bs7, lab_ng1_bs8 };
		BSNG2Label = new Label[8] { lab_ng2_bs1, lab_ng2_bs2, lab_ng2_bs3, lab_ng2_bs4, lab_ng2_bs5, lab_ng2_bs6, lab_ng2_bs7, lab_ng2_bs8 };
		BSNG3Label = new Label[8] { lab_ng3_bs1, lab_ng3_bs2, lab_ng3_bs3, lab_ng3_bs4, lab_ng3_bs5, lab_ng3_bs6, lab_ng3_bs7, lab_ng3_bs8 };
		BSNG4Label = new Label[8] { lab_ng4_bs1, lab_ng4_bs2, lab_ng4_bs3, lab_ng4_bs4, lab_ng4_bs5, lab_ng4_bs6, lab_ng4_bs7, lab_ng4_bs8 };
		BSRetestLabel = new Label[8] { lab_retest_bs1, lab_retest_bs2, lab_retest_bs3, lab_retest_bs4, lab_retest_bs5, lab_retest_bs6, lab_retest_bs7, lab_retest_bs8 };
		BSGrid = new Grid[8] { grd_bs1, grd_bs2, grd_bs3, grd_bs4, grd_bs5, grd_bs6, grd_bs7, grd_bs8 };
		ResetTable(t1: true, t2: true);
		btn_Test.IsEnabled = false;
		for (int i = 0; i < 8; i++)
		{
			BinCount[i] = new BIN_COUNT();
			OldBinCount[i] = new BIN_COUNT();
		}
		TestTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
		TestTimer.Tick += TestTimer_Tick;
		TestTimer.Stop();
		grd_Loading.Visibility = Visibility.Hidden;
		tab_StationNGCount.Visibility = Visibility.Hidden;
		lab_ParamRatio1.Content = (lab_ParamRatio2.Content = (lab_ParamRatio3.Content = ""));
		for (int j = 0; j < 8; j++)
		{
			ASOKLabel[j].Content = "OK:  0 %";
			ASOK2Label[j].Content = "0";
			ASNG1Label[j].Content = "NG1:  0 %";
			ASNG2Label[j].Content = "NG2:  0 %";
			ASNG3Label[j].Content = "NG3:  0 %";
			ASNG4Label[j].Content = "NG4:  0 %";
			ASRetestLabel[j].Content = "Retest:  0 %";
			BSOKLabel[j].Content = "OK:  0 %";
			BSOK2Label[j].Content = "0";
			BSNG1Label[j].Content = "NG1:  0 %";
			BSNG2Label[j].Content = "NG2:  0 %";
			BSNG3Label[j].Content = "NG3:  0 %";
			BSNG4Label[j].Content = "NG4:  0 %";
			BSRetestLabel[j].Content = "Retest:  0 %";
		}
		UpdateUI = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void TestTimer_Tick(object sender, EventArgs e)
	{
		TestTimer.Stop();
		try
		{
			inspect();
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
			{
				grd_Loading.Visibility = Visibility.Hidden;
				SetEnable(enable: true);
			});
		}
		catch (Exception ex)
		{
			grd_Loading.Visibility = Visibility.Hidden;
			string text = "PCW01-ParamCOpyWIndow-Btn_StationPara_Copy_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	public void SetInspection(CVInspection[] station, int[] width, int[] height, int[] band, string yieldRetestPath)
	{
		Station = station;
		YieldRetestPath = yieldRetestPath;
		for (int i = 0; i < 8; i++)
		{
			ImageWidth[i] = width[i];
			ImageHeight[i] = height[i];
			ImageBand[i] = band[i];
		}
		if (!CheckRecentBuffer(ShowError: false))
		{
			UpdateUI = false;
			rad_Localmage.IsChecked = true;
			UpdateUI = true;
			btn_SetPath.IsEnabled = true;
			UseRecentImg = false;
			CheckRetestPath();
			rt_RecentImage.Fill = Brushes.Transparent;
			rt_LocalImage.Fill = Brushes.White;
		}
		else
		{
			UpdateUI = false;
			rad_RecentImage.IsChecked = true;
			UpdateUI = true;
			btn_SetPath.IsEnabled = false;
		}
	}

	public void SetOldBinCount(int[,] oldbincount, int[] oldTotalResult)
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				OldBinCount[i].Count[j] = oldbincount[i, j];
				if (i == 0)
				{
					OldTotalResult.Count[j] = oldTotalResult[j];
				}
			}
		}
		UpdateBeforeResult();
	}

	public void GetOldBinCount(ref int[,] oldbincount, ref int[] oldTotalResult)
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				oldbincount[i, j] = OldBinCount[i].Count[j];
			}
		}
		for (int k = 0; k < 6; k++)
		{
			oldTotalResult[k] = OldTotalResult.Count[k];
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

	private void CopyToBinResult()
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < BinCount[i].Count.Length; j++)
			{
				OldBinCount[i].Count[j] = BinCount[i].Count[j];
			}
		}
		for (int k = 0; k < 6; k++)
		{
			OldTotalResult.Count[k] = TotalResult.Count[k];
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ResetStationYield()
	{
		for (int i = 0; i < 8; i++)
		{
			if (!StationEnable[i])
			{
				ASGrid[i].Visibility = Visibility.Hidden;
				BSGrid[i].Visibility = Visibility.Hidden;
			}
			else
			{
				ASGrid[i].Visibility = Visibility.Visible;
				BSGrid[i].Visibility = Visibility.Visible;
			}
			ASOKLabel[i].Content = "OK:  0 %";
			ASOK2Label[i].Content = "0";
			ASNG1Label[i].Content = "NG1:  0 %";
			ASNG2Label[i].Content = "NG2:  0 %";
			ASNG3Label[i].Content = "NG3:  0 %";
			ASNG4Label[i].Content = "NG4:  0 %";
			ASRetestLabel[i].Content = "Retest:  0 %";
			BSOKLabel[i].Content = "OK:  0 %";
			BSOK2Label[i].Content = "0";
			BSNG1Label[i].Content = "NG1:  0 %";
			BSNG2Label[i].Content = "NG2:  0 %";
			BSNG3Label[i].Content = "NG3:  0 %";
			BSNG4Label[i].Content = "NG4:  0 %";
			BSRetestLabel[i].Content = "Retest:  0 %";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ResetTable(bool t1, bool t2)
	{
		if (t1)
		{
			lab_TotalOKA.Content = "0";
			lab_TotalOKB.Content = "0";
			lab_TotalNG1A.Content = "0";
			lab_TotalNG1B.Content = "0";
			lab_TotalNG2A.Content = "0";
			lab_TotalNG2B.Content = "0";
			lab_TotalNG3A.Content = "0";
			lab_TotalNG3B.Content = "0";
			lab_TotalNG4A.Content = "0";
			lab_TotalNG4B.Content = "0";
			lab_TotalRetestA.Content = "0";
			lab_TotalRetestB.Content = "0";
		}
		if (t2)
		{
			lab_TotalOKRatioA.Content = "0.00";
			lab_TotalOK2RatioA.Content = "0.00";
			lab_TotalOKRatioB.Content = "0.00";
			lab_TotalOK2RatioB.Content = "0.00";
			lab_TotalNG1RatioA.Content = "0.00";
			lab_TotalNG1RatioB.Content = "0.00";
			lab_TotalNG2RatioA.Content = "0.00";
			lab_TotalNG2RatioB.Content = "0.00";
			lab_TotalNG3RatioA.Content = "0.00";
			lab_TotalNG3RatioB.Content = "0.00";
			lab_TotalNG4RatioA.Content = "0.00";
			lab_TotalNG4RatioB.Content = "0.00";
			lab_TotalRetestRatioA.Content = "0.00";
			lab_TotalRetestRatioB.Content = "0.00";
		}
	}

	private void btn_SetPath_Click(object sender, RoutedEventArgs e)
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (folderSelectDialog.ShowDialog() && Directory.Exists(folderSelectDialog.FileName))
		{
			YieldRetestPath = folderSelectDialog.FileName;
			lab_path.Text = YieldRetestPath;
			GetPathImage(YieldRetestPath);
		}
	}

	private void GetRecentImage()
	{
		EnableStationNum = 0;
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].RecentBufferList != null)
			{
				if (Station[i].RecentBufferList.Count > 0)
				{
					btn_Test.IsEnabled = true;
					EnableStationNum++;
					StationEnable[i] = true;
					CBStation[i].IsEnabled = true;
					CBStation[i].IsChecked = true;
				}
				else
				{
					StationEnable[i] = false;
					CBStation[i].IsEnabled = false;
					CBStation[i].IsChecked = false;
				}
			}
			else
			{
				StationEnable[i] = false;
				CBStation[i].IsEnabled = false;
				CBStation[i].IsChecked = false;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetPathImage(string path)
	{
		try
		{
			TotalTestImage = 0;
			EnableStationNum = 0;
			for (int i = 0; i < 8; i++)
			{
				string path2 = string.Format("{0}\\{1}", path, i + 1);
				if (Directory.Exists(path2))
				{
					filelist[i] = Directory.GetFiles(path2, "*.png", SearchOption.AllDirectories);
					if (filelist[i].Length != 0)
					{
						btn_Test.IsEnabled = true;
						EnableStationNum++;
						StationEnable[i] = true;
						CBStation[i].IsEnabled = true;
						CBStation[i].IsChecked = true;
						if (filelist[i].Length > TotalTestImage)
						{
							TotalTestImage = filelist[i].Length;
						}
					}
					else
					{
						StationEnable[i] = false;
						CBStation[i].IsEnabled = false;
						CBStation[i].IsChecked = false;
					}
				}
				else
				{
					StationEnable[i] = false;
					CBStation[i].IsEnabled = false;
					CBStation[i].IsChecked = false;
				}
			}
			lab_Total.Text = TotalTestImage.ToString();
			ResetStationYield();
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("YieldRetestWindow-btn_LogAlert_Click()" + ex.ToString());
		}
	}

	private bool FindStationImageExist(int fileid, ref int[] imageindex)
	{
		int num = 0;
		_ = new int[8];
		for (int i = 0; i < 8; i++)
		{
			imageindex[i] = -1;
			if (!StationEnable[i])
			{
				continue;
			}
			for (int j = 0; j < Station[i].RecentBufferIDList.Count; j++)
			{
				if (Station[i].RecentBufferIDList[j] == fileid)
				{
					num++;
					imageindex[i] = j;
					break;
				}
			}
		}
		if (num != EnableStationNum)
		{
			return false;
		}
		return true;
	}

	private bool FindStationImageExist(string filename, ref int[] imageindex)
	{
		int num = 0;
		_ = new int[8];
		for (int i = 0; i < 8; i++)
		{
			imageindex[i] = -1;
			if (!StationEnable[i])
			{
				continue;
			}
			for (int j = 0; j < filelist[i].Length; j++)
			{
				if (System.IO.Path.GetFileName(filelist[i][j]).ToLower() == filename)
				{
					num++;
					imageindex[i] = j;
					break;
				}
			}
		}
		if (num != EnableStationNum)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void inspect()
	{
		int mainid = -1;
		EnableStationNum = 0;
		try
		{
			string teststr = LanguageManager.Instance["Testing"];
			TotalResult.Reset();
			for (int i = 0; i < 8; i++)
			{
				StationEnable[i] = CBStation[i].IsChecked.Value;
				if (StationEnable[i])
				{
					EnableStationNum++;
					if (mainid == -1)
					{
						mainid = i;
					}
				}
				BinCount[i].Reset();
			}
			if (mainid == -1)
			{
				return;
			}
			string text = "";
			int num = 0;
			TotalResult.Reset();
			int[] imageindex = new int[8];
			if (UseRecentImg)
			{
				int f;
				for (f = 0; f < Station[mainid].RecentBufferList.Count; f++)
				{
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
					{
						lab_Loading.Content = string.Format("{0}...{1:N1} %", teststr, (double)(f + 1) / (double)Station[mainid].RecentBufferList.Count * 100.0);
						pb_TestProgress.Value = (double)(f + 1) / (double)Station[mainid].RecentBufferList.Count * 100.0;
					}));
					num = Station[mainid].RecentBufferIDList[f];
					if (!FindStationImageExist(num, ref imageindex))
					{
						continue;
					}
					BinResult.Reset();
					bool flag = true;
					Parallel.For(0, 8, delegate(int num7, ParallelLoopState loopState)
					{
						if (StationEnable[num7] && imageindex[num7] != -1)
						{
							Station[num7].SetBuffers(Station[num7].RecentBufferList[f], ImageWidth[num7], ImageHeight[num7], ImageBand[num7]);
							StationMat[num7] = Station[num7].buffers.Src.Clone();
							INSPRESULT iNSPRESULT = Station[num7].Inspect(ref StationMat[num7]);
							BinCount[num7].Count[(int)iNSPRESULT.bin]++;
							BinResult.Count[(int)iNSPRESULT.bin]++;
						}
					});
					if (BinResult.Count[0] != 0 || !flag)
					{
						TotalResult.Count[0]++;
					}
					else if (BinResult.Count[2] != 0)
					{
						TotalResult.Count[2]++;
					}
					else if (BinResult.Count[3] != 0)
					{
						TotalResult.Count[3]++;
					}
					else if (BinResult.Count[4] != 0)
					{
						TotalResult.Count[4]++;
					}
					else if (BinResult.Count[5] != 0)
					{
						TotalResult.Count[5]++;
					}
					else
					{
						TotalResult.Count[1]++;
					}
					if (InspectCancel)
					{
						break;
					}
				}
			}
			else
			{
				for (int f2 = 0; f2 < filelist[mainid].Length; f2++)
				{
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
					{
						lab_Loading.Content = string.Format("測試中...{0:N1} %", (double)(f2 + 1) / (double)filelist[mainid].Length * 100.0);
						pb_TestProgress.Value = (double)(f2 + 1) / (double)filelist[mainid].Length * 100.0;
					}));
					text = System.IO.Path.GetFileName(filelist[mainid][f2]).ToLower();
					if (!FindStationImageExist(text, ref imageindex))
					{
						continue;
					}
					BinResult.Reset();
					bool founded = true;
					Parallel.For(0, 8, delegate(int num7, ParallelLoopState loopState)
					{
						if (StationEnable[num7] && imageindex[num7] != -1)
						{
							int num8 = imageindex[num7];
							StationMat[num7] = CVBuffer.ImRead(filelist[num7][num8]);
							if (StationMat[num7].Width > 0 && StationMat[num7].Height > 0)
							{
								Station[num7].SetBuffers(StationMat[num7]);
								INSPRESULT iNSPRESULT = Station[num7].Inspect(ref StationMat[num7]);
								BinCount[num7].Count[(int)iNSPRESULT.bin]++;
								BinResult.Count[(int)iNSPRESULT.bin]++;
							}
							else
							{
								founded = false;
							}
						}
					});
					if (BinResult.Count[0] != 0 || !founded)
					{
						TotalResult.Count[0]++;
					}
					else if (BinResult.Count[2] != 0)
					{
						TotalResult.Count[2]++;
					}
					else if (BinResult.Count[3] != 0)
					{
						TotalResult.Count[3]++;
					}
					else if (BinResult.Count[4] != 0)
					{
						TotalResult.Count[4]++;
					}
					else if (BinResult.Count[5] != 0)
					{
						TotalResult.Count[5]++;
					}
					else
					{
						TotalResult.Count[1]++;
					}
					if (InspectCancel)
					{
						break;
					}
				}
			}
			int num2 = 0;
			for (int num3 = 0; num3 < 6; num3++)
			{
				num2 += TotalResult.Count[num3];
			}
			if (num2 > 0)
			{
				lab_TotalOKA.Content = TotalResult.Count[1];
				lab_TotalNG1A.Content = TotalResult.Count[2];
				lab_TotalNG2A.Content = TotalResult.Count[3];
				lab_TotalNG3A.Content = TotalResult.Count[4];
				lab_TotalNG4A.Content = TotalResult.Count[5];
				lab_TotalRetestA.Content = TotalResult.Count[0];
				OK_Ratio = (double)TotalResult.Count[1] / (double)num2 * 100.0;
				lab_TotalOKRatioA.Content = string.Format("{0:N2} %", (double)TotalResult.Count[1] / (double)num2 * 100.0);
				lab_TotalOK2RatioA.Content = string.Format("{0:N2}", (double)TotalResult.Count[1] / (double)num2 * 100.0);
				lab_TotalNG1RatioA.Content = string.Format("{0:N2} %", (double)TotalResult.Count[2] / (double)num2 * 100.0);
				lab_TotalNG2RatioA.Content = string.Format("{0:N2} %", (double)TotalResult.Count[3] / (double)num2 * 100.0);
				lab_TotalNG3RatioA.Content = string.Format("{0:N2} %", (double)TotalResult.Count[4] / (double)num2 * 100.0);
				lab_TotalNG4RatioA.Content = string.Format("{0:N2} %", (double)TotalResult.Count[5] / (double)num2 * 100.0);
				lab_Total.Text = num2.ToString();
			}
			int num4 = 0;
			for (int num5 = 0; num5 < 6; num5++)
			{
				num4 += OldTotalResult.Count[num5];
			}
			if (num4 > 0)
			{
				lab_TotalOKB.Content = OldTotalResult.Count[1];
				lab_TotalNG1B.Content = OldTotalResult.Count[2];
				lab_TotalNG2B.Content = OldTotalResult.Count[3];
				lab_TotalNG3B.Content = OldTotalResult.Count[4];
				lab_TotalNG4B.Content = OldTotalResult.Count[5];
				lab_TotalRetestB.Content = OldTotalResult.Count[0];
				lab_TotalOKRatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[1] / (double)num4 * 100.0);
				lab_TotalOK2RatioB.Content = string.Format("{0:N2}", (double)OldTotalResult.Count[1] / (double)num4 * 100.0);
				lab_TotalNG1RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[2] / (double)num4 * 100.0);
				lab_TotalNG2RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[3] / (double)num4 * 100.0);
				lab_TotalNG3RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[4] / (double)num4 * 100.0);
				lab_TotalNG4RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[5] / (double)num4 * 100.0);
				lab_TotalRetestRatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[0] / (double)num4 * 100.0);
			}
			for (int num6 = 0; num6 < 8; num6++)
			{
				if (num2 > 0)
				{
					ASOKLabel[num6].Content = string.Format("OK:  {0:N1} %", (double)BinCount[num6].Count[1] / (double)num2 * 100.0);
					ASOK2Label[num6].Content = string.Format("{0:N1}", (double)BinCount[num6].Count[1] / (double)num2 * 100.0);
					ASNG1Label[num6].Content = string.Format("NG1:  {0:N1} %", (double)BinCount[num6].Count[2] / (double)num2 * 100.0);
					ASNG2Label[num6].Content = string.Format("NG2:  {0:N1} %", (double)BinCount[num6].Count[3] / (double)num2 * 100.0);
					ASNG3Label[num6].Content = string.Format("NG3:  {0:N1} %", (double)BinCount[num6].Count[4] / (double)num2 * 100.0);
					ASNG4Label[num6].Content = string.Format("NG4:  {0:N1} %", (double)BinCount[num6].Count[5] / (double)num2 * 100.0);
					ASRetestLabel[num6].Content = string.Format("Retest:  {0:N1} %", (double)BinCount[num6].Count[0] / (double)num2 * 100.0);
				}
				if (num4 > 0)
				{
					BSOKLabel[num6].Content = string.Format("OK:  {0:N1} %", (double)OldBinCount[num6].Count[1] / (double)num4 * 100.0);
					BSOK2Label[num6].Content = string.Format("{0:N1}", (double)OldBinCount[num6].Count[1] / (double)num4 * 100.0);
					BSNG1Label[num6].Content = string.Format("NG1:  {0:N1} %", (double)OldBinCount[num6].Count[2] / (double)num4 * 100.0);
					BSNG2Label[num6].Content = string.Format("NG2:  {0:N1} %", (double)OldBinCount[num6].Count[3] / (double)num4 * 100.0);
					BSNG3Label[num6].Content = string.Format("NG3:  {0:N1} %", (double)OldBinCount[num6].Count[4] / (double)num4 * 100.0);
					BSNG4Label[num6].Content = string.Format("NG4:  {0:N1} %", (double)OldBinCount[num6].Count[5] / (double)num4 * 100.0);
					BSRetestLabel[num6].Content = string.Format("Retest:  {0:N1} %", (double)OldBinCount[num6].Count[0] / (double)num4 * 100.0);
				}
			}
			InspectCancel = false;
			lab_Loading.Content = string.Format("{0}...", teststr);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("YieldRetestWindow-Inspect()" + ex.ToString());
		}
	}

	private void btn_Test_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
		{
			InspectCancel = false;
			pb_TestProgress.Value = 0.0;
			grd_Loading.Visibility = Visibility.Visible;
			SetEnable(enable: false);
			btn_SaveParam1.IsEnabled = true;
			btn_SaveParam2.IsEnabled = true;
			btn_SaveParam3.IsEnabled = true;
		});
		TestTimer.Start();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.Source is TabControl && e.AddedItems[0] is TabItem tabItem)
		{
			if (tabItem.Name == "tab_MainYield")
			{
				tab_MainYield.Background = Brushes.White;
				tab_StationYield.Background = Brushes.Transparent;
				tab_StationNGCount.Background = Brushes.Transparent;
			}
			else if (tabItem.Name == "tab_StationYield")
			{
				tab_MainYield.Background = Brushes.Transparent;
				tab_StationYield.Background = Brushes.White;
				tab_StationNGCount.Background = Brushes.Transparent;
			}
			else
			{
				tab_MainYield.Background = Brushes.Transparent;
				tab_StationYield.Background = Brushes.Transparent;
				tab_StationNGCount.Background = Brushes.White;
			}
		}
	}

	private void btn_CopyTo_Click(object sender, RoutedEventArgs e)
	{
		CopyToBinResult();
		UpdateBeforeResult();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateBeforeResult()
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			num += OldTotalResult.Count[i];
		}
		if (num == 0)
		{
			lab_TotalOKB.Content = "0";
			lab_TotalNG1B.Content = "0";
			lab_TotalNG2B.Content = "0";
			lab_TotalNG3B.Content = "0";
			lab_TotalNG4B.Content = "0";
			lab_TotalRetestB.Content = "0";
			lab_TotalOKRatioB.Content = "0.00";
			lab_TotalNG1RatioB.Content = "0.00";
			lab_TotalNG2RatioB.Content = "0.00";
			lab_TotalNG3RatioB.Content = "0.00";
			lab_TotalNG4RatioB.Content = "0.00";
			lab_TotalRetestRatioB.Content = "0.00";
		}
		else if (num > 0)
		{
			lab_TotalOKB.Content = OldTotalResult.Count[1];
			lab_TotalNG1B.Content = OldTotalResult.Count[2];
			lab_TotalNG2B.Content = OldTotalResult.Count[3];
			lab_TotalNG3B.Content = OldTotalResult.Count[4];
			lab_TotalNG4B.Content = OldTotalResult.Count[5];
			lab_TotalRetestB.Content = OldTotalResult.Count[0];
			lab_TotalOKRatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[1] / (double)num * 100.0);
			lab_TotalOK2RatioB.Content = string.Format("{0:N2}", (double)OldTotalResult.Count[1] / (double)num * 100.0);
			lab_TotalNG1RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[2] / (double)num * 100.0);
			lab_TotalNG2RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[3] / (double)num * 100.0);
			lab_TotalNG3RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[4] / (double)num * 100.0);
			lab_TotalNG4RatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[5] / (double)num * 100.0);
			lab_Total.Text = num.ToString();
			lab_TotalRetestRatioB.Content = string.Format("{0:N2} %", (double)OldTotalResult.Count[0] / (double)num * 100.0);
			for (int j = 0; j < 8; j++)
			{
				BSOKLabel[j].Content = string.Format("OK:  {0:N1} %", (double)OldBinCount[j].Count[1] / (double)num * 100.0);
				BSOK2Label[j].Content = string.Format("{0:N1}", (double)OldBinCount[j].Count[1] / (double)num * 100.0);
				BSNG1Label[j].Content = string.Format("NG1:  {0:N1} %", (double)OldBinCount[j].Count[2] / (double)num * 100.0);
				BSNG2Label[j].Content = string.Format("NG2:  {0:N1} %", (double)OldBinCount[j].Count[3] / (double)num * 100.0);
				BSNG3Label[j].Content = string.Format("NG3:  {0:N1} %", (double)OldBinCount[j].Count[4] / (double)num * 100.0);
				BSNG4Label[j].Content = string.Format("NG4:  {0:N1} %", (double)OldBinCount[j].Count[5] / (double)num * 100.0);
				BSRetestLabel[j].Content = string.Format("Retest:  {0:N1} %", (double)OldBinCount[j].Count[0] / (double)num * 100.0);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckRecentBuffer(bool ShowError = true)
	{
		try
		{
			int num = -1;
			for (int i = 0; i < 8; i++)
			{
				if (Station[i].paramFile.StationON)
				{
					if (num == -1)
					{
						num = i;
					}
				}
				else
				{
					StationEnable[i] = false;
					CBStation[i].IsEnabled = false;
					CBStation[i].IsChecked = false;
				}
				if (Station[i].RecentBufferList == null)
				{
					StationEnable[i] = false;
					CBStation[i].IsEnabled = false;
					CBStation[i].IsChecked = false;
				}
			}
			bool flag = false;
			if (num != -1 && Station[num].RecentBufferList != null && Station[num].RecentBufferList.Count > 0)
			{
				btn_Test.IsEnabled = true;
				GetRecentImage();
				lab_path.Text = "";
				lab_Total.Text = Station[num].RecentBufferList.Count.ToString();
				flag = true;
				UpdateUI = false;
				rad_RecentImage.IsChecked = true;
				UpdateUI = true;
			}
			if (!flag)
			{
				if (ShowError)
				{
					MessageBox.Show(LanguageManager.Instance["RecentImageNotExist"], LanguageManager.Instance["Warring"]);
				}
				btn_Test.IsEnabled = false;
				lab_Total.Text = "0";
				lab_path.Text = "";
				return false;
			}
			GetRecentImage();
			btn_Test.IsEnabled = true;
			return true;
		}
		catch (Exception ex)
		{
			grd_Loading.Visibility = Visibility.Hidden;
			string text = "YieldRtetestWindow-CheckRetestPath()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckRetestPath()
	{
		try
		{
			if (!Directory.Exists(YieldRetestPath))
			{
				MessageBox.Show(LanguageManager.Instance["LocalImageNotExist"], LanguageManager.Instance["Warring"]);
				btn_Test.IsEnabled = false;
				return false;
			}
			GetPathImage(YieldRetestPath);
			lab_path.Text = YieldRetestPath;
			btn_Test.IsEnabled = true;
			return true;
		}
		catch (Exception ex)
		{
			grd_Loading.Visibility = Visibility.Hidden;
			string text = "YieldRtetestWindow-CheckRetestPath()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return false;
		}
	}

	public void GetParamRatio(ref double ratio1, ref double ratio2, ref double ratio3)
	{
		ratio1 = Param1Ratio;
		ratio2 = Param2Ratio;
		ratio3 = Param3Ratio;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetParamRatio(double ratio1, double ratio2, double ratio3)
	{
		Param1Ratio = ratio1;
		Param2Ratio = ratio2;
		Param3Ratio = ratio3;
		if (Param1Ratio != 0.0)
		{
			lab_ParamRatio1.Content = string.Format("{0:N2}%", Param1Ratio);
			btn_LoadParam1.IsEnabled = true;
		}
		if (Param2Ratio != 0.0)
		{
			lab_ParamRatio2.Content = string.Format("{0:N2}%", Param2Ratio);
			btn_LoadParam2.IsEnabled = true;
		}
		if (Param3Ratio != 0.0)
		{
			lab_ParamRatio3.Content = string.Format("{0:N2}%", Param3Ratio);
			btn_LoadParam3.IsEnabled = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_SaveParam1_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.CopyParam(ref Station[i].param1);
		}
		lab_ParamRatio1.Content = string.Format("{0:N2}%", OK_Ratio);
		Param1Ratio = OK_Ratio;
		btn_LoadParam1.IsEnabled = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_SaveParam2_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.CopyParam(ref Station[i].param2);
		}
		lab_ParamRatio2.Content = string.Format("{0:N2}%", OK_Ratio);
		Param2Ratio = OK_Ratio;
		btn_LoadParam2.IsEnabled = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_SaveParam3_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.CopyParam(ref Station[i].param3);
		}
		lab_ParamRatio3.Content = string.Format("{0:N2}%", OK_Ratio);
		Param3Ratio = OK_Ratio;
		btn_LoadParam3.IsEnabled = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_LoadParam1_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(LanguageManager.Instance["ParamRestoreWarring"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			for (int i = 0; i < 8; i++)
			{
				Station[i].param1.CopyParam(ref Station[i].paramFile);
			}
			MessageBox.Show(LanguageManager.Instance["ParamRestoreFinish"], LanguageManager.Instance["Warring"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_LoadParam2_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(LanguageManager.Instance["ParamRestoreWarring"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			for (int i = 0; i < 8; i++)
			{
				Station[i].param2.CopyParam(ref Station[i].paramFile);
			}
			MessageBox.Show(LanguageManager.Instance["ParamRestoreFinish"], LanguageManager.Instance["Warring"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_LoadParam3_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(LanguageManager.Instance["ParamRestoreWarring"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			for (int i = 0; i < 8; i++)
			{
				Station[i].param3.CopyParam(ref Station[i].paramFile);
			}
			MessageBox.Show(LanguageManager.Instance["ParamRestoreFinish"], LanguageManager.Instance["Warring"]);
		}
	}

	private void btn_cancel_Click(object sender, RoutedEventArgs e)
	{
		InspectCancel = true;
	}

	private void SetEnable(bool enable)
	{
		rad_RecentImage.IsEnabled = enable;
		rad_Localmage.IsEnabled = enable;
		dp_Station.IsEnabled = enable;
		btn_Test.IsEnabled = enable;
		tab_Main.IsEnabled = enable;
		grd_Temp1.IsEnabled = enable;
		grd_Temp2.IsEnabled = enable;
		grd_Temp3.IsEnabled = enable;
		if (!UseRecentImg)
		{
			btn_SetPath.IsEnabled = enable;
		}
	}

	private unsafe void ConvertBGR2Bayer(Mat BGRImage, out byte[] data)
	{
		IntPtr data2 = BGRImage.Data;
		int width = BGRImage.Width;
		Mat mat = new Mat(BGRImage.Rows, BGRImage.Cols, MatType.CV_8UC1);
		IntPtr data3 = mat.Data;
		byte* ptr = (byte*)data2.ToPointer();
		byte* ptr2 = (byte*)data3.ToPointer();
		for (int i = 0; i < mat.Rows; i++)
		{
			int num = width * 3 * i;
			int num2 = width * i;
			for (int j = 0; j < mat.Cols; j++)
			{
				int num3 = ((i % 2 != 0) ? ((j % 2 == 0) ? 1 : 0) : ((j % 2 != 0) ? 1 : 2));
				byte b = 0;
				ptr2[num2 + j] = num3 switch
				{
					0 => ptr[num + j * 3], 
					1 => ptr[num + j * 3 + 1], 
					_ => ptr[num + j * 3 + 2], 
				};
			}
		}
		if (mat.Width > 0 && mat.Height > 0)
		{
			data = new byte[mat.Width * mat.Height];
			Marshal.Copy(mat.Data, data, 0, mat.Width * mat.Height);
		}
		else
		{
			data = new byte[1];
		}
	}

	public void GetImageData(ref int[] width, ref int[] height, ref int[] band)
	{
		for (int i = 0; i < 8; i++)
		{
			width[i] = ImageWidth[i];
			height[i] = ImageHeight[i];
			band[i] = ImageBand[i];
		}
	}

	private void rad_RecentImage_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			btn_SetPath.IsEnabled = false;
			lab_path.Text = "";
			UseRecentImg = true;
			lab_Total.Text = "";
			CheckRecentBuffer();
			rt_RecentImage.Fill = Brushes.White;
			rt_LocalImage.Fill = Brushes.Transparent;
		}
	}

	private void rad_Localmage_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			btn_SetPath.IsEnabled = true;
			lab_path.Text = YieldRetestPath;
			UseRecentImg = false;
			CheckRetestPath();
			rt_RecentImage.Fill = Brushes.Transparent;
			rt_LocalImage.Fill = Brushes.White;
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
			Uri resourceLocator = new Uri("/InspTools;component/other/yieldretestwindow.xaml", UriKind.Relative);
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
			wnd_main = (YieldRetestWindow)target;
			break;
		case 2:
			grd_Main = (Grid)target;
			break;
		case 3:
			rt_RecentImage = (Rectangle)target;
			break;
		case 4:
			rt_LocalImage = (Rectangle)target;
			break;
		case 5:
			lab_LocalImage = (Label)target;
			break;
		case 6:
			lab_RecentImage = (Label)target;
			break;
		case 7:
			tab_Main = (TabControl)target;
			tab_Main.SelectionChanged += TabControl_SelectionChanged;
			break;
		case 8:
			tab_MainYield = (TabItem)target;
			break;
		case 9:
			lab_TotalOKB = (Label)target;
			break;
		case 10:
			lab_TotalOKRatioB = (Label)target;
			break;
		case 11:
			lab_TotalNG1B = (Label)target;
			break;
		case 12:
			lab_TotalNG1RatioB = (Label)target;
			break;
		case 13:
			lab_TotalNG2B = (Label)target;
			break;
		case 14:
			lab_TotalNG2RatioB = (Label)target;
			break;
		case 15:
			lab_TotalNG3B = (Label)target;
			break;
		case 16:
			lab_TotalNG3RatioB = (Label)target;
			break;
		case 17:
			lab_TotalNG4B = (Label)target;
			break;
		case 18:
			lab_TotalNG4RatioB = (Label)target;
			break;
		case 19:
			lab_TotalRetestB = (Label)target;
			break;
		case 20:
			lab_TotalRetestRatioB = (Label)target;
			break;
		case 21:
			lab_TotalOK2RatioB = (Label)target;
			break;
		case 22:
			lab_TotalOKA = (Label)target;
			break;
		case 23:
			lab_TotalOKRatioA = (Label)target;
			break;
		case 24:
			lab_TotalNG1A = (Label)target;
			break;
		case 25:
			lab_TotalNG1RatioA = (Label)target;
			break;
		case 26:
			lab_TotalNG2A = (Label)target;
			break;
		case 27:
			lab_TotalNG2RatioA = (Label)target;
			break;
		case 28:
			lab_TotalNG3A = (Label)target;
			break;
		case 29:
			lab_TotalNG3RatioA = (Label)target;
			break;
		case 30:
			lab_TotalNG4A = (Label)target;
			break;
		case 31:
			lab_TotalNG4RatioA = (Label)target;
			break;
		case 32:
			lab_TotalRetestA = (Label)target;
			break;
		case 33:
			lab_TotalRetestRatioA = (Label)target;
			break;
		case 34:
			lab_TotalOK2RatioA = (Label)target;
			break;
		case 35:
			btn_CopyTo = (Button)target;
			btn_CopyTo.Click += btn_CopyTo_Click;
			break;
		case 36:
			tab_StationYield = (TabItem)target;
			break;
		case 37:
			grd_bs1 = (Grid)target;
			break;
		case 38:
			lab_ok_bs1 = (Label)target;
			break;
		case 39:
			lab_ng1_bs1 = (Label)target;
			break;
		case 40:
			lab_ng2_bs1 = (Label)target;
			break;
		case 41:
			lab_ng3_bs1 = (Label)target;
			break;
		case 42:
			lab_ng4_bs1 = (Label)target;
			break;
		case 43:
			lab_retest_bs1 = (Label)target;
			break;
		case 44:
			lab_ok2_bs1 = (Label)target;
			break;
		case 45:
			grd_bs2 = (Grid)target;
			break;
		case 46:
			lab_ok_bs2 = (Label)target;
			break;
		case 47:
			lab_ng1_bs2 = (Label)target;
			break;
		case 48:
			lab_ng2_bs2 = (Label)target;
			break;
		case 49:
			lab_ng3_bs2 = (Label)target;
			break;
		case 50:
			lab_ng4_bs2 = (Label)target;
			break;
		case 51:
			lab_retest_bs2 = (Label)target;
			break;
		case 52:
			lab_ok2_bs2 = (Label)target;
			break;
		case 53:
			grd_bs3 = (Grid)target;
			break;
		case 54:
			lab_ok_bs3 = (Label)target;
			break;
		case 55:
			lab_ng1_bs3 = (Label)target;
			break;
		case 56:
			lab_ng2_bs3 = (Label)target;
			break;
		case 57:
			lab_ng3_bs3 = (Label)target;
			break;
		case 58:
			lab_ng4_bs3 = (Label)target;
			break;
		case 59:
			lab_retest_bs3 = (Label)target;
			break;
		case 60:
			lab_ok2_bs3 = (Label)target;
			break;
		case 61:
			grd_bs4 = (Grid)target;
			break;
		case 62:
			lab_ok_bs4 = (Label)target;
			break;
		case 63:
			lab_ng1_bs4 = (Label)target;
			break;
		case 64:
			lab_ng2_bs4 = (Label)target;
			break;
		case 65:
			lab_ng3_bs4 = (Label)target;
			break;
		case 66:
			lab_ng4_bs4 = (Label)target;
			break;
		case 67:
			lab_retest_bs4 = (Label)target;
			break;
		case 68:
			lab_ok2_bs4 = (Label)target;
			break;
		case 69:
			grd_bs5 = (Grid)target;
			break;
		case 70:
			lab_ok_bs5 = (Label)target;
			break;
		case 71:
			lab_ng1_bs5 = (Label)target;
			break;
		case 72:
			lab_ng2_bs5 = (Label)target;
			break;
		case 73:
			lab_ng3_bs5 = (Label)target;
			break;
		case 74:
			lab_ng4_bs5 = (Label)target;
			break;
		case 75:
			lab_retest_bs5 = (Label)target;
			break;
		case 76:
			lab_ok2_bs5 = (Label)target;
			break;
		case 77:
			grd_bs6 = (Grid)target;
			break;
		case 78:
			lab_ok_bs6 = (Label)target;
			break;
		case 79:
			lab_ng1_bs6 = (Label)target;
			break;
		case 80:
			lab_ng2_bs6 = (Label)target;
			break;
		case 81:
			lab_ng3_bs6 = (Label)target;
			break;
		case 82:
			lab_ng4_bs6 = (Label)target;
			break;
		case 83:
			lab_retest_bs6 = (Label)target;
			break;
		case 84:
			lab_ok2_bs6 = (Label)target;
			break;
		case 85:
			grd_bs7 = (Grid)target;
			break;
		case 86:
			lab_ok_bs7 = (Label)target;
			break;
		case 87:
			lab_ng1_bs7 = (Label)target;
			break;
		case 88:
			lab_ng2_bs7 = (Label)target;
			break;
		case 89:
			lab_ng3_bs7 = (Label)target;
			break;
		case 90:
			lab_ng4_bs7 = (Label)target;
			break;
		case 91:
			lab_retest_bs7 = (Label)target;
			break;
		case 92:
			lab_ok2_bs7 = (Label)target;
			break;
		case 93:
			grd_bs8 = (Grid)target;
			break;
		case 94:
			lab_ok_bs8 = (Label)target;
			break;
		case 95:
			lab_ng1_bs8 = (Label)target;
			break;
		case 96:
			lab_ng2_bs8 = (Label)target;
			break;
		case 97:
			lab_ng3_bs8 = (Label)target;
			break;
		case 98:
			lab_ng4_bs8 = (Label)target;
			break;
		case 99:
			lab_retest_bs8 = (Label)target;
			break;
		case 100:
			lab_ok2_bs8 = (Label)target;
			break;
		case 101:
			grd_as1 = (Grid)target;
			break;
		case 102:
			lab_ok_as1 = (Label)target;
			break;
		case 103:
			lab_ng1_as1 = (Label)target;
			break;
		case 104:
			lab_ng2_as1 = (Label)target;
			break;
		case 105:
			lab_ng3_as1 = (Label)target;
			break;
		case 106:
			lab_ng4_as1 = (Label)target;
			break;
		case 107:
			lab_retest_as1 = (Label)target;
			break;
		case 108:
			lab_ok2_as1 = (Label)target;
			break;
		case 109:
			grd_as2 = (Grid)target;
			break;
		case 110:
			lab_ok_as2 = (Label)target;
			break;
		case 111:
			lab_ng1_as2 = (Label)target;
			break;
		case 112:
			lab_ng2_as2 = (Label)target;
			break;
		case 113:
			lab_ng3_as2 = (Label)target;
			break;
		case 114:
			lab_ng4_as2 = (Label)target;
			break;
		case 115:
			lab_retest_as2 = (Label)target;
			break;
		case 116:
			lab_ok2_as2 = (Label)target;
			break;
		case 117:
			grd_as3 = (Grid)target;
			break;
		case 118:
			lab_ok_as3 = (Label)target;
			break;
		case 119:
			lab_ng1_as3 = (Label)target;
			break;
		case 120:
			lab_ng2_as3 = (Label)target;
			break;
		case 121:
			lab_ng3_as3 = (Label)target;
			break;
		case 122:
			lab_ng4_as3 = (Label)target;
			break;
		case 123:
			lab_retest_as3 = (Label)target;
			break;
		case 124:
			lab_ok2_as3 = (Label)target;
			break;
		case 125:
			grd_as4 = (Grid)target;
			break;
		case 126:
			lab_ok_as4 = (Label)target;
			break;
		case 127:
			lab_ng1_as4 = (Label)target;
			break;
		case 128:
			lab_ng2_as4 = (Label)target;
			break;
		case 129:
			lab_ng3_as4 = (Label)target;
			break;
		case 130:
			lab_ng4_as4 = (Label)target;
			break;
		case 131:
			lab_retest_as4 = (Label)target;
			break;
		case 132:
			lab_ok2_as4 = (Label)target;
			break;
		case 133:
			grd_as5 = (Grid)target;
			break;
		case 134:
			lab_ok_as5 = (Label)target;
			break;
		case 135:
			lab_ng1_as5 = (Label)target;
			break;
		case 136:
			lab_ng2_as5 = (Label)target;
			break;
		case 137:
			lab_ng3_as5 = (Label)target;
			break;
		case 138:
			lab_ng4_as5 = (Label)target;
			break;
		case 139:
			lab_retest_as5 = (Label)target;
			break;
		case 140:
			lab_ok2_as5 = (Label)target;
			break;
		case 141:
			grd_as6 = (Grid)target;
			break;
		case 142:
			lab_ok_as6 = (Label)target;
			break;
		case 143:
			lab_ng1_as6 = (Label)target;
			break;
		case 144:
			lab_ng2_as6 = (Label)target;
			break;
		case 145:
			lab_ng3_as6 = (Label)target;
			break;
		case 146:
			lab_ng4_as6 = (Label)target;
			break;
		case 147:
			lab_retest_as6 = (Label)target;
			break;
		case 148:
			lab_ok2_as6 = (Label)target;
			break;
		case 149:
			grd_as7 = (Grid)target;
			break;
		case 150:
			lab_ok_as7 = (Label)target;
			break;
		case 151:
			lab_ng1_as7 = (Label)target;
			break;
		case 152:
			lab_ng2_as7 = (Label)target;
			break;
		case 153:
			lab_ng3_as7 = (Label)target;
			break;
		case 154:
			lab_ng4_as7 = (Label)target;
			break;
		case 155:
			lab_retest_as7 = (Label)target;
			break;
		case 156:
			lab_ok2_as7 = (Label)target;
			break;
		case 157:
			grd_as8 = (Grid)target;
			break;
		case 158:
			lab_ok_as8 = (Label)target;
			break;
		case 159:
			lab_ng1_as8 = (Label)target;
			break;
		case 160:
			lab_ng2_as8 = (Label)target;
			break;
		case 161:
			lab_ng3_as8 = (Label)target;
			break;
		case 162:
			lab_ng4_as8 = (Label)target;
			break;
		case 163:
			lab_retest_as8 = (Label)target;
			break;
		case 164:
			lab_ok2_as8 = (Label)target;
			break;
		case 165:
			tab_StationNGCount = (TabItem)target;
			break;
		case 166:
			lab_path = (TextBox)target;
			break;
		case 167:
			btn_SetPath = (Button)target;
			btn_SetPath.Click += btn_SetPath_Click;
			break;
		case 168:
			btn_Test = (Button)target;
			btn_Test.Click += btn_Test_Click;
			break;
		case 169:
			lab_Total = (TextBox)target;
			break;
		case 170:
			dp_Station = (DockPanel)target;
			break;
		case 171:
			cb_st1 = (CheckBox)target;
			break;
		case 172:
			cb_st2 = (CheckBox)target;
			break;
		case 173:
			cb_st3 = (CheckBox)target;
			break;
		case 174:
			cb_st4 = (CheckBox)target;
			break;
		case 175:
			cb_st5 = (CheckBox)target;
			break;
		case 176:
			cb_st6 = (CheckBox)target;
			break;
		case 177:
			cb_st7 = (CheckBox)target;
			break;
		case 178:
			cb_st8 = (CheckBox)target;
			break;
		case 179:
			grd_Loading = (Grid)target;
			break;
		case 180:
			lab_Loading = (Label)target;
			break;
		case 181:
			pb_TestProgress = (ProgressBar)target;
			break;
		case 182:
			btn_cancel = (Button)target;
			btn_cancel.Click += btn_cancel_Click;
			break;
		case 183:
			grd_Temp1 = (Grid)target;
			break;
		case 184:
			btn_SaveParam1 = (Button)target;
			btn_SaveParam1.Click += btn_SaveParam1_Click;
			break;
		case 185:
			btn_LoadParam1 = (Button)target;
			btn_LoadParam1.Click += btn_LoadParam1_Click;
			break;
		case 186:
			lab_ParamRatio1 = (Label)target;
			break;
		case 187:
			grd_Temp2 = (Grid)target;
			break;
		case 188:
			btn_SaveParam2 = (Button)target;
			btn_SaveParam2.Click += btn_SaveParam2_Click;
			break;
		case 189:
			btn_LoadParam2 = (Button)target;
			btn_LoadParam2.Click += btn_LoadParam2_Click;
			break;
		case 190:
			lab_ParamRatio2 = (Label)target;
			break;
		case 191:
			grd_Temp3 = (Grid)target;
			break;
		case 192:
			btn_SaveParam3 = (Button)target;
			btn_SaveParam3.Click += btn_SaveParam3_Click;
			break;
		case 193:
			btn_LoadParam3 = (Button)target;
			btn_LoadParam3.Click += btn_LoadParam3_Click;
			break;
		case 194:
			lab_ParamRatio3 = (Label)target;
			break;
		case 195:
			rad_RecentImage = (RadioButton)target;
			rad_RecentImage.Checked += rad_RecentImage_Checked;
			break;
		case 196:
			rad_Localmage = (RadioButton)target;
			rad_Localmage.Checked += rad_Localmage_Checked;
			break;
		case 197:
			lab_RecentImage2 = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
