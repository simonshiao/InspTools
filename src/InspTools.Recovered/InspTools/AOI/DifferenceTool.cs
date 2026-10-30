using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class DifferenceTool
{
	private const int MaxTracker = 2;

	private CVBuffer buffers;

	public Mat[] Binary = new Mat[2];

	private ParamFile pfile;

	private Rect[] InspeRect = new Rect[2];

	public BlobStruct[] Blobs = new BlobStruct[2];

	public List<SimpleBlobStruct> BlobLine1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> BlobLine2 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob1 = new List<SimpleBlobStruct>();

	public List<SimpleBlobStruct> MultiBlob2 = new List<SimpleBlobStruct>();

	private int DifferenceIndex;

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool DifferenceSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public string Tracker2ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public DifferenceTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect[0] = new Rect(0, 0, 0, 0);
		InspeRect[1] = new Rect(0, 0, 0, 0);
		Binary[0] = new Mat();
		Binary[1] = new Mat();
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
		if (pfile.DifferenceParam[index].Tracker1UseAutoSize)
		{
			InspeRect[0].X = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize1_SelectIndex].L + pfile.DifferenceParam[index].Track1_L;
			InspeRect[0].Y = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize1_SelectIndex].T + pfile.DifferenceParam[index].Track1_T;
			InspeRect[0].Width = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize1_SelectIndex].Width - (pfile.DifferenceParam[index].Track1_L + pfile.DifferenceParam[index].Track1_R);
			InspeRect[0].Height = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize1_SelectIndex].Height - (pfile.DifferenceParam[index].Track1_T + pfile.DifferenceParam[index].Track1_B);
		}
		else
		{
			int width = pfile.DifferenceParam[index].Track1_R - pfile.DifferenceParam[index].Track1_L;
			int height = pfile.DifferenceParam[index].Track1_B - pfile.DifferenceParam[index].Track1_T;
			InspeRect[0].X = pfile.VSLocateParam[pfile.DifferenceParam[index].Tracker1AligmentXIndex].Value + pfile.DifferenceParam[index].Track1_L;
			InspeRect[0].Y = pfile.HSLocateParam[pfile.DifferenceParam[index].Tracker1AligmentYIndex].Value + pfile.DifferenceParam[index].Track1_T;
			InspeRect[0].Width = width;
			InspeRect[0].Height = height;
		}
		if (pfile.DifferenceParam[index].Tracker2UseAutoSize)
		{
			InspeRect[1].X = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize2_SelectIndex].L + pfile.DifferenceParam[index].Track2_L;
			InspeRect[1].Y = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize2_SelectIndex].T + pfile.DifferenceParam[index].Track2_T;
			InspeRect[1].Width = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize2_SelectIndex].Width - (pfile.DifferenceParam[index].Track2_L + pfile.DifferenceParam[index].Track2_R);
			InspeRect[1].Height = pfile.AdvLocateParam[pfile.DifferenceParam[index].AutoSize2_SelectIndex].Height - (pfile.DifferenceParam[index].Track2_T + pfile.DifferenceParam[index].Track2_B);
		}
		else
		{
			int width2 = pfile.DifferenceParam[index].Track2_R - pfile.DifferenceParam[index].Track2_L;
			int height2 = pfile.DifferenceParam[index].Track2_B - pfile.DifferenceParam[index].Track2_T;
			InspeRect[1].X = pfile.VSLocateParam[pfile.DifferenceParam[index].Tracker2AligmentXIndex].Value + pfile.DifferenceParam[index].Track2_L;
			InspeRect[1].Y = pfile.HSLocateParam[pfile.DifferenceParam[index].Tracker2AligmentYIndex].Value + pfile.DifferenceParam[index].Track2_T;
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
			string text = "DT01-DifferenceTool-filterNoise()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-001");
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
			switch (pfile.DifferenceParam[index].BlobLineSort)
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
			for (int num3 = 0; num3 < BlobLine.Count && num3 < pfile.DifferenceParam[index].BlobLineNum; num3++)
			{
				bool flag = true;
				if (BlobLine[num3].Width < pfile.DifferenceParam[index].BlobLineWidthMin || BlobLine[num3].Width > pfile.DifferenceParam[index].BlobLineWidthMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Height < pfile.DifferenceParam[index].BlobLineHeightMin || BlobLine[num3].Height > pfile.DifferenceParam[index].BlobLineHeightMax)
				{
					flag = false;
				}
				if (BlobLine[num3].Area < pfile.DifferenceParam[index].BlobLineAreaMin || BlobLine[num3].Area > pfile.DifferenceParam[index].BlobLineAreaMax)
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
					if (buffers.LanguageType == 1)
					{
						text2 += string.Format("[{0}:{1}][{2}] 線條寬度{3}, 線條高度{4}, 線條面積{5} [NG] \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 2)
					{
						text2 += string.Format("[{0}:{1}][{2}] 线条宽度{3}, 线条高度{4}, 线条面积{5} [NG] \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
					else if (buffers.LanguageType == 3)
					{
						string.Format("[{0}:{1}][{2}] 線の幅{3}, 線の高さ{4}, 線の面積{5} [NG] \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
					}
					else
					{
						text2 += string.Format("[{0}:{1}][{2}] Line Width={3}, Line Height={4}, Line Area={5}  [NG] \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, BlobLine[num3].Width, BlobLine[num3].Height, BlobLine[num3].Area);
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
			}
			if (num2 < pfile.DifferenceParam[index].BlobLineMin || num2 > pfile.DifferenceParam[index].BlobLineMax)
			{
				result = false;
			}
			string text3 = "";
			text3 = ((buffers.LanguageType == 1) ? (text3 + string.Format("[{0}:{1}][{2}] 線條合格數量為{3}，允許最小合格數為{4}，允許最大合格數為{5} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, num2, pfile.DifferenceParam[index].BlobLineMin, pfile.DifferenceParam[index].BlobLineMax)) : ((buffers.LanguageType == 2) ? (text3 + string.Format("[{0}:{1}][{2}] 线条合格数量为{3}，允许最小合格数为{4}，允许最大合格数为{5} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, num2, pfile.DifferenceParam[index].BlobLineMin, pfile.DifferenceParam[index].BlobLineMax)) : ((buffers.LanguageType != 3) ? (text3 + string.Format("[{0}:{1}][{2}] Passed Line={3}，Min Passs Line={4}，Max Passs Line={5} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, num2, pfile.DifferenceParam[index].BlobLineMin, pfile.DifferenceParam[index].BlobLineMax)) : (text3 + string.Format("[{0}:{1}][{2}] 線条合格数量は{3}です。 許容最小合格数は{4}です。 許容最大合格数は{5}です。 \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text, num2, pfile.DifferenceParam[index].BlobLineMin, pfile.DifferenceParam[index].BlobLineMax)))));
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
		else if (BlobLine.Count < pfile.DifferenceParam[index].BlobLineNum)
		{
			result = false;
			Blobs[TrackerIndex].result = false;
			string text4 = "";
			text4 = ((buffers.LanguageType == 1) ? (text4 + string.Format("[{0}:{1}][{2}] 線條數量為0 \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text)) : ((buffers.LanguageType == 2) ? (text4 + string.Format("[{0}:{1}][{2}] 线条数量为0 \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text)) : ((buffers.LanguageType != 3) ? (text4 + string.Format("[{0}:{1}][{2}] Passed Line = 0  \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text)) : (text4 + string.Format("[{0}:{1}][{2}] 線条数为0 \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text)))));
			if (TrackerIndex == 0)
			{
				Tracker1ResultText += text4;
			}
			else
			{
				Tracker2ResultText += text4;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool findBlob(Mat binary, int TrackerIndex)
	{
		try
		{
			buffers.TimeStart();
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
			buffers.TimeStop();
			buffers.TimeStart();
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
			Blobs[TrackerIndex].Width = rect.Width;
			Blobs[TrackerIndex].Height = rect.Height;
			Blobs[TrackerIndex].WHRatio = num4;
			Blobs[TrackerIndex].Area = num3;
			Blobs[TrackerIndex].NGRect = rect;
			Blobs[TrackerIndex].NGRect.X += InspeRect[TrackerIndex].X;
			Blobs[TrackerIndex].NGRect.Y += InspeRect[TrackerIndex].Y;
			buffers.TimeStop();
			return checkResult(rect, num, num4, TrackerIndex);
		}
		catch (Exception ex)
		{
			string text = "DT02-DifferenceTool-findBlob()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-002");
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
			array[0] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].InspRule1_Index, pfile.DifferenceParam[DifferenceIndex].InspRule1_Min, pfile.DifferenceParam[DifferenceIndex].InspRule1_Max, rect, area, TrackerIndex);
			array[1] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].InspRule2_Index, pfile.DifferenceParam[DifferenceIndex].InspRule2_Min, pfile.DifferenceParam[DifferenceIndex].InspRule2_Max, rect, area, TrackerIndex);
			array[2] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].InspRule3_Index, pfile.DifferenceParam[DifferenceIndex].InspRule3_Min, pfile.DifferenceParam[DifferenceIndex].InspRule3_Max, rect, area, TrackerIndex);
			array[3] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].InspRule4_Index, pfile.DifferenceParam[DifferenceIndex].InspRule4_Min, pfile.DifferenceParam[DifferenceIndex].InspRule4_Max, rect, area, TrackerIndex);
			if (pfile.DifferenceParam[DifferenceIndex].InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.DifferenceParam[DifferenceIndex].InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.DifferenceParam[DifferenceIndex].InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.DifferenceParam[DifferenceIndex].InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.DifferenceParam[DifferenceIndex].InspRule4_Index > BlobCondition.EM_NOT_USE)
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
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}um、高さ:{4:N3}um、面積:{5:N3}um、縦横比:{6:N3}、面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
							else
							{
								Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}um, 高さ:{4}um, 面積:{5}um, 縦横比:{6:N3}, 面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
						}
					}
					else if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}mm、高さ:{4:N3}mm、面積:{5:N3}mm、縦横比:{6:N3}、面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs[0].AreaRatio);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}mm, 高さ:{4}mm, 面積:{5}mm, 縦横比:{6:N3}, 面積比:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
					}
					else
					{
						Tracker1ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs[0].AreaRatio);
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
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}um, 高:{4:N3}um, 面積:{5:N3}um, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9:N3}, AB寬度差:{10:N3}, AB寬度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}um, 高:{4:N3}um, 面积:{5:N3}um, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9:N3}, AB宽度差:{10:N3}, AB宽度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}um, 高さ:{4:N3}um, 面積:{5:N3}um, 縦横比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高低差:{9:N3}, AB幅差:{10:N3}, AB幅比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
							else
							{
								Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}um, Height:{4:N3}um, Area:{5:N3}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9:N3}, AB Width Diff.:{10:N3}, AB Width Ratio:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}um, 高:{4}um, 面積:{5}um, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9}, AB寬度差:{10}, AB寬度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}um, 高:{4}um, 面积:{5}um, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9}, AB宽度差:{10}, AB宽度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}um, 高さ:{4}um, 面積:{5}um, 縦横比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高低差:{9}, AB幅差:{10}, AB幅比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}um, Height:{4}um, Area:{5}um, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9}, AB Width Diff.:{10}, AB Width Ratio:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
					}
					else if (pfile.UseFloat)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3:N3}mm, 高:{4:N3}mm, 面積:{5:N3}mm, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9:N3}, AB寬度差:{10:N3}, AB寬度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3:N3}mm, 高:{4:N3}mm, 面积:{5:N3}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9:N3}, AB宽度差:{10:N3}, AB宽度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3:N3}mm, 高さ:{4:N3}mm, 面積:{5:N3}mm, 縦横比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高低差:{9:N3}, AB幅差:{10:N3}, AB幅比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
						else
						{
							Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3:N3}mm, Height:{4:N3}mm, Area:{5:N3}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9:N3}, AB Width Diff.:{10:N3}, AB Width Ratio:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, Blobs[1].HeightDiff, Blobs[1].WidthDiff, Blobs[1].WidthRatio);
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 寬:{3}mm, 高:{4}mm, 面積:{5}mm, 長寬比:{6:N3}, 面積比:{7:N3}, AB區域面積比:{8:N3}, AB高度差:{9}, AB寬度差:{10}, AB寬度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 宽:{3}mm, 高:{4}mm, 面积:{5}mm, 长宽比:{6:N3}, 面积比:{7:N3}, AB区域面积比:{8:N3}, AB高度差:{9}, AB宽度差:{10}, AB宽度比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 幅:{3}mm, 高さ:{4}mm, 面積:{5}mm, 縦横比:{6:N3}, 面積比:{7:N3}, AB領域面積比:{8:N3}, AB高低差:{9}, AB幅差:{10}, AB幅比:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Width:{3}mm, Height:{4}mm, Area:{5}mm, WH Ratio:{6:N3}, Area Ratio:{7:N3}, AB Area Ratio:{8:N3}, AB Height Diff.:{9}, AB Width Diff.:{10}, AB Width Ratio:{11:N3} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "B", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), area, whratio * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, Blobs[1].AreaRatio, Blobs[1].TwoAreaRatio, (int)Blobs[1].HeightDiff, (int)Blobs[1].WidthDiff, Blobs[1].WidthRatio);
					}
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text = "DT03-DifferenceTool-checkResult()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-003");
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
			string text = "SMT04-DifferenceTool-GetRule()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-004");
			}
			return false;
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
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
			string text = "DT05-DifferenceTool-CreateKernal()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-005");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		OffLine = offline;
		DifferenceSuccess = true;
		Tracker1ResultText = "";
		Tracker2ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		if (!pfile.DifferenceParam[index].TrackerON)
		{
			iresult = TOOL_RESULT.EM_NOT_USE;
			return true;
		}
		if (FaceIndex != 0 && pfile.DifferenceParam[index].EnableSelectFace && FaceIndex != pfile.DifferenceParam[index].FaceIndex)
		{
			iresult = TOOL_RESULT.EM_OK;
			return true;
		}
		DifferenceIndex = index;
		GetInspeRect(index);
		_ = new bool[2];
		int channelIndex = (int)pfile.DifferenceParam[index].ChannelIndex;
		bool[] array = new bool[2];
		_ = ref pfile.DifferenceParam[index];
		_ = ref pfile.DifferenceParam[index];
		int tHDLimit_Min = pfile.DifferenceParam[index].THDLimit_Min;
		int tHDLimit_Max = pfile.DifferenceParam[index].THDLimit_Max;
		try
		{
			BlobLine1.Clear();
			BlobLine2.Clear();
			MultiBlob1.Clear();
			MultiBlob2.Clear();
			for (int i = 0; i < 2; i++)
			{
				if (!pfile.DifferenceParam[index].ThdDownON && !pfile.DifferenceParam[index].ThdUpON)
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
				if (i == 1 && !pfile.DifferenceParam[index].SearchRect2ON)
				{
					continue;
				}
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					if (i == 0)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 检测区域宽高为0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが 0 です！\n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが 0 です！ \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					if (pfile.DifferenceParam[index].TrackErrorSetNG)
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
				InspeRect[i] &= new Rect(1, 1, buffers.GetChannelMat(channelIndex).Cols - 1, buffers.GetChannelMat(channelIndex).Rows - 1);
				if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					if (i == 0)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが 0 です！ \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "A");
						}
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 檢測區域寬高為0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] 検出領域の幅と高さが 0 です！ \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					else
					{
						Tracker2ResultText = string.Format("[{0}:{1}][{2}] Tracker Value <= 0! \n", index + 1, pfile.DifferenceParam[index].TrackerName, "B");
					}
					if (pfile.DifferenceParam[index].TrackErrorSetNG)
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
				int channelIndex2 = (int)pfile.DifferenceParam[index].ChannelIndex;
				Mat mat = new Mat(buffers.GetChannelMat(channelIndex2), InspeRect[i]).Clone();
				Mat mat2 = mat.Clone();
				Mat mat3 = mat.Clone();
				Mat mat4 = mat.Clone();
				Mat mat5 = mat.Clone();
				int thdUp = pfile.DifferenceParam[index].ThdUp;
				int thdDown = pfile.DifferenceParam[index].ThdDown;
				int p2PIntervalX = pfile.DifferenceParam[index].P2PIntervalX;
				int p2PIntervalY = pfile.DifferenceParam[index].P2PIntervalY;
				if (InspeRect[i].Width <= p2PIntervalX * 3 + 1)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					string arg = "A";
					if (i == 1)
					{
						arg = "B";
					}
					string text = "";
					text = ((buffers.LanguageType == 1) ? string.Format("[{0}:{1}][{2}] 檢測區域寬小於計算範圍! \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg) : ((buffers.LanguageType == 2) ? string.Format("[{0}:{1}][{2}] 检测区域宽小于计算范围! \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg) : ((buffers.LanguageType != 3) ? string.Format("[{0}:{1}][{2}] Tracker Width too small! \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg) : string.Format("[{0}:{1}][{2}] 検出領域の幅が計算範囲よりも小さいです！ \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg))));
					if (i == 0)
					{
						Tracker1ResultText = text;
					}
					else
					{
						Tracker2ResultText = text;
					}
					array[i] = true;
					Binary[i].SetTo(0.0);
					continue;
				}
				if (InspeRect[i].Height <= p2PIntervalY * 3 + 1)
				{
					iresult = TOOL_RESULT.EM_OK;
					Blobs[i].result = false;
					string arg2 = "A";
					if (i == 1)
					{
						arg2 = "B";
					}
					string text2 = "";
					text2 = ((buffers.LanguageType == 1) ? string.Format("[{0}:{1}][{2}] 檢測區域高小於計算範圍! \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg2) : ((buffers.LanguageType == 2) ? string.Format("[{0}:{1}][{2}] 检测区域高小于计算范围! \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg2) : ((buffers.LanguageType != 3) ? string.Format("[{0}:{1}][{2}] Tracker Height too small! \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg2) : string.Format("[{0}:{1}][{2}] 検出領域の高さが計算範囲よりも小さいです！ \n", index + 1, pfile.DifferenceParam[index].TrackerName, arg2))));
					if (i == 0)
					{
						Tracker1ResultText = text2;
					}
					else
					{
						Tracker2ResultText = text2;
					}
					array[i] = true;
					Binary[i].SetTo(0.0);
					continue;
				}
				Rect roi = new Rect(p2PIntervalX, p2PIntervalY, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi2 = new Rect(0, p2PIntervalY, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi3 = new Rect(p2PIntervalX * 2, p2PIntervalY, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi4 = new Rect(p2PIntervalX, 0, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi5 = new Rect(p2PIntervalX, p2PIntervalY * 2, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi6 = new Rect(0, 0, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi7 = new Rect(p2PIntervalX * 2, 0, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				new Rect(0, p2PIntervalY * 2, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Rect roi8 = new Rect(p2PIntervalX * 2, p2PIntervalY * 2, mat.Cols - p2PIntervalX * 2, mat.Rows - p2PIntervalY * 2);
				Mat mat6 = new Mat(mat, roi).Clone();
				Mat mat7 = new Mat(mat, roi2).Clone();
				Mat mat8 = new Mat(mat, roi3).Clone();
				Mat mat9 = new Mat(mat, roi4).Clone();
				Mat mat10 = new Mat(mat, roi5).Clone();
				Mat mat11 = new Mat(mat, roi6).Clone();
				Mat mat12 = new Mat(mat, roi7).Clone();
				Mat mat13 = new Mat(mat, roi6).Clone();
				Mat mat14 = new Mat(mat, roi8).Clone();
				buffers.TimeStart();
				if (!pfile.DifferenceParam[index].ThdDownON && !pfile.DifferenceParam[index].ThdUpON)
				{
					iresult = TOOL_RESULT.EM_NOT_USE;
					Binary[i].SetTo(0.0);
					return true;
				}
				if (pfile.DifferenceParam[index].ThdDownON)
				{
					using (Mat mat15 = new Mat())
					{
						using Mat mat16 = new Mat();
						using Mat mat17 = new Mat();
						using Mat mat18 = new Mat();
						Cv2.Subtract(mat7, mat6, mat15);
						Cv2.Subtract(mat8, mat6, mat16);
						Cv2.Subtract(mat9, mat6, mat17);
						Cv2.Subtract(mat10, mat6, mat18);
						Cv2.Threshold(mat15, mat15, thdDown, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat16, mat16, thdDown, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat17, mat17, thdDown, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat18, mat18, thdDown, 255.0, ThresholdTypes.Binary);
						using Mat mat19 = new Mat(mat, roi).Clone();
						using Mat mat20 = new Mat(mat, roi).Clone();
						using Mat mat21 = new Mat(mat, roi).Clone();
						Cv2.BitwiseOr(mat15, mat16, mat19);
						Cv2.BitwiseOr(mat17, mat18, mat20);
						Cv2.BitwiseOr(mat19, mat20, mat21);
						Rect roi9 = new Rect(p2PIntervalX, p2PIntervalY, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						Rect roi10 = new Rect(0, 0, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						mat2.SetTo(0.0);
						using Mat mat22 = new Mat(mat21, roi10);
						using Mat m = new Mat(mat2, roi9);
						mat22.CopyTo(m);
					}
					if (pfile.DifferenceParam[index].InspDirection == 1)
					{
						using Mat mat23 = new Mat();
						using Mat mat24 = new Mat();
						using Mat mat25 = new Mat();
						using Mat mat26 = new Mat();
						Cv2.Subtract(mat11, mat6, mat23);
						Cv2.Subtract(mat12, mat6, mat24);
						Cv2.Subtract(mat13, mat6, mat25);
						Cv2.Subtract(mat14, mat6, mat26);
						Cv2.Threshold(mat23, mat23, thdDown, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat24, mat24, thdDown, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat25, mat25, thdDown, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat26, mat26, thdDown, 255.0, ThresholdTypes.Binary);
						using Mat mat27 = new Mat(mat, roi).Clone();
						using Mat mat28 = new Mat(mat, roi).Clone();
						using Mat mat29 = new Mat(mat, roi).Clone();
						Cv2.BitwiseOr(mat23, mat24, mat27);
						Cv2.BitwiseOr(mat25, mat26, mat28);
						Cv2.BitwiseOr(mat27, mat28, mat29);
						Rect roi11 = new Rect(p2PIntervalX, p2PIntervalY, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						Rect roi12 = new Rect(0, 0, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						mat4.SetTo(0.0);
						using (Mat mat30 = new Mat(mat29, roi12))
						{
							using Mat m2 = new Mat(mat4, roi11);
							mat30.CopyTo(m2);
						}
						Cv2.BitwiseOr(mat2, mat4, mat2);
					}
				}
				if (pfile.DifferenceParam[index].ThdUpON)
				{
					using (Mat mat31 = new Mat())
					{
						using Mat mat32 = new Mat();
						using Mat mat33 = new Mat();
						using Mat mat34 = new Mat();
						Cv2.Subtract(mat6, mat7, mat31);
						Cv2.Subtract(mat6, mat8, mat32);
						Cv2.Subtract(mat6, mat9, mat33);
						Cv2.Subtract(mat6, mat10, mat34);
						Cv2.Threshold(mat31, mat31, thdUp, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat32, mat32, thdUp, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat33, mat33, thdUp, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat34, mat34, thdUp, 255.0, ThresholdTypes.Binary);
						using Mat mat35 = new Mat(mat, roi).Clone();
						using Mat mat36 = new Mat(mat, roi).Clone();
						using Mat mat37 = new Mat(mat, roi).Clone();
						Cv2.BitwiseOr(mat31, mat32, mat35);
						Cv2.BitwiseOr(mat33, mat34, mat36);
						Cv2.BitwiseOr(mat35, mat36, mat37);
						Rect roi13 = new Rect(p2PIntervalX, p2PIntervalY, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						Rect roi14 = new Rect(0, 0, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						mat3.SetTo(0.0);
						using Mat mat38 = new Mat(mat37, roi14);
						using Mat m3 = new Mat(mat3, roi13);
						mat38.CopyTo(m3);
					}
					if (pfile.DifferenceParam[index].InspDirection == 1)
					{
						using Mat mat39 = new Mat();
						using Mat mat40 = new Mat();
						using Mat mat41 = new Mat();
						using Mat mat42 = new Mat();
						Cv2.Subtract(mat6, mat11, mat39);
						Cv2.Subtract(mat6, mat12, mat40);
						Cv2.Subtract(mat6, mat13, mat41);
						Cv2.Subtract(mat6, mat14, mat42);
						Cv2.Threshold(mat39, mat39, thdUp, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat40, mat40, thdUp, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat41, mat41, thdUp, 255.0, ThresholdTypes.Binary);
						Cv2.Threshold(mat42, mat42, thdUp, 255.0, ThresholdTypes.Binary);
						using Mat mat43 = new Mat(mat, roi).Clone();
						using Mat mat44 = new Mat(mat, roi).Clone();
						using Mat mat45 = new Mat(mat, roi).Clone();
						Cv2.BitwiseOr(mat39, mat40, mat43);
						Cv2.BitwiseOr(mat41, mat42, mat44);
						Cv2.BitwiseOr(mat43, mat44, mat45);
						Rect roi15 = new Rect(p2PIntervalX, p2PIntervalY, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						Rect roi16 = new Rect(0, 0, mat6.Cols - p2PIntervalX, mat6.Rows - p2PIntervalY);
						mat5.SetTo(0.0);
						using (Mat mat46 = new Mat(mat45, roi16))
						{
							using Mat m4 = new Mat(mat5, roi15);
							mat46.CopyTo(m4);
						}
						Cv2.BitwiseOr(mat3, mat5, mat3);
					}
				}
				if (pfile.DifferenceParam[index].ThdDownON && pfile.DifferenceParam[index].ThdUpON)
				{
					Cv2.BitwiseOr(mat2, mat3, Binary[i]);
				}
				else if (pfile.DifferenceParam[index].ThdDownON)
				{
					Binary[i] = mat2.Clone();
				}
				else
				{
					Binary[i] = mat3.Clone();
				}
				if (tHDLimit_Min != 0 || tHDLimit_Max != 255)
				{
					Mat mat47 = mat.Clone();
					Cv2.InRange(mat, tHDLimit_Min, tHDLimit_Max, mat47);
					Cv2.BitwiseAnd(mat47, Binary[i], Binary[i]);
				}
				doMorph(ref Binary[i], (int)pfile.DifferenceParam[index].DE_Start_Index, pfile.DifferenceParam[index].DE_Start_Count);
				doMorph(ref Binary[i], (int)pfile.DifferenceParam[index].DE_End_Index, pfile.DifferenceParam[index].DE_End_Count);
				if (pfile.DifferenceParam[index].MaskSelectIndex > 0)
				{
					if (!pfile.MaskToolON)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具未開啟! \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具未开启! \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 検出エリアはマスクツールを使用していますが、マスクツールが有効になっていません！ \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool has been Disable! \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName);
						}
						iresult = TOOL_RESULT.EM_OK;
						array[i] = true;
						continue;
					}
					int num = pfile.DifferenceParam[index].MaskSelectIndex - 1;
					if (!pfile.MaskParam[num].TrackerON)
					{
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 檢測區使用遮罩工具，但遮罩工具[{2}]未開啟! \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, num + 1);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 检测区使用遮罩工具，但遮罩工具[{2}]未开启! \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, num + 1);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText = string.Format("[{0}:{1}] 検出エリアはマスクツールを使用しますが、マスクツール [{2}] が有効ではありません！ \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, num + 1);
						}
						else
						{
							Tracker1ResultText = string.Format("[{0}:{1}] Tracker Link Error, Mask Tool [{2}] has been Disable! \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, num + 1);
						}
						iresult = TOOL_RESULT.EM_OK;
						array[i] = true;
						continue;
					}
					if (pfile.MaskParam[num].Mask1 != null)
					{
						Mat mat48 = buffers.GetChannelMat(0).Clone();
						mat48.SetTo(0.0);
						bool flag = false;
						Rect[] array2 = new Rect[2]
						{
							new Rect(pfile.MaskParam[num].Mask1Pt.X, pfile.MaskParam[num].Mask1Pt.Y, pfile.MaskParam[num].Mask1.Width, pfile.MaskParam[num].Mask1.Height),
							default(Rect)
						};
						if (pfile.MaskParam[num].SearchRect2ON && num < 20 && pfile.MaskParam[num].Mask2 != null)
						{
							array2[1] = new Rect(pfile.MaskParam[num].Mask2Pt.X, pfile.MaskParam[num].Mask2Pt.Y, pfile.MaskParam[num].Mask2.Width, pfile.MaskParam[num].Mask2.Height);
						}
						else
						{
							array2[1] = new Rect(0, 0, 0, 0);
						}
						if (array2[0].Width > 0 && array2[0].Height > 0)
						{
							using Mat m5 = new Mat(mat48, array2[0]);
							using Mat mat49 = pfile.MaskParam[num].Mask1.Clone();
							mat49.CopyTo(m5);
							flag = true;
						}
						if (pfile.MaskParam[num].SearchRect2ON && array2[1].Width > 0 && array2[1].Height > 0 && num < 20)
						{
							using Mat m6 = new Mat(mat48, array2[1]);
							using Mat mat50 = pfile.MaskParam[num].Mask2.Clone();
							mat50.CopyTo(m6);
							flag = true;
						}
						if (flag)
						{
							Mat mat51 = new Mat(mat48, InspeRect[i]);
							if (pfile.DifferenceParam[index].UseMaskAsInspArea)
							{
								Cv2.BitwiseAnd(Binary[i], mat51, Binary[i]);
							}
							else
							{
								Cv2.BitwiseNot(mat51, mat51);
								Cv2.BitwiseAnd(Binary[i], mat51, Binary[i]);
							}
						}
					}
				}
				if (i == 0)
				{
					buffers.DoFilllet(ref Binary[i], new Rect(0, 0, Binary[i].Width, Binary[i].Height), pfile.DifferenceParam[index].T1FillletCount0, pfile.DifferenceParam[index].T1FillletCount1, pfile.DifferenceParam[index].T1FillletCount2, pfile.DifferenceParam[index].T1FillletCount3);
				}
				else
				{
					buffers.DoFilllet(ref Binary[i], new Rect(0, 0, Binary[i].Width, Binary[i].Height), pfile.DifferenceParam[index].T2FillletCount0, pfile.DifferenceParam[index].T2FillletCount1, pfile.DifferenceParam[index].T2FillletCount2, pfile.DifferenceParam[index].T2FillletCount3);
				}
				if (pfile.MatchToolON)
				{
					Mat mat52 = buffers.GetChannelMat(0).Clone();
					if (pfile.DifferenceParam[index].UseMatchTextMask1 > 0)
					{
						mat52.SetTo(255.0);
						int num2 = pfile.DifferenceParam[index].UseMatchTextMask1 - 1;
						if (pfile.MatchParam[num2].TextMatResult)
						{
							Rect textRect = pfile.MatchParam[num2].TextRect;
							textRect &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect.Width > 0 && textRect.Height > 0 && Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
							{
								using Mat m7 = new Mat(mat52, textRect);
								Mat mat53 = pfile.MatchParam[num2].TextMat.Clone();
								doMorph(ref mat53, (int)pfile.DifferenceParam[index].MatchTextDE_Start1_Index, pfile.DifferenceParam[index].MatchTextDE_Start1_Count);
								Cv2.BitwiseNot(mat53, mat53);
								mat53.CopyTo(m7);
								Mat mat54 = new Mat(mat52, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(Binary[i], mat54, Binary[i]);
							}
						}
					}
					if (pfile.DifferenceParam[index].UseMatchTextMask2 > 0)
					{
						mat52.SetTo(255.0);
						int num3 = pfile.DifferenceParam[index].UseMatchTextMask2 - 1;
						if (pfile.MatchParam[num3].TextMatResult)
						{
							Rect textRect2 = pfile.MatchParam[num3].TextRect;
							textRect2 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect2.Width > 0 && textRect2.Height > 0 && Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
							{
								using Mat m8 = new Mat(mat52, textRect2);
								Mat mat55 = pfile.MatchParam[num3].TextMat.Clone();
								doMorph(ref mat55, (int)pfile.DifferenceParam[index].MatchTextDE_Start2_Index, pfile.DifferenceParam[index].MatchTextDE_Start2_Count);
								Cv2.BitwiseNot(mat55, mat55);
								mat55.CopyTo(m8);
								Mat mat56 = new Mat(mat52, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(Binary[i], mat56, Binary[i]);
							}
						}
					}
					if (pfile.DifferenceParam[index].UseMatchTextMask3 > 0)
					{
						mat52.SetTo(255.0);
						int num4 = pfile.DifferenceParam[index].UseMatchTextMask3 - 1;
						if (pfile.MatchParam[num4].TextMatResult)
						{
							Rect textRect3 = pfile.MatchParam[num4].TextRect;
							textRect3 &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
							if (textRect3.Width > 0 && textRect3.Height > 0 && Binary[i].Width == InspeRect[i].Width && Binary[i].Height == InspeRect[i].Height)
							{
								using Mat m9 = new Mat(mat52, textRect3);
								Mat mat57 = pfile.MatchParam[num4].TextMat.Clone();
								doMorph(ref mat57, (int)pfile.DifferenceParam[index].MatchTextDE_Start3_Index, pfile.DifferenceParam[index].MatchTextDE_Start3_Count);
								Cv2.BitwiseNot(mat57, mat57);
								mat57.CopyTo(m9);
								Mat mat58 = new Mat(mat52, InspeRect[i]).Clone();
								Cv2.BitwiseAnd(Binary[i], mat58, Binary[i]);
							}
						}
					}
				}
				if (pfile.DifferenceParam[index].BlobAnalysisSelect == 0)
				{
					array[i] = findBlob(Binary[i], i);
				}
				else if (pfile.DifferenceParam[index].BlobAnalysisSelect == 1)
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
				else if (pfile.DifferenceParam[index].BlobAnalysisSelect == 2)
				{
					if (i == 0)
					{
						array[i] = findMultiBlob(Binary[i], index, i, ref MultiBlob1, pfile.DifferenceParam[index].MB_AllowNGCount, i);
					}
					else
					{
						array[i] = findMultiBlob(Binary[i], index, i, ref MultiBlob2, pfile.DifferenceParam[index].MB_AllowNGCount, i);
					}
				}
			}
		}
		catch (Exception ex)
		{
			string text3 = "DT06-DifferenceTool-Inspect()-";
			text3 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text3);
			}
			else
			{
				CVBuffer.SaveLog(text3, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-006");
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
			switch (pfile.DifferenceParam[index].MB_BlobSort)
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
			for (int num3 = 0; num3 < MultiBlob.Count && num3 < pfile.DifferenceParam[index].MB_BlobNum; num3++)
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
				_ = 1;
				string text = "";
				text = ((buffers.LanguageType == 1) ? (text + string.Format("[{0}:{1}][{2}] NG個數={3}\n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", num2)) : ((buffers.LanguageType == 2) ? (text + string.Format("[{0}:{1}][{2}] NG个数={3}\n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", num2)) : ((buffers.LanguageType != 3) ? (text + string.Format("[{0}:{1}][{2}] NG Count={3}\n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", num2)) : (text + string.Format("[{0}:{1}][{2}] NG個数={3}\n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, "A", num2)))));
				if (TrackerIndex == 0)
				{
					Tracker1ResultText += text;
				}
				else
				{
					Tracker2ResultText += text;
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
			array[0] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].MB_InspRule1_Index, pfile.DifferenceParam[DifferenceIndex].MB_InspRule1_Min, pfile.DifferenceParam[DifferenceIndex].MB_InspRule1_Max, rect, area, TrackIndex);
			array[1] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].MB_InspRule2_Index, pfile.DifferenceParam[DifferenceIndex].MB_InspRule2_Min, pfile.DifferenceParam[DifferenceIndex].MB_InspRule2_Max, rect, area, TrackIndex);
			array[2] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].MB_InspRule3_Index, pfile.DifferenceParam[DifferenceIndex].MB_InspRule3_Min, pfile.DifferenceParam[DifferenceIndex].MB_InspRule3_Max, rect, area, TrackIndex);
			array[3] = GetRule((int)pfile.DifferenceParam[DifferenceIndex].MB_InspRule4_Index, pfile.DifferenceParam[DifferenceIndex].MB_InspRule4_Min, pfile.DifferenceParam[DifferenceIndex].MB_InspRule4_Max, rect, area, TrackIndex);
			if (pfile.DifferenceParam[DifferenceIndex].MB_InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.DifferenceParam[DifferenceIndex].MB_InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.DifferenceParam[DifferenceIndex].MB_InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.DifferenceParam[DifferenceIndex].MB_InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.DifferenceParam[DifferenceIndex].MB_InspRule4_Index > BlobCondition.EM_NOT_USE)
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
			string text3 = "";
			text3 = ((buffers.LanguageType == 1) ? (text3 + string.Format("[{0}:{1}][{2}]-[{3}] 寬:{4:N3}um, 高:{5:N3}um, 面積:{6:N3}um, 長寬比:{7:N3}, 面積比:{8:N3} {9} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text)) : ((buffers.LanguageType == 2) ? (text3 + string.Format("[{0}:{1}][{2}]-[{3}] 宽:{4:N3}um, 高:{5:N3}um, 面积:{6:N3}um, 长宽比:{7:N3}, 面积比:{8:N3} {9} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text)) : ((buffers.LanguageType != 3) ? (text3 + string.Format("[{0}:{1}][{2}]-[{3}] Width:{4:N3}um, Height:{5:N3}um, Area:{6:N3}um, WH Ratio:{7:N3}, Area Ratio:{8:N3} {9} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text)) : (text3 + string.Format("[{0}:{1}][{2}]-[{3}] 幅:{4:N3}um、高:{5:N3}um、面積:{6:N3}um、縱横比:{7:N3}、面積比:{8:N3} {9} \n", DifferenceIndex + 1, pfile.DifferenceParam[DifferenceIndex].TrackerName, text2, BlobIndex + 1, (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, num2, num, text)))));
			if (TrackIndex == 0)
			{
				Tracker1ResultText += text3;
			}
			else
			{
				Tracker2ResultText += text3;
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text4 = "BT03-Difference-checkMultiResult()-";
			text4 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text4);
			}
			else
			{
				CVBuffer.SaveLog(text4, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "DiFFTool-007");
			}
			return false;
		}
	}
}
