using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class AIPredictTool
{
	private IntPtr trt_ptr;

	private bool TRTDllExist;

	private IntPtr m_DeltaAIProcess_A;

	private IntPtr m_DeltaAIProcess_B;

	private IntPtr m_DeltaAIShow_A;

	private IntPtr m_DeltaAIShow_B;

	private Size AIImageSize = new Size(256, 256);

	private Mat ResizeOrg = new Mat();

	private bool[] PbExist = new bool[5];

	private bool DllExist;

	public bool useCUDA = true;

	private string H5Name = "";

	private string H5DName = "";

	private ParamFile param;

	private Mat TestImage = new Mat();

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

	private bool ModelACreatInst;

	private bool ModelBCreatInst;

	private int _gpu;

	private CVBuffer buffers;

	public bool PredictModelShowA;

	public bool PredictModelShowB;

	private string ScriptResult = "";

	public string ResultText = "";

	public bool DLLMiss;

	public string DLLMissStr = "";

	public double UsageTime { get; protected set; }

	public bool ModelLoaded_A { get; set; }

	public bool ModelLoaded_B { get; set; }

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

	[DllImport("TRT.dll")]
	public static extern IntPtr LoadNet(string trtFileName, int GPU_id);

	[DllImport("TRT.dll")]
	public static extern void ReleaseNet(IntPtr ptr);

	[DllImport("TRT.dll")]
	public static extern void DoInference(IntPtr ptr, string input_name, string output_name, float[] input, float[] output, int input_size, int output_size);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public AIPredictTool(ParamFile paramfile)
	{
		useCUDA = true;
		ModelLoaded_A = false;
		ModelLoaded_B = false;
		string path = AppDomain.CurrentDomain.BaseDirectory + "DeltaAIProcessDll.dll";
		if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "TRT.dll"))
		{
			TRTDllExist = true;
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
		for (int i = 0; i < 5; i++)
		{
			PbExist[i] = false;
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

	public void CreatInst(int abindex)
	{
		if (abindex == 0)
		{
			if (!ModelACreatInst)
			{
				m_DeltaAIProcess_A = CreateInstance();
				m_DeltaAIShow_A = CreateInstance();
				if (useCUDA)
				{
					SetGPU(m_DeltaAIProcess_A, _gpu);
					SetGPU(m_DeltaAIProcess_B, _gpu);
				}
				ModelACreatInst = true;
			}
		}
		else if (!ModelBCreatInst)
		{
			m_DeltaAIProcess_B = CreateInstance();
			m_DeltaAIShow_B = CreateInstance();
			if (useCUDA)
			{
				SetGPU(m_DeltaAIProcess_B, _gpu);
				SetGPU(m_DeltaAIShow_B, _gpu);
			}
			ModelBCreatInst = true;
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
				CreateNoWindow = true
			});
			process.WaitForExit();
			process.Close();
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredictTool-RunSavePbScript()-05-" + ex.ToString());
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
				ResultText += string.Format("[AI檢測] PB檔轉檔失敗，找不到{0}。\n", h5file);
				return false;
			}
			if (new FileInfo(h5file).Length < 10)
			{
				ResultText += string.Format("[AI檢測] PB檔轉檔失敗，{0}模型尚未訓練。\n", Path.GetFileNameWithoutExtension(h5file));
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
			File.Copy(h5file, text, overwrite: true);
			if (RunSavePbScript())
			{
				string text2 = string.Format("{0}{1}navs.pb", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\");
				if (File.Exists(text2))
				{
					if (File.Exists(pbfile))
					{
						File.Delete(pbfile);
					}
					File.Copy(text2, pbfile, overwrite: true);
					File.Delete(text2);
					return true;
				}
				if (!File.Exists(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "license.lic")))
				{
					ResultText += string.Format("[AI檢測] 認證失敗，找不到License認證檔案。\n");
				}
				else
				{
					ResultText += string.Format("[AI檢測] PB檔轉檔失敗，找不到Training\\Model\\navs.pb。\n");
				}
				return false;
			}
			ResultText = string.Format("[AI檢測] 找不到轉換PB程式(SavePb.bat)。\n");
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredictTool-ConvertToPb()-05-" + ex.ToString());
			if (abIndex == 0 || !param.AIPredictParam.EnableSelectFace)
			{
				param.AIPredictParam.ModelExist_A = false;
				ModelLoaded_A = false;
			}
			else
			{
				param.AIPredictParam.ModelExist_B = false;
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
			string text4 = "AT01-AIPredictTool-RunSaveTRTScript()-H52Onnx-";
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
			using (StreamWriter streamWriter2 = new StreamWriter(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "SaveTrt.bat"), append: true))
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
			CVBuffer.SaveLog(string.Concat("AT01-AIPredictTool-RunSaveTRTScript()-Onnx2Trt-" + ex2.ToString(), ScriptResult));
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
					CVBuffer.SaveLog("AT01-AIPredictTool-ConvertToTRT()-01-" + ScriptResult);
				}
				ResultText += string.Format("[AI前處理] Trt檔案轉換失敗，找不到training\\model\\navs.trt。\n");
				return false;
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredictTool-ConvertToTRT()-05-" + ex.ToString());
			if (abIndex == 0 || !param.AIPredictParam.EnableSelectFace)
			{
				param.AIPredictParam.ModelExist_A = false;
				ModelLoaded_A = false;
			}
			else
			{
				param.AIPredictParam.ModelExist_B = false;
				ModelLoaded_B = false;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(bool ReCreatePB = false, int abIndex = 0, int stationIndex = 0)
	{
		bool flag = true;
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
			if (param.AIPredictParam.ModelName_A == "")
			{
				ResultText = string.Format("[AI檢測] 尚未設定AI檢測模型。\n");
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
			AIImageSize.Width = param.AIPredictParam.Width_A;
			AIImageSize.Height = param.AIPredictParam.Height_A;
			string arg = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			H5Name = Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A) + ".h5";
			string text3 = string.Format("{0}{1}", arg, H5Name);
			H5DName = Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A) + ".h5d";
			string h5DFile = string.Format("{0}{1}", arg, H5DName);
			if (param.AIPredictParam.UseTensorRT && TRTDllExist)
			{
				string arg2 = Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A) + ".Trt";
				string text4 = string.Format("{0}{1}", arg, arg2);
				PbExist[abIndex] = File.Exists(text4);
				if (!PbExist[abIndex] && !ConvertToTRT(text3, text4))
				{
					flag = false;
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIPredictParam.ModelExist_A = false;
					return false;
				}
				PbExist[abIndex] = true;
				trt_ptr = LoadNet(text4, _gpu);
				ModelLoaded_A = true;
				param.AIPredictParam.ModelExist_A = true;
				return true;
			}
			string arg3 = Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A) + ".pb";
			string text5 = string.Format("{0}{1}", arg, arg3);
			string arg4 = Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A) + ".pbd";
			string pbDFile = string.Format("{0}{1}", arg, arg4);
			if (!File.Exists(text3))
			{
				ResultText += string.Format("[AI檢測] 找不到模型檔案 {0}。\n", H5Name);
				return false;
			}
			PbExist[abIndex] = File.Exists(text5);
			if (!PbExist[abIndex])
			{
				if (ConvertToPb(text3, text5))
				{
					PbExist[abIndex] = File.Exists(text5);
					if (PbExist[abIndex])
					{
						if (CheckEncrypt(text5, stationIndex))
						{
							if (!ReadDecryptModel(m_DeltaAIProcess_A, text5))
							{
								ModelLoaded_A = false;
								param.AIPredictParam.ModelExist_A = false;
								if (File.Exists(text5))
								{
									File.Delete(text5);
								}
								return false;
							}
							ModelLoaded_A = true;
							param.AIPredictParam.ModelExist_A = true;
						}
						else
						{
							ReadModel(m_DeltaAIProcess_A, text5, useCUDA);
							SetPredictModelShow(abIndex, h5DFile, pbDFile);
						}
						ModelLoaded_A = true;
						param.AIPredictParam.ModelExist_A = true;
						return true;
					}
					flag = false;
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIPredictParam.ModelExist_A = false;
					ResultText += string.Format("[AI檢測] 找不到AI檢測模型檔案{0}。\n", H5Name);
					return false;
				}
				flag = false;
				PbExist[abIndex] = false;
				ModelLoaded_A = false;
				param.AIPredictParam.ModelExist_A = false;
				ResultText += string.Format("[AI檢測] AI檢測模型檔案轉換失敗{0}。\n", text5);
				return false;
			}
			if (CheckEncrypt(text5, stationIndex))
			{
				if (!ReadDecryptModel(m_DeltaAIProcess_A, text5, stationIndex))
				{
					ModelLoaded_A = false;
					param.AIPredictParam.ModelExist_A = false;
					if (File.Exists(text5))
					{
						File.Delete(text5);
					}
					return false;
				}
				ModelLoaded_A = true;
				param.AIPredictParam.ModelExist_A = true;
			}
			else
			{
				ReadModel(m_DeltaAIProcess_A, text5, useCUDA);
				SetPredictModelShow(abIndex, h5DFile, pbDFile);
			}
			ModelLoaded_A = true;
			param.AIPredictParam.ModelExist_A = false;
			ResultText += string.Format("[AI檢測] AI前處理模型檔案讀取失敗{0}。\n", text5);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredictTool-ReadModel()-01-Model格式錯誤 " + ex.ToString());
			if (abIndex == 0 || !param.AIPredictParam.EnableSelectFace)
			{
				ModelLoaded_A = false;
				param.AIPredictParam.ModelExist_A = false;
				if (!flag)
				{
					ResultText = string.Format("[AI檢測] 模型檔案轉換失敗。\n");
				}
				else
				{
					ResultText = string.Format("[AI檢測] 模型檔案讀取失敗。(Model格式錯誤)\n");
				}
			}
			else
			{
				ModelLoaded_B = false;
				param.AIPredictParam.ModelExist_B = false;
				if (!flag)
				{
					ResultText = string.Format("[AI檢測][B面] 模型檔案轉換失敗。\n");
				}
				else
				{
					ResultText = string.Format("[AI檢測][B面] 模型檔案讀取失敗。(Model格式錯誤)\n");
				}
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(string modelname, int abIndex = 0)
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
			H5Name = Path.GetFileNameWithoutExtension(modelname) + ".h5";
			string h5file = string.Format("{0}{1}", arg, H5Name);
			AIImageSize.Width = param.AIPredictParam.Width_A;
			AIImageSize.Height = param.AIPredictParam.Height_A;
			if (param.AIPredictParam.UseTensorRT && TRTDllExist)
			{
				string arg2 = modelname + ".Trt";
				string text = string.Format("{0}{1}", arg, arg2);
				PbExist[abIndex] = File.Exists(text);
				if (!PbExist[abIndex] && !ConvertToTRT(h5file, text))
				{
					PbExist[abIndex] = false;
					ModelLoaded_A = false;
					param.AIPredictParam.ModelExist_A = false;
					return false;
				}
				PbExist[abIndex] = true;
				trt_ptr = LoadNet(text, _gpu);
				ModelLoaded_A = true;
				param.AIPredictParam.ModelExist_A = true;
				return true;
			}
			string arg3 = Path.GetFileNameWithoutExtension(modelname) + ".pb";
			string text2 = string.Format("{0}{1}", arg, arg3);
			if (ConvertToPb(h5file, text2))
			{
				PbExist[abIndex] = File.Exists(text2);
				if (PbExist[abIndex])
				{
					if (CheckEncrypt(text2))
					{
						if (!ReadDecryptModel(m_DeltaAIProcess_A, text2))
						{
							ModelLoaded_A = false;
							param.AIPredictParam.ModelExist_A = false;
							if (File.Exists(text2))
							{
								File.Delete(text2);
							}
							return false;
						}
						ModelLoaded_A = true;
						param.AIPredictParam.ModelExist_A = true;
					}
					else
					{
						ReadModel(m_DeltaAIProcess_A, text2, useCUDA);
					}
					ModelLoaded_A = true;
					param.AIPredictParam.ModelExist_A = true;
					return true;
				}
				PbExist[abIndex] = false;
				ModelLoaded_A = false;
				param.AIPredictParam.ModelExist_A = false;
				ResultText = string.Format("[AI檢測] 找不到AI前處理模型檔案{0}。\n", H5Name);
				return false;
			}
			PbExist[abIndex] = false;
			ModelLoaded_A = false;
			param.AIPredictParam.ModelExist_A = false;
			ResultText = string.Format("[AI檢測]AI前處理模型檔案轉換失敗{0}。\n", text2);
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredict-ReadModel(string modelname)-01-" + ex.ToString());
			if (abIndex == 0 || !param.AIPredictParam.EnableSelectFace)
			{
				ModelLoaded_A = false;
				param.AIPredictParam.ModelExist_A = false;
			}
			else
			{
				ModelLoaded_B = false;
				param.AIPredictParam.ModelExist_B = false;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(Mat org, ref TOOL_RESULT iresult, ref FLOW_BIN ibin, bool Offline = false, int FaceIndex = 1)
	{
		if (param.AIPredictParam.NewModelMode)
		{
			iresult = TOOL_RESULT.EM_OK;
			ibin = FLOW_BIN.EM_FLOW_OK;
			return true;
		}
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		if (!param.AIPredictParam.EnableSelectFace || FaceIndex <= 0 || FaceIndex >= 5)
		{
			FaceIndex = 1;
		}
		try
		{
			if (FaceIndex == 1)
			{
				if (!PbExist[FaceIndex - 1] || !DllExist || !ModelLoaded_A || !ModelACreatInst)
				{
					if (!PbExist[FaceIndex - 1])
					{
						ResultText = string.Format("[AI檢測][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
					}
					if (!DllExist)
					{
						ResultText += string.Format("[AI檢測][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					if (!ModelLoaded_A)
					{
						ResultText += string.Format("[AI檢測][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
					}
					iresult = TOOL_RESULT.EM_Retest;
					return false;
				}
				iresult = TOOL_RESULT.EM_OK;
				UsageTime = 0.0;
				if (param.AIPredictParam.UseTensorRT && TRTDllExist)
				{
					if (!PbExist[FaceIndex - 1])
					{
						if (buffers.LanguageType == 1)
						{
							ResultText = string.Format("[AI檢測] 找不到AI模型檔案({0}.trt)\n", Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A));
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText = string.Format("[AI檢測] 找不到AI模型檔案({0}.trt)\n", Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A));
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText = string.Format("[AI 検出] AI モデル ファイルが見つかりません。({0}.trt)\n", Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A));
						}
						else
						{
							ResultText = string.Format("[AI Predict Tool] Can't find AI Model.{0}.trt\n", Path.GetFileNameWithoutExtension(param.AIPredictParam.ModelName_A));
						}
						return false;
					}
					if (!ModelLoaded_A)
					{
						if (!ModelLoaded_A)
						{
							ResultText += string.Format("[AI檢測][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
						}
						return false;
					}
					string input_name = "args_0";
					string output_name = "predictions";
					int height = AIImageSize.Height;
					int height2 = AIImageSize.Height;
					int num = 3;
					int input_size = height * height2 * num;
					int num2 = 4;
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					TestImage = org.Resize(AIImageSize);
					DestMatTemp = new Mat(AIImageSize, MatType.CV_32FC3);
					TestImage.ConvertTo(DestMatTemp, MatType.CV_32FC3, 1.0 / 255.0);
					long num3 = DestMatTemp.Total() * DestMatTemp.Channels();
					float[] array = new float[num3];
					Marshal.Copy(DestMatTemp.Data, array, 0, Convert.ToInt32(num3));
					float[] array2 = new float[num2];
					DoInference(trt_ptr, input_name, output_name, array, array2, input_size, num2);
					double num4 = array2[0];
					double num5 = array2[1];
					double num6 = array2[2];
					double num7 = array2[3];
					if (num4 >= num5 && num4 >= num6 && num4 >= num7)
					{
						ibin = param.AIPredictParam.ErrorBin_A0;
						iresult = GetFlowResult(ibin);
					}
					else if (num5 >= num4 && num5 >= num6 && num5 >= num7)
					{
						ibin = param.AIPredictParam.ErrorBin_A1;
						iresult = GetFlowResult(ibin);
					}
					else if (num6 >= num4 && num6 >= num5 && num6 >= num7)
					{
						ibin = param.AIPredictParam.ErrorBin_A2;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_A3;
						iresult = GetFlowResult(ibin);
					}
					stopwatch.Stop();
					ResultText = string.Format("[AI檢測][{0}][{1}] 檢測結果 0:{2:N3}, 1:{3:N3}, 2:{4:N3}, 3:{5:N3}。\n", H5Name, text, num4, num5, num6, num7);
					return true;
				}
				Stopwatch stopwatch2 = new Stopwatch();
				stopwatch2.Start();
				TestImage = org.Resize(AIImageSize);
				Mat mat = new Mat(PredictImage(m_DeltaAIProcess_A, TestImage.CvPtr));
				stopwatch2.Stop();
				UsageTime = stopwatch2.Elapsed.TotalMilliseconds;
				if (Offline && PredictModelShowA)
				{
					PredictMat(org, Offline, FaceIndex);
				}
				if (mat.Cols == 2 || mat.Cols > 7)
				{
					double num8 = mat.At<float>(0, 0);
					double num9 = mat.At<float>(0, 1);
					if (num8 >= num9)
					{
						ibin = param.AIPredictParam.ErrorBin_A0;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_A1;
						iresult = GetFlowResult(ibin);
					}
					ResultText = string.Format("[AI檢測][{0}][{1}] 檢測結果 0={2:N3}, 1={3:N3}。\n", H5Name, text, num8, num9);
				}
				else if (mat.Cols == 3)
				{
					double num10 = mat.At<float>(0, 0);
					double num11 = mat.At<float>(0, 1);
					double num12 = mat.At<float>(0, 2);
					if (num10 >= num11 && num10 >= num12)
					{
						ibin = param.AIPredictParam.ErrorBin_A0;
						iresult = GetFlowResult(ibin);
					}
					else if (num11 >= num12)
					{
						ibin = param.AIPredictParam.ErrorBin_A1;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_A2;
						iresult = GetFlowResult(ibin);
					}
					ResultText = string.Format("[AI檢測][{0}][{1}] 檢測結果 0:{2:N3}, 1:{3:N3}, 2:{4:N3}。\n", H5Name, text, num10, num11, num12);
				}
				else
				{
					if (mat.Cols != 4)
					{
						iresult = TOOL_RESULT.EM_Retest;
						ibin = FLOW_BIN.EM_FLOW_RETESET;
						ResultText = string.Format("[AI檢測][{0}][{1}] 檢測結果欄位不符合(欄位數={2})。\n", H5Name, text, mat.Cols);
						return false;
					}
					double num13 = mat.At<float>(0, 0);
					double num14 = mat.At<float>(0, 1);
					double num15 = mat.At<float>(0, 2);
					double num16 = mat.At<float>(0, 3);
					if (num13 >= num14 && num13 >= num15 && num13 >= num16)
					{
						ibin = param.AIPredictParam.ErrorBin_A0;
						iresult = GetFlowResult(ibin);
					}
					else if (num14 >= num13 && num14 >= num15 && num14 >= num16)
					{
						ibin = param.AIPredictParam.ErrorBin_A1;
						iresult = GetFlowResult(ibin);
					}
					else if (num15 >= num13 && num15 >= num14 && num15 >= num16)
					{
						ibin = param.AIPredictParam.ErrorBin_A2;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_A3;
						iresult = GetFlowResult(ibin);
					}
					ResultText = string.Format("[AI檢測][{0}][{1}] 檢測結果 0:{2:N3}, 1:{3:N3}, 2:{4:N3}, 3:{5:N3}。\n", H5Name, text, num13, num14, num15, num16);
				}
			}
			else
			{
				if (!PbExist[FaceIndex - 1] || !DllExist || !ModelLoaded_B || !ModelBCreatInst)
				{
					if (!PbExist[FaceIndex - 1])
					{
						ResultText = string.Format("[AI檢測][B面][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
					}
					if (!DllExist)
					{
						ResultText += string.Format("[AI檢測][B面][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					if (!ModelLoaded_B)
					{
						ResultText += string.Format("[AI檢測][B面][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
					}
					iresult = TOOL_RESULT.EM_Retest;
					return false;
				}
				iresult = TOOL_RESULT.EM_OK;
				UsageTime = 0.0;
				Stopwatch stopwatch3 = new Stopwatch();
				stopwatch3.Start();
				TestImage = org.Resize(AIImageSize);
				Mat mat2 = new Mat(PredictImage(m_DeltaAIProcess_B, TestImage.CvPtr));
				stopwatch3.Stop();
				UsageTime = stopwatch3.Elapsed.TotalMilliseconds;
				if (Offline && PredictModelShowB)
				{
					PredictMat(org, Offline, FaceIndex);
				}
				if (mat2.Cols == 2 || mat2.Cols > 7)
				{
					double num17 = mat2.At<float>(0, 0);
					double num18 = mat2.At<float>(0, 1);
					if (num17 >= num18)
					{
						ibin = param.AIPredictParam.ErrorBin_B0;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_B1;
						iresult = GetFlowResult(ibin);
					}
					ResultText = string.Format("[AI檢測][B面][{0}][{1}] 檢測結果 0={2:N3}, 1={3:N3}。\n", H5Name, text, num17, num18);
				}
				else if (mat2.Cols == 3)
				{
					double num19 = mat2.At<float>(0, 0);
					double num20 = mat2.At<float>(0, 1);
					double num21 = mat2.At<float>(0, 2);
					if (num19 >= num20 && num19 >= num21)
					{
						ibin = param.AIPredictParam.ErrorBin_B0;
						iresult = GetFlowResult(ibin);
					}
					else if (num20 >= num21)
					{
						ibin = param.AIPredictParam.ErrorBin_B1;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_B2;
						iresult = GetFlowResult(ibin);
					}
					ResultText = string.Format("[AI檢測][B面][{0}][{1}] 檢測結果 0:{2:N3}, 1:{3:N3}, 2:{4:N3}。\n", H5Name, text, num19, num20, num21);
				}
				else
				{
					if (mat2.Cols != 4)
					{
						iresult = TOOL_RESULT.EM_Retest;
						ResultText = string.Format("[AI檢測][B面][{0}][{1}] 檢測結果欄位不符合(欄位數={2})。\n", H5Name, text, mat2.Cols);
						return false;
					}
					double num22 = mat2.At<float>(0, 0);
					double num23 = mat2.At<float>(0, 1);
					double num24 = mat2.At<float>(0, 2);
					double num25 = mat2.At<float>(0, 3);
					if (num22 >= num23 && num22 >= num24 && num22 >= num25)
					{
						ibin = param.AIPredictParam.ErrorBin_B0;
						iresult = GetFlowResult(ibin);
					}
					else if (num23 >= num22 && num23 >= num24 && num23 >= num25)
					{
						ibin = param.AIPredictParam.ErrorBin_B1;
						iresult = GetFlowResult(ibin);
					}
					else if (num24 >= num22 && num24 >= num23 && num24 >= num25)
					{
						ibin = param.AIPredictParam.ErrorBin_B2;
						iresult = GetFlowResult(ibin);
					}
					else
					{
						ibin = param.AIPredictParam.ErrorBin_B3;
						iresult = GetFlowResult(ibin);
					}
					ResultText = string.Format("[AI檢測][B面][{0}][{1}] 檢測結果 0:{2:N3}, 1:{3:N3}, 2:{4:N3}, 3:{5:N3}。\n", H5Name, text, num22, num23, num24, num25);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string msg = "AT01-AIPredict-Inspect()-01-" + ex.ToString();
			iresult = TOOL_RESULT.EM_Retest;
			ResultText = string.Format("[AI檢測] AI檢測異常!(Exception)。\n");
			CVBuffer.SaveLog(msg);
			return false;
		}
	}

	private TOOL_RESULT GetFlowResult(FLOW_BIN flowbin)
	{
		TOOL_RESULT result = TOOL_RESULT.EM_OK;
		switch (flowbin)
		{
		case FLOW_BIN.EM_FLOW_OK:
			result = TOOL_RESULT.EM_OK;
			break;
		case FLOW_BIN.EM_FLOW_NG1:
		case FLOW_BIN.EM_FLOW_NG2:
		case FLOW_BIN.EM_FLOW_NG3:
		case FLOW_BIN.EM_FLOW_NG4:
			result = TOOL_RESULT.EM_NG;
			break;
		case FLOW_BIN.EM_FLOW_RETESET:
			result = TOOL_RESULT.EM_Retest;
			break;
		}
		return result;
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
			ResultText = string.Format("[AI檢測] 找不到AI檢測模型解碼檔案{0}。\n", PbFile);
			File.Delete(text);
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredictTool-ReadCryptModel()-05-" + ex.ToString());
			ResultText = string.Format("[AI檢測] 找不到AI檢測模型解碼失敗{0}。\n", PbFile);
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SetPredictModelShow(int abIndex, string H5DFile, string PbDFile)
	{
		try
		{
			if (abIndex == 0)
			{
				PredictModelShowA = false;
				if (File.Exists(H5DFile))
				{
					if (File.Exists(PbDFile))
					{
						ReadModel(m_DeltaAIShow_A, PbDFile, useCUDA);
						PredictModelShowA = true;
					}
					else if (ConvertToPb(H5DFile, PbDFile))
					{
						ReadModel(m_DeltaAIShow_A, PbDFile, useCUDA);
						PredictModelShowA = true;
					}
				}
				return;
			}
			PredictModelShowB = false;
			if (File.Exists(H5DFile))
			{
				if (File.Exists(PbDFile))
				{
					ReadModel(m_DeltaAIShow_B, PbDFile, useCUDA);
					PredictModelShowB = true;
				}
				else if (ConvertToPb(H5DFile, PbDFile))
				{
					ReadModel(m_DeltaAIShow_B, PbDFile, useCUDA);
					PredictModelShowB = true;
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIPredictTool-SetPredictModelShow()-" + ex.ToString());
			if (abIndex == 0)
			{
				PredictModelShowA = false;
			}
			else
			{
				PredictModelShowB = false;
			}
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
				if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
				{
					ResultText += string.Format("[AI檢測][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
					return false;
				}
				_ = org.Width;
				_ = org.Height;
				ResizeOrg = org.Resize(AIImageSize);
				Cv2.CvtColor(new Mat(PredictImageToImage(m_DeltaAIShow_A, ResizeOrg.CvPtr)), bgr, ColorConversionCodes.BGR2RGB);
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
					ResultText = string.Format("[AI檢測][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
				}
				if (!DllExist)
				{
					ResultText += string.Format("[AI檢測[{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
				}
				if (!ModelLoaded_B)
				{
					ResultText += string.Format("[AI檢測][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
				}
				return false;
			}
			_ = org.Width;
			_ = org.Height;
			ResizeOrg = org.Resize(AIImageSize);
			IntPtr ptr = PredictImageToImage(m_DeltaAIShow_B, ResizeOrg.CvPtr);
			MatPtrTemp = new Mat(ptr);
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
			CVBuffer.SaveLog("AT01-AIPredict-PredictMat()-01-" + ex.ToString());
			ResultText += string.Format("[AI前處理][{0}][{1}] AI前處理執行錯誤(PredictMat)，請檢查GPU!({2})。\n", H5Name, text, H5Name);
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
			int input_size = AIImageSize.Width * AIImageSize.Height * 3;
			int num = AIImageSize.Width * AIImageSize.Height * 3;
			ResizeOrg = org.Resize(AIImageSize);
			DestMatTemp = new Mat(AIImageSize, MatType.CV_32FC3);
			ResizeOrg.ConvertTo(DestMatTemp, MatType.CV_32FC3, 1.0 / 255.0);
			long num2 = DestMatTemp.Total() * DestMatTemp.Channels();
			float[] array = new float[num2];
			Marshal.Copy(DestMatTemp.Data, array, 0, Convert.ToInt32(num2));
			float[] array2 = new float[num];
			DoInference(trt_ptr, input_name, output_name, array, array2, input_size, num);
			DestMatTemp2 = new Mat(AIImageSize.Height, AIImageSize.Width, MatType.CV_32FC3, array2, 0L);
			DestMatTemp2.ConvertTo(MatPtrTemp, MatType.CV_8UC3, 255.0);
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
}
