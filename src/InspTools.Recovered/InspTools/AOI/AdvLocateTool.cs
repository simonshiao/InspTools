using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class AdvLocateTool
{
	private const int MaxThdTool = 3;

	private const int MaxTools = 20;

	private const int MaxMeasureTools = 40;

	private Mat[] Binary = new Mat[3];

	private Mat[] AdvLocateBinary = new Mat[40];

	private Mat FinalBinary = new Mat();

	private CVBuffer buffers;

	private int MaxComDigitsItems = 24;

	public Rect NGRect;

	public Rect NGRect2;

	public Point NGPt1;

	public Point NGPt2;

	public List<Point> Line1Pt = new List<Point>();

	public List<Point> Line2Pt = new List<Point>();

	private ParamFile pfile;

	public Rect InspeRect;

	private Stopwatch sw = new Stopwatch();

	public double OfflineTime;

	private bool OffLine = true;

	public bool AdvLocateSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public double AdvAWidth { get; set; }

	public double AdvAHeight { get; set; }

	public double MeasureDistance { get; set; }

	public int Line1 { get; set; }

	public int Line2 { get; set; }

	public Point PT1 { get; set; }

	public Point PT2 { get; set; }

	public string ErrorMsg { get; set; }

	public AdvLocateTool(ParamFile paramfile)
	{
		MaxComDigitsItems = 24;
		pfile = paramfile;
		InspeRect = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 40; i++)
		{
			AdvLocateBinary[i] = new Mat();
		}
		for (int j = 0; j < 3; j++)
		{
			Binary[j] = new Mat();
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
	public bool doVScan(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = false)
	{
		try
		{
			OffLine = offline;
			pfile.VSLocateParam[index].Value = -1;
			iresult = TOOL_RESULT.EM_OK;
			if (!pfile.VSLocateParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.VSLocateParam[index].EnableSelectFace && FaceIndex != pfile.VSLocateParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			if (!pfile.VSLocateParam[index].THD1_ChannelON && !pfile.VSLocateParam[index].THD2_ChannelON && !pfile.VSLocateParam[index].THD3_ChannelON)
			{
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			if (!doThdImage(pfile.VSLocateParam[index]))
			{
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			ErrorMsg = "";
			if (!ScanVLine(directionL2R: pfile.VSLocateParam[index].Direction == ScanDirection.EM_LEFT_RIGHT, Binary: FinalBinary, roi: InspeRect, ResultX: out pfile.VSLocateParam[index].Value, FindEdgeMethod: pfile.VSLocateParam[index].FindEdgeMethod, UseStdFilter: pfile.VSLocateParam[index].UseStdFilter))
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[垂直 {0} {1}] 掃描合格點數小於2。\n", index + 1, pfile.VSLocateParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[垂直 {0} {1}] 扫描合格点数小于2。\n", index + 1, pfile.VSLocateParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[垂直 {0} {1}] の検査合格点数が2未満です。\n", index + 1, pfile.VSLocateParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Vertical {0} {1}] Pass edge point < 2。\n", index + 1, pfile.VSLocateParam[index].TrackerName);
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT01-AdvLocateTool-doVScan()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-001");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool doHScan(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = false)
	{
		try
		{
			OffLine = offline;
			pfile.HSLocateParam[index].Value = -1;
			iresult = TOOL_RESULT.EM_OK;
			if (!pfile.HSLocateParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.HSLocateParam[index].EnableSelectFace && FaceIndex != pfile.HSLocateParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			if (!pfile.HSLocateParam[index].THD1_ChannelON && !pfile.HSLocateParam[index].THD2_ChannelON && !pfile.HSLocateParam[index].THD3_ChannelON)
			{
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			if (!doThdImage(pfile.HSLocateParam[index]))
			{
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			ErrorMsg = "";
			if (pfile.HSLocateParam[index].Direction < ScanDirection.EM_LEFT_RIGHT_HOLE)
			{
				if (!ScanHLine(directionT2B: pfile.HSLocateParam[index].Direction == ScanDirection.EM_TOP_BOTTOM, Binary: FinalBinary, roi: InspeRect, ResultY: out pfile.HSLocateParam[index].Value, FindEdgeMethod: pfile.HSLocateParam[index].FindEdgeMethod, UseStdFilter: pfile.HSLocateParam[index].UseStdFilter))
				{
					iresult = TOOL_RESULT.EM_NG;
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[水平 {0} {1}] 掃描合格點數小於2。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[水平 {0} {1}] 扫描合格点数小于2。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[水平 {0} {1}] の検査合格点数が2未満です。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
					}
					else
					{
						ErrorMsg += string.Format("[Horizontal {0} {1}] Pass edge point < 2。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
					}
					return false;
				}
				return true;
			}
			if (!ScanVLineHole(directionL2R: pfile.HSLocateParam[index].Direction == ScanDirection.EM_LEFT_RIGHT_HOLE, Binary: FinalBinary, roi: InspeRect, ResultY: out pfile.HSLocateParam[index].Value, FindEdgeMethod: pfile.HSLocateParam[index].FindEdgeMethod))
			{
				iresult = TOOL_RESULT.EM_NG;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[水平 {0} {1}] 掃描合格點數小於2。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[水平 {0} {1}] 扫描合格点数小于2。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[水平 {0} {1}] の検査合格点数が2未満です。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
				}
				else
				{
					ErrorMsg += string.Format("[Horizontal {0} {1}] Pass edge point < 2。\n", index + 1, pfile.HSLocateParam[index].TrackerName);
				}
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT02-AdvLocateTool-doHScan()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-002");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		try
		{
			OffLine = offline;
			iresult = TOOL_RESULT.EM_OK;
			pfile.AdvLocateParam[index].Width = 0;
			pfile.AdvLocateParam[index].Height = 0;
			pfile.AdvLocateParam[index].InsideRect = (NGRect = new Rect(0, 0, 0, 0));
			double advAHeight = (AdvAWidth = 0.0);
			AdvAHeight = advAHeight;
			if (!pfile.AdvLocateParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.AdvLocateParam[index].EnableSelectFace && FaceIndex != pfile.AdvLocateParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			ErrorMsg = "";
			Rect rect = new Rect(0, 0, 0, 0);
			if (!pfile.AdvLocateParam[index].UseBinary)
			{
				int num2 = (int)(pfile.AdvLocateParam[index].SearchRectShiftL / pfile.PixelPerUM_X);
				int num3 = (int)(pfile.AdvLocateParam[index].SearchRectShiftR / pfile.PixelPerUM_X);
				int num4 = (int)(pfile.AdvLocateParam[index].SearchRectShiftT / pfile.PixelPerUM_Y);
				int num5 = (int)(pfile.AdvLocateParam[index].SearchRectShiftB / pfile.PixelPerUM_Y);
				pfile.AdvLocateParam[index].T = pfile.HSLocateParam[pfile.AdvLocateParam[index].RectIndexT].Value + num4;
				pfile.AdvLocateParam[index].B = pfile.HSLocateParam[pfile.AdvLocateParam[index].RectIndexB].Value - num5;
				pfile.AdvLocateParam[index].L = pfile.VSLocateParam[pfile.AdvLocateParam[index].RectIndexL].Value + num2;
				pfile.AdvLocateParam[index].R = pfile.VSLocateParam[pfile.AdvLocateParam[index].RectIndexR].Value - num3;
				bool flag = true;
				bool flag2 = true;
				bool flag3 = true;
				if (pfile.AdvLocateParam[index].T < 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位上邊掃描失敗\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位上边扫描失败\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置上部スキャン失敗。\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Top side scan error!\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					iresult = TOOL_RESULT.EM_NG;
				}
				if (pfile.AdvLocateParam[index].B < 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位下邊掃描失敗\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位下边扫描失败\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置下部スキャン失敗。\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 0)
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Bottom side scan error!\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					iresult = TOOL_RESULT.EM_NG;
				}
				if (pfile.AdvLocateParam[index].L < 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位左邊掃描失敗\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位左边扫描失败\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置左部スキャン失敗。\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Left side scan error!\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					iresult = TOOL_RESULT.EM_NG;
				}
				if (pfile.AdvLocateParam[index].R < 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位右邊掃描失敗\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位右边扫描失败\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置右部スキャン失敗。\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					else
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Right side scan error!\n", index + 1, pfile.AdvLocateParam[index].TrackerName);
					}
					iresult = TOOL_RESULT.EM_NG;
				}
				if (1 == 0 || !flag || !flag2 || !flag3)
				{
					return false;
				}
				rect = new Rect(pfile.AdvLocateParam[index].L, pfile.AdvLocateParam[index].T, pfile.AdvLocateParam[index].R - pfile.AdvLocateParam[index].L, pfile.AdvLocateParam[index].B - pfile.AdvLocateParam[index].T);
			}
			else
			{
				rect = doThdImage(pfile.AdvLocateParam[index], index);
				pfile.AdvLocateParam[index].T = rect.Top;
				pfile.AdvLocateParam[index].B = rect.Top + rect.Height;
				pfile.AdvLocateParam[index].L = rect.Left;
				pfile.AdvLocateParam[index].R = rect.Left + rect.Width;
			}
			pfile.AdvLocateParam[index].InsideRect = rect;
			NGRect = rect;
			double num6 = (double)rect.Width * pfile.PixelPerUM_X;
			double num7 = (double)rect.Height * pfile.PixelPerUM_Y;
			if (pfile.AdvLocateParam[index].EnableFakeWidthRatio)
			{
				num6 *= pfile.AdvLocateParam[index].WidthRatio;
			}
			if (pfile.AdvLocateParam[index].EnableFakeHeightRatio)
			{
				num7 *= pfile.AdvLocateParam[index].HeightRatio;
			}
			AdvAWidth = num6;
			AdvAHeight = num7;
			double comDigitsValue = GetComDigitsValue(pfile.AdvLocateParam[index].WidthMin);
			double comDigitsValue2 = GetComDigitsValue(pfile.AdvLocateParam[index].WidthMax);
			double comDigitsValue3 = GetComDigitsValue(pfile.AdvLocateParam[index].HeightMin);
			double comDigitsValue4 = GetComDigitsValue(pfile.AdvLocateParam[index].HeightMax);
			if (num6 < comDigitsValue || num6 > comDigitsValue2 || num7 < comDigitsValue3 || num7 > comDigitsValue4 || num6 < 0.0 || num7 < 0.0)
			{
				if (num6 < comDigitsValue || num6 <= 0.0)
				{
					if (pfile.MeasureUnit == 0)
					{
						if (pfile.UseFloat)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位寬度{2:N3}um小於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位宽度{2:N3}um小于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置幅が{2:N3}μmが設定値{3}未満です。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
							}
							else
							{
								ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Width={2:N3}um, Allow min Rect width={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位寬度{2}um小於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位宽度{2}um小于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置幅が{2}μmが設定値{3}未満です。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue);
						}
						else if (buffers.LanguageType == 0)
						{
							ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Width={2}um, Allow min Rect width={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位寬度{2:N3}mm小於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位宽度{2:N3}mm小于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置幅が{2:N3}μmが設定値{3}未満です。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
					}
					else if (buffers.LanguageType == 0)
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Width={2:N3}mm, Allow min Rect width={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue);
					}
				}
				if (num6 > comDigitsValue2)
				{
					if (pfile.MeasureUnit == 0)
					{
						if (pfile.UseFloat)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位寬度{2:N3}um大於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位宽度{2:N3}um大于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置幅が{2:N3}μmが設定値{3}より大きいです。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
							}
							else
							{
								ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Width={2:N3}um, Allow max Rect width={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位寬度{2}um大於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue2);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位宽度{2}um大于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue2);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置幅が{2}μmが設定値{3}より大きいです。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue2);
						}
						else
						{
							ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Width={2}um, Allow max Rect width={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num6, comDigitsValue2);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位寬度{2:N3}mm大於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位宽度{2:N3}mm大于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置幅が{2}mmが設定値{3}より大きいです。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
					}
					else
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Width={2:N3}mm, Allow max Rect width={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num6, comDigitsValue2);
					}
				}
				if (num7 < comDigitsValue3 || num7 <= 0.0)
				{
					if (pfile.MeasureUnit == 0)
					{
						if (pfile.UseFloat)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位高度{2:N3}um小於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位高度{2:N3}um小于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置高さ{2:N3}μmが設定値{3}未満です。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
							}
							else
							{
								ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Height={2:N3}um, Allow min Rect Height={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位高度{2}um小於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue3);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位高度{2}um小于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue3);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置高さ{2}μmが設定値{3}未満です。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue3);
						}
						else
						{
							ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Height={2}um, Allow min Rect Height={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue3);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位高度{2:N3}mm小於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位高度{2:N3}mm小于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置高さ{2:N3}mmが設定値{3}未満です。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
					}
					else
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Height={2:N3}mm, Allow min Rect Height={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue3);
					}
				}
				if (num7 > comDigitsValue4)
				{
					if (pfile.MeasureUnit == 0)
					{
						if (pfile.UseFloat)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位高度{2:N3}um大於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位高度{2:N3}um大于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置高さ{2:N3}μmが設定値{3}より大きいです\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
							}
							else
							{
								ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Height={2:N3}um, Allow max Rect Height={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位高度{2}um大於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue4);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位高度{2}um大于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue4);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置高さ{2}μmが設定値{3}より大きいです。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue4);
						}
						else
						{
							ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Height={2}um, Allow max Rect Height={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, (int)num7, comDigitsValue4);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[內部定位 {0} {1}] 內部定位高度{2:N3}mm大於設定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[内部定位 {0} {1}] 内部定位高度{2:N3}mm大于设定值{3}。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[内部配置 {0} {1}] 内部配置高さ{2:N3}mmが設定値{3}より大きいです。\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
					}
					else
					{
						ErrorMsg += string.Format("[Inside Locate {0} {1}] Rect Height={2:N3}mm, Allow max Rect Height={3}\n", index + 1, pfile.AdvLocateParam[index].TrackerName, num7, comDigitsValue4);
					}
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			pfile.AdvLocateParam[index].Width = rect.Width;
			pfile.AdvLocateParam[index].Height = rect.Height;
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT03-AdvLocateTool-Inspect()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool doAdvMeasure(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool OffLine = true)
	{
		try
		{
			iresult = TOOL_RESULT.EM_OK;
			OfflineTime = 0.0;
			pfile.AdvLocateMParam[index].MeasureValue = 0.0;
			MeasureDistance = 0.0;
			if (!pfile.AdvLocateMParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.AdvLocateMParam[index].EnableSelectFace && FaceIndex != pfile.AdvLocateMParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			double num = 0.0;
			ErrorMsg = "";
			NGPt1.X = (NGPt1.Y = (NGPt2.X = (NGPt2.Y = 0)));
			switch (pfile.AdvLocateMParam[index].MeasureFunction)
			{
			case 0:
			{
				Point point4 = new Point(0, 0);
				double num6;
				if (pfile.AdvLocateMParam[index].MStartSelect == 0)
				{
					NGRect = new Rect(0, 0, 0, 0);
					num6 = GetVerticalAxisX(pfile.AdvLocateMParam[index].MStartWHIndex);
				}
				else
				{
					NGRect = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MStartAIndex].InsideRect;
					point4.Y = NGRect.Top + NGRect.Height / 2;
					num6 = GetAdvLocateX(pfile.AdvLocateMParam[index].MStartAIndex, pfile.AdvLocateMParam[index].MStartAPt);
				}
				double num7;
				if (pfile.AdvLocateMParam[index].MEndSelect == 0)
				{
					NGRect2 = new Rect(0, 0, 0, 0);
					num7 = GetVerticalAxisX(pfile.AdvLocateMParam[index].MEndWHIndex);
				}
				else
				{
					NGRect2 = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MEndAIndex].InsideRect;
					point4.Y = NGRect.Top + NGRect.Height / 2;
					num7 = GetAdvLocateX(pfile.AdvLocateMParam[index].MEndAIndex, pfile.AdvLocateMParam[index].MEndAPt);
				}
				ref Point nGPt4 = ref NGPt1;
				int y = (Line1 = (int)num6);
				nGPt4.X = y;
				ref Point nGPt5 = ref NGPt2;
				y = (Line2 = (int)num7);
				nGPt5.X = y;
				if (point4.Y == 0)
				{
					NGPt1.Y = (NGPt2.Y = 15);
				}
				else
				{
					NGPt1.Y = (NGPt2.Y = point4.Y);
				}
				num = (num7 - num6) * pfile.PixelPerUM_X;
				pfile.AdvLocateMParam[index].MeasureValue = num;
				break;
			}
			case 1:
			{
				Point point3 = new Point(0, 0);
				double num2;
				if (pfile.AdvLocateMParam[index].MStartSelect == 0)
				{
					NGRect = new Rect(0, 0, 0, 0);
					num2 = GetHorizentalAxisY(pfile.AdvLocateMParam[index].MStartWHIndex);
				}
				else
				{
					NGRect = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MStartAIndex].InsideRect;
					point3.X = NGRect2.Left + NGRect2.Width / 2;
					num2 = GetAdvLocateY(pfile.AdvLocateMParam[index].MStartAIndex, pfile.AdvLocateMParam[index].MStartAPt);
				}
				double num3;
				if (pfile.AdvLocateMParam[index].MEndSelect == 0)
				{
					NGRect2 = new Rect(0, 0, 0, 0);
					num3 = GetHorizentalAxisY(pfile.AdvLocateMParam[index].MEndWHIndex);
				}
				else
				{
					NGRect2 = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MEndAIndex].InsideRect;
					point3.X = NGRect2.Left + NGRect2.Width / 2;
					num3 = GetAdvLocateY(pfile.AdvLocateMParam[index].MEndAIndex, pfile.AdvLocateMParam[index].MEndAPt);
				}
				ref Point nGPt2 = ref NGPt1;
				int y = (Line1 = (int)num2);
				nGPt2.Y = y;
				ref Point nGPt3 = ref NGPt2;
				y = (Line2 = (int)num3);
				nGPt3.Y = y;
				if (point3.X == 0)
				{
					NGPt1.X = (NGPt2.X = 15);
				}
				else
				{
					NGPt1.X = (NGPt2.X = point3.X);
				}
				num = (num3 - num2) * pfile.PixelPerUM_Y;
				pfile.AdvLocateMParam[index].MeasureValue = num;
				break;
			}
			case 2:
			{
				Point2f point2f = new Point2f(0f, 0f);
				Point2f point2f2 = new Point2f(0f, 0f);
				point2f = GetAdvLocatePt(pfile.AdvLocateMParam[index].MStartAIndex, pfile.AdvLocateMParam[index].MStartAPt);
				point2f2 = GetAdvLocatePt(pfile.AdvLocateMParam[index].MEndAIndex, pfile.AdvLocateMParam[index].MEndAPt);
				NGRect = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MStartAIndex].InsideRect;
				NGRect2 = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MEndAIndex].InsideRect;
				Point nGPt = (PT1 = (Point)point2f);
				NGPt1 = nGPt;
				nGPt = (PT2 = (Point)point2f2);
				NGPt2 = nGPt;
				num = CalcDistance(point2f, point2f2);
				pfile.AdvLocateMParam[index].MeasureValue = num;
				break;
			}
			case 3:
			{
				Point MinPt = new Point(0, 0);
				Point MinPt2 = new Point(0, 0);
				NGRect = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MStartWHIndex].InsideRect;
				NGRect2 = pfile.AdvLocateParam[pfile.AdvLocateMParam[index].MEndWHIndex].InsideRect;
				num = MeasureKValue(index, ref MinPt, ref MinPt2, OffLine);
				if (pfile.ThroughputFile.param.EnableCustomRatio && pfile.ThroughputFile.param.CustomRatio > 0.0)
				{
					num *= pfile.ThroughputFile.param.CustomRatio;
				}
				pfile.AdvLocateMParam[index].MeasureValue = num;
				Point nGPt = (PT1 = MinPt);
				NGPt1 = nGPt;
				nGPt = (PT2 = MinPt2);
				NGPt2 = nGPt;
				break;
			}
			case 4:
				num = Math.Abs(pfile.AdvLocateMParam[pfile.AdvLocateMParam[index].MStartWHIndex].MeasureValue - pfile.AdvLocateMParam[pfile.AdvLocateMParam[index].MEndWHIndex].MeasureValue);
				pfile.AdvLocateMParam[index].MeasureValue = num;
				NGRect = new Rect(0, 0, 0, 0);
				NGRect2 = new Rect(0, 0, 0, 0);
				break;
			}
			MeasureDistance = num;
			double comDigitsValue = GetComDigitsValue(pfile.AdvLocateMParam[index].ValueMin);
			double comDigitsValue2 = GetComDigitsValue(pfile.AdvLocateMParam[index].ValueMax);
			if (num < comDigitsValue || num > comDigitsValue2)
			{
				if (pfile.MeasureUnit == 0)
				{
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg += string.Format("[{0}: {1}] 距離={2:N3}um，超過設定值範圍!\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += string.Format("[{0}: {1}] 距离={2:N3}um，超过设定值范围!\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += string.Format("[{0}: {1}] 距離は{2:N3}μmで、設定値の範囲を超えています。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
						}
						else
						{
							ErrorMsg += string.Format("[{0}: {1}] Distance over range, distance={2}um \n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[{0}: {1}] 距離={2}um，超過設定值範圍!\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[{0}: {1}] 距离={2}um，超过设定值范围!\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[{0}: {1}] 距離は{2}μmで、設定値の範囲を超えています。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
					}
					else
					{
						ErrorMsg += string.Format("[{0}: {1}] Distance over range, distance={2}um \n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[{0}: {1}] 距離={2:N3}mm，超過設定值範圍!\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[{0}: {1}] 距离={2:N3}mm，超过设定值范围!\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[{0}: {1}] 距離は{2}μmで、設定値の範囲を超えています。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
				}
				else
				{
					ErrorMsg += string.Format("[{0}: {1}] Distance over range, distance={2}mm \n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
				}
				iresult = TOOL_RESULT.EM_NG;
				return false;
			}
			if (pfile.MeasureUnit == 0)
			{
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += string.Format("[{0} {1}] 距離={2:N3}um。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += string.Format("[{0} {1}] 距离={2:N3}um。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += string.Format("[{0} {1}] 距離は{2:N3}μmです。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
					}
					else
					{
						ErrorMsg += string.Format("[{0} {1}] distance={2:N3}um \n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("[{0} {1}] 距離={2}um。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("[{0} {1}] 距离={2}um。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("[{0} {1}] 距離は{2}μmです。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
				}
				else
				{
					ErrorMsg += string.Format("[{0} {1}] distance={2}um \n", index + 1, pfile.AdvLocateMParam[index].TrackerName, (int)num);
				}
			}
			else if (buffers.LanguageType == 1)
			{
				ErrorMsg += string.Format("[{0} {1}] 距離={2:N3}mm。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg += string.Format("[{0} {1}] 距离={2:N3}mm。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg += string.Format("[{0} {1}] 距離は{2}μmです。\n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
			}
			else
			{
				ErrorMsg += string.Format("[{0} {1}] distance={2}mm \n", index + 1, pfile.AdvLocateMParam[index].TrackerName, num);
			}
		}
		catch (Exception ex)
		{
			string text = "AT03-AdvLocateTool-doAdvMeasure()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-004");
			}
			return false;
		}
		return true;
	}

	private void FillHole(ref Mat Src)
	{
		using Mat mat = new Mat(Src.Height + 2, Src.Width + 2, MatType.CV_8UC1);
		mat.SetTo(0.0);
		Mat mat2 = new Mat(mat, new Rect(1, 1, Src.Width, Src.Height));
		Src.CopyTo(mat2);
		Cv2.FloodFill(mat, new Point(0, 0), new Scalar(255.0));
		Cv2.BitwiseNot(mat2, mat2);
		Cv2.BitwiseOr(Src, mat2, Src);
	}

	private double MeasureKValue(int index, ref Point MinPt1, ref Point MinPt2, bool OffLine = true)
	{
		double result = 0.0;
		int num = pfile.AdvLocateMParam[index].MStartWHIndex;
		int num2 = pfile.AdvLocateMParam[index].MEndWHIndex;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		Line1Pt.Clear();
		Line2Pt.Clear();
		Point[][] contours = null;
		Point[][] contours2 = null;
		int num3 = 0;
		int num4 = 0;
		Point point;
		if (AdvLocateBinary[num].Width > 0 && AdvLocateBinary[num].Height > 0 && pfile.AdvLocateParam[num].UseBinary)
		{
			Cv2.FindContours(AdvLocateBinary[num], out contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			if (contours.Length == 0)
			{
				return result;
			}
			point = new Point(pfile.AdvLocateParam[num].L + AdvLocateBinary[num].Width / 2, pfile.AdvLocateParam[num].T + AdvLocateBinary[num].Height / 2);
			double num5 = 0.0;
			for (int i = 0; i < contours.Length; i++)
			{
				double num6 = Cv2.ContourArea(contours[i]);
				if (num6 > num5)
				{
					num5 = num6;
					num3 = i;
				}
			}
			for (int j = 0; j < contours[num3].Length; j++)
			{
				contours[num3][j].X += NGRect.Left;
				contours[num3][j].Y += NGRect.Top;
				Line1Pt.Add(contours[num3][j]);
			}
		}
		else
		{
			if (pfile.AdvLocateParam[num].InsideRect.Width <= 0 || pfile.AdvLocateParam[num].InsideRect.Height <= 0)
			{
				return result;
			}
			int top = pfile.AdvLocateParam[num].InsideRect.Top;
			int bottom = pfile.AdvLocateParam[num].InsideRect.Bottom;
			int left = pfile.AdvLocateParam[num].InsideRect.Left;
			int right = pfile.AdvLocateParam[num].InsideRect.Right;
			if (pfile.AdvLocateMParam[index].MeasureAllY)
			{
				for (int k = left; k < right; k++)
				{
					Line1Pt.Add(new Point(k, top));
				}
				for (int l = left; l < right; l++)
				{
					Line1Pt.Add(new Point(l, bottom));
				}
			}
			if (pfile.AdvLocateMParam[index].MeasureAllX)
			{
				for (int m = top; m < bottom; m++)
				{
					Line1Pt.Add(new Point(left, m));
				}
				for (int n = top; n < bottom; n++)
				{
					Line1Pt.Add(new Point(right, n));
				}
			}
			point = new Point(pfile.AdvLocateParam[num].InsideRect.Left + pfile.AdvLocateParam[num].InsideRect.Width / 2, pfile.AdvLocateParam[num].InsideRect.Top + pfile.AdvLocateParam[num].InsideRect.Height / 2);
		}
		Point point2;
		if (AdvLocateBinary[num2].Width > 0 && AdvLocateBinary[num2].Height > 0 && pfile.AdvLocateParam[num2].UseBinary)
		{
			Cv2.FindContours(AdvLocateBinary[num2], out contours2, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			if (contours2.Length == 0)
			{
				return result;
			}
			point2 = new Point(pfile.AdvLocateParam[num2].L + AdvLocateBinary[num2].Width / 2, pfile.AdvLocateParam[num2].T + AdvLocateBinary[num2].Height / 2);
			double num7 = 0.0;
			num7 = 0.0;
			for (int num8 = 0; num8 < contours2.Length; num8++)
			{
				double num9 = Cv2.ContourArea(contours2[num8]);
				if (num9 > num7)
				{
					num7 = num9;
					num4 = num8;
				}
			}
			Rect rect = Cv2.BoundingRect(contours2[num4]);
			int scanExtend = pfile.AdvLocateMParam[index].ScanExtend;
			int scanShrink = pfile.AdvLocateMParam[index].ScanShrink;
			if (pfile.AdvLocateMParam[index].MeasureAllY && !pfile.AdvLocateMParam[index].MeasureAllX)
			{
				Rect roi = new Rect(rect.Left + scanShrink, rect.Top, rect.Right - scanShrink * 2, rect.Top + scanExtend);
				Rect roi2 = new Rect(rect.Left + scanShrink, rect.Bottom - scanExtend, rect.Right - scanShrink * 2, rect.Bottom);
				Rect rect2 = new Rect(0, 0, AdvLocateBinary[num2].Width, AdvLocateBinary[num2].Height);
				roi &= rect2;
				roi2 &= rect2;
				Point trackPos = new Point(NGRect2.Left, NGRect2.Top);
				ScanGetHLine(AdvLocateBinary[num2], roi, out var edgept, trackPos);
				ScanGetHLine(AdvLocateBinary[num2], roi2, out var edgept2, trackPos, directionT2B: false);
				Line2Pt.Clear();
				for (int num10 = 0; num10 < edgept.Count; num10++)
				{
					Line2Pt.Add(edgept[num10]);
				}
				for (int num11 = 0; num11 < edgept2.Count; num11++)
				{
					Line2Pt.Add(edgept2[num11]);
				}
			}
			else if (pfile.AdvLocateMParam[index].MeasureAllX && !pfile.AdvLocateMParam[index].MeasureAllY)
			{
				Rect roi3 = new Rect(rect.Left, rect.Top + scanShrink, rect.Left + scanExtend, rect.Bottom - scanShrink * 2);
				Rect roi4 = new Rect(rect.Right - scanExtend, rect.Top + scanShrink, rect.Right, rect.Bottom - scanShrink * 2);
				Rect rect3 = new Rect(0, 0, AdvLocateBinary[num2].Width, AdvLocateBinary[num2].Height);
				roi3 &= rect3;
				roi4 &= rect3;
				Point trackPos2 = new Point(NGRect2.Left, NGRect2.Top);
				ScanGetVLine(AdvLocateBinary[num2], roi3, out var edgept3, trackPos2);
				ScanGetVLine(AdvLocateBinary[num2], roi4, out var edgept4, trackPos2, directionL2R: false);
				Line2Pt.Clear();
				for (int num12 = 0; num12 < edgept3.Count; num12++)
				{
					Line2Pt.Add(edgept3[num12]);
				}
				for (int num13 = 0; num13 < edgept4.Count; num13++)
				{
					Line2Pt.Add(edgept4[num13]);
				}
			}
			else
			{
				for (int num14 = 0; num14 < contours2[num4].Length; num14++)
				{
					contours2[num4][num14].X += NGRect2.Left;
					contours2[num4][num14].Y += NGRect2.Top;
					Line2Pt.Add(contours2[num4][num14]);
				}
			}
		}
		else
		{
			if (pfile.AdvLocateParam[num2].InsideRect.Width <= 0 || pfile.AdvLocateParam[num2].InsideRect.Height <= 0)
			{
				return result;
			}
			int top2 = pfile.AdvLocateParam[num2].InsideRect.Top;
			int bottom2 = pfile.AdvLocateParam[num2].InsideRect.Bottom;
			int left2 = pfile.AdvLocateParam[num2].InsideRect.Left;
			int right2 = pfile.AdvLocateParam[num2].InsideRect.Right;
			if (pfile.AdvLocateMParam[index].MeasureAllY)
			{
				for (int num15 = left2; num15 < right2; num15++)
				{
					Line2Pt.Add(new Point(num15, top2));
				}
				for (int num16 = left2; num16 < right2; num16++)
				{
					Line2Pt.Add(new Point(num16, bottom2));
				}
			}
			if (pfile.AdvLocateMParam[index].MeasureAllX)
			{
				for (int num17 = top2; num17 < bottom2; num17++)
				{
					Line2Pt.Add(new Point(left2, num17));
				}
				for (int num18 = top2; num18 < bottom2; num18++)
				{
					Line2Pt.Add(new Point(right2, num18));
				}
			}
			point2 = new Point(pfile.AdvLocateParam[num2].InsideRect.Left + pfile.AdvLocateParam[num2].InsideRect.Width / 2, pfile.AdvLocateParam[num2].InsideRect.Top + pfile.AdvLocateParam[num2].InsideRect.Height / 2);
		}
		if (Line1Pt.Count <= 0 || Line2Pt.Count <= 0)
		{
			return result;
		}
		double num19 = 100000.0;
		List<Point> list = new List<Point>();
		if (pfile.AdvLocateMParam[index].MeasureAllX || pfile.AdvLocateMParam[index].MeasureAllY)
		{
			if (Line1Pt.Count <= 0 || Line2Pt.Count <= 0)
			{
				num19 = 0.0;
			}
			else
			{
				for (int num20 = 0; num20 < Line2Pt.Count; num20++)
				{
					list.Clear();
					Point point3 = Line2Pt[num20];
					for (int num21 = 0; num21 < Line1Pt.Count; num21++)
					{
						if ((double)Math.Abs(Line1Pt[num21].X - point3.X) < num19 && (double)Math.Abs(Line1Pt[num21].Y - point3.Y) < num19)
						{
							list.Add(Line1Pt[num21]);
						}
					}
					if (list.Count > 0)
					{
						double num22 = Math.Abs(Cv2.PointPolygonTest(list, point3, measureDist: true));
						if (num22 < num19)
						{
							num19 = num22;
							MinPt1 = point3;
						}
					}
				}
				num19 *= (pfile.PixelPerUM_X + pfile.PixelPerUM_Y) / 2.0;
			}
			if (OffLine)
			{
				double num23 = 100000.0;
				sw.Reset();
				sw.Start();
				for (int num24 = 0; num24 < Line1Pt.Count; num24++)
				{
					Point point4 = Line1Pt[num24];
					for (int num25 = 0; num25 < Line2Pt.Count; num25++)
					{
						Point point5 = Line2Pt[num25];
						double num26 = Math.Abs(CalcDistance2(point4, point5));
						if (num26 < num23)
						{
							num23 = num26;
							MinPt1 = point4;
							MinPt2 = point5;
						}
					}
				}
				sw.Stop();
				OfflineTime = sw.Elapsed.TotalMilliseconds;
			}
		}
		else if (Math.Abs(point.X - point2.X) > Math.Abs(point.Y - point2.Y))
		{
			if (point.X < point2.X)
			{
				for (int num27 = 0; num27 < Line1Pt.Count; num27++)
				{
					Point point6 = new Point(Line1Pt[num27].X, Line1Pt[num27].Y);
					if (point6.X < point.X)
					{
						continue;
					}
					for (int num28 = 0; num28 < Line2Pt.Count; num28++)
					{
						Point point7 = new Point(Line2Pt[num28].X, Line2Pt[num28].Y);
						if (point7.X <= point2.X)
						{
							double num29 = Math.Abs(CalcDistance2(point6, point7));
							if (num29 < num19)
							{
								num19 = num29;
								MinPt1 = point6;
								MinPt2 = point7;
							}
						}
					}
				}
			}
			else
			{
				for (int num30 = 0; num30 < Line1Pt.Count; num30++)
				{
					Point point8 = new Point(Line1Pt[num30].X, Line1Pt[num30].Y);
					if (point8.X > point.X)
					{
						continue;
					}
					for (int num31 = 0; num31 < Line2Pt.Count; num31++)
					{
						Point point9 = new Point(Line2Pt[num31].X, Line2Pt[num31].Y);
						if (point9.X >= point2.X)
						{
							double num32 = Math.Abs(CalcDistance2(point8, point9));
							if (num32 < num19)
							{
								num19 = num32;
								MinPt1 = point8;
								MinPt2 = point9;
							}
						}
					}
				}
			}
		}
		else if (point.Y < point2.Y)
		{
			for (int num33 = 0; num33 < Line1Pt.Count; num33++)
			{
				Point point10 = new Point(Line1Pt[num33].X, Line1Pt[num33].Y);
				if (point10.Y < point.Y)
				{
					continue;
				}
				for (int num34 = 0; num34 < Line2Pt.Count; num34++)
				{
					Point point11 = new Point(Line2Pt[num34].X, Line2Pt[num34].Y);
					if (point11.Y <= point2.Y)
					{
						double num35 = Math.Abs(CalcDistance(point10, point11));
						if (num35 < num19)
						{
							num19 = num35;
							MinPt1 = point10;
							MinPt2 = point11;
						}
					}
				}
			}
		}
		else
		{
			for (int num36 = 0; num36 < Line1Pt.Count; num36++)
			{
				Point point12 = new Point(Line1Pt[num36].X, Line1Pt[num36].Y);
				if (point12.Y > point.Y)
				{
					continue;
				}
				for (int num37 = 0; num37 < Line2Pt.Count; num37++)
				{
					Point point13 = new Point(Line2Pt[num37].X, Line2Pt[num37].Y);
					if (point13.Y >= point2.Y)
					{
						double num38 = Math.Abs(CalcDistance(point12, point13));
						if (num38 < num19)
						{
							num19 = num38;
							MinPt1 = point12;
							MinPt2 = point13;
						}
					}
				}
			}
		}
		return num19;
	}

	private int GetVerticalAxisX(int index)
	{
		if (index < 0 || index >= 20)
		{
			index = 0;
		}
		return pfile.VSLocateParam[index].Value;
	}

	private int GetHorizentalAxisY(int index)
	{
		if (index < 0 || index >= 20)
		{
			index = 0;
		}
		return pfile.HSLocateParam[index].Value;
	}

	public int GetAdvLocateX(int index, int RectIndex)
	{
		if (index < 0 || index >= 20)
		{
			index = 0;
		}
		int result = 0;
		switch (RectIndex)
		{
		case 0:
		case 2:
		case 4:
			result = pfile.AdvLocateParam[index].InsideRect.Left;
			break;
		case 1:
		case 3:
		case 5:
			result = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width;
			break;
		case 6:
		case 7:
		case 8:
			result = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width / 2;
			break;
		}
		return result;
	}

	public int GetAdvLocateY(int index, int RectIndex)
	{
		if (index < 0 || index >= 20)
		{
			index = 0;
		}
		int result = 0;
		switch (RectIndex)
		{
		case 0:
		case 1:
		case 6:
			result = pfile.AdvLocateParam[index].InsideRect.Top;
			break;
		case 2:
		case 3:
		case 7:
			result = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height;
			break;
		case 4:
		case 5:
		case 8:
			result = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height / 2;
			break;
		}
		return result;
	}

	public Point2f GetAdvLocatePt(int index, int RectIndex)
	{
		if (index < 0 || index >= 20)
		{
			index = 0;
		}
		Point2f result = new Point2f(0f, 0f);
		switch (RectIndex)
		{
		case 0:
			return pfile.AdvLocateParam[index].InsideRect.TopLeft;
		case 1:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top;
			break;
		case 2:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height;
			break;
		case 3:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height;
			break;
		case 4:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height / 2;
			break;
		case 5:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height / 2;
			break;
		case 6:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width / 2;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top;
			break;
		case 7:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width / 2;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height;
			break;
		case 8:
			result.X = pfile.AdvLocateParam[index].InsideRect.Left + pfile.AdvLocateParam[index].InsideRect.Width / 2;
			result.Y = pfile.AdvLocateParam[index].InsideRect.Top + pfile.AdvLocateParam[index].InsideRect.Height / 2;
			break;
		}
		return result;
	}

	private double CalcDistance(Point2f a, Point2f b)
	{
		Point2f point2f = a - b;
		point2f.X *= (float)pfile.PixelPerUM_X;
		point2f.Y *= (float)pfile.PixelPerUM_Y;
		return Math.Sqrt(point2f.X * point2f.X + point2f.Y * point2f.Y);
	}

	private double CalcDistance2(Point2f a, Point2f b)
	{
		double num = (double)(a.X - b.X) * pfile.PixelPerUM_X;
		double num2 = (double)(a.Y - b.Y) * pfile.PixelPerUM_Y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	private double CalcDistance3(Point2f a, Point2f b)
	{
		double num = a.X - b.X;
		double num2 = a.Y - b.Y;
		return num + num2;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Rect findBlob(Mat binary, Rect rect, int index)
	{
		Rect rect2 = new Rect(0, 0, 0, 0);
		try
		{
			if (binary.CountNonZero() <= 3)
			{
				rect2.Left += rect.Left;
				rect2.Top += rect.Top;
				return rect2;
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num2 = 0;
			int i = 0;
			if (num > 1)
			{
				for (int j = 1; j < num; j++)
				{
					if (genericIndexer[j, 4] > num2)
					{
						i = j;
						num2 = genericIndexer[j, 4];
					}
				}
				rect2 = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
			}
			else
			{
				rect2 = new Rect(0, 0, 0, 0);
			}
			if (rect2.Width > 0 || rect2.Height > 0)
			{
				new Mat(binary, rect2).CopyTo(AdvLocateBinary[index]);
			}
			else
			{
				AdvLocateBinary[index] = new Mat();
			}
			rect2.Left += rect.Left;
			rect2.Top += rect.Top;
			return rect2;
		}
		catch (Exception ex)
		{
			string text = "Adv02-AdvLocateTool-findBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-005");
			}
			return rect2;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Rect doThdImage(INSP_ADVLOCATETOOL_PARAM param, int index)
	{
		Rect result = new Rect(0, 0, 0, 0);
		try
		{
			bool[] array = new bool[3];
			GetInspeRect(param);
			InspeRect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				return result;
			}
			int num = 0;
			if (param.THD1_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect).CopyTo(Binary[0]);
				if (param.DE_Start1_Index != USE_DILATE_ERODE.EM_USE_CANNY)
				{
					Cv2.InRange(Binary[0], param.THD1_Min, param.THD1_Max, Binary[0]);
				}
				doMorph(ref Binary[0], (int)param.DE_Start1_Index, param.DE_Start1_Count);
				doMorph(ref Binary[0], (int)param.DE_End1_Index, param.DE_End1_Count);
				num++;
				array[0] = true;
			}
			else
			{
				array[0] = false;
			}
			if (param.THD2_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD2_ChannelIndex), InspeRect).CopyTo(Binary[1]);
				Cv2.InRange(Binary[1], param.THD2_Min, param.THD2_Max, Binary[1]);
				doMorph(ref Binary[1], (int)param.DE_Start2_Index, param.DE_Start2_Count);
				doMorph(ref Binary[1], (int)param.DE_End2_Index, param.DE_End2_Count);
				num++;
				array[1] = true;
			}
			else
			{
				array[1] = false;
			}
			if (param.THD3_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD3_ChannelIndex), InspeRect).CopyTo(Binary[2]);
				Cv2.InRange(Binary[2], param.THD3_Min, param.THD3_Max, Binary[2]);
				doMorph(ref Binary[2], (int)param.DE_Start3_Index, param.DE_Start3_Count);
				doMorph(ref Binary[2], (int)param.DE_End3_Index, param.DE_End3_Count);
				num++;
				array[2] = true;
			}
			else
			{
				array[2] = false;
			}
			if (num == 0)
			{
				return result;
			}
			new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect).CopyTo(FinalBinary);
			FinalBinary.SetTo(255.0);
			for (int i = 0; i < 3; i++)
			{
				if (array[i])
				{
					Cv2.BitwiseAnd(FinalBinary, Binary[i], FinalBinary);
				}
			}
			return findBlob(FinalBinary, InspeRect, index);
		}
		catch (Exception ex)
		{
			string text = "AT04-AdvLocateTool-doThdImage()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-006");
			}
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool doThdImage(INSP_SCANLINETOOL_PARAM param)
	{
		if (!param.TrackerON)
		{
			return true;
		}
		try
		{
			bool[] array = new bool[3];
			GetInspeRect(param);
			InspeRect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				return true;
			}
			int num = 0;
			if (param.THD1_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect).CopyTo(Binary[0]);
				if (param.DE_Start1_Index != USE_DILATE_ERODE.EM_USE_CANNY)
				{
					Cv2.InRange(Binary[0], param.THD1_Min, param.THD1_Max, Binary[0]);
				}
				doMorph(ref Binary[0], (int)param.DE_Start1_Index, param.DE_Start1_Count);
				doMorph(ref Binary[0], (int)param.DE_End1_Index, param.DE_End1_Count);
				num++;
				array[0] = true;
			}
			else
			{
				array[0] = false;
			}
			if (param.THD2_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD2_ChannelIndex), InspeRect).CopyTo(Binary[1]);
				Cv2.InRange(Binary[1], param.THD2_Min, param.THD2_Max, Binary[1]);
				doMorph(ref Binary[1], (int)param.DE_Start2_Index, param.DE_Start2_Count);
				doMorph(ref Binary[1], (int)param.DE_End2_Index, param.DE_End2_Count);
				num++;
				array[1] = true;
			}
			else
			{
				array[1] = false;
			}
			if (param.THD3_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD3_ChannelIndex), InspeRect).CopyTo(Binary[2]);
				Cv2.InRange(Binary[2], param.THD3_Min, param.THD3_Max, Binary[2]);
				doMorph(ref Binary[2], (int)param.DE_Start3_Index, param.DE_Start3_Count);
				doMorph(ref Binary[2], (int)param.DE_End3_Index, param.DE_End3_Count);
				num++;
				array[2] = true;
			}
			else
			{
				array[2] = false;
			}
			if (num == 0)
			{
				return true;
			}
			new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect).CopyTo(FinalBinary);
			FinalBinary.SetTo(255.0);
			for (int i = 0; i < 3; i++)
			{
				if (array[i] && FinalBinary.Width == Binary[i].Width && FinalBinary.Height == Binary[i].Height)
				{
					Cv2.BitwiseAnd(FinalBinary, Binary[i], FinalBinary);
				}
			}
			if (param.UseColorInverse)
			{
				Cv2.BitwiseNot(FinalBinary, FinalBinary);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT04-AdvLocateTool-doThdImage()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-007");
			}
			return false;
		}
	}

	public void GetInspeRect(INSP_ADVLOCATETOOL_PARAM param)
	{
		LocateRect = buffers.LocatedRect;
		InspeRect.X = LocateRect.X + param.SearchRectL;
		InspeRect.Y = LocateRect.Y + param.SearchRectT;
		if (param.UseAutoSize)
		{
			if (param.AutoSizeIndex == 0)
			{
				InspeRect.Width = LocateRect.Width - (param.SearchRectL + param.SearchRectR);
				InspeRect.Height = LocateRect.Height - (param.SearchRectT + param.SearchRectB);
				return;
			}
			Rect insideRect = pfile.AdvLocateParam[param.AutoSizeIndex].InsideRect;
			InspeRect.X = insideRect.X + param.SearchRectL;
			InspeRect.Y = insideRect.Y + param.SearchRectT;
			InspeRect.Width = insideRect.Width - (param.SearchRectL + param.SearchRectR);
			InspeRect.Height = insideRect.Height - (param.SearchRectT + param.SearchRectB);
		}
		else
		{
			InspeRect.Width = param.SearchRectWidth;
			InspeRect.Height = param.SearchRectHeight;
		}
	}

	public void GetInspeRect(INSP_SCANLINETOOL_PARAM param)
	{
		LocateRect = buffers.LocatedRect;
		if (param.UseAutoSize)
		{
			InspeRect.X = LocateRect.X + param.SearchRectL;
			InspeRect.Y = LocateRect.Y + param.SearchRectT;
			InspeRect.Width = LocateRect.Width - (param.SearchRectL + param.SearchRectR);
			InspeRect.Height = LocateRect.Height - (param.SearchRectT + param.SearchRectB);
			return;
		}
		InspeRect.Width = param.SearchRectWidth;
		InspeRect.Height = param.SearchRectHeight;
		if (param.FollowMode == 0)
		{
			InspeRect.X = LocateRect.X + param.SearchRectL;
			InspeRect.Y = LocateRect.Y + param.SearchRectT;
		}
		else if (param.FollowMode == 1)
		{
			InspeRect.X = LocateRect.X + param.SearchRectL;
			InspeRect.Y = LocateRect.Y + LocateRect.Height + param.SearchRectT;
		}
		else if (param.FollowMode == 2)
		{
			InspeRect.X = LocateRect.X + LocateRect.Width + param.SearchRectL;
			InspeRect.Y = LocateRect.Y + param.SearchRectT;
		}
		else
		{
			InspeRect.X = LocateRect.X + LocateRect.Width + param.SearchRectL;
			InspeRect.Y = LocateRect.Y + LocateRect.Height + param.SearchRectT;
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
	{
		if (count != 0 || index == 9)
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
				Cv2.Canny(mat, mat, 0.0, 130.0);
				break;
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanHLine(Mat Binary, Rect roi, out int ResultY, bool directionT2B = true, int FindEdgeMethod = 0, bool UseStdFilter = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			Point[] array = new Point[2];
			List<Point> pt = new List<Point>();
			Rect rect = roi;
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, Binary.Cols - 1, Binary.Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				ResultY = -1;
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
							pt.Add(array[1]);
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
					int num2 = width * (roi.Bottom - 1) + k;
					array[0].Y = 0;
					array[1].Y = 0;
					for (int num3 = roi.Bottom - 1; num3 > roi.Top + 1; num3--)
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
							pt.Add(array[1]);
							break;
						}
						num2 -= width;
					}
				}
			}
			if (pt.Count <= 2)
			{
				ResultY = -1;
				return false;
			}
			if (UseStdFilter)
			{
				StdFilter(ref pt, 1);
				if (pt.Count <= 1)
				{
					ResultY = -1;
					return false;
				}
			}
			int num4 = 0;
			for (int l = 0; l < pt.Count; l++)
			{
				num4 += pt[l].Y;
			}
			double num5 = num4 / pt.Count;
			num4 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 10000;
			for (int m = 0; m < pt.Count; m++)
			{
				num4 += pt[m].Y;
				num6++;
				if (pt[m].Y < num8)
				{
					num8 = pt[m].Y;
				}
				if (pt[m].Y > num7)
				{
					num7 = pt[m].Y;
				}
			}
			if (num6 == 0)
			{
				ResultY = -1;
				return false;
			}
			num5 = num4 / num6;
			switch (FindEdgeMethod)
			{
			case 0:
				ResultY = (int)num5;
				break;
			case 1:
				ResultY = num8;
				break;
			case 2:
				ResultY = num7;
				break;
			default:
				ResultY = (int)num5;
				break;
			}
			ResultY += rect.Top;
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT05-AdvLocateTool-ScanHLine()";
			text += ex.ToString();
			ResultY = 0;
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-008");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanVLine(Mat Binary, Rect roi, out int ResultX, bool directionL2R = true, int FindEdgeMethod = 0, bool UseStdFilter = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			int num = 0;
			Point[] array = new Point[2];
			List<Point> pt = new List<Point>();
			Rect rect = roi;
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, Binary.Cols - 1, Binary.Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				ResultX = -1;
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
							pt.Add(array[1]);
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
					for (int num2 = roi.Right - 1; num2 > roi.Left + 1; num2--)
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
							pt.Add(array[1]);
							break;
						}
					}
				}
			}
			if (pt.Count <= 2)
			{
				ResultX = -1;
				return false;
			}
			if (UseStdFilter)
			{
				StdFilter(ref pt, 0);
				if (pt.Count <= 1)
				{
					ResultX = -1;
					return false;
				}
			}
			int num3 = 0;
			for (int l = 0; l < pt.Count; l++)
			{
				num3 += pt[l].X;
			}
			double num4 = num3 / pt.Count;
			num3 = 0;
			int num5 = 0;
			int num6 = 10000;
			int num7 = 0;
			for (int m = 0; m < pt.Count; m++)
			{
				num3 += pt[m].X;
				num5++;
				if (pt[m].X < num6)
				{
					num6 = pt[m].X;
				}
				if (pt[m].X > num7)
				{
					num7 = pt[m].X;
				}
			}
			if (num5 == 0)
			{
				ResultX = -1;
				return false;
			}
			num4 = num3 / num5;
			switch (FindEdgeMethod)
			{
			case 0:
				ResultX = (int)num4;
				break;
			case 1:
				ResultX = num6;
				break;
			case 2:
				ResultX = num7;
				break;
			default:
				ResultX = (int)num4;
				break;
			}
			ResultX += rect.Left;
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT06-AdvLocateTool-ScanVline()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-009");
			}
			ResultX = 0;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanVLineHole(Mat Binary, Rect roi, out int ResultY, bool directionL2R = true, int FindEdgeMethod = 0, bool UseStdFilter = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			ResultY = -1;
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			int num = 0;
			Point[] array = new Point[2];
			List<Point> pt = new List<Point>();
			Rect rect = roi;
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(1, 1, Binary.Cols - 1, Binary.Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				ResultY = -1;
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
							pt.Add(array[1]);
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
					for (int num2 = roi.Right - 1; num2 > roi.Left + 1; num2--)
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
							pt.Add(array[1]);
							break;
						}
					}
				}
			}
			if (pt.Count <= 1)
			{
				ResultY = -1;
				return false;
			}
			if (UseStdFilter)
			{
				StdFilter(ref pt, 0);
				if (pt.Count <= 1)
				{
					ResultY = -1;
					return false;
				}
			}
			int num3 = 0;
			for (int l = 0; l < pt.Count; l++)
			{
				num3 += pt[l].X;
			}
			_ = num3 / pt.Count;
			num3 = 0;
			int num4 = 10000;
			int num5 = 0;
			int num6 = -1;
			int num7 = -1;
			for (int m = 0; m < pt.Count; m++)
			{
				if (pt[m].X < num4)
				{
					num4 = pt[m].X;
					num6 = pt[m].Y;
				}
				if (pt[m].X > num5)
				{
					num5 = pt[m].X;
					num7 = pt[m].Y;
				}
			}
			if (FindEdgeMethod == 1)
			{
				ResultY = num6;
			}
			else
			{
				ResultY = num7;
			}
			ResultY += rect.Top;
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT06-AdvLocateTool-ScanVlineHole()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-010");
			}
			ResultY = 0;
			return false;
		}
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
			string text = "AT010-AdvLocateTool-GetComDigitsValue()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-011");
			}
			return 0.0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanGetVLine(Mat Binary, Rect roi, out List<Point> edgept1, Point TrackPos, bool directionL2R = true, int FindEdgeMethod = 0, bool UseStdFilter = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		edgept1 = new List<Point>();
		try
		{
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			int num = 0;
			Point[] array = new Point[2];
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				edgept1.Clear();
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
						if (ptr[num + j] == EdgeColor)
						{
							array[1].X = j + TrackPos.X;
							array[1].Y = i + TrackPos.Y;
							edgept1.Add(array[1]);
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
					for (int num2 = roi.Right - 1; num2 > roi.Left + 1; num2--)
					{
						if (ptr2[num + num2] == EdgeColor)
						{
							array[1].X = num2 + TrackPos.X;
							array[1].Y = k + TrackPos.Y;
							edgept1.Add(array[1]);
							break;
						}
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT012-AdvLocateTool-ScanVline()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-012");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanGetHLine(Mat Binary, Rect roi, out List<Point> edgept1, Point TrackPos, bool directionT2B = true, int FindEdgeMethod = 0, bool UseStdFilter = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		edgept1 = new List<Point>();
		try
		{
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			Point[] array = new Point[2];
			roi &= new Rect(0, 0, Binary.Cols, Binary.Rows);
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
						if (ptr[num] == EdgeColor)
						{
							array[1].X = i + TrackPos.X;
							array[1].Y = j + TrackPos.Y;
							edgept1.Add(array[1]);
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
					int num2 = width * (roi.Bottom - 1) + k;
					array[0].Y = 0;
					array[1].Y = 0;
					for (int num3 = roi.Bottom - 1; num3 > roi.Top + 1; num3--)
					{
						if (ptr2[num2] == EdgeColor)
						{
							array[1].X = k + TrackPos.X;
							array[1].Y = num3 + TrackPos.Y;
							edgept1.Add(array[1]);
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
			string text = "AT05-AdvLocateTool-ScanHLine()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "ADV-013");
			}
			return false;
		}
	}
}
