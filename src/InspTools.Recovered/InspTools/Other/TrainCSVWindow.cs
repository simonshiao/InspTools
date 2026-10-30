using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.Other;

public class TrainCSVWindow : System.Windows.Window, IComponentConnector
{
	public struct TrainResultItem
	{
		public string ItemIndex { get; set; }

		public string BinaryValue { get; set; }

		public string Morph { get; set; }

		public string MorphCount { get; set; }

		public string OKWrong { get; set; }

		public string NGMiss { get; set; }

		public List<bool> TrainResultList { get; set; }

		public List<float> TrainResultMin { get; set; }

		public List<float> TrainResultMax { get; set; }

		public List<string> FilelistOKWrong { get; set; }

		public List<string> FilelistNGWrong { get; set; }
	}

	private List<TrainResultItem> TRItems = new List<TrainResultItem>();

	private List<TrainResultItem> ScriptItems = new List<TrainResultItem>();

	private int TrainCount;

	private CheckBox[] FontCheckBefore;

	private CheckBox[] FontCheckAfter;

	private Grid[] GrdFontCheck;

	private Label[] LabValueChange;

	private int FontAlgorithmCount;

	private string SelectedPath = "";

	private string TrainOKPath = "";

	private string TrainNGPath = "";

	private string CurrentTestPath = "";

	private string[] FilelistOK;

	private string[] FilelistNG;

	private string[] FileListTest;

	private int FileListTestIndex;

	private List<string> FilelistOKWrong = new List<string>();

	private List<string> FilelistNGWrong = new List<string>();

	private double Yield_Old;

	private double MissYield_Old;

	private const int MaxItem = 16;

	private int FontIndex;

	private string FontSetupChar = "R";

	private bool HideTrainingWnd = true;

	private bool ResultCreated;

	private List<bool> TrainResultList = new List<bool>();

	private List<float> TrainResultMin = new List<float>();

	private List<float> TrainResultMax = new List<float>();

	private CVInspection inspection;

	private INSP_FONTTOOL_PARAM BackupParam;

	private bool ShowMessage = true;

	private bool WriteResult = true;

	private bool inited;

	private bool isDragging2;

	private System.Windows.Point startPosition2;

	private bool IsBusy;

	private DispatcherTimer TrainCSVTimer = new DispatcherTimer();

	private DispatcherTimer TrainScriptCSVTimer = new DispatcherTimer();

	internal Grid grd_Advance;

	internal ComboBox cb_NGWeight;

	internal CheckBox chk_TrainPara;

	internal Label lab_FontSetup;

	internal Label lab_FontSetupMain_Copy;

	internal Button btn_OpenTrainCSV;

	internal Grid grd_Result;

	internal Grid grd_FontCheck1;

	internal Label lab_ValueChange1;

	internal CheckBox rad_FontCheckBefore1;

	internal CheckBox rad_FontCheckAfter1;

	internal Grid grd_FontCheck2;

	internal CheckBox rad_FontCheckBefore2;

	internal Label lab_ValueChange2;

	internal CheckBox rad_FontCheckAfter2;

	internal Grid grd_FontCheck3;

	internal CheckBox rad_FontCheckBefore3;

	internal Label lab_ValueChange3;

	internal CheckBox rad_FontCheckAfter3;

	internal Grid grd_FontCheck4;

	internal CheckBox rad_FontCheckBefore4;

	internal Label lab_ValueChange4;

	internal CheckBox rad_FontCheckAfter4;

	internal Grid grd_FontCheck5;

	internal CheckBox rad_FontCheckBefore5;

	internal Label lab_ValueChange5;

	internal CheckBox rad_FontCheckAfter5;

	internal Grid grd_FontCheck6;

	internal CheckBox rad_FontCheckBefore6;

	internal Label lab_ValueChange6;

	internal CheckBox rad_FontCheckAfter6;

	internal Grid grd_FontCheck7;

	internal CheckBox rad_FontCheckBefore7;

	internal Label lab_ValueChange7;

	internal CheckBox rad_FontCheckAfter7;

	internal Grid grd_FontCheck8;

	internal CheckBox rad_FontCheckBefore8;

	internal Label lab_ValueChange8;

	internal CheckBox rad_FontCheckAfter8;

	internal Grid grd_FontCheck9;

	internal CheckBox rad_FontCheckBefore9;

	internal Label lab_ValueChange9;

	internal CheckBox rad_FontCheckAfter9;

	internal Grid grd_FontCheck10;

	internal CheckBox rad_FontCheckBefore10;

	internal Label lab_ValueChange10;

	internal CheckBox rad_FontCheckAfter10;

	internal Grid grd_FontCheck11;

	internal CheckBox rad_FontCheckBefore11;

	internal Label lab_ValueChange11;

	internal CheckBox rad_FontCheckAfter11;

	internal Grid grd_FontCheck12;

	internal CheckBox rad_FontCheckBefore12;

	internal Label lab_ValueChange12;

	internal CheckBox rad_FontCheckAfter12;

	internal Grid grd_FontCheck13;

	internal CheckBox rad_FontCheckBefore13;

	internal Label lab_ValueChange13;

	internal CheckBox rad_FontCheckAfter13;

	internal Grid grd_FontCheck14;

	internal CheckBox rad_FontCheckBefore14;

	internal Label lab_ValueChange14;

	internal CheckBox rad_FontCheckAfter14;

	internal Grid grd_FontCheck15;

	internal CheckBox rad_FontCheckBefore15;

	internal Label lab_ValueChange15;

	internal CheckBox rad_FontCheckAfter15;

	internal Grid grd_FontCheck16;

	internal CheckBox rad_FontCheckBefore16;

	internal Label lab_ValueChange16;

	internal CheckBox rad_FontCheckAfter16;

	internal Button btn_SaveTrainResult;

	internal CheckBox cb_ShowTrainWnd;

	internal Button btn_ShowList;

	internal Grid grd_2;

	internal Label ico_ResultNGFont1_Copy;

	internal ComboBox cb_FontSelectTest;

	internal Button btn_TrainCSV;

	internal Grid grd_3;

	internal Label ico_ResultNGFont1_Copy1;

	internal Grid grd_4;

	internal Label lab_TrainResult;

	internal Label ico_ResultNGFont1_Copy2;

	internal Button btn_testYield;

	internal Grid grd_Image;

	internal Button btn_OKImage;

	internal Button btn_NGImage;

	internal Label lab_ValueChange1_Copy;

	internal Label lab_ValueChange1_Copy2;

	internal Grid grd_Preview;

	internal Button btn_OKWrong;

	internal Button btn_NGWrong;

	internal Label lab_ValueChange1_Copy1;

	internal Label lab_ValueChange1_Copy3;

	internal Label lab_Yelid;

	internal Label lab_MissYield;

	internal ListView lv_AIResultList;

	internal Grid grd_FontTest;

	internal Image img_TestFont;

	internal Button btn_prev;

	internal Button btn_next;

	internal Button btn_FontTest;

	internal TextBox txt_FontCount;

	internal Label lab_FontTestResult;

	internal TextBox txt_TestFontResult;

	internal Image img_TestFontPattern;

	internal Label lab_FontTestResultScore;

	internal Label lab_FontOCRResult;

	internal Button btn_CloseCheckFontTool;

	internal Label lab_FontTestSet;

	internal Button btn_OpenFolder;

	internal Slider sl_BinaryShift;

	internal ComboBox cb_OpenClose;

	internal ComboBox cb_OpenCloseCount;

	internal Label lab_BinaryShift;

	internal Image img_TestFontBinary;

	internal ListView lv_ScriptList;

	internal Button btn_AddToScript;

	internal Button btn_DeleteScript;

	internal Button btn_ScriptTest;

	internal Grid grd_Loading;

	internal Label lab_Loading;

	internal Button btn_ClearList;

	internal Button btn_TrainResultLoad;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public TrainCSVWindow()
	{
		InitializeComponent();
		FontCheckBefore = new CheckBox[16]
		{
			rad_FontCheckBefore1, rad_FontCheckBefore2, rad_FontCheckBefore3, rad_FontCheckBefore4, rad_FontCheckBefore5, rad_FontCheckBefore6, rad_FontCheckBefore7, rad_FontCheckBefore8, rad_FontCheckBefore9, rad_FontCheckBefore10,
			rad_FontCheckBefore11, rad_FontCheckBefore12, rad_FontCheckBefore13, rad_FontCheckBefore14, rad_FontCheckBefore15, rad_FontCheckBefore16
		};
		FontCheckAfter = new CheckBox[16]
		{
			rad_FontCheckAfter1, rad_FontCheckAfter2, rad_FontCheckAfter3, rad_FontCheckAfter4, rad_FontCheckAfter5, rad_FontCheckAfter6, rad_FontCheckAfter7, rad_FontCheckAfter8, rad_FontCheckAfter9, rad_FontCheckAfter10,
			rad_FontCheckAfter11, rad_FontCheckAfter12, rad_FontCheckAfter13, rad_FontCheckAfter14, rad_FontCheckAfter15, rad_FontCheckAfter16
		};
		GrdFontCheck = new Grid[16]
		{
			grd_FontCheck1, grd_FontCheck2, grd_FontCheck3, grd_FontCheck4, grd_FontCheck5, grd_FontCheck6, grd_FontCheck7, grd_FontCheck8, grd_FontCheck9, grd_FontCheck10,
			grd_FontCheck11, grd_FontCheck12, grd_FontCheck13, grd_FontCheck14, grd_FontCheck15, grd_FontCheck16
		};
		LabValueChange = new Label[16]
		{
			lab_ValueChange1, lab_ValueChange2, lab_ValueChange3, lab_ValueChange4, lab_ValueChange5, lab_ValueChange6, lab_ValueChange7, lab_ValueChange8, lab_ValueChange9, lab_ValueChange10,
			lab_ValueChange11, lab_ValueChange12, lab_ValueChange13, lab_ValueChange14, lab_ValueChange15, lab_ValueChange16
		};
		TrainCSVTimer.Interval = TimeSpan.FromMilliseconds(500.0);
		TrainScriptCSVTimer.Interval = TimeSpan.FromMilliseconds(500.0);
		TrainCSVTimer.Tick += TrainCSVTimer_Tick;
		TrainCSVTimer.Stop();
		TrainScriptCSVTimer.Tick += TrainScriptCSVTimer_Tick;
		TrainScriptCSVTimer.Stop();
		grd_Loading.Visibility = Visibility.Hidden;
		grd_2.Visibility = Visibility.Hidden;
		grd_3.Visibility = Visibility.Hidden;
		grd_4.Visibility = Visibility.Hidden;
		for (int i = 0; i < 16; i++)
		{
			GrdFontCheck[i].Visibility = Visibility.Hidden;
		}
		grd_Preview.Visibility = Visibility.Hidden;
		grd_Image.Visibility = Visibility.Hidden;
		btn_SaveTrainResult.Visibility = Visibility.Hidden;
		grd_FontTest.Visibility = Visibility.Hidden;
		lab_Yelid.Content = "";
		lab_MissYield.Content = "";
		btn_testYield.Visibility = Visibility.Hidden;
		lv_AIResultList.ItemsSource = null;
		grd_Advance.Visibility = Visibility.Hidden;
	}

	public void SetInspection(CVInspection station)
	{
		inspection = station;
		inspection.fontTool.TrainAIFontParamMode = true;
		BackupParam = inspection.paramFile.FontToolParam;
		inited = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void TrainScriptCSVTimer_Tick(object sender, EventArgs e)
	{
		TrainCSVTimer.Stop();
		TrainScriptCSVTimer.Stop();
		IsBusy = true;
		string ErrorStr = "";
		try
		{
			inspection.fontTool.TrainAIFontParamMode = true;
			for (int s = 0; s < ScriptItems.Count; s++)
			{
				string text = string.Format("{0}Training\\FontAITrain\\result\\{1}", AppDomain.CurrentDomain.BaseDirectory, "result.csv");
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				LoadScriptValue(s);
				ClearCSVFile();
				FilelistOKWrong.Clear();
				FilelistNGWrong.Clear();
				ExportCSV();
				int AlgorithmCount = GetFontAlgorithmCount(FontIndex);
				FontAlgorithmCount = AlgorithmCount;
				if (AlgorithmCount <= 0)
				{
					ErrorStr = string.Format("字元 [{0}] 演算法筆數錯誤。", FontSetupChar);
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
					{
						MessageBox.Show(ErrorStr);
						grd_Loading.Visibility = Visibility.Hidden;
						grd_4.Visibility = Visibility.Visible;
						lab_TrainResult.Content = "訓練失敗";
						lab_TrainResult.Background = Brushes.PaleVioletRed;
						lab_Loading.Content = string.Format("批次字元參數訓練中...({0}/{1})", s + 1, ScriptItems.Count);
					}));
					IsBusy = false;
					return;
				}
				string path = string.Format("{0}Training\\FontAITrain\\{1}", AppDomain.CurrentDomain.BaseDirectory, "f_limit.txt");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				string text2 = "";
				for (int num = 0; num < AlgorithmCount; num++)
				{
					if (!inspection.fontTool.FontUsePara[num])
					{
						text2 += "bool\n";
						continue;
					}
					string text3 = "";
					string text4 = "";
					text2 += string.Format(arg0: (!inspection.fontTool.FontUseParaMin[num]) ? "0" : "1", arg1: (!inspection.fontTool.FontUseParaMax[num]) ? "0" : "1", format: "float, {0}, {1}\n");
				}
				using (StreamWriter streamWriter = new StreamWriter(path, append: true))
				{
					streamWriter.Write("{0}", text2);
					streamWriter.WriteLine();
				}
				HiniFile hiniFile = new HiniFile(string.Format("{0}Training\\FontAITrain\\{1}", AppDomain.CurrentDomain.BaseDirectory, "parameter.txt"), ";");
				hiniFile.WriteInt("Parameter", "depth", AlgorithmCount);
				hiniFile.WriteDouble("Parameter", "ng_weight", 0.1 * (double)cb_NGWeight.SelectedIndex);
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
				{
					lab_Loading.Content = string.Format("批次字元參數訓練中...({0}/{1})", s + 1, ScriptItems.Count);
					if (!RunTrainingScript(ref ErrorStr))
					{
						MessageBox.Show(ErrorStr);
						grd_Loading.Visibility = Visibility.Hidden;
						grd_4.Visibility = Visibility.Visible;
						lab_TrainResult.Content = "訓練失敗";
						lab_TrainResult.Background = Brushes.PaleVioletRed;
					}
				}));
				if (File.Exists(text))
				{
					bool flag = false;
					using (StreamReader streamReader = new StreamReader(text, Encoding.Default))
					{
						if (streamReader.ReadLine().Contains("feature"))
						{
							flag = true;
						}
					}
					if (flag)
					{
						CSVstrToArry(text, ref TrainResultList, ref TrainResultMin, ref TrainResultMax);
					}
					else
					{
						CSVstrToArry(text, ref TrainResultList);
					}
					int num2 = 0;
					for (int num3 = 0; num3 < TrainResultList.Count; num3++)
					{
						if (!TrainResultList[num3])
						{
							num2++;
						}
					}
					if (TrainResultList.Count > 0 && TrainResultList.Count == AlgorithmCount && num2 < TrainResultList.Count)
					{
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
						{
							UpdateResultUI(AlgorithmCount);
							TestYield();
							grd_4.Visibility = Visibility.Visible;
							grd_Preview.Visibility = Visibility.Visible;
						});
						continue;
					}
					if (TrainResultList.Count <= 0)
					{
						ErrorStr = string.Format("訓練失敗，讀取結果筆數為0");
					}
					if (TrainResultList.Count != AlgorithmCount)
					{
						ErrorStr = string.Format("訓練失敗，讀取結果筆數與字元演算法筆數不相符");
					}
					if (num2 >= TrainResultList.Count)
					{
						ErrorStr = string.Format("訓練結果異常，請檢查OK影像是否正確");
					}
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
					{
						lab_TrainResult.Content = "訓練失敗";
					}));
				}
				else
				{
					ErrorStr = string.Format("訓練失敗，找不到result.csv檔案");
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
					{
						MessageBox.Show(ErrorStr);
						grd_Loading.Visibility = Visibility.Hidden;
						grd_4.Visibility = Visibility.Visible;
						lab_TrainResult.Content = "訓練失敗";
						lab_TrainResult.Background = Brushes.PaleVioletRed;
					}));
				}
			}
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
			{
				grd_Loading.Visibility = Visibility.Hidden;
				lab_TrainResult.Content = "訓練成功";
				lab_TrainResult.Background = Brushes.LightGreen;
				ScriptItems.Clear();
				lv_ScriptList.ItemsSource = null;
				lv_ScriptList.ItemsSource = ScriptItems;
			}));
			IsBusy = false;
		}
		catch (Exception ex)
		{
			string str = "TCW01-TrainCSVWindow-TrainCSVTimer_Tick()-";
			str += ex.ToString();
			CVBuffer.SaveLog(str);
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
			{
				MessageBox.Show(str);
			});
			IsBusy = false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void TrainCSVTimer_Tick(object sender, EventArgs e)
	{
		TrainCSVTimer.Stop();
		TrainScriptCSVTimer.Stop();
		IsBusy = true;
		string ErrorStr = "";
		try
		{
			string text = string.Format("{0}Training\\FontAITrain\\result\\{1}", AppDomain.CurrentDomain.BaseDirectory, "result.csv");
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
			{
				grd_3.Visibility = Visibility.Visible;
				grd_Loading.Visibility = Visibility.Visible;
			});
			ClearCSVFile();
			ExportCSV();
			int AlgorithmCount = GetFontAlgorithmCount(FontIndex);
			FontAlgorithmCount = AlgorithmCount;
			if (AlgorithmCount <= 0)
			{
				ErrorStr = string.Format("字元 [{0}] 演算法筆數錯誤。", FontSetupChar);
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
				{
					MessageBox.Show(ErrorStr);
					grd_Loading.Visibility = Visibility.Hidden;
					grd_4.Visibility = Visibility.Visible;
					lab_TrainResult.Content = "訓練失敗";
					lab_TrainResult.Background = Brushes.PaleVioletRed;
				}));
				IsBusy = false;
				return;
			}
			string path = string.Format("{0}Training\\FontAITrain\\{1}", AppDomain.CurrentDomain.BaseDirectory, "f_limit.txt");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			string text2 = "";
			for (int num = 0; num < AlgorithmCount; num++)
			{
				if (!inspection.fontTool.FontUsePara[num])
				{
					text2 += "bool\n";
					continue;
				}
				string text3 = "";
				string text4 = "";
				text2 += string.Format(arg0: (!inspection.fontTool.FontUseParaMin[num]) ? "0" : "1", arg1: (!inspection.fontTool.FontUseParaMax[num]) ? "0" : "1", format: "float,{0},{1}\n");
			}
			using (StreamWriter streamWriter = new StreamWriter(path, append: true))
			{
				streamWriter.Write("{0}", text2);
				streamWriter.WriteLine();
			}
			HiniFile hiniFile = new HiniFile(string.Format("{0}Training\\FontAITrain\\{1}", AppDomain.CurrentDomain.BaseDirectory, "parameter.txt"), ";");
			hiniFile.WriteInt("Parameter", "depth", AlgorithmCount);
			hiniFile.WriteDouble("Parameter", "ng_weight", 0.1 * (double)cb_NGWeight.SelectedIndex);
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
			{
				if (!RunTrainingScript(ref ErrorStr))
				{
					MessageBox.Show(ErrorStr);
					grd_Loading.Visibility = Visibility.Hidden;
					grd_4.Visibility = Visibility.Visible;
					lab_TrainResult.Content = "訓練失敗";
					lab_TrainResult.Background = Brushes.PaleVioletRed;
				}
			}));
			if (File.Exists(text))
			{
				bool flag = false;
				using (StreamReader streamReader = new StreamReader(text, Encoding.Default))
				{
					if (streamReader.ReadLine().Contains("feature"))
					{
						flag = true;
					}
				}
				if (flag)
				{
					CSVstrToArry(text, ref TrainResultList, ref TrainResultMin, ref TrainResultMax);
				}
				else
				{
					CSVstrToArry(text, ref TrainResultList);
				}
				int num2 = 0;
				for (int num3 = 0; num3 < TrainResultList.Count; num3++)
				{
					if (!TrainResultList[num3])
					{
						num2++;
					}
				}
				if (TrainResultList.Count > 0 && TrainResultList.Count == AlgorithmCount && num2 < TrainResultList.Count)
				{
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
					{
						UpdateResultUI(AlgorithmCount);
						TestYield();
						grd_4.Visibility = Visibility.Visible;
						grd_Preview.Visibility = Visibility.Visible;
					});
					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
					{
						grd_4.Visibility = Visibility.Visible;
						grd_Loading.Visibility = Visibility.Hidden;
						grd_4.Visibility = Visibility.Visible;
						lab_TrainResult.Content = "訓練成功";
						lab_TrainResult.Background = Brushes.LightGreen;
					}));
					IsBusy = false;
					return;
				}
				if (TrainResultList.Count <= 0)
				{
					ErrorStr = string.Format("訓練失敗，讀取結果筆數為0");
				}
				if (TrainResultList.Count != AlgorithmCount)
				{
					ErrorStr = string.Format("訓練失敗，讀取結果筆數與字元演算法筆數不相符");
				}
				if (num2 >= TrainResultList.Count)
				{
					ErrorStr = string.Format("訓練結果異常，請檢查OK影像是否正確");
				}
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
				{
					MessageBox.Show(ErrorStr);
					grd_Loading.Visibility = Visibility.Hidden;
					grd_4.Visibility = Visibility.Visible;
					lab_TrainResult.Content = "訓練失敗";
					lab_TrainResult.Background = Brushes.PaleVioletRed;
				}));
				IsBusy = false;
			}
			else
			{
				ErrorStr = string.Format("訓練失敗，找不到result.csv檔案");
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
				{
					MessageBox.Show(ErrorStr);
					grd_Loading.Visibility = Visibility.Hidden;
					grd_4.Visibility = Visibility.Visible;
					lab_TrainResult.Content = "訓練失敗";
					lab_TrainResult.Background = Brushes.PaleVioletRed;
				}));
				IsBusy = false;
			}
		}
		catch (Exception ex)
		{
			string str = "TCW01-TrainCSVWindow-TrainCSVTimer_Tick()-";
			str += ex.ToString();
			CVBuffer.SaveLog(str);
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
			{
				MessageBox.Show(str);
			});
			IsBusy = false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateResultUI(int AlgorithmCount)
	{
		if (AlgorithmCount > 16)
		{
			return;
		}
		for (int i = AlgorithmCount; i < 16; i++)
		{
			GrdFontCheck[i].Visibility = Visibility.Hidden;
		}
		for (int j = 0; j < AlgorithmCount; j++)
		{
			GrdFontCheck[j].Visibility = Visibility.Visible;
			FontCheckAfter[j].IsChecked = TrainResultList[j];
		}
		btn_SaveTrainResult.Visibility = Visibility.Visible;
		try
		{
			switch (FontSetupChar)
			{
			case "0":
				Setup0Check();
				break;
			case "R":
				SetupRCheck();
				break;
			case "M":
				SetupMCheck();
				break;
			case "C":
				SetupCCheck();
				break;
			case "U":
				SetupUCheck();
				break;
			case "X":
				SetupXCheck();
				break;
			case "B":
				SetupBCheck();
				break;
			case "D":
				SetupDCheck();
				break;
			case "A":
				SetupACheck();
				break;
			case "E":
				SetupECheck();
				break;
			case "-":
				SetupDashCheck();
				break;
			case "1":
				Setup1Check();
				break;
			case "2":
				Setup2Check();
				break;
			case "3":
				Setup3Check();
				break;
			case "4":
				Setup4Check();
				break;
			case "5":
				Setup5Check();
				break;
			case "6":
				Setup6Check();
				break;
			case "7":
				Setup7Check();
				break;
			case "8":
				Setup8Check();
				break;
			case "9":
				Setup9Check();
				break;
			}
			for (int k = 0; k < AlgorithmCount; k++)
			{
				if (FontCheckBefore[k].IsChecked != TrainResultList[k])
				{
					GrdFontCheck[k].Background = Brushes.LightBlue;
					LabValueChange[k].Visibility = Visibility.Visible;
				}
				else
				{
					GrdFontCheck[k].Background = Brushes.White;
					LabValueChange[k].Visibility = Visibility.Hidden;
				}
			}
			string path = string.Format("{0}Training\\FontAITrain\\result\\OK_wrong\\", AppDomain.CurrentDomain.BaseDirectory);
			string path2 = string.Format("{0}Training\\FontAITrain\\result\\NG_wrong\\", AppDomain.CurrentDomain.BaseDirectory);
			string[] files = Directory.GetFiles(path, "*.csv", SearchOption.TopDirectoryOnly);
			string[] files2 = Directory.GetFiles(path2, "*.csv", SearchOption.TopDirectoryOnly);
			for (int l = 0; l < files.Length; l++)
			{
				for (int m = 0; m < FilelistOK.Length; m++)
				{
					if (Path.GetFileNameWithoutExtension(FilelistOK[m]) == Path.GetFileNameWithoutExtension(files[l]))
					{
						FilelistOKWrong.Add(FilelistOK[m]);
					}
				}
			}
			for (int n = 0; n < files2.Length; n++)
			{
				for (int num = 0; num < FilelistNG.Length; num++)
				{
					if (Path.GetFileNameWithoutExtension(FilelistNG[num]) == Path.GetFileNameWithoutExtension(files2[n]))
					{
						FilelistNGWrong.Add(FilelistNG[num]);
					}
				}
			}
			if (SelectedPath != "")
			{
				string text = string.Format("{0}\\OK_wrong\\", SelectedPath);
				string text2 = string.Format("{0}\\NG_wrong\\", SelectedPath);
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				string[] files3 = Directory.GetFiles(text, "*.bmp", SearchOption.TopDirectoryOnly);
				string[] files4 = Directory.GetFiles(text2, "*.bmp", SearchOption.TopDirectoryOnly);
				for (int num2 = 0; num2 < files3.Length; num2++)
				{
					File.Delete(files3[num2]);
				}
				for (int num3 = 0; num3 < files4.Length; num3++)
				{
					File.Delete(files4[num3]);
				}
				for (int num4 = 0; num4 < FilelistOKWrong.Count; num4++)
				{
					string destFileName = string.Format("{0}{1}", text, Path.GetFileName(FilelistOKWrong[num4]));
					File.Copy(FilelistOKWrong[num4], destFileName, overwrite: true);
				}
				for (int num5 = 0; num5 < FilelistNGWrong.Count; num5++)
				{
					string destFileName2 = string.Format("{0}{1}", text2, Path.GetFileName(FilelistNGWrong[num5]));
					File.Copy(FilelistNGWrong[num5], destFileName2, overwrite: true);
				}
			}
		}
		catch (Exception ex)
		{
			string text3 = "TCW01-TrainCSVWindow-TrainCSVTimer_Tick()-" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void TestYield()
	{
		if (FilelistOK.Length == 0 || FilelistNG.Length == 0)
		{
			return;
		}
		inspection.fontTool.TrainAIFontParamMode = false;
		int num = 0;
		int num2 = 0;
		try
		{
			num = 0;
			for (int i = 0; i < FilelistOK.Length; i++)
			{
				if (!TestFontInspect(FilelistOK[i]))
				{
					num2++;
				}
			}
			Yield_Old = (double)num2 / (double)FilelistOK.Length * 100.0;
			num = 0;
			for (int j = 0; j < FilelistNG.Length; j++)
			{
				if (TestFontInspect(FilelistNG[j]))
				{
					num++;
				}
			}
			MissYield_Old = (double)num / (double)FilelistNG.Length * 100.0;
			SaveFontSetup(FontSetupChar);
			string text = string.Format("{0}Training\\FontAITrain\\Debug\\OK_wrong\\", AppDomain.CurrentDomain.BaseDirectory);
			string text2 = string.Format("{0}Training\\FontAITrain\\Debug\\NG_wrong\\", AppDomain.CurrentDomain.BaseDirectory);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string[] files = Directory.GetFiles(text, "*.csv", SearchOption.TopDirectoryOnly);
			for (int k = 0; k < files.Length; k++)
			{
				File.Delete(files[k]);
			}
			files = Directory.GetFiles(text2, "*.csv", SearchOption.TopDirectoryOnly);
			for (int l = 0; l < files.Length; l++)
			{
				File.Delete(files[l]);
			}
			string path = string.Format("{0}Training\\FontAITrain\\OK\\", AppDomain.CurrentDomain.BaseDirectory);
			string path2 = string.Format("{0}Training\\FontAITrain\\NG\\", AppDomain.CurrentDomain.BaseDirectory);
			string[] files2 = Directory.GetFiles(path, "*.csv", SearchOption.TopDirectoryOnly);
			string[] files3 = Directory.GetFiles(path2, "*.csv", SearchOption.TopDirectoryOnly);
			num2 = 0;
			for (int m = 0; m < FilelistOK.Length; m++)
			{
				if (TestFontInspect(FilelistOK[m]))
				{
					continue;
				}
				bool flag = false;
				for (int n = 0; n < FilelistOKWrong.Count; n++)
				{
					if (Path.GetFileNameWithoutExtension(FilelistOK[m]) == Path.GetFileNameWithoutExtension(FilelistOKWrong[n]))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					for (int num3 = 0; num3 < files2.Length; num3++)
					{
						if (Path.GetFileNameWithoutExtension(FilelistOK[m]) == Path.GetFileNameWithoutExtension(files2[num3]))
						{
							string destFileName = string.Format("{0}{1}.csv", text, Path.GetFileNameWithoutExtension(FilelistOK[m]));
							File.Copy(files2[num3], destFileName);
						}
					}
				}
				num2++;
			}
			double num4 = (double)num2 / (double)FilelistOK.Length * 100.0;
			num = 0;
			for (int num5 = 0; num5 < FilelistNG.Length; num5++)
			{
				if (!TestFontInspect(FilelistNG[num5]))
				{
					continue;
				}
				bool flag2 = false;
				for (int num6 = 0; num6 < FilelistNGWrong.Count; num6++)
				{
					if (Path.GetFileNameWithoutExtension(FilelistNG[num5]) == Path.GetFileNameWithoutExtension(FilelistNGWrong[num6]))
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					for (int num7 = 0; num7 < files3.Length; num7++)
					{
						if (Path.GetFileNameWithoutExtension(FilelistNG[num5]) == Path.GetFileNameWithoutExtension(files3[num7]))
						{
							string destFileName2 = string.Format("{0}{1}.csv", text2, Path.GetFileNameWithoutExtension(FilelistNG[num5]));
							File.Copy(files3[num7], destFileName2);
						}
					}
				}
				num++;
			}
			double num8 = (double)num / (double)FilelistNG.Length * 100.0;
			btn_OKWrong.Content = string.Format("OK誤判 ({0})", num2);
			btn_NGWrong.Content = string.Format("NG漏判 ({0})", num);
			lab_Yelid.Content = string.Format("  誤判率: {0:N2}% => {1:N2}%", Yield_Old, num4);
			lab_MissYield.Content = string.Format("  漏判率: {0:N2}% => {1:N2}%", MissYield_Old, num8);
			inspection.fontTool.TrainAIFontParamMode = true;
			ResultCreated = true;
			ReadBinarySetting();
			TrainResultItem item = new TrainResultItem
			{
				ItemIndex = (TrainCount + 1).ToString(),
				BinaryValue = lab_BinaryShift.Content.ToString()
			};
			if (cb_OpenClose.SelectedIndex == 0)
			{
				item.Morph = "不使用";
			}
			else if (cb_OpenClose.SelectedIndex == 1)
			{
				item.Morph = "斷開";
			}
			else
			{
				item.Morph = "連結";
			}
			item.MorphCount = cb_OpenCloseCount.SelectedIndex.ToString();
			item.OKWrong = num2.ToString();
			item.NGMiss = num.ToString();
			item.TrainResultList = new List<bool>();
			item.TrainResultList = TrainResultList.ToList();
			item.TrainResultMin = new List<float>();
			item.TrainResultMin = TrainResultMin.ToList();
			item.TrainResultMax = new List<float>();
			item.TrainResultMax = TrainResultMax.ToList();
			TrainCount++;
			item.FilelistOKWrong = new List<string>();
			item.FilelistOKWrong = FilelistOKWrong.ToList();
			item.FilelistNGWrong = new List<string>();
			item.FilelistNGWrong = FilelistNGWrong.ToList();
			TRItems.Add(item);
			int selectedIndex = 0;
			int num9 = 99999;
			int num10 = 9999;
			for (int num11 = 0; num11 < TRItems.Count; num11++)
			{
				int num12 = Convert.ToInt32(TRItems[num11].NGMiss);
				int num13 = Convert.ToInt32(TRItems[num11].OKWrong);
				if (num12 > num9)
				{
					continue;
				}
				if (num12 == num9)
				{
					if (num13 < num10)
					{
						num9 = num12;
						num10 = num13;
						selectedIndex = num11;
					}
				}
				else
				{
					num9 = num12;
					num10 = num13;
					selectedIndex = num11;
				}
			}
			lv_AIResultList.Visibility = Visibility.Visible;
			btn_TrainResultLoad.Visibility = Visibility.Visible;
			lv_AIResultList.ItemsSource = null;
			lv_AIResultList.ItemsSource = TRItems;
			lv_AIResultList.SelectedIndex = selectedIndex;
		}
		catch (Exception ex)
		{
			string text3 = "FTW01-TrainCSVWindow-TestYield()";
			text3 += ex.ToString();
			CVBuffer.SaveLog(text3);
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateYield(int index)
	{
		if (TRItems.Count > 0 && index >= 0 && index < TRItems.Count && FilelistOK.Length != 0 && FilelistNG.Length != 0)
		{
			grd_FontTest.Visibility = Visibility.Hidden;
			FilelistOKWrong.Clear();
			FilelistOKWrong = TRItems[index].FilelistOKWrong.ToList();
			FilelistNGWrong.Clear();
			FilelistNGWrong = TRItems[index].FilelistNGWrong.ToList();
			double num = (double)FilelistOKWrong.Count / (double)FilelistOK.Length * 100.0;
			double num2 = (double)FilelistNGWrong.Count / (double)FilelistNG.Length * 100.0;
			btn_OKWrong.Content = string.Format("OK誤判 ({0})", FilelistOKWrong.Count);
			btn_NGWrong.Content = string.Format("NG漏判 ({0})", FilelistNGWrong.Count);
			lab_Yelid.Content = string.Format("  誤判率: {0:N2}% => {1:N2}%", Yield_Old, num);
			lab_MissYield.Content = string.Format("  漏判率: {0:N2}% => {1:N2}%", MissYield_Old, num2);
			ReadBinarySetting();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CSVstrToArry(string CSVFile, ref List<bool> enabledList, ref List<float> minList, ref List<float> maxList)
	{
		int num = 0;
		enabledList.Clear();
		minList.Clear();
		maxList.Clear();
		try
		{
			using StreamReader streamReader = new StreamReader(CSVFile, Encoding.Default);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!inspection.fontTool.FontUsePara[num])
				{
					string[] array = text.Split(',');
					if (array.Length >= 1)
					{
						if (array[1].Replace("\"", "").Trim() == "0")
						{
							enabledList.Add(item: false);
						}
						else
						{
							enabledList.Add(item: true);
						}
						minList.Add(0f);
						maxList.Add(0f);
					}
				}
				else
				{
					string[] array2 = text.Split(',');
					if (array2.Length >= 3)
					{
						string text2 = array2[1].Replace("\"", "").Trim();
						if (text2 == "0")
						{
							enabledList.Add(item: false);
						}
						else
						{
							enabledList.Add(item: true);
						}
						text2 = array2[2].Replace("\"", "").Trim();
						minList.Add((float)Convert.ToDouble(text2));
						text2 = array2[3].Replace("\"", "").Trim();
						maxList.Add((float)Convert.ToDouble(array2[3]));
					}
				}
				num++;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TCW01-TrainCSVWindow-CSVstrToArry" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CSVstrToArry(string CSVFile, ref List<bool> sList)
	{
		try
		{
			string text;
			using (StreamReader streamReader = new StreamReader(CSVFile, Encoding.Default))
			{
				text = streamReader.ReadLine();
			}
			sList.Clear();
			string text2 = string.Empty;
			bool flag = false;
			string[] array = text.Split(',');
			foreach (string text3 in array)
			{
				if (!string.IsNullOrEmpty(text3) && text3.IndexOf('"') > -1)
				{
					string text4 = text3.Substring(0, 1);
					string text5 = string.Empty;
					if (text3.Length > 0)
					{
						text5 = text3.Substring(text3.Length - 1, 1);
					}
					if (text4.Equals("\"") && !text5.Equals("\""))
					{
						flag = true;
					}
					if (text5.Equals("\""))
					{
						text2 = (flag ? (text2 + "," + text3) : (text2 + text3));
						flag = false;
					}
				}
				else if (string.IsNullOrEmpty(text2))
				{
					text2 += text3;
				}
				if (flag)
				{
					text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + "," + text3) : (text2 + text3));
					continue;
				}
				string text6 = text2.Replace("\"", "").Trim();
				if (text6 == "0")
				{
					sList.Add(item: false);
				}
				else if (text6 == "1")
				{
					sList.Add(item: true);
				}
				text2 = string.Empty;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TCW01-TrainCSVWindow-TrainCSVTimer_Tick()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool RunTrainingScript(ref string ErrorStr)
	{
		string text = "";
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "TrainCSV.bat");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		string arg = "cd Training\ncd FontAITrain\ndist\\auto\\auto.exe";
		using (StreamWriter streamWriter = new StreamWriter(path, append: true))
		{
			streamWriter.Write("{0}", arg);
			streamWriter.WriteLine();
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = "TrainCSV.bat";
		if (!File.Exists(path))
		{
			ErrorStr = string.Format("找不到訓練程式", processStartInfo.FileName);
			return false;
		}
		string text2 = "";
		if (HideTrainingWnd)
		{
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
		}
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = HideTrainingWnd;
		Process process = Process.Start(processStartInfo);
		if (HideTrainingWnd)
		{
			string eOut = null;
			process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
			{
				eOut += e.Data;
			};
			process.BeginErrorReadLine();
			text2 = process.StandardOutput.ReadToEnd();
			text = text + text2 + eOut;
			string path2 = string.Format("{0}Training\\FontAITrain\\{1}", AppDomain.CurrentDomain.BaseDirectory, "run.log");
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			using StreamWriter streamWriter2 = new StreamWriter(path2, append: true);
			streamWriter2.Write("{0}", text);
			streamWriter2.WriteLine();
		}
		process.WaitForExit();
		process.Close();
		return true;
	}

	private void ExportCSV()
	{
		inspection.fontTool.ResetFontPara();
		for (int i = 0; i < FilelistOK.Length; i++)
		{
			TestFontInspectExportCSV(FilelistOK[i], IsOK: true);
		}
		for (int j = 0; j < FilelistNG.Length; j++)
		{
			TestFontInspectExportCSV(FilelistNG[j], IsOK: false);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool TestFontInspect(string imgfile)
	{
		try
		{
			if (imgfile == "")
			{
				return false;
			}
			if (!File.Exists(imgfile))
			{
				return false;
			}
			string extension = Path.GetExtension(imgfile);
			if (extension.ToLower() != ".bmp" && extension.ToLower() != ".jpg")
			{
				return false;
			}
			Mat mat = CVBuffer.ImRead(imgfile, ImreadModes.Grayscale);
			Mat fontImageFromIndex = GetFontImageFromIndex(cb_FontSelectTest.SelectedIndex);
			if (mat.Width <= 0 || mat.Height <= 0 || mat.Channels() != 1)
			{
				return false;
			}
			if (fontImageFromIndex.Width <= 0 || fontImageFromIndex.Height <= 0 || fontImageFromIndex.Channels() != 1)
			{
				return false;
			}
			Mat mat2 = mat.Clone();
			Mat mat3 = fontImageFromIndex.Clone();
			if (inspection.paramFile.FontToolParam.FontAlignVertical)
			{
				Cv2.Transpose(mat3, mat3);
				Cv2.Flip(mat3, mat3, FlipMode.Y);
			}
			string str = "";
			Mat fontbinary = new Mat();
			if (inspection.fontTool.Check_Font(cb_FontSelectTest.SelectedIndex, mat2, mat3, ref str, ref fontbinary))
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			string text = "TW01-TrainCSVWindow-TestFontInspect()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool TestFontInspect(Mat mat)
	{
		try
		{
			inspection.fontTool.TrainAIFontParamMode = false;
			txt_TestFontResult.Text = "";
			string text = "";
			if (mat.Width <= 0 || mat.Height <= 0)
			{
				text = "讀取影像異常。";
				img_TestFontPattern.Source = null;
				img_TestFontBinary.Source = null;
				img_TestFont.Source = null;
				txt_TestFontResult.Text = text;
				return false;
			}
			Mat fontImageFromIndex = GetFontImageFromIndex(cb_FontSelectTest.SelectedIndex);
			img_TestFont.Source = mat.ToBitmapSource();
			if (fontImageFromIndex.Width <= 0 || fontImageFromIndex.Height <= 0 || fontImageFromIndex.Channels() != 1)
			{
				text = "尚未設定該字元 (字元資料庫)。";
				img_TestFontPattern.Source = null;
				img_TestFontBinary.Source = null;
				txt_TestFontResult.Text = text;
				return false;
			}
			bool result = true;
			Mat mat2 = mat.Clone();
			Mat mat3 = fontImageFromIndex.Clone();
			if (inspection.paramFile.FontToolParam.FontAlignVertical)
			{
				Cv2.Transpose(mat3, mat3);
				Cv2.Flip(mat3, mat3, FlipMode.Y);
			}
			img_TestFontPattern.Source = mat3.ToBitmapSource();
			string str = "";
			string ocrChar = "";
			bool aIOCRResult = inspection.fontTool.GetAIOCRResult(mat2, ref ocrChar);
			string fontCharFromIndex = inspection.fontTool.GetFontCharFromIndex(cb_FontSelectTest.SelectedIndex);
			bool flag = false;
			if (ocrChar == "1" && fontCharFromIndex == "-")
			{
				flag = true;
				lab_FontOCRResult.Content = ocrChar;
			}
			else if (ocrChar == "-" && fontCharFromIndex == "1")
			{
				flag = true;
				lab_FontOCRResult.Content = ocrChar;
			}
			else if (ocrChar == "7" && fontCharFromIndex == "1")
			{
				Mat fontImageFromIndex2 = GetFontImageFromIndex(12);
				if (fontImageFromIndex2.Width <= 0 || fontImageFromIndex2.Height <= 0)
				{
					flag = true;
					ocrChar = fontCharFromIndex;
				}
				else if (Math.Abs(mat2.Width - mat3.Width) < Math.Abs(fontImageFromIndex2.Width - mat3.Width))
				{
					flag = true;
					ocrChar = fontCharFromIndex;
				}
				else
				{
					flag = false;
				}
				lab_FontOCRResult.Content = ocrChar;
			}
			else if (ocrChar == "1" && fontCharFromIndex == "7")
			{
				Mat fontImageFromIndex3 = GetFontImageFromIndex(6);
				if (fontImageFromIndex3.Width <= 0 || fontImageFromIndex3.Height <= 0)
				{
					flag = true;
					ocrChar = fontCharFromIndex;
				}
				else if (Math.Abs(mat2.Width - mat3.Width) < Math.Abs(fontImageFromIndex3.Width - mat3.Width))
				{
					flag = true;
					ocrChar = fontCharFromIndex;
				}
				else
				{
					flag = false;
				}
				lab_FontOCRResult.Content = ocrChar;
			}
			else
			{
				lab_FontOCRResult.Content = ocrChar;
				flag = aIOCRResult;
			}
			if (!flag)
			{
				lab_FontOCRResult.Content = "OCR失敗";
			}
			lab_FontOCRResult.Background = Brushes.Aqua;
			if (ocrChar != fontCharFromIndex)
			{
				lab_FontOCRResult.Background = Brushes.Fuchsia;
			}
			Mat fontbinary = new Mat();
			if (inspection.fontTool.Check_Font(cb_FontSelectTest.SelectedIndex, mat2, mat3, ref str, ref fontbinary))
			{
				img_TestFontBinary.Source = fontbinary.ToBitmapSource();
				lab_FontTestResult.Content = "OK";
				lab_FontTestResult.Background = Brushes.LimeGreen;
			}
			else
			{
				img_TestFontBinary.Source = fontbinary.ToBitmapSource();
				txt_TestFontResult.Text = str;
				lab_FontTestResult.Content = "NG";
				lab_FontTestResult.Background = Brushes.Fuchsia;
				result = false;
			}
			OpenCvSharp.Rect rt = new OpenCvSharp.Rect(0, 0, 0, 0);
			if (mat2.Width != mat3.Width || mat2.Height != mat3.Height)
			{
				mat2 = mat2.Resize(new OpenCvSharp.Size(mat3.Width, mat3.Height));
			}
			int openCVMatchScore = inspection.fontTool.GetOpenCVMatchScore(mat2, mat3, out rt);
			lab_FontTestResultScore.Content = openCVMatchScore;
			lab_FontTestResultScore.Background = Brushes.LightGreen;
			return result;
		}
		catch (Exception ex)
		{
			string text2 = "TW01-TrainCSVWindow-TestFontInspect()";
			text2 += ex.ToString();
			CVBuffer.SaveLog(text2);
			if (ShowMessage)
			{
				MessageBox.Show(text2, LanguageManager.Instance["Error"]);
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool TestFontInspectExportCSV(string imgfile, bool IsOK)
	{
		try
		{
			if (imgfile == "")
			{
				return false;
			}
			if (!File.Exists(imgfile))
			{
				return false;
			}
			string extension = Path.GetExtension(imgfile);
			if (extension.ToLower() != ".bmp" && extension.ToLower() != ".jpg")
			{
				return false;
			}
			Mat mat = CVBuffer.ImRead(imgfile, ImreadModes.Grayscale);
			Mat fontImageFromIndex = GetFontImageFromIndex(cb_FontSelectTest.SelectedIndex);
			if (mat.Width <= 0 || mat.Height <= 0 || mat.Channels() != 1)
			{
				return false;
			}
			if (fontImageFromIndex.Width <= 0 || fontImageFromIndex.Height <= 0 || fontImageFromIndex.Channels() != 1)
			{
				return false;
			}
			Mat mat2 = mat.Clone();
			Mat mat3 = fontImageFromIndex.Clone();
			if (inspection.paramFile.FontToolParam.FontAlignVertical)
			{
				Cv2.Transpose(mat3, mat3);
				Cv2.Flip(mat3, mat3, FlipMode.Y);
			}
			string str = "";
			Mat fontbinary = new Mat();
			inspection.fontTool.Check_Font(cb_FontSelectTest.SelectedIndex, mat2, mat3, ref str, ref fontbinary);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imgfile);
			inspection.fontTool.ExportCSV(fileNameWithoutExtension, IsOK);
			return true;
		}
		catch (Exception ex)
		{
			string text = "TW01-TrainCSVWindow-TestFontInspect()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenTrainCSV_Click(object sender, RoutedEventArgs e)
	{
		if (IsBusy)
		{
			MessageBox.Show("系統訓練中...", "警告");
			return;
		}
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		try
		{
			if (!folderSelectDialog.ShowDialog())
			{
				return;
			}
			ResetUI();
			TrainOKPath = string.Format("{0}\\OK\\", folderSelectDialog.FileName);
			TrainNGPath = string.Format("{0}\\NG\\", folderSelectDialog.FileName);
			SelectedPath = folderSelectDialog.FileName;
			if (!Directory.Exists(TrainOKPath) || !Directory.Exists(TrainNGPath))
			{
				TrainOKPath = (TrainNGPath = "");
				return;
			}
			if (CheckFileList(TrainOKPath, TrainNGPath))
			{
				grd_2.Visibility = Visibility.Visible;
			}
			FindSameImage(FilelistOK, FilelistNG);
			grd_Image.Visibility = Visibility.Visible;
			btn_OKImage.Content = string.Format("OK ({0})", FilelistOK.Length);
			btn_NGImage.Content = string.Format("NG ({0})", FilelistNG.Length);
		}
		catch (Exception ex)
		{
			string text = "FTW01-TrainCSVWindow-Btn_OpenOKNG_Click()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string FindSameImage(string[] list1, string[] list2 = null)
	{
		string result = "";
		if (list1 == null)
		{
			return result;
		}
		try
		{
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			for (int i = 0; i < list1.Length; i++)
			{
				list3.Add(GetFileHash(list1[i]));
			}
			if (list2 != null)
			{
				for (int j = 0; j < list2.Length; j++)
				{
					list4.Add(GetFileHash(list2[j]));
				}
			}
			for (int k = 0; k < list3.Count; k++)
			{
				if (list3[k] == "")
				{
					continue;
				}
				for (int l = 0; l < list4.Count; l++)
				{
					if (list3[k] == list4[l])
					{
						Path.GetFileName(list1[k]);
						Path.GetFileName(list2[l]);
						string text = list1[k];
						string oldDrawpath = "";
						bool oldstatus = true;
						if (text.Contains("\\NG\\"))
						{
							oldDrawpath = "";
							oldstatus = false;
						}
						else if (text.Contains("\\OK\\"))
						{
							oldDrawpath = "";
							oldstatus = true;
						}
						string text2 = list2[l];
						string newDrawpath = "";
						bool newstatus = true;
						if (text2.Contains("\\NG\\"))
						{
							newDrawpath = "";
							newstatus = false;
						}
						else if (text2.Contains("\\OK\\"))
						{
							newDrawpath = "";
							newstatus = true;
						}
						SelectImageWindow selectImageWindow = new SelectImageWindow();
						selectImageWindow.SetMarkImageHide();
						selectImageWindow.SetImage(text, oldDrawpath, oldstatus, text2, newDrawpath, newstatus);
						selectImageWindow.ShowDialog();
						switch (selectImageWindow.GetDeleteResult())
						{
						case 1:
							list1[k] = "";
							break;
						case 2:
							list2[l] = "";
							break;
						}
					}
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIPreviewWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string FindSameImage2(string[] list1, string[] list2 = null)
	{
		string result = "";
		if (list1 == null)
		{
			return result;
		}
		try
		{
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			for (int i = 0; i < list1.Length; i++)
			{
				list3.Add(list1[i]);
			}
			if (list2 != null)
			{
				for (int j = 0; j < list2.Length; j++)
				{
					list3.Add(list2[j]);
				}
			}
			for (int k = 0; k < list3.Count; k++)
			{
				list4.Add(GetFileHash(list3[k]));
			}
			for (int l = 0; l < list4.Count; l++)
			{
				if (list4[l] == "")
				{
					continue;
				}
				for (int m = 0; m < list4.Count; m++)
				{
					if (!(list3[l] == list3[m]) && list4[l] == list4[m])
					{
						Path.GetFileName(list3[l]);
						Path.GetFileName(list3[m]);
						string text = list3[l];
						string oldDrawpath = "";
						bool oldstatus = true;
						if (text.Contains("\\NG\\"))
						{
							oldDrawpath = "";
							oldstatus = false;
						}
						else if (text.Contains("\\OK\\"))
						{
							oldDrawpath = "";
							oldstatus = true;
						}
						string text2 = list3[m];
						string newDrawpath = "";
						bool newstatus = true;
						if (text2.Contains("\\NG\\"))
						{
							newDrawpath = "";
							newstatus = false;
						}
						else if (text2.Contains("\\OK\\"))
						{
							newDrawpath = "";
							newstatus = true;
						}
						SelectImageWindow selectImageWindow = new SelectImageWindow();
						selectImageWindow.SetMarkImageHide();
						selectImageWindow.SetImage(text, oldDrawpath, oldstatus, text2, newDrawpath, newstatus);
						selectImageWindow.ShowDialog();
						switch (selectImageWindow.GetDeleteResult())
						{
						case 1:
							list4[l] = "";
							break;
						case 2:
							list4[m] = "";
							break;
						}
					}
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIPreviewWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
			return result;
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
	private bool CheckFileList(string okpath, string ngpath)
	{
		try
		{
			TrainCount = 0;
			TRItems.Clear();
			lv_AIResultList.ItemsSource = TRItems;
			FilelistOK = Directory.GetFiles(okpath, "*.bmp", SearchOption.AllDirectories);
			FilelistNG = Directory.GetFiles(ngpath, "*.bmp", SearchOption.AllDirectories);
			if (FilelistOK.Length != 0 && FilelistNG.Length != 0)
			{
				Mat checkImg = CVBuffer.ImRead(FilelistOK[0], ImreadModes.Grayscale).Clone();
				string ocrChar = "";
				inspection.fontTool.GetAIOCRResult(checkImg, ref ocrChar);
				int fontIndex = (cb_FontSelectTest.SelectedIndex = GetFontIndex(ocrChar));
				FontIndex = fontIndex;
				lab_FontSetup.Content = GetFontCharFromIndex(FontIndex);
				return true;
			}
			if (FilelistOK.Length == 0)
			{
				MessageBox.Show("OK影像資料夾找不到任何影像", LanguageManager.Instance["Error"]);
			}
			if (FilelistNG.Length == 0)
			{
				MessageBox.Show("NG影像資料夾找不到任何影像", LanguageManager.Instance["Error"]);
			}
			return false;
		}
		catch (Exception ex)
		{
			string text = "TW01-TrainCSVWindow-CheckFileList()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
			return false;
		}
	}

	private Mat GetFontImageFromIndex(int index)
	{
		return index switch
		{
			0 => inspection.fontTool.FontMatR[0], 
			1 => inspection.fontTool.FontMatM[0], 
			2 => inspection.fontTool.FontMatC[0], 
			3 => inspection.fontTool.FontMatU[0], 
			4 => inspection.fontTool.FontMatDash[0], 
			5 => inspection.fontTool.FontMat0[0], 
			6 => inspection.fontTool.FontMat1[0], 
			7 => inspection.fontTool.FontMat2[0], 
			8 => inspection.fontTool.FontMat3[0], 
			9 => inspection.fontTool.FontMat4[0], 
			10 => inspection.fontTool.FontMat5[0], 
			11 => inspection.fontTool.FontMat6[0], 
			12 => inspection.fontTool.FontMat7[0], 
			13 => inspection.fontTool.FontMat8[0], 
			14 => inspection.fontTool.FontMat9[0], 
			15 => inspection.fontTool.FontMatX[0], 
			16 => inspection.fontTool.FontMatB[0], 
			17 => inspection.fontTool.FontMatE[0], 
			18 => inspection.fontTool.FontMatD[0], 
			19 => inspection.fontTool.FontMatA[0], 
			_ => inspection.fontTool.FontMat0[0], 
		};
	}

	private int GetFontIndex(string str)
	{
		return str switch
		{
			"R" => 0, 
			"M" => 1, 
			"C" => 2, 
			"U" => 3, 
			"-" => 4, 
			"0" => 5, 
			"1" => 6, 
			"2" => 7, 
			"3" => 8, 
			"4" => 9, 
			"5" => 10, 
			"6" => 11, 
			"7" => 12, 
			"8" => 13, 
			"9" => 14, 
			"X" => 15, 
			"B" => 16, 
			"E" => 17, 
			"D" => 18, 
			"A" => 19, 
			_ => 0, 
		};
	}

	private int GetFontAlgorithmCount(int index)
	{
		return index switch
		{
			0 => 15, 
			1 => 8, 
			2 => 7, 
			3 => 7, 
			4 => 3, 
			5 => 16, 
			6 => 3, 
			7 => 9, 
			8 => 12, 
			9 => 9, 
			10 => 11, 
			11 => 8, 
			12 => 5, 
			13 => 15, 
			14 => 8, 
			15 => 7, 
			16 => 9, 
			17 => 7, 
			18 => 14, 
			19 => 7, 
			_ => 0, 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetFontCharFromIndex(int index)
	{
		return index switch
		{
			0 => "R", 
			1 => "M", 
			2 => "C", 
			3 => "U", 
			4 => "-", 
			5 => "0", 
			6 => "1", 
			7 => "2", 
			8 => "3", 
			9 => "4", 
			10 => "5", 
			11 => "6", 
			12 => "7", 
			13 => "8", 
			14 => "9", 
			15 => "X", 
			16 => "B", 
			17 => "E", 
			18 => "D", 
			19 => "A", 
			_ => "", 
		};
	}

	private void SaveFontSetup(string str)
	{
		if (str == null)
		{
			return;
		}
		int length = str.Length;
		if (length == 1)
		{
			switch (str[0])
			{
			case '0':
				SaveCheck_0();
				break;
			case 'R':
				SaveCheck_R();
				break;
			case 'M':
				SaveCheck_M();
				break;
			case 'C':
				SaveCheck_C();
				break;
			case 'U':
				SaveCheck_U();
				break;
			case 'X':
				SaveCheck_X();
				break;
			case 'B':
				SaveCheck_B();
				break;
			case 'D':
				SaveCheck_D();
				break;
			case 'A':
				SaveCheck_A();
				break;
			case 'E':
				SaveCheck_E();
				break;
			case '-':
				SaveCheck_Dash();
				break;
			case '1':
				SaveCheck_1();
				break;
			case '2':
				SaveCheck_2();
				break;
			case '3':
				SaveCheck_3();
				break;
			case '4':
				SaveCheck_4();
				break;
			case '5':
				SaveCheck_5();
				break;
			case '6':
				SaveCheck_6();
				break;
			case '7':
				SaveCheck_7();
				break;
			case '8':
				SaveCheck_8();
				break;
			case '9':
				SaveCheck_9();
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ClearCSVFile()
	{
		string path = string.Format("{0}Training\\FontAITrain\\", AppDomain.CurrentDomain.BaseDirectory);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrainCSV_Click(object sender, RoutedEventArgs e)
	{
		WriteResult = false;
		if (ResultCreated)
		{
			RecoverFontSetup(FontSetupChar);
		}
		FontIndex = cb_FontSelectTest.SelectedIndex;
		FontSetupChar = GetFontCharFromIndex(FontIndex);
		lab_FontSetup.Content = FontSetupChar;
		FilelistOKWrong.Clear();
		FilelistNGWrong.Clear();
		lab_Yelid.Content = "";
		lab_MissYield.Content = "";
		grd_3.Visibility = Visibility.Hidden;
		grd_4.Visibility = Visibility.Hidden;
		grd_Preview.Visibility = Visibility.Hidden;
		btn_SaveTrainResult.Visibility = Visibility.Hidden;
		for (int i = 0; i < 16; i++)
		{
			GrdFontCheck[i].Visibility = Visibility.Hidden;
		}
		grd_3.Visibility = Visibility.Visible;
		lab_Loading.Content = string.Format("字元參數訓練中...");
		grd_Loading.Visibility = Visibility.Visible;
		inspection.fontTool.TrainAIFontParamMode = true;
		TrainCSVTimer.Start();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (!WriteResult)
		{
			if (MessageBox.Show("訓練結果尚未寫入(所有結果不會被儲存)，是否寫入訓練結果? ", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			{
				return;
			}
			RecoverFontSetup(FontSetupChar);
		}
		inspection.fontTool.TrainAIFontParamMode = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupRCheck()
	{
		FontCheckBefore[0].Content = "R-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "R-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "R-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "R-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "R-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "R-06 檢查面積異常";
		FontCheckBefore[6].Content = "R-07 檢查上方寬度異常";
		FontCheckBefore[7].Content = "R-08 檢查下方寬度異常";
		FontCheckBefore[8].Content = "R-09 檢查左邊高度異常";
		FontCheckBefore[9].Content = "R-10 檢查右邊高度異常";
		FontCheckBefore[10].Content = "R-11 檢查中心異常";
		FontCheckBefore[11].Content = "R-12 檢查中心高度異常";
		FontCheckBefore[12].Content = "R-13 檢查左右邊高度比例異常";
		FontCheckBefore[13].Content = "R-14 檢查斷字異常";
		FontCheckBefore[14].Content = "R-15 檢查右下角高度異常";
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.R_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.R_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.R_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.R_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.R_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.R_Area;
		FontCheckBefore[6].IsChecked = backupParam.R_T_W;
		FontCheckBefore[7].IsChecked = backupParam.R_B_W;
		FontCheckBefore[8].IsChecked = backupParam.R_L_H;
		FontCheckBefore[9].IsChecked = backupParam.R_R_H;
		FontCheckBefore[10].IsChecked = backupParam.R_C;
		FontCheckBefore[11].IsChecked = backupParam.R_C_H_MIN;
		FontCheckBefore[12].IsChecked = backupParam.R_LR_H_Diff;
		FontCheckBefore[13].IsChecked = backupParam.R_Break;
		FontCheckBefore[14].IsChecked = backupParam.R_RB_H;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupMCheck()
	{
		FontCheckBefore[0].Content = "M-01 檢查上方寬度異常";
		FontCheckBefore[1].Content = "M-02 檢查下方寬度異常";
		FontCheckBefore[2].Content = "M-03 檢查寬度異常";
		FontCheckBefore[3].Content = "M-04 檢查斷字異常";
		FontCheckBefore[4].Content = "M-05 檢查第1隻腳高度異常";
		FontCheckBefore[5].Content = "M-06 檢查第2隻腳高度異常";
		FontCheckBefore[6].Content = "M-07 檢查第3隻腳高度異常";
		FontCheckBefore[7].Content = "M-08 檢查文字沾黏異常";
		for (int i = 8; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.M_T_W;
		FontCheckBefore[1].IsChecked = backupParam.M_B_W;
		FontCheckBefore[2].IsChecked = backupParam.M_W;
		FontCheckBefore[3].IsChecked = backupParam.M_Break;
		FontCheckBefore[4].IsChecked = backupParam.M_Leg1;
		FontCheckBefore[5].IsChecked = backupParam.M_Leg2;
		FontCheckBefore[6].IsChecked = backupParam.M_Leg3;
		FontCheckBefore[7].IsChecked = backupParam.M_Connected;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupCCheck()
	{
		FontCheckBefore[0].Content = "C-01 檢查寬度異常";
		FontCheckBefore[1].Content = "C-02 檢查高度異常";
		FontCheckBefore[2].Content = "C-03 檢查上方寬度異常";
		FontCheckBefore[3].Content = "C-04 檢查下方寬度異常";
		FontCheckBefore[4].Content = "C-05 檢查左邊高度異常";
		FontCheckBefore[5].Content = "C-06 檢查右邊高度異常";
		FontCheckBefore[6].Content = "C-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.C_W;
		FontCheckBefore[1].IsChecked = backupParam.C_H;
		FontCheckBefore[2].IsChecked = backupParam.C_T_W;
		FontCheckBefore[3].IsChecked = backupParam.C_B_W;
		FontCheckBefore[4].IsChecked = backupParam.C_L_H;
		FontCheckBefore[4].IsChecked = backupParam.C_L_H;
		FontCheckBefore[5].IsChecked = backupParam.C_R_H;
		FontCheckBefore[6].IsChecked = backupParam.C_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupUCheck()
	{
		FontCheckBefore[0].Content = "U-01 檢查寬度異常";
		FontCheckBefore[1].Content = "U-02 檢查高度異常";
		FontCheckBefore[2].Content = "U-03 檢查上方寬度異常";
		FontCheckBefore[3].Content = "U-04 檢查下方寬度異常";
		FontCheckBefore[4].Content = "U-05 檢查左邊高度異常";
		FontCheckBefore[5].Content = "U-06 檢查右邊高度異常";
		FontCheckBefore[6].Content = "U-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.U_W;
		FontCheckBefore[1].IsChecked = backupParam.U_H;
		FontCheckBefore[2].IsChecked = backupParam.U_T_W;
		FontCheckBefore[3].IsChecked = backupParam.U_B_W;
		FontCheckBefore[4].IsChecked = backupParam.U_L_H;
		FontCheckBefore[5].IsChecked = backupParam.U_R_H;
		FontCheckBefore[6].IsChecked = backupParam.U_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupXCheck()
	{
		FontCheckBefore[0].Content = "X-01 檢查寬度異常";
		FontCheckBefore[1].Content = "X-02 檢查高度異常";
		FontCheckBefore[2].Content = "X-03 檢查上方寬度異常";
		FontCheckBefore[3].Content = "X-04 檢查下方寬度異常";
		FontCheckBefore[4].Content = "X-05 檢查左邊高度異常";
		FontCheckBefore[5].Content = "X-06 檢查右邊高度異常";
		FontCheckBefore[6].Content = "X-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.X_W;
		FontCheckBefore[1].IsChecked = backupParam.X_H;
		FontCheckBefore[2].IsChecked = backupParam.X_T_W;
		FontCheckBefore[3].IsChecked = backupParam.X_B_W;
		FontCheckBefore[4].IsChecked = backupParam.X_L_H;
		FontCheckBefore[4].IsChecked = backupParam.X_L_H;
		FontCheckBefore[5].IsChecked = backupParam.X_R_H;
		FontCheckBefore[6].IsChecked = backupParam.X_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupBCheck()
	{
		FontCheckBefore[0].Content = "B-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "B-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "B-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "B-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "B-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "B-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "B-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "B-0B 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "B-09 檢查下方寬度異常";
		FontCheckBefore[9].Content = "B-10 檢查中心異常";
		FontCheckBefore[10].Content = "B-11 檢查中心高度過短";
		FontCheckBefore[11].Content = "B-12 檢查字元中心高度過長";
		FontCheckBefore[12].Content = "B-13 檢查斷字異常";
		for (int i = 13; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.TB_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.TB_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.TB_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.TB_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.TB_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.TB_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.TB_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.TB_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.TB_B_W;
		FontCheckBefore[9].IsChecked = backupParam.TB_C;
		FontCheckBefore[10].IsChecked = backupParam.TB_C_H_MIN;
		FontCheckBefore[11].IsChecked = backupParam.TB_C_H_MAX;
		FontCheckBefore[12].IsChecked = backupParam.TB_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupECheck()
	{
		FontCheckBefore[0].Content = "E-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "E-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "E-0E 檢查右上角高度異常";
		FontCheckBefore[3].Content = "E-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "E-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "E-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "E-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "E-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "E-09 檢查斷字異常";
		for (int i = 9; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.TE_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.TE_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.TE_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.TE_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.TE_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.TE_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.TE_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.TE_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.TE_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupDashCheck()
	{
		FontCheckBefore[0].Content = "DA-01 檢查高度異常";
		FontCheckBefore[1].Content = "DA-02 檢查寬度異常";
		FontCheckBefore[2].Content = "DA-03 檢查斷字異常";
		for (int i = 3; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.Dash_H;
		FontCheckBefore[1].IsChecked = backupParam.Dash_W;
		FontCheckBefore[2].IsChecked = backupParam.Dash_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupDCheck()
	{
		FontCheckBefore[0].Content = "D-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "D-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "D-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "D-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "D-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "D-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "D-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "D-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "D-09 檢查上方寬度異常";
		FontCheckBefore[9].Content = "D-10 檢查下方寬度異常";
		FontCheckBefore[10].Content = "D-11 檢查中心異常";
		FontCheckBefore[11].Content = "D-12 檢查中心高度過短";
		FontCheckBefore[12].Content = "D-13 檢查字元中心高度過長";
		FontCheckBefore[13].Content = "D-14 檢查斷字異常";
		FontCheckBefore[14].Visibility = Visibility.Hidden;
		FontCheckBefore[15].Visibility = Visibility.Hidden;
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.D_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.D_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.D_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.D_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.D_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.D_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.D_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.D_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.D_T_W;
		FontCheckBefore[9].IsChecked = backupParam.D_B_W;
		FontCheckBefore[10].IsChecked = backupParam.D_C;
		FontCheckBefore[11].IsChecked = backupParam.D_C_H_MIN;
		FontCheckBefore[12].IsChecked = backupParam.D_C_H_MAX;
		FontCheckBefore[13].IsChecked = backupParam.D_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupACheck()
	{
		FontCheckBefore[0].Content = "A-01 檢查寬度異常";
		FontCheckBefore[1].Content = "A-02 檢查高度異常";
		FontCheckBefore[2].Content = "A-03 檢查上方寬度異常";
		FontCheckBefore[3].Content = "A-04 檢查下方寬度異常";
		FontCheckBefore[4].Content = "A-05 檢查左邊高度異常";
		FontCheckBefore[5].Content = "A-06 檢查右邊高度異常";
		FontCheckBefore[6].Content = "A-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.A_W;
		FontCheckBefore[1].IsChecked = backupParam.A_H;
		FontCheckBefore[2].IsChecked = backupParam.A_T_W;
		FontCheckBefore[3].IsChecked = backupParam.A_B_W;
		FontCheckBefore[4].IsChecked = backupParam.A_L_H;
		FontCheckBefore[4].IsChecked = backupParam.A_L_H;
		FontCheckBefore[5].IsChecked = backupParam.A_R_H;
		FontCheckBefore[6].IsChecked = backupParam.A_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup0Check()
	{
		FontCheckBefore[0].Content = "0-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "0-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "0-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "0-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "0-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "0-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "0-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "0-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "0-09 檢查上方寬度異常";
		FontCheckBefore[9].Content = "0-10 檢查下方寬度異常";
		FontCheckBefore[10].Content = "0-11 檢查中心異常";
		FontCheckBefore[11].Content = "0-12 檢查中心高度過短";
		FontCheckBefore[12].Content = "0-13 檢查字元中心高度過長";
		FontCheckBefore[13].Content = "0-14 檢查斷字異常";
		FontCheckBefore[14].Content = "0-15 檢查左邊高度異常";
		FontCheckBefore[15].Content = "0-16 檢查右邊高度異常";
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T0_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T0_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T0_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T0_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T0_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.T0_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.T0_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.T0_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.T0_T_W;
		FontCheckBefore[9].IsChecked = backupParam.T0_B_W;
		FontCheckBefore[10].IsChecked = backupParam.T0_C;
		FontCheckBefore[11].IsChecked = backupParam.T0_C_H_MIN;
		FontCheckBefore[12].IsChecked = backupParam.T0_C_H_MAX;
		FontCheckBefore[13].IsChecked = backupParam.T0_Break;
		FontCheckBefore[14].IsChecked = backupParam.T0_L_H;
		FontCheckBefore[15].IsChecked = backupParam.T0_R_H;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup1Check()
	{
		FontCheckBefore[0].Content = "1-01 檢查高度異常";
		FontCheckBefore[1].Content = "1-02 檢查寬度異常";
		FontCheckBefore[2].Content = "1-03 檢查斷字異常";
		for (int i = 3; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T1_H;
		FontCheckBefore[1].IsChecked = backupParam.T1_W;
		FontCheckBefore[2].IsChecked = backupParam.T1_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup2Check()
	{
		FontCheckBefore[0].Content = "2-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "2-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "2-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "2-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "2-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "2-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "2-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "2-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "2-09 檢查斷字異常";
		for (int i = 9; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T2_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T2_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T2_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T2_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T2_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.T2_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.T2_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.T2_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.T2_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup3Check()
	{
		FontCheckBefore[0].Content = "3-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "3-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "3-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "3-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "3-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "3-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "3-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "3-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "3-09 檢查斷字異常";
		FontCheckBefore[9].Content = "3-10 檢查上方寬度異常";
		FontCheckBefore[10].Content = "3-11 檢查下方寬度異常";
		FontCheckBefore[11].Content = "3-12 檢查中心寬度異常";
		for (int i = 12; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T3_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T3_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T3_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T3_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T3_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.T3_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.T3_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.T3_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.T3_Break;
		FontCheckBefore[9].IsChecked = backupParam.T3_T_W;
		FontCheckBefore[10].IsChecked = backupParam.T3_B_W;
		FontCheckBefore[11].IsChecked = backupParam.T3_C_W;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup4Check()
	{
		FontCheckBefore[0].Content = "4-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "4-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "4-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "4-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "4-05 檢查上方寬度異常";
		FontCheckBefore[5].Content = "4-06 檢查下方寬度異常";
		FontCheckBefore[6].Content = "4-07 檢查斷字異常";
		FontCheckBefore[7].Content = "4-08 檢查高度異常";
		FontCheckBefore[8].Content = "4-09 檢查寬度異常";
		for (int i = 9; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T4_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T4_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T4_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T4_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T4_T_W;
		FontCheckBefore[5].IsChecked = backupParam.T4_B_W;
		FontCheckBefore[6].IsChecked = backupParam.T4_Break;
		FontCheckBefore[7].IsChecked = backupParam.T4_H;
		FontCheckBefore[8].IsChecked = backupParam.T4_W;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup5Check()
	{
		FontCheckBefore[0].Content = "5-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "5-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "5-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "5-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "5-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "5-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "5-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "5-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "5-09 檢查斷字異常";
		FontCheckBefore[9].Content = "5-10 檢查上方高度異常";
		FontCheckBefore[10].Content = "5-11 檢查下方高度異常";
		for (int i = 11; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T5_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T5_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T5_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T5_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T5_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.T5_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.T5_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.T5_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.T5_Break;
		FontCheckBefore[9].IsChecked = backupParam.T5_TT_W;
		FontCheckBefore[10].IsChecked = backupParam.T5_TB_W;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup6Check()
	{
		FontCheckBefore[0].Content = "6-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "6-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "6-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "6-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "6-05 檢查上方寬度異常";
		FontCheckBefore[5].Content = "6-06 檢查下方寬度異常";
		FontCheckBefore[6].Content = "6-07 檢查斷字異常";
		FontCheckBefore[7].Content = "6-08 檢查中心異常";
		for (int i = 8; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T6_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T6_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T6_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T6_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T6_T_W;
		FontCheckBefore[5].IsChecked = backupParam.T6_B_W;
		FontCheckBefore[6].IsChecked = backupParam.T6_Break;
		FontCheckBefore[7].IsChecked = backupParam.T6_C;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup7Check()
	{
		FontCheckBefore[0].Content = "7-01 檢查上方寬度異常";
		FontCheckBefore[1].Content = "7-02 檢查下方寬度異常";
		FontCheckBefore[2].Content = "7-03 檢查高度異常";
		FontCheckBefore[3].Content = "7-04 檢查寬度異常";
		FontCheckBefore[4].Content = "7-05 檢查斷字異常";
		for (int i = 5; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T7_T_W;
		FontCheckBefore[1].IsChecked = backupParam.T7_B_W;
		FontCheckBefore[2].IsChecked = backupParam.T7_H;
		FontCheckBefore[3].IsChecked = backupParam.T7_W;
		FontCheckBefore[4].IsChecked = backupParam.T7_Break;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup8Check()
	{
		FontCheckBefore[0].Content = "8-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "8-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "8-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "8-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "8-05 檢查左下角高度異常";
		FontCheckBefore[5].Content = "8-06 檢查左下角寬度異常";
		FontCheckBefore[6].Content = "8-07 檢查右下角高度異常";
		FontCheckBefore[7].Content = "8-08 檢查右下角寬度異常";
		FontCheckBefore[8].Content = "8-09 檢查下方寬度異常";
		FontCheckBefore[9].Content = "8-10 檢查中心異常";
		FontCheckBefore[10].Content = "8-11 檢查中心高度過短";
		FontCheckBefore[11].Content = "8-12 檢查字元中心高度過長";
		FontCheckBefore[12].Content = "8-13 檢查斷字異常";
		FontCheckBefore[13].Content = "8-14 檢查左邊高度異常";
		FontCheckBefore[14].Content = "8-15 檢查右邊高度異常";
		for (int i = 15; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T8_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T8_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T8_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T8_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T8_LB_H;
		FontCheckBefore[5].IsChecked = backupParam.T8_LB_W;
		FontCheckBefore[6].IsChecked = backupParam.T8_RB_H;
		FontCheckBefore[7].IsChecked = backupParam.T8_RB_W;
		FontCheckBefore[8].IsChecked = backupParam.T8_B_W;
		FontCheckBefore[9].IsChecked = backupParam.T8_C;
		FontCheckBefore[10].IsChecked = backupParam.T8_C_H_MIN;
		FontCheckBefore[11].IsChecked = backupParam.T8_C_H_MAX;
		FontCheckBefore[12].IsChecked = backupParam.T8_Break;
		FontCheckBefore[13].IsChecked = backupParam.T8_L_H;
		FontCheckBefore[14].IsChecked = backupParam.T8_R_H;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup9Check()
	{
		FontCheckBefore[0].Content = "9-01 檢查左上角高度異常";
		FontCheckBefore[1].Content = "9-02 檢查左上角寬度異常";
		FontCheckBefore[2].Content = "9-03 檢查右上角高度異常";
		FontCheckBefore[3].Content = "9-04 檢查右上角寬度異常";
		FontCheckBefore[4].Content = "9-05 檢查下方高度異常";
		FontCheckBefore[5].Content = "9-06 檢查下方寬度異常";
		FontCheckBefore[6].Content = "9-07 檢查斷字異常";
		FontCheckBefore[7].Content = "9-08 檢查中心異常";
		for (int i = 8; i < 16; i++)
		{
			FontCheckBefore[i].Content = "";
		}
		INSP_FONTTOOL_PARAM backupParam = BackupParam;
		FontCheckBefore[0].IsChecked = backupParam.T9_LT_H;
		FontCheckBefore[1].IsChecked = backupParam.T9_LT_W;
		FontCheckBefore[2].IsChecked = backupParam.T9_RT_H;
		FontCheckBefore[3].IsChecked = backupParam.T9_RT_W;
		FontCheckBefore[4].IsChecked = backupParam.T9_B_H;
		FontCheckBefore[5].IsChecked = backupParam.T9_B_W;
		FontCheckBefore[6].IsChecked = backupParam.T9_Break;
		FontCheckBefore[7].IsChecked = backupParam.T9_C;
	}

	private void SaveCheck_R()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.R_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.R_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.R_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.R_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.R_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.R_Area = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.R_T_W = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.R_B_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.R_L_H = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.R_R_H = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.R_C = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.R_C_H_MIN = FontCheckAfter[11].IsChecked.Value;
		fontToolParam.R_LR_H_Diff = FontCheckAfter[12].IsChecked.Value;
		fontToolParam.R_Break = FontCheckAfter[13].IsChecked.Value;
		fontToolParam.R_RB_H = FontCheckAfter[14].IsChecked.Value;
		if (inspection.fontTool.FontUsePara[0] && FontCheckAfter[0].IsChecked.Value)
		{
			fontToolParam.R_LT_H_P = TrainResultMin[0];
		}
		if (inspection.fontTool.FontUsePara[1] && FontCheckAfter[1].IsChecked.Value)
		{
			fontToolParam.R_LT_W_P = TrainResultMin[1];
		}
		if (inspection.fontTool.FontUsePara[2] && FontCheckAfter[2].IsChecked.Value)
		{
			fontToolParam.R_RT_H_P = TrainResultMin[2];
		}
		if (inspection.fontTool.FontUsePara[3] && FontCheckAfter[3].IsChecked.Value)
		{
			fontToolParam.R_RT_W_P = TrainResultMin[3];
		}
		if (inspection.fontTool.FontUsePara[4] && FontCheckAfter[4].IsChecked.Value)
		{
			fontToolParam.R_LB_H_P = TrainResultMin[4];
		}
		if (inspection.fontTool.FontUsePara[5] && FontCheckAfter[5].IsChecked.Value)
		{
			fontToolParam.R_Area_P = TrainResultMin[5];
		}
		if (inspection.fontTool.FontUsePara[6] && FontCheckAfter[6].IsChecked.Value)
		{
			fontToolParam.R_T_W_P = TrainResultMin[6];
		}
		if (inspection.fontTool.FontUsePara[7] && FontCheckAfter[7].IsChecked.Value)
		{
			fontToolParam.R_B_W_P = TrainResultMax[7];
		}
		if (inspection.fontTool.FontUsePara[8] && FontCheckAfter[8].IsChecked.Value)
		{
			fontToolParam.R_L_H_P = TrainResultMin[8];
		}
		if (inspection.fontTool.FontUsePara[9] && FontCheckAfter[9].IsChecked.Value)
		{
			fontToolParam.R_R_H_P = TrainResultMin[9];
		}
		if (inspection.fontTool.FontUsePara[11] && FontCheckAfter[11].IsChecked.Value)
		{
			fontToolParam.R_C_H_MIN_P = TrainResultMin[11];
			fontToolParam.R_C_H_MAX_P = TrainResultMax[11];
		}
		if (inspection.fontTool.FontUsePara[12] && FontCheckAfter[12].IsChecked.Value)
		{
			fontToolParam.R_LR_H_Diff_P = TrainResultMin[12];
		}
		if (inspection.fontTool.FontUsePara[13] && FontCheckAfter[13].IsChecked.Value)
		{
			fontToolParam.R_Break_PMin = TrainResultMin[13];
			fontToolParam.R_Break_PMax = TrainResultMax[13];
		}
		if (inspection.fontTool.FontUsePara[14] && FontCheckAfter[14].IsChecked.Value)
		{
			fontToolParam.R_RB_H_P = TrainResultMin[14];
		}
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_M()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.M_T_W = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.M_B_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.M_W = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.M_Break = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.M_Leg1 = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.M_Leg2 = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.M_Leg3 = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.M_Connected = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_C()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.C_W = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.C_H = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.C_T_W = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.C_B_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.C_L_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.C_R_H = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.C_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_U()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.U_W = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.U_H = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.U_T_W = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.U_B_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.U_L_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.U_R_H = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.U_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_X()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.X_W = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.X_H = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.X_T_W = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.X_B_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.X_L_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.X_R_H = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.X_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_B()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.TB_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.TB_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.TB_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.TB_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.TB_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.TB_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.TB_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.TB_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.TB_B_W = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.TB_C = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.TB_C_H_MIN = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.TB_C_H_MAX = FontCheckAfter[11].IsChecked.Value;
		fontToolParam.TB_Break = FontCheckAfter[12].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_E()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.TE_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.TE_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.TE_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.TE_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.TE_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.TE_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.TE_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.TE_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.TE_Break = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_Dash()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.Dash_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.Dash_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.Dash_Break = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_D()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.D_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.D_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.D_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.D_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.D_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.D_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.D_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.D_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.D_T_W = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.D_B_W = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.D_C = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.D_C_H_MIN = FontCheckAfter[11].IsChecked.Value;
		fontToolParam.D_C_H_MAX = FontCheckAfter[12].IsChecked.Value;
		fontToolParam.D_Break = FontCheckAfter[13].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_A()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.A_W = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.A_H = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.A_T_W = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.A_B_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.A_L_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.A_R_H = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.A_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_0()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T0_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T0_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T0_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T0_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T0_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T0_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T0_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T0_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.T0_T_W = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.T0_B_W = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.T0_C = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.T0_C_H_MIN = FontCheckAfter[11].IsChecked.Value;
		fontToolParam.T0_C_H_MAX = FontCheckAfter[12].IsChecked.Value;
		fontToolParam.T0_Break = FontCheckAfter[13].IsChecked.Value;
		fontToolParam.T0_L_H = FontCheckAfter[14].IsChecked.Value;
		fontToolParam.T0_R_H = FontCheckAfter[15].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_1()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T1_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T1_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T1_Break = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_2()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T2_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T2_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T2_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T2_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T2_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T2_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T2_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T2_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.T2_Break = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_3()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T3_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T3_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T3_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T3_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T3_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T3_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T3_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T3_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.T3_Break = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.T3_T_W = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.T3_B_W = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.T3_C_W = FontCheckAfter[11].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_4()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T4_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T4_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T4_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T4_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T4_T_W = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T4_B_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T4_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T4_H = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.T4_W = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_5()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T5_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T5_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T5_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T5_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T5_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T5_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T5_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T5_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.T5_Break = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.T5_TT_W = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.T5_TB_W = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_6()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T6_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T6_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T6_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T6_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T6_T_W = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T6_B_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T6_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T6_C = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_7()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T7_T_W = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T7_B_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T7_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T7_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T7_Break = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_8()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T8_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T8_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T8_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T8_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T8_LB_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T8_LB_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T8_RB_H = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T8_RB_W = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.T8_B_W = FontCheckAfter[8].IsChecked.Value;
		fontToolParam.T8_C = FontCheckAfter[9].IsChecked.Value;
		fontToolParam.T8_C_H_MIN = FontCheckAfter[10].IsChecked.Value;
		fontToolParam.T8_C_H_MAX = FontCheckAfter[11].IsChecked.Value;
		fontToolParam.T8_Break = FontCheckAfter[12].IsChecked.Value;
		fontToolParam.T8_L_H = FontCheckAfter[14].IsChecked.Value;
		fontToolParam.T8_R_H = FontCheckAfter[15].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_9()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T9_LT_H = FontCheckAfter[0].IsChecked.Value;
		fontToolParam.T9_LT_W = FontCheckAfter[1].IsChecked.Value;
		fontToolParam.T9_RT_H = FontCheckAfter[2].IsChecked.Value;
		fontToolParam.T9_RT_W = FontCheckAfter[3].IsChecked.Value;
		fontToolParam.T9_B_H = FontCheckAfter[4].IsChecked.Value;
		fontToolParam.T9_B_W = FontCheckAfter[5].IsChecked.Value;
		fontToolParam.T9_Break = FontCheckAfter[6].IsChecked.Value;
		fontToolParam.T9_C = FontCheckAfter[7].IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NGWrong_Click(object sender, RoutedEventArgs e)
	{
		string.Format("{0}\\OK_wrong\\", SelectedPath);
		string currentTestPath = string.Format("{0}\\NG_wrong\\", SelectedPath);
		if (FilelistNGWrong.Count <= 0)
		{
			return;
		}
		try
		{
			CurrentTestPath = currentTestPath;
			ReadBinarySetting();
			FileListTest = null;
			FileListTest = new string[FilelistNGWrong.Count];
			for (int i = 0; i < FilelistNGWrong.Count; i++)
			{
				FileListTest[i] = FilelistNGWrong[i];
			}
			FileListTestIndex = 0;
			grd_FontTest.Visibility = Visibility.Visible;
			if (FileListTest.Length != 0)
			{
				txt_FontCount.Text = string.Format("{0} / {1}", FileListTestIndex + 1, FileListTest.Length);
				lab_FontTestSet.Content = string.Format("NG漏判 ({0})", FileListTest.Length);
				if (File.Exists(FileListTest[FileListTestIndex]))
				{
					Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
					TestFontInspect(mat);
				}
			}
			else
			{
				txt_FontCount.Text = "0 / 0";
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TrainCSVWindow-Btn_NGWrong_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OKWrong_Click(object sender, RoutedEventArgs e)
	{
		string currentTestPath = string.Format("{0}\\OK_wrong\\", SelectedPath);
		string.Format("{0}\\NG_wrong\\", SelectedPath);
		try
		{
			CurrentTestPath = currentTestPath;
			ReadBinarySetting();
			FileListTest = null;
			FileListTest = new string[FilelistOKWrong.Count];
			for (int i = 0; i < FilelistOKWrong.Count; i++)
			{
				FileListTest[i] = FilelistOKWrong[i];
			}
			FileListTestIndex = 0;
			grd_FontTest.Visibility = Visibility.Visible;
			if (FileListTest.Length != 0)
			{
				txt_FontCount.Text = string.Format("{0} / {1}", FileListTestIndex + 1, FileListTest.Length);
				lab_FontTestSet.Content = string.Format("OK誤判 ({0})", FileListTest.Length);
				if (File.Exists(FileListTest[FileListTestIndex]))
				{
					Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
					TestFontInspect(mat);
				}
			}
			else
			{
				txt_FontCount.Text = "0 / 0";
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TrainCSVWindow-Btn_OKWrong_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OKImage_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (TrainOKPath == "")
			{
				return;
			}
			if (!Directory.Exists(TrainOKPath))
			{
				Directory.CreateDirectory(TrainOKPath);
			}
			if (!Directory.Exists(TrainOKPath))
			{
				return;
			}
			CurrentTestPath = TrainOKPath;
			ReadBinarySetting();
			FileListTest = Directory.GetFiles(TrainOKPath, "*.bmp", SearchOption.AllDirectories);
			FileListTestIndex = 0;
			grd_FontTest.Visibility = Visibility.Visible;
			if (FileListTest.Length != 0)
			{
				txt_FontCount.Text = string.Format("{0} / {1}", FileListTestIndex + 1, FileListTest.Length);
				lab_FontTestSet.Content = string.Format("OK ({0})", FileListTest.Length);
				if (File.Exists(FileListTest[FileListTestIndex]))
				{
					Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
					TestFontInspect(mat);
				}
			}
			else
			{
				txt_FontCount.Text = "0 / 0";
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TrainCSVWindow-Btn_OKImage_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NGImage_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (TrainNGPath == "")
			{
				return;
			}
			if (!Directory.Exists(TrainNGPath))
			{
				Directory.CreateDirectory(TrainNGPath);
			}
			if (!Directory.Exists(TrainNGPath))
			{
				return;
			}
			CurrentTestPath = TrainNGPath;
			ReadBinarySetting();
			FileListTest = Directory.GetFiles(TrainNGPath, "*.bmp", SearchOption.AllDirectories);
			FileListTestIndex = 0;
			grd_FontTest.Visibility = Visibility.Visible;
			if (FileListTest.Length != 0)
			{
				txt_FontCount.Text = string.Format("{0} / {1}", FileListTestIndex + 1, FileListTest.Length);
				lab_FontTestSet.Content = string.Format("NG ({0})", FileListTest.Length);
				if (File.Exists(FileListTest[FileListTestIndex]))
				{
					Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
					TestFontInspect(mat);
				}
			}
			else
			{
				txt_FontCount.Text = "0 / 0";
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TrainCSVWindow-Btn_NGImage_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SaveTrainResult_Click(object sender, RoutedEventArgs e)
	{
		SaveFontSetup(FontSetupChar);
		MessageBox.Show("參數寫入成功");
		WriteResult = true;
		ResetUI();
		BackupParam = inspection.paramFile.FontToolParam;
	}

	private void ResetUI()
	{
		grd_2.Visibility = Visibility.Hidden;
		grd_3.Visibility = Visibility.Hidden;
		grd_4.Visibility = Visibility.Hidden;
		grd_Preview.Visibility = Visibility.Hidden;
		btn_SaveTrainResult.Visibility = Visibility.Hidden;
		grd_Image.Visibility = Visibility.Hidden;
		for (int i = 0; i < 16; i++)
		{
			GrdFontCheck[i].Visibility = Visibility.Hidden;
		}
		FilelistOK = null;
		FilelistNG = null;
		FilelistOKWrong.Clear();
		FilelistNGWrong.Clear();
		SelectedPath = "";
		TrainOKPath = "";
		TrainNGPath = "";
		TrainResultList.Clear();
		lab_Yelid.Content = "";
		lab_MissYield.Content = "";
		ResultCreated = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Window_Drop(object sender, DragEventArgs e)
	{
		if (IsBusy)
		{
			MessageBox.Show("系統訓練中...", "警告");
			return;
		}
		string loading = lab_Loading.Content.ToString();
		Action method = [MethodImpl(MethodImplOptions.NoInlining)] () =>
		{
			grd_Loading.Visibility = Visibility.Visible;
			lab_Loading.Content = string.Format("載入影像中");
		};
		base.Dispatcher.BeginInvoke(method);
		base.Dispatcher.BeginInvoke((Action)([MethodImpl(MethodImplOptions.NoInlining)] () =>
		{
			string[] array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
			if (array.Length == 0)
			{
				grd_Loading.Visibility = Visibility.Hidden;
				lab_Loading.Content = loading;
			}
			else
			{
				string text = array[0];
				if (!string.IsNullOrEmpty(text) && File.Exists(text))
				{
					Path.GetExtension(text);
					grd_Loading.Visibility = Visibility.Hidden;
					lab_Loading.Content = loading;
				}
				else
				{
					ResetUI();
					TrainOKPath = string.Format("{0}\\OK\\", text);
					TrainNGPath = string.Format("{0}\\NG\\", text);
					if (Directory.Exists(TrainOKPath) && Directory.Exists(TrainNGPath))
					{
						try
						{
							SelectedPath = text;
							string path = string.Format("{0}Training\\FontAITrain\\", AppDomain.CurrentDomain.BaseDirectory);
							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}
							if (CheckFileList(TrainOKPath, TrainNGPath))
							{
								grd_2.Visibility = Visibility.Visible;
							}
							FindSameImage(FilelistOK, FilelistNG);
							grd_Image.Visibility = Visibility.Visible;
							btn_OKImage.Content = string.Format("OK ({0})", FilelistOK.Length);
							btn_NGImage.Content = string.Format("NG ({0})", FilelistNG.Length);
							grd_Loading.Visibility = Visibility.Hidden;
							lab_Loading.Content = loading;
							return;
						}
						catch (Exception ex)
						{
							string text2 = "FTW01-TrainCSVWindow-Window_Drop()" + ex.ToString();
							CVBuffer.SaveLog(text2);
							MessageBox.Show(text2, "錯誤");
							grd_Loading.Visibility = Visibility.Hidden;
							lab_Loading.Content = loading;
							return;
						}
					}
					TrainOKPath = (TrainNGPath = "");
					grd_Loading.Visibility = Visibility.Hidden;
					lab_Loading.Content = loading;
					MessageBox.Show("路徑找不到OK及NG資料夾。", "警告");
				}
			}
		}));
	}

	private void Cb_ShowTrainWnd_Click(object sender, RoutedEventArgs e)
	{
		HideTrainingWnd = (!cb_ShowTrainWnd.IsChecked).Value;
	}

	public void RecoverFontSetup(string str)
	{
		if (str == null)
		{
			return;
		}
		int length = str.Length;
		if (length == 1)
		{
			switch (str[0])
			{
			case '0':
				Recover_0();
				break;
			case 'R':
				Recover_R();
				break;
			case 'M':
				Recover_M();
				break;
			case 'C':
				Recover_C();
				break;
			case 'X':
				Recover_X();
				break;
			case 'B':
				Recover_B();
				break;
			case 'D':
				Recover_D();
				break;
			case 'A':
				Recover_A();
				break;
			case 'E':
				Recover_E();
				break;
			case '-':
				Recover_Dash();
				break;
			case '1':
				Recover_1();
				break;
			case '2':
				Recover_2();
				break;
			case '3':
				Recover_3();
				break;
			case '4':
				Recover_4();
				break;
			case '5':
				Recover_5();
				break;
			case '6':
				Recover_6();
				break;
			case '7':
				Recover_7();
				break;
			case '8':
				Recover_8();
				break;
			case '9':
				Recover_9();
				break;
			}
		}
	}

	private void Recover_R()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.R_LT_H = fontToolParam2.R_LT_H;
		fontToolParam.R_LT_W = fontToolParam2.R_LT_W;
		fontToolParam.R_RT_H = fontToolParam2.R_RT_H;
		fontToolParam.R_RT_W = fontToolParam2.R_RT_W;
		fontToolParam.R_LB_H = fontToolParam2.R_LB_H;
		fontToolParam.R_Area = fontToolParam2.R_Area;
		fontToolParam.R_T_W = fontToolParam2.R_T_W;
		fontToolParam.R_B_W = fontToolParam2.R_B_W;
		fontToolParam.R_L_H = fontToolParam2.R_L_H;
		fontToolParam.R_R_H = fontToolParam2.R_R_H;
		fontToolParam.R_C = fontToolParam2.R_C;
		fontToolParam.R_C_H_MIN = fontToolParam2.R_C_H_MIN;
		fontToolParam.R_LR_H_Diff = fontToolParam2.R_LR_H_Diff;
		fontToolParam.R_Break = fontToolParam2.R_Break;
		fontToolParam.R_RB_H = fontToolParam2.R_RB_H;
		fontToolParam.R_LT_H_P = fontToolParam2.R_LT_H_P;
		fontToolParam.R_LT_W_P = fontToolParam2.R_LT_W_P;
		fontToolParam.R_RT_H_P = fontToolParam2.R_RT_H_P;
		fontToolParam.R_RT_W_P = fontToolParam2.R_RT_W_P;
		fontToolParam.R_LB_H_P = fontToolParam2.R_LB_H_P;
		fontToolParam.R_Area_P = fontToolParam2.R_Area_P;
		fontToolParam.R_T_W_P = fontToolParam2.R_T_W_P;
		fontToolParam.R_B_W_P = fontToolParam2.R_B_W_P;
		fontToolParam.R_L_H_P = fontToolParam2.R_L_H_P;
		fontToolParam.R_R_H_P = fontToolParam2.R_R_H_P;
		fontToolParam.R_C_P = fontToolParam2.R_C_P;
		fontToolParam.R_LR_H_Diff_P = fontToolParam2.R_LR_H_Diff_P;
		fontToolParam.R_C_H_MIN_P = fontToolParam2.R_C_H_MIN_P;
		fontToolParam.R_C_H_MAX_P = fontToolParam2.R_C_H_MAX_P;
		fontToolParam.R_Break_PMin = fontToolParam2.R_Break_PMin;
		fontToolParam.R_Break_PMax = fontToolParam2.R_Break_PMax;
		fontToolParam.R_RB_H_P = fontToolParam2.R_RB_H_P;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_M()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.M_T_W = fontToolParam2.M_T_W;
		fontToolParam.M_B_W = fontToolParam2.M_B_W;
		fontToolParam.M_W = fontToolParam2.M_W;
		fontToolParam.M_Break = fontToolParam2.M_Break;
		fontToolParam.M_Leg1 = fontToolParam2.M_Leg1;
		fontToolParam.M_Leg2 = fontToolParam2.M_Leg2;
		fontToolParam.M_Leg3 = fontToolParam2.M_Leg3;
		fontToolParam.M_Connected = fontToolParam2.M_Connected;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_C()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.C_W = fontToolParam2.C_W;
		fontToolParam.C_H = fontToolParam2.C_H;
		fontToolParam.C_T_W = fontToolParam2.C_T_W;
		fontToolParam.C_B_W = fontToolParam2.C_B_W;
		fontToolParam.C_L_H = fontToolParam2.C_L_H;
		fontToolParam.C_R_H = fontToolParam2.C_R_H;
		fontToolParam.C_Break = fontToolParam2.C_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_U()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.U_W = fontToolParam2.U_W;
		fontToolParam.U_H = fontToolParam2.U_H;
		fontToolParam.U_T_W = fontToolParam2.U_T_W;
		fontToolParam.U_B_W = fontToolParam2.U_B_W;
		fontToolParam.U_L_H = fontToolParam2.U_L_H;
		fontToolParam.U_R_H = fontToolParam2.U_R_H;
		fontToolParam.U_Break = fontToolParam2.U_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_X()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.X_W = fontToolParam2.X_W;
		fontToolParam.X_H = fontToolParam2.X_H;
		fontToolParam.X_T_W = fontToolParam2.X_T_W;
		fontToolParam.X_B_W = fontToolParam2.X_B_W;
		fontToolParam.X_L_H = fontToolParam2.X_L_H;
		fontToolParam.X_R_H = fontToolParam2.X_R_H;
		fontToolParam.X_Break = fontToolParam2.X_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_B()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.TB_LT_H = fontToolParam2.TB_LT_H;
		fontToolParam.TB_LT_W = fontToolParam2.TB_LT_W;
		fontToolParam.TB_RT_H = fontToolParam2.TB_RT_H;
		fontToolParam.TB_RT_W = fontToolParam2.TB_RT_W;
		fontToolParam.TB_LB_H = fontToolParam2.TB_LB_H;
		fontToolParam.TB_LB_W = fontToolParam2.TB_LB_W;
		fontToolParam.TB_RB_H = fontToolParam2.TB_RB_H;
		fontToolParam.TB_RB_W = fontToolParam2.TB_RB_W;
		fontToolParam.TB_B_W = fontToolParam2.TB_B_W;
		fontToolParam.TB_C = fontToolParam2.TB_C;
		fontToolParam.TB_C_H_MIN = fontToolParam2.TB_C_H_MIN;
		fontToolParam.TB_C_H_MAX = fontToolParam2.TB_C_H_MAX;
		fontToolParam.TB_Break = fontToolParam2.TB_Break;
		fontToolParam.TB_BinaryShift = fontToolParam2.TB_BinaryShift;
		fontToolParam.TB_OpenClose = fontToolParam2.TB_OpenClose;
		fontToolParam.TB_OpenCloseCount = fontToolParam2.TB_OpenCloseCount;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_E()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.TE_LT_H = fontToolParam2.TE_LT_H;
		fontToolParam.TE_LT_W = fontToolParam2.TE_LT_W;
		fontToolParam.TE_RT_H = fontToolParam2.TE_RT_H;
		fontToolParam.TE_RT_W = fontToolParam2.TE_RT_W;
		fontToolParam.TE_LB_H = fontToolParam2.TE_LB_H;
		fontToolParam.TE_LB_W = fontToolParam2.TE_LB_W;
		fontToolParam.TE_RB_H = fontToolParam2.TE_RB_H;
		fontToolParam.TE_RB_W = fontToolParam2.TE_RB_W;
		fontToolParam.TE_Break = fontToolParam2.TE_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_Dash()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.Dash_H = fontToolParam2.Dash_H;
		fontToolParam.Dash_W = fontToolParam2.Dash_W;
		fontToolParam.Dash_Break = fontToolParam2.Dash_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_D()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.D_LT_H = fontToolParam2.D_LT_H;
		fontToolParam.D_LT_W = fontToolParam2.D_LT_W;
		fontToolParam.D_RT_H = fontToolParam2.D_RT_H;
		fontToolParam.D_RT_W = fontToolParam2.D_RT_W;
		fontToolParam.D_LB_H = fontToolParam2.D_LB_H;
		fontToolParam.D_LB_W = fontToolParam2.D_LB_W;
		fontToolParam.D_RB_H = fontToolParam2.D_RB_H;
		fontToolParam.D_RB_W = fontToolParam2.D_RB_W;
		fontToolParam.D_T_W = fontToolParam2.D_T_W;
		fontToolParam.D_B_W = fontToolParam2.D_B_W;
		fontToolParam.D_C = fontToolParam2.D_C;
		fontToolParam.D_C_H_MIN = fontToolParam2.D_C_H_MIN;
		fontToolParam.D_C_H_MAX = fontToolParam2.D_C_H_MAX;
		fontToolParam.D_Break = fontToolParam2.D_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_A()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.A_W = fontToolParam2.A_W;
		fontToolParam.A_H = fontToolParam2.A_H;
		fontToolParam.A_T_W = fontToolParam2.A_T_W;
		fontToolParam.A_B_W = fontToolParam2.A_B_W;
		fontToolParam.A_L_H = fontToolParam2.A_L_H;
		fontToolParam.A_R_H = fontToolParam2.A_R_H;
		fontToolParam.A_Break = fontToolParam2.A_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_0()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T0_LT_H = fontToolParam2.T0_LT_H;
		fontToolParam.T0_LT_W = fontToolParam2.T0_LT_W;
		fontToolParam.T0_RT_H = fontToolParam2.T0_RT_H;
		fontToolParam.T0_RT_W = fontToolParam2.T0_RT_W;
		fontToolParam.T0_LB_H = fontToolParam2.T0_LB_H;
		fontToolParam.T0_LB_W = fontToolParam2.T0_LB_W;
		fontToolParam.T0_RB_H = fontToolParam2.T0_RB_H;
		fontToolParam.T0_RB_W = fontToolParam2.T0_RB_W;
		fontToolParam.T0_T_W = fontToolParam2.T0_T_W;
		fontToolParam.T0_B_W = fontToolParam2.T0_B_W;
		fontToolParam.T0_C = fontToolParam2.T0_C;
		fontToolParam.T0_C_H_MIN = fontToolParam2.T0_C_H_MIN;
		fontToolParam.T0_C_H_MAX = fontToolParam2.T0_C_H_MAX;
		fontToolParam.T0_Break = fontToolParam2.T0_Break;
		fontToolParam.T0_L_H = fontToolParam2.T0_L_H;
		fontToolParam.T0_R_H = fontToolParam2.T0_R_H;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_1()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T1_H = fontToolParam2.T1_H;
		fontToolParam.T1_W = fontToolParam2.T1_W;
		fontToolParam.T1_Break = fontToolParam2.T1_Break;
		fontToolParam.T1_BinaryShift = fontToolParam2.T1_BinaryShift;
		fontToolParam.T1_OpenClose = fontToolParam2.T1_OpenClose;
		fontToolParam.T1_OpenCloseCount = fontToolParam2.T1_OpenCloseCount;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_2()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T2_LT_H = fontToolParam2.T2_LT_H;
		fontToolParam.T2_LT_W = fontToolParam2.T2_LT_W;
		fontToolParam.T2_RT_H = fontToolParam2.T2_RT_H;
		fontToolParam.T2_RT_W = fontToolParam2.T2_RT_W;
		fontToolParam.T2_LB_H = fontToolParam2.T2_LB_H;
		fontToolParam.T2_LB_W = fontToolParam2.T2_LB_W;
		fontToolParam.T2_RB_H = fontToolParam2.T2_RB_H;
		fontToolParam.T2_RB_W = fontToolParam2.T2_RB_W;
		fontToolParam.T2_Break = fontToolParam2.T2_Break;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_3()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T3_LT_H = fontToolParam2.T3_LT_H;
		fontToolParam.T3_LT_W = fontToolParam2.T3_LT_W;
		fontToolParam.T3_RT_H = fontToolParam2.T3_RT_H;
		fontToolParam.T3_RT_W = fontToolParam2.T3_RT_W;
		fontToolParam.T3_LB_H = fontToolParam2.T3_LB_H;
		fontToolParam.T3_LB_W = fontToolParam2.T3_LB_W;
		fontToolParam.T3_RB_H = fontToolParam2.T3_RB_H;
		fontToolParam.T3_RB_W = fontToolParam2.T3_RB_W;
		fontToolParam.T3_Break = fontToolParam2.T3_Break;
		fontToolParam.T3_T_W = fontToolParam2.T3_T_W;
		fontToolParam.T3_B_W = fontToolParam2.T3_B_W;
		fontToolParam.T3_C_W = fontToolParam2.T3_C_W;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_4()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T4_LT_H = fontToolParam2.T4_LT_H;
		fontToolParam.T4_LT_W = fontToolParam2.T4_LT_W;
		fontToolParam.T4_RT_H = fontToolParam2.T4_RT_H;
		fontToolParam.T4_RT_W = fontToolParam2.T4_RT_W;
		fontToolParam.T4_T_W = fontToolParam2.T4_T_W;
		fontToolParam.T4_B_W = fontToolParam2.T4_B_W;
		fontToolParam.T4_Break = fontToolParam2.T4_Break;
		fontToolParam.T4_Break = fontToolParam2.T4_Break;
		fontToolParam.T4_W = fontToolParam2.T4_W;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_5()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T5_LT_H = fontToolParam2.T5_LT_H;
		fontToolParam.T5_LT_W = fontToolParam2.T5_LT_W;
		fontToolParam.T5_RT_H = fontToolParam2.T5_RT_H;
		fontToolParam.T5_RT_W = fontToolParam2.T5_RT_W;
		fontToolParam.T5_LB_H = fontToolParam2.T5_LB_H;
		fontToolParam.T5_LB_W = fontToolParam2.T5_LB_W;
		fontToolParam.T5_RB_H = fontToolParam2.T5_RB_H;
		fontToolParam.T5_RB_W = fontToolParam2.T5_RB_W;
		fontToolParam.T5_Break = fontToolParam2.T5_Break;
		fontToolParam.T5_TT_W = fontToolParam2.T5_TT_W;
		fontToolParam.T5_TB_W = fontToolParam2.T5_TB_W;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_6()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile_Backup.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile.FontToolParam;
		fontToolParam2.T6_LT_H = fontToolParam.T6_LT_H;
		fontToolParam2.T6_LT_W = fontToolParam.T6_LT_W;
		fontToolParam2.T6_RT_H = fontToolParam.T6_RT_H;
		fontToolParam2.T6_RT_W = fontToolParam.T6_RT_W;
		fontToolParam2.T6_T_W = fontToolParam.T6_T_W;
		fontToolParam2.T6_B_W = fontToolParam.T6_B_W;
		fontToolParam2.T6_Break = fontToolParam.T6_Break;
		fontToolParam2.T6_C = fontToolParam.T6_C;
		fontToolParam2.T6_BinaryShift = fontToolParam.T6_BinaryShift;
		fontToolParam2.T6_OpenClose = fontToolParam.T6_OpenClose;
		fontToolParam2.T6_OpenCloseCount = fontToolParam.T6_OpenCloseCount;
		fontToolParam2.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam2;
	}

	private void Recover_7()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T7_T_W = fontToolParam2.T7_T_W;
		fontToolParam.T7_B_W = fontToolParam2.T7_B_W;
		fontToolParam.T7_H = fontToolParam2.T7_H;
		fontToolParam.T7_W = fontToolParam2.T7_W;
		fontToolParam.T7_Break = fontToolParam2.T7_Break;
		fontToolParam.T7_BinaryShift = fontToolParam2.T7_BinaryShift;
		fontToolParam.T7_OpenClose = fontToolParam2.T7_OpenClose;
		fontToolParam.T7_OpenCloseCount = fontToolParam2.T7_OpenCloseCount;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_8()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T8_LT_H = fontToolParam2.T8_LT_H;
		fontToolParam.T8_LT_W = fontToolParam2.T8_LT_W;
		fontToolParam.T8_RT_H = fontToolParam2.T8_RT_H;
		fontToolParam.T8_RT_W = fontToolParam2.T8_RT_W;
		fontToolParam.T8_LB_H = fontToolParam2.T8_LB_H;
		fontToolParam.T8_LB_W = fontToolParam2.T8_LB_W;
		fontToolParam.T8_RB_H = fontToolParam2.T8_RB_H;
		fontToolParam.T8_RB_W = fontToolParam2.T8_RB_W;
		fontToolParam.T8_B_W = fontToolParam2.T8_B_W;
		fontToolParam.T8_C = fontToolParam2.T8_C;
		fontToolParam.T8_C_H_MIN = fontToolParam2.T8_C_H_MIN;
		fontToolParam.T8_C_H_MAX = fontToolParam2.T8_C_H_MAX;
		fontToolParam.T8_Break = fontToolParam2.T8_Break;
		fontToolParam.T8_L_H = fontToolParam2.T8_L_H;
		fontToolParam.T8_R_H = fontToolParam2.T8_R_H;
		fontToolParam.T8_BinaryShift = fontToolParam2.T8_BinaryShift;
		fontToolParam.T8_OpenClose = fontToolParam2.T8_OpenClose;
		fontToolParam.T8_OpenCloseCount = fontToolParam2.T8_OpenCloseCount;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Recover_9()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.T9_LT_H = fontToolParam2.T9_LT_H;
		fontToolParam.T9_LT_W = fontToolParam2.T9_LT_W;
		fontToolParam.T9_RT_H = fontToolParam2.T9_RT_H;
		fontToolParam.T9_RT_W = fontToolParam2.T9_RT_W;
		fontToolParam.T9_B_H = fontToolParam2.T9_B_H;
		fontToolParam.T9_B_W = fontToolParam2.T9_B_W;
		fontToolParam.T9_Break = fontToolParam2.T9_Break;
		fontToolParam.T9_C = fontToolParam2.T9_C;
		fontToolParam.T9_BinaryShift = fontToolParam2.T9_BinaryShift;
		fontToolParam.T9_OpenClose = fontToolParam2.T9_OpenClose;
		fontToolParam.T9_OpenCloseCount = fontToolParam2.T9_OpenCloseCount;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Sl_BinaryShift_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (inited && grd_FontTest.Visibility != Visibility.Hidden)
		{
			WriteBinarySetting();
			Btn_FontTest_Click_1(null, null);
		}
	}

	private void Cb_OpenClose_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && grd_FontTest.Visibility != Visibility.Hidden)
		{
			WriteBinarySetting();
			Btn_FontTest_Click_1(null, null);
		}
	}

	private void Btn_FontTest_Click_1(object sender, RoutedEventArgs e)
	{
		if (File.Exists(FileListTest[FileListTestIndex]))
		{
			Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
			TestFontInspect(mat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenFolder_Click(object sender, RoutedEventArgs e)
	{
		if (!(CurrentTestPath == "") && Directory.Exists(CurrentTestPath))
		{
			string fileName = "c:\\windows\\explorer.exe";
			string arguments = string.Format("/root, {0}", CurrentTestPath);
			Process.Start(fileName, arguments);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_next_Click(object sender, RoutedEventArgs e)
	{
		if (FileListTest.Length != 0)
		{
			FileListTestIndex++;
			if (FileListTestIndex >= FileListTest.Length)
			{
				FileListTestIndex = 0;
			}
			if (File.Exists(FileListTest[FileListTestIndex]))
			{
				Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
				TestFontInspect(mat);
				txt_FontCount.Text = string.Format("{0} / {1}", FileListTestIndex + 1, FileListTest.Length);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_prev_Click(object sender, RoutedEventArgs e)
	{
		if (FileListTest.Length != 0)
		{
			FileListTestIndex--;
			if (FileListTestIndex < 0)
			{
				FileListTestIndex = FileListTest.Length - 1;
			}
			if (File.Exists(FileListTest[FileListTestIndex]))
			{
				Mat mat = CVBuffer.ImRead(FileListTest[FileListTestIndex], ImreadModes.Grayscale);
				TestFontInspect(mat);
				txt_FontCount.Text = string.Format("{0} / {1}", FileListTestIndex + 1, FileListTest.Length);
			}
		}
	}

	private void Btn_CloseCheckFontTool_Click(object sender, RoutedEventArgs e)
	{
		grd_FontTest.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LoadScriptValue(int index)
	{
		if (ScriptItems.Count <= 0 || index < 0 || index >= ScriptItems.Count)
		{
			return;
		}
		try
		{
			int num = Convert.ToInt32(ScriptItems[index].BinaryValue);
			int num2 = 0;
			num2 = ((!(ScriptItems[index].Morph == "不使用")) ? ((ScriptItems[index].Morph == "斷開") ? 1 : 2) : 0);
			int num3 = Convert.ToInt32(ScriptItems[index].MorphCount);
			INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
			switch (FontSetupChar)
			{
			case "0":
				fontToolParam.T0_BinaryShift = num;
				fontToolParam.T0_OpenClose = num2;
				fontToolParam.T0_OpenCloseCount = num3;
				break;
			case "R":
				fontToolParam.R_BinaryShift = num;
				fontToolParam.R_OpenClose = num2;
				fontToolParam.R_OpenCloseCount = num3;
				break;
			case "M":
				fontToolParam.M_BinaryShift = num;
				fontToolParam.M_OpenClose = num2;
				fontToolParam.M_OpenCloseCount = num3;
				break;
			case "C":
				fontToolParam.C_BinaryShift = num;
				fontToolParam.C_OpenClose = num2;
				fontToolParam.C_OpenCloseCount = num3;
				break;
			case "X":
				fontToolParam.X_BinaryShift = num;
				fontToolParam.X_OpenClose = num2;
				fontToolParam.X_OpenCloseCount = num3;
				break;
			case "B":
				fontToolParam.TB_BinaryShift = num;
				fontToolParam.TB_OpenClose = num2;
				fontToolParam.TB_OpenCloseCount = num3;
				break;
			case "D":
				fontToolParam.D_BinaryShift = num;
				fontToolParam.D_OpenClose = num2;
				fontToolParam.D_OpenCloseCount = num3;
				break;
			case "A":
				fontToolParam.A_BinaryShift = num;
				fontToolParam.A_OpenClose = num2;
				fontToolParam.A_OpenCloseCount = num3;
				break;
			case "E":
				fontToolParam.TE_BinaryShift = num;
				fontToolParam.TE_OpenClose = num2;
				fontToolParam.TE_OpenCloseCount = num3;
				break;
			case "-":
				fontToolParam.Dash_BinaryShift = num;
				fontToolParam.Dash_OpenClose = num2;
				fontToolParam.Dash_OpenCloseCount = num3;
				break;
			case "1":
				fontToolParam.T1_BinaryShift = num;
				fontToolParam.T1_OpenClose = num2;
				fontToolParam.T1_OpenCloseCount = num3;
				break;
			case "2":
				fontToolParam.T2_BinaryShift = num;
				fontToolParam.T2_OpenClose = num2;
				fontToolParam.T2_OpenCloseCount = num3;
				break;
			case "3":
				fontToolParam.T3_BinaryShift = num;
				fontToolParam.T3_OpenClose = num2;
				fontToolParam.T3_OpenCloseCount = num3;
				break;
			case "4":
				fontToolParam.T4_BinaryShift = num;
				fontToolParam.T4_OpenClose = num2;
				fontToolParam.T4_OpenCloseCount = num3;
				break;
			case "5":
				fontToolParam.T5_BinaryShift = num;
				fontToolParam.T5_OpenClose = num2;
				fontToolParam.T5_OpenCloseCount = num3;
				break;
			case "6":
				fontToolParam.T6_BinaryShift = num;
				fontToolParam.T6_OpenClose = num2;
				fontToolParam.T6_OpenCloseCount = num3;
				break;
			case "7":
				fontToolParam.T7_BinaryShift = num;
				fontToolParam.T7_OpenClose = num2;
				fontToolParam.T7_OpenCloseCount = num3;
				break;
			case "8":
				fontToolParam.T8_BinaryShift = num;
				fontToolParam.T8_OpenClose = num2;
				fontToolParam.T8_OpenCloseCount = num3;
				break;
			case "9":
				fontToolParam.T9_BinaryShift = num;
				fontToolParam.T9_OpenClose = num2;
				fontToolParam.T9_OpenCloseCount = num3;
				break;
			}
			inspection.paramFile.FontToolParam = fontToolParam;
		}
		catch (Exception ex)
		{
			string text = "FTW01-TrainCSVWindow-LoadScriptValue()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void ReadBinarySetting()
	{
		if (!inited)
		{
			return;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		string fontSetupChar = FontSetupChar;
		if (fontSetupChar == null)
		{
			return;
		}
		int length = fontSetupChar.Length;
		if (length == 1)
		{
			switch (fontSetupChar[0])
			{
			case '0':
				sl_BinaryShift.Value = fontToolParam.T0_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T0_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T0_OpenCloseCount;
				break;
			case 'R':
				sl_BinaryShift.Value = fontToolParam.R_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.R_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.R_OpenCloseCount;
				break;
			case 'M':
				sl_BinaryShift.Value = fontToolParam.M_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.M_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.M_OpenCloseCount;
				break;
			case 'C':
				sl_BinaryShift.Value = fontToolParam.C_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.C_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.C_OpenCloseCount;
				break;
			case 'X':
				sl_BinaryShift.Value = fontToolParam.X_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.X_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.X_OpenCloseCount;
				break;
			case 'B':
				sl_BinaryShift.Value = fontToolParam.TB_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.TB_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.TB_OpenCloseCount;
				break;
			case 'D':
				sl_BinaryShift.Value = fontToolParam.D_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.D_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.D_OpenCloseCount;
				break;
			case 'A':
				sl_BinaryShift.Value = fontToolParam.A_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.A_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.A_OpenCloseCount;
				break;
			case 'E':
				sl_BinaryShift.Value = fontToolParam.TE_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.TE_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.TE_OpenCloseCount;
				break;
			case '-':
				sl_BinaryShift.Value = fontToolParam.Dash_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.Dash_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.Dash_OpenCloseCount;
				break;
			case '1':
				sl_BinaryShift.Value = fontToolParam.T1_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T1_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T1_OpenCloseCount;
				break;
			case '2':
				sl_BinaryShift.Value = fontToolParam.T2_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T2_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T2_OpenCloseCount;
				break;
			case '3':
				sl_BinaryShift.Value = fontToolParam.T3_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T3_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T3_OpenCloseCount;
				break;
			case '4':
				sl_BinaryShift.Value = fontToolParam.T4_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T4_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T4_OpenCloseCount;
				break;
			case '5':
				sl_BinaryShift.Value = fontToolParam.T5_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T5_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T5_OpenCloseCount;
				break;
			case '6':
				sl_BinaryShift.Value = fontToolParam.T6_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T6_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T6_OpenCloseCount;
				break;
			case '7':
				sl_BinaryShift.Value = fontToolParam.T7_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T7_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T7_OpenCloseCount;
				break;
			case '8':
				sl_BinaryShift.Value = fontToolParam.T8_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T8_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T8_OpenCloseCount;
				break;
			case '9':
				sl_BinaryShift.Value = fontToolParam.T9_BinaryShift;
				lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
				cb_OpenClose.SelectedIndex = fontToolParam.T9_OpenClose;
				cb_OpenCloseCount.SelectedIndex = fontToolParam.T9_OpenCloseCount;
				break;
			}
		}
	}

	private void WriteBinarySetting()
	{
		if (inited)
		{
			INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
			inspection.paramFile.FontToolParam.ValueChangeFlag = true;
			switch (FontSetupChar)
			{
			case "0":
				fontToolParam.T0_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T0_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T0_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "R":
				fontToolParam.R_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.R_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.R_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "M":
				fontToolParam.M_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.M_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.M_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "C":
				fontToolParam.C_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.C_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.C_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "X":
				fontToolParam.X_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.X_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.X_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "B":
				fontToolParam.TB_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.TB_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.TB_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "D":
				fontToolParam.D_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.D_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.D_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "A":
				fontToolParam.A_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.A_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.A_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "E":
				fontToolParam.TE_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.TE_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.TE_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "-":
				fontToolParam.Dash_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.Dash_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.Dash_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "1":
				fontToolParam.T1_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T1_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T1_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "2":
				fontToolParam.T2_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T2_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T2_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "3":
				fontToolParam.T3_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T3_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T3_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "4":
				fontToolParam.T4_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T4_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T4_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "5":
				fontToolParam.T5_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T5_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T5_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "6":
				fontToolParam.T6_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T6_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T6_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "7":
				fontToolParam.T7_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T7_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T7_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "8":
				fontToolParam.T8_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T8_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T8_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			case "9":
				fontToolParam.T9_BinaryShift = (int)sl_BinaryShift.Value;
				fontToolParam.T9_OpenClose = cb_OpenClose.SelectedIndex;
				fontToolParam.T9_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
				break;
			}
			lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
			inspection.paramFile.FontToolParam = fontToolParam;
		}
	}

	private void Btn_testYield_Click(object sender, RoutedEventArgs e)
	{
		TestYield();
	}

	private void Lab_TrainResult_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		btn_testYield.Visibility = Visibility.Visible;
	}

	private void Lv_AIResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Lv_AIResultList_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
	{
		int selectedIndex = lv_AIResultList.SelectedIndex;
		if (selectedIndex < 0)
		{
			return;
		}
		sl_BinaryShift.Value = Convert.ToInt32(TRItems[selectedIndex].BinaryValue);
		lab_BinaryShift.Content = ((int)sl_BinaryShift.Value).ToString();
		if (TRItems[selectedIndex].Morph == "不使用")
		{
			cb_OpenClose.SelectedIndex = 0;
		}
		else if (TRItems[selectedIndex].Morph == "斷開")
		{
			cb_OpenClose.SelectedIndex = 1;
		}
		else
		{
			cb_OpenClose.SelectedIndex = 2;
		}
		cb_OpenCloseCount.SelectedIndex = Convert.ToInt32(TRItems[selectedIndex].MorphCount);
		WriteBinarySetting();
		TrainResultList.Clear();
		TrainResultList = TRItems[selectedIndex].TrainResultList.ToList();
		TrainResultMin.Clear();
		TrainResultMin = TRItems[selectedIndex].TrainResultMin.ToList();
		TrainResultMax.Clear();
		TrainResultMax = TRItems[selectedIndex].TrainResultMax.ToList();
		for (int i = FontAlgorithmCount; i < 16; i++)
		{
			GrdFontCheck[i].Visibility = Visibility.Hidden;
		}
		for (int j = 0; j < FontAlgorithmCount; j++)
		{
			GrdFontCheck[j].Visibility = Visibility.Visible;
			FontCheckAfter[j].IsChecked = TrainResultList[j];
		}
		btn_SaveTrainResult.Visibility = Visibility.Visible;
		try
		{
			switch (FontSetupChar)
			{
			case "0":
				Setup0Check();
				break;
			case "R":
				SetupRCheck();
				break;
			case "M":
				SetupMCheck();
				break;
			case "C":
				SetupCCheck();
				break;
			case "U":
				SetupUCheck();
				break;
			case "X":
				SetupXCheck();
				break;
			case "B":
				SetupBCheck();
				break;
			case "D":
				SetupDCheck();
				break;
			case "A":
				SetupACheck();
				break;
			case "E":
				SetupECheck();
				break;
			case "-":
				SetupDashCheck();
				break;
			case "1":
				Setup1Check();
				break;
			case "2":
				Setup2Check();
				break;
			case "3":
				Setup3Check();
				break;
			case "4":
				Setup4Check();
				break;
			case "5":
				Setup5Check();
				break;
			case "6":
				Setup6Check();
				break;
			case "7":
				Setup7Check();
				break;
			case "8":
				Setup8Check();
				break;
			case "9":
				Setup9Check();
				break;
			}
			SaveFontSetup(FontSetupChar);
			for (int k = 0; k < FontAlgorithmCount; k++)
			{
				if (FontCheckBefore[k].IsChecked != TrainResultList[k])
				{
					GrdFontCheck[k].Background = Brushes.LightBlue;
					LabValueChange[k].Visibility = Visibility.Visible;
				}
				else
				{
					GrdFontCheck[k].Background = Brushes.White;
					LabValueChange[k].Visibility = Visibility.Hidden;
				}
			}
			lv_AIResultList.Visibility = Visibility.Hidden;
			btn_TrainResultLoad.Visibility = Visibility.Hidden;
			UpdateYield(selectedIndex);
		}
		catch (Exception ex)
		{
			string text = "FTW01-TrainCSVWindow-Lv_AIResultList_MouseDoubleClick_1()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Btn_ShowList_Click(object sender, RoutedEventArgs e)
	{
		lv_AIResultList.Visibility = Visibility.Visible;
		grd_FontTest.Visibility = Visibility.Hidden;
		btn_TrainResultLoad.Visibility = Visibility.Visible;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetMorphStr()
	{
		if (cb_OpenClose.SelectedIndex == 0)
		{
			return "不使用";
		}
		if (cb_OpenClose.SelectedIndex == 0)
		{
			return "斷開";
		}
		return "連結";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ScriptTest_Click(object sender, RoutedEventArgs e)
	{
		if (ScriptItems.Count <= 0)
		{
			MessageBox.Show("請先將參數加入批次訓練!", "警告");
			return;
		}
		lab_Loading.Content = string.Format("批次字元參數訓練中...({0}/{1})", 1, ScriptItems.Count);
		grd_Loading.Visibility = Visibility.Visible;
		grd_FontTest.Visibility = Visibility.Hidden;
		TrainScriptCSVTimer.Start();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ClearList_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show("是否清除所有訓練結果? ", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			lv_AIResultList.ItemsSource = null;
			TRItems.Clear();
			TrainCount = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_DeleteScript_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = lv_ScriptList.SelectedIndex;
		if (selectedIndex < 0)
		{
			MessageBox.Show("尚未選擇刪除項次!", "警告");
			return;
		}
		ScriptItems.RemoveAt(selectedIndex);
		lv_ScriptList.ItemsSource = null;
		lv_ScriptList.ItemsSource = ScriptItems;
	}

	private void Btn_TrainResultLoad_Click(object sender, RoutedEventArgs e)
	{
		if (TRItems.Count > 0 && lv_AIResultList.SelectedIndex >= 0 && lv_AIResultList.SelectedIndex < TRItems.Count)
		{
			Lv_AIResultList_MouseDoubleClick_1(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AddToScript_Click(object sender, RoutedEventArgs e)
	{
		TrainResultItem item = new TrainResultItem
		{
			BinaryValue = lab_BinaryShift.Content.ToString(),
			Morph = GetMorphStr(),
			MorphCount = cb_OpenCloseCount.SelectedIndex.ToString(),
			ItemIndex = (ScriptItems.Count + 1).ToString()
		};
		for (int i = 0; i < ScriptItems.Count; i++)
		{
			if (item.BinaryValue == ScriptItems[i].BinaryValue && item.Morph == ScriptItems[i].Morph && item.MorphCount == ScriptItems[i].MorphCount)
			{
				MessageBox.Show("相同參數已加入批次訓練中!", "警告");
				return;
			}
		}
		for (int j = 0; j < TRItems.Count; j++)
		{
			if (item.BinaryValue == TRItems[j].BinaryValue && item.Morph == TRItems[j].Morph && item.MorphCount == TRItems[j].MorphCount)
			{
				MessageBox.Show("相同參數已訓練過!", "警告");
				return;
			}
		}
		ScriptItems.Add(item);
		lv_ScriptList.ItemsSource = null;
		lv_ScriptList.ItemsSource = ScriptItems;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/traincsvwindow.xaml", UriKind.Relative);
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
			((TrainCSVWindow)target).Closing += Window_Closing;
			((TrainCSVWindow)target).Drop += Window_Drop;
			break;
		case 2:
			grd_Advance = (Grid)target;
			break;
		case 3:
			cb_NGWeight = (ComboBox)target;
			cb_NGWeight.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 4:
			chk_TrainPara = (CheckBox)target;
			break;
		case 5:
			lab_FontSetup = (Label)target;
			break;
		case 6:
			lab_FontSetupMain_Copy = (Label)target;
			break;
		case 7:
			btn_OpenTrainCSV = (Button)target;
			btn_OpenTrainCSV.Click += Btn_OpenTrainCSV_Click;
			break;
		case 8:
			grd_Result = (Grid)target;
			break;
		case 9:
			grd_FontCheck1 = (Grid)target;
			break;
		case 10:
			lab_ValueChange1 = (Label)target;
			break;
		case 11:
			rad_FontCheckBefore1 = (CheckBox)target;
			break;
		case 12:
			rad_FontCheckAfter1 = (CheckBox)target;
			break;
		case 13:
			grd_FontCheck2 = (Grid)target;
			break;
		case 14:
			rad_FontCheckBefore2 = (CheckBox)target;
			break;
		case 15:
			lab_ValueChange2 = (Label)target;
			break;
		case 16:
			rad_FontCheckAfter2 = (CheckBox)target;
			break;
		case 17:
			grd_FontCheck3 = (Grid)target;
			break;
		case 18:
			rad_FontCheckBefore3 = (CheckBox)target;
			break;
		case 19:
			lab_ValueChange3 = (Label)target;
			break;
		case 20:
			rad_FontCheckAfter3 = (CheckBox)target;
			break;
		case 21:
			grd_FontCheck4 = (Grid)target;
			break;
		case 22:
			rad_FontCheckBefore4 = (CheckBox)target;
			break;
		case 23:
			lab_ValueChange4 = (Label)target;
			break;
		case 24:
			rad_FontCheckAfter4 = (CheckBox)target;
			break;
		case 25:
			grd_FontCheck5 = (Grid)target;
			break;
		case 26:
			rad_FontCheckBefore5 = (CheckBox)target;
			break;
		case 27:
			lab_ValueChange5 = (Label)target;
			break;
		case 28:
			rad_FontCheckAfter5 = (CheckBox)target;
			break;
		case 29:
			grd_FontCheck6 = (Grid)target;
			break;
		case 30:
			rad_FontCheckBefore6 = (CheckBox)target;
			break;
		case 31:
			lab_ValueChange6 = (Label)target;
			break;
		case 32:
			rad_FontCheckAfter6 = (CheckBox)target;
			break;
		case 33:
			grd_FontCheck7 = (Grid)target;
			break;
		case 34:
			rad_FontCheckBefore7 = (CheckBox)target;
			break;
		case 35:
			lab_ValueChange7 = (Label)target;
			break;
		case 36:
			rad_FontCheckAfter7 = (CheckBox)target;
			break;
		case 37:
			grd_FontCheck8 = (Grid)target;
			break;
		case 38:
			rad_FontCheckBefore8 = (CheckBox)target;
			break;
		case 39:
			lab_ValueChange8 = (Label)target;
			break;
		case 40:
			rad_FontCheckAfter8 = (CheckBox)target;
			break;
		case 41:
			grd_FontCheck9 = (Grid)target;
			break;
		case 42:
			rad_FontCheckBefore9 = (CheckBox)target;
			break;
		case 43:
			lab_ValueChange9 = (Label)target;
			break;
		case 44:
			rad_FontCheckAfter9 = (CheckBox)target;
			break;
		case 45:
			grd_FontCheck10 = (Grid)target;
			break;
		case 46:
			rad_FontCheckBefore10 = (CheckBox)target;
			break;
		case 47:
			lab_ValueChange10 = (Label)target;
			break;
		case 48:
			rad_FontCheckAfter10 = (CheckBox)target;
			break;
		case 49:
			grd_FontCheck11 = (Grid)target;
			break;
		case 50:
			rad_FontCheckBefore11 = (CheckBox)target;
			break;
		case 51:
			lab_ValueChange11 = (Label)target;
			break;
		case 52:
			rad_FontCheckAfter11 = (CheckBox)target;
			break;
		case 53:
			grd_FontCheck12 = (Grid)target;
			break;
		case 54:
			rad_FontCheckBefore12 = (CheckBox)target;
			break;
		case 55:
			lab_ValueChange12 = (Label)target;
			break;
		case 56:
			rad_FontCheckAfter12 = (CheckBox)target;
			break;
		case 57:
			grd_FontCheck13 = (Grid)target;
			break;
		case 58:
			rad_FontCheckBefore13 = (CheckBox)target;
			break;
		case 59:
			lab_ValueChange13 = (Label)target;
			break;
		case 60:
			rad_FontCheckAfter13 = (CheckBox)target;
			break;
		case 61:
			grd_FontCheck14 = (Grid)target;
			break;
		case 62:
			rad_FontCheckBefore14 = (CheckBox)target;
			break;
		case 63:
			lab_ValueChange14 = (Label)target;
			break;
		case 64:
			rad_FontCheckAfter14 = (CheckBox)target;
			break;
		case 65:
			grd_FontCheck15 = (Grid)target;
			break;
		case 66:
			rad_FontCheckBefore15 = (CheckBox)target;
			break;
		case 67:
			lab_ValueChange15 = (Label)target;
			break;
		case 68:
			rad_FontCheckAfter15 = (CheckBox)target;
			break;
		case 69:
			grd_FontCheck16 = (Grid)target;
			break;
		case 70:
			rad_FontCheckBefore16 = (CheckBox)target;
			break;
		case 71:
			lab_ValueChange16 = (Label)target;
			break;
		case 72:
			rad_FontCheckAfter16 = (CheckBox)target;
			break;
		case 73:
			btn_SaveTrainResult = (Button)target;
			btn_SaveTrainResult.Click += Btn_SaveTrainResult_Click;
			break;
		case 74:
			cb_ShowTrainWnd = (CheckBox)target;
			cb_ShowTrainWnd.Click += Cb_ShowTrainWnd_Click;
			break;
		case 75:
			btn_ShowList = (Button)target;
			btn_ShowList.Click += Btn_ShowList_Click;
			break;
		case 76:
			grd_2 = (Grid)target;
			break;
		case 77:
			ico_ResultNGFont1_Copy = (Label)target;
			break;
		case 78:
			cb_FontSelectTest = (ComboBox)target;
			break;
		case 79:
			btn_TrainCSV = (Button)target;
			btn_TrainCSV.Click += Btn_TrainCSV_Click;
			break;
		case 80:
			grd_3 = (Grid)target;
			break;
		case 81:
			ico_ResultNGFont1_Copy1 = (Label)target;
			break;
		case 82:
			grd_4 = (Grid)target;
			break;
		case 83:
			lab_TrainResult = (Label)target;
			lab_TrainResult.PreviewMouseDoubleClick += Lab_TrainResult_PreviewMouseDoubleClick;
			break;
		case 84:
			ico_ResultNGFont1_Copy2 = (Label)target;
			break;
		case 85:
			btn_testYield = (Button)target;
			btn_testYield.Click += Btn_testYield_Click;
			break;
		case 86:
			grd_Image = (Grid)target;
			break;
		case 87:
			btn_OKImage = (Button)target;
			btn_OKImage.Click += Btn_OKImage_Click;
			break;
		case 88:
			btn_NGImage = (Button)target;
			btn_NGImage.Click += Btn_NGImage_Click;
			break;
		case 89:
			lab_ValueChange1_Copy = (Label)target;
			break;
		case 90:
			lab_ValueChange1_Copy2 = (Label)target;
			break;
		case 91:
			grd_Preview = (Grid)target;
			break;
		case 92:
			btn_OKWrong = (Button)target;
			btn_OKWrong.Click += Btn_OKWrong_Click;
			break;
		case 93:
			btn_NGWrong = (Button)target;
			btn_NGWrong.Click += Btn_NGWrong_Click;
			break;
		case 94:
			lab_ValueChange1_Copy1 = (Label)target;
			break;
		case 95:
			lab_ValueChange1_Copy3 = (Label)target;
			break;
		case 96:
			lab_Yelid = (Label)target;
			break;
		case 97:
			lab_MissYield = (Label)target;
			break;
		case 98:
			lv_AIResultList = (ListView)target;
			lv_AIResultList.MouseDoubleClick += Lv_AIResultList_MouseDoubleClick_1;
			break;
		case 99:
			grd_FontTest = (Grid)target;
			break;
		case 100:
			img_TestFont = (Image)target;
			break;
		case 101:
			btn_prev = (Button)target;
			btn_prev.Click += Btn_prev_Click;
			break;
		case 102:
			btn_next = (Button)target;
			btn_next.Click += Btn_next_Click;
			break;
		case 103:
			btn_FontTest = (Button)target;
			btn_FontTest.Click += Btn_FontTest_Click_1;
			break;
		case 104:
			txt_FontCount = (TextBox)target;
			break;
		case 105:
			lab_FontTestResult = (Label)target;
			break;
		case 106:
			txt_TestFontResult = (TextBox)target;
			break;
		case 107:
			img_TestFontPattern = (Image)target;
			break;
		case 108:
			lab_FontTestResultScore = (Label)target;
			break;
		case 109:
			lab_FontOCRResult = (Label)target;
			break;
		case 110:
			btn_CloseCheckFontTool = (Button)target;
			btn_CloseCheckFontTool.Click += Btn_CloseCheckFontTool_Click;
			break;
		case 111:
			lab_FontTestSet = (Label)target;
			break;
		case 112:
			btn_OpenFolder = (Button)target;
			btn_OpenFolder.Click += Btn_OpenFolder_Click;
			break;
		case 113:
			sl_BinaryShift = (Slider)target;
			sl_BinaryShift.ValueChanged += Sl_BinaryShift_ValueChanged;
			break;
		case 114:
			cb_OpenClose = (ComboBox)target;
			cb_OpenClose.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 115:
			cb_OpenCloseCount = (ComboBox)target;
			cb_OpenCloseCount.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 116:
			lab_BinaryShift = (Label)target;
			break;
		case 117:
			img_TestFontBinary = (Image)target;
			break;
		case 118:
			lv_ScriptList = (ListView)target;
			break;
		case 119:
			btn_AddToScript = (Button)target;
			btn_AddToScript.Click += Btn_AddToScript_Click;
			break;
		case 120:
			btn_DeleteScript = (Button)target;
			btn_DeleteScript.Click += Btn_DeleteScript_Click;
			break;
		case 121:
			btn_ScriptTest = (Button)target;
			btn_ScriptTest.Click += Btn_ScriptTest_Click;
			break;
		case 122:
			grd_Loading = (Grid)target;
			break;
		case 123:
			lab_Loading = (Label)target;
			break;
		case 124:
			btn_ClearList = (Button)target;
			btn_ClearList.Click += Btn_ClearList_Click;
			break;
		case 125:
			btn_TrainResultLoad = (Button)target;
			btn_TrainResultLoad.Click += Btn_TrainResultLoad_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
