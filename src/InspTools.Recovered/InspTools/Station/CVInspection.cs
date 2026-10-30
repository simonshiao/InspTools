using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using InspTools.AOI;
using OpenCvSharp;

namespace InspTools.Station;

public class CVInspection
{
	public LocateTool locateTool;

	public SelectWayTool selectwayTool;

	public SelectFaceTool selectfaceTool;

	public LightMonitorTool lightmonitorTool;

	public EnhanceTool enhanceTool;

	public AdvLocateTool advLocateTool;

	public MaskTool maskTool;

	public BinaryTool[] binaryTool = new BinaryTool[30];

	public MeasureTool measuretool;

	public SmoothTool smoothTool;

	public DifferenceTool differenceTool;

	public BlockAvgTool[] blockavgTool = new BlockAvgTool[20];

	public LineAnalysisTool lineanalysisTool;

	public MatchTool[] matchTool = new MatchTool[20];

	public FontTool fontTool;

	public IRTool irTool;

	public AIProphetTool aiProphetTools;

	public AIEnhanceTool aiEnhanceTools;

	public AIPredictTool aiPredictTools;

	public bool AIDLLExist;

	public bool AIPBFileExist;

	public int[] ErrorCount = new int[18];

	public ParamFile paramFile;

	public ParamFile paramFile_Backup;

	public ParamFile param1;

	public ParamFile param2;

	public ParamFile param3;

	private int StationIndex;

	private bool SaveAIImage;

	private const int MaxMeasureTools = 40;

	private const int MaxBinaryTools = 30;

	private const int MaxTools = 20;

	private const int MaxFace = 4;

	private HiniFile StationINI;

	public string RecipeName;

	private string setupinipath;

	private const int MAXStaticCount = 1100;

	public List<byte[]> RetestBufferList;

	public List<byte[]> RecentBufferList;

	public List<byte[]> OKBufferList;

	public List<byte[]> NGBufferList;

	public List<byte[]>[] NGParetoBufferList = new List<byte[]>[30];

	public List<int> RecentBufferIDList;

	public List<int> OKBufferIDList;

	public List<int> NGBufferIDList;

	public int LastIndex = -1;

	private int BufferWidth = 640;

	private int BufferHeight = 480;

	private int BufferBand = 3;

	public CVBuffer buffers = new CVBuffer();

	public CVInspResult iResult;

	public int[] ParetoChartTable = new int[30];

	public string[] ParetoCharName = new string[30];

	public int ParetoItemCount;

	public bool EnableMultiParetoMode;

	public double[] InspTime = new double[500];

	private int timecount;

	public double[] InspTimePerTool = new double[20];

	private Stopwatch sw = new Stopwatch();

	public Mat[] TempImage = new Mat[3];

	private bool ReverseBinPriority;

	public bool StationAuthed = true;

	private bool EnableLightMonitor;

	private bool EnableLightCorrection;

	private bool EnableSetupLightMonitorBaseValue;

	private bool StartSoftCorrection;

	public List<OpenCvSharp.Rect> NGRect = new List<OpenCvSharp.Rect>();

	public List<InspToolsFunciton> NGTool = new List<InspToolsFunciton>();

	public List<OpenCvSharp.Point> NGPt = new List<OpenCvSharp.Point>();

	public List<int> NGToolIndex = new List<int>();

	public List<SIZESTATIC> SizeStatics = new List<SIZESTATIC>();

	private List<SIZESTATIC> Last100Data = new List<SIZESTATIC>();

	private bool UpdateLast100;

	public List<PARETODATA> ParetoData = new List<PARETODATA>();

	private PARETODATA pData;

	public List<INSPRESULT> InlineResult = new List<INSPRESULT>();

	public List<INSPRESULT> OfflineResult = new List<INSPRESULT>();

	private object BinaryOffLineLock = new object();

	private int tempcount;

	private int MaxEnhance = 6;

	private int ThreadID;

	public OpenCvSharp.Rect LocateRect { get; set; }

	public OpenCvSharp.Rect LocateRectTmp { get; set; }

	public OpenCvSharp.Rect LocateImageRect { get; protected set; }

	public int ParetoIndex { get; set; }

	public double LocateTime { get; protected set; }

	public string StationTxt { get; set; }

	public string ErrorMsg { get; protected set; }

	public bool AdvlocateResult { get; protected set; }

	public bool AdvMeasureResult { get; protected set; }

	public bool LocaetResult { get; protected set; }

	public bool SelectWayDoFlip { get; protected set; }

	public int SelectFaceResult { get; protected set; }

	public bool MeasureResult { get; protected set; }

	public bool BinaryResult { get; protected set; }

	public bool SmoothResult { get; protected set; }

	public bool DifferenceResult { get; protected set; }

	public bool BlockAvgResult { get; protected set; }

	public bool LineAnalysisResult { get; protected set; }

	public bool MatchResult { get; protected set; }

	public bool AIPredictResult { get; protected set; }

	public bool AIProphetResult { get; protected set; }

	public bool FontToolInspResult { get; protected set; }

	public bool IRToolInspResult { get; protected set; }

	public bool OfflineBufTestStop { get; set; }

	public double GetInspTime(int count)
	{
		if (count >= 500)
		{
			return -1.0;
		}
		return InspTime[count];
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public CVInspection()
	{
		try
		{
			paramFile = new ParamFile();
			paramFile_Backup = new ParamFile();
			param1 = new ParamFile();
			param2 = new ParamFile();
			param3 = new ParamFile();
			locateTool = new LocateTool(paramFile);
			locateTool.SetBuffers(buffers);
			selectwayTool = new SelectWayTool(paramFile);
			selectwayTool.SetBuffers(buffers);
			selectfaceTool = new SelectFaceTool(paramFile);
			selectfaceTool.SetBuffers(buffers);
			lightmonitorTool = new LightMonitorTool(paramFile);
			lightmonitorTool.SetBuffers(buffers);
			MaxEnhance = 6;
			enhanceTool = new EnhanceTool(paramFile);
			enhanceTool.SetBuffers(buffers);
			advLocateTool = new AdvLocateTool(paramFile);
			advLocateTool.SetBuffers(buffers);
			measuretool = new MeasureTool(paramFile);
			measuretool.SetBuffers(buffers);
			maskTool = new MaskTool(paramFile);
			maskTool.SetBuffers(buffers);
			for (int i = 0; i < 30; i++)
			{
				binaryTool[i] = new BinaryTool(paramFile);
				binaryTool[i].SetBuffers(buffers);
			}
			smoothTool = new SmoothTool(paramFile);
			smoothTool.SetBuffers(buffers);
			differenceTool = new DifferenceTool(paramFile);
			differenceTool.SetBuffers(buffers);
			for (int j = 0; j < 20; j++)
			{
				blockavgTool[j] = new BlockAvgTool(paramFile);
				blockavgTool[j].SetBuffers(buffers);
			}
			lineanalysisTool = new LineAnalysisTool(paramFile);
			lineanalysisTool.SetBuffers(buffers);
			for (int k = 0; k < 20; k++)
			{
				matchTool[k] = new MatchTool(paramFile);
				matchTool[k].SetBuffers(buffers);
			}
			fontTool = new FontTool(paramFile);
			fontTool.SetBuffers(buffers);
			irTool = new IRTool(paramFile);
			irTool.SetBuffers(buffers);
			string path = AppDomain.CurrentDomain.BaseDirectory + "DeltaAIProcessDll.dll";
			string.Format("{0}PModel\\Train\\dist\\AIProphet\\AIProphet.exe", AppDomain.CurrentDomain.BaseDirectory);
			aiProphetTools = new AIProphetTool(paramFile);
			aiProphetTools.SetBuffers(buffers);
			if (File.Exists(path))
			{
				aiEnhanceTools = new AIEnhanceTool(paramFile);
				aiEnhanceTools.SetBuffers(buffers);
				AIDLLExist = true;
				aiPredictTools = new AIPredictTool(paramFile);
				aiPredictTools.SetBuffers(buffers);
			}
			else
			{
				aiEnhanceTools = null;
				aiPredictTools = null;
				AIDLLExist = false;
			}
			for (int l = 0; l < 3; l++)
			{
				TempImage[l] = new Mat();
			}
			iResult = new CVInspResult();
			for (int m = 0; m < 30; m++)
			{
				ParetoChartTable[m] = 0;
				NGParetoBufferList[m] = new List<byte[]>();
			}
			ParetoIndex = 0;
			OfflineBufTestStop = false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Inspection-CVInspection()-00-" + ex.ToString());
		}
	}

	public void SetStationON(bool ON)
	{
		paramFile.StationON = ON;
	}

	public bool GetStationON()
	{
		return paramFile.StationON;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetRecipe(string recipename, int index, ref string CopyErrorMsg)
	{
		CopyErrorMsg = "";
		ErrorMsg = "";
		RecipeName = recipename;
		setupinipath = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + RecipeName + "\\";
		if (fontTool != null)
		{
			fontTool.SetRecipeName(recipename);
			fontTool.PatternRectSeted = false;
		}
		try
		{
			if (!Directory.Exists(setupinipath))
			{
				Directory.CreateDirectory(setupinipath);
			}
			string path = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + recipename + "\\Font\\";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
		catch (Exception ex)
		{
			string msg = "Inspection-SetRecipe()-01-" + ex.ToString();
			CopyErrorMsg = "fontpath n及 Recipe路徑建立錯誤";
			CVBuffer.SaveLog(msg);
		}
		string text = setupinipath + "station" + (index + 1) + ".ini";
		if (!File.Exists(text))
		{
			FileStream fileStream = File.Create(text);
			byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes("[Main]");
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Close();
		}
		Encoding encoding = DetectCode(text);
		paramFile.CheckRecipeDeleteDuplicateSection(text, encoding);
		if (1 == 0)
		{
			MessageBox.Show(string.Format("第{0}站Recipe讀取錯誤，檔案尺寸為0，請檢查Recipe內容。{1}", index + 1, text), "嚴重錯誤");
			return;
		}
		StationINI = new HiniFile(text, ";");
		paramFile.SetINI(StationINI);
		paramFile.SetIniFile(text, index, encoding);
		paramFile.SetRecipeName(recipename);
		paramFile.SetStationIndex(StationIndex);
		paramFile.ReadAllParam();
		paramFile_Backup.SetINI(StationINI);
		paramFile_Backup.ReadAllParam();
		paramFile.SoftCorrectionON = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Encoding DetectCode(string files)
	{
		using Stream fileStream = File.OpenRead(files);
		string text = DetectFileEncoding(fileStream);
		if (text == "ISO-8859-1")
		{
			return Encoding.Default;
		}
		if (text == "utf-8")
		{
			return Encoding.UTF8;
		}
		return Encoding.Unicode;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string DetectFileEncoding(Stream fileStream)
	{
		Encoding encoding = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());
		using StreamReader streamReader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
		string result;
		try
		{
			while (!streamReader.EndOfStream)
			{
				streamReader.ReadLine();
			}
			result = streamReader.CurrentEncoding.BodyName;
		}
		catch (Exception)
		{
			result = "ISO-8859-1";
		}
		fileStream.Seek(0L, SeekOrigin.Begin);
		return result;
	}

	public Encoding GetFileEncodeType(string filename)
	{
		using FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		byte[] array = binaryReader.ReadBytes(2);
		if (array[0] >= 239)
		{
			if (array[0] == 239 && array[1] == 187)
			{
				return Encoding.UTF8;
			}
			if (array[0] == 254 && array[1] == byte.MaxValue)
			{
				return Encoding.BigEndianUnicode;
			}
			if (array[0] == byte.MaxValue && array[1] == 254)
			{
				return Encoding.Unicode;
			}
			return Encoding.Default;
		}
		return Encoding.Default;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void ReadModel(int index)
	{
		if (AIDLLExist && aiEnhanceTools != null)
		{
			if (paramFile.LocateParam.EnableAILocate)
			{
				locateTool.SendAIPredictTest();
			}
			if (paramFile.AIEnhanceToolON)
			{
				try
				{
					if (!aiEnhanceTools.ReadModel(ReCreatePB: false, 0, index))
					{
						ErrorMsg += string.Format("第{0}站\n{1}\n", index + 1, aiEnhanceTools.ResultText);
					}
					else
					{
						aiEnhanceTools.SendPredictTest();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("DeltaAIProcess載入錯誤，請檢查AI相關環境是否安裝正確。", "警告");
					ErrorMsg = string.Format("第{0}站 讀取模型錯誤，請檢查DeltaAIProcess AI相關環境是否安裝正確", index + 1);
					CVBuffer.SaveLog(string.Concat("Inspection-SetRecipe()-ReadModel-" + ErrorMsg, ex.ToString()));
				}
			}
			if (paramFile.AIProphetToolON && paramFile.AllowEnableAIProphet)
			{
				try
				{
					if (!aiProphetTools.ReadModel(""))
					{
						ErrorMsg += string.Format("第{0}站\n{1}\n", index + 1, aiProphetTools.ResultText);
					}
				}
				catch (Exception ex2)
				{
					MessageBox.Show("DeltaAIProcess載入錯誤，請檢查AI相關環境是否安裝正確。", "警告");
					ErrorMsg = string.Format("第{0}站 讀取模型錯誤，請檢查DeltaAIProcess AI相關環境是否安裝正確", index + 1);
					CVBuffer.SaveLog(string.Concat("Inspection-SetRecipe()-ReadModel-" + ErrorMsg, ex2.ToString()));
				}
			}
		}
		if (AIDLLExist && aiPredictTools != null && paramFile.AIPredictToolON)
		{
			if (aiPredictTools.DLLMiss)
			{
				ErrorMsg = aiPredictTools.DLLMissStr;
			}
			if (!aiPredictTools.ReadModel(ReCreatePB: false, 0, index))
			{
				ErrorMsg += string.Format("第{0}站\n{1}\n", index + 1, aiPredictTools.ResultText);
			}
		}
		try
		{
			if (Directory.Exists("D:\\"))
			{
				string path = "d:\\AIImage\\AI_Font\\";
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
		}
		catch (Exception ex3)
		{
			CVBuffer.SaveLog("Inspection-SetRecipe()-02-" + ex3.ToString());
		}
		if (AIDLLExist && matchTool != null && paramFile.MatchToolON)
		{
			for (int i = 0; i < 20; i++)
			{
				INSP_MATCHTOOL_PARAM iNSP_MATCHTOOL_PARAM = paramFile.MatchParam[i];
				if (iNSP_MATCHTOOL_PARAM.TrackerON && iNSP_MATCHTOOL_PARAM.EnableAISimilarityCheck)
				{
					string ResultStr = "";
					if (!matchTool[i].ReadModel(i, ref ResultStr))
					{
						ErrorMsg += ResultStr;
					}
				}
			}
		}
		try
		{
			fontTool.UpdateFontPattern();
			for (int j = 0; j < fontTool.GetMaxFontCount(); j++)
			{
				fontTool.FontModelOnnxType[j] = 0;
				fontTool.FontModelReaded[j] = false;
			}
			for (int k = 0; k < fontTool.GetMaxFontCount(); k++)
			{
				if (fontTool.ModelName[k] != "")
				{
					Mat mat = new Mat(64, 64, MatType.CV_8UC1);
					int Score = 0;
					fontTool.GetAIMatchScore(fontTool.ModelName[k], k, mat, mat, ref Score);
				}
			}
		}
		catch (Exception ex4)
		{
			CVBuffer.SaveLog("Inspection-SetRecipe()-03-" + ex4.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffers(List<byte[]> recentbufferlist, List<byte[]> okbufferlist, List<byte[]> ngbufferlist, List<byte[]> retestbufferlist, List<int> okIDList, List<int> ngIDList, List<int> recentIDList, int lastindex, int w, int h, int band)
	{
		RecentBufferList = recentbufferlist;
		OKBufferList = okbufferlist;
		NGBufferList = ngbufferlist;
		RetestBufferList = retestbufferlist;
		OKBufferIDList = okIDList;
		NGBufferIDList = ngIDList;
		RecentBufferIDList = recentIDList;
		LastIndex = lastindex;
		BufferWidth = w;
		BufferHeight = h;
		BufferBand = 3;
		if (RecentBufferList.Count > 0)
		{
			if (LastIndex >= 0 && LastIndex < RecentBufferList.Count)
			{
				buffers.SetBuffer(RecentBufferList[LastIndex], w, h, band);
			}
			else
			{
				buffers.SetBuffer(RecentBufferList.Last(), w, h, band);
			}
		}
		try
		{
			if (NGBufferList != null && NGBufferIDList != null)
			{
				NGParetoBufferList[0] = NGBufferList;
				for (int i = 1; i < 30; i++)
				{
					NGParetoBufferList[i].Clear();
				}
				int num = 0;
				for (int j = 0; j < NGBufferList.Count; j++)
				{
					num = FindSameID(NGBufferIDList[j]);
					byte[] item = NGBufferList[j];
					if (num > 0)
					{
						NGParetoBufferList[num].Add(item);
					}
				}
			}
			else if (NGBufferList != null)
			{
				NGParetoBufferList[0] = NGBufferList;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Inspection-SetBuffers()-01-" + ex.ToString());
		}
	}

	private int FindSameID(int ID)
	{
		int result = 0;
		for (int i = 0; i < ParetoData.Count; i++)
		{
			if (ID == ParetoData[i].ID && ParetoData[i].ParetoID > 0 && ParetoData[i].ParetoID < 30)
			{
				return ParetoData[i].ParetoID;
			}
		}
		return result;
	}

	public void SetBuffers(byte[] bayerbuffer, int w, int h, int band)
	{
		buffers.SetBuffer(bayerbuffer, w, h, band);
	}

	public void SetBuffers(Mat mat)
	{
		buffers.SetBuffer(mat);
	}

	public void SetBuffers(Bitmap bitmap)
	{
		buffers.SetBuffer(bitmap);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public double OfflineBufTest(int buffertype, bool offLine = false, bool UseAI = false)
	{
		double num = 0.0;
		try
		{
			if (buffertype <= 2)
			{
				List<byte[]> list = buffertype switch
				{
					0 => RecentBufferList, 
					1 => OKBufferList, 
					2 => NGBufferList, 
					_ => RecentBufferList, 
				};
				if (list == null)
				{
					return 0.0;
				}
				Mat mat = new Mat();
				if (list.Count <= 0)
				{
					return -1.0;
				}
				int num2 = list.Count / 10;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (OfflineBufTestStop)
					{
						return -1.0;
					}
					num4++;
					buffers.SetBuffer(list[i], BufferWidth, BufferHeight, BufferBand);
					buffers.Src.CopyTo(mat);
					INSPRESULT iNSPRESULT = Inspect(ref mat, offLine, UseAI);
					if (num2 > 0 && i % num2 == 0)
					{
						num3++;
						SaveProcessFile(num3 * 10);
					}
					if (iNSPRESULT.bin == FLOW_BIN.EM_FLOW_OK)
					{
						num5++;
					}
					else
					{
						num6++;
					}
				}
				num = ((num4 <= 0) ? (-1.0) : ((double)num5 / (double)num4 * 100.0));
			}
			else
			{
				string path = buffertype switch
				{
					3 => paramFile.OKPath, 
					4 => paramFile.NGPath, 
					5 => paramFile.OK_Path_A, 
					6 => paramFile.NG_Path_A, 
					_ => paramFile.OKPath, 
				};
				if (!Directory.Exists(path))
				{
					return -1.0;
				}
				string[] array = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				for (int j = 0; j < array.Length; j++)
				{
					string extension = Path.GetExtension(array[j]);
					extension = extension.ToLower();
					if (extension != ".bmp" && extension != ".jpg" && extension != ".tif" && extension != ".png")
					{
						array = array.Take(j).Concat(array.Skip(j + 1)).ToArray();
						j--;
					}
				}
				Mat mat2 = new Mat();
				if (array.Length == 0)
				{
					return -1.0;
				}
				double num7 = 100 / array.Length;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				for (int k = 0; k < array.Length; k++)
				{
					if (OfflineBufTestStop)
					{
						return -1.0;
					}
					mat2 = CVBuffer.ImRead(array[k]);
					if (mat2.Width > 0 && mat2.Height > 0)
					{
						num8++;
						buffers.SetBuffer(mat2);
						buffers.Src.CopyTo(mat2);
						INSPRESULT iNSPRESULT2 = Inspect(ref mat2, offLine, UseAI);
						int num11 = (int)(num7 * (double)(k + 1));
						if (num11 > 100)
						{
							num11 = 100;
						}
						SaveProcessFile(num11);
						if (iNSPRESULT2.bin == FLOW_BIN.EM_FLOW_OK)
						{
							num9++;
						}
						else
						{
							num10++;
						}
					}
				}
				SaveProcessFile(100);
				num = ((num8 <= 0) ? (-1.0) : ((double)num9 / (double)num8 * 100.0));
			}
			return num;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("CVI-CVInspection-OfflineBufTest()" + ex.ToString());
			return -1.0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool OffLineTest(string ImagePath, ref int TotalCount, ref int OKCount, ref string testresult)
	{
		try
		{
			TotalCount = (OKCount = 0);
			testresult = "";
			if (!Directory.Exists(ImagePath))
			{
				testresult = "影像路徑不存在。";
				return false;
			}
			string[] array = Directory.GetFiles(ImagePath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < array.Length; i++)
			{
				string extension = Path.GetExtension(array[i]);
				extension = extension.ToLower();
				if (extension != ".bmp" && extension != ".jpg" && extension != ".tif" && extension != ".png")
				{
					array = array.Take(i).Concat(array.Skip(i + 1)).ToArray();
					i--;
				}
			}
			if (array.Length == 0)
			{
				if (buffers.LanguageType == 1)
				{
					testresult = "找不到任何影像。";
				}
				else if (buffers.LanguageType == 2)
				{
					testresult = "未找到任何影像。";
				}
				else if (buffers.LanguageType == 3)
				{
					testresult = "影像が見つかりません。";
				}
				else
				{
					testresult = "No image found.";
				}
				return false;
			}
			Mat mat = new Mat();
			_ = 100 / array.Length;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int j = 0; j < array.Length; j++)
			{
				mat = CVBuffer.ImRead(array[j]);
				if (mat.Width > 0 && mat.Height > 0)
				{
					num++;
					buffers.SetBuffer(mat);
					buffers.Src.CopyTo(mat);
					if (Inspect(ref mat, offLine: true, UseAI: true).bin == FLOW_BIN.EM_FLOW_OK)
					{
						num2++;
					}
					else
					{
						num3++;
					}
				}
			}
			TotalCount = num;
			OKCount = num2;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("CVI-CVInspection-OfflineTest()" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SaveProcessFile(int value)
	{
		string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string[] files = Directory.GetFiles(text, "*.proc", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
		if (value > 100)
		{
			value = 100;
		}
		using StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}.proc", text, value), append: true);
		streamWriter.Write("{0}", "");
		streamWriter.WriteLine();
	}

	public bool DoLocate()
	{
		return locateTool.DoLocate();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public INSPRESULT Inspect(ref Mat mat, bool offLine = false, bool UseAI = false, int ImageID = -1, int ImageListSource = 0, int ImageListIndex = 0)
	{
		ErrorMsg = "";
		ParetoIndex = 0;
		SIZESTATIC sIZESTATIC = new SIZESTATIC();
		sIZESTATIC.ID = ImageID;
		if (paramFile.AIPredictToolON || paramFile.AIEnhanceToolON || paramFile.AIProphetToolON)
		{
			UseAI = true;
		}
		bool finalRetest = false;
		bool finalNG1 = false;
		bool finalNG2 = false;
		bool finalNG3 = false;
		bool finalNG4 = false;
		bool firstNG = true;
		int num = 0;
		bool InLine = !offLine;
		if (offLine)
		{
			NGRect.Clear();
			NGPt.Clear();
			NGTool.Clear();
			NGToolIndex.Clear();
			for (int i = 0; i < 20; i++)
			{
				InspTimePerTool[i] = 0.0;
			}
			for (int j = 0; j < 18; j++)
			{
				ErrorCount[j] = 0;
			}
			TimeStart();
		}
		INSPRESULT result = default(INSPRESULT);
		if (!paramFile.StationON)
		{
			if (buffers.LanguageType == 1)
			{
				ErrorMsg += "\n 檢測站別未開啟! \n";
			}
			else if (buffers.LanguageType == 2)
			{
				ErrorMsg += "\n 检测站别未开启! \n";
			}
			else if (buffers.LanguageType == 3)
			{
				ErrorMsg += "\n 検測ステーションが未起動です。 \n";
			}
			else
			{
				ErrorMsg += "\n Station Disabled! \n";
			}
			result.bin = FLOW_BIN.EM_FLOW_OK;
			result.ParetoIndex = 0;
			CountInspResult(result, offLine);
			return result;
		}
		LocaetResult = true;
		if (paramFile.LocateToolON)
		{
			LocaetResult = locateTool.DoLocate(offLine);
			sIZESTATIC.LocateWidth = locateTool.LocateWidth;
			sIZESTATIC.LocateHeight = locateTool.LocateHeight;
			sIZESTATIC.LocateAngle = locateTool.LocateAngle;
			sIZESTATIC.LocateCenterX = locateTool.LocateCenterX;
			sIZESTATIC.LocateCenterY = locateTool.LocateCenterY;
			if (LocaetResult)
			{
				iResult.LocaetResult = TOOL_RESULT.EM_OK;
				if (InLine)
				{
					num += 2;
				}
			}
			else
			{
				iResult.LocaetResult = TOOL_RESULT.EM_NG;
				if (!locateTool.LocateSuccess)
				{
					if (InLine)
					{
						iResult.ToolErrorList[num]++;
						num += 2;
					}
				}
				else if (InLine)
				{
					num++;
					iResult.ToolErrorList[num]++;
					num++;
				}
				if (offLine && paramFile.LocateParam.LocateType == 1)
				{
					for (int k = 0; k < 4; k++)
					{
						if (!locateTool.QuadLocateStatus[k])
						{
							switch (k)
							{
							case 0:
								NGRect.Add(paramFile.LocateParam.QuadSearchRectL);
								break;
							case 1:
								NGRect.Add(paramFile.LocateParam.QuadSearchRectR);
								break;
							case 2:
								NGRect.Add(paramFile.LocateParam.QuadSearchRectT);
								break;
							case 3:
								NGRect.Add(paramFile.LocateParam.QuadSearchRectB);
								break;
							}
							NGTool.Add(InspToolsFunciton.EM_LOCATE);
							NGToolIndex.Add(0);
						}
					}
				}
			}
			OpenCvSharp.Rect locateRectTmp = (LocateRect = locateTool.LocateRect);
			LocateRectTmp = locateRectTmp;
			LocateImageRect = locateTool.LocateImageRect;
		}
		else
		{
			iResult.LocaetResult = TOOL_RESULT.EM_NOT_USE;
			OpenCvSharp.Rect locateRectTmp = (locateTool.LocateRect = buffers.SrcRect);
			LocateRectTmp = locateRectTmp;
			locateTool.LocateImageRect = buffers.SrcRect;
			buffers.LocatedRect = buffers.SrcRect;
			int[] array = new int[6];
			for (int l = 0; l < 6; l++)
			{
				array[l] = paramFile.EnhanceParam[l].FunctionIndex;
			}
			buffers.SetLocatedBuffer(buffers.Src, array, UseAIEnhance: true, CreateHSVChannel: false);
		}
		if (offLine)
		{
			buffers.ReSetAiEnhanceMatColor();
		}
		if (!LocaetResult)
		{
			if (offLine)
			{
				double[] inspTimePerTool = InspTimePerTool;
				double num2 = (LocateTime = TimeStop());
				inspTimePerTool[0] = num2;
				ErrorMsg = locateTool.ErrorMsg;
			}
			if (UseAI)
			{
				maskTool.ResetAIMat();
			}
			if (offLine)
			{
				buffers.Dst.CopyTo(mat);
			}
			else if (mat == null)
			{
				mat = new Mat();
			}
			if (!locateTool.LocateSuccess)
			{
				iResult.LocateBIN = paramFile.LocateParam.LocateErrorBin;
				iResult.LocaetErrorCount++;
				if (paramFile.LocateParam.LocateErrorBin != FLOW_BIN.EM_FLOW_OK)
				{
					result.bin = paramFile.LocateParam.LocateErrorBin;
					result.ParetoIndex = 0;
					CountInspResult(result, offLine);
					return result;
				}
				if (offLine)
				{
					if (buffers.LanguageType == 1)
					{
						ErrorMsg += "定位失敗，但分bin選擇OK料盒! \n";
					}
					else if (buffers.LanguageType == 2)
					{
						ErrorMsg += "定位失败，但分bin选择OK料盒! \n";
					}
					else if (buffers.LanguageType == 3)
					{
						ErrorMsg += "位置合わせが失敗しましたが， ビン分けの選択はOKです! \n";
					}
					else
					{
						ErrorMsg += "Locate Fail, but default error bin is set to OK Bin! \n";
					}
				}
				result.bin = FLOW_BIN.EM_FLOW_OK;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				return result;
			}
			iResult.LocateBIN = paramFile.LocateParam.LocateSizeErrorBin;
			iResult.LocaetSizeErrorCount++;
			if (paramFile.LocateParam.ParetoIndex > 0 && paramFile.LocateParam.LocateSizeErrorBin != FLOW_BIN.EM_FLOW_RETESET && ImageID != -1)
			{
				ParetoIndex = paramFile.LocateParam.ParetoIndex;
				if (ParetoIndex != 0)
				{
					ParetoChartTable[ParetoIndex]++;
				}
				pData.ID = ImageID;
				pData.ParetoID = ParetoIndex;
				if (ParetoData.Count >= 1100)
				{
					ParetoData.RemoveAt(ParetoData.Count - 1);
				}
				ParetoData.Add(pData);
			}
			if (SizeStatics.Count >= 1100)
			{
				SizeStatics.RemoveAt(0);
			}
			if (sIZESTATIC.ID >= 0 && !UpdateLast100)
			{
				SizeStatics.Add(sIZESTATIC);
			}
			if (paramFile.LocateParam.LocateSizeErrorBin != FLOW_BIN.EM_FLOW_OK)
			{
				result.bin = paramFile.LocateParam.LocateSizeErrorBin;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (offLine)
			{
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += "定位失敗，但分bin選擇OK料盒! \n";
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += "定位失败，但分bin选择OK料盒! \n";
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += "位置合わせが失敗しましたが， ビン分けの選択はOKです! \n";
				}
				else
				{
					ErrorMsg += "Locate Fail, but default error bin is set to OK Bin! \n";
				}
			}
			result.bin = FLOW_BIN.EM_FLOW_OK;
			result.ParetoIndex = ParetoIndex;
			CountInspResult(result, offLine);
			return result;
		}
		if (buffers.LocatedRect.Width <= 0 || buffers.LocatedRect.Height <= 0 || buffers.LocateImageRect.Width <= 0 || buffers.LocateImageRect.Height <= 0)
		{
			if (offLine)
			{
				buffers.Dst.CopyTo(mat);
			}
			else if (mat == null)
			{
				mat = new Mat();
			}
			if (offLine)
			{
				double[] inspTimePerTool2 = InspTimePerTool;
				double num2 = (LocateTime = TimeStop());
				inspTimePerTool2[0] = num2;
				if (timecount >= 500)
				{
					timecount = 0;
				}
				InspTime[timecount] = LocateTime;
				timecount++;
			}
			if (UseAI)
			{
				maskTool.ResetAIMat();
			}
			if (paramFile.LocateParam.LocateErrorBin == FLOW_BIN.EM_FLOW_RETESET)
			{
				result.bin = FLOW_BIN.EM_FLOW_RETESET;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				return result;
			}
			if (paramFile.LocateParam.LocateErrorBin == FLOW_BIN.EM_FLOW_NG4)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG4;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (paramFile.LocateParam.LocateErrorBin == FLOW_BIN.EM_FLOW_NG3)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG3;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (paramFile.LocateParam.LocateErrorBin == FLOW_BIN.EM_FLOW_NG2)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG2;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (paramFile.LocateParam.LocateErrorBin == FLOW_BIN.EM_FLOW_NG1)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG1;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (paramFile.LocateParam.LocateErrorBin == FLOW_BIN.EM_FLOW_OK)
			{
				result.bin = FLOW_BIN.EM_FLOW_OK;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				return result;
			}
			result.bin = FLOW_BIN.EM_FLOW_RETESET;
			result.ParetoIndex = 0;
			CountInspResult(result, offLine);
			return result;
		}
		if (offLine)
		{
			double[] inspTimePerTool3 = InspTimePerTool;
			double num2 = (LocateTime = TimeStop());
			inspTimePerTool3[0] = num2;
			TimeStart();
		}
		SelectWayDoFlip = false;
		if (paramFile.SelectWayON)
		{
			SelectWayDoFlip = selectwayTool.Inspect();
			if (SelectWayDoFlip)
			{
				OpenCvSharp.Rect locateRect2 = locateTool.LocateRect;
				OpenCvSharp.Rect foundTextRect = selectwayTool.FoundTextRect;
				if (!selectwayTool.TwoWayFlip)
				{
					if (paramFile.SelectWayParam.RotateMethod == 0)
					{
						locateRect2.Left = buffers.LocateImageRect.Width - locateTool.LocateRect.Right - 1;
						foundTextRect.Left = buffers.LocateImageRect.Width - paramFile.SelectWayParam.FoundTextRectL - paramFile.SelectWayParam.FoundTextRectW - 1;
					}
					else if (paramFile.SelectWayParam.RotateMethod == 1)
					{
						locateRect2.Top = buffers.LocateImageRect.Height - locateTool.LocateRect.Bottom - 1;
						foundTextRect.Top = buffers.LocateImageRect.Height - paramFile.SelectWayParam.FoundTextRectT - paramFile.SelectWayParam.FoundTextRectH - 1;
					}
					else if (paramFile.SelectWayParam.RotateMethod == 2)
					{
						locateRect2.Left = buffers.LocateImageRect.Width - locateTool.LocateRect.Right - 1;
						locateRect2.Top = buffers.LocateImageRect.Height - locateTool.LocateRect.Bottom - 1;
						foundTextRect.Left = buffers.LocateImageRect.Width - paramFile.SelectWayParam.FoundTextRectL - paramFile.SelectWayParam.FoundTextRectW - 1;
						foundTextRect.Top = buffers.LocateImageRect.Height - paramFile.SelectWayParam.FoundTextRectT - paramFile.SelectWayParam.FoundTextRectH - 1;
					}
				}
				else if (paramFile.SelectWayParam.RotateMethodB == 0)
				{
					locateRect2.Left = buffers.LocateImageRect.Width - locateTool.LocateRect.Right - 1;
					foundTextRect.Left = buffers.LocateImageRect.Width - paramFile.SelectWayParam.FoundTextRectL - paramFile.SelectWayParam.FoundTextRectW - 1;
				}
				else if (paramFile.SelectWayParam.RotateMethodB == 1)
				{
					locateRect2.Top = buffers.LocateImageRect.Height - locateTool.LocateRect.Bottom - 1;
					foundTextRect.Top = buffers.LocateImageRect.Height - paramFile.SelectWayParam.FoundTextRectT - paramFile.SelectWayParam.FoundTextRectH - 1;
				}
				else if (paramFile.SelectWayParam.RotateMethodB == 2)
				{
					locateRect2.Left = buffers.LocateImageRect.Width - locateTool.LocateRect.Right - 1;
					locateRect2.Top = buffers.LocateImageRect.Height - locateTool.LocateRect.Bottom - 1;
					foundTextRect.Left = buffers.LocateImageRect.Width - paramFile.SelectWayParam.FoundTextRectL - paramFile.SelectWayParam.FoundTextRectW - 1;
					foundTextRect.Top = buffers.LocateImageRect.Height - paramFile.SelectWayParam.FoundTextRectT - paramFile.SelectWayParam.FoundTextRectH - 1;
				}
				paramFile.SelectWayParam.FoundTextRectL = foundTextRect.Left;
				paramFile.SelectWayParam.FoundTextRectT = foundTextRect.Top;
				selectwayTool.FoundTextRect.Left = foundTextRect.Left;
				selectwayTool.FoundTextRect.Top = foundTextRect.Top;
				using (Mat m = new Mat(buffers.Dst, buffers.LocateImageRect))
				{
					buffers.LocatedSrc.CopyTo(m);
				}
				CVBuffer cVBuffer = buffers;
				LocateTool obj = locateTool;
				OpenCvSharp.Rect rect = (LocateRect = locateRect2);
				OpenCvSharp.Rect locateRectTmp = (obj.LocateRect = rect);
				cVBuffer.LocatedRect = locateRectTmp;
			}
		}
		if (offLine)
		{
			buffers.Dst.CopyTo(mat);
		}
		else if (mat == null)
		{
			mat = new Mat();
		}
		if (offLine)
		{
			InspTimePerTool[1] = TimeStop();
			TimeStart();
		}
		SelectFaceResult = 0;
		if (paramFile.SelectFaceON)
		{
			for (int n = 0; n < 4; n++)
			{
				if (selectfaceTool.Inspect(n, offLine))
				{
					SelectFaceResult = n + 1;
					if (!paramFile.SelectFaceParam[0].RowSelectFace && !paramFile.SelectFaceParam[0].RowSelectFace)
					{
						break;
					}
				}
			}
			if (paramFile.SelectFaceParam[0].RowSelectFace)
			{
				if (selectfaceTool.MaxFaceResult[0] > selectfaceTool.MaxFaceResult[1])
				{
					SelectFaceResult = 1;
				}
				else
				{
					SelectFaceResult = 2;
				}
			}
			else if (paramFile.SelectFaceParam[0].RowMinSelectFace)
			{
				if (selectfaceTool.RowMin == 0)
				{
					SelectFaceResult = 1;
				}
				else
				{
					SelectFaceResult = 2;
				}
			}
			if (SelectFaceResult == 0)
			{
				SelectFaceResult = -1;
				if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("選面工具錯誤! \n");
				}
				else if (buffers.LanguageType == 2)
				{
					ErrorMsg += string.Format("选面工具错误! \n");
				}
				else if (buffers.LanguageType == 3)
				{
					ErrorMsg += string.Format("選択面ツールエラー！\n");
				}
				else
				{
					ErrorMsg += string.Format("Select Fance Tools Error, No Match Face! \n");
				}
				ErrorMsg += advLocateTool.ErrorMsg;
				NGTool.Add(InspToolsFunciton.EM_SELECTFACE);
				NGToolIndex.Add(0);
				ErrorCount[2]++;
				if (InLine)
				{
					iResult.ToolErrorList[num]++;
					num++;
				}
				if (SizeStatics.Count >= 1100)
				{
					SizeStatics.RemoveAt(0);
				}
				if (sIZESTATIC.ID >= 0 && !finalRetest && !UpdateLast100)
				{
					SizeStatics.Add(sIZESTATIC);
				}
				result.bin = paramFile.SelectFaceParam[0].ErrorBin;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				return result;
			}
			num++;
		}
		if (offLine)
		{
			InspTimePerTool[2] = TimeStop();
			TimeStart();
		}
		if (EnableLightCorrection || EnableLightMonitor || EnableSetupLightMonitorBaseValue)
		{
			lightmonitorTool.InspectRGB();
		}
		if (StartSoftCorrection)
		{
			if (paramFile.SoftCorrSetup_R < 0 || paramFile.SoftCorrSetup_G < 0 || paramFile.SoftCorrSetup_B < 0)
			{
				if (buffers.LanguageType == 0)
				{
					ErrorMsg += string.Format("Please Setup Soft Correction First! \n");
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg += string.Format("尚未寫入軟體校正基準值! \n");
				}
				else
				{
					ErrorMsg += string.Format("尚未写入软体校正基准值! \n");
				}
				result.bin = FLOW_BIN.EM_FLOW_RETESET;
				result.ParetoIndex = 0;
				return result;
			}
			lightmonitorTool.InspectRGB();
		}
		if (paramFile.EnahnceToolON)
		{
			for (int num6 = 0; num6 < MaxEnhance; num6++)
			{
				enhanceTool.Inspect(num6, offLine);
			}
			enhanceTool.DoLABFinish = false;
			enhanceTool.DoLUVFinish = false;
		}
		if (offLine)
		{
			InspTimePerTool[3] = TimeStop();
			TimeStart();
		}
		AdvlocateResult = true;
		paramFile.VSLocateParam[0].TrackerON = true;
		paramFile.VSLocateParam[0].Value = LocateRect.Left;
		paramFile.VSLocateParam[1].TrackerON = true;
		paramFile.VSLocateParam[1].Value = LocateRect.Left + LocateRect.Width;
		paramFile.HSLocateParam[0].TrackerON = true;
		paramFile.HSLocateParam[0].Value = LocateRect.Top;
		paramFile.HSLocateParam[1].TrackerON = true;
		paramFile.HSLocateParam[1].Value = LocateRect.Top + LocateRect.Height;
		paramFile.AdvLocateParam[0].TrackerON = true;
		paramFile.AdvLocateParam[0].L = LocateRect.Left;
		paramFile.AdvLocateParam[0].T = LocateRect.Top;
		paramFile.AdvLocateParam[0].R = LocateRect.Left + LocateRect.Width;
		paramFile.AdvLocateParam[0].B = LocateRect.Top + LocateRect.Height;
		paramFile.AdvLocateParam[0].Width = LocateRect.Width;
		paramFile.AdvLocateParam[0].Height = LocateRect.Height;
		paramFile.AdvLocateParam[0].InsideRect = LocateRect;
		sIZESTATIC.AdvAWidth[0] = LocateRect.Width;
		sIZESTATIC.AdvAHeight[0] = LocateRect.Height;
		if (offLine)
		{
			if (buffers.LanguageType == 1)
			{
				paramFile.VSLocateParam[0].TrackerName = "定位左邊";
				paramFile.VSLocateParam[1].TrackerName = "定位右邊";
				paramFile.HSLocateParam[0].TrackerName = "定位上邊";
				paramFile.HSLocateParam[1].TrackerName = "定位下邊";
				paramFile.AdvLocateParam[0].TrackerName = "定位框";
			}
			else if (buffers.LanguageType == 2)
			{
				paramFile.VSLocateParam[0].TrackerName = "定位左边";
				paramFile.VSLocateParam[1].TrackerName = "定位右边";
				paramFile.HSLocateParam[0].TrackerName = "定位上边";
				paramFile.HSLocateParam[1].TrackerName = "定位下边";
				paramFile.AdvLocateParam[0].TrackerName = "定位框";
			}
			else if (buffers.LanguageType == 3)
			{
				paramFile.VSLocateParam[0].TrackerName = "左側の位置決め";
				paramFile.VSLocateParam[1].TrackerName = "右側の位置決め";
				paramFile.HSLocateParam[0].TrackerName = "上側の位置決め";
				paramFile.HSLocateParam[1].TrackerName = "下側の位置決め";
				paramFile.AdvLocateParam[0].TrackerName = "位置決めボックス";
			}
			else
			{
				paramFile.VSLocateParam[0].TrackerName = "Locate Left";
				paramFile.VSLocateParam[1].TrackerName = "Locate Right";
				paramFile.HSLocateParam[0].TrackerName = "Locate Top";
				paramFile.HSLocateParam[1].TrackerName = "Locate Bottom";
				paramFile.AdvLocateParam[0].TrackerName = "Locate Rect";
			}
		}
		AIProphetResult = true;
		if (aiProphetTools != null && paramFile.AIProphetToolON && paramFile.AllowEnableAIProphet)
		{
			AIProphetResult = aiProphetTools.Inspect(offLine, ImageID);
			if (!AIProphetResult)
			{
				ErrorCount[17]++;
				AIProphetResult = false;
				result.bin = paramFile.AIProphetParam.ErrorBin;
				result.ParetoIndex = paramFile.AIProphetParam.ParetoIndex;
				if (InLine)
				{
					iResult.AIProphetErrorCount++;
					iResult.ToolErrorList[num]++;
				}
				if (result.bin == FLOW_BIN.EM_FLOW_RETESET)
				{
					result.ParetoIndex = 0;
					finalRetest = true;
				}
				else if (result.bin == FLOW_BIN.EM_FLOW_NG4)
				{
					finalNG4 = true;
				}
				else if (result.bin == FLOW_BIN.EM_FLOW_NG3)
				{
					finalNG3 = true;
				}
				else if (result.bin == FLOW_BIN.EM_FLOW_NG2)
				{
					finalNG2 = true;
				}
				else if (result.bin == FLOW_BIN.EM_FLOW_NG1)
				{
					finalNG1 = true;
				}
				ErrorMsg += aiProphetTools.ResultText;
			}
			num++;
			sIZESTATIC.AIProphetScore = aiProphetTools.FinalScore;
		}
		if (offLine)
		{
			InspTimePerTool[17] = TimeStop();
			TimeStart();
		}
		if (UseAI)
		{
			maskTool.ResetAIMat();
		}
		firstNG = true;
		bool flag = false;
		if (paramFile.MaskParam[0].TrackerON && paramFile.AIEnhanceToolON)
		{
			maskTool.SetStationIndex(StationIndex);
			maskTool.SetSaveAIImage(SaveAIImage);
			maskTool.Inspect(0, SelectFaceResult, UseAI, offLine);
			if (maskTool.AIMat.Width > 0 && maskTool.AIMat.Height > 0)
			{
				flag = true;
			}
		}
		if (aiEnhanceTools != null && paramFile.AIEnhanceToolON && AIDLLExist)
		{
			if (!flag)
			{
				if (offLine)
				{
					InspTimePerTool[14] = TimeStop();
				}
				result.bin = FLOW_BIN.EM_FLOW_RETESET;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				ErrorMsg += "AI Image Create error. \n";
				return result;
			}
			if (!aiEnhanceTools.Inspect(maskTool.AIMat, offLine, SelectFaceResult))
			{
				if (offLine)
				{
					InspTimePerTool[14] = TimeStop();
				}
				result.bin = FLOW_BIN.EM_FLOW_RETESET;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				ErrorMsg += aiEnhanceTools.ResultText;
				return result;
			}
			buffers.SetAiEnhanceMat(aiEnhanceTools.AIMatR, aiEnhanceTools.AIMatG, aiEnhanceTools.AIMatB, maskTool.AIRect);
			if (offLine)
			{
				buffers.SetAiEnhanceMatColor(aiEnhanceTools.DestMatColor, maskTool.AIRect);
			}
		}
		if (offLine)
		{
			InspTimePerTool[14] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		AdvlocateResult = true;
		advLocateTool.ErrorMsg = "";
		double num7 = 0.0;
		if (paramFile.AdvLocateToolON)
		{
			iResult.VScanResult[0] = TOOL_RESULT.EM_OK;
			iResult.HScanResult[0] = TOOL_RESULT.EM_OK;
			iResult.VScanResult[1] = TOOL_RESULT.EM_OK;
			iResult.HScanResult[1] = TOOL_RESULT.EM_OK;
			for (int num8 = 2; num8 < 20; num8++)
			{
				advLocateTool.doVScan(num8, ref iResult.VScanResult[num8], SelectFaceResult, offLine);
				if (iResult.VScanResult[num8] == TOOL_RESULT.EM_NG)
				{
					AdvlocateResult = false;
					iResult.VScanBIN[num8] = paramFile.VSLocateParam[num8].ErrorBin;
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [進階定位工具]\n";
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += "\n [进阶定位工具]\n";
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += "\n [上級位置合わせ]\n";
							}
							else
							{
								ErrorMsg += "\n [Adv.Locate Tool]\n";
							}
							firstNG = false;
						}
						ErrorMsg += advLocateTool.ErrorMsg;
						if (advLocateTool.InspeRect.Width > 0 && advLocateTool.InspeRect.Height > 0)
						{
							NGRect.Add(advLocateTool.InspeRect);
							NGTool.Add(InspToolsFunciton.EM_ADVLOCATE_V);
							NGToolIndex.Add(num8);
						}
						ErrorCount[4]++;
					}
					if (InLine)
					{
						iResult.VScanErrorCount[num8]++;
						iResult.ToolErrorList[num]++;
						num++;
					}
					if (paramFile.VSLocateParam[num8].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.VSLocateParam[num8].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.VSLocateParam[num8].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.VSLocateParam[num8].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.VSLocateParam[num8].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.VScanBIN[num8] == FLOW_BIN.EM_FLOW_RETESET)
					{
						if (offLine)
						{
							InspTimePerTool[4] = TimeStop();
						}
						result.bin = FLOW_BIN.EM_FLOW_RETESET;
						result.ParetoIndex = 0;
						CountInspResult(result, offLine);
						return result;
					}
					if (iResult.VScanBIN[num8] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.VScanBIN[num8] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.VScanBIN[num8] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.VScanBIN[num8] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg += "進階定位[垂直]失敗，但分bin選擇OK料盒! \n";
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += "进阶定位[垂直]失败，但分bin选择OK料盒! \n";
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += "上級位置合わせ（垂直）が失敗しましたが， ビン分けの選択はOKです!\n";
						}
						else
						{
							ErrorMsg += "Adv.Locate [Vertical] Fail, but default error bin is set to OK Bin! \n";
						}
					}
				}
				else if (iResult.VScanResult[num8] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
			firstNG = true;
			advLocateTool.ErrorMsg = "";
			for (int num9 = 2; num9 < 20; num9++)
			{
				advLocateTool.doHScan(num9, ref iResult.HScanResult[num9], SelectFaceResult, offLine);
				if (iResult.HScanResult[num9] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [進階定位工具]\n";
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += "\n [进阶定位工具]\n";
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += "\n [上級位置合わせ]\n";
							}
							else
							{
								ErrorMsg += "\n [Adv.Locate Tool]\n";
							}
							firstNG = false;
						}
						ErrorMsg += advLocateTool.ErrorMsg;
						if (advLocateTool.InspeRect.Width > 0 && advLocateTool.InspeRect.Height > 0)
						{
							NGRect.Add(advLocateTool.InspeRect);
							NGTool.Add(InspToolsFunciton.EM_ADVLOCATE_H);
							NGToolIndex.Add(num9);
						}
						ErrorCount[4]++;
					}
					AdvlocateResult = false;
					iResult.HScanBIN[num9] = paramFile.HSLocateParam[num9].ErrorBin;
					if (InLine)
					{
						iResult.HScanErrorCount[num9]++;
						iResult.ToolErrorList[num]++;
						num++;
					}
					if (paramFile.HSLocateParam[num9].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.HSLocateParam[num9].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.HSLocateParam[num9].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.HSLocateParam[num9].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.HSLocateParam[num9].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.HScanBIN[num9] == FLOW_BIN.EM_FLOW_RETESET)
					{
						if (offLine)
						{
							InspTimePerTool[4] = TimeStop();
						}
						result.bin = FLOW_BIN.EM_FLOW_RETESET;
						result.ParetoIndex = 0;
						CountInspResult(result, offLine);
						return result;
					}
					if (iResult.HScanBIN[num9] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.HScanBIN[num9] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.HScanBIN[num9] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.HScanBIN[num9] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg += "進階定位[水平]失敗，但分bin選擇OK料盒! \n";
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += "进阶定位[水平]失败，但分bin选择OK料盒! \n";
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += "上級位置合わせ（水平）が失敗しましたが， ビン分けの選択はOKです!\n";
						}
						else
						{
							ErrorMsg += "Adv.Locate [Horizontal] Fail, but default error bin is set to OK Bin! \n";
						}
					}
				}
				else if (iResult.HScanResult[num9] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
			firstNG = true;
			advLocateTool.ErrorMsg = "";
			for (int num10 = 1; num10 < 20; num10++)
			{
				advLocateTool.Inspect(num10, ref iResult.AdvLocateResult[num10], SelectFaceResult, offLine);
				sIZESTATIC.AdvAWidth[num10] = advLocateTool.AdvAWidth;
				sIZESTATIC.AdvAHeight[num10] = advLocateTool.AdvAHeight;
				if (iResult.AdvLocateResult[num10] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [進階定位工具]\n";
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += "\n [进阶定位工具]\n";
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += "\n [上級位置合わせ]\n";
							}
							else
							{
								ErrorMsg += "\n [Adv.Locate Tool]\n";
							}
							firstNG = false;
						}
						ErrorMsg += advLocateTool.ErrorMsg;
						if (advLocateTool.NGRect.Width > 0 && advLocateTool.NGRect.Height > 0)
						{
							NGRect.Add(advLocateTool.NGRect);
							NGTool.Add(InspToolsFunciton.EM_ADVLOCATE_A);
							NGToolIndex.Add(num10);
						}
						ErrorCount[4]++;
					}
					AdvlocateResult = false;
					iResult.AdvLocateBIN[num10] = paramFile.AdvLocateParam[num10].ErrorBin;
					if (InLine)
					{
						iResult.AdvLocateErrorCount[num10]++;
						iResult.ToolErrorList[num]++;
						num++;
					}
					if (paramFile.AdvLocateParam[num10].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.AdvLocateParam[num10].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.AdvLocateParam[num10].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.AdvLocateParam[num10].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.AdvLocateParam[num10].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.AdvLocateBIN[num10] == FLOW_BIN.EM_FLOW_RETESET)
					{
						if (offLine)
						{
							InspTimePerTool[4] = TimeStop();
						}
						result.bin = FLOW_BIN.EM_FLOW_RETESET;
						result.ParetoIndex = 0;
						CountInspResult(result, offLine);
						return result;
					}
					if (iResult.AdvLocateBIN[num10] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.AdvLocateBIN[num10] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.AdvLocateBIN[num10] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.AdvLocateBIN[num10] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg += "進階定位[內部定位]失敗，但分bin選擇OK料盒! \n";
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += "进阶定位[內部定位]失败，但分bin选择OK料盒! \n";
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += "上級位置合わせ（内部位置）が失敗しましたが， ビン分けの選択はOKです!\n";
						}
						else
						{
							ErrorMsg += "Adv.Locate [Inside Locate] Fail, but default error bin is set to OK Bin! \n";
						}
					}
				}
				else if (iResult.AdvLocateResult[num10] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
			firstNG = true;
			advLocateTool.ErrorMsg = "";
			paramFile.JudgeWithAdvMeasureResult = true;
			AdvMeasureResult = true;
			for (int num11 = 0; num11 < 40; num11++)
			{
				advLocateTool.doAdvMeasure(num11, ref iResult.AdvMeasureResult[num11], SelectFaceResult, offLine);
				sIZESTATIC.AdvM[num11] = advLocateTool.MeasureDistance;
				if (offLine)
				{
					num7 += advLocateTool.OfflineTime;
				}
				if (iResult.AdvMeasureResult[num11] == TOOL_RESULT.EM_NG)
				{
					if (paramFile.EnableJudgeWithAdvMeasure && (num11 == paramFile.JudgeWithAdvMeasureIndexA - 1 || num11 == paramFile.JudgeWithAdvMeasureIndexB - 1))
					{
						paramFile.JudgeWithAdvMeasureResult = false;
					}
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [進階量測工具]\n";
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += "\n [进阶量测工具]\n";
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += "\n [上級寸法測定]\n";
							}
							else
							{
								ErrorMsg += "\n [Adv.Measure Tool]\n";
							}
							firstNG = false;
						}
						ErrorMsg += advLocateTool.ErrorMsg;
						NGPt.Add(advLocateTool.NGPt1);
						NGPt.Add(advLocateTool.NGPt2);
						if (advLocateTool.NGRect.Width > 0 && advLocateTool.NGRect.Height > 0)
						{
							NGRect.Add(advLocateTool.NGRect);
							NGToolIndex.Add(num11);
							NGTool.Add(InspToolsFunciton.EM_ADVLOCATE_M);
						}
						if (advLocateTool.NGRect2.Width > 0 && advLocateTool.NGRect2.Height > 0)
						{
							NGRect.Add(advLocateTool.NGRect2);
							NGToolIndex.Add(num11);
							NGTool.Add(InspToolsFunciton.EM_ADVLOCATE_M);
						}
						ErrorCount[4]++;
					}
					iResult.AdvMeasureBIN[num11] = paramFile.AdvLocateMParam[num11].ErrorBin;
					AdvMeasureResult = false;
					if (InLine)
					{
						iResult.AdvMeasureErrorCount[num11]++;
						iResult.ToolErrorList[num]++;
						num++;
					}
					if (paramFile.AdvLocateMParam[num11].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.AdvLocateMParam[num11].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.AdvLocateMParam[num11].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.AdvLocateMParam[num11].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.AdvLocateMParam[num11].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.AdvMeasureBIN[num11] == FLOW_BIN.EM_FLOW_RETESET)
					{
						if (offLine)
						{
							InspTimePerTool[4] = TimeStop();
						}
						result.bin = FLOW_BIN.EM_FLOW_RETESET;
						result.ParetoIndex = 0;
						CountInspResult(result, offLine);
						return result;
					}
					if (iResult.AdvMeasureBIN[num11] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.AdvMeasureBIN[num11] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.AdvMeasureBIN[num11] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.AdvMeasureBIN[num11] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 1)
						{
							ErrorMsg += "進階定位[進階量測]失敗，但分bin選擇OK料盒! \n";
						}
						else if (buffers.LanguageType == 2)
						{
							ErrorMsg += "进阶定位[进阶量测]失败，但分bin选择OK料盒! \n";
						}
						else if (buffers.LanguageType == 3)
						{
							ErrorMsg += "上級位置合わせ（上級寸法測定）が失敗しましたが， ビン分けの選択はOKです!\n";
						}
						else
						{
							ErrorMsg += "Adv.Locate [Adv. Measure] Fail, but default error bin is set to OK Bin! \n";
						}
					}
				}
				else if (iResult.AdvMeasureResult[num11] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[4] = TimeStop();
			InspTimePerTool[4] -= num7;
			TimeStart();
		}
		if (!AdvlocateResult)
		{
			if (offLine)
			{
				InspTimePerTool[5] = TimeStop();
				LocateTime = 0.0;
				for (int num12 = 0; num12 < 5; num12++)
				{
					LocateTime += InspTimePerTool[num12];
				}
				if (timecount >= 500)
				{
					timecount = 0;
				}
				InspTime[timecount] = LocateTime;
				timecount++;
			}
			if (ImageID != -1 && ParetoIndex > 0)
			{
				pData.ID = ImageID;
				pData.ParetoID = ParetoIndex;
				if (ParetoData.Count >= 1000)
				{
					ParetoData.RemoveAt(ParetoData.Count - 1);
				}
				ParetoData.Add(pData);
			}
			if (ParetoIndex > 0 && !EnableMultiParetoMode)
			{
				ParetoChartTable[ParetoIndex]++;
			}
			if (finalNG1 | finalNG2 | finalNG3 | finalNG4)
			{
				if (SizeStatics.Count >= 1100)
				{
					SizeStatics.RemoveAt(0);
				}
				if (sIZESTATIC.ID >= 0 && !finalRetest && !UpdateLast100)
				{
					SizeStatics.Add(sIZESTATIC);
				}
			}
			if (finalRetest)
			{
				if (offLine)
				{
					InspTimePerTool[4] = TimeStop();
				}
				result.bin = FLOW_BIN.EM_FLOW_RETESET;
				result.ParetoIndex = 0;
				CountInspResult(result, offLine);
				return result;
			}
			if (!ReverseBinPriority)
			{
				if (finalNG4)
				{
					result.bin = FLOW_BIN.EM_FLOW_NG4;
					result.ParetoIndex = ParetoIndex;
					CountInspResult(result, offLine);
					return result;
				}
				if (finalNG3)
				{
					result.bin = FLOW_BIN.EM_FLOW_NG3;
					result.ParetoIndex = ParetoIndex;
					CountInspResult(result, offLine);
					return result;
				}
				if (finalNG2)
				{
					result.bin = FLOW_BIN.EM_FLOW_NG2;
					result.ParetoIndex = ParetoIndex;
					CountInspResult(result, offLine);
					return result;
				}
				result.bin = FLOW_BIN.EM_FLOW_NG1;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (finalNG1)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG1;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (finalNG2)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG2;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (finalNG3)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG3;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			result.bin = FLOW_BIN.EM_FLOW_NG4;
			result.ParetoIndex = ParetoIndex;
			CountInspResult(result, offLine);
			return result;
		}
		firstNG = true;
		if (paramFile.MaskToolON)
		{
			maskTool.SetStationIndex(StationIndex);
			maskTool.SetSaveAIImage(SaveAIImage);
			for (int num13 = 1; num13 < 30; num13++)
			{
				maskTool.Inspect(num13, SelectFaceResult, UseAI, offLine);
			}
		}
		if (offLine)
		{
			InspTimePerTool[6] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		MeasureResult = true;
		measuretool.ErrorMsg = "";
		if (paramFile.MeasureToolON)
		{
			for (int num14 = 0; num14 < 20; num14++)
			{
				measuretool.MeasureWidth(num14, ref iResult.MeaWidthResult[num14], SelectFaceResult, offLine);
				sIZESTATIC.MeasureW[num14] = measuretool.AvgWidth;
				if (iResult.MeaWidthResult[num14] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [尺寸量測工具]\n";
							}
							else if (buffers.LanguageType == 2)
							{
								ErrorMsg += "\n [尺寸量测工具]\n";
							}
							else if (buffers.LanguageType == 3)
							{
								ErrorMsg += "\n [寸法測定]\n";
							}
							else
							{
								ErrorMsg += "\n [Measure Tool]\n";
							}
							firstNG = false;
						}
						ErrorMsg += measuretool.ErrorMsg;
						if (measuretool.InspeRect[0].Width > 0 && measuretool.InspeRect[0].Height > 0 && measuretool.InspeRect[1].Width > 0 && measuretool.InspeRect[1].Height > 0)
						{
							NGRect.Add(measuretool.InspeRect[0]);
							NGRect.Add(measuretool.InspeRect[1]);
							NGTool.Add(InspToolsFunciton.EM_MEASURE_W);
							NGToolIndex.Add(num14);
							NGTool.Add(InspToolsFunciton.EM_MEASURE_W);
							NGToolIndex.Add(num14);
						}
						ErrorCount[5]++;
					}
					iResult.MeaWidthBIN[num14] = paramFile.MeaWidthParam[num14].ErrorBin;
					MeasureResult = false;
					if (InLine)
					{
						iResult.MeaWidthErrorCount[num14]++;
						iResult.ToolErrorList[num]++;
						num++;
					}
					if (paramFile.MeaWidthParam[num14].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.MeaWidthParam[num14].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.MeaWidthParam[num14].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.MeaWidthParam[num14].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.MeaWidthParam[num14].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.MeaWidthBIN[num14] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.MeaWidthBIN[num14] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.MeaWidthBIN[num14] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.MeaWidthBIN[num14] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.MeaWidthBIN[num14] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Measure Tool [Width] inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "尺寸量測[寬度]檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "尺寸量测[宽度]检测失败，但分bin选择OK料盒! \n";
						}
					}
				}
				else if (iResult.MeaWidthResult[num14] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
			measuretool.ErrorMsg = "";
			for (int num15 = 0; num15 < 20; num15++)
			{
				measuretool.MeasureHeight(num15, ref iResult.MeaHeightResult[num15], SelectFaceResult, offLine);
				if (!finalRetest)
				{
					sIZESTATIC.MeasureH[num15] = measuretool.AvgHeight;
				}
				if (iResult.MeaHeightResult[num15] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 0)
							{
								ErrorMsg += "\n [Measure Tool]\n";
							}
							else if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [尺寸量測工具]\n";
							}
							else
							{
								ErrorMsg += "\n [尺寸量测工具]\n";
							}
							firstNG = false;
						}
						ErrorMsg += measuretool.ErrorMsg;
						if (measuretool.InspeRect[0].Width > 0 && measuretool.InspeRect[0].Height > 0 && measuretool.InspeRect[1].Width > 0 && measuretool.InspeRect[1].Height > 0)
						{
							NGRect.Add(measuretool.InspeRect[0]);
							NGRect.Add(measuretool.InspeRect[1]);
							NGTool.Add(InspToolsFunciton.EM_MEASURE_H);
							NGToolIndex.Add(num15);
							NGTool.Add(InspToolsFunciton.EM_MEASURE_H);
							NGToolIndex.Add(num15);
						}
						ErrorCount[5]++;
					}
					iResult.MeaHeightBIN[num15] = paramFile.MeaHeightParam[num15].ErrorBin;
					MeasureResult = false;
					if (InLine)
					{
						iResult.MeaHeightErrorCount[num15]++;
						iResult.ToolErrorList[num]++;
						num++;
					}
					if (paramFile.MeaHeightParam[num15].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.MeaHeightParam[num15].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.MeaHeightParam[num15].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.MeaHeightParam[num15].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.MeaHeightParam[num15].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.MeaHeightBIN[num15] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.MeaHeightBIN[num15] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.MeaHeightBIN[num15] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.MeaHeightBIN[num15] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.MeaHeightBIN[num15] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Measure Tool [Height] inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "尺寸量測[高度]檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "尺寸量测[高度]检测失败，但分bin选择OK料盒! \n";
						}
					}
				}
				else if (iResult.MeaHeightResult[num15] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[5] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		MatchResult = true;
		int num16 = num;
		if (paramFile.MatchToolON)
		{
			MatchResult = true;
			int[] array2 = new int[30];
			int num17 = num;
			for (int num18 = 0; num18 < 20; num18++)
			{
				if (paramFile.MatchParam[num18].TrackerON)
				{
					array2[num18] = num17;
					num17++;
				}
				else
				{
					array2[num18] = 0;
				}
			}
			num = num17;
			tempcount++;
			if (tempcount > 100000)
			{
				tempcount = 0;
			}
			for (int num19 = 0; num19 < 20; num19++)
			{
				matchTool[num19].Tracker1ResultText = "";
				matchTool[num19].Inspect(num19, ref iResult.MatchResult[num19], SelectFaceResult, offLine);
				if (offLine && paramFile.SaveAIFontImage2 && paramFile.MatchParam[num19].TrackerON)
				{
					string arg = "NG";
					if (iResult.MatchResult[num19] == TOOL_RESULT.EM_OK)
					{
						arg = "OK";
					}
					string text = string.Format("D:\\AIImage\\AI_Font\\S{0}\\{1}-{2}\\", StationIndex + 1, num19 + 1, arg);
					try
					{
						if (!Directory.Exists(text))
						{
							Directory.CreateDirectory(text);
						}
						DateTime now = DateTime.Now;
						string fileName = string.Format("{0}{1}-{2}-{3}.png", text, paramFile.MatchParam[num19].TrackerName, tempcount, now.ToString("yyyy-MM-dd-HH-mm-ss"));
						if (matchTool[num19].FoundMat.Width > 0 && matchTool[num19].FoundMat.Height > 0)
						{
							matchTool[num19].FoundMat.SaveImage(fileName);
						}
					}
					catch
					{
					}
				}
				if (iResult.MatchResult[num19] != TOOL_RESULT.EM_NG)
				{
					continue;
				}
				if (offLine)
				{
					if (firstNG)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "\n [Match Tool]\n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "\n [比對工具]\n";
						}
						else
						{
							ErrorMsg += "\n [比对工具]\n";
						}
						firstNG = false;
					}
					if (matchTool[num19].Tracker1ResultText != "")
					{
						ErrorMsg += matchTool[num19].Tracker1ResultText;
						if (matchTool[num19].FoundRect.Width > 0 && matchTool[num19].FoundRect.Height > 0)
						{
							NGRect.Add(matchTool[num19].FoundRect);
							NGTool.Add(InspToolsFunciton.EM_MATCH);
							NGToolIndex.Add(num19);
						}
						else if (matchTool[num19].LocateRect.Width > 0 && matchTool[num19].LocateRect.Height > 0)
						{
							NGRect.Add(matchTool[num19].LocateRect);
							NGTool.Add(InspToolsFunciton.EM_MATCH);
							NGToolIndex.Add(num19);
						}
					}
				}
				iResult.MatchBIN[num19] = paramFile.MatchParam[num19].ErrorBin;
				MatchResult = false;
				ErrorCount[12]++;
				if (InLine)
				{
					iResult.MatchErrorCount[num19]++;
				}
				if (iResult.MatchBIN[num19] == FLOW_BIN.EM_FLOW_RETESET)
				{
					finalRetest = true;
				}
				else if (iResult.MatchBIN[num19] == FLOW_BIN.EM_FLOW_NG4)
				{
					finalNG4 = true;
				}
				else if (iResult.MatchBIN[num19] == FLOW_BIN.EM_FLOW_NG3)
				{
					finalNG3 = true;
				}
				else if (iResult.MatchBIN[num19] == FLOW_BIN.EM_FLOW_NG2)
				{
					finalNG2 = true;
				}
				else if (iResult.MatchBIN[num19] == FLOW_BIN.EM_FLOW_NG1)
				{
					finalNG1 = true;
				}
				else if (offLine)
				{
					if (buffers.LanguageType == 0)
					{
						ErrorMsg += "Match Tool inspect Fail, but default error bin is set to OK Bin! \n";
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += "比對工具檢測失敗，但分bin選擇OK料盒! \n";
					}
					else
					{
						ErrorMsg += "比对工具检测失败，但分bin选择OK料盒! \n";
					}
				}
				if (InLine)
				{
					iResult.ToolErrorList[array2[num19]]++;
				}
			}
			if (!MatchResult)
			{
				for (int num20 = 0; num20 < 20; num20++)
				{
					if (iResult.MatchResult[num20] == TOOL_RESULT.EM_NG && iResult.MatchBIN[num20] != FLOW_BIN.EM_FLOW_RETESET && paramFile.MatchParam[num20].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.MatchParam[num20].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							ParetoIndex = paramFile.MatchParam[num20].ParetoIndex;
						}
						else if (paramFile.MatchParam[num20].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.MatchParam[num20].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[12] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		FontToolInspResult = true;
		fontTool.Tracker1ResultText = "";
		if (paramFile.FontToolON)
		{
			tempcount++;
			if (tempcount > 100000)
			{
				tempcount = 0;
			}
			if (paramFile.FontToolParam.AllFont.Length == 8)
			{
				fontTool.InspectF8(ref iResult.FontToolResult, SelectFaceResult, offLine);
			}
			else if (paramFile.FontToolParam.AllFont.Length >= 5)
			{
				fontTool.InspectF9(ref iResult.FontToolResult, SelectFaceResult, offLine);
			}
			else
			{
				fontTool.Inspect(ref iResult.FontToolResult, SelectFaceResult, offLine);
			}
			if (offLine && paramFile.SaveAIFontImage)
			{
				string text2 = "NG";
				try
				{
					DateTime now2 = DateTime.Now;
					for (int num21 = 0; num21 < fontTool.GetMaxFontCount(); num21++)
					{
						if (fontTool.GetFontChar(num21) == "" || !fontTool.FirstMatchALLResult)
						{
							continue;
						}
						Mat mat2 = fontTool.FoundFontMat[num21];
						text2 = ((!fontTool.FontResult[num21]) ? "NG" : "OK");
						string text3 = string.Format("D:\\AIImage\\AI_Font\\S{0}\\{1}\\{2}\\{3}\\", StationIndex + 1, now2.ToString("yyyy-MM-dd"), fontTool.GetFontChar(num21), text2);
						string text4 = string.Format("D:\\AIImage\\AI_Font\\S{0}\\{1}\\{2}_OCR\\{3}\\", StationIndex + 1, now2.ToString("yyyy-MM-dd"), fontTool.GetFontChar(num21), text2);
						if (!Directory.Exists(text3))
						{
							Directory.CreateDirectory(text3);
						}
						if (mat2.Width > 0 && mat2.Height > 0 && !fontTool.FontConnectNG)
						{
							string fileName2 = string.Format("{0}{1}-{2}-{3}-{4}.png", text3, fontTool.GetFontChar(num21), num21 + 1, tempcount, now2.ToString("yyyy-MM-dd-HH-mm-ss"));
							if (paramFile.FontToolParam.FontAlignVertical)
							{
								Cv2.Transpose(mat2, mat2);
								Cv2.Flip(mat2, mat2, FlipMode.Y);
							}
							mat2.SaveImage(fileName2);
							if (!Directory.Exists(text4))
							{
								Directory.CreateDirectory(text4);
							}
							fileName2 = string.Format("{0}{1}-{2}-{3}-{4}.png", text4, fontTool.GetFontChar(num21), num21 + 1, tempcount, now2.ToString("yyyy-MM-dd-HH-mm-ss"));
							if (fontTool.FontIsBlack)
							{
								Cv2.BitwiseNot(mat2, mat2);
							}
							mat2.SaveImage(fileName2);
						}
					}
				}
				catch
				{
				}
			}
			if (iResult.FontToolResult == TOOL_RESULT.EM_NG)
			{
				if (paramFile.ThroughputFile.CurrentClient == 3 && paramFile.MatchToolON)
				{
					if (!fontTool.FontResult[0] && paramFile.MatchParam[0].TrackerON)
					{
						iResult.MatchErrorCount[0]++;
						iResult.ToolErrorList[num16]++;
					}
					if (!fontTool.FontResult[1] && paramFile.MatchParam[1].TrackerON)
					{
						iResult.MatchErrorCount[1]++;
						iResult.ToolErrorList[num16 + 1]++;
					}
					if (!fontTool.FontResult[2] && paramFile.MatchParam[2].TrackerON)
					{
						iResult.MatchErrorCount[2]++;
						iResult.ToolErrorList[num16 + 2]++;
					}
					if (!fontTool.FontResult[3] && paramFile.MatchParam[3].TrackerON)
					{
						iResult.MatchErrorCount[3]++;
						iResult.ToolErrorList[num16 + 3]++;
					}
					if (fontTool.FontResult[0] && fontTool.FontResult[2] && fontTool.FontResult[2] && fontTool.FontResult[3])
					{
						iResult.MatchErrorCount[0]++;
						iResult.ToolErrorList[num16]++;
					}
				}
				if (offLine)
				{
					if (firstNG)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg = ErrorMsg + "\n [Font Tool] - " + paramFile.FontToolParam.AllFont + "\n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg = ErrorMsg + "\n [文字比對工具] - " + paramFile.FontToolParam.AllFont + "\n";
						}
						else
						{
							ErrorMsg = ErrorMsg + "\n [文字比对工具] - " + paramFile.FontToolParam.AllFont + "\n";
						}
						firstNG = false;
					}
					ErrorMsg += fontTool.Tracker1ResultText;
					for (int num22 = 0; num22 < fontTool.NGRect.Count; num22++)
					{
						if (fontTool.NGRect[num22].Width > 0 && fontTool.NGRect[num22].Height > 0)
						{
							NGRect.Add(fontTool.NGRect[num22]);
							NGTool.Add(InspToolsFunciton.EM_FONT);
							NGToolIndex.Add(0);
						}
					}
				}
				iResult.FontToolBIN = paramFile.FontToolParam.ErrorBin;
				FontToolInspResult = false;
				ErrorCount[15]++;
				if (InLine)
				{
					iResult.FontToolErrorCount++;
				}
				if (paramFile.FontToolParam.ParetoIndex > 0 && ImageID != -1)
				{
					if (EnableMultiParetoMode)
					{
						ParetoChartTable[paramFile.FontToolParam.ParetoIndex]++;
					}
					if (ParetoIndex == 0)
					{
						int paretoIndex = (ParetoIndex = paramFile.FontToolParam.ParetoIndex);
						result.ParetoIndex = paretoIndex;
					}
					else if (paramFile.FontToolParam.ParetoIndex < ParetoIndex)
					{
						int paretoIndex = (ParetoIndex = paramFile.FontToolParam.ParetoIndex);
						result.ParetoIndex = paretoIndex;
					}
				}
				if (iResult.FontToolBIN == FLOW_BIN.EM_FLOW_RETESET)
				{
					finalRetest = true;
				}
				else if (iResult.FontToolBIN == FLOW_BIN.EM_FLOW_NG4)
				{
					finalNG4 = true;
				}
				else if (iResult.FontToolBIN == FLOW_BIN.EM_FLOW_NG3)
				{
					finalNG3 = true;
				}
				else if (iResult.FontToolBIN == FLOW_BIN.EM_FLOW_NG2)
				{
					finalNG2 = true;
				}
				else if (iResult.FontToolBIN == FLOW_BIN.EM_FLOW_NG1)
				{
					finalNG1 = true;
				}
				else if (offLine)
				{
					if (buffers.LanguageType == 0)
					{
						ErrorMsg += "Font inspect Fail, but default error bin is set to OK Bin! \n";
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += "文字比對工具檢測失敗，但分bin選擇OK料盒! \n";
					}
					else
					{
						ErrorMsg += "文字比对工具检测失败，但分bin选择OK料盒! \n";
					}
				}
				if (InLine)
				{
					iResult.ToolErrorList[num]++;
					num++;
				}
			}
			else if (iResult.FontToolResult == TOOL_RESULT.EM_OK)
			{
				num++;
			}
		}
		if (offLine)
		{
			InspTimePerTool[15] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		BinaryResult = true;
		if (paramFile.BinaryToolON)
		{
			int[] bCountList = new int[30];
			int num23 = num;
			for (int num24 = 0; num24 < 30; num24++)
			{
				if (paramFile.BinaryParam[num24].TrackerON)
				{
					bCountList[num24] = num23;
					num23++;
				}
				else
				{
					bCountList[num24] = 0;
				}
			}
			num = num23;
			BinaryResult = true;
			Parallel.For(0, 30, [MethodImpl(MethodImplOptions.NoInlining)] (int num46, ParallelLoopState loopState) =>
			{
				binaryTool[num46].Tracker1ResultText = "";
				binaryTool[num46].Tracker2ResultText = "";
				if (offLine)
				{
					binaryTool[num46].Inspect(num46, ref iResult.BinaryResult[num46], SelectFaceResult, offline: true, ImageListSource, ImageListIndex);
				}
				else
				{
					binaryTool[num46].Inspect(num46, ref iResult.BinaryResult[num46], SelectFaceResult);
				}
				if (iResult.BinaryResult[num46] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						lock (BinaryOffLineLock)
						{
							if (firstNG)
							{
								if (buffers.LanguageType == 0)
								{
									ErrorMsg += "\n [Binary Tool]";
								}
								else if (buffers.LanguageType == 1)
								{
									ErrorMsg += "\n [二值化工具]";
								}
								else
								{
									ErrorMsg += "\n [二值化工具]";
								}
								firstNG = false;
							}
							try
							{
								if (paramFile.BinaryParam[num46].BlobAnalysisSelect == 0)
								{
									if (binaryTool[num46].Tracker1ResultText != "" && !binaryTool[num46].Blobs[0].result)
									{
										ErrorMsg += binaryTool[num46].TrackErrorStrA;
										if (binaryTool[num46].Blobs[0].NGRect.Width > 0 && binaryTool[num46].Blobs[0].NGRect.Height > 0)
										{
											NGRect.Add(binaryTool[num46].Blobs[0].NGRect);
											NGTool.Add(InspToolsFunciton.EM_BINARY);
											NGToolIndex.Add(num46);
										}
									}
									if (binaryTool[num46].Tracker2ResultText != "" && !binaryTool[num46].Blobs[1].result)
									{
										ErrorMsg += binaryTool[num46].TrackErrorStrB;
										if (binaryTool[num46].Blobs[1].NGRect.Width > 0 && binaryTool[num46].Blobs[1].NGRect.Height > 0)
										{
											NGRect.Add(binaryTool[num46].Blobs[1].NGRect);
											NGTool.Add(InspToolsFunciton.EM_BINARY);
											NGToolIndex.Add(num46);
										}
									}
								}
								else if (paramFile.BinaryParam[num46].BlobAnalysisSelect == 1)
								{
									if (binaryTool[num46].Tracker1ResultText != "")
									{
										ErrorMsg += binaryTool[num46].Tracker1ResultText;
										for (int num47 = 0; num47 < binaryTool[num46].BlobLine1.Count; num47++)
										{
											if (!binaryTool[num46].BlobLine1[num47].result && binaryTool[num46].BlobLine1[num47].NGRect.Width > 0 && binaryTool[num46].BlobLine1[num47].NGRect.Height > 0)
											{
												NGRect.Add(binaryTool[num46].BlobLine1[num47].NGRect);
												NGTool.Add(InspToolsFunciton.EM_BINARY);
												NGToolIndex.Add(num46);
											}
										}
									}
									if (binaryTool[num46].Tracker2ResultText != "")
									{
										ErrorMsg += binaryTool[num46].Tracker2ResultText;
										for (int num48 = 0; num48 < binaryTool[num46].BlobLine2.Count; num48++)
										{
											if (!binaryTool[num46].BlobLine2[num48].result && binaryTool[num46].BlobLine2[num48].NGRect.Width > 0 && binaryTool[num46].BlobLine2[num48].NGRect.Height > 0)
											{
												NGRect.Add(binaryTool[num46].BlobLine2[num48].NGRect);
												NGTool.Add(InspToolsFunciton.EM_BINARY);
												NGToolIndex.Add(num46);
											}
										}
									}
								}
								else if (paramFile.BinaryParam[num46].BlobAnalysisSelect == 2)
								{
									if (binaryTool[num46].Tracker1ResultText != "")
									{
										ErrorMsg += binaryTool[num46].TrackErrorStrA;
										for (int num49 = 0; num49 < binaryTool[num46].MultiBlob1.Count; num49++)
										{
											if (!binaryTool[num46].MultiBlob1[num49].result && binaryTool[num46].MultiBlob1[num49].NGRect.Width > 0 && binaryTool[num46].MultiBlob1[num49].NGRect.Height > 0)
											{
												NGRect.Add(binaryTool[num46].MultiBlob1[num49].NGRect);
												NGTool.Add(InspToolsFunciton.EM_BINARY);
												NGToolIndex.Add(num46);
											}
										}
									}
									if (binaryTool[num46].Tracker2ResultText != "")
									{
										ErrorMsg += binaryTool[num46].TrackErrorStrB;
										for (int num50 = 0; num50 < binaryTool[num46].MultiBlob2.Count; num50++)
										{
											if (!binaryTool[num46].MultiBlob2[num50].result && binaryTool[num46].MultiBlob2[num50].NGRect.Width > 0 && binaryTool[num46].MultiBlob2[num50].NGRect.Height > 0)
											{
												NGRect.Add(binaryTool[num46].MultiBlob2[num50].NGRect);
												NGTool.Add(InspToolsFunciton.EM_BINARY);
												NGToolIndex.Add(num46);
											}
										}
									}
								}
								else
								{
									if (binaryTool[num46].Tracker1ResultText != "")
									{
										ErrorMsg += binaryTool[num46].Tracker1ResultText;
									}
									if (binaryTool[num46].Tracker2ResultText != "")
									{
										ErrorMsg += binaryTool[num46].Tracker2ResultText;
									}
									for (int num51 = 0; num51 < binaryTool[num46].LineWidthRect.Count; num51++)
									{
										if (binaryTool[num46].LineWidthRect[num51].Width > 0 && binaryTool[num46].LineWidthRect[num51].Height > 0)
										{
											NGRect.Add(binaryTool[num46].LineWidthRect[num51]);
											NGTool.Add(InspToolsFunciton.EM_BINARY);
											NGToolIndex.Add(num46);
										}
									}
								}
							}
							catch (Exception ex)
							{
								CVBuffer.SaveLog("Inspection-BinaryTool-Inspect()-01-Draw-NGRect" + ex.ToString());
							}
						}
					}
					iResult.BinaryBIN[num46] = paramFile.BinaryParam[num46].ErrorBin;
					BinaryResult = false;
					ErrorCount[7]++;
					if (InLine)
					{
						iResult.BinaryErrorCount[num46]++;
					}
					if (iResult.BinaryBIN[num46] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.BinaryBIN[num46] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.BinaryBIN[num46] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.BinaryBIN[num46] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.BinaryBIN[num46] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Binary Tool inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "二值化工具檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "二值化工具检测失败，但分bin选择OK料盒! \n";
						}
					}
					if (InLine)
					{
						iResult.ToolErrorList[bCountList[num46]]++;
					}
				}
			});
			if (!BinaryResult)
			{
				for (int num25 = 0; num25 < 30; num25++)
				{
					if (iResult.BinaryResult[num25] == TOOL_RESULT.EM_NG && iResult.BinaryBIN[num25] != FLOW_BIN.EM_FLOW_RETESET && paramFile.BinaryParam[num25].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.BinaryParam[num25].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.BinaryParam[num25].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.BinaryParam[num25].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.BinaryParam[num25].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[7] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		SmoothResult = true;
		smoothTool.Tracker1ResultText = "";
		smoothTool.Tracker2ResultText = "";
		if (paramFile.SmoothToolON)
		{
			SmoothResult = true;
			for (int num26 = 0; num26 < 20; num26++)
			{
				smoothTool.Inspect(num26, ref iResult.SmoothResult[num26], SelectFaceResult, offLine);
				if (iResult.SmoothResult[num26] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 0)
							{
								ErrorMsg += "\n [Smooth Tool]\n";
							}
							else if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [平滑化工具]\n";
							}
							else
							{
								ErrorMsg += "\n [平滑化工具]\n";
							}
							firstNG = false;
						}
						if (paramFile.SmoothParam[num26].BlobAnalysisSelect == 0)
						{
							if (smoothTool.Tracker1ResultText != "" && !smoothTool.Blobs[0].result)
							{
								ErrorMsg += smoothTool.Tracker1ResultText;
								if (smoothTool.Blobs[0].NGRect.Width > 0 && smoothTool.Blobs[0].NGRect.Height > 0)
								{
									NGRect.Add(smoothTool.Blobs[0].NGRect);
									NGTool.Add(InspToolsFunciton.EM_SMOOTH);
									NGToolIndex.Add(num26);
								}
							}
							if (smoothTool.Tracker2ResultText != "" && !smoothTool.Blobs[1].result)
							{
								ErrorMsg += smoothTool.Tracker2ResultText;
								if (smoothTool.Blobs[1].NGRect.Width > 0 && smoothTool.Blobs[1].NGRect.Height > 0)
								{
									NGRect.Add(smoothTool.Blobs[1].NGRect);
									NGTool.Add(InspToolsFunciton.EM_SMOOTH);
									NGToolIndex.Add(num26);
								}
							}
						}
						else if (paramFile.SmoothParam[num26].BlobAnalysisSelect == 1)
						{
							if (smoothTool.Tracker1ResultText != "")
							{
								ErrorMsg += smoothTool.Tracker1ResultText;
								for (int num27 = 0; num27 < smoothTool.BlobLine1.Count; num27++)
								{
									if (!smoothTool.BlobLine1[num27].result && smoothTool.BlobLine1[num27].NGRect.Width > 0 && smoothTool.BlobLine1[num27].NGRect.Height > 0)
									{
										NGRect.Add(smoothTool.BlobLine1[num27].NGRect);
										NGTool.Add(InspToolsFunciton.EM_SMOOTH);
										NGToolIndex.Add(num26);
									}
								}
							}
							if (smoothTool.Tracker2ResultText != "")
							{
								ErrorMsg += smoothTool.Tracker2ResultText;
								for (int num28 = 0; num28 < smoothTool.BlobLine2.Count; num28++)
								{
									if (!smoothTool.BlobLine2[num28].result && smoothTool.BlobLine2[num28].NGRect.Width > 0 && smoothTool.BlobLine2[num28].NGRect.Height > 0)
									{
										NGRect.Add(smoothTool.BlobLine2[num28].NGRect);
										NGTool.Add(InspToolsFunciton.EM_SMOOTH);
										NGToolIndex.Add(num26);
									}
								}
							}
						}
						else if (paramFile.SmoothParam[num26].BlobAnalysisSelect == 2)
						{
							if (smoothTool.Tracker1ResultText != "")
							{
								ErrorMsg += smoothTool.Tracker1ResultText;
								for (int num29 = 0; num29 < smoothTool.MultiBlob1.Count; num29++)
								{
									if (!smoothTool.MultiBlob1[num29].result && smoothTool.MultiBlob1[num29].NGRect.Width > 0 && smoothTool.MultiBlob1[num29].NGRect.Height > 0)
									{
										NGRect.Add(smoothTool.MultiBlob1[num29].NGRect);
										NGTool.Add(InspToolsFunciton.EM_SMOOTH);
										NGToolIndex.Add(num26);
									}
								}
							}
							if (smoothTool.Tracker2ResultText != "")
							{
								ErrorMsg += smoothTool.Tracker2ResultText;
								for (int num30 = 0; num30 < smoothTool.MultiBlob2.Count; num30++)
								{
									if (!smoothTool.MultiBlob2[num30].result && smoothTool.MultiBlob2[num30].NGRect.Width > 0 && smoothTool.MultiBlob2[num30].NGRect.Height > 0)
									{
										NGRect.Add(smoothTool.MultiBlob2[num30].NGRect);
										NGTool.Add(InspToolsFunciton.EM_SMOOTH);
										NGToolIndex.Add(num26);
									}
								}
							}
						}
					}
					iResult.SmoothBIN[num26] = paramFile.SmoothParam[num26].ErrorBin;
					SmoothResult = false;
					ErrorCount[8]++;
					if (InLine)
					{
						iResult.SmoothErrorCount[num26]++;
					}
					if (paramFile.SmoothParam[num26].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.SmoothParam[num26].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.SmoothParam[num26].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.SmoothParam[num26].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.SmoothParam[num26].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.SmoothBIN[num26] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.SmoothBIN[num26] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.SmoothBIN[num26] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.SmoothBIN[num26] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.SmoothBIN[num26] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Smooth Binary inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "平滑化工具檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "平滑化工具检测失败，但分bin选择OK料盒! \n";
						}
					}
					if (InLine)
					{
						iResult.ToolErrorList[num]++;
						num++;
					}
				}
				else if (iResult.SmoothResult[num26] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[8] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		BlockAvgResult = true;
		if (paramFile.BlockAvgToolON)
		{
			int[] bCountList2 = new int[20];
			int num31 = num;
			for (int num32 = 0; num32 < 20; num32++)
			{
				if (paramFile.BlockAvgParam[num32].TrackerON)
				{
					bCountList2[num32] = num31;
					num31++;
				}
				else
				{
					bCountList2[num32] = 0;
				}
			}
			num = num31;
			BlockAvgResult = true;
			Parallel.For(0, 20, [MethodImpl(MethodImplOptions.NoInlining)] (int num46, ParallelLoopState loopState) =>
			{
				blockavgTool[num46].Tracker1ResultText = "";
				blockavgTool[num46].Tracker2ResultText = "";
				blockavgTool[num46].Inspect(num46, ref iResult.BlockAvgResult[num46], SelectFaceResult, offLine);
				if (iResult.BlockAvgResult[num46] == TOOL_RESULT.EM_NG)
				{
					iResult.BlockAvgBIN[num46] = paramFile.BlockAvgParam[num46].ErrorBin;
					BlockAvgResult = false;
					ErrorCount[9]++;
					if (InLine)
					{
						iResult.BlockAvgErrorCount[num46]++;
					}
					if (iResult.BlockAvgBIN[num46] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.BlockAvgBIN[num46] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.BlockAvgBIN[num46] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.BlockAvgBIN[num46] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.BlockAvgBIN[num46] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Block Bianry inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "區域二值化工具檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "区域二值化工具检测失败，但分bin选择OK料盒! \n";
						}
					}
					if (InLine)
					{
						iResult.ToolErrorList[bCountList2[num46]]++;
					}
				}
			});
			if (!BlockAvgResult)
			{
				for (int num33 = 0; num33 < 20; num33++)
				{
					if (iResult.BlockAvgResult[num33] == TOOL_RESULT.EM_NG && offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 0)
							{
								ErrorMsg += "\n [Block Binary Tool]\n";
							}
							else if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [區域二值化工具]\n";
							}
							else
							{
								ErrorMsg += "\n [区域二值化工具]\n";
							}
							firstNG = false;
						}
						if (paramFile.BlockAvgParam[num33].BlobAnalysisSelect == 0)
						{
							if (blockavgTool[num33].Tracker1ResultText != "" && !blockavgTool[num33].Blobs[0].result)
							{
								ErrorMsg += blockavgTool[num33].Tracker1ResultText;
								if (blockavgTool[num33].Blobs[0].NGRect.Width > 0 && blockavgTool[num33].Blobs[0].NGRect.Height > 0)
								{
									NGRect.Add(blockavgTool[num33].Blobs[0].NGRect);
									NGTool.Add(InspToolsFunciton.EM_BLOCKAVG);
									NGToolIndex.Add(num33);
								}
							}
							if (blockavgTool[num33].Tracker2ResultText != "" && !blockavgTool[num33].Blobs[1].result)
							{
								ErrorMsg += blockavgTool[num33].Tracker2ResultText;
								if (blockavgTool[num33].Blobs[1].NGRect.Width > 0 && blockavgTool[num33].Blobs[1].NGRect.Height > 0)
								{
									NGRect.Add(blockavgTool[num33].Blobs[1].NGRect);
									NGTool.Add(InspToolsFunciton.EM_BLOCKAVG);
									NGToolIndex.Add(num33);
								}
							}
						}
						else if (paramFile.BlockAvgParam[num33].BlobAnalysisSelect == 1)
						{
							if (blockavgTool[num33].Tracker1ResultText != "")
							{
								ErrorMsg += blockavgTool[num33].Tracker1ResultText;
								for (int num34 = 0; num34 < blockavgTool[num33].BlobLine1.Count; num34++)
								{
									if (!blockavgTool[num33].BlobLine1[num34].result && blockavgTool[num33].BlobLine1[num34].NGRect.Width > 0 && blockavgTool[num33].BlobLine1[num34].NGRect.Height > 0)
									{
										NGRect.Add(blockavgTool[num33].BlobLine1[num34].NGRect);
										NGTool.Add(InspToolsFunciton.EM_BLOCKAVG);
										NGToolIndex.Add(num33);
									}
								}
							}
							if (blockavgTool[num33].Tracker2ResultText != "")
							{
								ErrorMsg += blockavgTool[num33].Tracker2ResultText;
								for (int num35 = 0; num35 < blockavgTool[num33].BlobLine2.Count; num35++)
								{
									if (!blockavgTool[num33].BlobLine2[num35].result && blockavgTool[num33].BlobLine2[num35].NGRect.Width > 0 && blockavgTool[num33].BlobLine2[num35].NGRect.Height > 0)
									{
										NGRect.Add(blockavgTool[num33].BlobLine2[num35].NGRect);
										NGTool.Add(InspToolsFunciton.EM_BLOCKAVG);
										NGToolIndex.Add(num33);
									}
								}
							}
						}
						else if (paramFile.BlockAvgParam[num33].BlobAnalysisSelect == 2)
						{
							if (blockavgTool[num33].Tracker1ResultText != "")
							{
								ErrorMsg += blockavgTool[num33].Tracker1ResultText;
								for (int num36 = 0; num36 < blockavgTool[num33].MultiBlob1.Count; num36++)
								{
									if (!blockavgTool[num33].MultiBlob1[num36].result && blockavgTool[num33].MultiBlob1[num36].NGRect.Width > 0 && blockavgTool[num33].MultiBlob1[num36].NGRect.Height > 0)
									{
										NGRect.Add(blockavgTool[num33].MultiBlob1[num36].NGRect);
										NGTool.Add(InspToolsFunciton.EM_BLOCKAVG);
										NGToolIndex.Add(num33);
									}
								}
							}
							if (blockavgTool[num33].Tracker2ResultText != "")
							{
								ErrorMsg += blockavgTool[num33].Tracker2ResultText;
								for (int num37 = 0; num37 < blockavgTool[num33].MultiBlob2.Count; num37++)
								{
									if (!blockavgTool[num33].MultiBlob2[num37].result && blockavgTool[num33].MultiBlob2[num37].NGRect.Width > 0 && blockavgTool[num33].MultiBlob2[num37].NGRect.Height > 0)
									{
										NGRect.Add(blockavgTool[num33].MultiBlob2[num37].NGRect);
										NGTool.Add(InspToolsFunciton.EM_BLOCKAVG);
										NGToolIndex.Add(num33);
									}
								}
							}
						}
					}
					if (iResult.BlockAvgResult[num33] == TOOL_RESULT.EM_NG && iResult.BlockAvgBIN[num33] != FLOW_BIN.EM_FLOW_RETESET && paramFile.BlockAvgParam[num33].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.BlockAvgParam[num33].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.BlockAvgParam[num33].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.BlockAvgParam[num33].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.BlockAvgParam[num33].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[9] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		DifferenceResult = true;
		differenceTool.Tracker1ResultText = "";
		differenceTool.Tracker2ResultText = "";
		if (paramFile.DifferenceToolON)
		{
			DifferenceResult = true;
			for (int num38 = 0; num38 < 20; num38++)
			{
				differenceTool.Inspect(num38, ref iResult.DifferenceResult[num38], SelectFaceResult);
				if (iResult.DifferenceResult[num38] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 0)
							{
								ErrorMsg += "\n [Difference Binary Tool]\n";
							}
							else if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [差異二值化工具]\n";
							}
							else
							{
								ErrorMsg += "\n [差异二值化工具]\n";
							}
							firstNG = false;
						}
						if (paramFile.DifferenceParam[num38].BlobAnalysisSelect == 0)
						{
							if (differenceTool.Tracker1ResultText != "" && !differenceTool.Blobs[0].result)
							{
								ErrorMsg += differenceTool.Tracker1ResultText;
								if (differenceTool.Blobs[0].NGRect.Width > 0 && differenceTool.Blobs[0].NGRect.Height > 0)
								{
									NGRect.Add(differenceTool.Blobs[0].NGRect);
									NGTool.Add(InspToolsFunciton.EM_DIFFERENCE);
									NGToolIndex.Add(num38);
								}
							}
							if (differenceTool.Tracker2ResultText != "" && !differenceTool.Blobs[1].result)
							{
								ErrorMsg += differenceTool.Tracker2ResultText;
								if (differenceTool.Blobs[1].NGRect.Width > 0 && differenceTool.Blobs[1].NGRect.Height > 0)
								{
									NGRect.Add(differenceTool.Blobs[1].NGRect);
									NGTool.Add(InspToolsFunciton.EM_DIFFERENCE);
									NGToolIndex.Add(num38);
								}
							}
						}
						else if (paramFile.DifferenceParam[num38].BlobAnalysisSelect == 1)
						{
							if (differenceTool.Tracker1ResultText != "")
							{
								ErrorMsg += differenceTool.Tracker1ResultText;
								for (int num39 = 0; num39 < differenceTool.BlobLine1.Count; num39++)
								{
									if (!differenceTool.BlobLine1[num39].result && differenceTool.BlobLine1[num39].NGRect.Width > 0 && differenceTool.BlobLine1[num39].NGRect.Height > 0)
									{
										NGRect.Add(differenceTool.BlobLine1[num39].NGRect);
										NGTool.Add(InspToolsFunciton.EM_DIFFERENCE);
										NGToolIndex.Add(num38);
									}
								}
							}
							if (differenceTool.Tracker2ResultText != "")
							{
								ErrorMsg += differenceTool.Tracker2ResultText;
								for (int num40 = 0; num40 < differenceTool.BlobLine2.Count; num40++)
								{
									if (!differenceTool.BlobLine2[num40].result && differenceTool.BlobLine2[num40].NGRect.Width > 0 && differenceTool.BlobLine2[num40].NGRect.Height > 0)
									{
										NGRect.Add(differenceTool.BlobLine2[num40].NGRect);
										NGTool.Add(InspToolsFunciton.EM_DIFFERENCE);
										NGToolIndex.Add(num38);
									}
								}
							}
						}
						else if (paramFile.DifferenceParam[num38].BlobAnalysisSelect == 2)
						{
							if (differenceTool.Tracker1ResultText != "")
							{
								ErrorMsg += differenceTool.Tracker1ResultText;
								for (int num41 = 0; num41 < differenceTool.MultiBlob1.Count; num41++)
								{
									if (!differenceTool.MultiBlob1[num41].result && differenceTool.MultiBlob1[num41].NGRect.Width > 0 && differenceTool.MultiBlob1[num41].NGRect.Height > 0)
									{
										NGRect.Add(differenceTool.MultiBlob1[num41].NGRect);
										NGTool.Add(InspToolsFunciton.EM_DIFFERENCE);
										NGToolIndex.Add(num38);
									}
								}
							}
							if (differenceTool.Tracker2ResultText != "")
							{
								ErrorMsg += differenceTool.Tracker2ResultText;
								for (int num42 = 0; num42 < differenceTool.MultiBlob2.Count; num42++)
								{
									if (!differenceTool.MultiBlob2[num42].result && differenceTool.MultiBlob2[num42].NGRect.Width > 0 && differenceTool.MultiBlob2[num42].NGRect.Height > 0)
									{
										NGRect.Add(differenceTool.MultiBlob2[num42].NGRect);
										NGTool.Add(InspToolsFunciton.EM_DIFFERENCE);
										NGToolIndex.Add(num38);
									}
								}
							}
						}
					}
					iResult.DifferenceBIN[num38] = paramFile.DifferenceParam[num38].ErrorBin;
					DifferenceResult = false;
					ErrorCount[10]++;
					if (InLine)
					{
						iResult.DifferenceErrorCount[num38]++;
					}
					if (paramFile.DifferenceParam[num38].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.DifferenceParam[num38].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.DifferenceParam[num38].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.DifferenceParam[num38].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.DifferenceParam[num38].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.DifferenceBIN[num38] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.DifferenceBIN[num38] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.DifferenceBIN[num38] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.DifferenceBIN[num38] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.DifferenceBIN[num38] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Difference Bianry inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "差異二值化工具檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "差异二值化工具检测失败，但分bin选择OK料盒! \n";
						}
					}
					if (InLine)
					{
						iResult.ToolErrorList[num]++;
						num++;
					}
				}
				else if (iResult.DifferenceResult[num38] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[10] = TimeStop();
			TimeStart();
		}
		firstNG = true;
		LineAnalysisResult = true;
		lineanalysisTool.Tracker1ResultText = "";
		if (paramFile.LineAnalysisToolON)
		{
			LineAnalysisResult = true;
			for (int num43 = 0; num43 < 20; num43++)
			{
				lineanalysisTool.Inspect(num43, ref iResult.LineAnalysisResult[num43], SelectFaceResult, offLine);
				if (iResult.LineAnalysisResult[num43] == TOOL_RESULT.EM_NG)
				{
					if (offLine)
					{
						if (firstNG)
						{
							if (buffers.LanguageType == 0)
							{
								ErrorMsg += "\n [Line Analysis Tool]\n";
							}
							else if (buffers.LanguageType == 1)
							{
								ErrorMsg += "\n [線段分析工具]\n";
							}
							else
							{
								ErrorMsg += "\n [线段分析工具]\n";
							}
							firstNG = false;
						}
						ErrorMsg += lineanalysisTool.ErrorStr;
						if (lineanalysisTool.InspeRect.Width > 0 && lineanalysisTool.InspeRect.Height > 0)
						{
							NGRect.Add(lineanalysisTool.InspeRect);
							NGTool.Add(InspToolsFunciton.EM_LINEANALYSIS);
							NGToolIndex.Add(num43);
						}
					}
					iResult.LineAnalysisBIN[num43] = paramFile.LineAnalysisParam[num43].ErrorBin;
					LineAnalysisResult = false;
					ErrorCount[11]++;
					if (InLine)
					{
						iResult.LineAnalysisErrorCount[num43]++;
					}
					if (paramFile.LineAnalysisParam[num43].ParetoIndex > 0 && ImageID != -1)
					{
						if (EnableMultiParetoMode)
						{
							ParetoChartTable[paramFile.LineAnalysisParam[num43].ParetoIndex]++;
						}
						if (ParetoIndex == 0)
						{
							int paretoIndex = (ParetoIndex = paramFile.LineAnalysisParam[num43].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
						else if (paramFile.LineAnalysisParam[num43].ParetoIndex < ParetoIndex)
						{
							int paretoIndex = (ParetoIndex = paramFile.LineAnalysisParam[num43].ParetoIndex);
							result.ParetoIndex = paretoIndex;
						}
					}
					if (iResult.LineAnalysisBIN[num43] == FLOW_BIN.EM_FLOW_RETESET)
					{
						finalRetest = true;
					}
					else if (iResult.LineAnalysisBIN[num43] == FLOW_BIN.EM_FLOW_NG4)
					{
						finalNG4 = true;
					}
					else if (iResult.LineAnalysisBIN[num43] == FLOW_BIN.EM_FLOW_NG3)
					{
						finalNG3 = true;
					}
					else if (iResult.LineAnalysisBIN[num43] == FLOW_BIN.EM_FLOW_NG2)
					{
						finalNG2 = true;
					}
					else if (iResult.LineAnalysisBIN[num43] == FLOW_BIN.EM_FLOW_NG1)
					{
						finalNG1 = true;
					}
					else if (offLine)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "Line Analysis inspect Fail, but default error bin is set to OK Bin! \n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "線段分析工具檢測失敗，但分bin選擇OK料盒! \n";
						}
						else
						{
							ErrorMsg += "线段分析工具检测失败，但分bin选择OK料盒! \n";
						}
					}
					if (InLine)
					{
						iResult.ToolErrorList[num]++;
						num++;
					}
				}
				else if (iResult.LineAnalysisResult[num43] == TOOL_RESULT.EM_OK)
				{
					num++;
				}
			}
		}
		if (offLine)
		{
			InspTimePerTool[11] = TimeStop();
			TimeStart();
		}
		AIPredictResult = true;
		if (aiPredictTools != null && paramFile.AIPredictToolON && AIDLLExist && maskTool.AIMat.Width > 0 && maskTool.AIMat.Height > 0)
		{
			TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
			if (aiPredictTools.Inspect(maskTool.AIMat, ref iresult, ref iResult.AIPredictBIN, offLine, SelectFaceResult))
			{
				if (offLine && (aiPredictTools.PredictModelShowA || aiPredictTools.PredictModelShowB))
				{
					buffers.SetAiEnhanceMat(aiPredictTools.AIMatR, aiPredictTools.AIMatG, aiPredictTools.AIMatB, maskTool.AIRect);
					buffers.SetAiEnhanceMatColor(aiPredictTools.DestMatColor, maskTool.AIRect);
				}
				if (iResult.AIPredictBIN == FLOW_BIN.EM_FLOW_RETESET)
				{
					finalRetest = true;
				}
				else if (iResult.AIPredictBIN == FLOW_BIN.EM_FLOW_NG4)
				{
					finalNG4 = true;
				}
				else if (iResult.AIPredictBIN == FLOW_BIN.EM_FLOW_NG3)
				{
					finalNG3 = true;
				}
				else if (iResult.AIPredictBIN == FLOW_BIN.EM_FLOW_NG2)
				{
					finalNG2 = true;
				}
				else if (iResult.AIPredictBIN == FLOW_BIN.EM_FLOW_NG1)
				{
					finalNG1 = true;
				}
				if (iresult != TOOL_RESULT.EM_OK)
				{
					AIPredictResult = false;
					ErrorMsg += aiPredictTools.ResultText;
					ErrorCount[13]++;
					if (ParetoIndex == 0)
					{
						int paretoIndex = (ParetoIndex = paramFile.AIPredictParam.ParetoIndex_A);
						result.ParetoIndex = paretoIndex;
					}
					else if (paramFile.AIPredictParam.ParetoIndex_A < ParetoIndex)
					{
						int paretoIndex = (ParetoIndex = paramFile.AIPredictParam.ParetoIndex_A);
						result.ParetoIndex = paretoIndex;
					}
					if (InLine)
					{
						iResult.ToolErrorList[num]++;
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			else
			{
				ErrorMsg += aiPredictTools.ResultText;
				if (buffers.LanguageType == 0)
				{
					ErrorMsg += "AI Predict Tools Error! \n";
				}
				else if (buffers.LanguageType == 1)
				{
					ErrorMsg += "AI檢測工具異常! \n";
				}
				else
				{
					ErrorMsg += "AI检测工具异常! \n";
				}
				finalRetest = true;
			}
		}
		if (offLine && aiPredictTools != null)
		{
			InspTimePerTool[13] = aiPredictTools.UsageTime;
		}
		firstNG = true;
		IRToolInspResult = true;
		irTool.ErrorStr = "";
		if (paramFile.IRToolON)
		{
			tempcount++;
			if (tempcount > 100000)
			{
				tempcount = 0;
			}
			irTool.Inspect(ref iResult.IRToolResult, SelectFaceResult, offLine);
			if (iResult.IRToolResult == TOOL_RESULT.EM_NG)
			{
				if (offLine)
				{
					if (firstNG)
					{
						if (buffers.LanguageType == 0)
						{
							ErrorMsg += "\n [IRTool]\n";
						}
						else if (buffers.LanguageType == 1)
						{
							ErrorMsg += "\n [色碼工具]\n";
						}
						else
						{
							ErrorMsg += "\n [色码工具]\n";
						}
						firstNG = false;
					}
					ErrorMsg += irTool.ErrorStr;
					for (int num44 = 0; num44 < 4; num44++)
					{
						if (irTool.NGRect[num44].Width > 0 && irTool.NGRect[num44].Height > 0)
						{
							NGRect.Add(irTool.NGRect[num44]);
							NGTool.Add(InspToolsFunciton.EM_FONT);
							NGToolIndex.Add(0);
						}
					}
				}
				iResult.IRToolBIN = paramFile.IRToolParam.ErrorBin;
				IRToolInspResult = false;
				ErrorCount[16]++;
				if (InLine)
				{
					iResult.IRToolErrorCount++;
				}
				if (paramFile.IRToolParam.ParetoIndex > 0 && ImageID != -1)
				{
					if (EnableMultiParetoMode)
					{
						ParetoChartTable[paramFile.IRToolParam.ParetoIndex]++;
					}
					if (ParetoIndex == 0)
					{
						int paretoIndex = (ParetoIndex = paramFile.IRToolParam.ParetoIndex);
						result.ParetoIndex = paretoIndex;
					}
					else if (paramFile.IRToolParam.ParetoIndex < ParetoIndex)
					{
						int paretoIndex = (ParetoIndex = paramFile.IRToolParam.ParetoIndex);
						result.ParetoIndex = paretoIndex;
					}
				}
				if (iResult.IRToolBIN == FLOW_BIN.EM_FLOW_RETESET)
				{
					finalRetest = true;
				}
				else if (iResult.IRToolBIN == FLOW_BIN.EM_FLOW_NG4)
				{
					finalNG4 = true;
				}
				else if (iResult.IRToolBIN == FLOW_BIN.EM_FLOW_NG3)
				{
					finalNG3 = true;
				}
				else if (iResult.IRToolBIN == FLOW_BIN.EM_FLOW_NG2)
				{
					finalNG2 = true;
				}
				else if (iResult.IRToolBIN == FLOW_BIN.EM_FLOW_NG1)
				{
					finalNG1 = true;
				}
				else if (offLine)
				{
					if (buffers.LanguageType == 0)
					{
						ErrorMsg += "IR Tool inspect Fail, but default error bin is set to OK Bin! \n";
					}
					else if (buffers.LanguageType == 1)
					{
						ErrorMsg += "色碼工具檢測失敗，但分bin選擇OK料盒! \n";
					}
					else
					{
						ErrorMsg += "色码工具检测失败，但分bin选择OK料盒! \n";
					}
				}
				if (InLine)
				{
					iResult.ToolErrorList[num]++;
					num++;
				}
			}
			else if (iResult.IRToolResult == TOOL_RESULT.EM_OK)
			{
				num++;
			}
		}
		if (offLine)
		{
			InspTimePerTool[16] = TimeStop();
		}
		if (offLine)
		{
			TimeStop();
			LocateTime = 0.0;
			for (int num45 = 0; num45 < 20; num45++)
			{
				LocateTime += InspTimePerTool[num45];
			}
			if (timecount >= 500)
			{
				timecount = 0;
			}
			InspTime[timecount] = LocateTime;
			timecount++;
		}
		if (SizeStatics.Count >= 1100)
		{
			SizeStatics.RemoveAt(0);
		}
		if (sIZESTATIC.ID >= 0 && !finalRetest && !UpdateLast100)
		{
			SizeStatics.Add(sIZESTATIC);
		}
		if (!finalRetest)
		{
			if (ParetoIndex > 0 && !EnableMultiParetoMode)
			{
				ParetoChartTable[ParetoIndex]++;
				result.ParetoIndex = ParetoIndex;
			}
			if ((finalNG1 | finalNG2 | finalNG3 | finalNG4) && ImageID != -1 && ParetoIndex > 0)
			{
				pData.ID = ImageID;
				pData.ParetoID = ParetoIndex;
				if (ParetoData.Count >= 1000)
				{
					ParetoData.RemoveAt(ParetoData.Count - 1);
				}
				ParetoData.Add(pData);
			}
		}
		if (finalRetest)
		{
			result.bin = FLOW_BIN.EM_FLOW_RETESET;
			result.ParetoIndex = 0;
			CountInspResult(result, offLine);
			return result;
		}
		if (!ReverseBinPriority)
		{
			if (finalNG4)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG4;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (finalNG3)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG3;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (finalNG2)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG2;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			if (finalNG1)
			{
				result.bin = FLOW_BIN.EM_FLOW_NG1;
				result.ParetoIndex = ParetoIndex;
				CountInspResult(result, offLine);
				return result;
			}
			result.bin = FLOW_BIN.EM_FLOW_OK;
			result.ParetoIndex = 0;
			CountInspResult(result, offLine);
			return result;
		}
		if (finalNG1)
		{
			result.bin = FLOW_BIN.EM_FLOW_NG1;
			result.ParetoIndex = ParetoIndex;
			CountInspResult(result, offLine);
			return result;
		}
		if (finalNG2)
		{
			result.bin = FLOW_BIN.EM_FLOW_NG2;
			result.ParetoIndex = ParetoIndex;
			CountInspResult(result, offLine);
			return result;
		}
		if (finalNG3)
		{
			result.bin = FLOW_BIN.EM_FLOW_NG3;
			result.ParetoIndex = ParetoIndex;
			CountInspResult(result, offLine);
			return result;
		}
		if (finalNG4)
		{
			result.bin = FLOW_BIN.EM_FLOW_NG4;
			result.ParetoIndex = ParetoIndex;
			CountInspResult(result, offLine);
			return result;
		}
		result.bin = FLOW_BIN.EM_FLOW_OK;
		result.ParetoIndex = 0;
		CountInspResult(result, offLine);
		return result;
	}

	private void CountInspResult(INSPRESULT result, bool offline)
	{
		if (!offline)
		{
			InlineResult.Add(result);
			if (InlineResult.Count > 500)
			{
				InlineResult.RemoveAt(0);
			}
		}
		else
		{
			OfflineResult.Add(result);
			if (OfflineResult.Count > 5000)
			{
				OfflineResult.RemoveAt(0);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<SIZESTATIC> GetLast100Data(ref double LocateAngle)
	{
		UpdateLast100 = true;
		Last100Data.Clear();
		LocateAngle = paramFile.LocateParam.LocateAngleLimit;
		try
		{
			for (int i = 0; i < 100 && i < SizeStatics.Count; i++)
			{
				Last100Data.Add(SizeStatics[i]);
			}
			UpdateLast100 = false;
			return Last100Data;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Inspection-GetLast100Data" + ex.ToString());
			UpdateLast100 = false;
			return Last100Data;
		}
	}

	public int[] GetToolErrorList(ref int count)
	{
		count = iResult.ToolUsedCount;
		return iResult.ToolErrorList;
	}

	public double[] GetToolErrorRatioAlarmList(ref int count)
	{
		GetToolNameList();
		count = iResult.ToolUsedCount;
		return iResult.ToolErrorRatioAlarmList;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private double GetToolErrorRatio(int station, int toolid, int trackid)
	{
		double result = 0.0;
		try
		{
			for (int i = 0; i < paramFile.ThroughputFile.IsItems.Count; i++)
			{
				if (station == paramFile.ThroughputFile.IsItems[i].Station && toolid == paramFile.ThroughputFile.IsItems[i].InspToolID && trackid == paramFile.ThroughputFile.IsItems[i].TrackID)
				{
					return paramFile.ThroughputFile.IsItems[i].AlarmRatio;
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Inspection-GetToolErrorRatio()-00-" + ex.ToString());
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<string> GetToolNameList()
	{
		iResult.ToolNameList.Clear();
		iResult.ToolUsedCount = 0;
		string text = "";
		try
		{
			if (paramFile.LocateToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[定位] " : ((buffers.LanguageType == 2) ? "[定位] " : ((buffers.LanguageType != 3) ? "[Locate] " : "[位置決め] ")));
				iResult.ToolNameList.Add(text + paramFile.LocateParam.TrackerName);
				iResult.ToolUsedCount++;
				text = ((buffers.LanguageType == 1) ? "[定位尺寸] " : ((buffers.LanguageType == 2) ? "[定位尺寸] " : ((buffers.LanguageType != 3) ? "[Locate Size] " : "[寸法位置決め] ")));
				iResult.ToolNameList.Add(text + paramFile.LocateParam.TrackerName);
				iResult.ToolUsedCount++;
			}
			if (paramFile.SelectFaceON)
			{
				if (buffers.LanguageType == 1)
				{
					text = "[選面] ";
					iResult.ToolNameList.Add(text + "選面錯誤");
				}
				else if (buffers.LanguageType == 2)
				{
					text = "[选面] ";
					iResult.ToolNameList.Add(text + "选面错误");
				}
				else if (buffers.LanguageType == 3)
				{
					text = "[向き合わせ] ";
					iResult.ToolNameList.Add(text + "向き合わせ不具合");
				}
				else
				{
					text = "[Select Face] ";
					iResult.ToolNameList.Add(text + "Select Face Error");
				}
				iResult.ToolUsedCount++;
			}
			if (paramFile.AIProphetToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[AI先知] " : ((buffers.LanguageType == 2) ? "[AI先知] " : ((buffers.LanguageType != 3) ? "[AI Prophet Tool] " : "[AI 預言者] ")));
				iResult.ToolNameList.Add(text);
				iResult.ToolUsedCount++;
			}
			if (paramFile.AdvLocateToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[進階定位] " : ((buffers.LanguageType == 2) ? "[进阶定位] " : ((buffers.LanguageType != 3) ? "[Adv. Locate] " : "[進階位置決め] ")));
				for (int i = 2; i < 20; i++)
				{
					if (paramFile.VSLocateParam[i].TrackerON)
					{
						iResult.ToolNameList.Add(text + paramFile.VSLocateParam[i].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
				for (int j = 2; j < 20; j++)
				{
					if (paramFile.HSLocateParam[j].TrackerON)
					{
						iResult.ToolNameList.Add(text + paramFile.HSLocateParam[j].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
				for (int k = 1; k < 20; k++)
				{
					if (paramFile.AdvLocateParam[k].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 0, k);
						iResult.ToolNameList.Add(text + paramFile.AdvLocateParam[k].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
				for (int l = 0; l < 40; l++)
				{
					if (paramFile.AdvLocateMParam[l].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 1, l);
						iResult.ToolNameList.Add(text + paramFile.AdvLocateMParam[l].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.MeasureToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[尺寸量測] " : ((buffers.LanguageType == 2) ? "[尺寸量测] " : ((buffers.LanguageType != 3) ? "[Measure Tool] " : "[サイズ量測] ")));
				for (int m = 0; m < 20; m++)
				{
					if (paramFile.MeaWidthParam[m].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 2, m);
						iResult.ToolNameList.Add(text + paramFile.MeaWidthParam[m].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
				for (int n = 0; n < 20; n++)
				{
					if (paramFile.MeaHeightParam[n].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 3, n);
						iResult.ToolNameList.Add(text + paramFile.MeaHeightParam[n].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.MatchToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[圖形比對] " : ((buffers.LanguageType == 2) ? "[图形比对] " : ((buffers.LanguageType != 3) ? "[Match Tool] " : "[グラフ比較] ")));
				for (int num = 0; num < 20; num++)
				{
					if (paramFile.MatchParam[num].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 9, num);
						iResult.ToolNameList.Add(text + paramFile.MatchParam[num].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.FontToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[文字比對] " : ((buffers.LanguageType == 2) ? "[文字比对] " : ((buffers.LanguageType != 3) ? "[Font Tool] " : "[文字比較] ")));
				iResult.ToolNameList.Add(text);
				iResult.ToolUsedCount++;
			}
			if (paramFile.BinaryToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[二值化] " : ((buffers.LanguageType == 2) ? "[二值化] " : ((buffers.LanguageType != 3) ? "[Binary Tool] " : "[二値化] ")));
				for (int num2 = 0; num2 < 30; num2++)
				{
					if (paramFile.BinaryParam[num2].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 4, num2);
						iResult.ToolNameList.Add(text + paramFile.BinaryParam[num2].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.SmoothToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[平滑二值化] " : ((buffers.LanguageType == 2) ? "[平滑二值化] " : ((buffers.LanguageType != 3) ? "[Smooth Tool] " : "[平滑二値化] ")));
				for (int num3 = 0; num3 < 20; num3++)
				{
					if (paramFile.SmoothParam[num3].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 5, num3);
						iResult.ToolNameList.Add(text + paramFile.SmoothParam[num3].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.BlockAvgToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[區域二值化] " : ((buffers.LanguageType == 2) ? "[区域二值化] " : ((buffers.LanguageType != 3) ? "[BlockAvg Tool] " : "[ブロック二値化] ")));
				for (int num4 = 0; num4 < 20; num4++)
				{
					if (paramFile.BlockAvgParam[num4].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 6, num4);
						iResult.ToolNameList.Add(text + paramFile.BlockAvgParam[num4].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.DifferenceToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[差異二值化] " : ((buffers.LanguageType == 2) ? "[差异二值化] " : ((buffers.LanguageType != 3) ? "[Difference Tool] " : "[差分二値化] ")));
				for (int num5 = 0; num5 < 20; num5++)
				{
					if (paramFile.DifferenceParam[num5].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 7, num5);
						iResult.ToolNameList.Add(text + paramFile.DifferenceParam[num5].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.LineAnalysisToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[線段分析] " : ((buffers.LanguageType == 2) ? "[线段分析] " : ((buffers.LanguageType != 3) ? "[LineAnalysis Tool] " : "[線分析] ")));
				for (int num6 = 0; num6 < 20; num6++)
				{
					if (paramFile.LineAnalysisParam[num6].TrackerON)
					{
						iResult.ToolErrorRatioAlarmList[iResult.ToolUsedCount] = GetToolErrorRatio(StationIndex, 8, num6);
						iResult.ToolNameList.Add(text + paramFile.LineAnalysisParam[num6].TrackerName);
						iResult.ToolUsedCount++;
					}
				}
			}
			if (paramFile.AIPredictToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[AI檢測] " : ((buffers.LanguageType == 2) ? "[AI检测] " : ((buffers.LanguageType != 3) ? "[AIPredict Tool] " : "[AI検出] ")));
				iResult.ToolNameList.Add(text);
				iResult.ToolUsedCount++;
			}
			if (paramFile.IRToolON)
			{
				text = ((buffers.LanguageType == 1) ? "[色碼工具] " : ((buffers.LanguageType == 2) ? "[色码工具] " : ((buffers.LanguageType != 3) ? "[IR Tool] " : "[色碼ツール] ")));
				iResult.ToolNameList.Add(text);
				iResult.ToolUsedCount++;
			}
			return iResult.ToolNameList;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Inspection-GetToolNameList()-00-" + ex.ToString());
			return iResult.ToolNameList;
		}
	}

	public void ResetParetoChart()
	{
		for (int i = 0; i < 30; i++)
		{
			ParetoChartTable[i] = 0;
		}
	}

	public void ResetResult()
	{
		iResult.ResetResult();
		ResetParetoChart();
		SizeStatics.Clear();
		ParetoData.Clear();
		InlineResult.Clear();
		OfflineResult.Clear();
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

	public Mat GetAIMat()
	{
		return maskTool.AIMat;
	}

	public void SetStationIndex(int index)
	{
		StationIndex = index;
	}

	public void SetThreadID(int threadid)
	{
		ThreadID = threadid;
	}

	public void SetSaveAIImage(bool status)
	{
		SaveAIImage = status;
	}

	public int[] GetParetoChartTable()
	{
		return ParetoChartTable;
	}

	public string[] GetParetoChartName(ref int paretoItemCount)
	{
		paretoItemCount = ParetoItemCount;
		return ParetoCharName;
	}

	public void SetParetoChartName(string[] paretoCharName)
	{
		for (int i = 1; i < 30; i++)
		{
			ParetoCharName[i] = paretoCharName[i];
		}
	}

	public bool GetAIStatus()
	{
		if (!paramFile.AIEnhanceToolON)
		{
			return paramFile.AIPredictToolON;
		}
		return true;
	}

	public void SetReverseBin(bool enable)
	{
		ReverseBinPriority = enable;
	}

	public void SetLightMonitorON(bool enabled)
	{
		EnableLightMonitor = enabled;
		if (enabled)
		{
			lightmonitorTool.ClearLightMonitorThd();
		}
	}

	public bool GetSetLightMonitorResult(ref string ResultStr)
	{
		return lightmonitorTool.LightMonitorCheck(ref ResultStr, StationIndex);
	}

	public void SetLightCorrectionON(bool enabled)
	{
		EnableLightCorrection = enabled;
		if (enabled)
		{
			lightmonitorTool.ClearLightMonitorThd();
		}
	}

	public void SetUpdateLightMonitorBaseValueON(bool enabled)
	{
		EnableSetupLightMonitorBaseValue = enabled;
		if (enabled)
		{
			lightmonitorTool.ClearLightMonitorThd();
		}
		else
		{
			lightmonitorTool.UpdateLightMonitorBaseValue();
		}
	}

	public bool GetCorrectionVoltageShfitValue(ref double ShiftB, ref double ShiftG, ref double ShiftR, ref string ResultStr)
	{
		return lightmonitorTool.GetVoltageShfitValue(ref ShiftB, ref ShiftG, ref ShiftR, ref ResultStr, StationIndex);
	}

	public bool GetCorrectionShiftValue(ref LightCorrectonDiffValue DValue, ref string result, int StationIndex)
	{
		return lightmonitorTool.GetCorrectionShiftValue(ref DValue, ref result, StationIndex);
	}

	public bool GetSetLightCorrectionResult(ref int GainShift, ref string ResultStr)
	{
		return lightmonitorTool.GetGainShfitValue(ref GainShift, ref ResultStr, StationIndex);
	}

	public void GetLightMonitorData(ref List<int> Rdata, ref List<int> Gdata, ref List<int> Bdata)
	{
		lightmonitorTool.GetLightMonitorData(ref Rdata, ref Gdata, ref Bdata);
	}

	public bool GetLightMonitorEnable()
	{
		return paramFile.LightMonitorParam.LightMonitor_ON;
	}

	public bool GetLightCorrectionEnable()
	{
		return paramFile.LightMonitorParam.LightCorrection_ON;
	}

	public bool StartSoftCorrectionAcquire(ref string result, int StationIndex)
	{
		StartSoftCorrection = true;
		return lightmonitorTool.StartSoftCorrection(ref result, StationIndex);
	}

	public bool EndoftCorrectionAcquire(ref string result, int StationIndex)
	{
		StartSoftCorrection = false;
		return lightmonitorTool.EndSoftCorrection(ref result, StationIndex);
	}

	public void SetSoftCorrectionONOFF(bool enabled)
	{
		paramFile.SoftCorrectionON = enabled;
		if (!enabled)
		{
			paramFile.SoftCorrection_R = (paramFile.SoftCorrection_G = (paramFile.SoftCorrection_B = -1));
			paramFile.SoftShift_R = (paramFile.SoftShift_G = (paramFile.SoftShift_B = 0));
		}
	}

	public void SetEnableFakeRatio(bool enabled)
	{
		for (int i = 0; i < 20; i++)
		{
			if (enabled)
			{
				paramFile.AdvLocateParam[i].EnableFakeWidthRatio = true;
				paramFile.AdvLocateParam[i].EnableFakeHeightRatio = true;
				paramFile.AdvLocateParam[i].ValueChangeFlag = true;
			}
			else
			{
				paramFile.AdvLocateParam[i].EnableFakeWidthRatio = false;
				paramFile.AdvLocateParam[i].EnableFakeHeightRatio = false;
				paramFile.AdvLocateParam[i].ValueChangeFlag = true;
			}
		}
	}

	public void SetIRColor(string ircolor)
	{
		irTool.SetIRColor(ircolor);
	}

	private void DirectoryCopy(string sourceDir, string targetDir)
	{
		if (!Directory.Exists(sourceDir))
		{
			return;
		}
		if (!Directory.Exists(targetDir))
		{
			Directory.CreateDirectory(targetDir);
			if (!Directory.Exists(targetDir))
			{
				return;
			}
		}
		string text = sourceDir.Replace(Directory.GetParent(sourceDir).ToString(), "").Replace(Path.DirectorySeparatorChar.ToString(), "");
		if (!(sourceDir == targetDir + text))
		{
			string[] files = Directory.GetFiles(sourceDir);
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i], targetDir + Path.DirectorySeparatorChar + Path.GetFileName(files[i]), overwrite: true);
			}
			string[] directories = Directory.GetDirectories(sourceDir);
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryCopy(directories[j], targetDir);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting)
	{
		bool flag = false;
		try
		{
			SourcePath = (SourcePath.EndsWith("\\") ? SourcePath : (SourcePath + "\\"));
			DestinationPath = (DestinationPath.EndsWith("\\") ? DestinationPath : (DestinationPath + "\\"));
			if (Directory.Exists(SourcePath))
			{
				if (!Directory.Exists(DestinationPath))
				{
					Directory.CreateDirectory(DestinationPath);
				}
				string[] files = Directory.GetFiles(SourcePath);
				for (int i = 0; i < files.Length; i++)
				{
					FileInfo fileInfo = new FileInfo(files[i]);
					fileInfo.CopyTo(DestinationPath + fileInfo.Name, overwriteexisting);
				}
				files = Directory.GetDirectories(SourcePath);
				foreach (string text in files)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(text);
					if (!CopyDirectory(text, DestinationPath + directoryInfo.Name, overwriteexisting))
					{
						flag = false;
					}
				}
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void CopyDireToDire(string sourceDir, string destDir)
	{
		if (!Directory.Exists(destDir))
		{
			Directory.CreateDirectory(destDir);
		}
		DirectoryInfo dir = new DirectoryInfo(sourceDir);
		List<FileInfo> list = new List<FileInfo>();
		GetFileList(dir, list);
		List<DirectoryInfo> list2 = new List<DirectoryInfo>();
		GetDirList(dir, list2);
		foreach (DirectoryInfo item in list2)
		{
			string path = item.FullName.Replace(sourceDir, destDir);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
		foreach (FileInfo item2 in list)
		{
			string fullName = item2.FullName;
			string destFileName = fullName.Replace(sourceDir, destDir);
			File.Copy(fullName, destFileName, overwrite: true);
		}
	}

	private static void GetFileList(DirectoryInfo dir, List<FileInfo> fileList)
	{
		fileList.AddRange(dir.GetFiles());
		DirectoryInfo[] directories = dir.GetDirectories();
		for (int i = 0; i < directories.Length; i++)
		{
			GetFileList(directories[i], fileList);
		}
	}

	private static void GetDirList(DirectoryInfo dir, List<DirectoryInfo> dirList)
	{
		dirList.AddRange(dir.GetDirectories());
		DirectoryInfo[] directories = dir.GetDirectories();
		for (int i = 0; i < directories.Length; i++)
		{
			GetDirList(directories[i], dirList);
		}
	}

	public bool GetStationINIStatus()
	{
		if (StationINI == null)
		{
			return false;
		}
		return true;
	}

	public int GetStationIndex()
	{
		return StationIndex;
	}
}
