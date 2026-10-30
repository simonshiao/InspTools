namespace InspTools.AOI;

public struct INSP_AITOOL_PARAM
{
	public bool ValueChangeFlag;

	public int Width_A;

	public int Height_A;

	public bool EnableAsignGPU;

	public int GPUID;

	public string ModelName_A;

	public string LastModelFile_A;

	public int DnetMode_A;

	public int DnetMode_B;

	public int DnetROIIndex_A1;

	public int DnetROIIndex_A2;

	public int DnetROIIndex_A3;

	public int DnetROIIndex_A4;

	public int DnetROIIndex_A5;

	public int DnetROIIndex_A6;

	public int DnetROIIndex_A7;

	public int DnetROIIndex_A8;

	public int DnetROIIndex_B1;

	public int DnetROIIndex_B2;

	public int DnetROIIndex_B3;

	public int DnetROIIndex_B4;

	public int DnetROIIndex_B5;

	public int DnetROIIndex_B6;

	public int DnetROIIndex_B7;

	public int DnetROIIndex_B8;

	public FLOW_BIN ErrorBin_A0;

	public FLOW_BIN ErrorBin_A1;

	public FLOW_BIN ErrorBin_A2;

	public FLOW_BIN ErrorBin_A3;

	public bool EnableCustomRGBChannel;

	public int CustomChannelR;

	public int CustomChannelG;

	public int CustomChannelB;

	public int Width_B;

	public int Height_B;

	public string ModelName_B;

	public string LastModelFile_B;

	public string NVCardName;

	public FLOW_BIN ErrorBin_B0;

	public FLOW_BIN ErrorBin_B1;

	public FLOW_BIN ErrorBin_B2;

	public FLOW_BIN ErrorBin_B3;

	public int Width_C;

	public int Height_C;

	public string ModelName_C;

	public string LastModelFile_C;

	public FLOW_BIN ErrorBin_C0;

	public FLOW_BIN ErrorBin_C1;

	public FLOW_BIN ErrorBin_C2;

	public FLOW_BIN ErrorBin_C3;

	public int Width_D;

	public int Height_D;

	public string ModelName_D;

	public string LastModelFile_D;

	public FLOW_BIN ErrorBin_D0;

	public FLOW_BIN ErrorBin_D1;

	public FLOW_BIN ErrorBin_D2;

	public FLOW_BIN ErrorBin_D3;

	public int ParetoIndex_A;

	public int ParetoIndex_B;

	public bool EnableSelectFace;

	public int FaceIndex;

	public bool ModelExist_A;

	public bool ModelExist_B;

	public bool ModelExist_C;

	public bool ModelExist_D;

	public int TrainingCount;

	public int TotalTrainedCount;

	public bool Model_A_Changed;

	public bool Model_B_Changed;

	public bool UseUNET;

	public bool UseTensorRT;

	public bool NewModelMode;

	public bool NGClone_EnableAutoMark;

	public int NGClone_AutoMarkDilate;

	public int NGClone_ImageMixMode;

	public bool NGClone_EnableRandom;

	public bool NGClone_EnableRandomH;

	public bool NGClone_EnableRandomV;

	public bool NGClone_EnableRandomA;

	public bool NGClone_EnableFlipH;

	public bool NGClone_EnableFlipV;

	public double NGClone_RotateAngle;
}
