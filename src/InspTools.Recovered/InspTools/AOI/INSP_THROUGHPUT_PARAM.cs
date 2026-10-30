using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InspTools.AOI;

[StructLayout(LayoutKind.Sequential)]
public class INSP_THROUGHPUT_PARAM
{
	public string OutputPath;

	public int OutputFormat;

	public int OutputImageCount;

	public int OutputALLOKNG;

	public bool Station1ON;

	public bool Station2ON;

	public bool Station3ON;

	public bool Station4ON;

	public bool Station5ON;

	public bool Station6ON;

	public bool Station7ON;

	public bool Station8ON;

	public string[] AllParetoItems = new string[30];

	public bool EnableMultiParetoMode;

	public string YieldRetestPath;

	public bool OpenThroughputWindow;

	public int OpenCVThreadNum;

	public bool EnableSetOpenCVThreadNum;

	public bool CpuUseOptimized;

	public bool EnableAIPasswordProtect;

	public string AIPassword;

	public int CurrentClient;

	public int TrackerAColorIndex;

	public int TrackerAThickness;

	public int TrackerBColorIndex;

	public int TrackerBThickness;

	public bool LSizeEnable;

	public int LSizeStation;

	public int LSizeToolSelect;

	public int LSizeSubSelect;

	public int LSizeItemSelect;

	public bool WSizeEnable;

	public int WSizeStation;

	public int WSizeToolSelect;

	public int WSizeSubSelect;

	public int WSizeItemSelect;

	public bool TSizeEnable;

	public int TSizeStation;

	public int TSizeToolSelect;

	public int TSizeSubSelect;

	public int TSizeItemSelect;

	public bool LPadSizeEnable;

	public string LPadStr;

	public int LPadSizeStation;

	public int LPadSizeToolSelect;

	public int LPadSizeSubSelect;

	public int LPadSizeItemSelect;

	public bool RPadSizeEnable;

	public string RPadStr;

	public int RPadSizeStation;

	public int RPadSizeToolSelect;

	public int RPadSizeSubSelect;

	public int RPadSizeItemSelect;

	public bool GapSizeEnable;

	public string GapStr;

	public int GapSizeStation;

	public int GapSizeToolSelect;

	public int GapSizeSubSelect;

	public int GapSizeItemSelect;

	public bool C1Enable;

	public string C1Str;

	public int C1SizeStation;

	public int C1SizeToolSelect;

	public int C1SizeSubSelect;

	public int C1SizeItemSelect;

	public bool C2Enable;

	public string C2Str;

	public int C2SizeStation;

	public int C2SizeToolSelect;

	public int C2SizeSubSelect;

	public int C2SizeItemSelect;

	public bool C3Enable;

	public string C3Str;

	public int C3SizeStation;

	public int C3SizeToolSelect;

	public int C3SizeSubSelect;

	public int C3SizeItemSelect;

	public bool C4Enable;

	public string C4Str;

	public int C4SizeStation;

	public int C4SizeToolSelect;

	public int C4SizeSubSelect;

	public int C4SizeItemSelect;

	public bool C5Enable;

	public string C5Str;

	public int C5SizeStation;

	public int C5SizeToolSelect;

	public int C5SizeSubSelect;

	public int C5SizeItemSelect;

	public bool C6Enable;

	public string C6Str;

	public int C6SizeStation;

	public int C6SizeToolSelect;

	public int C6SizeSubSelect;

	public int C6SizeItemSelect;

	public bool C7Enable;

	public string C7Str;

	public int C7SizeStation;

	public int C7SizeToolSelect;

	public int C7SizeSubSelect;

	public int C7SizeItemSelect;

	public bool C8Enable;

	public string C8Str;

	public int C8SizeStation;

	public int C8SizeToolSelect;

	public int C8SizeSubSelect;

	public int C8SizeItemSelect;

	public bool C9Enable;

	public string C9Str;

	public int C9SizeStation;

	public int C9SizeToolSelect;

	public int C9SizeSubSelect;

	public int C9SizeItemSelect;

	public bool C10Enable;

	public string C10Str;

	public int C10SizeStation;

	public int C10SizeToolSelect;

	public int C10SizeSubSelect;

	public int C10SizeItemSelect;

	public bool C11Enable;

	public string C11Str;

	public int C11SizeStation;

	public int C11SizeToolSelect;

	public int C11SizeSubSelect;

	public int C11SizeItemSelect;

	public bool C12Enable;

	public string C12Str;

	public int C12SizeStation;

	public int C12SizeToolSelect;

	public int C12SizeSubSelect;

	public int C12SizeItemSelect;

	public bool C13Enable;

	public string C13Str;

	public int C13SizeStation;

	public int C13SizeToolSelect;

	public int C13SizeSubSelect;

	public int C13SizeItemSelect;

	public bool C14Enable;

	public string C14Str;

	public int C14SizeStation;

	public int C14SizeToolSelect;

	public int C14SizeSubSelect;

	public int C14SizeItemSelect;

	public double ComDigits0;

	public double ComDigits1;

	public double ComDigits2;

	public double ComDigits3;

	public double ComDigits4;

	public double ComDigits5;

	public double ComDigits6;

	public double ComDigits7;

	public double ComDigits8;

	public double ComDigits9;

	public double ComDigits10;

	public double ComDigits11;

	public double ComDigits12;

	public double ComDigits13;

	public double ComDigits14;

	public double ComDigits15;

	public double ComDigits16;

	public double ComDigits17;

	public double ComDigits18;

	public double ComDigits19;

	public double ComDigits20;

	public double ComDigits21;

	public double ComDigits22;

	public double ComDigits23;

	public string ComDigitsName0;

	public string ComDigitsName1;

	public string ComDigitsName2;

	public string ComDigitsName3;

	public string ComDigitsName4;

	public string ComDigitsName5;

	public string ComDigitsName6;

	public string ComDigitsName7;

	public string ComDigitsName8;

	public string ComDigitsName9;

	public string ComDigitsName10;

	public string ComDigitsName11;

	public string ComDigitsName12;

	public string ComDigitsName13;

	public string ComDigitsName14;

	public string ComDigitsName15;

	public string ComDigitsName16;

	public string ComDigitsName17;

	public string ComDigitsName18;

	public string ComDigitsName19;

	public string ComDigitsName20;

	public string ComDigitsName21;

	public string ComDigitsName22;

	public string ComDigitsName23;

	public double CustomRatio;

	public bool EnableCustomRatio;

	public bool HideTrainingWnd;

	public bool EnableTrainingBufferTest;

	public int TrainingBufferIndex;

	public bool SaveImgUseDateForlder;

	public bool EnableAutoUpdateModel;

	public bool EnableMoveToTrainingImg;

	public bool[] EnableStationTrain = new bool[8];

	public string[] HashTagStr = new string[8];

	public int[] TrainingCount = new int[8];

	public double[] TrainingImageDoubleRate = new double[8];

	public double[] Training_horizontalFlip = new double[8];

	public double[] Training_verticalFlip = new double[8];

	public double[] Training_brightness_limit = new double[8];

	public double[] Training_contrast_limit = new double[8];

	public double[] Training_scale_limit = new double[8];

	public double[] Training_rotate_limit = new double[8];

	public double[] Training_shift_limit = new double[8];

	public int[] Training_hue_shift_limit = new int[8];

	public int[] Training_sat_shift_limit = new int[8];

	public int[] Training_val_shift_limit = new int[8];

	public int[] Training_r_shift_limit = new int[8];

	public int[] Training_g_shift_limit = new int[8];

	public int[] Training_b_shift_limit = new int[8];

	public int[] BatchSize = new int[8];

	public int[] DnetDiv = new int[8];

	public bool BinReverseMode;

	public string[] InspSurvItems = new string[20];

	public double[] InspSurvItemsAlarmRatio = new double[20];

	[MethodImpl(MethodImplOptions.NoInlining)]
	public INSP_THROUGHPUT_PARAM()
	{
		for (int i = 0; i < 8; i++)
		{
			TrainingCount[i] = 100;
			Training_horizontalFlip[i] = 1.0;
			Training_verticalFlip[i] = 1.0;
			Training_brightness_limit[i] = 0.2;
			Training_contrast_limit[i] = 0.2;
			Training_scale_limit[i] = 0.1;
			Training_rotate_limit[i] = 0.0;
			Training_shift_limit[i] = 0.1;
			Training_hue_shift_limit[i] = 20;
			Training_sat_shift_limit[i] = 30;
			Training_val_shift_limit[i] = 20;
			Training_r_shift_limit[i] = 20;
			Training_g_shift_limit[i] = 20;
			Training_b_shift_limit[i] = 20;
		}
		for (int j = 0; j < 30; j++)
		{
			if (j == 0)
			{
				AllParetoItems[j] = "------";
			}
			else
			{
				AllParetoItems[j] = "";
			}
		}
		for (int k = 0; k < 20; k++)
		{
			InspSurvItems[k] = "";
			InspSurvItemsAlarmRatio[k] = 100.0;
		}
	}
}
