using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class LocateTool
{
	private CVBuffer buffers;

	private LocateAIEnhanceTool locateAIEnhanceTool;

	public bool[] QuadScanStatus = new bool[] { true, true, true, true };

	public bool[] QuadLocateStatus = new bool[] { true, true, true, true };

	private ParamFile pfile;

	private List<Point>[] QuadScanPt = new List<Point>[4];

	private Stopwatch sw = new Stopwatch();

	private Mat AILocatePredictMat = new Mat();

	private Mat SIMPMat = new Mat();

	private Mat SIMPBinary = new Mat();

	private Mat SIMPMat2 = new Mat();

	private Mat SIMPBinary2 = new Mat();

	private Mat SIMPMat3 = new Mat();

	private Mat SIMPBinary3 = new Mat();

	private Mat SIMPRotateImageROI = new Mat();

	private Mat QuadRotateImageROI = new Mat();

	private Mat PadRotateImageROI = new Mat();

	private Mat[] QuadBinary = new Mat[4];

	private Mat[] PadBinary = new Mat[4];

	private Mat LMat = new Mat();

	private Mat RMat = new Mat();

	private Mat TMat = new Mat();

	private Mat BMat = new Mat();

	private Mat LaplacianMat;

	private int DebugCode;

	private bool OffLine;

	private int MaxComDigitsItems = 24;

	public LineSegmentPoint[] Drawlines = new LineSegmentPoint[4];

	public bool OffLineTestEnhance;

	public double FocusScore;

	public bool FocusResult;

	private double EnhanceH;

	private double EnhanceS;

	private double EnhanceV;

	private double EnhanceR;

	private double EnhanceG;

	private double EnhanceB;

	private bool EnhanceHMirror;

	private bool EnhanceVMirror;

	private Mat[] SrcBGR = new Mat[3];

	private Mat[] SrcHSV = new Mat[3];

	public bool LocateSuccess { get; set; }

	public bool LocateSizeSuccess { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public Rect LocateExtendRect { get; set; }

	public double LocateWidth { get; set; }

	public double LocateHeight { get; set; }

	public double LocateAngle { get; set; }

	public int LocateCenterX { get; set; }

	public int LocateCenterY { get; set; }

	public string ErrorMsg { get; protected set; }

	public LocateTool(ParamFile paramfile)
	{
		MaxComDigitsItems = 24;
		pfile = paramfile;
		for (int i = 0; i < 4; i++)
		{
			QuadScanPt[i] = new List<Point>();
			QuadBinary[i] = new Mat();
			PadBinary[i] = new Mat();
		}
		LocateExtendRect = new Rect(0, 0, 1, 1);
		EnhanceH = (EnhanceS = (EnhanceV = (EnhanceR = (EnhanceG = (EnhanceB = 1.0)))));
		for (int j = 0; j < 3; j++)
		{
			SrcBGR[j] = new Mat();
			SrcHSV[j] = new Mat();
		}
		locateAIEnhanceTool = new LocateAIEnhanceTool(paramfile);
	}

	public List<Point> GetScanPt(int index)
	{
		return QuadScanPt[index];
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
		locateAIEnhanceTool.SetBuffers(bufs);
		locateAIEnhanceTool.CreatInst();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool DoLocate(bool offline = true)
	{
		if (buffers == null)
		{
			return false;
		}
		OffLine = offline;
		int locateCenterX = (LocateCenterY = 0);
		LocateCenterX = locateCenterX;
		LocateAngle = 0.0;
		if (pfile.LocateParam.EnableAILocate && pfile.AllowEnableAILocate && !UseAIPreLocate(buffers.Src, offline))
		{
			ErrorMsg = locateAIEnhanceTool.ResultText;
			return false;
		}
		switch (pfile.LocateParam.LocateType)
		{
		case 0:
			try
			{
				return SimpleLocate();
			}
			catch (Exception ex3)
			{
				string text4 = "LT01-LocateTool-SimpleLocate()";
				text4 += ex3.ToString();
				if (!OffLine)
				{
					CVBuffer.SaveLog(text4);
				}
				else
				{
					CVBuffer.SaveLog(text4, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-001");
				}
				return false;
			}
		case 1:
			try
			{
				return QuadLocate();
			}
			catch (Exception ex2)
			{
				string text2 = "LT02-LocateTool-QuadLocate()";
				text2 += ex2.ToString();
				string text3 = string.Format(", DebugCode = {0}", DebugCode);
				text2 += text3;
				if (!OffLine)
				{
					CVBuffer.SaveLog(text2);
				}
				else
				{
					CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-002");
				}
				return false;
			}
		case 2:
			try
			{
				return PadLocate();
			}
			catch (Exception ex)
			{
				string text = "LT03-LocateTool-PadLocate()";
				text += ex.ToString();
				if (!OffLine)
				{
					CVBuffer.SaveLog(text);
				}
				else
				{
					CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-003");
				}
				return false;
			}
		default:
			return SimpleLocate();
		}
	}

	public void ReCalcMaxSearchRect(int locatetype, Rect maxrect)
	{
		switch (locatetype)
		{
		case 0:
			pfile.LocateParam.MaxSearchRect = pfile.LocateParam.SIMPSearchRect;
			break;
		case 1:
			pfile.LocateParam.MaxSearchRect = new Rect(pfile.LocateParam.QuadSearchRectL.Left, pfile.LocateParam.QuadSearchRectT.Top, pfile.LocateParam.QuadSearchRectR.Right - pfile.LocateParam.QuadSearchRectL.Left, pfile.LocateParam.QuadSearchRectB.Bottom - pfile.LocateParam.QuadSearchRectT.Top);
			break;
		case 2:
			pfile.LocateParam.MaxSearchRect.Left = pfile.LocateParam.PadSearchRectL.Left;
			pfile.LocateParam.MaxSearchRect.Width = pfile.LocateParam.PadSearchRectR.Right - pfile.LocateParam.PadSearchRectL.Left;
			if (pfile.LocateParam.PadSearchRectL.Top < pfile.LocateParam.PadSearchRectR.Top)
			{
				pfile.LocateParam.MaxSearchRect.Top = pfile.LocateParam.PadSearchRectL.Top;
			}
			else
			{
				pfile.LocateParam.MaxSearchRect.Top = pfile.LocateParam.PadSearchRectR.Top;
			}
			if (pfile.LocateParam.PadSearchRectL.Bottom > pfile.LocateParam.PadSearchRectR.Bottom)
			{
				pfile.LocateParam.MaxSearchRect.Height = pfile.LocateParam.PadSearchRectL.Bottom - pfile.LocateParam.MaxSearchRect.Top;
			}
			else
			{
				pfile.LocateParam.MaxSearchRect.Height = pfile.LocateParam.PadSearchRectR.Bottom - pfile.LocateParam.MaxSearchRect.Top;
			}
			break;
		}
		pfile.LocateParam.MaxSearchRect &= maxrect;
	}

	public bool UseAIPreLocate(Mat mat, bool offline)
	{
		ReCalcMaxSearchRect(maxrect: new Rect(0, 0, mat.Width, mat.Height), locatetype: pfile.LocateParam.LocateType);
		Mat mat2 = new Mat(mat, pfile.LocateParam.MaxSearchRect);
		mat2.CopyTo(AILocatePredictMat);
		return locateAIEnhanceTool.Inspect(mat2, offline);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool SimpleLocate()
	{
		ErrorMsg = "";
		LocateSuccess = false;
		LocateSizeSuccess = false;
		LocateRect = new Rect(0, 0, 0, 0);
		LocateImageRect = new Rect(0, 0, 0, 0);
		double locateWidth = (LocateHeight = 0.0);
		LocateWidth = locateWidth;
		Rect rect = new Rect(0, 0, buffers.Src.Width, buffers.Src.Height);
		if (!pfile.LocateParam.EnableAILocate)
		{
			ReCalcMaxSearchRect(pfile.LocateParam.LocateType, rect);
		}
		Rect maxSearchRect = pfile.LocateParam.MaxSearchRect;
		if (maxSearchRect.Width <= 0 || maxSearchRect.Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[簡單定位失敗] 定位檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[简单定位失败] 定位检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "【簡単位置測定失敗】位置検出ボックスのパラメータエラー、幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Easy Locate] Tracker error! Tracker width and height = 0!";
			}
			return false;
		}
		new Mat(buffers.GetSrcBGR(pfile.LocateParam.SIMP_THD1_Channel), maxSearchRect).CopyTo(SIMPMat);
		pfile.LocateParam.MaxSearchRect = maxSearchRect;
		Cv2.InRange(SIMPMat, pfile.LocateParam.SIMP_THD1_Min, pfile.LocateParam.SIMP_THD1_Max, SIMPBinary);
		DoMorph(ref SIMPBinary, (int)pfile.LocateParam.SIMP_DilateErode1Start, pfile.LocateParam.SIMP_DilateErode1StartCount);
		DoMorph(ref SIMPBinary, (int)pfile.LocateParam.SIMP_DilateErode1End, pfile.LocateParam.SIMP_DilateErode1EndCount);
		if (pfile.LocateParam.SIMP_THD2_Enabled)
		{
			new Mat(buffers.GetSrcBGR(pfile.LocateParam.SIMP_THD2_Channel), maxSearchRect).CopyTo(SIMPMat2);
			Cv2.InRange(SIMPMat2, pfile.LocateParam.SIMP_THD2_Min, pfile.LocateParam.SIMP_THD2_Max, SIMPBinary2);
			DoMorph(ref SIMPBinary2, (int)pfile.LocateParam.SIMP_DilateErode2Start, pfile.LocateParam.SIMP_DilateErode2StartCount);
			DoMorph(ref SIMPBinary2, (int)pfile.LocateParam.SIMP_DilateErode2End, pfile.LocateParam.SIMP_DilateErode2EndCount);
			Cv2.BitwiseAnd(SIMPBinary, SIMPBinary2, SIMPBinary);
		}
		if (pfile.LocateParam.SIMP_THD3_Enabled)
		{
			new Mat(buffers.GetSrcBGR(pfile.LocateParam.SIMP_THD3_Channel), maxSearchRect).CopyTo(SIMPMat3);
			Cv2.InRange(SIMPMat3, pfile.LocateParam.SIMP_THD3_Min, pfile.LocateParam.SIMP_THD3_Max, SIMPBinary3);
			DoMorph(ref SIMPBinary3, (int)pfile.LocateParam.SIMP_DilateErode3Start, pfile.LocateParam.SIMP_DilateErode3StartCount);
			DoMorph(ref SIMPBinary3, (int)pfile.LocateParam.SIMP_DilateErode3End, pfile.LocateParam.SIMP_DilateErode3EndCount);
			Cv2.BitwiseAnd(SIMPBinary, SIMPBinary3, SIMPBinary);
		}
		_ = new Point2f[4];
		Cv2.FindContours(SIMPBinary, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
		if (contours.Length == 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[簡單定位失敗] 找不到任何區塊! 請試著調整二值化參數!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[简单定位失败] 找不到任何区块! 请试着调整二值化参数!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "[簡単位置測定失敗]。ブロックが見つかりません。二値化パラメータを調整してみてください。";
			}
			else
			{
				ErrorMsg = "[Easy Locate] Found area is 0, Try to Finetune binary parameter!";
			}
			return false;
		}
		double num2 = 0.0;
		int num3 = 0;
		RotatedRect rotatedRect = default(RotatedRect);
		for (int i = 0; i < contours.Length; i++)
		{
			double num4 = Cv2.ContourArea(contours[i]);
			if (num4 > num2)
			{
				num2 = num4;
				num3 = i;
			}
		}
		rotatedRect = Cv2.MinAreaRect(contours[num3]);
		Point2f[] array = new Point2f[4];
		array = rotatedRect.Points();
		new Mat(buffers.Src, maxSearchRect).CopyTo(SIMPRotateImageROI);
		Point2f point2f = default(Point);
		point2f.X = maxSearchRect.Width / 2;
		point2f.Y = maxSearchRect.Height / 2;
		double num5 = rotatedRect.Angle;
		if (num5 > 60.0)
		{
			num5 -= 90.0;
		}
		else if (num5 < -60.0)
		{
			num5 += 90.0;
		}
		LocateAngle = num5;
		Mat rotationMatrix2D = Cv2.GetRotationMatrix2D(point2f, num5, 1.0);
		Size dsize = default(Size);
		dsize.Width = SIMPRotateImageROI.Cols;
		dsize.Height = SIMPRotateImageROI.Rows;
		double num6 = Math.Abs(num5);
		double num7 = 0.1;
		if (pfile.LocateParam.LocateSpeedUp)
		{
			num7 = pfile.LocateParam.doRotateMinAngle;
		}
		Rect roi = maxSearchRect;
		if (num6 <= num7)
		{
			SIMPRotateImageROI.CopyTo(buffers.LocatedSrc);
			if (pfile.LocateParam.EnableAILocate)
			{
				roi &= rect;
				Mat mat = new Mat(buffers.GetChannelMat(18), roi);
				Mat m = new Mat(buffers.GetAILocateMat(21), roi);
				mat.CopyTo(m);
				Mat mat2 = new Mat(buffers.GetChannelMat(19), roi);
				m = new Mat(buffers.GetAILocateMat(22), roi);
				mat2.CopyTo(m);
				Mat mat3 = new Mat(buffers.GetChannelMat(20), roi);
				m = new Mat(buffers.GetAILocateMat(23), roi);
				mat3.CopyTo(m);
			}
		}
		else
		{
			Cv2.WarpAffine(SIMPRotateImageROI, buffers.LocatedSrc, rotationMatrix2D, dsize, InterpolationFlags.Linear, BorderTypes.Replicate, Scalar.White);
			if (pfile.LocateParam.EnableAILocate)
			{
				Mat mat4 = new Mat(buffers.GetChannelMat(18), roi);
				Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(21), roi), src: mat4, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
				Mat mat5 = new Mat(buffers.GetChannelMat(19), roi);
				Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(22), roi), src: mat5, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
				Mat mat6 = new Mat(buffers.GetChannelMat(20), roi);
				Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(23), roi), src: mat6, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
			}
		}
		Point dstpt = default(Point);
		dstpt.X = (int)array[1].X;
		dstpt.Y = (int)array[1].Y;
		Point dstpt2 = default(Point);
		dstpt2.X = (int)array[3].X;
		dstpt2.Y = (int)array[3].Y;
		if (pfile.LocateParam.SIMP_SideLocate)
		{
			dstpt2.Y = dstpt.Y + (dstpt2.Y - dstpt.Y) / 2;
		}
		GetPointAffinedPos(dstpt, (Point)point2f, num5, ref dstpt);
		GetPointAffinedPos(dstpt2, (Point)point2f, num5, ref dstpt2);
		LocateRect = new Rect
		{
			X = Math.Min(dstpt.X, dstpt2.X),
			Y = Math.Min(dstpt.Y, dstpt2.Y),
			Width = Math.Abs(dstpt2.X - dstpt.X),
			Height = Math.Abs(dstpt2.Y - dstpt.Y)
		};
		LocateRect &= rect;
		LocateCenterX = LocateRect.X;
		LocateCenterY = LocateRect.Y + LocateRect.Height / 2;
		if (OffLine && OffLineTestEnhance)
		{
			ImageEnhance(LocateRect);
		}
		if (OffLine)
		{
			using Mat m2 = new Mat(buffers.Dst, maxSearchRect);
			buffers.LocatedSrc.CopyTo(m2);
		}
		LocateImageRect = maxSearchRect;
		buffers.LocateImageRect = LocateImageRect;
		buffers.LocatedRect = LocateRect;
		buffers.LocatedRectTmp = LocateRect;
		if (pfile.SelectWayON)
		{
			buffers.LocatedSrc.CopyTo(buffers.LocatedSrcTmp);
		}
		else
		{
			buffers.LocatedSrcTmp = buffers.LocatedSrc;
		}
		int[] array2 = new int[6];
		for (int j = 0; j < 6; j++)
		{
			array2[j] = pfile.EnhanceParam[j].FunctionIndex;
		}
		buffers.SetLocatedBuffer(buffers.LocatedSrc, array2, pfile.AIEnhanceToolON, pfile.LocateParam.CreateHSVChannel, OffLine);
		if (pfile.LocateParam.EnableFocusCheck)
		{
			FocusResult = CheckFocus();
			if (!FocusResult)
			{
				FocusResult = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[定位失敗] 檢測到失焦! 分數={0:N1}, 設定={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[定位失败] 检测到失焦! 分数={0:N1}, 设定={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[検出失敗] ピントが外れています!スコア={0:N1}, 設定={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
				}
				else
				{
					ErrorMsg = string.Format("[Locate Fail] Out of focus! Score={0:N1}, Threshold={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
				}
				return false;
			}
		}
		LocateAngle = num5;
		double num8 = (double)LocateRect.Width * pfile.PixelPerUM_X;
		double num9 = (double)LocateRect.Height * pfile.PixelPerUM_Y;
		LocateWidth = num8;
		LocateHeight = num9;
		if (Math.Abs(num5) > pfile.LocateParam.LocateAngleLimit)
		{
			if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[簡單定位角度NG]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[简单定位角度NG]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[簡単位置測定NG]\n 幅:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
				else
				{
					ErrorMsg = string.Format("[Easy Locate Angle Fail]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
			}
			else if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[簡單定位角度NG]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[简单定位角度NG]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[簡単位置測定NG]\n 幅:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else
			{
				ErrorMsg = string.Format("[Easy Locate Angle Fail]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			LocateSuccess = false;
			return false;
		}
		LocateSuccess = true;
		double LocateWidthMax = 0.0;
		double LocateWidthMin = 0.0;
		double LocateHeightMin = 0.0;
		double LocateHeightMax = 0.0;
		GetLocateSizeLimit(ref LocateWidthMin, ref LocateWidthMax, ref LocateHeightMin, ref LocateHeightMax);
		if (num8 > LocateWidthMax || num8 < LocateWidthMin || num9 < LocateHeightMin || num9 > LocateHeightMax)
		{
			if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[簡單定位尺寸NG]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[简单定位尺寸NG]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[簡単な位置決め寸法NG]\n 幅:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
				else
				{
					ErrorMsg = string.Format("[Easy Locate Size Fail]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num8, num9, Math.Abs(num5));
				}
			}
			else if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[簡單定位尺寸NG]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[简单定位尺寸NG]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[簡単な位置決め寸法NG]\n 幅:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else
			{
				ErrorMsg = string.Format("[Easy Locate Size Fail]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			LocateSizeSuccess = false;
			return false;
		}
		if (pfile.MeasureUnit == 0)
		{
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[簡單定位成功]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[简单定位成功]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[簡単な位置決に成功]\n 幅:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
			else
			{
				ErrorMsg = string.Format("[Easy Locate Success]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num8, num9, Math.Abs(num5));
			}
		}
		else if (buffers.LanguageType == 1)
		{
			ErrorMsg = string.Format("[簡單定位成功]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
		}
		else if (buffers.LanguageType == 2)
		{
			ErrorMsg = string.Format("[简单定位成功]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
		}
		else if (buffers.LanguageType == 3)
		{
			ErrorMsg = string.Format("[簡単な位置決に成功]\n 幅:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num8, num9, Math.Abs(num5));
		}
		else
		{
			ErrorMsg = string.Format("[Easy Locate Success]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num8, num9, Math.Abs(num5));
		}
		LocateSuccess = true;
		LocateSizeSuccess = true;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool QuadLocate()
	{
		LineSegmentPoint[] array = new LineSegmentPoint[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = default(LineSegmentPoint);
			array[i].P1.X = (array[i].P1.Y = (array[i].P2.X = (array[i].P2.Y = 0)));
			QuadLocateStatus[i] = true;
		}
		DebugCode = 0;
		ErrorMsg = "";
		LocateSuccess = false;
		LocateSizeSuccess = false;
		LocateRect = new Rect(0, 0, 0, 0);
		LocateImageRect = new Rect(0, 0, 0, 0);
		LocateWidth = 0.0;
		LocateHeight = 0.0;
		Rect[] array2 = new Rect[4];
		Rect rect = new Rect(0, 0, buffers.Src.Width, buffers.Src.Height);
		array2[0] = pfile.LocateParam.QuadSearchRectL & rect;
		array2[1] = pfile.LocateParam.QuadSearchRectR & rect;
		array2[2] = pfile.LocateParam.QuadSearchRectT & rect;
		array2[3] = pfile.LocateParam.QuadSearchRectB & rect;
		Rect[] array3 = new Rect[4]
		{
			pfile.LocateParam.Quad_Mask_L & array2[0],
			pfile.LocateParam.Quad_Mask_R & array2[1],
			pfile.LocateParam.Quad_Mask_T & array2[2],
			pfile.LocateParam.Quad_Mask_B & array2[3]
		};
		if (array2[0].Width <= 0 || array2[0].Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位左邊檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位左边检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "四辺形位置決め失敗] 四辺形位置決め左側の検出ボックスパラメータエラー、幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Left Tracker width or height = 0!";
			}
			return false;
		}
		if (array2[1].Width <= 0 || array2[1].Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位右邊檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位右边检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "四辺形位置決め失敗] 四辺形位置決め右側の検出ボックスパラメータエラー、幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Right Tracker width or height = 0!";
			}
			return false;
		}
		if (array2[2].Width <= 0 || array2[2].Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位上邊檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位上边检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "四辺形位置決め失敗] 四辺形位置決め上側の検出ボックスパラメータエラー、幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Top Tracker width or height = 0!";
			}
			return false;
		}
		if (array2[3].Width <= 0 || array2[3].Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位下邊檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位下边检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "四辺形位置決め失敗] 四辺形位置決め下側の検出ボックスパラメータエラー、幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Bottom Tracker width or height = 0!";
			}
			return false;
		}
		if (!pfile.LocateParam.EnableAILocate)
		{
			ReCalcMaxSearchRect(pfile.LocateParam.LocateType, rect);
		}
		Rect maxSearchRect = pfile.LocateParam.MaxSearchRect;
		if (maxSearchRect.Width <= 0 || maxSearchRect.Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "四辺形位置決め失敗] 四辺形位置決め検出ボックスパラメータエラー、幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Tracker width or height = 0!";
			}
			return false;
		}
		DebugCode = 1;
		if (!pfile.LocateParam.Quad_EnableRGBThd_L)
		{
			LMat = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Quad_Channel_L), array2[0]);
			LMat.CopyTo(QuadBinary[0]);
			if (pfile.LocateParam.Quad_DEStart_L != USE_DILATE_ERODE.EM_USE_CANNY)
			{
				Cv2.InRange(QuadBinary[0], pfile.LocateParam.Quad_THD_Min_L, pfile.LocateParam.Quad_THD_Max_L, QuadBinary[0]);
			}
		}
		else
		{
			LMat = new Mat(buffers.Src, array2[0]);
			Cv2.InRange(LMat, new Scalar(pfile.LocateParam.Quad_THDB_Min_L, pfile.LocateParam.Quad_THDG_Min_L, pfile.LocateParam.Quad_THDR_Min_L), new Scalar(pfile.LocateParam.Quad_THDB_Max_L, pfile.LocateParam.Quad_THDG_Max_L, pfile.LocateParam.Quad_THDR_Max_L), QuadBinary[0]);
		}
		DoMorph(ref QuadBinary[0], (int)pfile.LocateParam.Quad_DEStart_L, pfile.LocateParam.Quad_DEStartCount_L);
		DoMorph(ref QuadBinary[0], (int)pfile.LocateParam.Quad_DEEnd_L, pfile.LocateParam.Quad_DEEndCount_L);
		if (pfile.LocateParam.Quad_EnableMask_L)
		{
			array3[0].Left -= array2[0].Left;
			array3[0].Top -= array2[0].Top;
			if (array3[0].Width > 0 && array3[0].Height > 0)
			{
				using Mat mat = new Mat(QuadBinary[0], array3[0]);
				mat.SetTo(0.0);
			}
		}
		if (!pfile.LocateParam.Quad_EnableRGBThd_R)
		{
			RMat = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Quad_Channel_R), array2[1]);
			RMat.CopyTo(QuadBinary[1]);
			if (pfile.LocateParam.Quad_DEStart_R != USE_DILATE_ERODE.EM_USE_CANNY)
			{
				Cv2.InRange(QuadBinary[1], pfile.LocateParam.Quad_THD_Min_R, pfile.LocateParam.Quad_THD_Max_R, QuadBinary[1]);
			}
		}
		else
		{
			RMat = new Mat(buffers.Src, array2[1]);
			Cv2.InRange(RMat, new Scalar(pfile.LocateParam.Quad_THDB_Min_R, pfile.LocateParam.Quad_THDG_Min_R, pfile.LocateParam.Quad_THDR_Min_R), new Scalar(pfile.LocateParam.Quad_THDB_Max_R, pfile.LocateParam.Quad_THDG_Max_R, pfile.LocateParam.Quad_THDR_Max_R), QuadBinary[1]);
		}
		DoMorph(ref QuadBinary[1], (int)pfile.LocateParam.Quad_DEStart_R, pfile.LocateParam.Quad_DEStartCount_R);
		DoMorph(ref QuadBinary[1], (int)pfile.LocateParam.Quad_DEEnd_R, pfile.LocateParam.Quad_DEEndCount_R);
		if (pfile.LocateParam.Quad_EnableMask_R)
		{
			array3[1].Left -= array2[1].Left;
			array3[1].Top -= array2[1].Top;
			if (array3[1].Width > 0 && array3[1].Height > 0)
			{
				using Mat mat2 = new Mat(QuadBinary[1], array3[1]);
				mat2.SetTo(0.0);
			}
		}
		if (!pfile.LocateParam.Quad_EnableRGBThd_T)
		{
			TMat = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Quad_Channel_T), array2[2]);
			TMat.CopyTo(QuadBinary[2]);
			if (pfile.LocateParam.Quad_DEStart_T != USE_DILATE_ERODE.EM_USE_CANNY)
			{
				Cv2.InRange(QuadBinary[2], pfile.LocateParam.Quad_THD_Min_T, pfile.LocateParam.Quad_THD_Max_T, QuadBinary[2]);
			}
		}
		else
		{
			TMat = new Mat(buffers.Src, array2[2]);
			Cv2.InRange(TMat, new Scalar(pfile.LocateParam.Quad_THDB_Min_T, pfile.LocateParam.Quad_THDG_Min_T, pfile.LocateParam.Quad_THDR_Min_T), new Scalar(pfile.LocateParam.Quad_THDB_Max_T, pfile.LocateParam.Quad_THDG_Max_T, pfile.LocateParam.Quad_THDR_Max_T), QuadBinary[2]);
		}
		DoMorph(ref QuadBinary[2], (int)pfile.LocateParam.Quad_DEStart_T, pfile.LocateParam.Quad_DEStartCount_T);
		DoMorph(ref QuadBinary[2], (int)pfile.LocateParam.Quad_DEEnd_T, pfile.LocateParam.Quad_DEEndCount_T);
		if (pfile.LocateParam.Quad_EnableMask_T)
		{
			array3[2].Left -= array2[2].Left;
			array3[2].Top -= array2[2].Top;
			if (array3[2].Width > 0 && array3[2].Height > 0)
			{
				using Mat mat3 = new Mat(QuadBinary[2], array3[2]);
				mat3.SetTo(0.0);
			}
		}
		if (!pfile.LocateParam.Quad_EnableRGBThd_B)
		{
			BMat = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Quad_Channel_B), array2[3]);
			BMat.CopyTo(QuadBinary[3]);
			if (pfile.LocateParam.Quad_DEStart_B != USE_DILATE_ERODE.EM_USE_CANNY)
			{
				Cv2.InRange(QuadBinary[3], pfile.LocateParam.Quad_THD_Min_B, pfile.LocateParam.Quad_THD_Max_B, QuadBinary[3]);
			}
		}
		else
		{
			BMat = new Mat(buffers.Src, array2[3]);
			Cv2.InRange(BMat, new Scalar(pfile.LocateParam.Quad_THDB_Min_B, pfile.LocateParam.Quad_THDG_Min_B, pfile.LocateParam.Quad_THDR_Min_B), new Scalar(pfile.LocateParam.Quad_THDB_Max_B, pfile.LocateParam.Quad_THDG_Max_B, pfile.LocateParam.Quad_THDR_Max_B), QuadBinary[3]);
		}
		DoMorph(ref QuadBinary[3], (int)pfile.LocateParam.Quad_DEStart_B, pfile.LocateParam.Quad_DEStartCount_B);
		DoMorph(ref QuadBinary[3], (int)pfile.LocateParam.Quad_DEEnd_B, pfile.LocateParam.Quad_DEEndCount_B);
		if (pfile.LocateParam.Quad_EnableMask_B)
		{
			array3[3].Left -= array2[3].Left;
			array3[3].Top -= array2[3].Top;
			if (array3[3].Width > 0 && array3[3].Height > 0)
			{
				using Mat mat4 = new Mat(QuadBinary[3], array3[3]);
				mat4.SetTo(0.0);
			}
		}
		DebugCode = 2;
		bool flag = false;
		if (!ScanVLine(directionL2R: (int)pfile.LocateParam.Direction_L % 2 == 0, BinaryROI: QuadBinary[0], roi: array2[0], edgept1: out QuadScanPt[0]))
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位左邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位左边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "[四角形位置測定失敗] 四角形位置特定左辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Left side scan error,  Try to Finetune binary parameter!";
			}
			QuadLocateStatus[0] = false;
			return false;
		}
		if (QuadScanPt[0].Count > 12)
		{
			int num = QuadScanPt[0].Count / 4;
			List<Point> list = new List<Point>();
			for (int j = num; j < QuadScanPt[0].Count - num; j++)
			{
				list.Add(QuadScanPt[0][j]);
			}
			if (list.Count > 4)
			{
				QuadScanPt[0].Clear();
				for (int k = 0; k < list.Count; k++)
				{
					QuadScanPt[0].Add(list[k]);
				}
			}
		}
		DebugCode = 3;
		flag = (int)pfile.LocateParam.Direction_R % 2 == 0;
		if (!pfile.LocateParam.UseMelfMode)
		{
			if (!ScanVLine(QuadBinary[1], array2[1], out QuadScanPt[1], flag))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位右邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位右边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] 四角形位置特定右辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Right side scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[1] = false;
				return false;
			}
			if (QuadScanPt[1].Count > 12)
			{
				int num2 = QuadScanPt[1].Count / 4;
				List<Point> list2 = new List<Point>();
				for (int l = num2; l < QuadScanPt[1].Count - num2; l++)
				{
					list2.Add(QuadScanPt[1][l]);
				}
				if (list2.Count > 4)
				{
					QuadScanPt[1].Clear();
					for (int m = 0; m < list2.Count; m++)
					{
						QuadScanPt[1].Add(list2[m]);
					}
				}
			}
		}
		else
		{
			List<Point> edgept = new List<Point>();
			List<Point> edgept2 = new List<Point>();
			if (!ScanHLine(QuadBinary[1], array2[1], out edgept))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] Melf右邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] Melf右边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] Melf位置特定右辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Melf Right side scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[1] = false;
				return false;
			}
			if (!ScanHLine(QuadBinary[1], array2[1], out edgept2, directionT2B: false))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] Melf右邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] Melf右边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] Melf位置特定右辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Melf Right side scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[1] = false;
				return false;
			}
			QuadScanPt[1] = new List<Point>();
			int num3 = 0;
			int num4 = 10000;
			Point item = new Point(0, 0);
			Point item2 = new Point(0, 0);
			QuadScanPt[1].Clear();
			for (int n = 0; n < edgept.Count; n++)
			{
				if (edgept[n].Y > num3 && edgept[n].Y < array2[1].Top + array2[1].Height / 2)
				{
					num3 = edgept[n].Y;
					item = edgept[n];
				}
			}
			QuadScanPt[1].Add(item);
			for (int num5 = 0; num5 < edgept2.Count; num5++)
			{
				if (edgept2[num5].Y < num4 && edgept2[num5].Y > array2[1].Top + array2[1].Height / 2)
				{
					num4 = edgept2[num5].Y;
					item2 = edgept2[num5];
				}
			}
			QuadScanPt[1].Add(item2);
			if (QuadScanPt[1].Count < 2)
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] Melf右邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] Melf右边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] Melf位置特定右辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Melf Right side scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[1] = false;
				return false;
			}
		}
		DebugCode = 4;
		if (!ScanHLine(directionT2B: (int)pfile.LocateParam.Direction_T % 2 == 0, BinaryROI: QuadBinary[2], roi: array2[2], edgept1: out QuadScanPt[2]))
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[四邊形定位失敗] 四邊形定位上邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[四边形定位失败] 四边形定位上边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "[四角形位置測定失敗] 四角形位置特定上辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
			}
			else
			{
				ErrorMsg = "[Quad Locate Fail] Top side scan error,  Try to Finetune binary parameter!";
			}
			QuadLocateStatus[2] = false;
			return false;
		}
		DebugCode = 5;
		flag = (int)pfile.LocateParam.Direction_B % 2 == 0;
		if (!pfile.LocateParam.Quad_UseSideLocate)
		{
			if (!ScanHLine(QuadBinary[3], array2[3], out QuadScanPt[3], flag))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位下邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位下边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] 四角形位置特定下辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Bottom side scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[3] = false;
				return false;
			}
		}
		else
		{
			List<Point> edgept3 = new List<Point>();
			List<Point> edgept4 = new List<Point>();
			if (!ScanVLine(QuadBinary[3], array2[3], out edgept3))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位下邊左掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位下边左扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] 四角形位置特定下辺左スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Bottom side Left scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[3] = false;
				return false;
			}
			if (!ScanVLine(QuadBinary[3], array2[3], out edgept4, directionL2R: false))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位下邊右掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位下边右扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] 四角形位置特定下辺右スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Bottom side Left scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[3] = false;
				return false;
			}
			int num6 = 0;
			int num7 = 10000;
			Point item3 = new Point(0, 0);
			Point item4 = new Point(0, 0);
			QuadScanPt[3].Clear();
			for (int num8 = 0; num8 < edgept3.Count; num8++)
			{
				if (edgept3[num8].X > num6 && edgept3[num8].X < array2[3].Left + array2[3].Width / 2)
				{
					num6 = edgept3[num8].X;
					item3 = edgept3[num8];
				}
			}
			QuadScanPt[3].Add(item3);
			for (int num9 = 0; num9 < edgept4.Count; num9++)
			{
				if (edgept4[num9].X < num7 && edgept4[num9].X > array2[3].Left + array2[3].Width / 2)
				{
					num7 = edgept4[num9].X;
					item4 = edgept4[num9];
				}
			}
			QuadScanPt[3].Add(item4);
			if (QuadScanPt[3].Count < 2)
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位下邊掃描失敗，掃描點小於2!，請檢查二值化數值再重新測試。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位下边扫描失败，扫描点小于2!，请检查二值化数值再重新测试。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "[四角形位置測定失敗] 四角形位置特定下辺スキャン失敗、スキャンポイントが2未満！、二値化数値を確認して再テストしてください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Bottom side scan error,  Try to Finetune binary parameter!";
				}
				QuadLocateStatus[3] = false;
				return false;
			}
		}
		DebugCode = 6;
		double[] array4 = new double[4];
		if (pfile.LocateParam.Quad_FixSideLocateLR)
		{
			if (QuadScanPt[0].Count > 1)
			{
				array4[0] = 0.0;
				double num10 = 0.0;
				for (int num11 = 0; num11 < QuadScanPt[0].Count; num11++)
				{
					num10 += (double)QuadScanPt[0][num11].X;
				}
				int x = (int)num10 / QuadScanPt[0].Count;
				Point point = new Point(x, QuadScanPt[0][0].Y);
				Point point2 = new Point(x, QuadScanPt[0][1].Y);
				array[0] = new LineSegmentPoint(point, point2);
				if (point == point2)
				{
					QuadScanStatus[0] = false;
				}
				else
				{
					QuadScanStatus[0] = true;
				}
			}
			else
			{
				QuadScanStatus[0] = false;
			}
			if (QuadScanPt[1].Count > 1)
			{
				array4[1] = 0.0;
				double num12 = 0.0;
				for (int num13 = 0; num13 < QuadScanPt[1].Count; num13++)
				{
					num12 += (double)QuadScanPt[1][num13].X;
				}
				int x2 = (int)num12 / QuadScanPt[1].Count;
				Point point3 = new Point(x2, QuadScanPt[1][0].Y);
				Point point4 = new Point(x2, QuadScanPt[1][1].Y);
				array[1] = new LineSegmentPoint(point3, point4);
				if (point3 == point4)
				{
					QuadScanStatus[1] = false;
				}
				else
				{
					QuadScanStatus[1] = true;
				}
			}
			else
			{
				QuadScanStatus[1] = false;
			}
		}
		if (pfile.LocateParam.Quad_UseSideLocate)
		{
			if (!pfile.LocateParam.Quad_FixSideLocateLR)
			{
				for (int num14 = 0; num14 < 2; num14++)
				{
					if (QuadScanPt[num14].Count > 1)
					{
						Line2D line2D = Cv2.FitLine(QuadScanPt[num14], DistanceTypes.L2, 0.0, 0.01, 0.01);
						array4[num14] = line2D.GetVectorAngle();
						Point pt = new Point(0, 0);
						Point pt2 = new Point(0, 0);
						line2D.FitSize(array2[num14].Width, array2[num14].Height, out pt, out pt2);
						array[num14] = new LineSegmentPoint(pt, pt2);
						if (pt == pt2)
						{
							QuadScanStatus[num14] = false;
						}
						else
						{
							QuadScanStatus[num14] = true;
						}
					}
					else
					{
						QuadScanStatus[num14] = false;
					}
				}
			}
			if (QuadScanPt[2].Count > 1)
			{
				Line2D line2D2 = Cv2.FitLine(QuadScanPt[2], DistanceTypes.L2, 0.0, 0.01, 0.01);
				array4[2] = line2D2.GetVectorAngle();
				Point pt3 = new Point(0, 0);
				Point pt4 = new Point(0, 0);
				line2D2.FitSize(array2[2].Width, array2[2].Height, out pt3, out pt4);
				array[2] = new LineSegmentPoint(pt3, pt4);
				if (array2[2].Width <= 0 || array2[2].Height <= 0 || pt3 == pt4)
				{
					QuadScanStatus[2] = false;
				}
				else
				{
					QuadScanStatus[2] = true;
				}
			}
			else
			{
				QuadScanStatus[2] = false;
			}
			if (QuadScanPt[3].Count > 1)
			{
				Line2D line2D3 = Cv2.FitLine(QuadScanPt[3], DistanceTypes.L2, 0.0, 0.01, 0.01);
				array4[3] = line2D3.GetVectorAngle();
				Point pt5 = new Point(0, 0);
				Point pt6 = new Point(0, 0);
				line2D3.FitSize(array2[3].Width, array2[3].Height, out pt5, out pt6);
				array[3] = new LineSegmentPoint(pt5, pt6);
				if (array2[3].Width <= 0 || array2[3].Height <= 0 || pt5 == pt6)
				{
					QuadScanStatus[3] = false;
				}
				else
				{
					QuadScanStatus[3] = true;
				}
			}
			else
			{
				QuadScanStatus[3] = false;
			}
		}
		else
		{
			for (int num15 = 0; num15 < 4; num15++)
			{
				if (QuadScanPt[num15].Count > 1)
				{
					Line2D line2D4 = Cv2.FitLine(QuadScanPt[num15], DistanceTypes.L2, 0.0, 0.01, 0.01);
					array4[num15] = line2D4.GetVectorAngle();
					Point pt7 = new Point(0, 0);
					Point pt8 = new Point(0, 0);
					line2D4.FitSize(array2[num15].Width, array2[num15].Height, out pt7, out pt8);
					array[num15] = new LineSegmentPoint(pt7, pt8);
					Drawlines[num15] = new LineSegmentPoint(pt7, pt8);
					if (pt7 == pt8)
					{
						QuadScanStatus[num15] = false;
					}
					else
					{
						QuadScanStatus[num15] = true;
					}
				}
				else
				{
					QuadScanStatus[num15] = false;
				}
			}
		}
		DebugCode = 7;
		if (QuadScanStatus[0] && QuadScanStatus[1] && QuadScanStatus[2] && QuadScanStatus[3])
		{
			DebugCode = 71;
			List<Point> list3 = new List<Point>();
			if (!GetIntersection(array[0], array[2], out var Corner))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位左上角錯誤!，請檢查左邊與上邊掃描點。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位左上角错误!，请检查左边与上边扫描点。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗：左上隅の特定が誤っています。左辺と上辺の走査点を点検してください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Left Top Corner Error,  Try to Left Side and Top Side parameter!";
				}
				return false;
			}
			if (!GetIntersection(array[1], array[2], out var Corner2))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位右上角錯誤!，請檢查右邊與上邊掃描點。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位右上角错误!，请检查右边与上边扫描点。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗：右上隅の特定が誤っています。右辺と上辺の走査点を点検してください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Right Top Corner Error,  Try to Right Side and Top Side parameter!";
				}
				return false;
			}
			if (!GetIntersection(array[0], array[3], out var Corner3))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位左下角錯誤!，請檢查左邊與下邊掃描點。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位左下角错误!，请检查左边与下边扫描点。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗：左下隅の特定が誤っています。左辺と下辺の走査点を点検してください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Left Bottom Corner Error,  Try to Left Side and Bottom Side parameter!";
				}
				return false;
			}
			if (!GetIntersection(array[1], array[3], out var Corner4))
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位右下角錯誤!，請檢查右邊與下邊掃描點。";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位右下角错误!，请检查右边与下边扫描点。";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗：右下隅の特定が誤っています。右辺と下辺の走査点を点検してください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Left Bottom Corner Error,  Try to Left Side and Bottom Side parameter!";
				}
				return false;
			}
			list3.Add(Corner);
			list3.Add(Corner2);
			list3.Add(Corner3);
			list3.Add(Corner4);
			DebugCode = 77;
			RotatedRect rotatedRect = Cv2.MinAreaRect(list3);
			DebugCode = 78;
			Point2f center = rotatedRect.Center;
			DebugCode = 79;
			_ = new Point2f[4];
			rotatedRect.Points();
			DebugCode = 8;
			Rect rect2 = maxSearchRect;
			rect2 &= new Rect(0, 0, buffers.Src.Cols, buffers.Src.Rows);
			if (center.X < (float)rect2.Left || center.X > (float)rect2.Right || center.Y < (float)rect2.Top || center.Y > (float)rect2.Bottom)
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位旋轉中心點錯誤，請檢查定位邊緣二值化數值!";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位旋转中心点错误，请检查定位边缘二值化数值!";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗。四角形定位の回転中心点が誤っています。定位エッジの二値化数値を確認してください。";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Locate Center axis error!";
				}
				return false;
			}
			if (rect2.Width <= 0 || rect2.Height <= 0)
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位檢測框(MaxRect)參數錯誤，寬度高度為0!";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位检测框(MaxRect)参数错误，宽度高度为0!";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗：四角形位置検出ボックス(MaxRect)のパラメータエラー、幅と高さが0です！";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Locate MaxRect width or height = 0!";
				}
				return false;
			}
			new Mat(buffers.Src, rect2).CopyTo(QuadRotateImageROI);
			Point2f center2 = center;
			double num16 = 0.0;
			if (!pfile.LocateParam.Quad_UseSideLocate)
			{
				switch (pfile.LocateParam.Quad_LocateAngleSelect)
				{
				case 0:
					num16 = rotatedRect.Angle;
					break;
				case 1:
					num16 = array4[0];
					break;
				case 2:
					num16 = array4[1];
					break;
				case 3:
					num16 = array4[2];
					break;
				case 4:
					num16 = array4[3];
					break;
				case 5:
					num16 = GetAvgAngle3(array4);
					break;
				case 6:
					num16 = (array4[0] + array4[1]) / 2.0;
					break;
				case 7:
					num16 = (array4[2] + array4[3]) / 2.0;
					break;
				}
			}
			else
			{
				num16 = array4[3];
			}
			if (num16 > 60.0)
			{
				num16 -= 90.0;
			}
			else if (num16 < -60.0)
			{
				num16 += 90.0;
			}
			if (pfile.LocateParam.Quad_UseSideLocate && (num16 >= 1.0 || num16 <= -1.0))
			{
				num16 = 0.0;
			}
			LocateAngle = num16;
			DebugCode = 9;
			Mat rotationMatrix2D = Cv2.GetRotationMatrix2D(center2, num16, 1.0);
			Size dsize = default(Size);
			dsize.Width = QuadRotateImageROI.Cols;
			dsize.Height = QuadRotateImageROI.Rows;
			double num17 = Math.Abs(num16);
			double num18 = 0.1;
			if (pfile.LocateParam.LocateSpeedUp)
			{
				num18 = pfile.LocateParam.doRotateMinAngle;
			}
			Rect roi = maxSearchRect;
			if (num17 <= num18)
			{
				QuadRotateImageROI.CopyTo(buffers.LocatedSrc);
				if (pfile.LocateParam.EnableAILocate)
				{
					roi &= rect;
					Mat mat5 = new Mat(buffers.GetChannelMat(18), roi);
					Mat m2 = new Mat(buffers.GetAILocateMat(21), roi);
					mat5.CopyTo(m2);
					Mat mat6 = new Mat(buffers.GetChannelMat(19), roi);
					m2 = new Mat(buffers.GetAILocateMat(22), roi);
					mat6.CopyTo(m2);
					Mat mat7 = new Mat(buffers.GetChannelMat(20), roi);
					m2 = new Mat(buffers.GetAILocateMat(23), roi);
					mat7.CopyTo(m2);
				}
			}
			else
			{
				Cv2.WarpAffine(QuadRotateImageROI, buffers.LocatedSrc, rotationMatrix2D, dsize, InterpolationFlags.Linear, BorderTypes.Replicate, Scalar.White);
				if (pfile.LocateParam.EnableAILocate)
				{
					Mat mat8 = new Mat(buffers.GetChannelMat(18), roi);
					Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(21), roi), src: mat8, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
					Mat mat9 = new Mat(buffers.GetChannelMat(19), roi);
					Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(22), roi), src: mat9, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
					Mat mat10 = new Mat(buffers.GetChannelMat(20), roi);
					Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(23), roi), src: mat10, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
				}
			}
			Corner.X -= rect2.X;
			Corner.Y -= rect2.Y;
			Corner3.X -= rect2.X;
			Corner3.Y -= rect2.Y;
			Corner2.X -= rect2.X;
			Corner2.Y -= rect2.Y;
			Corner4.X -= rect2.X;
			Corner4.Y -= rect2.Y;
			GetPointAffinedPos(Corner, (Point)center, num16, ref Corner);
			GetPointAffinedPos(Corner2, (Point)center, num16, ref Corner2);
			GetPointAffinedPos(Corner3, (Point)center, num16, ref Corner3);
			GetPointAffinedPos(Corner4, (Point)center, num16, ref Corner4);
			list3.Clear();
			list3.Add(Corner);
			list3.Add(Corner2);
			list3.Add(Corner3);
			list3.Add(Corner4);
			rotatedRect = Cv2.MinAreaRect(list3);
			Rect rect3 = default(Rect);
			rect3 = Cv2.BoundingRect(list3);
			if (pfile.LocateParam.Quad_EnableAvgEdge_L)
			{
				new List<Point>();
				int num19 = 0;
				int num20 = 0;
				int num21 = 0;
				for (int num22 = 0; num22 < QuadScanPt[0].Count; num22++)
				{
					num20 += QuadScanPt[0][num22].X;
					num21++;
				}
				if (num21 > 0)
				{
					num19 = num20 / num21 - rect2.X;
					rect3.Width -= num19 - rect3.Left;
					rect3.Left = num19;
				}
			}
			if (pfile.LocateParam.Quad_EnableAvgEdge_R)
			{
				new List<Point>();
				int num23 = 0;
				int num24 = 0;
				int num25 = 0;
				for (int num26 = 0; num26 < QuadScanPt[1].Count; num26++)
				{
					num24 += QuadScanPt[1][num26].X;
					num25++;
				}
				if (num25 > 0)
				{
					num23 = num24 / num25 - rect2.X;
					rect3.Width += num23 - rect3.Right;
				}
			}
			if (pfile.LocateParam.Quad_EnableAvgEdge_T)
			{
				new List<Point>();
				int num27 = 0;
				int num28 = 0;
				int num29 = 0;
				for (int num30 = 0; num30 < QuadScanPt[2].Count; num30++)
				{
					num28 += QuadScanPt[2][num30].Y;
					num29++;
				}
				if (num29 > 0)
				{
					num27 = num28 / num29 - rect2.Y;
					rect3.Height -= num27 - rect3.Top;
					rect3.Top = num27;
				}
			}
			if (pfile.LocateParam.Quad_EnableAvgEdge_B)
			{
				new List<Point>();
				int num31 = 0;
				int num32 = 0;
				int num33 = 0;
				for (int num34 = 0; num34 < QuadScanPt[3].Count; num34++)
				{
					num32 += QuadScanPt[3][num34].Y;
					num33++;
				}
				if (num33 > 0)
				{
					num31 = num32 / num33 - rect2.Y;
					rect3.Height += num31 - rect3.Bottom;
				}
			}
			if (rect3.Width <= 0 || rect3.Height <= 0)
			{
				LocateSuccess = false;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[四邊形定位失敗] 四邊形定位檢測框(rotaterect)參數錯誤，寬度高度為0!";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[四边形定位失败] 四边形定位检测框(rotaterect)参数错误，宽度高度为0!";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "四角形位置測定失敗：四角形位置検出ボックス(rotaterect)のパラメータエラー、幅と高さが0です！";
				}
				else
				{
					ErrorMsg = "[Quad Locate Fail] Locate RotateRect width or height = 0!";
				}
				return false;
			}
			DebugCode = 10;
			if (pfile.LocateParam.QUAD_SideLocate)
			{
				rect3.Height /= 2;
			}
			LocateRect = rect3;
			LocateCenterX = LocateRect.X;
			LocateCenterY = LocateRect.Y + LocateRect.Height / 2;
			if (OffLine && OffLineTestEnhance)
			{
				ImageEnhance(LocateRect);
			}
			if (OffLine)
			{
				using Mat m3 = new Mat(buffers.Dst, rect2);
				buffers.LocatedSrc.CopyTo(m3);
			}
			LocateRect &= new Rect(0, 0, buffers.Src.Cols, buffers.Src.Rows);
			LocateImageRect = rect2;
			buffers.LocateImageRect = LocateImageRect;
			buffers.LocatedRect = LocateRect;
			DebugCode = 11;
			buffers.LocatedRectTmp = LocateRect;
			if (pfile.SelectWayON)
			{
				buffers.LocatedSrc.CopyTo(buffers.LocatedSrcTmp);
			}
			else
			{
				buffers.LocatedSrcTmp = buffers.LocatedSrc;
			}
			int[] array5 = new int[6];
			for (int num35 = 0; num35 < 6; num35++)
			{
				array5[num35] = pfile.EnhanceParam[num35].FunctionIndex;
			}
			buffers.SetLocatedBuffer(buffers.LocatedSrc, array5, pfile.AIEnhanceToolON, pfile.LocateParam.CreateHSVChannel, OffLine);
			if (pfile.LocateParam.EnableFocusCheck)
			{
				FocusResult = CheckFocus();
				if (!FocusResult)
				{
					FocusResult = false;
					if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[定位失敗] 檢測到失焦! 分數={0:N1}, 設定={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[定位失败] 检测到失焦! 分数={0:N1}, 设定={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[検出失敗] ピントが外れています!スコア={0:N1}, 設定={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
					}
					else
					{
						ErrorMsg = string.Format("[Locate Fail] Out of focus! Score={0:N1}, Threshold={1:N1}", FocusScore, pfile.LocateParam.FocusLimit);
					}
					return false;
				}
			}
			DebugCode = 12;
			LocateAngle = num16;
			double num36 = (double)LocateRect.Width * pfile.PixelPerUM_X;
			double num37 = (double)LocateRect.Height * pfile.PixelPerUM_Y;
			LocateWidth = num36;
			LocateHeight = num37;
			if (Math.Abs(num16) > pfile.LocateParam.LocateAngleLimit)
			{
				LocateSuccess = false;
				if (pfile.MeasureUnit == 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[定位角度NG]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[[定位角度NG]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[[位置決め角度NG]\n 幅:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
					else
					{
						ErrorMsg = string.Format("[Locate Angle Fail]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[定位角度NG]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[[定位角度NG]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[[位置決め角度NG]\n 幅:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else
				{
					ErrorMsg = string.Format("[Locate Angle Fail]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				LocateSizeSuccess = false;
				return false;
			}
			LocateSuccess = true;
			double LocateWidthMax = 0.0;
			double LocateWidthMin = 0.0;
			double LocateHeightMin = 0.0;
			double LocateHeightMax = 0.0;
			GetLocateSizeLimit(ref LocateWidthMin, ref LocateWidthMax, ref LocateHeightMin, ref LocateHeightMax);
			if (num36 > LocateWidthMax || num36 < LocateWidthMin || num37 < LocateHeightMin || num37 > LocateHeightMax)
			{
				if (pfile.MeasureUnit == 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg = string.Format("[定位尺寸NG]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = string.Format("[[定位尺寸NG]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = string.Format("[位置サイズNG]\n 幅:{0:N3}um, 高さ:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
					else
					{
						ErrorMsg = string.Format("[Locate Size Fail]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num36, num37, Math.Abs(num16));
					}
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[定位尺寸NG]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[[定位尺寸NG]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[位置サイズNG]\n 幅:{0:N3}mm, 高さ:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else
				{
					ErrorMsg = string.Format("[Locate Size Fail]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				LocateSizeSuccess = false;
				return false;
			}
			if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[定位成功]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[定位成功]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[四角形位置が確定しました]\n 幅:{0:N3}um, 高さ:{1:N3}um, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
				else
				{
					ErrorMsg = string.Format("[Locate Size Success]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num36, num37, Math.Abs(num16));
				}
			}
			else if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[定位成功]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[定位成功]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[四角形位置が確定しました]\n 幅:{0:N3}mm, 高さ:{1:N3}mm, 角度:{2:N3} \n", num36, num37, Math.Abs(num16));
			}
			else
			{
				ErrorMsg = string.Format("[Locate Size Success]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num36, num37, Math.Abs(num16));
			}
			LocateSuccess = true;
			LocateSizeSuccess = true;
			return true;
		}
		LocateSuccess = false;
		LocateSizeSuccess = false;
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool PadLocate()
	{
		_ = new LineSegmentPoint[4];
		ErrorMsg = "";
		LocateSuccess = false;
		LocateSizeSuccess = false;
		LocateRect = new Rect(0, 0, 0, 0);
		LocateImageRect = new Rect(0, 0, 0, 0);
		LocateWidth = 0.0;
		LocateHeight = 0.0;
		Rect rect = new Rect(0, 0, buffers.Src.Width, buffers.Src.Height);
		if (!pfile.LocateParam.EnableAILocate)
		{
			ReCalcMaxSearchRect(pfile.LocateParam.LocateType, rect);
		}
		Rect maxSearchRect = pfile.LocateParam.MaxSearchRect;
		Rect[] array = new Rect[2]
		{
			pfile.LocateParam.PadSearchRectL & rect,
			pfile.LocateParam.PadSearchRectR & rect
		};
		if (array[0].Width <= 0 || array[0].Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[電極定位失敗] 電極定位左邊檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[电极定位失败] 电极定位左边检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "【電極定位失敗】電極定位左側の検出ボックスのパラメータが誤っています。幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Pad Locate Fail]  Left Tracker width = 0!";
			}
			return false;
		}
		if (array[1].Width <= 0 || array[1].Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[電極定位失敗] 電極定位右邊檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[电极定位失败] 电极定位右边检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "【電極定位失敗】電極定位右側の検出ボックスのパラメータが誤っています。幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Pad Locate Fail]  Left Tracker width = 0!";
			}
			return false;
		}
		if (maxSearchRect.Width <= 0 || maxSearchRect.Height <= 0)
		{
			LocateSuccess = false;
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = "[電極定位失敗] 電極定位檢測框參數錯誤，寬度高度為0!";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = "[电极定位失败] 电极定位检测框参数错误，宽度高度为0!";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = "【電極定位失敗】電極定位の検出ボックスのパラメータが誤っています。幅と高さが0です！";
			}
			else
			{
				ErrorMsg = "[Pad Locate Fail]  Tracker width = 0!";
			}
			return false;
		}
		PadBinary[0] = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Pad_Channel_L1), array[0]).Clone();
		PadBinary[1] = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Pad_Channel_R1), array[1]).Clone();
		PadBinary[2] = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Pad_Channel_L2), array[0]).Clone();
		PadBinary[3] = new Mat(buffers.GetSrcBGR(pfile.LocateParam.Pad_Channel_R2), array[1]).Clone();
		Cv2.InRange(PadBinary[0], pfile.LocateParam.Pad_THD_Min_L1, pfile.LocateParam.Pad_THD_Max_L1, PadBinary[0]);
		DoMorph(ref PadBinary[0], (int)pfile.LocateParam.Pad_DEStart_L1, pfile.LocateParam.Pad_DEStartCount_L1);
		DoMorph(ref PadBinary[0], (int)pfile.LocateParam.Pad_DEEnd_L1, pfile.LocateParam.Pad_DEEndCount_L1);
		Cv2.InRange(PadBinary[1], pfile.LocateParam.Pad_THD_Min_R1, pfile.LocateParam.Pad_THD_Max_R1, PadBinary[1]);
		DoMorph(ref PadBinary[1], (int)pfile.LocateParam.Pad_DEStart_R1, pfile.LocateParam.Pad_DEStartCount_R1);
		DoMorph(ref PadBinary[1], (int)pfile.LocateParam.Pad_DEEnd_R1, pfile.LocateParam.Pad_DEEndCount_R1);
		Cv2.InRange(PadBinary[2], pfile.LocateParam.Pad_THD_Min_L2, pfile.LocateParam.Pad_THD_Max_L2, PadBinary[2]);
		DoMorph(ref PadBinary[2], (int)pfile.LocateParam.Pad_DEStart_L2, pfile.LocateParam.Pad_DEStartCount_L2);
		DoMorph(ref PadBinary[2], (int)pfile.LocateParam.Pad_DEEnd_L2, pfile.LocateParam.Pad_DEEndCount_L2);
		Cv2.InRange(PadBinary[3], pfile.LocateParam.Pad_THD_Min_R2, pfile.LocateParam.Pad_THD_Max_R2, PadBinary[3]);
		DoMorph(ref PadBinary[3], (int)pfile.LocateParam.Pad_DEStart_R2, pfile.LocateParam.Pad_DEStartCount_R2);
		DoMorph(ref PadBinary[3], (int)pfile.LocateParam.Pad_DEEnd_R2, pfile.LocateParam.Pad_DEEndCount_R2);
		Cv2.BitwiseAnd(PadBinary[0], PadBinary[2], PadBinary[0]);
		Cv2.BitwiseAnd(PadBinary[1], PadBinary[3], PadBinary[1]);
		List<Point> list = new List<Point>();
		Point item = default(Point);
		for (int i = 0; i < 2; i++)
		{
			_ = new Point2f[4];
			Cv2.FindContours(PadBinary[i], out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			if (contours.Length == 0)
			{
				LocateSuccess = false;
				if (i == 0)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg = "[電極定位失敗] 電極定位左邊檢測框掃描錯誤，找不到任何區塊，請重新調整二值化數值!";
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg = "[电极定位失败] 电极定位左边检测框扫描错误，找不到任何区块，请重新调整二值化数值!";
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg = "電極定位失敗] 電極定位左側の検出ボックススキャンエラー、ブロックが見つかりません。二値化数値を再調整してください！";
					}
					else
					{
						ErrorMsg = "[Pad Locate Fail] Left side scan error,  Try to Finetune binary parameter!";
					}
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg = "[電極定位失敗] 電極定位右邊檢測框掃描錯誤，找不到任何區塊，請重新調整二值化數值!";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = "[电极定位失败] 电极定位右边检测框扫描错误，找不到任何区块，请重新调整二值化数值!";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = "電極定位失敗] 電極定位右側の検出ボックススキャンエラー、ブロックが見つかりません。二値化数値を再調整してください！";
				}
				else
				{
					ErrorMsg = "[Pad Locate Fail] Left side scan error,  Try to Finetune binary parameter!";
				}
				return false;
			}
			double num = 0.0;
			RotatedRect rotatedRect = default(RotatedRect);
			for (int j = 0; j < contours.Length; j++)
			{
				double num2 = Cv2.ContourArea(contours[j]);
				if (num2 > num)
				{
					num = num2;
					rotatedRect = Cv2.MinAreaRect(contours[j]);
				}
			}
			Point2f[] array2 = new Point2f[4];
			array2 = rotatedRect.Points();
			for (int k = 0; k < 4; k++)
			{
				item.X = (int)array2[k].X + array[i].Left;
				item.Y = (int)array2[k].Y + array[i].Top;
				list.Add(item);
			}
		}
		RotatedRect rotatedRect2 = Cv2.MinAreaRect(list);
		Point2f center = rotatedRect2.Center;
		Point2f[] array3 = new Point2f[4];
		array3 = rotatedRect2.Points();
		Rect rect2 = maxSearchRect;
		new Mat(buffers.Src, rect2).CopyTo(PadRotateImageROI);
		Point2f center2 = default(Point);
		center2.X = rect2.Width / 2;
		center2.Y = rect2.Height / 2;
		double num3 = rotatedRect2.Angle;
		if (num3 > 60.0)
		{
			num3 -= 90.0;
		}
		else if (num3 < -60.0)
		{
			num3 += 90.0;
		}
		LocateAngle = num3;
		Mat rotationMatrix2D = Cv2.GetRotationMatrix2D(center2, num3, 1.0);
		Size dsize = default(Size);
		dsize.Width = PadRotateImageROI.Cols;
		dsize.Height = PadRotateImageROI.Rows;
		double num4 = Math.Abs(num3);
		double num5 = 0.1;
		if (pfile.LocateParam.LocateSpeedUp)
		{
			num5 = pfile.LocateParam.doRotateMinAngle;
		}
		if (num4 <= num5)
		{
			PadRotateImageROI.CopyTo(buffers.LocatedSrc);
			if (pfile.LocateParam.EnableAILocate)
			{
				Rect roi = maxSearchRect;
				Mat mat = new Mat(buffers.GetChannelMat(18), roi);
				Mat m = new Mat(buffers.GetAILocateMat(21), roi);
				mat.CopyTo(m);
				Mat mat2 = new Mat(buffers.GetChannelMat(19), roi);
				m = new Mat(buffers.GetAILocateMat(22), roi);
				mat2.CopyTo(m);
				Mat mat3 = new Mat(buffers.GetChannelMat(20), roi);
				m = new Mat(buffers.GetAILocateMat(23), roi);
				mat3.CopyTo(m);
			}
		}
		else
		{
			Cv2.WarpAffine(PadRotateImageROI, buffers.LocatedSrc, rotationMatrix2D, dsize, InterpolationFlags.Linear, BorderTypes.Replicate, Scalar.White);
			if (pfile.LocateParam.EnableAILocate)
			{
				Rect roi2 = maxSearchRect;
				Mat mat4 = new Mat(buffers.GetChannelMat(18), roi2);
				Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(21), roi2), src: mat4, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
				Mat mat5 = new Mat(buffers.GetChannelMat(19), roi2);
				Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(22), roi2), src: mat5, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
				Mat mat6 = new Mat(buffers.GetChannelMat(20), roi2);
				Cv2.WarpAffine(dst: new Mat(buffers.GetAILocateMat(23), roi2), src: mat6, m: rotationMatrix2D, dsize: dsize, flags: InterpolationFlags.Linear, borderMode: BorderTypes.Replicate, borderValue: Scalar.White);
			}
		}
		Point dstpt = default(Point);
		dstpt.X = (int)array3[1].X;
		dstpt.Y = (int)array3[1].Y;
		Point dstpt2 = default(Point);
		dstpt2.X = (int)array3[3].X;
		dstpt2.Y = (int)array3[3].Y;
		dstpt.X -= rect2.X;
		dstpt.Y -= rect2.Y;
		dstpt2.X -= rect2.X;
		dstpt2.Y -= rect2.Y;
		GetPointAffinedPos(dstpt, (Point)center, num3, ref dstpt);
		GetPointAffinedPos(dstpt2, (Point)center, num3, ref dstpt2);
		LocateRect = new Rect
		{
			X = Math.Min(dstpt.X, dstpt2.X),
			Y = Math.Min(dstpt.Y, dstpt2.Y),
			Width = Math.Abs(dstpt2.X - dstpt.X),
			Height = Math.Abs(dstpt2.Y - dstpt.Y)
		};
		LocateCenterX = LocateRect.X;
		LocateCenterY = LocateRect.Y + LocateRect.Height / 2;
		if (OffLine && OffLineTestEnhance)
		{
			ImageEnhance(LocateRect);
		}
		if (OffLine)
		{
			using Mat m2 = new Mat(buffers.Dst, rect2);
			buffers.LocatedSrc.CopyTo(m2);
		}
		LocateRect &= new Rect(0, 0, buffers.Src.Cols, buffers.Src.Rows);
		LocateImageRect = rect2;
		buffers.LocateImageRect = LocateImageRect;
		buffers.LocatedRect = LocateRect;
		buffers.LocatedRectTmp = LocateRect;
		if (pfile.SelectWayON)
		{
			buffers.LocatedSrc.CopyTo(buffers.LocatedSrcTmp);
		}
		else
		{
			buffers.LocatedSrcTmp = buffers.LocatedSrc;
		}
		int[] array4 = new int[6];
		for (int l = 0; l < 6; l++)
		{
			array4[l] = pfile.EnhanceParam[l].FunctionIndex;
		}
		buffers.SetLocatedBuffer(buffers.LocatedSrc, array4, pfile.AIEnhanceToolON, pfile.LocateParam.CreateHSVChannel, OffLine);
		LocateAngle = num3;
		double num6 = (double)LocateRect.Width * pfile.PixelPerUM_X;
		double num7 = (double)LocateRect.Height * pfile.PixelPerUM_Y;
		LocateWidth = num6;
		LocateHeight = num7;
		if (Math.Abs(num3) > pfile.LocateParam.LocateAngleLimit)
		{
			if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[定位角度NG]\n [Locate] 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[[定位角度NG]\n [Locate] 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[[位置決め角度NG]\n [Locate] 幅:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
				else
				{
					ErrorMsg = string.Format("[Locate Angle Fail]\n [Locate] Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
			}
			else if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[定位角度NG]\n [Locate] 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[[定位角度NG]\n [Locate] 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[[位置決め角度NG]\n [Locate] 幅:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else
			{
				ErrorMsg = string.Format("[Locate Angle Fail]\n [Locate] Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			LocateSuccess = false;
			return false;
		}
		LocateSuccess = true;
		double LocateWidthMax = 0.0;
		double LocateWidthMin = 0.0;
		double LocateHeightMin = 0.0;
		double LocateHeightMax = 0.0;
		GetLocateSizeLimit(ref LocateWidthMin, ref LocateWidthMax, ref LocateHeightMin, ref LocateHeightMax);
		if (num6 > LocateWidthMax || num6 < LocateWidthMin || num7 < LocateHeightMin || num7 > LocateHeightMax)
		{
			if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg = string.Format("[定位尺寸NG]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg = string.Format("[[定位尺寸NG]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg = string.Format("[[位置決め寸法NG]\n 幅:{0:N3}um, 高さ:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
				else
				{
					ErrorMsg = string.Format("[Locate Size Fail]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num6, num7, Math.Abs(num3));
				}
			}
			else if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[定位尺寸NG]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[[定位尺寸NG]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[[位置決め寸法NG]\n 幅:{0:N3}mm, 高さ:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else
			{
				ErrorMsg = string.Format("[Locate Size Fail]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			LocateSizeSuccess = false;
			return false;
		}
		if (pfile.MeasureUnit == 0)
		{
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[定位成功]\n 寬:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[定位成功]\n 宽:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[定位成功]\n 幅:{0:N3}um, 高:{1:N3}um, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
			else
			{
				ErrorMsg = string.Format("[Locate Success]\n Width:{0:N3}um, Height:{1:N3}um, Angle:{2:N3} \n", num6, num7, Math.Abs(num3));
			}
		}
		else if (buffers.LanguageType == 1)
		{
			ErrorMsg = string.Format("[定位成功]\n 寬:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
		}
		else if (buffers.LanguageType == 2)
		{
			ErrorMsg = string.Format("[定位成功]\n 宽:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
		}
		else if (buffers.LanguageType == 3)
		{
			ErrorMsg = string.Format("[定位成功]\n 幅:{0:N3}mm, 高:{1:N3}mm, 角度:{2:N3} \n", num6, num7, Math.Abs(num3));
		}
		else
		{
			ErrorMsg = string.Format("[Locate Success]\n Width:{0:N3}mm, Height:{1:N3}mm, Angle:{2:N3} \n", num6, num7, Math.Abs(num3));
		}
		LocateSuccess = true;
		LocateSizeSuccess = true;
		return true;
	}

	public bool GetPointAffinedPos(Point srcpt, Point center, double angle, ref Point dstpt)
	{
		angle = angle * Math.PI / 180.0;
		int num = srcpt.X - center.X;
		int num2 = srcpt.Y - center.Y;
		dstpt.X = (int)Math.Round((double)num * Math.Cos(angle) + (double)num2 * Math.Sin(angle) + (double)center.X);
		dstpt.Y = (int)Math.Round((double)(-num) * Math.Sin(angle) + (double)num2 * Math.Cos(angle) + (double)center.Y);
		return true;
	}

	private double GetAvgAngle3(double[] angles)
	{
		for (int i = 0; i < 4; i++)
		{
			if (angles[i] > 60.0)
			{
				angles[i] -= 90.0;
			}
			else if (angles[i] < -60.0)
			{
				angles[i] += 90.0;
			}
		}
		double num = (angles[0] + angles[1] + angles[2] + angles[3]) / 4.0;
		double num2 = 0.0;
		int num3 = 0;
		double num4 = 0.0;
		for (int j = 0; j < 4; j++)
		{
			num4 = Math.Abs(angles[j] - num);
			if (num4 > num2)
			{
				num3 = j;
				num2 = num4;
			}
		}
		double num5 = 0.0;
		for (int k = 0; k < 4; k++)
		{
			if (k != num3)
			{
				num5 += angles[k];
			}
		}
		return num5 / 3.0;
	}

	private void DoMorph(ref Mat mat, int index, int count)
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
			case 10:
				Cv2.BitwiseNot(mat, mat);
				break;
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanHLine(Mat BinaryROI, Rect roi, out List<Point> edgept1, bool directionT2B = true, bool UseSinglePt = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			IntPtr data = BinaryROI.Data;
			int width = BinaryROI.Width;
			Point[] array = new Point[2];
			edgept1 = new List<Point>();
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
							if (ptr[num] == BackgroundColor)
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
							if (ptr2[num2] == BackgroundColor)
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
					}
				}
			}
			if (edgept1.Count <= 1)
			{
				return false;
			}
			if (!pfile.LocateParam.UseMelfMode)
			{
				if (pfile.LocateParam.Quad_FilterPoint)
				{
					FilterPoint(ref edgept1, 1);
				}
				if (pfile.LocateParam.Quad_UseStdFilter)
				{
					StdFilter(ref edgept1, 1);
				}
			}
			if (edgept1.Count <= 1)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "LT03-LocateTool-ScanHLine()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-004");
			}
			edgept1 = new List<Point>();
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanVLine(Mat BinaryROI, Rect roi, out List<Point> edgept1, bool directionL2R = true, bool UseSinglePt = false, int EdgeColor = 255, int BackgroundColor = 0)
	{
		try
		{
			IntPtr data = BinaryROI.Data;
			int width = BinaryROI.Width;
			int num = 0;
			Point[] array = new Point[2];
			edgept1 = new List<Point>();
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
							if (ptr[num + j] == BackgroundColor)
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
							if (ptr2[num + num2] == BackgroundColor)
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
					}
				}
			}
			if (edgept1.Count <= 1)
			{
				return false;
			}
			if (!pfile.LocateParam.Quad_UseSideLocate)
			{
				if (pfile.LocateParam.Quad_FilterPoint)
				{
					FilterPoint(ref edgept1, 0);
				}
				if (pfile.LocateParam.Quad_UseStdFilter)
				{
					StdFilter(ref edgept1, 0);
				}
			}
			if (edgept1.Count <= 1)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "LT04-LocateTool-ScanVLine()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-005");
			}
			edgept1 = new List<Point>();
			return false;
		}
	}

	public void StdFilter(ref List<Point> pt, int index)
	{
		if (pt.Count - 3 <= 0)
		{
			return;
		}
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
			for (int k = 0; k < pt.Count - 1; k++)
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
			for (int n = 0; n < pt.Count - 1; n++)
			{
				if (Math.Abs((double)pt[n].Y - num) <= num4)
				{
					list.Add(pt[n]);
				}
			}
		}
		pt.Clear();
		for (int num5 = 0; num5 < list.Count - 1; num5++)
		{
			pt.Add(list[num5]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void FilterPoint(ref List<Point> pt, int index)
	{
		if (pt.Count - 3 <= 0)
		{
			return;
		}
		try
		{
			if (index == 0)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				for (int i = 0; i < pt.Count - 3; i++)
				{
					if (pt[i].X == pt[i + 1].X)
					{
						num3++;
					}
					else if (pt[i].X < pt[i + 1].X)
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
				List<Point> list = new List<Point>();
				if (num3 > num && num3 > num2)
				{
					for (int j = 0; j < pt.Count - 3; j++)
					{
						if (pt[j].X == pt[j + 1].X)
						{
							list.Add(pt[j]);
						}
					}
				}
				else if (num > num2 && num > num3)
				{
					for (int k = 0; k < pt.Count - 3; k++)
					{
						if (pt[k].X < pt[k + 1].X)
						{
							list.Add(pt[k]);
						}
					}
				}
				else
				{
					for (int l = 0; l < pt.Count - 3; l++)
					{
						if (pt[l].X > pt[l + 1].X)
						{
							list.Add(pt[l]);
						}
					}
				}
				pt.Clear();
				for (int m = 0; m < list.Count - 3; m++)
				{
					pt.Add(list[m]);
				}
				return;
			}
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int n = 0; n < pt.Count - 3; n++)
			{
				if (pt[n].Y == pt[n + 1].Y)
				{
					num6++;
				}
				else if (pt[n].Y < pt[n + 1].Y)
				{
					num4++;
				}
				else
				{
					num5++;
				}
			}
			List<Point> list2 = new List<Point>();
			if (num6 >= num4 && num6 >= num5)
			{
				for (int num7 = 0; num7 < pt.Count - 3; num7++)
				{
					if (pt[num7].Y == pt[num7 + 1].Y)
					{
						list2.Add(pt[num7]);
					}
				}
			}
			else if (num4 >= num5 && num4 >= num6)
			{
				for (int num8 = 0; num8 < pt.Count - 3; num8++)
				{
					if (pt[num8].Y < pt[num8 + 1].Y)
					{
						list2.Add(pt[num8]);
					}
				}
			}
			else
			{
				for (int num9 = 0; num9 < pt.Count - 3; num9++)
				{
					if (pt[num9].Y > pt[num9 + 1].Y)
					{
						list2.Add(pt[num9]);
					}
				}
			}
			pt.Clear();
			for (int num10 = 0; num10 < list2.Count - 1; num10++)
			{
				pt.Add(list2[num10]);
			}
		}
		catch (Exception ex)
		{
			string text = "LT05-LocateTool-FilterPoint()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-006");
			}
		}
	}

	public void TimeStart()
	{
		sw.Start();
	}

	public double TimeStop()
	{
		sw.Stop();
		double totalMilliseconds = sw.Elapsed.TotalMilliseconds;
		sw.Reset();
		return totalMilliseconds;
	}

	private double Get_point_angle(Point pointO, Point pointA)
	{
		Point point = new Point(pointA.X - pointO.X, pointA.Y - pointO.Y);
		if (point.X == 0 && point.Y == 0)
		{
			return 0.0;
		}
		if (point.X == 0)
		{
			return 90.0;
		}
		if (point.Y == 0)
		{
			return 0.0;
		}
		double d = Math.Abs((double)point.Y / (double)point.X);
		d = Math.Atan(d);
		d = d * 180.0 / Math.PI;
		if (0 < point.X && 0 < point.Y)
		{
			return d;
		}
		return 0.0;
	}

	private void GetLocateSizeLimit(ref double LocateWidthMin, ref double LocateWidthMax, ref double LocateHeightMin, ref double LocateHeightMax)
	{
		LocateWidthMin = GetComDigitsValue(pfile.LocateParam.LocateWidthMin);
		LocateWidthMax = GetComDigitsValue(pfile.LocateParam.LocateWidthMax);
		LocateHeightMin = GetComDigitsValue(pfile.LocateParam.LocateHeightMin);
		LocateHeightMax = GetComDigitsValue(pfile.LocateParam.LocateHeightMax);
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
			string text = "BT01-BinaryTool-GetComDigitsValue()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Locate-007");
			}
			return 0.0;
		}
	}

	private bool GetIntersection2(LineSegmentPoint line1, LineSegmentPoint line2, out Point Corner)
	{
		Corner = new Point(0, 0);
		double num = (double)(line1.P1.Y - line1.P2.Y) / (double)(line1.P1.X - line1.P2.X);
		double num2 = (double)line1.P1.Y - num * (double)line1.P1.X;
		double num3 = (double)(line2.P1.Y - line2.P2.Y) / (double)(line2.P1.X - line2.P2.X);
		double num4 = (double)line2.P1.Y - num3 * (double)line2.P1.X;
		if (Math.Abs(num - num3) < double.Epsilon)
		{
			return false;
		}
		double num5 = (num4 - num2) / (num - num3);
		double num6 = num * num5 + num2;
		Corner = new Point((int)num5, (int)num6);
		return true;
	}

	private bool GetIntersection(LineSegmentPoint line1, LineSegmentPoint line2, out Point Corner)
	{
		Corner = new Point(0, 0);
		int x = line1.P1.X;
		int y = line1.P1.Y;
		int num = line1.P2.X - line1.P1.X;
		int num2 = line1.P2.Y - line1.P1.Y;
		int x2 = line2.P1.X;
		int y2 = line2.P1.Y;
		int num3 = line2.P2.X - line2.P1.X;
		int num4 = line2.P2.Y - line2.P1.Y;
		double num5 = num3 * num2 - num * num4;
		if (num5 == 0.0)
		{
			return false;
		}
		int num6 = x2 - x;
		int num7 = y2 - y;
		double num8 = (double)(num3 * num7 - num4 * num6) / num5;
		_ = (double)(num * num7 - num2 * num6) / num5;
		Corner = new Point(Math.Round((double)x + (double)num * num8), Math.Round((double)y + (double)num2 * num8));
		return true;
	}

	public void SetEnhanceParam(double h, double s, double v, double r, double g, double b, bool mirrorh, bool mirrorv)
	{
		EnhanceH = h;
		EnhanceS = s;
		EnhanceV = v;
		EnhanceR = r;
		EnhanceG = g;
		EnhanceB = b;
		EnhanceHMirror = mirrorh;
		EnhanceVMirror = mirrorv;
	}

	private void ImageEnhance(Rect LocateRect)
	{
		Mat mat = new Mat(buffers.LocatedSrc, LocateRect);
		Mat mat2 = mat.Clone();
		Cv2.Split(mat2, out SrcBGR);
		if (EnhanceB != 1.0)
		{
			SrcBGR[0].ConvertTo(SrcBGR[0], MatType.CV_8UC1, EnhanceB);
		}
		if (EnhanceG != 1.0)
		{
			SrcBGR[1].ConvertTo(SrcBGR[1], MatType.CV_8UC1, EnhanceG);
		}
		if (EnhanceR != 1.0)
		{
			SrcBGR[2].ConvertTo(SrcBGR[2], MatType.CV_8UC1, EnhanceR);
		}
		Cv2.Merge(SrcBGR, mat2);
		Mat mat3 = new Mat();
		Cv2.CvtColor(mat2, mat3, ColorConversionCodes.BGR2HSV);
		Cv2.Split(mat3, out SrcHSV);
		if (EnhanceH != 1.0)
		{
			SrcHSV[0].ConvertTo(SrcHSV[0], MatType.CV_8UC1, EnhanceH);
		}
		if (EnhanceS != 1.0)
		{
			SrcHSV[1].ConvertTo(SrcHSV[1], MatType.CV_8UC1, EnhanceS);
		}
		if (EnhanceV != 1.0)
		{
			SrcHSV[2].ConvertTo(SrcHSV[2], MatType.CV_8UC1, EnhanceV);
		}
		Cv2.Merge(SrcHSV, mat3);
		Cv2.CvtColor(mat3, mat2, ColorConversionCodes.HSV2BGR);
		if (EnhanceVMirror)
		{
			Cv2.Flip(mat2, mat2, FlipMode.X);
		}
		if (EnhanceHMirror)
		{
			Cv2.Flip(mat2, mat2, FlipMode.Y);
		}
		mat2.CopyTo(mat);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckFocus()
	{
		LaplacianMat = buffers.LocateGray.Laplacian(3, 5);
		Cv2.ConvertScaleAbs(LaplacianMat, LaplacianMat);
		double num = (FocusScore = LaplacianMat.Mean()[0] * 2.0);
		if (num < pfile.LocateParam.FocusLimit)
		{
			if (buffers.LanguageType == 1)
			{
				ErrorMsg = string.Format("[定位失敗] 檢測到失焦! 分數={0:N1}, 設定={1:N1}", num, pfile.LocateParam.FocusLimit);
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg = string.Format("[定位失败] 检测到失焦! 分数={0:N1}, 设定={1:N1}", num, pfile.LocateParam.FocusLimit);
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg = string.Format("[検出失敗] ピントが外れています!スコア={0:N1}, 設定={1:N1}", num, pfile.LocateParam.FocusLimit);
			}
			else
			{
				ErrorMsg = string.Format("[Locate Fail] Out of focus! Score={0:N1}, Threshold={1:N1}", num, pfile.LocateParam.FocusLimit);
			}
			return false;
		}
		return true;
	}

	public void ReLoadAIModel(bool ReCreatePB = false)
	{
		locateAIEnhanceTool.ModelLoaded = false;
		locateAIEnhanceTool.ReadModel(ReCreatePB);
	}

	public bool PredictDBMat(Mat mat)
	{
		return locateAIEnhanceTool.PredictDBMat(mat);
	}

	public bool PredictMat(Mat mat)
	{
		return locateAIEnhanceTool.PredictMat(mat, Offline: true);
	}

	public Mat GetDestDBMatColor()
	{
		return locateAIEnhanceTool.GetDestDBMatColor();
	}

	public Mat GetDestMatColor()
	{
		return locateAIEnhanceTool.GetDestMatColor();
	}

	public void SendAIPredictTest()
	{
		locateAIEnhanceTool.SendPredictTest();
	}
}
