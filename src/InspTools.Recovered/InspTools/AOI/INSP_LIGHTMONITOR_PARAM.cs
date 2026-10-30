namespace InspTools.AOI;

public struct INSP_LIGHTMONITOR_PARAM
{
	public bool ValueChangeFlag;

	public bool LightMonitor_ON;

	public bool LightCorrection_ON;

	public bool EnableSoftCorrection;

	public int SearchRectT;

	public int SearchRectB;

	public int SearchRectL;

	public int SearchRectR;

	public int SearchRectWidth;

	public int SearchRectHeight;

	public COLOR_CHANNEL ChannelIndex;

	public int TrackerAligmentIndex;

	public int GainPerValue;

	public int VoltagePerValueR;

	public int VoltagePerValueG;

	public int VoltagePerValueB;

	public int AlarmBinaryValue;

	public double AlarmRatioR;

	public double AlarmRatioG;

	public double AlarmRatioB;

	public int MonitorValue;

	public int MonitorValueR;

	public int MonitorValueG;

	public int MonitorValueB;

	public int CorrectionBasicValue;

	public int CorrectionValueR;

	public int CorrectionValueG;

	public int CorrectionValueB;

	public double CorrectionRatioR;

	public double CorrectionRatioG;

	public double CorrectionRatioB;
}
