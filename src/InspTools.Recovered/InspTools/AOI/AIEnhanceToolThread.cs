using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;

namespace InspTools.AOI;

public class AIEnhanceToolThread
{
	private IntPtr trt_process;

	private bool TRTDllExist;

	private IntPtr m_DeltaAIProcess_A;

	private IntPtr m_DeltaAIProcess_B;

	private Size AIImageSize = new Size(256, 256);

	private Mat ResizeOrg = new Mat();

	public Mat AIMatR = new Mat();

	public Mat AIMatG = new Mat();

	public Mat AIMatB = new Mat();

	public Mat DestMatColor = new Mat();

	private Mat MatPtrTemp = new Mat();

	private Mat DestMatTemp = new Mat();

	private Mat DestMatTemp2 = new Mat();

	private Mat[] bgrmat = new Mat[3];

	private Mat bgr = new Mat();

	private Mat bgrResize = new Mat();

	private bool[] PbExist = new bool[5];

	private bool DllExist;

	public bool useCUDA = true;

	private string H5Name = "";

	private ParamFile param;

	public bool ModelLoaded_A;

	public bool ModelLoaded_B;

	private bool ModelACreatInst;

	private bool ModelBCreatInst;

	public string ResultText;

	public bool DLLMiss;

	public string DLLMissStr = "";

	private CVBuffer buffers;

	private int _gpu;

	private string ScriptResult = "";

	private Thread AIPredictThread;

	private ManualResetEvent AIPredict_PauseEvent = new ManualResetEvent(initialState: false);

	private ManualResetEvent WaitAIPredict_PauseEvent = new ManualResetEvent(initialState: false);

	private Mat ResultMat = new Mat();

	public bool GPUSeted;

	private bool ThreadRun;

	private bool OffLine;

	public double UsageTime { get; protected set; }

	[DllImport("DeltaAIProcessDll.dll")]
	private static extern IntPtr CreateInstance();

	[DllImport("DeltaAIProcessDll.dll")]
	private static extern void DisposeInstance(IntPtr pShapeInstance);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern void SetGPU(IntPtr pShapeInstance, int gpu_id);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern void ReadModel(IntPtr pShapeInstance, string modelname, bool useCUDA);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern IntPtr PredictImageToImage(IntPtr pShapeInstance, IntPtr Image);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern IntPtr PredictImage(IntPtr pShapeInstance, IntPtr Image);

	[DllImport("DeltaAIProcessDll.dll")]
	private static extern void AddImageToPool(IntPtr pShapeInstance, IntPtr Image);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern IntPtr PredictImages(IntPtr pShapeInstance);

	[DllImport("TRT_0610.dll")]
	private static extern IntPtr TRT_CreateInstance();

	[DllImport("TRT_0610.dll")]
	private static extern void TRT_DisposeInstance(IntPtr pShapeInstance);

	[DllImport("TRT_0610.dll")]
	public static extern IntPtr TRT_LoadNet(IntPtr pShapeInstance, string trtFileName, int GPU_id);

	[DllImport("TRT_0610.dll")]
	public static extern void TRT_ReleaseNet(IntPtr pShapeInstance, IntPtr net);

	[DllImport("TRT_0610.dll")]
	public static extern IntPtr TRT_DoInference_image2image(IntPtr pShapeInstance, string input_name, string output_name, IntPtr input);

	[DllImport("TRT_0610.dll")]
	public static extern IntPtr TRT_DoInference_unet_channel(IntPtr pShapeInstance, string input_name, string output_name, IntPtr input, float[] output, int input_X, int input_Y, int input_C, int div, int output_size, int width, int height);

	[DllImport("TRT_0610.dll")]
	public static extern IntPtr TRT_SetImageSize(IntPtr pShapeInstance, int width, int height);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public AIEnhanceToolThread(ParamFile paramfile)
	{
		useCUDA = true;
		useCUDA = true;
		string path = AppDomain.CurrentDomain.BaseDirectory + "DeltaAIProcessDll.dll";
		if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "TRT_New.dll"))
		{
			TRTDllExist = true;
			trt_process = TRT_CreateInstance();
		}
		else
		{
			TRTDllExist = false;
		}
		if (File.Exists(path))
		{
			DllExist = true;
		}
		else
		{
			DllExist = false;
		}
		param = paramfile;
		ResizeOrg = new Mat(480, 640, MatType.CV_8UC3);
		ResizeOrg.SetTo(255.0);
		param = paramfile;
		for (int i = 0; i < 3; i++)
		{
			bgrmat[i] = new Mat();
		}
		for (int j = 0; j < 5; j++)
		{
			PbExist[j] = false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void ReleaseThread()
	{
		try
		{
			ThreadRun = false;
			AIPredict_PauseEvent.Set();
			WaitAIPredict_PauseEvent.Set();
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("AIT01-AIEnhanceTool-ReleaseThread-") + ex.ToString());
		}
	}

	private void PredictThreadRun(IntPtr DeltaAIProcess, int gpu_id)
	{
		while (ThreadRun)
		{
			AIPredict_PauseEvent.WaitOne(-1);
			if (!ThreadRun)
			{
				AIPredict_PauseEvent.Set();
				WaitAIPredict_PauseEvent.Set();
				break;
			}
			if (!GPUSeted)
			{
				SetGPU(DeltaAIProcess, gpu_id);
			}
			GPUSeted = true;
			if (ResizeOrg.Width > 0 || ResizeOrg.Height > 0)
			{
				new Mat(PredictImageToImage(DeltaAIProcess, ResizeOrg.CvPtr)).CopyTo(ResultMat);
			}
			if (!ThreadRun)
			{
				AIPredict_PauseEvent.Set();
				WaitAIPredict_PauseEvent.Set();
				break;
			}
			AIPredict_PauseEvent.Reset();
			WaitAIPredict_PauseEvent.Set();
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CreatInst(int abindex)
	{
		try
		{
			if (ModelACreatInst)
			{
				return;
			}
			m_DeltaAIProcess_A = CreateInstance();
			if (useCUDA)
			{
				int GPUID = 0;
				if (param.AIEnhanceParam.EnableAsignGPU && param.GetThreadID() == 1)
				{
					GPUID = param.AIEnhanceParam.GPUID;
				}
				if (GPUID < 0)
				{
					GPUID = 0;
				}
				if (!ThreadRun)
				{
					ThreadRun = true;
					AIPredictThread = new Thread((ThreadStart)delegate
					{
						PredictThreadRun(m_DeltaAIProcess_A, GPUID);
					});
					AIPredictThread.Start();
				}
			}
			ModelACreatInst = true;
		}
		catch (Exception ex)
		{
			string msg = "AT01-AIEnhanceTool-CreatInst()-01-" + ex.ToString();
			ModelACreatInst = false;
			CVBuffer.SaveLog(msg);
		}
	}

	public void ReAssignGPU(int GPUID)
	{
		if (ModelACreatInst)
		{
			if (GPUID < 0)
			{
				GPUID = 0;
			}
			GPUSeted = false;
		}
	}

	public void SetBuffers(CVBuffer bufs)
	{
		buffers = bufs;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool RunSavePbScript()
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "SavePb.bat");
		ScriptResult = "";
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			string.Format("Training\\Model\\h5_to_pb.exe");
			using (StreamWriter streamWriter = new StreamWriter(path, append: true))
			{
				streamWriter.Write("{0}", "cd Training\ncd Model\nh5_to_pb.exe");
				streamWriter.WriteLine();
			}
			Process process = Process.Start(new ProcessStartInfo
			{
				FileName = "SavePb.bat",
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
			string text = process.StandardOutput.ReadToEnd();
			ScriptResult = text + eOut;
			process.WaitForExit();
			process.Close();
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat("AT01-AIEnhanceTool-RunSavePbScript()-05-" + ex.ToString(), ScriptResult));
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ConvertToPb(string h5file, string pbfile, int abIndex = 0)
	{
		string text = string.Format("{0}{1}navs.h5", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
		try
		{
			if (!File.Exists(h5file))
			{
				ResultText += string.Format("[AI前處理] PB檔轉檔失敗，找不到{0}。\n", h5file);
				return false;
			}
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text);
			}
			if (new FileInfo(h5file).Length < 10)
			{
				ResultText += string.Format("[AI前處理] PB檔轉檔失敗，{0}模型尚未訓練。\n", Path.GetFileNameWithoutExtension(h5file));
				return false;
			}
			File.Copy(h5file, text, overwrite: true);
			if (RunSavePbScript())
			{
				string text2 = string.Format("{0}{1}navs.pb", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
				if (File.Exists(text2))
				{
					if (File.Exists(pbfile))
					{
						FileInfo fileInfo2 = new FileInfo(pbfile);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
						File.Delete(pbfile);
					}
					File.Move(text2, pbfile);
					ScriptResult = "";
					return true;
				}
				if (ScriptResult != "")
				{
					CVBuffer.SaveLog("AT01-AIEnhanceTool-ConvertToPb()-" + ScriptResult);
				}
				if (!File.Exists(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "license.lic")))
				{
					ResultText += string.Format("[AI前處理] 認證失敗，找不到License認證檔案。\n");
				}
				else
				{
					ResultText += string.Format("[AI前處理] PB檔轉檔失敗，找不到Training\\Model\\navs.pb。\n");
				}
				return false;
			}
			ResultText = string.Format("[AI前處理] 找不到轉換PB程式或轉換PB失敗(SavePb.bat)。\n");
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ConvertToPb()-" + ex.ToString());
			if (abIndex == 0 || !param.AIEnhanceParam.EnableSelectFace)
			{
				param.AIEnhanceParam.ModelExist_A = false;
				ModelLoaded_A = false;
			}
			else
			{
				param.AIEnhanceParam.ModelExist_B = false;
				ModelLoaded_B = false;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool RunSaveTRTScript(ref string Result)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "SaveOnnx.bat");
		string path2 = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs.h5");
		string text = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs.onnx");
		string text2 = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs.trt");
		Result = "";
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			using (StreamWriter streamWriter = new StreamWriter(path, append: true))
			{
				streamWriter.Write("{0}", "cd Training\ncd Model\nh52onnx.exe");
				streamWriter.WriteLine();
			}
			Process process = Process.Start(new ProcessStartInfo
			{
				FileName = "SaveOnnx.bat",
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
			string text3 = process.StandardOutput.ReadToEnd();
			ScriptResult = text3 + eOut;
			process.WaitForExit();
			process.Close();
		}
		catch (Exception ex)
		{
			string text4 = "AT01-AIEnhanceTool-RunSaveTRTScript()-H52Onnx-";
			CVBuffer.SaveLog(Result = text4 + ex.ToString());
			return false;
		}
		try
		{
			if (!File.Exists(text))
			{
				Result = string.Format("[AI前處理] Onnx檔轉檔失敗，找不到{0}。\n", text);
				return false;
			}
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			string path3 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "SaveTrt.bat");
			if (File.Exists(path3))
			{
				File.Delete(path3);
			}
			using (StreamWriter streamWriter2 = new StreamWriter(path3, append: true))
			{
				streamWriter2.Write("{0}", "cd Training\ncd Model\ntrtexec --workspace=2048 --onnx=navs.onnx --saveEngine=navs.trt  --explicitBatch --fp16");
				streamWriter2.WriteLine();
			}
			Process process2 = Process.Start(new ProcessStartInfo
			{
				FileName = "SaveTrt.bat",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			});
			string eOut2 = null;
			process2.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
			{
				eOut2 += e.Data;
			};
			process2.BeginErrorReadLine();
			string text5 = process2.StandardOutput.ReadToEnd();
			ScriptResult = text5 + eOut2;
			process2.WaitForExit();
			process2.Close();
		}
		catch (Exception ex2)
		{
			CVBuffer.SaveLog(string.Concat("AT01-AIEnhanceTool-RunSaveTRTScript()-Onnx2Trt-" + ex2.ToString(), ScriptResult));
			return false;
		}
		if (!File.Exists(text2))
		{
			Result = string.Format("[AI前處理] Trt檔轉檔失敗，找不到{0}。\n", text2);
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			return false;
		}
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ConvertToTRT(string h5file, string trtfile, int abIndex = 0)
	{
		string text = string.Format("{0}{1}navs.h5", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
		try
		{
			if (!File.Exists(h5file))
			{
				ResultText += string.Format("[AI前處理] TRT檔轉檔失敗，找不到{0}。\n", h5file);
				return false;
			}
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text);
			}
			if (new FileInfo(h5file).Length < 10)
			{
				ResultText += string.Format("[AI前處理] TRT檔轉檔失敗，{0}模型尚未訓練。\n", Path.GetFileNameWithoutExtension(h5file));
				return false;
			}
			File.Copy(h5file, text, overwrite: true);
			if (RunSaveTRTScript(ref ResultText))
			{
				string text2 = string.Format("{0}{1}navs.trt", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
				if (File.Exists(text2))
				{
					if (File.Exists(trtfile))
					{
						FileInfo fileInfo2 = new FileInfo(trtfile);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
						File.Delete(trtfile);
					}
					File.Move(text2, trtfile);
					ScriptResult = "";
					return true;
				}
				if (ScriptResult != "")
				{
					CVBuffer.SaveLog("AT01-AIEnhanceTool-ConvertToTRT()-01-" + ScriptResult);
				}
				ResultText += string.Format("[AI前處理] Trt檔案轉換失敗，找不到training\\model\\navs.trt。\n");
				return false;
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ConvertToTRT()-05-" + ex.ToString());
			if (abIndex == 0 || !param.AIEnhanceParam.EnableSelectFace)
			{
				param.AIEnhanceParam.ModelExist_A = false;
				ModelLoaded_A = false;
			}
			else
			{
				param.AIEnhanceParam.ModelExist_B = false;
				ModelLoaded_B = false;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(bool ReCreatePB = false, int abIndex = 0, int stationIndex = 0)
	{
		bool flag = true;
		if (abIndex == 0 && !ModelACreatInst)
		{
			CreatInst(abIndex);
		}
		if (abIndex == 1 && !ModelBCreatInst)
		{
			CreatInst(abIndex);
		}
		try
		{
			ModelLoaded_A = false;
			if (param.AIEnhanceParam.ModelName_A == "")
			{
				ResultText = string.Format("[AI前處理] 尚未設定AI前處理模型。\n");
				return false;
			}
			ResultText = "";
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\train.exe");
			string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\h5_to_pb.exe");
			if (!File.Exists(text) || !File.Exists(text2))
			{
				if (!File.Exists(text))
				{
					ResultText += string.Format("[AI前處理] 找不到訓練主程式 {0}。\n", text);
				}
				if (!File.Exists(text2))
				{
					ResultText += string.Format("[AI前處理] 找不到PB轉換程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text2);
				}
				return false;
			}
			string arg = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			string arg2 = Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A) + ".h5";
			string text3 = string.Format("{0}{1}", arg, arg2);
			H5Name = Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A) + ".h5d";
			string text4 = string.Format("{0}{1}", arg, H5Name);
			File.Exists(text4);
			if (!File.Exists(text3) && !File.Exists(text4))
			{
				if (!File.Exists(text4))
				{
					ResultText += string.Format("[AI前處理] 找不到模型檔案 {0}。\n", H5Name);
					return false;
				}
			}
			else if (File.Exists(text3) && !File.Exists(text4))
			{
				FileInfo fileInfo = new FileInfo(text3);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Move(text3, text4);
			}
			if (!File.Exists(text4))
			{
				ResultText += string.Format("[AI前處理] 找不到模型檔案 {0}。\n", H5Name);
				return false;
			}
			AIImageSize.Width = param.AIEnhanceParam.Width_A;
			AIImageSize.Height = param.AIEnhanceParam.Height_A;
			if (param.AIEnhanceParam.UseTensorRT && TRTDllExist)
			{
				string arg3 = Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A) + ".Trt";
				string text5 = string.Format("{0}{1}", arg, arg3);
				PbExist[abIndex] = File.Exists(text5);
				if (!PbExist[abIndex] && !ConvertToTRT(text4, text5))
				{
					flag = false;
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					return false;
				}
				PbExist[abIndex] = true;
				TRT_LoadNet(trt_process, text5, _gpu);
				TRT_SetImageSize(trt_process, AIImageSize.Width, AIImageSize.Height);
				ModelLoaded_A = true;
				param.AIEnhanceParam.ModelExist_A = true;
				return true;
			}
			string arg4 = Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A) + ".pbd";
			string text6 = string.Format("{0}{1}", arg, arg4);
			PbExist[abIndex] = File.Exists(text6);
			if (!PbExist[abIndex])
			{
				if (ConvertToPb(text4, text6))
				{
					PbExist[abIndex] = File.Exists(text6);
					if (PbExist[abIndex])
					{
						if (CheckEncrypt(text6, stationIndex))
						{
							if (!ReadDecryptModel(m_DeltaAIProcess_A, text6, stationIndex))
							{
								ModelLoaded_A = false;
								param.AIEnhanceParam.ModelExist_A = false;
								if (File.Exists(text6))
								{
									File.Delete(text6);
								}
								return false;
							}
							ModelLoaded_A = true;
							param.AIEnhanceParam.ModelExist_A = true;
						}
						else
						{
							ReadModel(m_DeltaAIProcess_A, text6, useCUDA);
						}
						ModelLoaded_A = true;
						param.AIEnhanceParam.ModelExist_A = true;
						return true;
					}
					flag = false;
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					ResultText += string.Format("[AI前處理] 找不到AI前處理模型檔案{0}。\n", H5Name);
					return false;
				}
				flag = false;
				PbExist[abIndex] = false;
				ModelLoaded_A = false;
				param.AIEnhanceParam.ModelExist_A = false;
				ResultText += string.Format("[AI前處理] AI前處理模型檔案轉換失敗{0}。\n", text6);
				return false;
			}
			if (CheckEncrypt(text6, stationIndex))
			{
				if (!ReadDecryptModel(m_DeltaAIProcess_A, text6, stationIndex))
				{
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					if (File.Exists(text6))
					{
						File.Delete(text6);
					}
					return false;
				}
				ModelLoaded_A = true;
				param.AIEnhanceParam.ModelExist_A = true;
			}
			else
			{
				ReadModel(m_DeltaAIProcess_A, text6, useCUDA);
			}
			PbExist[abIndex] = true;
			ModelLoaded_A = true;
			param.AIEnhanceParam.ModelExist_A = true;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-ReadModel()-01-Model格式錯誤 " + ex.ToString());
			ModelLoaded_A = false;
			param.AIEnhanceParam.ModelExist_A = false;
			if (!flag)
			{
				ResultText = string.Format("[AI前處理] 模型檔案轉換失敗。\n");
			}
			else
			{
				ResultText = string.Format("[AI前處理] 模型檔案讀取失敗。\n");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(string modelname, int abIndex = 0, bool AbsolutePath = false)
	{
		try
		{
			if (abIndex == 0 && !ModelACreatInst)
			{
				CreatInst(abIndex);
			}
			if (abIndex == 1 && !ModelBCreatInst)
			{
				CreatInst(abIndex);
			}
			string arg = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			H5Name = modelname + ".h5d";
			string text = "";
			text = (AbsolutePath ? modelname : string.Format("{0}{1}", arg, H5Name));
			AIImageSize.Width = param.AIEnhanceParam.Width_A;
			AIImageSize.Height = param.AIEnhanceParam.Height_A;
			if (param.AIEnhanceParam.UseTensorRT && TRTDllExist)
			{
				string text2 = text.Replace(".h5d", ".trt");
				PbExist[abIndex] = File.Exists(text2);
				if (!PbExist[abIndex] && !ConvertToTRT(text, text2))
				{
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					return false;
				}
				PbExist[abIndex] = true;
				TRT_LoadNet(trt_process, text2, _gpu);
				TRT_SetImageSize(trt_process, AIImageSize.Width, AIImageSize.Height);
				ModelLoaded_A = true;
				param.AIEnhanceParam.ModelExist_A = true;
				param.AIEnhanceParam.NewModelMode = false;
				return true;
			}
			string text3 = text.Replace(".h5d", ".pbd");
			if (!File.Exists(text3))
			{
				if (ConvertToPb(text, text3))
				{
					PbExist[abIndex] = File.Exists(text3);
					if (PbExist[abIndex])
					{
						if (CheckEncrypt(text3))
						{
							if (!ReadDecryptModel(m_DeltaAIProcess_A, text3))
							{
								ModelLoaded_A = false;
								param.AIEnhanceParam.ModelExist_A = false;
								if (File.Exists(text3))
								{
									File.Delete(text3);
								}
								return false;
							}
							ModelLoaded_A = true;
							param.AIEnhanceParam.ModelExist_A = true;
						}
						else
						{
							ReadModel(m_DeltaAIProcess_A, text3, useCUDA);
						}
						ModelLoaded_A = true;
						param.AIEnhanceParam.ModelExist_A = true;
						return true;
					}
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					ResultText = string.Format("[AI前處理] 找不到AI前處理模型檔案{0}。\n", H5Name);
					return false;
				}
				PbExist[abIndex] = false;
				ModelLoaded_A = false;
				param.AIEnhanceParam.ModelExist_A = false;
				ResultText = string.Format("[AI前處理]AI前處理模型檔案轉換失敗{0}。\n", text3);
				return false;
			}
			PbExist[abIndex] = true;
			if (CheckEncrypt(text3))
			{
				if (!ReadDecryptModel(m_DeltaAIProcess_A, text3))
				{
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					if (File.Exists(text3))
					{
						File.Delete(text3);
					}
					return false;
				}
				ModelLoaded_A = true;
				param.AIEnhanceParam.ModelExist_A = true;
			}
			else
			{
				ReadModel(m_DeltaAIProcess_A, text3, useCUDA);
			}
			ModelLoaded_A = true;
			param.AIEnhanceParam.ModelExist_A = true;
			param.AIEnhanceParam.NewModelMode = false;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-ReadModel(string modelname)-01-" + ex.ToString());
			if (abIndex == 0 || !param.AIEnhanceParam.EnableSelectFace)
			{
				ModelLoaded_A = false;
				param.AIEnhanceParam.ModelExist_A = false;
			}
			else
			{
				ModelLoaded_B = false;
				param.AIEnhanceParam.ModelExist_B = false;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(Mat org, bool Offline, int FaceIndex, bool offline = false)
	{
		OffLine = Offline;
		H5Name = param.AIEnhanceParam.ModelName_A;
		if (param.AIEnhanceParam.NewModelMode)
		{
			string text = string.Format("{0}Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, H5Name);
			if (!File.Exists(text))
			{
				return true;
			}
			if (new FileInfo(text).Length < 10)
			{
				return true;
			}
			param.AIEnhanceParam.NewModelMode = false;
		}
		ResultText = "";
		string arg = "GPU";
		if (!useCUDA)
		{
			arg = "CPU";
		}
		if (!param.AIEnhanceParam.EnableSelectFace || FaceIndex <= 0 || FaceIndex >= 5)
		{
			FaceIndex = 1;
		}
		try
		{
			H5Name = param.AIEnhanceParam.ModelName_A;
			if (param.AIEnhanceParam.UseTensorRT && TRTDllExist)
			{
				if (!PbExist[FaceIndex - 1])
				{
					if (buffers.LanguageType == 1)
					{
						ResultText = string.Format("[AI前處理] 找不到AI模型檔案({0}.trt)。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI前處理] 找不到AI模型檔案({0}.trt)。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText = string.Format("[AI前処理] AI モデル ファイルが見つかりません。({0}.trt)。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
					}
					else
					{
						ResultText = string.Format("[AI Enhance Tool] Can't find AI Model.{0}.trt。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
					}
					return false;
				}
				if (!ModelLoaded_A)
				{
					if (!ModelLoaded_A)
					{
						ResultText += string.Format("[AI前處理][{0}][{1}] 未載入AI模型({2})。\n", H5Name, arg, H5Name);
					}
					return false;
				}
				UsageTime = 0.0;
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				bool result = PredictTrtMat(org, Offline, FaceIndex);
				stopwatch.Stop();
				UsageTime = stopwatch.Elapsed.TotalMilliseconds;
				return result;
			}
			if (!PbExist[FaceIndex - 1])
			{
				if (buffers.LanguageType == 1)
				{
					ResultText = string.Format("[AI前處理] 找不到AI模型檔案({0}.pbd)。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText = string.Format("[AI前處理] 找不到AI模型檔案({0}.pbd)。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText = string.Format("[AI前処理] AI モデル ファイルが見つかりません。({0}.pbd)。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
				}
				else
				{
					ResultText = string.Format("[AI Enhance Tool] Can't find AI Model.{0}.pbd。\n", Path.GetFileNameWithoutExtension(param.AIEnhanceParam.ModelName_A));
				}
				return false;
			}
			if (!DllExist || !ModelLoaded_A || !ModelACreatInst)
			{
				if (!DllExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI前處理][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI前处理][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText += string.Format("[AI前処理][{0}][{1}] AI DLL が見つかりません。({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
					}
					else
					{
						ResultText += string.Format("[AI Enhance Tool][{0}][{1}] Can't find AI DLL({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
					}
				}
				if (!ModelLoaded_A)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI前處理][{0}][{1}] 未載入AI模型({2})。\n", H5Name, arg, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI前处理][{0}][{1}] 未载入AI模型({2})。\n", H5Name, arg, H5Name);
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText += string.Format("[AI前処理][{0}][{1}] AI モデルがロードされていません。({2})。\n", H5Name, arg, H5Name);
					}
					else
					{
						ResultText += string.Format("[AI Enhance Tool][{0}][{1}] Can't Load AI Model({2})。\n", H5Name, arg, H5Name);
					}
				}
				return false;
			}
			UsageTime = 0.0;
			Stopwatch stopwatch2 = new Stopwatch();
			stopwatch2.Start();
			bool result2 = PredictMat(org, Offline, FaceIndex);
			stopwatch2.Stop();
			UsageTime = stopwatch2.Elapsed.TotalMilliseconds;
			return result2;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("Model檔案已存在" + ex.ToString());
			ResultText = string.Format("[AI前處理] AI前處理異常!(Exception)。\n");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SendPredictTest()
	{
		try
		{
			if (DllExist && ModelLoaded_A && ModelACreatInst && AIImageSize.Width > 0 && AIImageSize.Height > 0)
			{
				Mat mat = new Mat(AIImageSize, MatType.CV_8UC3);
				_ = mat.Width;
				_ = mat.Height;
				ResizeOrg = mat.Resize(AIImageSize);
				AIPredict_PauseEvent.Set();
				WaitAIPredict_PauseEvent.WaitOne(-1);
				WaitAIPredict_PauseEvent.Reset();
				if (ResultMat.Width > 0 && ResultMat.Height > 0)
				{
					Cv2.CvtColor(ResultMat, bgr, ColorConversionCodes.BGR2RGB);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-SendPredictTest()-01-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool PredictMat(Mat org, bool Offline = false, int FaceIndex = 1)
	{
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		try
		{
			if (FaceIndex == 1)
			{
				if (!PbExist[FaceIndex - 1] || !DllExist || !ModelLoaded_A || !ModelACreatInst)
				{
					if (!PbExist[FaceIndex - 1])
					{
						ResultText = string.Format("[AI前處理][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
					}
					if (!DllExist)
					{
						ResultText += string.Format("[AI前處理][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					if (!ModelLoaded_A)
					{
						ResultText += string.Format("[AI前處理][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
					}
					return false;
				}
				if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
				{
					ResultText += string.Format("[AI前處理][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
					return false;
				}
				_ = org.Width;
				_ = org.Height;
				ResizeOrg = org.Resize(AIImageSize);
				AIPredict_PauseEvent.Set();
				WaitAIPredict_PauseEvent.WaitOne(-1);
				WaitAIPredict_PauseEvent.Reset();
				if (ResultMat.Width <= 0 || ResultMat.Height <= 0)
				{
					ResultText += string.Format("[AI前處理][{0}][{1}] AI回傳影像尺寸為0，請檢查AI前處理模型檔案。\n", H5Name, text);
					return false;
				}
				Cv2.CvtColor(ResultMat, bgr, ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(bgr, bgrResize, new Size(org.Width, org.Height));
					bgrResize.CopyTo(DestMatColor);
				}
				Cv2.Split(bgr, out bgrmat);
				Cv2.Resize(bgrmat[2], AIMatR, new Size(org.Width, org.Height));
				Cv2.Resize(bgrmat[1], AIMatG, new Size(org.Width, org.Height));
				Cv2.Resize(bgrmat[0], AIMatB, new Size(org.Width, org.Height));
				return true;
			}
			if (!PbExist[FaceIndex - 1] || !DllExist || !ModelLoaded_B || !ModelBCreatInst)
			{
				if (!PbExist[FaceIndex - 1])
				{
					ResultText = string.Format("[AI前處理][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
				}
				if (!DllExist)
				{
					ResultText += string.Format("[AI前處理][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
				}
				if (!ModelLoaded_B)
				{
					ResultText += string.Format("[AI前處理][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
				}
				return false;
			}
			if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
			{
				ResultText += string.Format("[AI前處理][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				return false;
			}
			_ = org.Width;
			_ = org.Height;
			ResizeOrg = org.Resize(AIImageSize);
			AIPredict_PauseEvent.Set();
			WaitAIPredict_PauseEvent.WaitOne(-1);
			WaitAIPredict_PauseEvent.Reset();
			if (ResultMat.Width <= 0 || ResultMat.Height <= 0)
			{
				ResultText += string.Format("[AI前處理][{0}][{1}] AI回傳影像尺寸為0，請檢查AI前處理模型檔案。\n", H5Name, text);
				return false;
			}
			Cv2.CvtColor(ResultMat, bgr, ColorConversionCodes.BGR2RGB);
			if (Offline)
			{
				Cv2.Resize(bgr, bgrResize, new Size(org.Width, org.Height));
				bgrResize.CopyTo(DestMatColor);
			}
			Cv2.Split(bgr, out bgrmat);
			Cv2.Resize(bgrmat[2], AIMatR, new Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[1], AIMatG, new Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[0], AIMatB, new Size(org.Width, org.Height));
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-PredictMat()-01-" + ex.ToString());
			ResultText += string.Format("[AI前處理][{0}][{1}] AI前處理執行錯誤(PredictMat)，請檢查GPU、或是模型檔!({2})。\n", H5Name, text, H5Name);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool PredictTrtMat(Mat org, bool Offline = false, int FaceIndex = 1)
	{
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		try
		{
			if (!PbExist[FaceIndex - 1] || !TRTDllExist || !ModelLoaded_A)
			{
				if (!PbExist[FaceIndex - 1])
				{
					ResultText = string.Format("[AI前處理][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
				}
				if (!TRTDllExist)
				{
					ResultText += string.Format("[AI前處理][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "Trt.dll");
				}
				if (!ModelLoaded_A)
				{
					ResultText += string.Format("[AI前處理][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
				}
				return false;
			}
			if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
			{
				ResultText += string.Format("[AI前處理][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				return false;
			}
			string input_name = "args_0";
			string output_name = "unet_final_conv";
			_ = org.Width;
			_ = org.Height;
			_ = AIImageSize;
			_ = AIImageSize;
			_ = AIImageSize;
			_ = AIImageSize;
			ResizeOrg = org.Resize(AIImageSize);
			Cv2.CvtColor(ResizeOrg, ResizeOrg, ColorConversionCodes.BGR2RGB);
			DestMatTemp = new Mat(AIImageSize, MatType.CV_32FC3);
			ResizeOrg.ConvertTo(DestMatTemp, MatType.CV_32FC3, 1.0 / 255.0);
			IntPtr ptr = TRT_DoInference_image2image(trt_process, input_name, output_name, DestMatTemp.CvPtr);
			MatPtrTemp = new Mat(ptr);
			if (MatPtrTemp.Width <= 0 || MatPtrTemp.Height <= 0)
			{
				ResultText += string.Format("[AI前處理][{0}][{1}] AI回傳影像尺寸為0，請檢查AI前處理模型檔案。\n", H5Name, text);
				return false;
			}
			Cv2.CvtColor(MatPtrTemp, bgr, ColorConversionCodes.BGR2RGB);
			if (Offline)
			{
				Cv2.Resize(bgr, bgrResize, new Size(org.Width, org.Height));
				bgrResize.CopyTo(DestMatColor);
			}
			Cv2.Split(bgr, out bgrmat);
			Cv2.Resize(bgrmat[2], AIMatR, new Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[1], AIMatG, new Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[0], AIMatB, new Size(org.Width, org.Height));
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-PredictMat()-01-" + ex.ToString());
			ResultText += string.Format("[AI前處理][{0}][{1}] AI前處理執行錯誤(PredictMat)，請檢查GPU、或是模型檔!({2})。\n", H5Name, text, H5Name);
			return false;
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
	public bool ReadDecryptModel(IntPtr m_DeltaAIProcess, string PbFile, int stationIndex = 0)
	{
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string text = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", stationIndex + 1);
			File.Copy(PbFile, text, overwrite: true);
			string text2 = string.Format("{0}{1}{2}.pb", AppDomain.CurrentDomain.BaseDirectory, "temp", stationIndex);
			PbDecrypt(text, text2);
			if (File.Exists(text2))
			{
				ReadModel(m_DeltaAIProcess, text2, useCUDA);
				File.Delete(text2);
				File.Delete(text);
				return true;
			}
			ResultText = string.Format("[AI前處理] 找不到AI前處理模型解碼檔案{0}。\n", PbFile);
			File.Delete(text);
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ReadCryptModel()-05-" + ex.ToString());
			ResultText = string.Format("[AI前處理] 找不到AI前處理模型解碼失敗{0}。\n", PbFile);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CheckEncrypt(string _filename, int stationIndex = 0)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string text = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", stationIndex + 1);
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
}
