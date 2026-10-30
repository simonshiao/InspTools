using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class EnhanceTool
{
	private CVBuffer buffers;

	private Rect LocateRect;

	private ParamFile pfile;

	private Stopwatch sw = new Stopwatch();

	private Mat[] LocatedLAB = new Mat[3];

	private Mat[] LocatedLUV = new Mat[3];

	private Mat lab = new Mat();

	private Mat luv = new Mat();

	private Mat Binary = new Mat();

	private Mat BinaryTemp = new Mat();

	private Mat lookUpTable = new Mat();

	private bool OffLine = true;

	public Rect LocateImageRect { get; set; }

	public bool DoLABFinish { get; set; }

	public bool DoLUVFinish { get; set; }

	public EnhanceTool(ParamFile paramfile)
	{
		pfile = paramfile;
		for (int i = 0; i < 3; i++)
		{
			LocatedLAB[i] = new Mat();
			LocatedLUV[i] = new Mat();
		}
		DoLABFinish = false;
		DoLUVFinish = false;
		lookUpTable = new Mat(1, 256, MatType.CV_8UC1);
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

	public bool Inspect(int index, bool offline = true)
	{
		OffLine = offline;
		switch (pfile.EnhanceParam[index].FunctionIndex)
		{
		case 0:
			return true;
		case 1:
			DoBrightnessAndContrast(index);
			return true;
		case 2:
			DoSmoothSharpen(index);
			return true;
		case 3:
			DoEdge(index);
			return true;
		case 4:
			DoColorMix(index);
			return true;
		case 5:
			DoCustom(index);
			return true;
		case 6:
			DoLAB(index);
			return true;
		case 7:
			DoLUV(index);
			return true;
		case 8:
			DoBlockAvg(index);
			return true;
		case 9:
			DoBrightness(index);
			return true;
		case 10:
			DoBinary(index);
			return true;
		default:
			return true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe void DoBrightnessAndContrast(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Mat channelMatNoAI = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex);
			double num = 0.0;
			num = iNSP_ENHANCE_PARAM.BrightnessIndex switch
			{
				0 => 1.5, 
				1 => 1.25, 
				2 => 1.0, 
				3 => 0.75, 
				4 => 0.5, 
				5 => 0.25, 
				_ => 1.0, 
			};
			int num2 = 0;
			channelMatNoAI.ConvertTo(beta: iNSP_ENHANCE_PARAM.ContrastIndex switch
			{
				0 => 30, 
				1 => 20, 
				2 => 10, 
				3 => 0, 
				4 => -10, 
				5 => -20, 
				6 => -30, 
				_ => 10, 
			}, m: locatedCustomMat, rtype: MatType.CV_8UC1, alpha: num);
			double num3 = 1.0;
			num3 = iNSP_ENHANCE_PARAM.GammaIndex switch
			{
				0 => 5.0, 
				1 => 2.5, 
				2 => 1.5, 
				3 => 1.0, 
				4 => 0.67, 
				5 => 0.4, 
				6 => 0.2, 
				_ => 1.0, 
			};
			if (iNSP_ENHANCE_PARAM.GammaIndex != 3)
			{
				byte* ptr = (byte*)lookUpTable.Data.ToPointer();
				for (int i = 0; i < 256; i++)
				{
					ptr[i] = (byte)(Math.Pow((double)i / 255.0, num3) * 255.0);
				}
				Cv2.LUT(locatedCustomMat, lookUpTable, locatedCustomMat);
			}
		}
		catch (Exception ex)
		{
			string text = "ET01-Enhance-DoBrightnessAndContrast()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-001");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoBrightness(int index)
	{
		try
		{
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex).ConvertTo(alpha: iNSP_ENHANCE_PARAM.BrightnessValue, beta: iNSP_ENHANCE_PARAM.ContrastValue, m: locatedCustomMat, rtype: MatType.CV_8UC1);
		}
		catch (Exception ex)
		{
			string text = "ET01-Enhance-DoBrightness()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-002");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoSmoothSharpen(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Mat channelMatNoAI = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex);
			if (iNSP_ENHANCE_PARAM.SSIndex == 0)
			{
				float[,] data = new float[5, 5]
				{
					{ -0.125f, -0.125f, -0.125f, -0.125f, -0.125f },
					{ -0.125f, 0.25f, 0.25f, 0.25f, -0.125f },
					{ -0.125f, 0.25f, 1f, 0.25f, -0.125f },
					{ -0.125f, 0.25f, 0.25f, 0.25f, -0.125f },
					{ -0.125f, -0.125f, -0.125f, -0.125f, -0.125f }
				};
				Mat mat = new Mat(5, 5, MatType.CV_32FC1, data, 0L);
				Cv2.Filter2D(channelMatNoAI, locatedCustomMat, MatType.CV_8UC1, mat);
			}
			else if (iNSP_ENHANCE_PARAM.SSIndex == 1)
			{
				float[,] data2 = new float[3, 3]
				{
					{ -1f, -1f, -1f },
					{ -1f, 9f, -1f },
					{ -1f, -1f, -1f }
				};
				Mat mat2 = new Mat(3, 3, MatType.CV_32FC1, data2, 0L);
				Cv2.Filter2D(channelMatNoAI, locatedCustomMat, MatType.CV_8UC1, mat2);
			}
			else if (iNSP_ENHANCE_PARAM.SSIndex == 2)
			{
				Cv2.GaussianBlur(channelMatNoAI, locatedCustomMat, new Size(3, 3), 0.0);
			}
			else if (iNSP_ENHANCE_PARAM.SSIndex == 3)
			{
				Cv2.GaussianBlur(channelMatNoAI, locatedCustomMat, new Size(5, 5), 0.0);
			}
		}
		catch (Exception ex)
		{
			string text = "ET02-Enhance-DoSmoothSharpen()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-003");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoBinary(int index)
	{
		try
		{
			LocateRect = buffers.LocatedRect;
			Mat mat = new Mat(buffers.GetLocatedCustomMat(index), LocateRect);
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Cv2.InRange(new Mat(new Mat(buffers.Dst, buffers.LocateImageRect), LocateRect), new Scalar(iNSP_ENHANCE_PARAM.Thd_R_Min, iNSP_ENHANCE_PARAM.Thd_G_Min, iNSP_ENHANCE_PARAM.Thd_B_Min), new Scalar(iNSP_ENHANCE_PARAM.Thd_R_Max, iNSP_ENHANCE_PARAM.Thd_G_Max, iNSP_ENHANCE_PARAM.Thd_B_Max), mat);
		}
		catch (Exception ex)
		{
			string text = "ET03-Enhance-DoBinary()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-004");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoEdge(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Mat channelMatNoAI = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex);
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 0)
			{
				float[,] data = new float[5, 5]
				{
					{ 0f, 0f, -1f, 0f, 0f },
					{ 0f, -1f, -2f, -1f, 0f },
					{ -1f, -2f, 16f, -2f, -1f },
					{ 0f, -1f, -2f, -1f, 0f },
					{ 0f, 0f, -1f, 0f, 0f }
				};
				Mat mat = new Mat(5, 5, MatType.CV_32FC1, data, 0L);
				Cv2.Filter2D(channelMatNoAI, locatedCustomMat, MatType.CV_8UC1, mat);
				return;
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 1)
			{
				float[,] data2 = new float[3, 3]
				{
					{ 1f, 1f, 1f },
					{ 1f, -8f, 1f },
					{ 1f, 1f, 1f }
				};
				Mat mat2 = new Mat(3, 3, MatType.CV_32FC1, data2, 0L);
				Cv2.Filter2D(channelMatNoAI, locatedCustomMat, MatType.CV_8UC1, mat2);
				return;
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 2 || iNSP_ENHANCE_PARAM.EdgeIndex == 3 || iNSP_ENHANCE_PARAM.EdgeIndex == 4)
			{
				using (CLAHE cLAHE = Cv2.CreateCLAHE())
				{
					if (iNSP_ENHANCE_PARAM.EdgeIndex == 2)
					{
						cLAHE.ClipLimit = 2.0;
						cLAHE.Apply(channelMatNoAI, locatedCustomMat);
					}
					else if (iNSP_ENHANCE_PARAM.EdgeIndex == 3)
					{
						cLAHE.ClipLimit = 5.0;
						cLAHE.Apply(channelMatNoAI, locatedCustomMat);
					}
					else
					{
						cLAHE.ClipLimit = 10.0;
						cLAHE.Apply(channelMatNoAI, locatedCustomMat);
					}
					return;
				}
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 5)
			{
				Cv2.Canny(channelMatNoAI, locatedCustomMat, 0.0, 100.0);
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 6)
			{
				Cv2.Canny(channelMatNoAI, locatedCustomMat, 50.0, 100.0);
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 7)
			{
				Cv2.Canny(channelMatNoAI, locatedCustomMat, 100.0, 200.0);
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 8)
			{
				double num = (double)channelMatNoAI.Mean();
				double num2 = 0.33;
				int num3 = Math.Max(0, (int)((1.0 - num2) * num));
				int num4 = Math.Min(255, (int)((1.0 + num2) * num));
				Cv2.Canny(channelMatNoAI, locatedCustomMat, num3, num4);
			}
			if (iNSP_ENHANCE_PARAM.EdgeIndex == 9)
			{
				Cv2.Canny(channelMatNoAI, locatedCustomMat, iNSP_ENHANCE_PARAM.CannyThd1, iNSP_ENHANCE_PARAM.CannyThd2);
			}
		}
		catch (Exception ex)
		{
			string text = "ET03-Enhance-DoEdge()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-005");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoColorMix(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Mat channelMatNoAI = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex);
			Mat channelMatNoAI2 = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.MixedIndex);
			if (!iNSP_ENHANCE_PARAM.UseSubtract)
			{
				Cv2.AddWeighted(channelMatNoAI, 0.5, channelMatNoAI2, 0.5, 0.0, locatedCustomMat);
			}
			else
			{
				Cv2.Subtract(channelMatNoAI, channelMatNoAI2, locatedCustomMat);
			}
		}
		catch (Exception ex)
		{
			string text = "ET04-Enhance-DoColorMix()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-006");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoCustom(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
			Mat channelMatNoAI = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex);
			float[,] data = new float[3, 3]
			{
				{
					(float)iNSP_ENHANCE_PARAM.Custom1,
					(float)iNSP_ENHANCE_PARAM.Custom2,
					(float)iNSP_ENHANCE_PARAM.Custom3
				},
				{
					(float)iNSP_ENHANCE_PARAM.Custom4,
					(float)iNSP_ENHANCE_PARAM.Custom5,
					(float)iNSP_ENHANCE_PARAM.Custom6
				},
				{
					(float)iNSP_ENHANCE_PARAM.Custom7,
					(float)iNSP_ENHANCE_PARAM.Custom8,
					(float)iNSP_ENHANCE_PARAM.Custom9
				}
			};
			Mat mat = new Mat(3, 3, MatType.CV_32FC1, data, 0L);
			Cv2.Filter2D(channelMatNoAI, locatedCustomMat, MatType.CV_8UC1, mat);
		}
		catch (Exception ex)
		{
			string text = "ET05-Enhance-DoColorMix()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-007");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoLAB(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			GetLocatedLAB(pfile.EnhanceParam[index].ChannelIndex).CopyTo(locatedCustomMat);
		}
		catch (Exception ex)
		{
			string text = "ET05-Enhance-DoLAB()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-008");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void DoLUV(int index)
	{
		try
		{
			Mat locatedCustomMat = buffers.GetLocatedCustomMat(index);
			GetLocatedLUV(pfile.EnhanceParam[index].ChannelIndex).CopyTo(locatedCustomMat);
		}
		catch (Exception ex)
		{
			string text = "ET05-Enhance-DoLUV()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Enhance-009");
			}
		}
	}

	public void DoBlockAvg(int index)
	{
		INSP_ENHANCE_PARAM iNSP_ENHANCE_PARAM = pfile.EnhanceParam[index];
		Mat channelMatNoAI = buffers.GetChannelMatNoAI(iNSP_ENHANCE_PARAM.ChannelIndex);
		Mat mat = buffers.GetLocatedCustomMat(index);
		Cv2.Blur(channelMatNoAI, Binary, new Size(15, 15));
		if (iNSP_ENHANCE_PARAM.BlockAvgFun == 0)
		{
			Cv2.Subtract(channelMatNoAI, Binary, mat);
			Cv2.InRange(mat, iNSP_ENHANCE_PARAM.BlockAvgThd, 255.0, mat);
		}
		else if (iNSP_ENHANCE_PARAM.BlockAvgFun == 1)
		{
			Cv2.Subtract(Binary, channelMatNoAI, mat);
			Cv2.InRange(mat, iNSP_ENHANCE_PARAM.BlockAvgThd, 255.0, mat);
		}
		else
		{
			Cv2.Subtract(channelMatNoAI, Binary, mat);
			Cv2.InRange(mat, iNSP_ENHANCE_PARAM.BlockAvgThd, 255.0, mat);
			Cv2.Subtract(Binary, channelMatNoAI, BinaryTemp);
			Cv2.InRange(BinaryTemp, iNSP_ENHANCE_PARAM.BlockAvgThd, 255.0, BinaryTemp);
			Cv2.BitwiseOr(mat, BinaryTemp, mat);
		}
		if (iNSP_ENHANCE_PARAM.MorphType > 0 && iNSP_ENHANCE_PARAM.MorphCount > 0)
		{
			DoMorph(ref mat, iNSP_ENHANCE_PARAM.MorphType, iNSP_ENHANCE_PARAM.MorphCount);
		}
	}

	public Mat GetLocatedLAB(int index)
	{
		if (!DoLABFinish)
		{
			Cv2.CvtColor(buffers.LocatedSrc, lab, ColorConversionCodes.BGR2Lab);
			Cv2.Split(lab, out LocatedLAB);
			DoLABFinish = true;
		}
		return LocatedLAB[index];
	}

	public Mat GetLocatedLUV(int index)
	{
		if (!DoLUVFinish)
		{
			Cv2.CvtColor(buffers.LocatedSrc, luv, ColorConversionCodes.BGR2Luv);
			Cv2.Split(luv, out LocatedLUV);
			DoLUVFinish = true;
		}
		return LocatedLUV[index];
	}

	private void DoMorph(ref Mat mat, int index, int count)
	{
		if (count != 0 || index == 10)
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
}
