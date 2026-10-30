using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using IniParser;
using IniParser.Model;
using InspTools.Other;
using OpenCvSharp;

namespace InspTools.AOI;

public class ParamFile
{
	private const int MaxTracker = 20;

	private const int MaxMeasureTracker = 40;

	private const int MaxInhance = 6;

	private const int MaxFace = 4;

	private const int MaxRings = 4;

	private const int MaxOtherFont = 20;

	private const int MaxQuickSetupItem = 50;

	private const int MaxPublicPattern = 210;

	private const int MaxPublicMatchParam = 50;

	public double PixelPerUM_X;

	public double PixelPerUM_Y;

	public double ObjSizeUM_W;

	public double ObjSizeUM_H;

	public bool UseFloat;

	public int MeasureUnit;

	public bool StationON;

	public string OKPath;

	public string NGPath;

	public int LanguageType = 1;

	public string AIRoot_Path;

	public string AIOK_Path_A;

	public string AING_Path_A;

	public string OK_Path_A;

	public string NG_Path_A;

	public string AI_X_Path_A;

	public string AI_Y_Path_A;

	public string AI_X_Path_OK;

	public string AI_Y_Path_OK;

	public string AI_X_Path_NG;

	public string AI_Y_Path_NG;

	public string AIOK_Path_B;

	public string AING_Path_B;

	public string OK_Path_B;

	public string NG_Path_B;

	public bool FontOCRUseCuda;

	public bool UseUNET;

	public int ColorSelect;

	public int PreviewMode;

	public int ZoomMode;

	public int PenSize;

	public bool UseExpandArea;

	public int ExpandValue;

	public bool EnableJudgeWithAdvMeasure;

	public int JudgeWithAdvMeasureIndexA;

	public int JudgeWithAdvMeasureIndexB;

	public bool JudgeWithAdvMeasureResult;

	public bool EnableContinueWrite;

	public bool EnableAutoIgnoreDraw;

	public int AutoIgnoreSize;

	public bool SaveAsFolder;

	public string[] DrawCaition = new string[6];

	public bool SoftCorrectionON;

	public int SoftCorrSetup_R;

	public int SoftCorrSetup_G;

	public int SoftCorrSetup_B;

	public int SoftCorrection_R = -1;

	public int SoftCorrection_G = -1;

	public int SoftCorrection_B = -1;

	public int SoftShift_R;

	public int SoftShift_G;

	public int SoftShift_B;

	public int SoftCorrectoinAllowDiff;

	public bool SaveAIFontImage;

	public bool SaveAIFontImage2;

	public int AuthLevel;

	public bool AllowEnableAILocate;

	public bool AllowEnableAIProphet;

	public INSP_LOCATE_PARAM LocateParam;

	public bool LocateToolON;

	public INSP_SELECTWAYTOOL_PARAM SelectWayParam;

	public bool SelectWayON;

	public INSP_SELECTFACETOOL_PARAM[] SelectFaceParam = new INSP_SELECTFACETOOL_PARAM[4];

	public bool SelectFaceON;

	public INSP_LIGHTMONITOR_PARAM LightMonitorParam;

	public INSP_ENHANCE_PARAM[] EnhanceParam = new INSP_ENHANCE_PARAM[6];

	public bool EnahnceToolON;

	public INSP_COLORREFERENCE_PARAM[] ColorReferenceParam = new INSP_COLORREFERENCE_PARAM[20];

	public INSP_BINARYTOOL_PARAM[] BinaryParam = new INSP_BINARYTOOL_PARAM[30];

	public bool BinaryToolON;

	public INSP_IRTOOL_PARAM IRToolParam;

	public bool IRToolON;

	public INSP_SCANLINETOOL_PARAM[] HSLocateParam = new INSP_SCANLINETOOL_PARAM[20];

	public INSP_SCANLINETOOL_PARAM[] VSLocateParam = new INSP_SCANLINETOOL_PARAM[20];

	public INSP_ADVLOCATETOOL_PARAM[] AdvLocateParam = new INSP_ADVLOCATETOOL_PARAM[20];

	public INSP_ADVMEASURETOOL_PARAM[] AdvLocateMParam = new INSP_ADVMEASURETOOL_PARAM[40];

	public bool AdvLocateToolON;

	public INSP_MEASURETOOL_PARAM[] MeaWidthParam = new INSP_MEASURETOOL_PARAM[20];

	public INSP_MEASURETOOL_PARAM[] MeaHeightParam = new INSP_MEASURETOOL_PARAM[20];

	public bool MeasureToolON;

	public INSP_SMOOTHTOOL_PARAM[] SmoothParam = new INSP_SMOOTHTOOL_PARAM[20];

	public bool SmoothToolON;

	public INSP_DIFFERENCETOOL_PARAM[] DifferenceParam = new INSP_DIFFERENCETOOL_PARAM[20];

	public bool DifferenceToolON;

	public INSP_DIFFERENCETOOL_PARAM[] BlockAvgParam = new INSP_DIFFERENCETOOL_PARAM[20];

	public bool BlockAvgToolON;

	public INSP_LINEANALYSISTOOL_PARAM[] LineAnalysisParam = new INSP_LINEANALYSISTOOL_PARAM[20];

	public bool LineAnalysisToolON;

	public INSP_MASKTOOL_PARAM[] MaskParam = new INSP_MASKTOOL_PARAM[30];

	public bool MaskToolON;

	public INSP_MATCHTOOL_PARAM[] MatchParam = new INSP_MATCHTOOL_PARAM[20];

	public bool MatchToolON;

	public INSP_FONTTOOL_PARAM FontToolParam;

	public bool FontToolON;

	public INSP_MATCHTOOL_PARAM[] MatchQuickSetupParam;

	public MatchPattern[] patternDB = new MatchPattern[20];

	public MatchPattern[] PublicPatternDB;

	public MatchPattern[] PrivatePatternDB;

	public INSP_AIPROPHET_PARAM AIProphetParam;

	public bool AIProphetToolON;

	public INSP_AITOOL_PARAM AIEnhanceParam;

	public bool AIEnhanceToolON;

	public INSP_AITOOL_PARAM AIPredictParam;

	public bool AIPredictToolON;

	public double[] Comdigits = new double[24];

	private List<TOOLSENABLEITEM> StationToolEnableItem = new List<TOOLSENABLEITEM>();

	public ThroughputParamFile ThroughputFile;

	private HiniFile ini;

	private FileIniDataParser parser = new FileIniDataParser();

	private IniData iData;

	private string iDataFile = "";

	private int ThreadID;

	private string RecipeName = "";

	private int StationIndex;

	public ParamFile()
	{
		StationON = true;
		LocateToolON = true;
		SelectWayON = false;
		SelectFaceON = false;
		BinaryToolON = true;
		AdvLocateToolON = true;
		MeasureToolON = true;
		SmoothToolON = true;
		DifferenceToolON = true;
		BlockAvgToolON = true;
		LineAnalysisToolON = true;
		MatchToolON = false;
		AIProphetToolON = false;
		AIEnhanceToolON = false;
		AIPredictToolON = false;
		FontToolON = false;
		IRToolON = false;
		ini = null;
		for (int i = 0; i < 20; i++)
		{
			patternDB[i] = new MatchPattern();
		}
		FontToolParam.OtherFont = new INSP_FONTTOOL_OTHERFONT_PARAM[20];
		for (int j = 0; j < 20; j++)
		{
			FontToolParam.OtherFont[j] = default(INSP_FONTTOOL_OTHERFONT_PARAM);
		}
		IRToolParam.ColorRings = new INSP_BINARYTOOL_PARAM[4];
		for (int k = 0; k < 4; k++)
		{
			IRToolParam.ColorRings[k] = default(INSP_BINARYTOOL_PARAM);
		}
	}

	public void SetINI(HiniFile Ini)
	{
		ini = Ini;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CheckRecipeDeleteDuplicateSection(string _filePath, Encoding encoding)
	{
		string[] array = File.ReadAllLines(_filePath, encoding);
		List<string> list = new List<string>();
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (string.IsNullOrEmpty(text) || text.StartsWith(";") || !text.StartsWith("["))
			{
				continue;
			}
			string item = text.TrimStart('[').TrimEnd(']');
			if (list.Contains(item))
			{
				array[i] = string.Empty;
				for (int j = i + 1; j < array.Length && !array[j].StartsWith("["); j++)
				{
					array[j] = string.Empty;
					flag = true;
				}
			}
			else
			{
				list.Add(item);
			}
		}
		if (flag)
		{
			File.WriteAllLines(_filePath, array, encoding);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetIniFile(string inifile, int index, Encoding encode)
	{
		try
		{
			iData = parser.ReadFile(inifile, encode);
			iDataFile = inifile;
			if (iData.ToString().Length > 10)
			{
				ini.SetiData(iData);
			}
			else
			{
				iData = null;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ParamFile-SetRecipe()-01" + ex.ToString());
			MessageBox.Show(string.Format("第{0}站Recipe分析錯誤(iData = parser.ReadFile)，請檢查Recipe檔案。 {1}", index + 1, inifile), "嚴重錯誤");
			iData = null;
			ini.ReSetiData();
		}
	}

	public void SetRecipeName(string recipeName)
	{
		RecipeName = recipeName;
	}

	public string GetRecipeName()
	{
		return RecipeName;
	}

	public void SetStationIndex(int stationIndex)
	{
		StationIndex = stationIndex;
	}

	public int GetStationIndex()
	{
		return StationIndex;
	}

	public void SetThreadID(int threadid)
	{
		ThreadID = threadid;
	}

	public int GetThreadID()
	{
		return ThreadID;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadAllParam()
	{
		if (ini == null)
		{
			return false;
		}
		ReadMainParam();
		ReadLocateParam();
		ReadLightMonitorParam();
		ReadSelectWayParam();
		ReadAIProphetParam();
		ReadAIEnhanceParam();
		ReadAIPredictParam();
		ReadFontToolParam();
		ReadIRToolParam();
		for (int i = 0; i < 6; i++)
		{
			ReadEnhanceParam(i);
		}
		for (int j = 0; j < 4; j++)
		{
			ReadSelectFaceParam(j);
		}
		for (int k = 0; k < 20; k++)
		{
			ReadAdvLocaeParam(k);
			ReadMeasureParam(k);
			ReadColorReferenceParam(k);
			ReadSmoothParam(k);
			ReadDifferenceParam(k);
			ReadBlockAvgParam(k);
			ReadLineAnalysisParam(k);
			ReadMatchParam(k);
			ReadPattern(k);
		}
		for (int l = 0; l < 40; l++)
		{
			ReadAdvMeasureParam(l);
		}
		for (int m = 0; m < 30; m++)
		{
			ReadMaskParam(m);
			ReadBinaryParam(ref BinaryParam[m], m, "BinaryTools");
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteAllParam()
	{
		if (ini == null)
		{
			return false;
		}
		try
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			WriteMainParam();
			WriteLocateParam();
			WriteLightMonitorParam();
			WriteSelectWayParam();
			WriteAIProphetParam();
			WriteAIEnhanceParam();
			WriteAIPredictParam();
			WriteFontToolParam();
			WriteIRToolParam();
			for (int i = 0; i < 6; i++)
			{
				WriteEnhanceParam(i);
			}
			for (int j = 0; j < 4; j++)
			{
				WriteSelectFaceParam(j);
			}
			for (int k = 0; k < 20; k++)
			{
				WriteAdvLocaeParam(k);
				WriteMeasureParam(k);
				WriteColorReferenceParam(k);
				WriteSmoothParam(k);
				WriteDifferenceParam(k);
				WriteBlockAvgParam(k);
				WriteLineAnalysisParam(k);
				WriteMatchParam(k);
				WritePattern(k);
			}
			for (int l = 0; l < 40; l++)
			{
				WriteAdvMeasureParam(l);
			}
			for (int m = 0; m < 30; m++)
			{
				WriteBinaryParam(BinaryParam[m], m, "BinaryTools");
				WriteMaskParam(m);
			}
			if (iData != null)
			{
				if (iData.ToString().Length > 10)
				{
					parser.WriteFile(iDataFile, iData, Encoding.Unicode);
				}
				else
				{
					ini.ReSetiData();
					OldWayWriteAllParam();
				}
			}
			else
			{
				OldWayWriteAllParam();
			}
			stopwatch.Stop();
			_ = stopwatch.Elapsed.TotalMilliseconds;
			stopwatch.Reset();
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ParamFile-WriteAllParam()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool OldWayWriteAllParam()
	{
		if (ini == null)
		{
			return false;
		}
		try
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			WriteMainParam();
			WriteLocateParam();
			WriteLightMonitorParam();
			WriteSelectWayParam();
			WriteAIProphetParam();
			WriteAIEnhanceParam();
			WriteAIPredictParam();
			WriteFontToolParam();
			WriteIRToolParam();
			for (int i = 0; i < 6; i++)
			{
				WriteEnhanceParam(i);
			}
			for (int j = 0; j < 4; j++)
			{
				WriteSelectFaceParam(j);
			}
			for (int k = 0; k < 20; k++)
			{
				WriteAdvLocaeParam(k);
				WriteMeasureParam(k);
				WriteColorReferenceParam(k);
				WriteSmoothParam(k);
				WriteDifferenceParam(k);
				WriteBlockAvgParam(k);
				WriteLineAnalysisParam(k);
				WriteMatchParam(k);
				WritePattern(k);
			}
			for (int l = 0; l < 40; l++)
			{
				WriteAdvMeasureParam(l);
			}
			for (int m = 0; m < 30; m++)
			{
				WriteBinaryParam(BinaryParam[m], m, "BinaryTools");
				WriteMaskParam(m);
			}
			stopwatch.Stop();
			_ = stopwatch.Elapsed.TotalMilliseconds;
			stopwatch.Reset();
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ParamFile-WriteAllParam()" + ex.ToString());
			return false;
		}
	}

	public void CopyParam(ref ParamFile param)
	{
		param.LocateParam = LocateParam;
		param.LightMonitorParam = LightMonitorParam;
		param.SelectWayParam = SelectWayParam;
		param.AIProphetParam = AIProphetParam;
		param.AIEnhanceParam = AIEnhanceParam;
		param.AIPredictParam = AIPredictParam;
		param.FontToolParam = FontToolParam;
		param.IRToolParam = IRToolParam;
		for (int i = 0; i < 6; i++)
		{
			param.EnhanceParam[i] = EnhanceParam[i];
		}
		for (int j = 0; j < 4; j++)
		{
			param.SelectFaceParam[j] = SelectFaceParam[j];
		}
		for (int k = 0; k < 20; k++)
		{
			param.HSLocateParam = HSLocateParam;
			param.VSLocateParam = VSLocateParam;
			param.AdvLocateParam[k] = AdvLocateParam[k];
			param.MeaHeightParam[k] = MeaHeightParam[k];
			param.MeaWidthParam[k] = MeaWidthParam[k];
			param.ColorReferenceParam[k] = ColorReferenceParam[k];
			param.SmoothParam[k] = SmoothParam[k];
			param.DifferenceParam[k] = DifferenceParam[k];
			param.BlockAvgParam[k] = BlockAvgParam[k];
			param.LineAnalysisParam[k] = LineAnalysisParam[k];
			param.MatchParam[k] = MatchParam[k];
			param.patternDB[k] = patternDB[k];
		}
		if (MatchQuickSetupParam != null)
		{
			param.MatchQuickSetupParam = MatchQuickSetupParam;
		}
		for (int l = 0; l < 40; l++)
		{
			param.AdvLocateMParam[l] = AdvLocateMParam[l];
		}
		for (int m = 0; m < 30; m++)
		{
			param.BinaryParam[m] = BinaryParam[m];
			param.MaskParam[m] = MaskParam[m];
		}
		for (int n = 0; n < 4; n++)
		{
			param.IRToolParam.ColorRings[n] = IRToolParam.ColorRings[n];
		}
		param.StationON = StationON;
		param.LocateToolON = LocateToolON;
		param.SelectWayON = SelectWayON;
		param.SelectFaceON = SelectFaceON;
		param.BinaryToolON = BinaryToolON;
		param.AdvLocateToolON = AdvLocateToolON;
		param.MeasureToolON = MeasureToolON;
		param.SmoothToolON = SmoothToolON;
		param.DifferenceToolON = DifferenceToolON;
		param.BlockAvgToolON = BlockAvgToolON;
		param.LineAnalysisToolON = LineAnalysisToolON;
		param.MatchToolON = MatchToolON;
		param.AIProphetToolON = AIProphetToolON;
		param.AIEnhanceToolON = AIEnhanceToolON;
		param.AIPredictToolON = AIPredictToolON;
		param.FontToolON = FontToolON;
		param.IRToolON = IRToolON;
		param.PixelPerUM_X = PixelPerUM_X;
		param.PixelPerUM_Y = PixelPerUM_Y;
		param.ObjSizeUM_W = ObjSizeUM_W;
		param.ObjSizeUM_H = ObjSizeUM_H;
		param.UseFloat = UseFloat;
		param.MeasureUnit = MeasureUnit;
		param.StationON = StationON;
		param.OKPath = OKPath;
		param.NGPath = NGPath;
		param.AIRoot_Path = AIRoot_Path;
		param.AIOK_Path_A = AIOK_Path_A;
		param.AING_Path_A = AING_Path_A;
		param.OK_Path_A = OK_Path_A;
		param.NG_Path_A = NG_Path_A;
		param.AI_X_Path_A = AI_X_Path_A;
		param.AI_Y_Path_A = AI_Y_Path_A;
		param.AI_X_Path_OK = AI_X_Path_OK;
		param.AI_Y_Path_OK = AI_Y_Path_OK;
		param.AI_X_Path_NG = AI_X_Path_NG;
		param.AI_Y_Path_NG = AI_Y_Path_NG;
		param.AIOK_Path_B = AIOK_Path_B;
		param.AING_Path_B = AING_Path_B;
		param.OK_Path_B = OK_Path_B;
		param.NG_Path_B = NG_Path_B;
		param.UseUNET = UseUNET;
		param.ColorSelect = ColorSelect;
		param.PreviewMode = PreviewMode;
		param.ZoomMode = ZoomMode;
		param.PenSize = PenSize;
		param.UseExpandArea = UseExpandArea;
		param.ExpandValue = ExpandValue;
		param.EnableJudgeWithAdvMeasure = EnableJudgeWithAdvMeasure;
		param.JudgeWithAdvMeasureIndexA = JudgeWithAdvMeasureIndexA;
		param.JudgeWithAdvMeasureIndexB = JudgeWithAdvMeasureIndexB;
		param.JudgeWithAdvMeasureResult = JudgeWithAdvMeasureResult;
		param.EnableContinueWrite = EnableContinueWrite;
		param.EnableAutoIgnoreDraw = EnableAutoIgnoreDraw;
		param.AutoIgnoreSize = AutoIgnoreSize;
		param.SoftCorrectionON = SoftCorrectionON;
		param.SoftCorrSetup_R = SoftCorrSetup_R;
		param.SoftCorrSetup_G = SoftCorrSetup_G;
		param.SoftCorrSetup_B = SoftCorrSetup_B;
		param.SoftCorrection_R = SoftCorrection_R;
		param.SoftCorrection_G = SoftCorrection_G;
		param.SoftCorrection_B = SoftCorrection_B;
		param.SoftShift_R = SoftShift_R;
		param.SoftShift_G = SoftShift_G;
		param.SoftShift_B = SoftShift_B;
		param.SoftCorrectoinAllowDiff = SoftCorrectoinAllowDiff;
		param.SaveAIFontImage = false;
		param.SaveAIFontImage2 = false;
	}

	public void SetPublicPatternDB(MatchPattern[] db, INSP_MATCHTOOL_PARAM[] param)
	{
		PublicPatternDB = db;
		MatchQuickSetupParam = param;
	}

	public void SetPrivatePatternDB(MatchPattern[] db)
	{
		PrivatePatternDB = db;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ReadMainParam()
	{
		StationON = ini.GetBoolean("Main", "StationON", defaultValue: true);
		LocateToolON = ini.GetBoolean("Main", "LocateToolON", defaultValue: true);
		EnahnceToolON = ini.GetBoolean("Main", "EnahnceToolON");
		SelectWayON = ini.GetBoolean("Main", "SelectWayON");
		SelectFaceON = ini.GetBoolean("Main", "SelectFaceON");
		MaskToolON = ini.GetBoolean("Main", "MaskToolON", defaultValue: true);
		BinaryToolON = ini.GetBoolean("Main", "BinaryToolON", defaultValue: true);
		AdvLocateToolON = ini.GetBoolean("Main", "AdvLocateToolON", defaultValue: true);
		MeasureToolON = ini.GetBoolean("Main", "MeasureToolON");
		SmoothToolON = ini.GetBoolean("Main", "SmoothToolON");
		DifferenceToolON = ini.GetBoolean("Main", "DifferenceToolON");
		BlockAvgToolON = ini.GetBoolean("Main", "BlockAvgToolON");
		LineAnalysisToolON = ini.GetBoolean("Main", "LineAnalysisToolON");
		MatchToolON = ini.GetBoolean("Main", "MatchToolON");
		FontToolON = ini.GetBoolean("Main", "FontToolON");
		IRToolON = ini.GetBoolean("Main", "IRToolON");
		AIProphetToolON = ini.GetBoolean("Main", "AIProphetToolON");
		AIEnhanceToolON = ini.GetBoolean("Main", "AIEnhanceToolON");
		AIPredictToolON = ini.GetBoolean("Main", "AIPredictToolON");
		PixelPerUM_X = ini.GetDouble("Main", "PixelPerUM_X", 1.0);
		PixelPerUM_Y = ini.GetDouble("Main", "PixelPerUM_Y", 1.0);
		ObjSizeUM_W = ini.GetDouble("Main", "ObjSizeUM_W", 500.0);
		ObjSizeUM_H = ini.GetDouble("Main", "ObjSizeUM_H", 500.0);
		UseFloat = ini.GetBoolean("Main", "UseFloat", defaultValue: true);
		MeasureUnit = ini.GetInteger("Main", "MeasureUnit");
		LanguageType = ini.GetInteger("Main", "LanguageType", 1);
		OKPath = ini.GetValue("Main", "OKPath");
		NGPath = ini.GetValue("Main", "NGPath");
		AIRoot_Path = ini.GetValue("Main", "AIRoot_Path");
		AIOK_Path_A = ini.GetValue("Main", "AIOK_Path_A");
		AING_Path_A = ini.GetValue("Main", "AING_Path_A");
		OK_Path_A = ini.GetValue("Main", "OK_Path_A");
		NG_Path_A = ini.GetValue("Main", "NG_Path_A");
		AI_X_Path_A = ini.GetValue("Main", "AI_X_Path_A");
		AI_Y_Path_A = ini.GetValue("Main", "AI_Y_Path_A");
		AI_X_Path_OK = ini.GetValue("Main", "AI_X_Path_OK");
		AI_Y_Path_OK = ini.GetValue("Main", "AI_Y_Path_OK");
		AIOK_Path_B = ini.GetValue("Main", "AIOK_Path_B");
		AING_Path_B = ini.GetValue("Main", "AING_Path_B");
		OK_Path_B = ini.GetValue("Main", "OK_Path_B");
		NG_Path_B = ini.GetValue("Main", "NG_Path_B");
		AI_X_Path_NG = ini.GetValue("Main", "AI_X_Path_NG");
		AI_Y_Path_NG = ini.GetValue("Main", "AI_Y_Path_NG");
		FontOCRUseCuda = ini.GetBoolean("Main", "FontOCRUseCuda");
		UseUNET = ini.GetBoolean("Main", "UseUNET", defaultValue: true);
		ColorSelect = ini.GetInteger("Main", "ColorSelect");
		PreviewMode = ini.GetInteger("Main", "PreviewMode");
		ZoomMode = ini.GetInteger("Main", "ZoomMode");
		PenSize = ini.GetInteger("Main", "PenSize", 12);
		UseExpandArea = ini.GetBoolean("Main", "UseExpandArea", defaultValue: true);
		SaveAsFolder = ini.GetBoolean("Main", "SaveAsFolder");
		AutoIgnoreSize = ini.GetInteger("Main", "AutoIgnoreSize", 3);
		ExpandValue = ini.GetInteger("Main", "ExpandValue", 10);
		EnableContinueWrite = ini.GetBoolean("Main", "EnableContinueWrite");
		EnableAutoIgnoreDraw = ini.GetBoolean("Main", "EnableAutoIgnoreDraw", defaultValue: true);
		DrawCaition[0] = ini.GetValue("Main", "DrawCaition1", "Note");
		DrawCaition[1] = ini.GetValue("Main", "DrawCaition2", "Note");
		DrawCaition[2] = ini.GetValue("Main", "DrawCaition3", "Note");
		DrawCaition[3] = ini.GetValue("Main", "DrawCaition4");
		DrawCaition[4] = ini.GetValue("Main", "DrawCaition5");
		DrawCaition[5] = ini.GetValue("Main", "DrawCaition6");
		SoftCorrSetup_R = ini.GetInteger("Main", "SoftCorrSetup_R", -1);
		SoftCorrSetup_G = ini.GetInteger("Main", "SoftCorrSetup_G", -1);
		SoftCorrSetup_B = ini.GetInteger("Main", "SoftCorrSetup_B", -1);
		SoftCorrectoinAllowDiff = ini.GetInteger("Main", "SoftCorrectoinAllowDiff", 10);
		SoftCorrection_R = ini.GetInteger("Main", "SoftCorrection_R", -1);
		SoftCorrection_G = ini.GetInteger("Main", "SoftCorrection_G", -1);
		SoftCorrection_B = ini.GetInteger("Main", "SoftCorrection_B", -1);
		JudgeWithAdvMeasureIndexA = ini.GetInteger("Main", "JudgeWithAdvMeasureIndexA");
		JudgeWithAdvMeasureIndexB = ini.GetInteger("Main", "JudgeWithAdvMeasureIndexB");
		EnableJudgeWithAdvMeasure = ini.GetBoolean("Main", "EnableJudgeWithAdvMeasure");
		SaveAIFontImage = false;
		SaveAIFontImage2 = false;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool WriteMainParam()
	{
		ini.WriteBool("Main", "StationON", StationON);
		ini.WriteBool("Main", "LocateToolON", LocateToolON);
		ini.WriteBool("Main", "SelectWayON", SelectWayON);
		ini.WriteBool("Main", "SelectFaceON", SelectFaceON);
		ini.WriteBool("Main", "EnahnceToolON", EnahnceToolON);
		ini.WriteBool("Main", "MaskToolON", MaskToolON);
		ini.WriteBool("Main", "BinaryToolON", BinaryToolON);
		ini.WriteBool("Main", "AdvLocateToolON", AdvLocateToolON);
		ini.WriteBool("Main", "MeasureToolON", MeasureToolON);
		ini.WriteBool("Main", "SmoothToolON", SmoothToolON);
		ini.WriteBool("Main", "DifferenceToolON", DifferenceToolON);
		ini.WriteBool("Main", "BlockAvgToolON", BlockAvgToolON);
		ini.WriteBool("Main", "LineAnalysisToolON", LineAnalysisToolON);
		ini.WriteBool("Main", "MatchToolON", MatchToolON);
		ini.WriteBool("Main", "FontToolON", FontToolON);
		ini.WriteBool("Main", "IRToolON", IRToolON);
		ini.WriteBool("Main", "AIProphetToolON", AIProphetToolON);
		ini.WriteBool("Main", "AIEnhanceToolON", AIEnhanceToolON);
		ini.WriteBool("Main", "AIPredictToolON", AIPredictToolON);
		ini.WriteDouble("Main", "PixelPerUM_X", PixelPerUM_X);
		ini.WriteDouble("Main", "PixelPerUM_Y", PixelPerUM_Y);
		ini.WriteDouble("Main", "ObjSizeUM_W", ObjSizeUM_W);
		ini.WriteDouble("Main", "ObjSizeUM_H", ObjSizeUM_H);
		ini.WriteBool("Main", "UseFloat", UseFloat);
		ini.WriteInt("Main", "MeasureUnit", MeasureUnit);
		ini.WriteInt("Main", "LanguageType", LanguageType);
		ini.WriteString("Main", "OKPath", OKPath);
		ini.WriteString("Main", "NGPath", NGPath);
		ini.WriteString("Main", "AIRoot_Path", AIRoot_Path);
		ini.WriteString("Main", "AIOK_Path_A", AIOK_Path_A);
		ini.WriteString("Main", "AING_Path_A", AING_Path_A);
		ini.WriteString("Main", "OK_Path_A", OK_Path_A);
		ini.WriteString("Main", "NG_Path_A", NG_Path_A);
		ini.WriteString("Main", "AI_X_Path_A", AI_X_Path_A);
		ini.WriteString("Main", "AI_Y_Path_A", AI_Y_Path_A);
		ini.WriteString("Main", "AI_X_Path_OK", AI_X_Path_OK);
		ini.WriteString("Main", "AI_Y_Path_OK", AI_Y_Path_OK);
		ini.WriteString("Main", "AI_X_Path_NG", AI_X_Path_NG);
		ini.WriteString("Main", "AI_Y_Path_NG", AI_Y_Path_NG);
		ini.WriteString("Main", "AIOK_Path_B", AIOK_Path_B);
		ini.WriteString("Main", "AING_Path_B", AING_Path_B);
		ini.WriteString("Main", "OK_Path_B", OK_Path_B);
		ini.WriteString("Main", "NG_Path_B", NG_Path_B);
		ini.WriteBool("Main", "FontOCRUseCuda", FontOCRUseCuda);
		ini.WriteBool("Main", "UseUNET", UseUNET);
		ini.WriteInt("Main", "ColorSelect", ColorSelect);
		ini.WriteInt("Main", "PreviewMode", PreviewMode);
		ini.WriteInt("Main", "ZoomMode", ZoomMode);
		ini.WriteInt("Main", "PenSize", PenSize);
		ini.WriteBool("Main", "UseExpandArea", UseExpandArea);
		ini.WriteBool("Main", "SaveAsFolder", SaveAsFolder);
		ini.WriteInt("Main", "AutoIgnoreSize", AutoIgnoreSize);
		ini.WriteInt("Main", "ExpandValue", ExpandValue);
		ini.WriteBool("Main", "EnableContinueWrite", EnableContinueWrite);
		ini.WriteBool("Main", "EnableAutoIgnoreDraw", EnableAutoIgnoreDraw);
		ini.WriteString("Main", "DrawCaition1", DrawCaition[0]);
		ini.WriteString("Main", "DrawCaition2", DrawCaition[1]);
		ini.WriteString("Main", "DrawCaition3", DrawCaition[2]);
		ini.WriteString("Main", "DrawCaition4", DrawCaition[3]);
		ini.WriteString("Main", "DrawCaition5", DrawCaition[4]);
		ini.WriteString("Main", "DrawCaition6", DrawCaition[5]);
		ini.WriteInt("Main", "SoftCorrSetup_R", SoftCorrSetup_R);
		ini.WriteInt("Main", "SoftCorrSetup_G", SoftCorrSetup_G);
		ini.WriteInt("Main", "SoftCorrSetup_B", SoftCorrSetup_B);
		ini.WriteInt("Main", "SoftCorrectoinAllowDiff", SoftCorrectoinAllowDiff);
		ini.WriteInt("Main", "JudgeWithAdvMeasureIndexA", JudgeWithAdvMeasureIndexA);
		ini.WriteInt("Main", "JudgeWithAdvMeasureIndexB", JudgeWithAdvMeasureIndexB);
		ini.WriteBool("Main", "EnableJudgeWithAdvMeasure", EnableJudgeWithAdvMeasure);
		return true;
	}

	public void SetComDigits(double[] comdigits)
	{
		for (int i = 0; i < 24; i++)
		{
			Comdigits[i] = comdigits[i];
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadLocateParam()
	{
		if (ini == null)
		{
			return false;
		}
		LocateParam.ValueChangeFlag = false;
		LocateParam.TrackerName = ini.GetValue("Locate", "TrackerName", "定位");
		LocateParam.LocateType = ini.GetInteger("Locate", "LocateType");
		LocateParam.LocateAngleLimit = ini.GetDouble("Locate", "LocateAngleLimit", 30.0);
		LocateParam.LocateHeightMin = ini.GetDouble("Locate", "LocateHeightMin", 100.0);
		LocateParam.LocateHeightMax = ini.GetDouble("Locate", "LocateHeightMax", 200.0);
		LocateParam.LocateWidthMin = ini.GetDouble("Locate", "LocateWidthMin", 100.0);
		LocateParam.LocateWidthMax = ini.GetDouble("Locate", "LocateWidthMax", 200.0);
		LocateParam.doRotateMinAngle = ini.GetDouble("Locate", "doRotateMinAngle", 0.2);
		LocateParam.LocateErrorBin = (FLOW_BIN)ini.GetInteger("Locate", "LocateErrorBin");
		LocateParam.LocateSizeErrorBin = (FLOW_BIN)ini.GetInteger("Locate", "LocateSizeErrorBin");
		LocateParam.ParetoIndex = ini.GetInteger("Locate", "ParetoIndex");
		LocateParam.LocateSpeedUp = ini.GetBoolean("Locate", "LocateSpeedUp");
		LocateParam.CreateHSVChannel = ini.GetBoolean("Locate", "CreateHSVChannel", defaultValue: true);
		LocateParam.EnableFocusCheck = ini.GetBoolean("Locate", "EnableFocusCheck");
		LocateParam.FocusLimit = ini.GetDouble("Locate", "FocusLimit");
		LocateParam.MaxSearchRect = ini.ReadRect("Locate", "MaxSearchRect", 100);
		LocateParam.SIMPSearchRect = ini.ReadRect("Locate", "SIMPSearchRect", 50);
		LocateParam.SIMP_THD1_Channel = ini.GetInteger("Locate", "SIMP_THD1_Channel");
		LocateParam.SIMP_THD1_Min = ini.GetInteger("Locate", "SIMP_THD1_Min");
		LocateParam.SIMP_THD1_Max = ini.GetInteger("Locate", "SIMP_THD1_Max", 255);
		LocateParam.SIMP_THD2_Enabled = ini.GetBoolean("Locate", "SIMP_THD2_Enabled");
		LocateParam.SIMP_THD2_Channel = ini.GetInteger("Locate", "SIMP_THD2_Channel");
		LocateParam.SIMP_THD2_Min = ini.GetInteger("Locate", "SIMP_THD2_Min");
		LocateParam.SIMP_THD2_Max = ini.GetInteger("Locate", "SIMP_THD2_Max", 255);
		LocateParam.SIMP_THD3_Enabled = ini.GetBoolean("Locate", "SIMP_THD3_Enabled");
		LocateParam.SIMP_THD3_Channel = ini.GetInteger("Locate", "SIMP_THD3_Channel");
		LocateParam.SIMP_THD3_Min = ini.GetInteger("Locate", "SIMP_THD3_Min");
		LocateParam.SIMP_THD3_Max = ini.GetInteger("Locate", "SIMP_THD3_Max", 255);
		LocateParam.SIMP_SideLocate = ini.GetBoolean("Locate", "SIMP_SideLocate");
		LocateParam.SIMP_DilateErode1Start = (USE_DILATE_ERODE)ini.GetInteger("Locate", "SIMP_DilateErode1Start");
		LocateParam.SIMP_DilateErode1End = (USE_DILATE_ERODE)ini.GetInteger("Locate", "SIMP_DilateErode1End");
		LocateParam.SIMP_DilateErode2Start = (USE_DILATE_ERODE)ini.GetInteger("Locate", "SIMP_DilateErode2Start");
		LocateParam.SIMP_DilateErode2End = (USE_DILATE_ERODE)ini.GetInteger("Locate", "SIMP_DilateErode2End");
		LocateParam.SIMP_DilateErode3Start = (USE_DILATE_ERODE)ini.GetInteger("Locate", "SIMP_DilateErode3Start");
		LocateParam.SIMP_DilateErode3End = (USE_DILATE_ERODE)ini.GetInteger("Locate", "SIMP_DilateErode3End");
		LocateParam.SIMP_DilateErode1StartCount = ini.GetInteger("Locate", "SIMP_DilateErode1StartCount");
		LocateParam.SIMP_DilateErode1EndCount = ini.GetInteger("Locate", "SIMP_DilateErode1EndCount");
		LocateParam.SIMP_DilateErode2StartCount = ini.GetInteger("Locate", "SIMP_DilateErode2StartCount");
		LocateParam.SIMP_DilateErode2EndCount = ini.GetInteger("Locate", "SIMP_DilateErode2EndCount");
		LocateParam.SIMP_DilateErode3StartCount = ini.GetInteger("Locate", "SIMP_DilateErode3StartCount");
		LocateParam.SIMP_DilateErode3EndCount = ini.GetInteger("Locate", "SIMP_DilateErode3EndCount");
		LocateParam.QuadSearchRectL = ini.ReadRect("Locate", "QuadSearchRectL");
		LocateParam.QuadSearchRectR = ini.ReadRect("Locate", "QuadSearchRectR");
		LocateParam.QuadSearchRectT = ini.ReadRect("Locate", "QuadSearchRectT");
		LocateParam.QuadSearchRectB = ini.ReadRect("Locate", "QuadSearchRectB");
		LocateParam.Quad_Channel_L = ini.GetInteger("Locate", "Quad_Channel_L", 1);
		LocateParam.Quad_Channel_R = ini.GetInteger("Locate", "Quad_Channel_R", 1);
		LocateParam.Quad_Channel_T = ini.GetInteger("Locate", "Quad_Channel_T", 1);
		LocateParam.Quad_Channel_B = ini.GetInteger("Locate", "Quad_Channel_B", 1);
		LocateParam.Quad_THD_Min_L = ini.GetInteger("Locate", "Quad_THD_Min_L");
		LocateParam.Quad_THD_Max_L = ini.GetInteger("Locate", "Quad_THD_Max_L", 255);
		LocateParam.Quad_THD_Min_R = ini.GetInteger("Locate", "Quad_THD_Min_R");
		LocateParam.Quad_THD_Max_R = ini.GetInteger("Locate", "Quad_THD_Max_R", 255);
		LocateParam.Quad_THD_Min_T = ini.GetInteger("Locate", "Quad_THD_Min_T");
		LocateParam.Quad_THD_Max_T = ini.GetInteger("Locate", "Quad_THD_Max_T", 255);
		LocateParam.Quad_THD_Min_B = ini.GetInteger("Locate", "Quad_THD_Min_B");
		LocateParam.Quad_THD_Max_B = ini.GetInteger("Locate", "Quad_THD_Max_B", 255);
		LocateParam.Quad_DEStartCount_L = ini.GetInteger("Locate", "Quad_DEStartCount_L");
		LocateParam.Quad_DEEndCount_L = ini.GetInteger("Locate", "Quad_DEEndCount_L");
		LocateParam.Quad_DEStartCount_R = ini.GetInteger("Locate", "Quad_DEStartCount_R");
		LocateParam.Quad_DEEndCount_R = ini.GetInteger("Locate", "Quad_DEEndCount_R");
		LocateParam.Quad_DEStartCount_T = ini.GetInteger("Locate", "Quad_DEStartCount_T");
		LocateParam.Quad_DEEndCount_T = ini.GetInteger("Locate", "Quad_DEEndCount_T");
		LocateParam.Quad_DEStartCount_B = ini.GetInteger("Locate", "Quad_DEStartCount_B");
		LocateParam.Quad_DEEndCount_B = ini.GetInteger("Locate", "Quad_DEEndCount_B");
		LocateParam.Quad_DEStart_L = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEStart_L");
		LocateParam.Quad_DEEnd_L = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEEnd_L");
		LocateParam.Quad_DEStart_R = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEStart_R");
		LocateParam.Quad_DEEnd_R = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEEnd_R");
		LocateParam.Quad_DEStart_T = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEStart_T");
		LocateParam.Quad_DEEnd_T = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEEnd_T");
		LocateParam.Quad_DEStart_B = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEStart_B");
		LocateParam.Quad_DEEnd_B = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Quad_DEEnd_B");
		LocateParam.Direction_L = (ScanDirection)ini.GetInteger("Locate", "Direction_L");
		LocateParam.Direction_R = (ScanDirection)ini.GetInteger("Locate", "Direction_R", 1);
		LocateParam.Direction_T = (ScanDirection)ini.GetInteger("Locate", "Direction_T", 2);
		LocateParam.Direction_B = (ScanDirection)ini.GetInteger("Locate", "Direction_B", 3);
		LocateParam.QUAD_SideLocate = ini.GetBoolean("Locate", "QUAD_SideLocate");
		LocateParam.Quad_FilterPoint = ini.GetBoolean("Locate", "Quad_FilterPoint");
		LocateParam.Quad_UseStdFilter = ini.GetBoolean("Locate", "Quad_UseStdFilter");
		LocateParam.Quad_UseSideLocate = ini.GetBoolean("Locate", "Quad_UseSideLocate");
		LocateParam.Quad_FixSideLocateLR = ini.GetBoolean("Locate", "Quad_FixSideLocateLR");
		LocateParam.Quad_LocateAngleSelect = ini.GetInteger("Locate", "Quad_LocateAngleSelect");
		LocateParam.Quad_Mask_T = ini.ReadRect("Locate", "Quad_Mask_T");
		LocateParam.Quad_Mask_B = ini.ReadRect("Locate", "Quad_Mask_B");
		LocateParam.Quad_Mask_L = ini.ReadRect("Locate", "Quad_Mask_L");
		LocateParam.Quad_Mask_R = ini.ReadRect("Locate", "Quad_Mask_R");
		LocateParam.Quad_EnableMask_T = ini.GetBoolean("Locate", "Quad_EnableMask_T");
		LocateParam.Quad_EnableMask_B = ini.GetBoolean("Locate", "Quad_EnableMask_B");
		LocateParam.Quad_EnableMask_L = ini.GetBoolean("Locate", "Quad_EnableMask_L");
		LocateParam.Quad_EnableMask_R = ini.GetBoolean("Locate", "Quad_EnableMask_R");
		LocateParam.Quad_EnableAvgEdge_T = ini.GetBoolean("Locate", "Quad_EnableAvgEdge_T");
		LocateParam.Quad_EnableAvgEdge_B = ini.GetBoolean("Locate", "Quad_EnableAvgEdge_B");
		LocateParam.Quad_EnableAvgEdge_L = ini.GetBoolean("Locate", "Quad_EnableAvgEdge_L");
		LocateParam.Quad_EnableAvgEdge_R = ini.GetBoolean("Locate", "Quad_EnableAvgEdge_R");
		LocateParam.Quad_EnableRGBThd_L = ini.GetBoolean("Locate", "Quad_EnableRGBThd_L");
		LocateParam.Quad_EnableRGBThd_R = ini.GetBoolean("Locate", "Quad_EnableRGBThd_R");
		LocateParam.Quad_EnableRGBThd_T = ini.GetBoolean("Locate", "Quad_EnableRGBThd_T");
		LocateParam.Quad_EnableRGBThd_B = ini.GetBoolean("Locate", "Quad_EnableRGBThd_B");
		LocateParam.Quad_THDR_Min_L = ini.GetInteger("Locate", "Quad_THDR_Min_L");
		LocateParam.Quad_THDR_Max_L = ini.GetInteger("Locate", "Quad_THDR_Max_L", 255);
		LocateParam.Quad_THDR_Min_R = ini.GetInteger("Locate", "Quad_THDR_Min_R");
		LocateParam.Quad_THDR_Max_R = ini.GetInteger("Locate", "Quad_THDR_Max_R", 255);
		LocateParam.Quad_THDR_Min_T = ini.GetInteger("Locate", "Quad_THDR_Min_T");
		LocateParam.Quad_THDR_Max_T = ini.GetInteger("Locate", "Quad_THDR_Max_T", 255);
		LocateParam.Quad_THDR_Min_B = ini.GetInteger("Locate", "Quad_THDR_Min_B");
		LocateParam.Quad_THDR_Max_B = ini.GetInteger("Locate", "Quad_THDR_Max_B", 255);
		LocateParam.Quad_THDG_Min_L = ini.GetInteger("Locate", "Quad_THDG_Min_L");
		LocateParam.Quad_THDG_Max_L = ini.GetInteger("Locate", "Quad_THDG_Max_L", 255);
		LocateParam.Quad_THDG_Min_R = ini.GetInteger("Locate", "Quad_THDG_Min_R");
		LocateParam.Quad_THDG_Max_R = ini.GetInteger("Locate", "Quad_THDG_Max_R", 255);
		LocateParam.Quad_THDG_Min_T = ini.GetInteger("Locate", "Quad_THDG_Min_T");
		LocateParam.Quad_THDG_Max_T = ini.GetInteger("Locate", "Quad_THDG_Max_T", 255);
		LocateParam.Quad_THDG_Min_B = ini.GetInteger("Locate", "Quad_THDG_Min_B");
		LocateParam.Quad_THDG_Max_B = ini.GetInteger("Locate", "Quad_THDG_Max_B", 255);
		LocateParam.Quad_THDB_Min_L = ini.GetInteger("Locate", "Quad_THDB_Min_L");
		LocateParam.Quad_THDB_Max_L = ini.GetInteger("Locate", "Quad_THDB_Max_L", 255);
		LocateParam.Quad_THDB_Min_R = ini.GetInteger("Locate", "Quad_THDB_Min_R");
		LocateParam.Quad_THDB_Max_R = ini.GetInteger("Locate", "Quad_THDB_Max_R", 255);
		LocateParam.Quad_THDB_Min_T = ini.GetInteger("Locate", "Quad_THDB_Min_T");
		LocateParam.Quad_THDB_Max_T = ini.GetInteger("Locate", "Quad_THDB_Max_T", 255);
		LocateParam.Quad_THDB_Min_B = ini.GetInteger("Locate", "Quad_THDB_Min_B");
		LocateParam.Quad_THDB_Max_B = ini.GetInteger("Locate", "Quad_THDB_Max_B", 255);
		LocateParam.UseMelfMode = ini.GetBoolean("Locate", "UseMelfMode");
		LocateParam.PadSearchRectL = ini.ReadRect("Locate", "PadSearchRectL", 50);
		LocateParam.PadSearchRectR = ini.ReadRect("Locate", "PadSearchRectR", 50);
		LocateParam.Pad_Channel_L1 = ini.GetInteger("Locate", "Pad_Channel_L1", 1);
		LocateParam.Pad_Channel_L2 = ini.GetInteger("Locate", "Pad_Channel_L2", 1);
		LocateParam.Pad_Channel_R1 = ini.GetInteger("Locate", "Pad_Channel_R1", 1);
		LocateParam.Pad_Channel_R2 = ini.GetInteger("Locate", "Pad_Channel_R2", 1);
		LocateParam.Pad_THD_Min_L1 = ini.GetInteger("Locate", "Pad_THD_Min_L1");
		LocateParam.Pad_THD_Max_L1 = ini.GetInteger("Locate", "Pad_THD_Max_L1", 255);
		LocateParam.Pad_THD_Min_L2 = ini.GetInteger("Locate", "Pad_THD_Min_L2");
		LocateParam.Pad_THD_Max_L2 = ini.GetInteger("Locate", "Pad_THD_Max_L2", 255);
		LocateParam.Pad_THD_Min_R1 = ini.GetInteger("Locate", "Pad_THD_Min_R1");
		LocateParam.Pad_THD_Max_R1 = ini.GetInteger("Locate", "Pad_THD_Max_R1", 255);
		LocateParam.Pad_THD_Min_R2 = ini.GetInteger("Locate", "Pad_THD_Min_R2");
		LocateParam.Pad_THD_Max_R2 = ini.GetInteger("Locate", "Pad_THD_Max_R2", 255);
		LocateParam.Pad_DEStartCount_L1 = ini.GetInteger("Locate", "Pad_DEStartCount_L1");
		LocateParam.Pad_DEEndCount_L1 = ini.GetInteger("Locate", "Pad_DEEndCount_L1");
		LocateParam.Pad_DEStartCount_R1 = ini.GetInteger("Locate", "Pad_DEStartCount_R1");
		LocateParam.Pad_DEEndCount_R1 = ini.GetInteger("Locate", "Pad_DEEndCount_R1");
		LocateParam.Pad_DEStartCount_L2 = ini.GetInteger("Locate", "Pad_DEStartCount_L2");
		LocateParam.Pad_DEEndCount_L2 = ini.GetInteger("Locate", "Pad_DEEndCount_L2");
		LocateParam.Pad_DEStartCount_R2 = ini.GetInteger("Locate", "Pad_DEStartCount_R2");
		LocateParam.Pad_DEEndCount_R2 = ini.GetInteger("Locate", "Pad_DEEndCount_R2");
		LocateParam.Pad_DEStart_L1 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEStart_L1");
		LocateParam.Pad_DEEnd_L1 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEEnd_L1");
		LocateParam.Pad_DEStart_R1 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEStart_R1");
		LocateParam.Pad_DEEnd_R1 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEEnd_R1");
		LocateParam.Pad_DEStart_L2 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEStart_L2");
		LocateParam.Pad_DEEnd_L2 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEEnd_L2");
		LocateParam.Pad_DEStart_R2 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEStart_R2");
		LocateParam.Pad_DEEnd_R2 = (USE_DILATE_ERODE)ini.GetInteger("Locate", "Pad_DEEnd_R2");
		LocateParam.EnableAILocate = ini.GetBoolean("Locate", "EnableAILocate");
		LocateParam.EnableTensorRT = ini.GetBoolean("Locate", "EnableTensorRT");
		LocateParam.AILocateTrainCount = ini.GetInteger("Locate", "AILocateTrainCount", 300);
		LocateParam.ModelWidth = ini.GetInteger("Locate", "ModelWidth", 128);
		LocateParam.ModelHeight = ini.GetInteger("Locate", "ModelHeight", 128);
		LocateParam.ModelDiv = ini.GetInteger("Locate", "ModelDiv", 8);
		LocateParam.ModelBatchSize = ini.GetInteger("Locate", "ModelBatchSize", 8);
		LocateParam.AILocateModelName = ini.GetValue("Locate", "AILocateModelName");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteLocateParam()
	{
		if (ini == null)
		{
			return false;
		}
		if (!LocateParam.ValueChangeFlag)
		{
			return true;
		}
		LocateParam.ValueChangeFlag = false;
		ini.WriteString("Locate", "TrackerName", LocateParam.TrackerName);
		ini.WriteInt("Locate", "LocateType", LocateParam.LocateType);
		ini.WriteDouble("Locate", "LocateAngleLimit", LocateParam.LocateAngleLimit);
		ini.WriteDouble("Locate", "LocateHeightMin", LocateParam.LocateHeightMin);
		ini.WriteDouble("Locate", "LocateHeightMax", LocateParam.LocateHeightMax);
		ini.WriteDouble("Locate", "LocateWidthMin", LocateParam.LocateWidthMin);
		ini.WriteDouble("Locate", "LocateWidthMax", LocateParam.LocateWidthMax);
		ini.WriteDouble("Locate", "doRotateMinAngle", LocateParam.doRotateMinAngle);
		ini.WriteInt("Locate", "LocateErrorBin", (int)LocateParam.LocateErrorBin);
		ini.WriteInt("Locate", "LocateSizeErrorBin", (int)LocateParam.LocateSizeErrorBin);
		ini.WriteInt("Locate", "ParetoIndex", LocateParam.ParetoIndex);
		ini.WriteBool("Locate", "LocateSpeedUp", LocateParam.LocateSpeedUp);
		ini.WriteBool("Locate", "CreateHSVChannel", LocateParam.CreateHSVChannel);
		ini.WriteBool("Locate", "EnableFocusCheck", LocateParam.EnableFocusCheck);
		ini.WriteDouble("Locate", "FocusLimit", LocateParam.FocusLimit);
		ini.WriteRect("Locate", "MaxSearchRect", LocateParam.MaxSearchRect);
		ini.WriteRect("Locate", "SIMPSearchRect", LocateParam.SIMPSearchRect);
		ini.WriteInt("Locate", "SIMP_THD1_Channel", LocateParam.SIMP_THD1_Channel);
		ini.WriteInt("Locate", "SIMP_THD1_Min", LocateParam.SIMP_THD1_Min);
		ini.WriteInt("Locate", "SIMP_THD1_Max", LocateParam.SIMP_THD1_Max);
		ini.WriteInt("Locate", "SIMP_THD2_Channel", LocateParam.SIMP_THD2_Channel);
		ini.WriteInt("Locate", "SIMP_THD2_Min", LocateParam.SIMP_THD2_Min);
		ini.WriteInt("Locate", "SIMP_THD2_Max", LocateParam.SIMP_THD2_Max);
		ini.WriteInt("Locate", "SIMP_THD3_Channel", LocateParam.SIMP_THD3_Channel);
		ini.WriteInt("Locate", "SIMP_THD3_Min", LocateParam.SIMP_THD3_Min);
		ini.WriteInt("Locate", "SIMP_THD3_Max", LocateParam.SIMP_THD3_Max);
		ini.WriteBool("Locate", "SIMP_AdvancdeMode", LocateParam.SIMP_AdvancdeMode);
		ini.WriteBool("Locate", "SIMP_THD2_Enabled", LocateParam.SIMP_THD2_Enabled);
		ini.WriteBool("Locate", "SIMP_THD3_Enabled", LocateParam.SIMP_THD3_Enabled);
		ini.WriteBool("Locate", "SIMP_SideLocate", LocateParam.SIMP_SideLocate);
		ini.WriteInt("Locate", "SIMP_DilateErode1Start", (int)LocateParam.SIMP_DilateErode1Start);
		ini.WriteInt("Locate", "SIMP_DilateErode1End", (int)LocateParam.SIMP_DilateErode1End);
		ini.WriteInt("Locate", "SIMP_DilateErode2Start", (int)LocateParam.SIMP_DilateErode2Start);
		ini.WriteInt("Locate", "SIMP_DilateErode2End", (int)LocateParam.SIMP_DilateErode2End);
		ini.WriteInt("Locate", "SIMP_DilateErode3Start", (int)LocateParam.SIMP_DilateErode3Start);
		ini.WriteInt("Locate", "SIMP_DilateErode3End", (int)LocateParam.SIMP_DilateErode3End);
		ini.WriteInt("Locate", "SIMP_DilateErode1StartCount", LocateParam.SIMP_DilateErode1StartCount);
		ini.WriteInt("Locate", "SIMP_DilateErode1EndCount", LocateParam.SIMP_DilateErode1EndCount);
		ini.WriteInt("Locate", "SIMP_DilateErode2StartCount", LocateParam.SIMP_DilateErode2StartCount);
		ini.WriteInt("Locate", "SIMP_DilateErode2EndCount", LocateParam.SIMP_DilateErode2EndCount);
		ini.WriteInt("Locate", "SIMP_DilateErode3StartCount", LocateParam.SIMP_DilateErode3StartCount);
		ini.WriteInt("Locate", "SIMP_DilateErode3EndCount", LocateParam.SIMP_DilateErode3EndCount);
		ini.WriteRect("Locate", "QuadSearchRectL", LocateParam.QuadSearchRectL);
		ini.WriteRect("Locate", "QuadSearchRectR", LocateParam.QuadSearchRectR);
		ini.WriteRect("Locate", "QuadSearchRectT", LocateParam.QuadSearchRectT);
		ini.WriteRect("Locate", "QuadSearchRectB", LocateParam.QuadSearchRectB);
		ini.WriteInt("Locate", "Quad_Channel_L", LocateParam.Quad_Channel_L);
		ini.WriteInt("Locate", "Quad_Channel_R", LocateParam.Quad_Channel_R);
		ini.WriteInt("Locate", "Quad_Channel_T", LocateParam.Quad_Channel_T);
		ini.WriteInt("Locate", "Quad_Channel_B", LocateParam.Quad_Channel_B);
		ini.WriteInt("Locate", "Quad_THD_Min_L", LocateParam.Quad_THD_Min_L);
		ini.WriteInt("Locate", "Quad_THD_Max_L", LocateParam.Quad_THD_Max_L);
		ini.WriteInt("Locate", "Quad_THD_Min_R", LocateParam.Quad_THD_Min_R);
		ini.WriteInt("Locate", "Quad_THD_Max_R", LocateParam.Quad_THD_Max_R);
		ini.WriteInt("Locate", "Quad_THD_Min_T", LocateParam.Quad_THD_Min_T);
		ini.WriteInt("Locate", "Quad_THD_Max_T", LocateParam.Quad_THD_Max_T);
		ini.WriteInt("Locate", "Quad_THD_Min_B", LocateParam.Quad_THD_Min_B);
		ini.WriteInt("Locate", "Quad_THD_Max_B", LocateParam.Quad_THD_Max_B);
		ini.WriteInt("Locate", "Quad_DEStartCount_L", LocateParam.Quad_DEStartCount_L);
		ini.WriteInt("Locate", "Quad_DEEndCount_L", LocateParam.Quad_DEEndCount_L);
		ini.WriteInt("Locate", "Quad_DEStartCount_R", LocateParam.Quad_DEStartCount_R);
		ini.WriteInt("Locate", "Quad_DEEndCount_R", LocateParam.Quad_DEEndCount_R);
		ini.WriteInt("Locate", "Quad_DEStartCount_T", LocateParam.Quad_DEStartCount_T);
		ini.WriteInt("Locate", "Quad_DEEndCount_T", LocateParam.Quad_DEEndCount_T);
		ini.WriteInt("Locate", "Quad_DEStartCount_B", LocateParam.Quad_DEStartCount_B);
		ini.WriteInt("Locate", "Quad_DEEndCount_B", LocateParam.Quad_DEEndCount_B);
		ini.WriteInt("Locate", "Quad_DEStart_L", (int)LocateParam.Quad_DEStart_L);
		ini.WriteInt("Locate", "Quad_DEEnd_L", (int)LocateParam.Quad_DEEnd_L);
		ini.WriteInt("Locate", "Quad_DEStart_R", (int)LocateParam.Quad_DEStart_R);
		ini.WriteInt("Locate", "Quad_DEEnd_R", (int)LocateParam.Quad_DEEnd_R);
		ini.WriteInt("Locate", "Quad_DEStart_T", (int)LocateParam.Quad_DEStart_T);
		ini.WriteInt("Locate", "Quad_DEEnd_T", (int)LocateParam.Quad_DEEnd_T);
		ini.WriteInt("Locate", "Quad_DEStart_B", (int)LocateParam.Quad_DEStart_B);
		ini.WriteInt("Locate", "Quad_DEEnd_B", (int)LocateParam.Quad_DEEnd_B);
		ini.WriteInt("Locate", "Direction_L", (int)LocateParam.Direction_L);
		ini.WriteInt("Locate", "Direction_R", (int)LocateParam.Direction_R);
		ini.WriteInt("Locate", "Direction_T", (int)LocateParam.Direction_T);
		ini.WriteInt("Locate", "Direction_B", (int)LocateParam.Direction_B);
		ini.WriteBool("Locate", "QUAD_SideLocate", LocateParam.QUAD_SideLocate);
		ini.WriteBool("Locate", "Quad_FilterPoint", LocateParam.Quad_FilterPoint);
		ini.WriteBool("Locate", "Quad_UseStdFilter", LocateParam.Quad_UseStdFilter);
		ini.WriteBool("Locate", "Quad_UseSideLocate", LocateParam.Quad_UseSideLocate);
		ini.WriteBool("Locate", "Quad_FixSideLocateLR", LocateParam.Quad_FixSideLocateLR);
		ini.WriteInt("Locate", "Quad_LocateAngleSelect", LocateParam.Quad_LocateAngleSelect);
		ini.WriteRect("Locate", "Quad_Mask_T", LocateParam.Quad_Mask_T);
		ini.WriteRect("Locate", "Quad_Mask_B", LocateParam.Quad_Mask_B);
		ini.WriteRect("Locate", "Quad_Mask_L", LocateParam.Quad_Mask_L);
		ini.WriteRect("Locate", "Quad_Mask_R", LocateParam.Quad_Mask_R);
		ini.WriteBool("Locate", "Quad_EnableMask_T", LocateParam.Quad_EnableMask_T);
		ini.WriteBool("Locate", "Quad_EnableMask_B", LocateParam.Quad_EnableMask_B);
		ini.WriteBool("Locate", "Quad_EnableMask_L", LocateParam.Quad_EnableMask_L);
		ini.WriteBool("Locate", "Quad_EnableMask_R", LocateParam.Quad_EnableMask_R);
		ini.WriteBool("Locate", "Quad_EnableAvgEdge_T", LocateParam.Quad_EnableAvgEdge_T);
		ini.WriteBool("Locate", "Quad_EnableAvgEdge_B", LocateParam.Quad_EnableAvgEdge_B);
		ini.WriteBool("Locate", "Quad_EnableAvgEdge_L", LocateParam.Quad_EnableAvgEdge_L);
		ini.WriteBool("Locate", "Quad_EnableAvgEdge_R", LocateParam.Quad_EnableAvgEdge_R);
		ini.WriteBool("Locate", "Quad_EnableRGBThd_T", LocateParam.Quad_EnableRGBThd_T);
		ini.WriteBool("Locate", "Quad_EnableRGBThd_B", LocateParam.Quad_EnableRGBThd_B);
		ini.WriteBool("Locate", "Quad_EnableRGBThd_L", LocateParam.Quad_EnableRGBThd_L);
		ini.WriteBool("Locate", "Quad_EnableRGBThd_R", LocateParam.Quad_EnableRGBThd_R);
		ini.WriteInt("Locate", "Quad_THDR_Min_L", LocateParam.Quad_THDR_Min_L);
		ini.WriteInt("Locate", "Quad_THDR_Max_L", LocateParam.Quad_THDR_Max_L);
		ini.WriteInt("Locate", "Quad_THDR_Min_R", LocateParam.Quad_THDR_Min_R);
		ini.WriteInt("Locate", "Quad_THDR_Max_R", LocateParam.Quad_THDR_Max_R);
		ini.WriteInt("Locate", "Quad_THDR_Min_T", LocateParam.Quad_THDR_Min_T);
		ini.WriteInt("Locate", "Quad_THDR_Max_T", LocateParam.Quad_THDR_Max_T);
		ini.WriteInt("Locate", "Quad_THDR_Min_B", LocateParam.Quad_THDR_Min_B);
		ini.WriteInt("Locate", "Quad_THDR_Max_B", LocateParam.Quad_THDR_Max_B);
		ini.WriteInt("Locate", "Quad_THDG_Min_L", LocateParam.Quad_THDG_Min_L);
		ini.WriteInt("Locate", "Quad_THDG_Max_L", LocateParam.Quad_THDG_Max_L);
		ini.WriteInt("Locate", "Quad_THDG_Min_R", LocateParam.Quad_THDG_Min_R);
		ini.WriteInt("Locate", "Quad_THDG_Max_R", LocateParam.Quad_THDG_Max_R);
		ini.WriteInt("Locate", "Quad_THDG_Min_T", LocateParam.Quad_THDG_Min_T);
		ini.WriteInt("Locate", "Quad_THDG_Max_T", LocateParam.Quad_THDG_Max_T);
		ini.WriteInt("Locate", "Quad_THDG_Min_B", LocateParam.Quad_THDG_Min_B);
		ini.WriteInt("Locate", "Quad_THDG_Max_B", LocateParam.Quad_THDG_Max_B);
		ini.WriteInt("Locate", "Quad_THDB_Min_L", LocateParam.Quad_THDB_Min_L);
		ini.WriteInt("Locate", "Quad_THDB_Max_L", LocateParam.Quad_THDB_Max_L);
		ini.WriteInt("Locate", "Quad_THDB_Min_R", LocateParam.Quad_THDB_Min_R);
		ini.WriteInt("Locate", "Quad_THDB_Max_R", LocateParam.Quad_THDB_Max_R);
		ini.WriteInt("Locate", "Quad_THDB_Min_T", LocateParam.Quad_THDB_Min_T);
		ini.WriteInt("Locate", "Quad_THDB_Max_T", LocateParam.Quad_THDB_Max_T);
		ini.WriteInt("Locate", "Quad_THDB_Min_B", LocateParam.Quad_THDB_Min_B);
		ini.WriteInt("Locate", "Quad_THDB_Max_B", LocateParam.Quad_THDB_Max_B);
		ini.WriteBool("Locate", "UseMelfMode", LocateParam.UseMelfMode);
		ini.WriteRect("Locate", "PadSearchRectL", LocateParam.PadSearchRectL);
		ini.WriteRect("Locate", "PadSearchRectR", LocateParam.PadSearchRectR);
		ini.WriteInt("Locate", "Pad_Channel_L1", LocateParam.Pad_Channel_L1);
		ini.WriteInt("Locate", "Pad_Channel_R1", LocateParam.Pad_Channel_R1);
		ini.WriteInt("Locate", "Pad_Channel_L2", LocateParam.Pad_Channel_L2);
		ini.WriteInt("Locate", "Pad_Channel_R2", LocateParam.Pad_Channel_R2);
		ini.WriteInt("Locate", "Pad_THD_Min_L1", LocateParam.Pad_THD_Min_L1);
		ini.WriteInt("Locate", "Pad_THD_Max_L1", LocateParam.Pad_THD_Max_L1);
		ini.WriteInt("Locate", "Pad_THD_Min_R1", LocateParam.Pad_THD_Min_R1);
		ini.WriteInt("Locate", "Pad_THD_Max_R1", LocateParam.Pad_THD_Max_R1);
		ini.WriteInt("Locate", "Pad_THD_Min_L2", LocateParam.Pad_THD_Min_L2);
		ini.WriteInt("Locate", "Pad_THD_Max_L2", LocateParam.Pad_THD_Max_L2);
		ini.WriteInt("Locate", "Pad_THD_Min_R2", LocateParam.Pad_THD_Min_R2);
		ini.WriteInt("Locate", "Pad_THD_Max_R2", LocateParam.Pad_THD_Max_R2);
		ini.WriteInt("Locate", "Pad_DEStartCount_L1", LocateParam.Pad_DEStartCount_L1);
		ini.WriteInt("Locate", "Pad_DEEndCount_L1", LocateParam.Pad_DEEndCount_L1);
		ini.WriteInt("Locate", "Pad_DEStartCount_R1", LocateParam.Pad_DEStartCount_R1);
		ini.WriteInt("Locate", "Pad_DEEndCount_R1", LocateParam.Pad_DEEndCount_R1);
		ini.WriteInt("Locate", "Pad_DEStartCount_L2", LocateParam.Pad_DEStartCount_L2);
		ini.WriteInt("Locate", "Pad_DEEndCount_L2", LocateParam.Pad_DEEndCount_L2);
		ini.WriteInt("Locate", "Pad_DEStartCount_R2", LocateParam.Pad_DEStartCount_R2);
		ini.WriteInt("Locate", "Pad_DEEndCount_R2", LocateParam.Pad_DEEndCount_R2);
		ini.WriteInt("Locate", "Pad_DEStart_L1", (int)LocateParam.Pad_DEStart_L1);
		ini.WriteInt("Locate", "Pad_DEEnd_L1", (int)LocateParam.Pad_DEEnd_L1);
		ini.WriteInt("Locate", "Pad_DEStart_R1", (int)LocateParam.Pad_DEStart_R1);
		ini.WriteInt("Locate", "Pad_DEEnd_R1", (int)LocateParam.Pad_DEEnd_R1);
		ini.WriteInt("Locate", "Pad_DEStart_L2", (int)LocateParam.Pad_DEStart_L2);
		ini.WriteInt("Locate", "Pad_DEEnd_L2", (int)LocateParam.Pad_DEEnd_L2);
		ini.WriteInt("Locate", "Pad_DEStart_R2", (int)LocateParam.Pad_DEStart_R2);
		ini.WriteInt("Locate", "Pad_DEEnd_R2", (int)LocateParam.Pad_DEEnd_R2);
		ini.WriteBool("Locate", "EnableAILocate", LocateParam.EnableAILocate);
		ini.WriteBool("Locate", "EnableTensorRT", LocateParam.EnableTensorRT);
		ini.WriteInt("Locate", "AILocateTrainCount", LocateParam.AILocateTrainCount);
		ini.WriteInt("Locate", "ModelWidth", LocateParam.ModelWidth);
		ini.WriteInt("Locate", "ModelHeight", LocateParam.ModelHeight);
		ini.WriteInt("Locate", "ModelDiv", LocateParam.ModelDiv);
		ini.WriteInt("Locate", "ModelBatchSize", LocateParam.ModelBatchSize);
		ini.WriteString("Locate", "AILocateModelName", LocateParam.AILocateModelName);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadLightMonitorParam()
	{
		INSP_LIGHTMONITOR_PARAM lightMonitorParam = new INSP_LIGHTMONITOR_PARAM
		{
			ValueChangeFlag = false
		};
		string section = "LightMonitorTool";
		lightMonitorParam.LightMonitor_ON = ini.GetBoolean(section, "LightMonitor_ON");
		lightMonitorParam.LightCorrection_ON = ini.GetBoolean(section, "LightCorrection_ON");
		lightMonitorParam.EnableSoftCorrection = ini.GetBoolean(section, "EnableSoftCorrection");
		lightMonitorParam.SearchRectT = ini.GetInteger(section, "SearchRectT", 5);
		lightMonitorParam.SearchRectB = ini.GetInteger(section, "SearchRectB", 5);
		lightMonitorParam.SearchRectL = ini.GetInteger(section, "SearchRectL", 5);
		lightMonitorParam.SearchRectR = ini.GetInteger(section, "SearchRectR", 5);
		lightMonitorParam.SearchRectWidth = ini.GetInteger(section, "SearchRectWidth", 50);
		lightMonitorParam.SearchRectHeight = ini.GetInteger(section, "SearchRectHeight", 50);
		lightMonitorParam.ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "ChannelIndex");
		lightMonitorParam.TrackerAligmentIndex = ini.GetInteger(section, "TrackerAligmentIndex");
		lightMonitorParam.GainPerValue = ini.GetInteger(section, "GainPerValue", 15);
		lightMonitorParam.AlarmBinaryValue = ini.GetInteger(section, "AlarmBinaryValue", 50);
		lightMonitorParam.MonitorValue = ini.GetInteger(section, "MonitorValue", -1);
		lightMonitorParam.CorrectionBasicValue = ini.GetInteger(section, "CorrectionBasicValue", -1);
		lightMonitorParam.MonitorValueR = ini.GetInteger(section, "MonitorValueR", -1);
		lightMonitorParam.MonitorValueG = ini.GetInteger(section, "MonitorValueG", -1);
		lightMonitorParam.MonitorValueB = ini.GetInteger(section, "MonitorValueB", -1);
		lightMonitorParam.CorrectionValueR = ini.GetInteger(section, "CorrectionValueR", -1);
		lightMonitorParam.CorrectionValueG = ini.GetInteger(section, "CorrectionValueG", -1);
		lightMonitorParam.CorrectionValueB = ini.GetInteger(section, "CorrectionValueB", -1);
		lightMonitorParam.AlarmRatioR = ini.GetDouble(section, "AlarmRatioR", 5.0);
		lightMonitorParam.AlarmRatioG = ini.GetDouble(section, "AlarmRatioG", 5.0);
		lightMonitorParam.AlarmRatioB = ini.GetDouble(section, "AlarmRatioB", 5.0);
		lightMonitorParam.CorrectionRatioR = ini.GetDouble(section, "CorrectionRatioR", 5.0);
		lightMonitorParam.CorrectionRatioG = ini.GetDouble(section, "CorrectionRatioG", 5.0);
		lightMonitorParam.CorrectionRatioB = ini.GetDouble(section, "CorrectionRatioB", 5.0);
		lightMonitorParam.VoltagePerValueR = ini.GetInteger(section, "VoltagePerValueR", 5);
		lightMonitorParam.VoltagePerValueG = ini.GetInteger(section, "VoltagePerValueG", 5);
		lightMonitorParam.VoltagePerValueB = ini.GetInteger(section, "VoltagePerValueB", 5);
		LightMonitorParam = lightMonitorParam;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteLightMonitorParam()
	{
		string section = "LightMonitorTool";
		INSP_LIGHTMONITOR_PARAM lightMonitorParam = LightMonitorParam;
		if (!lightMonitorParam.ValueChangeFlag)
		{
			return true;
		}
		lightMonitorParam.ValueChangeFlag = false;
		ini.WriteBool(section, "LightMonitor_ON", lightMonitorParam.LightMonitor_ON);
		ini.WriteBool(section, "LightCorrection_ON", lightMonitorParam.LightCorrection_ON);
		ini.WriteBool(section, "EnableSoftCorrection", lightMonitorParam.EnableSoftCorrection);
		ini.WriteInt(section, "SearchRectT", lightMonitorParam.SearchRectT);
		ini.WriteInt(section, "SearchRectB", lightMonitorParam.SearchRectB);
		ini.WriteInt(section, "SearchRectL", lightMonitorParam.SearchRectL);
		ini.WriteInt(section, "SearchRectR", lightMonitorParam.SearchRectR);
		ini.WriteInt(section, "SearchRectWidth", lightMonitorParam.SearchRectWidth);
		ini.WriteInt(section, "SearchRectHeight", lightMonitorParam.SearchRectHeight);
		ini.WriteInt(section, "ChannelIndex", (int)lightMonitorParam.ChannelIndex);
		ini.WriteInt(section, "TrackerAligmentIndex", lightMonitorParam.TrackerAligmentIndex);
		ini.WriteInt(section, "GainPerValue", lightMonitorParam.GainPerValue);
		ini.WriteInt(section, "AlarmBinaryValue", lightMonitorParam.AlarmBinaryValue);
		ini.WriteInt(section, "MonitorValue", lightMonitorParam.MonitorValue);
		ini.WriteInt(section, "CorrectionBasicValue", lightMonitorParam.CorrectionBasicValue);
		ini.WriteInt(section, "MonitorValueR", lightMonitorParam.MonitorValueR);
		ini.WriteInt(section, "MonitorValueG", lightMonitorParam.MonitorValueG);
		ini.WriteInt(section, "MonitorValueB", lightMonitorParam.MonitorValueB);
		ini.WriteInt(section, "CorrectionValueR", lightMonitorParam.CorrectionValueR);
		ini.WriteInt(section, "CorrectionValueG", lightMonitorParam.CorrectionValueG);
		ini.WriteInt(section, "CorrectionValueB", lightMonitorParam.CorrectionValueB);
		ini.WriteDouble(section, "AlarmRatioR", lightMonitorParam.AlarmRatioR);
		ini.WriteDouble(section, "AlarmRatioG", lightMonitorParam.AlarmRatioG);
		ini.WriteDouble(section, "AlarmRatioB", lightMonitorParam.AlarmRatioB);
		ini.WriteDouble(section, "CorrectionRatioR", lightMonitorParam.CorrectionRatioR);
		ini.WriteDouble(section, "CorrectionRatioG", lightMonitorParam.CorrectionRatioG);
		ini.WriteDouble(section, "CorrectionRatioB", lightMonitorParam.CorrectionRatioB);
		ini.WriteInt(section, "VoltagePerValueR", lightMonitorParam.VoltagePerValueR);
		ini.WriteInt(section, "VoltagePerValueG", lightMonitorParam.VoltagePerValueG);
		ini.WriteInt(section, "VoltagePerValueB", lightMonitorParam.VoltagePerValueB);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadSelectWayParam()
	{
		INSP_SELECTWAYTOOL_PARAM selectWayParam = new INSP_SELECTWAYTOOL_PARAM
		{
			ValueChangeFlag = false
		};
		string section = "SelectWayTool";
		selectWayParam.SuccessDoFlip = ini.GetInteger(section, "SuccessDoFlip");
		selectWayParam.SuccessDoFlipB = ini.GetInteger(section, "SuccessDoFlipB");
		selectWayParam.SelelctWayFunction = ini.GetInteger(section, "SelelctWayFunction");
		selectWayParam.SearchRectT = ini.GetInteger(section, "SearchRectT", 5);
		selectWayParam.SearchRectB = ini.GetInteger(section, "SearchRectB", 5);
		selectWayParam.SearchRectL = ini.GetInteger(section, "SearchRectL", 5);
		selectWayParam.SearchRectR = ini.GetInteger(section, "SearchRectR", 5);
		selectWayParam.SearchRectWidth = ini.GetInteger(section, "SearchRectWidth", 50);
		selectWayParam.SearchRectHeight = ini.GetInteger(section, "SearchRectHeight", 50);
		selectWayParam.SearchRect2T = ini.GetInteger(section, "SearchRect2T", 5);
		selectWayParam.SearchRect2B = ini.GetInteger(section, "SearchRect2B", 5);
		selectWayParam.SearchRect2L = ini.GetInteger(section, "SearchRect2L", 5);
		selectWayParam.SearchRect2R = ini.GetInteger(section, "SearchRect2R", 5);
		selectWayParam.SearchRect2Width = ini.GetInteger(section, "SearchRect2Width", 100);
		selectWayParam.SearchRect2Height = ini.GetInteger(section, "SearchRect2Height", 100);
		selectWayParam.THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		selectWayParam.THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		selectWayParam.THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		selectWayParam.THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
		selectWayParam.THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON", defaultValue: true);
		selectWayParam.THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON", defaultValue: true);
		selectWayParam.ColorReferenceIndex = ini.GetInteger(section, "ColorReferenceIndex");
		selectWayParam.THD1_Min = ini.GetInteger(section, "THD1_Min");
		selectWayParam.THD2_Min = ini.GetInteger(section, "THD2_Min");
		selectWayParam.THD3_Min = ini.GetInteger(section, "THD3_Min");
		selectWayParam.THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		selectWayParam.THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		selectWayParam.THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		selectWayParam.DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
		selectWayParam.DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
		selectWayParam.DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
		selectWayParam.DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
		selectWayParam.DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
		selectWayParam.DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
		selectWayParam.DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
		selectWayParam.DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
		selectWayParam.DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
		selectWayParam.DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
		selectWayParam.DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
		selectWayParam.DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
		selectWayParam.InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		selectWayParam.InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		selectWayParam.InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		selectWayParam.InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		selectWayParam.InspRule1_Min = ini.GetDouble(section, "InspRule1_Min");
		selectWayParam.InspRule2_Min = ini.GetDouble(section, "InspRule2_Min");
		selectWayParam.InspRule3_Min = ini.GetDouble(section, "InspRule3_Min");
		selectWayParam.InspRule4_Min = ini.GetDouble(section, "InspRule4_Min");
		selectWayParam.InspRule1_Max = ini.GetDouble(section, "InspRule1_Max");
		selectWayParam.InspRule2_Max = ini.GetDouble(section, "InspRule2_Max");
		selectWayParam.InspRule3_Max = ini.GetDouble(section, "InspRule3_Max", 300.0);
		selectWayParam.InspRule4_Max = ini.GetDouble(section, "InspRule4_Max", 300.0);
		selectWayParam.InspRuleAND = ini.GetBoolean(section, "InspRuleAND", defaultValue: true);
		selectWayParam.Use2WayMode = ini.GetBoolean(section, "Use2WayMode");
		selectWayParam.RotateMethod = ini.GetInteger(section, "RotateMethod");
		selectWayParam.RotateMethodB = ini.GetInteger(section, "RotateMethodB");
		selectWayParam.UseAutoSize = ini.GetBoolean(section, "UseAutoSize", defaultValue: true);
		selectWayParam.PatternSelect = ini.GetInteger(section, "PatternSelect");
		selectWayParam.MatchScoreMin = ini.GetInteger(section, "MatchScoreMin", 70);
		selectWayParam.MatchScoreMax = ini.GetInteger(section, "MatchScoreMax", 100);
		SelectWayParam = selectWayParam;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteSelectWayParam()
	{
		string section = "SelectWayTool";
		INSP_SELECTWAYTOOL_PARAM selectWayParam = SelectWayParam;
		if (!selectWayParam.ValueChangeFlag)
		{
			return true;
		}
		selectWayParam.ValueChangeFlag = false;
		ini.WriteInt(section, "SuccessDoFlip", selectWayParam.SuccessDoFlip);
		ini.WriteInt(section, "SuccessDoFlipB", selectWayParam.SuccessDoFlipB);
		ini.WriteInt(section, "SelelctWayFunction", selectWayParam.SelelctWayFunction);
		ini.WriteInt(section, "SearchRectT", selectWayParam.SearchRectT);
		ini.WriteInt(section, "SearchRectB", selectWayParam.SearchRectB);
		ini.WriteInt(section, "SearchRectL", selectWayParam.SearchRectL);
		ini.WriteInt(section, "SearchRectR", selectWayParam.SearchRectR);
		ini.WriteInt(section, "SearchRectWidth", selectWayParam.SearchRectWidth);
		ini.WriteInt(section, "SearchRectHeight", selectWayParam.SearchRectHeight);
		ini.WriteInt(section, "SearchRect2T", selectWayParam.SearchRect2T);
		ini.WriteInt(section, "SearchRect2B", selectWayParam.SearchRect2B);
		ini.WriteInt(section, "SearchRect2L", selectWayParam.SearchRect2L);
		ini.WriteInt(section, "SearchRect2R", selectWayParam.SearchRect2R);
		ini.WriteInt(section, "SearchRect2Width", selectWayParam.SearchRect2Width);
		ini.WriteInt(section, "SearchRect2Height", selectWayParam.SearchRect2Height);
		ini.WriteInt(section, "THD1_ChannelIndex", (int)selectWayParam.THD1_ChannelIndex);
		ini.WriteInt(section, "THD2_ChannelIndex", (int)selectWayParam.THD2_ChannelIndex);
		ini.WriteInt(section, "THD3_ChannelIndex", (int)selectWayParam.THD3_ChannelIndex);
		ini.WriteBool(section, "THD1_ChannelON", selectWayParam.THD1_ChannelON);
		ini.WriteBool(section, "THD2_ChannelON", selectWayParam.THD2_ChannelON);
		ini.WriteBool(section, "THD3_ChannelON", selectWayParam.THD3_ChannelON);
		ini.WriteInt(section, "ColorReferenceIndex", selectWayParam.ColorReferenceIndex);
		ini.WriteInt(section, "THD1_Min", selectWayParam.THD1_Min);
		ini.WriteInt(section, "THD2_Min", selectWayParam.THD2_Min);
		ini.WriteInt(section, "THD3_Min", selectWayParam.THD3_Min);
		ini.WriteInt(section, "THD1_Max", selectWayParam.THD1_Max);
		ini.WriteInt(section, "THD2_Max", selectWayParam.THD2_Max);
		ini.WriteInt(section, "THD3_Max", selectWayParam.THD3_Max);
		ini.WriteInt(section, "DE_Start1_Count", selectWayParam.DE_Start1_Count);
		ini.WriteInt(section, "DE_Start2_Count", selectWayParam.DE_Start2_Count);
		ini.WriteInt(section, "DE_Start3_Count", selectWayParam.DE_Start3_Count);
		ini.WriteInt(section, "DE_End1_Count", selectWayParam.DE_End1_Count);
		ini.WriteInt(section, "DE_End2_Count", selectWayParam.DE_End2_Count);
		ini.WriteInt(section, "DE_End3_Count", selectWayParam.DE_End3_Count);
		ini.WriteInt(section, "DE_Start1_Index", (int)selectWayParam.DE_Start1_Index);
		ini.WriteInt(section, "DE_Start2_Index", (int)selectWayParam.DE_Start2_Index);
		ini.WriteInt(section, "DE_Start3_Index", (int)selectWayParam.DE_Start3_Index);
		ini.WriteInt(section, "DE_End1_Index", (int)selectWayParam.DE_End1_Index);
		ini.WriteInt(section, "DE_End2_Index", (int)selectWayParam.DE_End2_Index);
		ini.WriteInt(section, "DE_End3_Index", (int)selectWayParam.DE_End3_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)selectWayParam.InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)selectWayParam.InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)selectWayParam.InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)selectWayParam.InspRule4_Index);
		ini.WriteDouble(section, "InspRule1_Min", selectWayParam.InspRule1_Min);
		ini.WriteDouble(section, "InspRule2_Min", selectWayParam.InspRule2_Min);
		ini.WriteDouble(section, "InspRule3_Min", selectWayParam.InspRule3_Min);
		ini.WriteDouble(section, "InspRule4_Min", selectWayParam.InspRule4_Min);
		ini.WriteDouble(section, "InspRule1_Max", selectWayParam.InspRule1_Max);
		ini.WriteDouble(section, "InspRule2_Max", selectWayParam.InspRule2_Max);
		ini.WriteDouble(section, "InspRule3_Max", selectWayParam.InspRule3_Max);
		ini.WriteDouble(section, "InspRule4_Max", selectWayParam.InspRule4_Max);
		ini.WriteBool(section, "InspRuleAND", selectWayParam.InspRuleAND);
		ini.WriteBool(section, "Use2WayMode", selectWayParam.Use2WayMode);
		ini.WriteInt(section, "RotateMethod", selectWayParam.RotateMethod);
		ini.WriteInt(section, "RotateMethodB", selectWayParam.RotateMethodB);
		ini.WriteBool(section, "UseAutoSize", selectWayParam.UseAutoSize);
		ini.WriteInt(section, "PatternSelect", selectWayParam.PatternSelect);
		ini.WriteInt(section, "MatchScoreMin", selectWayParam.MatchScoreMin);
		ini.WriteInt(section, "MatchScoreMax", selectWayParam.MatchScoreMax);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadSelectFaceParam(int index)
	{
		SelectFaceParam[index].ValueChangeFlag = false;
		string section = "SelectFaceTool" + index;
		SelectFaceParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		SelectFaceParam[index].RowSelectFace = ini.GetBoolean(section, "RowSelectFace");
		SelectFaceParam[index].RowMinSelectFace = ini.GetBoolean(section, "RowMinSelectFace");
		SelectFaceParam[index].SearchRectT = ini.GetInteger(section, "SearchRectT", 5);
		SelectFaceParam[index].SearchRectB = ini.GetInteger(section, "SearchRectB", 5);
		SelectFaceParam[index].SearchRectL = ini.GetInteger(section, "SearchRectL", 5);
		SelectFaceParam[index].SearchRectR = ini.GetInteger(section, "SearchRectR", 5);
		SelectFaceParam[index].SearchRectWidth = ini.GetInteger(section, "SearchRectWidth", 50);
		SelectFaceParam[index].SearchRectHeight = ini.GetInteger(section, "SearchRectHeight", 50);
		SelectFaceParam[index].THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		SelectFaceParam[index].THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		SelectFaceParam[index].THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		SelectFaceParam[index].THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
		SelectFaceParam[index].THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON", defaultValue: true);
		SelectFaceParam[index].THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON", defaultValue: true);
		SelectFaceParam[index].THD2_ORMode = ini.GetBoolean(section, "THD2_ORMode");
		SelectFaceParam[index].THD3_ORMode = ini.GetBoolean(section, "THD3_ORMode");
		SelectFaceParam[index].THD1_Min = ini.GetInteger(section, "THD1_Min");
		SelectFaceParam[index].THD2_Min = ini.GetInteger(section, "THD2_Min");
		SelectFaceParam[index].THD3_Min = ini.GetInteger(section, "THD3_Min");
		SelectFaceParam[index].THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		SelectFaceParam[index].THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		SelectFaceParam[index].THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		SelectFaceParam[index].DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
		SelectFaceParam[index].DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
		SelectFaceParam[index].DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
		SelectFaceParam[index].DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
		SelectFaceParam[index].DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
		SelectFaceParam[index].DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
		SelectFaceParam[index].DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
		SelectFaceParam[index].DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
		SelectFaceParam[index].DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
		SelectFaceParam[index].DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
		SelectFaceParam[index].DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
		SelectFaceParam[index].DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
		SelectFaceParam[index].InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		SelectFaceParam[index].InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		SelectFaceParam[index].InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		SelectFaceParam[index].InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		SelectFaceParam[index].InspRule1_Min = ini.GetDouble(section, "InspRule1_Min");
		SelectFaceParam[index].InspRule2_Min = ini.GetDouble(section, "InspRule2_Min");
		SelectFaceParam[index].InspRule3_Min = ini.GetDouble(section, "InspRule3_Min");
		SelectFaceParam[index].InspRule4_Min = ini.GetDouble(section, "InspRule4_Min");
		SelectFaceParam[index].InspRule1_Max = ini.GetDouble(section, "InspRule1_Max");
		SelectFaceParam[index].InspRule2_Max = ini.GetDouble(section, "InspRule2_Max");
		SelectFaceParam[index].InspRule3_Max = ini.GetDouble(section, "InspRule3_Max", 300.0);
		SelectFaceParam[index].InspRule4_Max = ini.GetDouble(section, "InspRule4_Max", 300.0);
		SelectFaceParam[index].InspRuleAND = ini.GetBoolean(section, "InspRuleAND", defaultValue: true);
		SelectFaceParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin");
		SelectFaceParam[index].RotateMethod = ini.GetInteger(section, "RotateMethod");
		SelectFaceParam[index].UseAutoSize = ini.GetBoolean(section, "UseAutoSize", defaultValue: true);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteSelectFaceParam(int index)
	{
		string section = "SelectFaceTool" + index;
		if (!SelectFaceParam[index].ValueChangeFlag)
		{
			return true;
		}
		SelectFaceParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", SelectFaceParam[index].TrackerON);
		ini.WriteBool(section, "RowSelectFace", SelectFaceParam[index].RowSelectFace);
		ini.WriteBool(section, "RowMinSelectFace", SelectFaceParam[index].RowMinSelectFace);
		ini.WriteInt(section, "SearchRectT", SelectFaceParam[index].SearchRectT);
		ini.WriteInt(section, "SearchRectB", SelectFaceParam[index].SearchRectB);
		ini.WriteInt(section, "SearchRectL", SelectFaceParam[index].SearchRectL);
		ini.WriteInt(section, "SearchRectR", SelectFaceParam[index].SearchRectR);
		ini.WriteInt(section, "SearchRectWidth", SelectFaceParam[index].SearchRectWidth);
		ini.WriteInt(section, "SearchRectHeight", SelectFaceParam[index].SearchRectHeight);
		ini.WriteInt(section, "THD1_ChannelIndex", (int)SelectFaceParam[index].THD1_ChannelIndex);
		ini.WriteInt(section, "THD2_ChannelIndex", (int)SelectFaceParam[index].THD2_ChannelIndex);
		ini.WriteInt(section, "THD3_ChannelIndex", (int)SelectFaceParam[index].THD3_ChannelIndex);
		ini.WriteBool(section, "THD1_ChannelON", SelectFaceParam[index].THD1_ChannelON);
		ini.WriteBool(section, "THD2_ChannelON", SelectFaceParam[index].THD2_ChannelON);
		ini.WriteBool(section, "THD3_ChannelON", SelectFaceParam[index].THD3_ChannelON);
		ini.WriteBool(section, "THD2_ORMode", SelectFaceParam[index].THD2_ORMode);
		ini.WriteBool(section, "THD3_ORMode", SelectFaceParam[index].THD3_ORMode);
		ini.WriteInt(section, "THD1_Min", SelectFaceParam[index].THD1_Min);
		ini.WriteInt(section, "THD2_Min", SelectFaceParam[index].THD2_Min);
		ini.WriteInt(section, "THD3_Min", SelectFaceParam[index].THD3_Min);
		ini.WriteInt(section, "THD1_Max", SelectFaceParam[index].THD1_Max);
		ini.WriteInt(section, "THD2_Max", SelectFaceParam[index].THD2_Max);
		ini.WriteInt(section, "THD3_Max", SelectFaceParam[index].THD3_Max);
		ini.WriteInt(section, "DE_Start1_Count", SelectFaceParam[index].DE_Start1_Count);
		ini.WriteInt(section, "DE_Start2_Count", SelectFaceParam[index].DE_Start2_Count);
		ini.WriteInt(section, "DE_Start3_Count", SelectFaceParam[index].DE_Start3_Count);
		ini.WriteInt(section, "DE_End1_Count", SelectFaceParam[index].DE_End1_Count);
		ini.WriteInt(section, "DE_End2_Count", SelectFaceParam[index].DE_End2_Count);
		ini.WriteInt(section, "DE_End3_Count", SelectFaceParam[index].DE_End3_Count);
		ini.WriteInt(section, "DE_Start1_Index", (int)SelectFaceParam[index].DE_Start1_Index);
		ini.WriteInt(section, "DE_Start2_Index", (int)SelectFaceParam[index].DE_Start2_Index);
		ini.WriteInt(section, "DE_Start3_Index", (int)SelectFaceParam[index].DE_Start3_Index);
		ini.WriteInt(section, "DE_End1_Index", (int)SelectFaceParam[index].DE_End1_Index);
		ini.WriteInt(section, "DE_End2_Index", (int)SelectFaceParam[index].DE_End2_Index);
		ini.WriteInt(section, "DE_End3_Index", (int)SelectFaceParam[index].DE_End3_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)SelectFaceParam[index].InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)SelectFaceParam[index].InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)SelectFaceParam[index].InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)SelectFaceParam[index].InspRule4_Index);
		ini.WriteDouble(section, "InspRule1_Min", SelectFaceParam[index].InspRule1_Min);
		ini.WriteDouble(section, "InspRule2_Min", SelectFaceParam[index].InspRule2_Min);
		ini.WriteDouble(section, "InspRule3_Min", SelectFaceParam[index].InspRule3_Min);
		ini.WriteDouble(section, "InspRule4_Min", SelectFaceParam[index].InspRule4_Min);
		ini.WriteDouble(section, "InspRule1_Max", SelectFaceParam[index].InspRule1_Max);
		ini.WriteDouble(section, "InspRule2_Max", SelectFaceParam[index].InspRule2_Max);
		ini.WriteDouble(section, "InspRule3_Max", SelectFaceParam[index].InspRule3_Max);
		ini.WriteDouble(section, "InspRule4_Max", SelectFaceParam[index].InspRule4_Max);
		ini.WriteBool(section, "InspRuleAND", SelectFaceParam[index].InspRuleAND);
		ini.WriteInt(section, "RotateMethod", SelectFaceParam[index].RotateMethod);
		ini.WriteInt(section, "ErrorBin", (int)SelectFaceParam[index].ErrorBin);
		ini.WriteBool(section, "UseAutoSize", SelectFaceParam[index].UseAutoSize);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadEnhanceParam(int index)
	{
		if (index >= 6)
		{
			return true;
		}
		EnhanceParam[index].ValueChangeFlag = false;
		string section = "Enhance" + index;
		EnhanceParam[index].FunctionIndex = ini.GetInteger(section, "FunctionIndex");
		EnhanceParam[index].ChannelIndex = ini.GetInteger(section, "ChannelIndex", 1);
		EnhanceParam[index].BrightnessIndex = ini.GetInteger(section, "BrightnessIndex", 1);
		EnhanceParam[index].ContrastIndex = ini.GetInteger(section, "ContrastIndex", 1);
		EnhanceParam[index].GammaIndex = ini.GetInteger(section, "GammaIndex", 1);
		EnhanceParam[index].SSIndex = ini.GetInteger(section, "SSIndex", 1);
		EnhanceParam[index].MixedIndex = ini.GetInteger(section, "MixedIndex", 1);
		EnhanceParam[index].UseSubtract = ini.GetBoolean(section, "UseSubtract");
		EnhanceParam[index].EdgeIndex = ini.GetInteger(section, "EdgeIndex", 1);
		EnhanceParam[index].CannyThd1 = ini.GetInteger(section, "CannyThd1");
		EnhanceParam[index].CannyThd2 = ini.GetInteger(section, "CannyThd2", 255);
		EnhanceParam[index].BrightnessValue = ini.GetDouble(section, "BrightnessValue", 1.0);
		EnhanceParam[index].ContrastValue = ini.GetDouble(section, "ContrastValue");
		EnhanceParam[index].Thd_R_Min = ini.GetInteger(section, "Thd_R_Min");
		EnhanceParam[index].Thd_R_Max = ini.GetInteger(section, "Thd_R_Max", 255);
		EnhanceParam[index].Thd_G_Min = ini.GetInteger(section, "Thd_G_Min");
		EnhanceParam[index].Thd_G_Max = ini.GetInteger(section, "Thd_G_Max", 255);
		EnhanceParam[index].Thd_B_Min = ini.GetInteger(section, "Thd_B_Min");
		EnhanceParam[index].Thd_B_Max = ini.GetInteger(section, "Thd_B_Max", 255);
		EnhanceParam[index].BlockAvgFun = ini.GetInteger(section, "BlockAvgFun");
		EnhanceParam[index].BlockAvgThd = ini.GetInteger(section, "BlockAvgThd", 10);
		EnhanceParam[index].MorphType = ini.GetInteger(section, "MorphType");
		EnhanceParam[index].MorphCount = ini.GetInteger(section, "MorphCount", 1);
		EnhanceParam[index].Custom1 = ini.GetDouble(section, "Custom1", 1.0);
		EnhanceParam[index].Custom2 = ini.GetDouble(section, "Custom2", 1.0);
		EnhanceParam[index].Custom3 = ini.GetDouble(section, "Custom3", 1.0);
		EnhanceParam[index].Custom4 = ini.GetDouble(section, "Custom4", 1.0);
		EnhanceParam[index].Custom5 = ini.GetDouble(section, "Custom5", -8.0);
		EnhanceParam[index].Custom6 = ini.GetDouble(section, "Custom6", 1.0);
		EnhanceParam[index].Custom7 = ini.GetDouble(section, "Custom7", 1.0);
		EnhanceParam[index].Custom8 = ini.GetDouble(section, "Custom8", 1.0);
		EnhanceParam[index].Custom9 = ini.GetDouble(section, "Custom9", 1.0);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteEnhanceParam(int index)
	{
		if (index >= 6)
		{
			return true;
		}
		if (!EnhanceParam[index].ValueChangeFlag)
		{
			return true;
		}
		string section = "Enhance" + index;
		EnhanceParam[index].ValueChangeFlag = false;
		ini.WriteInt(section, "FunctionIndex", EnhanceParam[index].FunctionIndex);
		ini.WriteInt(section, "ChannelIndex", EnhanceParam[index].ChannelIndex);
		ini.WriteInt(section, "BrightnessIndex", EnhanceParam[index].BrightnessIndex);
		ini.WriteInt(section, "ContrastIndex", EnhanceParam[index].ContrastIndex);
		ini.WriteInt(section, "GammaIndex", EnhanceParam[index].GammaIndex);
		ini.WriteInt(section, "SSIndex", EnhanceParam[index].SSIndex);
		ini.WriteInt(section, "MixedIndex", EnhanceParam[index].MixedIndex);
		ini.WriteBool(section, "UseSubtract", EnhanceParam[index].UseSubtract);
		ini.WriteInt(section, "EdgeIndex", EnhanceParam[index].EdgeIndex);
		ini.WriteInt(section, "CannyThd1", EnhanceParam[index].CannyThd1);
		ini.WriteInt(section, "CannyThd2", EnhanceParam[index].CannyThd2);
		ini.WriteDouble(section, "BrightnessValue", EnhanceParam[index].BrightnessValue);
		ini.WriteDouble(section, "ContrastValue", EnhanceParam[index].ContrastValue);
		ini.WriteInt(section, "Thd_R_Min", EnhanceParam[index].Thd_R_Min);
		ini.WriteInt(section, "Thd_R_Max", EnhanceParam[index].Thd_R_Max);
		ini.WriteInt(section, "Thd_G_Min", EnhanceParam[index].Thd_G_Min);
		ini.WriteInt(section, "Thd_G_Max", EnhanceParam[index].Thd_G_Max);
		ini.WriteInt(section, "Thd_B_Min", EnhanceParam[index].Thd_B_Min);
		ini.WriteInt(section, "Thd_B_Max", EnhanceParam[index].Thd_B_Max);
		ini.WriteInt(section, "BlockAvgFun", EnhanceParam[index].BlockAvgFun);
		ini.WriteInt(section, "BlockAvgThd", EnhanceParam[index].BlockAvgThd);
		ini.WriteInt(section, "MorphCount", EnhanceParam[index].MorphCount);
		ini.WriteInt(section, "MorphType", EnhanceParam[index].MorphType);
		ini.WriteDouble(section, "Custom1", EnhanceParam[index].Custom1);
		ini.WriteDouble(section, "Custom2", EnhanceParam[index].Custom2);
		ini.WriteDouble(section, "Custom3", EnhanceParam[index].Custom3);
		ini.WriteDouble(section, "Custom4", EnhanceParam[index].Custom4);
		ini.WriteDouble(section, "Custom5", EnhanceParam[index].Custom5);
		ini.WriteDouble(section, "Custom6", EnhanceParam[index].Custom6);
		ini.WriteDouble(section, "Custom7", EnhanceParam[index].Custom7);
		ini.WriteDouble(section, "Custom8", EnhanceParam[index].Custom8);
		ini.WriteDouble(section, "Custom9", EnhanceParam[index].Custom9);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadColorReferenceParam(int index)
	{
		ColorReferenceParam[index].ValueChangeFlag = false;
		string section = "ColorReference" + index;
		ColorReferenceParam[index].TrackerName = ini.GetValue(section, "TrackerName", "ColorSet");
		ColorReferenceParam[index].THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		ColorReferenceParam[index].THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		ColorReferenceParam[index].THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		ColorReferenceParam[index].THD4_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD4_ChannelIndex", 3);
		ColorReferenceParam[index].THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
		ColorReferenceParam[index].THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON");
		ColorReferenceParam[index].THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON");
		ColorReferenceParam[index].THD4_ChannelON = ini.GetBoolean(section, "THD4_ChannelON");
		ColorReferenceParam[index].THD1_Min = ini.GetInteger(section, "THD1_Min");
		ColorReferenceParam[index].THD2_Min = ini.GetInteger(section, "THD2_Min");
		ColorReferenceParam[index].THD3_Min = ini.GetInteger(section, "THD3_Min");
		ColorReferenceParam[index].THD4_Min = ini.GetInteger(section, "THD4_Min");
		ColorReferenceParam[index].THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		ColorReferenceParam[index].THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		ColorReferenceParam[index].THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		ColorReferenceParam[index].THD4_Max = ini.GetInteger(section, "THD4_Max", 255);
		ColorReferenceParam[index].DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
		ColorReferenceParam[index].DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
		ColorReferenceParam[index].DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
		ColorReferenceParam[index].DE_Start4_Count = ini.GetInteger(section, "DE_Start4_Count");
		ColorReferenceParam[index].DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
		ColorReferenceParam[index].DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
		ColorReferenceParam[index].DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
		ColorReferenceParam[index].DE_End4_Count = ini.GetInteger(section, "DE_End4_Count");
		ColorReferenceParam[index].DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
		ColorReferenceParam[index].DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
		ColorReferenceParam[index].DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
		ColorReferenceParam[index].DE_Start4_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start4_Index");
		ColorReferenceParam[index].DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
		ColorReferenceParam[index].DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
		ColorReferenceParam[index].DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
		ColorReferenceParam[index].DE_End4_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End4_Index");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteColorReferenceParam(int index)
	{
		if (!ColorReferenceParam[index].ValueChangeFlag)
		{
			return true;
		}
		ColorReferenceParam[index].ValueChangeFlag = false;
		string section = "ColorReference" + index;
		ini.WriteString(section, "TrackerName", ColorReferenceParam[index].TrackerName);
		ini.WriteInt(section, "THD1_ChannelIndex", (int)ColorReferenceParam[index].THD1_ChannelIndex);
		ini.WriteInt(section, "THD2_ChannelIndex", (int)ColorReferenceParam[index].THD2_ChannelIndex);
		ini.WriteInt(section, "THD3_ChannelIndex", (int)ColorReferenceParam[index].THD3_ChannelIndex);
		ini.WriteInt(section, "THD4_ChannelIndex", (int)ColorReferenceParam[index].THD4_ChannelIndex);
		ini.WriteBool(section, "THD1_ChannelON", ColorReferenceParam[index].THD1_ChannelON);
		ini.WriteBool(section, "THD2_ChannelON", ColorReferenceParam[index].THD2_ChannelON);
		ini.WriteBool(section, "THD3_ChannelON", ColorReferenceParam[index].THD3_ChannelON);
		ini.WriteBool(section, "THD4_ChannelON", ColorReferenceParam[index].THD4_ChannelON);
		ini.WriteInt(section, "THD1_Min", ColorReferenceParam[index].THD1_Min);
		ini.WriteInt(section, "THD2_Min", ColorReferenceParam[index].THD2_Min);
		ini.WriteInt(section, "THD3_Min", ColorReferenceParam[index].THD3_Min);
		ini.WriteInt(section, "THD4_Min", ColorReferenceParam[index].THD4_Min);
		ini.WriteInt(section, "THD1_Max", ColorReferenceParam[index].THD1_Max);
		ini.WriteInt(section, "THD2_Max", ColorReferenceParam[index].THD2_Max);
		ini.WriteInt(section, "THD3_Max", ColorReferenceParam[index].THD3_Max);
		ini.WriteInt(section, "THD4_Max", ColorReferenceParam[index].THD4_Max);
		ini.WriteInt(section, "DE_Start1_Count", ColorReferenceParam[index].DE_Start1_Count);
		ini.WriteInt(section, "DE_Start2_Count", ColorReferenceParam[index].DE_Start2_Count);
		ini.WriteInt(section, "DE_Start3_Count", ColorReferenceParam[index].DE_Start3_Count);
		ini.WriteInt(section, "DE_Start4_Count", ColorReferenceParam[index].DE_Start4_Count);
		ini.WriteInt(section, "DE_End1_Count", ColorReferenceParam[index].DE_End1_Count);
		ini.WriteInt(section, "DE_End2_Count", ColorReferenceParam[index].DE_End2_Count);
		ini.WriteInt(section, "DE_End3_Count", ColorReferenceParam[index].DE_End3_Count);
		ini.WriteInt(section, "DE_End4_Count", ColorReferenceParam[index].DE_End4_Count);
		ini.WriteInt(section, "DE_Start1_Index", (int)ColorReferenceParam[index].DE_Start1_Index);
		ini.WriteInt(section, "DE_Start2_Index", (int)ColorReferenceParam[index].DE_Start2_Index);
		ini.WriteInt(section, "DE_Start3_Index", (int)ColorReferenceParam[index].DE_Start3_Index);
		ini.WriteInt(section, "DE_Start4_Index", (int)ColorReferenceParam[index].DE_Start4_Index);
		ini.WriteInt(section, "DE_End1_Index", (int)ColorReferenceParam[index].DE_End1_Index);
		ini.WriteInt(section, "DE_End2_Index", (int)ColorReferenceParam[index].DE_End2_Index);
		ini.WriteInt(section, "DE_End3_Index", (int)ColorReferenceParam[index].DE_End3_Index);
		ini.WriteInt(section, "DE_End4_Index", (int)ColorReferenceParam[index].DE_End4_Index);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadAdvLocaeParam(int index)
	{
		string section;
		for (int i = 0; i < 2; i++)
		{
			INSP_SCANLINETOOL_PARAM iNSP_SCANLINETOOL_PARAM = new INSP_SCANLINETOOL_PARAM
			{
				ValueChangeFlag = false,
				NameChangeFlag = false
			};
			section = ((i != 0) ? ("HLocateTools" + index) : ("VLocateTools" + index));
			iNSP_SCANLINETOOL_PARAM.TrackerON = ini.GetBoolean(section, "TrackerON");
			if (index < 2)
			{
				iNSP_SCANLINETOOL_PARAM.TrackerON = true;
			}
			iNSP_SCANLINETOOL_PARAM.TrackerName = ini.GetValue(section, "TrackerName", "Unused");
			iNSP_SCANLINETOOL_PARAM.Locked = ini.GetBoolean(section, "Locked");
			iNSP_SCANLINETOOL_PARAM.SearchRectT = ini.GetInteger(section, "SearchRectT", 5);
			iNSP_SCANLINETOOL_PARAM.SearchRectB = ini.GetInteger(section, "SearchRectB", 5);
			iNSP_SCANLINETOOL_PARAM.SearchRectL = ini.GetInteger(section, "SearchRectL", 5);
			iNSP_SCANLINETOOL_PARAM.SearchRectR = ini.GetInteger(section, "SearchRectR", 5);
			iNSP_SCANLINETOOL_PARAM.SearchRectWidth = ini.GetInteger(section, "SearchRectWidth", 50);
			iNSP_SCANLINETOOL_PARAM.SearchRectHeight = ini.GetInteger(section, "SearchRectHeight", 50);
			iNSP_SCANLINETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
			iNSP_SCANLINETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
			iNSP_SCANLINETOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
			iNSP_SCANLINETOOL_PARAM.THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
			iNSP_SCANLINETOOL_PARAM.THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON");
			iNSP_SCANLINETOOL_PARAM.THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON");
			iNSP_SCANLINETOOL_PARAM.THD1_Min = ini.GetInteger(section, "THD1_Min");
			iNSP_SCANLINETOOL_PARAM.THD2_Min = ini.GetInteger(section, "THD2_Min");
			iNSP_SCANLINETOOL_PARAM.THD3_Min = ini.GetInteger(section, "THD3_Min");
			iNSP_SCANLINETOOL_PARAM.THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
			iNSP_SCANLINETOOL_PARAM.THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
			iNSP_SCANLINETOOL_PARAM.THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
			iNSP_SCANLINETOOL_PARAM.DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
			iNSP_SCANLINETOOL_PARAM.DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
			iNSP_SCANLINETOOL_PARAM.DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
			iNSP_SCANLINETOOL_PARAM.DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
			iNSP_SCANLINETOOL_PARAM.DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
			iNSP_SCANLINETOOL_PARAM.DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
			iNSP_SCANLINETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
			iNSP_SCANLINETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
			iNSP_SCANLINETOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
			iNSP_SCANLINETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
			iNSP_SCANLINETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
			iNSP_SCANLINETOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
			iNSP_SCANLINETOOL_PARAM.ParetoIndex = ini.GetInteger(section, "ParetoIndex");
			iNSP_SCANLINETOOL_PARAM.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
			iNSP_SCANLINETOOL_PARAM.FaceIndex = ini.GetInteger(section, "FaceIndex");
			iNSP_SCANLINETOOL_PARAM.UseStdFilter = ini.GetBoolean(section, "UseStdFilter");
			iNSP_SCANLINETOOL_PARAM.UseColorInverse = ini.GetBoolean(section, "UseColorInverse");
			iNSP_SCANLINETOOL_PARAM.FindEdgeMethod = ini.GetInteger(section, "FindEdgeMethod");
			iNSP_SCANLINETOOL_PARAM.Value = ini.GetInteger(section, "Value", -1);
			if (i == 0)
			{
				iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)(ini.GetInteger(section, "Direction") % 2);
			}
			else
			{
				int integer = ini.GetInteger(section, "Direction", 2);
				if (integer < 4)
				{
					iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)(2 + integer % 2);
				}
				else
				{
					iNSP_SCANLINETOOL_PARAM.Direction = (ScanDirection)integer;
				}
			}
			iNSP_SCANLINETOOL_PARAM.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
			iNSP_SCANLINETOOL_PARAM.UseAutoSize = ini.GetBoolean(section, "UseAutoSize", defaultValue: true);
			iNSP_SCANLINETOOL_PARAM.FollowMode = ini.GetInteger(section, "FollowMode");
			if (i == 0)
			{
				VSLocateParam[index] = iNSP_SCANLINETOOL_PARAM;
			}
			else
			{
				HSLocateParam[index] = iNSP_SCANLINETOOL_PARAM;
			}
		}
		INSP_ADVLOCATETOOL_PARAM iNSP_ADVLOCATETOOL_PARAM = new INSP_ADVLOCATETOOL_PARAM
		{
			ValueChangeFlag = false,
			NameChangeFlag = false
		};
		section = "AdvLocateTools" + index;
		iNSP_ADVLOCATETOOL_PARAM.TrackerON = ini.GetBoolean(section, "TrackerON");
		if (index < 1)
		{
			iNSP_ADVLOCATETOOL_PARAM.TrackerON = true;
		}
		iNSP_ADVLOCATETOOL_PARAM.Locked = ini.GetBoolean(section, "Locked");
		iNSP_ADVLOCATETOOL_PARAM.TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		iNSP_ADVLOCATETOOL_PARAM.RectIndexT = ini.GetInteger(section, "RectIndexT");
		iNSP_ADVLOCATETOOL_PARAM.RectIndexB = ini.GetInteger(section, "RectIndexB", 1);
		iNSP_ADVLOCATETOOL_PARAM.RectIndexL = ini.GetInteger(section, "RectIndexL");
		iNSP_ADVLOCATETOOL_PARAM.RectIndexR = ini.GetInteger(section, "RectIndexR", 1);
		iNSP_ADVLOCATETOOL_PARAM.WidthMax = ini.GetDouble(section, "WidthMax");
		iNSP_ADVLOCATETOOL_PARAM.WidthMin = ini.GetDouble(section, "WidthMin");
		iNSP_ADVLOCATETOOL_PARAM.HeightMax = ini.GetDouble(section, "HeightMax");
		iNSP_ADVLOCATETOOL_PARAM.HeightMin = ini.GetDouble(section, "HeightMin");
		iNSP_ADVLOCATETOOL_PARAM.EnableFakeWidthRatio = ini.GetBoolean(section, "EnableFakeWidthRatio");
		iNSP_ADVLOCATETOOL_PARAM.EnableFakeHeightRatio = ini.GetBoolean(section, "EnableFakeHeightRatio");
		iNSP_ADVLOCATETOOL_PARAM.WidthRatio = ini.GetDouble(section, "WidthRatio", 1.0);
		iNSP_ADVLOCATETOOL_PARAM.HeightRatio = ini.GetDouble(section, "HeightRatio", 1.0);
		iNSP_ADVLOCATETOOL_PARAM.UseBinary = ini.GetBoolean(section, "UseBinary");
		iNSP_ADVLOCATETOOL_PARAM.SearchRectT = ini.GetInteger(section, "SearchRectT", 5);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectL = ini.GetInteger(section, "SearchRectL", 5);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectB = ini.GetInteger(section, "SearchRectB", 5);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectR = ini.GetInteger(section, "SearchRectR", 5);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectW = ini.GetInteger(section, "SearchRectW", 50);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectH = ini.GetInteger(section, "SearchRectH", 50);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftT = ini.GetDouble(section, "SearchRectShiftT");
		iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftB = ini.GetDouble(section, "SearchRectShiftB");
		iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftL = ini.GetDouble(section, "SearchRectShiftL");
		iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftR = ini.GetDouble(section, "SearchRectShiftR");
		iNSP_ADVLOCATETOOL_PARAM.SearchRectWidth = ini.GetInteger(section, "SearchRectWidth", 50);
		iNSP_ADVLOCATETOOL_PARAM.SearchRectHeight = ini.GetInteger(section, "SearchRectHeight", 50);
		iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
		iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON");
		iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON");
		iNSP_ADVLOCATETOOL_PARAM.THD1_Min = ini.GetInteger(section, "THD1_Min");
		iNSP_ADVLOCATETOOL_PARAM.THD2_Min = ini.GetInteger(section, "THD2_Min");
		iNSP_ADVLOCATETOOL_PARAM.THD3_Min = ini.GetInteger(section, "THD3_Min");
		iNSP_ADVLOCATETOOL_PARAM.THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		iNSP_ADVLOCATETOOL_PARAM.THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		iNSP_ADVLOCATETOOL_PARAM.THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
		iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
		iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
		iNSP_ADVLOCATETOOL_PARAM.DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
		iNSP_ADVLOCATETOOL_PARAM.DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
		iNSP_ADVLOCATETOOL_PARAM.DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
		iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
		iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
		iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
		iNSP_ADVLOCATETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
		iNSP_ADVLOCATETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
		iNSP_ADVLOCATETOOL_PARAM.DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
		iNSP_ADVLOCATETOOL_PARAM.ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		iNSP_ADVLOCATETOOL_PARAM.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		iNSP_ADVLOCATETOOL_PARAM.FaceIndex = ini.GetInteger(section, "FaceIndex");
		iNSP_ADVLOCATETOOL_PARAM.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		iNSP_ADVLOCATETOOL_PARAM.UseAutoSize = ini.GetBoolean(section, "UseAutoSize", defaultValue: true);
		iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex = ini.GetInteger(section, "AutoSizeIndex");
		AdvLocateParam[index] = iNSP_ADVLOCATETOOL_PARAM;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteAdvLocaeParam(int index)
	{
		string text = "";
		INSP_SCANLINETOOL_PARAM iNSP_SCANLINETOOL_PARAM = default(INSP_SCANLINETOOL_PARAM);
		for (int i = 0; i < 2; i++)
		{
			if (i == 0)
			{
				iNSP_SCANLINETOOL_PARAM = VSLocateParam[index];
				text = "VLocateTools" + index;
			}
			else
			{
				iNSP_SCANLINETOOL_PARAM = HSLocateParam[index];
				text = "HLocateTools" + index;
			}
			if (iNSP_SCANLINETOOL_PARAM.NameChangeFlag)
			{
				ini.WriteString(text, "TrackerName", iNSP_SCANLINETOOL_PARAM.TrackerName);
				iNSP_SCANLINETOOL_PARAM.NameChangeFlag = false;
			}
			if (iNSP_SCANLINETOOL_PARAM.ValueChangeFlag)
			{
				iNSP_SCANLINETOOL_PARAM.ValueChangeFlag = false;
				ini.WriteBool(text, "TrackerON", iNSP_SCANLINETOOL_PARAM.TrackerON);
				ini.WriteString(text, "TrackerName", iNSP_SCANLINETOOL_PARAM.TrackerName);
				ini.WriteBool(text, "Locked", iNSP_SCANLINETOOL_PARAM.Locked);
				ini.WriteInt(text, "SearchRectT", iNSP_SCANLINETOOL_PARAM.SearchRectT);
				ini.WriteInt(text, "SearchRectB", iNSP_SCANLINETOOL_PARAM.SearchRectB);
				ini.WriteInt(text, "SearchRectL", iNSP_SCANLINETOOL_PARAM.SearchRectL);
				ini.WriteInt(text, "SearchRectR", iNSP_SCANLINETOOL_PARAM.SearchRectR);
				ini.WriteInt(text, "SearchRectWidth", iNSP_SCANLINETOOL_PARAM.SearchRectWidth);
				ini.WriteInt(text, "SearchRectHeight", iNSP_SCANLINETOOL_PARAM.SearchRectHeight);
				ini.WriteInt(text, "THD1_ChannelIndex", (int)iNSP_SCANLINETOOL_PARAM.THD1_ChannelIndex);
				ini.WriteInt(text, "THD2_ChannelIndex", (int)iNSP_SCANLINETOOL_PARAM.THD2_ChannelIndex);
				ini.WriteInt(text, "THD3_ChannelIndex", (int)iNSP_SCANLINETOOL_PARAM.THD3_ChannelIndex);
				ini.WriteBool(text, "THD1_ChannelON", iNSP_SCANLINETOOL_PARAM.THD1_ChannelON);
				ini.WriteBool(text, "THD2_ChannelON", iNSP_SCANLINETOOL_PARAM.THD2_ChannelON);
				ini.WriteBool(text, "THD3_ChannelON", iNSP_SCANLINETOOL_PARAM.THD3_ChannelON);
				ini.WriteInt(text, "THD1_Min", iNSP_SCANLINETOOL_PARAM.THD1_Min);
				ini.WriteInt(text, "THD2_Min", iNSP_SCANLINETOOL_PARAM.THD2_Min);
				ini.WriteInt(text, "THD3_Min", iNSP_SCANLINETOOL_PARAM.THD3_Min);
				ini.WriteInt(text, "THD1_Max", iNSP_SCANLINETOOL_PARAM.THD1_Max);
				ini.WriteInt(text, "THD2_Max", iNSP_SCANLINETOOL_PARAM.THD2_Max);
				ini.WriteInt(text, "THD3_Max", iNSP_SCANLINETOOL_PARAM.THD3_Max);
				ini.WriteInt(text, "DE_Start1_Count", iNSP_SCANLINETOOL_PARAM.DE_Start1_Count);
				ini.WriteInt(text, "DE_Start2_Count", iNSP_SCANLINETOOL_PARAM.DE_Start2_Count);
				ini.WriteInt(text, "DE_Start3_Count", iNSP_SCANLINETOOL_PARAM.DE_Start3_Count);
				ini.WriteInt(text, "DE_End1_Count", iNSP_SCANLINETOOL_PARAM.DE_End1_Count);
				ini.WriteInt(text, "DE_End2_Count", iNSP_SCANLINETOOL_PARAM.DE_End2_Count);
				ini.WriteInt(text, "DE_End3_Count", iNSP_SCANLINETOOL_PARAM.DE_End3_Count);
				ini.WriteInt(text, "DE_Start1_Index", (int)iNSP_SCANLINETOOL_PARAM.DE_Start1_Index);
				ini.WriteInt(text, "DE_Start2_Index", (int)iNSP_SCANLINETOOL_PARAM.DE_Start2_Index);
				ini.WriteInt(text, "DE_Start3_Index", (int)iNSP_SCANLINETOOL_PARAM.DE_Start3_Index);
				ini.WriteInt(text, "DE_End1_Index", (int)iNSP_SCANLINETOOL_PARAM.DE_End1_Index);
				ini.WriteInt(text, "DE_End2_Index", (int)iNSP_SCANLINETOOL_PARAM.DE_End2_Index);
				ini.WriteInt(text, "DE_End3_Index", (int)iNSP_SCANLINETOOL_PARAM.DE_End3_Index);
				ini.WriteInt(text, "ParetoIndex", iNSP_SCANLINETOOL_PARAM.ParetoIndex);
				ini.WriteBool(text, "EnableSelectFace", iNSP_SCANLINETOOL_PARAM.EnableSelectFace);
				ini.WriteInt(text, "FaceIndex", iNSP_SCANLINETOOL_PARAM.FaceIndex);
				ini.WriteInt(text, "FindEdgeMethod", iNSP_SCANLINETOOL_PARAM.FindEdgeMethod);
				ini.WriteInt(text, "Direction", (int)iNSP_SCANLINETOOL_PARAM.Direction);
				ini.WriteBool(text, "UseStdFilter", iNSP_SCANLINETOOL_PARAM.UseStdFilter);
				ini.WriteBool(text, "UseColorInverse", iNSP_SCANLINETOOL_PARAM.UseColorInverse);
				ini.WriteInt(text, "Value", -1);
				ini.WriteBool(text, "UseAutoSize", iNSP_SCANLINETOOL_PARAM.UseAutoSize);
				ini.WriteInt(text, "FollowMode", iNSP_SCANLINETOOL_PARAM.FollowMode);
				ini.WriteInt(text, "ErrorBin", (int)iNSP_SCANLINETOOL_PARAM.ErrorBin);
			}
		}
		INSP_ADVLOCATETOOL_PARAM iNSP_ADVLOCATETOOL_PARAM = default(INSP_ADVLOCATETOOL_PARAM);
		iNSP_ADVLOCATETOOL_PARAM = AdvLocateParam[index];
		text = "AdvLocateTools" + index;
		if (iNSP_ADVLOCATETOOL_PARAM.NameChangeFlag)
		{
			ini.WriteString(text, "TrackerName", iNSP_ADVLOCATETOOL_PARAM.TrackerName);
			iNSP_ADVLOCATETOOL_PARAM.NameChangeFlag = false;
		}
		if (!iNSP_ADVLOCATETOOL_PARAM.ValueChangeFlag)
		{
			return true;
		}
		iNSP_ADVLOCATETOOL_PARAM.ValueChangeFlag = false;
		ini.WriteBool(text, "TrackerON", iNSP_ADVLOCATETOOL_PARAM.TrackerON);
		ini.WriteString(text, "TrackerName", iNSP_ADVLOCATETOOL_PARAM.TrackerName);
		ini.WriteBool(text, "Locked", iNSP_ADVLOCATETOOL_PARAM.Locked);
		ini.WriteInt(text, "RectIndexT", iNSP_ADVLOCATETOOL_PARAM.RectIndexT);
		ini.WriteInt(text, "RectIndexB", iNSP_ADVLOCATETOOL_PARAM.RectIndexB);
		ini.WriteInt(text, "RectIndexL", iNSP_ADVLOCATETOOL_PARAM.RectIndexL);
		ini.WriteInt(text, "RectIndexR", iNSP_ADVLOCATETOOL_PARAM.RectIndexR);
		ini.WriteDouble(text, "WidthMax", iNSP_ADVLOCATETOOL_PARAM.WidthMax);
		ini.WriteDouble(text, "WidthMin", iNSP_ADVLOCATETOOL_PARAM.WidthMin);
		ini.WriteDouble(text, "HeightMax", iNSP_ADVLOCATETOOL_PARAM.HeightMax);
		ini.WriteDouble(text, "HeightMin", iNSP_ADVLOCATETOOL_PARAM.HeightMin);
		ini.WriteBool(text, "EnableFakeWidthRatio", iNSP_ADVLOCATETOOL_PARAM.EnableFakeWidthRatio);
		ini.WriteBool(text, "EnableFakeHeightRatio", iNSP_ADVLOCATETOOL_PARAM.EnableFakeHeightRatio);
		ini.WriteDouble(text, "WidthRatio", iNSP_ADVLOCATETOOL_PARAM.WidthRatio);
		ini.WriteDouble(text, "HeightRatio", iNSP_ADVLOCATETOOL_PARAM.HeightRatio);
		ini.WriteBool(text, "UseBinary", iNSP_ADVLOCATETOOL_PARAM.UseBinary);
		ini.WriteInt(text, "SearchRectT", iNSP_ADVLOCATETOOL_PARAM.SearchRectT);
		ini.WriteInt(text, "SearchRectL", iNSP_ADVLOCATETOOL_PARAM.SearchRectL);
		ini.WriteInt(text, "SearchRectB", iNSP_ADVLOCATETOOL_PARAM.SearchRectB);
		ini.WriteInt(text, "SearchRectR", iNSP_ADVLOCATETOOL_PARAM.SearchRectR);
		ini.WriteInt(text, "SearchRectW", iNSP_ADVLOCATETOOL_PARAM.SearchRectW);
		ini.WriteInt(text, "SearchRectH", iNSP_ADVLOCATETOOL_PARAM.SearchRectH);
		ini.WriteDouble(text, "SearchRectShiftT", iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftT);
		ini.WriteDouble(text, "SearchRectShiftB", iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftB);
		ini.WriteDouble(text, "SearchRectShiftL", iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftL);
		ini.WriteDouble(text, "SearchRectShiftR", iNSP_ADVLOCATETOOL_PARAM.SearchRectShiftR);
		ini.WriteInt(text, "SearchRectWidth", iNSP_ADVLOCATETOOL_PARAM.SearchRectWidth);
		ini.WriteInt(text, "SearchRectHeight", iNSP_ADVLOCATETOOL_PARAM.SearchRectHeight);
		ini.WriteInt(text, "THD1_ChannelIndex", (int)iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelIndex);
		ini.WriteInt(text, "THD2_ChannelIndex", (int)iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelIndex);
		ini.WriteInt(text, "THD3_ChannelIndex", (int)iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelIndex);
		ini.WriteBool(text, "THD1_ChannelON", iNSP_ADVLOCATETOOL_PARAM.THD1_ChannelON);
		ini.WriteBool(text, "THD2_ChannelON", iNSP_ADVLOCATETOOL_PARAM.THD2_ChannelON);
		ini.WriteBool(text, "THD3_ChannelON", iNSP_ADVLOCATETOOL_PARAM.THD3_ChannelON);
		ini.WriteInt(text, "THD1_Min", iNSP_ADVLOCATETOOL_PARAM.THD1_Min);
		ini.WriteInt(text, "THD2_Min", iNSP_ADVLOCATETOOL_PARAM.THD2_Min);
		ini.WriteInt(text, "THD3_Min", iNSP_ADVLOCATETOOL_PARAM.THD3_Min);
		ini.WriteInt(text, "THD1_Max", iNSP_ADVLOCATETOOL_PARAM.THD1_Max);
		ini.WriteInt(text, "THD2_Max", iNSP_ADVLOCATETOOL_PARAM.THD2_Max);
		ini.WriteInt(text, "THD3_Max", iNSP_ADVLOCATETOOL_PARAM.THD3_Max);
		ini.WriteInt(text, "DE_Start1_Count", iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Count);
		ini.WriteInt(text, "DE_Start2_Count", iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Count);
		ini.WriteInt(text, "DE_Start3_Count", iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Count);
		ini.WriteInt(text, "DE_End1_Count", iNSP_ADVLOCATETOOL_PARAM.DE_End1_Count);
		ini.WriteInt(text, "DE_End2_Count", iNSP_ADVLOCATETOOL_PARAM.DE_End2_Count);
		ini.WriteInt(text, "DE_End3_Count", iNSP_ADVLOCATETOOL_PARAM.DE_End3_Count);
		ini.WriteInt(text, "DE_Start1_Index", (int)iNSP_ADVLOCATETOOL_PARAM.DE_Start1_Index);
		ini.WriteInt(text, "DE_Start2_Index", (int)iNSP_ADVLOCATETOOL_PARAM.DE_Start2_Index);
		ini.WriteInt(text, "DE_Start3_Index", (int)iNSP_ADVLOCATETOOL_PARAM.DE_Start3_Index);
		ini.WriteInt(text, "DE_End1_Index", (int)iNSP_ADVLOCATETOOL_PARAM.DE_End1_Index);
		ini.WriteInt(text, "DE_End2_Index", (int)iNSP_ADVLOCATETOOL_PARAM.DE_End2_Index);
		ini.WriteInt(text, "DE_End3_Index", (int)iNSP_ADVLOCATETOOL_PARAM.DE_End3_Index);
		ini.WriteInt(text, "ParetoIndex", iNSP_ADVLOCATETOOL_PARAM.ParetoIndex);
		ini.WriteBool(text, "EnableSelectFace", iNSP_ADVLOCATETOOL_PARAM.EnableSelectFace);
		ini.WriteInt(text, "FaceIndex", iNSP_ADVLOCATETOOL_PARAM.FaceIndex);
		ini.WriteBool(text, "UseAutoSize", iNSP_ADVLOCATETOOL_PARAM.UseAutoSize);
		ini.WriteInt(text, "AutoSizeIndex", iNSP_ADVLOCATETOOL_PARAM.AutoSizeIndex);
		ini.WriteInt(text, "ErrorBin", (int)iNSP_ADVLOCATETOOL_PARAM.ErrorBin);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadAdvMeasureParam(int index)
	{
		AdvLocateMParam[index].ValueChangeFlag = false;
		AdvLocateMParam[index].NameChangeFlag = false;
		string section = "AdvMeasure" + index;
		AdvLocateMParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		AdvLocateMParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		AdvLocateMParam[index].Locked = ini.GetBoolean(section, "Locked");
		AdvLocateMParam[index].MeasureFunction = ini.GetInteger(section, "MeasureFunction");
		AdvLocateMParam[index].MStartSelect = ini.GetInteger(section, "MStartSelect");
		AdvLocateMParam[index].MEndSelect = ini.GetInteger(section, "MEndSelect");
		AdvLocateMParam[index].MStartAPt = ini.GetInteger(section, "MStartAPt");
		AdvLocateMParam[index].MEndAPt = ini.GetInteger(section, "MEndAPt");
		AdvLocateMParam[index].MeasureAllX = ini.GetBoolean(section, "MeasureAllX");
		AdvLocateMParam[index].MeasureAllY = ini.GetBoolean(section, "MeasureAllY");
		AdvLocateMParam[index].ScanExtend = ini.GetInteger(section, "ScanExtend", 15);
		AdvLocateMParam[index].ScanShrink = ini.GetInteger(section, "ScanShrink");
		AdvLocateMParam[index].MStartWHIndex = ini.GetInteger(section, "MStartWHIndex");
		AdvLocateMParam[index].MEndWHIndex = ini.GetInteger(section, "MEndWHIndex");
		AdvLocateMParam[index].MStartAIndex = ini.GetInteger(section, "MStartAIndex");
		AdvLocateMParam[index].MEndAIndex = ini.GetInteger(section, "MEndAIndex");
		AdvLocateMParam[index].ValueMin = ini.GetDouble(section, "ValueMin");
		AdvLocateMParam[index].ValueMax = ini.GetDouble(section, "ValueMax");
		AdvLocateMParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		AdvLocateMParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		AdvLocateMParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		AdvLocateMParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteAdvMeasureParam(int index)
	{
		string section = "AdvMeasure" + index;
		if (AdvLocateMParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", AdvLocateMParam[index].TrackerName);
			AdvLocateMParam[index].NameChangeFlag = false;
		}
		if (!AdvLocateMParam[index].ValueChangeFlag)
		{
			return true;
		}
		AdvLocateMParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", AdvLocateMParam[index].TrackerON);
		ini.WriteString(section, "TrackerName", AdvLocateMParam[index].TrackerName);
		ini.WriteBool(section, "Locked", AdvLocateMParam[index].Locked);
		ini.WriteInt(section, "MeasureFunction", AdvLocateMParam[index].MeasureFunction);
		ini.WriteInt(section, "MStartSelect", AdvLocateMParam[index].MStartSelect);
		ini.WriteInt(section, "MEndSelect", AdvLocateMParam[index].MEndSelect);
		ini.WriteInt(section, "ScanExtend", AdvLocateMParam[index].ScanExtend);
		ini.WriteInt(section, "ScanShrink", AdvLocateMParam[index].ScanShrink);
		ini.WriteBool(section, "MeasureAllX", AdvLocateMParam[index].MeasureAllX);
		ini.WriteBool(section, "MeasureAllY", AdvLocateMParam[index].MeasureAllY);
		ini.WriteInt(section, "MStartWHIndex", AdvLocateMParam[index].MStartWHIndex);
		ini.WriteInt(section, "MEndWHIndex", AdvLocateMParam[index].MEndWHIndex);
		ini.WriteInt(section, "MStartAIndex", AdvLocateMParam[index].MStartAIndex);
		ini.WriteInt(section, "MEndAIndex", AdvLocateMParam[index].MEndAIndex);
		ini.WriteInt(section, "MStartAPt", AdvLocateMParam[index].MStartAPt);
		ini.WriteInt(section, "MEndAPt", AdvLocateMParam[index].MEndAPt);
		ini.WriteDouble(section, "ValueMin", AdvLocateMParam[index].ValueMin);
		ini.WriteDouble(section, "ValueMax", AdvLocateMParam[index].ValueMax);
		ini.WriteInt(section, "ParetoIndex", AdvLocateMParam[index].ParetoIndex);
		ini.WriteBool(section, "EnableSelectFace", AdvLocateMParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", AdvLocateMParam[index].FaceIndex);
		ini.WriteInt(section, "ErrorBin", (int)AdvLocateMParam[index].ErrorBin);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadMeasureParam(int index)
	{
		for (int i = 0; i < 2; i++)
		{
			INSP_MEASURETOOL_PARAM iNSP_MEASURETOOL_PARAM = new INSP_MEASURETOOL_PARAM
			{
				ValueChangeFlag = false,
				NameChangeFlag = false
			};
			string section = ((i != 0) ? ("MeaHeightTools" + index) : ("MeaWidthTools" + index));
			iNSP_MEASURETOOL_PARAM.TrackerON = ini.GetBoolean(section, "TrackerON");
			iNSP_MEASURETOOL_PARAM.TrackerName = ini.GetValue(section, "TrackerName", "Unused");
			iNSP_MEASURETOOL_PARAM.Locked = ini.GetBoolean(section, "Locked");
			iNSP_MEASURETOOL_PARAM.SearchRect1T = ini.GetInteger(section, "SearchRect1T");
			iNSP_MEASURETOOL_PARAM.SearchRect1B = ini.GetInteger(section, "SearchRect1B");
			iNSP_MEASURETOOL_PARAM.SearchRect1L = ini.GetInteger(section, "SearchRect1L");
			iNSP_MEASURETOOL_PARAM.SearchRect1R = ini.GetInteger(section, "SearchRect1R", 50);
			iNSP_MEASURETOOL_PARAM.SearchRect2T = ini.GetInteger(section, "SearchRect2T", 50);
			iNSP_MEASURETOOL_PARAM.SearchRect2B = ini.GetInteger(section, "SearchRect2B");
			iNSP_MEASURETOOL_PARAM.SearchRect2L = ini.GetInteger(section, "SearchRect2L", 50);
			iNSP_MEASURETOOL_PARAM.SearchRect2R = ini.GetInteger(section, "SearchRect2R", 50);
			iNSP_MEASURETOOL_PARAM.THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
			iNSP_MEASURETOOL_PARAM.THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
			iNSP_MEASURETOOL_PARAM.THD1_Min = ini.GetInteger(section, "THD1_Min");
			iNSP_MEASURETOOL_PARAM.THD2_Min = ini.GetInteger(section, "THD2_Min");
			iNSP_MEASURETOOL_PARAM.THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
			iNSP_MEASURETOOL_PARAM.THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
			iNSP_MEASURETOOL_PARAM.EnableRGBThdA = ini.GetBoolean(section, "EnableRGBThdA");
			iNSP_MEASURETOOL_PARAM.EnableRGBThdB = ini.GetBoolean(section, "EnableRGBThdB");
			iNSP_MEASURETOOL_PARAM.THD_AR_Min = ini.GetInteger(section, "THD_AR_Min");
			iNSP_MEASURETOOL_PARAM.THD_AR_Max = ini.GetInteger(section, "THD_AR_Max", 255);
			iNSP_MEASURETOOL_PARAM.THD_AG_Min = ini.GetInteger(section, "THD_AG_Min");
			iNSP_MEASURETOOL_PARAM.THD_AG_Max = ini.GetInteger(section, "THD_AG_Max", 255);
			iNSP_MEASURETOOL_PARAM.THD_AB_Min = ini.GetInteger(section, "THD_AB_Min");
			iNSP_MEASURETOOL_PARAM.THD_AB_Max = ini.GetInteger(section, "THD_AB_Max", 255);
			iNSP_MEASURETOOL_PARAM.THD_BR_Min = ini.GetInteger(section, "THD_BR_Min");
			iNSP_MEASURETOOL_PARAM.THD_BR_Max = ini.GetInteger(section, "THD_BR_Max", 255);
			iNSP_MEASURETOOL_PARAM.THD_BG_Min = ini.GetInteger(section, "THD_BG_Min");
			iNSP_MEASURETOOL_PARAM.THD_BG_Max = ini.GetInteger(section, "THD_BG_Max", 255);
			iNSP_MEASURETOOL_PARAM.THD_BB_Min = ini.GetInteger(section, "THD_BB_Min");
			iNSP_MEASURETOOL_PARAM.THD_BB_Max = ini.GetInteger(section, "THD_BB_Max", 255);
			iNSP_MEASURETOOL_PARAM.DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
			iNSP_MEASURETOOL_PARAM.DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
			iNSP_MEASURETOOL_PARAM.DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
			iNSP_MEASURETOOL_PARAM.DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
			iNSP_MEASURETOOL_PARAM.DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
			iNSP_MEASURETOOL_PARAM.DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
			iNSP_MEASURETOOL_PARAM.DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
			iNSP_MEASURETOOL_PARAM.DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
			if (i == 0)
			{
				iNSP_MEASURETOOL_PARAM.Direction1 = (ScanDirection)(ini.GetInteger(section, "Direction1") % 2);
				iNSP_MEASURETOOL_PARAM.Direction2 = (ScanDirection)(ini.GetInteger(section, "Direction2", 1) % 2);
			}
			else
			{
				iNSP_MEASURETOOL_PARAM.Direction1 = (ScanDirection)(2 + ini.GetInteger(section, "Direction1", 2) % 2);
				iNSP_MEASURETOOL_PARAM.Direction2 = (ScanDirection)(2 + ini.GetInteger(section, "Direction2", 3) % 2);
			}
			iNSP_MEASURETOOL_PARAM.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
			iNSP_MEASURETOOL_PARAM.Scan1Rule = (MeasureCondition)ini.GetInteger(section, "Scan1Rule");
			iNSP_MEASURETOOL_PARAM.Scan2Rule = (MeasureCondition)ini.GetInteger(section, "Scan2Rule");
			iNSP_MEASURETOOL_PARAM.SinglePtLengthMin = ini.GetDouble(section, "SinglePtLengthMin");
			iNSP_MEASURETOOL_PARAM.SinglePtLengthMax = ini.GetDouble(section, "SinglePtLengthMax", 300.0);
			iNSP_MEASURETOOL_PARAM.LengthMin = ini.GetDouble(section, "LengthMin");
			iNSP_MEASURETOOL_PARAM.LengthMax = ini.GetDouble(section, "LengthMax", 300.0);
			iNSP_MEASURETOOL_PARAM.LengthDiffMax = ini.GetDouble(section, "LengthDiffMax");
			iNSP_MEASURETOOL_PARAM.ContinueError = ini.GetInteger(section, "ContinueError", 1);
			iNSP_MEASURETOOL_PARAM.MaxError = ini.GetInteger(section, "MaxError", 1);
			iNSP_MEASURETOOL_PARAM.UseSinglePt = ini.GetBoolean(section, "UseSinglePt");
			iNSP_MEASURETOOL_PARAM.ParetoIndex = ini.GetInteger(section, "ParetoIndex");
			iNSP_MEASURETOOL_PARAM.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
			iNSP_MEASURETOOL_PARAM.FaceIndex = ini.GetInteger(section, "FaceIndex");
			iNSP_MEASURETOOL_PARAM.CheckEveryone = ini.GetBoolean(section, "CheckEveryone");
			iNSP_MEASURETOOL_PARAM.MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
			iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Count = ini.GetInteger(section, "MaskDE_Start1_Count");
			iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MaskDE_Start1_Index");
			if (i == 0)
			{
				MeaWidthParam[index] = iNSP_MEASURETOOL_PARAM;
			}
			else
			{
				MeaHeightParam[index] = iNSP_MEASURETOOL_PARAM;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteMeasureParam(int index)
	{
		string text = "";
		INSP_MEASURETOOL_PARAM iNSP_MEASURETOOL_PARAM = default(INSP_MEASURETOOL_PARAM);
		for (int i = 0; i < 2; i++)
		{
			if (i == 0)
			{
				iNSP_MEASURETOOL_PARAM = MeaWidthParam[index];
				text = "MeaWidthTools" + index;
			}
			else
			{
				iNSP_MEASURETOOL_PARAM = MeaHeightParam[index];
				text = "MeaHeightTools" + index;
			}
			if (iNSP_MEASURETOOL_PARAM.NameChangeFlag)
			{
				ini.WriteString(text, "TrackerName", iNSP_MEASURETOOL_PARAM.TrackerName);
				iNSP_MEASURETOOL_PARAM.NameChangeFlag = false;
			}
			if (iNSP_MEASURETOOL_PARAM.ValueChangeFlag)
			{
				iNSP_MEASURETOOL_PARAM.ValueChangeFlag = false;
				ini.WriteBool(text, "TrackerON", iNSP_MEASURETOOL_PARAM.TrackerON);
				ini.WriteBool(text, "Locked", iNSP_MEASURETOOL_PARAM.Locked);
				ini.WriteString(text, "TrackerName", iNSP_MEASURETOOL_PARAM.TrackerName);
				ini.WriteInt(text, "SearchRect1T", iNSP_MEASURETOOL_PARAM.SearchRect1T);
				ini.WriteInt(text, "SearchRect1B", iNSP_MEASURETOOL_PARAM.SearchRect1B);
				ini.WriteInt(text, "SearchRect1L", iNSP_MEASURETOOL_PARAM.SearchRect1L);
				ini.WriteInt(text, "SearchRect1R", iNSP_MEASURETOOL_PARAM.SearchRect1R);
				ini.WriteInt(text, "SearchRect2T", iNSP_MEASURETOOL_PARAM.SearchRect2T);
				ini.WriteInt(text, "SearchRect2B", iNSP_MEASURETOOL_PARAM.SearchRect2B);
				ini.WriteInt(text, "SearchRect2L", iNSP_MEASURETOOL_PARAM.SearchRect2L);
				ini.WriteInt(text, "SearchRect2R", iNSP_MEASURETOOL_PARAM.SearchRect2R);
				ini.WriteInt(text, "THD1_ChannelIndex", (int)iNSP_MEASURETOOL_PARAM.THD1_ChannelIndex);
				ini.WriteInt(text, "THD2_ChannelIndex", (int)iNSP_MEASURETOOL_PARAM.THD2_ChannelIndex);
				ini.WriteInt(text, "THD1_Min", iNSP_MEASURETOOL_PARAM.THD1_Min);
				ini.WriteInt(text, "THD2_Min", iNSP_MEASURETOOL_PARAM.THD2_Min);
				ini.WriteInt(text, "THD1_Max", iNSP_MEASURETOOL_PARAM.THD1_Max);
				ini.WriteInt(text, "THD2_Max", iNSP_MEASURETOOL_PARAM.THD2_Max);
				ini.WriteBool(text, "EnableRGBThdA", iNSP_MEASURETOOL_PARAM.EnableRGBThdA);
				ini.WriteBool(text, "EnableRGBThdB", iNSP_MEASURETOOL_PARAM.EnableRGBThdB);
				ini.WriteInt(text, "THD_AR_Min", iNSP_MEASURETOOL_PARAM.THD_AR_Min);
				ini.WriteInt(text, "THD_AR_Max", iNSP_MEASURETOOL_PARAM.THD_AR_Max);
				ini.WriteInt(text, "THD_AG_Min", iNSP_MEASURETOOL_PARAM.THD_AG_Min);
				ini.WriteInt(text, "THD_AG_Max", iNSP_MEASURETOOL_PARAM.THD_AG_Max);
				ini.WriteInt(text, "THD_AB_Min", iNSP_MEASURETOOL_PARAM.THD_AB_Min);
				ini.WriteInt(text, "THD_AB_Max", iNSP_MEASURETOOL_PARAM.THD_AB_Max);
				ini.WriteInt(text, "THD_BR_Min", iNSP_MEASURETOOL_PARAM.THD_BR_Min);
				ini.WriteInt(text, "THD_BR_Max", iNSP_MEASURETOOL_PARAM.THD_BR_Max);
				ini.WriteInt(text, "THD_BG_Min", iNSP_MEASURETOOL_PARAM.THD_BG_Min);
				ini.WriteInt(text, "THD_BG_Max", iNSP_MEASURETOOL_PARAM.THD_BG_Max);
				ini.WriteInt(text, "THD_BB_Min", iNSP_MEASURETOOL_PARAM.THD_BB_Min);
				ini.WriteInt(text, "THD_BB_Max", iNSP_MEASURETOOL_PARAM.THD_BB_Max);
				ini.WriteInt(text, "DE_Start1_Count", iNSP_MEASURETOOL_PARAM.DE_Start1_Count);
				ini.WriteInt(text, "DE_Start2_Count", iNSP_MEASURETOOL_PARAM.DE_Start2_Count);
				ini.WriteInt(text, "DE_End1_Count", iNSP_MEASURETOOL_PARAM.DE_End1_Count);
				ini.WriteInt(text, "DE_End2_Count", iNSP_MEASURETOOL_PARAM.DE_End2_Count);
				ini.WriteInt(text, "DE_Start1_Index", (int)iNSP_MEASURETOOL_PARAM.DE_Start1_Index);
				ini.WriteInt(text, "DE_Start2_Index", (int)iNSP_MEASURETOOL_PARAM.DE_Start2_Index);
				ini.WriteInt(text, "DE_End1_Index", (int)iNSP_MEASURETOOL_PARAM.DE_End1_Index);
				ini.WriteInt(text, "DE_End2_Index", (int)iNSP_MEASURETOOL_PARAM.DE_End2_Index);
				ini.WriteInt(text, "Direction1", (int)iNSP_MEASURETOOL_PARAM.Direction1);
				ini.WriteInt(text, "Direction2", (int)iNSP_MEASURETOOL_PARAM.Direction2);
				ini.WriteInt(text, "Scan1Rule", (int)iNSP_MEASURETOOL_PARAM.Scan1Rule);
				ini.WriteInt(text, "Scan2Rule", (int)iNSP_MEASURETOOL_PARAM.Scan2Rule);
				ini.WriteDouble(text, "SinglePtLengthMin", iNSP_MEASURETOOL_PARAM.SinglePtLengthMin);
				ini.WriteDouble(text, "SinglePtLengthMax", iNSP_MEASURETOOL_PARAM.SinglePtLengthMax);
				ini.WriteDouble(text, "LengthMin", iNSP_MEASURETOOL_PARAM.LengthMin);
				ini.WriteDouble(text, "LengthMax", iNSP_MEASURETOOL_PARAM.LengthMax);
				ini.WriteDouble(text, "LengthDiffMax", iNSP_MEASURETOOL_PARAM.LengthDiffMax);
				ini.WriteInt(text, "ContinueError", iNSP_MEASURETOOL_PARAM.ContinueError);
				ini.WriteInt(text, "MaxError", iNSP_MEASURETOOL_PARAM.MaxError);
				ini.WriteBool(text, "UseSinglePt", iNSP_MEASURETOOL_PARAM.UseSinglePt);
				ini.WriteInt(text, "ParetoIndex", iNSP_MEASURETOOL_PARAM.ParetoIndex);
				ini.WriteBool(text, "EnableSelectFace", iNSP_MEASURETOOL_PARAM.EnableSelectFace);
				ini.WriteInt(text, "FaceIndex", iNSP_MEASURETOOL_PARAM.FaceIndex);
				ini.WriteBool(text, "CheckEveryone", iNSP_MEASURETOOL_PARAM.CheckEveryone);
				ini.WriteInt(text, "MaskSelectIndex", iNSP_MEASURETOOL_PARAM.MaskSelectIndex);
				ini.WriteInt(text, "MaskDE_Start1_Count", iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Count);
				ini.WriteInt(text, "MaskDE_Start1_Index", (int)iNSP_MEASURETOOL_PARAM.MaskDE_Start1_Index);
				ini.WriteInt(text, "ErrorBin", (int)iNSP_MEASURETOOL_PARAM.ErrorBin);
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadMaskParam(int index)
	{
		MaskParam[index].ValueChangeFlag = false;
		MaskParam[index].NameChangeFlag = false;
		MaskParam[index].Mask1 = new Mat();
		MaskParam[index].Mask2 = new Mat();
		string section = "MaskTools" + index;
		MaskParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		MaskParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		MaskParam[index].Locked = ini.GetBoolean(section, "Locked");
		MaskParam[index].SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		MaskParam[index].SearchRect2 = ini.ReadRect(section, "SearchRect2", 50);
		MaskParam[index].SearchRect2ON = ini.GetBoolean(section, "SearchRect2ON");
		MaskParam[index].SearchRect2AsMask = ini.GetBoolean(section, "SearchRect2AsMask");
		MaskParam[index].Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		MaskParam[index].Tracker2UseAutoSize = ini.GetBoolean(section, "Tracker2UseAutoSize", defaultValue: true);
		MaskParam[index].Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		MaskParam[index].Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		MaskParam[index].Tracker2AligmentXIndex = ini.GetInteger(section, "Tracker2AligmentXIndex");
		MaskParam[index].Tracker2AligmentYIndex = ini.GetInteger(section, "Tracker2AligmentYIndex");
		MaskParam[index].Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		MaskParam[index].Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		MaskParam[index].Tracker2AligmentX = ini.GetInteger(section, "Tracker2AligmentX", 10);
		MaskParam[index].Tracker2AligmentY = ini.GetInteger(section, "Tracker2AligmentY", 10);
		MaskParam[index].AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		MaskParam[index].AutoSize2_SelectIndex = ini.GetInteger(section, "AutoSize2_SelectIndex");
		MaskParam[index].Track1_T = ini.GetInteger(section, "Track1_T", 5);
		MaskParam[index].Track1_B = ini.GetInteger(section, "Track1_B", 5);
		MaskParam[index].Track1_L = ini.GetInteger(section, "Track1_L", 5);
		MaskParam[index].Track1_R = ini.GetInteger(section, "Track1_R", 5);
		MaskParam[index].Track2_T = ini.GetInteger(section, "Track2_T", 10);
		MaskParam[index].Track2_B = ini.GetInteger(section, "Track2_B", 10);
		MaskParam[index].Track2_L = ini.GetInteger(section, "Track2_L", 10);
		MaskParam[index].Track2_R = ini.GetInteger(section, "Track2_R", 10);
		MaskParam[index].THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		MaskParam[index].THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		MaskParam[index].THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		MaskParam[index].THD4_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD4_ChannelIndex", 3);
		MaskParam[index].THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
		MaskParam[index].THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON");
		MaskParam[index].THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON");
		MaskParam[index].THD4_ChannelON = ini.GetBoolean(section, "THD4_ChannelON");
		MaskParam[index].THD1_Min = ini.GetInteger(section, "THD1_Min");
		MaskParam[index].THD2_Min = ini.GetInteger(section, "THD2_Min");
		MaskParam[index].THD3_Min = ini.GetInteger(section, "THD3_Min");
		MaskParam[index].THD4_Min = ini.GetInteger(section, "THD4_Min");
		MaskParam[index].THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		MaskParam[index].THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		MaskParam[index].THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		MaskParam[index].THD4_Max = ini.GetInteger(section, "THD4_Max", 255);
		MaskParam[index].DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
		MaskParam[index].DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
		MaskParam[index].DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
		MaskParam[index].DE_Start4_Count = ini.GetInteger(section, "DE_Start4_Count");
		MaskParam[index].DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
		MaskParam[index].DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
		MaskParam[index].DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
		MaskParam[index].DE_End4_Count = ini.GetInteger(section, "DE_End4_Count");
		MaskParam[index].DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
		MaskParam[index].DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
		MaskParam[index].DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
		MaskParam[index].DE_Start4_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start4_Index");
		MaskParam[index].DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
		MaskParam[index].DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
		MaskParam[index].DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
		MaskParam[index].DE_End4_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End4_Index");
		MaskParam[index].InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		MaskParam[index].InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		MaskParam[index].InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		MaskParam[index].InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		MaskParam[index].DE_FinalStart_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_FinalStart_Index");
		MaskParam[index].DE_FinalEnd_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_FinalEnd_Index");
		MaskParam[index].DE_FinalStart_Count = ini.GetInteger(section, "DE_FinalStart_Count");
		MaskParam[index].DE_FinalEnd_Count = ini.GetInteger(section, "DE_FinalEnd_Count");
		MaskParam[index].T1FillletCount0 = ini.GetInteger(section, "T1FillletCount0");
		MaskParam[index].T1FillletCount1 = ini.GetInteger(section, "T1FillletCount1");
		MaskParam[index].T1FillletCount2 = ini.GetInteger(section, "T1FillletCount2");
		MaskParam[index].T1FillletCount3 = ini.GetInteger(section, "T1FillletCount3");
		MaskParam[index].T2FillletCount0 = ini.GetInteger(section, "T2FillletCount0");
		MaskParam[index].T2FillletCount1 = ini.GetInteger(section, "T2FillletCount1");
		MaskParam[index].T2FillletCount2 = ini.GetInteger(section, "T2FillletCount2");
		MaskParam[index].T2FillletCount3 = ini.GetInteger(section, "T2FillletCount3");
		MaskParam[index].FillHole = ini.GetBoolean(section, "FillHole");
		MaskParam[index].doConvexHull = ini.GetBoolean(section, "doConvexHull");
		MaskParam[index].MaxBlob = ini.GetInteger(section, "MaxBlob");
		MaskParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		MaskParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		MaskParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		MaskParam[index].EnableAdvLocateMask = ini.GetBoolean(section, "EnableAdvLocateMask");
		MaskParam[index].AdvLocateStartIndex = ini.GetInteger(section, "AdvLocateStartIndex");
		MaskParam[index].AdvLocateEndIndex = ini.GetInteger(section, "AdvLocateEndIndex");
		MaskParam[index].AdvLocateBorder1 = ini.GetInteger(section, "AdvLocateBorder1");
		MaskParam[index].AdvLocateBorder2 = ini.GetInteger(section, "AdvLocateBorder2");
		MaskParam[index].AdvLocateExpand1 = ini.GetInteger(section, "AdvLocateExpand1");
		MaskParam[index].AdvLocateExpand2 = ini.GetInteger(section, "AdvLocateExpand2");
		MaskParam[index].AdvLocateExpandCount1 = ini.GetInteger(section, "AdvLocateExpandCount1");
		MaskParam[index].AdvLocateExpandCount2 = ini.GetInteger(section, "AdvLocateExpandCount2");
		MaskParam[index].AdvLocateAsInsp = ini.GetBoolean(section, "AdvLocateAsInsp");
		MaskParam[index].AdvLocateBorderFilllet = ini.GetInteger(section, "AdvLocateBorderFilllet");
		MaskParam[index].Combine1Select = ini.GetInteger(section, "Combine1Select");
		MaskParam[index].Combine2Select = ini.GetInteger(section, "Combine2Select");
		MaskParam[index].Combine3Select = ini.GetInteger(section, "Combine3Select");
		MaskParam[index].Combine4Select = ini.GetInteger(section, "Combine4Select");
		MaskParam[index].Combine5Select = ini.GetInteger(section, "Combine5Select");
		MaskParam[index].Combine6Select = ini.GetInteger(section, "Combine6Select");
		MaskParam[index].Combine7Select = ini.GetInteger(section, "Combine7Select");
		MaskParam[index].Combine8Select = ini.GetInteger(section, "Combine8Select");
		MaskParam[index].Combine1Rule = ini.GetInteger(section, "Combine1Rule");
		MaskParam[index].Combine2Rule = ini.GetInteger(section, "Combine2Rule");
		MaskParam[index].Combine3Rule = ini.GetInteger(section, "Combine3Rule");
		MaskParam[index].Combine4Rule = ini.GetInteger(section, "Combine4Rule");
		MaskParam[index].Combine5Rule = ini.GetInteger(section, "Combine5Rule");
		MaskParam[index].Combine6Rule = ini.GetInteger(section, "Combine6Rule");
		MaskParam[index].Combine7Rule = ini.GetInteger(section, "Combine7Rule");
		MaskParam[index].Combine8Rule = ini.GetInteger(section, "Combine8Rule");
		MaskParam[index].Combine1DE_Start_Index = ini.GetInteger(section, "Combine1DE_Start_Index");
		MaskParam[index].Combine2DE_Start_Index = ini.GetInteger(section, "Combine2DE_Start_Index");
		MaskParam[index].Combine3DE_Start_Index = ini.GetInteger(section, "Combine3DE_Start_Index");
		MaskParam[index].Combine4DE_Start_Index = ini.GetInteger(section, "Combine4DE_Start_Index");
		MaskParam[index].Combine5DE_Start_Index = ini.GetInteger(section, "Combine5DE_Start_Index");
		MaskParam[index].Combine6DE_Start_Index = ini.GetInteger(section, "Combine6DE_Start_Index");
		MaskParam[index].Combine7DE_Start_Index = ini.GetInteger(section, "Combine7DE_Start_Index");
		MaskParam[index].Combine8DE_Start_Index = ini.GetInteger(section, "Combine8DE_Start_Index");
		MaskParam[index].Combine1DE_Start_Count = ini.GetInteger(section, "Combine1DE_Start_Count");
		MaskParam[index].Combine2DE_Start_Count = ini.GetInteger(section, "Combine2DE_Start_Count");
		MaskParam[index].Combine3DE_Start_Count = ini.GetInteger(section, "Combine3DE_Start_Count");
		MaskParam[index].Combine4DE_Start_Count = ini.GetInteger(section, "Combine4DE_Start_Count");
		MaskParam[index].Combine5DE_Start_Count = ini.GetInteger(section, "Combine5DE_Start_Count");
		MaskParam[index].Combine6DE_Start_Count = ini.GetInteger(section, "Combine6DE_Start_Count");
		MaskParam[index].Combine7DE_Start_Count = ini.GetInteger(section, "Combine7DE_Start_Count");
		MaskParam[index].Combine8DE_Start_Count = ini.GetInteger(section, "Combine8DE_Start_Count");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteMaskParam(int index)
	{
		string section = "MaskTools" + index;
		if (MaskParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", MaskParam[index].TrackerName);
			MaskParam[index].NameChangeFlag = false;
		}
		if (!MaskParam[index].ValueChangeFlag)
		{
			return true;
		}
		MaskParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", MaskParam[index].TrackerON);
		ini.WriteString(section, "TrackerName", MaskParam[index].TrackerName);
		ini.WriteBool(section, "Locked", MaskParam[index].Locked);
		ini.WriteBool(section, "SearchRect2ON", MaskParam[index].SearchRect2ON);
		ini.WriteBool(section, "SearchRect2AsMask", MaskParam[index].SearchRect2AsMask);
		ini.WriteBool(section, "Tracker1UseAutoSize", MaskParam[index].Tracker1UseAutoSize);
		ini.WriteBool(section, "Tracker2UseAutoSize", MaskParam[index].Tracker2UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", MaskParam[index].Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker2AligmentXIndex", MaskParam[index].Tracker2AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", MaskParam[index].Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker2AligmentYIndex", MaskParam[index].Tracker2AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", MaskParam[index].Tracker1AligmentX);
		ini.WriteInt(section, "Tracker2AligmentX", MaskParam[index].Tracker2AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", MaskParam[index].Tracker1AligmentY);
		ini.WriteInt(section, "Tracker2AligmentY", MaskParam[index].Tracker2AligmentY);
		ini.WriteInt(section, "AutoSize1_SelectIndex", MaskParam[index].AutoSize1_SelectIndex);
		ini.WriteInt(section, "AutoSize2_SelectIndex", MaskParam[index].AutoSize2_SelectIndex);
		ini.WriteInt(section, "Track1_T", MaskParam[index].Track1_T);
		ini.WriteInt(section, "Track1_B", MaskParam[index].Track1_B);
		ini.WriteInt(section, "Track1_L", MaskParam[index].Track1_L);
		ini.WriteInt(section, "Track1_R", MaskParam[index].Track1_R);
		ini.WriteInt(section, "Track2_T", MaskParam[index].Track2_T);
		ini.WriteInt(section, "Track2_B", MaskParam[index].Track2_B);
		ini.WriteInt(section, "Track2_L", MaskParam[index].Track2_L);
		ini.WriteInt(section, "Track2_R", MaskParam[index].Track2_R);
		ini.WriteInt(section, "THD1_ChannelIndex", (int)MaskParam[index].THD1_ChannelIndex);
		ini.WriteInt(section, "THD2_ChannelIndex", (int)MaskParam[index].THD2_ChannelIndex);
		ini.WriteInt(section, "THD3_ChannelIndex", (int)MaskParam[index].THD3_ChannelIndex);
		ini.WriteInt(section, "THD4_ChannelIndex", (int)MaskParam[index].THD4_ChannelIndex);
		ini.WriteBool(section, "THD1_ChannelON", MaskParam[index].THD1_ChannelON);
		ini.WriteBool(section, "THD2_ChannelON", MaskParam[index].THD2_ChannelON);
		ini.WriteBool(section, "THD3_ChannelON", MaskParam[index].THD3_ChannelON);
		ini.WriteBool(section, "THD4_ChannelON", MaskParam[index].THD4_ChannelON);
		ini.WriteInt(section, "THD1_Min", MaskParam[index].THD1_Min);
		ini.WriteInt(section, "THD2_Min", MaskParam[index].THD2_Min);
		ini.WriteInt(section, "THD3_Min", MaskParam[index].THD3_Min);
		ini.WriteInt(section, "THD4_Min", MaskParam[index].THD4_Min);
		ini.WriteInt(section, "THD1_Max", MaskParam[index].THD1_Max);
		ini.WriteInt(section, "THD2_Max", MaskParam[index].THD2_Max);
		ini.WriteInt(section, "THD3_Max", MaskParam[index].THD3_Max);
		ini.WriteInt(section, "THD4_Max", MaskParam[index].THD4_Max);
		ini.WriteInt(section, "DE_Start1_Count", MaskParam[index].DE_Start1_Count);
		ini.WriteInt(section, "DE_Start2_Count", MaskParam[index].DE_Start2_Count);
		ini.WriteInt(section, "DE_Start3_Count", MaskParam[index].DE_Start3_Count);
		ini.WriteInt(section, "DE_Start4_Count", MaskParam[index].DE_Start4_Count);
		ini.WriteInt(section, "DE_End1_Count", MaskParam[index].DE_End1_Count);
		ini.WriteInt(section, "DE_End2_Count", MaskParam[index].DE_End2_Count);
		ini.WriteInt(section, "DE_End3_Count", MaskParam[index].DE_End3_Count);
		ini.WriteInt(section, "DE_End4_Count", MaskParam[index].DE_End4_Count);
		ini.WriteInt(section, "DE_Start1_Index", (int)MaskParam[index].DE_Start1_Index);
		ini.WriteInt(section, "DE_Start2_Index", (int)MaskParam[index].DE_Start2_Index);
		ini.WriteInt(section, "DE_Start3_Index", (int)MaskParam[index].DE_Start3_Index);
		ini.WriteInt(section, "DE_Start4_Index", (int)MaskParam[index].DE_Start4_Index);
		ini.WriteInt(section, "DE_End1_Index", (int)MaskParam[index].DE_End1_Index);
		ini.WriteInt(section, "DE_End2_Index", (int)MaskParam[index].DE_End2_Index);
		ini.WriteInt(section, "DE_End3_Index", (int)MaskParam[index].DE_End3_Index);
		ini.WriteInt(section, "DE_End4_Index", (int)MaskParam[index].DE_End4_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)MaskParam[index].InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)MaskParam[index].InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)MaskParam[index].InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)MaskParam[index].InspRule4_Index);
		ini.WriteInt(section, "DE_FinalStart_Index", (int)MaskParam[index].DE_FinalStart_Index);
		ini.WriteInt(section, "DE_FinalEnd_Index", (int)MaskParam[index].DE_FinalEnd_Index);
		ini.WriteInt(section, "DE_FinalStart_Count", MaskParam[index].DE_FinalStart_Count);
		ini.WriteInt(section, "DE_FinalEnd_Count", MaskParam[index].DE_FinalEnd_Count);
		ini.WriteInt(section, "T1FillletCount0", MaskParam[index].T1FillletCount0);
		ini.WriteInt(section, "T1FillletCount1", MaskParam[index].T1FillletCount1);
		ini.WriteInt(section, "T1FillletCount2", MaskParam[index].T1FillletCount2);
		ini.WriteInt(section, "T1FillletCount3", MaskParam[index].T1FillletCount3);
		ini.WriteInt(section, "T2FillletCount0", MaskParam[index].T2FillletCount0);
		ini.WriteInt(section, "T2FillletCount1", MaskParam[index].T2FillletCount1);
		ini.WriteInt(section, "T2FillletCount2", MaskParam[index].T2FillletCount2);
		ini.WriteInt(section, "T2FillletCount3", MaskParam[index].T2FillletCount3);
		ini.WriteBool(section, "FillHole", MaskParam[index].FillHole);
		ini.WriteBool(section, "doConvexHull", MaskParam[index].doConvexHull);
		ini.WriteInt(section, "MaxBlob", MaskParam[index].MaxBlob);
		ini.WriteInt(section, "ParetoIndex", MaskParam[index].ParetoIndex);
		ini.WriteBool(section, "EnableSelectFace", MaskParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", MaskParam[index].FaceIndex);
		ini.WriteBool(section, "EnableAdvLocateMask", MaskParam[index].EnableAdvLocateMask);
		ini.WriteInt(section, "AdvLocateStartIndex", MaskParam[index].AdvLocateStartIndex);
		ini.WriteInt(section, "AdvLocateEndIndex", MaskParam[index].AdvLocateEndIndex);
		ini.WriteInt(section, "AdvLocateBorder1", MaskParam[index].AdvLocateBorder1);
		ini.WriteInt(section, "AdvLocateBorder2", MaskParam[index].AdvLocateBorder2);
		ini.WriteInt(section, "AdvLocateExpand1", MaskParam[index].AdvLocateExpand1);
		ini.WriteInt(section, "AdvLocateExpand2", MaskParam[index].AdvLocateExpand2);
		ini.WriteInt(section, "AdvLocateExpandCount1", MaskParam[index].AdvLocateExpandCount1);
		ini.WriteInt(section, "AdvLocateExpandCount2", MaskParam[index].AdvLocateExpandCount2);
		ini.WriteBool(section, "AdvLocateAsInsp", MaskParam[index].AdvLocateAsInsp);
		ini.WriteInt(section, "AdvLocateBorderFilllet", MaskParam[index].AdvLocateBorderFilllet);
		ini.WriteInt(section, "Combine1Select", MaskParam[index].Combine1Select);
		ini.WriteInt(section, "Combine2Select", MaskParam[index].Combine2Select);
		ini.WriteInt(section, "Combine3Select", MaskParam[index].Combine3Select);
		ini.WriteInt(section, "Combine4Select", MaskParam[index].Combine4Select);
		ini.WriteInt(section, "Combine5Select", MaskParam[index].Combine5Select);
		ini.WriteInt(section, "Combine6Select", MaskParam[index].Combine6Select);
		ini.WriteInt(section, "Combine7Select", MaskParam[index].Combine7Select);
		ini.WriteInt(section, "Combine8Select", MaskParam[index].Combine8Select);
		ini.WriteInt(section, "Combine1Rule", MaskParam[index].Combine1Rule);
		ini.WriteInt(section, "Combine2Rule", MaskParam[index].Combine2Rule);
		ini.WriteInt(section, "Combine3Rule", MaskParam[index].Combine3Rule);
		ini.WriteInt(section, "Combine4Rule", MaskParam[index].Combine4Rule);
		ini.WriteInt(section, "Combine5Rule", MaskParam[index].Combine5Rule);
		ini.WriteInt(section, "Combine6Rule", MaskParam[index].Combine6Rule);
		ini.WriteInt(section, "Combine7Rule", MaskParam[index].Combine7Rule);
		ini.WriteInt(section, "Combine8Rule", MaskParam[index].Combine8Rule);
		ini.WriteInt(section, "Combine1DE_Start_Index", MaskParam[index].Combine1DE_Start_Index);
		ini.WriteInt(section, "Combine2DE_Start_Index", MaskParam[index].Combine2DE_Start_Index);
		ini.WriteInt(section, "Combine3DE_Start_Index", MaskParam[index].Combine3DE_Start_Index);
		ini.WriteInt(section, "Combine4DE_Start_Index", MaskParam[index].Combine4DE_Start_Index);
		ini.WriteInt(section, "Combine5DE_Start_Index", MaskParam[index].Combine5DE_Start_Index);
		ini.WriteInt(section, "Combine6DE_Start_Index", MaskParam[index].Combine6DE_Start_Index);
		ini.WriteInt(section, "Combine7DE_Start_Index", MaskParam[index].Combine7DE_Start_Index);
		ini.WriteInt(section, "Combine8DE_Start_Index", MaskParam[index].Combine8DE_Start_Index);
		ini.WriteInt(section, "Combine1DE_Start_Count", MaskParam[index].Combine1DE_Start_Count);
		ini.WriteInt(section, "Combine2DE_Start_Count", MaskParam[index].Combine2DE_Start_Count);
		ini.WriteInt(section, "Combine3DE_Start_Count", MaskParam[index].Combine3DE_Start_Count);
		ini.WriteInt(section, "Combine4DE_Start_Count", MaskParam[index].Combine4DE_Start_Count);
		ini.WriteInt(section, "Combine5DE_Start_Count", MaskParam[index].Combine5DE_Start_Count);
		ini.WriteInt(section, "Combine6DE_Start_Count", MaskParam[index].Combine6DE_Start_Count);
		ini.WriteInt(section, "Combine7DE_Start_Count", MaskParam[index].Combine7DE_Start_Count);
		ini.WriteInt(section, "Combine8DE_Start_Count", MaskParam[index].Combine8DE_Start_Count);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadBinaryParam(ref INSP_BINARYTOOL_PARAM bParam, int index, string sectionbase = "BinaryTools")
	{
		bParam.ValueChangeFlag = false;
		bParam.NameChangeFlag = false;
		string section = sectionbase + index;
		bParam.TrackerON = ini.GetBoolean(section, "TrackerON");
		bParam.TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		bParam.Locked = ini.GetBoolean(section, "Locked");
		bParam.SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		bParam.SearchRect2 = ini.ReadRect(section, "SearchRect2", 50);
		bParam.SearchRect2ON = ini.GetBoolean(section, "SearchRect2ON");
		bParam.Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		bParam.Tracker2UseAutoSize = ini.GetBoolean(section, "Tracker2UseAutoSize", defaultValue: true);
		bParam.Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		bParam.Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		bParam.Tracker2AligmentXIndex = ini.GetInteger(section, "Tracker2AligmentXIndex");
		bParam.Tracker2AligmentYIndex = ini.GetInteger(section, "Tracker2AligmentYIndex");
		bParam.Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		bParam.Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		bParam.Tracker2AligmentX = ini.GetInteger(section, "Tracker2AligmentX", 10);
		bParam.Tracker2AligmentY = ini.GetInteger(section, "Tracker2AligmentY", 10);
		bParam.MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
		bParam.MaskDE_Start1_Count = ini.GetInteger(section, "MaskDE_Start1_Count");
		bParam.MaskDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MaskDE_Start1_Index");
		bParam.UseMaskAsInspArea = ini.GetBoolean(section, "UseMaskAsInspArea");
		bParam.AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		bParam.AutoSize2_SelectIndex = ini.GetInteger(section, "AutoSize2_SelectIndex");
		bParam.Track1_T = ini.GetInteger(section, "Track1_T", 5);
		bParam.Track1_B = ini.GetInteger(section, "Track1_B", 5);
		bParam.Track1_L = ini.GetInteger(section, "Track1_L", 5);
		bParam.Track1_R = ini.GetInteger(section, "Track1_R", 5);
		bParam.Track2_T = ini.GetInteger(section, "Track2_T", 10);
		bParam.Track2_B = ini.GetInteger(section, "Track2_B", 10);
		bParam.Track2_L = ini.GetInteger(section, "Track2_L", 10);
		bParam.Track2_R = ini.GetInteger(section, "Track2_R", 10);
		bParam.ColorReferenceIndex = ini.GetInteger(section, "ColorReferenceIndex");
		bParam.THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		bParam.THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		bParam.THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		bParam.THD4_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD4_ChannelIndex", 3);
		bParam.THD1_ChannelON = ini.GetBoolean(section, "THD1_ChannelON", defaultValue: true);
		bParam.THD2_ChannelON = ini.GetBoolean(section, "THD2_ChannelON");
		bParam.THD3_ChannelON = ini.GetBoolean(section, "THD3_ChannelON");
		bParam.THD4_ChannelON = ini.GetBoolean(section, "THD4_ChannelON");
		bParam.THD_ORMode = ini.GetBoolean(section, "THD_ORMode");
		bParam.THD1_Min = ini.GetInteger(section, "THD1_Min");
		bParam.THD2_Min = ini.GetInteger(section, "THD2_Min");
		bParam.THD3_Min = ini.GetInteger(section, "THD3_Min");
		bParam.THD4_Min = ini.GetInteger(section, "THD4_Min");
		bParam.THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		bParam.THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		bParam.THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		bParam.THD4_Max = ini.GetInteger(section, "THD4_Max", 255);
		bParam.DE_Start1_Count = ini.GetInteger(section, "DE_Start1_Count");
		bParam.DE_Start2_Count = ini.GetInteger(section, "DE_Start2_Count");
		bParam.DE_Start3_Count = ini.GetInteger(section, "DE_Start3_Count");
		bParam.DE_Start4_Count = ini.GetInteger(section, "DE_Start4_Count");
		bParam.DE_End1_Count = ini.GetInteger(section, "DE_End1_Count");
		bParam.DE_End2_Count = ini.GetInteger(section, "DE_End2_Count");
		bParam.DE_End3_Count = ini.GetInteger(section, "DE_End3_Count");
		bParam.DE_End4_Count = ini.GetInteger(section, "DE_End4_Count");
		bParam.DE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start1_Index");
		bParam.DE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start2_Index");
		bParam.DE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start3_Index");
		bParam.DE_Start4_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start4_Index");
		bParam.DE_End1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End1_Index");
		bParam.DE_End2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End2_Index");
		bParam.DE_End3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End3_Index");
		bParam.DE_End4_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End4_Index");
		bParam.InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		bParam.InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		bParam.InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		bParam.InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		bParam.InspRule1_Min = ini.GetDouble(section, "InspRule1_Min");
		bParam.InspRule2_Min = ini.GetDouble(section, "InspRule2_Min");
		bParam.InspRule3_Min = ini.GetDouble(section, "InspRule3_Min");
		bParam.InspRule4_Min = ini.GetDouble(section, "InspRule4_Min");
		bParam.InspRule1_Max = ini.GetDouble(section, "InspRule1_Max", 10.0);
		bParam.InspRule2_Max = ini.GetDouble(section, "InspRule2_Max", 10.0);
		bParam.InspRule3_Max = ini.GetDouble(section, "InspRule3_Max", 50.0);
		bParam.InspRule4_Max = ini.GetDouble(section, "InspRule4_Max", 50.0);
		bParam.Use_THD_ReferenceIndex = ini.GetInteger(section, "Use_THD_ReferenceIndex");
		bParam.Use_Rule_ReferenceIndex = ini.GetInteger(section, "Use_Rule_ReferenceIndex");
		bParam.InspRuleAND = ini.GetBoolean(section, "InspRuleAND", defaultValue: true);
		bParam.BlobAnalysisSelect = ini.GetInteger(section, "BlobAnalysisSelect");
		bParam.BlobLineSort = ini.GetInteger(section, "BlobLineSort");
		bParam.BlobLineNum = ini.GetInteger(section, "BlobLineNum", 3);
		bParam.BlobLineMin = ini.GetInteger(section, "BlobLineMin", 1);
		bParam.BlobLineMax = ini.GetInteger(section, "BlobLineMax", 2);
		bParam.BlobLineWidthMin = ini.GetDouble(section, "BlobLineWidthMin");
		bParam.BlobLineWidthMax = ini.GetDouble(section, "BlobLineWidthMax", 5.0);
		bParam.BlobLineHeightMin = ini.GetDouble(section, "BlobLineHeightMin", 50.0);
		bParam.BlobLineHeightMax = ini.GetDouble(section, "BlobLineHeightMax", 100.0);
		bParam.BlobLineAreaMin = ini.GetDouble(section, "BlobLineAreaMin", 100.0);
		bParam.BlobLineAreaMax = ini.GetDouble(section, "BlobLineAreaMax", 500.0);
		bParam.MB_InspRule1_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule1_Index", 1);
		bParam.MB_InspRule2_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule2_Index", 2);
		bParam.MB_InspRule3_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule3_Index", 3);
		bParam.MB_InspRule4_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule4_Index");
		bParam.MB_InspRule1_Min = ini.GetDouble(section, "MB_InspRule1_Min");
		bParam.MB_InspRule2_Min = ini.GetDouble(section, "MB_InspRule2_Min");
		bParam.MB_InspRule3_Min = ini.GetDouble(section, "MB_InspRule3_Min");
		bParam.MB_InspRule4_Min = ini.GetDouble(section, "MB_InspRule4_Min");
		bParam.MB_InspRule1_Max = ini.GetDouble(section, "MB_InspRule1_Max", 10.0);
		bParam.MB_InspRule2_Max = ini.GetDouble(section, "MB_InspRule2_Max", 10.0);
		bParam.MB_InspRule3_Max = ini.GetDouble(section, "MB_InspRule3_Max", 50.0);
		bParam.MB_InspRule4_Max = ini.GetDouble(section, "MB_InspRule4_Max", 50.0);
		bParam.MB_InspRuleAND = ini.GetBoolean(section, "MB_InspRuleAND", defaultValue: true);
		bParam.MB_BlobSort = ini.GetInteger(section, "MB_BlobSort");
		bParam.MB_BlobNum = ini.GetInteger(section, "MB_BlobNum", 3);
		bParam.MB_AllowNGCount = ini.GetInteger(section, "MB_AllowNGCount", 1);
		bParam.LineWidthMinW = ini.GetInteger(section, "LineWidthMinW", 5);
		bParam.LineWidthMinH = ini.GetInteger(section, "LineWidthMinH", 5);
		bParam.LineWidthMinArea = ini.GetInteger(section, "LineWidthMinArea", 20);
		bParam.AllowLineWidth = ini.GetInteger(section, "AllowLineWidth", 25);
		bParam.AllowLineHeight = ini.GetInteger(section, "AllowLineHeight", 25);
		bParam.AllowLineError = ini.GetInteger(section, "AllowLineError", 5);
		bParam.AllowErrorW = ini.GetInteger(section, "AllowErrorW", 5);
		bParam.AllowErrorH = ini.GetInteger(section, "AllowErrorH", 5);
		bParam.EnableLineWidthA = ini.GetBoolean(section, "EnableLineWidthA", defaultValue: true);
		bParam.EnableLineWidthW = ini.GetBoolean(section, "EnableLineWidthW");
		bParam.EnableLineWidthH = ini.GetBoolean(section, "EnableLineWidthH");
		bParam.UsePixelValue = ini.GetBoolean(section, "UsePixelValue", defaultValue: true);
		bParam.UseFontMask = ini.GetBoolean(section, "UseFontMask");
		bParam.UseMatchTextMask1 = ini.GetInteger(section, "UseMatchTextMask1");
		bParam.UseMatchTextMask2 = ini.GetInteger(section, "UseMatchTextMask2");
		bParam.UseMatchTextMask3 = ini.GetInteger(section, "UseMatchTextMask3");
		bParam.MatchTextDE_Start1_Count = ini.GetInteger(section, "MatchTextDE_Start1_Count");
		bParam.MatchTextDE_Start2_Count = ini.GetInteger(section, "MatchTextDE_Start2_Count");
		bParam.MatchTextDE_Start3_Count = ini.GetInteger(section, "MatchTextDE_Start3_Count");
		bParam.MatchTextDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start1_Index");
		bParam.MatchTextDE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start2_Index");
		bParam.MatchTextDE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start3_Index");
		bParam.T1FillletCount0 = ini.GetInteger(section, "T1FillletCount0");
		bParam.T1FillletCount1 = ini.GetInteger(section, "T1FillletCount1");
		bParam.T1FillletCount2 = ini.GetInteger(section, "T1FillletCount2");
		bParam.T1FillletCount3 = ini.GetInteger(section, "T1FillletCount3");
		bParam.T2FillletCount0 = ini.GetInteger(section, "T2FillletCount0");
		bParam.T2FillletCount1 = ini.GetInteger(section, "T2FillletCount1");
		bParam.T2FillletCount2 = ini.GetInteger(section, "T2FillletCount2");
		bParam.T2FillletCount3 = ini.GetInteger(section, "T2FillletCount3");
		bParam.ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		bParam.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		bParam.FaceIndex = ini.GetInteger(section, "FaceIndex");
		bParam.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		bParam.EnableSoftCorrection = ini.GetBoolean(section, "EnableSoftCorrection");
		bParam.TrackErrorSetNG = ini.GetBoolean(section, "TrackErrorSetNG", defaultValue: true);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteBinaryParam(INSP_BINARYTOOL_PARAM bParam, int index, string sectionbase = "BinaryTools")
	{
		string section = sectionbase + index;
		if (bParam.NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", bParam.TrackerName);
			bParam.NameChangeFlag = false;
		}
		if (!bParam.ValueChangeFlag)
		{
			return true;
		}
		bParam.ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", bParam.TrackerON);
		ini.WriteString(section, "TrackerName", bParam.TrackerName);
		ini.WriteBool(section, "Locked", bParam.Locked);
		ini.WriteBool(section, "SearchRect2ON", bParam.SearchRect2ON);
		ini.WriteBool(section, "Tracker1UseAutoSize", bParam.Tracker1UseAutoSize);
		ini.WriteBool(section, "Tracker2UseAutoSize", bParam.Tracker2UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", bParam.Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker2AligmentXIndex", bParam.Tracker2AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", bParam.Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker2AligmentYIndex", bParam.Tracker2AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", bParam.Tracker1AligmentX);
		ini.WriteInt(section, "Tracker2AligmentX", bParam.Tracker2AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", bParam.Tracker1AligmentY);
		ini.WriteInt(section, "Tracker2AligmentY", bParam.Tracker2AligmentY);
		ini.WriteInt(section, "MaskSelectIndex", bParam.MaskSelectIndex);
		ini.WriteInt(section, "MaskDE_Start1_Count", bParam.MaskDE_Start1_Count);
		ini.WriteInt(section, "MaskDE_Start1_Index", (int)bParam.MaskDE_Start1_Index);
		ini.WriteBool(section, "UseMaskAsInspArea", bParam.UseMaskAsInspArea);
		ini.WriteInt(section, "AutoSize1_SelectIndex", bParam.AutoSize1_SelectIndex);
		ini.WriteInt(section, "AutoSize2_SelectIndex", bParam.AutoSize2_SelectIndex);
		ini.WriteInt(section, "Track1_T", bParam.Track1_T);
		ini.WriteInt(section, "Track1_B", bParam.Track1_B);
		ini.WriteInt(section, "Track1_L", bParam.Track1_L);
		ini.WriteInt(section, "Track1_R", bParam.Track1_R);
		ini.WriteInt(section, "Track2_T", bParam.Track2_T);
		ini.WriteInt(section, "Track2_B", bParam.Track2_B);
		ini.WriteInt(section, "Track2_L", bParam.Track2_L);
		ini.WriteInt(section, "Track2_R", bParam.Track2_R);
		ini.WriteInt(section, "ColorReferenceIndex", bParam.ColorReferenceIndex);
		ini.WriteInt(section, "THD1_ChannelIndex", (int)bParam.THD1_ChannelIndex);
		ini.WriteInt(section, "THD2_ChannelIndex", (int)bParam.THD2_ChannelIndex);
		ini.WriteInt(section, "THD3_ChannelIndex", (int)bParam.THD3_ChannelIndex);
		ini.WriteInt(section, "THD4_ChannelIndex", (int)bParam.THD4_ChannelIndex);
		ini.WriteBool(section, "THD1_ChannelON", bParam.THD1_ChannelON);
		ini.WriteBool(section, "THD2_ChannelON", bParam.THD2_ChannelON);
		ini.WriteBool(section, "THD3_ChannelON", bParam.THD3_ChannelON);
		ini.WriteBool(section, "THD4_ChannelON", bParam.THD4_ChannelON);
		ini.WriteBool(section, "THD_ORMode", bParam.THD_ORMode);
		ini.WriteInt(section, "THD1_Min", bParam.THD1_Min);
		ini.WriteInt(section, "THD2_Min", bParam.THD2_Min);
		ini.WriteInt(section, "THD3_Min", bParam.THD3_Min);
		ini.WriteInt(section, "THD4_Min", bParam.THD4_Min);
		ini.WriteInt(section, "THD1_Max", bParam.THD1_Max);
		ini.WriteInt(section, "THD2_Max", bParam.THD2_Max);
		ini.WriteInt(section, "THD3_Max", bParam.THD3_Max);
		ini.WriteInt(section, "THD4_Max", bParam.THD4_Max);
		ini.WriteInt(section, "DE_Start1_Count", bParam.DE_Start1_Count);
		ini.WriteInt(section, "DE_Start2_Count", bParam.DE_Start2_Count);
		ini.WriteInt(section, "DE_Start3_Count", bParam.DE_Start3_Count);
		ini.WriteInt(section, "DE_Start4_Count", bParam.DE_Start4_Count);
		ini.WriteInt(section, "DE_End1_Count", bParam.DE_End1_Count);
		ini.WriteInt(section, "DE_End2_Count", bParam.DE_End2_Count);
		ini.WriteInt(section, "DE_End3_Count", bParam.DE_End3_Count);
		ini.WriteInt(section, "DE_End4_Count", bParam.DE_End4_Count);
		ini.WriteInt(section, "DE_Start1_Index", (int)bParam.DE_Start1_Index);
		ini.WriteInt(section, "DE_Start2_Index", (int)bParam.DE_Start2_Index);
		ini.WriteInt(section, "DE_Start3_Index", (int)bParam.DE_Start3_Index);
		ini.WriteInt(section, "DE_Start4_Index", (int)bParam.DE_Start4_Index);
		ini.WriteInt(section, "DE_End1_Index", (int)bParam.DE_End1_Index);
		ini.WriteInt(section, "DE_End2_Index", (int)bParam.DE_End2_Index);
		ini.WriteInt(section, "DE_End3_Index", (int)bParam.DE_End3_Index);
		ini.WriteInt(section, "DE_End4_Index", (int)bParam.DE_End4_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)bParam.InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)bParam.InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)bParam.InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)bParam.InspRule4_Index);
		ini.WriteDouble(section, "InspRule1_Min", bParam.InspRule1_Min);
		ini.WriteDouble(section, "InspRule2_Min", bParam.InspRule2_Min);
		ini.WriteDouble(section, "InspRule3_Min", bParam.InspRule3_Min);
		ini.WriteDouble(section, "InspRule4_Min", bParam.InspRule4_Min);
		ini.WriteDouble(section, "InspRule1_Max", bParam.InspRule1_Max);
		ini.WriteDouble(section, "InspRule2_Max", bParam.InspRule2_Max);
		ini.WriteDouble(section, "InspRule3_Max", bParam.InspRule3_Max);
		ini.WriteDouble(section, "InspRule4_Max", bParam.InspRule4_Max);
		ini.WriteBool(section, "InspRuleAND", bParam.InspRuleAND);
		ini.WriteInt(section, "Use_THD_ReferenceIndex", bParam.Use_THD_ReferenceIndex);
		ini.WriteInt(section, "Use_Rule_ReferenceIndex", bParam.Use_Rule_ReferenceIndex);
		ini.WriteInt(section, "BlobAnalysisSelect", bParam.BlobAnalysisSelect);
		ini.WriteInt(section, "BlobLineSort", bParam.BlobLineSort);
		ini.WriteInt(section, "BlobLineNum", bParam.BlobLineNum);
		ini.WriteInt(section, "BlobLineMin", bParam.BlobLineMin);
		ini.WriteInt(section, "BlobLineMax", bParam.BlobLineMax);
		ini.WriteDouble(section, "BlobLineWidthMin", bParam.BlobLineWidthMin);
		ini.WriteDouble(section, "BlobLineWidthMax", bParam.BlobLineWidthMax);
		ini.WriteDouble(section, "BlobLineHeightMin", bParam.BlobLineHeightMin);
		ini.WriteDouble(section, "BlobLineHeightMax", bParam.BlobLineHeightMax);
		ini.WriteDouble(section, "BlobLineAreaMin", bParam.BlobLineAreaMin);
		ini.WriteDouble(section, "BlobLineAreaMax", bParam.BlobLineAreaMax);
		ini.WriteDouble(section, "AllowLineWidth", bParam.AllowLineWidth);
		ini.WriteDouble(section, "AllowLineHeight", bParam.AllowLineHeight);
		ini.WriteDouble(section, "AllowLineError", bParam.AllowLineError);
		ini.WriteDouble(section, "AllowErrorW", bParam.AllowErrorW);
		ini.WriteDouble(section, "AllowErrorH", bParam.AllowErrorH);
		ini.WriteBool(section, "EnableLineWidthA", bParam.EnableLineWidthA);
		ini.WriteBool(section, "EnableLineWidthW", bParam.EnableLineWidthW);
		ini.WriteBool(section, "EnableLineWidthH", bParam.EnableLineWidthH);
		ini.WriteBool(section, "UsePixelValue", bParam.UsePixelValue);
		ini.WriteInt(section, "MB_InspRule1_Index", (int)bParam.MB_InspRule1_Index);
		ini.WriteInt(section, "MB_InspRule2_Index", (int)bParam.MB_InspRule2_Index);
		ini.WriteInt(section, "MB_InspRule3_Index", (int)bParam.MB_InspRule3_Index);
		ini.WriteInt(section, "MB_InspRule4_Index", (int)bParam.MB_InspRule4_Index);
		ini.WriteDouble(section, "MB_InspRule1_Min", bParam.MB_InspRule1_Min);
		ini.WriteDouble(section, "MB_InspRule2_Min", bParam.MB_InspRule2_Min);
		ini.WriteDouble(section, "MB_InspRule3_Min", bParam.MB_InspRule3_Min);
		ini.WriteDouble(section, "MB_InspRule4_Min", bParam.MB_InspRule4_Min);
		ini.WriteDouble(section, "MB_InspRule1_Max", bParam.MB_InspRule1_Max);
		ini.WriteDouble(section, "MB_InspRule2_Max", bParam.MB_InspRule2_Max);
		ini.WriteDouble(section, "MB_InspRule3_Max", bParam.MB_InspRule3_Max);
		ini.WriteDouble(section, "MB_InspRule4_Max", bParam.MB_InspRule4_Max);
		ini.WriteBool(section, "MB_InspRuleAND", bParam.MB_InspRuleAND);
		ini.WriteInt(section, "MB_BlobSort", bParam.MB_BlobSort);
		ini.WriteInt(section, "MB_BlobNum", bParam.MB_BlobNum);
		ini.WriteInt(section, "MB_AllowNGCount", bParam.MB_AllowNGCount);
		ini.WriteInt(section, "LineWidthMinW", bParam.LineWidthMinW);
		ini.WriteInt(section, "LineWidthMinH", bParam.LineWidthMinH);
		ini.WriteInt(section, "LineWidthMinArea", bParam.LineWidthMinArea);
		ini.WriteInt(section, "ParetoIndex", bParam.ParetoIndex);
		ini.WriteBool(section, "EnableSelectFace", bParam.EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", bParam.FaceIndex);
		ini.WriteBool(section, "UseFontMask", bParam.UseFontMask);
		ini.WriteInt(section, "UseMatchTextMask1", bParam.UseMatchTextMask1);
		ini.WriteInt(section, "UseMatchTextMask2", bParam.UseMatchTextMask2);
		ini.WriteInt(section, "UseMatchTextMask3", bParam.UseMatchTextMask3);
		ini.WriteInt(section, "MatchTextDE_Start1_Count", bParam.MatchTextDE_Start1_Count);
		ini.WriteInt(section, "MatchTextDE_Start2_Count", bParam.MatchTextDE_Start2_Count);
		ini.WriteInt(section, "MatchTextDE_Start3_Count", bParam.MatchTextDE_Start3_Count);
		ini.WriteInt(section, "MatchTextDE_Start1_Index", (int)bParam.MatchTextDE_Start1_Index);
		ini.WriteInt(section, "MatchTextDE_Start2_Index", (int)bParam.MatchTextDE_Start2_Index);
		ini.WriteInt(section, "MatchTextDE_Start3_Index", (int)bParam.MatchTextDE_Start3_Index);
		ini.WriteInt(section, "T1FillletCount0", bParam.T1FillletCount0);
		ini.WriteInt(section, "T1FillletCount1", bParam.T1FillletCount1);
		ini.WriteInt(section, "T1FillletCount2", bParam.T1FillletCount2);
		ini.WriteInt(section, "T1FillletCount3", bParam.T1FillletCount3);
		ini.WriteInt(section, "T2FillletCount0", bParam.T2FillletCount0);
		ini.WriteInt(section, "T2FillletCount1", bParam.T2FillletCount1);
		ini.WriteInt(section, "T2FillletCount2", bParam.T2FillletCount2);
		ini.WriteInt(section, "T2FillletCount3", bParam.T2FillletCount3);
		ini.WriteBool(section, "EnableSoftCorrection", bParam.EnableSoftCorrection);
		ini.WriteInt(section, "ErrorBin", (int)bParam.ErrorBin);
		ini.WriteBool(section, "TrackErrorSetNG", bParam.TrackErrorSetNG);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadSmoothParam(int index)
	{
		SmoothParam[index].NameChangeFlag = false;
		SmoothParam[index].ValueChangeFlag = false;
		string section = "SmoothTools" + index;
		SmoothParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		SmoothParam[index].Locked = ini.GetBoolean(section, "Locked");
		SmoothParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		SmoothParam[index].SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		SmoothParam[index].SearchRect2 = ini.ReadRect(section, "SearchRect2", 50);
		SmoothParam[index].FilterNoise = ini.GetBoolean(section, "FilterNoise");
		SmoothParam[index].NoiseWidth = ini.GetInteger(section, "NoiseWidth", 1);
		SmoothParam[index].NoiseHeight = ini.GetInteger(section, "NoiseHeight", 1);
		SmoothParam[index].SearchRect2ON = ini.GetBoolean(section, "SearchRect2ON");
		SmoothParam[index].Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		SmoothParam[index].Tracker2UseAutoSize = ini.GetBoolean(section, "Tracker2UseAutoSize", defaultValue: true);
		SmoothParam[index].Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		SmoothParam[index].Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		SmoothParam[index].Tracker2AligmentXIndex = ini.GetInteger(section, "Tracker2AligmentXIndex");
		SmoothParam[index].Tracker2AligmentYIndex = ini.GetInteger(section, "Tracker2AligmentYIndex");
		SmoothParam[index].Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		SmoothParam[index].Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		SmoothParam[index].Tracker2AligmentX = ini.GetInteger(section, "Tracker2AligmentX", 10);
		SmoothParam[index].Tracker2AligmentY = ini.GetInteger(section, "Tracker2AligmentY", 10);
		SmoothParam[index].MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
		SmoothParam[index].UseMaskAsInspArea = ini.GetBoolean(section, "UseMaskAsInspArea");
		SmoothParam[index].AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		SmoothParam[index].AutoSize2_SelectIndex = ini.GetInteger(section, "AutoSize2_SelectIndex");
		SmoothParam[index].Track1_T = ini.GetInteger(section, "Track1_T", 5);
		SmoothParam[index].Track1_B = ini.GetInteger(section, "Track1_B", 5);
		SmoothParam[index].Track1_L = ini.GetInteger(section, "Track1_L", 5);
		SmoothParam[index].Track1_R = ini.GetInteger(section, "Track1_R", 5);
		SmoothParam[index].Track2_T = ini.GetInteger(section, "Track2_T", 10);
		SmoothParam[index].Track2_B = ini.GetInteger(section, "Track2_B", 10);
		SmoothParam[index].Track2_L = ini.GetInteger(section, "Track2_L", 10);
		SmoothParam[index].Track2_R = ini.GetInteger(section, "Track2_R", 10);
		SmoothParam[index].ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "ChannelIndex");
		SmoothParam[index].KernalCountH = ini.GetInteger(section, "KernalCountH", 5);
		SmoothParam[index].KernalCountV = ini.GetInteger(section, "KernalCountV", 5);
		SmoothParam[index].ThdUpON = ini.GetBoolean(section, "ThdUpON");
		SmoothParam[index].ThdUp = ini.GetInteger(section, "ThdUp", 10);
		SmoothParam[index].ThdDownON = ini.GetBoolean(section, "ThdDownON", defaultValue: true);
		SmoothParam[index].ThdDown = ini.GetInteger(section, "ThdDown", 10);
		SmoothParam[index].THDLimit_Min = ini.GetInteger(section, "THDLimit_Min");
		SmoothParam[index].THDLimit_Max = ini.GetInteger(section, "THDLimit_Max", 255);
		SmoothParam[index].DE_Start_Count = ini.GetInteger(section, "DE_Start_Count");
		SmoothParam[index].DE_End_Count = ini.GetInteger(section, "DE_End_Count");
		SmoothParam[index].DE_Start_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start_Index");
		SmoothParam[index].DE_End_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End_Index");
		SmoothParam[index].InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		SmoothParam[index].InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		SmoothParam[index].InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		SmoothParam[index].InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		SmoothParam[index].InspRule1_Min = ini.GetDouble(section, "InspRule1_Min");
		SmoothParam[index].InspRule2_Min = ini.GetDouble(section, "InspRule2_Min");
		SmoothParam[index].InspRule3_Min = ini.GetDouble(section, "InspRule3_Min");
		SmoothParam[index].InspRule4_Min = ini.GetDouble(section, "InspRule4_Min");
		SmoothParam[index].InspRule1_Max = ini.GetDouble(section, "InspRule1_Max", 10.0);
		SmoothParam[index].InspRule2_Max = ini.GetDouble(section, "InspRule2_Max", 10.0);
		SmoothParam[index].InspRule3_Max = ini.GetDouble(section, "InspRule3_Max", 50.0);
		SmoothParam[index].InspRule4_Max = ini.GetDouble(section, "InspRule4_Max", 50.0);
		SmoothParam[index].InspRuleAND = ini.GetBoolean(section, "InspRuleAND", defaultValue: true);
		SmoothParam[index].BlobAnalysisSelect = ini.GetInteger(section, "BlobAnalysisSelect");
		SmoothParam[index].BlobLineSort = ini.GetInteger(section, "BlobLineSort");
		SmoothParam[index].BlobLineNum = ini.GetInteger(section, "BlobLineNum", 3);
		SmoothParam[index].BlobLineMin = ini.GetInteger(section, "BlobLineMin", 1);
		SmoothParam[index].BlobLineMax = ini.GetInteger(section, "BlobLineMax", 2);
		SmoothParam[index].BlobLineWidthMin = ini.GetDouble(section, "BlobLineWidthMin");
		SmoothParam[index].BlobLineWidthMax = ini.GetDouble(section, "BlobLineWidthMax", 5.0);
		SmoothParam[index].BlobLineHeightMin = ini.GetDouble(section, "BlobLineHeightMin", 50.0);
		SmoothParam[index].BlobLineHeightMax = ini.GetDouble(section, "BlobLineHeightMax", 100.0);
		SmoothParam[index].BlobLineAreaMin = ini.GetDouble(section, "BlobLineAreaMin", 100.0);
		SmoothParam[index].BlobLineAreaMax = ini.GetDouble(section, "BlobLineAreaMax", 500.0);
		SmoothParam[index].MB_InspRule1_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule1_Index", 1);
		SmoothParam[index].MB_InspRule2_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule2_Index", 2);
		SmoothParam[index].MB_InspRule3_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule3_Index", 3);
		SmoothParam[index].MB_InspRule4_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule4_Index");
		SmoothParam[index].MB_InspRule1_Min = ini.GetDouble(section, "MB_InspRule1_Min");
		SmoothParam[index].MB_InspRule2_Min = ini.GetDouble(section, "MB_InspRule2_Min");
		SmoothParam[index].MB_InspRule3_Min = ini.GetDouble(section, "MB_InspRule3_Min");
		SmoothParam[index].MB_InspRule4_Min = ini.GetDouble(section, "MB_InspRule4_Min");
		SmoothParam[index].MB_InspRule1_Max = ini.GetDouble(section, "MB_InspRule1_Max", 10.0);
		SmoothParam[index].MB_InspRule2_Max = ini.GetDouble(section, "MB_InspRule2_Max", 10.0);
		SmoothParam[index].MB_InspRule3_Max = ini.GetDouble(section, "MB_InspRule3_Max", 50.0);
		SmoothParam[index].MB_InspRule4_Max = ini.GetDouble(section, "MB_InspRule4_Max", 50.0);
		SmoothParam[index].MB_InspRuleAND = ini.GetBoolean(section, "MB_InspRuleAND", defaultValue: true);
		SmoothParam[index].MB_BlobSort = ini.GetInteger(section, "MB_BlobSort");
		SmoothParam[index].MB_BlobNum = ini.GetInteger(section, "MB_BlobNum", 3);
		SmoothParam[index].MB_AllowNGCount = ini.GetInteger(section, "MB_AllowNGCount", 1);
		SmoothParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		SmoothParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		SmoothParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		SmoothParam[index].UseMatchTextMask1 = ini.GetInteger(section, "UseMatchTextMask1");
		SmoothParam[index].UseMatchTextMask2 = ini.GetInteger(section, "UseMatchTextMask2");
		SmoothParam[index].UseMatchTextMask3 = ini.GetInteger(section, "UseMatchTextMask3");
		SmoothParam[index].MatchTextDE_Start1_Count = ini.GetInteger(section, "MatchTextDE_Start1_Count");
		SmoothParam[index].MatchTextDE_Start2_Count = ini.GetInteger(section, "MatchTextDE_Start2_Count");
		SmoothParam[index].MatchTextDE_Start3_Count = ini.GetInteger(section, "MatchTextDE_Start3_Count");
		SmoothParam[index].MatchTextDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start1_Index");
		SmoothParam[index].MatchTextDE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start2_Index");
		SmoothParam[index].MatchTextDE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start3_Index");
		SmoothParam[index].MaskDE_Start1_Count = ini.GetInteger(section, "MaskDE_Start1_Count");
		SmoothParam[index].MaskDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MaskDE_Start1_Index");
		SmoothParam[index].T1FillletCount0 = ini.GetInteger(section, "T1FillletCount0");
		SmoothParam[index].T1FillletCount1 = ini.GetInteger(section, "T1FillletCount1");
		SmoothParam[index].T1FillletCount2 = ini.GetInteger(section, "T1FillletCount2");
		SmoothParam[index].T1FillletCount3 = ini.GetInteger(section, "T1FillletCount3");
		SmoothParam[index].T2FillletCount0 = ini.GetInteger(section, "T2FillletCount0");
		SmoothParam[index].T2FillletCount1 = ini.GetInteger(section, "T2FillletCount1");
		SmoothParam[index].T2FillletCount2 = ini.GetInteger(section, "T2FillletCount2");
		SmoothParam[index].T2FillletCount3 = ini.GetInteger(section, "T2FillletCount3");
		SmoothParam[index].TrackErrorSetNG = ini.GetBoolean(section, "TrackErrorSetNG", defaultValue: true);
		SmoothParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteSmoothParam(int index)
	{
		string section = "SmoothTools" + index;
		if (SmoothParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", SmoothParam[index].TrackerName);
			SmoothParam[index].NameChangeFlag = false;
		}
		if (!SmoothParam[index].ValueChangeFlag)
		{
			return true;
		}
		SmoothParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", SmoothParam[index].TrackerON);
		ini.WriteBool(section, "Locked", SmoothParam[index].Locked);
		ini.WriteString(section, "TrackerName", SmoothParam[index].TrackerName);
		ini.WriteBool(section, "FilterNoise", SmoothParam[index].FilterNoise);
		ini.WriteInt(section, "NoiseWidth", SmoothParam[index].NoiseWidth);
		ini.WriteInt(section, "NoiseHeight", SmoothParam[index].NoiseHeight);
		ini.WriteBool(section, "SearchRect2ON", SmoothParam[index].SearchRect2ON);
		ini.WriteBool(section, "Tracker1UseAutoSize", SmoothParam[index].Tracker1UseAutoSize);
		ini.WriteBool(section, "Tracker2UseAutoSize", SmoothParam[index].Tracker2UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", SmoothParam[index].Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker2AligmentXIndex", SmoothParam[index].Tracker2AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", SmoothParam[index].Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker2AligmentYIndex", SmoothParam[index].Tracker2AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", SmoothParam[index].Tracker1AligmentX);
		ini.WriteInt(section, "Tracker2AligmentX", SmoothParam[index].Tracker2AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", SmoothParam[index].Tracker1AligmentY);
		ini.WriteInt(section, "Tracker2AligmentY", SmoothParam[index].Tracker2AligmentY);
		ini.WriteInt(section, "MaskSelectIndex", SmoothParam[index].MaskSelectIndex);
		ini.WriteBool(section, "UseMaskAsInspArea", SmoothParam[index].UseMaskAsInspArea);
		ini.WriteInt(section, "AutoSize1_SelectIndex", SmoothParam[index].AutoSize1_SelectIndex);
		ini.WriteInt(section, "AutoSize2_SelectIndex", SmoothParam[index].AutoSize2_SelectIndex);
		ini.WriteInt(section, "Track1_T", SmoothParam[index].Track1_T);
		ini.WriteInt(section, "Track1_B", SmoothParam[index].Track1_B);
		ini.WriteInt(section, "Track1_L", SmoothParam[index].Track1_L);
		ini.WriteInt(section, "Track1_R", SmoothParam[index].Track1_R);
		ini.WriteInt(section, "Track2_T", SmoothParam[index].Track2_T);
		ini.WriteInt(section, "Track2_B", SmoothParam[index].Track2_B);
		ini.WriteInt(section, "Track2_L", SmoothParam[index].Track2_L);
		ini.WriteInt(section, "Track2_R", SmoothParam[index].Track2_R);
		ini.WriteInt(section, "ChannelIndex", (int)SmoothParam[index].ChannelIndex);
		ini.WriteBool(section, "ThdUpON", SmoothParam[index].ThdUpON);
		ini.WriteInt(section, "ThdUp", SmoothParam[index].ThdUp);
		ini.WriteBool(section, "ThdDownON", SmoothParam[index].ThdDownON);
		ini.WriteInt(section, "ThdDown", SmoothParam[index].ThdDown);
		ini.WriteInt(section, "KernalCountH", SmoothParam[index].KernalCountH);
		ini.WriteInt(section, "KernalCountV", SmoothParam[index].KernalCountV);
		ini.WriteInt(section, "THDLimit_Min", SmoothParam[index].THDLimit_Min);
		ini.WriteInt(section, "THDLimit_Max", SmoothParam[index].THDLimit_Max);
		ini.WriteInt(section, "DE_Start_Count", SmoothParam[index].DE_Start_Count);
		ini.WriteInt(section, "DE_End_Count", SmoothParam[index].DE_End_Count);
		ini.WriteInt(section, "DE_Start_Index", (int)SmoothParam[index].DE_Start_Index);
		ini.WriteInt(section, "DE_End_Index", (int)SmoothParam[index].DE_End_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)SmoothParam[index].InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)SmoothParam[index].InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)SmoothParam[index].InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)SmoothParam[index].InspRule4_Index);
		ini.WriteDouble(section, "InspRule1_Min", SmoothParam[index].InspRule1_Min);
		ini.WriteDouble(section, "InspRule2_Min", SmoothParam[index].InspRule2_Min);
		ini.WriteDouble(section, "InspRule3_Min", SmoothParam[index].InspRule3_Min);
		ini.WriteDouble(section, "InspRule4_Min", SmoothParam[index].InspRule4_Min);
		ini.WriteDouble(section, "InspRule1_Max", SmoothParam[index].InspRule1_Max);
		ini.WriteDouble(section, "InspRule2_Max", SmoothParam[index].InspRule2_Max);
		ini.WriteDouble(section, "InspRule3_Max", SmoothParam[index].InspRule3_Max);
		ini.WriteDouble(section, "InspRule4_Max", SmoothParam[index].InspRule4_Max);
		ini.WriteBool(section, "InspRuleAND", SmoothParam[index].InspRuleAND);
		ini.WriteInt(section, "BlobAnalysisSelect", SmoothParam[index].BlobAnalysisSelect);
		ini.WriteInt(section, "BlobLineSort", SmoothParam[index].BlobLineSort);
		ini.WriteInt(section, "BlobLineNum", SmoothParam[index].BlobLineNum);
		ini.WriteInt(section, "BlobLineMin", SmoothParam[index].BlobLineMin);
		ini.WriteInt(section, "BlobLineMax", SmoothParam[index].BlobLineMax);
		ini.WriteDouble(section, "BlobLineWidthMin", SmoothParam[index].BlobLineWidthMin);
		ini.WriteDouble(section, "BlobLineWidthMax", SmoothParam[index].BlobLineWidthMax);
		ini.WriteDouble(section, "BlobLineHeightMin", SmoothParam[index].BlobLineHeightMin);
		ini.WriteDouble(section, "BlobLineHeightMax", SmoothParam[index].BlobLineHeightMax);
		ini.WriteDouble(section, "BlobLineAreaMin", SmoothParam[index].BlobLineAreaMin);
		ini.WriteDouble(section, "BlobLineAreaMax", SmoothParam[index].BlobLineAreaMax);
		ini.WriteInt(section, "MB_InspRule1_Index", (int)SmoothParam[index].MB_InspRule1_Index);
		ini.WriteInt(section, "MB_InspRule2_Index", (int)SmoothParam[index].MB_InspRule2_Index);
		ini.WriteInt(section, "MB_InspRule3_Index", (int)SmoothParam[index].MB_InspRule3_Index);
		ini.WriteInt(section, "MB_InspRule4_Index", (int)SmoothParam[index].MB_InspRule4_Index);
		ini.WriteDouble(section, "MB_InspRule1_Min", SmoothParam[index].MB_InspRule1_Min);
		ini.WriteDouble(section, "MB_InspRule2_Min", SmoothParam[index].MB_InspRule2_Min);
		ini.WriteDouble(section, "MB_InspRule3_Min", SmoothParam[index].MB_InspRule3_Min);
		ini.WriteDouble(section, "MB_InspRule4_Min", SmoothParam[index].MB_InspRule4_Min);
		ini.WriteDouble(section, "MB_InspRule1_Max", SmoothParam[index].MB_InspRule1_Max);
		ini.WriteDouble(section, "MB_InspRule2_Max", SmoothParam[index].MB_InspRule2_Max);
		ini.WriteDouble(section, "MB_InspRule3_Max", SmoothParam[index].MB_InspRule3_Max);
		ini.WriteDouble(section, "MB_InspRule4_Max", SmoothParam[index].MB_InspRule4_Max);
		ini.WriteBool(section, "MB_InspRuleAND", SmoothParam[index].MB_InspRuleAND);
		ini.WriteInt(section, "MB_BlobSort", SmoothParam[index].MB_BlobSort);
		ini.WriteInt(section, "MB_BlobNum", SmoothParam[index].MB_BlobNum);
		ini.WriteInt(section, "MB_AllowNGCount", SmoothParam[index].MB_AllowNGCount);
		ini.WriteBool(section, "EnableSelectFace", SmoothParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", SmoothParam[index].FaceIndex);
		ini.WriteInt(section, "ParetoIndex", SmoothParam[index].ParetoIndex);
		ini.WriteInt(section, "UseMatchTextMask1", SmoothParam[index].UseMatchTextMask1);
		ini.WriteInt(section, "UseMatchTextMask2", SmoothParam[index].UseMatchTextMask2);
		ini.WriteInt(section, "UseMatchTextMask3", SmoothParam[index].UseMatchTextMask3);
		ini.WriteInt(section, "MatchTextDE_Start1_Count", SmoothParam[index].MatchTextDE_Start1_Count);
		ini.WriteInt(section, "MatchTextDE_Start2_Count", SmoothParam[index].MatchTextDE_Start2_Count);
		ini.WriteInt(section, "MatchTextDE_Start3_Count", SmoothParam[index].MatchTextDE_Start3_Count);
		ini.WriteInt(section, "MatchTextDE_Start1_Index", (int)SmoothParam[index].MatchTextDE_Start1_Index);
		ini.WriteInt(section, "MatchTextDE_Start2_Index", (int)SmoothParam[index].MatchTextDE_Start2_Index);
		ini.WriteInt(section, "MatchTextDE_Start3_Index", (int)SmoothParam[index].MatchTextDE_Start3_Index);
		ini.WriteInt(section, "MaskDE_Start1_Count", SmoothParam[index].MaskDE_Start1_Count);
		ini.WriteInt(section, "MaskDE_Start1_Index", (int)SmoothParam[index].MaskDE_Start1_Index);
		ini.WriteInt(section, "T1FillletCount0", SmoothParam[index].T1FillletCount0);
		ini.WriteInt(section, "T1FillletCount1", SmoothParam[index].T1FillletCount1);
		ini.WriteInt(section, "T1FillletCount2", SmoothParam[index].T1FillletCount2);
		ini.WriteInt(section, "T1FillletCount3", SmoothParam[index].T1FillletCount3);
		ini.WriteInt(section, "T2FillletCount0", SmoothParam[index].T2FillletCount0);
		ini.WriteInt(section, "T2FillletCount1", SmoothParam[index].T2FillletCount1);
		ini.WriteInt(section, "T2FillletCount2", SmoothParam[index].T2FillletCount2);
		ini.WriteInt(section, "T2FillletCount3", SmoothParam[index].T2FillletCount3);
		ini.WriteBool(section, "TrackErrorSetNG", SmoothParam[index].TrackErrorSetNG);
		ini.WriteInt(section, "ErrorBin", (int)SmoothParam[index].ErrorBin);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadDifferenceParam(int index)
	{
		DifferenceParam[index].NameChangeFlag = false;
		DifferenceParam[index].ValueChangeFlag = false;
		string section = "DifferenceTools" + index;
		DifferenceParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		DifferenceParam[index].Locked = ini.GetBoolean(section, "Locked");
		DifferenceParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		DifferenceParam[index].SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		DifferenceParam[index].SearchRect2 = ini.ReadRect(section, "SearchRect2", 50);
		DifferenceParam[index].FilterNoise = ini.GetBoolean(section, "FilterNoise");
		DifferenceParam[index].NoiseWidth = ini.GetInteger(section, "NoiseWidth", 1);
		DifferenceParam[index].NoiseHeight = ini.GetInteger(section, "NoiseHeight", 1);
		DifferenceParam[index].SearchRect2ON = ini.GetBoolean(section, "SearchRect2ON");
		DifferenceParam[index].Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		DifferenceParam[index].Tracker2UseAutoSize = ini.GetBoolean(section, "Tracker2UseAutoSize", defaultValue: true);
		DifferenceParam[index].Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		DifferenceParam[index].Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		DifferenceParam[index].Tracker2AligmentXIndex = ini.GetInteger(section, "Tracker2AligmentXIndex");
		DifferenceParam[index].Tracker2AligmentYIndex = ini.GetInteger(section, "Tracker2AligmentYIndex");
		DifferenceParam[index].Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		DifferenceParam[index].Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		DifferenceParam[index].Tracker2AligmentX = ini.GetInteger(section, "Tracker2AligmentX", 10);
		DifferenceParam[index].Tracker2AligmentY = ini.GetInteger(section, "Tracker2AligmentY", 10);
		DifferenceParam[index].MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
		DifferenceParam[index].UseMaskAsInspArea = ini.GetBoolean(section, "UseMaskAsInspArea");
		DifferenceParam[index].AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		DifferenceParam[index].AutoSize2_SelectIndex = ini.GetInteger(section, "AutoSize2_SelectIndex");
		DifferenceParam[index].Track1_T = ini.GetInteger(section, "Track1_T");
		DifferenceParam[index].Track1_B = ini.GetInteger(section, "Track1_B");
		DifferenceParam[index].Track1_L = ini.GetInteger(section, "Track1_L");
		DifferenceParam[index].Track1_R = ini.GetInteger(section, "Track1_R");
		DifferenceParam[index].Track2_T = ini.GetInteger(section, "Track2_T", 10);
		DifferenceParam[index].Track2_B = ini.GetInteger(section, "Track2_B", 10);
		DifferenceParam[index].Track2_L = ini.GetInteger(section, "Track2_L", 10);
		DifferenceParam[index].Track2_R = ini.GetInteger(section, "Track2_R", 10);
		DifferenceParam[index].ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "ChannelIndex");
		DifferenceParam[index].InspDirection = ini.GetInteger(section, "InspDirection");
		DifferenceParam[index].P2PIntervalX = ini.GetInteger(section, "P2PIntervalX", 5);
		DifferenceParam[index].P2PIntervalY = ini.GetInteger(section, "P2PIntervalY", 5);
		DifferenceParam[index].ThdUpON = ini.GetBoolean(section, "ThdUpON");
		DifferenceParam[index].ThdUp = ini.GetInteger(section, "ThdUp", 10);
		DifferenceParam[index].ThdDownON = ini.GetBoolean(section, "ThdDownON", defaultValue: true);
		DifferenceParam[index].ThdDown = ini.GetInteger(section, "ThdDown", 10);
		DifferenceParam[index].THDLimit_Min = ini.GetInteger(section, "THDLimit_Min");
		DifferenceParam[index].THDLimit_Max = ini.GetInteger(section, "THDLimit_Max", 255);
		DifferenceParam[index].DE_Start_Count = ini.GetInteger(section, "DE_Start_Count");
		DifferenceParam[index].DE_End_Count = ini.GetInteger(section, "DE_End_Count");
		DifferenceParam[index].DE_Start_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start_Index");
		DifferenceParam[index].DE_End_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End_Index");
		DifferenceParam[index].InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		DifferenceParam[index].InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		DifferenceParam[index].InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		DifferenceParam[index].InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		DifferenceParam[index].InspRule1_Min = ini.GetDouble(section, "InspRule1_Min");
		DifferenceParam[index].InspRule2_Min = ini.GetDouble(section, "InspRule2_Min");
		DifferenceParam[index].InspRule3_Min = ini.GetDouble(section, "InspRule3_Min");
		DifferenceParam[index].InspRule4_Min = ini.GetDouble(section, "InspRule4_Min");
		DifferenceParam[index].InspRule1_Max = ini.GetDouble(section, "InspRule1_Max", 10.0);
		DifferenceParam[index].InspRule2_Max = ini.GetDouble(section, "InspRule2_Max", 10.0);
		DifferenceParam[index].InspRule3_Max = ini.GetDouble(section, "InspRule3_Max", 50.0);
		DifferenceParam[index].InspRule4_Max = ini.GetDouble(section, "InspRule4_Max", 50.0);
		DifferenceParam[index].InspRuleAND = ini.GetBoolean(section, "InspRuleAND", defaultValue: true);
		DifferenceParam[index].BlobAnalysisSelect = ini.GetInteger(section, "BlobAnalysisSelect");
		DifferenceParam[index].BlobLineSort = ini.GetInteger(section, "BlobLineSort");
		DifferenceParam[index].BlobLineNum = ini.GetInteger(section, "BlobLineNum", 3);
		DifferenceParam[index].BlobLineMin = ini.GetInteger(section, "BlobLineMin", 1);
		DifferenceParam[index].BlobLineMax = ini.GetInteger(section, "BlobLineMax", 2);
		DifferenceParam[index].BlobLineWidthMin = ini.GetDouble(section, "BlobLineWidthMin");
		DifferenceParam[index].BlobLineWidthMax = ini.GetDouble(section, "BlobLineWidthMax", 5.0);
		DifferenceParam[index].BlobLineHeightMin = ini.GetDouble(section, "BlobLineHeightMin", 50.0);
		DifferenceParam[index].BlobLineHeightMax = ini.GetDouble(section, "BlobLineHeightMax", 100.0);
		DifferenceParam[index].BlobLineAreaMin = ini.GetDouble(section, "BlobLineAreaMin", 100.0);
		DifferenceParam[index].BlobLineAreaMax = ini.GetDouble(section, "BlobLineAreaMax", 500.0);
		DifferenceParam[index].MB_InspRule1_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule1_Index", 1);
		DifferenceParam[index].MB_InspRule2_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule2_Index", 2);
		DifferenceParam[index].MB_InspRule3_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule3_Index", 3);
		DifferenceParam[index].MB_InspRule4_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule4_Index");
		DifferenceParam[index].MB_InspRule1_Min = ini.GetDouble(section, "MB_InspRule1_Min");
		DifferenceParam[index].MB_InspRule2_Min = ini.GetDouble(section, "MB_InspRule2_Min");
		DifferenceParam[index].MB_InspRule3_Min = ini.GetDouble(section, "MB_InspRule3_Min");
		DifferenceParam[index].MB_InspRule4_Min = ini.GetDouble(section, "MB_InspRule4_Min");
		DifferenceParam[index].MB_InspRule1_Max = ini.GetDouble(section, "MB_InspRule1_Max", 10.0);
		DifferenceParam[index].MB_InspRule2_Max = ini.GetDouble(section, "MB_InspRule2_Max", 10.0);
		DifferenceParam[index].MB_InspRule3_Max = ini.GetDouble(section, "MB_InspRule3_Max", 50.0);
		DifferenceParam[index].MB_InspRule4_Max = ini.GetDouble(section, "MB_InspRule4_Max", 50.0);
		DifferenceParam[index].MB_InspRuleAND = ini.GetBoolean(section, "MB_InspRuleAND", defaultValue: true);
		DifferenceParam[index].MB_BlobSort = ini.GetInteger(section, "MB_BlobSort");
		DifferenceParam[index].MB_BlobNum = ini.GetInteger(section, "MB_BlobNum", 3);
		DifferenceParam[index].MB_AllowNGCount = ini.GetInteger(section, "MB_AllowNGCount", 1);
		DifferenceParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		DifferenceParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		DifferenceParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		DifferenceParam[index].UseMatchTextMask1 = ini.GetInteger(section, "UseMatchTextMask1");
		DifferenceParam[index].UseMatchTextMask2 = ini.GetInteger(section, "UseMatchTextMask2");
		DifferenceParam[index].UseMatchTextMask3 = ini.GetInteger(section, "UseMatchTextMask3");
		DifferenceParam[index].MatchTextDE_Start1_Count = ini.GetInteger(section, "MatchTextDE_Start1_Count");
		DifferenceParam[index].MatchTextDE_Start2_Count = ini.GetInteger(section, "MatchTextDE_Start2_Count");
		DifferenceParam[index].MatchTextDE_Start3_Count = ini.GetInteger(section, "MatchTextDE_Start3_Count");
		DifferenceParam[index].MatchTextDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start1_Index");
		DifferenceParam[index].MatchTextDE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start2_Index");
		DifferenceParam[index].MatchTextDE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start3_Index");
		DifferenceParam[index].MaskDE_Start1_Count = ini.GetInteger(section, "MaskDE_Start1_Count");
		DifferenceParam[index].MaskDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MaskDE_Start1_Index");
		DifferenceParam[index].T1FillletCount0 = ini.GetInteger(section, "T1FillletCount0");
		DifferenceParam[index].T1FillletCount1 = ini.GetInteger(section, "T1FillletCount1");
		DifferenceParam[index].T1FillletCount2 = ini.GetInteger(section, "T1FillletCount2");
		DifferenceParam[index].T1FillletCount3 = ini.GetInteger(section, "T1FillletCount3");
		DifferenceParam[index].T2FillletCount0 = ini.GetInteger(section, "T2FillletCount0");
		DifferenceParam[index].T2FillletCount1 = ini.GetInteger(section, "T2FillletCount1");
		DifferenceParam[index].T2FillletCount2 = ini.GetInteger(section, "T2FillletCount2");
		DifferenceParam[index].T2FillletCount3 = ini.GetInteger(section, "T2FillletCount3");
		DifferenceParam[index].TrackErrorSetNG = ini.GetBoolean(section, "TrackErrorSetNG", defaultValue: true);
		DifferenceParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteDifferenceParam(int index)
	{
		string section = "DifferenceTools" + index;
		if (DifferenceParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", DifferenceParam[index].TrackerName);
			DifferenceParam[index].NameChangeFlag = false;
		}
		if (!DifferenceParam[index].ValueChangeFlag)
		{
			return true;
		}
		DifferenceParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", DifferenceParam[index].TrackerON);
		ini.WriteBool(section, "Locked", DifferenceParam[index].Locked);
		ini.WriteString(section, "TrackerName", DifferenceParam[index].TrackerName);
		ini.WriteBool(section, "FilterNoise", DifferenceParam[index].FilterNoise);
		ini.WriteInt(section, "NoiseWidth", DifferenceParam[index].NoiseWidth);
		ini.WriteInt(section, "NoiseHeight", DifferenceParam[index].NoiseHeight);
		ini.WriteBool(section, "SearchRect2ON", DifferenceParam[index].SearchRect2ON);
		ini.WriteBool(section, "Tracker1UseAutoSize", DifferenceParam[index].Tracker1UseAutoSize);
		ini.WriteBool(section, "Tracker2UseAutoSize", DifferenceParam[index].Tracker2UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", DifferenceParam[index].Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker2AligmentXIndex", DifferenceParam[index].Tracker2AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", DifferenceParam[index].Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker2AligmentYIndex", DifferenceParam[index].Tracker2AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", DifferenceParam[index].Tracker1AligmentX);
		ini.WriteInt(section, "Tracker2AligmentX", DifferenceParam[index].Tracker2AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", DifferenceParam[index].Tracker1AligmentY);
		ini.WriteInt(section, "Tracker2AligmentY", DifferenceParam[index].Tracker2AligmentY);
		ini.WriteInt(section, "MaskSelectIndex", DifferenceParam[index].MaskSelectIndex);
		ini.WriteBool(section, "UseMaskAsInspArea", DifferenceParam[index].UseMaskAsInspArea);
		ini.WriteInt(section, "AutoSize1_SelectIndex", DifferenceParam[index].AutoSize1_SelectIndex);
		ini.WriteInt(section, "AutoSize2_SelectIndex", DifferenceParam[index].AutoSize2_SelectIndex);
		ini.WriteInt(section, "Track1_T", DifferenceParam[index].Track1_T);
		ini.WriteInt(section, "Track1_B", DifferenceParam[index].Track1_B);
		ini.WriteInt(section, "Track1_L", DifferenceParam[index].Track1_L);
		ini.WriteInt(section, "Track1_R", DifferenceParam[index].Track1_R);
		ini.WriteInt(section, "Track2_T", DifferenceParam[index].Track2_T);
		ini.WriteInt(section, "Track2_B", DifferenceParam[index].Track2_B);
		ini.WriteInt(section, "Track2_L", DifferenceParam[index].Track2_L);
		ini.WriteInt(section, "Track2_R", DifferenceParam[index].Track2_R);
		ini.WriteInt(section, "ChannelIndex", (int)DifferenceParam[index].ChannelIndex);
		ini.WriteBool(section, "ThdUpON", DifferenceParam[index].ThdUpON);
		ini.WriteInt(section, "ThdUp", DifferenceParam[index].ThdUp);
		ini.WriteBool(section, "ThdDownON", DifferenceParam[index].ThdDownON);
		ini.WriteInt(section, "ThdDown", DifferenceParam[index].ThdDown);
		ini.WriteInt(section, "InspDirection", DifferenceParam[index].InspDirection);
		ini.WriteInt(section, "P2PIntervalX", DifferenceParam[index].P2PIntervalX);
		ini.WriteInt(section, "P2PIntervalY", DifferenceParam[index].P2PIntervalY);
		ini.WriteInt(section, "THDLimit_Min", DifferenceParam[index].THDLimit_Min);
		ini.WriteInt(section, "THDLimit_Max", DifferenceParam[index].THDLimit_Max);
		ini.WriteInt(section, "DE_Start_Count", DifferenceParam[index].DE_Start_Count);
		ini.WriteInt(section, "DE_End_Count", DifferenceParam[index].DE_End_Count);
		ini.WriteInt(section, "DE_Start_Index", (int)DifferenceParam[index].DE_Start_Index);
		ini.WriteInt(section, "DE_End_Index", (int)DifferenceParam[index].DE_End_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)DifferenceParam[index].InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)DifferenceParam[index].InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)DifferenceParam[index].InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)DifferenceParam[index].InspRule4_Index);
		ini.WriteDouble(section, "InspRule1_Min", DifferenceParam[index].InspRule1_Min);
		ini.WriteDouble(section, "InspRule2_Min", DifferenceParam[index].InspRule2_Min);
		ini.WriteDouble(section, "InspRule3_Min", DifferenceParam[index].InspRule3_Min);
		ini.WriteDouble(section, "InspRule4_Min", DifferenceParam[index].InspRule4_Min);
		ini.WriteDouble(section, "InspRule1_Max", DifferenceParam[index].InspRule1_Max);
		ini.WriteDouble(section, "InspRule2_Max", DifferenceParam[index].InspRule2_Max);
		ini.WriteDouble(section, "InspRule3_Max", DifferenceParam[index].InspRule3_Max);
		ini.WriteDouble(section, "InspRule4_Max", DifferenceParam[index].InspRule4_Max);
		ini.WriteBool(section, "InspRuleAND", DifferenceParam[index].InspRuleAND);
		ini.WriteInt(section, "BlobAnalysisSelect", DifferenceParam[index].BlobAnalysisSelect);
		ini.WriteInt(section, "BlobLineSort", DifferenceParam[index].BlobLineSort);
		ini.WriteInt(section, "BlobLineNum", DifferenceParam[index].BlobLineNum);
		ini.WriteInt(section, "BlobLineMin", DifferenceParam[index].BlobLineMin);
		ini.WriteInt(section, "BlobLineMax", DifferenceParam[index].BlobLineMax);
		ini.WriteDouble(section, "BlobLineWidthMin", DifferenceParam[index].BlobLineWidthMin);
		ini.WriteDouble(section, "BlobLineWidthMax", DifferenceParam[index].BlobLineWidthMax);
		ini.WriteDouble(section, "BlobLineHeightMin", DifferenceParam[index].BlobLineHeightMin);
		ini.WriteDouble(section, "BlobLineHeightMax", DifferenceParam[index].BlobLineHeightMax);
		ini.WriteDouble(section, "BlobLineAreaMin", DifferenceParam[index].BlobLineAreaMin);
		ini.WriteDouble(section, "BlobLineAreaMax", DifferenceParam[index].BlobLineAreaMax);
		ini.WriteInt(section, "MB_InspRule1_Index", (int)DifferenceParam[index].MB_InspRule1_Index);
		ini.WriteInt(section, "MB_InspRule2_Index", (int)DifferenceParam[index].MB_InspRule2_Index);
		ini.WriteInt(section, "MB_InspRule3_Index", (int)DifferenceParam[index].MB_InspRule3_Index);
		ini.WriteInt(section, "MB_InspRule4_Index", (int)DifferenceParam[index].MB_InspRule4_Index);
		ini.WriteDouble(section, "MB_InspRule1_Min", DifferenceParam[index].MB_InspRule1_Min);
		ini.WriteDouble(section, "MB_InspRule2_Min", DifferenceParam[index].MB_InspRule2_Min);
		ini.WriteDouble(section, "MB_InspRule3_Min", DifferenceParam[index].MB_InspRule3_Min);
		ini.WriteDouble(section, "MB_InspRule4_Min", DifferenceParam[index].MB_InspRule4_Min);
		ini.WriteDouble(section, "MB_InspRule1_Max", DifferenceParam[index].MB_InspRule1_Max);
		ini.WriteDouble(section, "MB_InspRule2_Max", DifferenceParam[index].MB_InspRule2_Max);
		ini.WriteDouble(section, "MB_InspRule3_Max", DifferenceParam[index].MB_InspRule3_Max);
		ini.WriteDouble(section, "MB_InspRule4_Max", DifferenceParam[index].MB_InspRule4_Max);
		ini.WriteBool(section, "MB_InspRuleAND", DifferenceParam[index].MB_InspRuleAND);
		ini.WriteInt(section, "MB_BlobSort", DifferenceParam[index].MB_BlobSort);
		ini.WriteInt(section, "MB_BlobNum", DifferenceParam[index].MB_BlobNum);
		ini.WriteInt(section, "MB_AllowNGCount", DifferenceParam[index].MB_AllowNGCount);
		ini.WriteBool(section, "EnableSelectFace", DifferenceParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", DifferenceParam[index].FaceIndex);
		ini.WriteInt(section, "ParetoIndex", DifferenceParam[index].ParetoIndex);
		ini.WriteInt(section, "UseMatchTextMask1", DifferenceParam[index].UseMatchTextMask1);
		ini.WriteInt(section, "UseMatchTextMask2", DifferenceParam[index].UseMatchTextMask2);
		ini.WriteInt(section, "UseMatchTextMask3", DifferenceParam[index].UseMatchTextMask3);
		ini.WriteInt(section, "MatchTextDE_Start1_Count", DifferenceParam[index].MatchTextDE_Start1_Count);
		ini.WriteInt(section, "MatchTextDE_Start2_Count", DifferenceParam[index].MatchTextDE_Start2_Count);
		ini.WriteInt(section, "MatchTextDE_Start3_Count", DifferenceParam[index].MatchTextDE_Start3_Count);
		ini.WriteInt(section, "MatchTextDE_Start1_Index", (int)DifferenceParam[index].MatchTextDE_Start1_Index);
		ini.WriteInt(section, "MatchTextDE_Start2_Index", (int)DifferenceParam[index].MatchTextDE_Start2_Index);
		ini.WriteInt(section, "MatchTextDE_Start3_Index", (int)DifferenceParam[index].MatchTextDE_Start3_Index);
		ini.WriteInt(section, "MaskDE_Start1_Count", DifferenceParam[index].MaskDE_Start1_Count);
		ini.WriteInt(section, "MaskDE_Start1_Index", (int)DifferenceParam[index].MaskDE_Start1_Index);
		ini.WriteInt(section, "T1FillletCount0", DifferenceParam[index].T1FillletCount0);
		ini.WriteInt(section, "T1FillletCount1", DifferenceParam[index].T1FillletCount1);
		ini.WriteInt(section, "T1FillletCount2", DifferenceParam[index].T1FillletCount2);
		ini.WriteInt(section, "T1FillletCount3", DifferenceParam[index].T1FillletCount3);
		ini.WriteInt(section, "T2FillletCount0", DifferenceParam[index].T2FillletCount0);
		ini.WriteInt(section, "T2FillletCount1", DifferenceParam[index].T2FillletCount1);
		ini.WriteInt(section, "T2FillletCount2", DifferenceParam[index].T2FillletCount2);
		ini.WriteInt(section, "T2FillletCount3", DifferenceParam[index].T2FillletCount3);
		ini.WriteInt(section, "ErrorBin", (int)DifferenceParam[index].ErrorBin);
		ini.WriteBool(section, "TrackErrorSetNG", DifferenceParam[index].TrackErrorSetNG);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadBlockAvgParam(int index)
	{
		BlockAvgParam[index].NameChangeFlag = false;
		BlockAvgParam[index].ValueChangeFlag = false;
		string section = "BlockAvgTools" + index;
		BlockAvgParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		BlockAvgParam[index].Locked = ini.GetBoolean(section, "Locked");
		BlockAvgParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		BlockAvgParam[index].SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		BlockAvgParam[index].SearchRect2 = ini.ReadRect(section, "SearchRect2", 50);
		BlockAvgParam[index].FilterNoise = ini.GetBoolean(section, "FilterNoise");
		BlockAvgParam[index].NoiseWidth = ini.GetInteger(section, "NoiseWidth", 1);
		BlockAvgParam[index].NoiseHeight = ini.GetInteger(section, "NoiseHeight", 1);
		BlockAvgParam[index].SearchRect2ON = ini.GetBoolean(section, "SearchRect2ON");
		BlockAvgParam[index].Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		BlockAvgParam[index].Tracker2UseAutoSize = ini.GetBoolean(section, "Tracker2UseAutoSize", defaultValue: true);
		BlockAvgParam[index].Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		BlockAvgParam[index].Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		BlockAvgParam[index].Tracker2AligmentXIndex = ini.GetInteger(section, "Tracker2AligmentXIndex");
		BlockAvgParam[index].Tracker2AligmentYIndex = ini.GetInteger(section, "Tracker2AligmentYIndex");
		BlockAvgParam[index].Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		BlockAvgParam[index].Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		BlockAvgParam[index].Tracker2AligmentX = ini.GetInteger(section, "Tracker2AligmentX", 10);
		BlockAvgParam[index].Tracker2AligmentY = ini.GetInteger(section, "Tracker2AligmentY", 10);
		BlockAvgParam[index].MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
		BlockAvgParam[index].UseMaskAsInspArea = ini.GetBoolean(section, "UseMaskAsInspArea");
		BlockAvgParam[index].AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		BlockAvgParam[index].AutoSize2_SelectIndex = ini.GetInteger(section, "AutoSize2_SelectIndex");
		BlockAvgParam[index].Track1_T = ini.GetInteger(section, "Track1_T", 5);
		BlockAvgParam[index].Track1_B = ini.GetInteger(section, "Track1_B", 5);
		BlockAvgParam[index].Track1_L = ini.GetInteger(section, "Track1_L", 5);
		BlockAvgParam[index].Track1_R = ini.GetInteger(section, "Track1_R", 5);
		BlockAvgParam[index].Track2_T = ini.GetInteger(section, "Track2_T", 10);
		BlockAvgParam[index].Track2_B = ini.GetInteger(section, "Track2_B", 10);
		BlockAvgParam[index].Track2_L = ini.GetInteger(section, "Track2_L", 10);
		BlockAvgParam[index].Track2_R = ini.GetInteger(section, "Track2_R", 10);
		BlockAvgParam[index].ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "ChannelIndex");
		BlockAvgParam[index].InspDirection = ini.GetInteger(section, "InspDirection");
		BlockAvgParam[index].P2PIntervalX = ini.GetInteger(section, "P2PIntervalX", 5);
		BlockAvgParam[index].P2PIntervalY = ini.GetInteger(section, "P2PIntervalY", 5);
		BlockAvgParam[index].ThdUpON = ini.GetBoolean(section, "ThdUpON");
		BlockAvgParam[index].ThdUp = ini.GetInteger(section, "ThdUp", 10);
		BlockAvgParam[index].ThdDownON = ini.GetBoolean(section, "ThdDownON", defaultValue: true);
		BlockAvgParam[index].ThdDown = ini.GetInteger(section, "ThdDown", 10);
		BlockAvgParam[index].THDLimit_Min = ini.GetInteger(section, "THDLimit_Min");
		BlockAvgParam[index].THDLimit_Max = ini.GetInteger(section, "THDLimit_Max", 255);
		BlockAvgParam[index].DE_Start_Count = ini.GetInteger(section, "DE_Start_Count");
		BlockAvgParam[index].DE_End_Count = ini.GetInteger(section, "DE_End_Count");
		BlockAvgParam[index].DE_Start_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_Start_Index");
		BlockAvgParam[index].DE_End_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "DE_End_Index");
		BlockAvgParam[index].InspRule1_Index = (BlobCondition)ini.GetInteger(section, "InspRule1_Index", 1);
		BlockAvgParam[index].InspRule2_Index = (BlobCondition)ini.GetInteger(section, "InspRule2_Index", 2);
		BlockAvgParam[index].InspRule3_Index = (BlobCondition)ini.GetInteger(section, "InspRule3_Index", 3);
		BlockAvgParam[index].InspRule4_Index = (BlobCondition)ini.GetInteger(section, "InspRule4_Index");
		BlockAvgParam[index].InspRule1_Min = ini.GetDouble(section, "InspRule1_Min");
		BlockAvgParam[index].InspRule2_Min = ini.GetDouble(section, "InspRule2_Min");
		BlockAvgParam[index].InspRule3_Min = ini.GetDouble(section, "InspRule3_Min");
		BlockAvgParam[index].InspRule4_Min = ini.GetDouble(section, "InspRule4_Min");
		BlockAvgParam[index].InspRule1_Max = ini.GetDouble(section, "InspRule1_Max", 10.0);
		BlockAvgParam[index].InspRule2_Max = ini.GetDouble(section, "InspRule2_Max", 10.0);
		BlockAvgParam[index].InspRule3_Max = ini.GetDouble(section, "InspRule3_Max", 50.0);
		BlockAvgParam[index].InspRule4_Max = ini.GetDouble(section, "InspRule4_Max", 50.0);
		BlockAvgParam[index].InspRuleAND = ini.GetBoolean(section, "InspRuleAND", defaultValue: true);
		BlockAvgParam[index].UseAvgBinary = ini.GetBoolean(section, "UseAvgBinary");
		BlockAvgParam[index].AvgBinaryMethod = ini.GetInteger(section, "AvgBinaryMethod");
		BlockAvgParam[index].AvgShiftValue = ini.GetInteger(section, "AvgShiftValue", 9);
		BlockAvgParam[index].AvgGrayMax = ini.GetInteger(section, "AvgGrayMax", 255);
		BlockAvgParam[index].AvgGrayMin = ini.GetInteger(section, "AvgGrayMin");
		BlockAvgParam[index].BlobAnalysisSelect = ini.GetInteger(section, "BlobAnalysisSelect");
		BlockAvgParam[index].BlobLineSort = ini.GetInteger(section, "BlobLineSort");
		BlockAvgParam[index].BlobLineNum = ini.GetInteger(section, "BlobLineNum", 3);
		BlockAvgParam[index].BlobLineMin = ini.GetInteger(section, "BlobLineMin", 1);
		BlockAvgParam[index].BlobLineMax = ini.GetInteger(section, "BlobLineMax", 2);
		BlockAvgParam[index].BlobLineWidthMin = ini.GetDouble(section, "BlobLineWidthMin");
		BlockAvgParam[index].BlobLineWidthMax = ini.GetDouble(section, "BlobLineWidthMax", 5.0);
		BlockAvgParam[index].BlobLineHeightMin = ini.GetDouble(section, "BlobLineHeightMin", 50.0);
		BlockAvgParam[index].BlobLineHeightMax = ini.GetDouble(section, "BlobLineHeightMax", 100.0);
		BlockAvgParam[index].BlobLineAreaMin = ini.GetDouble(section, "BlobLineAreaMin", 100.0);
		BlockAvgParam[index].BlobLineAreaMax = ini.GetDouble(section, "BlobLineAreaMax", 500.0);
		BlockAvgParam[index].MB_InspRule1_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule1_Index", 1);
		BlockAvgParam[index].MB_InspRule2_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule2_Index", 2);
		BlockAvgParam[index].MB_InspRule3_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule3_Index", 3);
		BlockAvgParam[index].MB_InspRule4_Index = (BlobCondition)ini.GetInteger(section, "MB_InspRule4_Index");
		BlockAvgParam[index].MB_InspRule1_Min = ini.GetDouble(section, "MB_InspRule1_Min");
		BlockAvgParam[index].MB_InspRule2_Min = ini.GetDouble(section, "MB_InspRule2_Min");
		BlockAvgParam[index].MB_InspRule3_Min = ini.GetDouble(section, "MB_InspRule3_Min");
		BlockAvgParam[index].MB_InspRule4_Min = ini.GetDouble(section, "MB_InspRule4_Min");
		BlockAvgParam[index].MB_InspRule1_Max = ini.GetDouble(section, "MB_InspRule1_Max", 10.0);
		BlockAvgParam[index].MB_InspRule2_Max = ini.GetDouble(section, "MB_InspRule2_Max", 10.0);
		BlockAvgParam[index].MB_InspRule3_Max = ini.GetDouble(section, "MB_InspRule3_Max", 50.0);
		BlockAvgParam[index].MB_InspRule4_Max = ini.GetDouble(section, "MB_InspRule4_Max", 50.0);
		BlockAvgParam[index].MB_InspRuleAND = ini.GetBoolean(section, "MB_InspRuleAND", defaultValue: true);
		BlockAvgParam[index].MB_BlobSort = ini.GetInteger(section, "MB_BlobSort");
		BlockAvgParam[index].MB_BlobNum = ini.GetInteger(section, "MB_BlobNum", 3);
		BlockAvgParam[index].MB_AllowNGCount = ini.GetInteger(section, "MB_AllowNGCount", 1);
		BlockAvgParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		BlockAvgParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		BlockAvgParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		BlockAvgParam[index].UseMatchTextMask1 = ini.GetInteger(section, "UseMatchTextMask1");
		BlockAvgParam[index].UseMatchTextMask2 = ini.GetInteger(section, "UseMatchTextMask2");
		BlockAvgParam[index].UseMatchTextMask3 = ini.GetInteger(section, "UseMatchTextMask3");
		BlockAvgParam[index].MatchTextDE_Start1_Count = ini.GetInteger(section, "MatchTextDE_Start1_Count");
		BlockAvgParam[index].MatchTextDE_Start2_Count = ini.GetInteger(section, "MatchTextDE_Start2_Count");
		BlockAvgParam[index].MatchTextDE_Start3_Count = ini.GetInteger(section, "MatchTextDE_Start3_Count");
		BlockAvgParam[index].MatchTextDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start1_Index");
		BlockAvgParam[index].MatchTextDE_Start2_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start2_Index");
		BlockAvgParam[index].MatchTextDE_Start3_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MatchTextDE_Start3_Index");
		BlockAvgParam[index].MaskDE_Start1_Count = ini.GetInteger(section, "MaskDE_Start1_Count");
		BlockAvgParam[index].MaskDE_Start1_Index = (USE_DILATE_ERODE)ini.GetInteger(section, "MaskDE_Start1_Index");
		BlockAvgParam[index].T1FillletCount0 = ini.GetInteger(section, "T1FillletCount0");
		BlockAvgParam[index].T1FillletCount1 = ini.GetInteger(section, "T1FillletCount1");
		BlockAvgParam[index].T1FillletCount2 = ini.GetInteger(section, "T1FillletCount2");
		BlockAvgParam[index].T1FillletCount3 = ini.GetInteger(section, "T1FillletCount3");
		BlockAvgParam[index].T2FillletCount0 = ini.GetInteger(section, "T2FillletCount0");
		BlockAvgParam[index].T2FillletCount1 = ini.GetInteger(section, "T2FillletCount1");
		BlockAvgParam[index].T2FillletCount2 = ini.GetInteger(section, "T2FillletCount2");
		BlockAvgParam[index].T2FillletCount3 = ini.GetInteger(section, "T2FillletCount3");
		BlockAvgParam[index].TrackErrorSetNG = ini.GetBoolean(section, "TrackErrorSetNG", defaultValue: true);
		BlockAvgParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteBlockAvgParam(int index)
	{
		string section = "BlockAvgTools" + index;
		if (BlockAvgParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", BlockAvgParam[index].TrackerName);
			BlockAvgParam[index].NameChangeFlag = false;
		}
		if (!BlockAvgParam[index].ValueChangeFlag)
		{
			return true;
		}
		BlockAvgParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", BlockAvgParam[index].TrackerON);
		ini.WriteBool(section, "Locked", BlockAvgParam[index].Locked);
		ini.WriteString(section, "TrackerName", BlockAvgParam[index].TrackerName);
		ini.WriteBool(section, "FilterNoise", BlockAvgParam[index].FilterNoise);
		ini.WriteInt(section, "NoiseWidth", BlockAvgParam[index].NoiseWidth);
		ini.WriteInt(section, "NoiseHeight", BlockAvgParam[index].NoiseHeight);
		ini.WriteBool(section, "SearchRect2ON", BlockAvgParam[index].SearchRect2ON);
		ini.WriteBool(section, "Tracker1UseAutoSize", BlockAvgParam[index].Tracker1UseAutoSize);
		ini.WriteBool(section, "Tracker2UseAutoSize", BlockAvgParam[index].Tracker2UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", BlockAvgParam[index].Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker2AligmentXIndex", BlockAvgParam[index].Tracker2AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", BlockAvgParam[index].Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker2AligmentYIndex", BlockAvgParam[index].Tracker2AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", BlockAvgParam[index].Tracker1AligmentX);
		ini.WriteInt(section, "Tracker2AligmentX", BlockAvgParam[index].Tracker2AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", BlockAvgParam[index].Tracker1AligmentY);
		ini.WriteInt(section, "Tracker2AligmentY", BlockAvgParam[index].Tracker2AligmentY);
		ini.WriteInt(section, "MaskSelectIndex", BlockAvgParam[index].MaskSelectIndex);
		ini.WriteBool(section, "UseMaskAsInspArea", BlockAvgParam[index].UseMaskAsInspArea);
		ini.WriteInt(section, "AutoSize1_SelectIndex", BlockAvgParam[index].AutoSize1_SelectIndex);
		ini.WriteInt(section, "AutoSize2_SelectIndex", BlockAvgParam[index].AutoSize2_SelectIndex);
		ini.WriteInt(section, "Track1_T", BlockAvgParam[index].Track1_T);
		ini.WriteInt(section, "Track1_B", BlockAvgParam[index].Track1_B);
		ini.WriteInt(section, "Track1_L", BlockAvgParam[index].Track1_L);
		ini.WriteInt(section, "Track1_R", BlockAvgParam[index].Track1_R);
		ini.WriteInt(section, "Track2_T", BlockAvgParam[index].Track2_T);
		ini.WriteInt(section, "Track2_B", BlockAvgParam[index].Track2_B);
		ini.WriteInt(section, "Track2_L", BlockAvgParam[index].Track2_L);
		ini.WriteInt(section, "Track2_R", BlockAvgParam[index].Track2_R);
		ini.WriteInt(section, "ChannelIndex", (int)BlockAvgParam[index].ChannelIndex);
		ini.WriteBool(section, "ThdUpON", BlockAvgParam[index].ThdUpON);
		ini.WriteInt(section, "ThdUp", BlockAvgParam[index].ThdUp);
		ini.WriteBool(section, "ThdDownON", BlockAvgParam[index].ThdDownON);
		ini.WriteInt(section, "ThdDown", BlockAvgParam[index].ThdDown);
		ini.WriteInt(section, "InspDirection", BlockAvgParam[index].InspDirection);
		ini.WriteInt(section, "P2PIntervalX", BlockAvgParam[index].P2PIntervalX);
		ini.WriteInt(section, "P2PIntervalY", BlockAvgParam[index].P2PIntervalY);
		ini.WriteInt(section, "THDLimit_Min", BlockAvgParam[index].THDLimit_Min);
		ini.WriteInt(section, "THDLimit_Max", BlockAvgParam[index].THDLimit_Max);
		ini.WriteInt(section, "DE_Start_Count", BlockAvgParam[index].DE_Start_Count);
		ini.WriteInt(section, "DE_End_Count", BlockAvgParam[index].DE_End_Count);
		ini.WriteInt(section, "DE_Start_Index", (int)BlockAvgParam[index].DE_Start_Index);
		ini.WriteInt(section, "DE_End_Index", (int)BlockAvgParam[index].DE_End_Index);
		ini.WriteInt(section, "InspRule1_Index", (int)BlockAvgParam[index].InspRule1_Index);
		ini.WriteInt(section, "InspRule2_Index", (int)BlockAvgParam[index].InspRule2_Index);
		ini.WriteInt(section, "InspRule3_Index", (int)BlockAvgParam[index].InspRule3_Index);
		ini.WriteInt(section, "InspRule4_Index", (int)BlockAvgParam[index].InspRule4_Index);
		ini.WriteDouble(section, "InspRule1_Min", BlockAvgParam[index].InspRule1_Min);
		ini.WriteDouble(section, "InspRule2_Min", BlockAvgParam[index].InspRule2_Min);
		ini.WriteDouble(section, "InspRule3_Min", BlockAvgParam[index].InspRule3_Min);
		ini.WriteDouble(section, "InspRule4_Min", BlockAvgParam[index].InspRule4_Min);
		ini.WriteDouble(section, "InspRule1_Max", BlockAvgParam[index].InspRule1_Max);
		ini.WriteDouble(section, "InspRule2_Max", BlockAvgParam[index].InspRule2_Max);
		ini.WriteDouble(section, "InspRule3_Max", BlockAvgParam[index].InspRule3_Max);
		ini.WriteDouble(section, "InspRule4_Max", BlockAvgParam[index].InspRule4_Max);
		ini.WriteBool(section, "InspRuleAND", BlockAvgParam[index].InspRuleAND);
		ini.WriteBool(section, "UseAvgBinary", BlockAvgParam[index].UseAvgBinary);
		ini.WriteInt(section, "AvgBinaryMethod", BlockAvgParam[index].AvgBinaryMethod);
		ini.WriteInt(section, "AvgShiftValue", BlockAvgParam[index].AvgShiftValue);
		ini.WriteInt(section, "AvgGrayMax", BlockAvgParam[index].AvgGrayMax);
		ini.WriteInt(section, "AvgGrayMin", BlockAvgParam[index].AvgGrayMin);
		ini.WriteInt(section, "BlobAnalysisSelect", BlockAvgParam[index].BlobAnalysisSelect);
		ini.WriteInt(section, "BlobLineSort", BlockAvgParam[index].BlobLineSort);
		ini.WriteInt(section, "BlobLineNum", BlockAvgParam[index].BlobLineNum);
		ini.WriteInt(section, "BlobLineMin", BlockAvgParam[index].BlobLineMin);
		ini.WriteInt(section, "BlobLineMax", BlockAvgParam[index].BlobLineMax);
		ini.WriteDouble(section, "BlobLineWidthMin", BlockAvgParam[index].BlobLineWidthMin);
		ini.WriteDouble(section, "BlobLineWidthMax", BlockAvgParam[index].BlobLineWidthMax);
		ini.WriteDouble(section, "BlobLineHeightMin", BlockAvgParam[index].BlobLineHeightMin);
		ini.WriteDouble(section, "BlobLineHeightMax", BlockAvgParam[index].BlobLineHeightMax);
		ini.WriteDouble(section, "BlobLineAreaMin", BlockAvgParam[index].BlobLineAreaMin);
		ini.WriteDouble(section, "BlobLineAreaMax", BlockAvgParam[index].BlobLineAreaMax);
		ini.WriteInt(section, "MB_InspRule1_Index", (int)BlockAvgParam[index].MB_InspRule1_Index);
		ini.WriteInt(section, "MB_InspRule2_Index", (int)BlockAvgParam[index].MB_InspRule2_Index);
		ini.WriteInt(section, "MB_InspRule3_Index", (int)BlockAvgParam[index].MB_InspRule3_Index);
		ini.WriteInt(section, "MB_InspRule4_Index", (int)BlockAvgParam[index].MB_InspRule4_Index);
		ini.WriteDouble(section, "MB_InspRule1_Min", BlockAvgParam[index].MB_InspRule1_Min);
		ini.WriteDouble(section, "MB_InspRule2_Min", BlockAvgParam[index].MB_InspRule2_Min);
		ini.WriteDouble(section, "MB_InspRule3_Min", BlockAvgParam[index].MB_InspRule3_Min);
		ini.WriteDouble(section, "MB_InspRule4_Min", BlockAvgParam[index].MB_InspRule4_Min);
		ini.WriteDouble(section, "MB_InspRule1_Max", BlockAvgParam[index].MB_InspRule1_Max);
		ini.WriteDouble(section, "MB_InspRule2_Max", BlockAvgParam[index].MB_InspRule2_Max);
		ini.WriteDouble(section, "MB_InspRule3_Max", BlockAvgParam[index].MB_InspRule3_Max);
		ini.WriteDouble(section, "MB_InspRule4_Max", BlockAvgParam[index].MB_InspRule4_Max);
		ini.WriteBool(section, "MB_InspRuleAND", BlockAvgParam[index].MB_InspRuleAND);
		ini.WriteInt(section, "MB_BlobSort", BlockAvgParam[index].MB_BlobSort);
		ini.WriteInt(section, "MB_BlobNum", BlockAvgParam[index].MB_BlobNum);
		ini.WriteInt(section, "MB_AllowNGCount", BlockAvgParam[index].MB_AllowNGCount);
		ini.WriteBool(section, "EnableSelectFace", BlockAvgParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", BlockAvgParam[index].FaceIndex);
		ini.WriteInt(section, "ParetoIndex", BlockAvgParam[index].ParetoIndex);
		ini.WriteInt(section, "UseMatchTextMask1", BlockAvgParam[index].UseMatchTextMask1);
		ini.WriteInt(section, "UseMatchTextMask2", BlockAvgParam[index].UseMatchTextMask2);
		ini.WriteInt(section, "UseMatchTextMask3", BlockAvgParam[index].UseMatchTextMask3);
		ini.WriteInt(section, "MatchTextDE_Start1_Count", BlockAvgParam[index].MatchTextDE_Start1_Count);
		ini.WriteInt(section, "MatchTextDE_Start2_Count", BlockAvgParam[index].MatchTextDE_Start2_Count);
		ini.WriteInt(section, "MatchTextDE_Start3_Count", BlockAvgParam[index].MatchTextDE_Start3_Count);
		ini.WriteInt(section, "MatchTextDE_Start1_Index", (int)BlockAvgParam[index].MatchTextDE_Start1_Index);
		ini.WriteInt(section, "MatchTextDE_Start2_Index", (int)BlockAvgParam[index].MatchTextDE_Start2_Index);
		ini.WriteInt(section, "MatchTextDE_Start3_Index", (int)BlockAvgParam[index].MatchTextDE_Start3_Index);
		ini.WriteInt(section, "MaskDE_Start1_Count", BlockAvgParam[index].MaskDE_Start1_Count);
		ini.WriteInt(section, "MaskDE_Start1_Index", (int)BlockAvgParam[index].MaskDE_Start1_Index);
		ini.WriteInt(section, "T1FillletCount0", BlockAvgParam[index].T1FillletCount0);
		ini.WriteInt(section, "T1FillletCount1", BlockAvgParam[index].T1FillletCount1);
		ini.WriteInt(section, "T1FillletCount2", BlockAvgParam[index].T1FillletCount2);
		ini.WriteInt(section, "T1FillletCount3", BlockAvgParam[index].T1FillletCount3);
		ini.WriteInt(section, "T2FillletCount0", BlockAvgParam[index].T2FillletCount0);
		ini.WriteInt(section, "T2FillletCount1", BlockAvgParam[index].T2FillletCount1);
		ini.WriteInt(section, "T2FillletCount2", BlockAvgParam[index].T2FillletCount2);
		ini.WriteInt(section, "T2FillletCount3", BlockAvgParam[index].T2FillletCount3);
		ini.WriteBool(section, "TrackErrorSetNG", BlockAvgParam[index].TrackErrorSetNG);
		ini.WriteInt(section, "ErrorBin", (int)BlockAvgParam[index].ErrorBin);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadLineAnalysisParam(int index)
	{
		LineAnalysisParam[index].NameChangeFlag = false;
		LineAnalysisParam[index].ValueChangeFlag = false;
		string section = "LineAnalysisTool" + index;
		LineAnalysisParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		LineAnalysisParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		LineAnalysisParam[index].Locked = ini.GetBoolean(section, "Locked");
		LineAnalysisParam[index].SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		LineAnalysisParam[index].Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		LineAnalysisParam[index].Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		LineAnalysisParam[index].Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		LineAnalysisParam[index].Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		LineAnalysisParam[index].Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		LineAnalysisParam[index].MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
		LineAnalysisParam[index].UseMaskAsInspArea = ini.GetBoolean(section, "UseMaskAsInspArea");
		LineAnalysisParam[index].AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		LineAnalysisParam[index].Track1_T = ini.GetInteger(section, "Track1_T", 5);
		LineAnalysisParam[index].Track1_B = ini.GetInteger(section, "Track1_B", 5);
		LineAnalysisParam[index].Track1_L = ini.GetInteger(section, "Track1_L", 5);
		LineAnalysisParam[index].Track1_R = ini.GetInteger(section, "Track1_R", 5);
		LineAnalysisParam[index].THD1_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD1_ChannelIndex");
		LineAnalysisParam[index].THD2_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD2_ChannelIndex", 1);
		LineAnalysisParam[index].THD3_ChannelIndex = (COLOR_CHANNEL)ini.GetInteger(section, "THD3_ChannelIndex", 2);
		LineAnalysisParam[index].THD23_ChannelON = ini.GetBoolean(section, "THD23_ChannelON");
		LineAnalysisParam[index].THD1_Min = ini.GetInteger(section, "THD1_Min");
		LineAnalysisParam[index].THD2_Min = ini.GetInteger(section, "THD2_Min");
		LineAnalysisParam[index].THD3_Min = ini.GetInteger(section, "THD3_Min");
		LineAnalysisParam[index].THD1_Max = ini.GetInteger(section, "THD1_Max", 255);
		LineAnalysisParam[index].THD2_Max = ini.GetInteger(section, "THD2_Max", 255);
		LineAnalysisParam[index].THD3_Max = ini.GetInteger(section, "THD3_Max", 255);
		LineAnalysisParam[index].THD1_DEStart = ini.GetInteger(section, "THD1_DEStart");
		LineAnalysisParam[index].THD1_DEStartCount = ini.GetInteger(section, "THD1_DEStartCount");
		LineAnalysisParam[index].THD2_DEStart = ini.GetInteger(section, "THD2_DEStart");
		LineAnalysisParam[index].THD2_DEStartCount = ini.GetInteger(section, "THD2_DEStartCount");
		LineAnalysisParam[index].THD3_DEStart = ini.GetInteger(section, "THD3_DEStart");
		LineAnalysisParam[index].THD3_DEStartCount = ini.GetInteger(section, "THD3_DEStartCount");
		LineAnalysisParam[index].THD1_DEEnd = ini.GetInteger(section, "THD1_DEEnd");
		LineAnalysisParam[index].THD1_DEEndCount = ini.GetInteger(section, "THD1_DEEndCount");
		LineAnalysisParam[index].THD2_DEEnd = ini.GetInteger(section, "THD2_DEEnd");
		LineAnalysisParam[index].THD2_DEEndCount = ini.GetInteger(section, "THD2_DEEndCount");
		LineAnalysisParam[index].THD3_DEEnd = ini.GetInteger(section, "THD3_DEEnd");
		LineAnalysisParam[index].THD3_DEEndCount = ini.GetInteger(section, "THD3_DEEndCount");
		LineAnalysisParam[index].UseStdFilter = ini.GetBoolean(section, "UseStdFilter");
		LineAnalysisParam[index].UseLineFilter = ini.GetBoolean(section, "UseLineFilter");
		LineAnalysisParam[index].EnableDistanceCheck = ini.GetBoolean(section, "EnableDistanceCheck");
		LineAnalysisParam[index].AllowDistance = ini.GetDouble(section, "AllowDistance", 5.0);
		LineAnalysisParam[index].AllowDistanceContErrorCount = ini.GetInteger(section, "AllowDistanceContErrorCount", 3);
		LineAnalysisParam[index].AllowDistanceErrorCount = ini.GetInteger(section, "AllowDistanceErrorCount", 10);
		LineAnalysisParam[index].DistanceLineSegment = ini.GetInteger(section, "DistanceLineSegment");
		LineAnalysisParam[index].FindEdgeMethod = ini.GetInteger(section, "FindEdgeMethod");
		LineAnalysisParam[index].Direction = (ScanDirection)ini.GetInteger(section, "Direction");
		LineAnalysisParam[index].ContinueError = ini.GetInteger(section, "ContinueError", 2);
		LineAnalysisParam[index].TotalError = ini.GetInteger(section, "TotalError", 5);
		LineAnalysisParam[index].P2PInterval = ini.GetInteger(section, "P2PInterval", 3);
		LineAnalysisParam[index].P2PAllowValue = ini.GetInteger(section, "P2PAllowValue", 5);
		LineAnalysisParam[index].LineAngleMin = ini.GetDouble(section, "LineAngleMin", -90.0);
		LineAnalysisParam[index].LineAngleMax = ini.GetDouble(section, "LineAngleMax", 90.0);
		LineAnalysisParam[index].LineSegment = ini.GetInteger(section, "LineSegment");
		LineAnalysisParam[index].LineAngleDiff = ini.GetDouble(section, "LineAngleDiff", 20.0);
		LineAnalysisParam[index].EnableAngleCheck = ini.GetBoolean(section, "EnableAngleCheck", defaultValue: true);
		LineAnalysisParam[index].EnablePointCheck = ini.GetBoolean(section, "EnablePointCheck", defaultValue: true);
		LineAnalysisParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		LineAnalysisParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		LineAnalysisParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		LineAnalysisParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteLineAnalysisParam(int index)
	{
		string section = "LineAnalysisTool" + index;
		if (LineAnalysisParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", LineAnalysisParam[index].TrackerName);
			LineAnalysisParam[index].NameChangeFlag = false;
		}
		if (!LineAnalysisParam[index].ValueChangeFlag)
		{
			return true;
		}
		LineAnalysisParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", LineAnalysisParam[index].TrackerON);
		ini.WriteString(section, "TrackerName", LineAnalysisParam[index].TrackerName);
		ini.WriteBool(section, "Locked", LineAnalysisParam[index].Locked);
		ini.WriteBool(section, "Tracker1UseAutoSize", LineAnalysisParam[index].Tracker1UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", LineAnalysisParam[index].Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", LineAnalysisParam[index].Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", LineAnalysisParam[index].Tracker1AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", LineAnalysisParam[index].Tracker1AligmentY);
		ini.WriteInt(section, "MaskSelectIndex", LineAnalysisParam[index].MaskSelectIndex);
		ini.WriteBool(section, "UseMaskAsInspArea", LineAnalysisParam[index].UseMaskAsInspArea);
		ini.WriteInt(section, "AutoSize1_SelectIndex", LineAnalysisParam[index].AutoSize1_SelectIndex);
		ini.WriteInt(section, "Track1_T", LineAnalysisParam[index].Track1_T);
		ini.WriteInt(section, "Track1_B", LineAnalysisParam[index].Track1_B);
		ini.WriteInt(section, "Track1_L", LineAnalysisParam[index].Track1_L);
		ini.WriteInt(section, "Track1_R", LineAnalysisParam[index].Track1_R);
		ini.WriteInt(section, "THD1_ChannelIndex", (int)LineAnalysisParam[index].THD1_ChannelIndex);
		ini.WriteInt(section, "THD2_ChannelIndex", (int)LineAnalysisParam[index].THD2_ChannelIndex);
		ini.WriteInt(section, "THD3_ChannelIndex", (int)LineAnalysisParam[index].THD3_ChannelIndex);
		ini.WriteBool(section, "THD23_ChannelON", LineAnalysisParam[index].THD23_ChannelON);
		ini.WriteInt(section, "THD1_Min", LineAnalysisParam[index].THD1_Min);
		ini.WriteInt(section, "THD2_Min", LineAnalysisParam[index].THD2_Min);
		ini.WriteInt(section, "THD3_Min", LineAnalysisParam[index].THD3_Min);
		ini.WriteInt(section, "THD1_Max", LineAnalysisParam[index].THD1_Max);
		ini.WriteInt(section, "THD2_Max", LineAnalysisParam[index].THD2_Max);
		ini.WriteInt(section, "THD3_Max", LineAnalysisParam[index].THD3_Max);
		ini.WriteInt(section, "THD1_DEStart", LineAnalysisParam[index].THD1_DEStart);
		ini.WriteInt(section, "THD1_DEStartCount", LineAnalysisParam[index].THD1_DEStartCount);
		ini.WriteInt(section, "THD2_DEStart", LineAnalysisParam[index].THD2_DEStart);
		ini.WriteInt(section, "THD2_DEStartCount", LineAnalysisParam[index].THD2_DEStartCount);
		ini.WriteInt(section, "THD3_DEStart", LineAnalysisParam[index].THD3_DEStart);
		ini.WriteInt(section, "THD3_DEStartCount", LineAnalysisParam[index].THD3_DEStartCount);
		ini.WriteInt(section, "THD1_DEEnd", LineAnalysisParam[index].THD1_DEEnd);
		ini.WriteInt(section, "THD1_DEEndCount", LineAnalysisParam[index].THD1_DEEndCount);
		ini.WriteInt(section, "THD2_DEEnd", LineAnalysisParam[index].THD2_DEEnd);
		ini.WriteInt(section, "THD2_DEEndCount", LineAnalysisParam[index].THD2_DEEndCount);
		ini.WriteInt(section, "THD3_DEEnd", LineAnalysisParam[index].THD3_DEEnd);
		ini.WriteInt(section, "THD3_DEEndCount", LineAnalysisParam[index].THD3_DEEndCount);
		ini.WriteBool(section, "UseStdFilter", LineAnalysisParam[index].UseStdFilter);
		ini.WriteBool(section, "UseLineFilter", LineAnalysisParam[index].UseLineFilter);
		ini.WriteBool(section, "EnableDistanceCheck", LineAnalysisParam[index].EnableDistanceCheck);
		ini.WriteDouble(section, "AllowDistance", LineAnalysisParam[index].AllowDistance);
		ini.WriteInt(section, "AllowDistanceContErrorCount", LineAnalysisParam[index].AllowDistanceContErrorCount);
		ini.WriteInt(section, "AllowDistanceErrorCount", LineAnalysisParam[index].AllowDistanceErrorCount);
		ini.WriteInt(section, "DistanceLineSegment", LineAnalysisParam[index].DistanceLineSegment);
		ini.WriteInt(section, "FindEdgeMethod", LineAnalysisParam[index].FindEdgeMethod);
		ini.WriteInt(section, "Direction", (int)LineAnalysisParam[index].Direction);
		ini.WriteInt(section, "ContinueError", LineAnalysisParam[index].ContinueError);
		ini.WriteInt(section, "TotalError", LineAnalysisParam[index].TotalError);
		ini.WriteInt(section, "P2PInterval", LineAnalysisParam[index].P2PInterval);
		ini.WriteInt(section, "P2PAllowValue", LineAnalysisParam[index].P2PAllowValue);
		ini.WriteInt(section, "LineSegment", LineAnalysisParam[index].LineSegment);
		ini.WriteDouble(section, "LineAngleMin", LineAnalysisParam[index].LineAngleMin);
		ini.WriteDouble(section, "LineAngleMax", LineAnalysisParam[index].LineAngleMax);
		ini.WriteDouble(section, "LineAngleDiff", LineAnalysisParam[index].LineAngleDiff);
		ini.WriteBool(section, "EnableAngleCheck", LineAnalysisParam[index].EnableAngleCheck);
		ini.WriteBool(section, "EnablePointCheck", LineAnalysisParam[index].EnablePointCheck);
		ini.WriteBool(section, "EnableSelectFace", LineAnalysisParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", LineAnalysisParam[index].FaceIndex);
		ini.WriteInt(section, "ParetoIndex", LineAnalysisParam[index].ParetoIndex);
		ini.WriteInt(section, "ErrorBin", (int)LineAnalysisParam[index].ErrorBin);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadMatchParam(int index)
	{
		MatchParam[index].NameChangeFlag = false;
		MatchParam[index].ValueChangeFlag = false;
		string section = "MatchTools" + index;
		MatchParam[index].TrackerON = ini.GetBoolean(section, "TrackerON");
		MatchParam[index].TrackerName = ini.GetValue(section, "TrackerName", "Unused");
		MatchParam[index].Locked = ini.GetBoolean(section, "Locked");
		MatchParam[index].SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		MatchParam[index].Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		MatchParam[index].Tracker1AligmentXIndex = ini.GetInteger(section, "Tracker1AligmentXIndex");
		MatchParam[index].Tracker1AligmentYIndex = ini.GetInteger(section, "Tracker1AligmentYIndex");
		MatchParam[index].Tracker1AligmentX = ini.GetInteger(section, "Tracker1AligmentX");
		MatchParam[index].Tracker1AligmentY = ini.GetInteger(section, "Tracker1AligmentY");
		MatchParam[index].AutoSize1_SelectIndex = ini.GetInteger(section, "AutoSize1_SelectIndex");
		MatchParam[index].Track1_T = ini.GetInteger(section, "Track1_T", 5);
		MatchParam[index].Track1_B = ini.GetInteger(section, "Track1_B", 5);
		MatchParam[index].Track1_L = ini.GetInteger(section, "Track1_L", 5);
		MatchParam[index].Track1_R = ini.GetInteger(section, "Track1_R", 5);
		MatchParam[index].PatternSelect = ini.GetInteger(section, "PatternSelect", -1);
		MatchParam[index].PatternSelect2 = ini.GetInteger(section, "PatternSelect2", -1);
		MatchParam[index].PatternSelect3 = ini.GetInteger(section, "PatternSelect3", -1);
		MatchParam[index].PatternSelect4 = ini.GetInteger(section, "PatternSelect4", -1);
		MatchParam[index].MatchScoreMin = ini.GetInteger(section, "MatchScoreMin", 60);
		MatchParam[index].MatchScoreMax = ini.GetInteger(section, "MatchScoreMax", 100);
		MatchParam[index].ShapeMatchScoreMin = ini.GetInteger(section, "ShapeMatchScoreMin", 60);
		MatchParam[index].MinMatchShapoeScore = ini.GetInteger(section, "MinMatchShapoeScore", 60);
		MatchParam[index].UseTextMatch = ini.GetBoolean(section, "UseTextMatch", defaultValue: true);
		MatchParam[index].UseFoundToMask = ini.GetBoolean(section, "UseFoundToMask", defaultValue: true);
		MatchParam[index].TextisWhite = ini.GetBoolean(section, "TextisWhite", defaultValue: true);
		MatchParam[index].TextPosDiff = ini.GetInteger(section, "TextPosDiff", 60);
		MatchParam[index].TextAreaDiff = ini.GetInteger(section, "TextAreaDiff", 60);
		MatchParam[index].TextWidthDiff = ini.GetInteger(section, "TextWidthDiff", 60);
		MatchParam[index].TextHeightDiff = ini.GetInteger(section, "TextHeightDiff", 60);
		MatchParam[index].MinColorMatchScore = ini.GetDouble(section, "MinColorMatchScore", 60.0);
		MatchParam[index].EnableMatchShape = ini.GetBoolean(section, "EnableMatchShape");
		MatchParam[index].ShapeTextisWhite = ini.GetBoolean(section, "ShapeTextisWhite");
		MatchParam[index].EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		MatchParam[index].FaceIndex = ini.GetInteger(section, "FaceIndex");
		MatchParam[index].ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		MatchParam[index].EnableRotateInspect = ini.GetBoolean(section, "EnableRotateInspect");
		MatchParam[index].FindCloseArea = ini.GetInteger(section, "FindCloseArea");
		MatchParam[index].FindTextArea = ini.GetInteger(section, "FindTextArea");
		MatchParam[index].ClosedArea = ini.GetDouble(section, "ClosedArea", 100.0);
		MatchParam[index].MaxClosedArea = ini.GetDouble(section, "MaxClosedArea", 100.0);
		MatchParam[index].TextArea = ini.GetDouble(section, "TextArea", 100.0);
		MatchParam[index].MaxTextArea = ini.GetDouble(section, "MaxTextArea", 1000.0);
		MatchParam[index].MinTextWidth = ini.GetDouble(section, "MinTextWidth", 100.0);
		MatchParam[index].MinTextHeight = ini.GetDouble(section, "MinTextHeight", 100.0);
		MatchParam[index].MaxTextWidth = ini.GetDouble(section, "MaxTextWidth", 1000000.0);
		MatchParam[index].MaxTextHeight = ini.GetDouble(section, "MaxTextHeight", 1000000.0);
		MatchParam[index].DEStart = ini.GetInteger(section, "DEStart");
		MatchParam[index].DEStartCount = ini.GetInteger(section, "DEStartCount");
		MatchParam[index].UseOCR = ini.GetBoolean(section, "UseOCR");
		MatchParam[index].OCRText = ini.GetValue(section, "OCRText", "123");
		MatchParam[index].OCRImageChannel = ini.GetInteger(section, "OCRImageChannel");
		MatchParam[index].OCRTextisWhite = ini.GetBoolean(section, "OCRTextisWhite");
		MatchParam[index].UsePublicDB = ini.GetBoolean(section, "UsePublicDB");
		MatchParam[index].QuickSetupFinish = ini.GetBoolean(section, "QuickSetupFinish");
		MatchParam[index].QuickSetupIndex = ini.GetInteger(section, "QuickSetupIndex", -1);
		MatchParam[index].QuickSetupName = ini.GetValue(section, "QuickSetupName", "-----");
		MatchParam[index].MaskInFound = ini.GetBoolean(section, "MaskInFound");
		MatchParam[index].EnableManualThd = ini.GetBoolean(section, "EnableManualThd");
		MatchParam[index].TxtBinaryThdMin = ini.GetInteger(section, "TxtBinaryThdMin");
		MatchParam[index].TxtBinaryThdMax = ini.GetInteger(section, "TxtBinaryThdMax", 255);
		MatchParam[index].EnableAISimilarityCheck = ini.GetBoolean(section, "EnableAISimilarityCheck");
		MatchParam[index].AISimilarityScore = ini.GetInteger(section, "AISimilarityScore", 75);
		MatchParam[index].OnnxName = ini.GetValue(section, "OnnxName");
		MatchParam[index].ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		MatchParam[index].AlignToMatchText = ini.GetBoolean(section, "AlignToMatchText");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteMatchParam(int index)
	{
		string section = "MatchTools" + index;
		if (MatchParam[index].NameChangeFlag)
		{
			ini.WriteString(section, "TrackerName", MatchParam[index].TrackerName);
			MatchParam[index].NameChangeFlag = false;
		}
		if (!MatchParam[index].ValueChangeFlag)
		{
			return true;
		}
		MatchParam[index].ValueChangeFlag = false;
		ini.WriteBool(section, "TrackerON", MatchParam[index].TrackerON);
		ini.WriteString(section, "TrackerName", MatchParam[index].TrackerName);
		ini.WriteBool(section, "Locked", MatchParam[index].Locked);
		ini.WriteBool(section, "Tracker1UseAutoSize", MatchParam[index].Tracker1UseAutoSize);
		ini.WriteInt(section, "Tracker1AligmentXIndex", MatchParam[index].Tracker1AligmentXIndex);
		ini.WriteInt(section, "Tracker1AligmentYIndex", MatchParam[index].Tracker1AligmentYIndex);
		ini.WriteInt(section, "Tracker1AligmentX", MatchParam[index].Tracker1AligmentX);
		ini.WriteInt(section, "Tracker1AligmentY", MatchParam[index].Tracker1AligmentY);
		ini.WriteInt(section, "AutoSize1_SelectIndex", MatchParam[index].AutoSize1_SelectIndex);
		ini.WriteInt(section, "Track1_T", MatchParam[index].Track1_T);
		ini.WriteInt(section, "Track1_B", MatchParam[index].Track1_B);
		ini.WriteInt(section, "Track1_L", MatchParam[index].Track1_L);
		ini.WriteInt(section, "Track1_R", MatchParam[index].Track1_R);
		ini.WriteInt(section, "PatternSelect", MatchParam[index].PatternSelect);
		ini.WriteInt(section, "PatternSelect2", MatchParam[index].PatternSelect2);
		ini.WriteInt(section, "PatternSelect3", MatchParam[index].PatternSelect3);
		ini.WriteInt(section, "PatternSelect4", MatchParam[index].PatternSelect4);
		ini.WriteInt(section, "MatchScoreMin", MatchParam[index].MatchScoreMin);
		ini.WriteInt(section, "MatchScoreMax", MatchParam[index].MatchScoreMax);
		ini.WriteInt(section, "ShapeMatchScoreMin", MatchParam[index].ShapeMatchScoreMin);
		ini.WriteBool(section, "EnableSelectFace", MatchParam[index].EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", MatchParam[index].FaceIndex);
		ini.WriteInt(section, "ParetoIndex", MatchParam[index].ParetoIndex);
		ini.WriteBool(section, "UseTextMatch", MatchParam[index].UseTextMatch);
		ini.WriteBool(section, "UseFoundToMask", MatchParam[index].UseFoundToMask);
		ini.WriteBool(section, "TextisWhite", MatchParam[index].TextisWhite);
		ini.WriteInt(section, "TextPosDiff", MatchParam[index].TextPosDiff);
		ini.WriteInt(section, "TextAreaDiff", MatchParam[index].TextAreaDiff);
		ini.WriteInt(section, "TextWidthDiff", MatchParam[index].TextWidthDiff);
		ini.WriteInt(section, "TextHeightDiff", MatchParam[index].TextHeightDiff);
		ini.WriteDouble(section, "MinColorMatchScore", MatchParam[index].MinColorMatchScore);
		ini.WriteInt(section, "MinMatchShapoeScore", MatchParam[index].MinMatchShapoeScore);
		ini.WriteBool(section, "EnableMatchShape", MatchParam[index].EnableMatchShape);
		ini.WriteBool(section, "ShapeTextisWhite", MatchParam[index].ShapeTextisWhite);
		ini.WriteBool(section, "EnableRotateInspect", MatchParam[index].EnableRotateInspect);
		ini.WriteInt(section, "FindCloseArea", MatchParam[index].FindCloseArea);
		ini.WriteInt(section, "FindTextArea", MatchParam[index].FindTextArea);
		ini.WriteDouble(section, "ClosedArea", MatchParam[index].ClosedArea);
		ini.WriteDouble(section, "MaxClosedArea", MatchParam[index].MaxClosedArea);
		ini.WriteDouble(section, "TextArea", MatchParam[index].TextArea);
		ini.WriteDouble(section, "MaxTextArea", MatchParam[index].MaxTextArea);
		ini.WriteDouble(section, "MinTextWidth", MatchParam[index].MinTextWidth);
		ini.WriteDouble(section, "MinTextHeight", MatchParam[index].MinTextHeight);
		ini.WriteDouble(section, "MaxTextWidth", MatchParam[index].MaxTextWidth);
		ini.WriteDouble(section, "MaxTextHeight", MatchParam[index].MaxTextHeight);
		ini.WriteInt(section, "DEStart", MatchParam[index].DEStart);
		ini.WriteInt(section, "DEStartCount", MatchParam[index].DEStartCount);
		ini.WriteBool(section, "UseOCR", MatchParam[index].UseOCR);
		ini.WriteString(section, "OCRText", MatchParam[index].OCRText);
		ini.WriteInt(section, "OCRImageChannel", MatchParam[index].OCRImageChannel);
		ini.WriteBool(section, "OCRTextisWhite", MatchParam[index].OCRTextisWhite);
		ini.WriteBool(section, "UsePublicDB", MatchParam[index].UsePublicDB);
		ini.WriteBool(section, "QuickSetupFinish", MatchParam[index].QuickSetupFinish);
		ini.WriteString(section, "QuickSetupName", MatchParam[index].QuickSetupName);
		ini.WriteInt(section, "QuickSetupIndex", MatchParam[index].QuickSetupIndex);
		ini.WriteBool(section, "MaskInFound", MatchParam[index].MaskInFound);
		ini.WriteBool(section, "EnableManualThd", MatchParam[index].EnableManualThd);
		ini.WriteInt(section, "TxtBinaryThdMin", MatchParam[index].TxtBinaryThdMin);
		ini.WriteInt(section, "TxtBinaryThdMax", MatchParam[index].TxtBinaryThdMax);
		ini.WriteBool(section, "EnableAISimilarityCheck", MatchParam[index].EnableAISimilarityCheck);
		ini.WriteInt(section, "AISimilarityScore", MatchParam[index].AISimilarityScore);
		ini.WriteString(section, "OnnxName", MatchParam[index].OnnxName);
		ini.WriteInt(section, "ErrorBin", (int)MatchParam[index].ErrorBin);
		ini.WriteBool(section, "AlignToMatchText", MatchParam[index].AlignToMatchText);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadPattern(int index)
	{
		string section = "PatternDB" + index;
		patternDB[index].ValueChangeFlag = false;
		patternDB[index].PatternON = ini.GetBoolean(section, "PatternON");
		patternDB[index].PatternName = ini.GetValue(section, "PatternName", "Unused");
		patternDB[index].PatternPath = ini.GetValue(section, "PatternPath");
		patternDB[index].PatternChannel = ini.GetInteger(section, "PatternChannel");
		string fileName = Path.GetFileName(patternDB[index].PatternPath);
		string text = Path.GetDirectoryName(ini.TheFile) + "\\Pattern\\" + fileName;
		if (patternDB[index].PatternON && text != "")
		{
			if (File.Exists(text))
			{
				if (patternDB[index].PatternPath != text)
				{
					patternDB[index].PatternPath = text;
					patternDB[index].ValueChangeFlag = true;
				}
				patternDB[index].PatternImage = CVBuffer.ImRead(text, ImreadModes.Grayscale);
				patternDB[index].PatternRect = new OpenCvSharp.Rect(0, 0, patternDB[index].PatternImage.Width, patternDB[index].PatternImage.Height);
				if (!patternDB[index].PatternImage.Empty())
				{
					patternDB[index].PatternON = true;
					if (!fileName.Contains("-") && fileName.Contains("."))
					{
						int startIndex = fileName.IndexOf(".");
						string value = string.Format("-{0}", patternDB[index].PatternChannel);
						string text2 = fileName.Insert(startIndex, value);
						patternDB[index].PatternPath = Path.GetDirectoryName(ini.TheFile) + "\\Pattern\\" + text2;
						Cv2.ImWrite(patternDB[index].PatternPath, patternDB[index].PatternImage);
						patternDB[index].ValueChangeFlag = true;
					}
				}
				else
				{
					patternDB[index].PatternON = false;
				}
			}
			else
			{
				patternDB[index].PatternON = false;
			}
		}
		else
		{
			patternDB[index].PatternON = false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WritePattern(int index)
	{
		if (!patternDB[index].ValueChangeFlag)
		{
			return true;
		}
		string section = "PatternDB" + index;
		ini.WriteBool(section, "PatternON", patternDB[index].PatternON);
		ini.WriteString(section, "PatternName", patternDB[index].PatternName);
		ini.WriteString(section, "PatternPath", patternDB[index].PatternPath);
		ini.WriteInt(section, "PatternChannel", patternDB[index].PatternChannel);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadFontToolParam()
	{
		FontToolParam.ValueChangeFlag = false;
		string section = "FontTool";
		FontToolParam.Locked = ini.GetBoolean(section, "Locked");
		FontToolParam.SearchRect1 = ini.ReadRect(section, "SearchRect1", 50);
		FontToolParam.Tracker1UseAutoSize = ini.GetBoolean(section, "Tracker1UseAutoSize", defaultValue: true);
		FontToolParam.Track1_T = ini.GetInteger(section, "Track1_T", 5);
		FontToolParam.Track1_B = ini.GetInteger(section, "Track1_B", 5);
		FontToolParam.Track1_L = ini.GetInteger(section, "Track1_L", 5);
		FontToolParam.Track1_R = ini.GetInteger(section, "Track1_R", 5);
		FontToolParam.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		FontToolParam.FaceIndex = ini.GetInteger(section, "FaceIndex");
		FontToolParam.ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		FontToolParam.EnableRotateInspect = ini.GetBoolean(section, "EnableRotateInspect");
		FontToolParam.PatternSelect = ini.GetInteger(section, "PatternSelect", -1);
		FontToolParam.MaskSelectIndex = ini.GetInteger(section, "MaskSelectIndex");
		FontToolParam.MinMatchScore = ini.GetInteger(section, "MinMatchScore", 60);
		FontToolParam.AllFont = ini.GetValue(section, "AllFont");
		FontToolParam.DoubleWidthFont1 = ini.GetBoolean(section, "DoubleWidthFont1");
		FontToolParam.DoubleWidthFont2 = ini.GetBoolean(section, "DoubleWidthFont2");
		FontToolParam.DoubleWidthFont3 = ini.GetBoolean(section, "DoubleWidthFont3");
		FontToolParam.DoubleWidthFont4 = ini.GetBoolean(section, "DoubleWidthFont4");
		FontToolParam.DoubleWidthFont5 = ini.GetBoolean(section, "DoubleWidthFont5");
		FontToolParam.DoubleWidthFont6 = ini.GetBoolean(section, "DoubleWidthFont6");
		FontToolParam.DoubleWidthFont7 = ini.GetBoolean(section, "DoubleWidthFont7");
		FontToolParam.DoubleWidthFont8 = ini.GetBoolean(section, "DoubleWidthFont8");
		FontToolParam.DoubleWidthFont9 = ini.GetBoolean(section, "DoubleWidthFont9");
		FontToolParam.FontChar1 = ini.GetValue(section, "FontChar1");
		FontToolParam.FontChar2 = ini.GetValue(section, "FontChar2");
		FontToolParam.FontChar3 = ini.GetValue(section, "FontChar3");
		FontToolParam.FontChar4 = ini.GetValue(section, "FontChar4");
		FontToolParam.FontChar5 = ini.GetValue(section, "FontChar5");
		FontToolParam.FontChar6 = ini.GetValue(section, "FontChar6");
		FontToolParam.FontChar7 = ini.GetValue(section, "FontChar7");
		FontToolParam.FontChar8 = ini.GetValue(section, "FontChar8");
		FontToolParam.FontChar9 = ini.GetValue(section, "FontChar9");
		FontToolParam.FontIndex1 = ini.GetInteger(section, "FontIndex1");
		FontToolParam.FontIndex2 = ini.GetInteger(section, "FontIndex2", 1);
		FontToolParam.FontIndex3 = ini.GetInteger(section, "FontIndex3", 2);
		FontToolParam.FontIndex4 = ini.GetInteger(section, "FontIndex4", 3);
		FontToolParam.FontIndex5 = ini.GetInteger(section, "FontIndex5", 4);
		FontToolParam.FontIndex6 = ini.GetInteger(section, "FontIndex6");
		FontToolParam.FontIndex7 = ini.GetInteger(section, "FontIndex7");
		FontToolParam.FontIndex8 = ini.GetInteger(section, "FontIndex8");
		FontToolParam.FontIndex9 = ini.GetInteger(section, "FontIndex9");
		FontToolParam.EnableAI_Font1 = ini.GetBoolean(section, "EnableAI_Font1");
		FontToolParam.EnableAI_Font2 = ini.GetBoolean(section, "EnableAI_Font2");
		FontToolParam.EnableAI_Font3 = ini.GetBoolean(section, "EnableAI_Font3");
		FontToolParam.EnableAI_Font4 = ini.GetBoolean(section, "EnableAI_Font4");
		FontToolParam.EnableAI_Font5 = ini.GetBoolean(section, "EnableAI_Font5");
		FontToolParam.EnableAI_Font6 = ini.GetBoolean(section, "EnableAI_Font6");
		FontToolParam.EnableAI_Font7 = ini.GetBoolean(section, "EnableAI_Font7");
		FontToolParam.EnableAI_Font8 = ini.GetBoolean(section, "EnableAI_Font8");
		FontToolParam.EnableAI_Font9 = ini.GetBoolean(section, "EnableAI_Font9");
		FontToolParam.EnableFontCheck1 = ini.GetBoolean(section, "EnableFontCheck1", defaultValue: true);
		FontToolParam.EnableFontCheck2 = ini.GetBoolean(section, "EnableFontCheck2", defaultValue: true);
		FontToolParam.EnableFontCheck3 = ini.GetBoolean(section, "EnableFontCheck3", defaultValue: true);
		FontToolParam.EnableFontCheck4 = ini.GetBoolean(section, "EnableFontCheck4", defaultValue: true);
		FontToolParam.EnableFontCheck5 = ini.GetBoolean(section, "EnableFontCheck5", defaultValue: true);
		FontToolParam.EnableFontCheck6 = ini.GetBoolean(section, "EnableFontCheck6", defaultValue: true);
		FontToolParam.EnableFontCheck7 = ini.GetBoolean(section, "EnableFontCheck7", defaultValue: true);
		FontToolParam.EnableFontCheck8 = ini.GetBoolean(section, "EnableFontCheck8", defaultValue: true);
		FontToolParam.EnableFontCheck9 = ini.GetBoolean(section, "EnableFontCheck9", defaultValue: true);
		FontToolParam.BypassOCR1 = ini.GetBoolean(section, "BypassOCR1");
		FontToolParam.BypassOCR2 = ini.GetBoolean(section, "BypassOCR2");
		FontToolParam.BypassOCR3 = ini.GetBoolean(section, "BypassOCR3");
		FontToolParam.BypassOCR4 = ini.GetBoolean(section, "BypassOCR4");
		FontToolParam.BypassOCR5 = ini.GetBoolean(section, "BypassOCR5");
		FontToolParam.BypassOCR6 = ini.GetBoolean(section, "BypassOCR6");
		FontToolParam.BypassOCR7 = ini.GetBoolean(section, "BypassOCR7");
		FontToolParam.BypassOCR8 = ini.GetBoolean(section, "BypassOCR8");
		FontToolParam.BypassOCR9 = ini.GetBoolean(section, "BypassOCR9");
		FontToolParam.AIModel_Font1 = ini.GetValue(section, "AIModel_Font1");
		FontToolParam.AIModel_Font2 = ini.GetValue(section, "AIModel_Font2");
		FontToolParam.AIModel_Font3 = ini.GetValue(section, "AIModel_Font3");
		FontToolParam.AIModel_Font4 = ini.GetValue(section, "AIModel_Font4");
		FontToolParam.AIModel_Font5 = ini.GetValue(section, "AIModel_Font5");
		FontToolParam.AIModel_Font6 = ini.GetValue(section, "AIModel_Font6");
		FontToolParam.AIModel_Font7 = ini.GetValue(section, "AIModel_Font7");
		FontToolParam.AIModel_Font8 = ini.GetValue(section, "AIModel_Font8");
		FontToolParam.AIModel_Font9 = ini.GetValue(section, "AIModel_Font9");
		FontToolParam.MinScore_Font1 = ini.GetInteger(section, "MinScore_Font1", 60);
		FontToolParam.MinScore_Font2 = ini.GetInteger(section, "MinScore_Font2", 60);
		FontToolParam.MinScore_Font3 = ini.GetInteger(section, "MinScore_Font3", 60);
		FontToolParam.MinScore_Font4 = ini.GetInteger(section, "MinScore_Font4", 60);
		FontToolParam.MinScore_Font5 = ini.GetInteger(section, "MinScore_Font5", 60);
		FontToolParam.MinScore_Font6 = ini.GetInteger(section, "MinScore_Font6", 60);
		FontToolParam.MinScore_Font7 = ini.GetInteger(section, "MinScore_Font7", 60);
		FontToolParam.MinScore_Font8 = ini.GetInteger(section, "MinScore_Font8", 60);
		FontToolParam.MinScore_Font9 = ini.GetInteger(section, "MinScore_Font9", 60);
		FontToolParam.WFontExtend1 = ini.GetInteger(section, "WFontExtend1", 10);
		FontToolParam.WFontExtend2 = ini.GetInteger(section, "WFontExtend2", 10);
		FontToolParam.WFontExtend3 = ini.GetInteger(section, "WFontExtend3", 10);
		FontToolParam.WFontExtend4 = ini.GetInteger(section, "WFontExtend4", 10);
		FontToolParam.WFontExtend5 = ini.GetInteger(section, "WFontExtend5", 10);
		FontToolParam.WFontExtend6 = ini.GetInteger(section, "WFontExtend6", 10);
		FontToolParam.WFontExtend7 = ini.GetInteger(section, "WFontExtend7", 10);
		FontToolParam.WFontExtend8 = ini.GetInteger(section, "WFontExtend8", 10);
		FontToolParam.WFontExtend9 = ini.GetInteger(section, "WFontExtend9", 10);
		FontToolParam.HFontExtend1 = ini.GetInteger(section, "HFontExtend1", 10);
		FontToolParam.HFontExtend2 = ini.GetInteger(section, "HFontExtend2", 10);
		FontToolParam.HFontExtend3 = ini.GetInteger(section, "HFontExtend3", 10);
		FontToolParam.HFontExtend4 = ini.GetInteger(section, "HFontExtend4", 10);
		FontToolParam.HFontExtend5 = ini.GetInteger(section, "HFontExtend5", 10);
		FontToolParam.HFontExtend6 = ini.GetInteger(section, "HFontExtend6", 10);
		FontToolParam.HFontExtend7 = ini.GetInteger(section, "HFontExtend7", 10);
		FontToolParam.HFontExtend8 = ini.GetInteger(section, "HFontExtend8", 10);
		FontToolParam.HFontExtend9 = ini.GetInteger(section, "HFontExtend9", 10);
		FontToolParam.doAutoCombineFontToDB = ini.GetBoolean(section, "doAutoCombineFontToDB");
		FontToolParam.AutoCombineFontToDBSet = ini.GetBoolean(section, "AutoCombineFontToDBSet");
		FontToolParam.AutoCombineFontCreate = false;
		FontToolParam.Font9Center1.X = ini.GetInteger(section, "Font9Center1X");
		FontToolParam.Font9Center1.Y = ini.GetInteger(section, "Font9Center1Y");
		FontToolParam.Font9Center2.X = ini.GetInteger(section, "Font9Center2X");
		FontToolParam.Font9Center2.Y = ini.GetInteger(section, "Font9Center2Y");
		FontToolParam.Font9Center3.X = ini.GetInteger(section, "Font9Center3X");
		FontToolParam.Font9Center3.Y = ini.GetInteger(section, "Font9Center3Y");
		FontToolParam.Font9Center4.X = ini.GetInteger(section, "Font9Center4X");
		FontToolParam.Font9Center4.Y = ini.GetInteger(section, "Font9Center4Y");
		FontToolParam.Font9Center5.X = ini.GetInteger(section, "Font9Center5X");
		FontToolParam.Font9Center5.Y = ini.GetInteger(section, "Font9Center5Y");
		FontToolParam.Font9Center6.X = ini.GetInteger(section, "Font9Center6X");
		FontToolParam.Font9Center6.Y = ini.GetInteger(section, "Font9Center6Y");
		FontToolParam.Font9Center7.X = ini.GetInteger(section, "Font9Center7X");
		FontToolParam.Font9Center7.Y = ini.GetInteger(section, "Font9Center7Y");
		FontToolParam.Font9Center8.X = ini.GetInteger(section, "Font9Center8X");
		FontToolParam.Font9Center8.Y = ini.GetInteger(section, "Font9Center8Y");
		FontToolParam.Font9Center9.X = ini.GetInteger(section, "Font9Center9X");
		FontToolParam.Font9Center9.Y = ini.GetInteger(section, "Font9Center9Y");
		FontToolParam.UseSameScore = ini.GetBoolean(section, "UseSameScore");
		FontToolParam.MinScore_ALL = ini.GetInteger(section, "MinScore_ALL", 90);
		FontToolParam.UsePublicDB = ini.GetBoolean(section, "UsePublicDB", defaultValue: true);
		FontToolParam.FontDBUsePublicMode = ini.GetBoolean(section, "FontDBUsePublicMode", defaultValue: true);
		FontToolParam.PublicDBUsePublicMode = ini.GetBoolean(section, "PublicDBUsePublicMode", defaultValue: true);
		FontToolParam.FontAlignVertical = ini.GetBoolean(section, "FontAlignVertical");
		FontToolParam.EnabledOCR = ini.GetBoolean(section, "EnabledOCR", defaultValue: true);
		FontToolParam.AutoLoadAI = ini.GetBoolean(section, "AutoLoadAI");
		FontToolParam.FontConnected = ini.GetBoolean(section, "FontConnected", defaultValue: true);
		FontToolParam.FontExtend = ini.GetInteger(section, "FontExtend", 5);
		FontToolParam.PatternChannel = ini.GetInteger(section, "PatternChannel", 1);
		FontToolParam.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		FontToolParam.R_LT_H = ini.GetBoolean(section, "R_LT_H", defaultValue: true);
		FontToolParam.R_LT_W = ini.GetBoolean(section, "R_LT_W", defaultValue: true);
		FontToolParam.R_RT_H = ini.GetBoolean(section, "R_RT_H", defaultValue: true);
		FontToolParam.R_RT_W = ini.GetBoolean(section, "R_RT_W", defaultValue: true);
		FontToolParam.R_LB_H = ini.GetBoolean(section, "R_LB_H", defaultValue: true);
		FontToolParam.R_RB_H = ini.GetBoolean(section, "R_RB_H");
		FontToolParam.R_Area = ini.GetBoolean(section, "R_Area", defaultValue: true);
		FontToolParam.R_T_W = ini.GetBoolean(section, "R_T_W", defaultValue: true);
		FontToolParam.R_B_W = ini.GetBoolean(section, "R_B_W", defaultValue: true);
		FontToolParam.R_L_H = ini.GetBoolean(section, "R_L_H", defaultValue: true);
		FontToolParam.R_R_H = ini.GetBoolean(section, "R_R_H", defaultValue: true);
		FontToolParam.R_C = ini.GetBoolean(section, "R_C", defaultValue: true);
		FontToolParam.R_C_H_MIN = ini.GetBoolean(section, "R_C_H_MIN", defaultValue: true);
		FontToolParam.R_LR_H_Diff = ini.GetBoolean(section, "R_LR_H_Diff", defaultValue: true);
		FontToolParam.R_Break = ini.GetBoolean(section, "R_Break", defaultValue: true);
		FontToolParam.R_LT_H_P = ini.GetDouble(section, "R_LT_H_P", 0.7);
		FontToolParam.R_LT_W_P = ini.GetDouble(section, "R_LT_W_P", 0.6);
		FontToolParam.R_RT_H_P = ini.GetDouble(section, "R_RT_H_P", 0.7);
		FontToolParam.R_RT_W_P = ini.GetDouble(section, "R_RT_W_P", 0.6);
		FontToolParam.R_LB_H_P = ini.GetDouble(section, "R_LB_H_P", 0.5);
		FontToolParam.R_Area_P = ini.GetDouble(section, "R_Area_P", 0.45);
		FontToolParam.R_T_W_P = ini.GetDouble(section, "R_T_W_P", 0.7);
		FontToolParam.R_B_W_P = ini.GetDouble(section, "R_B_W_P", 0.9);
		FontToolParam.R_L_H_P = ini.GetDouble(section, "R_L_H_P", 0.75);
		FontToolParam.R_R_H_P = ini.GetDouble(section, "R_R_H_P", 0.75);
		FontToolParam.R_LR_H_Diff_P = ini.GetDouble(section, "R_LR_H_Diff_P", 0.75);
		FontToolParam.R_C_P = ini.GetDouble(section, "R_C_P", 2.0);
		FontToolParam.R_C_H_MIN_P = ini.GetDouble(section, "R_C_H_MIN_P", 0.3);
		FontToolParam.R_C_H_MAX_P = ini.GetDouble(section, "R_C_H_MAX_P", 2.0);
		FontToolParam.R_Break_PMin = ini.GetDouble(section, "R_Break_PMin", 0.55);
		FontToolParam.R_Break_PMax = ini.GetDouble(section, "R_Break_PMax", 1.4);
		FontToolParam.R_RB_H_P = ini.GetDouble(section, "R_RB_H_P", 0.5);
		FontToolParam.R_BinaryShift = ini.GetInteger(section, "R_BinaryShift", -20);
		FontToolParam.R_OpenClose = ini.GetInteger(section, "R_OpenClose", 2);
		FontToolParam.R_OpenCloseCount = ini.GetInteger(section, "R_OpenCloseCount", 1);
		FontToolParam.R_OpenClose2 = ini.GetInteger(section, "R_OpenClose2");
		FontToolParam.R_OpenCloseCount2 = ini.GetInteger(section, "R_OpenCloseCount2", 1);
		FontToolParam.R_UseMorphFirst = ini.GetBoolean(section, "R_UseMorphFirst");
		FontToolParam.R_UseFontDBMask = ini.GetBoolean(section, "R_UseFontDBMask");
		FontToolParam.M_T_W = ini.GetBoolean(section, "M_T_W", defaultValue: true);
		FontToolParam.M_B_W = ini.GetBoolean(section, "M_B_W", defaultValue: true);
		FontToolParam.M_W = ini.GetBoolean(section, "M_W", defaultValue: true);
		FontToolParam.M_Break = ini.GetBoolean(section, "M_Break");
		FontToolParam.M_BinaryShift = ini.GetInteger(section, "M_BinaryShift", -20);
		FontToolParam.M_OpenClose = ini.GetInteger(section, "M_OpenClose", 2);
		FontToolParam.M_OpenCloseCount = ini.GetInteger(section, "M_OpenCloseCount", 2);
		FontToolParam.M_OpenClose2 = ini.GetInteger(section, "M_OpenClose2");
		FontToolParam.M_OpenCloseCount2 = ini.GetInteger(section, "M_OpenCloseCount2", 2);
		FontToolParam.M_UseMorphFirst = ini.GetBoolean(section, "M_UseMorphFirst");
		FontToolParam.M_UseFontDBMask = ini.GetBoolean(section, "M_UseFontDBMask");
		FontToolParam.M_Leg1 = ini.GetBoolean(section, "M_Leg1", defaultValue: true);
		FontToolParam.M_Leg2 = ini.GetBoolean(section, "M_Leg2", defaultValue: true);
		FontToolParam.M_Leg3 = ini.GetBoolean(section, "M_Leg3", defaultValue: true);
		FontToolParam.M_Connected = ini.GetBoolean(section, "M_Connected", defaultValue: true);
		FontToolParam.C_W = ini.GetBoolean(section, "C_W", defaultValue: true);
		FontToolParam.C_H = ini.GetBoolean(section, "C_H", defaultValue: true);
		FontToolParam.C_T_W = ini.GetBoolean(section, "C_T_W", defaultValue: true);
		FontToolParam.C_B_W = ini.GetBoolean(section, "C_B_W", defaultValue: true);
		FontToolParam.C_L_H = ini.GetBoolean(section, "C_L_H", defaultValue: true);
		FontToolParam.C_R_H = ini.GetBoolean(section, "C_R_H", defaultValue: true);
		FontToolParam.C_Break = ini.GetBoolean(section, "C_Break");
		FontToolParam.C_BinaryShift = ini.GetInteger(section, "C_BinaryShift", -20);
		FontToolParam.C_OpenClose = ini.GetInteger(section, "C_OpenClose", 2);
		FontToolParam.C_OpenCloseCount = ini.GetInteger(section, "C_OpenCloseCount", 1);
		FontToolParam.C_OpenClose2 = ini.GetInteger(section, "C_OpenClose2");
		FontToolParam.C_OpenCloseCount2 = ini.GetInteger(section, "C_OpenCloseCount2", 1);
		FontToolParam.C_UseMorphFirst = ini.GetBoolean(section, "C_UseMorphFirst");
		FontToolParam.C_UseFontDBMask = ini.GetBoolean(section, "C_UseFontDBMask");
		FontToolParam.U_W = ini.GetBoolean(section, "U_W", defaultValue: true);
		FontToolParam.U_H = ini.GetBoolean(section, "U_H", defaultValue: true);
		FontToolParam.U_T_W = ini.GetBoolean(section, "U_T_W", defaultValue: true);
		FontToolParam.U_B_W = ini.GetBoolean(section, "U_B_W", defaultValue: true);
		FontToolParam.U_L_H = ini.GetBoolean(section, "U_L_H", defaultValue: true);
		FontToolParam.U_R_H = ini.GetBoolean(section, "U_R_H", defaultValue: true);
		FontToolParam.U_Break = ini.GetBoolean(section, "U_Break");
		FontToolParam.U_BinaryShift = ini.GetInteger(section, "U_BinaryShift", -20);
		FontToolParam.U_OpenClose = ini.GetInteger(section, "U_OpenClose", 2);
		FontToolParam.U_OpenCloseCount = ini.GetInteger(section, "U_OpenCloseCount", 1);
		FontToolParam.U_OpenClose2 = ini.GetInteger(section, "U_OpenClose2");
		FontToolParam.U_OpenCloseCount2 = ini.GetInteger(section, "U_OpenCloseCount2", 1);
		FontToolParam.U_UseMorphFirst = ini.GetBoolean(section, "U_UseMorphFirst");
		FontToolParam.U_UseFontDBMask = ini.GetBoolean(section, "U_UseFontDBMask");
		FontToolParam.X_W = ini.GetBoolean(section, "X_W", defaultValue: true);
		FontToolParam.X_H = ini.GetBoolean(section, "X_H", defaultValue: true);
		FontToolParam.X_T_W = ini.GetBoolean(section, "X_T_W", defaultValue: true);
		FontToolParam.X_B_W = ini.GetBoolean(section, "X_B_W", defaultValue: true);
		FontToolParam.X_L_H = ini.GetBoolean(section, "X_L_H", defaultValue: true);
		FontToolParam.X_R_H = ini.GetBoolean(section, "X_R_H", defaultValue: true);
		FontToolParam.X_Break = ini.GetBoolean(section, "X_Break");
		FontToolParam.X_BinaryShift = ini.GetInteger(section, "X_BinaryShift", -20);
		FontToolParam.X_OpenClose = ini.GetInteger(section, "X_OpenClose", 2);
		FontToolParam.X_OpenCloseCount = ini.GetInteger(section, "X_OpenCloseCount", 1);
		FontToolParam.X_OpenClose2 = ini.GetInteger(section, "X_OpenClose2");
		FontToolParam.X_OpenCloseCount2 = ini.GetInteger(section, "X_OpenCloseCount2", 1);
		FontToolParam.X_UseMorphFirst = ini.GetBoolean(section, "X_UseMorphFirst");
		FontToolParam.X_UseFontDBMask = ini.GetBoolean(section, "X_UseFontDBMask");
		FontToolParam.TB_LT_H = ini.GetBoolean(section, "TB_LT_H", defaultValue: true);
		FontToolParam.TB_LT_W = ini.GetBoolean(section, "TB_LT_W", defaultValue: true);
		FontToolParam.TB_RT_H = ini.GetBoolean(section, "TB_RT_H", defaultValue: true);
		FontToolParam.TB_RT_W = ini.GetBoolean(section, "TB_RT_W", defaultValue: true);
		FontToolParam.TB_LB_H = ini.GetBoolean(section, "TB_LB_H", defaultValue: true);
		FontToolParam.TB_LB_W = ini.GetBoolean(section, "TB_LB_W", defaultValue: true);
		FontToolParam.TB_RB_H = ini.GetBoolean(section, "TB_RB_H", defaultValue: true);
		FontToolParam.TB_RB_W = ini.GetBoolean(section, "TB_RB_W", defaultValue: true);
		FontToolParam.TB_B_W = ini.GetBoolean(section, "TB_B_W", defaultValue: true);
		FontToolParam.TB_C = ini.GetBoolean(section, "TB_C", defaultValue: true);
		FontToolParam.TB_C_H_MIN = ini.GetBoolean(section, "TB_C_H_MIN", defaultValue: true);
		FontToolParam.TB_C_H_MAX = ini.GetBoolean(section, "TB_C_H_MAX", defaultValue: true);
		FontToolParam.TB_Break = ini.GetBoolean(section, "TB_Break");
		FontToolParam.TB_BinaryShift = ini.GetInteger(section, "TB_BinaryShift", -20);
		FontToolParam.TB_OpenClose = ini.GetInteger(section, "TB_OpenClose", 2);
		FontToolParam.TB_OpenCloseCount = ini.GetInteger(section, "TB_OpenCloseCount", 2);
		FontToolParam.TB_UseFontDBMask = ini.GetBoolean(section, "TB_UseFontDBMask");
		FontToolParam.TE_LT_H = ini.GetBoolean(section, "TE_LT_H", defaultValue: true);
		FontToolParam.TE_LT_W = ini.GetBoolean(section, "TE_LT_W", defaultValue: true);
		FontToolParam.TE_RT_H = ini.GetBoolean(section, "TE_RT_H", defaultValue: true);
		FontToolParam.TE_RT_W = ini.GetBoolean(section, "TE_RT_W", defaultValue: true);
		FontToolParam.TE_LB_H = ini.GetBoolean(section, "TE_LB_H", defaultValue: true);
		FontToolParam.TE_LB_W = ini.GetBoolean(section, "TE_LB_W", defaultValue: true);
		FontToolParam.TE_RB_H = ini.GetBoolean(section, "TE_RB_H", defaultValue: true);
		FontToolParam.TE_RB_W = ini.GetBoolean(section, "TE_RB_W", defaultValue: true);
		FontToolParam.TE_Break = ini.GetBoolean(section, "TE_Break", defaultValue: true);
		FontToolParam.TE_BinaryShift = ini.GetInteger(section, "TE_BinaryShift", -10);
		FontToolParam.TE_OpenClose = ini.GetInteger(section, "TE_OpenClose", 2);
		FontToolParam.TE_OpenCloseCount = ini.GetInteger(section, "TE_OpenCloseCount", 1);
		FontToolParam.TE_OpenClose2 = ini.GetInteger(section, "TE_OpenClose2");
		FontToolParam.TE_OpenCloseCount2 = ini.GetInteger(section, "TE_OpenCloseCount2", 1);
		FontToolParam.TE_UseMorphFirst = ini.GetBoolean(section, "TE_UseMorphFirst");
		FontToolParam.TE_UseFontDBMask = ini.GetBoolean(section, "TE_UseFontDBMask");
		FontToolParam.Dash_H = ini.GetBoolean(section, "Dash_H", defaultValue: true);
		FontToolParam.Dash_W = ini.GetBoolean(section, "Dash_W", defaultValue: true);
		FontToolParam.Dash_Break = ini.GetBoolean(section, "Dash_Break");
		FontToolParam.Dash_BinaryShift = ini.GetInteger(section, "Dash_BinaryShift", -20);
		FontToolParam.Dash_OpenClose = ini.GetInteger(section, "Dash_OpenClose", 2);
		FontToolParam.Dash_OpenCloseCount = ini.GetInteger(section, "Dash_OpenCloseCount", 2);
		FontToolParam.Dash_OpenClose2 = ini.GetInteger(section, "Dash_OpenClose2");
		FontToolParam.Dash_OpenCloseCount2 = ini.GetInteger(section, "Dash_OpenCloseCount2", 1);
		FontToolParam.Dash_UseMorphFirst = ini.GetBoolean(section, "Dash_UseMorphFirst");
		FontToolParam.Dash_UseFontDBMask = ini.GetBoolean(section, "Dash_UseFontDBMask");
		FontToolParam.D_LT_H = ini.GetBoolean(section, "D_LT_H", defaultValue: true);
		FontToolParam.D_LT_W = ini.GetBoolean(section, "D_LT_W", defaultValue: true);
		FontToolParam.D_RT_H = ini.GetBoolean(section, "D_RT_H", defaultValue: true);
		FontToolParam.D_RT_W = ini.GetBoolean(section, "D_RT_W", defaultValue: true);
		FontToolParam.D_LB_H = ini.GetBoolean(section, "D_LB_H", defaultValue: true);
		FontToolParam.D_LB_W = ini.GetBoolean(section, "D_LB_W", defaultValue: true);
		FontToolParam.D_RB_H = ini.GetBoolean(section, "D_RB_H", defaultValue: true);
		FontToolParam.D_RB_W = ini.GetBoolean(section, "D_RB_W", defaultValue: true);
		FontToolParam.D_T_W = ini.GetBoolean(section, "D_T_W", defaultValue: true);
		FontToolParam.D_B_W = ini.GetBoolean(section, "D_B_W", defaultValue: true);
		FontToolParam.D_C = ini.GetBoolean(section, "D_C", defaultValue: true);
		FontToolParam.D_C_H_MIN = ini.GetBoolean(section, "D_C_H_MIN", defaultValue: true);
		FontToolParam.D_C_H_MAX = ini.GetBoolean(section, "D_C_H_MAX", defaultValue: true);
		FontToolParam.D_Break = ini.GetBoolean(section, "D_Break");
		FontToolParam.D_BinaryShift = ini.GetInteger(section, "D_BinaryShift", -20);
		FontToolParam.D_OpenClose = ini.GetInteger(section, "D_OpenClose", 2);
		FontToolParam.D_OpenCloseCount = ini.GetInteger(section, "D_OpenCloseCount", 1);
		FontToolParam.D_OpenClose2 = ini.GetInteger(section, "D_OpenClose2");
		FontToolParam.D_OpenCloseCount2 = ini.GetInteger(section, "D_OpenCloseCount2", 1);
		FontToolParam.D_UseFontDBMask = ini.GetBoolean(section, "D_UseFontDBMask");
		FontToolParam.A_W = ini.GetBoolean(section, "A_W", defaultValue: true);
		FontToolParam.A_H = ini.GetBoolean(section, "A_H", defaultValue: true);
		FontToolParam.A_T_W = ini.GetBoolean(section, "A_T_W", defaultValue: true);
		FontToolParam.A_B_W = ini.GetBoolean(section, "A_B_W", defaultValue: true);
		FontToolParam.A_L_H = ini.GetBoolean(section, "A_L_H", defaultValue: true);
		FontToolParam.A_R_H = ini.GetBoolean(section, "A_R_H", defaultValue: true);
		FontToolParam.A_Break = ini.GetBoolean(section, "A_Break");
		FontToolParam.A_BinaryShift = ini.GetInteger(section, "A_BinaryShift", -20);
		FontToolParam.A_OpenClose = ini.GetInteger(section, "A_OpenClose", 2);
		FontToolParam.A_OpenCloseCount = ini.GetInteger(section, "A_OpenCloseCount", 1);
		FontToolParam.A_OpenClose2 = ini.GetInteger(section, "A_OpenClose2");
		FontToolParam.A_OpenCloseCount2 = ini.GetInteger(section, "A_OpenCloseCount2", 1);
		FontToolParam.A_UseMorphFirst = ini.GetBoolean(section, "A_UseMorphFirst");
		FontToolParam.A_UseFontDBMask = ini.GetBoolean(section, "A_UseFontDBMask");
		FontToolParam.T0_LT_H = ini.GetBoolean(section, "T0_LT_H", defaultValue: true);
		FontToolParam.T0_LT_W = ini.GetBoolean(section, "T0_LT_W", defaultValue: true);
		FontToolParam.T0_RT_H = ini.GetBoolean(section, "T0_RT_H", defaultValue: true);
		FontToolParam.T0_RT_W = ini.GetBoolean(section, "T0_RT_W", defaultValue: true);
		FontToolParam.T0_LB_H = ini.GetBoolean(section, "T0_LB_H", defaultValue: true);
		FontToolParam.T0_LB_W = ini.GetBoolean(section, "T0_LB_W", defaultValue: true);
		FontToolParam.T0_RB_H = ini.GetBoolean(section, "T0_RB_H", defaultValue: true);
		FontToolParam.T0_RB_W = ini.GetBoolean(section, "T0_RB_W", defaultValue: true);
		FontToolParam.T0_T_W = ini.GetBoolean(section, "T0_T_W", defaultValue: true);
		FontToolParam.T0_B_W = ini.GetBoolean(section, "T0_B_W", defaultValue: true);
		FontToolParam.T0_C = ini.GetBoolean(section, "T0_C", defaultValue: true);
		FontToolParam.T0_C_H_MIN = ini.GetBoolean(section, "T0_C_H_MIN", defaultValue: true);
		FontToolParam.T0_C_H_MAX = ini.GetBoolean(section, "T0_C_H_MAX", defaultValue: true);
		FontToolParam.T0_Break = ini.GetBoolean(section, "T0_Break");
		FontToolParam.T0_L_H = ini.GetBoolean(section, "T0_L_H");
		FontToolParam.T0_R_H = ini.GetBoolean(section, "T0_R_H");
		FontToolParam.T0_BinaryShift = ini.GetInteger(section, "T0_BinaryShift", -20);
		FontToolParam.T0_OpenClose = ini.GetInteger(section, "T0_OpenClose", 2);
		FontToolParam.T0_OpenCloseCount = ini.GetInteger(section, "T0_OpenCloseCount", 1);
		FontToolParam.T0_OpenClose2 = ini.GetInteger(section, "T0_OpenClose2");
		FontToolParam.T0_OpenCloseCount2 = ini.GetInteger(section, "T0_OpenCloseCount2", 1);
		FontToolParam.T0_UseMorphFirst = ini.GetBoolean(section, "T0_UseMorphFirst");
		FontToolParam.T0_UseFontDBMask = ini.GetBoolean(section, "T0_UseFontDBMask");
		FontToolParam.T1_H = ini.GetBoolean(section, "T1_H", defaultValue: true);
		FontToolParam.T1_W = ini.GetBoolean(section, "T1_W", defaultValue: true);
		FontToolParam.T1_Break = ini.GetBoolean(section, "T1_Break", defaultValue: true);
		FontToolParam.T1_BinaryShift = ini.GetInteger(section, "T1_BinaryShift", -10);
		FontToolParam.T1_OpenClose = ini.GetInteger(section, "T1_OpenClose", 2);
		FontToolParam.T1_OpenCloseCount = ini.GetInteger(section, "T1_OpenCloseCount", 1);
		FontToolParam.T1_OpenClose2 = ini.GetInteger(section, "T1_OpenClose2");
		FontToolParam.T1_OpenCloseCount2 = ini.GetInteger(section, "T1_OpenCloseCount2", 1);
		FontToolParam.T1_UseMorphFirst = ini.GetBoolean(section, "T1_UseMorphFirst");
		FontToolParam.T1_UseFontDBMask = ini.GetBoolean(section, "T1_UseFontDBMask");
		FontToolParam.T2_LT_H = ini.GetBoolean(section, "T2_LT_H", defaultValue: true);
		FontToolParam.T2_LT_W = ini.GetBoolean(section, "T2_LT_W", defaultValue: true);
		FontToolParam.T2_RT_H = ini.GetBoolean(section, "T2_RT_H", defaultValue: true);
		FontToolParam.T2_RT_W = ini.GetBoolean(section, "T2_RT_W", defaultValue: true);
		FontToolParam.T2_LB_H = ini.GetBoolean(section, "T2_LB_H", defaultValue: true);
		FontToolParam.T2_LB_W = ini.GetBoolean(section, "T2_LB_W", defaultValue: true);
		FontToolParam.T2_RB_H = ini.GetBoolean(section, "T2_RB_H", defaultValue: true);
		FontToolParam.T2_RB_W = ini.GetBoolean(section, "T2_RB_W", defaultValue: true);
		FontToolParam.T2_Break = ini.GetBoolean(section, "T2_Break", defaultValue: true);
		FontToolParam.T2_BinaryShift = ini.GetInteger(section, "T2_BinaryShift", -10);
		FontToolParam.T2_OpenClose = ini.GetInteger(section, "T2_OpenClose", 2);
		FontToolParam.T2_OpenCloseCount = ini.GetInteger(section, "T2_OpenCloseCount", 1);
		FontToolParam.T2_OpenClose2 = ini.GetInteger(section, "T2_OpenClose2");
		FontToolParam.T2_OpenCloseCount2 = ini.GetInteger(section, "T2_OpenCloseCount2", 1);
		FontToolParam.T2_UseMorphFirst = ini.GetBoolean(section, "T2_UseMorphFirst");
		FontToolParam.T2_UseFontDBMask = ini.GetBoolean(section, "T2_UseFontDBMask");
		FontToolParam.T3_LT_H = ini.GetBoolean(section, "T3_LT_H", defaultValue: true);
		FontToolParam.T3_LT_W = ini.GetBoolean(section, "T3_LT_W", defaultValue: true);
		FontToolParam.T3_RT_H = ini.GetBoolean(section, "T3_RT_H", defaultValue: true);
		FontToolParam.T3_RT_W = ini.GetBoolean(section, "T3_RT_W", defaultValue: true);
		FontToolParam.T3_LB_H = ini.GetBoolean(section, "T3_LB_H", defaultValue: true);
		FontToolParam.T3_LB_W = ini.GetBoolean(section, "T3_LB_W", defaultValue: true);
		FontToolParam.T3_RB_H = ini.GetBoolean(section, "T3_RB_H", defaultValue: true);
		FontToolParam.T3_RB_W = ini.GetBoolean(section, "T3_RB_W", defaultValue: true);
		FontToolParam.T3_Break = ini.GetBoolean(section, "T3_Break", defaultValue: true);
		FontToolParam.T3_T_W = ini.GetBoolean(section, "T3_T_W");
		FontToolParam.T3_B_W = ini.GetBoolean(section, "T3_B_W");
		FontToolParam.T3_C_W = ini.GetBoolean(section, "T3_C_W");
		FontToolParam.T3_BinaryShift = ini.GetInteger(section, "T3_BinaryShift", -10);
		FontToolParam.T3_OpenClose = ini.GetInteger(section, "T3_OpenClose", 2);
		FontToolParam.T3_OpenCloseCount = ini.GetInteger(section, "T3_OpenCloseCount", 1);
		FontToolParam.T3_OpenClose2 = ini.GetInteger(section, "T3_OpenClose2");
		FontToolParam.T3_OpenCloseCount2 = ini.GetInteger(section, "T3_OpenCloseCount2", 1);
		FontToolParam.T3_UseMorphFirst = ini.GetBoolean(section, "T3_UseMorphFirst");
		FontToolParam.T3_UseFontDBMask = ini.GetBoolean(section, "T3_UseFontDBMask");
		FontToolParam.T3_LT_H = ini.GetBoolean(section, "T4_LT_H", defaultValue: true);
		FontToolParam.T4_LT_W = ini.GetBoolean(section, "T4_LT_W", defaultValue: true);
		FontToolParam.T4_RT_H = ini.GetBoolean(section, "T4_RT_H", defaultValue: true);
		FontToolParam.T4_RT_W = ini.GetBoolean(section, "T4_RT_W", defaultValue: true);
		FontToolParam.T4_T_W = ini.GetBoolean(section, "T4_T_W", defaultValue: true);
		FontToolParam.T4_B_W = ini.GetBoolean(section, "T4_B_W", defaultValue: true);
		FontToolParam.T4_Break = ini.GetBoolean(section, "T4_Break", defaultValue: true);
		FontToolParam.T4_W = ini.GetBoolean(section, "T4_W", defaultValue: true);
		FontToolParam.T4_H = ini.GetBoolean(section, "T4_H", defaultValue: true);
		FontToolParam.T4_BinaryShift = ini.GetInteger(section, "T4_BinaryShift", -10);
		FontToolParam.T4_OpenClose = ini.GetInteger(section, "T4_OpenClose", 2);
		FontToolParam.T4_OpenCloseCount = ini.GetInteger(section, "T4_OpenCloseCount", 1);
		FontToolParam.T4_OpenClose2 = ini.GetInteger(section, "T4_OpenClose2");
		FontToolParam.T4_OpenCloseCount2 = ini.GetInteger(section, "T4_OpenCloseCount2", 1);
		FontToolParam.T4_UseMorphFirst = ini.GetBoolean(section, "T4_UseMorphFirst");
		FontToolParam.T4_UseFontDBMask = ini.GetBoolean(section, "T4_UseFontDBMask");
		FontToolParam.T5_LT_H = ini.GetBoolean(section, "T5_LT_H", defaultValue: true);
		FontToolParam.T5_LT_W = ini.GetBoolean(section, "T5_LT_W", defaultValue: true);
		FontToolParam.T5_RT_H = ini.GetBoolean(section, "T5_RT_H", defaultValue: true);
		FontToolParam.T5_RT_W = ini.GetBoolean(section, "T5_RT_W", defaultValue: true);
		FontToolParam.T5_LB_H = ini.GetBoolean(section, "T5_LB_H", defaultValue: true);
		FontToolParam.T5_LB_W = ini.GetBoolean(section, "T5_LB_W", defaultValue: true);
		FontToolParam.T5_RB_H = ini.GetBoolean(section, "T5_RB_H", defaultValue: true);
		FontToolParam.T5_RB_W = ini.GetBoolean(section, "T5_RB_W", defaultValue: true);
		FontToolParam.T5_Break = ini.GetBoolean(section, "T5_Break", defaultValue: true);
		FontToolParam.T5_TT_W = ini.GetBoolean(section, "T5_TT_W", defaultValue: true);
		FontToolParam.T5_TB_W = ini.GetBoolean(section, "T5_TB_W", defaultValue: true);
		FontToolParam.T5_BinaryShift = ini.GetInteger(section, "T5_BinaryShift", -20);
		FontToolParam.T5_OpenClose = ini.GetInteger(section, "T5_OpenClose", 2);
		FontToolParam.T5_OpenCloseCount = ini.GetInteger(section, "T5_OpenCloseCount", 1);
		FontToolParam.T5_OpenClose2 = ini.GetInteger(section, "T5_OpenClose2");
		FontToolParam.T5_OpenCloseCount2 = ini.GetInteger(section, "T5_OpenCloseCount2", 1);
		FontToolParam.T5_UseMorphFirst = ini.GetBoolean(section, "T5_UseMorphFirst");
		FontToolParam.T5_UseFontDBMask = ini.GetBoolean(section, "T5_UseFontDBMask");
		FontToolParam.T6_LT_H = ini.GetBoolean(section, "T6_LT_H", defaultValue: true);
		FontToolParam.T6_LT_W = ini.GetBoolean(section, "T6_LT_W", defaultValue: true);
		FontToolParam.T6_RT_H = ini.GetBoolean(section, "T6_RT_H", defaultValue: true);
		FontToolParam.T6_RT_W = ini.GetBoolean(section, "T6_RT_W", defaultValue: true);
		FontToolParam.T6_T_W = ini.GetBoolean(section, "T6_T_W", defaultValue: true);
		FontToolParam.T6_B_W = ini.GetBoolean(section, "T6_B_W", defaultValue: true);
		FontToolParam.T6_Break = ini.GetBoolean(section, "T6_Break", defaultValue: true);
		FontToolParam.T6_C = ini.GetBoolean(section, "T6_C", defaultValue: true);
		FontToolParam.T6_BinaryShift = ini.GetInteger(section, "T6_BinaryShift", -10);
		FontToolParam.T6_OpenClose = ini.GetInteger(section, "T6_OpenClose", 2);
		FontToolParam.T6_OpenCloseCount = ini.GetInteger(section, "T6_OpenCloseCount", 1);
		FontToolParam.T6_OpenClose2 = ini.GetInteger(section, "T6_OpenClose2");
		FontToolParam.T6_OpenCloseCount2 = ini.GetInteger(section, "T6_OpenCloseCount2", 1);
		FontToolParam.T6_UseMorphFirst = ini.GetBoolean(section, "T6_UseMorphFirst");
		FontToolParam.T6_UseFontDBMask = ini.GetBoolean(section, "T6_UseFontDBMask");
		FontToolParam.T7_T_W = ini.GetBoolean(section, "T7_T_W", defaultValue: true);
		FontToolParam.T7_B_W = ini.GetBoolean(section, "T7_B_W", defaultValue: true);
		FontToolParam.T7_W = ini.GetBoolean(section, "T7_W", defaultValue: true);
		FontToolParam.T7_H = ini.GetBoolean(section, "T7_H", defaultValue: true);
		FontToolParam.T7_Break = ini.GetBoolean(section, "T7_Break", defaultValue: true);
		FontToolParam.T7_BinaryShift = ini.GetInteger(section, "T7_BinaryShift", -10);
		FontToolParam.T7_OpenClose = ini.GetInteger(section, "T7_OpenClose", 2);
		FontToolParam.T7_OpenCloseCount = ini.GetInteger(section, "T7_OpenCloseCount", 1);
		FontToolParam.T7_OpenClose2 = ini.GetInteger(section, "T7_OpenClose2");
		FontToolParam.T7_OpenCloseCount2 = ini.GetInteger(section, "T7_OpenCloseCount2", 1);
		FontToolParam.T7_UseMorphFirst = ini.GetBoolean(section, "T7_UseMorphFirst");
		FontToolParam.T7_UseFontDBMask = ini.GetBoolean(section, "T7_UseFontDBMask");
		FontToolParam.T8_LT_H = ini.GetBoolean(section, "T8_LT_H", defaultValue: true);
		FontToolParam.T8_LT_W = ini.GetBoolean(section, "T8_LT_W", defaultValue: true);
		FontToolParam.T8_RT_H = ini.GetBoolean(section, "T8_RT_H", defaultValue: true);
		FontToolParam.T8_RT_W = ini.GetBoolean(section, "T8_RT_W", defaultValue: true);
		FontToolParam.T8_LB_H = ini.GetBoolean(section, "T8_LB_H", defaultValue: true);
		FontToolParam.T8_LB_W = ini.GetBoolean(section, "T8_LB_W", defaultValue: true);
		FontToolParam.T8_RB_H = ini.GetBoolean(section, "T8_RB_H", defaultValue: true);
		FontToolParam.T8_RB_W = ini.GetBoolean(section, "T8_RB_W", defaultValue: true);
		FontToolParam.T8_B_W = ini.GetBoolean(section, "T8_B_W", defaultValue: true);
		FontToolParam.T8_C = ini.GetBoolean(section, "T8_C", defaultValue: true);
		FontToolParam.T8_C_H_MIN = ini.GetBoolean(section, "T8_C_H_MIN", defaultValue: true);
		FontToolParam.T8_C_H_MAX = ini.GetBoolean(section, "T8_C_H_MAX", defaultValue: true);
		FontToolParam.T8_Break = ini.GetBoolean(section, "T8_Break");
		FontToolParam.T8_L_H = ini.GetBoolean(section, "T8_L_H");
		FontToolParam.T8_R_H = ini.GetBoolean(section, "T8_R_H");
		FontToolParam.T8_BinaryShift = ini.GetInteger(section, "T8_BinaryShift", -20);
		FontToolParam.T8_OpenClose = ini.GetInteger(section, "T8_OpenClose", 2);
		FontToolParam.T8_OpenCloseCount = ini.GetInteger(section, "T8_OpenCloseCount", 2);
		FontToolParam.T8_OpenClose2 = ini.GetInteger(section, "T8_OpenClose2");
		FontToolParam.T8_OpenCloseCount2 = ini.GetInteger(section, "T8_OpenCloseCount2", 1);
		FontToolParam.T8_UseMorphFirst = ini.GetBoolean(section, "T8_UseMorphFirst");
		FontToolParam.T8_UseFontDBMask = ini.GetBoolean(section, "T8_UseFontDBMask");
		FontToolParam.T9_LT_H = ini.GetBoolean(section, "T9_LT_H", defaultValue: true);
		FontToolParam.T9_LT_W = ini.GetBoolean(section, "T9_LT_W", defaultValue: true);
		FontToolParam.T9_RT_H = ini.GetBoolean(section, "T9_RT_H", defaultValue: true);
		FontToolParam.T9_RT_W = ini.GetBoolean(section, "T9_RT_W", defaultValue: true);
		FontToolParam.T9_B_H = ini.GetBoolean(section, "T9_B_H", defaultValue: true);
		FontToolParam.T9_B_W = ini.GetBoolean(section, "T9_B_W", defaultValue: true);
		FontToolParam.T9_Break = ini.GetBoolean(section, "T9_Break", defaultValue: true);
		FontToolParam.T9_C = ini.GetBoolean(section, "T9_C", defaultValue: true);
		FontToolParam.T9_BinaryShift = ini.GetInteger(section, "T9_BinaryShift", -10);
		FontToolParam.T9_OpenClose = ini.GetInteger(section, "T9_OpenClose", 2);
		FontToolParam.T9_OpenCloseCount = ini.GetInteger(section, "T9_OpenCloseCount", 1);
		FontToolParam.T9_OpenClose2 = ini.GetInteger(section, "T9_OpenClose2");
		FontToolParam.T9_OpenCloseCount2 = ini.GetInteger(section, "T9_OpenCloseCount2", 1);
		FontToolParam.T9_UseMorphFirst = ini.GetBoolean(section, "T9_UseMorphFirst");
		FontToolParam.T9_UseFontDBMask = ini.GetBoolean(section, "T9_UseFontDBMask");
		FontToolParam.ConnectBinaryShift = ini.GetInteger(section, "ConnectBinaryShift", -5);
		FontToolParam.ConnectOpenClose = ini.GetInteger(section, "ConnectOpenClose", 1);
		FontToolParam.ConnectOpenCloseCount = ini.GetInteger(section, "ConnectOpenCloseCount", 1);
		FontToolParam.ConnectOpenClose2 = ini.GetInteger(section, "ConnectOpenClose2");
		FontToolParam.ConnectOpenCloseCount2 = ini.GetInteger(section, "ConnectOpenCloseCount2", 1);
		FontToolParam.ConnectCountAlarm = ini.GetBoolean(section, "ConnectCountAlarm", defaultValue: true);
		FontToolParam.ConnectWidthAlarm = ini.GetBoolean(section, "ConnectWidthAlarm", defaultValue: true);
		FontToolParam.UseFastMatch = ini.GetBoolean(section, "UseFastMatch");
		FontToolParam.AutoSetFontImage = ini.GetBoolean(section, "AutoSetFontImage");
		for (int i = 0; i < 20; i++)
		{
			FontToolParam.OtherFont[i].T_LT_H = ini.GetBoolean(section, "FT" + i + "_LT_H", defaultValue: true);
			FontToolParam.OtherFont[i].T_LT_W = ini.GetBoolean(section, "FT" + i + "_LT_W", defaultValue: true);
			FontToolParam.OtherFont[i].T_RT_H = ini.GetBoolean(section, "FT" + i + "_RT_H", defaultValue: true);
			FontToolParam.OtherFont[i].T_RT_W = ini.GetBoolean(section, "FT" + i + "_RT_W", defaultValue: true);
			FontToolParam.OtherFont[i].T_LB_H = ini.GetBoolean(section, "FT" + i + "_LB_H", defaultValue: true);
			FontToolParam.OtherFont[i].T_LB_W = ini.GetBoolean(section, "FT" + i + "_LB_W", defaultValue: true);
			FontToolParam.OtherFont[i].T_RB_H = ini.GetBoolean(section, "FT" + i + "_RB_H", defaultValue: true);
			FontToolParam.OtherFont[i].T_RB_W = ini.GetBoolean(section, "FT" + i + "_RB_W", defaultValue: true);
			FontToolParam.OtherFont[i].T_Break = ini.GetBoolean(section, "FT" + i + "_Break");
			FontToolParam.OtherFont[i].T_T_W = ini.GetBoolean(section, "FT" + i + "_T_W", defaultValue: true);
			FontToolParam.OtherFont[i].T_B_W = ini.GetBoolean(section, "FT" + i + "_B_W", defaultValue: true);
			FontToolParam.OtherFont[i].T_C_W = ini.GetBoolean(section, "FT" + i + "_C_W");
			FontToolParam.OtherFont[i].T_BinaryShift = ini.GetInteger(section, "FT" + i + "_BinaryShift", -10);
			FontToolParam.OtherFont[i].T_OpenClose = ini.GetInteger(section, "FT" + i + "_OpenClose", 2);
			FontToolParam.OtherFont[i].T_OpenCloseCount = ini.GetInteger(section, "FT" + i + "_OpenCloseCount", 1);
			FontToolParam.OtherFont[i].T_OpenClose2 = ini.GetInteger(section, "FT" + i + "_OpenClose2");
			FontToolParam.OtherFont[i].T_OpenCloseCount2 = ini.GetInteger(section, "FT" + i + "_OpenCloseCount2", 1);
			FontToolParam.OtherFont[i].T_UseMorphFirst = ini.GetBoolean(section, "FT" + i + "_UseMorphFirst");
			FontToolParam.OtherFont[i].T_UseFontDBMask = ini.GetBoolean(section, "FT" + i + "_UseFontDBMask");
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteFontToolParam()
	{
		string section = "FontTool";
		if (!FontToolParam.ValueChangeFlag)
		{
			return true;
		}
		FontToolParam.ValueChangeFlag = false;
		ini.WriteBool(section, "Locked", FontToolParam.Locked);
		ini.WriteBool(section, "Tracker1UseAutoSize", FontToolParam.Tracker1UseAutoSize);
		ini.WriteInt(section, "Track1_T", FontToolParam.Track1_T);
		ini.WriteInt(section, "Track1_B", FontToolParam.Track1_B);
		ini.WriteInt(section, "Track1_L", FontToolParam.Track1_L);
		ini.WriteInt(section, "Track1_R", FontToolParam.Track1_R);
		ini.WriteBool(section, "EnableSelectFace", FontToolParam.EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", FontToolParam.FaceIndex);
		ini.WriteInt(section, "ParetoIndex", FontToolParam.ParetoIndex);
		ini.WriteBool(section, "EnableRotateInspect", FontToolParam.EnableRotateInspect);
		ini.WriteInt(section, "PatternSelect", FontToolParam.PatternSelect);
		ini.WriteInt(section, "MaskSelectIndex", FontToolParam.MaskSelectIndex);
		ini.WriteInt(section, "MinMatchScore", FontToolParam.MinMatchScore);
		ini.WriteInt(section, "FontIndex1", FontToolParam.FontIndex1);
		ini.WriteInt(section, "FontIndex2", FontToolParam.FontIndex2);
		ini.WriteInt(section, "FontIndex3", FontToolParam.FontIndex3);
		ini.WriteInt(section, "FontIndex4", FontToolParam.FontIndex4);
		ini.WriteInt(section, "FontIndex5", FontToolParam.FontIndex5);
		ini.WriteInt(section, "FontIndex6", FontToolParam.FontIndex6);
		ini.WriteInt(section, "FontIndex7", FontToolParam.FontIndex7);
		ini.WriteInt(section, "FontIndex8", FontToolParam.FontIndex8);
		ini.WriteInt(section, "FontIndex9", FontToolParam.FontIndex9);
		ini.WriteBool(section, "EnableFontCheck1", FontToolParam.EnableFontCheck1);
		ini.WriteBool(section, "EnableFontCheck2", FontToolParam.EnableFontCheck2);
		ini.WriteBool(section, "EnableFontCheck3", FontToolParam.EnableFontCheck3);
		ini.WriteBool(section, "EnableFontCheck4", FontToolParam.EnableFontCheck4);
		ini.WriteBool(section, "EnableFontCheck5", FontToolParam.EnableFontCheck5);
		ini.WriteBool(section, "EnableFontCheck6", FontToolParam.EnableFontCheck6);
		ini.WriteBool(section, "EnableFontCheck7", FontToolParam.EnableFontCheck7);
		ini.WriteBool(section, "EnableFontCheck8", FontToolParam.EnableFontCheck8);
		ini.WriteBool(section, "EnableFontCheck9", FontToolParam.EnableFontCheck9);
		ini.WriteBool(section, "BypassOCR1", FontToolParam.BypassOCR1);
		ini.WriteBool(section, "BypassOCR2", FontToolParam.BypassOCR2);
		ini.WriteBool(section, "BypassOCR3", FontToolParam.BypassOCR3);
		ini.WriteBool(section, "BypassOCR4", FontToolParam.BypassOCR4);
		ini.WriteBool(section, "BypassOCR5", FontToolParam.BypassOCR5);
		ini.WriteBool(section, "BypassOCR6", FontToolParam.BypassOCR6);
		ini.WriteBool(section, "BypassOCR7", FontToolParam.BypassOCR7);
		ini.WriteBool(section, "BypassOCR8", FontToolParam.BypassOCR8);
		ini.WriteBool(section, "BypassOCR9", FontToolParam.BypassOCR9);
		ini.WriteString(section, "AllFont", FontToolParam.AllFont);
		ini.WriteString(section, "FontChar1", FontToolParam.FontChar1);
		ini.WriteBool(section, "DoubleWidthFont1", FontToolParam.DoubleWidthFont1);
		ini.WriteBool(section, "EnableAI_Font1", FontToolParam.EnableAI_Font1);
		ini.WriteString(section, "AIModel_Font1", FontToolParam.AIModel_Font1);
		ini.WriteInt(section, "MinScore_Font1", FontToolParam.MinScore_Font1);
		ini.WriteString(section, "FontChar2", FontToolParam.FontChar2);
		ini.WriteBool(section, "DoubleWidthFont2", FontToolParam.DoubleWidthFont2);
		ini.WriteBool(section, "EnableAI_Font2", FontToolParam.EnableAI_Font2);
		ini.WriteString(section, "AIModel_Font2", FontToolParam.AIModel_Font2);
		ini.WriteInt(section, "MinScore_Font2", FontToolParam.MinScore_Font2);
		ini.WriteString(section, "FontChar3", FontToolParam.FontChar3);
		ini.WriteBool(section, "DoubleWidthFont3", FontToolParam.DoubleWidthFont3);
		ini.WriteBool(section, "EnableAI_Font3", FontToolParam.EnableAI_Font3);
		ini.WriteString(section, "AIModel_Font3", FontToolParam.AIModel_Font3);
		ini.WriteInt(section, "MinScore_Font3", FontToolParam.MinScore_Font3);
		ini.WriteString(section, "FontChar4", FontToolParam.FontChar4);
		ini.WriteBool(section, "DoubleWidthFont4", FontToolParam.DoubleWidthFont4);
		ini.WriteBool(section, "EnableAI_Font4", FontToolParam.EnableAI_Font4);
		ini.WriteString(section, "AIModel_Font4", FontToolParam.AIModel_Font4);
		ini.WriteInt(section, "MinScore_Font4", FontToolParam.MinScore_Font4);
		ini.WriteString(section, "FontChar5", FontToolParam.FontChar5);
		ini.WriteBool(section, "DoubleWidthFont5", FontToolParam.DoubleWidthFont5);
		ini.WriteBool(section, "EnableAI_Font5", FontToolParam.EnableAI_Font5);
		ini.WriteString(section, "AIModel_Font5", FontToolParam.AIModel_Font5);
		ini.WriteInt(section, "MinScore_Font5", FontToolParam.MinScore_Font5);
		ini.WriteString(section, "FontChar6", FontToolParam.FontChar6);
		ini.WriteBool(section, "DoubleWidthFont6", FontToolParam.DoubleWidthFont6);
		ini.WriteBool(section, "EnableAI_Font6", FontToolParam.EnableAI_Font6);
		ini.WriteString(section, "AIModel_Font6", FontToolParam.AIModel_Font6);
		ini.WriteInt(section, "MinScore_Font6", FontToolParam.MinScore_Font6);
		ini.WriteString(section, "FontChar7", FontToolParam.FontChar7);
		ini.WriteBool(section, "DoubleWidthFont7", FontToolParam.DoubleWidthFont7);
		ini.WriteBool(section, "EnableAI_Font7", FontToolParam.EnableAI_Font7);
		ini.WriteString(section, "AIModel_Font7", FontToolParam.AIModel_Font7);
		ini.WriteInt(section, "MinScore_Font7", FontToolParam.MinScore_Font7);
		ini.WriteString(section, "FontChar8", FontToolParam.FontChar8);
		ini.WriteBool(section, "DoubleWidthFont8", FontToolParam.DoubleWidthFont8);
		ini.WriteBool(section, "EnableAI_Font8", FontToolParam.EnableAI_Font8);
		ini.WriteString(section, "AIModel_Font8", FontToolParam.AIModel_Font8);
		ini.WriteInt(section, "MinScore_Font8", FontToolParam.MinScore_Font8);
		ini.WriteString(section, "FontChar9", FontToolParam.FontChar9);
		ini.WriteBool(section, "DoubleWidthFont9", FontToolParam.DoubleWidthFont9);
		ini.WriteBool(section, "EnableAI_Font9", FontToolParam.EnableAI_Font9);
		ini.WriteString(section, "AIModel_Font9", FontToolParam.AIModel_Font9);
		ini.WriteInt(section, "MinScore_Font9", FontToolParam.MinScore_Font9);
		ini.WriteInt(section, "MinScore_ALL", FontToolParam.MinScore_ALL);
		ini.WriteBool(section, "UsePublicDB", FontToolParam.UsePublicDB);
		ini.WriteBool(section, "PublicDBUsePublicMode", FontToolParam.PublicDBUsePublicMode);
		FontToolParam.UsePublicDB = true;
		ini.WriteBool(section, "FontDBUsePublicMode", FontToolParam.FontDBUsePublicMode);
		ini.WriteBool(section, "FontAlignVertical", FontToolParam.FontAlignVertical);
		ini.WriteBool(section, "EnabledOCR", FontToolParam.EnabledOCR);
		ini.WriteBool(section, "AutoLoadAI", FontToolParam.AutoLoadAI);
		ini.WriteBool(section, "UseSameScore", FontToolParam.UseSameScore);
		ini.WriteBool(section, "FontConnected", FontToolParam.FontConnected);
		ini.WriteInt(section, "PatternChannel", FontToolParam.PatternChannel);
		ini.WriteInt(section, "FontExtend", FontToolParam.FontExtend);
		ini.WriteInt(section, "WFontExtend1", FontToolParam.WFontExtend1);
		ini.WriteInt(section, "WFontExtend2", FontToolParam.WFontExtend2);
		ini.WriteInt(section, "WFontExtend3", FontToolParam.WFontExtend3);
		ini.WriteInt(section, "WFontExtend4", FontToolParam.WFontExtend4);
		ini.WriteInt(section, "WFontExtend5", FontToolParam.WFontExtend5);
		ini.WriteInt(section, "WFontExtend6", FontToolParam.WFontExtend6);
		ini.WriteInt(section, "WFontExtend7", FontToolParam.WFontExtend7);
		ini.WriteInt(section, "WFontExtend8", FontToolParam.WFontExtend8);
		ini.WriteInt(section, "WFontExtend9", FontToolParam.WFontExtend9);
		ini.WriteInt(section, "HFontExtend1", FontToolParam.HFontExtend1);
		ini.WriteInt(section, "HFontExtend2", FontToolParam.HFontExtend2);
		ini.WriteInt(section, "HFontExtend3", FontToolParam.HFontExtend3);
		ini.WriteInt(section, "HFontExtend4", FontToolParam.HFontExtend4);
		ini.WriteInt(section, "HFontExtend5", FontToolParam.HFontExtend5);
		ini.WriteInt(section, "HFontExtend6", FontToolParam.HFontExtend6);
		ini.WriteInt(section, "HFontExtend7", FontToolParam.HFontExtend7);
		ini.WriteInt(section, "HFontExtend8", FontToolParam.HFontExtend8);
		ini.WriteInt(section, "HFontExtend9", FontToolParam.HFontExtend9);
		ini.WriteBool(section, "doAutoCombineFontToDB", FontToolParam.doAutoCombineFontToDB);
		ini.WriteBool(section, "AutoCombineFontToDBSet", FontToolParam.AutoCombineFontToDBSet);
		ini.WriteInt(section, "Font9Center1X", FontToolParam.Font9Center1.X);
		ini.WriteInt(section, "Font9Center1Y", FontToolParam.Font9Center1.Y);
		ini.WriteInt(section, "Font9Center2X", FontToolParam.Font9Center2.X);
		ini.WriteInt(section, "Font9Center2Y", FontToolParam.Font9Center2.Y);
		ini.WriteInt(section, "Font9Center3X", FontToolParam.Font9Center3.X);
		ini.WriteInt(section, "Font9Center3Y", FontToolParam.Font9Center3.Y);
		ini.WriteInt(section, "Font9Center4X", FontToolParam.Font9Center4.X);
		ini.WriteInt(section, "Font9Center4Y", FontToolParam.Font9Center4.Y);
		ini.WriteInt(section, "Font9Center5X", FontToolParam.Font9Center5.X);
		ini.WriteInt(section, "Font9Center5Y", FontToolParam.Font9Center5.Y);
		ini.WriteInt(section, "Font9Center6X", FontToolParam.Font9Center6.X);
		ini.WriteInt(section, "Font9Center6Y", FontToolParam.Font9Center6.Y);
		ini.WriteInt(section, "Font9Center7X", FontToolParam.Font9Center7.X);
		ini.WriteInt(section, "Font9Center7Y", FontToolParam.Font9Center7.Y);
		ini.WriteInt(section, "Font9Center8X", FontToolParam.Font9Center8.X);
		ini.WriteInt(section, "Font9Center8Y", FontToolParam.Font9Center8.Y);
		ini.WriteInt(section, "Font9Center9X", FontToolParam.Font9Center9.X);
		ini.WriteInt(section, "Font9Center9Y", FontToolParam.Font9Center9.Y);
		ini.WriteInt(section, "ErrorBin", (int)FontToolParam.ErrorBin);
		ini.WriteBool(section, "R_LT_H", FontToolParam.R_LT_H);
		ini.WriteBool(section, "R_LT_W", FontToolParam.R_LT_W);
		ini.WriteBool(section, "R_RT_H", FontToolParam.R_RT_H);
		ini.WriteBool(section, "R_RT_W", FontToolParam.R_RT_W);
		ini.WriteBool(section, "R_RB_H", FontToolParam.R_RB_H);
		ini.WriteBool(section, "R_LB_H", FontToolParam.R_LB_H);
		ini.WriteBool(section, "R_Area", FontToolParam.R_Area);
		ini.WriteBool(section, "R_T_W", FontToolParam.R_T_W);
		ini.WriteBool(section, "R_B_W", FontToolParam.R_B_W);
		ini.WriteBool(section, "R_L_H", FontToolParam.R_L_H);
		ini.WriteBool(section, "R_R_H", FontToolParam.R_R_H);
		ini.WriteBool(section, "R_C", FontToolParam.R_C);
		ini.WriteBool(section, "R_C_H_MIN", FontToolParam.R_C_H_MIN);
		ini.WriteBool(section, "R_LR_H_Diff", FontToolParam.R_LR_H_Diff);
		ini.WriteBool(section, "R_Break", FontToolParam.R_Break);
		ini.WriteInt(section, "R_BinaryShift", FontToolParam.R_BinaryShift);
		ini.WriteInt(section, "R_OpenClose", FontToolParam.R_OpenClose);
		ini.WriteInt(section, "R_OpenCloseCount", FontToolParam.R_OpenCloseCount);
		ini.WriteInt(section, "R_OpenClose2", FontToolParam.R_OpenClose2);
		ini.WriteInt(section, "R_OpenCloseCount2", FontToolParam.R_OpenCloseCount2);
		ini.WriteBool(section, "R_UseMorphFirst", FontToolParam.R_UseMorphFirst);
		ini.WriteBool(section, "R_UseFontDBMask", FontToolParam.R_UseFontDBMask);
		ini.WriteDouble(section, "R_LT_H_P", FontToolParam.R_LT_H_P);
		ini.WriteDouble(section, "R_LT_W_P", FontToolParam.R_LT_W_P);
		ini.WriteDouble(section, "R_RT_H_P", FontToolParam.R_RT_H_P);
		ini.WriteDouble(section, "R_RT_W_P", FontToolParam.R_RT_W_P);
		ini.WriteDouble(section, "R_RB_H_P", FontToolParam.R_RB_H_P);
		ini.WriteDouble(section, "R_LB_H_P", FontToolParam.R_LB_H_P);
		ini.WriteDouble(section, "R_Area_P", FontToolParam.R_Area_P);
		ini.WriteDouble(section, "R_T_W_P", FontToolParam.R_T_W_P);
		ini.WriteDouble(section, "R_B_W_P", FontToolParam.R_B_W_P);
		ini.WriteDouble(section, "R_L_H_P", FontToolParam.R_L_H_P);
		ini.WriteDouble(section, "R_R_H_P", FontToolParam.R_R_H_P);
		ini.WriteDouble(section, "R_C_P", FontToolParam.R_C_P);
		ini.WriteDouble(section, "R_LR_H_Diff_P", FontToolParam.R_LR_H_Diff_P);
		ini.WriteDouble(section, "R_C_H_MIN_P", FontToolParam.R_C_H_MIN_P);
		ini.WriteDouble(section, "R_C_H_MAX_P", FontToolParam.R_C_H_MAX_P);
		ini.WriteDouble(section, "R_Break_PMax", FontToolParam.R_Break_PMax);
		ini.WriteDouble(section, "R_Break_PMin", FontToolParam.R_Break_PMin);
		ini.WriteBool(section, "M_T_W", FontToolParam.M_T_W);
		ini.WriteBool(section, "M_B_W", FontToolParam.M_B_W);
		ini.WriteBool(section, "M_W", FontToolParam.M_W);
		ini.WriteBool(section, "M_Break", FontToolParam.M_Break);
		ini.WriteInt(section, "M_BinaryShift", FontToolParam.M_BinaryShift);
		ini.WriteInt(section, "M_OpenClose", FontToolParam.M_OpenClose);
		ini.WriteInt(section, "M_OpenCloseCount", FontToolParam.M_OpenCloseCount);
		ini.WriteInt(section, "M_OpenClose2", FontToolParam.M_OpenClose2);
		ini.WriteInt(section, "M_OpenCloseCount2", FontToolParam.M_OpenCloseCount2);
		ini.WriteBool(section, "M_UseMorphFirst", FontToolParam.M_UseMorphFirst);
		ini.WriteBool(section, "M_UseFontDBMask", FontToolParam.M_UseFontDBMask);
		ini.WriteBool(section, "M_Leg1", FontToolParam.M_Leg1);
		ini.WriteBool(section, "M_Leg2", FontToolParam.M_Leg2);
		ini.WriteBool(section, "M_Leg3", FontToolParam.M_Leg3);
		ini.WriteBool(section, "M_Connected", FontToolParam.M_Connected);
		ini.WriteBool(section, "C_W", FontToolParam.C_W);
		ini.WriteBool(section, "C_H", FontToolParam.C_H);
		ini.WriteBool(section, "C_T_W", FontToolParam.C_T_W);
		ini.WriteBool(section, "C_B_W", FontToolParam.C_B_W);
		ini.WriteBool(section, "C_L_H", FontToolParam.C_L_H);
		ini.WriteBool(section, "C_R_H", FontToolParam.C_R_H);
		ini.WriteBool(section, "C_Break", FontToolParam.C_Break);
		ini.WriteInt(section, "C_BinaryShift", FontToolParam.C_BinaryShift);
		ini.WriteInt(section, "C_OpenClose", FontToolParam.C_OpenClose);
		ini.WriteInt(section, "C_OpenCloseCount", FontToolParam.C_OpenCloseCount);
		ini.WriteInt(section, "C_OpenClose2", FontToolParam.C_OpenClose2);
		ini.WriteInt(section, "C_OpenCloseCount2", FontToolParam.C_OpenCloseCount2);
		ini.WriteBool(section, "C_UseMorphFirst", FontToolParam.C_UseMorphFirst);
		ini.WriteBool(section, "C_UseFontDBMask", FontToolParam.C_UseFontDBMask);
		ini.WriteBool(section, "U_W", FontToolParam.U_W);
		ini.WriteBool(section, "U_H", FontToolParam.U_H);
		ini.WriteBool(section, "U_T_W", FontToolParam.U_T_W);
		ini.WriteBool(section, "U_B_W", FontToolParam.U_B_W);
		ini.WriteBool(section, "U_L_H", FontToolParam.U_L_H);
		ini.WriteBool(section, "U_R_H", FontToolParam.U_R_H);
		ini.WriteBool(section, "U_Break", FontToolParam.U_Break);
		ini.WriteInt(section, "U_BinaryShift", FontToolParam.U_BinaryShift);
		ini.WriteInt(section, "U_OpenClose", FontToolParam.U_OpenClose);
		ini.WriteInt(section, "U_OpenCloseCount", FontToolParam.U_OpenCloseCount);
		ini.WriteInt(section, "U_OpenClose2", FontToolParam.U_OpenClose2);
		ini.WriteInt(section, "U_OpenCloseCount2", FontToolParam.U_OpenCloseCount2);
		ini.WriteBool(section, "U_UseMorphFirst", FontToolParam.U_UseMorphFirst);
		ini.WriteBool(section, "U_UseFontDBMask", FontToolParam.U_UseFontDBMask);
		ini.WriteBool(section, "X_W", FontToolParam.X_W);
		ini.WriteBool(section, "X_H", FontToolParam.X_H);
		ini.WriteBool(section, "X_T_W", FontToolParam.X_T_W);
		ini.WriteBool(section, "X_B_W", FontToolParam.X_B_W);
		ini.WriteBool(section, "X_L_H", FontToolParam.X_L_H);
		ini.WriteBool(section, "X_R_H", FontToolParam.X_R_H);
		ini.WriteBool(section, "X_Break", FontToolParam.X_Break);
		ini.WriteInt(section, "X_BinaryShift", FontToolParam.X_BinaryShift);
		ini.WriteInt(section, "X_OpenClose", FontToolParam.X_OpenClose);
		ini.WriteInt(section, "X_OpenCloseCount", FontToolParam.X_OpenCloseCount);
		ini.WriteInt(section, "X_OpenClose2", FontToolParam.X_OpenClose2);
		ini.WriteInt(section, "X_OpenCloseCount2", FontToolParam.X_OpenCloseCount2);
		ini.WriteBool(section, "X_UseMorphFirst", FontToolParam.X_UseMorphFirst);
		ini.WriteBool(section, "X_UseFontDBMask", FontToolParam.X_UseFontDBMask);
		ini.WriteBool(section, "TB_LT_H", FontToolParam.TB_LT_H);
		ini.WriteBool(section, "TB_LT_W", FontToolParam.TB_LT_W);
		ini.WriteBool(section, "TB_RT_H", FontToolParam.TB_RT_H);
		ini.WriteBool(section, "TB_RT_W", FontToolParam.TB_RT_W);
		ini.WriteBool(section, "TB_LB_H", FontToolParam.TB_LB_H);
		ini.WriteBool(section, "TB_LB_W", FontToolParam.TB_LB_W);
		ini.WriteBool(section, "TB_RB_H", FontToolParam.TB_RB_H);
		ini.WriteBool(section, "TB_RB_W", FontToolParam.TB_RB_W);
		ini.WriteBool(section, "TB_B_W", FontToolParam.TB_B_W);
		ini.WriteBool(section, "TB_C", FontToolParam.TB_C);
		ini.WriteBool(section, "TB_C_H_MIN", FontToolParam.TB_C_H_MIN);
		ini.WriteBool(section, "TB_C_H_MAX", FontToolParam.TB_C_H_MAX);
		ini.WriteBool(section, "TB_Break", FontToolParam.TB_Break);
		ini.WriteInt(section, "TB_BinaryShift", FontToolParam.TB_BinaryShift);
		ini.WriteInt(section, "TB_OpenClose", FontToolParam.TB_OpenClose);
		ini.WriteInt(section, "TB_OpenCloseCount", FontToolParam.TB_OpenCloseCount);
		ini.WriteInt(section, "TB_OpenClose2", FontToolParam.TB_OpenClose2);
		ini.WriteInt(section, "TB_OpenCloseCount2", FontToolParam.TB_OpenCloseCount2);
		ini.WriteBool(section, "TB_UseMorphFirst", FontToolParam.TB_UseMorphFirst);
		ini.WriteBool(section, "TB_UseFontDBMask", FontToolParam.TB_UseFontDBMask);
		ini.WriteBool(section, "TE_LT_H", FontToolParam.TE_LT_H);
		ini.WriteBool(section, "TE_LT_W", FontToolParam.TE_LT_W);
		ini.WriteBool(section, "TE_RT_H", FontToolParam.TE_RT_H);
		ini.WriteBool(section, "TE_RT_W", FontToolParam.TE_RT_W);
		ini.WriteBool(section, "TE_LB_H", FontToolParam.TE_LB_H);
		ini.WriteBool(section, "TE_LB_W", FontToolParam.TE_LB_W);
		ini.WriteBool(section, "TE_RB_H", FontToolParam.TE_RB_H);
		ini.WriteBool(section, "TE_RB_W", FontToolParam.TE_RB_W);
		ini.WriteBool(section, "TE_Break", FontToolParam.TE_Break);
		ini.WriteInt(section, "TE_BinaryShift", FontToolParam.TE_BinaryShift);
		ini.WriteInt(section, "TE_OpenClose", FontToolParam.TE_OpenClose);
		ini.WriteInt(section, "TE_OpenCloseCount", FontToolParam.TE_OpenCloseCount);
		ini.WriteInt(section, "TE_OpenClose2", FontToolParam.TE_OpenClose2);
		ini.WriteInt(section, "TE_OpenCloseCount2", FontToolParam.TE_OpenCloseCount2);
		ini.WriteBool(section, "TE_UseMorphFirst", FontToolParam.TE_UseMorphFirst);
		ini.WriteBool(section, "TE_UseFontDBMask", FontToolParam.TE_UseFontDBMask);
		ini.WriteBool(section, "Dash_H", FontToolParam.Dash_H);
		ini.WriteBool(section, "Dash_W", FontToolParam.Dash_W);
		ini.WriteBool(section, "Dash_Break", FontToolParam.Dash_Break);
		ini.WriteInt(section, "Dash_BinaryShift", FontToolParam.Dash_BinaryShift);
		ini.WriteInt(section, "Dash_OpenClose", FontToolParam.Dash_OpenClose);
		ini.WriteInt(section, "Dash_OpenCloseCount", FontToolParam.Dash_OpenCloseCount);
		ini.WriteInt(section, "Dash_OpenClose2", FontToolParam.Dash_OpenClose2);
		ini.WriteInt(section, "Dash_OpenCloseCount2", FontToolParam.Dash_OpenCloseCount2);
		ini.WriteBool(section, "Dash_UseMorphFirst", FontToolParam.Dash_UseMorphFirst);
		ini.WriteBool(section, "Dash_UseFontDBMask", FontToolParam.Dash_UseFontDBMask);
		ini.WriteBool(section, "D_LT_H", FontToolParam.D_LT_H);
		ini.WriteBool(section, "D_LT_W", FontToolParam.D_LT_W);
		ini.WriteBool(section, "D_RT_H", FontToolParam.D_RT_H);
		ini.WriteBool(section, "D_RT_W", FontToolParam.D_RT_W);
		ini.WriteBool(section, "D_LB_H", FontToolParam.D_LB_H);
		ini.WriteBool(section, "D_LB_W", FontToolParam.D_LB_W);
		ini.WriteBool(section, "D_RB_H", FontToolParam.D_RB_H);
		ini.WriteBool(section, "D_RB_W", FontToolParam.D_RB_W);
		ini.WriteBool(section, "D_T_W", FontToolParam.D_T_W);
		ini.WriteBool(section, "D_B_W", FontToolParam.D_B_W);
		ini.WriteBool(section, "D_C", FontToolParam.D_C);
		ini.WriteBool(section, "D_C_H_MIN", FontToolParam.D_C_H_MIN);
		ini.WriteBool(section, "D_C_H_MAX", FontToolParam.D_C_H_MAX);
		ini.WriteBool(section, "D_Break", FontToolParam.D_Break);
		ini.WriteInt(section, "D_BinaryShift", FontToolParam.D_BinaryShift);
		ini.WriteInt(section, "D_OpenClose", FontToolParam.D_OpenClose);
		ini.WriteInt(section, "D_OpenCloseCount", FontToolParam.D_OpenCloseCount);
		ini.WriteInt(section, "D_OpenClose2", FontToolParam.D_OpenClose2);
		ini.WriteInt(section, "D_OpenCloseCount2", FontToolParam.D_OpenCloseCount2);
		ini.WriteBool(section, "D_UseMorphFirst", FontToolParam.D_UseMorphFirst);
		ini.WriteBool(section, "D_UseFontDBMask", FontToolParam.D_UseFontDBMask);
		ini.WriteBool(section, "A_W", FontToolParam.A_W);
		ini.WriteBool(section, "A_H", FontToolParam.A_H);
		ini.WriteBool(section, "A_T_W", FontToolParam.A_T_W);
		ini.WriteBool(section, "A_B_W", FontToolParam.A_B_W);
		ini.WriteBool(section, "A_L_H", FontToolParam.A_L_H);
		ini.WriteBool(section, "A_R_H", FontToolParam.A_R_H);
		ini.WriteBool(section, "A_Break", FontToolParam.A_Break);
		ini.WriteInt(section, "A_BinaryShift", FontToolParam.A_BinaryShift);
		ini.WriteInt(section, "A_OpenClose", FontToolParam.A_OpenClose);
		ini.WriteInt(section, "A_OpenCloseCount", FontToolParam.A_OpenCloseCount);
		ini.WriteInt(section, "A_OpenClose2", FontToolParam.A_OpenClose2);
		ini.WriteInt(section, "A_OpenCloseCount2", FontToolParam.A_OpenCloseCount2);
		ini.WriteBool(section, "A_UseMorphFirst", FontToolParam.A_UseMorphFirst);
		ini.WriteBool(section, "A_UseFontDBMask", FontToolParam.A_UseFontDBMask);
		ini.WriteBool(section, "T0_LT_H", FontToolParam.T0_LT_H);
		ini.WriteBool(section, "T0_LT_W", FontToolParam.T0_LT_W);
		ini.WriteBool(section, "T0_RT_H", FontToolParam.T0_RT_H);
		ini.WriteBool(section, "T0_RT_W", FontToolParam.T0_RT_W);
		ini.WriteBool(section, "T0_LB_H", FontToolParam.T0_LB_H);
		ini.WriteBool(section, "T0_LB_W", FontToolParam.T0_LB_W);
		ini.WriteBool(section, "T0_RB_H", FontToolParam.T0_RB_H);
		ini.WriteBool(section, "T0_RB_W", FontToolParam.T0_RB_W);
		ini.WriteBool(section, "T0_T_W", FontToolParam.T0_T_W);
		ini.WriteBool(section, "T0_B_W", FontToolParam.T0_B_W);
		ini.WriteBool(section, "T0_C", FontToolParam.T0_C);
		ini.WriteBool(section, "T0_C_H_MIN", FontToolParam.T0_C_H_MIN);
		ini.WriteBool(section, "T0_C_H_MAX", FontToolParam.T0_C_H_MAX);
		ini.WriteBool(section, "T0_Break", FontToolParam.T0_Break);
		ini.WriteBool(section, "T0_L_H", FontToolParam.T0_L_H);
		ini.WriteBool(section, "T0_R_H", FontToolParam.T0_R_H);
		ini.WriteInt(section, "T0_BinaryShift", FontToolParam.T0_BinaryShift);
		ini.WriteInt(section, "T0_OpenClose", FontToolParam.T0_OpenClose);
		ini.WriteInt(section, "T0_OpenCloseCount", FontToolParam.T0_OpenCloseCount);
		ini.WriteInt(section, "T0_OpenClose2", FontToolParam.T0_OpenClose2);
		ini.WriteInt(section, "T0_OpenCloseCount2", FontToolParam.T0_OpenCloseCount2);
		ini.WriteBool(section, "T0_UseMorphFirst", FontToolParam.T0_UseMorphFirst);
		ini.WriteBool(section, "T0_UseFontDBMask", FontToolParam.T0_UseFontDBMask);
		ini.WriteBool(section, "T1_H", FontToolParam.T1_H);
		ini.WriteBool(section, "T1_W", FontToolParam.T1_W);
		ini.WriteBool(section, "T1_Break", FontToolParam.T1_Break);
		ini.WriteInt(section, "T1_BinaryShift", FontToolParam.T1_BinaryShift);
		ini.WriteInt(section, "T1_OpenClose", FontToolParam.T1_OpenClose);
		ini.WriteInt(section, "T1_OpenCloseCount", FontToolParam.T1_OpenCloseCount);
		ini.WriteInt(section, "T1_OpenClose", FontToolParam.T1_OpenClose);
		ini.WriteInt(section, "T1_OpenCloseCount", FontToolParam.T1_OpenCloseCount);
		ini.WriteInt(section, "T1_OpenClose2", FontToolParam.T1_OpenClose2);
		ini.WriteInt(section, "T1_OpenCloseCount2", FontToolParam.T1_OpenCloseCount2);
		ini.WriteBool(section, "T1_UseMorphFirst", FontToolParam.T1_UseMorphFirst);
		ini.WriteBool(section, "T1_UseFontDBMask", FontToolParam.T1_UseFontDBMask);
		ini.WriteBool(section, "T2_LT_H", FontToolParam.T2_LT_H);
		ini.WriteBool(section, "T2_LT_W", FontToolParam.T2_LT_W);
		ini.WriteBool(section, "T2_RT_H", FontToolParam.T2_RT_H);
		ini.WriteBool(section, "T2_RT_W", FontToolParam.T2_RT_W);
		ini.WriteBool(section, "T2_LB_H", FontToolParam.T2_LB_H);
		ini.WriteBool(section, "T2_LB_W", FontToolParam.T2_LB_W);
		ini.WriteBool(section, "T2_RB_H", FontToolParam.T2_RB_H);
		ini.WriteBool(section, "T2_RB_W", FontToolParam.T2_RB_W);
		ini.WriteBool(section, "T2_Break", FontToolParam.T2_Break);
		ini.WriteInt(section, "T2_BinaryShift", FontToolParam.T2_BinaryShift);
		ini.WriteInt(section, "T2_OpenClose", FontToolParam.T2_OpenClose);
		ini.WriteInt(section, "T2_OpenCloseCount", FontToolParam.T2_OpenCloseCount);
		ini.WriteInt(section, "T2_OpenClose2", FontToolParam.T2_OpenClose2);
		ini.WriteInt(section, "T2_OpenCloseCount2", FontToolParam.T2_OpenCloseCount2);
		ini.WriteBool(section, "T2_UseMorphFirst", FontToolParam.T2_UseMorphFirst);
		ini.WriteBool(section, "T2_UseFontDBMask", FontToolParam.T2_UseFontDBMask);
		ini.WriteBool(section, "T3_LT_H", FontToolParam.T3_LT_H);
		ini.WriteBool(section, "T3_LT_W", FontToolParam.T3_LT_W);
		ini.WriteBool(section, "T3_RT_H", FontToolParam.T3_RT_H);
		ini.WriteBool(section, "T3_RT_W", FontToolParam.T3_RT_W);
		ini.WriteBool(section, "T3_LB_H", FontToolParam.T3_LB_H);
		ini.WriteBool(section, "T3_LB_W", FontToolParam.T3_LB_W);
		ini.WriteBool(section, "T3_RB_H", FontToolParam.T3_RB_H);
		ini.WriteBool(section, "T3_RB_W", FontToolParam.T3_RB_W);
		ini.WriteBool(section, "T3_Break", FontToolParam.T3_Break);
		ini.WriteBool(section, "T3_T_W", FontToolParam.T3_T_W);
		ini.WriteBool(section, "T3_B_W", FontToolParam.T3_B_W);
		ini.WriteBool(section, "T3_C_W", FontToolParam.T3_C_W);
		ini.WriteInt(section, "T3_BinaryShift", FontToolParam.T3_BinaryShift);
		ini.WriteInt(section, "T3_OpenClose", FontToolParam.T3_OpenClose);
		ini.WriteInt(section, "T3_OpenCloseCount", FontToolParam.T3_OpenCloseCount);
		ini.WriteInt(section, "T3_OpenClose2", FontToolParam.T3_OpenClose2);
		ini.WriteInt(section, "T3_OpenCloseCount2", FontToolParam.T3_OpenCloseCount2);
		ini.WriteBool(section, "T3_UseMorphFirst", FontToolParam.T3_UseMorphFirst);
		ini.WriteBool(section, "T3_UseFontDBMask", FontToolParam.T3_UseFontDBMask);
		ini.WriteBool(section, "T4_LT_H", FontToolParam.T4_LT_H);
		ini.WriteBool(section, "T4_LT_W", FontToolParam.T4_LT_W);
		ini.WriteBool(section, "T4_RT_H", FontToolParam.T4_RT_H);
		ini.WriteBool(section, "T4_RT_W", FontToolParam.T4_RT_W);
		ini.WriteBool(section, "T4_T_W", FontToolParam.T4_T_W);
		ini.WriteBool(section, "T4_B_W", FontToolParam.T4_B_W);
		ini.WriteBool(section, "T4_Break", FontToolParam.T4_Break);
		ini.WriteInt(section, "T4_BinaryShift", FontToolParam.T4_BinaryShift);
		ini.WriteInt(section, "T4_OpenClose", FontToolParam.T4_OpenClose);
		ini.WriteInt(section, "T4_OpenCloseCount", FontToolParam.T4_OpenCloseCount);
		ini.WriteInt(section, "T4_OpenClose2", FontToolParam.T4_OpenClose2);
		ini.WriteInt(section, "T4_OpenCloseCount2", FontToolParam.T4_OpenCloseCount2);
		ini.WriteBool(section, "T4_UseMorphFirst", FontToolParam.T4_UseMorphFirst);
		ini.WriteBool(section, "T4_UseFontDBMask", FontToolParam.T4_UseFontDBMask);
		ini.WriteBool(section, "T5_LT_H", FontToolParam.T5_LT_H);
		ini.WriteBool(section, "T5_LT_W", FontToolParam.T5_LT_W);
		ini.WriteBool(section, "T5_RT_H", FontToolParam.T5_RT_H);
		ini.WriteBool(section, "T5_RT_W", FontToolParam.T5_RT_W);
		ini.WriteBool(section, "T5_LB_H", FontToolParam.T5_LB_H);
		ini.WriteBool(section, "T5_LB_W", FontToolParam.T5_LB_W);
		ini.WriteBool(section, "T5_RB_H", FontToolParam.T5_RB_H);
		ini.WriteBool(section, "T5_RB_W", FontToolParam.T5_RB_W);
		ini.WriteBool(section, "T5_Break", FontToolParam.T5_Break);
		ini.WriteBool(section, "T5_TT_W", FontToolParam.T5_TT_W);
		ini.WriteBool(section, "T5_TB_W", FontToolParam.T5_TB_W);
		ini.WriteInt(section, "T5_BinaryShift", FontToolParam.T5_BinaryShift);
		ini.WriteInt(section, "T5_OpenClose", FontToolParam.T5_OpenClose);
		ini.WriteInt(section, "T5_OpenCloseCount", FontToolParam.T5_OpenCloseCount);
		ini.WriteInt(section, "T5_OpenClose2", FontToolParam.T5_OpenClose2);
		ini.WriteInt(section, "T5_OpenCloseCount2", FontToolParam.T5_OpenCloseCount2);
		ini.WriteBool(section, "T5_UseMorphFirst", FontToolParam.T5_UseMorphFirst);
		ini.WriteBool(section, "T5_UseFontDBMask", FontToolParam.T5_UseFontDBMask);
		ini.WriteBool(section, "T6_LT_H", FontToolParam.T6_LT_H);
		ini.WriteBool(section, "T6_LT_W", FontToolParam.T6_LT_W);
		ini.WriteBool(section, "T6_RT_H", FontToolParam.T6_RT_H);
		ini.WriteBool(section, "T6_RT_W", FontToolParam.T6_RT_W);
		ini.WriteBool(section, "T6_T_W", FontToolParam.T6_T_W);
		ini.WriteBool(section, "T6_B_W", FontToolParam.T6_B_W);
		ini.WriteBool(section, "T6_Break", FontToolParam.T6_Break);
		ini.WriteInt(section, "T6_BinaryShift", FontToolParam.T6_BinaryShift);
		ini.WriteInt(section, "T6_OpenClose", FontToolParam.T6_OpenClose);
		ini.WriteInt(section, "T6_OpenCloseCount", FontToolParam.T6_OpenCloseCount);
		ini.WriteInt(section, "T6_OpenClose2", FontToolParam.T6_OpenClose2);
		ini.WriteInt(section, "T6_OpenCloseCount2", FontToolParam.T6_OpenCloseCount2);
		ini.WriteBool(section, "T6_UseMorphFirst", FontToolParam.T6_UseMorphFirst);
		ini.WriteBool(section, "T6_UseFontDBMask", FontToolParam.T6_UseFontDBMask);
		ini.WriteBool(section, "T7_T_W", FontToolParam.T7_T_W);
		ini.WriteBool(section, "T7_B_W", FontToolParam.T7_B_W);
		ini.WriteBool(section, "T7_W", FontToolParam.T7_W);
		ini.WriteBool(section, "T7_H", FontToolParam.T7_H);
		ini.WriteBool(section, "T7_Break", FontToolParam.T7_Break);
		ini.WriteInt(section, "T7_BinaryShift", FontToolParam.T7_BinaryShift);
		ini.WriteInt(section, "T7_OpenClose", FontToolParam.T7_OpenClose);
		ini.WriteInt(section, "T7_OpenCloseCount", FontToolParam.T7_OpenCloseCount);
		ini.WriteInt(section, "T7_OpenClose2", FontToolParam.T7_OpenClose2);
		ini.WriteInt(section, "T7_OpenCloseCount2", FontToolParam.T7_OpenCloseCount2);
		ini.WriteBool(section, "T7_UseMorphFirst", FontToolParam.T7_UseMorphFirst);
		ini.WriteBool(section, "T7_UseFontDBMask", FontToolParam.T7_UseFontDBMask);
		ini.WriteBool(section, "T8_LT_H", FontToolParam.T8_LT_H);
		ini.WriteBool(section, "T8_LT_W", FontToolParam.T8_LT_W);
		ini.WriteBool(section, "T8_RT_H", FontToolParam.T8_RT_H);
		ini.WriteBool(section, "T8_RT_W", FontToolParam.T8_RT_W);
		ini.WriteBool(section, "T8_LB_H", FontToolParam.T8_LB_H);
		ini.WriteBool(section, "T8_LB_W", FontToolParam.T8_LB_W);
		ini.WriteBool(section, "T8_RB_H", FontToolParam.T8_RB_H);
		ini.WriteBool(section, "T8_RB_W", FontToolParam.T8_RB_W);
		ini.WriteBool(section, "T8_B_W", FontToolParam.T8_B_W);
		ini.WriteBool(section, "T8_C", FontToolParam.T8_C);
		ini.WriteBool(section, "T8_C_H_MIN", FontToolParam.T8_C_H_MIN);
		ini.WriteBool(section, "T8_C_H_MAX", FontToolParam.T8_C_H_MAX);
		ini.WriteBool(section, "T8_Break", FontToolParam.T8_Break);
		ini.WriteBool(section, "T8_L_H", FontToolParam.T8_L_H);
		ini.WriteBool(section, "T8_R_H", FontToolParam.T8_R_H);
		ini.WriteInt(section, "T8_BinaryShift", FontToolParam.T8_BinaryShift);
		ini.WriteInt(section, "T8_OpenClose", FontToolParam.T8_OpenClose);
		ini.WriteInt(section, "T8_OpenCloseCount", FontToolParam.T8_OpenCloseCount);
		ini.WriteInt(section, "T8_OpenClose2", FontToolParam.T8_OpenClose2);
		ini.WriteInt(section, "T8_OpenCloseCount2", FontToolParam.T8_OpenCloseCount2);
		ini.WriteBool(section, "T8_UseMorphFirst", FontToolParam.T8_UseMorphFirst);
		ini.WriteBool(section, "T8_UseFontDBMask", FontToolParam.T8_UseFontDBMask);
		ini.WriteBool(section, "T9_LT_H", FontToolParam.T9_LT_H);
		ini.WriteBool(section, "T9_LT_W", FontToolParam.T9_LT_W);
		ini.WriteBool(section, "T9_RT_H", FontToolParam.T9_RT_H);
		ini.WriteBool(section, "T9_RT_W", FontToolParam.T9_RT_W);
		ini.WriteBool(section, "T9_B_H", FontToolParam.T9_B_H);
		ini.WriteBool(section, "T9_B_W", FontToolParam.T9_B_W);
		ini.WriteBool(section, "T9_Break", FontToolParam.T9_Break);
		ini.WriteInt(section, "T9_BinaryShift", FontToolParam.T9_BinaryShift);
		ini.WriteInt(section, "T9_OpenClose", FontToolParam.T9_OpenClose);
		ini.WriteInt(section, "T9_OpenCloseCount", FontToolParam.T9_OpenCloseCount);
		ini.WriteInt(section, "T9_OpenClose2", FontToolParam.T9_OpenClose2);
		ini.WriteInt(section, "T9_OpenCloseCount2", FontToolParam.T9_OpenCloseCount2);
		ini.WriteBool(section, "T9_UseMorphFirst", FontToolParam.T9_UseMorphFirst);
		ini.WriteBool(section, "T9_UseFontDBMask", FontToolParam.T9_UseFontDBMask);
		ini.WriteInt(section, "ConnectBinaryShift", FontToolParam.ConnectBinaryShift);
		ini.WriteInt(section, "ConnectOpenClose", FontToolParam.ConnectOpenClose);
		ini.WriteInt(section, "ConnectOpenCloseCount", FontToolParam.ConnectOpenCloseCount);
		ini.WriteInt(section, "ConnectOpenClose2", FontToolParam.ConnectOpenClose2);
		ini.WriteInt(section, "ConnectOpenCloseCount2", FontToolParam.ConnectOpenCloseCount2);
		ini.WriteBool(section, "ConnectCountAlarm", FontToolParam.ConnectCountAlarm);
		ini.WriteBool(section, "ConnectWidthAlarm", FontToolParam.ConnectWidthAlarm);
		ini.WriteBool(section, "UseFastMatch", FontToolParam.UseFastMatch);
		ini.WriteBool(section, "AutoSetFontImage", FontToolParam.AutoSetFontImage);
		for (int i = 0; i < 20; i++)
		{
			ini.WriteBool(section, "FT" + i + "_LT_H", FontToolParam.OtherFont[i].T_LT_H);
			ini.WriteBool(section, "FT" + i + "_LT_W", FontToolParam.OtherFont[i].T_LT_W);
			ini.WriteBool(section, "FT" + i + "_RT_H", FontToolParam.OtherFont[i].T_RT_H);
			ini.WriteBool(section, "FT" + i + "_RT_W", FontToolParam.OtherFont[i].T_RT_W);
			ini.WriteBool(section, "FT" + i + "_LB_H", FontToolParam.OtherFont[i].T_LB_H);
			ini.WriteBool(section, "FT" + i + "_LB_W", FontToolParam.OtherFont[i].T_LB_W);
			ini.WriteBool(section, "FT" + i + "_RB_H", FontToolParam.OtherFont[i].T_RB_H);
			ini.WriteBool(section, "FT" + i + "_RB_W", FontToolParam.OtherFont[i].T_RB_W);
			ini.WriteBool(section, "FT" + i + "_Break", FontToolParam.OtherFont[i].T_Break);
			ini.WriteBool(section, "FT" + i + "_T_W", FontToolParam.OtherFont[i].T_T_W);
			ini.WriteBool(section, "FT" + i + "_B_W", FontToolParam.OtherFont[i].T_B_W);
			ini.WriteBool(section, "FT" + i + "_C_W", FontToolParam.OtherFont[i].T_C_W);
			ini.WriteInt(section, "FT" + i + "_BinaryShift", FontToolParam.OtherFont[i].T_BinaryShift);
			ini.WriteInt(section, "FT" + i + "_OpenClose", FontToolParam.OtherFont[i].T_OpenClose);
			ini.WriteInt(section, "FT" + i + "_OpenCloseCount", FontToolParam.OtherFont[i].T_OpenCloseCount);
			ini.WriteInt(section, "FT" + i + "_OpenClose2", FontToolParam.OtherFont[i].T_OpenClose2);
			ini.WriteInt(section, "FT" + i + "_OpenCloseCount2", FontToolParam.OtherFont[i].T_OpenCloseCount2);
			ini.WriteBool(section, "FT" + i + "_UseMorphFirst", FontToolParam.OtherFont[i].T_UseMorphFirst);
			ini.WriteBool(section, "FT" + i + "_UseFontDBMask", FontToolParam.OtherFont[i].T_UseFontDBMask);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadIRToolParam()
	{
		IRToolParam.ValueChangeFlag = false;
		string section = "IRTool";
		IRToolParam.RingsType = ini.GetInteger(section, "RingsType", 3);
		IRToolParam.ParetoIndex = ini.GetInteger(section, "ParetoIndex");
		IRToolParam.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		int[] array = new int[4]
		{
			(IRToolParam.RingsColor1 = ini.GetInteger(section, "RingsColor1")),
			(IRToolParam.RingsColor2 = ini.GetInteger(section, "RingsColor2", 1)),
			(IRToolParam.RingsColor3 = ini.GetInteger(section, "RingsColor3", 2)),
			(IRToolParam.RingsColor4 = ini.GetInteger(section, "RingsColor4", 3))
		};
		IRToolParam.ColorRings[0].TrackerON = true;
		for (int i = 0; i < 4; i++)
		{
			ReadBinaryParam(ref IRToolParam.ColorRings[i], i, "IRTool_Color");
			IRToolParam.ColorRings[i].SearchRect2ON = false;
			IRToolParam.ColorRings[i].Tracker1UseAutoSize = false;
			IRToolParam.ColorRings[i].Tracker1AligmentXIndex = 0;
			IRToolParam.ColorRings[i].Tracker1AligmentYIndex = 0;
			IRToolParam.ColorRings[i].MaskSelectIndex = 0;
			IRToolParam.ColorRings[i].UseMaskAsInspArea = false;
			if (array[i] == 13)
			{
				IRToolParam.ColorRings[i].TrackerON = false;
			}
			else
			{
				IRToolParam.ColorRings[i].ColorReferenceIndex = ini.GetInteger(section, "ColorReferenceIndex", array[i]);
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteIRToolParam()
	{
		string section = "IRTool";
		ini.WriteInt(section, "RingsType", IRToolParam.RingsType);
		ini.WriteInt(section, "RingsColor1", IRToolParam.RingsColor1);
		ini.WriteInt(section, "RingsColor2", IRToolParam.RingsColor2);
		ini.WriteInt(section, "RingsColor3", IRToolParam.RingsColor3);
		ini.WriteInt(section, "RingsColor4", IRToolParam.RingsColor4);
		ini.WriteInt(section, "ParetoIndex", IRToolParam.ParetoIndex);
		ini.WriteInt(section, "ErrorBin", (int)IRToolParam.ErrorBin);
		for (int i = 0; i < 4; i++)
		{
			WriteBinaryParam(IRToolParam.ColorRings[i], i, "IRTool_Color");
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadAIProphetParam()
	{
		AIProphetParam.ValueChangeFlag = false;
		string section = "AIProphetTool";
		AIProphetParam.ModelName = ini.GetValue(section, "ModelName");
		AIProphetParam.ModelImagePath = ini.GetValue(section, "ModelImagePath");
		AIProphetParam.ModelImageSize = ini.GetInteger(section, "ModelImageSize", 256);
		AIProphetParam.TrainCount = ini.GetInteger(section, "TrainCount", 300);
		AIProphetParam.Track_L = ini.GetInteger(section, "Track_L", -6);
		AIProphetParam.Track_R = ini.GetInteger(section, "Track_R", -6);
		AIProphetParam.Track_T = ini.GetInteger(section, "Track_T", -6);
		AIProphetParam.Track_B = ini.GetInteger(section, "Track_B", -6);
		AIProphetParam.EnableTensorRT = ini.GetBoolean(section, "EnableTensorRT");
		AIProphetParam.Mask1_ON = ini.GetBoolean(section, "Mask1_ON");
		AIProphetParam.Mask2_ON = ini.GetBoolean(section, "Mask2_ON");
		AIProphetParam.Mask3_ON = ini.GetBoolean(section, "Mask3_ON");
		AIProphetParam.Mask4_ON = ini.GetBoolean(section, "Mask4_ON");
		AIProphetParam.Mask1_L = ini.GetInteger(section, "Mask1_L", 5);
		AIProphetParam.Mask1_R = ini.GetInteger(section, "Mask1_R", 5);
		AIProphetParam.Mask1_T = ini.GetInteger(section, "Mask1_T", 5);
		AIProphetParam.Mask1_B = ini.GetInteger(section, "Mask1_B", 5);
		AIProphetParam.Mask2_L = ini.GetInteger(section, "Mask2_L", 5);
		AIProphetParam.Mask2_R = ini.GetInteger(section, "Mask2_R", 5);
		AIProphetParam.Mask2_T = ini.GetInteger(section, "Mask2_T", 5);
		AIProphetParam.Mask2_B = ini.GetInteger(section, "Mask2_B", 5);
		AIProphetParam.Mask3_L = ini.GetInteger(section, "Mask3_L", 5);
		AIProphetParam.Mask3_R = ini.GetInteger(section, "Mask3_R", 5);
		AIProphetParam.Mask3_T = ini.GetInteger(section, "Mask3_T", 5);
		AIProphetParam.Mask3_B = ini.GetInteger(section, "Mask3_B", 5);
		AIProphetParam.Mask4_L = ini.GetInteger(section, "Mask4_L", 5);
		AIProphetParam.Mask4_R = ini.GetInteger(section, "Mask4_R", 5);
		AIProphetParam.Mask4_T = ini.GetInteger(section, "Mask4_T", 5);
		AIProphetParam.Mask4_B = ini.GetInteger(section, "Mask4_B", 5);
		AIProphetParam.AllowSocreMin = ini.GetDouble(section, "AllowSocreMin");
		AIProphetParam.AllowSocreMax = ini.GetDouble(section, "AllowSocreMax", 1.0);
		AIProphetParam.ModelUnTrain = ini.GetBoolean(section, "ModelUnTrain");
		AIProphetParam.ErrorBin = (FLOW_BIN)ini.GetInteger(section, "ErrorBin", 2);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteAIProphetParam()
	{
		AIProphetParam.ValueChangeFlag = false;
		string section = "AIProphetTool";
		ini.WriteString(section, "ModelName", AIProphetParam.ModelName);
		ini.WriteString(section, "ModelImagePath", AIProphetParam.ModelImagePath);
		ini.WriteInt(section, "ModelImageSize", AIProphetParam.ModelImageSize);
		ini.WriteInt(section, "TrainCount", AIProphetParam.TrainCount);
		ini.WriteInt(section, "Track_L", AIProphetParam.Track_L);
		ini.WriteInt(section, "Track_R", AIProphetParam.Track_R);
		ini.WriteInt(section, "Track_T", AIProphetParam.Track_T);
		ini.WriteInt(section, "Track_B", AIProphetParam.Track_B);
		ini.WriteBool(section, "EnableTensorRT", AIProphetParam.EnableTensorRT);
		ini.WriteBool(section, "Mask1_ON", AIProphetParam.Mask1_ON);
		ini.WriteBool(section, "Mask2_ON", AIProphetParam.Mask2_ON);
		ini.WriteBool(section, "Mask3_ON", AIProphetParam.Mask3_ON);
		ini.WriteBool(section, "Mask4_ON", AIProphetParam.Mask4_ON);
		ini.WriteInt(section, "Mask1_L", AIProphetParam.Mask1_L);
		ini.WriteInt(section, "Mask1_R", AIProphetParam.Mask1_R);
		ini.WriteInt(section, "Mask1_T", AIProphetParam.Mask1_T);
		ini.WriteInt(section, "Mask1_B", AIProphetParam.Mask1_B);
		ini.WriteInt(section, "Mask2_L", AIProphetParam.Mask2_L);
		ini.WriteInt(section, "Mask2_R", AIProphetParam.Mask2_R);
		ini.WriteInt(section, "Mask2_T", AIProphetParam.Mask2_T);
		ini.WriteInt(section, "Mask2_B", AIProphetParam.Mask2_B);
		ini.WriteInt(section, "Mask3_L", AIProphetParam.Mask3_L);
		ini.WriteInt(section, "Mask3_R", AIProphetParam.Mask3_R);
		ini.WriteInt(section, "Mask3_T", AIProphetParam.Mask3_T);
		ini.WriteInt(section, "Mask3_B", AIProphetParam.Mask3_B);
		ini.WriteInt(section, "Mask4_L", AIProphetParam.Mask4_L);
		ini.WriteInt(section, "Mask4_R", AIProphetParam.Mask4_R);
		ini.WriteInt(section, "Mask4_T", AIProphetParam.Mask4_T);
		ini.WriteInt(section, "Mask4_B", AIProphetParam.Mask4_B);
		ini.WriteDouble(section, "AllowSocreMin", AIProphetParam.AllowSocreMin);
		ini.WriteDouble(section, "AllowSocreMax", AIProphetParam.AllowSocreMax);
		ini.WriteBool(section, "ModelUnTrain", AIProphetParam.ModelUnTrain);
		ini.WriteInt(section, "ErrorBin", (int)AIProphetParam.ErrorBin);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadAIEnhanceParam()
	{
		AIEnhanceParam.ValueChangeFlag = false;
		string section = "AIEnhanceTool";
		AIEnhanceParam.ModelName_A = ini.GetValue(section, "ModelName_A");
		AIEnhanceParam.LastModelFile_A = ini.GetValue(section, "LastModelFile_A");
		AIEnhanceParam.EnableAsignGPU = ini.GetBoolean(section, "EnableAsignGPU");
		AIEnhanceParam.GPUID = ini.GetInteger(section, "GPUID");
		AIEnhanceParam.Width_A = ini.GetInteger(section, "Width_A", 256);
		AIEnhanceParam.Height_A = ini.GetInteger(section, "Height_A", 256);
		AIEnhanceParam.DnetMode_A = ini.GetInteger(section, "DnetMode_A", 1);
		AIEnhanceParam.DnetROIIndex_A1 = ini.GetInteger(section, "DnetROIIndex_A1", -1);
		AIEnhanceParam.DnetROIIndex_A2 = ini.GetInteger(section, "DnetROIIndex_A2", -1);
		AIEnhanceParam.DnetROIIndex_A3 = ini.GetInteger(section, "DnetROIIndex_A3", -1);
		AIEnhanceParam.DnetROIIndex_A4 = ini.GetInteger(section, "DnetROIIndex_A4", -1);
		AIEnhanceParam.DnetROIIndex_A5 = ini.GetInteger(section, "DnetROIIndex_A5", -1);
		AIEnhanceParam.DnetROIIndex_A6 = ini.GetInteger(section, "DnetROIIndex_A6", -1);
		AIEnhanceParam.DnetROIIndex_A7 = ini.GetInteger(section, "DnetROIIndex_A7", -1);
		AIEnhanceParam.DnetROIIndex_A8 = ini.GetInteger(section, "DnetROIIndex_A8", -1);
		AIEnhanceParam.DnetROIIndex_B1 = ini.GetInteger(section, "DnetROIIndex_B1", -1);
		AIEnhanceParam.DnetROIIndex_B2 = ini.GetInteger(section, "DnetROIIndex_B2", -1);
		AIEnhanceParam.DnetROIIndex_B3 = ini.GetInteger(section, "DnetROIIndex_B3", -1);
		AIEnhanceParam.DnetROIIndex_B4 = ini.GetInteger(section, "DnetROIIndex_B4", -1);
		AIEnhanceParam.DnetROIIndex_B5 = ini.GetInteger(section, "DnetROIIndex_B5", -1);
		AIEnhanceParam.DnetROIIndex_B6 = ini.GetInteger(section, "DnetROIIndex_B6", -1);
		AIEnhanceParam.DnetROIIndex_B7 = ini.GetInteger(section, "DnetROIIndex_B7", -1);
		AIEnhanceParam.DnetROIIndex_B8 = ini.GetInteger(section, "DnetROIIndex_B8", -1);
		AIEnhanceParam.ModelName_B = ini.GetValue(section, "ModelName_B");
		AIEnhanceParam.LastModelFile_B = ini.GetValue(section, "LastModelFile_B");
		AIEnhanceParam.Width_B = ini.GetInteger(section, "Width_B", 256);
		AIEnhanceParam.Height_B = ini.GetInteger(section, "Height_B", 256);
		AIEnhanceParam.DnetMode_B = ini.GetInteger(section, "DnetMode_B");
		AIEnhanceParam.ModelName_C = ini.GetValue(section, "ModelName_C");
		AIEnhanceParam.LastModelFile_C = ini.GetValue(section, "LastModelFile_C");
		AIEnhanceParam.Width_C = ini.GetInteger(section, "Width_C", 256);
		AIEnhanceParam.Height_C = ini.GetInteger(section, "Height_C", 256);
		AIEnhanceParam.ModelName_D = ini.GetValue(section, "ModelName_D");
		AIEnhanceParam.LastModelFile_D = ini.GetValue(section, "LastModelFile_D");
		AIEnhanceParam.Width_D = ini.GetInteger(section, "Width_D", 256);
		AIEnhanceParam.Height_D = ini.GetInteger(section, "Height_D", 256);
		AIEnhanceParam.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		AIEnhanceParam.FaceIndex = ini.GetInteger(section, "FaceIndex");
		AIEnhanceParam.ParetoIndex_A = ini.GetInteger(section, "ParetoIndex_A");
		AIEnhanceParam.ParetoIndex_B = ini.GetInteger(section, "ParetoIndex_B");
		AIEnhanceParam.UseUNET = ini.GetBoolean(section, "UseUNET", defaultValue: true);
		AIEnhanceParam.TrainingCount = ini.GetInteger(section, "TrainingCount", 100);
		AIEnhanceParam.TotalTrainedCount = ini.GetInteger(section, "TotalTrainedCount");
		AIEnhanceParam.NewModelMode = ini.GetBoolean(section, "NewModelMode");
		AIEnhanceParam.EnableCustomRGBChannel = ini.GetBoolean(section, "EnableCustomRGBChannel");
		AIEnhanceParam.CustomChannelR = ini.GetInteger(section, "CustomChannelR");
		AIEnhanceParam.CustomChannelG = ini.GetInteger(section, "CustomChannelG", 1);
		AIEnhanceParam.CustomChannelB = ini.GetInteger(section, "CustomChannelB", 2);
		AIEnhanceParam.UseTensorRT = ini.GetBoolean(section, "UseTensorRT");
		AIEnhanceParam.NGClone_EnableAutoMark = ini.GetBoolean(section, "NGClone_EnableAutoMark");
		AIEnhanceParam.NGClone_EnableRandom = ini.GetBoolean(section, "NGClone_EnableRandom", defaultValue: true);
		AIEnhanceParam.NGClone_EnableRandomH = ini.GetBoolean(section, "NGClone_EnableRandomH", defaultValue: true);
		AIEnhanceParam.NGClone_EnableRandomV = ini.GetBoolean(section, "NGClone_EnableRandomV", defaultValue: true);
		AIEnhanceParam.NGClone_EnableRandomA = ini.GetBoolean(section, "NGClone_EnableRandomA", defaultValue: true);
		AIEnhanceParam.NGClone_EnableFlipH = ini.GetBoolean(section, "NGClone_EnableFlipH", defaultValue: true);
		AIEnhanceParam.NGClone_EnableFlipV = ini.GetBoolean(section, "NGClone_EnableFlipV");
		AIEnhanceParam.NGClone_AutoMarkDilate = ini.GetInteger(section, "NGClone_AutoMarkDilate", 3);
		AIEnhanceParam.NGClone_ImageMixMode = ini.GetInteger(section, "NGClone_ImageMixMode", 1);
		AIEnhanceParam.NGClone_RotateAngle = ini.GetDouble(section, "NGClone_RotateAngle");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteAIEnhanceParam()
	{
		if (!AIEnhanceParam.ValueChangeFlag)
		{
			return true;
		}
		AIEnhanceParam.ValueChangeFlag = false;
		string section = "AIEnhanceTool";
		ini.WriteString(section, "ModelName_A", AIEnhanceParam.ModelName_A);
		ini.WriteString(section, "LastModelFile_A", AIEnhanceParam.LastModelFile_A);
		ini.WriteBool(section, "EnableAsignGPU", AIEnhanceParam.EnableAsignGPU);
		ini.WriteInt(section, "GPUID", AIEnhanceParam.GPUID);
		ini.WriteInt(section, "Width_A", AIEnhanceParam.Width_A);
		ini.WriteInt(section, "Height_A", AIEnhanceParam.Height_A);
		ini.WriteInt(section, "DnetMode_A", AIEnhanceParam.DnetMode_A);
		ini.WriteInt(section, "DnetROIIndex_A1", AIEnhanceParam.DnetROIIndex_A1);
		ini.WriteInt(section, "DnetROIIndex_A2", AIEnhanceParam.DnetROIIndex_A2);
		ini.WriteInt(section, "DnetROIIndex_A3", AIEnhanceParam.DnetROIIndex_A3);
		ini.WriteInt(section, "DnetROIIndex_A4", AIEnhanceParam.DnetROIIndex_A4);
		ini.WriteInt(section, "DnetROIIndex_A5", AIEnhanceParam.DnetROIIndex_A5);
		ini.WriteInt(section, "DnetROIIndex_A6", AIEnhanceParam.DnetROIIndex_A6);
		ini.WriteInt(section, "DnetROIIndex_A7", AIEnhanceParam.DnetROIIndex_A7);
		ini.WriteInt(section, "DnetROIIndex_A8", AIEnhanceParam.DnetROIIndex_A8);
		ini.WriteInt(section, "DnetROIIndex_B1", AIEnhanceParam.DnetROIIndex_B1);
		ini.WriteInt(section, "DnetROIIndex_B2", AIEnhanceParam.DnetROIIndex_B2);
		ini.WriteInt(section, "DnetROIIndex_B3", AIEnhanceParam.DnetROIIndex_B3);
		ini.WriteInt(section, "DnetROIIndex_B4", AIEnhanceParam.DnetROIIndex_B4);
		ini.WriteInt(section, "DnetROIIndex_B5", AIEnhanceParam.DnetROIIndex_B5);
		ini.WriteInt(section, "DnetROIIndex_B6", AIEnhanceParam.DnetROIIndex_B6);
		ini.WriteInt(section, "DnetROIIndex_B7", AIEnhanceParam.DnetROIIndex_B7);
		ini.WriteInt(section, "DnetROIIndex_B8", AIEnhanceParam.DnetROIIndex_B8);
		ini.WriteString(section, "ModelName_B", AIEnhanceParam.ModelName_B);
		ini.WriteString(section, "LastModelFile_B", AIEnhanceParam.LastModelFile_B);
		ini.WriteInt(section, "Width_B", AIEnhanceParam.Width_B);
		ini.WriteInt(section, "Height_B", AIEnhanceParam.Height_B);
		ini.WriteInt(section, "DnetMode_B", AIEnhanceParam.DnetMode_B);
		ini.WriteString(section, "ModelName_C", AIEnhanceParam.ModelName_C);
		ini.WriteString(section, "LastModelFile_C", AIEnhanceParam.LastModelFile_C);
		ini.WriteInt(section, "Width_C", AIEnhanceParam.Width_C);
		ini.WriteInt(section, "Height_C", AIEnhanceParam.Height_C);
		ini.WriteString(section, "ModelName_D", AIEnhanceParam.ModelName_D);
		ini.WriteString(section, "LastModelFile_D", AIEnhanceParam.LastModelFile_D);
		ini.WriteInt(section, "Width_D", AIEnhanceParam.Width_D);
		ini.WriteInt(section, "Height_D", AIEnhanceParam.Height_D);
		ini.WriteBool(section, "EnableSelectFace", AIEnhanceParam.EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", AIEnhanceParam.FaceIndex);
		ini.WriteInt(section, "ParetoIndex_A", AIEnhanceParam.ParetoIndex_A);
		ini.WriteInt(section, "ParetoIndex_B", AIEnhanceParam.ParetoIndex_B);
		ini.WriteBool(section, "UseUNET", AIEnhanceParam.UseUNET);
		ini.WriteInt(section, "TrainingCount", AIEnhanceParam.TrainingCount);
		ini.WriteInt(section, "TotalTrainedCount", AIEnhanceParam.TotalTrainedCount);
		ini.WriteBool(section, "EnableCustomRGBChannel", AIEnhanceParam.EnableCustomRGBChannel);
		ini.WriteInt(section, "CustomChannelR", AIEnhanceParam.CustomChannelR);
		ini.WriteInt(section, "CustomChannelG", AIEnhanceParam.CustomChannelG);
		ini.WriteInt(section, "CustomChannelB", AIEnhanceParam.CustomChannelB);
		ini.WriteBool(section, "UseTensorRT", AIEnhanceParam.UseTensorRT);
		ini.WriteBool(section, "NGClone_EnableAutoMark", AIEnhanceParam.NGClone_EnableAutoMark);
		ini.WriteBool(section, "NGClone_EnableRandom", AIEnhanceParam.NGClone_EnableRandom);
		ini.WriteBool(section, "NGClone_EnableRandomH", AIEnhanceParam.NGClone_EnableRandomH);
		ini.WriteBool(section, "NGClone_EnableRandomV", AIEnhanceParam.NGClone_EnableRandomV);
		ini.WriteBool(section, "NGClone_EnableRandomA", AIEnhanceParam.NGClone_EnableRandomA);
		ini.WriteBool(section, "NGClone_EnableFlipH", AIEnhanceParam.NGClone_EnableFlipH);
		ini.WriteBool(section, "NGClone_EnableFlipV", AIEnhanceParam.NGClone_EnableFlipV);
		ini.WriteInt(section, "NGClone_AutoMarkDilate", AIEnhanceParam.NGClone_AutoMarkDilate);
		ini.WriteInt(section, "NGClone_ImageMixMode", AIEnhanceParam.NGClone_ImageMixMode);
		ini.WriteDouble(section, "NGClone_RotateAngle", AIEnhanceParam.NGClone_RotateAngle);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadAIPredictParam()
	{
		AIPredictParam.ValueChangeFlag = false;
		string section = "AIPredictTool";
		AIPredictParam.ModelName_A = ini.GetValue(section, "ModelName_A");
		AIPredictParam.LastModelFile_A = ini.GetValue(section, "LastModelFile_A");
		AIPredictParam.GPUID = ini.GetInteger(section, "GPUID");
		AIPredictParam.Width_A = ini.GetInteger(section, "Width_A", 320);
		AIPredictParam.Height_A = ini.GetInteger(section, "Height_A", 240);
		AIPredictParam.DnetMode_A = ini.GetInteger(section, "DnetMode_A");
		AIPredictParam.ModelName_B = ini.GetValue(section, "ModelName_B");
		AIPredictParam.LastModelFile_B = ini.GetValue(section, "LastModelFile_B");
		AIPredictParam.Width_B = ini.GetInteger(section, "Width_B", 320);
		AIPredictParam.Height_B = ini.GetInteger(section, "Height_B", 240);
		AIPredictParam.DnetMode_B = ini.GetInteger(section, "DnetMode_B");
		AIPredictParam.ModelName_C = ini.GetValue(section, "ModelName_C");
		AIPredictParam.LastModelFile_C = ini.GetValue(section, "LastModelFile_C");
		AIPredictParam.Width_C = ini.GetInteger(section, "Width_C", 320);
		AIPredictParam.Height_C = ini.GetInteger(section, "Height_C", 240);
		AIPredictParam.ModelName_D = ini.GetValue(section, "ModelName_D");
		AIPredictParam.LastModelFile_D = ini.GetValue(section, "LastModelFile_D");
		AIPredictParam.Width_D = ini.GetInteger(section, "Width_D", 320);
		AIPredictParam.Height_D = ini.GetInteger(section, "Height_D", 240);
		AIPredictParam.EnableSelectFace = ini.GetBoolean(section, "EnableSelectFace");
		AIPredictParam.FaceIndex = ini.GetInteger(section, "FaceIndex");
		AIPredictParam.ParetoIndex_A = ini.GetInteger(section, "ParetoIndex_A");
		AIPredictParam.ParetoIndex_B = ini.GetInteger(section, "ParetoIndex_B");
		AIPredictParam.UseUNET = ini.GetBoolean(section, "UseUNET", defaultValue: true);
		AIPredictParam.ErrorBin_A0 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_A0", 2);
		AIPredictParam.ErrorBin_A1 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_A1", 1);
		AIPredictParam.ErrorBin_A2 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_A2", 3);
		AIPredictParam.ErrorBin_A3 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_A3", 4);
		AIPredictParam.ErrorBin_B0 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_B0", 2);
		AIPredictParam.ErrorBin_B1 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_B1", 1);
		AIPredictParam.ErrorBin_B2 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_B2", 3);
		AIPredictParam.ErrorBin_B3 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_B3", 4);
		AIPredictParam.ErrorBin_C0 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_C0", 2);
		AIPredictParam.ErrorBin_C1 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_C1", 1);
		AIPredictParam.ErrorBin_C2 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_C2", 3);
		AIPredictParam.ErrorBin_C3 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_C3", 4);
		AIPredictParam.ErrorBin_D0 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_D0", 2);
		AIPredictParam.ErrorBin_D1 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_D1", 1);
		AIPredictParam.ErrorBin_D2 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_D2", 3);
		AIPredictParam.ErrorBin_D3 = (FLOW_BIN)ini.GetInteger(section, "ErrorBin_D3", 4);
		AIPredictParam.UseUNET = ini.GetBoolean(section, "UseUNET", defaultValue: true);
		AIPredictParam.TrainingCount = ini.GetInteger(section, "TrainingCount", 100);
		AIPredictParam.TotalTrainedCount = ini.GetInteger(section, "TotalTrainedCount");
		AIPredictParam.NewModelMode = ini.GetBoolean(section, "NewModelMode");
		AIPredictParam.UseTensorRT = ini.GetBoolean(section, "UseTensorRT", defaultValue: true);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteAIPredictParam()
	{
		if (!AIPredictParam.ValueChangeFlag)
		{
			return true;
		}
		AIPredictParam.ValueChangeFlag = false;
		string section = "AIPredictTool";
		ini.WriteString(section, "ModelName_A", AIPredictParam.ModelName_A);
		ini.WriteString(section, "LastModelFile_A", AIPredictParam.LastModelFile_A);
		ini.WriteInt(section, "GPUID", AIPredictParam.GPUID);
		ini.WriteInt(section, "Width_A", AIPredictParam.Width_A);
		ini.WriteInt(section, "Height_A", AIPredictParam.Height_A);
		ini.WriteInt(section, "DnetMode_A", AIPredictParam.DnetMode_A);
		ini.WriteString(section, "ModelName_B", AIPredictParam.ModelName_B);
		ini.WriteString(section, "LastModelFile_B", AIPredictParam.LastModelFile_B);
		ini.WriteInt(section, "Width_B", AIPredictParam.Width_B);
		ini.WriteInt(section, "Height_B", AIPredictParam.Height_B);
		ini.WriteInt(section, "DnetMode_B", AIPredictParam.DnetMode_B);
		ini.WriteString(section, "ModelName_C", AIPredictParam.ModelName_C);
		ini.WriteString(section, "LastModelFile_C", AIPredictParam.LastModelFile_C);
		ini.WriteInt(section, "Width_C", AIPredictParam.Width_C);
		ini.WriteInt(section, "Height_C", AIPredictParam.Height_C);
		ini.WriteString(section, "ModelName_D", AIPredictParam.ModelName_D);
		ini.WriteString(section, "LastModelFile_D", AIPredictParam.LastModelFile_D);
		ini.WriteInt(section, "Width_D", AIPredictParam.Width_D);
		ini.WriteInt(section, "Height_D", AIPredictParam.Height_D);
		ini.WriteBool(section, "EnableSelectFace", AIPredictParam.EnableSelectFace);
		ini.WriteInt(section, "FaceIndex", AIPredictParam.FaceIndex);
		ini.WriteInt(section, "ParetoIndex_A", AIPredictParam.ParetoIndex_A);
		ini.WriteInt(section, "ParetoIndex_B", AIPredictParam.ParetoIndex_B);
		ini.WriteBool(section, "UseUNET", AIPredictParam.UseUNET);
		ini.WriteInt(section, "ErrorBin_A0", (int)AIPredictParam.ErrorBin_A0);
		ini.WriteInt(section, "ErrorBin_A1", (int)AIPredictParam.ErrorBin_A1);
		ini.WriteInt(section, "ErrorBin_A2", (int)AIPredictParam.ErrorBin_A2);
		ini.WriteInt(section, "ErrorBin_A3", (int)AIPredictParam.ErrorBin_A3);
		ini.WriteInt(section, "ErrorBin_B0", (int)AIPredictParam.ErrorBin_B0);
		ini.WriteInt(section, "ErrorBin_B1", (int)AIPredictParam.ErrorBin_B1);
		ini.WriteInt(section, "ErrorBin_B2", (int)AIPredictParam.ErrorBin_B2);
		ini.WriteInt(section, "ErrorBin_B3", (int)AIPredictParam.ErrorBin_B3);
		ini.WriteInt(section, "ErrorBin_C0", (int)AIPredictParam.ErrorBin_C0);
		ini.WriteInt(section, "ErrorBin_C1", (int)AIPredictParam.ErrorBin_C1);
		ini.WriteInt(section, "ErrorBin_C2", (int)AIPredictParam.ErrorBin_C2);
		ini.WriteInt(section, "ErrorBin_C3", (int)AIPredictParam.ErrorBin_C3);
		ini.WriteInt(section, "ErrorBin_D0", (int)AIPredictParam.ErrorBin_D0);
		ini.WriteInt(section, "ErrorBin_D1", (int)AIPredictParam.ErrorBin_D1);
		ini.WriteInt(section, "ErrorBin_D2", (int)AIPredictParam.ErrorBin_D2);
		ini.WriteInt(section, "ErrorBin_D3", (int)AIPredictParam.ErrorBin_D3);
		ini.WriteBool(section, "UseUNET", AIPredictParam.UseUNET);
		ini.WriteInt(section, "TrainingCount", AIPredictParam.TrainingCount);
		ini.WriteInt(section, "TotalTrainedCount", AIPredictParam.TotalTrainedCount);
		ini.WriteBool(section, "UseTensorRT", AIPredictParam.UseTensorRT);
		return true;
	}

	public List<TOOLSENABLEITEM> GetEnabledItem()
	{
		StationToolEnableItem.Clear();
		TOOLSENABLEITEM item = default(TOOLSENABLEITEM);
		for (int i = 0; i < 20; i++)
		{
			item.Name = AdvLocateParam[i].TrackerName;
			item.Enabled = AdvLocateParam[i].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int j = 0; j < 40; j++)
		{
			item.Name = AdvLocateMParam[j].TrackerName;
			item.Enabled = AdvLocateMParam[j].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int k = 0; k < 20; k++)
		{
			item.Name = MeaWidthParam[k].TrackerName;
			item.Enabled = MeaWidthParam[k].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int l = 0; l < 20; l++)
		{
			item.Name = MeaHeightParam[l].TrackerName;
			item.Enabled = MeaHeightParam[l].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int m = 0; m < 30; m++)
		{
			item.Name = BinaryParam[m].TrackerName;
			item.Enabled = BinaryParam[m].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int n = 0; n < 20; n++)
		{
			item.Name = SmoothParam[n].TrackerName;
			item.Enabled = SmoothParam[n].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int num = 0; num < 20; num++)
		{
			item.Name = BlockAvgParam[num].TrackerName;
			item.Enabled = BlockAvgParam[num].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int num2 = 0; num2 < 20; num2++)
		{
			item.Name = DifferenceParam[num2].TrackerName;
			item.Enabled = DifferenceParam[num2].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int num3 = 0; num3 < 20; num3++)
		{
			item.Name = LineAnalysisParam[num3].TrackerName;
			item.Enabled = LineAnalysisParam[num3].TrackerON;
			StationToolEnableItem.Add(item);
		}
		for (int num4 = 0; num4 < 20; num4++)
		{
			item.Name = MatchParam[num4].TrackerName;
			item.Enabled = MatchParam[num4].TrackerON;
			StationToolEnableItem.Add(item);
		}
		return StationToolEnableItem;
	}
}
