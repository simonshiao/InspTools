using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using OpenCvSharp;

namespace InspTools.AOI;

public class LocateAIEnhanceTool
{
	private IntPtr m_DeltaAIProcess;

	private IntPtr m_DeltaAIProcess_TRT;

	private OpenCvSharp.Size AIImageSize = new OpenCvSharp.Size(128, 128);

	private Mat ResizeOrg = new Mat();

	public Mat AIMatR = new Mat();

	public Mat AIMatG = new Mat();

	public Mat AIMatB = new Mat();

	public Mat DestMatColor = new Mat();

	public Mat AIDBMatR = new Mat();

	public Mat AIDBMatG = new Mat();

	public Mat AIDBMatB = new Mat();

	private Mat DestDBMatColor = new Mat();

	private Mat MatPtrTemp = new Mat();

	private Mat DestMatTemp = new Mat();

	private Mat DestMatTemp2 = new Mat();

	private Mat[] bgrmat = new Mat[3];

	private Mat bgr = new Mat();

	private Mat bgrResize = new Mat();

	private bool PbExist;

	public bool DllExist;

	public bool useCUDA = true;

	private string H5Name = "";

	private ParamFile param;

	public bool ModelLoaded;

	private bool ModelCreatInst;

	public string ResultText;

	public bool DLLMiss;

	public string DLLMissStr = "";

	public bool GPUExist = true;

	public string GPUErrorMSG = "";

	private CVBuffer buffers;

	private int _gpu;

	private string ScriptResult = "";

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

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern int ReadDnetTensorRT(IntPtr pShapeInstance, string modelname);

	[DllImport("DeltaAIProcessDll.dll")]
	public static extern IntPtr PredictDnetImageTensorRT(IntPtr pShapeInstance, IntPtr Image);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public LocateAIEnhanceTool(ParamFile paramfile)
	{
		useCUDA = true;
		ResizeOrg = new Mat(480, 640, MatType.CV_8UC3);
		param = paramfile;
		for (int i = 0; i < 3; i++)
		{
			bgrmat[i] = new Mat();
		}
		try
		{
			if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "DeltaAIProcessDll.dll"))
			{
				DllExist = true;
				ManagementObjectCollection instances = new ManagementClass("Win32_VideoController").GetInstances();
				string text = "";
				foreach (ManagementObject item in instances)
				{
					foreach (PropertyData property in item.Properties)
					{
						if (item[property.Name] != null && property.Name == "Name")
						{
							text = text + item[property.Name].ToString() + ", ";
						}
					}
				}
				if (text.Contains("RTX"))
				{
					if (!text.Contains("A4000") && !text.Contains("4000") && !text.Contains("3080") && !text.Contains("2080") && !text.Contains("4060") && !text.Contains("4070") && !text.Contains("4080"))
					{
						useCUDA = false;
						GPUExist = false;
						if (paramfile.LanguageType == 1)
						{
							GPUErrorMSG = string.Format("未找到GPU卡(僅支援RTX-4000, RTX-A4000, RTX 2080, RTX 3080, RTX 4060, RTX 4070, RTX 4080)，無法使用GPU做AI功能檢測。(找到顯卡:{0})", text);
						}
						else if (paramfile.LanguageType == 2)
						{
							GPUErrorMSG = string.Format("未找到GPU卡(仅支援RTX-4000, RTX-A4000, RTX 2080, RTX 3080, RTX 4060, RTX 4070, RTX 4080)，无法使用GPU做AI功能检测。(找到显卡:{0})", text);
						}
						else if (paramfile.LanguageType == 3)
						{
							GPUErrorMSG = string.Format("GPUカードが見つかりません (RTX-4000、RTX-A4000、RTX 2080、RTX 3080, 4060, RTX 4070, RTX 4080 のみサポート)。GPUを使用してAI機能を検出できません。(検出されたグラフィックカード:{0})", text);
						}
						else
						{
							GPUErrorMSG = string.Format("No GPU card found (only RTX-4000, RTX-A4000, RTX 2080, RTX 3080 ,RTX 4060, RTX 4070, RTX 4080 are supported), unable to use GPU for AI function detection. (Found graphics card: {0})", text);
						}
					}
				}
				else
				{
					useCUDA = false;
					GPUExist = false;
					GPUErrorMSG = string.Format("未找到GPU卡(僅支援RTX-4000, RTX-A4000, RTX 2080, RTX 3080, RTX 4060, RTX 4070, RTX 4080)，無法使用GPU做AI功能檢測。(找到顯卡:{0})", text);
				}
			}
			else
			{
				DllExist = false;
				useCUDA = false;
				GPUExist = false;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat(string.Concat("AT01-AIEnhanceTool-AIEnhanceTool(ParamFile paramfile)-Check GPU Card" + "未找到GPU卡(僅支援RTX-4000, RTX-A4000, RTX 2080, RTX 3080, RTX 4060, RTX 4070, RTX 4080)，無法使用GPU做AI功能檢測。", ex.ToString()), ScriptResult));
			GPUExist = false;
			useCUDA = false;
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
	public void CreatInst()
	{
		try
		{
			if (!ModelCreatInst)
			{
				m_DeltaAIProcess = CreateInstance();
				if (useCUDA)
				{
					int gpu_id = 0;
					SetGPU(m_DeltaAIProcess, gpu_id);
				}
				ModelCreatInst = true;
			}
		}
		catch (Exception)
		{
			MessageBox.Show("DeltaAIProcess載入錯誤，請檢查AI相關環境是否安裝正確。", "警告");
		}
	}

	public void ReAssignGPU(int GPUID)
	{
		if (ModelCreatInst)
		{
			if (GPUID < 0 || GPUID > 1)
			{
				GPUID = 0;
			}
			SetGPU(m_DeltaAIProcess, GPUID);
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
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] PB檔轉檔失敗，找不到{0}。\n", h5file);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] PB档转档失败，找不到{0}。\n", h5file);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] PBファイル変換失敗、{0}が見つかりません。\n", h5file);
				}
				else
				{
					ResultText += string.Format("[AI Locate] PB file conversion failed, cannot find {0}.\n", h5file);
				}
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
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] PB檔轉檔失敗，{0}模型尚未訓練。\n", Path.GetFileNameWithoutExtension(h5file));
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] PB档转档失败，{0}模型尚未训练。\n", Path.GetFileNameWithoutExtension(h5file));
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] PBファイル変換に失敗しました。{0}モデルはまだトレーニングされていません。\n", Path.GetFileNameWithoutExtension(h5file));
				}
				else
				{
					ResultText += string.Format("[AI Locate] PB file conversion failed, {0} model is not trained yet.\n", Path.GetFileNameWithoutExtension(h5file));
				}
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
					CVBuffer.SaveLog("AT01-LocateEnhanceTool-ConvertToPb()-" + ScriptResult);
				}
				if (!File.Exists(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "license.lic")))
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += (ResultText += string.Format("[AI定位] 認證失敗，找不到License認證檔案。\n"));
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += (ResultText += string.Format("[AI定位] 认证失败，找不到License认证档案。\n"));
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText += string.Format("[AI定位] 認証に失敗しました。ライセンス認証ファイルが見つかりません。\n");
					}
					else
					{
						ResultText += string.Format("[AI Locate] Authentication failed, no license authentication file found.\n");
					}
				}
				else if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] PB檔轉檔失敗，找不到Training\\Model\\navs.pb。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] PB档转档失败，找不到Training\\Model\\navs.pb。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] PBファイル変換失敗，Training\\Model\\navs.pbが見つかりません。\n");
				}
				else
				{
					ResultText += string.Format("[AI Locate] PB file conversion failed, Training\\Model\\navs.pb not found.\n");
				}
				return false;
			}
			if (buffers.LanguageType == 1)
			{
				ResultText += string.Format("[AI定位] 找不到轉換PB程式或轉換PB失敗(SavePb.bat)。\n");
			}
			else if (buffers.LanguageType == 2)
			{
				ResultText += string.Format("[AI定位] 找不到转换PB程式或转换PB失败(SavePb.bat)。\n");
			}
			else if (buffers.LanguageType == 3)
			{
				ResultText += string.Format("[AI定位] 変換PBプログラムが見つからないか、変換PBに失敗しました(SavePb.bat)。\n");
			}
			else
			{
				ResultText += string.Format("[AI定位 Preprocessing] Cannot find the conversion PB program or the conversion PB failed (SavePb.bat).\n");
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ConvertToPb()-" + ex.ToString());
			ModelLoaded = false;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool RunSaveTRTScript(ref string Result)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "SaveOnnx.bat");
		string path2 = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs.h5");
		string text = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs.onnx");
		string text2 = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs_tensorrt.onnx");
		string text3 = string.Format("{0}Training\\Model\\{1}", AppDomain.CurrentDomain.BaseDirectory, "navs.trt");
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
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			using (StreamWriter streamWriter = new StreamWriter(path, append: true))
			{
				streamWriter.Write("cd Training\ncd Model\n{0}navs.h5", "python h52intOnnx.py --h5-path ");
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
			string text4 = process.StandardOutput.ReadToEnd();
			ScriptResult = text4 + eOut;
			process.WaitForExit();
			process.Close();
			if (!File.Exists(text2))
			{
				return false;
			}
			File.Copy(text2, text, overwrite: true);
			File.Delete(text2);
		}
		catch (Exception ex)
		{
			string text5 = "AT01-AIEnhanceTool-RunSaveTRTScript()-H52Onnx-";
			CVBuffer.SaveLog(Result = text5 + ex.ToString());
			return false;
		}
		try
		{
			if (!File.Exists(text))
			{
				if (buffers.LanguageType == 1)
				{
					Result += string.Format("[AI定位] Onnx檔轉檔失敗，找不到{0}。\n", text);
				}
				else if (buffers.LanguageType == 2)
				{
					Result += string.Format("[AI定位] Onnx档转档失败，找不到{0}。\n", text);
				}
				else if (buffers.LanguageType == 3)
				{
					Result += string.Format("[AI定位] Onnxファイル変換失敗、{0}が見つかりません。\n", text);
				}
				else
				{
					Result += string.Format("[AI Locate] Onnx file conversion failed, cannot find {0}.\n", text);
				}
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
				_ = param.LocateParam;
				_ = param.LocateParam;
				string arg = string.Format("cd Training\ncd Model\npython onnx2engine.py --onnx-path navs.onnx --engine-path navs.trt");
				streamWriter2.Write("{0}", arg);
				streamWriter2.WriteLine();
				streamWriter2.Close();
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
			string text6 = process2.StandardOutput.ReadToEnd();
			ScriptResult = text6 + eOut2;
			process2.WaitForExit();
			process2.Close();
		}
		catch (Exception ex2)
		{
			CVBuffer.SaveLog(string.Concat("AT01-AIEnhanceTool-RunSaveTRTScript()-Onnx2Trt-" + ex2.ToString(), ScriptResult));
			return false;
		}
		if (!File.Exists(text3))
		{
			if (buffers.LanguageType == 1)
			{
				Result += string.Format("[AI定位] Trt檔轉檔失敗，找不到{0}。\n", text3);
			}
			else if (buffers.LanguageType == 2)
			{
				Result += string.Format("[AI定位] Trt档转档失败，找不到{0}。\n", text3);
			}
			else if (buffers.LanguageType == 3)
			{
				Result += string.Format("[AI定位] Trtファイル変換失敗、{0}が見つかりません。\n", text3);
			}
			else
			{
				Result += string.Format("[AI Locate] Trt file conversion failed, cannot find {0}.\n", text3);
			}
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
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] Trt檔轉檔失敗，找不到{0}。\n", h5file);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] Trt档转档失败，找不到{0}。\n", h5file);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] Trtファイル変換失敗、{0}が見つかりません。\n", h5file);
				}
				else
				{
					ResultText += string.Format("[AI Locate] Trt file conversion failed, cannot find {0}.\n", h5file);
				}
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
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] TRT檔轉檔失敗，{0}模型尚未訓練。\n", Path.GetFileNameWithoutExtension(h5file));
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] TRT档转档失败，{0}模型尚未训练。\n", Path.GetFileNameWithoutExtension(h5file));
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] TRTファイル変換に失敗しました。{0}モデルはまだトレーニングされていません。\n", Path.GetFileNameWithoutExtension(h5file));
				}
				else
				{
					ResultText += string.Format("[AI Locate] TRT file conversion failed, {0} model is not trained yet.\n", Path.GetFileNameWithoutExtension(h5file));
				}
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
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] TRT檔轉檔失敗，找不到Training\\Model\\navs.trt。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] TRT档转档失败，找不到Training\\Model\\navs.trt。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] TRTファイル変換失敗，Training\\Model\\navs.trt が見つかりません。\n");
				}
				else
				{
					ResultText += string.Format("[AI Locate] TRT file conversion failed, Training\\Model\\navs.trt not found.\n");
				}
				return false;
			}
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ConvertToTRT()-05-" + ex.ToString());
			ModelLoaded = false;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadModel(bool ReCreatePB = false)
	{
		bool flag = true;
		if (!ModelCreatInst)
		{
			CreatInst();
		}
		try
		{
			ModelLoaded = false;
			if (param.LocateParam.AILocateModelName == "")
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] 尚未設定AI定位模型。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] 尚未设定AI定位模型。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] まだAI位置合わせモデルが設定されていません。\n");
				}
				else
				{
					ResultText += string.Format("[AI Locate] AI Enhance model is not set yet.\n");
				}
				return false;
			}
			ResultText = "";
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Train\\train.exe");
			string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Model\\h5_to_pb.exe");
			if (!File.Exists(text) || !File.Exists(text2))
			{
				if (!File.Exists(text))
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位] 找不到訓練主程式 {0}。\n", text);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位] 找不到训练主程式 {0}。\n", text);
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText += string.Format("[AI定位] 訓練主程式 {0} を見つかりません。\n", text);
					}
					else
					{
						ResultText += string.Format("[AI Locate] Cannot find training program {0}.\n", text);
					}
				}
				if (!File.Exists(text2))
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位] 找不到PB轉換程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text2);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位] 找不到PB转换程式(Training\\Model\\h5_to_pb.exe) {0}。\n", text2);
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText += string.Format("[AI定位] PB変換プログラム(Training\\Model\\h5_to_pb.exe) {0}が見つかりません。\n", text2);
					}
					else
					{
						ResultText += string.Format("[AI定位 Preprocessing] Cannot find the PB conversion program (Training\\Model\\h5_to_pb.exe) {0}。\n", text2);
					}
				}
				return false;
			}
			string arg = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
			H5Name = Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName) + ".lh5d";
			string text3 = string.Format("{0}{1}", arg, H5Name);
			if (!File.Exists(text3))
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] 找不到模型檔案 {0}。\n", H5Name);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] 找不到模型档案 {0}。\n", H5Name);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] モデルファイル {0} が見つかりません。\n", H5Name);
				}
				else
				{
					ResultText += string.Format("[AI定位 Preprocessing] Model file {0} not found.\n", H5Name);
				}
				return false;
			}
			AIImageSize.Width = param.LocateParam.ModelWidth;
			AIImageSize.Height = param.LocateParam.ModelHeight;
			if (param.LocateParam.EnableTensorRT)
			{
				string arg2 = Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName) + ".ltrt";
				string text4 = string.Format("{0}{1}", arg, arg2);
				PbExist = File.Exists(text4);
				if (!PbExist && !ConvertToTRT(text3, text4))
				{
					flag = false;
					PbExist = false;
					ModelLoaded = false;
					return false;
				}
				PbExist = true;
				m_DeltaAIProcess_TRT = CreateInstance();
				if (CheckEncrypt(text4))
				{
					ReadDecryptTRTModel(m_DeltaAIProcess_TRT, text4);
				}
				else
				{
					ReadDnetTensorRT(m_DeltaAIProcess_TRT, text4);
				}
				ModelLoaded = true;
				return true;
			}
			string arg3 = Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName) + ".lpbd";
			string text5 = string.Format("{0}{1}", arg, arg3);
			PbExist = File.Exists(text5);
			if (!PbExist | ReCreatePB)
			{
				if (ConvertToPb(text3, text5))
				{
					PbExist = File.Exists(text5);
					if (PbExist)
					{
						if (CheckEncrypt(text5))
						{
							if (!ReadDecryptModel(m_DeltaAIProcess, text5))
							{
								ModelLoaded = false;
								if (File.Exists(text5))
								{
									File.Delete(text5);
								}
								return false;
							}
							ModelLoaded = true;
						}
						else
						{
							ReadModel(m_DeltaAIProcess, text5, useCUDA);
						}
						ModelLoaded = true;
						return true;
					}
					flag = false;
					PbExist = false;
					ModelLoaded = false;
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位] 找不到AI定位模型檔案{0}。\n", text5);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位] 找不到AI定位模型档案{0}。\n", text5);
					}
					else if (buffers.LanguageType == 3)
					{
						ResultText += string.Format("[AI定位] AI位置合わせモデルファイル {0} が見つかりません。\n", text5);
					}
					else
					{
						ResultText += string.Format("[AI Locate] Cannot find AI preprocess model file {0}.\n", text5);
					}
					return false;
				}
				flag = false;
				PbExist = false;
				ModelLoaded = false;
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位] AI定位模型檔案轉換失敗{0}。\n", text5);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位] AI定位模型档案转换失败{0}。\n", text5);
				}
				else if (buffers.LanguageType == 3)
				{
					ResultText += string.Format("[AI定位] AI位置合わせモデルファイル変換失敗{0}。\n", text5);
				}
				else
				{
					ResultText += string.Format("[AI定位 Preprocessing] AI Preprocessing model file conversion failed{0}。\n", text5);
				}
				return false;
			}
			if (CheckEncrypt(text5))
			{
				if (!ReadDecryptModel(m_DeltaAIProcess, text5))
				{
					ModelLoaded = false;
					if (File.Exists(text5))
					{
						File.Delete(text5);
					}
					return false;
				}
				ModelLoaded = true;
			}
			else
			{
				ReadModel(m_DeltaAIProcess, text5, useCUDA);
			}
			PbExist = true;
			ModelLoaded = true;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-ReadModel()-01-Model格式錯誤 " + ex.ToString());
			ModelLoaded = false;
			if (!flag)
			{
				ResultText = string.Format("[AI定位] 模型檔案轉換失敗。\n");
			}
			else
			{
				ResultText = string.Format("[AI定位] 模型檔案讀取失敗。\n");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ReadDecryptTRTModel(IntPtr m_DeltaAIProcess, string TrtFile)
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
			PbDecrypt(TrtFile, text);
			if (File.Exists(text))
			{
				ReadDnetTensorRT(m_DeltaAIProcess, text);
				File.Delete(text);
				return true;
			}
			if (buffers.LanguageType == 1)
			{
				ResultText = string.Format("[AI前處理] 找不到AI前處理模型檔案{0}。\n", TrtFile);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultText = string.Format("[AI前处理] 找不到AI前处理模型档案{0}。\n", TrtFile);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultText = string.Format("[AI前処理] AI前処理モデルファイル {0} が見つかりません。\n", TrtFile);
			}
			else
			{
				ResultText = string.Format("[AI Enhance] Cannot find AI preprocess model file {0}.\n", TrtFile);
			}
			File.Delete(text);
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ReadCryptModel()-05-" + ex.ToString());
			ResultText = string.Format("[AI前處理] 找不到AI前處理模型解碼失敗{0}。\n", TrtFile);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(Mat org, bool Offline)
	{
		if (!ModelLoaded && !ReadModel())
		{
			return false;
		}
		if (!ModelLoaded)
		{
			return false;
		}
		H5Name = param.LocateParam.AILocateModelName;
		string text = string.Format("{0}Model\\{1}.lh5d", AppDomain.CurrentDomain.BaseDirectory, H5Name);
		if (File.Exists(text))
		{
			if (new FileInfo(text).Length < 10)
			{
				return false;
			}
			if (org.Width <= 0 || org.Height <= 0)
			{
				return false;
			}
			if (param.LocateParam.ModelWidth <= 0 || param.LocateParam.ModelHeight <= 0)
			{
				return false;
			}
			ResultText = "";
			string arg = "GPU";
			if (!useCUDA)
			{
				arg = "CPU";
			}
			try
			{
				if (param.LocateParam.EnableTensorRT)
				{
					if (!PbExist)
					{
						if (buffers.LanguageType == 1)
						{
							ResultText = string.Format("[AI定位] 找不到AI模型檔案({0}.trt)。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText = string.Format("[AI定位] 找不到AI模型檔案({0}.trt)。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText = string.Format("[AI定位] AI モデル ファイルが見つかりません。({0}.trt)。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
						}
						else
						{
							ResultText = string.Format("[AI Locate Tool] Can't find AI Model.{0}.trt。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
						}
						return false;
					}
					if (!ModelLoaded)
					{
						if (!ModelLoaded)
						{
							if (buffers.LanguageType == 1)
							{
								ResultText += string.Format("[AI定位][{0}][{1}] 未載入AI模型({2})。\n", H5Name, arg, H5Name);
							}
							else if (buffers.LanguageType == 2)
							{
								ResultText += string.Format("[AI定位][{0}][{1}] 未载入AI模型({2})。\n", H5Name, arg, H5Name);
							}
							else if (buffers.LanguageType == 3)
							{
								ResultText += string.Format("[AI定位][{0}][{1}] AI モデルがロードされていません。({2})。\n", H5Name, arg, H5Name);
							}
							else
							{
								ResultText += string.Format("[AI Locate Tool][{0}][{1}] Can't Load AI Model({2})。\n", H5Name, arg, H5Name);
							}
						}
						return false;
					}
					UsageTime = 0.0;
					bool flag = true;
					flag = (param.LocateParam.EnableTensorRT ? PredictTrtMat(org, Offline) : PredictMat(org));
					if (flag)
					{
						buffers.SetORGLcateAIMat(AIMatR, AIMatG, AIMatB, param.LocateParam.MaxSearchRect);
					}
					return flag;
				}
				if (!PbExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText = string.Format("[AI定位] 找不到AI模型檔案({0}.pbd)。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位] 找不到AI模型檔案({0}.pbd)。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位] AI モデル ファイルが見つかりません。({0}.pbd)。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
					}
					else
					{
						ResultText = string.Format("[AI Locate Tool] Can't find AI Model.{0}.pbd。\n", Path.GetFileNameWithoutExtension(param.LocateParam.AILocateModelName));
					}
					return false;
				}
				if (!DllExist || !ModelLoaded || !ModelCreatInst)
				{
					if (!DllExist)
					{
						if (buffers.LanguageType == 1)
						{
							ResultText += string.Format("[AI定位][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText += string.Format("[AI定位][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText += string.Format("[AI定位][{0}][{1}] AI DLL が見つかりません。({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
						}
						else
						{
							ResultText += string.Format("[AI Locate Tool][{0}][{1}] Can't find AI DLL({2})。\n", H5Name, arg, "DeltaAIProcessDll.dll");
						}
					}
					if (!ModelLoaded)
					{
						if (buffers.LanguageType == 1)
						{
							ResultText += string.Format("[AI定位][{0}][{1}] 未載入AI模型({2})。\n", H5Name, arg, H5Name);
						}
						else if (buffers.LanguageType == 2)
						{
							ResultText += string.Format("[AI定位][{0}][{1}] 未载入AI模型({2})。\n", H5Name, arg, H5Name);
						}
						else if (buffers.LanguageType == 3)
						{
							ResultText += string.Format("[AI定位][{0}][{1}] AI モデルがロードされていません。({2})。\n", H5Name, arg, H5Name);
						}
						else
						{
							ResultText += string.Format("[AI Locate Tool][{0}][{1}] Can't Load AI Model({2})。\n", H5Name, arg, H5Name);
						}
					}
					return false;
				}
				UsageTime = 0.0;
				bool flag2 = true;
				flag2 = (param.LocateParam.EnableTensorRT ? PredictTrtMat(org, Offline) : PredictMat(org, Offline));
				if (flag2)
				{
					buffers.SetORGLcateAIMat(AIMatR, AIMatG, AIMatB, param.LocateParam.MaxSearchRect);
				}
				return flag2;
			}
			catch (Exception ex)
			{
				CVBuffer.SaveLog("Model檔案已存在" + ex.ToString());
				ResultText = string.Format("[AI定位] AI定位異常!(Exception)。\n");
				return false;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SendPredictTest()
	{
		int num = 0;
		try
		{
			if (DllExist && ModelLoaded && ModelCreatInst && AIImageSize.Width > 0 && AIImageSize.Height > 0)
			{
				Mat mat = new Mat(AIImageSize, MatType.CV_8UC3);
				_ = mat.Width;
				_ = mat.Height;
				ResizeOrg = mat.Resize(AIImageSize);
				num = 1;
				IntPtr intPtr = default(IntPtr);
				if (!param.LocateParam.EnableTensorRT)
				{
					intPtr = PredictImageToImage(m_DeltaAIProcess, ResizeOrg.CvPtr);
				}
				else
				{
					intPtr = PredictDnetImageTensorRT(m_DeltaAIProcess_TRT, ResizeOrg.CvPtr);
				}
				num = 2;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Format("AT01-AIEnhance-SendPredictTest()-01-DebugCode={0}\n", num) + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool PredictDBMat(Mat org)
	{
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		try
		{
			if (!PbExist || !DllExist || !ModelLoaded || !ModelCreatInst)
			{
				if (!PbExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] 找不到AI模型档案({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] AI モデル ファイルが見つかりません。({0})\n", H5Name, text, H5Name);
					}
					else
					{
						ResultText = string.Format("[AI Locate Tool][{0}][{1}] Can't find AI Model.{0}\n", H5Name, text, H5Name);
					}
				}
				if (!DllExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] AI DLL({2})が見つかりません。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					else
					{
						ResultText += string.Format("[AI Locate Tool][{0}][{1}] Can't find AI DLL.{0}\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
				}
				if (!ModelLoaded)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 未加载AI模型({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] AIモデル({2})が読み込まれていません。\n", H5Name, text, H5Name);
					}
					else
					{
						ResultText += string.Format("[AI定位 Preprocessing][{0}][{1}] AI model ({2}) is not loaded.\n", H5Name, text, H5Name);
					}
				}
				return false;
			}
			if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AIモデル画像サイズ設定が誤っています。(幅:{2}、高さ:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else
				{
					ResultText += string.Format("[AI Locate][{0}][{1}] AI model image size setting is incorrect. (Width:{2}, Height:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				return false;
			}
			_ = org.Width;
			_ = org.Height;
			ResizeOrg = org.Resize(AIImageSize);
			IntPtr intPtr = default(IntPtr);
			new Mat(data: param.LocateParam.EnableTensorRT ? PredictDnetImageTensorRT(m_DeltaAIProcess_TRT, ResizeOrg.CvPtr) : PredictImageToImage(m_DeltaAIProcess, ResizeOrg.CvPtr), rows: AIImageSize.Height, cols: AIImageSize.Width, type: MatType.CV_8UC3, step: 0L).CopyTo(MatPtrTemp);
			if (MatPtrTemp.Width <= 0 || MatPtrTemp.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI回傳影像尺寸為0，請檢查AI定位模型檔案。\n", H5Name, text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI返回图像尺寸为0，请检查AI预处理模型文件。\n", H5Name, text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AIは画像サイズが0を返しました。AI位置合わせモデルファイルを確認してください。\n", H5Name, text);
				}
				else
				{
					ResultText += string.Format("[AI Locate][{0}][{1}] AI returned image size of 0. Please check the AI preprocessing model file.\n", H5Name, text);
				}
				return false;
			}
			if (!param.LocateParam.EnableTensorRT)
			{
				Cv2.CvtColor(MatPtrTemp, bgr, ColorConversionCodes.BGR2RGB);
			}
			else
			{
				MatPtrTemp.CopyTo(bgr);
			}
			Cv2.Resize(bgr, bgrResize, new OpenCvSharp.Size(org.Width, org.Height));
			bgrResize.CopyTo(DestDBMatColor);
			Cv2.Split(bgr, out bgrmat);
			Cv2.Resize(bgrmat[2], AIDBMatR, new OpenCvSharp.Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[1], AIDBMatG, new OpenCvSharp.Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[0], AIDBMatB, new OpenCvSharp.Size(org.Width, org.Height));
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-PredictMat()-01-" + ex.ToString());
			ResultText += string.Format("[AI定位][{0}][{1}] AI定位執行錯誤(PredictMat)，請檢查GPU、或是模型檔!({2})。\n", H5Name, text, H5Name);
			return false;
		}
	}

	public Mat GetDestDBMatColor()
	{
		return DestDBMatColor;
	}

	public Mat GetDestMatColor()
	{
		return DestMatColor;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool PredictMat(Mat org, bool Offline = false)
	{
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		try
		{
			if (!PbExist || !DllExist || !ModelLoaded || !ModelCreatInst)
			{
				if (!PbExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] 找不到AI模型档案({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] AI モデル ファイルが見つかりません。({0})\n", H5Name, text, H5Name);
					}
					else
					{
						ResultText = string.Format("[AI Locate Tool][{0}][{1}] Can't find AI Model.{0}\n", H5Name, text, H5Name);
					}
				}
				if (!DllExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 找不到AI DLL({2})。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] AI DLL({2})が見つかりません。\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
					else
					{
						ResultText += string.Format("[AI Locate Tool][{0}][{1}] Can't find AI DLL.{0}\n", H5Name, text, "DeltaAIProcessDll.dll");
					}
				}
				if (!ModelLoaded)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 未加载AI模型({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] AIモデル({2})が読み込まれていません。\n", H5Name, text, H5Name);
					}
					else
					{
						ResultText += string.Format("[AI定位 Preprocessing][{0}][{1}] AI model ({2}) is not loaded.\n", H5Name, text, H5Name);
					}
				}
				return false;
			}
			if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AIモデル画像サイズ設定が誤っています。(幅:{2}、高さ:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else
				{
					ResultText += string.Format("[AI Locate][{0}][{1}] AI model image size setting is incorrect. (Width:{2}, Height:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				return false;
			}
			_ = org.Width;
			_ = org.Height;
			ResizeOrg = org.Resize(AIImageSize);
			IntPtr intPtr = default(IntPtr);
			intPtr = PredictImageToImage(m_DeltaAIProcess, ResizeOrg.CvPtr);
			Mat mat = new Mat(AIImageSize.Height, AIImageSize.Width, MatType.CV_8UC3, intPtr, 0L).Clone();
			if (mat.Width <= 0 && mat.Height <= 0)
			{
				return false;
			}
			mat.CopyTo(MatPtrTemp);
			if (MatPtrTemp.Width <= 0 || MatPtrTemp.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI回傳影像尺寸為0，請檢查AI定位模型檔案。\n", H5Name, text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI返回图像尺寸为0，请检查AI预处理模型文件。\n", H5Name, text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AIは画像サイズが0を返しました。AI位置合わせモデルファイルを確認してください。\n", H5Name, text);
				}
				else
				{
					ResultText += string.Format("[AI Locate][{0}][{1}] AI returned image size of 0. Please check the AI preprocessing model file.\n", H5Name, text);
				}
				return false;
			}
			Cv2.CvtColor(MatPtrTemp, bgr, ColorConversionCodes.BGR2RGB);
			if (Offline)
			{
				Cv2.Resize(bgr, bgrResize, new OpenCvSharp.Size(org.Width, org.Height));
				bgrResize.CopyTo(DestMatColor);
			}
			Cv2.Split(bgr, out bgrmat);
			Cv2.Resize(bgrmat[2], AIMatR, new OpenCvSharp.Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[1], AIMatG, new OpenCvSharp.Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[0], AIMatB, new OpenCvSharp.Size(org.Width, org.Height));
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-PredictMat()-01-" + ex.ToString());
			ResultText += string.Format("[AI定位][{0}][{1}] AI定位執行錯誤(PredictMat)，請檢查GPU、或是模型檔!({2})。\n", H5Name, text, H5Name);
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool PredictTrtMat(Mat org, bool Offline = false)
	{
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		try
		{
			if (!PbExist || !ModelLoaded)
			{
				if (!PbExist)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] 找不到AI模型檔案({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] 找不到AI模型档案({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText = string.Format("[AI定位][{0}][{1}] AI モデル ファイルが見つかりません。({0})\n", H5Name, text, H5Name);
					}
					else
					{
						ResultText = string.Format("[AI Locate Tool][{0}][{1}] Can't find AI Model.{0}\n", H5Name, text, H5Name);
					}
				}
				if (!ModelLoaded)
				{
					if (buffers.LanguageType == 1)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 未載入AI模型({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] 未加载AI模型({2})。\n", H5Name, text, H5Name);
					}
					else if (buffers.LanguageType == 2)
					{
						ResultText += string.Format("[AI定位][{0}][{1}] AIモデル({2})が読み込まれていません。\n", H5Name, text, H5Name);
					}
					else
					{
						ResultText += string.Format("[AI定位 Preprocessing][{0}][{1}] AI model ({2}) is not loaded.\n", H5Name, text, H5Name);
					}
				}
				return false;
			}
			if (AIImageSize.Width <= 0 || AIImageSize.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI模型影像尺寸設定錯誤。(寬:{2}、高:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AIモデル画像サイズ設定が誤っています。(幅:{2}、高さ:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				else
				{
					ResultText += string.Format("[AI Locate][{0}][{1}] AI model image size setting is incorrect. (Width:{2}, Height:{3})。\n", H5Name, text, AIImageSize.Width, AIImageSize.Height);
				}
				return false;
			}
			_ = org.Width;
			_ = org.Height;
			_ = AIImageSize;
			_ = AIImageSize;
			_ = AIImageSize;
			_ = AIImageSize;
			ResizeOrg = org.Resize(AIImageSize);
			IntPtr data = PredictDnetImageTensorRT(m_DeltaAIProcess_TRT, ResizeOrg.CvPtr);
			new Mat(AIImageSize.Height, AIImageSize.Width, MatType.CV_8UC3, data, 0L).CopyTo(MatPtrTemp);
			if (MatPtrTemp.Width <= 0 || MatPtrTemp.Height <= 0)
			{
				if (buffers.LanguageType == 1)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI回傳影像尺寸為0，請檢查AI定位模型檔案。\n", H5Name, text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AI返回图像尺寸为0，请检查AI预处理模型文件。\n", H5Name, text);
				}
				else if (buffers.LanguageType == 2)
				{
					ResultText += string.Format("[AI定位][{0}][{1}] AIは画像サイズが0を返しました。AI位置合わせモデルファイルを確認してください。\n", H5Name, text);
				}
				else
				{
					ResultText += string.Format("[AI Locate][{0}][{1}] AI returned image size of 0. Please check the AI preprocessing model file.\n", H5Name, text);
				}
				return false;
			}
			MatPtrTemp.CopyTo(bgr);
			if (Offline)
			{
				Cv2.Resize(bgr, bgrResize, new OpenCvSharp.Size(org.Width, org.Height));
				bgrResize.CopyTo(DestMatColor);
			}
			Cv2.Split(bgr, out bgrmat);
			Cv2.Resize(bgrmat[2], AIMatR, new OpenCvSharp.Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[1], AIMatG, new OpenCvSharp.Size(org.Width, org.Height));
			Cv2.Resize(bgrmat[0], AIMatB, new OpenCvSharp.Size(org.Width, org.Height));
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-PredictMat()-01-" + ex.ToString());
			ResultText += string.Format("[AI定位][{0}][{1}] AI定位執行錯誤(PredictMat)，請檢查GPU、或是模型檔!({2})。\n", H5Name, text, H5Name);
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
	public bool ReadDecryptModel(IntPtr m_DeltaAIProcess, string PbFile)
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
			PbDecrypt(PbFile, text);
			if (File.Exists(text))
			{
				ReadModel(m_DeltaAIProcess, text, useCUDA);
				File.Delete(text);
				return true;
			}
			if (buffers.LanguageType == 1)
			{
				ResultText = string.Format("[AI定位] 找不到AI定位模型檔案{0}。\n", PbFile);
			}
			else if (buffers.LanguageType == 2)
			{
				ResultText = string.Format("[AI定位] 找不到AI定位模型档案{0}。\n", PbFile);
			}
			else if (buffers.LanguageType == 3)
			{
				ResultText = string.Format("[AI定位] AI位置合わせモデルファイル {0} が見つかりません。\n", PbFile);
			}
			else
			{
				ResultText = string.Format("[AI Locate] Cannot find AI preprocess model file {0}.\n", PbFile);
			}
			File.Delete(text);
			return false;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhanceTool-ReadCryptModel()-05-" + ex.ToString());
			ResultText = string.Format("[AI定位] 找不到AI定位模型解碼失敗{0}。\n", PbFile);
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
}
