using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;

namespace InspTools.AOI;

public class AIToolWindow : Window, IComponentConnector
{
	private int LanguageType = 1;

	private CVInspection Inspection;

	private CVInspection[] Station;

	private int StationIndex;

	private ThroughputParamFile ThroughputFile;

	private bool DontUpdateUI = true;

	private bool PredictMode = true;

	public bool[] ReloadModel = new bool[2];

	private string ModelName = "";

	private int DNetMode;

	private int ModelWidth;

	private int ModelHeight;

	private Thread AILoadingThread;

	private ManualResetEvent AILoadingPauseEvent = new ManualResetEvent(initialState: false);

	private bool ContinueRun = true;

	public bool EditHashTag;

	internal AIToolWindow wnd_Main;

	internal Grid grd_FaceA;

	internal TextBox txt_ModelPath_A;

	internal Button btn_SetModel_A;

	internal TextBox txt_ModelWidth_A;

	internal TextBox txt_ModelHeight_A;

	internal Label lb_ModelName_A;

	internal Label lab_FaceA;

	internal Grid grd_OutputA;

	internal ComboBox cb_ErrorBin_A0;

	internal Label lab_A0;

	internal Label lab_Output;

	internal ComboBox cb_ErrorBin_A1;

	internal Label lab_A1;

	internal ComboBox cb_ErrorBin_A2;

	internal Label lab_A2;

	internal ComboBox cb_ErrorBin_A3;

	internal Label lab_A3;

	internal Button btn_RevrseModelA;

	internal Button btn_CreateModelA;

	internal Label lab_Pareto_A;

	internal ComboBox cb_ParetoChart_A;

	internal Label lab_SetModelSizeResult;

	internal Grid grd_DNET;

	internal RadioButton rad_DnetMode1A;

	internal RadioButton rad_DnetMode2A;

	internal RadioButton rad_DnetMode3A;

	internal RadioButton rad_DnetMode15A;

	internal Grid grd_CustomChannel;

	internal Rectangle rt_1;

	internal ComboBox cb_Channel_G;

	internal ComboBox cb_Channel_B;

	internal ComboBox cb_Channel_R;

	internal Label lab_1;

	internal CheckBox cb_EnableCustumRGBChannel;

	internal Grid grd_Other;

	internal CheckBox chk_SelectFace;

	internal RadioButton chk_UseGPU;

	internal Label lab_DNet;

	internal CheckBox chk_EnableTensorRT;

	internal Label lab_NVCard;

	internal ComboBox cb_GPUID;

	internal CheckBox chk_EnableAsignGPU;

	internal Grid grd_FaceB;

	internal TextBox txt_ModelPath_B;

	internal Button btn_SetModel_B;

	internal TextBox txt_ModelWidth_B;

	internal TextBox txt_ModelHeight_B;

	internal Label lb_ModelName_B;

	internal Label lab_FaceB;

	internal Grid grd_OutputB;

	internal ComboBox cb_ErrorBin_B0;

	internal Label lab_A4;

	internal Label lab_Output1;

	internal ComboBox cb_ErrorBin_B1;

	internal Label lab_A5;

	internal ComboBox cb_ErrorBin_B2;

	internal Label lab_A6;

	internal ComboBox cb_ErrorBin_B3;

	internal Label lab_A7;

	internal RadioButton rad_DnetMode1B;

	internal RadioButton rad_DnetMode2B;

	internal RadioButton rad_DnetMode3B;

	internal Button btn_RevrseModelB;

	internal Button btn_CreateModelB;

	internal Label lab_Pareto_B;

	internal ComboBox cb_ParetoChart_B;

	internal Grid grd_AILoading;

	private bool _contentLoaded;

	public AIToolWindow()
	{
		InitializeComponent();
		rad_DnetMode3A.Visibility = Visibility.Hidden;
		chk_SelectFace.Visibility = Visibility.Hidden;
		AILoadingThread = new Thread(AILoadingRun);
		AILoadingThread.Start();
		grd_CustomChannel.Visibility = Visibility.Hidden;
	}

	public void SetInspection(CVInspection[] station, CVInspection inspection, int stationindex)
	{
		Station = station;
		Inspection = inspection;
		StationIndex = stationindex;
		CopyValueToUI();
		if (PredictMode)
		{
			chk_UseGPU.IsChecked = Inspection.aiPredictTools.useCUDA;
		}
		else
		{
			chk_UseGPU.IsChecked = Inspection.aiEnhanceTools.useCUDA;
		}
		if (!Inspection.paramFile.SelectFaceParam[1].TrackerON || !Inspection.paramFile.SelectFaceON)
		{
			chk_SelectFace.IsChecked = false;
			chk_SelectFace.Visibility = Visibility.Hidden;
		}
		ReloadModel[0] = (ReloadModel[1] = false);
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		DontUpdateUI = true;
		ThroughputFile = file;
		cb_ParetoChart_A.Items.Clear();
		cb_ParetoChart_B.Items.Clear();
		for (int i = 0; i < 30; i++)
		{
			if (ThroughputFile.param.AllParetoItems[i] != "")
			{
				cb_ParetoChart_A.Items.Add(ThroughputFile.param.AllParetoItems[i]);
				cb_ParetoChart_B.Items.Add(ThroughputFile.param.AllParetoItems[i]);
			}
		}
		DontUpdateUI = false;
	}

	public void SetPredictMode(bool UsePredictMode)
	{
		PredictMode = UsePredictMode;
		if (PredictMode)
		{
			grd_DNET.Visibility = Visibility.Hidden;
			grd_CustomChannel.Visibility = Visibility.Hidden;
			chk_EnableTensorRT.Visibility = Visibility.Hidden;
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

	private void AILoadingRun()
	{
		while (true)
		{
			AILoadingPauseEvent.WaitOne(-1);
			if (!ContinueRun)
			{
				break;
			}
			bool result = AutoGetModelSize();
			Action method = [MethodImpl(MethodImplOptions.NoInlining)] () =>
			{
				if (!result)
				{
					if (LanguageType == 1)
					{
						lab_SetModelSizeResult.Content = "請自行設置模型尺寸";
					}
					else if (LanguageType == 2)
					{
						lab_SetModelSizeResult.Content = "请自行设置模型尺寸";
					}
					else if (LanguageType == 3)
					{
						lab_SetModelSizeResult.Content = "モデルサイズを自分で設定してください";
					}
					else
					{
						lab_SetModelSizeResult.Content = "Please set the model size first.";
					}
					txt_ModelWidth_A.Background = Brushes.White;
					txt_ModelHeight_A.Background = Brushes.White;
				}
				else
				{
					txt_ModelWidth_A.Text = ModelWidth.ToString();
					txt_ModelHeight_A.Text = ModelHeight.ToString();
					txt_ModelWidth_A.Background = Brushes.Gold;
					txt_ModelHeight_A.Background = Brushes.Gold;
					txt_ModelWidth_A.IsReadOnly = true;
					txt_ModelHeight_A.IsReadOnly = true;
					if (LanguageType == 1)
					{
						lab_SetModelSizeResult.Content = "模型尺寸讀取成功";
					}
					else if (LanguageType == 2)
					{
						lab_SetModelSizeResult.Content = "模型尺寸读取成功";
					}
					else if (LanguageType == 3)
					{
						lab_SetModelSizeResult.Content = "モデルのサイズ読み込みが成功しました";
					}
					else
					{
						lab_SetModelSizeResult.Content = "Model size read successfully";
					}
					lab_SetModelSizeResult.Content = "模型尺寸讀取成功";
				}
				grd_AILoading.Visibility = Visibility.Hidden;
			};
			base.Dispatcher.BeginInvoke(method);
			AILoadingPauseEvent.Reset();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		DontUpdateUI = true;
		string arg = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
		if (PredictMode)
		{
			INSP_AITOOL_PARAM aIPredictParam = Inspection.paramFile.AIPredictParam;
			string modelName_A = Inspection.paramFile.AIPredictParam.ModelName_A;
			string path = string.Format("{0}{1}", arg, modelName_A);
			lab_NVCard.Content = aIPredictParam.NVCardName;
			cb_ParetoChart_A.SelectedIndex = Inspection.paramFile.AIPredictParam.ParetoIndex_A;
			cb_ParetoChart_B.SelectedIndex = Inspection.paramFile.AIPredictParam.ParetoIndex_B;
			if (Inspection.paramFile.AIPredictParam.ModelName_A == "" || !File.Exists(path))
			{
				lb_ModelName_A.Background = Brushes.Red;
				lb_ModelName_A.Content = (Inspection.paramFile.AIPredictParam.ModelName_A = "");
				Inspection.paramFile.AIPredictParam.ModelExist_A = false;
			}
			else
			{
				lb_ModelName_A.Content = modelName_A;
				txt_ModelPath_A.Text = string.Format("Model\\{0}", Inspection.paramFile.AIPredictParam.ModelName_A);
				lb_ModelName_A.Background = Brushes.Lime;
				Inspection.paramFile.AIPredictParam.ModelExist_A = true;
			}
			txt_ModelWidth_A.Text = Inspection.paramFile.AIPredictParam.Width_A.ToString();
			txt_ModelHeight_A.Text = Inspection.paramFile.AIPredictParam.Height_A.ToString();
			chk_SelectFace.IsChecked = false;
			lab_DNet.Visibility = Visibility.Hidden;
			chk_EnableTensorRT.IsChecked = Inspection.paramFile.AIPredictParam.UseTensorRT;
			if (aIPredictParam.EnableSelectFace)
			{
				string modelName_B = Inspection.paramFile.AIPredictParam.ModelName_B;
				string path2 = string.Format("{0}{1}", arg, modelName_B);
				if (Inspection.paramFile.AIPredictParam.ModelName_B == "" || !File.Exists(path2))
				{
					lb_ModelName_B.Background = Brushes.Red;
					lb_ModelName_B.Content = (Inspection.paramFile.AIPredictParam.ModelName_B = "");
					Inspection.paramFile.AIPredictParam.ModelExist_B = false;
				}
				else
				{
					lb_ModelName_B.Content = modelName_B;
					txt_ModelPath_B.Text = string.Format("Model\\{0}", Inspection.paramFile.AIPredictParam.ModelName_B);
					lb_ModelName_B.Background = Brushes.Lime;
					Inspection.paramFile.AIPredictParam.ModelExist_B = true;
				}
				txt_ModelWidth_B.Text = Inspection.paramFile.AIPredictParam.Width_B.ToString();
				txt_ModelHeight_B.Text = Inspection.paramFile.AIPredictParam.Height_B.ToString();
			}
			cb_ErrorBin_A0.SelectedIndex = (int)aIPredictParam.ErrorBin_A0;
			cb_ErrorBin_A1.SelectedIndex = (int)aIPredictParam.ErrorBin_A1;
			cb_ErrorBin_A2.SelectedIndex = (int)aIPredictParam.ErrorBin_A2;
			cb_ErrorBin_A3.SelectedIndex = (int)aIPredictParam.ErrorBin_A3;
			cb_ErrorBin_B0.SelectedIndex = (int)aIPredictParam.ErrorBin_B0;
			cb_ErrorBin_B1.SelectedIndex = (int)aIPredictParam.ErrorBin_B1;
			cb_ErrorBin_B2.SelectedIndex = (int)aIPredictParam.ErrorBin_B2;
			cb_ErrorBin_B3.SelectedIndex = (int)aIPredictParam.ErrorBin_B3;
		}
		else
		{
			INSP_AITOOL_PARAM aIPredictParam = Inspection.paramFile.AIEnhanceParam;
			grd_OutputA.Visibility = Visibility.Hidden;
			grd_OutputB.Visibility = Visibility.Hidden;
			cb_ParetoChart_A.Visibility = Visibility.Hidden;
			lab_Pareto_A.Visibility = Visibility.Hidden;
			cb_ParetoChart_B.Visibility = Visibility.Hidden;
			lab_Pareto_B.Visibility = Visibility.Hidden;
			if (aIPredictParam.EnableAsignGPU)
			{
				chk_EnableAsignGPU.IsChecked = true;
				cb_GPUID.IsEnabled = true;
				cb_GPUID.SelectedIndex = aIPredictParam.GPUID;
			}
			else
			{
				cb_GPUID.IsEnabled = false;
				chk_EnableAsignGPU.IsChecked = false;
				cb_GPUID.SelectedIndex = 0;
			}
			string modelName_A2 = Inspection.paramFile.AIEnhanceParam.ModelName_A;
			string path3 = string.Format("{0}{1}", arg, modelName_A2);
			lab_NVCard.Content = aIPredictParam.NVCardName;
			lb_ModelName_A.Content = Inspection.paramFile.AIEnhanceParam.ModelName_A;
			switch (Inspection.paramFile.AIEnhanceParam.DnetMode_A)
			{
			case 0:
				rad_DnetMode1A.IsChecked = true;
				break;
			case 1:
				rad_DnetMode2A.IsChecked = true;
				break;
			case 2:
				rad_DnetMode15A.IsChecked = true;
				break;
			default:
				rad_DnetMode1A.IsChecked = true;
				break;
			}
			if (Inspection.paramFile.AIEnhanceParam.ModelName_A == "" || !File.Exists(path3))
			{
				lb_ModelName_A.Background = Brushes.Red;
				lb_ModelName_A.Content = (Inspection.paramFile.AIEnhanceParam.ModelName_A = "");
			}
			else
			{
				Inspection.paramFile.AIEnhanceParam.ModelExist_A = true;
				lb_ModelName_A.Content = modelName_A2;
				txt_ModelPath_A.Text = string.Format("Model\\{0}", Inspection.paramFile.AIEnhanceParam.ModelName_A);
				lb_ModelName_A.Background = Brushes.LimeGreen;
			}
			txt_ModelWidth_A.Text = Inspection.paramFile.AIEnhanceParam.Width_A.ToString();
			txt_ModelHeight_A.Text = Inspection.paramFile.AIEnhanceParam.Height_A.ToString();
			cb_EnableCustumRGBChannel.IsChecked = Inspection.paramFile.AIEnhanceParam.EnableCustomRGBChannel;
			cb_Channel_R.SelectedIndex = Inspection.paramFile.AIEnhanceParam.CustomChannelR;
			cb_Channel_G.SelectedIndex = Inspection.paramFile.AIEnhanceParam.CustomChannelG;
			cb_Channel_B.SelectedIndex = Inspection.paramFile.AIEnhanceParam.CustomChannelB;
			chk_EnableTensorRT.IsChecked = Inspection.paramFile.AIEnhanceParam.UseTensorRT;
			chk_SelectFace.IsChecked = false;
			if (aIPredictParam.EnableSelectFace)
			{
				string modelName_B2 = Inspection.paramFile.AIEnhanceParam.ModelName_B;
				string path4 = string.Format("{0}{1}", arg, modelName_B2);
				lb_ModelName_B.Content = Inspection.paramFile.AIEnhanceParam.ModelName_B;
				switch (Inspection.paramFile.AIEnhanceParam.DnetMode_B)
				{
				case 0:
					rad_DnetMode1B.IsChecked = true;
					break;
				case 1:
					rad_DnetMode2B.IsChecked = true;
					break;
				case 2:
					rad_DnetMode3B.IsChecked = true;
					break;
				default:
					rad_DnetMode1B.IsChecked = true;
					break;
				}
				if (Inspection.paramFile.AIEnhanceParam.ModelName_B == "" || !File.Exists(path4))
				{
					lb_ModelName_B.Background = Brushes.Red;
					lb_ModelName_B.Content = (Inspection.paramFile.AIEnhanceParam.ModelName_B = "");
				}
				else
				{
					Inspection.paramFile.AIEnhanceParam.ModelExist_B = true;
					lb_ModelName_B.Content = modelName_B2;
					txt_ModelPath_B.Text = string.Format("Model\\{0}", Inspection.paramFile.AIEnhanceParam.ModelName_B);
					lb_ModelName_B.Background = Brushes.LimeGreen;
				}
				txt_ModelWidth_B.Text = Inspection.paramFile.AIEnhanceParam.Width_B.ToString();
				txt_ModelHeight_B.Text = Inspection.paramFile.AIEnhanceParam.Height_B.ToString();
				cb_ParetoChart_B.SelectedIndex = aIPredictParam.ParetoIndex_B;
			}
			lab_DNet.Visibility = Visibility.Visible;
		}
		DontUpdateUI = false;
		FaceSwitch(chk_SelectFace.IsChecked.Value);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ShowModelAlarm(int id)
	{
		if (id == 0)
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("尚未指定AI模型檔案！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("尚未指定AI模型档案！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("AIモデルファイルが指定されていません！", "注意", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
			else
			{
				MessageBox.Show("Seupt Model file first！", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool WriteUIValueToParam()
	{
		if (PredictMode)
		{
			INSP_AITOOL_PARAM aIPredictParam = Inspection.paramFile.AIPredictParam;
			aIPredictParam.ValueChangeFlag = true;
			aIPredictParam.EnableSelectFace = false;
			aIPredictParam.ParetoIndex_A = cb_ParetoChart_A.SelectedIndex;
			aIPredictParam.ParetoIndex_B = cb_ParetoChart_B.SelectedIndex;
			aIPredictParam.UseUNET = false;
			aIPredictParam.UseTensorRT = chk_EnableTensorRT.IsChecked.Value;
			if (lb_ModelName_A.Content.ToString() == "")
			{
				ShowModelAlarm(0);
				return false;
			}
			aIPredictParam.ModelName_A = lb_ModelName_A.Content.ToString();
			if (!CheckTextBox(txt_ModelWidth_A, txt_ModelHeight_A))
			{
				return false;
			}
			aIPredictParam.Width_A = Convert.ToInt32(txt_ModelWidth_A.Text);
			aIPredictParam.Height_A = Convert.ToInt32(txt_ModelHeight_A.Text);
			if (aIPredictParam.Width_A % 16 != 0 || aIPredictParam.Height_A % 16 != 0)
			{
				if (LanguageType == 1)
				{
					MessageBox.Show("AI模型尺寸必須為16的倍數。", "警告");
				}
				else if (LanguageType == 2)
				{
					MessageBox.Show("AI模型尺寸必须为16的倍数。", "警告");
				}
				else if (LanguageType == 3)
				{
					MessageBox.Show("AIモデルのサイズは16の倍数である必要があります。", "警告");
				}
				else
				{
					MessageBox.Show("The AI model size must be a multiple of 16.", "Warning");
				}
				return false;
			}
			aIPredictParam.ErrorBin_A0 = (FLOW_BIN)cb_ErrorBin_A0.SelectedIndex;
			aIPredictParam.ErrorBin_A1 = (FLOW_BIN)cb_ErrorBin_A1.SelectedIndex;
			aIPredictParam.ErrorBin_A2 = (FLOW_BIN)cb_ErrorBin_A2.SelectedIndex;
			aIPredictParam.ErrorBin_A3 = (FLOW_BIN)cb_ErrorBin_A3.SelectedIndex;
			if (chk_SelectFace.IsChecked == true && chk_SelectFace.IsChecked == true)
			{
				if (lb_ModelName_B.Content.ToString() == "")
				{
					ShowModelAlarm(0);
					return false;
				}
				aIPredictParam.ModelName_B = lb_ModelName_B.Content.ToString();
				if (!CheckTextBox(txt_ModelWidth_B, txt_ModelHeight_B))
				{
					return false;
				}
				aIPredictParam.Width_B = Convert.ToInt32(txt_ModelWidth_B.Text);
				aIPredictParam.Height_B = Convert.ToInt32(txt_ModelHeight_B.Text);
				if (aIPredictParam.Width_B % 16 != 0 || aIPredictParam.Height_B % 16 != 0)
				{
					if (LanguageType == 1)
					{
						MessageBox.Show("AI模型尺寸必須為16的倍數。", "警告");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show("AI模型尺寸必须为16的倍数。", "警告");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show("AIモデルのサイズは16の倍数である必要があります。", "警告");
					}
					else
					{
						MessageBox.Show("The AI model size must be a multiple of 16.", "Warning");
					}
					return false;
				}
				aIPredictParam.ErrorBin_B0 = (FLOW_BIN)cb_ErrorBin_B0.SelectedIndex;
				aIPredictParam.ErrorBin_B1 = (FLOW_BIN)cb_ErrorBin_B1.SelectedIndex;
				aIPredictParam.ErrorBin_B2 = (FLOW_BIN)cb_ErrorBin_B2.SelectedIndex;
				aIPredictParam.ErrorBin_B3 = (FLOW_BIN)cb_ErrorBin_B3.SelectedIndex;
				aIPredictParam.ParetoIndex_B = cb_ParetoChart_B.SelectedIndex;
			}
			Inspection.paramFile.AIPredictParam = aIPredictParam;
		}
		else
		{
			INSP_AITOOL_PARAM aIPredictParam = Inspection.paramFile.AIEnhanceParam;
			if (lb_ModelName_A.Content.ToString() == "")
			{
				ShowModelAlarm(0);
				return false;
			}
			aIPredictParam.ValueChangeFlag = true;
			aIPredictParam.ModelName_A = lb_ModelName_A.Content.ToString();
			aIPredictParam.UseUNET = true;
			aIPredictParam.UseTensorRT = chk_EnableTensorRT.IsChecked.Value;
			if (rad_DnetMode1A.IsChecked == true)
			{
				aIPredictParam.DnetMode_A = 0;
			}
			else if (rad_DnetMode2A.IsChecked == true)
			{
				aIPredictParam.DnetMode_A = 1;
			}
			else
			{
				aIPredictParam.DnetMode_A = 2;
			}
			if (chk_EnableAsignGPU.IsChecked == true)
			{
				aIPredictParam.EnableAsignGPU = true;
				aIPredictParam.GPUID = cb_GPUID.SelectedIndex;
			}
			else
			{
				aIPredictParam.EnableAsignGPU = false;
				aIPredictParam.GPUID = 0;
			}
			if (!CheckTextBox(txt_ModelWidth_A, txt_ModelHeight_A))
			{
				return false;
			}
			aIPredictParam.Width_A = Convert.ToInt32(txt_ModelWidth_A.Text);
			aIPredictParam.Height_A = Convert.ToInt32(txt_ModelHeight_A.Text);
			if (aIPredictParam.Width_A % 16 != 0 || aIPredictParam.Height_A % 16 != 0)
			{
				if (LanguageType == 1)
				{
					MessageBox.Show("AI模型尺寸必須為16的倍數。", "警告");
				}
				else if (LanguageType == 2)
				{
					MessageBox.Show("AI模型尺寸必须为16的倍数。", "警告");
				}
				else if (LanguageType == 3)
				{
					MessageBox.Show("AIモデルのサイズは16の倍数である必要があります。", "警告");
				}
				else
				{
					MessageBox.Show("The AI model size must be a multiple of 16.", "Warning");
				}
				aIPredictParam.Width_A = aIPredictParam.Width_A / 16 * 16;
				aIPredictParam.Height_A = aIPredictParam.Height_A / 16 * 16;
			}
			aIPredictParam.EnableCustomRGBChannel = cb_EnableCustumRGBChannel.IsChecked.Value;
			aIPredictParam.CustomChannelR = cb_Channel_R.SelectedIndex;
			aIPredictParam.CustomChannelG = cb_Channel_G.SelectedIndex;
			aIPredictParam.CustomChannelB = cb_Channel_B.SelectedIndex;
			if (chk_SelectFace.IsChecked == true)
			{
				if (lb_ModelName_B.Content.ToString() == "")
				{
					ShowModelAlarm(0);
					return false;
				}
				aIPredictParam.ValueChangeFlag = true;
				aIPredictParam.ModelName_B = lb_ModelName_B.Content.ToString();
				aIPredictParam.UseUNET = true;
				if (rad_DnetMode1B.IsChecked == true)
				{
					aIPredictParam.DnetMode_B = 0;
				}
				else if (rad_DnetMode2B.IsChecked == true)
				{
					aIPredictParam.DnetMode_B = 1;
				}
				else
				{
					aIPredictParam.DnetMode_B = 2;
				}
				if (!CheckTextBox(txt_ModelWidth_B, txt_ModelHeight_B))
				{
					return false;
				}
				aIPredictParam.Width_B = Convert.ToInt32(txt_ModelWidth_B.Text);
				aIPredictParam.Height_B = Convert.ToInt32(txt_ModelHeight_B.Text);
				if (aIPredictParam.Width_B % 16 != 0 || aIPredictParam.Height_B % 16 != 0)
				{
					if (LanguageType == 1)
					{
						MessageBox.Show("AI模型尺寸必須為16的倍數。", "警告");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show("AI模型尺寸必须为16的倍数。", "警告");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show("AIモデルのサイズは16の倍数である必要があります。", "警告");
					}
					else
					{
						MessageBox.Show("The AI model size must be a multiple of 16.", "Warning");
					}
					return false;
				}
			}
			Inspection.paramFile.AIEnhanceParam = aIPredictParam;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Wnd_Main_Closing(object sender, CancelEventArgs e)
	{
		if (!WriteUIValueToParam())
		{
			return;
		}
		if (PredictMode)
		{
			for (int i = 0; i < 8; i++)
			{
				if (StationIndex != i && !(Station[i].paramFile.AIPredictParam.ModelName_A == "") && Station[i].paramFile.AIPredictParam.ModelName_A == Inspection.paramFile.AIPredictParam.ModelName_A && (Station[i].paramFile.AIPredictParam.ErrorBin_A0 != Inspection.paramFile.AIPredictParam.ErrorBin_A0 || Station[i].paramFile.AIPredictParam.ErrorBin_A1 != Inspection.paramFile.AIPredictParam.ErrorBin_A1 || Station[i].paramFile.AIPredictParam.ErrorBin_A2 != Inspection.paramFile.AIPredictParam.ErrorBin_A2 || Station[i].paramFile.AIPredictParam.ErrorBin_A3 != Inspection.paramFile.AIPredictParam.ErrorBin_A3))
				{
					MessageBoxResult messageBoxResult = MessageBoxResult.Yes;
					messageBoxResult = ((LanguageType == 1) ? MessageBox.Show(string.Format("第{0}站與第{1}站使用相同模型，模型輸出設定方式卻不相同，是否設為相同?", i + 1, StationIndex + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show(string.Format("第{0}站与第{1}站使用相同模型，模型输出设置方式却不同，是否设置为相同?", i + 1, StationIndex + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show(string.Format("Station {0} and Station {1} use the same model, but the model output setting methods are different. Do you want to set them the same?", i + 1, StationIndex + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show(string.Format("第{0}駅と第{1}駅は同じモデルを使用していますが、モデル出力設定方法は異なります。同じ設定にしますか?", i + 1, StationIndex + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
					if (messageBoxResult == MessageBoxResult.Yes)
					{
						Inspection.paramFile.AIPredictParam.ErrorBin_A0 = Station[i].paramFile.AIPredictParam.ErrorBin_A0;
						Inspection.paramFile.AIPredictParam.ErrorBin_A1 = Station[i].paramFile.AIPredictParam.ErrorBin_A1;
						Inspection.paramFile.AIPredictParam.ErrorBin_A2 = Station[i].paramFile.AIPredictParam.ErrorBin_A2;
						Inspection.paramFile.AIPredictParam.ErrorBin_A3 = Station[i].paramFile.AIPredictParam.ErrorBin_A3;
					}
					break;
				}
			}
			return;
		}
		for (int j = 0; j < 8; j++)
		{
			if (StationIndex == j || Station[j].paramFile.AIEnhanceParam.ModelName_A == "" || !(Station[j].paramFile.AIEnhanceParam.ModelName_A == Inspection.paramFile.AIEnhanceParam.ModelName_A) || Station[j].paramFile.AIEnhanceParam.DnetMode_A == Inspection.paramFile.AIEnhanceParam.DnetMode_A)
			{
				continue;
			}
			MessageBoxResult messageBoxResult2 = MessageBoxResult.Yes;
			messageBoxResult2 = ((LanguageType == 1) ? MessageBox.Show(string.Format("第{0}站與第{1}站使用相同模型，模型輸出設定方式卻不相同，是否設為相同?", j + 1, StationIndex + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show(string.Format("第{0}站与第{1}站使用相同模型，模型输出设置方式却不同，是否设置为相同?", j + 1, StationIndex + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show(string.Format("Station {0} and Station {1} use the same model, but the model output setting methods are different. Do you want to set them the same?", j + 1, StationIndex + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show(string.Format("第{0}駅と第{1}駅は同じモデルを使用していますが、モデル出力設定方法は異なります。同じ設定にしますか?", j + 1, StationIndex + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult2 == MessageBoxResult.Yes)
			{
				MessageBoxResult messageBoxResult3 = MessageBoxResult.Yes;
				messageBoxResult3 = ((Station[j].paramFile.AIEnhanceParam.DnetMode_A == 0) ? ((LanguageType == 1) ? MessageBox.Show(string.Format("第{0}站模型標註方式為DNET，是否以此站數值為主?", j + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show(string.Format("第{0}站模型标注方式为DNET，是否以此站数值为主?", j + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show(string.Format("The model annotation method for station {0} is DNET. Do you want to use the value of this station as the main value?", j + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show(string.Format("第{0}番目の駅のモデルはDNETで注釈されています。この駅の値を基準にしますか？", j + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)))) : ((LanguageType == 1) ? MessageBox.Show(string.Format("第{0}站模型標註方式為DNET III，是否以此站數值為主?", j + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show(string.Format("第{0}站模型标注方式为DNET III，是否以此站数值为主?", j + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show(string.Format("The model annotation method for the {0}th station is DNET III. Do you want to use the value for this station?", j + 1), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show(string.Format("第 {0} 站モデル標注方式は DNET III です。この站の値を使用しますか?", j + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)))));
				if (messageBoxResult3 == MessageBoxResult.Yes)
				{
					Inspection.paramFile.AIEnhanceParam.DnetMode_A = Station[j].paramFile.AIEnhanceParam.DnetMode_A;
				}
				else
				{
					Station[j].paramFile.AIEnhanceParam.DnetMode_A = Inspection.paramFile.AIEnhanceParam.DnetMode_A;
				}
			}
			break;
		}
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		ContinueRun = false;
		AILoadingPauseEvent.Set();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetModel_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (button.Uid == "A")
		{
			ReloadModel[0] = true;
		}
		else
		{
			ReloadModel[1] = true;
		}
		ModelsWindow modelsWindow = new ModelsWindow();
		modelsWindow.LoadLanguage(LanguageType);
		if (ReloadModel[0])
		{
			modelsWindow.SetCurrentModelName(lb_ModelName_A.Content.ToString());
		}
		else
		{
			modelsWindow.SetCurrentModelName(lb_ModelName_B.Content.ToString());
		}
		modelsWindow.SetThroghputFile(ThroughputFile);
		modelsWindow.SetInspection(Station, PredictMode);
		modelsWindow.ShowDialog();
		EditHashTag = modelsWindow.EditHashTag;
		string selectModelName = modelsWindow.GetSelectModelName();
		if (selectModelName == "")
		{
			txt_ModelPath_A.Text = "";
			lb_ModelName_A.Content = selectModelName;
			lb_ModelName_A.Background = Brushes.Red;
		}
		else
		{
			string uid = button.Uid;
			if (!(uid == "A"))
			{
				if (uid == "B")
				{
					txt_ModelPath_B.Text = string.Format("Model\\{0}", selectModelName);
					lb_ModelName_B.Content = selectModelName;
					lb_ModelName_B.Background = Brushes.LimeGreen;
					if (PredictMode)
					{
						Inspection.aiPredictTools.ModelLoaded_B = false;
						Inspection.paramFile.AIPredictParam.ModelExist_B = true;
					}
					else
					{
						Inspection.aiEnhanceTools.ModelLoaded_B = false;
						Inspection.paramFile.AIEnhanceParam.ModelExist_B = true;
					}
				}
			}
			else
			{
				txt_ModelPath_A.Text = string.Format("Model\\{0}", selectModelName);
				lb_ModelName_A.Content = selectModelName;
				lb_ModelName_A.Background = Brushes.LimeGreen;
				if (PredictMode)
				{
					Inspection.aiPredictTools.ModelLoaded_A = false;
					Inspection.paramFile.AIPredictParam.ModelExist_A = true;
					Inspection.paramFile.AIPredictParam.ModelName_A = selectModelName;
				}
				else
				{
					Inspection.aiEnhanceTools.ModelLoaded_A = false;
					Inspection.paramFile.AIEnhanceParam.ModelExist_A = true;
					Inspection.paramFile.AIEnhanceParam.ModelName_A = selectModelName;
				}
			}
		}
		txt_ModelWidth_A.Background = Brushes.White;
		txt_ModelHeight_A.Background = Brushes.White;
		if (LanguageType == 1)
		{
			lab_SetModelSizeResult.Content = "請自行設置模型尺寸";
		}
		else if (LanguageType == 2)
		{
			lab_SetModelSizeResult.Content = "请自行设置模型尺寸";
		}
		else if (LanguageType == 3)
		{
			lab_SetModelSizeResult.Content = "モデルサイズを自分で設定してください";
		}
		else
		{
			lab_SetModelSizeResult.Content = "Please set the model size first.";
		}
		txt_ModelWidth_A.IsReadOnly = false;
		txt_ModelHeight_A.IsReadOnly = false;
		if (selectModelName == "")
		{
			return;
		}
		ModelName = selectModelName;
		if (rad_DnetMode1A.IsChecked == true)
		{
			DNetMode = 0;
		}
		else if (rad_DnetMode2A.IsChecked == true)
		{
			DNetMode = 1;
		}
		else
		{
			DNetMode = 2;
		}
		if (new FileInfo(string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, selectModelName)).Length < 10)
		{
			if (PredictMode)
			{
				Inspection.paramFile.AIPredictParam.ModelName_A = selectModelName;
				Inspection.paramFile.AIPredictParam.NewModelMode = true;
				Inspection.aiPredictTools.ModelLoaded_A = false;
			}
			else
			{
				Inspection.paramFile.AIEnhanceParam.ModelName_A = selectModelName;
				Inspection.paramFile.AIEnhanceParam.NewModelMode = true;
				Inspection.aiEnhanceTools.ModelLoaded_A = false;
				Inspection.paramFile.AIEnhanceParam.LastModelFile_A = "";
			}
			ReloadModel[0] = false;
			if (LanguageType == 1)
			{
				lab_SetModelSizeResult.Content = "請自行設置模型尺寸";
			}
			else if (LanguageType == 2)
			{
				lab_SetModelSizeResult.Content = "请自行设置模型尺寸";
			}
			else if (LanguageType == 3)
			{
				lab_SetModelSizeResult.Content = "モデルサイズを自分で設定してください";
			}
			else
			{
				lab_SetModelSizeResult.Content = "Please set the model size first.";
			}
		}
		else
		{
			Action method = delegate
			{
				grd_AILoading.Visibility = Visibility.Visible;
				AILoadingPauseEvent.Set();
			};
			base.Dispatcher.BeginInvoke(method);
			Inspection.paramFile.AIEnhanceParam.LastModelFile_A = "";
		}
		if (PredictMode)
		{
			for (int num = 0; num < 8; num++)
			{
				if (StationIndex != num && !(Station[num].paramFile.AIPredictParam.ModelName_A == "") && Station[num].paramFile.AIPredictParam.ModelName_A == Inspection.paramFile.AIPredictParam.ModelName_A && (Station[num].paramFile.AIPredictParam.Width_A != Inspection.paramFile.AIPredictParam.Width_A || Station[num].paramFile.AIPredictParam.Height_A != Inspection.paramFile.AIPredictParam.Height_A || Station[num].paramFile.AIPredictParam.ErrorBin_A0 != Inspection.paramFile.AIPredictParam.ErrorBin_A0 || Station[num].paramFile.AIPredictParam.ErrorBin_A1 != Inspection.paramFile.AIPredictParam.ErrorBin_A1 || Station[num].paramFile.AIPredictParam.ErrorBin_A2 != Inspection.paramFile.AIPredictParam.ErrorBin_A2 || Station[num].paramFile.AIPredictParam.ErrorBin_A3 != Inspection.paramFile.AIPredictParam.ErrorBin_A3))
				{
					txt_ModelWidth_A.Text = Station[num].paramFile.AIEnhanceParam.Width_A.ToString();
					txt_ModelHeight_A.Text = Station[num].paramFile.AIEnhanceParam.Height_A.ToString();
					cb_ErrorBin_A0.SelectedIndex = (int)Station[num].paramFile.AIPredictParam.ErrorBin_A0;
					cb_ErrorBin_A1.SelectedIndex = (int)Station[num].paramFile.AIPredictParam.ErrorBin_A1;
					cb_ErrorBin_A2.SelectedIndex = (int)Station[num].paramFile.AIPredictParam.ErrorBin_A2;
					cb_ErrorBin_A3.SelectedIndex = (int)Station[num].paramFile.AIPredictParam.ErrorBin_A3;
					break;
				}
			}
			return;
		}
		for (int num2 = 0; num2 < 8; num2++)
		{
			if (StationIndex == num2 || Station[num2].paramFile.AIEnhanceParam.ModelName_A == "" || !(Station[num2].paramFile.AIEnhanceParam.ModelName_A == Inspection.paramFile.AIEnhanceParam.ModelName_A))
			{
				continue;
			}
			for (int num3 = 0; num3 < 6; num3++)
			{
				Inspection.paramFile.DrawCaition[num3] = Station[num2].paramFile.DrawCaition[num3];
			}
			if (Station[num2].paramFile.AIEnhanceParam.DnetMode_A != Inspection.paramFile.AIEnhanceParam.DnetMode_A)
			{
				switch (Station[num2].paramFile.AIEnhanceParam.DnetMode_A)
				{
				case 0:
					rad_DnetMode1A.IsChecked = true;
					break;
				case 1:
					rad_DnetMode2A.IsChecked = true;
					break;
				case 2:
					rad_DnetMode3A.IsChecked = true;
					break;
				default:
					rad_DnetMode1A.IsChecked = true;
					break;
				}
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckTextBox(TextBox width, TextBox height)
	{
		bool result = true;
		if (!width.Text.Contains(":"))
		{
			if (!IsNumeric(width.Text))
			{
				width.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				width.Background = Brushes.White;
			}
		}
		if (!height.Text.Contains(":"))
		{
			if (!IsNumeric(height.Text))
			{
				height.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				height.Background = Brushes.White;
			}
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
		catch
		{
			MessageBox.Show("輸入格式錯誤-必須為整數", "錯誤");
			return false;
		}
	}

	private void Chk_SelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (!DontUpdateUI)
		{
			FaceSwitch(chk_SelectFace.IsChecked.Value);
		}
	}

	private void FaceSwitch(bool enable)
	{
		if (chk_SelectFace.IsChecked == true)
		{
			grd_FaceB.Visibility = Visibility.Visible;
			wnd_Main.Width = 1340.0;
			if (Inspection.paramFile.SelectFaceParam[1].TrackerON && Inspection.paramFile.SelectFaceON)
			{
				grd_FaceB.Visibility = Visibility.Visible;
			}
			else
			{
				grd_FaceB.Visibility = Visibility.Hidden;
			}
		}
		else
		{
			grd_FaceB.Visibility = Visibility.Hidden;
			wnd_Main.Width = 690.0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RevrseModelA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		int num = 0;
		if (obj.Uid == "B")
		{
			num = 1;
		}
		if (!PredictMode)
		{
			string text = ((num != 0) ? Inspection.paramFile.AIEnhanceParam.LastModelFile_B : Inspection.paramFile.AIEnhanceParam.LastModelFile_A);
			string fileName = System.IO.Path.GetFileName(text);
			if (text == "" || !File.Exists(text))
			{
				return;
			}
			string text2 = ((num != 0) ? Inspection.paramFile.AIEnhanceParam.ModelName_B : Inspection.paramFile.AIEnhanceParam.ModelName_A);
			string text3 = (text3 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, text2));
			MessageBoxResult messageBoxResult;
			if (LanguageType == 1)
			{
				messageBoxResult = MessageBox.Show("AI模型將還原至訓練前版本！" + fileName, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 2)
			{
				messageBoxResult = MessageBox.Show("AI模型將還原至訓練前版本！" + fileName, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			messageBoxResult = ((LanguageType != 3) ? MessageBox.Show("AI Model will be reverse to last version！" + fileName, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("AIモデルはトレーニング前のバージョンに復元されます！" + fileName, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes));
			if (messageBoxResult != MessageBoxResult.Yes || !File.Exists(text))
			{
				return;
			}
			DateTime lastWriteTime = new FileInfo(text3).LastWriteTime;
			string text4 = string.Format("{0}Model\\backup\\{1}{2}", AppDomain.CurrentDomain.BaseDirectory, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), text2);
			string text5 = "";
			text5 = System.IO.Path.GetFileNameWithoutExtension(text2) + ".pbd";
			string text6 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, text5);
			if (File.Exists(text6))
			{
				FileInfo fileInfo = new FileInfo(text6);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text6);
			}
			MoveFile(text3, text4);
			MoveFile(text, text3);
			if (num == 0)
			{
				Inspection.paramFile.AIEnhanceParam.Model_A_Changed = true;
				Inspection.paramFile.AIEnhanceParam.LastModelFile_A = text4;
			}
			else
			{
				Inspection.paramFile.AIEnhanceParam.Model_B_Changed = true;
				Inspection.paramFile.AIEnhanceParam.LastModelFile_B = text4;
			}
			ReloadModel[num] = true;
			if (LanguageType == 1)
			{
				MessageBox.Show(string.Format("AI模型({0})還原成功！", fileName), "警告");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show(string.Format("AI模型({0})还原成功！", fileName), "警告");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show(string.Format("AIモデル({0})の復元が成功しました！", fileName), "警告");
			}
			else
			{
				MessageBox.Show(string.Format("AI model ({0}) restored successfully!", fileName), "Warning");
			}
			return;
		}
		string text7 = ((num != 0) ? Inspection.paramFile.AIPredictParam.LastModelFile_B : Inspection.paramFile.AIPredictParam.LastModelFile_A);
		string fileName2 = System.IO.Path.GetFileName(text7);
		if (text7 == "" || !File.Exists(text7))
		{
			return;
		}
		string text8 = ((num != 0) ? Inspection.paramFile.AIPredictParam.ModelName_B : Inspection.paramFile.AIPredictParam.ModelName_A);
		string text9 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, text8);
		MessageBoxResult messageBoxResult2 = ((LanguageType == 1) ? MessageBox.Show("AI模型將還原至訓練前版本！" + fileName2, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("AI模型将还原至训练前版本！" + fileName2, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("The AI model will be restored to the pre-training version!" + fileName2, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("AIモデルがトレーニング前のバージョンに復元されます！" + fileName2, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult2 != MessageBoxResult.Yes || !File.Exists(text7))
		{
			return;
		}
		DateTime lastWriteTime2 = new FileInfo(text9).LastWriteTime;
		string text10 = string.Format("{0}Model\\backup\\{1}{2}", AppDomain.CurrentDomain.BaseDirectory, lastWriteTime2.ToString("yyyy-MM-dd-HHmm_"), Inspection.paramFile.AIPredictParam.ModelName_A);
		string arg = System.IO.Path.GetFileNameWithoutExtension(text8) + ".pb";
		string text11 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg);
		if (File.Exists(text11))
		{
			FileInfo fileInfo2 = new FileInfo(text11);
			if (fileInfo2.IsReadOnly)
			{
				fileInfo2.IsReadOnly = false;
			}
			File.Delete(text11);
		}
		MoveFile(text9, text10);
		MoveFile(text7, text9);
		if (num == 0)
		{
			Inspection.paramFile.AIPredictParam.Model_A_Changed = true;
			Inspection.paramFile.AIPredictParam.LastModelFile_A = text10;
		}
		else
		{
			Inspection.paramFile.AIPredictParam.Model_B_Changed = true;
			Inspection.paramFile.AIPredictParam.LastModelFile_B = text10;
		}
		ReloadModel[num] = true;
		if (LanguageType == 1)
		{
			MessageBox.Show(string.Format("AI模型({0})還原成功！", fileName2), "警告");
		}
		else if (LanguageType == 2)
		{
			MessageBox.Show(string.Format("AI模型({0})还原成功！", fileName2), "警告");
		}
		else if (LanguageType == 3)
		{
			MessageBox.Show(string.Format("AIモデル({0})の復元が成功しました！", fileName2), "警告");
		}
		else
		{
			MessageBox.Show(string.Format("AI model ({0}) restored successfully!", fileName2), "Warning");
		}
	}

	private bool MoveFile(string source, string dest)
	{
		try
		{
			if (File.Exists(source))
			{
				FileInfo fileInfo = new FileInfo(source);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				if (File.Exists(dest))
				{
					FileInfo fileInfo2 = new FileInfo(dest);
					if (fileInfo2.IsReadOnly)
					{
						fileInfo2.IsReadOnly = false;
					}
					File.Delete(dest);
				}
				File.Move(source, dest);
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	private void rad_DnetMode1A_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void rad_DnetMode3A_Checked(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_CreateModelA_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("是否重新建立一個新的AI模型?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("是否重新建立一个新的AI模型?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 0) ? MessageBox.Show("Do you want to rebuild a new AI model?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("新しいAIモデルを再構築しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		if (LanguageType == 1)
		{
			itemInputWindow.SetTitle("請輸入欲建立的AI模型名稱 (不需包含子檔名.h5)");
		}
		else if (LanguageType == 2)
		{
			itemInputWindow.SetTitle("请输入欲建立的AI模型名称 (无需包含子文件名.h5)");
		}
		else if (LanguageType == 3)
		{
			itemInputWindow.SetTitle("AIモデル名を入力してください (サブファイル名.h5は不要です)");
		}
		else
		{
			itemInputWindow.SetTitle("Please enter the name of the AI model you want to create (without the sub-filename .h5)");
		}
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr == "")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("檔名不得為空白!", "Warning");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("文件名不得为空!", "警告");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("ファイル名は空白にしてはいけません", "警告");
			}
			else
			{
				MessageBox.Show("The file name cannot be blank!", "Warning");
			}
			return;
		}
		string text = itemInputWindow.InputStr;
		if (!text.ToLower().Contains(".h5"))
		{
			text += ".h5";
		}
		string uid = ((Button)sender).Uid;
		if (!(uid == "A"))
		{
			if (uid == "B")
			{
				if (PredictMode)
				{
					Inspection.paramFile.AIPredictParam.ModelName_B = text;
					Inspection.paramFile.AIPredictParam.NewModelMode = true;
					Inspection.aiPredictTools.ModelLoaded_B = false;
				}
				else
				{
					Inspection.paramFile.AIEnhanceParam.ModelName_B = text;
					Inspection.paramFile.AIEnhanceParam.NewModelMode = true;
					Inspection.aiEnhanceTools.ModelLoaded_B = false;
				}
				txt_ModelPath_B.Text = string.Format("Model\\{0}", text);
				lb_ModelName_B.Content = text;
				lb_ModelName_B.Background = Brushes.LimeGreen;
				Inspection.aiEnhanceTools.ModelLoaded_A = false;
				ReloadModel[1] = false;
			}
		}
		else
		{
			if (PredictMode)
			{
				Inspection.paramFile.AIPredictParam.ModelName_A = text;
				Inspection.paramFile.AIPredictParam.NewModelMode = true;
				Inspection.aiPredictTools.ModelLoaded_A = false;
			}
			else
			{
				Inspection.paramFile.AIEnhanceParam.ModelName_A = text;
				Inspection.paramFile.AIEnhanceParam.NewModelMode = true;
				Inspection.aiEnhanceTools.ModelLoaded_A = false;
			}
			txt_ModelPath_A.Text = string.Format("Model\\{0}", text);
			lb_ModelName_A.Content = text;
			lb_ModelName_A.Background = Brushes.LimeGreen;
			ReloadModel[0] = false;
		}
		if (chk_SelectFace.IsChecked != true)
		{
			return;
		}
		if (PredictMode)
		{
			if (Inspection.paramFile.AIPredictParam.ModelName_A == Inspection.paramFile.AIPredictParam.ModelName_B && Inspection.paramFile.AIPredictParam.ModelName_A != "")
			{
				if (LanguageType == 1)
				{
					MessageBox.Show("A面與B面Model不得相同，請重新選擇或建立B面Model", "警告");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("A面与B面Model不得相同，请重新选择或建立B面Model", "警告");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("A面とB面のModelは同じではありません。B面のModelを再選択または作成してください", "警告");
				}
				else
				{
					MessageBox.Show("A-side and B-side models cannot be the same. Please reselect or create a B-side model", "Warning");
				}
				Inspection.aiPredictTools.ModelLoaded_B = false;
				Inspection.paramFile.AIPredictParam.ModelExist_B = false;
				txt_ModelPath_B.Text = "";
				lb_ModelName_B.Content = "";
				lb_ModelName_B.Background = Brushes.Red;
			}
		}
		else if (Inspection.paramFile.AIEnhanceParam.ModelName_A == Inspection.paramFile.AIEnhanceParam.ModelName_B && Inspection.paramFile.AIEnhanceParam.ModelName_A != "")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("A面與B面Model不得相同，請重新選擇或建立B面Model", "警告");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("A面与B面Model不得相同，请重新选择或建立B面Model", "警告");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("A面とB面のModelは同じにすることができません。B面Modelを再度選択するか、新規作成してください", "警告");
			}
			else
			{
				MessageBox.Show("A面 and B面 Model cannot be the same. Please re-select or create a B面 Model", "Warning");
			}
			Inspection.aiEnhanceTools.ModelLoaded_B = false;
			Inspection.paramFile.AIEnhanceParam.ModelExist_B = false;
			txt_ModelPath_B.Text = "";
			lb_ModelName_B.Content = "";
			lb_ModelName_B.Background = Brushes.Red;
		}
	}

	private void Rad_DnetMode1B_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void Rad_DnetMode3B_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void Chk_EnableTensorRT_Click(object sender, RoutedEventArgs e)
	{
		ReloadModel[0] = true;
		Inspection.paramFile.AIEnhanceParam.Model_A_Changed = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool AutoGetModelSize()
	{
		if (ModelName == "")
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("請先選擇AI模型!");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("请先选择AI模型!");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("AIモデルを選択してください!");
			}
			else
			{
				MessageBox.Show("Please select an AI model first!");
			}
			return false;
		}
		if (!CreateIni(DNetMode))
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("AI模型 Ini建立失敗!");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("AI模型初始化失敗!");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("AIモデルの初期化に失敗しました!");
			}
			else
			{
				MessageBox.Show("AI model initialization failed!");
			}
			return false;
		}
		if (!CopyModel(ModelName))
		{
			if (LanguageType == 1)
			{
				MessageBox.Show("AI模型拷貝失敗!");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("AI模型复制失败!");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("AIモデルのコピーに失敗しました!");
			}
			else
			{
				MessageBox.Show("AI model copy failed!");
			}
			return false;
		}
		string TrainingMsg = "";
		if (!RunReadModelScript(ref TrainingMsg, HideTrainingWnd: true))
		{
			MessageBox.Show(TrainingMsg);
			return false;
		}
		ModelWidth = 0;
		ModelHeight = 0;
		return ReadModelSizeIni(ref ModelWidth, ref ModelHeight);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ReadModelSizeIni(ref int width, ref int height)
	{
		HiniFile hiniFile = null;
		string text = System.IO.Path.Combine(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\train\\"), "shape.txt");
		if (!File.Exists(text))
		{
			return false;
		}
		hiniFile = new HiniFile(text, ";");
		width = hiniFile.GetInteger("Shape", "Width");
		height = hiniFile.GetInteger("Shape", "Height");
		if (width == 0 || height == 0)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CreateIni(int DnetMode)
	{
		HiniFile hiniFile = null;
		try
		{
			string text = System.IO.Path.Combine(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\"), "navs.ini");
			hiniFile = new HiniFile(text, ";");
			hiniFile.WriteString("DAVS_V3", "SegModelName", "Training\\Model\\navs.h5");
			hiniFile.WriteInt("DAVS_V3", "ImgWidth", 256);
			hiniFile.WriteInt("DAVS_V3", "ImgHeight", 256);
			hiniFile.WriteDouble("DAVS_V3", "AugRatio", 1.0);
			hiniFile.WriteInt("DAVS_V3", "Epoch", 8);
			hiniFile.WriteString("DAVS_V3", "PredPath", "");
			hiniFile.WriteString("DAVS_V3", "OutPath", "OutPath");
			if (DnetMode == 0 || DnetMode == 1)
			{
				hiniFile.WriteInt("DAVS_V3", "LossFunction", 1);
			}
			else
			{
				hiniFile.WriteInt("DAVS_V3", "LossFunction", 6);
			}
			hiniFile.WriteDouble("DAVS_V3", "LR", 0.001);
			hiniFile.WriteInt("DAVS_V3", "NGIgnore", 0);
			hiniFile.WriteInt("DAVS_V3", "OKBlank", 1);
			hiniFile.WriteInt("DAVS_V3", "Threshold", 220);
			hiniFile.WriteString("DAVS_V3", "ModelType", "US");
			hiniFile.WriteInt("DAVS_V3", "UnetDiv", 8);
			hiniFile.WriteInt("DAVS_V3", "BatchSize", 8);
			if (GetFileEncodeType(text) != Encoding.UTF8)
			{
				string value = File.ReadAllText(text, Encoding.Default);
				File.Delete(text);
				FileStream fileStream = File.Create(text);
				using (StreamWriter streamWriter = new StreamWriter(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
				{
					streamWriter.Write(value);
					streamWriter.WriteLine();
				}
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CreateIni()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CopyModel(string ModelName)
	{
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = System.IO.Path.Combine(path, ModelName);
			string text3 = System.IO.Path.Combine(text, "navs.h5");
			if (File.Exists(text3))
			{
				FileInfo fileInfo = new FileInfo(text3);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text3);
			}
			if (!File.Exists(text2))
			{
				return false;
			}
			File.Copy(text2, text3, overwrite: true);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CopyModel()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool RunReadModelScript(ref string TrainingMsg, bool HideTrainingWnd = false)
	{
		TrainingMsg = "";
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "ReadModel.bat");
		string path2 = string.Format("{0}Training\\train\\{1}", AppDomain.CurrentDomain.BaseDirectory, "shape.txt");
		string text = string.Format("{0}Training\\train\\train.exe", AppDomain.CurrentDomain.BaseDirectory);
		if (File.Exists(path2))
		{
			File.Delete(path2);
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = "ReadModel.bat";
		if (File.Exists(text))
		{
			DateTime lastWriteTime = new FileInfo(text).LastWriteTime;
			DateTime dateTime = new DateTime(2022, 3, 20);
			if (lastWriteTime <= dateTime)
			{
				if (LanguageType == 1)
				{
					TrainingMsg = "Train.exe舊版本不支援查詢Model尺寸，請更新Train.exe版本!";
				}
				else if (LanguageType == 2)
				{
					TrainingMsg = "Train.exe舊版本不支援查詢Model尺寸，請更新Train.exe版本!";
				}
				else if (LanguageType == 3)
				{
					TrainingMsg = "Train.exeの旧バージョンはModelサイズをクエリできません。Train.exeのバージョンを更新してください。";
				}
				else
				{
					TrainingMsg = "The old version of Train.exe does not support querying the Model size. Please update the Train.exe version!";
				}
				return false;
			}
		}
		string arg = string.Format("cd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V3 -r GetInputSize");
		if (!File.Exists(path))
		{
			using StreamWriter streamWriter = new StreamWriter(path, append: true);
			streamWriter.Write("{0}", arg);
			streamWriter.WriteLine();
		}
		if (!File.Exists(path))
		{
			TrainingMsg += string.Format("找不到訓練程式", processStartInfo.FileName);
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
			TrainingMsg = TrainingMsg + text2 + eOut;
		}
		process.WaitForExit();
		process.Close();
		return true;
	}

	public Encoding GetFileEncodeType(string filename)
	{
		using FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
		byte[] array = new BinaryReader(input).ReadBytes(2);
		if (array[0] >= 239)
		{
			if (array[0] == 239 && array[1] == 187)
			{
				return Encoding.UTF8;
			}
			if (array[0] == 254 && array[1] == byte.MaxValue)
			{
				return Encoding.BigEndianUnicode;
			}
			if (array[0] == byte.MaxValue && array[1] == 254)
			{
				return Encoding.Unicode;
			}
			return Encoding.Default;
		}
		return Encoding.Default;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Chk_EnableAsignGPU_Click(object sender, RoutedEventArgs e)
	{
		if (DontUpdateUI)
		{
			return;
		}
		if (chk_EnableAsignGPU.IsChecked == true)
		{
			PasswordWindow passwordWindow = new PasswordWindow();
			passwordWindow.SetPassWord("7678");
			passwordWindow.ShowDialog();
			if (!passwordWindow.GetLoginStatus())
			{
				chk_EnableAsignGPU.IsChecked = false;
				cb_GPUID.IsEnabled = false;
				cb_GPUID.SelectedIndex = 0;
			}
			else
			{
				cb_GPUID.IsEnabled = true;
			}
		}
		else
		{
			chk_EnableAsignGPU.IsChecked = false;
			cb_GPUID.IsEnabled = false;
			cb_GPUID.SelectedIndex = 0;
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/aitoolwindow.xaml", UriKind.Relative);
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
			wnd_Main = (AIToolWindow)target;
			wnd_Main.Closed += Window_Closed;
			wnd_Main.Closing += Wnd_Main_Closing;
			break;
		case 2:
			grd_FaceA = (Grid)target;
			break;
		case 3:
			txt_ModelPath_A = (TextBox)target;
			break;
		case 4:
			btn_SetModel_A = (Button)target;
			btn_SetModel_A.Click += Btn_SetModel_Click;
			break;
		case 5:
			txt_ModelWidth_A = (TextBox)target;
			break;
		case 6:
			txt_ModelHeight_A = (TextBox)target;
			break;
		case 7:
			lb_ModelName_A = (Label)target;
			break;
		case 8:
			lab_FaceA = (Label)target;
			break;
		case 9:
			grd_OutputA = (Grid)target;
			break;
		case 10:
			cb_ErrorBin_A0 = (ComboBox)target;
			break;
		case 11:
			lab_A0 = (Label)target;
			break;
		case 12:
			lab_Output = (Label)target;
			break;
		case 13:
			cb_ErrorBin_A1 = (ComboBox)target;
			break;
		case 14:
			lab_A1 = (Label)target;
			break;
		case 15:
			cb_ErrorBin_A2 = (ComboBox)target;
			break;
		case 16:
			lab_A2 = (Label)target;
			break;
		case 17:
			cb_ErrorBin_A3 = (ComboBox)target;
			break;
		case 18:
			lab_A3 = (Label)target;
			break;
		case 19:
			btn_RevrseModelA = (Button)target;
			btn_RevrseModelA.Click += Btn_RevrseModelA_Click;
			break;
		case 20:
			btn_CreateModelA = (Button)target;
			btn_CreateModelA.Click += Btn_CreateModelA_Click;
			break;
		case 21:
			lab_Pareto_A = (Label)target;
			break;
		case 22:
			cb_ParetoChart_A = (ComboBox)target;
			break;
		case 23:
			lab_SetModelSizeResult = (Label)target;
			break;
		case 24:
			grd_DNET = (Grid)target;
			break;
		case 25:
			rad_DnetMode1A = (RadioButton)target;
			rad_DnetMode1A.Checked += rad_DnetMode1A_Checked;
			break;
		case 26:
			rad_DnetMode2A = (RadioButton)target;
			rad_DnetMode2A.Checked += rad_DnetMode1A_Checked;
			break;
		case 27:
			rad_DnetMode3A = (RadioButton)target;
			rad_DnetMode3A.Checked += rad_DnetMode3A_Checked;
			break;
		case 28:
			rad_DnetMode15A = (RadioButton)target;
			rad_DnetMode15A.Checked += rad_DnetMode1A_Checked;
			break;
		case 29:
			grd_CustomChannel = (Grid)target;
			break;
		case 30:
			rt_1 = (Rectangle)target;
			break;
		case 31:
			cb_Channel_G = (ComboBox)target;
			break;
		case 32:
			cb_Channel_B = (ComboBox)target;
			break;
		case 33:
			cb_Channel_R = (ComboBox)target;
			break;
		case 34:
			lab_1 = (Label)target;
			break;
		case 35:
			cb_EnableCustumRGBChannel = (CheckBox)target;
			break;
		case 36:
			grd_Other = (Grid)target;
			break;
		case 37:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 38:
			chk_UseGPU = (RadioButton)target;
			break;
		case 39:
			lab_DNet = (Label)target;
			break;
		case 40:
			chk_EnableTensorRT = (CheckBox)target;
			chk_EnableTensorRT.Click += Chk_EnableTensorRT_Click;
			break;
		case 41:
			lab_NVCard = (Label)target;
			break;
		case 42:
			cb_GPUID = (ComboBox)target;
			break;
		case 43:
			chk_EnableAsignGPU = (CheckBox)target;
			chk_EnableAsignGPU.Click += Chk_EnableAsignGPU_Click;
			break;
		case 44:
			grd_FaceB = (Grid)target;
			break;
		case 45:
			txt_ModelPath_B = (TextBox)target;
			break;
		case 46:
			btn_SetModel_B = (Button)target;
			btn_SetModel_B.Click += Btn_SetModel_Click;
			break;
		case 47:
			txt_ModelWidth_B = (TextBox)target;
			break;
		case 48:
			txt_ModelHeight_B = (TextBox)target;
			break;
		case 49:
			lb_ModelName_B = (Label)target;
			break;
		case 50:
			lab_FaceB = (Label)target;
			break;
		case 51:
			grd_OutputB = (Grid)target;
			break;
		case 52:
			cb_ErrorBin_B0 = (ComboBox)target;
			break;
		case 53:
			lab_A4 = (Label)target;
			break;
		case 54:
			lab_Output1 = (Label)target;
			break;
		case 55:
			cb_ErrorBin_B1 = (ComboBox)target;
			break;
		case 56:
			lab_A5 = (Label)target;
			break;
		case 57:
			cb_ErrorBin_B2 = (ComboBox)target;
			break;
		case 58:
			lab_A6 = (Label)target;
			break;
		case 59:
			cb_ErrorBin_B3 = (ComboBox)target;
			break;
		case 60:
			lab_A7 = (Label)target;
			break;
		case 61:
			rad_DnetMode1B = (RadioButton)target;
			rad_DnetMode1B.Checked += Rad_DnetMode1B_Checked;
			break;
		case 62:
			rad_DnetMode2B = (RadioButton)target;
			rad_DnetMode2B.Checked += Rad_DnetMode1B_Checked;
			break;
		case 63:
			rad_DnetMode3B = (RadioButton)target;
			rad_DnetMode3B.Checked += Rad_DnetMode3B_Checked;
			break;
		case 64:
			btn_RevrseModelB = (Button)target;
			btn_RevrseModelB.Click += Btn_RevrseModelA_Click;
			break;
		case 65:
			btn_CreateModelB = (Button)target;
			btn_CreateModelB.Click += Btn_CreateModelA_Click;
			break;
		case 66:
			lab_Pareto_B = (Label)target;
			break;
		case 67:
			cb_ParetoChart_B = (ComboBox)target;
			break;
		case 68:
			grd_AILoading = (Grid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
