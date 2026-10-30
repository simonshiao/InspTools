using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class SelectFaceTool
{
	private const int MaxThdTool = 3;

	private Mat[] Binary = new Mat[3];

	private Mat FinalBinary = new Mat();

	private CVBuffer buffers;

	private int SelectFaceIndex;

	public BlobStruct Blobs;

	public Rect NGRect;

	private ParamFile pfile;

	public Rect InspeRect;

	private Stopwatch sw = new Stopwatch();

	public bool[] FACEResult = new bool[4];

	public double[] MaxFaceResult = new double[4];

	public int FaceEnableCount;

	public double AllArea;

	public int RowMin;

	private bool OffLine = true;

	public bool SelectFaceSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public string ErrorMsg { get; set; }

	public SelectFaceTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 4; i++)
		{
			FACEResult[i] = false;
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
	public bool Inspect(int index, bool offline = true)
	{
		try
		{
			OffLine = offline;
			Tracker1ResultText = "";
			FACEResult[index] = false;
			MaxFaceResult[index] = 0.0;
			if (index == 0)
			{
				FaceEnableCount = 0;
			}
			if (!pfile.SelectFaceParam[index].TrackerON)
			{
				return false;
			}
			FaceEnableCount++;
			SelectFaceIndex = index;
			DoThdImage(pfile.SelectFaceParam[index]);
			bool flag = FindBlob(FinalBinary, index);
			if (pfile.SelectFaceParam[0].RowMinSelectFace && index == 0)
			{
				if (FindMinRow())
				{
					RowMin = 1;
				}
				else
				{
					RowMin = 0;
				}
			}
			FACEResult[index] = flag;
			return flag;
		}
		catch (Exception ex)
		{
			string text = "SFT03-SelectFaceTool-Inspect()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-001");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool FindMinRow()
	{
		try
		{
			Rect rect = new Rect(Blobs.NGRect.Left - InspeRect.Left, Blobs.NGRect.Top - InspeRect.Top, Blobs.NGRect.Width, Blobs.NGRect.Height);
			int num = 5;
			int num2 = 5;
			Rect roi = new Rect(rect.Left + num, rect.Top - num2, rect.Width - num * 2, num2 * 6);
			Rect roi2 = new Rect(rect.Left + num, rect.Bottom - num2 * 5, rect.Width - num * 2, num2 * 6);
			Rect rect2 = new Rect(0, 0, FinalBinary.Width, FinalBinary.Height);
			roi &= rect2;
			roi2 &= rect2;
			Point trackPos = new Point(rect.Left, rect.Top);
			ScanGetHLine(FinalBinary, roi, out var edgept, trackPos);
			ScanGetHLine(FinalBinary, roi2, out var edgept2, trackPos, directionT2B: false);
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int i = 0; i < edgept.Count - 5; i++)
			{
				num3 += Math.Abs(edgept[i].Y - edgept[i + 5].Y);
			}
			for (int j = 0; j < edgept2.Count - 5; j++)
			{
				num4 += Math.Abs(edgept2[j].Y - edgept2[j + 5].Y);
			}
			Rect roi3 = new Rect(rect.Left - num2, rect.Top + num, num2 * 6, rect.Height - num * 2);
			Rect roi4 = new Rect(rect.Right - num2 * 5, rect.Top + num, num2 * 6, rect.Height - num * 2);
			roi3 &= rect2;
			roi4 &= rect2;
			ScanGetVLine(FinalBinary, roi3, out var edgept3, trackPos);
			ScanGetVLine(FinalBinary, roi4, out var edgept4, trackPos, directionL2R: false);
			for (int k = 0; k < edgept3.Count - 5; k++)
			{
				num5 += Math.Abs(edgept3[k].X - edgept3[k + 5].X);
			}
			for (int l = 0; l < edgept4.Count - 5; l++)
			{
				num6 += Math.Abs(edgept4[l].X - edgept4[l + 5].X);
			}
			int num7 = num3 + num4;
			int num8 = num5 + num6;
			if (edgept.Count <= 0 || edgept2.Count <= 0)
			{
				return true;
			}
			if (edgept3.Count <= 0 || edgept4.Count <= 0)
			{
				return false;
			}
			double num9 = Math.Abs(Cv2.FitLine(edgept, DistanceTypes.L2, 0.0, 0.01, 0.01).GetVectorAngle());
			double num10 = Math.Abs(Cv2.FitLine(edgept2, DistanceTypes.L2, 0.0, 0.01, 0.01).GetVectorAngle());
			Line2D line2D = Cv2.FitLine(edgept3, DistanceTypes.L2, 0.0, 0.01, 0.01);
			double num11 = Math.Abs(90.0 - Math.Abs(line2D.GetVectorAngle()));
			Line2D line2D2 = Cv2.FitLine(edgept4, DistanceTypes.L2, 0.0, 0.01, 0.01);
			double num12 = Math.Abs(90.0 - Math.Abs(line2D2.GetVectorAngle()));
			if (num9 + num10 > num11 + num12)
			{
				if (num9 + num10 > 4.0)
				{
					return true;
				}
			}
			else if (num11 + num12 > 4.0)
			{
				return false;
			}
			if (num7 > num8)
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			string text = "SFT03-SelectFaceTool-FindMinRow()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-002");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool DoThdImage(INSP_SELECTFACETOOL_PARAM param)
	{
		if (!param.TrackerON)
		{
			return true;
		}
		try
		{
			bool[] array = new bool[3];
			GetInspeRect(param);
			InspeRect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				return true;
			}
			bool[] array2 = new bool[3] { false, param.THD2_ORMode, param.THD3_ORMode };
			int num = 0;
			if (param.THD1_ChannelON)
			{
				new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), InspeRect).CopyTo(Binary[0]);
				Cv2.InRange(Binary[0], param.THD1_Min, param.THD1_Max, Binary[0]);
				DoMorph(ref Binary[0], (int)param.DE_Start1_Index, param.DE_Start1_Count);
				DoMorph(ref Binary[0], (int)param.DE_End1_Index, param.DE_End1_Count);
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
				DoMorph(ref Binary[1], (int)param.DE_Start2_Index, param.DE_Start2_Count);
				DoMorph(ref Binary[1], (int)param.DE_End2_Index, param.DE_End2_Count);
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
				DoMorph(ref Binary[2], (int)param.DE_Start3_Index, param.DE_Start3_Count);
				DoMorph(ref Binary[2], (int)param.DE_End3_Index, param.DE_End3_Count);
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
				if (array[i])
				{
					if (i == 0 || !array2[i])
					{
						Cv2.BitwiseAnd(FinalBinary, Binary[i], FinalBinary);
					}
					else
					{
						Cv2.BitwiseOr(FinalBinary, Binary[i], FinalBinary);
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "SFT04-SelectFaceTool-DoThdImage()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-003");
			}
			return false;
		}
	}

	public void GetInspeRect(INSP_SELECTFACETOOL_PARAM param)
	{
		LocateRect = buffers.LocatedRect;
		InspeRect.X = LocateRect.X + param.SearchRectL;
		InspeRect.Y = LocateRect.Y + param.SearchRectT;
		if (param.UseAutoSize)
		{
			InspeRect.Width = LocateRect.Width - (param.SearchRectL + param.SearchRectR);
			InspeRect.Height = LocateRect.Height - (param.SearchRectT + param.SearchRectB);
		}
		else
		{
			InspeRect.Width = param.SearchRectWidth;
			InspeRect.Height = param.SearchRectHeight;
		}
	}

	private void DoMorph(ref Mat mat, int index, int count)
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
	private bool FindBlob(Mat binary, int index)
	{
		try
		{
			AllArea = 0.0;
			int num = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num2 = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			AllArea = (double)binary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num3 = 0;
			int i = 0;
			if (num2 > 1)
			{
				for (int j = 1; j < num2; j++)
				{
					if (genericIndexer[j, 4] > num3)
					{
						i = j;
						num3 = genericIndexer[j, 4];
					}
				}
				rect = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
				num = num3;
			}
			else
			{
				rect = new Rect(0, 0, 0, 0);
				num = num3;
			}
			double num4 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			Blobs.Width = rect.Width;
			Blobs.Height = rect.Height;
			Blobs.WHRatio = num4;
			Blobs.Area = num3;
			Blobs.NGRect = rect;
			Blobs.NGRect.X += InspeRect.X;
			Blobs.NGRect.Y += InspeRect.Y;
			NGRect = Blobs.NGRect;
			if (pfile.SelectFaceParam[0].RowSelectFace)
			{
				switch (index)
				{
				case 0:
					MaxFaceResult[0] = (double)rect.Width * pfile.PixelPerUM_X;
					break;
				case 1:
					MaxFaceResult[1] = (double)rect.Height * pfile.PixelPerUM_Y;
					break;
				}
				switch (index)
				{
				case 0:
					MaxFaceResult[0] = (double)rect.Width * pfile.PixelPerUM_X;
					break;
				case 1:
					MaxFaceResult[1] = (double)rect.Height * pfile.PixelPerUM_Y;
					break;
				}
			}
			return CheckResult(rect, num, num4);
		}
		catch (Exception ex)
		{
			string text = "BT02-SelectFaceTool-FindBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-004");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckResult(Rect rect, int area, double whratio)
	{
		try
		{
			int selectFaceIndex = SelectFaceIndex;
			bool[] array = new bool[4];
			bool flag = true;
			array[0] = GetRule((int)pfile.SelectFaceParam[selectFaceIndex].InspRule1_Index, pfile.SelectFaceParam[selectFaceIndex].InspRule1_Min, pfile.SelectFaceParam[selectFaceIndex].InspRule1_Max, rect, area);
			array[1] = GetRule((int)pfile.SelectFaceParam[selectFaceIndex].InspRule2_Index, pfile.SelectFaceParam[selectFaceIndex].InspRule2_Min, pfile.SelectFaceParam[selectFaceIndex].InspRule2_Max, rect, area);
			array[2] = GetRule((int)pfile.SelectFaceParam[selectFaceIndex].InspRule3_Index, pfile.SelectFaceParam[selectFaceIndex].InspRule3_Min, pfile.SelectFaceParam[selectFaceIndex].InspRule3_Max, rect, area);
			array[3] = GetRule((int)pfile.SelectFaceParam[selectFaceIndex].InspRule4_Index, pfile.SelectFaceParam[selectFaceIndex].InspRule4_Min, pfile.SelectFaceParam[selectFaceIndex].InspRule4_Max, rect, area);
			if (pfile.SelectFaceParam[selectFaceIndex].InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.SelectFaceParam[selectFaceIndex].InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.SelectFaceParam[selectFaceIndex].InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.SelectFaceParam[selectFaceIndex].InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.SelectFaceParam[selectFaceIndex].InspRule4_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
			}
			Blobs.result = flag;
			if (area == 0 || rect.Width <= 0 || rect.Height <= 0)
			{
				Blobs.AreaRatio = 0.0;
			}
			else
			{
				Blobs.AreaRatio = (double)area / (double)(rect.Width * rect.Height);
			}
			string text = SelectFaceIndex switch
			{
				0 => "A", 
				1 => "B", 
				2 => "C", 
				3 => "D", 
				_ => "A", 
			};
			if (pfile.MeasureUnit == 0)
			{
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}面] 寬:{1:N3}um, 高:{2:N3}um, 面積:{3:N3}um, 長寬比:{4:N3}, 面積比:{5:N3} 所有面積:{6:N3}um \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}面] 宽:{1:N3}um, 高:{2:N3}um, 面积:{3:N3}um, 长宽比:{4:N3}, 面积比:{5:N3} 所有面积:{6:N3}um \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}面] 幅:{1:N3}um, 高:{2:N3}um, 面積:{3:N3}um, アスペクト比:{4:N3}, 面積比:{5:N3}, 全面積:{6:N3}um \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
					else
					{
						Tracker1ResultText = string.Format("[FACE {0}] Width:{1:N3}um, Height:{2:N3}um, Area:{3:N3}um, WH Ratio:{4:N3}, Area Ratio:{5:N3}, All Area:{6:N3}um \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}面] 寬:{1}um, 高:{2}um, 面積:{3}um, 長寬比:{4:N3}, 面積比:{5:N3} 所有面積:{6}um \n", text, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}面] 宽:{1}um, 高:{2}um, 面积:{3}um, 长宽比:{4:N3}, 面积比:{5:N3} 所有面积:{6}um \n", text, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}面] 幅:{1}um, 高:{2}um, 面積:{3}um, アスペクト比:{4:N3}, 面積比:{5:N3} 全ての面積:{6}um \n", text, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
				else
				{
					Tracker1ResultText = string.Format("[FACE {0}] Width:{1}um, Height:{2}um, Area:{3}um, WH Ratio:{4:N3}, Area Ratio:{5:N3}, All Area:{6}um \n", text, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker1ResultText = string.Format("[{0}面] 寬:{1:N3}mm, 高:{2:N3}mm, 面積:{3:N3}mm, 長寬比:{4:N3}, 面積比:{5:N3} 所有面積:{6:N3}mm \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText = string.Format("[{0}面] 宽:{1:N3}mm, 高:{2:N3}mm, 面积:{3:N3}mm, 长宽比:{4:N3}, 面积比:{5:N3} 所有面积:{6:N3}mm \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText = string.Format("[{0}面] 幅:{1:N3}mm, 高さ:{2:N3}mm, 面積:{3:N3}mm, アスペクト比:{4:N3}, 面積比:{5:N3} 全面積:{6:N3}mm \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			else
			{
				Tracker1ResultText = string.Format("[FACE {0}] Width:{1:N3}mm, Height:{2:N3}mm, Area:{3:N3}mm, WH Ratio:{4:N3}, Area Ratio:{5:N3}, All Area:{6:N3}mm \n", text, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text2 = "BT03-SelectFaceTool-CheckResult()-";
			text2 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-005");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, double min, double max, Rect rect, double area)
	{
		try
		{
			bool result = true;
			switch (rule)
			{
			case 0:
				return true;
			case 1:
			{
				double num = (double)rect.Width * pfile.PixelPerUM_X;
				if (num < min || num > max)
				{
					result = false;
				}
				break;
			}
			case 2:
			{
				double num4 = (double)rect.Height * pfile.PixelPerUM_Y;
				if (num4 < min || num4 > max)
				{
					result = false;
				}
				break;
			}
			case 3:
			{
				double num2 = area * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num2 < min || num2 > max)
				{
					result = false;
				}
				break;
			}
			case 4:
			{
				double num3 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				if (num3 < min || num3 > max)
				{
					result = false;
				}
				break;
			}
			case 5:
			{
				double areaRatio = 0.0;
				Blobs.AreaRatio = areaRatio;
				if ((double)rect.Width * (double)rect.Height <= 0.0)
				{
					Blobs.AreaRatio = areaRatio;
					result = true;
				}
				else
				{
					areaRatio = area / (double)(rect.Width * rect.Height);
					Blobs.AreaRatio = areaRatio;
					result = ((!(areaRatio < min) && !(areaRatio > max)) ? true : false);
				}
				break;
			}
			case 6:
				AllArea = (double)FinalBinary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				result = ((!(AllArea < min) && !(AllArea > max)) ? true : false);
				break;
			default:
				return true;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT04-SelectFaceTool-GetRule()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-006");
			}
			return false;
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
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-007");
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
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectFace-008");
			}
			return false;
		}
	}
}
