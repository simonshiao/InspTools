using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class BlockAvgTool
{
	private const int MaxTracker = 2;

	private CVBuffer buffers;

	public Mat[] Binary = new Mat[2];

	private Mat[] doThdBinary = new Mat[2];

	private Mat[] SelectMat = new Mat[2];

	private Mat[] SelectMatBinary = new Mat[2];

	private Mat BinaryTemp1 = new Mat();

	private Mat BinaryTemp2 = new Mat();

	public Mat[] WholeMask = new Mat[2];

	private Mat MaskTemp = new Mat();

	private Mat AvgUpBinary = new Mat();

	private Mat AvgDownBinary = new Mat();

	private Mat RowMat = new Mat();

	private Mat RowRangeMat = new Mat();

	private Mat ColMat = new Mat();

	private Mat ColRangeMat = new Mat();

	private const int RowShiftValue = 10;

	private ParamFile pfile;

	private Rect[] InspeRect = new Rect[2];

	public BlobStruct[] Blobs = new BlobStruct[2];

	public List<SimpleBlobStruct> BlobLine1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> BlobLine2 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob2 = new List<SimpleBlobStruct>();

	private int BlockAvgIndex;

	public int[] AvgGray = new int[2];

	public bool[] AvgGrayResult = new bool[2];

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool TrackerError { get; set; }

	public bool BlockAvgSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public string Tracker2ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public BlockAvgTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect[0] = new Rect(0, 0, 0, 0);
		InspeRect[1] = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 2; i++)
		{
			Binary[i] = new Mat();
			SelectMat[i] = new Mat();
			doThdBinary[i] = new Mat();
			SelectMatBinary[i] = new Mat();
			WholeMask[i] = new Mat();
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

	public void GetInspeRect(int index)
	{
		LocateRect = buffers.LocatedRect;
		if (pfile.BlockAvgParam[index].Tracker1UseAutoSize)
		{
			InspeRect[0].X = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize1_SelectIndex].L + pfile.BlockAvgParam[index].Track1_L;
			InspeRect[0].Y = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize1_SelectIndex].T + pfile.BlockAvgParam[index].Track1_T;
			InspeRect[0].Width = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize1_SelectIndex].Width - (pfile.BlockAvgParam[index].Track1_L + pfile.BlockAvgParam[index].Track1_R);
			InspeRect[0].Height = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize1_SelectIndex].Height - (pfile.BlockAvgParam[index].Track1_T + pfile.BlockAvgParam[index].Track1_B);
		}
		else
		{
			int width = pfile.BlockAvgParam[index].Track1_R - pfile.BlockAvgParam[index].Track1_L;
			int height = pfile.BlockAvgParam[index].Track1_B - pfile.BlockAvgParam[index].Track1_T;
			InspeRect[0].X = pfile.VSLocateParam[pfile.BlockAvgParam[index].Tracker1AligmentXIndex].Value + pfile.BlockAvgParam[index].Track1_L;
			InspeRect[0].Y = pfile.HSLocateParam[pfile.BlockAvgParam[index].Tracker1AligmentYIndex].Value + pfile.BlockAvgParam[index].Track1_T;
			InspeRect[0].Width = width;
			InspeRect[0].Height = height;
		}
		if (pfile.BlockAvgParam[index].Tracker2UseAutoSize)
		{
			InspeRect[1].X = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize2_SelectIndex].L + pfile.BlockAvgParam[index].Track2_L;
			InspeRect[1].Y = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize2_SelectIndex].T + pfile.BlockAvgParam[index].Track2_T;
			InspeRect[1].Width = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize2_SelectIndex].Width - (pfile.BlockAvgParam[index].Track2_L + pfile.BlockAvgParam[index].Track2_R);
			InspeRect[1].Height = pfile.AdvLocateParam[pfile.BlockAvgParam[index].AutoSize2_SelectIndex].Height - (pfile.BlockAvgParam[index].Track2_T + pfile.BlockAvgParam[index].Track2_B);
		}
		else
		{
			int width2 = pfile.BlockAvgParam[index].Track2_R - pfile.BlockAvgParam[index].Track2_L;
			int height2 = pfile.BlockAvgParam[index].Track2_B - pfile.BlockAvgParam[index].Track2_T;
			InspeRect[1].X = pfile.VSLocateParam[pfile.BlockAvgParam[index].Tracker2AligmentXIndex].Value + pfile.BlockAvgParam[index].Track2_L;
			InspeRect[1].Y = pfile.HSLocateParam[pfile.BlockAvgParam[index].Tracker2AligmentYIndex].Value + pfile.BlockAvgParam[index].Track2_T;
			InspeRect[1].Width = width2;
			InspeRect[1].Height = height2;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe Mat filterNoise(Mat mat, int w, int h)
	{
		try
		{
			if ((w <= 0) & (h <= 0))
			{
				return mat;
			}
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(mat, mat2, mat3, mat4, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat3.GetGenericIndexer<int>();
			if (num <= 0)
			{
				return mat;
			}
			bool[] array = new bool[num];
			array[0] = false;
			for (int i = 1; i < num; i++)
			{
				if (genericIndexer[i, 2] <= w && genericIndexer[i, 3] <= h)
				{
					array[i] = true;
				}
				else
				{
					array[i] = false;
				}
			}
			IntPtr data = mat2.Data;
			IntPtr data2 = mat.Data;
			int cols = mat.Cols;
			int rows = mat.Rows;
			byte* ptr = (byte*)data.ToPointer();
			byte* ptr2 = (byte*)data2.ToPointer();
			int num2 = 0;
			int num3 = 0;
			for (int j = 0; j < rows; j++)
			{
				for (int k = 0; k < cols; k++)
				{
					int num4 = ptr[num3];
					if (array[num4])
					{
						ptr2[num2] = 0;
					}
					num3 += 4;
					num2++;
				}
			}
			return mat;
		}
		catch (Exception ex)
		{
			string text = "DT01-BlockAvgTool-filterNoise()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-001");
			}
			return mat;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Mat filterNoise2(Mat mat, int w, int h)
	{
		try
		{
			if ((w <= 0) & (h <= 0))
			{
				return mat;
			}
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(mat, mat2, mat3, mat4, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat3.GetGenericIndexer<int>();
			if (num <= 0)
			{
				return mat;
			}
			(new bool[num])[0] = false;
			for (int i = 1; i < num; i++)
			{
				if (genericIndexer[i, 2] <= w && genericIndexer[i, 3] <= h)
				{
					Rect roi = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
					using Mat mat5 = new Mat(mat, roi);
					mat5.SetTo(0.0);
				}
			}
			return mat;
		}
		catch (Exception ex)
		{
			string text = "DT01-BlockAvgTool-filterNoise()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-002");
			}
			return mat;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlobLine(Mat binary, int index, int TrackerIndex, ref List<SimpleBlobStruct> BlobLine)
	{
		bool result = true;
		Mat mat = new Mat();
		Mat mat2 = new Mat();
		Mat mat3 = new Mat();
		int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
		Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
		string text = ((TrackerIndex != 0) ? "B" : "A");
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
				BlobLine.Add(simpleBlobStruct);
			}
			switch (pfile.BlockAvgParam[index].BlobLineSort)
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
			for (int num3 = 0; num3 < BlobLine.Count && num3 < pfile.BlockAvgParam[index].BlobLineNum; num3++)
			{
				bool flag = true;
				if (BlobLine[num3].Width < pfile.BlockAvgParam[index].BlobLineWidthMin || BlobLine[num3].Width > pfile.BlockAvgParam[index].BlobLineWidthMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Height < pfile.BlockAvgParam[index].BlobLineHeightMin || BlobLine[num3].Height > pfile.BlockAvgParam[index].BlobLineHeightMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Area < pfile.BlockAvgParam[index].BlobLineAreaMin || BlobLine[num3].Area > pfile.BlockAvgParam[index].BlobLineAreaMax)
				{
					flag = false;
				}
				BlobLine[num3].result = flag;
				if (flag)
				{
					num2++;
				}
				if (!flag)
				{
					string text2 = "";
					text2 = ((buffers.LanguageType == 1) ? (text2 + string.Format("[{0}:{1}][{2}] 線條寬度{3}, 線條高度{4}, 線條面積{5} [NG] \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)) : ((buffers.LanguageType == 2) ? (text2 + string.Format("[{0}:{1}][{2}] 线条宽度{3}, 线条高度{4}, 线条面积{5} [NG] \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)) : ((buffers.LanguageType != 3) ? (text2 + string.Format("[{0}:{1}][{2}] Line Width={3}, Line Height={4}, Line Area={5}  [NG] \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)) : (text2 + string.Format("[{0}:{1}][{2}] 線幅{3}, 線高{4}, 線面積{5} [NG] \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)))));
					if (TrackerIndex == 0)
					{
						Tracker1ResultText += text2;
					}
					else
					{
						Tracker2ResultText += text2;
					}
				}
			}
			if (num2 < pfile.BlockAvgParam[index].BlobLineMin || num2 > pfile.BlockAvgParam[index].BlobLineMax)
			{
				result = false;
			}
			string text3 = "";
			text3 = ((buffers.LanguageType == 1) ? (text3 + string.Format("[{0}:{1}][{2}] 線條合格數量為{3}，允許最小合格數為{4}，允許最大合格數為{5} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, num2, pfile.BlockAvgParam[index].BlobLineMin, pfile.BlockAvgParam[index].BlobLineMax)) : ((buffers.LanguageType == 2) ? (text3 + string.Format("[{0}:{1}][{2}] 线条合格数量为{3}，允许最小合格数为{4}，允许最大合格数为{5} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, num2, pfile.BlockAvgParam[index].BlobLineMin, pfile.BlockAvgParam[index].BlobLineMax)) : ((buffers.LanguageType != 3) ? (text3 + string.Format("[{0}:{1}][{2}] Passed Line={3}，Min Passs Line={4}，Max Passs Line={5} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, num2, pfile.BlockAvgParam[index].BlobLineMin, pfile.BlockAvgParam[index].BlobLineMax)) : (text3 + string.Format("[{0}:{1}][{2}] 線の合格数は{3}個、最小合格数は{4}個、最大合格数は{5}個です \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text, num2, pfile.BlockAvgParam[index].BlobLineMin, pfile.BlockAvgParam[index].BlobLineMax)))));
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
		else if (BlobLine.Count < pfile.BlockAvgParam[index].BlobLineNum)
		{
			result = false;
			Blobs[TrackerIndex].result = false;
			string text4 = "";
			text4 = ((buffers.LanguageType == 1) ? (text4 + string.Format("[{0}:{1}][{2}] 線條數量為0 \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text)) : ((buffers.LanguageType == 2) ? (text4 + string.Format("[{0}:{1}][{2}] 线条数量为0 \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text)) : ((buffers.LanguageType != 3) ? (text4 + string.Format("[{0}:{1}][{2}] Passed Line = 0  \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text)) : (text4 + string.Format("[{0}:{1}][{2}] 線の数は0です \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text)))));
			if (TrackerIndex == 0)
			{
				Tracker1ResultText += text4;
			}
			else
			{
				Tracker2ResultText += text4;
			}
			return result;
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
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num2 = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
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
				num3 = (num = genericIndexer[i, 4]);
			}
			else
			{
				rect = new Rect(0, 0, 0, 0);
				num = num3;
			}
			double num4 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			Blobs[TrackerIndex].Width = rect.Width;
			Blobs[TrackerIndex].Height = rect.Height;
			Blobs[TrackerIndex].WHRatio = num4;
			Blobs[TrackerIndex].Area = num3;
			Blobs[TrackerIndex].NGRect = rect;
			Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
			Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
			return checkResult(rect, num, num4, TrackerIndex);
		}
		catch (Exception ex)
		{
			string text = "DT02-BlockAvgTool-findBlob()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-003");
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
			array[0] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].InspRule1_Index, pfile.BlockAvgParam[BlockAvgIndex].InspRule1_Min, pfile.BlockAvgParam[BlockAvgIndex].InspRule1_Max, rect, area, TrackerIndex);
			array[1] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].InspRule2_Index, pfile.BlockAvgParam[BlockAvgIndex].InspRule2_Min, pfile.BlockAvgParam[BlockAvgIndex].InspRule2_Max, rect, area, TrackerIndex);
			array[2] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].InspRule3_Index, pfile.BlockAvgParam[BlockAvgIndex].InspRule3_Min, pfile.BlockAvgParam[BlockAvgIndex].InspRule3_Max, rect, area, TrackerIndex);
			array[3] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].InspRule4_Index, pfile.BlockAvgParam[BlockAvgIndex].InspRule4_Min, pfile.BlockAvgParam[BlockAvgIndex].InspRule4_Max, rect, area, TrackerIndex);
			if (pfile.BlockAvgParam[BlockAvgIndex].InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.BlockAvgParam[BlockAvgIndex].InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.BlockAvgParam[BlockAvgIndex].InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.BlockAvgParam[BlockAvgIndex].InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.BlockAvgParam[BlockAvgIndex].InspRule4_Index > BlobCondition.EM_NOT_USE)
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
			if (!flag)
			{
				if (TrackerIndex == 0)
				{
					if (pfile.MeasureUnit == 0)
					{
						if (pfile.UseFloat)
						{
							if (buffers.LanguageType == 1)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3} um, 高さ:{4:N3} um, 面積:{5:N3} um, 縦横比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
							else
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
						}
						else if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3} um, 高さ:{4} um, 面積:{5} um, 縦横比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
					}
					else if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3} mm, 高さ:{4} mm, 面積:{5} mm, 縦横比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}mm, 高さ:{4}mm, 面積:{5}mm, 縦横比:{6:N3}, 面積比:{7:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
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
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9:N3}, AB Width Diff.:{10:N3}, AB Width Ratio:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9:N3}, AB寬度差:{10:N3}, AB寬度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9:N3}, AB宽度差:{10:N3}, AB宽度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅：{3}µm, 高さ：{4}µm, 面積：{5}µm², 縦横比：{6:N3}, 面積比：{7:N3}, AB面積比：{8:N3}, AB高さ差：{9:N3}, AB幅差：{10:N3}, AB幅比：{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
						}
						else if (buffers.LanguageType == 0)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9}, AB Width Diff.:{10}, AB Width Ratio:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9}, AB寬度差:{10}, AB寬度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅：{3}µm, 高さ：{4}µm, 面積：{5}µm², 縦横比：{6:N3}, 面積比：{7:N3}, AB面積比：{8:N3}, AB高さ差：{9}, AB幅差：{10}, AB幅比：{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅：{3} µm, 高さ：{4} µm, 面積：{5} µm², 縦横比：{6:N3}, 面積比：{7:N3}, AB面積比：{8:N3}, AB高さ差：{9} µm, AB幅差：{10} µm, AB幅比：{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
					}
					else if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9:N3}, AB Width Diff.:{10:N3}, AB Width Ratio:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9:N3}, AB寬度差:{10:N3}, AB寬度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9:N3}, AB宽度差:{10:N3}, AB宽度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅：{3}mm, 高さ：{4}mm, 面積：{5}mm², 縦横比：{6:N3}, 面積比：{7:N3}, AB面積比：{8:N3}, AB高さ差：{9:N3}, AB幅差：{10:N3}, AB幅比：{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9}, AB Width Diff.:{10}, AB Width Ratio:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9}, AB寬度差:{10}, AB寬度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9}, AB宽度差:{10}, AB宽度比:{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅：{3} mm, 高さ：{4} mm, 面積：{5} mm², 縦横比：{6:N3}, 面積比：{7:N3}, AB面積比：{8:N3}, AB高さ差：{9} mm, AB幅差：{10} mm, AB幅比：{11:N3} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text = "DT03-BlockAvgTool-checkResult()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-004");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, double min, double max, Rect rect, double area, int TrackerIndex = 0)
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
				double num4 = (double)rect.Width * pfile.PixelPerUM_X;
				if (num4 < min || num4 > max)
				{
					result = false;
				}
				break;
			}
			case 2:
			{
				double num2 = (double)rect.Height * pfile.PixelPerUM_Y;
				if (num2 < min || num2 > max)
				{
					result = false;
				}
				break;
			}
			case 3:
			{
				double num7 = area * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num7 < min || num7 > max)
				{
					result = false;
				}
				break;
			}
			case 4:
			{
				double num5 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				if (num5 < min || num5 > max)
				{
					result = false;
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
				}
				else
				{
					areaRatio = area / (double)(rect.Width * rect.Height);
					Blobs[TrackerIndex].AreaRatio = areaRatio;
					result = ((!(areaRatio < min) && !(areaRatio > max)) ? true : false);
				}
				break;
			}
			case 6:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double num6 = 0.0;
				if (Blobs[0].Area > 0.0 && Blobs[1].Area > 0.0)
				{
					num6 = ((!(Blobs[0].Area > Blobs[1].Area)) ? (Blobs[1].Area / Blobs[0].Area) : (Blobs[0].Area / Blobs[1].Area));
					Blobs[1].TwoAreaRatio = num6;
					result = ((!(num6 < min) && !(num6 > max)) ? true : false);
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
				double num8 = Math.Abs(Blobs[0].Height - Blobs[1].Height);
				num8 *= pfile.PixelPerUM_Y;
				result = ((!(num8 < min) && !(num8 > max)) ? true : false);
				Blobs[1].HeightDiff = num8;
				break;
			}
			case 8:
			{
				if (TrackerIndex == 0)
				{
					return true;
				}
				double num3 = Math.Abs(Blobs[0].Width - Blobs[1].Width);
				num3 *= pfile.PixelPerUM_X;
				result = ((!(num3 < min) && !(num3 > max)) ? true : false);
				Blobs[1].WidthDiff = num3;
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
					result = ((!(num < min) && !(num > max)) ? true : false);
				}
				else
				{
					result = false;
				}
				Blobs[1].WidthRatio = num;
				break;
			}
			default:
				return true;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "SMT04-SmoothTool-GetRule()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-005");
			}
			return false;
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
	private void CreateKernal(ref Mat KernalH, ref Mat KernalV, int FilterCountH, int FilterCountV)
	{
		try
		{
			switch (FilterCountH)
			{
			case 0:
			{
				float[,] array = new float[1, 1];
				for (int j = 0; j < 1; j++)
				{
					array[0, j] = 1f;
				}
				KernalH = new Mat(1, 1, MatType.CV_32FC1, array, 0L);
				break;
			}
			case 1:
			{
				float[,] array = new float[1, 3];
				for (int n = 0; n < 3; n++)
				{
					array[0, n] = 1f / 3f;
				}
				KernalH = new Mat(1, 3, MatType.CV_32FC1, array, 0L);
				break;
			}
			case 2:
			{
				float[,] array = new float[1, 5];
				for (int k = 0; k < 5; k++)
				{
					array[0, k] = 0.2f;
				}
				KernalH = new Mat(1, 5, MatType.CV_32FC1, array, 0L);
				break;
			}
			case 3:
			{
				float[,] array = new float[1, 8];
				for (int m = 0; m < 8; m++)
				{
					array[0, m] = 0.125f;
				}
				KernalH = new Mat(1, 8, MatType.CV_32FC1, array, 0L);
				break;
			}
			case 4:
			{
				float[,] array = new float[1, 11];
				for (int num = 0; num < 11; num++)
				{
					array[0, num] = 1f / 11f;
				}
				KernalH = new Mat(1, 11, MatType.CV_32FC1, array, 0L);
				break;
			}
			case 5:
			{
				float[,] array = new float[1, 15];
				for (int l = 0; l < 15; l++)
				{
					array[0, l] = 1f / 15f;
				}
				KernalH = new Mat(1, 15, MatType.CV_32FC1, array, 0L);
				break;
			}
			default:
			{
				float[,] array = new float[1, 5];
				for (int i = 0; i < 5; i++)
				{
					array[0, i] = 0.2f;
				}
				KernalH = new Mat(1, 5, MatType.CV_32FC1, array, 0L);
				break;
			}
			}
			switch (FilterCountV)
			{
			case 0:
			{
				float[,] array2 = new float[1, 1];
				for (int num3 = 0; num3 < 1; num3++)
				{
					array2[num3, 0] = 1f;
				}
				KernalV = new Mat(1, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			case 1:
			{
				float[,] array2 = new float[3, 1];
				for (int num7 = 0; num7 < 3; num7++)
				{
					array2[num7, 0] = 1f / 3f;
				}
				KernalV = new Mat(3, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			case 2:
			{
				float[,] array2 = new float[5, 1];
				for (int num4 = 0; num4 < 5; num4++)
				{
					array2[num4, 0] = 0.2f;
				}
				KernalV = new Mat(5, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			case 3:
			{
				float[,] array2 = new float[8, 1];
				for (int num6 = 0; num6 < 8; num6++)
				{
					array2[num6, 0] = 0.125f;
				}
				KernalV = new Mat(8, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			case 4:
			{
				float[,] array2 = new float[11, 1];
				for (int num8 = 0; num8 < 11; num8++)
				{
					array2[num8, 0] = 1f / 11f;
				}
				KernalV = new Mat(11, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			case 5:
			{
				float[,] array2 = new float[15, 1];
				for (int num5 = 0; num5 < 15; num5++)
				{
					array2[num5, 0] = 1f / 15f;
				}
				KernalV = new Mat(15, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			default:
			{
				float[,] array2 = new float[5, 1];
				for (int num2 = 0; num2 < 5; num2++)
				{
					array2[num2, 0] = 0.2f;
				}
				KernalV = new Mat(5, 1, MatType.CV_32FC1, array2, 0L);
				break;
			}
			}
		}
		catch (Exception ex)
		{
			string text = "DT05-BlockAvgTool-CreateKernal()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-006");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		OffLine = offline;
		BlockAvgSuccess = true;
		Tracker1ResultText = "";
		Tracker2ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		TrackerError = false;
		if (!pfile.BlockAvgParam[index].TrackerON)
		{
			iresult = TOOL_RESULT.EM_NOT_USE;
			return true;
		}
		if (FaceIndex != 0 && pfile.BlockAvgParam[index].EnableSelectFace && FaceIndex != pfile.BlockAvgParam[index].FaceIndex)
		{
			iresult = TOOL_RESULT.EM_OK;
			return true;
		}
		BlockAvgIndex = index;
		GetInspeRect(index);
		_ = new bool[2];
		int channelIndex = (int)pfile.BlockAvgParam[index].ChannelIndex;
		bool[] array = new bool[2];
		_ = ref pfile.BlockAvgParam[index];
		_ = ref pfile.BlockAvgParam[index];
		int tHDLimit_Min = pfile.BlockAvgParam[index].THDLimit_Min;
		int tHDLimit_Max = pfile.BlockAvgParam[index].THDLimit_Max;
		int num = 0;
		try
		{
			BlobLine1.Clear();
			BlobLine2.Clear();
			MultiBlob1.Clear();
			MultiBlob2.Clear();
			for (int i = 0; i < 2; i++)
			{
				AvgGray[i] = 0;
				AvgGrayResult[i] = true;
				if (!pfile.BlockAvgParam[index].ThdDownON && !pfile.BlockAvgParam[index].ThdUpON)
				{
					Blobs[i].Width = (Blobs[i].Height = (Blobs[i].Area = 0.0));
					Blobs[i].AreaRatio = (Blobs[i].WHRatio = 0.0);
					Blobs[i].NGRect = new Rect(0, 0, 0, 0);
					Blobs[i].result = true;
					Binary[i].SetTo(0.0);
					array[i] = true;
					continue;
				}
				Blobs[i].Width = (Blobs[i].Height = (Blobs[i].Area = 0.0));
				Blobs[i].AreaRatio = (Blobs[i].WHRatio = 0.0);
				Blobs[i].NGRect = new Rect(0, 0, 0, 0);
				Blobs[i].result = true;
				array[i] = true;
				if (i == 1 && !pfile.BlockAvgParam[index].SearchRect2ON)
				{
					continue;
				}
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = true;
					if (i == 0)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					TrackerError = true;
					if (pfile.BlockAvgParam[index].TrackErrorSetNG)
					{
						array[i] = false;
						Blobs[i].result = false;
					}
					else
					{
						array[i] = true;
					}
					Binary[i].SetTo(0.0);
					continue;
				}
				InspeRect[i] &= new Rect(0, 0, buffers.GetChannelMat(channelIndex).Cols, buffers.GetChannelMat(channelIndex).Rows);
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = true;
					if (i == 0)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					TrackerError = true;
					if (pfile.BlockAvgParam[index].TrackErrorSetNG)
					{
						array[i] = false;
						Blobs[i].result = false;
					}
					else
					{
						array[i] = true;
					}
					Binary[i].SetTo(0.0);
					continue;
				}
				num = 1;
				int p2PIntervalX = pfile.BlockAvgParam[index].P2PIntervalX;
				int p2PIntervalY = pfile.BlockAvgParam[index].P2PIntervalY;
				if (InspeRect[i].Width <= p2PIntervalX && !pfile.BlockAvgParam[index].UseAvgBinary)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					if (i == 0)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Width too small! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬小於計算範圍! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽小于计算范围! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅が計算範囲より小さいです! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Width too small! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬小於計算範圍! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽小于计算范围 \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅が計算範囲より小さいです! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					TrackerError = true;
					array[i] = true;
					Binary[i].SetTo(0.0);
					continue;
				}
				if (InspeRect[i].Height <= p2PIntervalY && !pfile.BlockAvgParam[index].UseAvgBinary)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					if (i == 0)
					{
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Height too small! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域高小於計算範圍! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域高小于计算范围! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の高さが計算範囲より小さいです! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 0)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Height too small! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域高小於計算範圍! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域高小于计算范围! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の高さが計算範囲より小さいです! \n", index + 1, pfile.BlockAvgParam[index].TrackerName, "B");
					}
					TrackerError = true;
					array[i] = true;
					Binary[i].SetTo(0.0);
					continue;
				}
				num = 2;
				int channelIndex2 = (int)pfile.BlockAvgParam[index].ChannelIndex;
				new Mat(buffers.GetChannelMat(channelIndex2), InspeRect[i]).CopyTo(SelectMat[i]);
				int thdUp = pfile.BlockAvgParam[index].ThdUp;
				int thdDown = pfile.BlockAvgParam[index].ThdDown;
				GetMask(ref WholeMask[i], index, i);
				if (!pfile.BlockAvgParam[index].UseAvgBinary)
				{
					num = 3;
					Cv2.Blur(SelectMat[i], SelectMatBinary[i], new Size(p2PIntervalX, p2PIntervalY));
					num = 4;
					if (pfile.BlockAvgParam[index].ThdDownON && pfile.BlockAvgParam[index].ThdUpON)
					{
						Cv2.Subtract(SelectMat[i], SelectMatBinary[i], Binary[i]);
						Cv2.InRange(Binary[i], thdUp, 255.0, Binary[i]);
						Cv2.Subtract(SelectMatBinary[i], SelectMat[i], BinaryTemp2);
						Cv2.InRange(BinaryTemp2, thdDown, 255.0, BinaryTemp1);
						Cv2.BitwiseOr(Binary[i], BinaryTemp1, Binary[i]);
					}
					else if (pfile.BlockAvgParam[index].ThdDownON)
					{
						Cv2.Subtract(SelectMatBinary[i], SelectMat[i], Binary[i]);
						Cv2.InRange(Binary[i], thdDown, 255.0, Binary[i]);
					}
					else
					{
						if (!pfile.BlockAvgParam[index].ThdUpON)
						{
							iresult = TOOL_RESULT.EM_NOT_USE;
							Binary[i].SetTo(0.0);
							return true;
						}
						Cv2.Subtract(SelectMat[i], SelectMatBinary[i], Binary[i]);
						Cv2.InRange(Binary[i], thdUp, 255.0, Binary[i]);
					}
					if (WholeMask[i].Width == Binary[i].Width && WholeMask[i].Height == Binary[i].Height)
					{
						Cv2.BitwiseAnd(Binary[i], WholeMask[i], Binary[i]);
					}
				}
				else
				{
					Binary[i] = MM.CloneFrom(SelectMat[i], Binary[i]);
					TimeStart();
					if (pfile.BlockAvgParam[index].AvgBinaryMethod == 0)
					{
						if (pfile.BlockAvgParam[index].ThdDownON && pfile.BlockAvgParam[index].ThdUpON)
						{
							Scalar scalar = ((WholeMask[i].Width <= 0 || WholeMask[i].Height <= 0) ? Binary[i].Mean() : Binary[i].Mean(WholeMask[i]));
							AvgGray[i] = (int)scalar.Val0;
							int num2 = (int)scalar.Val0 + thdUp;
							int num3 = (int)scalar.Val0 - thdDown;
							if (num2 > 255)
							{
								num2 = 255;
							}
							if (num3 < 0)
							{
								num3 = 0;
							}
							Cv2.InRange(Binary[i], num2, 255.0, BinaryTemp1);
							Cv2.InRange(Binary[i], 0.0, num3, BinaryTemp2);
							Cv2.BitwiseOr(BinaryTemp1, BinaryTemp2, Binary[i]);
						}
						else if (pfile.BlockAvgParam[index].ThdUpON)
						{
							Scalar scalar2 = ((WholeMask[i].Width <= 0 || WholeMask[i].Height <= 0) ? Binary[i].Mean() : Binary[i].Mean(WholeMask[i]));
							AvgGray[i] = (int)scalar2.Val0;
							int num4 = (int)scalar2.Val0 + thdUp;
							if (num4 > 255)
							{
								num4 = 255;
							}
							Cv2.InRange(Binary[i], num4, 255.0, Binary[i]);
						}
						else if (pfile.BlockAvgParam[index].ThdDownON)
						{
							Scalar scalar3 = ((WholeMask[i].Width <= 0 || WholeMask[i].Height <= 0) ? Binary[i].Mean() : Binary[i].Mean(WholeMask[i]));
							AvgGray[i] = (int)scalar3.Val0;
							int num5 = (int)scalar3.Val0 - thdDown;
							if (num5 < 0)
							{
								num5 = 0;
							}
							Cv2.InRange(Binary[i], 0.0, num5, Binary[i]);
						}
						if (AvgGray[i] < pfile.BlockAvgParam[index].AvgGrayMin || AvgGray[i] > pfile.BlockAvgParam[index].AvgGrayMax)
						{
							AvgGrayResult[i] = false;
						}
					}
					else if (pfile.BlockAvgParam[index].AvgBinaryMethod == 1)
					{
						Binary[i].SetTo(0.0);
						int num6 = pfile.BlockAvgParam[index].AvgShiftValue + 1;
						if (pfile.BlockAvgParam[index].ThdDownON && pfile.BlockAvgParam[index].ThdUpON)
						{
							for (int j = 0; j < Binary[i].Height - num6; j += num6)
							{
								RowMat = SelectMat[i].RowRange(j, j + num6);
								Scalar scalar4;
								if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
								{
									RowRangeMat = WholeMask[i].RowRange(j, j + num6);
									scalar4 = RowMat.Mean(RowRangeMat);
								}
								else
								{
									scalar4 = RowMat.Mean();
								}
								int num7 = (int)scalar4.Val0 + thdUp;
								num7 = ((num7 > 255) ? 255 : num7);
								int num8 = (int)scalar4.Val0 - thdDown;
								num8 = ((num8 >= 0) ? num8 : 0);
								Cv2.InRange(RowMat, num7, 255.0, AvgUpBinary);
								Cv2.InRange(RowMat, 0.0, num8, AvgDownBinary);
								Cv2.BitwiseOr(AvgUpBinary, AvgDownBinary, Binary[i].RowRange(j, j + num6));
								if (j + num6 < Binary[i].Height - num6 || num6 <= 1)
								{
									continue;
								}
								j += num6;
								num6 = Binary[i].Height - j;
								if (num6 >= 1)
								{
									RowMat = SelectMat[i].RowRange(j, j + num6);
									if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
									{
										RowRangeMat = WholeMask[i].RowRange(j, j + num6);
										scalar4 = RowMat.Mean(RowRangeMat);
									}
									else
									{
										scalar4 = RowMat.Mean();
									}
									num7 = (int)scalar4.Val0 + thdUp;
									num7 = ((num7 > 255) ? 255 : num7);
									num8 = (int)scalar4.Val0 - thdDown;
									num8 = ((num8 >= 0) ? num8 : 0);
									Cv2.InRange(RowMat, num7, 255.0, AvgUpBinary);
									Cv2.InRange(RowMat, 0.0, num8, AvgDownBinary);
									Cv2.BitwiseOr(AvgUpBinary, AvgDownBinary, Binary[i].RowRange(j, j + num6));
								}
								j = Binary[i].Height - num6;
							}
						}
						else if (pfile.BlockAvgParam[index].ThdUpON)
						{
							for (int k = 0; k < Binary[i].Height - num6; k += num6)
							{
								RowMat = SelectMat[i].RowRange(k, k + num6);
								Scalar scalar5;
								if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
								{
									RowRangeMat = WholeMask[i].RowRange(k, k + num6);
									scalar5 = RowMat.Mean(RowRangeMat);
								}
								else
								{
									scalar5 = RowMat.Mean();
								}
								int num9 = (int)scalar5.Val0 + thdUp;
								num9 = ((num9 > 255) ? 255 : num9);
								Cv2.InRange(RowMat, num9, 255.0, Binary[i].RowRange(k, k + num6));
								if (k + num6 < Binary[i].Height - num6 || num6 <= 1)
								{
									continue;
								}
								k += num6;
								num6 = Binary[i].Height - k;
								if (num6 >= 1)
								{
									RowMat = SelectMat[i].RowRange(k, k + num6);
									if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
									{
										RowRangeMat = WholeMask[i].RowRange(k, k + num6);
										scalar5 = RowMat.Mean(RowRangeMat);
									}
									else
									{
										scalar5 = RowMat.Mean();
									}
									num9 = (int)scalar5.Val0 + thdUp;
									num9 = ((num9 > 255) ? 255 : num9);
									Cv2.InRange(RowMat, num9, 255.0, Binary[i].RowRange(k, k + num6));
								}
								k = Binary[i].Height - num6;
							}
						}
						else if (pfile.BlockAvgParam[index].ThdDownON)
						{
							for (int l = 0; l < Binary[i].Height - num6; l += num6)
							{
								RowMat = SelectMat[i].RowRange(l, l + num6);
								Scalar scalar6;
								if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
								{
									RowRangeMat = WholeMask[i].RowRange(l, l + num6);
									scalar6 = RowMat.Mean(RowRangeMat);
								}
								else
								{
									scalar6 = RowMat.Mean();
								}
								int num10 = (int)scalar6.Val0 - thdDown;
								num10 = ((num10 >= 0) ? num10 : 0);
								Cv2.InRange(RowMat, 0.0, num10, Binary[i].RowRange(l, l + num6));
								if (l + num6 < Binary[i].Height - num6 || num6 <= 1)
								{
									continue;
								}
								l += num6;
								num6 = Binary[i].Height - l;
								if (num6 >= 1)
								{
									RowMat = SelectMat[i].RowRange(l, l + num6);
									if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
									{
										RowRangeMat = WholeMask[i].RowRange(l, l + num6);
										scalar6 = RowMat.Mean(RowRangeMat);
									}
									else
									{
										scalar6 = RowMat.Mean();
									}
									num10 = (int)scalar6.Val0 - thdDown;
									num10 = ((num10 >= 0) ? num10 : 0);
									Cv2.InRange(RowMat, 0.0, num10, Binary[i].RowRange(l, l + num6));
								}
								l = Binary[i].Height - num6;
							}
						}
					}
					else if (pfile.BlockAvgParam[index].AvgBinaryMethod == 2)
					{
						Cv2.Transpose(Binary[i], Binary[i]);
						Cv2.Flip(Binary[i], Binary[i], FlipMode.Y);
						if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
						{
							Cv2.Transpose(WholeMask[i], WholeMask[i]);
							Cv2.Flip(WholeMask[i], WholeMask[i], FlipMode.Y);
						}
						Cv2.Transpose(SelectMat[i], SelectMat[i]);
						Cv2.Flip(SelectMat[i], SelectMat[i], FlipMode.Y);
						int num11 = pfile.BlockAvgParam[index].AvgShiftValue + 1;
						Binary[i].SetTo(0.0);
						if (pfile.BlockAvgParam[index].ThdDownON && pfile.BlockAvgParam[index].ThdUpON)
						{
							for (int m = 0; m < Binary[i].Height - num11; m += num11)
							{
								ColMat = SelectMat[i].RowRange(m, m + num11);
								Scalar scalar7;
								if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
								{
									ColRangeMat = WholeMask[i].RowRange(m, m + num11);
									scalar7 = ColMat.Mean(ColRangeMat);
								}
								else
								{
									scalar7 = ColMat.Mean();
								}
								int num12 = (int)scalar7.Val0 + thdUp;
								num12 = ((num12 > 255) ? 255 : num12);
								int num13 = (int)scalar7.Val0 - thdDown;
								num13 = ((num13 >= 0) ? num13 : 0);
								Cv2.InRange(ColMat, num12, 255.0, AvgUpBinary);
								Cv2.InRange(ColMat, 0.0, num13, AvgDownBinary);
								Cv2.BitwiseOr(AvgUpBinary, AvgDownBinary, Binary[i].RowRange(m, m + num11));
								if (m + num11 < Binary[i].Height - num11 || num11 <= 1)
								{
									continue;
								}
								m += num11;
								num11 = Binary[i].Height - m;
								if (num11 >= 1)
								{
									ColMat = SelectMat[i].RowRange(m, m + num11);
									if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
									{
										ColRangeMat = WholeMask[i].RowRange(m, m + num11);
										scalar7 = ColMat.Mean(ColRangeMat);
									}
									else
									{
										scalar7 = ColMat.Mean();
									}
									num12 = (int)scalar7.Val0 + thdUp;
									num12 = ((num12 > 255) ? 255 : num12);
									num13 = (int)scalar7.Val0 - thdDown;
									num13 = ((num13 >= 0) ? num13 : 0);
									Cv2.InRange(ColMat, num12, 255.0, AvgUpBinary);
									Cv2.InRange(ColMat, 0.0, num13, AvgDownBinary);
									Cv2.BitwiseOr(AvgUpBinary, AvgDownBinary, Binary[i].RowRange(m, m + num11));
								}
								m = Binary[i].Height - num11;
							}
						}
						else if (pfile.BlockAvgParam[index].ThdUpON)
						{
							for (int n = 0; n < Binary[i].Height - num11; n += num11)
							{
								ColMat = SelectMat[i].RowRange(n, n + num11);
								Scalar scalar8;
								if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
								{
									ColRangeMat = WholeMask[i].RowRange(n, n + num11);
									scalar8 = ColMat.Mean(ColRangeMat);
								}
								else
								{
									scalar8 = ColMat.Mean();
								}
								int num14 = (int)scalar8.Val0 + thdUp;
								num14 = ((num14 > 255) ? 255 : num14);
								Cv2.InRange(ColMat, num14, 255.0, Binary[i].RowRange(n, n + num11));
								if (n + num11 < Binary[i].Height - num11 || num11 <= 1)
								{
									continue;
								}
								n += num11;
								num11 = Binary[i].Height - n;
								if (num11 >= 1)
								{
									ColMat = SelectMat[i].RowRange(n, n + num11);
									if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
									{
										ColRangeMat = WholeMask[i].RowRange(n, n + num11);
										scalar8 = ColMat.Mean(ColRangeMat);
									}
									else
									{
										scalar8 = ColMat.Mean();
									}
									num14 = (int)scalar8.Val0 + thdUp;
									num14 = ((num14 > 255) ? 255 : num14);
									Cv2.InRange(ColMat, num14, 255.0, Binary[i].RowRange(n, n + num11));
								}
								n = Binary[i].Height - num11;
							}
						}
						else if (pfile.BlockAvgParam[index].ThdDownON)
						{
							for (int num15 = 0; num15 < Binary[i].Height - num11; num15 += num11)
							{
								ColMat = SelectMat[i].RowRange(num15, num15 + num11);
								Scalar scalar9;
								if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
								{
									ColRangeMat = WholeMask[i].RowRange(num15, num15 + num11);
									scalar9 = ColMat.Mean(ColRangeMat);
								}
								else
								{
									scalar9 = ColMat.Mean();
								}
								int num16 = (int)scalar9.Val0 - thdDown;
								num16 = ((num16 >= 0) ? num16 : 0);
								Cv2.InRange(ColMat, 0.0, num16, Binary[i].RowRange(num15, num15 + num11));
								if (num15 + num11 < Binary[i].Height - num11 || num11 <= 1)
								{
									continue;
								}
								num15 += num11;
								num11 = Binary[i].Height - num15;
								if (num11 >= 1)
								{
									ColMat = SelectMat[i].RowRange(num15, num15 + num11);
									if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
									{
										ColRangeMat = WholeMask[i].RowRange(num15, num15 + num11);
										scalar9 = ColMat.Mean(ColRangeMat);
									}
									else
									{
										scalar9 = ColMat.Mean();
									}
									num16 = (int)scalar9.Val0 - thdDown;
									num16 = ((num16 >= 0) ? num16 : 0);
									Cv2.InRange(ColMat, 0.0, num16, Binary[i].RowRange(num15, num15 + num11));
								}
								num15 = Binary[i].Height - num11;
							}
						}
						Cv2.Transpose(Binary[i], Binary[i]);
						Cv2.Flip(Binary[i], Binary[i], FlipMode.X);
						if (WholeMask[i].Width > 0 && WholeMask[i].Height > 0)
						{
							Cv2.Transpose(WholeMask[i], WholeMask[i]);
							Cv2.Flip(WholeMask[i], WholeMask[i], FlipMode.X);
						}
						Cv2.Transpose(SelectMat[i], SelectMat[i]);
						Cv2.Flip(SelectMat[i], SelectMat[i], FlipMode.X);
					}
					TimeStop();
				}
				num = 5;
				num = 6;
				num = 7;
				if (tHDLimit_Min != 0 || tHDLimit_Max != 255)
				{
					Cv2.InRange(SelectMat[i], tHDLimit_Min, tHDLimit_Max, doThdBinary[i]);
					Cv2.BitwiseAnd(doThdBinary[i], Binary[i], Binary[i]);
				}
				DoMorph(ref Binary[i], (int)pfile.BlockAvgParam[index].DE_Start_Index, pfile.BlockAvgParam[index].DE_Start_Count);
				DoMorph(ref Binary[i], (int)pfile.BlockAvgParam[index].DE_End_Index, pfile.BlockAvgParam[index].DE_End_Count);
				num = 8;
				if (pfile.BlockAvgParam[index].MaskSelectIndex <= 0 && pfile.BlockAvgParam[index].UseMatchTextMask1 <= 0 && pfile.BlockAvgParam[index].UseMatchTextMask2 <= 0)
				{
					_ = pfile.BlockAvgParam[index].UseMatchTextMask3;
					_ = 0;
				}
				if (WholeMask[i].Width == Binary[i].Width && WholeMask[i].Height == Binary[i].Height)
				{
					Cv2.BitwiseAnd(Binary[i], WholeMask[i], Binary[i]);
				}
				num = 13;
				if (pfile.BlockAvgParam[index].BlobAnalysisSelect == 0)
				{
					array[i] = findBlob(Binary[i], i);
				}
				else if (pfile.BlockAvgParam[index].BlobAnalysisSelect == 1)
				{
					if (i == 0)
					{
						array[i] = findBlobLine(Binary[i], index, i, ref BlobLine1);
					}
					else
					{
						array[i] = findBlobLine(Binary[i], index, i, ref BlobLine2);
					}
				}
				else if (pfile.BlockAvgParam[index].BlobAnalysisSelect == 2)
				{
					if (i == 0)
					{
						array[i] = findMultiBlob(Binary[i], index, i, ref MultiBlob1, pfile.BlockAvgParam[index].MB_AllowNGCount, i);
					}
					else
					{
						array[i] = findMultiBlob(Binary[i], index, i, ref MultiBlob2, pfile.BlockAvgParam[index].MB_AllowNGCount, i);
					}
				}
				num = 14;
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("{0}:{1}-DebugIndex={2}, ", index + 1, pfile.BlockAvgParam[index].TrackerName, num) + "DT06-BlockAvgTool-Inspect()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-007");
			}
			return false;
		}
		if (array[0] && array[1])
		{
			if (AvgGrayResult[0] && AvgGrayResult[1])
			{
				return true;
			}
			if (!AvgGrayResult[0])
			{
				Tracker1ResultText += string.Format("[{0}:{1}:A] 平均二值化平均門檻值({2})超過設定值!({3}~{4}) \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, AvgGray[0], pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMin, pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMax);
				Blobs[0].result = false;
			}
			if (!AvgGrayResult[1])
			{
				Tracker1ResultText += string.Format("[{0}:{1}:B] 平均二值化平均門檻值({2})超過設定值!({3}~{4}) \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, AvgGray[1], pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMin, pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMax);
				Blobs[1].result = false;
			}
			iresult = TOOL_RESULT.EM_NG;
			return false;
		}
		if (!AvgGrayResult[0])
		{
			Tracker1ResultText += string.Format("[{0}:{1}:A] 平均二值化平均門檻值({2})超過設定值!({3}~{4}) \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, AvgGray[0], pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMin, pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMax);
			Blobs[0].result = false;
		}
		if (!AvgGrayResult[1])
		{
			Tracker1ResultText += string.Format("[{0}:{1}:B] 平均二值化平均門檻值({2})超過設定值!({3}~{4}) \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, AvgGray[1], pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMin, pfile.BlockAvgParam[BlockAvgIndex].AvgGrayMax);
			Blobs[1].result = false;
		}
		iresult = TOOL_RESULT.EM_NG;
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetMask(ref Mat BinaryMask, int index, int trackAB)
	{
		Rect[] array = new Rect[2];
		Mat channelMat = buffers.GetChannelMat(0);
		channelMat.CopyTo(MaskTemp);
		MaskTemp.SetTo(255.0);
		if (pfile.BlockAvgParam[index].MaskSelectIndex > 0)
		{
			if (!pfile.MaskToolON)
			{
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool has been Disable! \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具未開啟! \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具未开启! \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 検出領域はマスクツールを使用していますが、マスクツールは開かれていません。 \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName);
				}
				return false;
			}
			int num = pfile.BlockAvgParam[index].MaskSelectIndex - 1;
			if (!pfile.MaskParam[num].TrackerON)
			{
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool [{2}] has been Disable! \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, num + 1);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具[{2}]未開啟! \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, num + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具[{2}]未开启! \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, num + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}:{1}] 検出領域はマスクツールを使用していますが、マスクツール[2]は開かれていません。 \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, num + 1);
				}
				return false;
			}
			if (pfile.MaskParam[num].Mask1 != null)
			{
				array[0] = new Rect(pfile.MaskParam[num].Mask1Pt.X, pfile.MaskParam[num].Mask1Pt.Y, pfile.MaskParam[num].Mask1.Width, pfile.MaskParam[num].Mask1.Height);
				if (pfile.MaskParam[num].SearchRect2ON && num < 20 && pfile.MaskParam[num].Mask2 != null)
				{
					array[1] = new Rect(pfile.MaskParam[num].Mask2Pt.X, pfile.MaskParam[num].Mask2Pt.Y, pfile.MaskParam[num].Mask2.Width, pfile.MaskParam[num].Mask2.Height);
				}
				else
				{
					array[1] = new Rect(0, 0, 0, 0);
				}
				if (array[0].Width > 0 && array[0].Height > 0)
				{
					using Mat m = new Mat(MaskTemp, array[0]);
					using Mat mat = MM.CloneFrom(pfile.MaskParam[num].Mask1);
					Cv2.BitwiseNot(mat, mat);
					mat.CopyTo(m);
				}
				if (pfile.MaskParam[num].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num < 20)
				{
					using Mat m2 = new Mat(MaskTemp, array[1]);
					using Mat mat2 = MM.CloneFrom(pfile.MaskParam[num].Mask2);
					Cv2.BitwiseNot(mat2, mat2);
					mat2.CopyTo(m2);
				}
			}
		}
		new Mat(MaskTemp, InspeRect[trackAB]).CopyTo(BinaryMask);
		if (pfile.BlockAvgParam[index].UseMaskAsInspArea)
		{
			Cv2.BitwiseNot(BinaryMask, BinaryMask);
		}
		if (trackAB == 0)
		{
			buffers.DoFilllet(ref BinaryMask, new Rect(0, 0, BinaryMask.Width, BinaryMask.Height), pfile.BlockAvgParam[index].T1FillletCount0, pfile.BlockAvgParam[index].T1FillletCount1, pfile.BlockAvgParam[index].T1FillletCount2, pfile.BlockAvgParam[index].T1FillletCount3);
		}
		else
		{
			buffers.DoFilllet(ref BinaryMask, new Rect(0, 0, BinaryMask.Width, BinaryMask.Height), pfile.BlockAvgParam[index].T2FillletCount0, pfile.BlockAvgParam[index].T2FillletCount1, pfile.BlockAvgParam[index].T2FillletCount2, pfile.BlockAvgParam[index].T2FillletCount3);
		}
		if (pfile.MatchToolON)
		{
			MaskTemp.SetTo(255.0);
			if (pfile.BlockAvgParam[index].UseMatchTextMask1 > 0)
			{
				int num = pfile.BlockAvgParam[index].UseMatchTextMask1 - 1;
				if (pfile.MatchParam[num].TextMatResult)
				{
					Rect textRect = pfile.MatchParam[num].TextRect;
					textRect &= new Rect(0, 0, channelMat.Cols, channelMat.Rows);
					if (textRect.Width > 0 && textRect.Height > 0 && BinaryMask.Width == InspeRect[trackAB].Width && BinaryMask.Height == InspeRect[trackAB].Height)
					{
						using Mat m3 = new Mat(MaskTemp, textRect);
						Mat mat3 = MM.CloneFrom(pfile.MatchParam[num].TextMat);
						DoMorph(ref mat3, (int)pfile.BlockAvgParam[index].MatchTextDE_Start1_Index, pfile.BlockAvgParam[index].MatchTextDE_Start1_Count);
						Cv2.BitwiseNot(mat3, mat3);
						mat3.CopyTo(m3);
						Mat mat4 = MM.newMat(MaskTemp, InspeRect[trackAB]).Clone();
						Cv2.BitwiseAnd(BinaryMask, mat4, BinaryMask);
						MM.GiveBack(mat3);
						MM.GiveBack(mat4);
					}
				}
			}
			if (pfile.BlockAvgParam[index].UseMatchTextMask2 > 0)
			{
				MaskTemp.SetTo(255.0);
				int num = pfile.BlockAvgParam[index].UseMatchTextMask2 - 1;
				if (pfile.MatchParam[num].TextMatResult)
				{
					Rect textRect2 = pfile.MatchParam[num].TextRect;
					textRect2 &= new Rect(0, 0, channelMat.Cols, channelMat.Rows);
					if (textRect2.Width > 0 && textRect2.Height > 0 && BinaryMask.Width == InspeRect[trackAB].Width && BinaryMask.Height == InspeRect[trackAB].Height)
					{
						using Mat m4 = new Mat(MaskTemp, textRect2);
						Mat mat5 = MM.CloneFrom(pfile.MatchParam[num].TextMat);
						DoMorph(ref mat5, (int)pfile.BlockAvgParam[index].MatchTextDE_Start2_Index, pfile.BlockAvgParam[index].MatchTextDE_Start2_Count);
						Cv2.BitwiseNot(mat5, mat5);
						mat5.CopyTo(m4);
						Mat mat6 = MM.newMat(MaskTemp, InspeRect[trackAB]).Clone();
						Cv2.BitwiseAnd(BinaryMask, mat6, BinaryMask);
						MM.GiveBack(mat5);
						MM.GiveBack(mat6);
					}
				}
			}
			if (pfile.BlockAvgParam[index].UseMatchTextMask3 > 0)
			{
				MaskTemp.SetTo(255.0);
				int num = pfile.BlockAvgParam[index].UseMatchTextMask3 - 1;
				if (pfile.MatchParam[num].TextMatResult)
				{
					Rect textRect3 = pfile.MatchParam[num].TextRect;
					textRect3 &= new Rect(0, 0, channelMat.Cols, channelMat.Rows);
					if (textRect3.Width > 0 && textRect3.Height > 0 && BinaryMask.Width == InspeRect[trackAB].Width && BinaryMask.Height == InspeRect[trackAB].Height)
					{
						using Mat m5 = new Mat(MaskTemp, textRect3);
						Mat mat7 = MM.CloneFrom(pfile.MatchParam[num].TextMat);
						DoMorph(ref mat7, (int)pfile.BlockAvgParam[index].MatchTextDE_Start3_Index, pfile.BlockAvgParam[index].MatchTextDE_Start3_Count);
						Cv2.BitwiseNot(mat7, mat7);
						mat7.CopyTo(m5);
						Mat mat8 = MM.newMat(MaskTemp, InspeRect[trackAB]).Clone();
						Cv2.BitwiseAnd(BinaryMask, mat8, BinaryMask);
						MM.GiveBack(mat7);
						MM.GiveBack(mat8);
					}
				}
			}
		}
		return true;
	}

	public void FillMatchMat(ref Mat mat, int index, int i)
	{
		if (!pfile.MatchToolON)
		{
			return;
		}
		Mat channelMat = buffers.GetChannelMat(0);
		if (pfile.BlockAvgParam[index].UseMatchTextMask1 > 0)
		{
			int num = pfile.BlockAvgParam[index].UseMatchTextMask1 - 1;
			if (pfile.MatchParam[num].TextMatResult)
			{
				Rect rect = new Rect(pfile.MatchParam[num].TextRect.Left - InspeRect[i].Left, pfile.MatchParam[num].TextRect.Top - InspeRect[i].Top, pfile.MatchParam[num].TextRect.Width, pfile.MatchParam[num].TextRect.Height);
				rect &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
				rect &= new Rect(0, 0, mat.Cols, mat.Rows);
				if (rect.Width > 2 && rect.Height > 2)
				{
					Mat mat2 = MM.new_Mat(channelMat.Height, channelMat.Width, MatType.CV_8UC1);
					using (Mat mat3 = new Mat(mat, new Rect(rect.Left, rect.Top, rect.Width, 2)))
					{
						using Mat m = new Mat(mat2, new Rect(rect.Left + InspeRect[i].Left, rect.Top + InspeRect[i].Top, rect.Width, rect.Height));
						mat2.SetTo(0.0);
						Mat mat4 = MM.CloneFrom(pfile.MatchParam[num].TextMat.Clone());
						DoMorph(ref mat4, (int)pfile.BlockAvgParam[index].MatchTextDE_Start1_Index, pfile.BlockAvgParam[index].MatchTextDE_Start1_Count);
						mat4.CopyTo(m);
						Mat mat5 = MM.newMat(mat2, InspeRect[i]).Clone();
						Scalar value = mat3.Mean();
						mat.SetTo(value, mat5);
						MM.GiveBack(mat5);
						MM.GiveBack(mat4);
					}
					MM.GiveBack(mat2);
				}
			}
		}
		if (pfile.BlockAvgParam[index].UseMatchTextMask2 > 0)
		{
			int num2 = pfile.BlockAvgParam[index].UseMatchTextMask2 - 1;
			if (pfile.MatchParam[num2].TextMatResult)
			{
				Rect rect2 = new Rect(pfile.MatchParam[num2].TextRect.Left - InspeRect[i].Left, pfile.MatchParam[num2].TextRect.Top - InspeRect[i].Top, pfile.MatchParam[num2].TextRect.Width, pfile.MatchParam[num2].TextRect.Height);
				rect2 &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
				rect2 &= new Rect(0, 0, mat.Cols, mat.Rows);
				if (rect2.Width > 2 && rect2.Height > 2)
				{
					Mat mat6 = MM.new_Mat(channelMat.Height, channelMat.Width, MatType.CV_8UC1);
					using (Mat mat7 = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, 2)))
					{
						using Mat m2 = new Mat(mat6, new Rect(rect2.Left + InspeRect[i].Left, rect2.Top + InspeRect[i].Top, rect2.Width, rect2.Height));
						mat6.SetTo(0.0);
						Mat mat8 = MM.CloneFrom(pfile.MatchParam[num2].TextMat.Clone());
						DoMorph(ref mat8, (int)pfile.BlockAvgParam[index].MatchTextDE_Start2_Index, pfile.BlockAvgParam[index].MatchTextDE_Start2_Count);
						mat8.CopyTo(m2);
						Mat mat9 = MM.newMat(mat6, InspeRect[i]).Clone();
						Scalar value2 = mat7.Mean();
						mat.SetTo(value2, mat9);
						MM.GiveBack(mat9);
						MM.GiveBack(mat8);
					}
					MM.GiveBack(mat6);
				}
			}
		}
		if (pfile.BlockAvgParam[index].UseMatchTextMask3 <= 0)
		{
			return;
		}
		int num3 = pfile.BlockAvgParam[index].UseMatchTextMask3 - 1;
		if (!pfile.MatchParam[num3].TextMatResult)
		{
			return;
		}
		Rect rect3 = new Rect(pfile.MatchParam[num3].TextRect.Left - InspeRect[i].Left, pfile.MatchParam[num3].TextRect.Top - InspeRect[i].Top, pfile.MatchParam[num3].TextRect.Width, pfile.MatchParam[num3].TextRect.Height);
		rect3 &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
		rect3 &= new Rect(0, 0, mat.Cols, mat.Rows);
		if (rect3.Width <= 2 || rect3.Height <= 2)
		{
			return;
		}
		Mat mat10 = MM.new_Mat(channelMat.Height, channelMat.Width, MatType.CV_8UC1);
		using (Mat mat11 = new Mat(mat, new Rect(rect3.Left, rect3.Top, rect3.Width, 2)))
		{
			using Mat m3 = new Mat(mat10, new Rect(rect3.Left + InspeRect[i].Left, rect3.Top + InspeRect[i].Top, rect3.Width, rect3.Height));
			mat10.SetTo(0.0);
			Mat mat12 = MM.CloneFrom(pfile.MatchParam[num3].TextMat.Clone());
			DoMorph(ref mat12, (int)pfile.BlockAvgParam[index].MatchTextDE_Start3_Index, pfile.BlockAvgParam[index].MatchTextDE_Start3_Count);
			mat12.CopyTo(m3);
			Mat mat13 = MM.newMat(mat10, InspeRect[i]).Clone();
			Scalar value3 = mat11.Mean();
			mat.SetTo(value3, mat13);
			MM.GiveBack(mat13);
			MM.GiveBack(mat12);
		}
		MM.GiveBack(mat10);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findMultiBlob(Mat binary, int index, int TrackerIndex, ref List<SimpleBlobStruct> MultiBlob, int AllowNGCount, int TrackIndex)
	{
		bool result = true;
		Mat mat = new Mat();
		Mat mat2 = new Mat();
		Mat mat3 = new Mat();
		int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
		Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
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
			switch (pfile.BlockAvgParam[index].MB_BlobSort)
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
			for (int num3 = 0; num3 < MultiBlob.Count && num3 < pfile.BlockAvgParam[index].MB_BlobNum; num3++)
			{
				if (!checkMultiResult(MultiBlob[num3].NGRect, (int)MultiBlob[num3].Area, num3, TrackIndex))
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
				if (TrackerIndex == 0)
				{
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG Count={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", num2);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG個數={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", num2);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", num2);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] 不良品数={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "A", num2);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}] NG Count={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", num2);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}] NG個數={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", num2);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", num2);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}] 不良品数={3}\n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, "B", num2);
				}
				result = false;
			}
			Blobs[TrackerIndex].result = result;
			return result;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool checkMultiResult(Rect rect, int area, int BlobIndex, int TrackIndex)
	{
		try
		{
			bool[] array = new bool[4];
			bool flag = true;
			array[0] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule1_Index, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule1_Min, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule1_Max, rect, area, TrackIndex);
			array[1] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule2_Index, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule2_Min, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule2_Max, rect, area, TrackIndex);
			array[2] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule3_Index, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule3_Min, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule3_Max, rect, area, TrackIndex);
			array[3] = GetRule((int)pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule4_Index, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule4_Min, pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule4_Max, rect, area, TrackIndex);
			if (pfile.BlockAvgParam[BlockAvgIndex].MB_InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.BlockAvgParam[BlockAvgIndex].MB_InspRule4_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
			}
			double num = 0.0;
			num = ((area != 0 && rect.Width > 0 && rect.Height > 0) ? ((double)area / (double)(rect.Width * rect.Height)) : 0.0);
			double num2 = 0.0;
			num2 = ((rect.Width <= 0 || rect.Height <= 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			string text = "";
			if (!flag)
			{
				text = "[NG]";
			}
			string text2 = "A";
			if (TrackIndex == 1)
			{
				text2 = "B";
			}
			if (pfile.UseFloat)
			{
				if (pfile.MeasureUnit == 0)
				{
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}um, Height:{5:N3}um, Area:{6:N3}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}um, 高:{5:N3}um, 面积:{6:N3}um, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}µm, 高:{5:N3}µm, 面積:{6:N3}µm², 縦横比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
				}
				else if (buffers.LanguageType == 0)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}mm, Height:{5:N3}mm, Area:{6:N3}mm, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}mm, 高:{5:N3}mm, 面积:{6:N3}mm, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm², 縦横比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
			}
			else if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 0)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4}um, Height:{5}um, Area:{6}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4}um, 高:{5}um, 面積:{6}um, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4}um, 高:{5}um, 面积:{6}um, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4}μm, 高:{5}μm, 面積:{6}μm, 縦横比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
			}
			else if (buffers.LanguageType == 0)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4}mm, Height:{5}mm, Area:{6}mm, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4}mm, 高:{5}mm, 面積:{6}mm, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4}mm, 高:{5}mm, 面积:{6}mm, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4}mm, 高:{5}mm, 面積:{6}mm, 縦横比:{7:N3}, 面積比:{8:N3} {9} \n", BlockAvgIndex + 1, pfile.BlockAvgParam[BlockAvgIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text3 = "BT03-BlockAvg-checkMultiResult()-";
			text3 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text3);
			}
			else
			{
				CVBuffer.SaveLog(text3, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "BLOAVG-008");
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
}
