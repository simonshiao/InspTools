using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class SmoothTool
{
	private const int MaxTracker = 2;

	private CVBuffer buffers;

	public Mat[] Binary = new Mat[2];

	private Mat doThdBinary = new Mat();

	private Mat SelectMat = new Mat();

	private Mat SelectMatBinary = new Mat();

	private Mat result1 = new Mat();

	private Mat result2 = new Mat();

	private Mat WholeMask = new Mat();

	private Mat MaskTemp = new Mat();

	private Mat FilterMat1 = new Mat();

	private Mat FilterMat2 = new Mat();

	private Mat MatchMatBuf1 = new Mat();

	private Mat MatchMatBuf2 = new Mat();

	private Mat MatchMatBuf3 = new Mat();

	private ParamFile pfile;

	private Rect[] InspeRect = new Rect[2];

	public BlobStruct[] Blobs = new BlobStruct[2];

	public List<SimpleBlobStruct> BlobLine1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> BlobLine2 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob2 = new List<SimpleBlobStruct>();

	private int SmoothIndex;

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool SmoothSuccess { get; set; }

	public bool TrackerError { get; set; }

	public string Tracker1ResultText { get; set; }

	public string Tracker2ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public SmoothTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect[0] = new Rect(0, 0, 0, 0);
		InspeRect[1] = new Rect(0, 0, 0, 0);
		Binary[0] = new Mat();
		Binary[1] = new Mat();
	}

	public void SetBuffers(CVBuffer bufs)
	{
		buffers = bufs;
	}

	public void SetNumThreads(int num)
	{
		Cv2.SetNumThreads(num);
	}

	public void SetUseOptimized(bool enable)
	{
		Cv2.SetUseOptimized(enable);
	}

	public void GetInspeRect(int index)
	{
		LocateRect = buffers.LocatedRect;
		if (pfile.SmoothParam[index].Tracker1UseAutoSize)
		{
			InspeRect[0].X = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize1_SelectIndex].L + pfile.SmoothParam[index].Track1_L;
			InspeRect[0].Y = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize1_SelectIndex].T + pfile.SmoothParam[index].Track1_T;
			InspeRect[0].Width = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize1_SelectIndex].Width - (pfile.SmoothParam[index].Track1_L + pfile.SmoothParam[index].Track1_R);
			InspeRect[0].Height = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize1_SelectIndex].Height - (pfile.SmoothParam[index].Track1_T + pfile.SmoothParam[index].Track1_B);
		}
		else
		{
			int width = pfile.SmoothParam[index].Track1_R - pfile.SmoothParam[index].Track1_L;
			int height = pfile.SmoothParam[index].Track1_B - pfile.SmoothParam[index].Track1_T;
			InspeRect[0].X = pfile.VSLocateParam[pfile.SmoothParam[index].Tracker1AligmentXIndex].Value + pfile.SmoothParam[index].Track1_L;
			InspeRect[0].Y = pfile.HSLocateParam[pfile.SmoothParam[index].Tracker1AligmentYIndex].Value + pfile.SmoothParam[index].Track1_T;
			InspeRect[0].Width = width;
			InspeRect[0].Height = height;
		}
		if (pfile.SmoothParam[index].Tracker2UseAutoSize)
		{
			InspeRect[1].X = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize2_SelectIndex].L + pfile.SmoothParam[index].Track2_L;
			InspeRect[1].Y = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize2_SelectIndex].T + pfile.SmoothParam[index].Track2_T;
			InspeRect[1].Width = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize2_SelectIndex].Width - (pfile.SmoothParam[index].Track2_L + pfile.SmoothParam[index].Track2_R);
			InspeRect[1].Height = pfile.AdvLocateParam[pfile.SmoothParam[index].AutoSize2_SelectIndex].Height - (pfile.SmoothParam[index].Track2_T + pfile.SmoothParam[index].Track2_B);
		}
		else
		{
			int width2 = pfile.SmoothParam[index].Track2_R - pfile.SmoothParam[index].Track2_L;
			int height2 = pfile.SmoothParam[index].Track2_B - pfile.SmoothParam[index].Track2_T;
			InspeRect[1].X = pfile.VSLocateParam[pfile.SmoothParam[index].Tracker2AligmentXIndex].Value + pfile.SmoothParam[index].Track2_L;
			InspeRect[1].Y = pfile.HSLocateParam[pfile.SmoothParam[index].Tracker2AligmentYIndex].Value + pfile.SmoothParam[index].Track2_T;
			InspeRect[1].Width = width2;
			InspeRect[1].Height = height2;
		}
	}

	private Mat FilterNoise2(Mat mat, int w, int h)
	{
		Cv2.FindContours(mat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
		int num = w * h;
		for (int i = 0; i < contours.Length; i++)
		{
			if (Cv2.ContourArea(contours[i]) <= (double)num)
			{
				Cv2.DrawContours(mat, contours, i, Scalar.Black);
			}
		}
		return mat;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe Mat FilterNoise(Mat mat, int w, int h)
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
			string text = "ST01-SmoothTool-FilterNoise()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-001");
			}
			return mat;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool FindBlobLine(Mat binary, int index, int TrackerIndex, ref List<SimpleBlobStruct> BlobLine)
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
			switch (pfile.SmoothParam[index].BlobLineSort)
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
			for (int num3 = 0; num3 < BlobLine.Count && num3 < pfile.SmoothParam[index].BlobLineNum; num3++)
			{
				bool flag = true;
				if (BlobLine[num3].Width < pfile.SmoothParam[index].BlobLineWidthMin || BlobLine[num3].Width > pfile.SmoothParam[index].BlobLineWidthMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Height < pfile.SmoothParam[index].BlobLineHeightMin || BlobLine[num3].Height > pfile.SmoothParam[index].BlobLineHeightMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Area < pfile.SmoothParam[index].BlobLineAreaMin || BlobLine[num3].Area > pfile.SmoothParam[index].BlobLineAreaMax)
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
					text2 = ((buffers.LanguageType == 1) ? (text2 + string.Format("[{0}:{1}][{2}] 線條寬度{3}, 線條高度{4}, 線條面積{5} [NG] \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)) : ((buffers.LanguageType == 2) ? (text2 + string.Format("[{0}:{1}][{2}] 线条宽度{3}, 线条高度{4}, 线条面积{5} [NG] \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)) : ((buffers.LanguageType != 3) ? (text2 + string.Format("[{0}:{1}][{2}] Line Width={3}, Line Height={4}, Line Area={5}  [NG] \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)) : (text2 + string.Format("[{0}:{1}][{2}] 線の幅{3}, 線の高さ{4}, 線の面積{5} [NG] \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area)))));
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
			if (num2 < pfile.SmoothParam[index].BlobLineMin || num2 > pfile.SmoothParam[index].BlobLineMax)
			{
				result = false;
			}
			string text3 = "";
			text3 = ((buffers.LanguageType == 1) ? (text3 + string.Format("[{0}:{1}][{2}] 線條合格數量為{3}，允許最小合格數為{4}，允許最大合格數為{5} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, num2, pfile.SmoothParam[index].BlobLineMin, pfile.SmoothParam[index].BlobLineMax)) : ((buffers.LanguageType == 2) ? (text3 + string.Format("[{0}:{1}][{2}] 线条合格数量为{3}，允许最小合格数为{4}，允许最大合格数为{5} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, num2, pfile.SmoothParam[index].BlobLineMin, pfile.SmoothParam[index].BlobLineMax)) : ((buffers.LanguageType != 3) ? (text3 + string.Format("[{0}:{1}][{2}] Passed Line={3}，Min Passs Line={4}，Max Passs Line={5} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, num2, pfile.SmoothParam[index].BlobLineMin, pfile.SmoothParam[index].BlobLineMax)) : (text3 + string.Format("[{0}:{1}][{2}] 線の合格数が{3}個、許容最小合格数は{4}個、許容最大合格数は{5}個です。\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text, num2, pfile.SmoothParam[index].BlobLineMin, pfile.SmoothParam[index].BlobLineMax)))));
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
		else if (BlobLine.Count < pfile.SmoothParam[index].BlobLineNum)
		{
			result = false;
			Blobs[TrackerIndex].result = false;
			string text4 = "";
			text4 = ((buffers.LanguageType == 1) ? (text4 + string.Format("[{0}:{1}][{2}] 線條數量為0 \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text)) : ((buffers.LanguageType == 2) ? (text4 + string.Format("[{0}:{1}][{2}] 线条数量为0 \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text)) : ((buffers.LanguageType != 3) ? (text4 + string.Format("[{0}:{1}][{2}] Passed Line = 0  \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text)) : (text4 + string.Format("[{0}:{1}][{2}] 線の数: 0 \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, text)))));
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool FindBlob(Mat binary, int TrackerIndex)
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
				Blobs[TrackerIndex].Area = 0.0;
				Blobs[TrackerIndex].NGRect = rect;
				Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
				Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
				return CheckResult(rect, num, 0.0, TrackerIndex);
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
					if (genericIndexer[j, 2] + genericIndexer[j, 3] > num3)
					{
						i = j;
						num3 = genericIndexer[j, 2] + genericIndexer[j, 3];
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
			return CheckResult(rect, num, num4, TrackerIndex);
		}
		catch (Exception ex)
		{
			string text = "ST02-SmoothTool-FindBlob()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-002");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool FindBlob3(Mat binary, int TrackerIndex)
	{
		try
		{
			buffers.TimeStart();
			int area = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			double whratio = 0.0;
			for (int i = 0; i < 100; i++)
			{
				ConnectedComponents connectedComponents = Cv2.ConnectedComponentsEx(binary);
				if (connectedComponents.LabelCount >= 1)
				{
					ConnectedComponents.Blob largestBlob = connectedComponents.GetLargestBlob();
					whratio = ((largestBlob.Width == 0 || largestBlob.Height == 0) ? 0.0 : ((largestBlob.Width <= largestBlob.Height) ? ((double)(largestBlob.Height / largestBlob.Width)) : ((double)(largestBlob.Width / largestBlob.Height))));
					Blobs[TrackerIndex].Width = largestBlob.Width;
					Blobs[TrackerIndex].Height = largestBlob.Height;
					Blobs[TrackerIndex].WHRatio = 0.0;
					Blobs[TrackerIndex].Area = largestBlob.Area;
					Blobs[TrackerIndex].NGRect = largestBlob.Rect;
					Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
					Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
				}
			}
			buffers.TimeStop();
			return CheckResult(rect, area, whratio, TrackerIndex);
		}
		catch (Exception ex)
		{
			string text = "ST02-SmoothTool-FindBlob()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckResult(Rect rect, int area, double whratio, int TrackerIndex)
	{
		try
		{
			bool[] array = new bool[4];
			bool flag = true;
			array[0] = GetRule((int)pfile.SmoothParam[SmoothIndex].InspRule1_Index, pfile.SmoothParam[SmoothIndex].InspRule1_Min, pfile.SmoothParam[SmoothIndex].InspRule1_Max, rect, area, TrackerIndex);
			array[1] = GetRule((int)pfile.SmoothParam[SmoothIndex].InspRule2_Index, pfile.SmoothParam[SmoothIndex].InspRule2_Min, pfile.SmoothParam[SmoothIndex].InspRule2_Max, rect, area, TrackerIndex);
			array[2] = GetRule((int)pfile.SmoothParam[SmoothIndex].InspRule3_Index, pfile.SmoothParam[SmoothIndex].InspRule3_Min, pfile.SmoothParam[SmoothIndex].InspRule3_Max, rect, area, TrackerIndex);
			array[3] = GetRule((int)pfile.SmoothParam[SmoothIndex].InspRule4_Index, pfile.SmoothParam[SmoothIndex].InspRule4_Min, pfile.SmoothParam[SmoothIndex].InspRule4_Max, rect, area, TrackerIndex);
			if (pfile.SmoothParam[SmoothIndex].InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.SmoothParam[SmoothIndex].InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.SmoothParam[SmoothIndex].InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.SmoothParam[SmoothIndex].InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.SmoothParam[SmoothIndex].InspRule4_Index > BlobCondition.EM_NOT_USE)
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
			if (TrackerIndex == 0)
			{
				if (pfile.MeasureUnit == 0)
				{
					if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, アスペクト比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}um, 高:{4}um, 面積:{5}um, アスペクト比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
				}
				else if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm², アスペクト比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
					}
					else
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}mm, 高:{4}mm, 面積:{5}mm, アスペクト比:{6:N3}, 面積比:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
				}
				else
				{
					Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
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
						if (buffers.LanguageType == 1)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9:N3}, AB寬度差:{10:N3}, AB寬度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9:N3}, AB宽度差:{10:N3}, AB宽度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um², アスペクト比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高さ差:{9:N3}, AB幅差:{10:N3}, AB幅比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9:N3}, AB Width Diff.:{10:N3}, AB Width Ratio:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9}, AB寬度差:{10}, AB寬度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9}, AB宽度差:{10}, AB宽度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}um, 高さ:{4}um, 面積:{5}um, アスペクト比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高さ差:{9}, AB幅差:{10}, AB幅比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9}, AB Width Diff.:{10}, AB Width Ratio:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
				}
				else if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9:N3}, AB寬度差:{10:N3}, AB寬度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9:N3}, AB宽度差:{10:N3}, AB宽度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm², アスペクト比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高さ差:{9:N3}, AB幅差:{10:N3}, AB幅比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9:N3}, AB Width Diff.:{10:N3}, AB Width Ratio:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9}, AB寬度差:{10}, AB寬度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9}, AB宽度差:{10}, AB宽度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9}, AB宽度差:{10}, AB宽度比:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
				}
				else
				{
					Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9}, AB Width Diff.:{10}, AB Width Ratio:{11:N3} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text = "ST03-SmoothTool-CheckResult()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-004");
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
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-005");
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
			string text = "ST05-SmoothTool-CreateKernal()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-006");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		OffLine = offline;
		SmoothSuccess = true;
		Tracker1ResultText = "";
		Tracker2ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		TrackerError = false;
		int num = 0;
		if (!pfile.SmoothParam[index].TrackerON)
		{
			iresult = TOOL_RESULT.EM_NOT_USE;
			return true;
		}
		if (FaceIndex != 0 && pfile.SmoothParam[index].EnableSelectFace && FaceIndex != pfile.SmoothParam[index].FaceIndex)
		{
			iresult = TOOL_RESULT.EM_OK;
			return true;
		}
		SmoothIndex = index;
		GetInspeRect(index);
		_ = new bool[2];
		int channelIndex = (int)pfile.SmoothParam[index].ChannelIndex;
		bool[] array = new bool[2];
		Mat KernalH = new Mat();
		Mat KernalV = new Mat();
		int thdDown = pfile.SmoothParam[index].ThdDown;
		int thdUp = pfile.SmoothParam[index].ThdUp;
		int tHDLimit_Min = pfile.SmoothParam[index].THDLimit_Min;
		int tHDLimit_Max = pfile.SmoothParam[index].THDLimit_Max;
		num = 1;
		CreateKernal(ref KernalH, ref KernalV, pfile.SmoothParam[index].KernalCountH, pfile.SmoothParam[index].KernalCountV);
		try
		{
			BlobLine1.Clear();
			BlobLine2.Clear();
			MultiBlob1.Clear();
			MultiBlob2.Clear();
			for (int i = 0; i < 2; i++)
			{
				if (!pfile.SmoothParam[index].ThdUpON && !pfile.SmoothParam[index].ThdDownON)
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
				if (i == 1 && !pfile.SmoothParam[index].SearchRect2ON)
				{
					continue;
				}
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = true;
					if (i == 0)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です！\n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅または高さが0です！\n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					TrackerError = true;
					if (pfile.SmoothParam[index].TrackErrorSetNG)
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
				num = 2;
				InspeRect[i] &= new Rect(1, 1, buffers.GetChannelMat(channelIndex).Cols - 1, buffers.GetChannelMat(channelIndex).Rows - 1);
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = true;
					if (i == 0)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です！\n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが0です！\n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					TrackerError = true;
					if (pfile.SmoothParam[index].TrackErrorSetNG)
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
				num = 3;
				if (InspeRect[i].Width <= pfile.SmoothParam[index].KernalCountH)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					if (i == 0)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬小於計算範圍! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽小于计算范围! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅が計算範囲より小さいです！\n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Width too small! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬小於計算範圍! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽小于计算范围 \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域幅が計算範囲より小さい \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Width too small! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					TrackerError = true;
					array[i] = true;
					Binary[i].SetTo(0.0);
					continue;
				}
				num = 4;
				if (InspeRect[i].Height <= pfile.SmoothParam[index].KernalCountV)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					if (i == 0)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域高小於計算範圍! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域高小于计算范围! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の高さが計算範囲より小さいです！\n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Height too small! \n", index + 1, pfile.SmoothParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域高小於計算範圍! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 检测区域高小于计算范围! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の高さが計算範囲より小さいです！\n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Height too small! \n", index + 1, pfile.SmoothParam[index].TrackerName, "B");
					}
					TrackerError = true;
					array[i] = true;
					Binary[i].SetTo(0.0);
					continue;
				}
				num = 5;
				buffers.TimeStart();
				new Mat(buffers.GetChannelMat(channelIndex), InspeRect[i]).CopyTo(SelectMat);
				Cv2.Filter2D(SelectMat, result1, MatType.CV_8UC1, KernalH);
				Cv2.Filter2D(SelectMat, result2, MatType.CV_8UC1, KernalV);
				num = 6;
				if (pfile.SmoothParam[index].ThdUpON && pfile.SmoothParam[index].ThdDownON)
				{
					Mat mat = new Mat();
					Cv2.Subtract(SelectMat, result1, FilterMat1);
					Cv2.Subtract(SelectMat, result2, FilterMat2);
					Cv2.InRange(FilterMat1, thdUp, 255.0, FilterMat1);
					Cv2.InRange(FilterMat2, thdUp, 255.0, FilterMat2);
					Cv2.BitwiseOr(FilterMat1, FilterMat2, mat);
					Cv2.Subtract(result1, SelectMat, FilterMat1);
					Cv2.Subtract(result2, SelectMat, FilterMat2);
					Cv2.InRange(FilterMat1, thdDown, 255.0, FilterMat1);
					Cv2.InRange(FilterMat2, thdDown, 255.0, FilterMat2);
					Cv2.BitwiseOr(FilterMat1, FilterMat2, Binary[i]);
					Cv2.BitwiseOr(mat, Binary[i], Binary[i]);
				}
				else if (pfile.SmoothParam[index].ThdDownON)
				{
					Cv2.Subtract(result1, SelectMat, FilterMat1);
					Cv2.Subtract(result2, SelectMat, FilterMat2);
					Cv2.InRange(FilterMat1, thdDown, 255.0, FilterMat1);
					Cv2.InRange(FilterMat2, thdDown, 255.0, FilterMat2);
					Cv2.BitwiseOr(FilterMat1, FilterMat2, Binary[i]);
				}
				else
				{
					if (!pfile.SmoothParam[index].ThdUpON)
					{
						iresult = TOOL_RESULT.EM_NOT_USE;
						Binary[i].SetTo(0.0);
						return true;
					}
					Cv2.Subtract(SelectMat, result1, FilterMat1);
					Cv2.Subtract(SelectMat, result2, FilterMat2);
					Cv2.InRange(FilterMat1, thdUp, 255.0, FilterMat1);
					Cv2.InRange(FilterMat2, thdUp, 255.0, FilterMat2);
					Cv2.BitwiseOr(FilterMat1, FilterMat2, Binary[i]);
				}
				num = 7;
				int num2 = pfile.SmoothParam[index].KernalCountV / 2 + 1;
				int num3 = pfile.SmoothParam[index].KernalCountH / 2 + 1;
				Rect roi = new Rect(0, 0, Binary[i].Width, num2);
				Rect roi2 = new Rect(0, Binary[i].Height - num2, Binary[i].Width, num2);
				Rect roi3 = new Rect(0, 0, num3, Binary[i].Height);
				Rect roi4 = new Rect(Binary[i].Width - num3, 0, num3, Binary[i].Height);
				new Mat(Binary[i], roi).SetTo(0.0);
				new Mat(Binary[i], roi2).SetTo(0.0);
				new Mat(Binary[i], roi3).SetTo(0.0);
				new Mat(Binary[i], roi4).SetTo(0.0);
				num = 8;
				if (tHDLimit_Min != 0 || tHDLimit_Max != 255)
				{
					Cv2.InRange(SelectMat, tHDLimit_Min, tHDLimit_Max, SelectMat);
					Cv2.BitwiseAnd(SelectMat, Binary[i], Binary[i]);
				}
				DoMorph(ref Binary[i], (int)pfile.SmoothParam[index].DE_Start_Index, pfile.SmoothParam[index].DE_Start_Count);
				DoMorph(ref Binary[i], (int)pfile.SmoothParam[index].DE_End_Index, pfile.SmoothParam[index].DE_End_Count);
				num = 9;
				if (pfile.SmoothParam[index].MaskSelectIndex > 0)
				{
					if (!pfile.MaskToolON)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具未開啟! \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具未开启! \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 検出領域はマスクツールを使用していますが、マスクツールが有効になっていません！\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool has been Disable! \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName);
						}
						iresult = TOOL_RESULT.EM_OK;
						array[i] = true;
						continue;
					}
					int num4 = pfile.SmoothParam[index].MaskSelectIndex - 1;
					if (!pfile.MaskParam[num4].TrackerON)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具[{2}]未開啟! \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, num4 + 1);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具[{2}]未开启! \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, num4 + 1);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 検出領域はマスクツールを使用していますが、マスクツール[{2}]はオフになっています！ \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, num4 + 1);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool [{2}] has been Disable! \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, num4 + 1);
						}
						iresult = TOOL_RESULT.EM_OK;
						array[i] = true;
						continue;
					}
					num = 10;
					buffers.GetChannelMat(0).CopyTo(MaskTemp);
					MaskTemp.SetTo(0.0);
					if (pfile.MaskParam[num4].Mask1 != null)
					{
						bool flag = false;
						Rect[] array2 = new Rect[2]
						{
							new Rect(pfile.MaskParam[num4].Mask1Pt.X, pfile.MaskParam[num4].Mask1Pt.Y, pfile.MaskParam[num4].Mask1.Width, pfile.MaskParam[num4].Mask1.Height),
							default(Rect)
						};
						if (pfile.MaskParam[num4].SearchRect2ON && num4 < 20 && pfile.MaskParam[num4].Mask2 != null)
						{
							array2[1] = new Rect(pfile.MaskParam[num4].Mask2Pt.X, pfile.MaskParam[num4].Mask2Pt.Y, pfile.MaskParam[num4].Mask2.Width, pfile.MaskParam[num4].Mask2.Height);
						}
						else
						{
							array2[1] = new Rect(0, 0, 0, 0);
						}
						if (array2[0].Width > 0 && array2[0].Height > 0)
						{
							using Mat m = new Mat(MaskTemp, array2[0]);
							using Mat mat2 = pfile.MaskParam[num4].Mask1.Clone();
							mat2.CopyTo(m);
							flag = true;
						}
						if (pfile.MaskParam[num4].SearchRect2ON && array2[1].Width > 0 && array2[1].Height > 0 && num4 < 20)
						{
							using Mat m2 = new Mat(MaskTemp, array2[1]);
							using Mat mat3 = pfile.MaskParam[num4].Mask2.Clone();
							mat3.CopyTo(m2);
							flag = true;
						}
						if (flag)
						{
							Mat mat4 = new Mat(MaskTemp, InspeRect[i]);
							if (pfile.SmoothParam[index].UseMaskAsInspArea)
							{
								Cv2.BitwiseAnd(Binary[i], mat4, Binary[i]);
							}
							else
							{
								Cv2.BitwiseNot(mat4, mat4);
								Cv2.BitwiseAnd(Binary[i], mat4, Binary[i]);
							}
						}
					}
					num = 11;
				}
				if (i == 0)
				{
					buffers.DoFilllet(ref Binary[i], new Rect(0, 0, Binary[i].Width, Binary[i].Height), pfile.SmoothParam[index].T1FillletCount0, pfile.SmoothParam[index].T1FillletCount1, pfile.SmoothParam[index].T1FillletCount2, pfile.SmoothParam[index].T1FillletCount3);
				}
				else
				{
					buffers.DoFilllet(ref Binary[i], new Rect(0, 0, Binary[i].Width, Binary[i].Height), pfile.SmoothParam[index].T2FillletCount0, pfile.SmoothParam[index].T2FillletCount1, pfile.SmoothParam[index].T2FillletCount2, pfile.SmoothParam[index].T2FillletCount3);
				}
				if (pfile.MatchToolON)
				{
					Mat mat5 = buffers.GetChannelMat(0).Clone();
					if (pfile.SmoothParam[index].UseMatchTextMask1 > 0)
					{
						mat5.SetTo(255.0);
						int num5 = pfile.SmoothParam[index].UseMatchTextMask1 - 1;
						if (pfile.MatchParam[num5].TextMatResult)
						{
							Rect textRect = pfile.MatchParam[num5].TextRect;
							textRect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect.Width > 0 && textRect.Height > 0 && Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
							{
								using Mat m3 = new Mat(mat5, textRect);
								Mat mat6 = pfile.MatchParam[num5].TextMat.Clone();
								DoMorph(ref mat6, (int)pfile.SmoothParam[index].MatchTextDE_Start1_Index, pfile.SmoothParam[index].MatchTextDE_Start1_Count);
								Cv2.BitwiseNot(mat6, mat6);
								mat6.CopyTo(m3);
								Mat mat7 = MM.newMat(mat5, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(Binary[i], mat7, Binary[i]);
								MM.GiveBack(mat7);
							}
						}
					}
					if (pfile.SmoothParam[index].UseMatchTextMask2 > 0)
					{
						mat5.SetTo(255.0);
						int num6 = pfile.SmoothParam[index].UseMatchTextMask2 - 1;
						if (pfile.MatchParam[num6].TextMatResult)
						{
							Rect textRect2 = pfile.MatchParam[num6].TextRect;
							textRect2 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect2.Width > 0 && textRect2.Height > 0 && Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
							{
								using Mat m4 = new Mat(mat5, textRect2);
								Mat mat8 = pfile.MatchParam[num6].TextMat.Clone();
								DoMorph(ref mat8, (int)pfile.SmoothParam[index].MatchTextDE_Start2_Index, pfile.SmoothParam[index].MatchTextDE_Start2_Count);
								Cv2.BitwiseNot(mat8, mat8);
								mat8.CopyTo(m4);
								Mat mat9 = MM.newMat(mat5, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(Binary[i], mat9, Binary[i]);
								MM.GiveBack(mat9);
							}
						}
					}
					if (pfile.SmoothParam[index].UseMatchTextMask3 > 0)
					{
						mat5.SetTo(255.0);
						int num7 = pfile.SmoothParam[index].UseMatchTextMask3 - 1;
						if (pfile.MatchParam[num7].TextMatResult)
						{
							Rect textRect3 = pfile.MatchParam[num7].TextRect;
							textRect3 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect3.Width > 0 && textRect3.Height > 0 && Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
							{
								using Mat m5 = new Mat(mat5, textRect3);
								Mat mat10 = pfile.MatchParam[num7].TextMat.Clone();
								DoMorph(ref mat10, (int)pfile.SmoothParam[index].MatchTextDE_Start3_Index, pfile.SmoothParam[index].MatchTextDE_Start3_Count);
								Cv2.BitwiseNot(mat10, mat10);
								mat10.CopyTo(m5);
								Mat mat11 = MM.newMat(mat5, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(Binary[i], mat11, Binary[i]);
								MM.GiveBack(mat11);
							}
						}
					}
				}
				if (pfile.SmoothParam[index].BlobAnalysisSelect == 0)
				{
					array[i] = FindBlob(Binary[i], i);
				}
				else if (pfile.SmoothParam[index].BlobAnalysisSelect == 1)
				{
					if (i == 0)
					{
						array[i] = FindBlobLine(Binary[i], index, i, ref BlobLine1);
					}
					else
					{
						array[i] = FindBlobLine(Binary[i], index, i, ref BlobLine2);
					}
				}
				else if (pfile.SmoothParam[index].BlobAnalysisSelect == 2)
				{
					if (i == 0)
					{
						array[i] = findMultiBlob(Binary[i], index, i, ref MultiBlob1, pfile.SmoothParam[index].MB_AllowNGCount, i);
					}
					else
					{
						array[i] = findMultiBlob(Binary[i], index, i, ref MultiBlob2, pfile.SmoothParam[index].MB_AllowNGCount, i);
					}
				}
				num = 12;
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("{0}:{1}-DebugIndex={2}, ", index + 1, pfile.SmoothParam[index].TrackerName, num) + "ST06-SmoothTool-Inspect()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-007");
			}
			return false;
		}
		if (array[0] && array[1])
		{
			return true;
		}
		iresult = TOOL_RESULT.EM_NG;
		return false;
	}

	public void FillMaskMat(ref Mat mat, int index, int i)
	{
		if (pfile.SmoothParam[index].MaskSelectIndex <= 0 || !pfile.MaskToolON)
		{
			return;
		}
		int num = pfile.SmoothParam[index].MaskSelectIndex - 1;
		if (!pfile.MaskParam[num].TrackerON)
		{
			return;
		}
		Mat channelMat = buffers.GetChannelMat(0);
		Mat mat2 = new Mat(channelMat.Height, channelMat.Width, MatType.CV_8UC1);
		mat2.SetTo(255.0);
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
			using Mat m = new Mat(mat2, array[0]);
			using Mat mat3 = pfile.MaskParam[num].Mask1.Clone();
			mat3.CopyTo(m);
			flag = true;
		}
		if (pfile.MaskParam[num].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num < 20)
		{
			using Mat m2 = new Mat(mat2, array[1]);
			using Mat mat4 = pfile.MaskParam[num].Mask2.Clone();
			mat4.CopyTo(m2);
			flag = true;
		}
		if (flag && mat.Height > 12 && flag && mat.Height > 12)
		{
			Mat mat5 = new Mat(mat2, InspeRect[i]);
			int num2 = mat.Height / 2;
			if (pfile.SmoothParam[index].UseMaskAsInspArea)
			{
				Mat mat6 = new Mat(mat, new Rect(0, num2 - 3, mat.Width, 6));
				Mat mat7 = new Mat(mat5, new Rect(0, num2 - 3, mat5.Width, 6));
				Scalar scalar = mat6.Mean(mat7);
				mat.SetTo(scalar.Val0, mat5);
			}
			else
			{
				Cv2.BitwiseNot(mat5, mat5);
				Mat mat8 = new Mat(mat, new Rect(0, num2 - 3, mat.Width, 6));
				Mat mat9 = new Mat(mat5, new Rect(0, num2 - 3, mat5.Width, 6));
				Scalar scalar2 = mat8.Mean(mat9);
				mat.SetTo(scalar2.Val0, mat5);
			}
		}
	}

	public void FillMatchMat(ref Mat mat, int index, int i)
	{
		if (!pfile.MatchToolON)
		{
			return;
		}
		Mat channelMat = buffers.GetChannelMat(0);
		if (pfile.SmoothParam[index].UseMatchTextMask1 > 0)
		{
			int num = pfile.SmoothParam[index].UseMatchTextMask1 - 1;
			if (pfile.MatchParam[num].TextMatResult)
			{
				Rect rect = new Rect(pfile.MatchParam[num].TextRect.Left - InspeRect[i].Left, pfile.MatchParam[num].TextRect.Top - InspeRect[i].Top, pfile.MatchParam[num].TextRect.Width, pfile.MatchParam[num].TextRect.Height);
				rect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
				rect &= new Rect(0, 0, mat.Cols, mat.Rows);
				if (rect.Width > 2 && rect.Height > 2)
				{
					channelMat.CopyTo(MatchMatBuf1);
					MatchMatBuf1.SetTo(0.0);
					using Mat mat2 = new Mat(mat, new Rect(rect.Left, rect.Top, rect.Width, 2));
					using Mat m = new Mat(MatchMatBuf1, new Rect(rect.Left + InspeRect[i].Left, rect.Top + InspeRect[i].Top, rect.Width, rect.Height));
					using Mat mask = new Mat(MatchMatBuf1, InspeRect[i]).Clone();
					Mat mat3 = pfile.MatchParam[num].TextMat.Clone();
					DoMorph(ref mat3, (int)pfile.SmoothParam[index].MatchTextDE_Start1_Index, pfile.SmoothParam[index].MatchTextDE_Start1_Count);
					mat3.CopyTo(m);
					Scalar value = mat2.Mean();
					mat.SetTo(value, mask);
				}
			}
		}
		if (pfile.SmoothParam[index].UseMatchTextMask2 > 0)
		{
			int num2 = pfile.SmoothParam[index].UseMatchTextMask2 - 1;
			if (pfile.MatchParam[num2].TextMatResult)
			{
				Rect rect2 = new Rect(pfile.MatchParam[num2].TextRect.Left - InspeRect[i].Left, pfile.MatchParam[num2].TextRect.Top - InspeRect[i].Top, pfile.MatchParam[num2].TextRect.Width, pfile.MatchParam[num2].TextRect.Height);
				rect2 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
				rect2 &= new Rect(0, 0, mat.Cols, mat.Rows);
				if (rect2.Width > 2 && rect2.Height > 2)
				{
					channelMat.CopyTo(MatchMatBuf2);
					MatchMatBuf2.SetTo(0.0);
					using Mat mat4 = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, 2));
					using Mat m2 = new Mat(MatchMatBuf2, new Rect(rect2.Left + InspeRect[i].Left, rect2.Top + InspeRect[i].Top, rect2.Width, rect2.Height));
					using Mat mask2 = new Mat(MatchMatBuf2, InspeRect[i]).Clone();
					Mat mat5 = pfile.MatchParam[num2].TextMat.Clone();
					DoMorph(ref mat5, (int)pfile.SmoothParam[index].MatchTextDE_Start2_Index, pfile.SmoothParam[index].MatchTextDE_Start2_Count);
					mat5.CopyTo(m2);
					Scalar value2 = mat4.Mean();
					mat.SetTo(value2, mask2);
				}
			}
		}
		if (pfile.SmoothParam[index].UseMatchTextMask3 <= 0)
		{
			return;
		}
		int num3 = pfile.SmoothParam[index].UseMatchTextMask3 - 1;
		if (!pfile.MatchParam[num3].TextMatResult)
		{
			return;
		}
		Rect rect3 = new Rect(pfile.MatchParam[num3].TextRect.Left - InspeRect[i].Left, pfile.MatchParam[num3].TextRect.Top - InspeRect[i].Top, pfile.MatchParam[num3].TextRect.Width, pfile.MatchParam[num3].TextRect.Height);
		rect3 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
		rect3 &= new Rect(0, 0, mat.Cols, mat.Rows);
		if (rect3.Width <= 2 || rect3.Height <= 2)
		{
			return;
		}
		channelMat.CopyTo(MatchMatBuf3);
		MatchMatBuf3.SetTo(0.0);
		using Mat mat6 = new Mat(mat, new Rect(rect3.Left, rect3.Top, rect3.Width, 2));
		using Mat m3 = new Mat(MatchMatBuf3, new Rect(rect3.Left + InspeRect[i].Left, rect3.Top + InspeRect[i].Top, rect3.Width, rect3.Height));
		using Mat mask3 = new Mat(MatchMatBuf3, InspeRect[i]).Clone();
		Mat mat7 = pfile.MatchParam[num3].TextMat.Clone();
		DoMorph(ref mat7, (int)pfile.SmoothParam[index].MatchTextDE_Start3_Index, pfile.SmoothParam[index].MatchTextDE_Start3_Count);
		mat7.CopyTo(m3);
		Scalar value3 = mat6.Mean();
		mat.SetTo(value3, mask3);
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
			switch (pfile.SmoothParam[index].MB_BlobSort)
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
			for (int num3 = 0; num3 < MultiBlob.Count && num3 < pfile.SmoothParam[index].MB_BlobNum; num3++)
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
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG個數={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", num2);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", num2);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", num2);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG Count={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", num2);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG個數={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", num2);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", num2);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG个数={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", num2);
				}
				else
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}] NG Count={3}\n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", num2);
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
			array[0] = GetRule((int)pfile.SmoothParam[SmoothIndex].MB_InspRule1_Index, pfile.SmoothParam[SmoothIndex].MB_InspRule1_Min, pfile.SmoothParam[SmoothIndex].MB_InspRule1_Max, rect, area, TrackIndex);
			array[1] = GetRule((int)pfile.SmoothParam[SmoothIndex].MB_InspRule2_Index, pfile.SmoothParam[SmoothIndex].MB_InspRule2_Min, pfile.SmoothParam[SmoothIndex].MB_InspRule2_Max, rect, area, TrackIndex);
			array[2] = GetRule((int)pfile.SmoothParam[SmoothIndex].MB_InspRule3_Index, pfile.SmoothParam[SmoothIndex].MB_InspRule3_Min, pfile.SmoothParam[SmoothIndex].MB_InspRule3_Max, rect, area, TrackIndex);
			array[3] = GetRule((int)pfile.SmoothParam[SmoothIndex].MB_InspRule4_Index, pfile.SmoothParam[SmoothIndex].MB_InspRule4_Min, pfile.SmoothParam[SmoothIndex].MB_InspRule4_Max, rect, area, TrackIndex);
			if (pfile.SmoothParam[SmoothIndex].MB_InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.SmoothParam[SmoothIndex].MB_InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.SmoothParam[SmoothIndex].MB_InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.SmoothParam[SmoothIndex].MB_InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.SmoothParam[SmoothIndex].MB_InspRule4_Index > BlobCondition.EM_NOT_USE)
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
			if (TrackIndex == 0)
			{
				if (pfile.MeasureUnit == 0)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}um, 高:{5:N3}um, 面积:{6:N3}um, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, アスペクト比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}um, Height:{5:N3}um, Area:{6:N3}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}mm, 高:{5:N3}mm, 面积:{6:N3}mm, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm, アスペクト比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else
				{
					Tracker1ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}mm, Height:{5:N3}mm, Area:{6:N3}mm, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "A", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
			}
			else if (pfile.MeasureUnit == 0)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}um, 高:{5:N3}um, 面积:{6:N3}um, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um², アスペクト比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
				else
				{
					Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}um, Height:{5:N3}um, Area:{6:N3}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
				}
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}mm, 高:{5:N3}mm, 面积:{6:N3}mm, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}mm, 高:{5:N3}mm, 面積:{6:N3}mm², アスペクト比:{7:N3}, 面積比:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			else
			{
				Tracker2ResultText += string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}mm, Height:{5:N3}mm, Area:{6:N3}mm, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", SmoothIndex + 1, pfile.SmoothParam[SmoothIndex].TrackerName, "B", BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text);
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text2 = "BT03-SmoothTool-checkMultiResult()-";
			text2 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Smooth-008");
			}
			return false;
		}
	}
}
