using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.AOI;

public class CVBuffer
{
	public const int MaxTools = 20;

	public const int MaxBinaryTools = 30;

	public const int MaxParetoItems = 30;

	public const int MaxMeasureTools = 40;

	public const int MaxMaskTools = 30;

	public const int MaxComDigitsItems = 24;

	public const int MaxFace = 4;

	public const int MaxEnhance = 6;

	public const int InspToolsCount = 18;

	public const int MaxInspSurvItems = 20;

	public const double ComDigitsMask = -98980.0;

	public const int MaxStation = 8;

	private Mat BayerMat = new Mat();

	private Mat BitmapMat = new Mat();

	private Mat[] SrcBGR = new Mat[3];

	private Mat LocateC1 = new Mat();

	private Mat LocateC2 = new Mat();

	private Mat LocateC3 = new Mat();

	private Mat LocateC4 = new Mat();

	private Mat LocateC5 = new Mat();

	private Mat LocateC6 = new Mat();

	private Mat LocatedAIR = new Mat();

	private Mat LocatedAIG = new Mat();

	private Mat LocatedAIB = new Mat();

	private bool LocateAICreated;

	private Mat RotatedLocateAIR = new Mat();

	private Mat RotatedLocateAIG = new Mat();

	private Mat RotatedLocateAIB = new Mat();

	private Mat OrgLocateAIR = new Mat();

	private Mat OrgLocateAIG = new Mat();

	private Mat OrgLocateAIB = new Mat();

	private Mat BeforeLocateAITemp = new Mat();

	private Mat BeforeRotateAITemp = new Mat();

	private Mat LocateAIEnhanceTemp = new Mat();

	private Mat LocateAIEnhanceColor = new Mat();

	private Mat LocateAIEnhanceColorTemp = new Mat();

	private Mat[] LocatedHSV = new Mat[3];

	private Mat[] LocatedBGR = new Mat[3];

	private Mat Fillet0 = new Mat();

	private Mat[] Fillet5 = new Mat[4];

	private Mat[] Fillet10 = new Mat[4];

	private Mat[] Fillet15 = new Mat[4];

	private Mat[] Fillet20 = new Mat[4];

	private Mat[] Fillet25 = new Mat[4];

	private Mat[] Fillet30 = new Mat[4];

	private Mat[] Fillet40 = new Mat[4];

	private Mat[] Fillet50 = new Mat[4];

	private Mat[] Fillet60 = new Mat[4];

	public Mat roihsv = new Mat();

	private static bool Saveing = false;

	private static string LastMsg = "";

	private Stopwatch sw = new Stopwatch();

	public Mat Src { get; set; }

	public Mat Bayer { get; set; }

	public Mat Dst { get; set; }

	public Mat Gray { get; set; }

	public Mat Binary { get; set; }

	public Rect SrcRect { get; set; }

	public Mat LocatedSrc { get; set; }

	public Mat LocatedSrcTmp { get; set; }

	public Mat LocatedBinary { get; set; }

	public Mat LocateGray { get; set; }

	public bool SrcReady { get; protected set; }

	public bool LocatedReady { get; protected set; }

	public bool IsColor { get; protected set; }

	public bool UseLABColor { get; set; }

	public Rect LocatedRect { get; set; }

	public Rect LocatedRectTmp { get; set; }

	public Rect LocateImageRect { get; set; }

	public int LanguageType { get; set; }

	public CVBuffer()
	{
		SrcReady = false;
		LocatedReady = false;
		IsColor = true;
		Src = new Mat();
		Bayer = new Mat();
		Dst = new Mat();
		Gray = new Mat();
		Binary = new Mat();
		LocatedSrc = new Mat();
		LocatedBinary = new Mat();
		LocatedSrcTmp = new Mat();
		LocateGray = new Mat();
		LocateC1 = new Mat();
		LocateC2 = new Mat();
		LocateC3 = new Mat();
		LocateC4 = new Mat();
		LocateC5 = new Mat();
		LocateC6 = new Mat();
		LocatedAIR = new Mat();
		LocatedAIG = new Mat();
		LocatedAIB = new Mat();
		RotatedLocateAIR = new Mat();
		RotatedLocateAIB = new Mat();
		RotatedLocateAIB = new Mat();
		OrgLocateAIR = new Mat();
		OrgLocateAIG = new Mat();
		OrgLocateAIB = new Mat();
		UseLABColor = false;
		for (int i = 0; i < 3; i++)
		{
			SrcBGR[i] = new Mat();
			LocatedHSV[i] = new Mat();
			LocatedBGR[i] = new Mat();
		}
		LanguageType = 1;
		for (int j = 0; j < 4; j++)
		{
			Fillet5[j] = new Mat();
			Fillet10[j] = new Mat();
			Fillet15[j] = new Mat();
			Fillet20[j] = new Mat();
			Fillet25[j] = new Mat();
			Fillet30[j] = new Mat();
			Fillet40[j] = new Mat();
			Fillet50[j] = new Mat();
			Fillet60[j] = new Mat();
		}
		CreatFillet();
	}

	public bool SetBuffer(Mat mat)
	{
		if (mat.Empty())
		{
			return false;
		}
		mat.CopyTo(Src);
		mat.CopyTo(Dst);
		SrcRect = new Rect(0, 0, Src.Width, Src.Height);
		SrcReady = true;
		LocatedReady = false;
		if (mat.Channels() == 3 || mat.Channels() == 4)
		{
			Cv2.CvtColor(Src, Gray, ColorConversionCodes.BGR2GRAY);
			Cv2.Split(Src, out SrcBGR);
		}
		else if (mat.Channels() == 1)
		{
			IsColor = false;
			Src.CopyTo(Gray);
			for (int i = 0; i < 3; i++)
			{
				SrcBGR[i] = Gray;
			}
		}
		else
		{
			SrcReady = false;
		}
		if (RotatedLocateAIR.Width == 0 || RotatedLocateAIR.Height == 0 || RotatedLocateAIR.Width != Gray.Width || RotatedLocateAIR.Height != Gray.Height)
		{
			RotatedLocateAIR = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIR.SetTo(0.0);
			RotatedLocateAIG = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIG.SetTo(0.0);
			RotatedLocateAIB = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIB.SetTo(0.0);
			OrgLocateAIR = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIR.SetTo(0.0);
			OrgLocateAIG = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIG.SetTo(0.0);
			OrgLocateAIB = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIB.SetTo(0.0);
		}
		return SrcReady;
	}

	public bool SetBuffer(Bitmap bitmap)
	{
		SrcReady = true;
		LocatedReady = false;
		BitmapMat = Bitmap2Mat(bitmap);
		BitmapMat.CopyTo(Src);
		BitmapMat.CopyTo(Dst);
		SrcRect = new Rect(0, 0, Src.Width, Src.Height);
		SrcReady = true;
		LocatedReady = false;
		if (BitmapMat.Channels() == 3 || BitmapMat.Channels() == 4)
		{
			Cv2.CvtColor(Src, Gray, ColorConversionCodes.BGR2GRAY);
			Cv2.Split(Src, out SrcBGR);
		}
		else if (BitmapMat.Channels() == 1)
		{
			IsColor = false;
			Gray = MM.CloneFrom(Src, Gray);
		}
		else
		{
			SrcReady = false;
		}
		if (RotatedLocateAIR.Width == 0 || RotatedLocateAIR.Height == 0 || RotatedLocateAIR.Width != Gray.Width || RotatedLocateAIR.Height != Gray.Height)
		{
			RotatedLocateAIR = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIR.SetTo(0.0);
			RotatedLocateAIG = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIG.SetTo(0.0);
			RotatedLocateAIB = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIB.SetTo(0.0);
			OrgLocateAIR = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIR.SetTo(0.0);
			OrgLocateAIG = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIG.SetTo(0.0);
			OrgLocateAIB = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIB.SetTo(0.0);
		}
		return SrcReady;
	}

	public bool SetBuffer(byte[] bayerBuffer, int w, int h, int band)
	{
		switch (band)
		{
		case 3:
		{
			using (Mat mat = new Mat(h, w, MatType.CV_8UC1, bayerBuffer, 0L))
			{
				Cv2.CvtColor(mat, BayerMat, ColorConversionCodes.BayerBG2BGR);
				BayerMat.CopyTo(Src);
				BayerMat.CopyTo(Dst);
				SrcRect = new Rect(0, 0, Src.Width, Src.Height);
				SrcReady = true;
				LocatedReady = false;
				Cv2.CvtColor(Src, Gray, ColorConversionCodes.BGR2GRAY);
				Cv2.Split(Src, out SrcBGR);
			}
			break;
		}
		case 1:
		{
			Src = new Mat(h, w, MatType.CV_8UC1, bayerBuffer, 0L);
			Src.CopyTo(Gray);
			SrcReady = true;
			LocatedReady = false;
			IsColor = false;
			for (int i = 0; i < 3; i++)
			{
				SrcBGR[i] = Gray;
			}
			break;
		}
		default:
			SrcReady = false;
			break;
		}
		if (RotatedLocateAIR.Width == 0 || RotatedLocateAIR.Height == 0 || RotatedLocateAIR.Width != Gray.Width || RotatedLocateAIR.Height != Gray.Height)
		{
			RotatedLocateAIR = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIR.SetTo(0.0);
			RotatedLocateAIG = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIG.SetTo(0.0);
			RotatedLocateAIB = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			RotatedLocateAIB.SetTo(0.0);
			OrgLocateAIR = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIR.SetTo(0.0);
			OrgLocateAIG = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIG.SetTo(0.0);
			OrgLocateAIB = new Mat(Gray.Rows, Gray.Cols, MatType.CV_8UC1);
			OrgLocateAIB.SetTo(0.0);
		}
		return SrcReady;
	}

	public bool SetLocatedBuffer(Mat mat, int[] EnhanceFunIndex, bool UseAIEnhance = false, bool CreateHSVChannel = true, bool OffLine = false)
	{
		LocatedReady = true;
		mat.CopyTo(LocatedSrc);
		if (mat.Channels() == 3)
		{
			Cv2.Split(LocatedSrc, out LocatedBGR);
			Cv2.CvtColor(LocatedSrc, LocateGray, ColorConversionCodes.BGR2GRAY);
			if (CreateHSVChannel)
			{
				Cv2.CvtColor(LocatedSrc, roihsv, ColorConversionCodes.BGR2HSV);
				Cv2.Split(roihsv, out LocatedHSV);
			}
			else if (OffLine)
			{
				LocatedBGR[0].CopyTo(LocatedHSV[0]);
				LocatedBGR[1].CopyTo(LocatedHSV[1]);
				LocatedBGR[2].CopyTo(LocatedHSV[2]);
			}
			else
			{
				LocatedHSV[0] = LocatedBGR[0];
				LocatedHSV[1] = LocatedBGR[1];
				LocatedHSV[2] = LocatedBGR[2];
			}
			if (OffLine)
			{
				LocateGray.CopyTo(LocateC1);
				LocateGray.CopyTo(LocateC2);
				LocateGray.CopyTo(LocateC3);
				LocateGray.CopyTo(LocateC4);
				LocateGray.CopyTo(LocateC5);
				LocateGray.CopyTo(LocateC6);
			}
			else
			{
				if (EnhanceFunIndex[0] != 0)
				{
					if (LocateGray.Width != LocateC1.Width || LocateGray.Height != LocateC1.Height)
					{
						LocateC1 = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
					}
				}
				else
				{
					LocateC1 = LocateGray;
				}
				if (EnhanceFunIndex[1] != 0)
				{
					if (LocateGray.Width != LocateC2.Width || LocateGray.Height != LocateC2.Height)
					{
						LocateC2 = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
					}
				}
				else
				{
					LocateC2 = LocateGray;
				}
				if (EnhanceFunIndex[2] != 0)
				{
					if (LocateGray.Width != LocateC3.Width || LocateGray.Height != LocateC3.Height)
					{
						LocateC3 = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
					}
				}
				else
				{
					LocateC3 = LocateGray;
				}
				if (EnhanceFunIndex[3] != 0)
				{
					if (LocateGray.Width != LocateC4.Width || LocateGray.Height != LocateC4.Height)
					{
						LocateC4 = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
					}
				}
				else
				{
					LocateC4 = LocateGray;
				}
				if (EnhanceFunIndex[4] != 0)
				{
					if (LocateGray.Width != LocateC5.Width || LocateGray.Height != LocateC5.Height)
					{
						LocateC5 = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
					}
				}
				else
				{
					LocateC5 = LocateGray;
				}
				if (EnhanceFunIndex[5] != 0)
				{
					if (LocateGray.Width != LocateC6.Width || LocateGray.Height != LocateC6.Height)
					{
						LocateC6 = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
					}
				}
				else
				{
					LocateC6 = LocateGray;
				}
			}
			if (LocateGray.Width != LocatedAIR.Width || LocateGray.Height != LocatedAIR.Height)
			{
				LocatedAIR = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
			}
			if (LocateGray.Width != LocatedAIG.Width || LocateGray.Height != LocatedAIG.Height)
			{
				LocatedAIG = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
			}
			if (LocateGray.Width != LocatedAIB.Width || LocateGray.Height != LocatedAIB.Height)
			{
				LocatedAIB = new Mat(LocateGray.Rows, LocateGray.Cols, MatType.CV_8UC1);
			}
			LocateAICreated = true;
			LocatedAIR.SetTo(0.0);
			LocatedAIG.SetTo(0.0);
			LocatedAIB.SetTo(0.0);
		}
		else if (mat.Channels() == 1)
		{
			IsColor = false;
			LocatedSrc.CopyTo(LocateGray);
			LocateGray.CopyTo(LocateC1);
			LocateGray.CopyTo(LocateC2);
			LocateGray.CopyTo(LocateC3);
			LocateGray.CopyTo(LocateC4);
			LocateGray.CopyTo(LocateC5);
			LocateGray.CopyTo(LocateC6);
			if (!UseAIEnhance)
			{
				LocatedAIR = LocateGray;
				LocatedAIG = LocateGray;
				LocatedAIB = LocateGray;
			}
			else
			{
				LocateGray.CopyTo(LocatedAIR);
				LocatedAIR.SetTo(0.0);
				LocatedAIR.CopyTo(LocatedAIG);
				LocatedAIR.CopyTo(LocatedAIB);
			}
			Cv2.CvtColor(mat, roihsv, ColorConversionCodes.GRAY2BGR);
			Cv2.CvtColor(roihsv, roihsv, ColorConversionCodes.BGR2HSV);
			for (int i = 0; i < 3; i++)
			{
				LocatedSrc.CopyTo(LocatedHSV[i]);
				LocatedSrc.CopyTo(LocatedBGR[i]);
			}
		}
		else
		{
			LocatedReady = false;
		}
		return LocatedReady;
	}

	public Mat GetSrcBGR(int index)
	{
		return index switch
		{
			0 => SrcBGR[2], 
			1 => SrcBGR[1], 
			2 => SrcBGR[0], 
			3 => OrgLocateAIR, 
			4 => OrgLocateAIG, 
			5 => OrgLocateAIB, 
			_ => SrcBGR[2], 
		};
	}

	public Mat GetLocatedHSV(int index)
	{
		return LocatedHSV[index];
	}

	public Mat GetLocatedBGR(int index)
	{
		return index switch
		{
			0 => LocatedBGR[2], 
			1 => LocatedBGR[1], 
			2 => LocatedBGR[0], 
			_ => LocatedBGR[2], 
		};
	}

	public Mat GetLocatedCustomMat(int index)
	{
		return index switch
		{
			0 => LocateC1, 
			1 => LocateC2, 
			2 => LocateC3, 
			3 => LocateC4, 
			4 => LocateC5, 
			5 => LocateC6, 
			_ => LocateC1, 
		};
	}

	public void SetORGLcateAIMat(Mat matR, Mat matG, Mat matB, Rect rect)
	{
		OrgLocateAIR.SetTo(0.0);
		OrgLocateAIG.SetTo(0.0);
		OrgLocateAIB.SetTo(0.0);
		BeforeRotateAITemp = new Mat(OrgLocateAIR, rect);
		matR.CopyTo(BeforeRotateAITemp);
		BeforeRotateAITemp = new Mat(OrgLocateAIG, rect);
		matG.CopyTo(BeforeRotateAITemp);
		BeforeRotateAITemp = new Mat(OrgLocateAIB, rect);
		matB.CopyTo(BeforeRotateAITemp);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetAiEnhanceMat(Mat matR, Mat matG, Mat matB, Rect airect)
	{
		try
		{
			if (airect.Width > 0 && airect.Height > 0)
			{
				Rect rect = new Rect(0, 0, LocatedAIR.Width, LocatedAIR.Height);
				airect &= rect;
				LocateAIEnhanceTemp = new Mat(LocatedAIR, airect);
				matR.CopyTo(LocateAIEnhanceTemp);
				LocateAIEnhanceTemp = new Mat(LocatedAIG, airect);
				matG.CopyTo(LocateAIEnhanceTemp);
				LocateAIEnhanceTemp = new Mat(LocatedAIB, airect);
				matB.CopyTo(LocateAIEnhanceTemp);
			}
		}
		catch (Exception ex)
		{
			SaveLog(string.Concat(string.Concat(string.Concat("CVBuffer-SetAiEnhanceMat()" + string.Format("Airect w={0}, h = {1}", airect.Width, airect.Height), string.Format(" LocatedAIR w={0}, h = {1}", LocatedAIR.Width, LocatedAIR.Height)), string.Format(" matR w={0}, h = {1}", matR.Width, matR.Height)), ex.ToString()));
		}
	}

	public void ReSetAiEnhanceMatColor()
	{
		LocatedSrc.CopyTo(LocateAIEnhanceColor);
	}

	public void SetAiEnhanceMatColor(Mat mat, Rect airect)
	{
		LocatedSrc.CopyTo(LocateAIEnhanceColor);
		LocateAIEnhanceColorTemp = new Mat(LocateAIEnhanceColor, airect);
		mat.CopyTo(LocateAIEnhanceColorTemp);
	}

	public Mat GetEnhanceAiMat(int channel)
	{
		return channel switch
		{
			0 => LocatedAIR, 
			1 => LocatedAIG, 
			2 => LocatedAIB, 
			_ => LocatedAIG, 
		};
	}

	public Mat GetEnhanceAiMatColor()
	{
		return LocateAIEnhanceColor;
	}

	public Mat GetChannelMatNoAI(int index)
	{
		Mat mat = new Mat();
		return index switch
		{
			0 => GetLocatedBGR(0), 
			1 => GetLocatedBGR(1), 
			2 => GetLocatedBGR(2), 
			3 => GetLocatedHSV(0), 
			4 => GetLocatedHSV(1), 
			5 => GetLocatedHSV(2), 
			6 => LocateGray, 
			7 => GetLocatedCustomMat(0), 
			8 => GetLocatedCustomMat(1), 
			9 => GetLocatedCustomMat(2), 
			10 => GetLocatedCustomMat(3), 
			11 => GetLocatedCustomMat(4), 
			12 => GetLocatedCustomMat(5), 
			_ => GetLocatedBGR(1), 
		};
	}

	public Mat GetAILocateMat(int index)
	{
		Mat result = new Mat();
		switch (index)
		{
		case 21:
			result = RotatedLocateAIR;
			break;
		case 22:
			result = RotatedLocateAIG;
			break;
		case 23:
			result = RotatedLocateAIB;
			break;
		}
		return result;
	}

	public Mat GetChannelMat(int index)
	{
		Mat mat = new Mat();
		return index switch
		{
			0 => GetLocatedBGR(0), 
			1 => GetLocatedBGR(1), 
			2 => GetLocatedBGR(2), 
			3 => GetLocatedHSV(0), 
			4 => GetLocatedHSV(1), 
			5 => GetLocatedHSV(2), 
			6 => LocateGray, 
			7 => GetLocatedCustomMat(0), 
			8 => GetLocatedCustomMat(1), 
			9 => GetLocatedCustomMat(2), 
			10 => LocatedAIR, 
			11 => LocatedAIG, 
			12 => LocatedAIB, 
			13 => GetLocatedCustomMat(3), 
			14 => GetLocatedCustomMat(4), 
			15 => GetLocatedCustomMat(5), 
			16 => Dst, 
			17 => LocateAIEnhanceColor, 
			18 => OrgLocateAIR, 
			19 => OrgLocateAIG, 
			20 => OrgLocateAIB, 
			21 => new Mat(RotatedLocateAIR, LocateImageRect), 
			22 => new Mat(RotatedLocateAIG, LocateImageRect), 
			23 => new Mat(RotatedLocateAIB, LocateImageRect), 
			_ => GetLocatedBGR(1), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Mat GetOfflineMatNoAI(string id)
	{
		if (id != null)
		{
			switch (id.Length)
			{
			case 1:
				switch (id[0])
				{
				case '0':
					return LocatedSrc.Clone();
				case '1':
					return GetLocatedBGR(0).Clone();
				case '2':
					return GetLocatedBGR(1).Clone();
				case '3':
					return GetLocatedBGR(2).Clone();
				case '4':
					return GetLocatedHSV(0).Clone();
				case '5':
					return GetLocatedHSV(1).Clone();
				case '6':
					return GetLocatedHSV(2).Clone();
				case '7':
					return LocateGray.Clone();
				case '8':
					return GetLocatedCustomMat(0).Clone();
				case '9':
					return GetLocatedCustomMat(1).Clone();
				}
				break;
			case 2:
				switch (id[1])
				{
				case '0':
					if (!(id == "10"))
					{
						break;
					}
					return GetLocatedCustomMat(2).Clone();
				case '1':
					if (!(id == "11"))
					{
						break;
					}
					return GetLocatedCustomMat(3).Clone();
				case '2':
					if (!(id == "12"))
					{
						break;
					}
					return GetLocatedCustomMat(4).Clone();
				case '3':
					if (!(id == "13"))
					{
						break;
					}
					return GetLocatedCustomMat(5).Clone();
				}
				break;
			}
		}
		return LocatedSrc.Clone();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Mat GetOfflineMat(string id)
	{
		if (id != null)
		{
			switch (id.Length)
			{
			case 1:
				switch (id[0])
				{
				case '0':
					return LocatedSrc.Clone();
				case '1':
					return GetLocatedBGR(0).Clone();
				case '2':
					return GetLocatedBGR(1).Clone();
				case '3':
					return GetLocatedBGR(2).Clone();
				case '4':
					return GetLocatedHSV(0).Clone();
				case '5':
					return GetLocatedHSV(1).Clone();
				case '6':
					return GetLocatedHSV(2).Clone();
				case '7':
					return LocateGray.Clone();
				case '8':
					return GetLocatedCustomMat(0).Clone();
				case '9':
					return GetLocatedCustomMat(1).Clone();
				}
				break;
			case 2:
				switch (id[1])
				{
				case '0':
					if (!(id == "10"))
					{
						if (!(id == "20"))
						{
							break;
						}
						return OrgLocateAIB.Clone();
					}
					return GetLocatedCustomMat(2).Clone();
				case '1':
					if (!(id == "11"))
					{
						if (!(id == "21"))
						{
							break;
						}
						return RotatedLocateAIR.Clone();
					}
					return LocatedAIR.Clone();
				case '2':
					if (!(id == "12"))
					{
						if (!(id == "22"))
						{
							break;
						}
						return RotatedLocateAIG.Clone();
					}
					return LocatedAIG.Clone();
				case '3':
					if (!(id == "13"))
					{
						if (!(id == "23"))
						{
							break;
						}
						return RotatedLocateAIB.Clone();
					}
					return LocatedAIB.Clone();
				case '4':
					if (!(id == "14"))
					{
						break;
					}
					return GetLocatedCustomMat(3).Clone();
				case '5':
					if (!(id == "15"))
					{
						break;
					}
					return GetLocatedCustomMat(4).Clone();
				case '6':
					if (!(id == "16"))
					{
						break;
					}
					return GetLocatedCustomMat(5).Clone();
				case '7':
					if (!(id == "17"))
					{
						break;
					}
					return LocateAIEnhanceColor.Clone();
				case '8':
					if (!(id == "18"))
					{
						break;
					}
					return OrgLocateAIR.Clone();
				case '9':
					if (!(id == "19"))
					{
						break;
					}
					return OrgLocateAIG.Clone();
				}
				break;
			}
		}
		return LocatedSrc.Clone();
	}

	public void SetSelectWayFlip(int flipmode, bool offline = false)
	{
		for (int i = 0; i <= 6; i++)
		{
			switch (flipmode)
			{
			case 0:
				Cv2.Flip(GetChannelMat(i), GetChannelMat(i), FlipMode.Y);
				break;
			case 1:
				Cv2.Flip(GetChannelMat(i), GetChannelMat(i), FlipMode.X);
				break;
			case 2:
				Cv2.Flip(GetChannelMat(i), GetChannelMat(i), FlipMode.XY);
				break;
			case 3:
			{
				using (Mat mat3 = MM.newMat(GetChannelMat(i), LocatedRect).Clone())
				{
					using Mat m2 = new Mat(GetChannelMat(i), LocatedRect);
					using Mat mat4 = new Mat();
					Cv2.Transpose(mat3, mat4);
					Cv2.Flip(mat4, mat3, FlipMode.Y);
					Cv2.Resize(mat3, mat3, new OpenCvSharp.Size(LocatedRect.Width, LocatedRect.Height));
					mat3.CopyTo(m2);
				}
				break;
			}
			case 4:
			{
				using (Mat mat = MM.newMat(GetChannelMat(i), LocatedRect).Clone())
				{
					using Mat m = new Mat(GetChannelMat(i), LocatedRect);
					using Mat mat2 = new Mat();
					Cv2.Transpose(mat, mat2);
					Cv2.Flip(mat2, mat, FlipMode.X);
					Cv2.Resize(mat, mat, new OpenCvSharp.Size(LocatedRect.Width, LocatedRect.Height));
					mat.CopyTo(m);
				}
				break;
			}
			}
		}
		switch (flipmode)
		{
		case 0:
			Cv2.Flip(LocatedSrc, LocatedSrc, FlipMode.Y);
			Cv2.Flip(Dst, Dst, FlipMode.Y);
			break;
		case 1:
			Cv2.Flip(LocatedSrc, LocatedSrc, FlipMode.X);
			Cv2.Flip(Dst, Dst, FlipMode.X);
			break;
		case 2:
			Cv2.Flip(LocatedSrc, LocatedSrc, FlipMode.XY);
			Cv2.Flip(Dst, Dst, FlipMode.XY);
			break;
		case 3:
		{
			using Mat mat7 = MM.newMat(LocatedSrc, LocatedRect).Clone();
			using Mat m4 = new Mat(LocatedSrc, LocatedRect);
			using Mat mat8 = new Mat();
			Cv2.Transpose(mat7, mat8);
			Cv2.Flip(mat8, mat7, FlipMode.Y);
			Cv2.Resize(mat7, mat7, new OpenCvSharp.Size(LocatedRect.Width, LocatedRect.Height));
			mat7.CopyTo(m4);
			break;
		}
		case 4:
		{
			using Mat mat5 = MM.newMat(LocatedSrc, LocatedRect).Clone();
			using Mat m3 = new Mat(LocatedSrc, LocatedRect);
			using Mat mat6 = new Mat();
			Cv2.Transpose(mat5, mat6);
			Cv2.Flip(mat6, mat5, FlipMode.X);
			Cv2.Resize(mat5, mat5, new OpenCvSharp.Size(LocatedRect.Width, LocatedRect.Height));
			mat5.CopyTo(m3);
			break;
		}
		}
	}

	public Mat Bitmap2Mat(Bitmap bitmap)
	{
		return bitmap.ToMat();
	}

	public Mat Buffer2Mat(byte[] buffer, int w, int h, int band)
	{
		MatType type = ((band != 3) ? MatType.CV_8UC1 : MatType.CV_8UC3);
		return new Mat(h, w, type, buffer, 0L);
	}

	public Bitmap MatToBitmap(Mat image)
	{
		return image.ToBitmap();
	}

	public BitmapImage MatToBitmapImage2(Mat image)
	{
		Bitmap bitmap = MatToBitmap(image);
		using MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Bmp);
		memoryStream.Position = 0L;
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		return bitmapImage;
	}

	public ImageSource MatToBitmapImage(Mat image)
	{
		return image.ToBitmapSource();
	}

	public bool Mat2Bitmap(Mat src, ref Bitmap bitmap)
	{
		bitmap = src.ToBitmap();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Bitmap Mat2Bitmap(Mat src)
	{
		using MemoryStream stream = src.ToMemoryStream(".png");
		return (Bitmap)Image.FromStream(stream);
	}

	public byte[] BmpToBytes(Bitmap bmp)
	{
		Bitmap bitmap = VerticalFlip(bmp);
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Bmp);
		return memoryStream.GetBuffer();
	}

	public static Bitmap VerticalFlip(Bitmap bmp)
	{
		try
		{
			int width = bmp.Width;
			int height = bmp.Height;
			Graphics graphics = Graphics.FromImage(bmp);
			Rectangle rect = new Rectangle(0, 0, width, height);
			bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
			graphics.DrawImage(bmp, rect);
			return bmp;
		}
		catch
		{
			return bmp;
		}
	}

	public ImageSource Mat2ImageSource(Mat image)
	{
		if (image.Empty())
		{
			image = new Mat(100, 100, MatType.CV_8UC1);
			image.SetTo(0.0);
		}
		BitmapImage bitmapImage = new BitmapImage();
		Bitmap bitmap = Mat2Bitmap(image);
		using MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, bitmap.RawFormat);
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		return bitmapImage;
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
				Mat structuringElement4 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, count));
				Cv2.Dilate(mat, mat, structuringElement4, null, count);
				break;
			}
			case 6:
			{
				Mat structuringElement3 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, count));
				Cv2.Erode(mat, mat, structuringElement3, null, count);
				break;
			}
			case 7:
			{
				Mat structuringElement2 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(count, 1));
				Cv2.Dilate(mat, mat, structuringElement2, null, count);
				break;
			}
			case 8:
			{
				Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(count, 1));
				Cv2.Erode(mat, mat, structuringElement, null, count);
				break;
			}
			case 0:
				break;
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

	public static bool IsFileOpen(string filePath)
	{
		bool result = false;
		try
		{
			File.OpenWrite(filePath).Close();
		}
		catch
		{
			result = true;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void SaveLog(string msg, int StationIndex, string RecipeName, Mat mat, string LogID = "")
	{
		if (Saveing || (msg != "" && LastMsg == msg))
		{
			return;
		}
		LastMsg = msg;
		try
		{
			Saveing = true;
			DateTime now = DateTime.Now;
			string arg = now.ToString("yyyyMM");
			string text = string.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory, "Log\\", arg);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arg2 = now.ToString("yyyyMMdd");
			string text2 = string.Format("{0}\\{1}-soft.log", text, arg2);
			string arg3 = now.ToString("yyyy-MM-dd HH:mm:ss");
			File.Exists(text2);
			if (IsFileOpen(text2))
			{
				Saveing = false;
				return;
			}
			using (StreamWriter streamWriter = new StreamWriter(text2, append: true))
			{
				streamWriter.Write("{0}\t{1}\t", arg3, msg);
				streamWriter.WriteLine();
			}
			string recipefile = string.Format("{0}Recipe\\{1}\\station{2}.ini", AppDomain.CurrentDomain.BaseDirectory, RecipeName, StationIndex + 1);
			ZipErrorFile(text2, recipefile, mat, LogID, "D:\\LogError\\");
		}
		catch
		{
			Saveing = false;
			return;
		}
		Saveing = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ZipErrorFile(string logfile, string recipefile, Mat mat, string LogID, string LogErrorPath = "D:\\LogError\\")
	{
		try
		{
			if (LogID != "")
			{
				string[] files = Directory.GetFiles("D:\\LogError\\", "*.zip", SearchOption.AllDirectories);
				for (int i = 0; i < files.Length; i++)
				{
					if (Path.GetFileNameWithoutExtension(files[i]) == LogID)
					{
						return;
					}
				}
			}
			if (!Directory.Exists(LogErrorPath))
			{
				Directory.CreateDirectory(LogErrorPath);
			}
			string text = Path.Combine(LogErrorPath, "temp");
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			string destFileName = Path.Combine(text, Path.GetFileName(logfile));
			string destFileName2 = Path.Combine(text, Path.GetFileName(recipefile));
			string fileName = Path.Combine(text, "DebugImage.bmp");
			if (File.Exists(logfile))
			{
				File.Copy(logfile, destFileName);
			}
			if (File.Exists(recipefile))
			{
				File.Copy(recipefile, destFileName2);
			}
			if (mat.Width > 0 && mat.Height > 0)
			{
				mat.SaveImage(fileName);
			}
			DateTime now = DateTime.Now;
			string text2 = string.Format("{0}{1}\\", LogErrorPath, now.ToString("yyyy-MM"));
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			string text3 = string.Format("{0}{1}\\", text2, now.ToString("dd"));
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			string text4 = ((!(LogID == "")) ? string.Format("{0}{1}.zip", text3, LogID) : string.Format("{0}{1}.zip", text3, now.ToString("yyyy-MM-dd-HH-mm")));
			if (File.Exists(text4))
			{
				File.Delete(text4);
			}
			ZipFile.CreateFromDirectory(text, text4);
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
		}
		catch (Exception ex)
		{
			SaveLog("CVBuffer-ZipErrorFile()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void SaveLog(string msg)
	{
		if (Saveing || (msg != "" && LastMsg == msg))
		{
			return;
		}
		LastMsg = msg;
		try
		{
			Saveing = true;
			DateTime now = DateTime.Now;
			string arg = now.ToString("yyyyMM");
			string text = string.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory, "Log\\", arg);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string arg2 = now.ToString("yyyyMMdd");
			string text2 = string.Format("{0}\\{1}-soft.log", text, arg2);
			string arg3 = now.ToString("yyyy-MM-dd HH:mm:ss");
			File.Exists(text2);
			if (IsFileOpen(text2))
			{
				Saveing = false;
				return;
			}
			using StreamWriter streamWriter = new StreamWriter(text2, append: true);
			streamWriter.Write("{0}\t{1}\t", arg3, msg);
			streamWriter.WriteLine();
		}
		catch
		{
			Saveing = false;
			return;
		}
		Saveing = false;
	}

	private void CreatFillet()
	{
		using Mat mat = new Mat(10, 10, MatType.CV_8UC1);
		using Mat mat2 = new Mat(20, 20, MatType.CV_8UC1);
		using Mat mat3 = new Mat(30, 30, MatType.CV_8UC1);
		using Mat mat4 = new Mat(40, 40, MatType.CV_8UC1);
		using Mat mat5 = new Mat(50, 50, MatType.CV_8UC1);
		using Mat mat6 = new Mat(60, 60, MatType.CV_8UC1);
		using Mat mat7 = new Mat(80, 80, MatType.CV_8UC1);
		using Mat mat8 = new Mat(100, 100, MatType.CV_8UC1);
		using Mat mat9 = new Mat(120, 120, MatType.CV_8UC1);
		mat.SetTo(0.0);
		mat2.SetTo(0.0);
		mat3.SetTo(0.0);
		mat4.SetTo(0.0);
		mat5.SetTo(0.0);
		mat6.SetTo(0.0);
		mat7.SetTo(0.0);
		mat8.SetTo(0.0);
		mat9.SetTo(0.0);
		Fillet0 = new Mat(5, 5, MatType.CV_8UC1);
		Fillet0.SetTo(0.0);
		mat.Circle(new OpenCvSharp.Point(5, 5), 5, Scalar.White, -1);
		mat2.Circle(new OpenCvSharp.Point(10, 10), 10, Scalar.White, -1);
		mat3.Circle(new OpenCvSharp.Point(15, 15), 15, Scalar.White, -1);
		mat4.Circle(new OpenCvSharp.Point(20, 20), 20, Scalar.White, -1);
		mat5.Circle(new OpenCvSharp.Point(25, 25), 25, Scalar.White, -1);
		mat6.Circle(new OpenCvSharp.Point(30, 30), 30, Scalar.White, -1);
		mat7.Circle(new OpenCvSharp.Point(40, 40), 40, Scalar.White, -1);
		mat8.Circle(new OpenCvSharp.Point(50, 50), 50, Scalar.White, -1);
		mat9.Circle(new OpenCvSharp.Point(60, 60), 60, Scalar.White, -1);
		Fillet5[0] = new Mat(mat, new Rect(0, 0, 5, 5)).Clone();
		Fillet10[0] = new Mat(mat2, new Rect(0, 0, 10, 10)).Clone();
		Fillet15[0] = new Mat(mat3, new Rect(0, 0, 15, 15)).Clone();
		Fillet20[0] = new Mat(mat4, new Rect(0, 0, 20, 20)).Clone();
		Fillet25[0] = new Mat(mat5, new Rect(0, 0, 25, 25)).Clone();
		Fillet30[0] = new Mat(mat6, new Rect(0, 0, 30, 30)).Clone();
		Fillet40[0] = new Mat(mat7, new Rect(0, 0, 40, 40)).Clone();
		Fillet50[0] = new Mat(mat8, new Rect(0, 0, 50, 50)).Clone();
		Fillet60[0] = new Mat(mat9, new Rect(0, 0, 60, 60)).Clone();
		Fillet5[1] = new Mat(mat, new Rect(5, 0, 5, 5)).Clone();
		Fillet10[1] = new Mat(mat2, new Rect(10, 0, 10, 10)).Clone();
		Fillet15[1] = new Mat(mat3, new Rect(15, 0, 15, 15)).Clone();
		Fillet20[1] = new Mat(mat4, new Rect(20, 0, 20, 20)).Clone();
		Fillet25[1] = new Mat(mat5, new Rect(25, 0, 25, 25)).Clone();
		Fillet30[1] = new Mat(mat6, new Rect(30, 0, 30, 30)).Clone();
		Fillet40[1] = new Mat(mat7, new Rect(40, 0, 40, 40)).Clone();
		Fillet50[1] = new Mat(mat8, new Rect(50, 0, 50, 50)).Clone();
		Fillet60[1] = new Mat(mat9, new Rect(60, 0, 60, 60)).Clone();
		Fillet5[2] = new Mat(mat, new Rect(0, 5, 5, 5)).Clone();
		Fillet10[2] = new Mat(mat2, new Rect(0, 10, 10, 10)).Clone();
		Fillet15[2] = new Mat(mat3, new Rect(0, 15, 15, 15)).Clone();
		Fillet20[2] = new Mat(mat4, new Rect(0, 20, 20, 20)).Clone();
		Fillet25[2] = new Mat(mat5, new Rect(0, 25, 25, 25)).Clone();
		Fillet30[2] = new Mat(mat6, new Rect(0, 30, 30, 30)).Clone();
		Fillet40[2] = new Mat(mat7, new Rect(0, 40, 40, 40)).Clone();
		Fillet50[2] = new Mat(mat8, new Rect(0, 50, 50, 50)).Clone();
		Fillet60[2] = new Mat(mat9, new Rect(0, 60, 60, 60)).Clone();
		Fillet5[3] = new Mat(mat, new Rect(5, 5, 5, 5)).Clone();
		Fillet10[3] = new Mat(mat2, new Rect(10, 10, 10, 10)).Clone();
		Fillet15[3] = new Mat(mat3, new Rect(15, 15, 15, 15)).Clone();
		Fillet20[3] = new Mat(mat4, new Rect(20, 20, 20, 20)).Clone();
		Fillet25[3] = new Mat(mat5, new Rect(25, 25, 25, 25)).Clone();
		Fillet30[3] = new Mat(mat6, new Rect(30, 30, 30, 30)).Clone();
		Fillet40[3] = new Mat(mat7, new Rect(40, 40, 40, 40)).Clone();
		Fillet50[3] = new Mat(mat8, new Rect(50, 50, 50, 50)).Clone();
		Fillet60[3] = new Mat(mat9, new Rect(60, 60, 60, 60)).Clone();
	}

	public Mat GetFillet(int size, int corner)
	{
		if (Fillet5[0].Width <= 0 || Fillet5[0].Height <= 0)
		{
			CreatFillet();
		}
		return size switch
		{
			1 => Fillet5[corner], 
			2 => Fillet10[corner], 
			3 => Fillet15[corner], 
			4 => Fillet20[corner], 
			5 => Fillet25[corner], 
			6 => Fillet30[corner], 
			7 => Fillet40[corner], 
			8 => Fillet50[corner], 
			9 => Fillet60[corner], 
			_ => Fillet0, 
		};
	}

	public void DoFilllet(ref Mat Mat, Rect rect, int Filllet0, int Filllet1, int Filllet2, int Filllet3)
	{
		int num = 0;
		using Mat mat = new Mat(Mat, rect);
		using Mat mat2 = MM.newMat(Mat, rect).Clone();
		mat2.SetTo(255.0);
		if (Filllet0 > 0 && GetFillletSize(Filllet0) < rect.Width && GetFillletSize(Filllet0) < rect.Height)
		{
			if (Filllet0 == 10)
			{
				using Mat mat3 = MM.CloneFrom(mat2);
				mat3.SetTo(0.0);
				RotatedRect box = new RotatedRect(new Point2f(mat3.Width / 2, mat3.Height / 2), new Size2f(mat3.Width, mat3.Height), 0f);
				mat3.Ellipse(box, Scalar.White, -1);
				Cv2.BitwiseAnd(mat, mat3, mat);
				return;
			}
			num++;
			using Mat m = new Mat(mat2, new Rect(0, 0, GetFillletSize(Filllet0), GetFillletSize(Filllet0)));
			GetFillet(Filllet0, 0).CopyTo(m);
		}
		if (Filllet1 > 0 && GetFillletSize(Filllet1) < rect.Width && GetFillletSize(Filllet1) < rect.Height)
		{
			num++;
			using Mat m2 = new Mat(mat2, new Rect(mat2.Width - GetFillletSize(Filllet1), 0, GetFillletSize(Filllet1), GetFillletSize(Filllet1)));
			GetFillet(Filllet1, 1).CopyTo(m2);
		}
		if (Filllet2 > 0 && GetFillletSize(Filllet2) < rect.Width && GetFillletSize(Filllet2) < rect.Height)
		{
			num++;
			using Mat m3 = new Mat(mat2, new Rect(0, mat2.Height - GetFillletSize(Filllet2), GetFillletSize(Filllet2), GetFillletSize(Filllet2)));
			GetFillet(Filllet2, 2).CopyTo(m3);
		}
		if (Filllet3 > 0 && GetFillletSize(Filllet3) < rect.Width && GetFillletSize(Filllet3) < rect.Height)
		{
			num++;
			using Mat m4 = new Mat(mat2, new Rect(mat2.Width - GetFillletSize(Filllet3), mat2.Height - GetFillletSize(Filllet3), GetFillletSize(Filllet3), GetFillletSize(Filllet3)));
			GetFillet(Filllet3, 3).CopyTo(m4);
		}
		if (num > 0)
		{
			Cv2.BitwiseAnd(mat, mat2, mat);
		}
	}

	private int GetFillletSize(int index)
	{
		return index switch
		{
			0 => 0, 
			1 => 5, 
			2 => 10, 
			3 => 15, 
			4 => 20, 
			5 => 25, 
			6 => 30, 
			7 => 40, 
			8 => 50, 
			9 => 60, 
			_ => 0, 
		};
	}

	public static Mat ImRead(string path, ImreadModes imcolor = ImreadModes.Color)
	{
		Mat mat = new Mat();
		if (IsContainGBKString(path))
		{
			Bitmap bitmap = new Bitmap(path);
			mat = bitmap.ToMat();
			bitmap.Dispose();
		}
		else
		{
			mat = Cv2.ImRead(path, imcolor);
		}
		if (mat.Channels() == 4)
		{
			Cv2.CvtColor(mat, mat, ColorConversionCodes.BGRA2BGR);
		}
		return mat;
	}

	private static bool IsContainGBKString(string str)
	{
		char[] array = str.ToCharArray();
		foreach (char c in array)
		{
			if (IsGBKCode(c.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool IsGBKCode(string str)
	{
		byte[] bytes = Encoding.GetEncoding("GBK").GetBytes(str.ToString());
		if (bytes.Length <= 1)
		{
			return false;
		}
		byte b = bytes[0];
		byte b2 = bytes[1];
		if (b >= 129 && b <= 254 && b2 >= 64 && b2 <= 254)
		{
			return true;
		}
		return false;
	}
}
