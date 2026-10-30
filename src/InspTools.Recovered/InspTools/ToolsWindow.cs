using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using InspTools.AOI;
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using OnnxRuntimeEnv;
using OpenCvSharp;
using SAMViewer;

namespace InspTools;

public class ToolsWindow : System.Windows.Window, IComponentConnector
{
	public delegate int GetAIResultDelegate(int station, Mat AiMat, ref Mat heatMap);

	public delegate void SetSaveFinishResultDelegate();

	public const int MaxBin = 6;

	private int StationIndex;

	public CVInspection[] Station = new CVInspection[8];

	public string[] OKPath = new string[8];

	public string[] NGPath = new string[8];

	private bool inited;

	private System.Windows.Controls.Button[] StationBtn;

	private System.Windows.Controls.Button[] ChannelBtn;

	private bool UpdaeUI;

	private bool AIDebug;

	private string filenameID = "";

	private System.Windows.Controls.Button[] ToolsBtn;

	private System.Windows.Shapes.Rectangle[] ResultLine;

	private Ellipse[] ErrorEs;

	private System.Windows.Controls.Label[] ErrorLabel;

	private bool ShowMessage;

	private int ImageChannnel;

	private System.Windows.Controls.Label[] TimeLabel;

	private bool ReferenceShow = true;

	private int LanguageType = 1;

	private ResourceDictionary resourceDictionary;

	private bool SaveAIImage;

	private int ParetoNGSelect;

	private bool EnableModifyLog = true;

	private Mat OldDst = new Mat();

	private bool AIMatReDrawing;

	private string RemoteTrainingFile = "";

	private string RemoteTrainingFinishFile = "";

	private string ProcPath = "";

	private int LastV;

	private const int MaxImageChannel = 22;

	private const int MAXInspTool = 18;

	private bool isDragging;

	private System.Windows.Point startPosition;

	private bool isDragging2;

	private System.Windows.Point startPosition2;

	private int ImageListSource = 2;

	private int[] RecentBufferIndex = new int[8];

	private int[] OKBufferIndex = new int[8];

	private int[] NGBufferIndex = new int[8];

	private int[] RetestBufferIndex = new int[8];

	private string[][] filelist = new string[8][];

	private int fileIndex;

	private FolderBrowserDialog folderPath = new FolderBrowserDialog();

	private Mat DrawMat;

	private bool TestPressed;

	private string RuncountText;

	private double TestProgress;

	private double[] xRatio = new double[8];

	private double[] yRatio = new double[8];

	private int[] ImageWidth = new int[8];

	private int[] ImageHeight = new int[8];

	private int[] ImageBand = new int[8];

	private int[] BufferImageWidth = new int[8];

	private int[] BufferImageHeight = new int[8];

	private int[] BufferImageBand = new int[8];

	private Stopwatch sw = new Stopwatch();

	private int OKCount;

	private int NGCount;

	private double OKRatio;

	private double NGRatio;

	private double Param1Ratio;

	private double Param2Ratio;

	private double Param3Ratio;

	private int timer_state;

	private HiniFile ThroughputINI;

	private HiniFile MainINI;

	public ThroughputParamFile ThroughputFile = new ThroughputParamFile();

	public List<OBJSIZE> OBJSizeStatistics = new List<OBJSIZE>();

	public List<ParetoSatisicsItem> ParetoChartFile;

	private bool EnableMultiParetoMode;

	private string RecipeName = "";

	private string OldRecipeName = "X-OldRecipeName";

	private string UserID = "123456";

	private List<TOOLSENABLEITEM> AllToolEnableItem = new List<TOOLSENABLEITEM>();

	private User_Permissions User = User_Permissions.USER_SuperUser;

	private int[,] OldBinCount = new int[8, 6];

	private int[] OldTotalBinCount = new int[6];

	private string FontTrainDir = "";

	private bool LowLevelVersion;

	private bool GPUExist = true;

	private bool EnhanceTestExtend;

	private int AuthLevel;

	private char AuthFunStr = 'A';

	private string Authdatestr = "";

	private bool InspToolAuth;

	private readonly ResourceManager _resourceManager;

	private bool OKStop;

	private bool NGStop;

	private int ImageAddr;

	private Thread AutoTestThread;

	private ManualResetEvent AutoTestPauseEvent = new ManualResetEvent(initialState: false);

	private ManualResetEvent AutoTestShutdownEvent = new ManualResetEvent(initialState: false);

	private Thread AutoTestUIThread;

	private ManualResetEvent AutoTestUIPauseEvent = new ManualResetEvent(initialState: false);

	private ManualResetEvent AutoTestUIShutdownEvent = new ManualResetEvent(initialState: false);

	private string ThreadStateStr;

	private Thread AILoadingThread;

	private Thread AIAutoDrawThread;

	private ManualResetEvent AILoadingPauseEvent = new ManualResetEvent(initialState: false);

	private ManualResetEvent AIAutoDrawPauseEvent = new ManualResetEvent(initialState: false);

	private AI_MODE UsePredictEnhance;

	private bool ForceClose;

	private Thread RecipeSaveThread;

	private ManualResetEvent RecipeSavePauseEvent = new ManualResetEvent(initialState: false);

	private ManualResetEvent RecipeSaveFinishEvent = new ManualResetEvent(initialState: false);

	private bool RecipSaving;

	private bool RecipLoading;

	private bool RemoteMode;

	private int ThreadID;

	private DispatcherTimer UpdateUITImer = new DispatcherTimer();

	private SAM mSam = SAM.Instance();

	private bool SAMModelLoad;

	public GetAIResultDelegate getAIResult;

	public SetSaveFinishResultDelegate setSaveFinishResult;

	private string[] CurrentAIDBPath = new string[8];

	private string LoadingAIResult = "";

	public bool[] LoadingAIModel = new bool[8];

	private bool AILoginSuccess;

	private const int MaxPublicPattern = 210;

	private const int MaxPublicMatchParam = 50;

	private MatchPattern[] PublicPatternDB = new MatchPattern[210];

	private MatchPattern[] PrivatePatternDB = new MatchPattern[210];

	private INSP_MATCHTOOL_PARAM[] MatchQuickSetupParam = new INSP_MATCHTOOL_PARAM[50];

	private HiniFile DBIni;

	private HiniFile PrivateDBIni;

	internal ToolsWindow Main;

	internal Grid grd_Main;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal System.Windows.Controls.Label Label_Result;

	internal System.Windows.Controls.Button btn_Result_Flip;

	internal System.Windows.Controls.Label lab_Channel;

	internal System.Windows.Controls.Label lab_Message;

	internal Grid grd_TestEnhance;

	internal System.Windows.Controls.CheckBox chk_EnableVMirror;

	internal System.Windows.Controls.CheckBox chk_EnableHMirror;

	internal MahApps.Metro.Controls.NumericUpDown nu_Hue;

	internal MahApps.Metro.Controls.NumericUpDown nu_Sat;

	internal MahApps.Metro.Controls.NumericUpDown nu_Val;

	internal MahApps.Metro.Controls.NumericUpDown nu_Red;

	internal MahApps.Metro.Controls.NumericUpDown nu_Green;

	internal MahApps.Metro.Controls.NumericUpDown nu_Blue;

	internal System.Windows.Controls.Button btn_TestEnhanceReset;

	internal Grid grd_ImageSourceSelect;

	internal System.Windows.Controls.Button btn_ImageNum;

	internal System.Windows.Controls.Button btn_PrevImage;

	internal System.Windows.Controls.Button btn_TestAllImage;

	internal System.Windows.Controls.Button btn_NextImage;

	internal System.Windows.Controls.CheckBox chk_OKStop;

	internal System.Windows.Controls.CheckBox chk_NGStop;

	internal System.Windows.Controls.Button btn_UseRecentBufList;

	internal System.Windows.Controls.Button btn_UseOKBufList;

	internal System.Windows.Controls.Button btn_UseNGBufList;

	internal System.Windows.Controls.Button btn_UseFileBufList;

	internal System.Windows.Controls.Button btn_UseOpenBitmap;

	internal System.Windows.Controls.Button btn_BitmapSave;

	internal System.Windows.Controls.Button btn_ReferenceShow;

	internal System.Windows.Controls.Button btn_UseRetestBufList;

	internal System.Windows.Controls.Button btn_SetAll;

	internal System.Windows.Controls.Button btn_NGDB;

	internal System.Windows.Controls.Button btn_OKDB;

	internal System.Windows.Controls.Button btn_RemoveFromDB;

	internal System.Windows.Controls.Button btn_AddToDB;

	internal System.Windows.Controls.Button btn_AIBitmapSave;

	internal System.Windows.Controls.Button btn_ImgT1;

	internal System.Windows.Controls.Button btn_ImgT2;

	internal System.Windows.Controls.Button btn_ImgT3;

	internal System.Windows.Controls.Button btn_AINGDB;

	internal System.Windows.Controls.Button btn_AIOKDB;

	internal System.Windows.Controls.Button btn_YieldRetest;

	internal System.Windows.Controls.TextBox txt_OKCount;

	internal System.Windows.Controls.TextBox txt_OKCountRatio;

	internal System.Windows.Controls.TextBox txt_NGCount;

	internal System.Windows.Controls.TextBox txt_NGCountRatio;

	internal System.Windows.Controls.Label lab_AutestState;

	internal System.Windows.Controls.Label lab_FileName;

	internal System.Windows.Controls.ProgressBar pg_TestProgress;

	internal System.Windows.Controls.Button btn_DelAIOKDB;

	internal Grid grd_AINGQuickDraw;

	internal System.Windows.Controls.Label lab_QuickAIDraw;

	internal System.Windows.Controls.Button btn_DrawTempMask;

	internal System.Windows.Controls.Button btn_AddToNGTemp;

	internal System.Windows.Controls.Label lab_AINGTempCount;

	internal System.Windows.Controls.CheckBox chk_TestEnhanceEnabled;

	internal System.Windows.Controls.Button btn_ImgAILR;

	internal System.Windows.Controls.Button btn_ImgAILG;

	internal System.Windows.Controls.Button btn_ImgAILB;

	internal Grid grd_Channel;

	internal System.Windows.Controls.Button btn_ImgSrc;

	internal System.Windows.Controls.Button btn_ImgR;

	internal System.Windows.Controls.Button btn_ImgG;

	internal System.Windows.Controls.Button btn_ImgB;

	internal System.Windows.Controls.Button btn_ImgH;

	internal System.Windows.Controls.Button btn_ImgS;

	internal System.Windows.Controls.Button btn_ImgV;

	internal System.Windows.Controls.Button btn_ImgGray;

	internal System.Windows.Controls.Button btn_ImgC1;

	internal System.Windows.Controls.Button btn_ImgC2;

	internal System.Windows.Controls.Button btn_ImgC3;

	internal System.Windows.Controls.Button btn_ImgAI;

	internal System.Windows.Controls.Button btn_ImgAIR;

	internal System.Windows.Controls.Button btn_ImgAIG;

	internal System.Windows.Controls.Button btn_ImgAIB;

	internal System.Windows.Controls.Button btn_ImgC4;

	internal System.Windows.Controls.Button btn_ImgC5;

	internal System.Windows.Controls.Button btn_ImgC6;

	internal System.Windows.Controls.Button btn_ChannelExpend;

	internal System.Windows.Controls.Button btn_ImgAI2;

	internal System.Windows.Controls.GroupBox gb_TestResult;

	internal System.Windows.Controls.TextBox txt_InspResult;

	internal System.Windows.Shapes.Rectangle rt_result01;

	internal System.Windows.Shapes.Rectangle rt_result02;

	internal System.Windows.Shapes.Rectangle rt_result03;

	internal System.Windows.Shapes.Rectangle rt_result04;

	internal System.Windows.Shapes.Rectangle rt_result05;

	internal System.Windows.Shapes.Rectangle rt_result06;

	internal System.Windows.Shapes.Rectangle rt_result07;

	internal System.Windows.Shapes.Rectangle rt_result01_W;

	internal System.Windows.Shapes.Rectangle rt_result02_W;

	internal System.Windows.Shapes.Rectangle rt_result03_W;

	internal System.Windows.Shapes.Rectangle rt_result04_W;

	internal System.Windows.Shapes.Rectangle rt_result05_W;

	internal System.Windows.Shapes.Rectangle rt_result06_W;

	internal System.Windows.Shapes.Rectangle rt_result07_W;

	internal System.Windows.Controls.CheckBox chk_LocateToolON;

	internal System.Windows.Controls.CheckBox chk_SelectWayON;

	internal System.Windows.Controls.CheckBox chk_EnhanceToolON;

	internal System.Windows.Controls.CheckBox chk_AdvLocateToolON;

	internal System.Windows.Controls.CheckBox chk_MeasureToolON;

	internal Ellipse ep_Result;

	internal System.Windows.Controls.Label lab_TestResult;

	internal System.Windows.Controls.Label lab_StatinoIndex;

	internal System.Windows.Controls.Button btn_LocateTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy;

	internal System.Windows.Controls.Button btn_SelectWayTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy1;

	internal System.Windows.Controls.Button btn_SelectFaceTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy6;

	internal System.Windows.Controls.Button btn_EnhanceTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy2;

	internal System.Windows.Controls.Button btn_AIProphet;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy17;

	internal System.Windows.Controls.Button btn_AIEnhanceTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy15;

	internal System.Windows.Controls.Button btn_SmoothTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy5;

	internal System.Windows.Controls.Button btn_BinaryTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy4;

	internal System.Windows.Controls.Button btn_MaskTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy3;

	internal System.Windows.Controls.Button btn_MeasureTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy12;

	internal System.Windows.Controls.Button btn_AdvancedLocate;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy7;

	internal System.Windows.Controls.Button btn_BlockAvgTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy8;

	internal System.Windows.Controls.Button btn_DifferenceTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy9;

	internal System.Windows.Controls.Button btn_LineAnalysis;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy11;

	internal System.Windows.Controls.Button btn_MatchTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy10;

	internal System.Windows.Controls.Button btn_AIPredictTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy13;

	internal System.Windows.Controls.CheckBox chk_MaskToolON;

	internal System.Windows.Controls.CheckBox chk_BinaryToolON;

	internal System.Windows.Controls.CheckBox chk_SmoothToolON;

	internal System.Windows.Controls.CheckBox chk_BlockAvgToolON;

	internal System.Windows.Controls.CheckBox chk_DifferenceToolON;

	internal System.Windows.Controls.CheckBox chk_MatchToolON;

	internal System.Windows.Controls.CheckBox chk_LineAnalysisON;

	internal System.Windows.Controls.CheckBox chk_AIPredictToolON;

	internal System.Windows.Controls.Button btn_TestAll;

	internal System.Windows.Controls.Label lab_Time1;

	internal System.Windows.Controls.Label lab_Time2;

	internal System.Windows.Controls.Label lab_Time3;

	internal System.Windows.Controls.Label lab_Time4;

	internal System.Windows.Controls.Label lab_Time5;

	internal System.Windows.Controls.Label lab_Time6;

	internal System.Windows.Controls.Label lab_Time7;

	internal System.Windows.Controls.Label lab_Time8;

	internal System.Windows.Controls.Label lab_TotalTime;

	internal System.Windows.Controls.Label lab_Time9;

	internal System.Windows.Controls.Label lab_Time11;

	internal System.Windows.Controls.Label lab_Time10;

	internal System.Windows.Controls.Label lab_Time12;

	internal System.Windows.Controls.CheckBox chk_SelectFaceON;

	internal System.Windows.Controls.Label lab_Time2_2;

	internal System.Windows.Controls.Label lab_FaceIndex;

	internal Ellipse es_FaceIndex;

	internal System.Windows.Controls.Label lab_FaceIndex2;

	internal Ellipse es_01;

	internal System.Windows.Controls.Label lab_Error01;

	internal Ellipse es_03;

	internal System.Windows.Controls.Label lab_Error03;

	internal Ellipse es_04;

	internal System.Windows.Controls.Label lab_Error04;

	internal Ellipse es_00;

	internal System.Windows.Controls.Label lab_Error00;

	internal Ellipse es_07;

	internal System.Windows.Controls.Label lab_Error07;

	internal Ellipse es_08;

	internal System.Windows.Controls.Label lab_Error08;

	internal Ellipse es_06;

	internal System.Windows.Controls.Label lab_Error06;

	internal Ellipse es_05;

	internal System.Windows.Controls.Label lab_Error05;

	internal Ellipse es_09;

	internal System.Windows.Controls.Label lab_Error09;

	internal Ellipse es_12;

	internal System.Windows.Controls.Label lab_Error12;

	internal Ellipse es_11;

	internal System.Windows.Controls.Label lab_Error11;

	internal Ellipse es_10;

	internal System.Windows.Controls.Label lab_Error10;

	internal System.Windows.Controls.Button btn_ParetoChart;

	internal System.Windows.Controls.Button btn_SizeStatic;

	internal System.Windows.Controls.ComboBox cb_ParetoNGSelect;

	internal System.Windows.Controls.Label lab_ParetoNGSelect;

	internal System.Windows.Controls.CheckBox chk_SaveAI;

	internal System.Windows.Controls.Button btn_IRTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy14;

	internal System.Windows.Controls.Label lab_TimeAIEhance;

	internal System.Windows.Controls.CheckBox chk_AIEnhanceToolON;

	internal System.Windows.Controls.Button btn_CancleSave;

	internal System.Windows.Controls.Button btn_AITrain;

	internal System.Windows.Controls.Label lab_TimeAIPredict;

	internal Ellipse es_AIPredict;

	internal System.Windows.Controls.Label lab_ErrorAIPredict;

	internal System.Windows.Controls.Button btn_AutoMatch;

	internal System.Windows.Controls.Button b1;

	internal System.Windows.Controls.Button b2;

	internal System.Windows.Controls.Button b3;

	internal System.Windows.Controls.CheckBox chk_SaveFontAI;

	internal System.Windows.Controls.Button btn_InspSurv;

	internal System.Windows.Controls.Button btn_test;

	internal Grid grd_AIFunction;

	internal TranslateTransform tt2;

	internal System.Windows.Controls.Button btn_AI_Function;

	internal System.Windows.Controls.Button btn_DrawMask;

	internal System.Windows.Controls.Button btn_AddToNG;

	internal System.Windows.Controls.Button btn_AddToOK;

	internal System.Windows.Controls.Button btn_OpenFolder;

	internal Ellipse es_201;

	internal Ellipse es_202;

	internal Ellipse es_203;

	internal System.Windows.Controls.Label lab_Error201;

	internal System.Windows.Controls.Label lab_Error202;

	internal System.Windows.Controls.Label lab_Error203;

	internal System.Windows.Controls.Button btn_SetupAIFolder;

	internal System.Windows.Controls.Label lab_ModelName;

	internal System.Windows.Controls.Button btn_AIPreview;

	internal System.Windows.Controls.Button btn_GerNG;

	internal System.Windows.Controls.Button btn_AutoMarked;

	internal System.Windows.Controls.TextBox lab_RemoteMode;

	internal System.Windows.Controls.ProgressBar pb_TrainingProcess;

	internal System.Windows.Controls.Label lab_TrainingProcess;

	internal Ellipse es_AITrainFinish;

	internal Expander exp_PreviewAI;

	internal TranslateTransform tt;

	internal Grid grd_AIImage;

	internal Canvas cnv_AIImage;

	internal Grid grd_AILoading;

	internal System.Windows.Controls.Button btn_FontTool;

	internal System.Windows.Controls.Button btn_BitmapSave_Copy16;

	internal System.Windows.Controls.CheckBox chk_FontToolON;

	internal System.Windows.Controls.Label lab_TimeFontTool;

	internal Ellipse es_FontTool;

	internal System.Windows.Controls.Label lab_ErrorFontTool;

	internal System.Windows.Controls.Button btn_AutoFontMatch;

	internal System.Windows.Controls.Label lab_CpuUseOptimized;

	internal System.Windows.Controls.Label lab_EnableSetOpenCVThreadNum;

	internal System.Windows.Controls.Label lab_EnableGPU;

	internal System.Windows.Controls.Label lab_ThreadNum;

	internal System.Windows.Controls.Label lab_Version;

	internal System.Windows.Controls.Label lab_FontToolText;

	internal System.Windows.Controls.Button btn_OpenAIFontFolder;

	internal System.Windows.Controls.CheckBox chk_SaveFontAI2;

	internal System.Windows.Controls.Label lab_AITrainFinish;

	internal System.Windows.Controls.TextBox txt_b1;

	internal System.Windows.Controls.CheckBox chk_IRToolON;

	internal System.Windows.Controls.Label lab_TimeIRTool;

	internal Ellipse es_IRTool;

	internal System.Windows.Controls.Label lab_ErrorIRTool;

	internal System.Windows.Controls.Label lab_SaveAIImage;

	internal System.Windows.Controls.Button btn_LogAlert;

	internal System.Windows.Controls.Label lab_User;

	internal System.Windows.Controls.Label lab_Customer;

	internal System.Windows.Controls.Button btn_ParamCopy;

	internal System.Windows.Controls.Button btn_Stage1;

	internal System.Windows.Controls.Button btn_Stage2;

	internal System.Windows.Controls.Button btn_Stage3;

	internal System.Windows.Controls.Button btn_Stage4;

	internal System.Windows.Controls.Button btn_Stage5;

	internal System.Windows.Controls.Button btn_Stage6;

	internal System.Windows.Controls.Button btn_Stage7;

	internal System.Windows.Controls.Button btn_Stage8;

	internal System.Windows.Controls.Button btn_Stage9;

	internal System.Windows.Controls.Button btn_Stage10;

	internal System.Windows.Controls.CheckBox chk_AIProphetToolON;

	internal System.Windows.Controls.Label lab_TimeProphetTool;

	internal Ellipse es_AIProphet;

	internal System.Windows.Controls.Label lab_ErrorAIProphet;

	internal System.Windows.Controls.Button btn_AddPTrainImage;

	internal System.Windows.Controls.Button btn_AddPTrainNGImage;

	internal Expander exp_PreviewAIHeatMap;

	internal TranslateTransform tt1;

	internal Grid grd_AIHeatMapImage;

	internal Canvas cnv_AIHeatMapImage;

	internal System.Windows.Controls.Label lab_AIProphetScore;

	internal System.Windows.Controls.Button btn_UnLockAuth;

	internal System.Windows.Controls.Button btn_LightMonitorTool;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ToolsWindow()
	{
		inited = false;
		InitializeComponent();
		inited = true;
		lab_AutestState.Content = "";
		DrawMat = new Mat();
		lab_Message.Opacity = 0.0;
		for (int i = 0; i < 8; i++)
		{
			Station[i] = null;
			xRatio[i] = 1.0;
			yRatio[i] = 1.0;
			ImageWidth[i] = 640;
			ImageHeight[i] = 480;
			ImageBand[i] = 3;
			BufferImageWidth[i] = 640;
			BufferImageHeight[i] = 480;
			BufferImageBand[i] = 3;
			RecentBufferIndex[i] = 0;
			OKBufferIndex[i] = 0;
			NGBufferIndex[i] = 0;
			RetestBufferIndex[i] = 0;
			LoadingAIModel[i] = false;
		}
		for (int j = 0; j < 8; j++)
		{
			for (int k = 0; k < 6; k++)
			{
				OldBinCount[j, k] = 0;
				if (j == 0)
				{
					OldTotalBinCount[k] = 0;
				}
			}
		}
		StationIndex = 0;
		btn_Stage1.Background = System.Windows.Media.Brushes.HotPink;
		lab_StatinoIndex.Content = 1.ToString();
		StationBtn = new System.Windows.Controls.Button[8] { btn_Stage1, btn_Stage2, btn_Stage3, btn_Stage4, btn_Stage5, btn_Stage6, btn_Stage7, btn_Stage8 };
		ToolsBtn = new System.Windows.Controls.Button[18]
		{
			btn_LocateTool, btn_SelectWayTool, btn_SelectFaceTool, btn_EnhanceTool, btn_AdvancedLocate, btn_MeasureTool, btn_MaskTool, btn_BinaryTool, btn_SmoothTool, btn_BlockAvgTool,
			btn_DifferenceTool, btn_LineAnalysis, btn_MatchTool, btn_AIPredictTool, btn_AIEnhanceTool, btn_FontTool, btn_IRTool, btn_AIProphet
		};
		ResultLine = new System.Windows.Shapes.Rectangle[7] { rt_result01, rt_result02, rt_result03, rt_result04, rt_result05, rt_result06, rt_result07 };
		TimeLabel = new System.Windows.Controls.Label[18]
		{
			lab_Time1, lab_Time2, lab_Time2_2, lab_Time3, lab_Time4, lab_Time5, lab_Time6, lab_Time7, lab_Time8, lab_Time9,
			lab_Time10, lab_Time11, lab_Time12, lab_TimeAIPredict, lab_TimeAIEhance, lab_TimeFontTool, lab_TimeIRTool, lab_TimeProphetTool
		};
		ErrorEs = new Ellipse[18]
		{
			es_00, es_01, es_FaceIndex, es_03, es_04, es_05, es_06, es_07, es_08, es_09,
			es_10, es_11, es_12, es_AIPredict, es_AITrainFinish, es_FontTool, es_IRTool, es_AIProphet
		};
		ErrorLabel = new System.Windows.Controls.Label[18]
		{
			lab_Error00, lab_Error01, lab_FaceIndex2, lab_Error03, lab_Error04, lab_Error05, lab_Error06, lab_Error07, lab_Error08, lab_Error09,
			lab_Error10, lab_Error11, lab_Error12, lab_ErrorAIPredict, lab_AITrainFinish, lab_ErrorFontTool, lab_ErrorIRTool, lab_ErrorAIProphet
		};
		ChannelBtn = new System.Windows.Controls.Button[22]
		{
			btn_ImgSrc, btn_ImgR, btn_ImgG, btn_ImgB, btn_ImgH, btn_ImgS, btn_ImgV, btn_ImgGray, btn_ImgC1, btn_ImgC2,
			btn_ImgC3, btn_ImgAIR, btn_ImgAIG, btn_ImgAIB, btn_ImgC4, btn_ImgC5, btn_ImgC6, btn_ImgAI, btn_ImgAI2, btn_ImgAILR,
			btn_ImgAILG, btn_ImgAILB
		};
		for (int l = 0; l < 18; l++)
		{
			ErrorEs[l].Visibility = Visibility.Hidden;
			ErrorLabel[l].Visibility = Visibility.Hidden;
		}
		lab_FaceIndex.Visibility = Visibility.Hidden;
		es_FaceIndex.Visibility = Visibility.Hidden;
		lab_FaceIndex2.Visibility = Visibility.Hidden;
		btn_Result_Flip.Visibility = Visibility.Hidden;
		AutoTestThread = new Thread(AutoTestThreadRun);
		AutoTestThread.Start();
		AutoTestUIThread = new Thread(AutoTestUIThreadRun);
		AutoTestUIThread.Start();
		cb_ParetoNGSelect.Visibility = Visibility.Hidden;
		lab_ParetoNGSelect.Visibility = Visibility.Hidden;
		AILoadingThread = new Thread(AILoadingRun);
		AILoadingThread.Start();
		AIAutoDrawThread = new Thread(AIAutoDrawRun);
		AIAutoDrawThread.Start();
		RecipeSaveThread = new Thread(RunRecipeSave);
		RecipeSaveThread.Start();
		RecipeSaveFinishEvent.Set();
		es_201.Visibility = Visibility.Hidden;
		lab_Error201.Visibility = Visibility.Hidden;
		es_202.Visibility = Visibility.Hidden;
		lab_Error202.Visibility = Visibility.Hidden;
		es_203.Visibility = Visibility.Hidden;
		lab_Error203.Visibility = Visibility.Hidden;
		for (int m = 0; m < 210; m++)
		{
			PublicPatternDB[m] = new MatchPattern();
			PrivatePatternDB[m] = new MatchPattern();
		}
		grd_AIFunction.Width = 60.0;
		btn_DelAIOKDB.Visibility = Visibility.Hidden;
		b1.Visibility = Visibility.Hidden;
		b2.Visibility = Visibility.Hidden;
		b3.Visibility = Visibility.Hidden;
		b1.Visibility = Visibility.Hidden;
		b2.Visibility = Visibility.Hidden;
		btn_test.Visibility = Visibility.Hidden;
		txt_b1.Visibility = Visibility.Hidden;
		UpdateUITImer.Interval = TimeSpan.FromMilliseconds(2000.0);
		UpdateUITImer.Tick += UpdateUITImer_Tick;
		RemoteTrainingFile = string.Format("{0}Comm\\training.msg", AppDomain.CurrentDomain.BaseDirectory);
		RemoteTrainingFinishFile = string.Format("{0}Comm\\Finish.msg", AppDomain.CurrentDomain.BaseDirectory);
		ProcPath = string.Format("{0}RemoteTraining\\Proc\\", AppDomain.CurrentDomain.BaseDirectory);
		lab_TrainingProcess.Visibility = Visibility.Hidden;
		pb_TrainingProcess.Visibility = Visibility.Hidden;
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll"));
		lab_Version.Content = versionInfo.FileVersion;
		grd_Channel.Width = 140.0;
		btn_AIBitmapSave.Visibility = Visibility.Hidden;
		btn_LogAlert.Visibility = Visibility.Hidden;
		grd_AINGQuickDraw.Visibility = Visibility.Hidden;
		btn_Stage9.Visibility = Visibility.Hidden;
		btn_Stage10.Visibility = Visibility.Hidden;
		grd_TestEnhance.Visibility = Visibility.Hidden;
		CheckErrorLog();
		if (!File.Exists(string.Format("{0}PModel\\Train\\dist\\AIProphet\\AIProphet.exe", AppDomain.CurrentDomain.BaseDirectory)))
		{
			chk_AIProphetToolON.Visibility = Visibility.Hidden;
			btn_AIProphet.Visibility = Visibility.Hidden;
			btn_AddPTrainImage.Visibility = Visibility.Hidden;
			btn_AddPTrainNGImage.Visibility = Visibility.Hidden;
			lab_TimeProphetTool.Visibility = Visibility.Hidden;
			btn_AutoMarked.Visibility = Visibility.Hidden;
		}
	}

	private void UpdateUITImer_Tick(object sender, EventArgs e)
	{
		if (File.Exists(RemoteTrainingFile) || File.Exists(RemoteTrainingFinishFile))
		{
			if (Directory.Exists(ProcPath))
			{
				lab_TrainingProcess.Visibility = Visibility.Visible;
				pb_TrainingProcess.Visibility = Visibility.Visible;
				if (GetProgress() == 100)
				{
					lab_AITrainFinish.Visibility = Visibility.Visible;
					es_AITrainFinish.Visibility = Visibility.Visible;
				}
			}
		}
		else
		{
			lab_TrainingProcess.Visibility = Visibility.Hidden;
			pb_TrainingProcess.Visibility = Visibility.Hidden;
			lab_AITrainFinish.Visibility = Visibility.Hidden;
			es_AITrainFinish.Visibility = Visibility.Hidden;
			pb_TrainingProcess.Value = 0.0;
			lab_TrainingProcess.Content = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int GetProgress()
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "RemoteTraining\\Proc\\");
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
			if (files.Length == 0)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < files.Length; i++)
			{
				num = Convert.ToInt32(System.IO.Path.GetFileNameWithoutExtension(files[i]));
				if (num > LastV)
				{
					LastV = num;
					if (num < 0)
					{
						num = 0;
					}
					if (num > 100)
					{
						num = 100;
					}
					if (num == 100)
					{
						if (!File.Exists(RemoteTrainingFinishFile))
						{
							num = (LastV = 99);
						}
						else
						{
							lab_AITrainFinish.Visibility = Visibility.Visible;
							es_AITrainFinish.Visibility = Visibility.Visible;
						}
					}
					else
					{
						lab_AITrainFinish.Visibility = Visibility.Hidden;
						es_AITrainFinish.Visibility = Visibility.Hidden;
					}
					pb_TrainingProcess.Value = num;
					lab_TrainingProcess.Content = string.Format("{0} %", num);
				}
				if (num == 100)
				{
					if (!File.Exists(RemoteTrainingFinishFile))
					{
						num = (LastV = 99);
						continue;
					}
					lab_AITrainFinish.Visibility = Visibility.Visible;
					es_AITrainFinish.Visibility = Visibility.Visible;
				}
			}
			return num;
		}
		catch
		{
			return 0;
		}
	}

	private void RunRecipeSave()
	{
		while (true)
		{
			RecipeSavePauseEvent.WaitOne(-1);
			if (!ForceClose)
			{
				RecipSaving = true;
				if (!RemoteMode)
				{
					SaveAllRecipe();
				}
				RecipSaving = false;
				if (setSaveFinishResult != null)
				{
					setSaveFinishResult();
				}
				RecipeSaveFinishEvent.Set();
				RecipeSavePauseEvent.Reset();
				continue;
			}
			break;
		}
	}

	private void AIAutoDrawRun()
	{
		while (true)
		{
			AIAutoDrawPauseEvent.WaitOne(-1);
			if (!ForceClose)
			{
				Action method = delegate
				{
					AutoMarkedUseAIProphetResult();
					grd_AILoading.Visibility = Visibility.Hidden;
				};
				base.Dispatcher.BeginInvoke(method);
				AIAutoDrawPauseEvent.Reset();
				continue;
			}
			break;
		}
	}

	private void AILoadingRun()
	{
		while (true)
		{
			AILoadingPauseEvent.WaitOne(-1);
			if (ForceClose)
			{
				break;
			}
			bool result = LoadingAIModelNow();
			if (!SAMModelLoad)
			{
				mSam.LoadONNXModel(GPUExist);
			}
			Action method = [MethodImpl(MethodImplOptions.NoInlining)] () =>
			{
				if (!result)
				{
					System.Windows.MessageBox.Show(LoadingAIResult, "錯誤");
					txt_InspResult.Text += LoadingAIResult;
				}
				grd_AILoading.Visibility = Visibility.Hidden;
			};
			base.Dispatcher.BeginInvoke(method);
			AILoadingPauseEvent.Reset();
		}
	}

	private void AutoTestThreadRun()
	{
		while (true)
		{
			AutoTestPauseEvent.WaitOne(-1);
			if (!AutoTestShutdownEvent.WaitOne(0))
			{
				RunTestThread();
				continue;
			}
			break;
		}
	}

	private void AutoTestUIThreadRun()
	{
		while (true)
		{
			AutoTestUIPauseEvent.WaitOne(-1);
			if (!AutoTestUIShutdownEvent.WaitOne(0))
			{
				Action method = delegate
				{
					RunCheckThread();
				};
				base.Dispatcher.BeginInvoke(method);
				Thread.Sleep(100);
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RunCheckThread()
	{
		txt_OKCount.Text = OKCount.ToString();
		txt_OKCountRatio.Text = string.Format("{0:P}", OKRatio);
		txt_NGCount.Text = NGCount.ToString();
		txt_NGCountRatio.Text = string.Format("{0:P}", NGRatio);
		btn_ImageNum.Content = RuncountText;
		pg_TestProgress.Value = TestProgress;
		if (timer_state == 0)
		{
			AutoTestUIPauseEvent.Reset();
			ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
			ShowLabMessage(ThreadStateStr);
			base.Cursor = System.Windows.Input.Cursors.Arrow;
			OKCount = (NGCount = 0);
		}
		else if (timer_state == 1)
		{
			base.Cursor = System.Windows.Input.Cursors.Wait;
		}
		else if (timer_state == 2)
		{
			AutoTestUIPauseEvent.Reset();
			ThreadStateStr = LanguageManager.Instance["AutorunPause"];
			base.Cursor = System.Windows.Input.Cursors.Arrow;
			Btn_TestAll_Click(null, null);
		}
		lab_AutestState.Content = ThreadStateStr;
	}

	public void SetThreadID(int threadid)
	{
		ThreadID = threadid;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetStation(CVInspection[] station)
	{
		try
		{
			for (int i = 0; i < 8; i++)
			{
				Station[i] = station[i];
				string name = string.Format("Station{0}", i + 1);
				Station[i].StationTxt = LanguageManager.Instance[name];
				Station[i].SetStationIndex(i);
				station[i].SetThreadID(ThreadID);
			}
			string path = AppDomain.CurrentDomain.BaseDirectory + "Comm\\OnLine.msg";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-SetStation()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetRecipe(string recipename, bool[] StationEnabled = null)
	{
		int num = 0;
		RecipLoading = true;
		try
		{
			RecipeSaveFinishEvent.WaitOne(10000);
			Main.Title = "INSPECTION TOOLS - [" + recipename + "]";
			RecipeName = recipename;
			if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "INIFileParser.dll"))
			{
				System.Windows.MessageBox.Show("找不到INIFileParser.dll，請更新此檔案", "錯誤");
			}
			GetPublicPattern();
			BackupRecipe(recipename, "D:\\RecipeBackup\\");
			string text = "";
			if (StationEnabled != null)
			{
				for (int i = 0; i < 8; i++)
				{
					if (StationEnabled[i])
					{
						Station[i].paramFile.SetPublicPatternDB(PublicPatternDB, MatchQuickSetupParam);
						Station[i].paramFile.SetPrivatePatternDB(PrivatePatternDB);
						Station[i].paramFile.SetThreadID(ThreadID);
						text = "";
						num = i;
						Station[i].SetRecipe(recipename, i, ref text);
						if (!Station[i].GetStationINIStatus())
						{
							StationBtn[i].IsEnabled = false;
							StationBtn[i].Background = System.Windows.Media.Brushes.Gray;
						}
						else
						{
							StationBtn[i].IsEnabled = true;
							StationBtn[i].Background = System.Windows.Media.Brushes.White;
						}
						if (Station[i].ErrorMsg != "")
						{
							System.Windows.MessageBox.Show(Station[i].ErrorMsg, "警告");
							txt_InspResult.Text += Station[i].ErrorMsg;
						}
						if (text != "")
						{
							System.Windows.MessageBox.Show(text, "警告");
							txt_InspResult.Text += text;
						}
						Station[i].paramFile_Backup.SetPublicPatternDB(PublicPatternDB, MatchQuickSetupParam);
						Station[i].paramFile_Backup.SetPrivatePatternDB(PrivatePatternDB);
					}
				}
			}
			else
			{
				for (int j = 0; j < 8; j++)
				{
					Station[j].paramFile.SetPublicPatternDB(PublicPatternDB, MatchQuickSetupParam);
					Station[j].paramFile.SetPrivatePatternDB(PrivatePatternDB);
					Station[j].paramFile.SetThreadID(ThreadID);
					text = "";
					num = j;
					Station[j].SetRecipe(recipename, j, ref text);
					if (!Station[j].GetStationINIStatus())
					{
						StationBtn[j].IsEnabled = false;
						StationBtn[j].Background = System.Windows.Media.Brushes.Gray;
					}
					else
					{
						StationBtn[j].IsEnabled = true;
						StationBtn[j].Background = System.Windows.Media.Brushes.White;
					}
					if (Station[j].ErrorMsg != "")
					{
						System.Windows.MessageBox.Show(Station[j].ErrorMsg, "警告");
						txt_InspResult.Text += Station[j].ErrorMsg;
					}
					if (text != "")
					{
						System.Windows.MessageBox.Show(text, "警告");
						txt_InspResult.Text += text;
					}
					Station[j].paramFile_Backup.SetPublicPatternDB(PublicPatternDB, MatchQuickSetupParam);
					Station[j].paramFile_Backup.SetPrivatePatternDB(PrivatePatternDB);
				}
			}
			bool flag = false;
			string text2 = "";
			for (int k = 0; k < 8; k++)
			{
				if (Station[k].paramFile.AIEnhanceToolON && Station[k].AIDLLExist)
				{
					flag = true;
					if (!Station[k].aiEnhanceTools.GPUExist)
					{
						GPUExist = false;
						text2 = Station[k].aiEnhanceTools.GPUErrorMSG;
					}
				}
			}
			if (flag && text2 != "")
			{
				System.Windows.MessageBox.Show(text2, "警告");
			}
			GetPrivatePattern(RecipeName);
			ReadToolONStatus();
			SetThroughputRecipe(recipename);
			lab_Customer.Content = "C:" + ThroughputFile.CurrentClient;
			if (!Station[0].AIDLLExist)
			{
				btn_AIEnhanceTool.Visibility = Visibility.Hidden;
				chk_AIEnhanceToolON.Visibility = Visibility.Hidden;
				lab_TimeAIEhance.Visibility = Visibility.Hidden;
				btn_AIPredictTool.Visibility = Visibility.Hidden;
				chk_AIPredictToolON.Visibility = Visibility.Hidden;
				lab_TimeAIPredict.Visibility = Visibility.Hidden;
				chk_SaveAI.Visibility = Visibility.Hidden;
				grd_AIFunction.Visibility = Visibility.Hidden;
				btn_AITrain.Visibility = Visibility.Hidden;
			}
			CheckAuthNoDLG();
		}
		catch (Exception ex)
		{
			string text3 = "ToolsWindow-SetRecipe()Station-" + num + " - ";
			text3 += ex.ToString();
			CVBuffer.SaveLog(text3);
			if (text3.Contains("[Main]"))
			{
				System.Windows.MessageBox.Show(string.Format("第{0}站Recipe讀取錯誤。[Main]重複", num + 1), "嚴重錯誤");
			}
			else if (text3.Contains("Duplicate section"))
			{
				System.Windows.MessageBox.Show(string.Format("第{0}站Recipe讀取錯誤。標籤內容重複", num + 1), "嚴重錯誤");
			}
			RecipLoading = false;
			return;
		}
		TimeStart();
		for (int l = 0; l < 8; l++)
		{
			if (RecipeName != OldRecipeName)
			{
				Station[l].aiEnhanceTools.ResetModelRead();
				Station[l].aiProphetTools.ResetModelRead();
				OldRecipeName = RecipeName;
			}
			Station[l].ReadModel(l);
		}
		TimeStop();
		if (!SAMModelLoad)
		{
			AILoadingPauseEvent.Set();
		}
		RecipLoading = false;
	}

	public void SetRemoteMode()
	{
		RemoteMode = true;
		lab_RemoteMode.Visibility = Visibility.Visible;
		grd_Main.Background = System.Windows.Media.Brushes.LightYellow;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetPublicPattern()
	{
		string text = AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = text + "publicDB.ini";
		if (!File.Exists(text2))
		{
			FileStream fileStream = File.Create(text2);
			byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes("[Main]");
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Close();
		}
		if (GetFileEncodeType(text2) != Encoding.Unicode)
		{
			File.WriteAllText(text2, File.ReadAllText(text2, Encoding.Default), Encoding.Unicode);
		}
		DBIni = new HiniFile(text2, ";");
		ReadPublicDBParam();
		ReadMatchQuickSetupParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GetPrivatePattern(string RecipeName)
	{
		string text = string.Format("{0}Recipe\\{1}\\PrivateDB\\", AppDomain.CurrentDomain.BaseDirectory, RecipeName);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = text + "privateDB.ini";
		if (!File.Exists(text2))
		{
			FileStream fileStream = File.Create(text2);
			byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes("[Main]");
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Close();
		}
		if (GetFileEncodeType(text2) != Encoding.Unicode)
		{
			File.WriteAllText(text2, File.ReadAllText(text2, Encoding.Default), Encoding.Unicode);
		}
		PrivateDBIni = new HiniFile(text2, ";");
		ReadPrivateDBParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetThroughputRecipe(string recipename)
	{
		int num = 0;
		try
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + recipename + "\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = text + "Setting.ini";
			bool num2 = File.Exists(text2);
			num = 1;
			if (num2 && !IsFileLocked(text2) && GetFileEncodeType(text2) != Encoding.Unicode)
			{
				File.WriteAllText(text2, File.ReadAllText(text2, Encoding.Default), Encoding.Unicode);
			}
			num = 2;
			ThroughputINI = new HiniFile(text2, ";");
			ThroughputFile.SetINI(ThroughputINI);
			ThroughputFile.SetInspection(Station);
			ThroughputFile.ReadThroughputParam();
			ThroughputFile.ReadParetoItems();
			ThroughputFile.ReadSizeRecordSetupParam();
			EnableMultiParetoMode = ThroughputFile.GetEnableMultiParetoMode();
			ThroughputFile.ReadInspSurvParam();
			ThroughputFile.ReadMainParam();
			cb_ParetoNGSelect.Items.Clear();
			cb_ParetoNGSelect.Items.Add("All NG");
			num = 3;
			int num3 = 0;
			for (int i = 1; i < 30 && ThroughputFile.param.AllParetoItems[i] != ""; i++)
			{
				cb_ParetoNGSelect.Items.Add(ThroughputFile.param.AllParetoItems[i]);
				num3++;
			}
			num = 4;
			if (Station != null)
			{
				for (int j = 0; j < 8; j++)
				{
					Station[j].SetParetoChartName(ThroughputFile.param.AllParetoItems);
					Station[j].ParetoItemCount = num3;
					Station[j].paramFile.SetComDigits(ThroughputFile.ComDigits);
					Station[j].EnableMultiParetoMode = EnableMultiParetoMode;
					Station[j].SetReverseBin(ThroughputFile.param.BinReverseMode);
					Station[j].paramFile.ThroughputFile = ThroughputFile;
				}
			}
			cb_ParetoNGSelect.SelectedIndex = 0;
			num = 5;
			if (!ThroughputFile.EnableSetOpenCVThreadNum)
			{
				lab_EnableSetOpenCVThreadNum.Visibility = Visibility.Hidden;
				lab_ThreadNum.Visibility = Visibility.Hidden;
			}
			if (Station != null && Station[0].aiEnhanceTools != null && !Station[0].aiEnhanceTools.useCUDA)
			{
				lab_EnableGPU.Visibility = Visibility.Hidden;
			}
			num = 6;
			if (!ThroughputFile.CpuUseOptimized)
			{
				bool useOptimized = false;
				Cv2.SetUseOptimized(useOptimized);
				for (int k = 0; k < 8; k++)
				{
					if (Station[k].advLocateTool != null)
					{
						Station[k].advLocateTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].aiEnhanceTools != null)
					{
						Station[k].aiEnhanceTools.SetUseOptimized(useOptimized);
					}
					if (Station[k].aiPredictTools != null)
					{
						Station[k].aiPredictTools.SetUseOptimized(useOptimized);
					}
					if (Station[k].differenceTool != null)
					{
						Station[k].differenceTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].enhanceTool != null)
					{
						Station[k].enhanceTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].lightmonitorTool != null)
					{
						Station[k].lightmonitorTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].lineanalysisTool != null)
					{
						Station[k].lineanalysisTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].locateTool != null)
					{
						Station[k].locateTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].maskTool != null)
					{
						Station[k].maskTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].measuretool != null)
					{
						Station[k].measuretool.SetUseOptimized(useOptimized);
					}
					if (Station[k].selectfaceTool != null)
					{
						Station[k].selectfaceTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].selectwayTool != null)
					{
						Station[k].selectwayTool.SetUseOptimized(useOptimized);
					}
					if (Station[k].smoothTool != null)
					{
						Station[k].smoothTool.SetUseOptimized(useOptimized);
					}
					for (int l = 0; l < 20; l++)
					{
						if (Station[k].matchTool != null && Station[k].matchTool[l] != null)
						{
							Station[k].matchTool[l].SetUseOptimized(useOptimized);
						}
						if (Station[k].blockavgTool != null && Station[k].blockavgTool[l] != null)
						{
							Station[k].blockavgTool[l].SetUseOptimized(useOptimized);
						}
					}
					for (int m = 0; m < 30; m++)
					{
						if (Station[k].binaryTool != null && Station[k].binaryTool[m] != null)
						{
							Station[k].binaryTool[m].SetUseOptimized(useOptimized);
						}
					}
				}
			}
			if (!Cv2.UseOptimized())
			{
				lab_CpuUseOptimized.Visibility = Visibility.Hidden;
			}
			num = 7;
			if (ThroughputFile.EnableSetOpenCVThreadNum)
			{
				int openCVThreadNum = ThroughputFile.OpenCVThreadNum;
				lab_ThreadNum.Content = openCVThreadNum;
				for (int n = 0; n < 8; n++)
				{
					if (Station[n].advLocateTool != null)
					{
						Station[n].advLocateTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].aiEnhanceTools != null)
					{
						Station[n].aiEnhanceTools.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].aiPredictTools != null)
					{
						Station[n].aiPredictTools.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].differenceTool != null)
					{
						Station[n].differenceTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].enhanceTool != null)
					{
						Station[n].enhanceTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].lightmonitorTool != null)
					{
						Station[n].lightmonitorTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].lineanalysisTool != null)
					{
						Station[n].lineanalysisTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].locateTool != null)
					{
						Station[n].locateTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].maskTool != null)
					{
						Station[n].maskTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].measuretool != null)
					{
						Station[n].measuretool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].selectfaceTool != null)
					{
						Station[n].selectfaceTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].selectwayTool != null)
					{
						Station[n].selectwayTool.SetNumThreads(openCVThreadNum);
					}
					if (Station[n].smoothTool != null)
					{
						Station[n].smoothTool.SetNumThreads(openCVThreadNum);
					}
					for (int num4 = 0; num4 < 20; num4++)
					{
						if (Station[n].matchTool != null && Station[n].matchTool[num4] != null)
						{
							Station[n].matchTool[num4].SetNumThreads(openCVThreadNum);
						}
						if (Station[n].blockavgTool != null && Station[n].blockavgTool[num4] != null)
						{
							Station[n].blockavgTool[num4].SetNumThreads(openCVThreadNum);
						}
					}
					for (int num5 = 0; num5 < 30; num5++)
					{
						if (Station[n].binaryTool != null && Station[n].binaryTool[num5] != null)
						{
							Station[n].binaryTool[num5].SetNumThreads(openCVThreadNum);
						}
					}
				}
			}
			lab_Customer.Content = "C:" + ThroughputFile.CurrentClient;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat("ToolsWindow-SetThroughputRecipe() + DebugCode = " + num + " ;", ex.ToString()));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffers(List<byte[]>[] recentbufferlist, List<byte[]>[] okbufferlist, List<byte[]>[] ngbufferlist, List<byte[]>[] retestbufferlist, List<int>[] okIDLists, List<int>[] ngIDLists, List<int>[] recentIDLists, int[] lastIndex, int[] w, int[] h, int[] band)
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
				lastIndex[i]--;
				if (lastIndex[i] < 0)
				{
					lastIndex[i] = 0;
				}
				Station[i].SetBuffers(recentbufferlist[i], okbufferlist[i], ngbufferlist[i], retestbufferlist[i], okIDLists[i], ngIDLists[i], recentIDLists[i], lastIndex[i], w[i], h[i], band[i]);
				if (lastIndex[i] >= 0 && lastIndex[i] < recentbufferlist[i].Count)
				{
					RecentBufferIndex[i] = lastIndex[i];
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-SetBuffers()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetStationIndex(int index)
	{
		StationIndex = index;
		ReadToolONStatus();
		for (int i = 0; i < 8; i++)
		{
			StationBtn[i].Background = System.Windows.Media.Brushes.White;
		}
		StationBtn[index].Background = System.Windows.Media.Brushes.HotPink;
		lab_StatinoIndex.Content = (StationIndex + 1).ToString();
		try
		{
			switch (ImageListSource)
			{
			case 0:
				ShowNoContent();
				break;
			case 1:
			{
				if (filelist[StationIndex] == null)
				{
					ShowNoContent();
					break;
				}
				if (filelist[StationIndex].Length == 0)
				{
					ShowNoContent();
					break;
				}
				if (fileIndex >= filelist[StationIndex].Length)
				{
					fileIndex = 0;
				}
				RuncountText = string.Format("{0} / {1}", fileIndex + 1, filelist[StationIndex].Length);
				btn_ImageNum.Content = RuncountText;
				string text = LanguageManager.Instance["FileNotFound"];
				if (!File.Exists(filelist[StationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[StationIndex][fileIndex] + text);
					break;
				}
				if (!File.Exists(filelist[StationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[StationIndex][fileIndex] + text);
					break;
				}
				Mat buffers = CVBuffer.ImRead(filelist[StationIndex][fileIndex]);
				Station[StationIndex].SetBuffers(buffers);
				DrawImage(Station[StationIndex].buffers.Src);
				break;
			}
			case 2:
				if (Station[StationIndex].RecentBufferList == null)
				{
					ShowNoContent();
					break;
				}
				if (Station[StationIndex].RecentBufferList.Count == 0)
				{
					ShowNoContent();
					break;
				}
				if (RecentBufferIndex[StationIndex] >= Station[StationIndex].RecentBufferList.Count || RecentBufferIndex[StationIndex] < 0)
				{
					RecentBufferIndex[StationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", RecentBufferIndex[StationIndex] + 1, Station[StationIndex].RecentBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[StationIndex].SetBuffers(Station[StationIndex].RecentBufferList[RecentBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
				DrawImage(Station[StationIndex].buffers.Src);
				break;
			case 3:
				if (Station[StationIndex].OKBufferList == null)
				{
					ShowNoContent();
					break;
				}
				if (Station[StationIndex].OKBufferList.Count == 0)
				{
					ShowNoContent();
					break;
				}
				if (OKBufferIndex[StationIndex] >= Station[StationIndex].OKBufferList.Count)
				{
					OKBufferIndex[StationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", OKBufferIndex[StationIndex] + 1, Station[StationIndex].OKBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[StationIndex].SetBuffers(Station[StationIndex].OKBufferList[OKBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
				DrawImage(Station[StationIndex].buffers.Src);
				break;
			case 4:
				if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect] == null)
				{
					ShowNoContent();
					break;
				}
				if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count == 0)
				{
					ShowNoContent();
					break;
				}
				if (NGBufferIndex[StationIndex] >= Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count)
				{
					NGBufferIndex[StationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", NGBufferIndex[StationIndex] + 1, Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count);
				btn_ImageNum.Content = RuncountText;
				Station[StationIndex].SetBuffers(Station[StationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
				DrawImage(Station[StationIndex].buffers.Src);
				break;
			case 5:
				if (Station[StationIndex].RetestBufferList == null)
				{
					ShowNoContent();
					break;
				}
				if (Station[StationIndex].RetestBufferList.Count == 0)
				{
					ShowNoContent();
					break;
				}
				if (RetestBufferIndex[StationIndex] >= Station[StationIndex].RetestBufferList.Count)
				{
					RetestBufferIndex[StationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", RetestBufferIndex[StationIndex] + 1, Station[StationIndex].RetestBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[StationIndex].SetBuffers(Station[StationIndex].RetestBufferList[RetestBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
				DrawImage(Station[StationIndex].buffers.Src);
				break;
			}
		}
		catch
		{
			CVBuffer.SaveLog(string.Format("TW01-ToolsWindow-SetStationIndex()- Station={0}, RecentBufferIndex={1}, RecentBufferListCount={2}, ImageListSource{3}", StationIndex, RecentBufferIndex[StationIndex], Station[StationIndex].RecentBufferList.Count, ImageListSource));
		}
	}

	public void GetMeasureValue(ref double MinLength, ref double MaxLength, ref double MinWidth, ref double MaxWidth, ref double MinThickness, ref double MaxThickness, ref double LMinPadWidth, ref double LMaxPadWidth, ref double RMinPadWidth, ref double RMaxPadWidth, ref double MinGapSize, ref double MaxGapSize)
	{
		MinLength = ThroughputFile.param.ComDigits0;
		MaxLength = ThroughputFile.param.ComDigits1;
		MinWidth = ThroughputFile.param.ComDigits2;
		MaxWidth = ThroughputFile.param.ComDigits3;
		MinThickness = ThroughputFile.param.ComDigits4;
		MaxThickness = ThroughputFile.param.ComDigits5;
		LMinPadWidth = ThroughputFile.param.ComDigits6;
		LMaxPadWidth = ThroughputFile.param.ComDigits7;
		RMinPadWidth = ThroughputFile.param.ComDigits8;
		RMaxPadWidth = ThroughputFile.param.ComDigits9;
		MinGapSize = ThroughputFile.param.ComDigits10;
		MaxGapSize = ThroughputFile.param.ComDigits11;
	}

	public void GetMeasureValueExtend(ref double[] ComDigitsExtend)
	{
		ComDigitsExtend[0] = ThroughputFile.param.ComDigits12;
		ComDigitsExtend[1] = ThroughputFile.param.ComDigits13;
		ComDigitsExtend[2] = ThroughputFile.param.ComDigits14;
		ComDigitsExtend[3] = ThroughputFile.param.ComDigits15;
		ComDigitsExtend[4] = ThroughputFile.param.ComDigits16;
		ComDigitsExtend[5] = ThroughputFile.param.ComDigits17;
		ComDigitsExtend[6] = ThroughputFile.param.ComDigits18;
		ComDigitsExtend[7] = ThroughputFile.param.ComDigits19;
		ComDigitsExtend[8] = ThroughputFile.param.ComDigits20;
		ComDigitsExtend[9] = ThroughputFile.param.ComDigits21;
		ComDigitsExtend[10] = ThroughputFile.param.ComDigits22;
		ComDigitsExtend[11] = ThroughputFile.param.ComDigits23;
	}

	public List<INSPSURVITEM> GetInspSurvItem()
	{
		return ThroughputFile.IsItems;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetUserAuthority(User_Permissions permissions)
	{
		User = permissions;
		if (User == User_Permissions.USER_SuperUser)
		{
			lab_User.Content = "SuperUser";
		}
		else if (User == User_Permissions.USER_Administrator)
		{
			lab_User.Content = "Administrator";
		}
		else if (User == User_Permissions.USER_Engineer)
		{
			lab_User.Content = "Engineer";
		}
		else if (User == User_Permissions.USER_OP)
		{
			lab_User.Content = "OP";
		}
		else
		{
			lab_User.Content = "SuperUser";
		}
	}

	public bool GetSavingStatus()
	{
		return RecipSaving;
	}

	public bool GetLoadingStatus()
	{
		return RecipLoading;
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

	public void SetUserID(string id)
	{
		UserID = id;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LoadLanguage(int language)
	{
		try
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
			for (int i = 0; i < 8; i++)
			{
				if (Station[i] != null)
				{
					Station[i].buffers.LanguageType = language;
					Station[i].paramFile.LanguageType = language;
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-LoadLanguage()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LoadLanguageManual()
	{
		if (LanguageType == 1)
		{
			gb_TestResult.Header = "檢測結果";
			chk_LocateToolON.Content = "定位";
			chk_SelectWayON.Content = "選向";
			chk_SelectFaceON.Content = "選面";
			chk_EnhanceToolON.Content = "前處理";
			chk_AdvLocateToolON.Content = "進階定位";
			chk_MeasureToolON.Content = "尺寸量測";
			chk_MaskToolON.Content = "遮罩";
			chk_BinaryToolON.Content = "二值化";
			chk_SmoothToolON.Content = "平滑二值化";
			chk_BlockAvgToolON.Content = "區域二值化";
			chk_DifferenceToolON.Content = "差異二值化";
			chk_LineAnalysisON.Content = "線段分析";
			chk_MatchToolON.Content = "圖形比對";
			chk_FontToolON.Content = "字元比對";
			chk_IRToolON.Content = "色碼工具";
		}
		else if (LanguageType == 2)
		{
			gb_TestResult.Header = "检测结果";
			chk_LocateToolON.Content = "定位";
			chk_SelectWayON.Content = "选向";
			chk_SelectFaceON.Content = "选面";
			chk_EnhanceToolON.Content = "前处理";
			chk_AdvLocateToolON.Content = "进阶定位";
			chk_MeasureToolON.Content = "尺寸量测";
			chk_MaskToolON.Content = "遮罩";
			chk_BinaryToolON.Content = "二值化";
			chk_SmoothToolON.Content = "平滑二值化";
			chk_BlockAvgToolON.Content = "区域二值化";
			chk_DifferenceToolON.Content = "差异二值化";
			chk_LineAnalysisON.Content = "线段分析";
			chk_MatchToolON.Content = "图形比对";
			chk_FontToolON.Content = "字元比对";
			chk_IRToolON.Content = "色码工具";
		}
		else if (LanguageType == 3)
		{
			gb_TestResult.Header = "検査結果";
			chk_LocateToolON.Content = "位置合わせ";
			chk_SelectWayON.Content = "向き選択";
			chk_SelectFaceON.Content = "面選択";
			chk_EnhanceToolON.Content = "前処理";
			chk_AdvLocateToolON.Content = "高度な位置合わせ";
			chk_MeasureToolON.Content = "寸法測定";
			chk_MaskToolON.Content = "マスク";
			chk_BinaryToolON.Content = "二値化";
			chk_SmoothToolON.Content = "滑らかな二値化";
			chk_BlockAvgToolON.Content = "領域二値化";
			chk_DifferenceToolON.Content = "差分二値化";
			chk_LineAnalysisON.Content = "線形解析";
			chk_MatchToolON.Content = "図形比較";
			chk_FontToolON.Content = "文字比較";
			chk_IRToolON.Content = "色コードツール";
		}
		else
		{
			gb_TestResult.Header = "Inspect Result";
			chk_LocateToolON.Content = "Locate Tool";
			chk_SelectWayON.Content = "Select Way";
			chk_SelectFaceON.Content = "Select Face";
			chk_EnhanceToolON.Content = "Enhance Tool";
			chk_AdvLocateToolON.Content = "Adv. Locate";
			chk_MeasureToolON.Content = "Measure Tool";
			chk_MaskToolON.Content = "Mask Tool";
			chk_BinaryToolON.Content = "Binary Tool";
			chk_SmoothToolON.Content = "Smooth Binary";
			chk_BlockAvgToolON.Content = "Block Binary";
			chk_DifferenceToolON.Content = "Difference Binary";
			chk_LineAnalysisON.Content = "Line Analysis";
			chk_MatchToolON.Content = "Match Tool";
			chk_FontToolON.Content = "Font Tool";
			chk_IRToolON.Content = "IRTool";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ShowNoContent()
	{
		RuncountText = string.Format("{0} / {1}", 0, 0);
		btn_ImageNum.Content = RuncountText;
		cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
	}

	public void SetBitmapBuffer(Bitmap bitmap)
	{
		Station[StationIndex].buffers.SetBuffer(bitmap);
	}

	public void SetLowLevelVersion()
	{
		btn_SetAll.Visibility = Visibility.Hidden;
		btn_AddToDB.Visibility = Visibility.Hidden;
		btn_OKDB.Visibility = Visibility.Hidden;
		btn_NGDB.Visibility = Visibility.Hidden;
		btn_AIOKDB.Visibility = Visibility.Hidden;
		btn_AINGDB.Visibility = Visibility.Hidden;
		btn_DelAIOKDB.Visibility = Visibility.Hidden;
		chk_OKStop.Visibility = Visibility.Hidden;
		chk_NGStop.Visibility = Visibility.Hidden;
		btn_ImgT1.Visibility = Visibility.Hidden;
		btn_ImgT2.Visibility = Visibility.Hidden;
		btn_ImgT3.Visibility = Visibility.Hidden;
		btn_YieldRetest.Visibility = Visibility.Hidden;
		grd_AINGQuickDraw.Visibility = Visibility.Hidden;
		lab_AINGTempCount.Visibility = Visibility.Hidden;
		btn_DrawTempMask.Visibility = Visibility.Hidden;
		btn_AddToNGTemp.Visibility = Visibility.Hidden;
		btn_ParamCopy.Visibility = Visibility.Hidden;
		btn_ParetoChart.Visibility = Visibility.Hidden;
		btn_SizeStatic.Visibility = Visibility.Hidden;
		btn_InspSurv.Visibility = Visibility.Hidden;
		chk_SmoothToolON.Visibility = Visibility.Hidden;
		btn_SmoothTool.Visibility = Visibility.Hidden;
		lab_Time8.Visibility = Visibility.Hidden;
		chk_AIPredictToolON.Visibility = Visibility.Hidden;
		btn_AIPredictTool.Visibility = Visibility.Hidden;
		chk_SaveAI.Visibility = Visibility.Hidden;
		lab_TimeAIPredict.Visibility = Visibility.Hidden;
		chk_LineAnalysisON.Visibility = Visibility.Hidden;
		btn_LineAnalysis.Visibility = Visibility.Hidden;
		lab_Time11.Visibility = Visibility.Hidden;
		chk_DifferenceToolON.Visibility = Visibility.Hidden;
		btn_DifferenceTool.Visibility = Visibility.Hidden;
		lab_Time10.Visibility = Visibility.Hidden;
		chk_BlockAvgToolON.Visibility = Visibility.Hidden;
		btn_BlockAvgTool.Visibility = Visibility.Hidden;
		lab_Time9.Visibility = Visibility.Hidden;
		chk_FontToolON.Visibility = Visibility.Hidden;
		btn_FontTool.Visibility = Visibility.Hidden;
		btn_AutoFontMatch.Visibility = Visibility.Hidden;
		chk_SaveFontAI.Visibility = Visibility.Hidden;
		lab_TimeFontTool.Visibility = Visibility.Hidden;
		lab_FontToolText.Visibility = Visibility.Hidden;
		chk_IRToolON.Visibility = Visibility.Hidden;
		btn_IRTool.Visibility = Visibility.Hidden;
		lab_TimeIRTool.Visibility = Visibility.Hidden;
		chk_SelectFaceON.Visibility = Visibility.Hidden;
		btn_SelectFaceTool.Visibility = Visibility.Hidden;
		lab_Time2_2.Visibility = Visibility.Hidden;
		btn_LightMonitorTool.Visibility = Visibility.Hidden;
		LowLevelVersion = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DrawImage(Mat mat)
	{
		Mat heatMap = new Mat();
		_ = StationIndex;
		int num = 0;
		if (getAIResult != null && !Station[StationIndex].maskTool.AIMat.Empty())
		{
			num = getAIResult(StationIndex, Station[StationIndex].maskTool.AIMat, ref heatMap);
			btn_AIPredictTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		cnv_Image.SetValue(FrameworkElement.WidthProperty, 720.0);
		cnv_Image.SetValue(FrameworkElement.HeightProperty, 540.0);
		ImageWidth[StationIndex] = mat.Width;
		ImageHeight[StationIndex] = mat.Height;
		ImageBand[StationIndex] = mat.Channels();
		xRatio[StationIndex] = cnv_Image.Width / (double)ImageWidth[StationIndex];
		yRatio[StationIndex] = cnv_Image.Height / (double)ImageHeight[StationIndex];
		if (xRatio[StationIndex] > yRatio[StationIndex])
		{
			xRatio[StationIndex] = yRatio[StationIndex];
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)ImageWidth[StationIndex] * xRatio[StationIndex]);
		}
		else
		{
			yRatio[StationIndex] = xRatio[StationIndex];
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)ImageHeight[StationIndex] * yRatio[StationIndex]);
		}
		ImageSource imageSource;
		if (heatMap.Empty() || Station[StationIndex].maskTool.AIMat.Empty() || num != 3)
		{
			Mat mat2 = new Mat();
			if (mat.Channels() == 3 || mat.Channels() == 4)
			{
				mat2 = mat.Clone();
			}
			else
			{
				Cv2.CvtColor(mat, mat2, ColorConversionCodes.GRAY2BGR);
			}
			OldDst = mat;
			imageSource = Station[StationIndex].buffers.MatToBitmapImage(mat);
		}
		else
		{
			lab_TestResult.Content = "NG";
			Label_Result.Content = "AI NG";
			SetResultLine(TOOL_RESULT.EM_NG);
			Label_Result.Background = System.Windows.Media.Brushes.Red;
			ep_Result.Fill = System.Windows.Media.Brushes.Red;
			btn_AIPredictTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			btn_AIPredictTool.BorderBrush = System.Windows.Media.Brushes.Red;
			int num2 = mat.Height / 2;
			OpenCvSharp.Rect roi = new OpenCvSharp.Rect(0, 0, mat.Width, num2);
			OpenCvSharp.Rect roi2 = new OpenCvSharp.Rect(0, mat.Height - num2, mat.Width, mat.Height - num2);
			Mat mat3 = mat.Clone();
			Mat mat4 = new Mat(mat3, roi);
			Mat mat5 = new Mat(mat3, roi2);
			Cv2.Resize(heatMap, mat4, new OpenCvSharp.Size(mat.Cols, num2));
			Cv2.Resize(Station[StationIndex].maskTool.AIMat, mat5, new OpenCvSharp.Size(mat.Cols, mat.Rows - num2));
			OldDst = mat3;
			imageSource = Station[StationIndex].buffers.MatToBitmapImage(mat3);
		}
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		if (exp_PreviewAI.IsExpanded)
		{
			Mat aIMat = Station[StationIndex].GetAIMat();
			if (aIMat.Width > 0 && aIMat.Height > 0)
			{
				if (aIMat.Width < 480 && aIMat.Height < 360)
				{
					exp_PreviewAI.Width = aIMat.Width + 20;
					exp_PreviewAI.Height = aIMat.Height + 5;
					grd_AIImage.Width = aIMat.Width;
					grd_AIImage.Height = aIMat.Height;
					cnv_AIImage.Width = aIMat.Width;
					cnv_AIImage.Height = aIMat.Height;
				}
				else if (aIMat.Width >= 480 || aIMat.Height >= 360)
				{
					exp_PreviewAI.Width = 500.0;
					exp_PreviewAI.Height = 365.0;
					grd_AIImage.Width = 480.0;
					grd_AIImage.Height = 360.0;
					cnv_AIImage.Width = 480.0;
					cnv_AIImage.Height = 360.0;
				}
				ImageBrush imageBrush2 = new ImageBrush();
				imageBrush2.ImageSource = Station[StationIndex].buffers.MatToBitmapImage(aIMat);
				cnv_AIImage.Background = imageBrush2;
			}
			else
			{
				cnv_AIImage.Background = System.Windows.Media.Brushes.Transparent;
			}
		}
		if (!exp_PreviewAIHeatMap.IsExpanded)
		{
			return;
		}
		Mat heatMapImage = Station[StationIndex].aiProphetTools.GetHeatMapImage();
		if (heatMapImage.Width > 0 && heatMapImage.Height > 0)
		{
			if (heatMapImage.Width < 480 && heatMapImage.Height < 360)
			{
				exp_PreviewAIHeatMap.Width = heatMapImage.Width + 20;
				exp_PreviewAIHeatMap.Height = heatMapImage.Height + 5;
				grd_AIHeatMapImage.Width = heatMapImage.Width;
				grd_AIHeatMapImage.Height = heatMapImage.Height;
				cnv_AIHeatMapImage.Width = heatMapImage.Width;
				cnv_AIHeatMapImage.Height = heatMapImage.Height;
			}
			else if (heatMapImage.Width >= 480 || heatMapImage.Height >= 360)
			{
				exp_PreviewAIHeatMap.Width = 500.0;
				exp_PreviewAIHeatMap.Height = 365.0;
				grd_AIHeatMapImage.Width = 480.0;
				grd_AIHeatMapImage.Height = 360.0;
				cnv_AIHeatMapImage.Width = 480.0;
				cnv_AIHeatMapImage.Height = 360.0;
			}
			ImageBrush imageBrush3 = new ImageBrush();
			imageBrush3.ImageSource = Station[StationIndex].buffers.MatToBitmapImage(heatMapImage);
			cnv_AIHeatMapImage.Background = imageBrush3;
			lab_AIProphetScore.Content = Station[StationIndex].aiProphetTools.FinalScore;
		}
		else
		{
			cnv_AIHeatMapImage.Background = System.Windows.Media.Brushes.Transparent;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TestAll_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].buffers.SrcReady)
		{
			SetResult(result: false, FLOW_BIN.EM_FLOW_RETESET);
			txt_InspResult.Clear();
			txt_InspResult.Text = LanguageManager.Instance["SetImageFirst"];
			return;
		}
		if ((chk_AIEnhanceToolON.IsChecked.Value || chk_AIPredictToolON.IsChecked.Value) && !Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
		}
		TestPressed = true;
		WriteToolONStatus();
		CheckUpdateModel();
		Inspect();
		UpdateInspTime();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CheckUpdateModel()
	{
		if (RemoteMode)
		{
			return;
		}
		string path = string.Format("{0}\\Comm\\", AppDomain.CurrentDomain.BaseDirectory);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string text = string.Format("{0}\\Comm\\{1}", AppDomain.CurrentDomain.BaseDirectory, "UpdateModel.msg");
		if (!File.Exists(text))
		{
			return;
		}
		FileInfo fileInfo = new FileInfo(text);
		if (fileInfo.IsReadOnly)
		{
			fileInfo.IsReadOnly = false;
		}
		File.Delete(text);
		for (int i = 0; i < 8; i++)
		{
			if (chk_AIEnhanceToolON.IsChecked == true || chk_AIPredictToolON.IsChecked == true)
			{
				if (Station[StationIndex].paramFile.AIEnhanceParam.ValueChangeFlag)
				{
					Station[i].paramFile.ReadAIEnhanceParam();
				}
				else
				{
					Station[i].paramFile.ReadAIPredictParam();
				}
				LoadingAIModel[i] = true;
			}
		}
		AILoadingPauseEvent.Set();
		System.Windows.MessageBox.Show("遠端模型更新完成", "注意");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(bool Show = true)
	{
		bool flag = true;
		Station[StationIndex].SetStationIndex(StationIndex);
		FLOW_BIN bin = ((getAIResult == null) ? Station[StationIndex].Inspect(ref DrawMat, offLine: true) : Station[StationIndex].Inspect(ref DrawMat, offLine: true, UseAI: true)).bin;
		Mat mat = new Mat();
		try
		{
			if (ImageChannnel != 0)
			{
				if ((Station[StationIndex].paramFile.AIEnhanceToolON || Station[StationIndex].paramFile.AIPredictToolON) && (ImageChannnel == 17 || ImageChannnel == 18))
				{
					if (Station[StationIndex].buffers.GetChannelMat(ImageChannnel - 1).Width <= 0 || Station[StationIndex].buffers.GetChannelMat(ImageChannnel - 1).Height <= 0)
					{
						return false;
					}
					using Mat mat2 = Station[StationIndex].buffers.GetEnhanceAiMatColor().Clone();
					using Mat m = new Mat(DrawMat, Station[StationIndex].LocateImageRect);
					if (bin != FLOW_BIN.EM_FLOW_RETESET)
					{
						if (Station[StationIndex].paramFile.AIEnhanceParam.DnetMode_A == 1 || Station[StationIndex].paramFile.AIEnhanceParam.DnetMode_A == 2)
						{
							Mat mat3 = new Mat();
							Cv2.CvtColor(mat2, mat3, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat3, 20.0, 255.0, mat3);
							if (ImageChannnel == 18)
							{
								Cv2.FindContours(mat3, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
								Mat mat4 = mat2.Clone();
								mat4.SetTo(0.0);
								for (int i = 0; i < contours.Length; i++)
								{
									Cv2.DrawContours(mat4, contours, i, Scalar.White, 3);
								}
								Cv2.CvtColor(mat4, mat4, ColorConversionCodes.BGR2GRAY);
								mat3.SetTo(0.0);
								mat4.CopyTo(mat3);
							}
							mat2.CopyTo(m, mat3);
						}
						else
						{
							mat2.CopyTo(m);
						}
					}
					mat = DrawMat;
				}
				else if (ImageChannnel >= 19)
				{
					if (Station[StationIndex].buffers.GetAILocateMat(ImageChannnel - 1 + 3).Width <= 0 || Station[StationIndex].buffers.GetAILocateMat(ImageChannnel - 1 + 3).Height <= 0)
					{
						return false;
					}
					using Mat mat5 = Station[StationIndex].buffers.GetAILocateMat(ImageChannnel - 1 + 3).Clone();
					Cv2.CvtColor(mat5, mat5, ColorConversionCodes.GRAY2BGR);
					mat = mat5.Clone();
				}
				else
				{
					if (Station[StationIndex].buffers.GetChannelMat(ImageChannnel - 1).Width <= 0 || Station[StationIndex].buffers.GetChannelMat(ImageChannnel - 1).Height <= 0)
					{
						return false;
					}
					using Mat mat6 = Station[StationIndex].buffers.GetChannelMat(ImageChannnel - 1).Clone();
					using Mat m2 = new Mat(DrawMat, Station[StationIndex].LocateImageRect);
					Cv2.CvtColor(mat6, mat6, ColorConversionCodes.GRAY2BGR);
					mat6.CopyTo(m2);
					mat = DrawMat;
				}
			}
			else
			{
				mat = DrawMat;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Inspect() Image Channel Convert Error!" + ex.ToString());
			return false;
		}
		if (mat.Width <= 0 || mat.Height <= 0)
		{
			txt_InspResult.Text = Station[StationIndex].ErrorMsg;
			return false;
		}
		try
		{
			Mat mat7 = new Mat();
			if (mat.Channels() == 3 || mat.Channels() == 4)
			{
				mat7 = mat.Clone();
			}
			else
			{
				Cv2.CvtColor(mat, mat7, ColorConversionCodes.GRAY2BGR);
			}
			if (!Show)
			{
				flag = bin == FLOW_BIN.EM_FLOW_OK;
			}
			else
			{
				txt_InspResult.Text = "";
				if (bin != FLOW_BIN.EM_FLOW_OK)
				{
					txt_InspResult.Text = Station[StationIndex].ErrorMsg;
					SetResult(result: false, bin);
					flag = false;
				}
				else
				{
					SetResult(result: true);
					txt_InspResult.Text = Station[StationIndex].ErrorMsg;
				}
				OpenCvSharp.Rect locateImageRect = Station[StationIndex].LocateImageRect;
				OpenCvSharp.Rect locateRect = Station[StationIndex].LocateRect;
				locateRect.Left = Station[StationIndex].LocateRect.Left + locateImageRect.Left;
				locateRect.Top = Station[StationIndex].LocateRect.Top + locateImageRect.Top;
				int thickness = 1;
				if (mat7.Width > 640 || mat7.Height > 480)
				{
					thickness = 2;
				}
				if (ReferenceShow)
				{
					mat7.Rectangle(locateImageRect, Scalar.Blue, thickness);
					mat7.Rectangle(locateRect, Scalar.Lime, thickness);
				}
				if (ReferenceShow && Station[StationIndex].LocaetResult)
				{
					for (int j = 1; j < 20; j++)
					{
						if (Station[StationIndex].paramFile.AdvLocateParam[j].TrackerON && Station[StationIndex].paramFile.AdvLocateParam[j].InsideRect.Width > 0 && Station[StationIndex].paramFile.AdvLocateParam[j].InsideRect.Height > 0)
						{
							mat7.Rectangle(new OpenCvSharp.Rect
							{
								X = locateImageRect.X + Station[StationIndex].paramFile.AdvLocateParam[j].InsideRect.X,
								Y = locateImageRect.Y + Station[StationIndex].paramFile.AdvLocateParam[j].InsideRect.Y,
								Width = Station[StationIndex].paramFile.AdvLocateParam[j].InsideRect.Width,
								Height = Station[StationIndex].paramFile.AdvLocateParam[j].InsideRect.Height
							}, Scalar.Yellow, thickness);
						}
					}
				}
				if (!flag)
				{
					if (ReferenceShow)
					{
						for (int k = 0; k < Station[StationIndex].NGRect.Count; k++)
						{
							if (Station[StationIndex].NGRect[k].Width > 0 && Station[StationIndex].NGRect[k].Height > 0)
							{
								OpenCvSharp.Rect rect = new OpenCvSharp.Rect
								{
									X = locateImageRect.X + Station[StationIndex].NGRect[k].X,
									Y = locateImageRect.Y + Station[StationIndex].NGRect[k].Y,
									Width = Station[StationIndex].NGRect[k].Width,
									Height = Station[StationIndex].NGRect[k].Height
								};
								mat7.Rectangle(rect, Scalar.Red, thickness);
								if (k < Station[StationIndex].NGTool.Count && Station[StationIndex].NGTool[k] == InspToolsFunciton.EM_ADVLOCATE_A)
								{
									mat7.Line(new OpenCvSharp.Point(rect.Left, rect.Top), new OpenCvSharp.Point(rect.Left - 5, rect.Top - 5), Scalar.Red, thickness);
									mat7.Line(new OpenCvSharp.Point(rect.Right, rect.Top), new OpenCvSharp.Point(rect.Right + 5, rect.Top - 5), Scalar.Red, thickness);
									mat7.Line(new OpenCvSharp.Point(rect.Left, rect.Bottom), new OpenCvSharp.Point(rect.Left - 5, rect.Bottom + 5), Scalar.Red, thickness);
									mat7.Line(new OpenCvSharp.Point(rect.Right, rect.Bottom), new OpenCvSharp.Point(rect.Right + 5, rect.Bottom + 5), Scalar.Red, thickness);
								}
							}
						}
					}
					for (int l = 1; l <= Station[StationIndex].NGPt.Count; l += 2)
					{
						if (Station[StationIndex].NGPt[l].X > 0 && Station[StationIndex].NGPt[l].Y > 0 && Station[StationIndex].NGPt[l - 1].X > 0 && Station[StationIndex].NGPt[l - 1].Y > 0)
						{
							OpenCvSharp.Point pt = new OpenCvSharp.Point(Station[StationIndex].NGPt[l - 1].X + locateImageRect.X, Station[StationIndex].NGPt[l - 1].Y + locateImageRect.Y);
							OpenCvSharp.Point pt2 = new OpenCvSharp.Point(Station[StationIndex].NGPt[l].X + locateImageRect.X, Station[StationIndex].NGPt[l].Y + locateImageRect.Y);
							OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(pt.X - 3, pt.Y - 3, 6, 6);
							OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(pt2.X - 3, pt2.Y - 3, 6, 6);
							mat7.Rectangle(rect2, Scalar.Gold, -1);
							mat7.Rectangle(rect3, Scalar.Gold, -1);
							mat7.Rectangle(rect2, Scalar.Black);
							mat7.Rectangle(rect3, Scalar.Black);
							mat7.Line(pt, pt2, Scalar.Red, thickness);
						}
					}
				}
				DrawImage(mat7);
			}
			return flag;
		}
		catch (Exception ex2)
		{
			CVBuffer.SaveLog("ToolsWindow-Inspect() Image Show Error!" + ex2.ToString());
			txt_InspResult.Text = Station[StationIndex].ErrorMsg;
			return false;
		}
	}

	public bool InspectNoUI(bool Show = true)
	{
		bool flag = true;
		Station[StationIndex].SetStationIndex(StationIndex);
		if (AIDebug && filenameID != "")
		{
			Station[StationIndex].StationTxt = filenameID;
		}
		if (((getAIResult == null) ? Station[StationIndex].Inspect(ref DrawMat, offLine: true, UseAI: false, -1, ImageListSource, ImageAddr) : Station[StationIndex].Inspect(ref DrawMat, offLine: true, UseAI: true, -1, ImageListSource, ImageAddr)).bin != FLOW_BIN.EM_FLOW_OK)
		{
			return false;
		}
		return true;
	}

	private void SetResultLine(TOOL_RESULT result)
	{
		SolidColorBrush fill = System.Windows.Media.Brushes.Lime;
		switch (result)
		{
		case TOOL_RESULT.EM_OK:
			fill = System.Windows.Media.Brushes.Lime;
			break;
		case TOOL_RESULT.EM_NG:
			fill = System.Windows.Media.Brushes.Red;
			break;
		case TOOL_RESULT.EM_Retest:
			fill = System.Windows.Media.Brushes.Gray;
			break;
		}
		for (int i = 0; i < 7; i++)
		{
			ResultLine[i].Fill = fill;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetResult(bool result, FLOW_BIN binresult = FLOW_BIN.EM_FLOW_NG1)
	{
		Label_Result.Visibility = Visibility.Visible;
		btn_LocateTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_SelectWayTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_SelectFaceTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_EnhanceTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_AdvancedLocate.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_MeasureTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_MaskTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_BinaryTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_SmoothTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_DifferenceTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_BlockAvgTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_LineAnalysis.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_MatchTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_FontTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_AIPredictTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_AIEnhanceTool.BorderBrush = System.Windows.Media.Brushes.LightGray;
		btn_AIProphet.BorderBrush = System.Windows.Media.Brushes.LightGray;
		for (int i = 0; i < 18; i++)
		{
			ErrorEs[i].Visibility = Visibility.Hidden;
			ErrorLabel[i].Visibility = Visibility.Hidden;
		}
		if (Station[StationIndex].SelectFaceResult == 0)
		{
			btn_SelectFaceTool.BorderBrush = System.Windows.Media.Brushes.Lime;
			lab_FaceIndex.Visibility = Visibility.Hidden;
			es_FaceIndex.Visibility = Visibility.Hidden;
			lab_FaceIndex2.Visibility = Visibility.Hidden;
		}
		else
		{
			if (Station[StationIndex].SelectFaceResult == -1)
			{
				btn_SelectFaceTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			}
			else
			{
				btn_SelectFaceTool.BorderBrush = System.Windows.Media.Brushes.Lime;
			}
			lab_FaceIndex.Visibility = Visibility.Visible;
			es_FaceIndex.Visibility = Visibility.Visible;
			lab_FaceIndex2.Visibility = Visibility.Visible;
			string content = Station[StationIndex].SelectFaceResult switch
			{
				1 => "A", 
				2 => "B", 
				3 => "C", 
				4 => "D", 
				_ => "X", 
			};
			lab_FaceIndex.Content = content;
			lab_FaceIndex2.Content = content;
		}
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = System.Windows.Media.Brushes.Lime;
			lab_TestResult.Content = "OK";
			SetResultLine(TOOL_RESULT.EM_OK);
			ep_Result.Fill = System.Windows.Media.Brushes.Lime;
			if (Station[StationIndex].SelectWayDoFlip)
			{
				btn_SelectWayTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
				btn_Result_Flip.Visibility = Visibility.Visible;
			}
			else
			{
				btn_SelectWayTool.BorderBrush = System.Windows.Media.Brushes.Lime;
				btn_Result_Flip.Visibility = Visibility.Hidden;
			}
			return;
		}
		switch (binresult)
		{
		case FLOW_BIN.EM_FLOW_RETESET:
			lab_TestResult.Content = "Retest";
			Label_Result.Content = "Retest";
			SetResultLine(TOOL_RESULT.EM_Retest);
			Label_Result.Background = System.Windows.Media.Brushes.Gray;
			ep_Result.Fill = System.Windows.Media.Brushes.Gray;
			break;
		case FLOW_BIN.EM_FLOW_NG1:
			lab_TestResult.Content = "NG1";
			Label_Result.Content = "NG1";
			goto IL_0492;
		case FLOW_BIN.EM_FLOW_NG2:
			lab_TestResult.Content = "NG2";
			Label_Result.Content = "NG2";
			goto IL_0492;
		case FLOW_BIN.EM_FLOW_NG3:
			lab_TestResult.Content = "NG3";
			Label_Result.Content = "NG3";
			goto IL_0492;
		case FLOW_BIN.EM_FLOW_NG4:
			lab_TestResult.Content = "NG4";
			Label_Result.Content = "NG4";
			goto IL_0492;
		default:
			{
				lab_TestResult.Content = "NG";
				Label_Result.Content = "NG";
				goto IL_0492;
			}
			IL_0492:
			SetResultLine(TOOL_RESULT.EM_NG);
			Label_Result.Background = System.Windows.Media.Brushes.Red;
			ep_Result.Fill = System.Windows.Media.Brushes.Red;
			break;
		}
		if (Station[StationIndex].iResult.LocaetResult == TOOL_RESULT.EM_NG)
		{
			btn_LocateTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[0].Visibility = Visibility.Visible;
			ErrorLabel[0].Visibility = Visibility.Visible;
			ErrorLabel[0].Content = 1;
			return;
		}
		btn_LocateTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		if (Station[StationIndex].SelectWayDoFlip)
		{
			btn_SelectWayTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			btn_Result_Flip.Visibility = Visibility.Visible;
		}
		else
		{
			btn_SelectWayTool.BorderBrush = System.Windows.Media.Brushes.Lime;
			btn_Result_Flip.Visibility = Visibility.Hidden;
		}
		if (Station[StationIndex].AIProphetResult)
		{
			btn_AIProphet.BorderBrush = System.Windows.Media.Brushes.Lime;
			ErrorEs[17].Visibility = Visibility.Hidden;
			ErrorLabel[17].Visibility = Visibility.Hidden;
		}
		else
		{
			btn_AIProphet.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[17].Visibility = Visibility.Visible;
			ErrorLabel[17].Visibility = Visibility.Visible;
			ErrorLabel[17].Content = "1";
		}
		if (!Station[StationIndex].AdvlocateResult || !Station[StationIndex].AdvMeasureResult)
		{
			btn_AdvancedLocate.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[4].Visibility = Visibility.Visible;
			ErrorLabel[4].Visibility = Visibility.Visible;
			ErrorLabel[4].Content = Station[StationIndex].ErrorCount[4];
			if (!Station[StationIndex].AdvlocateResult)
			{
				return;
			}
		}
		else
		{
			btn_AdvancedLocate.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		btn_EnhanceTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		if (chk_MaskToolON.IsChecked.Value)
		{
			btn_MaskTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].MeasureResult)
		{
			btn_MeasureTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[5].Visibility = Visibility.Visible;
			ErrorLabel[5].Visibility = Visibility.Visible;
			ErrorLabel[5].Content = Station[StationIndex].ErrorCount[5];
		}
		else
		{
			btn_MeasureTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].BinaryResult)
		{
			btn_BinaryTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[7].Visibility = Visibility.Visible;
			ErrorLabel[7].Visibility = Visibility.Visible;
			ErrorLabel[7].Content = Station[StationIndex].ErrorCount[7];
		}
		else
		{
			btn_BinaryTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].SmoothResult)
		{
			btn_SmoothTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[8].Visibility = Visibility.Visible;
			ErrorLabel[8].Visibility = Visibility.Visible;
			ErrorLabel[8].Content = Station[StationIndex].ErrorCount[8];
		}
		else
		{
			btn_SmoothTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].DifferenceResult)
		{
			btn_DifferenceTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[10].Visibility = Visibility.Visible;
			ErrorLabel[10].Visibility = Visibility.Visible;
			ErrorLabel[10].Content = Station[StationIndex].ErrorCount[10];
		}
		else
		{
			btn_DifferenceTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].BlockAvgResult)
		{
			btn_BlockAvgTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[9].Visibility = Visibility.Visible;
			ErrorLabel[9].Visibility = Visibility.Visible;
			ErrorLabel[9].Content = Station[StationIndex].ErrorCount[9];
		}
		else
		{
			btn_BlockAvgTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].LineAnalysisResult)
		{
			btn_LineAnalysis.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[11].Visibility = Visibility.Visible;
			ErrorLabel[11].Visibility = Visibility.Visible;
			ErrorLabel[11].Content = Station[StationIndex].ErrorCount[11];
		}
		else
		{
			btn_LineAnalysis.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].MatchResult)
		{
			btn_MatchTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[12].Visibility = Visibility.Visible;
			ErrorLabel[12].Visibility = Visibility.Visible;
			ErrorLabel[12].Content = Station[StationIndex].ErrorCount[12];
		}
		else
		{
			btn_MatchTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].FontToolInspResult)
		{
			btn_FontTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[15].Visibility = Visibility.Visible;
			ErrorLabel[15].Visibility = Visibility.Visible;
			ErrorLabel[15].Content = 1;
		}
		else
		{
			btn_FontTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].AIPredictResult)
		{
			btn_AIPredictTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[13].Visibility = Visibility.Visible;
			ErrorLabel[13].Visibility = Visibility.Visible;
			ErrorLabel[13].Content = 1;
			Label_Result.Content = "AI NG";
		}
		else
		{
			btn_AIPredictTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
		if (!Station[StationIndex].IRToolInspResult)
		{
			btn_IRTool.BorderBrush = System.Windows.Media.Brushes.Fuchsia;
			ErrorEs[16].Visibility = Visibility.Visible;
			ErrorLabel[16].Visibility = Visibility.Visible;
			ErrorLabel[16].Content = 1;
		}
		else
		{
			btn_IRTool.BorderBrush = System.Windows.Media.Brushes.Lime;
		}
	}

	private void Btn_LocateTool_Click(object sender, RoutedEventArgs e)
	{
		if (Station[StationIndex].buffers.Src.Empty())
		{
			LocateWindow locateWindow = new LocateWindow();
			locateWindow.LoadLanguage(LanguageType);
			locateWindow.SetInspection(Station[StationIndex]);
			locateWindow.SetThroghputFile(ThroughputFile);
			locateWindow.SetPreviewMode(on: true);
			locateWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			locateWindow.SetClientCode(ThroughputFile.CurrentClient);
			locateWindow.SetSAM(mSam);
			locateWindow.SetAuthLevel(AuthLevel);
			locateWindow.ShowDialog();
		}
		else
		{
			LocateWindow locateWindow2 = new LocateWindow();
			locateWindow2.LoadLanguage(LanguageType);
			locateWindow2.SetInspection(Station[StationIndex]);
			locateWindow2.SetThroghputFile(ThroughputFile);
			locateWindow2.SetBuffer(Station[StationIndex].buffers.Src);
			locateWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			locateWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			locateWindow2.SetClientCode(ThroughputFile.CurrentClient);
			locateWindow2.SetSAM(mSam);
			locateWindow2.SetAuthLevel(AuthLevel);
			locateWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void Btn_BinaryTool_Click(object sender, RoutedEventArgs e)
	{
		OpenBinaryTool(0);
	}

	private void OpenBinaryTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			BinaryWindow binaryWindow = new BinaryWindow(User);
			binaryWindow.LoadLanguage(LanguageType);
			binaryWindow.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			binaryWindow.SetThroghputFile(ThroughputFile);
			binaryWindow.SetPreviewMode(on: true);
			binaryWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			binaryWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			BinaryWindow binaryWindow2 = new BinaryWindow(User);
			binaryWindow2.LoadLanguage(LanguageType);
			binaryWindow2.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			binaryWindow2.SetThroghputFile(ThroughputFile);
			binaryWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			binaryWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			binaryWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			binaryWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Stage1_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			WriteToolONStatus();
			System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
			StationIndex = Convert.ToInt32(button.Uid);
			SetStationIndex(StationIndex);
			if (Station[StationIndex].TempImage[0].Empty())
			{
				btn_ImgT1.Background = System.Windows.Media.Brushes.White;
			}
			else
			{
				btn_ImgT1.Background = System.Windows.Media.Brushes.LightBlue;
			}
			if (Station[StationIndex].TempImage[1].Empty())
			{
				btn_ImgT2.Background = System.Windows.Media.Brushes.White;
			}
			else
			{
				btn_ImgT2.Background = System.Windows.Media.Brushes.LightBlue;
			}
			if (Station[StationIndex].TempImage[2].Empty())
			{
				btn_ImgT3.Background = System.Windows.Media.Brushes.White;
			}
			else
			{
				btn_ImgT3.Background = System.Windows.Media.Brushes.LightBlue;
			}
			if (ImageChannnel == 17 && !Station[StationIndex].paramFile.AIEnhanceToolON)
			{
				ChannelBtn[17].Background = System.Windows.Media.Brushes.White;
				ChannelBtn[0].Background = System.Windows.Media.Brushes.Yellow;
				ImageChannnel = 0;
			}
			if (ImageChannnel == 18 && !Station[StationIndex].paramFile.AIEnhanceToolON)
			{
				ChannelBtn[18].Background = System.Windows.Media.Brushes.White;
				ChannelBtn[0].Background = System.Windows.Media.Brushes.Yellow;
				ImageChannnel = 0;
			}
			if (chk_AIEnhanceToolON.IsChecked == true)
			{
				lab_ModelName.Content = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
			}
			else if (chk_AIPredictToolON.IsChecked == true)
			{
				lab_ModelName.Content = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
			}
			else
			{
				lab_ModelName.Content = "";
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-ToolsWindow-Btn_Stage1_Click()-1";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				System.Windows.MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
			return;
		}
		try
		{
			RunTest(AutoMode: false);
			UpdateAIImageStatus(StationIndex);
			string text2 = "";
			if (Station[StationIndex].paramFile.AIEnhanceToolON)
			{
				text2 = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
			}
			else if (Station[StationIndex].paramFile.AIPredictToolON)
			{
				text2 = Station[StationIndex].paramFile.AIPredictParam.ModelName_A;
			}
			if (!(text2 == ""))
			{
				text2 = System.IO.Path.GetFileNameWithoutExtension(text2);
				string path = string.Format("{0}TrainingImage\\{1}\\Temp\\", AppDomain.CurrentDomain.BaseDirectory, text2);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				lab_AINGTempCount.Content = files.Length.ToString();
			}
		}
		catch (Exception ex2)
		{
			string text3 = "TW01-ToolsWindow-Btn_Stage1_Click()";
			text3 += ex2.ToString();
			CVBuffer.SaveLog(text3);
			if (ShowMessage)
			{
				System.Windows.MessageBox.Show(text3, LanguageManager.Instance["Error"]);
			}
		}
	}

	public void TimeStart()
	{
		sw.Start();
	}

	private void Btn_AdvancedLocate_Click(object sender, RoutedEventArgs e)
	{
		OpenAdvancedLocate(0, 2);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckBuffer()
	{
		if (Station[StationIndex].buffers.Src.Empty())
		{
			RunTest(AutoMode: false);
			if (Station[StationIndex].buffers.Src.Empty())
			{
				txt_InspResult.Clear();
				ThreadStateStr = LanguageManager.Instance["SetImageFirst"];
				txt_InspResult.Text = ThreadStateStr;
				return false;
			}
		}
		if (!Station[StationIndex].buffers.LocatedReady)
		{
			Btn_TestAll_Click(this, null);
			if (!Station[StationIndex].buffers.LocatedReady)
			{
				txt_InspResult.Clear();
				txt_InspResult.Text = LanguageManager.Instance["LocateFail"];
				return false;
			}
		}
		if (!TestPressed)
		{
			RunTest(AutoMode: false);
		}
		return true;
	}

	private void OpenAdvancedLocate(int function, int index)
	{
		AdvLocateWindow advLocateWindow = ((ThroughputFile.CurrentClient != 8 || User != User_Permissions.USER_OP) ? new AdvLocateWindow(User) : new AdvLocateWindow(User_Permissions.USER_Engineer));
		if (!CheckBuffer())
		{
			advLocateWindow.LoadLanguage(LanguageType);
			advLocateWindow.SetInspection(function, index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			advLocateWindow.SetThroghputFile(ThroughputFile);
			advLocateWindow.SetPreviewMode(on: true);
			advLocateWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			advLocateWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			advLocateWindow.LoadLanguage(LanguageType);
			advLocateWindow.SetInspection(function, index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			advLocateWindow.SetThroghputFile(ThroughputFile);
			advLocateWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			advLocateWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			advLocateWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
	{
		LoadLanguage(LanguageType);
		CheckAuth(Showdlg: false);
	}

	private void SetAIProphetON(bool enabled)
	{
		if (enabled)
		{
			chk_AIProphetToolON.Visibility = Visibility.Visible;
			btn_AIProphet.Visibility = Visibility.Visible;
			btn_AddPTrainImage.Visibility = Visibility.Visible;
			btn_AddPTrainNGImage.Visibility = Visibility.Visible;
			lab_TimeProphetTool.Visibility = Visibility.Visible;
			chk_AIProphetToolON.IsEnabled = true;
			btn_AIProphet.IsEnabled = true;
			btn_AddPTrainImage.IsEnabled = true;
			btn_AddPTrainNGImage.IsEnabled = true;
			lab_TimeProphetTool.IsEnabled = true;
		}
		else
		{
			chk_AIProphetToolON.Visibility = Visibility.Hidden;
			btn_AIProphet.Visibility = Visibility.Hidden;
			btn_AddPTrainImage.Visibility = Visibility.Hidden;
			btn_AddPTrainNGImage.Visibility = Visibility.Hidden;
			lab_TimeProphetTool.Visibility = Visibility.Hidden;
			chk_AIProphetToolON.IsEnabled = false;
			btn_AIProphet.IsEnabled = false;
			btn_AddPTrainImage.IsEnabled = false;
			btn_AddPTrainNGImage.IsEnabled = false;
			lab_TimeProphetTool.IsEnabled = false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetAILocateON(bool enabled)
	{
		if (enabled)
		{
			if ((string)btn_ChannelExpend.Content == "<")
			{
				btn_ImgAILR.Visibility = Visibility.Visible;
				btn_ImgAILG.Visibility = Visibility.Visible;
				btn_ImgAILB.Visibility = Visibility.Visible;
			}
		}
		else
		{
			btn_ImgAILR.Visibility = Visibility.Hidden;
			btn_ImgAILG.Visibility = Visibility.Hidden;
			btn_ImgAILB.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CheckAuthNoDLG()
	{
		if (!File.Exists(string.Format("{0}OnnxRuntimeEnv.dll", AppDomain.CurrentDomain.BaseDirectory)))
		{
			InspToolAuth = false;
			return;
		}
		if (!File.Exists(string.Format("{0}ThoughtWorks.QRCode.dll", AppDomain.CurrentDomain.BaseDirectory)))
		{
			InspToolAuth = false;
			return;
		}
		string ErrorMsg = "";
		if (!Directory.Exists("C:\\Temp\\"))
		{
			Directory.CreateDirectory("C:\\Temp\\");
			return;
		}
		string text = string.Format("C:\\Temp\\{0}", "onnxruntime.dat");
		if (!File.Exists(text))
		{
			return;
		}
		AuthWindow authWindow = new AuthWindow();
		if (authWindow.CheckAuth(ref ErrorMsg, text))
		{
			btn_UnLockAuth.Visibility = Visibility.Hidden;
		}
		else
		{
			btn_UnLockAuth.Visibility = Visibility.Visible;
		}
		AuthLevel = authWindow.GetLevel();
		AuthFunStr = authWindow.GetFunctionChar();
		Authdatestr = authWindow.GetLimitDateStr();
		authWindow.GetOverTime();
		InspToolAuth = authWindow.GetAuthed();
		authWindow.GetLimitDate();
		authWindow.Close();
		if (InspToolAuth)
		{
			btn_UnLockAuth.Visibility = Visibility.Hidden;
		}
		else
		{
			btn_UnLockAuth.Visibility = Visibility.Visible;
		}
		for (int i = 1; i < 8; i++)
		{
			Station[i].paramFile.AuthLevel = AuthLevel;
		}
		if (AuthFunStr == 'B' || AuthFunStr == 'C')
		{
			SetAIProphetON(enabled: true);
			for (int j = 0; j < 8; j++)
			{
				Station[j].paramFile.AllowEnableAIProphet = true;
			}
			if (AuthFunStr == 'C')
			{
				for (int k = 0; k < 8; k++)
				{
					Station[k].paramFile.AllowEnableAILocate = true;
				}
				SetAILocateON(enabled: true);
			}
			else
			{
				SetAILocateON(enabled: false);
			}
		}
		else
		{
			SetAIProphetON(enabled: false);
			SetAILocateON(enabled: false);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CheckAuth(bool Showdlg = true)
	{
		if (!File.Exists(string.Format("{0}OnnxRuntimeEnv.dll", AppDomain.CurrentDomain.BaseDirectory)))
		{
			InspToolAuth = false;
			if (Showdlg)
			{
				System.Windows.MessageBox.Show("缺少OnnxRuntimeEnv.dll");
			}
			return;
		}
		if (!File.Exists(string.Format("{0}ThoughtWorks.QRCode.dll", AppDomain.CurrentDomain.BaseDirectory)))
		{
			InspToolAuth = false;
			if (Showdlg)
			{
				System.Windows.MessageBox.Show("缺少ThoughtWorks.QRCode.dll");
			}
			return;
		}
		string ErrorMsg = "";
		if (!Directory.Exists("C:\\Temp\\"))
		{
			Directory.CreateDirectory("C:\\Temp\\");
		}
		string encDataPath = string.Format("C:\\Temp\\{0}", "onnxruntime.dat");
		AuthWindow authWindow = new AuthWindow();
		if (authWindow.CheckAuth(ref ErrorMsg, encDataPath))
		{
			btn_UnLockAuth.Visibility = Visibility.Hidden;
		}
		else
		{
			btn_UnLockAuth.Visibility = Visibility.Visible;
			if (Showdlg)
			{
				authWindow.ShowDialog();
			}
		}
		AuthLevel = authWindow.GetLevel();
		AuthFunStr = authWindow.GetFunctionChar();
		Authdatestr = authWindow.GetLimitDateStr();
		authWindow.GetOverTime();
		InspToolAuth = authWindow.GetAuthed();
		authWindow.GetLimitDate();
		authWindow.Close();
		if (InspToolAuth)
		{
			btn_UnLockAuth.Visibility = Visibility.Hidden;
		}
		else
		{
			btn_UnLockAuth.Visibility = Visibility.Visible;
		}
		for (int i = 1; i < 8; i++)
		{
			Station[i].paramFile.AuthLevel = AuthLevel;
		}
		if (AuthFunStr == 'B' || AuthFunStr == 'C')
		{
			SetAIProphetON(enabled: true);
			for (int j = 0; j < 8; j++)
			{
				Station[j].paramFile.AllowEnableAIProphet = true;
			}
			if (AuthFunStr == 'C')
			{
				for (int k = 0; k < 8; k++)
				{
					Station[k].paramFile.AllowEnableAILocate = true;
				}
				SetAILocateON(enabled: true);
			}
			else
			{
				SetAILocateON(enabled: false);
			}
		}
		else
		{
			SetAIProphetON(enabled: false);
			SetAILocateON(enabled: false);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void OpenBitmap()
	{
		Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
		openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.tif;*.png)|*.bmp;*.jpg;*.tif;*.png|All files (*.*)|*.*";
		if (openFileDialog.ShowDialog() == true)
		{
			Mat mat = CVBuffer.ImRead(openFileDialog.FileName);
			if (mat.Channels() == 4)
			{
				Cv2.CvtColor(mat, mat, ColorConversionCodes.BGRA2BGR);
			}
			Station[StationIndex].SetBuffers(mat);
			Mat mat2 = Station[StationIndex].buffers.Src.Clone();
			DrawImage(mat2);
			RuncountText = string.Format("{0} / {1}", 1, 1);
			btn_ImageNum.Content = RuncountText;
			ImageListSource = 0;
		}
	}

	public double TimeStop()
	{
		sw.Stop();
		double totalMilliseconds = sw.Elapsed.TotalMilliseconds;
		sw.Reset();
		return totalMilliseconds;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_PrevImage_Click(object sender, RoutedEventArgs e)
	{
		int stationIndex = StationIndex;
		try
		{
			switch (ImageListSource)
			{
			case 0:
				return;
			case 1:
			{
				if (filelist[stationIndex] == null)
				{
					ShowNoContent();
					return;
				}
				if (filelist[stationIndex].Length == 0)
				{
					ShowNoContent();
					return;
				}
				if (fileIndex - 1 >= 0)
				{
					fileIndex--;
				}
				else
				{
					fileIndex = filelist[stationIndex].Length - 1;
				}
				RuncountText = string.Format("{0} / {1}", fileIndex + 1, filelist[stationIndex].Length);
				btn_ImageNum.Content = RuncountText;
				string text = LanguageManager.Instance["FileNotFound"];
				if (!File.Exists(filelist[stationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[stationIndex][fileIndex] + text);
					return;
				}
				if (!File.Exists(filelist[stationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[stationIndex][fileIndex] + text);
					return;
				}
				Mat buffers = CVBuffer.ImRead(filelist[stationIndex][fileIndex]);
				lab_FileName.Content = System.IO.Path.GetFileName(filelist[stationIndex][fileIndex]);
				Station[stationIndex].SetBuffers(buffers);
				break;
			}
			case 2:
				if (Station[stationIndex].RecentBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].RecentBufferList.Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (RecentBufferIndex[stationIndex] - 1 >= 0)
				{
					RecentBufferIndex[stationIndex]--;
				}
				else
				{
					RecentBufferIndex[stationIndex] = Station[stationIndex].RecentBufferList.Count - 1;
				}
				RuncountText = string.Format("{0} / {1}", RecentBufferIndex[stationIndex] + 1, Station[stationIndex].RecentBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].RecentBufferList[RecentBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				if (Station[stationIndex].RecentBufferIDList != null && RecentBufferIndex[stationIndex] < Station[stationIndex].RecentBufferIDList.Count)
				{
					lab_FileName.Content = Station[stationIndex].RecentBufferIDList[RecentBufferIndex[stationIndex]];
				}
				break;
			case 3:
				if (Station[stationIndex].OKBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].OKBufferList.Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (OKBufferIndex[stationIndex] - 1 >= 0)
				{
					OKBufferIndex[stationIndex]--;
				}
				else
				{
					OKBufferIndex[stationIndex] = Station[stationIndex].OKBufferList.Count - 1;
				}
				RuncountText = string.Format("{0} / {1}", OKBufferIndex[stationIndex] + 1, Station[stationIndex].OKBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].OKBufferList[OKBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 4:
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect] == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (NGBufferIndex[stationIndex] - 1 >= 0)
				{
					NGBufferIndex[stationIndex]--;
				}
				else
				{
					NGBufferIndex[stationIndex] = Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count - 1;
				}
				RuncountText = string.Format("{0} / {1}", NGBufferIndex[stationIndex] + 1, Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 5:
				if (Station[stationIndex].RetestBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].RetestBufferList.Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (RetestBufferIndex[stationIndex] - 1 >= 0)
				{
					RetestBufferIndex[stationIndex]--;
				}
				else
				{
					RetestBufferIndex[stationIndex] = Station[stationIndex].RetestBufferList.Count - 1;
				}
				RuncountText = string.Format("{0} / {1}", RetestBufferIndex[stationIndex] + 1, Station[stationIndex].RetestBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].RetestBufferList[RetestBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			}
		}
		catch
		{
			CVBuffer.SaveLog(string.Format("TW01-ToolsWindow-Btn_PrevImage_Click()- Station={0}, ImageListSource{1]", StationIndex, ImageListSource));
			return;
		}
		Inspect();
		UpdateInspTime();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NextImage_Click(object sender, RoutedEventArgs e)
	{
		int stationIndex = StationIndex;
		try
		{
			switch (ImageListSource)
			{
			case 1:
			{
				if (filelist[stationIndex] == null)
				{
					ShowNoContent();
					return;
				}
				if (filelist[stationIndex].Length == 0)
				{
					ShowNoContent();
					return;
				}
				if (fileIndex + 1 < filelist[stationIndex].Length)
				{
					fileIndex++;
				}
				else
				{
					fileIndex = 0;
				}
				RuncountText = string.Format("{0} / {1}", fileIndex + 1, filelist[stationIndex].Length);
				btn_ImageNum.Content = RuncountText;
				string text = LanguageManager.Instance["FileNotFound"];
				if (!File.Exists(filelist[stationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[stationIndex][fileIndex] + text);
					return;
				}
				if (!File.Exists(filelist[stationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[stationIndex][fileIndex] + text);
					return;
				}
				Mat buffers = CVBuffer.ImRead(filelist[stationIndex][fileIndex]);
				lab_FileName.Content = System.IO.Path.GetFileName(filelist[stationIndex][fileIndex]);
				Station[stationIndex].SetBuffers(buffers);
				break;
			}
			case 2:
				if (Station[stationIndex].RecentBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].RecentBufferList.Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (RecentBufferIndex[stationIndex] + 1 < Station[stationIndex].RecentBufferList.Count)
				{
					RecentBufferIndex[stationIndex]++;
				}
				else
				{
					RecentBufferIndex[stationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", RecentBufferIndex[stationIndex] + 1, Station[stationIndex].RecentBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].RecentBufferList[RecentBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				if (Station[stationIndex].RecentBufferIDList != null && RecentBufferIndex[stationIndex] < Station[stationIndex].RecentBufferIDList.Count)
				{
					lab_FileName.Content = Station[stationIndex].RecentBufferIDList[RecentBufferIndex[stationIndex]];
				}
				break;
			case 3:
				if (Station[stationIndex].OKBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].OKBufferList.Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (OKBufferIndex[stationIndex] + 1 < Station[stationIndex].OKBufferList.Count)
				{
					OKBufferIndex[stationIndex]++;
				}
				else
				{
					OKBufferIndex[stationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", OKBufferIndex[stationIndex] + 1, Station[stationIndex].OKBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].OKBufferList[OKBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 4:
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect] == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (NGBufferIndex[stationIndex] + 1 < Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count)
				{
					NGBufferIndex[stationIndex]++;
				}
				else
				{
					NGBufferIndex[stationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", NGBufferIndex[stationIndex] + 1, Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 5:
				if (Station[stationIndex].RetestBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[stationIndex].RetestBufferList.Count <= 0)
				{
					ShowNoContent();
					return;
				}
				if (RetestBufferIndex[stationIndex] + 1 < Station[stationIndex].RetestBufferList.Count)
				{
					RetestBufferIndex[stationIndex]++;
				}
				else
				{
					RetestBufferIndex[stationIndex] = 0;
				}
				RuncountText = string.Format("{0} / {1}", RetestBufferIndex[stationIndex] + 1, Station[stationIndex].RetestBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				Station[stationIndex].SetBuffers(Station[stationIndex].RetestBufferList[RetestBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 0:
				break;
			}
		}
		catch
		{
			CVBuffer.SaveLog(string.Format("TW01-ToolsWindow-Btn_NextImage_Click()- Station={0}, ImageListSource{1]", StationIndex, ImageListSource));
			return;
		}
		Inspect();
		UpdateInspTime();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateInspTime()
	{
		double num = 0.0;
		for (int i = 0; i < 18; i++)
		{
			TimeLabel[i].Content = string.Format("{0:N3}", Station[StationIndex].InspTimePerTool[i]);
			num += Station[StationIndex].InspTimePerTool[i];
		}
		lab_TotalTime.Content = string.Format("{0:N3}", num);
	}

	private void OpenFileList()
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (folderSelectDialog.ShowDialog())
		{
			CheckFileList(folderSelectDialog.FileName);
		}
		else
		{
			ShowNoContent();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CheckFileList(string path)
	{
		try
		{
			if (path == "" || path == null)
			{
				return;
			}
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				if (!Directory.Exists(path))
				{
					return;
				}
			}
			filelist[StationIndex] = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			if (filelist[StationIndex].Length == 0)
			{
				ShowNoContent();
				return;
			}
			lab_ParetoNGSelect.Visibility = Visibility.Hidden;
			cb_ParetoNGSelect.Visibility = Visibility.Hidden;
			ImageListSource = 1;
			OKRatio = (NGRatio = 0.0);
			OKCount = (NGCount = 0);
			txt_NGCount.Text = "0";
			txt_OKCount.Text = "0";
			txt_OKCountRatio.Text = "0%";
			txt_NGCountRatio.Text = "0%";
			for (int i = 0; i < filelist[StationIndex].Length; i++)
			{
				string extension = System.IO.Path.GetExtension(filelist[StationIndex][i]);
				extension = extension.ToLower();
				if (extension != ".bmp" && extension != ".jpg" && extension != ".tif" && extension != ".png")
				{
					filelist[StationIndex] = filelist[StationIndex].Take(i).Concat(filelist[StationIndex].Skip(i + 1)).ToArray();
					i--;
				}
			}
			if (filelist[StationIndex].Length == 0)
			{
				ShowNoContent();
				return;
			}
			if (filelist[StationIndex].Length != 0)
			{
				fileIndex = 0;
				RuncountText = string.Format("{0} / {1}", 1, filelist[StationIndex].Length);
				btn_ImageNum.Content = RuncountText;
			}
			string text = LanguageManager.Instance["FileNotFound"];
			if (!File.Exists(filelist[StationIndex][fileIndex]))
			{
				System.Windows.MessageBox.Show(filelist[StationIndex][fileIndex] + text);
				return;
			}
			Mat buffers = CVBuffer.ImRead(filelist[StationIndex][fileIndex]);
			Station[StationIndex].SetBuffers(buffers);
			Inspect();
		}
		catch (Exception ex)
		{
			string text2 = "TW01-ToolsWindow-CheckFileList()";
			text2 += ex.ToString();
			CVBuffer.SaveLog(text2);
			if (ShowMessage)
			{
				System.Windows.MessageBox.Show(text2, LanguageManager.Instance["Error"]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TestAllImage_Click(object sender, RoutedEventArgs e)
	{
		if (timer_state == 0)
		{
			OKCount = 0;
			NGCount = 0;
			OKRatio = 0.0;
			NGRatio = 0.0;
			timer_state = 1;
			btn_TestAllImage.Background = System.Windows.Media.Brushes.Yellow;
			ThreadStateStr = LanguageManager.Instance["AutorunRunning"];
			lab_AutestState.Content = ThreadStateStr;
			AutoTestPauseEvent.Set();
			AutoTestUIPauseEvent.Set();
		}
		else if (timer_state == 1)
		{
			base.Cursor = System.Windows.Input.Cursors.Arrow;
			AutoTestPauseEvent.Reset();
			AutoTestUIPauseEvent.Reset();
			timer_state = 2;
			ThreadStateStr = LanguageManager.Instance["AutorunPause"];
			lab_AutestState.Content = ThreadStateStr;
			txt_OKCount.Text = OKCount.ToString();
			txt_OKCountRatio.Text = string.Format("{0:P}", OKRatio);
			txt_NGCount.Text = NGCount.ToString();
			txt_NGCountRatio.Text = string.Format("{0:P}", NGRatio);
			btn_ImageNum.Content = RuncountText;
		}
		else if (timer_state == 2)
		{
			timer_state = 1;
			AutoTestPauseEvent.Set();
			AutoTestUIPauseEvent.Set();
			ThreadStateStr = LanguageManager.Instance["AutorunRunning"];
			lab_AutestState.Content = ThreadStateStr;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_UseRecentBufList_Click(object sender, RoutedEventArgs e)
	{
		if (timer_state == 1)
		{
			base.Cursor = System.Windows.Input.Cursors.Arrow;
			AutoTestPauseEvent.Reset();
			AutoTestUIPauseEvent.Reset();
			timer_state = 2;
			ThreadStateStr = LanguageManager.Instance["AutorunPause"];
			lab_AutestState.Content = ThreadStateStr;
			txt_OKCount.Text = OKCount.ToString();
			txt_OKCountRatio.Text = string.Format("{0:P}", OKRatio);
			txt_NGCount.Text = NGCount.ToString();
			txt_NGCountRatio.Text = string.Format("{0:P}", NGRatio);
			btn_ImageNum.Content = RuncountText;
		}
		System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
		btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseFileBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
		button.Background = System.Windows.Media.Brushes.Yellow;
		ImageListSource = Convert.ToInt32(button.Uid);
		cb_ParetoNGSelect.Visibility = Visibility.Hidden;
		lab_ParetoNGSelect.Visibility = Visibility.Hidden;
		btn_DelAIOKDB.Visibility = Visibility.Hidden;
		string uid = button.Uid;
		if (!(uid == "0"))
		{
			if (!(uid == "1"))
			{
				if (!(uid == "2"))
				{
					if (!(uid == "3"))
					{
						if (!(uid == "4"))
						{
							if (uid == "5")
							{
								OpenBufferList();
							}
						}
						else
						{
							OpenBufferList();
						}
					}
					else
					{
						OpenBufferList();
					}
				}
				else
				{
					OpenBufferList();
				}
			}
			else
			{
				OpenFileList();
			}
		}
		else
		{
			OpenBitmap();
		}
	}

	private void WriteToolONStatus()
	{
		UpdaeUI = false;
		Station[StationIndex].paramFile.LocateToolON = chk_LocateToolON.IsChecked.Value;
		Station[StationIndex].paramFile.SelectWayON = chk_SelectWayON.IsChecked.Value;
		Station[StationIndex].paramFile.SelectFaceON = chk_SelectFaceON.IsChecked.Value;
		Station[StationIndex].paramFile.AdvLocateToolON = true;
		Station[StationIndex].paramFile.MeasureToolON = chk_MeasureToolON.IsChecked.Value;
		Station[StationIndex].paramFile.EnahnceToolON = chk_EnhanceToolON.IsChecked.Value;
		Station[StationIndex].paramFile.MaskToolON = true;
		Station[StationIndex].paramFile.BinaryToolON = chk_BinaryToolON.IsChecked.Value;
		Station[StationIndex].paramFile.SmoothToolON = chk_SmoothToolON.IsChecked.Value;
		Station[StationIndex].paramFile.DifferenceToolON = chk_DifferenceToolON.IsChecked.Value;
		Station[StationIndex].paramFile.BlockAvgToolON = chk_BlockAvgToolON.IsChecked.Value;
		Station[StationIndex].paramFile.LineAnalysisToolON = chk_LineAnalysisON.IsChecked.Value;
		Station[StationIndex].paramFile.MatchToolON = chk_MatchToolON.IsChecked.Value;
		Station[StationIndex].paramFile.AIProphetToolON = chk_AIProphetToolON.IsChecked.Value;
		if (Station[StationIndex].paramFile.AIProphetToolON)
		{
			btn_AddPTrainImage.IsEnabled = true;
			btn_AddPTrainNGImage.IsEnabled = true;
		}
		else
		{
			btn_AddPTrainImage.IsEnabled = false;
			btn_AddPTrainNGImage.IsEnabled = false;
		}
		Station[StationIndex].paramFile.AIEnhanceToolON = chk_AIEnhanceToolON.IsChecked.Value;
		Station[StationIndex].paramFile.AIPredictToolON = chk_AIPredictToolON.IsChecked.Value;
		Station[StationIndex].paramFile.FontToolON = chk_FontToolON.IsChecked.Value;
		Station[StationIndex].paramFile.IRToolON = chk_IRToolON.IsChecked.Value;
		if (Station[StationIndex].paramFile.FontToolON && !LowLevelVersion)
		{
			lab_FontToolText.Visibility = Visibility.Visible;
		}
		else
		{
			lab_FontToolText.Visibility = Visibility.Hidden;
		}
		ShowAIEnhanceChannel(Station[StationIndex].paramFile.AIEnhanceToolON || Station[StationIndex].paramFile.AIPredictToolON);
		UpdateAIImageStatus(StationIndex);
		UpdaeUI = true;
	}

	private void ReadToolONStatus()
	{
		UpdaeUI = false;
		chk_LocateToolON.IsChecked = Station[StationIndex].paramFile.LocateToolON;
		chk_SelectWayON.IsChecked = Station[StationIndex].paramFile.SelectWayON;
		chk_SelectFaceON.IsChecked = Station[StationIndex].paramFile.SelectFaceON;
		chk_AdvLocateToolON.IsChecked = true;
		chk_MeasureToolON.IsChecked = Station[StationIndex].paramFile.MeasureToolON;
		chk_EnhanceToolON.IsChecked = Station[StationIndex].paramFile.EnahnceToolON;
		chk_MaskToolON.IsChecked = true;
		chk_BinaryToolON.IsChecked = Station[StationIndex].paramFile.BinaryToolON;
		chk_SmoothToolON.IsChecked = Station[StationIndex].paramFile.SmoothToolON;
		chk_DifferenceToolON.IsChecked = Station[StationIndex].paramFile.DifferenceToolON;
		chk_BlockAvgToolON.IsChecked = Station[StationIndex].paramFile.BlockAvgToolON;
		chk_LineAnalysisON.IsChecked = Station[StationIndex].paramFile.LineAnalysisToolON;
		chk_MatchToolON.IsChecked = Station[StationIndex].paramFile.MatchToolON;
		chk_AIProphetToolON.IsChecked = Station[StationIndex].paramFile.AIProphetToolON;
		if (Station[StationIndex].paramFile.AIProphetToolON)
		{
			btn_AddPTrainImage.IsEnabled = true;
			btn_AddPTrainNGImage.IsEnabled = true;
		}
		else
		{
			btn_AddPTrainImage.IsEnabled = false;
			btn_AddPTrainNGImage.IsEnabled = false;
		}
		chk_AIEnhanceToolON.IsChecked = Station[StationIndex].paramFile.AIEnhanceToolON;
		chk_AIPredictToolON.IsChecked = Station[StationIndex].paramFile.AIPredictToolON;
		chk_FontToolON.IsChecked = Station[StationIndex].paramFile.FontToolON;
		chk_IRToolON.IsChecked = Station[StationIndex].paramFile.IRToolON;
		if (Station[StationIndex].paramFile.FontToolON && !LowLevelVersion)
		{
			lab_FontToolText.Content = Station[StationIndex].paramFile.FontToolParam.AllFont;
			lab_FontToolText.Visibility = Visibility.Visible;
		}
		else
		{
			lab_FontToolText.Visibility = Visibility.Hidden;
		}
		if (Station[StationIndex].paramFile.AIPredictToolON)
		{
			UsePredictEnhance = AI_MODE.EM_Predict;
		}
		else if (Station[StationIndex].paramFile.AIEnhanceToolON)
		{
			UsePredictEnhance = AI_MODE.EM_Enhance;
		}
		else
		{
			UsePredictEnhance = AI_MODE.EM_NoUse;
		}
		ShowAIEnhanceChannel(Station[StationIndex].paramFile.AIEnhanceToolON || Station[StationIndex].paramFile.AIPredictToolON);
		if (Station[StationIndex].paramFile.AIEnhanceToolON)
		{
			exp_PreviewAI.Visibility = Visibility.Visible;
		}
		else
		{
			exp_PreviewAI.Visibility = Visibility.Hidden;
		}
		if (Station[StationIndex].paramFile.AIProphetToolON)
		{
			exp_PreviewAIHeatMap.Visibility = Visibility.Visible;
		}
		else
		{
			exp_PreviewAIHeatMap.Visibility = Visibility.Hidden;
			btn_AutoMarked.Visibility = Visibility.Hidden;
		}
		if (!Station[StationIndex].AIDLLExist)
		{
			chk_AIEnhanceToolON.IsEnabled = false;
			btn_AIEnhanceTool.IsEnabled = false;
			chk_AIPredictToolON.IsEnabled = false;
			btn_AIPredictTool.IsEnabled = false;
		}
		if ((Station[StationIndex].paramFile.AIEnhanceToolON || Station[StationIndex].paramFile.AIPredictToolON) && !LowLevelVersion)
		{
			grd_AIFunction.Visibility = Visibility.Visible;
			btn_AIOKDB.Visibility = Visibility.Visible;
			btn_AINGDB.Visibility = Visibility.Visible;
		}
		btn_ImgAI.IsEnabled = Station[StationIndex].AIDLLExist;
		btn_ImgAI2.IsEnabled = Station[StationIndex].AIDLLExist;
		for (int i = 0; i < 8; i++)
		{
			OKPath[i] = Station[i].paramFile.OKPath;
			NGPath[i] = Station[i].paramFile.NGPath;
		}
		UpdateAIImageStatus(StationIndex);
		if (Station[StationIndex].paramFile.SoftCorrectionON && Station[StationIndex].paramFile.LightMonitorParam.EnableSoftCorrection)
		{
			btn_LightMonitorTool.Background = System.Windows.Media.Brushes.Gold;
		}
		else
		{
			btn_LightMonitorTool.Background = System.Windows.Media.Brushes.White;
		}
		UpdaeUI = true;
	}

	private void ShowAIEnhanceChannel(bool show)
	{
		if (show)
		{
			btn_ImgAI.Visibility = Visibility.Visible;
			btn_ImgAI2.Visibility = Visibility.Visible;
			btn_ImgAIR.Visibility = Visibility.Visible;
			btn_ImgAIG.Visibility = Visibility.Visible;
			btn_ImgAIB.Visibility = Visibility.Visible;
		}
		else
		{
			btn_ImgAI.Visibility = Visibility.Hidden;
			btn_ImgAI2.Visibility = Visibility.Hidden;
			btn_ImgAIR.Visibility = Visibility.Hidden;
			btn_ImgAIG.Visibility = Visibility.Hidden;
			btn_ImgAIB.Visibility = Visibility.Hidden;
		}
	}

	public void SaveAllRecipe()
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.WriteAllParam();
		}
		if (ThroughputINI != null)
		{
			ThroughputFile.WriteParetoItems();
		}
		WritePublicDBParam();
		WritePrivateDBParam();
		WriteMatchQuickSetupParam();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		UpdateUITImer.Stop();
		lab_TrainingProcess.Visibility = Visibility.Hidden;
		pb_TrainingProcess.Visibility = Visibility.Hidden;
		chk_TestEnhanceEnabled.IsChecked = false;
		for (int i = 0; i < 8; i++)
		{
			if (Station[i] != null)
			{
				Station[i].locateTool.OffLineTestEnhance = false;
			}
		}
		if (!base.IsVisible)
		{
			return;
		}
		chk_SaveAI.IsChecked = false;
		for (int j = 0; j < 8; j++)
		{
			if (Station[j] != null)
			{
				Station[j].SetSaveAIImage(status: false);
			}
		}
		chk_SaveFontAI.IsChecked = false;
		chk_SaveFontAI2.IsChecked = false;
		for (int k = 0; k < 8; k++)
		{
			if (Station[k] != null)
			{
				Station[k].paramFile.SaveAIFontImage = false;
			}
		}
		SetSaveAIImage(status: false);
		WriteToolONStatus();
		TestPressed = false;
		Hide();
		RecipeSaveFinishEvent.Reset();
		RecipeSavePauseEvent.Set();
		e.Cancel = true;
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		RecipeSaveFinishEvent.WaitOne(60000);
		AutoTestShutdownEvent.Set();
		AutoTestPauseEvent.Set();
		AutoTestUIShutdownEvent.Set();
		AutoTestUIPauseEvent.Set();
		ForceClose = true;
		AILoadingPauseEvent.Set();
		AIAutoDrawPauseEvent.Set();
		RecipeSavePauseEvent.Set();
		Thread.Sleep(100);
	}

	private void Btn_Hide_Click(object sender, RoutedEventArgs e)
	{
		TestPressed = false;
		Hide();
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.WriteAllParam();
		}
	}

	private void Btn_MeasureTool_Click(object sender, RoutedEventArgs e)
	{
		OpenMeasureTool(0, 0);
	}

	private void OpenMeasureTool(int function, int index)
	{
		if (!CheckBuffer())
		{
			MeasureWindow measureWindow = new MeasureWindow(User);
			measureWindow.LoadLanguage(LanguageType);
			measureWindow.SetInspection(function, index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			measureWindow.SetThroghputFile(ThroughputFile);
			measureWindow.SetPreviewMode(on: true);
			measureWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			measureWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
			return;
		}
		if (!TestPressed)
		{
			RunTest(AutoMode: false);
		}
		MeasureWindow measureWindow2 = new MeasureWindow(User);
		measureWindow2.LoadLanguage(LanguageType);
		measureWindow2.SetInspection(function, index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
		measureWindow2.SetThroghputFile(ThroughputFile);
		measureWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
		measureWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
		measureWindow2.ShowDialog();
		Btn_TestAll_Click(this, null);
	}

	private void Btn_SmoothTool_Click(object sender, RoutedEventArgs e)
	{
		OpenSmoothTool(0);
	}

	private void OpenSmoothTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			SmoothWindow smoothWindow = new SmoothWindow(User);
			smoothWindow.LoadLanguage(LanguageType);
			smoothWindow.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			smoothWindow.SetThroghputFile(ThroughputFile);
			smoothWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			smoothWindow.SetPreviewMode(on: true);
			smoothWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			SmoothWindow smoothWindow2 = new SmoothWindow(User);
			smoothWindow2.LoadLanguage(LanguageType);
			smoothWindow2.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			smoothWindow2.SetThroghputFile(ThroughputFile);
			smoothWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			smoothWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			smoothWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			smoothWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_BitmapSave_Click(object sender, RoutedEventArgs e)
	{
		GetOBJSizeStatistics();
		if (!Station[StationIndex].buffers.SrcReady)
		{
			return;
		}
		try
		{
			Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
			saveFileDialog.Filter = "Image files (*.png;)|*.png;|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() != true)
			{
				return;
			}
			if (!Directory.Exists(System.IO.Path.GetDirectoryName(saveFileDialog.FileName)))
			{
				if (ShowMessage)
				{
					System.Windows.MessageBox.Show(LanguageManager.Instance["Error"], LanguageManager.Instance["Error"]);
				}
			}
			else
			{
				Station[StationIndex].buffers.Src.SaveImage(saveFileDialog.FileName);
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-ToolsWindow-Btn_BitmapSave_Click()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				System.Windows.MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckBufferAndRef()
	{
		if (Station[StationIndex].buffers.Src.Empty())
		{
			RunTest(AutoMode: false);
			if (Station[StationIndex].buffers.Src.Empty())
			{
				txt_InspResult.Clear();
				ThreadStateStr = LanguageManager.Instance["SetImageFirst"];
				txt_InspResult.Text = ThreadStateStr;
				return false;
			}
		}
		if (!Station[StationIndex].buffers.LocatedReady)
		{
			Btn_TestAll_Click(this, null);
			if (!Station[StationIndex].buffers.LocatedReady)
			{
				txt_InspResult.Clear();
				txt_InspResult.Text = LanguageManager.Instance["LocateFail"];
				return false;
			}
		}
		if (!Station[StationIndex].AdvlocateResult)
		{
			txt_InspResult.Clear();
			txt_InspResult.Text = LanguageManager.Instance["AdvLocateFail"];
			return false;
		}
		if (!TestPressed)
		{
			RunTest(AutoMode: false);
		}
		return true;
	}

	private void Btn_MaskTool_Click(object sender, RoutedEventArgs e)
	{
		OpenMaskTool(0);
	}

	private void OpenMaskTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			MaskWindow maskWindow = new MaskWindow();
			maskWindow.LoadLanguage(LanguageType);
			maskWindow.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			maskWindow.SetPreviewMode(on: true);
			maskWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			MaskWindow maskWindow2 = new MaskWindow();
			maskWindow2.LoadLanguage(LanguageType);
			maskWindow2.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			maskWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			maskWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			maskWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cnv_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		try
		{
			if (Station[StationIndex].NGRect.Count <= 0 || (ThroughputFile.CurrentClient == 7 && User == User_Permissions.USER_OP))
			{
				return;
			}
			System.Windows.Point position = e.GetPosition(this);
			if (xRatio[StationIndex] == 0.0 || yRatio[StationIndex] == 0.0)
			{
				return;
			}
			double num = position.X - cnv_Image.Margin.Left;
			double num2 = position.Y - cnv_Image.Margin.Top;
			int x = (int)(num / xRatio[StationIndex]);
			int y = (int)(num2 / yRatio[StationIndex]);
			OpenCvSharp.Rect locateImageRect = Station[StationIndex].LocateImageRect;
			for (int i = 0; i < Station[StationIndex].NGRect.Count; i++)
			{
				if (new OpenCvSharp.Rect
				{
					X = locateImageRect.X + Station[StationIndex].NGRect[i].X,
					Y = locateImageRect.Y + Station[StationIndex].NGRect[i].Y,
					Width = Station[StationIndex].NGRect[i].Width,
					Height = Station[StationIndex].NGRect[i].Height
				}.Contains(x, y))
				{
					QuickOpenTools(Station[StationIndex].NGTool[i], Station[StationIndex].NGToolIndex[i]);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-ToolsWindow-Cnv_Image_PreviewMouseLeftButtonDown()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				System.Windows.MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	private void QuickOpenTools(InspToolsFunciton tools, int index)
	{
		switch (tools)
		{
		case InspToolsFunciton.EM_ADVLOCATE_V:
			OpenAdvancedLocate(0, index);
			break;
		case InspToolsFunciton.EM_ADVLOCATE_H:
			OpenAdvancedLocate(1, index);
			break;
		case InspToolsFunciton.EM_ADVLOCATE_A:
			OpenAdvancedLocate(2, index);
			break;
		case InspToolsFunciton.EM_ADVLOCATE_M:
			OpenAdvancedLocate(3, index);
			break;
		case InspToolsFunciton.EM_MEASURE_W:
			OpenMeasureTool(0, index);
			break;
		case InspToolsFunciton.EM_MEASURE_H:
			OpenMeasureTool(1, index);
			break;
		case InspToolsFunciton.EM_BINARY:
			OpenBinaryTool(index);
			break;
		case InspToolsFunciton.EM_SMOOTH:
			OpenSmoothTool(index);
			break;
		case InspToolsFunciton.EM_BLOCKAVG:
			OpenBlockAvgTool(index);
			break;
		case InspToolsFunciton.EM_DIFFERENCE:
			OpenDifferenceTool(index);
			break;
		case InspToolsFunciton.EM_LINEANALYSIS:
			OpenLineAnalysisTool(index);
			break;
		case InspToolsFunciton.EM_MATCH:
			OpenMatchTool(index);
			break;
		case InspToolsFunciton.EM_FONT:
			OpenFontTool();
			break;
		case InspToolsFunciton.EM_AIPREDICT:
			break;
		}
	}

	private void Btn_EnhanceTool_Click(object sender, RoutedEventArgs e)
	{
		if (CheckBuffer())
		{
			EnhanceWindow enhanceWindow = new EnhanceWindow();
			enhanceWindow.LoadLanguage(LanguageType);
			enhanceWindow.SetInspection(0, Station[StationIndex]);
			enhanceWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			enhanceWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImgSrc_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].buffers.LocatedReady)
		{
			Btn_TestAll_Click(this, null);
		}
		if (!Station[StationIndex].buffers.LocatedReady)
		{
			return;
		}
		try
		{
			System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
			for (int i = 0; i < 22; i++)
			{
				ChannelBtn[i].Background = System.Windows.Media.Brushes.White;
			}
			lab_Channel.Content = button.Content;
			ImageChannnel = Convert.ToInt32(button.Uid);
			ChannelBtn[ImageChannnel].Background = System.Windows.Media.Brushes.LightGreen;
			Mat mat = new Mat();
			if (ImageChannnel != 0)
			{
				if ((ImageChannnel == 17 || ImageChannnel == 18) && (Station[StationIndex].paramFile.AIEnhanceToolON || Station[StationIndex].paramFile.AIPredictToolON))
				{
					mat = Station[StationIndex].buffers.Dst.Clone();
					using Mat mat2 = Station[StationIndex].buffers.GetEnhanceAiMatColor().Clone();
					using Mat m = new Mat(mat, Station[StationIndex].buffers.LocateImageRect);
					if (mat2.Width > 0 && mat2.Height > 0)
					{
						if (Station[StationIndex].paramFile.AIEnhanceParam.DnetMode_A == 1 || Station[StationIndex].paramFile.AIEnhanceParam.DnetMode_A == 2)
						{
							Mat mat3 = new Mat();
							Cv2.CvtColor(mat2, mat3, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat3, 20.0, 255.0, mat3);
							if (ImageChannnel == 18)
							{
								Cv2.FindContours(mat3, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
								Mat mat4 = mat2.Clone();
								mat4.SetTo(0.0);
								for (int j = 0; j < contours.Length; j++)
								{
									Cv2.DrawContours(mat4, contours, j, Scalar.White, 3);
								}
								Cv2.CvtColor(mat4, mat4, ColorConversionCodes.BGR2GRAY);
								mat3.SetTo(0.0);
								mat4.CopyTo(mat3);
							}
							mat2.CopyTo(m, mat3);
						}
						else
						{
							mat2.CopyTo(m);
						}
					}
				}
				else if (ImageChannnel >= 19)
				{
					mat = Station[StationIndex].buffers.Gray.Clone();
					using Mat mat5 = Station[StationIndex].buffers.GetAILocateMat(ImageChannnel - 1 + 3).Clone();
					using Mat m2 = new Mat(mat, Station[StationIndex].buffers.LocateImageRect);
					if (mat5.Width > 0 && mat5.Height > 0)
					{
						mat5.CopyTo(m2);
					}
					mat5.CopyTo(mat);
				}
				else
				{
					mat = Station[StationIndex].buffers.Gray.Clone();
					using Mat mat6 = Station[StationIndex].buffers.GetChannelMat(ImageChannnel - 1).Clone();
					using Mat m3 = new Mat(mat, Station[StationIndex].buffers.LocateImageRect);
					if (mat6.Width > 0 && mat6.Height > 0)
					{
						mat6.CopyTo(m3);
					}
				}
			}
			else
			{
				mat = Station[StationIndex].buffers.Src.Clone();
			}
			OldDst = mat;
			ImageSource imageSource = Station[StationIndex].buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
		}
		catch (Exception ex)
		{
			string text = "TW01-ToolsWindow-Btn_ImgSrc_Click()";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			if (ShowMessage)
			{
				System.Windows.MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	private void Ts_StationON_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Btn_ParamCopy_Click(object sender, RoutedEventArgs e)
	{
		if (User < User_Permissions.USER_Engineer)
		{
			return;
		}
		ParamCopyWindow paramCopyWindow = new ParamCopyWindow(User);
		paramCopyWindow.LoadLanguage(LanguageType);
		paramCopyWindow.SetStation(Station);
		paramCopyWindow.ShowDialog();
		if (paramCopyWindow.UpdateComDigits)
		{
			ThroughputFile.SetComDigis(paramCopyWindow.ComDigits, paramCopyWindow.ComDigitsName);
		}
		if (paramCopyWindow.ReloadAI)
		{
			for (int i = 0; i < 8; i++)
			{
				LoadingAIModel[i] = true;
			}
			grd_AILoading.Visibility = Visibility.Visible;
			AILoadingPauseEvent.Set();
		}
	}

	private void Ts_LocateToolON_Unchecked(object sender, RoutedEventArgs e)
	{
		if (inited && UpdaeUI)
		{
			int num = Convert.ToInt32(((System.Windows.Controls.CheckBox)sender).Uid);
			if (num == 14)
			{
				exp_PreviewAI.Visibility = Visibility.Hidden;
				grd_AINGQuickDraw.Visibility = Visibility.Hidden;
			}
			if (num == 17)
			{
				exp_PreviewAIHeatMap.Visibility = Visibility.Hidden;
			}
			WriteToolONStatus();
		}
	}

	private void Ts_LocateToolON_Checked(object sender, RoutedEventArgs e)
	{
		if (!inited || !UpdaeUI)
		{
			return;
		}
		int num = Convert.ToInt32(((System.Windows.Controls.CheckBox)sender).Uid);
		ToolsBtn[num].IsEnabled = true;
		if (num == 13 || num == 14 || num == 17)
		{
			switch (num)
			{
			case 13:
				chk_AIEnhanceToolON.IsChecked = false;
				UsePredictEnhance = AI_MODE.EM_Predict;
				Station[StationIndex].paramFile.AIPredictToolON = true;
				LoadingAIModel[StationIndex] = true;
				break;
			case 14:
				chk_AIPredictToolON.IsChecked = false;
				UsePredictEnhance = AI_MODE.EM_Enhance;
				Station[StationIndex].paramFile.AIEnhanceToolON = true;
				LoadingAIModel[StationIndex] = true;
				exp_PreviewAI.Visibility = Visibility.Visible;
				break;
			case 17:
				chk_AIProphetToolON.IsChecked = true;
				Station[StationIndex].paramFile.AIProphetToolON = true;
				exp_PreviewAIHeatMap.Visibility = Visibility.Visible;
				break;
			default:
				UsePredictEnhance = AI_MODE.EM_NoUse;
				break;
			}
			Action method = delegate
			{
				grd_AILoading.Visibility = Visibility.Visible;
				AILoadingPauseEvent.Set();
			};
			base.Dispatcher.BeginInvoke(method);
			if (!LowLevelVersion)
			{
				grd_AIFunction.Visibility = Visibility.Visible;
				btn_AIOKDB.Visibility = Visibility.Visible;
				btn_AINGDB.Visibility = Visibility.Visible;
			}
		}
		WriteToolONStatus();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool LoadingAIModelNow()
	{
		bool result = true;
		LoadingAIResult = "";
		for (int i = 0; i < 8; i++)
		{
			if (!LoadingAIModel[i])
			{
				continue;
			}
			LoadingAIModel[i] = false;
			if (Station[i].paramFile.AIEnhanceToolON)
			{
				if (!Station[i].aiEnhanceTools.ModelLoaded_A || Station[i].paramFile.AIEnhanceParam.Model_A_Changed)
				{
					Station[i].aiEnhanceTools.ResetModelRead();
					if (!Station[i].aiEnhanceTools.ReadModel(ReCreatePB: true, 0, StationIndex))
					{
						result = false;
						Station[i].aiEnhanceTools.ModelLoaded_A = false;
						string text = string.Format("第{0}站\n{1}\n", i + 1, Station[i].aiEnhanceTools.ResultText);
						LoadingAIResult += text;
					}
					else
					{
						Station[i].aiEnhanceTools.ModelLoaded_A = true;
						Station[i].paramFile.AIEnhanceParam.Model_A_Changed = false;
					}
				}
				if (Station[i].paramFile.SelectFaceParam[1].TrackerON && Station[i].paramFile.AIEnhanceParam.EnableSelectFace && (!Station[i].aiEnhanceTools.ModelLoaded_B || Station[i].paramFile.AIEnhanceParam.Model_B_Changed))
				{
					Station[i].aiEnhanceTools.ResetModelRead();
					if (!Station[i].aiEnhanceTools.ReadModel(ReCreatePB: true, 1, StationIndex))
					{
						result = false;
						Station[i].aiEnhanceTools.ModelLoaded_B = false;
						string text2 = string.Format("第{0}站\n{1}\n", i + 1, Station[i].aiEnhanceTools.ResultText);
						LoadingAIResult += text2;
					}
					else
					{
						Station[i].aiEnhanceTools.ModelLoaded_B = true;
						Station[i].paramFile.AIEnhanceParam.Model_B_Changed = false;
					}
				}
			}
			else
			{
				if (!Station[i].paramFile.AIPredictToolON)
				{
					continue;
				}
				if (!Station[i].aiPredictTools.ModelLoaded_A || Station[i].paramFile.AIPredictParam.Model_A_Changed)
				{
					if (!Station[i].aiPredictTools.ReadModel(ReCreatePB: true))
					{
						result = false;
						Station[i].aiPredictTools.ModelLoaded_A = false;
						string text3 = string.Format("第{0}站\n{1}\n", i + 1, Station[i].aiPredictTools.ResultText);
						LoadingAIResult += text3;
					}
					else
					{
						Station[i].aiPredictTools.ModelLoaded_A = true;
						Station[i].paramFile.AIPredictParam.Model_A_Changed = false;
					}
				}
				if (Station[i].paramFile.SelectFaceParam[1].TrackerON && Station[i].paramFile.AIPredictParam.EnableSelectFace && (!Station[i].aiPredictTools.ModelLoaded_B || Station[i].paramFile.AIPredictParam.Model_B_Changed))
				{
					if (!Station[i].aiPredictTools.ReadModel(ReCreatePB: true, 1))
					{
						result = false;
						Station[i].aiPredictTools.ModelLoaded_B = false;
						string text4 = string.Format("第{0}站\n{1}\n", i + 1, Station[i].aiPredictTools.ResultText);
						LoadingAIResult += text4;
					}
					else
					{
						Station[i].aiPredictTools.ModelLoaded_B = true;
						Station[i].paramFile.AIPredictParam.Model_B_Changed = false;
					}
				}
			}
		}
		return result;
	}

	private void Btn_ReferenceShow_Click(object sender, RoutedEventArgs e)
	{
		if (ReferenceShow)
		{
			ReferenceShow = false;
		}
		else
		{
			ReferenceShow = true;
		}
	}

	private void Lab_StatinoIndex_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (User >= User_Permissions.USER_Administrator)
		{
			QuickSetupWindow quickSetupWindow = new QuickSetupWindow();
			quickSetupWindow.LoadLanguage(LanguageType);
			quickSetupWindow.SetInspection(Station[StationIndex]);
			quickSetupWindow.SetThroghputFile(ThroughputFile);
			quickSetupWindow.ShowDialog();
		}
	}

	private void Btn_LineAnalysis_Click(object sender, RoutedEventArgs e)
	{
		OpenLineAnalysisTool(0);
	}

	private void OpenLineAnalysisTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			LineAnalysisWindow lineAnalysisWindow = new LineAnalysisWindow(User);
			lineAnalysisWindow.LoadLanguage(LanguageType);
			lineAnalysisWindow.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			lineAnalysisWindow.SetThroghputFile(ThroughputFile);
			lineAnalysisWindow.SetPreviewMode(on: true);
			lineAnalysisWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			LineAnalysisWindow lineAnalysisWindow2 = new LineAnalysisWindow(User);
			lineAnalysisWindow2.LoadLanguage(LanguageType);
			lineAnalysisWindow2.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			lineAnalysisWindow2.SetThroghputFile(ThroughputFile);
			lineAnalysisWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			lineAnalysisWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			lineAnalysisWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void Btn_DifferenceTool_Click(object sender, RoutedEventArgs e)
	{
		OpenDifferenceTool(0);
	}

	private void OpenDifferenceTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			DifferenceWindow differenceWindow = new DifferenceWindow(User);
			differenceWindow.LoadLanguage(LanguageType);
			differenceWindow.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			differenceWindow.SetThroghputFile(ThroughputFile);
			differenceWindow.SetPreviewMode(on: true);
			differenceWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			differenceWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			DifferenceWindow differenceWindow2 = new DifferenceWindow(User);
			differenceWindow2.LoadLanguage(LanguageType);
			differenceWindow2.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			differenceWindow2.SetThroghputFile(ThroughputFile);
			differenceWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			differenceWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			differenceWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			differenceWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void Btn_BlockAvgTool_Click(object sender, RoutedEventArgs e)
	{
		OpenBlockAvgTool(0);
	}

	private void OpenBlockAvgTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			BlockAvgToolWindow blockAvgToolWindow = new BlockAvgToolWindow(User);
			blockAvgToolWindow.LoadLanguage(LanguageType);
			blockAvgToolWindow.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			blockAvgToolWindow.SetThroghputFile(ThroughputFile);
			blockAvgToolWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			blockAvgToolWindow.SetPreviewMode(on: true);
			blockAvgToolWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			BlockAvgToolWindow blockAvgToolWindow2 = new BlockAvgToolWindow(User);
			blockAvgToolWindow2.LoadLanguage(LanguageType);
			blockAvgToolWindow2.SetInspection(index, Station[StationIndex], Station[StationIndex].SelectFaceResult);
			blockAvgToolWindow2.SetThroghputFile(ThroughputFile);
			blockAvgToolWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			blockAvgToolWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			blockAvgToolWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
			blockAvgToolWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void Lab_TestResult_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		HistoryViewWindow historyViewWindow = new HistoryViewWindow();
		if (ImageListSource == 2 && Station[StationIndex].RecentBufferIDList != null)
		{
			if (RecentBufferIndex[StationIndex] >= 0 && RecentBufferIndex[StationIndex] < Station[StationIndex].RecentBufferIDList.Count)
			{
				historyViewWindow.SetStation(Station, BufferImageWidth, BufferImageHeight, BufferImageBand, StationIndex, RecentBufferIndex[StationIndex]);
			}
		}
		else
		{
			historyViewWindow.SetStation(Station, BufferImageWidth, BufferImageHeight, BufferImageBand);
		}
		historyViewWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void OpenBufferList()
	{
		switch (ImageListSource)
		{
		case 2:
			try
			{
				if (Station[StationIndex].RecentBufferList != null)
				{
					if (Station[StationIndex].RecentBufferList.Count > 0)
					{
						if (RecentBufferIndex[StationIndex] < 0 || RecentBufferIndex[StationIndex] >= Station[StationIndex].RecentBufferList.Count)
						{
							RecentBufferIndex[StationIndex] = 0;
						}
						RecentBufferIndex[StationIndex] = Station[StationIndex].RecentBufferList.Count - 1;
						Station[StationIndex].SetBuffers(Station[StationIndex].RecentBufferList[RecentBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
						Inspect();
						RuncountText = string.Format("{0} / {1}", RecentBufferIndex[StationIndex] + 1, Station[StationIndex].RecentBufferList.Count);
						btn_ImageNum.Content = RuncountText;
					}
					else
					{
						ShowNoContent();
					}
				}
				else
				{
					ShowNoContent();
				}
				break;
			}
			catch
			{
				string text4 = "TW01-ToolsWindow-OpenBufferList()-RecentBufferList";
				CVBuffer.SaveLog(text4);
				if (ShowMessage)
				{
					System.Windows.MessageBox.Show(text4, LanguageManager.Instance["Error"]);
				}
				break;
			}
		case 3:
			try
			{
				if (Station[StationIndex].OKBufferList != null)
				{
					if (Station[StationIndex].OKBufferList.Count > 0)
					{
						if (OKBufferIndex[StationIndex] < 0 || OKBufferIndex[StationIndex] >= Station[StationIndex].OKBufferList.Count)
						{
							OKBufferIndex[StationIndex] = 0;
						}
						Station[StationIndex].SetBuffers(Station[StationIndex].OKBufferList[OKBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
						Inspect();
						RuncountText = string.Format("{0} / {1}", OKBufferIndex[StationIndex] + 1, Station[StationIndex].OKBufferList.Count);
						btn_ImageNum.Content = RuncountText;
					}
					else
					{
						ShowNoContent();
					}
				}
				else
				{
					ShowNoContent();
				}
				break;
			}
			catch
			{
				string text3 = "TW02-ToolsWindow-OpenBufferList()-OKBufferList";
				CVBuffer.SaveLog(text3);
				if (ShowMessage)
				{
					System.Windows.MessageBox.Show(text3, LanguageManager.Instance["Error"]);
				}
				break;
			}
		case 4:
			try
			{
				cb_ParetoNGSelect.Visibility = Visibility.Visible;
				lab_ParetoNGSelect.Visibility = Visibility.Visible;
				ParetoNGSelect = 0;
				cb_ParetoNGSelect.SelectedIndex = 0;
				if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect] != null)
				{
					if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count > 0)
					{
						if (NGBufferIndex[StationIndex] < 0 || NGBufferIndex[StationIndex] >= Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count)
						{
							NGBufferIndex[StationIndex] = 0;
						}
						Station[StationIndex].SetBuffers(Station[StationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
						Inspect();
						RuncountText = string.Format("{0} / {1}", NGBufferIndex[StationIndex] + 1, Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count);
						btn_ImageNum.Content = RuncountText;
					}
					else
					{
						ShowNoContent();
					}
				}
				else
				{
					ShowNoContent();
				}
				break;
			}
			catch
			{
				string text2 = "TW03-ToolsWindow-OpenBufferList()-NGBufferList";
				CVBuffer.SaveLog(text2);
				if (ShowMessage)
				{
					System.Windows.MessageBox.Show(text2, LanguageManager.Instance["Error"]);
				}
				break;
			}
		case 5:
			try
			{
				if (Station[StationIndex].RetestBufferList != null)
				{
					if (Station[StationIndex].RetestBufferList.Count > 0)
					{
						if (RetestBufferIndex[StationIndex] < 0 || RetestBufferIndex[StationIndex] >= Station[StationIndex].RetestBufferList.Count)
						{
							RetestBufferIndex[StationIndex] = 0;
						}
						Station[StationIndex].SetBuffers(Station[StationIndex].RetestBufferList[RetestBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
						Inspect();
						RuncountText = string.Format("{0} / {1}", RetestBufferIndex[StationIndex] + 1, Station[StationIndex].RetestBufferList.Count);
						btn_ImageNum.Content = RuncountText;
					}
					else
					{
						ShowNoContent();
					}
				}
				else
				{
					ShowNoContent();
				}
				break;
			}
			catch
			{
				string text = "TW03-ToolsWindow-OpenBufferList()-RetestBufferList";
				CVBuffer.SaveLog(text);
				if (ShowMessage)
				{
					System.Windows.MessageBox.Show(text, LanguageManager.Instance["Error"]);
				}
				break;
			}
		}
	}

	private void Btn_MatchTool_Click(object sender, RoutedEventArgs e)
	{
		OpenMatchTool(0);
	}

	private void OpenMatchTool(int index)
	{
		if (!CheckBufferAndRef())
		{
			MatchToolWindow matchToolWindow = new MatchToolWindow(User);
			matchToolWindow.LoadLanguage(LanguageType);
			matchToolWindow.SetPublicDB(PublicPatternDB);
			matchToolWindow.SetInspection(index, Station[StationIndex], StationIndex, Station[StationIndex].SelectFaceResult);
			matchToolWindow.SetThroghputFile(ThroughputFile);
			matchToolWindow.SetPreviewMode(on: true);
			matchToolWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
		else
		{
			Main.AllowDrop = false;
			MatchToolWindow matchToolWindow2 = new MatchToolWindow(User);
			matchToolWindow2.LoadLanguage(LanguageType);
			matchToolWindow2.SetPublicDB(PublicPatternDB);
			matchToolWindow2.SetInspection(index, Station[StationIndex], StationIndex, Station[StationIndex].SelectFaceResult);
			matchToolWindow2.SetThroghputFile(ThroughputFile);
			matchToolWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			matchToolWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			matchToolWindow2.ShowDialog();
			Btn_TestAll_Click(this, null);
			Main.AllowDrop = true;
		}
	}

	private void Btn_SelectWayTool_Click(object sender, RoutedEventArgs e)
	{
		OpenSelectWayTool();
	}

	private void OpenSelectWayTool()
	{
		if (CheckBuffer())
		{
			SelectWayWindow selectWayWindow = new SelectWayWindow();
			selectWayWindow.LoadLanguage(LanguageType);
			selectWayWindow.SetInspection(Station[StationIndex], StationIndex);
			selectWayWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrcTmp, Station[StationIndex].LocateRectTmp);
			selectWayWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void Btn_OKDB_Click(object sender, RoutedEventArgs e)
	{
		btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseFileBufList.Background = System.Windows.Media.Brushes.Yellow;
		btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
		btn_DelAIOKDB.Visibility = Visibility.Hidden;
		CheckFileList(OKPath[StationIndex]);
	}

	private void Btn_NGDB_Click(object sender, RoutedEventArgs e)
	{
		btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
		btn_UseFileBufList.Background = System.Windows.Media.Brushes.Yellow;
		btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
		btn_DelAIOKDB.Visibility = Visibility.Hidden;
		CheckFileList(NGPath[StationIndex]);
	}

	private void Btn_SetAll_Click(object sender, RoutedEventArgs e)
	{
		DBSetupWindow dBSetupWindow = new DBSetupWindow();
		dBSetupWindow.LoadLanguage(LanguageType);
		dBSetupWindow.SetPath(OKPath, NGPath);
		dBSetupWindow.ShowDialog();
		dBSetupWindow.GetPath(ref OKPath, ref NGPath);
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.OKPath = OKPath[i];
			Station[i].paramFile.NGPath = NGPath[i];
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AddToDB_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].buffers.SrcReady || Station[StationIndex].buffers.Src.Empty())
		{
			return;
		}
		try
		{
			if (!Directory.Exists(NGPath[StationIndex]))
			{
				Directory.CreateDirectory(NGPath[StationIndex]);
				if (!Directory.Exists(NGPath[StationIndex]))
				{
					return;
				}
			}
			DateTime now = DateTime.Now;
			string fileName = NGPath[StationIndex] + "\\" + now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
			Station[StationIndex].buffers.Src.SaveImage(fileName);
			if (LanguageType == 0)
			{
				ShowLabMessage("Copy Finish!");
			}
			else if (LanguageType == 1)
			{
				ShowLabMessage("拷貝完成!");
			}
			else if (LanguageType == 2)
			{
				ShowLabMessage("拷贝完成!");
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TW07-ToolsWindows-Btn_AddToDB_Click() " + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RunTest(bool AutoMode = true)
	{
		if (Station[StationIndex] == null || (AutoMode && (timer_state == 0 || timer_state == 2)))
		{
			return;
		}
		int stationIndex = StationIndex;
		try
		{
			switch (ImageListSource)
			{
			case 0:
				if (!Station[stationIndex].buffers.SrcReady)
				{
					SetResult(result: false, FLOW_BIN.EM_FLOW_RETESET);
					txt_InspResult.Clear();
					txt_InspResult.Text = LanguageManager.Instance["SetImageFirst"];
					cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
					return;
				}
				AutoTestPauseEvent.Reset();
				break;
			case 1:
			{
				if (AutoMode)
				{
					if (filelist[StationIndex] == null)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (filelist[StationIndex].Length == 0)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (fileIndex < filelist[StationIndex].Length - 1)
					{
						fileIndex++;
					}
					else
					{
						AutoTestPauseEvent.Reset();
						fileIndex = 0;
						timer_state = 0;
						btn_TestAllImage.Background = System.Windows.Media.Brushes.White;
						DrawImage(DrawMat);
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						lab_AutestState.Content = ThreadStateStr;
					}
				}
				if (filelist[StationIndex] == null)
				{
					return;
				}
				string text = LanguageManager.Instance["FileNotFound"];
				if (fileIndex >= filelist[StationIndex].Length)
				{
					fileIndex = 0;
					if (filelist[StationIndex].Length == 0)
					{
						return;
					}
				}
				if (!File.Exists(filelist[StationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[StationIndex][fileIndex] + text);
					return;
				}
				Mat buffers = CVBuffer.ImRead(filelist[StationIndex][fileIndex]);
				Station[stationIndex].SetBuffers(buffers);
				RuncountText = string.Format("{0} / {1}", fileIndex + 1, filelist[StationIndex].Length);
				btn_ImageNum.Content = RuncountText;
				break;
			}
			case 2:
				if (AutoMode)
				{
					if (Station[stationIndex].RecentBufferList == null)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (Station[stationIndex].RecentBufferList.Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (RecentBufferIndex[StationIndex] < Station[stationIndex].RecentBufferList.Count - 1)
					{
						RecentBufferIndex[StationIndex]++;
					}
					else
					{
						AutoTestPauseEvent.Reset();
						RecentBufferIndex[StationIndex] = 0;
						timer_state = 0;
						btn_TestAllImage.Background = System.Windows.Media.Brushes.White;
						DrawImage(DrawMat);
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						lab_AutestState.Content = ThreadStateStr;
					}
				}
				if (Station[stationIndex].RecentBufferList == null || Station[stationIndex].RecentBufferList.Count <= 0)
				{
					return;
				}
				if (RecentBufferIndex[StationIndex] >= Station[stationIndex].RecentBufferList.Count)
				{
					RecentBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].RecentBufferList[RecentBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", RecentBufferIndex[StationIndex] + 1, Station[stationIndex].RecentBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				break;
			case 3:
				if (AutoMode)
				{
					if (Station[stationIndex].OKBufferList == null)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (Station[stationIndex].OKBufferList.Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (OKBufferIndex[StationIndex] < Station[stationIndex].OKBufferList.Count - 1)
					{
						OKBufferIndex[StationIndex]++;
					}
					else
					{
						AutoTestPauseEvent.Reset();
						OKBufferIndex[StationIndex] = 0;
						timer_state = 0;
						btn_TestAllImage.Background = System.Windows.Media.Brushes.White;
						DrawImage(DrawMat);
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						lab_AutestState.Content = ThreadStateStr;
					}
				}
				if (Station[stationIndex].OKBufferList == null || Station[stationIndex].OKBufferList.Count <= 0)
				{
					return;
				}
				if (OKBufferIndex[StationIndex] >= Station[stationIndex].OKBufferList.Count)
				{
					OKBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].OKBufferList[OKBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", OKBufferIndex[StationIndex] + 1, Station[stationIndex].OKBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				break;
			case 4:
				if (AutoMode)
				{
					if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect] == null)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (NGBufferIndex[StationIndex] < Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count - 1)
					{
						NGBufferIndex[StationIndex]++;
					}
					else
					{
						AutoTestPauseEvent.Reset();
						NGBufferIndex[StationIndex] = 0;
						timer_state = 0;
						btn_TestAllImage.Background = System.Windows.Media.Brushes.White;
						DrawImage(DrawMat);
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						lab_AutestState.Content = ThreadStateStr;
					}
				}
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect] == null || Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count <= 0)
				{
					return;
				}
				if (NGBufferIndex[StationIndex] >= Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count)
				{
					NGBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", NGBufferIndex[StationIndex] + 1, Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count);
				btn_ImageNum.Content = RuncountText;
				break;
			case 5:
				if (AutoMode)
				{
					if (Station[stationIndex].RetestBufferList == null)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (Station[stationIndex].RetestBufferList.Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						return;
					}
					if (RetestBufferIndex[StationIndex] < Station[stationIndex].RetestBufferList.Count - 1)
					{
						RetestBufferIndex[StationIndex]++;
					}
					else
					{
						AutoTestPauseEvent.Reset();
						RetestBufferIndex[StationIndex] = 0;
						timer_state = 0;
						btn_TestAllImage.Background = System.Windows.Media.Brushes.White;
						DrawImage(DrawMat);
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						lab_AutestState.Content = ThreadStateStr;
					}
				}
				if (Station[stationIndex].RetestBufferList == null || Station[stationIndex].RetestBufferList.Count <= 0)
				{
					return;
				}
				if (RetestBufferIndex[StationIndex] >= Station[stationIndex].RetestBufferList.Count)
				{
					RetestBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].RetestBufferList[RetestBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", RetestBufferIndex[StationIndex] + 1, Station[stationIndex].RetestBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				break;
			}
		}
		catch (Exception ex)
		{
			string text2 = string.Format("ImageListSource={0},{1}", ImageListSource, AutoMode);
			CVBuffer.SaveLog(string.Concat("TW07-ToolsWindows-RunTest()" + text2, ex.ToString()));
			return;
		}
		if (Inspect())
		{
			if (AutoMode)
			{
				OKCount++;
				OKRatio = (double)OKCount / (double)(OKCount + NGCount);
				txt_OKCount.Text = OKCount.ToString();
				txt_OKCountRatio.Text = string.Format("{0:P}", OKRatio);
				if (chk_OKStop.IsChecked == true)
				{
					AutoTestPauseEvent.Reset();
					timer_state = 2;
					DrawImage(DrawMat);
					ThreadStateStr = LanguageManager.Instance["AutorunPause"];
					lab_AutestState.Content = ThreadStateStr;
				}
			}
			else
			{
				DrawImage(DrawMat);
			}
		}
		else if (AutoMode)
		{
			NGCount++;
			NGRatio = (double)NGCount / (double)(OKCount + NGCount);
			txt_NGCount.Text = NGCount.ToString();
			txt_NGCountRatio.Text = string.Format("{0:P}", NGRatio);
			if (chk_NGStop.IsChecked == true)
			{
				AutoTestPauseEvent.Reset();
				timer_state = 2;
				DrawImage(DrawMat);
				ThreadStateStr = LanguageManager.Instance["AutorunPause"];
				lab_AutestState.Content = ThreadStateStr;
			}
		}
		else
		{
			DrawImage(DrawMat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RunTestThread(bool AutoMode = true)
	{
		if (Station[StationIndex] == null || (AutoMode && (timer_state == 0 || timer_state == 2)))
		{
			return;
		}
		int stationIndex = StationIndex;
		ImageAddr = -1;
		filenameID = "";
		try
		{
			switch (ImageListSource)
			{
			case 0:
				AutoTestPauseEvent.Reset();
				AutoTestUIPauseEvent.Reset();
				break;
			case 1:
			{
				if (AutoMode)
				{
					if (filelist[StationIndex] == null)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (filelist[StationIndex].Length == 0)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (fileIndex < filelist[StationIndex].Length - 1)
					{
						fileIndex++;
						ImageAddr = fileIndex;
					}
					else
					{
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						RuncountText = string.Format("{0} / {1}", fileIndex + 1, filelist[StationIndex].Length);
						fileIndex = 0;
						timer_state = 0;
						AutoTestPauseEvent.Reset();
					}
				}
				if (filelist[StationIndex] == null || !File.Exists(filelist[StationIndex][fileIndex]))
				{
					return;
				}
				string path = filelist[StationIndex][fileIndex];
				filenameID = System.IO.Path.GetFileNameWithoutExtension(path);
				Mat buffers = CVBuffer.ImRead(path);
				Station[stationIndex].SetBuffers(buffers);
				RuncountText = string.Format("{0} / {1}", fileIndex + 1, filelist[StationIndex].Length);
				if (filelist[StationIndex].Length != 0)
				{
					TestProgress = (double)(fileIndex + 1) / (double)filelist[StationIndex].Length * 100.0;
				}
				break;
			}
			case 2:
				if (AutoMode)
				{
					if (Station[stationIndex].RecentBufferList == null)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (Station[stationIndex].RecentBufferList.Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (RecentBufferIndex[StationIndex] < Station[stationIndex].RecentBufferList.Count - 1)
					{
						RecentBufferIndex[StationIndex]++;
						ImageAddr = RecentBufferIndex[StationIndex];
					}
					else
					{
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						RuncountText = string.Format("{0} / {1}", RecentBufferIndex[StationIndex] + 1, Station[stationIndex].RecentBufferList.Count);
						RecentBufferIndex[StationIndex] = 0;
						timer_state = 0;
						AutoTestPauseEvent.Reset();
					}
				}
				if (Station[stationIndex].RecentBufferList == null || Station[stationIndex].RecentBufferList.Count <= 0)
				{
					return;
				}
				if (RecentBufferIndex[StationIndex] >= Station[stationIndex].RecentBufferList.Count)
				{
					RecentBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].RecentBufferList[RecentBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", RecentBufferIndex[StationIndex] + 1, Station[stationIndex].RecentBufferList.Count);
				if (Station[stationIndex].RecentBufferList.Count > 0)
				{
					TestProgress = (double)(RecentBufferIndex[StationIndex] + 1) / (double)Station[stationIndex].RecentBufferList.Count * 100.0;
				}
				break;
			case 3:
				if (AutoMode)
				{
					if (Station[stationIndex].OKBufferList == null)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (Station[stationIndex].OKBufferList.Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (OKBufferIndex[StationIndex] < Station[stationIndex].OKBufferList.Count - 1)
					{
						OKBufferIndex[StationIndex]++;
						ImageAddr = OKBufferIndex[StationIndex];
					}
					else
					{
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						RuncountText = string.Format("{0} / {1}", OKBufferIndex[StationIndex] + 1, Station[stationIndex].OKBufferList.Count);
						OKBufferIndex[StationIndex] = 0;
						timer_state = 0;
						AutoTestPauseEvent.Reset();
					}
				}
				if (Station[stationIndex].OKBufferList == null || Station[stationIndex].OKBufferList.Count <= 0)
				{
					return;
				}
				if (OKBufferIndex[StationIndex] >= Station[stationIndex].OKBufferList.Count)
				{
					OKBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].OKBufferList[OKBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", OKBufferIndex[StationIndex] + 1, Station[stationIndex].OKBufferList.Count);
				if (Station[stationIndex].OKBufferList.Count > 0)
				{
					TestProgress = (double)(OKBufferIndex[StationIndex] + 1) / (double)Station[stationIndex].OKBufferList.Count * 100.0;
				}
				break;
			case 4:
				if (AutoMode)
				{
					if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect] == null)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (NGBufferIndex[StationIndex] < Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count - 1)
					{
						NGBufferIndex[StationIndex]++;
						ImageAddr = NGBufferIndex[StationIndex];
					}
					else
					{
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						RuncountText = string.Format("{0} / {1}", NGBufferIndex[StationIndex] + 1, Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count);
						NGBufferIndex[StationIndex] = 0;
						AutoTestPauseEvent.Reset();
						timer_state = 0;
					}
				}
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect] == null || Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count <= 0)
				{
					return;
				}
				if (NGBufferIndex[StationIndex] >= Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count)
				{
					NGBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", NGBufferIndex[StationIndex] + 1, Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count);
				if (Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count > 0)
				{
					TestProgress = (double)(NGBufferIndex[StationIndex] + 1) / (double)Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count * 100.0;
				}
				break;
			case 5:
				if (AutoMode)
				{
					if (Station[stationIndex].RetestBufferList == null)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (Station[stationIndex].RetestBufferList.Count <= 0)
					{
						AutoTestPauseEvent.Reset();
						timer_state = 0;
						return;
					}
					if (RetestBufferIndex[StationIndex] < Station[stationIndex].RetestBufferList.Count - 1)
					{
						RetestBufferIndex[StationIndex]++;
						ImageAddr = RetestBufferIndex[StationIndex];
					}
					else
					{
						ThreadStateStr = LanguageManager.Instance["AutorunFinish"];
						RuncountText = string.Format("{0} / {1}", RetestBufferIndex[StationIndex] + 1, Station[stationIndex].RetestBufferList.Count);
						RetestBufferIndex[StationIndex] = 0;
						AutoTestPauseEvent.Reset();
						timer_state = 0;
					}
				}
				if (Station[stationIndex].RetestBufferList == null || Station[stationIndex].RetestBufferList.Count <= 0)
				{
					return;
				}
				if (RetestBufferIndex[StationIndex] >= Station[stationIndex].RetestBufferList.Count)
				{
					RetestBufferIndex[StationIndex] = 0;
				}
				Station[stationIndex].SetBuffers(Station[stationIndex].RetestBufferList[RetestBufferIndex[StationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				RuncountText = string.Format("{0} / {1}", RetestBufferIndex[StationIndex] + 1, Station[stationIndex].RetestBufferList.Count);
				if (Station[stationIndex].RetestBufferList.Count > 0)
				{
					TestProgress = (double)(RetestBufferIndex[StationIndex] + 1) / (double)Station[stationIndex].RetestBufferList.Count * 100.0;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("ImageListSource={0},{1}", ImageListSource, AutoMode);
			CVBuffer.SaveLog(string.Concat("TW07-ToolsWindows-RunTestThread()" + text, ex.ToString()));
			return;
		}
		if (InspectNoUI())
		{
			if (AutoMode)
			{
				OKCount++;
				if (OKStop)
				{
					ThreadStateStr = LanguageManager.Instance["AutorunPause"];
					AutoTestPauseEvent.Reset();
					timer_state = 2;
					OKRatio = (double)OKCount / (double)(OKCount + NGCount);
					NGRatio = (double)NGCount / (double)(OKCount + NGCount);
					return;
				}
			}
		}
		else if (AutoMode)
		{
			NGCount++;
			if (NGStop)
			{
				ThreadStateStr = LanguageManager.Instance["AutorunPause"];
				AutoTestPauseEvent.Reset();
				timer_state = 2;
				OKRatio = (double)OKCount / (double)(OKCount + NGCount);
				NGRatio = (double)NGCount / (double)(OKCount + NGCount);
				return;
			}
		}
		if (AutoMode)
		{
			OKRatio = (double)OKCount / (double)(OKCount + NGCount);
			NGRatio = (double)NGCount / (double)(OKCount + NGCount);
		}
	}

	private void Btn_SelectFaceTool_Click(object sender, RoutedEventArgs e)
	{
		OpenSelectFaceTool();
	}

	private void OpenSelectFaceTool()
	{
		if (CheckBuffer())
		{
			SelectFaceWindow selectFaceWindow = new SelectFaceWindow();
			selectFaceWindow.LoadLanguage(LanguageType);
			selectFaceWindow.SetInspection(0, Station[StationIndex]);
			selectFaceWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			selectFaceWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void Chk_SelectWayON_Unchecked(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string CheckImg(string ext)
	{
		string text = ext.ToLower();
		if (text == ".gif" || text == ".jpg" || text == ".png" || text == ".bmp" || text == ".tif")
		{
			return "1";
		}
		return "0";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Window_Drop(object sender, System.Windows.DragEventArgs e)
	{
		if (!Main.AllowDrop)
		{
			return;
		}
		string[] array = e.Data.GetData(System.Windows.DataFormats.FileDrop, autoConvert: true) as string[];
		if (array.Length < 1)
		{
			return;
		}
		string text = array[0];
		if (!string.IsNullOrEmpty(text) && File.Exists(text))
		{
			string extension = System.IO.Path.GetExtension(text);
			if (!CheckImg(extension).Equals("0"))
			{
				try
				{
					btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
					btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
					btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
					btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
					btn_UseFileBufList.Background = System.Windows.Media.Brushes.White;
					btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.Yellow;
					Mat buffers = CVBuffer.ImRead(text, ImreadModes.Unchanged);
					Station[StationIndex].SetBuffers(buffers);
					Mat mat = Station[StationIndex].buffers.Src.Clone();
					DrawImage(mat);
					RuncountText = string.Format("{0} / {1}", 1, 1);
					btn_ImageNum.Content = RuncountText;
					ImageListSource = 0;
				}
				catch
				{
					System.Windows.MessageBox.Show("不是有效的圖檔格式", LanguageManager.Instance["Error"]);
				}
			}
		}
		else
		{
			btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseFileBufList.Background = System.Windows.Media.Brushes.Yellow;
			btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
			CheckFileList(text);
		}
	}

	private void Window_DragEnter(object sender, System.Windows.DragEventArgs e)
	{
		e.Effects = System.Windows.DragDropEffects.All;
	}

	private void Cnv_Image_MouseWheel(object sender, MouseWheelEventArgs e)
	{
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

	private void Chk_NGStop_Checked(object sender, RoutedEventArgs e)
	{
		NGStop = true;
	}

	private void Chk_NGStop_Unchecked(object sender, RoutedEventArgs e)
	{
		NGStop = false;
	}

	private void Chk_OKStop_Unchecked(object sender, RoutedEventArgs e)
	{
		OKStop = false;
	}

	private void Chk_OKStop_Checked(object sender, RoutedEventArgs e)
	{
		OKStop = true;
	}

	private void Grd_Main_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta > 0)
		{
			Btn_PrevImage_Click(null, null);
		}
		else
		{
			Btn_NextImage_Click(null, null);
		}
	}

	private void Btn_ImgT1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((System.Windows.Controls.Button)sender).Uid);
		if (!Station[StationIndex].TempImage[num].Empty())
		{
			Station[StationIndex].SetBuffers(Station[StationIndex].TempImage[num]);
			DrawImage(Station[StationIndex].buffers.Src);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImgT1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (Station[StationIndex].buffers.Src.Empty())
		{
			return;
		}
		int num = Convert.ToInt32(((System.Windows.Controls.Button)sender).Uid);
		if (!Station[StationIndex].TempImage[num].Empty())
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? System.Windows.MessageBox.Show("Replace temporary Image?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? System.Windows.MessageBox.Show("是否替换暂存影像?！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : System.Windows.MessageBox.Show("是否替換暫存影像?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
		}
		Station[StationIndex].TempImage[num] = Station[StationIndex].buffers.Src.Clone();
		switch (num)
		{
		case 0:
			btn_ImgT1.Background = System.Windows.Media.Brushes.LightSkyBlue;
			break;
		case 1:
			btn_ImgT2.Background = System.Windows.Media.Brushes.LightSkyBlue;
			break;
		case 2:
			btn_ImgT3.Background = System.Windows.Media.Brushes.LightSkyBlue;
			break;
		}
		if (LanguageType == 0)
		{
			ShowLabMessage("Set Temporary Image Finish!");
		}
		else if (LanguageType == 1)
		{
			ShowLabMessage("存入暫存影像!");
		}
		else if (LanguageType == 2)
		{
			ShowLabMessage("存入暂存影像!");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AIBitmapSave_Click(object sender, RoutedEventArgs e)
	{
		Btn_TestAll_Click(this, null);
		if (!CheckBufferAndRef() || Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
		{
			return;
		}
		string text = string.Format("D:\\AIIMage\\{0}\\", StationIndex + 1);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (Directory.Exists(text))
		{
			string text2 = text + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
			Station[StationIndex].maskTool.AIMat.SaveImage(text2);
			string str = "";
			if (LanguageType == 0)
			{
				str = string.Format("{0} Save AI Image Finish", text2);
			}
			else if (LanguageType == 1)
			{
				str = string.Format("{0} AI 影像儲存成功", text2);
			}
			else if (LanguageType == 2)
			{
				str = string.Format("{0} AI 影像储存成功!", text2);
			}
			ShowLabMessage(str);
		}
	}

	private void Cnv_Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!Station[StationIndex].buffers.LocatedReady)
		{
			Btn_TestAll_Click(this, null);
		}
		if (Station[StationIndex].buffers.LocatedReady)
		{
			Mat mat = new Mat();
			mat = Station[StationIndex].buffers.Src;
			ImageSource imageSource = Station[StationIndex].buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
		}
	}

	private void Cnv_Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Btn_TestAll_Click(this, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ParetoChart_Click(object sender, RoutedEventArgs e)
	{
		ParetoChartWindow paretoChartWindow = new ParetoChartWindow();
		paretoChartWindow.SetInspection(Station);
		paretoChartWindow.LoadLanguage(LanguageType);
		paretoChartWindow.SetRecipe(ThroughputFile);
		paretoChartWindow.ShowDialog();
		ThroughputFile.WriteMainParam();
		ThroughputFile.WriteParetoItems();
		cb_ParetoNGSelect.Items.Clear();
		cb_ParetoNGSelect.Items.Add("All NG");
		int num = 0;
		for (int i = 1; i < 30 && ThroughputFile.param.AllParetoItems[i] != ""; i++)
		{
			cb_ParetoNGSelect.Items.Add(ThroughputFile.param.AllParetoItems[i]);
			num++;
		}
		for (int j = 0; j < 8; j++)
		{
			Station[j].SetParetoChartName(ThroughputFile.param.AllParetoItems);
			Station[j].ParetoItemCount = num;
		}
		cb_ParetoNGSelect.SelectedIndex = 0;
	}

	private void Btn_SizeStatic_Click(object sender, RoutedEventArgs e)
	{
		SizeChartWindow sizeChartWindow = new SizeChartWindow();
		sizeChartWindow.SetInspection(Station);
		sizeChartWindow.LoadLanguage(LanguageType);
		sizeChartWindow.SetOJBSizes(OBJSizeStatistics);
		sizeChartWindow.SetRecipe(ThroughputFile);
		sizeChartWindow.SetClientCode(ThroughputFile.CurrentClient);
		sizeChartWindow.ShowDialog();
		ThroughputFile.WriteThroughputParam();
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.SetComDigits(ThroughputFile.ComDigits);
		}
	}

	public void SetAIResultDelagateObj(ref GetAIResultDelegate gAID)
	{
		getAIResult = gAID;
	}

	public void SetSaveFinishResultDelagateObj(ref SetSaveFinishResultDelegate gSFD)
	{
		setSaveFinishResult = gSFD;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_ParetoNGSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited)
		{
			return;
		}
		ParetoNGSelect = cb_ParetoNGSelect.SelectedIndex;
		if (ParetoNGSelect < 0)
		{
			ParetoNGSelect = 0;
		}
		else
		{
			if (ImageListSource != 4)
			{
				return;
			}
			try
			{
				if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect] != null)
				{
					if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count > 0)
					{
						if (NGBufferIndex[StationIndex] < 0 || NGBufferIndex[StationIndex] >= Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count)
						{
							NGBufferIndex[StationIndex] = 0;
						}
						Station[StationIndex].SetBuffers(Station[StationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
						Inspect();
						RuncountText = string.Format("{0} / {1}", NGBufferIndex[StationIndex] + 1, Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count);
						btn_ImageNum.Content = RuncountText;
					}
					else
					{
						ShowNoContent();
					}
				}
				else
				{
					ShowNoContent();
				}
			}
			catch (Exception ex)
			{
				CVBuffer.SaveLog("ToolsWindow-Cb_ParetoNGSelect_SelectionChanged()" + ex.ToString());
			}
		}
	}

	private void Chk_SaveAI_Click(object sender, RoutedEventArgs e)
	{
		if (!inited)
		{
			return;
		}
		if (chk_SaveAI.IsChecked == true)
		{
			SetSaveAIImage(status: true);
			for (int i = 0; i < 8; i++)
			{
				if (Station[i] != null)
				{
					Station[i].SetSaveAIImage(status: true);
				}
			}
			return;
		}
		SetSaveAIImage(status: false);
		for (int j = 0; j < 8; j++)
		{
			if (Station[j] != null)
			{
				Station[j].SetSaveAIImage(status: false);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetSaveAIImage(bool status)
	{
		SaveAIImage = status;
		lab_SaveAIImage.Content = "AI存圖開啟";
		if (status)
		{
			lab_SaveAIImage.Visibility = Visibility.Visible;
		}
		else
		{
			lab_SaveAIImage.Visibility = Visibility.Hidden;
		}
	}

	public List<OBJSIZE> GetOBJSizeStatistics()
	{
		SizeStatistics sizeStatistics = new SizeStatistics();
		sizeStatistics.SetInspection(Station);
		sizeStatistics.SetOJBSizes(OBJSizeStatistics);
		sizeStatistics.SetRecipe(ThroughputFile);
		sizeStatistics.CalcSizeStatistics();
		return OBJSizeStatistics;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<string> GetOBJSizeStatisticsName()
	{
		List<string> list = new List<string>();
		string item = "L";
		list.Add(item);
		item = "W";
		list.Add(item);
		item = "T";
		list.Add(item);
		list.Add(ThroughputFile.param.LPadStr);
		list.Add(ThroughputFile.param.RPadStr);
		list.Add(ThroughputFile.param.GapStr);
		list.Add(ThroughputFile.param.C1Str);
		list.Add(ThroughputFile.param.C2Str);
		list.Add(ThroughputFile.param.C3Str);
		list.Add(ThroughputFile.param.C4Str);
		list.Add(ThroughputFile.param.C5Str);
		list.Add(ThroughputFile.param.C6Str);
		list.Add(ThroughputFile.param.C7Str);
		list.Add(ThroughputFile.param.C8Str);
		list.Add(ThroughputFile.param.C9Str);
		list.Add(ThroughputFile.param.C10Str);
		list.Add(ThroughputFile.param.C11Str);
		list.Add(ThroughputFile.param.C12Str);
		list.Add(ThroughputFile.param.C13Str);
		list.Add(ThroughputFile.param.C14Str);
		return list;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AIEnhanceTool_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			AIToolWindow aIToolWindow = new AIToolWindow();
			aIToolWindow.LoadLanguage(LanguageType);
			aIToolWindow.SetPredictMode(UsePredictMode: false);
			aIToolWindow.SetThroghputFile(ThroughputFile);
			aIToolWindow.SetInspection(Station, Station[StationIndex], StationIndex);
			aIToolWindow.ShowDialog();
			if (aIToolWindow.EditHashTag)
			{
				ThroughputFile.WriteThroughputParam();
			}
			if (Station[StationIndex].paramFile.AIEnhanceParam.EnableAsignGPU)
			{
				Station[StationIndex].aiEnhanceTools.ReAssignGPU(Station[StationIndex].paramFile.AIEnhanceParam.GPUID);
			}
			if (Station[StationIndex].aiEnhanceTools != null && chk_AIEnhanceToolON.IsChecked == true && (aIToolWindow.ReloadModel[0] || aIToolWindow.ReloadModel[1]))
			{
				LoadingAIModel[StationIndex] = true;
				for (int i = 0; i < 8; i++)
				{
					if (i != StationIndex)
					{
						if (aIToolWindow.ReloadModel[0] && (Station[i].paramFile.AIEnhanceParam.ModelName_A == Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A || Station[i].paramFile.AIEnhanceParam.ModelName_B == Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A))
						{
							Station[i].paramFile.AIEnhanceParam.Model_A_Changed = true;
							Station[i].paramFile.AIEnhanceParam.Model_B_Changed = true;
							LoadingAIModel[i] = true;
						}
						if (aIToolWindow.ReloadModel[1] && (Station[i].paramFile.AIEnhanceParam.ModelName_A == Station[StationIndex].paramFile.AIEnhanceParam.ModelName_B || Station[i].paramFile.AIEnhanceParam.ModelName_B == Station[StationIndex].paramFile.AIEnhanceParam.ModelName_B))
						{
							Station[i].paramFile.AIEnhanceParam.Model_A_Changed = true;
							Station[i].paramFile.AIEnhanceParam.Model_B_Changed = true;
							LoadingAIModel[i] = true;
						}
						if (Station[i].paramFile.AIPredictToolON && Station[i].paramFile.AIPredictParam.Model_A_Changed)
						{
							LoadingAIModel[i] = true;
						}
						if (Station[i].paramFile.AIEnhanceToolON && Station[i].paramFile.AIEnhanceParam.Model_A_Changed)
						{
							LoadingAIModel[i] = true;
						}
					}
				}
				grd_AILoading.Visibility = Visibility.Visible;
				UsePredictEnhance = AI_MODE.EM_Enhance;
				AILoadingPauseEvent.Set();
			}
			Station[StationIndex].paramFile.AIEnhanceParam.ValueChangeFlag = true;
			UpdateAIImageStatus(StationIndex);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_AIEnhanceTool_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_CancleSave_Click(object sender, RoutedEventArgs e)
	{
		if (System.Windows.MessageBox.Show(LanguageManager.Instance["CancleSaveResult"], LanguageManager.Instance["Warring"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			Station[i].paramFile.LocateParam = Station[i].paramFile_Backup.LocateParam;
			Station[i].paramFile.SelectWayParam = Station[i].paramFile_Backup.SelectWayParam;
			Station[i].paramFile.AIEnhanceParam = Station[i].paramFile_Backup.AIEnhanceParam;
			Station[i].paramFile.AIPredictParam = Station[i].paramFile_Backup.AIPredictParam;
			for (int j = 0; j < 4; j++)
			{
				Station[i].paramFile.SelectFaceParam = Station[i].paramFile_Backup.SelectFaceParam;
			}
			for (int k = 0; k < 3; k++)
			{
				Station[i].paramFile.EnhanceParam = Station[i].paramFile_Backup.EnhanceParam;
			}
			for (int l = 0; l < 20; l++)
			{
				Station[i].paramFile.HSLocateParam[l] = Station[i].paramFile_Backup.HSLocateParam[l];
				Station[i].paramFile.VSLocateParam[l] = Station[i].paramFile_Backup.VSLocateParam[l];
				Station[i].paramFile.AdvLocateParam[l] = Station[i].paramFile_Backup.AdvLocateParam[l];
				Station[i].paramFile.MeaWidthParam[l] = Station[i].paramFile_Backup.MeaWidthParam[l];
				Station[i].paramFile.MeaHeightParam[l] = Station[i].paramFile_Backup.MeaHeightParam[l];
				Station[i].paramFile.SmoothParam[l] = Station[i].paramFile_Backup.SmoothParam[l];
				Station[i].paramFile.BlockAvgParam[l] = Station[i].paramFile_Backup.BlockAvgParam[l];
				Station[i].paramFile.DifferenceParam[l] = Station[i].paramFile_Backup.DifferenceParam[l];
				Station[i].paramFile.LineAnalysisParam[l] = Station[i].paramFile_Backup.LineAnalysisParam[l];
				Station[i].paramFile.MatchParam[l] = Station[i].paramFile_Backup.MatchParam[l];
				Station[i].paramFile.patternDB[l] = Station[i].paramFile_Backup.patternDB[l];
			}
			for (int m = 0; m < 30; m++)
			{
				Station[i].paramFile.MaskParam[m] = Station[i].paramFile_Backup.MaskParam[m];
				Station[i].paramFile.BinaryParam[m] = Station[i].paramFile_Backup.BinaryParam[m];
			}
			for (int n = 0; n < 40; n++)
			{
				Station[i].paramFile.AdvLocateMParam[n] = Station[i].paramFile_Backup.AdvLocateMParam[n];
			}
		}
		Btn_TestAll_Click(this, null);
		btn_CancleSave.Background = System.Windows.Media.Brushes.Black;
	}

	public List<ParetoSatisicsItem> GetParetoChart()
	{
		ParetoChartWindow paretoChartWindow = new ParetoChartWindow();
		paretoChartWindow.SetInspection(Station);
		paretoChartWindow.SetRecipe(ThroughputFile, offline: false);
		ParetoChartFile = paretoChartWindow.GetParetoChart();
		return ParetoChartFile;
	}

	private void Btn_AIPredictTool_Click(object sender, RoutedEventArgs e)
	{
		AIToolWindow aIToolWindow = new AIToolWindow();
		aIToolWindow.LoadLanguage(LanguageType);
		aIToolWindow.SetPredictMode(UsePredictMode: true);
		aIToolWindow.SetThroghputFile(ThroughputFile);
		aIToolWindow.SetInspection(Station, Station[StationIndex], StationIndex);
		aIToolWindow.ShowDialog();
		if (Station[StationIndex].aiPredictTools != null && chk_AIPredictToolON.IsChecked == true && (aIToolWindow.ReloadModel[0] || aIToolWindow.ReloadModel[1]))
		{
			LoadingAIModel[StationIndex] = true;
			for (int i = 0; i < 8; i++)
			{
				if (i != StationIndex)
				{
					if (aIToolWindow.ReloadModel[0] && (Station[i].paramFile.AIPredictParam.ModelName_A == Station[StationIndex].paramFile.AIPredictParam.ModelName_A || Station[i].paramFile.AIPredictParam.ModelName_B == Station[StationIndex].paramFile.AIPredictParam.ModelName_A))
					{
						Station[i].paramFile.AIPredictParam.Model_A_Changed = true;
						Station[i].paramFile.AIPredictParam.Model_B_Changed = true;
						LoadingAIModel[i] = true;
					}
					if (aIToolWindow.ReloadModel[1] && (Station[i].paramFile.AIPredictParam.ModelName_A == Station[StationIndex].paramFile.AIPredictParam.ModelName_B || Station[i].paramFile.AIPredictParam.ModelName_B == Station[StationIndex].paramFile.AIPredictParam.ModelName_B))
					{
						Station[i].paramFile.AIPredictParam.Model_A_Changed = true;
						Station[i].paramFile.AIPredictParam.Model_B_Changed = true;
						LoadingAIModel[i] = true;
					}
				}
			}
			grd_AILoading.Visibility = Visibility.Visible;
			UsePredictEnhance = AI_MODE.EM_Predict;
			AILoadingPauseEvent.Set();
		}
		Station[StationIndex].paramFile.AIPredictParam.ValueChangeFlag = true;
		UpdateAIImageStatus(StationIndex);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_DrawMask_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
			return;
		}
		if (!Station[StationIndex].paramFile.AIEnhanceToolON)
		{
			System.Windows.MessageBox.Show("尚未啟用AI前處理工具。", "警告");
			return;
		}
		string text = "";
		if (Station[StationIndex].paramFile.AIEnhanceToolON)
		{
			text = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
		}
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		string x_ok_path = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text2 = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string y_ok_path = string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text3 = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			Btn_TestAll_Click(this, null);
			if (!CheckBuffer() || Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				System.Windows.MessageBox.Show("AI影像生成前錯誤，請檢查定位等工具是否有錯誤。", "警告");
				return;
			}
			int okcount = 0;
			int ngcount = 0;
			GetUNetAIImageNumbers(x_ok_path, text2, y_ok_path, text3, ref okcount, ref ngcount);
			Mat aIMat = Station[StationIndex].GetAIMat();
			if (aIMat.Width > 0 && aIMat.Height > 0)
			{
				AIMarkWindow aIMarkWindow = new AIMarkWindow();
				aIMarkWindow.SetInspection(Station[StationIndex], StationIndex);
				aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
				aIMarkWindow.SetSAM(mSam);
				aIMarkWindow.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
				aIMarkWindow.LoadLanguage(LanguageType);
				aIMarkWindow.SetOrgImage(Station[StationIndex].buffers.Dst);
				aIMarkWindow.SetMat(Station[StationIndex].GetAIMat(), Station[StationIndex].paramFile.ZoomMode);
				aIMarkWindow.SetNGMarkedFolder(text2, text3);
				aIMarkWindow.SetColorSelect(Station[StationIndex].paramFile.ColorSelect);
				aIMarkWindow.SetPreviewMode(Station[StationIndex].paramFile.PreviewMode);
				aIMarkWindow.SetPenSize(Station[StationIndex].paramFile.PenSize);
				aIMarkWindow.SetContinueWrite(Station[StationIndex].paramFile.EnableContinueWrite);
				aIMarkWindow.SetAutoDrawIgnore(Station[StationIndex].paramFile.EnableAutoIgnoreDraw, Station[StationIndex].paramFile.AutoIgnoreSize);
				aIMarkWindow.SetCaption(Station[StationIndex].paramFile.DrawCaition, Station[StationIndex].paramFile.SaveAsFolder);
				aIMarkWindow.ShowDialog();
				string[] s = new string[6];
				aIMarkWindow.GetCaption(ref s, ref Station[StationIndex].paramFile.SaveAsFolder);
				for (int i = 0; i < 6; i++)
				{
					if (!(Station[StationIndex].paramFile.DrawCaition[i] != s[i]))
					{
						continue;
					}
					Station[StationIndex].paramFile.DrawCaition[i] = s[i];
					for (int j = 0; j < 8; j++)
					{
						if (j != StationIndex && Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A == Station[j].paramFile.AIEnhanceParam.ModelName_A)
						{
							Station[j].paramFile.DrawCaition[i] = Station[StationIndex].paramFile.DrawCaition[i];
						}
					}
				}
				Station[StationIndex].paramFile.PreviewMode = aIMarkWindow.GetPreviewMode();
				Station[StationIndex].paramFile.ZoomMode = aIMarkWindow.GetZoomMode();
				Station[StationIndex].paramFile.ColorSelect = aIMarkWindow.GetColorSelect();
				Station[StationIndex].paramFile.PenSize = aIMarkWindow.GetPenSize();
				Station[StationIndex].paramFile.EnableContinueWrite = aIMarkWindow.GetContinueWrite();
				Station[StationIndex].paramFile.EnableAutoIgnoreDraw = aIMarkWindow.GetEnableAutoDrawIgnore();
				Station[StationIndex].paramFile.AutoIgnoreSize = aIMarkWindow.GetAutoDrawIgnoreSize();
			}
			UpdateAIImageStatus(StationIndex);
			Btn_TestAll_Click(this, null);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_DrawMask_Click()" + ex.ToString());
		}
	}

	private void Btn_AI_Function_Click(object sender, RoutedEventArgs e)
	{
		if (grd_AIFunction.Width > 200.0)
		{
			grd_AIFunction.Width = 60.0;
		}
		else
		{
			if (ThroughputFile.param.EnableAIPasswordProtect)
			{
				PasswordWindow passwordWindow = new PasswordWindow();
				passwordWindow.SetPassWord(ThroughputFile.param.AIPassword);
				passwordWindow.ShowDialog();
				if (!passwordWindow.GetLoginStatus())
				{
					return;
				}
				AILoginSuccess = true;
			}
			grd_AIFunction.Width = 580.0;
			UpdateAIImageStatus(StationIndex);
		}
		if (chk_AIEnhanceToolON.IsChecked == true)
		{
			lab_ModelName.Content = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
		}
		else if (chk_AIPredictToolON.IsChecked == true)
		{
			lab_ModelName.Content = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
		}
		else
		{
			lab_ModelName.Content = "";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenFolder_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
			return;
		}
		if (!Station[StationIndex].paramFile.AIEnhanceToolON && !Station[StationIndex].paramFile.AIPredictToolON)
		{
			System.Windows.MessageBox.Show("尚未啟用AI前處理或AI檢測工具。", "警告");
			return;
		}
		string text = "";
		text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		try
		{
			string text2 = string.Format("{0}TrainingImage\\{1}", AppDomain.CurrentDomain.BaseDirectory, text);
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
			CVBuffer.SaveLog("ToolsWindow-Btn_OpenFolder_Click()" + ex.ToString());
		}
	}

	private void Btn_SetupAIFolder_Click(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateAIImageStatus(int index)
	{
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON || (!Station[StationIndex].paramFile.AIEnhanceToolON && !Station[StationIndex].paramFile.AIPredictToolON))
		{
			return;
		}
		string text = "";
		text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
		if (text == "")
		{
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		lab_ModelName.Content = text;
		string x_ok_path = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string x_ng_path = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string y_ok_path = string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string y_ng_path = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		int okcount = 0;
		int ngcount = 0;
		bool flag = true;
		try
		{
			if (chk_AIPredictToolON.IsChecked != true)
			{
				GetUNetAIImageNumbers(x_ok_path, x_ng_path, y_ok_path, y_ng_path, ref okcount, ref ngcount);
				btn_AddToNG.Visibility = Visibility.Hidden;
				es_202.Visibility = Visibility.Hidden;
				lab_Error202.Visibility = Visibility.Hidden;
				es_201.Visibility = Visibility.Visible;
				lab_Error201.Visibility = Visibility.Visible;
				lab_Error201.Content = okcount.ToString();
				btn_DrawMask.Visibility = Visibility.Visible;
				es_203.Visibility = Visibility.Visible;
				lab_Error203.Visibility = Visibility.Visible;
				lab_Error203.Content = ngcount.ToString();
			}
			else
			{
				GetUNetAIImageNumbers(x_ok_path, x_ng_path, y_ok_path, y_ng_path, ref okcount, ref ngcount);
				lab_Error201.Content = okcount.ToString();
				lab_Error202.Content = ngcount.ToString();
				btn_AddToNG.Visibility = Visibility.Visible;
				es_202.Visibility = Visibility.Visible;
				lab_Error202.Visibility = Visibility.Visible;
				btn_DrawMask.Visibility = Visibility.Hidden;
				es_203.Visibility = Visibility.Hidden;
				lab_Error203.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-UpdateAiImageStatus()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetUNetAIImageNumbers(string x_ok_path, string x_ng_path, string y_ok_path, string y_ng_path, ref int okcount, ref int ngcount)
	{
		okcount = (ngcount = 0);
		if (x_ok_path == "" || x_ok_path == null || x_ng_path == "" || x_ng_path == null)
		{
			okcount = (ngcount = 0);
			return false;
		}
		try
		{
			if (!Directory.Exists(x_ok_path))
			{
				Directory.CreateDirectory(x_ok_path);
				okcount = (ngcount = 0);
				if (!Directory.Exists(x_ok_path))
				{
					return false;
				}
			}
			if (!Directory.Exists(x_ng_path))
			{
				Directory.CreateDirectory(x_ng_path);
				okcount = (ngcount = 0);
				if (!Directory.Exists(x_ng_path))
				{
					return false;
				}
			}
			if (!Directory.Exists(y_ok_path))
			{
				Directory.CreateDirectory(y_ok_path);
				okcount = (ngcount = 0);
				if (!Directory.Exists(y_ok_path))
				{
					return false;
				}
			}
			if (!Directory.Exists(y_ng_path))
			{
				Directory.CreateDirectory(y_ng_path);
				okcount = (ngcount = 0);
				if (!Directory.Exists(y_ng_path))
				{
					return false;
				}
			}
			string[] files = Directory.GetFiles(x_ok_path, "*.*", SearchOption.TopDirectoryOnly);
			okcount = files.Length;
			string[] files2 = Directory.GetFiles(x_ng_path, "*.*", SearchOption.TopDirectoryOnly);
			ngcount = files2.Length;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-GetUNetAIImageNumbers()" + ex.ToString());
			return false;
		}
	}

	private void RemoveAIPreviewImage()
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AddToOK_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
			return;
		}
		if (!Station[StationIndex].paramFile.AIEnhanceToolON && !Station[StationIndex].paramFile.AIPredictToolON)
		{
			System.Windows.MessageBox.Show("尚未啟用AI前處理或AI檢測工具。", "警告");
			return;
		}
		string text = "";
		text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		string text2 = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string x_ng_path = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text3 = string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string y_ng_path = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			Btn_TestAll_Click(this, null);
			if (!CheckBuffer() || Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				System.Windows.MessageBox.Show("AI影像生成前錯誤，請檢查定位、進階定位等工具是否有錯誤。", "警告");
				return;
			}
			int okcount = 0;
			int ngcount = 0;
			GetUNetAIImageNumbers(text2, x_ng_path, text3, y_ng_path, ref okcount, ref ngcount);
			if (Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				return;
			}
			DateTime now = DateTime.Now;
			string text4 = string.Format("OK-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string arg = string.Format("OK-{0}-{1}.jpg", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string text5 = "";
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			if (!Directory.Exists(text2))
			{
				System.Windows.MessageBox.Show(string.Format("AI OK資料夾建立失敗，請重新設定AI影像路徑。({0})", text2), "警告");
				return;
			}
			text5 = string.Format("{0}{1}", text2, text4);
			Station[StationIndex].maskTool.AIMat.SaveImage(text5);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			if (!Directory.Exists(text3))
			{
				System.Windows.MessageBox.Show(string.Format("AI OK資料夾建立失敗，請重新設定AI影像路徑。({0})", text3), "警告");
				return;
			}
			text5 = string.Format("{0}{1}", text3, text4);
			if (Station[StationIndex].paramFile.AIPredictToolON)
			{
				Station[StationIndex].maskTool.AIMat.SaveImage(text5);
			}
			else if (Station[StationIndex].paramFile.AIEnhanceParam.DnetMode_A != 0)
			{
				Mat mat = Station[StationIndex].maskTool.AIMat.Clone();
				mat.SetTo(0.0);
				mat.SaveImage(text5);
			}
			else
			{
				Station[StationIndex].maskTool.AIMat.SaveImage(text5);
			}
			string text6 = string.Format("{0}TrainingImage\\{1}\\Origional\\{2}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text, StationIndex + 1);
			if (!Directory.Exists(text6))
			{
				Directory.CreateDirectory(text6);
			}
			if (Directory.Exists(text6))
			{
				text5 = string.Format("{0}{1}", text6, arg);
				Station[StationIndex].buffers.Dst.SaveImage(text5);
			}
			string str = ((LanguageType == 0) ? string.Format("{0} Save AI Image Finish", text4) : ((LanguageType != 1) ? string.Format("{0} AI 影像储存成功!", text4) : string.Format("{0} AI 影像儲存成功", text4)));
			ShowLabMessage(str);
			UpdateAIImageStatus(StationIndex);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_AddToOK_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AddToNG_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
			return;
		}
		if (!Station[StationIndex].paramFile.AIEnhanceToolON)
		{
			System.Windows.MessageBox.Show("尚未啟用AI檢測工具。", "警告");
			return;
		}
		string text = "";
		text = Station[StationIndex].paramFile.AIPredictParam.ModelName_A;
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text2 = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			Btn_TestAll_Click(this, null);
			if (!CheckBufferAndRef() || Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				System.Windows.MessageBox.Show("AI影像生成前錯誤，請檢查定位、進階定位等工具是否有錯誤。", "警告");
				return;
			}
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			if (Directory.Exists(text2))
			{
				DateTime now = DateTime.Now;
				string text3 = text2 + "NG-" + now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
				Station[StationIndex].maskTool.AIMat.SaveImage(text3);
				string str = ((LanguageType == 0) ? string.Format("{0} Save AI Image Finish", text3) : ((LanguageType != 1) ? string.Format("{0} AI 影像储存成功!", text3) : string.Format("{0} AI 影像儲存成功", text3)));
				ShowLabMessage(str);
				UpdateAIImageStatus(StationIndex);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_AddToNG_Click()" + ex.ToString());
		}
	}

	public bool GetEnableMultiParetoMode()
	{
		return EnableMultiParetoMode;
	}

	public List<TOOLSENABLEITEM> GetEnabledItem()
	{
		AllToolEnableItem.Clear();
		for (int i = 0; i < 8; i++)
		{
			AllToolEnableItem.AddRange(Station[i].paramFile.GetEnabledItem());
		}
		return AllToolEnableItem;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadPrivateDBParam()
	{
		if (PrivateDBIni == null)
		{
			return false;
		}
		for (int i = 0; i < 210; i++)
		{
			string section = "PrivatePatternDB" + i;
			PrivatePatternDB[i].ValueChangeFlag = false;
			PrivatePatternDB[i].PatternON = PrivateDBIni.GetBoolean(section, "PatternON");
			PrivatePatternDB[i].PatternName = PrivateDBIni.GetValue(section, "PatternName");
			PrivatePatternDB[i].PatternPath = PrivateDBIni.GetValue(section, "PatternPath");
			PrivatePatternDB[i].PatternChannel = PrivateDBIni.GetInteger(section, "PatternChannel");
			string fileName = System.IO.Path.GetFileName(PrivatePatternDB[i].PatternPath);
			string text = System.IO.Path.GetDirectoryName(PrivateDBIni.TheFile) + "\\" + fileName;
			if (PrivatePatternDB[i].PatternON && text != "")
			{
				if (File.Exists(text))
				{
					if (PrivatePatternDB[i].PatternPath != text)
					{
						PrivatePatternDB[i].PatternPath = text;
						PrivatePatternDB[i].ValueChangeFlag = true;
					}
					PrivatePatternDB[i].PatternImage = CVBuffer.ImRead(text, ImreadModes.Grayscale);
					PrivatePatternDB[i].PatternRect = new OpenCvSharp.Rect(0, 0, PrivatePatternDB[i].PatternImage.Width, PrivatePatternDB[i].PatternImage.Height);
					if (!PrivatePatternDB[i].PatternImage.Empty())
					{
						PrivatePatternDB[i].PatternON = true;
						if (!fileName.Contains("-") && fileName.Contains("."))
						{
							int startIndex = fileName.IndexOf(".");
							string value = string.Format("-{0}", PrivatePatternDB[i].PatternChannel);
							string text2 = fileName.Insert(startIndex, value);
							PrivatePatternDB[i].PatternPath = System.IO.Path.GetDirectoryName(PrivateDBIni.TheFile) + "\\" + text2;
							Cv2.ImWrite(PrivatePatternDB[i].PatternPath, PrivatePatternDB[i].PatternImage);
							PrivatePatternDB[i].ValueChangeFlag = true;
						}
					}
					else
					{
						PrivatePatternDB[i].PatternON = false;
					}
				}
				else
				{
					PrivatePatternDB[i].PatternON = false;
				}
			}
			else
			{
				PrivatePatternDB[i].PatternON = false;
				PrivatePatternDB[i].PatternImage = new Mat();
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WritePrivateDBParam()
	{
		if (PrivateDBIni == null)
		{
			return false;
		}
		for (int i = 0; i < 210; i++)
		{
			if (PrivatePatternDB[i].ValueChangeFlag)
			{
				string section = "PrivatePatternDB" + i;
				PrivateDBIni.WriteBool(section, "PatternON", PrivatePatternDB[i].PatternON);
				PrivateDBIni.WriteString(section, "PatternName", PrivatePatternDB[i].PatternName);
				PrivateDBIni.WriteString(section, "PatternPath", PrivatePatternDB[i].PatternPath);
				PrivateDBIni.WriteInt(section, "PatternChannel", PrivatePatternDB[i].PatternChannel);
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadPublicDBParam()
	{
		if (DBIni == null)
		{
			return false;
		}
		for (int i = 0; i < 210; i++)
		{
			string section = "PublicPatternDB" + i;
			PublicPatternDB[i].ValueChangeFlag = false;
			PublicPatternDB[i].PatternON = DBIni.GetBoolean(section, "PatternON");
			PublicPatternDB[i].PatternName = DBIni.GetValue(section, "PatternName");
			PublicPatternDB[i].PatternPath = DBIni.GetValue(section, "PatternPath");
			PublicPatternDB[i].PatternChannel = DBIni.GetInteger(section, "PatternChannel");
			string fileName = System.IO.Path.GetFileName(PublicPatternDB[i].PatternPath);
			string text = System.IO.Path.GetDirectoryName(DBIni.TheFile) + "\\" + fileName;
			if (PublicPatternDB[i].PatternON && text != "")
			{
				if (File.Exists(text))
				{
					if (PublicPatternDB[i].PatternPath != text)
					{
						PublicPatternDB[i].PatternPath = text;
						PublicPatternDB[i].ValueChangeFlag = true;
					}
					PublicPatternDB[i].PatternImage = CVBuffer.ImRead(text, ImreadModes.Grayscale);
					PublicPatternDB[i].PatternRect = new OpenCvSharp.Rect(0, 0, PublicPatternDB[i].PatternImage.Width, PublicPatternDB[i].PatternImage.Height);
					if (!PublicPatternDB[i].PatternImage.Empty())
					{
						PublicPatternDB[i].PatternON = true;
						if (!fileName.Contains("-") && fileName.Contains("."))
						{
							int startIndex = fileName.IndexOf(".");
							string value = string.Format("-{0}", PublicPatternDB[i].PatternChannel);
							string text2 = fileName.Insert(startIndex, value);
							PublicPatternDB[i].PatternPath = System.IO.Path.GetDirectoryName(DBIni.TheFile) + "\\" + text2;
							Cv2.ImWrite(PublicPatternDB[i].PatternPath, PublicPatternDB[i].PatternImage);
							PublicPatternDB[i].ValueChangeFlag = true;
						}
					}
					else
					{
						PublicPatternDB[i].PatternON = false;
					}
				}
				else
				{
					PublicPatternDB[i].PatternON = false;
				}
			}
			else
			{
				PublicPatternDB[i].PatternON = false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WritePublicDBParam()
	{
		if (DBIni == null)
		{
			return false;
		}
		for (int i = 0; i < 210; i++)
		{
			if (PublicPatternDB[i].ValueChangeFlag)
			{
				string section = "PublicPatternDB" + i;
				DBIni.WriteBool(section, "PatternON", PublicPatternDB[i].PatternON);
				DBIni.WriteString(section, "PatternName", PublicPatternDB[i].PatternName);
				DBIni.WriteString(section, "PatternPath", PublicPatternDB[i].PatternPath);
				DBIni.WriteInt(section, "PatternChannel", PublicPatternDB[i].PatternChannel);
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AITrain_Click(object sender, RoutedEventArgs e)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "license.lic");
		string path2 = string.Format("{0}{1}", "D:\\", "license.lic");
		if (!File.Exists(path) && !File.Exists(path2))
		{
			System.Windows.MessageBox.Show(string.Format("[AI] 認證失敗，找不到License認證檔案。\n"), "錯誤");
			string path3 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training.bat");
			string arg = string.Format("cd Training\ncd train\ntrain -i ..\\Model\\navs.ini -m ..\\Model\\navs.h5 -s 0 -v DAVS_V1 -r Train -e {0} -l 6 -a {1}", 100, 0);
			using StreamWriter streamWriter = new StreamWriter(path3, append: true);
			streamWriter.Write("{0}", arg);
			streamWriter.WriteLine();
			return;
		}
		AI_MODE[] array = new AI_MODE[8];
		bool[] array2 = new bool[8];
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIPredictToolON)
			{
				array[i] = AI_MODE.EM_Predict;
				array2[i] = Station[i].paramFile.AIPredictParam.EnableSelectFace;
			}
			else if (Station[i].paramFile.AIEnhanceToolON)
			{
				array[i] = AI_MODE.EM_Enhance;
				array2[i] = Station[i].paramFile.AIEnhanceParam.EnableSelectFace;
			}
			else
			{
				array[i] = AI_MODE.EM_NoUse;
				array2[i] = false;
			}
		}
		if (RemoteMode)
		{
			ThroughputFile.ReadThroughputParam();
		}
		AITrainingWIndow aITrainingWIndow = new AITrainingWIndow();
		aITrainingWIndow.SetSAM(mSam);
		LastV = 0;
		string[] array3 = new string[8];
		string[] array4 = new string[8];
		string[] array5 = new string[8];
		string[] array6 = new string[8];
		for (int j = 0; j < 8; j++)
		{
			string text = "";
			if (array[j] == AI_MODE.EM_Enhance)
			{
				text = Station[j].paramFile.AIEnhanceParam.ModelName_A;
			}
			else if (array[j] == AI_MODE.EM_Predict)
			{
				text = Station[j].paramFile.AIPredictParam.ModelName_A;
			}
			if (text == "")
			{
				array3[j] = "";
				array4[j] = "";
				array5[j] = "";
				array6[j] = "";
				continue;
			}
			text = System.IO.Path.GetFileNameWithoutExtension(text);
			array3[j] = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
			array4[j] = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
			array5[j] = string.Format("{0}TrainingImage\\{1}\\X\\", AppDomain.CurrentDomain.BaseDirectory, text);
			array6[j] = string.Format("{0}TrainingImage\\{1}\\Y\\", AppDomain.CurrentDomain.BaseDirectory, text);
			if (Directory.Exists(array3[j]))
			{
				Directory.CreateDirectory(array3[j]);
			}
			if (Directory.Exists(array4[j]))
			{
				Directory.CreateDirectory(array4[j]);
			}
			if (Directory.Exists(array5[j]))
			{
				Directory.CreateDirectory(array5[j]);
			}
			if (Directory.Exists(array6[j]))
			{
				Directory.CreateDirectory(array6[j]);
			}
		}
		aITrainingWIndow.SetMode(array);
		aITrainingWIndow.SetThroughputFile(ThroughputFile);
		aITrainingWIndow.SetInspection(Station, array3, array4, array5, array6);
		aITrainingWIndow.ShowDialog();
		ThroughputFile.WriteThroughputParam();
		DateTime now = DateTime.Now;
		string text2 = string.Format("{0}Log\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, now.ToString("yyyyMM"));
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		using (StreamWriter streamWriter2 = new StreamWriter(string.Format("{0}{1}-AITraining.log", text2, now.ToString("yyyyMMdd")), append: true))
		{
			streamWriter2.Write(aITrainingWIndow.TrainingMsg);
			streamWriter2.WriteLine();
		}
		for (int k = 0; k < 8; k++)
		{
			LoadingAIModel[k] = true;
		}
		grd_AILoading.Visibility = Visibility.Visible;
		AILoadingPauseEvent.Set();
		UpdateAIImageStatus(StationIndex);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AIOKDB_Click(object sender, RoutedEventArgs e)
	{
		string modelName_A;
		if (chk_AIEnhanceToolON.IsChecked == true)
		{
			modelName_A = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
		}
		else
		{
			if (chk_AIPredictToolON.IsChecked != true)
			{
				return;
			}
			modelName_A = Station[StationIndex].paramFile.AIPredictParam.ModelName_A;
		}
		if (!(modelName_A == ""))
		{
			modelName_A = System.IO.Path.GetFileNameWithoutExtension(modelName_A);
			string text = string.Format("{0}TrainingImage\\{1}\\Origional\\{2}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, modelName_A, StationIndex + 1);
			btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseFileBufList.Background = System.Windows.Media.Brushes.Yellow;
			btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
			btn_DelAIOKDB.Visibility = Visibility.Visible;
			CurrentAIDBPath[StationIndex] = text;
			CheckFileList(text);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AINGDB_Click(object sender, RoutedEventArgs e)
	{
		string modelName_A;
		if (chk_AIEnhanceToolON.IsChecked == true)
		{
			modelName_A = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
		}
		else
		{
			if (chk_AIPredictToolON.IsChecked != true)
			{
				return;
			}
			modelName_A = Station[StationIndex].paramFile.AIPredictParam.ModelName_A;
		}
		if (!(modelName_A == ""))
		{
			modelName_A = System.IO.Path.GetFileNameWithoutExtension(modelName_A);
			string text = string.Format("{0}TrainingImage\\{1}\\Origional\\{2}\\NG\\", AppDomain.CurrentDomain.BaseDirectory, modelName_A, StationIndex + 1);
			btn_UseRecentBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
			btn_UseFileBufList.Background = System.Windows.Media.Brushes.Yellow;
			btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
			btn_DelAIOKDB.Visibility = Visibility.Visible;
			CurrentAIDBPath[StationIndex] = text;
			CheckFileList(text);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_DelAIOKDB_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentAIDBPath[StationIndex] == "" || CurrentAIDBPath[StationIndex] == null)
		{
			return;
		}
		string modelName_A;
		if (chk_AIEnhanceToolON.IsChecked == true)
		{
			modelName_A = Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A;
		}
		else
		{
			if (chk_AIPredictToolON.IsChecked != true)
			{
				return;
			}
			modelName_A = Station[StationIndex].paramFile.AIPredictParam.ModelName_A;
		}
		if (modelName_A == "")
		{
			return;
		}
		modelName_A = System.IO.Path.GetFileNameWithoutExtension(modelName_A);
		try
		{
			if (filelist[StationIndex][fileIndex].Length <= 0)
			{
				return;
			}
			string text = filelist[StationIndex][fileIndex];
			System.IO.Path.GetFileName(filelist[StationIndex][fileIndex]);
			System.IO.Path.GetDirectoryName(filelist[StationIndex][fileIndex]);
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? System.Windows.MessageBox.Show("Delete current AI database image ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? System.Windows.MessageBox.Show("当前的AI资料库影像将被删除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : System.Windows.MessageBox.Show("當前的AI資料庫影像將被刪除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text);
				string[] files = Directory.GetFiles(string.Format("{0}TrainingImage\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, modelName_A), "*.*", SearchOption.AllDirectories);
				string fileName = System.IO.Path.GetFileName(text);
				for (int i = 0; i < files.Length; i++)
				{
					if (System.IO.Path.GetFileName(files[i]) == fileName)
					{
						FileInfo fileInfo2 = new FileInfo(files[i]);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
						File.Delete(files[i]);
					}
				}
			}
			UpdateAIImageStatus(StationIndex);
			CheckFileList(CurrentAIDBPath[StationIndex]);
			Btn_NextImage_Click(this, null);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_DelAIOKDB_Click()" + ex.ToString());
		}
	}

	private void Btn_AutoMatch_Click(object sender, RoutedEventArgs e)
	{
		AutoMatchWindow autoMatchWindow = new AutoMatchWindow();
		autoMatchWindow.LoadLanguage(LanguageType);
		autoMatchWindow.SetInspection(Station[StationIndex], RecipeName);
		autoMatchWindow.SetLastText("");
		autoMatchWindow.ShowDialog();
		lab_FontToolText.Content = Station[StationIndex].paramFile.FontToolParam.AllFont;
	}

	private void Btn_LightMonitorTool_Click(object sender, RoutedEventArgs e)
	{
		if (CheckBuffer())
		{
			LightMonitorWindow lightMonitorWindow = new LightMonitorWindow();
			lightMonitorWindow.LoadLanguage(LanguageType);
			lightMonitorWindow.SetInspection(0, Station[StationIndex]);
			lightMonitorWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
			lightMonitorWindow.ShowDialog();
			Btn_TestAll_Click(this, null);
		}
	}

	private void B1_Click(object sender, RoutedEventArgs e)
	{
		Station[StationIndex].SetUpdateLightMonitorBaseValueON(enabled: true);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void AutoMarkedUseAIProphetResult()
	{
		string text = "";
		text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("請先設定AI遮罩。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		if (chk_AIProphetToolON.IsChecked == false)
		{
			System.Windows.MessageBox.Show("AI先知未開啟。", "警告");
			return;
		}
		grd_AILoading.Visibility = Visibility.Visible;
		Mat heatMapBinaryImage = Station[StationIndex].aiProphetTools.GetHeatMapBinaryImage();
		OpenCvSharp.Rect samAIRect = Station[StationIndex].aiProphetTools.findMaxBlob(heatMapBinaryImage);
		samAIRect.Left += Station[StationIndex].LocateRect.Left + Station[StationIndex].paramFile.AIProphetParam.Track_L;
		samAIRect.Top += Station[StationIndex].LocateRect.Top + Station[StationIndex].paramFile.AIProphetParam.Track_T;
		samAIRect.Left -= Station[StationIndex].maskTool.AIRect.Left;
		samAIRect.Top -= Station[StationIndex].maskTool.AIRect.Top;
		if (samAIRect.Width < 10 && samAIRect.Height < 10)
		{
			System.Windows.MessageBox.Show(string.Format("AI先知檢出區域過小。"), "警告");
			return;
		}
		AIMarkWindow aIMarkWindow = new AIMarkWindow();
		aIMarkWindow.SetInspection(Station[StationIndex], StationIndex);
		aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
		aIMarkWindow.SetSAM(mSam);
		aIMarkWindow.SetOrgImage(Station[StationIndex].buffers.Dst);
		Mat aIMat = Station[StationIndex].GetAIMat();
		if (aIMat.Width <= 0 || aIMat.Height <= 0)
		{
			return;
		}
		aIMarkWindow.OpenAIModelAndImage(aIMat);
		Mat[] mv = new Mat[3];
		Cv2.Split(aIMat, out mv);
		aIMarkWindow.SetSamAIRect(samAIRect);
		mv[1].SetTo(0.0);
		mv[0].SetTo(0.0);
		mv[2] = aIMarkWindow.OutPutAIDrawMat();
		aIMarkWindow.Saved = true;
		aIMarkWindow.Close();
		Mat mat = new Mat();
		Cv2.Merge(mv, mat);
		string x_ok_path = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text2 = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string y_ok_path = string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text3 = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			int okcount = 0;
			int ngcount = 0;
			GetUNetAIImageNumbers(x_ok_path, text2, y_ok_path, text3, ref okcount, ref ngcount);
			if (Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				return;
			}
			DateTime now = DateTime.Now;
			string text4 = string.Format("NG-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string arg = string.Format("NG-{0}-{1}.jpg", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string text5 = "";
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			if (!Directory.Exists(text2))
			{
				System.Windows.MessageBox.Show(string.Format("AI NG資料夾建立失敗，請重新設定AI影像路徑。({0})", text2), "警告");
				return;
			}
			text5 = string.Format("{0}{1}", text2, text4);
			Station[StationIndex].maskTool.AIMat.SaveImage(text5);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			if (!Directory.Exists(text3))
			{
				System.Windows.MessageBox.Show(string.Format("AI NG資料夾建立失敗，請重新設定AI影像路徑。({0})", text3), "警告");
				return;
			}
			text5 = string.Format("{0}{1}", text3, text4);
			mat.SaveImage(text5);
			string text6 = string.Format("{0}TrainingImage\\{1}\\Origional\\{2}\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text, StationIndex + 1);
			if (!Directory.Exists(text6))
			{
				Directory.CreateDirectory(text6);
			}
			if (Directory.Exists(text6))
			{
				text5 = string.Format("{0}{1}", text6, arg);
				Station[StationIndex].buffers.Dst.SaveImage(text5);
			}
			string str = ((LanguageType == 0) ? string.Format("{0} Save AI Image Finish", text4) : ((LanguageType != 1) ? string.Format("{0} AI 影像储存成功!", text4) : string.Format("{0} AI 影像儲存成功", text4)));
			ShowLabMessage(str);
			UpdateAIImageStatus(StationIndex);
		}
		catch
		{
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void B2_Click(object sender, RoutedEventArgs e)
	{
		string text = "";
		text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		if (chk_AIProphetToolON.IsChecked == false)
		{
			System.Windows.MessageBox.Show("AI先知未開啟。", "警告");
			return;
		}
		Mat heatMapBinaryImage = Station[StationIndex].aiProphetTools.GetHeatMapBinaryImage();
		OpenCvSharp.Rect samAIRect = Station[StationIndex].aiProphetTools.findMaxBlob(heatMapBinaryImage);
		samAIRect.Left += Station[StationIndex].LocateRect.Left + Station[StationIndex].paramFile.AIProphetParam.Track_L;
		samAIRect.Top += Station[StationIndex].LocateRect.Top + Station[StationIndex].paramFile.AIProphetParam.Track_T;
		samAIRect.Left -= Station[StationIndex].maskTool.AIRect.Left;
		samAIRect.Top -= Station[StationIndex].maskTool.AIRect.Top;
		if (samAIRect.Width < 10 && samAIRect.Height < 10)
		{
			System.Windows.MessageBox.Show(string.Format("AI先知檢出區域過小。"), "警告");
			return;
		}
		AIMarkWindow aIMarkWindow = new AIMarkWindow();
		aIMarkWindow.SetInspection(Station[StationIndex], StationIndex);
		aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
		aIMarkWindow.SetSAM(mSam);
		aIMarkWindow.SetOrgImage(Station[StationIndex].buffers.Dst);
		Mat aIMat = Station[StationIndex].GetAIMat();
		aIMarkWindow.OpenAIModelAndImage(aIMat);
		Mat[] mv = new Mat[3];
		Cv2.Split(aIMat, out mv);
		aIMarkWindow.SetSamAIRect(samAIRect);
		mv[2] = aIMarkWindow.OutPutAIDrawMat();
		aIMarkWindow.Saved = true;
		aIMarkWindow.Close();
		Mat mat = new Mat();
		mv[1].SetTo(0.0);
		mv[0].SetTo(0.0);
		Cv2.Merge(mv, mat);
		string x_ok_path = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text2 = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string y_ok_path = string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string text3 = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
		try
		{
			int okcount = 0;
			int ngcount = 0;
			GetUNetAIImageNumbers(x_ok_path, text2, y_ok_path, text3, ref okcount, ref ngcount);
			if (Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				return;
			}
			DateTime now = DateTime.Now;
			string text4 = string.Format("NG-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string arg = string.Format("NG-{0}-{1}.jpg", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string text5 = "";
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			if (!Directory.Exists(text2))
			{
				System.Windows.MessageBox.Show(string.Format("AI NG資料夾建立失敗，請重新設定AI影像路徑。({0})", text2), "警告");
				return;
			}
			text5 = string.Format("{0}{1}", text2, text4);
			Station[StationIndex].maskTool.AIMat.SaveImage(text5);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			if (!Directory.Exists(text3))
			{
				System.Windows.MessageBox.Show(string.Format("AI NG資料夾建立失敗，請重新設定AI影像路徑。({0})", text3), "警告");
				return;
			}
			text5 = string.Format("{0}{1}", text3, text4);
			mat.SaveImage(text5);
			string text6 = string.Format("{0}TrainingImage\\{1}\\Origional\\{2}\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text, StationIndex + 1);
			if (!Directory.Exists(text6))
			{
				Directory.CreateDirectory(text6);
			}
			if (Directory.Exists(text6))
			{
				text5 = string.Format("{0}{1}", text6, arg);
				Station[StationIndex].buffers.Dst.SaveImage(text5);
			}
			string str = ((LanguageType == 0) ? string.Format("{0} Save AI Image Finish", text4) : ((LanguageType != 1) ? string.Format("{0} AI 影像储存成功!", text4) : string.Format("{0} AI 影像儲存成功", text4)));
			ShowLabMessage(str);
			UpdateAIImageStatus(StationIndex);
		}
		catch
		{
		}
	}

	private void B3_Click(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Chk_SaveFontAI_Click(object sender, RoutedEventArgs e)
	{
		if (!inited)
		{
			return;
		}
		System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
		if (checkBox.IsChecked == true)
		{
			for (int i = 0; i < 8; i++)
			{
				if (Station[i] != null)
				{
					if (checkBox.Uid == "0")
					{
						Station[i].paramFile.SaveAIFontImage = true;
					}
					else
					{
						Station[i].paramFile.SaveAIFontImage2 = true;
					}
				}
			}
			lab_SaveAIImage.Visibility = Visibility.Visible;
			lab_SaveAIImage.Content = "字元存圖開啟";
			btn_OpenAIFontFolder.Visibility = Visibility.Visible;
			return;
		}
		for (int j = 0; j < 8; j++)
		{
			if (Station[j] != null)
			{
				if (checkBox.Uid == "0")
				{
					Station[j].paramFile.SaveAIFontImage = false;
				}
				else
				{
					Station[j].paramFile.SaveAIFontImage2 = false;
				}
			}
		}
		lab_SaveAIImage.Visibility = Visibility.Hidden;
		btn_OpenAIFontFolder.Visibility = Visibility.Hidden;
	}

	private void Btn_InspSurv_Click(object sender, RoutedEventArgs e)
	{
		InspSurvWindow inspSurvWindow = new InspSurvWindow();
		inspSurvWindow.SetInspection(Station);
		inspSurvWindow.SetThroghputFile(ThroughputFile);
		inspSurvWindow.ShowDialog();
		ThroughputFile.WriteInspSurvParam();
	}

	private void Btn_test_Click(object sender, RoutedEventArgs e)
	{
		Station[StationIndex].SetLightCorrectionON(enabled: true);
		UISetupWindow uISetupWindow = new UISetupWindow();
		uISetupWindow.SetThroughputFile(ThroughputFile);
		uISetupWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AIPreview_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string text = "";
			text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
			if (text == "")
			{
				return;
			}
			text = System.IO.Path.GetFileNameWithoutExtension(text);
			string okpath = string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string text2 = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string yng = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
			AIPreviewWindow aIPreviewWindow = new AIPreviewWindow();
			aIPreviewWindow.SetStation(Station[StationIndex], StationIndex);
			aIPreviewWindow.LoadLanguage(LanguageType);
			aIPreviewWindow.SetSAM(mSam);
			aIPreviewWindow.SetImagePath(text2, okpath);
			aIPreviewWindow.SetThroughputFile(ThroughputFile);
			aIPreviewWindow.SetPreviewMode(Station[StationIndex].paramFile.PreviewMode);
			aIPreviewWindow.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
			aIPreviewWindow.SetNGMarkedFolder(text2, yng);
			string oldmodel = string.Format("Backup\\{0}", System.IO.Path.GetFileNameWithoutExtension(Station[StationIndex].paramFile.AIEnhanceParam.LastModelFile_A));
			aIPreviewWindow.SetModel(text, oldmodel);
			aIPreviewWindow.ShowDialog();
			for (int i = 0; i < 6; i++)
			{
				if (aIPreviewWindow.caption[i] == "" || !(Station[StationIndex].paramFile.DrawCaition[i] != aIPreviewWindow.caption[i]))
				{
					continue;
				}
				Station[StationIndex].paramFile.DrawCaition[i] = aIPreviewWindow.caption[i];
				for (int j = 0; j < 8; j++)
				{
					if (j != StationIndex && Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A == Station[j].paramFile.AIEnhanceParam.ModelName_A)
					{
						Station[j].paramFile.DrawCaition[i] = Station[StationIndex].paramFile.DrawCaition[i];
					}
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_AIPreview_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AIBitmapSave_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (AIMatReDrawing)
		{
			return;
		}
		try
		{
			AIMatReDrawing = true;
			Btn_TestAll_Click(this, null);
			if (!CheckBufferAndRef())
			{
				AIMatReDrawing = false;
				return;
			}
			if (Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				AIMatReDrawing = false;
				return;
			}
			Mat mat = Station[StationIndex].buffers.Dst.Clone();
			OpenCvSharp.Rect roi = new OpenCvSharp.Rect(Station[StationIndex].maskTool.AIRect.Left + Station[StationIndex].LocateImageRect.Left, Station[StationIndex].maskTool.AIRect.Top + Station[StationIndex].LocateImageRect.Top, Station[StationIndex].maskTool.AIRect.Width, Station[StationIndex].maskTool.AIRect.Height);
			using (Mat mat2 = Station[StationIndex].maskTool.AIMat.Clone())
			{
				using Mat m = new Mat(mat, roi);
				if (mat2.Width > 0 && mat2.Height > 0)
				{
					mat2.CopyTo(m);
				}
			}
			ImageSource imageSource = Station[StationIndex].buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			AIMatReDrawing = false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_AIBitmapSave_MouseEnter()" + ex.ToString());
			AIMatReDrawing = false;
		}
	}

	private void Btn_AIBitmapSave_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (OldDst.Width > 0 && OldDst.Height > 0)
		{
			ImageSource imageSource = Station[StationIndex].buffers.MatToBitmapImage(OldDst);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
		}
	}

	private void Main_Activated(object sender, EventArgs e)
	{
		if (!UpdateUITImer.IsEnabled)
		{
			LastV = 0;
			UpdateUITImer.Start();
		}
		if (ThroughputFile.CurrentClient == 3)
		{
			btn_YieldRetest.Visibility = Visibility.Hidden;
			chk_IRToolON.Visibility = Visibility.Hidden;
			chk_IRToolON.Visibility = Visibility.Hidden;
			btn_IRTool.Visibility = Visibility.Hidden;
		}
		CheckErrorLog();
		if (ThroughputFile.CurrentClient == 8)
		{
			if (User == User_Permissions.USER_OP)
			{
				btn_SelectWayTool.IsEnabled = false;
				btn_SelectFaceTool.IsEnabled = false;
				btn_EnhanceTool.IsEnabled = false;
				btn_AIEnhanceTool.IsEnabled = false;
				btn_MeasureTool.IsEnabled = false;
				btn_MaskTool.IsEnabled = false;
				btn_BinaryTool.IsEnabled = false;
				btn_SmoothTool.IsEnabled = false;
				btn_BlockAvgTool.IsEnabled = false;
				btn_DifferenceTool.IsEnabled = false;
				btn_LineAnalysis.IsEnabled = false;
				btn_AIPredictTool.IsEnabled = false;
				btn_IRTool.IsEnabled = false;
				chk_SelectWayON.IsEnabled = false;
				chk_SelectFaceON.IsEnabled = false;
				chk_EnhanceToolON.IsEnabled = false;
				chk_AIEnhanceToolON.IsEnabled = false;
				chk_MeasureToolON.IsEnabled = false;
				chk_MaskToolON.IsEnabled = false;
				chk_BinaryToolON.IsEnabled = false;
				chk_SmoothToolON.IsEnabled = false;
				chk_BlockAvgToolON.IsEnabled = false;
				chk_DifferenceToolON.IsEnabled = false;
				chk_LineAnalysisON.IsEnabled = false;
				chk_AIPredictToolON.IsEnabled = false;
				chk_IRToolON.IsEnabled = false;
				btn_AITrain.IsEnabled = false;
				btn_SizeStatic.IsEnabled = false;
				btn_ParamCopy.IsEnabled = false;
				btn_AI_Function.IsEnabled = false;
				btn_YieldRetest.IsEnabled = false;
				btn_DrawTempMask.IsEnabled = false;
			}
			else
			{
				btn_SelectWayTool.IsEnabled = true;
				btn_SelectFaceTool.IsEnabled = true;
				btn_EnhanceTool.IsEnabled = true;
				btn_AIEnhanceTool.IsEnabled = true;
				btn_MeasureTool.IsEnabled = true;
				btn_MaskTool.IsEnabled = true;
				btn_BinaryTool.IsEnabled = true;
				btn_SmoothTool.IsEnabled = true;
				btn_BlockAvgTool.IsEnabled = true;
				btn_DifferenceTool.IsEnabled = true;
				btn_LineAnalysis.IsEnabled = true;
				btn_AIPredictTool.IsEnabled = true;
				btn_IRTool.IsEnabled = true;
				chk_SelectWayON.IsEnabled = true;
				chk_SelectFaceON.IsEnabled = true;
				chk_EnhanceToolON.IsEnabled = true;
				chk_AIEnhanceToolON.IsEnabled = true;
				chk_MeasureToolON.IsEnabled = true;
				chk_MaskToolON.IsEnabled = true;
				chk_BinaryToolON.IsEnabled = true;
				chk_SmoothToolON.IsEnabled = true;
				chk_BlockAvgToolON.IsEnabled = true;
				chk_DifferenceToolON.IsEnabled = true;
				chk_LineAnalysisON.IsEnabled = true;
				chk_AIPredictToolON.IsEnabled = true;
				chk_IRToolON.IsEnabled = true;
				btn_AITrain.IsEnabled = true;
				btn_SizeStatic.IsEnabled = true;
				btn_ParamCopy.IsEnabled = true;
				btn_AI_Function.IsEnabled = true;
				btn_YieldRetest.IsEnabled = true;
				btn_DrawTempMask.IsEnabled = true;
			}
		}
		else if (ThroughputFile.CurrentClient == 7)
		{
			if (User == User_Permissions.USER_OP)
			{
				btn_LocateTool.IsEnabled = true;
				btn_SelectWayTool.IsEnabled = false;
				btn_SelectFaceTool.IsEnabled = false;
				btn_EnhanceTool.IsEnabled = false;
				btn_AIEnhanceTool.IsEnabled = false;
				btn_AdvancedLocate.IsEnabled = true;
				btn_MeasureTool.IsEnabled = false;
				btn_MaskTool.IsEnabled = false;
				btn_BinaryTool.IsEnabled = false;
				btn_SmoothTool.IsEnabled = false;
				btn_BlockAvgTool.IsEnabled = false;
				btn_DifferenceTool.IsEnabled = false;
				btn_LineAnalysis.IsEnabled = false;
				btn_AIPredictTool.IsEnabled = false;
				btn_FontTool.IsEnabled = true;
				btn_IRTool.IsEnabled = false;
				chk_SelectWayON.IsEnabled = false;
				chk_SelectFaceON.IsEnabled = false;
				chk_EnhanceToolON.IsEnabled = false;
				chk_AIEnhanceToolON.IsEnabled = false;
				chk_MeasureToolON.IsEnabled = false;
				chk_MaskToolON.IsEnabled = false;
				chk_BinaryToolON.IsEnabled = false;
				chk_SmoothToolON.IsEnabled = false;
				chk_BlockAvgToolON.IsEnabled = false;
				chk_DifferenceToolON.IsEnabled = false;
				chk_LineAnalysisON.IsEnabled = false;
				chk_AIPredictToolON.IsEnabled = false;
				chk_FontToolON.IsEnabled = true;
				chk_IRToolON.IsEnabled = false;
				btn_AITrain.IsEnabled = false;
				btn_SizeStatic.IsEnabled = false;
				btn_ParamCopy.IsEnabled = false;
				btn_AI_Function.IsEnabled = false;
				btn_YieldRetest.IsEnabled = false;
				btn_DrawTempMask.IsEnabled = false;
			}
			else
			{
				btn_LocateTool.IsEnabled = true;
				btn_SelectWayTool.IsEnabled = true;
				btn_SelectFaceTool.IsEnabled = true;
				btn_EnhanceTool.IsEnabled = true;
				btn_AIEnhanceTool.IsEnabled = true;
				btn_AdvancedLocate.IsEnabled = true;
				btn_MeasureTool.IsEnabled = true;
				btn_MaskTool.IsEnabled = true;
				btn_BinaryTool.IsEnabled = true;
				btn_SmoothTool.IsEnabled = true;
				btn_BlockAvgTool.IsEnabled = true;
				btn_DifferenceTool.IsEnabled = true;
				btn_LineAnalysis.IsEnabled = true;
				btn_AIPredictTool.IsEnabled = true;
				btn_FontTool.IsEnabled = true;
				btn_IRTool.IsEnabled = true;
				chk_SelectWayON.IsEnabled = true;
				chk_SelectFaceON.IsEnabled = true;
				chk_EnhanceToolON.IsEnabled = true;
				chk_AIEnhanceToolON.IsEnabled = true;
				chk_MeasureToolON.IsEnabled = true;
				chk_MaskToolON.IsEnabled = true;
				chk_BinaryToolON.IsEnabled = true;
				chk_SmoothToolON.IsEnabled = true;
				chk_BlockAvgToolON.IsEnabled = true;
				chk_DifferenceToolON.IsEnabled = true;
				chk_LineAnalysisON.IsEnabled = true;
				chk_AIPredictToolON.IsEnabled = true;
				chk_FontToolON.IsEnabled = true;
				chk_IRToolON.IsEnabled = true;
				btn_AITrain.IsEnabled = true;
				btn_SizeStatic.IsEnabled = true;
				btn_ParamCopy.IsEnabled = true;
				btn_AI_Function.IsEnabled = true;
				btn_YieldRetest.IsEnabled = true;
				btn_DrawTempMask.IsEnabled = true;
			}
		}
	}

	private void Exp_PreviewAI_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (exp_PreviewAI.IsExpanded)
		{
			isDragging = true;
			UIElement obj = sender as UIElement;
			System.Windows.Point position = e.GetPosition(this);
			TranslateTransform translateTransform = obj.RenderTransform as TranslateTransform;
			startPosition.X = position.X - translateTransform.X;
			startPosition.Y = position.Y - translateTransform.Y;
			obj.CaptureMouse();
		}
	}

	private void Exp_PreviewAI_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (exp_PreviewAI.IsExpanded)
		{
			isDragging = false;
			(sender as UIElement).ReleaseMouseCapture();
		}
	}

	private void Exp_PreviewAI_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (exp_PreviewAI.IsExpanded)
		{
			UIElement uIElement = sender as UIElement;
			if (isDragging && uIElement != null)
			{
				System.Windows.Point position = e.GetPosition(base.Parent as UIElement);
				TranslateTransform obj = uIElement.RenderTransform as TranslateTransform;
				obj.X = position.X - startPosition.X;
				obj.Y = position.Y - startPosition.Y;
			}
		}
	}

	private void Grd_AIFunction_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		isDragging2 = true;
		UIElement obj = sender as UIElement;
		System.Windows.Point position = e.GetPosition(this);
		TranslateTransform translateTransform = obj.RenderTransform as TranslateTransform;
		startPosition2.X = position.X - translateTransform.X;
		startPosition2.Y = position.Y - translateTransform.Y;
		obj.CaptureMouse();
	}

	private void Grd_AIFunction_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		isDragging2 = false;
		(sender as UIElement).ReleaseMouseCapture();
	}

	private void Grd_AIFunction_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		UIElement uIElement = sender as UIElement;
		if (isDragging2 && uIElement != null)
		{
			System.Windows.Point position = e.GetPosition(base.Parent as UIElement);
			TranslateTransform obj = uIElement.RenderTransform as TranslateTransform;
			obj.X = position.X - startPosition2.X;
			obj.Y = position.Y - startPosition2.Y;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AddToNGTemp_Click(object sender, RoutedEventArgs e)
	{
		if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
		{
			System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
			return;
		}
		if (!Station[StationIndex].paramFile.AIEnhanceToolON && !Station[StationIndex].paramFile.AIPredictToolON)
		{
			System.Windows.MessageBox.Show("尚未啟用AI前處理或AI檢測工具。", "警告");
			return;
		}
		string text = "";
		text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
		if (text == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		text = System.IO.Path.GetFileNameWithoutExtension(text);
		string text2 = string.Format("{0}TrainingImage\\{1}\\Temp\\", AppDomain.CurrentDomain.BaseDirectory, text);
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		try
		{
			Btn_TestAll_Click(this, null);
			if (!CheckBufferAndRef() || Station[StationIndex].maskTool.AIMat.Width <= 0 || Station[StationIndex].maskTool.AIMat.Height <= 0)
			{
				System.Windows.MessageBox.Show("AI影像生成前錯誤，請檢查定位、進階定位等工具是否有錯誤。", "警告");
			}
			else if (Station[StationIndex].maskTool.AIMat.Width > 0 && Station[StationIndex].maskTool.AIMat.Height > 0)
			{
				DateTime now = DateTime.Now;
				string text3 = string.Format("Temp-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
				string text4 = "";
				if (!Directory.Exists(text2))
				{
					System.Windows.MessageBox.Show(string.Format("AI NG暫存資料夾建立失敗，請重新設定AI影像路徑。({0})", text2), "警告");
					return;
				}
				text4 = string.Format("{0}{1}", text2, text3);
				Station[StationIndex].GetAIMat().SaveImage(text4);
				string str = ((LanguageType == 0) ? string.Format("{0} Save AI Temp Image Finish", text3) : ((LanguageType != 1) ? string.Format("{0} 加入AI NG暂存影像成功", text3) : string.Format("{0} 加入AI NG暫存影像成功", text3)));
				ShowLabMessage(str);
				string[] files = Directory.GetFiles(text2, "*.*", SearchOption.AllDirectories);
				lab_AINGTempCount.Content = files.Length.ToString();
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_AddToNGTemp_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_DrawTempMask_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (!Station[StationIndex].paramFile.MaskParam[0].TrackerON)
			{
				System.Windows.MessageBox.Show("尚未設定AI遮罩。(請設定遮罩工具第一個檢測框。)", "警告");
				return;
			}
			if (!Station[StationIndex].paramFile.AIEnhanceToolON && !Station[StationIndex].paramFile.AIPredictToolON)
			{
				System.Windows.MessageBox.Show("尚未啟用AI前處理或AI檢測工具。", "警告");
				return;
			}
			string text = "";
			text = ((!Station[StationIndex].paramFile.AIEnhanceToolON) ? Station[StationIndex].paramFile.AIPredictParam.ModelName_A : Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A);
			if (text == "")
			{
				System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
				return;
			}
			text = System.IO.Path.GetFileNameWithoutExtension(text);
			string path = string.Format("{0}TrainingImage\\{1}\\Temp\\", AppDomain.CurrentDomain.BaseDirectory, text);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			if (files.Length == 0)
			{
				System.Windows.MessageBox.Show("AI NG暫存影像數量為0。", "警告");
				return;
			}
			string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string ngpath = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string markpath = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, text);
			AIMarkWindow aIMarkWindow = new AIMarkWindow();
			aIMarkWindow.SetInspection(Station[StationIndex], StationIndex);
			aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
			aIMarkWindow.SetSAM(mSam);
			aIMarkWindow.SetImageSource(1, files, 0, 0, 0, 0);
			aIMarkWindow.CropedMode = true;
			aIMarkWindow.LoadLanguage(LanguageType);
			Mat mat = new Mat();
			mat = CVBuffer.ImRead(files[0]);
			aIMarkWindow.SetMat(mat, Station[StationIndex].paramFile.ZoomMode);
			aIMarkWindow.SetNGMarkedFolder(ngpath, markpath);
			aIMarkWindow.SetColorSelect(Station[StationIndex].paramFile.ColorSelect);
			aIMarkWindow.SetPreviewMode(Station[StationIndex].paramFile.PreviewMode);
			aIMarkWindow.SetPenSize(Station[StationIndex].paramFile.PenSize);
			aIMarkWindow.SetContinueWrite(enabled: true);
			aIMarkWindow.SetAutoDrawIgnore(Station[StationIndex].paramFile.EnableAutoIgnoreDraw, Station[StationIndex].paramFile.AutoIgnoreSize);
			aIMarkWindow.SetCaption(Station[StationIndex].paramFile.DrawCaition, Station[StationIndex].paramFile.SaveAsFolder);
			aIMarkWindow.ShowDialog();
			string[] s = new string[6];
			aIMarkWindow.GetCaption(ref s, ref Station[StationIndex].paramFile.SaveAsFolder);
			for (int i = 0; i < 6; i++)
			{
				if (!(Station[StationIndex].paramFile.DrawCaition[i] != s[i]))
				{
					continue;
				}
				Station[StationIndex].paramFile.DrawCaition[i] = s[i];
				for (int j = 0; j < 8; j++)
				{
					if (j != StationIndex && Station[StationIndex].paramFile.AIEnhanceParam.ModelName_A == Station[j].paramFile.AIEnhanceParam.ModelName_A)
					{
						Station[j].paramFile.DrawCaition[i] = Station[StationIndex].paramFile.DrawCaition[i];
					}
				}
			}
			Station[StationIndex].paramFile.PreviewMode = aIMarkWindow.GetPreviewMode();
			Station[StationIndex].paramFile.ZoomMode = aIMarkWindow.GetZoomMode();
			Station[StationIndex].paramFile.ColorSelect = aIMarkWindow.GetColorSelect();
			Station[StationIndex].paramFile.PenSize = aIMarkWindow.GetPenSize();
			Station[StationIndex].paramFile.EnableAutoIgnoreDraw = aIMarkWindow.GetEnableAutoDrawIgnore();
			Station[StationIndex].paramFile.AutoIgnoreSize = aIMarkWindow.GetAutoDrawIgnoreSize();
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? System.Windows.MessageBox.Show("Delete temporary Image?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? System.Windows.MessageBox.Show("是否刪除暂存影像?！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : System.Windows.MessageBox.Show("是否刪除暫存影像?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				for (int k = 0; k < files.Length; k++)
				{
					File.Delete(files[k]);
				}
			}
			files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			lab_AINGTempCount.Content = files.Length.ToString();
			UpdateAIImageStatus(StationIndex);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-Btn_DrawTempMask_Click()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImageNum_Click(object sender, RoutedEventArgs e)
	{
		int stationIndex = StationIndex;
		int num = 0;
		string text = "";
		try
		{
			switch (ImageListSource)
			{
			case 0:
				return;
			case 1:
				num = filelist[stationIndex].Length;
				break;
			case 2:
				if (Station[StationIndex].RecentBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[StationIndex].RecentBufferList.Count == 0)
				{
					ShowNoContent();
					return;
				}
				num = Station[stationIndex].RecentBufferList.Count;
				break;
			case 3:
				if (Station[StationIndex].OKBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[StationIndex].OKBufferList.Count == 0)
				{
					ShowNoContent();
					return;
				}
				num = Station[stationIndex].OKBufferList.Count;
				break;
			case 4:
				if (Station[StationIndex].NGParetoBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[StationIndex].NGParetoBufferList[ParetoNGSelect].Count == 0)
				{
					ShowNoContent();
					return;
				}
				num = Station[stationIndex].NGParetoBufferList[ParetoNGSelect].Count;
				break;
			case 5:
				if (Station[StationIndex].RetestBufferList == null)
				{
					ShowNoContent();
					return;
				}
				if (Station[StationIndex].RetestBufferList.Count == 0)
				{
					ShowNoContent();
					return;
				}
				num = Station[stationIndex].RetestBufferList.Count;
				break;
			}
			if (num <= 0)
			{
				return;
			}
			System.Windows.Point point = new System.Windows.Point(btn_ImageNum.Margin.Left, btn_ImageNum.Margin.Top);
			HKeyboard hKeyboard = new HKeyboard();
			text = string.Format("1~{0}", num);
			hKeyboard.SetTitle(text);
			hKeyboard.SetInput("");
			hKeyboard.SetInputNegative(state: true);
			hKeyboard.SetDouble(state: false);
			hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
			hKeyboard.Left = point.X;
			hKeyboard.Top = 450.0;
			hKeyboard.ShowDialog();
			if (hKeyboard.GetInput() == "")
			{
				return;
			}
			int num2 = Convert.ToInt32(hKeyboard.GetInput());
			num2--;
			if (num2 < 0 || num2 >= num)
			{
				return;
			}
			switch (ImageListSource)
			{
			case 0:
				return;
			case 1:
			{
				fileIndex = num2;
				text = LanguageManager.Instance["FileNotFound"];
				if (!File.Exists(filelist[stationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[stationIndex][fileIndex] + text);
					return;
				}
				if (!File.Exists(filelist[stationIndex][fileIndex]))
				{
					System.Windows.MessageBox.Show(filelist[stationIndex][fileIndex] + text);
					return;
				}
				Mat buffers = CVBuffer.ImRead(filelist[stationIndex][fileIndex]);
				lab_FileName.Content = System.IO.Path.GetFileName(filelist[stationIndex][fileIndex]);
				Station[stationIndex].SetBuffers(buffers);
				break;
			}
			case 2:
				RecentBufferIndex[stationIndex] = num2;
				Station[stationIndex].SetBuffers(Station[stationIndex].RecentBufferList[RecentBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 3:
				OKBufferIndex[stationIndex] = num2;
				Station[stationIndex].SetBuffers(Station[stationIndex].OKBufferList[OKBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 4:
				NGBufferIndex[stationIndex] = num2;
				Station[stationIndex].SetBuffers(Station[stationIndex].NGParetoBufferList[ParetoNGSelect][NGBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			case 5:
				RetestBufferIndex[stationIndex] = num2;
				Station[stationIndex].SetBuffers(Station[stationIndex].RetestBufferList[RetestBufferIndex[stationIndex]], BufferImageWidth[stationIndex], BufferImageHeight[stationIndex], BufferImageBand[stationIndex]);
				break;
			}
			Inspect();
			UpdateInspTime();
			RuncountText = string.Format("{0} / {1}", num2 + 1, num);
			btn_ImageNum.Content = RuncountText;
		}
		catch
		{
			text = string.Format("TW01-ToolsWindow-Btn_ImageNum_Click()- Station={0}, ImageListSource{1]", StationIndex, ImageListSource);
			CVBuffer.SaveLog(text);
		}
	}

	private void Btn_FontTool_Click(object sender, RoutedEventArgs e)
	{
		OpenFontTool();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void OpenFontTool()
	{
		try
		{
			if (Station[StationIndex].paramFile.FontToolParam.AllFont.Length > 5)
			{
				NewFontToolWindow newFontToolWindow = ((ThroughputFile.CurrentClient != 8 || User != User_Permissions.USER_OP) ? new NewFontToolWindow(User) : new NewFontToolWindow(User_Permissions.USER_Engineer));
				if (!CheckBufferAndRef())
				{
					newFontToolWindow.LoadLanguage(LanguageType);
					newFontToolWindow.SetInspection(Station[StationIndex], StationIndex, Station[StationIndex].SelectFaceResult);
					newFontToolWindow.SetThroghputFile(ThroughputFile);
					newFontToolWindow.SetPublicDB(PublicPatternDB, PrivatePatternDB);
					newFontToolWindow.SetTrainDir(FontTrainDir);
					newFontToolWindow.SetPreviewMode(on: true);
					newFontToolWindow.ShowDialog();
					FontTrainDir = newFontToolWindow.GetTrainDir();
					Btn_TestAll_Click(this, null);
					return;
				}
				Main.AllowDrop = false;
				newFontToolWindow.LoadLanguage(LanguageType);
				newFontToolWindow.SetInspection(Station[StationIndex], StationIndex, Station[StationIndex].SelectFaceResult);
				newFontToolWindow.SetThroghputFile(ThroughputFile);
				newFontToolWindow.SetPublicDB(PublicPatternDB, PrivatePatternDB);
				newFontToolWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
				newFontToolWindow.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
				newFontToolWindow.SetTrainDir(FontTrainDir);
				newFontToolWindow.ShowDialog();
				FontTrainDir = newFontToolWindow.GetTrainDir();
				Btn_TestAll_Click(this, null);
				Main.AllowDrop = true;
			}
			else
			{
				FontToolWindow fontToolWindow = ((ThroughputFile.CurrentClient != 8 || User != User_Permissions.USER_OP) ? new FontToolWindow(User) : new FontToolWindow(User_Permissions.USER_Engineer));
				if (!CheckBufferAndRef())
				{
					fontToolWindow.LoadLanguage(LanguageType);
					fontToolWindow.SetInspection(Station[StationIndex], StationIndex, Station[StationIndex].SelectFaceResult);
					fontToolWindow.SetThroghputFile(ThroughputFile);
					fontToolWindow.SetPublicDB(PublicPatternDB, PrivatePatternDB);
					fontToolWindow.SetTrainDir(FontTrainDir);
					fontToolWindow.SetPreviewMode(on: true);
					fontToolWindow.ShowDialog();
					FontTrainDir = fontToolWindow.GetTrainDir();
					Btn_TestAll_Click(this, null);
					return;
				}
				Main.AllowDrop = false;
				fontToolWindow.LoadLanguage(LanguageType);
				fontToolWindow.SetInspection(Station[StationIndex], StationIndex, Station[StationIndex].SelectFaceResult);
				fontToolWindow.SetThroghputFile(ThroughputFile);
				fontToolWindow.SetPublicDB(PublicPatternDB, PrivatePatternDB);
				fontToolWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
				fontToolWindow.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
				fontToolWindow.SetTrainDir(FontTrainDir);
				fontToolWindow.ShowDialog();
				FontTrainDir = fontToolWindow.GetTrainDir();
				Btn_TestAll_Click(this, null);
				Main.AllowDrop = true;
			}
			lab_FontToolText.Content = Station[StationIndex].paramFile.FontToolParam.AllFont;
		}
		catch
		{
			System.Windows.MessageBox.Show("error", "錯誤");
		}
	}

	private void Btn_AutoFontMatch_Click(object sender, RoutedEventArgs e)
	{
		AutoMatchWindow autoMatchWindow = new AutoMatchWindow();
		autoMatchWindow.LoadLanguage(LanguageType);
		autoMatchWindow.SetInspection(Station[StationIndex], RecipeName);
		autoMatchWindow.SetLastText(Station[StationIndex].paramFile.FontToolParam.AllFont);
		autoMatchWindow.SetPublicDB(PublicPatternDB, PrivatePatternDB);
		autoMatchWindow.FontToolMode = true;
		autoMatchWindow.ShowDialog();
		lab_FontToolText.Content = Station[StationIndex].paramFile.FontToolParam.AllFont;
		Btn_TestAll_Click(null, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenFontFolder_Click(object sender, RoutedEventArgs e)
	{
		DateTime now = DateTime.Now;
		string text = string.Format("d:\\AIImage\\AI_Font\\S{0}\\{1}\\", StationIndex + 1, now.ToString("yyyy-MM-dd"));
		string fileName = "c:\\windows\\explorer.exe";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string arguments = string.Format("/root, {0}", text);
		Process.Start(fileName, arguments);
	}

	private void Exp_PreviewAI_Expanded(object sender, RoutedEventArgs e)
	{
		Btn_TestAll_Click(null, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ChannelExpend_Click(object sender, RoutedEventArgs e)
	{
		if ((string)btn_ChannelExpend.Content == ">")
		{
			grd_Channel.Width = 535.0;
			btn_ChannelExpend.Content = "<";
			btn_ImgAILR.Visibility = Visibility.Hidden;
			btn_ImgAILG.Visibility = Visibility.Hidden;
			btn_ImgAILB.Visibility = Visibility.Hidden;
			return;
		}
		grd_Channel.Width = 140.0;
		btn_ChannelExpend.Content = ">";
		if (Station[0].paramFile.AllowEnableAILocate)
		{
			btn_ImgAILR.Visibility = Visibility.Visible;
			btn_ImgAILG.Visibility = Visibility.Visible;
			btn_ImgAILB.Visibility = Visibility.Visible;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadMatchQuickSetupParam()
	{
		for (int i = 0; i < 50; i++)
		{
			MatchQuickSetupParam[i].NameChangeFlag = false;
			MatchQuickSetupParam[i].ValueChangeFlag = false;
			string section = "MatchToolsQuickSetup" + i;
			MatchQuickSetupParam[i].TrackerON = DBIni.GetBoolean(section, "TrackerON");
			MatchQuickSetupParam[i].TrackerName = DBIni.GetValue(section, "TrackerName", "Unused");
			MatchQuickSetupParam[i].SearchRect1 = DBIni.ReadRect(section, "SearchRect1", 50);
			MatchQuickSetupParam[i].Tracker1UseAutoSize = DBIni.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
			MatchQuickSetupParam[i].Tracker1AligmentXIndex = DBIni.GetInteger(section, "Tracker1AligmentXIndex");
			MatchQuickSetupParam[i].Tracker1AligmentYIndex = DBIni.GetInteger(section, "Tracker1AligmentYIndex");
			MatchQuickSetupParam[i].Tracker1AligmentX = DBIni.GetInteger(section, "Tracker1AligmentX");
			MatchQuickSetupParam[i].Tracker1AligmentY = DBIni.GetInteger(section, "Tracker1AligmentY");
			MatchQuickSetupParam[i].AutoSize1_SelectIndex = DBIni.GetInteger(section, "AutoSize1_SelectIndex");
			MatchQuickSetupParam[i].Track1_T = DBIni.GetInteger(section, "Track1_T", 5);
			MatchQuickSetupParam[i].Track1_B = DBIni.GetInteger(section, "Track1_B", 5);
			MatchQuickSetupParam[i].Track1_L = DBIni.GetInteger(section, "Track1_L", 5);
			MatchQuickSetupParam[i].Track1_R = DBIni.GetInteger(section, "Track1_R", 5);
			MatchQuickSetupParam[i].PatternSelect = DBIni.GetInteger(section, "PatternSelect", -1);
			MatchQuickSetupParam[i].PatternSelect2 = DBIni.GetInteger(section, "PatternSelect2", -1);
			MatchQuickSetupParam[i].PatternSelect3 = DBIni.GetInteger(section, "PatternSelect3", -1);
			MatchQuickSetupParam[i].PatternSelect4 = DBIni.GetInteger(section, "PatternSelect4", -1);
			MatchQuickSetupParam[i].MatchScoreMin = DBIni.GetInteger(section, "MatchScoreMin", 60);
			MatchQuickSetupParam[i].MatchScoreMax = DBIni.GetInteger(section, "MatchScoreMax", 100);
			MatchQuickSetupParam[i].ShapeMatchScoreMin = DBIni.GetInteger(section, "ShapeMatchScoreMin", 60);
			MatchQuickSetupParam[i].MinMatchShapoeScore = DBIni.GetInteger(section, "MinMatchShapoeScore", 60);
			MatchQuickSetupParam[i].UseTextMatch = DBIni.GetBoolean(section, "UseTextMatch", defaultValue: true);
			MatchQuickSetupParam[i].TextisWhite = DBIni.GetBoolean(section, "TextisWhite", defaultValue: true);
			MatchQuickSetupParam[i].TextPosDiff = DBIni.GetInteger(section, "TextPosDiff", 60);
			MatchQuickSetupParam[i].TextAreaDiff = DBIni.GetInteger(section, "TextAreaDiff", 60);
			MatchQuickSetupParam[i].TextWidthDiff = DBIni.GetInteger(section, "TextWidthDiff", 60);
			MatchQuickSetupParam[i].TextHeightDiff = DBIni.GetInteger(section, "TextHeightDiff", 60);
			MatchQuickSetupParam[i].MinColorMatchScore = DBIni.GetDouble(section, "MinColorMatchScore", 60.0);
			MatchQuickSetupParam[i].EnableMatchShape = DBIni.GetBoolean(section, "EnableMatchShape");
			MatchQuickSetupParam[i].ShapeTextisWhite = DBIni.GetBoolean(section, "ShapeTextisWhite");
			MatchQuickSetupParam[i].EnableSelectFace = DBIni.GetBoolean(section, "EnableSelectFace");
			MatchQuickSetupParam[i].FaceIndex = DBIni.GetInteger(section, "FaceIndex");
			MatchQuickSetupParam[i].ParetoIndex = DBIni.GetInteger(section, "ParetoIndex");
			MatchQuickSetupParam[i].EnableRotateInspect = DBIni.GetBoolean(section, "EnableRotateInspect");
			MatchQuickSetupParam[i].FindCloseArea = DBIni.GetInteger(section, "FindCloseArea");
			MatchQuickSetupParam[i].FindTextArea = DBIni.GetInteger(section, "FindTextArea");
			MatchQuickSetupParam[i].ClosedArea = DBIni.GetDouble(section, "ClosedArea", 100.0);
			MatchQuickSetupParam[i].MaxClosedArea = DBIni.GetDouble(section, "MaxClosedArea", 100.0);
			MatchQuickSetupParam[i].TextArea = DBIni.GetDouble(section, "TextArea", 100.0);
			MatchQuickSetupParam[i].MaxTextArea = DBIni.GetDouble(section, "MaxTextArea", 1000.0);
			MatchQuickSetupParam[i].MinTextWidth = DBIni.GetDouble(section, "MinTextWidth", 100.0);
			MatchQuickSetupParam[i].MinTextHeight = DBIni.GetDouble(section, "MinTextHeight", 100.0);
			MatchQuickSetupParam[i].MaxTextWidth = DBIni.GetDouble(section, "MaxTextWidth", 1000000.0);
			MatchQuickSetupParam[i].MaxTextHeight = DBIni.GetDouble(section, "MaxTextHeight", 1000000.0);
			MatchQuickSetupParam[i].DEStart = DBIni.GetInteger(section, "DEStart");
			MatchQuickSetupParam[i].DEStartCount = DBIni.GetInteger(section, "DEStartCount");
			MatchQuickSetupParam[i].UseOCR = DBIni.GetBoolean(section, "UseOCR");
			MatchQuickSetupParam[i].OCRText = DBIni.GetValue(section, "OCRText", "123");
			MatchQuickSetupParam[i].OCRImageChannel = DBIni.GetInteger(section, "OCRImageChannel");
			MatchQuickSetupParam[i].OCRTextisWhite = DBIni.GetBoolean(section, "OCRTextisWhite");
			MatchQuickSetupParam[i].UsePublicDB = DBIni.GetBoolean(section, "UsePublicDB");
			MatchQuickSetupParam[i].QuickSetupFinish = DBIni.GetBoolean(section, "QuickSetupFinish");
			MatchQuickSetupParam[i].QuickSetupIndex = DBIni.GetInteger(section, "QuickSetupIndex", -1);
			MatchQuickSetupParam[i].QuickSetupName = DBIni.GetValue(section, "QuickSetupName", "-----");
			MatchQuickSetupParam[i].MaskInFound = DBIni.GetBoolean(section, "MaskInFound");
			MatchQuickSetupParam[i].EnableManualThd = DBIni.GetBoolean(section, "EnableManualThd");
			MatchQuickSetupParam[i].TxtBinaryThdMin = DBIni.GetInteger(section, "TxtBinaryThdMin");
			MatchQuickSetupParam[i].TxtBinaryThdMax = DBIni.GetInteger(section, "TxtBinaryThdMax", 255);
			MatchQuickSetupParam[i].EnableAISimilarityCheck = DBIni.GetBoolean(section, "EnableAISimilarityCheck");
			MatchQuickSetupParam[i].AISimilarityScore = DBIni.GetInteger(section, "AISimilarityScore", 75);
			MatchQuickSetupParam[i].OnnxName = DBIni.GetValue(section, "OnnxName");
			MatchQuickSetupParam[i].ErrorBin = (FLOW_BIN)DBIni.GetInteger(section, "ErrorBin", 2);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Main_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (Station[StationIndex].RecentBufferList == null)
		{
			return;
		}
		try
		{
			if (Station[StationIndex].RecentBufferList.Count > 0 && Station[StationIndex].RecentBufferList[RecentBufferIndex[StationIndex]] != null)
			{
				Station[StationIndex].SetBuffers(Station[StationIndex].RecentBufferList[RecentBufferIndex[StationIndex]], BufferImageWidth[StationIndex], BufferImageHeight[StationIndex], BufferImageBand[StationIndex]);
				Inspect();
				RuncountText = string.Format("{0} / {1}", RecentBufferIndex[StationIndex] + 1, Station[StationIndex].RecentBufferList.Count);
				btn_ImageNum.Content = RuncountText;
				ImageListSource = 2;
				btn_UseRecentBufList.Background = System.Windows.Media.Brushes.Yellow;
				btn_UseOKBufList.Background = System.Windows.Media.Brushes.White;
				btn_UseNGBufList.Background = System.Windows.Media.Brushes.White;
				btn_UseRetestBufList.Background = System.Windows.Media.Brushes.White;
				btn_UseFileBufList.Background = System.Windows.Media.Brushes.White;
				btn_UseOpenBitmap.Background = System.Windows.Media.Brushes.White;
				cb_ParetoNGSelect.Visibility = Visibility.Hidden;
				lab_ParetoNGSelect.Visibility = Visibility.Hidden;
			}
		}
		catch
		{
			CVBuffer.SaveLog(string.Format("TW01-ToolsWindow-MetroWindow_Loaded()- Station={0}, RecentBufferIndex={1}, RecentBufferListCount={2}", StationIndex, RecentBufferIndex[StationIndex], Station[StationIndex].RecentBufferList.Count));
		}
	}

	private void btn_IRTool_Click(object sender, RoutedEventArgs e)
	{
		OpenIRTool();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void OpenIRTool()
	{
		try
		{
			if (!CheckBufferAndRef())
			{
				IRToolWindow iRToolWindow = new IRToolWindow(User);
				iRToolWindow.LoadLanguage(LanguageType);
				iRToolWindow.SetInspection(StationIndex, Station[StationIndex], Station[StationIndex].SelectFaceResult);
				iRToolWindow.SetThroghputFile(ThroughputFile);
				iRToolWindow.SetPreviewMode(on: true);
				iRToolWindow.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
				iRToolWindow.ShowDialog();
				Btn_TestAll_Click(this, null);
			}
			else
			{
				IRToolWindow iRToolWindow2 = new IRToolWindow(User);
				iRToolWindow2.LoadLanguage(LanguageType);
				iRToolWindow2.SetInspection(StationIndex, Station[StationIndex], Station[StationIndex].SelectFaceResult);
				iRToolWindow2.SetThroghputFile(ThroughputFile);
				iRToolWindow2.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
				iRToolWindow2.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
				iRToolWindow2.SetModifyLogEnable(EnableModifyLog, RecipeName, UserID);
				iRToolWindow2.SetRotateStatus(Station[StationIndex].irTool.ResultIsA);
				iRToolWindow2.ShowDialog();
				Btn_TestAll_Click(this, null);
			}
		}
		catch
		{
			System.Windows.MessageBox.Show("error", "錯誤");
		}
	}

	private void btn_YieldRetest_Click(object sender, RoutedEventArgs e)
	{
		YieldRetestWindow yieldRetestWindow = new YieldRetestWindow();
		yieldRetestWindow.SetInspection(Station, BufferImageWidth, BufferImageHeight, BufferImageBand, ThroughputFile.param.YieldRetestPath);
		yieldRetestWindow.SetOldBinCount(OldBinCount, OldTotalBinCount);
		yieldRetestWindow.SetParamRatio(Param1Ratio, Param2Ratio, Param3Ratio);
		yieldRetestWindow.ShowDialog();
		yieldRetestWindow.GetOldBinCount(ref OldBinCount, ref OldTotalBinCount);
		yieldRetestWindow.GetParamRatio(ref Param1Ratio, ref Param2Ratio, ref Param3Ratio);
		ReadToolONStatus();
		if (ThroughputFile.param.YieldRetestPath != yieldRetestWindow.YieldRetestPath)
		{
			ThroughputFile.param.YieldRetestPath = yieldRetestWindow.YieldRetestPath;
			ThroughputFile.WriteThroughputParam();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteMatchQuickSetupParam()
	{
		for (int i = 0; i < 50; i++)
		{
			string section = "MatchToolsQuickSetup" + i;
			if (MatchQuickSetupParam[i].NameChangeFlag)
			{
				DBIni.WriteString(section, "TrackerName", MatchQuickSetupParam[i].TrackerName);
				MatchQuickSetupParam[i].NameChangeFlag = false;
			}
			if (MatchQuickSetupParam[i].ValueChangeFlag)
			{
				MatchQuickSetupParam[i].ValueChangeFlag = false;
				DBIni.WriteBool(section, "TrackerON", MatchQuickSetupParam[i].TrackerON);
				DBIni.WriteString(section, "TrackerName", MatchQuickSetupParam[i].TrackerName);
				DBIni.WriteBool(section, "Tracker1UseAutoSize", MatchQuickSetupParam[i].Tracker1UseAutoSize);
				DBIni.WriteInt(section, "Tracker1AligmentXIndex", MatchQuickSetupParam[i].Tracker1AligmentXIndex);
				DBIni.WriteInt(section, "Tracker1AligmentYIndex", MatchQuickSetupParam[i].Tracker1AligmentYIndex);
				DBIni.WriteInt(section, "Tracker1AligmentX", MatchQuickSetupParam[i].Tracker1AligmentX);
				DBIni.WriteInt(section, "Tracker1AligmentY", MatchQuickSetupParam[i].Tracker1AligmentY);
				DBIni.WriteInt(section, "AutoSize1_SelectIndex", MatchQuickSetupParam[i].AutoSize1_SelectIndex);
				DBIni.WriteInt(section, "Track1_T", MatchQuickSetupParam[i].Track1_T);
				DBIni.WriteInt(section, "Track1_B", MatchQuickSetupParam[i].Track1_B);
				DBIni.WriteInt(section, "Track1_L", MatchQuickSetupParam[i].Track1_L);
				DBIni.WriteInt(section, "Track1_R", MatchQuickSetupParam[i].Track1_R);
				DBIni.WriteInt(section, "PatternSelect", MatchQuickSetupParam[i].PatternSelect);
				DBIni.WriteInt(section, "PatternSelect2", MatchQuickSetupParam[i].PatternSelect2);
				DBIni.WriteInt(section, "PatternSelect3", MatchQuickSetupParam[i].PatternSelect3);
				DBIni.WriteInt(section, "PatternSelect4", MatchQuickSetupParam[i].PatternSelect4);
				DBIni.WriteInt(section, "MatchScoreMin", MatchQuickSetupParam[i].MatchScoreMin);
				DBIni.WriteInt(section, "MatchScoreMax", MatchQuickSetupParam[i].MatchScoreMax);
				DBIni.WriteInt(section, "ShapeMatchScoreMin", MatchQuickSetupParam[i].ShapeMatchScoreMin);
				DBIni.WriteBool(section, "EnableSelectFace", MatchQuickSetupParam[i].EnableSelectFace);
				DBIni.WriteInt(section, "FaceIndex", MatchQuickSetupParam[i].FaceIndex);
				DBIni.WriteInt(section, "ParetoIndex", MatchQuickSetupParam[i].ParetoIndex);
				DBIni.WriteBool(section, "UseTextMatch", MatchQuickSetupParam[i].UseTextMatch);
				DBIni.WriteBool(section, "TextisWhite", MatchQuickSetupParam[i].TextisWhite);
				DBIni.WriteInt(section, "TextPosDiff", MatchQuickSetupParam[i].TextPosDiff);
				DBIni.WriteInt(section, "TextAreaDiff", MatchQuickSetupParam[i].TextAreaDiff);
				DBIni.WriteInt(section, "TextWidthDiff", MatchQuickSetupParam[i].TextWidthDiff);
				DBIni.WriteInt(section, "TextHeightDiff", MatchQuickSetupParam[i].TextHeightDiff);
				DBIni.WriteDouble(section, "MinColorMatchScore", MatchQuickSetupParam[i].MinColorMatchScore);
				DBIni.WriteInt(section, "MinMatchShapoeScore", MatchQuickSetupParam[i].MinMatchShapoeScore);
				DBIni.WriteBool(section, "EnableMatchShape", MatchQuickSetupParam[i].EnableMatchShape);
				DBIni.WriteBool(section, "ShapeTextisWhite", MatchQuickSetupParam[i].ShapeTextisWhite);
				DBIni.WriteBool(section, "EnableRotateInspect", MatchQuickSetupParam[i].EnableRotateInspect);
				DBIni.WriteInt(section, "FindCloseArea", MatchQuickSetupParam[i].FindCloseArea);
				DBIni.WriteInt(section, "FindTextArea", MatchQuickSetupParam[i].FindTextArea);
				DBIni.WriteDouble(section, "ClosedArea", MatchQuickSetupParam[i].ClosedArea);
				DBIni.WriteDouble(section, "MaxClosedArea", MatchQuickSetupParam[i].MaxClosedArea);
				DBIni.WriteDouble(section, "TextArea", MatchQuickSetupParam[i].TextArea);
				DBIni.WriteDouble(section, "MaxTextArea", MatchQuickSetupParam[i].MaxTextArea);
				DBIni.WriteDouble(section, "MinTextWidth", MatchQuickSetupParam[i].MinTextWidth);
				DBIni.WriteDouble(section, "MinTextHeight", MatchQuickSetupParam[i].MinTextHeight);
				DBIni.WriteDouble(section, "MaxTextWidth", MatchQuickSetupParam[i].MaxTextWidth);
				DBIni.WriteDouble(section, "MaxTextHeight", MatchQuickSetupParam[i].MaxTextHeight);
				DBIni.WriteInt(section, "DEStart", MatchQuickSetupParam[i].DEStart);
				DBIni.WriteInt(section, "DEStartCount", MatchQuickSetupParam[i].DEStartCount);
				DBIni.WriteBool(section, "UseOCR", MatchQuickSetupParam[i].UseOCR);
				DBIni.WriteString(section, "OCRText", MatchQuickSetupParam[i].OCRText);
				DBIni.WriteInt(section, "OCRImageChannel", MatchQuickSetupParam[i].OCRImageChannel);
				DBIni.WriteBool(section, "OCRTextisWhite", MatchQuickSetupParam[i].OCRTextisWhite);
				DBIni.WriteBool(section, "UsePublicDB", MatchQuickSetupParam[i].UsePublicDB);
				DBIni.WriteBool(section, "QuickSetupFinish", MatchQuickSetupParam[i].QuickSetupFinish);
				DBIni.WriteString(section, "QuickSetupName", MatchQuickSetupParam[i].QuickSetupName);
				DBIni.WriteInt(section, "QuickSetupIndex", MatchQuickSetupParam[i].QuickSetupIndex);
				DBIni.WriteBool(section, "MaskInFound", MatchQuickSetupParam[i].MaskInFound);
				DBIni.WriteBool(section, "EnableManualThd", MatchQuickSetupParam[i].EnableManualThd);
				DBIni.WriteInt(section, "TxtBinaryThdMin", MatchQuickSetupParam[i].TxtBinaryThdMin);
				DBIni.WriteInt(section, "TxtBinaryThdMax", MatchQuickSetupParam[i].TxtBinaryThdMax);
				DBIni.WriteBool(section, "EnableAISimilarityCheck", MatchQuickSetupParam[i].EnableAISimilarityCheck);
				DBIni.WriteInt(section, "AISimilarityScore", MatchQuickSetupParam[i].AISimilarityScore);
				DBIni.WriteString(section, "OnnxName", MatchQuickSetupParam[i].OnnxName);
				DBIni.WriteInt(section, "ErrorBin", (int)MatchQuickSetupParam[i].ErrorBin);
			}
		}
		return true;
	}

	private void grd_Main_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
	}

	private void Main_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (e.KeyStates == Keyboard.GetKeyStates(Key.Right))
		{
			Btn_NextImage_Click(this, null);
		}
		else if (e.KeyStates == Keyboard.GetKeyStates(Key.Left))
		{
			Btn_PrevImage_Click(this, null);
		}
	}

	public void SetFontText()
	{
		AutoMatchWindow autoMatchWindow = new AutoMatchWindow();
		autoMatchWindow.LoadLanguage(LanguageType);
		autoMatchWindow.SetInspection(Station[StationIndex], RecipeName);
		autoMatchWindow.SetPublicDB(PublicPatternDB, PrivatePatternDB);
		autoMatchWindow.SetLastText(Station[StationIndex].paramFile.FontToolParam.AllFont);
		autoMatchWindow.FontToolMode = true;
		autoMatchWindow.ShowDialog();
		Station[StationIndex].paramFile.WriteFontToolParam();
		lab_FontToolText.Content = Station[StationIndex].paramFile.FontToolParam.AllFont;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_LogAlert_Click(object sender, RoutedEventArgs e)
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		try
		{
			if (LanguageType == 1)
			{
				folderSelectDialog.Title = "選擇資料夾，匯出錯誤訊息及資料。(給台達RD)";
			}
			else
			{
				folderSelectDialog.Title = "Please select folder and export file and log.(To Delta RD)";
			}
			if (!folderSelectDialog.ShowDialog())
			{
				return;
			}
			string[] files = Directory.GetFiles("D:\\LogError\\", "*.zip", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				string text2 = System.IO.Path.Combine(folderSelectDialog.FileName, System.IO.Path.GetFileName(text));
				if (text != text2)
				{
					File.Copy(text, text2, overwrite: true);
					File.Delete(text);
				}
			}
			btn_LogAlert.Visibility = Visibility.Hidden;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-btn_LogAlert_Click()" + ex.ToString());
		}
	}

	public void SetBinCount(int[,] oldbincount, int[] oldtotalbincount)
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				OldBinCount[i, j] = oldbincount[i, j];
				if (i == 0)
				{
					OldTotalBinCount[j] = oldtotalbincount[j];
				}
			}
		}
	}

	private void Nu_Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (UpdaeUI)
		{
			for (int i = 0; i < 8; i++)
			{
				Station[i].locateTool.SetEnhanceParam(nu_Hue.Value.Value, nu_Sat.Value.Value, nu_Val.Value.Value, nu_Red.Value.Value, nu_Green.Value.Value, nu_Blue.Value.Value, chk_EnableHMirror.IsChecked.Value, chk_EnableVMirror.IsChecked.Value);
			}
			Btn_TestAll_Click(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void chk_TestEnhanceEnabled_Click(object sender, RoutedEventArgs e)
	{
		if (timer_state == 1)
		{
			base.Cursor = System.Windows.Input.Cursors.Arrow;
			AutoTestPauseEvent.Reset();
			AutoTestUIPauseEvent.Reset();
			timer_state = 2;
			ThreadStateStr = LanguageManager.Instance["AutorunPause"];
			lab_AutestState.Content = ThreadStateStr;
			txt_OKCount.Text = OKCount.ToString();
			txt_OKCountRatio.Text = string.Format("{0:P}", OKRatio);
			txt_NGCount.Text = NGCount.ToString();
			txt_NGCountRatio.Text = string.Format("{0:P}", NGRatio);
			btn_ImageNum.Content = RuncountText;
		}
		if (chk_TestEnhanceEnabled.IsChecked == true)
		{
			grd_TestEnhance.Visibility = Visibility.Visible;
			EnhanceTestExtend = true;
			for (int i = 0; i < 8; i++)
			{
				Station[i].locateTool.OffLineTestEnhance = true;
			}
		}
		else
		{
			grd_TestEnhance.Visibility = Visibility.Hidden;
			EnhanceTestExtend = false;
			for (int j = 0; j < 8; j++)
			{
				Station[j].locateTool.OffLineTestEnhance = false;
			}
		}
		Label_PreviewMouseLeftButtonDown(this, null);
		Btn_TestAll_Click(this, null);
	}

	private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!EnhanceTestExtend)
		{
			Thickness margin = new Thickness(70.0, 574.0, 0.0, 0.0);
			grd_TestEnhance.Margin = margin;
		}
		else
		{
			Thickness margin2 = new Thickness(70.0, 508.0, 0.0, 0.0);
			grd_TestEnhance.Margin = margin2;
		}
		EnhanceTestExtend = !EnhanceTestExtend;
	}

	private void btn_TestEnhanceReset_Click(object sender, RoutedEventArgs e)
	{
		nu_Hue.Value = 1.0;
		nu_Sat.Value = 1.0;
		nu_Val.Value = 1.0;
		nu_Red.Value = 1.0;
		nu_Green.Value = 1.0;
		nu_Blue.Value = 1.0;
		chk_EnableHMirror.IsChecked = false;
		chk_EnableVMirror.IsChecked = false;
	}

	private void chk_EnableVMirror_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i].locateTool.SetEnhanceParam(nu_Hue.Value.Value, nu_Sat.Value.Value, nu_Val.Value.Value, nu_Red.Value.Value, nu_Green.Value.Value, nu_Blue.Value.Value, chk_EnableHMirror.IsChecked.Value, chk_EnableVMirror.IsChecked.Value);
		}
		Btn_TestAll_Click(this, null);
	}

	private void btn_AIProphet_Click(object sender, RoutedEventArgs e)
	{
		if (!CheckBufferAndRef())
		{
			return;
		}
		AIProphetToolWindow aIProphetToolWindow = new AIProphetToolWindow();
		aIProphetToolWindow.LoadLanguage(LanguageType);
		aIProphetToolWindow.SetInspection(StationIndex, Station);
		aIProphetToolWindow.SetThroghputFile(ThroughputFile);
		aIProphetToolWindow.SetBuffer(Station[StationIndex].buffers.LocatedSrc, Station[StationIndex].LocateRect);
		aIProphetToolWindow.SetImageSource(ImageListSource, filelist[StationIndex], fileIndex, RecentBufferIndex[StationIndex], OKBufferIndex[StationIndex], NGBufferIndex[StationIndex]);
		aIProphetToolWindow.ShowDialog();
		if (Station[StationIndex].paramFile.AIProphetToolON)
		{
			chk_AIProphetToolON.IsChecked = true;
		}
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIProphetToolON && !Station[i].aiProphetTools.ModelReaded)
			{
				Station[i].aiProphetTools.ReadModel("");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AddPTrainImage_Click(object sender, RoutedEventArgs e)
	{
		string modelName = Station[StationIndex].paramFile.AIProphetParam.ModelName;
		if (modelName == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		string text = string.Format("{0}PModel\\{1}\\OK\\", AppDomain.CurrentDomain.BaseDirectory, modelName);
		if (!Directory.Exists(text))
		{
			string text2 = string.Format("{0}PModel\\{1}\\Good\\", AppDomain.CurrentDomain.BaseDirectory, modelName);
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
			System.Windows.MessageBox.Show(string.Format("AI OK資料夾建立失敗，請重新設定AI影像路徑。({0})", text), "警告");
			return;
		}
		try
		{
			Btn_TestAll_Click(this, null);
			if (Station[StationIndex].aiProphetTools.org.Width <= 0 || Station[StationIndex].aiProphetTools.org.Height <= 0)
			{
				System.Windows.MessageBox.Show("AI影像生成前錯誤，請檢查AI先知檢測框設定是否有錯誤。", "警告");
				return;
			}
			DateTime now = DateTime.Now;
			string text3 = string.Format("OK-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string fileName = string.Format("{0}{1}", text, text3);
			Station[StationIndex].aiProphetTools.org.SaveImage(fileName);
			string str = ((LanguageType == 0) ? string.Format("{0} Save AI Image Finish", text3) : ((LanguageType != 1) ? string.Format("{0} AI 影像储存成功!", text3) : string.Format("{0} AI 訓練影像儲存成功", text3)));
			ShowLabMessage(str);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-btn_AddPTrainImage_Click()" + ex.ToString());
		}
	}

	private void exp_PreviewAIHeatMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (exp_PreviewAIHeatMap.IsExpanded)
		{
			isDragging = true;
			UIElement obj = sender as UIElement;
			System.Windows.Point position = e.GetPosition(this);
			TranslateTransform translateTransform = obj.RenderTransform as TranslateTransform;
			startPosition.X = position.X - translateTransform.X;
			startPosition.Y = position.Y - translateTransform.Y;
			obj.CaptureMouse();
		}
	}

	private void exp_PreviewAIHeatMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (exp_PreviewAIHeatMap.IsExpanded)
		{
			isDragging = false;
			(sender as UIElement).ReleaseMouseCapture();
		}
	}

	private void exp_PreviewAIHeatMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (exp_PreviewAIHeatMap.IsExpanded)
		{
			UIElement uIElement = sender as UIElement;
			if (isDragging && uIElement != null)
			{
				System.Windows.Point position = e.GetPosition(base.Parent as UIElement);
				TranslateTransform obj = uIElement.RenderTransform as TranslateTransform;
				obj.X = position.X - startPosition.X;
				obj.Y = position.Y - startPosition.Y;
			}
		}
	}

	private void exp_PreviewAIHeatMap_Expanded(object sender, RoutedEventArgs e)
	{
		Btn_TestAll_Click(null, null);
	}

	private void btn_UnLockAuth_Click(object sender, RoutedEventArgs e)
	{
		CheckAuth();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AddPTrainNGImage_Click(object sender, RoutedEventArgs e)
	{
		string modelName = Station[StationIndex].paramFile.AIProphetParam.ModelName;
		if (modelName == "")
		{
			System.Windows.MessageBox.Show("請先設定及選擇AI模型。", "警告");
			return;
		}
		string text = string.Format("{0}PModel\\{1}\\NOPASS\\", AppDomain.CurrentDomain.BaseDirectory, modelName);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!Directory.Exists(text))
		{
			System.Windows.MessageBox.Show(string.Format("AI NG資料夾建立失敗，請重新設定AI影像路徑。({0})", text), "警告");
			return;
		}
		try
		{
			Btn_TestAll_Click(this, null);
			if (Station[StationIndex].aiProphetTools.org.Width <= 0 || Station[StationIndex].aiProphetTools.org.Height <= 0)
			{
				System.Windows.MessageBox.Show("AI影像生成前錯誤，請檢查AI先知檢測框設定是否有錯誤。", "警告");
				return;
			}
			DateTime now = DateTime.Now;
			string text2 = string.Format("NG-{0}-{1}.png", StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string fileName = string.Format("{0}{1}", text, text2);
			Station[StationIndex].aiProphetTools.org.SaveImage(fileName);
			string str = ((LanguageType == 0) ? string.Format("{0} Save AI Image Finish. (NG)", text2) : ((LanguageType != 1) ? string.Format("{0} AI 影像储存成功! (NG)", text2) : string.Format("{0} AI 訓練影像儲存成功。 (NG)", text2)));
			ShowLabMessage(str);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-btn_AddPTrainImage_Click()" + ex.ToString());
		}
	}

	private void btn_AutoMarked_Click(object sender, RoutedEventArgs e)
	{
		Action method = delegate
		{
			grd_AILoading.Visibility = Visibility.Visible;
			AIAutoDrawPauseEvent.Set();
		};
		base.Dispatcher.BeginInvoke(method);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void BackupRecipe(string recipename, string RecipeBackupPath = "D:\\RecipeBackup\\")
	{
		try
		{
			if (!Directory.Exists(RecipeBackupPath))
			{
				Directory.CreateDirectory(RecipeBackupPath);
			}
			string text = System.IO.Path.Combine(RecipeBackupPath, "temp");
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			DirectoryInfo directoryInfo = new DirectoryInfo(string.Format("{0}Recipe\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, recipename));
			if (!directoryInfo.Exists)
			{
				throw new DirectoryNotFoundException("Source directory not found: " + directoryInfo.FullName);
			}
			directoryInfo.GetDirectories();
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				string destFileName = System.IO.Path.Combine(text, fileInfo.Name);
				fileInfo.CopyTo(destFileName, overwrite: true);
			}
			DateTime now = DateTime.Now;
			string text2 = string.Format("{0}{1}\\", RecipeBackupPath, now.ToString("yyyy-MM"));
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string text3 = string.Format("{0}{1}\\", text2, now.ToString("dd"));
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			string text4 = string.Format("{0}{1}.zip", text3, recipename);
			if (File.Exists(text4))
			{
				File.Delete(text4);
			}
			ZipFile.CreateFromDirectory(text, text4);
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ToolsWindow-BackupRecipe()" + ex.ToString());
		}
	}

	private void btn_GerNG_Click(object sender, RoutedEventArgs e)
	{
		if (CheckBufferAndRef() && (Station[StationIndex].maskTool.AIMat.Width > 0 || Station[StationIndex].maskTool.AIMat.Height > 0))
		{
			NGCloneWindow nGCloneWindow = new NGCloneWindow();
			nGCloneWindow.LoadLanguage(LanguageType);
			nGCloneWindow.SetInspection(Station[StationIndex]);
			nGCloneWindow.setThroughputParamFile(ThroughputFile);
			nGCloneWindow.SetSAM(mSam);
			nGCloneWindow.SetBuffer(Station[StationIndex].maskTool.AIMat, Station[StationIndex].buffers.Src);
			nGCloneWindow.ShowDialog();
		}
	}

	private void CheckErrorLog()
	{
	}

	public static bool IsFileLocked(string file)
	{
		try
		{
			using (File.Open(file, FileMode.Open, FileAccess.Write, FileShare.None))
			{
				return false;
			}
		}
		catch (IOException e)
		{
			int num = Marshal.GetHRForException(e) & 0xFFFF;
			return num == 32 || num == 33;
		}
		catch (Exception)
		{
			return false;
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
			Uri resourceLocator = new Uri("/InspTools;component/toolswindow.xaml", UriKind.Relative);
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
			Main = (ToolsWindow)target;
			Main.Closed += Window_Closed;
			Main.Closing += Window_Closing;
			Main.DragEnter += Window_DragEnter;
			Main.Drop += Window_Drop;
			Main.Loaded += MetroWindow_Loaded;
			Main.Activated += Main_Activated;
			Main.IsVisibleChanged += Main_IsVisibleChanged;
			Main.KeyDown += Main_KeyDown;
			break;
		case 2:
			grd_Main = (Grid)target;
			grd_Main.MouseWheel += Grd_Main_MouseWheel;
			grd_Main.KeyDown += grd_Main_KeyDown;
			break;
		case 3:
			cnv_Image = (Canvas)target;
			cnv_Image.MouseLeftButtonDown += Cnv_Image_MouseLeftButtonDown;
			cnv_Image.MouseLeftButtonUp += Cnv_Image_MouseLeftButtonUp;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			cnv_Image.PreviewMouseLeftButtonDown += Cnv_Image_PreviewMouseLeftButtonDown;
			break;
		case 4:
			sfr = (ScaleTransform)target;
			break;
		case 5:
			Label_Result = (System.Windows.Controls.Label)target;
			break;
		case 6:
			btn_Result_Flip = (System.Windows.Controls.Button)target;
			break;
		case 7:
			lab_Channel = (System.Windows.Controls.Label)target;
			break;
		case 8:
			lab_Message = (System.Windows.Controls.Label)target;
			break;
		case 9:
			grd_TestEnhance = (Grid)target;
			break;
		case 10:
			chk_EnableVMirror = (System.Windows.Controls.CheckBox)target;
			chk_EnableVMirror.Click += chk_EnableVMirror_Click;
			break;
		case 11:
			chk_EnableHMirror = (System.Windows.Controls.CheckBox)target;
			chk_EnableHMirror.Click += chk_EnableVMirror_Click;
			break;
		case 12:
			nu_Hue = (MahApps.Metro.Controls.NumericUpDown)target;
			nu_Hue.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 13:
			nu_Sat = (MahApps.Metro.Controls.NumericUpDown)target;
			nu_Sat.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 14:
			nu_Val = (MahApps.Metro.Controls.NumericUpDown)target;
			nu_Val.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 15:
			nu_Red = (MahApps.Metro.Controls.NumericUpDown)target;
			nu_Red.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 16:
			nu_Green = (MahApps.Metro.Controls.NumericUpDown)target;
			nu_Green.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 17:
			nu_Blue = (MahApps.Metro.Controls.NumericUpDown)target;
			nu_Blue.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 18:
			((System.Windows.Controls.Label)target).PreviewMouseLeftButtonDown += Label_PreviewMouseLeftButtonDown;
			break;
		case 19:
			btn_TestEnhanceReset = (System.Windows.Controls.Button)target;
			btn_TestEnhanceReset.Click += btn_TestEnhanceReset_Click;
			break;
		case 20:
			grd_ImageSourceSelect = (Grid)target;
			break;
		case 21:
			btn_ImageNum = (System.Windows.Controls.Button)target;
			btn_ImageNum.Click += Btn_ImageNum_Click;
			break;
		case 22:
			btn_PrevImage = (System.Windows.Controls.Button)target;
			btn_PrevImage.Click += Btn_PrevImage_Click;
			break;
		case 23:
			btn_TestAllImage = (System.Windows.Controls.Button)target;
			btn_TestAllImage.Click += Btn_TestAllImage_Click;
			break;
		case 24:
			btn_NextImage = (System.Windows.Controls.Button)target;
			btn_NextImage.Click += Btn_NextImage_Click;
			break;
		case 25:
			chk_OKStop = (System.Windows.Controls.CheckBox)target;
			chk_OKStop.Checked += Chk_OKStop_Checked;
			chk_OKStop.Unchecked += Chk_OKStop_Unchecked;
			break;
		case 26:
			chk_NGStop = (System.Windows.Controls.CheckBox)target;
			chk_NGStop.Checked += Chk_NGStop_Checked;
			chk_NGStop.Unchecked += Chk_NGStop_Unchecked;
			break;
		case 27:
			btn_UseRecentBufList = (System.Windows.Controls.Button)target;
			btn_UseRecentBufList.Click += Btn_UseRecentBufList_Click;
			break;
		case 28:
			btn_UseOKBufList = (System.Windows.Controls.Button)target;
			btn_UseOKBufList.Click += Btn_UseRecentBufList_Click;
			break;
		case 29:
			btn_UseNGBufList = (System.Windows.Controls.Button)target;
			btn_UseNGBufList.Click += Btn_UseRecentBufList_Click;
			break;
		case 30:
			btn_UseFileBufList = (System.Windows.Controls.Button)target;
			btn_UseFileBufList.Click += Btn_UseRecentBufList_Click;
			break;
		case 31:
			btn_UseOpenBitmap = (System.Windows.Controls.Button)target;
			btn_UseOpenBitmap.Click += Btn_UseRecentBufList_Click;
			break;
		case 32:
			btn_BitmapSave = (System.Windows.Controls.Button)target;
			btn_BitmapSave.Click += Btn_BitmapSave_Click;
			break;
		case 33:
			btn_ReferenceShow = (System.Windows.Controls.Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 34:
			btn_UseRetestBufList = (System.Windows.Controls.Button)target;
			btn_UseRetestBufList.Click += Btn_UseRecentBufList_Click;
			break;
		case 35:
			btn_SetAll = (System.Windows.Controls.Button)target;
			btn_SetAll.Click += Btn_SetAll_Click;
			break;
		case 36:
			btn_NGDB = (System.Windows.Controls.Button)target;
			btn_NGDB.Click += Btn_NGDB_Click;
			break;
		case 37:
			btn_OKDB = (System.Windows.Controls.Button)target;
			btn_OKDB.Click += Btn_OKDB_Click;
			break;
		case 38:
			btn_RemoveFromDB = (System.Windows.Controls.Button)target;
			break;
		case 39:
			btn_AddToDB = (System.Windows.Controls.Button)target;
			btn_AddToDB.Click += Btn_AddToDB_Click;
			break;
		case 40:
			btn_AIBitmapSave = (System.Windows.Controls.Button)target;
			btn_AIBitmapSave.Click += Btn_AIBitmapSave_Click;
			btn_AIBitmapSave.MouseEnter += Btn_AIBitmapSave_MouseEnter;
			btn_AIBitmapSave.MouseLeave += Btn_AIBitmapSave_MouseLeave;
			break;
		case 41:
			btn_ImgT1 = (System.Windows.Controls.Button)target;
			btn_ImgT1.Click += Btn_ImgT1_Click;
			btn_ImgT1.MouseRightButtonDown += Btn_ImgT1_MouseRightButtonDown;
			break;
		case 42:
			btn_ImgT2 = (System.Windows.Controls.Button)target;
			btn_ImgT2.Click += Btn_ImgT1_Click;
			btn_ImgT2.MouseRightButtonDown += Btn_ImgT1_MouseRightButtonDown;
			break;
		case 43:
			btn_ImgT3 = (System.Windows.Controls.Button)target;
			btn_ImgT3.Click += Btn_ImgT1_Click;
			btn_ImgT3.MouseRightButtonDown += Btn_ImgT1_MouseRightButtonDown;
			break;
		case 44:
			btn_AINGDB = (System.Windows.Controls.Button)target;
			btn_AINGDB.Click += Btn_AINGDB_Click;
			break;
		case 45:
			btn_AIOKDB = (System.Windows.Controls.Button)target;
			btn_AIOKDB.Click += Btn_AIOKDB_Click;
			break;
		case 46:
			btn_YieldRetest = (System.Windows.Controls.Button)target;
			btn_YieldRetest.Click += btn_YieldRetest_Click;
			break;
		case 47:
			txt_OKCount = (System.Windows.Controls.TextBox)target;
			break;
		case 48:
			txt_OKCountRatio = (System.Windows.Controls.TextBox)target;
			break;
		case 49:
			txt_NGCount = (System.Windows.Controls.TextBox)target;
			break;
		case 50:
			txt_NGCountRatio = (System.Windows.Controls.TextBox)target;
			break;
		case 51:
			lab_AutestState = (System.Windows.Controls.Label)target;
			break;
		case 52:
			lab_FileName = (System.Windows.Controls.Label)target;
			break;
		case 53:
			pg_TestProgress = (System.Windows.Controls.ProgressBar)target;
			break;
		case 54:
			btn_DelAIOKDB = (System.Windows.Controls.Button)target;
			btn_DelAIOKDB.Click += Btn_DelAIOKDB_Click;
			break;
		case 55:
			grd_AINGQuickDraw = (Grid)target;
			break;
		case 56:
			lab_QuickAIDraw = (System.Windows.Controls.Label)target;
			break;
		case 57:
			btn_DrawTempMask = (System.Windows.Controls.Button)target;
			btn_DrawTempMask.Click += Btn_DrawTempMask_Click;
			break;
		case 58:
			btn_AddToNGTemp = (System.Windows.Controls.Button)target;
			btn_AddToNGTemp.Click += Btn_AddToNGTemp_Click;
			break;
		case 59:
			lab_AINGTempCount = (System.Windows.Controls.Label)target;
			break;
		case 60:
			chk_TestEnhanceEnabled = (System.Windows.Controls.CheckBox)target;
			chk_TestEnhanceEnabled.Click += chk_TestEnhanceEnabled_Click;
			break;
		case 61:
			btn_ImgAILR = (System.Windows.Controls.Button)target;
			btn_ImgAILR.Click += Btn_ImgSrc_Click;
			break;
		case 62:
			btn_ImgAILG = (System.Windows.Controls.Button)target;
			btn_ImgAILG.Click += Btn_ImgSrc_Click;
			break;
		case 63:
			btn_ImgAILB = (System.Windows.Controls.Button)target;
			btn_ImgAILB.Click += Btn_ImgSrc_Click;
			break;
		case 64:
			grd_Channel = (Grid)target;
			break;
		case 65:
			btn_ImgSrc = (System.Windows.Controls.Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 66:
			btn_ImgR = (System.Windows.Controls.Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 67:
			btn_ImgG = (System.Windows.Controls.Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 68:
			btn_ImgB = (System.Windows.Controls.Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 69:
			btn_ImgH = (System.Windows.Controls.Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 70:
			btn_ImgS = (System.Windows.Controls.Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 71:
			btn_ImgV = (System.Windows.Controls.Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 72:
			btn_ImgGray = (System.Windows.Controls.Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 73:
			btn_ImgC1 = (System.Windows.Controls.Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 74:
			btn_ImgC2 = (System.Windows.Controls.Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 75:
			btn_ImgC3 = (System.Windows.Controls.Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 76:
			btn_ImgAI = (System.Windows.Controls.Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 77:
			btn_ImgAIR = (System.Windows.Controls.Button)target;
			btn_ImgAIR.Click += Btn_ImgSrc_Click;
			break;
		case 78:
			btn_ImgAIG = (System.Windows.Controls.Button)target;
			btn_ImgAIG.Click += Btn_ImgSrc_Click;
			break;
		case 79:
			btn_ImgAIB = (System.Windows.Controls.Button)target;
			btn_ImgAIB.Click += Btn_ImgSrc_Click;
			break;
		case 80:
			btn_ImgC4 = (System.Windows.Controls.Button)target;
			btn_ImgC4.Click += Btn_ImgSrc_Click;
			break;
		case 81:
			btn_ImgC5 = (System.Windows.Controls.Button)target;
			btn_ImgC5.Click += Btn_ImgSrc_Click;
			break;
		case 82:
			btn_ImgC6 = (System.Windows.Controls.Button)target;
			btn_ImgC6.Click += Btn_ImgSrc_Click;
			break;
		case 83:
			btn_ChannelExpend = (System.Windows.Controls.Button)target;
			btn_ChannelExpend.Click += Btn_ChannelExpend_Click;
			break;
		case 84:
			btn_ImgAI2 = (System.Windows.Controls.Button)target;
			btn_ImgAI2.Click += Btn_ImgSrc_Click;
			break;
		case 85:
			gb_TestResult = (System.Windows.Controls.GroupBox)target;
			break;
		case 86:
			txt_InspResult = (System.Windows.Controls.TextBox)target;
			break;
		case 87:
			rt_result01 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 88:
			rt_result02 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 89:
			rt_result03 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 90:
			rt_result04 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 91:
			rt_result05 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 92:
			rt_result06 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 93:
			rt_result07 = (System.Windows.Shapes.Rectangle)target;
			break;
		case 94:
			rt_result01_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 95:
			rt_result02_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 96:
			rt_result03_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 97:
			rt_result04_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 98:
			rt_result05_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 99:
			rt_result06_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 100:
			rt_result07_W = (System.Windows.Shapes.Rectangle)target;
			break;
		case 101:
			chk_LocateToolON = (System.Windows.Controls.CheckBox)target;
			chk_LocateToolON.Checked += Ts_LocateToolON_Checked;
			chk_LocateToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 102:
			chk_SelectWayON = (System.Windows.Controls.CheckBox)target;
			chk_SelectWayON.Checked += Ts_LocateToolON_Checked;
			chk_SelectWayON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 103:
			chk_EnhanceToolON = (System.Windows.Controls.CheckBox)target;
			chk_EnhanceToolON.Checked += Ts_LocateToolON_Checked;
			chk_EnhanceToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 104:
			chk_AdvLocateToolON = (System.Windows.Controls.CheckBox)target;
			chk_AdvLocateToolON.Checked += Ts_LocateToolON_Checked;
			chk_AdvLocateToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 105:
			chk_MeasureToolON = (System.Windows.Controls.CheckBox)target;
			chk_MeasureToolON.Checked += Ts_LocateToolON_Checked;
			chk_MeasureToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 106:
			ep_Result = (Ellipse)target;
			break;
		case 107:
			lab_TestResult = (System.Windows.Controls.Label)target;
			lab_TestResult.PreviewMouseDoubleClick += Lab_TestResult_PreviewMouseDoubleClick;
			break;
		case 108:
			lab_StatinoIndex = (System.Windows.Controls.Label)target;
			lab_StatinoIndex.PreviewMouseDoubleClick += Lab_StatinoIndex_PreviewMouseDoubleClick;
			break;
		case 109:
			btn_LocateTool = (System.Windows.Controls.Button)target;
			btn_LocateTool.Click += Btn_LocateTool_Click;
			break;
		case 110:
			btn_BitmapSave_Copy = (System.Windows.Controls.Button)target;
			break;
		case 111:
			btn_SelectWayTool = (System.Windows.Controls.Button)target;
			btn_SelectWayTool.Click += Btn_SelectWayTool_Click;
			break;
		case 112:
			btn_BitmapSave_Copy1 = (System.Windows.Controls.Button)target;
			break;
		case 113:
			btn_SelectFaceTool = (System.Windows.Controls.Button)target;
			btn_SelectFaceTool.Click += Btn_SelectFaceTool_Click;
			break;
		case 114:
			btn_BitmapSave_Copy6 = (System.Windows.Controls.Button)target;
			break;
		case 115:
			btn_EnhanceTool = (System.Windows.Controls.Button)target;
			btn_EnhanceTool.Click += Btn_EnhanceTool_Click;
			break;
		case 116:
			btn_BitmapSave_Copy2 = (System.Windows.Controls.Button)target;
			break;
		case 117:
			btn_AIProphet = (System.Windows.Controls.Button)target;
			btn_AIProphet.Click += btn_AIProphet_Click;
			break;
		case 118:
			btn_BitmapSave_Copy17 = (System.Windows.Controls.Button)target;
			break;
		case 119:
			btn_AIEnhanceTool = (System.Windows.Controls.Button)target;
			btn_AIEnhanceTool.Click += Btn_AIEnhanceTool_Click;
			break;
		case 120:
			btn_BitmapSave_Copy15 = (System.Windows.Controls.Button)target;
			break;
		case 121:
			btn_SmoothTool = (System.Windows.Controls.Button)target;
			btn_SmoothTool.Click += Btn_SmoothTool_Click;
			break;
		case 122:
			btn_BitmapSave_Copy5 = (System.Windows.Controls.Button)target;
			break;
		case 123:
			btn_BinaryTool = (System.Windows.Controls.Button)target;
			btn_BinaryTool.Click += Btn_BinaryTool_Click;
			break;
		case 124:
			btn_BitmapSave_Copy4 = (System.Windows.Controls.Button)target;
			break;
		case 125:
			btn_MaskTool = (System.Windows.Controls.Button)target;
			btn_MaskTool.Click += Btn_MaskTool_Click;
			break;
		case 126:
			btn_BitmapSave_Copy3 = (System.Windows.Controls.Button)target;
			break;
		case 127:
			btn_MeasureTool = (System.Windows.Controls.Button)target;
			btn_MeasureTool.Click += Btn_MeasureTool_Click;
			break;
		case 128:
			btn_BitmapSave_Copy12 = (System.Windows.Controls.Button)target;
			break;
		case 129:
			btn_AdvancedLocate = (System.Windows.Controls.Button)target;
			btn_AdvancedLocate.Click += Btn_AdvancedLocate_Click;
			break;
		case 130:
			btn_BitmapSave_Copy7 = (System.Windows.Controls.Button)target;
			break;
		case 131:
			btn_BlockAvgTool = (System.Windows.Controls.Button)target;
			btn_BlockAvgTool.Click += Btn_BlockAvgTool_Click;
			break;
		case 132:
			btn_BitmapSave_Copy8 = (System.Windows.Controls.Button)target;
			break;
		case 133:
			btn_DifferenceTool = (System.Windows.Controls.Button)target;
			btn_DifferenceTool.Click += Btn_DifferenceTool_Click;
			break;
		case 134:
			btn_BitmapSave_Copy9 = (System.Windows.Controls.Button)target;
			break;
		case 135:
			btn_LineAnalysis = (System.Windows.Controls.Button)target;
			btn_LineAnalysis.Click += Btn_LineAnalysis_Click;
			break;
		case 136:
			btn_BitmapSave_Copy11 = (System.Windows.Controls.Button)target;
			break;
		case 137:
			btn_MatchTool = (System.Windows.Controls.Button)target;
			btn_MatchTool.Click += Btn_MatchTool_Click;
			break;
		case 138:
			btn_BitmapSave_Copy10 = (System.Windows.Controls.Button)target;
			break;
		case 139:
			btn_AIPredictTool = (System.Windows.Controls.Button)target;
			btn_AIPredictTool.Click += Btn_AIPredictTool_Click;
			break;
		case 140:
			btn_BitmapSave_Copy13 = (System.Windows.Controls.Button)target;
			break;
		case 141:
			chk_MaskToolON = (System.Windows.Controls.CheckBox)target;
			chk_MaskToolON.Checked += Ts_LocateToolON_Checked;
			chk_MaskToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 142:
			chk_BinaryToolON = (System.Windows.Controls.CheckBox)target;
			chk_BinaryToolON.Checked += Ts_LocateToolON_Checked;
			chk_BinaryToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 143:
			chk_SmoothToolON = (System.Windows.Controls.CheckBox)target;
			chk_SmoothToolON.Checked += Ts_LocateToolON_Checked;
			chk_SmoothToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 144:
			chk_BlockAvgToolON = (System.Windows.Controls.CheckBox)target;
			chk_BlockAvgToolON.Checked += Ts_LocateToolON_Checked;
			chk_BlockAvgToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 145:
			chk_DifferenceToolON = (System.Windows.Controls.CheckBox)target;
			chk_DifferenceToolON.Checked += Ts_LocateToolON_Checked;
			chk_DifferenceToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 146:
			chk_MatchToolON = (System.Windows.Controls.CheckBox)target;
			chk_MatchToolON.Checked += Ts_LocateToolON_Checked;
			chk_MatchToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 147:
			chk_LineAnalysisON = (System.Windows.Controls.CheckBox)target;
			chk_LineAnalysisON.Checked += Ts_LocateToolON_Checked;
			chk_LineAnalysisON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 148:
			chk_AIPredictToolON = (System.Windows.Controls.CheckBox)target;
			chk_AIPredictToolON.Checked += Ts_LocateToolON_Checked;
			chk_AIPredictToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 149:
			btn_TestAll = (System.Windows.Controls.Button)target;
			btn_TestAll.Click += Btn_TestAll_Click;
			break;
		case 150:
			lab_Time1 = (System.Windows.Controls.Label)target;
			break;
		case 151:
			lab_Time2 = (System.Windows.Controls.Label)target;
			break;
		case 152:
			lab_Time3 = (System.Windows.Controls.Label)target;
			break;
		case 153:
			lab_Time4 = (System.Windows.Controls.Label)target;
			break;
		case 154:
			lab_Time5 = (System.Windows.Controls.Label)target;
			break;
		case 155:
			lab_Time6 = (System.Windows.Controls.Label)target;
			break;
		case 156:
			lab_Time7 = (System.Windows.Controls.Label)target;
			break;
		case 157:
			lab_Time8 = (System.Windows.Controls.Label)target;
			break;
		case 158:
			lab_TotalTime = (System.Windows.Controls.Label)target;
			break;
		case 159:
			lab_Time9 = (System.Windows.Controls.Label)target;
			break;
		case 160:
			lab_Time11 = (System.Windows.Controls.Label)target;
			break;
		case 161:
			lab_Time10 = (System.Windows.Controls.Label)target;
			break;
		case 162:
			lab_Time12 = (System.Windows.Controls.Label)target;
			break;
		case 163:
			chk_SelectFaceON = (System.Windows.Controls.CheckBox)target;
			chk_SelectFaceON.Checked += Ts_LocateToolON_Checked;
			chk_SelectFaceON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 164:
			lab_Time2_2 = (System.Windows.Controls.Label)target;
			break;
		case 165:
			lab_FaceIndex = (System.Windows.Controls.Label)target;
			break;
		case 166:
			es_FaceIndex = (Ellipse)target;
			break;
		case 167:
			lab_FaceIndex2 = (System.Windows.Controls.Label)target;
			break;
		case 168:
			es_01 = (Ellipse)target;
			break;
		case 169:
			lab_Error01 = (System.Windows.Controls.Label)target;
			break;
		case 170:
			es_03 = (Ellipse)target;
			break;
		case 171:
			lab_Error03 = (System.Windows.Controls.Label)target;
			break;
		case 172:
			es_04 = (Ellipse)target;
			break;
		case 173:
			lab_Error04 = (System.Windows.Controls.Label)target;
			break;
		case 174:
			es_00 = (Ellipse)target;
			break;
		case 175:
			lab_Error00 = (System.Windows.Controls.Label)target;
			break;
		case 176:
			es_07 = (Ellipse)target;
			break;
		case 177:
			lab_Error07 = (System.Windows.Controls.Label)target;
			break;
		case 178:
			es_08 = (Ellipse)target;
			break;
		case 179:
			lab_Error08 = (System.Windows.Controls.Label)target;
			break;
		case 180:
			es_06 = (Ellipse)target;
			break;
		case 181:
			lab_Error06 = (System.Windows.Controls.Label)target;
			break;
		case 182:
			es_05 = (Ellipse)target;
			break;
		case 183:
			lab_Error05 = (System.Windows.Controls.Label)target;
			break;
		case 184:
			es_09 = (Ellipse)target;
			break;
		case 185:
			lab_Error09 = (System.Windows.Controls.Label)target;
			break;
		case 186:
			es_12 = (Ellipse)target;
			break;
		case 187:
			lab_Error12 = (System.Windows.Controls.Label)target;
			break;
		case 188:
			es_11 = (Ellipse)target;
			break;
		case 189:
			lab_Error11 = (System.Windows.Controls.Label)target;
			break;
		case 190:
			es_10 = (Ellipse)target;
			break;
		case 191:
			lab_Error10 = (System.Windows.Controls.Label)target;
			break;
		case 192:
			btn_ParetoChart = (System.Windows.Controls.Button)target;
			btn_ParetoChart.Click += Btn_ParetoChart_Click;
			break;
		case 193:
			btn_SizeStatic = (System.Windows.Controls.Button)target;
			btn_SizeStatic.Click += Btn_SizeStatic_Click;
			break;
		case 194:
			cb_ParetoNGSelect = (System.Windows.Controls.ComboBox)target;
			cb_ParetoNGSelect.SelectionChanged += Cb_ParetoNGSelect_SelectionChanged;
			break;
		case 195:
			lab_ParetoNGSelect = (System.Windows.Controls.Label)target;
			break;
		case 196:
			chk_SaveAI = (System.Windows.Controls.CheckBox)target;
			chk_SaveAI.Click += Chk_SaveAI_Click;
			break;
		case 197:
			btn_IRTool = (System.Windows.Controls.Button)target;
			btn_IRTool.Click += btn_IRTool_Click;
			break;
		case 198:
			btn_BitmapSave_Copy14 = (System.Windows.Controls.Button)target;
			break;
		case 199:
			lab_TimeAIEhance = (System.Windows.Controls.Label)target;
			break;
		case 200:
			chk_AIEnhanceToolON = (System.Windows.Controls.CheckBox)target;
			chk_AIEnhanceToolON.Checked += Ts_LocateToolON_Checked;
			chk_AIEnhanceToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 201:
			btn_CancleSave = (System.Windows.Controls.Button)target;
			btn_CancleSave.Click += Btn_CancleSave_Click;
			break;
		case 202:
			btn_AITrain = (System.Windows.Controls.Button)target;
			btn_AITrain.Click += Btn_AITrain_Click;
			break;
		case 203:
			lab_TimeAIPredict = (System.Windows.Controls.Label)target;
			break;
		case 204:
			es_AIPredict = (Ellipse)target;
			break;
		case 205:
			lab_ErrorAIPredict = (System.Windows.Controls.Label)target;
			break;
		case 206:
			btn_AutoMatch = (System.Windows.Controls.Button)target;
			btn_AutoMatch.Click += Btn_AutoMatch_Click;
			break;
		case 207:
			b1 = (System.Windows.Controls.Button)target;
			b1.Click += B1_Click;
			break;
		case 208:
			b2 = (System.Windows.Controls.Button)target;
			b2.Click += B2_Click;
			break;
		case 209:
			b3 = (System.Windows.Controls.Button)target;
			b3.Click += B3_Click;
			break;
		case 210:
			chk_SaveFontAI = (System.Windows.Controls.CheckBox)target;
			chk_SaveFontAI.Click += Chk_SaveFontAI_Click;
			break;
		case 211:
			btn_InspSurv = (System.Windows.Controls.Button)target;
			btn_InspSurv.Click += Btn_InspSurv_Click;
			break;
		case 212:
			btn_test = (System.Windows.Controls.Button)target;
			btn_test.Click += Btn_test_Click;
			break;
		case 213:
			grd_AIFunction = (Grid)target;
			grd_AIFunction.MouseLeftButtonDown += Grd_AIFunction_MouseLeftButtonDown;
			grd_AIFunction.MouseLeftButtonUp += Grd_AIFunction_MouseLeftButtonUp;
			grd_AIFunction.MouseMove += Grd_AIFunction_MouseMove;
			break;
		case 214:
			tt2 = (TranslateTransform)target;
			break;
		case 215:
			btn_AI_Function = (System.Windows.Controls.Button)target;
			btn_AI_Function.Click += Btn_AI_Function_Click;
			break;
		case 216:
			btn_DrawMask = (System.Windows.Controls.Button)target;
			btn_DrawMask.Click += Btn_DrawMask_Click;
			break;
		case 217:
			btn_AddToNG = (System.Windows.Controls.Button)target;
			btn_AddToNG.Click += Btn_AddToNG_Click;
			break;
		case 218:
			btn_AddToOK = (System.Windows.Controls.Button)target;
			btn_AddToOK.Click += Btn_AddToOK_Click;
			break;
		case 219:
			btn_OpenFolder = (System.Windows.Controls.Button)target;
			btn_OpenFolder.Click += Btn_OpenFolder_Click;
			break;
		case 220:
			es_201 = (Ellipse)target;
			break;
		case 221:
			es_202 = (Ellipse)target;
			break;
		case 222:
			es_203 = (Ellipse)target;
			break;
		case 223:
			lab_Error201 = (System.Windows.Controls.Label)target;
			break;
		case 224:
			lab_Error202 = (System.Windows.Controls.Label)target;
			break;
		case 225:
			lab_Error203 = (System.Windows.Controls.Label)target;
			break;
		case 226:
			btn_SetupAIFolder = (System.Windows.Controls.Button)target;
			btn_SetupAIFolder.Click += Btn_SetupAIFolder_Click;
			break;
		case 227:
			lab_ModelName = (System.Windows.Controls.Label)target;
			break;
		case 228:
			btn_AIPreview = (System.Windows.Controls.Button)target;
			btn_AIPreview.Click += Btn_AIPreview_Click;
			break;
		case 229:
			btn_GerNG = (System.Windows.Controls.Button)target;
			btn_GerNG.Click += btn_GerNG_Click;
			break;
		case 230:
			btn_AutoMarked = (System.Windows.Controls.Button)target;
			btn_AutoMarked.Click += btn_AutoMarked_Click;
			break;
		case 231:
			lab_RemoteMode = (System.Windows.Controls.TextBox)target;
			break;
		case 232:
			pb_TrainingProcess = (System.Windows.Controls.ProgressBar)target;
			break;
		case 233:
			lab_TrainingProcess = (System.Windows.Controls.Label)target;
			break;
		case 234:
			es_AITrainFinish = (Ellipse)target;
			break;
		case 235:
			exp_PreviewAI = (Expander)target;
			exp_PreviewAI.MouseLeftButtonDown += Exp_PreviewAI_MouseLeftButtonDown;
			exp_PreviewAI.MouseLeftButtonUp += Exp_PreviewAI_MouseLeftButtonUp;
			exp_PreviewAI.MouseMove += Exp_PreviewAI_MouseMove;
			exp_PreviewAI.Expanded += Exp_PreviewAI_Expanded;
			break;
		case 236:
			tt = (TranslateTransform)target;
			break;
		case 237:
			grd_AIImage = (Grid)target;
			break;
		case 238:
			cnv_AIImage = (Canvas)target;
			break;
		case 239:
			grd_AILoading = (Grid)target;
			break;
		case 240:
			btn_FontTool = (System.Windows.Controls.Button)target;
			btn_FontTool.Click += Btn_FontTool_Click;
			break;
		case 241:
			btn_BitmapSave_Copy16 = (System.Windows.Controls.Button)target;
			break;
		case 242:
			chk_FontToolON = (System.Windows.Controls.CheckBox)target;
			chk_FontToolON.Checked += Ts_LocateToolON_Checked;
			chk_FontToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 243:
			lab_TimeFontTool = (System.Windows.Controls.Label)target;
			break;
		case 244:
			es_FontTool = (Ellipse)target;
			break;
		case 245:
			lab_ErrorFontTool = (System.Windows.Controls.Label)target;
			break;
		case 246:
			btn_AutoFontMatch = (System.Windows.Controls.Button)target;
			btn_AutoFontMatch.Click += Btn_AutoFontMatch_Click;
			break;
		case 247:
			lab_CpuUseOptimized = (System.Windows.Controls.Label)target;
			break;
		case 248:
			lab_EnableSetOpenCVThreadNum = (System.Windows.Controls.Label)target;
			break;
		case 249:
			lab_EnableGPU = (System.Windows.Controls.Label)target;
			break;
		case 250:
			lab_ThreadNum = (System.Windows.Controls.Label)target;
			break;
		case 251:
			lab_Version = (System.Windows.Controls.Label)target;
			break;
		case 252:
			lab_FontToolText = (System.Windows.Controls.Label)target;
			break;
		case 253:
			btn_OpenAIFontFolder = (System.Windows.Controls.Button)target;
			btn_OpenAIFontFolder.Click += Btn_OpenFontFolder_Click;
			break;
		case 254:
			chk_SaveFontAI2 = (System.Windows.Controls.CheckBox)target;
			chk_SaveFontAI2.Click += Chk_SaveFontAI_Click;
			break;
		case 255:
			lab_AITrainFinish = (System.Windows.Controls.Label)target;
			break;
		case 256:
			txt_b1 = (System.Windows.Controls.TextBox)target;
			break;
		case 257:
			chk_IRToolON = (System.Windows.Controls.CheckBox)target;
			chk_IRToolON.Checked += Ts_LocateToolON_Checked;
			chk_IRToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 258:
			lab_TimeIRTool = (System.Windows.Controls.Label)target;
			break;
		case 259:
			es_IRTool = (Ellipse)target;
			break;
		case 260:
			lab_ErrorIRTool = (System.Windows.Controls.Label)target;
			break;
		case 261:
			lab_SaveAIImage = (System.Windows.Controls.Label)target;
			break;
		case 262:
			btn_LogAlert = (System.Windows.Controls.Button)target;
			btn_LogAlert.Click += btn_LogAlert_Click;
			break;
		case 263:
			lab_User = (System.Windows.Controls.Label)target;
			break;
		case 264:
			lab_Customer = (System.Windows.Controls.Label)target;
			break;
		case 265:
			btn_ParamCopy = (System.Windows.Controls.Button)target;
			btn_ParamCopy.Click += Btn_ParamCopy_Click;
			break;
		case 266:
			btn_Stage1 = (System.Windows.Controls.Button)target;
			btn_Stage1.Click += Btn_Stage1_Click;
			break;
		case 267:
			btn_Stage2 = (System.Windows.Controls.Button)target;
			btn_Stage2.Click += Btn_Stage1_Click;
			break;
		case 268:
			btn_Stage3 = (System.Windows.Controls.Button)target;
			btn_Stage3.Click += Btn_Stage1_Click;
			break;
		case 269:
			btn_Stage4 = (System.Windows.Controls.Button)target;
			btn_Stage4.Click += Btn_Stage1_Click;
			break;
		case 270:
			btn_Stage5 = (System.Windows.Controls.Button)target;
			btn_Stage5.Click += Btn_Stage1_Click;
			break;
		case 271:
			btn_Stage6 = (System.Windows.Controls.Button)target;
			btn_Stage6.Click += Btn_Stage1_Click;
			break;
		case 272:
			btn_Stage7 = (System.Windows.Controls.Button)target;
			btn_Stage7.Click += Btn_Stage1_Click;
			break;
		case 273:
			btn_Stage8 = (System.Windows.Controls.Button)target;
			btn_Stage8.Click += Btn_Stage1_Click;
			break;
		case 274:
			btn_Stage9 = (System.Windows.Controls.Button)target;
			btn_Stage9.Click += Btn_Stage1_Click;
			break;
		case 275:
			btn_Stage10 = (System.Windows.Controls.Button)target;
			btn_Stage10.Click += Btn_Stage1_Click;
			break;
		case 276:
			chk_AIProphetToolON = (System.Windows.Controls.CheckBox)target;
			chk_AIProphetToolON.Checked += Ts_LocateToolON_Checked;
			chk_AIProphetToolON.Unchecked += Ts_LocateToolON_Unchecked;
			break;
		case 277:
			lab_TimeProphetTool = (System.Windows.Controls.Label)target;
			break;
		case 278:
			es_AIProphet = (Ellipse)target;
			break;
		case 279:
			lab_ErrorAIProphet = (System.Windows.Controls.Label)target;
			break;
		case 280:
			btn_AddPTrainImage = (System.Windows.Controls.Button)target;
			btn_AddPTrainImage.Click += btn_AddPTrainImage_Click;
			break;
		case 281:
			btn_AddPTrainNGImage = (System.Windows.Controls.Button)target;
			btn_AddPTrainNGImage.Click += btn_AddPTrainNGImage_Click;
			break;
		case 282:
			exp_PreviewAIHeatMap = (Expander)target;
			exp_PreviewAIHeatMap.MouseLeftButtonDown += exp_PreviewAIHeatMap_MouseLeftButtonDown;
			exp_PreviewAIHeatMap.MouseLeftButtonUp += exp_PreviewAIHeatMap_MouseLeftButtonUp;
			exp_PreviewAIHeatMap.MouseMove += exp_PreviewAIHeatMap_MouseMove;
			exp_PreviewAIHeatMap.Expanded += exp_PreviewAIHeatMap_Expanded;
			break;
		case 283:
			tt1 = (TranslateTransform)target;
			break;
		case 284:
			grd_AIHeatMapImage = (Grid)target;
			break;
		case 285:
			cnv_AIHeatMapImage = (Canvas)target;
			break;
		case 286:
			lab_AIProphetScore = (System.Windows.Controls.Label)target;
			break;
		case 287:
			btn_UnLockAuth = (System.Windows.Controls.Button)target;
			btn_UnLockAuth.Click += btn_UnLockAuth_Click;
			break;
		case 288:
			btn_LightMonitorTool = (System.Windows.Controls.Button)target;
			btn_LightMonitorTool.Click += Btn_LightMonitorTool_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
