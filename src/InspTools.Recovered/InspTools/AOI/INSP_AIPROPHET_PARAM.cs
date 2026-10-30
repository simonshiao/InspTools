namespace InspTools.AOI;

public struct INSP_AIPROPHET_PARAM
{
	public bool ValueChangeFlag;

	public int ModelImageSize;

	public int TrainCount;

	public string ModelName;

	public string ModelImagePath;

	public bool EnableTensorRT;

	public int Track_T;

	public int Track_B;

	public int Track_L;

	public int Track_R;

	public bool Mask1_ON;

	public bool Mask2_ON;

	public bool Mask3_ON;

	public bool Mask4_ON;

	public int Mask1_T;

	public int Mask1_B;

	public int Mask1_L;

	public int Mask1_R;

	public int Mask2_T;

	public int Mask2_B;

	public int Mask2_L;

	public int Mask2_R;

	public int Mask3_T;

	public int Mask3_B;

	public int Mask3_L;

	public int Mask3_R;

	public int Mask4_T;

	public int Mask4_B;

	public int Mask4_L;

	public int Mask4_R;

	public double AllowSocreMin;

	public double AllowSocreMax;

	public FLOW_BIN ErrorBin;

	public int ParetoIndex;

	public bool ModelUnTrain;
}
