using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace InspTools.AOI;

public class FontTool
{
	private const int MaxPublicPattern = 210;

	private const int MaxFonts = 9;

	private const int MaxFontPattern = 5;

	private bool ModelACreatInst;

	private IntPtr m_DeltaAIProcess_F1;

	private IntPtr m_DeltaAIProcess_F2;

	private IntPtr m_DeltaAIProcess_F3;

	private IntPtr m_DeltaAIProcess_F4;

	private IntPtr m_DeltaAIProcess_F5;

	private IntPtr m_DeltaAIProcess_F6;

	private IntPtr m_DeltaAIProcess_F7;

	private IntPtr m_DeltaAIProcess_F8;

	private IntPtr m_DeltaAIProcess_F9;

	private IntPtr m_DeltaAIProcess_F10;

	public InferenceSession[] FontOnnxModel = new InferenceSession[10];

	private Size AIImageSize = new Size(28, 28);

	private Size[] AIMatchImageSize = new Size[10];

	private bool useCUDA;

	public bool[] FontModelReaded = new bool[10];

	public int[] FontModelOnnxType = new int[10];

	public bool OCRModelReaded;

	public bool FontIsBlack;

	private string RecipeName = "";

	private CVBuffer buffers;

	public Mat BufferCopy = new Mat();

	private ParamFile pfile;

	private Rect InspeRect = new Rect(0, 0, 50, 50);

	private int MatchIndex;

	private Mat SearchMatALL = new Mat();

	private Mat FoundMatALL = new Mat();

	private Mat FoundMat = new Mat();

	private Mat PatternMAT = new Mat();

	public Mat[] FoundFontMat = new Mat[9];

	private Mat[] AIMatchTempMat = new Mat[9];

	private Mat[] FontPattern = new Mat[5];

	public Mat[] SearchMat = new Mat[9];

	public int[] FontMatchScore = new int[9];

	public bool[] FontResult = new bool[9];

	public bool[] OCRResult = new bool[9];

	public bool[] ScoreResult = new bool[9];

	private string[] FontChar = new string[9];

	private Mat OcrFinalMat = new Mat();

	public Mat[] FontMatR = new Mat[5];

	public Mat[] FontMatM = new Mat[5];

	public Mat[] FontMatC = new Mat[5];

	public Mat[] FontMatU = new Mat[5];

	public Mat[] FontMatX = new Mat[5];

	public Mat[] FontMatB = new Mat[5];

	public Mat[] FontMatD = new Mat[5];

	public Mat[] FontMatE = new Mat[5];

	public Mat[] FontMatA = new Mat[5];

	public Mat[] FontMatDash = new Mat[5];

	public Mat[] FontMat0 = new Mat[5];

	public Mat[] FontMat1 = new Mat[5];

	public Mat[] FontMat2 = new Mat[5];

	public Mat[] FontMat3 = new Mat[5];

	public Mat[] FontMat4 = new Mat[5];

	public Mat[] FontMat5 = new Mat[5];

	public Mat[] FontMat6 = new Mat[5];

	public Mat[] FontMat7 = new Mat[5];

	public Mat[] FontMat8 = new Mat[5];

	public Mat[] FontMat9 = new Mat[5];

	public Mat[] FontBinary = new Mat[9];

	public Mat[][] FontMatOther = new Mat[20][];

	public Mat[] AllFontPattern = new Mat[9];

	public Mat FontConnectBinary = new Mat();

	public bool FontConnectNG;

	public bool FirstMatchResult;

	private Rect[] PatternRect = new Rect[9];

	private int[] PatternArea = new int[9];

	public bool PatternRectSeted;

	private int PatternRectMaxW;

	private int PatternRectMaxH;

	private int MinPatternArea;

	private int MaxPatternWH;

	private double AutoBinaryThd = -1.0;

	public int MatchScore_1;

	public int MatchScore_2;

	public List<Rect> NGRect = new List<Rect>();

	public int[][] AIMatchScore = new int[9][];

	public string[] ModelName = new string[9];

	public bool FlipStatus;

	private Mat FlipMat = new Mat();

	private Mat AIPatternMat = new Mat();

	private Mat AIPredictMat = new Mat();

	public Rect FoundMatchTextRect = new Rect(0, 0, 0, 0);

	private Rect[] FontSearchRect = new Rect[9];

	private Rect[] FontMatchRect = new Rect[9];

	public bool[] AdvFontCheck = new bool[9];

	public int ColorMatchScore;

	public bool FirstMatchALLResult;

	public int FirstMatchScore;

	public List<SimpleBlobStruct> MultiBlob = new List<SimpleBlobStruct>();

	private Stopwatch sw = new Stopwatch();

	private bool DllExist;

	private const int MaxOCRCount = 37;

	private Mat OCRMat = new Mat();

	private Mat OCRMatColor = new Mat();

	private Mat OCRMatTemp = new Mat();

	private Mat OCRMatReslut = new Mat();

	private Mat MaskArea = new Mat();

	private const int MaxFontCheckCount = 25;

	public bool TrainAIFontParamMode;

	public bool[] FontCheckResult = new bool[25];

	public float[] FontParaResult = new float[25];

	public float[] FontParaTemp = new float[25];

	public bool[] FontUsePara = new bool[25];

	public bool[] FontUseParaMin = new bool[25];

	public bool[] FontUseParaMax = new bool[25];

	public int FontCheckCount;

	private Mat SearchMatCloned = new Mat();

	private Mat SearchMatClonedTemp = new Mat();

	private Mat PatternMatCloned = new Mat();

	private Mat PatternMatClonedTemp = new Mat();

	private Mat FindAllMatchImage = new Mat();

	private Mat Score1MatchImage = new Mat();

	private Mat Score2MatchImage = new Mat();

	private Mat orgMat = new Mat();

	public Mat[] OcrMat1 = new Mat[9];

	public Mat[] CheckFontMat = new Mat[9];

	private Mat[] OcrMat2 = new Mat[9];

	private Mat FindAllI1 = new Mat();

	private Mat FindAllI2 = new Mat();

	private Mat FindAllI3 = new Mat();

	private Mat FindFontRectBinary = new Mat();

	private Mat[] CheckFontBinary = new Mat[9];

	private Mat[] CheckFontPBinary = new Mat[9];

	private int[] PatternMatchScore = new int[5];

	private Rect[] PatternMatchRect = new Rect[5];

	private IntPtr m_DeltaAIProcess_OCR;

	private INSP_FONTTOOL_PARAM FParam;

	private bool OffLine = true;

	public bool MatchSuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

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

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern void ReadModel(IntPtr pShapeInstance, string modelname, bool useCUDA);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern IntPtr PredictImage(IntPtr pShapeInstance, IntPtr Image);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public FontTool(ParamFile paramfile)
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
		for (int i = 0; i < 9; i++)
		{
			SearchMat[i] = new Mat();
			FoundFontMat[i] = new Mat();
			AIMatchTempMat[i] = new Mat();
			FontResult[i] = false;
			AIMatchScore[i] = new int[5];
			FontUsePara[i] = false;
			OcrMat1[i] = new Mat();
			OcrMat2[i] = new Mat();
			CheckFontMat[i] = new Mat();
			CheckFontBinary[i] = new Mat();
			CheckFontPBinary[i] = new Mat();
			FontBinary[i] = new Mat();
			AllFontPattern[i] = new Mat();
			ModelName[i] = "";
		}
		for (int j = 0; j <= 9; j++)
		{
			FontModelReaded[j] = false;
			FontModelOnnxType[j] = 0;
			AIMatchImageSize[j] = new Size(16, 32);
		}
		ResetFontMat();
	}

	private void ResetFontMat()
	{
		for (int i = 0; i < 5; i++)
		{
			FontPattern[i] = new Mat();
			FontMatR[i] = new Mat();
			FontMatM[i] = new Mat();
			FontMatC[i] = new Mat();
			FontMatU[i] = new Mat();
			FontMatX[i] = new Mat();
			FontMatB[i] = new Mat();
			FontMatD[i] = new Mat();
			FontMatA[i] = new Mat();
			FontMatE[i] = new Mat();
			FontMatDash[i] = new Mat();
			FontMat0[i] = new Mat();
			FontMat1[i] = new Mat();
			FontMat2[i] = new Mat();
			FontMat3[i] = new Mat();
			FontMat4[i] = new Mat();
			FontMat5[i] = new Mat();
			FontMat6[i] = new Mat();
			FontMat7[i] = new Mat();
			FontMat8[i] = new Mat();
			FontMat9[i] = new Mat();
		}
		for (int j = 0; j < 20; j++)
		{
			FontMatOther[j] = new Mat[5];
			for (int k = 0; k < 5; k++)
			{
				FontMatOther[j][k] = new Mat();
			}
		}
	}

	public void SetRecipeName(string name)
	{
		RecipeName = name;
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void UpdateFontPattern()
	{
		string text = "";
		ResetFontMat();
		try
		{
			if (pfile.FontToolParam.FontDBUsePublicMode)
			{
				text = AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\Font\\";
			}
			else
			{
				string path = string.Format("{0}Recipe\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, RecipeName);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				text = string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, RecipeName);
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text))
			{
				CVBuffer.SaveLog(string.Format("MTW07-FontTool-UpdateFontPattern() - 資料夾{0}建立失敗", text));
				return;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MTW07-FontTool-UpdateFontPattern()" + ex.ToString());
			return;
		}
		try
		{
			string[] files = Directory.GetFiles(text, "*.bmp", SearchOption.AllDirectories);
			int num = 0;
			string text2 = "R_";
			string text3 = "";
			for (int i = 0; i < files.Length; i++)
			{
				text3 = Path.GetFileNameWithoutExtension(files[i]);
				if (text3.Length != 4)
				{
					continue;
				}
				text2 = text3.Substring(0, 2);
				text3.Substring(3, 1);
				num = Convert.ToInt32(text3.Substring(2, 1)) - 1;
				if (!File.Exists(files[i]) || text2 == null)
				{
					continue;
				}
				int length = text2.Length;
				if (length != 2)
				{
					continue;
				}
				switch (text2[0])
				{
				case 'R':
					if (text2 == "R_")
					{
						FontMatR[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'M':
					if (text2 == "M_")
					{
						FontMatM[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'C':
					if (text2 == "C_")
					{
						FontMatC[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'U':
					if (text2 == "U_")
					{
						FontMatU[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'X':
					if (text2 == "X_")
					{
						FontMatX[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'B':
					if (text2 == "B_")
					{
						FontMatB[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'D':
					if (text2 == "D_")
					{
						FontMatD[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'E':
					if (text2 == "E_")
					{
						FontMatE[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'A':
					if (text2 == "A_")
					{
						FontMatA[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '-':
					if (text2 == "-_")
					{
						FontMatDash[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '0':
					if (text2 == "0_")
					{
						FontMat0[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '1':
					if (text2 == "1_")
					{
						FontMat1[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '2':
					if (text2 == "2_")
					{
						FontMat2[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '3':
					if (text2 == "3_")
					{
						FontMat3[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '4':
					if (text2 == "4_")
					{
						FontMat4[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '5':
					if (text2 == "5_")
					{
						FontMat5[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '6':
					if (text2 == "6_")
					{
						FontMat6[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '7':
					if (text2 == "7_")
					{
						FontMat7[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '8':
					if (text2 == "8_")
					{
						FontMat8[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case '9':
					if (text2 == "9_")
					{
						FontMat9[num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'F':
					if (text2 == "F_")
					{
						FontMatOther[0][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'G':
					if (text2 == "G_")
					{
						FontMatOther[1][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'H':
					if (text2 == "H_")
					{
						FontMatOther[2][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'I':
					if (text2 == "I_")
					{
						FontMatOther[3][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'J':
					if (text2 == "J_")
					{
						FontMatOther[4][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'K':
					if (text2 == "K_")
					{
						FontMatOther[5][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'L':
					if (text2 == "L_")
					{
						FontMatOther[6][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'N':
					if (text2 == "N_")
					{
						FontMatOther[7][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'O':
					if (text2 == "O_")
					{
						FontMatOther[8][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'P':
					if (text2 == "P_")
					{
						FontMatOther[9][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'Q':
					if (text2 == "Q_")
					{
						FontMatOther[10][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'S':
					if (text2 == "S_")
					{
						FontMatOther[11][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'T':
					if (text2 == "T_")
					{
						FontMatOther[12][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'V':
					if (text2 == "V_")
					{
						FontMatOther[13][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'W':
					if (text2 == "W_")
					{
						FontMatOther[14][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'Y':
					if (text2 == "Y_")
					{
						FontMatOther[15][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				case 'Z':
					if (text2 == "Z_")
					{
						FontMatOther[16][num] = CVBuffer.ImRead(files[i], ImreadModes.Grayscale);
					}
					break;
				}
			}
		}
		catch (Exception ex2)
		{
			string text4 = "MTW07-FontTool-UpdateFontPattern()";
			CVBuffer.SaveLog(text4);
			_ = text4 + ex2.ToString();
		}
	}

	public void GetInspeRect()
	{
		LocateRect = buffers.LocatedRect;
		INSP_FONTTOOL_PARAM fontToolParam = pfile.FontToolParam;
		InspeRect.X = pfile.AdvLocateParam[0].L + fontToolParam.Track1_L;
		InspeRect.Y = pfile.AdvLocateParam[0].T + fontToolParam.Track1_T;
		InspeRect.Width = pfile.AdvLocateParam[0].Width - (fontToolParam.Track1_L + fontToolParam.Track1_R);
		InspeRect.Height = pfile.AdvLocateParam[0].Height - (fontToolParam.Track1_T + fontToolParam.Track1_B);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool FindAllMatchText(Mat SearchMat, Mat patternmat, int channel)
	{
		try
		{
			bool useFastMatch = pfile.FontToolParam.UseFastMatch;
			FirstMatchResult = true;
			if (useFastMatch)
			{
				SearchMat.CopyTo(SearchMatClonedTemp);
				patternmat.CopyTo(PatternMatClonedTemp);
				double num = 0.5;
				Cv2.Resize(SearchMatClonedTemp, SearchMatCloned, default(Size), num, num);
				Cv2.Resize(PatternMatClonedTemp, PatternMatCloned, default(Size), num, num);
			}
			else
			{
				SearchMat.CopyTo(SearchMatCloned);
				patternmat.CopyTo(PatternMatCloned);
			}
			MatchScore_1 = 0;
			MatchScore_2 = 0;
			Cv2.MatchTemplate(SearchMatCloned, PatternMatCloned, FindAllMatchImage, TemplateMatchModes.CCoeffNormed);
			double minVal = 0.0;
			Cv2.MinMaxLoc(FindAllMatchImage, out minVal, out var maxVal, out var minLoc, out var maxLoc);
			Rect rect = new Rect(maxLoc.X, maxLoc.Y, PatternMatCloned.Cols, PatternMatCloned.Rows);
			FoundMatchTextRect = new Rect(0, 0, 0, 0);
			FirstMatchScore = 0;
			rect &= new Rect(0, 0, buffers.GetChannelMat(channel).Cols, buffers.GetChannelMat(channel).Rows);
			if (rect.Width != PatternMatCloned.Width || rect.Height != PatternMatCloned.Height)
			{
				FirstMatchResult = false;
				FoundMatchTextRect.Width = (FoundMatchTextRect.Height = 0);
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字定位] 比對整張影像失敗，[{0}] 比對分數={1}。\n", pfile.FontToolParam.AllFont, MatchScore_1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字定位] 比对整张影像失败，[{0}] 比对分数={1}。\n", pfile.FontToolParam.AllFont, MatchScore_1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[テキスト検出] 全体画像の照合に失敗しました、[{0}] 照合スコア={1}。\n", pfile.FontToolParam.AllFont, MatchScore_1);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Locate] Match error，[{0}] Match score ={1}。\n", pfile.FontToolParam.AllFont, MatchScore_1);
				}
				return false;
			}
			new Mat(SearchMatCloned, rect).CopyTo(FoundMatALL);
			MatchScore_1 = (int)(maxVal * 100.0);
			if (MatchScore_1 > 100)
			{
				MatchScore_1 = 100;
			}
			if (MatchScore_1 < 0)
			{
				MatchScore_1 = 0;
			}
			FirstMatchScore = MatchScore_1;
			if (!useFastMatch)
			{
				FoundMatchTextRect = rect;
				FoundMatchTextRect.Left = rect.Left + InspeRect.Left;
				FoundMatchTextRect.Top = rect.Top + InspeRect.Top;
			}
			else
			{
				FoundMatchTextRect = rect;
				FoundMatchTextRect.Left = rect.Left * 2 + InspeRect.Left;
				FoundMatchTextRect.Top = rect.Top * 2 + InspeRect.Top;
				FoundMatchTextRect.Width = rect.Width * 2;
				FoundMatchTextRect.Height = rect.Height * 2;
			}
			if (MatchScore_1 > 90 || pfile.SelectWayON)
			{
				FlipStatus = false;
				pfile.FontToolParam.FoundMatchTextRect = FoundMatchTextRect;
				return true;
			}
			Cv2.Flip(PatternMatCloned, FlipMat, FlipMode.XY);
			Cv2.MatchTemplate(SearchMatCloned, FlipMat, FindAllMatchImage, TemplateMatchModes.CCoeffNormed);
			Cv2.MinMaxLoc(FindAllMatchImage, out minVal, out maxVal, out minLoc, out maxLoc);
			Rect rect2 = default(Rect);
			rect2 = new Rect(maxLoc.X, maxLoc.Y, PatternMatCloned.Cols, PatternMatCloned.Rows);
			rect2 &= new Rect(0, 0, buffers.GetChannelMat(channel).Cols, buffers.GetChannelMat(channel).Rows);
			MatchScore_2 = (int)(maxVal * 100.0);
			if (MatchScore_2 > 100)
			{
				MatchScore_2 = 100;
			}
			if (MatchScore_2 < 0)
			{
				MatchScore_2 = 0;
			}
			if (MatchScore_2 < pfile.FontToolParam.MinMatchScore && MatchScore_1 < pfile.FontToolParam.MinMatchScore)
			{
				FirstMatchResult = false;
				if (MatchScore_1 > MatchScore_2)
				{
					FoundMatchTextRect = rect;
				}
				else
				{
					FoundMatchTextRect = rect2;
				}
				if (!useFastMatch)
				{
					FoundMatchTextRect.Left += InspeRect.Left;
					FoundMatchTextRect.Top += InspeRect.Top;
				}
				else
				{
					FoundMatchTextRect.Left = FoundMatchTextRect.Left * 2 + InspeRect.Left;
					FoundMatchTextRect.Top = FoundMatchTextRect.Top * 2 + InspeRect.Top;
					FoundMatchTextRect.Width *= 2;
					FoundMatchTextRect.Height *= 2;
				}
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字定位] 比對整張影像失敗，[{0}] 比對分數={1}。(設定分數={2})\n", pfile.FontToolParam.AllFont, Math.Max(MatchScore_2, MatchScore_1), pfile.FontToolParam.MinMatchScore);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字定位] 比对整张影像失败，[{0}] 比对分数={1}。(设定分数={2}\n", pfile.FontToolParam.AllFont, Math.Max(MatchScore_2, MatchScore_1), pfile.FontToolParam.MinMatchScore);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[テキスト検出] 画像全体の照合に失敗しました、[{0}] 照合スコア={1}。(設定スコア={2})\n", pfile.FontToolParam.AllFont, Math.Max(MatchScore_2, MatchScore_1), pfile.FontToolParam.MinMatchScore);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Locate] Match error，[{0}] Match score ={1}。(Setup Min Score={2})\n", pfile.FontToolParam.AllFont, Math.Max(MatchScore_2, MatchScore_1), pfile.FontToolParam.MinMatchScore);
				}
				return false;
			}
			FirstMatchResult = true;
			if (MatchScore_2 > MatchScore_1)
			{
				FirstMatchScore = MatchScore_2;
				if (!useFastMatch)
				{
					FoundMatchTextRect = rect2;
					FoundMatchTextRect.Left = rect2.Left + InspeRect.Left;
					FoundMatchTextRect.Top = rect2.Top + InspeRect.Top;
				}
				else
				{
					FoundMatchTextRect = rect2;
					FoundMatchTextRect.Left = rect2.Left * 2 + InspeRect.Left;
					FoundMatchTextRect.Top = rect2.Top * 2 + InspeRect.Top;
					FoundMatchTextRect.Width = rect2.Width * 2;
					FoundMatchTextRect.Height = rect2.Height * 2;
				}
				FlipStatus = true;
				new Mat(BufferCopy, FoundMatchTextRect).CopyTo(FindAllI1);
				FindAllI3 = new Mat(BufferCopy, FoundMatchTextRect);
				Cv2.Flip(FindAllI1, FindAllI2, FlipMode.XY);
				FindAllI2.CopyTo(FindAllI3);
			}
			else if (!useFastMatch)
			{
				FlipStatus = false;
				FoundMatchTextRect = rect;
				FoundMatchTextRect.Left = rect.Left + InspeRect.Left;
				FoundMatchTextRect.Top = rect.Top + InspeRect.Top;
			}
			else
			{
				FoundMatchTextRect = rect;
				FoundMatchTextRect.Left = rect.Left * 2 + InspeRect.Left;
				FoundMatchTextRect.Top = rect.Top * 2 + InspeRect.Top;
				FoundMatchTextRect.Width = rect.Width * 2;
				FoundMatchTextRect.Height = rect.Height * 2;
			}
			pfile.FontToolParam.FoundMatchTextRect = FoundMatchTextRect;
			return true;
		}
		catch (Exception ex)
		{
			string text = "MAT02-FontTool-FindAllMatchText()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-001");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetFontCharFromIndex(int index)
	{
		return index switch
		{
			0 => "R", 
			1 => "M", 
			2 => "C", 
			3 => "U", 
			4 => "-", 
			5 => "0", 
			6 => "1", 
			7 => "2", 
			8 => "3", 
			9 => "4", 
			10 => "5", 
			11 => "6", 
			12 => "7", 
			13 => "8", 
			14 => "9", 
			15 => "X", 
			16 => "B", 
			17 => "E", 
			18 => "D", 
			19 => "A", 
			20 => "F", 
			21 => "G", 
			22 => "H", 
			23 => "I", 
			24 => "J", 
			25 => "K", 
			26 => "L", 
			27 => "N", 
			28 => "O", 
			29 => "P", 
			30 => "Q", 
			31 => "S", 
			32 => "T", 
			33 => "V", 
			34 => "W", 
			35 => "Y", 
			36 => "Z", 
			_ => "", 
		};
	}

	public void GetFontPattern(int index, ref Mat[] pattern)
	{
		switch (index)
		{
		case 0:
			pattern = FontMatR;
			break;
		case 1:
			pattern = FontMatM;
			break;
		case 2:
			pattern = FontMatC;
			break;
		case 3:
			pattern = FontMatU;
			break;
		case 4:
			pattern = FontMatDash;
			break;
		case 5:
			pattern = FontMat0;
			break;
		case 6:
			pattern = FontMat1;
			break;
		case 7:
			pattern = FontMat2;
			break;
		case 8:
			pattern = FontMat3;
			break;
		case 9:
			pattern = FontMat4;
			break;
		case 10:
			pattern = FontMat5;
			break;
		case 11:
			pattern = FontMat6;
			break;
		case 12:
			pattern = FontMat7;
			break;
		case 13:
			pattern = FontMat8;
			break;
		case 14:
			pattern = FontMat9;
			break;
		case 15:
			pattern = FontMatX;
			break;
		case 16:
			pattern = FontMatB;
			break;
		case 17:
			pattern = FontMatE;
			break;
		case 18:
			pattern = FontMatD;
			break;
		case 19:
			pattern = FontMatA;
			break;
		case 20:
			pattern = FontMatOther[0];
			break;
		case 21:
			pattern = FontMatOther[1];
			break;
		case 22:
			pattern = FontMatOther[2];
			break;
		case 23:
			pattern = FontMatOther[3];
			break;
		case 24:
			pattern = FontMatOther[4];
			break;
		case 25:
			pattern = FontMatOther[5];
			break;
		case 26:
			pattern = FontMatOther[6];
			break;
		case 27:
			pattern = FontMatOther[7];
			break;
		case 28:
			pattern = FontMatOther[8];
			break;
		case 29:
			pattern = FontMatOther[9];
			break;
		case 30:
			pattern = FontMatOther[10];
			break;
		case 31:
			pattern = FontMatOther[11];
			break;
		case 32:
			pattern = FontMatOther[12];
			break;
		case 33:
			pattern = FontMatOther[13];
			break;
		case 34:
			pattern = FontMatOther[14];
			break;
		case 35:
			pattern = FontMatOther[15];
			break;
		case 36:
			pattern = FontMatOther[16];
			break;
		default:
			pattern = FontMatR;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetFontPattern(string str, Mat pattern)
	{
		if (str == null)
		{
			return;
		}
		switch (str.Length)
		{
		case 1:
			switch (str[0])
			{
			case 'R':
				pattern.CopyTo(FontMatR[0]);
				break;
			case 'M':
				pattern.CopyTo(FontMatM[0]);
				break;
			case 'C':
				pattern.CopyTo(FontMatC[0]);
				break;
			case 'U':
				pattern.CopyTo(FontMatU[0]);
				break;
			case '0':
				pattern.CopyTo(FontMat0[0]);
				break;
			case '1':
				pattern.CopyTo(FontMat1[0]);
				break;
			case '2':
				pattern.CopyTo(FontMat2[0]);
				break;
			case '3':
				pattern.CopyTo(FontMat3[0]);
				break;
			case '4':
				pattern.CopyTo(FontMat4[0]);
				break;
			case '5':
				pattern.CopyTo(FontMat5[0]);
				break;
			case '6':
				pattern.CopyTo(FontMat6[0]);
				break;
			case '7':
				pattern.CopyTo(FontMat7[0]);
				break;
			case '8':
				pattern.CopyTo(FontMat8[0]);
				break;
			case '9':
				pattern.CopyTo(FontMat9[0]);
				break;
			case 'X':
				pattern.CopyTo(FontMatX[0]);
				break;
			case 'B':
				pattern.CopyTo(FontMatB[0]);
				break;
			case 'E':
				pattern.CopyTo(FontMatE[0]);
				break;
			case 'D':
				pattern.CopyTo(FontMatD[0]);
				break;
			case 'A':
				pattern.CopyTo(FontMatA[0]);
				break;
			case 'F':
				pattern.CopyTo(FontMatOther[0][0]);
				break;
			case 'G':
				pattern.CopyTo(FontMatOther[1][0]);
				break;
			case 'H':
				pattern.CopyTo(FontMatOther[2][0]);
				break;
			case 'I':
				pattern.CopyTo(FontMatOther[3][0]);
				break;
			case 'J':
				pattern.CopyTo(FontMatOther[4][0]);
				break;
			case 'K':
				pattern.CopyTo(FontMatOther[5][0]);
				break;
			case 'L':
				pattern.CopyTo(FontMatOther[6][0]);
				break;
			case 'N':
				pattern.CopyTo(FontMatOther[7][0]);
				break;
			case 'O':
				pattern.CopyTo(FontMatOther[8][0]);
				break;
			case 'P':
				pattern.CopyTo(FontMatOther[9][0]);
				break;
			case 'Q':
				pattern.CopyTo(FontMatOther[10][0]);
				break;
			case 'S':
				pattern.CopyTo(FontMatOther[11][0]);
				break;
			case 'T':
				pattern.CopyTo(FontMatOther[12][0]);
				break;
			case 'V':
				pattern.CopyTo(FontMatOther[13][0]);
				break;
			case 'W':
				pattern.CopyTo(FontMatOther[14][0]);
				break;
			case 'Y':
				pattern.CopyTo(FontMatOther[15][0]);
				break;
			case 'Z':
				pattern.CopyTo(FontMatOther[16][0]);
				break;
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
			case '@':
				break;
			}
			break;
		case 4:
			if (str == "Dash")
			{
				pattern.CopyTo(FontMatDash[0]);
			}
			break;
		}
	}

	private void Calc8FontSearchRect(INSP_FONTTOOL_PARAM Param, string[] Fonts, ref Rect[] FontSearchRect)
	{
		int index = 1;
		int[] array = new int[8];
		int[] array2 = new int[8];
		int num = 4;
		int num2 = 2;
		int num3 = FoundMatchTextRect.Width / num;
		int num4 = FoundMatchTextRect.Height / num2;
		int num5 = FoundMatchTextRect.Width / 24;
		int num6 = FoundMatchTextRect.Height / 16;
		for (int i = 0; i < 8; i++)
		{
			array[i] = num5;
			array2[i] = num6;
		}
		pfile.FontToolParam.FontExtend = 20;
		for (int j = 0; j < Param.AllFont.Length; j++)
		{
			if (!(Fonts[j] == ""))
			{
				int num7 = array[j];
				int num8 = array2[j];
				int num9 = j % 4;
				int num10 = j / 4;
				if (j < 4)
				{
					FontSearchRect[j] = new Rect(FoundMatchTextRect.Left - num7 + num9 * num3, FoundMatchTextRect.Top - num8, num3 + num7 * 2, num4 + num8 * 2);
				}
				else
				{
					FontSearchRect[j] = new Rect(FoundMatchTextRect.Left - num7 + num9 * num3, FoundMatchTextRect.Top - num8 + num10 * num4, num3 + num7 * 2, num4 + num8 * 2);
				}
				FontSearchRect[j] &= new Rect(0, 0, buffers.GetChannelMat(index).Cols, buffers.GetChannelMat(index).Rows);
				if (FontSearchRect[j].Width > 0 && FontSearchRect[j].Height > 0)
				{
					SearchMat[j] = new Mat(BufferCopy, FontSearchRect[j]);
				}
			}
		}
	}

	private void Calc9FontSearchRect(INSP_FONTTOOL_PARAM Param, string[] Fonts, ref Rect[] FontSearchRect)
	{
		int index = 1;
		int[] array = new int[9];
		int[] array2 = new int[9];
		int num = 3;
		int num2 = 3;
		if (Param.AllFont.Length <= 6)
		{
			num2 = 2;
		}
		int num3 = FoundMatchTextRect.Width / num;
		int num4 = FoundMatchTextRect.Height / num2;
		int num5 = FoundMatchTextRect.Width / 23;
		int num6 = FoundMatchTextRect.Height / 23;
		for (int i = 0; i < 9; i++)
		{
			array[i] = num5;
			array2[i] = num6;
		}
		pfile.FontToolParam.FontExtend = 20;
		for (int j = 0; j < Param.AllFont.Length; j++)
		{
			if (!(Fonts[j] == ""))
			{
				int num7 = array[j];
				int num8 = array2[j];
				int num9 = j % 3;
				int num10 = j / 3;
				if (j < 3)
				{
					FontSearchRect[j] = new Rect(FoundMatchTextRect.Left - num7 + num9 * num3, FoundMatchTextRect.Top - num8 / 2, num3 + num7 * 2, num4 + (int)((double)num8 * 2.5));
				}
				else if (j < 6)
				{
					FontSearchRect[j] = new Rect(FoundMatchTextRect.Left - num7 + num9 * num3, FoundMatchTextRect.Top - num8 + num10 * num4, num3 + num7 * 2, num4 + num8 * 2);
				}
				else
				{
					FontSearchRect[j] = new Rect(FoundMatchTextRect.Left - num7 + num9 * num3, FoundMatchTextRect.Top - num8 + num10 * num4, num3 + num7 * 2, num4 + num8 * 2);
				}
				FontSearchRect[j] &= new Rect(0, 0, buffers.GetChannelMat(index).Cols, buffers.GetChannelMat(index).Rows);
				if (FontSearchRect[j].Width > 0 && FontSearchRect[j].Height > 0)
				{
					SearchMat[j] = new Mat(BufferCopy, FontSearchRect[j]);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CalcFontSearchRect(INSP_FONTTOOL_PARAM Param, string[] Fonts, bool[] FontDoubleWidth, ref Rect[] FontSearchRect)
	{
		int index = 1;
		int[] array = new int[9];
		int[] array2 = new int[9];
		if (!Param.FontAlignVertical)
		{
			array[0] = pfile.FontToolParam.WFontExtend1;
			array[1] = pfile.FontToolParam.WFontExtend2;
			array[2] = pfile.FontToolParam.WFontExtend3;
			array[3] = pfile.FontToolParam.WFontExtend4;
			array[4] = pfile.FontToolParam.WFontExtend5;
			array2[0] = pfile.FontToolParam.HFontExtend1;
			array2[1] = pfile.FontToolParam.HFontExtend2;
			array2[2] = pfile.FontToolParam.HFontExtend3;
			array2[3] = pfile.FontToolParam.HFontExtend4;
			array2[4] = pfile.FontToolParam.HFontExtend5;
			int num = Param.AllFont.Length;
			int num2 = 1;
			for (int i = 0; i < Param.AllFont.Length; i++)
			{
				if (Param.AllFont.Substring(i, 1) == "-")
				{
					num--;
					num2++;
				}
				if (FontDoubleWidth[i])
				{
					num++;
				}
			}
			if (num <= 0)
			{
				num = 1;
			}
			int num3 = FoundMatchTextRect.Width / num;
			int num4 = FoundMatchTextRect.Height / num2;
			int num5 = 0;
			for (int j = 0; j < Param.AllFont.Length; j++)
			{
				if (Fonts[j] == "1")
				{
					num5++;
				}
			}
			pfile.FontToolParam.FontExtend = 20;
			double num6 = 0.0;
			bool flag = false;
			for (int k = 0; k < Param.AllFont.Length; k++)
			{
				if (Fonts[k] == "")
				{
					continue;
				}
				int num7 = array[k] * 2;
				int num8 = array2[k];
				if (num5 == 1)
				{
					if (Fonts[k] == "1")
					{
						num3 = (int)((double)(FoundMatchTextRect.Width / num) * 0.7);
						num4 = FoundMatchTextRect.Height / num2;
					}
					else
					{
						num3 = (int)((double)(FoundMatchTextRect.Width / num) * 1.1);
						num4 = FoundMatchTextRect.Height / num2;
					}
				}
				if ((k == 0 || k == Param.AllFont.Length - 1) && Fonts[k] == "-")
				{
					if (k == 0)
					{
						FontSearchRect[k] = new Rect(FoundMatchTextRect.Left - num7, FoundMatchTextRect.Top - num8, FoundMatchTextRect.Width + num7 * 2, num4 + num8 * 2);
					}
					else
					{
						FontSearchRect[k] = new Rect(FoundMatchTextRect.Left - num7, FoundMatchTextRect.Top - num8 + num4, FoundMatchTextRect.Width + num7 * 2, num4 + num8 * 2);
					}
				}
				else
				{
					double num9 = num6 + 1.0;
					int num10 = num3;
					if (FontDoubleWidth[k])
					{
						num9++;
						num10 += num3;
					}
					if (Fonts[k] == "1" && num5 == 1)
					{
						num3 = (int)((double)(FoundMatchTextRect.Width / num) * 1.1);
						FontSearchRect[k] = new Rect(FoundMatchTextRect.Left - num7 + (int)((double)num3 * num6), FoundMatchTextRect.Top - num8, num10 + num7 * 2, FoundMatchTextRect.Height + num8 * 2);
						flag = true;
					}
					else if (flag)
					{
						FontSearchRect[k] = new Rect(FoundMatchTextRect.Left - num7 + (int)((double)num3 * num6) - (int)((double)num3 * 0.3), FoundMatchTextRect.Top - num8, num10 + num7 * 2, FoundMatchTextRect.Height + num8 * 2);
					}
					else
					{
						FontSearchRect[k] = new Rect(FoundMatchTextRect.Left - num7 + (int)((double)num3 * num6), FoundMatchTextRect.Top - num8, num10 + num7 * 2, FoundMatchTextRect.Height + num8 * 2);
					}
					num6 = num9;
				}
				FontSearchRect[k] &= new Rect(0, 0, buffers.GetChannelMat(index).Cols, buffers.GetChannelMat(index).Rows);
				if (FontSearchRect[k].Width > 0 && FontSearchRect[k].Height > 0)
				{
					SearchMat[k] = new Mat(BufferCopy, FontSearchRect[k]);
				}
			}
			return;
		}
		if (Fonts[0] == "-")
		{
			array[0] = pfile.FontToolParam.WFontExtend1;
			array[1] = pfile.FontToolParam.WFontExtend5;
			array[2] = pfile.FontToolParam.WFontExtend4;
			array[3] = pfile.FontToolParam.WFontExtend3;
			array[4] = pfile.FontToolParam.WFontExtend2;
			array2[0] = pfile.FontToolParam.HFontExtend1;
			array2[1] = pfile.FontToolParam.HFontExtend5;
			array2[2] = pfile.FontToolParam.HFontExtend4;
			array2[3] = pfile.FontToolParam.HFontExtend3;
			array2[4] = pfile.FontToolParam.HFontExtend2;
		}
		else if (Param.AllFont.Length == 4)
		{
			array[0] = pfile.FontToolParam.WFontExtend4;
			array[1] = pfile.FontToolParam.WFontExtend3;
			array[2] = pfile.FontToolParam.WFontExtend2;
			array[3] = pfile.FontToolParam.WFontExtend1;
			array2[0] = pfile.FontToolParam.HFontExtend4;
			array2[1] = pfile.FontToolParam.HFontExtend3;
			array2[2] = pfile.FontToolParam.HFontExtend2;
			array2[3] = pfile.FontToolParam.HFontExtend1;
		}
		else if (Param.AllFont.Length == 3)
		{
			array[0] = pfile.FontToolParam.WFontExtend3;
			array[1] = pfile.FontToolParam.WFontExtend2;
			array[2] = pfile.FontToolParam.WFontExtend1;
			array2[0] = pfile.FontToolParam.HFontExtend3;
			array2[1] = pfile.FontToolParam.HFontExtend2;
			array2[2] = pfile.FontToolParam.HFontExtend1;
		}
		else
		{
			array[0] = pfile.FontToolParam.WFontExtend5;
			array[1] = pfile.FontToolParam.WFontExtend4;
			array[2] = pfile.FontToolParam.WFontExtend3;
			array[3] = pfile.FontToolParam.WFontExtend2;
			array[4] = pfile.FontToolParam.WFontExtend1;
			array2[0] = pfile.FontToolParam.HFontExtend5;
			array2[1] = pfile.FontToolParam.HFontExtend4;
			array2[2] = pfile.FontToolParam.HFontExtend3;
			array2[3] = pfile.FontToolParam.HFontExtend2;
			array2[4] = pfile.FontToolParam.HFontExtend1;
		}
		int num11 = 1;
		int num12 = Param.AllFont.Length;
		for (int l = 0; l < Param.AllFont.Length; l++)
		{
			if (Param.AllFont.Substring(l, 1) == "-")
			{
				num11++;
				num12--;
			}
			if (FontDoubleWidth[l])
			{
				num12++;
			}
		}
		if (num12 <= 0)
		{
			num12 = 1;
		}
		int num13 = FoundMatchTextRect.Width / num11;
		int num14 = FoundMatchTextRect.Height / num12;
		double num15 = 0.0;
		bool flag2 = false;
		for (int m = 0; m < Param.AllFont.Length; m++)
		{
			if (Fonts[m] == "")
			{
				continue;
			}
			int num16 = array[m] * 2;
			int num17 = array2[m];
			if ((m == 0 || m == Param.AllFont.Length - 1) && Fonts[m] == "-")
			{
				flag2 = true;
				if (m == 0)
				{
					FontSearchRect[m] = new Rect(FoundMatchTextRect.Left - num17, FoundMatchTextRect.Top - num16, num13 + num17 * 2, FoundMatchTextRect.Height + num16 * 2);
				}
				else
				{
					FontSearchRect[m] = new Rect(FoundMatchTextRect.Left - num17 + num13, FoundMatchTextRect.Top - num16, num13 + num17 * 2, FoundMatchTextRect.Height + num16 * 2);
				}
			}
			else
			{
				double num18 = num15 + 1.0;
				int num19 = num14;
				if (FontDoubleWidth[m])
				{
					num18++;
					num19 += num14;
				}
				if (flag2)
				{
					FontSearchRect[m] = new Rect(FoundMatchTextRect.Left + 20 - num17, FoundMatchTextRect.Top - num16 + (int)((double)num14 * num15), FoundMatchTextRect.Width + num17 * 2 - 20, num19 + num16 * 2);
				}
				else
				{
					FontSearchRect[m] = new Rect(FoundMatchTextRect.Left - num17 - 10, FoundMatchTextRect.Top - num16 + (int)((double)num14 * num15), FoundMatchTextRect.Width + num17 * 2 + 20, num19 + num16 * 2);
				}
				num15 = num18;
			}
			FontSearchRect[m] &= new Rect(0, 0, buffers.GetChannelMat(index).Cols, buffers.GetChannelMat(index).Rows);
		}
		if (flag2)
		{
			if (!(Fonts[0] == "-"))
			{
				return;
			}
			Rect[] array3 = new Rect[Param.AllFont.Length];
			for (int n = 1; n < Param.AllFont.Length; n++)
			{
				array3[n] = FontSearchRect[Param.AllFont.Length - n];
			}
			for (int num20 = 0; num20 < Param.AllFont.Length; num20++)
			{
				if (num20 != 0)
				{
					FontSearchRect[num20] = array3[num20];
				}
				if (FontSearchRect[num20].Width > 0 && FontSearchRect[num20].Height > 0)
				{
					SearchMat[num20] = new Mat(BufferCopy, FontSearchRect[num20]);
				}
			}
			return;
		}
		Rect[] array4 = new Rect[Param.AllFont.Length];
		for (int num21 = 0; num21 < Param.AllFont.Length; num21++)
		{
			array4[num21] = FontSearchRect[Param.AllFont.Length - num21 - 1];
		}
		for (int num22 = 0; num22 < Param.AllFont.Length; num22++)
		{
			FontSearchRect[num22] = array4[num22];
			if (FontSearchRect[num22].Width > 0 && FontSearchRect[num22].Height > 0)
			{
				SearchMat[num22] = new Mat(BufferCopy, FontSearchRect[num22]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		OffLine = offline;
		FirstMatchALLResult = true;
		MatchSuccess = true;
		FlipStatus = false;
		Tracker1ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		NGRect.Clear();
		INSP_FONTTOOL_PARAM fontToolParam = pfile.FontToolParam;
		if (pfile.FontToolParam.TextMat == null)
		{
			pfile.FontToolParam.TextMat = new Mat();
		}
		for (int i = 0; i < 5; i++)
		{
			PatternMatchScore[i] = 0;
			PatternMatchRect[i] = new Rect(0, 0, 0, 0);
		}
		for (int j = 0; j < 9; j++)
		{
			FontMatchScore[j] = 0;
			FontResult[j] = false;
			FontMatchRect[j] = new Rect(0, 0, 0, 0);
			FontSearchRect[j] = new Rect(0, 0, 0, 0);
			AdvFontCheck[j] = true;
			OCRResult[j] = true;
			ScoreResult[j] = true;
		}
		try
		{
			if (FaceIndex != 0 && pfile.FontToolParam.EnableSelectFace && FaceIndex != pfile.FontToolParam.FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			if (fontToolParam.PatternSelect < 0 || fontToolParam.PatternSelect >= 210)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 圖形共用資料庫索引錯誤({0})，請重新選擇比對圖形。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 图形共用资料库索引错误({0})，请重新选择比对图形。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 画像共有データベースインデックスエラー({0})、比較画像を再度選択してください。\n", fontToolParam.PatternSelect + 1);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Tool] Public Pattern file index Error({0}), Select Pattern file again.\n", fontToolParam.PatternSelect + 1);
				}
				return false;
			}
			if (fontToolParam.PublicDBUsePublicMode)
			{
				pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternImage.CopyTo(PatternMAT);
				pfile.FontToolParam.PatternChannel = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternChannel;
				_ = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternON;
				_ = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternName;
			}
			else
			{
				pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternImage.CopyTo(PatternMAT);
				pfile.FontToolParam.PatternChannel = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternChannel;
				_ = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternON;
				_ = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternName;
			}
			buffers.GetChannelMat(fontToolParam.PatternChannel).CopyTo(BufferCopy);
			GetInspeRect();
			if (PatternMAT.Width <= 0 || PatternMAT.Height <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 圖形共用資料庫({0})影像遺失或檔案損毀。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 图形共用资料库({0})影像遗失或档案损毁。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 画像共有データベース({0})の画像が失われたか、ファイルが破損しています。\n", fontToolParam.PatternSelect + 1);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Tool]Public Image DB Image Error({0}).\n", fontToolParam.PatternSelect + 1);
				}
				return false;
			}
			int num = 0;
			num = fontToolParam.PatternChannel;
			InspeRect &= new Rect(0, 0, buffers.GetChannelMat(num).Cols, buffers.GetChannelMat(num).Rows);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，搜尋區域寬高小於0。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，搜寻区域宽高小于0。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー: 検索領域の幅と高さが 0 より小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Error，Search area is 0.\n");
				}
				return false;
			}
			if (fontToolParam.AllFont.Length <= 0 || fontToolParam.AllFont.Length > 9)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，尚未設定搜尋字元。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，尚未设定搜寻字元。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー、検索文字が設定されていません。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Setup Find char first.\n");
				}
				return false;
			}
			if (fontToolParam.MaskSelectIndex > 0)
			{
				int num2 = fontToolParam.MaskSelectIndex - 1;
				if (!pfile.MaskParam[num2].TrackerON)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("字元使用遮罩工具，但遮罩工具[{0}]未開啟! \n", num2 + 1);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("字元使用遮罩工具，但遮罩工具[{0}]未开启! \n", num2 + 1);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("文字はマスクツールを使用していますが、マスクツール[{0}]は有効になっていません! \n", num2 + 1);
					}
					else
					{
						Tracker1ResultText += string.Format("Font Tool Link Error, Mask Tool [{0}] has been Disable! \n", num2 + 1);
					}
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
				buffers.GetChannelMat(0).CopyTo(MaskArea);
				MaskArea.SetTo(0.0);
				Rect[] array = new Rect[2];
				if (num2 >= 0)
				{
					array[0] = new Rect(pfile.MaskParam[num2].Mask1Pt.X, pfile.MaskParam[num2].Mask1Pt.Y, pfile.MaskParam[num2].Mask1.Width, pfile.MaskParam[num2].Mask1.Height);
					array[0] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
					if (pfile.MaskParam[num2].SearchRect2ON && num2 < 20)
					{
						array[1] = new Rect(pfile.MaskParam[num2].Mask2Pt.X, pfile.MaskParam[num2].Mask2Pt.Y, pfile.MaskParam[num2].Mask2.Width, pfile.MaskParam[num2].Mask2.Height);
						array[1] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
					}
					else
					{
						array[1] = new Rect(0, 0, 0, 0);
					}
					if (array[0].Width > 0 && array[0].Height > 0)
					{
						using Mat m = new Mat(MaskArea, array[0]);
						using Mat mat = MM.CloneFrom(pfile.MaskParam[num2].Mask1);
						mat.CopyTo(m);
					}
					if (pfile.MaskParam[num2].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num2 < 20)
					{
						using Mat m2 = new Mat(MaskArea, array[1]);
						using Mat mat2 = MM.CloneFrom(pfile.MaskParam[num2].Mask2);
						mat2.CopyTo(m2);
					}
					Cv2.BitwiseNot(MaskArea, MaskArea);
					Cv2.BitwiseAnd(BufferCopy, MaskArea, BufferCopy);
				}
			}
			SearchMatALL = new Mat(BufferCopy, InspeRect);
			if (SearchMatALL.Width < PatternMAT.Width || SearchMatALL.Height < PatternMAT.Height)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，搜尋區域小於設定的字元大小。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，搜寻区域小于设定的字元大小。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー、検索範囲が設定された文字サイズよりも小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Search Area small than setup font size.\n");
				}
				return false;
			}
			FirstMatchALLResult = FindAllMatchText(SearchMatALL, PatternMAT, num);
			if (!FirstMatchALLResult)
			{
				iresult = TOOL_RESULT.EM_NG;
				NGRect.Add(InspeRect);
				return false;
			}
			bool flag = false;
			if (fontToolParam.FontChar1 == "-")
			{
				flag = true;
			}
			FontConnectNG = false;
			if (!PatternRectSeted)
			{
				pfile.FontToolParam.FontIsBlack = (fontToolParam.FontIsBlack = (FontIsBlack = CheckFontIsBlack(PatternMAT)));
				FindMaxFontRect(PatternMAT, fontToolParam.AllFont.Length, fontToolParam.FontAlignVertical, ref PatternRect, ref PatternArea, ref AutoBinaryThd);
				SortFontRect(flag, fontToolParam.FontAlignVertical, fontToolParam.AllFont.Length, ref PatternRect);
				GetMaxWH(flag, fontToolParam.FontAlignVertical, fontToolParam.AllFont.Length, ref PatternRect, ref PatternRectMaxW, ref PatternRectMaxH);
				MinPatternArea = GetMinArea(fontToolParam.AllFont.Length, PatternArea);
				MaxPatternWH = 0;
				for (int k = 0; k < fontToolParam.AllFont.Length; k++)
				{
					if (flag && k == 0)
					{
						continue;
					}
					if (fontToolParam.FontAlignVertical)
					{
						if (PatternRect[k].Height > MaxPatternWH)
						{
							MaxPatternWH = PatternRect[k].Height;
						}
					}
					else if (PatternRect[k].Width > MaxPatternWH)
					{
						MaxPatternWH = PatternRect[k].Width;
					}
				}
				PatternRectSeted = true;
			}
			Rect[] FontRect = new Rect[fontToolParam.AllFont.Length];
			int[] FontArea = new int[fontToolParam.AllFont.Length];
			new Mat(BufferCopy, FoundMatchTextRect).CopyTo(FoundMat);
			if (fontToolParam.FontConnected)
			{
				bool num3 = FindMaxFontRect(FoundMat, fontToolParam.AllFont.Length, fontToolParam.FontAlignVertical, ref FontRect, ref FontArea, ref AutoBinaryThd);
				SortFontRect(flag, fontToolParam.FontAlignVertical, fontToolParam.AllFont.Length, ref FontRect);
				if (!num3 && fontToolParam.ConnectCountAlarm)
				{
					iresult = TOOL_RESULT.EM_NG;
					if (offline)
					{
						if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
						{
							NGRect.Add(FoundMatchTextRect);
						}
						for (int l = 0; l < 5; l++)
						{
							FontBinary[l] = new Mat();
						}
					}
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，字元數量異常，設定{0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文字比对] 发现字符粘黏，字符数量异常，设置{0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字比較] 文字の粘着が検出されました。文字数が異常です。設定: {0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else
					{
						Tracker1ResultText += string.Format("[FontTool] Character adhesion detected. The number of characters is abnormal. Set to {0}. (CT-01)\n", fontToolParam.AllFont.Length);
					}
					FontConnectNG = true;
					return false;
				}
				for (int n = 0; n < fontToolParam.AllFont.Length; n++)
				{
					if ((flag && n == 0) || fontToolParam.AllFont.Length == 9)
					{
						continue;
					}
					if (fontToolParam.FontAlignVertical)
					{
						if (!((double)FontRect[n].Height > (double)MaxPatternWH * 1.5) || !fontToolParam.ConnectWidthAlarm)
						{
							continue;
						}
						iresult = TOOL_RESULT.EM_NG;
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，寬度異常。 (CT-02)\n");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[文本比对] 发现字符粘连，宽度异常。 (CT-02)\n");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字比較] 文字の結合と異常な幅が見つかりました。 (CT-02)\n");
						}
						else
						{
							Tracker1ResultText += string.Format("[FontTool] Character sticking and abnormal width were found. (CT-02)\n");
						}
						if (offline)
						{
							if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
							{
								if (FlipStatus)
								{
									if (FontRect[n].Width > 0 && FontRect[n].Height > 0)
									{
										NGRect.Add(new Rect(FoundMatchTextRect.Right - FontRect[n].Left - FontRect[n].Width, FoundMatchTextRect.Bottom - FontRect[n].Top - FontRect[n].Height, FontRect[n].Width, FontRect[n].Height));
									}
								}
								else if (FontRect[n].Width > 0 && FontRect[n].Height > 0)
								{
									NGRect.Add(new Rect(FoundMatchTextRect.Left + FontRect[n].Left, FoundMatchTextRect.Top + FontRect[n].Top, FontRect[n].Width, FontRect[n].Height));
								}
							}
							for (int num4 = 0; num4 < 5; num4++)
							{
								FontBinary[num4] = new Mat();
							}
						}
						FontConnectNG = true;
						return false;
					}
					if (!((double)FontRect[n].Width > (double)MaxPatternWH * 1.5) || !fontToolParam.ConnectWidthAlarm)
					{
						continue;
					}
					iresult = TOOL_RESULT.EM_NG;
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，寬度異常。 (CT-02)\n");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文本比对] 发现字符粘连，宽度异常。 (CT-02)\n");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字比較] 文字の結合と異常な幅が見つかりました。 (CT-02)\n");
					}
					else
					{
						Tracker1ResultText += string.Format("[FontTool] Character sticking and abnormal width were found. (CT-02)\n");
					}
					if (offline)
					{
						if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
						{
							if (FlipStatus)
							{
								if (FontRect[n].Width > 0 && FontRect[n].Height > 0)
								{
									NGRect.Add(new Rect(FoundMatchTextRect.Right - FontRect[n].Left - FontRect[n].Width, FoundMatchTextRect.Bottom - FontRect[n].Height - FontRect[n].Top, FontRect[n].Width, FontRect[n].Height));
								}
							}
							else if (FontRect[n].Width > 0 && FontRect[n].Height > 0)
							{
								NGRect.Add(new Rect(FoundMatchTextRect.Left + FontRect[n].Left, FoundMatchTextRect.Top + FontRect[n].Top, FontRect[n].Width, FontRect[n].Height));
							}
						}
						for (int num5 = 0; num5 < 5; num5++)
						{
							FontBinary[num5] = new Mat();
						}
					}
					FontConnectNG = true;
					return false;
				}
			}
			string[] array2 = new string[9];
			bool[] array3 = new bool[9];
			for (int num6 = 0; num6 < 9; num6++)
			{
				array2[num6] = "";
				array3[num6] = false;
				FontSearchRect[num6] = new Rect(0, 0, 0, 0);
				for (int num7 = 0; num7 < 5; num7++)
				{
					AIMatchScore[num6][num7] = 0;
				}
			}
			for (int num8 = 0; num8 < fontToolParam.AllFont.Length; num8++)
			{
				array2[num8] = fontToolParam.AllFont.Substring(num8, 1);
				switch (num8)
				{
				case 0:
					array3[num8] = fontToolParam.DoubleWidthFont1;
					break;
				case 1:
					array3[num8] = fontToolParam.DoubleWidthFont2;
					break;
				case 2:
					array3[num8] = fontToolParam.DoubleWidthFont3;
					break;
				case 3:
					array3[num8] = fontToolParam.DoubleWidthFont4;
					break;
				case 4:
					array3[num8] = fontToolParam.DoubleWidthFont5;
					break;
				}
			}
			CalcFontSearchRect(fontToolParam, array2, array3, ref FontSearchRect);
			int[] array4 = new int[9];
			int[] array5 = new int[9];
			array4[0] = fontToolParam.FontIndex1;
			array4[1] = fontToolParam.FontIndex2;
			array4[2] = fontToolParam.FontIndex3;
			array4[3] = fontToolParam.FontIndex4;
			array4[4] = fontToolParam.FontIndex5;
			array5[0] = fontToolParam.MinScore_Font1;
			array5[1] = fontToolParam.MinScore_Font2;
			array5[2] = fontToolParam.MinScore_Font3;
			array5[3] = fontToolParam.MinScore_Font4;
			array5[4] = fontToolParam.MinScore_Font5;
			FontChar[0] = fontToolParam.FontChar1;
			FontChar[1] = fontToolParam.FontChar2;
			FontChar[2] = fontToolParam.FontChar3;
			FontChar[3] = fontToolParam.FontChar4;
			FontChar[4] = fontToolParam.FontChar5;
			ModelName[0] = fontToolParam.AIModel_Font1;
			ModelName[1] = fontToolParam.AIModel_Font2;
			ModelName[2] = fontToolParam.AIModel_Font3;
			ModelName[3] = fontToolParam.AIModel_Font4;
			ModelName[4] = fontToolParam.AIModel_Font5;
			int num9 = 0;
			bool flag2 = true;
			bool flag3 = false;
			bool[] array6 = new bool[9] { fontToolParam.EnableFontCheck1, fontToolParam.EnableFontCheck2, fontToolParam.EnableFontCheck3, fontToolParam.EnableFontCheck4, fontToolParam.EnableFontCheck5, false, false, false, false };
			bool[] array7 = new bool[9] { fontToolParam.BypassOCR1, fontToolParam.BypassOCR2, fontToolParam.BypassOCR3, fontToolParam.BypassOCR4, fontToolParam.BypassOCR5, false, false, false, false };
			for (int num10 = 0; num10 < fontToolParam.AllFont.Length; num10++)
			{
				if (array4[num10] >= 0)
				{
					GetFontPattern(array4[num10], ref FontPattern);
					num9 = 0;
					int num11 = 0;
					Mat mat3 = new Mat();
					int num12 = 0;
					bool flag4 = false;
					string text = "";
					for (int num13 = 0; num13 < 5; num13++)
					{
						PatternMatchScore[num13] = 0;
						if (FontPattern[num13].Width <= 0 || FontPattern[num13].Height <= 0)
						{
							continue;
						}
						if (SearchMat[num10].Width < FontPattern[num13].Width || SearchMat[num10].Height < FontPattern[num13].Height)
						{
							flag3 = true;
							text = ((buffers.LanguageType != 1) ? ((buffers.LanguageType != 2) ? ((buffers.LanguageType != 3) ? (text + string.Format("[Font Catch] Font[{0}]Check Fail, search area small than font pattern)。\n", FontChar[num10])) : (text + string.Format("[文字抽出] 文字[{0}]検出失敗、検索領域が文字サイズより小さいです。\n", FontChar[num10]))) : (text + string.Format("[抓取字元] 字元[{0}]检测失败，搜寻区域小于文字大小。\n", FontChar[num10]))) : (text + string.Format("[抓取字元] 字元[{0}]檢測失敗，搜尋區域小於文字大小。\n", FontChar[num10])));
							continue;
						}
						PatternMatchScore[num13] = GetOpenCVMatchScore(SearchMat[num10], FontPattern[num13], out PatternMatchRect[num13]);
						if (PatternMatchRect[num13].Width <= 0 && PatternMatchRect[num13].Height <= 0)
						{
							num13 = 5;
						}
						else if (num12 == 0 || PatternMatchScore[num13] > num12)
						{
							mat3 = new Mat(SearchMat[num10], PatternMatchRect[num13]);
							FontMatchRect[num10] = PatternMatchRect[num13];
							FontMatchRect[num10].Left += FontSearchRect[num10].Left;
							FontMatchRect[num10].Top += FontSearchRect[num10].Top;
							num12 = PatternMatchScore[num13];
							num11 = num13;
							flag4 = true;
						}
					}
					if (!flag4)
					{
						if (text == "")
						{
							text = ((buffers.LanguageType == 1) ? (text + string.Format("[抓取字元] 字元[{0}] 尚未設定文字資料庫。\n", FontChar[num10])) : ((buffers.LanguageType == 2) ? (text + string.Format("[抓取字元] 字元[{0}] 尚未设定文字资料库。\n", FontChar[num10])) : ((buffers.LanguageType != 3) ? (text + string.Format("[Font Catch] Font[{0}]Check Fail, Please setup font pattern first)。\n", FontChar[num10])) : (text + string.Format("[文字抽出] 文字[{0}] はまだ文字データベースが設定されていません。\n", FontChar[num10])))));
						}
						Tracker1ResultText += text;
						iresult = TOOL_RESULT.EM_NG;
						return false;
					}
					string str = "";
					mat3.CopyTo(OcrMat1[num10]);
					FontPattern[num11].CopyTo(OcrMat2[num10]);
					if (fontToolParam.FontAlignVertical)
					{
						Cv2.Transpose(OcrMat1[num10], OcrMat1[num10]);
						Cv2.Flip(OcrMat1[num10], OcrMat1[num10], FlipMode.Y);
						Cv2.Transpose(OcrMat2[num10], OcrMat2[num10]);
						Cv2.Flip(OcrMat2[num10], OcrMat2[num10], FlipMode.Y);
					}
					OcrMat1[num10].CopyTo(FontBinary[num10]);
					if (fontToolParam.EnabledOCR && !array7[num10])
					{
						string ocrChar = "";
						OcrMat1[num10].CopyTo(OcrFinalMat);
						if (FontIsBlack)
						{
							Cv2.BitwiseNot(OcrFinalMat, OcrFinalMat);
						}
						OCRResult[num10] = GetAIOCRResult(OcrFinalMat, ref ocrChar);
						string fontCharFromIndex = GetFontCharFromIndex(array4[num10]);
						if (ocrChar != fontCharFromIndex)
						{
							bool flag5 = false;
							if ((ocrChar == "1" && fontCharFromIndex == "-") || (ocrChar == "-" && fontCharFromIndex == "1"))
							{
								flag5 = true;
								ocrChar = fontCharFromIndex;
							}
							else if (ocrChar == "1" && fontCharFromIndex == "7")
							{
								Mat[] pattern = new Mat[5];
								GetFontPattern(6, ref pattern);
								if (pattern[num11].Width <= 0 || pattern[num11].Height <= 0)
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else if (Math.Abs(OcrMat1[num10].Width - OcrMat2[num10].Width) < Math.Abs(pattern[num11].Width - OcrMat2[num10].Width))
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else
								{
									flag5 = false;
								}
							}
							else if (ocrChar == "7" && fontCharFromIndex == "1")
							{
								Mat[] pattern2 = new Mat[5];
								GetFontPattern(12, ref pattern2);
								if (pattern2[num11].Width <= 0 || pattern2[num11].Height <= 0)
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else if (Math.Abs(OcrMat1[num10].Width - OcrMat2[num10].Width) < Math.Abs(pattern2[num11].Width - OcrMat2[num10].Width))
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else
								{
									flag5 = false;
								}
							}
							else if ((ocrChar == "0" && fontCharFromIndex == "O") || (ocrChar == "O" && fontCharFromIndex == "0"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "0" && fontCharFromIndex == "D") || (ocrChar == "D" && fontCharFromIndex == "0"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "8" && fontCharFromIndex == "B") || (ocrChar == "B" && fontCharFromIndex == "8"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "R" && fontCharFromIndex == "A") || (ocrChar == "A" && fontCharFromIndex == "R"))
							{
								flag5 = true;
							}
							if (!flag5)
							{
								if (ocrChar == "1" || ocrChar == "-")
								{
									if (buffers.LanguageType == 1)
									{
										Tracker1ResultText += string.Format("[OCR文字比對] 字元[{0}]檢測失敗， 找不到符合字元。\n", FontChar[num10]);
									}
									else if (buffers.LanguageType == 2)
									{
										Tracker1ResultText += string.Format("[OCR文字比对] 字元[{0}]检测失败， 找不到符合字元。\n", FontChar[num10]);
									}
									else if (buffers.LanguageType == 3)
									{
										Tracker1ResultText += string.Format("[OCR文字照合] 文字[{0}]検出失敗、 一致する文字が見つかりません。\n", FontChar[num10]);
									}
									else
									{
										Tracker1ResultText += string.Format("[OCR Check] Font [{0}] OCR Check error, can't find match font.\n", FontChar[num10]);
									}
								}
								else if (buffers.LanguageType == 1)
								{
									Tracker1ResultText += string.Format("[OCR文字比對] 字元[{0}]檢測失敗， 找到字元為{1}。\n", FontChar[num10], ocrChar);
								}
								else if (buffers.LanguageType == 2)
								{
									Tracker1ResultText += string.Format("[OCR文字比对] 字元[{0}]检测失败， 找到字元为{1}。\n", FontChar[num10], ocrChar);
								}
								else if (buffers.LanguageType == 3)
								{
									Tracker1ResultText += string.Format("[OCR文字照合] 文字[{0}]検出失敗、検出された文字は{1}です。\n", FontChar[num10], ocrChar);
								}
								else
								{
									Tracker1ResultText += string.Format("[OCR Check] Font [{0}]OCR Check error, found [{1}]\n", FontChar[num10], ocrChar);
								}
								OCRResult[num10] = false;
								if (offline && FontMatchRect[num10].Width > 0 && FontMatchRect[num10].Height > 0)
								{
									if (!FlipStatus)
									{
										NGRect.Add(FontMatchRect[num10]);
									}
									else
									{
										Rect item = new Rect(FontMatchRect[num10].Left - FoundMatchTextRect.Left, FontMatchRect[num10].Top - FoundMatchTextRect.Top, FontMatchRect[num10].Width, FontMatchRect[num10].Height);
										item.Left = FoundMatchTextRect.Right - item.Left - item.Width;
										item.Top = FoundMatchTextRect.Bottom - item.Top - item.Height;
										NGRect.Add(item);
									}
								}
							}
						}
					}
					if (array6[num10])
					{
						OcrMat1[num10].CopyTo(CheckFontMat[num10]);
						if (FontIsBlack)
						{
							Cv2.BitwiseNot(CheckFontMat[num10], CheckFontMat[num10]);
							Cv2.BitwiseNot(OcrMat2[num10], OcrMat2[num10]);
						}
						AdvFontCheck[num10] = Check_Font(array4[num10], CheckFontMat[num10], OcrMat2[num10], ref str, ref FontBinary[num10], num10);
						if (offline && !AdvFontCheck[num10] && FontMatchRect[num10].Width > 0 && FontMatchRect[num10].Height > 0)
						{
							if (!FlipStatus)
							{
								NGRect.Add(FontMatchRect[num10]);
							}
							else
							{
								Rect item2 = new Rect(FontMatchRect[num10].Left - FoundMatchTextRect.Left, FontMatchRect[num10].Top - FoundMatchTextRect.Top, FontMatchRect[num10].Width, FontMatchRect[num10].Height);
								item2.Left = FoundMatchTextRect.Right - item2.Left - item2.Width;
								item2.Top = FoundMatchTextRect.Bottom - item2.Top - item2.Height;
								NGRect.Add(item2);
							}
						}
						Tracker1ResultText += str;
					}
					if (mat3.Width > 0 && mat3.Height > 0)
					{
						FoundFontMat[num10] = mat3;
						OcrMat1[num10].CopyTo(AIMatchTempMat[num10]);
						num9++;
						if (ModelName[num10] != "")
						{
							GetAIMatchScore(ModelName[num10], num10, AIMatchTempMat[num10], FontPattern[num11], ref PatternMatchScore[num11]);
						}
						else
						{
							new Rect(0, 0, 0, 0);
							PatternMatchScore[num11] = GetOpenCVMatchScore(SearchMat[num10], FontPattern[num11], out PatternMatchRect[num11]);
						}
						AIMatchScore[num10][num11] = PatternMatchScore[num11];
						FontMatchScore[num10] = PatternMatchScore[num11];
						if (FontMatchScore[num10] > 0 && FontMatchScore[num10] >= array5[num10])
						{
							FontResult[num10] = true;
							ScoreResult[num10] = true;
							if (!AdvFontCheck[num10])
							{
								FontResult[num10] = false;
								flag2 = false;
							}
							if (!OCRResult[num10])
							{
								FontResult[num10] = false;
								flag2 = false;
							}
							continue;
						}
						flag2 = false;
						ScoreResult[num10] = false;
						if (FontMatchRect[num10].Width > 0 && FontMatchRect[num10].Height > 0)
						{
							if (!FlipStatus)
							{
								NGRect.Add(FontMatchRect[num10]);
							}
							else
							{
								Rect item3 = new Rect(FontMatchRect[num10].Left - FoundMatchTextRect.Left, FontMatchRect[num10].Top - FoundMatchTextRect.Top, FontMatchRect[num10].Width, FontMatchRect[num10].Height);
								item3.Left = FoundMatchTextRect.Right - item3.Left - item3.Width;
								item3.Top = FoundMatchTextRect.Bottom - item3.Top - item3.Height;
								NGRect.Add(item3);
							}
						}
						if (num9 <= 0 && !flag3)
						{
							if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[抓取字元] 字元[{0}]檢測失敗， 尚未設定檢測字型資料庫。\n", FontChar[num10]);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText += string.Format("[抓取字元] 字元[{0}]检测失败， 尚未设定检测字型资料库。\n", FontChar[num10]);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText += string.Format("[文字抽出] 文字[{0}]の検出に失敗しました。検出フォントデータベースが設定されていません。\n", FontChar[num10]);
							}
							else
							{
								Tracker1ResultText += string.Format("[Font Catch] Font[{0}]Check Fail, font Database count is 0)。\n", FontChar[num10]);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[字元相似度] 字元[{0}]檢測失敗， 檢測分數 = {1}, 小於設定值({2})。\n", FontChar[num10], FontMatchScore[num10], array5[num10]);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[字元相似度] 字元[{0}]检测失败， 检测分数 = {1}, 小于设定值({2})。\n", FontChar[num10], FontMatchScore[num10], array5[num10]);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字類似度] 文字[{0}]検出に失敗しました。検出スコア = {1} で、設定値({2})より小さいです。\n", FontChar[num10], FontMatchScore[num10], array5[num10]);
						}
						else
						{
							Tracker1ResultText += string.Format("[Font Score Check] Font[{0}]Check Fail, Similarity Score = {1}, less than setup({2})。\n", FontChar[num10], FontMatchScore[num10], array5[num10]);
						}
					}
					else
					{
						FoundFontMat[num10] = new Mat();
					}
				}
				else
				{
					FontResult[num10] = true;
				}
			}
			if (!flag2)
			{
				iresult = TOOL_RESULT.EM_NG;
			}
			return flag2;
		}
		catch (Exception ex)
		{
			string text2 = "MAT02-FontTool-Inspect()-";
			text2 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-002");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool InspectF8(ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		FirstMatchALLResult = true;
		MatchSuccess = true;
		FlipStatus = false;
		Tracker1ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		NGRect.Clear();
		INSP_FONTTOOL_PARAM fontToolParam = pfile.FontToolParam;
		if (pfile.FontToolParam.TextMat == null)
		{
			pfile.FontToolParam.TextMat = new Mat();
		}
		for (int i = 0; i < 5; i++)
		{
			PatternMatchScore[i] = 0;
			PatternMatchRect[i] = new Rect(0, 0, 0, 0);
		}
		for (int j = 0; j < 9; j++)
		{
			FontMatchScore[j] = 0;
			FontResult[j] = false;
			FontMatchRect[j] = new Rect(0, 0, 0, 0);
			FontSearchRect[j] = new Rect(0, 0, 0, 0);
			AdvFontCheck[j] = true;
			OCRResult[j] = true;
			ScoreResult[j] = true;
		}
		try
		{
			if (FaceIndex != 0 && pfile.FontToolParam.EnableSelectFace && FaceIndex != pfile.FontToolParam.FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			if (fontToolParam.PatternSelect < 0 || fontToolParam.PatternSelect >= 210)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 圖形共用資料庫索引錯誤({0})，請重新選擇比對圖形。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 图形共用资料库索引错误({0})，请重新选择比对图形。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字照合] 画像共有データベースインデックスエラー({0})、照合する画像を再度選択してください。\n", fontToolParam.PatternSelect + 1);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Tool] Public Pattern file index Error({0}), Select Pattern file again.\n", fontToolParam.PatternSelect + 1);
				}
				return false;
			}
			string arg = "";
			if (fontToolParam.PublicDBUsePublicMode)
			{
				if (!fontToolParam.AutoCombineFontCreate && fontToolParam.AutoCombineFontToDBSet && fontToolParam.doAutoCombineFontToDB && fontToolParam.AllFont.Length == 9)
				{
					for (int k = 0; k < fontToolParam.AllFont.Length; k++)
					{
						string str = fontToolParam.AllFont.Substring(k, 1);
						Mat[] pattern = new Mat[5];
						GetFontPattern(GetFontIndex(str), ref pattern);
						AllFontPattern[k] = pattern[0];
					}
					string CombineError = "";
					if (!AutoCombineFont(fontToolParam, ref CombineError))
					{
						iresult = TOOL_RESULT.EM_NG;
						FirstMatchALLResult = false;
						MatchSuccess = false;
						Tracker1ResultText += CombineError;
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[文字比對] 自動拼字功能錯誤，請重新檢查字元設定。\n");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[文字比对] [文字比对] 自动拼字功能错误，请重新检查字元设定。\n");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字照合] 自動スペルチェック機能にエラーがあります。文字設定を再度確認してください。\n");
						}
						else
						{
							Tracker1ResultText += string.Format("[Font Tool] AutoCombine Error, Please check font setting again.\n");
						}
						return false;
					}
					pfile.FontToolParam.AutoCombineFontCreate = true;
					pfile.PublicPatternDB[208].PatternImage.CopyTo(PatternMAT);
				}
				else
				{
					pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternImage.CopyTo(PatternMAT);
					pfile.FontToolParam.PatternChannel = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternChannel;
					_ = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternON;
					arg = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternName;
				}
			}
			else if (!fontToolParam.AutoCombineFontCreate && fontToolParam.AutoCombineFontToDBSet && fontToolParam.doAutoCombineFontToDB && fontToolParam.AllFont.Length == 9)
			{
				for (int l = 0; l < fontToolParam.AllFont.Length; l++)
				{
					string str2 = fontToolParam.AllFont.Substring(l, 1);
					Mat[] pattern2 = new Mat[5];
					GetFontPattern(GetFontIndex(str2), ref pattern2);
					AllFontPattern[l] = pattern2[0];
				}
				string CombineError2 = "";
				if (!AutoCombineFont(fontToolParam, ref CombineError2))
				{
					iresult = TOOL_RESULT.EM_NG;
					FirstMatchALLResult = false;
					MatchSuccess = false;
					Tracker1ResultText += CombineError2;
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 自動拼字功能錯誤，請重新檢查字元設定。\n");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文字比对] [文字比对] 自动拼字功能错误，请重新检查字元设定。\n");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字照合] 自動スペルチェック機能にエラーがあります。文字設定を再度確認してください。\n");
					}
					else
					{
						Tracker1ResultText += string.Format("[Font Tool] AutoCombine Error, Please check font setting again.\n");
					}
					return false;
				}
				pfile.FontToolParam.AutoCombineFontCreate = true;
				pfile.PrivatePatternDB[208].PatternImage.CopyTo(PatternMAT);
			}
			else
			{
				pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternImage.CopyTo(PatternMAT);
				pfile.FontToolParam.PatternChannel = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternChannel;
				_ = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternON;
				arg = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternName;
			}
			buffers.GetChannelMat(fontToolParam.PatternChannel).CopyTo(BufferCopy);
			GetInspeRect();
			if (PatternMAT.Width <= 0 || PatternMAT.Height <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 圖形共用資料庫({0})影像遺失或檔案損毀。\n", arg);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 图形共用资料库({0})影像遗失或档案损毁。\n", arg);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 画像共有データベース({0})の画像が失われたか、ファイルが破損しています。\n", arg);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Tool]Public Image DB Image Error({0}).\n", arg);
				}
			}
			int num = 0;
			num = fontToolParam.PatternChannel;
			InspeRect &= new Rect(0, 0, buffers.GetChannelMat(num).Cols, buffers.GetChannelMat(num).Rows);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，搜尋區域寬高小於0。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，搜寻区域宽高小于0。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー: 検索領域の幅と高さが 0 より小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Error，Search area is 0.\n");
				}
				return false;
			}
			if (fontToolParam.AllFont.Length <= 0 || fontToolParam.AllFont.Length > 9)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，尚未設定搜尋字元。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，尚未设定搜寻字元。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー、検索文字が設定されていません。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Setup Find char first.\n");
				}
				return false;
			}
			if (fontToolParam.MaskSelectIndex > 0)
			{
				int num2 = fontToolParam.MaskSelectIndex - 1;
				if (!pfile.MaskParam[num2].TrackerON)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("字元使用遮罩工具，但遮罩工具[{0}]未開啟! \n", num2 + 1);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("字元使用遮罩工具，但遮罩工具[{0}]未开启! \n", num2 + 1);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("文字はマスクツールを使用していますが、マスクツール[{0}]は有効になっていません! \n", num2 + 1);
					}
					else
					{
						Tracker1ResultText += string.Format("Font Tool Link Error, Mask Tool [{0}] has been Disable! \n", num2 + 1);
					}
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
				buffers.GetChannelMat(0).CopyTo(MaskArea);
				MaskArea.SetTo(0.0);
				Rect[] array = new Rect[2];
				if (num2 >= 0)
				{
					array[0] = new Rect(pfile.MaskParam[num2].Mask1Pt.X, pfile.MaskParam[num2].Mask1Pt.Y, pfile.MaskParam[num2].Mask1.Width, pfile.MaskParam[num2].Mask1.Height);
					array[0] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
					if (pfile.MaskParam[num2].SearchRect2ON && num2 < 20)
					{
						array[1] = new Rect(pfile.MaskParam[num2].Mask2Pt.X, pfile.MaskParam[num2].Mask2Pt.Y, pfile.MaskParam[num2].Mask2.Width, pfile.MaskParam[num2].Mask2.Height);
						array[1] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
					}
					else
					{
						array[1] = new Rect(0, 0, 0, 0);
					}
					if (array[0].Width > 0 && array[0].Height > 0)
					{
						using Mat m = new Mat(MaskArea, array[0]);
						using Mat mat = MM.CloneFrom(pfile.MaskParam[num2].Mask1);
						mat.CopyTo(m);
					}
					if (pfile.MaskParam[num2].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num2 < 20)
					{
						using Mat m2 = new Mat(MaskArea, array[1]);
						using Mat mat2 = MM.CloneFrom(pfile.MaskParam[num2].Mask2);
						mat2.CopyTo(m2);
					}
					Cv2.BitwiseNot(MaskArea, MaskArea);
					Cv2.BitwiseAnd(BufferCopy, MaskArea, BufferCopy);
				}
			}
			SearchMatALL = new Mat(BufferCopy, InspeRect);
			if (SearchMatALL.Width < PatternMAT.Width || SearchMatALL.Height < PatternMAT.Height)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，搜尋區域小於設定的字元大小。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，搜寻区域小于设定的字元大小。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー、検索範囲が設定された文字サイズよりも小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Search Area small than setup font size.\n");
				}
				return false;
			}
			FirstMatchALLResult = FindAllMatchText(SearchMatALL, PatternMAT, num);
			if (!FirstMatchALLResult)
			{
				iresult = TOOL_RESULT.EM_NG;
				NGRect.Add(InspeRect);
				return false;
			}
			bool flag = false;
			if (fontToolParam.FontChar1 == "-")
			{
				flag = true;
			}
			FontConnectNG = false;
			if (!PatternRectSeted)
			{
				pfile.FontToolParam.FontIsBlack = (FontIsBlack = (fontToolParam.FontIsBlack = CheckFontIsBlack(PatternMAT)));
				FindMaxFontRect(PatternMAT, fontToolParam.AllFont.Length, fontToolParam.FontAlignVertical, ref PatternRect, ref PatternArea, ref AutoBinaryThd);
				Sort8FontRect(ref PatternRect, fontToolParam.AllFont.Length);
				GetMaxWH(flag, fontToolParam.FontAlignVertical, fontToolParam.AllFont.Length, ref PatternRect, ref PatternRectMaxW, ref PatternRectMaxH);
				MinPatternArea = GetMinArea(fontToolParam.AllFont.Length, PatternArea);
				MaxPatternWH = 0;
				for (int n = 0; n < fontToolParam.AllFont.Length; n++)
				{
					if ((!flag || n != 0) && PatternRect[n].Width > MaxPatternWH)
					{
						MaxPatternWH = PatternRect[n].Width;
					}
				}
				PatternRectSeted = true;
			}
			Rect[] FontRect = new Rect[fontToolParam.AllFont.Length];
			int[] FontArea = new int[fontToolParam.AllFont.Length];
			new Mat(BufferCopy, FoundMatchTextRect).CopyTo(FoundMat);
			if (fontToolParam.FontConnected)
			{
				bool num3 = FindMaxFontRect(FoundMat, fontToolParam.AllFont.Length, fontToolParam.FontAlignVertical, ref FontRect, ref FontArea, ref AutoBinaryThd);
				Sort8FontRect(ref FontRect, fontToolParam.AllFont.Length);
				if (!num3 && fontToolParam.ConnectCountAlarm)
				{
					iresult = TOOL_RESULT.EM_NG;
					if (offline)
					{
						if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
						{
							NGRect.Add(FoundMatchTextRect);
						}
						for (int num4 = 0; num4 < 5; num4++)
						{
							FontBinary[num4] = new Mat();
						}
					}
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，字元數量異常，設定{0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文字比对] 发现字符粘黏，字符数量异常，设置{0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字比較] 文字の粘着が検出されました。文字数が異常です。設定: {0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else
					{
						Tracker1ResultText += string.Format("[FontTool] Character adhesion detected. The number of characters is abnormal. Set to {0}. (CT-01)\n", fontToolParam.AllFont.Length);
					}
					FontConnectNG = true;
					return false;
				}
				for (int num5 = 0; num5 < fontToolParam.AllFont.Length; num5++)
				{
					if (flag && num5 == 0)
					{
						continue;
					}
					if ((double)FontRect[num5].Width > (double)MaxPatternWH * 1.5 && fontToolParam.ConnectWidthAlarm)
					{
						iresult = TOOL_RESULT.EM_NG;
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，寬度異常。 (CT-02)\n");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[文本比对] 发现字符粘连，宽度异常。 (CT-02)\n");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字比較] 文字の結合と異常な幅が見つかりました。 (CT-02)\n");
						}
						else
						{
							Tracker1ResultText += string.Format("[FontTool] Character sticking and abnormal width were found. (CT-02)\n");
						}
						if (offline)
						{
							if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
							{
								if (FlipStatus)
								{
									if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
									{
										NGRect.Add(new Rect(FoundMatchTextRect.Right - FontRect[num5].Left - FontRect[num5].Width, FoundMatchTextRect.Bottom - FontRect[num5].Height - FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
									}
								}
								else if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
								{
									NGRect.Add(new Rect(FoundMatchTextRect.Left + FontRect[num5].Left, FoundMatchTextRect.Top + FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
								}
							}
							for (int num6 = 0; num6 < 5; num6++)
							{
								FontBinary[num6] = new Mat();
							}
						}
						FontConnectNG = true;
						return false;
					}
					if (!((double)FontRect[num5].Height > (double)MaxPatternWH * 1.5) || !fontToolParam.ConnectWidthAlarm)
					{
						continue;
					}
					iresult = TOOL_RESULT.EM_NG;
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，高度異常。 (CT-02)\n");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文本比对] 发现字符粘连，高度异常。 (CT-02)\n");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字比較] 文字の結合と異常な高が見つかりました。 (CT-02)\n");
					}
					else
					{
						Tracker1ResultText += string.Format("[FontTool] Character sticking and abnormal width were found. (CT-02)\n");
					}
					if (offline)
					{
						if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
						{
							if (FlipStatus)
							{
								if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
								{
									NGRect.Add(new Rect(FoundMatchTextRect.Right - FontRect[num5].Left - FontRect[num5].Width, FoundMatchTextRect.Bottom - FontRect[num5].Height - FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
								}
							}
							else if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
							{
								NGRect.Add(new Rect(FoundMatchTextRect.Left + FontRect[num5].Left, FoundMatchTextRect.Top + FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
							}
						}
						for (int num7 = 0; num7 < 5; num7++)
						{
							FontBinary[num7] = new Mat();
						}
					}
					FontConnectNG = true;
					return false;
				}
			}
			string[] array2 = new string[9];
			bool[] array3 = new bool[9];
			for (int num8 = 0; num8 < 9; num8++)
			{
				array2[num8] = "";
				array3[num8] = false;
				FontSearchRect[num8] = new Rect(0, 0, 0, 0);
				for (int num9 = 0; num9 < 5; num9++)
				{
					AIMatchScore[num8][num9] = 0;
				}
			}
			for (int num10 = 0; num10 < fontToolParam.AllFont.Length; num10++)
			{
				array2[num10] = fontToolParam.AllFont.Substring(num10, 1);
			}
			Calc8FontSearchRect(fontToolParam, array2, ref FontSearchRect);
			int[] array4 = new int[9];
			int[] array5 = new int[9];
			array4[0] = fontToolParam.FontIndex1;
			array4[1] = fontToolParam.FontIndex2;
			array4[2] = fontToolParam.FontIndex3;
			array4[3] = fontToolParam.FontIndex4;
			array4[4] = fontToolParam.FontIndex5;
			array4[5] = fontToolParam.FontIndex6;
			array4[6] = fontToolParam.FontIndex7;
			array4[7] = fontToolParam.FontIndex8;
			array5[0] = fontToolParam.MinScore_Font1;
			array5[1] = fontToolParam.MinScore_Font2;
			array5[2] = fontToolParam.MinScore_Font3;
			array5[3] = fontToolParam.MinScore_Font4;
			array5[4] = fontToolParam.MinScore_Font5;
			array5[5] = fontToolParam.MinScore_Font6;
			array5[6] = fontToolParam.MinScore_Font7;
			array5[7] = fontToolParam.MinScore_Font8;
			FontChar[0] = fontToolParam.FontChar1;
			FontChar[1] = fontToolParam.FontChar2;
			FontChar[2] = fontToolParam.FontChar3;
			FontChar[3] = fontToolParam.FontChar4;
			FontChar[4] = fontToolParam.FontChar5;
			FontChar[5] = fontToolParam.FontChar6;
			FontChar[6] = fontToolParam.FontChar7;
			FontChar[7] = fontToolParam.FontChar8;
			ModelName[0] = fontToolParam.AIModel_Font1;
			ModelName[1] = fontToolParam.AIModel_Font2;
			ModelName[2] = fontToolParam.AIModel_Font3;
			ModelName[3] = fontToolParam.AIModel_Font4;
			ModelName[4] = fontToolParam.AIModel_Font5;
			ModelName[5] = fontToolParam.AIModel_Font6;
			ModelName[6] = fontToolParam.AIModel_Font7;
			ModelName[7] = fontToolParam.AIModel_Font8;
			int num11 = 0;
			bool flag2 = true;
			bool flag3 = false;
			bool[] array6 = new bool[9] { fontToolParam.EnableFontCheck1, fontToolParam.EnableFontCheck2, fontToolParam.EnableFontCheck3, fontToolParam.EnableFontCheck4, fontToolParam.EnableFontCheck5, fontToolParam.EnableFontCheck6, fontToolParam.EnableFontCheck7, fontToolParam.EnableFontCheck8, false };
			bool[] array7 = new bool[9] { fontToolParam.BypassOCR1, fontToolParam.BypassOCR2, fontToolParam.BypassOCR3, fontToolParam.BypassOCR4, fontToolParam.BypassOCR5, fontToolParam.BypassOCR6, fontToolParam.BypassOCR7, fontToolParam.BypassOCR8, false };
			for (int num12 = 0; num12 < fontToolParam.AllFont.Length; num12++)
			{
				if (array4[num12] >= 0)
				{
					FontResult[num12] = false;
					AdvFontCheck[num12] = true;
					OCRResult[num12] = true;
					ScoreResult[num12] = true;
					GetFontPattern(array4[num12], ref FontPattern);
					num11 = 0;
					int num13 = 0;
					Mat mat3 = new Mat();
					int num14 = 0;
					string text = "";
					bool flag4 = false;
					for (int num15 = 0; num15 < 5; num15++)
					{
						PatternMatchScore[num15] = 0;
						if (FontPattern[num15].Width <= 0 || FontPattern[num15].Height <= 0)
						{
							continue;
						}
						if (SearchMat[num12].Width < FontPattern[num15].Width || SearchMat[num12].Height < FontPattern[num15].Height)
						{
							flag3 = true;
							text = ((buffers.LanguageType != 1) ? ((buffers.LanguageType != 2) ? ((buffers.LanguageType != 3) ? (text + string.Format("[Font Catch] Font[{0}]Check Fail, search area small than font pattern)。\n", FontChar[num12])) : (text + string.Format("[文字抽出] 文字[{0}]検出失敗、検索領域が文字サイズより小さいです。\n", FontChar[num12]))) : (text + string.Format("[抓取字元] 字元[{0}]检测失败，搜寻区域小于文字大小。\n", FontChar[num12]))) : (text + string.Format("[抓取字元] 字元[{0}]檢測失敗，搜尋區域小於文字大小。\n", FontChar[num12])));
							continue;
						}
						PatternMatchScore[num15] = GetOpenCVMatchScore(SearchMat[num12], FontPattern[num15], out PatternMatchRect[num15]);
						if (PatternMatchRect[num15].Width <= 0 && PatternMatchRect[num15].Height <= 0)
						{
							num15 = 5;
						}
						else if (num14 == 0 || PatternMatchScore[num15] > num14)
						{
							mat3 = new Mat(SearchMat[num12], PatternMatchRect[num15]);
							FontMatchRect[num12] = PatternMatchRect[num15];
							FontMatchRect[num12].Left += FontSearchRect[num12].Left;
							FontMatchRect[num12].Top += FontSearchRect[num12].Top;
							AllFontPattern[num12] = FontPattern[num15];
							num14 = PatternMatchScore[num15];
							flag4 = true;
							num13 = num15;
						}
					}
					if (!flag4)
					{
						Tracker1ResultText += text;
						iresult = TOOL_RESULT.EM_NG;
						return false;
					}
					mat3.CopyTo(OcrMat1[num12]);
					FontPattern[num13].CopyTo(OcrMat2[num12]);
					if (fontToolParam.FontAlignVertical)
					{
						Cv2.Transpose(OcrMat1[num12], OcrMat1[num12]);
						Cv2.Flip(OcrMat1[num12], OcrMat1[num12], FlipMode.Y);
						Cv2.Transpose(OcrMat2[num12], OcrMat2[num12]);
						Cv2.Flip(OcrMat2[num12], OcrMat2[num12], FlipMode.Y);
					}
					OcrMat1[num12].CopyTo(FontBinary[num12]);
					if (fontToolParam.EnabledOCR && !array7[num12])
					{
						string ocrChar = "";
						OcrMat1[num12].CopyTo(OcrFinalMat);
						if (FontIsBlack)
						{
							Cv2.BitwiseNot(OcrFinalMat, OcrFinalMat);
						}
						OCRResult[num12] = GetAIOCRResult(OcrFinalMat, ref ocrChar);
						string fontCharFromIndex = GetFontCharFromIndex(array4[num12]);
						if (ocrChar != fontCharFromIndex)
						{
							bool flag5 = false;
							if ((ocrChar == "1" && fontCharFromIndex == "-") || (ocrChar == "-" && fontCharFromIndex == "1"))
							{
								flag5 = true;
								ocrChar = fontCharFromIndex;
							}
							else if (ocrChar == "1" && fontCharFromIndex == "7")
							{
								Mat[] pattern3 = new Mat[5];
								GetFontPattern(6, ref pattern3);
								if (pattern3[num13].Width <= 0 || pattern3[num13].Height <= 0)
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else if (Math.Abs(OcrMat1[num12].Width - OcrMat2[num12].Width) < Math.Abs(pattern3[num13].Width - OcrMat2[num12].Width))
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else
								{
									flag5 = false;
								}
							}
							else if (ocrChar == "7" && fontCharFromIndex == "1")
							{
								Mat[] pattern4 = new Mat[5];
								GetFontPattern(12, ref pattern4);
								if (pattern4[num13].Width <= 0 || pattern4[num13].Height <= 0)
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else if (Math.Abs(OcrMat1[num12].Width - OcrMat2[num12].Width) < Math.Abs(pattern4[num13].Width - OcrMat2[num12].Width))
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else
								{
									flag5 = false;
								}
							}
							else if ((ocrChar == "0" && fontCharFromIndex == "O") || (ocrChar == "O" && fontCharFromIndex == "0"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "0" && fontCharFromIndex == "D") || (ocrChar == "D" && fontCharFromIndex == "0"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "8" && fontCharFromIndex == "B") || (ocrChar == "B" && fontCharFromIndex == "8"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "R" && fontCharFromIndex == "A") || (ocrChar == "A" && fontCharFromIndex == "R"))
							{
								flag5 = true;
							}
							if (!flag5)
							{
								if (ocrChar == "1" || ocrChar == "-")
								{
									if (buffers.LanguageType == 1)
									{
										Tracker1ResultText += string.Format("[OCR文字比對] 字元[{0}]檢測失敗， 找不到符合字元。\n", FontChar[num12]);
									}
									else if (buffers.LanguageType == 2)
									{
										Tracker1ResultText += string.Format("[OCR文字比对] 字元[{0}]检测失败， 找不到符合字元。\n", FontChar[num12]);
									}
									else if (buffers.LanguageType == 3)
									{
										Tracker1ResultText += string.Format("[OCR文字照合] 文字[{0}]検出失敗、 一致する文字が見つかりません。\n", FontChar[num12]);
									}
									else
									{
										Tracker1ResultText += string.Format("[OCR Check] Font [{0}] OCR Check error, can't find match font.\n", FontChar[num12]);
									}
								}
								else if (buffers.LanguageType == 1)
								{
									Tracker1ResultText += string.Format("[OCR文字比對] 字元[{0}]檢測失敗， 找到字元為{1}。\n", FontChar[num12], ocrChar);
								}
								else if (buffers.LanguageType == 2)
								{
									Tracker1ResultText += string.Format("[OCR文字比对] 字元[{0}]检测失败， 找到字元为{1}。\n", FontChar[num12], ocrChar);
								}
								else if (buffers.LanguageType == 3)
								{
									Tracker1ResultText += string.Format("[OCR文字照合] 文字[{0}]検出失敗、検出された文字は{1}です。\n", FontChar[num12], ocrChar);
								}
								else
								{
									Tracker1ResultText += string.Format("[OCR Check] Font [{0}]OCR Check error, found [{1}]\n", FontChar[num12], ocrChar);
								}
								OCRResult[num12] = false;
								if (offline && FontMatchRect[num12].Width > 0 && FontMatchRect[num12].Height > 0)
								{
									if (!FlipStatus)
									{
										NGRect.Add(FontMatchRect[num12]);
									}
									else
									{
										Rect item = new Rect(FontMatchRect[num12].Left - FoundMatchTextRect.Left, FontMatchRect[num12].Top - FoundMatchTextRect.Top, FontMatchRect[num12].Width, FontMatchRect[num12].Height);
										item.Left = FoundMatchTextRect.Right - item.Left - item.Width;
										item.Top = FoundMatchTextRect.Bottom - item.Top - item.Height;
										NGRect.Add(item);
									}
								}
							}
						}
					}
					if (array6[num12])
					{
						string str3 = "";
						AdvFontCheck[num12] = Check_Font(array4[num12], OcrMat1[num12], OcrMat2[num12], ref str3, ref FontBinary[num12], num12);
						if (offline && !AdvFontCheck[num12] && FontMatchRect[num12].Width > 0 && FontMatchRect[num12].Height > 0)
						{
							if (!FlipStatus)
							{
								NGRect.Add(FontMatchRect[num12]);
							}
							else
							{
								Rect item2 = new Rect(FontMatchRect[num12].Left - FoundMatchTextRect.Left, FontMatchRect[num12].Top - FoundMatchTextRect.Top, FontMatchRect[num12].Width, FontMatchRect[num12].Height);
								item2.Left = FoundMatchTextRect.Right - item2.Left - item2.Width;
								item2.Top = FoundMatchTextRect.Bottom - item2.Top - item2.Height;
								NGRect.Add(item2);
							}
						}
						Tracker1ResultText += str3;
					}
					if (mat3.Width > 0 && mat3.Height > 0)
					{
						FoundFontMat[num12] = mat3;
						OcrMat1[num12].CopyTo(AIMatchTempMat[num12]);
						num11++;
						if (ModelName[num12] != "")
						{
							GetAIMatchScore(ModelName[num12], num12, AIMatchTempMat[num12], FontPattern[num13], ref PatternMatchScore[num13]);
						}
						else
						{
							new Rect(0, 0, 0, 0);
							PatternMatchScore[num13] = GetOpenCVMatchScore(SearchMat[num12], FontPattern[num13], out PatternMatchRect[num13]);
						}
						AIMatchScore[num12][num13] = PatternMatchScore[num13];
						FontMatchScore[num12] = PatternMatchScore[num13];
						if (FontMatchScore[num12] > 0 && FontMatchScore[num12] >= array5[num12])
						{
							ScoreResult[num12] = true;
							FontResult[num12] = true;
							if (!AdvFontCheck[num12])
							{
								FontResult[num12] = false;
								flag2 = false;
							}
							if (!OCRResult[num12])
							{
								FontResult[num12] = false;
								flag2 = false;
							}
							continue;
						}
						ScoreResult[num12] = false;
						flag2 = false;
						if (FontMatchRect[num12].Width > 0 && FontMatchRect[num12].Height > 0)
						{
							if (!FlipStatus)
							{
								NGRect.Add(FontMatchRect[num12]);
							}
							else
							{
								Rect item3 = new Rect(FontMatchRect[num12].Left - FoundMatchTextRect.Left, FontMatchRect[num12].Top - FoundMatchTextRect.Top, FontMatchRect[num12].Width, FontMatchRect[num12].Height);
								item3.Left = FoundMatchTextRect.Right - item3.Left - item3.Width;
								item3.Top = FoundMatchTextRect.Bottom - item3.Top - item3.Height;
								NGRect.Add(item3);
							}
						}
						if (num11 <= 0 && !flag3)
						{
							if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[抓取字元] 字元[{0}]檢測失敗， 尚未設定檢測字型資料庫。\n", FontChar[num12]);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText += string.Format("[抓取字元] 字元[{0}]检测失败， 尚未设定检测字型资料库。\n", FontChar[num12]);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText += string.Format("[文字抽出] 文字[{0}]の検出に失敗しました。検出フォントデータベースが設定されていません。\n", FontChar[num12]);
							}
							else
							{
								Tracker1ResultText += string.Format("[Font Catch] Font[{0}]Check Fail, font Database count is 0)。\n", FontChar[num12]);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[字元相似度] 字元[{0}]檢測失敗， 檢測分數 = {1}, 小於設定值({2})。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[字元相似度] 字元[{0}]检测失败， 检测分数 = {1}, 小于设定值({2})。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字類似度] 文字[{0}]検出に失敗しました。検出スコア = {1} で、設定値({2})より小さいです。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
						else
						{
							Tracker1ResultText += string.Format("[Font Score Check] Font[{0}]Check Fail, Similarity Score = {1}, less than setup({2})。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
					}
					else
					{
						FoundFontMat[num12] = new Mat();
					}
				}
				else
				{
					FontResult[num12] = true;
				}
			}
			if (!flag2)
			{
				iresult = TOOL_RESULT.EM_NG;
			}
			if (flag2 && !fontToolParam.AutoCombineFontToDBSet && fontToolParam.doAutoCombineFontToDB && fontToolParam.AllFont.Length == 9)
			{
				pfile.FontToolParam.Font9Center1.X = FontMatchRect[0].Left - FoundMatchTextRect.Left + FontMatchRect[0].Width / 2;
				pfile.FontToolParam.Font9Center1.Y = FontMatchRect[0].Top - FoundMatchTextRect.Top + FontMatchRect[0].Height / 2;
				pfile.FontToolParam.Font9Center2.X = FontMatchRect[1].Left - FoundMatchTextRect.Left + FontMatchRect[1].Width / 2;
				pfile.FontToolParam.Font9Center2.Y = FontMatchRect[1].Top - FoundMatchTextRect.Top + FontMatchRect[1].Height / 2;
				pfile.FontToolParam.Font9Center3.X = FontMatchRect[2].Left - FoundMatchTextRect.Left + FontMatchRect[2].Width / 2;
				pfile.FontToolParam.Font9Center3.Y = FontMatchRect[2].Top - FoundMatchTextRect.Top + FontMatchRect[2].Height / 2;
				pfile.FontToolParam.Font9Center4.X = FontMatchRect[3].Left - FoundMatchTextRect.Left + FontMatchRect[3].Width / 2;
				pfile.FontToolParam.Font9Center4.Y = FontMatchRect[3].Top - FoundMatchTextRect.Top + FontMatchRect[3].Height / 2;
				pfile.FontToolParam.Font9Center5.X = FontMatchRect[4].Left - FoundMatchTextRect.Left + FontMatchRect[4].Width / 2;
				pfile.FontToolParam.Font9Center5.Y = FontMatchRect[4].Top - FoundMatchTextRect.Top + FontMatchRect[4].Height / 2;
				pfile.FontToolParam.Font9Center6.X = FontMatchRect[5].Left - FoundMatchTextRect.Left + FontMatchRect[5].Width / 2;
				pfile.FontToolParam.Font9Center6.Y = FontMatchRect[5].Top - FoundMatchTextRect.Top + FontMatchRect[5].Height / 2;
				pfile.FontToolParam.Font9Center7.X = FontMatchRect[6].Left - FoundMatchTextRect.Left + FontMatchRect[6].Width / 2;
				pfile.FontToolParam.Font9Center7.Y = FontMatchRect[6].Top - FoundMatchTextRect.Top + FontMatchRect[6].Height / 2;
				pfile.FontToolParam.Font9Center8.X = FontMatchRect[7].Left - FoundMatchTextRect.Left + FontMatchRect[7].Width / 2;
				pfile.FontToolParam.Font9Center8.Y = FontMatchRect[7].Top - FoundMatchTextRect.Top + FontMatchRect[7].Height / 2;
				pfile.FontToolParam.Font9Center9.X = FontMatchRect[8].Left - FoundMatchTextRect.Left + FontMatchRect[8].Width / 2;
				pfile.FontToolParam.Font9Center9.Y = FontMatchRect[8].Top - FoundMatchTextRect.Top + FontMatchRect[8].Height / 2;
				string CombineError3 = "";
				AutoCombineFont(fontToolParam, ref CombineError3);
			}
			return flag2;
		}
		catch (Exception ex)
		{
			string text2 = "MAT02-FontTool-Inspect()-";
			text2 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool InspectF9(ref TOOL_RESULT iresult, int FaceIndex = 0, bool offline = true)
	{
		FirstMatchALLResult = true;
		MatchSuccess = true;
		FlipStatus = false;
		Tracker1ResultText = "";
		iresult = TOOL_RESULT.EM_OK;
		NGRect.Clear();
		INSP_FONTTOOL_PARAM fontToolParam = pfile.FontToolParam;
		if (pfile.FontToolParam.TextMat == null)
		{
			pfile.FontToolParam.TextMat = new Mat();
		}
		for (int i = 0; i < 5; i++)
		{
			PatternMatchScore[i] = 0;
			PatternMatchRect[i] = new Rect(0, 0, 0, 0);
		}
		for (int j = 0; j < 9; j++)
		{
			FontMatchScore[j] = 0;
			FontResult[j] = false;
			FontMatchRect[j] = new Rect(0, 0, 0, 0);
			FontSearchRect[j] = new Rect(0, 0, 0, 0);
			AdvFontCheck[j] = true;
			OCRResult[j] = true;
			ScoreResult[j] = true;
		}
		try
		{
			if (FaceIndex != 0 && pfile.FontToolParam.EnableSelectFace && FaceIndex != pfile.FontToolParam.FaceIndex)
			{
				iresult = TOOL_RESULT.EM_OK;
				return true;
			}
			if (fontToolParam.PatternSelect < 0 || fontToolParam.PatternSelect >= 210)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 圖形共用資料庫索引錯誤({0})，請重新選擇比對圖形。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 图形共用资料库索引错误({0})，请重新选择比对图形。\n", fontToolParam.PatternSelect + 1);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字照合] 画像共有データベースインデックスエラー({0})、照合する画像を再度選択してください。\n", fontToolParam.PatternSelect + 1);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Tool] Public Pattern file index Error({0}), Select Pattern file again.\n", fontToolParam.PatternSelect + 1);
				}
				return false;
			}
			string arg = "";
			if (fontToolParam.PublicDBUsePublicMode)
			{
				if (!fontToolParam.AutoCombineFontCreate && fontToolParam.AutoCombineFontToDBSet && fontToolParam.doAutoCombineFontToDB && fontToolParam.AllFont.Length == 9)
				{
					for (int k = 0; k < fontToolParam.AllFont.Length; k++)
					{
						string str = fontToolParam.AllFont.Substring(k, 1);
						Mat[] pattern = new Mat[5];
						GetFontPattern(GetFontIndex(str), ref pattern);
						AllFontPattern[k] = pattern[0];
					}
					string CombineError = "";
					if (!AutoCombineFont(fontToolParam, ref CombineError))
					{
						iresult = TOOL_RESULT.EM_NG;
						FirstMatchALLResult = false;
						MatchSuccess = false;
						Tracker1ResultText += CombineError;
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[文字比對] 自動拼字功能錯誤，請重新檢查字元設定。\n");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[文字比对] [文字比对] 自动拼字功能错误，请重新检查字元设定。\n");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字照合] 自動スペルチェック機能にエラーがあります。文字設定を再度確認してください。\n");
						}
						else
						{
							Tracker1ResultText += string.Format("[Font Tool] AutoCombine Error, Please check font setting again.\n");
						}
						return false;
					}
					pfile.FontToolParam.AutoCombineFontCreate = true;
					pfile.PublicPatternDB[208].PatternImage.CopyTo(PatternMAT);
				}
				else
				{
					pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternImage.CopyTo(PatternMAT);
					pfile.FontToolParam.PatternChannel = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternChannel;
					_ = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternON;
					arg = pfile.PublicPatternDB[fontToolParam.PatternSelect].PatternName;
				}
			}
			else if (!fontToolParam.AutoCombineFontCreate && fontToolParam.AutoCombineFontToDBSet && fontToolParam.doAutoCombineFontToDB && fontToolParam.AllFont.Length == 9)
			{
				for (int l = 0; l < fontToolParam.AllFont.Length; l++)
				{
					string str2 = fontToolParam.AllFont.Substring(l, 1);
					Mat[] pattern2 = new Mat[5];
					GetFontPattern(GetFontIndex(str2), ref pattern2);
					AllFontPattern[l] = pattern2[0];
				}
				string CombineError2 = "";
				if (!AutoCombineFont(fontToolParam, ref CombineError2))
				{
					iresult = TOOL_RESULT.EM_NG;
					FirstMatchALLResult = false;
					MatchSuccess = false;
					Tracker1ResultText += CombineError2;
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 自動拼字功能錯誤，請重新檢查字元設定。\n");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文字比对] [文字比对] 自动拼字功能错误，请重新检查字元设定。\n");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字照合] 自動スペルチェック機能にエラーがあります。文字設定を再度確認してください。\n");
					}
					else
					{
						Tracker1ResultText += string.Format("[Font Tool] AutoCombine Error, Please check font setting again.\n");
					}
					return false;
				}
				pfile.FontToolParam.AutoCombineFontCreate = true;
				pfile.PrivatePatternDB[208].PatternImage.CopyTo(PatternMAT);
			}
			else
			{
				pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternImage.CopyTo(PatternMAT);
				pfile.FontToolParam.PatternChannel = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternChannel;
				_ = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternON;
				arg = pfile.PrivatePatternDB[fontToolParam.PatternSelect].PatternName;
			}
			buffers.GetChannelMat(fontToolParam.PatternChannel).CopyTo(BufferCopy);
			GetInspeRect();
			if (PatternMAT.Width <= 0 || PatternMAT.Height <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 圖形共用資料庫({0})影像遺失或檔案損毀。\n", arg);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 图形共用资料库({0})影像遗失或档案损毁。\n", arg);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 画像共有データベース({0})の画像が失われたか、ファイルが破損しています。\n", arg);
				}
				else
				{
					Tracker1ResultText += string.Format("[Font Tool]Public Image DB Image Error({0}).\n", arg);
				}
			}
			int num = 0;
			num = fontToolParam.PatternChannel;
			InspeRect &= new Rect(0, 0, buffers.GetChannelMat(num).Cols, buffers.GetChannelMat(num).Rows);
			if (InspeRect.Width <= 0 || InspeRect.Height <= 0)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，搜尋區域寬高小於0。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，搜寻区域宽高小于0。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー: 検索領域の幅と高さが 0 より小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Error，Search area is 0.\n");
				}
				return false;
			}
			if (fontToolParam.AllFont.Length <= 0 || fontToolParam.AllFont.Length > 9)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，尚未設定搜尋字元。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，尚未设定搜寻字元。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー、検索文字が設定されていません。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Setup Find char first.\n");
				}
				return false;
			}
			if (fontToolParam.MaskSelectIndex > 0)
			{
				int num2 = fontToolParam.MaskSelectIndex - 1;
				if (!pfile.MaskParam[num2].TrackerON)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("字元使用遮罩工具，但遮罩工具[{0}]未開啟! \n", num2 + 1);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("字元使用遮罩工具，但遮罩工具[{0}]未开启! \n", num2 + 1);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("文字はマスクツールを使用していますが、マスクツール[{0}]は有効になっていません! \n", num2 + 1);
					}
					else
					{
						Tracker1ResultText += string.Format("Font Tool Link Error, Mask Tool [{0}] has been Disable! \n", num2 + 1);
					}
					iresult = TOOL_RESULT.EM_NG;
					return false;
				}
				buffers.GetChannelMat(0).CopyTo(MaskArea);
				MaskArea.SetTo(0.0);
				Rect[] array = new Rect[2];
				if (num2 >= 0)
				{
					array[0] = new Rect(pfile.MaskParam[num2].Mask1Pt.X, pfile.MaskParam[num2].Mask1Pt.Y, pfile.MaskParam[num2].Mask1.Width, pfile.MaskParam[num2].Mask1.Height);
					array[0] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
					if (pfile.MaskParam[num2].SearchRect2ON && num2 < 20)
					{
						array[1] = new Rect(pfile.MaskParam[num2].Mask2Pt.X, pfile.MaskParam[num2].Mask2Pt.Y, pfile.MaskParam[num2].Mask2.Width, pfile.MaskParam[num2].Mask2.Height);
						array[1] &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
					}
					else
					{
						array[1] = new Rect(0, 0, 0, 0);
					}
					if (array[0].Width > 0 && array[0].Height > 0)
					{
						using Mat m = new Mat(MaskArea, array[0]);
						using Mat mat = MM.CloneFrom(pfile.MaskParam[num2].Mask1);
						mat.CopyTo(m);
					}
					if (pfile.MaskParam[num2].SearchRect2ON && array[1].Width > 0 && array[1].Height > 0 && num2 < 20)
					{
						using Mat m2 = new Mat(MaskArea, array[1]);
						using Mat mat2 = MM.CloneFrom(pfile.MaskParam[num2].Mask2);
						mat2.CopyTo(m2);
					}
					Cv2.BitwiseNot(MaskArea, MaskArea);
					Cv2.BitwiseAnd(BufferCopy, MaskArea, BufferCopy);
				}
			}
			SearchMatALL = new Mat(BufferCopy, InspeRect);
			if (SearchMatALL.Width < PatternMAT.Width || SearchMatALL.Height < PatternMAT.Height)
			{
				iresult = TOOL_RESULT.EM_NG;
				FirstMatchALLResult = false;
				MatchSuccess = false;
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 比對錯誤，搜尋區域小於設定的字元大小。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 比对错误，搜寻区域小于设定的字元大小。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] 比較エラー、検索範囲が設定された文字サイズよりも小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] Search Area small than setup font size.\n");
				}
				return false;
			}
			FirstMatchALLResult = FindAllMatchText(SearchMatALL, PatternMAT, num);
			if (!FirstMatchALLResult)
			{
				iresult = TOOL_RESULT.EM_NG;
				NGRect.Add(InspeRect);
				return false;
			}
			bool flag = false;
			if (fontToolParam.FontChar1 == "-")
			{
				flag = true;
			}
			FontConnectNG = false;
			pfile.FontToolParam.FontIsBlack = (FontIsBlack = (fontToolParam.FontIsBlack = true));
			if (!PatternRectSeted)
			{
				FindMaxFontRect(PatternMAT, fontToolParam.AllFont.Length, fontToolParam.FontAlignVertical, ref PatternRect, ref PatternArea, ref AutoBinaryThd);
				Sort9FontRect(ref PatternRect, fontToolParam.AllFont.Length);
				GetMaxWH(flag, fontToolParam.FontAlignVertical, fontToolParam.AllFont.Length, ref PatternRect, ref PatternRectMaxW, ref PatternRectMaxH);
				MinPatternArea = GetMinArea(fontToolParam.AllFont.Length, PatternArea);
				MaxPatternWH = 0;
				for (int n = 0; n < fontToolParam.AllFont.Length; n++)
				{
					if ((!flag || n != 0) && PatternRect[n].Width > MaxPatternWH)
					{
						MaxPatternWH = PatternRect[n].Width;
					}
				}
				PatternRectSeted = true;
			}
			Rect[] FontRect = new Rect[fontToolParam.AllFont.Length];
			int[] FontArea = new int[fontToolParam.AllFont.Length];
			new Mat(BufferCopy, FoundMatchTextRect).CopyTo(FoundMat);
			if (fontToolParam.FontConnected)
			{
				bool num3 = FindMaxFontRect(FoundMat, fontToolParam.AllFont.Length, fontToolParam.FontAlignVertical, ref FontRect, ref FontArea, ref AutoBinaryThd);
				Sort9FontRect(ref FontRect, fontToolParam.AllFont.Length);
				if (!num3 && fontToolParam.ConnectCountAlarm)
				{
					iresult = TOOL_RESULT.EM_NG;
					if (offline)
					{
						if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
						{
							NGRect.Add(FoundMatchTextRect);
						}
						for (int num4 = 0; num4 < 5; num4++)
						{
							FontBinary[num4] = new Mat();
						}
					}
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，字元數量異常，設定{0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文字比对] 发现字符粘黏，字符数量异常，设置{0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字比較] 文字の粘着が検出されました。文字数が異常です。設定: {0}。 (CT-01)\n", fontToolParam.AllFont.Length);
					}
					else
					{
						Tracker1ResultText += string.Format("[FontTool] Character adhesion detected. The number of characters is abnormal. Set to {0}. (CT-01)\n", fontToolParam.AllFont.Length);
					}
					FontConnectNG = true;
					return false;
				}
				for (int num5 = 0; num5 < fontToolParam.AllFont.Length; num5++)
				{
					if (flag && num5 == 0)
					{
						continue;
					}
					if ((double)FontRect[num5].Width > (double)MaxPatternWH * 1.5 && fontToolParam.ConnectWidthAlarm)
					{
						iresult = TOOL_RESULT.EM_NG;
						if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，寬度異常。 (CT-02)\n");
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[文本比对] 发现字符粘连，宽度异常。 (CT-02)\n");
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字比較] 文字の結合と異常な幅が見つかりました。 (CT-02)\n");
						}
						else
						{
							Tracker1ResultText += string.Format("[FontTool] Character sticking and abnormal width were found. (CT-02)\n");
						}
						if (offline)
						{
							if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
							{
								if (FlipStatus)
								{
									if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
									{
										NGRect.Add(new Rect(FoundMatchTextRect.Right - FontRect[num5].Left - FontRect[num5].Width, FoundMatchTextRect.Bottom - FontRect[num5].Height - FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
									}
								}
								else if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
								{
									NGRect.Add(new Rect(FoundMatchTextRect.Left + FontRect[num5].Left, FoundMatchTextRect.Top + FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
								}
							}
							for (int num6 = 0; num6 < 5; num6++)
							{
								FontBinary[num6] = new Mat();
							}
						}
						FontConnectNG = true;
						return false;
					}
					if (!((double)FontRect[num5].Height > (double)MaxPatternWH * 1.5) || !fontToolParam.ConnectWidthAlarm)
					{
						continue;
					}
					iresult = TOOL_RESULT.EM_NG;
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("[文字比對] 發現字元沾黏，高度異常。 (CT-02)\n");
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("[文本比对] 发现字符粘连，高度异常。 (CT-02)\n");
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("[文字比較] 文字の結合と異常な高が見つかりました。 (CT-02)\n");
					}
					else
					{
						Tracker1ResultText += string.Format("[FontTool] Character sticking and abnormal width were found. (CT-02)\n");
					}
					if (offline)
					{
						if (FoundMatchTextRect.Width > 0 && FoundMatchTextRect.Height > 0)
						{
							if (FlipStatus)
							{
								if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
								{
									NGRect.Add(new Rect(FoundMatchTextRect.Right - FontRect[num5].Left - FontRect[num5].Width, FoundMatchTextRect.Bottom - FontRect[num5].Height - FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
								}
							}
							else if (FontRect[num5].Width > 0 && FontRect[num5].Height > 0)
							{
								NGRect.Add(new Rect(FoundMatchTextRect.Left + FontRect[num5].Left, FoundMatchTextRect.Top + FontRect[num5].Top, FontRect[num5].Width, FontRect[num5].Height));
							}
						}
						for (int num7 = 0; num7 < 5; num7++)
						{
							FontBinary[num7] = new Mat();
						}
					}
					FontConnectNG = true;
					return false;
				}
			}
			string[] array2 = new string[9];
			bool[] array3 = new bool[9];
			for (int num8 = 0; num8 < 9; num8++)
			{
				array2[num8] = "";
				array3[num8] = false;
				FontSearchRect[num8] = new Rect(0, 0, 0, 0);
				for (int num9 = 0; num9 < 5; num9++)
				{
					AIMatchScore[num8][num9] = 0;
				}
			}
			for (int num10 = 0; num10 < fontToolParam.AllFont.Length; num10++)
			{
				array2[num10] = fontToolParam.AllFont.Substring(num10, 1);
			}
			Calc9FontSearchRect(fontToolParam, array2, ref FontSearchRect);
			int[] array4 = new int[9];
			int[] array5 = new int[9];
			array4[0] = fontToolParam.FontIndex1;
			array4[1] = fontToolParam.FontIndex2;
			array4[2] = fontToolParam.FontIndex3;
			array4[3] = fontToolParam.FontIndex4;
			array4[4] = fontToolParam.FontIndex5;
			array4[5] = fontToolParam.FontIndex6;
			array4[6] = fontToolParam.FontIndex7;
			array4[7] = fontToolParam.FontIndex8;
			array4[8] = fontToolParam.FontIndex9;
			array5[0] = fontToolParam.MinScore_Font1;
			array5[1] = fontToolParam.MinScore_Font2;
			array5[2] = fontToolParam.MinScore_Font3;
			array5[3] = fontToolParam.MinScore_Font4;
			array5[4] = fontToolParam.MinScore_Font5;
			array5[5] = fontToolParam.MinScore_Font6;
			array5[6] = fontToolParam.MinScore_Font7;
			array5[7] = fontToolParam.MinScore_Font8;
			array5[8] = fontToolParam.MinScore_Font9;
			FontChar[0] = fontToolParam.FontChar1;
			FontChar[1] = fontToolParam.FontChar2;
			FontChar[2] = fontToolParam.FontChar3;
			FontChar[3] = fontToolParam.FontChar4;
			FontChar[4] = fontToolParam.FontChar5;
			FontChar[5] = fontToolParam.FontChar6;
			FontChar[6] = fontToolParam.FontChar7;
			FontChar[7] = fontToolParam.FontChar8;
			FontChar[8] = fontToolParam.FontChar9;
			ModelName[0] = fontToolParam.AIModel_Font1;
			ModelName[1] = fontToolParam.AIModel_Font2;
			ModelName[2] = fontToolParam.AIModel_Font3;
			ModelName[3] = fontToolParam.AIModel_Font4;
			ModelName[4] = fontToolParam.AIModel_Font5;
			ModelName[5] = fontToolParam.AIModel_Font6;
			ModelName[6] = fontToolParam.AIModel_Font7;
			ModelName[7] = fontToolParam.AIModel_Font8;
			ModelName[8] = fontToolParam.AIModel_Font9;
			int num11 = 0;
			bool flag2 = true;
			bool flag3 = false;
			_ = new bool[9] { fontToolParam.EnableFontCheck1, fontToolParam.EnableFontCheck2, fontToolParam.EnableFontCheck3, fontToolParam.EnableFontCheck4, fontToolParam.EnableFontCheck5, fontToolParam.EnableFontCheck6, fontToolParam.EnableFontCheck7, fontToolParam.EnableFontCheck8, fontToolParam.EnableFontCheck9 };
			bool[] array6 = new bool[9] { fontToolParam.BypassOCR1, fontToolParam.BypassOCR2, fontToolParam.BypassOCR3, fontToolParam.BypassOCR4, fontToolParam.BypassOCR5, fontToolParam.BypassOCR6, fontToolParam.BypassOCR7, fontToolParam.BypassOCR8, fontToolParam.BypassOCR9 };
			for (int num12 = 0; num12 < fontToolParam.AllFont.Length; num12++)
			{
				if (array4[num12] >= 0)
				{
					FontResult[num12] = false;
					AdvFontCheck[num12] = true;
					OCRResult[num12] = true;
					ScoreResult[num12] = true;
					GetFontPattern(array4[num12], ref FontPattern);
					num11 = 0;
					int num13 = 0;
					Mat mat3 = new Mat();
					int num14 = 0;
					string text = "";
					bool flag4 = false;
					for (int num15 = 0; num15 < 5; num15++)
					{
						PatternMatchScore[num15] = 0;
						if (FontPattern[num15].Width <= 0 || FontPattern[num15].Height <= 0)
						{
							continue;
						}
						if (SearchMat[num12].Width < FontPattern[num15].Width || SearchMat[num12].Height < FontPattern[num15].Height)
						{
							flag3 = true;
							text = ((buffers.LanguageType != 1) ? ((buffers.LanguageType != 2) ? ((buffers.LanguageType != 3) ? (text + string.Format("[Font Catch] Font[{0}]Check Fail, search area small than font pattern)。\n", FontChar[num12])) : (text + string.Format("[文字抽出] 文字[{0}]検出失敗、検索領域が文字サイズより小さいです。\n", FontChar[num12]))) : (text + string.Format("[抓取字元] 字元[{0}]检测失败，搜寻区域小于文字大小。\n", FontChar[num12]))) : (text + string.Format("[抓取字元] 字元[{0}]檢測失敗，搜尋區域小於文字大小。\n", FontChar[num12])));
							continue;
						}
						PatternMatchScore[num15] = GetOpenCVMatchScore(SearchMat[num12], FontPattern[num15], out PatternMatchRect[num15]);
						if (PatternMatchRect[num15].Width <= 0 && PatternMatchRect[num15].Height <= 0)
						{
							num15 = 5;
						}
						else if (num14 == 0 || PatternMatchScore[num15] > num14)
						{
							mat3 = new Mat(SearchMat[num12], PatternMatchRect[num15]);
							FontMatchRect[num12] = PatternMatchRect[num15];
							FontMatchRect[num12].Left += FontSearchRect[num12].Left;
							FontMatchRect[num12].Top += FontSearchRect[num12].Top;
							AllFontPattern[num12] = FontPattern[num15];
							num14 = PatternMatchScore[num15];
							flag4 = true;
							num13 = num15;
						}
					}
					if (!flag4)
					{
						Tracker1ResultText += text;
						iresult = TOOL_RESULT.EM_NG;
						return false;
					}
					mat3.CopyTo(OcrMat1[num12]);
					FontPattern[num13].CopyTo(OcrMat2[num12]);
					if (fontToolParam.FontAlignVertical)
					{
						Cv2.Transpose(OcrMat1[num12], OcrMat1[num12]);
						Cv2.Flip(OcrMat1[num12], OcrMat1[num12], FlipMode.Y);
						Cv2.Transpose(OcrMat2[num12], OcrMat2[num12]);
						Cv2.Flip(OcrMat2[num12], OcrMat2[num12], FlipMode.Y);
					}
					OcrMat1[num12].CopyTo(FontBinary[num12]);
					if (fontToolParam.EnabledOCR && !array6[num12])
					{
						string ocrChar = "";
						OcrMat1[num12].CopyTo(OcrFinalMat);
						if (FontIsBlack)
						{
							Cv2.BitwiseNot(OcrFinalMat, OcrFinalMat);
						}
						OCRResult[num12] = GetAIOCRResult(OcrFinalMat, ref ocrChar);
						string fontCharFromIndex = GetFontCharFromIndex(array4[num12]);
						if (ocrChar != fontCharFromIndex)
						{
							bool flag5 = false;
							if ((ocrChar == "1" && fontCharFromIndex == "-") || (ocrChar == "-" && fontCharFromIndex == "1"))
							{
								flag5 = true;
								ocrChar = fontCharFromIndex;
							}
							else if (ocrChar == "1" && fontCharFromIndex == "7")
							{
								Mat[] pattern3 = new Mat[5];
								GetFontPattern(6, ref pattern3);
								if (pattern3[num13].Width <= 0 || pattern3[num13].Height <= 0)
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else if (Math.Abs(OcrMat1[num12].Width - OcrMat2[num12].Width) < Math.Abs(pattern3[num13].Width - OcrMat2[num12].Width))
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else
								{
									flag5 = false;
								}
							}
							else if (ocrChar == "7" && fontCharFromIndex == "1")
							{
								Mat[] pattern4 = new Mat[5];
								GetFontPattern(12, ref pattern4);
								if (pattern4[num13].Width <= 0 || pattern4[num13].Height <= 0)
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else if (Math.Abs(OcrMat1[num12].Width - OcrMat2[num12].Width) < Math.Abs(pattern4[num13].Width - OcrMat2[num12].Width))
								{
									flag5 = true;
									ocrChar = fontCharFromIndex;
								}
								else
								{
									flag5 = false;
								}
							}
							else if ((ocrChar == "0" && fontCharFromIndex == "O") || (ocrChar == "O" && fontCharFromIndex == "0"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "0" && fontCharFromIndex == "D") || (ocrChar == "D" && fontCharFromIndex == "0"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "8" && fontCharFromIndex == "B") || (ocrChar == "B" && fontCharFromIndex == "8"))
							{
								flag5 = true;
							}
							else if ((ocrChar == "R" && fontCharFromIndex == "A") || (ocrChar == "A" && fontCharFromIndex == "R"))
							{
								flag5 = true;
							}
							if (!flag5)
							{
								if (ocrChar == "1" || ocrChar == "-")
								{
									if (buffers.LanguageType == 1)
									{
										Tracker1ResultText += string.Format("[OCR文字比對] 字元[{0}]檢測失敗， 找不到符合字元。\n", FontChar[num12]);
									}
									else if (buffers.LanguageType == 2)
									{
										Tracker1ResultText += string.Format("[OCR文字比对] 字元[{0}]检测失败， 找不到符合字元。\n", FontChar[num12]);
									}
									else if (buffers.LanguageType == 3)
									{
										Tracker1ResultText += string.Format("[OCR文字照合] 文字[{0}]検出失敗、 一致する文字が見つかりません。\n", FontChar[num12]);
									}
									else
									{
										Tracker1ResultText += string.Format("[OCR Check] Font [{0}] OCR Check error, can't find match font.\n", FontChar[num12]);
									}
								}
								else if (buffers.LanguageType == 1)
								{
									Tracker1ResultText += string.Format("[OCR文字比對] 字元[{0}]檢測失敗， 找到字元為{1}。\n", FontChar[num12], ocrChar);
								}
								else if (buffers.LanguageType == 2)
								{
									Tracker1ResultText += string.Format("[OCR文字比对] 字元[{0}]检测失败， 找到字元为{1}。\n", FontChar[num12], ocrChar);
								}
								else if (buffers.LanguageType == 3)
								{
									Tracker1ResultText += string.Format("[OCR文字照合] 文字[{0}]検出失敗、検出された文字は{1}です。\n", FontChar[num12], ocrChar);
								}
								else
								{
									Tracker1ResultText += string.Format("[OCR Check] Font [{0}]OCR Check error, found [{1}]\n", FontChar[num12], ocrChar);
								}
								OCRResult[num12] = false;
								if (offline && FontMatchRect[num12].Width > 0 && FontMatchRect[num12].Height > 0)
								{
									if (!FlipStatus)
									{
										NGRect.Add(FontMatchRect[num12]);
									}
									else
									{
										Rect item = new Rect(FontMatchRect[num12].Left - FoundMatchTextRect.Left, FontMatchRect[num12].Top - FoundMatchTextRect.Top, FontMatchRect[num12].Width, FontMatchRect[num12].Height);
										item.Left = FoundMatchTextRect.Right - item.Left - item.Width;
										item.Top = FoundMatchTextRect.Bottom - item.Top - item.Height;
										NGRect.Add(item);
									}
								}
							}
						}
					}
					if (mat3.Width > 0 && mat3.Height > 0)
					{
						FoundFontMat[num12] = mat3;
						OcrMat1[num12].CopyTo(AIMatchTempMat[num12]);
						num11++;
						if (ModelName[num12] != "")
						{
							GetAIMatchScore(ModelName[num12], num12, AIMatchTempMat[num12], FontPattern[num13], ref PatternMatchScore[num13]);
						}
						else
						{
							new Rect(0, 0, 0, 0);
							PatternMatchScore[num13] = GetOpenCVMatchScore(SearchMat[num12], FontPattern[num13], out PatternMatchRect[num13]);
						}
						AIMatchScore[num12][num13] = PatternMatchScore[num13];
						FontMatchScore[num12] = PatternMatchScore[num13];
						if (FontMatchScore[num12] > 0 && FontMatchScore[num12] >= array5[num12])
						{
							ScoreResult[num12] = true;
							FontResult[num12] = true;
							if (!AdvFontCheck[num12])
							{
								FontResult[num12] = false;
								flag2 = false;
							}
							if (!OCRResult[num12])
							{
								FontResult[num12] = false;
								flag2 = false;
							}
							continue;
						}
						ScoreResult[num12] = false;
						flag2 = false;
						if (FontMatchRect[num12].Width > 0 && FontMatchRect[num12].Height > 0)
						{
							if (!FlipStatus)
							{
								NGRect.Add(FontMatchRect[num12]);
							}
							else
							{
								Rect item2 = new Rect(FontMatchRect[num12].Left - FoundMatchTextRect.Left, FontMatchRect[num12].Top - FoundMatchTextRect.Top, FontMatchRect[num12].Width, FontMatchRect[num12].Height);
								item2.Left = FoundMatchTextRect.Right - item2.Left - item2.Width;
								item2.Top = FoundMatchTextRect.Bottom - item2.Top - item2.Height;
								NGRect.Add(item2);
							}
						}
						if (num11 <= 0 && !flag3)
						{
							if (buffers.LanguageType == 1)
							{
								Tracker1ResultText += string.Format("[抓取字元] 字元[{0}]檢測失敗， 尚未設定檢測字型資料庫。\n", FontChar[num12]);
							}
							else if (buffers.LanguageType == 2)
							{
								Tracker1ResultText += string.Format("[抓取字元] 字元[{0}]检测失败， 尚未设定检测字型资料库。\n", FontChar[num12]);
							}
							else if (buffers.LanguageType == 3)
							{
								Tracker1ResultText += string.Format("[文字抽出] 文字[{0}]の検出に失敗しました。検出フォントデータベースが設定されていません。\n", FontChar[num12]);
							}
							else
							{
								Tracker1ResultText += string.Format("[Font Catch] Font[{0}]Check Fail, font Database count is 0)。\n", FontChar[num12]);
							}
						}
						else if (buffers.LanguageType == 1)
						{
							Tracker1ResultText += string.Format("[字元相似度] 字元[{0}]檢測失敗， 檢測分數 = {1}, 小於設定值({2})。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
						else if (buffers.LanguageType == 2)
						{
							Tracker1ResultText += string.Format("[字元相似度] 字元[{0}]检测失败， 检测分数 = {1}, 小于设定值({2})。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
						else if (buffers.LanguageType == 3)
						{
							Tracker1ResultText += string.Format("[文字類似度] 文字[{0}]検出に失敗しました。検出スコア = {1} で、設定値({2})より小さいです。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
						else
						{
							Tracker1ResultText += string.Format("[Font Score Check] Font[{0}]Check Fail, Similarity Score = {1}, less than setup({2})。\n", FontChar[num12], FontMatchScore[num12], array5[num12]);
						}
					}
					else
					{
						FoundFontMat[num12] = new Mat();
					}
				}
				else
				{
					FontResult[num12] = true;
				}
			}
			if (!flag2)
			{
				iresult = TOOL_RESULT.EM_NG;
			}
			if (flag2 && !fontToolParam.AutoCombineFontToDBSet && fontToolParam.doAutoCombineFontToDB && fontToolParam.AllFont.Length == 9)
			{
				pfile.FontToolParam.Font9Center1.X = FontMatchRect[0].Left - FoundMatchTextRect.Left + FontMatchRect[0].Width / 2;
				pfile.FontToolParam.Font9Center1.Y = FontMatchRect[0].Top - FoundMatchTextRect.Top + FontMatchRect[0].Height / 2;
				pfile.FontToolParam.Font9Center2.X = FontMatchRect[1].Left - FoundMatchTextRect.Left + FontMatchRect[1].Width / 2;
				pfile.FontToolParam.Font9Center2.Y = FontMatchRect[1].Top - FoundMatchTextRect.Top + FontMatchRect[1].Height / 2;
				pfile.FontToolParam.Font9Center3.X = FontMatchRect[2].Left - FoundMatchTextRect.Left + FontMatchRect[2].Width / 2;
				pfile.FontToolParam.Font9Center3.Y = FontMatchRect[2].Top - FoundMatchTextRect.Top + FontMatchRect[2].Height / 2;
				pfile.FontToolParam.Font9Center4.X = FontMatchRect[3].Left - FoundMatchTextRect.Left + FontMatchRect[3].Width / 2;
				pfile.FontToolParam.Font9Center4.Y = FontMatchRect[3].Top - FoundMatchTextRect.Top + FontMatchRect[3].Height / 2;
				pfile.FontToolParam.Font9Center5.X = FontMatchRect[4].Left - FoundMatchTextRect.Left + FontMatchRect[4].Width / 2;
				pfile.FontToolParam.Font9Center5.Y = FontMatchRect[4].Top - FoundMatchTextRect.Top + FontMatchRect[4].Height / 2;
				pfile.FontToolParam.Font9Center6.X = FontMatchRect[5].Left - FoundMatchTextRect.Left + FontMatchRect[5].Width / 2;
				pfile.FontToolParam.Font9Center6.Y = FontMatchRect[5].Top - FoundMatchTextRect.Top + FontMatchRect[5].Height / 2;
				pfile.FontToolParam.Font9Center7.X = FontMatchRect[6].Left - FoundMatchTextRect.Left + FontMatchRect[6].Width / 2;
				pfile.FontToolParam.Font9Center7.Y = FontMatchRect[6].Top - FoundMatchTextRect.Top + FontMatchRect[6].Height / 2;
				pfile.FontToolParam.Font9Center8.X = FontMatchRect[7].Left - FoundMatchTextRect.Left + FontMatchRect[7].Width / 2;
				pfile.FontToolParam.Font9Center8.Y = FontMatchRect[7].Top - FoundMatchTextRect.Top + FontMatchRect[7].Height / 2;
				pfile.FontToolParam.Font9Center9.X = FontMatchRect[8].Left - FoundMatchTextRect.Left + FontMatchRect[8].Width / 2;
				pfile.FontToolParam.Font9Center9.Y = FontMatchRect[8].Top - FoundMatchTextRect.Top + FontMatchRect[8].Height / 2;
				string CombineError3 = "";
				AutoCombineFont(fontToolParam, ref CombineError3);
			}
			return flag2;
		}
		catch (Exception ex)
		{
			string text2 = "MAT02-FontTool-Inspect()-";
			text2 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-003");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool AutoCombineFont(INSP_FONTTOOL_PARAM Param, ref string CombineError)
	{
		MatchPattern[] array = ((!Param.PublicDBUsePublicMode) ? pfile.PrivatePatternDB : pfile.PublicPatternDB);
		if (array[209].PatternImage.Width > 0 && array[209].PatternImage.Height > 0)
		{
			array[208].PatternON = true;
			array[209].PatternImage.CopyTo(array[208].PatternImage);
			array[208].PatternChannel = array[209].PatternChannel;
			array[208].PatternName = pfile.FontToolParam.AllFont;
			array[208].PatternPath = string.Format("{0}\\P208-{1}.bmp", Path.GetDirectoryName(array[209].PatternPath), array[208].PatternChannel);
			array[208].PatternRect = array[209].PatternRect;
			array[208].ValueChangeFlag = true;
			array[208].NameChangeFlag = array[209].NameChangeFlag;
			bool flag = true;
			for (int i = 0; i < 9; i++)
			{
				if (AllFontPattern[i].Width <= 0 || AllFontPattern[i].Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						CombineError += string.Format("{0}字元尚未設定。\n", Param.AllFont.Substring(i, 1));
					}
					else if (buffers.LanguageType == 2)
					{
						CombineError += string.Format("{0}字符尚未设置。\n", Param.AllFont.Substring(i, 1));
					}
					else if (buffers.LanguageType == 3)
					{
						CombineError += string.Format("{0}文字が設定されていません。\n", Param.AllFont.Substring(i, 1));
					}
					else
					{
						CombineError += string.Format("{0} character is not set.\n", Param.AllFont.Substring(i, 1));
					}
					flag = false;
				}
			}
			if (!flag)
			{
				return false;
			}
			Rect roi = new Rect(Param.Font9Center1.X - AllFontPattern[0].Width / 2, Param.Font9Center1.Y - AllFontPattern[0].Height / 2, AllFontPattern[0].Width, AllFontPattern[0].Height);
			Mat patternImage = array[208].PatternImage;
			if (roi.Width <= patternImage.Width && roi.Height <= patternImage.Height)
			{
				if (roi.Left < 0)
				{
					roi.Left = 0;
				}
				if (roi.Top < 0)
				{
					roi.Top = 0;
				}
				if (roi.Left + roi.Width >= patternImage.Width)
				{
					roi.Left -= roi.Left + roi.Width - patternImage.Width;
				}
				if (roi.Top + roi.Height >= patternImage.Height)
				{
					roi.Top -= roi.Top + roi.Height - patternImage.Height;
				}
				Mat m = new Mat(patternImage, roi);
				AllFontPattern[0].CopyTo(m);
			}
			Rect roi2 = new Rect(Param.Font9Center2.X - AllFontPattern[1].Width / 2, Param.Font9Center2.Y - AllFontPattern[1].Height / 2, AllFontPattern[1].Width, AllFontPattern[1].Height);
			Mat patternImage2 = array[208].PatternImage;
			if (roi2.Width <= patternImage2.Width && roi2.Height <= patternImage2.Height)
			{
				if (roi2.Left < 0)
				{
					roi2.Left = 0;
				}
				if (roi2.Top < 0)
				{
					roi2.Top = 0;
				}
				if (roi2.Left + roi2.Width >= patternImage2.Width)
				{
					roi2.Left -= roi2.Left + roi2.Width - patternImage2.Width;
				}
				if (roi2.Top + roi2.Height >= patternImage2.Height)
				{
					roi2.Top -= roi2.Top + roi2.Height - patternImage2.Height;
				}
				Mat m2 = new Mat(patternImage2, roi2);
				AllFontPattern[1].CopyTo(m2);
			}
			Rect roi3 = new Rect(Param.Font9Center3.X - AllFontPattern[2].Width / 2, Param.Font9Center3.Y - AllFontPattern[2].Height / 2, AllFontPattern[2].Width, AllFontPattern[2].Height);
			Mat patternImage3 = array[208].PatternImage;
			if (roi3.Width <= patternImage3.Width && roi3.Height <= patternImage3.Height)
			{
				if (roi3.Left < 0)
				{
					roi3.Left = 0;
				}
				if (roi3.Top < 0)
				{
					roi3.Top = 0;
				}
				if (roi3.Left + roi3.Width >= patternImage3.Width)
				{
					roi3.Left -= roi3.Left + roi3.Width - patternImage3.Width;
				}
				if (roi3.Top + roi3.Height >= patternImage3.Height)
				{
					roi3.Top -= roi3.Top + roi3.Height - patternImage3.Height;
				}
				Mat m3 = new Mat(patternImage3, roi3);
				AllFontPattern[2].CopyTo(m3);
			}
			Rect roi4 = new Rect(Param.Font9Center4.X - AllFontPattern[3].Width / 2, Param.Font9Center4.Y - AllFontPattern[3].Height / 2, AllFontPattern[3].Width, AllFontPattern[3].Height);
			Mat patternImage4 = array[208].PatternImage;
			if (roi4.Width <= patternImage4.Width && roi4.Height <= patternImage4.Height)
			{
				if (roi4.Left < 0)
				{
					roi4.Left = 0;
				}
				if (roi4.Top < 0)
				{
					roi4.Top = 0;
				}
				if (roi4.Left + roi4.Width >= patternImage4.Width)
				{
					roi4.Left -= roi4.Left + roi4.Width - patternImage4.Width;
				}
				if (roi4.Top + roi4.Height >= patternImage4.Height)
				{
					roi4.Top -= roi4.Top + roi4.Height - patternImage4.Height;
				}
				Mat m4 = new Mat(patternImage4, roi4);
				AllFontPattern[3].CopyTo(m4);
			}
			Rect roi5 = new Rect(Param.Font9Center5.X - AllFontPattern[4].Width / 2, Param.Font9Center5.Y - AllFontPattern[4].Height / 2, AllFontPattern[4].Width, AllFontPattern[4].Height);
			Mat patternImage5 = array[208].PatternImage;
			if (roi5.Width <= patternImage5.Width && roi5.Height <= patternImage5.Height)
			{
				if (roi5.Left < 0)
				{
					roi5.Left = 0;
				}
				if (roi5.Top < 0)
				{
					roi5.Top = 0;
				}
				if (roi5.Left + roi5.Width >= patternImage5.Width)
				{
					roi5.Left -= roi5.Left + roi5.Width - patternImage5.Width;
				}
				if (roi5.Top + roi5.Height >= patternImage5.Height)
				{
					roi5.Top -= roi5.Top + roi5.Height - patternImage5.Height;
				}
				Mat m5 = new Mat(patternImage5, roi5);
				AllFontPattern[4].CopyTo(m5);
			}
			Rect roi6 = new Rect(Param.Font9Center6.X - AllFontPattern[5].Width / 2, Param.Font9Center6.Y - AllFontPattern[5].Height / 2, AllFontPattern[5].Width, AllFontPattern[5].Height);
			Mat patternImage6 = array[208].PatternImage;
			if (roi6.Width <= patternImage6.Width && roi6.Height <= patternImage6.Height)
			{
				if (roi6.Left < 0)
				{
					roi6.Left = 0;
				}
				if (roi6.Top < 0)
				{
					roi6.Top = 0;
				}
				if (roi6.Left + roi6.Width >= patternImage6.Width)
				{
					roi6.Left -= roi6.Left + roi6.Width - patternImage6.Width;
				}
				if (roi6.Top + roi6.Height >= patternImage6.Height)
				{
					roi6.Top -= roi6.Top + roi6.Height - patternImage6.Height;
				}
				Mat m6 = new Mat(patternImage6, roi6);
				AllFontPattern[5].CopyTo(m6);
			}
			Rect roi7 = new Rect(Param.Font9Center7.X - AllFontPattern[6].Width / 2, Param.Font9Center7.Y - AllFontPattern[6].Height / 2, AllFontPattern[6].Width, AllFontPattern[6].Height);
			Mat patternImage7 = array[208].PatternImage;
			if (roi7.Width <= patternImage7.Width && roi7.Height <= patternImage7.Height)
			{
				if (roi7.Left < 0)
				{
					roi7.Left = 0;
				}
				if (roi7.Top < 0)
				{
					roi7.Top = 0;
				}
				if (roi7.Left + roi7.Width >= patternImage7.Width)
				{
					roi7.Left -= roi7.Left + roi7.Width - patternImage7.Width;
				}
				if (roi7.Top + roi7.Height >= patternImage7.Height)
				{
					roi7.Top -= roi7.Top + roi7.Height - patternImage7.Height;
				}
				Mat m7 = new Mat(patternImage7, roi7);
				AllFontPattern[6].CopyTo(m7);
			}
			Rect roi8 = new Rect(Param.Font9Center8.X - AllFontPattern[7].Width / 2, Param.Font9Center8.Y - AllFontPattern[7].Height / 2, AllFontPattern[7].Width, AllFontPattern[7].Height);
			Mat patternImage8 = array[208].PatternImage;
			if (roi8.Width <= patternImage8.Width && roi8.Height <= patternImage8.Height)
			{
				if (roi8.Left < 0)
				{
					roi8.Left = 0;
				}
				if (roi8.Top < 0)
				{
					roi8.Top = 0;
				}
				if (roi8.Left + roi8.Width >= patternImage8.Width)
				{
					roi8.Left -= roi8.Left + roi8.Width - patternImage8.Width;
				}
				if (roi8.Top + roi8.Height >= patternImage8.Height)
				{
					roi8.Top -= roi8.Top + roi8.Height - patternImage8.Height;
				}
				Mat m8 = new Mat(patternImage8, roi8);
				AllFontPattern[7].CopyTo(m8);
			}
			Rect roi9 = new Rect(Param.Font9Center9.X - AllFontPattern[8].Width / 2, Param.Font9Center9.Y - AllFontPattern[8].Height / 2, AllFontPattern[8].Width, AllFontPattern[8].Height);
			Mat patternImage9 = array[208].PatternImage;
			if (roi9.Width <= patternImage9.Width && roi9.Height <= patternImage9.Height)
			{
				if (roi9.Left < 0)
				{
					roi9.Left = 0;
				}
				if (roi9.Top < 0)
				{
					roi9.Top = 0;
				}
				if (roi9.Left + roi9.Width >= patternImage9.Width)
				{
					roi9.Left -= roi9.Left + roi9.Width - patternImage9.Width;
				}
				if (roi9.Top + roi9.Height >= patternImage9.Height)
				{
					roi9.Top -= roi9.Top + roi9.Height - patternImage9.Height;
				}
				Mat m9 = new Mat(patternImage9, roi9);
				AllFontPattern[8].CopyTo(m9);
			}
			pfile.FontToolParam.AutoCombineFontToDBSet = true;
		}
		return true;
	}

	public int GetOpenCVMatchScore(Mat Smat, Mat Pmat, out Rect rt)
	{
		Cv2.MatchTemplate(Smat, Pmat, Score1MatchImage, TemplateMatchModes.CCoeffNormed);
		Cv2.MinMaxLoc(Score1MatchImage, out var _, out var maxVal, out var _, out var maxLoc);
		rt = new Rect(maxLoc.X, maxLoc.Y, Pmat.Cols, Pmat.Rows);
		rt &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
		return (int)(maxVal * 100.0);
	}

	private int GetOpenCVMatchScore2(Mat Smat, Mat Pmat, out Rect rt)
	{
		Cv2.MatchTemplate(Smat, Pmat, Score2MatchImage, TemplateMatchModes.SqDiffNormed);
		Cv2.MinMaxLoc(Score2MatchImage, out var minVal, out var _, out var minLoc, out var _);
		rt = new Rect(minLoc.X, minLoc.Y, Pmat.Cols, Pmat.Rows);
		rt &= new Rect(0, 0, buffers.GetChannelMat(0).Cols, buffers.GetChannelMat(0).Rows);
		return 100 - (int)(minVal * 100.0);
	}

	public void CreatInst()
	{
		if (!ModelACreatInst)
		{
			int gpu_id = 0;
			m_DeltaAIProcess_F1 = CreateInstance();
			m_DeltaAIProcess_F2 = CreateInstance();
			m_DeltaAIProcess_F3 = CreateInstance();
			m_DeltaAIProcess_F4 = CreateInstance();
			m_DeltaAIProcess_F5 = CreateInstance();
			m_DeltaAIProcess_F6 = CreateInstance();
			m_DeltaAIProcess_F7 = CreateInstance();
			m_DeltaAIProcess_F8 = CreateInstance();
			m_DeltaAIProcess_F9 = CreateInstance();
			m_DeltaAIProcess_F10 = CreateInstance();
			m_DeltaAIProcess_OCR = CreateInstance();
			useCUDA = pfile.FontOCRUseCuda;
			if (useCUDA)
			{
				SetGPU(m_DeltaAIProcess_F1, gpu_id);
				SetGPU(m_DeltaAIProcess_F2, gpu_id);
				SetGPU(m_DeltaAIProcess_F3, gpu_id);
				SetGPU(m_DeltaAIProcess_F4, gpu_id);
				SetGPU(m_DeltaAIProcess_F5, gpu_id);
				SetGPU(m_DeltaAIProcess_F6, gpu_id);
				SetGPU(m_DeltaAIProcess_F7, gpu_id);
				SetGPU(m_DeltaAIProcess_F8, gpu_id);
				SetGPU(m_DeltaAIProcess_F9, gpu_id);
				SetGPU(m_DeltaAIProcess_F10, gpu_id);
				SetGPU(m_DeltaAIProcess_OCR, gpu_id);
			}
			ModelACreatInst = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadOCRModel(string modelfile)
	{
		try
		{
			if (!ModelACreatInst)
			{
				CreatInst();
			}
			if (!DllExist)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 找不到AI DLL({0})。\n", "DeltaAIProcessDll.dll");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比對] 找不到AI DLL({0})。\n", "DeltaAIProcessDll.dll");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[テキスト比較] AI DLL({0})が見つかりません。\n", "DeltaAIProcessDll.dll");
				}
				else
				{
					Tracker1ResultText += string.Format("[Text Comparison] AI DLL({0}) not found.\n", "DeltaAIProcessDll.dll");
				}
				return false;
			}
			string text = "";
			text = ((!pfile.FontToolParam.FontDBUsePublicMode) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!File.Exists(modelfile))
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 找不到OCR模型({0}。\n", modelfile);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 找不到OCR模型({0}。\n", modelfile);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] OCRモデル({0})が見つかりません。\n", modelfile);
				}
				else
				{
					Tracker1ResultText += string.Format("[Text Comparison] OCR model ({0}) not found.\n", modelfile);
				}
				return false;
			}
			useCUDA = pfile.FontOCRUseCuda;
			if (CheckEncrypt(modelfile))
			{
				if (ReadDecryptModel(m_DeltaAIProcess_OCR, modelfile))
				{
					return true;
				}
				return false;
			}
			ReadModel(m_DeltaAIProcess_OCR, modelfile, useCUDA);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MT02-FontTool-ReadModel()-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(string modelname, IntPtr DeltaAIProcess)
	{
		try
		{
			if (!ModelACreatInst)
			{
				CreatInst();
			}
			if (!DllExist)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 找不到AI DLL({0})。\n", "DeltaAIProcessDll.dll");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比對] 找不到AI DLL({0})。\n", "DeltaAIProcessDll.dll");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] AI DLL({0})が見つかりません。\n", "DeltaAIProcessDll.dll");
				}
				else
				{
					Tracker1ResultText += string.Format("[Text Comparison] AI DLL({0}) not found.\n", "DeltaAIProcessDll.dll");
				}
				return false;
			}
			if (modelname == "")
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 尚未設定AI字元檢測模型。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文本比对] 尚未设置AI字符检测模型。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] AI文字検出モデルがまだ設定されていません。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("[FontTool] AI character detection model is not set yet.\n");
				}
				return false;
			}
			string text = "";
			text = ((!pfile.FontToolParam.FontDBUsePublicMode) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}{1}", text, modelname);
			if (!File.Exists(text2))
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("[文字比對] 找不到AI字元檢測模型檔案({0})。\n", text2);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("[文字比对] 找不到AI字符检测模型文件({0})。\n", text2);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("[文字比較] AI文字検出モデルファイル({0})が見つかりません。\n", text2);
				}
				else
				{
					Tracker1ResultText += string.Format("[Text Comparison] AI character detection model file ({0}) not found.\n", text2);
				}
				return false;
			}
			useCUDA = pfile.FontOCRUseCuda;
			ReadModelONNX(DeltaAIProcess, text2, useCUDA);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MT02-FontTool-ReadModel()-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool GetAIOCRResult(Mat CheckImg, ref string ocrChar)
	{
		if (!OCRModelReaded)
		{
			string modelfile = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "OcrAllFonts.pb");
			OCRModelReaded = ReadOCRModel(modelfile);
		}
		if (!OCRModelReaded)
		{
			return false;
		}
		if (CheckImg == null)
		{
			return false;
		}
		if (CheckImg.Width <= 0 || CheckImg.Height <= 0)
		{
			return false;
		}
		return PredictOCR(CheckImg, ref ocrChar);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool PredictOCR(Mat CheckImg, ref string ocrChar)
	{
		try
		{
			Size dsize = new Size(28, 28);
			OCRMat = CheckImg.Resize(dsize);
			Cv2.CvtColor(OCRMat, OCRMatColor, ColorConversionCodes.GRAY2BGR);
			IntPtr ptr = PredictImage(m_DeltaAIProcess_OCR, OCRMatColor.CvPtr);
			OCRMatReslut = new Mat(ptr);
			double[] array = new double[37];
			double num = -1.0;
			int num2 = -1;
			for (int i = 0; i < 37; i++)
			{
				array[i] = OCRMatReslut.At<float>(0, i);
				if (array[i] > num)
				{
					num = array[i];
					num2 = i;
				}
			}
			if (num2 == -1)
			{
				return false;
			}
			ocrChar = GetOCRResultChar(num2);
			return true;
		}
		catch (Exception ex)
		{
			string text = "AT01-FontTool-PredictOCR()-01-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-004");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetOCRResultChar(int index)
	{
		return index switch
		{
			0 => "-", 
			1 => "0", 
			2 => "1", 
			3 => "2", 
			4 => "3", 
			5 => "4", 
			6 => "5", 
			7 => "6", 
			8 => "7", 
			9 => "8", 
			10 => "9", 
			11 => "A", 
			12 => "B", 
			13 => "C", 
			14 => "D", 
			15 => "E", 
			16 => "F", 
			17 => "G", 
			18 => "H", 
			19 => "I", 
			20 => "J", 
			21 => "K", 
			22 => "L", 
			23 => "M", 
			24 => "N", 
			25 => "O", 
			26 => "P", 
			27 => "Q", 
			28 => "R", 
			29 => "S", 
			30 => "T", 
			31 => "U", 
			32 => "V", 
			33 => "W", 
			34 => "X", 
			35 => "Y", 
			36 => "Z", 
			_ => "Z", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public int ReadOnnxType(string onnxname, int FontIndex)
	{
		SessionOptions sessionOptions = new SessionOptions
		{
			GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED
		};
		try
		{
			string text = ((!pfile.FontToolParam.FontDBUsePublicMode) ? string.Format("{0}Recipe\\{1}\\Font\\{2}", AppDomain.CurrentDomain.BaseDirectory, RecipeName, onnxname) : (AppDomain.CurrentDomain.BaseDirectory + "PublicDB\\Font\\" + onnxname));
			if (!File.Exists(text))
			{
				return -1;
			}
			FontOnnxModel[FontIndex] = new InferenceSession(text, sessionOptions);
			IReadOnlyDictionary<string, NodeMetadata> inputMetadata = FontOnnxModel[FontIndex].InputMetadata;
			string key = inputMetadata.Keys.First();
			int[] array = inputMetadata[key].Dimensions.ToArray();
			int num = array[2];
			int num2 = array[3];
			if (num2 == 28 && num == 28)
			{
				sessionOptions.Dispose();
				return 1;
			}
			if (num2 == 32 && num == 64)
			{
				AIMatchImageSize[FontIndex].Width = num2;
				AIMatchImageSize[FontIndex].Height = num;
				string arg = onnxname.Substring(0, 1);
				string path = string.Format("D:\\AI_FONT_Train\\train\\{0}\\ocr.pth", arg);
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				string path2 = string.Format("D:\\AI_FONT_Train\\train\\{0}\\ocr.onnx", arg);
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
			}
			else
			{
				if (num2 != 16 || num != 32)
				{
					AIMatchImageSize[FontIndex].Width = num2;
					AIMatchImageSize[FontIndex].Height = num;
					string arg2 = onnxname.Substring(0, 1);
					string path3 = string.Format("D:\\AI_FONT_Train\\train\\{0}\\ocr.pth", arg2);
					if (File.Exists(path3))
					{
						File.Delete(path3);
					}
					string path4 = string.Format("D:\\AI_FONT_Train\\train\\{0}\\ocr.onnx", arg2);
					if (File.Exists(path4))
					{
						File.Delete(path4);
					}
					sessionOptions.Dispose();
					return 1;
				}
				AIMatchImageSize[FontIndex].Width = 16;
				AIMatchImageSize[FontIndex].Height = 32;
			}
			FontModelReaded[FontIndex] = true;
			sessionOptions.Dispose();
			return 2;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MT02-FontTool-ReadOnnxType()-" + ex.ToString());
			sessionOptions.Dispose();
			return -1;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool GetAIMatchScore(string modelname, int FontIndex, Mat CheckImg, Mat Pattern, ref int Score)
	{
		if (Pattern.Width <= 0 || Pattern.Height <= 0)
		{
			if (buffers.LanguageType == 1)
			{
				Tracker1ResultText += string.Format("[文字比對] 錯誤，比對資料庫影像寬高小於0");
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText += string.Format("[文字比对] 错误，比对数据库影像宽高小于0");
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText += string.Format("[文字比較] エラー、比較データベース画像の幅と高さが 0 より小さい");
			}
			else
			{
				Tracker1ResultText += string.Format("[Text Comparison] Error, the width and height of the compared database image are less than 0");
			}
			return false;
		}
		if (CheckImg.Width <= 0 || CheckImg.Height <= 0)
		{
			if (buffers.LanguageType == 1)
			{
				Tracker1ResultText += string.Format("[文字比對] 錯誤，比對影像寬高小於0");
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText += string.Format("[文字比对] 错误，比对图像宽高小于0");
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText += string.Format("[文字比較] エラー、照合画像の幅と高さが0より小さい");
			}
			else
			{
				Tracker1ResultText += string.Format("[FontTool] Error, comparison image width and height are less than 0");
			}
			return false;
		}
		if (FontModelOnnxType[FontIndex] == 0)
		{
			FontModelOnnxType[FontIndex] = ReadOnnxType(modelname, FontIndex);
		}
		if (FontModelOnnxType[FontIndex] == 1)
		{
			try
			{
				Score = 0;
				return false;
			}
			catch (Exception ex)
			{
				string text = "MT02-FontTool-GetAIMatchScore()-";
				text += ex.ToString();
				if (!OffLine)
				{
					CVBuffer.SaveLog(text);
				}
				else
				{
					CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-005");
				}
				return false;
			}
		}
		if (FontModelOnnxType[FontIndex] == 2)
		{
			try
			{
				double num = 0.0;
				AIPredictMat = CheckImg;
				num = PredictOnnx(FontOnnxModel[FontIndex], AIPredictMat, FontIndex);
				Score = (int)(num * 100.0);
				return true;
			}
			catch (Exception ex2)
			{
				string text2 = "MT02-FontTool-GetAIMatchScore()-";
				text2 += ex2.ToString();
				if (!OffLine)
				{
					CVBuffer.SaveLog(text2);
				}
				else
				{
					CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-006");
				}
				return false;
			}
		}
		return false;
	}

	public Rect GetFontMatchRect(int index)
	{
		return FontMatchRect[index];
	}

	public Rect GetFontSearchRect(int index)
	{
		return FontSearchRect[index];
	}

	public Rect GetAllFontMatchRect()
	{
		return FoundMatchTextRect;
	}

	public string GetFontChar(int index)
	{
		return FontChar[index];
	}

	public int GetMaxFontCount()
	{
		return pfile.FontToolParam.AllFont.Length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CheckEncrypt(string _filename)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		int num = new Random(Guid.NewGuid().GetHashCode()).Next();
		string text = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num);
		File.Copy(_filename, text, overwrite: true);
		BinaryReader binaryReader = new BinaryReader(new FileStream(text, FileMode.Open));
		byte[] array = binaryReader.ReadBytes(16);
		if (array[0] == 1 && array[4] == 2 && array[14] == 8 && array[15] == 64)
		{
			binaryReader.Close();
			File.Delete(text);
			return true;
		}
		binaryReader.Close();
		File.Delete(text);
		return false;
	}

	public void PbDecrypt(string _filename, string dst_name)
	{
		BinaryReader binaryReader = new BinaryReader(new FileStream(_filename, FileMode.Open));
		binaryReader.BaseStream.Seek(16L, SeekOrigin.Begin);
		long num = binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
		byte[] buffer = binaryReader.ReadBytes((int)num);
		binaryReader.Close();
		BinaryWriter binaryWriter = new BinaryWriter(new FileStream(dst_name, FileMode.Create));
		binaryWriter.Write(buffer);
		binaryWriter.Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ReadDecryptModel(IntPtr m_DeltaAIProcess, string PbFile)
	{
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			int num = new Random(Guid.NewGuid().GetHashCode()).Next();
			string text = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num);
			File.Copy(PbFile, text, overwrite: true);
			string text2 = string.Format("{0}{1}.tmp", AppDomain.CurrentDomain.BaseDirectory, num);
			PbDecrypt(text, text2);
			if (File.Exists(text2))
			{
				useCUDA = pfile.FontOCRUseCuda;
				ReadModel(m_DeltaAIProcess, text2, useCUDA);
				File.Delete(text2);
				File.Delete(text);
				return true;
			}
			File.Delete(text);
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ReadCryptModel()-05-" + ex.ToString());
			return false;
		}
	}

	public bool Check_Font(int index, Mat mat, Mat Pattern, ref string str, ref Mat fontbinary, int findex = 0)
	{
		switch (index)
		{
		case 0:
			return Check_R(mat, Pattern, ref str, ref fontbinary, findex);
		case 1:
			return Check_M(mat, Pattern, ref str, ref fontbinary, findex);
		case 2:
			return Check_C(mat, Pattern, ref str, ref fontbinary, findex);
		case 3:
			return Check_U(mat, Pattern, ref str, ref fontbinary, findex);
		case 4:
			return Check_Dash(mat, Pattern, ref str, ref fontbinary, findex);
		case 5:
			return Check_0(mat, Pattern, ref str, ref fontbinary, findex);
		case 6:
			return Check_1(mat, Pattern, ref str, ref fontbinary, findex);
		case 7:
			return Check_2(mat, Pattern, ref str, ref fontbinary, findex);
		case 8:
			return Check_3(mat, Pattern, ref str, ref fontbinary, findex);
		case 9:
			return Check_4(mat, Pattern, ref str, ref fontbinary, findex);
		case 10:
			return Check_5(mat, Pattern, ref str, ref fontbinary, findex);
		case 11:
			return Check_6(mat, Pattern, ref str, ref fontbinary, findex);
		case 12:
			return Check_7(mat, Pattern, ref str, ref fontbinary, findex);
		case 13:
			return Check_8(mat, Pattern, ref str, ref fontbinary, findex);
		case 14:
			return Check_9(mat, Pattern, ref str, ref fontbinary, findex);
		case 15:
			return Check_X(mat, Pattern, ref str, ref fontbinary, findex);
		case 16:
			return Check_B(mat, Pattern, ref str, ref fontbinary, findex);
		case 17:
			return Check_E(mat, Pattern, ref str, ref fontbinary, findex);
		case 18:
			return Check_D(mat, Pattern, ref str, ref fontbinary, findex);
		case 19:
			return Check_A(mat, Pattern, ref str, ref fontbinary, findex);
		default:
		{
			int num = index - 20;
			if (num < 0 || num >= 20)
			{
				num = 0;
			}
			return Check_OtherFont(num, GetFontPara(index), mat, Pattern, ref str, ref fontbinary, findex);
		}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_OtherFont(int id, string FontStr, Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.OtherFont[id].T_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.OtherFont[id].T_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.OtherFont[id].T_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.OtherFont[id].T_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.OtherFont[id].T_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.OtherFont[id].T_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.OtherFont[id].T_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.OtherFont[id].T_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.OtherFont[id].T_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount2);
				}
				if (pfile.FontToolParam.OtherFont[id].T_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount);
				}
				else if (pfile.FontToolParam.OtherFont[id].T_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.OtherFont[id].T_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.OtherFont[id].T_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary3 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary4 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary5 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary6 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary7 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary8 = new Mat(mat2, rect4);
			Rect rect9 = FindMaxBlob(binary5);
			Rect rect10 = FindMaxBlob(binary6);
			Rect rect11 = FindMaxBlob(binary7);
			Rect rect12 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.OtherFont[id].T_LT_H || TrainAIFontParamMode) && (double)rect5.Height > (double)rect9.Height * 2.5)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]左上角高度異常 ({1}-01)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]左上角高度异常 ({1}-01)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]左上角の高さ異常 ({1}-01)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Top-left Height Abnormal ({1}-01)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_LT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]左上角寬度異常 ({1}-02)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]左上角寬度异常 ({1}-02)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]左上角の幅異常 ({1}-02)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Top-left Width Abnormal ({1}-02)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_RT_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]右上角高度異常 ({1}-03)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]右上角高度异常 ({1}-03)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]右上角の高さ異常 ({1}-03)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Top-right Height Abnormal ({1}-03)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_RT_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]右上角寬度異常 ({1}-04)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]右上角寬度异常 ({1}-04)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]右上角の幅異常 ({1}-04)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Top-right Width Abnormal ({1}-04)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_LB_H || TrainAIFontParamMode) && (double)rect7.Height > (double)rect11.Height * 2.5)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]左下角高度異常 ({1}-05)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]左下角高度异常 ({1}-05)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]左下角の高さ異常 ({1}-05)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-left Height Abnormal ({1}-05)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_LB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]左下角寬度異常 ({1}-06)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]左下角寬度异常 ({1}-06)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]左下角の幅異常 ({1}-06)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-left Width Abnormal ({1}-06)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_RB_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect12.Height * 0.7)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]右下角高度異常 ({1}-07)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]右下角高度异常 ({1}-07)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]右下角の高さ異常 ({1}-07)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-right Height Abnormal ({1}-07)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_RB_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect12.Width * 0.6)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元{0}]右下角寬度異常 ({1}-08)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符{0}]右下角寬度异常 ({1}-08)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字{0}]右下角の幅異常 ({1}-08)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-right Width Abnormal ({1}-08)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元[{0}]找到斷字異常 ({1}-09)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符[{0}]找到断字异常 ({1}-09)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字[{0}]で断字異常 ({1}-09) を検出しました\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character[{0}] found hyphenation exception ({1}-09)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary9 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary10 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 3, rect2.Width, rect2.Height / 3);
			rect4 &= rect2;
			Mat binary11 = new Mat(mat, rect4);
			Rect rect13 = FindMaxBlob(binary9);
			Rect rect14 = FindMaxBlob(binary10);
			Rect rect15 = FindMaxBlob(binary11);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary12 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary13 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 3, rect.Width, rect.Height / 3);
			rect4 &= rect;
			Mat binary14 = new Mat(mat2, rect4);
			Rect rect16 = FindMaxBlob(binary12);
			Rect rect17 = FindMaxBlob(binary13);
			Rect rect18 = FindMaxBlob(binary14);
			if ((pfile.FontToolParam.OtherFont[id].T_T_W || TrainAIFontParamMode) && (double)rect13.Width < (double)rect16.Width * 0.7)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元[{0}]上方寬度異常 ({1}-10)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符[{0}]上方宽度异常 ({1}-10)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字[{0}]の上の幅が異常です ({1}-10)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character[{0}] top width is abnormal ({1}-10)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_B_W || TrainAIFontParamMode) && (double)rect14.Width < (double)rect17.Width * 0.7)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元[{0}]下方寬度異常 ({1}-11)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符[{0}]下方宽度异常 ({1}-11)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字[{0}]の下の幅が異常です ({1}-11)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character[{0}] bottom width is abnormal ({1}-11)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.OtherFont[id].T_C_W || TrainAIFontParamMode) && (double)rect15.Width < (double)rect18.Width * 0.7)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += string.Format("[字元分析]  字元[{0}]中心寬度異常 ({1}-11)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += string.Format("[字符分析]  字符[{0}]中心宽度异常 ({1}-11)\n", FontStr, FontStr);
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += string.Format("[文字分析]  文字[{0}]の中央幅が異常です ({1}-11)\n", FontStr, FontStr);
				}
				else
				{
					resultstr += string.Format("[Font Analysis]  Character[{0}] bottom width is abnormal ({1}-11)\n", FontStr, FontStr);
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_OtherFont()-" + FontStr;
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-007");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_R(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 2 || image.Height <= 10 || pattern.Width <= 2 || pattern.Height <= 10)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += "[字元分析]  字元[R]影像過小(寬小於2，高小於10)\n";
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += "[字符分析]  字符[R]图像过小(宽小于2，高小于10)\n";
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += "[文字分析]  文字[R]画像が小さすぎる(幅が2未満、高さが10未満)\n";
				}
				else
				{
					resultstr += "[Character analysis]  Character[R] image is too small (width less than 2, height less than 10)\n";
				}
				return false;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
					FontParaTemp[i] = 0f;
				}
			}
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.R_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.R_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.R_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.R_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.R_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.R_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.R_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.R_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.R_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount2);
				}
				if (pfile.FontToolParam.R_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount);
				}
				else if (pfile.FontToolParam.R_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.R_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.R_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect = default(Rect);
			Rect rect2 = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect3 = new Rect(0, 0, image.Width, image.Height);
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary2 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 5 * 4, rect3.Width / 2, rect3.Height / 5);
			rect &= rect3;
			Mat binary3 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top + rect3.Height / 5 * 4, rect3.Width / 2, rect3.Height / 5);
			rect &= rect3;
			Mat binary4 = new Mat(mat, rect);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary5 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary6 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 5 * 4, rect2.Width / 2, rect2.Height / 5);
			rect &= rect2;
			Mat binary7 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 5 * 4, rect2.Width / 2, rect2.Height / 5);
			rect &= rect2;
			Mat binary8 = new Mat(mat2, rect);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			Rect rect10 = FindMaxBlob(binary7);
			Rect rect11 = FindMaxBlob(binary8);
			string text = "R";
			if (pfile.FontToolParam.R_LT_H || TrainAIFontParamMode)
			{
				if (rect8.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]左上角高度異常 ({1}-01)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]左上角高度异常 ({1}-01)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]左上角の高さ異常 ({1}-01)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Top-left Height Abnormal ({1}-01)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect4.Height / (double)rect8.Height <= pfile.FontToolParam.R_LT_H_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]左上角高度異常 ({1}-01)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]左上角高度异常 ({1}-01)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]左上角の高さ異常 ({1}-01)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Top-left Height Abnormal ({1}-01)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect4.Height / (float)rect8.Height;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_LT_W || TrainAIFontParamMode)
			{
				if (rect8.Width <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]左上角寬度異常 ({1}-02)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]左上角寬度异常 ({1}-02)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]左上角の幅異常 ({1}-02)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Top-left Width Abnormal ({1}-02)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect4.Width / (double)rect8.Width <= pfile.FontToolParam.R_LT_W_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]左上角寬度異常 ({1}-02)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]左上角寬度异常 ({1}-02)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]左上角の幅異常 ({1}-02)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Top-left Width Abnormal ({1}-02)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect4.Width / (float)rect8.Width;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_RT_H || TrainAIFontParamMode)
			{
				if (rect9.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]右上角高度異常 ({1}-03)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]右上角高度异常 ({1}-03)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]右上角の高さ異常 ({1}-03)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Top-right Height Abnormal ({1}-03)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect5.Height / (double)rect9.Height <= pfile.FontToolParam.R_RT_H_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]右上角高度異常 ({1}-03)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]右上角高度异常 ({1}-03)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]右上角の高さ異常 ({1}-03)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Top-right Height Abnormal ({1}-03)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect5.Height / (float)rect9.Height;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_RT_W || TrainAIFontParamMode)
			{
				if (rect9.Width <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]右上角寬度異常 ({1}-04)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]右上角寬度异常 ({1}-04)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]右上角の幅異常 ({1}-04)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Top-right Width Abnormal ({1}-04)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect5.Width / (double)rect9.Width <= pfile.FontToolParam.R_RT_W_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]右上角寬度異常 ({1}-04)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]右上角寬度异常 ({1}-04)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]右上角の幅異常 ({1}-04)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Top-right Width Abnormal ({1}-04)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect5.Width / (float)rect9.Width;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_LB_H || TrainAIFontParamMode)
			{
				if (rect10.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]左下角高度異常 ({1}-05)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]左下角高度异常 ({1}-05)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]左下角の高さ異常 ({1}-05)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-left Height Abnormal ({1}-05)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect6.Height / (double)rect10.Height <= pfile.FontToolParam.R_LB_H_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]左下角高度異常 ({1}-05)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]左下角高度异常 ({1}-05)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]左下角の高さ異常 ({1}-05)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-left Height Abnormal ({1}-05)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect6.Height / (float)rect10.Height;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_Area || TrainAIFontParamMode)
			{
				int num4 = mat.CountNonZero();
				int num5 = mat2.CountNonZero();
				if (num4 <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]面積異常 ({1}-06)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]面积异常 ({1}-06)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]面積異常 ({1}-06)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-left Height Abnormal ({1}-06)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					double num6 = (double)num4 / (double)num5;
					if (num6 <= pfile.FontToolParam.R_Area_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]面積異常({1:F}) ({2}-06)\n", text, num6, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]面积异常({1:F}) ({2}-06)\n", text, num6, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]面積異常({1:F}) ({2}-06)\n", text, num6, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-left Height Abnormal({1:F}) ({2}-06)\n", text, num6, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)num6;
				}
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary9 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 10 * 9, rect3.Width, rect3.Height - rect3.Height / 10 * 9);
			rect &= rect3;
			Mat binary10 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 3, rect3.Height);
			rect &= rect3;
			Mat binary11 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 5 * 2, rect3.Top, rect3.Width / 5 * 3, rect3.Height);
			rect &= rect3;
			Mat binary12 = new Mat(mat, rect);
			Rect rect12 = FindMaxBlob(binary9);
			Rect rect13 = FindMaxBlob(binary10);
			Rect rect14 = FindMaxBlob(binary11);
			Rect rect15 = FindMaxBlob(binary12);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary13 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 10 * 9, rect2.Width, rect2.Height - rect2.Height / 10 * 9);
			rect &= rect2;
			Mat binary14 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 3, rect2.Height);
			rect &= rect2;
			Mat binary15 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 5 * 2, rect2.Top, rect2.Width / 5 * 3, rect2.Height);
			rect &= rect2;
			Mat binary16 = new Mat(mat2, rect);
			Rect rect16 = FindMaxBlob(binary13);
			FindMaxBlob(binary14);
			Rect rect17 = FindMaxBlob(binary15);
			Rect rect18 = FindMaxBlob(binary16);
			if (pfile.FontToolParam.R_T_W || TrainAIFontParamMode)
			{
				if (rect16.Width <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元[{0}]上方寬度異常 ({1}-07)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符[{0}]上方宽度异常 ({1}-07)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字[{0}]の上の幅が異常です ({1}-07)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character[{0}] top width is abnormal ({1}-07)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect12.Width / (double)rect16.Width <= pfile.FontToolParam.R_T_W_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元[{0}]上方寬度異常 ({1}-07)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符[{0}]上方宽度异常 ({1}-07)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字[{0}]の上の幅が異常です ({1}-07)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character[{0}] top width is abnormal ({1}-07)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect12.Width / (float)rect16.Width;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_B_W || TrainAIFontParamMode)
			{
				if (rect2.Width <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元[{0}]下方寬度異常 ({1}-08)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符[{0}]下方宽度异常 ({1}-08)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字[{0}]の下の幅が異常です ({1}-08)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character[{0}] bottom width is abnormal ({1}-08)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect13.Width / (double)rect2.Width >= pfile.FontToolParam.R_B_W_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元[{0}]下方寬度異常 ({1}-08)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符[{0}]下方宽度异常 ({1}-08)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字[{0}]の下の幅が異常です ({1}-08)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character[{0}] bottom width is abnormal ({1}-08)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect13.Width / (float)rect2.Width;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_L_H || TrainAIFontParamMode)
			{
				if (rect17.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += "[字元分析]  字元[R]左邊高度過短 (R-09)\n";
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += "[字符分析]  字符[R]左边高度过短 (R-09)\n";
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += "[文字分析]  文字[R]の左側の高さは短すぎます (R-09)\n";
					}
					else
					{
						resultstr += "[Character analysis]  Character [R] left side height is too short (R-09)\n";
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect14.Height / (double)rect17.Height <= pfile.FontToolParam.R_L_H_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]左邊高度過短 (R-09)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析]  字符[R]左边高度过短 (R-09)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析]  文字[R]の左側の高さは短すぎます (R-09)\n";
						}
						else
						{
							resultstr += "[Character analysis]  Character [R] left side height is too short (R-09)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect14.Height / (float)rect17.Height;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_R_H || TrainAIFontParamMode)
			{
				if (rect18.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += "[字元分析]  字元[R]右邊高度過短 (R-10)\n";
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += "[字符分析]  字符[R]右边高度过短 (R-10)\n";
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += "[文字分析]  文字[R]の右側の高さは短すぎます (R-10)\n";
					}
					else
					{
						resultstr += "[Character analysis]  Character [R] right side height is too short (R-10)\n";
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect15.Height / (double)rect18.Height <= pfile.FontToolParam.R_R_H_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]右邊高度過短 (R-10)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析]  字符[R]右边高度过短 (R-10)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析]  文字[R]の右側の高さは短すぎます (R-10)\n";
						}
						else
						{
							resultstr += "[Character analysis]  Character [R] right side height is too short (R-10)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect15.Height / (float)rect18.Height;
				}
			}
			FontCheckCount++;
			Rect roi = new Rect(image.Width / 3, image.Height / 5, image.Width / 3, image.Height / 5 * 3);
			roi &= rect3;
			Mat mat3 = new Mat(mat, roi);
			roi = new Rect(pattern.Width / 3, pattern.Height / 5, pattern.Width / 3, pattern.Height / 5 * 3);
			roi &= rect2;
			Mat mat4 = new Mat(mat2, roi);
			Cv2.BitwiseNot(mat3, mat3);
			Cv2.BitwiseNot(mat4, mat4);
			Rect rect19 = FindMaxBlob(mat3);
			Rect rect20 = FindMaxBlob(mat4);
			if ((pfile.FontToolParam.R_C || TrainAIFontParamMode) && ContNum < 2)
			{
				if (buffers.LanguageType == 1)
				{
					resultstr += "[字元分析]  字元[R]中心異常 (R-11)\n";
				}
				else if (buffers.LanguageType == 2)
				{
					resultstr += "[字符分析]  字符[R]中心异常 (R-11)\n";
				}
				else if (buffers.LanguageType == 3)
				{
					resultstr += "[文字分析]  文字[R]中心異常 (R-11)\n";
				}
				else
				{
					resultstr += "[Font analysis]Center Anomaly (R-11)\n";
				}
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_C_H_MIN || TrainAIFontParamMode)
			{
				if (rect20.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += "[字元分析]  字元[R]字元中心高度過短 (R-12)\n";
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += "[字符分析]  字符[R]字符中心高度过短 (R-12)\n";
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += "[文字分析]  文字[R]文字中心の高さが短すぎる (R-12)\n";
					}
					else
					{
						resultstr += "[Font analysis]Character center height is too short (R-12)\n";
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect19.Height / (double)rect20.Height <= pfile.FontToolParam.R_C_H_MIN_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]字元中心高度過短 (R-12)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析]  字符[R]字符中心高度过短 (R-12)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析]  文字[R]文字中心の高さが短すぎる (R-12)\n";
						}
						else
						{
							resultstr += "[Font analysis]Character center height is too short (R-12)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					if ((double)rect19.Height / (double)rect20.Height >= pfile.FontToolParam.R_C_H_MAX_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]字元中心高度過長 (R-12)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析]  字符[R]字符中心高度过长 (R-12)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析]  文字[R]文字中心の高さが長すぎる (R-12)\n";
						}
						else
						{
							resultstr += "[Font analysis]Character center height is too long (R-12)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect19.Height / (float)rect20.Height;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_LR_H_Diff || TrainAIFontParamMode)
			{
				if (rect14.Height <= 0 || rect15.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += "[字元分析]  字元[R]左右邊高度比例異常 (R-13)\n";
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += "[字符分析] 字符[R]左右两边高度比例异常 (R-13)\n";
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += "[文字分析] 文字[R]の左右の高さの比率が異常です (R-13)\n";
					}
					else
					{
						resultstr += "[Character analysis] Character [R] has abnormal height ratio between left and right sides (R-13)\n";
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else if (rect14.Height > rect15.Height)
				{
					if ((double)rect15.Height / (double)rect14.Height <= pfile.FontToolParam.R_LR_H_Diff_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]左右邊高度比例異常 (R-13)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析] 字符[R]左右两边高度比例异常 (R-13)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析] 文字[R]の左右の高さの比率が異常です (R-13)\n";
						}
						else
						{
							resultstr += "[Character analysis] Character [R] has abnormal height ratio between left and right sides (R-13)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect15.Height / (float)rect14.Height;
				}
				else
				{
					if ((double)rect14.Height / (double)rect15.Height <= pfile.FontToolParam.R_LR_H_Diff_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]左右邊高度比例異常 (R-13)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析] 字符[R]左右两边高度比例异常 (R-13)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析] 文字[R]の左右の高さの比率が異常です (R-13)\n";
						}
						else
						{
							resultstr += "[Character analysis] Character [R] has abnormal height ratio between left and right sides (R-13)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect14.Height / (float)rect15.Height;
				}
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_Break || TrainAIFontParamMode)
			{
				if (num3 <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += "[字元分析]  字元[R]斷字異常 (邊緣過小) (R-14)\n";
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += "[字符分析]  字符[R]断字异常 (边缘过小) (R-14)\n";
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += "[文字分析]  文字[R]の断字異常 (余白が小さい) (R-14)\n";
					}
					else
					{
						resultstr += "[Font analysis] Character[R] hyphenation exception (edge too small) (R-14)\n";
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)num2 / (double)num3 <= pfile.FontToolParam.R_Break_PMin)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]斷字異常 (邊緣過小) (R-14)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析]  字符[R]断字异常 (边缘过小) (R-14)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析]  文字[R]の断字異常 (余白が小さい) (R-14)\n";
						}
						else
						{
							resultstr += "[Font analysis] Character[R] hyphenation exception (edge too small) (R-14)\n";
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					if ((double)num2 / (double)num3 >= pfile.FontToolParam.R_Break_PMax)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += "[字元分析]  字元[R]斷字異常 (邊緣過大) (R-14)\n";
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += "[字符分析]  字符[R]断字异常 (边缘过大) (R-14)\n";
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += "[文字分析]  文字[R]の断字異常 (余白が大きすぎる) (R-14)\n";
						}
						else
						{
							resultstr += "[Font analysis] Character[R] hyphenation exception (edge too large) (R-14)\n";
						}
						resultstr += "[字元分析]  字元[R]斷字異常 (邊緣過大) (R-14)\n";
						result = (FontCheckResult[FontCheckCount] = false);
					}
				}
				FontParaResult[FontCheckCount] = (float)num2 / (float)num3;
			}
			FontCheckCount++;
			if (pfile.FontToolParam.R_RB_H || TrainAIFontParamMode)
			{
				if (rect11.Height <= 0)
				{
					if (buffers.LanguageType == 1)
					{
						resultstr += string.Format("[字元分析]  字元{0}]右下角高度異常 ({1}-15)\n", text, text);
					}
					else if (buffers.LanguageType == 2)
					{
						resultstr += string.Format("[字符分析]  字符{0}]右下角高度异常 ({1}-15)\n", text, text);
					}
					else if (buffers.LanguageType == 3)
					{
						resultstr += string.Format("[文字分析]  文字{0}]右下角の高さ異常 ({1}-15)\n", text, text);
					}
					else
					{
						resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-right Height Abnormal ({1}-15)\n", text, text);
					}
					result = (FontCheckResult[FontCheckCount] = false);
				}
				else
				{
					if ((double)rect7.Height / (double)rect11.Height <= pfile.FontToolParam.R_RB_H_P)
					{
						if (buffers.LanguageType == 1)
						{
							resultstr += string.Format("[字元分析]  字元{0}]右下角高度異常 ({1}-15)\n", text, text);
						}
						else if (buffers.LanguageType == 2)
						{
							resultstr += string.Format("[字符分析]  字符{0}]右下角高度异常 ({1}-15)\n", text, text);
						}
						else if (buffers.LanguageType == 3)
						{
							resultstr += string.Format("[文字分析]  文字{0}]右下角の高さ異常 ({1}-15)\n", text, text);
						}
						else
						{
							resultstr += string.Format("[Font Analysis]  Character {0}] Bottom-right Height Abnormal ({1}-15)\n", text, text);
						}
						result = (FontCheckResult[FontCheckCount] = false);
					}
					FontParaResult[FontCheckCount] = (float)rect7.Height / (float)rect11.Height;
				}
			}
			FontCheckCount++;
			return result;
		}
		catch (Exception ex)
		{
			string text2 = "BT02-FontTool-Check_R()-";
			text2 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-008");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_M(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.M_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.M_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.M_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.M_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.M_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.M_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.M_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.M_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.M_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount2);
				}
				if (pfile.FontToolParam.M_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount);
				}
				else if (pfile.FontToolParam.M_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.M_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.M_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			Rect SecondRect = new Rect(0, 0, 0, 0);
			FindMaxBlob(mat, ref SecondRect);
			int num2 = FindMaxContours(mat);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect3 &= rect2;
			Mat binary2 = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect3 &= rect;
			Mat binary3 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height / 4);
			rect3 &= rect;
			Mat binary4 = new Mat(mat2, rect3);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			if ((pfile.FontToolParam.M_T_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect6.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[M]上方寬度異常 (M-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.M_B_W || TrainAIFontParamMode) && rect5.Width > rect7.Width * 2)
			{
				resultstr += "[字元分析]  字元[M]下方寬度異常 (M-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Mat binary5 = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height));
			Rect rect8 = FindMaxBlob(binary5);
			Mat binary6 = new Mat(mat2, new Rect(rect.Left, rect.Top, rect.Width, rect.Height));
			Rect rect9 = FindMaxBlob(binary6);
			if ((pfile.FontToolParam.M_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect9.Width * 0.8)
			{
				resultstr += "[字元分析]  字元[M]寬度異常 (M-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.M_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.8)
			{
				resultstr += "[字元分析]  字元[M]斷字異常 (M-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 3, rect2.Height);
			rect3 &= rect2;
			Mat binary7 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 3, rect2.Top, rect2.Width / 3, rect2.Height);
			rect3 &= rect2;
			Mat binary8 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 3 * 2, rect2.Top, rect2.Width / 3, rect2.Height);
			rect3 &= rect2;
			Mat binary9 = new Mat(mat, rect3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 3, rect.Height);
			rect3 &= rect;
			Mat binary10 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 3, rect.Top, rect.Width / 3, rect.Height);
			rect3 &= rect;
			Mat binary11 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 3 * 2, rect.Top, rect.Width / 3, rect.Height);
			rect3 &= rect;
			Mat binary12 = new Mat(mat2, rect3);
			Rect rect10 = FindMaxBlob(binary7);
			Rect rect11 = FindMaxBlob(binary8);
			Rect rect12 = FindMaxBlob(binary9);
			Rect rect13 = FindMaxBlob(binary10);
			Rect rect14 = FindMaxBlob(binary11);
			Rect rect15 = FindMaxBlob(binary12);
			if ((pfile.FontToolParam.M_Leg1 || TrainAIFontParamMode) && (double)rect10.Height < (double)rect13.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[M]第1隻腳高度異常 (M-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.M_Leg2 || TrainAIFontParamMode) && (double)rect11.Height < (double)rect14.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[M]第2隻腳高度異常 (M-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.M_Leg3 || TrainAIFontParamMode) && (double)rect12.Height < (double)rect15.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[M]第3隻腳高度異常 (M-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Cv2.BitwiseNot(mat, mat);
			mat.Rectangle(new Rect(0, 0, mat.Width, mat.Height), Scalar.White);
			Rect SecondRect2 = new Rect(0, 0, 0, 0);
			FindMaxBlob(mat, ref SecondRect2, FilterSize: false);
			if ((pfile.FontToolParam.M_Connected || TrainAIFontParamMode) && (SecondRect2.Width > 3 || SecondRect2.Height > 3))
			{
				resultstr += "[字元分析]  字元[M]文字沾黏異常 (M-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_M-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-009");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_C(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.C_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.C_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.C_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.C_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.C_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.C_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.C_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.C_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.C_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount2);
				}
				if (pfile.FontToolParam.C_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount);
				}
				else if (pfile.FontToolParam.C_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.C_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.C_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			Rect SecondRect = new Rect(0, 0, 0, 0);
			FindMaxBlob(mat, ref SecondRect);
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
			rect3 &= rect;
			Mat binary2 = new Mat(mat2, rect3);
			Rect rect5 = FindMaxBlob(binary2);
			if ((pfile.FontToolParam.C_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect5.Width * 0.8)
			{
				resultstr += "[字元分析]  字元[C]寬度異常 (C-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.C_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect5.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[C]高度異常 (C-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect3 &= rect2;
			Mat binary3 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height - rect2.Height / 4 * 3);
			rect3 &= rect2;
			Mat binary4 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 3, rect2.Height);
			rect3 &= rect2;
			Mat binary5 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 5 * 3, rect2.Top, rect2.Width / 5 * 2, rect2.Height);
			rect3 &= rect2;
			Mat binary6 = new Mat(mat, rect3);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect3 &= rect;
			Mat binary7 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height - rect.Height / 4 * 3);
			rect3 &= rect;
			Mat binary8 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 3, rect.Height);
			rect3 &= rect;
			Mat binary9 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 5 * 3, rect.Top, rect.Width / 5 * 2, rect.Height);
			rect3 &= rect;
			Mat binary10 = new Mat(mat2, rect3);
			Rect rect10 = FindMaxBlob(binary7);
			FindMaxBlob(binary8);
			Rect rect11 = FindMaxBlob(binary9);
			Rect rect12 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.C_T_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[C]上方寬度異常 (C-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.C_B_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[C]下方寬度異常 (C-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.C_L_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect11.Height * 0.75)
			{
				resultstr += "[字元分析]  字元[C]左邊高度異常 (C-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.C_R_H || TrainAIFontParamMode) && rect9.Height > rect12.Height * 2)
			{
				resultstr += "[字元分析]  字元[C]右邊高度異常 (C-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.C_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.88)
			{
				resultstr += "[字元分析]  字元[C]斷字異常 (C-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_C-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-010");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_U(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.U_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.U_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.U_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.U_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.U_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.U_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.U_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.U_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.U_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount2);
				}
				if (pfile.FontToolParam.U_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount);
				}
				else if (pfile.FontToolParam.U_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.U_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.U_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			Rect SecondRect = new Rect(0, 0, 0, 0);
			FindMaxBlob(mat, ref SecondRect);
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
			rect3 &= rect;
			Mat binary2 = new Mat(mat2, rect3);
			Rect rect5 = FindMaxBlob(binary2);
			if ((pfile.FontToolParam.U_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect5.Width * 0.8)
			{
				resultstr += "[字元分析]  字元[U]寬度異常 (U-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.U_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect5.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[U]高度異常 (U-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect3 &= rect2;
			Mat binary3 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height - rect2.Height / 4 * 3);
			rect3 &= rect2;
			Mat binary4 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 3, rect2.Height);
			rect3 &= rect2;
			Mat binary5 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 5 * 3, rect2.Top, rect2.Width / 5 * 2, rect2.Height);
			rect3 &= rect2;
			Mat binary6 = new Mat(mat, rect3);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect3 &= rect;
			Mat binary7 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height - rect.Height / 4 * 3);
			rect3 &= rect;
			Mat binary8 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 3, rect.Height);
			rect3 &= rect;
			Mat binary9 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 5 * 3, rect.Top, rect.Width / 5 * 2, rect.Height);
			rect3 &= rect;
			Mat binary10 = new Mat(mat2, rect3);
			Rect rect10 = FindMaxBlob(binary7);
			FindMaxBlob(binary8);
			Rect rect11 = FindMaxBlob(binary9);
			Rect rect12 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.U_T_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[U]上方寬度異常 (U-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.U_B_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[U]下方寬度異常 (U-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.U_L_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect11.Height * 0.75)
			{
				resultstr += "[字元分析]  字元[U]左邊高度異常 (U-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.U_R_H || TrainAIFontParamMode) && rect9.Height > rect12.Height * 2)
			{
				resultstr += "[字元分析]  字元[U]右邊高度異常 (U-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.U_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.88)
			{
				resultstr += "[字元分析]  字元[U]斷字異常 (U-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_U-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-011");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_X(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.X_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.X_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.X_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.X_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.X_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.X_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.X_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.X_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.X_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount2);
				}
				if (pfile.FontToolParam.X_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount);
				}
				else if (pfile.FontToolParam.X_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.X_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.X_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			Rect SecondRect = new Rect(0, 0, 0, 0);
			FindMaxBlob(mat, ref SecondRect);
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
			rect3 &= rect;
			Mat binary2 = new Mat(mat2, rect3);
			Rect rect5 = FindMaxBlob(binary2);
			if ((pfile.FontToolParam.X_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect5.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[X]寬度異常 (X-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.X_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect5.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[X]高度異常 (X-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 2);
			rect3 &= rect2;
			Mat binary3 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width, rect2.Height - rect2.Height / 2);
			rect3 &= rect2;
			Mat binary4 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height);
			rect3 &= rect2;
			Mat binary5 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height);
			rect3 &= rect2;
			Mat binary6 = new Mat(mat, rect3);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 2);
			rect3 &= rect;
			Mat binary7 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 2, rect.Width, rect.Height - rect.Height / 2);
			rect3 &= rect;
			Mat binary8 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height);
			rect3 &= rect;
			Mat binary9 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height);
			rect3 &= rect;
			Mat binary10 = new Mat(mat2, rect3);
			Rect rect10 = FindMaxBlob(binary7);
			FindMaxBlob(binary8);
			Rect rect11 = FindMaxBlob(binary9);
			Rect rect12 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.X_T_W || TrainAIFontParamMode) && (double)rect6.Width > (double)rect10.Width * 2.5)
			{
				resultstr += "[字元分析]  字元[X]上方寬度異常 (X-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.X_B_W || TrainAIFontParamMode) && (double)rect7.Width > (double)rect.Width * 2.5)
			{
				resultstr += "[字元分析]  字元[X]下方寬度異常 (X-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.X_L_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect11.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[X]左邊高度異常 (X-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.X_R_H || TrainAIFontParamMode) && (double)rect9.Height < (double)rect12.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[X]右邊高度異常 (X-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.X_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.88)
			{
				resultstr += "[字元分析]  字元[X]斷字異常 (X-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_X-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-012");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_B(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.TB_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.TB_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.TB_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.TB_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.TB_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.TB_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.TB_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.TB_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.TB_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount2);
				}
				if (pfile.FontToolParam.TB_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount);
				}
				else if (pfile.FontToolParam.TB_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TB_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.TB_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			int num2 = FindMaxContours(mat);
			int num3 = FindMaxContours(mat2);
			Rect rect = default(Rect);
			Rect rect2 = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect3 = new Rect(0, 0, image.Width, image.Height);
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary2 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary3 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary4 = new Mat(mat, rect);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary5 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary6 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary7 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary8 = new Mat(mat2, rect);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			Rect rect10 = FindMaxBlob(binary7);
			Rect rect11 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.TB_LT_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect8.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[B]左上角高度異常 (B-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_LT_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect8.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[B]左上角寬度異常 (B-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_RT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect9.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[B]右上角高度異常 (B-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_RT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[B]右上角寬度異常 (B-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_LB_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[B]左下角高度異常 (B-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_LB_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[B]左下角寬度異常 (B-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_RB_H || TrainAIFontParamMode) && (double)rect7.Height < (double)rect11.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[B]右下角高度異常 (B-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_RB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[B]右下角寬度異常 (B-0B)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 4 * 3, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary9 = new Mat(mat, rect);
			Rect rect12 = FindMaxBlob(binary9);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary10 = new Mat(mat2, rect);
			Rect rect13 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.TB_B_W || TrainAIFontParamMode) && (double)rect12.Width < (double)rect13.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[B]下方寬度異常 (B-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Rect roi = new Rect(image.Width / 3, image.Height / 5, image.Width / 3, image.Height / 5 * 3);
			roi &= rect3;
			Mat mat3 = new Mat(mat, roi);
			roi = new Rect(pattern.Width / 3, pattern.Height / 5, pattern.Width / 3, pattern.Height / 5 * 3);
			roi &= rect2;
			Mat mat4 = new Mat(mat2, roi);
			Cv2.BitwiseNot(mat3, mat3);
			Cv2.BitwiseNot(mat4, mat4);
			Rect rect14 = FindMaxBlob(mat3);
			Rect rect15 = FindMaxBlob(mat4);
			int num4 = (int)((double)rect15.Height * 0.7);
			int num5 = (int)((double)rect15.Height * 1.5);
			if ((pfile.FontToolParam.TB_C || TrainAIFontParamMode) && (rect14.Width <= 0 || rect14.Height <= 0))
			{
				resultstr += "[字元分析]  字元[B]中心異常 (B-10)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_C_H_MIN || TrainAIFontParamMode) && rect14.Height < num4)
			{
				resultstr += "[字元分析]  字元[B]字元中心高度過短 (B-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_C_H_MAX || TrainAIFontParamMode) && rect14.Height > num5)
			{
				resultstr += "[字元分析]  字元[B]字元中心高度過長 (B-12)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TB_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.88)
			{
				resultstr += "[字元分析]  字元[B]斷字異常 (B-13)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_B-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-013");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_E(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.TE_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.TE_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.TE_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.TE_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.TE_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.TE_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.TE_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.TE_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.TE_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount2);
				}
				if (pfile.FontToolParam.TE_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount);
				}
				else if (pfile.FontToolParam.TE_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.TE_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.TE_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary3 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary4 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary5 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary6 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary7 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary8 = new Mat(mat2, rect4);
			Rect rect9 = FindMaxBlob(binary5);
			Rect rect10 = FindMaxBlob(binary6);
			Rect rect11 = FindMaxBlob(binary7);
			Rect rect12 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.TE_LT_H || TrainAIFontParamMode) && (double)rect5.Height > (double)rect9.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[E]左上角高度異常 (E-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_LT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[E]左上角寬度異常 (E-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_RT_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[E]右上角高度異常 (E-0E)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_RT_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[E]右上角寬度異常 (E-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_LB_H || TrainAIFontParamMode) && (double)rect7.Height > (double)rect11.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[E]左下角高度異常 (E-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_LB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[E]左下角寬度異常 (E-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_RB_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect12.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[E]右下角高度異常 (E-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_RB_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect12.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[E]右下角寬度異常 (E-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.TE_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += "[字元分析]  字元[E]找到斷字異常 (E-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_E()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-014");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_Dash(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 2 || image.Height <= 2 || pattern.Width <= 2 || pattern.Height <= 2)
			{
				return false;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.Dash_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.Dash_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.Dash_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.Dash_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.Dash_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.Dash_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.Dash_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.Dash_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.Dash_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount2);
				}
				if (pfile.FontToolParam.Dash_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount);
				}
				else if (pfile.FontToolParam.Dash_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.Dash_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.Dash_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			Rect SecondRect = new Rect(0, 0, 0, 0);
			Rect rect3 = FindMaxBlob(mat, ref SecondRect);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
			rect4 &= rect;
			Mat binary2 = new Mat(mat2, rect4);
			Rect rect6 = FindMaxBlob(binary2);
			if ((pfile.FontToolParam.Dash_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect6.Height * 0.6)
			{
				resultstr += "[字元分析]  字元[-]高度異常 (DA-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.Dash_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect6.Width * 0.8)
			{
				resultstr += "[字元分析]  字元[-]寬度異常 (DA-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.Dash_Break || TrainAIFontParamMode) && SecondRect.Width > 0 && SecondRect.Height > 0)
			{
				resultstr += "[字元分析]  字元[-]找到斷字異常 (Da-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_Dash()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-015");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_D(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.D_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.D_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.D_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.D_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.D_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.D_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.D_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.D_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.D_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount2);
				}
				if (pfile.FontToolParam.D_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount);
				}
				else if (pfile.FontToolParam.D_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.D_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.D_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 2));
			Cv2.Dilate(mat, mat, structuringElement, null, 2);
			Cv2.Erode(mat, mat, structuringElement, null, 2);
			bool result = true;
			int num2 = FindMaxContours(mat);
			int num3 = FindMaxContours(mat2);
			Rect rect = default(Rect);
			Rect rect2 = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect3 = new Rect(0, 0, image.Width, image.Height);
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary2 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary3 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary4 = new Mat(mat, rect);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary5 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary6 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary7 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary8 = new Mat(mat2, rect);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			Rect rect10 = FindMaxBlob(binary7);
			Rect rect11 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.D_LT_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect8.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[D]左上角高度異常 (D-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_LT_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect8.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[D]左上角寬度異常 (D-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_RT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect9.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[D]右上角高度異常 (D-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_RT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[D]右上角寬度異常 (D-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_LB_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[D]左下角高度異常 (D-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_LB_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[D]左下角寬度異常 (D-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_RB_H || TrainAIFontParamMode) && (double)rect7.Height < (double)rect11.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[D]右下角高度異常 (D-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_RB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[D]右下角寬度異常 (D-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary9 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 4 * 3, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary10 = new Mat(mat, rect);
			Rect rect12 = FindMaxBlob(binary9);
			Rect rect13 = FindMaxBlob(binary10);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary11 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary12 = new Mat(mat2, rect);
			Rect rect14 = FindMaxBlob(binary11);
			Rect rect15 = FindMaxBlob(binary12);
			if ((pfile.FontToolParam.D_T_W || TrainAIFontParamMode) && (double)rect12.Width < (double)rect14.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[D]上方寬度異常 (D-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_B_W || TrainAIFontParamMode) && (double)rect13.Width < (double)rect15.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[D]下方寬度異常 (D-10)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Rect roi = new Rect(image.Width / 3, image.Height / 5, image.Width / 3, image.Height / 5 * 3);
			roi &= rect3;
			Mat mat3 = new Mat(mat, roi);
			roi = new Rect(pattern.Width / 3, pattern.Height / 5, pattern.Width / 3, pattern.Height / 5 * 3);
			roi &= rect2;
			Mat mat4 = new Mat(mat2, roi);
			Cv2.BitwiseNot(mat3, mat3);
			Cv2.BitwiseNot(mat4, mat4);
			Cv2.MorphologyEx(mat3, mat3, MorphTypes.Close, new Mat());
			Rect rect16 = FindMaxBlob(mat3);
			Rect rect17 = FindMaxBlob(mat4);
			int num4 = (int)((double)rect17.Height * 0.7);
			int num5 = (int)((double)rect17.Height * 1.5);
			if ((pfile.FontToolParam.D_C || TrainAIFontParamMode) && (rect16.Width <= 0 || rect16.Height <= 0))
			{
				resultstr += "[字元分析]  字元[D]中心異常 (D-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_C_H_MIN || TrainAIFontParamMode) && rect16.Height < num4)
			{
				resultstr += "[字元分析]  字元[D]字元中心高度過短 (D-12)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_C_H_MAX || TrainAIFontParamMode) && rect16.Height > num5)
			{
				resultstr += "[字元分析]  字元[D]字元中心高度過長 (D-13)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.D_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.75)
			{
				resultstr += "[字元分析]  字元[D]斷字異常 (D-14)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BD2-FontTool-Check_D()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-016");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_A(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.A_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.A_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.A_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.A_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.A_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.A_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.A_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.A_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.A_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount2);
				}
				if (pfile.FontToolParam.A_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount);
				}
				else if (pfile.FontToolParam.A_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.A_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.A_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			Rect SecondRect = new Rect(0, 0, 0, 0);
			FindMaxBlob(mat, ref SecondRect);
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
			rect3 &= rect;
			Mat binary2 = new Mat(mat2, rect3);
			Rect rect5 = FindMaxBlob(binary2);
			if ((pfile.FontToolParam.A_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect5.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[A]寬度異常 (A-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.A_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect5.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[A]高度異常 (A-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 2);
			rect3 &= rect2;
			Mat binary3 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width, rect2.Height - rect2.Height / 2);
			rect3 &= rect2;
			Mat binary4 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height);
			rect3 &= rect2;
			Mat binary5 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height);
			rect3 &= rect2;
			Mat binary6 = new Mat(mat, rect3);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 2);
			rect3 &= rect;
			Mat binary7 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 2, rect.Width, rect.Height - rect.Height / 2);
			rect3 &= rect;
			Mat binary8 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height);
			rect3 &= rect;
			Mat binary9 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height);
			rect3 &= rect;
			Mat binary10 = new Mat(mat2, rect3);
			Rect rect10 = FindMaxBlob(binary7);
			FindMaxBlob(binary8);
			Rect rect11 = FindMaxBlob(binary9);
			Rect rect12 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.A_T_W || TrainAIFontParamMode) && (double)rect6.Width > (double)rect10.Width * 2.5)
			{
				resultstr += "[字元分析]  字元[A]上方寬度異常 (A-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.A_B_W || TrainAIFontParamMode) && (double)rect7.Width > (double)rect.Width * 2.5)
			{
				resultstr += "[字元分析]  字元[A]下方寬度異常 (A-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.A_L_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect11.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[A]左邊高度異常 (A-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.A_R_H || TrainAIFontParamMode) && (double)rect9.Height < (double)rect12.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[A]右邊高度異常 (A-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.A_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.88)
			{
				resultstr += "[字元分析]  字元[A]斷字異常 (A-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_A-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-017");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_0(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T0_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T0_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T0_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T0_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T0_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T0_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T0_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T0_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T0_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T0_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T0_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T0_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T0_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 2));
			Cv2.Dilate(mat, mat, structuringElement, null, 2);
			Cv2.Erode(mat, mat, structuringElement, null, 2);
			bool result = true;
			int num2 = FindMaxContours(mat);
			int num3 = FindMaxContours(mat2);
			Rect rect = default(Rect);
			Rect rect2 = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect3 = new Rect(0, 0, image.Width, image.Height);
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary2 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary3 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary4 = new Mat(mat, rect);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary5 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary6 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary7 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary8 = new Mat(mat2, rect);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			Rect rect10 = FindMaxBlob(binary7);
			Rect rect11 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.T0_LT_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect8.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[0]左上角高度異常 (0-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_LT_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect8.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[0]左上角寬度異常 (0-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_RT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect9.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[0]右上角高度異常 (0-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_RT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[0]右上角寬度異常 (0-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_LB_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[0]左下角高度異常 (0-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_LB_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[0]左下角寬度異常 (0-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_RB_H || TrainAIFontParamMode) && (double)rect7.Height < (double)rect11.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[0]右下角高度異常 (0-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_RB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[0]右下角寬度異常 (0-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary9 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 4 * 3, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary10 = new Mat(mat, rect);
			Rect rect12 = FindMaxBlob(binary9);
			Rect rect13 = FindMaxBlob(binary10);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary11 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary12 = new Mat(mat2, rect);
			Rect rect14 = FindMaxBlob(binary11);
			Rect rect15 = FindMaxBlob(binary12);
			if ((pfile.FontToolParam.T0_T_W || TrainAIFontParamMode) && (double)rect12.Width < (double)rect14.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[0]上方寬度異常 (0-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_B_W || TrainAIFontParamMode) && (double)rect13.Width < (double)rect15.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[0]下方寬度異常 (0-10)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Rect roi = new Rect(image.Width / 3, image.Height / 5, image.Width / 3, image.Height / 5 * 3);
			roi &= rect3;
			Mat mat3 = new Mat(mat, roi);
			roi = new Rect(pattern.Width / 3, pattern.Height / 5, pattern.Width / 3, pattern.Height / 5 * 3);
			roi &= rect2;
			Mat mat4 = new Mat(mat2, roi);
			Cv2.BitwiseNot(mat3, mat3);
			Cv2.BitwiseNot(mat4, mat4);
			Cv2.MorphologyEx(mat3, mat3, MorphTypes.Close, new Mat());
			Rect rect16 = FindMaxBlob(mat3);
			Rect rect17 = FindMaxBlob(mat4);
			int num4 = (int)((double)rect17.Height * 0.7);
			int num5 = (int)((double)rect17.Height * 1.5);
			if ((pfile.FontToolParam.T0_C || TrainAIFontParamMode) && (rect16.Width <= 0 || rect16.Height <= 0))
			{
				resultstr += "[字元分析]  字元[0]中心異常 (0-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_C_H_MIN || TrainAIFontParamMode) && rect16.Height < num4)
			{
				resultstr += "[字元分析]  字元[0]字元中心高度過短 (0-12)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_C_H_MAX || TrainAIFontParamMode) && rect16.Height > num5)
			{
				resultstr += "[字元分析]  字元[0]字元中心高度過長 (0-13)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.75)
			{
				resultstr += "[字元分析]  字元[0]斷字異常 (0-14)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 3, rect3.Height);
			rect &= rect3;
			Mat binary13 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 5 * 3, rect3.Top, rect3.Width / 5 * 2, rect3.Height);
			rect &= rect3;
			Mat binary14 = new Mat(mat, rect);
			Rect rect18 = FindMaxBlob(binary13);
			Rect rect19 = FindMaxBlob(binary14);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 3, rect2.Height);
			rect &= rect2;
			Mat binary15 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 5 * 3, rect2.Top, rect2.Width / 5 * 2, rect2.Height);
			rect &= rect2;
			Mat binary16 = new Mat(mat2, rect);
			Rect rect20 = FindMaxBlob(binary15);
			Rect rect21 = FindMaxBlob(binary16);
			if ((pfile.FontToolParam.T0_L_H || TrainAIFontParamMode) && (double)rect18.Height < (double)rect20.Height * 0.75)
			{
				resultstr += "[字元分析]  字元[0]左邊高度異常 (0-15)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T0_R_H || TrainAIFontParamMode) && rect19.Height > rect21.Height * 2)
			{
				resultstr += "[字元分析]  字元[0]右邊高度異常 (0-16)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_0()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-018");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_1(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T1_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T1_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T1_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T1_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T1_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T1_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T1_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T1_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T1_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T1_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T1_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T1_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T1_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Mat binary = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height));
			Rect rect4 = FindMaxBlob(binary);
			Mat binary2 = new Mat(mat2, new Rect(rect.Left, rect.Top, rect.Width, rect.Height));
			Rect rect5 = FindMaxBlob(binary2);
			if ((pfile.FontToolParam.T1_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect5.Height * 0.75)
			{
				resultstr += "[字元分析]  字元[1]高度異常 (1-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if (pfile.FontToolParam.T1_W || TrainAIFontParamMode)
			{
				if ((double)rect4.Width < (double)rect5.Width * 0.6)
				{
					resultstr += "[字元分析]  字元[1]寬度異常(小) (1-02)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
				if ((double)rect4.Width > (double)rect5.Width * 1.3)
				{
					resultstr += "[字元分析]  字元[1]寬度異常(大) (1-02)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T1_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += "[字元分析]  字元[1]找到斷字異常 (1-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_1-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-019");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_2(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T2_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T2_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T2_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T2_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T2_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T2_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T2_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T2_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T2_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T2_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T2_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T2_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T2_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary3 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 4 * 3, rect2.Width / 2, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary4 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary5 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary6 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary7 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top + rect.Height / 4 * 3, rect.Width / 2, rect.Height / 4);
			rect4 &= rect;
			Mat binary8 = new Mat(mat2, rect4);
			Rect rect9 = FindMaxBlob(binary5);
			Rect rect10 = FindMaxBlob(binary6);
			Rect rect11 = FindMaxBlob(binary7);
			Rect rect12 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.T2_LT_H || TrainAIFontParamMode) && (double)rect5.Height > (double)rect9.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[2]左上角高度異常 (2-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_LT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[2]左上角寬度異常 (2-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_RT_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[2]右上角高度異常 (2-03)\n";
				result = false;
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_RT_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[2]右上角寬度異常 (2-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_LB_H || TrainAIFontParamMode) && (double)rect7.Height < (double)rect11.Height * 0.8)
			{
				resultstr += "[字元分析]  字元[2]左下角高度異常 (2-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_LB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[2]左下角寬度異常 (2-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_RB_H || TrainAIFontParamMode) && (double)rect8.Height > (double)rect12.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[2]右下角高度異常 (2-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_RB_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect12.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[2]右下角寬度異常 (2-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T2_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += "[字元分析]  字元[2]找到斷字異常 (2-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_2()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-020");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_3(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T3_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T3_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T3_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T3_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T3_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T3_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T3_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T3_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T3_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T3_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T3_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T3_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T3_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary3 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary4 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary5 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary6 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary7 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary8 = new Mat(mat2, rect4);
			Rect rect9 = FindMaxBlob(binary5);
			Rect rect10 = FindMaxBlob(binary6);
			Rect rect11 = FindMaxBlob(binary7);
			Rect rect12 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.T3_LT_H || TrainAIFontParamMode) && (double)rect5.Height > (double)rect9.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[3]左上角高度異常 (3-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_LT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[3]左上角寬度異常 (3-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_RT_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[3]右上角高度異常 (3-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_RT_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[3]右上角寬度異常 (3-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_LB_H || TrainAIFontParamMode) && (double)rect7.Height > (double)rect11.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[3]左下角高度異常 (3-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_LB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[3]左下角寬度異常 (3-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_RB_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect12.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[3]右下角高度異常 (3-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_RB_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect12.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[3]右下角寬度異常 (3-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += "[字元分析]  字元[3]找到斷字異常 (3-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary9 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary10 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 3, rect2.Width, rect2.Height / 3);
			rect4 &= rect2;
			Mat binary11 = new Mat(mat, rect4);
			Rect rect13 = FindMaxBlob(binary9);
			Rect rect14 = FindMaxBlob(binary10);
			Rect rect15 = FindMaxBlob(binary11);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary12 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary13 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 3, rect.Width, rect.Height / 3);
			rect4 &= rect;
			Mat binary14 = new Mat(mat2, rect4);
			Rect rect16 = FindMaxBlob(binary12);
			Rect rect17 = FindMaxBlob(binary13);
			Rect rect18 = FindMaxBlob(binary14);
			if ((pfile.FontToolParam.T3_T_W || TrainAIFontParamMode) && (double)rect13.Width < (double)rect16.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[3]上方寬度異常 (3-10)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_B_W || TrainAIFontParamMode) && (double)rect14.Width < (double)rect17.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[3]下方寬度異常 (3-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T3_C_W || TrainAIFontParamMode) && (double)rect15.Width < (double)rect18.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[3]中心寬度異常 (3-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_3()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-021");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_4(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T4_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T4_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T4_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T4_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T4_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T4_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T4_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T4_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T4_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T4_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T4_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T4_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T4_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 3);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 3);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 3);
			rect4 &= rect;
			Mat binary3 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 3);
			rect4 &= rect;
			Mat binary4 = new Mat(mat2, rect4);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			if ((pfile.FontToolParam.T4_LT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect7.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[4]左上角高度異常 (4-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T4_LT_W || TrainAIFontParamMode) && (double)rect5.Width > (double)rect7.Width * 1.5)
			{
				resultstr += "[字元分析]  字元[4]左上角寬度異常 (4-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T4_RT_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect8.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[4]右上角高度異常 (4-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T4_RT_W || TrainAIFontParamMode) && (double)rect6.Width > (double)rect8.Width * 1.5)
			{
				resultstr += "[字元分析]  字元[4]右上角寬度異常 (4-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary5 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary6 = new Mat(mat, rect4);
			Rect rect9 = FindMaxBlob(binary5);
			Rect rect10 = FindMaxBlob(binary6);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary7 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary8 = new Mat(mat2, rect4);
			Rect rect11 = FindMaxBlob(binary7);
			Rect rect12 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.T4_T_W || TrainAIFontParamMode) && rect9.Width > rect11.Width * 2)
			{
				resultstr += "[字元分析]  字元[4]上方寬度異常 (4-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if (pfile.FontToolParam.T4_T_W || TrainAIFontParamMode)
			{
				if (rect10.Width > rect12.Width * 2)
				{
					resultstr += "[字元分析]  字元[4]下方寬度異常 (4-06)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
				if ((double)rect10.Width < (double)rect12.Width * 0.4)
				{
					resultstr += "[字元分析]  字元[4]下方寬度異常 (4-06)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
			}
			FontCheckCount++;
			Mat binary9 = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height));
			Rect rect13 = FindMaxBlob(binary9);
			Mat binary10 = new Mat(mat2, new Rect(rect.Left, rect.Top, rect.Width, rect.Height));
			Rect rect14 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.T4_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += "[字元分析]  字元[4]找到斷字異常 (4-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T4_H || TrainAIFontParamMode) && (double)rect13.Height < (double)rect14.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[4]高度異常 (4-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T4_W || TrainAIFontParamMode) && (double)rect13.Width < (double)rect14.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[4]寬度異常 (4-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check4-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-022");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_5(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T5_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T5_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T5_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T5_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T5_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T5_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T5_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T5_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T5_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T5_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T5_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T5_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T5_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 3);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 3 * 2, rect2.Width / 2, rect2.Height / 3);
			rect4 &= rect2;
			Mat binary3 = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect4 &= rect2;
			Mat binary4 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary5 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 3);
			rect4 &= rect;
			Mat binary6 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 3 * 2, rect.Width / 2, rect.Height / 3);
			rect4 &= rect;
			Mat binary7 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, rect.Width / 2, rect.Height / 2);
			rect4 &= rect;
			Mat binary8 = new Mat(mat2, rect4);
			Rect rect9 = FindMaxBlob(binary5);
			Rect rect10 = FindMaxBlob(binary6);
			Rect rect11 = FindMaxBlob(binary7);
			Rect rect12 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.T5_LT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect9.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[5]左上角高度異常 (5-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_LT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.5)
			{
				resultstr += "[字元分析]  字元[5]左上角寬度異常 (5-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_RT_H || TrainAIFontParamMode) && rect6.Height > rect10.Height * 3)
			{
				resultstr += "[字元分析]  字元[5]右上角高度異常 (5-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_RT_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[5]右上角寬度異常 (5-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_LB_H || TrainAIFontParamMode) && (double)rect7.Height > (double)rect11.Height * 2.5)
			{
				resultstr += "[字元分析]  字元[5]左下角高度異常 (5-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_LB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.4)
			{
				resultstr += "[字元分析]  字元[5]左下角寬度異常 (5-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_RB_H || TrainAIFontParamMode) && (double)rect8.Height < (double)rect12.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[5]右下角高度異常 (5-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_RB_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect12.Width * 0.5)
			{
				resultstr += "[字元分析]  字元[5]右下角寬度異常 (5-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += string.Format("[字元分析]  字元[5]找到斷字異常({0:N2}) (5-09)\n", SecondAreaRate);
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Mat binary9 = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 3));
			Rect rect13 = FindMaxBlob(binary9);
			Mat binary10 = new Mat(mat2, new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 3));
			Rect rect14 = FindMaxBlob(binary10);
			Mat binary11 = new Mat(mat, new Rect(rect2.Left, rect2.Top + rect2.Height / 3 * 2, rect2.Width, rect2.Height / 3));
			Rect rect15 = FindMaxBlob(binary11);
			Mat binary12 = new Mat(mat2, new Rect(rect.Left, rect.Top + rect.Height / 3 * 2, rect.Width, rect.Height / 3));
			Rect rect16 = FindMaxBlob(binary12);
			if ((pfile.FontToolParam.T5_TT_W || TrainAIFontParamMode) && (double)rect13.Width < (double)rect14.Width * 0.8)
			{
				resultstr += "[字元分析]  字元[5]上方寬度異常 (5-10)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T5_TB_W || TrainAIFontParamMode) && (double)rect15.Width < (double)rect16.Width * 0.8)
			{
				resultstr += "[字元分析]  字元[5]下方寬度異常 (5-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "FontTool-Check_5-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-023");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_6(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T6_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T6_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T6_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T6_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T6_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T6_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T6_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T6_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T6_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T6_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T6_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T6_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T6_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			TimeStart();
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect3 &= rect2;
			Mat binary2 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 3 * 2, rect2.Width, rect2.Height / 3);
			rect3 &= rect2;
			Mat binary3 = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			FindMaxBlob(binary3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect3 &= rect;
			Mat binary4 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
			rect3 &= rect;
			Mat binary5 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 3 * 2, rect.Width, rect.Height / 3);
			rect3 &= rect;
			Mat binary6 = new Mat(mat2, rect3);
			Rect rect6 = FindMaxBlob(binary4);
			Rect rect7 = FindMaxBlob(binary5);
			FindMaxBlob(binary6);
			if ((pfile.FontToolParam.T6_LT_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect6.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[6]左上角高度異常 (6-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T6_LT_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect6.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[6]左上角寬度異常 (6-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T6_RT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect7.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[6]右上角高度異常 (6-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T6_RT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect7.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[6]右上角寬度異常 (6-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect3 &= rect2;
			Mat binary7 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect3 &= rect2;
			Mat binary8 = new Mat(mat, rect3);
			Rect rect8 = FindMaxBlob(binary7);
			Rect rect9 = FindMaxBlob(binary8);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect3 &= rect;
			Mat binary9 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height / 4);
			rect3 &= rect;
			Mat binary10 = new Mat(mat2, rect3);
			Rect rect10 = FindMaxBlob(binary9);
			Rect rect11 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.T6_T_W || TrainAIFontParamMode) && (double)rect8.Width < (double)rect10.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[6]上方寬度異常 (6-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T6_B_W || TrainAIFontParamMode) && (double)rect9.Width < (double)rect11.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[6]下方寬度異常 (6-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if (pfile.FontToolParam.T6_Break || TrainAIFontParamMode)
			{
				if ((double)num2 < (double)num3 * 0.55)
				{
					resultstr += "[字元分析]  字元[6]斷字異常 (邊緣過小) (6-07)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
				if ((double)num2 > (double)num3 * 1.4 || TrainAIFontParamMode)
				{
					resultstr += "[字元分析]  字元[6]斷字異常 (邊緣過大) (6-07)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T6_C || TrainAIFontParamMode) && ContNum < 2)
			{
				resultstr += "[字元分析]  字元[6]中心異常 (6-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_6";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-024");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_7(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T7_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T7_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T7_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T7_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T7_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T7_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T7_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T7_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T7_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T7_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T7_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T7_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T7_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			double SecondAreaRate = 0.0;
			Rect rect3 = FindMaxBlob(mat, ref SecondAreaRate);
			if ((double)rect3.Width >= (double)rect2.Width * 0.8 && (double)rect3.Height >= (double)rect2.Height * 0.8)
			{
				rect2 = rect3;
			}
			Rect rect4 = default(Rect);
			rect4 = new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary = new Mat(mat, rect4);
			rect4 = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect4 &= rect2;
			Mat binary2 = new Mat(mat, rect4);
			Rect rect5 = FindMaxBlob(binary);
			Rect rect6 = FindMaxBlob(binary2);
			rect4 = new Rect(rect.Left, rect.Top, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary3 = new Mat(mat2, rect4);
			rect4 = new Rect(rect.Left, rect.Top + rect.Height / 4 * 3, rect.Width, rect.Height / 4);
			rect4 &= rect;
			Mat binary4 = new Mat(mat2, rect4);
			Rect rect7 = FindMaxBlob(binary3);
			Rect rect8 = FindMaxBlob(binary4);
			if ((pfile.FontToolParam.T7_T_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect7.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[7]上方寬度異常 (7-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T7_B_W || TrainAIFontParamMode) && (double)rect6.Width > (double)rect8.Width * 1.7)
			{
				resultstr += "[字元分析]  字元[7]下方寬度異常 (7-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Mat binary5 = new Mat(mat, new Rect(rect2.Left, rect2.Top, rect2.Width, rect2.Height));
			Rect rect9 = FindMaxBlob(binary5);
			Mat binary6 = new Mat(mat2, new Rect(rect.Left, rect.Top, rect.Width, rect.Height));
			Rect rect10 = FindMaxBlob(binary6);
			if ((pfile.FontToolParam.T7_H || TrainAIFontParamMode) && (double)rect9.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[7]高度異常 (7-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T7_W || TrainAIFontParamMode) && (double)rect9.Width < (double)rect10.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[7]寬度異常 (7-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T7_Break || TrainAIFontParamMode) && SecondAreaRate > 0.2)
			{
				resultstr += "[字元分析]  字元[7]找到斷字異常 (7-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_7-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-025");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_8(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			TimeStart();
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T8_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T8_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T8_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T8_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T8_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T8_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T8_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T8_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T8_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T8_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T8_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T8_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T8_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			bool result = true;
			int num2 = FindMaxContours(mat);
			int num3 = FindMaxContours(mat2);
			Rect rect = default(Rect);
			Rect rect2 = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect3 = new Rect(0, 0, image.Width, image.Height);
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary2 = new Mat(mat, rect);
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary3 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 2, rect3.Top + rect3.Height / 2, rect3.Width / 2, rect3.Height / 2);
			rect &= rect3;
			Mat binary4 = new Mat(mat, rect);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			Rect rect6 = FindMaxBlob(binary3);
			Rect rect7 = FindMaxBlob(binary4);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary5 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary6 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary7 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 2, rect2.Top + rect2.Height / 2, rect2.Width / 2, rect2.Height / 2);
			rect &= rect2;
			Mat binary8 = new Mat(mat2, rect);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			Rect rect10 = FindMaxBlob(binary7);
			Rect rect11 = FindMaxBlob(binary8);
			if ((pfile.FontToolParam.T8_LT_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect8.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[8]左上角高度異常 (8-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_LT_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect8.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[8]左上角寬度異常 (8-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_RT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect9.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[8]右上角高度異常 (8-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_RT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect9.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[8]右上角寬度異常 (8-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_LB_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect10.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[8]左下角高度異常 (8-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_LB_W || TrainAIFontParamMode) && (double)rect6.Width < (double)rect10.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[8]左下角寬度異常 (8-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_RB_H || TrainAIFontParamMode) && (double)rect7.Height < (double)rect11.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[8]右下角高度異常 (8-07)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_RB_W || TrainAIFontParamMode) && (double)rect7.Width < (double)rect11.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[8]右下角寬度異常 (8-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top + rect3.Height / 4 * 3, rect3.Width, rect3.Height / 4);
			rect &= rect3;
			Mat binary9 = new Mat(mat, rect);
			Rect rect12 = FindMaxBlob(binary9);
			rect = new Rect(rect2.Left, rect2.Top + rect2.Height / 4 * 3, rect2.Width, rect2.Height / 4);
			rect &= rect2;
			Mat binary10 = new Mat(mat2, rect);
			Rect rect13 = FindMaxBlob(binary10);
			if ((pfile.FontToolParam.T8_B_W || TrainAIFontParamMode) && (double)rect12.Width < (double)rect13.Width * 0.7)
			{
				resultstr += "[字元分析]  字元[8]下方寬度異常 (8-09)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			Rect roi = new Rect(image.Width / 3, image.Height / 5, image.Width / 3, image.Height / 5 * 3);
			roi &= rect3;
			Mat mat3 = new Mat(mat, roi);
			roi = new Rect(pattern.Width / 3, pattern.Height / 5, pattern.Width / 3, pattern.Height / 5 * 3);
			roi &= rect2;
			Mat mat4 = new Mat(mat2, roi);
			Cv2.BitwiseNot(mat3, mat3);
			Cv2.BitwiseNot(mat4, mat4);
			Rect rect14 = FindMaxBlob(mat3);
			Rect rect15 = FindMaxBlob(mat4);
			int num4 = (int)((double)rect15.Height * 0.7);
			int num5 = (int)((double)rect15.Height * 1.5);
			if (pfile.FontToolParam.T8_C || TrainAIFontParamMode)
			{
				Rect SecondRect = new Rect(0, 0, 0, 0);
				FindMaxBlob(mat3, ref SecondRect);
				if (rect14.Width <= 0 || rect14.Height <= 0 || SecondRect.Width <= 0 || SecondRect.Height <= 0)
				{
					resultstr += "[字元分析]  字元[8]中心異常 (8-10)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_C_H_MIN || TrainAIFontParamMode) && rect14.Height < num4)
			{
				resultstr += "[字元分析]  字元[8]字元中心高度過短 (8-11)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_C_H_MAX || TrainAIFontParamMode) && rect14.Height > num5)
			{
				resultstr += "[字元分析]  字元[8]字元中心高度過長 (8-12)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_Break || TrainAIFontParamMode) && (double)num2 < (double)num3 * 0.88)
			{
				resultstr += "[字元分析]  字元[8]斷字異常 (8-13)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			rect = new Rect(rect3.Left, rect3.Top, rect3.Width / 3, rect3.Height);
			rect &= rect3;
			Mat binary11 = new Mat(mat, rect);
			rect = new Rect(rect3.Left + rect3.Width / 5 * 3, rect3.Top, rect3.Width / 5 * 2, rect3.Height);
			rect &= rect3;
			Mat binary12 = new Mat(mat, rect);
			Rect rect16 = FindMaxBlob(binary11);
			Rect rect17 = FindMaxBlob(binary12);
			rect = new Rect(rect2.Left, rect2.Top, rect2.Width / 3, rect2.Height);
			rect &= rect2;
			Mat binary13 = new Mat(mat2, rect);
			rect = new Rect(rect2.Left + rect2.Width / 5 * 3, rect2.Top, rect2.Width / 5 * 2, rect2.Height);
			rect &= rect2;
			Mat binary14 = new Mat(mat2, rect);
			Rect rect18 = FindMaxBlob(binary13);
			Rect rect19 = FindMaxBlob(binary14);
			if ((pfile.FontToolParam.T8_L_H || TrainAIFontParamMode) && (double)rect16.Height < (double)rect18.Height * 0.75)
			{
				resultstr += "[字元分析]  字元[8]左邊高度異常 (8-14)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T8_R_H || TrainAIFontParamMode) && rect17.Height > rect19.Height * 2)
			{
				resultstr += "[字元分析]  字元[8]右邊高度異常 (8-15)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_8-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-026");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Check_9(Mat image, Mat pattern, ref string resultstr, ref Mat BinaryMat, int index = 0)
	{
		try
		{
			if (image.Width <= 6 || image.Height <= 6 || pattern.Width <= 6 || pattern.Height <= 6)
			{
				return true;
			}
			FontCheckCount = 0;
			if (TrainAIFontParamMode)
			{
				for (int i = 0; i < 25; i++)
				{
					FontCheckResult[i] = true;
				}
			}
			Mat mat = CheckFontBinary[index];
			Mat mat2 = CheckFontPBinary[index];
			double num = 0.0;
			num = Cv2.Threshold(pattern, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			if (AutoBinaryThd > 0.0)
			{
				num = AutoBinaryThd;
			}
			if (pfile.FontToolParam.T9_UseMorphFirst)
			{
				image.CopyTo(mat);
				if (pfile.FontToolParam.T9_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T9_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
				}
				if (pfile.FontToolParam.T9_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				else if (pfile.FontToolParam.T9_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
				}
				Cv2.InRange(mat, num + (double)pfile.FontToolParam.T9_BinaryShift, 255.0, mat);
			}
			else
			{
				Cv2.InRange(image, num + (double)pfile.FontToolParam.T9_BinaryShift, 255.0, mat);
				if (pfile.FontToolParam.T9_OpenClose2 == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount2);
				}
				else if (pfile.FontToolParam.T9_OpenClose2 == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount2);
				}
				if (pfile.FontToolParam.T9_OpenClose == 2)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount);
				}
				else if (pfile.FontToolParam.T9_OpenClose == 1)
				{
					Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.T9_OpenCloseCount);
				}
			}
			if (pfile.FontToolParam.T9_UseFontDBMask)
			{
				Cv2.BitwiseAnd(mat, mat2, mat);
			}
			mat.CopyTo(BinaryMat);
			TimeStart();
			bool result = true;
			Rect rect = new Rect(0, 0, pattern.Width, pattern.Height);
			Rect rect2 = new Rect(0, 0, image.Width, image.Height);
			int ContNum = 0;
			int num2 = FindMaxContours(mat, ref ContNum);
			int num3 = FindMaxContours(mat2);
			Rect rect3 = default(Rect);
			rect3 = new Rect(rect2.Left, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect3 &= rect2;
			Mat binary = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left + rect2.Width / 2, rect2.Top, rect2.Width / 2, rect2.Height / 2);
			rect3 &= rect2;
			Mat binary2 = new Mat(mat, rect3);
			rect3 = new Rect(rect2.Left, rect2.Top + rect2.Height / 3 * 2, rect2.Width, rect2.Height / 3);
			rect3 &= rect2;
			Mat binary3 = new Mat(mat, rect3);
			Rect rect4 = FindMaxBlob(binary);
			Rect rect5 = FindMaxBlob(binary2);
			Rect rect6 = FindMaxBlob(binary3);
			rect3 = new Rect(rect.Left, rect.Top, rect.Width / 2, rect.Height / 2);
			rect3 &= rect;
			Mat binary4 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height / 2);
			rect3 &= rect;
			Mat binary5 = new Mat(mat2, rect3);
			rect3 = new Rect(rect.Left, rect.Top + rect.Height / 3 * 2, rect.Width, rect.Height / 3);
			rect3 &= rect;
			Mat binary6 = new Mat(mat2, rect3);
			Rect rect7 = FindMaxBlob(binary4);
			Rect rect8 = FindMaxBlob(binary5);
			Rect rect9 = FindMaxBlob(binary6);
			if ((pfile.FontToolParam.T9_LT_H || TrainAIFontParamMode) && (double)rect4.Height < (double)rect7.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[9]左上角高度異常 (9-01)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T9_LT_W || TrainAIFontParamMode) && (double)rect4.Width < (double)rect7.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[9]左上角寬度異常 (9-02)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T9_RT_H || TrainAIFontParamMode) && (double)rect5.Height < (double)rect8.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[9]右上角高度異常 (9-03)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T9_RT_W || TrainAIFontParamMode) && (double)rect5.Width < (double)rect8.Width * 0.6)
			{
				resultstr += "[字元分析]  字元[9]右上角寬度異常 (9-04)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T9_B_H || TrainAIFontParamMode) && (double)rect6.Height < (double)rect9.Height * 0.7)
			{
				resultstr += "[字元分析]  字元[9]下方高度異常 (9-05)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T9_B_W || TrainAIFontParamMode) && (double)rect6.Width > (double)rect9.Width * 1.8)
			{
				resultstr += "[字元分析]  字元[9]下方寬度異常 (9-06)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			if (pfile.FontToolParam.T9_Break || TrainAIFontParamMode)
			{
				if ((double)num2 < (double)num3 * 0.55)
				{
					resultstr += "[字元分析]  字元[9]斷字異常 (邊緣過小) (9-07)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
				if ((double)num2 > (double)num3 * 1.4)
				{
					resultstr += "[字元分析]  字元[9]斷字異常 (邊緣過大) (9-07)\n";
					result = (FontCheckResult[FontCheckCount] = false);
				}
			}
			FontCheckCount++;
			if ((pfile.FontToolParam.T9_C || TrainAIFontParamMode) && ContNum < 2)
			{
				resultstr += "[字元分析]  字元[9]中心異常 (9-08)\n";
				result = (FontCheckResult[FontCheckCount] = false);
			}
			FontCheckCount++;
			TimeStop();
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Check_9";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-027");
			}
			return false;
		}
	}

	private int GetMinArea(int FontCount, int[] FontArea)
	{
		int num = 9000000;
		for (int i = 0; i < FontCount; i++)
		{
			if (FontArea[i] < num)
			{
				num = FontArea[i];
			}
		}
		return num;
	}

	private void GetMaxWH(bool UseDash, bool Ver, int FontCount, ref Rect[] rect, ref int MaxW, ref int MaxH)
	{
		MaxW = 0;
		MaxH = 0;
		for (int i = 0; i < FontCount; i++)
		{
			if (!UseDash || i != 0)
			{
				if (rect[i].Width > MaxW)
				{
					MaxW = rect[i].Width;
				}
				if (rect[i].Height > MaxH)
				{
					MaxH = rect[i].Height;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Sort8FontRect(ref Rect[] rect, int length)
	{
		List<Rect> list = new List<Rect>();
		List<Rect> list2 = new List<Rect>();
		List<Rect> list3 = new List<Rect>();
		_ = new Rect[9];
		try
		{
			for (int i = 0; i < length; i++)
			{
				list.Add(rect[i]);
			}
			list.Sort((Rect X, Rect Y) => X.Top.CompareTo(Y.Top));
			for (int num = 0; num < 4; num++)
			{
				list2.Add(list[num]);
			}
			list2.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
			for (int num2 = 4; num2 < length; num2++)
			{
				list3.Add(list[num2]);
			}
			list3.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
			for (int num3 = 0; num3 < length; num3++)
			{
				if (num3 < 4)
				{
					rect[num3] = list2[num3];
				}
				else
				{
					rect[num3] = list3[num3 % 4];
				}
			}
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Sort8FontRect";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-028");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Sort9FontRect(ref Rect[] rect, int length)
	{
		List<Rect> list = new List<Rect>();
		List<Rect> list2 = new List<Rect>();
		List<Rect> list3 = new List<Rect>();
		List<Rect> list4 = new List<Rect>();
		_ = new Rect[9];
		try
		{
			for (int i = 0; i < length; i++)
			{
				list.Add(rect[i]);
			}
			list.Sort((Rect X, Rect Y) => X.Top.CompareTo(Y.Top));
			for (int num = 0; num < 3; num++)
			{
				list2.Add(list[num]);
			}
			list2.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
			for (int num2 = 3; num2 < length; num2++)
			{
				list3.Add(list[num2]);
			}
			list3.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
			if (length > 6)
			{
				for (int num3 = 6; num3 < length; num3++)
				{
					list4.Add(list[num3]);
				}
				list4.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
			}
			for (int num4 = 0; num4 < length; num4++)
			{
				if (num4 < 3)
				{
					rect[num4] = list2[num4];
				}
				else if (num4 >= 3 && num4 < 6)
				{
					rect[num4] = list3[num4 % 3];
				}
				else
				{
					rect[num4] = list4[num4 % 3];
				}
			}
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Sort9FontRect";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-028");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SortFontRect(bool UsedDash, bool Ver, int FontCount, ref Rect[] rect)
	{
		List<Rect> list = new List<Rect>();
		Rect[] array = new Rect[FontCount];
		try
		{
			if (UsedDash)
			{
				if (Ver)
				{
					int num = 10000;
					int num2 = 0;
					for (int i = 0; i < FontCount; i++)
					{
						if (rect[i].Left < num)
						{
							num = rect[i].Left;
							num2 = i;
						}
					}
					array[0] = rect[num2];
					int num3 = 1;
					for (int j = 0; j < FontCount; j++)
					{
						if (j != num2)
						{
							list.Add(rect[j]);
							array[num3] = rect[j];
							num3++;
						}
					}
					list.Sort((Rect X, Rect Y) => X.Top.CompareTo(Y.Top));
					rect[0] = array[0];
					for (int num4 = 1; num4 < FontCount; num4++)
					{
						rect[num4] = list[list.Count - num4];
					}
				}
				else
				{
					for (int num5 = 1; num5 < FontCount; num5++)
					{
						list.Add(rect[num5]);
					}
					list.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
					for (int num6 = 1; num6 < FontCount; num6++)
					{
						rect[num6] = list[num6 - 1];
					}
				}
			}
			else if (Ver)
			{
				for (int num7 = 0; num7 < FontCount; num7++)
				{
					list.Add(rect[num7]);
				}
				list.Sort((Rect X, Rect Y) => X.Top.CompareTo(Y.Top));
				for (int num8 = 0; num8 < FontCount; num8++)
				{
					rect[num8] = list[num8];
				}
			}
			else
			{
				for (int num9 = 0; num9 < FontCount; num9++)
				{
					list.Add(rect[num9]);
				}
				list.Sort((Rect X, Rect Y) => X.Left.CompareTo(Y.Left));
				for (int num10 = 0; num10 < FontCount; num10++)
				{
					rect[num10] = list[num10];
				}
			}
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-SortFontRect";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-029");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckFontIsBlack(Mat mat)
	{
		try
		{
			Mat mat2 = new Mat();
			Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			Rect rect = new Rect(0, 0, 0, 0);
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			Mat mat5 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(mat2, mat3, mat4, mat5, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat4.GetGenericIndexer<int>();
			int num2 = 0;
			int i = -1;
			if (num > 1)
			{
				for (int j = 1; j < num; j++)
				{
					if (genericIndexer[j, 4] > num2)
					{
						i = j;
						num2 = genericIndexer[j, 4];
					}
				}
				rect = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
			}
			Rect rect2 = new Rect(0, 0, 0, 0);
			Cv2.BitwiseNot(mat2, mat2);
			num = Cv2.ConnectedComponentsWithStats(mat2, mat3, mat4, mat5, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			genericIndexer = mat4.GetGenericIndexer<int>();
			num2 = 0;
			i = -1;
			if (num > 1)
			{
				for (int k = 1; k < num; k++)
				{
					if (genericIndexer[k, 4] > num2)
					{
						i = k;
						num2 = genericIndexer[k, 4];
					}
				}
				rect2 = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
			}
			if (rect2.Width + rect2.Height > rect.Width + rect.Height)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-FindMaxFontRect";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-030");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckFontIsBlackOld(Mat mat)
	{
		try
		{
			Mat mat2 = new Mat();
			Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			Rect rect = new Rect(0, 0, 0, 0);
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			Mat mat5 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(mat2, mat3, mat4, mat5, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat4.GetGenericIndexer<int>();
			int num2 = 0;
			int i = -1;
			if (num > 1)
			{
				for (int j = 1; j < num; j++)
				{
					if (genericIndexer[j, 4] > num2)
					{
						i = j;
						num2 = genericIndexer[j, 4];
					}
				}
				rect = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
			}
			if ((double)(rect.Width * rect.Height) > (double)(mat.Width * mat.Height) * 0.7)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-FindMaxFontRect";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-031");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool FindMaxFontRect(Mat mat, int FontCount, bool Ver, ref Rect[] FontRect, ref int[] FontArea, ref double thd)
	{
		try
		{
			thd = 0.0;
			thd = Cv2.Threshold(mat, FindFontRectBinary, 0.0, 255.0, ThresholdTypes.Otsu);
			int connectBinaryShift = pfile.FontToolParam.ConnectBinaryShift;
			if (FontIsBlack)
			{
				Cv2.InRange(mat, 0.0, thd + (double)connectBinaryShift, FindFontRectBinary);
			}
			else
			{
				Cv2.InRange(mat, thd + (double)connectBinaryShift, 255.0, FindFontRectBinary);
			}
			if (pfile.FontToolParam.ConnectOpenClose2 == 1)
			{
				Cv2.MorphologyEx(FindFontRectBinary, FindFontRectBinary, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.ConnectOpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
			}
			else if (pfile.FontToolParam.ConnectOpenClose2 == 2)
			{
				Cv2.MorphologyEx(FindFontRectBinary, FindFontRectBinary, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.ConnectOpenCloseCount2, BorderTypes.Constant, new Scalar(0.0));
			}
			if (pfile.FontToolParam.ConnectOpenClose == 1)
			{
				Cv2.MorphologyEx(FindFontRectBinary, FindFontRectBinary, MorphTypes.Open, new Mat(), null, pfile.FontToolParam.ConnectOpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
			}
			else if (pfile.FontToolParam.ConnectOpenClose == 2)
			{
				Cv2.MorphologyEx(FindFontRectBinary, FindFontRectBinary, MorphTypes.Close, new Mat(), null, pfile.FontToolParam.ConnectOpenCloseCount, BorderTypes.Constant, new Scalar(0.0));
			}
			FontConnectBinary = FindFontRectBinary;
			FindFontRectBinary.CopyTo(pfile.FontToolParam.TextMat);
			if (FlipStatus)
			{
				Cv2.Flip(pfile.FontToolParam.TextMat, pfile.FontToolParam.TextMat, FlipMode.XY);
			}
			new Rect(0, 0, 0, 0);
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(FindFontRectBinary, mat2, mat3, mat4, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat3.GetGenericIndexer<int>();
			int num2 = 0;
			int num3 = -1;
			if (num <= FontCount)
			{
				return false;
			}
			int[] array = new int[FontCount];
			bool flag = false;
			for (int i = 0; i < FontCount; i++)
			{
				array[i] = -1;
				for (int j = 1; j < num; j++)
				{
					for (int k = 0; k < i; k++)
					{
						if (array[k] == j)
						{
							flag = true;
						}
					}
					if (flag)
					{
						flag = false;
					}
					else if (Ver)
					{
						if (genericIndexer[j, 3] > num2)
						{
							num3 = j;
							num2 = genericIndexer[j, 3];
						}
					}
					else if (genericIndexer[j, 2] > num2)
					{
						Rect rect = new Rect(genericIndexer[j, 0], genericIndexer[j, 1], genericIndexer[j, 2], genericIndexer[j, 3]);
						if (rect.Left != 0 || rect.Top != 0)
						{
							num3 = j;
							num2 = genericIndexer[j, 2];
						}
					}
				}
				if (num3 >= 0)
				{
					FontRect[i] = new Rect(genericIndexer[num3, 0], genericIndexer[num3, 1], genericIndexer[num3, 2], genericIndexer[num3, 3]);
					FontArea[i] = genericIndexer[num3, 4];
					array[i] = num3;
					num2 = 0;
					num3 = -1;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-FindMaxFontRect";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-032");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Rect FindMaxBlob(Mat binary)
	{
		Rect result = new Rect(0, 0, 0, 0);
		try
		{
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num2 = 0;
			int i = 0;
			if (num > 1)
			{
				for (int j = 1; j < num; j++)
				{
					if (genericIndexer[j, 4] > num2)
					{
						i = j;
						num2 = genericIndexer[j, 4];
					}
				}
				result = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
			}
			else
			{
				result = new Rect(0, 0, 0, 0);
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Rect FindMaxBlob(Mat binary)";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-033");
			}
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Rect FindMaxBlob(Mat binary, ref Rect SecondRect, bool FilterSize = true)
	{
		int num = 0;
		Rect result = new Rect(0, 0, 0, 0);
		SecondRect = new Rect(0, 0, 0, 0);
		try
		{
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num2 = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num3 = 0;
			int num4 = 0;
			if (num2 > 1)
			{
				for (int i = 1; i < num2; i++)
				{
					if (genericIndexer[i, 4] > num3)
					{
						num4 = i;
						num3 = genericIndexer[i, 4];
					}
				}
				result = new Rect(genericIndexer[num4, 0], genericIndexer[num4, 1], genericIndexer[num4, 2], genericIndexer[num4, 3]);
				num = num3;
			}
			else
			{
				result = new Rect(0, 0, 0, 0);
				num = num3;
			}
			int num5 = 0;
			int i2 = 0;
			if (num2 > 2)
			{
				for (int j = 1; j < num2; j++)
				{
					if (j != num4 && genericIndexer[j, 4] > num5)
					{
						i2 = j;
						num5 = genericIndexer[j, 4];
					}
				}
				SecondRect = new Rect(genericIndexer[i2, 0], genericIndexer[i2, 1], genericIndexer[i2, 2], genericIndexer[i2, 3]);
			}
			else
			{
				SecondRect = new Rect(0, 0, 0, 0);
				num5 = 0;
			}
			if (FilterSize && num5 > 0 && num5 < num / 12)
			{
				SecondRect = new Rect(0, 0, 0, 0);
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Rect Rect FindMaxBlob(Mat binary, ref Rect SecondRect)";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-034");
			}
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Rect FindMaxBlob(Mat binary, ref double SecondAreaRate)
	{
		int num = 0;
		SecondAreaRate = 0.0;
		Rect result = new Rect(0, 0, 0, 0);
		try
		{
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num2 = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num3 = 0;
			int num4 = 0;
			if (num2 > 1)
			{
				for (int i = 1; i < num2; i++)
				{
					if (genericIndexer[i, 4] > num3)
					{
						num4 = i;
						num3 = genericIndexer[i, 4];
					}
				}
				result = new Rect(genericIndexer[num4, 0], genericIndexer[num4, 1], genericIndexer[num4, 2], genericIndexer[num4, 3]);
				num = num3;
			}
			else
			{
				result = new Rect(0, 0, 0, 0);
				num = num3;
			}
			int num5 = 0;
			if (num2 > 2)
			{
				for (int j = 1; j < num2; j++)
				{
					if (j != num4 && genericIndexer[j, 4] > num5)
					{
						num5 = genericIndexer[j, 4];
					}
				}
				SecondAreaRate = (double)num5 / (double)num;
			}
			else
			{
				num5 = 0;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Rect Rect FindMaxBlob(Mat binary, ref Rect SecondRect)";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-035");
			}
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int FindMaxContours(Mat mat, ref int ContNum, int minarea = 2)
	{
		try
		{
			if (mat.Width <= 0 || mat.Height <= 0)
			{
				return 0;
			}
			_ = new Point2f[4];
			Cv2.FindContours(mat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
			double num = 0.0;
			for (int i = 0; i < contours.Length; i++)
			{
				if (contours[i].Length > minarea)
				{
					ContNum++;
				}
			}
			Mat mat2 = new Mat(mat.Rows, mat.Cols, MatType.CV_8UC3);
			for (int j = 0; j < contours.Length; j++)
			{
				Cv2.DrawContours(mat2, contours, j, Scalar.Red);
			}
			for (int k = 0; k < contours.Length; k++)
			{
				double num2 = Cv2.ContourArea(contours[k]);
				if (num2 > num)
				{
					num = num2;
				}
			}
			return (int)num;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Rect Rect FindMaxContours";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-036");
			}
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int FindMaxContours(Mat mat)
	{
		try
		{
			if (mat.Width <= 0 || mat.Height <= 0)
			{
				return 0;
			}
			_ = new Point2f[4];
			Cv2.FindContours(mat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			double num = 0.0;
			for (int i = 0; i < contours.Length; i++)
			{
				double num2 = Cv2.ContourArea(contours[i]);
				if (num2 > num)
				{
					num = num2;
				}
			}
			return (int)num;
		}
		catch (Exception ex)
		{
			string text = "BT02-FontTool-Rect Rect FindMaxContours";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-037");
			}
			return 0;
		}
	}

	private Rect FindBlackMaxBlob(Mat mat1)
	{
		Rect roi = new Rect(5, 5, mat1.Width - 10, mat1.Height - 10);
		Mat mat2 = new Mat(mat1, roi);
		Mat mat3 = new Mat();
		Cv2.Threshold(mat2, mat3, 0.0, 255.0, ThresholdTypes.Otsu);
		Cv2.MorphologyEx(mat3, mat3, MorphTypes.Close, new Mat(), null, 2);
		Cv2.BitwiseNot(mat3, mat3);
		Rect rect = new Rect(0, 0, 0, 0);
		Mat mat4 = new Mat();
		Mat mat5 = new Mat();
		Mat mat6 = new Mat();
		int num = Cv2.ConnectedComponentsWithStats(mat3, mat4, mat5, mat6, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
		Mat.Indexer<int> genericIndexer = mat5.GetGenericIndexer<int>();
		int num2 = 0;
		int i = 0;
		if (num > 1)
		{
			for (int j = 1; j < num; j++)
			{
				if (genericIndexer[j, 4] > num2)
				{
					i = j;
					num2 = genericIndexer[j, 4];
				}
			}
			rect = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
		}
		else
		{
			rect = new Rect(0, 0, 0, 0);
		}
		return rect;
	}

	private void TimeStart()
	{
	}

	private double TimeStop()
	{
		return 0.0;
	}

	private int GetTopLayer(Mat matTempl, int iMinDstLength)
	{
		int num = 0;
		int num2 = iMinDstLength * iMinDstLength;
		int num3 = matTempl.Cols * matTempl.Rows;
		while (num3 > num2)
		{
			num3 /= 4;
			num++;
		}
		return num;
	}

	private void LearnPattern()
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ExportCSV(string filename, bool IsOK)
	{
		try
		{
			string text = string.Format("{0}Training\\FontAITrain\\OK\\", AppDomain.CurrentDomain.BaseDirectory);
			string text2 = string.Format("{0}Training\\FontAITrain\\NG\\", AppDomain.CurrentDomain.BaseDirectory);
			string path = string.Format("{0}Training\\FontAITrain\\result\\result.csv", AppDomain.CurrentDomain.BaseDirectory);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			string text3 = "";
			text3 = ((!IsOK) ? text2 : text);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			if (!Directory.Exists(text3))
			{
				return false;
			}
			string path2 = string.Format("{0}{1}.csv", text3, filename);
			string text4 = "";
			for (int i = 0; i < FontCheckCount; i++)
			{
				text4 = (FontUsePara[i] ? (text4 + string.Format("{0},", FontParaResult[i])) : ((!FontCheckResult[i]) ? (text4 + "0,") : (text4 + "1,")));
			}
			using (StreamWriter streamWriter = new StreamWriter(path2, append: true))
			{
				streamWriter.Write("{0}", text4);
				streamWriter.WriteLine();
			}
			return true;
		}
		catch (Exception ex)
		{
			string text5 = "BT02-FontTool-ExportCSV";
			text5 += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text5);
			}
			else
			{
				CVBuffer.SaveLog(text5, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "FontTool-001");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string[] CSVstrToArry(string CSVFile)
	{
		string text;
		using (StreamReader streamReader = new StreamReader(CSVFile, Encoding.Default))
		{
			text = streamReader.ReadLine();
		}
		string text2 = string.Empty;
		List<string> list = new List<string>();
		bool flag = false;
		string[] array = text.Split(',');
		foreach (string text3 in array)
		{
			if (!string.IsNullOrEmpty(text3) && text3.IndexOf('"') > -1)
			{
				string text4 = text3.Substring(0, 1);
				string text5 = string.Empty;
				if (text3.Length > 0)
				{
					text5 = text3.Substring(text3.Length - 1, 1);
				}
				if (text4.Equals("\"") && !text5.Equals("\""))
				{
					flag = true;
				}
				if (text5.Equals("\""))
				{
					text2 = (flag ? (text2 + "," + text3) : (text2 + text3));
					flag = false;
				}
			}
			else if (string.IsNullOrEmpty(text2))
			{
				text2 += text3;
			}
			if (flag)
			{
				text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + "," + text3) : (text2 + text3));
				continue;
			}
			list.Add(text2.Replace("\"", "").Trim());
			text2 = string.Empty;
		}
		return list.ToArray();
	}

	public void ResetFontPara()
	{
		for (int i = 0; i < 9; i++)
		{
			FontUsePara[i] = false;
			FontUseParaMin[i] = false;
			FontUseParaMax[i] = false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetFontPara(int index)
	{
		ResetFontPara();
		return index switch
		{
			0 => "R", 
			1 => "M", 
			2 => "C", 
			3 => "U", 
			4 => "-", 
			5 => "0", 
			6 => "1", 
			7 => "2", 
			8 => "3", 
			9 => "4", 
			10 => "5", 
			11 => "6", 
			12 => "7", 
			13 => "8", 
			14 => "9", 
			15 => "X", 
			16 => "B", 
			17 => "E", 
			18 => "D", 
			19 => "A", 
			20 => "F", 
			21 => "G", 
			22 => "H", 
			23 => "I", 
			24 => "J", 
			25 => "K", 
			26 => "L", 
			27 => "N", 
			28 => "O", 
			29 => "P", 
			30 => "Q", 
			31 => "S", 
			32 => "T", 
			33 => "V", 
			34 => "W", 
			35 => "Y", 
			36 => "Z", 
			_ => "", 
		};
	}

	public int GetFontIndex(string str)
	{
		return str switch
		{
			"R" => 0, 
			"M" => 1, 
			"C" => 2, 
			"U" => 3, 
			"-" => 4, 
			"0" => 5, 
			"1" => 6, 
			"2" => 7, 
			"3" => 8, 
			"4" => 9, 
			"5" => 10, 
			"6" => 11, 
			"7" => 12, 
			"8" => 13, 
			"9" => 14, 
			"X" => 15, 
			"B" => 16, 
			"E" => 17, 
			"D" => 18, 
			"A" => 19, 
			"F" => 20, 
			"G" => 21, 
			"H" => 22, 
			"I" => 23, 
			"J" => 24, 
			"K" => 25, 
			"L" => 26, 
			"N" => 27, 
			"O" => 28, 
			"P" => 29, 
			"Q" => 30, 
			"S" => 31, 
			"T" => 32, 
			"V" => 33, 
			"W" => 34, 
			"Y" => 35, 
			"Z" => 36, 
			_ => 0, 
		};
	}

	private double PredictOnnx(InferenceSession session, Mat image, int FontIndex)
	{
		int width = AIMatchImageSize[FontIndex].Width;
		int height = AIMatchImageSize[FontIndex].Height;
		int num = 3;
		GCHandle gCHandle = GCHandle.Alloc(new float[height * width * num], GCHandleType.Pinned);
		gCHandle.AddrOfPinnedObject();
		Cv2.CvtColor(image, image, ColorConversionCodes.GRAY2BGR);
		Cv2.Resize(image, image, new Size(width, height));
		image.ConvertTo(image, 5, 1.0 / 255.0);
		float[] array = MatToFloatArray(image);
		int[] array2 = new int[4]
		{
			1,
			image.Channels(),
			image.Rows,
			image.Cols
		};
		DenseTensor<float> value = new DenseTensor<float>(array, array2);
		gCHandle.Free();
		string name = session.InputMetadata.Keys.First();
		List<NamedOnnxValue> list = new List<NamedOnnxValue>();
		list.Add(NamedOnnxValue.CreateFromTensor(name, value));
		return session.Run(list).First().AsTensor<float>()
			.ToArray()[0];
	}

	private static float[] MatToFloatArray(Mat image)
	{
		float[] array = new float[image.Rows * image.Cols * image.Channels()];
		Mat[] mv = new Mat[image.Channels()];
		Cv2.Split(image, out mv);
		int num = image.Rows * image.Cols;
		for (int i = 0; i < image.Channels(); i++)
		{
			using Mat m = new Mat(mv[i].Rows, mv[i].Cols, 5);
			mv[i].ConvertTo(m, 5);
			Marshal.Copy(mv[i].Data, array, i * num, num);
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void AutoSetFontImage(Mat mat, string FontStr, bool UseVertical)
	{
		try
		{
			mat.CopyTo(orgMat);
			CheckFontIsBlack(mat);
			Mat mat2 = new Mat();
			double num = Cv2.Threshold(mat, mat2, 0.0, 255.0, ThresholdTypes.Otsu);
			int connectBinaryShift = pfile.FontToolParam.ConnectBinaryShift;
			Cv2.InRange(mat, num + (double)connectBinaryShift, 255.0, mat);
			Cv2.FindContours(mat, out Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
			List<Rect> list = new List<Rect>();
			Point[][] array = contours;
			foreach (Point[] array2 in array)
			{
				if (Cv2.ContourArea(array2) > 0.0)
				{
					Rect item = Cv2.BoundingRect(array2);
					list.Add(item);
				}
			}
			if (list.Count < FontStr.Length)
			{
				return;
			}
			list = list.OrderByDescending((Rect r) => r.Width * r.Height).ToList().Take(FontStr.Length)
				.ToList();
			if (UseVertical)
			{
				list.Sort((Rect a, Rect b) => a.Y.CompareTo(b.Y));
				for (int num2 = 0; num2 < FontStr.Length; num2++)
				{
					Rect roi = list[FontStr.Length - num2 - 1];
					roi.Width += 10;
					roi.Height += 10;
					roi.Left -= 5;
					roi.Top -= 5;
					roi &= new Rect(0, 0, mat.Width, mat.Height);
					using Mat pattern = new Mat(orgMat, roi);
					SetFontPattern(FontStr.Substring(num2, 1), pattern);
				}
				return;
			}
			list.Sort((Rect a, Rect b) => a.X.CompareTo(b.X));
			for (int num3 = FontStr.Length - 1; num3 >= 0; num3--)
			{
				Rect roi2 = list[num3];
				roi2.Width += 10;
				roi2.Height += 10;
				roi2.Left -= 5;
				roi2.Top -= 5;
				roi2 &= new Rect(0, 0, mat.Width, mat.Height);
				using Mat pattern2 = new Mat(orgMat, roi2);
				SetFontPattern(FontStr.Substring(num3, 1), pattern2);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("MTW07-FontTool-AutoSetFontImage()" + ex.ToString());
		}
	}

	public unsafe void ScanAIFontImgVer(Mat binaryImage)
	{
		if (binaryImage == null || binaryImage.Cols <= 0 || binaryImage.Rows <= 0)
		{
			return;
		}
		byte* ptr = (byte*)(void*)binaryImage.Data;
		int num = (int)binaryImage.Step();
		int[] array = new int[binaryImage.Rows];
		int[] array2 = new int[binaryImage.Rows];
		int[] array3 = new int[binaryImage.Rows];
		for (int i = 0; i < binaryImage.Rows; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int j = 0; j < binaryImage.Cols; j++)
			{
				if (ptr[i * num + j] == byte.MaxValue)
				{
					if (num2 == 0)
					{
						num4 = j;
					}
					num2++;
					if (num2 > num3)
					{
						num3 = num2;
						array2[i] = num4;
						array3[i] = j;
					}
				}
				else
				{
					num2 = 0;
				}
			}
			array[i] = num3;
		}
	}

	public unsafe void ScanAIFontImgHor(Mat binaryImage)
	{
		if (binaryImage == null || binaryImage.Cols <= 0 || binaryImage.Rows <= 0)
		{
			return;
		}
		byte* ptr = (byte*)(void*)binaryImage.Data;
		int num = (int)binaryImage.Step();
		int[] array = new int[binaryImage.Cols];
		int[] array2 = new int[binaryImage.Cols];
		int[] array3 = new int[binaryImage.Cols];
		for (int i = 0; i < binaryImage.Cols; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int j = 0; j < binaryImage.Rows; j++)
			{
				if (ptr[j * num + i] == byte.MaxValue)
				{
					if (num2 == 0)
					{
						num4 = j;
					}
					num2++;
					if (num2 > num3)
					{
						num3 = num2;
						array2[i] = num4;
						array3[i] = j;
					}
				}
				else
				{
					num2 = 0;
				}
			}
			array[i] = num3;
		}
	}

	private bool CheckContinuousWhiteCounts(int[] maxContinuousWhiteCounts, int threshold, int consecutiveRows)
	{
		int num = 0;
		for (int i = 0; i < maxContinuousWhiteCounts.Length; i++)
		{
			if (maxContinuousWhiteCounts[i] > threshold)
			{
				num++;
				if (num >= consecutiveRows)
				{
					return false;
				}
			}
			else
			{
				num = 0;
			}
		}
		return true;
	}
}
