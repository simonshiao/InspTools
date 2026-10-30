using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class LineAnalysisTool
{
	private const int MaxTracker = 1;

	private const int MaxThdTool = 3;

	private CVBuffer buffers;

	public Mat Binary = new Mat();

	private Mat ThdBinary = new Mat();

	private Mat MaskTemp = new Mat();

	private ParamFile pfile;

	public Rect InspeRect;

	private bool OffLine = true;

	private int LineAnalysisIndex;

	public List<Point> NGPt = new List<Point>();

	public List<Point> DistanceNGPt = new List<Point>();

	public List<Point> EdgePT = new List<Point>();

	public List<double> PDistance = new List<double>();

	public List<double> LineAngles = new List<double>();

	public int LineSegmentCount;

	public int DistanceLineSegmentCount;

	private Stopwatch sw = new Stopwatch();

	public bool LineAnalysisSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public string ErrorStr { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public int TotalError { get; protected set; }

	public int DistanceTotalError { get; protected set; }

	public int DistanceContError { get; protected set; }

	public int ContinueError { get; protected set; }

	public double Angle { get; protected set; }

	public LineAnalysisTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect = new Rect(0, 0, 0, 0);
		Binary = new Mat();
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

	public void GetInspeRect(int index)
	{
		LocateRect = buffers.LocatedRect;
		if (pfile.LineAnalysisParam[index].Tracker1UseAutoSize)
		{
			InspeRect.X = pfile.AdvLocateParam[pfile.LineAnalysisParam[index].AutoSize1_SelectIndex].L + pfile.LineAnalysisParam[index].Track1_L;
			InspeRect.Y = pfile.AdvLocateParam[pfile.LineAnalysisParam[index].AutoSize1_SelectIndex].T + pfile.LineAnalysisParam[index].Track1_T;
			InspeRect.Width = pfile.AdvLocateParam[pfile.LineAnalysisParam[index].AutoSize1_SelectIndex].Width - (pfile.LineAnalysisParam[index].Track1_L + pfile.LineAnalysisParam[index].Track1_R);
			InspeRect.Height = pfile.AdvLocateParam[pfile.LineAnalysisParam[index].AutoSize1_SelectIndex].Height - (pfile.LineAnalysisParam[index].Track1_T + pfile.LineAnalysisParam[index].Track1_B);
		}
		else
		{
			int width = pfile.LineAnalysisParam[index].Track1_R - pfile.LineAnalysisParam[index].Track1_L;
			int height = pfile.LineAnalysisParam[index].Track1_B - pfile.LineAnalysisParam[index].Track1_T;
			InspeRect.X = pfile.VSLocateParam[pfile.LineAnalysisParam[index].Tracker1AligmentXIndex].Value + pfile.LineAnalysisParam[index].Track1_L;
			InspeRect.Y = pfile.HSLocateParam[pfile.LineAnalysisParam[index].Tracker1AligmentYIndex].Value + pfile.LineAnalysisParam[index].Track1_T;
			InspeRect.Width = width;
			InspeRect.Height = height;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		OffLine = offline;
		LineAnalysisSuccess = true;
		Tracker1ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		TotalError = 0;
		DistanceTotalError = 0;
		DistanceContError = 0;
		ContinueError = 0;
		Angle = 0.0;
		EdgePT.Clear();
		NGPt.Clear();
		LineAngles.Clear();
		PDistance.Clear();
		DistanceNGPt.Clear();
		if (!pfile.LineAnalysisParam[index].TrackerON)
		{
			iresult = TOOL_RESULT.EM_NOT_USE;
			return true;
		}
		if (FaceIndex != 0 && pfile.LineAnalysisParam[index].EnableSelectFace && FaceIndex != pfile.LineAnalysisParam[index].FaceIndex)
		{
			iresult = TOOL_RESULT.EM_OK;
			return true;
		}
		LineAnalysisIndex = index;
		GetInspeRect(index);
		_ = new bool[1];
		int tHD1_ChannelIndex = (int)pfile.LineAnalysisParam[index].THD1_ChannelIndex;
		InspeRect &= new Rect(1, 1, buffers.GetChannelMat(tHD1_ChannelIndex).Cols - 1, buffers.GetChannelMat(tHD1_ChannelIndex).Rows - 1);
		bool flag = true;
		ErrorStr = "";
		try
		{
			flag = true;
			if (InspeRect.Width == 0 || InspeRect.Height == 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				string text = ((buffers.LanguageType == 1) ? string.Format("[{0}:{1}] 檢測區域寬高為0! \n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType == 2) ? string.Format("[{0}:{1}] 检测区域宽高为0! \n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType != 3) ? string.Format("[{0}:{1}] Tracker Value <= 0! \n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : string.Format("[{0}:{1}] 検出領域の幅と高さが0です！ \n", index + 1, pfile.LineAnalysisParam[index].TrackerName))));
				Tracker1ResultText += text;
				ErrorStr += text;
				return false;
			}
			doThdImage(pfile.LineAnalysisParam[index]);
			if (pfile.LineAnalysisParam[index].MaskSelectIndex > 0 && !doMask())
			{
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			int direction = (int)pfile.LineAnalysisParam[LineAnalysisIndex].Direction;
			if (pfile.LineAnalysisParam[LineAnalysisIndex].UseLineFilter)
			{
				Cv2.FindContours(Binary, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
				int num = 0;
				int num2 = -1;
				for (int i = 0; i < contours.Length; i++)
				{
					if (contours[i].Length > num)
					{
						num = contours[i].Length;
						num2 = i;
					}
				}
				if (num2 >= 0)
				{
					Binary.SetTo(0.0);
					Cv2.DrawContours(Binary, contours, num2, Scalar.White);
				}
			}
			if (direction < 2)
			{
				flag = ((direction != 0) ? ScanVLine(Binary, InspeRect, out EdgePT, directionL2R: false, 255, 0, pfile.LineAnalysisParam[index].UseStdFilter) : ScanVLine(Binary, InspeRect, out EdgePT, directionL2R: true, 255, 0, pfile.LineAnalysisParam[index].UseStdFilter));
				if (!flag)
				{
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
				if (EdgePT.Count <= 0)
				{
					string text2 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 線段分析掃描點點數等於0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 线段分析扫描点点数等于0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Edge point less than 0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : string.Format("{0}:{1}] 線段分析スキャン点数等於0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName))));
					Tracker1ResultText += text2;
					ErrorStr += text2;
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
			}
			else
			{
				flag = ((direction != 2) ? ScanHLine(Binary, InspeRect, out EdgePT, directionT2B: false, 255, 0, pfile.LineAnalysisParam[index].UseStdFilter) : ScanHLine(Binary, InspeRect, out EdgePT, directionT2B: true, 255, 0, pfile.LineAnalysisParam[index].UseStdFilter));
				if (!flag)
				{
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
				if (EdgePT.Count <= 0)
				{
					string text3 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 線段分析掃描點點數等於0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 线段分析扫描点点数等于0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Edge point less than 0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : string.Format("{0}:{1}] 線段分析スキャン点数等於0。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName))));
					Tracker1ResultText += text3;
					ErrorStr += text3;
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
			}
			if (pfile.LineAnalysisParam[LineAnalysisIndex].EnablePointCheck)
			{
				if (EdgePT.Count <= pfile.LineAnalysisParam[LineAnalysisIndex].P2PInterval)
				{
					iresult = TOOL_RESULT.EM_NG;
					string text4 = ((buffers.LanguageType == 0) ? string.Format("{0}:{1}] Pass edge point < P2P Interval value。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : ((buffers.LanguageType != 1) ? string.Format("{0}:{1}] 线段分析扫描点点数小于比对间隔。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName) : string.Format("{0}:{1}] 線段分析掃描點點數小於比對間隔。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName)));
					Tracker1ResultText += text4;
					ErrorStr += text4;
					return false;
				}
				flag = doCheckResult(EdgePT, direction, pfile.LineAnalysisParam[LineAnalysisIndex].P2PInterval, pfile.LineAnalysisParam[LineAnalysisIndex].P2PAllowValue);
				if (!flag)
				{
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
			}
			if (pfile.LineAnalysisParam[LineAnalysisIndex].EnableDistanceCheck)
			{
				if (pfile.LineAnalysisParam[LineAnalysisIndex].DistanceLineSegment == 0)
				{
					List<Point> list = new List<Point>();
					list = EdgePT.ToList();
					if (pfile.LineAnalysisParam[index].UseStdFilter)
					{
						if (direction < 2)
						{
							StdFilter(ref list, 0);
						}
						else
						{
							StdFilter(ref list, 1);
						}
					}
					Line2D line2D = Cv2.FitLine(list, DistanceTypes.L2, 0.0, 0.01, 0.01);
					int num3 = 0;
					for (int j = 0; j < EdgePT.Count; j++)
					{
						double num4 = line2D.Distance(EdgePT[j]);
						PDistance.Add(num4);
						if (num4 > pfile.LineAnalysisParam[LineAnalysisIndex].AllowDistance)
						{
							DistanceNGPt.Add(EdgePT[j]);
							DistanceTotalError++;
							num3++;
							continue;
						}
						if (num3 > DistanceContError)
						{
							DistanceContError = num3;
						}
						num3 = 0;
					}
				}
				else
				{
					int num5 = pfile.LineAnalysisParam[LineAnalysisIndex].DistanceLineSegment + 1;
					int num6 = num5 * 2;
					int num7 = (DistanceLineSegmentCount = EdgePT.Count / num5);
					if (EdgePT.Count < num6 || num5 < 0 || num7 < 0 || num6 < 0)
					{
						iresult = TOOL_RESULT.EM_NG;
						string text5 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 線段分析點數過少({2} < {3})，無法進行分段距離檢查。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num6) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 线段分析点数过少({2} < {3})，无法进行分段距離检查。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num6) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Edge point({2}) < Minimum({3})。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num6) : string.Format("{0}:{1}] 線段分析点数過少({2} < {3})、分段距離検査を行うことができません。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num6))));
						Tracker1ResultText += text5;
						ErrorStr += text5;
						return false;
					}
					List<Point>[] array = new List<Point>[num5];
					for (int k = 0; k < num5; k++)
					{
						array[k] = new List<Point>();
					}
					int num8 = 0;
					bool flag2 = true;
					for (int l = 0; l < num5; l++)
					{
						flag2 = true;
						while (flag2)
						{
							array[l].Add(EdgePT[num8]);
							num8++;
							if (num8 == EdgePT.Count)
							{
								flag2 = false;
							}
							if (l != num5 - 1 && num8 % num7 == 0)
							{
								flag2 = false;
							}
						}
						if (num8 == EdgePT.Count)
						{
							break;
						}
					}
					int num9 = 0;
					for (int m = 0; m < num5; m++)
					{
						if (array[m].Count < 2)
						{
							continue;
						}
						List<Point> list2 = new List<Point>();
						list2 = array[m].ToList();
						if (pfile.LineAnalysisParam[index].UseStdFilter)
						{
							if (direction < 2)
							{
								StdFilter(ref list2, 0);
							}
							else
							{
								StdFilter(ref list2, 1);
							}
						}
						Line2D line2D2 = Cv2.FitLine(list2, DistanceTypes.L2, 0.0, 0.01, 0.01);
						line2D2.GetVectorAngle();
						for (int n = 0; n < array[m].Count; n++)
						{
							double num10 = line2D2.Distance(array[m][n]);
							PDistance.Add(num10);
							if (num10 > pfile.LineAnalysisParam[LineAnalysisIndex].AllowDistance)
							{
								DistanceNGPt.Add(array[m][n]);
								DistanceTotalError++;
								num9++;
								continue;
							}
							if (num9 > DistanceContError)
							{
								DistanceContError = num9;
							}
							num9 = 0;
						}
					}
				}
				if (DistanceTotalError > pfile.LineAnalysisParam[LineAnalysisIndex].AllowDistanceErrorCount || DistanceContError > pfile.LineAnalysisParam[LineAnalysisIndex].AllowDistanceContErrorCount)
				{
					iresult = TOOL_RESULT.EM_NG;
					string text6 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 分段檢測距離，連續錯誤數={2}，總錯誤數={3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 分段检测距离，连续错误数={2}，总错误数={3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Line Distance Error Count={2}, Total Error Count={3}, over Range. [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError) : string.Format("{0}:{1}] 分段検出距離，連続エラー数={2}，総エラー数={3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError))));
					Tracker1ResultText += text6;
					ErrorStr += text6;
					return false;
				}
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("{0}:{1}] 分段檢測距離，連續錯誤數={2}，連續錯誤數={3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("{0}:{1}] 分段检测距离，连续错误数={2}，总错误数={3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("{0}:{1}] 区分検出距離、連続エラー数={2}、総エラー数={3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError);
				}
				else
				{
					Tracker1ResultText += string.Format("{0}:{1}] Line Distance Continue Error Count={2}, Total Error Count={3},\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, DistanceContError, DistanceTotalError);
				}
			}
			if (pfile.LineAnalysisParam[LineAnalysisIndex].EnableAngleCheck)
			{
				if (pfile.LineAnalysisParam[LineAnalysisIndex].LineSegment == 0)
				{
					Line2D line2D3 = Cv2.FitLine(EdgePT, DistanceTypes.L2, 0.0, 0.01, 0.01);
					Angle = line2D3.GetVectorAngle();
					if (direction < 2)
					{
						if (Angle <= 0.0)
						{
							Angle = Math.Abs(Angle);
						}
						else
						{
							Angle = 90.0 + (90.0 - Angle);
						}
					}
					else if (Angle <= 0.0)
					{
						Angle = Math.Abs(Angle);
					}
					else
					{
						Angle *= -1.0;
					}
					LineAngles.Add(Angle);
					if (Angle < pfile.LineAnalysisParam[LineAnalysisIndex].LineAngleMin || Angle > pfile.LineAnalysisParam[LineAnalysisIndex].LineAngleMax)
					{
						iresult = TOOL_RESULT.EM_NG;
						string text7 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 檢測線段角度={2:N3}, 超過設定值。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 检测线段角度={2:N3}, 超过设定值。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Line Angle={2:N3}, Angle over Range。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle) : string.Format("{0}:{1}] 検出線分の角度={2:N3}, 設定値を超過。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle))));
						Tracker1ResultText += text7;
						ErrorStr += text7;
						return false;
					}
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("{0}:{1}] 檢測線段角度={2:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("{0}:{1}] 检测线段角度={2:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("{0}:{1}] 検出線分の角度={2:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle);
					}
					else
					{
						Tracker1ResultText += string.Format("{0}:{1}] Line Angle={2:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, Angle);
					}
				}
				else
				{
					LineAngles.Clear();
					int num11 = pfile.LineAnalysisParam[LineAnalysisIndex].LineSegment + 1;
					int num12 = num11 * 2;
					int num13 = (LineSegmentCount = EdgePT.Count / num11);
					if (EdgePT.Count < num12 || num11 < 0 || num13 < 0 || num12 < 0)
					{
						iresult = TOOL_RESULT.EM_NG;
						string text8 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 線段分析點數過少({2} < {3})，無法進行分段角度檢查。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num12) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 线段分析点数过少({2} < {3})，无法进行分段角度检查。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num12) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Edge point({2}) < Minimum({3})。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num12) : string.Format("{0}:{1}] 線分解析点数が少なすぎます({2} < {3})ため、分段角度検査を行うことができません。\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, EdgePT.Count, num12))));
						Tracker1ResultText += text8;
						ErrorStr += text8;
						return false;
					}
					List<Point>[] array2 = new List<Point>[num11];
					for (int num14 = 0; num14 < num11; num14++)
					{
						array2[num14] = new List<Point>();
					}
					int num15 = 0;
					bool flag3 = true;
					for (int num16 = 0; num16 < num11; num16++)
					{
						flag3 = true;
						while (flag3)
						{
							array2[num16].Add(EdgePT[num15]);
							num15++;
							if (num15 == EdgePT.Count)
							{
								flag3 = false;
							}
							if (num16 != num11 - 1 && num15 % num13 == 0)
							{
								flag3 = false;
							}
						}
						if (num15 == EdgePT.Count)
						{
							break;
						}
					}
					for (int num17 = 0; num17 < num11; num17++)
					{
						if (array2[num17].Count >= 2)
						{
							Line2D line2D4 = Cv2.FitLine(array2[num17], DistanceTypes.L2, 0.0, 0.01, 0.01);
							Angle = line2D4.GetVectorAngle();
						}
						else
						{
							Angle = 0.0;
						}
						if (direction < 2)
						{
							if (Angle <= 0.0)
							{
								Angle = Math.Abs(Angle);
							}
							else
							{
								Angle = 90.0 + (90.0 - Angle);
							}
						}
						else if (Angle <= 0.0)
						{
							Angle = Math.Abs(Angle);
						}
						else
						{
							Angle *= -1.0;
						}
						LineAngles.Add(Angle);
						if (Angle < pfile.LineAnalysisParam[LineAnalysisIndex].LineAngleMin || Angle > pfile.LineAnalysisParam[LineAnalysisIndex].LineAngleMax)
						{
							iresult = TOOL_RESULT.EM_NG;
							string text9 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 檢測線段[{2}]角度={3:N3}, 超過設定值。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 检测线段[{2}]角度={3:N3}, 超过设定值。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Line[{2}] Angle={3:N3}, Angle over Range。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle) : string.Format("{0}:{1}] 検出線分[{2}]角度={3:N3}, 設定値を超過。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle))));
							Tracker1ResultText += text9;
							ErrorStr += text9;
							flag = false;
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("{0}:{1}] 檢測線段[{2}]角度={3:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("{0}:{1}] 检测线段[{2}]角度={3:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("{0}:{1}] 検出線分[{2}]角度={3:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle);
						}
						else
						{
							Tracker1ResultText += string.Format("{0}:{1}] Line[{2}] Angle={3:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num17 + 1, Angle);
						}
					}
					for (int num18 = 1; num18 < LineAngles.Count; num18++)
					{
						double num19 = Math.Abs(LineAngles[num18 - 1] - LineAngles[num18]);
						if (num19 > pfile.LineAnalysisParam[LineAnalysisIndex].LineAngleDiff)
						{
							iresult = TOOL_RESULT.EM_NG;
							string text10 = ((buffers.LanguageType == 1) ? string.Format("{0}:{1}] 檢測線段[{2}]-[{3}] 角度差={4:N3}, 超過設定值。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19) : ((buffers.LanguageType == 2) ? string.Format("{0}:{1}] 检测线段[{2}]-[{3}] 角度={4:N3}, 超过设定值。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19) : ((buffers.LanguageType != 3) ? string.Format("{0}:{1}] Line[{2}]-[{3}] Angle Diff.={4:N3}, Difference over Range。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19) : string.Format("{0}:{1}] 検出線分[{2}]-[{3}] 角度={4:N3}, 設定値を超過。 [NG]\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19))));
							flag = false;
							Tracker1ResultText += text10;
							ErrorStr += text10;
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("{0}:{1}] 檢測線段[{2}]-[{3}] 角度差={4:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("{0}:{1}] 检测线段[{2}]-[{3}] 角度={4:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("{0}:{1}] 検出線分[{2}]-[{3}] 角度={4:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18, num18 + 1, num19);
						}
						else
						{
							Tracker1ResultText += string.Format("{0}:{1}] Line[{2}]-[{3}] Angle Diff.={4:N3}\n", index + 1, pfile.LineAnalysisParam[index].TrackerName, num18 + 1, num18 + 1, num19);
						}
					}
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text11 = "LAT01-LineAnalysisTool-Inspect()";
			text11 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text11);
			}
			else
			{
				CVBuffer.SaveLog(text11, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "LineAnaly-001");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool doCheckResult(List<Point> edgept, int dir, int P2PInterval, int P2PAllowValue)
	{
		bool flag = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		NGPt.Clear();
		if (dir < 2)
		{
			for (int i = 0; i < edgept.Count - P2PInterval; i++)
			{
				if (i + P2PInterval >= edgept.Count || i + P2PInterval < 0)
				{
					continue;
				}
				if (Math.Abs(edgept[i].X - edgept[i + P2PInterval].X) > P2PAllowValue)
				{
					NGPt.Add(edgept[i]);
					num++;
					num2++;
					if (num2 > num3)
					{
						num3 = num2;
					}
				}
				else
				{
					num2 = 0;
				}
			}
			num2 = 0;
			int num4 = edgept.Count - 1;
			while (num4 > edgept.Count - P2PInterval && num4 >= 0)
			{
				if (num4 - P2PInterval < edgept.Count && num4 - P2PInterval >= 0)
				{
					if (Math.Abs(edgept[num4].X - edgept[num4 - P2PInterval].X) > P2PAllowValue)
					{
						NGPt.Add(edgept[num4]);
						num++;
						num2++;
						if (num2 > num3)
						{
							num3 = num2;
						}
					}
					else
					{
						num2 = 0;
					}
				}
				num4--;
			}
		}
		else
		{
			for (int j = 0; j < edgept.Count - P2PInterval; j++)
			{
				if (j + P2PInterval >= edgept.Count || j + P2PInterval < 0)
				{
					continue;
				}
				if (Math.Abs(edgept[j].Y - edgept[j + P2PInterval].Y) > P2PAllowValue)
				{
					NGPt.Add(edgept[j]);
					num++;
					num2++;
					if (num2 > num3)
					{
						num3 = num2;
					}
				}
				else
				{
					num2 = 0;
				}
			}
			num2 = 0;
			int num5 = edgept.Count - 1;
			while (num5 >= edgept.Count - P2PInterval && num5 >= 0)
			{
				if (num5 - P2PInterval < edgept.Count && num5 - P2PInterval >= 0)
				{
					if (Math.Abs(edgept[num5].Y - edgept[num5 - P2PInterval].Y) > P2PAllowValue)
					{
						NGPt.Add(edgept[num5]);
						num++;
						num2++;
						if (num2 > num3)
						{
							num3 = num2;
						}
					}
					else
					{
						num2 = 0;
					}
				}
				num5--;
			}
		}
		TotalError = num;
		ContinueError = num3;
		if (num > pfile.LineAnalysisParam[LineAnalysisIndex].TotalError)
		{
			if (buffers.LanguageType == 1)
			{
				Tracker1ResultText = string.Format("[{0}:{1}]最大錯誤數={2}, 超過設定值{3} \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num, pfile.LineAnalysisParam[LineAnalysisIndex].TotalError);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText = string.Format("[{0}:{1}]最大错误数={2}, 超过设定值{3} \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num, pfile.LineAnalysisParam[LineAnalysisIndex].TotalError);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText = string.Format("[{0}:{1}]最大エラー数={2}, 設定値{3}を超過 \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num, pfile.LineAnalysisParam[LineAnalysisIndex].TotalError);
			}
			else
			{
				Tracker1ResultText = string.Format("[{0}:{1}]Total Error={2}, over range {3} \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num, pfile.LineAnalysisParam[LineAnalysisIndex].TotalError);
			}
			flag = false;
		}
		if (num3 > pfile.LineAnalysisParam[LineAnalysisIndex].ContinueError)
		{
			if (buffers.LanguageType == 1)
			{
				Tracker1ResultText += string.Format("[{0}:{1}]連續錯誤數={2}, 超過設定值{3} \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num3, pfile.LineAnalysisParam[LineAnalysisIndex].ContinueError);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText += string.Format("[{0}:{1}]连续错误数={2}, 超过设定值{3} \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num3, pfile.LineAnalysisParam[LineAnalysisIndex].ContinueError);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText += string.Format("[{0}:{1}]連続エラー数={2}, 設定値{3}を超過 \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num3, pfile.LineAnalysisParam[LineAnalysisIndex].ContinueError);
			}
			else
			{
				Tracker1ResultText += string.Format("[{0}:{1}]Continue Error={2}, over range {3} \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num3, pfile.LineAnalysisParam[LineAnalysisIndex].ContinueError);
			}
			flag = false;
		}
		if (!flag)
		{
			ErrorStr += Tracker1ResultText;
		}
		return flag;
	}

	public void StdFilter(ref List<Point> pt, int index)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		List<Point> list = new List<Point>();
		if (index == 0)
		{
			for (int i = 0; i < pt.Count; i++)
			{
				num2 += (double)pt[i].X;
			}
			num = num2 / (double)pt.Count;
			for (int j = 0; j < pt.Count; j++)
			{
				num3 += Math.Pow((double)pt[j].X - num, 2.0);
			}
			num4 = Math.Sqrt(num3 / (double)pt.Count);
			for (int k = 0; k < pt.Count; k++)
			{
				if (Math.Abs((double)pt[k].X - num) <= num4)
				{
					list.Add(pt[k]);
				}
			}
		}
		else
		{
			for (int l = 0; l < pt.Count; l++)
			{
				num2 += (double)pt[l].Y;
			}
			num = num2 / (double)pt.Count;
			for (int m = 0; m < pt.Count; m++)
			{
				num3 += Math.Pow((double)pt[m].Y - num, 2.0);
			}
			num4 = Math.Sqrt(num3 / (double)pt.Count);
			for (int n = 0; n < pt.Count; n++)
			{
				if (Math.Abs((double)pt[n].Y - num) <= num4)
				{
					list.Add(pt[n]);
				}
			}
		}
		if (list.Count >= 2)
		{
			pt.Clear();
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				pt.Add(list[num5]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool doThdImage(INSP_LINEANALYSISTOOL_PARAM param)
	{
		if (!param.TrackerON)
		{
			return true;
		}
		try
		{
			_ = new bool[3];
			new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect).CopyTo(Binary);
			Cv2.InRange(Binary, param.THD1_Min, param.THD1_Max, Binary);
			DoMorph(ref Binary, param.THD1_DEStart, param.THD1_DEStartCount);
			DoMorph(ref Binary, param.THD1_DEEnd, param.THD1_DEEndCount);
			if (param.THD23_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD2_ChannelIndex), InspeRect).CopyTo(ThdBinary);
				Cv2.InRange(ThdBinary, param.THD2_Min, param.THD2_Max, ThdBinary);
				DoMorph(ref ThdBinary, param.THD2_DEStart, param.THD2_DEStartCount);
				DoMorph(ref ThdBinary, param.THD2_DEEnd, param.THD2_DEEndCount);
				Cv2.BitwiseAnd(Binary, ThdBinary, Binary);
				new Mat(buffers.GetChannelMat((int)param.THD3_ChannelIndex), InspeRect).CopyTo(ThdBinary);
				Cv2.InRange(ThdBinary, param.THD3_Min, param.THD3_Max, ThdBinary);
				DoMorph(ref ThdBinary, param.THD3_DEStart, param.THD3_DEStartCount);
				DoMorph(ref ThdBinary, param.THD3_DEEnd, param.THD3_DEEndCount);
				Cv2.BitwiseAnd(Binary, ThdBinary, Binary);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "LAT02-LineAnalysisTool-doThdImage()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "LineAnaly-002");
			}
			return false;
		}
	}

	private void DoMorph(ref Mat mat, int index, int count)
	{
		if ((count > 0 || index == 10) && mat.Cols > 0 && mat.Rows > 0)
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
			case 9:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Gradient, new Mat(), null, count);
				break;
			case 10:
				Cv2.BitwiseNot(mat, mat);
				break;
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool doMask()
	{
		try
		{
			if (!pfile.MaskToolON)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具未開啟! \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具未开启! \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 検出領域でマスクツールが使用されていますが、マスクツールが有効になっていません！\n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName);
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool has been Disable! \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName);
				}
				return false;
			}
			int num = pfile.LineAnalysisParam[LineAnalysisIndex].MaskSelectIndex - 1;
			if (!pfile.MaskParam[num].TrackerON)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具[{2}]未開啟! \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具[{2}]未开启! \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 検出領域でマスクツールが使用されていますが、マスクツール[{2}]が有効になっていません！\n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num + 1);
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool [{2}] has been Disable! \n", LineAnalysisIndex + 1, pfile.LineAnalysisParam[LineAnalysisIndex].TrackerName, num + 1);
				}
				return false;
			}
			buffers.GetChannelMat(0).CopyTo(MaskTemp);
			MaskTemp.SetTo(0.0);
			bool flag = false;
			Rect[] array = new Rect[2]
			{
				new Rect(pfile.MaskParam[num].Mask1Pt.X, pfile.MaskParam[num].Mask1Pt.Y, pfile.MaskParam[num].Mask1.Width, pfile.MaskParam[num].Mask1.Height),
				default(Rect)
			};
			if (pfile.MaskParam[num].SearchRect2ON && num < 20)
			{
				array[1] = new Rect(pfile.MaskParam[num].Mask2Pt.X, pfile.MaskParam[num].Mask2Pt.Y, pfile.MaskParam[num].Mask2.Width, pfile.MaskParam[num].Mask2.Height);
			}
			else
			{
				array[1] = new Rect(0, 0, 0, 0);
			}
			if (array[0].Width > 0 && array[0].Height > 0)
			{
				Mat m = new Mat(MaskTemp, array[0]);
				pfile.MaskParam[num].Mask1.CopyTo(m);
				flag = true;
			}
			if (pfile.MaskParam[num].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num < 20)
			{
				Mat m2 = new Mat(MaskTemp, array[1]);
				pfile.MaskParam[num].Mask2.CopyTo(m2);
				flag = true;
			}
			if (flag)
			{
				Mat mat = new Mat(MaskTemp, InspeRect);
				if (pfile.LineAnalysisParam[LineAnalysisIndex].UseMaskAsInspArea)
				{
					Cv2.BitwiseAnd(Binary, mat, Binary);
				}
				else
				{
					Cv2.BitwiseNot(mat, mat);
					Cv2.BitwiseAnd(Binary, mat, Binary);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "LAT03-LineAnalysisTool-doMask()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "LineAnaly-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanHLine(Mat Binary, Rect roi, out List<Point> edgeptY, bool directionT2B = true, int EdgeColor = 255, int BackgroundColor = 0, bool UseStdFilter = false)
	{
		IntPtr data = Binary.Data;
		int width = Binary.Width;
		Point[] array = new Point[2];
		edgeptY = new List<Point>();
		try
		{
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, Binary.Cols - 1, Binary.Rows - 1);
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
								array[0].X = i;
								array[0].Y = j;
							}
						}
						else if (array[1].Y == 0 && ptr[num] == EdgeColor)
						{
							array[1].X = i;
							array[1].Y = j;
						}
						if (array[0].Y > 0 && array[1].Y > 0)
						{
							edgeptY.Add(array[1]);
							break;
						}
						num += width;
					}
				}
			}
			else
			{
				for (int k = roi.Left; k < roi.Right; k++)
				{
					byte* ptr2 = (byte*)data.ToPointer();
					int num2 = width * roi.Bottom + k;
					array[0].Y = 0;
					array[1].Y = 0;
					for (int num3 = roi.Bottom; num3 > roi.Top + 1; num3--)
					{
						if (array[0].Y == 0)
						{
							if (ptr2[num2] == BackgroundColor && ptr2[num2 - width] == BackgroundColor)
							{
								array[0].X = k;
								array[0].Y = num3;
							}
						}
						else if (array[1].Y == 0 && ptr2[num2] == EdgeColor)
						{
							array[1].X = k;
							array[1].Y = num3;
						}
						if (array[0].Y > 0 && array[1].Y > 0)
						{
							edgeptY.Add(array[1]);
							break;
						}
						num2 -= width;
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "LAT05-LineAnalysisTool-ScanHLine()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "LineAnaly-004");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanVLine(Mat Binary, Rect roi, out List<Point> edgeptX, bool directionL2R = true, int EdgeColor = 255, int BackgroundColor = 0, bool UseStdFilter = false)
	{
		IntPtr data = Binary.Data;
		int width = Binary.Width;
		int num = 0;
		Point[] array = new Point[2];
		edgeptX = new List<Point>();
		try
		{
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, Binary.Cols - 1, Binary.Rows - 1);
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
								array[0].X = j;
								array[0].Y = i;
							}
						}
						else if (array[1].X == 0 && ptr[num + j] == EdgeColor)
						{
							array[1].X = j;
							array[1].Y = i;
						}
						if (array[0].X > 0 && array[1].X > 0)
						{
							edgeptX.Add(array[1]);
							break;
						}
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
					for (int num2 = roi.Right; num2 > roi.Left + 1; num2--)
					{
						if (array[0].X == 0)
						{
							if (ptr2[num + num2] == BackgroundColor && ptr2[num + num2 - 1] == BackgroundColor)
							{
								array[0].X = num2;
								array[0].Y = k;
							}
						}
						else if (array[1].X == 0 && ptr2[num + num2] == EdgeColor)
						{
							array[1].X = num2;
							array[1].Y = k;
						}
						if (array[0].X > 0 && array[1].X > 0)
						{
							edgeptX.Add(array[1]);
							break;
						}
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "LAT05-LineAnalysisTool-ScanVLine()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "LineAnaly-005");
			}
			return false;
		}
	}
}
