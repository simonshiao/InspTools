using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using InspTools.Other;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.AOI;

public class MatchToolWindow : System.Windows.Window, IComponentConnector
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

	private int PatternChannelSelect;

	private INSP_MATCHTOOL_PARAM CopyParam;

	private INSP_MATCHTOOL_PARAM BackupParam;

	private bool CopyParamSeted;

	private bool DontSave;

	private bool ReferenceShow = true;

	private int SelectFace;

	private int FaceIndex;

	private bool AlignMatchTextFound = true;

	private MatchPattern[] PublicPatternDB;

	private Mat ShowBitmap = new Mat();

	private int ImageListSource;

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private double CanvasWidth;

	private double CanvasHeight;

	private string[] filelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int TrackIndex;

	private int StationIndex;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private Stopwatch sw = new Stopwatch();

	private Button[] ToolsButton;

	private TextBox[] PatternName;

	private Button[] ChannelBtn;

	private Button[] ChannelBtn2;

	private Label[] PatternInfo;

	private string[] ChannelStr;

	private TextBox[] PDBText;

	private Button[] PDBName;

	private Image[] PDBImaage;

	private Label[] PDBLabel;

	private Image[] PatternImage;

	private bool AlignShow;

	private bool PreviewMode;

	private const int MaxPublicPattern = 210;

	private const int MaxPagePattern = 70;

	private const int MaxPublicMatchParam = 50;

	private Mat[] PatternMat = new Mat[20];

	private Mat[] PDBMat = new Mat[210];

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

	internal MatchToolWindow MainW;

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

	internal Label Label_Result;

	internal Rectangle rt_ReferenceRect1;

	internal Rectangle rt_ReferenceRect2;

	internal Label Label_ResultError;

	internal Label lab_FaceIndex;

	internal TextBox txt_PreviewMode;

	internal Rectangle bor_PatternSet;

	internal TabControl tab_Function;

	internal TabItem tab_Main;

	internal ComboBox cb_TrackerSelect;

	internal Button btn_Reverse;

	internal Button btn_RecipeCopy;

	internal Button btn_RecipePaste;

	internal TextBox txt_TrackerName;

	internal ToggleSwitch ts_TrackerON;

	internal Label lab_TrackIndex;

	internal Label lab_Pareto;

	internal ComboBox cb_ParetoChart;

	internal Label lab_Time;

	internal GroupBox gb_BasicSetup;

	internal Grid grd_SIMP_THD_2;

	internal Rectangle rt_Result;

	internal Button btn_Test;

	internal ComboBox cb_ErrorBin;

	internal Grid grd_SelectFace;

	internal Button btn_FaceA;

	internal Button btn_FaceB;

	internal Button btn_FaceC;

	internal Button btn_FaceD;

	internal CheckBox chk_SelectFace;

	internal RadioButton rad_Match;

	internal Grid grd_MatchTool;

	internal Grid grd_Found;

	internal TextBox txt_PatternName21;

	internal Image img_PatternFound;

	internal Label lab_PatternFoundInfo;

	internal Grid grd_Pattern;

	internal Label label_Pattern1;

	internal Image img_PatternSet;

	internal ComboBox cb_Pattern1;

	internal Button btn_ShowPattern1;

	internal Label label_Pattern2;

	internal ComboBox cb_Pattern2;

	internal Button btn_ShowPattern2;

	internal Label label_Pattern3;

	internal ComboBox cb_Pattern3;

	internal Button btn_ShowPattern3;

	internal Label label_Pattern4;

	internal ComboBox cb_Pattern4;

	internal Button btn_ShowPattern4;

	internal CheckBox chk_EnableAISimilarityCheck;

	internal TextBox txt_SimilarityScore;

	internal Button btn_SetOnnxFile;

	internal TextBox txt_SimilarityScoreResult;

	internal Label lab_PatternSetInfo;

	internal Ellipse ep_Result;

	internal TextBox txt_MatchScoreMin;

	internal TextBox txt_MatchScoreMax;

	internal Label lab_MatchScore;

	internal Button btn_ReDraw_Copy;

	internal Button btn_ReDraw_Copy1;

	internal Button btn_ReDraw_Copy2;

	internal Grid grd_Second;

	internal CheckBox chk_RotateInspect;

	internal CheckBox chk_TextColorisWhite;

	internal ComboBox cb_DEStart;

	internal ComboBox cb_DEStartCount;

	internal RadioButton rad_EnableAutolThd;

	internal RadioButton rad_EnableManualThd;

	internal RangeSlider rs_BinaryThd;

	internal TextBox txt_BinaryThdMin;

	internal TextBox txt_BinaryThdMax;

	internal CheckBox chk_MaskInFound;

	internal Label lab_Score1;

	internal Label lab_Score2;

	internal Label lab_Score3;

	internal Label lab_Score4;

	internal Button btn_ReSetComboBox1;

	internal Button btn_ReSetComboBox2;

	internal Button btn_ReSetComboBox3;

	internal TextBox txt_OnnxName;

	internal Grid grd_OCR;

	internal TextBox txt_OCRSetup;

	internal TextBox txt_OCRResult;

	internal RadioButton rad_OCR;

	internal CheckBox chk_OCRTextColorisWhite;

	internal ComboBox cb_Channel_OCR;

	internal Grid grd_Third;

	internal RadioButton rad_UseOldDB;

	internal RadioButton rad_UsePublicDB;

	internal Grid grd_QuickSetup;

	internal ComboBox cb_QuickSetup;

	internal Button btn_QuickSetupFind;

	internal Button btn_QuickSetupRead;

	internal Button btn_QuickSetupWrite;

	internal Button btn_QuickSetupLock;

	internal Button btn_QuickSetupCancel;

	internal TextBox txt_QuickSetupName;

	internal CheckBox cb_AlignToMatchText;

	internal Expander exp_TrackerSetup;

	internal Grid grd_SIMP_DilateErode1_Copy;

	internal Grid grd_AdvSetup;

	internal RadioButton radMatchText;

	internal RadioButton radMatchPattern;

	internal Label txt_Tracker1Pos_Copy;

	internal Label txt_Tracker1Pos_Copy1;

	internal Label txt_Tracker1Pos_Copy2;

	internal Label txt_Tracker1Pos_Copy3;

	internal TextBox txt_DiffRatio;

	internal TextBox txt_WidthRatio;

	internal TextBox txt_AreaRatio;

	internal TextBox txt_HeightRatio;

	internal TextBox txt_ColorMatchScoreResult;

	internal Label txt_Tracker1Pos_Copy4;

	internal Image img_PatternFoundBinary;

	internal TextBox txt_MinColorMatchScore;

	internal ComboBox cb_FindCloseArea;

	internal TextBox txt_CloseArea;

	internal TextBox txt_MinTextWidth;

	internal TextBox txt_MinTextWidthResult;

	internal TextBox txt_MinTextHeight;

	internal TextBox txt_MinTextHeightResult;

	internal TextBox txt_CloseArea2;

	internal TextBox txt_CloseArea3;

	internal CheckBox chk_MatchShape;

	internal TextBox txt_MinShapeScore;

	internal TextBox txt_MinShapeScoreResult;

	internal TextBox txt_MaxTextWidth;

	internal TextBox txt_MaxTextHeight;

	internal ComboBox cb_FindTextArea;

	internal TextBox txt_TextArea;

	internal TextBox txt_MaxCloseArea;

	internal TextBox txt_MaxTextArea;

	internal TextBox txt_TxtPosDiff;

	internal TextBox txt_TxtWidthDiff;

	internal TextBox txt_TxtHeightDiff;

	internal TextBox txt_TxtAreaDiff;

	internal CheckBox chk_UseFoundToMask;

	internal Button btn_PrevTracker;

	internal Button btn_NextTracker;

	internal TabItem tab_PatternDB;

	internal GroupBox gb_BasicSetup_Copy;

	internal Grid grd_SIMP_THD_1;

	internal Button btn_ChannelR;

	internal Button btn_ChannelG;

	internal Button btn_ChannelB;

	internal Button btn_ChannelH;

	internal Button btn_ChannelS;

	internal Button btn_ChannelV;

	internal Button btn_ChannelGray;

	internal Button btn_ChannelC1;

	internal Button btn_ChannelC2;

	internal Button btn_ChannelC3;

	internal Button btn_ChannelRemove;

	internal TextBox txt_PatternName1;

	internal Image img_PatternImg1;

	internal Button btn_SetPattern;

	internal Label lab_PatternInfo1;

	internal TextBox txt_PatternName6;

	internal Image img_PatternImg6;

	internal Label lab_PatternInfo6;

	internal TextBox txt_PatternName11;

	internal Image img_PatternImg11;

	internal Label lab_PatternInfo11;

	internal TextBox txt_PatternName16;

	internal Image img_PatternImg16;

	internal Label lab_PatternInfo16;

	internal TextBox txt_PatternName2;

	internal Image img_PatternImg2;

	internal Label lab_PatternInfo2;

	internal TextBox txt_PatternName7;

	internal Image img_PatternImg7;

	internal Label lab_PatternInfo7;

	internal TextBox txt_PatternName12;

	internal Image img_PatternImg12;

	internal Label lab_PatternInfo12;

	internal TextBox txt_PatternName17;

	internal Image img_PatternImg17;

	internal Label lab_PatternInfo17;

	internal TextBox txt_PatternName3;

	internal Image img_PatternImg3;

	internal Label lab_PatternInfo3;

	internal TextBox txt_PatternName8;

	internal Image img_PatternImg8;

	internal Label lab_PatternInfo8;

	internal TextBox txt_PatternName13;

	internal Image img_PatternImg13;

	internal Label lab_PatternInfo13;

	internal TextBox txt_PatternName18;

	internal Image img_PatternImg18;

	internal Label lab_PatternInfo18;

	internal TextBox txt_PatternName4;

	internal Image img_PatternImg4;

	internal Label lab_PatternInfo4;

	internal TextBox txt_PatternName9;

	internal Image img_PatternImg9;

	internal Label lab_PatternInfo9;

	internal TextBox txt_PatternName14;

	internal Image img_PatternImg14;

	internal Label lab_PatternInfo14;

	internal TextBox txt_PatternName19;

	internal Image img_PatternImg19;

	internal Label lab_PatternInfo19;

	internal TextBox txt_PatternName5;

	internal Image img_PatternImg5;

	internal Label lab_PatternInfo5;

	internal TextBox txt_PatternName10;

	internal Image img_PatternImg10;

	internal Label lab_PatternInfo10;

	internal TextBox txt_PatternName15;

	internal Image img_PatternImg15;

	internal Label lab_PatternInfo15;

	internal TextBox txt_PatternName20;

	internal Image img_PatternImg20;

	internal Label lab_PatternInfo20;

	internal TabItem tab_PublicDB;

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

	internal TextBox txt_AllResult;

	internal Button btn_TrackerShow;

	internal Button btn_TestNextImg;

	internal Button btn_TestPrevImg;

	internal Button btn_Tools1;

	internal Button btn_Tools2;

	internal Button btn_Tools3;

	internal Button btn_Tools4;

	internal Button btn_Tools5;

	internal Button btn_Tools6;

	internal Button btn_Tools7;

	internal Button btn_Tools8;

	internal Button btn_Tools9;

	internal Button btn_Tools10;

	internal Button btn_Tools11;

	internal Button btn_Tools12;

	internal Button btn_Tools13;

	internal Button btn_Tools14;

	internal Button btn_Tools15;

	internal Button btn_Tools16;

	internal Button btn_Tools17;

	internal Button btn_Tools18;

	internal Button btn_Tools19;

	internal Button btn_Tools20;

	internal Button btn_ReferenceShow;

	internal Button btn_PreviewSwitch_Copy;

	internal Label Label_Index;

	internal Button btn_AlignShow;

	internal Grid grd_Align;

	internal ComboBox cb_Track1AlignWayX;

	internal RadioButton rad_Tracker1AutoSize;

	internal ComboBox cb_Track1AutoSize;

	internal RadioButton rad_Tracker1AlignSelect;

	internal Button btn_TrackerReset;

	internal ComboBox cb_Track1AlignWayY;

	internal Button btn_Tracker1SizeChange;

	internal Label txt_Tracker1Pos;

	internal TextBox lab_LockMode;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public MatchToolWindow(User_Permissions permissions)
	{
		inited = false;
		InitializeComponent();
		inited = true;
		THD_Preview = false;
		User = permissions;
		Label_ResultError.Visibility = Visibility.Hidden;
		CanvasWidth = cnv_Image.Width;
		CanvasHeight = cnv_Image.Height;
		for (int i = 0; i < 20; i++)
		{
			PatternMat[i] = new Mat();
		}
		for (int j = 0; j < 210; j++)
		{
			PDBMat[j] = new Mat();
		}
		ToolsButton = new Button[20]
		{
			btn_Tools1, btn_Tools2, btn_Tools3, btn_Tools4, btn_Tools5, btn_Tools6, btn_Tools7, btn_Tools8, btn_Tools9, btn_Tools10,
			btn_Tools11, btn_Tools12, btn_Tools13, btn_Tools14, btn_Tools15, btn_Tools16, btn_Tools17, btn_Tools18, btn_Tools19, btn_Tools20
		};
		PatternName = new TextBox[20]
		{
			txt_PatternName1, txt_PatternName2, txt_PatternName3, txt_PatternName4, txt_PatternName5, txt_PatternName6, txt_PatternName7, txt_PatternName8, txt_PatternName9, txt_PatternName10,
			txt_PatternName11, txt_PatternName12, txt_PatternName13, txt_PatternName14, txt_PatternName15, txt_PatternName16, txt_PatternName17, txt_PatternName18, txt_PatternName19, txt_PatternName20
		};
		PatternImage = new Image[20]
		{
			img_PatternImg1, img_PatternImg2, img_PatternImg3, img_PatternImg4, img_PatternImg5, img_PatternImg6, img_PatternImg7, img_PatternImg8, img_PatternImg9, img_PatternImg10,
			img_PatternImg11, img_PatternImg12, img_PatternImg13, img_PatternImg14, img_PatternImg15, img_PatternImg16, img_PatternImg17, img_PatternImg18, img_PatternImg19, img_PatternImg20
		};
		PatternInfo = new Label[20]
		{
			lab_PatternInfo1, lab_PatternInfo2, lab_PatternInfo3, lab_PatternInfo4, lab_PatternInfo5, lab_PatternInfo6, lab_PatternInfo7, lab_PatternInfo8, lab_PatternInfo9, lab_PatternInfo10,
			lab_PatternInfo11, lab_PatternInfo12, lab_PatternInfo13, lab_PatternInfo14, lab_PatternInfo15, lab_PatternInfo16, lab_PatternInfo17, lab_PatternInfo18, lab_PatternInfo19, lab_PatternInfo20
		};
		ChannelBtn = new Button[11]
		{
			btn_ChannelR, btn_ChannelG, btn_ChannelB, btn_ChannelH, btn_ChannelS, btn_ChannelV, btn_ChannelGray, btn_ChannelC1, btn_ChannelC2, btn_ChannelC3,
			btn_ChannelRemove
		};
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
		rt_ReferenceRect1.Visibility = Visibility.Hidden;
		rt_ReferenceRect2.Visibility = Visibility.Hidden;
		line_X1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		line_Y1.Visibility = (line_X2.Visibility = Visibility.Hidden);
		grd_OCR.Visibility = Visibility.Hidden;
		txt_PreviewMode.Visibility = Visibility.Hidden;
	}

	public void SetPublicDB(MatchPattern[] db)
	{
		PublicPatternDB = db;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(int index, CVInspection station, int stationIndex, int faceIndex = 0)
	{
		TrackIndex = index;
		inspection = station;
		FaceIndex = faceIndex;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		StationIndex = stationIndex;
		UpdateUI = false;
		tab_Function.SelectedIndex = 0;
		cb_TrackerSelect.Items.Clear();
		cb_Track1AlignWayX.Items.Clear();
		cb_Track1AlignWayY.Items.Clear();
		cb_Track1AutoSize.Items.Clear();
		cb_Pattern1.Items.Clear();
		cb_Pattern2.Items.Clear();
		cb_Pattern3.Items.Clear();
		cb_Pattern4.Items.Clear();
		SetFaceIndex(faceIndex);
		try
		{
			for (int i = 0; i < 20; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.MatchParam[i].TrackerName);
				if (!inspection.paramFile.AdvLocateToolON)
				{
					if (i < 2)
					{
						cb_Track1AlignWayX.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
						cb_Track1AlignWayY.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
						if (i < 1)
						{
							cb_Track1AutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
						}
					}
					continue;
				}
				if (inspection.paramFile.VSLocateParam[i].TrackerON)
				{
					cb_Track1AlignWayX.Items.Add(i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName;
					comboBoxItem.IsEnabled = false;
					cb_Track1AlignWayX.Items.Add(comboBoxItem);
					new ComboBoxItem
					{
						Content = i + 1 + " : " + inspection.paramFile.VSLocateParam[i].TrackerName,
						IsEnabled = false
					};
				}
				if (inspection.paramFile.HSLocateParam[i].TrackerON)
				{
					cb_Track1AlignWayY.Items.Add(i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName);
				}
				else
				{
					ComboBoxItem comboBoxItem2 = new ComboBoxItem();
					comboBoxItem2.Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName;
					comboBoxItem2.IsEnabled = false;
					cb_Track1AlignWayY.Items.Add(comboBoxItem2);
					new ComboBoxItem
					{
						Content = i + 1 + " : " + inspection.paramFile.HSLocateParam[i].TrackerName,
						IsEnabled = false
					};
				}
				if (inspection.paramFile.AdvLocateParam[i].TrackerON)
				{
					cb_Track1AutoSize.Items.Add(i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName);
					continue;
				}
				ComboBoxItem comboBoxItem3 = new ComboBoxItem();
				comboBoxItem3.Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName;
				comboBoxItem3.IsEnabled = false;
				cb_Track1AutoSize.Items.Add(comboBoxItem3);
				new ComboBoxItem
				{
					Content = i + 1 + " : " + inspection.paramFile.AdvLocateParam[i].TrackerName,
					IsEnabled = false
				};
			}
			UpdateToolBox();
			UpdatePatternImage();
			cb_TrackerSelect.SelectedIndex = index;
			cb_Pattern1.SelectedIndex = -1;
			cb_Pattern2.SelectedIndex = -1;
			UpdateQuickSetupList(-1);
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "MTW01-MatchToolWindow-SetInspection()";
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
	private void UpdateQuickSetupList(int index)
	{
		UpdateUI = false;
		try
		{
			cb_QuickSetup.Items.Clear();
			string text = "";
			for (int i = 0; i < 50; i++)
			{
				text = string.Format("{0}:{1}", i + 1, inspection.paramFile.MatchQuickSetupParam[i].QuickSetupName);
				cb_QuickSetup.Items.Add(text);
			}
			cb_QuickSetup.SelectedIndex = index;
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			UpdateUI = true;
			string text2 = "MTW01-MatchToolWindow-UpdateQuickSetupList()";
			text2 += ex.ToString();
			UpdateUI = true;
			CVBuffer.SaveLog(text2, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text2, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdatePatternDBComboBox(bool UsePublicDB)
	{
		cb_Pattern1.Items.Clear();
		cb_Pattern2.Items.Clear();
		cb_Pattern3.Items.Clear();
		cb_Pattern4.Items.Clear();
		if (UsePublicDB)
		{
			tab_PatternDB.Visibility = Visibility.Hidden;
			tab_PublicDB.Visibility = Visibility.Visible;
			for (int i = 0; i < 210; i++)
			{
				if (PublicPatternDB[i].PatternON)
				{
					cb_Pattern1.Items.Add(i + 1 + " : " + PublicPatternDB[i].PatternName);
					cb_Pattern2.Items.Add(i + 1 + " : " + PublicPatternDB[i].PatternName);
					cb_Pattern3.Items.Add(i + 1 + " : " + PublicPatternDB[i].PatternName);
					cb_Pattern4.Items.Add(i + 1 + " : " + PublicPatternDB[i].PatternName);
					continue;
				}
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = i + 1 + " : " + PublicPatternDB[i].PatternName;
				comboBoxItem.IsEnabled = false;
				cb_Pattern1.Items.Add(comboBoxItem);
				ComboBoxItem comboBoxItem2 = new ComboBoxItem();
				comboBoxItem2.Content = i + 1 + " : " + PublicPatternDB[i].PatternName;
				comboBoxItem2.IsEnabled = false;
				cb_Pattern2.Items.Add(comboBoxItem2);
				ComboBoxItem comboBoxItem3 = new ComboBoxItem();
				comboBoxItem3.Content = i + 1 + " : " + PublicPatternDB[i].PatternName;
				comboBoxItem3.IsEnabled = false;
				cb_Pattern3.Items.Add(comboBoxItem3);
				ComboBoxItem comboBoxItem4 = new ComboBoxItem();
				comboBoxItem4.Content = i + 1 + " : " + PublicPatternDB[i].PatternName;
				comboBoxItem4.IsEnabled = false;
				cb_Pattern4.Items.Add(comboBoxItem4);
			}
			cb_Pattern1.SelectedIndex = P1SelectPublic;
			cb_Pattern2.SelectedIndex = P2SelectPublic;
			cb_Pattern3.SelectedIndex = P3SelectPublic;
			cb_Pattern4.SelectedIndex = P4SelectPublic;
			return;
		}
		tab_PatternDB.Visibility = Visibility.Visible;
		tab_PublicDB.Visibility = Visibility.Hidden;
		for (int j = 0; j < 20; j++)
		{
			if (inspection.paramFile.patternDB[j].PatternON)
			{
				cb_Pattern1.Items.Add(j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName);
				cb_Pattern2.Items.Add(j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName);
				cb_Pattern3.Items.Add(j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName);
				cb_Pattern4.Items.Add(j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName);
				continue;
			}
			ComboBoxItem comboBoxItem5 = new ComboBoxItem();
			comboBoxItem5.Content = j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName;
			comboBoxItem5.IsEnabled = false;
			cb_Pattern1.Items.Add(comboBoxItem5);
			ComboBoxItem comboBoxItem6 = new ComboBoxItem();
			comboBoxItem6.Content = j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName;
			comboBoxItem6.IsEnabled = false;
			cb_Pattern2.Items.Add(comboBoxItem6);
			ComboBoxItem comboBoxItem7 = new ComboBoxItem();
			comboBoxItem7.Content = j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName;
			comboBoxItem7.IsEnabled = false;
			cb_Pattern3.Items.Add(comboBoxItem7);
			ComboBoxItem comboBoxItem8 = new ComboBoxItem();
			comboBoxItem8.Content = j + 1 + " : " + inspection.paramFile.patternDB[j].PatternName;
			comboBoxItem8.IsEnabled = false;
			cb_Pattern4.Items.Add(comboBoxItem8);
		}
		cb_Pattern1.SelectedIndex = P1SelectOld;
		cb_Pattern2.SelectedIndex = P2SelectOld;
		cb_Pattern3.SelectedIndex = P3SelectOld;
		cb_Pattern4.SelectedIndex = P4SelectOld;
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
			PDBText[i].Text = PublicPatternDB[num + i].PatternName;
			if (!PublicPatternDB[num + i].PatternImage.Empty())
			{
				PDBMat[num + i] = PublicPatternDB[num + i].PatternImage;
				PDBImaage[i].Source = PDBMat[num + i].ToBitmapSource();
				PDBLabel[i].Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[i].PatternChannel], PDBMat[num + i].Width, PDBMat[num + i].Height);
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
			if (!PublicPatternDB[i].PatternImage.Empty())
			{
				PDBMat[i] = PublicPatternDB[i].PatternImage;
			}
		}
		for (int j = 0; j < 70; j++)
		{
			PDBName[j].Content = j + 1;
			PDBLabel[j].Content = "";
			PDBText[j].Text = PublicPatternDB[j].PatternName;
			if (!PublicPatternDB[j].PatternImage.Empty())
			{
				PDBImaage[j].Source = PDBMat[j].ToBitmapSource();
				PDBLabel[j].Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[j].PatternChannel], PDBMat[j].Width, PDBMat[j].Height);
			}
			else
			{
				PDBImaage[j].Source = mat.ToBitmapSource();
				PDBLabel[j].Content = "";
			}
		}
		for (int k = 0; k < 20; k++)
		{
			PatternInfo[k].Content = "";
			PatternName[k].Text = inspection.paramFile.patternDB[k].PatternName;
			if (!inspection.paramFile.patternDB[k].PatternImage.Empty())
			{
				PatternMat[k] = inspection.paramFile.patternDB[k].PatternImage;
				ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[k]);
				PatternImage[k].Source = source;
				PatternInfo[k].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[k].PatternChannel], PatternMat[k].Width, PatternMat[k].Height);
			}
			else
			{
				PatternImage[k].Source = mat.ToBitmapSource();
			}
		}
	}

	private void UpdateToolBox()
	{
		for (int i = 0; i < 20; i++)
		{
			if (inspection.paramFile.MatchParam[i].TrackerON)
			{
				if (inspection.iResult.MatchResult[i] == TOOL_RESULT.EM_OK)
				{
					if (inspection.paramFile.MatchParam[i].EnableSelectFace)
					{
						if (inspection.paramFile.MatchParam[i].FaceIndex == FaceIndex)
						{
							ToolsButton[i].Background = Brushes.Lime;
						}
						else
						{
							ToolsButton[i].Background = Brushes.DarkGray;
						}
					}
					else
					{
						ToolsButton[i].Background = Brushes.Lime;
					}
				}
				else if (inspection.iResult.MatchResult[i] == TOOL_RESULT.EM_NG)
				{
					ToolsButton[i].Background = Brushes.Red;
				}
				else
				{
					ToolsButton[i].Background = Brushes.Cyan;
				}
			}
			else
			{
				ToolsButton[i].Background = Brushes.White;
			}
			ToolsButton[i].ToolTip = inspection.paramFile.MatchParam[i].TrackerName;
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

	public void SetBuffer(Mat mat, OpenCvSharp.Rect locateRect, bool CopyValue = true)
	{
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
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImageSource(int imagelistsource, string[] flist, int findex, int recentindex, int okindex, int ngindex)
	{
		try
		{
			ImageListSource = imagelistsource;
			filelist = flist;
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
				if (filelist != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
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
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.MatchParam[TrackIndex].TrackerName) + "BT01-MatchWindow-SetImageSource()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	public void SetPreviewMode(bool on)
	{
		PreviewMode = on;
		txt_PreviewMode.Visibility = Visibility.Visible;
		CopyValueToUI();
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		inited = false;
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
			txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
			result = false;
			bor_SearchRect1.Uid = "1";
			DontSave = true;
			btn_Test.IsEnabled = false;
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
		txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
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
			txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
			return false;
		}
		txt_Tracker1Pos.Content = string.Format("[1] X:{0}, Y:{1}, W:{2}, H:{3}", rect1.X, rect1.Y, rect1.Width, rect1.Height);
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
			txt_Tracker1Pos.Content = string.Format("[A] X:{0}, Y:{1}, W:{2}, H:{3}", 0, 0, 0, 0);
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
		Label_ResultError.Visibility = Visibility.Hidden;
		if (result)
		{
			Label_Result.Content = "OK";
			Label_Result.Background = Brushes.Lime;
			ToolsButton[TrackIndex].Background = Brushes.Lime;
			rt_Result.Fill = Brushes.Lime;
			ep_Result.Fill = Brushes.Green;
		}
		else
		{
			Label_Result.Content = "NG";
			Label_Result.Background = Brushes.Red;
			ToolsButton[TrackIndex].Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
			ep_Result.Fill = Brushes.Red;
		}
	}

	private void SetResult(int result, string msg)
	{
		if (result == 2)
		{
			Label_Result.Visibility = Visibility.Hidden;
			Label_ResultError.Visibility = Visibility.Visible;
			Label_ResultError.Content = msg;
			Label_ResultError.Background = Brushes.Red;
			ToolsButton[TrackIndex].Background = Brushes.Red;
			rt_Result.Fill = Brushes.Red;
		}
	}

	private void SetErrorResult(string str)
	{
		Label_Result.Visibility = Visibility.Visible;
		Label_Result.Content = str;
		Label_Result.Background = Brushes.Red;
		ToolsButton[TrackIndex].Background = Brushes.Red;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_TrackerSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		Label_ResultError.Visibility = Visibility.Hidden;
		WriteUIValueToParam();
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		CopyValueToUI();
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
		else
		{
			DrawBackGround();
		}
		UpdateToolBox();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_TrackerReset_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Tracker value will set to default！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测区域的位置将重设成预设值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測區域的位置將重設成預設值！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes && ((Button)sender).Uid == "0")
		{
			bor_SearchRect1.SetValue(Canvas.LeftProperty, 0.0);
			bor_SearchRect1.SetValue(Canvas.TopProperty, 0.0);
			bor_SearchRect1.SetValue(FrameworkElement.WidthProperty, 100.0);
			bor_SearchRect1.SetValue(FrameworkElement.HeightProperty, 100.0);
			if (TrackerShow)
			{
				bor_SearchRect1.Visibility = Visibility.Visible;
			}
			bor_SearchRect1.Uid = "0";
			btn_Test.IsEnabled = true;
			DontSave = false;
		}
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

	private void Ts_TrackerON_Click(object sender, RoutedEventArgs e)
	{
		if (User >= User_Permissions.USER_Administrator || !inspection.paramFile.MatchParam[TrackIndex].Locked)
		{
			if (ts_TrackerON.IsChecked.Value)
			{
				inspection.paramFile.MatchParam[TrackIndex].TrackerON = true;
				CopyValueToUI();
				return;
			}
			WriteUIValueToParam();
			inspection.paramFile.MatchParam[TrackIndex].TrackerON = false;
			inspection.paramFile.MatchParam[TrackIndex].ValueChangeFlag = true;
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Hidden;
			gb_BasicSetup.Visibility = Visibility.Hidden;
			exp_TrackerSetup.Visibility = Visibility.Hidden;
			ToolsButton[TrackIndex].Background = Brushes.White;
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
				if (filelist == null || filelist.Length == 0)
				{
					return;
				}
				if (fileIndex < 0)
				{
					fileIndex = 0;
				}
				if (fileIndex + 1 < filelist.Length)
				{
					fileIndex++;
				}
				else
				{
					fileIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
				mat = CVBuffer.ImRead(filelist[fileIndex]);
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
				btn_Test.IsEnabled = false;
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
				SetFaceIndex(FaceIndex);
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.MatchParam[TrackIndex].TrackerName) + "BT01-MatchWindow-Btn_TestNextImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
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
				if (filelist == null || filelist.Length == 0)
				{
					return;
				}
				if (fileIndex - 1 > 0)
				{
					fileIndex--;
				}
				else
				{
					fileIndex = filelist.Length - 1;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
				mat = CVBuffer.ImRead(filelist[fileIndex]);
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
				btn_Test.IsEnabled = false;
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
				SetFaceIndex(FaceIndex);
				DontSave = false;
				btn_Test.IsEnabled = true;
				SetBuffer(inspection.buffers.LocatedSrc, inspection.LocateRect);
				Btn_Test_Click(this, null);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Format("{0}:{1}-", TrackIndex + 1, inspection.paramFile.MatchParam[TrackIndex].TrackerName) + "BT01-MatchWindow-Btn_TestPrevImg_Click()-01-", ex.ToString()), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Btn_Tools1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		cb_TrackerSelect.SelectedIndex = num;
		for (int i = 0; i < 20; i++)
		{
			ToolsButton[i].BorderBrush = Brushes.Gray;
		}
		ToolsButton[num].BorderBrush = Brushes.Blue;
	}

	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		CopyParamSeted = true;
		CopyParam = inspection.paramFile.MatchParam[TrackIndex];
		btn_RecipeCopy.Background = Brushes.PaleGreen;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		if ((User >= User_Permissions.USER_Administrator || !inspection.paramFile.MatchParam[TrackIndex].Locked) && CopyParamSeted)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Overwrite Tracker value?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("目前检测区域的数值将被覆盖", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("目前檢測區域的數值將被覆蓋！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				string trackerName = inspection.paramFile.MatchParam[TrackIndex].TrackerName;
				inspection.paramFile.MatchParam[TrackIndex] = CopyParam;
				inspection.paramFile.MatchParam[TrackIndex].TrackerName = trackerName;
				PasteState = true;
				CopyValueToUI();
				PasteState = false;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Reverse_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Parameters will Revert to Previous save！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("检测参数将还原至上次储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("檢測參數將還原至上次儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			inspection.paramFile.MatchParam[TrackIndex] = inspection.paramFile_Backup.MatchParam[TrackIndex];
			CopyValueToUI();
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
			if (!IsNumeric(txt_MatchScoreMin.Text))
			{
				txt_MatchScoreMin.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MatchScoreMin.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MatchScoreMax.Text))
			{
				txt_MatchScoreMax.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MatchScoreMax.Background = Brushes.White;
			}
			if (!IsNumeric(txt_CloseArea.Text))
			{
				txt_CloseArea.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_CloseArea.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MinTextWidth.Text))
			{
				txt_MinTextWidth.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MinTextWidth.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MinTextHeight.Text))
			{
				txt_MinTextHeight.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MinTextHeight.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MaxTextWidth.Text))
			{
				txt_MaxTextWidth.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MaxTextWidth.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MaxTextHeight.Text))
			{
				txt_MaxTextHeight.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MaxTextHeight.Background = Brushes.White;
			}
			if (!IsNumeric(txt_MinColorMatchScore.Text))
			{
				txt_MinColorMatchScore.Background = Brushes.Fuchsia;
				result = false;
			}
			else
			{
				txt_MinColorMatchScore.Background = Brushes.White;
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MTW08-MatchToolWindow-CheckTextBox()-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show("輸入字元不合法，請重新輸入", "錯誤");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_PrevTracker(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		if (ts_TrackerON.IsChecked == true)
		{
			WriteUIValueToParam();
		}
		UpdateUI = false;
		if (TrackIndex == 0)
		{
			cb_TrackerSelect.SelectedIndex = 19;
		}
		else
		{
			cb_TrackerSelect.SelectedIndex--;
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		UpdateUI = true;
		CopyValueToUI();
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
		else
		{
			DrawBackGround();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NextTracker_Click(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		if (!CheckTextBox())
		{
			txt_AllResult.Clear();
			txt_AllResult.Text = LanguageManager.Instance["InputError"];
			return;
		}
		if (ts_TrackerON.IsChecked == true)
		{
			WriteUIValueToParam();
		}
		UpdateUI = false;
		if (TrackIndex == 19)
		{
			cb_TrackerSelect.SelectedIndex = 0;
		}
		else
		{
			cb_TrackerSelect.SelectedIndex++;
		}
		TrackIndex = cb_TrackerSelect.SelectedIndex;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		UpdateUI = true;
		CopyValueToUI();
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
		else
		{
			DrawBackGround();
		}
	}

	private void Nu_ThdUp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Chk_ThdUpON_Click(object sender, RoutedEventArgs e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
		}
	}

	private void Cb_ChannelIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
	}

	private void Cb_FilterNoise_Click(object sender, RoutedEventArgs e)
	{
		if (ts_TrackerON.IsChecked == true)
		{
			Btn_Test_Click(sender, e);
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
			Rectangle rectangle = rt_ReferenceRect1;
			visibility5 = (rt_ReferenceRect2.Visibility = Visibility.Hidden);
			rectangle.Visibility = visibility5;
			ReferenceShow = false;
		}
		else
		{
			ReferenceShow = true;
			if (rad_Tracker1AlignSelect.IsChecked == true)
			{
				Line line4 = line_X1;
				Visibility visibility5 = (line_Y1.Visibility = Visibility.Visible);
				line4.Visibility = visibility5;
			}
			else
			{
				rt_ReferenceRect1.Visibility = Visibility.Visible;
			}
		}
	}

	private void Cb_Track1AutoSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && rad_Tracker1AutoSize.IsChecked == true)
		{
			SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[cb_Track1AutoSize.SelectedIndex].InsideRect);
			rt_ReferenceRect1.Visibility = Visibility.Visible;
			Line line = line_X1;
			Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
			line.Visibility = visibility;
		}
	}

	private void Cb_Track1AlignWayX_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateUI && rad_Tracker1AlignSelect.IsChecked == true)
		{
			SetReferenceLine(line_X1, line_Y1, cb_Track1AlignWayX.SelectedIndex, cb_Track1AlignWayY.SelectedIndex);
			rt_ReferenceRect1.Visibility = Visibility.Hidden;
			Line line = line_X1;
			Visibility visibility = (line_Y1.Visibility = Visibility.Visible);
			line.Visibility = visibility;
		}
	}

	private void Rad_Tracker1AlignSelect_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track1AlignWayX_SelectionChanged(this, null);
	}

	private void Rad_Tracker1AutoSize_Click(object sender, RoutedEventArgs e)
	{
		Cb_Track1AutoSize_SelectionChanged(this, null);
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetPattern_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
			if (PatternChannelSelect == 10)
			{
				PatternMat[num] = new Mat();
				PatternImage[num].Source = null;
				PatternInfo[num].Content = "";
				inspection.paramFile.patternDB[num].PatternON = false;
				inspection.paramFile.patternDB[num].ValueChangeFlag = true;
				MatchPattern obj = inspection.paramFile.patternDB[num];
				obj.ValueChangeFlag = true;
				obj.PatternON = false;
				obj.PatternName = "";
				obj.PatternChannel = 0;
				obj.PatternPath = "";
				obj.PatternImage = PatternMat[num];
			}
			else if (GetPatternTracker(ref rect))
			{
				PatternMat[num] = new Mat(inspection.buffers.GetChannelMat(PatternChannelSelect), rect).Clone();
				WritePattern(num, PatternChannelSelect);
				ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num]);
				PatternImage[num].Source = source;
				PatternInfo[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num].PatternChannel], PatternMat[num].Width, PatternMat[num].Height);
			}
		}
		catch (Exception ex)
		{
			string text = "MTW08-MatchToolWindow-Btn_SetPattern_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Btn_ChannelR_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		for (int i = 0; i < 11; i++)
		{
			ChannelBtn[i].Background = Brushes.White;
		}
		for (int j = 0; j < 11; j++)
		{
			ChannelBtn2[j].Background = Brushes.White;
		}
		PatternChannelSelect = Convert.ToInt32(button.Uid);
		ChannelBtn[PatternChannelSelect].Background = Brushes.YellowGreen;
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
		if (UpdateUI && inited && e.Source is TabControl)
		{
			tab_Main.Background = Brushes.White;
			tab_PatternDB.Background = Brushes.White;
			tab_PublicDB.Background = Brushes.White;
			if (tab_Function.SelectedIndex == 0)
			{
				tab_Main.Background = Brushes.Yellow;
				bor_SearchRect1.Visibility = Visibility.Visible;
				bor_PatternSet.Visibility = Visibility.Hidden;
				DrawBackGround();
				CopyValueToUI();
			}
			else if (tab_Function.SelectedIndex == 1)
			{
				WriteUIValueToParam();
				tab_PatternDB.Background = Brushes.Yellow;
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_PatternSet.Visibility = Visibility.Visible;
				DrawBackGround(PatternChannelSelect);
			}
			else if (tab_Function.SelectedIndex == 2)
			{
				WriteUIValueToParam();
				tab_PublicDB.Background = Brushes.Yellow;
				bor_SearchRect1.Visibility = Visibility.Hidden;
				bor_PatternSet.Visibility = Visibility.Visible;
				DrawBackGround(PatternChannelSelect);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_Pattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || !inited)
		{
			return;
		}
		ComboBox obj = (ComboBox)sender;
		int num = 0;
		string uid = obj.Uid;
		num = ((uid == "0") ? cb_Pattern1.SelectedIndex : ((uid == "1") ? cb_Pattern2.SelectedIndex : ((uid == "2") ? cb_Pattern3.SelectedIndex : ((!(uid == "3")) ? cb_Pattern1.SelectedIndex : cb_Pattern4.SelectedIndex))));
		if (rad_UsePublicDB.IsChecked == true)
		{
			if (num >= 0 && num < 210 && PublicPatternDB[num].PatternON && !PDBMat[num].Empty())
			{
				img_PatternSet.Source = PDBMat[num].ToBitmapSource();
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[num].PatternChannel], PDBMat[num].Width, PDBMat[num].Height);
			}
		}
		else if (num >= 0 && num < 20 && inspection.paramFile.patternDB[num].PatternON && !PatternMat[num].Empty())
		{
			ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num]);
			img_PatternSet.Source = source;
			lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num].PatternChannel], PatternMat[num].Width, PatternMat[num].Height);
		}
	}

	private void QuickSetupLock(bool enable)
	{
		if (enable)
		{
			grd_Pattern.IsEnabled = false;
			grd_Second.IsEnabled = false;
			txt_MatchScoreMin.IsEnabled = false;
			txt_MatchScoreMax.IsEnabled = false;
			grd_Third.IsEnabled = false;
			grd_AdvSetup.IsEnabled = false;
			btn_ReSetComboBox1.IsEnabled = false;
			btn_ReSetComboBox2.IsEnabled = false;
			btn_ReSetComboBox3.IsEnabled = false;
		}
		else
		{
			grd_Pattern.IsEnabled = true;
			grd_Second.IsEnabled = true;
			txt_MatchScoreMin.IsEnabled = true;
			txt_MatchScoreMax.IsEnabled = true;
			grd_Third.IsEnabled = true;
			grd_AdvSetup.IsEnabled = true;
			btn_ReSetComboBox1.IsEnabled = true;
			btn_ReSetComboBox2.IsEnabled = true;
			btn_ReSetComboBox3.IsEnabled = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyValueToUI()
	{
		UpdateUI = false;
		DontSave = false;
		btn_Test.IsEnabled = true;
		rt_Result.Fill = Brushes.White;
		Label_ResultError.Visibility = Visibility.Hidden;
		grd_QuickSetup.Background = Brushes.LightBlue;
		int p1SelectOld = (cb_Pattern1.SelectedIndex = -1);
		P1SelectPublic = (P1SelectOld = p1SelectOld);
		p1SelectOld = (cb_Pattern2.SelectedIndex = -1);
		P2SelectPublic = (P2SelectOld = p1SelectOld);
		p1SelectOld = (cb_Pattern3.SelectedIndex = -1);
		P3SelectPublic = (P3SelectOld = p1SelectOld);
		p1SelectOld = (cb_Pattern4.SelectedIndex = -1);
		P4SelectPublic = (P4SelectOld = p1SelectOld);
		if (!PasteState)
		{
			BackupParam = inspection.paramFile.MatchParam[TrackIndex];
		}
		INSP_MATCHTOOL_PARAM iNSP_MATCHTOOL_PARAM;
		if (inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex >= 0)
		{
			if (inspection.paramFile.MatchQuickSetupParam[inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex].QuickSetupFinish)
			{
				iNSP_MATCHTOOL_PARAM = inspection.paramFile.MatchQuickSetupParam[inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex];
				cb_QuickSetup.SelectedIndex = inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex;
				txt_QuickSetupName.Text = iNSP_MATCHTOOL_PARAM.QuickSetupName;
				QuickSetupLock(enable: true);
				btn_QuickSetupRead.IsEnabled = false;
				grd_QuickSetup.Background = Brushes.LimeGreen;
			}
			else
			{
				iNSP_MATCHTOOL_PARAM = inspection.paramFile.MatchParam[TrackIndex];
				inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex = -1;
				cb_QuickSetup.SelectedIndex = -1;
				txt_QuickSetupName.Text = "";
				QuickSetupLock(enable: false);
				grd_QuickSetup.Background = Brushes.LightBlue;
			}
		}
		else
		{
			txt_QuickSetupName.Text = "";
			cb_QuickSetup.SelectedIndex = -1;
			iNSP_MATCHTOOL_PARAM = inspection.paramFile.MatchParam[TrackIndex];
			QuickSetupLock(enable: false);
		}
		if (iNSP_MATCHTOOL_PARAM.Locked && User < User_Permissions.USER_Administrator)
		{
			lab_LockMode.Visibility = Visibility.Visible;
		}
		else
		{
			lab_LockMode.Visibility = Visibility.Hidden;
		}
		tab_Function.SelectedIndex = 0;
		txt_AllResult.Clear();
		AlignMatchTextFound = true;
		if (inspection.paramFile.SelectWayON && inspection.paramFile.SelectWayParam.SelelctWayFunction == 1)
		{
			cb_AlignToMatchText.IsChecked = inspection.paramFile.MatchParam[TrackIndex].AlignToMatchText;
			if (!inspection.selectwayTool.PatternFound)
			{
				txt_AllResult.Clear();
				if (LanguageType == 0)
				{
					txt_AllResult.Text = string.Format("[{0}] - [{1}] SelectWay Tool Text Match Fail.\n", TrackIndex + 1, inspection.paramFile.MatchParam[TrackIndex].TrackerName);
				}
				else if (LanguageType == 1)
				{
					txt_AllResult.Text += string.Format("[{0}] - [{1}] 選向工具圖形比對失敗，無法搜尋參考位置，請重新設定選向工具。\n", TrackIndex + 1, inspection.paramFile.MatchParam[TrackIndex].TrackerName);
				}
				else
				{
					txt_AllResult.Text += string.Format("[{0}] - [{1}] 选向工具图形比对失败，无法搜寻参考位置，请重新设定选向工具。\n", TrackIndex + 1, inspection.paramFile.MatchParam[TrackIndex].TrackerName);
				}
				AlignMatchTextFound = false;
			}
		}
		else
		{
			cb_AlignToMatchText.IsChecked = false;
			cb_AlignToMatchText.IsEnabled = false;
		}
		if (PreviewMode)
		{
			btn_Test.Visibility = Visibility.Hidden;
			btn_TestNextImg.Visibility = Visibility.Hidden;
			btn_TestPrevImg.Visibility = Visibility.Hidden;
		}
		iNSP_MATCHTOOL_PARAM.TrackerON = inspection.paramFile.MatchParam[TrackIndex].TrackerON;
		ts_TrackerON.IsChecked = iNSP_MATCHTOOL_PARAM.TrackerON;
		lab_TrackIndex.Content = (TrackIndex + 1).ToString();
		Label_Index.Content = (TrackIndex + 1).ToString();
		if (iNSP_MATCHTOOL_PARAM.TrackerON)
		{
			gb_BasicSetup.Visibility = Visibility.Visible;
			exp_TrackerSetup.Visibility = Visibility.Visible;
		}
		else
		{
			gb_BasicSetup.Visibility = Visibility.Hidden;
			exp_TrackerSetup.Visibility = Visibility.Hidden;
		}
		if (iNSP_MATCHTOOL_PARAM.UseOCR)
		{
			rad_OCR.IsChecked = true;
			grd_MatchTool.IsEnabled = false;
		}
		else
		{
			rad_Match.IsChecked = true;
			grd_MatchTool.IsEnabled = true;
		}
		chk_UseFoundToMask.IsChecked = iNSP_MATCHTOOL_PARAM.UseFoundToMask;
		if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			rad_UsePublicDB.IsChecked = true;
			grd_QuickSetup.Visibility = Visibility.Visible;
			P1SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect;
			P2SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect2;
			P3SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect3;
			P4SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect4;
		}
		else
		{
			rad_UseOldDB.IsChecked = true;
			grd_QuickSetup.Visibility = Visibility.Hidden;
		}
		UpdatePatternDBComboBox(iNSP_MATCHTOOL_PARAM.UsePublicDB);
		cb_Channel_OCR.SelectedIndex = iNSP_MATCHTOOL_PARAM.OCRImageChannel;
		chk_OCRTextColorisWhite.IsChecked = iNSP_MATCHTOOL_PARAM.OCRTextisWhite;
		txt_OCRSetup.Text = iNSP_MATCHTOOL_PARAM.OCRText;
		if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			if (iNSP_MATCHTOOL_PARAM.PatternSelect >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect < 210)
			{
				if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern1.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect);
					P1SelectPublic = p1SelectOld;
					P1SelectOld = -1;
					img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect].ToBitmapSource();
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Height);
				}
				else
				{
					cb_Pattern1.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern1.SelectedIndex = -1;
			}
			if (iNSP_MATCHTOOL_PARAM.PatternSelect2 >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect2 < 210)
			{
				if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern2.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect2);
					P2SelectPublic = p1SelectOld;
					P2SelectOld = -1;
					img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].ToBitmapSource();
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Height);
				}
				else
				{
					cb_Pattern2.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern2.SelectedIndex = -1;
			}
			if (iNSP_MATCHTOOL_PARAM.PatternSelect3 >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect3 < 210)
			{
				if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern3.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect3);
					P3SelectPublic = p1SelectOld;
					P3SelectOld = -1;
					img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].ToBitmapSource();
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Height);
				}
				else
				{
					cb_Pattern3.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern3.SelectedIndex = -1;
			}
			if (iNSP_MATCHTOOL_PARAM.PatternSelect4 >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect4 < 210)
			{
				if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern4.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect4);
					P4SelectPublic = p1SelectOld;
					P4SelectOld = -1;
					img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].ToBitmapSource();
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Height);
				}
				else
				{
					cb_Pattern4.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern4.SelectedIndex = -1;
			}
		}
		else
		{
			if (iNSP_MATCHTOOL_PARAM.PatternSelect >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect < 20)
			{
				if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern1.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect);
					P1SelectOld = p1SelectOld;
					P1SelectPublic = -1;
					ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect]);
					img_PatternSet.Source = source;
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Height);
				}
				else
				{
					cb_Pattern1.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern1.SelectedIndex = -1;
			}
			if (iNSP_MATCHTOOL_PARAM.PatternSelect2 >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect2 < 20)
			{
				if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern2.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect2);
					P2SelectOld = p1SelectOld;
					P2SelectPublic = -1;
					ImageSource source2 = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect2]);
					img_PatternSet.Source = source2;
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Height);
				}
				else
				{
					cb_Pattern2.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern2.SelectedIndex = -1;
			}
			if (iNSP_MATCHTOOL_PARAM.PatternSelect3 >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect3 < 20)
			{
				if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern3.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect3);
					P3SelectOld = p1SelectOld;
					P3SelectPublic = -1;
					ImageSource source3 = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect3]);
					img_PatternSet.Source = source3;
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Height);
				}
				else
				{
					cb_Pattern3.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern3.SelectedIndex = -1;
			}
			if (iNSP_MATCHTOOL_PARAM.PatternSelect4 >= 0 && iNSP_MATCHTOOL_PARAM.PatternSelect4 < 20)
			{
				if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternImage.Empty())
				{
					p1SelectOld = (cb_Pattern4.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect4);
					P4SelectOld = p1SelectOld;
					P4SelectPublic = -1;
					ImageSource source4 = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect4]);
					img_PatternSet.Source = source4;
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Height);
				}
				else
				{
					cb_Pattern4.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern4.SelectedIndex = -1;
			}
		}
		if (cb_Pattern1.SelectedIndex == -1)
		{
			img_PatternSet.Source = null;
			lab_PatternSetInfo.Content = "";
		}
		img_PatternFound.Source = null;
		img_PatternFoundBinary.Source = null;
		lab_PatternFoundInfo.Content = "";
		ep_Result.Fill = Brushes.Gray;
		lab_MatchScore.Content = "0";
		cb_TrackerSelect.SelectedIndex = TrackIndex;
		txt_MatchScoreMin.Text = iNSP_MATCHTOOL_PARAM.MatchScoreMin.ToString();
		txt_MatchScoreMax.Text = iNSP_MATCHTOOL_PARAM.MatchScoreMax.ToString();
		txt_MinShapeScore.Text = iNSP_MATCHTOOL_PARAM.ShapeMatchScoreMin.ToString();
		cb_Track1AlignWayX.SelectedIndex = iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex;
		cb_Track1AlignWayY.SelectedIndex = iNSP_MATCHTOOL_PARAM.Tracker1AligmentYIndex;
		chk_RotateInspect.IsChecked = iNSP_MATCHTOOL_PARAM.EnableRotateInspect;
		if (iNSP_MATCHTOOL_PARAM.UseTextMatch)
		{
			radMatchText.IsChecked = true;
		}
		else
		{
			radMatchPattern.IsChecked = true;
		}
		chk_TextColorisWhite.IsChecked = iNSP_MATCHTOOL_PARAM.TextisWhite;
		chk_MatchShape.IsChecked = iNSP_MATCHTOOL_PARAM.EnableMatchShape;
		txt_TxtPosDiff.Text = iNSP_MATCHTOOL_PARAM.TextPosDiff.ToString();
		txt_TxtAreaDiff.Text = iNSP_MATCHTOOL_PARAM.TextAreaDiff.ToString();
		txt_TxtWidthDiff.Text = iNSP_MATCHTOOL_PARAM.TextWidthDiff.ToString();
		txt_TxtHeightDiff.Text = iNSP_MATCHTOOL_PARAM.TextHeightDiff.ToString();
		cb_DEStart.SelectedIndex = iNSP_MATCHTOOL_PARAM.DEStart;
		cb_DEStartCount.SelectedIndex = iNSP_MATCHTOOL_PARAM.DEStartCount;
		chk_MaskInFound.IsChecked = iNSP_MATCHTOOL_PARAM.MaskInFound;
		cb_FindCloseArea.SelectedIndex = iNSP_MATCHTOOL_PARAM.FindCloseArea;
		cb_FindTextArea.SelectedIndex = iNSP_MATCHTOOL_PARAM.FindTextArea;
		txt_CloseArea.Text = iNSP_MATCHTOOL_PARAM.ClosedArea.ToString();
		txt_MaxCloseArea.Text = iNSP_MATCHTOOL_PARAM.MaxClosedArea.ToString();
		txt_TextArea.Text = iNSP_MATCHTOOL_PARAM.TextArea.ToString();
		txt_MaxTextArea.Text = iNSP_MATCHTOOL_PARAM.MaxTextArea.ToString();
		txt_MinTextHeight.Text = iNSP_MATCHTOOL_PARAM.MinTextHeight.ToString();
		txt_MinTextWidth.Text = iNSP_MATCHTOOL_PARAM.MinTextWidth.ToString();
		txt_MaxTextHeight.Text = iNSP_MATCHTOOL_PARAM.MaxTextHeight.ToString();
		txt_MaxTextWidth.Text = iNSP_MATCHTOOL_PARAM.MaxTextWidth.ToString();
		txt_MinColorMatchScore.Text = iNSP_MATCHTOOL_PARAM.MinColorMatchScore.ToString();
		if (iNSP_MATCHTOOL_PARAM.EnableManualThd)
		{
			rad_EnableManualThd.IsChecked = true;
		}
		else
		{
			rad_EnableAutolThd.IsChecked = true;
		}
		txt_OnnxName.Text = "";
		chk_EnableAISimilarityCheck.IsChecked = iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck;
		txt_SimilarityScore.Text = iNSP_MATCHTOOL_PARAM.AISimilarityScore.ToString();
		string ResultStr = "";
		if (iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck && iNSP_MATCHTOOL_PARAM.OnnxName != "")
		{
			inspection.matchTool[TrackIndex].ReadModel(TrackIndex, ref ResultStr);
			txt_OnnxName.Text = iNSP_MATCHTOOL_PARAM.OnnxName;
		}
		else if (iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck && iNSP_MATCHTOOL_PARAM.OnnxName == "")
		{
			ResultStr = "尚未設定字元檢測AI模型。\n";
		}
		if (ResultStr != "")
		{
			MessageBox.Show(ResultStr);
			txt_AllResult.Text += ResultStr;
		}
		txt_BinaryThdMin.Text = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMin.ToString();
		txt_BinaryThdMax.Text = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMax.ToString();
		rs_BinaryThd.LowerValue = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMin;
		rs_BinaryThd.UpperValue = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMax;
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
		try
		{
			SetSelectFace(inspection.paramFile.MatchParam[TrackIndex].FaceIndex);
			cb_Track1AutoSize.SelectedIndex = inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex;
			txt_TrackerName.Text = inspection.paramFile.MatchParam[TrackIndex].TrackerName;
			cb_ErrorBin.SelectedIndex = (int)inspection.paramFile.MatchParam[TrackIndex].ErrorBin;
			cb_ParetoChart.SelectedIndex = inspection.paramFile.MatchParam[TrackIndex].ParetoIndex;
			if (cb_AlignToMatchText.IsChecked == true)
			{
				if (AlignMatchTextFound)
				{
					rect.X = inspection.selectwayTool.FoundTextRect.Left + inspection.paramFile.MatchParam[TrackIndex].Track1_L;
					rect.Y = inspection.selectwayTool.FoundTextRect.Top + inspection.paramFile.MatchParam[TrackIndex].Track1_T;
					rect.Width = inspection.selectwayTool.FoundTextRect.Right + inspection.paramFile.MatchParam[TrackIndex].Track1_R - rect.X;
					rect.Height = inspection.selectwayTool.FoundTextRect.Bottom + inspection.paramFile.MatchParam[TrackIndex].Track1_B - rect.Y;
					SetReferenceRect(rt_ReferenceRect1, inspection.selectwayTool.FoundTextRect);
				}
				else
				{
					rt_ReferenceRect1.Visibility = Visibility.Hidden;
				}
				Line line = line_X1;
				Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
				line.Visibility = visibility;
			}
			else if (inspection.paramFile.MatchParam[TrackIndex].Tracker1UseAutoSize)
			{
				if (inspection.paramFile.AdvLocateParam[inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex].TrackerON)
				{
					rad_Tracker1AutoSize.IsChecked = true;
					rect.X = inspection.paramFile.AdvLocateParam[inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex].L + inspection.paramFile.MatchParam[TrackIndex].Track1_L;
					rect.Y = inspection.paramFile.AdvLocateParam[inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex].T + inspection.paramFile.MatchParam[TrackIndex].Track1_T;
					rect.Width = inspection.paramFile.AdvLocateParam[inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex].Width - (inspection.paramFile.MatchParam[TrackIndex].Track1_L + inspection.paramFile.MatchParam[TrackIndex].Track1_R);
					rect.Height = inspection.paramFile.AdvLocateParam[inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex].Height - (inspection.paramFile.MatchParam[TrackIndex].Track1_T + inspection.paramFile.MatchParam[TrackIndex].Track1_B);
					SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex].InsideRect);
					Line line2 = line_X1;
					Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
					line2.Visibility = visibility;
				}
				else
				{
					DontSave = true;
					btn_Test.IsEnabled = false;
					MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
					SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
					txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
					rad_Tracker1AutoSize.IsChecked = true;
					rect.X = inspection.paramFile.AdvLocateParam[0].L + inspection.paramFile.MatchParam[TrackIndex].Track1_L;
					rect.Y = inspection.paramFile.AdvLocateParam[0].T + inspection.paramFile.MatchParam[TrackIndex].Track1_T;
					rect.Width = inspection.paramFile.AdvLocateParam[0].Width - (inspection.paramFile.MatchParam[TrackIndex].Track1_L + inspection.paramFile.MatchParam[TrackIndex].Track1_R);
					rect.Height = inspection.paramFile.AdvLocateParam[0].Height - (inspection.paramFile.MatchParam[TrackIndex].Track1_T + inspection.paramFile.MatchParam[TrackIndex].Track1_B);
					cb_Track1AutoSize.SelectedIndex = (inspection.paramFile.MatchParam[TrackIndex].AutoSize1_SelectIndex = 0);
					SetReferenceRect(rt_ReferenceRect1, inspection.paramFile.AdvLocateParam[0].InsideRect);
					Line line3 = line_X1;
					Visibility visibility = (line_Y1.Visibility = Visibility.Hidden);
					line3.Visibility = visibility;
				}
			}
			else if (inspection.paramFile.VSLocateParam[inspection.paramFile.MatchParam[TrackIndex].Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[inspection.paramFile.MatchParam[TrackIndex].Tracker1AligmentYIndex].TrackerON)
			{
				rad_Tracker1AlignSelect.IsChecked = true;
				int width = inspection.paramFile.MatchParam[TrackIndex].Track1_R - inspection.paramFile.MatchParam[TrackIndex].Track1_L;
				int height = inspection.paramFile.MatchParam[TrackIndex].Track1_B - inspection.paramFile.MatchParam[TrackIndex].Track1_T;
				rect.X = inspection.paramFile.VSLocateParam[inspection.paramFile.MatchParam[TrackIndex].Tracker1AligmentXIndex].Value + inspection.paramFile.MatchParam[TrackIndex].Track1_L;
				rect.Y = inspection.paramFile.HSLocateParam[inspection.paramFile.MatchParam[TrackIndex].Tracker1AligmentYIndex].Value + inspection.paramFile.MatchParam[TrackIndex].Track1_T;
				rect.Width = width;
				rect.Height = height;
				SetReferenceLine(line_X1, line_Y1, inspection.paramFile.MatchParam[TrackIndex].Tracker1AligmentXIndex, inspection.paramFile.MatchParam[TrackIndex].Tracker1AligmentYIndex);
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
			else
			{
				DontSave = true;
				btn_Test.IsEnabled = false;
				MessageBox.Show(LanguageManager.Instance["Adv.LocateLinkErrorA"], LanguageManager.Instance["Error"]);
				SetResult(2, LanguageManager.Instance["Adv.LocateLinkErrorA"]);
				txt_AllResult.Text += LanguageManager.Instance["Adv.LocateLinkErrorA"];
				rad_Tracker1AlignSelect.IsChecked = true;
				int width2 = inspection.paramFile.MatchParam[TrackIndex].Track1_R - inspection.paramFile.MatchParam[TrackIndex].Track1_L;
				int height2 = inspection.paramFile.MatchParam[TrackIndex].Track1_B - inspection.paramFile.MatchParam[TrackIndex].Track1_T;
				rect.X = inspection.paramFile.VSLocateParam[0].Value + inspection.paramFile.MatchParam[TrackIndex].Track1_L;
				rect.Y = inspection.paramFile.HSLocateParam[0].Value + inspection.paramFile.MatchParam[TrackIndex].Track1_T;
				rect.Width = width2;
				rect.Height = height2;
				cb_Track1AlignWayX.SelectedIndex = 0;
				cb_Track1AlignWayY.SelectedIndex = 0;
				SetReferenceLine(line_X1, line_Y1, 0, 0);
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex)
		{
			string text = "MTW06-MatchToolWindow-CopyVlaueToUI()-02";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
			return;
		}
		if (iNSP_MATCHTOOL_PARAM.TrackerON)
		{
			if (SetTracker(rect))
			{
				GetTracker(ref rect);
				LastSearchRect1 = rect;
				if (TrackerShow)
				{
					bor_SearchRect1.Visibility = Visibility.Visible;
				}
			}
		}
		else
		{
			bor_SearchRect1.Visibility = Visibility.Hidden;
			bor_PatternSet.Visibility = Visibility.Hidden;
		}
		UpdateUI = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyQuickSetupValueToUI()
	{
		UpdateUI = false;
		DontSave = false;
		btn_Test.IsEnabled = true;
		rt_Result.Fill = Brushes.White;
		Label_ResultError.Visibility = Visibility.Hidden;
		grd_QuickSetup.Background = Brushes.LightBlue;
		if (inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex < 0 || !inspection.paramFile.MatchQuickSetupParam[inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex].QuickSetupFinish)
		{
			return;
		}
		INSP_MATCHTOOL_PARAM iNSP_MATCHTOOL_PARAM = inspection.paramFile.MatchQuickSetupParam[inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex];
		cb_QuickSetup.SelectedIndex = inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex;
		txt_QuickSetupName.Text = iNSP_MATCHTOOL_PARAM.QuickSetupName;
		QuickSetupLock(enable: true);
		btn_QuickSetupRead.IsEnabled = false;
		grd_QuickSetup.Background = Brushes.LimeGreen;
		tab_Function.SelectedIndex = 0;
		txt_AllResult.Clear();
		if (PreviewMode)
		{
			btn_Test.Visibility = Visibility.Hidden;
			btn_TestNextImg.Visibility = Visibility.Hidden;
			btn_TestPrevImg.Visibility = Visibility.Hidden;
		}
		iNSP_MATCHTOOL_PARAM.TrackerON = true;
		if (iNSP_MATCHTOOL_PARAM.UseOCR)
		{
			rad_OCR.IsChecked = true;
			grd_MatchTool.IsEnabled = false;
		}
		else
		{
			rad_Match.IsChecked = true;
			grd_MatchTool.IsEnabled = true;
		}
		if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			rad_UsePublicDB.IsChecked = true;
			grd_QuickSetup.Visibility = Visibility.Visible;
		}
		else
		{
			rad_UseOldDB.IsChecked = true;
			grd_QuickSetup.Visibility = Visibility.Hidden;
		}
		P1SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect;
		P2SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect2;
		P3SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect3;
		P4SelectPublic = iNSP_MATCHTOOL_PARAM.PatternSelect4;
		UpdatePatternDBComboBox(iNSP_MATCHTOOL_PARAM.UsePublicDB);
		cb_Channel_OCR.SelectedIndex = iNSP_MATCHTOOL_PARAM.OCRImageChannel;
		chk_OCRTextColorisWhite.IsChecked = iNSP_MATCHTOOL_PARAM.OCRTextisWhite;
		txt_OCRSetup.Text = iNSP_MATCHTOOL_PARAM.OCRText;
		txt_QuickSetupName.Text = iNSP_MATCHTOOL_PARAM.QuickSetupName;
		if (iNSP_MATCHTOOL_PARAM.PatternSelect < 0 || iNSP_MATCHTOOL_PARAM.PatternSelect >= 210)
		{
			cb_Pattern1.SelectedIndex = -1;
		}
		else if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern1.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect);
				P1SelectPublic = p1SelectPublic;
				P1SelectOld = -1;
				img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect].ToBitmapSource();
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Height);
			}
			else
			{
				cb_Pattern1.SelectedIndex = -1;
			}
		}
		else if (iNSP_MATCHTOOL_PARAM.PatternSelect < 20)
		{
			if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern1.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect);
				P1SelectOld = p1SelectPublic;
				P1SelectPublic = -1;
				ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect]);
				img_PatternSet.Source = source;
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect].Height);
			}
			else
			{
				cb_Pattern1.SelectedIndex = -1;
			}
		}
		else
		{
			cb_Pattern1.SelectedIndex = -1;
		}
		if (iNSP_MATCHTOOL_PARAM.PatternSelect2 < 0 || iNSP_MATCHTOOL_PARAM.PatternSelect2 >= 210)
		{
			cb_Pattern2.SelectedIndex = -1;
		}
		else if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern2.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect2);
				P2SelectPublic = p1SelectPublic;
				P2SelectOld = -1;
				img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].ToBitmapSource();
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Height);
			}
			else
			{
				cb_Pattern2.SelectedIndex = -1;
			}
		}
		else if (iNSP_MATCHTOOL_PARAM.PatternSelect2 < 20)
		{
			if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern2.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect2);
				P2SelectOld = p1SelectPublic;
				P2SelectPublic = -1;
				ImageSource source2 = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect2]);
				img_PatternSet.Source = source2;
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect2].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect2].Height);
			}
			else
			{
				cb_Pattern2.SelectedIndex = -1;
			}
		}
		else
		{
			cb_Pattern2.SelectedIndex = -1;
		}
		if (iNSP_MATCHTOOL_PARAM.PatternSelect3 < 0 || iNSP_MATCHTOOL_PARAM.PatternSelect3 >= 210)
		{
			cb_Pattern3.SelectedIndex = -1;
		}
		else if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern3.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect3);
				P3SelectPublic = p1SelectPublic;
				P3SelectOld = -1;
				img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].ToBitmapSource();
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Height);
			}
			else
			{
				cb_Pattern3.SelectedIndex = -1;
			}
		}
		else if (iNSP_MATCHTOOL_PARAM.PatternSelect3 < 20)
		{
			if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern3.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect3);
				P3SelectOld = p1SelectPublic;
				P3SelectPublic = -1;
				ImageSource source3 = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect3]);
				img_PatternSet.Source = source3;
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect3].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect3].Height);
			}
			else
			{
				cb_Pattern3.SelectedIndex = -1;
			}
		}
		else
		{
			cb_Pattern3.SelectedIndex = -1;
		}
		if (iNSP_MATCHTOOL_PARAM.PatternSelect4 < 0 || iNSP_MATCHTOOL_PARAM.PatternSelect4 >= 210)
		{
			cb_Pattern4.SelectedIndex = -1;
		}
		else if (iNSP_MATCHTOOL_PARAM.UsePublicDB)
		{
			if (PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternON && PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternPath != null && !PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern4.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect4);
				P4SelectPublic = p1SelectPublic;
				P4SelectOld = -1;
				img_PatternSet.Source = PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].ToBitmapSource();
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternChannel], PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Width, PDBMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Height);
			}
			else
			{
				cb_Pattern4.SelectedIndex = -1;
			}
		}
		else if (iNSP_MATCHTOOL_PARAM.PatternSelect4 < 20)
		{
			if (inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternON && inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternPath != null && !inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternImage.Empty())
			{
				int p1SelectPublic = (cb_Pattern4.SelectedIndex = iNSP_MATCHTOOL_PARAM.PatternSelect4);
				P4SelectOld = p1SelectPublic;
				P4SelectPublic = -1;
				ImageSource source4 = inspection.buffers.MatToBitmapImage(PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect4]);
				img_PatternSet.Source = source4;
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[iNSP_MATCHTOOL_PARAM.PatternSelect4].PatternChannel], PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Width, PatternMat[iNSP_MATCHTOOL_PARAM.PatternSelect4].Height);
			}
			else
			{
				cb_Pattern4.SelectedIndex = -1;
			}
		}
		if (cb_Pattern1.SelectedIndex == -1)
		{
			img_PatternSet.Source = null;
			lab_PatternSetInfo.Content = "";
		}
		img_PatternFound.Source = null;
		img_PatternFoundBinary.Source = null;
		lab_PatternFoundInfo.Content = "";
		ep_Result.Fill = Brushes.Gray;
		lab_MatchScore.Content = "0";
		cb_TrackerSelect.SelectedIndex = TrackIndex;
		txt_MatchScoreMin.Text = iNSP_MATCHTOOL_PARAM.MatchScoreMin.ToString();
		txt_MatchScoreMax.Text = iNSP_MATCHTOOL_PARAM.MatchScoreMax.ToString();
		txt_MinShapeScore.Text = iNSP_MATCHTOOL_PARAM.ShapeMatchScoreMin.ToString();
		cb_Track1AlignWayX.SelectedIndex = iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex;
		cb_Track1AlignWayY.SelectedIndex = iNSP_MATCHTOOL_PARAM.Tracker1AligmentYIndex;
		chk_RotateInspect.IsChecked = iNSP_MATCHTOOL_PARAM.EnableRotateInspect;
		if (iNSP_MATCHTOOL_PARAM.UseTextMatch)
		{
			radMatchText.IsChecked = true;
		}
		else
		{
			radMatchPattern.IsChecked = true;
		}
		chk_TextColorisWhite.IsChecked = iNSP_MATCHTOOL_PARAM.TextisWhite;
		chk_MatchShape.IsChecked = iNSP_MATCHTOOL_PARAM.EnableMatchShape;
		txt_TxtPosDiff.Text = iNSP_MATCHTOOL_PARAM.TextPosDiff.ToString();
		txt_TxtAreaDiff.Text = iNSP_MATCHTOOL_PARAM.TextAreaDiff.ToString();
		txt_TxtWidthDiff.Text = iNSP_MATCHTOOL_PARAM.TextWidthDiff.ToString();
		txt_TxtHeightDiff.Text = iNSP_MATCHTOOL_PARAM.TextHeightDiff.ToString();
		cb_DEStart.SelectedIndex = iNSP_MATCHTOOL_PARAM.DEStart;
		cb_DEStartCount.SelectedIndex = iNSP_MATCHTOOL_PARAM.DEStartCount;
		chk_MaskInFound.IsChecked = iNSP_MATCHTOOL_PARAM.MaskInFound;
		cb_FindCloseArea.SelectedIndex = iNSP_MATCHTOOL_PARAM.FindCloseArea;
		cb_FindTextArea.SelectedIndex = iNSP_MATCHTOOL_PARAM.FindTextArea;
		txt_CloseArea.Text = iNSP_MATCHTOOL_PARAM.ClosedArea.ToString();
		txt_MaxCloseArea.Text = iNSP_MATCHTOOL_PARAM.MaxClosedArea.ToString();
		txt_TextArea.Text = iNSP_MATCHTOOL_PARAM.TextArea.ToString();
		txt_MaxTextArea.Text = iNSP_MATCHTOOL_PARAM.MaxTextArea.ToString();
		txt_MinTextHeight.Text = iNSP_MATCHTOOL_PARAM.MinTextHeight.ToString();
		txt_MinTextWidth.Text = iNSP_MATCHTOOL_PARAM.MinTextWidth.ToString();
		txt_MaxTextHeight.Text = iNSP_MATCHTOOL_PARAM.MaxTextHeight.ToString();
		txt_MaxTextWidth.Text = iNSP_MATCHTOOL_PARAM.MaxTextWidth.ToString();
		txt_MinColorMatchScore.Text = iNSP_MATCHTOOL_PARAM.MinColorMatchScore.ToString();
		txt_SimilarityScore.Text = iNSP_MATCHTOOL_PARAM.AISimilarityScore.ToString();
		chk_EnableAISimilarityCheck.IsChecked = iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck;
		txt_OnnxName.Text = iNSP_MATCHTOOL_PARAM.OnnxName;
		if (iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck && iNSP_MATCHTOOL_PARAM.OnnxName != "")
		{
			string ResultStr = "";
			inspection.matchTool[TrackIndex].ReadModel(TrackIndex, ref ResultStr);
		}
		if (iNSP_MATCHTOOL_PARAM.EnableManualThd)
		{
			rad_EnableManualThd.IsChecked = true;
		}
		else
		{
			rad_EnableAutolThd.IsChecked = true;
		}
		txt_BinaryThdMin.Text = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMin.ToString();
		txt_BinaryThdMax.Text = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMax.ToString();
		rs_BinaryThd.LowerValue = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMin;
		rs_BinaryThd.UpperValue = iNSP_MATCHTOOL_PARAM.TxtBinaryThdMax;
		UpdateUI = true;
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
	private void Img_PatternImg1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((Image)sender).Uid);
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.tif)|*.bmp;*.jpg;*.tif|All files (*.*)|*.*";
		if (openFileDialog.ShowDialog() != true)
		{
			return;
		}
		try
		{
			Mat mat = CVBuffer.ImRead(openFileDialog.FileName);
			string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
			if (fileName.Contains("-"))
			{
				int num2 = fileName.IndexOf("-");
				int num3 = Convert.ToInt32(fileName[num2 + 1].ToString());
				if (num3 >= 0 && num3 <= 9)
				{
					PatternMat[num] = mat.Clone();
					WritePattern(num, num3);
					ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num]);
					PatternImage[num].Source = source;
					PatternInfo[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num].PatternChannel], PatternMat[num].Width, PatternMat[num].Height);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-Img_PatternImg1_PreviewMouseRightButtonDown()-";
			text += ex.ToString();
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
		if ((User < User_Permissions.USER_Administrator && inspection.paramFile.MatchParam[TrackIndex].Locked) || PreviewMode)
		{
			return;
		}
		if (!AlignMatchTextFound)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("SelectWay Tool Text Match Fail, Can't be Save!", "Error");
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("錯誤，選向工具圖形比對失敗，自動儲存取消。(需重新設定選向工具或是取消參考選向圖形比對結果)", "錯誤");
			}
			else
			{
				MessageBox.Show("错误，选向工具图形比对失败，自动储存取消。(需重新设定选向工具或是取消参考选向图形比对结果)", "错误");
			}
			return;
		}
		try
		{
			if (DontSave)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Tracker value error, Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				else
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				return;
			}
			INSP_MATCHTOOL_PARAM iNSP_MATCHTOOL_PARAM = inspection.paramFile.MatchParam[TrackIndex];
			iNSP_MATCHTOOL_PARAM.ValueChangeFlag = true;
			iNSP_MATCHTOOL_PARAM.TrackerON = ts_TrackerON.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.TrackerName = txt_TrackerName.Text;
			iNSP_MATCHTOOL_PARAM.ErrorBin = (FLOW_BIN)cb_ErrorBin.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.ParetoIndex = cb_ParetoChart.SelectedIndex;
			bool flag = false;
			if (rad_Tracker1AutoSize.IsChecked.Value != iNSP_MATCHTOOL_PARAM.Tracker1UseAutoSize)
			{
				flag = true;
			}
			if (iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex != cb_Track1AlignWayX.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_MATCHTOOL_PARAM.Tracker1AligmentYIndex != cb_Track1AlignWayY.SelectedIndex)
			{
				flag = true;
			}
			if (iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex != cb_Track1AutoSize.SelectedIndex)
			{
				flag = true;
			}
			iNSP_MATCHTOOL_PARAM.Tracker1UseAutoSize = rad_Tracker1AutoSize.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.UseFoundToMask = chk_UseFoundToMask.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex = cb_Track1AlignWayX.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.Tracker1AligmentYIndex = cb_Track1AlignWayY.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex = cb_Track1AutoSize.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.AlignToMatchText = cb_AlignToMatchText.IsChecked.Value;
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			if (!GetTracker(ref rect))
			{
				return;
			}
			if ((LastSearchRect1 != rect) | flag)
			{
				if (cb_AlignToMatchText.IsChecked == true)
				{
					iNSP_MATCHTOOL_PARAM.Track1_L = rect.X - inspection.selectwayTool.FoundTextRect.Left;
					iNSP_MATCHTOOL_PARAM.Track1_T = rect.Y - inspection.selectwayTool.FoundTextRect.Top;
					iNSP_MATCHTOOL_PARAM.Track1_R = rect.X + rect.Width - inspection.selectwayTool.FoundTextRect.Right;
					iNSP_MATCHTOOL_PARAM.Track1_B = rect.Y + rect.Height - inspection.selectwayTool.FoundTextRect.Bottom;
				}
				else if (rad_Tracker1AutoSize.IsChecked.Value)
				{
					if (inspection.paramFile.AdvLocateParam[iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex].TrackerON)
					{
						iNSP_MATCHTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex].L;
						iNSP_MATCHTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex].T;
						iNSP_MATCHTOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex].Width - iNSP_MATCHTOOL_PARAM.Track1_L - rect.Width;
						iNSP_MATCHTOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex].Height - iNSP_MATCHTOOL_PARAM.Track1_T - rect.Height;
					}
					else
					{
						iNSP_MATCHTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.AdvLocateParam[0].L;
						iNSP_MATCHTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.AdvLocateParam[0].T;
						iNSP_MATCHTOOL_PARAM.Track1_R = inspection.paramFile.AdvLocateParam[0].Width - iNSP_MATCHTOOL_PARAM.Track1_L - rect.Width;
						iNSP_MATCHTOOL_PARAM.Track1_B = inspection.paramFile.AdvLocateParam[0].Height - iNSP_MATCHTOOL_PARAM.Track1_T - rect.Height;
						int autoSize1_SelectIndex = (cb_Track1AutoSize.SelectedIndex = 0);
						iNSP_MATCHTOOL_PARAM.AutoSize1_SelectIndex = autoSize1_SelectIndex;
					}
				}
				else if (inspection.paramFile.VSLocateParam[iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex].TrackerON && inspection.paramFile.HSLocateParam[iNSP_MATCHTOOL_PARAM.Tracker1AligmentYIndex].TrackerON)
				{
					iNSP_MATCHTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex].Value;
					iNSP_MATCHTOOL_PARAM.Track1_R = iNSP_MATCHTOOL_PARAM.Track1_L + rect.Width;
					iNSP_MATCHTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[iNSP_MATCHTOOL_PARAM.Tracker1AligmentYIndex].Value;
					iNSP_MATCHTOOL_PARAM.Track1_B = iNSP_MATCHTOOL_PARAM.Track1_T + rect.Height;
				}
				else
				{
					iNSP_MATCHTOOL_PARAM.Track1_L = rect.X - inspection.paramFile.VSLocateParam[0].Value;
					iNSP_MATCHTOOL_PARAM.Track1_R = iNSP_MATCHTOOL_PARAM.Track1_L + rect.Width;
					iNSP_MATCHTOOL_PARAM.Track1_T = rect.Y - inspection.paramFile.HSLocateParam[0].Value;
					iNSP_MATCHTOOL_PARAM.Track1_B = iNSP_MATCHTOOL_PARAM.Track1_T + rect.Height;
					iNSP_MATCHTOOL_PARAM.Tracker1UseAutoSize = false;
					int autoSize1_SelectIndex = (cb_Track1AlignWayX.SelectedIndex = 0);
					iNSP_MATCHTOOL_PARAM.Tracker1AligmentXIndex = autoSize1_SelectIndex;
				}
			}
			iNSP_MATCHTOOL_PARAM.EnableSelectFace = chk_SelectFace.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.FaceIndex = SelectFace;
			iNSP_MATCHTOOL_PARAM.UseOCR = false;
			iNSP_MATCHTOOL_PARAM.OCRImageChannel = cb_Channel_OCR.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.OCRTextisWhite = chk_OCRTextColorisWhite.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.OCRText = txt_OCRSetup.Text;
			iNSP_MATCHTOOL_PARAM.MatchScoreMin = Convert.ToInt32(txt_MatchScoreMin.Text);
			iNSP_MATCHTOOL_PARAM.MatchScoreMax = Convert.ToInt32(txt_MatchScoreMax.Text);
			iNSP_MATCHTOOL_PARAM.ShapeMatchScoreMin = Convert.ToInt32(txt_MinShapeScore.Text);
			iNSP_MATCHTOOL_PARAM.PatternSelect = cb_Pattern1.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.PatternSelect2 = cb_Pattern2.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.PatternSelect3 = cb_Pattern3.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.PatternSelect4 = cb_Pattern4.SelectedIndex;
			if (radMatchText.IsChecked == true)
			{
				iNSP_MATCHTOOL_PARAM.UseTextMatch = true;
			}
			else
			{
				iNSP_MATCHTOOL_PARAM.UseTextMatch = false;
			}
			iNSP_MATCHTOOL_PARAM.TextisWhite = chk_TextColorisWhite.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.EnableMatchShape = chk_MatchShape.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.TextPosDiff = Convert.ToInt32(txt_TxtPosDiff.Text);
			iNSP_MATCHTOOL_PARAM.TextAreaDiff = Convert.ToInt32(txt_TxtAreaDiff.Text);
			iNSP_MATCHTOOL_PARAM.TextWidthDiff = Convert.ToInt32(txt_TxtWidthDiff.Text);
			iNSP_MATCHTOOL_PARAM.TextHeightDiff = Convert.ToInt32(txt_TxtHeightDiff.Text);
			iNSP_MATCHTOOL_PARAM.MaskInFound = chk_MaskInFound.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.FindCloseArea = cb_FindCloseArea.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.FindTextArea = cb_FindTextArea.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.ClosedArea = Convert.ToDouble(txt_CloseArea.Text);
			iNSP_MATCHTOOL_PARAM.MaxClosedArea = Convert.ToDouble(txt_MaxCloseArea.Text);
			iNSP_MATCHTOOL_PARAM.TextArea = Convert.ToDouble(txt_TextArea.Text);
			iNSP_MATCHTOOL_PARAM.MaxTextArea = Convert.ToDouble(txt_MaxTextArea.Text);
			iNSP_MATCHTOOL_PARAM.MinTextWidth = Convert.ToDouble(txt_MinTextWidth.Text);
			iNSP_MATCHTOOL_PARAM.MinTextHeight = Convert.ToDouble(txt_MinTextHeight.Text);
			iNSP_MATCHTOOL_PARAM.MaxTextWidth = Convert.ToDouble(txt_MaxTextWidth.Text);
			iNSP_MATCHTOOL_PARAM.MaxTextHeight = Convert.ToDouble(txt_MaxTextHeight.Text);
			iNSP_MATCHTOOL_PARAM.MinColorMatchScore = Convert.ToDouble(txt_MinColorMatchScore.Text);
			iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck = chk_EnableAISimilarityCheck.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.AISimilarityScore = Convert.ToInt32(txt_SimilarityScore.Text);
			iNSP_MATCHTOOL_PARAM.OnnxName = txt_OnnxName.Text;
			iNSP_MATCHTOOL_PARAM.EnableManualThd = rad_EnableManualThd.IsChecked.Value;
			iNSP_MATCHTOOL_PARAM.TxtBinaryThdMin = (int)rs_BinaryThd.LowerValue;
			iNSP_MATCHTOOL_PARAM.TxtBinaryThdMax = (int)rs_BinaryThd.UpperValue;
			iNSP_MATCHTOOL_PARAM.DEStart = cb_DEStart.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.DEStartCount = cb_DEStartCount.SelectedIndex;
			iNSP_MATCHTOOL_PARAM.EnableRotateInspect = chk_RotateInspect.IsChecked.Value;
			if (rad_UseOldDB.IsChecked == true)
			{
				iNSP_MATCHTOOL_PARAM.UsePublicDB = false;
				iNSP_MATCHTOOL_PARAM.QuickSetupIndex = -1;
			}
			else
			{
				iNSP_MATCHTOOL_PARAM.UsePublicDB = true;
				iNSP_MATCHTOOL_PARAM.QuickSetupIndex = cb_QuickSetup.SelectedIndex;
			}
			inspection.paramFile.MatchParam[TrackIndex] = iNSP_MATCHTOOL_PARAM;
			UpdateUI = false;
			int selectedIndex = cb_TrackerSelect.SelectedIndex;
			cb_TrackerSelect.Items.Clear();
			for (int i = 0; i < 20; i++)
			{
				cb_TrackerSelect.Items.Add(i + 1 + " : " + inspection.paramFile.MatchParam[i].TrackerName);
			}
			cb_TrackerSelect.SelectedIndex = selectedIndex;
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-WriteUIValueToParam()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteQuickSetupParam()
	{
		if ((User < User_Permissions.USER_Administrator && inspection.paramFile.MatchParam[TrackIndex].Locked) || PreviewMode)
		{
			return;
		}
		try
		{
			if (DontSave)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Tracker value error, Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				else
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
			}
			else if (inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex >= 0)
			{
				INSP_MATCHTOOL_PARAM iNSP_MATCHTOOL_PARAM = inspection.paramFile.MatchQuickSetupParam[inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex];
				int selectedIndex = cb_QuickSetup.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.ValueChangeFlag = true;
				iNSP_MATCHTOOL_PARAM.QuickSetupName = txt_QuickSetupName.Text;
				iNSP_MATCHTOOL_PARAM.QuickSetupFinish = true;
				iNSP_MATCHTOOL_PARAM.QuickSetupIndex = selectedIndex;
				iNSP_MATCHTOOL_PARAM.UseOCR = false;
				iNSP_MATCHTOOL_PARAM.OCRImageChannel = cb_Channel_OCR.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.OCRTextisWhite = chk_OCRTextColorisWhite.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.OCRText = txt_OCRSetup.Text;
				iNSP_MATCHTOOL_PARAM.MatchScoreMin = Convert.ToInt32(txt_MatchScoreMin.Text);
				iNSP_MATCHTOOL_PARAM.MatchScoreMax = Convert.ToInt32(txt_MatchScoreMax.Text);
				iNSP_MATCHTOOL_PARAM.ShapeMatchScoreMin = Convert.ToInt32(txt_MinShapeScore.Text);
				iNSP_MATCHTOOL_PARAM.PatternSelect = cb_Pattern1.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.PatternSelect2 = cb_Pattern2.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.PatternSelect3 = cb_Pattern3.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.PatternSelect4 = cb_Pattern4.SelectedIndex;
				if (radMatchText.IsChecked == true)
				{
					iNSP_MATCHTOOL_PARAM.UseTextMatch = true;
				}
				else
				{
					iNSP_MATCHTOOL_PARAM.UseTextMatch = false;
				}
				iNSP_MATCHTOOL_PARAM.TextisWhite = chk_TextColorisWhite.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.EnableMatchShape = chk_MatchShape.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.TextPosDiff = Convert.ToInt32(txt_TxtPosDiff.Text);
				iNSP_MATCHTOOL_PARAM.TextAreaDiff = Convert.ToInt32(txt_TxtAreaDiff.Text);
				iNSP_MATCHTOOL_PARAM.TextWidthDiff = Convert.ToInt32(txt_TxtWidthDiff.Text);
				iNSP_MATCHTOOL_PARAM.TextHeightDiff = Convert.ToInt32(txt_TxtHeightDiff.Text);
				iNSP_MATCHTOOL_PARAM.MaskInFound = chk_MaskInFound.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.FindCloseArea = cb_FindCloseArea.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.FindTextArea = cb_FindTextArea.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.ClosedArea = Convert.ToDouble(txt_CloseArea.Text);
				iNSP_MATCHTOOL_PARAM.MaxClosedArea = Convert.ToDouble(txt_MaxCloseArea.Text);
				iNSP_MATCHTOOL_PARAM.TextArea = Convert.ToDouble(txt_TextArea.Text);
				iNSP_MATCHTOOL_PARAM.MaxTextArea = Convert.ToDouble(txt_MaxTextArea.Text);
				iNSP_MATCHTOOL_PARAM.MinTextWidth = Convert.ToDouble(txt_MinTextWidth.Text);
				iNSP_MATCHTOOL_PARAM.MinTextHeight = Convert.ToDouble(txt_MinTextHeight.Text);
				iNSP_MATCHTOOL_PARAM.MaxTextWidth = Convert.ToDouble(txt_MaxTextWidth.Text);
				iNSP_MATCHTOOL_PARAM.MaxTextHeight = Convert.ToDouble(txt_MaxTextHeight.Text);
				iNSP_MATCHTOOL_PARAM.MinColorMatchScore = Convert.ToDouble(txt_MinColorMatchScore.Text);
				iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck = chk_EnableAISimilarityCheck.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.AISimilarityScore = Convert.ToInt32(txt_SimilarityScore.Text);
				iNSP_MATCHTOOL_PARAM.OnnxName = txt_OnnxName.Text;
				iNSP_MATCHTOOL_PARAM.EnableManualThd = rad_EnableManualThd.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.TxtBinaryThdMin = (int)rs_BinaryThd.LowerValue;
				iNSP_MATCHTOOL_PARAM.TxtBinaryThdMax = (int)rs_BinaryThd.UpperValue;
				iNSP_MATCHTOOL_PARAM.DEStart = cb_DEStart.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.DEStartCount = cb_DEStartCount.SelectedIndex;
				iNSP_MATCHTOOL_PARAM.EnableRotateInspect = chk_RotateInspect.IsChecked.Value;
				iNSP_MATCHTOOL_PARAM.UsePublicDB = true;
				iNSP_MATCHTOOL_PARAM.QuickSetupIndex = cb_QuickSetup.SelectedIndex;
				inspection.paramFile.MatchQuickSetupParam[inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex] = iNSP_MATCHTOOL_PARAM;
				UpdateUI = false;
				UpdateQuickSetupList(selectedIndex);
				UpdateUI = true;
			}
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-WriteUIValueToParam()";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WritePattern(int index, int channel)
	{
		try
		{
			if (DontSave)
			{
				if (LanguageType == 0)
				{
					MessageBox.Show("Tracker value error, Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				else
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				return;
			}
			string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + inspection.RecipeName + "\\Pattern\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}S{1}P{2}-{3}.bmp", text, StationIndex + 1, index + 1, channel);
			MatchPattern matchPattern = inspection.paramFile.patternDB[index];
			matchPattern.ValueChangeFlag = true;
			if (!PatternMat[index].Empty())
			{
				Cv2.ImWrite(text2, PatternMat[index]);
				matchPattern.PatternON = true;
				matchPattern.PatternName = PatternName[index].Text;
				matchPattern.PatternChannel = PatternChannelSelect;
				matchPattern.PatternPath = text2;
				matchPattern.PatternImage = PatternMat[index];
			}
		}
		catch (Exception ex)
		{
			string text3 = "MTW07-MatchToolWindow-WritePattern()";
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text3 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
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
					MessageBox.Show("Tracker value error, Can't be Save!", "Error");
				}
				else if (LanguageType == 1)
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				else
				{
					MessageBox.Show("錯誤，檢測區域錯誤，自動儲存取消。", "錯誤");
				}
				return;
			}
			string text = AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}P{1}-{2}.bmp", text, index + 1, channel);
			MatchPattern matchPattern = PublicPatternDB[index];
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
			PublicPatternDB[index] = matchPattern;
		}
		catch (Exception ex)
		{
			string text3 = "MTW07-MatchToolWindow-WritePublicPattern()";
			CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			text3 += ex.ToString();
			if (ShowMessage)
			{
				MessageBox.Show(text3, "錯誤");
			}
		}
	}

	private void Rad_Match_Click(object sender, RoutedEventArgs e)
	{
		if (rad_Match.IsChecked == false)
		{
			rad_OCR.IsChecked = true;
			grd_MatchTool.IsEnabled = false;
		}
		else
		{
			rad_Match.IsChecked = true;
			grd_MatchTool.IsEnabled = true;
		}
	}

	private void Img_PatternFoundBinary_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Btn_AlignShow_Click(object sender, RoutedEventArgs e)
	{
		if (AlignShow)
		{
			grd_Align.Visibility = Visibility.Hidden;
			AlignShow = false;
		}
		else
		{
			grd_Align.Visibility = Visibility.Visible;
			AlignShow = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ShowPattern1_Click(object sender, RoutedEventArgs e)
	{
		if (!UpdateUI || !inited)
		{
			return;
		}
		Button button = (Button)sender;
		int num = 0;
		try
		{
			string uid = button.Uid;
			num = ((uid == "0") ? cb_Pattern1.SelectedIndex : ((uid == "1") ? cb_Pattern2.SelectedIndex : ((uid == "2") ? cb_Pattern3.SelectedIndex : ((!(uid == "3")) ? cb_Pattern1.SelectedIndex : cb_Pattern4.SelectedIndex))));
			if (num < 0 || num >= 210)
			{
				return;
			}
			if (rad_UsePublicDB.IsChecked == true)
			{
				if (PublicPatternDB[num].PatternON && !PDBMat[num].Empty())
				{
					img_PatternSet.Source = PDBMat[num].ToBitmapSource();
					lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[num].PatternChannel], PDBMat[num].Width, PDBMat[num].Height);
				}
			}
			else if (inspection.paramFile.patternDB[num].PatternON && !PatternMat[num].Empty())
			{
				ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num]);
				img_PatternSet.Source = source;
				lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num].PatternChannel], PatternMat[num].Width, PatternMat[num].Height);
			}
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-Btn_ShowPattern1_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Img_PatternImg1_Drop(object sender, DragEventArgs e)
	{
		string[] array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
		if (array.Length < 1)
		{
			return;
		}
		string path = array[0];
		int num = Convert.ToInt32(((Image)sender).Uid);
		if (System.IO.Path.GetExtension(path).ToLower() != ".bmp")
		{
			return;
		}
		try
		{
			Mat mat = CVBuffer.ImRead(path);
			string fileName = System.IO.Path.GetFileName(path);
			if (fileName.Contains("-"))
			{
				int num2 = fileName.IndexOf("-");
				int num3 = Convert.ToInt32(fileName[num2 + 1].ToString());
				if (num3 >= 0 && num3 <= 9)
				{
					PatternMat[num] = mat.Clone();
					WritePattern(num, num3);
					ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num]);
					PatternImage[num].Source = source;
					PatternInfo[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num].PatternChannel], PatternMat[num].Width, PatternMat[num].Height);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-Img_PatternImg1_Drop()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetPBDPattern_Click(object sender, RoutedEventArgs e)
	{
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
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
			if (PatternChannelSelect == 10)
			{
				PDBMat[num2] = new Mat();
				PDBImaage[num].Source = null;
				PDBLabel[num].Content = "";
				PublicPatternDB[num2].PatternON = false;
				PublicPatternDB[num2].ValueChangeFlag = true;
				MatchPattern obj = PublicPatternDB[num2];
				obj.ValueChangeFlag = true;
				obj.PatternON = false;
				obj.PatternName = "";
				obj.PatternChannel = 0;
				obj.PatternPath = "";
				obj.PatternImage = PDBMat[num2];
			}
			else if (GetPatternTracker(ref rect))
			{
				PDBMat[num2] = new Mat(inspection.buffers.GetChannelMat(PatternChannelSelect), rect).Clone();
				WritePublicPattern(num2, PatternChannelSelect);
				PDBImaage[num].Source = PDBMat[num2].ToBitmapSource();
				PDBLabel[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[PatternChannelSelect], PDBMat[num2].Width, PDBMat[num2].Height);
			}
		}
		catch (Exception ex)
		{
			string text = "MTW08-MatchToolWindow-Btn_SetPBDPattern_Click()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Img_pbd_Drop(object sender, DragEventArgs e)
	{
		string[] array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
		if (array.Length < 1)
		{
			return;
		}
		string path = array[0];
		int num = Convert.ToInt32(((Image)sender).Uid);
		int num2 = CurrentPublicDBPage * 70 + num;
		if (System.IO.Path.GetExtension(path).ToLower() != ".bmp")
		{
			return;
		}
		try
		{
			Mat mat = CVBuffer.ImRead(path);
			string fileName = System.IO.Path.GetFileName(path);
			if (!fileName.Contains("-"))
			{
				return;
			}
			int num3 = fileName.IndexOf("-");
			int num4 = Convert.ToInt32(fileName[num3 + 1].ToString());
			if (num4 < 0 || num4 > 9)
			{
				return;
			}
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
			PDBMat[num2] = mat.Clone();
			WritePublicPattern(num2, num4);
			PDBImaage[num].Source = mat.ToBitmapSource();
			PDBLabel[num].Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num2].PatternChannel], PDBMat[num2].Width, PDBMat[num2].Height);
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-Img_PatternImg1_Drop()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	private void Img_pbd_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Img_pbd_PreviewMouseMove(object sender, MouseEventArgs e)
	{
	}

	private void Rad_UseOldDB_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void Rad_UsePublicDB_Click(object sender, RoutedEventArgs e)
	{
		if (rad_UsePublicDB.IsChecked == true)
		{
			P1SelectOld = cb_Pattern1.SelectedIndex;
			P2SelectOld = cb_Pattern2.SelectedIndex;
			P3SelectOld = cb_Pattern3.SelectedIndex;
			P4SelectOld = cb_Pattern4.SelectedIndex;
			UpdatePatternDBComboBox(UsePublicDB: true);
			cb_QuickSetup.SelectedIndex = -1;
			grd_QuickSetup.Visibility = Visibility.Visible;
		}
		else
		{
			P1SelectPublic = cb_Pattern1.SelectedIndex;
			P2SelectPublic = cb_Pattern2.SelectedIndex;
			P3SelectPublic = cb_Pattern3.SelectedIndex;
			P4SelectPublic = cb_Pattern4.SelectedIndex;
			UpdatePatternDBComboBox(UsePublicDB: false);
			grd_QuickSetup.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ReSetComboBox_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Clear Selected Match Image?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("清除所选择的比对图形，是否继续！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("清除所選擇的比對圖形，是否繼續?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		string uid = ((Button)sender).Uid;
		if (!(uid == "1"))
		{
			if (!(uid == "2"))
			{
				if (uid == "3")
				{
					cb_Pattern4.SelectedIndex = -1;
				}
			}
			else
			{
				cb_Pattern3.SelectedIndex = -1;
			}
		}
		else
		{
			cb_Pattern2.SelectedIndex = -1;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_QuickSetupRead_Click(object sender, RoutedEventArgs e)
	{
		if (cb_QuickSetup.SelectedIndex >= 0)
		{
			int selectedIndex = cb_QuickSetup.SelectedIndex;
			if (!inspection.paramFile.MatchQuickSetupParam[selectedIndex].QuickSetupFinish)
			{
				MessageBox.Show("尚未設定快速設定數值", "錯誤");
				return;
			}
			QuickSetupWriteState = false;
			inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex = cb_QuickSetup.SelectedIndex;
			inspection.paramFile.MatchParam[TrackIndex].ValueChangeFlag = true;
			CopyQuickSetupValueToUI();
			WriteUIValueToParam();
			btn_QuickSetupWrite.IsEnabled = false;
			grd_QuickSetup.Background = Brushes.LimeGreen;
		}
	}

	private void Cb_QuickSetup_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited && UpdateUI && cb_QuickSetup.SelectedIndex >= 0)
		{
			if (inspection.paramFile.MatchQuickSetupParam[cb_QuickSetup.SelectedIndex].QuickSetupFinish)
			{
				btn_QuickSetupRead.IsEnabled = true;
				btn_QuickSetupWrite.IsEnabled = false;
			}
			else
			{
				btn_QuickSetupRead.IsEnabled = false;
				btn_QuickSetupWrite.IsEnabled = true;
			}
		}
	}

	private void Btn_QuickSetupLock_Click(object sender, RoutedEventArgs e)
	{
		if (cb_QuickSetup.SelectedIndex >= 0)
		{
			txt_QuickSetupName.IsReadOnly = false;
			btn_QuickSetupWrite.IsEnabled = true;
			QuickSetupLock(enable: false);
			QuickSetupWriteState = true;
			grd_QuickSetup.Background = Brushes.Yellow;
		}
		else
		{
			btn_QuickSetupWrite.IsEnabled = false;
			txt_QuickSetupName.IsReadOnly = true;
			grd_QuickSetup.Background = Brushes.LightBlue;
		}
	}

	private void Btn_QuickSetupWrite_Click(object sender, RoutedEventArgs e)
	{
		if (cb_QuickSetup.SelectedIndex >= 0)
		{
			inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex = cb_QuickSetup.SelectedIndex;
			inspection.paramFile.MatchParam[TrackIndex].ValueChangeFlag = true;
			WriteQuickSetupParam();
			WriteUIValueToParam();
			QuickSetupLock(enable: true);
			btn_QuickSetupWrite.IsEnabled = false;
			btn_QuickSetupRead.IsEnabled = false;
			QuickSetupWriteState = false;
			CopyValueToUI();
		}
	}

	private void Btn_QuickSetupCancel_Click(object sender, RoutedEventArgs e)
	{
		cb_QuickSetup.SelectedIndex = -1;
		inspection.paramFile.MatchParam[TrackIndex].QuickSetupIndex = -1;
		inspection.paramFile.MatchParam[TrackIndex].ValueChangeFlag = true;
		WriteUIValueToParam();
		CopyValueToUI();
	}

	private void Btn_QuickSetupFind_Click(object sender, RoutedEventArgs e)
	{
		int num = -1;
		string text = txt_TrackerName.Text.ToLower();
		for (int i = 0; i < 50; i++)
		{
			if (inspection.paramFile.MatchQuickSetupParam[i].QuickSetupFinish)
			{
				inspection.paramFile.MatchQuickSetupParam[i].QuickSetupName.ToLower();
				if (text.Contains(text))
				{
					num = i;
					break;
				}
			}
		}
		if (num != -1)
		{
			cb_QuickSetup.SelectedIndex = num;
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

	private void UpdateTxtBinary()
	{
		txt_BinaryThdMin.Text = ((int)rs_BinaryThd.LowerValue).ToString();
		txt_BinaryThdMax.Text = ((int)rs_BinaryThd.UpperValue).ToString();
		if (inspection.matchTool[TrackIndex].FoundMat.Width > 0 && inspection.matchTool[TrackIndex].FoundMat.Height > 0 && rad_EnableManualThd.IsChecked == true)
		{
			Mat mat = new Mat();
			Cv2.InRange(inspection.matchTool[TrackIndex].FoundMat, rs_BinaryThd.LowerValue, rs_BinaryThd.UpperValue, mat);
			if (chk_TextColorisWhite.IsChecked == false)
			{
				Cv2.BitwiseNot(mat, mat);
			}
			img_PatternFoundBinary.Source = inspection.buffers.MatToBitmapImage(mat);
		}
	}

	private void Rs_BinaryThd_CentralThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (UpdateUI && inited)
		{
			UpdateTxtBinary();
		}
	}

	private void Rs_BinaryThd_LowerThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (UpdateUI && inited)
		{
			UpdateTxtBinary();
		}
	}

	private void Rs_BinaryThd_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (UpdateUI && inited)
		{
			UpdateTxtBinary();
		}
	}

	private void Rs_BinaryThd_UpperThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (UpdateUI && inited)
		{
			UpdateTxtBinary();
		}
	}

	private void Rs_BinaryThd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (UpdateUI && inited)
		{
			UpdateTxtBinary();
		}
	}

	private void Btn_SetOnnxFile_Click(object sender, RoutedEventArgs e)
	{
		if (chk_EnableAISimilarityCheck.IsChecked == false)
		{
			return;
		}
		_ = (Button)sender;
		OnnxModelsWindow onnxModelsWindow = new OnnxModelsWindow();
		onnxModelsWindow.SetCurrentModelName(txt_OnnxName.Text);
		onnxModelsWindow.SetInspection(inspection);
		onnxModelsWindow.LoadLanguage(LanguageType);
		onnxModelsWindow.ShowDialog();
		txt_OnnxName.Text = onnxModelsWindow.GetSelectModelName();
		if (cb_QuickSetup.SelectedIndex >= 0)
		{
			WriteQuickSetupParam();
		}
		WriteUIValueToParam();
		string ResultStr = "";
		string text = "";
		for (int i = 0; i < 20; i++)
		{
			if (inspection.paramFile.MatchParam[i].EnableAISimilarityCheck)
			{
				inspection.matchTool[i].ReadModel(i, ref ResultStr);
				text += ResultStr;
			}
		}
		if (text != "")
		{
			MessageBox.Show(ResultStr);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Img_PatternFound_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (inspection.matchTool[TrackIndex].FoundMat.Width <= 0 || inspection.matchTool[TrackIndex].FoundMat.Height <= 0)
		{
			return;
		}
		_ = (Image)sender;
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Image files (*.bmp;)|*.bmp;|All files (*.*)|*.*";
		try
		{
			if (saveFileDialog.ShowDialog() != true)
			{
				return;
			}
			if (!Directory.Exists(System.IO.Path.GetDirectoryName(saveFileDialog.FileName)))
			{
				if (ShowMessage)
				{
					MessageBox.Show(LanguageManager.Instance["Error"], LanguageManager.Instance["Error"]);
				}
			}
			else
			{
				inspection.matchTool[TrackIndex].FoundMat.SaveImage(saveFileDialog.FileName);
			}
		}
		catch (Exception ex)
		{
			string text = "MTW07-MatchToolWindow-Img_PatternFound_PreviewMouseRightButtonDown()-";
			text += ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			if (ShowMessage)
			{
				MessageBox.Show(text, "錯誤");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Txt_PatternName21_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (inspection.matchTool[TrackIndex].SearchMat.Width > 0 && inspection.matchTool[TrackIndex].SearchMat.Height > 0)
		{
			Cv2.ImShow("尋找的影像區域", inspection.matchTool[TrackIndex].SearchMat);
		}
	}

	private void Cb_AlignToMatchText_Checked(object sender, RoutedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			if (inspection.selectwayTool.PatternFound)
			{
				rt_ReferenceRect1.Visibility = Visibility.Visible;
				SetReferenceRect(rt_ReferenceRect1, inspection.selectwayTool.FoundTextRect);
				AlignMatchTextFound = true;
			}
			else
			{
				AlignMatchTextFound = false;
				rt_ReferenceRect1.Visibility = Visibility.Hidden;
			}
		}
	}

	private void Cb_AlignToMatchText_Unchecked(object sender, RoutedEventArgs e)
	{
		if (inited && UpdateUI)
		{
			AlignMatchTextFound = true;
			rt_ReferenceRect1.Visibility = Visibility.Hidden;
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
			if (rect.Width <= 0 || rect.Height <= 0)
			{
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
				return;
			}
			WriteUIValueToParam();
			if (chk_SelectFace.IsChecked != true || FaceIndex == 0 || FaceIndex == SelectFace)
			{
				Label_ResultError.Visibility = Visibility.Hidden;
				try
				{
					if (rad_Match.IsChecked == true)
					{
						if (rad_UsePublicDB.IsChecked == true)
						{
							if (cb_Pattern1.SelectedIndex < 0 || cb_Pattern1.SelectedIndex >= 210)
							{
								txt_AllResult.Text = LanguageManager.Instance["SelectMatchImage"];
								SetResult(result: false);
								return;
							}
						}
						else if (cb_Pattern1.SelectedIndex < 0 || cb_Pattern1.SelectedIndex >= 20)
						{
							txt_AllResult.Text = LanguageManager.Instance["SelectMatchImage"];
							SetResult(result: false);
							return;
						}
						TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
						TimeStart();
						if (QuickSetupWriteState)
						{
							inspection.matchTool[TrackIndex].QuickSetupUpdateMode = true;
						}
						bool result = inspection.matchTool[TrackIndex].Inspect(TrackIndex, ref iresult);
						inspection.matchTool[TrackIndex].QuickSetupUpdateMode = false;
						double num = TimeStop();
						lab_Time.Content = string.Format("{0:N3}", num);
						label_Pattern1.Background = Brushes.Orange;
						label_Pattern2.Background = Brushes.Orange;
						label_Pattern3.Background = Brushes.Orange;
						label_Pattern4.Background = Brushes.Orange;
						if (chk_EnableAISimilarityCheck.IsChecked == false)
						{
							lab_Score1.Content = inspection.matchTool[TrackIndex].MatchScore1.ToString();
							lab_Score2.Content = inspection.matchTool[TrackIndex].MatchScore2.ToString();
							lab_Score3.Content = inspection.matchTool[TrackIndex].MatchScore3.ToString();
							lab_Score4.Content = inspection.matchTool[TrackIndex].MatchScore4.ToString();
						}
						else
						{
							lab_Score1.Content = inspection.matchTool[TrackIndex].AI_Score[0];
							lab_Score2.Content = inspection.matchTool[TrackIndex].AI_Score[1];
							lab_Score3.Content = inspection.matchTool[TrackIndex].AI_Score[2];
							lab_Score4.Content = inspection.matchTool[TrackIndex].AI_Score[3];
						}
						int num2 = 0;
						if (inspection.matchTool[TrackIndex].Mat4MatchFound)
						{
							label_Pattern4.Background = Brushes.Lime;
							num2 = cb_Pattern4.SelectedIndex;
						}
						else if (inspection.matchTool[TrackIndex].Mat3MatchFound)
						{
							label_Pattern3.Background = Brushes.Lime;
							num2 = cb_Pattern3.SelectedIndex;
						}
						else if (inspection.matchTool[TrackIndex].Mat2MatchFound)
						{
							label_Pattern2.Background = Brushes.Lime;
							num2 = cb_Pattern2.SelectedIndex;
						}
						else
						{
							label_Pattern1.Background = Brushes.Lime;
							num2 = cb_Pattern1.SelectedIndex;
						}
						if (num2 >= 0)
						{
							if (rad_UsePublicDB.IsChecked == true)
							{
								if (PublicPatternDB[num2].PatternON && !PDBMat[num2].Empty())
								{
									img_PatternSet.Source = PDBMat[num2].ToBitmapSource();
									lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[PublicPatternDB[num2].PatternChannel], PDBMat[num2].Width, PDBMat[num2].Height);
								}
							}
							else if (inspection.paramFile.patternDB[num2].PatternON && !PatternMat[num2].Empty())
							{
								ImageSource source = inspection.buffers.MatToBitmapImage(PatternMat[num2]);
								img_PatternSet.Source = source;
								lab_PatternSetInfo.Content = string.Format("{0}, {1} x {2}", ChannelStr[inspection.paramFile.patternDB[num2].PatternChannel], PatternMat[num2].Width, PatternMat[num2].Height);
							}
						}
						if (inspection.matchTool[TrackIndex].PatternFound)
						{
							img_PatternFound.Source = inspection.buffers.MatToBitmapImage(inspection.matchTool[TrackIndex].FoundMat);
							lab_MatchScore.Content = string.Format("{0}", inspection.matchTool[TrackIndex].MatchScore);
							if (inspection.matchTool[TrackIndex].FoundMatBinary.Width > 0 && inspection.matchTool[TrackIndex].FoundMatBinary.Height > 0)
							{
								img_PatternFoundBinary.Source = inspection.buffers.MatToBitmapImage(inspection.matchTool[TrackIndex].FoundMatBinary);
							}
						}
						else
						{
							img_PatternFound.Source = null;
							img_PatternFoundBinary.Source = null;
							lab_MatchScore.Content = string.Format("{0}", 0);
						}
						txt_DiffRatio.Text = ((int)inspection.matchTool[TrackIndex].DifferenceRatio).ToString();
						txt_AreaRatio.Text = ((int)inspection.matchTool[TrackIndex].AreaRatio).ToString();
						txt_WidthRatio.Text = ((int)inspection.matchTool[TrackIndex].WidthRatio).ToString();
						txt_HeightRatio.Text = ((int)inspection.matchTool[TrackIndex].HeightRatio).ToString();
						if (inspection.matchTool[TrackIndex].DifferenceRatioResult)
						{
							txt_TxtPosDiff.Background = Brushes.White;
						}
						else
						{
							txt_TxtPosDiff.Background = Brushes.Fuchsia;
						}
						if (inspection.matchTool[TrackIndex].AreaRatioResult)
						{
							txt_TxtAreaDiff.Background = Brushes.White;
						}
						else
						{
							txt_TxtAreaDiff.Background = Brushes.Fuchsia;
						}
						if (inspection.matchTool[TrackIndex].WidthRatioResult)
						{
							txt_TxtWidthDiff.Background = Brushes.White;
						}
						else
						{
							txt_TxtWidthDiff.Background = Brushes.Fuchsia;
						}
						if (inspection.matchTool[TrackIndex].HeightRatioResult)
						{
							txt_TxtHeightDiff.Background = Brushes.White;
						}
						else
						{
							txt_TxtHeightDiff.Background = Brushes.Fuchsia;
						}
						txt_CloseArea.Background = Brushes.White;
						txt_MaxCloseArea.Background = Brushes.White;
						if (inspection.matchTool[TrackIndex].MatchScore > Convert.ToInt32(txt_MatchScoreMax.Text))
						{
							txt_MatchScoreMax.Background = Brushes.Fuchsia;
						}
						else
						{
							txt_MatchScoreMax.Background = Brushes.White;
						}
						if (inspection.matchTool[TrackIndex].MatchScore < Convert.ToInt32(txt_MatchScoreMin.Text))
						{
							txt_MatchScoreMin.Background = Brushes.Fuchsia;
						}
						else
						{
							txt_MatchScoreMin.Background = Brushes.White;
						}
						SetResult(result);
						if (inspection.buffers.LocatedSrc.Channels() == 3)
						{
							ShowBitmap = inspection.buffers.LocatedSrc.Clone();
						}
						else
						{
							Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
						}
						txt_AllResult.Clear();
						txt_AllResult.Text = inspection.matchTool[TrackIndex].Tracker1ResultText;
						if (inspection.matchTool[TrackIndex].FoundRect.Width > 0 && inspection.matchTool[TrackIndex].FoundRect.Height > 0)
						{
							ShowBitmap.Rectangle(inspection.matchTool[TrackIndex].FoundRect, Scalar.GreenYellow);
						}
						if (chk_EnableAISimilarityCheck.IsChecked == true)
						{
							lab_PatternFoundInfo.Content = string.Format("{0:N5}", inspection.matchTool[TrackIndex].PredResult);
							txt_SimilarityScoreResult.Text = inspection.matchTool[TrackIndex].SimialityScore.ToString();
							if (inspection.matchTool[TrackIndex].AISimialityCheckResult)
							{
								txt_SimilarityScore.Background = Brushes.White;
							}
							else
							{
								txt_SimilarityScore.Background = Brushes.Fuchsia;
							}
						}
						if (radMatchText.IsChecked == true)
						{
							txt_MinTextWidth.Background = Brushes.White;
							txt_MinTextHeight.Background = Brushes.White;
							txt_MaxTextWidth.Background = Brushes.White;
							txt_MaxTextHeight.Background = Brushes.White;
							if (inspection.paramFile.UseFloat)
							{
								txt_MinTextWidthResult.Text = string.Format("{0:N3}", inspection.matchTool[TrackIndex].MinTextWidth);
								txt_MinTextHeightResult.Text = string.Format("{0:N3}", inspection.matchTool[TrackIndex].MinTextHeight);
							}
							else
							{
								txt_MinTextWidthResult.Text = string.Format("{0}", (int)inspection.matchTool[TrackIndex].MinTextWidth);
								txt_MinTextHeightResult.Text = string.Format("{0}", (int)inspection.matchTool[TrackIndex].MinTextHeight);
							}
							txt_MinColorMatchScore.Background = Brushes.White;
							txt_ColorMatchScoreResult.Text = inspection.matchTool[TrackIndex].ColorMatchScore.ToString();
							if (inspection.matchTool[TrackIndex].MinTextWidth < Convert.ToDouble(txt_MinTextWidth.Text))
							{
								txt_MinTextWidth.Background = Brushes.Fuchsia;
							}
							if (inspection.matchTool[TrackIndex].MinTextHeight < Convert.ToDouble(txt_MinTextHeight.Text))
							{
								txt_MinTextHeight.Background = Brushes.Fuchsia;
							}
							if (inspection.matchTool[TrackIndex].MinTextWidth > Convert.ToDouble(txt_MaxTextWidth.Text))
							{
								txt_MaxTextWidth.Background = Brushes.Fuchsia;
							}
							if (inspection.matchTool[TrackIndex].MinTextHeight > Convert.ToDouble(txt_MaxTextHeight.Text))
							{
								txt_MaxTextHeight.Background = Brushes.Fuchsia;
							}
							if ((double)inspection.matchTool[TrackIndex].ColorMatchScore < Convert.ToDouble(txt_MinColorMatchScore.Text))
							{
								txt_MinColorMatchScore.Background = Brushes.Fuchsia;
							}
							txt_TextArea.Background = Brushes.White;
							txt_MaxTextArea.Background = Brushes.White;
							if (cb_FindTextArea.SelectedIndex > 0)
							{
								int num3 = cb_FindTextArea.SelectedIndex;
								if (inspection.matchTool[TrackIndex].TextAreaVaildCount > cb_FindTextArea.SelectedIndex)
								{
									num3 = inspection.matchTool[TrackIndex].TextAreaVaildCount;
								}
								if (inspection.matchTool[TrackIndex].FoundTextArea)
								{
									for (int i = 0; i < inspection.matchTool[TrackIndex].MultiBlob.Count; i++)
									{
										ShowBitmap.Rectangle(inspection.matchTool[TrackIndex].MultiBlob[i].NGRect, Scalar.LightGreen);
									}
									txt_TextArea.Background = Brushes.White;
									cb_FindTextArea.Background = Brushes.White;
								}
								else
								{
									bool flag = false;
									bool flag2 = false;
									for (int j = 0; j < inspection.matchTool[TrackIndex].MultiBlob.Count; j++)
									{
										if (j < num3)
										{
											ShowBitmap.Rectangle(inspection.matchTool[TrackIndex].MultiBlob[j].NGRect, Scalar.Red);
											flag = inspection.matchTool[TrackIndex].MultiBlob[j].AreaMinNG;
											flag2 = inspection.matchTool[TrackIndex].MultiBlob[j].AreaMaxNG;
										}
									}
									if (flag)
									{
										txt_TextArea.Background = Brushes.Fuchsia;
									}
									if (flag2)
									{
										txt_MaxTextArea.Background = Brushes.Fuchsia;
									}
									cb_FindTextArea.Background = Brushes.Fuchsia;
									txt_AllResult.Text = inspection.matchTool[TrackIndex].Tracker1ResultText;
								}
								string text = "";
								for (int k = 0; k < inspection.matchTool[TrackIndex].MultiBlob.Count; k++)
								{
									if (k < num3)
									{
										string arg = ((!inspection.paramFile.UseFloat) ? string.Format("{0}", inspection.matchTool[TrackIndex].MultiBlob[k].Area) : string.Format("{0:N3}", inspection.matchTool[TrackIndex].MultiBlob[k].Area));
										text = ((LanguageType != 0) ? ((LanguageType != 1) ? (text + string.Format("文字区域[{0}]，面积={1}\n", k + 1, arg)) : (text + string.Format("文字區域[{0}]，面積={1}\n", k + 1, arg))) : (text + string.Format("Text[{0}]，Area={1}\n", k + 1, arg)));
									}
								}
								txt_AllResult.Text += text;
							}
							if (cb_FindCloseArea.SelectedIndex > 0)
							{
								OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(inspection.matchTool[TrackIndex].CloseAreaRect1.Left, inspection.matchTool[TrackIndex].CloseAreaRect1.Top, inspection.matchTool[TrackIndex].CloseAreaRect1.Width, inspection.matchTool[TrackIndex].CloseAreaRect1.Height);
								OpenCvSharp.Rect rect3 = new OpenCvSharp.Rect(inspection.matchTool[TrackIndex].CloseAreaRect2.Left, inspection.matchTool[TrackIndex].CloseAreaRect2.Top, inspection.matchTool[TrackIndex].CloseAreaRect2.Width, inspection.matchTool[TrackIndex].CloseAreaRect2.Height);
								string text2 = "";
								if (inspection.paramFile.UseFloat)
								{
									if (LanguageType == 0)
									{
										text2 += string.Format("CloseArea[1]，Area={0:N3}\n", inspection.matchTool[TrackIndex].CloseArea1);
										text2 += string.Format("CloseArea[2]，Area={0:N3}\n", inspection.matchTool[TrackIndex].CloseArea2);
									}
									else if (LanguageType == 1)
									{
										text2 += string.Format("封閉區域[1]，面積={0:N3}\n", inspection.matchTool[TrackIndex].CloseArea1);
										text2 += string.Format("封閉區域[2]，面積={0:N3}\n", inspection.matchTool[TrackIndex].CloseArea2);
									}
									else
									{
										text2 += string.Format("封闭区域[1]，面积={0:N3}\n", inspection.matchTool[TrackIndex].CloseArea1);
										text2 += string.Format("封闭区域[2]，面积={0:N3}\n", inspection.matchTool[TrackIndex].CloseArea2);
									}
								}
								else if (LanguageType == 0)
								{
									text2 += string.Format("CloseArea[1]，Area={0}\n", (int)inspection.matchTool[TrackIndex].CloseArea1);
									text2 += string.Format("CloseArea[2]，Area={0}\n", (int)inspection.matchTool[TrackIndex].CloseArea2);
								}
								else if (LanguageType == 1)
								{
									text2 += string.Format("封閉區域[1]，面積={0}\n", (int)inspection.matchTool[TrackIndex].CloseArea1);
									text2 += string.Format("封閉區域[2]，面積={0}\n", (int)inspection.matchTool[TrackIndex].CloseArea2);
								}
								else
								{
									text2 += string.Format("封闭区域[1]，面积={0}\n", (int)inspection.matchTool[TrackIndex].CloseArea1);
									text2 += string.Format("封闭区域[2]，面积={0}\n", (int)inspection.matchTool[TrackIndex].CloseArea2);
								}
								txt_AllResult.Text += text2;
								cb_FindCloseArea.Background = Brushes.White;
								txt_CloseArea.Background = Brushes.White;
								txt_MaxCloseArea.Background = Brushes.White;
								if (inspection.matchTool[TrackIndex].FoundCloseAreaResult && inspection.matchTool[TrackIndex].FoundMaxCloseAreaResult)
								{
									if (rect2.Width > 0 && rect2.Height > 0)
									{
										ShowBitmap.Rectangle(rect2, Scalar.LightYellow, 2);
									}
									if (rect3.Width > 0 && rect3.Height > 0)
									{
										ShowBitmap.Rectangle(rect3, Scalar.LightYellow, 2);
									}
								}
								else
								{
									if (rect2.Width > 0 && rect2.Height > 0)
									{
										ShowBitmap.Rectangle(rect2, Scalar.Red, 2);
									}
									if (rect3.Width > 0 && rect3.Height > 0)
									{
										ShowBitmap.Rectangle(rect3, Scalar.Red, 2);
									}
									if (!inspection.matchTool[TrackIndex].FoundCloseAreaResult)
									{
										txt_CloseArea.Background = Brushes.Fuchsia;
									}
									if (!inspection.matchTool[TrackIndex].FoundMaxCloseAreaResult)
									{
										txt_MaxCloseArea.Background = Brushes.Fuchsia;
									}
								}
								if (!inspection.matchTool[TrackIndex].FoundCloseCountResult)
								{
									cb_FindCloseArea.Background = Brushes.Fuchsia;
								}
							}
						}
						if (chk_MatchShape.IsChecked == true)
						{
							txt_MinShapeScore.Background = Brushes.White;
							if (inspection.matchTool[TrackIndex].MatchShpaeScore < Convert.ToInt32(txt_MinShapeScore.Text))
							{
								txt_MinShapeScore.Background = Brushes.Fuchsia;
							}
							txt_MinShapeScoreResult.Text = inspection.matchTool[TrackIndex].MatchShpaeScore.ToString();
						}
						ImageSource imageSource = inspection.buffers.MatToBitmapImage(ShowBitmap);
						ImageBrush imageBrush = new ImageBrush();
						imageBrush.ImageSource = imageSource;
						cnv_Image.Background = imageBrush;
						imageSource = null;
						THD_Preview = true;
					}
					else
					{
						TOOL_RESULT iresult2 = TOOL_RESULT.EM_OK;
						TimeStart();
						bool flag3 = inspection.matchTool[TrackIndex].Inspect(TrackIndex, ref iresult2);
						double num4 = TimeStop();
						lab_Time.Content = string.Format("{0:N3}", num4);
						SetResult(flag3);
						if (flag3)
						{
							txt_OCRResult.Background = Brushes.Lime;
						}
						else
						{
							txt_OCRResult.Background = Brushes.Fuchsia;
						}
						if (inspection.buffers.LocatedSrc.Channels() == 3)
						{
							ShowBitmap = inspection.buffers.LocatedSrc.Clone();
						}
						else
						{
							Cv2.CvtColor(inspection.buffers.LocatedSrc, ShowBitmap, ColorConversionCodes.GRAY2BGR);
						}
						txt_OCRResult.Text = inspection.matchTool[TrackIndex].OCRResult;
						txt_AllResult.Clear();
						txt_AllResult.Text = inspection.matchTool[TrackIndex].Tracker1ResultText;
						ImageSource imageSource2 = inspection.buffers.MatToBitmapImage(ShowBitmap);
						ImageBrush imageBrush2 = new ImageBrush();
						imageBrush2.ImageSource = imageSource2;
						cnv_Image.Background = imageBrush2;
						imageSource2 = null;
						THD_Preview = true;
					}
					return;
				}
				catch (Exception ex)
				{
					string text3 = "MTW07-MatchToolWindow-Btn_Test_Click()";
					CVBuffer.SaveLog(text3, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
					text3 += ex.ToString();
					if (ShowMessage)
					{
						MessageBox.Show(text3, "錯誤");
					}
					return;
				}
			}
			rt_Result.Fill = Brushes.White;
			Label_ResultError.Visibility = Visibility.Visible;
			if (LanguageType == 0)
			{
				Label_ResultError.Content = "Select Face Doesn't Match! ByPass!";
			}
			else if (LanguageType == 1)
			{
				Label_ResultError.Content = "選面不符合，ByPass!";
			}
			else if (LanguageType == 2)
			{
				Label_ResultError.Content = "选面不符合，ByPass!";
			}
			lab_Time.Content = "0";
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/aoi/matchtoolwindow.xaml", UriKind.Relative);
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
			MainW = (MatchToolWindow)target;
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
			break;
		case 11:
			txt_Search1Label = (TextBlock)target;
			break;
		case 12:
			Label_Result = (Label)target;
			break;
		case 13:
			rt_ReferenceRect1 = (Rectangle)target;
			break;
		case 14:
			rt_ReferenceRect2 = (Rectangle)target;
			break;
		case 15:
			Label_ResultError = (Label)target;
			break;
		case 16:
			lab_FaceIndex = (Label)target;
			break;
		case 17:
			txt_PreviewMode = (TextBox)target;
			break;
		case 18:
			bor_PatternSet = (Rectangle)target;
			break;
		case 19:
			tab_Function = (TabControl)target;
			tab_Function.SelectionChanged += Tab_Function_SelectionChanged;
			break;
		case 20:
			tab_Main = (TabItem)target;
			break;
		case 21:
			cb_TrackerSelect = (ComboBox)target;
			cb_TrackerSelect.SelectionChanged += Cb_TrackerSelect_SelectionChanged;
			break;
		case 22:
			btn_Reverse = (Button)target;
			btn_Reverse.Click += Btn_Reverse_Click;
			break;
		case 23:
			btn_RecipeCopy = (Button)target;
			btn_RecipeCopy.Click += Btn_RecipeCopy_Click;
			break;
		case 24:
			btn_RecipePaste = (Button)target;
			btn_RecipePaste.Click += Btn_RecipePaste_Click;
			break;
		case 25:
			txt_TrackerName = (TextBox)target;
			break;
		case 26:
			ts_TrackerON = (ToggleSwitch)target;
			ts_TrackerON.Click += Ts_TrackerON_Click;
			break;
		case 27:
			lab_TrackIndex = (Label)target;
			break;
		case 28:
			lab_Pareto = (Label)target;
			break;
		case 29:
			cb_ParetoChart = (ComboBox)target;
			break;
		case 30:
			lab_Time = (Label)target;
			break;
		case 31:
			gb_BasicSetup = (GroupBox)target;
			break;
		case 32:
			grd_SIMP_THD_2 = (Grid)target;
			break;
		case 33:
			rt_Result = (Rectangle)target;
			break;
		case 34:
			btn_Test = (Button)target;
			btn_Test.Click += Btn_Test_Click;
			break;
		case 35:
			cb_ErrorBin = (ComboBox)target;
			break;
		case 36:
			grd_SelectFace = (Grid)target;
			break;
		case 37:
			btn_FaceA = (Button)target;
			btn_FaceA.Click += Btn_FaceA_Click;
			break;
		case 38:
			btn_FaceB = (Button)target;
			btn_FaceB.Click += Btn_FaceA_Click;
			break;
		case 39:
			btn_FaceC = (Button)target;
			btn_FaceC.Click += Btn_FaceA_Click;
			break;
		case 40:
			btn_FaceD = (Button)target;
			btn_FaceD.Click += Btn_FaceA_Click;
			break;
		case 41:
			chk_SelectFace = (CheckBox)target;
			chk_SelectFace.Click += Chk_SelectFace_Click;
			break;
		case 42:
			rad_Match = (RadioButton)target;
			rad_Match.Click += Rad_Match_Click;
			break;
		case 43:
			grd_MatchTool = (Grid)target;
			break;
		case 44:
			grd_Found = (Grid)target;
			break;
		case 45:
			txt_PatternName21 = (TextBox)target;
			txt_PatternName21.PreviewMouseLeftButtonDown += Txt_PatternName21_PreviewMouseLeftButtonDown;
			break;
		case 46:
			img_PatternFound = (Image)target;
			img_PatternFound.PreviewMouseRightButtonDown += Img_PatternFound_PreviewMouseRightButtonDown;
			break;
		case 47:
			lab_PatternFoundInfo = (Label)target;
			break;
		case 48:
			grd_Pattern = (Grid)target;
			break;
		case 49:
			label_Pattern1 = (Label)target;
			break;
		case 50:
			img_PatternSet = (Image)target;
			break;
		case 51:
			cb_Pattern1 = (ComboBox)target;
			cb_Pattern1.SelectionChanged += Cb_Pattern_SelectionChanged;
			break;
		case 52:
			btn_ShowPattern1 = (Button)target;
			btn_ShowPattern1.Click += Btn_ShowPattern1_Click;
			break;
		case 53:
			label_Pattern2 = (Label)target;
			break;
		case 54:
			cb_Pattern2 = (ComboBox)target;
			cb_Pattern2.SelectionChanged += Cb_Pattern_SelectionChanged;
			break;
		case 55:
			btn_ShowPattern2 = (Button)target;
			btn_ShowPattern2.Click += Btn_ShowPattern1_Click;
			break;
		case 56:
			label_Pattern3 = (Label)target;
			break;
		case 57:
			cb_Pattern3 = (ComboBox)target;
			cb_Pattern3.SelectionChanged += Cb_Pattern_SelectionChanged;
			break;
		case 58:
			btn_ShowPattern3 = (Button)target;
			btn_ShowPattern3.Click += Btn_ShowPattern1_Click;
			break;
		case 59:
			label_Pattern4 = (Label)target;
			break;
		case 60:
			cb_Pattern4 = (ComboBox)target;
			cb_Pattern4.SelectionChanged += Cb_Pattern_SelectionChanged;
			break;
		case 61:
			btn_ShowPattern4 = (Button)target;
			btn_ShowPattern4.Click += Btn_ShowPattern1_Click;
			break;
		case 62:
			chk_EnableAISimilarityCheck = (CheckBox)target;
			break;
		case 63:
			txt_SimilarityScore = (TextBox)target;
			break;
		case 64:
			btn_SetOnnxFile = (Button)target;
			btn_SetOnnxFile.Click += Btn_SetOnnxFile_Click;
			break;
		case 65:
			txt_SimilarityScoreResult = (TextBox)target;
			break;
		case 66:
			lab_PatternSetInfo = (Label)target;
			break;
		case 67:
			ep_Result = (Ellipse)target;
			break;
		case 68:
			txt_MatchScoreMin = (TextBox)target;
			txt_MatchScoreMin.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 69:
			txt_MatchScoreMax = (TextBox)target;
			txt_MatchScoreMax.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 70:
			lab_MatchScore = (Label)target;
			break;
		case 71:
			btn_ReDraw_Copy = (Button)target;
			break;
		case 72:
			btn_ReDraw_Copy1 = (Button)target;
			break;
		case 73:
			btn_ReDraw_Copy2 = (Button)target;
			break;
		case 74:
			grd_Second = (Grid)target;
			break;
		case 75:
			chk_RotateInspect = (CheckBox)target;
			break;
		case 76:
			chk_TextColorisWhite = (CheckBox)target;
			break;
		case 77:
			cb_DEStart = (ComboBox)target;
			break;
		case 78:
			cb_DEStartCount = (ComboBox)target;
			break;
		case 79:
			rad_EnableAutolThd = (RadioButton)target;
			break;
		case 80:
			rad_EnableManualThd = (RadioButton)target;
			break;
		case 81:
			rs_BinaryThd = (RangeSlider)target;
			rs_BinaryThd.CentralThumbDragDelta += Rs_BinaryThd_CentralThumbDragDelta;
			rs_BinaryThd.PreviewMouseLeftButtonUp += Rs_BinaryThd_PreviewMouseLeftButtonUp;
			rs_BinaryThd.UpperThumbDragDelta += Rs_BinaryThd_UpperThumbDragDelta;
			rs_BinaryThd.LowerThumbDragDelta += Rs_BinaryThd_LowerThumbDragDelta;
			rs_BinaryThd.ValueChanged += Rs_BinaryThd_ValueChanged;
			break;
		case 82:
			txt_BinaryThdMin = (TextBox)target;
			break;
		case 83:
			txt_BinaryThdMax = (TextBox)target;
			break;
		case 84:
			chk_MaskInFound = (CheckBox)target;
			break;
		case 85:
			lab_Score1 = (Label)target;
			break;
		case 86:
			lab_Score2 = (Label)target;
			break;
		case 87:
			lab_Score3 = (Label)target;
			break;
		case 88:
			lab_Score4 = (Label)target;
			break;
		case 89:
			btn_ReSetComboBox1 = (Button)target;
			btn_ReSetComboBox1.Click += Btn_ReSetComboBox_Click;
			break;
		case 90:
			btn_ReSetComboBox2 = (Button)target;
			btn_ReSetComboBox2.Click += Btn_ReSetComboBox_Click;
			break;
		case 91:
			btn_ReSetComboBox3 = (Button)target;
			btn_ReSetComboBox3.Click += Btn_ReSetComboBox_Click;
			break;
		case 92:
			txt_OnnxName = (TextBox)target;
			break;
		case 93:
			grd_OCR = (Grid)target;
			break;
		case 94:
			txt_OCRSetup = (TextBox)target;
			break;
		case 95:
			txt_OCRResult = (TextBox)target;
			break;
		case 96:
			rad_OCR = (RadioButton)target;
			rad_OCR.Click += Rad_Match_Click;
			break;
		case 97:
			chk_OCRTextColorisWhite = (CheckBox)target;
			break;
		case 98:
			cb_Channel_OCR = (ComboBox)target;
			break;
		case 99:
			grd_Third = (Grid)target;
			break;
		case 100:
			rad_UseOldDB = (RadioButton)target;
			rad_UseOldDB.Click += Rad_UsePublicDB_Click;
			break;
		case 101:
			rad_UsePublicDB = (RadioButton)target;
			rad_UsePublicDB.Click += Rad_UsePublicDB_Click;
			break;
		case 102:
			grd_QuickSetup = (Grid)target;
			break;
		case 103:
			cb_QuickSetup = (ComboBox)target;
			cb_QuickSetup.SelectionChanged += Cb_QuickSetup_SelectionChanged;
			break;
		case 104:
			btn_QuickSetupFind = (Button)target;
			btn_QuickSetupFind.Click += Btn_QuickSetupFind_Click;
			break;
		case 105:
			btn_QuickSetupRead = (Button)target;
			btn_QuickSetupRead.Click += Btn_QuickSetupRead_Click;
			break;
		case 106:
			btn_QuickSetupWrite = (Button)target;
			btn_QuickSetupWrite.Click += Btn_QuickSetupWrite_Click;
			break;
		case 107:
			btn_QuickSetupLock = (Button)target;
			btn_QuickSetupLock.Click += Btn_QuickSetupLock_Click;
			break;
		case 108:
			btn_QuickSetupCancel = (Button)target;
			btn_QuickSetupCancel.Click += Btn_QuickSetupCancel_Click;
			break;
		case 109:
			txt_QuickSetupName = (TextBox)target;
			txt_QuickSetupName.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 110:
			cb_AlignToMatchText = (CheckBox)target;
			cb_AlignToMatchText.Checked += Cb_AlignToMatchText_Checked;
			cb_AlignToMatchText.Unchecked += Cb_AlignToMatchText_Unchecked;
			break;
		case 111:
			exp_TrackerSetup = (Expander)target;
			break;
		case 112:
			grd_SIMP_DilateErode1_Copy = (Grid)target;
			break;
		case 113:
			grd_AdvSetup = (Grid)target;
			break;
		case 114:
			radMatchText = (RadioButton)target;
			radMatchText.Click += Rad_Tracker1AutoSize_Click;
			break;
		case 115:
			radMatchPattern = (RadioButton)target;
			radMatchPattern.Click += Rad_Tracker1AlignSelect_Click;
			break;
		case 116:
			txt_Tracker1Pos_Copy = (Label)target;
			break;
		case 117:
			txt_Tracker1Pos_Copy1 = (Label)target;
			break;
		case 118:
			txt_Tracker1Pos_Copy2 = (Label)target;
			break;
		case 119:
			txt_Tracker1Pos_Copy3 = (Label)target;
			break;
		case 120:
			txt_DiffRatio = (TextBox)target;
			break;
		case 121:
			txt_WidthRatio = (TextBox)target;
			break;
		case 122:
			txt_AreaRatio = (TextBox)target;
			break;
		case 123:
			txt_HeightRatio = (TextBox)target;
			break;
		case 124:
			txt_ColorMatchScoreResult = (TextBox)target;
			break;
		case 125:
			txt_Tracker1Pos_Copy4 = (Label)target;
			break;
		case 126:
			img_PatternFoundBinary = (Image)target;
			img_PatternFoundBinary.MouseLeftButtonDown += Img_PatternFoundBinary_MouseLeftButtonDown;
			break;
		case 127:
			txt_MinColorMatchScore = (TextBox)target;
			break;
		case 128:
			cb_FindCloseArea = (ComboBox)target;
			break;
		case 129:
			txt_CloseArea = (TextBox)target;
			txt_CloseArea.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 130:
			txt_MinTextWidth = (TextBox)target;
			txt_MinTextWidth.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 131:
			txt_MinTextWidthResult = (TextBox)target;
			break;
		case 132:
			txt_MinTextHeight = (TextBox)target;
			txt_MinTextHeight.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 133:
			txt_MinTextHeightResult = (TextBox)target;
			break;
		case 134:
			txt_CloseArea2 = (TextBox)target;
			break;
		case 135:
			txt_CloseArea3 = (TextBox)target;
			break;
		case 136:
			chk_MatchShape = (CheckBox)target;
			break;
		case 137:
			txt_MinShapeScore = (TextBox)target;
			txt_MinShapeScore.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 138:
			txt_MinShapeScoreResult = (TextBox)target;
			break;
		case 139:
			txt_MaxTextWidth = (TextBox)target;
			txt_MaxTextWidth.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 140:
			txt_MaxTextHeight = (TextBox)target;
			txt_MaxTextHeight.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 141:
			cb_FindTextArea = (ComboBox)target;
			break;
		case 142:
			txt_TextArea = (TextBox)target;
			txt_TextArea.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 143:
			txt_MaxCloseArea = (TextBox)target;
			txt_MaxCloseArea.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 144:
			txt_MaxTextArea = (TextBox)target;
			txt_MaxTextArea.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 145:
			txt_TxtPosDiff = (TextBox)target;
			txt_TxtPosDiff.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 146:
			txt_TxtWidthDiff = (TextBox)target;
			txt_TxtWidthDiff.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 147:
			txt_TxtHeightDiff = (TextBox)target;
			txt_TxtHeightDiff.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 148:
			txt_TxtAreaDiff = (TextBox)target;
			txt_TxtAreaDiff.PreviewMouseDoubleClick += Txt_MatchScoreMin_PreviewMouseDoubleClick;
			break;
		case 149:
			chk_UseFoundToMask = (CheckBox)target;
			break;
		case 150:
			btn_PrevTracker = (Button)target;
			btn_PrevTracker.Click += Btn_PrevTracker;
			break;
		case 151:
			btn_NextTracker = (Button)target;
			btn_NextTracker.Click += Btn_NextTracker_Click;
			break;
		case 152:
			tab_PatternDB = (TabItem)target;
			break;
		case 153:
			gb_BasicSetup_Copy = (GroupBox)target;
			break;
		case 154:
			grd_SIMP_THD_1 = (Grid)target;
			break;
		case 155:
			btn_ChannelR = (Button)target;
			btn_ChannelR.Click += Btn_ChannelR_Click;
			break;
		case 156:
			btn_ChannelG = (Button)target;
			btn_ChannelG.Click += Btn_ChannelR_Click;
			break;
		case 157:
			btn_ChannelB = (Button)target;
			btn_ChannelB.Click += Btn_ChannelR_Click;
			break;
		case 158:
			btn_ChannelH = (Button)target;
			btn_ChannelH.Click += Btn_ChannelR_Click;
			break;
		case 159:
			btn_ChannelS = (Button)target;
			btn_ChannelS.Click += Btn_ChannelR_Click;
			break;
		case 160:
			btn_ChannelV = (Button)target;
			btn_ChannelV.Click += Btn_ChannelR_Click;
			break;
		case 161:
			btn_ChannelGray = (Button)target;
			btn_ChannelGray.Click += Btn_ChannelR_Click;
			break;
		case 162:
			btn_ChannelC1 = (Button)target;
			btn_ChannelC1.Click += Btn_ChannelR_Click;
			break;
		case 163:
			btn_ChannelC2 = (Button)target;
			btn_ChannelC2.Click += Btn_ChannelR_Click;
			break;
		case 164:
			btn_ChannelC3 = (Button)target;
			btn_ChannelC3.Click += Btn_ChannelR_Click;
			break;
		case 165:
			btn_ChannelRemove = (Button)target;
			btn_ChannelRemove.Click += Btn_ChannelR_Click;
			break;
		case 166:
			txt_PatternName1 = (TextBox)target;
			break;
		case 167:
			img_PatternImg1 = (Image)target;
			img_PatternImg1.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg1.Drop += Img_PatternImg1_Drop;
			break;
		case 168:
			btn_SetPattern = (Button)target;
			btn_SetPattern.Click += Btn_SetPattern_Click;
			break;
		case 169:
			lab_PatternInfo1 = (Label)target;
			break;
		case 170:
			txt_PatternName6 = (TextBox)target;
			break;
		case 171:
			img_PatternImg6 = (Image)target;
			img_PatternImg6.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg6.Drop += Img_PatternImg1_Drop;
			break;
		case 172:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 173:
			lab_PatternInfo6 = (Label)target;
			break;
		case 174:
			txt_PatternName11 = (TextBox)target;
			break;
		case 175:
			img_PatternImg11 = (Image)target;
			img_PatternImg11.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg11.Drop += Img_PatternImg1_Drop;
			break;
		case 176:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 177:
			lab_PatternInfo11 = (Label)target;
			break;
		case 178:
			txt_PatternName16 = (TextBox)target;
			break;
		case 179:
			img_PatternImg16 = (Image)target;
			img_PatternImg16.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg16.Drop += Img_PatternImg1_Drop;
			break;
		case 180:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 181:
			lab_PatternInfo16 = (Label)target;
			break;
		case 182:
			txt_PatternName2 = (TextBox)target;
			break;
		case 183:
			img_PatternImg2 = (Image)target;
			img_PatternImg2.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg2.Drop += Img_PatternImg1_Drop;
			break;
		case 184:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 185:
			lab_PatternInfo2 = (Label)target;
			break;
		case 186:
			txt_PatternName7 = (TextBox)target;
			break;
		case 187:
			img_PatternImg7 = (Image)target;
			img_PatternImg7.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg7.Drop += Img_PatternImg1_Drop;
			break;
		case 188:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 189:
			lab_PatternInfo7 = (Label)target;
			break;
		case 190:
			txt_PatternName12 = (TextBox)target;
			break;
		case 191:
			img_PatternImg12 = (Image)target;
			img_PatternImg12.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg12.Drop += Img_PatternImg1_Drop;
			break;
		case 192:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 193:
			lab_PatternInfo12 = (Label)target;
			break;
		case 194:
			txt_PatternName17 = (TextBox)target;
			break;
		case 195:
			img_PatternImg17 = (Image)target;
			img_PatternImg17.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg17.Drop += Img_PatternImg1_Drop;
			break;
		case 196:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 197:
			lab_PatternInfo17 = (Label)target;
			break;
		case 198:
			txt_PatternName3 = (TextBox)target;
			break;
		case 199:
			img_PatternImg3 = (Image)target;
			img_PatternImg3.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg3.Drop += Img_PatternImg1_Drop;
			break;
		case 200:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 201:
			lab_PatternInfo3 = (Label)target;
			break;
		case 202:
			txt_PatternName8 = (TextBox)target;
			break;
		case 203:
			img_PatternImg8 = (Image)target;
			img_PatternImg8.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg8.Drop += Img_PatternImg1_Drop;
			break;
		case 204:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 205:
			lab_PatternInfo8 = (Label)target;
			break;
		case 206:
			txt_PatternName13 = (TextBox)target;
			break;
		case 207:
			img_PatternImg13 = (Image)target;
			img_PatternImg13.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg13.Drop += Img_PatternImg1_Drop;
			break;
		case 208:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 209:
			lab_PatternInfo13 = (Label)target;
			break;
		case 210:
			txt_PatternName18 = (TextBox)target;
			break;
		case 211:
			img_PatternImg18 = (Image)target;
			img_PatternImg18.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg18.Drop += Img_PatternImg1_Drop;
			break;
		case 212:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 213:
			lab_PatternInfo18 = (Label)target;
			break;
		case 214:
			txt_PatternName4 = (TextBox)target;
			break;
		case 215:
			img_PatternImg4 = (Image)target;
			img_PatternImg4.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg4.Drop += Img_PatternImg1_Drop;
			break;
		case 216:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 217:
			lab_PatternInfo4 = (Label)target;
			break;
		case 218:
			txt_PatternName9 = (TextBox)target;
			break;
		case 219:
			img_PatternImg9 = (Image)target;
			img_PatternImg9.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg9.Drop += Img_PatternImg1_Drop;
			break;
		case 220:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 221:
			lab_PatternInfo9 = (Label)target;
			break;
		case 222:
			txt_PatternName14 = (TextBox)target;
			break;
		case 223:
			img_PatternImg14 = (Image)target;
			img_PatternImg14.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg14.Drop += Img_PatternImg1_Drop;
			break;
		case 224:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 225:
			lab_PatternInfo14 = (Label)target;
			break;
		case 226:
			txt_PatternName19 = (TextBox)target;
			break;
		case 227:
			img_PatternImg19 = (Image)target;
			img_PatternImg19.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg19.Drop += Img_PatternImg1_Drop;
			break;
		case 228:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 229:
			lab_PatternInfo19 = (Label)target;
			break;
		case 230:
			txt_PatternName5 = (TextBox)target;
			break;
		case 231:
			img_PatternImg5 = (Image)target;
			img_PatternImg5.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg5.Drop += Img_PatternImg1_Drop;
			break;
		case 232:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 233:
			lab_PatternInfo5 = (Label)target;
			break;
		case 234:
			txt_PatternName10 = (TextBox)target;
			break;
		case 235:
			img_PatternImg10 = (Image)target;
			img_PatternImg10.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg10.Drop += Img_PatternImg1_Drop;
			break;
		case 236:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 237:
			lab_PatternInfo10 = (Label)target;
			break;
		case 238:
			txt_PatternName15 = (TextBox)target;
			break;
		case 239:
			img_PatternImg15 = (Image)target;
			img_PatternImg15.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg15.Drop += Img_PatternImg1_Drop;
			break;
		case 240:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 241:
			lab_PatternInfo15 = (Label)target;
			break;
		case 242:
			txt_PatternName20 = (TextBox)target;
			break;
		case 243:
			img_PatternImg20 = (Image)target;
			img_PatternImg20.PreviewMouseRightButtonDown += Img_PatternImg1_PreviewMouseRightButtonDown;
			img_PatternImg20.Drop += Img_PatternImg1_Drop;
			break;
		case 244:
			((Button)target).Click += Btn_SetPattern_Click;
			break;
		case 245:
			lab_PatternInfo20 = (Label)target;
			break;
		case 246:
			tab_PublicDB = (TabItem)target;
			break;
		case 247:
			gb_BasicSetup_Copy1 = (GroupBox)target;
			break;
		case 248:
			grd_SIMP_THD_3 = (Grid)target;
			break;
		case 249:
			btn_ChannelR1 = (Button)target;
			btn_ChannelR1.Click += Btn_ChannelR_Click;
			break;
		case 250:
			btn_ChannelG1 = (Button)target;
			btn_ChannelG1.Click += Btn_ChannelR_Click;
			break;
		case 251:
			btn_ChannelB1 = (Button)target;
			btn_ChannelB1.Click += Btn_ChannelR_Click;
			break;
		case 252:
			btn_ChannelH1 = (Button)target;
			btn_ChannelH1.Click += Btn_ChannelR_Click;
			break;
		case 253:
			btn_ChannelS1 = (Button)target;
			btn_ChannelS1.Click += Btn_ChannelR_Click;
			break;
		case 254:
			btn_ChannelV1 = (Button)target;
			btn_ChannelV1.Click += Btn_ChannelR_Click;
			break;
		case 255:
			btn_ChannelGray1 = (Button)target;
			btn_ChannelGray1.Click += Btn_ChannelR_Click;
			break;
		case 256:
			btn_ChannelC11 = (Button)target;
			btn_ChannelC11.Click += Btn_ChannelR_Click;
			break;
		case 257:
			btn_ChannelC21 = (Button)target;
			btn_ChannelC21.Click += Btn_ChannelR_Click;
			break;
		case 258:
			btn_ChannelC31 = (Button)target;
			btn_ChannelC31.Click += Btn_ChannelR_Click;
			break;
		case 259:
			btn_ChannelRemove1 = (Button)target;
			btn_ChannelRemove1.Click += Btn_ChannelR_Click;
			break;
		case 260:
			btn_PublicPagePrev = (Button)target;
			btn_PublicPagePrev.Click += Btn_PublicPagePrev_Click;
			break;
		case 261:
			lab_PublicDBPage = (Label)target;
			break;
		case 262:
			btn_PublicPageNext = (Button)target;
			btn_PublicPageNext.Click += Btn_PublicPageNext_Click;
			break;
		case 263:
			grd_PPD1 = (Grid)target;
			break;
		case 264:
			gd_pdb001 = (Grid)target;
			break;
		case 265:
			txt_pbd001 = (TextBox)target;
			break;
		case 266:
			img_pbd001 = (Image)target;
			img_pbd001.Drop += Img_pbd_Drop;
			img_pbd001.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd001.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 267:
			lab_pbd001 = (Label)target;
			break;
		case 268:
			btn_pbd001 = (Button)target;
			btn_pbd001.Click += Btn_SetPBDPattern_Click;
			break;
		case 269:
			gd_pdb002 = (Grid)target;
			break;
		case 270:
			txt_pbd002 = (TextBox)target;
			break;
		case 271:
			lab_pbd002 = (Label)target;
			break;
		case 272:
			btn_pbd002 = (Button)target;
			btn_pbd002.Click += Btn_SetPBDPattern_Click;
			break;
		case 273:
			img_pbd002 = (Image)target;
			img_pbd002.Drop += Img_pbd_Drop;
			img_pbd002.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd002.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 274:
			gd_pdb003 = (Grid)target;
			break;
		case 275:
			txt_pbd003 = (TextBox)target;
			break;
		case 276:
			lab_pbd003 = (Label)target;
			break;
		case 277:
			btn_pbd003 = (Button)target;
			btn_pbd003.Click += Btn_SetPBDPattern_Click;
			break;
		case 278:
			img_pbd003 = (Image)target;
			img_pbd003.Drop += Img_pbd_Drop;
			img_pbd003.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd003.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 279:
			gd_pdb004 = (Grid)target;
			break;
		case 280:
			txt_pbd004 = (TextBox)target;
			break;
		case 281:
			lab_pbd004 = (Label)target;
			break;
		case 282:
			btn_pbd004 = (Button)target;
			btn_pbd004.Click += Btn_SetPBDPattern_Click;
			break;
		case 283:
			img_pbd004 = (Image)target;
			img_pbd004.Drop += Img_pbd_Drop;
			img_pbd004.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd004.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 284:
			gd_pdb005 = (Grid)target;
			break;
		case 285:
			txt_pbd005 = (TextBox)target;
			break;
		case 286:
			lab_pbd005 = (Label)target;
			break;
		case 287:
			btn_pbd005 = (Button)target;
			btn_pbd005.Click += Btn_SetPBDPattern_Click;
			break;
		case 288:
			img_pbd005 = (Image)target;
			img_pbd005.Drop += Img_pbd_Drop;
			img_pbd005.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd005.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 289:
			gd_pdb006 = (Grid)target;
			break;
		case 290:
			txt_pbd006 = (TextBox)target;
			break;
		case 291:
			lab_pbd006 = (Label)target;
			break;
		case 292:
			btn_pbd006 = (Button)target;
			btn_pbd006.Click += Btn_SetPBDPattern_Click;
			break;
		case 293:
			img_pbd006 = (Image)target;
			img_pbd006.Drop += Img_pbd_Drop;
			img_pbd006.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd006.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 294:
			gd_pdb007 = (Grid)target;
			break;
		case 295:
			txt_pbd007 = (TextBox)target;
			break;
		case 296:
			lab_pbd007 = (Label)target;
			break;
		case 297:
			btn_pbd007 = (Button)target;
			btn_pbd007.Click += Btn_SetPBDPattern_Click;
			break;
		case 298:
			img_pbd007 = (Image)target;
			img_pbd007.Drop += Img_pbd_Drop;
			img_pbd007.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd007.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 299:
			gd_pdb008 = (Grid)target;
			break;
		case 300:
			txt_pbd008 = (TextBox)target;
			break;
		case 301:
			lab_pbd008 = (Label)target;
			break;
		case 302:
			btn_pbd008 = (Button)target;
			btn_pbd008.Click += Btn_SetPBDPattern_Click;
			break;
		case 303:
			img_pbd008 = (Image)target;
			img_pbd008.Drop += Img_pbd_Drop;
			img_pbd008.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd008.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 304:
			gd_pdb009 = (Grid)target;
			break;
		case 305:
			txt_pbd009 = (TextBox)target;
			break;
		case 306:
			lab_pbd009 = (Label)target;
			break;
		case 307:
			btn_pbd009 = (Button)target;
			btn_pbd009.Click += Btn_SetPBDPattern_Click;
			break;
		case 308:
			img_pbd009 = (Image)target;
			img_pbd009.Drop += Img_pbd_Drop;
			img_pbd009.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd009.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 309:
			gd_pdb010 = (Grid)target;
			break;
		case 310:
			txt_pbd010 = (TextBox)target;
			break;
		case 311:
			lab_pbd010 = (Label)target;
			break;
		case 312:
			btn_pbd010 = (Button)target;
			btn_pbd010.Click += Btn_SetPBDPattern_Click;
			break;
		case 313:
			img_pbd010 = (Image)target;
			img_pbd010.Drop += Img_pbd_Drop;
			img_pbd010.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd010.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 314:
			grd_PPD2 = (Grid)target;
			break;
		case 315:
			gd_pdb011 = (Grid)target;
			break;
		case 316:
			txt_pbd011 = (TextBox)target;
			break;
		case 317:
			img_pbd011 = (Image)target;
			img_pbd011.Drop += Img_pbd_Drop;
			img_pbd011.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd011.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 318:
			lab_pbd011 = (Label)target;
			break;
		case 319:
			btn_pbd011 = (Button)target;
			btn_pbd011.Click += Btn_SetPBDPattern_Click;
			break;
		case 320:
			gd_pdb012 = (Grid)target;
			break;
		case 321:
			txt_pbd012 = (TextBox)target;
			break;
		case 322:
			lab_pbd012 = (Label)target;
			break;
		case 323:
			btn_pbd012 = (Button)target;
			btn_pbd012.Click += Btn_SetPBDPattern_Click;
			break;
		case 324:
			img_pbd012 = (Image)target;
			img_pbd012.Drop += Img_pbd_Drop;
			img_pbd012.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd012.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 325:
			gd_pdb013 = (Grid)target;
			break;
		case 326:
			txt_pbd013 = (TextBox)target;
			break;
		case 327:
			lab_pbd013 = (Label)target;
			break;
		case 328:
			btn_pbd013 = (Button)target;
			btn_pbd013.Click += Btn_SetPBDPattern_Click;
			break;
		case 329:
			img_pbd013 = (Image)target;
			img_pbd013.Drop += Img_pbd_Drop;
			img_pbd013.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd013.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 330:
			gd_pdb014 = (Grid)target;
			break;
		case 331:
			txt_pbd014 = (TextBox)target;
			break;
		case 332:
			lab_pbd014 = (Label)target;
			break;
		case 333:
			btn_pbd014 = (Button)target;
			btn_pbd014.Click += Btn_SetPBDPattern_Click;
			break;
		case 334:
			img_pbd014 = (Image)target;
			img_pbd014.Drop += Img_pbd_Drop;
			img_pbd014.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd014.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 335:
			gd_pdb015 = (Grid)target;
			break;
		case 336:
			txt_pbd015 = (TextBox)target;
			break;
		case 337:
			lab_pbd015 = (Label)target;
			break;
		case 338:
			btn_pbd015 = (Button)target;
			btn_pbd015.Click += Btn_SetPBDPattern_Click;
			break;
		case 339:
			img_pbd015 = (Image)target;
			img_pbd015.Drop += Img_pbd_Drop;
			img_pbd015.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd015.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 340:
			gd_pdb016 = (Grid)target;
			break;
		case 341:
			txt_pbd016 = (TextBox)target;
			break;
		case 342:
			lab_pbd016 = (Label)target;
			break;
		case 343:
			btn_pbd016 = (Button)target;
			btn_pbd016.Click += Btn_SetPBDPattern_Click;
			break;
		case 344:
			img_pbd016 = (Image)target;
			img_pbd016.Drop += Img_pbd_Drop;
			img_pbd016.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd016.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 345:
			gd_pdb017 = (Grid)target;
			break;
		case 346:
			txt_pbd017 = (TextBox)target;
			break;
		case 347:
			lab_pbd017 = (Label)target;
			break;
		case 348:
			btn_pbd017 = (Button)target;
			btn_pbd017.Click += Btn_SetPBDPattern_Click;
			break;
		case 349:
			img_pbd017 = (Image)target;
			img_pbd017.Drop += Img_pbd_Drop;
			img_pbd017.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd017.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 350:
			gd_pdb018 = (Grid)target;
			break;
		case 351:
			txt_pbd018 = (TextBox)target;
			break;
		case 352:
			lab_pbd018 = (Label)target;
			break;
		case 353:
			btn_pbd018 = (Button)target;
			btn_pbd018.Click += Btn_SetPBDPattern_Click;
			break;
		case 354:
			img_pbd018 = (Image)target;
			img_pbd018.Drop += Img_pbd_Drop;
			img_pbd018.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd018.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 355:
			gd_pdb019 = (Grid)target;
			break;
		case 356:
			txt_pbd019 = (TextBox)target;
			break;
		case 357:
			lab_pbd019 = (Label)target;
			break;
		case 358:
			btn_pbd019 = (Button)target;
			btn_pbd019.Click += Btn_SetPBDPattern_Click;
			break;
		case 359:
			img_pbd019 = (Image)target;
			img_pbd019.Drop += Img_pbd_Drop;
			img_pbd019.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd019.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 360:
			gd_pdb020 = (Grid)target;
			break;
		case 361:
			txt_pbd020 = (TextBox)target;
			break;
		case 362:
			lab_pbd020 = (Label)target;
			break;
		case 363:
			btn_pbd020 = (Button)target;
			btn_pbd020.Click += Btn_SetPBDPattern_Click;
			break;
		case 364:
			img_pbd020 = (Image)target;
			img_pbd020.Drop += Img_pbd_Drop;
			img_pbd020.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd020.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 365:
			grd_PPD3 = (Grid)target;
			break;
		case 366:
			gd_pdb021 = (Grid)target;
			break;
		case 367:
			txt_pbd021 = (TextBox)target;
			break;
		case 368:
			img_pbd021 = (Image)target;
			img_pbd021.Drop += Img_pbd_Drop;
			img_pbd021.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd021.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 369:
			lab_pbd021 = (Label)target;
			break;
		case 370:
			btn_pbd021 = (Button)target;
			btn_pbd021.Click += Btn_SetPBDPattern_Click;
			break;
		case 371:
			gd_pdb022 = (Grid)target;
			break;
		case 372:
			txt_pbd022 = (TextBox)target;
			break;
		case 373:
			lab_pbd022 = (Label)target;
			break;
		case 374:
			btn_pbd022 = (Button)target;
			btn_pbd022.Click += Btn_SetPBDPattern_Click;
			break;
		case 375:
			img_pbd022 = (Image)target;
			img_pbd022.Drop += Img_pbd_Drop;
			img_pbd022.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd022.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 376:
			gd_pdb023 = (Grid)target;
			break;
		case 377:
			txt_pbd023 = (TextBox)target;
			break;
		case 378:
			lab_pbd023 = (Label)target;
			break;
		case 379:
			btn_pbd023 = (Button)target;
			btn_pbd023.Click += Btn_SetPBDPattern_Click;
			break;
		case 380:
			img_pbd023 = (Image)target;
			img_pbd023.Drop += Img_pbd_Drop;
			img_pbd023.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd023.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 381:
			gd_pdb024 = (Grid)target;
			break;
		case 382:
			txt_pbd024 = (TextBox)target;
			break;
		case 383:
			lab_pbd024 = (Label)target;
			break;
		case 384:
			btn_pbd024 = (Button)target;
			btn_pbd024.Click += Btn_SetPBDPattern_Click;
			break;
		case 385:
			img_pbd024 = (Image)target;
			img_pbd024.Drop += Img_pbd_Drop;
			img_pbd024.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd024.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 386:
			gd_pdb025 = (Grid)target;
			break;
		case 387:
			txt_pbd025 = (TextBox)target;
			break;
		case 388:
			lab_pbd025 = (Label)target;
			break;
		case 389:
			btn_pbd025 = (Button)target;
			btn_pbd025.Click += Btn_SetPBDPattern_Click;
			break;
		case 390:
			img_pbd025 = (Image)target;
			img_pbd025.Drop += Img_pbd_Drop;
			img_pbd025.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd025.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 391:
			gd_pdb026 = (Grid)target;
			break;
		case 392:
			txt_pbd026 = (TextBox)target;
			break;
		case 393:
			lab_pbd026 = (Label)target;
			break;
		case 394:
			btn_pbd026 = (Button)target;
			btn_pbd026.Click += Btn_SetPBDPattern_Click;
			break;
		case 395:
			img_pbd026 = (Image)target;
			img_pbd026.Drop += Img_pbd_Drop;
			img_pbd026.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd026.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 396:
			gd_pdb027 = (Grid)target;
			break;
		case 397:
			txt_pbd027 = (TextBox)target;
			break;
		case 398:
			lab_pbd027 = (Label)target;
			break;
		case 399:
			btn_pbd027 = (Button)target;
			btn_pbd027.Click += Btn_SetPBDPattern_Click;
			break;
		case 400:
			img_pbd027 = (Image)target;
			img_pbd027.Drop += Img_pbd_Drop;
			img_pbd027.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd027.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 401:
			gd_pdb028 = (Grid)target;
			break;
		case 402:
			txt_pbd028 = (TextBox)target;
			break;
		case 403:
			lab_pbd028 = (Label)target;
			break;
		case 404:
			btn_pbd028 = (Button)target;
			btn_pbd028.Click += Btn_SetPBDPattern_Click;
			break;
		case 405:
			img_pbd028 = (Image)target;
			img_pbd028.Drop += Img_pbd_Drop;
			img_pbd028.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd028.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 406:
			gd_pdb029 = (Grid)target;
			break;
		case 407:
			txt_pbd029 = (TextBox)target;
			break;
		case 408:
			lab_pbd029 = (Label)target;
			break;
		case 409:
			btn_pbd029 = (Button)target;
			btn_pbd029.Click += Btn_SetPBDPattern_Click;
			break;
		case 410:
			img_pbd029 = (Image)target;
			img_pbd029.Drop += Img_pbd_Drop;
			img_pbd029.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd029.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 411:
			gd_pdb030 = (Grid)target;
			break;
		case 412:
			txt_pbd030 = (TextBox)target;
			break;
		case 413:
			lab_pbd030 = (Label)target;
			break;
		case 414:
			btn_pbd030 = (Button)target;
			btn_pbd030.Click += Btn_SetPBDPattern_Click;
			break;
		case 415:
			img_pbd030 = (Image)target;
			img_pbd030.Drop += Img_pbd_Drop;
			img_pbd030.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd030.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 416:
			grd_PPD4 = (Grid)target;
			break;
		case 417:
			gd_pdb031 = (Grid)target;
			break;
		case 418:
			txt_pbd031 = (TextBox)target;
			break;
		case 419:
			img_pbd031 = (Image)target;
			img_pbd031.Drop += Img_pbd_Drop;
			img_pbd031.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd031.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 420:
			lab_pbd031 = (Label)target;
			break;
		case 421:
			btn_pbd031 = (Button)target;
			btn_pbd031.Click += Btn_SetPBDPattern_Click;
			break;
		case 422:
			gd_pdb032 = (Grid)target;
			break;
		case 423:
			txt_pbd032 = (TextBox)target;
			break;
		case 424:
			lab_pbd032 = (Label)target;
			break;
		case 425:
			btn_pbd032 = (Button)target;
			btn_pbd032.Click += Btn_SetPBDPattern_Click;
			break;
		case 426:
			img_pbd032 = (Image)target;
			img_pbd032.Drop += Img_pbd_Drop;
			img_pbd032.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd032.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 427:
			gd_pdb033 = (Grid)target;
			break;
		case 428:
			txt_pbd033 = (TextBox)target;
			break;
		case 429:
			lab_pbd033 = (Label)target;
			break;
		case 430:
			btn_pbd033 = (Button)target;
			btn_pbd033.Click += Btn_SetPBDPattern_Click;
			break;
		case 431:
			img_pbd033 = (Image)target;
			img_pbd033.Drop += Img_pbd_Drop;
			img_pbd033.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd033.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 432:
			gd_pdb034 = (Grid)target;
			break;
		case 433:
			txt_pbd034 = (TextBox)target;
			break;
		case 434:
			lab_pbd034 = (Label)target;
			break;
		case 435:
			btn_pbd034 = (Button)target;
			btn_pbd034.Click += Btn_SetPBDPattern_Click;
			break;
		case 436:
			img_pbd034 = (Image)target;
			img_pbd034.Drop += Img_pbd_Drop;
			img_pbd034.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd034.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 437:
			gd_pdb035 = (Grid)target;
			break;
		case 438:
			txt_pbd035 = (TextBox)target;
			break;
		case 439:
			lab_pbd035 = (Label)target;
			break;
		case 440:
			btn_pbd035 = (Button)target;
			btn_pbd035.Click += Btn_SetPBDPattern_Click;
			break;
		case 441:
			img_pbd035 = (Image)target;
			img_pbd035.Drop += Img_pbd_Drop;
			img_pbd035.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd035.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 442:
			gd_pdb036 = (Grid)target;
			break;
		case 443:
			txt_pbd036 = (TextBox)target;
			break;
		case 444:
			lab_pbd036 = (Label)target;
			break;
		case 445:
			btn_pbd036 = (Button)target;
			btn_pbd036.Click += Btn_SetPBDPattern_Click;
			break;
		case 446:
			img_pbd036 = (Image)target;
			img_pbd036.Drop += Img_pbd_Drop;
			img_pbd036.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd036.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 447:
			gd_pdb037 = (Grid)target;
			break;
		case 448:
			txt_pbd037 = (TextBox)target;
			break;
		case 449:
			lab_pbd037 = (Label)target;
			break;
		case 450:
			btn_pbd037 = (Button)target;
			btn_pbd037.Click += Btn_SetPBDPattern_Click;
			break;
		case 451:
			img_pbd037 = (Image)target;
			img_pbd037.Drop += Img_pbd_Drop;
			img_pbd037.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd037.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 452:
			gd_pdb038 = (Grid)target;
			break;
		case 453:
			txt_pbd038 = (TextBox)target;
			break;
		case 454:
			lab_pbd038 = (Label)target;
			break;
		case 455:
			btn_pbd038 = (Button)target;
			btn_pbd038.Click += Btn_SetPBDPattern_Click;
			break;
		case 456:
			img_pbd038 = (Image)target;
			img_pbd038.Drop += Img_pbd_Drop;
			img_pbd038.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd038.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 457:
			gd_pdb039 = (Grid)target;
			break;
		case 458:
			txt_pbd039 = (TextBox)target;
			break;
		case 459:
			lab_pbd039 = (Label)target;
			break;
		case 460:
			btn_pbd039 = (Button)target;
			btn_pbd039.Click += Btn_SetPBDPattern_Click;
			break;
		case 461:
			img_pbd039 = (Image)target;
			img_pbd039.Drop += Img_pbd_Drop;
			img_pbd039.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd039.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 462:
			gd_pdb040 = (Grid)target;
			break;
		case 463:
			txt_pbd040 = (TextBox)target;
			break;
		case 464:
			lab_pbd040 = (Label)target;
			break;
		case 465:
			btn_pbd040 = (Button)target;
			btn_pbd040.Click += Btn_SetPBDPattern_Click;
			break;
		case 466:
			img_pbd040 = (Image)target;
			img_pbd040.Drop += Img_pbd_Drop;
			img_pbd040.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd040.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 467:
			grd_PPD5 = (Grid)target;
			break;
		case 468:
			gd_pdb041 = (Grid)target;
			break;
		case 469:
			txt_pbd041 = (TextBox)target;
			break;
		case 470:
			img_pbd041 = (Image)target;
			img_pbd041.Drop += Img_pbd_Drop;
			img_pbd041.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd041.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 471:
			lab_pbd041 = (Label)target;
			break;
		case 472:
			btn_pbd041 = (Button)target;
			btn_pbd041.Click += Btn_SetPBDPattern_Click;
			break;
		case 473:
			gd_pdb042 = (Grid)target;
			break;
		case 474:
			txt_pbd042 = (TextBox)target;
			break;
		case 475:
			lab_pbd042 = (Label)target;
			break;
		case 476:
			btn_pbd042 = (Button)target;
			btn_pbd042.Click += Btn_SetPBDPattern_Click;
			break;
		case 477:
			img_pbd042 = (Image)target;
			img_pbd042.Drop += Img_pbd_Drop;
			img_pbd042.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd042.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 478:
			gd_pdb043 = (Grid)target;
			break;
		case 479:
			txt_pbd043 = (TextBox)target;
			break;
		case 480:
			lab_pbd043 = (Label)target;
			break;
		case 481:
			btn_pbd043 = (Button)target;
			btn_pbd043.Click += Btn_SetPBDPattern_Click;
			break;
		case 482:
			img_pbd043 = (Image)target;
			img_pbd043.Drop += Img_pbd_Drop;
			img_pbd043.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd043.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 483:
			gd_pdb044 = (Grid)target;
			break;
		case 484:
			txt_pbd044 = (TextBox)target;
			break;
		case 485:
			lab_pbd044 = (Label)target;
			break;
		case 486:
			btn_pbd044 = (Button)target;
			btn_pbd044.Click += Btn_SetPBDPattern_Click;
			break;
		case 487:
			img_pbd044 = (Image)target;
			img_pbd044.Drop += Img_pbd_Drop;
			img_pbd044.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd044.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 488:
			gd_pdb045 = (Grid)target;
			break;
		case 489:
			txt_pbd045 = (TextBox)target;
			break;
		case 490:
			lab_pbd045 = (Label)target;
			break;
		case 491:
			btn_pbd045 = (Button)target;
			btn_pbd045.Click += Btn_SetPBDPattern_Click;
			break;
		case 492:
			img_pbd045 = (Image)target;
			img_pbd045.Drop += Img_pbd_Drop;
			img_pbd045.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd045.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 493:
			gd_pdb046 = (Grid)target;
			break;
		case 494:
			txt_pbd046 = (TextBox)target;
			break;
		case 495:
			lab_pbd046 = (Label)target;
			break;
		case 496:
			btn_pbd046 = (Button)target;
			btn_pbd046.Click += Btn_SetPBDPattern_Click;
			break;
		case 497:
			img_pbd046 = (Image)target;
			img_pbd046.Drop += Img_pbd_Drop;
			img_pbd046.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd046.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 498:
			gd_pdb047 = (Grid)target;
			break;
		case 499:
			txt_pbd047 = (TextBox)target;
			break;
		case 500:
			lab_pbd047 = (Label)target;
			break;
		case 501:
			btn_pbd047 = (Button)target;
			btn_pbd047.Click += Btn_SetPBDPattern_Click;
			break;
		case 502:
			img_pbd047 = (Image)target;
			img_pbd047.Drop += Img_pbd_Drop;
			img_pbd047.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd047.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 503:
			gd_pdb048 = (Grid)target;
			break;
		case 504:
			txt_pbd048 = (TextBox)target;
			break;
		case 505:
			lab_pbd048 = (Label)target;
			break;
		case 506:
			btn_pbd048 = (Button)target;
			btn_pbd048.Click += Btn_SetPBDPattern_Click;
			break;
		case 507:
			img_pbd048 = (Image)target;
			img_pbd048.Drop += Img_pbd_Drop;
			img_pbd048.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd048.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 508:
			gd_pdb049 = (Grid)target;
			break;
		case 509:
			txt_pbd049 = (TextBox)target;
			break;
		case 510:
			lab_pbd049 = (Label)target;
			break;
		case 511:
			btn_pbd049 = (Button)target;
			btn_pbd049.Click += Btn_SetPBDPattern_Click;
			break;
		case 512:
			img_pbd049 = (Image)target;
			img_pbd049.Drop += Img_pbd_Drop;
			img_pbd049.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd049.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 513:
			gd_pdb050 = (Grid)target;
			break;
		case 514:
			txt_pbd050 = (TextBox)target;
			break;
		case 515:
			lab_pbd050 = (Label)target;
			break;
		case 516:
			btn_pbd050 = (Button)target;
			btn_pbd050.Click += Btn_SetPBDPattern_Click;
			break;
		case 517:
			img_pbd050 = (Image)target;
			img_pbd050.Drop += Img_pbd_Drop;
			img_pbd050.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd050.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 518:
			grd_PPD6 = (Grid)target;
			break;
		case 519:
			gd_pdb051 = (Grid)target;
			break;
		case 520:
			txt_pbd051 = (TextBox)target;
			break;
		case 521:
			img_pbd051 = (Image)target;
			img_pbd051.Drop += Img_pbd_Drop;
			img_pbd051.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd051.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 522:
			lab_pbd051 = (Label)target;
			break;
		case 523:
			btn_pbd051 = (Button)target;
			btn_pbd051.Click += Btn_SetPBDPattern_Click;
			break;
		case 524:
			gd_pdb052 = (Grid)target;
			break;
		case 525:
			txt_pbd052 = (TextBox)target;
			break;
		case 526:
			lab_pbd052 = (Label)target;
			break;
		case 527:
			btn_pbd052 = (Button)target;
			btn_pbd052.Click += Btn_SetPBDPattern_Click;
			break;
		case 528:
			img_pbd052 = (Image)target;
			img_pbd052.Drop += Img_pbd_Drop;
			img_pbd052.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd052.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 529:
			gd_pdb053 = (Grid)target;
			break;
		case 530:
			txt_pbd053 = (TextBox)target;
			break;
		case 531:
			lab_pbd053 = (Label)target;
			break;
		case 532:
			btn_pbd053 = (Button)target;
			btn_pbd053.Click += Btn_SetPBDPattern_Click;
			break;
		case 533:
			img_pbd053 = (Image)target;
			img_pbd053.Drop += Img_pbd_Drop;
			img_pbd053.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd053.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 534:
			gd_pdb054 = (Grid)target;
			break;
		case 535:
			txt_pbd054 = (TextBox)target;
			break;
		case 536:
			lab_pbd054 = (Label)target;
			break;
		case 537:
			btn_pbd054 = (Button)target;
			btn_pbd054.Click += Btn_SetPBDPattern_Click;
			break;
		case 538:
			img_pbd054 = (Image)target;
			img_pbd054.Drop += Img_pbd_Drop;
			img_pbd054.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd054.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 539:
			gd_pdb055 = (Grid)target;
			break;
		case 540:
			txt_pbd055 = (TextBox)target;
			break;
		case 541:
			lab_pbd055 = (Label)target;
			break;
		case 542:
			btn_pbd055 = (Button)target;
			btn_pbd055.Click += Btn_SetPBDPattern_Click;
			break;
		case 543:
			img_pbd055 = (Image)target;
			img_pbd055.Drop += Img_pbd_Drop;
			img_pbd055.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd055.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 544:
			gd_pdb056 = (Grid)target;
			break;
		case 545:
			txt_pbd056 = (TextBox)target;
			break;
		case 546:
			lab_pbd056 = (Label)target;
			break;
		case 547:
			btn_pbd056 = (Button)target;
			btn_pbd056.Click += Btn_SetPBDPattern_Click;
			break;
		case 548:
			img_pbd056 = (Image)target;
			img_pbd056.Drop += Img_pbd_Drop;
			img_pbd056.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd056.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 549:
			gd_pdb057 = (Grid)target;
			break;
		case 550:
			txt_pbd057 = (TextBox)target;
			break;
		case 551:
			lab_pbd057 = (Label)target;
			break;
		case 552:
			btn_pbd057 = (Button)target;
			btn_pbd057.Click += Btn_SetPBDPattern_Click;
			break;
		case 553:
			img_pbd057 = (Image)target;
			img_pbd057.Drop += Img_pbd_Drop;
			img_pbd057.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd057.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 554:
			gd_pdb058 = (Grid)target;
			break;
		case 555:
			txt_pbd058 = (TextBox)target;
			break;
		case 556:
			lab_pbd058 = (Label)target;
			break;
		case 557:
			btn_pbd058 = (Button)target;
			btn_pbd058.Click += Btn_SetPBDPattern_Click;
			break;
		case 558:
			img_pbd058 = (Image)target;
			img_pbd058.Drop += Img_pbd_Drop;
			img_pbd058.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd058.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 559:
			gd_pdb059 = (Grid)target;
			break;
		case 560:
			txt_pbd059 = (TextBox)target;
			break;
		case 561:
			lab_pbd059 = (Label)target;
			break;
		case 562:
			btn_pbd059 = (Button)target;
			btn_pbd059.Click += Btn_SetPBDPattern_Click;
			break;
		case 563:
			img_pbd059 = (Image)target;
			img_pbd059.Drop += Img_pbd_Drop;
			img_pbd059.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd059.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 564:
			gd_pdb060 = (Grid)target;
			break;
		case 565:
			txt_pbd060 = (TextBox)target;
			break;
		case 566:
			lab_pbd060 = (Label)target;
			break;
		case 567:
			btn_pbd060 = (Button)target;
			btn_pbd060.Click += Btn_SetPBDPattern_Click;
			break;
		case 568:
			img_pbd060 = (Image)target;
			img_pbd060.Drop += Img_pbd_Drop;
			img_pbd060.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd060.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 569:
			grd_PPD7 = (Grid)target;
			break;
		case 570:
			gd_pdb061 = (Grid)target;
			break;
		case 571:
			txt_pbd061 = (TextBox)target;
			break;
		case 572:
			img_pbd061 = (Image)target;
			img_pbd061.Drop += Img_pbd_Drop;
			img_pbd061.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd061.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 573:
			lab_pbd061 = (Label)target;
			break;
		case 574:
			btn_pbd061 = (Button)target;
			btn_pbd061.Click += Btn_SetPBDPattern_Click;
			break;
		case 575:
			gd_pdb062 = (Grid)target;
			break;
		case 576:
			txt_pbd062 = (TextBox)target;
			break;
		case 577:
			lab_pbd062 = (Label)target;
			break;
		case 578:
			btn_pbd062 = (Button)target;
			btn_pbd062.Click += Btn_SetPBDPattern_Click;
			break;
		case 579:
			img_pbd062 = (Image)target;
			img_pbd062.Drop += Img_pbd_Drop;
			img_pbd062.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd062.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 580:
			gd_pdb063 = (Grid)target;
			break;
		case 581:
			txt_pbd063 = (TextBox)target;
			break;
		case 582:
			lab_pbd063 = (Label)target;
			break;
		case 583:
			btn_pbd063 = (Button)target;
			btn_pbd063.Click += Btn_SetPBDPattern_Click;
			break;
		case 584:
			img_pbd063 = (Image)target;
			img_pbd063.Drop += Img_pbd_Drop;
			img_pbd063.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd063.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 585:
			gd_pdb064 = (Grid)target;
			break;
		case 586:
			txt_pbd064 = (TextBox)target;
			break;
		case 587:
			lab_pbd064 = (Label)target;
			break;
		case 588:
			btn_pbd064 = (Button)target;
			btn_pbd064.Click += Btn_SetPBDPattern_Click;
			break;
		case 589:
			img_pbd064 = (Image)target;
			img_pbd064.Drop += Img_pbd_Drop;
			img_pbd064.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd064.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 590:
			gd_pdb065 = (Grid)target;
			break;
		case 591:
			txt_pbd065 = (TextBox)target;
			break;
		case 592:
			lab_pbd065 = (Label)target;
			break;
		case 593:
			btn_pbd065 = (Button)target;
			btn_pbd065.Click += Btn_SetPBDPattern_Click;
			break;
		case 594:
			img_pbd065 = (Image)target;
			img_pbd065.Drop += Img_pbd_Drop;
			img_pbd065.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd065.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 595:
			gd_pdb066 = (Grid)target;
			break;
		case 596:
			txt_pbd066 = (TextBox)target;
			break;
		case 597:
			lab_pbd066 = (Label)target;
			break;
		case 598:
			btn_pbd066 = (Button)target;
			btn_pbd066.Click += Btn_SetPBDPattern_Click;
			break;
		case 599:
			img_pbd066 = (Image)target;
			img_pbd066.Drop += Img_pbd_Drop;
			img_pbd066.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd066.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 600:
			gd_pdb067 = (Grid)target;
			break;
		case 601:
			txt_pbd067 = (TextBox)target;
			break;
		case 602:
			lab_pbd067 = (Label)target;
			break;
		case 603:
			btn_pbd067 = (Button)target;
			btn_pbd067.Click += Btn_SetPBDPattern_Click;
			break;
		case 604:
			img_pbd067 = (Image)target;
			img_pbd067.Drop += Img_pbd_Drop;
			img_pbd067.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd067.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 605:
			gd_pdb068 = (Grid)target;
			break;
		case 606:
			txt_pbd068 = (TextBox)target;
			break;
		case 607:
			lab_pbd068 = (Label)target;
			break;
		case 608:
			btn_pbd068 = (Button)target;
			btn_pbd068.Click += Btn_SetPBDPattern_Click;
			break;
		case 609:
			img_pbd068 = (Image)target;
			img_pbd068.Drop += Img_pbd_Drop;
			img_pbd068.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd068.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 610:
			gd_pdb069 = (Grid)target;
			break;
		case 611:
			txt_pbd069 = (TextBox)target;
			break;
		case 612:
			lab_pbd069 = (Label)target;
			break;
		case 613:
			btn_pbd069 = (Button)target;
			btn_pbd069.Click += Btn_SetPBDPattern_Click;
			break;
		case 614:
			img_pbd069 = (Image)target;
			img_pbd069.Drop += Img_pbd_Drop;
			img_pbd069.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd069.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 615:
			gd_pdb070 = (Grid)target;
			break;
		case 616:
			txt_pbd070 = (TextBox)target;
			break;
		case 617:
			lab_pbd070 = (Label)target;
			break;
		case 618:
			btn_pbd070 = (Button)target;
			btn_pbd070.Click += Btn_SetPBDPattern_Click;
			break;
		case 619:
			img_pbd070 = (Image)target;
			img_pbd070.Drop += Img_pbd_Drop;
			img_pbd070.PreviewMouseRightButtonDown += Img_pbd_PreviewMouseRightButtonDown;
			img_pbd070.PreviewMouseMove += Img_pbd_PreviewMouseMove;
			break;
		case 620:
			btn_ImgSrc = (Button)target;
			btn_ImgSrc.Click += Btn_ImgSrc_Click;
			break;
		case 621:
			btn_ImgR = (Button)target;
			btn_ImgR.Click += Btn_ImgSrc_Click;
			break;
		case 622:
			btn_ImgG = (Button)target;
			btn_ImgG.Click += Btn_ImgSrc_Click;
			break;
		case 623:
			btn_ImgB = (Button)target;
			btn_ImgB.Click += Btn_ImgSrc_Click;
			break;
		case 624:
			btn_ImgH = (Button)target;
			btn_ImgH.Click += Btn_ImgSrc_Click;
			break;
		case 625:
			btn_ImgS = (Button)target;
			btn_ImgS.Click += Btn_ImgSrc_Click;
			break;
		case 626:
			btn_ImgV = (Button)target;
			btn_ImgV.Click += Btn_ImgSrc_Click;
			break;
		case 627:
			btn_ImgGray = (Button)target;
			btn_ImgGray.Click += Btn_ImgSrc_Click;
			break;
		case 628:
			btn_ImgS_Copy = (Button)target;
			btn_ImgS_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 629:
			btn_ImgV_Copy = (Button)target;
			btn_ImgV_Copy.Click += Btn_ImgSrc_Click;
			break;
		case 630:
			btn_ImageNum = (Button)target;
			break;
		case 631:
			btn_ImgC1 = (Button)target;
			btn_ImgC1.Click += Btn_ImgSrc_Click;
			break;
		case 632:
			btn_ImgC2 = (Button)target;
			btn_ImgC2.Click += Btn_ImgSrc_Click;
			break;
		case 633:
			btn_ImgC3 = (Button)target;
			btn_ImgC3.Click += Btn_ImgSrc_Click;
			break;
		case 634:
			btn_ImgAI = (Button)target;
			btn_ImgAI.Click += Btn_ImgSrc_Click;
			break;
		case 635:
			lab_RValue = (Label)target;
			break;
		case 636:
			lab_GValue = (Label)target;
			break;
		case 637:
			lab_BValue = (Label)target;
			break;
		case 638:
			lab_HValue = (Label)target;
			break;
		case 639:
			lab_SValue = (Label)target;
			break;
		case 640:
			lab_VValue = (Label)target;
			break;
		case 641:
			lab_GrayValue = (Label)target;
			break;
		case 642:
			lab_XValue = (Label)target;
			break;
		case 643:
			lab_YValue = (Label)target;
			break;
		case 644:
			lab_C1Value = (Label)target;
			break;
		case 645:
			lab_C2Value = (Label)target;
			break;
		case 646:
			lab_C3Value = (Label)target;
			break;
		case 647:
			txt_AllResult = (TextBox)target;
			break;
		case 648:
			btn_TrackerShow = (Button)target;
			btn_TrackerShow.Click += Btn_TrackerShow_Click;
			break;
		case 649:
			btn_TestNextImg = (Button)target;
			btn_TestNextImg.Click += Btn_TestNextImg_Click;
			break;
		case 650:
			btn_TestPrevImg = (Button)target;
			btn_TestPrevImg.Click += Btn_TestPrevImg_Click;
			break;
		case 651:
			btn_Tools1 = (Button)target;
			btn_Tools1.Click += Btn_Tools1_Click;
			break;
		case 652:
			btn_Tools2 = (Button)target;
			btn_Tools2.Click += Btn_Tools1_Click;
			break;
		case 653:
			btn_Tools3 = (Button)target;
			btn_Tools3.Click += Btn_Tools1_Click;
			break;
		case 654:
			btn_Tools4 = (Button)target;
			btn_Tools4.Click += Btn_Tools1_Click;
			break;
		case 655:
			btn_Tools5 = (Button)target;
			btn_Tools5.Click += Btn_Tools1_Click;
			break;
		case 656:
			btn_Tools6 = (Button)target;
			btn_Tools6.Click += Btn_Tools1_Click;
			break;
		case 657:
			btn_Tools7 = (Button)target;
			btn_Tools7.Click += Btn_Tools1_Click;
			break;
		case 658:
			btn_Tools8 = (Button)target;
			btn_Tools8.Click += Btn_Tools1_Click;
			break;
		case 659:
			btn_Tools9 = (Button)target;
			btn_Tools9.Click += Btn_Tools1_Click;
			break;
		case 660:
			btn_Tools10 = (Button)target;
			btn_Tools10.Click += Btn_Tools1_Click;
			break;
		case 661:
			btn_Tools11 = (Button)target;
			btn_Tools11.Click += Btn_Tools1_Click;
			break;
		case 662:
			btn_Tools12 = (Button)target;
			btn_Tools12.Click += Btn_Tools1_Click;
			break;
		case 663:
			btn_Tools13 = (Button)target;
			btn_Tools13.Click += Btn_Tools1_Click;
			break;
		case 664:
			btn_Tools14 = (Button)target;
			btn_Tools14.Click += Btn_Tools1_Click;
			break;
		case 665:
			btn_Tools15 = (Button)target;
			btn_Tools15.Click += Btn_Tools1_Click;
			break;
		case 666:
			btn_Tools16 = (Button)target;
			btn_Tools16.Click += Btn_Tools1_Click;
			break;
		case 667:
			btn_Tools17 = (Button)target;
			btn_Tools17.Click += Btn_Tools1_Click;
			break;
		case 668:
			btn_Tools18 = (Button)target;
			btn_Tools18.Click += Btn_Tools1_Click;
			break;
		case 669:
			btn_Tools19 = (Button)target;
			btn_Tools19.Click += Btn_Tools1_Click;
			break;
		case 670:
			btn_Tools20 = (Button)target;
			btn_Tools20.Click += Btn_Tools1_Click;
			break;
		case 671:
			btn_ReferenceShow = (Button)target;
			btn_ReferenceShow.Click += Btn_ReferenceShow_Click;
			break;
		case 672:
			btn_PreviewSwitch_Copy = (Button)target;
			btn_PreviewSwitch_Copy.Click += Btn_PreviewSwitch_Click;
			break;
		case 673:
			Label_Index = (Label)target;
			break;
		case 674:
			btn_AlignShow = (Button)target;
			btn_AlignShow.Click += Btn_AlignShow_Click;
			break;
		case 675:
			grd_Align = (Grid)target;
			break;
		case 676:
			cb_Track1AlignWayX = (ComboBox)target;
			cb_Track1AlignWayX.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 677:
			rad_Tracker1AutoSize = (RadioButton)target;
			rad_Tracker1AutoSize.Click += Rad_Tracker1AutoSize_Click;
			break;
		case 678:
			cb_Track1AutoSize = (ComboBox)target;
			cb_Track1AutoSize.SelectionChanged += Cb_Track1AutoSize_SelectionChanged;
			break;
		case 679:
			rad_Tracker1AlignSelect = (RadioButton)target;
			rad_Tracker1AlignSelect.Click += Rad_Tracker1AlignSelect_Click;
			break;
		case 680:
			btn_TrackerReset = (Button)target;
			btn_TrackerReset.Click += Btn_TrackerReset_Click;
			break;
		case 681:
			cb_Track1AlignWayY = (ComboBox)target;
			cb_Track1AlignWayY.SelectionChanged += Cb_Track1AlignWayX_SelectionChanged;
			break;
		case 682:
			btn_Tracker1SizeChange = (Button)target;
			btn_Tracker1SizeChange.Click += Btn_Tracker1SizeChange_Click;
			break;
		case 683:
			txt_Tracker1Pos = (Label)target;
			break;
		case 684:
			lab_LockMode = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
