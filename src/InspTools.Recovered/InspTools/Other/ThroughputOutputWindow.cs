using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using OpenCvSharp;

namespace InspTools.Other;

public class ThroughputOutputWindow : System.Windows.Window, IComponentConnector
{
	private System.Windows.Controls.CheckBox[] cbstation;

	private bool Inited;

	private int LanguageType = 1;

	private HiniFile ThroughputINI;

	public ThroughputParamFile ThroughputFile;

	private string ImagePath = "";

	private string ProductID = "";

	private int imagecountindex;

	public CVInspection[] Station = new CVInspection[8];

	private FolderBrowserDialog folderPath = new FolderBrowserDialog();

	private int[] ImageWidth = new int[8];

	private int[] ImageHeight = new int[8];

	private int[] ImageBand = new int[8];

	private int[] BufferImageWidth = new int[8];

	private int[] BufferImageHeight = new int[8];

	private int[] BufferImageBand = new int[8];

	private bool[] ToolON = new bool[8];

	private bool AutoSaveSuccess = true;

	private int imageformat;

	private Thread AutoSaveThread;

	private ManualResetEvent AutoSavePauseEvent = new ManualResetEvent(initialState: false);

	private bool ForceClose;

	internal System.Windows.Controls.Button btn_Output;

	internal System.Windows.Controls.TextBox txt_Path;

	internal System.Windows.Controls.Button btn_SetPath;

	internal System.Windows.Controls.ComboBox cb_ImageFormatSelect;

	internal System.Windows.Controls.ComboBox cb_OutputImageCount;

	internal System.Windows.Controls.ComboBox cb_OutputOkNg;

	internal System.Windows.Controls.CheckBox chk_Station1;

	internal System.Windows.Controls.CheckBox chk_Station2;

	internal System.Windows.Controls.CheckBox chk_Station3;

	internal System.Windows.Controls.CheckBox chk_Station4;

	internal System.Windows.Controls.CheckBox chk_Station5;

	internal System.Windows.Controls.CheckBox chk_Station6;

	internal System.Windows.Controls.CheckBox chk_Station7;

	internal System.Windows.Controls.CheckBox chk_Station8;

	internal System.Windows.Controls.CheckBox chk_Station9;

	internal System.Windows.Controls.CheckBox chk_Station10;

	internal ProgressRing ico_Loading;

	private bool _contentLoaded;

	public ThroughputOutputWindow()
	{
		InitializeComponent();
		ThroughputFile = new ThroughputParamFile();
		cbstation = new System.Windows.Controls.CheckBox[8] { chk_Station1, chk_Station2, chk_Station3, chk_Station4, chk_Station5, chk_Station6, chk_Station7, chk_Station8 };
		for (int i = 0; i < 8; i++)
		{
			Station[i] = null;
		}
		Inited = true;
		ico_Loading.Visibility = Visibility.Hidden;
		AutoSaveThread = new Thread(AutoSaveThreadRun);
		AutoSaveThread.Start();
		AutoSaveSuccess = true;
		chk_Station9.Visibility = Visibility.Hidden;
		chk_Station10.Visibility = Visibility.Hidden;
	}

	private void AutoSaveThreadRun()
	{
		AutoSavePauseEvent.WaitOne(-1);
		if (!ForceClose)
		{
			SaveStart();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetProductID(string str)
	{
		ProductID = str.TrimEnd();
		DateTime now = DateTime.Now;
		string text = now.ToString("yyyyMM") + "\\";
		string text2 = now.ToString("dd") + "\\";
		ImagePath = "D:\\Throughput\\" + text + text2 + ProductID + "\\";
		try
		{
			if (!Directory.Exists(ImagePath))
			{
				Directory.CreateDirectory(ImagePath);
			}
			txt_Path.Text = ImagePath;
			AutoSaveSuccess = true;
		}
		catch (Exception ex)
		{
			_ = "ThroughputOutputWindow-SetProductID()" + ex.ToString();
			CVBuffer.SaveLog(str);
			AutoSaveSuccess = false;
		}
	}

	public void SetStation(CVInspection[] station)
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i] = station[i];
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffers(List<byte[]>[] recentbufferlist, List<byte[]>[] okbufferlist, List<byte[]>[] ngbufferlist, List<byte[]>[] retestbufferlist, List<int>[] okIDlists, List<int>[] ngIDlists, List<int>[] recentIDlists, int[] lastIndex, int[] w, int[] h, int[] band)
	{
		try
		{
			for (int i = 0; i < 8; i++)
			{
				ImageWidth[i] = w[i];
				ImageHeight[i] = h[i];
				ImageBand[i] = band[i];
				BufferImageWidth[i] = w[i];
				BufferImageHeight[i] = h[i];
				BufferImageBand[i] = band[i];
				Station[i].SetBuffers(recentbufferlist[i], okbufferlist[i], ngbufferlist[i], retestbufferlist[i], okIDlists[i], ngIDlists[i], recentIDlists[i], lastIndex[i], w[i], h[i], band[i]);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ThroughputOutputWindow-SetBuffers()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetRecipe(string recipename)
	{
		try
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + recipename + "\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string file = text + "Setting.ini";
			ThroughputINI = new HiniFile(file, ";");
			ThroughputFile.SetINI(ThroughputINI);
			ThroughputFile.ReadThroughputParam();
			ThroughputFile.ReadParetoItems();
			ReadParamFile();
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ThroughputOutputWindow-SetRecipe()" + ex.ToString());
		}
	}

	public string GetImagePath()
	{
		return ImagePath;
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
	private void Btn_SetPath_Click(object sender, RoutedEventArgs e)
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		try
		{
			if (folderSelectDialog.ShowDialog())
			{
				if (ProductID != "")
				{
					ImagePath = string.Format("{0}\\{1}\\", folderSelectDialog.FileName, ProductID);
				}
				else
				{
					ImagePath = string.Format("{0}\\", folderSelectDialog.FileName);
				}
				txt_Path.Text = ImagePath;
				AutoSaveSuccess = true;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ThroughputOutputWindow-ReadParamFile()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ReadParamFile()
	{
		if (ThroughputINI == null)
		{
			return;
		}
		try
		{
			cb_ImageFormatSelect.SelectedIndex = ThroughputFile.param.OutputFormat;
			cb_OutputImageCount.SelectedIndex = ThroughputFile.param.OutputImageCount;
			cb_OutputOkNg.SelectedIndex = ThroughputFile.param.OutputALLOKNG;
			if (ThroughputFile.param.OutputPath != "" && ThroughputFile.param.OutputPath != null)
			{
				ImagePath = ThroughputFile.param.OutputPath;
			}
			txt_Path.Text = ImagePath;
			chk_Station1.IsChecked = ThroughputFile.param.Station1ON;
			chk_Station2.IsChecked = ThroughputFile.param.Station2ON;
			chk_Station3.IsChecked = ThroughputFile.param.Station3ON;
			chk_Station4.IsChecked = ThroughputFile.param.Station4ON;
			chk_Station5.IsChecked = ThroughputFile.param.Station5ON;
			chk_Station6.IsChecked = ThroughputFile.param.Station6ON;
			chk_Station7.IsChecked = ThroughputFile.param.Station7ON;
			chk_Station8.IsChecked = ThroughputFile.param.Station8ON;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ThroughputOutputWindow-ReadParamFile()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteParamFile()
	{
		if (ThroughputINI == null)
		{
			return;
		}
		try
		{
			ThroughputFile.param.OutputFormat = cb_ImageFormatSelect.SelectedIndex;
			ThroughputFile.param.OutputALLOKNG = cb_OutputOkNg.SelectedIndex;
			ThroughputFile.param.OutputImageCount = cb_OutputImageCount.SelectedIndex;
			ThroughputFile.param.OutputPath = ImagePath;
			ThroughputFile.param.Station1ON = chk_Station1.IsChecked.Value;
			ThroughputFile.param.Station2ON = chk_Station2.IsChecked.Value;
			ThroughputFile.param.Station3ON = chk_Station3.IsChecked.Value;
			ThroughputFile.param.Station4ON = chk_Station4.IsChecked.Value;
			ThroughputFile.param.Station5ON = chk_Station5.IsChecked.Value;
			ThroughputFile.param.Station6ON = chk_Station6.IsChecked.Value;
			ThroughputFile.param.Station7ON = chk_Station7.IsChecked.Value;
			ThroughputFile.param.Station8ON = chk_Station8.IsChecked.Value;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ThroughputOutputWindow-WriteParamFile()" + ex.ToString());
		}
	}

	private void Btn_Output_Click(object sender, RoutedEventArgs e)
	{
		WriteParamFile();
		imageformat = cb_ImageFormatSelect.SelectedIndex;
		for (int i = 0; i < 8; i++)
		{
			ToolON[i] = cbstation[i].IsChecked.Value;
		}
		imagecountindex = cb_OutputImageCount.SelectedIndex;
		btn_Output.Visibility = Visibility.Hidden;
		ico_Loading.Visibility = Visibility.Visible;
		AutoSavePauseEvent.Set();
	}

	public bool SaveSuccess()
	{
		return AutoSaveSuccess;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SaveStart()
	{
		int num = 0;
		num = imagecountindex switch
		{
			0 => 100, 
			1 => 200, 
			2 => 300, 
			3 => 400, 
			4 => 500, 
			_ => 0, 
		};
		try
		{
			if (!Directory.Exists(ImagePath))
			{
				Directory.CreateDirectory(ImagePath);
			}
		}
		catch (Exception ex)
		{
			string text = "ThroughputOutputWindow-SaveStart-CreateDirectory()" + ex.ToString();
			System.Windows.MessageBox.Show(text, "錯誤");
			CVBuffer.SaveLog(text);
			AutoSaveSuccess = false;
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				Close();
			});
			return;
		}
		try
		{
			for (int num2 = 0; num2 < 8; num2++)
			{
				if (Station[num2] == null || !ToolON[num2])
				{
					continue;
				}
				string text3;
				string text4;
				try
				{
					string text2 = ImagePath + Convert.ToString(num2 + 1) + "\\";
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
					}
					text3 = text2 + "OK\\";
					text4 = text2 + "NG\\";
					if (!Directory.Exists(text3))
					{
						Directory.CreateDirectory(text3);
					}
					if (!Directory.Exists(text4))
					{
						Directory.CreateDirectory(text4);
					}
				}
				catch (Exception ex2)
				{
					string text5 = "ThroughputOutputWindow-SaveStart-CreateOKNG()" + ex2.ToString();
					System.Windows.MessageBox.Show(text5, "錯誤");
					CVBuffer.SaveLog(text5);
					AutoSaveSuccess = false;
					base.Dispatcher.BeginInvoke((Action)delegate
					{
						Close();
					});
					return;
				}
				string arg = ((imageformat == 0) ? "jpg" : ((imageformat != 1) ? "png" : "bmp"));
				bool flag = true;
				bool flag2 = true;
				if (ThroughputFile.param.OutputALLOKNG == 0)
				{
					flag = (flag2 = true);
				}
				else if (ThroughputFile.param.OutputALLOKNG == 1)
				{
					flag = true;
					flag2 = false;
				}
				else
				{
					flag = false;
					flag2 = true;
				}
				if (flag)
				{
					for (int num3 = 0; num3 < Station[num2].OKBufferList.Count; num3++)
					{
						if (num3 < num)
						{
							string path = string.Format("{0}\\{1}.{2}", text3, Station[num2].OKBufferIDList[num3], arg);
							SaveImage(Station[num2].OKBufferList[num3], BufferImageWidth[num2], BufferImageHeight[num2], BufferImageBand[num2], path);
						}
					}
				}
				if (!flag2)
				{
					continue;
				}
				int num4 = 0;
				for (int num5 = 0; num5 < Station[num2].NGBufferList.Count; num5++)
				{
					if (num5 >= num)
					{
						continue;
					}
					num4 = 0;
					for (int num6 = 0; num6 < Station[num2].ParetoData.Count; num6++)
					{
						if (Station[num2].NGBufferIDList != null)
						{
							if (num5 < Station[num2].NGBufferIDList.Count && Station[num2].ParetoData[num6].ID == Station[num2].NGBufferIDList[num5])
							{
								num4 = Station[num2].ParetoData[num6].ParetoID;
								num6 = Station[num2].ParetoData.Count;
							}
						}
						else
						{
							num6 = Station[num2].ParetoData.Count;
						}
					}
					string path;
					if (num4 == 0)
					{
						path = string.Format("{0}\\{1}.{2}", text4, Station[num2].NGBufferIDList[num5], arg);
					}
					else if (num4 < 30)
					{
						if (ThroughputFile.param.AllParetoItems[num4] == "")
						{
							path = string.Format("{0}\\{1}.{2}", text4, Station[num2].NGBufferIDList[num5], arg);
						}
						else
						{
							string text6 = string.Format("{0}\\{1}\\", text4, ThroughputFile.param.AllParetoItems[num4]);
							if (!Directory.Exists(text6))
							{
								Directory.CreateDirectory(text6);
							}
							path = string.Format("{0}{1}.{2}", text6, Station[num2].NGBufferIDList[num5], arg);
						}
					}
					else
					{
						path = string.Format("{0}\\{1}.{2}", text4, Station[num2].NGBufferIDList[num5], arg);
					}
					SaveImage(Station[num2].NGBufferList[num5], BufferImageWidth[num2], BufferImageHeight[num2], BufferImageBand[num2], path);
				}
			}
		}
		catch (Exception ex3)
		{
			string text7 = "ThroughputOutputWindow-SaveStart()" + ex3.ToString();
			System.Windows.MessageBox.Show(text7, "錯誤");
			CVBuffer.SaveLog(text7);
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				Close();
			});
			return;
		}
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			Close();
		});
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		ForceClose = true;
		AutoSavePauseEvent.Set();
		Thread.Sleep(100);
		AutoSaveThread.Join();
		Thread.Sleep(100);
		AutoSaveThread = null;
		ThroughputFile.WriteThroughputParam();
	}

	private void SaveImage(byte[] bayerBuffer, int w, int h, int band, string path)
	{
		if (w <= 0 || h <= 0 || band <= 0 || path == null)
		{
			return;
		}
		switch (band)
		{
		case 3:
		{
			using Mat mat = new Mat(h, w, MatType.CV_8UC1, bayerBuffer, 0L);
			using Mat mat2 = new Mat(h, w, MatType.CV_8UC3);
			Cv2.CvtColor(mat, mat2, ColorConversionCodes.BayerBG2BGR);
			mat2.SaveImage(path);
			break;
		}
		case 1:
			new Mat(h, w, MatType.CV_8UC1, bayerBuffer, 0L).SaveImage(path);
			break;
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
			Uri resourceLocator = new Uri("/InspTools;component/throughputoutputwindow.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, resourceLocator);
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
			((ThroughputOutputWindow)target).Closed += Window_Closed;
			break;
		case 2:
			btn_Output = (System.Windows.Controls.Button)target;
			btn_Output.Click += Btn_Output_Click;
			break;
		case 3:
			txt_Path = (System.Windows.Controls.TextBox)target;
			break;
		case 4:
			btn_SetPath = (System.Windows.Controls.Button)target;
			btn_SetPath.Click += Btn_SetPath_Click;
			break;
		case 5:
			cb_ImageFormatSelect = (System.Windows.Controls.ComboBox)target;
			break;
		case 6:
			cb_OutputImageCount = (System.Windows.Controls.ComboBox)target;
			break;
		case 7:
			cb_OutputOkNg = (System.Windows.Controls.ComboBox)target;
			break;
		case 8:
			chk_Station1 = (System.Windows.Controls.CheckBox)target;
			break;
		case 9:
			chk_Station2 = (System.Windows.Controls.CheckBox)target;
			break;
		case 10:
			chk_Station3 = (System.Windows.Controls.CheckBox)target;
			break;
		case 11:
			chk_Station4 = (System.Windows.Controls.CheckBox)target;
			break;
		case 12:
			chk_Station5 = (System.Windows.Controls.CheckBox)target;
			break;
		case 13:
			chk_Station6 = (System.Windows.Controls.CheckBox)target;
			break;
		case 14:
			chk_Station7 = (System.Windows.Controls.CheckBox)target;
			break;
		case 15:
			chk_Station8 = (System.Windows.Controls.CheckBox)target;
			break;
		case 16:
			chk_Station9 = (System.Windows.Controls.CheckBox)target;
			break;
		case 17:
			chk_Station10 = (System.Windows.Controls.CheckBox)target;
			break;
		case 18:
			ico_Loading = (ProgressRing)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
