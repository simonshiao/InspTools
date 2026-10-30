using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class MaskTool
{
	private const int MaxThdTool = 4;

	private const int MaxTracker = 2;

	private const int MaxMaskCombine = 8;

	private int ImgCounter = 1;

	private int StationIndex = 1;

	private bool SaveAIImage;

	private bool OffLineSaveAIImage;

	private Mat[] Binary = new Mat[4];

	private Mat MaskTemp = new Mat();

	private Mat MaskCombine = new Mat();

	private Mat MaskCombineTemp = new Mat();

	public Mat AIMat = new Mat();

	public Rect AIRect;

	private CVBuffer buffers;

	public BlobStruct[] Blobs = new BlobStruct[2];

	public List<Rect> BlobRect = new List<Rect>();

	private int BinaryIndex;

	private ParamFile pfile;

	private Rect[] InspeRect = new Rect[2];

	public Rect RoiInspectRect;

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool BinarySuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public string Tracker2ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public MaskTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect[0] = new Rect(0, 0, 0, 0);
		InspeRect[1] = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 4; i++)
		{
			Binary[i] = new Mat();
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
		if (index == 0)
		{
			if (pfile.MaskParam[index].Tracker1UseAutoSize)
			{
				InspeRect[0].X = pfile.AdvLocateParam[0].L + pfile.MaskParam[index].Track1_L;
				InspeRect[0].Y = pfile.AdvLocateParam[0].T + pfile.MaskParam[index].Track1_T;
				InspeRect[0].Width = pfile.AdvLocateParam[0].Width - (pfile.MaskParam[index].Track1_L + pfile.MaskParam[index].Track1_R);
				InspeRect[0].Height = pfile.AdvLocateParam[0].Height - (pfile.MaskParam[index].Track1_T + pfile.MaskParam[index].Track1_B);
			}
			else
			{
				int width = pfile.MaskParam[index].Track1_R - pfile.MaskParam[index].Track1_L;
				int height = pfile.MaskParam[index].Track1_B - pfile.MaskParam[index].Track1_T;
				InspeRect[0].X = pfile.VSLocateParam[0].Value + pfile.MaskParam[index].Track1_L;
				InspeRect[0].Y = pfile.HSLocateParam[0].Value + pfile.MaskParam[index].Track1_T;
				InspeRect[0].Width = width;
				InspeRect[0].Height = height;
			}
			if (pfile.MaskParam[index].Tracker2UseAutoSize)
			{
				InspeRect[1].X = pfile.AdvLocateParam[0].L + pfile.MaskParam[index].Track2_L;
				InspeRect[1].Y = pfile.AdvLocateParam[0].T + pfile.MaskParam[index].Track2_T;
				InspeRect[1].Width = pfile.AdvLocateParam[0].Width - (pfile.MaskParam[index].Track2_L + pfile.MaskParam[index].Track2_R);
				InspeRect[1].Height = pfile.AdvLocateParam[0].Height - (pfile.MaskParam[index].Track2_T + pfile.MaskParam[index].Track2_B);
			}
			else
			{
				int width2 = pfile.MaskParam[index].Track2_R - pfile.MaskParam[index].Track2_L;
				int height2 = pfile.MaskParam[index].Track2_B - pfile.MaskParam[index].Track2_T;
				InspeRect[1].X = pfile.VSLocateParam[0].Value + pfile.MaskParam[index].Track2_L;
				InspeRect[1].Y = pfile.HSLocateParam[0].Value + pfile.MaskParam[index].Track2_T;
				InspeRect[1].Width = width2;
				InspeRect[1].Height = height2;
			}
		}
		else
		{
			if (pfile.MaskParam[index].Tracker1UseAutoSize)
			{
				InspeRect[0].X = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].L + pfile.MaskParam[index].Track1_L;
				InspeRect[0].Y = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].T + pfile.MaskParam[index].Track1_T;
				InspeRect[0].Width = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].Width - (pfile.MaskParam[index].Track1_L + pfile.MaskParam[index].Track1_R);
				InspeRect[0].Height = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].Height - (pfile.MaskParam[index].Track1_T + pfile.MaskParam[index].Track1_B);
			}
			else
			{
				int width3 = pfile.MaskParam[index].Track1_R - pfile.MaskParam[index].Track1_L;
				int height3 = pfile.MaskParam[index].Track1_B - pfile.MaskParam[index].Track1_T;
				InspeRect[0].X = pfile.VSLocateParam[pfile.MaskParam[index].Tracker1AligmentXIndex].Value + pfile.MaskParam[index].Track1_L;
				InspeRect[0].Y = pfile.HSLocateParam[pfile.MaskParam[index].Tracker1AligmentYIndex].Value + pfile.MaskParam[index].Track1_T;
				InspeRect[0].Width = width3;
				InspeRect[0].Height = height3;
			}
			if (pfile.MaskParam[index].Tracker2UseAutoSize)
			{
				InspeRect[1].X = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize2_SelectIndex].L + pfile.MaskParam[index].Track2_L;
				InspeRect[1].Y = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize2_SelectIndex].T + pfile.MaskParam[index].Track2_T;
				InspeRect[1].Width = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize2_SelectIndex].Width - (pfile.MaskParam[index].Track2_L + pfile.MaskParam[index].Track2_R);
				InspeRect[1].Height = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize2_SelectIndex].Height - (pfile.MaskParam[index].Track2_T + pfile.MaskParam[index].Track2_B);
			}
			else
			{
				int width4 = pfile.MaskParam[index].Track2_R - pfile.MaskParam[index].Track2_L;
				int height4 = pfile.MaskParam[index].Track2_B - pfile.MaskParam[index].Track2_T;
				InspeRect[1].X = pfile.VSLocateParam[pfile.MaskParam[index].Tracker2AligmentXIndex].Value + pfile.MaskParam[index].Track2_L;
				InspeRect[1].Y = pfile.HSLocateParam[pfile.MaskParam[index].Tracker2AligmentYIndex].Value + pfile.MaskParam[index].Track2_T;
				InspeRect[1].Width = width4;
				InspeRect[1].Height = height4;
			}
		}
	}

	public void GetInspeRect(int index, ref Rect rect)
	{
		LocateRect = buffers.LocatedRect;
		if (pfile.MaskParam[index].Tracker1UseAutoSize)
		{
			rect.X = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].L + pfile.MaskParam[index].Track1_L;
			rect.Y = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].T + pfile.MaskParam[index].Track1_T;
			rect.Width = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].Width - (pfile.MaskParam[index].Track1_L + pfile.MaskParam[index].Track1_R);
			rect.Height = pfile.AdvLocateParam[pfile.MaskParam[index].AutoSize1_SelectIndex].Height - (pfile.MaskParam[index].Track1_T + pfile.MaskParam[index].Track1_B);
		}
		else
		{
			int width = pfile.MaskParam[index].Track1_R - pfile.MaskParam[index].Track1_L;
			int height = pfile.MaskParam[index].Track1_B - pfile.MaskParam[index].Track1_T;
			rect.X = pfile.VSLocateParam[pfile.MaskParam[index].Tracker1AligmentXIndex].Value + pfile.MaskParam[index].Track1_L;
			rect.Y = pfile.HSLocateParam[pfile.MaskParam[index].Tracker1AligmentYIndex].Value + pfile.MaskParam[index].Track1_T;
			rect.Width = width;
			rect.Height = height;
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
	public bool Inspect(int index, int FaceIndex = 0, bool UseAI = false, bool offline = true)
	{
		try
		{
			OffLine = offline;
			BinarySuccess = true;
			Tracker1ResultText = "";
			Tracker2ResultText = "";
			pfile.MaskParam[index].Mask1 = new Mat();
			pfile.MaskParam[index].Mask2 = new Mat();
			if (index == 0 && UseAI)
			{
				AIMat.SetTo(0.0);
				ref Rect aIRect = ref AIRect;
				int left = (AIRect.Top = (AIRect.Width = (AIRect.Height = 0)));
				aIRect.Left = left;
			}
			if (!pfile.MaskParam[index].TrackerON)
			{
				return true;
			}
			if (FaceIndex != 0 && pfile.MaskParam[index].EnableSelectFace && FaceIndex != pfile.MaskParam[index].FaceIndex)
			{
				return true;
			}
			BinaryIndex = index;
			if (index < 20)
			{
				GetInspeRect(index);
				bool[] array = new bool[4];
				InspeRect[0] &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
				InspeRect[1] &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
				RoiInspectRect = InspeRect[1];
				RoiInspectRect.Left = InspeRect[1].Left - InspeRect[0].Left;
				RoiInspectRect.Top = InspeRect[1].Top - InspeRect[0].Top;
				if ((UseAI || SaveAIImage || OffLineSaveAIImage) && index == 0)
				{
					AIRect = InspeRect[0];
					Mat mat = new Mat(buffers.LocatedSrc, InspeRect[0]);
					if (pfile.MaskParam[index].SearchRect2ON && pfile.MaskParam[index].SearchRect2AsMask)
					{
						InspeRect[1] &= InspeRect[0];
						new Rect(0, 0, InspeRect[0].Width, InspeRect[0].Height);
						Rect roi = new Rect(InspeRect[1].Left - InspeRect[0].Left, InspeRect[1].Top - InspeRect[0].Top, InspeRect[1].Width, InspeRect[1].Height);
						mat.CopyTo(AIMat);
						new Mat(AIMat, roi).SetTo(0.0);
					}
					else
					{
						mat.CopyTo(AIMat);
					}
					if (SaveAIImage)
					{
						string path = string.Format("D:\\AIIMage\\{0}\\", StationIndex + 1);
						if (!Directory.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
						string fileName = string.Format("D:\\AIIMage\\{0}\\{1}.png", StationIndex + 1, ImgCounter);
						ImgCounter++;
						Cv2.ImWrite(fileName, AIMat);
					}
					return true;
				}
				bool[] array2 = new bool[2];
				int[] array3 = new int[4];
				int[] array4 = new int[4];
				int[] array5 = new int[4];
				int[] array6 = new int[4];
				int[] array7 = new int[4];
				int[] array8 = new int[4];
				int[] array9 = new int[4];
				SetValue(array, pfile.MaskParam[index].THD1_ChannelON, pfile.MaskParam[index].THD2_ChannelON, pfile.MaskParam[index].THD3_ChannelON, pfile.MaskParam[index].THD4_ChannelON);
				SetValue(array3, (int)pfile.MaskParam[index].THD1_ChannelIndex, (int)pfile.MaskParam[index].THD2_ChannelIndex, (int)pfile.MaskParam[index].THD3_ChannelIndex, (int)pfile.MaskParam[index].THD4_ChannelIndex);
				SetValue(array4, pfile.MaskParam[index].THD1_Min, pfile.MaskParam[index].THD2_Min, pfile.MaskParam[index].THD3_Min, pfile.MaskParam[index].THD4_Min);
				SetValue(array5, pfile.MaskParam[index].THD1_Max, pfile.MaskParam[index].THD2_Max, pfile.MaskParam[index].THD3_Max, pfile.MaskParam[index].THD4_Max);
				SetValue(array7, pfile.MaskParam[index].DE_Start1_Count, pfile.MaskParam[index].DE_Start2_Count, pfile.MaskParam[index].DE_Start3_Count, pfile.MaskParam[index].DE_Start4_Count);
				SetValue(array6, (int)pfile.MaskParam[index].DE_Start1_Index, (int)pfile.MaskParam[index].DE_Start2_Index, (int)pfile.MaskParam[index].DE_Start3_Index, (int)pfile.MaskParam[index].DE_Start4_Index);
				SetValue(array9, pfile.MaskParam[index].DE_End1_Count, pfile.MaskParam[index].DE_End2_Count, pfile.MaskParam[index].DE_End3_Count, pfile.MaskParam[index].DE_End4_Count);
				SetValue(array8, (int)pfile.MaskParam[index].DE_End1_Index, (int)pfile.MaskParam[index].DE_End2_Index, (int)pfile.MaskParam[index].DE_End3_Index, (int)pfile.MaskParam[index].DE_End4_Index);
				for (int i = 0; i < 2; i++)
				{
					Blobs[i].Width = (Blobs[i].Height = (Blobs[i].Area = 0.0));
					Blobs[i].AreaRatio = (Blobs[i].WHRatio = 0.0);
					Blobs[i].NGRect = new Rect(0, 0, 0, 0);
					array2[i] = true;
					if (i == 1 && !pfile.MaskParam[index].SearchRect2ON)
					{
						continue;
					}
					if (InspeRect[i].Width <= 0 || InspeRect[i].Height <= 0)
					{
						if (i == 0)
						{
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][Area {2}] Tracker width and height = 0! \n", pfile.MaskParam[BinaryIndex].TrackerName, BinaryIndex, i + 1);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText = string.Format("[{0}:{1}][區域{2}] 檢測區域寬高為0! \n", pfile.MaskParam[BinaryIndex].TrackerName, BinaryIndex, i + 1);
							}
							else
							{
								Tracker1ResultText = string.Format("[{0}:{1}][区域{2}] 检测区域宽高为0! \n", pfile.MaskParam[BinaryIndex].TrackerName, BinaryIndex, i + 1);
							}
						}
						return false;
					}
					int num2 = 0;
					new Mat(buffers.GetChannelMat(array3[0]), InspeRect[i]).CopyTo(Binary[0]);
					Cv2.InRange(Binary[0], array4[0], array5[0], Binary[0]);
					doMorph(ref Binary[0], array6[0], array7[0]);
					doMorph(ref Binary[0], array8[0], array9[0]);
					num2++;
					array[0] = true;
					if (array[1])
					{
						new Mat(buffers.GetChannelMat(array3[1]), InspeRect[i]).CopyTo(Binary[1]);
						Cv2.InRange(Binary[1], array4[1], array5[1], Binary[1]);
						doMorph(ref Binary[1], array6[1], array7[1]);
						doMorph(ref Binary[1], array8[1], array9[1]);
					}
					if (array[2])
					{
						new Mat(buffers.GetChannelMat(array3[2]), InspeRect[i]).CopyTo(Binary[2]);
						Cv2.InRange(Binary[2], array4[2], array5[2], Binary[2]);
						doMorph(ref Binary[2], array6[2], array7[2]);
						doMorph(ref Binary[2], array8[2], array9[2]);
						num2++;
					}
					if (array[3])
					{
						new Mat(buffers.GetChannelMat(array3[3]), InspeRect[i]).CopyTo(Binary[3]);
						Cv2.InRange(Binary[3], array4[3], array5[3], Binary[3]);
						doMorph(ref Binary[3], array6[3], array7[3]);
						doMorph(ref Binary[3], array8[3], array9[3]);
						num2++;
					}
					Mat Mat = Binary[0];
					for (int j = 1; j < 4; j++)
					{
						if (array[j])
						{
							Cv2.BitwiseAnd(Mat, Binary[j], Mat);
						}
					}
					if (i == 0)
					{
						buffers.DoFilllet(ref Mat, new Rect(0, 0, Mat.Width, Mat.Height), pfile.MaskParam[index].T1FillletCount0, pfile.MaskParam[index].T1FillletCount1, pfile.MaskParam[index].T1FillletCount2, pfile.MaskParam[index].T1FillletCount3);
					}
					else
					{
						buffers.DoFilllet(ref Mat, new Rect(0, 0, Mat.Width, Mat.Height), pfile.MaskParam[index].T2FillletCount0, pfile.MaskParam[index].T2FillletCount1, pfile.MaskParam[index].T2FillletCount2, pfile.MaskParam[index].T2FillletCount3);
					}
					if (pfile.MaskParam[index].MaxBlob > 0)
					{
						if (!pfile.MaskParam[index].EnableAdvLocateMask || !pfile.MaskParam[index].AdvLocateAsInsp)
						{
							FindMaxBlob(ref Mat, pfile.MaskParam[index].MaxBlob, pfile.MaskParam[index].doConvexHull);
						}
					}
					else if (pfile.MaskParam[index].doConvexHull)
					{
						doFillConvexHull(ref Mat);
					}
					if (pfile.MaskParam[index].FillHole)
					{
						FillHole(ref Mat);
					}
					if (pfile.MaskParam[index].EnableAdvLocateMask && pfile.MaskParam[index].AdvLocateStartIndex > 0 && pfile.MaskParam[index].AdvLocateEndIndex > 0)
					{
						Mat channelMat = buffers.GetChannelMat(0);
						Rect rect = new Rect(0, 0, channelMat.Cols, channelMat.Rows);
						channelMat.CopyTo(MaskTemp);
						MaskTemp.SetTo(0.0);
						int num3 = pfile.MaskParam[index].AdvLocateStartIndex;
						int num4 = pfile.MaskParam[index].AdvLocateEndIndex;
						if (num3 > num4)
						{
							int num5 = num3;
							num3 = num4;
							num4 = num5;
						}
						for (int k = num3; k <= num4; k++)
						{
							Rect rect2 = pfile.AdvLocateParam[k].InsideRect;
							DoBorderExpend(ref rect2, pfile.MaskParam[index].AdvLocateBorder1, pfile.MaskParam[index].AdvLocateExpand1, pfile.MaskParam[index].AdvLocateExpandCount1);
							DoBorderExpend(ref rect2, pfile.MaskParam[index].AdvLocateBorder2, pfile.MaskParam[index].AdvLocateExpand2, pfile.MaskParam[index].AdvLocateExpandCount2);
							rect2 &= rect;
							if (rect2.Width > 0 && rect2.Height > 0)
							{
								using (Mat mat2 = new Mat(MaskTemp, rect2))
								{
									mat2.SetTo(255.0);
								}
								if (pfile.MaskParam[index].AdvLocateBorderFilllet != 0)
								{
									int advLocateBorderFilllet = pfile.MaskParam[index].AdvLocateBorderFilllet;
									buffers.DoFilllet(ref MaskTemp, rect2, advLocateBorderFilllet, advLocateBorderFilllet, advLocateBorderFilllet, advLocateBorderFilllet);
								}
							}
						}
						if (!pfile.MaskParam[index].AdvLocateAsInsp)
						{
							Cv2.BitwiseNot(MaskTemp, MaskTemp);
							using Mat mat3 = new Mat(MaskTemp, InspeRect[i]);
							Cv2.BitwiseAnd(Mat, mat3, Mat);
						}
						else
						{
							using Mat mat4 = new Mat(MaskTemp, InspeRect[i]);
							Mat.SetTo(255.0);
							Cv2.BitwiseAnd(Mat, mat4, Mat);
						}
					}
					if (i == 0)
					{
						doMorph(ref Mat, (int)pfile.MaskParam[index].DE_FinalStart_Index, pfile.MaskParam[index].DE_FinalStart_Count);
						doMorph(ref Mat, (int)pfile.MaskParam[index].DE_FinalEnd_Index, pfile.MaskParam[index].DE_FinalEnd_Count);
						pfile.MaskParam[index].Mask1Pt = InspeRect[i].TopLeft;
						if (pfile.MaskParam[index].Mask1 == null)
						{
							pfile.MaskParam[index].Mask1 = new Mat();
						}
						Mat.CopyTo(pfile.MaskParam[index].Mask1);
						if ((!UseAI && !SaveAIImage && !OffLineSaveAIImage) || index != 0)
						{
							continue;
						}
						if (pfile.AIEnhanceParam.EnableCustomRGBChannel)
						{
							_ = pfile.AIEnhanceParam;
							_ = pfile.AIEnhanceParam;
							_ = pfile.AIEnhanceParam;
						}
						new Mat(buffers.LocatedSrc, InspeRect[0]).CopyTo(AIMat);
						AIRect = InspeRect[0];
						if (SaveAIImage)
						{
							string path2 = string.Format("D:\\AIIMage\\{0}\\", StationIndex + 1);
							if (!Directory.Exists(path2))
							{
								Directory.CreateDirectory(path2);
							}
							string fileName2 = string.Format("D:\\AIIMage\\{0}\\{1}.png", StationIndex + 1, ImgCounter);
							ImgCounter++;
							Cv2.ImWrite(fileName2, AIMat);
						}
					}
					else
					{
						doMorph(ref Mat, (int)pfile.MaskParam[index].DE_FinalStart_Index, pfile.MaskParam[index].DE_FinalStart_Count);
						doMorph(ref Mat, (int)pfile.MaskParam[index].DE_FinalEnd_Index, pfile.MaskParam[index].DE_FinalEnd_Count);
						pfile.MaskParam[index].Mask2Pt = InspeRect[i].TopLeft;
						if (pfile.MaskParam[index].Mask2 == null)
						{
							pfile.MaskParam[index].Mask2 = new Mat();
						}
						Mat.CopyTo(pfile.MaskParam[index].Mask2);
					}
				}
			}
			else
			{
				buffers.GetChannelMat(0).CopyTo(MaskCombine);
				Rect[] array10 = new Rect[2];
				MaskCombine.SetTo(0.0);
				int[] array11 = new int[8]
				{
					pfile.MaskParam[index].Combine1Select,
					pfile.MaskParam[index].Combine2Select,
					pfile.MaskParam[index].Combine3Select,
					pfile.MaskParam[index].Combine4Select,
					pfile.MaskParam[index].Combine5Select,
					pfile.MaskParam[index].Combine6Select,
					pfile.MaskParam[index].Combine7Select,
					pfile.MaskParam[index].Combine8Select
				};
				int[] array12 = new int[8]
				{
					pfile.MaskParam[index].Combine1Rule,
					pfile.MaskParam[index].Combine2Rule,
					pfile.MaskParam[index].Combine3Rule,
					pfile.MaskParam[index].Combine4Rule,
					pfile.MaskParam[index].Combine5Rule,
					pfile.MaskParam[index].Combine6Rule,
					pfile.MaskParam[index].Combine7Rule,
					pfile.MaskParam[index].Combine8Rule
				};
				int[] array13 = new int[8]
				{
					pfile.MaskParam[index].Combine1DE_Start_Index,
					pfile.MaskParam[index].Combine2DE_Start_Index,
					pfile.MaskParam[index].Combine3DE_Start_Index,
					pfile.MaskParam[index].Combine4DE_Start_Index,
					pfile.MaskParam[index].Combine5DE_Start_Index,
					pfile.MaskParam[index].Combine6DE_Start_Index,
					pfile.MaskParam[index].Combine7DE_Start_Index,
					pfile.MaskParam[index].Combine8DE_Start_Index
				};
				int[] array14 = new int[8]
				{
					pfile.MaskParam[index].Combine1DE_Start_Count,
					pfile.MaskParam[index].Combine2DE_Start_Count,
					pfile.MaskParam[index].Combine3DE_Start_Count,
					pfile.MaskParam[index].Combine4DE_Start_Count,
					pfile.MaskParam[index].Combine5DE_Start_Count,
					pfile.MaskParam[index].Combine6DE_Start_Count,
					pfile.MaskParam[index].Combine7DE_Start_Count,
					pfile.MaskParam[index].Combine8DE_Start_Count
				};
				for (int l = 0; l < 8; l++)
				{
					int num6 = array11[l] - 1;
					array10[0] = new Rect(0, 0, 0, 0);
					array10[1] = new Rect(0, 0, 0, 0);
					MaskCombine.CopyTo(MaskCombineTemp);
					MaskCombineTemp.SetTo(0.0);
					if (num6 >= 0 && num6 < 20 && pfile.MaskParam[num6].TrackerON)
					{
						if (pfile.MaskParam[num6].Mask1 != null)
						{
							array10[0] = new Rect(pfile.MaskParam[num6].Mask1Pt.X, pfile.MaskParam[num6].Mask1Pt.Y, pfile.MaskParam[num6].Mask1.Cols, pfile.MaskParam[num6].Mask1.Rows);
						}
						if (pfile.MaskParam[num6].SearchRect2ON && pfile.MaskParam[num6].Mask2 != null)
						{
							array10[1] = new Rect(pfile.MaskParam[num6].Mask2Pt.X, pfile.MaskParam[num6].Mask2Pt.Y, pfile.MaskParam[num6].Mask2.Cols, pfile.MaskParam[num6].Mask2.Rows);
						}
						if (array10[0].Width > 0 && array10[0].Height > 0 && pfile.MaskParam[num6].Mask1 != null)
						{
							Mat m = new Mat(MaskCombineTemp, array10[0]);
							Mat mat5 = MM.CloneFrom(pfile.MaskParam[num6].Mask1);
							doMorph(ref mat5, array13[l], array14[l]);
							mat5.CopyTo(m);
						}
						if (array10[1].Width > 0 && array10[1].Height > 0 && pfile.MaskParam[num6].Mask2 != null)
						{
							Mat m2 = new Mat(MaskCombineTemp, array10[1]);
							Mat mat6 = MM.CloneFrom(pfile.MaskParam[num6].Mask2);
							doMorph(ref mat6, array13[l], array14[l]);
							mat6.CopyTo(m2);
						}
						if (l == 0)
						{
							Cv2.BitwiseOr(MaskCombine, MaskCombineTemp, MaskCombine);
						}
						else if (array12[l] == 0)
						{
							Cv2.BitwiseOr(MaskCombine, MaskCombineTemp, MaskCombine);
						}
						else if (array12[l] == 1)
						{
							Cv2.BitwiseAnd(MaskCombine, MaskCombineTemp, MaskCombine);
						}
						else if (array12[l] == 2)
						{
							Cv2.BitwiseXor(MaskCombine, MaskCombineTemp, MaskCombine);
						}
					}
				}
				doMorph(ref MaskCombine, (int)pfile.MaskParam[index].DE_FinalStart_Index, pfile.MaskParam[index].DE_FinalStart_Count);
				doMorph(ref MaskCombine, (int)pfile.MaskParam[index].DE_FinalEnd_Index, pfile.MaskParam[index].DE_FinalEnd_Count);
				pfile.MaskParam[index].Mask1Pt = new Point(0, 0);
				if (pfile.MaskParam[index].Mask1 == null)
				{
					pfile.MaskParam[index].Mask1 = new Mat();
				}
				MaskCombine.CopyTo(pfile.MaskParam[index].Mask1);
				pfile.MaskParam[index].Mask2 = null;
				pfile.MaskParam[index].SearchRect2ON = false;
			}
		}
		catch (Exception ex)
		{
			string text = "MT01-MaskTool-Inspect()-02-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Mask-001");
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void FindMaxBlob(ref Mat mat, int count, bool FillConvexHull)
	{
		try
		{
			BlobRect.Clear();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(mat, mat2, mat3, mat4, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat3.GetGenericIndexer<int>();
			if (num <= 1 || count <= 0)
			{
				return;
			}
			double[] array = new double[count];
			int[] array2 = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = 0.0;
				array2[i] = -1;
			}
			for (int j = 1; j < num; j++)
			{
				double num2 = genericIndexer[j, 4];
				int num3 = j;
				for (int k = 0; k < count; k++)
				{
					if (num2 > array[k] && num2 > 0.0)
					{
						double num4 = array[k];
						array[k] = num2;
						int num5 = array2[k];
						array2[k] = num3;
						num3 = num5;
						num2 = num4;
					}
				}
			}
			for (int l = 0; l < count; l++)
			{
				if (array2[l] >= 0 && array2[l] < num)
				{
					Rect item = new Rect(genericIndexer[array2[l], 0], genericIndexer[array2[l], 1], genericIndexer[array2[l], 2], genericIndexer[array2[l], 3]);
					item &= new Rect(1, 1, mat.Width - 1, mat.Height - 1);
					if (item.Width > 0 && item.Height > 0)
					{
						BlobRect.Add(item);
					}
				}
			}
			Mat mat5 = new Mat(mat.Height, mat.Width, mat.Type());
			mat5.SetTo(0.0);
			for (int m = 0; m < BlobRect.Count; m++)
			{
				using Mat mat6 = new Mat(mat5, BlobRect[m]);
				mat6.SetTo(255.0);
			}
			Cv2.BitwiseAnd(mat, mat5, mat);
			if (!FillConvexHull)
			{
				return;
			}
			for (int n = 0; n < BlobRect.Count; n++)
			{
				if (n < count && array2[n] >= 0)
				{
					Mat mat7 = new Mat(mat, new Rect(BlobRect[n].Left, BlobRect[n].Top, BlobRect[n].Width, BlobRect[n].Height));
					doFillConvexHull(ref mat7);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "MT01-MaskTool-Find MaxBlob()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Mask-002");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void FindMaxBlob2(ref Mat mat, int count, bool FillConvexHull)
	{
		try
		{
			TimeStart();
			BlobRect.Clear();
			Cv2.FindContours(mat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			if (contours.Length == 0)
			{
				return;
			}
			double[] array = new double[count];
			int[] array2 = new int[count];
			RotatedRect rotatedRect = default(RotatedRect);
			for (int i = 0; i < count; i++)
			{
				array[i] = 0.0;
				array2[i] = -1;
			}
			TimeStop();
			TimeStart();
			for (int j = 0; j < contours.Length; j++)
			{
				double num = Cv2.ContourArea(contours[j]);
				int num2 = j;
				for (int k = 0; k < count; k++)
				{
					if (num > array[k] && num > 0.0)
					{
						double num3 = array[k];
						array[k] = num;
						int num4 = array2[k];
						array2[k] = num2;
						num2 = num4;
						num = num3;
					}
				}
			}
			for (int l = 0; l < count; l++)
			{
				if (array2[l] >= 0)
				{
					Rect item = Cv2.MinAreaRect(contours[array2[l]]).BoundingRect();
					item &= new Rect(1, 1, mat.Width - 1, mat.Height - 1);
					BlobRect.Add(item);
				}
			}
			TimeStop();
			TimeStart();
			Mat mat2 = new Mat(mat.Height, mat.Width, mat.Type());
			mat2.SetTo(0.0);
			for (int m = 0; m < BlobRect.Count; m++)
			{
				using Mat mat3 = new Mat(mat2, BlobRect[m]);
				mat3.SetTo(255.0);
			}
			Cv2.BitwiseAnd(mat, mat2, mat);
			TimeStop();
			if (!FillConvexHull)
			{
				return;
			}
			for (int n = 0; n < BlobRect.Count; n++)
			{
				if (array2[n] >= 0)
				{
					Mat mat4 = new Mat(mat, new Rect(BlobRect[n].Left, BlobRect[n].Top, BlobRect[n].Width, BlobRect[n].Height));
					doFillConvexHull(ref mat4);
				}
			}
		}
		catch (Exception ex)
		{
			string text = "MT01-MaskTool-Find MaxBlob()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Mask-003");
			}
		}
	}

	private void DoBorderExpend(ref Rect rect, int BorderType, int ExpendType, int count)
	{
		Rect rect2 = rect;
		if (ExpendType != 0)
		{
			count *= -1;
		}
		switch (BorderType)
		{
		case 0:
			rect.Left -= count;
			rect.Top -= count;
			rect.Width += count * 2;
			rect.Height += count * 2;
			break;
		case 1:
			rect.Top -= count;
			rect.Height += count * 2;
			break;
		default:
			rect.Left -= count;
			rect.Width += count * 2;
			break;
		}
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			rect = rect2;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void doFillConvexHull(ref Mat mat)
	{
		try
		{
			using (Mat mat2 = new Mat(mat.Height + 2, mat.Width + 2, MatType.CV_8UC1))
			{
				mat2.SetTo(0.0);
				Mat m = new Mat(mat2, new Rect(1, 1, mat.Width, mat.Height));
				mat.CopyTo(m);
			}
			Cv2.FindContours(mat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			if (contours.Length == 0)
			{
				return;
			}
			double num = 0.0;
			int num2 = 0;
			for (int i = 0; i < contours.Length; i++)
			{
				double num3 = Cv2.ContourArea(contours[i]);
				if (num3 > num)
				{
					num = num3;
					num2 = i;
					Cv2.MinAreaRect(contours[i]);
				}
			}
			mat.SetTo(0.0);
			List<Point> list = new List<Point>();
			Cv2.ConvexHull(InputArray.Create(contours[num2]), OutputArray.Create(list));
			Cv2.FillConvexPoly(mat, list, Scalar.White);
		}
		catch (Exception ex)
		{
			string text = "MT01-MaskTool-doFillConvexHull()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Mask-004");
			}
		}
	}

	private void doMatch(Mat Src, Mat Pattern)
	{
		Mat mat = new Mat();
		new Stopwatch();
		Cv2.MatchTemplate(Src, Pattern, mat, TemplateMatchModes.CCoeff);
		Cv2.MinMaxLoc(mat, out var _, out var _, out var _, out var maxLoc);
		new Rect(maxLoc.X, maxLoc.Y, Pattern.Cols, Pattern.Rows);
		Point point = default(Point);
		point.X = maxLoc.X + Pattern.Cols / 2;
		point.Y = maxLoc.Y + Pattern.Rows / 2;
		Mat[] images = new Mat[1] { Src };
		Mat mat2 = new Mat();
		int[] channels = new int[1];
		int[] histSize = new int[1] { 256 };
		Rangef[] ranges = new Rangef[1]
		{
			new Rangef(0f, 256f)
		};
		Mat mat3 = new Mat();
		Cv2.CalcHist(images, channels, mat3, mat2, 1, histSize, ranges);
		Mat[] images2 = new Mat[1] { Pattern };
		Mat mat4 = new Mat();
		Cv2.CalcHist(images2, channels, mat3, mat4, 1, histSize, ranges);
		Cv2.CompareHist(mat2, mat4, HistCompMethods.Correl);
	}

	private void doMorph(ref Mat mat, int index, int count)
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

	public void SetStationIndex(int index)
	{
		StationIndex = index;
	}

	public void SetSaveAIImage(bool status)
	{
		SaveAIImage = status;
	}

	public void SetOffLineSaveAIImage(bool status)
	{
		OffLineSaveAIImage = status;
	}

	public bool GetOffLineSaveAIImage()
	{
		return OffLineSaveAIImage;
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

	public void ResetAIMat()
	{
		AIMat = new Mat();
		ref Rect aIRect = ref AIRect;
		int left = (AIRect.Top = (AIRect.Width = (AIRect.Height = 0)));
		aIRect.Left = left;
	}
}
