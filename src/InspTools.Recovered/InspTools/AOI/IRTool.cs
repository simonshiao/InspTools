using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class IRTool
{
	private const int MaxThdTool = 4;

	private int MaxComDigitsItems = 24;

	private int[] OrgColorCode = new int[4];

	private Mat[] Binary = new Mat[4];

	private Mat FinalBinary = new Mat();

	private Mat hsv = new Mat();

	private Mat rgb = new Mat();

	private Mat MaskArea = new Mat();

	private CVBuffer buffers;

	public BlobStruct Blobs;

	public bool ResultIsA = true;

	private Mat label = new Mat();

	private Mat stats = new Mat();

	private Mat centroids = new Mat();

	private Mat ReverseMat = new Mat();

	public string ErrorStr = "";

	public Rect[] NGRect = new Rect[4];

	public Rect[] NGRectA = new Rect[4];

	public Rect[] NGRectB = new Rect[4];

	public List<BlobStruct> BlobStatistics = new List<BlobStruct>();

	private int BinaryIndex;

	private ParamFile pfile;

	private int ImageListSource;

	private Rect InspeRect;

	public double AllArea;

	public double AllAreaRatio;

	public bool[] FinalRingResult = new bool[4];

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool BinarySuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public IRTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 4; i++)
		{
			Binary[i] = new Mat();
		}
		for (int j = 0; j < 4; j++)
		{
			NGRect[j] = new Rect(0, 0, 0, 0);
			NGRectA[j] = new Rect(0, 0, 0, 0);
			NGRectB[j] = new Rect(0, 0, 0, 0);
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

	public void GetInspeRect(INSP_BINARYTOOL_PARAM param)
	{
		LocateRect = buffers.LocatedRect;
		int width = param.Track1_R - param.Track1_L;
		int height = param.Track1_B - param.Track1_T;
		InspeRect.X = pfile.VSLocateParam[0].Value + param.Track1_L;
		InspeRect.Y = pfile.HSLocateParam[0].Value + param.Track1_T;
		InspeRect.Width = width;
		InspeRect.Height = height;
	}

	private void SetValue(int[] value, int v1, int v2, int v3, int v4)
	{
		value[0] = v1;
		value[1] = v2;
		value[2] = v3;
		value[3] = v4;
	}

	private void SetValue(bool[] value, bool v1, bool v2, bool v3, bool v4)
	{
		value[0] = v1;
		value[1] = v2;
		value[2] = v3;
		value[3] = v4;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetIRColor(string ircolor)
	{
		for (int i = 0; i < 4; i++)
		{
			int num = 0;
			string arg = "";
			string text = ircolor.Substring(i, 1);
			if (text != null)
			{
				int length = text.Length;
				if (length == 1)
				{
					switch (text[0])
					{
					case '0':
						num = 1;
						arg = ((buffers.LanguageType != 1) ? "Black" : "黑");
						break;
					case '1':
						num = 2;
						arg = ((buffers.LanguageType != 1) ? "Brown" : "棕");
						break;
					case '2':
						num = 3;
						arg = ((buffers.LanguageType != 1) ? "Red" : "紅");
						break;
					case '3':
						num = 4;
						arg = ((buffers.LanguageType != 1) ? "Orange" : "橘");
						break;
					case '4':
						num = 5;
						arg = ((buffers.LanguageType != 1) ? "Yellow" : "黃");
						break;
					case '5':
						num = 6;
						arg = ((buffers.LanguageType != 1) ? "Green" : "綠");
						break;
					case '6':
						num = 7;
						arg = ((buffers.LanguageType != 1) ? "Blue" : "藍");
						break;
					case '7':
						num = 8;
						arg = ((buffers.LanguageType != 1) ? "Violet" : "紫");
						break;
					case '8':
						num = 9;
						arg = ((buffers.LanguageType != 1) ? "Gray" : "灰");
						break;
					case '9':
						num = 10;
						arg = ((buffers.LanguageType != 1) ? "White" : "白");
						break;
					case 'A':
					case 'a':
						num = 11;
						arg = ((buffers.LanguageType != 1) ? "Gold" : "金");
						break;
					case 'B':
					case 'b':
						num = 12;
						arg = ((buffers.LanguageType != 1) ? "Silver" : "銀");
						break;
					case 'C':
					case 'c':
						num = 13;
						break;
					}
				}
			}
			OrgColorCode[i] = num;
			if (num == 13)
			{
				pfile.IRToolParam.ColorRings[i].TrackerON = false;
				pfile.IRToolParam.ColorRings[i].ColorReferenceIndex = 0;
				pfile.IRToolParam.ColorRings[i].ValueChangeFlag = true;
			}
			else
			{
				pfile.IRToolParam.ColorRings[i].TrackerON = true;
				pfile.IRToolParam.ColorRings[i].TrackerName = string.Format("{0}", arg);
				pfile.IRToolParam.ColorRings[i].ColorReferenceIndex = num;
				pfile.IRToolParam.ColorRings[i].ValueChangeFlag = true;
			}
			switch (i)
			{
			case 0:
				pfile.IRToolParam.RingsColor1 = num;
				break;
			case 1:
				pfile.IRToolParam.RingsColor2 = num;
				break;
			case 2:
				pfile.IRToolParam.RingsColor3 = num;
				break;
			case 3:
				pfile.IRToolParam.RingsColor4 = num;
				break;
			}
		}
	}

	private void ReverseColor(int ringtype)
	{
		switch (ringtype)
		{
		case 1:
		{
			int[] array3 = new int[2]
			{
				pfile.IRToolParam.ColorRings[0].ColorReferenceIndex,
				pfile.IRToolParam.ColorRings[1].ColorReferenceIndex
			};
			pfile.IRToolParam.ColorRings[0].ColorReferenceIndex = array3[1];
			pfile.IRToolParam.ColorRings[1].ColorReferenceIndex = array3[0];
			break;
		}
		case 2:
		{
			int[] array2 = new int[3]
			{
				pfile.IRToolParam.ColorRings[0].ColorReferenceIndex,
				0,
				pfile.IRToolParam.ColorRings[2].ColorReferenceIndex
			};
			pfile.IRToolParam.ColorRings[0].ColorReferenceIndex = array2[2];
			pfile.IRToolParam.ColorRings[2].ColorReferenceIndex = array2[0];
			break;
		}
		default:
		{
			int[] array = new int[4]
			{
				pfile.IRToolParam.ColorRings[0].ColorReferenceIndex,
				pfile.IRToolParam.ColorRings[1].ColorReferenceIndex,
				pfile.IRToolParam.ColorRings[2].ColorReferenceIndex,
				pfile.IRToolParam.ColorRings[3].ColorReferenceIndex
			};
			pfile.IRToolParam.ColorRings[0].ColorReferenceIndex = array[0];
			pfile.IRToolParam.ColorRings[1].ColorReferenceIndex = array[1];
			pfile.IRToolParam.ColorRings[2].ColorReferenceIndex = array[2];
			pfile.IRToolParam.ColorRings[3].ColorReferenceIndex = array[3];
			break;
		}
		case 0:
			break;
		}
	}

	private void SetColorDefault(int ringtype)
	{
		switch (ringtype)
		{
		case 1:
			pfile.IRToolParam.ColorRings[0].ColorReferenceIndex = OrgColorCode[0];
			pfile.IRToolParam.ColorRings[1].ColorReferenceIndex = OrgColorCode[1];
			break;
		case 2:
			pfile.IRToolParam.ColorRings[0].ColorReferenceIndex = OrgColorCode[0];
			pfile.IRToolParam.ColorRings[1].ColorReferenceIndex = OrgColorCode[1];
			pfile.IRToolParam.ColorRings[2].ColorReferenceIndex = OrgColorCode[2];
			break;
		default:
			pfile.IRToolParam.ColorRings[0].ColorReferenceIndex = OrgColorCode[0];
			pfile.IRToolParam.ColorRings[1].ColorReferenceIndex = OrgColorCode[1];
			pfile.IRToolParam.ColorRings[2].ColorReferenceIndex = OrgColorCode[2];
			pfile.IRToolParam.ColorRings[3].ColorReferenceIndex = OrgColorCode[3];
			break;
		case 0:
			break;
		}
	}

	public bool Inspect(ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true, int imageListSource = 0, int ImageAddr = 0)
	{
		OffLine = offline;
		iresult = TOOL_RESULT.EM_OK;
		ErrorStr = "";
		int ringsType = pfile.IRToolParam.RingsType;
		if (ringsType == 0)
		{
			string TrackErrorStrA = "";
			NGRect[0] = new Rect(0, 0, 0, 0);
			NGRect[1] = new Rect(0, 0, 0, 0);
			NGRect[2] = new Rect(0, 0, 0, 0);
			NGRect[3] = new Rect(0, 0, 0, 0);
			FinalRingResult[0] = true;
			ResultIsA = true;
			if (!BinaryInspect(0, ref TrackErrorStrA, 0, imageListSource, ImageAddr))
			{
				iresult = TOOL_RESULT.EM_NG;
				ErrorStr = TrackErrorStrA;
				NGRectA[0] = pfile.IRToolParam.ColorRings[0].SearchRect1;
				FinalRingResult[0] = false;
				return false;
			}
			return true;
		}
		int num = ringsType + 1;
		int num2 = 0;
		int num3 = 0;
		string[] array = new string[num];
		string[] array2 = new string[num];
		bool[] array3 = new bool[num];
		bool[] array4 = new bool[num];
		for (int i = 0; i < num; i++)
		{
			NGRectA[i] = new Rect(0, 0, 0, 0);
			array[i] = "";
			array3[i] = true;
			FinalRingResult[i] = true;
			if (!BinaryInspect(i, ref array[i], 0, imageListSource, ImageAddr))
			{
				num2++;
				array3[i] = false;
				ResultIsA = true;
			}
			NGRectA[i] = pfile.IRToolParam.ColorRings[i].SearchRect1;
		}
		if (num2 == 0)
		{
			ResultIsA = true;
			return true;
		}
		for (int j = 0; j < num; j++)
		{
			NGRectB[j] = new Rect(0, 0, 0, 0);
			array2[j] = "";
			array4[j] = true;
			FinalRingResult[j] = true;
			if (!BinaryInspect(j, ref array2[j], 0, imageListSource, ImageAddr, ReverseImage: true))
			{
				array4[j] = false;
				num3++;
			}
			NGRectB[j] = pfile.IRToolParam.ColorRings[j].SearchRect1;
		}
		if (num3 == 0)
		{
			ResultIsA = false;
			return true;
		}
		if (num2 <= num3)
		{
			for (int k = 0; k < num; k++)
			{
				if (!array3[k])
				{
					ErrorStr += array[k];
					NGRect[k] = NGRectA[k];
				}
				else
				{
					NGRect[k] = new Rect(0, 0, 0, 0);
				}
				FinalRingResult[k] = array3[k];
			}
			ResultIsA = true;
		}
		else
		{
			for (int l = 0; l < num; l++)
			{
				if (!array4[l])
				{
					ErrorStr += array2[l];
				}
				if (!array4[num - l - 1])
				{
					NGRect[l] = NGRectA[l];
				}
				else
				{
					NGRect[l] = new Rect(0, 0, 0, 0);
				}
				FinalRingResult[l] = array4[l];
			}
			ResultIsA = false;
		}
		iresult = TOOL_RESULT.EM_NG;
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool BinaryInspect(int index, ref string TrackErrorStrA, int FaceIndex = 0, int imageListSource = 0, int ImageAddr = 0, bool ReverseImage = false)
	{
		BinarySuccess = true;
		Tracker1ResultText = "";
		TrackErrorStrA = "";
		ImageListSource = imageListSource;
		MaxComDigitsItems = 24;
		INSP_BINARYTOOL_PARAM param = pfile.IRToolParam.ColorRings[index];
		if (!param.TrackerON)
		{
			return true;
		}
		BinaryIndex = index;
		GetInspeRect(param);
		pfile.IRToolParam.ColorRings[index].SearchRect1 = InspeRect;
		bool[] array = new bool[4];
		bool flag = true;
		int[] array2 = new int[4];
		int[] array3 = new int[4];
		int[] array4 = new int[4];
		int[] array5 = new int[4];
		int[] array6 = new int[4];
		int[] array7 = new int[4];
		int[] array8 = new int[4];
		if (pfile.FontToolParam.TextMat == null)
		{
			pfile.FontToolParam.TextMat = new Mat();
		}
		try
		{
			if (param.ColorReferenceIndex == 0)
			{
				SetValue(array, param.THD1_ChannelON, param.THD2_ChannelON, param.THD3_ChannelON, param.THD4_ChannelON);
				SetValue(array2, (int)param.THD1_ChannelIndex, (int)param.THD2_ChannelIndex, (int)param.THD3_ChannelIndex, (int)param.THD4_ChannelIndex);
				if (pfile.LightMonitorParam.EnableSoftCorrection && pfile.SoftCorrectionON && param.EnableSoftCorrection)
				{
					if (param.THD1_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD1_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int tHD1_Min = param.THD1_Min;
						int tHD1_Max = param.THD1_Max;
						array3[0] = tHD1_Min;
						array4[0] = tHD1_Max;
					}
					else
					{
						int tHD1_Min2 = param.THD1_Min;
						int tHD1_Max2 = param.THD1_Max;
						array3[0] = tHD1_Min2;
						array4[0] = tHD1_Max2;
					}
					if (param.THD2_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD2_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int tHD2_Min = param.THD2_Min;
						int tHD2_Max = param.THD2_Max;
						array3[1] = tHD2_Min;
						array4[1] = tHD2_Max;
					}
					else
					{
						int tHD2_Min2 = param.THD2_Min;
						int tHD2_Max2 = param.THD2_Max;
						array3[1] = tHD2_Min2;
						array4[1] = tHD2_Max2;
					}
					if (param.THD3_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD3_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int tHD3_Min = param.THD3_Min;
						int tHD3_Max = param.THD3_Max;
						array3[2] = tHD3_Min;
						array4[2] = tHD3_Max;
					}
					else
					{
						int tHD3_Min2 = param.THD3_Min;
						int tHD3_Max2 = param.THD3_Max;
						array3[2] = tHD3_Min2;
						array4[2] = tHD3_Max2;
					}
					if (param.THD4_ChannelIndex >= COLOR_CHANNEL.EM_RED && param.THD4_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int tHD4_Min = param.THD4_Min;
						int tHD4_Max = param.THD4_Max;
						array3[3] = tHD4_Min;
						array4[3] = tHD4_Max;
					}
					else
					{
						int tHD4_Min2 = param.THD4_Min;
						int tHD4_Max2 = param.THD4_Max;
						array3[3] = tHD4_Min2;
						array4[3] = tHD4_Max2;
					}
				}
				else
				{
					SetValue(array3, param.THD1_Min, param.THD2_Min, param.THD3_Min, param.THD4_Min);
					SetValue(array4, param.THD1_Max, param.THD2_Max, param.THD3_Max, param.THD4_Max);
				}
				SetValue(array6, param.DE_Start1_Count, param.DE_Start2_Count, param.DE_Start3_Count, param.DE_Start4_Count);
				SetValue(array5, (int)param.DE_Start1_Index, (int)param.DE_Start2_Index, (int)param.DE_Start3_Index, (int)param.DE_Start4_Index);
				SetValue(array8, param.DE_End1_Count, param.DE_End2_Count, param.DE_End3_Count, param.DE_End4_Count);
				SetValue(array7, (int)param.DE_End1_Index, (int)param.DE_End2_Index, (int)param.DE_End3_Index, (int)param.DE_End4_Index);
			}
			else
			{
				int num = param.ColorReferenceIndex - 1;
				SetValue(array, pfile.ColorReferenceParam[num].THD1_ChannelON, pfile.ColorReferenceParam[num].THD2_ChannelON, pfile.ColorReferenceParam[num].THD3_ChannelON, pfile.ColorReferenceParam[num].THD4_ChannelON);
				SetValue(array2, (int)pfile.ColorReferenceParam[num].THD1_ChannelIndex, (int)pfile.ColorReferenceParam[num].THD2_ChannelIndex, (int)pfile.ColorReferenceParam[num].THD3_ChannelIndex, (int)pfile.ColorReferenceParam[num].THD4_ChannelIndex);
				SetValue(array3, pfile.ColorReferenceParam[num].THD1_Min, pfile.ColorReferenceParam[num].THD2_Min, pfile.ColorReferenceParam[num].THD3_Min, pfile.ColorReferenceParam[num].THD4_Min);
				SetValue(array4, pfile.ColorReferenceParam[num].THD1_Max, pfile.ColorReferenceParam[num].THD2_Max, pfile.ColorReferenceParam[num].THD3_Max, pfile.ColorReferenceParam[num].THD4_Max);
				SetValue(array6, pfile.ColorReferenceParam[num].DE_Start1_Count, pfile.ColorReferenceParam[num].DE_Start2_Count, pfile.ColorReferenceParam[num].DE_Start3_Count, pfile.ColorReferenceParam[num].DE_Start4_Count);
				SetValue(array5, (int)pfile.ColorReferenceParam[num].DE_Start1_Index, (int)pfile.ColorReferenceParam[num].DE_Start2_Index, (int)pfile.ColorReferenceParam[num].DE_Start3_Index, (int)pfile.ColorReferenceParam[num].DE_Start4_Index);
				SetValue(array8, pfile.ColorReferenceParam[num].DE_End1_Count, pfile.ColorReferenceParam[num].DE_End2_Count, pfile.ColorReferenceParam[num].DE_End3_Count, pfile.ColorReferenceParam[num].DE_End4_Count);
				SetValue(array7, (int)pfile.ColorReferenceParam[num].DE_End1_Index, (int)pfile.ColorReferenceParam[num].DE_End2_Index, (int)pfile.ColorReferenceParam[num].DE_End3_Index, (int)pfile.ColorReferenceParam[num].DE_End4_Index);
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("{0}:{1}-", index + 1, param.TrackerName) + "BT01-IRTool-Inspect()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "IRTool-001");
			}
			return false;
		}
		try
		{
			Blobs.Width = (Blobs.Height = (Blobs.Area = (Blobs.HeightDiff = (Blobs.WidthDiff = 0.0))));
			Blobs.TwoAreaRatio = (Blobs.AreaRatio = (Blobs.WHRatio = (Blobs.WidthRatio = 0.0)));
			Blobs.TotalWidth = (Blobs.TotalHeight = 0);
			Blobs.NGRect = new Rect(0, 0, 0, 0);
			Blobs.result = true;
			AllArea = 0.0;
			AllAreaRatio = 0.0;
			flag = true;
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				Blobs.result = true;
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", BinaryIndex + 1, param.TrackerName, "A");
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", BinaryIndex + 1, param.TrackerName, "A");
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", BinaryIndex + 1, param.TrackerName, "A");
				}
				TrackErrorStrA = Tracker1ResultText;
				return false;
			}
			InspeRect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				Blobs.result = true;
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", BinaryIndex + 1, param.TrackerName, "A");
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", BinaryIndex + 1, param.TrackerName, "A");
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", BinaryIndex + 1, param.TrackerName, "A");
				}
				TrackErrorStrA = Tracker1ResultText;
				return false;
			}
			int num2 = 0;
			Mat mat = new Mat();
			if (!ReverseImage)
			{
				mat = new Mat(buffers.GetChannelMat(array2[0]), InspeRect);
			}
			else
			{
				buffers.GetChannelMat(array2[0]).CopyTo(ReverseMat);
				Mat mat2 = new Mat(ReverseMat, LocateRect);
				Cv2.Flip(mat2, mat2, FlipMode.XY);
				mat = new Mat(ReverseMat, InspeRect);
			}
			mat.CopyTo(Binary[0]);
			Cv2.InRange(Binary[0], array3[0], array4[0], Binary[0]);
			doMorph(ref Binary[0], array5[0], array6[0]);
			doMorph(ref Binary[0], array7[0], array8[0]);
			num2++;
			array[0] = true;
			if (array[1])
			{
				if (!ReverseImage)
				{
					mat = new Mat(buffers.GetChannelMat(array2[1]), InspeRect);
				}
				else
				{
					buffers.GetChannelMat(array2[1]).CopyTo(ReverseMat);
					Mat mat3 = new Mat(ReverseMat, LocateRect);
					Cv2.Flip(mat3, mat3, FlipMode.XY);
					mat = new Mat(ReverseMat, InspeRect);
				}
				mat.CopyTo(Binary[1]);
				Cv2.InRange(Binary[1], array3[1], array4[1], Binary[1]);
				doMorph(ref Binary[1], array5[1], array6[1]);
				doMorph(ref Binary[1], array7[1], array8[1]);
			}
			if (array[2])
			{
				if (!ReverseImage)
				{
					mat = new Mat(buffers.GetChannelMat(array2[2]), InspeRect);
				}
				else
				{
					buffers.GetChannelMat(array2[2]).CopyTo(ReverseMat);
					Mat mat4 = new Mat(ReverseMat, LocateRect);
					Cv2.Flip(mat4, mat4, FlipMode.XY);
					mat = new Mat(ReverseMat, InspeRect);
				}
				mat.CopyTo(Binary[2]);
				Cv2.InRange(Binary[2], array3[2], array4[2], Binary[2]);
				doMorph(ref Binary[2], array5[2], array6[2]);
				doMorph(ref Binary[2], array7[2], array8[2]);
				num2++;
			}
			if (array[3])
			{
				if (!ReverseImage)
				{
					mat = new Mat(buffers.GetChannelMat(array2[3]), InspeRect);
				}
				else
				{
					buffers.GetChannelMat(array2[3]).CopyTo(ReverseMat);
					Mat mat5 = new Mat(ReverseMat, LocateRect);
					Cv2.Flip(mat5, mat5, FlipMode.XY);
					mat = new Mat(ReverseMat, InspeRect);
				}
				mat.CopyTo(Binary[3]);
				Cv2.InRange(Binary[3], array3[3], array4[3], Binary[3]);
				doMorph(ref Binary[3], array5[3], array6[3]);
				doMorph(ref Binary[3], array7[3], array8[3]);
				num2++;
			}
			FinalBinary = Binary[0];
			for (int i = 1; i < 4; i++)
			{
				if (array[i] && FinalBinary.Width == Binary[i].Width && FinalBinary.Height == Binary[i].Height)
				{
					if (i != 3 || !param.THD_ORMode)
					{
						Cv2.BitwiseAnd(FinalBinary, Binary[i], FinalBinary);
					}
					else
					{
						Cv2.BitwiseOr(FinalBinary, Binary[i], FinalBinary);
					}
				}
			}
			Blobs.ImageListSource = ImageListSource;
			Blobs.ImageAddr = ImageAddr;
			flag = findBlob(FinalBinary, index, ref TrackErrorStrA);
		}
		catch (Exception ex2)
		{
			string text2 = "BT02-IRTool-Inspect()-02-";
			text2 += ex2.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "IRTool-002");
			}
			return false;
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlob(Mat binary, int Ringid, ref string TrackErrorStrA)
	{
		try
		{
			int num = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			if (binary.CountNonZero() <= 3)
			{
				num = 0;
				Blobs.Width = rect.Width;
				Blobs.Height = rect.Height;
				Blobs.WHRatio = 0.0;
				Blobs.Area = num;
				Blobs.NGRect = rect;
				Blobs.NGRect.X += InspeRect.X;
				Blobs.NGRect.Y += InspeRect.Y;
				return checkResult(rect, num, 0.0, Ringid, ref TrackErrorStrA);
			}
			int num2 = Cv2.ConnectedComponentsWithStats(binary, label, stats, centroids, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = stats.GetGenericIndexer<int>();
			int num3 = 0;
			int i = 0;
			if (num2 > 1)
			{
				for (int j = 1; j < num2; j++)
				{
					Blobs.TotalWidth += genericIndexer[j, 2];
					Blobs.TotalHeight += genericIndexer[j, 3];
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
			Blobs.Width = (double)rect.Width * pfile.PixelPerUM_X;
			Blobs.Height = (double)rect.Height * pfile.PixelPerUM_Y;
			Blobs.WHRatio = num4;
			Blobs.Area = (double)num3 * pfile.PixelPerUM_X * pfile.PixelPerUM_Y;
			Blobs.NGRect = rect;
			Blobs.NGRect.X += InspeRect.X;
			Blobs.NGRect.Y += InspeRect.Y;
			return checkResult(rect, num, num4, Ringid, ref TrackErrorStrA);
		}
		catch (Exception ex)
		{
			string text = "BT02-IRTool-findBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "IRTool-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool checkResult(Rect rect, int area, double whratio, int Ringid, ref string TrackErrorStrA)
	{
		try
		{
			bool[] array = new bool[4];
			bool flag = true;
			INSP_BINARYTOOL_PARAM iNSP_BINARYTOOL_PARAM = pfile.IRToolParam.ColorRings[Ringid];
			TrackErrorStrA = "";
			TrackErrorStrA = string.Format("\n[{0}:{1}][{2}] ", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A");
			array[0] = GetRule((int)iNSP_BINARYTOOL_PARAM.InspRule1_Index, iNSP_BINARYTOOL_PARAM.InspRule1_Min, iNSP_BINARYTOOL_PARAM.InspRule1_Max, rect, area, ref TrackErrorStrA);
			array[1] = GetRule((int)iNSP_BINARYTOOL_PARAM.InspRule2_Index, iNSP_BINARYTOOL_PARAM.InspRule2_Min, iNSP_BINARYTOOL_PARAM.InspRule2_Max, rect, area, ref TrackErrorStrA);
			array[2] = GetRule((int)iNSP_BINARYTOOL_PARAM.InspRule3_Index, iNSP_BINARYTOOL_PARAM.InspRule3_Min, iNSP_BINARYTOOL_PARAM.InspRule3_Max, rect, area, ref TrackErrorStrA);
			array[3] = GetRule((int)iNSP_BINARYTOOL_PARAM.InspRule4_Index, iNSP_BINARYTOOL_PARAM.InspRule4_Min, iNSP_BINARYTOOL_PARAM.InspRule4_Max, rect, area, ref TrackErrorStrA);
			if (iNSP_BINARYTOOL_PARAM.InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && iNSP_BINARYTOOL_PARAM.InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && iNSP_BINARYTOOL_PARAM.InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && iNSP_BINARYTOOL_PARAM.InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && iNSP_BINARYTOOL_PARAM.InspRule4_Index > BlobCondition.EM_NOT_USE)
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
			if (AllArea == 0.0)
			{
				AllArea = (double)FinalBinary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				Blobs.TotalArea = AllArea;
			}
			if (AllAreaRatio == 0.0)
			{
				double num = (double)(InspeRect.Width * InspeRect.Height) * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num > 0.0 && AllArea > 0.0)
				{
					AllAreaRatio = AllArea / num;
				}
				else
				{
					AllAreaRatio = 0.0;
				}
				Blobs.TotalAreaRatio = AllAreaRatio;
			}
			if (ImageListSource != 0)
			{
				if (BlobStatistics.Count >= 500)
				{
					BlobStatistics.RemoveAt(0);
				}
				BlobStatistics.Add(Blobs);
			}
			if (pfile.MeasureUnit == 0)
			{
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, Total Width:{6:N3}um , Total Height:{7:N3}um \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, (double)Blobs.TotalWidth * pfile.PixelPerUM_X, (double)Blobs.TotalHeight * pfile.PixelPerUM_Y);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 總寬度:{6:N3}um , 總高度:{7:N3}um \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, (double)Blobs.TotalWidth * pfile.PixelPerUM_X, (double)Blobs.TotalHeight * pfile.PixelPerUM_Y);
					}
					else
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 总宽度:{6:N3}um , 总高度:{7:N3}um\n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, (double)Blobs.TotalWidth * pfile.PixelPerUM_X, (double)Blobs.TotalHeight * pfile.PixelPerUM_Y);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, Total Width:{6}um , Total Height:{7}um \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), (int)((double)Blobs.TotalWidth * pfile.PixelPerUM_X), (int)((double)Blobs.TotalHeight * pfile.PixelPerUM_Y));
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 總寬度:{6}um , 總高度:{7}um \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), (int)((double)Blobs.TotalWidth * pfile.PixelPerUM_X), (int)((double)Blobs.TotalHeight * pfile.PixelPerUM_Y));
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 总宽度:{6}um , 总高度:{7}um \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), (int)((double)Blobs.TotalWidth * pfile.PixelPerUM_X), (int)((double)Blobs.TotalHeight * pfile.PixelPerUM_Y));
				}
			}
			else if (pfile.UseFloat)
			{
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, Total Width:{6:N3}mm , Total Height:{7:N3}mm \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, (double)Blobs.TotalWidth * pfile.PixelPerUM_X, (double)Blobs.TotalHeight * pfile.PixelPerUM_Y);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 總寬度:{6:N3}mm , 總高度:{7:N3}mm \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, (double)Blobs.TotalWidth * pfile.PixelPerUM_X, (double)Blobs.TotalHeight * pfile.PixelPerUM_Y);
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 总宽度:{6:N3}mm , 总高度:{7:N3}mm\n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, (double)Blobs.TotalWidth * pfile.PixelPerUM_X, (double)Blobs.TotalHeight * pfile.PixelPerUM_Y);
				}
			}
			else if (buffers.LanguageType == 0)
			{
				Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, Total Width:{6}mm , Total Height:{7}mm \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), (int)((double)Blobs.TotalWidth * pfile.PixelPerUM_X), (int)((double)Blobs.TotalHeight * pfile.PixelPerUM_Y));
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 總寬度:{6}mm , 總高度:{7}mm \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), (int)((double)Blobs.TotalWidth * pfile.PixelPerUM_X), (int)((double)Blobs.TotalHeight * pfile.PixelPerUM_Y));
			}
			else
			{
				Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 总宽度:{6}mm , 总高度:{7}mm \n", BinaryIndex + 1, iNSP_BINARYTOOL_PARAM.TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), (int)((double)Blobs.TotalWidth * pfile.PixelPerUM_X), (int)((double)Blobs.TotalHeight * pfile.PixelPerUM_Y));
			}
			TrackErrorStrA = Tracker1ResultText;
			return flag;
		}
		catch (Exception ex)
		{
			string text = "BT03-IRTool-checkResult()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "IRTool-004");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, double min, double max, Rect rect, double area, ref string errorstr)
	{
		try
		{
			bool result = true;
			string arg = ((pfile.MeasureUnit != 0) ? "mm" : "um");
			min = GetComDigitsValue(min);
			max = GetComDigitsValue(max);
			switch (rule)
			{
			case 0:
				return true;
			case 1:
			{
				double num3 = (double)rect.Width * pfile.PixelPerUM_X;
				if (!(num3 < min) && !(num3 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("width = {0:N3}{1}, ", num3, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("寬 = {0:N3}{1}, ", num3, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("宽 = {0:N3}{1}, ", num3, arg);
					}
				}
				else
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("width = {0}{1}, ", (int)num3, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("寬 = {0}{1}, ", (int)num3, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("宽 = {0}{1}, ", (int)num3, arg);
					}
				}
				break;
			}
			case 2:
			{
				double num7 = (double)rect.Height * pfile.PixelPerUM_Y;
				if (!(num7 < min) && !(num7 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Height = {0:N3}{1}, ", num7, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("高 = {0:N3}{1}, ", num7, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("高 = {0:N3}{1}, ", num7, arg);
					}
				}
				else
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Height = {0}{1}, ", (int)num7, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("高 = {0}{1}, ", (int)num7, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("高 = {0}{1}, ", (int)num7, arg);
					}
				}
				break;
			}
			case 3:
			{
				double num5 = area * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (!(num5 < min) && !(num5 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Area = {0:N3}{1}, ", num5, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("面積 = {0:N3}{1}, ", num5, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("面积 = {0:N3}{1}, ", num5, arg);
					}
				}
				else
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Area = {0}{1}, ", (int)num5, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("面積 = {0}{1}, ", (int)num5, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("面积 = {0}{1}, ", (int)num5, arg);
					}
				}
				break;
			}
			case 4:
			{
				double num4 = (double)Blobs.TotalWidth * pfile.PixelPerUM_X;
				if (!(num4 < min) && !(num4 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("總寬度 = {0:N3}{1}, ", num4, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("总宽度 = {0:N3}{1}, ", num4, arg);
					}
					else
					{
						errorstr += string.Format("Total Width = {0:N3}{1}, ", num4, arg);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					errorstr += string.Format("總寬度 = {0:N3}{1}, ", (int)num4, arg);
				}
				else if (buffers.LanguageType == 2)
				{
					errorstr += string.Format("总宽度 = {0:N3}{1}, ", (int)num4, arg);
				}
				else
				{
					errorstr += string.Format("Total Width = {0:N3}{1}, ", (int)num4, arg);
				}
				break;
			}
			case 5:
			{
				double num6 = (double)Blobs.TotalHeight * pfile.PixelPerUM_Y;
				if (!(num6 < min) && !(num6 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("總高度 = {0:N3}{1}, ", num6, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("总高度 = {0:N3}{1}, ", num6, arg);
					}
					else
					{
						errorstr += string.Format("Total Height = {0:N3}{1}, ", num6, arg);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					errorstr += string.Format("總高度 = {0:N3}{1}, ", (int)num6, arg);
				}
				else if (buffers.LanguageType == 2)
				{
					errorstr += string.Format("总高度 = {0:N3}{1}, ", (int)num6, arg);
				}
				else
				{
					errorstr += string.Format("Total Height = {0:N3}{1}, ", (int)num6, arg);
				}
				break;
			}
			case 6:
				return true;
			case 7:
				return true;
			case 8:
				return true;
			case 9:
				return true;
			case 10:
				return true;
			case 11:
				AllArea = (double)FinalBinary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (AllArea < min || AllArea > max)
				{
					result = false;
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("All Area = {0:N3}{1}, ", AllArea, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("所有面積 = {0:N3}{1}, ", AllArea, arg);
						}
						if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("所有面积 = {0:N3}{1}, ", AllArea, arg);
						}
					}
					else
					{
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("All Area = {0}{1}, ", (int)AllArea, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("所有面積 = {0}{1}, ", (int)AllArea, arg);
						}
						if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("所有面积 = {0}{1}, ", (int)AllArea, arg);
						}
					}
				}
				else
				{
					result = true;
				}
				break;
			case 12:
			{
				if (AllArea == 0.0)
				{
					AllArea = (double)FinalBinary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				}
				double num2 = (double)(InspeRect.Width * InspeRect.Height) * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num2 > 0.0)
				{
					AllAreaRatio = AllArea / num2;
				}
				else
				{
					AllAreaRatio = 0.0;
				}
				if (AllAreaRatio < min || AllAreaRatio > max)
				{
					result = false;
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("All Area Ratio = {0:N3}{1}, ", AllAreaRatio, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("所有面積比 = {0:N3}{1}, ", AllAreaRatio, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("所有面积比 = {0:N3}{1}, ", AllAreaRatio, arg);
					}
				}
				else
				{
					result = true;
				}
				break;
			}
			case 13:
				return true;
			case 14:
			{
				double val = (double)rect.Width * pfile.PixelPerUM_X;
				double val2 = (double)rect.Height * pfile.PixelPerUM_Y;
				double num = Math.Max(val, val2);
				if (!(num < min) && !(num > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Longest Side = {0:N3}{1}, ", num, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("最長邊 = {0:N3}{1}, ", num, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("最长边 = {0:N3}{1}, ", num, arg);
					}
				}
				else
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Longest Side = {0}{1}, ", (int)num, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("最長邊 = {0}{1}, ", (int)num, arg);
					}
					if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("最长边 = {0}{1}, ", (int)num, arg);
					}
				}
				break;
			}
			default:
				return true;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT04-IRTool-GetRule()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "IRTool-005");
			}
			return false;
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
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
			string text = "BT11-IRTool-GetComDigitsValue()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "IRTool-006");
			}
			return 0.0;
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
}
