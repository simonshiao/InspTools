using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.AOI;

public class FontToolWindow : System.Windows.Window, IComponentConnector
{
	private bool inited;

	private bool THD_Preview;

	private int LanguageType = 1;

	private bool UpdateUI;

	private bool ShowMessage = true;

	private bool TrackerShow = true;

	private byte[] CurrentColor = new byte[10];

	private bool[] SearchRectResult = new bool[2];

	private OpenCvSharp.Rect LocateRect;

	private OpenCvSharp.Rect LastSearchRect1;

	private int PatternChannelSelect = 1;

	private INSP_MATCHTOOL_PARAM CopyParam;

	private INSP_MATCHTOOL_PARAM BackupParam;

	private bool CopyParamSeted;

	private bool DontSave;

	private bool ReferenceShow = true;

	private int SelectFace;

	private int FaceIndex;

	private bool AlignMatchTextFound = true;

	private bool LoginSuccess;

	private string CurrentImg = "";

	private MatchPattern[] PublicPatternDB;

	private MatchPattern[] PrivatePatternDB;

	private MatchPattern[] PatternDB;

	private const int MaxPublicPattern = 210;

	private Mat ShowBitmap = new Mat();

	private Mat ShowBitmapFlip = new Mat();

	private Mat CurrentMat = new Mat();

	private int ImageListSource;

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private double CanvasWidth;

	private double CanvasHeight;

	private string[] Imagefilelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int StationIndex;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private Stopwatch sw = new Stopwatch();

	private Button[] ChannelBtn2;

	private string[] ChannelStr;

	private TextBox[] PDBText;

	private Button[] PDBName;

	private Label[] OKIcon;

	private Label[] NGIcon;

	private Label[] AlartIcon;

	private Label[] LabMatchScore;

	private TextBox[] TxtAcceptScore;

	private Image[] PDBImaage;

	private Label[] PDBLabel;

	private Button[] BtnFonts;

	private Button[] BTNQuickAdd;

	private Label[] LabScoreF1;

	private Label[] LabScoreF2;

	private Label[] LabScoreF3;

	private Label[] LabScoreF4;

	private Label[] LabScoreF5;

	private Rectangle[] RTFonts;

	private Grid[] FontGrid;

	private CheckBox[] FontCheckSetup;

	private Canvas[] CnvFontBinary;

	private Grid[] GridFontTrain;

	private ComboBox[] CBAIModeFont;

	private Button[] BTNCreateAIModel;

	private CheckBox[] CHKByPassOCR;

	private CheckBox[] CHK_CheckFont;

	private bool AlignShow;

	private bool PreviewMode;

	private const int MaxPagePattern = 70;

	private Mat[] PDBMat = new Mat[210];

	private bool UsePublicFont = true;

	private bool UsePublicDB = true;

	private string FontPatternBasePath = "";

	private const int MaxFontPattern = 5;

	private const int MaxFonts = 5;

	private int FontPageIndex;

	private bool[] FontsResult = new bool[5];

	private Button[] FontBtnR = new Button[5];

	private Button[] FontBtnM = new Button[5];

	private Button[] FontBtnU = new Button[5];

	private Button[] FontBtnC = new Button[5];

	private Button[] FontBtnDash = new Button[5];

	private Button[] FontBtn0 = new Button[5];

	private Button[] FontBtn1 = new Button[5];

	private Button[] FontBtn2 = new Button[5];

	private Button[] FontBtn3 = new Button[5];

	private Button[] FontBtn4 = new Button[5];

	private Button[] FontBtn5 = new Button[5];

	private Button[] FontBtn6 = new Button[5];

	private Button[] FontBtn7 = new Button[5];

	private Button[] FontBtn8 = new Button[5];

	private Button[] FontBtn9 = new Button[5];

	private Image[] FontImgR = new Image[5];

	private Image[] FontImgM = new Image[5];

	private Image[] FontImgC = new Image[5];

	private Image[] FontImgU = new Image[5];

	private Image[] FontImgX = new Image[5];

	private Image[] FontImgB = new Image[5];

	private Image[] FontImgE = new Image[5];

	private Image[] FontImgD = new Image[5];

	private Image[] FontImgA = new Image[5];

	private Image[] FontImgDash = new Image[5];

	private Image[] FontImg0 = new Image[5];

	private Image[] FontImg1 = new Image[5];

	private Image[] FontImg2 = new Image[5];

	private Image[] FontImg3 = new Image[5];

	private Image[] FontImg4 = new Image[5];

	private Image[] FontImg5 = new Image[5];

	private Image[] FontImg6 = new Image[5];

	private Image[] FontImg7 = new Image[5];

	private Image[] FontImg8 = new Image[5];

	private Image[] FontImg9 = new Image[5];

	private Image[][] FontImgOther = new Image[20][];

	private int P1SelectOld = -1;

	private int P2SelectOld = -1;

	private int P3SelectOld = -1;

	private int P4SelectOld = -1;

	private int P1SelectPublic = -1;

	private int P2SelectPublic = -1;

	private int P3SelectPublic = -1;

	private int P4SelectPublic = -1;

	private bool PasteState;

	private bool QuickSetupWriteState;

	private int CurrentPublicDBPage;

	private User_Permissions User = User_Permissions.USER_Administrator;

	private string[] filelistTest;

	private string BaseTrainFolder = "D:\\AI_FONT_Train\\";

	private int filelistTestIndex = -1;

	private int MaxfilelistTestCount;

	private string FontSetupChar = "R";

	private DispatcherTimer AutoTestTImer = new DispatcherTimer();

	private bool TimerInUsed;

	private int FontTestOKCount;

	private int FontTestNGCount;

	private int FontTestError_AOI;

	private int FontTestError_OCR;

	private int FontTestError_Check;

	private int FontTestError_AI;

	private int FontClick = -1;

	private OpenCvSharp.Rect PTracker = new OpenCvSharp.Rect(0, 0, 0, 0);

	private bool RunSaveOKFinish;

	private string TrainOKPath = "";

	private string TrainNGPath = "";

	private bool ShowExtend1 = true;

	private string TrainDir = "";

	internal FontToolWindow MainW;

	internal Grid grd_Main;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Line line_X1;

	internal Line line_Y1;

	internal Line line_X2;

	internal Line line_Y2;

	internal Border bor_LocateRect;

	internal Border bor_SearchRect1;

	internal TextBlock txt_Search1Label;

	internal Rectangle bor_PatternSet;

	internal Label Label_Result;

	internal Label lab_FaceIndex;

	internal TextBox txt_PreviewMode;

	internal Label lab_FontAlignVertical;

	internal TabControl tab_Function;

	internal TabItem tab_Main;

	internal Image img_Pattern;

	internal ComboBox cb_PatternSelect;

	internal Button btn_AutoSetFont;

	internal Grid grd_Font_1;

	internal ComboBox cb_AIModelFont1;

	internal Label txt_MatchScoreFont1;

	internal TextBox txt_AcceptScoreFont1;

	internal Label ico_ResultOKFont1;

	internal Label ico_ResultAlartFont1;

	internal Label ico_ResultNGFont1;

	internal CheckBox chk_DoubleWidthFont1;

	internal Label lab_AIScoreF11;

	internal Label lab_AIScoreF12;

	internal Label lab_AIScoreF13;

	internal Label lab_AIScoreF14;

	internal Label lab_AIScoreF15;

	internal CheckBox chk_CheckFont1;

	internal Button btn_Font_1;

	internal TextBox txt_WExtendFont1;

	internal TextBox txt_HExtendFont1;

	internal Button btn_ShowExtend1;

	internal CheckBox chk_BypassOCR1;

	internal Button btn_AI_Mode_Reload1;

	internal Button btn_CreateAIMode1F1;

	internal Rectangle rt_Font_1;

	internal TextBox txt_InputText;

	internal Grid grd_Font_2;

	internal ComboBox cb_AIModelFont2;

	internal Label txt_MatchScoreFont2;

	internal TextBox txt_AcceptScoreFont2;

	internal Label ico_ResultAlartFont2;

	internal Label ico_ResultOKFont2;

	internal Label ico_ResultNGFont2;

	internal CheckBox chk_DoubleWidthFont2;

	internal Label lab_AIScoreF21;

	internal Label lab_AIScoreF22;

	internal Label lab_AIScoreF23;

	internal Label lab_AIScoreF24;

	internal Label lab_AIScoreF25;

	internal CheckBox chk_CheckFont2;

	internal Button btn_Font_2;

	internal TextBox txt_WExtendFont2;

	internal TextBox txt_HExtendFont2;

	internal Button btn_ShowExtend2;

	internal CheckBox chk_BypassOCR2;

	internal Button btn_AI_Mode_Reload2;

	internal Button btn_CreateAIMode1F2;

	internal Rectangle rt_Font_2;

	internal Grid grd_Font_3;

	internal ComboBox cb_AIModelFont3;

	internal Label txt_MatchScoreFont3;

	internal TextBox txt_AcceptScoreFont3;

	internal Label ico_ResultAlartFont3;

	internal Label ico_ResultOKFont3;

	internal Label ico_ResultNGFont3;

	internal Label lab_AIScoreF31;

	internal Label lab_AIScoreF32;

	internal Label lab_AIScoreF33;

	internal Label lab_AIScoreF34;

	internal Label lab_AIScoreF35;

	internal CheckBox chk_CheckFont3;

	internal Button btn_Font_3;

	internal TextBox txt_WExtendFont3;

	internal TextBox txt_HExtendFont3;

	internal Button btn_ShowExtend3;

	internal CheckBox chk_BypassOCR3;

	internal Button btn_AI_Mode_Reload3;

	internal Button btn_CreateAIMode1F3;

	internal Rectangle rt_Font_3;

	internal Grid grd_Font_4;

	internal ComboBox cb_AIModelFont4;

	internal Label txt_MatchScoreFont4;

	internal TextBox txt_AcceptScoreFont4;

	internal Label ico_ResultAlartFont4;

	internal Label ico_ResultOKFont4;

	internal Label ico_ResultNGFont4;

	internal Label lab_AIScoreF41;

	internal Label lab_AIScoreF42;

	internal Label lab_AIScoreF43;

	internal Label lab_AIScoreF44;

	internal Label lab_AIScoreF45;

	internal CheckBox chk_CheckFont4;

	internal Button btn_Font_4;

	internal TextBox txt_WExtendFont4;

	internal TextBox txt_HExtendFont4;

	internal Button btn_ShowExtend4;

	internal CheckBox chk_BypassOCR4;

	internal Button btn_AI_Mode_Reload4;

	internal Button btn_CreateAIMode1F4;

	internal Rectangle rt_Font_4;

	internal Grid grd_Font_5;

	internal ComboBox cb_AIModelFont5;

	internal Label txt_MatchScoreFont5;

	internal TextBox txt_AcceptScoreFont5;

	internal Label ico_ResultAlartFont5;

	internal Label ico_ResultOKFont5;

	internal Label ico_ResultNGFont5;

	internal Label lab_AIScoreF51;

	internal Label lab_AIScoreF52;

	internal Label lab_AIScoreF53;

	internal Label lab_AIScoreF54;

	internal Label lab_AIScoreF55;

	internal CheckBox chk_CheckFont5;

	internal Button btn_Font_5;

	internal TextBox txt_WExtendFont5;

	internal TextBox txt_HExtendFont5;

	internal Button btn_ShowExtend5;

	internal CheckBox chk_BypassOCR5;

	internal Button btn_AI_Mode_Reload5;

	internal Button btn_CreateAIMode1F5;

	internal Rectangle rt_Font_5;

	internal Rectangle rt_Result;

	internal ComboBox cb_ErrorBin;

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal Button btn_Test;

	internal Label lab_Time;

	internal CheckBox chk_ShowSearch;

	internal CheckBox chk_ShowMatch;

	internal CheckBox chk_ShowMatchAll;

	internal Grid grd_EngTest;

	internal Button btn_Test0;

	internal CheckBox chk_CheckOK;

	internal TextBox txt_font;

	internal CheckBox chk_FontAlignVertical;

	internal CheckBox chk_EnabledOCR;

	internal CheckBox chk_AutoLoadAI;

	internal CheckBox chk_FontConnected;

	internal Canvas cnv_Font1;

	internal Canvas cnv_Font2;

	internal Canvas cnv_Font3;

	internal Canvas cnv_Font4;

	internal Canvas cnv_Font5;

	internal Button btn_FontConnectSetup;

	internal Label lab_VerAlign;

	internal Grid grd_TrainF1;

	internal Button btn_Add2TrainAF1;

	internal Button btn_Add2TrainBF1;

	internal Button btn_Add2TrainDF1;

	internal Button btn_Add2TrainEF1;

	internal Button btn_UploadModelF1;

	internal Grid grd_TrainF2;

	internal Button btn_Add2TrainAF2;

	internal Button btn_Add2TrainBF2;

	internal Button btn_Add2TrainDF2;

	internal Button btn_Add2TrainEF2;

	internal Button btn_UploadModelF2;

	internal Grid grd_TrainF3;

	internal Button btn_Add2TrainAF3;

	internal Button btn_Add2TrainBF3;

	internal Button btn_Add2TrainDF3;

	internal Button btn_Add2TrainEF3;

	internal Button btn_UploadModelF3;

	internal Grid grd_TrainF4;

	internal Button btn_Add2TrainAF4;

	internal Button btn_Add2TrainBF4;

	internal Button btn_Add2TrainDF4;

	internal Button btn_Add2TrainEF4;

	internal Button btn_UploadModelF4;

	internal Grid grd_TrainF5;

	internal Button btn_Add2TrainAF5;

	internal Button btn_Add2TrainBF5;

	internal Button btn_Add2TrainDF5;

	internal Button btn_Add2TrainEF5;

	internal Button btn_UploadModelF5;

	internal TabItem tab_PublicDB;

	internal Grid grd_PublicDB;

	internal GroupBox gb_BasicSetup_Copy1;

	internal Grid grd_SIMP_THD_3;

	internal Button btn_ChannelR1;

	internal Button btn_ChannelG1;

	internal Button btn_ChannelB1;

	internal Button btn_ChannelH1;

	internal Button btn_ChannelS1;

	internal Button btn_ChannelV1;

	internal Button btn_ChannelGray1;

	internal Button btn_ChannelC11;

	internal Button btn_ChannelC21;

	internal Button btn_ChannelC31;

	internal Button btn_ChannelRemove1;

	internal Button btn_PublicPagePrev;

	internal Label lab_PublicDBPage;

	internal Button btn_PublicPageNext;

	internal RadioButton rad_PDByRecipe;

	internal RadioButton rad_PDByPublic;

	internal Grid grd_PPD1;

	internal Grid gd_pdb001;

	internal TextBox txt_pbd001;

	internal Image img_pbd001;

	internal Label lab_pbd001;

	internal Button btn_pbd001;

	internal Grid gd_pdb002;

	internal TextBox txt_pbd002;

	internal Label lab_pbd002;

	internal Button btn_pbd002;

	internal Image img_pbd002;

	internal Grid gd_pdb003;

	internal TextBox txt_pbd003;

	internal Label lab_pbd003;

	internal Button btn_pbd003;

	internal Image img_pbd003;

	internal Grid gd_pdb004;

	internal TextBox txt_pbd004;

	internal Label lab_pbd004;

	internal Button btn_pbd004;

	internal Image img_pbd004;

	internal Grid gd_pdb005;

	internal TextBox txt_pbd005;

	internal Label lab_pbd005;

	internal Button btn_pbd005;

	internal Image img_pbd005;

	internal Grid gd_pdb006;

	internal TextBox txt_pbd006;

	internal Label lab_pbd006;

	internal Button btn_pbd006;

	internal Image img_pbd006;

	internal Grid gd_pdb007;

	internal TextBox txt_pbd007;

	internal Label lab_pbd007;

	internal Button btn_pbd007;

	internal Image img_pbd007;

	internal Grid gd_pdb008;

	internal TextBox txt_pbd008;

	internal Label lab_pbd008;

	internal Button btn_pbd008;

	internal Image img_pbd008;

	internal Grid gd_pdb009;

	internal TextBox txt_pbd009;

	internal Label lab_pbd009;

	internal Button btn_pbd009;

	internal Image img_pbd009;

	internal Grid gd_pdb010;

	internal TextBox txt_pbd010;

	internal Label lab_pbd010;

	internal Button btn_pbd010;

	internal Image img_pbd010;

	internal Grid grd_PPD2;

	internal Grid gd_pdb011;

	internal TextBox txt_pbd011;

	internal Image img_pbd011;

	internal Label lab_pbd011;

	internal Button btn_pbd011;

	internal Grid gd_pdb012;

	internal TextBox txt_pbd012;

	internal Label lab_pbd012;

	internal Button btn_pbd012;

	internal Image img_pbd012;

	internal Grid gd_pdb013;

	internal TextBox txt_pbd013;

	internal Label lab_pbd013;

	internal Button btn_pbd013;

	internal Image img_pbd013;

	internal Grid gd_pdb014;

	internal TextBox txt_pbd014;

	internal Label lab_pbd014;

	internal Button btn_pbd014;

	internal Image img_pbd014;

	internal Grid gd_pdb015;

	internal TextBox txt_pbd015;

	internal Label lab_pbd015;

	internal Button btn_pbd015;

	internal Image img_pbd015;

	internal Grid gd_pdb016;

	internal TextBox txt_pbd016;

	internal Label lab_pbd016;

	internal Button btn_pbd016;

	internal Image img_pbd016;

	internal Grid gd_pdb017;

	internal TextBox txt_pbd017;

	internal Label lab_pbd017;

	internal Button btn_pbd017;

	internal Image img_pbd017;

	internal Grid gd_pdb018;

	internal TextBox txt_pbd018;

	internal Label lab_pbd018;

	internal Button btn_pbd018;

	internal Image img_pbd018;

	internal Grid gd_pdb019;

	internal TextBox txt_pbd019;

	internal Label lab_pbd019;

	internal Button btn_pbd019;

	internal Image img_pbd019;

	internal Grid gd_pdb020;

	internal TextBox txt_pbd020;

	internal Label lab_pbd020;

	internal Button btn_pbd020;

	internal Image img_pbd020;

	internal Grid grd_PPD3;

	internal Grid gd_pdb021;

	internal TextBox txt_pbd021;

	internal Image img_pbd021;

	internal Label lab_pbd021;

	internal Button btn_pbd021;

	internal Grid gd_pdb022;

	internal TextBox txt_pbd022;

	internal Label lab_pbd022;

	internal Button btn_pbd022;

	internal Image img_pbd022;

	internal Grid gd_pdb023;

	internal TextBox txt_pbd023;

	internal Label lab_pbd023;

	internal Button btn_pbd023;

	internal Image img_pbd023;

	internal Grid gd_pdb024;

	internal TextBox txt_pbd024;

	internal Label lab_pbd024;

	internal Button btn_pbd024;

	internal Image img_pbd024;

	internal Grid gd_pdb025;

	internal TextBox txt_pbd025;

	internal Label lab_pbd025;

	internal Button btn_pbd025;

	internal Image img_pbd025;

	internal Grid gd_pdb026;

	internal TextBox txt_pbd026;

	internal Label lab_pbd026;

	internal Button btn_pbd026;

	internal Image img_pbd026;

	internal Grid gd_pdb027;

	internal TextBox txt_pbd027;

	internal Label lab_pbd027;

	internal Button btn_pbd027;

	internal Image img_pbd027;

	internal Grid gd_pdb028;

	internal TextBox txt_pbd028;

	internal Label lab_pbd028;

	internal Button btn_pbd028;

	internal Image img_pbd028;

	internal Grid gd_pdb029;

	internal TextBox txt_pbd029;

	internal Label lab_pbd029;

	internal Button btn_pbd029;

	internal Image img_pbd029;

	internal Grid gd_pdb030;

	internal TextBox txt_pbd030;

	internal Label lab_pbd030;

	internal Button btn_pbd030;

	internal Image img_pbd030;

	internal Grid grd_PPD4;

	internal Grid gd_pdb031;

	internal TextBox txt_pbd031;

	internal Image img_pbd031;

	internal Label lab_pbd031;

	internal Button btn_pbd031;

	internal Grid gd_pdb032;

	internal TextBox txt_pbd032;

	internal Label lab_pbd032;

	internal Button btn_pbd032;

	internal Image img_pbd032;

	internal Grid gd_pdb033;

	internal TextBox txt_pbd033;

	internal Label lab_pbd033;

	internal Button btn_pbd033;

	internal Image img_pbd033;

	internal Grid gd_pdb034;

	internal TextBox txt_pbd034;

	internal Label lab_pbd034;

	internal Button btn_pbd034;

	internal Image img_pbd034;

	internal Grid gd_pdb035;

	internal TextBox txt_pbd035;

	internal Label lab_pbd035;

	internal Button btn_pbd035;

	internal Image img_pbd035;

	internal Grid gd_pdb036;

	internal TextBox txt_pbd036;

	internal Label lab_pbd036;

	internal Button btn_pbd036;

	internal Image img_pbd036;

	internal Grid gd_pdb037;

	internal TextBox txt_pbd037;

	internal Label lab_pbd037;

	internal Button btn_pbd037;

	internal Image img_pbd037;

	internal Grid gd_pdb038;

	internal TextBox txt_pbd038;

	internal Label lab_pbd038;

	internal Button btn_pbd038;

	internal Image img_pbd038;

	internal Grid gd_pdb039;

	internal TextBox txt_pbd039;

	internal Label lab_pbd039;

	internal Button btn_pbd039;

	internal Image img_pbd039;

	internal Grid gd_pdb040;

	internal TextBox txt_pbd040;

	internal Label lab_pbd040;

	internal Button btn_pbd040;

	internal Image img_pbd040;

	internal Grid grd_PPD5;

	internal Grid gd_pdb041;

	internal TextBox txt_pbd041;

	internal Image img_pbd041;

	internal Label lab_pbd041;

	internal Button btn_pbd041;

	internal Grid gd_pdb042;

	internal TextBox txt_pbd042;

	internal Label lab_pbd042;

	internal Button btn_pbd042;

	internal Image img_pbd042;

	internal Grid gd_pdb043;

	internal TextBox txt_pbd043;

	internal Label lab_pbd043;

	internal Button btn_pbd043;

	internal Image img_pbd043;

	internal Grid gd_pdb044;

	internal TextBox txt_pbd044;

	internal Label lab_pbd044;

	internal Button btn_pbd044;

	internal Image img_pbd044;

	internal Grid gd_pdb045;

	internal TextBox txt_pbd045;

	internal Label lab_pbd045;

	internal Button btn_pbd045;

	internal Image img_pbd045;

	internal Grid gd_pdb046;

	internal TextBox txt_pbd046;

	internal Label lab_pbd046;

	internal Button btn_pbd046;

	internal Image img_pbd046;

	internal Grid gd_pdb047;

	internal TextBox txt_pbd047;

	internal Label lab_pbd047;

	internal Button btn_pbd047;

	internal Image img_pbd047;

	internal Grid gd_pdb048;

	internal TextBox txt_pbd048;

	internal Label lab_pbd048;

	internal Button btn_pbd048;

	internal Image img_pbd048;

	internal Grid gd_pdb049;

	internal TextBox txt_pbd049;

	internal Label lab_pbd049;

	internal Button btn_pbd049;

	internal Image img_pbd049;

	internal Grid gd_pdb050;

	internal TextBox txt_pbd050;

	internal Label lab_pbd050;

	internal Button btn_pbd050;

	internal Image img_pbd050;

	internal Grid grd_PPD6;

	internal Grid gd_pdb051;

	internal TextBox txt_pbd051;

	internal Image img_pbd051;

	internal Label lab_pbd051;

	internal Button btn_pbd051;

	internal Grid gd_pdb052;

	internal TextBox txt_pbd052;

	internal Label lab_pbd052;

	internal Button btn_pbd052;

	internal Image img_pbd052;

	internal Grid gd_pdb053;

	internal TextBox txt_pbd053;

	internal Label lab_pbd053;

	internal Button btn_pbd053;

	internal Image img_pbd053;

	internal Grid gd_pdb054;

	internal TextBox txt_pbd054;

	internal Label lab_pbd054;

	internal Button btn_pbd054;

	internal Image img_pbd054;

	internal Grid gd_pdb055;

	internal TextBox txt_pbd055;

	internal Label lab_pbd055;

	internal Button btn_pbd055;

	internal Image img_pbd055;

	internal Grid gd_pdb056;

	internal TextBox txt_pbd056;

	internal Label lab_pbd056;

	internal Button btn_pbd056;

	internal Image img_pbd056;

	internal Grid gd_pdb057;

	internal TextBox txt_pbd057;

	internal Label lab_pbd057;

	internal Button btn_pbd057;

	internal Image img_pbd057;

	internal Grid gd_pdb058;

	internal TextBox txt_pbd058;

	internal Label lab_pbd058;

	internal Button btn_pbd058;

	internal Image img_pbd058;

	internal Grid gd_pdb059;

	internal TextBox txt_pbd059;

	internal Label lab_pbd059;

	internal Button btn_pbd059;

	internal Image img_pbd059;

	internal Grid gd_pdb060;

	internal TextBox txt_pbd060;

	internal Label lab_pbd060;

	internal Button btn_pbd060;

	internal Image img_pbd060;

	internal Grid grd_PPD7;

	internal Grid gd_pdb061;

	internal TextBox txt_pbd061;

	internal Image img_pbd061;

	internal Label lab_pbd061;

	internal Button btn_pbd061;

	internal Grid gd_pdb062;

	internal TextBox txt_pbd062;

	internal Label lab_pbd062;

	internal Button btn_pbd062;

	internal Image img_pbd062;

	internal Grid gd_pdb063;

	internal TextBox txt_pbd063;

	internal Label lab_pbd063;

	internal Button btn_pbd063;

	internal Image img_pbd063;

	internal Grid gd_pdb064;

	internal TextBox txt_pbd064;

	internal Label lab_pbd064;

	internal Button btn_pbd064;

	internal Image img_pbd064;

	internal Grid gd_pdb065;

	internal TextBox txt_pbd065;

	internal Label lab_pbd065;

	internal Button btn_pbd065;

	internal Image img_pbd065;

	internal Grid gd_pdb066;

	internal TextBox txt_pbd066;

	internal Label lab_pbd066;

	internal Button btn_pbd066;

	internal Image img_pbd066;

	internal Grid gd_pdb067;

	internal TextBox txt_pbd067;

	internal Label lab_pbd067;

	internal Button btn_pbd067;

	internal Image img_pbd067;

	internal Grid gd_pdb068;

	internal TextBox txt_pbd068;

	internal Label lab_pbd068;

	internal Button btn_pbd068;

	internal Image img_pbd068;

	internal Grid gd_pdb069;

	internal TextBox txt_pbd069;

	internal Label lab_pbd069;

	internal Button btn_pbd069;

	internal Image img_pbd069;

	internal Grid gd_pdb070;

	internal TextBox txt_pbd070;

	internal Label lab_pbd070;

	internal Button btn_pbd070;

	internal Image img_pbd070;

	internal TabItem tab_FontDB;

	internal Grid grd_R;

	internal Image img_Font_R1;

	internal Button btn_Font_R1;

	internal Image img_Font_R2;

	internal Button btn_Font_R2;

	internal Image img_Font_R3;

	internal Button btn_Font_R3;

	internal Image img_Font_R4;

	internal Button btn_Font_R4;

	internal Image img_Font_R5;

	internal Button btn_Font_R5;

	internal Button btn_R_Setup;

	internal Grid grd_M;

	internal Image img_Font_M1;

	internal Button btn_Font_M1;

	internal Image img_Font_M2;

	internal Button btn_Font_M2;

	internal Image img_Font_M3;

	internal Button btn_Font_M3;

	internal Image img_Font_M4;

	internal Button btn_Font_M4;

	internal Image img_Font_M5;

	internal Button btn_Font_M5;

	internal Button btn_M_Setup;

	internal Grid grd_C;

	internal Image img_Font_C1;

	internal Button btn_Font_C1;

	internal Image img_Font_C2;

	internal Button btn_Font_C2;

	internal Image img_Font_C3;

	internal Button btn_Font_C3;

	internal Image img_Font_C4;

	internal Button btn_Font_C4;

	internal Image img_Font_C5;

	internal Button btn_Font_C5;

	internal Button btn_C_Setup;

	internal Grid grd_Dash;

	internal Image img_Font_Dash1;

	internal Button btn_Font_Dash1;

	internal Image img_Font_Dash2;

	internal Button btn_Font_Dash2;

	internal Image img_Font_Dash3;

	internal Button btn_Font_Dash3;

	internal Image img_Font_Dash4;

	internal Button btn_Font_Dash4;

	internal Image img_Font_Dash5;

	internal Button btn_Font_Dash5;

	internal Button btn_Dash_Setup;

	internal Grid grd_0;

	internal Image img_Font_01;

	internal Button btn_Font_01;

	internal Image img_Font_02;

	internal Button btn_Font_02;

	internal Image img_Font_03;

	internal Button btn_Font_03;

	internal Image img_Font_04;

	internal Button btn_Font_04;

	internal Image img_Font_05;

	internal Button btn_Font_05;

	internal Button btn_0_Setup;

	internal Grid grd_1;

	internal Image img_Font_11;

	internal Button btn_Font_11;

	internal Image img_Font_12;

	internal Button btn_Font_12;

	internal Image img_Font_13;

	internal Button btn_Font_13;

	internal Image img_Font_14;

	internal Button btn_Font_14;

	internal Image img_Font_15;

	internal Button btn_Font_15;

	internal Button btn_1_Setup;

	internal Grid grd_2;

	internal Image img_Font_21;

	internal Button btn_Font_21;

	internal Image img_Font_22;

	internal Button btn_Font_22;

	internal Image img_Font_23;

	internal Button btn_Font_23;

	internal Image img_Font_24;

	internal Button btn_Font_24;

	internal Image img_Font_25;

	internal Button btn_Font_25;

	internal Button btn_2_Setup;

	internal Grid grd_3;

	internal Image img_Font_31;

	internal Button btn_Font_31;

	internal Image img_Font_32;

	internal Button btn_Font_32;

	internal Image img_Font_33;

	internal Button btn_Font_33;

	internal Image img_Font_34;

	internal Button btn_Font_34;

	internal Image img_Font_35;

	internal Button btn_Font_35;

	internal Button btn_3_Setup;

	internal Grid grd_4;

	internal Image img_Font_41;

	internal Button btn_Font_41;

	internal Image img_Font_42;

	internal Button btn_Font_42;

	internal Image img_Font_43;

	internal Button btn_Font_43;

	internal Image img_Font_44;

	internal Button btn_Font_44;

	internal Image img_Font_45;

	internal Button btn_Font_45;

	internal Button btn_4_Setup;

	internal Grid grd_5;

	internal Image img_Font_51;

	internal Button btn_Font_51;

	internal Image img_Font_52;

	internal Button btn_Font_52;

	internal Image img_Font_53;

	internal Button btn_Font_53;

	internal Image img_Font_54;

	internal Button btn_Font_54;

	internal Image img_Font_55;

	internal Button btn_Font_55;

	internal Button btn_5_Setup;

	internal Grid grd_6;

	internal Image img_Font_61;

	internal Button btn_Font_61;

	internal Image img_Font_62;

	internal Button btn_Font_62;

	internal Image img_Font_63;

	internal Button btn_Font_63;

	internal Image img_Font_64;

	internal Button btn_Font_64;

	internal Image img_Font_65;

	internal Button btn_Font_65;

	internal Button btn_6_Setup;

	internal Grid grd_7;

	internal Image img_Font_71;

	internal Button btn_Font_71;

	internal Image img_Font_72;

	internal Button btn_Font_72;

	internal Image img_Font_73;

	internal Button btn_Font_73;

	internal Image img_Font_74;

	internal Button btn_Font_74;

	internal Image img_Font_75;

	internal Button btn_Font_75;

	internal Button btn_7_Setup;

	internal Grid grd_8;

	internal Image img_Font_81;

	internal Button btn_Font_81;

	internal Image img_Font_82;

	internal Button btn_Font_82;

	internal Image img_Font_83;

	internal Button btn_Font_83;

	internal Image img_Font_84;

	internal Button btn_Font_84;

	internal Image img_Font_85;

	internal Button btn_Font_85;

	internal Button btn_8_Setup;

	internal Grid grd_9;

	internal Image img_Font_91;

	internal Button btn_Font_91;

	internal Image img_Font_92;

	internal Button btn_Font_92;

	internal Image img_Font_93;

	internal Button btn_Font_93;

	internal Image img_Font_94;

	internal Button btn_Font_94;

	internal Image img_Font_95;

	internal Button btn_Font_95;

	internal Button btn_9_Setup;

	internal Grid grd_U;

	internal Image img_Font_U1;

	internal Button btn_Font_U1;

	internal Image img_Font_U2;

	internal Button btn_Font_U2;

	internal Image img_Font_U3;

	internal Button btn_Font_U3;

	internal Image img_Font_U4;

	internal Button btn_Font_U4;

	internal Image img_Font_U5;

	internal Button btn_Font_U5;

	internal Button btn_U_Setup;

	internal RadioButton rad_FDByRecipe;

	internal RadioButton rad_FDByPublic;

	internal Button btn_FontPagePrev;

	internal Label lab_FontDBPage;

	internal Button btn_FontPageNext;

	internal Button btn_AutoPutFont;

	internal Grid grd_Color;

	internal Button btn_ImgSrc;

	internal Button btn_ImgR;

	internal Button btn_ImgG;

	internal Button btn_ImgB;

	internal Button btn_ImgH;

	internal Button btn_ImgS;

	internal Button btn_ImgV;

	internal Button btn_ImgGray;

	internal Button btn_ImgS_Copy;

	internal Button btn_ImgV_Copy;

	internal Button btn_ImageNum;

	internal Button btn_ImgC1;

	internal Button btn_ImgC2;

	internal Button btn_ImgC3;

	internal Button btn_ImgAI;

	internal Label lab_RValue;

	internal Label lab_GValue;

	internal Label lab_BValue;

	internal Label lab_HValue;

	internal Label lab_SValue;

	internal Label lab_VValue;

	internal Label lab_GrayValue;

	internal Label lab_XValue;

	internal Label lab_YValue;

	internal Label lab_C1Value;

	internal Label lab_C2Value;

	internal Label lab_C3Value;

	internal Label lab_MatchScore;

	internal TextBox txt_AllResult;

	internal Button btn_TrackerShow;

	internal Button btn_TestNextImg;

	internal Button btn_TestPrevImg;

	internal Button btn_ReferenceShow;

	internal Button btn_PreviewSwitch_Copy;

	internal Label Label_Index;

	internal TextBox lab_LockMode;

	internal Grid grd_QuickExportImage;

	internal Button btn_QuickAdd_1;

	internal Button btn_QuickAdd_2;

	internal Button btn_QuickAdd_3;

	internal Button btn_QuickAdd_4;

	internal Button btn_QuickAdd_5;

	internal Button btn_OpenFontFolder;

	internal Label lab_Message;

	internal Button btn_Result_Flip;

	internal Button btn_ShowCheckFontTool;

	internal Grid grd_FontTest;

	internal Grid grd_AI_Test;

	internal Image img_TestFont;

	internal Button btn_Open;

	internal Button btn_prev;

	internal Button btn_next;

	internal Button btn_fonttest;

	internal TextBox txt_FontCount;

	internal Label lab_FontTestResult;

	internal TextBox txt_TestFontResult;

	internal ComboBox cb_FontSelectTest;

	internal Image img_TestFontPattern;

	internal CheckBox chk_OKStop;

	internal CheckBox chk_NGStop;

	internal Label lab_FontTestResultScore;

	internal Label lab_FontOCRResult;

	internal Label lab_FontAIResult;

	internal Button btn_AutoTest;

	internal Label lab_OKCount;

	internal Label lab_NGCount;

	internal Label lab_Yield;

	internal Button btn_SaveTestImage;

	internal Button btn_OpenOKNG;

	internal Button btn_GenCSVFile;

	internal Button btn_Font_6;

	internal Button btn_CloseCheckFontTool;

	internal CheckBox chk_TestFont_AOI;

	internal CheckBox chk_TestFont_OCR;

	internal CheckBox chk_TestFont_CheckFont;

	internal CheckBox chk_TestFont_AI;

	internal Grid grd_AI_Function;

	internal Button btn_SetTrainFolder;

	internal Button btn_Add2TrainA;

	internal Button btn_Add2TrainB;

	internal Button btn_Add2TrainD;

	internal Button btn_Add2TrainE;

	internal Button btn_UploadModel;

	internal Label lab_TrainStr;

	internal Button btn_ImageF1;

	internal Button btn_ImageF2;

	internal Button btn_ImageF3;

	internal Button btn_ImageF4;

	internal Button btn_ImageF5;

	internal Button btn_ShowFontBinary;

	internal Canvas img_FontConnected;

	internal Grid grd_FontSetup;

	internal Label lab_FontSetup;

	internal Label lab_FontSetupMain;

	internal CheckBox chk_C1;

	internal CheckBox chk_C2;

	internal CheckBox chk_C3;

	internal CheckBox chk_C4;

	internal CheckBox chk_C5;

	internal CheckBox chk_C6;

	internal CheckBox chk_C7;

	internal CheckBox chk_C8;

	internal CheckBox chk_C9;

	internal CheckBox chk_C10;

	internal CheckBox chk_C11;

	internal CheckBox chk_C12;

	internal CheckBox chk_C13;

	internal CheckBox chk_C14;

	internal CheckBox chk_C15;

	internal CheckBox chk_C16;

	internal Button btn_SaveFontCheckSetup;

	internal Grid grd_FontBinarySetup;

	internal Slider sl_BinaryShift;

	internal ComboBox cb_OpenClose;

	internal ComboBox cb_OpenCloseCount;

	internal Canvas img_BinaryShift;

	internal Label lab_BinaryShift;

	internal Canvas img_FontBinaryShift;

	internal Canvas img_PatternBinaryShift;

	internal Label lab_FontImage1;

	internal Label lab_FontImage2;

	internal ComboBox cb_MaskSelect;

	internal Label lab_MaskSelect;

	internal CheckBox chk_UseFontDBMask;

	internal ComboBox cb_OpenClose2;

	internal ComboBox cb_OpenCloseCount2;

	internal CheckBox chk_UseMorphFirst;

	internal Button btn_RecoverPara;

	internal TextBox txt_MinMatchScore;

	internal Label lab_MinMatchScore;

	internal Button btn_TrainCSV;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public FontToolWindow(User_Permissions permissions)
	{
		inited = false;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		User = permissions;
		Label_Result.Visibility = Visibility.Hidden;
		CanvasWidth = cnv_Image.Width;
		CanvasHeight = cnv_Image.Height;
		for (int i = 0; i < 210; i++)
		{
			PDBMat[i] = new Mat();
		}
		ChannelBtn2 = new Button[11]
		{
			btn_ChannelR1, btn_ChannelG1, btn_ChannelB1, btn_ChannelH1, btn_ChannelS1, btn_ChannelV1, btn_ChannelGray1, btn_ChannelC11, btn_ChannelC21, btn_ChannelC31,
			btn_ChannelRemove1
		};
		PDBName = new Button[70]
		{
			btn_pbd001, btn_pbd002, btn_pbd003, btn_pbd004, btn_pbd005, btn_pbd006, btn_pbd007, btn_pbd008, btn_pbd009, btn_pbd010,
			btn_pbd011, btn_pbd012, btn_pbd013, btn_pbd014, btn_pbd015, btn_pbd016, btn_pbd017, btn_pbd018, btn_pbd019, btn_pbd020,
			btn_pbd021, btn_pbd022, btn_pbd023, btn_pbd024, btn_pbd025, btn_pbd026, btn_pbd027, btn_pbd028, btn_pbd029, btn_pbd030,
			btn_pbd031, btn_pbd032, btn_pbd033, btn_pbd034, btn_pbd035, btn_pbd036, btn_pbd037, btn_pbd038, btn_pbd039, btn_pbd040,
			btn_pbd041, btn_pbd042, btn_pbd043, btn_pbd044, btn_pbd045, btn_pbd046, btn_pbd047, btn_pbd048, btn_pbd049, btn_pbd050,
			btn_pbd051, btn_pbd052, btn_pbd053, btn_pbd054, btn_pbd055, btn_pbd056, btn_pbd057, btn_pbd058, btn_pbd059, btn_pbd060,
			btn_pbd061, btn_pbd062, btn_pbd063, btn_pbd064, btn_pbd065, btn_pbd066, btn_pbd067, btn_pbd068, btn_pbd069, btn_pbd070
		};
		PDBText = new TextBox[70]
		{
			txt_pbd001, txt_pbd002, txt_pbd003, txt_pbd004, txt_pbd005, txt_pbd006, txt_pbd007, txt_pbd008, txt_pbd009, txt_pbd010,
			txt_pbd011, txt_pbd012, txt_pbd013, txt_pbd014, txt_pbd015, txt_pbd016, txt_pbd017, txt_pbd018, txt_pbd019, txt_pbd020,
			txt_pbd021, txt_pbd022, txt_pbd023, txt_pbd024, txt_pbd025, txt_pbd026, txt_pbd027, txt_pbd028, txt_pbd029, txt_pbd030,
			txt_pbd031, txt_pbd032, txt_pbd033, txt_pbd034, txt_pbd035, txt_pbd036, txt_pbd037, txt_pbd038, txt_pbd039, txt_pbd040,
			txt_pbd041, txt_pbd042, txt_pbd043, txt_pbd044, txt_pbd045, txt_pbd046, txt_pbd047, txt_pbd048, txt_pbd049, txt_pbd050,
			txt_pbd051, txt_pbd052, txt_pbd053, txt_pbd054, txt_pbd055, txt_pbd056, txt_pbd057, txt_pbd058, txt_pbd059, txt_pbd060,
			txt_pbd061, txt_pbd062, txt_pbd063, txt_pbd064, txt_pbd065, txt_pbd066, txt_pbd067, txt_pbd068, txt_pbd069, txt_pbd070
		};
		PDBImaage = new Image[70]
		{
			img_pbd001, img_pbd002, img_pbd003, img_pbd004, img_pbd005, img_pbd006, img_pbd007, img_pbd008, img_pbd009, img_pbd010,
			img_pbd011, img_pbd012, img_pbd013, img_pbd014, img_pbd015, img_pbd016, img_pbd017, img_pbd018, img_pbd019, img_pbd020,
			img_pbd021, img_pbd022, img_pbd023, img_pbd024, img_pbd025, img_pbd026, img_pbd027, img_pbd028, img_pbd029, img_pbd030,
			img_pbd031, img_pbd032, img_pbd033, img_pbd034, img_pbd035, img_pbd036, img_pbd037, img_pbd038, img_pbd039, img_pbd040,
			img_pbd041, img_pbd042, img_pbd043, img_pbd044, img_pbd045, img_pbd046, img_pbd047, img_pbd048, img_pbd049, img_pbd050,
			img_pbd051, img_pbd052, img_pbd053, img_pbd054, img_pbd055, img_pbd056, img_pbd057, img_pbd058, img_pbd059, img_pbd060,
			img_pbd061, img_pbd062, img_pbd063, img_pbd064, img_pbd065, img_pbd066, img_pbd067, img_pbd068, img_pbd069, img_pbd070
		};
		PDBLabel = new Label[70]
		{
			lab_pbd001, lab_pbd002, lab_pbd003, lab_pbd004, lab_pbd005, lab_pbd006, lab_pbd007, lab_pbd008, lab_pbd009, lab_pbd010,
			lab_pbd011, lab_pbd012, lab_pbd013, lab_pbd014, lab_pbd015, lab_pbd016, lab_pbd017, lab_pbd018, lab_pbd019, lab_pbd020,
			lab_pbd021, lab_pbd022, lab_pbd023, lab_pbd024, lab_pbd025, lab_pbd026, lab_pbd027, lab_pbd028, lab_pbd029, lab_pbd030,
			lab_pbd031, lab_pbd032, lab_pbd033, lab_pbd034, lab_pbd035, lab_pbd036, lab_pbd037, lab_pbd038, lab_pbd039, lab_pbd040,
			lab_pbd041, lab_pbd042, lab_pbd043, lab_pbd044, lab_pbd045, lab_pbd046, lab_pbd047, lab_pbd048, lab_pbd049, lab_pbd050,
			lab_pbd051, lab_pbd052, lab_pbd053, lab_pbd054, lab_pbd055, lab_pbd056, lab_pbd057, lab_pbd058, lab_pbd059, lab_pbd060,
			lab_pbd061, lab_pbd062, lab_pbd063, lab_pbd064, lab_pbd065, lab_pbd066, lab_pbd067, lab_pbd068, lab_pbd069, lab_pbd070
		};
		FontBtnR = new Button[5] { btn_Font_R1, btn_Font_R2, btn_Font_R3, btn_Font_R4, btn_Font_R5 };
		FontBtnM = new Button[5] { btn_Font_M1, btn_Font_M2, btn_Font_M3, btn_Font_M4, btn_Font_M5 };
		FontBtnU = new Button[5] { btn_Font_U1, btn_Font_U2, btn_Font_U3, btn_Font_U4, btn_Font_U5 };
		FontBtnC = new Button[5] { btn_Font_C1, btn_Font_C2, btn_Font_C3, btn_Font_C4, btn_Font_C5 };
		FontBtnDash = new Button[5] { btn_Font_Dash1, btn_Font_Dash2, btn_Font_Dash3, btn_Font_Dash4, btn_Font_Dash5 };
		FontBtn0 = new Button[5] { btn_Font_01, btn_Font_02, btn_Font_03, btn_Font_04, btn_Font_05 };
		FontBtn1 = new Button[5] { btn_Font_11, btn_Font_12, btn_Font_13, btn_Font_14, btn_Font_15 };
		FontBtn2 = new Button[5] { btn_Font_21, btn_Font_22, btn_Font_23, btn_Font_24, btn_Font_25 };
		FontBtn3 = new Button[5] { btn_Font_31, btn_Font_32, btn_Font_33, btn_Font_34, btn_Font_35 };
		FontBtn4 = new Button[5] { btn_Font_41, btn_Font_42, btn_Font_43, btn_Font_44, btn_Font_45 };
		FontBtn5 = new Button[5] { btn_Font_51, btn_Font_52, btn_Font_53, btn_Font_54, btn_Font_55 };
		FontBtn6 = new Button[5] { btn_Font_61, btn_Font_62, btn_Font_63, btn_Font_64, btn_Font_65 };
		FontBtn7 = new Button[5] { btn_Font_71, btn_Font_72, btn_Font_73, btn_Font_74, btn_Font_75 };
		FontBtn8 = new Button[5] { btn_Font_81, btn_Font_82, btn_Font_83, btn_Font_84, btn_Font_85 };
		FontImg9 = new Image[5] { img_Font_91, img_Font_92, img_Font_93, img_Font_94, img_Font_95 };
		FontImgR = new Image[5] { img_Font_R1, img_Font_R2, img_Font_R3, img_Font_R4, img_Font_R5 };
		FontImgX = new Image[5] { img_Font_R1, img_Font_R2, img_Font_R3, img_Font_R4, img_Font_R5 };
		FontImgB = new Image[5] { img_Font_M1, img_Font_M2, img_Font_M3, img_Font_M4, img_Font_M5 };
		FontImgE = new Image[5] { img_Font_C1, img_Font_C2, img_Font_C3, img_Font_C4, img_Font_C5 };
		FontImgD = new Image[5] { img_Font_U1, img_Font_U2, img_Font_U3, img_Font_U4, img_Font_U5 };
		FontImgA = new Image[5] { img_Font_Dash1, img_Font_Dash2, img_Font_Dash3, img_Font_Dash4, img_Font_Dash5 };
		FontImgM = new Image[5] { img_Font_M1, img_Font_M2, img_Font_M3, img_Font_M4, img_Font_M5 };
		FontImgC = new Image[5] { img_Font_C1, img_Font_C2, img_Font_C3, img_Font_C4, img_Font_C5 };
		FontImgU = new Image[5] { img_Font_U1, img_Font_U2, img_Font_U3, img_Font_U4, img_Font_U5 };
		FontImgDash = new Image[5] { img_Font_Dash1, img_Font_Dash2, img_Font_Dash3, img_Font_Dash4, img_Font_Dash5 };
		FontImg0 = new Image[5] { img_Font_01, img_Font_02, img_Font_03, img_Font_04, img_Font_05 };
		FontImg1 = new Image[5] { img_Font_11, img_Font_12, img_Font_13, img_Font_14, img_Font_15 };
		FontImg2 = new Image[5] { img_Font_21, img_Font_22, img_Font_23, img_Font_24, img_Font_25 };
		FontImg3 = new Image[5] { img_Font_31, img_Font_32, img_Font_33, img_Font_34, img_Font_35 };
		FontImg4 = new Image[5] { img_Font_41, img_Font_42, img_Font_43, img_Font_44, img_Font_45 };
		FontImg5 = new Image[5] { img_Font_51, img_Font_52, img_Font_53, img_Font_54, img_Font_55 };
		FontImg6 = new Image[5] { img_Font_61, img_Font_62, img_Font_63, img_Font_64, img_Font_65 };
		FontImg7 = new Image[5] { img_Font_71, img_Font_72, img_Font_73, img_Font_74, img_Font_75 };
		FontImg8 = new Image[5] { img_Font_81, img_Font_82, img_Font_83, img_Font_84, img_Font_85 };
		FontImg9 = new Image[5] { img_Font_91, img_Font_92, img_Font_93, img_Font_94, img_Font_95 };
		GridFontTrain = new Grid[5] { grd_TrainF1, grd_TrainF2, grd_TrainF3, grd_TrainF4, grd_TrainF5 };
		CBAIModeFont = new ComboBox[5] { cb_AIModelFont1, cb_AIModelFont2, cb_AIModelFont3, cb_AIModelFont4, cb_AIModelFont5 };
		BTNCreateAIModel = new Button[5] { btn_CreateAIMode1F1, btn_CreateAIMode1F2, btn_CreateAIMode1F3, btn_CreateAIMode1F4, btn_CreateAIMode1F5 };
		CHKByPassOCR = new CheckBox[5] { chk_BypassOCR1, chk_BypassOCR2, chk_BypassOCR3, chk_BypassOCR4, chk_BypassOCR5 };
		for (int j = 0; j < 20; j++)
		{
			FontImgOther[j] = new Image[5];
		}
		FontImgOther[0] = FontImg0;
		FontImgOther[1] = FontImg1;
		FontImgOther[2] = FontImg2;
		FontImgOther[3] = FontImg3;
		FontImgOther[4] = FontImg4;
		FontImgOther[5] = FontImg5;
		FontImgOther[6] = FontImg6;
		FontImgOther[7] = FontImg7;
		FontImgOther[8] = FontImg8;
		FontImgOther[9] = FontImg9;
		FontImgOther[10] = FontImgR;
		FontImgOther[11] = FontImgM;
		FontImgOther[12] = FontImgC;
		FontImgOther[13] = FontImgU;
		FontImgOther[14] = FontImgDash;
		FontImgOther[15] = FontImg0;
		FontImgOther[16] = FontImg1;
		OKIcon = new Label[5] { ico_ResultOKFont1, ico_ResultOKFont2, ico_ResultOKFont3, ico_ResultOKFont4, ico_ResultOKFont5 };
		NGIcon = new Label[5] { ico_ResultNGFont1, ico_ResultNGFont2, ico_ResultNGFont3, ico_ResultNGFont4, ico_ResultNGFont5 };
		AlartIcon = new Label[5] { ico_ResultAlartFont1, ico_ResultAlartFont2, ico_ResultAlartFont3, ico_ResultAlartFont4, ico_ResultAlartFont5 };
		LabMatchScore = new Label[5] { txt_MatchScoreFont1, txt_MatchScoreFont2, txt_MatchScoreFont3, txt_MatchScoreFont4, txt_MatchScoreFont5 };
		TxtAcceptScore = new TextBox[5] { txt_AcceptScoreFont1, txt_AcceptScoreFont2, txt_AcceptScoreFont3, txt_AcceptScoreFont4, txt_AcceptScoreFont5 };
		BtnFonts = new Button[5] { btn_Font_1, btn_Font_2, btn_Font_3, btn_Font_4, btn_Font_5 };
		RTFonts = new Rectangle[5] { rt_Font_1, rt_Font_2, rt_Font_3, rt_Font_4, rt_Font_5 };
		CHK_CheckFont = new CheckBox[5] { chk_CheckFont1, chk_CheckFont2, chk_CheckFont3, chk_CheckFont4, chk_CheckFont5 };
		BTNQuickAdd = new Button[5] { btn_QuickAdd_1, btn_QuickAdd_2, btn_QuickAdd_3, btn_QuickAdd_4, btn_QuickAdd_5 };
		LabScoreF1 = new Label[5] { lab_AIScoreF11, lab_AIScoreF12, lab_AIScoreF13, lab_AIScoreF14, lab_AIScoreF15 };
		LabScoreF2 = new Label[5] { lab_AIScoreF21, lab_AIScoreF22, lab_AIScoreF23, lab_AIScoreF24, lab_AIScoreF25 };
		LabScoreF3 = new Label[5] { lab_AIScoreF31, lab_AIScoreF32, lab_AIScoreF33, lab_AIScoreF34, lab_AIScoreF35 };
		LabScoreF4 = new Label[5] { lab_AIScoreF41, lab_AIScoreF42, lab_AIScoreF43, lab_AIScoreF44, lab_AIScoreF45 };
		LabScoreF5 = new Label[5] { lab_AIScoreF51, lab_AIScoreF52, lab_AIScoreF53, lab_AIScoreF54, lab_AIScoreF55 };
		CnvFontBinary = new Canvas[5] { cnv_Font1, cnv_Font2, cnv_Font3, cnv_Font4, cnv_Font5 };
		FontCheckSetup = new CheckBox[16]
		{
			chk_C1, chk_C2, chk_C3, chk_C4, chk_C5, chk_C6, chk_C7, chk_C8, chk_C9, chk_C10,
			chk_C11, chk_C12, chk_C13, chk_C14, chk_C15, chk_C16
		};
		FontGrid = new Grid[16]
		{
			grd_R, grd_M, grd_C, grd_U, grd_Dash, grd_0, grd_1, grd_2, grd_3, grd_4,
			grd_4, grd_5, grd_6, grd_7, grd_8, grd_9
		};
		ChannelStr = new string[10]
		{
			"R",
			"G",
			"B",
			"H",
			"S",
			"V",
			"Gray",
			"C1",
			"C2",
			"C3"
		};
		Label_Result.Visibility = Visibility.Hidden;
		line_X1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		line_Y1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		txt_PreviewMode.Visibility = Visibility.Hidden;
		for (int k = 0; k < 5; k++)
		{
			OKIcon[k].Visibility = Visibility.Hidden;
			NGIcon[k].Visibility = Visibility.Hidden;
			AlartIcon[k].Visibility = Visibility.Hidden;
		}
		lab_Message.Opacity = 0.0;
		lab_Message.Visibility = Visibility.Hidden;
		btn_Result_Flip.Visibility = Visibility.Hidden;
		tab_Main.Background = Brushes.Yellow;
		grd_EngTest.Visibility = Visibility.Hidden;
		grd_FontTest.Visibility = Visibility.Hidden;
		grd_FontSetup.Visibility = Visibility.Hidden;
		AutoTestTImer.Interval = TimeSpan.FromMilliseconds(3.0);
		AutoTestTImer.Tick += AutoTestTImer_Tick;
		if (!File.Exists(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "OcrAllFonts.pb")))
		{
			MessageBox.Show("程式資料夾下找不到OCR模型檔。(OcrAllFonts.pb)");
			chk_EnabledOCR.IsEnabled = false;
		}
		btn_AI_Mode_Reload1.Visibility = Visibility.Hidden;
		btn_AI_Mode_Reload2.Visibility = Visibility.Hidden;
		btn_AI_Mode_Reload3.Visibility = Visibility.Hidden;
		btn_AI_Mode_Reload4.Visibility = Visibility.Hidden;
		btn_AI_Mode_Reload5.Visibility = Visibility.Hidden;
		lab_VerAlign.Visibility = Visibility.Hidden;
		grd_QuickExportImage.Visibility = Visibility.Hidden;
		lab_FontAlignVertical.Visibility = Visibility.Hidden;
		btn_TrainCSV.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetPublicDB(MatchPattern[] DB, MatchPattern[] PrivateDB)
	{
		UsePublicDB = inspection.paramFile.FontToolParam.PublicDBUsePublicMode;
		UsePublicFont = inspection.paramFile.FontToolParam.FontDBUsePublicMode;
		PublicPatternDB = DB;
		PrivatePatternDB = PrivateDB;
		if (UsePublicDB)
		{
			PatternDB = PublicPatternDB;
		}
		else
		{
			PatternDB = PrivatePatternDB;
		}
		UpdateAIModelComboBox(cb_AIModelFont1);
		UpdateAIModelComboBox(cb_AIModelFont2);
		UpdateAIModelComboBox(cb_AIModelFont3);
		UpdateAIModelComboBox(cb_AIModelFont4);
		UpdateAIModelComboBox(cb_AIModelFont5);
		try
		{
			UpdatePatternDBComboBox();
			UpdatePatternImage();
			UpdateUI = true;
			cb_MaskSelect.Items.Clear();
			cb_MaskSelect.Items.Add("-----");
			for (int i = 0; i < 30; i++)
			{
				if (inspection.paramFile.MaskParam[i].TrackerON)
				{
					cb_MaskSelect.Items.Add(i + 1 + " : " + inspection.paramFile.MaskParam[i].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.MaskParam[i].TrackerName;
				comboBoxItem.IsEnabled = false;
				cb_MaskSelect.Items.Add(comboBoxItem);
			}
			if (!Directory.Exists(BaseTrainFolder))
			{
				Directory.CreateDirectory(BaseTrainFolder);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-SetInspection()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	public void SetInspection(CVInspection station, int stationIndex, int faceIndex = 0)
	{
		inspection = station;
		FaceIndex = faceIndex;
		StationIndex = stationIndex;
		UpdateUI = false;
		tab_Function.SelectedIndex = 0;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateAIModelComboBox(ComboBox box)
	{
		try
		{
			int selectedIndex = box.SelectedIndex;
			string text = "";
			if (selectedIndex > 0)
			{
				text = box.SelectedItem.ToString();
			}
			box.Items.Clear();
			string text2 = "";
			text2 = ((!UsePublicFont) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string[] files = Directory.GetFiles(text2, "*.onnx", SearchOption.TopDirectoryOnly);
			box.Items.Add("不使用");
			for (int i = 1; i <= files.Length; i++)
			{
				string fileName = System.IO.Path.GetFileName(files[i - 1]);
				if (inspection.fontTool.ReadOnnxType(fileName, 5) == 1)
				{
					File.Delete(files[i - 1]);
					continue;
				}
				string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(files[i - 1]);
				box.Items.Add(fileNameWithoutExtension);
			}
			if (selectedIndex > 0 && text != "")
			{
				box.SelectedIndex = GetAIModelIndex(text + ".onnx");
			}
		}
		catch (Exception ex)
		{
			string text3 = "FTW07-FontToolWindow-UpdateAIModelComboBox()";
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text3 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdatePBPatternImage()
	{
		Mat mat = new Mat(250, 400, MatType.CV_8UC1);
		mat.SetTo(255.0);
		int num = CurrentPublicDBPage * 70;
		for (int i = 0; i < 70; i++)
		{
			PDBName[i].Content = num + i + 1;
			PDBLabel[i].Content = "";
			PDBText[i].Text = PatternDB[num + i].PatternName;
			if (!PatternDB[num + i].PatternImage.Empty())
			{
				PDBMat[num + i] = PatternDB[num + i].PatternImage;
				PDBImaage[i].Source = PDBMat[num + i].ToBitmapSource();
				PDBLabel[i].Content = string.Format("{0}, {1} x {2}", ChannelStr[PatternDB[i].PatternChannel], PDBMat[num + i].Width, PDBMat[num + i].Height);
			}
			else
			{
				PDBImaage[i].Source = mat.ToBitmapSource();
				PDBLabel[i].Content = "";
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdatePatternImage()
	{
		Mat mat = new Mat(250, 400, MatType.CV_8UC1);
		mat.SetTo(255.0);
		for (int i = 0; i < 210; i++)
		{
			if (!PatternDB[i].PatternImage.Empty())
			{
				PDBMat[i] = PatternDB[i].PatternImage;
			}
		}
		for (int j = 0; j < 70; j++)
		{
			PDBName[j].Content = j + 1;
			PDBLabel[j].Content = "";
			PDBText[j].Text = PatternDB[j].PatternName;
			if (!PatternDB[j].PatternImage.Empty())
			{
				PDBImaage[j].Source = PDBMat[j].ToBitmapSource();
				PDBLabel[j].Content = string.Format("{0}, {1} x {2}", ChannelStr[PatternDB[j].PatternChannel], PDBMat[j].Width, PDBMat[j].Height);
			}
			else
			{
				PDBImaage[j].Source = mat.ToBitmapSource();
				PDBLabel[j].Content = "";
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateFontPattern()
	{
		try
		{
			Mat mat = new Mat(80, 70, MatType.CV_8UC1);
			mat.SetTo(240.0);
			for (int i = 0; i < 5; i++)
			{
				FontImgR[i].Source = mat.ToBitmapSource();
				FontImgM[i].Source = mat.ToBitmapSource();
				FontImgC[i].Source = mat.ToBitmapSource();
				FontImgU[i].Source = mat.ToBitmapSource();
				FontImgX[i].Source = mat.ToBitmapSource();
				FontImgB[i].Source = mat.ToBitmapSource();
				FontImgD[i].Source = mat.ToBitmapSource();
				FontImgE[i].Source = mat.ToBitmapSource();
				FontImgA[i].Source = mat.ToBitmapSource();
				FontImg0[i].Source = mat.ToBitmapSource();
				FontImg1[i].Source = mat.ToBitmapSource();
				FontImg2[i].Source = mat.ToBitmapSource();
				FontImg3[i].Source = mat.ToBitmapSource();
				FontImg4[i].Source = mat.ToBitmapSource();
				FontImg5[i].Source = mat.ToBitmapSource();
				FontImg6[i].Source = mat.ToBitmapSource();
				FontImg7[i].Source = mat.ToBitmapSource();
				FontImg8[i].Source = mat.ToBitmapSource();
				FontImg9[i].Source = mat.ToBitmapSource();
				FontImgDash[i].Source = mat.ToBitmapSource();
				for (int j = 0; j < 17; j++)
				{
					FontImgOther[j][i].Source = mat.ToBitmapSource();
				}
			}
			if (UsePublicFont)
			{
				FontPatternBasePath = AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\Font\\";
			}
			else
			{
				FontPatternBasePath = string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName);
			}
			if (!Directory.Exists(FontPatternBasePath))
			{
				Directory.CreateDirectory(FontPatternBasePath);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW07-FontTool-UpdateFontPattern()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			_ = text + ex.ToString();
			return;
		}
		try
		{
			for (int k = 0; k < 5; k++)
			{
				if (FontPageIndex == 0 && inspection.fontTool.FontMatR[k].Width > 0)
				{
					FontImgR[k].Source = inspection.fontTool.FontMatR[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMatM[k].Width > 0)
				{
					FontImgM[k].Source = inspection.fontTool.FontMatM[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMatC[k].Width > 0)
				{
					FontImgC[k].Source = inspection.fontTool.FontMatC[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMatU[k].Width > 0)
				{
					FontImgU[k].Source = inspection.fontTool.FontMatU[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMatDash[k].Width > 0)
				{
					FontImgDash[k].Source = inspection.fontTool.FontMatDash[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat0[k].Width > 0)
				{
					FontImg0[k].Source = inspection.fontTool.FontMat0[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat1[k].Width > 0)
				{
					FontImg1[k].Source = inspection.fontTool.FontMat1[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat2[k].Width > 0)
				{
					FontImg2[k].Source = inspection.fontTool.FontMat2[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat3[k].Width > 0)
				{
					FontImg3[k].Source = inspection.fontTool.FontMat3[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat4[k].Width > 0)
				{
					FontImg4[k].Source = inspection.fontTool.FontMat4[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat5[k].Width > 0)
				{
					FontImg5[k].Source = inspection.fontTool.FontMat5[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat6[k].Width > 0)
				{
					FontImg6[k].Source = inspection.fontTool.FontMat6[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat7[k].Width > 0)
				{
					FontImg7[k].Source = inspection.fontTool.FontMat7[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat8[k].Width > 0)
				{
					FontImg8[k].Source = inspection.fontTool.FontMat8[k].ToBitmapSource();
				}
				if (FontPageIndex == 0 && inspection.fontTool.FontMat9[k].Width > 0)
				{
					FontImg9[k].Source = inspection.fontTool.FontMat9[k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatX[k].Width > 0)
				{
					FontImgX[k].Source = inspection.fontTool.FontMatX[k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatB[k].Width > 0)
				{
					FontImgB[k].Source = inspection.fontTool.FontMatB[k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatD[k].Width > 0)
				{
					FontImgD[k].Source = inspection.fontTool.FontMatD[k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatE[k].Width > 0)
				{
					FontImgE[k].Source = inspection.fontTool.FontMatE[k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatA[k].Width > 0)
				{
					FontImgA[k].Source = inspection.fontTool.FontMatA[k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[0][k].Width > 0)
				{
					FontImgOther[0][k].Source = inspection.fontTool.FontMatOther[0][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[1][k].Width > 0)
				{
					FontImgOther[1][k].Source = inspection.fontTool.FontMatOther[1][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[2][k].Width > 0)
				{
					FontImgOther[2][k].Source = inspection.fontTool.FontMatOther[2][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[3][k].Width > 0)
				{
					FontImgOther[3][k].Source = inspection.fontTool.FontMatOther[3][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[4][k].Width > 0)
				{
					FontImgOther[4][k].Source = inspection.fontTool.FontMatOther[4][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[5][k].Width > 0)
				{
					FontImgOther[5][k].Source = inspection.fontTool.FontMatOther[5][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[6][k].Width > 0)
				{
					FontImgOther[6][k].Source = inspection.fontTool.FontMatOther[6][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[7][k].Width > 0)
				{
					FontImgOther[7][k].Source = inspection.fontTool.FontMatOther[7][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[8][k].Width > 0)
				{
					FontImgOther[8][k].Source = inspection.fontTool.FontMatOther[8][k].ToBitmapSource();
				}
				if (FontPageIndex == 1 && inspection.fontTool.FontMatOther[9][k].Width > 0)
				{
					FontImgOther[9][k].Source = inspection.fontTool.FontMatOther[9][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[10][k].Width > 0)
				{
					FontImgOther[10][k].Source = inspection.fontTool.FontMatOther[10][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[11][k].Width > 0)
				{
					FontImgOther[11][k].Source = inspection.fontTool.FontMatOther[11][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[12][k].Width > 0)
				{
					FontImgOther[12][k].Source = inspection.fontTool.FontMatOther[12][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[13][k].Width > 0)
				{
					FontImgOther[13][k].Source = inspection.fontTool.FontMatOther[13][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[14][k].Width > 0)
				{
					FontImgOther[14][k].Source = inspection.fontTool.FontMatOther[14][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[15][k].Width > 0)
				{
					FontImgOther[15][k].Source = inspection.fontTool.FontMatOther[15][k].ToBitmapSource();
				}
				if (FontPageIndex == 2 && inspection.fontTool.FontMatOther[16][k].Width > 0)
				{
					FontImgOther[16][k].Source = inspection.fontTool.FontMatOther[16][k].ToBitmapSource();
				}
			}
		}
		catch (Exception ex2)
		{
			string text2 = "MTW07-FontToolWindow-UpdateFontPattern()";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			_ = text2 + ex2.ToString();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateFontPatternOld()
	{
		try
		{
			for (int i = 0; i < 5; i++)
			{
				inspection.fontTool.FontMatR[i] = new Mat();
				FontImgR[i].Source = null;
				inspection.fontTool.FontMatM[i] = new Mat();
				FontImgM[i].Source = null;
				inspection.fontTool.FontMatC[i] = new Mat();
				FontImgC[i].Source = null;
				inspection.fontTool.FontMatU[i] = new Mat();
				FontImgU[i].Source = null;
				inspection.fontTool.FontMatX[i] = new Mat();
				FontImgX[i].Source = null;
				inspection.fontTool.FontMatB[i] = new Mat();
				FontImgB[i].Source = null;
				inspection.fontTool.FontMatD[i] = new Mat();
				FontImgD[i].Source = null;
				inspection.fontTool.FontMatE[i] = new Mat();
				FontImgE[i].Source = null;
				inspection.fontTool.FontMat0[i] = new Mat();
				FontImgA[i].Source = null;
				inspection.fontTool.FontMatB[i] = new Mat();
				FontImg0[i].Source = null;
				inspection.fontTool.FontMat1[i] = new Mat();
				FontImg1[i].Source = null;
				inspection.fontTool.FontMat2[i] = new Mat();
				FontImg2[i].Source = null;
				inspection.fontTool.FontMat3[i] = new Mat();
				FontImg3[i].Source = null;
				inspection.fontTool.FontMat4[i] = new Mat();
				FontImg4[i].Source = null;
				inspection.fontTool.FontMat5[i] = new Mat();
				FontImg5[i].Source = null;
				inspection.fontTool.FontMat6[i] = new Mat();
				FontImg6[i].Source = null;
				inspection.fontTool.FontMat7[i] = new Mat();
				FontImg7[i].Source = null;
				inspection.fontTool.FontMat8[i] = new Mat();
				FontImg8[i].Source = null;
				inspection.fontTool.FontMat9[i] = new Mat();
				FontImg9[i].Source = null;
				inspection.fontTool.FontMatDash[i] = new Mat();
				FontImgDash[i].Source = null;
				for (int j = 0; j < 17; j++)
				{
					inspection.fontTool.FontMatOther[j][i] = new Mat();
					FontImgOther[j][i].Source = null;
				}
			}
			if (UsePublicFont)
			{
				FontPatternBasePath = AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\Font\\";
			}
			else
			{
				FontPatternBasePath = string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName);
			}
			if (!Directory.Exists(FontPatternBasePath))
			{
				Directory.CreateDirectory(FontPatternBasePath);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW07-FontTool-UpdateFontPattern()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			_ = text + ex.ToString();
			return;
		}
		string[] files = Directory.GetFiles(FontPatternBasePath, "*.bmp", SearchOption.AllDirectories);
		int num = 0;
		string text2 = "R_";
		string text3 = "";
		for (int k = 0; k < files.Length; k++)
		{
			text3 = System.IO.Path.GetFileNameWithoutExtension(files[k]);
			if (text3.Length != 4)
			{
				continue;
			}
			try
			{
				text2 = text3.Substring(0, 2);
				text3.Substring(3, 1);
				num = Convert.ToInt32(text3.Substring(2, 1)) - 1;
				if (text2 == null)
				{
					continue;
				}
				int length = text2.Length;
				if (length != 2)
				{
					continue;
				}
				switch (text2[0])
				{
				case 'R':
					if (text2 == "R_")
					{
						inspection.fontTool.FontMatR[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImgR[num].Source = inspection.fontTool.FontMatR[num].ToBitmapSource();
						}
					}
					break;
				case 'M':
					if (text2 == "M_")
					{
						inspection.fontTool.FontMatM[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImgM[num].Source = inspection.fontTool.FontMatM[num].ToBitmapSource();
						}
					}
					break;
				case 'C':
					if (text2 == "C_")
					{
						inspection.fontTool.FontMatC[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImgC[num].Source = inspection.fontTool.FontMatC[num].ToBitmapSource();
						}
					}
					break;
				case 'U':
					if (text2 == "U_")
					{
						inspection.fontTool.FontMatU[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImgU[num].Source = inspection.fontTool.FontMatU[num].ToBitmapSource();
						}
					}
					break;
				case '-':
					if (text2 == "-_")
					{
						inspection.fontTool.FontMatDash[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImgDash[num].Source = inspection.fontTool.FontMatDash[num].ToBitmapSource();
						}
					}
					break;
				case '0':
					if (text2 == "0_")
					{
						inspection.fontTool.FontMat0[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg0[num].Source = inspection.fontTool.FontMat0[num].ToBitmapSource();
						}
					}
					break;
				case '1':
					if (text2 == "1_")
					{
						inspection.fontTool.FontMat1[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg1[num].Source = inspection.fontTool.FontMat1[num].ToBitmapSource();
						}
					}
					break;
				case '2':
					if (text2 == "2_")
					{
						inspection.fontTool.FontMat2[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg2[num].Source = inspection.fontTool.FontMat2[num].ToBitmapSource();
						}
					}
					break;
				case '3':
					if (text2 == "3_")
					{
						inspection.fontTool.FontMat3[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg3[num].Source = inspection.fontTool.FontMat3[num].ToBitmapSource();
						}
					}
					break;
				case '4':
					if (text2 == "4_")
					{
						inspection.fontTool.FontMat4[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg4[num].Source = inspection.fontTool.FontMat4[num].ToBitmapSource();
						}
					}
					break;
				case '5':
					if (text2 == "5_")
					{
						inspection.fontTool.FontMat5[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg5[num].Source = inspection.fontTool.FontMat5[num].ToBitmapSource();
						}
					}
					break;
				case '6':
					if (text2 == "6_")
					{
						inspection.fontTool.FontMat6[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg6[num].Source = inspection.fontTool.FontMat6[num].ToBitmapSource();
						}
					}
					break;
				case '7':
					if (text2 == "7_")
					{
						inspection.fontTool.FontMat7[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg7[num].Source = inspection.fontTool.FontMat7[num].ToBitmapSource();
						}
					}
					break;
				case '8':
					if (text2 == "8_")
					{
						inspection.fontTool.FontMat8[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg8[num].Source = inspection.fontTool.FontMat8[num].ToBitmapSource();
						}
					}
					break;
				case '9':
					if (text2 == "9_")
					{
						inspection.fontTool.FontMat9[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 0)
						{
							FontImg9[num].Source = inspection.fontTool.FontMat9[num].ToBitmapSource();
						}
					}
					break;
				case 'X':
					if (text2 == "X_")
					{
						inspection.fontTool.FontMatX[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgX[num].Source = inspection.fontTool.FontMatX[num].ToBitmapSource();
						}
					}
					break;
				case 'B':
					if (text2 == "B_")
					{
						inspection.fontTool.FontMatB[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgB[num].Source = inspection.fontTool.FontMatB[num].ToBitmapSource();
						}
					}
					break;
				case 'D':
					if (text2 == "D_")
					{
						inspection.fontTool.FontMatD[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgD[num].Source = inspection.fontTool.FontMatD[num].ToBitmapSource();
						}
					}
					break;
				case 'E':
					if (text2 == "E_")
					{
						inspection.fontTool.FontMatE[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgE[num].Source = inspection.fontTool.FontMatE[num].ToBitmapSource();
						}
					}
					break;
				case 'A':
					if (text2 == "A_")
					{
						inspection.fontTool.FontMatA[num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgA[num].Source = inspection.fontTool.FontMatA[num].ToBitmapSource();
						}
					}
					break;
				case 'F':
					if (text2 == "F_")
					{
						inspection.fontTool.FontMatOther[0][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[0][num].Source = inspection.fontTool.FontMatOther[0][num].ToBitmapSource();
						}
					}
					break;
				case 'G':
					if (text2 == "G_")
					{
						inspection.fontTool.FontMatOther[1][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[1][num].Source = inspection.fontTool.FontMatOther[1][num].ToBitmapSource();
						}
					}
					break;
				case 'H':
					if (text2 == "H_")
					{
						inspection.fontTool.FontMatOther[2][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[2][num].Source = inspection.fontTool.FontMatOther[2][num].ToBitmapSource();
						}
					}
					break;
				case 'I':
					if (text2 == "I_")
					{
						inspection.fontTool.FontMatOther[3][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[3][num].Source = inspection.fontTool.FontMatOther[3][num].ToBitmapSource();
						}
					}
					break;
				case 'J':
					if (text2 == "J_")
					{
						inspection.fontTool.FontMatOther[4][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[4][num].Source = inspection.fontTool.FontMatOther[4][num].ToBitmapSource();
						}
					}
					break;
				case 'K':
					if (text2 == "K_")
					{
						inspection.fontTool.FontMatOther[5][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[5][num].Source = inspection.fontTool.FontMatOther[5][num].ToBitmapSource();
						}
					}
					break;
				case 'L':
					if (text2 == "L_")
					{
						inspection.fontTool.FontMatOther[6][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[6][num].Source = inspection.fontTool.FontMatOther[6][num].ToBitmapSource();
						}
					}
					break;
				case 'N':
					if (text2 == "N_")
					{
						inspection.fontTool.FontMatOther[7][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[7][num].Source = inspection.fontTool.FontMatOther[7][num].ToBitmapSource();
						}
					}
					break;
				case 'O':
					if (text2 == "O_")
					{
						inspection.fontTool.FontMatOther[8][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[8][num].Source = inspection.fontTool.FontMatOther[8][num].ToBitmapSource();
						}
					}
					break;
				case 'P':
					if (text2 == "P_")
					{
						inspection.fontTool.FontMatOther[9][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 1)
						{
							FontImgOther[9][num].Source = inspection.fontTool.FontMatOther[9][num].ToBitmapSource();
						}
					}
					break;
				case 'Q':
					if (text2 == "Q_")
					{
						inspection.fontTool.FontMatOther[10][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[10][num].Source = inspection.fontTool.FontMatOther[10][num].ToBitmapSource();
						}
					}
					break;
				case 'S':
					if (text2 == "S_")
					{
						inspection.fontTool.FontMatOther[11][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[11][num].Source = inspection.fontTool.FontMatOther[11][num].ToBitmapSource();
						}
					}
					break;
				case 'T':
					if (text2 == "T_")
					{
						inspection.fontTool.FontMatOther[12][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[12][num].Source = inspection.fontTool.FontMatOther[12][num].ToBitmapSource();
						}
					}
					break;
				case 'V':
					if (text2 == "V_")
					{
						inspection.fontTool.FontMatOther[13][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[13][num].Source = inspection.fontTool.FontMatOther[13][num].ToBitmapSource();
						}
					}
					break;
				case 'W':
					if (text2 == "W_")
					{
						inspection.fontTool.FontMatOther[14][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[14][num].Source = inspection.fontTool.FontMatOther[14][num].ToBitmapSource();
						}
					}
					break;
				case 'Y':
					if (text2 == "Y_")
					{
						inspection.fontTool.FontMatOther[15][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[15][num].Source = inspection.fontTool.FontMatOther[15][num].ToBitmapSource();
						}
					}
					break;
				case 'Z':
					if (text2 == "Z_")
					{
						inspection.fontTool.FontMatOther[16][num] = CVBuffer.ImRead(files[k], ImreadModes.Grayscale);
						if (FontPageIndex == 2)
						{
							FontImgOther[16][num].Source = inspection.fontTool.FontMatOther[16][num].ToBitmapSource();
						}
					}
					break;
				case '.':
				case '/':
				case ':':
				case ';':
				case '<':
				case '=':
				case '>':
				case '?':
				case '@':
					break;
				}
			}
			catch (Exception ex2)
			{
				string text4 = "MTW07-FontToolWindow-UpdateFontPattern()";
				CVBuffer.SaveLog(text4, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
				_ = text4 + ex2.ToString();
			}
		}
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		bool updateUI = UpdateUI;
		UpdateUI = false;
		ThroughputFile = file;
		cb_ParetoChart.Items.Clear();
		for (int i = 0; i < 30; i++)
		{
			if (ThroughputFile.param.AllParetoItems[i] != "")
			{
				cb_ParetoChart.Items.Add(ThroughputFile.param.AllParetoItems[i]);
			}
		}
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetFaceIndex(int index)
	{
		switch (index)
		{
		case 0:
			lab_FaceIndex.Visibility = Visibility.Hidden;
			break;
		case 1:
			lab_FaceIndex.Content = "A";
			break;
		case 2:
			lab_FaceIndex.Content = "B";
			break;
		case 3:
			lab_FaceIndex.Content = "C";
			break;
		case 4:
			lab_FaceIndex.Content = "D";
			break;
		default:
			lab_FaceIndex.Visibility = Visibility.Hidden;
			break;
		}
	}

	private void SetSelectFace(int index)
	{
		SelectFace = index;
		if (index > 0)
		{
			chk_SelectFace.IsChecked = true;
			grd_SelectFace.Visibility = Visibility.Visible;
		}
		btn_FaceA.Background = Brushes.White;
		btn_FaceB.Background = Brushes.White;
		btn_FaceC.Background = Brushes.White;
		btn_FaceD.Background = Brushes.White;
		switch (index)
		{
		case 0:
			btn_FaceA.Background = Brushes.White;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			break;
		case 1:
			btn_FaceA.Background = Brushes.Yellow;
			break;
		case 2:
			btn_FaceB.Background = Brushes.Yellow;
			break;
		case 3:
			btn_FaceC.Background = Brushes.Yellow;
			break;
		case 4:
			btn_FaceD.Background = Brushes.Yellow;
			break;
		default:
			btn_FaceA.Background = Brushes.White;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
	{
		try
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			ImageWidth = mat.Cols;
			ImageHeight = mat.Rows;
			ImageBand = mat.Channels();
			xRatio = CanvasWidth / (double)ImageWidth;
			yRatio = CanvasHeight / (double)ImageHeight;
			if (xRatio > yRatio)
			{
				xRatio = yRatio;
			}
			else
			{
				yRatio = xRatio;
			}
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)ImageWidth * xRatio);
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)ImageHeight * yRatio);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = false;
			LocateRect = locateRect;
			bor_LocateRect.SetValue(Canvas.LeftProperty, (double)locateRect.X * xRatio);
			bor_LocateRect.SetValue(Canvas.TopProperty, (double)locateRect.Y * yRatio);
			bor_LocateRect.SetValue(FrameworkElement.WidthProperty, (double)locateRect.Width * xRatio);
			bor_LocateRect.SetValue(FrameworkElement.HeightProperty, (double)locateRect.Height * yRatio);
			if (CopyValue)
			{
				CopyValueToUI();
			}
			GetPatternTracker(ref PTracker);
		}
		catch
		{
			string text = "BW01-FontToolWindow-SetBuffer()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImageSource(int imagelistsource, string[] flist, int findex, int recentindex, int okindex, int ngindex)
	{
		try
		{
			ImageListSource = imagelistsource;
			Imagefilelist = flist;
			fileIndex = findex;
			RecentBufferIndex = recentindex;
			OKBufferIndex = okindex;
			NGBufferIndex = ngindex;
			switch (ImageListSource)
			{
			case 0:
				btn_ImageNum.Content = string.Format("1 / 1");
				break;
			case 1:
				if (Imagefilelist != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, Imagefilelist.Length);
				}
				break;
			case 2:
				if (inspection.RecentBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				}
				break;
			case 3:
				if (inspection.OKBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				}
				break;
			case 4:
				if (inspection.NGBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				}
				break;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-FontToolWindow-SetImageSource()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI();
		btn_AutoSetFont.Visibility = Visibility.Hidden;
		btn_Test.Visibility = Visibility.Hidden;
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
		inspection.fontTool.TrainAIFontParamMode = false;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
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

	private void Btn_ImgSrc_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		Mat offlineMat = inspection.buffers.GetOfflineMat(button.Uid);
		THD_Preview = false;
		ImageSource imageSource = inspection.buffers.Mat2ImageSource(offlineMat);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image.Background = imageBrush;
		imageSource = null;
		THD_Preview = false;
	}

	private void SetReferenceRect(Rectangle rt, OpenCvSharp.Rect rect)
	{
		if (rect.Width > 0 || rect.Height > 0)
		{
			rt.Visibility = Visibility.Visible;
			rt.SetValue(Canvas.LeftProperty, (double)rect.X * xRatio);
			rt.SetValue(Canvas.TopProperty, (double)rect.Y * yRatio);
			rt.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xRatio);
			rt.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * yRatio);
		}
	}

	private void SetReferenceLine(Line x, Line y, int xindex, int yindex)
	{
		x.Visibility = Visibility.Visible;
		double x2 = (x.X2 = (double)inspection.paramFile.VSLocateParam[xindex].Value * xRatio);
		x.X1 = x2;
		x.Y1 = 0.0;
		x.Y2 = (double)ImageHeight * yRatio;
		y.Visibility = Visibility.Visible;
		x2 = (y.Y2 = (double)inspection.paramFile.HSLocateParam[yindex].Value * yRatio);
		y.Y1 = x2;
		y.X1 = 0.0;
		y.X2 = (double)ImageWidth * xRatio;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool SetTracker(OpenCvSharp.Rect rect1)
	{
		if (PreviewMode)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		bor_PatternSet.Visibility = Visibility.Hidden;
		rect1 &= rect2;
		bool result = true;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Tracker A value < 0，Please check Aligment and Autofit value, or reset Tracker A! ", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("檢測區域A寬高小於0，請檢查對齊、自動內縮設定或是重設檢測區域A!", "錯誤");
			}
			else
			{
				MessageBox.Show("检测区域A宽高小于0，请检查对齐、自动内缩设定或是重设检测区域A!", "错误");
			}
			result = false;
			bor_SearchRect1.Uid = "1";
			DontSave = true;
			bor_SearchRect1.Visibility = Visibility.Hidden;
		}
		else
		{
			bor_SearchRect1.SetValue(Canvas.LeftProperty, (double)rect1.X * xRatio);
			bor_SearchRect1.SetValue(Canvas.TopProperty, (double)rect1.Y * yRatio);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, (double)rect1.Width * xRatio);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, (double)rect1.Height * yRatio);
			bor_SearchRect1.Uid = "0";
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetTracker(ref OpenCvSharp.Rect rect1)
	{
		if (PreviewMode)
		{
			return false;
		}
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		rect1.X = (int)(Canvas.GetLeft(bor_SearchRect1) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_SearchRect1) / yRatio);
		rect1.Width = (int)(bor_SearchRect1.Width / xRatio);
		rect1.Height = (int)(bor_SearchRect1.Height / yRatio);
		rect1 &= rect2;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Tracker A value over range，Please check Aligment and Autofit value, or reset Tracker A! ", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("檢測區域A超出範圍，請檢查對齊、自動內縮設定或是重設檢測區域A!", "錯誤");
			}
			else
			{
				MessageBox.Show("检测区域A超出范围，请检查对齐、自动内缩设定或是重设检测区域A!", "错误");
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetPatternTracker(ref OpenCvSharp.Rect rect1)
	{
		if (xRatio == 0.0 || yRatio == 0.0)
		{
			return false;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(1, 1, ImageWidth - 1, ImageHeight - 1);
		rect1.X = (int)(Canvas.GetLeft(bor_PatternSet) / xRatio);
		rect1.Y = (int)(Canvas.GetTop(bor_PatternSet) / yRatio);
		rect1.Width = (int)(bor_PatternSet.Width / xRatio);
		rect1.Height = (int)(bor_PatternSet.Height / yRatio);
		rect1 &= rect2;
		if (rect1.Width <= 0 || rect1.Height <= 0)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Pattern Tracker over range，Please check Aligment and Autofit value, or reset Tracker!", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("圖形設定區域超出範圍，請檢查對齊、自動內縮設定或是重設圖形區域!", "錯誤");
			}
			else
			{
				MessageBox.Show("图形设定区域超出范围，请检查对齐、自动内缩设定或是重设图形区域!", "错误");
			}
			return false;
		}
		return true;
	}

	private void Btn_PreviewSwitch_Click(object sender, RoutedEventArgs e)
	{
		SwitchView();
	}

	private void SwitchView()
	{
		if (THD_Preview)
		{
			Mat locatedSrc = inspection.buffers.LocatedSrc;
			ImageSource imageSource = inspection.buffers.Mat2ImageSource(locatedSrc);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			THD_Preview = false;
		}
		else
		{
			Btn_Test_Click(this, null);
			THD_Preview = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetResult(bool result)
	{
		Label_Result.Visibility = Visibility.Visible;
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = Brushes.Lime;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
		}
	}

	private void SetResult(int result, string msg)
	{
		if (result == 2)
		{
			Label_Result.Visibility = Visibility.Hidden;
		}
	}

	private void SetErrorResult(string str)
	{
		Label_Result.Visibility = Visibility.Visible;
		Label_Result.Content = str;
		Label_Result.Background = Brushes.Red;
	}

	private void Cnv_Image_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		System.Windows.Point position = e.GetPosition(this);
		if (xRatio != 0.0 && yRatio != 0.0)
		{
			int num = (int)((position.X - cnv_Image.Margin.Left) / xRatio);
			int num2 = (int)(position.Y / yRatio);
			if (num < ImageWidth && num >= 0 && num2 < ImageHeight && num2 >= 0)
			{
				lab_RValue.Content = (CurrentColor[0] = inspection.buffers.GetLocatedBGR(0).At<byte>(num2, num));
				lab_GValue.Content = (CurrentColor[1] = inspection.buffers.GetLocatedBGR(1).At<byte>(num2, num));
				lab_BValue.Content = (CurrentColor[2] = inspection.buffers.GetLocatedBGR(2).At<byte>(num2, num));
				lab_HValue.Content = (CurrentColor[3] = inspection.buffers.GetLocatedHSV(0).At<byte>(num2, num));
				lab_SValue.Content = (CurrentColor[4] = inspection.buffers.GetLocatedHSV(1).At<byte>(num2, num));
				lab_VValue.Content = (CurrentColor[5] = inspection.buffers.GetLocatedHSV(2).At<byte>(num2, num));
				lab_GrayValue.Content = (CurrentColor[6] = inspection.buffers.LocateGray.At<byte>(num2, num));
				lab_C1Value.Content = (CurrentColor[7] = inspection.buffers.GetLocatedCustomMat(0).At<byte>(num2, num));
				lab_C2Value.Content = (CurrentColor[8] = inspection.buffers.GetLocatedCustomMat(1).At<byte>(num2, num));
				lab_C3Value.Content = (CurrentColor[9] = inspection.buffers.GetLocatedCustomMat(2).At<byte>(num2, num));
				Label label = lab_XValue;
				int num3 = num;
				label.Content = num3.ToString();
				Label label2 = lab_YValue;
				num3 = num2;
				label2.Content = num3.ToString();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TestNextImg_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			tab_Function.SelectedIndex = 0;
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			default:
				return;
			case 0:
				return;
			case 1:
				if (Imagefilelist == null || Imagefilelist.Length == 0)
				{
					return;
				}
				if (fileIndex < 0)
				{
					fileIndex = 0;
				}
				if (fileIndex + 1 < Imagefilelist.Length)
				{
					fileIndex++;
				}
				else
				{
					fileIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, Imagefilelist.Length);
				mat = CVBuffer.ImRead(Imagefilelist[fileIndex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (RecentBufferIndex < 0 || inspection.RecentBufferList.Count <= 0)
				{
					return;
				}
				if (RecentBufferIndex + 1 < inspection.RecentBufferList.Count)
				{
					RecentBufferIndex++;
				}
				else
				{
					RecentBufferIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				inspection.SetBuffers(inspection.RecentBufferList[RecentBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (OKBufferIndex < 0 || inspection.OKBufferList.Count <= 0)
				{
					return;
				}
				if (OKBufferIndex + 1 < inspection.OKBufferList.Count)
				{
					OKBufferIndex++;
				}
				else
				{
					OKBufferIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				inspection.SetBuffers(inspection.OKBufferList[OKBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (NGBufferIndex < 0 || inspection.NGBufferList.Count <= 0)
				{
					return;
				}
				if (NGBufferIndex + 1 < inspection.NGBufferList.Count)
				{
					NGBufferIndex++;
				}
				else
				{
					NGBufferIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat);
			if (!inspection.LocaetResult || !inspection.AdvlocateResult)
			{
				cnv_Image.Background = Brushes.Black;
				THD_Preview = true;
				DontSave = true;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				FaceIndex = inspection.SelectFaceResult;
				DontSave = false;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-FontToolWindow-Btn_TestNextImg_Click()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TestPrevImg_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			tab_Function.SelectedIndex = 0;
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			default:
				return;
			case 0:
				return;
			case 1:
				if (Imagefilelist == null || Imagefilelist.Length == 0)
				{
					return;
				}
				if (fileIndex - 1 > 0)
				{
					fileIndex--;
				}
				else
				{
					fileIndex = Imagefilelist.Length - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, Imagefilelist.Length);
				mat = CVBuffer.ImRead(Imagefilelist[fileIndex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (RecentBufferIndex < 0 || inspection.RecentBufferList.Count <= 0)
				{
					return;
				}
				if (RecentBufferIndex - 1 > 0)
				{
					RecentBufferIndex--;
				}
				else
				{
					RecentBufferIndex = inspection.RecentBufferList.Count - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				inspection.SetBuffers(inspection.RecentBufferList[RecentBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (OKBufferIndex < 0 || inspection.OKBufferList.Count <= 0)
				{
					return;
				}
				if (OKBufferIndex - 1 > 0)
				{
					OKBufferIndex--;
				}
				else
				{
					OKBufferIndex = inspection.OKBufferList.Count - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				inspection.SetBuffers(inspection.OKBufferList[OKBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (NGBufferIndex < 0 || inspection.NGBufferList.Count <= 0)
				{
					return;
				}
				if (NGBufferIndex - 1 > 0)
				{
					NGBufferIndex--;
				}
				else
				{
					NGBufferIndex = inspection.NGBufferList.Count - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat);
			if (!inspection.LocaetResult || !inspection.AdvlocateResult)
			{
				DontSave = true;
				cnv_Image.Background = Brushes.Black;
				THD_Preview = true;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				FaceIndex = inspection.SelectFaceResult;
				DontSave = false;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-FontToolWindow-Btn_TestPrevImg_Click()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public static bool IsNumeric(string TextBoxValue)
	{
		try
		{
			Convert.ToDouble(TextBoxValue);
			return true;
		}
		catch
		{
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckTextBox()
	{
		try
		{
			bool result = true;
			if (!IsNumeric(txt_AcceptScoreFont1.Text))
			{
				txt_AcceptScoreFont1.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_AcceptScoreFont1.Background = Brushes.White;
			}
			if (!IsNumeric(txt_AcceptScoreFont2.Text))
			{
				txt_AcceptScoreFont2.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_AcceptScoreFont2.Background = Brushes.White;
			}
			if (!IsNumeric(txt_AcceptScoreFont3.Text))
			{
				txt_AcceptScoreFont3.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_AcceptScoreFont3.Background = Brushes.White;
			}
			if (!IsNumeric(txt_AcceptScoreFont4.Text))
			{
				txt_AcceptScoreFont4.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_AcceptScoreFont4.Background = Brushes.White;
			}
			if (!IsNumeric(txt_AcceptScoreFont5.Text))
			{
				txt_AcceptScoreFont5.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_AcceptScoreFont5.Background = Brushes.White;
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("FTW08-FontToolWindow-CheckTextBox()-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show("輸入字元不合法，請重新輸入", "錯誤");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Tracker1SizeChange_Click(object sender, RoutedEventArgs e)
	{
		Edit2InputWindow edit2InputWindow = new Edit2InputWindow();
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		new OpenCvSharp.Rect(0, 0, 0, 0);
		GetTracker(ref rect);
		if (((Button)sender).Uid == "1")
		{
			edit2InputWindow.SetMinMaxValue(0, ImageWidth, 0, ImageHeight);
			edit2InputWindow.SetValue(rect.Width, rect.Height);
			edit2InputWindow.ShowDialog();
			if (edit2InputWindow.SetSuccess)
			{
				rect.Width = edit2InputWindow.Value1;
				rect.Height = edit2InputWindow.Value2;
				SetTracker(rect);
			}
		}
	}

	private void Btn_ReferenceShow_Click(object sender, RoutedEventArgs e)
	{
		if (ReferenceShow)
		{
			Line line = line_X1;
			Line line2 = line_X2;
			Line line3 = line_Y1;
			Visibility visibility = (line_Y2.Visibility = Visibility.Hidden);
			Visibility visibility3 = (line3.Visibility = visibility);
			Visibility visibility5 = (line2.Visibility = visibility3);
			line.Visibility = visibility5;
			ReferenceShow = false;
		}
		else
		{
			ReferenceShow = true;
		}
	}

	private void Btn_TrackerShow_Click(object sender, RoutedEventArgs e)
	{
		if (!TrackerShow)
		{
			bor_SearchRect1.Visibility = Visibility.Visible;
			TrackerShow = true;
			bor_LocateRect.Visibility = Visibility.Visible;
		}
		else
		{
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Hidden;
			TrackerShow = false;
			bor_LocateRect.Visibility = Visibility.Hidden;
		}
	}

	private void DrawBackGround()
	{
		if (!PreviewMode)
		{
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				ShowBitmap = inspection.buffers.LocatedSrc.Clone();
			}
			else
			{
				Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void DrawBackGround(int channel)
	{
		if (!PreviewMode)
		{
			ShowBitmap = inspection.buffers.GetChannelMat(channel);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
			THD_Preview = true;
		}
	}

	private void Btn_ChannelR_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		for (int i = 0; i < 11; i++)
		{
			ChannelBtn2[i].Background = Brushes.White;
		}
		PatternChannelSelect = Convert.ToInt32(button.Uid);
		ChannelBtn2[PatternChannelSelect].Background = Brushes.YellowGreen;
		if (PatternChannelSelect == 10)
		{
			cnv_Image.Background = Brushes.Gray;
		}
		else
		{
			DrawBackGround(PatternChannelSelect);
		}
	}

	private void Tab_Function_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !inited || !(e.Source is TabControl))
		{
			return;
		}
		if (ThroughputFile.param.EnableAIPasswordProtect && !LoginSuccess && tab_Function.SelectedIndex != 0)
		{
			LoginSuccess = false;
			PasswordWindow passwordWindow = new PasswordWindow();
			passwordWindow.SetPassWord(ThroughputFile.param.AIPassword);
			passwordWindow.ShowDialog();
			if (!passwordWindow.GetLoginStatus())
			{
				UpdateUI = false;
				tab_Function.SelectedIndex = 0;
				UpdateUI = true;
				return;
			}
			LoginSuccess = true;
		}
		tab_Main.Background = Brushes.White;
		tab_PublicDB.Background = Brushes.White;
		tab_FontDB.Background = Brushes.White;
		if (tab_Function.SelectedIndex == 0)
		{
			tab_Main.Background = Brushes.Yellow;
			bor_SearchRect1.Visibility = Visibility.Visible;
			bor_PatternSet.Visibility = Visibility.Hidden;
			grd_Color.Visibility = Visibility.Visible;
			DrawBackGround();
			int selectedIndex = cb_PatternSelect.SelectedIndex;
			UpdatePatternDBComboBox();
			cb_PatternSelect.SelectedIndex = selectedIndex;
		}
		else if (tab_Function.SelectedIndex == 2)
		{
			WriteUIValueToParam();
			tab_FontDB.Background = Brushes.Yellow;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Visible;
			grd_Color.Visibility = Visibility.Visible;
			DrawBackGround();
		}
		else if (tab_Function.SelectedIndex == 1)
		{
			WriteUIValueToParam();
			tab_PublicDB.Background = Brushes.Yellow;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Visible;
			grd_Color.Visibility = Visibility.Hidden;
			DrawBackGround(PatternChannelSelect);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdatePatternDBComboBox()
	{
		cb_PatternSelect.Items.Clear();
		for (int i = 0; i < 210; i++)
		{
			if (PatternDB[i].PatternON)
			{
				cb_PatternSelect.Items.Add(i + 1 + " : " + PatternDB[i].PatternName);
				continue;
			}
			ComboBoxItem comboBoxItem = new ComboBoxItem();
			comboBoxItem.Content = i + 1 + " : " + PatternDB[i].PatternName;
			comboBoxItem.IsEnabled = false;
			cb_PatternSelect.Items.Add(comboBoxItem);
		}
	}

	private void Chk_SelectFace_Click(object sender, RoutedEventArgs e)
	{
		if (chk_SelectFace.IsChecked == true)
		{
			grd_SelectFace.IsEnabled = true;
			btn_FaceA.Background = Brushes.Yellow;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			SelectFace = 1;
		}
		else
		{
			grd_SelectFace.IsEnabled = false;
			btn_FaceA.Background = Brushes.White;
			btn_FaceB.Background = Brushes.White;
			btn_FaceC.Background = Brushes.White;
			btn_FaceD.Background = Brushes.White;
			SelectFace = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_FaceA_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		btn_FaceA.Background = Brushes.White;
		btn_FaceB.Background = Brushes.White;
		btn_FaceC.Background = Brushes.White;
		btn_FaceD.Background = Brushes.White;
		string uid = obj.Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (!(uid == "3"))
				{
					if (uid == "4")
					{
						btn_FaceD.Background = Brushes.Yellow;
						SelectFace = 4;
					}
				}
				else
				{
					btn_FaceC.Background = Brushes.Yellow;
					SelectFace = 3;
				}
			}
			else
			{
				btn_FaceB.Background = Brushes.Yellow;
				SelectFace = 2;
			}
		}
		else
		{
			btn_FaceA.Background = Brushes.Yellow;
			SelectFace = 1;
		}
	}

	private void Txt_MatchScoreMin_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		TextBox tbox = (TextBox)sender;
		System.Windows.Point position = e.GetPosition(this);
		SetUnsignedINTKeyboard(tbox, position);
	}

	private void SetKeyboard(TextBox tbox, System.Windows.Point p)
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

	private void SetUnsignedINTKeyboard(TextBox tbox, System.Windows.Point p)
	{
		HKeyboard hKeyboard = new HKeyboard();
		hKeyboard.setTxtbox(tbox);
		hKeyboard.SetInput(tbox.Text);
		hKeyboard.SetInputNegative(state: false);
		hKeyboard.SetDouble(state: false);
		hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
		hKeyboard.Left = p.X;
		hKeyboard.Top = p.Y - hKeyboard.Height + MainW.Top;
		tbox.Background = Brushes.Gold;
		hKeyboard.ShowDialog();
		tbox.Background = Brushes.White;
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

	private void Cnv_Image_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		System.Windows.Point position = e.GetPosition(cnv_Image);
		sfr.CenterX = position.X;
		sfr.CenterY = position.Y;
		if (!(sfr.ScaleX < 0.3) || !(sfr.ScaleY < 0.3) || e.Delta >= 0)
		{
			sfr.ScaleX += (double)e.Delta / 500.0;
			sfr.ScaleY += (double)e.Delta / 500.0;
			if (sfr.ScaleX <= 1.0)
			{
				sfr.ScaleX = 1.0;
			}
			if (sfr.ScaleY <= 1.0)
			{
				sfr.ScaleY = 1.0;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		UpdateUI = false;
		DontSave = false;
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		SetSelectFace(fontToolParam.FaceIndex);
		chk_FontAlignVertical.IsChecked = fontToolParam.FontAlignVertical;
		if (chk_EnabledOCR.IsEnabled)
		{
			chk_EnabledOCR.IsChecked = fontToolParam.EnabledOCR;
		}
		else
		{
			chk_EnabledOCR.IsChecked = false;
		}
		chk_FontConnected.IsChecked = fontToolParam.FontConnected;
		chk_AutoLoadAI.IsChecked = fontToolParam.AutoLoadAI;
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		rect.X = inspection.paramFile.AdvLocateParam[0].L + inspection.paramFile.FontToolParam.Track1_L;
		rect.Y = inspection.paramFile.AdvLocateParam[0].T + inspection.paramFile.FontToolParam.Track1_T;
		rect.Width = inspection.paramFile.AdvLocateParam[0].Width - (inspection.paramFile.FontToolParam.Track1_L + inspection.paramFile.FontToolParam.Track1_R);
		rect.Height = inspection.paramFile.AdvLocateParam[0].Height - (inspection.paramFile.FontToolParam.Track1_T + inspection.paramFile.FontToolParam.Track1_B);
		Line line = line_X1;
		Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
		line.Visibility = visibility;
		if (SetTracker(rect))
		{
			GetTracker(ref rect);
			LastSearchRect1 = rect;
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
		}
		cb_PatternSelect.SelectedIndex = fontToolParam.PatternSelect;
		if (fontToolParam.PatternSelect >= 0 && fontToolParam.PatternSelect < 210)
		{
			UpdatePatternSelectImage(fontToolParam.PatternSelect);
		}
		txt_InputText.Text = fontToolParam.AllFont;
		UsePublicFont = fontToolParam.FontDBUsePublicMode;
		UsePublicDB = fontToolParam.PublicDBUsePublicMode;
		cb_AIModelFont1.SelectedIndex = GetAIModelIndex(fontToolParam.AIModel_Font1);
		cb_AIModelFont2.SelectedIndex = GetAIModelIndex(fontToolParam.AIModel_Font2);
		cb_AIModelFont3.SelectedIndex = GetAIModelIndex(fontToolParam.AIModel_Font3);
		cb_AIModelFont4.SelectedIndex = GetAIModelIndex(fontToolParam.AIModel_Font4);
		cb_AIModelFont5.SelectedIndex = GetAIModelIndex(fontToolParam.AIModel_Font5);
		for (int i = 0; i < fontToolParam.AllFont.Length; i++)
		{
			string onnxfile = string.Format("{0}.onnx", fontToolParam.AllFont.Substring(i, 1));
			BTNCreateAIModel[i].Visibility = ((GetAIModelIndex(onnxfile) != 0) ? Visibility.Hidden : Visibility.Visible);
		}
		grd_TrainF1.Visibility = ((cb_AIModelFont1.SelectedIndex == 0) ? Visibility.Hidden : Visibility.Visible);
		grd_TrainF2.Visibility = ((cb_AIModelFont2.SelectedIndex == 0) ? Visibility.Hidden : Visibility.Visible);
		grd_TrainF3.Visibility = ((cb_AIModelFont3.SelectedIndex == 0) ? Visibility.Hidden : Visibility.Visible);
		grd_TrainF4.Visibility = ((cb_AIModelFont4.SelectedIndex == 0) ? Visibility.Hidden : Visibility.Visible);
		grd_TrainF5.Visibility = ((cb_AIModelFont5.SelectedIndex == 0) ? Visibility.Hidden : Visibility.Visible);
		for (int j = fontToolParam.AllFont.Length; j < 5; j++)
		{
			GridFontTrain[j].Visibility = Visibility.Hidden;
		}
		chk_CheckFont1.IsChecked = fontToolParam.EnableFontCheck1;
		chk_CheckFont2.IsChecked = fontToolParam.EnableFontCheck2;
		chk_CheckFont3.IsChecked = fontToolParam.EnableFontCheck3;
		chk_CheckFont4.IsChecked = fontToolParam.EnableFontCheck4;
		chk_CheckFont5.IsChecked = fontToolParam.EnableFontCheck5;
		chk_BypassOCR1.IsChecked = fontToolParam.BypassOCR1;
		chk_BypassOCR2.IsChecked = fontToolParam.BypassOCR2;
		chk_BypassOCR3.IsChecked = fontToolParam.BypassOCR3;
		chk_BypassOCR4.IsChecked = fontToolParam.BypassOCR4;
		chk_BypassOCR5.IsChecked = fontToolParam.BypassOCR5;
		txt_WExtendFont1.Text = fontToolParam.WFontExtend1.ToString();
		txt_WExtendFont2.Text = fontToolParam.WFontExtend2.ToString();
		txt_WExtendFont3.Text = fontToolParam.WFontExtend3.ToString();
		txt_WExtendFont4.Text = fontToolParam.WFontExtend4.ToString();
		txt_WExtendFont5.Text = fontToolParam.WFontExtend5.ToString();
		txt_HExtendFont1.Text = fontToolParam.HFontExtend1.ToString();
		txt_HExtendFont2.Text = fontToolParam.HFontExtend2.ToString();
		txt_HExtendFont3.Text = fontToolParam.HFontExtend3.ToString();
		txt_HExtendFont4.Text = fontToolParam.HFontExtend4.ToString();
		txt_HExtendFont5.Text = fontToolParam.HFontExtend5.ToString();
		chk_DoubleWidthFont1.IsChecked = fontToolParam.DoubleWidthFont1;
		chk_DoubleWidthFont2.IsChecked = fontToolParam.DoubleWidthFont2;
		btn_Font_1.Content = fontToolParam.FontChar1;
		btn_Font_2.Content = fontToolParam.FontChar2;
		btn_Font_3.Content = fontToolParam.FontChar3;
		btn_Font_4.Content = fontToolParam.FontChar4;
		btn_Font_5.Content = fontToolParam.FontChar5;
		btn_Font_1.Uid = fontToolParam.FontChar1;
		btn_Font_2.Uid = fontToolParam.FontChar2;
		btn_Font_3.Uid = fontToolParam.FontChar3;
		btn_Font_4.Uid = fontToolParam.FontChar4;
		btn_Font_5.Uid = fontToolParam.FontChar5;
		if (fontToolParam.FontChar1 == "")
		{
			grd_Font_1.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Font_1.Visibility = Visibility.Visible;
		}
		if (fontToolParam.FontChar2 == "")
		{
			grd_Font_2.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Font_2.Visibility = Visibility.Visible;
		}
		if (fontToolParam.FontChar3 == "")
		{
			grd_Font_3.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Font_3.Visibility = Visibility.Visible;
		}
		if (fontToolParam.FontChar4 == "")
		{
			grd_Font_4.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Font_4.Visibility = Visibility.Visible;
		}
		if (fontToolParam.FontChar5 == "")
		{
			grd_Font_5.Visibility = Visibility.Hidden;
		}
		else
		{
			grd_Font_5.Visibility = Visibility.Visible;
		}
		txt_AcceptScoreFont1.Text = fontToolParam.MinScore_Font1.ToString();
		txt_AcceptScoreFont2.Text = fontToolParam.MinScore_Font2.ToString();
		txt_AcceptScoreFont3.Text = fontToolParam.MinScore_Font3.ToString();
		txt_AcceptScoreFont4.Text = fontToolParam.MinScore_Font4.ToString();
		txt_AcceptScoreFont5.Text = fontToolParam.MinScore_Font5.ToString();
		txt_MatchScoreFont1.Content = "";
		txt_MatchScoreFont2.Content = "";
		txt_MatchScoreFont3.Content = "";
		txt_MatchScoreFont4.Content = "";
		txt_MatchScoreFont5.Content = "";
		cb_ErrorBin.SelectedIndex = (int)fontToolParam.ErrorBin;
		cb_ParetoChart.SelectedIndex = fontToolParam.ParetoIndex;
		if (fontToolParam.Locked && User < User_Permissions.USER_Administrator)
		{
			lab_LockMode.Visibility = Visibility.Visible;
		}
		else
		{
			lab_LockMode.Visibility = Visibility.Hidden;
		}
		if (fontToolParam.FontDBUsePublicMode)
		{
			rad_FDByPublic.IsChecked = true;
		}
		else
		{
			rad_FDByRecipe.IsChecked = true;
		}
		if (fontToolParam.PublicDBUsePublicMode)
		{
			rad_PDByPublic.IsChecked = true;
		}
		else
		{
			rad_PDByRecipe.IsChecked = true;
		}
		for (int k = 0; k <= 5; k++)
		{
			inspection.fontTool.FontModelReaded[k] = false;
			inspection.fontTool.FontModelOnnxType[k] = 0;
		}
		UpdateFontPattern();
		UpdateUI = true;
		try
		{
			if (!fontToolParam.PublicDBUsePublicMode)
			{
				tab_PublicDB.Header = LanguageManager.Instance["PrivateDB"];
			}
			else
			{
				tab_PublicDB.Header = LanguageManager.Instance["PublicDB"];
			}
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-Rad_PDByRecipe_Click()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		if ((User < User_Permissions.USER_Administrator && inspection.paramFile.FontToolParam.Locked) || PreviewMode)
		{
			return;
		}
		try
		{
			INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
			INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile.FontToolParam;
			fontToolParam.FontAlignVertical = chk_FontAlignVertical.IsChecked.Value;
			fontToolParam.EnabledOCR = chk_EnabledOCR.IsChecked.Value;
			fontToolParam.AutoLoadAI = chk_AutoLoadAI.IsChecked.Value;
			fontToolParam.FontConnected = chk_FontConnected.IsChecked.Value;
			fontToolParam.PatternSelect = cb_PatternSelect.SelectedIndex;
			fontToolParam.AllFont = txt_InputText.Text;
			fontToolParam.AIModel_Font1 = GetAIModelName(cb_AIModelFont1.SelectedIndex);
			fontToolParam.AIModel_Font2 = GetAIModelName(cb_AIModelFont2.SelectedIndex);
			fontToolParam.AIModel_Font3 = GetAIModelName(cb_AIModelFont3.SelectedIndex);
			fontToolParam.AIModel_Font4 = GetAIModelName(cb_AIModelFont4.SelectedIndex);
			fontToolParam.AIModel_Font5 = GetAIModelName(cb_AIModelFont5.SelectedIndex);
			fontToolParam.WFontExtend1 = Convert.ToInt32(txt_WExtendFont1.Text);
			fontToolParam.WFontExtend2 = Convert.ToInt32(txt_WExtendFont2.Text);
			fontToolParam.WFontExtend3 = Convert.ToInt32(txt_WExtendFont3.Text);
			fontToolParam.WFontExtend4 = Convert.ToInt32(txt_WExtendFont4.Text);
			fontToolParam.WFontExtend5 = Convert.ToInt32(txt_WExtendFont5.Text);
			fontToolParam.HFontExtend1 = Convert.ToInt32(txt_HExtendFont1.Text);
			fontToolParam.HFontExtend2 = Convert.ToInt32(txt_HExtendFont2.Text);
			fontToolParam.HFontExtend3 = Convert.ToInt32(txt_HExtendFont3.Text);
			fontToolParam.HFontExtend4 = Convert.ToInt32(txt_HExtendFont4.Text);
			fontToolParam.HFontExtend5 = Convert.ToInt32(txt_HExtendFont5.Text);
			if (fontToolParam2.AIModel_Font1 != fontToolParam.AIModel_Font1)
			{
				inspection.fontTool.FontModelReaded[0] = false;
				inspection.fontTool.FontModelOnnxType[0] = 0;
			}
			if (fontToolParam2.AIModel_Font2 != fontToolParam.AIModel_Font2)
			{
				inspection.fontTool.FontModelReaded[1] = false;
				inspection.fontTool.FontModelOnnxType[1] = 0;
			}
			if (fontToolParam2.AIModel_Font3 != fontToolParam.AIModel_Font3)
			{
				inspection.fontTool.FontModelReaded[2] = false;
				inspection.fontTool.FontModelOnnxType[2] = 0;
			}
			if (fontToolParam2.AIModel_Font4 != fontToolParam.AIModel_Font4)
			{
				inspection.fontTool.FontModelReaded[3] = false;
				inspection.fontTool.FontModelOnnxType[3] = 0;
			}
			if (fontToolParam2.AIModel_Font5 != fontToolParam.AIModel_Font5)
			{
				inspection.fontTool.FontModelReaded[4] = false;
				inspection.fontTool.FontModelOnnxType[4] = 0;
			}
			if (fontToolParam.AIModel_Font1 == "")
			{
				fontToolParam.EnableAI_Font1 = false;
			}
			else
			{
				fontToolParam.EnableAI_Font1 = true;
			}
			if (fontToolParam.AIModel_Font2 == "")
			{
				fontToolParam.EnableAI_Font2 = false;
			}
			else
			{
				fontToolParam.EnableAI_Font2 = true;
			}
			if (fontToolParam.AIModel_Font3 == "")
			{
				fontToolParam.EnableAI_Font3 = false;
			}
			else
			{
				fontToolParam.EnableAI_Font3 = true;
			}
			if (fontToolParam.AIModel_Font4 == "")
			{
				fontToolParam.EnableAI_Font4 = false;
			}
			else
			{
				fontToolParam.EnableAI_Font4 = true;
			}
			if (fontToolParam.AIModel_Font5 == "")
			{
				fontToolParam.EnableAI_Font5 = false;
			}
			else
			{
				fontToolParam.EnableAI_Font5 = true;
			}
			fontToolParam.FontChar1 = btn_Font_1.Content.ToString();
			fontToolParam.FontChar2 = btn_Font_2.Content.ToString();
			fontToolParam.FontChar3 = btn_Font_3.Content.ToString();
			fontToolParam.FontChar4 = btn_Font_4.Content.ToString();
			fontToolParam.FontChar5 = btn_Font_5.Content.ToString();
			fontToolParam.EnableFontCheck1 = chk_CheckFont1.IsChecked.Value;
			fontToolParam.EnableFontCheck2 = chk_CheckFont2.IsChecked.Value;
			fontToolParam.EnableFontCheck3 = chk_CheckFont3.IsChecked.Value;
			fontToolParam.EnableFontCheck4 = chk_CheckFont4.IsChecked.Value;
			fontToolParam.EnableFontCheck5 = chk_CheckFont5.IsChecked.Value;
			fontToolParam.BypassOCR1 = chk_BypassOCR1.IsChecked.Value;
			fontToolParam.BypassOCR2 = chk_BypassOCR2.IsChecked.Value;
			fontToolParam.BypassOCR3 = chk_BypassOCR3.IsChecked.Value;
			fontToolParam.BypassOCR4 = chk_BypassOCR4.IsChecked.Value;
			fontToolParam.BypassOCR5 = chk_BypassOCR5.IsChecked.Value;
			if (fontToolParam.FontChar1 == "-")
			{
				chk_DoubleWidthFont1.IsChecked = false;
			}
			fontToolParam.DoubleWidthFont1 = chk_DoubleWidthFont1.IsChecked.Value;
			fontToolParam.DoubleWidthFont2 = chk_DoubleWidthFont2.IsChecked.Value;
			fontToolParam.FontIndex1 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar1);
			fontToolParam.FontIndex2 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar2);
			fontToolParam.FontIndex3 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar3);
			fontToolParam.FontIndex4 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar4);
			fontToolParam.FontIndex5 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar5);
			fontToolParam.MinScore_Font1 = Convert.ToInt32(txt_AcceptScoreFont1.Text);
			fontToolParam.MinScore_Font2 = Convert.ToInt32(txt_AcceptScoreFont2.Text);
			fontToolParam.MinScore_Font3 = Convert.ToInt32(txt_AcceptScoreFont3.Text);
			fontToolParam.MinScore_Font4 = Convert.ToInt32(txt_AcceptScoreFont4.Text);
			fontToolParam.MinScore_Font5 = Convert.ToInt32(txt_AcceptScoreFont5.Text);
			fontToolParam.EnableSelectFace = chk_SelectFace.IsChecked.Value;
			fontToolParam.FaceIndex = SelectFace;
			fontToolParam.ErrorBin = (FLOW_BIN)cb_ErrorBin.SelectedIndex;
			fontToolParam.ParetoIndex = cb_ParetoChart.SelectedIndex;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (GetTracker(ref rect))
			{
				fontToolParam.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[0].L;
				fontToolParam.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[0].T;
				fontToolParam.Track1_R = inspection.paramFile.AdvLocateParam[0].Width - fontToolParam.Track1_L - rect.Width;
				fontToolParam.Track1_B = inspection.paramFile.AdvLocateParam[0].Height - fontToolParam.Track1_T - rect.Height;
				if (rad_FDByPublic.IsChecked == true)
				{
					fontToolParam.FontDBUsePublicMode = true;
				}
				else
				{
					fontToolParam.FontDBUsePublicMode = false;
				}
				if (rad_PDByPublic.IsChecked == true)
				{
					fontToolParam.PublicDBUsePublicMode = true;
				}
				else
				{
					fontToolParam.PublicDBUsePublicMode = false;
				}
				fontToolParam.ValueChangeFlag = true;
				inspection.paramFile.FontToolParam = fontToolParam;
			}
		}
		catch (Exception ex)
		{
			string text = "FTW07-FontToolWindow-WriteUIValuetoParam()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WritePublicPattern(int index, int channel)
	{
		try
		{
			if (DontSave)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Preview Mode Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，預覽模式下取消設定、儲存資料庫功能。", "錯誤");
				}
				else
				{
					MessageBox.Show("错误，预览模式下取消设定、储存资料库功能。", "错误");
				}
				return;
			}
			string text = "";
			text = ((!UsePublicDB) ? string.Format("{0}Recipe\\{1}\\PrivateDB\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : (AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}P{1}-{2}.bmp", text, index + 1, channel);
			MatchPattern matchPattern = PatternDB[index];
			matchPattern.ValueChangeFlag = true;
			if (!PDBMat[index].Empty())
			{
				Cv2.ImWrite(text2, PDBMat[index]);
				matchPattern.PatternON = true;
				matchPattern.PatternName = PDBText[index % 70].Text;
				matchPattern.PatternChannel = PatternChannelSelect;
				matchPattern.PatternPath = text2;
				matchPattern.PatternImage = PDBMat[index];
			}
			PatternDB[index] = matchPattern;
		}
		catch (Exception ex)
		{
			string text3 = "FTW07-FontToolWindow-WritePublicPattern()";
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text3 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetFontPatternPath(string FontChar, int index, string ColorChar)
	{
		try
		{
			string text = (index + 1).ToString();
			if (UsePublicFont)
			{
				FontPatternBasePath = AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\Font\\";
			}
			else
			{
				FontPatternBasePath = string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName);
			}
			if (!Directory.Exists(FontPatternBasePath))
			{
				Directory.CreateDirectory(FontPatternBasePath);
			}
			return string.Format("{0}{1}{2}{3}.bmp", FontPatternBasePath, FontChar, text, ColorChar);
		}
		catch (Exception ex)
		{
			string text2 = "FTW07-FontToolWindow-GetFontPatternPath()";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text2 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return "";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetPBDPattern_Click(object sender, RoutedEventArgs e)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		GetPatternTracker(ref rect);
		if (rect == PTracker)
		{
			return;
		}
		int num = Convert.ToInt32(((Button)sender).Uid);
		int num2 = CurrentPublicDBPage * 70 + num;
		if (PDBText[num].Text == "")
		{
			ItemInputWindow itemInputWindow = new ItemInputWindow();
			itemInputWindow.ShowDialog();
			if (itemInputWindow.InputStr == "")
			{
				MessageBox.Show("請輸入圖形/字形名稱", "錯誤");
				return;
			}
			PDBText[num].Text = itemInputWindow.InputStr;
		}
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
			if (PatternChannelSelect == 10)
			{
				PDBMat[num2] = new Mat();
				PDBImaage[num].Source = null;
				PDBLabel[num].Content = "";
				PatternDB[num2].PatternON = false;
				PatternDB[num2].ValueChangeFlag = true;
				MatchPattern obj = PatternDB[num2];
				obj.ValueChangeFlag = true;
				obj.PatternON = false;
				obj.PatternName = "";
				obj.PatternChannel = 0;
				obj.PatternPath = "";
				obj.PatternImage = PDBMat[num2];
			}
			else if (GetPatternTracker(ref rect2))
			{
				PDBMat[num2] = new Mat(inspection.buffers.GetChannelMat(PatternChannelSelect), rect2).Clone();
				inspection.paramFile.FontToolParam.PatternChannel = PatternChannelSelect;
				WritePublicPattern(num2, PatternChannelSelect);
				PDBImaage[num].Source = PDBMat[num2].ToBitmapSource();
				PDBLabel[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[PatternChannelSelect], PDBMat[num2].Width, PDBMat[num2].Height);
			}
			cb_PatternSelect.SelectedIndex = num2;
		}
		catch (Exception ex)
		{
			string text = "FTW08-FontToolWindow-Btn_SetPBDPattern_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Btn_PublicPagePrev_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentPublicDBPage > 0)
		{
			CurrentPublicDBPage--;
			lab_PublicDBPage.Content = CurrentPublicDBPage + 1;
			UpdatePBPatternImage();
		}
	}

	private void Btn_PublicPageNext_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentPublicDBPage < 2)
		{
			CurrentPublicDBPage++;
			lab_PublicDBPage.Content = CurrentPublicDBPage + 1;
			UpdatePBPatternImage();
		}
	}

	private void Cb_AlignToMatchText_Checked(object sender, RoutedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			if (inspection.selectwayTool.PatternFound)
			{
				AlignMatchTextFound = true;
			}
			else
			{
				AlignMatchTextFound = false;
			}
		}
	}

	private void Cb_AlignToMatchText_Unchecked(object sender, RoutedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			AlignMatchTextFound = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
		if (PreviewMode || !inited || !UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
		}
		else
		{
			if (!inspection.buffers.LocatedReady || DontSave)
			{
				return;
			}
			OpenCvSharp.Rect rect = default(OpenCvSharp.Rect);
			if (!GetTracker(ref rect))
			{
				return;
			}
			if (rect.Width > 0 && rect.Height > 0)
			{
				try
				{
					WriteUIValueToParam();
					if (chk_FontAlignVertical.IsChecked == true)
					{
						lab_FontAlignVertical.Visibility = Visibility.Visible;
					}
					else
					{
						lab_FontAlignVertical.Visibility = Visibility.Hidden;
					}
					for (int i = 0; i < 5; i++)
					{
						OKIcon[i].Visibility = Visibility.Hidden;
						NGIcon[i].Visibility = Visibility.Hidden;
						LabMatchScore[i].Content = "";
						RTFonts[i].Fill = Brushes.White;
						CHK_CheckFont[i].Foreground = Brushes.Gray;
						CHKByPassOCR[i].Foreground = Brushes.Gray;
						TxtAcceptScore[i].Background = Brushes.White;
						TxtAcceptScore[i].Foreground = Brushes.Gray;
					}
					TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
					TimeStart();
					bool flag = inspection.fontTool.Inspect(ref iresult, FaceIndex);
					double num = TimeStop();
					lab_Time.Content = string.Format("{0:N3}", num);
					lab_MatchScore.Content = string.Format("{0}, {1}", inspection.fontTool.MatchScore_1, inspection.fontTool.MatchScore_2);
					if (inspection.fontTool.FontConnectNG)
					{
						chk_FontConnected.Foreground = Brushes.Red;
					}
					else
					{
						chk_FontConnected.Foreground = Brushes.Black;
					}
					chk_EnabledOCR.Foreground = Brushes.Black;
					for (int j = 0; j < 5; j++)
					{
						if (!(BtnFonts[j].Content.ToString() == ""))
						{
							int num2 = inspection.fontTool.FontMatchScore[j];
							LabMatchScore[j].Content = num2;
							BTNQuickAdd[j].Content = BtnFonts[j].Content;
							if (!inspection.fontTool.FontResult[j])
							{
								FontsResult[j] = false;
								OKIcon[j].Visibility = Visibility.Hidden;
								NGIcon[j].Visibility = Visibility.Visible;
								BTNQuickAdd[j].Background = Brushes.PaleVioletRed;
								RTFonts[j].Fill = Brushes.Fuchsia;
								if (!inspection.fontTool.AdvFontCheck[j])
								{
									CHK_CheckFont[j].Foreground = Brushes.Red;
								}
								if (!inspection.fontTool.OCRResult[j])
								{
									CHKByPassOCR[j].Foreground = Brushes.Red;
									chk_EnabledOCR.Foreground = Brushes.Red;
								}
								if (!inspection.fontTool.ScoreResult[j])
								{
									TxtAcceptScore[j].Background = Brushes.Fuchsia;
									TxtAcceptScore[j].Foreground = Brushes.White;
								}
							}
							else
							{
								FontsResult[j] = true;
								OKIcon[j].Visibility = Visibility.Visible;
								NGIcon[j].Visibility = Visibility.Hidden;
								BTNQuickAdd[j].Background = Brushes.LawnGreen;
								RTFonts[j].Fill = Brushes.Lime;
							}
						}
					}
					for (int k = 0; k < 5; k++)
					{
						LabScoreF1[k].Content = inspection.fontTool.AIMatchScore[0][k];
						LabScoreF2[k].Content = inspection.fontTool.AIMatchScore[1][k];
						LabScoreF3[k].Content = inspection.fontTool.AIMatchScore[2][k];
						LabScoreF4[k].Content = inspection.fontTool.AIMatchScore[3][k];
						LabScoreF5[k].Content = inspection.fontTool.AIMatchScore[4][k];
					}
					Label_Result.Visibility = Visibility.Visible;
					if (flag)
					{
						Label_Result.Background = Brushes.LawnGreen;
						Label_Result.Content = "OK";
						rt_Result.Fill = Brushes.LawnGreen;
					}
					else
					{
						Label_Result.Background = Brushes.PaleVioletRed;
						Label_Result.Content = "NG";
						rt_Result.Fill = Brushes.PaleVioletRed;
					}
					Mat mat = new Mat();
					if (!inspection.fontTool.FlipStatus)
					{
						ShowBitmap.CopyTo(mat);
						btn_Result_Flip.Visibility = Visibility.Hidden;
					}
					else
					{
						ShowBitmap.CopyTo(ShowBitmapFlip);
						if (inspection.fontTool.FoundMatchTextRect.Width > 0 && inspection.fontTool.FoundMatchTextRect.Height > 0)
						{
							using Mat mat2 = MM.newMat(ShowBitmapFlip, inspection.fontTool.FoundMatchTextRect).Clone();
							using Mat m = new Mat(ShowBitmapFlip, inspection.fontTool.FoundMatchTextRect);
							using (new Mat())
							{
								Cv2.Flip(mat2, mat2, FlipMode.XY);
								mat2.CopyTo(m);
							}
						}
						mat = ShowBitmapFlip;
						btn_Result_Flip.Visibility = Visibility.Visible;
					}
					if (chk_ShowMatchAll.IsChecked == true)
					{
						mat.Rectangle(inspection.fontTool.GetAllFontMatchRect(), Scalar.LimeGreen);
					}
					if (inspection.fontTool.FontConnectNG)
					{
						mat.Rectangle(inspection.fontTool.FoundMatchTextRect, Scalar.Red, 2);
					}
					if (!inspection.fontTool.FirstMatchResult)
					{
						mat.Rectangle(inspection.fontTool.FoundMatchTextRect, Scalar.Red, 2);
					}
					for (int l = 0; l < 5; l++)
					{
						if (chk_ShowMatch.IsChecked == true)
						{
							if (inspection.fontTool.FontResult[l])
							{
								mat.Rectangle(inspection.fontTool.GetFontMatchRect(l), Scalar.Lime);
							}
							else
							{
								mat.Rectangle(inspection.fontTool.GetFontMatchRect(l), Scalar.Fuchsia, 2);
							}
						}
						if (chk_ShowSearch.IsChecked == true && (!ShowExtend1 || l % 2 != 0) && (ShowExtend1 || l % 2 != 1))
						{
							OpenCvSharp.Rect fontSearchRect = inspection.fontTool.GetFontSearchRect(l);
							if (l % 2 != 0 && fontSearchRect.Width > 0 && fontSearchRect.Height > 0)
							{
								fontSearchRect.Top -= 2;
								fontSearchRect.Height += 4;
								mat.Rectangle(fontSearchRect, Scalar.OrangeRed);
							}
							else
							{
								mat.Rectangle(fontSearchRect, Scalar.Orange);
							}
						}
					}
					ShowExtend1 = !ShowExtend1;
					ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
					ImageBrush imageBrush = new ImageBrush();
					imageBrush.ImageSource = imageSource;
					cnv_Image.Background = imageBrush;
					txt_AllResult.Text = inspection.fontTool.Tracker1ResultText;
					for (int n = 0; n < 5; n++)
					{
						if (inspection.fontTool.FontBinary[n].Width > 0 && inspection.fontTool.FontBinary[n].Height > 0)
						{
							if (n < inspection.paramFile.FontToolParam.AllFont.Length)
							{
								ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(inspection.fontTool.FontBinary[n]);
								ImageBrush imageBrush2 = new ImageBrush();
								imageBrush2.ImageSource = imageSource2;
								CnvFontBinary[n].Background = imageBrush2;
							}
							else
							{
								ImageSource imageSource3 = null;
								ImageBrush imageBrush3 = new ImageBrush();
								imageBrush3.ImageSource = imageSource3;
								CnvFontBinary[n].Background = imageBrush3;
							}
						}
					}
					return;
				}
				catch (Exception ex)
				{
					string text = "FTW01-FontToolWindow-Btn_Test_Click()";
					text += ex.ToString();
					UpdateUI = true;
					CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
					if (ShowMessage)
					{
						MessageBox.Show(text, "錯誤");
					}
					return;
				}
			}
			SetResult(result: false);
			txt_AllResult.Clear();
			if (LanguageType == 0)
			{
				txt_AllResult.Text = "Tracker A Width and Height < 0!，Please Reset Tracker A!";
			}
			else if (LanguageType == 1)
			{
				txt_AllResult.Text = "檢測區域A寬度高度為0!，請重設檢測區域A";
			}
			else
			{
				txt_AllResult.Text = "检测区域A宽度高度为0!，请重设检测区域A";
			}
			DrawBackGround();
		}
	}

	private void Img_Font_R1_PreviewMouseMove(object sender, MouseEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddFontImage(string FontChar, string id)
	{
		try
		{
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
			GetPatternTracker(ref rect);
			if (rect == PTracker)
			{
				return;
			}
			if (DontSave)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Preview Mode Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，預覽模式下取消設定、儲存資料庫功能。", "錯誤");
				}
				else
				{
					MessageBox.Show("错误，预览模式下取消设定、储存资料库功能。", "错误");
				}
				return;
			}
			Mat mat = new Mat();
			OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, 0, 0);
			int num = Convert.ToInt32(id);
			_ = FontChar + id;
			if (!GetPatternTracker(ref rect2))
			{
				return;
			}
			mat = new Mat(inspection.buffers.GetChannelMat(inspection.paramFile.FontToolParam.PatternChannel), rect2).Clone();
			if (FontChar == null)
			{
				return;
			}
			int length = FontChar.Length;
			if (length != 2)
			{
				return;
			}
			switch (FontChar[0])
			{
			default:
				return;
			case 'R':
				if (!(FontChar == "R_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatR[num]);
				FontImgR[num].Source = inspection.fontTool.FontMatR[num].ToBitmapSource();
				break;
			case 'M':
				if (!(FontChar == "M_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatM[num]);
				FontImgM[num].Source = inspection.fontTool.FontMatM[num].ToBitmapSource();
				break;
			case '0':
				if (!(FontChar == "0_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat0[num]);
				FontImg0[num].Source = inspection.fontTool.FontMat0[num].ToBitmapSource();
				break;
			case '1':
				if (!(FontChar == "1_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat1[num]);
				FontImg1[num].Source = inspection.fontTool.FontMat1[num].ToBitmapSource();
				break;
			case '2':
				if (!(FontChar == "2_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat2[num]);
				FontImg2[num].Source = inspection.fontTool.FontMat2[num].ToBitmapSource();
				break;
			case '3':
				if (!(FontChar == "3_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat3[num]);
				FontImg3[num].Source = inspection.fontTool.FontMat3[num].ToBitmapSource();
				break;
			case '4':
				if (!(FontChar == "4_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat4[num]);
				FontImg4[num].Source = inspection.fontTool.FontMat4[num].ToBitmapSource();
				break;
			case '5':
				if (!(FontChar == "5_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat5[num]);
				FontImg5[num].Source = inspection.fontTool.FontMat5[num].ToBitmapSource();
				break;
			case '6':
				if (!(FontChar == "6_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat6[num]);
				FontImg6[num].Source = inspection.fontTool.FontMat6[num].ToBitmapSource();
				break;
			case '7':
				if (!(FontChar == "7_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat7[num]);
				FontImg7[num].Source = inspection.fontTool.FontMat7[num].ToBitmapSource();
				break;
			case '8':
				if (!(FontChar == "8_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat8[num]);
				FontImg8[num].Source = inspection.fontTool.FontMat8[num].ToBitmapSource();
				break;
			case '9':
				if (!(FontChar == "9_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMat9[num]);
				FontImg9[num].Source = inspection.fontTool.FontMat9[num].ToBitmapSource();
				break;
			case 'C':
				if (!(FontChar == "C_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatC[num]);
				FontImgC[num].Source = inspection.fontTool.FontMatC[num].ToBitmapSource();
				break;
			case 'U':
				if (!(FontChar == "U_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatU[num]);
				FontImgU[num].Source = inspection.fontTool.FontMatU[num].ToBitmapSource();
				break;
			case 'X':
				if (!(FontChar == "X_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatX[num]);
				FontImgX[num].Source = inspection.fontTool.FontMatX[num].ToBitmapSource();
				break;
			case 'B':
				if (!(FontChar == "B_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatB[num]);
				FontImgB[num].Source = inspection.fontTool.FontMatB[num].ToBitmapSource();
				break;
			case 'D':
				if (!(FontChar == "D_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatD[num]);
				FontImgD[num].Source = inspection.fontTool.FontMatD[num].ToBitmapSource();
				break;
			case 'E':
				if (!(FontChar == "E_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatE[num]);
				FontImgE[num].Source = inspection.fontTool.FontMatE[num].ToBitmapSource();
				break;
			case 'A':
				if (!(FontChar == "A_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatA[num]);
				FontImgA[num].Source = inspection.fontTool.FontMatA[num].ToBitmapSource();
				break;
			case '-':
				if (!(FontChar == "-_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatDash[num]);
				FontImgDash[num].Source = inspection.fontTool.FontMatDash[num].ToBitmapSource();
				break;
			case 'F':
				if (!(FontChar == "F_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[0][num]);
				FontImgOther[0][num].Source = inspection.fontTool.FontMatOther[0][num].ToBitmapSource();
				break;
			case 'G':
				if (!(FontChar == "G_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[1][num]);
				FontImgOther[1][num].Source = inspection.fontTool.FontMatOther[1][num].ToBitmapSource();
				break;
			case 'H':
				if (!(FontChar == "H_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[2][num]);
				FontImgOther[2][num].Source = inspection.fontTool.FontMatOther[2][num].ToBitmapSource();
				break;
			case 'I':
				if (!(FontChar == "I_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[3][num]);
				FontImgOther[3][num].Source = inspection.fontTool.FontMatOther[3][num].ToBitmapSource();
				break;
			case 'J':
				if (!(FontChar == "J_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[4][num]);
				FontImgOther[4][num].Source = inspection.fontTool.FontMatOther[4][num].ToBitmapSource();
				break;
			case 'K':
				if (!(FontChar == "K_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[5][num]);
				FontImgOther[5][num].Source = inspection.fontTool.FontMatOther[5][num].ToBitmapSource();
				break;
			case 'L':
				if (!(FontChar == "L_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[6][num]);
				FontImgOther[6][num].Source = inspection.fontTool.FontMatOther[6][num].ToBitmapSource();
				break;
			case 'N':
				if (!(FontChar == "N_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[7][num]);
				FontImgOther[7][num].Source = inspection.fontTool.FontMatOther[7][num].ToBitmapSource();
				break;
			case 'O':
				if (!(FontChar == "O_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[8][num]);
				FontImgOther[8][num].Source = inspection.fontTool.FontMatOther[8][num].ToBitmapSource();
				break;
			case 'P':
				if (!(FontChar == "P_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[9][num]);
				FontImgOther[9][num].Source = inspection.fontTool.FontMatOther[9][num].ToBitmapSource();
				break;
			case 'Q':
				if (!(FontChar == "Q_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[10][num]);
				FontImgOther[10][num].Source = inspection.fontTool.FontMatOther[10][num].ToBitmapSource();
				break;
			case 'S':
				if (!(FontChar == "S_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[11][num]);
				FontImgOther[11][num].Source = inspection.fontTool.FontMatOther[11][num].ToBitmapSource();
				break;
			case 'T':
				if (!(FontChar == "T_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[12][num]);
				FontImgOther[12][num].Source = inspection.fontTool.FontMatOther[12][num].ToBitmapSource();
				break;
			case 'V':
				if (!(FontChar == "V_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[13][num]);
				FontImgOther[13][num].Source = inspection.fontTool.FontMatOther[13][num].ToBitmapSource();
				break;
			case 'W':
				if (!(FontChar == "W_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[14][num]);
				FontImgOther[14][num].Source = inspection.fontTool.FontMatOther[14][num].ToBitmapSource();
				break;
			case 'Y':
				if (!(FontChar == "Y_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[15][num]);
				FontImgOther[15][num].Source = inspection.fontTool.FontMatOther[15][num].ToBitmapSource();
				break;
			case 'Z':
				if (!(FontChar == "Z_"))
				{
					return;
				}
				mat.CopyTo(inspection.fontTool.FontMatOther[16][num]);
				FontImgOther[16][num].Source = inspection.fontTool.FontMatOther[16][num].ToBitmapSource();
				break;
			case '.':
			case '/':
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
			case '@':
				return;
			}
			string fontPatternPath = GetFontPatternPath(FontChar, num, "G");
			if (fontPatternPath != "" && mat.Width > 0 && mat.Height > 0)
			{
				Cv2.ImWrite(fontPatternPath, mat);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW08-FontToolWindow-AddFontImage()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RemoveFontImage(string FontChar, string id)
	{
		try
		{
			if (DontSave)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Preview Mode Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，預覽模式下取消設定、儲存資料庫功能。", "錯誤");
				}
				else
				{
					MessageBox.Show("错误，预览模式下取消设定、储存资料库功能。", "错误");
				}
				return;
			}
			int num = Convert.ToInt32(id);
			if (MessageBox.Show(string.Format("是否移除文字資料庫{0}{1}?", FontChar, num + 1), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes || FontChar == null)
			{
				return;
			}
			int length = FontChar.Length;
			if (length != 2)
			{
				return;
			}
			switch (FontChar[0])
			{
			default:
				return;
			case 'R':
				if (!(FontChar == "R_"))
				{
					return;
				}
				inspection.fontTool.FontMatR[num] = new Mat();
				FontImgR[num].Source = null;
				break;
			case 'M':
				if (!(FontChar == "M_"))
				{
					return;
				}
				inspection.fontTool.FontMatM[num] = new Mat();
				FontImgM[num].Source = null;
				break;
			case '0':
				if (!(FontChar == "0_"))
				{
					return;
				}
				inspection.fontTool.FontMat0[num] = new Mat();
				FontImg0[num].Source = null;
				break;
			case '1':
				if (!(FontChar == "1_"))
				{
					return;
				}
				inspection.fontTool.FontMat1[num] = new Mat();
				FontImg1[num].Source = null;
				break;
			case '2':
				if (!(FontChar == "2_"))
				{
					return;
				}
				inspection.fontTool.FontMat2[num] = new Mat();
				FontImg2[num].Source = null;
				break;
			case '3':
				if (!(FontChar == "3_"))
				{
					return;
				}
				inspection.fontTool.FontMat3[num] = new Mat();
				FontImg3[num].Source = null;
				break;
			case '4':
				if (!(FontChar == "4_"))
				{
					return;
				}
				inspection.fontTool.FontMat4[num] = new Mat();
				FontImg4[num].Source = null;
				break;
			case '5':
				if (!(FontChar == "5_"))
				{
					return;
				}
				inspection.fontTool.FontMat5[num] = new Mat();
				FontImg5[num].Source = null;
				break;
			case '6':
				if (!(FontChar == "6_"))
				{
					return;
				}
				inspection.fontTool.FontMat6[num] = new Mat();
				FontImg6[num].Source = null;
				break;
			case '7':
				if (!(FontChar == "7_"))
				{
					return;
				}
				inspection.fontTool.FontMat7[num] = new Mat();
				FontImg7[num].Source = null;
				break;
			case '8':
				if (!(FontChar == "8_"))
				{
					return;
				}
				inspection.fontTool.FontMat8[num] = new Mat();
				FontImg8[num].Source = null;
				break;
			case '9':
				if (!(FontChar == "9_"))
				{
					return;
				}
				inspection.fontTool.FontMat9[num] = new Mat();
				FontImg9[num].Source = null;
				break;
			case 'C':
				if (!(FontChar == "C_"))
				{
					return;
				}
				inspection.fontTool.FontMatC[num] = new Mat();
				FontImgC[num].Source = null;
				break;
			case 'X':
				if (!(FontChar == "X_"))
				{
					return;
				}
				inspection.fontTool.FontMatX[num] = new Mat();
				FontImgX[num].Source = null;
				break;
			case 'B':
				if (!(FontChar == "B_"))
				{
					return;
				}
				inspection.fontTool.FontMatB[num] = new Mat();
				FontImgB[num].Source = null;
				break;
			case 'D':
				if (!(FontChar == "D_"))
				{
					return;
				}
				inspection.fontTool.FontMatD[num] = new Mat();
				FontImgD[num].Source = null;
				break;
			case 'E':
				if (!(FontChar == "E_"))
				{
					return;
				}
				inspection.fontTool.FontMatE[num] = new Mat();
				FontImgE[num].Source = null;
				break;
			case 'U':
				if (!(FontChar == "U_"))
				{
					return;
				}
				inspection.fontTool.FontMatU[num] = new Mat();
				FontImgU[num].Source = null;
				break;
			case '-':
				if (!(FontChar == "-_"))
				{
					return;
				}
				inspection.fontTool.FontMatDash[num] = new Mat();
				FontImgDash[num].Source = null;
				break;
			case 'A':
				if (!(FontChar == "A_"))
				{
					return;
				}
				inspection.fontTool.FontMatA[num] = new Mat();
				FontImgA[num].Source = null;
				break;
			case 'F':
				if (!(FontChar == "F_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[0][num] = new Mat();
				FontImgOther[0][num].Source = null;
				break;
			case 'G':
				if (!(FontChar == "G_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[1][num] = new Mat();
				FontImgOther[1][num].Source = null;
				break;
			case 'H':
				if (!(FontChar == "H_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[2][num] = new Mat();
				FontImgOther[2][num].Source = null;
				break;
			case 'I':
				if (!(FontChar == "I_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[3][num] = new Mat();
				FontImgOther[3][num].Source = null;
				break;
			case 'J':
				if (!(FontChar == "J_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[4][num] = new Mat();
				FontImgOther[4][num].Source = null;
				break;
			case 'K':
				if (!(FontChar == "K_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[5][num] = new Mat();
				FontImgOther[5][num].Source = null;
				break;
			case 'L':
				if (!(FontChar == "L_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[6][num] = new Mat();
				FontImgOther[6][num].Source = null;
				break;
			case 'N':
				if (!(FontChar == "N_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[7][num] = new Mat();
				FontImgOther[7][num].Source = null;
				break;
			case 'O':
				if (!(FontChar == "O_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[8][num] = new Mat();
				FontImgOther[8][num].Source = null;
				break;
			case 'P':
				if (!(FontChar == "P_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[9][num] = new Mat();
				FontImgOther[9][num].Source = null;
				break;
			case 'Q':
				if (!(FontChar == "Q_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[10][num] = new Mat();
				FontImgOther[10][num].Source = null;
				break;
			case 'S':
				if (!(FontChar == "S_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[11][num] = new Mat();
				FontImgOther[11][num].Source = null;
				break;
			case 'T':
				if (!(FontChar == "T_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[12][num] = new Mat();
				FontImgOther[12][num].Source = null;
				break;
			case 'V':
				if (!(FontChar == "V_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[13][num] = new Mat();
				FontImgOther[13][num].Source = null;
				break;
			case 'W':
				if (!(FontChar == "W_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[14][num] = new Mat();
				FontImgOther[14][num].Source = null;
				break;
			case 'Y':
				if (!(FontChar == "Y_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[15][num] = new Mat();
				FontImgOther[15][num].Source = null;
				break;
			case 'Z':
				if (!(FontChar == "Z_"))
				{
					return;
				}
				inspection.fontTool.FontMatOther[16][num] = new Mat();
				FontImgOther[16][num].Source = null;
				break;
			case '.':
			case '/':
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
			case '@':
				return;
			}
			string fontPatternPath = GetFontPatternPath(FontChar, num, "G");
			if (File.Exists(fontPatternPath))
			{
				File.Delete(fontPatternPath);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW08-FontToolWindow-RemoveFontImage()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_R1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("R_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("X_", button.Uid);
		}
		else
		{
			AddFontImage("Q_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_M1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("M_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("B_", button.Uid);
		}
		else
		{
			AddFontImage("S_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_C1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("C_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("E_", button.Uid);
		}
		else
		{
			AddFontImage("T_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_U1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("U_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("D_", button.Uid);
		}
		else
		{
			AddFontImage("V_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_Dash1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("-_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("A_", button.Uid);
		}
		else
		{
			AddFontImage("W_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_01_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("0_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("F_", button.Uid);
		}
		else
		{
			AddFontImage("Y_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_11_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("1_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			AddFontImage("G_", button.Uid);
		}
		else
		{
			AddFontImage("Z_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_21_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("2_", button.Uid);
		}
		else
		{
			AddFontImage("H_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_31_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("3_", button.Uid);
		}
		else
		{
			AddFontImage("I_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_41_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("4_", button.Uid);
		}
		else
		{
			AddFontImage("J_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_51_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("5_", button.Uid);
		}
		else
		{
			AddFontImage("K_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_61_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("6_", button.Uid);
		}
		else
		{
			AddFontImage("L_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_71_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("7_", button.Uid);
		}
		else
		{
			AddFontImage("N_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_81_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("8_", button.Uid);
		}
		else
		{
			AddFontImage("O_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_91_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			AddFontImage("9_", button.Uid);
		}
		else
		{
			AddFontImage("P_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AutoSetFont_Click(object sender, RoutedEventArgs e)
	{
		if (cb_PatternSelect.SelectedIndex < 0)
		{
			MessageBox.Show("尚未設定文字影像", "警告");
			return;
		}
		string input = txt_InputText.Text.ToUpper();
		input = Regex.Replace(input, "\\s", "");
		txt_InputText.Text = input;
		if (input.Length > 5)
		{
			if (input.Length > 5 && input.Length <= 9)
			{
				inspection.paramFile.FontToolParam.AllFont = input;
				inspection.paramFile.FontToolParam.PatternSelect = cb_PatternSelect.SelectedIndex;
				inspection.paramFile.FontToolParam.FontChar1 = "";
				inspection.paramFile.FontToolParam.FontChar2 = "";
				inspection.paramFile.FontToolParam.FontChar3 = "";
				inspection.paramFile.FontToolParam.FontChar4 = "";
				inspection.paramFile.FontToolParam.FontChar5 = "";
				inspection.paramFile.FontToolParam.FontChar6 = "";
				inspection.paramFile.FontToolParam.FontChar7 = "";
				inspection.paramFile.FontToolParam.FontChar8 = "";
				inspection.paramFile.FontToolParam.FontChar9 = "";
				inspection.paramFile.FontToolParam.ValueChangeFlag = true;
				inspection.paramFile.FontToolParam.ValueChangeFlag = true;
				MessageBox.Show("字元字數更新，請重新開啟工具", "警告");
				Close();
			}
			else
			{
				MessageBox.Show("字元字數超過限制(9)", "警告");
			}
			return;
		}
		if (input.Length <= 0)
		{
			MessageBox.Show("請輸入檢測字元", "警告");
			return;
		}
		for (int i = 0; i < input.Length; i++)
		{
			if (inspection.fontTool.GetFontIndex(input.Substring(i, 1)) == -1)
			{
				MessageBox.Show(string.Format("輸入字元包含資料庫不支援的字元({0})", input.Substring(i, 1)), "警告");
				return;
			}
		}
		if (input.Length <= 0)
		{
			return;
		}
		for (int j = 0; j < 5; j++)
		{
			CBAIModeFont[j].SelectedIndex = 0;
		}
		grd_Font_1.Visibility = Visibility.Visible;
		btn_Font_1.Content = input.Substring(0, 1);
		btn_Font_1.Uid = input.Substring(0, 1);
		if (chk_AutoLoadAI.IsChecked == true)
		{
			cb_AIModelFont1.SelectedIndex = GetAIModelIndex(input.Substring(0, 1) + ".onnx");
			txt_AcceptScoreFont1.Text = "60";
		}
		if (input.Length >= 2)
		{
			grd_Font_2.Visibility = Visibility.Visible;
			btn_Font_2.Content = input.Substring(1, 1);
			btn_Font_2.Uid = input.Substring(1, 1);
			if (chk_AutoLoadAI.IsChecked == true)
			{
				cb_AIModelFont2.SelectedIndex = GetAIModelIndex(input.Substring(1, 1) + ".onnx");
				txt_AcceptScoreFont2.Text = "60";
			}
		}
		else
		{
			grd_Font_2.Visibility = Visibility.Hidden;
			btn_Font_2.Content = "";
			btn_Font_2.Uid = "";
			cb_AIModelFont2.SelectedIndex = 0;
		}
		if (input.Length >= 3)
		{
			grd_Font_3.Visibility = Visibility.Visible;
			btn_Font_3.Content = input.Substring(2, 1);
			btn_Font_3.Uid = input.Substring(2, 1);
			if (chk_AutoLoadAI.IsChecked == true)
			{
				cb_AIModelFont3.SelectedIndex = GetAIModelIndex(input.Substring(2, 1) + ".onnx");
				txt_AcceptScoreFont3.Text = "60";
			}
		}
		else
		{
			grd_Font_3.Visibility = Visibility.Hidden;
			btn_Font_3.Content = "";
			btn_Font_3.Uid = "";
			cb_AIModelFont3.SelectedIndex = 0;
		}
		if (input.Length >= 4)
		{
			grd_Font_4.Visibility = Visibility.Visible;
			btn_Font_4.Content = input.Substring(3, 1);
			btn_Font_4.Uid = input.Substring(3, 1);
			if (chk_AutoLoadAI.IsChecked == true)
			{
				cb_AIModelFont4.SelectedIndex = GetAIModelIndex(input.Substring(3, 1) + ".onnx");
				txt_AcceptScoreFont4.Text = "60";
			}
		}
		else
		{
			grd_Font_4.Visibility = Visibility.Hidden;
			btn_Font_4.Content = "";
			btn_Font_4.Uid = "";
			cb_AIModelFont4.SelectedIndex = 0;
		}
		if (input.Length >= 5)
		{
			grd_Font_5.Visibility = Visibility.Visible;
			btn_Font_5.Content = input.Substring(4, 1);
			btn_Font_5.Uid = input.Substring(4, 1);
			if (chk_AutoLoadAI.IsChecked == true)
			{
				cb_AIModelFont5.SelectedIndex = GetAIModelIndex(input.Substring(4, 1) + ".onnx");
				txt_AcceptScoreFont5.Text = "60";
			}
		}
		else
		{
			grd_Font_5.Visibility = Visibility.Hidden;
			btn_Font_5.Content = "";
			btn_Font_5.Uid = "";
			cb_AIModelFont5.SelectedIndex = 0;
		}
		for (int k = 0; k < input.Length; k++)
		{
			string onnxfile = string.Format("{0}.onnx", input.Substring(k, 1));
			BTNCreateAIModel[k].Visibility = ((GetAIModelIndex(onnxfile) != 0) ? Visibility.Hidden : Visibility.Visible);
		}
		inspection.fontTool.PatternRectSeted = false;
		Btn_Test_Click(this, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int GetAIModelIndex(string onnxfile)
	{
		try
		{
			string text = "";
			text = ((!UsePublicFont) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string[] files = Directory.GetFiles(text, "*.onnx", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				if (System.IO.Path.GetFileName(files[i]) == onnxfile)
				{
					return i + 1;
				}
			}
			return 0;
		}
		catch (Exception ex)
		{
			string text2 = "FTW07-FontToolWindow-GetAIModelIndex()";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text2 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetAIModelName(int index)
	{
		try
		{
			index--;
			string text = "";
			text = ((!UsePublicFont) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string[] files = Directory.GetFiles(text, "*.onnx", SearchOption.TopDirectoryOnly);
			if (index >= 0 && index < files.Length)
			{
				return System.IO.Path.GetFileName(files[index]);
			}
			return "";
		}
		catch (Exception ex)
		{
			string text2 = "FTW07-FontToolWindow-GetAIModelName()";
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text2 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
			return "";
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
			20 => inspection.fontTool.FontMatOther[0][0], 
			21 => inspection.fontTool.FontMatOther[1][0], 
			22 => inspection.fontTool.FontMatOther[2][0], 
			23 => inspection.fontTool.FontMatOther[3][0], 
			24 => inspection.fontTool.FontMatOther[4][0], 
			25 => inspection.fontTool.FontMatOther[5][0], 
			26 => inspection.fontTool.FontMatOther[6][0], 
			27 => inspection.fontTool.FontMatOther[7][0], 
			28 => inspection.fontTool.FontMatOther[8][0], 
			29 => inspection.fontTool.FontMatOther[9][0], 
			30 => inspection.fontTool.FontMatOther[10][0], 
			31 => inspection.fontTool.FontMatOther[11][0], 
			32 => inspection.fontTool.FontMatOther[12][0], 
			33 => inspection.fontTool.FontMatOther[13][0], 
			34 => inspection.fontTool.FontMatOther[14][0], 
			35 => inspection.fontTool.FontMatOther[15][0], 
			36 => inspection.fontTool.FontMatOther[16][0], 
			_ => inspection.fontTool.FontMat0[0], 
		};
	}

	private void Cb_PatternSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			int selectedIndex = cb_PatternSelect.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < 210)
			{
				string text = UpdatePatternSelectImage(selectedIndex);
				txt_InputText.Text = text;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string UpdatePatternSelectImage(int index)
	{
		try
		{
			if (!PDBMat[index].Empty())
			{
				img_Pattern.Source = PDBMat[index].ToBitmapSource();
				string patternName = PatternDB[index].PatternName;
				inspection.paramFile.FontToolParam.PatternChannel = PatternDB[index].PatternChannel;
				patternName.Replace(" ", "");
				if (patternName.Length > 5)
				{
					patternName.Substring(0, 5);
				}
				return patternName;
			}
			return "";
		}
		catch (Exception ex)
		{
			string text = "FTW07-FontToolWindow-UpdatePatternSelectImage()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return "";
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddImage2DB(int id, bool result)
	{
		string text = "OK";
		if (!result)
		{
			text = "NG";
		}
		Mat mat = inspection.fontTool.FoundFontMat[id].Clone();
		if (mat.Width <= 0 && mat.Height <= 0)
		{
			if (ShowMessage)
			{
				MessageBox.Show("影像尺寸錯誤，存檔取消", "錯誤");
			}
		}
		else
		{
			if (FontsResult[id] == result && MessageBox.Show(string.Format("字元{0}測試結果已經是{1}，是否要再將此影像放進{1}資料庫?", BtnFonts[id].Content.ToString(), text), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
			{
				return;
			}
			string text2 = string.Format("D:\\AIImage\\AI_Font\\TrainingImage\\{0}\\{1}\\", BtnFonts[id].Content.ToString(), text);
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			if (!Directory.Exists(text2))
			{
				if (ShowMessage)
				{
					MessageBox.Show("存檔路徑不合法", "錯誤");
				}
				return;
			}
			DateTime now = DateTime.Now;
			string fileName = string.Format("{0}{1}-{2}-{3}.bmp", text2, BtnFonts[id].Content.ToString(), text, now.ToString("yyyy-MM-dd-HH-mm-ss"));
			if (chk_FontAlignVertical.IsChecked == true)
			{
				Cv2.Transpose(mat, mat);
				Cv2.Flip(mat, mat, FlipMode.Y);
			}
			Cv2.ImWrite(fileName, mat);
			string str = string.Format("[{0}資料庫] 字元[{1}]影像儲存成功!", text, BtnFonts[id].Content.ToString());
			ShowLabMessage(str);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_QuickAdd_1_Click(object sender, RoutedEventArgs e)
	{
		if (!LoginSuccess)
		{
			PasswordWindow passwordWindow = new PasswordWindow();
			passwordWindow.SetPassWord(ThroughputFile.param.AIPassword);
			passwordWindow.ShowDialog();
			if (!passwordWindow.GetLoginStatus())
			{
				UpdateUI = false;
				tab_Function.SelectedIndex = 0;
				UpdateUI = true;
				return;
			}
			LoginSuccess = true;
		}
		try
		{
			int num = Convert.ToInt32(((Button)sender).Uid);
			if (FontsResult[num])
			{
				AddImage2DB(num, result: false);
			}
			else
			{
				AddImage2DB(num, result: true);
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-FontToolWindow-Btn_QuickAdd_1_Click()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	private void Img_pbd_Drop(object sender, DragEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Test0_Click(object sender, RoutedEventArgs e)
	{
		string text = "";
		string text2 = txt_font.Text;
		string modelname = string.Format("{0}.onnx", text2);
		text = ((chk_CheckOK.IsChecked != true) ? string.Format("D:\\AIImage\\FontDataBase\\{0}\\NG\\", text2) : string.Format("D:\\AIImage\\FontDataBase\\{0}\\OK\\", text2));
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string[] files = Directory.GetFiles(text, "*.bmp", SearchOption.AllDirectories);
		if (files.Length == 0)
		{
			MessageBox.Show(string.Format("{0}找不到任何影像", text), "警告");
			return;
		}
		string text3 = "d:\\AIImage\\FontDataBase\\Output\\OK\\";
		string text4 = "d:\\AIImage\\FontDataBase\\Output\\NG\\";
		try
		{
			if (chk_CheckOK.IsChecked == true)
			{
				if (Directory.Exists(text3))
				{
					Directory.Delete(text3, recursive: true);
					Directory.CreateDirectory(text3);
				}
				else
				{
					Directory.CreateDirectory(text3);
				}
			}
			else if (Directory.Exists(text4))
			{
				Directory.Delete(text4, recursive: true);
				Directory.CreateDirectory(text4);
			}
			else
			{
				Directory.CreateDirectory(text4);
			}
		}
		catch
		{
		}
		Mat[] pattern = new Mat[5];
		for (int i = 0; i < 5; i++)
		{
			pattern[i] = new Mat();
		}
		inspection.fontTool.GetFontPattern(inspection.fontTool.GetFontIndex(text2), ref pattern);
		Mat mat = new Mat();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < files.Length; j++)
		{
			num4 = 0;
			num3 = 0;
			mat = CVBuffer.ImRead(files[j], ImreadModes.Grayscale);
			for (int k = 0; k < 5; k++)
			{
				inspection.fontTool.GetAIMatchScore(modelname, 5, pattern[k].Clone(), mat, ref num3);
				if (num3 > num4)
				{
					num4 = num3;
				}
			}
			num2++;
			if (chk_CheckOK.IsChecked == true)
			{
				if (num4 < Convert.ToInt32(txt_AcceptScoreFont2.Text))
				{
					Cv2.ImWrite(string.Format("{0}{1}.bmp", text3, System.IO.Path.GetFileNameWithoutExtension(files[j])), mat);
					num++;
				}
			}
			else if (num4 >= Convert.ToInt32(txt_AcceptScoreFont2.Text))
			{
				Cv2.ImWrite(string.Format("{0}{1}.bmp", text4, System.IO.Path.GetFileNameWithoutExtension(files[j])), mat);
				num++;
			}
		}
		double num5 = (double)num / (double)num2;
		if (num2 > 0)
		{
			txt_AllResult.Text = string.Format("NG = {0}, Total = {1}, yield = {2:N3}", num, num2, num5);
		}
	}

	private void Btn_Test_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		grd_EngTest.Visibility = Visibility.Visible;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_R1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("R_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("X_", button.Uid);
		}
		else
		{
			RemoveFontImage("Q_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_M1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("M_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("B_", button.Uid);
		}
		else
		{
			RemoveFontImage("S_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_C1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("C_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("E_", button.Uid);
		}
		else
		{
			RemoveFontImage("T_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_U1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("U_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("D_", button.Uid);
		}
		else
		{
			RemoveFontImage("V_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_Dash1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("-_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("A_", button.Uid);
		}
		else
		{
			RemoveFontImage("W_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_01_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("0_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("F_", button.Uid);
		}
		else
		{
			RemoveFontImage("Y_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_11_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("1_", button.Uid);
		}
		else if (FontPageIndex == 1)
		{
			RemoveFontImage("G_", button.Uid);
		}
		else
		{
			RemoveFontImage("Z_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_21_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("2_", button.Uid);
		}
		else
		{
			RemoveFontImage("H_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_31_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("3_", button.Uid);
		}
		else
		{
			RemoveFontImage("I_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_41_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("4_", button.Uid);
		}
		else
		{
			RemoveFontImage("J_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_51_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("5_", button.Uid);
		}
		else
		{
			RemoveFontImage("K_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_61_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("6_", button.Uid);
		}
		else
		{
			RemoveFontImage("L_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_71_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("7_", button.Uid);
		}
		else
		{
			RemoveFontImage("N_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_81_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("8_", button.Uid);
		}
		else
		{
			RemoveFontImage("O_", button.Uid);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Font_91_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		if (FontPageIndex == 0)
		{
			RemoveFontImage("9_", button.Uid);
		}
		else
		{
			RemoveFontImage("Q_", button.Uid);
		}
	}

	private void Rad_FDByRecipe_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam();
			UsePublicFont = false;
			inspection.fontTool.UpdateFontPattern();
			for (int i = 0; i <= 5; i++)
			{
				inspection.fontTool.FontModelReaded[i] = false;
				inspection.fontTool.FontModelOnnxType[i] = 0;
			}
			UpdateAIModelComboBox(cb_AIModelFont1);
			UpdateAIModelComboBox(cb_AIModelFont2);
			UpdateAIModelComboBox(cb_AIModelFont3);
			UpdateAIModelComboBox(cb_AIModelFont4);
			UpdateAIModelComboBox(cb_AIModelFont5);
			UpdateFontPattern();
		}
	}

	private void Rad_FDByPublic_Checked(object sender, RoutedEventArgs e)
	{
		if (UpdateUI)
		{
			WriteUIValueToParam();
			UsePublicFont = true;
			inspection.fontTool.UpdateFontPattern();
			for (int i = 0; i <= 5; i++)
			{
				inspection.fontTool.FontModelReaded[i] = false;
				inspection.fontTool.FontModelOnnxType[i] = 0;
			}
			UpdateAIModelComboBox(cb_AIModelFont1);
			UpdateAIModelComboBox(cb_AIModelFont2);
			UpdateAIModelComboBox(cb_AIModelFont3);
			UpdateAIModelComboBox(cb_AIModelFont4);
			UpdateAIModelComboBox(cb_AIModelFont5);
			UpdateFontPattern();
		}
	}

	private void Btn_Open_Click(object sender, RoutedEventArgs e)
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (folderSelectDialog.ShowDialog())
		{
			CheckFileList(folderSelectDialog.FileName);
			btn_prev.IsEnabled = true;
			btn_next.IsEnabled = true;
			btn_AutoTest.IsEnabled = true;
			btn_Open.Background = Brushes.LightGoldenrodYellow;
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
			FontTestOKCount = 0;
			FontTestNGCount = 0;
			FontTestError_AOI = 0;
			FontTestError_OCR = 0;
			FontTestError_Check = 0;
			FontTestError_AI = 0;
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				string extension = System.IO.Path.GetExtension(path);
				if (extension.ToLower() != ".bmp" && extension.ToLower() != ".jpg" && extension.ToLower() != ".png")
				{
					MessageBox.Show("不支援此種格式。", "警告");
					return;
				}
				CurrentImg = path;
				CurrentMat = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale).Clone();
				TestFontInspect(CurrentMat);
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
			filelistTest = Directory.GetFiles(path, "*.bmp", SearchOption.AllDirectories);
			if (filelistTest.Length != 0)
			{
				filelistTestIndex = 0;
				MaxfilelistTestCount = filelistTest.Length;
				CurrentImg = filelistTest[filelistTestIndex];
				CurrentMat = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale).Clone();
				TestFontInspect(CurrentMat);
			}
			else
			{
				txt_TestFontResult.Text = "開啟資料夾內找不到任何影像。";
				filelistTestIndex = -1;
				MaxfilelistTestCount = 0;
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-FontToolWindow-CheckFileList()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	private void Btn_fonttest_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentMat.Width > 0 && CurrentMat.Height > 0)
		{
			TestFontInspect(CurrentMat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool TestFontInspect(Mat mat)
	{
		try
		{
			Mat fontImageFromIndex = GetFontImageFromIndex(cb_FontSelectTest.SelectedIndex);
			if (mat.Width <= 0 || mat.Height <= 0 || mat.Channels() != 1)
			{
				txt_TestFontResult.Text = "讀取影像異常。";
				return false;
			}
			if (fontImageFromIndex.Width <= 0 || fontImageFromIndex.Height <= 0 || fontImageFromIndex.Channels() != 1)
			{
				txt_TestFontResult.Text = "尚未設定該字元 (字元資料庫)。";
				return false;
			}
			Mat mat2 = mat.Clone();
			Mat mat3 = fontImageFromIndex.Clone();
			if (chk_FontAlignVertical.IsChecked == true)
			{
				Cv2.Transpose(mat3, mat3);
				Cv2.Flip(mat3, mat3, FlipMode.Y);
			}
			img_TestFont.Source = mat2.ToBitmapSource();
			img_TestFontPattern.Source = mat3.ToBitmapSource();
			string str = "";
			bool flag = true;
			string fontCharFromIndex = inspection.fontTool.GetFontCharFromIndex(cb_FontSelectTest.SelectedIndex);
			if (chk_TestFont_OCR.IsChecked == true)
			{
				string ocrChar = "";
				bool aIOCRResult = inspection.fontTool.GetAIOCRResult(mat2, ref ocrChar);
				bool flag2 = false;
				if (ocrChar == "1" && fontCharFromIndex == "-")
				{
					flag2 = true;
					lab_FontOCRResult.Content = ocrChar;
				}
				else if (ocrChar == "-" && fontCharFromIndex == "1")
				{
					flag2 = true;
					lab_FontOCRResult.Content = ocrChar;
				}
				else if (ocrChar == "7" && fontCharFromIndex == "1")
				{
					Mat fontImageFromIndex2 = GetFontImageFromIndex(12);
					if (fontImageFromIndex2.Width <= 0 || fontImageFromIndex2.Height <= 0)
					{
						flag2 = true;
						ocrChar = fontCharFromIndex;
					}
					else if (Math.Abs(mat2.Width - mat3.Width) < Math.Abs(fontImageFromIndex2.Width - mat3.Width))
					{
						flag2 = true;
						ocrChar = fontCharFromIndex;
					}
					else
					{
						flag2 = false;
					}
					lab_FontOCRResult.Content = ocrChar;
				}
				else if (ocrChar == "1" && fontCharFromIndex == "7")
				{
					Mat fontImageFromIndex3 = GetFontImageFromIndex(6);
					if (fontImageFromIndex3.Width <= 0 || fontImageFromIndex3.Height <= 0)
					{
						flag2 = true;
						ocrChar = fontCharFromIndex;
					}
					else if (Math.Abs(mat2.Width - mat3.Width) < Math.Abs(fontImageFromIndex3.Width - mat3.Width))
					{
						flag2 = true;
						ocrChar = fontCharFromIndex;
					}
					else
					{
						flag2 = false;
					}
					lab_FontOCRResult.Content = ocrChar;
				}
				else
				{
					lab_FontOCRResult.Content = ocrChar;
					flag2 = aIOCRResult;
				}
				if (!flag2)
				{
					lab_FontOCRResult.Content = "OCR失敗";
					flag = false;
					FontTestError_OCR++;
				}
				lab_FontOCRResult.Background = Brushes.Aqua;
				if (ocrChar != fontCharFromIndex)
				{
					lab_FontOCRResult.Background = Brushes.Fuchsia;
					flag = false;
				}
			}
			if (chk_TestFont_CheckFont.IsChecked == true)
			{
				Mat fontbinary = new Mat();
				if (inspection.fontTool.Check_Font(cb_FontSelectTest.SelectedIndex, mat2, mat3, ref str, ref fontbinary))
				{
					lab_FontTestResult.Content = "OK";
					lab_FontTestResult.Background = Brushes.LimeGreen;
				}
				else
				{
					lab_FontTestResult.Content = "NG";
					lab_FontTestResult.Background = Brushes.Fuchsia;
					flag = false;
					FontTestError_Check++;
				}
			}
			if (chk_TestFont_AI.IsChecked == true)
			{
				string modelname = string.Format("{0}.onnx", fontCharFromIndex);
				int Score = 0;
				bool aIMatchScore = inspection.fontTool.GetAIMatchScore(modelname, 5, mat2.Clone(), mat3, ref Score);
				lab_FontAIResult.Content = Score;
				lab_FontAIResult.Background = Brushes.MediumPurple;
				if ((Score < Convert.ToInt32(TxtAcceptScore[0].Text)) & aIMatchScore)
				{
					lab_FontAIResult.Background = Brushes.Fuchsia;
					flag = false;
					FontTestError_AI++;
				}
			}
			if (chk_TestFont_AOI.IsChecked == true)
			{
				OpenCvSharp.Rect rt = new OpenCvSharp.Rect(0, 0, 0, 0);
				if (mat2.Width != mat3.Width || mat2.Height != mat3.Height)
				{
					mat2 = mat2.Resize(new OpenCvSharp.Size(mat3.Width, mat3.Height));
				}
				int openCVMatchScore = inspection.fontTool.GetOpenCVMatchScore(mat2, mat3, out rt);
				lab_FontTestResultScore.Content = openCVMatchScore;
				lab_FontTestResultScore.Background = Brushes.LightGreen;
				if (openCVMatchScore < Convert.ToInt32(TxtAcceptScore[0].Text))
				{
					lab_FontTestResultScore.Background = Brushes.Fuchsia;
					flag = false;
					FontTestError_AOI++;
				}
			}
			txt_TestFontResult.Text = str;
			txt_FontCount.Text = string.Format("{0} / {1}", filelistTestIndex + 1, MaxfilelistTestCount);
			if (flag)
			{
				FontTestOKCount++;
			}
			else
			{
				FontTestNGCount++;
			}
			lab_OKCount.Content = string.Format("OK: {0}", FontTestOKCount);
			lab_NGCount.Content = string.Format("NG: {0}", FontTestNGCount);
			double num = (double)FontTestOKCount / (double)(FontTestOKCount + FontTestNGCount) * 100.0;
			lab_Yield.Content = string.Format("良率: {0:N1} %", num);
			return flag;
		}
		catch (Exception ex)
		{
			string text = "TW01-FontToolWindow-TestFontInspect()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_prev_Click(object sender, RoutedEventArgs e)
	{
		if (filelistTestIndex == -1 || MaxfilelistTestCount == 0 || filelistTest == null)
		{
			return;
		}
		filelistTestIndex--;
		if (filelistTestIndex < 0)
		{
			filelistTestIndex = MaxfilelistTestCount - 1;
		}
		CurrentImg = filelistTest[filelistTestIndex];
		if (File.Exists(CurrentImg))
		{
			string extension = System.IO.Path.GetExtension(CurrentImg);
			if (extension.ToLower() != ".bmp" && extension.ToLower() != ".jpg")
			{
				MessageBox.Show("不支援此種格式。", "警告");
				return;
			}
			CurrentMat = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale).Clone();
			TestFontInspect(CurrentMat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_next_Click(object sender, RoutedEventArgs e)
	{
		if (filelistTestIndex == -1 || MaxfilelistTestCount == 0 || filelistTest == null)
		{
			return;
		}
		filelistTestIndex++;
		if (filelistTestIndex >= MaxfilelistTestCount)
		{
			filelistTestIndex = 0;
			FontTestOKCount = 0;
			FontTestNGCount = 0;
		}
		CurrentImg = filelistTest[filelistTestIndex];
		if (File.Exists(CurrentImg))
		{
			string extension = System.IO.Path.GetExtension(CurrentImg);
			if (extension.ToLower() != ".bmp" && extension.ToLower() != ".jpg")
			{
				MessageBox.Show("不支援此種格式。", "警告");
				return;
			}
			CurrentMat = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale).Clone();
			TestFontInspect(CurrentMat);
		}
	}

	private void Btn_ShowCheckFontTool_Click(object sender, RoutedEventArgs e)
	{
		if (grd_FontTest.Visibility == Visibility.Hidden)
		{
			grd_FontTest.Visibility = Visibility.Visible;
		}
		else
		{
			grd_FontTest.Visibility = Visibility.Hidden;
		}
		if (inspection.fontTool.OcrMat1[0].Width > 0 && inspection.fontTool.OcrMat1[0].Height > 0)
		{
			btn_ImageF1.IsEnabled = true;
		}
		else
		{
			btn_ImageF1.IsEnabled = false;
		}
		if (inspection.fontTool.OcrMat1[1].Width > 0 && inspection.fontTool.OcrMat1[1].Height > 0)
		{
			btn_ImageF2.IsEnabled = true;
		}
		else
		{
			btn_ImageF2.IsEnabled = false;
		}
		if (inspection.fontTool.OcrMat1[2].Width > 0 && inspection.fontTool.OcrMat1[2].Height > 0)
		{
			btn_ImageF3.IsEnabled = true;
		}
		else
		{
			btn_ImageF3.IsEnabled = false;
		}
		if (inspection.fontTool.OcrMat1[3].Width > 0 && inspection.fontTool.OcrMat1[3].Height > 0)
		{
			btn_ImageF4.IsEnabled = true;
		}
		else
		{
			btn_ImageF4.IsEnabled = false;
		}
		if (inspection.fontTool.OcrMat1[4].Width > 0 && inspection.fontTool.OcrMat1[4].Height > 0)
		{
			btn_ImageF5.IsEnabled = true;
		}
		else
		{
			btn_ImageF5.IsEnabled = false;
		}
	}

	private void TimeStart()
	{
		sw.Start();
	}

	private double TimeStop()
	{
		sw.Stop();
		double totalMilliseconds = sw.Elapsed.TotalMilliseconds;
		sw.Reset();
		return totalMilliseconds;
	}

	private void Btn_SaveFontCheckSetup_Click(object sender, RoutedEventArgs e)
	{
		SaveFontSetup();
		grd_FontSetup.Visibility = Visibility.Hidden;
		Btn_Test_Click(null, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_R_Setup_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		FontSetupChar = button.Uid;
		if (FontSetupChar == "")
		{
			return;
		}
		if (button.Name == "btn_Font_1")
		{
			FontClick = 0;
		}
		else if (button.Name == "btn_Font_2")
		{
			FontClick = 1;
		}
		else if (button.Name == "btn_Font_3")
		{
			FontClick = 2;
		}
		else if (button.Name == "btn_Font_4")
		{
			FontClick = 3;
		}
		else if (button.Name == "btn_Font_5")
		{
			FontClick = 4;
		}
		else if (button.Name == "btn_Font_6")
		{
			FontClick = 5;
		}
		else
		{
			FontClick = -1;
		}
		if (FontSetupChar == "Connect")
		{
			lab_FontSetup.Content = "黏";
		}
		else
		{
			lab_FontSetup.Content = FontSetupChar;
		}
		if (FontClick == 5)
		{
			if (CurrentImg == "" || !File.Exists(CurrentImg))
			{
				return;
			}
			FontSetupChar = inspection.fontTool.GetFontCharFromIndex(cb_FontSelectTest.SelectedIndex);
		}
		SetFontSetup();
		BinaryShiftPreview();
		grd_FontSetup.Visibility = Visibility.Visible;
	}

	private void ShowLabMessage(string str)
	{
		lab_Message.Visibility = Visibility.Visible;
		DoubleAnimation doubleAnimation = new DoubleAnimation();
		doubleAnimation.From = 0.0;
		doubleAnimation.To = 1.0;
		doubleAnimation.AutoReverse = true;
		doubleAnimation.RepeatBehavior = new RepeatBehavior(1.0);
		lab_Message.Content = str;
		doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.0));
		lab_Message.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetFontSetup()
	{
		lab_FontSetupMain.Content = "字   元   分   析   設   定";
		img_BinaryShift.Visibility = Visibility.Hidden;
		cb_MaskSelect.Visibility = Visibility.Hidden;
		lab_MaskSelect.Visibility = Visibility.Hidden;
		img_FontBinaryShift.Visibility = Visibility.Visible;
		img_PatternBinaryShift.Visibility = Visibility.Visible;
		lab_FontImage1.Visibility = Visibility.Visible;
		lab_FontImage2.Visibility = Visibility.Visible;
		txt_MinMatchScore.Visibility = Visibility.Hidden;
		lab_MinMatchScore.Visibility = Visibility.Hidden;
		for (int i = 0; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Visible;
		}
		SetupFontSetup(FontSetupChar);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupFontSetup(string str)
	{
		chk_UseFontDBMask.Visibility = Visibility.Visible;
		chk_UseMorphFirst.Visibility = Visibility.Visible;
		if (str != null)
		{
			switch (str.Length)
			{
			case 1:
				switch (str[0])
				{
				case '0':
					Setup0Check();
					return;
				case 'R':
					SetupRCheck();
					return;
				case 'M':
					SetupMCheck();
					return;
				case 'C':
					SetupCCheck();
					return;
				case 'U':
					SetupUCheck();
					return;
				case 'X':
					SetupXCheck();
					return;
				case 'B':
					SetupBCheck();
					return;
				case 'D':
					SetupDCheck();
					return;
				case 'A':
					SetupACheck();
					return;
				case 'E':
					SetupECheck();
					return;
				case '-':
					SetupDashCheck();
					return;
				case '1':
					Setup1Check();
					return;
				case '2':
					Setup2Check();
					return;
				case '3':
					Setup3Check();
					return;
				case '4':
					Setup4Check();
					return;
				case '5':
					Setup5Check();
					return;
				case '6':
					Setup6Check();
					return;
				case '7':
					Setup7Check();
					return;
				case '8':
					Setup8Check();
					return;
				case '9':
					Setup9Check();
					return;
				}
				break;
			case 7:
				if (!(str == "Connect"))
				{
					break;
				}
				chk_UseFontDBMask.Visibility = Visibility.Hidden;
				chk_UseMorphFirst.Visibility = Visibility.Hidden;
				SetupConnectCheck();
				return;
			}
		}
		int num = inspection.fontTool.GetFontIndex(str) - 20;
		if (num < 0 || num >= 20)
		{
			num = 0;
		}
		SetupOtherCheck(str, num);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SaveFontSetup()
	{
		string fontSetupChar = FontSetupChar;
		if (fontSetupChar != null)
		{
			switch (fontSetupChar.Length)
			{
			case 1:
				switch (fontSetupChar[0])
				{
				case '0':
					SaveCheck_0();
					return;
				case 'R':
					SaveCheck_R();
					return;
				case 'M':
					SaveCheck_M();
					return;
				case 'C':
					SaveCheck_C();
					return;
				case 'U':
					SaveCheck_U();
					return;
				case 'X':
					SaveCheck_X();
					return;
				case 'B':
					SaveCheck_B();
					return;
				case 'D':
					SaveCheck_D();
					return;
				case 'A':
					SaveCheck_A();
					return;
				case 'E':
					SaveCheck_E();
					return;
				case '-':
					SaveCheck_Dash();
					return;
				case '1':
					SaveCheck_1();
					return;
				case '2':
					SaveCheck_2();
					return;
				case '3':
					SaveCheck_3();
					return;
				case '4':
					SaveCheck_4();
					return;
				case '5':
					SaveCheck_5();
					return;
				case '6':
					SaveCheck_6();
					return;
				case '7':
					SaveCheck_7();
					return;
				case '8':
					SaveCheck_8();
					return;
				case '9':
					SaveCheck_9();
					return;
				}
				break;
			case 7:
				if (!(fontSetupChar == "Connect"))
				{
					break;
				}
				SaveCheck_Connect();
				return;
			}
		}
		int num = inspection.fontTool.GetFontIndex(FontSetupChar) - 20;
		if (num < 0 || num >= 20)
		{
			num = 0;
		}
		SaveCheck_OtherFont(FontSetupChar, num);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupRCheck()
	{
		FontCheckSetup[0].Content = "R-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "R-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "R-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "R-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "R-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "R-06 檢查面積異常";
		FontCheckSetup[6].Content = "R-07 檢查上方寬度異常";
		FontCheckSetup[7].Content = "R-08 檢查下方寬度異常";
		FontCheckSetup[8].Content = "R-09 檢查左邊高度異常";
		FontCheckSetup[9].Content = "R-10 檢查右邊高度異常";
		FontCheckSetup[10].Content = "R-11 檢查中心異常";
		FontCheckSetup[11].Content = "R-12 檢查中心高度異常";
		FontCheckSetup[12].Content = "R-13 檢查左右邊高度比例異常";
		FontCheckSetup[13].Content = "R-14 檢查斷字異常";
		FontCheckSetup[14].Content = "R-15 檢查右下角高度異常";
		FontCheckSetup[15].Visibility = Visibility.Hidden;
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.R_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.R_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.R_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.R_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.R_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.R_Area;
		FontCheckSetup[6].IsChecked = fontToolParam.R_T_W;
		FontCheckSetup[7].IsChecked = fontToolParam.R_B_W;
		FontCheckSetup[8].IsChecked = fontToolParam.R_L_H;
		FontCheckSetup[9].IsChecked = fontToolParam.R_R_H;
		FontCheckSetup[10].IsChecked = fontToolParam.R_C;
		FontCheckSetup[11].IsChecked = fontToolParam.R_C_H_MIN;
		FontCheckSetup[12].IsChecked = fontToolParam.R_LR_H_Diff;
		FontCheckSetup[13].IsChecked = fontToolParam.R_Break;
		FontCheckSetup[14].IsChecked = fontToolParam.R_RB_H;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.R_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.R_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.R_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.R_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.R_OpenCloseCount2;
		chk_UseFontDBMask.IsChecked = fontToolParam.R_UseFontDBMask;
		chk_UseMorphFirst.IsChecked = fontToolParam.R_UseMorphFirst;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupMCheck()
	{
		FontCheckSetup[0].Content = "M-01 檢查上方寬度異常";
		FontCheckSetup[1].Content = "M-02 檢查下方寬度異常";
		FontCheckSetup[2].Content = "M-03 檢查寬度異常";
		FontCheckSetup[3].Content = "M-04 檢查斷字異常";
		FontCheckSetup[4].Content = "M-05 檢查第1隻腳高度異常";
		FontCheckSetup[5].Content = "M-06 檢查第2隻腳高度異常";
		FontCheckSetup[6].Content = "M-07 檢查第3隻腳高度異常";
		FontCheckSetup[7].Content = "M-08 檢查文字沾黏異常";
		for (int i = 8; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.M_T_W;
		FontCheckSetup[1].IsChecked = fontToolParam.M_B_W;
		FontCheckSetup[2].IsChecked = fontToolParam.M_W;
		FontCheckSetup[3].IsChecked = fontToolParam.M_Break;
		FontCheckSetup[4].IsChecked = fontToolParam.M_Leg1;
		FontCheckSetup[5].IsChecked = fontToolParam.M_Leg2;
		FontCheckSetup[6].IsChecked = fontToolParam.M_Leg3;
		FontCheckSetup[7].IsChecked = fontToolParam.M_Connected;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.M_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.M_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.M_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.M_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.M_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.M_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.M_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupCCheck()
	{
		FontCheckSetup[0].Content = "C-01 檢查寬度異常";
		FontCheckSetup[1].Content = "C-02 檢查高度異常";
		FontCheckSetup[2].Content = "C-03 檢查上方寬度異常";
		FontCheckSetup[3].Content = "C-04 檢查下方寬度異常";
		FontCheckSetup[4].Content = "C-05 檢查左邊高度異常";
		FontCheckSetup[5].Content = "C-06 檢查右邊高度異常";
		FontCheckSetup[6].Content = "C-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.C_W;
		FontCheckSetup[1].IsChecked = fontToolParam.C_H;
		FontCheckSetup[2].IsChecked = fontToolParam.C_T_W;
		FontCheckSetup[3].IsChecked = fontToolParam.C_B_W;
		FontCheckSetup[4].IsChecked = fontToolParam.C_L_H;
		FontCheckSetup[4].IsChecked = fontToolParam.C_L_H;
		FontCheckSetup[5].IsChecked = fontToolParam.C_R_H;
		FontCheckSetup[6].IsChecked = fontToolParam.C_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.C_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.C_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.C_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.C_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.C_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.C_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.C_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupUCheck()
	{
		FontCheckSetup[0].Content = "U-01 檢查寬度異常";
		FontCheckSetup[1].Content = "U-02 檢查高度異常";
		FontCheckSetup[2].Content = "U-03 檢查上方寬度異常";
		FontCheckSetup[3].Content = "U-04 檢查下方寬度異常";
		FontCheckSetup[4].Content = "U-05 檢查左邊高度異常";
		FontCheckSetup[5].Content = "U-06 檢查右邊高度異常";
		FontCheckSetup[6].Content = "U-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.U_W;
		FontCheckSetup[1].IsChecked = fontToolParam.U_H;
		FontCheckSetup[2].IsChecked = fontToolParam.U_T_W;
		FontCheckSetup[3].IsChecked = fontToolParam.U_B_W;
		FontCheckSetup[4].IsChecked = fontToolParam.U_L_H;
		FontCheckSetup[4].IsChecked = fontToolParam.U_L_H;
		FontCheckSetup[5].IsChecked = fontToolParam.U_R_H;
		FontCheckSetup[6].IsChecked = fontToolParam.U_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.U_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.U_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.U_OpenCloseCount;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		cb_OpenClose2.SelectedIndex = fontToolParam.U_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.U_OpenCloseCount2;
		chk_UseMorphFirst.IsChecked = fontToolParam.U_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.U_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupXCheck()
	{
		FontCheckSetup[0].Content = "X-01 檢查寬度異常";
		FontCheckSetup[1].Content = "X-02 檢查高度異常";
		FontCheckSetup[2].Content = "X-03 檢查上方寬度異常";
		FontCheckSetup[3].Content = "X-04 檢查下方寬度異常";
		FontCheckSetup[4].Content = "X-05 檢查左邊高度異常";
		FontCheckSetup[5].Content = "X-06 檢查右邊高度異常";
		FontCheckSetup[6].Content = "X-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.X_W;
		FontCheckSetup[1].IsChecked = fontToolParam.X_H;
		FontCheckSetup[2].IsChecked = fontToolParam.X_T_W;
		FontCheckSetup[3].IsChecked = fontToolParam.X_B_W;
		FontCheckSetup[4].IsChecked = fontToolParam.X_L_H;
		FontCheckSetup[4].IsChecked = fontToolParam.X_L_H;
		FontCheckSetup[5].IsChecked = fontToolParam.X_R_H;
		FontCheckSetup[6].IsChecked = fontToolParam.X_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.X_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.X_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.X_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.X_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.X_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.X_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.X_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupBCheck()
	{
		FontCheckSetup[0].Content = "B-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "B-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "B-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "B-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "B-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "B-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "B-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "B-0B 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "B-09 檢查下方寬度異常";
		FontCheckSetup[9].Content = "B-10 檢查中心異常";
		FontCheckSetup[10].Content = "B-11 檢查中心高度過短";
		FontCheckSetup[11].Content = "B-12 檢查字元中心高度過長";
		FontCheckSetup[12].Content = "B-13 檢查斷字異常";
		for (int i = 13; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.TB_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.TB_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.TB_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.TB_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.TB_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.TB_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.TB_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.TB_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.TB_B_W;
		FontCheckSetup[9].IsChecked = fontToolParam.TB_C;
		FontCheckSetup[10].IsChecked = fontToolParam.TB_C_H_MIN;
		FontCheckSetup[11].IsChecked = fontToolParam.TB_C_H_MAX;
		FontCheckSetup[12].IsChecked = fontToolParam.TB_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.TB_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.TB_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.TB_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.TB_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.TB_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.TB_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.TB_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupECheck()
	{
		FontCheckSetup[0].Content = "E-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "E-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "E-0E 檢查右上角高度異常";
		FontCheckSetup[3].Content = "E-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "E-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "E-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "E-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "E-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "E-09 檢查斷字異常";
		for (int i = 9; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.TE_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.TE_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.TE_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.TE_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.TE_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.TE_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.TE_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.TE_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.TE_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.TE_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.TE_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.TE_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.TE_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.TE_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.TE_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.TE_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupDashCheck()
	{
		FontCheckSetup[0].Content = "DA-01 檢查高度異常";
		FontCheckSetup[1].Content = "DA-02 檢查寬度異常";
		FontCheckSetup[2].Content = "DA-03 檢查斷字異常";
		for (int i = 3; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.Dash_H;
		FontCheckSetup[1].IsChecked = fontToolParam.Dash_W;
		FontCheckSetup[2].IsChecked = fontToolParam.Dash_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.Dash_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.Dash_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.Dash_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.Dash_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.Dash_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.Dash_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.Dash_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupDCheck()
	{
		FontCheckSetup[0].Content = "D-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "D-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "D-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "D-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "D-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "D-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "D-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "D-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "D-09 檢查上方寬度異常";
		FontCheckSetup[9].Content = "D-10 檢查下方寬度異常";
		FontCheckSetup[10].Content = "D-11 檢查中心異常";
		FontCheckSetup[11].Content = "D-12 檢查中心高度過短";
		FontCheckSetup[12].Content = "D-13 檢查字元中心高度過長";
		FontCheckSetup[13].Content = "D-14 檢查斷字異常";
		FontCheckSetup[14].Visibility = Visibility.Hidden;
		FontCheckSetup[15].Visibility = Visibility.Hidden;
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.D_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.D_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.D_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.D_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.D_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.D_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.D_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.D_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.D_T_W;
		FontCheckSetup[9].IsChecked = fontToolParam.D_B_W;
		FontCheckSetup[10].IsChecked = fontToolParam.D_C;
		FontCheckSetup[11].IsChecked = fontToolParam.D_C_H_MIN;
		FontCheckSetup[12].IsChecked = fontToolParam.D_C_H_MAX;
		FontCheckSetup[13].IsChecked = fontToolParam.D_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.D_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.D_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.D_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.D_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.D_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.D_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.D_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupACheck()
	{
		FontCheckSetup[0].Content = "A-01 檢查寬度異常";
		FontCheckSetup[1].Content = "A-02 檢查高度異常";
		FontCheckSetup[2].Content = "A-03 檢查上方寬度異常";
		FontCheckSetup[3].Content = "A-04 檢查下方寬度異常";
		FontCheckSetup[4].Content = "A-05 檢查左邊高度異常";
		FontCheckSetup[5].Content = "A-06 檢查右邊高度異常";
		FontCheckSetup[6].Content = "A-07 檢查斷字異常";
		for (int i = 7; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.A_W;
		FontCheckSetup[1].IsChecked = fontToolParam.A_H;
		FontCheckSetup[2].IsChecked = fontToolParam.A_T_W;
		FontCheckSetup[3].IsChecked = fontToolParam.A_B_W;
		FontCheckSetup[4].IsChecked = fontToolParam.A_L_H;
		FontCheckSetup[4].IsChecked = fontToolParam.A_L_H;
		FontCheckSetup[5].IsChecked = fontToolParam.A_R_H;
		FontCheckSetup[6].IsChecked = fontToolParam.A_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.A_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.A_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.A_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.A_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.A_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.A_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.A_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup0Check()
	{
		FontCheckSetup[0].Content = "0-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "0-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "0-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "0-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "0-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "0-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "0-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "0-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "0-09 檢查上方寬度異常";
		FontCheckSetup[9].Content = "0-10 檢查下方寬度異常";
		FontCheckSetup[10].Content = "0-11 檢查中心異常";
		FontCheckSetup[11].Content = "0-12 檢查中心高度過短";
		FontCheckSetup[12].Content = "0-13 檢查字元中心高度過長";
		FontCheckSetup[13].Content = "0-14 檢查斷字異常";
		FontCheckSetup[14].Content = "0-15 檢查左邊高度異常";
		FontCheckSetup[15].Content = "0-16 檢查右邊高度異常";
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T0_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T0_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T0_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T0_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T0_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.T0_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T0_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.T0_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.T0_T_W;
		FontCheckSetup[9].IsChecked = fontToolParam.T0_B_W;
		FontCheckSetup[10].IsChecked = fontToolParam.T0_C;
		FontCheckSetup[11].IsChecked = fontToolParam.T0_C_H_MIN;
		FontCheckSetup[12].IsChecked = fontToolParam.T0_C_H_MAX;
		FontCheckSetup[13].IsChecked = fontToolParam.T0_Break;
		FontCheckSetup[14].IsChecked = fontToolParam.T0_L_H;
		FontCheckSetup[15].IsChecked = fontToolParam.T0_R_H;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T0_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T0_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T0_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T0_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T0_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T0_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T0_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup1Check()
	{
		FontCheckSetup[0].Content = "1-01 檢查高度異常";
		FontCheckSetup[1].Content = "1-02 檢查寬度異常";
		FontCheckSetup[2].Content = "1-03 檢查斷字異常";
		for (int i = 3; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T1_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T1_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T1_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T1_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T1_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T1_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T1_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T1_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T1_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T1_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup2Check()
	{
		FontCheckSetup[0].Content = "2-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "2-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "2-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "2-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "2-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "2-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "2-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "2-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "2-09 檢查斷字異常";
		for (int i = 9; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T2_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T2_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T2_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T2_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T2_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.T2_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T2_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.T2_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.T2_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T2_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T2_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T2_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T2_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T2_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T2_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T2_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup3Check()
	{
		FontCheckSetup[0].Content = "3-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "3-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "3-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "3-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "3-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "3-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "3-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "3-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "3-09 檢查斷字異常";
		FontCheckSetup[9].Content = "3-10 檢查上方寬度異常";
		FontCheckSetup[10].Content = "3-11 檢查下方寬度異常";
		FontCheckSetup[11].Content = "3-12 檢查中心寬度異常";
		for (int i = 12; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T3_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T3_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T3_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T3_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T3_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.T3_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T3_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.T3_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.T3_Break;
		FontCheckSetup[9].IsChecked = fontToolParam.T3_T_W;
		FontCheckSetup[10].IsChecked = fontToolParam.T3_B_W;
		FontCheckSetup[11].IsChecked = fontToolParam.T3_C_W;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T3_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T3_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T3_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T3_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T3_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T3_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T3_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup4Check()
	{
		FontCheckSetup[0].Content = "4-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "4-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "4-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "4-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "4-05 檢查上方寬度異常";
		FontCheckSetup[5].Content = "4-06 檢查下方寬度異常";
		FontCheckSetup[6].Content = "4-07 檢查斷字異常";
		FontCheckSetup[7].Content = "4-08 檢查高度異常";
		FontCheckSetup[8].Content = "4-09 檢查寬度異常";
		for (int i = 9; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T4_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T4_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T4_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T4_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T4_T_W;
		FontCheckSetup[5].IsChecked = fontToolParam.T4_B_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T4_Break;
		FontCheckSetup[7].IsChecked = fontToolParam.T4_H;
		FontCheckSetup[8].IsChecked = fontToolParam.T4_W;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T4_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T4_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T4_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T4_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T4_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T4_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T4_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup5Check()
	{
		FontCheckSetup[0].Content = "5-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "5-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "5-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "5-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "5-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "5-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "5-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "5-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "5-09 檢查斷字異常";
		FontCheckSetup[9].Content = "5-10 檢查上方高度異常";
		FontCheckSetup[10].Content = "5-11 檢查下方高度異常";
		for (int i = 11; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T5_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T5_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T5_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T5_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T5_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.T5_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T5_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.T5_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.T5_Break;
		FontCheckSetup[9].IsChecked = fontToolParam.T5_TT_W;
		FontCheckSetup[10].IsChecked = fontToolParam.T5_TB_W;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T5_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T5_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T5_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T5_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T5_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T5_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T5_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup6Check()
	{
		FontCheckSetup[0].Content = "6-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "6-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "6-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "6-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "6-05 檢查上方寬度異常";
		FontCheckSetup[5].Content = "6-06 檢查下方寬度異常";
		FontCheckSetup[6].Content = "6-07 檢查斷字異常";
		FontCheckSetup[7].Content = "6-08 檢查中心異常";
		for (int i = 8; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T6_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T6_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T6_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T6_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T6_T_W;
		FontCheckSetup[5].IsChecked = fontToolParam.T6_B_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T6_Break;
		FontCheckSetup[7].IsChecked = fontToolParam.T6_C;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T6_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T6_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T6_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T6_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T6_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T6_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T6_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup7Check()
	{
		FontCheckSetup[0].Content = "7-01 檢查上方寬度異常";
		FontCheckSetup[1].Content = "7-02 檢查下方寬度異常";
		FontCheckSetup[2].Content = "7-03 檢查高度異常";
		FontCheckSetup[3].Content = "7-04 檢查寬度異常";
		FontCheckSetup[4].Content = "7-05 檢查斷字異常";
		for (int i = 5; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T7_T_W;
		FontCheckSetup[1].IsChecked = fontToolParam.T7_B_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T7_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T7_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T7_Break;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T7_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T7_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T7_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T7_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T7_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T7_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T7_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup8Check()
	{
		FontCheckSetup[0].Content = "8-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "8-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "8-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "8-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "8-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = "8-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = "8-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = "8-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = "8-09 檢查下方寬度異常";
		FontCheckSetup[9].Content = "8-10 檢查中心異常";
		FontCheckSetup[10].Content = "8-11 檢查中心高度過短";
		FontCheckSetup[11].Content = "8-12 檢查字元中心高度過長";
		FontCheckSetup[12].Content = "8-13 檢查斷字異常";
		FontCheckSetup[13].Content = "8-14 檢查左邊高度異常";
		FontCheckSetup[14].Content = "8-15 檢查右邊高度異常";
		for (int i = 15; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T8_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T8_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T8_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T8_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T8_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.T8_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T8_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.T8_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.T8_B_W;
		FontCheckSetup[9].IsChecked = fontToolParam.T8_C;
		FontCheckSetup[10].IsChecked = fontToolParam.T8_C_H_MIN;
		FontCheckSetup[11].IsChecked = fontToolParam.T8_C_H_MAX;
		FontCheckSetup[12].IsChecked = fontToolParam.T8_Break;
		FontCheckSetup[13].IsChecked = fontToolParam.T8_L_H;
		FontCheckSetup[14].IsChecked = fontToolParam.T8_R_H;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T8_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T8_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T8_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T8_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T8_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T8_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T8_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Setup9Check()
	{
		FontCheckSetup[0].Content = "9-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = "9-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = "9-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = "9-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = "9-05 檢查下方高度異常";
		FontCheckSetup[5].Content = "9-06 檢查下方寬度異常";
		FontCheckSetup[6].Content = "9-07 檢查斷字異常";
		FontCheckSetup[7].Content = "9-08 檢查中心異常";
		for (int i = 8; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.T9_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.T9_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.T9_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.T9_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.T9_B_H;
		FontCheckSetup[5].IsChecked = fontToolParam.T9_B_W;
		FontCheckSetup[6].IsChecked = fontToolParam.T9_Break;
		FontCheckSetup[7].IsChecked = fontToolParam.T9_C;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.T9_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.T9_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.T9_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.T9_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.T9_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.T9_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.T9_UseFontDBMask;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupConnectCheck()
	{
		img_BinaryShift.Visibility = Visibility.Visible;
		cb_MaskSelect.Visibility = Visibility.Visible;
		lab_MaskSelect.Visibility = Visibility.Visible;
		img_FontBinaryShift.Visibility = Visibility.Hidden;
		img_PatternBinaryShift.Visibility = Visibility.Hidden;
		lab_FontImage1.Visibility = Visibility.Hidden;
		lab_FontImage2.Visibility = Visibility.Hidden;
		txt_MinMatchScore.Visibility = Visibility.Visible;
		lab_MinMatchScore.Visibility = Visibility.Visible;
		lab_FontSetupMain.Content = "字   元   沾   黏   設   定";
		FontCheckSetup[0].Content = "CT-01 檢查字元數量異常";
		FontCheckSetup[1].Content = "CT-02 檢查字元寬度異常";
		FontCheckSetup[2].Content = "";
		FontCheckSetup[3].Content = "加速文字定位";
		FontCheckSetup[4].Content = "使用自動切割共用庫資料庫字元功能";
		for (int i = 0; i < 16; i++)
		{
			if (i < 5)
			{
				FontCheckSetup[i].Visibility = Visibility.Visible;
				continue;
			}
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		FontCheckSetup[2].Visibility = Visibility.Hidden;
		grd_FontBinarySetup.Visibility = Visibility.Visible;
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		FontCheckSetup[0].IsChecked = fontToolParam.ConnectCountAlarm;
		FontCheckSetup[1].IsChecked = fontToolParam.ConnectWidthAlarm;
		FontCheckSetup[3].IsChecked = fontToolParam.UseFastMatch;
		FontCheckSetup[4].IsChecked = fontToolParam.AutoSetFontImage;
		sl_BinaryShift.Value = fontToolParam.ConnectBinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.ConnectOpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.ConnectOpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.ConnectOpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.ConnectOpenCloseCount2;
		txt_MinMatchScore.Text = fontToolParam.MinMatchScore.ToString();
		cb_MaskSelect.SelectedIndex = fontToolParam.MaskSelectIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		UpdateUI = updateUI;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetupOtherCheck(string otherfont, int index)
	{
		FontCheckSetup[0].Content = otherfont + "-01 檢查左上角高度異常";
		FontCheckSetup[1].Content = otherfont + "-02 檢查左上角寬度異常";
		FontCheckSetup[2].Content = otherfont + "-03 檢查右上角高度異常";
		FontCheckSetup[3].Content = otherfont + "-04 檢查右上角寬度異常";
		FontCheckSetup[4].Content = otherfont + "-05 檢查左下角高度異常";
		FontCheckSetup[5].Content = otherfont + "-06 檢查左下角寬度異常";
		FontCheckSetup[6].Content = otherfont + "-07 檢查右下角高度異常";
		FontCheckSetup[7].Content = otherfont + "-08 檢查右下角寬度異常";
		FontCheckSetup[8].Content = otherfont + "-09 檢查斷字異常";
		FontCheckSetup[9].Content = otherfont + "-10 檢查上方寬度異常";
		FontCheckSetup[10].Content = otherfont + "-11 檢查下方寬度異常";
		FontCheckSetup[11].Content = otherfont + "-12 檢查中心寬度異常";
		for (int i = 12; i < 16; i++)
		{
			FontCheckSetup[i].Content = "";
			FontCheckSetup[i].Visibility = Visibility.Hidden;
		}
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		FontCheckSetup[0].IsChecked = fontToolParam.OtherFont[index].T_LT_H;
		FontCheckSetup[1].IsChecked = fontToolParam.OtherFont[index].T_LT_W;
		FontCheckSetup[2].IsChecked = fontToolParam.OtherFont[index].T_RT_H;
		FontCheckSetup[3].IsChecked = fontToolParam.OtherFont[index].T_RT_W;
		FontCheckSetup[4].IsChecked = fontToolParam.OtherFont[index].T_LB_H;
		FontCheckSetup[5].IsChecked = fontToolParam.OtherFont[index].T_LB_W;
		FontCheckSetup[6].IsChecked = fontToolParam.OtherFont[index].T_RB_H;
		FontCheckSetup[7].IsChecked = fontToolParam.OtherFont[index].T_RB_W;
		FontCheckSetup[8].IsChecked = fontToolParam.OtherFont[index].T_Break;
		FontCheckSetup[9].IsChecked = fontToolParam.OtherFont[index].T_T_W;
		FontCheckSetup[10].IsChecked = fontToolParam.OtherFont[index].T_B_W;
		FontCheckSetup[11].IsChecked = fontToolParam.OtherFont[index].T_C_W;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		sl_BinaryShift.Value = fontToolParam.OtherFont[index].T_BinaryShift;
		cb_OpenClose.SelectedIndex = fontToolParam.OtherFont[index].T_OpenClose;
		cb_OpenCloseCount.SelectedIndex = fontToolParam.OtherFont[index].T_OpenCloseCount;
		cb_OpenClose2.SelectedIndex = fontToolParam.OtherFont[index].T_OpenClose2;
		cb_OpenCloseCount2.SelectedIndex = fontToolParam.OtherFont[index].T_OpenCloseCount2;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		chk_UseMorphFirst.IsChecked = fontToolParam.OtherFont[index].T_UseMorphFirst;
		chk_UseFontDBMask.IsChecked = fontToolParam.OtherFont[index].T_UseFontDBMask;
		UpdateUI = updateUI;
	}

	private void SaveCheck_R()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.R_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.R_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.R_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.R_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.R_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.R_Area = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.R_T_W = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.R_B_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.R_L_H = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.R_R_H = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.R_C = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.R_C_H_MIN = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.R_LR_H_Diff = FontCheckSetup[12].IsChecked.Value;
		fontToolParam.R_Break = FontCheckSetup[13].IsChecked.Value;
		fontToolParam.R_RB_H = FontCheckSetup[14].IsChecked.Value;
		fontToolParam.R_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.R_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.R_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.R_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.R_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.R_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.R_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_M()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.M_T_W = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.M_B_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.M_W = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.M_Break = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.M_Leg1 = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.M_Leg2 = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.M_Leg3 = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.M_Connected = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.M_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.M_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.M_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.M_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.M_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.M_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.M_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_C()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.C_W = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.C_H = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.C_T_W = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.C_B_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.C_L_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.C_R_H = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.C_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.C_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.C_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.C_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.C_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.C_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.C_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.C_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_U()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.U_W = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.U_H = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.U_T_W = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.U_B_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.U_L_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.U_R_H = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.U_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.U_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.U_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.U_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.U_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.U_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.U_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.U_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_X()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.X_W = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.X_H = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.X_T_W = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.X_B_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.X_L_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.X_R_H = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.X_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.X_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.X_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.X_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.X_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.X_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.X_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.X_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_B()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.TB_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.TB_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.TB_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.TB_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.TB_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.TB_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.TB_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.TB_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.TB_B_W = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.TB_C = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.TB_C_H_MIN = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.TB_C_H_MAX = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.TB_Break = FontCheckSetup[12].IsChecked.Value;
		fontToolParam.TB_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.TB_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.TB_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.TB_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.TB_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.TB_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.TB_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_E()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.TE_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.TE_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.TE_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.TE_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.TE_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.TE_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.TE_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.TE_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.TE_Break = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.TE_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.TE_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.TE_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.TE_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.TE_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.TE_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.TE_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_Dash()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.Dash_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.Dash_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.Dash_Break = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.Dash_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.Dash_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.Dash_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.Dash_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.Dash_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.Dash_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.Dash_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_D()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.D_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.D_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.D_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.D_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.D_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.D_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.D_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.D_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.D_T_W = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.D_B_W = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.D_C = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.D_C_H_MIN = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.D_C_H_MAX = FontCheckSetup[12].IsChecked.Value;
		fontToolParam.D_Break = FontCheckSetup[13].IsChecked.Value;
		fontToolParam.D_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.D_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.D_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.D_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.D_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.D_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.D_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_A()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.A_W = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.A_H = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.A_T_W = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.A_B_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.A_L_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.A_R_H = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.A_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.A_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.A_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.A_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.A_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.A_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.A_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.A_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_0()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T0_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T0_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T0_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T0_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T0_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T0_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T0_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T0_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T0_T_W = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.T0_B_W = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.T0_C = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.T0_C_H_MIN = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.T0_C_H_MAX = FontCheckSetup[12].IsChecked.Value;
		fontToolParam.T0_Break = FontCheckSetup[13].IsChecked.Value;
		fontToolParam.T0_L_H = FontCheckSetup[14].IsChecked.Value;
		fontToolParam.T0_R_H = FontCheckSetup[15].IsChecked.Value;
		fontToolParam.T0_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T0_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T0_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T0_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T0_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T0_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T0_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_1()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T1_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T1_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T1_Break = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T1_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T1_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T1_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T1_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T1_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T1_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T1_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_2()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T2_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T2_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T2_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T2_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T2_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T2_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T2_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T2_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T2_Break = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.T2_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T2_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T2_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T2_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T2_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T2_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T2_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_3()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T3_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T3_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T3_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T3_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T3_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T3_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T3_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T3_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T3_Break = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.T3_T_W = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.T3_B_W = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.T3_C_W = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.T3_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T3_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T3_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T3_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T3_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T3_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T3_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_4()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T4_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T4_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T4_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T4_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T4_T_W = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T4_B_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T4_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T4_H = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T4_W = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.T4_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T4_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T4_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T4_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T4_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T4_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T4_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_5()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T5_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T5_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T5_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T5_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T5_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T5_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T5_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T5_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T5_Break = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.T5_TT_W = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.T5_TB_W = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.T5_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T5_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T5_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T5_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T5_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T5_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T5_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_6()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T6_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T6_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T6_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T6_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T6_T_W = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T6_B_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T6_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T6_C = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T6_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T6_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T6_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T6_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T6_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T6_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T6_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_7()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T7_T_W = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T7_B_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T7_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T7_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T7_Break = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T7_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T7_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T7_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T7_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T7_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T7_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T7_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_8()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T8_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T8_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T8_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T8_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T8_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T8_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T8_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T8_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T8_B_W = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.T8_C = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.T8_C_H_MIN = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.T8_C_H_MAX = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.T8_Break = FontCheckSetup[12].IsChecked.Value;
		fontToolParam.T8_L_H = FontCheckSetup[13].IsChecked.Value;
		fontToolParam.T8_R_H = FontCheckSetup[14].IsChecked.Value;
		fontToolParam.T8_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T8_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T8_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T8_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T8_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T8_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T8_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void SaveCheck_9()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.T9_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.T9_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.T9_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.T9_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.T9_B_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.T9_B_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.T9_Break = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.T9_C = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.T9_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.T9_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.T9_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.T9_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.T9_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.T9_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.T9_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SaveCheck_Connect()
	{
		try
		{
			INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
			fontToolParam.ConnectCountAlarm = FontCheckSetup[0].IsChecked.Value;
			fontToolParam.ConnectWidthAlarm = FontCheckSetup[1].IsChecked.Value;
			fontToolParam.UseFastMatch = FontCheckSetup[3].IsChecked.Value;
			fontToolParam.AutoSetFontImage = FontCheckSetup[4].IsChecked.Value;
			fontToolParam.ConnectBinaryShift = (int)sl_BinaryShift.Value;
			fontToolParam.ConnectOpenClose = cb_OpenClose.SelectedIndex;
			fontToolParam.ConnectOpenCloseCount = cb_OpenCloseCount.SelectedIndex;
			fontToolParam.ConnectOpenClose2 = cb_OpenClose2.SelectedIndex;
			fontToolParam.ConnectOpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
			fontToolParam.MaskSelectIndex = cb_MaskSelect.SelectedIndex;
			fontToolParam.MinMatchScore = Convert.ToInt32(txt_MinMatchScore.Text);
			lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
			fontToolParam.ValueChangeFlag = true;
			inspection.paramFile.FontToolParam = fontToolParam;
			inspection.fontTool.PatternRectSeted = false;
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-SaveCheck_Connect()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void SaveCheck_OtherFont(string otherfont, int index)
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		fontToolParam.OtherFont[index].T_LT_H = FontCheckSetup[0].IsChecked.Value;
		fontToolParam.OtherFont[index].T_LT_W = FontCheckSetup[1].IsChecked.Value;
		fontToolParam.OtherFont[index].T_RT_H = FontCheckSetup[2].IsChecked.Value;
		fontToolParam.OtherFont[index].T_RT_W = FontCheckSetup[3].IsChecked.Value;
		fontToolParam.OtherFont[index].T_LB_H = FontCheckSetup[4].IsChecked.Value;
		fontToolParam.OtherFont[index].T_LB_W = FontCheckSetup[5].IsChecked.Value;
		fontToolParam.OtherFont[index].T_RB_H = FontCheckSetup[6].IsChecked.Value;
		fontToolParam.OtherFont[index].T_RB_W = FontCheckSetup[7].IsChecked.Value;
		fontToolParam.OtherFont[index].T_Break = FontCheckSetup[8].IsChecked.Value;
		fontToolParam.OtherFont[index].T_T_W = FontCheckSetup[9].IsChecked.Value;
		fontToolParam.OtherFont[index].T_B_W = FontCheckSetup[10].IsChecked.Value;
		fontToolParam.OtherFont[index].T_C_W = FontCheckSetup[11].IsChecked.Value;
		fontToolParam.OtherFont[index].T_BinaryShift = (int)sl_BinaryShift.Value;
		fontToolParam.OtherFont[index].T_OpenClose = cb_OpenClose.SelectedIndex;
		fontToolParam.OtherFont[index].T_OpenCloseCount = cb_OpenCloseCount.SelectedIndex;
		fontToolParam.OtherFont[index].T_OpenClose2 = cb_OpenClose2.SelectedIndex;
		fontToolParam.OtherFont[index].T_OpenCloseCount2 = cb_OpenCloseCount2.SelectedIndex;
		lab_BinaryShift.Content = (int)sl_BinaryShift.Value;
		fontToolParam.OtherFont[index].T_UseFontDBMask = chk_UseFontDBMask.IsChecked.Value;
		fontToolParam.OtherFont[index].T_UseMorphFirst = chk_UseMorphFirst.IsChecked.Value;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AutoTest_Click(object sender, RoutedEventArgs e)
	{
		if (filelistTestIndex != -1 && MaxfilelistTestCount != 0 && filelistTest != null)
		{
			if (AutoTestTImer.IsEnabled)
			{
				AutoTestTImer.Stop();
				txt_TestFontResult.Text = "";
				txt_TestFontResult.Text += string.Format("\nAOI NG = {0}\n", FontTestError_AOI);
				txt_TestFontResult.Text += string.Format("\nOCR NG = {0}\n", FontTestError_OCR);
				txt_TestFontResult.Text += string.Format("\n字元分析 NG = {0}\n", FontTestError_Check);
				txt_TestFontResult.Text += string.Format("\nAI NG = {0}\n", FontTestError_AI);
			}
			else
			{
				AutoTestTImer.Start();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AutoTestTImer_Tick(object sender, EventArgs e)
	{
		if (TimerInUsed)
		{
			return;
		}
		TimerInUsed = true;
		CurrentImg = filelistTest[filelistTestIndex];
		System.IO.Path.GetExtension(CurrentImg);
		CurrentMat = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale).Clone();
		bool num = TestFontInspect(CurrentMat);
		filelistTestIndex++;
		if (filelistTestIndex >= MaxfilelistTestCount)
		{
			txt_TestFontResult.Text += string.Format("\nAOI NG = {0}\n", FontTestError_AOI);
			txt_TestFontResult.Text += string.Format("\nOCR NG = {0}\n", FontTestError_OCR);
			txt_TestFontResult.Text += string.Format("\n字元分析 NG = {0}\n", FontTestError_Check);
			txt_TestFontResult.Text += string.Format("\nAI NG = {0}\n", FontTestError_AI);
			filelistTestIndex = 0;
			FontTestOKCount = 0;
			FontTestNGCount = 0;
			FontTestError_AOI = 0;
			FontTestError_OCR = 0;
			FontTestError_Check = 0;
			FontTestError_AI = 0;
			if (inspection.fontTool.TrainAIFontParamMode)
			{
				if (!RunSaveOKFinish)
				{
					CheckFileList(TrainNGPath);
					RunSaveOKFinish = true;
				}
				else
				{
					btn_GenCSVFile.IsEnabled = false;
					txt_TestFontResult.Text += "OK CSV 產生完成。\n";
					txt_TestFontResult.Text += "NG CSV 產生完成。\n";
					inspection.fontTool.TrainAIFontParamMode = false;
					AutoTestTImer.Stop();
				}
			}
			else
			{
				AutoTestTImer.Stop();
			}
		}
		if (num)
		{
			if (chk_OKStop.IsChecked == true)
			{
				AutoTestTImer.Stop();
			}
		}
		else if (chk_NGStop.IsChecked == true)
		{
			AutoTestTImer.Stop();
		}
		TimerInUsed = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SaveTestImage_Click(object sender, RoutedEventArgs e)
	{
		if (CurrentImg == "" || !File.Exists(CurrentImg))
		{
			return;
		}
		Mat mat = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale);
		if (mat.Width <= 0 && mat.Height <= 0)
		{
			if (ShowMessage)
			{
				MessageBox.Show("影像尺寸錯誤，存檔取消", "錯誤");
			}
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Image files (*.bmp;)|*.bmp;|All files (*.*)|*.*";
		if (saveFileDialog.ShowDialog() != true)
		{
			return;
		}
		if (!Directory.Exists(System.IO.Path.GetDirectoryName(saveFileDialog.FileName)))
		{
			if (ShowMessage)
			{
				MessageBox.Show("存檔路徑不合法", "錯誤");
			}
		}
		else
		{
			Cv2.ImWrite(saveFileDialog.FileName, mat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenFontFolder_Click(object sender, RoutedEventArgs e)
	{
		string text = "D:\\AIImage\\AI_Font\\TrainingImage\\";
		string fileName = "c:\\windows\\explorer.exe";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string arguments = string.Format("/root, {0}", text);
		Process.Start(fileName, arguments);
	}

	private void Btn_ShowFontBinary_Click(object sender, RoutedEventArgs e)
	{
		if (inspection.fontTool.FontConnectBinary.Width > 0 && inspection.fontTool.FontConnectBinary.Height > 0)
		{
			Mat mat = inspection.fontTool.FontConnectBinary.Clone();
			if (chk_FontAlignVertical.IsChecked == true)
			{
				Cv2.Transpose(mat, mat);
				Cv2.Flip(mat, mat, FlipMode.Y);
			}
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			img_FontConnected.Background = imageBrush;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenOKNG_Click(object sender, RoutedEventArgs e)
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		try
		{
			if (!folderSelectDialog.ShowDialog())
			{
				return;
			}
			TrainOKPath = string.Format("{0}\\OK\\", folderSelectDialog.FileName);
			TrainNGPath = string.Format("{0}\\NG\\", folderSelectDialog.FileName);
			if (!Directory.Exists(TrainOKPath) || !Directory.Exists(TrainNGPath))
			{
				TrainOKPath = (TrainNGPath = "");
				return;
			}
			btn_GenCSVFile.IsEnabled = true;
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
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-Btn_OpenOKNG_Click()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Btn_GenCSVFile_Click(object sender, RoutedEventArgs e)
	{
		if (!Directory.Exists(TrainOKPath) || !Directory.Exists(TrainNGPath))
		{
			TrainOKPath = (TrainNGPath = "");
			btn_GenCSVFile.IsEnabled = false;
			return;
		}
		CheckFileList(TrainOKPath);
		if (filelistTestIndex != -1 && MaxfilelistTestCount != 0 && filelistTest != null)
		{
			RunSaveOKFinish = false;
			AutoTestTImer.Start();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ShowExtend1_Click(object sender, RoutedEventArgs e)
	{
		int index = Convert.ToInt32(((Button)sender).Uid);
		if (PreviewMode || !inited || !UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
		}
		else
		{
			if (!inspection.buffers.LocatedReady || DontSave)
			{
				return;
			}
			WriteUIValueToParam();
			TOOL_RESULT iresult = TOOL_RESULT.EM_NG;
			inspection.fontTool.Inspect(ref iresult, FaceIndex);
			OpenCvSharp.Rect fontSearchRect = inspection.fontTool.GetFontSearchRect(index);
			Mat mat = new Mat();
			if (!inspection.fontTool.FlipStatus)
			{
				ShowBitmap.CopyTo(mat);
			}
			else
			{
				ShowBitmap.CopyTo(ShowBitmapFlip);
				if (inspection.fontTool.FoundMatchTextRect.Width > 0 && inspection.fontTool.FoundMatchTextRect.Height > 0)
				{
					using Mat mat2 = MM.newMat(ShowBitmapFlip, inspection.fontTool.FoundMatchTextRect).Clone();
					using Mat m = new Mat(ShowBitmapFlip, inspection.fontTool.FoundMatchTextRect);
					using (new Mat())
					{
						Cv2.Flip(mat2, mat2, FlipMode.XY);
						mat2.CopyTo(m);
					}
				}
				ShowBitmapFlip.CopyTo(mat);
				btn_Result_Flip.Visibility = Visibility.Visible;
			}
			mat.Rectangle(fontSearchRect, Scalar.OrangeRed);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
		}
	}

	private void Cb_MaskSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!PreviewMode && inited && UpdateUI && inspection.buffers.LocatedReady && !DontSave && grd_FontSetup.Visibility != Visibility.Hidden)
		{
			BinaryShiftPreview();
			Btn_Test_Click(this, null);
		}
	}

	private void Btn_TrainCSV_Click(object sender, RoutedEventArgs e)
	{
		grd_FontSetup.Visibility = Visibility.Hidden;
		TrainCSVWindow trainCSVWindow = new TrainCSVWindow();
		trainCSVWindow.SetInspection(inspection);
		trainCSVWindow.ShowDialog();
		inspection.fontTool.TrainAIFontParamMode = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecoverPara_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(string.Format("是否回復上一次[{0}]字元分析設定?", FontSetupChar), "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			RecoverFontSetup(FontSetupChar);
			SetupFontSetup(FontSetupChar);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RecoverFontSetup(string str)
	{
		if (str != null)
		{
			switch (str.Length)
			{
			case 1:
				switch (str[0])
				{
				case '0':
					Recover_0();
					return;
				case 'R':
					Recover_R();
					return;
				case 'M':
					Recover_M();
					return;
				case 'C':
					Recover_C();
					return;
				case '-':
					Recover_Dash();
					return;
				case '1':
					Recover_1();
					return;
				case '2':
					Recover_2();
					return;
				case '3':
					Recover_3();
					return;
				case '4':
					Recover_4();
					return;
				case '5':
					Recover_5();
					return;
				case '6':
					Recover_6();
					return;
				case '7':
					Recover_7();
					return;
				case '8':
					Recover_8();
					return;
				case '9':
					Recover_9();
					return;
				case 'X':
					Recover_X();
					return;
				case 'B':
					Recover_B();
					return;
				case 'D':
					Recover_D();
					return;
				case 'E':
					Recover_E();
					return;
				case 'A':
					Recover_A();
					return;
				}
				break;
			case 7:
				if (!(str == "Connect"))
				{
					break;
				}
				Recover_Connect();
				return;
			}
		}
		int num = inspection.fontTool.GetFontIndex(str) - 20;
		if (num < 0 || num >= 20)
		{
			num = 0;
		}
		Recover_OtherFont(str, num);
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

	private void Recover_Connect()
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.ConnectCountAlarm = fontToolParam2.ConnectCountAlarm;
		fontToolParam.ConnectWidthAlarm = fontToolParam2.ConnectWidthAlarm;
		fontToolParam.UseFastMatch = fontToolParam2.UseFastMatch;
		fontToolParam.AutoSetFontImage = fontToolParam2.AutoSetFontImage;
		fontToolParam.ConnectBinaryShift = fontToolParam2.ConnectBinaryShift;
		fontToolParam.ConnectOpenClose = fontToolParam2.ConnectOpenClose;
		fontToolParam.ConnectOpenCloseCount = fontToolParam2.ConnectOpenCloseCount;
		fontToolParam.MaskSelectIndex = fontToolParam2.MaskSelectIndex;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
		inspection.fontTool.PatternRectSeted = false;
	}

	private void Recover_OtherFont(string str, int index)
	{
		INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile_Backup.FontToolParam;
		fontToolParam.OtherFont[index].T_LT_H = fontToolParam2.OtherFont[index].T_LT_H;
		fontToolParam.OtherFont[index].T_LT_W = fontToolParam2.OtherFont[index].T_LT_W;
		fontToolParam.OtherFont[index].T_RT_H = fontToolParam2.OtherFont[index].T_RT_H;
		fontToolParam.OtherFont[index].T_RT_W = fontToolParam2.OtherFont[index].T_RT_W;
		fontToolParam.OtherFont[index].T_LB_H = fontToolParam2.OtherFont[index].T_LB_H;
		fontToolParam.OtherFont[index].T_LB_W = fontToolParam2.OtherFont[index].T_LB_W;
		fontToolParam.OtherFont[index].T_RB_H = fontToolParam2.OtherFont[index].T_RB_H;
		fontToolParam.OtherFont[index].T_RB_W = fontToolParam2.OtherFont[index].T_RB_W;
		fontToolParam.OtherFont[index].T_Break = fontToolParam2.OtherFont[index].T_Break;
		fontToolParam.OtherFont[index].T_T_W = fontToolParam2.OtherFont[index].T_T_W;
		fontToolParam.OtherFont[index].T_B_W = fontToolParam2.OtherFont[index].T_B_W;
		fontToolParam.OtherFont[index].T_C_W = fontToolParam2.OtherFont[index].T_C_W;
		fontToolParam.ValueChangeFlag = true;
		inspection.paramFile.FontToolParam = fontToolParam;
	}

	private void Btn_CloseCheckFontTool_Click(object sender, RoutedEventArgs e)
	{
		grd_FontTest.Visibility = Visibility.Hidden;
	}

	private void Btn_FontPageNext_Click(object sender, RoutedEventArgs e)
	{
		if (FontPageIndex == 0)
		{
			btn_FontPagePrev.IsEnabled = true;
			FontPageIndex = 1;
			lab_FontDBPage.Content = 2;
			UpdateFontPage(1);
		}
		else if (FontPageIndex == 1)
		{
			btn_FontPagePrev.IsEnabled = true;
			FontPageIndex = 2;
			btn_FontPageNext.IsEnabled = false;
			lab_FontDBPage.Content = 3;
			UpdateFontPage(2);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateFontPage(int page)
	{
		switch (page)
		{
		case 0:
		{
			for (int k = 0; k < 16; k++)
			{
				FontGrid[k].Visibility = Visibility.Visible;
			}
			Button button23 = btn_R_Setup;
			string content = (btn_R_Setup.Uid = "R");
			button23.Content = content;
			Button button24 = btn_M_Setup;
			content = (btn_M_Setup.Uid = "M");
			button24.Content = content;
			Button button25 = btn_C_Setup;
			content = (btn_C_Setup.Uid = "C");
			button25.Content = content;
			Button button26 = btn_U_Setup;
			content = (btn_U_Setup.Uid = "U");
			button26.Content = content;
			Button button27 = btn_Dash_Setup;
			content = (btn_Dash_Setup.Uid = "-");
			button27.Content = content;
			Button button28 = btn_0_Setup;
			content = (btn_0_Setup.Uid = "0");
			button28.Content = content;
			Button button29 = btn_1_Setup;
			content = (btn_1_Setup.Uid = "1");
			button29.Content = content;
			Button button30 = btn_2_Setup;
			content = (btn_2_Setup.Uid = "2");
			button30.Content = content;
			Button button31 = btn_3_Setup;
			content = (btn_3_Setup.Uid = "3");
			button31.Content = content;
			Button button32 = btn_4_Setup;
			content = (btn_4_Setup.Uid = "4");
			button32.Content = content;
			Button button33 = btn_5_Setup;
			content = (btn_5_Setup.Uid = "5");
			button33.Content = content;
			Button button34 = btn_6_Setup;
			content = (btn_6_Setup.Uid = "6");
			button34.Content = content;
			Button button35 = btn_7_Setup;
			content = (btn_7_Setup.Uid = "7");
			button35.Content = content;
			Button button36 = btn_8_Setup;
			content = (btn_8_Setup.Uid = "8");
			button36.Content = content;
			Button button37 = btn_9_Setup;
			content = (btn_9_Setup.Uid = "9");
			button37.Content = content;
			break;
		}
		case 1:
		{
			for (int j = 0; j < 16; j++)
			{
				FontGrid[j].Visibility = Visibility.Visible;
			}
			Button button8 = btn_R_Setup;
			string content = (btn_R_Setup.Uid = "X");
			button8.Content = content;
			Button button9 = btn_M_Setup;
			content = (btn_M_Setup.Uid = "B");
			button9.Content = content;
			Button button10 = btn_C_Setup;
			content = (btn_C_Setup.Uid = "E");
			button10.Content = content;
			Button button11 = btn_U_Setup;
			content = (btn_U_Setup.Uid = "D");
			button11.Content = content;
			Button button12 = btn_Dash_Setup;
			content = (btn_Dash_Setup.Uid = "A");
			button12.Content = content;
			Button button13 = btn_0_Setup;
			content = (btn_0_Setup.Uid = "F");
			button13.Content = content;
			Button button14 = btn_1_Setup;
			content = (btn_1_Setup.Uid = "G");
			button14.Content = content;
			Button button15 = btn_2_Setup;
			content = (btn_2_Setup.Uid = "H");
			button15.Content = content;
			Button button16 = btn_3_Setup;
			content = (btn_3_Setup.Uid = "I");
			button16.Content = content;
			Button button17 = btn_4_Setup;
			content = (btn_4_Setup.Uid = "J");
			button17.Content = content;
			Button button18 = btn_5_Setup;
			content = (btn_5_Setup.Uid = "K");
			button18.Content = content;
			Button button19 = btn_6_Setup;
			content = (btn_6_Setup.Uid = "L");
			button19.Content = content;
			Button button20 = btn_7_Setup;
			content = (btn_7_Setup.Uid = "N");
			button20.Content = content;
			Button button21 = btn_8_Setup;
			content = (btn_8_Setup.Uid = "O");
			button21.Content = content;
			Button button22 = btn_9_Setup;
			content = (btn_9_Setup.Uid = "P");
			button22.Content = content;
			break;
		}
		default:
		{
			for (int i = 7; i < 16; i++)
			{
				FontGrid[i].Visibility = Visibility.Hidden;
			}
			Button button = btn_R_Setup;
			string content = (btn_R_Setup.Uid = "Q");
			button.Content = content;
			Button button2 = btn_M_Setup;
			content = (btn_M_Setup.Uid = "S");
			button2.Content = content;
			Button button3 = btn_C_Setup;
			content = (btn_C_Setup.Uid = "T");
			button3.Content = content;
			Button button4 = btn_U_Setup;
			content = (btn_U_Setup.Uid = "V");
			button4.Content = content;
			Button button5 = btn_Dash_Setup;
			content = (btn_Dash_Setup.Uid = "W");
			button5.Content = content;
			Button button6 = btn_0_Setup;
			content = (btn_0_Setup.Uid = "Y");
			button6.Content = content;
			Button button7 = btn_1_Setup;
			content = (btn_1_Setup.Uid = "Z");
			button7.Content = content;
			break;
		}
		}
		UpdateFontPattern();
	}

	private void Btn_FontPagePrev_Click(object sender, RoutedEventArgs e)
	{
		if (FontPageIndex == 1)
		{
			btn_FontPagePrev.IsEnabled = false;
			FontPageIndex = 0;
			btn_FontPageNext.IsEnabled = true;
			lab_FontDBPage.Content = 1;
			UpdateFontPage(0);
		}
		else if (FontPageIndex == 2)
		{
			FontPageIndex = 1;
			lab_FontDBPage.Content = 2;
			btn_FontPagePrev.IsEnabled = true;
			btn_FontPageNext.IsEnabled = true;
			UpdateFontPage(1);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Rad_PDByRecipe_Click(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (rad_PDByRecipe.IsChecked == true)
		{
			UsePublicDB = false;
			PatternDB = PrivatePatternDB;
		}
		else
		{
			UsePublicDB = true;
			PatternDB = PublicPatternDB;
		}
		UpdatePatternDBComboBox();
		UpdatePatternImage();
		try
		{
			if (rad_PDByRecipe.IsChecked == true)
			{
				tab_PublicDB.Header = LanguageManager.Instance["PrivateDB"];
			}
			else
			{
				tab_PublicDB.Header = LanguageManager.Instance["PublicDB"];
			}
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-Rad_PDByRecipe_Click()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Chk_UseFontDBMask_Click(object sender, RoutedEventArgs e)
	{
		if (UpdateUI && inited)
		{
			BinaryShiftPreview();
		}
	}

	private void bor_SearchRect1_MouseEnter(object sender, MouseEventArgs e)
	{
		lab_Message.Visibility = Visibility.Hidden;
	}

	private void btn_AI_Mode_Reload_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		inspection.fontTool.FontModelReaded[num] = false;
		inspection.fontTool.FontModelOnnxType[num] = 0;
	}

	private void chk_FontAlignVertical_MouseEnter(object sender, MouseEventArgs e)
	{
		lab_VerAlign.Visibility = Visibility.Visible;
	}

	private void chk_FontAlignVertical_MouseLeave(object sender, MouseEventArgs e)
	{
		lab_VerAlign.Visibility = Visibility.Hidden;
	}

	private void btn_SetTrainFolder_Click(object sender, RoutedEventArgs e)
	{
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (folderSelectDialog.ShowDialog() && Directory.Exists(folderSelectDialog.FileName) && CheckTrainFolder(folderSelectDialog.FileName))
		{
			TrainDir = folderSelectDialog.FileName;
			btn_SetTrainFolder.Background = Brushes.LightGoldenrodYellow;
			btn_Add2TrainA.IsEnabled = true;
			btn_Add2TrainB.IsEnabled = true;
			btn_Add2TrainD.IsEnabled = true;
			btn_Add2TrainE.IsEnabled = true;
			btn_UploadModel.IsEnabled = true;
			cb_FontSelectTest.SelectedIndex = inspection.fontTool.GetFontIndex(TrainDir.Substring(TrainDir.Length - 1));
			CheckClassicImage(TrainDir);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckTrainFolder(string path)
	{
		string path2 = path + "\\A\\";
		string path3 = path + "\\B\\";
		string path4 = path + "\\D\\";
		string path5 = path + "\\E\\";
		string path6 = path + "\\PASS\\";
		if (!Directory.Exists(path2))
		{
			MessageBox.Show("影像資料夾 A 不存在");
			return false;
		}
		if (!Directory.Exists(path3))
		{
			MessageBox.Show("影像資料夾 B 不存在");
			return false;
		}
		if (!Directory.Exists(path4))
		{
			MessageBox.Show("影像資料夾 D 不存在");
			return false;
		}
		if (!Directory.Exists(path5))
		{
			MessageBox.Show("影像資料夾 E 不存在");
			return false;
		}
		if (!Directory.Exists(path6))
		{
			MessageBox.Show("影像資料夾 PASS 不存在");
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CheckClassicImage(string path, bool updatelab = true)
	{
		string text = "";
		try
		{
			if (updatelab)
			{
				lab_TrainStr.Content = TrainDir.Substring(path.Length - 1);
			}
			string path2 = path + "\\A\\";
			string path3 = path + "\\B\\";
			string path4 = path + "\\D\\";
			string path5 = path + "\\E\\";
			string path6 = path + "\\PASS\\";
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
			}
			if (!Directory.Exists(path3))
			{
				Directory.CreateDirectory(path3);
			}
			if (!Directory.Exists(path4))
			{
				Directory.CreateDirectory(path4);
			}
			if (!Directory.Exists(path5))
			{
				Directory.CreateDirectory(path5);
			}
			if (!Directory.Exists(path6))
			{
				Directory.CreateDirectory(path6);
			}
			string[] files = Directory.GetFiles(path2, "*.bmp", SearchOption.TopDirectoryOnly);
			text = ClearSameImage(files, "A");
			if (text != "")
			{
				MessageBox.Show(text);
			}
			files = Directory.GetFiles(path3, "*.bmp", SearchOption.TopDirectoryOnly);
			text = ClearSameImage(files, "B");
			if (text != "")
			{
				MessageBox.Show(text);
			}
			files = Directory.GetFiles(path4, "*.bmp", SearchOption.TopDirectoryOnly);
			text = ClearSameImage(files, "D");
			if (text != "")
			{
				MessageBox.Show(text);
			}
			files = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			text = ClearSameImage(files, "E");
			if (text != "")
			{
				MessageBox.Show(text);
			}
			files = Directory.GetFiles(path6, "*.bmp", SearchOption.TopDirectoryOnly);
			text = ClearSameImage(files, "PASS");
			if (text != "")
			{
				MessageBox.Show(text);
			}
			string[] files2 = Directory.GetFiles(path2, "*.bmp", SearchOption.TopDirectoryOnly);
			string[] files3 = Directory.GetFiles(path3, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("A", files2, "B", files3);
			files3 = Directory.GetFiles(path4, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("A", files2, "D", files3);
			files3 = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("A", files2, "E", files3);
			files3 = Directory.GetFiles(path6, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("A", files2, "PASS", files3);
			files2 = Directory.GetFiles(path3, "*.bmp", SearchOption.TopDirectoryOnly);
			files3 = Directory.GetFiles(path4, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("B", files2, "D", files3);
			files3 = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("B", files2, "E", files3);
			files3 = Directory.GetFiles(path6, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("B", files2, "PASS", files3);
			files2 = Directory.GetFiles(path4, "*.bmp", SearchOption.TopDirectoryOnly);
			files3 = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("D", files2, "E", files3);
			files3 = Directory.GetFiles(path6, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("D", files2, "PASS", files3);
			files2 = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			files3 = Directory.GetFiles(path6, "*.bmp", SearchOption.TopDirectoryOnly);
			FindSameImage("E", files2, "PASS", files3);
		}
		catch (Exception ex)
		{
			string text2 = "TW01-FontToolWindow-CheckClassicImage()";
			text2 += ex.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, LanguageManager.Instance["Error"]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Add2TrainA_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (TrainDir == "")
			{
				if (ShowMessage)
				{
					MessageBox.Show("尚未設定訓練影像路徑，存檔取消", "錯誤");
				}
			}
			else
			{
				Button button = (Button)sender;
				AddImage2TrainDB(button.Uid, TrainDir);
				CheckClassicImage(TrainDir);
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-FontToolWindow-Btn_AddToAIOK_Click()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddImage2TrainDB(string classic, string baseTrainDir)
	{
		Mat currentMat = CurrentMat;
		if (currentMat.Width <= 0 || currentMat.Height <= 0)
		{
			if (ShowMessage)
			{
				MessageBox.Show("影像錯誤，存檔取消", "錯誤");
			}
			return;
		}
		string text = string.Format("{0}\\{1}\\", baseTrainDir, classic);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!Directory.Exists(text))
		{
			if (ShowMessage)
			{
				MessageBox.Show("存檔路徑不合法", "錯誤");
			}
		}
		else
		{
			DateTime now = DateTime.Now;
			Cv2.ImWrite(string.Format("{0}{1}-{2}.bmp", text, classic, now.ToString("yyyy-MM-dd-HH-mm-ss")), currentMat);
			string str = string.Format("[{0} 資料庫] 字元影像儲存成功!", classic);
			ShowLabMessage(str);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void cb_FontSelectTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		inspection.fontTool.FontModelOnnxType[5] = 0;
		inspection.fontTool.FontModelReaded[5] = false;
		if (cb_FontSelectTest.SelectedIndex >= 0)
		{
			string fontCharFromIndex = inspection.fontTool.GetFontCharFromIndex(cb_FontSelectTest.SelectedIndex);
			TrainDir = string.Format("D:\\AI_FONT_Train\\train\\{0}", fontCharFromIndex);
			if (Directory.Exists(TrainDir))
			{
				btn_SetTrainFolder.Background = Brushes.LightGoldenrodYellow;
				btn_Add2TrainA.IsEnabled = true;
				btn_Add2TrainB.IsEnabled = true;
				btn_Add2TrainD.IsEnabled = true;
				btn_Add2TrainE.IsEnabled = true;
				btn_UploadModel.IsEnabled = true;
				lab_TrainStr.Content = fontCharFromIndex;
			}
			else
			{
				btn_SetTrainFolder.Background = Brushes.White;
				btn_Add2TrainA.IsEnabled = false;
				btn_Add2TrainB.IsEnabled = false;
				btn_Add2TrainD.IsEnabled = false;
				btn_Add2TrainE.IsEnabled = false;
				btn_UploadModel.IsEnabled = false;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void cb_AIModelFont1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI)
		{
			ComboBox comboBox = (ComboBox)sender;
			int num = Convert.ToInt32(comboBox.Uid);
			inspection.fontTool.FontModelOnnxType[num] = 0;
			inspection.fontTool.FontModelReaded[num] = false;
			GridFontTrain[num].Visibility = ((comboBox.SelectedIndex == 0) ? Visibility.Hidden : Visibility.Visible);
			TxtAcceptScore[num].Text = "60";
			Btn_Test_Click(null, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_UploadModel_Click(object sender, RoutedEventArgs e)
	{
		if (cb_FontSelectTest.SelectedIndex < 0)
		{
			MessageBox.Show("尚未選擇AImodel!");
			return;
		}
		if (!File.Exists(string.Format("D:\\AI_FONT_Train\\scoreOcr\\scoreOcr.exe")))
		{
			MessageBox.Show("找不到訓練文字程式!");
			return;
		}
		string fontCharFromIndex = inspection.fontTool.GetFontCharFromIndex(cb_FontSelectTest.SelectedIndex);
		if (TrainDir == "")
		{
			TrainDir = string.Format("{0}\\{1}", "D:\\AI_FONT_Train", fontCharFromIndex);
		}
		CheckClassicImage(TrainDir, updatelab: false);
		string[] files = Directory.GetFiles(TrainDir, "*.bmp", SearchOption.AllDirectories);
		if (files.Length < 16)
		{
			MessageBox.Show(string.Format("訓練影像只有{0}張，小於最低訓練影像張數(16張)，請再增加訓練影像數量!", files.Length));
			return;
		}
		if (!UpdateIni(fontCharFromIndex))
		{
			MessageBox.Show("寫入parameter.ini錯誤!");
			return;
		}
		string text = string.Format("{0}\\ocr.onnx", TrainDir);
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		if (!RunTrainingScript())
		{
			return;
		}
		string text2 = "";
		string text3 = string.Format(arg0: (!UsePublicFont) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"), format: "{0}{1}.onnx", arg1: fontCharFromIndex);
		if (!File.Exists(text))
		{
			MessageBox.Show(string.Format("訓練失敗，找不到模型檔案。{0}", text));
			return;
		}
		File.Copy(text, text3, overwrite: true);
		File.Delete(text);
		for (int i = 0; i <= 5; i++)
		{
			inspection.fontTool.FontModelOnnxType[i] = 0;
			inspection.fontTool.FontModelReaded[i] = false;
		}
		for (int j = 0; j <= 5; j++)
		{
			inspection.fontTool.FontModelOnnxType[j] = 0;
			inspection.fontTool.FontModelReaded[j] = false;
		}
		try
		{
			string modelname = text3;
			int Score = 0;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			string text4 = string.Format("{0}\\{1}", TrainDir, fontCharFromIndex);
			string path = text4 + "\\A\\";
			string path2 = text4 + "\\B\\";
			string path3 = text4 + "\\D\\";
			string path4 = text4 + "\\E\\";
			string path5 = text4 + "\\PASS\\";
			Mat mat = new Mat();
			int num6 = 0;
			string[] files2 = Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly);
			int num7 = 60;
			int fontIndex = 5;
			for (int k = 0; k < files2.Length; k++)
			{
				mat = CVBuffer.ImRead(files2[k], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, fontIndex, mat.Clone(), mat, ref Score);
					if (Score < num7)
					{
						num++;
					}
					num6++;
				}
			}
			files2 = Directory.GetFiles(path2, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int l = 0; l < files2.Length; l++)
			{
				mat = CVBuffer.ImRead(files2[l], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, fontIndex, mat.Clone(), mat, ref Score);
					if (Score < num7)
					{
						num2++;
					}
					num6++;
				}
			}
			files2 = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int m = 0; m < files2.Length; m++)
			{
				mat = CVBuffer.ImRead(files2[m], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, fontIndex, mat.Clone(), mat, ref Score);
					if (Score < num7)
					{
						num3++;
					}
					num6++;
				}
			}
			files2 = Directory.GetFiles(path3, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int n = 0; n < files2.Length; n++)
			{
				mat = CVBuffer.ImRead(files2[n], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, fontIndex, mat.Clone(), mat, ref Score);
					if (Score >= num7)
					{
						num4++;
					}
					num6++;
				}
			}
			files2 = Directory.GetFiles(path4, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int num8 = 0; num8 < files2.Length; num8++)
			{
				mat = CVBuffer.ImRead(files2[num8], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, fontIndex, mat.Clone(), mat, ref Score);
					if (Score >= num7)
					{
						num5++;
					}
					num6++;
				}
			}
			if (ShowMessage)
			{
				MessageBox.Show(string.Format("模型更新成功。訓練資料庫測試結果，\n\n總測試張數 = {0} \n[A] 誤判 = {1} \n[B] 誤判 = {2} \n[PASS] 誤判 = {3} \n[D] 漏判 = {4} \n[E] 漏判 = {5}", num6, num, num2, num3, num4, num5), "注意");
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIP01-FontToolWindow-UpdateIni()" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(string.Format("模型更新成功，回測結果失敗，請檢查是否有影像不符合規格"));
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void grd_AI_Function_Drop(object sender, DragEventArgs e)
	{
		try
		{
			string[] array = null;
			array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
			if (array.Length != 0 && CheckTrainFolder(array[0]))
			{
				TrainDir = array[0];
				btn_SetTrainFolder.Background = Brushes.LightGoldenrodYellow;
				btn_Add2TrainA.IsEnabled = true;
				btn_Add2TrainB.IsEnabled = true;
				btn_Add2TrainD.IsEnabled = true;
				btn_Add2TrainE.IsEnabled = true;
				btn_UploadModel.IsEnabled = true;
				cb_FontSelectTest.SelectedIndex = inspection.fontTool.GetFontIndex(TrainDir.Substring(TrainDir.Length - 1));
				CheckClassicImage(TrainDir);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-grd_AI_Function_Drop()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void grd_AI_Test_Drop(object sender, DragEventArgs e)
	{
		try
		{
			string[] array = null;
			array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
			if (array.Length != 0)
			{
				CheckFileList(array[0]);
				btn_prev.IsEnabled = true;
				btn_next.IsEnabled = true;
				btn_AutoTest.IsEnabled = true;
				btn_Open.Background = Brushes.LightGoldenrodYellow;
			}
		}
		catch (Exception ex)
		{
			string text = "TW01-FontToolWindow-Grid_Drop()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, LanguageManager.Instance["Error"]);
			}
		}
	}

	private void btn_ImageF1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		string str = inspection.paramFile.FontToolParam.AllFont.Substring(num, 1);
		cb_FontSelectTest.SelectedIndex = inspection.fontTool.GetFontIndex(str);
		if (inspection.fontTool.OcrMat1[num].Width > 0 && inspection.fontTool.OcrMat1[num].Height > 0)
		{
			inspection.fontTool.OcrMat1[num].CopyTo(CurrentMat);
			TestFontInspect(CurrentMat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void BinaryShiftPreview()
	{
		try
		{
			SaveFontSetup();
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			inspection.fontTool.Inspect(ref iresult, FaceIndex);
			if (FontSetupChar == "Connect")
			{
				if (inspection.fontTool.FontConnectBinary.Width > 0 && inspection.fontTool.FontConnectBinary.Height > 0)
				{
					Mat mat = inspection.fontTool.FontConnectBinary.Clone();
					if (chk_FontAlignVertical.IsChecked == true)
					{
						Cv2.Transpose(mat, mat);
						Cv2.Flip(mat, mat, FlipMode.Y);
					}
					ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
					ImageBrush imageBrush = new ImageBrush();
					imageBrush.ImageSource = imageSource;
					img_BinaryShift.Background = imageBrush;
				}
				return;
			}
			Mat mat2 = new Mat();
			if (FontClick >= 0)
			{
				if (FontClick == 5)
				{
					mat2 = CVBuffer.ImRead(CurrentImg, ImreadModes.Grayscale);
					Mat fontbinary = new Mat();
					string str = "";
					Mat pattern = GetFontImageFromIndex(cb_FontSelectTest.SelectedIndex).Clone();
					if (inspection.fontTool.Check_Font(cb_FontSelectTest.SelectedIndex, mat2, pattern, ref str, ref fontbinary))
					{
						fontbinary.CopyTo(mat2);
					}
				}
				else
				{
					mat2 = inspection.fontTool.FontBinary[FontClick].Clone();
				}
				if (mat2.Width > 0 && mat2.Height > 0)
				{
					ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(mat2);
					ImageBrush imageBrush2 = new ImageBrush();
					imageBrush2.ImageSource = imageSource2;
					img_FontBinaryShift.Background = imageBrush2;
				}
			}
			else
			{
				img_FontBinaryShift.Background = Brushes.Gray;
			}
			mat2 = GetFontImageFromIndex(inspection.fontTool.GetFontIndex(FontSetupChar)).Clone();
			if (mat2.Width > 0 && mat2.Height > 0)
			{
				if (chk_FontAlignVertical.IsChecked == true)
				{
					Cv2.Transpose(mat2, mat2);
					Cv2.Flip(mat2, mat2, FlipMode.Y);
				}
				Mat mat3 = new Mat();
				Cv2.Threshold(mat2, mat3, 0.0, 255.0, ThresholdTypes.Otsu);
				if (inspection.paramFile.FontToolParam.FontIsBlack)
				{
					Cv2.BitwiseNot(mat3, mat3);
				}
				ImageSource imageSource3 = inspection.buffers.MatToBitmapImage(mat3);
				ImageBrush imageBrush3 = new ImageBrush();
				imageBrush3.ImageSource = imageSource3;
				img_PatternBinaryShift.Background = imageBrush3;
			}
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-BinaryShiftPreview()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Sl_BinaryShift_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (UpdateUI && inited)
		{
			BinaryShiftPreview();
		}
	}

	private void Cb_OpenClose_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && inited)
		{
			BinaryShiftPreview();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string ClearSameImage(string[] list1, string classicStr)
	{
		string result = "";
		if (list1 == null)
		{
			return result;
		}
		try
		{
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			int num = 0;
			for (int i = 0; i < list1.Length; i++)
			{
				list2.Add(list1[i]);
			}
			for (int j = 0; j < list2.Count; j++)
			{
				list3.Add(GetFileHash(list2[j]));
			}
			for (int k = 0; k < list3.Count; k++)
			{
				if (list3[k] == "")
				{
					continue;
				}
				for (int l = 0; l < list3.Count; l++)
				{
					if (!(list2[k] == list2[l]) && list3[k] == list3[l])
					{
						num++;
						list3[l] = "";
						if (File.Exists(list2[l]))
						{
							File.Delete(list2[l]);
						}
					}
				}
			}
			if (num > 0)
			{
				result = string.Format("[{0}]資料庫，刪除 {1} 張重複影像", classicStr, num);
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "AIP01-FontToolWindow-ClearSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string FindSameImage(string list1ID, string[] list1, string list2ID, string[] list2)
	{
		string result = "";
		if (list1 == null || list2 == null)
		{
			return result;
		}
		try
		{
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			List<string> list5 = new List<string>();
			List<string> list6 = new List<string>();
			for (int i = 0; i < list1.Length; i++)
			{
				list3.Add(list1[i]);
			}
			for (int j = 0; j < list2.Length; j++)
			{
				list4.Add(list2[j]);
			}
			for (int k = 0; k < list3.Count; k++)
			{
				list5.Add(GetFileHash(list3[k]));
			}
			for (int l = 0; l < list4.Count; l++)
			{
				list6.Add(GetFileHash(list4[l]));
			}
			for (int m = 0; m < list3.Count; m++)
			{
				if (list5[m] == "")
				{
					continue;
				}
				for (int n = 0; n < list4.Count; n++)
				{
					if (!(list3[m] == list4[n]) && list5[m] == list6[n])
					{
						SelectImageWindow selectImageWindow = new SelectImageWindow();
						selectImageWindow.SetClassicMode(list1ID, list2ID);
						selectImageWindow.SetClassicImage(list3[m], list4[n]);
						selectImageWindow.ShowDialog();
						switch (selectImageWindow.GetDeleteResult())
						{
						case 1:
							list6[n] = "";
							list4[n] = "";
							break;
						case 2:
							list5[m] = "";
							list3[m] = "";
							n = list4.Count;
							break;
						}
					}
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "AIP01-FontToolWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
			return result;
		}
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
	private bool UpdateIni(string fontchar)
	{
		try
		{
			string text = string.Format("D:\\AI_FONT_Train\\scoreOcr\\parameter.ini");
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			HiniFile hiniFile = new HiniFile(text, ";");
			hiniFile.WriteString("Mode", "ModelType", "CLASSIFY");
			hiniFile.WriteInt("Mode", "ShowWindow", 1);
			hiniFile.WriteString("Mode", "Command", "TRAIN");
			hiniFile.WriteInt("Parameter", "ImgHeight", 32);
			hiniFile.WriteInt("Parameter", "ImgWidth", 16);
			hiniFile.WriteInt("Parameter", "Epoch", 200);
			hiniFile.WriteInt("Parameter", "BatchSize", 16);
			hiniFile.WriteDouble("Parameter", "LossBThreshold", 0.1);
			hiniFile.WriteDouble("Parameter", "SubtractThreshold", 0.2);
			hiniFile.WriteDouble("Parameter", "ReconstructWeight", 1.0);
			hiniFile.WriteDouble("Parameter", "RegressionWeight", 1.0);
			hiniFile.WriteString("Path", "ModelFileName", "ocr.pth");
			hiniFile.WriteString("Path", "RawPath", "");
			hiniFile.WriteString(value: string.Format("D:\\AI_FONT_Train\\train\\{0}", fontchar), section: "Path", key: "TrainPath");
			hiniFile.WriteString("Path", "TestPath", "");
			hiniFile.WriteDouble("Augmentation", "horizontal_flip", 0.0);
			hiniFile.WriteDouble("Augmentation", "vertical_flip", 0.0);
			hiniFile.WriteDouble("Augmentation", "brightness_limit", 0.2);
			hiniFile.WriteDouble("Augmentation", "contrast_limit", 0.2);
			hiniFile.WriteDouble("Augmentation", "scale_limit", 0.1);
			hiniFile.WriteDouble("Augmentation", "rotate_limit", 0.0);
			hiniFile.WriteDouble("Augmentation", "shift_limit", 0.0);
			hiniFile.WriteDouble("Augmentation", "dilate", 1.0);
			hiniFile.WriteDouble("Augmentation", "erode", 0.0);
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
			string text2 = "AIP01-FontToolWindow-UpdateIni()" + ex.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_UploadModelF1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid.Substring(0, 1));
		int num2 = (new int[5]
		{
			inspection.paramFile.FontToolParam.MinScore_Font1,
			inspection.paramFile.FontToolParam.MinScore_Font2,
			inspection.paramFile.FontToolParam.MinScore_Font3,
			inspection.paramFile.FontToolParam.MinScore_Font4,
			inspection.paramFile.FontToolParam.MinScore_Font5
		})[num];
		if (inspection.fontTool.FontModelOnnxType[num] == 1)
		{
			MessageBox.Show("不支援舊版AI程式訓練。", "警告");
			return;
		}
		if (CBAIModeFont[num].SelectedIndex <= 0)
		{
			MessageBox.Show("尚未選擇AImodel!");
			return;
		}
		if (!File.Exists(string.Format("D:\\AI_FONT_Train\\scoreOcr\\scoreOcr.exe")))
		{
			MessageBox.Show("找不到訓練文字程式!");
			return;
		}
		string text = CBAIModeFont[num].SelectedItem.ToString();
		string text2 = string.Format("{0}train\\{1}", BaseTrainFolder, text);
		CheckClassicImage(text2, updatelab: false);
		string[] files = Directory.GetFiles(text2, "*.bmp", SearchOption.AllDirectories);
		if (files.Length < 16)
		{
			MessageBox.Show(string.Format("訓練影像只有{0}張，小於最低訓練影像張數(16張)，請再增加訓練影像數量!", files.Length));
			return;
		}
		if (!UpdateIni(text))
		{
			MessageBox.Show("寫入parameter.ini錯誤!");
			return;
		}
		string text3 = string.Format("{0}\\ocr.onnx", text2);
		if (File.Exists(text3))
		{
			File.Delete(text3);
		}
		if (!RunTrainingScript())
		{
			return;
		}
		string text4 = "";
		string destFileName = string.Format(arg0: (!UsePublicFont) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"), format: "{0}{1}.onnx", arg1: text);
		if (!File.Exists(text3))
		{
			MessageBox.Show(string.Format("訓練失敗，找不到模型檔案。{0}", text3));
			return;
		}
		File.Copy(text3, destFileName, overwrite: true);
		File.Delete(text3);
		for (int i = 0; i <= 5; i++)
		{
			inspection.fontTool.FontModelOnnxType[i] = 0;
			inspection.fontTool.FontModelReaded[i] = false;
		}
		try
		{
			string modelname = string.Format("{0}.onnx", text);
			int Score = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			string path = text2 + "\\A\\";
			string path2 = text2 + "\\B\\";
			string path3 = text2 + "\\D\\";
			string path4 = text2 + "\\E\\";
			string path5 = text2 + "\\PASS\\";
			Mat mat = new Mat();
			int num8 = 0;
			string[] files2 = Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int j = 0; j < files2.Length; j++)
			{
				mat = CVBuffer.ImRead(files2[j], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, num, mat.Clone(), mat, ref Score);
					if (Score < num2)
					{
						num3++;
					}
					num8++;
				}
			}
			files2 = Directory.GetFiles(path2, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int k = 0; k < files2.Length; k++)
			{
				mat = CVBuffer.ImRead(files2[k], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, num, mat.Clone(), mat, ref Score);
					if (Score < num2)
					{
						num4++;
					}
					num8++;
				}
			}
			files2 = Directory.GetFiles(path5, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int l = 0; l < files2.Length; l++)
			{
				mat = CVBuffer.ImRead(files2[l], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, num, mat.Clone(), mat, ref Score);
					if (Score < num2)
					{
						num5++;
					}
					num8++;
				}
			}
			files2 = Directory.GetFiles(path3, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int m = 0; m < files2.Length; m++)
			{
				mat = CVBuffer.ImRead(files2[m], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, num, mat.Clone(), mat, ref Score);
					if (Score >= num2)
					{
						num6++;
					}
					num8++;
				}
			}
			files2 = Directory.GetFiles(path4, "*.bmp", SearchOption.TopDirectoryOnly);
			for (int n = 0; n < files2.Length; n++)
			{
				mat = CVBuffer.ImRead(files2[n], ImreadModes.Grayscale);
				if (mat.Width > 0 && mat.Height > 0)
				{
					inspection.fontTool.GetAIMatchScore(modelname, num, mat.Clone(), mat, ref Score);
					if (Score >= num2)
					{
						num7++;
					}
					num8++;
				}
			}
			if (ShowMessage)
			{
				MessageBox.Show(string.Format("模型更新成功。訓練資料庫測試結果，\n\n總測試張數 = {0} \n[A] 誤判 = {1} \n[B] 誤判 = {2} \n[PASS] 誤判 = {3} \n[D] 漏判 = {4} \n[E] 漏判 = {5}", num8, num3, num4, num5, num6, num7), "注意");
			}
			Btn_Test_Click(this, null);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIP01-FontToolWindow-UpdateIni()" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(string.Format("模型更新成功，回測結果失敗，請檢查是否有影像不符合規格"));
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool RunTrainingScript()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		string text = "";
		string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "TrainFont.bat");
		processStartInfo.FileName = "TrainFont.bat";
		text = string.Format("D:\ncd\\ \ncd AI_FONT_Train\ncd scoreOcr\nscoreOcr.exe -s 0 -r Train");
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
			MessageBox.Show("訓練失敗，找不到訓練批次程式(TrainFont.bat)。", "警告");
			return false;
		}
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = false;
		Process process = Process.Start(processStartInfo);
		process.WaitForExit();
		process.Close();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Add2TrainAF1_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		int num = Convert.ToInt32(obj.Uid.Substring(0, 1));
		string classic = obj.Uid.Substring(1, 1);
		if (CBAIModeFont[num].SelectedIndex <= 0)
		{
			MessageBox.Show("尚未選擇AImodel!");
			return;
		}
		try
		{
			string text = CBAIModeFont[num].SelectedItem.ToString();
			if (inspection.fontTool.OcrMat1[num].Width <= 0 || inspection.fontTool.OcrMat1[num].Height <= 0)
			{
				MessageBox.Show("影像讀取錯誤!");
				return;
			}
			inspection.fontTool.OcrMat1[num].CopyTo(CurrentMat);
			AddImage2TrainDB(classic, BaseTrainFolder + "train\\" + text);
			string path = string.Format("{0}train\\{1}", BaseTrainFolder, text);
			CheckClassicImage(path, updatelab: false);
		}
		catch (Exception ex)
		{
			string text2 = "TW01-FontToolWindow-btn_Add2TrainAF1_Click()";
			text2 += ex.ToString();
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, LanguageManager.Instance["Error"]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_CreateAIMode1F1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		try
		{
			int num = Convert.ToInt32(button.Uid);
			string text = "";
			text = ((!UsePublicFont) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, inspection.RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			string text2 = inspection.paramFile.FontToolParam.AllFont.Substring(num, 1);
			if (text2 == "")
			{
				return;
			}
			string text3 = string.Format("{0}{1}.onnx", text, text2);
			if (File.Exists(text3))
			{
				return;
			}
			string text4 = string.Format("{0}ocr.onnx", BaseTrainFolder);
			if (!File.Exists(text4))
			{
				return;
			}
			File.Copy(text4, text3, overwrite: true);
			BTNCreateAIModel[num].Visibility = Visibility.Hidden;
			for (int i = 0; i < inspection.paramFile.FontToolParam.AllFont.Length; i++)
			{
				UpdateAIModelComboBox(CBAIModeFont[i]);
				int selectedIndex = 0;
				if (i == num)
				{
					string onnxfile = string.Format("{0}.onnx", inspection.paramFile.FontToolParam.AllFont.Substring(i, 1));
					selectedIndex = GetAIModelIndex(onnxfile);
					CBAIModeFont[i].SelectedIndex = selectedIndex;
					continue;
				}
				switch (i)
				{
				case 0:
					selectedIndex = GetAIModelIndex(inspection.paramFile.FontToolParam.AIModel_Font1);
					break;
				case 1:
					selectedIndex = GetAIModelIndex(inspection.paramFile.FontToolParam.AIModel_Font2);
					break;
				case 2:
					selectedIndex = GetAIModelIndex(inspection.paramFile.FontToolParam.AIModel_Font3);
					break;
				case 3:
					selectedIndex = GetAIModelIndex(inspection.paramFile.FontToolParam.AIModel_Font4);
					break;
				case 4:
					selectedIndex = GetAIModelIndex(inspection.paramFile.FontToolParam.AIModel_Font5);
					break;
				}
				CBAIModeFont[i].SelectedIndex = selectedIndex;
			}
			string text5 = string.Format("{0}train\\{1}\\", BaseTrainFolder, text2);
			if (!Directory.Exists(text5))
			{
				Directory.CreateDirectory(text5);
			}
			string path = string.Format("{0}A\\", text5);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string path2 = string.Format("{0}B\\", text5);
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
			}
			string path3 = string.Format("{0}C\\", text5);
			if (!Directory.Exists(path3))
			{
				Directory.CreateDirectory(path3);
			}
			string path4 = string.Format("{0}D\\", text5);
			if (!Directory.Exists(path4))
			{
				Directory.CreateDirectory(path4);
			}
			string path5 = string.Format("{0}E\\", text5);
			if (!Directory.Exists(path5))
			{
				Directory.CreateDirectory(path5);
			}
			string path6 = string.Format("{0}PASS\\", text5);
			if (!Directory.Exists(path6))
			{
				Directory.CreateDirectory(path6);
			}
		}
		catch (Exception ex)
		{
			string text6 = "AIP01-FontToolWindow-btn_CreateAIMode1F1_Click()" + ex.ToString();
			CVBuffer.SaveLog(text6, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text6, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Add2TrainAF1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button obj = (Button)sender;
		int num = Convert.ToInt32(obj.Uid.Substring(0, 1));
		string arg = obj.Uid.Substring(1, 1);
		if (CBAIModeFont[num].SelectedIndex <= 0)
		{
			MessageBox.Show("尚未選擇AImodel");
			return;
		}
		try
		{
			string arg2 = CBAIModeFont[num].SelectedItem.ToString();
			string text = string.Format("{0}train\\{1}\\{2}\\", BaseTrainFolder, arg2, arg);
			string fileName = "c:\\windows\\explorer.exe";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arguments = string.Format("/root, {0}", text);
			Process.Start(fileName, arguments);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("FontToolWindow-btn_Add2TrainAF1_PreviewMouseRightButtonDown()" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_UploadModelF1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (num < 0 || num >= 5)
		{
			return;
		}
		string arg = CBAIModeFont[num].SelectedItem.ToString();
		string text = string.Format("D:\\AI_FONT_Train\\train\\{0}", arg);
		try
		{
			string fileName = "c:\\windows\\explorer.exe";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arguments = string.Format("/root, {0}", text);
			Process.Start(fileName, arguments);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("FontToolWindow-btn_UploadModelF1_PreviewMouseRightButtonDown()" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AutoPutFont_Click(object sender, RoutedEventArgs e)
	{
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		string ocrChar = "";
		if (GetPatternTracker(ref rect))
		{
			Mat mat = new Mat(inspection.buffers.GetChannelMat(inspection.paramFile.FontToolParam.PatternChannel), rect).Clone();
			if (inspection.fontTool.FontIsBlack)
			{
				Cv2.BitwiseNot(mat, mat);
			}
			inspection.fontTool.GetAIOCRResult(mat, ref ocrChar);
		}
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.SetTitle("請輸入字元 (檢查字元是否正確)");
		itemInputWindow.SetText(ocrChar);
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr.Length <= 0 || itemInputWindow.InputStr.Length > 1)
		{
			MessageBox.Show("字元數量異常，請重新輸入合法字元。(A~Z, 0~9)");
			return;
		}
		if (!Regex.IsMatch(itemInputWindow.InputStr, "^[a-zA-Z0-9]*$") && itemInputWindow.InputStr != "-")
		{
			MessageBox.Show("輸入不合法字元，請重新輸入。(A~Z, 0~9)");
			return;
		}
		string fontChar = string.Format("{0}_", itemInputWindow.InputStr.ToUpper());
		AddFontImage(fontChar, "0");
		ShowLabMessage("文字資料庫增加完成");
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

	public string GetTrainDir()
	{
		return TrainDir;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetTrainDir(string path)
	{
		TrainDir = path;
		try
		{
			if (Directory.Exists(TrainDir) && CheckTrainFolder(TrainDir))
			{
				btn_SetTrainFolder.Background = Brushes.LightGoldenrodYellow;
				btn_Add2TrainA.IsEnabled = true;
				btn_Add2TrainB.IsEnabled = true;
				btn_Add2TrainD.IsEnabled = true;
				btn_Add2TrainE.IsEnabled = true;
				btn_UploadModel.IsEnabled = true;
				cb_FontSelectTest.SelectedIndex = inspection.fontTool.GetFontIndex(TrainDir.Substring(TrainDir.Length - 1));
				CheckClassicImage(TrainDir);
			}
		}
		catch (Exception ex)
		{
			string text = "FTW01-FontToolWindow-SetTrainDir()";
			text += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
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
			Uri resourceLocator = new Uri("/InspTools;component/aoi/fonttoolwindow.xaml", UriKind.Relative);
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
			MainW = (FontToolWindow)target;
			MainW.Closed += Window_Closed;
			MainW.Loaded += Window_Loaded;
			break;
		case 2:
			grd_Main = (Grid)target;
			break;
		case 3:
			cnv_Image = (Canvas)target;
			cnv_Image.PreviewMouseMove += Cnv_Image_PreviewMouseMove;
			cnv_Image.MouseWheel += Cnv_Image_MouseWheel;
			break;
		case 4:
			sfr = (ScaleTransform)target;
			break;
		case 5:
			line_X1 = (Line)target;
			break;
		case 6:
			line_Y1 = (Line)target;
			break;
		case 7:
			line_X2 = (Line)target;
			break;
		case 8:
			line_Y2 = (Line)target;
			break;
		case 9:
			bor_LocateRect = (Border)target;
			break;
		case 10:
			bor_SearchRect1 = (Border)target;
			bor_SearchRect1.MouseEnter += bor_SearchRect1_MouseEnter;
			break;
		case 11:
			txt_Search1Label = (TextBlock)target;
			break;
		case 12:
			bor_PatternSet = (Rectangle)target;
			break;
		case 13:
			Label_Result = (Label)target;
			break;
		case 14:
			lab_FaceIndex = (Label)target;
			break;
		case 15:
			txt_PreviewMode = (TextBox)target;
			break;
		case 16:
			lab_FontAlignVertical = (Label)target;
			break;
		case 17:
			tab_Function = (TabControl)target;
			tab_Function.SelectionChanged += Tab_Function_SelectionChanged;
			break;
		case 18:
			tab_Main = (TabItem)target;
			break;
		case 19:
			img_Pattern = (Image)target;
			break;
		case 20:
			cb_PatternSelect = (ComboBox)target;
			cb_PatternSelect.SelectionChanged += Cb_PatternSelect_SelectionChanged;
			break;
		case 21:
			btn_AutoSetFont = (Button)target;
			btn_AutoSetFont.Click += Btn_AutoSetFont_Click;
			break;
		case 22:
			grd_Font_1 = (Grid)target;
			break;
		case 23:
			cb_AIModelFont1 = (ComboBox)target;
			cb_AIModelFont1.SelectionChanged += cb_AIModelFont1_SelectionChanged;
			break;
		case 24:
			txt_MatchScoreFont1 = (Label)target;
			break;
		case 25:
			txt_AcceptScoreFont1 = (TextBox)target;
			break;
		case 26:
			ico_ResultOKFont1 = (Label)target;
			break;
		case 27:
			ico_ResultAlartFont1 = (Label)target;
			break;
		case 28:
			ico_ResultNGFont1 = (Label)target;
			break;
		case 29:
			chk_DoubleWidthFont1 = (CheckBox)target;
			break;
		case 30:
			lab_AIScoreF11 = (Label)target;
			break;
		case 31:
			lab_AIScoreF12 = (Label)target;
			break;
		case 32:
			lab_AIScoreF13 = (Label)target;
			break;
		case 33:
			lab_AIScoreF14 = (Label)target;
			break;
		case 34:
			lab_AIScoreF15 = (Label)target;
			break;
		case 35:
			chk_CheckFont1 = (CheckBox)target;
			break;
		case 36:
			btn_Font_1 = (Button)target;
			btn_Font_1.Click += Btn_R_Setup_Click;
			break;
		case 37:
			txt_WExtendFont1 = (TextBox)target;
			break;
		case 38:
			txt_HExtendFont1 = (TextBox)target;
			break;
		case 39:
			btn_ShowExtend1 = (Button)target;
			btn_ShowExtend1.Click += Btn_ShowExtend1_Click;
			break;
		case 40:
			chk_BypassOCR1 = (CheckBox)target;
			break;
		case 41:
			btn_AI_Mode_Reload1 = (Button)target;
			btn_AI_Mode_Reload1.Click += btn_AI_Mode_Reload_Click;
			break;
		case 42:
			btn_CreateAIMode1F1 = (Button)target;
			btn_CreateAIMode1F1.Click += btn_CreateAIMode1F1_Click;
			break;
		case 43:
			rt_Font_1 = (Rectangle)target;
			break;
		case 44:
			txt_InputText = (TextBox)target;
			break;
		case 45:
			grd_Font_2 = (Grid)target;
			break;
		case 46:
			cb_AIModelFont2 = (ComboBox)target;
			cb_AIModelFont2.SelectionChanged += cb_AIModelFont1_SelectionChanged;
			break;
		case 47:
			txt_MatchScoreFont2 = (Label)target;
			break;
		case 48:
			txt_AcceptScoreFont2 = (TextBox)target;
			break;
		case 49:
			ico_ResultAlartFont2 = (Label)target;
			break;
		case 50:
			ico_ResultOKFont2 = (Label)target;
			break;
		case 51:
			ico_ResultNGFont2 = (Label)target;
			break;
		case 52:
			chk_DoubleWidthFont2 = (CheckBox)target;
			break;
		case 53:
			lab_AIScoreF21 = (Label)target;
			break;
		case 54:
			lab_AIScoreF22 = (Label)target;
			break;
		case 55:
			lab_AIScoreF23 = (Label)target;
			break;
		case 56:
			lab_AIScoreF24 = (Label)target;
			break;
		case 57:
			lab_AIScoreF25 = (Label)target;
			break;
		case 58:
			chk_CheckFont2 = (CheckBox)target;
			break;
		case 59:
			btn_Font_2 = (Button)target;
			btn_Font_2.Click += Btn_R_Setup_Click;
			break;
		case 60:
			txt_WExtendFont2 = (TextBox)target;
			break;
		case 61:
			txt_HExtendFont2 = (TextBox)target;
			break;
		case 62:
			btn_ShowExtend2 = (Button)target;
			btn_ShowExtend2.Click += Btn_ShowExtend1_Click;
			break;
		case 63:
			chk_BypassOCR2 = (CheckBox)target;
			break;
		case 64:
			btn_AI_Mode_Reload2 = (Button)target;
			btn_AI_Mode_Reload2.Click += btn_AI_Mode_Reload_Click;
			break;
		case 65:
			btn_CreateAIMode1F2 = (Button)target;
			btn_CreateAIMode1F2.Click += btn_CreateAIMode1F1_Click;
			break;
		case 66:
			rt_Font_2 = (Rectangle)target;
			break;
		case 67:
			grd_Font_3 = (Grid)target;
			break;
		case 68:
			cb_AIModelFont3 = (ComboBox)target;
			cb_AIModelFont3.SelectionChanged += cb_AIModelFont1_SelectionChanged;
			break;
		case 69:
			txt_MatchScoreFont3 = (Label)target;
			break;
		case 70:
			txt_AcceptScoreFont3 = (TextBox)target;
			break;
		case 71:
			ico_ResultAlartFont3 = (Label)target;
			break;
		case 72:
			ico_ResultOKFont3 = (Label)target;
			break;
		case 73:
			ico_ResultNGFont3 = (Label)target;
			break;
		case 74:
			lab_AIScoreF31 = (Label)target;
			break;
		case 75:
			lab_AIScoreF32 = (Label)target;
			break;
		case 76:
			lab_AIScoreF33 = (Label)target;
			break;
		case 77:
			lab_AIScoreF34 = (Label)target;
			break;
		case 78:
			lab_AIScoreF35 = (Label)target;
			break;
		case 79:
			chk_CheckFont3 = (CheckBox)target;
			break;
		case 80:
			btn_Font_3 = (Button)target;
			btn_Font_3.Click += Btn_R_Setup_Click;
			break;
		case 81:
			txt_WExtendFont3 = (TextBox)target;
			break;
		case 82:
			txt_HExtendFont3 = (TextBox)target;
			break;
		case 83:
			btn_ShowExtend3 = (Button)target;
			btn_ShowExtend3.Click += Btn_ShowExtend1_Click;
			break;
		case 84:
			chk_BypassOCR3 = (CheckBox)target;
			break;
		case 85:
			btn_AI_Mode_Reload3 = (Button)target;
			btn_AI_Mode_Reload3.Click += btn_AI_Mode_Reload_Click;
			break;
		case 86:
			btn_CreateAIMode1F3 = (Button)target;
			btn_CreateAIMode1F3.Click += btn_CreateAIMode1F1_Click;
			break;
		case 87:
			rt_Font_3 = (Rectangle)target;
			break;
		case 88:
			grd_Font_4 = (Grid)target;
			break;
		case 89:
			cb_AIModelFont4 = (ComboBox)target;
			cb_AIModelFont4.SelectionChanged += cb_AIModelFont1_SelectionChanged;
			break;
		case 90:
			txt_MatchScoreFont4 = (Label)target;
			break;
		case 91:
			txt_AcceptScoreFont4 = (TextBox)target;
			break;
		case 92:
			ico_ResultAlartFont4 = (Label)target;
			break;
		case 93:
			ico_ResultOKFont4 = (Label)target;
			break;
		case 94:
			ico_ResultNGFont4 = (Label)target;
			break;
		case 95:
			lab_AIScoreF41 = (Label)target;
			break;
		case 96:
			lab_AIScoreF42 = (Label)target;
			break;
		case 97:
			lab_AIScoreF43 = (Label)target;
			break;
		case 98:
			lab_AIScoreF44 = (Label)target;
			break;
		case 99:
			lab_AIScoreF45 = (Label)target;
			break;
		case 100:
			chk_CheckFont4 = (CheckBox)target;
			break;
		case 101:
			btn_Font_4 = (Button)target;
			btn_Font_4.Click += Btn_R_Setup_Click;
			break;
		case 102:
			txt_WExtendFont4 = (TextBox)target;
			break;
		case 103:
			txt_HExtendFont4 = (TextBox)target;
			break;
		case 104:
			btn_ShowExtend4 = (Button)target;
			btn_ShowExtend4.Click += Btn_ShowExtend1_Click;
			break;
		case 105:
			chk_BypassOCR4 = (CheckBox)target;
			break;
		case 106:
			btn_AI_Mode_Reload4 = (Button)target;
			btn_AI_Mode_Reload4.Click += btn_AI_Mode_Reload_Click;
			break;
		case 107:
			btn_CreateAIMode1F4 = (Button)target;
			btn_CreateAIMode1F4.Click += btn_CreateAIMode1F1_Click;
			break;
		case 108:
			rt_Font_4 = (Rectangle)target;
			break;
		case 109:
			grd_Font_5 = (Grid)target;
			break;
		case 110:
			cb_AIModelFont5 = (ComboBox)target;
			cb_AIModelFont5.SelectionChanged += cb_AIModelFont1_SelectionChanged;
			break;
		case 111:
			txt_MatchScoreFont5 = (Label)target;
			break;
		case 112:
			txt_AcceptScoreFont5 = (TextBox)target;
			break;
		case 113:
			ico_ResultAlartFont5 = (Label)target;
			break;
		case 114:
			ico_ResultOKFont5 = (Label)target;
			break;
		case 115:
			ico_ResultNGFont5 = (Label)target;
			break;
		case 116:
			lab_AIScoreF51 = (Label)target;
			break;
		case 117:
			lab_AIScoreF52 = (Label)target;
			break;
		case 118:
			lab_AIScoreF53 = (Label)target;
			break;
		case 119:
			lab_AIScoreF54 = (Label)target;
			break;
		case 120:
			lab_AIScoreF55 = (Label)target;
			break;
		case 121:
			chk_CheckFont5 = (CheckBox)target;
			break;
		case 122:
			btn_Font_5 = (Button)target;
			btn_Font_5.Click += Btn_R_Setup_Click;
			break;
		case 123:
			txt_WExtendFont5 = (TextBox)target;
			break;
		case 124:
			txt_HExtendFont5 = (TextBox)target;
			break;
		case 125:
			btn_ShowExtend5 = (Button)target;
			btn_ShowExtend5.Click += Btn_ShowExtend1_Click;
			break;
		case 126:
			chk_BypassOCR5 = (CheckBox)target;
			break;
		case 127:
			btn_AI_Mode_Reload5 = (Button)target;
			btn_AI_Mode_Reload5.Click += btn_AI_Mode_Reload_Click;
			break;
		case 128:
			btn_CreateAIMode1F5 = (Button)target;
			btn_CreateAIMode1F5.Click += btn_CreateAIMode1F1_Click;
			break;
		case 129:
			rt_Font_5 = (Rectangle)target;
			break;
		case 130:
			rt_Result = (Rectangle)target;
			break;
		case 131:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 132:
			grd_SelectFace = (Grid)target;
			break;
		case 133:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 134:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 135:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 136:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 137:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 138:
			lab_Pareto = (Label)target;
			break;
		case 139:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 140:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			btn_Test.MouseRightButtonDown += Btn_Test_MouseRightButtonDown;
			break;
		case 141:
			lab_Time = (Label)target;
			break;
		case 142:
			chk_ShowSearch = (CheckBox)target;
			break;
		case 143:
			chk_ShowMatch = (CheckBox)target;
			break;
		case 144:
			chk_ShowMatchAll = (CheckBox)target;
			break;
		case 145:
			grd_EngTest = (Grid)target;
			break;
		case 146:
			btn_Test0 = (Button)target;
			btn_Test0.Click += Btn_Test0_Click;
			break;
		case 147:
			chk_CheckOK = (CheckBox)target;
			break;
		case 148:
			txt_font = (TextBox)target;
			break;
		case 149:
			chk_FontAlignVertical = (CheckBox)target;
			chk_FontAlignVertical.MouseEnter += chk_FontAlignVertical_MouseEnter;
			chk_FontAlignVertical.MouseLeave += chk_FontAlignVertical_MouseLeave;
			break;
		case 150:
			chk_EnabledOCR = (CheckBox)target;
			break;
		case 151:
			chk_AutoLoadAI = (CheckBox)target;
			break;
		case 152:
			chk_FontConnected = (CheckBox)target;
			break;
		case 153:
			cnv_Font1 = (Canvas)target;
			break;
		case 154:
			cnv_Font2 = (Canvas)target;
			break;
		case 155:
			cnv_Font3 = (Canvas)target;
			break;
		case 156:
			cnv_Font4 = (Canvas)target;
			break;
		case 157:
			cnv_Font5 = (Canvas)target;
			break;
		case 158:
			btn_FontConnectSetup = (Button)target;
			btn_FontConnectSetup.Click += Btn_R_Setup_Click;
			break;
		case 159:
			lab_VerAlign = (Label)target;
			break;
		case 160:
			grd_TrainF1 = (Grid)target;
			break;
		case 161:
			btn_Add2TrainAF1 = (Button)target;
			btn_Add2TrainAF1.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainAF1.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 162:
			btn_Add2TrainBF1 = (Button)target;
			btn_Add2TrainBF1.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainBF1.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 163:
			btn_Add2TrainDF1 = (Button)target;
			btn_Add2TrainDF1.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainDF1.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 164:
			btn_Add2TrainEF1 = (Button)target;
			btn_Add2TrainEF1.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainEF1.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 165:
			btn_UploadModelF1 = (Button)target;
			btn_UploadModelF1.Click += btn_UploadModelF1_Click;
			btn_UploadModelF1.PreviewMouseRightButtonDown += btn_UploadModelF1_PreviewMouseRightButtonDown;
			break;
		case 166:
			grd_TrainF2 = (Grid)target;
			break;
		case 167:
			btn_Add2TrainAF2 = (Button)target;
			btn_Add2TrainAF2.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainAF2.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 168:
			btn_Add2TrainBF2 = (Button)target;
			btn_Add2TrainBF2.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainBF2.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 169:
			btn_Add2TrainDF2 = (Button)target;
			btn_Add2TrainDF2.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainDF2.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 170:
			btn_Add2TrainEF2 = (Button)target;
			btn_Add2TrainEF2.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainEF2.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 171:
			btn_UploadModelF2 = (Button)target;
			btn_UploadModelF2.Click += btn_UploadModelF1_Click;
			btn_UploadModelF2.PreviewMouseRightButtonDown += btn_UploadModelF1_PreviewMouseRightButtonDown;
			break;
		case 172:
			grd_TrainF3 = (Grid)target;
			break;
		case 173:
			btn_Add2TrainAF3 = (Button)target;
			btn_Add2TrainAF3.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainAF3.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 174:
			btn_Add2TrainBF3 = (Button)target;
			btn_Add2TrainBF3.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainBF3.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 175:
			btn_Add2TrainDF3 = (Button)target;
			btn_Add2TrainDF3.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainDF3.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 176:
			btn_Add2TrainEF3 = (Button)target;
			btn_Add2TrainEF3.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainEF3.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 177:
			btn_UploadModelF3 = (Button)target;
			btn_UploadModelF3.Click += btn_UploadModelF1_Click;
			btn_UploadModelF3.PreviewMouseRightButtonDown += btn_UploadModelF1_PreviewMouseRightButtonDown;
			break;
		case 178:
			grd_TrainF4 = (Grid)target;
			break;
		case 179:
			btn_Add2TrainAF4 = (Button)target;
			btn_Add2TrainAF4.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainAF4.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 180:
			btn_Add2TrainBF4 = (Button)target;
			btn_Add2TrainBF4.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainBF4.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 181:
			btn_Add2TrainDF4 = (Button)target;
			btn_Add2TrainDF4.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainDF4.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 182:
			btn_Add2TrainEF4 = (Button)target;
			btn_Add2TrainEF4.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainEF4.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 183:
			btn_UploadModelF4 = (Button)target;
			btn_UploadModelF4.Click += btn_UploadModelF1_Click;
			btn_UploadModelF4.PreviewMouseRightButtonDown += btn_UploadModelF1_PreviewMouseRightButtonDown;
			break;
		case 184:
			grd_TrainF5 = (Grid)target;
			break;
		case 185:
			btn_Add2TrainAF5 = (Button)target;
			btn_Add2TrainAF5.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainAF5.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 186:
			btn_Add2TrainBF5 = (Button)target;
			btn_Add2TrainBF5.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainBF5.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 187:
			btn_Add2TrainDF5 = (Button)target;
			btn_Add2TrainDF5.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainDF5.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 188:
			btn_Add2TrainEF5 = (Button)target;
			btn_Add2TrainEF5.Click += btn_Add2TrainAF1_Click;
			btn_Add2TrainEF5.PreviewMouseRightButtonDown += btn_Add2TrainAF1_PreviewMouseRightButtonDown;
			break;
		case 189:
			btn_UploadModelF5 = (Button)target;
			btn_UploadModelF5.Click += btn_UploadModelF1_Click;
			btn_UploadModelF5.PreviewMouseRightButtonDown += btn_UploadModelF1_PreviewMouseRightButtonDown;
			break;
		case 190:
			tab_PublicDB = (TabItem)target;
			break;
		case 191:
			grd_PublicDB = (Grid)target;
			break;
		case 192:
			gb_BasicSetup_Copy1 = (GroupBox)target;
			break;
		case 193:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 194:
			btn_ChannelR1 = (Button)target;
			btn_ChannelR1.Click += Btn_ChannelR_Click;
			break;
		case 195:
			btn_ChannelG1 = (Button)target;
			btn_ChannelG1.Click += Btn_ChannelR_Click;
			break;
		case 196:
			btn_ChannelB1 = (Button)target;
			btn_ChannelB1.Click += Btn_ChannelR_Click;
			break;
		case 197:
			btn_ChannelH1 = (Button)target;
			btn_ChannelH1.Click += Btn_ChannelR_Click;
			break;
		case 198:
			btn_ChannelS1 = (Button)target;
			btn_ChannelS1.Click += Btn_ChannelR_Click;
			break;
		case 199:
			btn_ChannelV1 = (Button)target;
			btn_ChannelV1.Click += Btn_ChannelR_Click;
			break;
		case 200:
			btn_ChannelGray1 = (Button)target;
			btn_ChannelGray1.Click += Btn_ChannelR_Click;
			break;
		case 201:
			btn_ChannelC11 = (Button)target;
			btn_ChannelC11.Click += Btn_ChannelR_Click;
			break;
		case 202:
			btn_ChannelC21 = (Button)target;
			btn_ChannelC21.Click += Btn_ChannelR_Click;
			break;
		case 203:
			btn_ChannelC31 = (Button)target;
			btn_ChannelC31.Click += Btn_ChannelR_Click;
			break;
		case 204:
			btn_ChannelRemove1 = (Button)target;
			btn_ChannelRemove1.Click += Btn_ChannelR_Click;
			break;
		case 205:
			btn_PublicPagePrev = (Button)target;
			btn_PublicPagePrev.Click += Btn_PublicPagePrev_Click;
			break;
		case 206:
			lab_PublicDBPage = (Label)target;
			break;
		case 207:
			btn_PublicPageNext = (Button)target;
			btn_PublicPageNext.Click += Btn_PublicPageNext_Click;
			break;
		case 208:
			rad_PDByRecipe = (RadioButton)target;
			rad_PDByRecipe.Click += Rad_PDByRecipe_Click;
			break;
		case 209:
			rad_PDByPublic = (RadioButton)target;
			rad_PDByPublic.Click += Rad_PDByRecipe_Click;
			break;
		case 210:
			grd_PPD1 = (Grid)target;
			break;
		case 211:
			gd_pdb001 = (Grid)target;
			break;
		case 212:
			txt_pbd001 = (TextBox)target;
			break;
		case 213:
			img_pbd001 = (Image)target;
			img_pbd001.Drop += Img_pbd_Drop;
			break;
		case 214:
			lab_pbd001 = (Label)target;
			break;
		case 215:
			btn_pbd001 = (Button)target;
			btn_pbd001.Click += Btn_SetPBDPattern_Click;
			break;
		case 216:
			gd_pdb002 = (Grid)target;
			break;
		case 217:
			txt_pbd002 = (TextBox)target;
			break;
		case 218:
			lab_pbd002 = (Label)target;
			break;
		case 219:
			btn_pbd002 = (Button)target;
			btn_pbd002.Click += Btn_SetPBDPattern_Click;
			break;
		case 220:
			img_pbd002 = (Image)target;
			img_pbd002.Drop += Img_pbd_Drop;
			break;
		case 221:
			gd_pdb003 = (Grid)target;
			break;
		case 222:
			txt_pbd003 = (TextBox)target;
			break;
		case 223:
			lab_pbd003 = (Label)target;
			break;
		case 224:
			btn_pbd003 = (Button)target;
			btn_pbd003.Click += Btn_SetPBDPattern_Click;
			break;
		case 225:
			img_pbd003 = (Image)target;
			img_pbd003.Drop += Img_pbd_Drop;
			break;
		case 226:
			gd_pdb004 = (Grid)target;
			break;
		case 227:
			txt_pbd004 = (TextBox)target;
			break;
		case 228:
			lab_pbd004 = (Label)target;
			break;
		case 229:
			btn_pbd004 = (Button)target;
			btn_pbd004.Click += Btn_SetPBDPattern_Click;
			break;
		case 230:
			img_pbd004 = (Image)target;
			img_pbd004.Drop += Img_pbd_Drop;
			break;
		case 231:
			gd_pdb005 = (Grid)target;
			break;
		case 232:
			txt_pbd005 = (TextBox)target;
			break;
		case 233:
			lab_pbd005 = (Label)target;
			break;
		case 234:
			btn_pbd005 = (Button)target;
			btn_pbd005.Click += Btn_SetPBDPattern_Click;
			break;
		case 235:
			img_pbd005 = (Image)target;
			img_pbd005.Drop += Img_pbd_Drop;
			break;
		case 236:
			gd_pdb006 = (Grid)target;
			break;
		case 237:
			txt_pbd006 = (TextBox)target;
			break;
		case 238:
			lab_pbd006 = (Label)target;
			break;
		case 239:
			btn_pbd006 = (Button)target;
			btn_pbd006.Click += Btn_SetPBDPattern_Click;
			break;
		case 240:
			img_pbd006 = (Image)target;
			img_pbd006.Drop += Img_pbd_Drop;
			break;
		case 241:
			gd_pdb007 = (Grid)target;
			break;
		case 242:
			txt_pbd007 = (TextBox)target;
			break;
		case 243:
			lab_pbd007 = (Label)target;
			break;
		case 244:
			btn_pbd007 = (Button)target;
			btn_pbd007.Click += Btn_SetPBDPattern_Click;
			break;
		case 245:
			img_pbd007 = (Image)target;
			img_pbd007.Drop += Img_pbd_Drop;
			break;
		case 246:
			gd_pdb008 = (Grid)target;
			break;
		case 247:
			txt_pbd008 = (TextBox)target;
			break;
		case 248:
			lab_pbd008 = (Label)target;
			break;
		case 249:
			btn_pbd008 = (Button)target;
			btn_pbd008.Click += Btn_SetPBDPattern_Click;
			break;
		case 250:
			img_pbd008 = (Image)target;
			img_pbd008.Drop += Img_pbd_Drop;
			break;
		case 251:
			gd_pdb009 = (Grid)target;
			break;
		case 252:
			txt_pbd009 = (TextBox)target;
			break;
		case 253:
			lab_pbd009 = (Label)target;
			break;
		case 254:
			btn_pbd009 = (Button)target;
			btn_pbd009.Click += Btn_SetPBDPattern_Click;
			break;
		case 255:
			img_pbd009 = (Image)target;
			img_pbd009.Drop += Img_pbd_Drop;
			break;
		case 256:
			gd_pdb010 = (Grid)target;
			break;
		case 257:
			txt_pbd010 = (TextBox)target;
			break;
		case 258:
			lab_pbd010 = (Label)target;
			break;
		case 259:
			btn_pbd010 = (Button)target;
			btn_pbd010.Click += Btn_SetPBDPattern_Click;
			break;
		case 260:
			img_pbd010 = (Image)target;
			img_pbd010.Drop += Img_pbd_Drop;
			break;
		case 261:
			grd_PPD2 = (Grid)target;
			break;
		case 262:
			gd_pdb011 = (Grid)target;
			break;
		case 263:
			txt_pbd011 = (TextBox)target;
			break;
		case 264:
			img_pbd011 = (Image)target;
			img_pbd011.Drop += Img_pbd_Drop;
			break;
		case 265:
			lab_pbd011 = (Label)target;
			break;
		case 266:
			btn_pbd011 = (Button)target;
			btn_pbd011.Click += Btn_SetPBDPattern_Click;
			break;
		case 267:
			gd_pdb012 = (Grid)target;
			break;
		case 268:
			txt_pbd012 = (TextBox)target;
			break;
		case 269:
			lab_pbd012 = (Label)target;
			break;
		case 270:
			btn_pbd012 = (Button)target;
			btn_pbd012.Click += Btn_SetPBDPattern_Click;
			break;
		case 271:
			img_pbd012 = (Image)target;
			img_pbd012.Drop += Img_pbd_Drop;
			break;
		case 272:
			gd_pdb013 = (Grid)target;
			break;
		case 273:
			txt_pbd013 = (TextBox)target;
			break;
		case 274:
			lab_pbd013 = (Label)target;
			break;
		case 275:
			btn_pbd013 = (Button)target;
			btn_pbd013.Click += Btn_SetPBDPattern_Click;
			break;
		case 276:
			img_pbd013 = (Image)target;
			img_pbd013.Drop += Img_pbd_Drop;
			break;
		case 277:
			gd_pdb014 = (Grid)target;
			break;
		case 278:
			txt_pbd014 = (TextBox)target;
			break;
		case 279:
			lab_pbd014 = (Label)target;
			break;
		case 280:
			btn_pbd014 = (Button)target;
			btn_pbd014.Click += Btn_SetPBDPattern_Click;
			break;
		case 281:
			img_pbd014 = (Image)target;
			img_pbd014.Drop += Img_pbd_Drop;
			break;
		case 282:
			gd_pdb015 = (Grid)target;
			break;
		case 283:
			txt_pbd015 = (TextBox)target;
			break;
		case 284:
			lab_pbd015 = (Label)target;
			break;
		case 285:
			btn_pbd015 = (Button)target;
			btn_pbd015.Click += Btn_SetPBDPattern_Click;
			break;
		case 286:
			img_pbd015 = (Image)target;
			img_pbd015.Drop += Img_pbd_Drop;
			break;
		case 287:
			gd_pdb016 = (Grid)target;
			break;
		case 288:
			txt_pbd016 = (TextBox)target;
			break;
		case 289:
			lab_pbd016 = (Label)target;
			break;
		case 290:
			btn_pbd016 = (Button)target;
			btn_pbd016.Click += Btn_SetPBDPattern_Click;
			break;
		case 291:
			img_pbd016 = (Image)target;
			img_pbd016.Drop += Img_pbd_Drop;
			break;
		case 292:
			gd_pdb017 = (Grid)target;
			break;
		case 293:
			txt_pbd017 = (TextBox)target;
			break;
		case 294:
			lab_pbd017 = (Label)target;
			break;
		case 295:
			btn_pbd017 = (Button)target;
			btn_pbd017.Click += Btn_SetPBDPattern_Click;
			break;
		case 296:
			img_pbd017 = (Image)target;
			img_pbd017.Drop += Img_pbd_Drop;
			break;
		case 297:
			gd_pdb018 = (Grid)target;
			break;
		case 298:
			txt_pbd018 = (TextBox)target;
			break;
		case 299:
			lab_pbd018 = (Label)target;
			break;
		case 300:
			btn_pbd018 = (Button)target;
			btn_pbd018.Click += Btn_SetPBDPattern_Click;
			break;
		case 301:
			img_pbd018 = (Image)target;
			img_pbd018.Drop += Img_pbd_Drop;
			break;
		case 302:
			gd_pdb019 = (Grid)target;
			break;
		case 303:
			txt_pbd019 = (TextBox)target;
			break;
		case 304:
			lab_pbd019 = (Label)target;
			break;
		case 305:
			btn_pbd019 = (Button)target;
			btn_pbd019.Click += Btn_SetPBDPattern_Click;
			break;
		case 306:
			img_pbd019 = (Image)target;
			img_pbd019.Drop += Img_pbd_Drop;
			break;
		case 307:
			gd_pdb020 = (Grid)target;
			break;
		case 308:
			txt_pbd020 = (TextBox)target;
			break;
		case 309:
			lab_pbd020 = (Label)target;
			break;
		case 310:
			btn_pbd020 = (Button)target;
			btn_pbd020.Click += Btn_SetPBDPattern_Click;
			break;
		case 311:
			img_pbd020 = (Image)target;
			img_pbd020.Drop += Img_pbd_Drop;
			break;
		case 312:
			grd_PPD3 = (Grid)target;
			break;
		case 313:
			gd_pdb021 = (Grid)target;
			break;
		case 314:
			txt_pbd021 = (TextBox)target;
			break;
		case 315:
			img_pbd021 = (Image)target;
			img_pbd021.Drop += Img_pbd_Drop;
			break;
		case 316:
			lab_pbd021 = (Label)target;
			break;
		case 317:
			btn_pbd021 = (Button)target;
			btn_pbd021.Click += Btn_SetPBDPattern_Click;
			break;
		case 318:
			gd_pdb022 = (Grid)target;
			break;
		case 319:
			txt_pbd022 = (TextBox)target;
			break;
		case 320:
			lab_pbd022 = (Label)target;
			break;
		case 321:
			btn_pbd022 = (Button)target;
			btn_pbd022.Click += Btn_SetPBDPattern_Click;
			break;
		case 322:
			img_pbd022 = (Image)target;
			img_pbd022.Drop += Img_pbd_Drop;
			break;
		case 323:
			gd_pdb023 = (Grid)target;
			break;
		case 324:
			txt_pbd023 = (TextBox)target;
			break;
		case 325:
			lab_pbd023 = (Label)target;
			break;
		case 326:
			btn_pbd023 = (Button)target;
			btn_pbd023.Click += Btn_SetPBDPattern_Click;
			break;
		case 327:
			img_pbd023 = (Image)target;
			img_pbd023.Drop += Img_pbd_Drop;
			break;
		case 328:
			gd_pdb024 = (Grid)target;
			break;
		case 329:
			txt_pbd024 = (TextBox)target;
			break;
		case 330:
			lab_pbd024 = (Label)target;
			break;
		case 331:
			btn_pbd024 = (Button)target;
			btn_pbd024.Click += Btn_SetPBDPattern_Click;
			break;
		case 332:
			img_pbd024 = (Image)target;
			img_pbd024.Drop += Img_pbd_Drop;
			break;
		case 333:
			gd_pdb025 = (Grid)target;
			break;
		case 334:
			txt_pbd025 = (TextBox)target;
			break;
		case 335:
			lab_pbd025 = (Label)target;
			break;
		case 336:
			btn_pbd025 = (Button)target;
			btn_pbd025.Click += Btn_SetPBDPattern_Click;
			break;
		case 337:
			img_pbd025 = (Image)target;
			img_pbd025.Drop += Img_pbd_Drop;
			break;
		case 338:
			gd_pdb026 = (Grid)target;
			break;
		case 339:
			txt_pbd026 = (TextBox)target;
			break;
		case 340:
			lab_pbd026 = (Label)target;
			break;
		case 341:
			btn_pbd026 = (Button)target;
			btn_pbd026.Click += Btn_SetPBDPattern_Click;
			break;
		case 342:
			img_pbd026 = (Image)target;
			img_pbd026.Drop += Img_pbd_Drop;
			break;
		case 343:
			gd_pdb027 = (Grid)target;
			break;
		case 344:
			txt_pbd027 = (TextBox)target;
			break;
		case 345:
			lab_pbd027 = (Label)target;
			break;
		case 346:
			btn_pbd027 = (Button)target;
			btn_pbd027.Click += Btn_SetPBDPattern_Click;
			break;
		case 347:
			img_pbd027 = (Image)target;
			img_pbd027.Drop += Img_pbd_Drop;
			break;
		case 348:
			gd_pdb028 = (Grid)target;
			break;
		case 349:
			txt_pbd028 = (TextBox)target;
			break;
		case 350:
			lab_pbd028 = (Label)target;
			break;
		case 351:
			btn_pbd028 = (Button)target;
			btn_pbd028.Click += Btn_SetPBDPattern_Click;
			break;
		case 352:
			img_pbd028 = (Image)target;
			img_pbd028.Drop += Img_pbd_Drop;
			break;
		case 353:
			gd_pdb029 = (Grid)target;
			break;
		case 354:
			txt_pbd029 = (TextBox)target;
			break;
		case 355:
			lab_pbd029 = (Label)target;
			break;
		case 356:
			btn_pbd029 = (Button)target;
			btn_pbd029.Click += Btn_SetPBDPattern_Click;
			break;
		case 357:
			img_pbd029 = (Image)target;
			img_pbd029.Drop += Img_pbd_Drop;
			break;
		case 358:
			gd_pdb030 = (Grid)target;
			break;
		case 359:
			txt_pbd030 = (TextBox)target;
			break;
		case 360:
			lab_pbd030 = (Label)target;
			break;
		case 361:
			btn_pbd030 = (Button)target;
			btn_pbd030.Click += Btn_SetPBDPattern_Click;
			break;
		case 362:
			img_pbd030 = (Image)target;
			img_pbd030.Drop += Img_pbd_Drop;
			break;
		case 363:
			grd_PPD4 = (Grid)target;
			break;
		case 364:
			gd_pdb031 = (Grid)target;
			break;
		case 365:
			txt_pbd031 = (TextBox)target;
			break;
		case 366:
			img_pbd031 = (Image)target;
			img_pbd031.Drop += Img_pbd_Drop;
			break;
		case 367:
			lab_pbd031 = (Label)target;
			break;
		case 368:
			btn_pbd031 = (Button)target;
			btn_pbd031.Click += Btn_SetPBDPattern_Click;
			break;
		case 369:
			gd_pdb032 = (Grid)target;
			break;
		case 370:
			txt_pbd032 = (TextBox)target;
			break;
		case 371:
			lab_pbd032 = (Label)target;
			break;
		case 372:
			btn_pbd032 = (Button)target;
			btn_pbd032.Click += Btn_SetPBDPattern_Click;
			break;
		case 373:
			img_pbd032 = (Image)target;
			img_pbd032.Drop += Img_pbd_Drop;
			break;
		case 374:
			gd_pdb033 = (Grid)target;
			break;
		case 375:
			txt_pbd033 = (TextBox)target;
			break;
		case 376:
			lab_pbd033 = (Label)target;
			break;
		case 377:
			btn_pbd033 = (Button)target;
			btn_pbd033.Click += Btn_SetPBDPattern_Click;
			break;
		case 378:
			img_pbd033 = (Image)target;
			img_pbd033.Drop += Img_pbd_Drop;
			break;
		case 379:
			gd_pdb034 = (Grid)target;
			break;
		case 380:
			txt_pbd034 = (TextBox)target;
			break;
		case 381:
			lab_pbd034 = (Label)target;
			break;
		case 382:
			btn_pbd034 = (Button)target;
			btn_pbd034.Click += Btn_SetPBDPattern_Click;
			break;
		case 383:
			img_pbd034 = (Image)target;
			img_pbd034.Drop += Img_pbd_Drop;
			break;
		case 384:
			gd_pdb035 = (Grid)target;
			break;
		case 385:
			txt_pbd035 = (TextBox)target;
			break;
		case 386:
			lab_pbd035 = (Label)target;
			break;
		case 387:
			btn_pbd035 = (Button)target;
			btn_pbd035.Click += Btn_SetPBDPattern_Click;
			break;
		case 388:
			img_pbd035 = (Image)target;
			img_pbd035.Drop += Img_pbd_Drop;
			break;
		case 389:
			gd_pdb036 = (Grid)target;
			break;
		case 390:
			txt_pbd036 = (TextBox)target;
			break;
		case 391:
			lab_pbd036 = (Label)target;
			break;
		case 392:
			btn_pbd036 = (Button)target;
			btn_pbd036.Click += Btn_SetPBDPattern_Click;
			break;
		case 393:
			img_pbd036 = (Image)target;
			img_pbd036.Drop += Img_pbd_Drop;
			break;
		case 394:
			gd_pdb037 = (Grid)target;
			break;
		case 395:
			txt_pbd037 = (TextBox)target;
			break;
		case 396:
			lab_pbd037 = (Label)target;
			break;
		case 397:
			btn_pbd037 = (Button)target;
			btn_pbd037.Click += Btn_SetPBDPattern_Click;
			break;
		case 398:
			img_pbd037 = (Image)target;
			img_pbd037.Drop += Img_pbd_Drop;
			break;
		case 399:
			gd_pdb038 = (Grid)target;
			break;
		case 400:
			txt_pbd038 = (TextBox)target;
			break;
		case 401:
			lab_pbd038 = (Label)target;
			break;
		case 402:
			btn_pbd038 = (Button)target;
			btn_pbd038.Click += Btn_SetPBDPattern_Click;
			break;
		case 403:
			img_pbd038 = (Image)target;
			img_pbd038.Drop += Img_pbd_Drop;
			break;
		case 404:
			gd_pdb039 = (Grid)target;
			break;
		case 405:
			txt_pbd039 = (TextBox)target;
			break;
		case 406:
			lab_pbd039 = (Label)target;
			break;
		case 407:
			btn_pbd039 = (Button)target;
			btn_pbd039.Click += Btn_SetPBDPattern_Click;
			break;
		case 408:
			img_pbd039 = (Image)target;
			img_pbd039.Drop += Img_pbd_Drop;
			break;
		case 409:
			gd_pdb040 = (Grid)target;
			break;
		case 410:
			txt_pbd040 = (TextBox)target;
			break;
		case 411:
			lab_pbd040 = (Label)target;
			break;
		case 412:
			btn_pbd040 = (Button)target;
			btn_pbd040.Click += Btn_SetPBDPattern_Click;
			break;
		case 413:
			img_pbd040 = (Image)target;
			img_pbd040.Drop += Img_pbd_Drop;
			break;
		case 414:
			grd_PPD5 = (Grid)target;
			break;
		case 415:
			gd_pdb041 = (Grid)target;
			break;
		case 416:
			txt_pbd041 = (TextBox)target;
			break;
		case 417:
			img_pbd041 = (Image)target;
			img_pbd041.Drop += Img_pbd_Drop;
			break;
		case 418:
			lab_pbd041 = (Label)target;
			break;
		case 419:
			btn_pbd041 = (Button)target;
			btn_pbd041.Click += Btn_SetPBDPattern_Click;
			break;
		case 420:
			gd_pdb042 = (Grid)target;
			break;
		case 421:
			txt_pbd042 = (TextBox)target;
			break;
		case 422:
			lab_pbd042 = (Label)target;
			break;
		case 423:
			btn_pbd042 = (Button)target;
			btn_pbd042.Click += Btn_SetPBDPattern_Click;
			break;
		case 424:
			img_pbd042 = (Image)target;
			img_pbd042.Drop += Img_pbd_Drop;
			break;
		case 425:
			gd_pdb043 = (Grid)target;
			break;
		case 426:
			txt_pbd043 = (TextBox)target;
			break;
		case 427:
			lab_pbd043 = (Label)target;
			break;
		case 428:
			btn_pbd043 = (Button)target;
			btn_pbd043.Click += Btn_SetPBDPattern_Click;
			break;
		case 429:
			img_pbd043 = (Image)target;
			img_pbd043.Drop += Img_pbd_Drop;
			break;
		case 430:
			gd_pdb044 = (Grid)target;
			break;
		case 431:
			txt_pbd044 = (TextBox)target;
			break;
		case 432:
			lab_pbd044 = (Label)target;
			break;
		case 433:
			btn_pbd044 = (Button)target;
			btn_pbd044.Click += Btn_SetPBDPattern_Click;
			break;
		case 434:
			img_pbd044 = (Image)target;
			img_pbd044.Drop += Img_pbd_Drop;
			break;
		case 435:
			gd_pdb045 = (Grid)target;
			break;
		case 436:
			txt_pbd045 = (TextBox)target;
			break;
		case 437:
			lab_pbd045 = (Label)target;
			break;
		case 438:
			btn_pbd045 = (Button)target;
			btn_pbd045.Click += Btn_SetPBDPattern_Click;
			break;
		case 439:
			img_pbd045 = (Image)target;
			img_pbd045.Drop += Img_pbd_Drop;
			break;
		case 440:
			gd_pdb046 = (Grid)target;
			break;
		case 441:
			txt_pbd046 = (TextBox)target;
			break;
		case 442:
			lab_pbd046 = (Label)target;
			break;
		case 443:
			btn_pbd046 = (Button)target;
			btn_pbd046.Click += Btn_SetPBDPattern_Click;
			break;
		case 444:
			img_pbd046 = (Image)target;
			img_pbd046.Drop += Img_pbd_Drop;
			break;
		case 445:
			gd_pdb047 = (Grid)target;
			break;
		case 446:
			txt_pbd047 = (TextBox)target;
			break;
		case 447:
			lab_pbd047 = (Label)target;
			break;
		case 448:
			btn_pbd047 = (Button)target;
			btn_pbd047.Click += Btn_SetPBDPattern_Click;
			break;
		case 449:
			img_pbd047 = (Image)target;
			img_pbd047.Drop += Img_pbd_Drop;
			break;
		case 450:
			gd_pdb048 = (Grid)target;
			break;
		case 451:
			txt_pbd048 = (TextBox)target;
			break;
		case 452:
			lab_pbd048 = (Label)target;
			break;
		case 453:
			btn_pbd048 = (Button)target;
			btn_pbd048.Click += Btn_SetPBDPattern_Click;
			break;
		case 454:
			img_pbd048 = (Image)target;
			img_pbd048.Drop += Img_pbd_Drop;
			break;
		case 455:
			gd_pdb049 = (Grid)target;
			break;
		case 456:
			txt_pbd049 = (TextBox)target;
			break;
		case 457:
			lab_pbd049 = (Label)target;
			break;
		case 458:
			btn_pbd049 = (Button)target;
			btn_pbd049.Click += Btn_SetPBDPattern_Click;
			break;
		case 459:
			img_pbd049 = (Image)target;
			img_pbd049.Drop += Img_pbd_Drop;
			break;
		case 460:
			gd_pdb050 = (Grid)target;
			break;
		case 461:
			txt_pbd050 = (TextBox)target;
			break;
		case 462:
			lab_pbd050 = (Label)target;
			break;
		case 463:
			btn_pbd050 = (Button)target;
			btn_pbd050.Click += Btn_SetPBDPattern_Click;
			break;
		case 464:
			img_pbd050 = (Image)target;
			img_pbd050.Drop += Img_pbd_Drop;
			break;
		case 465:
			grd_PPD6 = (Grid)target;
			break;
		case 466:
			gd_pdb051 = (Grid)target;
			break;
		case 467:
			txt_pbd051 = (TextBox)target;
			break;
		case 468:
			img_pbd051 = (Image)target;
			img_pbd051.Drop += Img_pbd_Drop;
			break;
		case 469:
			lab_pbd051 = (Label)target;
			break;
		case 470:
			btn_pbd051 = (Button)target;
			btn_pbd051.Click += Btn_SetPBDPattern_Click;
			break;
		case 471:
			gd_pdb052 = (Grid)target;
			break;
		case 472:
			txt_pbd052 = (TextBox)target;
			break;
		case 473:
			lab_pbd052 = (Label)target;
			break;
		case 474:
			btn_pbd052 = (Button)target;
			btn_pbd052.Click += Btn_SetPBDPattern_Click;
			break;
		case 475:
			img_pbd052 = (Image)target;
			img_pbd052.Drop += Img_pbd_Drop;
			break;
		case 476:
			gd_pdb053 = (Grid)target;
			break;
		case 477:
			txt_pbd053 = (TextBox)target;
			break;
		case 478:
			lab_pbd053 = (Label)target;
			break;
		case 479:
			btn_pbd053 = (Button)target;
			btn_pbd053.Click += Btn_SetPBDPattern_Click;
			break;
		case 480:
			img_pbd053 = (Image)target;
			img_pbd053.Drop += Img_pbd_Drop;
			break;
		case 481:
			gd_pdb054 = (Grid)target;
			break;
		case 482:
			txt_pbd054 = (TextBox)target;
			break;
		case 483:
			lab_pbd054 = (Label)target;
			break;
		case 484:
			btn_pbd054 = (Button)target;
			btn_pbd054.Click += Btn_SetPBDPattern_Click;
			break;
		case 485:
			img_pbd054 = (Image)target;
			img_pbd054.Drop += Img_pbd_Drop;
			break;
		case 486:
			gd_pdb055 = (Grid)target;
			break;
		case 487:
			txt_pbd055 = (TextBox)target;
			break;
		case 488:
			lab_pbd055 = (Label)target;
			break;
		case 489:
			btn_pbd055 = (Button)target;
			btn_pbd055.Click += Btn_SetPBDPattern_Click;
			break;
		case 490:
			img_pbd055 = (Image)target;
			img_pbd055.Drop += Img_pbd_Drop;
			break;
		case 491:
			gd_pdb056 = (Grid)target;
			break;
		case 492:
			txt_pbd056 = (TextBox)target;
			break;
		case 493:
			lab_pbd056 = (Label)target;
			break;
		case 494:
			btn_pbd056 = (Button)target;
			btn_pbd056.Click += Btn_SetPBDPattern_Click;
			break;
		case 495:
			img_pbd056 = (Image)target;
			img_pbd056.Drop += Img_pbd_Drop;
			break;
		case 496:
			gd_pdb057 = (Grid)target;
			break;
		case 497:
			txt_pbd057 = (TextBox)target;
			break;
		case 498:
			lab_pbd057 = (Label)target;
			break;
		case 499:
			btn_pbd057 = (Button)target;
			btn_pbd057.Click += Btn_SetPBDPattern_Click;
			break;
		case 500:
			img_pbd057 = (Image)target;
			img_pbd057.Drop += Img_pbd_Drop;
			break;
		case 501:
			gd_pdb058 = (Grid)target;
			break;
		case 502:
			txt_pbd058 = (TextBox)target;
			break;
		case 503:
			lab_pbd058 = (Label)target;
			break;
		case 504:
			btn_pbd058 = (Button)target;
			btn_pbd058.Click += Btn_SetPBDPattern_Click;
			break;
		case 505:
			img_pbd058 = (Image)target;
			img_pbd058.Drop += Img_pbd_Drop;
			break;
		case 506:
			gd_pdb059 = (Grid)target;
			break;
		case 507:
			txt_pbd059 = (TextBox)target;
			break;
		case 508:
			lab_pbd059 = (Label)target;
			break;
		case 509:
			btn_pbd059 = (Button)target;
			btn_pbd059.Click += Btn_SetPBDPattern_Click;
			break;
		case 510:
			img_pbd059 = (Image)target;
			img_pbd059.Drop += Img_pbd_Drop;
			break;
		case 511:
			gd_pdb060 = (Grid)target;
			break;
		case 512:
			txt_pbd060 = (TextBox)target;
			break;
		case 513:
			lab_pbd060 = (Label)target;
			break;
		case 514:
			btn_pbd060 = (Button)target;
			btn_pbd060.Click += Btn_SetPBDPattern_Click;
			break;
		case 515:
			img_pbd060 = (Image)target;
			img_pbd060.Drop += Img_pbd_Drop;
			break;
		case 516:
			grd_PPD7 = (Grid)target;
			break;
		case 517:
			gd_pdb061 = (Grid)target;
			break;
		case 518:
			txt_pbd061 = (TextBox)target;
			break;
		case 519:
			img_pbd061 = (Image)target;
			img_pbd061.Drop += Img_pbd_Drop;
			break;
		case 520:
			lab_pbd061 = (Label)target;
			break;
		case 521:
			btn_pbd061 = (Button)target;
			btn_pbd061.Click += Btn_SetPBDPattern_Click;
			break;
		case 522:
			gd_pdb062 = (Grid)target;
			break;
		case 523:
			txt_pbd062 = (TextBox)target;
			break;
		case 524:
			lab_pbd062 = (Label)target;
			break;
		case 525:
			btn_pbd062 = (Button)target;
			btn_pbd062.Click += Btn_SetPBDPattern_Click;
			break;
		case 526:
			img_pbd062 = (Image)target;
			img_pbd062.Drop += Img_pbd_Drop;
			break;
		case 527:
			gd_pdb063 = (Grid)target;
			break;
		case 528:
			txt_pbd063 = (TextBox)target;
			break;
		case 529:
			lab_pbd063 = (Label)target;
			break;
		case 530:
			btn_pbd063 = (Button)target;
			btn_pbd063.Click += Btn_SetPBDPattern_Click;
			break;
		case 531:
			img_pbd063 = (Image)target;
			img_pbd063.Drop += Img_pbd_Drop;
			break;
		case 532:
			gd_pdb064 = (Grid)target;
			break;
		case 533:
			txt_pbd064 = (TextBox)target;
			break;
		case 534:
			lab_pbd064 = (Label)target;
			break;
		case 535:
			btn_pbd064 = (Button)target;
			btn_pbd064.Click += Btn_SetPBDPattern_Click;
			break;
		case 536:
			img_pbd064 = (Image)target;
			img_pbd064.Drop += Img_pbd_Drop;
			break;
		case 537:
			gd_pdb065 = (Grid)target;
			break;
		case 538:
			txt_pbd065 = (TextBox)target;
			break;
		case 539:
			lab_pbd065 = (Label)target;
			break;
		case 540:
			btn_pbd065 = (Button)target;
			btn_pbd065.Click += Btn_SetPBDPattern_Click;
			break;
		case 541:
			img_pbd065 = (Image)target;
			img_pbd065.Drop += Img_pbd_Drop;
			break;
		case 542:
			gd_pdb066 = (Grid)target;
			break;
		case 543:
			txt_pbd066 = (TextBox)target;
			break;
		case 544:
			lab_pbd066 = (Label)target;
			break;
		case 545:
			btn_pbd066 = (Button)target;
			btn_pbd066.Click += Btn_SetPBDPattern_Click;
			break;
		case 546:
			img_pbd066 = (Image)target;
			img_pbd066.Drop += Img_pbd_Drop;
			break;
		case 547:
			gd_pdb067 = (Grid)target;
			break;
		case 548:
			txt_pbd067 = (TextBox)target;
			break;
		case 549:
			lab_pbd067 = (Label)target;
			break;
		case 550:
			btn_pbd067 = (Button)target;
			btn_pbd067.Click += Btn_SetPBDPattern_Click;
			break;
		case 551:
			img_pbd067 = (Image)target;
			img_pbd067.Drop += Img_pbd_Drop;
			break;
		case 552:
			gd_pdb068 = (Grid)target;
			break;
		case 553:
			txt_pbd068 = (TextBox)target;
			break;
		case 554:
			lab_pbd068 = (Label)target;
			break;
		case 555:
			btn_pbd068 = (Button)target;
			btn_pbd068.Click += Btn_SetPBDPattern_Click;
			break;
		case 556:
			img_pbd068 = (Image)target;
			img_pbd068.Drop += Img_pbd_Drop;
			break;
		case 557:
			gd_pdb069 = (Grid)target;
			break;
		case 558:
			txt_pbd069 = (TextBox)target;
			break;
		case 559:
			lab_pbd069 = (Label)target;
			break;
		case 560:
			btn_pbd069 = (Button)target;
			btn_pbd069.Click += Btn_SetPBDPattern_Click;
			break;
		case 561:
			img_pbd069 = (Image)target;
			img_pbd069.Drop += Img_pbd_Drop;
			break;
		case 562:
			gd_pdb070 = (Grid)target;
			break;
		case 563:
			txt_pbd070 = (TextBox)target;
			break;
		case 564:
			lab_pbd070 = (Label)target;
			break;
		case 565:
			btn_pbd070 = (Button)target;
			btn_pbd070.Click += Btn_SetPBDPattern_Click;
			break;
		case 566:
			img_pbd070 = (Image)target;
			img_pbd070.Drop += Img_pbd_Drop;
			break;
		case 567:
			tab_FontDB = (TabItem)target;
			break;
		case 568:
			grd_R = (Grid)target;
			break;
		case 569:
			img_Font_R1 = (Image)target;
			img_Font_R1.PreviewMouseMove += Img_Font_R1_PreviewMouseMove;
			break;
		case 570:
			btn_Font_R1 = (Button)target;
			btn_Font_R1.Click += Btn_Font_R1_Click;
			btn_Font_R1.MouseRightButtonDown += Btn_Font_R1_MouseRightButtonDown;
			break;
		case 571:
			img_Font_R2 = (Image)target;
			img_Font_R2.PreviewMouseMove += Img_Font_R1_PreviewMouseMove;
			break;
		case 572:
			btn_Font_R2 = (Button)target;
			btn_Font_R2.Click += Btn_Font_R1_Click;
			btn_Font_R2.MouseRightButtonDown += Btn_Font_R1_MouseRightButtonDown;
			break;
		case 573:
			img_Font_R3 = (Image)target;
			img_Font_R3.PreviewMouseMove += Img_Font_R1_PreviewMouseMove;
			break;
		case 574:
			btn_Font_R3 = (Button)target;
			btn_Font_R3.Click += Btn_Font_R1_Click;
			btn_Font_R3.MouseRightButtonDown += Btn_Font_R1_MouseRightButtonDown;
			break;
		case 575:
			img_Font_R4 = (Image)target;
			img_Font_R4.PreviewMouseMove += Img_Font_R1_PreviewMouseMove;
			break;
		case 576:
			btn_Font_R4 = (Button)target;
			btn_Font_R4.Click += Btn_Font_R1_Click;
			btn_Font_R4.MouseRightButtonDown += Btn_Font_R1_MouseRightButtonDown;
			break;
		case 577:
			img_Font_R5 = (Image)target;
			img_Font_R5.PreviewMouseMove += Img_Font_R1_PreviewMouseMove;
			break;
		case 578:
			btn_Font_R5 = (Button)target;
			btn_Font_R5.Click += Btn_Font_R1_Click;
			btn_Font_R5.MouseRightButtonDown += Btn_Font_R1_MouseRightButtonDown;
			break;
		case 579:
			btn_R_Setup = (Button)target;
			btn_R_Setup.Click += Btn_R_Setup_Click;
			break;
		case 580:
			grd_M = (Grid)target;
			break;
		case 581:
			img_Font_M1 = (Image)target;
			break;
		case 582:
			btn_Font_M1 = (Button)target;
			btn_Font_M1.Click += Btn_Font_M1_Click;
			btn_Font_M1.MouseRightButtonDown += Btn_Font_M1_MouseRightButtonDown;
			break;
		case 583:
			img_Font_M2 = (Image)target;
			break;
		case 584:
			btn_Font_M2 = (Button)target;
			btn_Font_M2.Click += Btn_Font_M1_Click;
			btn_Font_M2.MouseRightButtonDown += Btn_Font_M1_MouseRightButtonDown;
			break;
		case 585:
			img_Font_M3 = (Image)target;
			break;
		case 586:
			btn_Font_M3 = (Button)target;
			btn_Font_M3.Click += Btn_Font_M1_Click;
			btn_Font_M3.MouseRightButtonDown += Btn_Font_M1_MouseRightButtonDown;
			break;
		case 587:
			img_Font_M4 = (Image)target;
			break;
		case 588:
			btn_Font_M4 = (Button)target;
			btn_Font_M4.Click += Btn_Font_M1_Click;
			btn_Font_M4.MouseRightButtonDown += Btn_Font_M1_MouseRightButtonDown;
			break;
		case 589:
			img_Font_M5 = (Image)target;
			break;
		case 590:
			btn_Font_M5 = (Button)target;
			btn_Font_M5.Click += Btn_Font_M1_Click;
			btn_Font_M5.MouseRightButtonDown += Btn_Font_M1_MouseRightButtonDown;
			break;
		case 591:
			btn_M_Setup = (Button)target;
			btn_M_Setup.Click += Btn_R_Setup_Click;
			break;
		case 592:
			grd_C = (Grid)target;
			break;
		case 593:
			img_Font_C1 = (Image)target;
			break;
		case 594:
			btn_Font_C1 = (Button)target;
			btn_Font_C1.Click += Btn_Font_C1_Click;
			btn_Font_C1.MouseRightButtonDown += Btn_Font_C1_MouseRightButtonDown;
			break;
		case 595:
			img_Font_C2 = (Image)target;
			break;
		case 596:
			btn_Font_C2 = (Button)target;
			btn_Font_C2.Click += Btn_Font_C1_Click;
			btn_Font_C2.MouseRightButtonDown += Btn_Font_C1_MouseRightButtonDown;
			break;
		case 597:
			img_Font_C3 = (Image)target;
			break;
		case 598:
			btn_Font_C3 = (Button)target;
			btn_Font_C3.Click += Btn_Font_C1_Click;
			btn_Font_C3.MouseRightButtonDown += Btn_Font_C1_MouseRightButtonDown;
			break;
		case 599:
			img_Font_C4 = (Image)target;
			break;
		case 600:
			btn_Font_C4 = (Button)target;
			btn_Font_C4.Click += Btn_Font_C1_Click;
			btn_Font_C4.MouseRightButtonDown += Btn_Font_C1_MouseRightButtonDown;
			break;
		case 601:
			img_Font_C5 = (Image)target;
			break;
		case 602:
			btn_Font_C5 = (Button)target;
			btn_Font_C5.Click += Btn_Font_C1_Click;
			btn_Font_C5.MouseRightButtonDown += Btn_Font_C1_MouseRightButtonDown;
			break;
		case 603:
			btn_C_Setup = (Button)target;
			btn_C_Setup.Click += Btn_R_Setup_Click;
			break;
		case 604:
			grd_Dash = (Grid)target;
			break;
		case 605:
			img_Font_Dash1 = (Image)target;
			break;
		case 606:
			btn_Font_Dash1 = (Button)target;
			btn_Font_Dash1.Click += Btn_Font_Dash1_Click;
			btn_Font_Dash1.MouseRightButtonDown += Btn_Font_Dash1_MouseRightButtonDown;
			break;
		case 607:
			img_Font_Dash2 = (Image)target;
			break;
		case 608:
			btn_Font_Dash2 = (Button)target;
			btn_Font_Dash2.Click += Btn_Font_Dash1_Click;
			btn_Font_Dash2.MouseRightButtonDown += Btn_Font_Dash1_MouseRightButtonDown;
			break;
		case 609:
			img_Font_Dash3 = (Image)target;
			break;
		case 610:
			btn_Font_Dash3 = (Button)target;
			btn_Font_Dash3.Click += Btn_Font_Dash1_Click;
			btn_Font_Dash3.MouseRightButtonDown += Btn_Font_Dash1_MouseRightButtonDown;
			break;
		case 611:
			img_Font_Dash4 = (Image)target;
			break;
		case 612:
			btn_Font_Dash4 = (Button)target;
			btn_Font_Dash4.Click += Btn_Font_Dash1_Click;
			btn_Font_Dash4.MouseRightButtonDown += Btn_Font_Dash1_MouseRightButtonDown;
			break;
		case 613:
			img_Font_Dash5 = (Image)target;
			break;
		case 614:
			btn_Font_Dash5 = (Button)target;
			btn_Font_Dash5.Click += Btn_Font_Dash1_Click;
			btn_Font_Dash5.MouseRightButtonDown += Btn_Font_Dash1_MouseRightButtonDown;
			break;
		case 615:
			btn_Dash_Setup = (Button)target;
			btn_Dash_Setup.Click += Btn_R_Setup_Click;
			break;
		case 616:
			grd_0 = (Grid)target;
			break;
		case 617:
			img_Font_01 = (Image)target;
			break;
		case 618:
			btn_Font_01 = (Button)target;
			btn_Font_01.Click += Btn_Font_01_Click;
			btn_Font_01.MouseRightButtonDown += Btn_Font_01_MouseRightButtonDown;
			break;
		case 619:
			img_Font_02 = (Image)target;
			break;
		case 620:
			btn_Font_02 = (Button)target;
			btn_Font_02.Click += Btn_Font_01_Click;
			btn_Font_02.MouseRightButtonDown += Btn_Font_01_MouseRightButtonDown;
			break;
		case 621:
			img_Font_03 = (Image)target;
			break;
		case 622:
			btn_Font_03 = (Button)target;
			btn_Font_03.Click += Btn_Font_01_Click;
			btn_Font_03.MouseRightButtonDown += Btn_Font_01_MouseRightButtonDown;
			break;
		case 623:
			img_Font_04 = (Image)target;
			break;
		case 624:
			btn_Font_04 = (Button)target;
			btn_Font_04.Click += Btn_Font_01_Click;
			btn_Font_04.MouseRightButtonDown += Btn_Font_01_MouseRightButtonDown;
			break;
		case 625:
			img_Font_05 = (Image)target;
			break;
		case 626:
			btn_Font_05 = (Button)target;
			btn_Font_05.Click += Btn_Font_01_Click;
			btn_Font_05.MouseRightButtonDown += Btn_Font_01_MouseRightButtonDown;
			break;
		case 627:
			btn_0_Setup = (Button)target;
			btn_0_Setup.Click += Btn_R_Setup_Click;
			break;
		case 628:
			grd_1 = (Grid)target;
			break;
		case 629:
			img_Font_11 = (Image)target;
			break;
		case 630:
			btn_Font_11 = (Button)target;
			btn_Font_11.Click += Btn_Font_11_Click;
			btn_Font_11.MouseRightButtonDown += Btn_Font_11_MouseRightButtonDown;
			break;
		case 631:
			img_Font_12 = (Image)target;
			break;
		case 632:
			btn_Font_12 = (Button)target;
			btn_Font_12.Click += Btn_Font_11_Click;
			btn_Font_12.MouseRightButtonDown += Btn_Font_11_MouseRightButtonDown;
			break;
		case 633:
			img_Font_13 = (Image)target;
			break;
		case 634:
			btn_Font_13 = (Button)target;
			btn_Font_13.Click += Btn_Font_11_Click;
			btn_Font_13.MouseRightButtonDown += Btn_Font_11_MouseRightButtonDown;
			break;
		case 635:
			img_Font_14 = (Image)target;
			break;
		case 636:
			btn_Font_14 = (Button)target;
			btn_Font_14.Click += Btn_Font_11_Click;
			btn_Font_14.MouseRightButtonDown += Btn_Font_11_MouseRightButtonDown;
			break;
		case 637:
			img_Font_15 = (Image)target;
			break;
		case 638:
			btn_Font_15 = (Button)target;
			btn_Font_15.Click += Btn_Font_11_Click;
			btn_Font_15.MouseRightButtonDown += Btn_Font_11_MouseRightButtonDown;
			break;
		case 639:
			btn_1_Setup = (Button)target;
			btn_1_Setup.Click += Btn_R_Setup_Click;
			break;
		case 640:
			grd_2 = (Grid)target;
			break;
		case 641:
			img_Font_21 = (Image)target;
			break;
		case 642:
			btn_Font_21 = (Button)target;
			btn_Font_21.Click += Btn_Font_21_Click;
			btn_Font_21.MouseRightButtonDown += Btn_Font_21_MouseRightButtonDown;
			break;
		case 643:
			img_Font_22 = (Image)target;
			break;
		case 644:
			btn_Font_22 = (Button)target;
			btn_Font_22.Click += Btn_Font_21_Click;
			btn_Font_22.MouseRightButtonDown += Btn_Font_21_MouseRightButtonDown;
			break;
		case 645:
			img_Font_23 = (Image)target;
			break;
		case 646:
			btn_Font_23 = (Button)target;
			btn_Font_23.Click += Btn_Font_21_Click;
			btn_Font_23.MouseRightButtonDown += Btn_Font_21_MouseRightButtonDown;
			break;
		case 647:
			img_Font_24 = (Image)target;
			break;
		case 648:
			btn_Font_24 = (Button)target;
			btn_Font_24.Click += Btn_Font_21_Click;
			btn_Font_24.MouseRightButtonDown += Btn_Font_21_MouseRightButtonDown;
			break;
		case 649:
			img_Font_25 = (Image)target;
			break;
		case 650:
			btn_Font_25 = (Button)target;
			btn_Font_25.Click += Btn_Font_21_Click;
			btn_Font_25.MouseRightButtonDown += Btn_Font_21_MouseRightButtonDown;
			break;
		case 651:
			btn_2_Setup = (Button)target;
			btn_2_Setup.Click += Btn_R_Setup_Click;
			break;
		case 652:
			grd_3 = (Grid)target;
			break;
		case 653:
			img_Font_31 = (Image)target;
			break;
		case 654:
			btn_Font_31 = (Button)target;
			btn_Font_31.Click += Btn_Font_31_Click;
			btn_Font_31.MouseRightButtonDown += Btn_Font_31_MouseRightButtonDown;
			break;
		case 655:
			img_Font_32 = (Image)target;
			break;
		case 656:
			btn_Font_32 = (Button)target;
			btn_Font_32.Click += Btn_Font_31_Click;
			btn_Font_32.MouseRightButtonDown += Btn_Font_31_MouseRightButtonDown;
			break;
		case 657:
			img_Font_33 = (Image)target;
			break;
		case 658:
			btn_Font_33 = (Button)target;
			btn_Font_33.Click += Btn_Font_31_Click;
			btn_Font_33.MouseRightButtonDown += Btn_Font_31_MouseRightButtonDown;
			break;
		case 659:
			img_Font_34 = (Image)target;
			break;
		case 660:
			btn_Font_34 = (Button)target;
			btn_Font_34.Click += Btn_Font_31_Click;
			btn_Font_34.MouseRightButtonDown += Btn_Font_31_MouseRightButtonDown;
			break;
		case 661:
			img_Font_35 = (Image)target;
			break;
		case 662:
			btn_Font_35 = (Button)target;
			btn_Font_35.Click += Btn_Font_31_Click;
			btn_Font_35.MouseRightButtonDown += Btn_Font_31_MouseRightButtonDown;
			break;
		case 663:
			btn_3_Setup = (Button)target;
			btn_3_Setup.Click += Btn_R_Setup_Click;
			break;
		case 664:
			grd_4 = (Grid)target;
			break;
		case 665:
			img_Font_41 = (Image)target;
			break;
		case 666:
			btn_Font_41 = (Button)target;
			btn_Font_41.Click += Btn_Font_41_Click;
			btn_Font_41.MouseRightButtonDown += Btn_Font_41_MouseRightButtonDown;
			break;
		case 667:
			img_Font_42 = (Image)target;
			break;
		case 668:
			btn_Font_42 = (Button)target;
			btn_Font_42.Click += Btn_Font_41_Click;
			btn_Font_42.MouseRightButtonDown += Btn_Font_41_MouseRightButtonDown;
			break;
		case 669:
			img_Font_43 = (Image)target;
			break;
		case 670:
			btn_Font_43 = (Button)target;
			btn_Font_43.Click += Btn_Font_41_Click;
			btn_Font_43.MouseRightButtonDown += Btn_Font_41_MouseRightButtonDown;
			break;
		case 671:
			img_Font_44 = (Image)target;
			break;
		case 672:
			btn_Font_44 = (Button)target;
			btn_Font_44.Click += Btn_Font_41_Click;
			btn_Font_44.MouseRightButtonDown += Btn_Font_41_MouseRightButtonDown;
			break;
		case 673:
			img_Font_45 = (Image)target;
			break;
		case 674:
			btn_Font_45 = (Button)target;
			btn_Font_45.Click += Btn_Font_41_Click;
			btn_Font_45.MouseRightButtonDown += Btn_Font_41_MouseRightButtonDown;
			break;
		case 675:
			btn_4_Setup = (Button)target;
			btn_4_Setup.Click += Btn_R_Setup_Click;
			break;
		case 676:
			grd_5 = (Grid)target;
			break;
		case 677:
			img_Font_51 = (Image)target;
			break;
		case 678:
			btn_Font_51 = (Button)target;
			btn_Font_51.Click += Btn_Font_51_Click;
			btn_Font_51.MouseRightButtonDown += Btn_Font_51_MouseRightButtonDown;
			break;
		case 679:
			img_Font_52 = (Image)target;
			break;
		case 680:
			btn_Font_52 = (Button)target;
			btn_Font_52.Click += Btn_Font_51_Click;
			btn_Font_52.MouseRightButtonDown += Btn_Font_51_MouseRightButtonDown;
			break;
		case 681:
			img_Font_53 = (Image)target;
			break;
		case 682:
			btn_Font_53 = (Button)target;
			btn_Font_53.Click += Btn_Font_51_Click;
			btn_Font_53.MouseRightButtonDown += Btn_Font_51_MouseRightButtonDown;
			break;
		case 683:
			img_Font_54 = (Image)target;
			break;
		case 684:
			btn_Font_54 = (Button)target;
			btn_Font_54.Click += Btn_Font_51_Click;
			btn_Font_54.MouseRightButtonDown += Btn_Font_51_MouseRightButtonDown;
			break;
		case 685:
			img_Font_55 = (Image)target;
			break;
		case 686:
			btn_Font_55 = (Button)target;
			btn_Font_55.Click += Btn_Font_51_Click;
			btn_Font_55.MouseRightButtonDown += Btn_Font_51_MouseRightButtonDown;
			break;
		case 687:
			btn_5_Setup = (Button)target;
			btn_5_Setup.Click += Btn_R_Setup_Click;
			break;
		case 688:
			grd_6 = (Grid)target;
			break;
		case 689:
			img_Font_61 = (Image)target;
			break;
		case 690:
			btn_Font_61 = (Button)target;
			btn_Font_61.Click += Btn_Font_61_Click;
			btn_Font_61.MouseRightButtonDown += Btn_Font_61_MouseRightButtonDown;
			break;
		case 691:
			img_Font_62 = (Image)target;
			break;
		case 692:
			btn_Font_62 = (Button)target;
			btn_Font_62.Click += Btn_Font_61_Click;
			btn_Font_62.MouseRightButtonDown += Btn_Font_61_MouseRightButtonDown;
			break;
		case 693:
			img_Font_63 = (Image)target;
			break;
		case 694:
			btn_Font_63 = (Button)target;
			btn_Font_63.Click += Btn_Font_61_Click;
			btn_Font_63.MouseRightButtonDown += Btn_Font_61_MouseRightButtonDown;
			break;
		case 695:
			img_Font_64 = (Image)target;
			break;
		case 696:
			btn_Font_64 = (Button)target;
			btn_Font_64.Click += Btn_Font_61_Click;
			btn_Font_64.MouseRightButtonDown += Btn_Font_61_MouseRightButtonDown;
			break;
		case 697:
			img_Font_65 = (Image)target;
			break;
		case 698:
			btn_Font_65 = (Button)target;
			btn_Font_65.Click += Btn_Font_61_Click;
			btn_Font_65.MouseRightButtonDown += Btn_Font_61_MouseRightButtonDown;
			break;
		case 699:
			btn_6_Setup = (Button)target;
			btn_6_Setup.Click += Btn_R_Setup_Click;
			break;
		case 700:
			grd_7 = (Grid)target;
			break;
		case 701:
			img_Font_71 = (Image)target;
			break;
		case 702:
			btn_Font_71 = (Button)target;
			btn_Font_71.Click += Btn_Font_71_Click;
			btn_Font_71.MouseRightButtonDown += Btn_Font_71_MouseRightButtonDown;
			break;
		case 703:
			img_Font_72 = (Image)target;
			break;
		case 704:
			btn_Font_72 = (Button)target;
			btn_Font_72.Click += Btn_Font_71_Click;
			btn_Font_72.MouseRightButtonDown += Btn_Font_71_MouseRightButtonDown;
			break;
		case 705:
			img_Font_73 = (Image)target;
			break;
		case 706:
			btn_Font_73 = (Button)target;
			btn_Font_73.Click += Btn_Font_71_Click;
			btn_Font_73.MouseRightButtonDown += Btn_Font_71_MouseRightButtonDown;
			break;
		case 707:
			img_Font_74 = (Image)target;
			break;
		case 708:
			btn_Font_74 = (Button)target;
			btn_Font_74.Click += Btn_Font_71_Click;
			btn_Font_74.MouseRightButtonDown += Btn_Font_71_MouseRightButtonDown;
			break;
		case 709:
			img_Font_75 = (Image)target;
			break;
		case 710:
			btn_Font_75 = (Button)target;
			btn_Font_75.Click += Btn_Font_71_Click;
			btn_Font_75.MouseRightButtonDown += Btn_Font_71_MouseRightButtonDown;
			break;
		case 711:
			btn_7_Setup = (Button)target;
			btn_7_Setup.Click += Btn_R_Setup_Click;
			break;
		case 712:
			grd_8 = (Grid)target;
			break;
		case 713:
			img_Font_81 = (Image)target;
			break;
		case 714:
			btn_Font_81 = (Button)target;
			btn_Font_81.Click += Btn_Font_81_Click;
			btn_Font_81.MouseRightButtonDown += Btn_Font_81_MouseRightButtonDown;
			break;
		case 715:
			img_Font_82 = (Image)target;
			break;
		case 716:
			btn_Font_82 = (Button)target;
			btn_Font_82.Click += Btn_Font_81_Click;
			btn_Font_82.MouseRightButtonDown += Btn_Font_81_MouseRightButtonDown;
			break;
		case 717:
			img_Font_83 = (Image)target;
			break;
		case 718:
			btn_Font_83 = (Button)target;
			btn_Font_83.Click += Btn_Font_81_Click;
			btn_Font_83.MouseRightButtonDown += Btn_Font_81_MouseRightButtonDown;
			break;
		case 719:
			img_Font_84 = (Image)target;
			break;
		case 720:
			btn_Font_84 = (Button)target;
			btn_Font_84.Click += Btn_Font_81_Click;
			btn_Font_84.MouseRightButtonDown += Btn_Font_81_MouseRightButtonDown;
			break;
		case 721:
			img_Font_85 = (Image)target;
			break;
		case 722:
			btn_Font_85 = (Button)target;
			btn_Font_85.Click += Btn_Font_81_Click;
			btn_Font_85.MouseRightButtonDown += Btn_Font_81_MouseRightButtonDown;
			break;
		case 723:
			btn_8_Setup = (Button)target;
			btn_8_Setup.Click += Btn_R_Setup_Click;
			break;
		case 724:
			grd_9 = (Grid)target;
			break;
		case 725:
			img_Font_91 = (Image)target;
			break;
		case 726:
			btn_Font_91 = (Button)target;
			btn_Font_91.Click += Btn_Font_91_Click;
			btn_Font_91.MouseRightButtonDown += Btn_Font_91_MouseRightButtonDown;
			break;
		case 727:
			img_Font_92 = (Image)target;
			break;
		case 728:
			btn_Font_92 = (Button)target;
			btn_Font_92.Click += Btn_Font_91_Click;
			btn_Font_92.MouseRightButtonDown += Btn_Font_91_MouseRightButtonDown;
			break;
		case 729:
			img_Font_93 = (Image)target;
			break;
		case 730:
			btn_Font_93 = (Button)target;
			btn_Font_93.Click += Btn_Font_91_Click;
			btn_Font_93.MouseRightButtonDown += Btn_Font_91_MouseRightButtonDown;
			break;
		case 731:
			img_Font_94 = (Image)target;
			break;
		case 732:
			btn_Font_94 = (Button)target;
			btn_Font_94.Click += Btn_Font_91_Click;
			btn_Font_94.MouseRightButtonDown += Btn_Font_91_MouseRightButtonDown;
			break;
		case 733:
			img_Font_95 = (Image)target;
			break;
		case 734:
			btn_Font_95 = (Button)target;
			btn_Font_95.Click += Btn_Font_91_Click;
			btn_Font_95.MouseRightButtonDown += Btn_Font_91_MouseRightButtonDown;
			break;
		case 735:
			btn_9_Setup = (Button)target;
			btn_9_Setup.Click += Btn_R_Setup_Click;
			break;
		case 736:
			grd_U = (Grid)target;
			break;
		case 737:
			img_Font_U1 = (Image)target;
			break;
		case 738:
			btn_Font_U1 = (Button)target;
			btn_Font_U1.Click += Btn_Font_U1_Click;
			btn_Font_U1.MouseRightButtonDown += Btn_Font_U1_MouseRightButtonDown;
			break;
		case 739:
			img_Font_U2 = (Image)target;
			break;
		case 740:
			btn_Font_U2 = (Button)target;
			btn_Font_U2.Click += Btn_Font_U1_Click;
			btn_Font_U2.MouseRightButtonDown += Btn_Font_U1_MouseRightButtonDown;
			break;
		case 741:
			img_Font_U3 = (Image)target;
			break;
		case 742:
			btn_Font_U3 = (Button)target;
			btn_Font_U3.Click += Btn_Font_U1_Click;
			btn_Font_U3.MouseRightButtonDown += Btn_Font_U1_MouseRightButtonDown;
			break;
		case 743:
			img_Font_U4 = (Image)target;
			break;
		case 744:
			btn_Font_U4 = (Button)target;
			btn_Font_U4.Click += Btn_Font_U1_Click;
			btn_Font_U4.MouseRightButtonDown += Btn_Font_U1_MouseRightButtonDown;
			break;
		case 745:
			img_Font_U5 = (Image)target;
			break;
		case 746:
			btn_Font_U5 = (Button)target;
			btn_Font_U5.Click += Btn_Font_U1_Click;
			btn_Font_U5.MouseRightButtonDown += Btn_Font_U1_MouseRightButtonDown;
			break;
		case 747:
			btn_U_Setup = (Button)target;
			btn_U_Setup.Click += Btn_R_Setup_Click;
			break;
		case 748:
			rad_FDByRecipe = (RadioButton)target;
			rad_FDByRecipe.Checked += Rad_FDByRecipe_Checked;
			break;
		case 749:
			rad_FDByPublic = (RadioButton)target;
			rad_FDByPublic.Checked += Rad_FDByPublic_Checked;
			break;
		case 750:
			btn_FontPagePrev = (Button)target;
			btn_FontPagePrev.Click += Btn_FontPagePrev_Click;
			break;
		case 751:
			lab_FontDBPage = (Label)target;
			break;
		case 752:
			btn_FontPageNext = (Button)target;
			btn_FontPageNext.Click += Btn_FontPageNext_Click;
			break;
		case 753:
			btn_AutoPutFont = (Button)target;
			btn_AutoPutFont.Click += btn_AutoPutFont_Click;
			break;
		case 754:
			grd_Color = (Grid)target;
			break;
		case 755:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 756:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 757:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 758:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 759:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 760:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 761:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 762:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 763:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 764:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 765:
			btn_ImageNum = (Button)target;
			break;
		case 766:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 767:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 768:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 769:
			btn_ImgAI = (Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 770:
			lab_RValue = (Label)target;
			break;
		case 771:
			lab_GValue = (Label)target;
			break;
		case 772:
			lab_BValue = (Label)target;
			break;
		case 773:
			lab_HValue = (Label)target;
			break;
		case 774:
			lab_SValue = (Label)target;
			break;
		case 775:
			lab_VValue = (Label)target;
			break;
		case 776:
			lab_GrayValue = (Label)target;
			break;
		case 777:
			lab_XValue = (Label)target;
			break;
		case 778:
			lab_YValue = (Label)target;
			break;
		case 779:
			lab_C1Value = (Label)target;
			break;
		case 780:
			lab_C2Value = (Label)target;
			break;
		case 781:
			lab_C3Value = (Label)target;
			break;
		case 782:
			lab_MatchScore = (Label)target;
			break;
		case 783:
			txt_AllResult = (TextBox)target;
			break;
		case 784:
			btn_TrackerShow = (Button)target;
			btn_TrackerShow.Click += Btn_TrackerShow_Click;
			break;
		case 785:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 786:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 787:
			btn_ReferenceShow = (Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 788:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 789:
			Label_Index = (Label)target;
			break;
		case 790:
			lab_LockMode = (TextBox)target;
			break;
		case 791:
			grd_QuickExportImage = (Grid)target;
			break;
		case 792:
			btn_QuickAdd_1 = (Button)target;
			btn_QuickAdd_1.Click += Btn_QuickAdd_1_Click;
			break;
		case 793:
			btn_QuickAdd_2 = (Button)target;
			btn_QuickAdd_2.Click += Btn_QuickAdd_1_Click;
			break;
		case 794:
			btn_QuickAdd_3 = (Button)target;
			btn_QuickAdd_3.Click += Btn_QuickAdd_1_Click;
			break;
		case 795:
			btn_QuickAdd_4 = (Button)target;
			btn_QuickAdd_4.Click += Btn_QuickAdd_1_Click;
			break;
		case 796:
			btn_QuickAdd_5 = (Button)target;
			btn_QuickAdd_5.Click += Btn_QuickAdd_1_Click;
			break;
		case 797:
			btn_OpenFontFolder = (Button)target;
			btn_OpenFontFolder.Click += Btn_OpenFontFolder_Click;
			break;
		case 798:
			lab_Message = (Label)target;
			break;
		case 799:
			btn_Result_Flip = (Button)target;
			break;
		case 800:
			btn_ShowCheckFontTool = (Button)target;
			btn_ShowCheckFontTool.Click += Btn_ShowCheckFontTool_Click;
			break;
		case 801:
			grd_FontTest = (Grid)target;
			break;
		case 802:
			grd_AI_Test = (Grid)target;
			grd_AI_Test.Drop += grd_AI_Test_Drop;
			break;
		case 803:
			img_TestFont = (Image)target;
			break;
		case 804:
			btn_Open = (Button)target;
			btn_Open.Click += Btn_Open_Click;
			break;
		case 805:
			btn_prev = (Button)target;
			btn_prev.Click += Btn_prev_Click;
			break;
		case 806:
			btn_next = (Button)target;
			btn_next.Click += Btn_next_Click;
			break;
		case 807:
			btn_fonttest = (Button)target;
			btn_fonttest.Click += Btn_fonttest_Click;
			break;
		case 808:
			txt_FontCount = (TextBox)target;
			break;
		case 809:
			lab_FontTestResult = (Label)target;
			break;
		case 810:
			txt_TestFontResult = (TextBox)target;
			break;
		case 811:
			cb_FontSelectTest = (ComboBox)target;
			cb_FontSelectTest.SelectionChanged += cb_FontSelectTest_SelectionChanged;
			break;
		case 812:
			img_TestFontPattern = (Image)target;
			break;
		case 813:
			chk_OKStop = (CheckBox)target;
			break;
		case 814:
			chk_NGStop = (CheckBox)target;
			break;
		case 815:
			lab_FontTestResultScore = (Label)target;
			break;
		case 816:
			lab_FontOCRResult = (Label)target;
			break;
		case 817:
			lab_FontAIResult = (Label)target;
			break;
		case 818:
			btn_AutoTest = (Button)target;
			btn_AutoTest.Click += Btn_AutoTest_Click;
			break;
		case 819:
			lab_OKCount = (Label)target;
			break;
		case 820:
			lab_NGCount = (Label)target;
			break;
		case 821:
			lab_Yield = (Label)target;
			break;
		case 822:
			btn_SaveTestImage = (Button)target;
			btn_SaveTestImage.Click += Btn_SaveTestImage_Click;
			break;
		case 823:
			btn_OpenOKNG = (Button)target;
			btn_OpenOKNG.Click += Btn_OpenOKNG_Click;
			break;
		case 824:
			btn_GenCSVFile = (Button)target;
			btn_GenCSVFile.Click += Btn_GenCSVFile_Click;
			break;
		case 825:
			btn_Font_6 = (Button)target;
			btn_Font_6.Click += Btn_R_Setup_Click;
			break;
		case 826:
			btn_CloseCheckFontTool = (Button)target;
			btn_CloseCheckFontTool.Click += Btn_CloseCheckFontTool_Click;
			break;
		case 827:
			chk_TestFont_AOI = (CheckBox)target;
			break;
		case 828:
			chk_TestFont_OCR = (CheckBox)target;
			break;
		case 829:
			chk_TestFont_CheckFont = (CheckBox)target;
			break;
		case 830:
			chk_TestFont_AI = (CheckBox)target;
			break;
		case 831:
			grd_AI_Function = (Grid)target;
			grd_AI_Function.Drop += grd_AI_Function_Drop;
			break;
		case 832:
			btn_SetTrainFolder = (Button)target;
			btn_SetTrainFolder.Click += btn_SetTrainFolder_Click;
			break;
		case 833:
			btn_Add2TrainA = (Button)target;
			btn_Add2TrainA.Click += btn_Add2TrainA_Click;
			break;
		case 834:
			btn_Add2TrainB = (Button)target;
			btn_Add2TrainB.Click += btn_Add2TrainA_Click;
			break;
		case 835:
			btn_Add2TrainD = (Button)target;
			btn_Add2TrainD.Click += btn_Add2TrainA_Click;
			break;
		case 836:
			btn_Add2TrainE = (Button)target;
			btn_Add2TrainE.Click += btn_Add2TrainA_Click;
			break;
		case 837:
			btn_UploadModel = (Button)target;
			btn_UploadModel.Click += btn_UploadModel_Click;
			break;
		case 838:
			lab_TrainStr = (Label)target;
			break;
		case 839:
			btn_ImageF1 = (Button)target;
			btn_ImageF1.Click += btn_ImageF1_Click;
			break;
		case 840:
			btn_ImageF2 = (Button)target;
			btn_ImageF2.Click += btn_ImageF1_Click;
			break;
		case 841:
			btn_ImageF3 = (Button)target;
			btn_ImageF3.Click += btn_ImageF1_Click;
			break;
		case 842:
			btn_ImageF4 = (Button)target;
			btn_ImageF4.Click += btn_ImageF1_Click;
			break;
		case 843:
			btn_ImageF5 = (Button)target;
			btn_ImageF5.Click += btn_ImageF1_Click;
			break;
		case 844:
			btn_ShowFontBinary = (Button)target;
			btn_ShowFontBinary.Click += Btn_ShowFontBinary_Click;
			break;
		case 845:
			img_FontConnected = (Canvas)target;
			break;
		case 846:
			grd_FontSetup = (Grid)target;
			break;
		case 847:
			lab_FontSetup = (Label)target;
			break;
		case 848:
			lab_FontSetupMain = (Label)target;
			break;
		case 849:
			chk_C1 = (CheckBox)target;
			break;
		case 850:
			chk_C2 = (CheckBox)target;
			break;
		case 851:
			chk_C3 = (CheckBox)target;
			break;
		case 852:
			chk_C4 = (CheckBox)target;
			break;
		case 853:
			chk_C5 = (CheckBox)target;
			break;
		case 854:
			chk_C6 = (CheckBox)target;
			break;
		case 855:
			chk_C7 = (CheckBox)target;
			break;
		case 856:
			chk_C8 = (CheckBox)target;
			break;
		case 857:
			chk_C9 = (CheckBox)target;
			break;
		case 858:
			chk_C10 = (CheckBox)target;
			break;
		case 859:
			chk_C11 = (CheckBox)target;
			break;
		case 860:
			chk_C12 = (CheckBox)target;
			break;
		case 861:
			chk_C13 = (CheckBox)target;
			break;
		case 862:
			chk_C14 = (CheckBox)target;
			break;
		case 863:
			chk_C15 = (CheckBox)target;
			break;
		case 864:
			chk_C16 = (CheckBox)target;
			break;
		case 865:
			btn_SaveFontCheckSetup = (Button)target;
			btn_SaveFontCheckSetup.Click += Btn_SaveFontCheckSetup_Click;
			break;
		case 866:
			grd_FontBinarySetup = (Grid)target;
			break;
		case 867:
			sl_BinaryShift = (Slider)target;
			sl_BinaryShift.ValueChanged += Sl_BinaryShift_ValueChanged;
			break;
		case 868:
			cb_OpenClose = (ComboBox)target;
			cb_OpenClose.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 869:
			cb_OpenCloseCount = (ComboBox)target;
			cb_OpenCloseCount.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 870:
			img_BinaryShift = (Canvas)target;
			break;
		case 871:
			lab_BinaryShift = (Label)target;
			break;
		case 872:
			img_FontBinaryShift = (Canvas)target;
			break;
		case 873:
			img_PatternBinaryShift = (Canvas)target;
			break;
		case 874:
			lab_FontImage1 = (Label)target;
			break;
		case 875:
			lab_FontImage2 = (Label)target;
			break;
		case 876:
			cb_MaskSelect = (ComboBox)target;
			cb_MaskSelect.SelectionChanged += Cb_MaskSelect_SelectionChanged;
			break;
		case 877:
			lab_MaskSelect = (Label)target;
			break;
		case 878:
			chk_UseFontDBMask = (CheckBox)target;
			chk_UseFontDBMask.Click += Chk_UseFontDBMask_Click;
			break;
		case 879:
			cb_OpenClose2 = (ComboBox)target;
			cb_OpenClose2.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 880:
			cb_OpenCloseCount2 = (ComboBox)target;
			cb_OpenCloseCount2.SelectionChanged += Cb_OpenClose_SelectionChanged;
			break;
		case 881:
			chk_UseMorphFirst = (CheckBox)target;
			chk_UseMorphFirst.Click += Chk_UseFontDBMask_Click;
			break;
		case 882:
			btn_RecoverPara = (Button)target;
			btn_RecoverPara.Click += Btn_RecoverPara_Click;
			break;
		case 883:
			txt_MinMatchScore = (TextBox)target;
			break;
		case 884:
			lab_MinMatchScore = (Label)target;
			break;
		case 885:
			btn_TrainCSV = (Button)target;
			btn_TrainCSV.Click += Btn_TrainCSV_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
