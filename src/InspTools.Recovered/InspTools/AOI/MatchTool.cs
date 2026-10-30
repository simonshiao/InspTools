using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class MatchTool
{
	private bool ModelACreatInst;

	private IntPtr m_DeltaAIProcess_A;

	private IntPtr m_DeltaAIProcess_B;

	private Size AIImageSize = new Size(28, 28);

	private bool useCUDA;

	public bool ModelAReaded;

	private const int MaxPublicPattern = 210;

	private const int MaxPublicMatchParam = 50;

	private CVBuffer buffers;

	public Mat Binary = new Mat();

	private ParamFile pfile;

	private Rect InspeRect = new Rect(0, 0, 50, 50);

	private int MatchIndex;

	public Rect FoundRect;

	public Mat SearchMat = new Mat();

	private Mat SearchMatBinary = new Mat();

	private Mat OCRMat = new Mat();

	private Mat MatchImage = new Mat();

	public Mat FoundMat = new Mat();

	private Mat FlipMat = new Mat();

	public Mat FoundMatBinary = new Mat();

	private Mat PatternMatBinary = new Mat();

	private Mat BinaryTemp = new Mat();

	private const string TessData = "tessdata";

	private Mat AIPatternMat = new Mat();

	private Mat AIPredictMat = new Mat();

	public int ColorMatchScore;

	public int MatchScore;

	public int MatchScore1;

	public int MatchScore2;

	public int MatchScore3;

	public int MatchScore4;

	public int MatchShpaeScore;

	public int SimialityScore;

	public bool AISimialityCheckResult;

	public double PredResult;

	public double DifferenceRatio;

	public double AreaRatio;

	public double WidthRatio;

	public double HeightRatio;

	public bool DifferenceRatioResult = true;

	public bool AreaRatioResult = true;

	public bool WidthRatioResult = true;

	public bool HeightRatioResult = true;

	public int TextAreaVaildCount;

	public double MinTextHeight;

	public double MinTextWidth;

	public bool FoundCloseAreaResult = true;

	public bool FoundMaxCloseAreaResult = true;

	public bool FoundCloseCountResult = true;

	public Rect CloseAreaRect1 = new Rect(0, 0, 0, 0);

	public Rect CloseAreaRect2 = new Rect(0, 0, 0, 0);

	public double CloseArea1;

	public double CloseArea2;

	public bool FoundTextArea = true;

	public Rect TextAreaRect2 = new Rect(0, 0, 0, 0);

	public Rect TextAreaRect3 = new Rect(0, 0, 0, 0);

	public bool Mat2MatchFound;

	public bool Mat3MatchFound;

	public bool Mat4MatchFound;

	public bool QuickSetupUpdateMode;

	public int[] AI_Score = new int[4];

	public List<SimpleBlobStruct> MultiBlob = new List<SimpleBlobStruct>();

	private Stopwatch sw = new Stopwatch();

	private bool DllExist;

	private bool OffLine = true;

	public bool MatchSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public bool PatternFound { get; set; }

	public string OCRResult { get; set; }

	[DllImport("DeltaAIProcessDll.dll")]
	private static extern IntPtr CreateInstance();

	[DllImport("DeltaAIProcessDll.dll")]
	private static extern void DisposeInstance(IntPtr pShapeInstance);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern void SetGPU(IntPtr pShapeInstance, int gpu_id);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern void ReadModelONNX(IntPtr pShapeInstance, string modelname, bool useCUDA);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern double PredictImageSimilarity(IntPtr pShapeInstance, IntPtr Image, IntPtr Image2);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public MatchTool(ParamFile paramfile)
	{
		pfile = paramfile;
		if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "DeltaAIProcessDll.dll"))
		{
			DllExist = true;
		}
		else
		{
			DllExist = false;
		}
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

	public void GetInspeRect(int index, INSP_MATCHTOOL_PARAM Param)
	{
		LocateRect = buffers.LocatedRect;
		if (Param.AlignToMatchText)
		{
			InspeRect.X = pfile.SelectWayParam.FoundTextRectL + Param.Track1_L;
			InspeRect.Y = pfile.SelectWayParam.FoundTextRectT + Param.Track1_T;
			InspeRect.Width = pfile.SelectWayParam.FoundTextRectL + pfile.SelectWayParam.FoundTextRectW + Param.Track1_R - InspeRect.X;
			InspeRect.Height = pfile.SelectWayParam.FoundTextRectT + pfile.SelectWayParam.FoundTextRectH + Param.Track1_B - InspeRect.Y;
		}
		else if (Param.Tracker1UseAutoSize)
		{
			InspeRect.X = pfile.AdvLocateParam[Param.AutoSize1_SelectIndex].L + Param.Track1_L;
			InspeRect.Y = pfile.AdvLocateParam[Param.AutoSize1_SelectIndex].T + Param.Track1_T;
			InspeRect.Width = pfile.AdvLocateParam[Param.AutoSize1_SelectIndex].Width - (Param.Track1_L + Param.Track1_R);
			InspeRect.Height = pfile.AdvLocateParam[Param.AutoSize1_SelectIndex].Height - (Param.Track1_T + Param.Track1_B);
		}
		else
		{
			int width = Param.Track1_R - Param.Track1_L;
			int height = Param.Track1_B - Param.Track1_T;
			InspeRect.X = pfile.VSLocateParam[Param.Tracker1AligmentXIndex].Value + Param.Track1_L;
			InspeRect.Y = pfile.HSLocateParam[Param.Tracker1AligmentYIndex].Value + Param.Track1_T;
			InspeRect.Width = width;
			InspeRect.Height = height;
		}
	}

	private void doMorph(ref Mat mat, int index, int count)
	{
		if (count != 0)
		{
			switch (index)
			{
			case 1:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, count);
				break;
			case 2:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, count);
				break;
			}
			mat.RowRange(0, 2).SetTo(0.0);
			mat.RowRange(mat.Height - 2, mat.Height).SetTo(0.0);
			mat.ColRange(0, 2).SetTo(0.0);
			mat.ColRange(mat.Width - 2, mat.Width).SetTo(0.0);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(int index, ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		OffLine = offline;
		PatternFound = true;
		MatchSuccess = true;
		DifferenceRatioResult = true;
		AreaRatioResult = true;
		WidthRatioResult = true;
		HeightRatioResult = true;
		Tracker1ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		INSP_MATCHTOOL_PARAM param;
		if (pfile.MatchParam[index].QuickSetupIndex >= 0 && pfile.MatchParam[index].QuickSetupIndex < 50 && pfile.MatchQuickSetupParam[index].QuickSetupFinish && !QuickSetupUpdateMode)
		{
			param = pfile.MatchQuickSetupParam[pfile.MatchParam[index].QuickSetupIndex];
			param.TrackerName = pfile.MatchParam[index].TrackerName;
		}
		else
		{
			param = pfile.MatchParam[index];
		}
		pfile.MatchParam[index].TextRect.Width = 0;
		pfile.MatchParam[index].TextRect.Height = 0;
		pfile.MatchParam[index].TextMat = new Mat();
		pfile.MatchParam[index].TextMatResult = true;
		Mat mat = new Mat();
		bool flag = false;
		try
		{
			if (!pfile.MatchParam[index].TrackerON)
			{
				iresult = TOOL_RESULT.EM_NOT_USE;
				return true;
			}
			if (FaceIndex != 0 && pfile.MatchParam[index].EnableSelectFace && FaceIndex != pfile.MatchParam[index].FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			if (pfile.MatchParam[index].AlignToMatchText && (pfile.SelectWayParam.FoundTextRectW <= 0 || pfile.SelectWayParam.FoundTextRectH <= 0))
			{
				iresult = TOOL_RESULT.EM_NG;
				PatternFound = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText += string.Format("[{0}] - [{1}] SelectWay Tool Text Match Fail.\n", index + 1, param.TrackerName);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}] - [{1}] 選向工具圖形比對失敗，無法搜尋參考位置，請重新設定選向工具。\n", index + 1, param.TrackerName);
				}
				else
				{
					Tracker1ResultText += string.Format("[{0}] - [{1}] 选向工具图形比对失败，无法搜寻参考位置，请重新设定选向工具。\n", index + 1, param.TrackerName);
				}
				return false;
			}
			if (param.UsePublicDB)
			{
				if (param.PatternSelect < 0 || param.PatternSelect >= 210)
				{
					iresult = TOOL_RESULT.EM_NG;
					PatternFound = false;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Public Pattern file index Error, Select Pattern file again.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形共用資料庫索引錯誤，請重新選擇比對圖形。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形共用资料库索引错误，请重新选择比对图形。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				mat = pfile.PublicPatternDB[param.PatternSelect].PatternImage;
				flag = pfile.PublicPatternDB[param.PatternSelect].PatternON;
			}
			else
			{
				if (param.PatternSelect < 0 || param.PatternSelect >= 210)
				{
					iresult = TOOL_RESULT.EM_NG;
					PatternFound = false;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Pattern file index Error, Select Pattern file again.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形資料庫索引錯誤，請重新選擇比對圖形。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形资料库索引错误，请重新选择比对图形。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				mat = pfile.patternDB[param.PatternSelect].PatternImage;
				flag = pfile.patternDB[param.PatternSelect].PatternON;
			}
			if (!param.UseOCR)
			{
				if (param.PatternSelect < 0)
				{
					PatternFound = false;
					MatchSuccess = false;
					iresult = TOOL_RESULT.EM_NG;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Match Error, Missing Pattern file.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形比對錯誤，遺失比對圖形。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形比对错误，遗失比对图形。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				if (mat.Empty() || !flag)
				{
					if (param.UsePublicDB)
					{
						pfile.PublicPatternDB[param.PatternSelect].PatternON = false;
					}
					else
					{
						pfile.patternDB[param.PatternSelect].PatternON = false;
					}
					iresult = TOOL_RESULT.EM_NG;
					PatternFound = false;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Match Error, Missing Pattern file.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形比對錯誤，遺失比對圖形。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形比对错误，遗失比对图形。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				MatchIndex = index;
			}
		}
		catch (Exception ex)
		{
			string text = "MAT01-MatchTool-Inspect()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Match-001");
			}
			return false;
		}
		try
		{
			GetInspeRect(index, pfile.MatchParam[index]);
			if (!param.UseOCR)
			{
				Mat2MatchFound = false;
				Mat3MatchFound = false;
				Mat4MatchFound = false;
				int patternSelect = param.PatternSelect;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				num = ((!param.UsePublicDB) ? pfile.patternDB[patternSelect].PatternChannel : pfile.PublicPatternDB[patternSelect].PatternChannel);
				Mat mat2 = mat;
				Mat pattern = mat;
				FoundMat = new Mat();
				Mat mat3 = new Mat();
				Mat mat4 = new Mat();
				Mat mat5 = new Mat();
				Rect rect = default(Rect);
				Rect rt = default(Rect);
				Rect rt2 = default(Rect);
				Rect rt3 = default(Rect);
				Rect rt4 = default(Rect);
				MatchScore1 = 0;
				MatchScore2 = 0;
				MatchScore3 = 0;
				MatchScore4 = 0;
				InspeRect &= new Rect(0, 0, buffers.GetChannelMat(num).Cols, buffers.GetChannelMat(num).Rows);
				if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
				{
					iresult = TOOL_RESULT.EM_NG;
					PatternFound = false;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Match Error，Search area is 0.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形比對錯誤，搜尋區域寬高小於0。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形比对错误，搜寻区域宽高小于0。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				new Mat(buffers.GetChannelMat(num), InspeRect).CopyTo(SearchMat);
				if (param.MaskInFound)
				{
					for (int i = 0; i < index; i++)
					{
						if (!pfile.MatchParam[i].TrackerON || pfile.MatchParam[i].TextRect.Width <= 0 || pfile.MatchParam[i].TextRect.Height <= 0)
						{
							continue;
						}
						Rect roi = new Rect(pfile.MatchParam[i].TextRect.Left - InspeRect.Left - 1, pfile.MatchParam[i].TextRect.Top - InspeRect.Top - 1, pfile.MatchParam[i].TextRect.Width + 1, pfile.MatchParam[i].TextRect.Height + 1);
						roi &= new Rect(0, 0, SearchMat.Width, SearchMat.Height);
						if (roi.Width <= 0 || roi.Height <= 0)
						{
							continue;
						}
						using Mat mat6 = new Mat(SearchMat, roi);
						if (!pfile.MatchParam[i].TextisWhite)
						{
							mat6.SetTo(255.0);
						}
						else
						{
							mat6.SetTo(0.0);
						}
					}
				}
				_ = new bool[2];
				if (SearchMat.Width < mat2.Width || SearchMat.Height < mat2.Height)
				{
					iresult = TOOL_RESULT.EM_NG;
					PatternFound = false;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Match Error，Search area is smaller than pattern.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形比對錯誤，搜尋區域寬高小於比對圖形。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形比对错误，搜寻区域宽高小于比对图形。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				Mat mat7 = new Mat();
				MatchScore1 = doMatchTemplete(SearchMat, mat2, out rt);
				if (param.EnableRotateInspect)
				{
					int num5 = 0;
					Rect rt5 = default(Rect);
					Cv2.Flip(mat2, FlipMat, FlipMode.XY);
					num5 = doMatchTemplete(SearchMat, FlipMat, out rt5);
					if (num5 > MatchScore1)
					{
						MatchScore1 = num5;
						rt = rt5;
					}
				}
				MatchScore = MatchScore1;
				rect = rt;
				mat7 = mat2;
				new Mat(SearchMat, rect).CopyTo(FoundMat);
				bool flag2 = false;
				if (param.PatternSelect2 >= 0 && param.PatternSelect2 < 210)
				{
					int patternSelect2 = param.PatternSelect2;
					if (param.UsePublicDB)
					{
						num2 = pfile.PublicPatternDB[patternSelect2].PatternChannel;
						mat3 = pfile.PublicPatternDB[patternSelect2].PatternImage;
						flag2 = pfile.PublicPatternDB[param.PatternSelect2].PatternON;
					}
					else
					{
						num2 = pfile.patternDB[patternSelect2].PatternChannel;
						mat3 = pfile.patternDB[patternSelect2].PatternImage;
						flag2 = pfile.patternDB[param.PatternSelect2].PatternON;
					}
					if (!param.EnableAISimilarityCheck)
					{
						if (num != num2)
						{
							new Mat(buffers.GetChannelMat(num2), InspeRect).CopyTo(SearchMat);
						}
						if (param.MaskInFound && num != num2)
						{
							for (int j = 0; j < index; j++)
							{
								if (!pfile.MatchParam[j].TrackerON || pfile.MatchParam[j].TextRect.Width <= 0 || pfile.MatchParam[j].TextRect.Height <= 0)
								{
									continue;
								}
								Rect roi2 = new Rect(pfile.MatchParam[j].TextRect.Left - InspeRect.Left - 1, pfile.MatchParam[j].TextRect.Top - InspeRect.Top - 1, pfile.MatchParam[j].TextRect.Width + 1, pfile.MatchParam[j].TextRect.Height + 1);
								roi2 &= new Rect(0, 0, SearchMat.Width, SearchMat.Height);
								if (roi2.Width <= 0 || roi2.Height <= 0)
								{
									continue;
								}
								using Mat mat8 = new Mat(SearchMat, roi2);
								if (!pfile.MatchParam[j].TextisWhite)
								{
									mat8.SetTo(255.0);
								}
								else
								{
									mat8.SetTo(0.0);
								}
							}
						}
						if (mat3.Empty() || !flag2 || SearchMat.Width < mat3.Width || SearchMat.Height < mat3.Height)
						{
							MatchScore2 = 0;
						}
						else
						{
							MatchScore2 = doMatchTemplete(SearchMat, mat3, out rt2);
							if (param.EnableRotateInspect)
							{
								int num6 = 0;
								Rect rt6 = default(Rect);
								Cv2.Flip(mat3, FlipMat, FlipMode.XY);
								num6 = doMatchTemplete(SearchMat, FlipMat, out rt6);
								if (num6 > MatchScore2)
								{
									MatchScore2 = num6;
									rt2 = rt6;
								}
							}
							if (MatchScore2 > MatchScore)
							{
								Mat2MatchFound = true;
								rect = rt2;
								mat7 = mat3;
								mat2 = mat3;
								new Mat(SearchMat, rect).CopyTo(FoundMat);
								MatchScore = MatchScore2;
							}
						}
					}
				}
				bool flag3 = false;
				if (param.PatternSelect3 >= 0 && param.PatternSelect3 < 210)
				{
					int patternSelect3 = param.PatternSelect3;
					if (param.UsePublicDB)
					{
						num3 = pfile.PublicPatternDB[patternSelect3].PatternChannel;
						mat4 = pfile.PublicPatternDB[patternSelect3].PatternImage;
						flag3 = pfile.PublicPatternDB[param.PatternSelect3].PatternON;
					}
					else
					{
						num3 = pfile.patternDB[patternSelect3].PatternChannel;
						mat4 = pfile.patternDB[patternSelect3].PatternImage;
						flag3 = pfile.patternDB[param.PatternSelect3].PatternON;
					}
					if (!param.EnableAISimilarityCheck)
					{
						if (num3 != num2)
						{
							new Mat(buffers.GetChannelMat(num3), InspeRect).CopyTo(SearchMat);
						}
						if (param.MaskInFound && num3 != num2)
						{
							for (int k = 0; k < index; k++)
							{
								if (!pfile.MatchParam[k].TrackerON || pfile.MatchParam[k].TextRect.Width <= 0 || pfile.MatchParam[k].TextRect.Height <= 0)
								{
									continue;
								}
								Rect roi3 = new Rect(pfile.MatchParam[k].TextRect.Left - InspeRect.Left - 1, pfile.MatchParam[k].TextRect.Top - InspeRect.Top - 1, pfile.MatchParam[k].TextRect.Width + 1, pfile.MatchParam[k].TextRect.Height + 1);
								roi3 &= new Rect(0, 0, SearchMat.Width, SearchMat.Height);
								if (roi3.Width <= 0 || roi3.Height <= 0)
								{
									continue;
								}
								using Mat mat9 = new Mat(SearchMat, roi3);
								if (!pfile.MatchParam[k].TextisWhite)
								{
									mat9.SetTo(255.0);
								}
								else
								{
									mat9.SetTo(0.0);
								}
							}
						}
						if (mat4.Empty() || !flag3 || SearchMat.Width < mat4.Width || SearchMat.Height < mat4.Height)
						{
							MatchScore3 = 0;
						}
						else
						{
							MatchScore3 = doMatchTemplete(SearchMat, mat4, out rt3);
							if (param.EnableRotateInspect)
							{
								int num7 = 0;
								Rect rt7 = default(Rect);
								Cv2.Flip(mat4, FlipMat, FlipMode.XY);
								num7 = doMatchTemplete(SearchMat, FlipMat, out rt7);
								if (num7 > MatchScore3)
								{
									MatchScore3 = num7;
									rt3 = rt7;
								}
							}
							if (MatchScore3 > MatchScore)
							{
								Mat3MatchFound = true;
								rect = rt3;
								mat7 = mat4;
								mat2 = mat4;
								new Mat(SearchMat, rect).CopyTo(FoundMat);
								MatchScore = MatchScore3;
							}
						}
					}
				}
				bool flag4 = false;
				if (param.PatternSelect4 >= 0 && param.PatternSelect4 < 210)
				{
					int patternSelect4 = param.PatternSelect4;
					if (param.UsePublicDB)
					{
						num4 = pfile.PublicPatternDB[patternSelect4].PatternChannel;
						mat5 = pfile.PublicPatternDB[patternSelect4].PatternImage;
						flag4 = pfile.PublicPatternDB[param.PatternSelect4].PatternON;
					}
					else
					{
						num4 = pfile.patternDB[patternSelect4].PatternChannel;
						mat5 = pfile.patternDB[patternSelect4].PatternImage;
						flag4 = pfile.patternDB[param.PatternSelect4].PatternON;
					}
					if (!param.EnableAISimilarityCheck)
					{
						if (num3 != num4)
						{
							new Mat(buffers.GetChannelMat(num4), InspeRect).CopyTo(SearchMat);
						}
						if (param.MaskInFound && num3 != num4)
						{
							for (int l = 0; l < index; l++)
							{
								if (!pfile.MatchParam[l].TrackerON || pfile.MatchParam[l].TextRect.Width <= 0 || pfile.MatchParam[l].TextRect.Height <= 0)
								{
									continue;
								}
								Rect roi4 = new Rect(pfile.MatchParam[l].TextRect.Left - InspeRect.Left - 1, pfile.MatchParam[l].TextRect.Top - InspeRect.Top - 1, pfile.MatchParam[l].TextRect.Width + 1, pfile.MatchParam[l].TextRect.Height + 1);
								roi4 &= new Rect(0, 0, SearchMat.Width, SearchMat.Height);
								if (roi4.Width <= 0 || roi4.Height <= 0)
								{
									continue;
								}
								using Mat mat10 = new Mat(SearchMat, roi4);
								if (!pfile.MatchParam[l].TextisWhite)
								{
									mat10.SetTo(255.0);
								}
								else
								{
									mat10.SetTo(0.0);
								}
							}
						}
						if (mat5.Empty() || !flag4 || SearchMat.Width < mat5.Width || SearchMat.Height < mat5.Height)
						{
							MatchScore4 = 0;
						}
						else
						{
							MatchScore4 = doMatchTemplete(SearchMat, mat5, out rt4);
							if (param.EnableRotateInspect)
							{
								int num8 = 0;
								Rect rt8 = default(Rect);
								Cv2.Flip(mat5, FlipMat, FlipMode.XY);
								num8 = doMatchTemplete(SearchMat, FlipMat, out rt8);
								if (num8 > MatchScore4)
								{
									MatchScore4 = num8;
									rt4 = rt8;
								}
							}
							if (MatchScore4 > MatchScore)
							{
								Mat4MatchFound = true;
								rect = rt4;
								mat7 = mat5;
								mat2 = mat5;
								new Mat(SearchMat, rect).CopyTo(FoundMat);
								MatchScore = MatchScore4;
							}
						}
					}
				}
				FoundRect = rect;
				FoundRect.Left = rect.Left + InspeRect.Left;
				FoundRect.Top = rect.Top + InspeRect.Top;
				if (MatchScore > 100)
				{
					MatchScore = 100;
				}
				if (MatchScore < 0)
				{
					MatchScore = 0;
				}
				pfile.MatchParam[index].TextRect = FoundRect;
				ColorMatchScore = MatchScore;
				DifferenceRatio = 0.0;
				AreaRatio = 0.0;
				WidthRatio = 0.0;
				HeightRatio = 0.0;
				CloseAreaRect1 = new Rect(0, 0, 0, 0);
				CloseAreaRect2 = new Rect(0, 0, 0, 0);
				MinTextHeight = 0.0;
				MinTextWidth = 0.0;
				MatchShpaeScore = 0;
				SimialityScore = 0;
				AISimialityCheckResult = true;
				if (FoundMat.Width <= 0 || FoundMat.Height <= 0)
				{
					iresult = TOOL_RESULT.EM_NG;
					PatternFound = false;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] Match Error，Search Image is found 0.\n", index + 1, param.TrackerName);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 圖形比對錯誤，找不到相似的影像。\n", index + 1, param.TrackerName);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}] 图形比对错误，搜寻到图形小于0。\n", index + 1, param.TrackerName);
					}
					return false;
				}
				if (mat2.Width != FoundMat.Width || mat2.Height != FoundMat.Height)
				{
					mat2.Resize(new Size(FoundMat.Width, FoundMat.Height));
				}
				if (param.EnableAISimilarityCheck)
				{
					for (int m = 0; m < 4; m++)
					{
						AI_Score[m] = 0;
					}
					CheckSimilarity(index, pattern, FoundMat, ref AI_Score[0]);
					if (flag2 && mat3.Width > 0)
					{
						CheckSimilarity(index, mat3, FoundMat, ref AI_Score[1]);
					}
					if (flag3 && mat4.Width > 0)
					{
						CheckSimilarity(index, mat4, FoundMat, ref AI_Score[2]);
					}
					if (flag4 && mat5.Width > 0)
					{
						CheckSimilarity(index, mat5, FoundMat, ref AI_Score[3]);
					}
					SimialityScore = 0;
					int num9 = 0;
					for (int n = 0; n < 4; n++)
					{
						if (AI_Score[n] > SimialityScore)
						{
							SimialityScore = AI_Score[n];
							num9 = n;
						}
					}
					switch (num9)
					{
					case 1:
						Mat2MatchFound = true;
						break;
					case 2:
						Mat3MatchFound = true;
						break;
					case 3:
						Mat4MatchFound = true;
						break;
					}
					if (SimialityScore < param.AISimilarityScore)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						AISimialityCheckResult = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]AI Similarity Score({2}) less than setup({3})。\n", index + 1, param.TrackerName, SimialityScore, param.AISimilarityScore);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]AI字元檢測分數({2})低於設定分數{3}。\n", index + 1, param.TrackerName, SimialityScore, param.AISimilarityScore);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]AI字元检测分数({2})低于设定分数{3}。\n", index + 1, param.TrackerName, SimialityScore, param.AISimilarityScore);
						}
						return false;
					}
				}
				if (param.EnableMatchShape && MatchScore > 0 && FoundRect.Width > 0 && FoundRect.Height > 0)
				{
					Rect rt9 = default(Rect);
					if (!param.TextisWhite)
					{
						double thresh = Cv2.Threshold(mat2, PatternMatBinary, 0.0, 255.0, ThresholdTypes.Otsu);
						Cv2.BitwiseNot(PatternMatBinary, PatternMatBinary);
						if (!param.EnableManualThd)
						{
							thresh = Cv2.Threshold(FoundMat, SearchMatBinary, thresh, 255.0, ThresholdTypes.Binary);
							Cv2.BitwiseNot(SearchMatBinary, SearchMatBinary);
						}
						else
						{
							Cv2.InRange(FoundMat, param.TxtBinaryThdMin, param.TxtBinaryThdMax, SearchMatBinary);
							Cv2.BitwiseNot(SearchMatBinary, SearchMatBinary);
						}
					}
					else
					{
						double thresh = Cv2.Threshold(mat2, PatternMatBinary, 0.0, 255.0, ThresholdTypes.Otsu);
						if (!param.EnableManualThd)
						{
							thresh = Cv2.Threshold(FoundMat, SearchMatBinary, thresh, 255.0, ThresholdTypes.Binary);
						}
						else
						{
							Cv2.InRange(FoundMat, param.TxtBinaryThdMin, param.TxtBinaryThdMax, SearchMatBinary);
						}
					}
					MatchShpaeScore = doMatchShape(param, index, SearchMatBinary, PatternMatBinary, out rt9);
				}
				if (param.UseTextMatch)
				{
					CompareShape(param, index, mat7, FoundMat);
					if (DifferenceRatio < (double)param.TextPosDiff)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						DifferenceRatioResult = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Text Position difference error。\n", index + 1, param.TrackerName);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字位置偏移分數({2})低於設定分數{3}。\n", index + 1, param.TrackerName, (int)DifferenceRatio, param.TextPosDiff);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字位置偏移分数({2})低于设定分数{3}。\n", index + 1, param.TrackerName, (int)DifferenceRatio, param.TextPosDiff);
						}
					}
					if (AreaRatio < (double)param.TextAreaDiff)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						AreaRatioResult = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Text Area difference error。\n", index + 1, param.TrackerName);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字面積差異分數({2})低於設定分數{3}。\n", index + 1, param.TrackerName, (int)AreaRatio, param.TextAreaDiff);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字面积差异分数({2})低于设定分数{3}。\n", index + 1, param.TrackerName, (int)AreaRatio, param.TextAreaDiff);
						}
					}
					if (WidthRatio < (double)param.TextWidthDiff)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						WidthRatioResult = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Text Width difference error。\n", index + 1, param.TrackerName);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字寬度差異分數({2})低於設定分數{3}。\n", index + 1, param.TrackerName, (int)WidthRatio, param.TextWidthDiff);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字宽度差异分数({2})低于设定分数{3}。\n", index + 1, param.TrackerName, (int)WidthRatio, param.TextWidthDiff);
						}
					}
					if (HeightRatio < (double)param.TextHeightDiff)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						HeightRatioResult = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Text Height difference error。\n", index + 1, param.TrackerName);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字高度差異分數({2})低於設定分數{3}。\n", index + 1, param.TrackerName, (int)HeightRatio, param.TextHeightDiff);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字高度差异分数({2})低于设定分数{3}。\n", index + 1, param.TrackerName, (int)HeightRatio, param.TextHeightDiff);
						}
					}
					if (!pfile.MatchParam[index].TextMatResult)
					{
						return false;
					}
				}
				else
				{
					if (pfile.MatchParam[index].TextMat == null)
					{
						pfile.MatchParam[index].TextMat = new Mat();
					}
					mat7.CopyTo(pfile.MatchParam[index].TextMat);
					pfile.MatchParam[index].TextMat.SetTo(255.0);
					pfile.MatchParam[index].TextMatResult = true;
				}
				FoundTextArea = true;
				if (param.UseTextMatch)
				{
					if (MinTextHeight < param.MinTextHeight || MinTextHeight > param.MaxTextHeight)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Text Height error，Match Text Height ={2}。\n", index + 1, param.TrackerName, MinTextHeight);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字高度超過設定上下限，文字高度={2}。\n", index + 1, param.TrackerName, MinTextHeight);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字高度超过设定上下限，文字高度={2}。\n", index + 1, param.TrackerName, MinTextHeight);
						}
						return false;
					}
					if (MinTextWidth < param.MinTextWidth || MinTextWidth > param.MaxTextWidth)
					{
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Text Width error，Match Text Width ={2}。\n", index + 1, param.TrackerName, MinTextWidth);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字寬度超過設定上下限，文字寬度={2}。\n", index + 1, param.TrackerName, MinTextWidth);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]文字宽度超过设定上下限，文字宽度={2}。\n", index + 1, param.TrackerName, MinTextWidth);
						}
						return false;
					}
				}
				if (param.FindTextArea > 0 && param.UseTextMatch)
				{
					TextAreaVaildCount = 0;
					for (int num10 = 0; num10 < MultiBlob.Count; num10++)
					{
						if (MultiBlob[num10].result)
						{
							TextAreaVaildCount++;
						}
					}
					if (param.FindTextArea != TextAreaVaildCount)
					{
						FoundTextArea = false;
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Text Count ({2}) doesn't match setting({3})。\n", index + 1, param.TrackerName, TextAreaVaildCount, param.FindTextArea);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，文字區域個數({2})超過設定值({3})。\n", index + 1, param.TrackerName, TextAreaVaildCount, param.FindTextArea);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，文字区域个数({2})超过设定值({3})。\n", index + 1, param.TrackerName, TextAreaVaildCount, param.FindTextArea);
						}
						return false;
					}
				}
				if (param.FindCloseArea > 0 && param.UseTextMatch)
				{
					FoundCloseAreaResult = true;
					FoundMaxCloseAreaResult = true;
					FoundCloseCountResult = true;
					if (param.FindCloseArea == 1)
					{
						if (CloseArea1 < param.ClosedArea)
						{
							FoundCloseAreaResult = false;
							pfile.MatchParam[index].TextMatResult = false;
							iresult = TOOL_RESULT.EM_NG;
							MatchSuccess = false;
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Closed Area1 ={2:N3}。\n", index + 1, param.TrackerName, CloseArea1);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，封閉區域1面積={2:N3}。\n", index + 1, param.TrackerName, CloseArea1);
							}
							else
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，封闭区域1面积={2:N3}。\n", index + 1, param.TrackerName, CloseArea1);
							}
							return false;
						}
						if (CloseArea1 > param.MaxClosedArea)
						{
							FoundMaxCloseAreaResult = false;
							pfile.MatchParam[index].TextMatResult = false;
							iresult = TOOL_RESULT.EM_NG;
							MatchSuccess = false;
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，MaxClosed Area1 ={2:N3}。\n", index + 1, param.TrackerName, CloseArea1);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，封閉區域1面積={2:N3}。\n", index + 1, param.TrackerName, CloseArea1);
							}
							else
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，封闭区域1面积={2:N3}。\n", index + 1, param.TrackerName, CloseArea1);
							}
							return false;
						}
						if (CloseArea2 > param.ClosedArea)
						{
							FoundCloseCountResult = false;
							pfile.MatchParam[index].TextMatResult = false;
							iresult = TOOL_RESULT.EM_NG;
							MatchSuccess = false;
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Finded Closed Area num = 2。\n", index + 1, param.TrackerName);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，找到兩個封閉區域。\n", index + 1, param.TrackerName);
							}
							else
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，找到两个封闭区域。\n", index + 1, param.TrackerName);
							}
							return false;
						}
					}
					else if (param.FindCloseArea == 2)
					{
						if (CloseArea1 < param.ClosedArea || CloseArea2 < param.ClosedArea)
						{
							FoundCloseAreaResult = false;
							pfile.MatchParam[index].TextMatResult = false;
							iresult = TOOL_RESULT.EM_NG;
							MatchSuccess = false;
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Closed Area1={2:N3}。Closed Area2={3:N3}。\n", index + 1, param.TrackerName, CloseArea1, CloseArea2);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，封閉區域1面積={2:N3}。封閉區域2面積={3:N3}。\n", index + 1, param.TrackerName, CloseArea1, CloseArea2);
							}
							else
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，封闭区域1面积={2:N3}。封闭区域2面积={3:N3}。\n", index + 1, param.TrackerName, CloseArea1, CloseArea2);
							}
							return false;
						}
						if (CloseArea1 > param.MaxClosedArea || CloseArea2 > param.MaxClosedArea)
						{
							FoundMaxCloseAreaResult = false;
							pfile.MatchParam[index].TextMatResult = false;
							iresult = TOOL_RESULT.EM_NG;
							MatchSuccess = false;
							if (buffers.LanguageType == 0)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Closed Area1={2:N3}。Closed Area2={3:N3}。\n", index + 1, param.TrackerName, CloseArea1, CloseArea2);
							}
							else if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，封閉區域1面積={2:N3}。封閉區域2面積={3:N3}。\n", index + 1, param.TrackerName, CloseArea1, CloseArea2);
							}
							else
							{
								Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，封闭区域1面积={2:N3}。封闭区域2面积={3:N3}。\n", index + 1, param.TrackerName, CloseArea1, CloseArea2);
							}
							return false;
						}
					}
					else if (param.FindCloseArea == 3 && CloseArea1 > param.ClosedArea)
					{
						FoundCloseAreaResult = false;
						pfile.MatchParam[index].TextMatResult = false;
						iresult = TOOL_RESULT.EM_NG;
						MatchSuccess = false;
						if (buffers.LanguageType == 0)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Finded Closed Area num = 2。\n", index + 1, param.TrackerName);
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，找到一個以上的封閉區域。\n", index + 1, param.TrackerName);
						}
						else
						{
							Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，找到一个以上的封闭区域。\n", index + 1, param.TrackerName);
						}
						return false;
					}
				}
				if (MatchShpaeScore < param.ShapeMatchScoreMin && param.EnableMatchShape)
				{
					pfile.MatchParam[index].TextMatResult = false;
					iresult = TOOL_RESULT.EM_NG;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}]Match error, Shape Match score ={2}。\n", index + 1, param.TrackerName, MatchShpaeScore);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，邊緣比對分數={2}。\n", index + 1, param.TrackerName, MatchShpaeScore);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，边缘比对分数={2}。\n", index + 1, param.TrackerName, MatchShpaeScore);
					}
					return false;
				}
				if (MatchScore < param.MatchScoreMin || MatchScore > param.MatchScoreMax)
				{
					pfile.MatchParam[index].TextMatResult = false;
					iresult = TOOL_RESULT.EM_NG;
					MatchSuccess = false;
					if (buffers.LanguageType == 0)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}]Match error，Match score ={2}。\n", index + 1, param.TrackerName, MatchScore);
					}
					else if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}]圖形比對失敗，圖形比對分數={2}。\n", index + 1, param.TrackerName, MatchScore);
					}
					else
					{
						Tracker1ResultText += string.Format("[{0}] - [{1}]图形比对失败，图形比对分数={2}。\n", index + 1, param.TrackerName, MatchScore);
					}
					return false;
				}
				pfile.MatchParam[index].TextMatResult = true;
				return true;
			}
			OCRResult = "";
			int oCRImageChannel = param.OCRImageChannel;
			InspeRect &= new Rect(0, 0, buffers.GetChannelMat(oCRImageChannel).Cols, buffers.GetChannelMat(oCRImageChannel).Rows);
			new Mat(buffers.GetChannelMat(oCRImageChannel), InspeRect).CopyTo(OCRMat);
			OCRResult = OCRImage(OCRMat, param.TextisWhite);
			OCRResult = OCRResult.Trim();
			bool flag5 = true;
			int num11 = 0;
			for (int num12 = 0; num12 < param.OCRText.Length; num12++)
			{
				if (num12 >= OCRResult.Length)
				{
					flag5 = false;
					break;
				}
				if (param.OCRText[num12] != OCRResult[num12])
				{
					num11++;
					flag5 = false;
					break;
				}
			}
			if (!flag5)
			{
				pfile.MatchParam[index].TextMatResult = false;
				pfile.MatchParam[index].TextMat = new Mat();
				iresult = TOOL_RESULT.EM_NG;
				MatchSuccess = false;
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText += string.Format("[{0}] - [{1}] OCR error，OCR Text ={2}。\n", index + 1, param.TrackerName, OCRResult);
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[{0}] - [{1}] 文字辨識失敗，辨識文字={2}。\n", index + 1, param.TrackerName, OCRResult);
				}
				else
				{
					Tracker1ResultText += string.Format("[{0}] - [{1}] 文字辨识失败，辨识文字={2}。\n", index + 1, param.TrackerName, OCRResult);
				}
				return false;
			}
			pfile.MatchParam[index].TextMatResult = true;
			iresult = TOOL_RESULT.EM_OK;
			MatchSuccess = true;
			if (buffers.LanguageType == 0)
			{
				Tracker1ResultText += string.Format("[{0}] - [{1}] OCR Text ={2}。\n", index + 1, param.TrackerName, OCRResult);
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker1ResultText += string.Format("[{0}] - [{1}] 辨識文字={2}。\n", index + 1, param.TrackerName, OCRResult);
			}
			else
			{
				Tracker1ResultText += string.Format("[{0}] - [{1}] 辨识文字={2}。\n", index + 1, param.TrackerName, OCRResult);
			}
			return true;
		}
		catch (Exception ex2)
		{
			string text2 = "MAT02-MatchTool-Inspect()-";
			text2 += ex2.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Match-002");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private double CompareShape(INSP_MATCHTOOL_PARAM Param, int index, Mat PatternMat, Mat FoundMat, bool TextIsWhite = true)
	{
		double result = 0.0;
		try
		{
			FoundMat.CopyTo(FoundMatBinary);
			PatternMat.CopyTo(PatternMatBinary);
			if (!Param.TextisWhite)
			{
				Cv2.Threshold(PatternMat, PatternMatBinary, 0.0, 255.0, ThresholdTypes.Otsu);
				if (PatternMatBinary.Cols < 0 || PatternMatBinary.Rows < 0)
				{
					return 0.0;
				}
				Cv2.BitwiseNot(PatternMatBinary, PatternMatBinary);
				if (!Param.EnableManualThd)
				{
					Cv2.Threshold(FoundMat, FoundMatBinary, 0.0, 255.0, ThresholdTypes.Otsu);
					if (FoundMatBinary.Cols < 0 || FoundMatBinary.Rows < 0)
					{
						return 0.0;
					}
					Cv2.BitwiseNot(FoundMatBinary, FoundMatBinary);
				}
				else
				{
					Cv2.InRange(FoundMat, Param.TxtBinaryThdMin, Param.TxtBinaryThdMax, FoundMatBinary);
					if (FoundMatBinary.Cols < 0 || FoundMatBinary.Rows < 0)
					{
						return 0.0;
					}
					Cv2.BitwiseNot(FoundMatBinary, FoundMatBinary);
				}
				if (Param.UseFoundToMask)
				{
					pfile.MatchParam[index].TextMat = FoundMatBinary.Clone();
				}
				else
				{
					pfile.MatchParam[index].TextMat = PatternMatBinary.Clone();
				}
			}
			else
			{
				Cv2.Threshold(PatternMat, PatternMatBinary, 0.0, 255.0, ThresholdTypes.Otsu);
				if (!Param.EnableManualThd)
				{
					Cv2.Threshold(FoundMat, FoundMatBinary, 0.0, 255.0, ThresholdTypes.Otsu);
				}
				else
				{
					Cv2.InRange(FoundMat, Param.TxtBinaryThdMin, Param.TxtBinaryThdMax, FoundMatBinary);
				}
				if (Param.UseFoundToMask)
				{
					pfile.MatchParam[index].TextMat = FoundMatBinary.Clone();
				}
				else
				{
					pfile.MatchParam[index].TextMat = PatternMatBinary.Clone();
				}
			}
			if (Param.DEStart > 0 && Param.DEStartCount > 0)
			{
				doMorph(ref FoundMatBinary, Param.DEStart, Param.DEStartCount);
			}
			DifferenceRatio = 0.0;
			AreaRatio = 0.0;
			WidthRatio = 0.0;
			HeightRatio = 0.0;
			Rect MaxRect = default(Rect);
			Rect MaxRect2 = default(Rect);
			int val = FindMaxBlob(PatternMatBinary, FindCloseAreaMode: false, ref MaxRect);
			int val2 = FindMaxBlob(FoundMatBinary, FindCloseAreaMode: false, ref MaxRect2);
			Mat mat = new Mat();
			Cv2.BitwiseNot(FoundMatBinary, mat);
			if (Param.TextisWhite)
			{
				mat.Rectangle(new Rect(0, 0, FoundMatBinary.Width, FoundMatBinary.Height), Scalar.White);
			}
			else
			{
				mat.Rectangle(new Rect(0, 0, FoundMatBinary.Width, FoundMatBinary.Height), Scalar.Black);
			}
			CloseArea1 = 0.0;
			CloseArea2 = 0.0;
			FindCloseBlob(mat, ref CloseAreaRect1, ref CloseAreaRect2, ref CloseArea1, ref CloseArea2);
			double num = pfile.PixelPerUM_X * pfile.PixelPerUM_Y;
			CloseArea1 *= num;
			CloseArea2 *= num;
			MultiBlob.Clear();
			if (Param.FindTextArea > 0)
			{
				findMultiBlob(FoundMatBinary, ref MultiBlob, FoundRect, Param.TextArea, Param.MaxTextArea);
			}
			MinTextWidth = (double)MaxRect2.Width * pfile.PixelPerUM_X;
			MinTextHeight = (double)MaxRect2.Height * pfile.PixelPerUM_Y;
			CloseAreaRect1.Left += FoundRect.Left;
			CloseAreaRect1.Top += FoundRect.Top;
			CloseAreaRect2.Left += FoundRect.Left;
			CloseAreaRect2.Top += FoundRect.Top;
			int num2 = Math.Max(val2, val);
			int num3 = Math.Min(val2, val);
			if (num2 > 0)
			{
				AreaRatio = 100.0 - 100.0 * ((double)Math.Abs(num2 - num3) / (double)num2);
			}
			else
			{
				AreaRatio = 100.0;
			}
			int num4 = 0;
			int num5 = 0;
			if (FoundMatBinary.Rows == PatternMatBinary.Rows && FoundMatBinary.Cols == PatternMatBinary.Cols)
			{
				Cv2.BitwiseXor(PatternMatBinary, FoundMatBinary, BinaryTemp);
				num4 = PatternMatBinary.CountNonZero();
				num5 = BinaryTemp.CountNonZero();
			}
			else
			{
				num4 = PatternMatBinary.CountNonZero();
				num5 = 0;
			}
			if (num4 > 0 && num5 > 0)
			{
				DifferenceRatio = 100.0 - 100.0 * ((double)num5 / (double)num4);
				if (DifferenceRatio < 0.0)
				{
					DifferenceRatio = 0.0;
				}
			}
			else
			{
				DifferenceRatio = 100.0;
			}
			num2 = Math.Max(MaxRect2.Width, MaxRect.Width);
			num3 = Math.Min(MaxRect2.Width, MaxRect.Width);
			if (num2 > 0)
			{
				WidthRatio = 100.0 - 100.0 * ((double)Math.Abs(num2 - num3) / (double)num2);
			}
			else
			{
				WidthRatio = 100.0;
			}
			num2 = Math.Max(MaxRect2.Height, MaxRect.Height);
			num3 = Math.Min(MaxRect2.Height, MaxRect.Height);
			if (num2 > 0)
			{
				HeightRatio = 100.0 - 100.0 * ((double)Math.Abs(num2 - num3) / (double)num2);
			}
			else
			{
				HeightRatio = 100.0;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "MT02-MatchTool-FindCloseBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Match-003");
			}
			return 0.0;
		}
	}

	private int findMultiBlob(Mat binary, ref List<SimpleBlobStruct> multiBlob, Rect FoundRect, double AllowMinArea, double AllowMaxArea)
	{
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
				simpleBlobStruct.Width = (int)((double)genericIndexer[i, 2] * pfile.PixelPerUM_X);
				simpleBlobStruct.Height = (int)((double)genericIndexer[i, 3] * pfile.PixelPerUM_Y);
				simpleBlobStruct.NGRect.Left = genericIndexer[i, 0] + FoundRect.Left;
				simpleBlobStruct.NGRect.Top = genericIndexer[i, 1] + FoundRect.Top;
				simpleBlobStruct.NGRect.Width = genericIndexer[i, 2];
				simpleBlobStruct.NGRect.Height = genericIndexer[i, 3];
				simpleBlobStruct.Area = (int)((double)genericIndexer[i, 4] * pfile.PixelPerUM_X * pfile.PixelPerUM_Y);
				if (simpleBlobStruct.Area >= AllowMinArea && simpleBlobStruct.Area < AllowMaxArea)
				{
					simpleBlobStruct.result = true;
					simpleBlobStruct.AreaMinNG = (simpleBlobStruct.AreaMaxNG = false);
				}
				else
				{
					if (simpleBlobStruct.Area < AllowMinArea)
					{
						simpleBlobStruct.AreaMinNG = true;
					}
					if (simpleBlobStruct.Area >= AllowMaxArea)
					{
						simpleBlobStruct.AreaMaxNG = true;
					}
					simpleBlobStruct.result = false;
				}
				multiBlob.Add(simpleBlobStruct);
			}
			multiBlob.Sort((SimpleBlobStruct x, SimpleBlobStruct y) => -x.Area.CompareTo(y.Area));
			return multiBlob.Count;
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int FindMaxBlob(Mat binary, bool FindCloseAreaMode, ref Rect MaxRect)
	{
		try
		{
			Cv2.FindContours(binary, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
			double num = 0.0;
			int num2 = 0;
			MaxRect = new Rect(0, 0, 0, 0);
			if (contours.Length == 0)
			{
				return 0;
			}
			for (int i = 0; i < contours.Length; i++)
			{
				double num3 = Cv2.ContourArea(contours[i]);
				if (num3 > num)
				{
					num = num3;
					num2 = i;
				}
			}
			_ = contours[num2].Length;
			MaxRect = Cv2.BoundingRect(contours[num2]);
			return (int)num;
		}
		catch (Exception ex)
		{
			string text = "MT02-MatchTool-FindMaxBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Match-004");
			}
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void FindCloseBlob(Mat binary, ref Rect CloseRect1, ref Rect CloseRect2, ref double CArea1, ref double CArea2)
	{
		try
		{
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			double num2 = 0.0;
			int num3 = 0;
			CloseRect1 = new Rect(0, 0, 0, 0);
			CloseRect2 = new Rect(0, 0, 0, 0);
			CArea1 = 0.0;
			CArea2 = 0.0;
			if (num <= 1)
			{
				return;
			}
			for (int i = 1; i < num; i++)
			{
				if ((double)genericIndexer[i, 4] > num2)
				{
					num2 = genericIndexer[i, 4];
					num3 = i;
				}
			}
			double num4 = 0.0;
			int num5 = -1;
			for (int j = 1; j < num; j++)
			{
				if (j != num3 && (double)genericIndexer[j, 4] > num4)
				{
					num4 = genericIndexer[j, 4];
					num5 = j;
				}
			}
			if (num5 >= 0)
			{
				CloseAreaRect1 = new Rect(genericIndexer[num5, 0], genericIndexer[num5, 1], genericIndexer[num5, 2], genericIndexer[num5, 3]);
			}
			if (num <= 2)
			{
				CArea1 = num4;
				return;
			}
			double num6 = 0.0;
			int num7 = -1;
			for (int k = 1; k < num; k++)
			{
				if (k != num3 && k != num5 && (double)genericIndexer[k, 4] > num6)
				{
					num6 = genericIndexer[k, 4];
					num7 = k;
				}
			}
			CArea1 = num4;
			CArea2 = num6;
			if (num7 >= 0)
			{
				CloseAreaRect2 = new Rect(genericIndexer[num7, 0], genericIndexer[num7, 1], genericIndexer[num7, 2], genericIndexer[num7, 3]);
			}
		}
		catch (Exception ex)
		{
			string text = "MT02-MatchTool-findBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Match-005");
			}
		}
	}

	private int doMatchTemplete(Mat Smat, Mat Pmat, out Rect rt)
	{
		Cv2.MatchTemplate(Smat, Pmat, MatchImage, TemplateMatchModes.CCoeffNormed);
		Cv2.MinMaxLoc(MatchImage, out var _, out var maxVal, out var _, out var maxLoc);
		rt = new Rect(maxLoc.X, maxLoc.Y, Pmat.Cols, Pmat.Rows);
		rt &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
		return (int)(maxVal * 100.0);
	}

	private int doMatchShape(INSP_MATCHTOOL_PARAM Param, int index, Mat Smat, Mat Pmat, out Rect rt)
	{
		rt = new Rect(0, 0, 0, 0);
		if (Param.DEStart > 0 && Param.DEStartCount > 0)
		{
			doMorph(ref Smat, Param.DEStart, Param.DEStartCount);
		}
		Cv2.FindContours(Pmat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
		Cv2.FindContours(Smat, out Point[][] contours2, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
		double num = 0.0;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < contours.Length; i++)
		{
			double num4 = Cv2.ContourArea(contours[i]);
			if (num4 > num)
			{
				num = num4;
				num2 = i;
			}
		}
		num = 0.0;
		double num5 = 0.0;
		RotatedRect rotatedRect = default(RotatedRect);
		for (int j = 0; j < contours2.Length; j++)
		{
			double num6 = Cv2.ContourArea(contours2[j]);
			if (num6 > num)
			{
				num3 = j;
				num = num6;
			}
		}
		if (contours.Length == 0 || contours2.Length == 0)
		{
			return 0;
		}
		if (num2 < 0 || num3 < 0)
		{
			return 0;
		}
		num5 = Cv2.MatchShapes(contours[num2], contours2[num3], ShapeMatchModes.I3);
		rt = rotatedRect.BoundingRect();
		rt &= new Rect(0, 0, Smat.Width, Smat.Height);
		int num7 = 0;
		if (num5 > 1.0)
		{
			return 40;
		}
		return (int)((1.0 - num5) * 100.0);
	}

	private string OCRImage(Mat mat, bool TextisWhite)
	{
		return "";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(int index, ref string ResultStr)
	{
		ResultStr = "";
		ModelAReaded = false;
		string arg = "";
		INSP_MATCHTOOL_PARAM iNSP_MATCHTOOL_PARAM;
		if (pfile.MatchParam[index].QuickSetupIndex >= 0 && pfile.MatchParam[index].QuickSetupIndex < 50)
		{
			iNSP_MATCHTOOL_PARAM = pfile.MatchQuickSetupParam[pfile.MatchParam[index].QuickSetupIndex];
			iNSP_MATCHTOOL_PARAM.TrackerName = pfile.MatchParam[index].TrackerName;
			arg = string.Format("[使用快速設定{0}-{1}]", index + 1, pfile.MatchParam[index].TrackerName);
		}
		else
		{
			iNSP_MATCHTOOL_PARAM = pfile.MatchParam[index];
		}
		DateTime now = DateTime.Now;
		string arg2 = now.ToString("yyyyMM");
		string text = string.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory, "Log\\", arg2);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string arg3 = now.ToString("yyyyMMdd");
		string path = string.Format("{0}\\{1}-AILoad.log", text, arg3);
		string text2 = now.ToString("[yyyy-MM-dd] [HH:mm:ss]");
		string.Format("{0}{1}\t", text2, arg);
		string text3 = "";
		if (!DllExist)
		{
			ResultStr = string.Format("圖形比對工具[{0}][{1}]找不到AI DLL({2})。\n", index + 1, iNSP_MATCHTOOL_PARAM.TrackerName, "DeltaAIProcessDll.dll");
			text3 = text2 + ResultStr;
			using (StreamWriter streamWriter = new StreamWriter(path, append: true))
			{
				streamWriter.Write("{0}", text3);
				streamWriter.WriteLine();
			}
			return false;
		}
		if (iNSP_MATCHTOOL_PARAM.OnnxName == "")
		{
			ResultStr = string.Format("圖形比對工具[{0}][{1}]尚未設定AI字元檢測模型。\n", index + 1, iNSP_MATCHTOOL_PARAM.TrackerName);
			text3 = text2 + ResultStr;
			using (StreamWriter streamWriter2 = new StreamWriter(path, append: true))
			{
				streamWriter2.Write("{0}", text3);
				streamWriter2.WriteLine();
			}
			return false;
		}
		text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\");
		string text4 = string.Format("{0}{1}", text, iNSP_MATCHTOOL_PARAM.OnnxName);
		if (!File.Exists(text4))
		{
			ResultStr = string.Format("圖形比對工具[{0}][{1}]找不到AI字元檢測模型檔案({2})。\n", index + 1, iNSP_MATCHTOOL_PARAM.TrackerName, text4);
			text3 = text2 + ResultStr;
			using (StreamWriter streamWriter3 = new StreamWriter(path, append: true))
			{
				streamWriter3.Write("{0}", text3);
				streamWriter3.WriteLine();
			}
			return false;
		}
		if (!ModelACreatInst)
		{
			CreatInst();
		}
		try
		{
			ReadModelONNX(m_DeltaAIProcess_A, text4, useCUDA);
			ModelAReaded = true;
			FileInfo fileInfo = new FileInfo(text4);
			string text5 = string.Format("圖形比對工具[{0}][{1}]模型檔案讀取成功({2})，模型檔案日期({3})。\n", index + 1, iNSP_MATCHTOOL_PARAM.TrackerName, text4, fileInfo.LastWriteTime.ToString());
			text3 = text2 + text5;
			using (StreamWriter streamWriter4 = new StreamWriter(path, append: true))
			{
				streamWriter4.Write("{0}", text3);
				streamWriter4.WriteLine();
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MT02-MatchTool-ReadModel()-" + ex.ToString());
			ResultStr = string.Format("圖形比對工具[{0}][{1}]讀取AI字元檢測模型檔案失敗。({2})。\n", index + 1, iNSP_MATCHTOOL_PARAM.TrackerName, text4);
			ModelAReaded = false;
			return false;
		}
	}

	public void CreatInst()
	{
		if (!ModelACreatInst)
		{
			int gpu_id = 0;
			m_DeltaAIProcess_A = CreateInstance();
			if (useCUDA)
			{
				SetGPU(m_DeltaAIProcess_A, gpu_id);
			}
			ModelACreatInst = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CheckSimilarity(int index, Mat Pattern, Mat CheckImg, ref int Score)
	{
		try
		{
			if (Pattern.Width <= 0 || Pattern.Height <= 0)
			{
				return false;
			}
			if (CheckImg.Width <= 0 || CheckImg.Height <= 0)
			{
				return false;
			}
			if (!ModelACreatInst)
			{
				CreatInst();
			}
			if (!ModelAReaded)
			{
				string ResultStr = "";
				if (!ReadModel(index, ref ResultStr))
				{
					return false;
				}
			}
			AIPatternMat = Pattern.Resize(AIImageSize);
			AIPredictMat = CheckImg.Resize(AIImageSize);
			PredResult = 0.0;
			sw.Start();
			PredResult = PredictImageSimilarity(m_DeltaAIProcess_A, AIPatternMat.CvPtr, AIPredictMat.CvPtr);
			sw.Stop();
			_ = sw.ElapsedMilliseconds;
			sw.Reset();
			if (PredResult >= 4.0 || PredResult < 0.0)
			{
				Score = 0;
			}
			else
			{
				Score = (int)((4.0 - PredResult) * 25.0);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "MT02-MatchTool-CheckSimilarity()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "Match-006");
			}
			return false;
		}
	}
}
