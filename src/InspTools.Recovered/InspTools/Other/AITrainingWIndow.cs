using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using SAMViewer;

namespace InspTools.Other;

public class AITrainingWIndow : Window, IComponentConnector
{
	public enum REMOTERUNFLAG
	{
		EM_Standby,
		EM_Running,
		EM_Finish
	}

	private int LanguageType = 1;

	private const int MaxStation = 8;

	private const int MaxModel = 8;

	private string[] X_Path = new string[8];

	private string[] Y_Path = new string[8];

	private string[] X_OK_Path = new string[8];

	private string[] X_NG_Path = new string[8];

	private TextBox[] BoxOKImgNum;

	private TextBox[] BoxNGImgNum;

	private TextBox[] BoxTrainingCount;

	private Button[] BtnSetAIParam;

	private Button[] BtnPreview;

	private Label[] LabOKTrain;

	private Label[] LabNGTrain;

	private Button[] BtnOpenOK;

	private Button[] BtnOpenNG;

	private string[] TrainingCount = new string[8];

	private CheckBox[] ChkStationEnable;

	private Grid[] CheckEnable;

	private Label[] LabModelName;

	private Label[] LabUNetModel;

	private Label[] LabTrainingStation;

	private Label[] LabTrainingCount;

	private Label[] LabStation;

	private Label[] LabProgress;

	private Label[] LabTestProgress;

	private Label[] LabAcc;

	private Label[] LabYield;

	private Label[] LabNewYield;

	private Label[] LabTime;

	private Label[] LabTotalTime;

	private Label[] LabRemote;

	private ProgressBar[] PBProgress;

	private ProgressBar[] PBTestProgress;

	private Grid[] StationGrid;

	private Grid[] GridResult;

	private List<double>[] ACCValue = new List<double>[8];

	private REMOTERUNFLAG RemoteRunFlag;

	private bool RemoteStatus;

	private bool[] RemoteModelUpdate = new bool[8];

	private DispatcherTimer UpdateRemoteUITImer = new DispatcherTimer();

	private List<string> RemoteModelName = new List<string>();

	public bool ResetProgress;

	private bool RemoteContinue;

	private List<int>[] AllModel = new List<int>[8];

	private SolidColorBrush[] ModelColor = new SolidColorBrush[8];

	private AI_MODE[] StationAIMode = new AI_MODE[8];

	private int ModelNumber;

	private int StationNumber;

	private int CurrentModel;

	private int CurrentStation;

	private CVInspection[] Station;

	private INSP_AITOOL_PARAM[] AI_Param = new INSP_AITOOL_PARAM[8];

	private ThroughputParamFile ThroughputFile;

	private bool UpdateUI;

	private bool HideTrainingWnd = true;

	private bool BufferRetest = true;

	private bool SaveImgUseDateForlder = true;

	private bool EnableMoveToTraninngImg;

	private int TestImageSet = 4;

	private bool AutoUpdateModel;

	private bool OfflineBufTestFinish;

	private HiniFile NavsIni;

	private HiniFile ConfigIni;

	private Thread AITrainingThread;

	private ManualResetEvent AITrainingPauseEvent = new ManualResetEvent(initialState: false);

	private bool ForceClose;

	private DispatcherTimer UpdateUITImer = new DispatcherTimer();

	private int[] CurrentModelPBValue = new int[8];

	private int[] CurrentPBTestValue = new int[8];

	private double[] CurrentAccValue = new double[8];

	private double[] CurrentYieldValue = new double[8];

	private double[] CurrentNewYieldValue = new double[8];

	private Button[] BTNUpdate = new Button[8];

	private string[] accstr = new string[8];

	private int EnableModelCount;

	private int EnableStationCount;

	private bool[] StationEnable = new bool[8];

	private bool[] ModelEnable = new bool[8];

	private bool[] ModelBypass = new bool[8];

	private bool[] ModelTrained = new bool[8];

	private double[] Yield = new double[8];

	private double[] NewYield = new double[8];

	private bool TrainingFinish;

	private int[] trainingImgOKCount = new int[8];

	private int[] trainingImgNGCount = new int[8];

	private string[] ModelTraingingLog = new string[8];

	public string TrainingDateTime = "2021";

	private string ProgramPath = "";

	public string TrainingMsg = "";

	private string SavePbScriptResult = "";

	private bool ShowLabel;

	private bool LastShowLabel;

	private bool InterruptMode;

	private bool RemoteMode;

	private DateTime TrainStartTime;

	private DateTime TrainSpanTime;

	private DateTime RemoteTrainLastTime;

	private int LastRemoteProcess;

	private int LastV;

	private string OldLogStr = "";

	private int LastProcess;

	private bool Train_LocalAndRemote;

	private bool Train_Local;

	private bool Train_Remote;

	private double MaybeArea = 0.3;

	private SAM mSam;

	internal AITrainingWIndow wnd_Main;

	internal Grid grd_Main;

	internal Grid grd_S1;

	internal Label lab_OKTrainingS1;

	internal TextBox txt_OKNumS1;

	internal Button btn_OpenOKS1;

	internal Label lab_S1;

	internal Label lab_NGTrainingS1;

	internal TextBox txt_NGNumS1;

	internal Button btn_OpenNGS1;

	internal Label lab_ModelS1;

	internal Grid grd_CheckS1;

	internal CheckBox chk_TrainingS1;

	internal TextBox txt_TrainingCountS1;

	internal Label lab_TrainingCountS1;

	internal Label lab_DNet_S1;

	internal Button btn_SetAIParam_1;

	internal Button btn_Training;

	internal CheckBox chk_HideTrainingWnd;

	internal CheckBox chk_EnableBufferRetest;

	internal ComboBox cb_TestImageSet;

	internal CheckBox chk_EnableBackupTrainingImg;

	internal CheckBox chk_AutoUpdateModel;

	internal CheckBox chk_EnableMoveToTrainingImg;

	internal CheckBox chk_SaveImgUseDateForlder;

	internal TextBox txt_TargetImageFolder;

	internal Label lab_SaveImgUseDateForlder;

	internal Button btn_Close_Copy;

	internal Button btn_RemoteTraining;

	internal Label lab_Training1;

	internal Label lab_Training2;

	internal Label lab_Training3;

	internal Label lab_Training4;

	internal Label lab_Training5;

	internal Label lab_Training6;

	internal Label lab_Training7;

	internal Label lab_Training8;

	internal Button btn_LocalAndRemoteTraining;

	internal Label lab_RemoteStatus;

	internal Button btn_SelectALL;

	internal Button btn_UnSelectALL;

	internal Button btn_TrainCount1;

	internal Button btn_TrainCount2;

	internal Button btn_TrainCount3;

	internal Button btn_TrainCount4;

	internal Button btn_TrainCount5;

	internal Button btn_TrainCount6;

	internal Label lab_SelectStation;

	internal Grid grd_S2;

	internal Label lab_OKTrainingS2;

	internal Label lab_S4;

	internal Label lab_NGTrainingS2;

	internal Label lab_ModelS2;

	internal Grid grd_CheckS2;

	internal CheckBox chk_TrainingS2;

	internal Label lab_TrainingCountS2;

	internal Button btn_OpenOKS2;

	internal TextBox txt_OKNumS2;

	internal TextBox txt_NGNumS2;

	internal TextBox txt_TrainingCountS2;

	internal Button btn_OpenNGS2;

	internal Label lab_DNet_S2;

	internal Button btn_SetAIParam_2;

	internal Grid grd_S3;

	internal Label lab_OKTrainingS3;

	internal Label lab_S5;

	internal Label lab_NGTrainingS3;

	internal Label lab_ModelS3;

	internal Grid grd_CheckS3;

	internal CheckBox chk_TrainingS3;

	internal Label lab_TrainingCountS3;

	internal Button btn_OpenOKS3;

	internal TextBox txt_OKNumS3;

	internal TextBox txt_NGNumS3;

	internal TextBox txt_TrainingCountS3;

	internal Button btn_OpenNGS3;

	internal Label lab_DNet_S3;

	internal Button btn_SetAIParam_3;

	internal Grid grd_S4;

	internal Label lab_OKTrainingS4;

	internal Label lab_S6;

	internal Label lab_NGTrainingS4;

	internal Label lab_ModelS4;

	internal Grid grd_CheckS4;

	internal CheckBox chk_TrainingS4;

	internal Label lab_TrainingCountS4;

	internal Button btn_OpenOKS4;

	internal TextBox txt_OKNumS4;

	internal TextBox txt_NGNumS4;

	internal TextBox txt_TrainingCountS4;

	internal Button btn_OpenNGS4;

	internal Label lab_DNet_S4;

	internal Button btn_SetAIParam_4;

	internal Grid grd_S5;

	internal Label lab_OKTrainingS5;

	internal Label lab_S7;

	internal Label lab_NGTrainingS5;

	internal Label lab_ModelS5;

	internal Grid grd_CheckS5;

	internal CheckBox chk_TrainingS5;

	internal Label lab_TrainingCountS5;

	internal TextBox txt_OKNumS5;

	internal Button btn_OpenOKS5;

	internal TextBox txt_NGNumS5;

	internal TextBox txt_TrainingCountS5;

	internal Button btn_OpenNGS5;

	internal Label lab_DNet_S5;

	internal Button btn_SetAIParam_5;

	internal Grid grd_S6;

	internal Label lab_OKTrainingS6;

	internal Label lab_S8;

	internal Label lab_NGTrainingS6;

	internal Label lab_ModelS6;

	internal Grid grd_CheckS6;

	internal CheckBox chk_TrainingS6;

	internal Label lab_TrainingCountS6;

	internal TextBox txt_OKNumS6;

	internal Button btn_OpenOKS6;

	internal TextBox txt_NGNumS6;

	internal TextBox txt_TrainingCountS6;

	internal Button btn_OpenNGS6;

	internal Label lab_DNet_S6;

	internal Button btn_SetAIParam_6;

	internal Grid grd_S7;

	internal Label lab_OKTrainingS7;

	internal Label lab_S9;

	internal Label lab_NGTrainingS7;

	internal Label lab_ModelS7;

	internal Grid grd_CheckS7;

	internal CheckBox chk_TrainingS7;

	internal Label lab_TrainingCountS7;

	internal TextBox txt_OKNumS7;

	internal Button btn_OpenOKS7;

	internal TextBox txt_NGNumS7;

	internal TextBox txt_TrainingCountS7;

	internal Button btn_OpenNGS7;

	internal Label lab_DNet_S7;

	internal Button btn_SetAIParam_7;

	internal Grid grd_S8;

	internal Label lab_OKTrainingS8;

	internal Label lab_S10;

	internal Label lab_NGTrainingS8;

	internal Label lab_ModelS8;

	internal Grid grd_CheckS8;

	internal CheckBox chk_TrainingS8;

	internal Label lab_TrainingCountS8;

	internal TextBox txt_OKNumS8;

	internal Button btn_OpenOKS8;

	internal TextBox txt_NGNumS8;

	internal TextBox txt_TrainingCountS8;

	internal Button btn_OpenNGS8;

	internal Label lab_DNet_S8;

	internal Button btn_SetAIParam_8;

	internal Grid grd_Loading;

	internal Grid grd_Result1;

	internal ProgressBar pb_Progress1;

	internal Label lab_Progress1;

	internal Label lab_acc1;

	internal Label lab_Yield1;

	internal Label lab_NewYield1;

	internal Button btn_Update1;

	internal ProgressBar pb_TestProgress1;

	internal Label lab_TestProgress1;

	internal Button btn_Preview1;

	internal Label lab_Time1;

	internal Label lab_TotalTime1;

	internal Label lab_RS1;

	internal Grid grd_Result2;

	internal Label lab_acc2;

	internal Label lab_Yield2;

	internal Label lab_NewYield2;

	internal Button btn_Update2;

	internal ProgressBar pb_Progress2;

	internal Label lab_Progress2;

	internal ProgressBar pb_TestProgress2;

	internal Label lab_TestProgress2;

	internal Button btn_Preview2;

	internal Label lab_Time2;

	internal Label lab_TotalTime2;

	internal Label lab_RS2;

	internal Grid grd_Result3;

	internal Label lab_acc3;

	internal Label lab_Yield3;

	internal Label lab_NewYield3;

	internal Button btn_Update3;

	internal ProgressBar pb_Progress3;

	internal Label lab_Progress3;

	internal ProgressBar pb_TestProgress3;

	internal Label lab_TestProgress3;

	internal Button btn_Preview3;

	internal Label lab_Time3;

	internal Label lab_TotalTime3;

	internal Label lab_RS3;

	internal Grid grd_Result4;

	internal Label lab_acc4;

	internal Label lab_Yield4;

	internal Label lab_NewYield4;

	internal Button btn_Update4;

	internal ProgressBar pb_Progress4;

	internal Label lab_Progress4;

	internal ProgressBar pb_TestProgress4;

	internal Label lab_TestProgress4;

	internal Button btn_Preview4;

	internal Label lab_Time4;

	internal Label lab_TotalTime4;

	internal Label lab_RemoteTraining;

	internal Label lab_RS4;

	internal Grid grd_Result5;

	internal Label lab_acc5;

	internal Label lab_Yield5;

	internal Label lab_NewYield5;

	internal Button btn_Update5;

	internal ProgressBar pb_Progress5;

	internal Label lab_Progress5;

	internal ProgressBar pb_TestProgress5;

	internal Label lab_TestProgress5;

	internal Button btn_Preview5;

	internal Label lab_Time5;

	internal Label lab_TotalTime5;

	internal Label lab_RS5;

	internal Grid grd_Result6;

	internal Label lab_acc6;

	internal Label lab_Yield6;

	internal Label lab_NewYield6;

	internal Button btn_Update6;

	internal ProgressBar pb_Progress6;

	internal Label lab_Progress6;

	internal ProgressBar pb_TestProgress6;

	internal Label lab_TestProgress6;

	internal Button btn_Preview6;

	internal Label lab_Time6;

	internal Label lab_TotalTime6;

	internal Label lab_RS6;

	internal Grid grd_Result7;

	internal Label lab_acc7;

	internal Label lab_Yield7;

	internal Label lab_NewYield7;

	internal Button btn_Update7;

	internal ProgressBar pb_Progress7;

	internal Label lab_Progress7;

	internal ProgressBar pb_TestProgress7;

	internal Label lab_TestProgress7;

	internal Button btn_Preview7;

	internal Label lab_Time7;

	internal Label lab_TotalTime7;

	internal Label lab_RS7;

	internal Grid grd_Result8;

	internal Label lab_acc8;

	internal Label lab_Yield8;

	internal Label lab_NewYield8;

	internal Button btn_Update8;

	internal ProgressBar pb_Progress8;

	internal Label lab_Progress8;

	internal ProgressBar pb_TestProgress8;

	internal Label lab_TestProgress8;

	internal Button btn_Preview8;

	internal Label lab_Time8;

	internal Label lab_TotalTime8;

	internal Label lab_RS8;

	internal Button btn_UpdateAll;

	internal Button btn_Close;

	internal TextBox txt_InspResult;

	internal TextBox txt_CmdResult;

	internal Button btn_Interrupt_S1;

	internal Label lab_Message;

	internal Label lab_Remote1;

	internal Label lab_Remote2;

	internal Label lab_Remote3;

	internal Label lab_Remote4;

	internal Label lab_Remote5;

	internal Label lab_Remote6;

	internal Label lab_Remote7;

	internal Label lab_Remote8;

	internal TextBox lab_RemoteMode;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public AITrainingWIndow()
	{
		InitializeComponent();
		BoxOKImgNum = new TextBox[8] { txt_OKNumS1, txt_OKNumS2, txt_OKNumS3, txt_OKNumS4, txt_OKNumS5, txt_OKNumS6, txt_OKNumS7, txt_OKNumS8 };
		BoxNGImgNum = new TextBox[8] { txt_NGNumS1, txt_NGNumS2, txt_NGNumS3, txt_NGNumS4, txt_NGNumS5, txt_NGNumS6, txt_NGNumS7, txt_NGNumS8 };
		BoxTrainingCount = new TextBox[8] { txt_TrainingCountS1, txt_TrainingCountS2, txt_TrainingCountS3, txt_TrainingCountS4, txt_TrainingCountS5, txt_TrainingCountS6, txt_TrainingCountS7, txt_TrainingCountS8 };
		BtnSetAIParam = new Button[8] { btn_SetAIParam_1, btn_SetAIParam_2, btn_SetAIParam_3, btn_SetAIParam_4, btn_SetAIParam_5, btn_SetAIParam_6, btn_SetAIParam_7, btn_SetAIParam_8 };
		BtnPreview = new Button[8] { btn_Preview1, btn_Preview2, btn_Preview3, btn_Preview4, btn_Preview5, btn_Preview6, btn_Preview7, btn_Preview8 };
		LabTrainingStation = new Label[8] { lab_Training1, lab_Training2, lab_Training3, lab_Training4, lab_Training5, lab_Training6, lab_Training7, lab_Training8 };
		LabTrainingCount = new Label[8] { lab_TrainingCountS1, lab_TrainingCountS2, lab_TrainingCountS3, lab_TrainingCountS4, lab_TrainingCountS5, lab_TrainingCountS6, lab_TrainingCountS7, lab_TrainingCountS8 };
		LabTime = new Label[8] { lab_Time1, lab_Time2, lab_Time3, lab_Time4, lab_Time5, lab_Time6, lab_Time7, lab_Time8 };
		LabTotalTime = new Label[8] { lab_TotalTime1, lab_TotalTime2, lab_TotalTime3, lab_TotalTime4, lab_TotalTime5, lab_TotalTime6, lab_TotalTime7, lab_TotalTime8 };
		ChkStationEnable = new CheckBox[8] { chk_TrainingS1, chk_TrainingS2, chk_TrainingS3, chk_TrainingS4, chk_TrainingS5, chk_TrainingS6, chk_TrainingS7, chk_TrainingS8 };
		CheckEnable = new Grid[8] { grd_CheckS1, grd_CheckS2, grd_CheckS3, grd_CheckS4, grd_CheckS5, grd_CheckS6, grd_CheckS7, grd_CheckS8 };
		LabModelName = new Label[8] { lab_ModelS1, lab_ModelS2, lab_ModelS3, lab_ModelS4, lab_ModelS5, lab_ModelS6, lab_ModelS7, lab_ModelS8 };
		LabUNetModel = new Label[8] { lab_DNet_S1, lab_DNet_S2, lab_DNet_S3, lab_DNet_S4, lab_DNet_S5, lab_DNet_S6, lab_DNet_S7, lab_DNet_S8 };
		LabStation = new Label[8] { lab_RS1, lab_RS2, lab_RS3, lab_RS4, lab_RS5, lab_RS6, lab_RS7, lab_RS8 };
		StationGrid = new Grid[8] { grd_S1, grd_S2, grd_S3, grd_S4, grd_S5, grd_S6, grd_S7, grd_S8 };
		LabProgress = new Label[8] { lab_Progress1, lab_Progress2, lab_Progress3, lab_Progress4, lab_Progress5, lab_Progress6, lab_Progress7, lab_Progress8 };
		LabTestProgress = new Label[8] { lab_TestProgress1, lab_TestProgress2, lab_TestProgress3, lab_TestProgress4, lab_TestProgress5, lab_TestProgress6, lab_TestProgress7, lab_TestProgress8 };
		LabAcc = new Label[8] { lab_acc1, lab_acc2, lab_acc3, lab_acc4, lab_acc5, lab_acc6, lab_acc7, lab_acc8 };
		LabYield = new Label[8] { lab_Yield1, lab_Yield2, lab_Yield3, lab_Yield4, lab_Yield5, lab_Yield6, lab_Yield7, lab_Yield8 };
		LabNewYield = new Label[8] { lab_NewYield1, lab_NewYield2, lab_NewYield3, lab_NewYield4, lab_NewYield5, lab_NewYield6, lab_NewYield7, lab_NewYield8 };
		LabRemote = new Label[8] { lab_Remote1, lab_Remote2, lab_Remote3, lab_Remote4, lab_Remote5, lab_Remote6, lab_Remote7, lab_Remote8 };
		PBProgress = new ProgressBar[8] { pb_Progress1, pb_Progress2, pb_Progress3, pb_Progress4, pb_Progress5, pb_Progress6, pb_Progress7, pb_Progress8 };
		PBTestProgress = new ProgressBar[8] { pb_TestProgress1, pb_TestProgress2, pb_TestProgress3, pb_TestProgress4, pb_TestProgress5, pb_TestProgress6, pb_TestProgress7, pb_TestProgress8 };
		GridResult = new Grid[8] { grd_Result1, grd_Result2, grd_Result3, grd_Result4, grd_Result5, grd_Result6, grd_Result7, grd_Result8 };
		BTNUpdate = new Button[8] { btn_Update1, btn_Update2, btn_Update3, btn_Update4, btn_Update5, btn_Update6, btn_Update7, btn_Update8 };
		LabNGTrain = new Label[8] { lab_NGTrainingS1, lab_NGTrainingS2, lab_NGTrainingS3, lab_NGTrainingS4, lab_NGTrainingS5, lab_NGTrainingS6, lab_NGTrainingS7, lab_NGTrainingS8 };
		LabOKTrain = new Label[8] { lab_OKTrainingS1, lab_OKTrainingS2, lab_OKTrainingS3, lab_OKTrainingS4, lab_OKTrainingS5, lab_OKTrainingS6, lab_OKTrainingS7, lab_OKTrainingS8 };
		BtnOpenOK = new Button[8] { btn_OpenOKS1, btn_OpenOKS2, btn_OpenOKS3, btn_OpenOKS4, btn_OpenOKS5, btn_OpenOKS6, btn_OpenOKS7, btn_OpenOKS8 };
		BtnOpenNG = new Button[8] { btn_OpenNGS1, btn_OpenNGS2, btn_OpenNGS3, btn_OpenNGS4, btn_OpenNGS5, btn_OpenNGS6, btn_OpenNGS7, btn_OpenNGS8 };
		ModelColor[0] = new SolidColorBrush(Color.FromRgb(126, 208, byte.MaxValue));
		ModelColor[1] = new SolidColorBrush(Color.FromRgb(253, 137, 137));
		ModelColor[2] = new SolidColorBrush(Color.FromRgb(211, 199, 253));
		ModelColor[3] = new SolidColorBrush(Color.FromRgb(34, 222, 214));
		ModelColor[4] = new SolidColorBrush(Color.FromRgb(208, 126, byte.MaxValue));
		ModelColor[5] = new SolidColorBrush(Color.FromRgb(253, 195, 137));
		ModelColor[6] = new SolidColorBrush(Color.FromRgb(253, 199, 199));
		ModelColor[7] = new SolidColorBrush(Color.FromRgb(58, 191, 100));
		for (int i = 0; i < 8; i++)
		{
			AI_Param[i] = default(INSP_AITOOL_PARAM);
			AllModel[i] = new List<int>();
			StationAIMode[i] = AI_MODE.EM_NoUse;
			CurrentPBTestValue[i] = 0;
			CurrentAccValue[i] = 0.0;
			CurrentYieldValue[i] = 0.0;
			CurrentNewYieldValue[i] = 0.0;
			accstr[i] = "";
			ACCValue[i] = new List<double>();
			LabRemote[i].Visibility = Visibility.Hidden;
		}
		for (int j = 0; j < 8; j++)
		{
			CurrentModelPBValue[j] = 0;
			ModelTraingingLog[j] = "";
		}
		AITrainingThread = new Thread(AITrainingRun);
		AITrainingThread.Start();
		UpdateUITImer.Interval = TimeSpan.FromMilliseconds(500.0);
		UpdateUITImer.Tick += UpdateUITImer_Tick;
		UpdateUITImer.Stop();
		UpdateRemoteUITImer.Interval = TimeSpan.FromMilliseconds(500.0);
		UpdateRemoteUITImer.Tick += UpdateRemoteUITImer_Tick;
		UpdateRemoteUITImer.Stop();
		grd_Loading.Visibility = Visibility.Hidden;
		UpdateUI = true;
		KillTask("train.exe");
		KillTask("main.exe");
		KillTask("h5_to_pb.exe");
		ProgramPath = AppDomain.CurrentDomain.BaseDirectory;
		lab_SelectStation.Visibility = Visibility.Hidden;
		btn_SelectALL.Visibility = Visibility.Hidden;
		btn_UnSelectALL.Visibility = Visibility.Hidden;
		lab_SaveImgUseDateForlder.Visibility = Visibility.Hidden;
		chk_SaveImgUseDateForlder.Visibility = Visibility.Hidden;
		txt_TargetImageFolder.Visibility = Visibility.Hidden;
		Thread.Sleep(200);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateRemoteUITImer_Tick(object sender, EventArgs e)
	{
		try
		{
			RemoteRunFlag = GetRemoteRunningStatus();
			if (RemoteRunFlag == REMOTERUNFLAG.EM_Running)
			{
				for (int i = 0; i < RemoteModelName.Count; i++)
				{
					string acc = "0";
					bool ReadSuccess = false;
					int remoteProgress = GetRemoteProgress(RemoteModelName[i], ref acc, ref ReadSuccess);
					double num = Convert.ToDouble(acc);
					string text = string.Format("{0:e3}", num);
					string path = string.Format("{0}Comm\\{1}\\model.finish", AppDomain.CurrentDomain.BaseDirectory, RemoteModelName[i]);
					DateTime now = DateTime.Now;
					for (int j = 0; j < 8; j++)
					{
						if (!(Path.GetFileNameWithoutExtension(AI_Param[j].ModelName_A) == RemoteModelName[i]))
						{
							continue;
						}
						if (ReadSuccess && LastProcess != remoteProgress)
						{
							int stationModeID = GetStationModeID(j);
							ACCValue[stationModeID].Add(num);
							LastProcess = remoteProgress;
						}
						if (File.Exists(path))
						{
							PBProgress[j].Value = 100.0;
							LabProgress[j].Content = "100";
							LabAcc[j].Content = text;
							accstr[j] = text;
							RemoteModelUpdate[i] = false;
							string path2 = string.Format("{0}Model\\{1}-TempFile.h5d", AppDomain.CurrentDomain.BaseDirectory, RemoteModelName[i]);
							string path3 = string.Format("{0}Model\\{1}-TempFile.pbd", AppDomain.CurrentDomain.BaseDirectory, RemoteModelName[i]);
							if (File.Exists(path2) && File.Exists(path3))
							{
								BTNUpdate[j].IsEnabled = true;
								if (AI_Param[j].UseTensorRT)
								{
									BtnPreview[j].IsEnabled = false;
								}
								else
								{
									BtnPreview[j].IsEnabled = true;
								}
								GridResult[j].Background = Brushes.LightGoldenrodYellow;
							}
						}
						else
						{
							PBProgress[j].Value = remoteProgress;
							LabProgress[j].Content = remoteProgress.ToString();
							LabAcc[j].Content = text;
						}
						if (LastRemoteProcess != remoteProgress && LastRemoteProcess != 0 && remoteProgress - LastRemoteProcess > 0)
						{
							TimeSpan timeSpan = TimeSpan.FromTicks(now.Subtract(RemoteTrainLastTime).Ticks / (remoteProgress - LastRemoteProcess) * (100 - remoteProgress));
							LabTime[j].Content = string.Format("剩餘時間: {0}時: {1}分: {2}秒", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
						}
					}
					if (LastRemoteProcess != remoteProgress)
					{
						LastRemoteProcess = remoteProgress;
						RemoteTrainLastTime = now;
					}
				}
			}
			else
			{
				if (RemoteRunFlag != REMOTERUNFLAG.EM_Finish)
				{
					return;
				}
				if (Train_LocalAndRemote)
				{
					if (TrainingFinish)
					{
						btn_UpdateAll.IsEnabled = true;
						btn_Interrupt_S1.IsEnabled = false;
						btn_Close.Content = "關閉";
					}
				}
				else
				{
					btn_UpdateAll.IsEnabled = true;
					btn_Interrupt_S1.IsEnabled = false;
					btn_Close.Content = "關閉";
				}
				for (int k = 0; k < RemoteModelName.Count; k++)
				{
					string acc2 = "0";
					bool ReadSuccess2 = false;
					int remoteProgress2 = GetRemoteProgress(RemoteModelName[k], ref acc2, ref ReadSuccess2);
					double num2 = Convert.ToDouble(acc2);
					string text2 = string.Format("{0:e3}", num2);
					string path4 = string.Format("{0}Comm\\{1}\\model.finish", AppDomain.CurrentDomain.BaseDirectory, RemoteModelName[k]);
					for (int l = 0; l < 8; l++)
					{
						if (!(Path.GetFileNameWithoutExtension(AI_Param[l].ModelName_A) == RemoteModelName[k]))
						{
							continue;
						}
						if (File.Exists(path4))
						{
							PBProgress[l].Value = 100.0;
							LabProgress[l].Content = "100";
							LabAcc[l].Content = text2;
							accstr[l] = text2;
							RemoteModelUpdate[k] = false;
							string path5 = string.Format("{0}Model\\{1}-TempFile.h5d", AppDomain.CurrentDomain.BaseDirectory, RemoteModelName[k]);
							string path6 = string.Format("{0}Model\\{1}-TempFile.pbd", AppDomain.CurrentDomain.BaseDirectory, RemoteModelName[k]);
							if (File.Exists(path5) && File.Exists(path6))
							{
								BTNUpdate[l].IsEnabled = true;
								if (AI_Param[l].UseTensorRT)
								{
									BtnPreview[l].IsEnabled = false;
								}
								else
								{
									BtnPreview[l].IsEnabled = true;
								}
								GridResult[l].Background = Brushes.LightGoldenrodYellow;
								if (Train_LocalAndRemote)
								{
									ModelTrained[k * 2] = true;
								}
								else
								{
									ModelTrained[k] = true;
								}
							}
						}
						else
						{
							PBProgress[l].Value = remoteProgress2;
							LabProgress[l].Content = remoteProgress2.ToString();
							LabAcc[l].Content = text2;
						}
					}
				}
				if (Train_LocalAndRemote)
				{
					if (TrainingFinish)
					{
						grd_Loading.Background = Brushes.LightGreen;
						lab_RemoteTraining.Content = "訓練完成";
					}
				}
				else
				{
					grd_Loading.Background = Brushes.LightGreen;
				}
				UpdateRemoteUITImer.Stop();
			}
		}
		catch (Exception ex)
		{
			string text3 = "AITrainingWindow-UpdateRemoteUITImer_Tick()";
			text3 += ex.ToString();
			if (OldLogStr != text3)
			{
				CVBuffer.SaveLog(text3);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateUITImer_Tick(object sender, EventArgs e)
	{
		if (TrainingFinish)
		{
			lab_Message.Visibility = Visibility.Hidden;
			btn_Interrupt_S1.IsEnabled = false;
			if (Train_LocalAndRemote)
			{
				if (RemoteRunFlag == REMOTERUNFLAG.EM_Finish)
				{
					grd_Loading.Background = Brushes.LightGreen;
					lab_RemoteTraining.Content = "訓練完成";
				}
				else
				{
					btn_Close.Content = "離開";
					btn_Close.Background = Brushes.Orange;
				}
			}
			else
			{
				grd_Loading.Background = Brushes.LightGreen;
			}
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				CurrentPBTestValue[i] = 100;
				PBProgress[i].Value = 100.0;
				PBTestProgress[i].Value = 100.0;
				int stationModeID = GetStationModeID(i);
				if (stationModeID >= 0 && ModelTrained[stationModeID])
				{
					BTNUpdate[i].IsEnabled = true;
					BtnPreview[i].IsEnabled = true;
					GridResult[i].Background = Brushes.LightGoldenrodYellow;
					num++;
				}
			}
			for (int j = 0; j < 8; j++)
			{
				if (ModelTrained[j])
				{
					continue;
				}
				for (int k = 0; k < AllModel[j].Count; k++)
				{
					if (!ModelBypass[j])
					{
						BTNUpdate[AllModel[j][k]].IsEnabled = false;
						BTNUpdate[AllModel[j][k]].Content = "失敗";
						BTNUpdate[AllModel[j][k]].Background = Brushes.Red;
					}
				}
			}
			if (num > 0)
			{
				if (Train_LocalAndRemote)
				{
					if (RemoteRunFlag == REMOTERUNFLAG.EM_Finish)
					{
						btn_UpdateAll.IsEnabled = true;
						btn_Interrupt_S1.IsEnabled = false;
						btn_Close.Content = "關閉";
					}
					else
					{
						btn_Close.Content = "離開";
						btn_Close.Background = Brushes.Orange;
					}
				}
				else
				{
					btn_UpdateAll.IsEnabled = true;
					btn_Interrupt_S1.IsEnabled = false;
					btn_Close.Content = "關閉";
				}
			}
			UpdateUITImer.Stop();
			return;
		}
		if (LastShowLabel != ShowLabel)
		{
			LastShowLabel = ShowLabel;
			if (ShowLabel)
			{
				lab_Message.Visibility = Visibility.Visible;
			}
			else
			{
				lab_Message.Visibility = Visibility.Hidden;
			}
		}
		int currentModel = CurrentModel;
		int num2 = 0;
		num2 = GetProgress(currentModel);
		if (num2 >= 0)
		{
			TrainSpanTime = DateTime.Now;
			for (int l = 0; l < AllModel[currentModel].Count; l++)
			{
				PBProgress[AllModel[currentModel][l]].Value = num2;
				LabProgress[AllModel[currentModel][l]].Content = num2.ToString();
				int num3 = AllModel[currentModel][l];
				bool flag = false;
				if (num3 >= 0 && num3 <= 8 && num2 > 0 && LastV != num2)
				{
					if (l == 0 && AllModel[currentModel].Count == 1)
					{
						LastV = num2;
						flag = true;
					}
					else if (l == AllModel[currentModel].Count - 1 && AllModel[currentModel].Count > 1)
					{
						LastV = num2;
						flag = true;
					}
					TimeSpan timeSpan = TimeSpan.FromTicks(TrainSpanTime.Subtract(TrainStartTime).Ticks * (100 - num2) / num2);
					LabTime[num3].Content = string.Format("剩餘時間: {0}時: {1}分: {2}秒", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
					timeSpan = TimeSpan.FromTicks(TrainSpanTime.Subtract(TrainStartTime).Ticks);
					LabTotalTime[num3].Content = string.Format("訓練時間: {0}時: {1}分: {2}秒", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
				}
				double num4 = 0.0;
				if (accstr[currentModel] != "")
				{
					num4 = Convert.ToDouble(accstr[currentModel]);
				}
				if (flag)
				{
					ACCValue[currentModel].Add(num4);
				}
				string.Format("{0:e3}", num4);
				LabAcc[AllModel[currentModel][l]].Foreground = Brushes.Red;
				LabAcc[AllModel[currentModel][l]].Content = string.Format("{0:e3}", num4);
			}
		}
		if (BufferRetest)
		{
			if (OfflineBufTestFinish)
			{
				num2 = 100;
				PBTestProgress[CurrentStation].Value = num2;
				LabTestProgress[CurrentStation].Content = num2.ToString();
				OfflineBufTestFinish = false;
			}
			else
			{
				num2 = GetTestProgress(CurrentStation);
				if (PBTestProgress[CurrentStation].Value < 100.0 && num2 >= 0)
				{
					PBTestProgress[CurrentStation].Value = num2;
					LabTestProgress[CurrentStation].Content = num2.ToString();
				}
			}
			LabYield[CurrentStation].Content = string.Format("{0:N1}", Yield[CurrentStation]);
			LabNewYield[CurrentStation].Content = string.Format("{0:N1}", NewYield[CurrentStation]);
		}
		txt_InspResult.Text = TrainingMsg;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AITrainingRun()
	{
		while (true)
		{
			AITrainingPauseEvent.WaitOne(-1);
			if (ForceClose)
			{
				UpdateUITImer.Stop();
				break;
			}
			UpdateUITImer.Start();
			for (int i = 0; i < 8; i++)
			{
				if (ForceClose)
				{
					UpdateUITImer.Stop();
					break;
				}
				if (i >= ModelNumber)
				{
					break;
				}
				if (ModelBypass[i])
				{
					continue;
				}
				ModelTrained[i] = false;
				if (!ModelEnable[i])
				{
					TrainingMsg += string.Format("[訓練中止] Model [{0}] 未啟用。\n", AI_Param[AllModel[i][0]].ModelName_A);
					continue;
				}
				if (GetStationTrainingImageCount(AllModel[i][0]) <= 0)
				{
					TrainingMsg += string.Format("[訓練中止] Model [{0}] 訓練影像 = 0。\n", AI_Param[AllModel[i][0]].ModelName_A);
					continue;
				}
				if (!CreateIni(i, AI_Param[AllModel[i][0]].DnetMode_A))
				{
					TrainingMsg += string.Format("[訓練中止] Model [{0}] 訓練ini檔案建立失敗。\n", AI_Param[AllModel[i][0]].ModelName_A);
					continue;
				}
				bool flag = false;
				if (!CopyModel(i))
				{
					TrainingMsg += string.Format("Model [{0}] 拷貝現有模型失敗(重新訓練新模型)。\n", AI_Param[AllModel[i][0]].ModelName_A);
				}
				flag = AI_Param[AllModel[i][0]].NewModelMode;
				CurrentModel = i;
				TrainStartTime = DateTime.Now;
				TrainStartTime.AddSeconds(20.0);
				TrainingMsg += string.Format("\nModel [{0}] 訓練開始。\n", AI_Param[AllModel[i][0]].ModelName_A);
				RunTrainingScript(flag, AllModel[i][0], AI_Param[AllModel[i][0]].DnetMode_A);
				Thread.Sleep(1000);
				if (CurrentModelPBValue[i] < 80 && !InterruptMode)
				{
					TrainingMsg += string.Format("\nModel [{0}] 訓練失敗。(訓練進度小於80%，請檢查訓練程式LOG)\n", AI_Param[AllModel[i][0]].ModelName_A);
					continue;
				}
				DelTempProcFile(ClearRemote: false);
				TrainingMsg += string.Format("Model [{0}] 刪除訓練進程檔案。\n\n", AI_Param[AllModel[i][0]].ModelName_A);
				if (ForceClose)
				{
					TrainingMsg += string.Format("[訓練中斷] 程式強制關閉(1)。\n");
					break;
				}
				TrainingMsg += string.Format("Model [{0}] 訓練完成。\n\n", AI_Param[AllModel[i][0]].ModelName_A);
				if (!CopyNewModel(i))
				{
					TrainingMsg += string.Format("Model [{0}] 模型拷貝失敗(拷貝訓練完成模型到模型資料夾)。\n", AI_Param[AllModel[i][0]].ModelName_A);
					continue;
				}
				for (int j = 0; j < AllModel[i].Count; j++)
				{
					if (ForceClose)
					{
						TrainingMsg += string.Format("[訓練中斷] 程式強制關閉(2)。\n");
						break;
					}
					CurrentStation = AllModel[i][j];
					Station[CurrentStation].paramFile.AIPredictParam.NewModelMode = false;
					if (BufferRetest && !flag)
					{
						TrainingMsg += string.Format("第{0}站 讀取原模型檔[{1}]...。\n", AllModel[i][j] + 1, AI_Param[AllModel[i][0]].ModelName_A);
						if (!Station[AllModel[i][j]].aiEnhanceTools.ReadModel())
						{
							TrainingMsg += string.Format("[回測中斷] 第{0}站 讀取原模型檔[{1}]失敗。\n", AllModel[i][j] + 1, AI_Param[AllModel[i][0]].ModelName_A);
							continue;
						}
						Station[AllModel[i][j]].OfflineBufTestStop = false;
						TrainingMsg += string.Format("第{0}站 [{1}] 原模型，回測影像開始。\n", AllModel[i][j] + 1, AI_Param[AllModel[i][0]].ModelName_A);
						OfflineBufTestFinish = false;
						Yield[CurrentStation] = Station[AllModel[i][j]].OfflineBufTest(TestImageSet, offLine: false, UseAI: true);
						TrainingMsg += string.Format("第{0}站 [{1}] 原模型，回測影像結束。\n", AllModel[i][j] + 1, AI_Param[AllModel[i][0]].ModelName_A);
						OfflineBufTestFinish = true;
					}
					else
					{
						TrainingMsg += string.Format("第{0}站 關閉回測影像。\n", AllModel[i][j] + 1);
					}
					if (ForceClose)
					{
						TrainingMsg += string.Format("[訓練中斷] 程式強制關閉(3)。\n");
						break;
					}
					ShowLabel = true;
					string text = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[i][0]].ModelName_A));
					if (!Station[AllModel[i][j]].aiEnhanceTools.ReadModel(text, ReLoadModel: true))
					{
						TrainingMsg += string.Format("[回測中斷] 第{0}站 讀取訓練後模型檔[{1}]失敗。\n", AllModel[i][j] + 1, text);
						continue;
					}
					ShowLabel = false;
					if (BufferRetest)
					{
						TrainingMsg += string.Format("第{0}站 讀取訓練後模型檔[{1}]...。\n", AllModel[i][j] + 1, text);
						if (!Station[AllModel[i][j]].aiEnhanceTools.ReadModel(text, ReLoadModel: true))
						{
							TrainingMsg += string.Format("[回測中斷] 第{0}站 讀取訓練後模型檔[{1}]失敗。\n", AllModel[i][j] + 1, text);
							continue;
						}
						TrainingMsg += string.Format("第{0}站 [{1}] 訓練後模型，回測影像開始。\n", AllModel[i][j] + 1, text);
						Station[AllModel[i][j]].OfflineBufTestStop = false;
						OfflineBufTestFinish = false;
						NewYield[CurrentStation] = Station[AllModel[i][j]].OfflineBufTest(TestImageSet, offLine: false, UseAI: true);
						TrainingMsg += string.Format("第{0}站 [{1}] 訓練後模型，回測影像結束。\n", AllModel[i][j] + 1, text);
						OfflineBufTestFinish = true;
					}
					else
					{
						CurrentPBTestValue[AllModel[i][j]] = 100;
					}
					TrainingMsg += string.Format("第{0}站 回測影像結束。\n\n", AllModel[i][j] + 1);
				}
				ModelTrained[i] = true;
				if (InterruptMode)
				{
					break;
				}
				Action method = [MethodImpl(MethodImplOptions.NoInlining)] () =>
				{
					int num = 0;
					txt_InspResult.Text = TrainingMsg;
					for (int k = 0; k < 8; k++)
					{
						if (k < ModelNumber && ModelEnable[k])
						{
							for (int l = 0; l < AllModel[k].Count; l++)
							{
								num = AllModel[k][l];
								LabYield[num].Content = string.Format("{0:N1}", Yield[num]);
								LabNewYield[num].Content = string.Format("{0:N1}", NewYield[num]);
							}
						}
					}
				};
				base.Dispatcher.BeginInvoke(method);
				DateTime now = DateTime.Now;
				TrainingMsg += string.Format("{0} Model [{1}] 訓練結束，測試完成。\n\n\n", now.ToString("yyyy-MM-dd-HH-mm-ss"), AI_Param[AllModel[i][0]].ModelName_A);
			}
			DateTime now2 = DateTime.Now;
			TrainingMsg += string.Format("{0} 所有站別訓練完成。\n\n\n", now2.ToString("yyyy-MM-dd-HH-mm-ss"));
			TrainingFinish = true;
			Thread.Sleep(1300);
			if (ForceClose)
			{
				break;
			}
			AITrainingPauseEvent.Reset();
		}
		TrainingFinish = true;
	}

	public int GetStationModeID(int station)
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < AllModel[i].Count; j++)
			{
				if (station == AllModel[i][j])
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int GetMainStation(int station)
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < AllModel[i].Count; j++)
			{
				if (station == AllModel[i][j])
				{
					return AllModel[i][0];
				}
			}
		}
		return -1;
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

	public void SetMode(AI_MODE[] mode)
	{
		StationAIMode = mode;
	}

	public void SetSAM(SAM sam)
	{
		mSam = sam;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(CVInspection[] station, string[] x_ok_path, string[] x_ng_path, string[] x_path, string[] y_path)
	{
		Station = station;
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			X_Path[i] = x_path[i];
			Y_Path[i] = y_path[i];
			X_OK_Path[i] = x_ok_path[i];
			X_NG_Path[i] = x_ng_path[i];
			if (Station[i].paramFile.AIEnhanceToolON && Station[i].paramFile.AIEnhanceParam.ModelName_A != "")
			{
				AI_Param[i] = Station[i].paramFile.AIEnhanceParam;
			}
			else if (Station[i].paramFile.AIPredictToolON && Station[i].paramFile.AIPredictParam.ModelName_A != "")
			{
				AI_Param[i] = Station[i].paramFile.AIPredictParam;
			}
			else
			{
				AI_Param[i] = Station[i].paramFile.AIEnhanceParam;
			}
			if ((X_Path[i] == "" || Y_Path[i] == "") && (Station[i].paramFile.AIEnhanceToolON || Station[i].paramFile.AIPredictToolON))
			{
				string text2 = "";
				text2 = ((LanguageType == 1) ? string.Format("站別[{0}]尚未設定AI影像資料夾。\n", i + 1) : ((LanguageType == 2) ? string.Format("站别[{0}]尚未设定AI影像资料夹。\n", i + 1) : ((LanguageType != 3) ? string.Format("Please setup Station [{0}] AI Image Folder First!\n", i + 1) : string.Format("ステーション[{0}]のAI画像フォルダーが設定されていません。\n", i + 1))));
				text += text2;
			}
			if (ThroughputFile.param.TrainingImageDoubleRate[i] > 0.0 || ThroughputFile.param.BatchSize[i] != 8 || ThroughputFile.param.DnetDiv[i] != 8)
			{
				BtnSetAIParam[i].Background = Brushes.Fuchsia;
			}
		}
		for (int j = 0; j < 7; j++)
		{
			for (int k = j + 1; k < 8; k++)
			{
				if (Station[j].paramFile.AIEnhanceParam.ModelName_A == Station[k].paramFile.AIEnhanceParam.ModelName_A)
				{
					ThroughputFile.param.EnableStationTrain[k] = ThroughputFile.param.EnableStationTrain[j];
					ThroughputFile.param.TrainingCount[k] = ThroughputFile.param.TrainingCount[j];
				}
			}
		}
		if (text != "")
		{
			MessageBox.Show(text, "Warning");
		}
		RemoteStatus = GetRemoteStatus();
		btn_RemoteTraining.IsEnabled = RemoteStatus;
		btn_LocalAndRemoteTraining.IsEnabled = RemoteStatus;
		lab_RemoteTraining.Visibility = Visibility.Hidden;
		if (RemoteStatus)
		{
			lab_RemoteTraining.Visibility = Visibility.Visible;
			RemoteRunFlag = CheckRemoteRunningStatus();
			if (RemoteRunFlag == REMOTERUNFLAG.EM_Standby)
			{
				UpdateRemoteUITImer.Stop();
				btn_RemoteTraining.IsEnabled = true;
				btn_LocalAndRemoteTraining.IsEnabled = true;
				CalcImage();
			}
			else
			{
				RemoteContinue = true;
				UpdateRemoteUITImer.Start();
				btn_RemoteTraining.IsEnabled = false;
				btn_LocalAndRemoteTraining.IsEnabled = false;
				btn_Interrupt_S1.IsEnabled = true;
			}
		}
		else
		{
			CalcImage();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private REMOTERUNFLAG GetRemoteRunningStatus()
	{
		string path = AppDomain.CurrentDomain.BaseDirectory + "Comm\\";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
			return REMOTERUNFLAG.EM_Standby;
		}
		string path2 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Training.msg";
		string path3 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
		if (File.Exists(path2))
		{
			return REMOTERUNFLAG.EM_Running;
		}
		if (File.Exists(path3))
		{
			if (Train_LocalAndRemote)
			{
				if (TrainingFinish)
				{
					btn_UpdateAll.IsEnabled = true;
					btn_Interrupt_S1.IsEnabled = false;
				}
				return REMOTERUNFLAG.EM_Finish;
			}
			btn_UpdateAll.IsEnabled = true;
			btn_Interrupt_S1.IsEnabled = false;
			return REMOTERUNFLAG.EM_Finish;
		}
		return REMOTERUNFLAG.EM_Standby;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private REMOTERUNFLAG CheckRemoteRunningFile()
	{
		string path = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Training.msg";
		string path2 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
		if (!File.Exists(path) && !File.Exists(path2))
		{
			return REMOTERUNFLAG.EM_Standby;
		}
		if (File.Exists(path))
		{
			return REMOTERUNFLAG.EM_Running;
		}
		btn_UpdateAll.IsEnabled = true;
		btn_Interrupt_S1.IsEnabled = false;
		return REMOTERUNFLAG.EM_Finish;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private REMOTERUNFLAG CheckRemoteRunningStatus()
	{
		string path = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Training.msg";
		string path2 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
		if (!File.Exists(path) && !File.Exists(path2))
		{
			return REMOTERUNFLAG.EM_Standby;
		}
		string path3 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\";
		if (!Directory.Exists(path3))
		{
			Directory.CreateDirectory(path3);
		}
		string[] directories = Directory.GetDirectories(path3);
		for (int i = 0; i < directories.Length; i++)
		{
			string path4 = directories[i] + "\\model.train";
			string path5 = directories[i] + "\\model.finish";
			string fileName = Path.GetFileName(directories[i]);
			if (File.Exists(path4) || File.Exists(path5))
			{
				RemoteModelName.Add(fileName);
				string text = string.Format("遠端訓練新增[{0}]\n", fileName);
				TrainingMsg += text;
			}
			else
			{
				string text2 = string.Format("遠端訓練[{0}]找不到model.train or model.finish\n", fileName);
				TrainingMsg += text2;
			}
		}
		if (!Train_LocalAndRemote)
		{
			for (int j = 0; j < 8; j++)
			{
				GridResult[j].Visibility = Visibility.Hidden;
				AllModel[j].Clear();
			}
			for (int k = 0; k < RemoteModelName.Count; k++)
			{
				for (int l = 0; l < 8; l++)
				{
					if (Path.GetFileNameWithoutExtension(AI_Param[l].ModelName_A) == RemoteModelName[k])
					{
						AllModel[k].Add(l);
						GridResult[l].Visibility = Visibility.Visible;
						LabStation[l].Background = ModelColor[k];
						LabRemote[l].Visibility = Visibility.Visible;
						PBTestProgress[l].Visibility = Visibility.Hidden;
						LabYield[l].Foreground = Brushes.White;
						LabNewYield[l].Foreground = Brushes.White;
					}
				}
			}
			ModelNumber = RemoteModelName.Count;
		}
		grd_Loading.Visibility = Visibility.Visible;
		btn_UpdateAll.IsEnabled = false;
		if (File.Exists(path))
		{
			return REMOTERUNFLAG.EM_Running;
		}
		btn_UpdateAll.IsEnabled = true;
		btn_Interrupt_S1.IsEnabled = false;
		if (!Train_LocalAndRemote)
		{
			lab_RemoteTraining.Content = "訓練完成";
		}
		return REMOTERUNFLAG.EM_Finish;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRemoteStatus()
	{
		string text = AppDomain.CurrentDomain.BaseDirectory + "RemoteTraining\\";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = text + "RemoteAITraining.exe";
		if (!File.Exists(text2))
		{
			lab_RemoteStatus.Content = string.Format("找不到遠端訓練程式! {0}", text2);
			return false;
		}
		string text3 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\OnLine.msg";
		if (!File.Exists(text3))
		{
			try
			{
				lab_RemoteStatus.Content = string.Format("遠端主機未開啟! 找不到 {0}", text3);
				return false;
			}
			catch (Exception ex)
			{
				CVBuffer.SaveLog("AITrainingWindow-GetRemoteStatus-Delete Directory()" + ex.ToString());
				return false;
			}
		}
		lab_RemoteStatus.Content = string.Format("遠端主機已就緒");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CalcImage()
	{
		try
		{
			int[] array = new int[8];
			int[] array2 = new int[8];
			StationNumber = 0;
			for (int i = 0; i < 8; i++)
			{
				array[i] = 0;
				array2[i] = 0;
				if (StationAIMode[i] == AI_MODE.EM_Enhance)
				{
					GetUNetAIImageNumbers(X_OK_Path[i], X_NG_Path[i], ref array[i], ref array2[i]);
					LabUNetModel[i].Visibility = Visibility.Visible;
				}
				else
				{
					if (X_Path[i] == "")
					{
						array[i] = 0;
					}
					else
					{
						if (Directory.Exists(X_OK_Path[i]))
						{
							Directory.CreateDirectory(X_OK_Path[i]);
						}
						string[] files = Directory.GetFiles(X_OK_Path[i], "*.*", SearchOption.TopDirectoryOnly);
						array[i] = files.Length;
					}
					if (Y_Path[i] == "")
					{
						array2[i] = 0;
					}
					else
					{
						if (Directory.Exists(X_NG_Path[i]))
						{
							Directory.CreateDirectory(X_NG_Path[i]);
						}
						string[] files2 = Directory.GetFiles(X_NG_Path[i], "*.*", SearchOption.TopDirectoryOnly);
						array2[i] = files2.Length;
					}
					LabUNetModel[i].Visibility = Visibility.Hidden;
				}
				BoxOKImgNum[i].Text = array[i].ToString();
				BoxNGImgNum[i].Text = array2[i].ToString();
				if (StationAIMode[i] == AI_MODE.EM_NoUse)
				{
					StationEnable[i] = false;
					SetStationEnable(i, enabled: false);
					GridResult[i].Visibility = Visibility.Hidden;
				}
				else
				{
					SetStationEnable(i, enabled: true);
					StationNumber++;
					AllModel[i].Clear();
				}
			}
			string text = "";
			ModelNumber = 0;
			bool[] array3 = new bool[8];
			for (int j = 0; j < 8; j++)
			{
				array3[j] = false;
			}
			for (int k = 0; k < 8; k++)
			{
				text = "";
				trainingImgOKCount[k] = 0;
				trainingImgNGCount[k] = 0;
				ModelBypass[k] = false;
				for (int l = 0; l < 8; l++)
				{
					if (StationEnable[l] && AI_Param[l].ModelName_A != "")
					{
						if (text == "")
						{
							text = AI_Param[l].ModelName_A;
							AllModel[k].Add(l);
							StationEnable[l] = false;
							array3[l] = true;
							trainingImgOKCount[k] = array[l];
							trainingImgNGCount[k] = array2[l];
						}
						else if (text == AI_Param[l].ModelName_A)
						{
							AllModel[k].Add(l);
							StationEnable[l] = false;
							trainingImgOKCount[k] = array[l];
							trainingImgNGCount[k] = array2[l];
							array3[l] = true;
						}
					}
				}
				if (AllModel[k].Count > 0)
				{
					string[] files3 = Directory.GetFiles(X_OK_Path[AllModel[k][0]], "*.*", SearchOption.AllDirectories);
					string[] files4 = Directory.GetFiles(X_NG_Path[AllModel[k][0]], "*.*", SearchOption.AllDirectories);
					trainingImgOKCount[k] += files3.Length;
					trainingImgNGCount[k] += files4.Length;
					TextBox obj = BoxOKImgNum[AllModel[k][0]];
					obj.Text = obj.Text + " (" + files3.Length + ")";
					TextBox obj2 = BoxNGImgNum[AllModel[k][0]];
					obj2.Text = obj2.Text + " (" + files4.Length + ")";
				}
				if (text != "")
				{
					ModelNumber++;
					if (trainingImgOKCount[k] > 0 || trainingImgNGCount[k] > 0)
					{
						ModelEnable[k] = true;
					}
					else
					{
						ModelEnable[k] = false;
					}
				}
				else
				{
					ModelEnable[k] = false;
				}
			}
			for (int m = 0; m < 8; m++)
			{
				if (!array3[m])
				{
					SetStationEnable(m, enabled: false);
				}
			}
			EnableModelCount = 0;
			EnableStationCount = 0;
			UpdateUI = false;
			for (int n = 0; n < ModelNumber; n++)
			{
				for (int num = 0; num < AllModel[n].Count; num++)
				{
					StationGrid[AllModel[n][num]].Background = ModelColor[n];
					if (ModelEnable[n] && ThroughputFile.param.EnableStationTrain[AllModel[n][num]])
					{
						EnableStationCount++;
					}
					else
					{
						ChkStationEnable[AllModel[n][num]].IsChecked = false;
						CheckEnable[AllModel[n][num]].Background = Brushes.White;
					}
					if (num != 0)
					{
						BoxTrainingCount[AllModel[n][num]].IsEnabled = false;
						BoxTrainingCount[AllModel[n][num]].Visibility = Visibility.Hidden;
						BtnSetAIParam[AllModel[n][num]].Visibility = Visibility.Hidden;
						LabTrainingCount[AllModel[n][num]].Visibility = Visibility.Hidden;
						BoxOKImgNum[AllModel[n][num]].Visibility = Visibility.Hidden;
						BoxNGImgNum[AllModel[n][num]].Visibility = Visibility.Hidden;
						LabOKTrain[AllModel[n][num]].Visibility = Visibility.Hidden;
						LabNGTrain[AllModel[n][num]].Visibility = Visibility.Hidden;
						BtnOpenOK[AllModel[n][num]].Visibility = Visibility.Hidden;
						BtnOpenNG[AllModel[n][num]].Visibility = Visibility.Hidden;
					}
				}
				if (ModelEnable[n] && ThroughputFile.param.EnableStationTrain[AllModel[n][0]])
				{
					EnableModelCount++;
				}
			}
			UpdateUI = true;
			for (int num2 = 0; num2 < 8; num2++)
			{
				LabTrainingStation[num2].Content = "";
				LabTrainingStation[num2].Background = Brushes.White;
			}
			int num3 = 0;
			for (int num4 = 0; num4 < 8; num4++)
			{
				if (ChkStationEnable[num4].IsChecked == true)
				{
					LabTrainingStation[num3].Content = num4 + 1;
					LabTrainingStation[num3].Background = Brushes.LightSeaGreen;
					num3++;
				}
			}
			if (EnableModelCount <= 0 || EnableStationCount <= 0)
			{
				btn_Training.IsEnabled = false;
				btn_RemoteTraining.IsEnabled = false;
				btn_LocalAndRemoteTraining.IsEnabled = false;
				return;
			}
			btn_Training.IsEnabled = true;
			if (RemoteStatus)
			{
				btn_RemoteTraining.IsEnabled = true;
				btn_LocalAndRemoteTraining.IsEnabled = true;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-SetPath()" + ex.ToString());
		}
	}

	public void SetThroughputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
		CopyValueToUI();
	}

	public void CopyValueToUI()
	{
		if (ThroughputFile != null)
		{
			chk_HideTrainingWnd.IsChecked = ThroughputFile.param.HideTrainingWnd;
			chk_EnableBufferRetest.IsChecked = ThroughputFile.param.EnableTrainingBufferTest;
			cb_TestImageSet.SelectedIndex = ThroughputFile.param.TrainingBufferIndex;
			chk_SaveImgUseDateForlder.IsChecked = true;
			chk_EnableMoveToTrainingImg.IsChecked = ThroughputFile.param.EnableMoveToTrainingImg;
			chk_AutoUpdateModel.IsChecked = ThroughputFile.param.EnableAutoUpdateModel;
			for (int i = 0; i < 8; i++)
			{
				BoxTrainingCount[i].Text = ThroughputFile.param.TrainingCount[i].ToString();
			}
		}
	}

	public void WriteUIValueToParam()
	{
		if (ThroughputFile != null)
		{
			ThroughputFile.param.HideTrainingWnd = chk_HideTrainingWnd.IsChecked.Value;
			ThroughputFile.param.EnableTrainingBufferTest = chk_EnableBufferRetest.IsChecked.Value;
			ThroughputFile.param.TrainingBufferIndex = cb_TestImageSet.SelectedIndex;
			ThroughputFile.param.SaveImgUseDateForlder = true;
			ThroughputFile.param.EnableMoveToTrainingImg = chk_EnableMoveToTrainingImg.IsChecked.Value;
			ThroughputFile.param.EnableAutoUpdateModel = chk_AutoUpdateModel.IsChecked.Value;
			for (int i = 0; i < 8; i++)
			{
				ThroughputFile.param.TrainingCount[i] = Convert.ToInt32(BoxTrainingCount[i].Text);
				ThroughputFile.param.EnableStationTrain[i] = ChkStationEnable[i].IsChecked.Value;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetTrainingImageAlarm(string modelname, string modelpath)
	{
		string text = "";
		text = ((LanguageType == 1) ? string.Format("模型[{0}]訓練影像資料夾缺少原始訓練影像，是否打開資料夾增加影像?", modelname) : ((LanguageType == 2) ? string.Format("模型[{0}]训练影像资料夹缺少原始训练影像，是否打开资料夹增加影像?", modelname) : ((LanguageType != 3) ? string.Format("Model[{0}] Add origional Training image to current forder first, Open current forder now?", modelname) : string.Format("モデル[{0}]のトレーニング画像フォルダーに元のトレーニング画像がありません。画像を追加するには、フォルダーを開きますか？", modelname))));
		if (MessageBox.Show(text, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			string fileName = "c:\\windows\\explorer.exe";
			string arguments = string.Format("/root, {0}", modelpath);
			Process.Start(fileName, arguments);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetUNetAIImageNumbers(string pathok, string pathng, ref int okcount, ref int ngcount)
	{
		okcount = (ngcount = 0);
		if (pathok == "" || pathok == null || pathng == "" || pathng == null)
		{
			okcount = (ngcount = 0);
			return false;
		}
		try
		{
			if (!Directory.Exists(pathok))
			{
				Directory.CreateDirectory(pathok);
				okcount = (ngcount = 0);
				if (!Directory.Exists(pathok))
				{
					return false;
				}
			}
			if (!Directory.Exists(pathng))
			{
				Directory.CreateDirectory(pathng);
				okcount = (ngcount = 0);
				if (!Directory.Exists(pathng))
				{
					return false;
				}
			}
			string[] files = Directory.GetFiles(pathok, "*.*", SearchOption.TopDirectoryOnly);
			okcount = files.Length;
			string[] files2 = Directory.GetFiles(pathng, "*.*", SearchOption.TopDirectoryOnly);
			ngcount = files2.Length;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-GetUNetAIImageNumbers()" + ex.ToString());
			return false;
		}
	}

	private void SetStationEnable(int index, bool enabled)
	{
		if (enabled)
		{
			if (ThroughputFile.param.EnableStationTrain[index])
			{
				ChkStationEnable[index].IsChecked = ThroughputFile.param.EnableStationTrain[index];
				if (!ThroughputFile.param.EnableStationTrain[index])
				{
					CheckEnable[index].Background = Brushes.White;
				}
				else
				{
					CheckEnable[index].Background = Brushes.Gold;
				}
				StationGrid[index].IsEnabled = true;
				LabModelName[index].Content = AI_Param[index].ModelName_A;
				BoxTrainingCount[index].Text = ThroughputFile.param.TrainingCount[index].ToString();
				StationEnable[index] = true;
				GridResult[index].Visibility = Visibility.Visible;
			}
			else
			{
				ChkStationEnable[index].IsChecked = false;
				CheckEnable[index].Background = Brushes.White;
				StationGrid[index].IsEnabled = true;
				LabModelName[index].Content = AI_Param[index].ModelName_A;
				BoxTrainingCount[index].Text = ThroughputFile.param.TrainingCount[index].ToString();
				StationEnable[index] = true;
				GridResult[index].Visibility = Visibility.Hidden;
			}
		}
		else
		{
			ChkStationEnable[index].IsChecked = false;
			CheckEnable[index].Background = Brushes.White;
			ThroughputFile.param.EnableStationTrain[index] = false;
			StationGrid[index].IsEnabled = false;
			LabModelName[index].Content = "";
			BoxTrainingCount[index].Text = AI_Param[index].TrainingCount.ToString();
			StationGrid[index].Background = Brushes.White;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenOKS1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		string fileName = "c:\\windows\\explorer.exe";
		string text = X_OK_Path[num];
		string text2 = "";
		if (text == "")
		{
			text2 = ((LanguageType == 1) ? string.Format("尚未設定AI影像資料夾!") : ((LanguageType == 2) ? string.Format("尚未设定AI影像资料夹!") : ((LanguageType != 3) ? string.Format("Set AI image folder first!") : "AI画像フォルダーが設定されていません!")));
			MessageBox.Show(text2);
		}
		else
		{
			string arguments = string.Format("/root, {0}", text);
			Process.Start(fileName, arguments);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenNGS1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		string fileName = "c:\\windows\\explorer.exe";
		string text = X_NG_Path[num];
		string text2 = "";
		if (text == "")
		{
			text2 = ((LanguageType == 1) ? string.Format("尚未設定AI影像資料夾!") : ((LanguageType == 2) ? string.Format("尚未设定AI影像资料夹!") : ((LanguageType != 3) ? string.Format("Set AI image folder first!") : "AI画像フォルダーが設定されていません!")));
			MessageBox.Show(text2);
		}
		else
		{
			string arguments = string.Format("/root, {0}", text);
			Process.Start(fileName, arguments);
		}
	}

	private void Chk_TrainingS1_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			CheckBox checkBox = (CheckBox)sender;
			int id = Convert.ToInt32(checkBox.Uid);
			UpdateTrainCheckBox(id, checkBox);
		}
	}

	private void UpdateTrainCheckBox(int id, CheckBox chk)
	{
		int num = -1;
		for (int i = 0; i < ModelNumber; i++)
		{
			for (int j = 0; j < AllModel[i].Count; j++)
			{
				if (AllModel[i][j] == id)
				{
					num = i;
				}
			}
		}
		if (num != -1)
		{
			UpdateUI = false;
			for (int k = 0; k < AllModel[num].Count; k++)
			{
				ChkStationEnable[AllModel[num][k]].IsChecked = chk.IsChecked;
				if (chk.IsChecked == true)
				{
					CheckEnable[AllModel[num][k]].Background = Brushes.Gold;
					GridResult[AllModel[num][k]].Visibility = Visibility.Visible;
				}
				else
				{
					CheckEnable[AllModel[num][k]].Background = Brushes.White;
					GridResult[AllModel[num][k]].Visibility = Visibility.Hidden;
				}
			}
			UpdateUI = true;
		}
		UpdateUI = false;
		StationNumber = 0;
		for (int l = 0; l < 8; l++)
		{
			if (ChkStationEnable[l].IsChecked == false)
			{
				CheckEnable[l].Background = Brushes.White;
				StationEnable[l] = false;
				int stationModeID = GetStationModeID(l);
				if (stationModeID >= 0)
				{
					ModelEnable[stationModeID] = false;
				}
				continue;
			}
			CheckEnable[l].Background = Brushes.Gold;
			StationEnable[l] = true;
			int stationModeID2 = GetStationModeID(l);
			if (stationModeID2 >= 0 && (trainingImgOKCount[stationModeID2] > 0 || trainingImgNGCount[stationModeID2] > 0))
			{
				ModelEnable[stationModeID2] = true;
				StationNumber++;
			}
		}
		int num2 = 0;
		for (int m = 0; m < 8; m++)
		{
			if (ModelEnable[m])
			{
				num2++;
			}
		}
		UpdateUI = true;
		if (StationNumber <= 0 || num2 <= 0)
		{
			btn_Training.IsEnabled = false;
			btn_RemoteTraining.IsEnabled = false;
			btn_LocalAndRemoteTraining.IsEnabled = false;
		}
		else
		{
			btn_Training.IsEnabled = true;
			if (RemoteStatus)
			{
				btn_RemoteTraining.IsEnabled = true;
				btn_LocalAndRemoteTraining.IsEnabled = true;
			}
		}
		for (int n = 0; n < 8; n++)
		{
			LabTrainingStation[n].Content = "";
			LabTrainingStation[n].Background = Brushes.White;
		}
		int num3 = 0;
		for (int num4 = 0; num4 < 8; num4++)
		{
			if (ChkStationEnable[num4].IsChecked == true)
			{
				LabTrainingStation[num3].Content = num4 + 1;
				LabTrainingStation[num3].Background = Brushes.LightSeaGreen;
				num3++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool RunTrainingScript(bool CreateNewModel, int StationNo, int DnetMode)
	{
		int num = 1;
		CreateNewModel = false;
		if (CreateNewModel)
		{
			num = 2;
		}
		for (int i = 0; i < num; i++)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			string text = "";
			double num2 = ThroughputFile.param.TrainingImageDoubleRate[StationNo];
			if (num2 < 0.0)
			{
				num2 = 0.0;
			}
			else if (num2 > 5.0)
			{
				num2 = 5.0;
			}
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "TrainningNew.bat");
			string path2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Trainning.bat");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training.bat");
			processStartInfo.FileName = "Training.bat";
			string arg = TrainingCount[StationNo];
			if (CreateNewModel && i == 0)
			{
				arg = "200";
			}
			string text3 = string.Format("{0}Training\\train\\train.exe", AppDomain.CurrentDomain.BaseDirectory);
			int num3 = 6;
			if (File.Exists(text3))
			{
				DateTime lastWriteTime = new FileInfo(text3).LastWriteTime;
				DateTime dateTime = new DateTime(2022, 3, 20);
				DateTime dateTime2 = new DateTime(2022, 6, 8);
				if (lastWriteTime > dateTime2)
				{
					num3 = 9;
				}
				else if (lastWriteTime > dateTime)
				{
					num3 = 8;
				}
				text = ((DnetMode != 0 && DnetMode != 2) ? string.Format("cd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V3 -r Train -e {0} -l {1} -a {2}", arg, num3, num2) : string.Format("cd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V1 -r Train -e {0} -l 1 -a {1}", arg, num2));
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
					TrainingMsg += string.Format("找不到訓練程式", processStartInfo.FileName);
					return false;
				}
				string text4 = "";
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
					text4 = process.StandardOutput.ReadToEnd();
					TrainingMsg = TrainingMsg + text4 + eOut;
					ModelTraingingLog[GetStationModeID(StationNo)] = text4;
				}
				process.WaitForExit();
				process.Close();
				if (CreateNewModel && i == 0)
				{
					DelTempProcFile();
					CurrentModelPBValue[GetStationModeID(StationNo)] = 0;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool IsFileInUse(string fileName)
	{
		bool result = true;
		FileStream fileStream = null;
		try
		{
			fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
			result = false;
		}
		catch
		{
		}
		finally
		{
			fileStream?.Close();
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public int GetRemoteProgress(string modelname, ref string acc, ref bool ReadSuccess)
	{
		string path = string.Format("{0}Comm\\{1}", AppDomain.CurrentDomain.BaseDirectory, modelname);
		acc = "0";
		ReadSuccess = false;
		int num = 0;
		try
		{
			if (!Directory.Exists(path))
			{
				return 0;
			}
			string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
			if (files.Length == 0)
			{
				return 0;
			}
			int num2 = -1;
			int num3 = -1000;
			for (int i = 0; i < files.Length; i++)
			{
				num = Convert.ToInt32(Path.GetFileNameWithoutExtension(files[i]));
				if (num > num3 && !IsFileInUse(files[i]) && new FileInfo(files[i]).Length > 1)
				{
					num2 = i;
					num3 = num;
					acc = ReadFile(files[num2]);
					ReadSuccess = true;
				}
			}
			return num3;
		}
		catch
		{
			return -1;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public int GetProgress(int modelid)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\proc\\");
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
			if (files.Length == 0)
			{
				return -1;
			}
			int num = -1;
			int num2 = 10000;
			int num3 = 0;
			for (int i = 0; i < files.Length; i++)
			{
				num3 = Convert.ToInt32(Path.GetFileNameWithoutExtension(files[i]));
				if (num3 > CurrentModelPBValue[modelid] && !IsFileInUse(files[i]))
				{
					num2 = num3;
					num = i;
					CurrentModelPBValue[modelid] = num2;
					if (new FileInfo(files[i]).Length > 1)
					{
						accstr[modelid] = ReadFile(files[num]);
					}
					File.Delete(files[num]);
				}
			}
			return CurrentModelPBValue[modelid];
		}
		catch
		{
			return -1;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public int GetTestProgress(int modelid)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path, "*.proc", SearchOption.AllDirectories);
		if (files.Length == 0)
		{
			return -1;
		}
		try
		{
			int num = 0;
			int num2 = 0;
			if (num2 < files.Length)
			{
				num = Convert.ToInt32(Path.GetFileNameWithoutExtension(files[num2]));
				CurrentPBTestValue[modelid] = num;
			}
			return CurrentPBTestValue[modelid];
		}
		catch
		{
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string ReadFile(string file)
	{
		string result = "";
		try
		{
			StreamReader streamReader = new StreamReader(file);
			result = streamReader.ReadLine();
			streamReader.Close();
			return result;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception: " + ex.Message);
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CopyModel(int modelid)
	{
		try
		{
			int num = AllModel[modelid][0];
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string modelName_A = AI_Param[num].ModelName_A;
			string text2 = Path.Combine(path, modelName_A);
			string text3 = Path.Combine(text, "navs.h5");
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
	private bool CopyNewModel(int modelid)
	{
		try
		{
			int num = AllModel[modelid][0];
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arg = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[modelid][0]].ModelName_A));
			string path2 = ((StationAIMode[num] != AI_MODE.EM_Predict) ? string.Format("{0}.h5d", arg) : string.Format("{0}.h5", arg));
			string text2 = Path.Combine(path, path2);
			string text3 = Path.Combine(text, "navs.h5");
			if (!File.Exists(text3))
			{
				return false;
			}
			FileInfo fileInfo = new FileInfo(text3);
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
			if (File.Exists(text2))
			{
				FileInfo fileInfo2 = new FileInfo(text2);
				if (fileInfo2.IsReadOnly)
				{
					fileInfo2.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			File.Copy(text3, text2, overwrite: true);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CopyNewModel()" + ex.ToString());
			return false;
		}
	}

	private void RunDuplicateImageCheck(string xpath, string ypath, bool extramode)
	{
		FindSameImage(xpath, ypath);
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
	private bool CreateRemoteIni2(int modelid)
	{
		if (AllModel[modelid].Count < 1)
		{
			return false;
		}
		try
		{
			int num = AllModel[modelid][0];
			int dnetMode_A = AI_Param[AllModel[modelid][0]].DnetMode_A;
			string text = string.Format("{0}Comm\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(AI_Param[num].ModelName_A));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string[] files = Directory.GetFiles(text, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
			string text2 = Path.Combine(text, "navs.ini");
			string path = string.Format("{0}Training\\Model1\\", AppDomain.CurrentDomain.BaseDirectory);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			NavsIni = new HiniFile(text2, ";");
			if (StationAIMode[num] == AI_MODE.EM_Enhance)
			{
				NavsIni.WriteString("DAVS_V3", "SegModelName", "Training\\Model1\\navs.h5");
				NavsIni.WriteInt("DAVS_V3", "ImgWidth", AI_Param[num].Width_A);
				NavsIni.WriteInt("DAVS_V3", "ImgHeight", AI_Param[num].Height_A);
				NavsIni.WriteDouble("DAVS_V3", "AugRatio", 1.0);
				NavsIni.WriteInt("DAVS_V3", "Epoch", GetTrainingCount(num));
				NavsIni.WriteString("DAVS_V3", "PredPath", "");
				NavsIni.WriteString("DAVS_V3", "OutPath", "OutPath");
				if (dnetMode_A == 0 || dnetMode_A == 1)
				{
					NavsIni.WriteInt("DAVS_V3", "LossFunction", 1);
				}
				else
				{
					NavsIni.WriteInt("DAVS_V3", "LossFunction", 6);
				}
				NavsIni.WriteDouble("DAVS_V3", "MaybeArea", MaybeArea);
				NavsIni.WriteDouble("DAVS_V3", "LR", 0.001);
				NavsIni.WriteInt("DAVS_V3", "NGIgnore", 0);
				NavsIni.WriteInt("DAVS_V3", "OKBlank", 1);
				NavsIni.WriteInt("DAVS_V3", "Threshold", 220);
				string value = string.Format("..\\..\\TrainingImage\\{0}\\X\\", Path.GetFileNameWithoutExtension(AI_Param[num].ModelName_A));
				NavsIni.WriteString("DAVS_V3", "XPath", value);
				value = string.Format("..\\..\\TrainingImage\\{0}\\Y\\", Path.GetFileNameWithoutExtension(AI_Param[num].ModelName_A));
				NavsIni.WriteString("DAVS_V3", "YPath", value);
				NavsIni.WriteString("DAVS_V3", "ModelType", "US");
				NavsIni.WriteInt("DAVS_V3", "UnetDiv", ThroughputFile.param.DnetDiv[num]);
				NavsIni.WriteInt("DAVS_V3", "BatchSize", ThroughputFile.param.BatchSize[num]);
				NavsIni.WriteInt("DAVS_V3", "GPUTempThrehold", 85);
				NavsIni.WriteInt("DAVS_V3", "GPUSleepMs", 1000);
				NavsIni.WriteDouble("Augmentation", "horizontalFlip", ThroughputFile.param.Training_horizontalFlip[num]);
				NavsIni.WriteDouble("Augmentation", "verticalFlip", ThroughputFile.param.Training_verticalFlip[num]);
				NavsIni.WriteDouble("Augmentation", "brightness_limit", ThroughputFile.param.Training_brightness_limit[num]);
				NavsIni.WriteDouble("Augmentation", "contrast_limit", ThroughputFile.param.Training_contrast_limit[num]);
				NavsIni.WriteDouble("Augmentation", "scale_limit", ThroughputFile.param.Training_scale_limit[num]);
				NavsIni.WriteDouble("Augmentation", "rotate_limit", ThroughputFile.param.Training_rotate_limit[num]);
				NavsIni.WriteDouble("Augmentation", "shift_limit", ThroughputFile.param.Training_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "hue_shift_limit", ThroughputFile.param.Training_hue_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "sat_shift_limit", ThroughputFile.param.Training_sat_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "val_shift_limit", ThroughputFile.param.Training_val_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "r_shift_limit", ThroughputFile.param.Training_r_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "g_shift_limit", ThroughputFile.param.Training_g_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "b_shift_limit", ThroughputFile.param.Training_b_shift_limit[num]);
			}
			else
			{
				NavsIni.WriteInt("BaseParam", "Epochs", GetTrainingCount(num));
				NavsIni.WriteInt("BaseParam", "Img_Width", AI_Param[num].Width_A);
				NavsIni.WriteInt("BaseParam", "Img_Height", AI_Param[num].Height_A);
				NavsIni.WriteString("Path", "TrainImgsPath", Path.Combine(X_Path[num], "..\\"));
			}
			if (GetFileEncodeType(text2) != Encoding.UTF8)
			{
				string value2 = File.ReadAllText(text2, Encoding.Default);
				File.Delete(text2);
				FileStream fileStream = File.Create(text2);
				using (StreamWriter streamWriter = new StreamWriter(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
				{
					streamWriter.Write(value2);
					streamWriter.WriteLine();
				}
				fileStream.Close();
			}
			string text3 = "";
			double num2 = ThroughputFile.param.TrainingImageDoubleRate[num];
			if (num2 < 0.0)
			{
				num2 = 0.0;
			}
			else if (num2 > 5.0)
			{
				num2 = 5.0;
			}
			string arg = TrainingCount[num];
			string text4 = string.Format("{0}Training\\train1\\train.exe", AppDomain.CurrentDomain.BaseDirectory);
			int num3 = 6;
			if (File.Exists(text4))
			{
				DateTime lastWriteTime = new FileInfo(text4).LastWriteTime;
				DateTime dateTime = new DateTime(2022, 3, 25);
				DateTime dateTime2 = new DateTime(2022, 6, 7);
				if (lastWriteTime > dateTime2)
				{
					num3 = 9;
				}
				else if (lastWriteTime > dateTime)
				{
					num3 = 8;
				}
				text3 = ((dnetMode_A != 0 && dnetMode_A != 2) ? string.Format("cd..\ncd Training\ncd train1\ntrain -i ..\\Model1\\navs.ini -m ..\\Model1\\navs.h5 -s 0 -v DAVS_V3 -r Train -e {0} -l {1} -a {2}", arg, num3, num2) : string.Format("cd..\ncd Training\ncd train1\ntrain -i ..\\Model1\\navs.ini -m ..\\Model1\\navs.h5 -s 0 -v DAVS_V1 -r Train -e {0} -l 1 -a {1}", arg, num2));
				string text5 = Path.Combine(text, "Training.bat");
				if (File.Exists(text5))
				{
					FileInfo fileInfo = new FileInfo(text5);
					if (fileInfo.IsReadOnly)
					{
						fileInfo.IsReadOnly = false;
					}
					File.Delete(text5);
				}
				using (StreamWriter streamWriter2 = new StreamWriter(text5, append: true))
				{
					streamWriter2.Write("{0}", text3);
					streamWriter2.WriteLine();
				}
				string destFileName = Path.Combine(text, "model.train");
				File.Copy(text5, destFileName, overwrite: true);
				string path2 = Path.Combine(text, "TensorRT.msg");
				if (AI_Param[num].UseTensorRT)
				{
					using StreamWriter streamWriter3 = new StreamWriter(path2, append: true);
					streamWriter3.Write("{0}", "");
					streamWriter3.WriteLine();
				}
				else if (File.Exists(path2))
				{
					File.Delete(path2);
				}
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CreateRemoteIni2()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CreateRemoteIni(int modelid)
	{
		if (AllModel[modelid].Count < 1)
		{
			return false;
		}
		try
		{
			int num = AllModel[modelid][0];
			int dnetMode_A = AI_Param[AllModel[modelid][0]].DnetMode_A;
			string text = string.Format("{0}Comm\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(AI_Param[num].ModelName_A));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string[] files = Directory.GetFiles(text, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
			string text2 = Path.Combine(text, "navs.ini");
			NavsIni = new HiniFile(text2, ";");
			if (StationAIMode[num] == AI_MODE.EM_Enhance)
			{
				NavsIni.WriteString("DAVS_V3", "SegModelName", "Training\\Model\\navs.h5");
				NavsIni.WriteInt("DAVS_V3", "ImgWidth", AI_Param[num].Width_A);
				NavsIni.WriteInt("DAVS_V3", "ImgHeight", AI_Param[num].Height_A);
				NavsIni.WriteDouble("DAVS_V3", "AugRatio", 1.0);
				NavsIni.WriteInt("DAVS_V3", "Epoch", GetTrainingCount(num));
				NavsIni.WriteString("DAVS_V3", "PredPath", "");
				NavsIni.WriteString("DAVS_V3", "OutPath", "OutPath");
				if (dnetMode_A == 0 || dnetMode_A == 1)
				{
					NavsIni.WriteInt("DAVS_V3", "LossFunction", 1);
				}
				else
				{
					NavsIni.WriteInt("DAVS_V3", "LossFunction", 6);
				}
				NavsIni.WriteDouble("DAVS_V3", "MaybeArea", MaybeArea);
				NavsIni.WriteDouble("DAVS_V3", "LR", 0.001);
				NavsIni.WriteInt("DAVS_V3", "NGIgnore", 0);
				NavsIni.WriteInt("DAVS_V3", "OKBlank", 1);
				NavsIni.WriteInt("DAVS_V3", "Threshold", 220);
				string value = string.Format("..\\..\\TrainingImage\\{0}\\X\\", Path.GetFileNameWithoutExtension(AI_Param[num].ModelName_A));
				NavsIni.WriteString("DAVS_V3", "XPath", value);
				value = string.Format("..\\..\\TrainingImage\\{0}\\Y\\", Path.GetFileNameWithoutExtension(AI_Param[num].ModelName_A));
				NavsIni.WriteString("DAVS_V3", "YPath", value);
				NavsIni.WriteString("DAVS_V3", "ModelType", "US");
				NavsIni.WriteInt("DAVS_V3", "UnetDiv", ThroughputFile.param.DnetDiv[num]);
				NavsIni.WriteInt("DAVS_V3", "BatchSize", ThroughputFile.param.BatchSize[num]);
				NavsIni.WriteInt("DAVS_V3", "GPUTempThrehold", 85);
				NavsIni.WriteInt("DAVS_V3", "GPUSleepMs", 1000);
				NavsIni.WriteDouble("Augmentation", "horizontalFlip", ThroughputFile.param.Training_horizontalFlip[num]);
				NavsIni.WriteDouble("Augmentation", "verticalFlip", ThroughputFile.param.Training_verticalFlip[num]);
				NavsIni.WriteDouble("Augmentation", "brightness_limit", ThroughputFile.param.Training_brightness_limit[num]);
				NavsIni.WriteDouble("Augmentation", "contrast_limit", ThroughputFile.param.Training_contrast_limit[num]);
				NavsIni.WriteDouble("Augmentation", "scale_limit", ThroughputFile.param.Training_scale_limit[num]);
				NavsIni.WriteDouble("Augmentation", "rotate_limit", ThroughputFile.param.Training_rotate_limit[num]);
				NavsIni.WriteDouble("Augmentation", "shift_limit", ThroughputFile.param.Training_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "hue_shift_limit", ThroughputFile.param.Training_hue_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "sat_shift_limit", ThroughputFile.param.Training_sat_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "val_shift_limit", ThroughputFile.param.Training_val_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "r_shift_limit", ThroughputFile.param.Training_r_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "g_shift_limit", ThroughputFile.param.Training_g_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "b_shift_limit", ThroughputFile.param.Training_b_shift_limit[num]);
			}
			else
			{
				NavsIni.WriteInt("BaseParam", "Epochs", GetTrainingCount(num));
				NavsIni.WriteInt("BaseParam", "Img_Width", AI_Param[num].Width_A);
				NavsIni.WriteInt("BaseParam", "Img_Height", AI_Param[num].Height_A);
				NavsIni.WriteString("Path", "TrainImgsPath", Path.Combine(X_Path[num], "..\\"));
			}
			if (GetFileEncodeType(text2) != Encoding.UTF8)
			{
				string value2 = File.ReadAllText(text2, Encoding.Default);
				File.Delete(text2);
				FileStream fileStream = File.Create(text2);
				using (StreamWriter streamWriter = new StreamWriter(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
				{
					streamWriter.Write(value2);
					streamWriter.WriteLine();
				}
				fileStream.Close();
			}
			string text3 = "";
			double num2 = ThroughputFile.param.TrainingImageDoubleRate[num];
			if (num2 < 0.0)
			{
				num2 = 0.0;
			}
			else if (num2 > 5.0)
			{
				num2 = 5.0;
			}
			string text4 = Path.Combine(text, "Training.bat");
			string arg = TrainingCount[num];
			string text5 = string.Format("{0}Training\\train\\train.exe", AppDomain.CurrentDomain.BaseDirectory);
			int num3 = 6;
			if (File.Exists(text5))
			{
				DateTime lastWriteTime = new FileInfo(text5).LastWriteTime;
				DateTime dateTime = new DateTime(2022, 3, 25);
				if (lastWriteTime > dateTime)
				{
					num3 = 8;
				}
				text3 = ((dnetMode_A != 0 && dnetMode_A != 2) ? string.Format("cd..\ncd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V3 -r Train -e {0} -l {1} -a {2}", arg, num3, num2) : string.Format("cd..\ncd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V1 -r Train -e {0} -l 1 -a {1}", arg, num2));
				if (File.Exists(text4))
				{
					FileInfo fileInfo = new FileInfo(text4);
					if (fileInfo.IsReadOnly)
					{
						fileInfo.IsReadOnly = false;
					}
					File.Delete(text4);
				}
				using (StreamWriter streamWriter2 = new StreamWriter(text4, append: true))
				{
					streamWriter2.Write("{0}", text3);
					streamWriter2.WriteLine();
				}
				string path = Path.Combine(text, "TensorRT.msg");
				if (AI_Param[num].UseTensorRT)
				{
					using StreamWriter streamWriter3 = new StreamWriter(path, append: true);
					streamWriter3.Write("{0}", "");
					streamWriter3.WriteLine();
				}
				else if (File.Exists(path))
				{
					File.Delete(path);
				}
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-CreateRemoteIni()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CreateIni(int modelid, int DnetMode)
	{
		if (AllModel[modelid].Count < 1)
		{
			return false;
		}
		try
		{
			string text = Path.Combine(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\"), "navs.ini");
			NavsIni = new HiniFile(text, ";");
			int num = AllModel[modelid][0];
			if (StationAIMode[num] == AI_MODE.EM_Enhance)
			{
				NavsIni.WriteString("DAVS_V3", "SegModelName", "Training\\Model\\navs.h5");
				NavsIni.WriteInt("DAVS_V3", "ImgWidth", AI_Param[num].Width_A);
				NavsIni.WriteInt("DAVS_V3", "ImgHeight", AI_Param[num].Height_A);
				NavsIni.WriteDouble("DAVS_V3", "AugRatio", 1.0);
				NavsIni.WriteInt("DAVS_V3", "Epoch", GetTrainingCount(num));
				NavsIni.WriteString("DAVS_V3", "PredPath", "");
				NavsIni.WriteString("DAVS_V3", "OutPath", "OutPath");
				if (DnetMode == 0 || DnetMode == 1)
				{
					NavsIni.WriteInt("DAVS_V3", "LossFunction", 1);
				}
				else
				{
					NavsIni.WriteInt("DAVS_V3", "LossFunction", 6);
				}
				NavsIni.WriteDouble("DAVS_V3", "MaybeArea", MaybeArea);
				NavsIni.WriteDouble("DAVS_V3", "LR", 0.001);
				NavsIni.WriteInt("DAVS_V3", "NGIgnore", 0);
				NavsIni.WriteInt("DAVS_V3", "OKBlank", 1);
				NavsIni.WriteInt("DAVS_V3", "Threshold", 220);
				NavsIni.WriteString("DAVS_V3", "XPath", X_Path[num]);
				NavsIni.WriteString("DAVS_V3", "YPath", Y_Path[num]);
				NavsIni.WriteString("DAVS_V3", "ModelType", "US");
				NavsIni.WriteInt("DAVS_V3", "UnetDiv", ThroughputFile.param.DnetDiv[num]);
				NavsIni.WriteInt("DAVS_V3", "BatchSize", ThroughputFile.param.BatchSize[num]);
				NavsIni.WriteInt("DAVS_V3", "GPUTempThrehold", 85);
				NavsIni.WriteInt("DAVS_V3", "GPUSleepMs", 1000);
				NavsIni.WriteDouble("Augmentation", "horizontalFlip", ThroughputFile.param.Training_horizontalFlip[num]);
				NavsIni.WriteDouble("Augmentation", "verticalFlip", ThroughputFile.param.Training_verticalFlip[num]);
				NavsIni.WriteDouble("Augmentation", "brightness_limit", ThroughputFile.param.Training_brightness_limit[num]);
				NavsIni.WriteDouble("Augmentation", "contrast_limit", ThroughputFile.param.Training_contrast_limit[num]);
				NavsIni.WriteDouble("Augmentation", "scale_limit", ThroughputFile.param.Training_scale_limit[num]);
				NavsIni.WriteDouble("Augmentation", "rotate_limit", ThroughputFile.param.Training_rotate_limit[num]);
				NavsIni.WriteDouble("Augmentation", "shift_limit", ThroughputFile.param.Training_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "hue_shift_limit", ThroughputFile.param.Training_hue_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "sat_shift_limit", ThroughputFile.param.Training_sat_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "val_shift_limit", ThroughputFile.param.Training_val_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "r_shift_limit", ThroughputFile.param.Training_r_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "g_shift_limit", ThroughputFile.param.Training_g_shift_limit[num]);
				NavsIni.WriteInt("Augmentation", "b_shift_limit", ThroughputFile.param.Training_b_shift_limit[num]);
			}
			else
			{
				NavsIni.WriteInt("BaseParam", "Epochs", GetTrainingCount(num));
				NavsIni.WriteInt("BaseParam", "Img_Width", AI_Param[num].Width_A);
				NavsIni.WriteInt("BaseParam", "Img_Height", AI_Param[num].Height_A);
				NavsIni.WriteString("Path", "TrainImgsPath", Path.Combine(X_Path[num], "..\\"));
			}
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
	private int GetTrainingCount(int id)
	{
		try
		{
			return Convert.ToInt32(TrainingCount[id]);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-GetTrainingCount()" + ex.ToString());
			return 100;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int GetStationTrainingImageCount(int station)
	{
		string path = X_Path[station];
		if (!Directory.Exists(path))
		{
			return -1;
		}
		string[] array = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		for (int i = 0; i < array.Length; i++)
		{
			string extension = Path.GetExtension(array[i]);
			extension = extension.ToLower();
			if (extension != ".bmp" && extension != ".jpg" && extension != ".tif" && extension != ".png")
			{
				array = array.Take(i).Concat(array.Skip(i + 1)).ToArray();
				i--;
			}
		}
		return array.Length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CombineModelImage(int MainModelID, string dstr, ref string str)
	{
		int num = AllModel[MainModelID][0];
		if (AllModel[MainModelID].Count < 1)
		{
			str += string.Format("Model [{0}] 訓練模型站別數量為0。\n\n", AI_Param[AllModel[MainModelID][0]].ModelName_A, dstr);
			return false;
		}
		try
		{
			string text = Path.Combine(X_Path[num], "OK\\");
			string text2 = Path.Combine(X_Path[num], "NG\\");
			string text3 = Path.Combine(Y_Path[num], "OK\\");
			string text4 = Path.Combine(Y_Path[num], "NG\\");
			string dstDir = Path.Combine(text, string.Format("{0}\\", dstr));
			string dstDir2 = Path.Combine(text2, string.Format("{0}\\", dstr));
			string dstDir3 = Path.Combine(text3, string.Format("{0}\\", dstr));
			string dstDir4 = Path.Combine(text4, string.Format("{0}\\", dstr));
			str += string.Format("Model [{0}] 搬移本次新增訓練影像至資料夾({1})。\n\n", AI_Param[AllModel[MainModelID][0]].ModelName_A, dstr);
			ImageFileCopy(text, dstDir, File_Action.EM_Move, "*.*");
			ImageFileCopy(text2, dstDir2, File_Action.EM_Move, "*.*");
			ImageFileCopy(text3, dstDir3, File_Action.EM_Move, "*.*");
			ImageFileCopy(text4, dstDir4, File_Action.EM_Move, "*.*");
			return true;
		}
		catch (Exception ex)
		{
			string text5 = "AITrainingWindow-CombineModelImage()";
			CVBuffer.SaveLog(str = text5 + ex.ToString());
			return false;
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
		string text = sourceDir.Replace(Directory.GetParent(sourceDir).ToString(), "").Replace(Path.DirectorySeparatorChar.ToString(), "");
		if (!(sourceDir == targetDir + text))
		{
			string text2 = targetDir + Path.DirectorySeparatorChar + text;
			string[] files = Directory.GetFiles(sourceDir);
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i], text2 + Path.DirectorySeparatorChar + Path.GetFileName(files[i]), overwrite: true);
			}
			string[] directories = Directory.GetDirectories(sourceDir);
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryCopy(directories[j], text2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool DirectoryCopy(string sourceDirName, string destDirName, bool MoveAndBackup, string dstr)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		try
		{
			if (!directoryInfo.Exists)
			{
				return false;
			}
			FileInfo[] files = directoryInfo.GetFiles();
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				string destFileName = Path.Combine(destDirName, fileInfo.Name);
				fileInfo.CopyTo(destFileName, overwrite: true);
			}
			if (MoveAndBackup && files.Length != 0)
			{
				MoveAndBackupDirectory(sourceDirName, dstr, "..\\..\\Backup\\");
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-DirectoryCopy()" + ex.ToString());
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void MoveToTrainingImgDB(string sourceDirName, string dateStr, string uppath)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		try
		{
			if (directoryInfo.Exists)
			{
				string name = directoryInfo.Name;
				if (!Directory.Exists(uppath))
				{
					Directory.CreateDirectory(uppath);
				}
				string text = Path.Combine(uppath, dateStr);
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				string text2 = Path.Combine(text, name);
				if (Directory.Exists(text2))
				{
					Directory.Delete(text2, recursive: true);
				}
				Directory.Move(sourceDirName, text2);
				Directory.CreateDirectory(sourceDirName);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-MoveAndBackupDirectory()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ImageFileCopy(string OrgDir, string DstDir, File_Action action = File_Action.EM_Copy, string Filetype = "*.*")
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(OrgDir);
		try
		{
			if (!directoryInfo.Exists)
			{
				return false;
			}
			string[] files = Directory.GetFiles(OrgDir, Filetype, SearchOption.TopDirectoryOnly);
			if (files.Length == 0)
			{
				return false;
			}
			if (!Directory.Exists(DstDir))
			{
				Directory.CreateDirectory(DstDir);
			}
			for (int i = 0; i < files.Length; i++)
			{
				string text = Path.Combine(DstDir, Path.GetFileName(files[i]));
				if (!File.Exists(files[i]))
				{
					continue;
				}
				FileInfo fileInfo = new FileInfo(files[i]);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				if (action != File_Action.EM_Delete)
				{
					if (File.Exists(text))
					{
						FileInfo fileInfo2 = new FileInfo(text);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
					}
					File.Delete(text);
				}
				switch (action)
				{
				case File_Action.EM_Move:
					File.Move(files[i], text);
					break;
				case File_Action.EM_Delete:
					File.Delete(files[i]);
					break;
				case File_Action.EM_Copy:
					File.Copy(files[i], text, overwrite: true);
					break;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-ImagefileCopy()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void MoveTrainingImage(string OrgPath, string DstDir, string ImageFolder)
	{
		string text = Path.Combine(OrgPath, ImageFolder);
		DirectoryInfo directoryInfo = new DirectoryInfo(text);
		try
		{
			if (directoryInfo.Exists)
			{
				string text2 = Path.Combine(text, DstDir);
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				string[] files = Directory.GetFiles(text, "*.*", SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					string destFileName = Path.Combine(text2, Path.GetFileName(files[i]));
					File.Move(files[i], destFileName);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-MoveTrainingImage()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void BackupTrainingOrgImage(string OrgPath, string Folder, string BackupMainFolder, File_Action action)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(OrgPath);
		try
		{
			if (!directoryInfo.Exists)
			{
				return;
			}
			string text = Path.Combine(OrgPath, "..\\");
			string path = Path.Combine(text, Folder);
			if (!Directory.Exists(text))
			{
				return;
			}
			string path2 = Path.Combine(Path.Combine(OrgPath, "..\\Backup"), BackupMainFolder);
			path2 = Path.Combine(path2, Folder);
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
			}
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				string destFileName = Path.Combine(path2, Path.GetFileName(files[i]));
				switch (action)
				{
				case File_Action.EM_Move:
					File.Move(files[i], destFileName);
					break;
				case File_Action.EM_Delete:
					File.Delete(files[i]);
					break;
				case File_Action.EM_Copy:
					File.Copy(files[i], destFileName, overwrite: true);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-BackupTrainingOrgImage()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void MoveAndBackupDirectory(string sourceDirName, string dateStr, string targetFolder)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		try
		{
			if (directoryInfo.Exists)
			{
				string name = directoryInfo.Name;
				string text = "";
				text = Path.Combine(sourceDirName, targetFolder);
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				string text2 = Path.Combine(text, dateStr);
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				string text3 = Path.Combine(text2, name);
				if (Directory.Exists(text3))
				{
					Directory.Delete(text3, recursive: true);
				}
				Directory.Move(sourceDirName, text3);
				Directory.CreateDirectory(sourceDirName);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-MoveAndBackupDirectory()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Wnd_Main_Closed(object sender, EventArgs e)
	{
		try
		{
			ForceClose = true;
			Thread.Sleep(100);
			AITrainingPauseEvent.Set();
			UpdateUITImer.Stop();
			UpdateRemoteUITImer.Stop();
			Thread.Sleep(100);
			for (int i = 0; i < 8; i++)
			{
				Station[i].OfflineBufTestStop = true;
			}
			if (!RemoteContinue)
			{
				WriteUIValueToParam();
			}
			if (!RemoteStatus)
			{
				KillTask("train.exe");
			}
			string path = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
			if (File.Exists(path))
			{
				File.Delete(path);
				ResetProgress = true;
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("AIP01-AITrainingWindow-Wnd_Main_Closed-") + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		if (!TrainingFinish && Train_LocalAndRemote)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("是否終止訓練? (所有結果不會被儲存)", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("是否终止训练? (所有结果不会被储存)", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Abort training? (Model will not be save)", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("トレーニングを中止しますか？ (すべての結果は保存されません)", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
		}
		else if (!TrainingFinish && Train_Local)
		{
			MessageBoxResult messageBoxResult2 = ((LanguageType == 1) ? MessageBox.Show("是否終止訓練? (所有結果不會被儲存)", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("是否终止训练? (所有结果不会被储存)", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Abort training? (Model will not be save)", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("トレーニングを中止しますか？ (すべての結果は保存されません)", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult2 != MessageBoxResult.Yes)
			{
				return;
			}
		}
		bool flag = false;
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			if (StationGrid[i].Visibility == Visibility.Visible && BTNUpdate[i].IsEnabled)
			{
				flag = true;
				text += string.Format("[{0}], ", i + 1);
			}
		}
		if (flag)
		{
			MessageBoxResult messageBoxResult3;
			if (LanguageType == 1)
			{
				text += "訓練好的模型尚未更換，是否放棄更新，離開程式?";
				messageBoxResult3 = MessageBox.Show(text, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 2)
			{
				text += "训练好的模型尚未更换，是否放弃更新，离开程式?";
				messageBoxResult3 = MessageBox.Show(text, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 3)
			{
				text += "トレーニング済みのモデルは未交換です。更新を放棄して、プログラムを終了しますか？";
				messageBoxResult3 = MessageBox.Show(text, "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else
			{
				text += "Training Model will not be saved. exit?";
				messageBoxResult3 = MessageBox.Show(text, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			if (messageBoxResult3 != MessageBoxResult.Yes)
			{
				return;
			}
		}
		for (int j = 0; j < 8; j++)
		{
			if (ModelTrained[j])
			{
				string path = string.Format("{0}{1}\\", AppDomain.CurrentDomain.BaseDirectory, "Model");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				string arg = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[j][0]].ModelName_A));
				string path2 = string.Format("{0}{1}{2}.h5d", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
				string path3 = string.Format("{0}{1}{2}.pbd", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
				string path4 = string.Format("{0}{1}{2}.trt", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
				if (File.Exists(path3))
				{
					File.Delete(path3);
				}
				if (File.Exists(path4))
				{
					File.Delete(path4);
				}
			}
		}
		Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DelTempProcFile(bool ClearRemote = true)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path, "*.proc", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
		path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Proc\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files2 = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
		for (int j = 0; j < files2.Length; j++)
		{
			File.Delete(files2[j]);
		}
		path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\Proc\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files3 = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
		for (int k = 0; k < files3.Length; k++)
		{
			File.Delete(files3[k]);
		}
		string path2 = string.Format("{0}RemoteTraining\\Proc\\", AppDomain.CurrentDomain.BaseDirectory);
		if (!Directory.Exists(path2))
		{
			Directory.CreateDirectory(path2);
		}
		string[] files4 = Directory.GetFiles(path2, "*.txt", SearchOption.AllDirectories);
		for (int l = 0; l < files4.Length; l++)
		{
			File.Delete(files4[l]);
		}
		if (ClearRemote)
		{
			path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Comm\\");
			string[] files5 = Directory.GetFiles(path, "*.finish", SearchOption.AllDirectories);
			for (int m = 0; m < files5.Length; m++)
			{
				File.Delete(files5[m]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckTrainingFile()
	{
		string text = "";
		string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\train.exe");
		string text3 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\h5_to_pb.exe");
		if (!File.Exists(text2))
		{
			if (!File.Exists(text2))
			{
				text = ((LanguageType == 1) ? (text + string.Format("[AI前處理] 找不到訓練主程式 {0}。\n", text2)) : ((LanguageType == 2) ? (text + string.Format("[AI前处理] 找不到训练主程式 {0}。\n", text2)) : ((LanguageType != 3) ? (text + string.Format("[AI Enhence] Cannot find the training program {0}。\n", text2)) : (text + string.Format("[AI前処理] トレーニング主プログラム {0} が見つかりません。\n", text2)))));
			}
			if (!File.Exists(text3))
			{
				text = ((LanguageType == 1) ? (text + string.Format("[AI前處理] 找不到PB轉換程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text3)) : ((LanguageType == 2) ? (text + string.Format("[AI前处理] 找不到PB转换程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text3)) : ((LanguageType != 3) ? (text + string.Format("[AI Enhence] Cannot find the PB conversion program (Training\\Model\\h5_to_pb.exe)。\n", text3)) : (text + string.Format("[AI前処理] PB 変換プログラム (Training\\Model\\h5_to_pb.exe) {0} が見つかりません。\n", text3)))));
			}
			MessageBox.Show(text);
			return false;
		}
		txt_TargetImageFolder.Background = Brushes.White;
		if (txt_TargetImageFolder.Text.Contains("\\") || txt_TargetImageFolder.Text.Contains("/") || txt_TargetImageFolder.Text.Contains(":") || txt_TargetImageFolder.Text.Contains("\"") || txt_TargetImageFolder.Text.Contains("<") || txt_TargetImageFolder.Text.Contains(">") || txt_TargetImageFolder.Text.Contains("|") || txt_TargetImageFolder.Text.Contains("?") || txt_TargetImageFolder.Text.Contains("*"))
		{
			string text4 = "";
			text4 = ((LanguageType == 1) ? string.Format("您輸入的資料夾名稱不合法({0})，資料夾名稱不得包含以下字元 \\ / : * \" < > | ?", txt_TargetImageFolder.Text) : ((LanguageType == 2) ? string.Format("您输入的资料夹名称不合法({0})，资料夹名称不得包含以下字元 \\ / : * \" < > | ?", txt_TargetImageFolder.Text) : ((LanguageType != 3) ? string.Format("The folder name you entered is invalid ({0}). Folder names must not contain the following characters: \\ / : * \" < > | ? ", txt_TargetImageFolder.Text) : string.Format("入力したフォルダー名が無効です({0})。フォルダー名には、次の文字を含めることはできません。\\ / : * \" < > | ? ", txt_TargetImageFolder.Text))));
			MessageBox.Show(text4, "Warning");
			txt_TargetImageFolder.Background = Brushes.Fuchsia;
			txt_TargetImageFolder.Text = "ALL";
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Training_Click(object sender, RoutedEventArgs e)
	{
		if (!CheckTrainingFile())
		{
			return;
		}
		int num = 0;
		lab_RemoteTraining.Visibility = Visibility.Hidden;
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\proc\\Interrupt.txt");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			InterruptMode = false;
			TrainingMsg = "";
			DateTime now = DateTime.Now;
			TrainingMsg = string.Format("{0} Training Start.....\n\n", now.ToString("yyyy-MM-dd-HH-mm-ss"));
			HideTrainingWnd = chk_HideTrainingWnd.IsChecked.Value;
			BufferRetest = chk_EnableBufferRetest.IsChecked.Value;
			AutoUpdateModel = chk_AutoUpdateModel.IsChecked.Value;
			num = 1;
			for (int i = 0; i < 8; i++)
			{
				int num2 = 0;
				for (int j = 0; j < AllModel[i].Count; j++)
				{
					if (ChkStationEnable[AllModel[i][j]].IsChecked == true)
					{
						num2++;
					}
					if (num2 == 0)
					{
						ModelEnable[i] = false;
					}
				}
				if (AllModel[i].Count > 0)
				{
					string arg = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[i][0]].ModelName_A));
					string path2 = string.Format("{0}{1}{2}.h5d", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
					string path3 = string.Format("{0}{1}{2}.pbd", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
					if (File.Exists(path2))
					{
						File.Delete(path2);
					}
					if (File.Exists(path3))
					{
						File.Delete(path3);
					}
				}
			}
			num = 2;
			for (int k = 0; k < 8; k++)
			{
				CurrentPBTestValue[k] = 0;
				PBProgress[k].Value = 0.0;
				if (BufferRetest)
				{
					PBTestProgress[k].Value = 0.0;
				}
				else
				{
					PBTestProgress[k].Value = 100.0;
				}
				LabProgress[k].Content = 0;
				LabTestProgress[k].Content = 0;
				LabAcc[k].Content = 0;
				LabYield[k].Content = 0;
				LabNewYield[k].Content = 0;
				BTNUpdate[k].IsEnabled = false;
				NewYield[k] = 0.0;
				Yield[k] = 0.0;
				Station[k].paramFile.AIEnhanceParam.Model_A_Changed = false;
				TrainingCount[k] = BoxTrainingCount[k].Text;
				ACCValue[k].Clear();
			}
			num = 3;
			for (int l = 0; l < 8; l++)
			{
				CurrentModelPBValue[l] = 0;
				ModelTraingingLog[l] = "";
			}
			TrainingMsg += "Reset all UI Value.\n";
			DelTempProcFile();
			TrainingMsg += "Delete all temp progress file.\n";
			TestImageSet = cb_TestImageSet.SelectedIndex;
			EnableMoveToTraninngImg = chk_EnableMoveToTrainingImg.IsChecked.Value;
			btn_UpdateAll.IsEnabled = false;
			btn_Training.IsEnabled = false;
			btn_Interrupt_S1.IsEnabled = true;
			grd_Loading.Visibility = Visibility.Visible;
			num = 4;
			for (int m = 0; m < ModelNumber; m++)
			{
				for (int n = 0; n < AllModel[m].Count; n++)
				{
					LabStation[AllModel[m][n]].Background = ModelColor[m];
				}
			}
			DateTime now2 = DateTime.Now;
			TrainingDateTime = string.Format("{0}", now2.ToString("yyyy-MM-dd-HH-mm"));
			string text = "";
			text = "ALL";
			num = 5;
			for (int num3 = 0; num3 < ModelNumber; num3++)
			{
				string str = "";
				if (!CombineModelImage(num3, text, ref str))
				{
					TrainingMsg += string.Format("({0} {1})\n", AI_Param[AllModel[num3][0]].ModelName_A, str);
					ModelEnable[num3] = false;
					if (LanguageType == 1)
					{
						MessageBox.Show(string.Format("({0} 訓練中止，錯誤訊息: {1})\n", AI_Param[AllModel[num3][0]].ModelName_A, str), "警告");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show(string.Format("({0} 训练中止，错误讯息: {1})\n", AI_Param[AllModel[num3][0]].ModelName_A, str), "警告");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show(string.Format("({0} のトレーニングを中止しました。エラーメッセージ：{1})\n", AI_Param[AllModel[num3][0]].ModelName_A, str), "警告");
					}
					else
					{
						MessageBox.Show(string.Format("({0} Training aborted, Error message: {1})\n", AI_Param[AllModel[num3][0]].ModelName_A, str), "Warning");
					}
				}
				RunDuplicateImageCheck(X_NG_Path[AllModel[num3][0]], X_OK_Path[AllModel[num3][0]], extramode: false);
			}
			TrainingMsg += "Copy and move Station Image. Combine same model image.\n";
			num = 6;
			for (int num4 = 0; num4 < ModelNumber; num4++)
			{
				if (GetStationTrainingImageCount(AllModel[num4][0]) <= 0 || !ModelEnable[num4])
				{
					for (int num5 = 0; num5 < AllModel[num4].Count; num5++)
					{
						GridResult[AllModel[num4][num5]].Visibility = Visibility.Hidden;
					}
				}
			}
			TrainingMsg += "Copy Station Image count.\n";
			btn_Close.Content = "終止訓練";
			Train_Local = true;
			TrainingFinish = false;
			AITrainingPauseEvent.Set();
		}
		catch (Exception ex)
		{
			string text2 = string.Format("AIP01-AITrainingWindow-Btn_Training_Click(DebugCode-{0})", num) + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Update1_Click(object sender, RoutedEventArgs e)
	{
		int station = Convert.ToInt32(((Button)sender).Uid);
		int stationModeID = GetStationModeID(station);
		string str = "";
		if (UpdateModel(stationModeID, ref str))
		{
			MessageBox.Show(str, "Warning");
			for (int i = 0; i < AllModel[stationModeID].Count; i++)
			{
				BTNUpdate[AllModel[stationModeID][i]].IsEnabled = false;
			}
		}
		bool flag = false;
		for (int j = 0; j < 8; j++)
		{
			if (BTNUpdate[j].IsEnabled)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			btn_UpdateAll.IsEnabled = false;
		}
		if (!RemoteMode)
		{
			return;
		}
		string path = string.Format("{0}\\Comm\\", AppDomain.CurrentDomain.BaseDirectory);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string path2 = string.Format("{0}\\Comm\\{1}", AppDomain.CurrentDomain.BaseDirectory, "UpdateModel.msg");
		if (!File.Exists(path2))
		{
			using (StreamWriter streamWriter = new StreamWriter(path2, append: true))
			{
				streamWriter.Write("{0}", "");
				streamWriter.WriteLine();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool UpdateModel(int modelid, ref string str)
	{
		try
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
			if (File.Exists(path))
			{
				File.Delete(path);
				ResetProgress = true;
			}
			string text = string.Format("{0}Model\\Backup\\", AppDomain.CurrentDomain.BaseDirectory);
			string arg = string.Format("{0}Model\\Backup\\", "C:\\DiaChip8Side\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			int num = AllModel[modelid][0];
			bool flag = false;
			if (StationAIMode[num] == AI_MODE.EM_Predict)
			{
				flag = true;
			}
			string text2 = ((!flag) ? string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, Station[num].paramFile.AIEnhanceParam.ModelName_A) : string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, Station[num].paramFile.AIPredictParam.ModelName_A));
			string arg2 = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[modelid][0]].ModelName_A));
			string text3 = ((!flag) ? string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, arg2) : string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, arg2));
			bool flag2 = true;
			if (!File.Exists(text2))
			{
				flag2 = false;
			}
			DateTime lastWriteTime = new FileInfo(text2).LastWriteTime;
			string text4 = "";
			string lastModelFile_A = "";
			string text5 = "";
			if (flag)
			{
				text5 = Station[num].paramFile.AIPredictParam.ModelName_A;
				text4 = string.Format("{0}{1}{2}", text, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), text5);
			}
			else
			{
				text5 = Station[num].paramFile.AIEnhanceParam.ModelName_A;
				text4 = string.Format("{0}{1}{2}", text, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), text5);
			}
			if (RemoteMode)
			{
				lastModelFile_A = ((!flag) ? string.Format("{0}{1}{2}", arg, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), Station[num].paramFile.AIEnhanceParam.ModelName_A) : string.Format("{0}{1}{2}", arg, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), Station[num].paramFile.AIPredictParam.ModelName_A));
			}
			RemoveOldBackupModel(text5);
			if (File.Exists(text3))
			{
				bool flag3 = false;
				if (!((!flag) ? Station[num].paramFile.AIEnhanceParam.UseTensorRT : Station[num].paramFile.AIPredictParam.UseTensorRT))
				{
					string text6 = ((!flag) ? (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".pbd") : (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIPredictParam.ModelName_A) + ".pb"));
					RemoveOldBackupModel(text6);
					string text7 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, text6);
					if (flag2)
					{
						MoveFile(text2, text4);
						if (flag)
						{
							string text8 = text2.Replace(".h5", ".pb");
							string text9 = text4.Replace(".h5", ".pb");
							if (File.Exists(text9))
							{
								FileInfo fileInfo = new FileInfo(text9);
								if (fileInfo.IsReadOnly)
								{
									fileInfo.IsReadOnly = false;
								}
								File.Delete(text9);
							}
							if (File.Exists(text8))
							{
								MoveFile(text8, text9);
							}
						}
						else
						{
							string text10 = text2.Replace(".h5d", ".pbd");
							string text11 = text4.Replace(".h5d", ".pbd");
							if (File.Exists(text11))
							{
								FileInfo fileInfo2 = new FileInfo(text11);
								if (fileInfo2.IsReadOnly)
								{
									fileInfo2.IsReadOnly = false;
								}
								File.Delete(text11);
							}
							if (File.Exists(text10))
							{
								MoveFile(text10, text11);
							}
						}
					}
					if (File.Exists(text7))
					{
						FileInfo fileInfo3 = new FileInfo(text7);
						if (fileInfo3.IsReadOnly)
						{
							fileInfo3.IsReadOnly = false;
						}
						File.Delete(text7);
					}
					MoveFile(text3, text2);
					string text12 = ((!flag) ? string.Format("{0}Model\\{1}.pbd", AppDomain.CurrentDomain.BaseDirectory, arg2) : string.Format("{0}Model\\{1}.pb", AppDomain.CurrentDomain.BaseDirectory, arg2));
					if (File.Exists(text12))
					{
						MoveFile(text12, text7);
					}
					string arg3 = Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".Trt";
					string path2 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg3);
					if (File.Exists(path2))
					{
						File.Delete(path2);
					}
				}
				else
				{
					string text13 = ((!flag) ? (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".Trt") : (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIPredictParam.ModelName_A) + ".Trt"));
					RemoveOldBackupModel(text13);
					string text14 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, text13);
					if (flag2)
					{
						MoveFile(text2, text4);
						if (flag)
						{
							string text15 = text2.Replace(".h5", ".Trt");
							string text16 = text4.Replace(".h5", ".Trt");
							if (File.Exists(text16))
							{
								FileInfo fileInfo4 = new FileInfo(text16);
								if (fileInfo4.IsReadOnly)
								{
									fileInfo4.IsReadOnly = false;
								}
								File.Delete(text16);
							}
							if (File.Exists(text15))
							{
								MoveFile(text15, text16);
							}
						}
						else
						{
							string text17 = text2.Replace(".h5d", ".Trt");
							string text18 = text4.Replace(".h5d", ".Trt");
							if (File.Exists(text18))
							{
								FileInfo fileInfo5 = new FileInfo(text18);
								if (fileInfo5.IsReadOnly)
								{
									fileInfo5.IsReadOnly = false;
								}
								File.Delete(text18);
							}
							if (File.Exists(text17))
							{
								MoveFile(text17, text18);
							}
						}
					}
					if (File.Exists(text14))
					{
						FileInfo fileInfo6 = new FileInfo(text14);
						if (fileInfo6.IsReadOnly)
						{
							fileInfo6.IsReadOnly = false;
						}
						File.Delete(text14);
					}
					MoveFile(text3, text2);
					string text19 = ((!flag) ? string.Format("{0}Model\\{1}.Trt", AppDomain.CurrentDomain.BaseDirectory, arg2) : string.Format("{0}Model\\{1}.Trt", AppDomain.CurrentDomain.BaseDirectory, arg2));
					if (File.Exists(text19))
					{
						MoveFile(text19, text14);
					}
					string arg4 = Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".pbd";
					string path3 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg4);
					if (File.Exists(path3))
					{
						File.Delete(path3);
					}
				}
				ModelTrained[modelid] = false;
				int num2 = 0;
				for (int i = 0; i < AllModel[modelid].Count; i++)
				{
					num2 = AllModel[modelid][i];
					str += string.Format("第{0}站 Model更新完成!\n", num2 + 1);
					if (flag)
					{
						Station[num2].paramFile.AIPredictParam.Model_A_Changed = true;
						Station[num2].paramFile.AIPredictParam.NewModelMode = false;
						Station[num2].paramFile.AIPredictParam.ValueChangeFlag = true;
						if (flag2)
						{
							Station[num2].paramFile.AIPredictParam.LastModelFile_A = text4;
						}
						else
						{
							Station[num2].paramFile.AIPredictParam.LastModelFile_A = "";
						}
						if (RemoteMode)
						{
							if (flag2)
							{
								Station[num2].paramFile.AIPredictParam.LastModelFile_A = lastModelFile_A;
							}
							else
							{
								Station[num2].paramFile.AIPredictParam.LastModelFile_A = "";
							}
							Station[num2].paramFile.WriteAIPredictParam();
						}
						continue;
					}
					Station[num2].paramFile.AIEnhanceParam.Model_A_Changed = true;
					Station[num2].paramFile.AIEnhanceParam.NewModelMode = false;
					Station[num2].paramFile.AIEnhanceParam.ValueChangeFlag = true;
					if (flag2)
					{
						Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = text4;
					}
					else
					{
						Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = "";
					}
					if (RemoteMode)
					{
						if (flag2)
						{
							Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = lastModelFile_A;
						}
						else
						{
							Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = "";
						}
						Station[num2].paramFile.WriteAIEnhanceParam();
					}
				}
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			string text20 = "AIP01-AITrainingWindow-UpdateModel()" + ex.ToString();
			CVBuffer.SaveLog(text20);
			MessageBox.Show(text20, LanguageManager.Instance["Error"]);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RemoveOldBackupModel(string modelname)
	{
		try
		{
			string path = string.Format("{0}Model\\Backup\\", AppDomain.CurrentDomain.BaseDirectory);
			string searchPattern = string.Format("*{0}", modelname);
			string[] arrFi = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
			SortAsFileTime(ref arrFi);
			if (arrFi.Length > 3)
			{
				for (int i = 0; i < arrFi.Length - 3; i++)
				{
					File.Delete(arrFi[i]);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "AIP01-AITrainingWindow-RemoveOldBackupModel()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	private void SortAsFileTime(ref string[] arrFi)
	{
		Array.Sort(arrFi, (string x, string y) => x.CompareTo(y));
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void KillTask(string ProcessName)
	{
		try
		{
			using Process process = new Process();
			process.StartInfo = new ProcessStartInfo
			{
				FileName = "taskkill",
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = "/F /IM \"" + ProcessName + "\""
			};
			process.Start();
			process.WaitForExit(60000);
		}
		catch
		{
			using Process process2 = new Process();
			process2.StartInfo = new ProcessStartInfo
			{
				FileName = "tskill",
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = "\"" + ProcessName + "\" /A /V"
			};
			process2.Start();
			process2.WaitForExit(60000);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_UpdateAll_Click(object sender, RoutedEventArgs e)
	{
		string text = "";
		for (int i = 0; i < ModelNumber; i++)
		{
			if (!ModelTrained[i])
			{
				continue;
			}
			int num = AllModel[i][0];
			bool flag = false;
			if (StationAIMode[num] == AI_MODE.EM_Predict)
			{
				flag = true;
			}
			string text2 = string.Format("{0}Model\\Backup\\", AppDomain.CurrentDomain.BaseDirectory);
			string arg = string.Format("{0}Model\\Backup\\", "C:\\DiaChip8Side\\");
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string text3 = ((!flag) ? string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, Station[num].paramFile.AIEnhanceParam.ModelName_A) : string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, Station[num].paramFile.AIPredictParam.ModelName_A));
			if (AllModel[i].Count < 0)
			{
				continue;
			}
			string arg2 = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[i][0]].ModelName_A));
			string text4 = ((!flag) ? string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, arg2) : string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, arg2));
			bool flag2 = true;
			if (!File.Exists(text3))
			{
				flag2 = false;
			}
			DateTime lastWriteTime = new FileInfo(text3).LastWriteTime;
			string text5 = "";
			string lastModelFile_A = "";
			text5 = ((!flag) ? string.Format("{0}{1}{2}", text2, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), Station[num].paramFile.AIEnhanceParam.ModelName_A) : string.Format("{0}{1}{2}", text2, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), Station[num].paramFile.AIPredictParam.ModelName_A));
			if (RemoteMode)
			{
				lastModelFile_A = ((!flag) ? string.Format("{0}{1}{2}", arg, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), Station[num].paramFile.AIEnhanceParam.ModelName_A) : string.Format("{0}{1}{2}", arg, lastWriteTime.ToString("yyyy-MM-dd-HHmm_"), Station[num].paramFile.AIPredictParam.ModelName_A));
			}
			if (!File.Exists(text4))
			{
				continue;
			}
			bool flag3 = false;
			if (!((!flag) ? Station[num].paramFile.AIEnhanceParam.UseTensorRT : Station[num].paramFile.AIPredictParam.UseTensorRT))
			{
				string text6 = string.Format(arg1: (!flag) ? (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".pbd") : (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIPredictParam.ModelName_A) + ".pb"), format: "{0}Model\\{1}", arg0: AppDomain.CurrentDomain.BaseDirectory);
				if (flag2)
				{
					MoveFile(text3, text5);
					if (flag)
					{
						string text7 = text3.Replace(".h5", ".pb");
						string text8 = text5.Replace(".h5", ".pb");
						if (File.Exists(text8))
						{
							FileInfo fileInfo = new FileInfo(text8);
							if (fileInfo.IsReadOnly)
							{
								fileInfo.IsReadOnly = false;
							}
							File.Delete(text8);
						}
						if (File.Exists(text7))
						{
							MoveFile(text7, text8);
						}
					}
					else
					{
						string text9 = text3.Replace(".h5d", ".pbd");
						string text10 = text5.Replace(".h5d", ".pbd");
						if (File.Exists(text10))
						{
							FileInfo fileInfo2 = new FileInfo(text10);
							if (fileInfo2.IsReadOnly)
							{
								fileInfo2.IsReadOnly = false;
							}
							File.Delete(text10);
						}
						if (File.Exists(text9))
						{
							MoveFile(text9, text10);
						}
					}
				}
				if (File.Exists(text6))
				{
					FileInfo fileInfo3 = new FileInfo(text6);
					if (fileInfo3.IsReadOnly)
					{
						fileInfo3.IsReadOnly = false;
					}
					File.Delete(text6);
				}
				MoveFile(text4, text3);
				string text11 = ((!flag) ? string.Format("{0}Model\\{1}.pbd", AppDomain.CurrentDomain.BaseDirectory, arg2) : string.Format("{0}Model\\{1}.pb", AppDomain.CurrentDomain.BaseDirectory, arg2));
				if (File.Exists(text11))
				{
					MoveFile(text11, text6);
				}
				string arg3 = Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".Trt";
				string path = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg3);
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			else
			{
				string text12 = string.Format(arg1: (!flag) ? (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".Trt") : (Path.GetFileNameWithoutExtension(Station[num].paramFile.AIPredictParam.ModelName_A) + ".Trt"), format: "{0}Model\\{1}", arg0: AppDomain.CurrentDomain.BaseDirectory);
				if (flag2)
				{
					MoveFile(text3, text5);
					if (flag)
					{
						string text13 = text3.Replace(".h5", ".Trt");
						string text14 = text5.Replace(".h5", ".Trt");
						if (File.Exists(text14))
						{
							FileInfo fileInfo4 = new FileInfo(text14);
							if (fileInfo4.IsReadOnly)
							{
								fileInfo4.IsReadOnly = false;
							}
							File.Delete(text14);
						}
						if (File.Exists(text13))
						{
							MoveFile(text13, text14);
						}
					}
					else
					{
						string text15 = text3.Replace(".h5d", ".Trt");
						string text16 = text5.Replace(".h5d", ".Trt");
						if (File.Exists(text16))
						{
							FileInfo fileInfo5 = new FileInfo(text16);
							if (fileInfo5.IsReadOnly)
							{
								fileInfo5.IsReadOnly = false;
							}
							File.Delete(text16);
						}
						if (File.Exists(text15))
						{
							MoveFile(text15, text16);
						}
					}
				}
				if (File.Exists(text12))
				{
					FileInfo fileInfo6 = new FileInfo(text12);
					if (fileInfo6.IsReadOnly)
					{
						fileInfo6.IsReadOnly = false;
					}
					File.Delete(text12);
				}
				MoveFile(text4, text3);
				string text17 = ((!flag) ? string.Format("{0}Model\\{1}.Trt", AppDomain.CurrentDomain.BaseDirectory, arg2) : string.Format("{0}Model\\{1}.Trt", AppDomain.CurrentDomain.BaseDirectory, arg2));
				if (File.Exists(text17))
				{
					MoveFile(text17, text12);
				}
				string arg4 = Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A) + ".pbd";
				string path2 = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg4);
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
			}
			ModelTrained[i] = false;
			int num2 = 0;
			for (int j = 0; j < AllModel[i].Count; j++)
			{
				num2 = AllModel[i][j];
				text += string.Format("第{0}站 Model更新完成!\n", num2 + 1);
				if (flag)
				{
					Station[num2].paramFile.AIPredictParam.Model_A_Changed = true;
					if (flag2)
					{
						Station[num2].paramFile.AIPredictParam.LastModelFile_A = text5;
					}
					else
					{
						Station[num2].paramFile.AIPredictParam.LastModelFile_A = "";
					}
					Station[num2].paramFile.AIPredictParam.ValueChangeFlag = true;
					if (RemoteMode)
					{
						if (flag2)
						{
							Station[num2].paramFile.AIPredictParam.LastModelFile_A = lastModelFile_A;
						}
						else
						{
							Station[num2].paramFile.AIPredictParam.LastModelFile_A = "";
						}
						Station[num2].paramFile.WriteAIPredictParam();
					}
				}
				else
				{
					Station[num2].paramFile.AIEnhanceParam.Model_A_Changed = true;
					if (flag2)
					{
						Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = text5;
					}
					else
					{
						Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = "";
					}
					Station[num2].paramFile.AIEnhanceParam.ValueChangeFlag = true;
					if (RemoteMode)
					{
						if (flag2)
						{
							Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = lastModelFile_A;
						}
						else
						{
							Station[num2].paramFile.AIEnhanceParam.LastModelFile_A = "";
						}
						Station[num2].paramFile.WriteAIEnhanceParam();
					}
				}
				BTNUpdate[num2].IsEnabled = false;
			}
		}
		if (text != "")
		{
			MessageBox.Show(text, "Warning");
		}
		btn_UpdateAll.IsEnabled = false;
		if (!RemoteMode)
		{
			return;
		}
		string path3 = string.Format("{0}\\Comm\\", AppDomain.CurrentDomain.BaseDirectory);
		if (!Directory.Exists(path3))
		{
			Directory.CreateDirectory(path3);
		}
		string path4 = string.Format("{0}\\Comm\\{1}", AppDomain.CurrentDomain.BaseDirectory, "UpdateModel.msg");
		if (!File.Exists(path4))
		{
			using (StreamWriter streamWriter = new StreamWriter(path4, append: true))
			{
				streamWriter.Write("{0}", "");
				streamWriter.WriteLine();
			}
		}
	}

	private void Txt_TrainingCountS1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
	}

	private void SetUnsignedINTKeyboard(TextBox tbox, Point p)
	{
		tbox.IsReadOnly = true;
		HKeyboard hKeyboard = new HKeyboard();
		hKeyboard.setTxtbox(tbox);
		hKeyboard.SetInput(tbox.Text);
		hKeyboard.SetInputNegative(state: false);
		hKeyboard.SetDouble(state: false);
		hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
		hKeyboard.Left = p.X;
		hKeyboard.Top = p.Y - hKeyboard.Height + wnd_Main.Top;
		tbox.Background = Brushes.Gold;
		hKeyboard.ShowDialog();
		tbox.Background = Brushes.White;
	}

	private void Btn_SetAIParam_1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		AIParamSetupWindow aIParamSetupWindow = new AIParamSetupWindow();
		aIParamSetupWindow.LoadLanguage(LanguageType);
		aIParamSetupWindow.SetInspection(Station[num], ThroughputFile, num);
		aIParamSetupWindow.ShowDialog();
		if (ThroughputFile.param.TrainingImageDoubleRate[num] > 0.0 || ThroughputFile.param.BatchSize[num] != 8 || ThroughputFile.param.DnetDiv[num] != 8)
		{
			BtnSetAIParam[num].Background = Brushes.Fuchsia;
		}
		else
		{
			BtnSetAIParam[num].Background = Brushes.Orange;
		}
	}

	private void Lab_RS1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		int station = Convert.ToInt32(((Label)sender).Uid);
		txt_CmdResult.Text = ModelTraingingLog[GetStationModeID(station)];
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Preview1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		AIPreviewWindow aIPreviewWindow = new AIPreviewWindow();
		aIPreviewWindow.SetStation(Station[num], num);
		aIPreviewWindow.LoadLanguage(LanguageType);
		aIPreviewWindow.SetSAM(mSam);
		int stationModeID = GetStationModeID(num);
		int num2 = AllModel[stationModeID][0];
		aIPreviewWindow.SetThroughputFile(ThroughputFile);
		string newmodel = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[stationModeID][0]].ModelName_A));
		aIPreviewWindow.SetModel(newmodel, Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A));
		aIPreviewWindow.SetImagePath(X_NG_Path[num2], X_OK_Path[num2]);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Station[num].paramFile.AIEnhanceParam.ModelName_A);
		string xng = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, fileNameWithoutExtension);
		string yng = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, fileNameWithoutExtension);
		aIPreviewWindow.SetNGMarkedFolder(xng, yng);
		aIPreviewWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string FindSameImage(string path1, string path2)
	{
		string text = "";
		if (!Directory.Exists(path1))
		{
			return "Path1路徑錯誤";
		}
		if (!Directory.Exists(path2))
		{
			return "Path2路徑錯誤";
		}
		try
		{
			if (path1 == "")
			{
				return text;
			}
			string[] files = Directory.GetFiles(path1, "*.*", SearchOption.AllDirectories);
			string[] array = null;
			if (path2 != "")
			{
				array = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
			}
			if (files.Length == 0 && array.Length == 0)
			{
				return text;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < files.Length; i++)
			{
				list.Add(files[i]);
			}
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					list.Add(array[j]);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				list2.Add(GetFileHash(list[k]));
			}
			for (int l = 0; l < list2.Count; l++)
			{
				if (list2[l] == "")
				{
					continue;
				}
				for (int m = 0; m < list2.Count; m++)
				{
					if (list[l] == list[m] || !(list2[l] == list2[m]))
					{
						continue;
					}
					string fileName = Path.GetFileName(list[l]);
					string fileName2 = Path.GetFileName(list[m]);
					string text2 = list[m];
					string text3 = "";
					if (text2.Contains("\\X\\NG\\"))
					{
						text3 = text2.Replace("\\X\\NG\\", "\\Y\\NG\\");
					}
					else if (text2.Contains("\\X\\OK\\"))
					{
						text3 = text2.Replace("\\X\\OK\\", "\\Y\\OK\\");
					}
					if (File.Exists(text2))
					{
						FileInfo fileInfo = new FileInfo(text2);
						if (fileInfo.IsReadOnly)
						{
							fileInfo.IsReadOnly = false;
						}
						File.Delete(text2);
						text += string.Format("{0}影像與{1}影像相同，系統自動刪除原影像{2}\n", fileName, fileName2, fileName2);
					}
					if (File.Exists(text3) && text3 != "")
					{
						FileInfo fileInfo2 = new FileInfo(text3);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
						File.Delete(text3);
						text += string.Format("{0}影像與{1}影像相同，系統自動刪除標註影像{2}\n", fileName, fileName2, Path.GetFileName(text3));
					}
					list2[m] = "";
				}
			}
			return text;
		}
		catch (Exception ex)
		{
			string text4 = "AIP01-AITrainingWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text4);
			MessageBox.Show(text4, LanguageManager.Instance["Error"]);
			return text;
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
	private void Btn_Interrupt_S1_Click(object sender, RoutedEventArgs e)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\proc\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		using (StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\proc\\Interrupt.txt"), append: true))
		{
			streamWriter.Write("{0}", "");
			streamWriter.WriteLine();
		}
		using (StreamWriter streamWriter2 = new StreamWriter(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Comm\\Interrupt.msg"), append: true))
		{
			streamWriter2.Write("{0}", "");
			streamWriter2.WriteLine();
		}
		path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train1\\proc\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		using (StreamWriter streamWriter3 = new StreamWriter(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train1\\proc\\Interrupt.txt"), append: true))
		{
			streamWriter3.Write("{0}", "");
			streamWriter3.WriteLine();
		}
		InterruptMode = true;
		btn_Interrupt_S1.IsEnabled = false;
	}

	private void Lab_acc1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((Label)sender).Uid);
		int stationModeID = GetStationModeID(num);
		GetMainStation(num);
		if (ACCValue[stationModeID].Count > 0)
		{
			ChartWindow chartWindow = new ChartWindow();
			chartWindow.LoadLanguage(LanguageType);
			chartWindow.SetStaticValue(ACCValue[stationModeID], AI_Param[num].ModelName_A);
			chartWindow.ShowDialog();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_LocalAndRemoteTraining_Click(object sender, RoutedEventArgs e)
	{
		if (!CheckTrainingFile())
		{
			return;
		}
		_ = AppDomain.CurrentDomain.BaseDirectory + "Comm\\OnLine.msg";
		if (LanguageType == 1)
		{
			lab_RemoteTraining.Content = "(本機 + 遠端)  訓練模式";
		}
		else if (LanguageType == 2)
		{
			lab_RemoteTraining.Content = "(本机 + 远端)  训练模式";
		}
		else if (LanguageType == 3)
		{
			lab_RemoteTraining.Content = "(ローカル + リモート) トレーニングモード";
		}
		else
		{
			lab_RemoteTraining.Content = "(Local + Remote)  Train Mode";
		}
		int num = 0;
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\proc\\Interrupt.txt");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			string path2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Comm\\Interrupt.msg");
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			string path3 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train1\\proc\\Interrupt.txt");
			if (File.Exists(path3))
			{
				File.Delete(path3);
			}
			string[] files = Directory.GetFiles(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "RemoteTraining\\Proc\\"), "*.*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
			InterruptMode = false;
			TrainingMsg = "";
			DateTime now = DateTime.Now;
			TrainingMsg = string.Format("{0} Training Start.....\n\n", now.ToString("yyyy-MM-dd-HH-mm-ss"));
			HideTrainingWnd = chk_HideTrainingWnd.IsChecked.Value;
			BufferRetest = chk_EnableBufferRetest.IsChecked.Value;
			AutoUpdateModel = chk_AutoUpdateModel.IsChecked.Value;
			Train_LocalAndRemote = true;
			num = 1;
			for (int j = 0; j < 8; j++)
			{
				ModelTrained[j] = false;
				int num2 = 0;
				for (int k = 0; k < AllModel[j].Count; k++)
				{
					if (ChkStationEnable[AllModel[j][k]].IsChecked == true)
					{
						num2++;
					}
					if (num2 == 0)
					{
						ModelEnable[j] = false;
					}
				}
				if (AllModel[j].Count > 0)
				{
					string arg = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[j][0]].ModelName_A));
					string path4 = string.Format("{0}{1}{2}.h5d", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
					string path5 = string.Format("{0}{1}{2}.pbd", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
					if (File.Exists(path4))
					{
						File.Delete(path4);
					}
					if (File.Exists(path5))
					{
						File.Delete(path5);
					}
				}
			}
			num = 2;
			for (int l = 0; l < 8; l++)
			{
				CurrentPBTestValue[l] = 0;
				PBProgress[l].Value = 0.0;
				if (BufferRetest)
				{
					PBTestProgress[l].Value = 0.0;
				}
				else
				{
					PBTestProgress[l].Value = 100.0;
				}
				LabProgress[l].Content = 0;
				LabTestProgress[l].Content = 0;
				LabAcc[l].Content = 0;
				LabYield[l].Content = 0;
				LabNewYield[l].Content = 0;
				BTNUpdate[l].IsEnabled = false;
				NewYield[l] = 0.0;
				Yield[l] = 0.0;
				Station[l].paramFile.AIEnhanceParam.Model_A_Changed = false;
				TrainingCount[l] = BoxTrainingCount[l].Text;
			}
			num = 3;
			for (int m = 0; m < 8; m++)
			{
				CurrentModelPBValue[m] = 0;
				ModelTraingingLog[m] = "";
			}
			TrainingMsg += "Reset all UI Value.\n";
			DelTempProcFile();
			TrainingMsg += "Delete all temp progress file.\n";
			TestImageSet = cb_TestImageSet.SelectedIndex;
			EnableMoveToTraninngImg = chk_EnableMoveToTrainingImg.IsChecked.Value;
			btn_UpdateAll.IsEnabled = false;
			btn_Training.IsEnabled = false;
			btn_Interrupt_S1.IsEnabled = true;
			grd_Loading.Visibility = Visibility.Visible;
			num = 4;
			for (int n = 0; n < ModelNumber; n++)
			{
				for (int num3 = 0; num3 < AllModel[n].Count; num3++)
				{
					LabStation[AllModel[n][num3]].Background = ModelColor[n];
				}
			}
			DateTime now2 = DateTime.Now;
			TrainingDateTime = string.Format("{0}", now2.ToString("yyyy-MM-dd-HH-mm"));
			string text = "";
			text = "ALL";
			for (int num4 = 0; num4 < 8; num4++)
			{
				if (ChkStationEnable[num4].IsChecked == true)
				{
					int stationModeID = GetStationModeID(num4);
					if (stationModeID >= 0)
					{
						ModelEnable[stationModeID] = true;
						GridResult[num4].Visibility = Visibility.Visible;
					}
					else
					{
						GridResult[num4].Visibility = Visibility.Hidden;
					}
				}
				else
				{
					GridResult[num4].Visibility = Visibility.Hidden;
				}
			}
			num = 5;
			for (int num5 = 0; num5 < ModelNumber; num5++)
			{
				string str = "";
				if (!CombineModelImage(num5, text, ref str))
				{
					TrainingMsg += string.Format("({0} {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str);
					ModelEnable[num5] = false;
					if (LanguageType == 1)
					{
						MessageBox.Show(string.Format("({0} 訓練中止，錯誤訊息: {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "警告");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show(string.Format("({0} 训练中止，错误讯息: {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "警告");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show(string.Format("({0} のトレーニングを中止しました。エラーメッセージ：{1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "警告");
					}
					else
					{
						MessageBox.Show(string.Format("({0} Training aborted, Error message: {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "Warning");
					}
				}
				RunDuplicateImageCheck(X_NG_Path[AllModel[num5][0]], X_OK_Path[AllModel[num5][0]], extramode: false);
			}
			TrainingMsg += "Copy and move Station Image. Combine same model image.\n";
			num = 6;
			bool flag = true;
			for (int num6 = 0; num6 < ModelNumber; num6++)
			{
				ModelBypass[num6] = false;
				if (GetStationTrainingImageCount(AllModel[num6][0]) <= 0 || !ModelEnable[num6])
				{
					continue;
				}
				if (flag)
				{
					for (int num7 = 0; num7 < AllModel[num6].Count; num7++)
					{
						GridResult[AllModel[num6][num7]].Visibility = Visibility.Visible;
						LabRemote[AllModel[num6][num7]].Visibility = Visibility.Visible;
						PBTestProgress[AllModel[num6][num7]].Visibility = Visibility.Hidden;
						LabYield[AllModel[num6][num7]].Foreground = Brushes.White;
						LabNewYield[AllModel[num6][num7]].Foreground = Brushes.White;
					}
					CreateRemoteIni2(num6);
					ModelBypass[num6] = true;
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			TrainingMsg += "Copy Station Image count.\n";
			TrainingFinish = false;
			string path6 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
			if (File.Exists(path6))
			{
				File.Delete(path6);
			}
			string path7 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Run.msg";
			using (StreamWriter streamWriter = new StreamWriter(path7, append: true))
			{
				streamWriter.Write("{0}", "");
				streamWriter.WriteLine();
			}
			int num8 = 0;
			do
			{
				num8++;
				Thread.Sleep(500);
				RemoteRunFlag = CheckRemoteRunningFile();
				if (RemoteRunFlag == REMOTERUNFLAG.EM_Running)
				{
					num8 = 20;
				}
			}
			while (num8 < 20);
			CheckRemoteRunningStatus();
			if (RemoteRunFlag != REMOTERUNFLAG.EM_Running)
			{
				if (LanguageType == 1)
				{
					MessageBox.Show("遠端訓練失敗，遠端主機無回應。(找不到Comm\\Training.msg)", "警告");
				}
				else if (LanguageType == 2)
				{
					MessageBox.Show("远端训练失败，远端主机无回应。(找不到Comm\\Training.msg)", "警告");
				}
				else if (LanguageType == 3)
				{
					MessageBox.Show("リモート トレーニングに失敗しました。リモート ホストからの応答がありません。(Comm\\Training.msg が見つかりません。)", "警告");
				}
				else
				{
					MessageBox.Show("Remote training failed, no response from the remote host. (Cannot find Comm\\Training.msg)", "警告");
				}
				if (File.Exists(path7))
				{
					File.Delete(path7);
				}
			}
			else
			{
				lab_RemoteTraining.Visibility = Visibility.Visible;
				UpdateRemoteUITImer.Start();
				AITrainingPauseEvent.Set();
			}
		}
		catch (Exception ex)
		{
			string text2 = string.Format("AIP01-AITrainingWindow-Btn_Training_Click(DebugCode-{0})", num) + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
		}
	}

	private void Btn_SelectALL_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIPredictParam.ModelName_A == "")
			{
				continue;
			}
			ChkStationEnable[i].IsChecked = true;
			for (int j = 0; j < ModelNumber; j++)
			{
				if (AllModel[j][0] == i)
				{
					UpdateTrainCheckBox(i, ChkStationEnable[i]);
				}
			}
		}
	}

	private void Btn_UnSelectALL_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIPredictParam.ModelName_A == "")
			{
				continue;
			}
			ChkStationEnable[i].IsChecked = false;
			for (int j = 0; j < ModelNumber; j++)
			{
				if (AllModel[j][0] == i)
				{
					UpdateTrainCheckBox(i, ChkStationEnable[i]);
				}
			}
		}
	}

	private void Btn_TrainCount1_Click(object sender, RoutedEventArgs e)
	{
		string text = ((Button)sender).Content.ToString();
		for (int i = 0; i < 8; i++)
		{
			if (ChkStationEnable[i].IsChecked != false)
			{
				BoxTrainingCount[i].Text = text;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RemoteTraining_Click(object sender, RoutedEventArgs e)
	{
		if (!CheckTrainingFile())
		{
			return;
		}
		_ = AppDomain.CurrentDomain.BaseDirectory + "Comm\\OnLine.msg";
		int num = 0;
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train1\\proc\\Interrupt.txt");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			string path2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Comm\\Interrupt.msg");
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			string[] files = Directory.GetFiles(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "RemoteTraining\\Proc\\"), "*.*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
			InterruptMode = false;
			TrainingMsg = "";
			DateTime now = DateTime.Now;
			TrainingMsg = string.Format("{0} Training Start.....\n\n", now.ToString("yyyy-MM-dd-HH-mm-ss"));
			HideTrainingWnd = chk_HideTrainingWnd.IsChecked.Value;
			BufferRetest = chk_EnableBufferRetest.IsChecked.Value;
			AutoUpdateModel = chk_AutoUpdateModel.IsChecked.Value;
			num = 1;
			for (int j = 0; j < 8; j++)
			{
				int num2 = 0;
				for (int k = 0; k < AllModel[j].Count; k++)
				{
					if (ChkStationEnable[AllModel[j][k]].IsChecked == true)
					{
						num2++;
					}
					if (num2 == 0)
					{
						ModelEnable[j] = false;
					}
				}
				if (AllModel[j].Count > 0)
				{
					string arg = string.Format("{0}-TempFile", Path.GetFileNameWithoutExtension(AI_Param[AllModel[j][0]].ModelName_A));
					string path3 = string.Format("{0}{1}{2}.h5d", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
					string path4 = string.Format("{0}{1}{2}.pbd", AppDomain.CurrentDomain.BaseDirectory, "Model\\", arg);
					if (File.Exists(path3))
					{
						File.Delete(path3);
					}
					if (File.Exists(path4))
					{
						File.Delete(path4);
					}
				}
			}
			num = 2;
			for (int l = 0; l < 8; l++)
			{
				CurrentPBTestValue[l] = 0;
				PBProgress[l].Value = 0.0;
				if (BufferRetest)
				{
					PBTestProgress[l].Value = 0.0;
				}
				else
				{
					PBTestProgress[l].Value = 100.0;
				}
				LabProgress[l].Content = 0;
				LabTestProgress[l].Content = 0;
				LabAcc[l].Content = 0;
				LabYield[l].Content = 0;
				LabNewYield[l].Content = 0;
				BTNUpdate[l].IsEnabled = false;
				NewYield[l] = 0.0;
				Yield[l] = 0.0;
				Station[l].paramFile.AIEnhanceParam.Model_A_Changed = false;
				TrainingCount[l] = BoxTrainingCount[l].Text;
			}
			num = 3;
			for (int m = 0; m < 8; m++)
			{
				CurrentModelPBValue[m] = 0;
				ModelTraingingLog[m] = "";
			}
			TrainingMsg += "Reset all UI Value.\n";
			DelTempProcFile();
			TrainingMsg += "Delete all temp progress file.\n";
			TestImageSet = cb_TestImageSet.SelectedIndex;
			EnableMoveToTraninngImg = chk_EnableMoveToTrainingImg.IsChecked.Value;
			btn_UpdateAll.IsEnabled = false;
			btn_Training.IsEnabled = false;
			btn_Interrupt_S1.IsEnabled = true;
			grd_Loading.Visibility = Visibility.Visible;
			num = 4;
			for (int n = 0; n < ModelNumber; n++)
			{
				for (int num3 = 0; num3 < AllModel[n].Count; num3++)
				{
					LabStation[AllModel[n][num3]].Background = ModelColor[n];
				}
			}
			DateTime now2 = DateTime.Now;
			TrainingDateTime = string.Format("{0}", now2.ToString("yyyy-MM-dd-HH-mm"));
			string text = "";
			text = "ALL";
			for (int num4 = 0; num4 < 8; num4++)
			{
				if (ChkStationEnable[num4].IsChecked == true)
				{
					int stationModeID = GetStationModeID(num4);
					if (stationModeID >= 0)
					{
						ModelEnable[stationModeID] = true;
						GridResult[num4].Visibility = Visibility.Visible;
					}
					else
					{
						GridResult[num4].Visibility = Visibility.Hidden;
					}
				}
				else
				{
					GridResult[num4].Visibility = Visibility.Hidden;
				}
			}
			num = 5;
			for (int num5 = 0; num5 < ModelNumber; num5++)
			{
				string str = "";
				if (!CombineModelImage(num5, text, ref str))
				{
					TrainingMsg += string.Format("({0} {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str);
					ModelEnable[num5] = false;
					if (LanguageType == 1)
					{
						MessageBox.Show(string.Format("({0} 訓練中止，錯誤訊息: {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "警告");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show(string.Format("({0} 训练中止，错误讯息: {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "警告");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show(string.Format("({0} のトレーニングを中止しました。エラーメッセージ：{1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "警告");
					}
					else
					{
						MessageBox.Show(string.Format("({0} Training aborted, Error message: {1})\n", AI_Param[AllModel[num5][0]].ModelName_A, str), "Warning");
					}
				}
				RunDuplicateImageCheck(X_NG_Path[AllModel[num5][0]], X_OK_Path[AllModel[num5][0]], extramode: false);
			}
			TrainingMsg += "Copy and move Station Image. Combine same model image.\n";
			num = 6;
			for (int num6 = 0; num6 < ModelNumber; num6++)
			{
				if (GetStationTrainingImageCount(AllModel[num6][0]) > 0 && ModelEnable[num6])
				{
					for (int num7 = 0; num7 < AllModel[num6].Count; num7++)
					{
						LabRemote[AllModel[num6][num7]].Visibility = Visibility.Visible;
						PBTestProgress[AllModel[num6][num7]].Visibility = Visibility.Hidden;
						LabYield[AllModel[num6][num7]].Foreground = Brushes.White;
						LabNewYield[AllModel[num6][num7]].Foreground = Brushes.White;
					}
					CreateRemoteIni2(num6);
				}
			}
			TrainingMsg += "Copy Station Image count.\n";
			TrainingFinish = false;
			string path5 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Finish.msg";
			if (File.Exists(path5))
			{
				File.Delete(path5);
			}
			string path6 = AppDomain.CurrentDomain.BaseDirectory + "Comm\\Run.msg";
			using (StreamWriter streamWriter = new StreamWriter(path6, append: true))
			{
				streamWriter.Write("{0}", "");
				streamWriter.WriteLine();
			}
			int num8 = 0;
			do
			{
				num8++;
				Thread.Sleep(500);
				RemoteRunFlag = CheckRemoteRunningFile();
				if (RemoteRunFlag == REMOTERUNFLAG.EM_Running)
				{
					num8 = 10;
				}
			}
			while (num8 < 10);
			CheckRemoteRunningStatus();
			if (RemoteRunFlag != REMOTERUNFLAG.EM_Running)
			{
				if (LanguageType == 1)
				{
					MessageBox.Show("遠端訓練失敗，遠端主機無回應。(找不到Comm\\Training.msg)", "警告");
				}
				else if (LanguageType == 2)
				{
					MessageBox.Show("远端训练失败，远端主机无回应。(找不到Comm\\Training.msg)", "警告");
				}
				else if (LanguageType == 3)
				{
					MessageBox.Show("リモート トレーニングに失敗しました。リモート ホストからの応答がありません。(原因：Comm\\Training.msg が見つかりません。)", "警告");
				}
				else
				{
					MessageBox.Show("\"Remote training failed, no response from the remote host. (Cannot find Comm\\Training.msg)", "警告");
				}
				if (File.Exists(path6))
				{
					File.Delete(path6);
				}
				return;
			}
			if (LanguageType == 1)
			{
				btn_Close.Content = "離開";
			}
			else if (LanguageType == 2)
			{
				btn_Close.Content = "离开";
			}
			else if (LanguageType == 3)
			{
				btn_Close.Content = "プログラムを終了します";
			}
			else
			{
				btn_Close.Content = "離開";
			}
			btn_Close.Background = Brushes.Orange;
			Train_Remote = true;
			lab_RemoteTraining.Visibility = Visibility.Visible;
			UpdateRemoteUITImer.Start();
		}
		catch (Exception ex)
		{
			string text2 = string.Format("AIP01-AITrainingWindow-Btn_Training_Click(DebugCode-{0})", num) + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
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
			Uri resourceLocator = new Uri("/InspTools;component/other/aitrainingwindow.xaml", UriKind.Relative);
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
			wnd_Main = (AITrainingWIndow)target;
			wnd_Main.Closed += Wnd_Main_Closed;
			break;
		case 2:
			grd_Main = (Grid)target;
			break;
		case 3:
			grd_S1 = (Grid)target;
			break;
		case 4:
			lab_OKTrainingS1 = (Label)target;
			break;
		case 5:
			txt_OKNumS1 = (TextBox)target;
			break;
		case 6:
			btn_OpenOKS1 = (Button)target;
			btn_OpenOKS1.Click += Btn_OpenOKS1_Click;
			break;
		case 7:
			lab_S1 = (Label)target;
			break;
		case 8:
			lab_NGTrainingS1 = (Label)target;
			break;
		case 9:
			txt_NGNumS1 = (TextBox)target;
			break;
		case 10:
			btn_OpenNGS1 = (Button)target;
			btn_OpenNGS1.Click += Btn_OpenNGS1_Click;
			break;
		case 11:
			lab_ModelS1 = (Label)target;
			break;
		case 12:
			grd_CheckS1 = (Grid)target;
			break;
		case 13:
			chk_TrainingS1 = (CheckBox)target;
			chk_TrainingS1.Click += Chk_TrainingS1_Click;
			break;
		case 14:
			txt_TrainingCountS1 = (TextBox)target;
			txt_TrainingCountS1.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 15:
			lab_TrainingCountS1 = (Label)target;
			break;
		case 16:
			lab_DNet_S1 = (Label)target;
			break;
		case 17:
			btn_SetAIParam_1 = (Button)target;
			btn_SetAIParam_1.Click += Btn_SetAIParam_1_Click;
			break;
		case 18:
			btn_Training = (Button)target;
			btn_Training.Click += Btn_Training_Click;
			break;
		case 19:
			chk_HideTrainingWnd = (CheckBox)target;
			break;
		case 20:
			chk_EnableBufferRetest = (CheckBox)target;
			break;
		case 21:
			cb_TestImageSet = (ComboBox)target;
			break;
		case 22:
			chk_EnableBackupTrainingImg = (CheckBox)target;
			break;
		case 23:
			chk_AutoUpdateModel = (CheckBox)target;
			break;
		case 24:
			chk_EnableMoveToTrainingImg = (CheckBox)target;
			break;
		case 25:
			chk_SaveImgUseDateForlder = (CheckBox)target;
			break;
		case 26:
			txt_TargetImageFolder = (TextBox)target;
			break;
		case 27:
			lab_SaveImgUseDateForlder = (Label)target;
			break;
		case 28:
			btn_Close_Copy = (Button)target;
			btn_Close_Copy.Click += Btn_Close_Click;
			break;
		case 29:
			btn_RemoteTraining = (Button)target;
			btn_RemoteTraining.Click += Btn_RemoteTraining_Click;
			break;
		case 30:
			lab_Training1 = (Label)target;
			break;
		case 31:
			lab_Training2 = (Label)target;
			break;
		case 32:
			lab_Training3 = (Label)target;
			break;
		case 33:
			lab_Training4 = (Label)target;
			break;
		case 34:
			lab_Training5 = (Label)target;
			break;
		case 35:
			lab_Training6 = (Label)target;
			break;
		case 36:
			lab_Training7 = (Label)target;
			break;
		case 37:
			lab_Training8 = (Label)target;
			break;
		case 38:
			btn_LocalAndRemoteTraining = (Button)target;
			btn_LocalAndRemoteTraining.Click += btn_LocalAndRemoteTraining_Click;
			break;
		case 39:
			lab_RemoteStatus = (Label)target;
			break;
		case 40:
			btn_SelectALL = (Button)target;
			btn_SelectALL.Click += Btn_SelectALL_Click;
			break;
		case 41:
			btn_UnSelectALL = (Button)target;
			btn_UnSelectALL.Click += Btn_UnSelectALL_Click;
			break;
		case 42:
			btn_TrainCount1 = (Button)target;
			btn_TrainCount1.Click += Btn_TrainCount1_Click;
			break;
		case 43:
			btn_TrainCount2 = (Button)target;
			btn_TrainCount2.Click += Btn_TrainCount1_Click;
			break;
		case 44:
			btn_TrainCount3 = (Button)target;
			btn_TrainCount3.Click += Btn_TrainCount1_Click;
			break;
		case 45:
			btn_TrainCount4 = (Button)target;
			btn_TrainCount4.Click += Btn_TrainCount1_Click;
			break;
		case 46:
			btn_TrainCount5 = (Button)target;
			btn_TrainCount5.Click += Btn_TrainCount1_Click;
			break;
		case 47:
			btn_TrainCount6 = (Button)target;
			btn_TrainCount6.Click += Btn_TrainCount1_Click;
			break;
		case 48:
			lab_SelectStation = (Label)target;
			break;
		case 49:
			grd_S2 = (Grid)target;
			break;
		case 50:
			lab_OKTrainingS2 = (Label)target;
			break;
		case 51:
			lab_S4 = (Label)target;
			break;
		case 52:
			lab_NGTrainingS2 = (Label)target;
			break;
		case 53:
			lab_ModelS2 = (Label)target;
			break;
		case 54:
			grd_CheckS2 = (Grid)target;
			break;
		case 55:
			chk_TrainingS2 = (CheckBox)target;
			chk_TrainingS2.Click += Chk_TrainingS1_Click;
			break;
		case 56:
			lab_TrainingCountS2 = (Label)target;
			break;
		case 57:
			btn_OpenOKS2 = (Button)target;
			btn_OpenOKS2.Click += Btn_OpenOKS1_Click;
			break;
		case 58:
			txt_OKNumS2 = (TextBox)target;
			break;
		case 59:
			txt_NGNumS2 = (TextBox)target;
			break;
		case 60:
			txt_TrainingCountS2 = (TextBox)target;
			txt_TrainingCountS2.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 61:
			btn_OpenNGS2 = (Button)target;
			btn_OpenNGS2.Click += Btn_OpenNGS1_Click;
			break;
		case 62:
			lab_DNet_S2 = (Label)target;
			break;
		case 63:
			btn_SetAIParam_2 = (Button)target;
			btn_SetAIParam_2.Click += Btn_SetAIParam_1_Click;
			break;
		case 64:
			grd_S3 = (Grid)target;
			break;
		case 65:
			lab_OKTrainingS3 = (Label)target;
			break;
		case 66:
			lab_S5 = (Label)target;
			break;
		case 67:
			lab_NGTrainingS3 = (Label)target;
			break;
		case 68:
			lab_ModelS3 = (Label)target;
			break;
		case 69:
			grd_CheckS3 = (Grid)target;
			break;
		case 70:
			chk_TrainingS3 = (CheckBox)target;
			chk_TrainingS3.Click += Chk_TrainingS1_Click;
			break;
		case 71:
			lab_TrainingCountS3 = (Label)target;
			break;
		case 72:
			btn_OpenOKS3 = (Button)target;
			btn_OpenOKS3.Click += Btn_OpenOKS1_Click;
			break;
		case 73:
			txt_OKNumS3 = (TextBox)target;
			break;
		case 74:
			txt_NGNumS3 = (TextBox)target;
			break;
		case 75:
			txt_TrainingCountS3 = (TextBox)target;
			txt_TrainingCountS3.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 76:
			btn_OpenNGS3 = (Button)target;
			btn_OpenNGS3.Click += Btn_OpenNGS1_Click;
			break;
		case 77:
			lab_DNet_S3 = (Label)target;
			break;
		case 78:
			btn_SetAIParam_3 = (Button)target;
			btn_SetAIParam_3.Click += Btn_SetAIParam_1_Click;
			break;
		case 79:
			grd_S4 = (Grid)target;
			break;
		case 80:
			lab_OKTrainingS4 = (Label)target;
			break;
		case 81:
			lab_S6 = (Label)target;
			break;
		case 82:
			lab_NGTrainingS4 = (Label)target;
			break;
		case 83:
			lab_ModelS4 = (Label)target;
			break;
		case 84:
			grd_CheckS4 = (Grid)target;
			break;
		case 85:
			chk_TrainingS4 = (CheckBox)target;
			chk_TrainingS4.Click += Chk_TrainingS1_Click;
			break;
		case 86:
			lab_TrainingCountS4 = (Label)target;
			break;
		case 87:
			btn_OpenOKS4 = (Button)target;
			btn_OpenOKS4.Click += Btn_OpenOKS1_Click;
			break;
		case 88:
			txt_OKNumS4 = (TextBox)target;
			break;
		case 89:
			txt_NGNumS4 = (TextBox)target;
			break;
		case 90:
			txt_TrainingCountS4 = (TextBox)target;
			txt_TrainingCountS4.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 91:
			btn_OpenNGS4 = (Button)target;
			btn_OpenNGS4.Click += Btn_OpenNGS1_Click;
			break;
		case 92:
			lab_DNet_S4 = (Label)target;
			break;
		case 93:
			btn_SetAIParam_4 = (Button)target;
			btn_SetAIParam_4.Click += Btn_SetAIParam_1_Click;
			break;
		case 94:
			grd_S5 = (Grid)target;
			break;
		case 95:
			lab_OKTrainingS5 = (Label)target;
			break;
		case 96:
			lab_S7 = (Label)target;
			break;
		case 97:
			lab_NGTrainingS5 = (Label)target;
			break;
		case 98:
			lab_ModelS5 = (Label)target;
			break;
		case 99:
			grd_CheckS5 = (Grid)target;
			break;
		case 100:
			chk_TrainingS5 = (CheckBox)target;
			chk_TrainingS5.Click += Chk_TrainingS1_Click;
			break;
		case 101:
			lab_TrainingCountS5 = (Label)target;
			break;
		case 102:
			txt_OKNumS5 = (TextBox)target;
			break;
		case 103:
			btn_OpenOKS5 = (Button)target;
			btn_OpenOKS5.Click += Btn_OpenOKS1_Click;
			break;
		case 104:
			txt_NGNumS5 = (TextBox)target;
			break;
		case 105:
			txt_TrainingCountS5 = (TextBox)target;
			txt_TrainingCountS5.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 106:
			btn_OpenNGS5 = (Button)target;
			btn_OpenNGS5.Click += Btn_OpenNGS1_Click;
			break;
		case 107:
			lab_DNet_S5 = (Label)target;
			break;
		case 108:
			btn_SetAIParam_5 = (Button)target;
			btn_SetAIParam_5.Click += Btn_SetAIParam_1_Click;
			break;
		case 109:
			grd_S6 = (Grid)target;
			break;
		case 110:
			lab_OKTrainingS6 = (Label)target;
			break;
		case 111:
			lab_S8 = (Label)target;
			break;
		case 112:
			lab_NGTrainingS6 = (Label)target;
			break;
		case 113:
			lab_ModelS6 = (Label)target;
			break;
		case 114:
			grd_CheckS6 = (Grid)target;
			break;
		case 115:
			chk_TrainingS6 = (CheckBox)target;
			chk_TrainingS6.Click += Chk_TrainingS1_Click;
			break;
		case 116:
			lab_TrainingCountS6 = (Label)target;
			break;
		case 117:
			txt_OKNumS6 = (TextBox)target;
			break;
		case 118:
			btn_OpenOKS6 = (Button)target;
			btn_OpenOKS6.Click += Btn_OpenOKS1_Click;
			break;
		case 119:
			txt_NGNumS6 = (TextBox)target;
			break;
		case 120:
			txt_TrainingCountS6 = (TextBox)target;
			txt_TrainingCountS6.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 121:
			btn_OpenNGS6 = (Button)target;
			btn_OpenNGS6.Click += Btn_OpenNGS1_Click;
			break;
		case 122:
			lab_DNet_S6 = (Label)target;
			break;
		case 123:
			btn_SetAIParam_6 = (Button)target;
			btn_SetAIParam_6.Click += Btn_SetAIParam_1_Click;
			break;
		case 124:
			grd_S7 = (Grid)target;
			break;
		case 125:
			lab_OKTrainingS7 = (Label)target;
			break;
		case 126:
			lab_S9 = (Label)target;
			break;
		case 127:
			lab_NGTrainingS7 = (Label)target;
			break;
		case 128:
			lab_ModelS7 = (Label)target;
			break;
		case 129:
			grd_CheckS7 = (Grid)target;
			break;
		case 130:
			chk_TrainingS7 = (CheckBox)target;
			chk_TrainingS7.Click += Chk_TrainingS1_Click;
			break;
		case 131:
			lab_TrainingCountS7 = (Label)target;
			break;
		case 132:
			txt_OKNumS7 = (TextBox)target;
			break;
		case 133:
			btn_OpenOKS7 = (Button)target;
			btn_OpenOKS7.Click += Btn_OpenOKS1_Click;
			break;
		case 134:
			txt_NGNumS7 = (TextBox)target;
			break;
		case 135:
			txt_TrainingCountS7 = (TextBox)target;
			txt_TrainingCountS7.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 136:
			btn_OpenNGS7 = (Button)target;
			btn_OpenNGS7.Click += Btn_OpenNGS1_Click;
			break;
		case 137:
			lab_DNet_S7 = (Label)target;
			break;
		case 138:
			btn_SetAIParam_7 = (Button)target;
			btn_SetAIParam_7.Click += Btn_SetAIParam_1_Click;
			break;
		case 139:
			grd_S8 = (Grid)target;
			break;
		case 140:
			lab_OKTrainingS8 = (Label)target;
			break;
		case 141:
			lab_S10 = (Label)target;
			break;
		case 142:
			lab_NGTrainingS8 = (Label)target;
			break;
		case 143:
			lab_ModelS8 = (Label)target;
			break;
		case 144:
			grd_CheckS8 = (Grid)target;
			break;
		case 145:
			chk_TrainingS8 = (CheckBox)target;
			chk_TrainingS8.Click += Chk_TrainingS1_Click;
			break;
		case 146:
			lab_TrainingCountS8 = (Label)target;
			break;
		case 147:
			txt_OKNumS8 = (TextBox)target;
			break;
		case 148:
			btn_OpenOKS8 = (Button)target;
			btn_OpenOKS8.Click += Btn_OpenOKS1_Click;
			break;
		case 149:
			txt_NGNumS8 = (TextBox)target;
			break;
		case 150:
			txt_TrainingCountS8 = (TextBox)target;
			txt_TrainingCountS8.PreviewMouseLeftButtonDown += Txt_TrainingCountS1_PreviewMouseLeftButtonDown;
			break;
		case 151:
			btn_OpenNGS8 = (Button)target;
			btn_OpenNGS8.Click += Btn_OpenNGS1_Click;
			break;
		case 152:
			lab_DNet_S8 = (Label)target;
			break;
		case 153:
			btn_SetAIParam_8 = (Button)target;
			btn_SetAIParam_8.Click += Btn_SetAIParam_1_Click;
			break;
		case 154:
			grd_Loading = (Grid)target;
			break;
		case 155:
			grd_Result1 = (Grid)target;
			break;
		case 156:
			pb_Progress1 = (ProgressBar)target;
			break;
		case 157:
			lab_Progress1 = (Label)target;
			break;
		case 158:
			lab_acc1 = (Label)target;
			lab_acc1.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 159:
			lab_Yield1 = (Label)target;
			break;
		case 160:
			lab_NewYield1 = (Label)target;
			break;
		case 161:
			btn_Update1 = (Button)target;
			btn_Update1.Click += Btn_Update1_Click;
			break;
		case 162:
			pb_TestProgress1 = (ProgressBar)target;
			break;
		case 163:
			lab_TestProgress1 = (Label)target;
			break;
		case 164:
			btn_Preview1 = (Button)target;
			btn_Preview1.Click += Btn_Preview1_Click;
			break;
		case 165:
			lab_Time1 = (Label)target;
			break;
		case 166:
			lab_TotalTime1 = (Label)target;
			break;
		case 167:
			lab_RS1 = (Label)target;
			lab_RS1.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 168:
			grd_Result2 = (Grid)target;
			break;
		case 169:
			lab_acc2 = (Label)target;
			lab_acc2.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 170:
			lab_Yield2 = (Label)target;
			break;
		case 171:
			lab_NewYield2 = (Label)target;
			break;
		case 172:
			btn_Update2 = (Button)target;
			btn_Update2.Click += Btn_Update1_Click;
			break;
		case 173:
			pb_Progress2 = (ProgressBar)target;
			break;
		case 174:
			lab_Progress2 = (Label)target;
			break;
		case 175:
			pb_TestProgress2 = (ProgressBar)target;
			break;
		case 176:
			lab_TestProgress2 = (Label)target;
			break;
		case 177:
			btn_Preview2 = (Button)target;
			btn_Preview2.Click += Btn_Preview1_Click;
			break;
		case 178:
			lab_Time2 = (Label)target;
			break;
		case 179:
			lab_TotalTime2 = (Label)target;
			break;
		case 180:
			lab_RS2 = (Label)target;
			lab_RS2.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 181:
			grd_Result3 = (Grid)target;
			break;
		case 182:
			lab_acc3 = (Label)target;
			lab_acc3.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 183:
			lab_Yield3 = (Label)target;
			break;
		case 184:
			lab_NewYield3 = (Label)target;
			break;
		case 185:
			btn_Update3 = (Button)target;
			btn_Update3.Click += Btn_Update1_Click;
			break;
		case 186:
			pb_Progress3 = (ProgressBar)target;
			break;
		case 187:
			lab_Progress3 = (Label)target;
			break;
		case 188:
			pb_TestProgress3 = (ProgressBar)target;
			break;
		case 189:
			lab_TestProgress3 = (Label)target;
			break;
		case 190:
			btn_Preview3 = (Button)target;
			btn_Preview3.Click += Btn_Preview1_Click;
			break;
		case 191:
			lab_Time3 = (Label)target;
			break;
		case 192:
			lab_TotalTime3 = (Label)target;
			break;
		case 193:
			lab_RS3 = (Label)target;
			lab_RS3.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 194:
			grd_Result4 = (Grid)target;
			break;
		case 195:
			lab_acc4 = (Label)target;
			lab_acc4.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 196:
			lab_Yield4 = (Label)target;
			break;
		case 197:
			lab_NewYield4 = (Label)target;
			break;
		case 198:
			btn_Update4 = (Button)target;
			btn_Update4.Click += Btn_Update1_Click;
			break;
		case 199:
			pb_Progress4 = (ProgressBar)target;
			break;
		case 200:
			lab_Progress4 = (Label)target;
			break;
		case 201:
			pb_TestProgress4 = (ProgressBar)target;
			break;
		case 202:
			lab_TestProgress4 = (Label)target;
			break;
		case 203:
			btn_Preview4 = (Button)target;
			btn_Preview4.Click += Btn_Preview1_Click;
			break;
		case 204:
			lab_Time4 = (Label)target;
			break;
		case 205:
			lab_TotalTime4 = (Label)target;
			break;
		case 206:
			lab_RemoteTraining = (Label)target;
			lab_RemoteTraining.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 207:
			lab_RS4 = (Label)target;
			lab_RS4.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 208:
			grd_Result5 = (Grid)target;
			break;
		case 209:
			lab_acc5 = (Label)target;
			lab_acc5.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 210:
			lab_Yield5 = (Label)target;
			break;
		case 211:
			lab_NewYield5 = (Label)target;
			break;
		case 212:
			btn_Update5 = (Button)target;
			btn_Update5.Click += Btn_Update1_Click;
			break;
		case 213:
			pb_Progress5 = (ProgressBar)target;
			break;
		case 214:
			lab_Progress5 = (Label)target;
			break;
		case 215:
			pb_TestProgress5 = (ProgressBar)target;
			break;
		case 216:
			lab_TestProgress5 = (Label)target;
			break;
		case 217:
			btn_Preview5 = (Button)target;
			btn_Preview5.Click += Btn_Preview1_Click;
			break;
		case 218:
			lab_Time5 = (Label)target;
			break;
		case 219:
			lab_TotalTime5 = (Label)target;
			break;
		case 220:
			lab_RS5 = (Label)target;
			lab_RS5.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 221:
			grd_Result6 = (Grid)target;
			break;
		case 222:
			lab_acc6 = (Label)target;
			lab_acc6.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 223:
			lab_Yield6 = (Label)target;
			break;
		case 224:
			lab_NewYield6 = (Label)target;
			break;
		case 225:
			btn_Update6 = (Button)target;
			btn_Update6.Click += Btn_Update1_Click;
			break;
		case 226:
			pb_Progress6 = (ProgressBar)target;
			break;
		case 227:
			lab_Progress6 = (Label)target;
			break;
		case 228:
			pb_TestProgress6 = (ProgressBar)target;
			break;
		case 229:
			lab_TestProgress6 = (Label)target;
			break;
		case 230:
			btn_Preview6 = (Button)target;
			btn_Preview6.Click += Btn_Preview1_Click;
			break;
		case 231:
			lab_Time6 = (Label)target;
			break;
		case 232:
			lab_TotalTime6 = (Label)target;
			break;
		case 233:
			lab_RS6 = (Label)target;
			lab_RS6.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 234:
			grd_Result7 = (Grid)target;
			break;
		case 235:
			lab_acc7 = (Label)target;
			lab_acc7.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 236:
			lab_Yield7 = (Label)target;
			break;
		case 237:
			lab_NewYield7 = (Label)target;
			break;
		case 238:
			btn_Update7 = (Button)target;
			btn_Update7.Click += Btn_Update1_Click;
			break;
		case 239:
			pb_Progress7 = (ProgressBar)target;
			break;
		case 240:
			lab_Progress7 = (Label)target;
			break;
		case 241:
			pb_TestProgress7 = (ProgressBar)target;
			break;
		case 242:
			lab_TestProgress7 = (Label)target;
			break;
		case 243:
			btn_Preview7 = (Button)target;
			btn_Preview7.Click += Btn_Preview1_Click;
			break;
		case 244:
			lab_Time7 = (Label)target;
			break;
		case 245:
			lab_TotalTime7 = (Label)target;
			break;
		case 246:
			lab_RS7 = (Label)target;
			lab_RS7.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 247:
			grd_Result8 = (Grid)target;
			break;
		case 248:
			lab_acc8 = (Label)target;
			lab_acc8.PreviewMouseLeftButtonDown += Lab_acc1_PreviewMouseLeftButtonDown;
			break;
		case 249:
			lab_Yield8 = (Label)target;
			break;
		case 250:
			lab_NewYield8 = (Label)target;
			break;
		case 251:
			btn_Update8 = (Button)target;
			btn_Update8.Click += Btn_Update1_Click;
			break;
		case 252:
			pb_Progress8 = (ProgressBar)target;
			break;
		case 253:
			lab_Progress8 = (Label)target;
			break;
		case 254:
			pb_TestProgress8 = (ProgressBar)target;
			break;
		case 255:
			lab_TestProgress8 = (Label)target;
			break;
		case 256:
			btn_Preview8 = (Button)target;
			btn_Preview8.Click += Btn_Preview1_Click;
			break;
		case 257:
			lab_Time8 = (Label)target;
			break;
		case 258:
			lab_TotalTime8 = (Label)target;
			break;
		case 259:
			lab_RS8 = (Label)target;
			lab_RS8.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 260:
			btn_UpdateAll = (Button)target;
			btn_UpdateAll.Click += Btn_UpdateAll_Click;
			break;
		case 261:
			btn_Close = (Button)target;
			btn_Close.Click += Btn_Close_Click;
			break;
		case 262:
			txt_InspResult = (TextBox)target;
			break;
		case 263:
			txt_CmdResult = (TextBox)target;
			break;
		case 264:
			btn_Interrupt_S1 = (Button)target;
			btn_Interrupt_S1.Click += Btn_Interrupt_S1_Click;
			break;
		case 265:
			lab_Message = (Label)target;
			break;
		case 266:
			lab_Remote1 = (Label)target;
			lab_Remote1.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 267:
			lab_Remote2 = (Label)target;
			lab_Remote2.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 268:
			lab_Remote3 = (Label)target;
			lab_Remote3.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 269:
			lab_Remote4 = (Label)target;
			lab_Remote4.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 270:
			lab_Remote5 = (Label)target;
			lab_Remote5.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 271:
			lab_Remote6 = (Label)target;
			lab_Remote6.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 272:
			lab_Remote7 = (Label)target;
			lab_Remote7.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 273:
			lab_Remote8 = (Label)target;
			lab_Remote8.PreviewMouseLeftButtonDown += Lab_RS1_PreviewMouseLeftButtonDown;
			break;
		case 274:
			lab_RemoteMode = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
