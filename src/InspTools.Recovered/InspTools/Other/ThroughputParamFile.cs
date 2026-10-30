using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using InspTools.AOI;
using InspTools.Station;

namespace InspTools.Other;

public class ThroughputParamFile
{
	private HiniFile ini;

	public INSP_THROUGHPUT_PARAM param = new INSP_THROUGHPUT_PARAM();

	public double[] ComDigits = new double[24];

	public string[] ComDigitsName = new string[24];

	private bool EnableMultiParetoMode;

	private bool EnableThroughputWindow;

	public int CurrentClient;

	public int OpenCVThreadNum;

	public bool EnableSetOpenCVThreadNum;

	public bool CpuUseOptimized = true;

	public bool EnableAssignGPU = true;

	private int LanguageType = 1;

	private CVInspection[] inspection;

	public bool MainValueChanged;

	public List<INSPSURVITEM> IsItems = new List<INSPSURVITEM>
	{
		new INSPSURVITEM
		{
			ID = 0,
			Station = 0,
			InspToolID = 0,
			InspToolName = "",
			TrackID = 0,
			Status = true,
			StatusStr = "",
			TrackName = ""
		}
	};

	public void SetINI(HiniFile Ini)
	{
		ini = Ini;
		EnableMultiParetoMode = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadMainParam()
	{
		string text = AppDomain.CurrentDomain.BaseDirectory + "DeviceSetting\\Setting.ini";
		if (!File.Exists(text))
		{
			return false;
		}
		HiniFile hiniFile = new HiniFile(text, ";");
		CurrentClient = (param.CurrentClient = hiniFile.GetInteger("Main", "CurrentClient"));
		EnableThroughputWindow = (param.OpenThroughputWindow = hiniFile.GetBoolean("Customization", "OpenThroughputWindow"));
		param.EnableAIPasswordProtect = hiniFile.GetBoolean("Customization", "EnableAIPasswordProtect");
		param.AIPassword = hiniFile.GetValue("Customization", "AIPassword", "1111");
		OpenCVThreadNum = (param.OpenCVThreadNum = hiniFile.GetInteger("Main", "OpenCVThreadNum"));
		EnableSetOpenCVThreadNum = (param.EnableSetOpenCVThreadNum = hiniFile.GetBoolean("Main", "EnableSetOpenCVThreadNum"));
		CpuUseOptimized = (param.CpuUseOptimized = hiniFile.GetBoolean("Main", "CpuUseOptimized", defaultValue: true));
		MainValueChanged = false;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteMainParam()
	{
		if (!MainValueChanged)
		{
			return true;
		}
		string text = AppDomain.CurrentDomain.BaseDirectory + "DeviceSetting\\Setting.ini";
		if (!File.Exists(text))
		{
			return false;
		}
		HiniFile hiniFile = new HiniFile(text, ";");
		param.OpenThroughputWindow = EnableThroughputWindow;
		hiniFile.WriteBool("Customization", "OpenThroughputWindow", param.OpenThroughputWindow);
		MainValueChanged = false;
		return true;
	}

	public void SetInspection(CVInspection[] station)
	{
		inspection = station;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadThroughputParam()
	{
		if (ini == null)
		{
			return false;
		}
		param.OutputPath = ini.GetValue("Throughput", "OutputPath");
		param.YieldRetestPath = ini.GetValue("Throughput", "YieldRetestPath");
		param.OutputFormat = ini.GetInteger("Throughput", "OutputFormat");
		param.OutputImageCount = ini.GetInteger("Throughput", "OutputImageCount", 4);
		param.OutputALLOKNG = ini.GetInteger("Throughput", "OutputALLOKNG");
		param.Station1ON = ini.GetBoolean("Throughput", "Station1ON", defaultValue: true);
		param.Station2ON = ini.GetBoolean("Throughput", "Station2ON", defaultValue: true);
		param.Station3ON = ini.GetBoolean("Throughput", "Station3ON", defaultValue: true);
		param.Station4ON = ini.GetBoolean("Throughput", "Station4ON", defaultValue: true);
		param.Station5ON = ini.GetBoolean("Throughput", "Station5ON", defaultValue: true);
		param.Station6ON = ini.GetBoolean("Throughput", "Station6ON", defaultValue: true);
		param.Station7ON = ini.GetBoolean("Throughput", "Station7ON");
		param.Station8ON = ini.GetBoolean("Throughput", "Station8ON");
		param.TrackerAColorIndex = ini.GetInteger("UI", "TrackerAColorIndex");
		param.TrackerBColorIndex = ini.GetInteger("UI", "TrackerBColorIndex", 1);
		param.TrackerAThickness = ini.GetInteger("UI", "TrackerAThickness", 2);
		param.TrackerBThickness = ini.GetInteger("UI", "TrackerBThickness", 2);
		param.HideTrainingWnd = ini.GetBoolean("AITraining", "HideTrainingWnd", defaultValue: true);
		param.EnableTrainingBufferTest = ini.GetBoolean("AITraining", "EnableTrainingBufferTest", defaultValue: true);
		param.TrainingBufferIndex = ini.GetInteger("AITraining", "TrainingBufferIndex");
		param.SaveImgUseDateForlder = ini.GetBoolean("AITraining", "SaveImgUseDateForlder", defaultValue: true);
		param.EnableMoveToTrainingImg = ini.GetBoolean("AITraining", "EnableMoveToTrainingImg");
		param.EnableAutoUpdateModel = ini.GetBoolean("AITraining", "EnableAutoUpdateModel");
		for (int i = 0; i < 8; i++)
		{
			string key = string.Format("EnableStationTrain{0}", i + 1);
			param.EnableStationTrain[i] = ini.GetBoolean("AITraining", key, defaultValue: true);
			key = string.Format("TrainingCount{0}", i + 1);
			param.TrainingCount[i] = ini.GetInteger("AITraining", key, 100);
			key = string.Format("TrainingImageDoubleRate{0}", i + 1);
			param.TrainingImageDoubleRate[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_horizontalFlip{0}", i + 1);
			param.Training_horizontalFlip[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_verticalFlip{0}", i + 1);
			param.Training_verticalFlip[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_brightness_limit{0}", i + 1);
			param.Training_brightness_limit[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_contrast_limit{0}", i + 1);
			param.Training_contrast_limit[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_scale_limit{0}", i + 1);
			param.Training_scale_limit[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_rotate_limit{0}", i + 1);
			param.Training_rotate_limit[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_shift_limit{0}", i + 1);
			param.Training_shift_limit[i] = ini.GetDouble("AITraining", key);
			key = string.Format("Training_hue_shift_limit{0}", i + 1);
			param.Training_hue_shift_limit[i] = ini.GetInteger("AITraining", key);
			key = string.Format("Training_sat_shift_limit{0}", i + 1);
			param.Training_sat_shift_limit[i] = ini.GetInteger("AITraining", key);
			key = string.Format("Training_val_shift_limit{0}", i + 1);
			param.Training_val_shift_limit[i] = ini.GetInteger("AITraining", key);
			key = string.Format("Training_r_shift_limit{0}", i + 1);
			param.Training_r_shift_limit[i] = ini.GetInteger("AITraining", key);
			key = string.Format("Training_g_shift_limit{0}", i + 1);
			param.Training_g_shift_limit[i] = ini.GetInteger("AITraining", key);
			key = string.Format("Training_b_shift_limit{0}", i + 1);
			param.Training_b_shift_limit[i] = ini.GetInteger("AITraining", key);
			key = string.Format("BatchSize{0}", i + 1);
			param.BatchSize[i] = ini.GetInteger("AITraining", key, 8);
			key = string.Format("DnetDiv{0}", i + 1);
			param.DnetDiv[i] = ini.GetInteger("AITraining", key, 8);
			key = string.Format("HashTagStr{0}", i + 1);
			param.HashTagStr[i] = ini.GetValue("AITraining", key);
		}
		param.CustomRatio = ini.GetDouble("Throughput", "CustomRatio", 1.0);
		param.EnableCustomRatio = ini.GetBoolean("Throughput", "EnableCustomRatio");
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteThroughputParam()
	{
		if (ini == null)
		{
			return false;
		}
		ini.WriteString("Throughput", "OutputPath", param.OutputPath);
		ini.WriteString("Throughput", "YieldRetestPath", param.YieldRetestPath);
		ini.WriteInt("Throughput", "OutputFormat", param.OutputFormat);
		ini.WriteInt("Throughput", "OutputImageCount", param.OutputImageCount);
		ini.WriteInt("Throughput", "OutputALLOKNG", param.OutputALLOKNG);
		ini.WriteBool("Throughput", "Station1ON", param.Station1ON);
		ini.WriteBool("Throughput", "Station2ON", param.Station2ON);
		ini.WriteBool("Throughput", "Station3ON", param.Station3ON);
		ini.WriteBool("Throughput", "Station4ON", param.Station4ON);
		ini.WriteBool("Throughput", "Station5ON", param.Station5ON);
		ini.WriteBool("Throughput", "Station6ON", param.Station6ON);
		ini.WriteBool("Throughput", "Station7ON", param.Station7ON);
		ini.WriteBool("Throughput", "Station8ON", param.Station8ON);
		ini.WriteBool("AITraining", "HideTrainingWnd", param.HideTrainingWnd);
		ini.WriteBool("AITraining", "EnableTrainingBufferTest", param.EnableTrainingBufferTest);
		ini.WriteInt("AITraining", "TrainingBufferIndex", param.TrainingBufferIndex);
		ini.WriteBool("AITraining", "SaveImgUseDateForlder", param.SaveImgUseDateForlder);
		ini.WriteBool("AITraining", "EnableMoveToTrainingImg", param.EnableMoveToTrainingImg);
		ini.WriteBool("AITraining", "EnableAutoUpdateModel", param.EnableAutoUpdateModel);
		ini.WriteInt("UI", "TrackerAColorIndex", param.TrackerAColorIndex);
		ini.WriteInt("UI", "TrackerBColorIndex", param.TrackerBColorIndex);
		ini.WriteInt("UI", "TrackerAThickness", param.TrackerAThickness);
		ini.WriteInt("UI", "TrackerBThickness", param.TrackerBThickness);
		for (int i = 0; i < 8; i++)
		{
			string key = string.Format("EnableStationTrain{0}", i + 1);
			ini.WriteBool("AITraining", key, param.EnableStationTrain[i]);
			key = string.Format("TrainingCount{0}", i + 1);
			ini.WriteInt("AITraining", key, param.TrainingCount[i]);
			key = string.Format("TrainingImageDoubleRate{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.TrainingImageDoubleRate[i]);
			key = string.Format("Training_horizontalFlip{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_horizontalFlip[i]);
			key = string.Format("Training_verticalFlip{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_verticalFlip[i]);
			key = string.Format("Training_brightness_limit{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_brightness_limit[i]);
			key = string.Format("Training_contrast_limit{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_contrast_limit[i]);
			key = string.Format("Training_scale_limit{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_scale_limit[i]);
			key = string.Format("Training_rotate_limit{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_rotate_limit[i]);
			key = string.Format("Training_shift_limit{0}", i + 1);
			ini.WriteDouble("AITraining", key, param.Training_shift_limit[i]);
			key = string.Format("Training_hue_shift_limit{0}", i + 1);
			ini.WriteInt("AITraining", key, param.Training_hue_shift_limit[i]);
			key = string.Format("Training_sat_shift_limit{0}", i + 1);
			ini.WriteInt("AITraining", key, param.Training_sat_shift_limit[i]);
			key = string.Format("Training_val_shift_limit{0}", i + 1);
			ini.WriteInt("AITraining", key, param.Training_val_shift_limit[i]);
			key = string.Format("Training_r_shift_limit{0}", i + 1);
			ini.WriteInt("AITraining", key, param.Training_r_shift_limit[i]);
			key = string.Format("Training_g_shift_limit{0}", i + 1);
			ini.WriteInt("AITraining", key, param.Training_g_shift_limit[i]);
			key = string.Format("Training_b_shift_limit{0}", i + 1);
			ini.WriteInt("AITraining", key, param.Training_b_shift_limit[i]);
			key = string.Format("BatchSize{0}", i + 1);
			ini.WriteInt("AITraining", key, param.BatchSize[i]);
			key = string.Format("DnetDiv{0}", i + 1);
			ini.WriteInt("AITraining", key, param.DnetDiv[i]);
			key = string.Format("HashTagStr{0}", i + 1);
			ini.WriteString("AITraining", key, param.HashTagStr[i]);
		}
		ini.WriteDouble("Throughput", "CustomRatio", param.CustomRatio);
		ini.WriteBool("Throughput", "EnableCustomRatio", param.EnableCustomRatio);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadSizeRecordSetupParam()
	{
		if (ini == null)
		{
			return false;
		}
		param.LSizeEnable = ini.GetBoolean("Throughput", "LSizeEnable", defaultValue: true);
		param.LSizeStation = ini.GetInteger("Throughput", "LSizeStation");
		param.LSizeToolSelect = ini.GetInteger("Throughput", "LSizeToolSelect");
		param.LSizeSubSelect = ini.GetInteger("Throughput", "LSizeSubSelect");
		param.LSizeItemSelect = ini.GetInteger("Throughput", "LSizeItemSelect");
		param.WSizeEnable = ini.GetBoolean("Throughput", "WSizeEnable", defaultValue: true);
		param.WSizeStation = ini.GetInteger("Throughput", "WSizeStation");
		param.WSizeToolSelect = ini.GetInteger("Throughput", "WSizeToolSelect");
		param.WSizeSubSelect = ini.GetInteger("Throughput", "WSizeSubSelect");
		param.WSizeItemSelect = ini.GetInteger("Throughput", "WSizeItemSelect");
		param.TSizeEnable = ini.GetBoolean("Throughput", "TSizeEnable", defaultValue: true);
		param.TSizeStation = ini.GetInteger("Throughput", "TSizeStation");
		param.TSizeToolSelect = ini.GetInteger("Throughput", "TSizeToolSelect");
		param.TSizeSubSelect = ini.GetInteger("Throughput", "TSizeSubSelect");
		param.TSizeItemSelect = ini.GetInteger("Throughput", "TSizeItemSelect");
		param.LPadSizeEnable = ini.GetBoolean("Throughput", "LPadSizeEnable", defaultValue: true);
		param.LPadSizeStation = ini.GetInteger("Throughput", "LPadSizeStation");
		param.LPadSizeToolSelect = ini.GetInteger("Throughput", "LPadSizeToolSelect");
		param.LPadSizeSubSelect = ini.GetInteger("Throughput", "LPadSizeSubSelect");
		param.LPadSizeItemSelect = ini.GetInteger("Throughput", "LPadSizeItemSelect");
		param.RPadSizeEnable = ini.GetBoolean("Throughput", "RPadSizeEnable", defaultValue: true);
		param.RPadSizeStation = ini.GetInteger("Throughput", "RPadSizeStation");
		param.RPadSizeToolSelect = ini.GetInteger("Throughput", "RPadSizeToolSelect");
		param.RPadSizeSubSelect = ini.GetInteger("Throughput", "RPadSizeSubSelect");
		param.RPadSizeItemSelect = ini.GetInteger("Throughput", "RPadSizeItemSelect");
		param.GapSizeEnable = ini.GetBoolean("Throughput", "GapSizeEnable", defaultValue: true);
		param.GapSizeStation = ini.GetInteger("Throughput", "GapSizeStation");
		param.GapSizeToolSelect = ini.GetInteger("Throughput", "GapSizeToolSelect");
		param.GapSizeSubSelect = ini.GetInteger("Throughput", "GapSizeSubSelect");
		param.GapSizeItemSelect = ini.GetInteger("Throughput", "GapSizeItemSelect");
		param.LPadStr = ini.GetValue("Throughput", "LPadStr", "左電極寬度");
		param.RPadStr = ini.GetValue("Throughput", "RPadStr", "右電極寬度");
		param.GapStr = ini.GetValue("Throughput", "GapStr", "Gap");
		param.C1Enable = ini.GetBoolean("Throughput", "C1Enable", defaultValue: true);
		param.C1Str = ini.GetValue("Throughput", "C1Str", "Custom1");
		param.C1SizeStation = ini.GetInteger("Throughput", "C1SizeStation");
		param.C1SizeToolSelect = ini.GetInteger("Throughput", "C1SizeToolSelect");
		param.C1SizeSubSelect = ini.GetInteger("Throughput", "C1SizeSubSelect");
		param.C1SizeItemSelect = ini.GetInteger("Throughput", "C1SizeItemSelect");
		param.C2Enable = ini.GetBoolean("Throughput", "C2Enable", defaultValue: true);
		param.C2Str = ini.GetValue("Throughput", "C2Str", "Custom2");
		param.C2SizeStation = ini.GetInteger("Throughput", "C2SizeStation");
		param.C2SizeToolSelect = ini.GetInteger("Throughput", "C2SizeToolSelect");
		param.C2SizeSubSelect = ini.GetInteger("Throughput", "C2SizeSubSelect");
		param.C2SizeItemSelect = ini.GetInteger("Throughput", "C2SizeItemSelect");
		param.C3Enable = ini.GetBoolean("Throughput", "C3Enable", defaultValue: true);
		param.C3Str = ini.GetValue("Throughput", "C3Str", "Custom3");
		param.C3SizeStation = ini.GetInteger("Throughput", "C3SizeStation");
		param.C3SizeToolSelect = ini.GetInteger("Throughput", "C3SizeToolSelect");
		param.C3SizeSubSelect = ini.GetInteger("Throughput", "C3SizeSubSelect");
		param.C3SizeItemSelect = ini.GetInteger("Throughput", "C3SizeItemSelect");
		param.C4Enable = ini.GetBoolean("Throughput", "C4Enable", defaultValue: true);
		param.C4Str = ini.GetValue("Throughput", "C4Str", "Custom4");
		param.C4SizeStation = ini.GetInteger("Throughput", "C4SizeStation");
		param.C4SizeToolSelect = ini.GetInteger("Throughput", "C4SizeToolSelect");
		param.C4SizeSubSelect = ini.GetInteger("Throughput", "C4SizeSubSelect");
		param.C4SizeItemSelect = ini.GetInteger("Throughput", "C4SizeItemSelect");
		param.C5Enable = ini.GetBoolean("Throughput", "C5Enable", defaultValue: true);
		param.C5Str = ini.GetValue("Throughput", "C5Str", "Custom5");
		param.C5SizeStation = ini.GetInteger("Throughput", "C5SizeStation");
		param.C5SizeToolSelect = ini.GetInteger("Throughput", "C5SizeToolSelect");
		param.C5SizeSubSelect = ini.GetInteger("Throughput", "C5SizeSubSelect");
		param.C5SizeItemSelect = ini.GetInteger("Throughput", "C5SizeItemSelect");
		param.C6Enable = ini.GetBoolean("Throughput", "C6Enable", defaultValue: true);
		param.C6Str = ini.GetValue("Throughput", "C6Str", "Custom6");
		param.C6SizeStation = ini.GetInteger("Throughput", "C6SizeStation");
		param.C6SizeToolSelect = ini.GetInteger("Throughput", "C6SizeToolSelect");
		param.C6SizeSubSelect = ini.GetInteger("Throughput", "C6SizeSubSelect");
		param.C6SizeItemSelect = ini.GetInteger("Throughput", "C6SizeItemSelect");
		param.C7Enable = ini.GetBoolean("Throughput", "C7Enable", defaultValue: true);
		param.C7Str = ini.GetValue("Throughput", "C7Str", "Custom7");
		param.C7SizeStation = ini.GetInteger("Throughput", "C7SizeStation");
		param.C7SizeToolSelect = ini.GetInteger("Throughput", "C7SizeToolSelect");
		param.C7SizeSubSelect = ini.GetInteger("Throughput", "C7SizeSubSelect");
		param.C7SizeItemSelect = ini.GetInteger("Throughput", "C7SizeItemSelect");
		param.C8Enable = ini.GetBoolean("Throughput", "C8Enable", defaultValue: true);
		param.C8Str = ini.GetValue("Throughput", "C8Str", "Custom8");
		param.C8SizeStation = ini.GetInteger("Throughput", "C8SizeStation");
		param.C8SizeToolSelect = ini.GetInteger("Throughput", "C8SizeToolSelect");
		param.C8SizeSubSelect = ini.GetInteger("Throughput", "C8SizeSubSelect");
		param.C8SizeItemSelect = ini.GetInteger("Throughput", "C8SizeItemSelect");
		param.C9Enable = ini.GetBoolean("Throughput", "C9Enable", defaultValue: true);
		param.C9Str = ini.GetValue("Throughput", "C9Str", "Custom9");
		param.C9SizeStation = ini.GetInteger("Throughput", "C9SizeStation");
		param.C9SizeToolSelect = ini.GetInteger("Throughput", "C9SizeToolSelect");
		param.C9SizeSubSelect = ini.GetInteger("Throughput", "C9SizeSubSelect");
		param.C9SizeItemSelect = ini.GetInteger("Throughput", "C9SizeItemSelect");
		param.C10Enable = ini.GetBoolean("Throughput", "C10Enable", defaultValue: true);
		param.C10Str = ini.GetValue("Throughput", "C10Str", "Custom10");
		param.C10SizeStation = ini.GetInteger("Throughput", "C10SizeStation");
		param.C10SizeToolSelect = ini.GetInteger("Throughput", "C10SizeToolSelect");
		param.C10SizeSubSelect = ini.GetInteger("Throughput", "C10SizeSubSelect");
		param.C10SizeItemSelect = ini.GetInteger("Throughput", "C10SizeItemSelect");
		param.C11Enable = ini.GetBoolean("Throughput", "C11Enable", defaultValue: true);
		param.C11Str = ini.GetValue("Throughput", "C11Str", "Custom11");
		param.C11SizeStation = ini.GetInteger("Throughput", "C11SizeStation");
		param.C11SizeToolSelect = ini.GetInteger("Throughput", "C11SizeToolSelect");
		param.C11SizeSubSelect = ini.GetInteger("Throughput", "C11SizeSubSelect");
		param.C11SizeItemSelect = ini.GetInteger("Throughput", "C11SizeItemSelect");
		param.C12Enable = ini.GetBoolean("Throughput", "C12Enable", defaultValue: true);
		param.C12Str = ini.GetValue("Throughput", "C12Str", "Custom12");
		param.C12SizeStation = ini.GetInteger("Throughput", "C12SizeStation");
		param.C12SizeToolSelect = ini.GetInteger("Throughput", "C12SizeToolSelect");
		param.C12SizeSubSelect = ini.GetInteger("Throughput", "C12SizeSubSelect");
		param.C12SizeItemSelect = ini.GetInteger("Throughput", "C12SizeItemSelect");
		param.C13Enable = ini.GetBoolean("Throughput", "C13Enable", defaultValue: true);
		param.C13Str = ini.GetValue("Throughput", "C13Str", "Custom13");
		param.C13SizeStation = ini.GetInteger("Throughput", "C13SizeStation");
		param.C13SizeToolSelect = ini.GetInteger("Throughput", "C13SizeToolSelect");
		param.C13SizeSubSelect = ini.GetInteger("Throughput", "C13SizeSubSelect");
		param.C13SizeItemSelect = ini.GetInteger("Throughput", "C13SizeItemSelect");
		param.C14Enable = ini.GetBoolean("Throughput", "C14Enable", defaultValue: true);
		param.C14Str = ini.GetValue("Throughput", "C14Str", "Custom14");
		param.C14SizeStation = ini.GetInteger("Throughput", "C14SizeStation");
		param.C14SizeToolSelect = ini.GetInteger("Throughput", "C14SizeToolSelect");
		param.C14SizeSubSelect = ini.GetInteger("Throughput", "C14SizeSubSelect");
		param.C14SizeItemSelect = ini.GetInteger("Throughput", "C14SizeItemSelect");
		param.ComDigits0 = ini.GetDouble("Throughput", "ComDigits0");
		param.ComDigits1 = ini.GetDouble("Throughput", "ComDigits1");
		param.ComDigits2 = ini.GetDouble("Throughput", "ComDigits2");
		param.ComDigits3 = ini.GetDouble("Throughput", "ComDigits3");
		param.ComDigits4 = ini.GetDouble("Throughput", "ComDigits4");
		param.ComDigits5 = ini.GetDouble("Throughput", "ComDigits5");
		param.ComDigits6 = ini.GetDouble("Throughput", "ComDigits6");
		param.ComDigits7 = ini.GetDouble("Throughput", "ComDigits7");
		param.ComDigits8 = ini.GetDouble("Throughput", "ComDigits8");
		param.ComDigits9 = ini.GetDouble("Throughput", "ComDigits9");
		param.ComDigits10 = ini.GetDouble("Throughput", "ComDigits10");
		param.ComDigits11 = ini.GetDouble("Throughput", "ComDigits11");
		param.ComDigits12 = ini.GetDouble("Throughput", "ComDigits12");
		param.ComDigits13 = ini.GetDouble("Throughput", "ComDigits13");
		param.ComDigits14 = ini.GetDouble("Throughput", "ComDigits14");
		param.ComDigits15 = ini.GetDouble("Throughput", "ComDigits15");
		param.ComDigits16 = ini.GetDouble("Throughput", "ComDigits16");
		param.ComDigits17 = ini.GetDouble("Throughput", "ComDigits17");
		param.ComDigits18 = ini.GetDouble("Throughput", "ComDigits18");
		param.ComDigits19 = ini.GetDouble("Throughput", "ComDigits19");
		param.ComDigits20 = ini.GetDouble("Throughput", "ComDigits20");
		param.ComDigits21 = ini.GetDouble("Throughput", "ComDigits21");
		param.ComDigits22 = ini.GetDouble("Throughput", "ComDigits22");
		param.ComDigits23 = ini.GetDouble("Throughput", "ComDigits23");
		param.ComDigitsName0 = ini.GetValue("Throughput", "ComDigitsName0", "Min Length");
		param.ComDigitsName1 = ini.GetValue("Throughput", "ComDigitsName1", "Max Length");
		param.ComDigitsName2 = ini.GetValue("Throughput", "ComDigitsName2", "Min Width");
		param.ComDigitsName3 = ini.GetValue("Throughput", "ComDigitsName3", "Max Width");
		param.ComDigitsName4 = ini.GetValue("Throughput", "ComDigitsName4", "Min Thickness");
		param.ComDigitsName5 = ini.GetValue("Throughput", "ComDigitsName5", "Max Thickness");
		param.ComDigitsName6 = ini.GetValue("Throughput", "ComDigitsName6", "Min Pad Width");
		param.ComDigitsName7 = ini.GetValue("Throughput", "ComDigitsName7", "Max Pad Width");
		param.ComDigitsName8 = ini.GetValue("Throughput", "ComDigitsName8", "Custom 1");
		param.ComDigitsName9 = ini.GetValue("Throughput", "ComDigitsName9", "Custom 2");
		param.ComDigitsName10 = ini.GetValue("Throughput", "ComDigitsName10", "Custom 3");
		param.ComDigitsName11 = ini.GetValue("Throughput", "ComDigitsName11", "Custom 4");
		param.ComDigitsName12 = ini.GetValue("Throughput", "ComDigitsName12", "Custom 5");
		param.ComDigitsName13 = ini.GetValue("Throughput", "ComDigitsName13", "Custom 6");
		param.ComDigitsName14 = ini.GetValue("Throughput", "ComDigitsName14", "Custom 7");
		param.ComDigitsName15 = ini.GetValue("Throughput", "ComDigitsName15", "Custom 8");
		param.ComDigitsName16 = ini.GetValue("Throughput", "ComDigitsName16", "Custom 9");
		param.ComDigitsName17 = ini.GetValue("Throughput", "ComDigitsName17", "Custom 10");
		param.ComDigitsName18 = ini.GetValue("Throughput", "ComDigitsName18", "Custom 11");
		param.ComDigitsName19 = ini.GetValue("Throughput", "ComDigitsName19", "Custom 12");
		param.ComDigitsName20 = ini.GetValue("Throughput", "ComDigitsName20", "Custom 13");
		param.ComDigitsName21 = ini.GetValue("Throughput", "ComDigitsName21", "Custom 14");
		param.ComDigitsName22 = ini.GetValue("Throughput", "ComDigitsName22", "Custom 15");
		param.ComDigitsName23 = ini.GetValue("Throughput", "ComDigitsName23", "Custom 16");
		param.BinReverseMode = ini.GetBoolean("Other", "BinReverseMode");
		UpdateComDigis();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteSizeRecordSetupParam()
	{
		if (ini == null)
		{
			return false;
		}
		ini.WriteBool("Throughput", "LSizeEnable", param.LSizeEnable);
		ini.WriteInt("Throughput", "LSizeStation", param.LSizeStation);
		ini.WriteInt("Throughput", "LSizeToolSelect", param.LSizeToolSelect);
		ini.WriteInt("Throughput", "LSizeSubSelect", param.LSizeSubSelect);
		ini.WriteInt("Throughput", "LSizeItemSelect", param.LSizeItemSelect);
		ini.WriteBool("Throughput", "WSizeEnable", param.WSizeEnable);
		ini.WriteInt("Throughput", "WSizeStation", param.WSizeStation);
		ini.WriteInt("Throughput", "WSizeToolSelect", param.WSizeToolSelect);
		ini.WriteInt("Throughput", "WSizeSubSelect", param.WSizeSubSelect);
		ini.WriteInt("Throughput", "WSizeItemSelect", param.WSizeItemSelect);
		ini.WriteBool("Throughput", "TSizeEnable", param.TSizeEnable);
		ini.WriteInt("Throughput", "TSizeStation", param.TSizeStation);
		ini.WriteInt("Throughput", "TSizeToolSelect", param.TSizeToolSelect);
		ini.WriteInt("Throughput", "TSizeSubSelect", param.TSizeSubSelect);
		ini.WriteInt("Throughput", "TSizeItemSelect", param.TSizeItemSelect);
		ini.WriteBool("Throughput", "LPadSizeEnable", param.LPadSizeEnable);
		ini.WriteInt("Throughput", "LPadSizeStation", param.LPadSizeStation);
		ini.WriteInt("Throughput", "LPadSizeToolSelect", param.LPadSizeToolSelect);
		ini.WriteInt("Throughput", "LPadSizeSubSelect", param.LPadSizeSubSelect);
		ini.WriteInt("Throughput", "LPadSizeItemSelect", param.LPadSizeItemSelect);
		ini.WriteBool("Throughput", "RPadSizeEnable", param.RPadSizeEnable);
		ini.WriteInt("Throughput", "RPadSizeStation", param.RPadSizeStation);
		ini.WriteInt("Throughput", "RPadSizeToolSelect", param.RPadSizeToolSelect);
		ini.WriteInt("Throughput", "RPadSizeSubSelect", param.RPadSizeSubSelect);
		ini.WriteInt("Throughput", "RPadSizeItemSelect", param.RPadSizeItemSelect);
		ini.WriteBool("Throughput", "GapSizeEnable", param.GapSizeEnable);
		ini.WriteInt("Throughput", "GapSizeStation", param.GapSizeStation);
		ini.WriteInt("Throughput", "GapSizeToolSelect", param.GapSizeToolSelect);
		ini.WriteInt("Throughput", "GapSizeSubSelect", param.GapSizeSubSelect);
		ini.WriteInt("Throughput", "GapSizeItemSelect", param.GapSizeItemSelect);
		ini.WriteString("Throughput", "LPadStr", param.LPadStr);
		ini.WriteString("Throughput", "RPadStr", param.RPadStr);
		ini.WriteString("Throughput", "GapStr", param.GapStr);
		ini.WriteBool("Throughput", "C1Enable", param.C1Enable);
		ini.WriteString("Throughput", "C1Str", param.C1Str);
		ini.WriteInt("Throughput", "C1SizeStation", param.C1SizeStation);
		ini.WriteInt("Throughput", "C1SizeToolSelect", param.C1SizeToolSelect);
		ini.WriteInt("Throughput", "C1SizeSubSelect", param.C1SizeSubSelect);
		ini.WriteInt("Throughput", "C1SizeItemSelect", param.C1SizeItemSelect);
		ini.WriteBool("Throughput", "C2Enable", param.C2Enable);
		ini.WriteString("Throughput", "C2Str", param.C2Str);
		ini.WriteInt("Throughput", "C2SizeStation", param.C2SizeStation);
		ini.WriteInt("Throughput", "C2SizeToolSelect", param.C2SizeToolSelect);
		ini.WriteInt("Throughput", "C2SizeSubSelect", param.C2SizeSubSelect);
		ini.WriteInt("Throughput", "C2SizeItemSelect", param.C2SizeItemSelect);
		ini.WriteBool("Throughput", "C3Enable", param.C3Enable);
		ini.WriteString("Throughput", "C3Str", param.C3Str);
		ini.WriteInt("Throughput", "C3SizeStation", param.C3SizeStation);
		ini.WriteInt("Throughput", "C3SizeToolSelect", param.C3SizeToolSelect);
		ini.WriteInt("Throughput", "C3SizeSubSelect", param.C3SizeSubSelect);
		ini.WriteInt("Throughput", "C3SizeItemSelect", param.C3SizeItemSelect);
		ini.WriteBool("Throughput", "C4Enable", param.C4Enable);
		ini.WriteString("Throughput", "C4Str", param.C4Str);
		ini.WriteInt("Throughput", "C4SizeStation", param.C4SizeStation);
		ini.WriteInt("Throughput", "C4SizeToolSelect", param.C4SizeToolSelect);
		ini.WriteInt("Throughput", "C4SizeSubSelect", param.C4SizeSubSelect);
		ini.WriteInt("Throughput", "C4SizeItemSelect", param.C4SizeItemSelect);
		ini.WriteBool("Throughput", "C5Enable", param.C5Enable);
		ini.WriteString("Throughput", "C5Str", param.C5Str);
		ini.WriteInt("Throughput", "C5SizeStation", param.C5SizeStation);
		ini.WriteInt("Throughput", "C5SizeToolSelect", param.C5SizeToolSelect);
		ini.WriteInt("Throughput", "C5SizeSubSelect", param.C5SizeSubSelect);
		ini.WriteInt("Throughput", "C5SizeItemSelect", param.C5SizeItemSelect);
		ini.WriteBool("Throughput", "C6Enable", param.C6Enable);
		ini.WriteString("Throughput", "C6Str", param.C6Str);
		ini.WriteInt("Throughput", "C6SizeStation", param.C6SizeStation);
		ini.WriteInt("Throughput", "C6SizeToolSelect", param.C6SizeToolSelect);
		ini.WriteInt("Throughput", "C6SizeSubSelect", param.C6SizeSubSelect);
		ini.WriteInt("Throughput", "C6SizeItemSelect", param.C6SizeItemSelect);
		ini.WriteBool("Throughput", "C7Enable", param.C7Enable);
		ini.WriteString("Throughput", "C7Str", param.C7Str);
		ini.WriteInt("Throughput", "C7SizeStation", param.C7SizeStation);
		ini.WriteInt("Throughput", "C7SizeToolSelect", param.C7SizeToolSelect);
		ini.WriteInt("Throughput", "C7SizeSubSelect", param.C7SizeSubSelect);
		ini.WriteInt("Throughput", "C7SizeItemSelect", param.C7SizeItemSelect);
		ini.WriteBool("Throughput", "C8Enable", param.C8Enable);
		ini.WriteString("Throughput", "C8Str", param.C8Str);
		ini.WriteInt("Throughput", "C8SizeStation", param.C8SizeStation);
		ini.WriteInt("Throughput", "C8SizeToolSelect", param.C8SizeToolSelect);
		ini.WriteInt("Throughput", "C8SizeSubSelect", param.C8SizeSubSelect);
		ini.WriteInt("Throughput", "C8SizeItemSelect", param.C8SizeItemSelect);
		ini.WriteBool("Throughput", "C9Enable", param.C9Enable);
		ini.WriteString("Throughput", "C9Str", param.C9Str);
		ini.WriteInt("Throughput", "C9SizeStation", param.C9SizeStation);
		ini.WriteInt("Throughput", "C9SizeToolSelect", param.C9SizeToolSelect);
		ini.WriteInt("Throughput", "C9SizeSubSelect", param.C9SizeSubSelect);
		ini.WriteInt("Throughput", "C9SizeItemSelect", param.C9SizeItemSelect);
		ini.WriteBool("Throughput", "C10Enable", param.C10Enable);
		ini.WriteString("Throughput", "C10Str", param.C10Str);
		ini.WriteInt("Throughput", "C10SizeStation", param.C10SizeStation);
		ini.WriteInt("Throughput", "C10SizeToolSelect", param.C10SizeToolSelect);
		ini.WriteInt("Throughput", "C10SizeSubSelect", param.C10SizeSubSelect);
		ini.WriteInt("Throughput", "C10SizeItemSelect", param.C10SizeItemSelect);
		ini.WriteBool("Throughput", "C11Enable", param.C11Enable);
		ini.WriteString("Throughput", "C11Str", param.C11Str);
		ini.WriteInt("Throughput", "C11SizeStation", param.C11SizeStation);
		ini.WriteInt("Throughput", "C11SizeToolSelect", param.C11SizeToolSelect);
		ini.WriteInt("Throughput", "C11SizeSubSelect", param.C11SizeSubSelect);
		ini.WriteInt("Throughput", "C11SizeItemSelect", param.C11SizeItemSelect);
		ini.WriteBool("Throughput", "C12Enable", param.C12Enable);
		ini.WriteString("Throughput", "C12Str", param.C12Str);
		ini.WriteInt("Throughput", "C12SizeStation", param.C12SizeStation);
		ini.WriteInt("Throughput", "C12SizeToolSelect", param.C12SizeToolSelect);
		ini.WriteInt("Throughput", "C12SizeSubSelect", param.C12SizeSubSelect);
		ini.WriteInt("Throughput", "C12SizeItemSelect", param.C12SizeItemSelect);
		ini.WriteBool("Throughput", "C13Enable", param.C13Enable);
		ini.WriteString("Throughput", "C13Str", param.C13Str);
		ini.WriteInt("Throughput", "C13SizeStation", param.C13SizeStation);
		ini.WriteInt("Throughput", "C13SizeToolSelect", param.C13SizeToolSelect);
		ini.WriteInt("Throughput", "C13SizeSubSelect", param.C13SizeSubSelect);
		ini.WriteInt("Throughput", "C13SizeItemSelect", param.C13SizeItemSelect);
		ini.WriteBool("Throughput", "C14Enable", param.C14Enable);
		ini.WriteString("Throughput", "C14Str", param.C14Str);
		ini.WriteInt("Throughput", "C14SizeStation", param.C14SizeStation);
		ini.WriteInt("Throughput", "C14SizeToolSelect", param.C14SizeToolSelect);
		ini.WriteInt("Throughput", "C14SizeSubSelect", param.C14SizeSubSelect);
		ini.WriteInt("Throughput", "C14SizeItemSelect", param.C14SizeItemSelect);
		WriteComDigitsParam();
		UpdateComDigis();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteComDigitsParam()
	{
		if (ini == null)
		{
			return false;
		}
		ini.WriteDouble("Throughput", "ComDigits0", param.ComDigits0);
		ini.WriteDouble("Throughput", "ComDigits1", param.ComDigits1);
		ini.WriteDouble("Throughput", "ComDigits2", param.ComDigits2);
		ini.WriteDouble("Throughput", "ComDigits3", param.ComDigits3);
		ini.WriteDouble("Throughput", "ComDigits4", param.ComDigits4);
		ini.WriteDouble("Throughput", "ComDigits5", param.ComDigits5);
		ini.WriteDouble("Throughput", "ComDigits6", param.ComDigits6);
		ini.WriteDouble("Throughput", "ComDigits7", param.ComDigits7);
		ini.WriteDouble("Throughput", "ComDigits8", param.ComDigits8);
		ini.WriteDouble("Throughput", "ComDigits9", param.ComDigits9);
		ini.WriteDouble("Throughput", "ComDigits10", param.ComDigits10);
		ini.WriteDouble("Throughput", "ComDigits11", param.ComDigits11);
		ini.WriteDouble("Throughput", "ComDigits12", param.ComDigits12);
		ini.WriteDouble("Throughput", "ComDigits13", param.ComDigits13);
		ini.WriteDouble("Throughput", "ComDigits14", param.ComDigits14);
		ini.WriteDouble("Throughput", "ComDigits15", param.ComDigits15);
		ini.WriteDouble("Throughput", "ComDigits16", param.ComDigits16);
		ini.WriteDouble("Throughput", "ComDigits17", param.ComDigits17);
		ini.WriteDouble("Throughput", "ComDigits18", param.ComDigits18);
		ini.WriteDouble("Throughput", "ComDigits19", param.ComDigits19);
		ini.WriteDouble("Throughput", "ComDigits20", param.ComDigits20);
		ini.WriteDouble("Throughput", "ComDigits21", param.ComDigits21);
		ini.WriteDouble("Throughput", "ComDigits22", param.ComDigits22);
		ini.WriteDouble("Throughput", "ComDigits23", param.ComDigits23);
		ini.WriteString("Throughput", "ComDigitsName0", param.ComDigitsName0);
		ini.WriteString("Throughput", "ComDigitsName1", param.ComDigitsName1);
		ini.WriteString("Throughput", "ComDigitsName2", param.ComDigitsName2);
		ini.WriteString("Throughput", "ComDigitsName3", param.ComDigitsName3);
		ini.WriteString("Throughput", "ComDigitsName4", param.ComDigitsName4);
		ini.WriteString("Throughput", "ComDigitsName5", param.ComDigitsName5);
		ini.WriteString("Throughput", "ComDigitsName6", param.ComDigitsName6);
		ini.WriteString("Throughput", "ComDigitsName7", param.ComDigitsName7);
		ini.WriteString("Throughput", "ComDigitsName8", param.ComDigitsName8);
		ini.WriteString("Throughput", "ComDigitsName9", param.ComDigitsName9);
		ini.WriteString("Throughput", "ComDigitsName10", param.ComDigitsName10);
		ini.WriteString("Throughput", "ComDigitsName11", param.ComDigitsName11);
		ini.WriteString("Throughput", "ComDigitsName12", param.ComDigitsName12);
		ini.WriteString("Throughput", "ComDigitsName13", param.ComDigitsName13);
		ini.WriteString("Throughput", "ComDigitsName14", param.ComDigitsName14);
		ini.WriteString("Throughput", "ComDigitsName15", param.ComDigitsName15);
		ini.WriteString("Throughput", "ComDigitsName16", param.ComDigitsName16);
		ini.WriteString("Throughput", "ComDigitsName17", param.ComDigitsName17);
		ini.WriteString("Throughput", "ComDigitsName18", param.ComDigitsName18);
		ini.WriteString("Throughput", "ComDigitsName19", param.ComDigitsName19);
		ini.WriteString("Throughput", "ComDigitsName20", param.ComDigitsName20);
		ini.WriteString("Throughput", "ComDigitsName21", param.ComDigitsName21);
		ini.WriteString("Throughput", "ComDigitsName22", param.ComDigitsName22);
		ini.WriteString("Throughput", "ComDigitsName23", param.ComDigitsName23);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadInspSurvParam()
	{
		if (ini == null)
		{
			return false;
		}
		string text = "";
		for (int i = 0; i < 20; i++)
		{
			text = string.Format("InspSurvItems{0}", i);
			param.InspSurvItems[i] = ini.GetValue("Throughput", text);
			text = string.Format("InspSurvItemsAlarmRatio{0}", i);
			param.InspSurvItemsAlarmRatio[i] = ini.GetDouble("Throughput", text, 30.0);
		}
		int num = 1;
		IsItems.Clear();
		for (int j = 0; j < 20; j++)
		{
			text = param.InspSurvItems[j];
			if (text != "")
			{
				IsItems.Add(ProcessString(text, num, param.InspSurvItemsAlarmRatio[j]));
				num++;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private INSPSURVITEM ProcessString(string str, int count, double AlarmRatio)
	{
		INSPSURVITEM iNSPSURVITEM = new INSPSURVITEM();
		try
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			iNSPSURVITEM.ID = count;
			num = Convert.ToInt32(str.Substring(0, 1));
			num2 = Convert.ToInt32(str.Substring(1, 2));
			num3 = Convert.ToInt32(str.Substring(3, 2));
			iNSPSURVITEM.Station = num;
			iNSPSURVITEM.InspToolID = num2;
			iNSPSURVITEM.InspToolName = GetInspToolName(num2);
			iNSPSURVITEM.TrackID = num3;
			bool status = true;
			iNSPSURVITEM.TrackName = GetTrackName(num, num2, num3, ref status);
			iNSPSURVITEM.Status = status;
			if (status)
			{
				iNSPSURVITEM.StatusStr = "開啟";
			}
			else
			{
				iNSPSURVITEM.StatusStr = "關閉";
			}
			iNSPSURVITEM.StationStr = GetStationStr(num);
			iNSPSURVITEM.AlarmRatio = AlarmRatio;
			return iNSPSURVITEM;
		}
		catch (Exception ex)
		{
			string text = "ISW03-InspSurvWindow-Btn_Delete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return iNSPSURVITEM;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetStationStr(int station)
	{
		return string.Format("第{0}站", station + 1);
	}

	private string GetTrackName(int station, int toolid, int trackno, ref bool status)
	{
		string result = "";
		switch (toolid)
		{
		case 0:
			result = inspection[station].paramFile.AdvLocateMParam[trackno].TrackerName;
			if (!inspection[station].paramFile.AdvLocateToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.AdvLocateMParam[trackno].TrackerON;
			}
			break;
		case 1:
			result = inspection[station].paramFile.MeaWidthParam[trackno].TrackerName;
			if (!inspection[station].paramFile.MeasureToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.MeaWidthParam[trackno].TrackerON;
			}
			break;
		case 2:
			result = inspection[station].paramFile.MeaHeightParam[trackno].TrackerName;
			if (!inspection[station].paramFile.MeasureToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.MeaHeightParam[trackno].TrackerON;
			}
			break;
		case 3:
			result = inspection[station].paramFile.BinaryParam[trackno].TrackerName;
			if (!inspection[station].paramFile.BinaryToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.BinaryParam[trackno].TrackerON;
			}
			break;
		case 4:
			result = inspection[station].paramFile.SmoothParam[trackno].TrackerName;
			if (!inspection[station].paramFile.SmoothToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.SmoothParam[trackno].TrackerON;
			}
			break;
		case 5:
			result = inspection[station].paramFile.BlockAvgParam[trackno].TrackerName;
			if (!inspection[station].paramFile.BlockAvgToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.BlockAvgParam[trackno].TrackerON;
			}
			break;
		case 6:
			result = inspection[station].paramFile.DifferenceParam[trackno].TrackerName;
			if (!inspection[station].paramFile.DifferenceToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.DifferenceParam[trackno].TrackerON;
			}
			break;
		case 7:
			result = inspection[station].paramFile.LineAnalysisParam[trackno].TrackerName;
			if (!inspection[station].paramFile.LineAnalysisToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.LineAnalysisParam[trackno].TrackerON;
			}
			break;
		case 8:
			result = inspection[station].paramFile.MatchParam[trackno].TrackerName;
			if (!inspection[station].paramFile.MaskToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.MatchParam[trackno].TrackerON;
			}
			break;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetInspToolName(int toolid)
	{
		try
		{
			string result = "";
			switch (toolid)
			{
			case 0:
				result = "進階量測";
				break;
			case 1:
				result = "量測寬度";
				break;
			case 2:
				result = "量測高度";
				break;
			case 3:
				result = "二值化";
				break;
			case 4:
				result = "平滑二值化";
				break;
			case 5:
				result = "差異二值化";
				break;
			case 6:
				result = "線段分析";
				break;
			case 7:
				result = "圖形比對";
				break;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "ISW02-InspSurvWindow-Btn_Delete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			return text;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteInspSurvParam()
	{
		if (ini == null)
		{
			return false;
		}
		string text = "";
		for (int i = 0; i < 20; i++)
		{
			text = string.Format("InspSurvItems{0}", i);
			ini.WriteString("Throughput", text, param.InspSurvItems[i]);
			text = string.Format("InspSurvItemsAlarmRatio{0}", i);
			ini.WriteDouble("Throughput", text, param.InspSurvItemsAlarmRatio[i]);
		}
		return true;
	}

	public void GetComDigis(ref double[] Digits, ref string[] DigitsName)
	{
		for (int i = 0; i < 24; i++)
		{
			Digits[i] = ComDigits[i];
			DigitsName[i] = ComDigitsName[i];
		}
	}

	public void SetComDigis(double[] Digits, string[] DigitsName)
	{
		for (int i = 0; i < 24; i++)
		{
			ComDigits[i] = Digits[i];
			ComDigitsName[i] = DigitsName[i];
		}
		param.ComDigits0 = ComDigits[0];
		param.ComDigits1 = ComDigits[1];
		param.ComDigits2 = ComDigits[2];
		param.ComDigits3 = ComDigits[3];
		param.ComDigits4 = ComDigits[4];
		param.ComDigits5 = ComDigits[5];
		param.ComDigits6 = ComDigits[6];
		param.ComDigits7 = ComDigits[7];
		param.ComDigits8 = ComDigits[8];
		param.ComDigits9 = ComDigits[9];
		param.ComDigits10 = ComDigits[10];
		param.ComDigits11 = ComDigits[11];
		param.ComDigits12 = ComDigits[12];
		param.ComDigits13 = ComDigits[13];
		param.ComDigits14 = ComDigits[14];
		param.ComDigits15 = ComDigits[15];
		param.ComDigits16 = ComDigits[16];
		param.ComDigits17 = ComDigits[17];
		param.ComDigits18 = ComDigits[18];
		param.ComDigits19 = ComDigits[19];
		param.ComDigits20 = ComDigits[20];
		param.ComDigits21 = ComDigits[21];
		param.ComDigits22 = ComDigits[22];
		param.ComDigits23 = ComDigits[23];
		param.ComDigitsName0 = ComDigitsName[0];
		param.ComDigitsName1 = ComDigitsName[1];
		param.ComDigitsName2 = ComDigitsName[2];
		param.ComDigitsName3 = ComDigitsName[3];
		param.ComDigitsName4 = ComDigitsName[4];
		param.ComDigitsName5 = ComDigitsName[5];
		param.ComDigitsName6 = ComDigitsName[6];
		param.ComDigitsName7 = ComDigitsName[7];
		param.ComDigitsName8 = ComDigitsName[8];
		param.ComDigitsName9 = ComDigitsName[9];
		param.ComDigitsName10 = ComDigitsName[10];
		param.ComDigitsName11 = ComDigitsName[11];
		param.ComDigitsName12 = ComDigitsName[12];
		param.ComDigitsName13 = ComDigitsName[13];
		param.ComDigitsName14 = ComDigitsName[14];
		param.ComDigitsName15 = ComDigitsName[15];
		param.ComDigitsName16 = ComDigitsName[16];
		param.ComDigitsName17 = ComDigitsName[17];
		param.ComDigitsName18 = ComDigitsName[18];
		param.ComDigitsName19 = ComDigitsName[19];
		param.ComDigitsName20 = ComDigitsName[20];
		param.ComDigitsName21 = ComDigitsName[21];
		param.ComDigitsName22 = ComDigitsName[22];
		param.ComDigitsName23 = ComDigitsName[23];
		WriteComDigitsParam();
	}

	public void UpdateComDigis()
	{
		ComDigits[0] = param.ComDigits0;
		ComDigits[1] = param.ComDigits1;
		ComDigits[2] = param.ComDigits2;
		ComDigits[3] = param.ComDigits3;
		ComDigits[4] = param.ComDigits4;
		ComDigits[5] = param.ComDigits5;
		ComDigits[6] = param.ComDigits6;
		ComDigits[7] = param.ComDigits7;
		ComDigits[8] = param.ComDigits8;
		ComDigits[9] = param.ComDigits9;
		ComDigits[10] = param.ComDigits10;
		ComDigits[11] = param.ComDigits11;
		ComDigits[12] = param.ComDigits12;
		ComDigits[13] = param.ComDigits13;
		ComDigits[14] = param.ComDigits14;
		ComDigits[15] = param.ComDigits15;
		ComDigits[16] = param.ComDigits16;
		ComDigits[17] = param.ComDigits17;
		ComDigits[18] = param.ComDigits18;
		ComDigits[19] = param.ComDigits19;
		ComDigits[20] = param.ComDigits20;
		ComDigits[21] = param.ComDigits21;
		ComDigits[22] = param.ComDigits22;
		ComDigits[23] = param.ComDigits23;
		ComDigitsName[0] = param.ComDigitsName0;
		ComDigitsName[1] = param.ComDigitsName1;
		ComDigitsName[2] = param.ComDigitsName2;
		ComDigitsName[3] = param.ComDigitsName3;
		ComDigitsName[4] = param.ComDigitsName4;
		ComDigitsName[5] = param.ComDigitsName5;
		ComDigitsName[6] = param.ComDigitsName6;
		ComDigitsName[7] = param.ComDigitsName7;
		ComDigitsName[8] = param.ComDigitsName8;
		ComDigitsName[9] = param.ComDigitsName9;
		ComDigitsName[10] = param.ComDigitsName10;
		ComDigitsName[11] = param.ComDigitsName11;
		ComDigitsName[12] = param.ComDigitsName12;
		ComDigitsName[13] = param.ComDigitsName13;
		ComDigitsName[14] = param.ComDigitsName14;
		ComDigitsName[15] = param.ComDigitsName15;
		ComDigitsName[16] = param.ComDigitsName16;
		ComDigitsName[17] = param.ComDigitsName17;
		ComDigitsName[18] = param.ComDigitsName18;
		ComDigitsName[19] = param.ComDigitsName19;
		ComDigitsName[20] = param.ComDigitsName20;
		ComDigitsName[21] = param.ComDigitsName21;
		ComDigitsName[22] = param.ComDigitsName22;
		ComDigitsName[23] = param.ComDigitsName23;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadParetoItems()
	{
		if (ini == null)
		{
			return false;
		}
		try
		{
			EnableMultiParetoMode = (param.EnableMultiParetoMode = ini.GetBoolean("Throughput", "EnableMultiParetoMode"));
			ReadMainParam();
			int num = 1;
			for (int i = 1; i < 30; i++)
			{
				param.AllParetoItems[i] = "";
				if (num == 30)
				{
					break;
				}
				string key = string.Format("Pareto{0}", num);
				string value = ini.GetValue("ParetoItmes", key);
				num++;
				if (value != "")
				{
					param.AllParetoItems[i] = value;
				}
				else if (num < 29)
				{
					key = string.Format("Pareto{0}", num);
					value = ini.GetValue("ParetoItmes", key);
					if (value != "")
					{
						param.AllParetoItems[i] = value;
						num++;
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("TPF01-ThroughputParamFile-ReadParetoItems()-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool WriteParetoItems()
	{
		if (ini == null)
		{
			return false;
		}
		param.EnableMultiParetoMode = EnableMultiParetoMode;
		ini.WriteBool("Throughput", "EnableMultiParetoMode", param.EnableMultiParetoMode);
		WriteMainParam();
		for (int i = 1; i < 30; i++)
		{
			string key = string.Format("Pareto{0}", i);
			ini.WriteString("ParetoItmes", key, param.AllParetoItems[i]);
		}
		return true;
	}

	public bool GetEnableMultiParetoMode()
	{
		return EnableMultiParetoMode;
	}

	public void SetEnableMultiParetoMode(bool enable)
	{
		EnableMultiParetoMode = enable;
	}

	public bool GetEnableThroughputWindow()
	{
		return EnableThroughputWindow;
	}

	public void SetEnableThroughputWindow(bool enable)
	{
		EnableThroughputWindow = enable;
	}
}
