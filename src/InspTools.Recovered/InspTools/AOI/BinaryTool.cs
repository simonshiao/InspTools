using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class BinaryTool
{
	public class ConnectedComponent
	{
		public int Label { get; set; }

		public int Area { get; set; }

		public Rect BoundingBox { get; set; }
	}

	private const int MaxThdTool = 4;

	private const int MaxTracker = 2;

	private int MaxComDigitsItems = 24;

	private Mat[] Binary = new Mat[4];

	private Mat[] FinalBinary = new Mat[2];

	private Mat hsv = new Mat();

	private Mat rgb = new Mat();

	private Mat MaskArea = new Mat();

	private CVBuffer buffers;

	public BlobStruct[] Blobs = new BlobStruct[2];

	private Mat label = new Mat();

	private Mat stats = new Mat();

	private Mat centroids = new Mat();

	public List<SimpleBlobStruct> BlobLine1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> BlobLine2 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob2 = new List<SimpleBlobStruct>();

	public List<BlobStruct>[] BlobStatistics = new List<BlobStruct>[2];

	public List<Rect> LineWidthRect = new List<Rect>();

	private int BinaryIndex;

	private ParamFile pfile;

	private int ImageListSource;

	private Rect[] InspeRect = new Rect[2];

	public double[] AllArea = new double[2];

	public double[] AllAreaRatio = new double[2];

	public int LineWidthErrorCountA;

	public int LineWidthErrorCountB;

	public string TrackErrorStrA;

	public string TrackErrorStrB;

	private Mat FontToolTextMat = new Mat();

	private Mat FontToolTempMat = new Mat();

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool BinarySuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public string Tracker2ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public bool LineWidthResultAA { get; private set; }

	public bool LineWidthResultWA { get; private set; }

	public bool LineWidthResultHA { get; private set; }

	public bool LineWidthResultAB { get; private set; }

	public bool LineWidthResultWB { get; private set; }

	public bool LineWidthResultHB { get; private set; }

	public int LineWidthErrorA { get; private set; }

	public int LineWidthErrorB { get; private set; }

	public int LineHeightErrorA { get; private set; }

	public int LineHeightErrorB { get; private set; }

	public BinaryTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect[0] = new Rect(0, 0, 0, 0);
		InspeRect[1] = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 4; i++)
		{
			Binary[i] = new Mat();
		}
		for (int j = 0; j < 2; j++)
		{
			FinalBinary[j] = new Mat();
		}
		BlobStatistics[0] = new List<BlobStruct>();
		BlobStatistics[1] = new List<BlobStruct>();
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
		if (pfile.BinaryParam[index].Tracker1UseAutoSize)
		{
			InspeRect[0].X = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize1_SelectIndex].L + pfile.BinaryParam[index].Track1_L;
			InspeRect[0].Y = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize1_SelectIndex].T + pfile.BinaryParam[index].Track1_T;
			InspeRect[0].Width = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize1_SelectIndex].Width - (pfile.BinaryParam[index].Track1_L + pfile.BinaryParam[index].Track1_R);
			InspeRect[0].Height = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize1_SelectIndex].Height - (pfile.BinaryParam[index].Track1_T + pfile.BinaryParam[index].Track1_B);
		}
		else
		{
			int width = pfile.BinaryParam[index].Track1_R - pfile.BinaryParam[index].Track1_L;
			int height = pfile.BinaryParam[index].Track1_B - pfile.BinaryParam[index].Track1_T;
			InspeRect[0].X = pfile.VSLocateParam[pfile.BinaryParam[index].Tracker1AligmentXIndex].Value + pfile.BinaryParam[index].Track1_L;
			InspeRect[0].Y = pfile.HSLocateParam[pfile.BinaryParam[index].Tracker1AligmentYIndex].Value + pfile.BinaryParam[index].Track1_T;
			InspeRect[0].Width = width;
			InspeRect[0].Height = height;
		}
		if (pfile.BinaryParam[index].Tracker2UseAutoSize)
		{
			InspeRect[1].X = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize2_SelectIndex].L + pfile.BinaryParam[index].Track2_L;
			InspeRect[1].Y = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize2_SelectIndex].T + pfile.BinaryParam[index].Track2_T;
			InspeRect[1].Width = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize2_SelectIndex].Width - (pfile.BinaryParam[index].Track2_L + pfile.BinaryParam[index].Track2_R);
			InspeRect[1].Height = pfile.AdvLocateParam[pfile.BinaryParam[index].AutoSize2_SelectIndex].Height - (pfile.BinaryParam[index].Track2_T + pfile.BinaryParam[index].Track2_B);
		}
		else
		{
			int width2 = pfile.BinaryParam[index].Track2_R - pfile.BinaryParam[index].Track2_L;
			int height2 = pfile.BinaryParam[index].Track2_B - pfile.BinaryParam[index].Track2_T;
			InspeRect[1].X = pfile.VSLocateParam[pfile.BinaryParam[index].Tracker2AligmentXIndex].Value + pfile.BinaryParam[index].Track2_L;
			InspeRect[1].Y = pfile.HSLocateParam[pfile.BinaryParam[index].Tracker2AligmentYIndex].Value + pfile.BinaryParam[index].Track2_T;
			InspeRect[1].Width = width2;
			InspeRect[1].Height = height2;
		}
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
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true, int imageListSource = 0, int ImageAddr = 0)
	{
		OffLine = offline;
		BinarySuccess = true;
		Tracker1ResultText = "";
		Tracker2ResultText = "";
		TrackErrorStrA = "";
		TrackErrorStrB = "";
		ImageListSource = imageListSource;
		iresult = TOOL_RESULT.EM_OK;
		LineWidthRect.Clear();
		MaxComDigitsItems = 24;
		if (!pfile.BinaryParam[index].TrackerON)
		{
			iresult = TOOL_RESULT.EM_NOT_USE;
			return true;
		}
		if (FaceIndex != 0 && pfile.BinaryParam[index].EnableSelectFace && FaceIndex != pfile.BinaryParam[index].FaceIndex)
		{
			iresult = TOOL_RESULT.EM_OK;
			return true;
		}
		BinaryIndex = index;
		GetInspeRect(index);
		bool[] array = new bool[4];
		bool[] array2 = new bool[2];
		int[] array3 = new int[4];
		int[] array4 = new int[4];
		int[] array5 = new int[4];
		int[] array6 = new int[4];
		int[] array7 = new int[4];
		int[] array8 = new int[4];
		int[] array9 = new int[4];
		if (pfile.FontToolParam.TextMat == null)
		{
			pfile.FontToolParam.TextMat = new Mat();
		}
		try
		{
			if (pfile.BinaryParam[index].ColorReferenceIndex == 0)
			{
				SetValue(array, pfile.BinaryParam[index].THD1_ChannelON, pfile.BinaryParam[index].THD2_ChannelON, pfile.BinaryParam[index].THD3_ChannelON, pfile.BinaryParam[index].THD4_ChannelON);
				SetValue(array3, (int)pfile.BinaryParam[index].THD1_ChannelIndex, (int)pfile.BinaryParam[index].THD2_ChannelIndex, (int)pfile.BinaryParam[index].THD3_ChannelIndex, (int)pfile.BinaryParam[index].THD4_ChannelIndex);
				if (pfile.LightMonitorParam.EnableSoftCorrection && pfile.SoftCorrectionON && pfile.BinaryParam[index].EnableSoftCorrection)
				{
					if (pfile.BinaryParam[index].THD1_ChannelIndex >= COLOR_CHANNEL.EM_RED && pfile.BinaryParam[index].THD1_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min = pfile.BinaryParam[index].THD1_Min;
						int max = pfile.BinaryParam[index].THD1_Max;
						GetSoftCorrectionShiftValue((int)pfile.BinaryParam[index].THD1_ChannelIndex, ref min, ref max);
						array4[0] = min;
						array5[0] = max;
					}
					else
					{
						int tHD1_Min = pfile.BinaryParam[index].THD1_Min;
						int tHD1_Max = pfile.BinaryParam[index].THD1_Max;
						array4[0] = tHD1_Min;
						array5[0] = tHD1_Max;
					}
					if (pfile.BinaryParam[index].THD2_ChannelIndex >= COLOR_CHANNEL.EM_RED && pfile.BinaryParam[index].THD2_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min2 = pfile.BinaryParam[index].THD2_Min;
						int max2 = pfile.BinaryParam[index].THD2_Max;
						GetSoftCorrectionShiftValue((int)pfile.BinaryParam[index].THD2_ChannelIndex, ref min2, ref max2);
						array4[1] = min2;
						array5[1] = max2;
					}
					else
					{
						int tHD2_Min = pfile.BinaryParam[index].THD2_Min;
						int tHD2_Max = pfile.BinaryParam[index].THD2_Max;
						array4[1] = tHD2_Min;
						array5[1] = tHD2_Max;
					}
					if (pfile.BinaryParam[index].THD3_ChannelIndex >= COLOR_CHANNEL.EM_RED && pfile.BinaryParam[index].THD3_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min3 = pfile.BinaryParam[index].THD3_Min;
						int max3 = pfile.BinaryParam[index].THD3_Max;
						GetSoftCorrectionShiftValue((int)pfile.BinaryParam[index].THD3_ChannelIndex, ref min3, ref max3);
						array4[2] = min3;
						array5[2] = max3;
					}
					else
					{
						int tHD3_Min = pfile.BinaryParam[index].THD3_Min;
						int tHD3_Max = pfile.BinaryParam[index].THD3_Max;
						array4[2] = tHD3_Min;
						array5[2] = tHD3_Max;
					}
					if (pfile.BinaryParam[index].THD4_ChannelIndex >= COLOR_CHANNEL.EM_RED && pfile.BinaryParam[index].THD4_ChannelIndex <= COLOR_CHANNEL.EM_BLUE)
					{
						int min4 = pfile.BinaryParam[index].THD4_Min;
						int max4 = pfile.BinaryParam[index].THD4_Max;
						GetSoftCorrectionShiftValue((int)pfile.BinaryParam[index].THD4_ChannelIndex, ref min4, ref max4);
						array4[3] = min4;
						array5[3] = max4;
					}
					else
					{
						int tHD4_Min = pfile.BinaryParam[index].THD4_Min;
						int tHD4_Max = pfile.BinaryParam[index].THD4_Max;
						array4[3] = tHD4_Min;
						array5[3] = tHD4_Max;
					}
				}
				else
				{
					SetValue(array4, pfile.BinaryParam[index].THD1_Min, pfile.BinaryParam[index].THD2_Min, pfile.BinaryParam[index].THD3_Min, pfile.BinaryParam[index].THD4_Min);
					SetValue(array5, pfile.BinaryParam[index].THD1_Max, pfile.BinaryParam[index].THD2_Max, pfile.BinaryParam[index].THD3_Max, pfile.BinaryParam[index].THD4_Max);
				}
				SetValue(array7, pfile.BinaryParam[index].DE_Start1_Count, pfile.BinaryParam[index].DE_Start2_Count, pfile.BinaryParam[index].DE_Start3_Count, pfile.BinaryParam[index].DE_Start4_Count);
				SetValue(array6, (int)pfile.BinaryParam[index].DE_Start1_Index, (int)pfile.BinaryParam[index].DE_Start2_Index, (int)pfile.BinaryParam[index].DE_Start3_Index, (int)pfile.BinaryParam[index].DE_Start4_Index);
				SetValue(array9, pfile.BinaryParam[index].DE_End1_Count, pfile.BinaryParam[index].DE_End2_Count, pfile.BinaryParam[index].DE_End3_Count, pfile.BinaryParam[index].DE_End4_Count);
				SetValue(array8, (int)pfile.BinaryParam[index].DE_End1_Index, (int)pfile.BinaryParam[index].DE_End2_Index, (int)pfile.BinaryParam[index].DE_End3_Index, (int)pfile.BinaryParam[index].DE_End4_Index);
			}
			else
			{
				int num = pfile.BinaryParam[index].ColorReferenceIndex - 1;
				SetValue(array, pfile.ColorReferenceParam[num].THD1_ChannelON, pfile.ColorReferenceParam[num].THD2_ChannelON, pfile.ColorReferenceParam[num].THD3_ChannelON, pfile.ColorReferenceParam[num].THD4_ChannelON);
				SetValue(array3, (int)pfile.ColorReferenceParam[num].THD1_ChannelIndex, (int)pfile.ColorReferenceParam[num].THD2_ChannelIndex, (int)pfile.ColorReferenceParam[num].THD3_ChannelIndex, (int)pfile.ColorReferenceParam[num].THD4_ChannelIndex);
				SetValue(array4, pfile.ColorReferenceParam[num].THD1_Min, pfile.ColorReferenceParam[num].THD2_Min, pfile.ColorReferenceParam[num].THD3_Min, pfile.ColorReferenceParam[num].THD4_Min);
				SetValue(array5, pfile.ColorReferenceParam[num].THD1_Max, pfile.ColorReferenceParam[num].THD2_Max, pfile.ColorReferenceParam[num].THD3_Max, pfile.ColorReferenceParam[num].THD4_Max);
				SetValue(array7, pfile.ColorReferenceParam[num].DE_Start1_Count, pfile.ColorReferenceParam[num].DE_Start2_Count, pfile.ColorReferenceParam[num].DE_Start3_Count, pfile.ColorReferenceParam[num].DE_Start4_Count);
				SetValue(array6, (int)pfile.ColorReferenceParam[num].DE_Start1_Index, (int)pfile.ColorReferenceParam[num].DE_Start2_Index, (int)pfile.ColorReferenceParam[num].DE_Start3_Index, (int)pfile.ColorReferenceParam[num].DE_Start4_Index);
				SetValue(array9, pfile.ColorReferenceParam[num].DE_End1_Count, pfile.ColorReferenceParam[num].DE_End2_Count, pfile.ColorReferenceParam[num].DE_End3_Count, pfile.ColorReferenceParam[num].DE_End4_Count);
				SetValue(array8, (int)pfile.ColorReferenceParam[num].DE_End1_Index, (int)pfile.ColorReferenceParam[num].DE_End2_Index, (int)pfile.ColorReferenceParam[num].DE_End3_Index, (int)pfile.ColorReferenceParam[num].DE_End4_Index);
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("{0}:{1}-", index + 1, pfile.BinaryParam[index].TrackerName) + "BT01-BinaryTool-Inspect()-01-";
			text += ex.ToString();
			iresult = TOOL_RESULT.EM_NG;
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-001");
			}
			return false;
		}
		try
		{
			BlobLine1.Clear();
			BlobLine2.Clear();
			MultiBlob1.Clear();
			MultiBlob2.Clear();
			for (int i = 0; i < 2; i++)
			{
				Blobs[i].Width = (Blobs[i].Height = (Blobs[i].Area = (Blobs[i].HeightDiff = (Blobs[i].WidthDiff = 0.0))));
				Blobs[i].TwoAreaRatio = (Blobs[i].AreaRatio = (Blobs[i].WHRatio = (Blobs[i].WidthRatio = 0.0)));
				Blobs[i].NGRect = new Rect(0, 0, 0, 0);
				Blobs[i].result = true;
				AllArea[i] = 0.0;
				AllAreaRatio[i] = 0.0;
				array2[i] = true;
				if (i == 1 && !pfile.BinaryParam[index].SearchRect2ON)
				{
					continue;
				}
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[0].result = true;
					if (offline)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						if (i == 0)
						{
							TrackErrorStrA = Tracker1ResultText;
						}
						else
						{
							TrackErrorStrB = Tracker2ResultText;
						}
					}
					if (pfile.BinaryParam[BinaryIndex].TrackErrorSetNG)
					{
						array2[i] = false;
						Blobs[i].result = false;
					}
					else
					{
						array2[i] = true;
					}
					continue;
				}
				InspeRect[i] &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[0].result = true;
					if (offline)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (i == 0) ? "A" : "B");
						}
						if (i == 0)
						{
							TrackErrorStrA = Tracker1ResultText;
						}
						else
						{
							TrackErrorStrB = Tracker2ResultText;
						}
					}
					if (pfile.BinaryParam[BinaryIndex].TrackErrorSetNG)
					{
						array2[i] = false;
						Blobs[i].result = false;
					}
					else
					{
						array2[i] = true;
					}
					continue;
				}
				bool flag = false;
				flag = ((array7[0] != 0 || array7[1] != 0 || array7[2] != 0 || array7[3] != 0 || array9[0] != 0 || array9[1] != 0 || array9[2] != 0 || array9[3] != 0 || array6[0] == 10 || array8[0] == 10) ? true : false);
				if (array3[0] == 0 && array3[1] == 1 && array3[2] == 2 && array[1] && array[2] && !array[3] && !flag)
				{
					using (Mat mat = new Mat(buffers.LocatedSrc, InspeRect[i]))
					{
						mat.CopyTo(rgb);
						Cv2.InRange(rgb, new Scalar(array4[2], array4[1], array4[0]), new Scalar(array5[2], array5[1], array5[0]), FinalBinary[i]);
					}
					doMorph(ref FinalBinary[i], array6[0], array7[0]);
					doMorph(ref FinalBinary[i], array8[0], array9[0]);
				}
				else
				{
					int num2 = 0;
					if (array3[0] >= 16)
					{
						array3[0] += 5;
					}
					new Mat(buffers.GetChannelMat(array3[0]), InspeRect[i]).CopyTo(Binary[0]);
					Cv2.InRange(Binary[0], array4[0], array5[0], Binary[0]);
					doMorph(ref Binary[0], array6[0], array7[0]);
					doMorph(ref Binary[0], array8[0], array9[0]);
					num2++;
					array[0] = true;
					if (array[1])
					{
						if (array3[1] >= 16)
						{
							array3[1] += 5;
						}
						new Mat(buffers.GetChannelMat(array3[1]), InspeRect[i]).CopyTo(Binary[1]);
						Cv2.InRange(Binary[1], array4[1], array5[1], Binary[1]);
						doMorph(ref Binary[1], array6[1], array7[1]);
						doMorph(ref Binary[1], array8[1], array9[1]);
					}
					if (array[2])
					{
						if (array3[2] >= 16)
						{
							array3[2] += 5;
						}
						new Mat(buffers.GetChannelMat(array3[2]), InspeRect[i]).CopyTo(Binary[2]);
						Cv2.InRange(Binary[2], array4[2], array5[2], Binary[2]);
						doMorph(ref Binary[2], array6[2], array7[2]);
						doMorph(ref Binary[2], array8[2], array9[2]);
						num2++;
					}
					if (array[3])
					{
						if (array3[3] >= 16)
						{
							array3[3] += 5;
						}
						new Mat(buffers.GetChannelMat(array3[3]), InspeRect[i]).CopyTo(Binary[3]);
						Cv2.InRange(Binary[3], array4[3], array5[3], Binary[3]);
						doMorph(ref Binary[3], array6[3], array7[3]);
						doMorph(ref Binary[3], array8[3], array9[3]);
						num2++;
					}
					FinalBinary[i] = Binary[0];
					for (int j = 1; j < 4; j++)
					{
						if (array[j] && FinalBinary[i].Width == Binary[j].Width && FinalBinary[i].Height == Binary[j].Height)
						{
							if (j != 3 || !pfile.BinaryParam[index].THD_ORMode)
							{
								Cv2.BitwiseAnd(FinalBinary[i], Binary[j], FinalBinary[i]);
							}
							else
							{
								Cv2.BitwiseOr(FinalBinary[i], Binary[j], FinalBinary[i]);
							}
						}
					}
				}
				if (pfile.BinaryParam[index].MaskSelectIndex > 0)
				{
					if (!pfile.MaskToolON)
					{
						Blobs[0].result = false;
						if (offline)
						{
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool has been Disable! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具未開啟! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具未开启! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] 検出領域はマスクツールを使用していますが、マスクツールは開かれていません。 \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName);
							}
						}
						iresult = TOOL_RESULT.EM_NG;
						array2[i] = true;
						continue;
					}
					int num3 = pfile.BinaryParam[index].MaskSelectIndex - 1;
					if (!pfile.MaskParam[num3].TrackerON)
					{
						Blobs[0].result = false;
						if (offline)
						{
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool [{2}] has been Disable! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, num3 + 1);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具[{2}]未開啟! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, num3 + 1);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具[{2}]未开启! \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, num3 + 1);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText = string.Format("[{0}:{1}] 検出領域はマスクツールを使用していますが、マスクツール[2]は開かれていません。 \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, num3 + 1);
							}
						}
						iresult = TOOL_RESULT.EM_NG;
						array2[i] = true;
						continue;
					}
					if (pfile.MaskParam[num3].Mask1 != null)
					{
						buffers.GetChannelMat(0).CopyTo(MaskArea);
						MaskArea.SetTo(0.0);
						bool flag2 = false;
						Rect[] array10 = new Rect[2]
						{
							new Rect(pfile.MaskParam[num3].Mask1Pt.X, pfile.MaskParam[num3].Mask1Pt.Y, pfile.MaskParam[num3].Mask1.Width, pfile.MaskParam[num3].Mask1.Height),
							default(Rect)
						};
						array10[0] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
						if (pfile.MaskParam[num3].SearchRect2ON && num3 < 20 && pfile.MaskParam[num3].Mask2 != null)
						{
							array10[1] = new Rect(pfile.MaskParam[num3].Mask2Pt.X, pfile.MaskParam[num3].Mask2Pt.Y, pfile.MaskParam[num3].Mask2.Width, pfile.MaskParam[num3].Mask2.Height);
							array10[1] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
						}
						else
						{
							array10[1] = new Rect(0, 0, 0, 0);
						}
						if (array10[0].Width > 0 && array10[0].Height > 0)
						{
							using Mat m = new Mat(MaskArea, array10[0]);
							using Mat mat2 = MM.CloneFrom(pfile.MaskParam[num3].Mask1);
							mat2.CopyTo(m);
							flag2 = true;
						}
						if (pfile.MaskParam[num3].SearchRect2ON && array10[1].Width > 0 && array10[1].Height > 0 && num3 < 20)
						{
							using Mat m2 = new Mat(MaskArea, array10[1]);
							using Mat mat3 = MM.CloneFrom(pfile.MaskParam[num3].Mask2);
							mat3.CopyTo(m2);
							flag2 = true;
						}
						if (flag2)
						{
							if (pfile.BinaryParam[index].UseMaskAsInspArea)
							{
								if (FinalBinary[i].Width == InspeRect[i].Width && FinalBinary[i].Height == InspeRect[i].Height)
								{
									Mat mat4 = MM.newMat(MaskArea, InspeRect[i]).Clone();
									doMorph(ref mat4, (int)pfile.BinaryParam[index].MaskDE_Start1_Index, pfile.BinaryParam[index].MaskDE_Start1_Count);
									Cv2.BitwiseAnd(FinalBinary[i], mat4, FinalBinary[i]);
								}
							}
							else if (FinalBinary[i].Width == InspeRect[i].Width && FinalBinary[i].Height == InspeRect[i].Height)
							{
								Mat mat5 = MM.newMat(MaskArea, InspeRect[i]).Clone();
								doMorph(ref mat5, (int)pfile.BinaryParam[index].MaskDE_Start1_Index, pfile.BinaryParam[index].MaskDE_Start1_Count);
								Cv2.BitwiseNot(mat5, mat5);
								Cv2.BitwiseAnd(FinalBinary[i], mat5, FinalBinary[i]);
							}
						}
					}
				}
				if (i == 0)
				{
					buffers.DoFilllet(ref FinalBinary[i], new Rect(0, 0, FinalBinary[i].Width, FinalBinary[i].Height), pfile.BinaryParam[index].T1FillletCount0, pfile.BinaryParam[index].T1FillletCount1, pfile.BinaryParam[index].T1FillletCount2, pfile.BinaryParam[index].T1FillletCount3);
				}
				else
				{
					buffers.DoFilllet(ref FinalBinary[i], new Rect(0, 0, FinalBinary[i].Width, FinalBinary[i].Height), pfile.BinaryParam[index].T2FillletCount0, pfile.BinaryParam[index].T2FillletCount1, pfile.BinaryParam[index].T2FillletCount2, pfile.BinaryParam[index].T2FillletCount3);
				}
				if (pfile.MatchToolON)
				{
					buffers.GetChannelMat(0).CopyTo(MaskArea);
					if (pfile.BinaryParam[index].UseMatchTextMask1 > 0)
					{
						MaskArea.SetTo(255.0);
						int num4 = pfile.BinaryParam[index].UseMatchTextMask1 - 1;
						if (pfile.MatchParam[num4].TextMatResult)
						{
							Rect textRect = pfile.MatchParam[num4].TextRect;
							textRect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect.Width > 0 && textRect.Height > 0 && FinalBinary[i].Width == InspeRect[i].Width && FinalBinary[i].Height == InspeRect[i].Height)
							{
								using Mat m3 = new Mat(MaskArea, textRect);
								if (pfile.MatchParam[num4].TextMat == null)
								{
									pfile.MatchParam[num4].TextMat = new Mat();
								}
								Mat mat6 = MM.CloneFrom(pfile.MatchParam[num4].TextMat);
								doMorph(ref mat6, (int)pfile.BinaryParam[index].MatchTextDE_Start1_Index, pfile.BinaryParam[index].MatchTextDE_Start1_Count);
								Cv2.BitwiseNot(mat6, mat6);
								mat6.CopyTo(m3);
								Mat mat7 = MM.newMat(MaskArea, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(FinalBinary[i], mat7, FinalBinary[i]);
								MM.GiveBack(mat7);
								MM.GiveBack(mat6);
							}
						}
					}
					if (pfile.BinaryParam[index].UseMatchTextMask2 > 0)
					{
						MaskArea.SetTo(255.0);
						int num5 = pfile.BinaryParam[index].UseMatchTextMask2 - 1;
						if (pfile.MatchParam[num5].TextMatResult)
						{
							Rect textRect2 = pfile.MatchParam[num5].TextRect;
							textRect2 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect2.Width > 0 && textRect2.Height > 0 && FinalBinary[i].Width == InspeRect[i].Width && FinalBinary[i].Height == InspeRect[i].Height)
							{
								using Mat m4 = new Mat(MaskArea, textRect2);
								if (pfile.MatchParam[num5].TextMat == null)
								{
									pfile.MatchParam[num5].TextMat = new Mat();
								}
								Mat mat8 = MM.CloneFrom(pfile.MatchParam[num5].TextMat);
								doMorph(ref mat8, (int)pfile.BinaryParam[index].MatchTextDE_Start2_Index, pfile.BinaryParam[index].MatchTextDE_Start2_Count);
								Cv2.BitwiseNot(mat8, mat8);
								mat8.CopyTo(m4);
								Mat mat9 = MM.newMat(MaskArea, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(FinalBinary[i], mat9, FinalBinary[i]);
								MM.GiveBack(mat9);
								MM.GiveBack(mat8);
							}
						}
					}
					if (pfile.BinaryParam[index].UseMatchTextMask3 > 0)
					{
						MaskArea.SetTo(255.0);
						int num6 = pfile.BinaryParam[index].UseMatchTextMask3 - 1;
						if (pfile.MatchParam[num6].TextMatResult)
						{
							Rect textRect3 = pfile.MatchParam[num6].TextRect;
							textRect3 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect3.Width > 0 && textRect3.Height > 0 && FinalBinary[i].Width == InspeRect[i].Width && FinalBinary[i].Height == InspeRect[i].Height)
							{
								using Mat m5 = new Mat(MaskArea, textRect3);
								if (pfile.MatchParam[num6].TextMat == null)
								{
									pfile.MatchParam[num6].TextMat = new Mat();
								}
								Mat mat10 = MM.CloneFrom(pfile.MatchParam[num6].TextMat);
								doMorph(ref mat10, (int)pfile.BinaryParam[index].MatchTextDE_Start3_Index, pfile.BinaryParam[index].MatchTextDE_Start3_Count);
								Cv2.BitwiseNot(mat10, mat10);
								mat10.CopyTo(m5);
								Mat mat11 = MM.newMat(MaskArea, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(FinalBinary[i], mat11, FinalBinary[i]);
								MM.GiveBack(mat11);
								MM.GiveBack(mat10);
							}
						}
					}
				}
				if (pfile.FontToolON && pfile.BinaryParam[index].UseFontMask)
				{
					buffers.GetChannelMat(0).CopyTo(MaskArea);
					MaskArea.SetTo(255.0);
					Rect foundMatchTextRect = pfile.FontToolParam.FoundMatchTextRect;
					foundMatchTextRect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
					if (foundMatchTextRect.Width > 0 && foundMatchTextRect.Height > 0 && FinalBinary[i].Width == InspeRect[i].Width && FinalBinary[i].Height == InspeRect[i].Height)
					{
						using Mat m6 = new Mat(MaskArea, foundMatchTextRect);
						pfile.FontToolParam.TextMat.CopyTo(FontToolTextMat);
						doMorph(ref FontToolTextMat, (int)pfile.BinaryParam[index].MaskDE_Start1_Index, pfile.BinaryParam[index].MaskDE_Start1_Count);
						Cv2.BitwiseNot(FontToolTextMat, FontToolTextMat);
						FontToolTextMat.CopyTo(m6);
						new Mat(MaskArea, InspeRect[i]).CopyTo(FontToolTempMat);
						Cv2.BitwiseAnd(FinalBinary[i], FontToolTempMat, FinalBinary[i]);
					}
				}
				if (pfile.BinaryParam[index].BlobAnalysisSelect == 0)
				{
					Blobs[i].ImageListSource = ImageListSource;
					Blobs[i].ImageAddr = ImageAddr;
					array2[i] = findBlob(FinalBinary[i], i);
					continue;
				}
				if (pfile.BinaryParam[index].BlobAnalysisSelect == 1)
				{
					if (i == 0)
					{
						array2[i] = findBlobLine(FinalBinary[i], index, i, ref BlobLine1);
					}
					else
					{
						array2[i] = findBlobLine(FinalBinary[i], index, i, ref BlobLine2);
					}
					continue;
				}
				if (pfile.BinaryParam[index].BlobAnalysisSelect == 2)
				{
					if (i == 0)
					{
						array2[i] = findMultiBlob(FinalBinary[i], index, i, ref MultiBlob1, pfile.BinaryParam[index].MB_AllowNGCount, i);
					}
					else
					{
						array2[i] = findMultiBlob(FinalBinary[i], index, i, ref MultiBlob2, pfile.BinaryParam[index].MB_AllowNGCount, i);
					}
					continue;
				}
				Rect scanWindow = new Rect(0, 0, pfile.BinaryParam[index].LineWidthMinW, pfile.BinaryParam[index].LineWidthMinH);
				int MaxCount = 0;
				if (i == 0)
				{
					LineWidthResultAA = true;
					LineWidthResultWA = true;
					LineWidthResultHA = true;
					if (pfile.BinaryParam[index].EnableLineWidthA)
					{
						LineWidthResultAA = ScanLineWidth(FinalBinary[i], InspeRect[i], scanWindow, pfile.BinaryParam[index].LineWidthMinArea, out MaxCount, pfile.BinaryParam[index].AllowLineError, out LineWidthErrorCountA);
					}
				}
				else
				{
					LineWidthResultAB = true;
					LineWidthResultWB = true;
					LineWidthResultHB = true;
					if (pfile.BinaryParam[index].EnableLineWidthA)
					{
						LineWidthResultAB = ScanLineWidth(FinalBinary[i], InspeRect[i], scanWindow, pfile.BinaryParam[index].LineWidthMinArea, out MaxCount, pfile.BinaryParam[index].AllowLineError, out LineWidthErrorCountB);
					}
				}
				ScanLineWH(FinalBinary[i], pfile.BinaryParam[index].AllowLineWidth, pfile.BinaryParam[index].AllowLineHeight, out var WErrorCount, out var HErrorCount);
				if (i == 0)
				{
					LineWidthErrorA = WErrorCount;
					LineHeightErrorA = HErrorCount;
					if (pfile.BinaryParam[index].EnableLineWidthW && WErrorCount > pfile.BinaryParam[index].AllowErrorW)
					{
						LineWidthResultWA = false;
					}
					if (pfile.BinaryParam[index].EnableLineWidthH && HErrorCount > pfile.BinaryParam[index].AllowErrorH)
					{
						LineWidthResultHA = false;
					}
					if (LineWidthResultAA && LineWidthResultWA && LineWidthResultHA)
					{
						array2[i] = true;
					}
					else
					{
						array2[i] = false;
					}
				}
				else
				{
					LineWidthErrorB = WErrorCount;
					LineHeightErrorB = HErrorCount;
					if (pfile.BinaryParam[index].EnableLineWidthW && WErrorCount > pfile.BinaryParam[index].AllowErrorW)
					{
						LineWidthResultWB = false;
					}
					if (pfile.BinaryParam[index].EnableLineWidthH && HErrorCount > pfile.BinaryParam[index].AllowErrorH)
					{
						LineWidthResultHB = false;
					}
					if (LineWidthResultAB && LineWidthResultWB && LineWidthResultHB)
					{
						array2[i] = true;
					}
					else
					{
						array2[i] = false;
					}
				}
				int num7 = 0;
				string text2;
				if (i == 0)
				{
					text2 = "A";
					num7 = LineWidthErrorCountA;
				}
				else
				{
					text2 = "B";
					num7 = LineWidthErrorCountB;
				}
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] Line Width MinArea = {3}, Area Error Count = {4}, Line Width Error = {5}, Line Height Error = {6} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text2, MaxCount, num7, WErrorCount, HErrorCount);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] 最小檢出面積={3}, 面積錯誤數 = {4}, 線寬錯誤數 = {5}, 線高錯誤數 = {6} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text2, MaxCount, num7, WErrorCount, HErrorCount);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] 最小检出面积={3}, 面积错误数 = {4}, 线宽错误数 = {5}, 线高错误数 = {6} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text2, MaxCount, num7, WErrorCount, HErrorCount);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] 最小検出面積は{3}、面積エラー数は{4}、線幅エラー数は{5}、線高エラー数は{6}です。 \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text2, MaxCount, num7, WErrorCount, HErrorCount);
				}
			}
		}
		catch (Exception ex2)
		{
			string text3 = "BT02-BinaryTool-Inspect()-02-";
			text3 += ex2.ToString();
			iresult = TOOL_RESULT.EM_NG;
			if (!OffLine)
			{
				CVBuffer.SaveLog(text3);
			}
			else
			{
				CVBuffer.SaveLog(text3, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-002");
			}
			return false;
		}
		if (array2[0] && array2[1])
		{
			return true;
		}
		if (index == 0 && pfile.EnableJudgeWithAdvMeasure && (pfile.JudgeWithAdvMeasureIndexA > 0 || pfile.JudgeWithAdvMeasureIndexB > 0) && pfile.JudgeWithAdvMeasureResult)
		{
			return true;
		}
		iresult = TOOL_RESULT.EM_NG;
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findMultiBlob(Mat binary, int index, int TrackerIndex, ref List<SimpleBlobStruct> MultiBlob, int AllowNGCount, int TrackIndex)
	{
		bool result = true;
		int num = Cv2.ConnectedComponentsWithStats(binary, label, stats, centroids, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
		Mat.Indexer<int> genericIndexer = stats.GetGenericIndexer<int>();
		if (num > 1)
		{
			for (int i = 1; i < num; i++)
			{
				SimpleBlobStruct simpleBlobStruct = new SimpleBlobStruct();
				simpleBlobStruct.Width = genericIndexer[i, 2];
				simpleBlobStruct.Height = genericIndexer[i, 3];
				simpleBlobStruct.NGRect.Left = genericIndexer[i, 0] + InspeRect[TrackerIndex].Left;
				simpleBlobStruct.NGRect.Top = genericIndexer[i, 1] + InspeRect[TrackerIndex].Top;
				simpleBlobStruct.NGRect.Width = genericIndexer[i, 2];
				simpleBlobStruct.NGRect.Height = genericIndexer[i, 3];
				simpleBlobStruct.Area = genericIndexer[i, 4];
				simpleBlobStruct.result = true;
				MultiBlob.Add(simpleBlobStruct);
			}
			switch (pfile.BinaryParam[index].MB_BlobSort)
			{
			case 0:
				MultiBlob.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Width.CompareTo(y.Width));
				break;
			case 1:
				MultiBlob.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Height.CompareTo(y.Height));
				break;
			case 2:
				MultiBlob.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Area.CompareTo(y.Area));
				break;
			default:
				MultiBlob.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Area.CompareTo(y.Area));
				break;
			}
			int num2 = 0;
			for (int num3 = 0; num3 < MultiBlob.Count && num3 < pfile.BinaryParam[index].MB_BlobNum; num3++)
			{
				if (!checkMultiResult(MultiBlob[num3].NGRect, MultiBlob[num3].Area, num3, TrackIndex))
				{
					MultiBlob[num3].result = false;
					num2++;
				}
				else
				{
					MultiBlob[num3].result = true;
				}
			}
			if (num2 > AllowNGCount)
			{
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG Count={3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", num2);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG個數={3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", num2);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", num2);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] 不良品数={3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", num2);
				}
				result = false;
			}
			Blobs[TrackerIndex].result = result;
			return result;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlobLine(Mat binary, int index, int TrackerIndex, ref List<SimpleBlobStruct> BlobLine)
	{
		bool result = true;
		int num = Cv2.ConnectedComponentsWithStats(binary, label, stats, centroids, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
		Mat.Indexer<int> genericIndexer = stats.GetGenericIndexer<int>();
		string text = ((TrackerIndex != 0) ? "B" : "A");
		if (num > 1)
		{
			for (int i = 1; i < num; i++)
			{
				SimpleBlobStruct simpleBlobStruct = new SimpleBlobStruct();
				if (pfile.BinaryParam[index].UsePixelValue)
				{
					simpleBlobStruct.Width = genericIndexer[i, 2];
					simpleBlobStruct.Height = genericIndexer[i, 3];
					simpleBlobStruct.Area = genericIndexer[i, 4];
				}
				else
				{
					simpleBlobStruct.Width = (double)genericIndexer[i, 2] * pfile.PixelPerUM_X;
					simpleBlobStruct.Height = (double)genericIndexer[i, 3] * pfile.PixelPerUM_Y;
					simpleBlobStruct.Area = (double)genericIndexer[i, 4] * pfile.PixelPerUM_X * pfile.PixelPerUM_Y;
				}
				simpleBlobStruct.NGRect.Left = genericIndexer[i, 0] + InspeRect[TrackerIndex].Left;
				simpleBlobStruct.NGRect.Top = genericIndexer[i, 1] + InspeRect[TrackerIndex].Top;
				simpleBlobStruct.NGRect.Width = genericIndexer[i, 2];
				simpleBlobStruct.NGRect.Height = genericIndexer[i, 3];
				simpleBlobStruct.result = true;
				BlobLine.Add(simpleBlobStruct);
			}
			switch (pfile.BinaryParam[index].BlobLineSort)
			{
			case 0:
				BlobLine.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Width.CompareTo(y.Width));
				break;
			case 1:
				BlobLine.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Height.CompareTo(y.Height));
				break;
			case 2:
				BlobLine.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Area.CompareTo(y.Area));
				break;
			default:
				BlobLine.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Area.CompareTo(y.Area));
				break;
			}
			int num2 = 0;
			for (int num3 = 0; num3 < BlobLine.Count && num3 < pfile.BinaryParam[index].BlobLineNum; num3++)
			{
				bool flag = true;
				if (BlobLine[num3].Width < pfile.BinaryParam[index].BlobLineWidthMin || BlobLine[num3].Width > pfile.BinaryParam[index].BlobLineWidthMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Height < pfile.BinaryParam[index].BlobLineHeightMin || BlobLine[num3].Height > pfile.BinaryParam[index].BlobLineHeightMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Area < pfile.BinaryParam[index].BlobLineAreaMin || BlobLine[num3].Area > pfile.BinaryParam[index].BlobLineAreaMax)
				{
					flag = false;
				}
				BlobLine[num3].result = flag;
				if (flag)
				{
					num2++;
				}
				if (flag)
				{
					continue;
				}
				string text2 = "";
				if (pfile.MeasureUnit == 0)
				{
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							text2 += string.Format("[{0}:{1}][{2}] Line Width={3:N3}, Line Height={4:N3}, Line Area={5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
						}
						else if (buffers.LanguageType == 1)
						{
							text2 += string.Format("[{0}:{1}][{2}] 線條寬度{3:N3}, 線條高度{4:N3}, 線條面積{5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
						}
						else if (buffers.LanguageType == 2)
						{
							text2 += string.Format("[{0}:{1}][{2}] 线条宽度{3:N3}, 线条高度{4:N3}, 线条面积{5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
						}
						else if (buffers.LanguageType == 3)
						{
							text2 += string.Format("[{0}:{1}][{2}] 線幅{3:N3}, 線高{4:N3}, 線面積{5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						text2 += string.Format("[{0}:{1}][{2}] Line Width={3}, Line Height={4}, Line Area={5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 1)
					{
						text2 += string.Format("[{0}:{1}][{2}] 線條寬度{3}, 線條高度{4}, 線條面積{5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 2)
					{
						text2 += string.Format("[{0}:{1}][{2}] 线条宽度{3}, 线条高度{4}, 线条面积{5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 3)
					{
						text2 += string.Format("[{0}:{1}][{2}] 線幅{3}, 線高{4}, 線面積{5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
					}
				}
				else if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						text2 += string.Format("[{0}:{1}][{2}] Line Width={3:N3}, Line Height={4:N3}, Line Area={5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 1)
					{
						text2 += string.Format("[{0}:{1}][{2}] 線條寬度{3:N3}, 線條高度{4:N3}, 線條面積{5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 2)
					{
						text2 += string.Format("[{0}:{1}][{2}] 线条宽度{3:N3}, 线条高度{4:N3}, 线条面积{5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 3)
					{
						text2 += string.Format("[{0}:{1}][{2}] 線幅{3:N3}, 線高{4:N3}, 線面積{5:N3} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					text2 += string.Format("[{0}:{1}][{2}] Line Width={3}, Line Height={4}, Line Area={5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
				}
				else if (buffers.LanguageType == 1)
				{
					text2 += string.Format("[{0}:{1}][{2}] 線條寬度{3}, 線條高度{4}, 線條面積{5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
				}
				else if (buffers.LanguageType == 2)
				{
					text2 += string.Format("[{0}:{1}][{2}] 线条宽度{3}, 线条高度{4}, 线条面积{5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
				}
				else if (buffers.LanguageType == 3)
				{
					text2 += string.Format("[{0}:{1}][{2}] 線幅{3}, 線高{4}, 線面積{5} [NG] \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, (int)BlobLine[num3].Width, (int)BlobLine[num3].Height, (int)BlobLine[num3].Area);
				}
				if (TrackerIndex == 0)
				{
					Tracker1ResultText += text2;
				}
				else
				{
					Tracker2ResultText += text2;
				}
			}
			if (num2 < pfile.BinaryParam[index].BlobLineMin || num2 > pfile.BinaryParam[index].BlobLineMax)
			{
				result = false;
			}
			string text3 = "";
			if (buffers.LanguageType == 0)
			{
				text3 += string.Format("[{0}:{1}][{2}] Passed Line={3}，Min Passs Line={4}，Max Passs Line={5} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, num2, pfile.BinaryParam[index].BlobLineMin, pfile.BinaryParam[index].BlobLineMax);
			}
			else if (buffers.LanguageType == 1)
			{
				text3 += string.Format("[{0}:{1}][{2}] 線條合格數量為{3}，允許最小合格數為{4}，允許最大合格數為{5} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, num2, pfile.BinaryParam[index].BlobLineMin, pfile.BinaryParam[index].BlobLineMax);
			}
			else if (buffers.LanguageType == 2)
			{
				text3 += string.Format("[{0}:{1}][{2}] 线条合格数量为{3}，允许最小合格数为{4}，允许最大合格数为{5} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, num2, pfile.BinaryParam[index].BlobLineMin, pfile.BinaryParam[index].BlobLineMax);
			}
			else if (buffers.LanguageType == 3)
			{
				text3 += string.Format("[{0}:{1}][{2}] 線の合格数は{3}個、最小合格数は{4}個、最大合格数は{5}個です \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text, num2, pfile.BinaryParam[index].BlobLineMin, pfile.BinaryParam[index].BlobLineMax);
			}
			if (TrackerIndex == 0)
			{
				Tracker1ResultText += text3;
			}
			else
			{
				Tracker2ResultText += text3;
			}
			Blobs[TrackerIndex].result = result;
		}
		else
		{
			string text4 = "";
			if (buffers.LanguageType == 0)
			{
				text4 += string.Format("[{0}:{1}][{2}] Passed Line = 0 \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text);
			}
			else if (buffers.LanguageType == 1)
			{
				text4 += string.Format("[{0}:{1}][{2}] 線條數量為0 \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text);
			}
			else if (buffers.LanguageType == 2)
			{
				text4 += string.Format("[{0}:{1}][{2}] 线条数量为0 \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text);
			}
			else if (buffers.LanguageType == 3)
			{
				text4 += string.Format("[{0}:{1}][{2}] 線の数は0です \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, text);
			}
			if (TrackerIndex == 0)
			{
				Tracker1ResultText += text4;
			}
			else
			{
				Tracker2ResultText += text4;
			}
			if (BlobLine.Count < pfile.BinaryParam[index].BlobLineMin)
			{
				result = false;
				Blobs[TrackerIndex].result = false;
				return result;
			}
		}
		return result;
	}

	private bool findBlob2(Mat binary, int TrackerIndex)
	{
		ConnectedComponents connectedComponents = Cv2.ConnectedComponentsEx(binary);
		if (connectedComponents.LabelCount <= 1)
		{
			Rect rect = new Rect(0, 0, 0, 0);
			Blobs[TrackerIndex].Width = 0.0;
			Blobs[TrackerIndex].Height = 0.0;
			Blobs[TrackerIndex].WHRatio = 0.0;
			Blobs[TrackerIndex].Area = 0.0;
			Blobs[TrackerIndex].NGRect = rect;
			Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
			Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
			return checkResult(rect, 0, 0.0, TrackerIndex);
		}
		ConnectedComponents.Blob largestBlob = connectedComponents.GetLargestBlob();
		Blobs[TrackerIndex].Width = largestBlob.Width;
		Blobs[TrackerIndex].Height = largestBlob.Height;
		double num = ((largestBlob.Width == 0 || largestBlob.Height == 0) ? 0.0 : ((largestBlob.Width <= largestBlob.Height) ? ((double)largestBlob.Height / (double)largestBlob.Width) : ((double)largestBlob.Width / (double)largestBlob.Height)));
		Blobs[TrackerIndex].WHRatio = num;
		Blobs[TrackerIndex].Area = largestBlob.Area;
		Blobs[TrackerIndex].NGRect = largestBlob.Rect;
		Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
		Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
		return checkResult(largestBlob.Rect, largestBlob.Area, num, TrackerIndex);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlobNew(Mat binary, int TrackerIndex)
	{
		try
		{
			int num = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			if (binary.CountNonZero() <= 3)
			{
				num = 0;
				Blobs[TrackerIndex].Width = rect.Width;
				Blobs[TrackerIndex].Height = rect.Height;
				Blobs[TrackerIndex].WHRatio = 0.0;
				Blobs[TrackerIndex].Area = num;
				Blobs[TrackerIndex].NGRect = rect;
				Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
				Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
				return checkResult(rect, num, 0.0, TrackerIndex);
			}
			List<ConnectedComponent> list = new List<ConnectedComponent>();
			list = FindConnectedComponents(binary);
			int num2 = 0;
			int index = 0;
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Area > num2)
					{
						index = i;
						num2 = list[i].Area;
					}
				}
				rect = new Rect(list[index].BoundingBox.Left, list[index].BoundingBox.Top, list[index].BoundingBox.Width, list[index].BoundingBox.Height);
				num = num2;
			}
			else
			{
				rect = new Rect(0, 0, 0, 0);
				num = num2;
			}
			double num3 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			Blobs[TrackerIndex].Width = (double)rect.Width * pfile.PixelPerUM_X;
			Blobs[TrackerIndex].Height = (double)rect.Height * pfile.PixelPerUM_Y;
			Blobs[TrackerIndex].WHRatio = num3;
			Blobs[TrackerIndex].Area = (double)num2 * pfile.PixelPerUM_X * pfile.PixelPerUM_Y;
			Blobs[TrackerIndex].NGRect = rect;
			Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
			Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
			return checkResult(rect, num, num3, TrackerIndex);
		}
		catch (Exception ex)
		{
			string text = "BT02-BinaryTool-findBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlob(Mat binary, int TrackerIndex)
	{
		try
		{
			int num = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			if (binary.CountNonZero() <= 3)
			{
				num = 0;
				Blobs[TrackerIndex].Width = rect.Width;
				Blobs[TrackerIndex].Height = rect.Height;
				Blobs[TrackerIndex].WHRatio = 0.0;
				Blobs[TrackerIndex].Area = num;
				Blobs[TrackerIndex].NGRect = rect;
				Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
				Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
				return checkResult(rect, num, 0.0, TrackerIndex);
			}
			int num2 = Cv2.ConnectedComponentsWithStats(binary, label, stats, centroids, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = stats.GetGenericIndexer<int>();
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
			Blobs[TrackerIndex].Width = (double)rect.Width * pfile.PixelPerUM_X;
			Blobs[TrackerIndex].Height = (double)rect.Height * pfile.PixelPerUM_Y;
			Blobs[TrackerIndex].WHRatio = num4;
			Blobs[TrackerIndex].Area = (double)num3 * pfile.PixelPerUM_X * pfile.PixelPerUM_Y;
			Blobs[TrackerIndex].NGRect = rect;
			Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
			Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
			return checkResult(rect, num, num4, TrackerIndex);
		}
		catch (Exception ex)
		{
			string text = "BT02-BinaryTool-findBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-004");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool checkMultiResult(Rect rect, double area, int BlobIndex, int TrackIndex)
	{
		try
		{
			bool[] array = new bool[4];
			bool flag = true;
			if (TrackIndex == 0)
			{
				TrackErrorStrA = "";
				TrackErrorStrA = string.Format("[{0}:{1}][{2}] ", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, "A");
				array[0] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule1_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule1_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule1_Max, rect, area, TrackIndex, ref TrackErrorStrA);
				array[1] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule2_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule2_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule2_Max, rect, area, TrackIndex, ref TrackErrorStrA);
				array[2] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule3_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule3_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule3_Max, rect, area, TrackIndex, ref TrackErrorStrA);
				array[3] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule4_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule4_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule4_Max, rect, area, TrackIndex, ref TrackErrorStrA);
			}
			else
			{
				TrackErrorStrB = "";
				TrackErrorStrB = string.Format("[{0}:{1}][{2}] ", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, "B");
				array[0] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule1_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule1_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule1_Max, rect, area, TrackIndex, ref TrackErrorStrB);
				array[1] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule2_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule2_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule2_Max, rect, area, TrackIndex, ref TrackErrorStrB);
				array[2] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule3_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule3_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule3_Max, rect, area, TrackIndex, ref TrackErrorStrB);
				array[3] = GetRule((int)pfile.BinaryParam[BinaryIndex].MB_InspRule4_Index, pfile.BinaryParam[BinaryIndex].MB_InspRule4_Min, pfile.BinaryParam[BinaryIndex].MB_InspRule4_Max, rect, area, TrackIndex, ref TrackErrorStrB);
			}
			if (pfile.BinaryParam[BinaryIndex].MB_InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.BinaryParam[BinaryIndex].MB_InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.BinaryParam[BinaryIndex].MB_InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.BinaryParam[BinaryIndex].MB_InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.BinaryParam[BinaryIndex].MB_InspRule4_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
			}
			double num = 0.0;
			num = ((area != 0.0 && rect.Width > 0 && rect.Height > 0) ? (area / (double)(rect.Width * rect.Height)) : 0.0);
			double num2 = 0.0;
			num2 = ((rect.Width <= 0 || rect.Height <= 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			string text = "";
			if (!flag)
			{
				text = "[NG]";
			}
			string text2 = "";
			if (pfile.MeasureUnit == 0)
			{
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						text2 += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}um, Height:{5:N3}um, Area:{6:N3}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} All Arera{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
					}
					else if (buffers.LanguageType == 1)
					{
						text2 += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, 長寬比:{7:N3}, 面積比:{8:N3}  所有面積{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
					}
					else if (buffers.LanguageType == 2)
					{
						text2 += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}um, 高:{5:N3}um, 面积:{6:N3}um, 长宽比:{7:N3}, 面积比:{8:N3}  所有面积{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
					}
					else if (buffers.LanguageType == 3)
					{
						text2 += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, 長さ比:{7:N3}, 面積比:{8:N3}  全体面積:{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4}um, Height:{5}um, Area:{6}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} All Arera{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
				}
				else if (buffers.LanguageType == 1)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4}um, 高:{5}um, 面積:{6}um, 長寬比:{7:N3}, 面積比:{8:N3}  所有面積{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
				}
				else if (buffers.LanguageType == 2)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4}um, 高:{5}um, 面积:{6}um, 长宽比:{7:N3}, 面积比:{8:N3}  所有面积{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
				}
				else if (buffers.LanguageType == 3)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4}um, 高:{5}um, 面積:{6}um, 長さ比:{7:N3}, 面積比:{8:N3}  全体面積:{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
				}
			}
			else if (pfile.UseFloat)
			{
				if (buffers.LanguageType == 0)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}mm, Height:{5:N3}mm, Area:{6:N3}mm, WH Ratio:{7:N3}, Area Ratio:{8:N3} All Arera{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
				}
				else if (buffers.LanguageType == 1)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm, 長寬比:{7:N3}, 面積比:{8:N3} 所有面積{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
				}
				else if (buffers.LanguageType == 2)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}mm, 高:{5:N3}mm, 面积:{6:N3}mm, 长宽比:{7:N3}, 面积比:{8:N3} 所有面积{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
				}
				else if (buffers.LanguageType == 3)
				{
					text2 += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm, 長さ比:{7:N3}, 面積比:{8:N3}  全体面積:{9:N3} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, AllArea[0], text);
				}
			}
			else if (buffers.LanguageType == 0)
			{
				text2 += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4}mm, Height:{5}mm, Area:{6:N3}mm, WH Ratio:{7:N3}, Area Ratio:{8:N3} All Arera{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
			}
			else if (buffers.LanguageType == 1)
			{
				text2 += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4}mm, 高:{5}mm, 面積:{6:N3}mm, 長寬比:{7:N3}, 面積比:{8:N3} 所有面積{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
			}
			else if (buffers.LanguageType == 2)
			{
				text2 += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4}mm, 高:{5:N3}mm, 面积:{6:N3}mm, 长宽比:{7:N3}, 面积比:{8:N3} 所有面积{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
			}
			else if (buffers.LanguageType == 3)
			{
				text2 += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4}mm, 高:{5}mm, 面積:{6}mm, 長さ比:{7:N3}, 面積比:{8:N3}  全体面積:{9} {10} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackIndex == 0) ? "A" : "B", BlobIndex + 1, (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)(area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), num2, num, (int)AllArea[0], text);
			}
			if (TrackIndex == 0)
			{
				Tracker1ResultText += text2;
			}
			else
			{
				Tracker2ResultText += text2;
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text3 = "BT03-BinaryTool-checkMultiResult()-";
			text3 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text3);
			}
			else
			{
				CVBuffer.SaveLog(text3, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-005");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool checkResult(Rect rect, int area, double whratio, int TrackerIndex)
	{
		try
		{
			bool[] array = new bool[4];
			bool flag = true;
			if (TrackerIndex == 0)
			{
				TrackErrorStrA = "";
				TrackErrorStrA = string.Format("\n[{0}:{1}][{2}] ", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, "A");
				array[0] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule1_Index, pfile.BinaryParam[BinaryIndex].InspRule1_Min, pfile.BinaryParam[BinaryIndex].InspRule1_Max, rect, area, TrackerIndex, ref TrackErrorStrA);
				array[1] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule2_Index, pfile.BinaryParam[BinaryIndex].InspRule2_Min, pfile.BinaryParam[BinaryIndex].InspRule2_Max, rect, area, TrackerIndex, ref TrackErrorStrA);
				array[2] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule3_Index, pfile.BinaryParam[BinaryIndex].InspRule3_Min, pfile.BinaryParam[BinaryIndex].InspRule3_Max, rect, area, TrackerIndex, ref TrackErrorStrA);
				array[3] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule4_Index, pfile.BinaryParam[BinaryIndex].InspRule4_Min, pfile.BinaryParam[BinaryIndex].InspRule4_Max, rect, area, TrackerIndex, ref TrackErrorStrA);
			}
			else
			{
				TrackErrorStrB = "";
				TrackErrorStrB = string.Format("\n[{0}:{1}][{2}] ", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, "B");
				array[0] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule1_Index, pfile.BinaryParam[BinaryIndex].InspRule1_Min, pfile.BinaryParam[BinaryIndex].InspRule1_Max, rect, area, TrackerIndex, ref TrackErrorStrB);
				array[1] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule2_Index, pfile.BinaryParam[BinaryIndex].InspRule2_Min, pfile.BinaryParam[BinaryIndex].InspRule2_Max, rect, area, TrackerIndex, ref TrackErrorStrB);
				array[2] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule3_Index, pfile.BinaryParam[BinaryIndex].InspRule3_Min, pfile.BinaryParam[BinaryIndex].InspRule3_Max, rect, area, TrackerIndex, ref TrackErrorStrB);
				array[3] = GetRule((int)pfile.BinaryParam[BinaryIndex].InspRule4_Index, pfile.BinaryParam[BinaryIndex].InspRule4_Min, pfile.BinaryParam[BinaryIndex].InspRule4_Max, rect, area, TrackerIndex, ref TrackErrorStrB);
			}
			if (pfile.BinaryParam[BinaryIndex].InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.BinaryParam[BinaryIndex].InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.BinaryParam[BinaryIndex].InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.BinaryParam[BinaryIndex].InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.BinaryParam[BinaryIndex].InspRule4_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
			}
			Blobs[TrackerIndex].result = flag;
			if (area == 0 || rect.Width <= 0 || rect.Height <= 0)
			{
				Blobs[TrackerIndex].AreaRatio = 0.0;
			}
			else
			{
				Blobs[TrackerIndex].AreaRatio = (double)area / (double)(rect.Width * rect.Height);
			}
			if (AllArea[TrackerIndex] == 0.0)
			{
				AllArea[TrackerIndex] = (double)FinalBinary[TrackerIndex].CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				Blobs[TrackerIndex].TotalArea = AllArea[TrackerIndex];
			}
			if (AllAreaRatio[TrackerIndex] == 0.0)
			{
				double num = (double)(InspeRect[TrackerIndex].Width * InspeRect[TrackerIndex].Height) * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num > 0.0 && AllArea[TrackerIndex] > 0.0)
				{
					AllAreaRatio[TrackerIndex] = AllArea[TrackerIndex] / num;
				}
				else
				{
					AllAreaRatio[TrackerIndex] = 0.0;
				}
				Blobs[TrackerIndex].TotalAreaRatio = AllAreaRatio[TrackerIndex];
			}
			if (ImageListSource != 0)
			{
				if (BlobStatistics[TrackerIndex].Count >= 500)
				{
					BlobStatistics[TrackerIndex].RemoveAt(0);
				}
				BlobStatistics[TrackerIndex].Add(Blobs[TrackerIndex]);
			}
			if (TrackerIndex == 0)
			{
				if (pfile.MeasureUnit == 0)
				{
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8:N3}, All Area Ratio:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8:N3}, 所有面積比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8:N3}, 所有面积比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}µm, 高:{4:N3}µm, 面積:{5:N3}µm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8:N3}, 全体面積比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8}, All Area Ratio:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8}, 所有面積比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8}, 所有面积比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}µm, 高:{4}µm, 面積:{5}µm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8}, 全体面積比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
					}
				}
				else if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8:N3}, All Area Ratio:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8:N3}, 所有面積比:{9:N3}  \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8:N3}, 所有面积比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8:N3}, 全体面積比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio, AllArea[0], AllAreaRatio[0]);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8}, All Area Ratio:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8}, 所有面積比:{9:N3}  \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8}, 所有面积比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}mm, 高:{4}mm, 面積:{5}mm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8}, 全体面積比:{9:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio, (int)AllArea[0], AllAreaRatio[0]);
				}
			}
			else
			{
				if (Blobs[0].Area > 0.0 && Blobs[1].Area > 0.0)
				{
					if (Blobs[0].Area > Blobs[1].Area)
					{
						Blobs[1].TwoAreaRatio = Blobs[0].Area / Blobs[1].Area;
					}
					else
					{
						Blobs[1].TwoAreaRatio = Blobs[1].Area / Blobs[0].Area;
					}
					Blobs[1].WidthDiff = Math.Abs(Blobs[0].Width - Blobs[1].Width);
					Blobs[1].HeightDiff = Math.Abs(Blobs[0].Height - Blobs[1].Height);
					if (Blobs[0].Width > Blobs[1].Width)
					{
						if (Blobs[1].Width > 0.0)
						{
							Blobs[1].WidthRatio = Blobs[0].Width / Blobs[1].Width;
						}
					}
					else if (Blobs[0].Width > 0.0)
					{
						Blobs[1].WidthRatio = Blobs[1].Width / Blobs[0].Width;
					}
				}
				if (pfile.MeasureUnit == 0)
				{
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8:N3}, All Area Ratio:{9:N3}, AB Area Ratio:{10:N3}, AB Height Diff.:{11:N3}, AB Width Diff.:{12:N3}, AB Width Ratio:{13:N3}, AB Height Ratio:{14:N3}, AB Area Diff:{15:N3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8:N3}, 所有面積比:{9:N3}, AB區域面積比:{10:N3}, AB高度差:{11:N3}, AB寬度差:{12:N3}, AB寬度比:{13:N3}, AB高度比:{14:N3}, AB所有面積差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8:N3}, 所有面积比:{9:N3}, AB区域面积比:{10:N3}, AB高度差:{11:N3}, AB宽度差:{12:N3}, AB宽度比:{13:N3}, AB寬度比:{14:N3}, AB所有面积差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}µm, 高:{4:N3}µm, 面積:{5:N3}µm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8:N3}, 全体面積比:{9:N3}, AB領域面積比:{10:N3}, AB高さ差:{11:N3}, AB幅度差:{12:N3}, AB幅度比:{13:N3}, AB幅度比:{14:N3}, AB全体面積差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8}, All Area Ratio:{9:N3}, AB Area Ratio:{10:N3}, AB Height Diff.:{11}, AB Width Diff.:{12}, AB Width Ratio:{13:N3}, AB Height Ratio:{14:N3}, AB Area Diff:{15}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8}, 所有面積比:{9:N3}, AB區域面積比:{10:N3}, AB高度差:{11}, AB寬度差:{12}, AB寬度比:{13:N3}, AB高度比:{14:N3}, AB所有面積差:{15} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8}, 所有面积比:{9:N3}, AB区域面积比:{10:N3}, AB高度差:{11}, AB宽度差:{12}, AB宽度比:{13:N3}, AB高度比:{14:N3}, AB所有面积差:{15} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}µm, 高:{4}µm, 面積:{5}µm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8}, 全体面積比:{9:N3}, AB領域面積比:{10:N3}, AB高さ差:{11}, AB幅度差:{12}, AB幅度比:{13:N3}, AB幅度比:{14:N3}, AB全体面積差:{15} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
					}
				}
				else if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8:N3}, All Area Ratio:{9:N3}, AB Area Ratio:{10:N3}, AB Height Diff.:{11:N3}, AB Width Diff.:{12:N3}, AB Width Ratio:{13:N3}, AB Height Ratio:{14:N3}, AB Area Diff:{15:N3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8:N3}, 所有面積比:{9:N3}, AB區域面積比:{10:N3}, AB高度差:{11:N3}, AB寬度差:{12:N3}, AB寬度比:{13:N3}, AB高度比:{14:N3}, AB所有面積差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8:N3}, 所有面积比:{9:N3}, AB区域面积比:{10:N3}, AB高度差:{11:N3}, AB宽度差:{12:N3}, AB宽度比:{13:N3}, AB高度比:{14:N3}, AB所有面积差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8:N3}mm^2, 全体面積比:{9:N3}, AB領域面積比:{10:N3}, AB高さ差:{11:N3}, AB幅度差:{12:N3}, AB幅度比:{13:N3}, AB高度比:{14:N3}, AB全体面積差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, All Area:{8}, All Area Ratio:{9:N3}, AB Area Ratio:{10:N3}, AB Height Diff.:{11}, AB Width Diff.:{12}, AB Width Ratio:{13:N3}, AB Height Ratio:{14:N3}, AB Area Diff:{15:N3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3}, 所有面積:{8}, 所有面積比:{9:N3}, AB區域面積比:{10:N3}, AB高度差:{11}, AB寬度差:{12}, AB寬度比:{13:N3}, AB高度比:{14:N3}, AB所有面積差:{15:N3}\n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3}, 所有面积:{8}, 所有面积比:{9:N3}, AB区域面积比:{10:N3}, AB高度差:{11}, AB宽度差:{12}, AB宽度比:{13:N3}, AB高度比:{14:N3}, AB所有面积差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, (int)AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, (int)Blobs[1].TotalAreaDiff);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}mm, 高:{4}mm, 面積:{5}mm, 長さ比:{6:N3}, 面積比:{7:N3}, 全体面積:{8}, 全体面積比:{9:N3}, AB領域面積比:{10:N3}, AB高さ差:{11}, AB幅度差:{12}, AB幅度比:{13:N3}, AB高度比:{14:N3}, AB全体面積差:{15:N3} \n", BinaryIndex + 1, pfile.BinaryParam[BinaryIndex].TrackerName, (TrackerIndex == 0) ? "A" : "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[1].AreaRatio, AllArea[1], AllAreaRatio[1], Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio, Blobs[1].HeightRatio, Blobs[1].TotalAreaDiff);
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text = "BT03-BinaryTool-checkResult()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-006");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, double min, double max, Rect rect, double area, int TrackerIndex, ref string errorstr)
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
				double value3 = (double)rect.Width * pfile.PixelPerUM_X;
				value3 = ((!pfile.UseFloat) ? Math.Round(value3, 0) : Math.Round(value3, 3));
				if (!(value3 < min) && !(value3 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("width = {0:N3}{1}, ", value3, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("寬 = {0:N3}{1}, ", value3, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("宽 = {0:N3}{1}, ", value3, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("幅 = {0:N3}{1}, ", value3, arg);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					errorstr += string.Format("width = {0}{1}, ", (int)value3, arg);
				}
				else if (buffers.LanguageType == 1)
				{
					errorstr += string.Format("寬 = {0}{1}, ", (int)value3, arg);
				}
				else if (buffers.LanguageType == 2)
				{
					errorstr += string.Format("宽 = {0}{1}, ", (int)value3, arg);
				}
				else if (buffers.LanguageType == 3)
				{
					errorstr += string.Format("幅 = {0}{1}, ", (int)value3, arg);
				}
				break;
			}
			case 2:
			{
				double value7 = (double)rect.Height * pfile.PixelPerUM_Y;
				value7 = ((!pfile.UseFloat) ? Math.Round(value7, 0) : Math.Round(value7, 3));
				if (!(value7 < min) && !(value7 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Height = {0:N3}{1}, ", value7, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("高 = {0:N3}{1}, ", value7, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("高 = {0:N3}{1}, ", value7, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("高 = {0:N3}{1}, ", value7, arg);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					errorstr += string.Format("Height = {0}{1}, ", (int)value7, arg);
				}
				else if (buffers.LanguageType == 1)
				{
					errorstr += string.Format("高 = {0}{1}, ", (int)value7, arg);
				}
				else if (buffers.LanguageType == 2)
				{
					errorstr += string.Format("高 = {0}{1}, ", (int)value7, arg);
				}
				else if (buffers.LanguageType == 3)
				{
					errorstr += string.Format("高 = {0}{1}, ", (int)value7, arg);
				}
				break;
			}
			case 3:
			{
				double value6 = area * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				value6 = ((!pfile.UseFloat) ? Math.Round(value6, 0) : Math.Round(value6, 3));
				if (!(value6 < min) && !(value6 > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Area = {0:N3}{1}, ", value6, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("面積 = {0:N3}{1}, ", value6, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("面积 = {0:N3}{1}, ", value6, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("面積 = {0:N3}{1}, ", value6, arg);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					errorstr += string.Format("Area = {0}{1}, ", (int)value6, arg);
				}
				else if (buffers.LanguageType == 1)
				{
					errorstr += string.Format("面積 = {0}{1}, ", (int)value6, arg);
				}
				else if (buffers.LanguageType == 2)
				{
					errorstr += string.Format("面积 = {0}{1}, ", (int)value6, arg);
				}
				else if (buffers.LanguageType == 3)
				{
					errorstr += string.Format("面積 = {0}{1}, ", (int)value6, arg);
				}
				break;
			}
			case 4:
			{
				double value5 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				value5 = Math.Round(value5, 3);
				if (value5 < min || value5 > max)
				{
					result = false;
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("WH Ratio = {0:N3}{1}, ", value5, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("長寬比 = {0:N3}{1}, ", value5, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("长宽比 = {0:N3}{1}, ", value5, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("長さ比 = {0:N3}{1}, ", value5, arg);
					}
				}
				break;
			}
			case 5:
			{
				double areaRatio = 0.0;
				Blobs[TrackerIndex].AreaRatio = areaRatio;
				if ((double)rect.Width * (double)rect.Height <= 0.0)
				{
					Blobs[TrackerIndex].AreaRatio = areaRatio;
					result = true;
					break;
				}
				areaRatio = area / (double)(rect.Width * rect.Height);
				Blobs[TrackerIndex].AreaRatio = areaRatio;
				areaRatio = Math.Round(areaRatio, 3);
				if (areaRatio < min || areaRatio > max)
				{
					result = false;
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Area Ratio = {0:N3}{1}, ", areaRatio, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("面積比 = {0:N3}{1}, ", areaRatio, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("面积比 = {0:N3}{1}, ", areaRatio, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("面積比 = {0:N3}{1}, ", areaRatio, arg);
					}
				}
				else
				{
					result = true;
				}
				break;
			}
			case 6:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double num5 = 0.0;
				if (Blobs[0].Area > 0.0 && Blobs[1].Area > 0.0)
				{
					num5 = ((!(Blobs[0].Area > Blobs[1].Area)) ? (Blobs[1].Area / Blobs[0].Area) : (Blobs[0].Area / Blobs[1].Area));
					Blobs[1].TwoAreaRatio = num5;
					num5 = Math.Round(num5, 3);
					if (num5 < min || num5 > max)
					{
						result = false;
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("AB Area Ratio = {0:N3}{1}, ", num5, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("AB區域面積比 = {0:N3}{1}, ", num5, arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("AB区域面积比 = {0:N3}{1}, ", num5, arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("AB領域面積比 = {0:N3}{1}, ", num5, arg);
						}
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = false;
				}
				break;
			}
			case 7:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double value4 = Math.Abs(Blobs[0].Height - Blobs[1].Height);
				value4 = ((!pfile.UseFloat) ? Math.Round(value4, 0) : Math.Round(value4, 3));
				if (value4 < min || value4 > max)
				{
					result = false;
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("AB Height Diff. = {0:N3}{1}, ", value4, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("AB高度差 = {0:N3}{1}, ", value4, arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("AB高度差 = {0:N3}{1}, ", value4, arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("AB高さ差 = {0:N3}{1}, ", value4, arg);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("AB Height Diff. = {0}{1}, ", (int)value4, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("AB高度差 = {0}{1}, ", (int)value4, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("AB高度差 = {0}{1}, ", (int)value4, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("AB高さ差 = {0}{1}, ", (int)value4, arg);
					}
				}
				else
				{
					result = true;
				}
				Blobs[1].HeightDiff = value4;
				break;
			}
			case 8:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double value2 = Math.Abs(Blobs[0].Width - Blobs[1].Width);
				value2 = ((!pfile.UseFloat) ? Math.Round(value2, 0) : Math.Round(value2, 3));
				if (value2 < min || value2 > max)
				{
					result = false;
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("AB Width Diff. = {0:N3}{1}, ", value2, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("AB寬度差 = {0:N3}{1}, ", value2, arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("AB宽度差 = {0:N3}{1}, ", value2, arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("AB幅度差 = {0:N3}{1}, ", value2, arg);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("AB Width Diff. = {0}{1}, ", (int)value2, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("AB寬度差 = {0}{1}, ", (int)value2, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("AB宽度差 = {0}{1}, ", (int)value2, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("AB幅度差 = {0}{1}, ", (int)value2, arg);
					}
				}
				else
				{
					result = true;
				}
				Blobs[1].WidthDiff = value2;
				break;
			}
			case 9:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double num = 0.0;
				if (Blobs[0].Width > 0.0 && Blobs[1].Width > 0.0)
				{
					num = ((!(Blobs[0].Width > Blobs[1].Width)) ? (Blobs[1].Width / Blobs[0].Width) : (Blobs[0].Width / Blobs[1].Width));
					num = Math.Round(num, 3);
					if (num < min || num > max)
					{
						result = false;
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("AB Width Ratio. = {0:N3}{1}, ", num, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("AB寬度比 = {0:N3}{1}, ", num, arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("AB宽度比 = {0:N3}{1}, ", num, arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("AB幅度比 = {0:N3}{1}, ", num, arg);
						}
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = false;
				}
				Blobs[1].WidthRatio = num;
				break;
			}
			case 10:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double num4 = 0.0;
				if (Blobs[0].Height > 0.0 && Blobs[1].Height > 0.0)
				{
					num4 = ((!(Blobs[0].Height > Blobs[1].Height)) ? (Blobs[1].Height / Blobs[0].Height) : (Blobs[0].Height / Blobs[1].Height));
					num4 = Math.Round(num4, 3);
					if (num4 < min || num4 > max)
					{
						result = false;
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("AB Height Ratio. = {0:N3}{1}, ", num4, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("AB高度比 = {0:N3}{1}, ", num4, arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("AB高度比 = {0:N3}{1}, ", num4, arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("AB高度比 = {0:N3}{1}, ", num4, arg);
						}
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = false;
				}
				Blobs[1].HeightRatio = num4;
				break;
			}
			case 11:
				AllArea[TrackerIndex] = (double)FinalBinary[TrackerIndex].CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (pfile.UseFloat)
				{
					AllArea[TrackerIndex] = Math.Round(AllArea[TrackerIndex], 3);
				}
				else
				{
					AllArea[TrackerIndex] = Math.Round(AllArea[TrackerIndex], 0);
				}
				if (AllArea[TrackerIndex] < min || AllArea[TrackerIndex] > max)
				{
					result = false;
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("All Area = {0:N3}{1}, ", AllArea[TrackerIndex], arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("所有面積 = {0:N3}{1}, ", AllArea[TrackerIndex], arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("所有面积 = {0:N3}{1}, ", AllArea[TrackerIndex], arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("全体面積 = {0:N3}{1}, ", AllArea[TrackerIndex], arg);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("All Area = {0}{1}, ", (int)AllArea[TrackerIndex], arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("所有面積 = {0}{1}, ", (int)AllArea[TrackerIndex], arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("所有面积 = {0}{1}, ", (int)AllArea[TrackerIndex], arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("全体面積 = {0}{1}, ", (int)AllArea[TrackerIndex], arg);
					}
				}
				else
				{
					result = true;
				}
				break;
			case 12:
			{
				if (AllArea[TrackerIndex] == 0.0)
				{
					AllArea[TrackerIndex] = (double)FinalBinary[TrackerIndex].CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				}
				double num2 = (double)(InspeRect[TrackerIndex].Width * InspeRect[TrackerIndex].Height) * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num2 > 0.0)
				{
					AllAreaRatio[TrackerIndex] = AllArea[TrackerIndex] / num2;
				}
				else
				{
					AllAreaRatio[TrackerIndex] = 0.0;
				}
				AllAreaRatio[TrackerIndex] = Math.Round(AllAreaRatio[TrackerIndex], 3);
				if (AllAreaRatio[TrackerIndex] < min || AllAreaRatio[TrackerIndex] > max)
				{
					result = false;
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("All Area Ratio = {0:N3}{1}, ", AllAreaRatio[TrackerIndex], arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("所有面積比 = {0:N3}{1}, ", AllAreaRatio[TrackerIndex], arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("所有面积比 = {0:N3}{1}, ", AllAreaRatio[TrackerIndex], arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("全体面積比 = {0:N3}{1}, ", AllAreaRatio[TrackerIndex], arg);
					}
				}
				else
				{
					result = true;
				}
				break;
			}
			case 13:
			{
				if (TrackerIndex == 0)
				{
					Blobs[0].TotalAreaDiff = 0.0;
					AllArea[TrackerIndex] = (double)FinalBinary[TrackerIndex].CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
					return true;
				}
				if (pfile.UseFloat)
				{
					AllArea[TrackerIndex] = Math.Round(AllArea[TrackerIndex], 3);
				}
				else
				{
					AllArea[TrackerIndex] = Math.Round(AllArea[TrackerIndex], 0);
				}
				AllArea[TrackerIndex] = (double)FinalBinary[TrackerIndex].CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				double num3 = Math.Abs(AllArea[0] - AllArea[1]);
				Blobs[1].TotalAreaDiff = num3;
				if (num3 < min || num3 > max)
				{
					result = false;
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							errorstr += string.Format("B Area Diff = {0:N3}{1}, ", num3, arg);
						}
						else if (buffers.LanguageType == 1)
						{
							errorstr += string.Format("AB所有面积差 = {0:N3}{1}, ", num3, arg);
						}
						else if (buffers.LanguageType == 2)
						{
							errorstr += string.Format("AB所有面积差 = {0:N3}{1}, ", num3, arg);
						}
						else if (buffers.LanguageType == 3)
						{
							errorstr += string.Format("AB全体面積差 = {0:N3}{1}, ", num3, arg);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("B Area Diff = {0}{1}, ", (int)num3, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("AB所有面积差 = {0}{1}, ", (int)num3, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("AB所有面积差 = {0}{1}, ", (int)num3, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("AB全体面積差 = {0}{1}, ", (int)num3, arg);
					}
				}
				else
				{
					result = true;
				}
				break;
			}
			case 14:
			{
				double val = (double)rect.Width * pfile.PixelPerUM_X;
				double val2 = (double)rect.Height * pfile.PixelPerUM_Y;
				double value = Math.Max(val, val2);
				value = ((!pfile.UseFloat) ? Math.Round(value, 0) : Math.Round(value, 3));
				if (!(value < min) && !(value > max))
				{
					break;
				}
				result = false;
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 0)
					{
						errorstr += string.Format("Longest Side = {0:N3}{1}, ", value, arg);
					}
					else if (buffers.LanguageType == 1)
					{
						errorstr += string.Format("最長邊 = {0:N3}{1}, ", value, arg);
					}
					else if (buffers.LanguageType == 2)
					{
						errorstr += string.Format("最长边 = {0:N3}{1}, ", value, arg);
					}
					else if (buffers.LanguageType == 3)
					{
						errorstr += string.Format("最長辺 = {0:N3}{1}, ", value, arg);
					}
					break;
				}
				if (buffers.LanguageType == 0)
				{
					errorstr += string.Format("Longest Side = {0}{1}, ", (int)value, arg);
				}
				else if (buffers.LanguageType == 1)
				{
					errorstr += string.Format("最長邊 = {0}{1}, ", (int)value, arg);
				}
				if (buffers.LanguageType == 2)
				{
					errorstr += string.Format("最长边 = {0}{1}, ", (int)value, arg);
				}
				if (buffers.LanguageType == 3)
				{
					errorstr += string.Format("最長辺 = {0}{1}, ", (int)value, arg);
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
			string text = "BT04-BinaryTool-GetRule()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-007");
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
	public unsafe bool ScanLineWidth(Mat Binary, Rect roi, Rect ScanWindow, int AllowWindowCount, out int MaxCount, int AllowErrorCount, out int ErrorCount)
	{
		try
		{
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			int num = 0;
			MaxCount = 0;
			bool result = true;
			int width2 = ScanWindow.Width;
			int height = ScanWindow.Height;
			ErrorCount = 0;
			Rect rect = roi;
			roi.Left -= roi.Left;
			roi.Top -= roi.Top;
			roi &= new Rect(0, 0, Binary.Cols - 1, Binary.Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0 || ScanWindow.Width > roi.Width || ScanWindow.Height > roi.Height)
			{
				return true;
			}
			MaxCount = 0;
			for (int i = roi.Top; i < roi.Bottom - height; i += 3)
			{
				for (int j = roi.Left; j < roi.Right - width2; j += 3)
				{
					int num2 = 0;
					for (int k = 0; k < ScanWindow.Height; k++)
					{
						byte* ptr = (byte*)data.ToPointer();
						num = width * (i + k);
						for (int l = 0; l < ScanWindow.Width; l++)
						{
							if (ptr[num + j + l] == byte.MaxValue)
							{
								num2++;
							}
						}
					}
					if (num2 > MaxCount)
					{
						MaxCount = num2;
					}
					if (num2 > AllowWindowCount)
					{
						Rect item = ScanWindow;
						item.Left = rect.Left + j;
						item.Top = rect.Top + i;
						LineWidthRect.Add(item);
						ErrorCount++;
						if (ErrorCount > AllowErrorCount)
						{
							result = false;
						}
					}
				}
			}
			return result;
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
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-008");
			}
			MaxCount = 0;
			ErrorCount = 0;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe bool ScanLineWH(Mat Binary, int AllowWidth, int AllowHeight, out int WErrorCount, out int HErrorCount)
	{
		try
		{
			IntPtr data = Binary.Data;
			int width = Binary.Width;
			_ = Binary.Height;
			int num = 0;
			WErrorCount = 0;
			HErrorCount = 0;
			bool result = true;
			int[] array = new int[width];
			for (int i = 0; i < Binary.Width; i++)
			{
				array[i] = 0;
			}
			for (int j = 0; j < Binary.Height; j++)
			{
				byte* ptr = (byte*)data.ToPointer();
				num = width * j;
				int num2 = 0;
				for (int k = 0; k < Binary.Width; k++)
				{
					if (ptr[num + k] == byte.MaxValue)
					{
						num2++;
						array[k]++;
					}
				}
				if (num2 > AllowWidth)
				{
					WErrorCount++;
				}
			}
			for (int l = 0; l < Binary.Width; l++)
			{
				if (array[l] > AllowHeight)
				{
					HErrorCount++;
				}
			}
			return result;
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
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-009");
			}
			WErrorCount = 0;
			HErrorCount = 0;
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
			string text = "BT11-BinaryTool-GetComDigitsValue()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BIN-010");
			}
			return 0.0;
		}
	}

	private void doThiningImage(Mat mat)
	{
		Mat mat2 = new Mat();
		Cv2.Threshold(mat, mat2, 120.0, 255.0, ThresholdTypes.Binary);
		Mat dst = mat2.Clone();
		sw.Start();
		CVThinImage(mat2, ref dst, 10);
		Mat mat3 = new Mat();
		Cv2.DistanceTransform(mat2, mat3, DistanceTypes.L2, DistanceMaskSize.Mask3);
		Mat mat4 = new Mat();
		Cv2.Normalize(mat3, mat4, 255.0, 0.0, NormTypes.MinMax);
		Point minLoc = new Point(0, 0);
		Point maxLoc = new Point(0, 0);
		Cv2.MinMaxLoc((InputArray)mat3, out minLoc, out maxLoc);
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

	private unsafe void CVThinImage(Mat src, ref Mat dst, int intera)
	{
		int num = src.Cols - 1;
		int num2 = src.Rows - 1;
		int num3 = (int)src.Step();
		_ = src.Data;
		Mat mat = new Mat();
		for (int i = 0; i < intera; i++)
		{
			dst.CopyTo(mat);
			bool flag = false;
			IntPtr data = mat.Data;
			IntPtr data2 = dst.Data;
			for (int j = 1; j < num2; j++)
			{
				data += num3;
				data2 += num3;
				for (int k = 1; k < num; k++)
				{
					byte* ptr = (byte*)(data + k).ToPointer();
					byte* ptr2 = (byte*)(data2 + k).ToPointer();
					int num4 = 0;
					if (*ptr > 0)
					{
						if (ptr[-num3] == 0 && ptr[-num3 + 1] > 0)
						{
							num4++;
						}
						if (ptr[-num3 + 1] == 0 && ptr[1] > 0)
						{
							num4++;
						}
						if (ptr[1] == 0 && ptr[num3 + 1] > 0)
						{
							num4++;
						}
						if (ptr[num3 + 1] == 0 && ptr[num3] > 0)
						{
							num4++;
						}
						if (ptr[num3] == 0 && ptr[num3 - 1] > 0)
						{
							num4++;
						}
						if (ptr[num3 - 1] == 0 && ptr[-1] > 0)
						{
							num4++;
						}
						if (ptr[-1] == 0 && ptr[-num3 - 1] > 0)
						{
							num4++;
						}
						if (ptr[-num3 - 1] == 0 && ptr[-num3] > 0)
						{
							num4++;
						}
						int num5 = ((ptr[-num3] > 0) ? 1 : 0);
						int num6 = ((ptr[-num3 + 1] > 0) ? 1 : 0);
						int num7 = ((ptr[1] > 0) ? 1 : 0);
						int num8 = ((ptr[num3 + 1] > 0) ? 1 : 0);
						int num9 = ((ptr[num3] > 0) ? 1 : 0);
						int num10 = ((ptr[num3 - 1] > 0) ? 1 : 0);
						int num11 = ((ptr[-1] > 0) ? 1 : 0);
						int num12 = ((ptr[-num3 - 1] > 0) ? 1 : 0);
						if (num5 + num6 + num7 + num8 + num9 + num10 + num11 + num12 > 1 && num5 + num6 + num7 + num8 + num9 + num10 + num11 + num12 < 7 && num4 == 1 && (num5 == 0 || num7 == 0 || num9 == 0) && (num7 == 0 || num9 == 0 || num11 == 0))
						{
							*ptr2 = 0;
							flag = true;
						}
					}
				}
			}
			dst.CopyTo(mat);
			data = mat.Data;
			data2 = dst.Data;
			for (int j = 1; j < num2; j++)
			{
				data += num3;
				data2 += num3;
				for (int k = 1; k < num; k++)
				{
					int num4 = 0;
					byte* ptr3 = (byte*)(data + k).ToPointer();
					byte* ptr4 = (byte*)(data2 + k).ToPointer();
					if (*ptr3 > 0)
					{
						if (ptr3[-num3] == 0 && ptr3[-num3 + 1] > 0)
						{
							num4++;
						}
						if (ptr3[-num3 + 1] == 0 && ptr3[1] > 0)
						{
							num4++;
						}
						if (ptr3[1] == 0 && ptr3[num3 + 1] > 0)
						{
							num4++;
						}
						if (ptr3[num3 + 1] == 0 && ptr3[num3] > 0)
						{
							num4++;
						}
						if (ptr3[num3] == 0 && ptr3[num3 - 1] > 0)
						{
							num4++;
						}
						if (ptr3[num3 - 1] == 0 && ptr3[-1] > 0)
						{
							num4++;
						}
						if (ptr3[-1] == 0 && ptr3[-num3 - 1] > 0)
						{
							num4++;
						}
						if (ptr3[-num3 - 1] == 0 && ptr3[-num3] > 0)
						{
							num4++;
						}
						int num5 = ((ptr3[-num3] > 0) ? 1 : 0);
						int num6 = ((ptr3[-num3 + 1] > 0) ? 1 : 0);
						int num7 = ((ptr3[1] > 0) ? 1 : 0);
						int num8 = ((ptr3[num3 + 1] > 0) ? 1 : 0);
						int num9 = ((ptr3[num3] > 0) ? 1 : 0);
						int num10 = ((ptr3[num3 - 1] > 0) ? 1 : 0);
						int num11 = ((ptr3[-1] > 0) ? 1 : 0);
						int num12 = ((ptr3[-num3 - 1] > 0) ? 1 : 0);
						if (num5 + num6 + num7 + num8 + num9 + num10 + num11 + num12 > 1 && num5 + num6 + num7 + num8 + num9 + num10 + num11 + num12 < 7 && num4 == 1 && (num5 == 0 || num7 == 0 || num11 == 0) && (num5 == 0 || num9 == 0 || num11 == 0))
						{
							*ptr4 = 0;
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				break;
			}
		}
	}

	private void GetSoftCorrectionShiftValue(int channel, ref int min, ref int max)
	{
		switch (channel)
		{
		case 0:
			if (min != 0)
			{
				min += pfile.SoftShift_R;
			}
			if (max != 255)
			{
				max += pfile.SoftShift_R;
			}
			break;
		case 1:
			if (min != 0)
			{
				min += pfile.SoftShift_G;
			}
			if (max != 255)
			{
				max += pfile.SoftShift_G;
			}
			break;
		default:
			if (min != 0)
			{
				min += pfile.SoftShift_B;
			}
			if (max != 255)
			{
				max += pfile.SoftShift_B;
			}
			break;
		}
		if (min < 0)
		{
			min = 0;
		}
		if (min > 254)
		{
			min = 254;
		}
		if (max < 1)
		{
			max = 1;
		}
		if (max > 255)
		{
			max = 255;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static List<ConnectedComponent> FindConnectedComponents(Mat binary)
	{
		int cols = binary.Cols;
		int rows = binary.Rows;
		int num = 1;
		int[,] array = new int[rows, cols];
		List<ConnectedComponent> list = new List<ConnectedComponent>();
		try
		{
			byte* ptr = (byte*)binary.Data.ToPointer();
			int num2 = (int)binary.Step();
			for (int i = 0; i < rows; i++)
			{
				byte* ptr2 = ptr + i * num2;
				for (int j = 0; j < cols; j++)
				{
					if (ptr2[j] == 0)
					{
						continue;
					}
					int num3 = ((j > 0) ? array[i, j - 1] : 0);
					int num4 = ((i > 0) ? array[i - 1, j] : 0);
					if (num3 == 0 && num4 == 0)
					{
						array[i, j] = num;
						list.Add(new ConnectedComponent
						{
							Label = num,
							Area = 1,
							BoundingBox = new Rect(j, i, 1, 1)
						});
						num++;
					}
					else
					{
						array[i, j] = Math.Max(num3, num4);
						if (num3 != 0 && num4 != 0 && num3 != num4)
						{
							MergeComponents(num3, num4, list);
						}
					}
				}
			}
			CompressLabels(list);
			return list;
		}
		catch (Exception ex)
		{
			_ = "BT01-BinaryTool-FindConnectedComponents()-01-" + ex.ToString();
			return list;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void MergeComponents(int labelA, int labelB, List<ConnectedComponent> components)
	{
		try
		{
			ConnectedComponent connectedComponent = components.Find((ConnectedComponent c) => c.Label == labelA);
			ConnectedComponent connectedComponent2 = components.Find((ConnectedComponent c) => c.Label == labelB);
			if (connectedComponent == null || connectedComponent2 == null)
			{
				return;
			}
			connectedComponent.Area += connectedComponent2.Area;
			connectedComponent.BoundingBox = connectedComponent.BoundingBox.Union(connectedComponent2.BoundingBox);
			foreach (ConnectedComponent component in components)
			{
				if (component.Label == labelB)
				{
					component.Label = labelA;
				}
			}
			components.RemoveAll((ConnectedComponent c) => c.Label == labelB);
		}
		catch (Exception ex)
		{
			_ = "BT01-BinaryTool-MergeComponents()-01-" + ex.ToString();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CompressLabels(List<ConnectedComponent> components)
	{
		int num = 1;
		try
		{
			foreach (ConnectedComponent component in components)
			{
				component.Label = num;
				num++;
			}
		}
		catch (Exception ex)
		{
			_ = "BT01-BinaryTool-CompressLabels()-01-" + ex.ToString();
		}
	}

	public Mat GetFinalBinary(int id)
	{
		return FinalBinary[id];
	}

	public Rect GetInspectRect(int index)
	{
		return InspeRect[index];
	}
}
