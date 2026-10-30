using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class LightMonitorTool
{
	private CVBuffer buffers;

	private ParamFile pfile;

	private Stopwatch sw = new Stopwatch();

	private Mat Binary = new Mat();

	private Mat BinaryTemp = new Mat();

	public string ResultStr = "";

	public int MaxMonitorCount = 500;

	private List<int> LightMonitorThd = new List<int>();

	private List<int> LightMonitorR = new List<int>();

	private List<int> LightMonitorG = new List<int>();

	private List<int> LightMonitorB = new List<int>();

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public LightMonitorTool(ParamFile paramfile)
	{
		pfile = paramfile;
	}

	public void SetNumThreads(int num)
	{
		Cv2.SetNumThreads(num);
	}

	public void SetUseOptimized(bool enable)
	{
		Cv2.SetUseOptimized(enable);
	}

	public void SetBuffers(CVBuffer bufs)
	{
		buffers = bufs;
	}

	public Rect GetInspeRect()
	{
		LocateRect = buffers.LocatedRect;
		int trackerAligmentIndex = pfile.LightMonitorParam.TrackerAligmentIndex;
		Rect result = default(Rect);
		result.X = pfile.AdvLocateParam[trackerAligmentIndex].L + pfile.LightMonitorParam.SearchRectL;
		result.Y = pfile.AdvLocateParam[trackerAligmentIndex].T + pfile.LightMonitorParam.SearchRectT;
		result.Width = pfile.AdvLocateParam[trackerAligmentIndex].Width - (pfile.LightMonitorParam.SearchRectL + pfile.LightMonitorParam.SearchRectR);
		result.Height = pfile.AdvLocateParam[trackerAligmentIndex].Height - (pfile.LightMonitorParam.SearchRectT + pfile.LightMonitorParam.SearchRectB);
		return result;
	}

	public void ClearLightMonitorThd()
	{
		LightMonitorThd.Clear();
		LightMonitorR.Clear();
		LightMonitorG.Clear();
		LightMonitorB.Clear();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void UpdateLightMonitorBaseValue()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		try
		{
			for (int i = 0; i < LightMonitorR.Count; i++)
			{
				num += LightMonitorR[i];
				num3++;
			}
			if (num3 > 0)
			{
				num2 = (int)((double)num / (double)num3);
				pfile.LightMonitorParam.CorrectionValueR = num2;
				pfile.LightMonitorParam.ValueChangeFlag = true;
			}
			num = 0;
			num2 = 0;
			num3 = 0;
			for (int j = 0; j < LightMonitorG.Count; j++)
			{
				num += LightMonitorG[j];
				num3++;
			}
			if (num3 > 0)
			{
				num2 = (int)((double)num / (double)num3);
				pfile.LightMonitorParam.CorrectionValueG = num2;
				pfile.LightMonitorParam.ValueChangeFlag = true;
			}
			num = 0;
			num2 = 0;
			num3 = 0;
			for (int k = 0; k < LightMonitorB.Count; k++)
			{
				num += LightMonitorB[k];
				num3++;
			}
			if (num3 > 0)
			{
				num2 = (int)((double)num / (double)num3);
				pfile.LightMonitorParam.CorrectionValueB = num2;
				pfile.LightMonitorParam.ValueChangeFlag = true;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("SW04-LightMonitor-UpdateLightMonitorBaseValue()-" + ex.ToString());
		}
	}

	public List<int> GetLightMonitorRGB(int id)
	{
		return id switch
		{
			0 => LightMonitorR, 
			1 => LightMonitorG, 
			2 => LightMonitorB, 
			_ => LightMonitorR, 
		};
	}

	public void GetLightMonitorData(ref List<int> Rdata, ref List<int> Gdata, ref List<int> Bdata)
	{
		Rdata.Clear();
		Gdata.Clear();
		Bdata.Clear();
		Rdata = LightMonitorR.ToList();
		Gdata = LightMonitorG.ToList();
		Bdata = LightMonitorB.ToList();
	}

	public List<int> GetLightMonitorThd()
	{
		return LightMonitorThd;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool GetGainShfitValue(ref int GainShift, ref string result, int StationIndex)
	{
		if (!pfile.LightMonitorParam.LightCorrection_ON)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站光源校正系統尚未啟用。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站光源校正系统尚未启用。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}站光源校正システムはまだ有効になっていません。", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Light Monitor Function are not enabled.", StationIndex + 1);
			}
			result = ResultStr;
			return true;
		}
		if (LightMonitorThd.Count <= 0)
		{
			if (buffers.LanguageType == 0)
			{
				ResultStr = string.Format("Station{0} Monitor value count is 0.\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站光源偵測值合格數為0\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("第{0}站光源侦测值合格数为0\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		if (pfile.LightMonitorParam.CorrectionBasicValue <= 0)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站尚未設定光源校正基準值。\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站尚未设定光源校正基准值。\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}ステーションは光源校正基準値が設定されていません。\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Light Correction Basic value are not setup.\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		ResultStr = "";
		if (LightMonitorThd.Count > 10)
		{
			StdFilter(ref LightMonitorThd);
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < LightMonitorThd.Count; i++)
		{
			num += LightMonitorThd[i];
		}
		num2 = num / LightMonitorThd.Count;
		int num3 = pfile.LightMonitorParam.CorrectionBasicValue - num2;
		GainShift = num3 / pfile.LightMonitorParam.GainPerValue;
		if (buffers.LanguageType == 1)
		{
			ResultStr = string.Format("第{0}站光源校正完成。(校正設定值:{1}, 當批平均值:{2}, Gain值修正:{3})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionBasicValue, num2, GainShift);
		}
		else if (buffers.LanguageType == 2)
		{
			ResultStr = string.Format("第{0}站光源校正完成。(校正设定值:{1}, 当批平均值:{2}, Gain值修正:{3})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionBasicValue, num2, GainShift);
		}
		else if (buffers.LanguageType == 3)
		{
			ResultStr = string.Format("第{0}站光源校正完了。(校正設定値:{1}, 当批平均値:{2}, Gain値修正:{3})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionBasicValue, num2, GainShift);
		}
		else
		{
			ResultStr = string.Format("Station{0} Light Correction Finish。(Setup Value:{1}, Current Value:{2}, Gain Value Fixed:{3})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionBasicValue, num2, GainShift);
		}
		result = ResultStr;
		ClearLightMonitorThd();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool GetCorrectionShiftValue(ref LightCorrectonDiffValue DValue, ref string ResultStr, int StationIndex)
	{
		try
		{
			ResultStr = "";
			if (LightMonitorR.Count <= 0 || LightMonitorG.Count <= 0 || LightMonitorB.Count <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr = string.Format("第{0}站光源偵測值合格數為0(R={1}, G={2}, B={3})\n", StationIndex + 1, LightMonitorR.Count, LightMonitorG.Count, LightMonitorB.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr = string.Format("第{0}站光源侦测值合格数为0\n", StationIndex + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr = string.Format("第{0}站光源検出値合格数が0です\n", StationIndex + 1);
				}
				else
				{
					ResultStr = string.Format("Station{0} Monitor value count is 0.\n", StationIndex + 1);
				}
				return false;
			}
			if (pfile.LightMonitorParam.CorrectionValueR == -1 || pfile.LightMonitorParam.CorrectionValueG == -1 || pfile.LightMonitorParam.CorrectionValueB == -1)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr = string.Format("第{0}站尚未設定光源校正基準值。\n", StationIndex + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr = string.Format("第{0}站尚未设定光源校正基准值。\n", StationIndex + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr = string.Format("第{0}站は光源校正基準値が設定されていません。\n", StationIndex + 1);
				}
				else
				{
					ResultStr = string.Format("Station{0} Light Correction Basic value are not setup.\n", StationIndex + 1);
				}
				return false;
			}
			if (LightMonitorR.Count > 10)
			{
				StdFilter(ref LightMonitorR);
			}
			if (LightMonitorG.Count > 10)
			{
				StdFilter(ref LightMonitorG);
			}
			if (LightMonitorB.Count > 10)
			{
				StdFilter(ref LightMonitorB);
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			if (LightMonitorR.Count > 0)
			{
				for (int i = 0; i < LightMonitorR.Count; i++)
				{
					num += LightMonitorR[i];
				}
				num2 = num / LightMonitorR.Count;
			}
			else
			{
				num2 = 0;
			}
			int value = (DValue.DiffR = num2 - pfile.LightMonitorParam.CorrectionValueR);
			DValue.MinDiffR = (int)(255.0 * pfile.LightMonitorParam.CorrectionRatioR * -1.0 / 100.0);
			DValue.MaxDiffR = (int)(255.0 * pfile.LightMonitorParam.CorrectionRatioR / 100.0);
			num = 0;
			if (LightMonitorG.Count > 0)
			{
				for (int j = 0; j < LightMonitorG.Count; j++)
				{
					num += LightMonitorG[j];
				}
				num3 = num / LightMonitorG.Count;
			}
			else
			{
				num3 = 0;
			}
			int value2 = (DValue.DiffG = num3 - pfile.LightMonitorParam.CorrectionValueG);
			DValue.MinDiffG = (int)(255.0 * pfile.LightMonitorParam.CorrectionRatioG * -1.0 / 100.0);
			DValue.MaxDiffG = (int)(255.0 * pfile.LightMonitorParam.CorrectionRatioG / 100.0);
			num = 0;
			if (LightMonitorB.Count > 0)
			{
				for (int k = 0; k < LightMonitorB.Count; k++)
				{
					num += LightMonitorB[k];
				}
				num4 = num / LightMonitorB.Count;
			}
			else
			{
				num4 = 0;
			}
			int value3 = (DValue.DiffB = num4 - pfile.LightMonitorParam.CorrectionValueB);
			DValue.MinDiffB = (int)(255.0 * pfile.LightMonitorParam.CorrectionRatioB * -1.0 / 100.0);
			DValue.MaxDiffB = (int)(255.0 * pfile.LightMonitorParam.CorrectionRatioB / 100.0);
			double num5 = (double)Math.Abs(value) / 255.0 * 100.0;
			double num6 = (double)Math.Abs(value2) / 255.0 * 100.0;
			double num7 = (double)Math.Abs(value3) / 255.0 * 100.0;
			bool result = true;
			if (num5 > pfile.LightMonitorParam.CorrectionRatioR)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站紅色光源校正失敗。色階差異超過警報比例(設定警報比例:{1:N1}, 當批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num5);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站红色光源校正失败。色阶差异超过警报比例(设定警报比例:{1:N1}, 当批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num5);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の赤色光源校正に失敗しました。色階差が警告比率を超えています（設定警告比率:{1:N1}、このバッチの色差比率:{2:N1}）\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num5);
				}
				else
				{
					ResultStr += string.Format("Red light source calibration failed at station {0}. Color difference exceeds the alert ratio (Set alert ratio: {1:N1}, Batch color difference ratio: {2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num5);
				}
				result = false;
			}
			if (num6 > pfile.LightMonitorParam.CorrectionRatioG)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站綠色光源校正失敗。色階差異超過警報比例(設定警報比例:{1:N1}, 當批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num6);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站绿色光源校正失败。色阶差异超过警报比例(设定警报比例:{1:N1}, 当批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num6);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の緑色光源校正に失敗しました。階調差が警告比率を超えています（設定警告比率:{1:N1}、このバッチの色差比率:{2:N1}）\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num6);
				}
				else
				{
					ResultStr += string.Format("Green light source calibration failed at station {0}. Color difference exceeds the alert ratio (Set alert ratio: {1:N1}, Batch color difference ratio: {2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num6);
				}
				result = false;
			}
			if (num7 > pfile.LightMonitorParam.CorrectionRatioB)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站藍色光源校正失敗。色階差異超過警報比例(設定警報比例:{1:N1}, 當批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num7);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站蓝色光源校正失败。色阶差异超过警报比例(设定警报比例:{1:N1}, 当批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num7);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}站の青色光源校正に失敗しました。階調差が警報比率を超えています（設定警報比率:{1:N1}、当批色差比率:{2:N1}）\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num7);
				}
				else
				{
					ResultStr += string.Format("Blue light source calibration failed at station {0}. Color difference exceeds the alert ratio (Set alert ratio: {1:N1}, Batch color difference ratio: {2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num7);
				}
				result = false;
			}
			ClearLightMonitorThd();
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("LightMonitor-GetCorrectionShiftValue()-00-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool GetVoltageShfitValue(ref double ShiftB, ref double ShiftG, ref double ShiftR, ref string result, int StationIndex)
	{
		if (LightMonitorR.Count <= 0 || LightMonitorG.Count <= 0 || LightMonitorB.Count <= 0)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站光源偵測值合格數為0(R={1}, G={2}, B={3})\n", StationIndex + 1, LightMonitorR.Count, LightMonitorG.Count, LightMonitorB.Count);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站光源侦测值合格数为0\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}站光源検出値合格数が0です\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Monitor value count is 0.\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		if (pfile.LightMonitorParam.CorrectionValueR == -1 || pfile.LightMonitorParam.CorrectionValueG == -1 || pfile.LightMonitorParam.CorrectionValueB == -1)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站尚未設定光源校正基準值。\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站尚未设定光源校正基准值。\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}站は光源校正基準値が設定されていません。\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Light Correction Basic value are not setup.\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		ResultStr = "";
		if (LightMonitorR.Count > 10)
		{
			StdFilter(ref LightMonitorR);
		}
		if (LightMonitorG.Count > 10)
		{
			StdFilter(ref LightMonitorG);
		}
		if (LightMonitorB.Count > 10)
		{
			StdFilter(ref LightMonitorB);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (LightMonitorR.Count > 0)
		{
			for (int i = 0; i < LightMonitorR.Count; i++)
			{
				num += LightMonitorR[i];
			}
			num2 = num / LightMonitorR.Count;
		}
		else
		{
			num2 = 0;
		}
		int num5 = pfile.LightMonitorParam.CorrectionValueR - num2;
		num = 0;
		if (LightMonitorG.Count > 0)
		{
			for (int j = 0; j < LightMonitorG.Count; j++)
			{
				num += LightMonitorG[j];
			}
			num3 = num / LightMonitorG.Count;
		}
		else
		{
			num3 = 0;
		}
		int num6 = pfile.LightMonitorParam.CorrectionValueG - num3;
		num = 0;
		if (LightMonitorB.Count > 0)
		{
			for (int k = 0; k < LightMonitorB.Count; k++)
			{
				num += LightMonitorB[k];
			}
			num4 = num / LightMonitorB.Count;
		}
		else
		{
			num4 = 0;
		}
		int num7 = pfile.LightMonitorParam.CorrectionValueB - num4;
		if (pfile.LightMonitorParam.VoltagePerValueR != 0)
		{
			ShiftR = (double)num5 / (double)pfile.LightMonitorParam.VoltagePerValueR * 0.1;
		}
		else
		{
			ShiftR = 0.0;
		}
		if (pfile.LightMonitorParam.VoltagePerValueG != 0)
		{
			ShiftG = (double)num6 / (double)pfile.LightMonitorParam.VoltagePerValueG * 0.1;
		}
		else
		{
			ShiftG = 0.0;
		}
		if (pfile.LightMonitorParam.VoltagePerValueB != 0)
		{
			ShiftB = (double)num7 / (double)pfile.LightMonitorParam.VoltagePerValueB * 0.1;
		}
		else
		{
			ShiftB = 0.0;
		}
		double num8 = (double)num5 / 255.0;
		double num9 = (double)num6 / 255.0;
		double num10 = (double)num7 / 255.0;
		bool flag = true;
		if (num8 > pfile.LightMonitorParam.CorrectionRatioR)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr += string.Format("第{0}站紅色光源校正失敗。色階差異超過警報比例(設定警報比例:{1:N1}, 當批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num8);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr += string.Format("第{0}站红色光源校正失败。色阶差异超过警报比例(设定警报比例:{1:N1}, 当批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num8);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr += string.Format("第{0}面の赤色光源校正に失敗しました。色階差が警告比率を超えています（設定警告比率:{1:N1}、このバッチの色差比率:{2:N1}）\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num8);
			}
			else
			{
				ResultStr += string.Format("Red light source calibration failed at station {0}. Color difference exceeds the alert ratio (Set alert ratio: {1:N1}, Batch color difference ratio: {2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioR, num8);
			}
			flag = false;
		}
		if (num9 > pfile.LightMonitorParam.CorrectionRatioG)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr += string.Format("第{0}站綠色光源校正失敗。色階差異超過警報比例(設定警報比例:{1:N1}, 當批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num9);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr += string.Format("第{0}站绿色光源校正失败。色阶差异超过警报比例(设定警报比例:{1:N1}, 当批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num9);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr += string.Format("第{0}面の緑色光源校正に失敗しました。階調差が警告比率を超えています（設定警告比率:{1:N1}、このバッチの色差比率:{2:N1}）\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num9);
			}
			else
			{
				ResultStr += string.Format("Green light source calibration failed at station {0}. Color difference exceeds the alert ratio (Set alert ratio: {1:N1}, Batch color difference ratio: {2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioG, num9);
			}
			flag = false;
		}
		if (num10 > pfile.LightMonitorParam.CorrectionRatioB)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr += string.Format("第{0}站藍色光源校正失敗。色階差異超過警報比例(設定警報比例:{1:N1}, 當批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num10);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr += string.Format("第{0}站蓝色光源校正失败。色阶差异超过警报比例(设定警报比例:{1:N1}, 当批色差比例:{2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num10);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr += string.Format("第{0}站の青色光源校正に失敗しました。階調差が警報比率を超えています（設定警報比率:{1:N1}、当批色差比率:{2:N1}）\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num10);
			}
			else
			{
				ResultStr += string.Format("Blue light source calibration failed at station {0}. Color difference exceeds the alert ratio (Set alert ratio: {1:N1}, Batch color difference ratio: {2:N1})\n", StationIndex + 1, pfile.LightMonitorParam.CorrectionRatioB, num10);
			}
			flag = false;
		}
		if (!flag)
		{
			return flag;
		}
		if (buffers.LanguageType == 1)
		{
			ResultStr = string.Format("第{0}站光源校正完成。(紅色光源修正:{1}, 綠色光源修正:{2}, 藍色光源修正:{3})\n", StationIndex + 1, ShiftR, ShiftG, ShiftB);
		}
		else if (buffers.LanguageType == 2)
		{
			ResultStr = string.Format("第{0}站光源校正完成。(红色光源修正:{1}, 绿色光源修正:{2}, 蓝色光源修正:{3})\n", StationIndex + 1, ShiftR, ShiftG, ShiftB);
		}
		else if (buffers.LanguageType == 3)
		{
			ResultStr = string.Format("第{0}ステーション光源校正完了。(赤色光源修正:{1}, 緑色光源修正:{2}, 青色光源修正:{3})\n", StationIndex + 1, ShiftR, ShiftG, ShiftB);
		}
		else
		{
			ResultStr = string.Format("Calibration of light source at station {0} completed. (Red light correction: {1}, Green light correction: {2}, Blue light correction: {3})\n", StationIndex + 1, ShiftR, ShiftG, ShiftB);
		}
		LightMonitorR.Add(num2);
		LightMonitorG.Add(num3);
		LightMonitorB.Add(num4);
		result = ResultStr;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool SetLightCorrectionValue()
	{
		if (LightMonitorThd.Count <= 0)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("光源偵測值合格數為0\n");
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("光源侦测值合格数为0\n");
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("光源検出値合格数が0です\n");
			}
			else
			{
				ResultStr = string.Format("Monitor value count is 0.\n");
			}
			return false;
		}
		if (LightMonitorThd.Count > 10)
		{
			StdFilter(ref LightMonitorThd);
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < LightMonitorThd.Count; i++)
		{
			num += LightMonitorThd[i];
		}
		num2 = num / LightMonitorThd.Count;
		pfile.LightMonitorParam.CorrectionBasicValue = num2;
		pfile.LightMonitorParam.ValueChangeFlag = true;
		pfile.WriteLightMonitorParam();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool StartSoftCorrection(ref string result, int StationIndex)
	{
		if (!pfile.LightMonitorParam.EnableSoftCorrection)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站軟體校正系統尚未啟用。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站软体校正系统尚未启用。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}面のソフトウェア校正システムはまだ有効になっていません。", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Soft Monitor Function are not enabled.", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		ClearLightMonitorThd();
		return true;
	}

	public bool EndSoftCorrection(ref string result, int StationIndex)
	{
		return GetSoftCorrectionValue(ref result, StationIndex);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetSoftCorrectionValue(ref string result, int StationIndex)
	{
		int num2;
		int num = (num2 = 0);
		if (!pfile.LightMonitorParam.EnableSoftCorrection)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站軟體校正系統尚未啟用。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站软体校正系统尚未启用。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}面のソフトウェア校正システムはまだ有効になっていません。", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Soft Monitor Function are not enabled.", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		if (LightMonitorR.Count <= 0 || LightMonitorG.Count <= 0 || LightMonitorB.Count <= 0)
		{
			if (buffers.LanguageType == 0)
			{
				ResultStr = string.Format("Station{0} Monitor value count is 0.\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站軟體偵測值合格數為0\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("第{0}站软体侦测值合格数为0\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		if (pfile.SoftCorrSetup_R <= 0 && pfile.SoftCorrSetup_G <= 0 && pfile.SoftCorrSetup_B <= 0)
		{
			if (buffers.LanguageType == 0)
			{
				ResultStr = string.Format("Station{0} Soft Correction Basic value are not setup.\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站尚未設定軟體校正基準值。\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("第{0}站尚未设定软体校正基准值。\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		ResultStr = "";
		if (LightMonitorR.Count > 10)
		{
			StdFilter(ref LightMonitorR);
		}
		if (LightMonitorG.Count > 10)
		{
			StdFilter(ref LightMonitorG);
		}
		if (LightMonitorB.Count > 10)
		{
			StdFilter(ref LightMonitorB);
		}
		if (LightMonitorR.Count <= 0 || LightMonitorG.Count <= 0 || LightMonitorB.Count <= 0)
		{
			if (buffers.LanguageType == 0)
			{
				ResultStr = string.Format("Station{0} Monitor value count is 0.(After std.)\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站軟體偵測值，標準差後合格數為0\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("第{0}站软体侦测值，标准差后合格数为0\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		int num3 = 0;
		for (int i = 0; i < LightMonitorR.Count; i++)
		{
			num3 += LightMonitorR[i];
		}
		int num4 = num3 / LightMonitorR.Count;
		int num5 = 0;
		for (int j = 0; j < LightMonitorG.Count; j++)
		{
			num5 += LightMonitorG[j];
		}
		num = num5 / LightMonitorG.Count;
		int num6 = 0;
		for (int k = 0; k < LightMonitorB.Count; k++)
		{
			num6 += LightMonitorB[k];
		}
		num2 = num6 / LightMonitorB.Count;
		if (buffers.LanguageType == 0)
		{
			ResultStr = string.Format("Station{0} Soft Correction Finish。(Current Value R={1}, G={2}, B={3}\n", StationIndex + 1, num4, num, num2);
		}
		else if (buffers.LanguageType == 1)
		{
			ResultStr = string.Format("第{0}站軟體校正完成。(R={1}, G={2}, B={3})\n", StationIndex + 1, num4, num, num2);
		}
		else
		{
			ResultStr = string.Format("第{0}站软体校正完成。(R={1}, G={2}, B={3})\n", StationIndex + 1, num4, num, num2);
		}
		result = ResultStr;
		pfile.SoftCorrection_R = num4;
		pfile.SoftCorrection_G = num;
		pfile.SoftCorrection_B = num2;
		pfile.SoftShift_R = num4 - pfile.SoftCorrSetup_R;
		pfile.SoftShift_G = num - pfile.SoftCorrSetup_G;
		pfile.SoftShift_B = num2 - pfile.SoftCorrSetup_B;
		if (Math.Abs(pfile.SoftShift_R) <= pfile.SoftCorrectoinAllowDiff)
		{
			pfile.SoftShift_R = 0;
		}
		if (Math.Abs(pfile.SoftShift_G) <= pfile.SoftCorrectoinAllowDiff)
		{
			pfile.SoftShift_G = 0;
		}
		if (Math.Abs(pfile.SoftShift_B) <= pfile.SoftCorrectoinAllowDiff)
		{
			pfile.SoftShift_B = 0;
		}
		ClearLightMonitorThd();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(ref int AvgThd)
	{
		if (!pfile.LightMonitorParam.LightCorrection_ON && !pfile.LightMonitorParam.LightMonitor_ON)
		{
			return true;
		}
		ResultStr = "";
		try
		{
			Rect inspeRect = GetInspeRect();
			inspeRect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
			Mat mat = new Mat(buffers.GetChannelMat((int)pfile.LightMonitorParam.ChannelIndex), inspeRect);
			mat.CopyTo(Binary);
			AvgThd = (int)mat.Mean().Val0;
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("偵測區域平均值 = {0}\n\n", AvgThd);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("侦测区域平均值 = {0}\n\n", AvgThd);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("検出領域の平均値 = {0}\n\n", AvgThd);
			}
			else
			{
				ResultStr = string.Format("Monitor Area Average Thd. = {0}\n\n", AvgThd);
			}
			LightMonitorThd.Add(AvgThd);
			if (LightMonitorThd.Count >= MaxMonitorCount)
			{
				LightMonitorThd.RemoveAt(LightMonitorThd.Count - 1);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT01-LightMonitor-Inspect()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			ResultStr = text;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(ref int AvgR, ref int AvgG, ref int AvgB)
	{
		ResultStr = "";
		try
		{
			Rect inspeRect = GetInspeRect();
			inspeRect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
			Mat mat = new Mat(buffers.GetChannelMat(0), inspeRect);
			Mat mat2 = new Mat(buffers.GetChannelMat(1), inspeRect);
			Mat mat3 = new Mat(buffers.GetChannelMat(2), inspeRect);
			AvgR = (int)mat.Mean().Val0;
			AvgG = (int)mat2.Mean().Val0;
			AvgB = (int)mat3.Mean().Val0;
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("偵測區域平均值 R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("侦测区域平均值 R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("検出領域の平均値 R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			else
			{
				ResultStr = string.Format("Monitor Area R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			if (AvgR != 0)
			{
				LightMonitorR.Add(AvgR);
			}
			else
			{
				AvgR = 0;
			}
			if (AvgG != 0)
			{
				LightMonitorG.Add(AvgG);
			}
			else
			{
				AvgG = 0;
			}
			if (AvgB != 0)
			{
				LightMonitorB.Add(AvgB);
			}
			else
			{
				AvgB = 0;
			}
			if (LightMonitorR.Count >= MaxMonitorCount)
			{
				LightMonitorR.RemoveAt(LightMonitorThd.Count - 1);
			}
			if (LightMonitorG.Count >= MaxMonitorCount)
			{
				LightMonitorG.RemoveAt(LightMonitorThd.Count - 1);
			}
			if (LightMonitorB.Count >= MaxMonitorCount)
			{
				LightMonitorB.RemoveAt(LightMonitorThd.Count - 1);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT01-LightMonitor-Inspect()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			ResultStr = text;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool InspectRGB()
	{
		ResultStr = "";
		try
		{
			Rect inspeRect = GetInspeRect();
			inspeRect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
			int num = (int)new Mat(buffers.GetChannelMat(0), inspeRect).Mean().Val0;
			LightMonitorR.Add(num);
			int num2 = (int)new Mat(buffers.GetChannelMat(1), inspeRect).Mean().Val0;
			LightMonitorG.Add(num2);
			int num3 = (int)new Mat(buffers.GetChannelMat(2), inspeRect).Mean().Val0;
			LightMonitorB.Add(num3);
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("偵測區域平均值 R = {0}, G = {1}, B = {2}\n\n", num, num2, num3);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("侦测区域平均值 R = {0}, G = {1}, B = {2}\n\n", num, num2, num3);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("検出領域の平均値 R = {0}, G = {1}, B = {2}\n\n", num, num2, num3);
			}
			else
			{
				ResultStr = string.Format("Monitor Area Average Thd. R = {0}, G = {1}, B = {2}\n\n", num, num2, num3);
			}
			if (LightMonitorR.Count >= MaxMonitorCount)
			{
				LightMonitorR.RemoveAt(LightMonitorR.Count - 1);
			}
			if (LightMonitorG.Count >= MaxMonitorCount)
			{
				LightMonitorG.RemoveAt(LightMonitorG.Count - 1);
			}
			if (LightMonitorB.Count >= MaxMonitorCount)
			{
				LightMonitorB.RemoveAt(LightMonitorB.Count - 1);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT01-LightMonitor-InspectRGB()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			ResultStr = text;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool InspectRGB(ref int AvgR, ref int AvgG, ref int AvgB, ref string ResultStr)
	{
		ResultStr = "";
		try
		{
			Rect inspeRect = GetInspeRect();
			inspeRect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
			AvgR = (int)new Mat(buffers.GetChannelMat(0), inspeRect).Mean().Val0;
			AvgG = (int)new Mat(buffers.GetChannelMat(1), inspeRect).Mean().Val0;
			AvgB = (int)new Mat(buffers.GetChannelMat(2), inspeRect).Mean().Val0;
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("軟體校正監控值寫入成功，偵測區域平均值 R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("软体校正监控值写入成功，侦测区域平均值 R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("ソフトウェア校正監視値の書き込みに成功しました。検出領域の平均値 R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			else
			{
				ResultStr = string.Format("Set Sort Correction Setup Value Finish. Monitor Area Average Thd. R = {0}, G = {1}, B = {2}\n\n", AvgR, AvgG, AvgB);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT01-LightMonitor-Inspect()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			ResultStr = text;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool LightMonitorCheck(ref string result, int StationIndex)
	{
		if (LightMonitorR.Count <= 0 || LightMonitorG.Count <= 0 || LightMonitorB.Count <= 0)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站光源偵測值合格數為0\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站光源侦测值合格数为0\n", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}站光源検出値合格数が0です\n", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0} Monitor value count is 0.\n", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		if (pfile.LightMonitorParam.MonitorValueR < 0 || pfile.LightMonitorParam.MonitorValueG < 0 || pfile.LightMonitorParam.MonitorValueB < 0)
		{
			if (buffers.LanguageType == 1)
			{
				ResultStr = string.Format("第{0}站尚未設定監控基準值。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultStr = string.Format("第{0}站尚未设定监控基准值。。", StationIndex + 1);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultStr = string.Format("第{0}面監視基準値が設定されていません。", StationIndex + 1);
			}
			else
			{
				ResultStr = string.Format("Station{0}Light Monitor Value are not setup.", StationIndex + 1);
			}
			result = ResultStr;
			return false;
		}
		ResultStr = "";
		bool flag = true;
		try
		{
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			for (int i = 0; i < LightMonitorR.Count; i++)
			{
				num7 += LightMonitorR[i];
			}
			num4 = num7 / LightMonitorR.Count;
			num = (double)Math.Abs(pfile.LightMonitorParam.MonitorValueR - num4) / 255.0 * 100.0;
			num7 = 0;
			for (int j = 0; j < LightMonitorG.Count; j++)
			{
				num7 += LightMonitorG[j];
			}
			num5 = num7 / LightMonitorG.Count;
			num2 = (double)Math.Abs(pfile.LightMonitorParam.MonitorValueG - num5) / 255.0 * 100.0;
			num7 = 0;
			for (int k = 0; k < LightMonitorB.Count; k++)
			{
				num7 += LightMonitorB[k];
			}
			num6 = num7 / LightMonitorB.Count;
			num3 = (double)Math.Abs(pfile.LightMonitorParam.MonitorValueB - num6) / 255.0 * 100.0;
			ResultStr = "";
			if (num > pfile.LightMonitorParam.AlarmRatioR)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站紅色光源監控值超過設定值，請檢查光源亮度是否衰減。(光源監控基準值:{1}, 色階差異警報設定比例:{2}, 目前光源平均數值:{3}, 差異值{4:N2}, 監控數:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站红色光源监控值超过设定值，请检查光源亮度是否衰减。(光源监控基准值:{1}, 色阶差异警报设定比例:{2}, 目前光源平均数值:{3}, 差异值{4:N2}, 监控数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("{0}面の赤色光源の監視値が異常です。光源の輝度が低下している可能性があります。詳細は以下の通りです。（基準値:{1}、警報比率:{2}、平均値:{3}、差異:{4:N2}、監視回数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				else
				{
					ResultStr += string.Format("Station{0} Red Light Monitor Value less than defult value, Please check LED Light。(Monitor Value:{1}, Alarm setup Value:{2}, Now:{3}, Difference:{4:N2}, Total:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				result = ResultStr;
				flag = false;
			}
			else
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站红色光源監控數值合格。(光源監控基準值:{1}, 色階差異警報設定比例:{2}, 目前光源平均數值:{3}, 差異值{4:N2}, 監控數:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站红色監控數值合格。(光源监控基准值:{1}, 色阶差异警报设定比例:{2}, 目前光源平均数值:{3}, 差异值{4:N2}, 监控数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の赤色監視数値が合格です。(光源監視基準値:{1}, 色階差異アラーム設定比率:{2}, 現在の光源平均数値:{3}, 差異値{4:N2}, 監視数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				else
				{
					ResultStr += string.Format("Station{0} Red Light Monitor Value check ok。(Monitor Value:{1}, Alarm setup Value:{2}, Now:{3}, Difference{4:N2}, Total:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueR, pfile.LightMonitorParam.AlarmRatioR, num4, num, LightMonitorR.Count);
				}
				result = ResultStr;
				flag = true;
			}
			if (num2 > pfile.LightMonitorParam.AlarmRatioG)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站綠色光源監控值超過設定值，請檢查光源亮度是否衰減。(光源監控基準值:{1}, 色階差異警報設定比例:{2}, 目前光源平均數值:{3}, 差異值{4:N2}, 監控數:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站绿色光源监控值超过设定值，请检查光源亮度是否衰减。(光源监控基准值:{1}, 色阶差异警报设定比例:{2}, 目前光源平均数值:{3}, 差异值{4:N2}, 监控数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の緑色光源監視値が設定値を超過しました。光源の輝度が低下していないか確認してください。(光源監視基準値:{1}, 階調差アラーム設定比率:{2}, 現在の光源平均値:{3}, 差異値{4:N2}, 監視数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				else
				{
					ResultStr += string.Format("Station{0} Green Light Monitor Value less than defult value, Please check LED Light。(Monitor Value:{1}, Alarm setup Value:{2}, Now:{3}, Difference:{4:N2}, Total:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				result = ResultStr;
				flag = false;
			}
			else
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站綠色光源監控數值合格。(光源監控基準值:{1}, 色階差異警報設定比例:{2}, 目前光源平均數值:{3}, 差異值{4:N2}, 監控數:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站绿色監控數值合格。(光源监控基准值:{1}, 色阶差异警报设定比例:{2}, 目前光源平均数值:{3}, 差异值{4:N2}, 监控数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の緑色監視数値が合格です。(光源監視基準値:{1}, 色階差異警報設定比率:{2}, 現在の光源平均値:{3}, 差異値{4:N2}, 監視数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				else
				{
					ResultStr += string.Format("Station{0} Green Light Monitor Value check ok。(Monitor Value:{1}, Alarm setup Value:{2}, Now:{3}, Difference{4:N2}, Total:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueG, pfile.LightMonitorParam.AlarmRatioG, num5, num2, LightMonitorG.Count);
				}
				result = ResultStr;
				flag = true;
			}
			if (num3 > pfile.LightMonitorParam.AlarmRatioB)
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站藍色光源監控值超過設定值，請檢查光源亮度是否衰減。(光源監控基準值:{1}, 色階差異警報設定比例:{2}, 目前光源平均數值:{3}, 差異值{4:N2}, 監控數:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站蓝色光源监控值超过设定值，请检查光源亮度是否衰减。(光源监控基准值:{1}, 色阶差异警报设定比例:{2}, 目前光源平均数值:{3}, 差异值{4:N2}, 监控数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の青色光源監視値が設定値を超えています。光源の明るさが減衰している可能性があります。（光源監視基準値:{1}, 色階差警報設定比率:{2}, 現在の光源平均値:{3}, 差異値{4:N2}, 監視数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				else
				{
					ResultStr += string.Format("Station{0} Blue Light Monitor Value less than defult value, Please check LED Light。(Monitor Value:{1}, Alarm setup Value:{2}, Now:{3}, Difference:{4:N2}, Total:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				result = ResultStr;
				flag = false;
			}
			else
			{
				if (buffers.LanguageType == 1)
				{
					ResultStr += string.Format("第{0}站藍色光源監控數值合格。(光源監控基準值:{1}, 色階差異警報設定比例:{2}, 目前光源平均數值:{3}, 差異值{4:N2}, 監控數:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultStr += string.Format("第{0}站蓝色監控數值合格。(光源监控基准值:{1}, 色阶差异警报设定比例:{2}, 目前光源平均数值:{3}, 差异值{4:N2}, 监控数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultStr += string.Format("第{0}面の青色監視数値合格。（光源監視基準値:{1}, 色階差異警報設定比率:{2}, 現在光源平均数値:{3}, 差異値{4:N2}, 監視数:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				else
				{
					ResultStr += string.Format("Station{0} Blue Light Monitor Value check ok。(Monitor Value:{1}, Alarm setup Value:{2}, Now:{3}, Difference{4:N2}, Total:{5})", StationIndex + 1, pfile.LightMonitorParam.MonitorValueB, pfile.LightMonitorParam.AlarmRatioB, num6, num3, LightMonitorB.Count);
				}
				result = ResultStr;
				flag = true;
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text = "MT01-LightMonitor-Inspect()-01-";
			text += ex.ToString();
			CVBuffer.SaveLog(text);
			ResultStr += text;
			result = ResultStr;
			return false;
		}
	}

	private void StdFilter(ref List<int> thd)
	{
		if (thd.Count <= 0)
		{
			return;
		}
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		List<int> list = new List<int>();
		for (int i = 0; i < thd.Count; i++)
		{
			num2 += (double)thd[i];
		}
		num = num2 / (double)thd.Count;
		for (int j = 0; j < thd.Count; j++)
		{
			num3 += Math.Pow((double)thd[j] - num, 2.0);
		}
		num4 = Math.Sqrt(num3 / (double)thd.Count);
		for (int k = 0; k < thd.Count; k++)
		{
			if (Math.Abs((double)thd[k] - num) <= num4)
			{
				list.Add(thd[k]);
			}
		}
		if (list.Count >= 2)
		{
			thd.Clear();
			for (int l = 0; l < list.Count; l++)
			{
				thd.Add(list[l]);
			}
		}
	}
}
