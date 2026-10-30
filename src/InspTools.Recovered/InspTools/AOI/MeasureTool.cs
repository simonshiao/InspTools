using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class MeasureTool
{
	private Mat[] Binary = new Mat[2];

	private Mat FinarBinary = new Mat();

	private CVBuffer buffers;

	private int MaxComDigitsItems = 24;

	public List<Point> ScanPtL = new List<Point>();

	public List<Point> ScanPtR = new List<Point>();

	public List<Point> ScanPtT = new List<Point>();

	public List<Point> ScanPtB = new List<Point>();

	public List<double> MeasureLength = new List<double>();

	private Mat MaskArea = new Mat();

	private ParamFile pfile;

	public Rect[] InspeRect = new Rect[2];

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public double MaxWidth { get; set; }

	public double AvgWidth { get; set; }

	public double MinWidth { get; set; }

	public double MaxHeight { get; set; }

	public double AvgHeight { get; set; }

	public double MinHeight { get; set; }

	public bool MeasureSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public int MaxError { get; protected set; }

	public int ContinueError { get; protected set; }

	public string ErrorMsg { get; set; }

	public MeasureTool(ParamFile paramfile)
	{
		MaxComDigitsItems = 24;
		pfile = paramfile;
		InspeRect[0] = new Rect(0, 0, 0, 0);
		InspeRect[1] = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 2; i++)
		{
			Binary[i] = new Mat();
		}
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool MeasureWidth(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		try
		{
			OffLine = offline;
			iresult = TOOL_RESULT.EM_OK;
			MaxWidth = 0.0;
			AvgWidth = 0.0;
			MinWidth = 0.0;
			if (!pfile.MeaWidthParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.MeaWidthParam[index].EnableSelectFace && FaceIndex != pfile.MeaWidthParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			MaxWidth = -1.0;
			AvgWidth = -1.0;
			MinWidth = -1.0;
			pfile.MeaWidthParam[index].Scan1AvgValue = 0;
			pfile.MeaWidthParam[index].Scan1MinValue = 0;
			pfile.MeaWidthParam[index].Scan1MaxValue = 0;
			pfile.MeaWidthParam[index].Scan2AvgValue = 0;
			pfile.MeaWidthParam[index].Scan2MinValue = 0;
			pfile.MeaWidthParam[index].Scan2MaxValue = 0;
			pfile.MeaWidthParam[index].LengthAvgValue = 0.0;
			pfile.MeaWidthParam[index].LengthMaxValue = 0.0;
			pfile.MeaWidthParam[index].LengthMinValue = 0.0;
			GetInspeRect(pfile.MeaWidthParam[index], measurewidth: true);
			InspeRect[0] &= new Rect(1, 1, buffers.LocatedSrc.Width - 1, buffers.LocatedSrc.Height - 1);
			InspeRect[1] &= new Rect(1, 1, buffers.LocatedSrc.Width - 1, buffers.LocatedSrc.Height - 1);
			if (!doThdImage(pfile.MeaWidthParam[index]))
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度][{0}:{1}] 檢測區域寬高小於0。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度][{0}:{1}] 检测区域宽高小于0。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定幅][{0}:{1}] 検出領域の幅または高さが0未満です。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width][{0}:{1}] Tracker Width and Height < 0.\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				return false;
			}
			int[] array = new int[2];
			int[] array2 = new int[2];
			int[] array3 = new int[2];
			if (!ScanVLine(directionL2R: pfile.MeaWidthParam[index].Direction1 == ScanDirection.EM_LEFT_RIGHT, BinaryROI: Binary[0], roi: InspeRect[0], edgept1: out ScanPtL, Min: out array[0], Max: out array2[0], Avg: out array3[0]))
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度-左邊][{0}:{1}] 掃描合格點數小於3。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度-左边][{0}:{1}] 扫描合格点数小于3。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定幅度-左][{0}:{1}] スキャン合格点数が3未満です。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width-Left][{0}:{1}] Scan pt < 3。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			pfile.MeaWidthParam[index].Scan1AvgValue = array3[0];
			pfile.MeaWidthParam[index].Scan1MinValue = array[0];
			pfile.MeaWidthParam[index].Scan1MaxValue = array2[0];
			if (!ScanVLine(directionL2R: pfile.MeaWidthParam[index].Direction2 == ScanDirection.EM_LEFT_RIGHT, BinaryROI: Binary[1], roi: InspeRect[1], edgept1: out ScanPtR, Min: out array[1], Max: out array2[1], Avg: out array3[1]))
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度-右邊][{0}:{1}] 掃描合格點數小於3。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度-右边][{0}:{1}] 扫描合格点数小于3。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[宽度計測-右側][{0}:{1}] スキャン合格点数が3未満です。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width-Right][{0}:{1}] Scan pt < 3。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			pfile.MeaWidthParam[index].Scan2AvgValue = array3[1];
			pfile.MeaWidthParam[index].Scan2MinValue = array[1];
			pfile.MeaWidthParam[index].Scan2MaxValue = array2[1];
			pfile.MeaWidthParam[index].LengthMinValue = Math.Abs(array[0] - array[1]);
			pfile.MeaWidthParam[index].LengthMaxValue = Math.Abs(array2[0] - array2[1]);
			pfile.MeaWidthParam[index].LengthAvgValue = Math.Abs(array3[0] - array3[1]);
			if (pfile.MeaWidthParam[index].UseSinglePt)
			{
				bool result = true;
				int num = 0;
				num = ((pfile.MeaWidthParam[index].Scan1Rule == MeasureCondition.EM_AVG_LENGTH) ? array3[0] : ((pfile.MeaWidthParam[index].Scan1Rule != MeasureCondition.EM_MIN_LENGTH) ? array2[0] : array[0]));
				int num2 = 0;
				num2 = ((pfile.MeaWidthParam[index].Scan2Rule == MeasureCondition.EM_AVG_LENGTH) ? array3[1] : ((pfile.MeaWidthParam[index].Scan2Rule != MeasureCondition.EM_MIN_LENGTH) ? array2[1] : array[1]));
				double num3 = (AvgWidth = (double)Math.Abs(num - num2) * pfile.PixelPerUM_X);
				double comDigitsValue = GetComDigitsValue(pfile.MeaWidthParam[index].SinglePtLengthMin);
				if (num3 < comDigitsValue)
				{
					iresult = TOOL_RESULT.EM_NG;
					result = false;
					if (pfile.MeasureUnit == 0)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg = string.Format("[量測寬度][{0}:{1}] 寬度{2}um小於設定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg = string.Format("[量测宽度][{0}:{1}] 宽度{2}um小于设定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg = string.Format("[測定幅度][{0}:{1}] 幅度{2}umは設定値{3}未満です。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
						}
						else
						{
							ErrorMsg = string.Format("[Measure Width][{0}:{1}] Width={2}um, < {3}.\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[量測寬度][{0}:{1}] 寬度{2}mm小於設定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[量测宽度][{0}:{1}] 宽度{2}mm小于设定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[測定幅度][{0}:{1}] 幅度{2}mmは設定値{3}未満です。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
					}
					else
					{
						ErrorMsg = string.Format("[Measure Width][{0}:{1}] Width={2}mm, < {3}.\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue);
					}
				}
				double comDigitsValue2 = GetComDigitsValue(pfile.MeaWidthParam[index].SinglePtLengthMax);
				if (num3 > comDigitsValue2)
				{
					iresult = TOOL_RESULT.EM_NG;
					result = false;
					if (pfile.MeasureUnit == 0)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg = string.Format("[量測寬度][{0}:{1}] 寬度{2}um大於設定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg = string.Format("[量测宽度][{0}:{1}] 宽度{2}um大于设定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg = string.Format("[幅測定幅][{0}:{1}] 幅{2}umは設定値{3}を超えています。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
						}
						else
						{
							ErrorMsg = string.Format("[Measure Width][{0}:{1}] Width={2}um < {3}.\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[量測寬度][{0}:{1}] 寬度{2}mm大於設定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[量测宽度][{0}:{1}] 宽度{2}mm大于设定值{3}。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[幅測定幅][{0}:{1}] 幅{2}mmは設定値{3}を超えています。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
					}
					else
					{
						ErrorMsg = string.Format("[Measure Width][{0}:{1}] Width={2}mm < {3}.\n", index + 1, pfile.MeaWidthParam[index].TrackerName, num3, comDigitsValue2);
					}
				}
				return result;
			}
			if (ScanPtL.Count <= 0 || ScanPtR.Count <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度][{0}:{1}] 掃描點數小於0。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度][{0}:{1}] 扫描点数小于0。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定幅][{0}:{1}] スキャン点数は0未満です。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width][{0}:{1}] scan pt < 0。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				return false;
			}
			int num5 = ((ScanPtL.Count <= ScanPtR.Count) ? ScanPtL.Count : ScanPtR.Count);
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			double num6 = 0.0;
			int num7 = 0;
			ContinueError = 0;
			MaxError = 0;
			MeasureLength.Clear();
			int num8 = 0;
			double num9 = 0.0;
			for (int i = 0; i < num5; i++)
			{
				if (ScanPtR[i].X == -1 || ScanPtL[i].X == -1)
				{
					if (pfile.MeaWidthParam[index].CheckEveryone)
					{
						MaxError++;
						num7++;
						if (num7 > ContinueError)
						{
							ContinueError = num7;
						}
					}
					continue;
				}
				num6 = (double)Math.Abs(ScanPtR[i].X - ScanPtL[i].X) * pfile.PixelPerUM_X;
				if (MaxWidth < num6)
				{
					MaxWidth = num6;
				}
				if (MinWidth < 0.0)
				{
					MinWidth = num6;
				}
				else if (MinWidth > num6)
				{
					MinWidth = num6;
				}
				num9 += num6;
				num8++;
				MeasureLength.Add(num6);
				double comDigitsValue3 = GetComDigitsValue(pfile.MeaWidthParam[index].LengthMin);
				double comDigitsValue4 = GetComDigitsValue(pfile.MeaWidthParam[index].LengthMax);
				if (num6 < comDigitsValue3 || num6 > comDigitsValue4)
				{
					MaxError++;
					num7++;
					if (num7 > ContinueError)
					{
						ContinueError = num7;
					}
				}
				else
				{
					num7 = 0;
				}
			}
			if (num8 > 0)
			{
				AvgWidth = num9 / (double)num8;
			}
			if (MaxError > pfile.MeaWidthParam[index].MaxError)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度][{0}:{1}] Min = {2:N3}, Max={3:N3} \n 全檢模式最大錯誤數超過設定值。\n ", index + 1, pfile.MeaWidthParam[index].TrackerName, MinWidth, MaxWidth);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度][{0}:{1}] 全检模式最大错误数超过设定值。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定幅][{0}:{1}] 全検モード最大エラー数超過設定値。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width][{0}:{1}] Total error over range in Full Check Mode.\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				flag = false;
			}
			if (ContinueError > pfile.MeaWidthParam[index].ContinueError)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度][{0}:{1}] Min = {2:N3}, Max={3:N3} \n 全檢模式連續錯誤數超過設定值。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, MinWidth, MaxWidth);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度][{0}:{1}] 全检模式连续错误数超过设定值。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定幅][{0}:{1}] 全検モード連続エラー数超過設定値。\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width][{0}:{1}] Continue error over range in Full Check Mode.\n", index + 1, pfile.MeaWidthParam[index].TrackerName);
				}
				flag2 = false;
			}
			if (MaxWidth - MinWidth > pfile.MeaWidthParam[index].LengthDiffMax && pfile.MeaWidthParam[index].LengthDiffMax != 0.0)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測寬度][{0}:{1}] Min = {2}, Max = {3:N3}, Max Diff = {4:N3}, 寬度高低差超過設定值。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, MinWidth, MaxWidth, MaxWidth - MinWidth);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测宽度][{0}:{1}] Min = {2}, Max = {3:N3}, Max Diff = {4:N3}, 宽度高低差超过设定值。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, MinWidth, MaxWidth, MaxWidth - MinWidth);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定幅][{0}:{1}] Min = {2}, Max = {3:N3}, Max Diff = {4:N3}, 幅の高低差が設定値を超えています。\n", index + 1, pfile.MeaWidthParam[index].TrackerName, MinWidth, MaxWidth, MaxWidth - MinWidth);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Width][{0}:{1}] Min = {2}, Max = {3:N3}, Max Diff = {4:N3}, Width min max diff over range.\n", index + 1, pfile.MeaWidthParam[index].TrackerName, MinWidth, MaxWidth, MaxWidth - MinWidth);
				}
				flag3 = false;
			}
			if (flag3 & flag2 & flag)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			string text = "MT01-MeasureTool-MeasureWidth()";
			text += ex.ToString();
			iresult = TOOL_RESULT.EM_NG;
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Measure-001");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool MeasureHeight(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		try
		{
			OffLine = offline;
			iresult = TOOL_RESULT.EM_OK;
			MaxHeight = 0.0;
			MinHeight = 0.0;
			AvgHeight = 0.0;
			if (!pfile.MeaHeightParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.MeaHeightParam[index].EnableSelectFace && FaceIndex != pfile.MeaHeightParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			GetInspeRect(pfile.MeaHeightParam[index], measurewidth: false);
			InspeRect[0] &= new Rect(1, 1, buffers.LocatedSrc.Width - 1, buffers.LocatedSrc.Height - 1);
			InspeRect[1] &= new Rect(1, 1, buffers.LocatedSrc.Width - 1, buffers.LocatedSrc.Height - 1);
			MaxHeight = -1.0;
			MinHeight = -1.0;
			AvgHeight = -1.0;
			pfile.MeaHeightParam[index].Scan1AvgValue = 0;
			pfile.MeaHeightParam[index].Scan1MinValue = 0;
			pfile.MeaHeightParam[index].Scan1MaxValue = 0;
			pfile.MeaHeightParam[index].Scan2AvgValue = 0;
			pfile.MeaHeightParam[index].Scan2MinValue = 0;
			pfile.MeaHeightParam[index].Scan2MaxValue = 0;
			pfile.MeaHeightParam[index].LengthAvgValue = 0.0;
			pfile.MeaHeightParam[index].LengthMaxValue = 0.0;
			pfile.MeaHeightParam[index].LengthMinValue = 0.0;
			if (!doThdImage(pfile.MeaHeightParam[index]))
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度][{0}:{1}] 檢測區域寬高小於0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度][{0}:{1}] 检测区域宽高小于0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ][{0}:{1}] 検出領域の幅または高さが0未満です。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Height][{0}:{1}] Tracker windth and Height < 0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				return false;
			}
			int[] array = new int[2];
			int[] array2 = new int[2];
			int[] array3 = new int[2];
			if (!ScanHLine(directionT2B: pfile.MeaHeightParam[index].Direction1 == ScanDirection.EM_TOP_BOTTOM, BinaryROI: Binary[0], roi: InspeRect[0], edgept1: out ScanPtT, Min: out array[0], Max: out array2[0], Avg: out array3[0]))
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度-上邊][{0}:{1}] 掃描合格點數小於3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度-上边][{0}:{1}] 扫描合格点数小于3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ-上辺][{0}:{1}] スキャン合格点数3未満。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Height-Top][{0}:{1}] Scan pass pt < 3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			pfile.MeaHeightParam[index].Scan1AvgValue = array3[0];
			pfile.MeaHeightParam[index].Scan1MinValue = array[0];
			pfile.MeaHeightParam[index].Scan1MaxValue = array2[0];
			if (!ScanHLine(directionT2B: pfile.MeaHeightParam[index].Direction2 == ScanDirection.EM_TOP_BOTTOM, BinaryROI: Binary[1], roi: InspeRect[1], edgept1: out ScanPtB, Min: out array[1], Max: out array2[1], Avg: out array3[1]))
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度-下邊][{0}:{1}] 掃描合格點數小於3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度-下边][{0}:{1}] 扫描合格点数小于3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ-下辺][{0}:{1}] スキャン合格点数＜3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure bottom-Top][{0}:{1}] Scan pass pt < 3。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			pfile.MeaHeightParam[index].Scan2AvgValue = array3[1];
			pfile.MeaHeightParam[index].Scan2MinValue = array[1];
			pfile.MeaHeightParam[index].Scan2MaxValue = array2[1];
			pfile.MeaHeightParam[index].LengthMinValue = Math.Abs(array[0] - array[1]);
			pfile.MeaHeightParam[index].LengthMaxValue = Math.Abs(array2[0] - array2[1]);
			pfile.MeaHeightParam[index].LengthAvgValue = Math.Abs(array3[0] - array3[1]);
			if (pfile.MeaHeightParam[index].UseSinglePt)
			{
				pfile.MeaHeightParam[index].LengthMinValue = Math.Abs(array[0] - array[1]);
				pfile.MeaHeightParam[index].LengthMaxValue = Math.Abs(array2[0] - array2[1]);
				pfile.MeaHeightParam[index].LengthAvgValue = Math.Abs(array3[0] - array3[1]);
				bool result = true;
				int num = 0;
				num = ((pfile.MeaHeightParam[index].Scan1Rule == MeasureCondition.EM_AVG_LENGTH) ? array3[0] : ((pfile.MeaHeightParam[index].Scan1Rule != MeasureCondition.EM_MIN_LENGTH) ? array2[0] : array[0]));
				int num2 = 0;
				num2 = ((pfile.MeaHeightParam[index].Scan2Rule == MeasureCondition.EM_AVG_LENGTH) ? array3[1] : ((pfile.MeaHeightParam[index].Scan2Rule != MeasureCondition.EM_MIN_LENGTH) ? array2[1] : array[1]));
				double num3 = (AvgHeight = (double)Math.Abs(num - num2) * pfile.PixelPerUM_Y);
				double comDigitsValue = GetComDigitsValue(pfile.MeaHeightParam[index].SinglePtLengthMin);
				if (num3 < comDigitsValue)
				{
					iresult = TOOL_RESULT.EM_NG;
					result = false;
					if (pfile.MeasureUnit == 0)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg = string.Format("[量測高度][{0}:{1}] 高度{2}um小於設定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg = string.Format("[量测高度][{0}:{1}] 高度{2}um小于设定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg = string.Format("[測定高さ][{0}:{1}] 高さ{2}umは設定値{3}未満です。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
						}
						else
						{
							ErrorMsg = string.Format("[Measure Height][{0}:{1}] Height={2}um, < Min value{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[量測高度][{0}:{1}] 高度{2}mm小於設定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[量测高度][{0}:{1}] 高度{2}mm小于设定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[測定高さ][{0}:{1}] 高さ{2}mmは設定値{3}未満です。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
					}
					else
					{
						ErrorMsg = string.Format("[Measure Height][{0}:{1}] Height={2}mm, < Min value{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue);
					}
					return result;
				}
				double comDigitsValue2 = GetComDigitsValue(pfile.MeaHeightParam[index].SinglePtLengthMax);
				if (num3 > comDigitsValue2)
				{
					iresult = TOOL_RESULT.EM_NG;
					result = false;
					if (pfile.MeasureUnit == 0)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg = string.Format("[量測高度][{0}:{1}] 高度{2}um大於設定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg = string.Format("[量测高度][{0}:{1}] 高度{2}um大于设定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg = string.Format("[測定高さ][{0}:{1}] 高さ{2}umは設定値{3}を超えています。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
						}
						else
						{
							ErrorMsg = string.Format("[Measure Height][{0}:{1}] Height={2}um, > max value{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[量測高度][{0}:{1}] 高度{2}mm大於設定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[量测高度][{0}:{1}] 高度{2}mm大于设定值{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[測定高さ][{0}:{1}] 高さ{2}mmは設定値{3}を超えています。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
					}
					else
					{
						ErrorMsg = string.Format("[Measure Height][{0}:{1}] Height={2}mm, > max value{3}。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, num3, comDigitsValue2);
					}
					return result;
				}
				return result;
			}
			if (ScanPtT.Count <= 0 || ScanPtB.Count <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度][{0}:{1}] 掃描點數小於0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度][{0}:{1}] 扫描点数小于0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ][{0}:{1}] スキャン点数<0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Height][{0}:{1}] Scan pass pt <= 0。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				return false;
			}
			int num5 = ((ScanPtT.Count <= ScanPtB.Count) ? ScanPtT.Count : ScanPtB.Count);
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			double num6 = 0.0;
			int num7 = 0;
			ContinueError = 0;
			MaxError = 0;
			MeasureLength.Clear();
			int num8 = 0;
			double num9 = 0.0;
			for (int i = 0; i < num5; i++)
			{
				if (ScanPtB[i].Y == -1 || ScanPtT[i].Y == -1)
				{
					if (pfile.MeaHeightParam[index].CheckEveryone)
					{
						MaxError++;
						num7++;
						if (num7 > ContinueError)
						{
							ContinueError = num7;
						}
					}
					continue;
				}
				num6 = (double)Math.Abs(ScanPtB[i].Y - ScanPtT[i].Y) * pfile.PixelPerUM_Y;
				MeasureLength.Add(num6);
				if (MaxHeight < num6)
				{
					MaxHeight = num6;
				}
				if (MinHeight < 0.0)
				{
					MinHeight = num6;
				}
				else if (MinHeight > num6)
				{
					MinHeight = num6;
				}
				num9 += num6;
				num8++;
				double comDigitsValue3 = GetComDigitsValue(pfile.MeaHeightParam[index].LengthMin);
				double comDigitsValue4 = GetComDigitsValue(pfile.MeaHeightParam[index].LengthMax);
				if (num6 < comDigitsValue3 || num6 > comDigitsValue4)
				{
					MaxError++;
					num7++;
					if (num7 > ContinueError)
					{
						ContinueError = num7;
					}
				}
				else
				{
					num7 = 0;
				}
			}
			if (num8 > 0)
			{
				AvgHeight = num9 / (double)num8;
			}
			if (MaxError > pfile.MeaHeightParam[index].MaxError)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度][{0}:{1}] Min={2}, Max={3} \n 全檢模式最大錯誤數超過設定值。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, MinHeight, MaxHeight);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度][{0}:{1}] 全检模式最大错误数超过设定值。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ][{0}:{1}] 全検モード最大エラー数超過設定値。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Height][{0}:{1}] Total error over range in Full Check Mode.\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				flag = false;
			}
			if (ContinueError > pfile.MeaHeightParam[index].ContinueError)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度][{0}:{1}] Min={2}, Max={3} \n 全檢模式連續錯誤數超過設定值。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, MinHeight, MaxHeight);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度][{0}:{1}] 全检模式连续错误数超过设定值。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ][{0}:{1}] 全検モード連続エラー数超過設定値。\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Height][{0}:{1}] Continue error over range in Full Check Mode.\n", index + 1, pfile.MeaHeightParam[index].TrackerName);
				}
				flag2 = false;
			}
			if (MaxHeight - MinHeight > pfile.MeaHeightParam[index].LengthDiffMax && pfile.MeaHeightParam[index].LengthDiffMax != 0.0)
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[量測高度][{0}:{1}] Min = {2:N3}, Max = {3:N3}, Max Diff = {4:N3}, 高度高低差超過設定值。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, MinHeight, MaxHeight, MaxHeight - MinHeight);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[量测高度][{0}:{1}] Min = {2:N3}, Max = {3:N3}, Max Diff = {4:N3}, 高度高低差超过设定值。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, MinHeight, MaxHeight, MaxHeight - MinHeight);
				}
				if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[測定高さ][{0}:{1}] Min = {2:N3}, Max = {3:N3}, Max Diff = {4:N3}, 高度差が設定値を超えています。\n", index + 1, pfile.MeaHeightParam[index].TrackerName, MinHeight, MaxHeight, MaxHeight - MinHeight);
				}
				else
				{
					ErrorMsg += string.Format("[Measure Height][{0}:{1}] Min = {2:N3}, Max = {3:N3}, Max Diff = {4:N3}, Height min max diff over range.\n", index + 1, pfile.MeaHeightParam[index].TrackerName, MinHeight, MaxHeight, MaxHeight - MinHeight);
				}
				flag3 = false;
			}
			if (flag3 & flag & flag2)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			string text = "MT02-MeasureTool-MeasureHeight()";
			text += ex.ToString();
			iresult = TOOL_RESULT.EM_NG;
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Measure-002");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool doThdImage(INSP_MEASURETOOL_PARAM param)
	{
		try
		{
			if (!param.TrackerON)
			{
				return true;
			}
			if (InspeRect[0].Width <= 0 || InspeRect[0].Height <= 0 || InspeRect[1].Width <= 0 || InspeRect[1].Height <= 0)
			{
				return false;
			}
			if (!param.EnableRGBThdA)
			{
				new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect[0]).CopyTo(Binary[0]);
				Cv2.InRange(Binary[0], param.THD1_Min, param.THD1_Max, Binary[0]);
			}
			else
			{
				Cv2.InRange(new Mat(buffers.LocatedSrc, InspeRect[0]), new Scalar(param.THD_AB_Min, param.THD_AG_Min, param.THD_AR_Min), new Scalar(param.THD_AB_Max, param.THD_AG_Max, param.THD_AR_Max), Binary[0]);
			}
			doMorph(ref Binary[0], (int)param.DE_Start1_Index, param.DE_Start1_Count);
			doMorph(ref Binary[0], (int)param.DE_End1_Index, param.DE_End1_Count);
			if (!param.EnableRGBThdB)
			{
				new Mat(buffers.GetChannelMat((int)param.THD2_ChannelIndex), InspeRect[1]).CopyTo(Binary[1]);
				Cv2.InRange(Binary[1], param.THD2_Min, param.THD2_Max, Binary[1]);
			}
			else
			{
				Cv2.InRange(new Mat(buffers.LocatedSrc, InspeRect[1]), new Scalar(param.THD_BB_Min, param.THD_BG_Min, param.THD_BR_Min), new Scalar(param.THD_BB_Max, param.THD_BG_Max, param.THD_BR_Max), Binary[1]);
			}
			doMorph(ref Binary[1], (int)param.DE_Start2_Index, param.DE_Start2_Count);
			doMorph(ref Binary[1], (int)param.DE_End2_Index, param.DE_End2_Count);
			int num = param.MaskSelectIndex - 1;
			if (num >= 0 && pfile.MaskParam[num].Mask1 != null)
			{
				buffers.GetChannelMat(0).CopyTo(MaskArea);
				MaskArea.SetTo(0.0);
				bool flag = false;
				Rect[] array = new Rect[2]
				{
					new Rect(pfile.MaskParam[num].Mask1Pt.X, pfile.MaskParam[num].Mask1Pt.Y, pfile.MaskParam[num].Mask1.Width, pfile.MaskParam[num].Mask1.Height),
					default(Rect)
				};
				array[0] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
				if (pfile.MaskParam[num].SearchRect2ON && num < 20 && pfile.MaskParam[num].Mask2 != null)
				{
					array[1] = new Rect(pfile.MaskParam[num].Mask2Pt.X, pfile.MaskParam[num].Mask2Pt.Y, pfile.MaskParam[num].Mask2.Width, pfile.MaskParam[num].Mask2.Height);
					array[1] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
				}
				else
				{
					array[1] = new Rect(0, 0, 0, 0);
				}
				if (array[0].Width > 0 && array[0].Height > 0)
				{
					using Mat m = new Mat(MaskArea, array[0]);
					using Mat mat = MM.CloneFrom(pfile.MaskParam[num].Mask1);
					mat.CopyTo(m);
					flag = true;
				}
				if (pfile.MaskParam[num].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num < 20)
				{
					using Mat m2 = new Mat(MaskArea, array[1]);
					using Mat mat2 = MM.CloneFrom(pfile.MaskParam[num].Mask2);
					mat2.CopyTo(m2);
					flag = true;
				}
				if (flag)
				{
					for (int i = 0; i < 2; i++)
					{
						if (Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
						{
							Mat mat3 = MM.newMat(MaskArea, InspeRect[i]).Clone();
							doMorph(ref mat3, (int)param.MaskDE_Start1_Index, param.MaskDE_Start1_Count);
							Cv2.BitwiseNot(mat3, mat3);
							Cv2.BitwiseAnd(Binary[i], mat3, Binary[i]);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			string text = "MT03-MeasureTool-doThdImage()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Measure-003");
			}
			return false;
		}
		return true;
	}

	public void GetInspeRect(INSP_MEASURETOOL_PARAM param, bool measurewidth)
	{
		LocateRect = buffers.LocatedRect;
		InspeRect[0].X = LocateRect.X + param.SearchRect1L;
		InspeRect[0].Y = LocateRect.Y + param.SearchRect1T;
		InspeRect[0].Width = LocateRect.Width - (param.SearchRect1L + param.SearchRect1R);
		InspeRect[0].Height = LocateRect.Height - (param.SearchRect1T + param.SearchRect1B);
		if (!param.UseSinglePt)
		{
			if (!measurewidth)
			{
				InspeRect[1].X = InspeRect[0].X;
				InspeRect[1].Width = InspeRect[0].Width;
				InspeRect[1].Y = LocateRect.Y + param.SearchRect2T;
				InspeRect[1].Height = LocateRect.Height - (param.SearchRect2T + param.SearchRect2B);
			}
			else
			{
				InspeRect[1].X = LocateRect.X + param.SearchRect2L;
				InspeRect[1].Width = LocateRect.Width - (param.SearchRect2L + param.SearchRect2R);
				InspeRect[1].Y = InspeRect[0].Y;
				InspeRect[1].Height = InspeRect[0].Height;
			}
		}
		else
		{
			InspeRect[1].X = LocateRect.X + param.SearchRect2L;
			InspeRect[1].Width = LocateRect.Width - (param.SearchRect2L + param.SearchRect2R);
			InspeRect[1].Y = LocateRect.Y + param.SearchRect2T;
			InspeRect[1].Height = LocateRect.Height - (param.SearchRect2T + param.SearchRect2B);
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
	{
		if (count != 0)
		{
			switch (index)
			{
			case 1:
				Cv2.Dilate(mat, mat, new Mat(), null, count);
				break;
			case 2:
				Cv2.Erode(mat, mat, new Mat(), null, count);
				break;
			case 3:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, count);
				break;
			case 4:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, count);
				break;
			case 5:
			{
				Mat structuringElement4 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, count));
				Cv2.Dilate(mat, mat, structuringElement4, null, count);
				break;
			}
			case 6:
			{
				Mat structuringElement3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, count));
				Cv2.Erode(mat, mat, structuringElement3, null, count);
				break;
			}
			case 7:
			{
				Mat structuringElement2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(count, 1));
				Cv2.Dilate(mat, mat, structuringElement2, null, count);
				break;
			}
			case 8:
			{
				Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(count, 1));
				Cv2.Erode(mat, mat, structuringElement, null, count);
				break;
			}
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanHLine(Mat BinaryROI, Rect roi, out List<Point> edgept1, out int Min, out int Max, out int Avg, bool directionT2B = true, bool UseSinglePt = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			IntPtr data = BinaryROI.Data;
			int width = BinaryROI.Width;
			Point[] array = new Point[2];
			edgept1 = new List<Point>();
			Min = (Max = (Avg = 0));
			int top = roi.Top;
			int left = roi.Left;
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, BinaryROI.Cols - 1, BinaryROI.Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				return false;
			}
			if (directionT2B)
			{
				for (int i = roi.Left; i < roi.Right; i++)
				{
					byte* ptr = (byte*)data.ToPointer();
					int num = width * roi.Top + i;
					array[0].Y = 0;
					array[1].Y = 0;
					for (int j = roi.Top; j < roi.Bottom - 1; j++)
					{
						if (array[0].Y == 0)
						{
							if (ptr[num] == BackgroundColor && ptr[num + width] == BackgroundColor)
							{
								array[0].X = i + left;
								array[0].Y = j + top;
							}
						}
						else if (array[1].Y == 0 && ptr[num] == EdgeColor)
						{
							array[1].X = i + left;
							array[1].Y = j + top;
						}
						if (array[0].Y > 0 && array[1].Y > 0)
						{
							edgept1.Add(array[1]);
							break;
						}
						if (j == roi.Bottom - 2)
						{
							array[1].X = -1;
							array[1].Y = -1;
							edgept1.Add(array[1]);
							break;
						}
						num += width;
					}
					if (UseSinglePt && array[1].Y == 0)
					{
						array[1].X = i + left;
						array[1].Y = roi.Bottom - 2 + top;
						edgept1.Add(array[1]);
					}
				}
			}
			else
			{
				for (int k = roi.Left; k < roi.Right; k++)
				{
					byte* ptr2 = (byte*)data.ToPointer();
					int num2 = width * (roi.Bottom - 1) + k;
					array[0].Y = 0;
					array[1].Y = 0;
					for (int num3 = roi.Bottom - 1; num3 > roi.Top + 1; num3--)
					{
						if (array[0].Y == 0)
						{
							if (ptr2[num2] == BackgroundColor && ptr2[num2 - width] == BackgroundColor)
							{
								array[0].X = k + left;
								array[0].Y = num3 + top;
							}
						}
						else if (array[1].Y == 0 && ptr2[num2] == EdgeColor)
						{
							array[1].X = k + left;
							array[1].Y = num3 + top;
						}
						if (array[0].Y > 0 && array[1].Y > 0)
						{
							edgept1.Add(array[1]);
							break;
						}
						num2 -= width;
						if (num3 == roi.Top + 2)
						{
							array[1].X = -1;
							array[1].Y = -1;
							edgept1.Add(array[1]);
						}
					}
					if (UseSinglePt && array[1].Y == 0)
					{
						array[1].X = k + left;
						array[1].Y = roi.Top + 2 + top;
						edgept1.Add(array[1]);
					}
				}
			}
			if (edgept1.Count <= 0)
			{
				return false;
			}
			int num4 = 0;
			int num5 = 0;
			int num6 = 10000;
			int num7 = 0;
			for (int l = 0; l < edgept1.Count; l++)
			{
				if (edgept1[l].X != -1 && edgept1[l].Y != -1)
				{
					num4 += edgept1[l].Y;
					if (edgept1[l].Y < num6)
					{
						num6 = edgept1[l].Y;
					}
					if (edgept1[l].Y > num5)
					{
						num5 = edgept1[l].Y;
					}
					num7++;
				}
			}
			double num8 = 0.0;
			num8 = ((num7 > 0) ? ((double)(num4 / num7)) : 0.0);
			Min = num6;
			Max = num5;
			Avg = (int)num8;
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT04-MeasureTool-ScanHline()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Measure-004");
			}
			edgept1 = new List<Point>();
			Min = (Max = (Avg = 0));
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanVLine(Mat BinaryROI, Rect roi, out List<Point> edgept1, out int Min, out int Max, out int Avg, bool directionL2R = true, bool UseSinglePt = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			IntPtr data = BinaryROI.Data;
			int width = BinaryROI.Width;
			int num = 0;
			Point[] array = new Point[2];
			edgept1 = new List<Point>();
			Min = (Max = (Avg = 0));
			int top = roi.Top;
			int left = roi.Left;
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, BinaryROI.Cols - 1, BinaryROI.Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				return false;
			}
			if (directionL2R)
			{
				for (int i = roi.Top; i < roi.Bottom; i++)
				{
					byte* ptr = (byte*)data.ToPointer();
					num = width * i;
					array[0].X = 0;
					array[1].X = 0;
					for (int j = roi.Left; j < roi.Right - 1; j++)
					{
						if (array[0].X == 0)
						{
							if (ptr[num + j] == BackgroundColor && ptr[num + j + 1] == BackgroundColor)
							{
								array[0].X = j + left;
								array[0].Y = i + top;
							}
						}
						else if (array[1].X == 0 && ptr[num + j] == EdgeColor)
						{
							array[1].X = j + left;
							array[1].Y = i + top;
						}
						if (array[0].X > 0 && array[1].X > 0)
						{
							edgept1.Add(array[1]);
							break;
						}
						if (j == roi.Right - 2)
						{
							array[1].X = -1;
							array[1].Y = -1;
							edgept1.Add(array[1]);
							break;
						}
					}
					if (UseSinglePt && array[1].X == 0)
					{
						array[1].X = roi.Right - 2 + left;
						array[1].Y = i + top;
						edgept1.Add(array[1]);
					}
				}
			}
			else
			{
				for (int k = roi.Top; k < roi.Bottom; k++)
				{
					byte* ptr2 = (byte*)data.ToPointer();
					num = width * k;
					array[0].X = 0;
					array[1].X = 0;
					for (int num2 = roi.Right - 1; num2 > roi.Left + 1; num2--)
					{
						if (array[0].X == 0)
						{
							if (ptr2[num + num2] == BackgroundColor && ptr2[num + num2 - 1] == BackgroundColor)
							{
								array[0].X = num2 + left;
								array[0].Y = k + top;
							}
						}
						else if (array[1].X == 0 && ptr2[num + num2] == EdgeColor)
						{
							array[1].X = num2 + left;
							array[1].Y = k + top;
						}
						if (array[0].X > 0 && array[1].X > 0)
						{
							edgept1.Add(array[1]);
							break;
						}
						if (num2 == roi.Left + 2)
						{
							array[1].X = -1;
							array[1].Y = -1;
							edgept1.Add(array[1]);
							break;
						}
					}
					if (UseSinglePt && array[1].X == 0)
					{
						array[1].X = roi.Left + 2 + left;
						array[1].Y = k + top;
						edgept1.Add(array[1]);
					}
				}
			}
			if (edgept1.Count <= 0)
			{
				return false;
			}
			int num3 = 0;
			int num4 = 10000;
			int num5 = 0;
			int num6 = 0;
			for (int l = 0; l < edgept1.Count; l++)
			{
				if (edgept1[l].X != -1 && edgept1[l].Y != -1)
				{
					num3 += edgept1[l].X;
					if (edgept1[l].X < num4)
					{
						num4 = edgept1[l].X;
					}
					if (edgept1[l].X > num5)
					{
						num5 = edgept1[l].X;
					}
					num6++;
				}
			}
			double num7 = 0.0;
			num7 = ((num6 > 0) ? ((double)(num3 / num6)) : 0.0);
			Min = num4;
			Max = num5;
			Avg = (int)num7;
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT05-MeasureTool-ScanVline()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Measure-005");
			}
			edgept1 = new List<Point>();
			Min = (Max = (Avg = 0));
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private double GetComDigitsValue(double value)
	{
		if (value > -98980.0)
		{
			return value;
		}
		if (value > -98980.0 || value < -98980.0 - (double)MaxComDigitsItems)
		{
			return value;
		}
		try
		{
			int num = (int)Math.Abs(-98980.0 - value);
			value = pfile.Comdigits[num];
			return value;
		}
		catch (Exception ex)
		{
			string text = "MT11-MeasureTool-GetComDigitsValue()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Measure-006");
			}
			return 0.0;
		}
	}
}
