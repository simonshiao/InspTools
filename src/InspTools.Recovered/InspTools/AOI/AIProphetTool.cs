using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace InspTools.AOI;

public class AIProphetTool
{
	private SessionOptions sessionOptions = new SessionOptions();

	private InferenceSession session;

	private IntPtr m_DeltaAIProcessTensorRT;

	private Mat ResizeOrg = new Mat();

	public Mat DestMatBinary = new Mat();

	private Mat[] bgrmat = new Mat[3];

	private Mat bgr = new Mat();

	private Mat bgrResize = new Mat();

	private Mat SimpleNetMat = new Mat();

	private Mat SimpleNetTemp = new Mat();

	private Mat SimpleNetTemp2 = new Mat();

	private Mat heatMapTemp = new Mat();

	private Mat heatMap = new Mat();

	private Mat heatMapColor = new Mat();

	private float[] imageData;

	private float[] imageData2;

	public bool useCUDA = true;

	private bool Inited;

	private ParamFile param;

	public bool GPUExist = true;

	private CVBuffer buffers;

	public string ResultText = "";

	public bool ModelReaded;

	public Mat org = new Mat();

	private string[] DBfilelist;

	public bool ScoreNormalize;

	private float[] TotalScore;

	private int TestCounter;

	public double UsageTime { get; protected set; }

	public float FinalScore { get; protected set; }

	[DllImport("DeltaAIProcessDll.dll")]
	private static extern IntPtr CreateInstance();

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern int ReadSimpleNetTensorRT(IntPtr pShapeInstance, string modelname);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern float PredictSimpleNetImageTensorRT(IntPtr pShapeInstance, IntPtr Image, IntPtr[] outPtr);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern IntPtr PredictSimpleNetImageTensorRT(IntPtr pShapeInstance, IntPtr Image, ref float score);

	public AIProphetTool(ParamFile paramfile)
	{
		useCUDA = true;
		ResizeOrg = new Mat(480, 640, MatType.CV_8UC3);
		param = paramfile;
		for (int i = 0; i < 3; i++)
		{
			bgrmat[i] = new Mat();
		}
	}

	~AIProphetTool()
	{
		sessionOptions.Dispose();
		sessionOptions.Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void OnnxInit(int usecuda = 2)
	{
		switch (usecuda)
		{
		case 1:
			useCUDA = true;
			break;
		default:
			useCUDA = false;
			break;
		case 2:
			break;
		}
		if (useCUDA)
		{
			sessionOptions.AppendExecutionProvider_CUDA();
		}
		sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_BASIC;
		Inited = true;
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PModel\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	public void ResetModelRead()
	{
		ModelReaded = false;
	}

	public Size GetModelSize()
	{
		if (session == null)
		{
			return new Size(0, 0);
		}
		Size result = new Size(256, 256);
		_ = session.InputMetadata;
		IReadOnlyDictionary<string, NodeMetadata> inputMetadata = session.InputMetadata;
		_ = session.OutputMetadata;
		using (IEnumerator<KeyValuePair<string, NodeMetadata>> enumerator = inputMetadata.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				int[] dimensions = enumerator.Current.Value.Dimensions;
				result.Height = dimensions[1];
				result.Width = dimensions[2];
				return result;
			}
		}
		return new Size(0, 0);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(string modelname)
	{
		try
		{
			if (!Inited)
			{
				OnnxInit();
			}
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PModel\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2;
			if (modelname == "")
			{
				text2 = string.Format("{0}{1}.onnx", text, param.AIProphetParam.ModelName);
				modelname = param.AIProphetParam.ModelName;
			}
			else
			{
				text2 = string.Format("{0}{1}.onnx", text, modelname);
			}
			if (!File.Exists(text2))
			{
				if (buffers.LanguageType == 1)
				{
					ResultText = string.Format("[AI先知] 找不到AI模型檔案({0})。\n", text2);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText = string.Format("[AI先知] 找不到AI模型檔案({0})。\n", text2);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText = string.Format("[AI予言者] AI モデル ファイルが見つかりません。({0})。\n", text2);
				}
				else
				{
					ResultText = string.Format("[AI Prophet Tool] Can't find AI Model.{0}。\n", text2);
				}
				return false;
			}
			if (new FileInfo(text2).Length < 10)
			{
				param.AIProphetParam.ModelUnTrain = true;
				if (buffers.LanguageType == 1)
				{
					ResultText = string.Format("[AI先知] AI模型尚未訓練。({0})。\n", param.AIProphetParam.ModelName);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText = string.Format("[AI先知] AI模型尚未训练。({0})。\n", param.AIProphetParam.ModelName);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText = string.Format("[AI予言者] AIモデルはまだ訓練されていない。({0})。\n", param.AIProphetParam.ModelName);
				}
				else
				{
					ResultText = string.Format("[AI Prophet Tool] AI model is not trained yet.{0}。\n", param.AIProphetParam.ModelName);
				}
				return false;
			}
			if (CheckEncrypt(text2))
			{
				if (ReadDecryptModel(text2))
				{
					param.AIProphetParam.ModelUnTrain = false;
					ModelReaded = true;
					return true;
				}
				return false;
			}
			session = new InferenceSession(text2, sessionOptions);
			param.AIProphetParam.ModelImageSize = GetModelSize().Width;
			if (!param.AIProphetParam.EnableTensorRT)
			{
				param.AIProphetParam.ModelUnTrain = false;
				ModelReaded = true;
			}
			else
			{
				string text3 = string.Format("{0}{1}.trt", text, modelname);
				m_DeltaAIProcessTensorRT = CreateInstance();
				if (!File.Exists(text3))
				{
					string text4 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\temp\\001.onnx");
					string text5 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\temp\\001.trt");
					string text6 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\temp\\0011.trt");
					File.Copy(text2, text4, overwrite: true);
					switch (RunCovertTRTScript(text4, text5))
					{
					case -1:
						if (buffers.LanguageType == 1)
						{
							ResultText = string.Format("[AI先知] onnx2engine.py。\n");
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText = string.Format("[AI先知] onnx2engine.py。\n");
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText = string.Format("[AI先知] onnx2engine.pyが見つかりません。\n");
						}
						else
						{
							ResultText = string.Format("[AI Prophet] Trt conversion program onnx2engine.py not found.\n");
						}
						return false;
					case 0:
						if (buffers.LanguageType == 1)
						{
							ResultText = string.Format("[AI先知] 找不到AI先知模型檔案{0}。\n", text3);
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText = string.Format("[AI先知] 找不到AI先知模型档案{0}。\n", text3);
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText = string.Format("[AI先知] AI予言者モデルファイル {0} が見つかりません。\n", text3);
						}
						else
						{
							ResultText = string.Format("[AI Prophet] Cannot find AI Prophet model file {0}.\n", text3);
						}
						return false;
					}
					if (File.Exists(text3))
					{
						File.Delete(text3);
					}
					Decrypt(text5, text6);
					ReadSimpleNetTensorRT(m_DeltaAIProcessTensorRT, text6);
					File.Copy(text5, text3);
					File.Delete(text4);
					File.Delete(text5);
					File.Delete(text6);
				}
				else
				{
					string text7 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\temp\\002.trt");
					Decrypt(text3, text7);
					ReadSimpleNetTensorRT(m_DeltaAIProcessTensorRT, text7);
					File.Delete(text7);
				}
				param.AIProphetParam.ModelUnTrain = false;
				ModelReaded = true;
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIProphetTool-ReadModel()-" + ex.ToString());
			return false;
		}
	}

	private void Normalize(string modelname)
	{
		int dBKind = 0;
		UpdateDBList(dBKind, modelname);
		if (DBfilelist.Length == 0)
		{
			ScoreNormalize = false;
			return;
		}
		_ = DBfilelist.Length;
		float[] array = new float[DBfilelist.Length];
		for (int i = 0; i < DBfilelist.Length; i++)
		{
			Mat mat = Cv2.ImRead(DBfilelist[i]);
			float num = PredictDB(mat);
			array[i] = num;
		}
		dBKind = 1;
		UpdateDBList(dBKind, modelname);
		if (DBfilelist.Length == 0)
		{
			ScoreNormalize = false;
			return;
		}
		_ = DBfilelist.Length;
		float[] array2 = new float[DBfilelist.Length + 1];
		array2[DBfilelist.Length - 1] = 0f;
		for (int j = 0; j < DBfilelist.Length; j++)
		{
			Mat mat = Cv2.ImRead(DBfilelist[j]);
			float num2 = PredictDB(mat);
			array2[j] = num2;
		}
		TotalScore = array.Concat(array2).ToArray();
		ScoreNormalize = true;
	}

	public int GetRealScore(float score)
	{
		if (!ScoreNormalize || TotalScore == null)
		{
			return -1;
		}
		if (TotalScore.Length == 0)
		{
			return -1;
		}
		TotalScore[TotalScore.Length - 1] = score;
		return (int)(100.0 - Normalize(TotalScore, 0.0, 100.0));
	}

	private static double Normalize(float[] numbers, double min, double max)
	{
		double num = numbers.Min();
		double num2 = numbers.Max();
		double[] array = new double[numbers.Length];
		for (int i = 0; i < numbers.Length; i++)
		{
			if ((num2 - num) * (max - min) > 0.0)
			{
				array[i] = ((double)numbers[i] - num) / (num2 - num) * (max - min);
			}
			else
			{
				array[i] = 0.0;
			}
		}
		return array[array.Length - 1];
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool UpdateDBList(int DBKind, string ModelName)
	{
		string text = "";
		string path = string.Format(arg2: DBKind switch
		{
			0 => "NOPASS", 
			1 => "OK", 
			_ => "PASS", 
		}, format: "{0}PModel\\{1}\\{2}\\", arg0: AppDomain.CurrentDomain.BaseDirectory, arg1: ModelName);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		DBfilelist = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
		if (DBfilelist.Length == 0)
		{
			return false;
		}
		return true;
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

	private Rect GetTrack(Rect LocatedRect, Rect MaxRect, int L, int R, int T, int B)
	{
		return new Rect
		{
			X = LocatedRect.Left + L,
			Y = LocatedRect.Top + T,
			Width = LocatedRect.Width - (L + R),
			Height = LocatedRect.Height - (T + B)
		} & MaxRect;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(bool Offline, int ImageID = 0)
	{
		FinalScore = 0f;
		try
		{
			if (Offline)
			{
				heatMapColor = new Mat();
				heatMap = new Mat();
				org.SetTo(0.0);
			}
			Rect locatedRect = buffers.LocatedRect;
			Rect maxRect = new Rect(0, 0, buffers.LocatedSrc.Width, buffers.LocatedSrc.Height);
			Rect track = GetTrack(locatedRect, maxRect, param.AIProphetParam.Track_L, param.AIProphetParam.Track_R, param.AIProphetParam.Track_T, param.AIProphetParam.Track_B);
			new Mat(buffers.LocatedSrc, track).CopyTo(org);
			Rect rect = new Rect(0, 0, org.Width, org.Height);
			if (param.AIProphetParam.Mask1_ON)
			{
				Rect track2 = GetTrack(locatedRect, maxRect, param.AIProphetParam.Mask1_L, param.AIProphetParam.Mask1_R, param.AIProphetParam.Mask1_T, param.AIProphetParam.Mask1_B);
				if (track2.Width > 0 && track2.Height > 0)
				{
					track2.Left -= track.Left;
					track2.Top -= track.Top;
					track2 &= rect;
					Mat mat = new Mat(org, track2);
					if (mat.Width > 0 && mat.Height > 0)
					{
						mat.SetTo(0.0);
					}
				}
			}
			if (param.AIProphetParam.Mask2_ON)
			{
				Rect track3 = GetTrack(locatedRect, maxRect, param.AIProphetParam.Mask2_L, param.AIProphetParam.Mask2_R, param.AIProphetParam.Mask2_T, param.AIProphetParam.Mask2_B);
				if (track3.Width > 0 && track3.Height > 0)
				{
					track3.Left -= track.Left;
					track3.Top -= track.Top;
					track3 &= rect;
					Mat mat2 = new Mat(org, track3);
					if (mat2.Width > 0 && mat2.Height > 0)
					{
						mat2.SetTo(0.0);
					}
				}
			}
			if (param.AIProphetParam.Mask3_ON)
			{
				Rect track4 = GetTrack(locatedRect, maxRect, param.AIProphetParam.Mask3_L, param.AIProphetParam.Mask3_R, param.AIProphetParam.Mask3_T, param.AIProphetParam.Mask3_B);
				if (track4.Width > 0 && track4.Height > 0)
				{
					track4.Left -= track.Left;
					track4.Top -= track.Top;
					track4 &= rect;
					Mat mat3 = new Mat(org, track4);
					if (mat3.Width > 0 && mat3.Height > 0)
					{
						mat3.SetTo(0.0);
					}
				}
			}
			if (param.AIProphetParam.Mask4_ON)
			{
				Rect track5 = GetTrack(locatedRect, maxRect, param.AIProphetParam.Mask4_L, param.AIProphetParam.Mask4_R, param.AIProphetParam.Mask4_T, param.AIProphetParam.Mask4_B);
				if (track5.Width > 0 && track5.Height > 0)
				{
					track5.Left -= track.Left;
					track5.Top -= track.Top;
					track5 &= rect;
					Mat mat4 = new Mat(org, track5);
					if (mat4.Width > 0 && mat4.Height > 0)
					{
						mat4.SetTo(0.0);
					}
				}
			}
			ResultText = "";
			if (!ModelReaded && !ReadModel(""))
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += "[AI先知] 檢測失敗，模型讀取失敗。\n";
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += "[AI先知] 检测失败，模型读取失败。\n";
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += "[AIせんし] 検知失敗、モデル読み込み失敗.\n";
				}
				else
				{
					ResultText += "[AI Prophet Tool] Inspect error, Read model error.\n";
				}
				FinalScore = 10f;
				return false;
			}
			if (org.Width <= 0 && org.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += "[AI先知] 檢測失敗，AI影像寬高為0，請檢查遮罩工具[1]是否開啟設定。\n";
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += "[AI先知] 检测失败，AI影像宽高为0，请检查遮罩工具[1]是否开启设定。\n";
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += "[AIせんし] 検知失敗、AI画像の幅と高さが0です。マスクツール[1]の設定が有効になっているかどうかを確認してください\n";
				}
				else
				{
					ResultText += "[AI Prophet Tool] Inspect error, AI image width and height = 0, Please check mask tool[1] first.\n";
				}
				FinalScore = 10f;
				return false;
			}
			UsageTime = 0.0;
			FinalScore = 0f;
			FinalScore = PredictAI(org, Offline, ImageID);
			if ((double)FinalScore > param.AIProphetParam.AllowSocreMax)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText = string.Format("[AI 先知]檢測失敗，分數 = {0:N2}\n\n", FinalScore);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText = string.Format("[AI先知]检测失败，分数 = {0:N2}\n\n", FinalScore);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText = string.Format("[AIせんし]検知失敗、スコア = {0:N2}\n\n", FinalScore);
				}
				else
				{
					ResultText = string.Format("[AI Prophet Tool] Inspect error, Score = {0:N2}\n\n", FinalScore);
				}
				return false;
			}
			if (buffers.LanguageType == 1)
			{
				ResultText = string.Format("[AI 先知] 分數 = {0:N2}\n\n", FinalScore);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultText = string.Format("[AI先知] 分数 = {0:N2}\n\n", FinalScore);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultText = string.Format("[AIせんし] スコア = {0:N2}\n\n", FinalScore);
			}
			else
			{
				ResultText = string.Format("[AI Prophet Tool] Score = {0:N2}\n\n", FinalScore);
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Model檔案已存在" + ex.ToString());
			ResultText = string.Format("[AI前處理] AI前處理異常!(Exception)。\n");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private float PredictAI(Mat mat, bool offline, int ImageID = 0)
	{
		float score = 0f;
		int num = 0;
		if (!ModelReaded)
		{
			return -1f;
		}
		if (mat.Width <= 0 || mat.Height <= 0)
		{
			return -1f;
		}
		Size dsize = new Size(param.AIProphetParam.ModelImageSize, param.AIProphetParam.ModelImageSize);
		try
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			num = 1;
			bool enableTensorRT = param.AIProphetParam.EnableTensorRT;
			Size dsize2 = mat.Size();
			ResizeOrg = mat.Resize(dsize, 0.0, 0.0, InterpolationFlags.Area);
			if (!enableTensorRT)
			{
				num = 2;
				score = SimpleNetPredict(ResizeOrg, session, out var heat_map);
				if (offline)
				{
					heatMap = SimpleNetPostProcess(heat_map, param.AIProphetParam.ModelImageSize, param.AIProphetParam.ModelImageSize);
				}
			}
			else
			{
				num = 3;
				IntPtr intPtr = default(IntPtr);
				intPtr = PredictSimpleNetImageTensorRT(m_DeltaAIProcessTensorRT, ResizeOrg.CvPtr, ref score);
				num = 31;
				Mat mat2 = new Mat(dsize.Height, dsize.Width, MatType.CV_8UC1, intPtr, 0L);
				num = 32;
				mat2.CopyTo(heatMapTemp);
				num = 33;
				if (offline)
				{
					heatMapTemp.CopyTo(heatMap);
				}
			}
			num = 4;
			stopwatch.Stop();
			UsageTime = stopwatch.Elapsed.TotalMilliseconds;
			if (offline)
			{
				Cv2.ApplyColorMap(heatMap, heatMapColor, ColormapTypes.Jet);
				heatMapColor = heatMapColor.Resize(dsize2, 0.0, 0.0, InterpolationFlags.Area);
				DestMatBinary = heatMap.Resize(dsize2, 0.0, 0.0, InterpolationFlags.Area);
				Cv2.InRange(DestMatBinary, 128.0, 255.0, DestMatBinary);
				num = 6;
			}
			return score;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("AT01-AIProphet-PredictAI()-01-DebugCode={0}, w={1}, h={2}\n", num, dsize.Width, dsize.Height) + ex.ToString());
			return 0f;
		}
	}

	public Mat GetHeatMapBinaryImage()
	{
		return DestMatBinary;
	}

	public Mat GetHeatMapImage()
	{
		return heatMapColor;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SendPredictTest()
	{
		int num = 0;
		try
		{
			Size size = new Size(param.AIProphetParam.ModelImageSize, param.AIProphetParam.ModelImageSize);
			Mat mat = new Mat(size, MatType.CV_8UC3);
			ResizeOrg = mat.Resize(size);
			PredictAI(mat, offline: false);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("AT01-AIProphet-SendPredictTest()-01-DebugCode={0}\n", num) + ResultText + ex.ToString());
		}
	}

	private void findMaxBlob(Mat binary, ref Mat result)
	{
		ConnectedComponents connectedComponents = Cv2.ConnectedComponentsEx(binary);
		if (connectedComponents.LabelCount > 1)
		{
			ConnectedComponents.Blob largestBlob = connectedComponents.GetLargestBlob();
			result.SetTo(0.0);
			connectedComponents.FilterByBlob(binary, result, largestBlob);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int RunCovertTRTScript(string onnxfile, string trtfile)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "SaveAIPTrt.bat");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if (!File.Exists(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\temp\\onnx2engine.py")))
		{
			return -1;
		}
		using (StreamWriter streamWriter = new StreamWriter(path, append: true))
		{
			_ = param.AIProphetParam;
			string arg = string.Format("cd Training\ncd temp\npython onnx2engine.py --onnx-path {0} --engine-path {1}", Path.GetFileName(onnxfile), Path.GetFileName(trtfile));
			streamWriter.Write("{0}", arg);
			streamWriter.WriteLine();
			streamWriter.Close();
		}
		Process process = Process.Start(new ProcessStartInfo
		{
			FileName = "SaveAIPTrt.bat",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		});
		string eOut = null;
		process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
		{
			eOut += e.Data;
		};
		process.BeginErrorReadLine();
		process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		process.Close();
		if (File.Exists(trtfile))
		{
			return 1;
		}
		return 0;
	}

	public int GetModelSize2()
	{
		return param.AIProphetParam.ModelImageSize;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadDecryptModel(string ModelFile)
	{
		try
		{
			string text = ModelFile.Replace(".onnx", ".trt");
			if (param.AIProphetParam.EnableTensorRT && !File.Exists(ModelFile))
			{
				param.AIProphetParam.EnableTensorRT = false;
			}
			if (param.AIProphetParam.EnableTensorRT)
			{
				if (!File.Exists(text))
				{
					string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					int num = new Random(Guid.NewGuid().GetHashCode()).Next();
					string text2 = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num);
					Decrypt(ModelFile, text2);
					session = new InferenceSession(text2, sessionOptions);
					param.AIProphetParam.ModelImageSize = GetModelSize().Width;
					string text3 = text2.Replace(".tmp", ".trmp");
					string text4 = text2.Replace(".tmp", ".trmp2");
					if (File.Exists(text3))
					{
						File.Delete(text3);
					}
					if (RunCovertTRTScript(text2, text3) == -1)
					{
						if (buffers.LanguageType == 1)
						{
							ResultText = string.Format("[AI先知] onnx2engine.py。\n");
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText = string.Format("[AI先知] onnx2engine.py。\n");
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText = string.Format("[AI先知] onnx2engine.pyが見つかりません。\n");
						}
						else
						{
							ResultText = string.Format("[AI Prophet] Trt conversion program onnx2engine.py not found.\n");
						}
						return false;
					}
					if (File.Exists(text3))
					{
						Decrypt(text3, text4);
						m_DeltaAIProcessTensorRT = CreateInstance();
						ReadSimpleNetTensorRT(m_DeltaAIProcessTensorRT, text4);
						File.Copy(text3, text, overwrite: true);
						File.Delete(text3);
						File.Delete(text4);
						File.Delete(text2);
						return true;
					}
					if (buffers.LanguageType == 1)
					{
						ResultText = string.Format("[AI先知] 找不到AI先知模型檔案{0}。\n", ModelFile);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI先知] 找不到AI先知模型档案{0}。\n", ModelFile);
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText = string.Format("[AI先知] AI予言者モデルファイル {0} が見つかりません。\n", ModelFile);
					}
					else
					{
						ResultText = string.Format("[AI Prophet] Cannot find AI Prophet model file {0}.\n", ModelFile);
					}
					File.Delete(text2);
					return false;
				}
				string path2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
				if (!Directory.Exists(path2))
				{
					Directory.CreateDirectory(path2);
				}
				int num2 = new Random(Guid.NewGuid().GetHashCode()).Next();
				string text5 = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num2);
				Decrypt(ModelFile, text5);
				session = new InferenceSession(text5, sessionOptions);
				param.AIProphetParam.ModelImageSize = GetModelSize().Width;
				string text6 = string.Format("{0}{1}{2}.trmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num2);
				Decrypt(text, text6);
				if (File.Exists(text6))
				{
					m_DeltaAIProcessTensorRT = CreateInstance();
					ReadSimpleNetTensorRT(m_DeltaAIProcessTensorRT, text6);
					File.Delete(text6);
					File.Delete(text5);
					return true;
				}
				if (buffers.LanguageType == 1)
				{
					ResultText = string.Format("[AI先知] 找不到AI先知模型檔案{0}。\n", text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText = string.Format("[AI先知] 找不到AI先知模型档案{0}。\n", text);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText = string.Format("[AI先知] AI予言者モデルファイル {0} が見つかりません。\n", text);
				}
				else
				{
					ResultText = string.Format("[AI Prophet] Cannot find AI Prophet model file {0}.\n", text);
				}
				File.Delete(text5);
				return false;
			}
			string path3 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
			if (!Directory.Exists(path3))
			{
				Directory.CreateDirectory(path3);
			}
			int num3 = new Random(Guid.NewGuid().GetHashCode()).Next();
			string text7 = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", num3);
			Decrypt(ModelFile, text7);
			if (File.Exists(text7))
			{
				session = new InferenceSession(text7, sessionOptions);
				param.AIProphetParam.ModelImageSize = GetModelSize().Width;
				File.Delete(text7);
				return true;
			}
			if (buffers.LanguageType == 1)
			{
				ResultText = string.Format("[AI先知] 找不到AI先知模型檔案{0}。\n", ModelFile);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultText = string.Format("[AI先知] 找不到AI先知模型档案{0}。\n", ModelFile);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultText = string.Format("[AI先知] AI予言者モデルファイル {0} が見つかりません。\n", ModelFile);
			}
			else
			{
				ResultText = string.Format("[AI Prophet] Cannot find AI Prophet model file {0}.\n", ModelFile);
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIProphet-ReadCryptModel()-05-" + ResultText + ex.ToString());
			ResultText = string.Format("[AI先知] 找不到AIAI先知模型{0}。\n", ModelFile);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CheckEncrypt(string _filename)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		if (!File.Exists(_filename))
		{
			return false;
		}
		try
		{
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
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIProphetTool-CheckEncrypt()-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void Encrypt(string _filename, string dst_name)
	{
		try
		{
			if (!File.Exists(dst_name))
			{
				File.Delete(dst_name);
			}
			BinaryReader binaryReader = new BinaryReader(new FileStream(_filename, FileMode.Open));
			binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
			long length = binaryReader.BaseStream.Length;
			byte[] buffer = binaryReader.ReadBytes((int)length);
			binaryReader.Close();
			byte[] array = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				array[i] = 0;
			}
			array[0] = 1;
			array[4] = 2;
			array[14] = 8;
			array[15] = 64;
			BinaryWriter binaryWriter = new BinaryWriter(new FileStream(dst_name, FileMode.Create));
			binaryWriter.Write(array);
			binaryWriter.Write(buffer);
			binaryWriter.Close();
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIProphetTool-Encrypt()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void Decrypt(string _filename, string dst_name)
	{
		try
		{
			if (File.Exists(dst_name))
			{
				File.Delete(dst_name);
			}
			BinaryReader binaryReader = new BinaryReader(new FileStream(_filename, FileMode.Open));
			binaryReader.BaseStream.Seek(16L, SeekOrigin.Begin);
			long num = binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
			byte[] buffer = binaryReader.ReadBytes((int)num);
			binaryReader.Close();
			BinaryWriter binaryWriter = new BinaryWriter(new FileStream(dst_name, FileMode.Create));
			binaryWriter.Write(buffer);
			binaryWriter.Close();
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIProphetTool-Decrypt()-" + ex.ToString());
		}
	}

	private static float SimpleNetPredict(Mat image, InferenceSession session, out byte[] heat_map)
	{
		byte[] array = new byte[image.Total() * image.Channels()];
		Marshal.Copy(image.Data, array, 0, array.Length);
		int[] array2 = new int[4]
		{
			1,
			image.Rows,
			image.Cols,
			image.Channels()
		};
		DenseTensor<byte> value = new DenseTensor<byte>(array, array2);
		string name = session.InputMetadata.Keys.First();
		List<NamedOnnxValue> list = new List<NamedOnnxValue>();
		list.Add(NamedOnnxValue.CreateFromTensor(name, value));
		IDisposableReadOnlyCollection<DisposableNamedOnnxValue> disposableReadOnlyCollection = session.Run(list);
		Tensor<byte> source = disposableReadOnlyCollection.First().AsTensor<byte>();
		Tensor<byte> source2 = disposableReadOnlyCollection[1].AsTensor<byte>();
		Tensor<byte> source3 = disposableReadOnlyCollection[2].AsTensor<byte>();
		heat_map = source.ToArray();
		float result = (float)(int)source2.ToArray()[0] / 255f + (float)(int)source3.ToArray()[0];
		disposableReadOnlyCollection.Dispose();
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public float PredictDB(Mat mat)
	{
		if (session == null)
		{
			return 99f;
		}
		try
		{
			bool enableTensorRT = param.AIProphetParam.EnableTensorRT;
			Size dsize = mat.Size();
			Size dsize2 = new Size(param.AIProphetParam.ModelImageSize, param.AIProphetParam.ModelImageSize);
			ResizeOrg = mat.Resize(dsize2, 0.0, 0.0, InterpolationFlags.Area);
			float score = 0f;
			if (!enableTensorRT)
			{
				score = SimpleNetPredict(ResizeOrg, session, out var heat_map);
				heatMap = SimpleNetPostProcess(heat_map, param.AIProphetParam.ModelImageSize, param.AIProphetParam.ModelImageSize);
			}
			else
			{
				IntPtr intPtr = default(IntPtr);
				intPtr = PredictSimpleNetImageTensorRT(m_DeltaAIProcessTensorRT, ResizeOrg.CvPtr, ref score);
				new Mat(ResizeOrg.Rows, ResizeOrg.Cols, MatType.CV_8UC1, intPtr, 0L).CopyTo(heatMap);
			}
			Cv2.ApplyColorMap(heatMap, heatMapColor, ColormapTypes.Jet);
			heatMapColor = heatMapColor.Resize(dsize, 0.0, 0.0, InterpolationFlags.Area);
			return score;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIProphetTool-PredictDB()-" + ex.ToString());
			return 99f;
		}
	}

	private Mat SimpleNetPostProcess(byte[] outputValues, int width, int height)
	{
		Mat mat = new Mat(height, width, MatType.CV_8UC1);
		Marshal.Copy(outputValues, 0, mat.Data, outputValues.Length);
		return mat;
	}

	private float[] MatToFloatArray(Mat image)
	{
		int num = image.Rows * image.Cols * image.Channels();
		imageData2 = new float[num];
		Mat[] mv = new Mat[image.Channels()];
		Cv2.Split(image, out mv);
		int num2 = image.Rows * image.Cols;
		for (int i = 0; i < image.Channels(); i++)
		{
			Marshal.Copy(mv[i].Data, imageData2, i * num2, num2);
		}
		return imageData2;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Rect findMaxBlob(Mat binary)
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
			_ = "BT02-AIPropherTool-findBlob()-" + ex.ToString();
			return result;
		}
	}
}
