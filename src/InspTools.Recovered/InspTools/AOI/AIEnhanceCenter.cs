using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using InspTools.Station;
using OpenCvSharp;

namespace InspTools.AOI;

public class AIEnhanceCenter
{
	private IntPtr trt_process;

	private bool TRTDllExist;

	private const int MaxQueueImg = 5;

	private IntPtr[] m_DeltaAIProcess = new IntPtr[8];

	private OpenCvSharp.Size[] m_AIImageSize = new OpenCvSharp.Size[8];

	private string[] m_ModelName = new string[8];

	private int[] m_ThreadID = new int[8];

	private bool[] ThreadRun = new bool[8];

	private int[] StationModelIndex = new int[8];

	private int ModelNum;

	private CVInspection[] Station;

	private Mat[] ResizeOrg = new Mat[8];

	public Mat[] DestMatColor = new Mat[8];

	private Mat MatPtrTemp = new Mat();

	private Mat DestMatTemp = new Mat();

	private Mat DestMatTemp2 = new Mat();

	private Mat[] bgr = new Mat[8];

	private Mat[] bgrResize = new Mat[8];

	private List<Mat>[] QImages = new List<Mat>[8];

	private List<int>[] QImagesCCDIndex = new List<int>[8];

	private Stopwatch[] sw = new Stopwatch[8];

	private bool[] PbExist = new bool[8];

	private bool DllExist;

	public bool useCUDA = true;

	private string H5Name = "";

	private ParamFile param;

	public bool ModelLoaded_A;

	public bool ModelLoaded_B;

	private bool ModelACreatInst;

	public string ResultText;

	public bool DLLMiss;

	public string DLLMissStr = "";

	private CVBuffer buffers;

	private int _gpu;

	private string ScriptResult = "";

	private bool Offline;

	private Thread[] AIPredictThread = new Thread[8];

	private ManualResetEvent[] AIPredict_PauseEvent = new ManualResetEvent[8];

	private ManualResetEvent[] UnderAIPredict_PauseEvent = new ManualResetEvent[8];

	private ManualResetEvent[] WaitAIPredict_PauseEvent = new ManualResetEvent[8];

	private Mat[] ResultMat = new Mat[8];

	private int MaxWaitms = 300;

	private Mat[] PredictImageTempMat = new Mat[8];

	private bool[] m_GPUSeted = new bool[8];

	private bool[] m_ModelPredictTest = new bool[8];

	private bool[] m_ModelRead = new bool[8];

	private IntPtr[] matPtr = new IntPtr[8];

	private Mat[] M1_ResultMat = new Mat[5];

	private Mat[] M1_BGR = new Mat[5];

	private Mat[] M1_BGRResize = new Mat[5];

	private Mat[] M1_DestMatColor = new Mat[5];

	private Mat[] ResizePredictMat = new Mat[8];

	private Mat[] PredictDstMat = new Mat[8];

	private Mat[] DstBGRMat = new Mat[8];

	private Mat[] DstBGRResizeMat = new Mat[8];

	private Mat[,] bgrMat = new Mat[8, 5];

	public Mat[] AIMatR = new Mat[8];

	public Mat[] AIMatG = new Mat[8];

	public Mat[] AIMatB = new Mat[8];

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
	public static extern IntPtr PredictBatchImage(IntPtr pShapeInstance, IntPtr[] imgArray, IntPtr[] outPtrs, int batch_size);

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
	public AIEnhanceCenter(ParamFile paramfile)
	{
		useCUDA = true;
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
					if (!text.Contains("A4000") && !text.Contains("4000") && !text.Contains("3080") && !text.Contains("2080") && !text.Contains("4060"))
					{
						useCUDA = false;
						MessageBox.Show(string.Format("未找到GPU卡(僅支援RTX-4000, RTX-A4000, RTX 2080, RTX 3080)，無法使用GPU做AI功能檢測。(找到顯卡:{0})", text), "警告");
					}
				}
				else
				{
					useCUDA = false;
					MessageBox.Show(string.Format("未找到GPU卡(僅支援RTX-4000, RTX-A4000, RTX 2080, RTX 3080)，無法使用GPU做AI功能檢測。(找到顯卡:{0})", text), "警告");
				}
			}
			else
			{
				DllExist = false;
			}
			if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "TRT_New.dll"))
			{
				TRTDllExist = true;
				trt_process = TRT_CreateInstance();
			}
			else
			{
				TRTDllExist = false;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog(string.Concat("AT01-AIEnhanceTool-AIEnhanceTool(ParamFile paramfile)-Check GPU Card" + ex.ToString(), ScriptResult));
			return;
		}
		param = paramfile;
		for (int i = 0; i < 8; i++)
		{
			m_DeltaAIProcess[i] = default(IntPtr);
			m_AIImageSize[i] = new OpenCvSharp.Size(256, 256);
			m_ModelName[i] = "";
			m_ThreadID[i] = 0;
			AIPredict_PauseEvent[i] = new ManualResetEvent(initialState: false);
			UnderAIPredict_PauseEvent[i] = new ManualResetEvent(initialState: false);
			WaitAIPredict_PauseEvent[i] = new ManualResetEvent(initialState: false);
			ResultMat[i] = new Mat();
			m_GPUSeted[i] = false;
			m_ModelPredictTest[i] = false;
			m_ModelRead[i] = false;
			StationModelIndex[i] = -1;
			ResizeOrg[i] = new Mat(480, 640, MatType.CV_8UC3);
			ResizeOrg[i].SetTo(255.0);
			bgr[i] = new Mat();
			bgrResize[i] = new Mat();
			DestMatColor[i] = new Mat();
			QImages[i] = new List<Mat>();
			sw[i] = new Stopwatch();
			AIMatR[i] = new Mat();
			AIMatG[i] = new Mat();
			AIMatB[i] = new Mat();
			ResizePredictMat[i] = new Mat();
			PredictDstMat[i] = new Mat();
			DstBGRMat[i] = new Mat();
			DstBGRResizeMat[i] = new Mat();
			for (int j = 0; j < 5; j++)
			{
				bgrMat[i, j] = new Mat();
			}
		}
	}

	public void ReleaseThread()
	{
		for (int i = 0; i < 8; i++)
		{
			ThreadRun[i] = false;
			AIPredict_PauseEvent[i].Set();
			UnderAIPredict_PauseEvent[i].Set();
			WaitAIPredict_PauseEvent[i].Set();
		}
	}

	public void SetStation(CVInspection[] station)
	{
		Station = station;
	}

	private void PredictThreadRun0(int gpu_id)
	{
		int num = 0;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			if (!m_ModelRead[num])
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun1(int gpu_id)
	{
		int num = 1;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun2(int gpu_id)
	{
		int num = 2;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun3(int gpu_id)
	{
		int num = 3;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun4(int gpu_id)
	{
		int num = 4;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun5(int gpu_id)
	{
		int num = 5;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun6(int gpu_id)
	{
		int num = 6;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	private void PredictThreadRun7(int gpu_id)
	{
		int num = 7;
		while (ThreadRun[num])
		{
			AIPredict_PauseEvent[num].WaitOne(-1);
			while (sw[num].ElapsedMilliseconds < MaxWaitms && QImages[num].Count <= 5)
			{
				Thread.Sleep(5);
			}
			UnderAIPredict_PauseEvent[num].Reset();
			if (!m_GPUSeted[num])
			{
				SetGPU(m_DeltaAIProcess[num], gpu_id);
				m_GPUSeted[num] = true;
			}
			int count = QImages[num].Count;
			if (count > 5)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			if (count <= num)
			{
				AIPredict_PauseEvent[num].Reset();
				WaitAIPredict_PauseEvent[num].Set();
				UnderAIPredict_PauseEvent[num].Set();
				continue;
			}
			IntPtr[] array = new IntPtr[count];
			IntPtr[] array2 = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				int num2 = QImagesCCDIndex[num][i];
				Cv2.Resize(QImages[num][i], ResizePredictMat[num2], m_AIImageSize[num]);
				array[i] = default(IntPtr);
				array2[i] = default(IntPtr);
				array2[i] = ResizePredictMat[num2].CvPtr;
			}
			PredictBatchImage(m_DeltaAIProcess[num], array2, array, count);
			Mat[] mv = new Mat[3];
			for (int j = 0; j < count; j++)
			{
				int num3 = QImagesCCDIndex[num][j];
				PredictDstMat[num3] = new Mat(array[j]);
				Cv2.CvtColor(PredictDstMat[num3], DstBGRMat[num3], ColorConversionCodes.BGR2RGB);
				if (Offline)
				{
					Cv2.Resize(PredictDstMat[num3], DstBGRResizeMat[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
					M1_BGRResize[j].CopyTo(DestMatColor[num3]);
				}
				mv[0] = bgrMat[j, 0];
				mv[1] = bgrMat[j, 1];
				mv[2] = bgrMat[j, 2];
				Cv2.Split(DstBGRMat[num3], out mv);
				Cv2.Resize(mv[2], AIMatR[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[1], AIMatG[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
				Cv2.Resize(mv[0], AIMatB[num3], new OpenCvSharp.Size(m_AIImageSize[num].Width, m_AIImageSize[num].Height));
			}
			QImages[num].Clear();
			QImagesCCDIndex[num].Clear();
			AIPredict_PauseEvent[num].Reset();
			WaitAIPredict_PauseEvent[num].Set();
			UnderAIPredict_PauseEvent[num].Set();
		}
	}

	public bool CCD1Inspect(Mat org, bool offline)
	{
		int num = 0;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD2Inspect(Mat org, bool offline)
	{
		int num = 1;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD3Inspect(Mat org, bool offline)
	{
		int num = 2;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD4Inspect(Mat org, bool offline)
	{
		int num = 3;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD5Inspect(Mat org, bool offline)
	{
		int num = 4;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD6Inspect(Mat org, bool offline)
	{
		int num = 5;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD7Inspect(Mat org, bool offline)
	{
		int num = 6;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public bool CCD8Inspect(Mat org, bool offline)
	{
		int num = 7;
		_ = org.Width;
		_ = org.Height;
		Offline = offline;
		int cCDModelIndex = GetCCDModelIndex(num);
		if (!m_ModelRead[cCDModelIndex])
		{
			m_ModelRead[cCDModelIndex] = ReadModel(m_ModelName[cCDModelIndex], m_AIImageSize[cCDModelIndex]);
		}
		if (!m_ModelRead[cCDModelIndex])
		{
			return false;
		}
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		_ = QImages[cCDModelIndex].Count;
		QImages[cCDModelIndex].Add(org);
		QImagesCCDIndex[cCDModelIndex].Add(num);
		AIPredict_PauseEvent[cCDModelIndex].Set();
		UnderAIPredict_PauseEvent[cCDModelIndex].WaitOne(-1);
		return true;
	}

	public int GetModelIndex(string modelname)
	{
		for (int i = 0; i < 8; i++)
		{
			if (!(m_ModelName[i] == "") && m_ModelName[i] == modelname)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetCCDModelIndex(int ccdindex)
	{
		return StationModelIndex[ccdindex];
	}

	public void UpdateModelCenter()
	{
		for (int i = 0; i < 8; i++)
		{
			m_ModelName[i] = "";
		}
		for (int j = 0; j < 8; j++)
		{
			if (!Station[j].paramFile.AIEnhanceToolON || !(Station[j].paramFile.AIEnhanceParam.ModelName_A != ""))
			{
				continue;
			}
			bool flag = false;
			for (int k = 0; k < ModelNum; k++)
			{
				if (Station[j].paramFile.AIEnhanceParam.ModelName_A == m_ModelName[k] && m_ModelName[k] != "")
				{
					flag = true;
					StationModelIndex[j] = k;
					m_AIImageSize[ModelNum] = new OpenCvSharp.Size(Station[j].paramFile.AIEnhanceParam.Width_A, Station[j].paramFile.AIEnhanceParam.Height_A);
					break;
				}
			}
			if (!flag)
			{
				m_DeltaAIProcess[ModelNum] = CreateInstance();
				m_ModelName[ModelNum] = Station[j].paramFile.AIEnhanceParam.ModelName_A;
				m_AIImageSize[ModelNum] = new OpenCvSharp.Size(Station[j].paramFile.AIEnhanceParam.Width_A, Station[j].paramFile.AIEnhanceParam.Height_A);
				m_ThreadID[ModelNum] = Station[j].paramFile.GetThreadID();
				StationModelIndex[j] = ModelNum;
				ModelNum++;
			}
		}
		int gPUID = 0;
		for (int l = 0; l < ModelNum; l++)
		{
			m_ModelPredictTest[l] = false;
			if (!(m_ModelName[l] != ""))
			{
				continue;
			}
			if (!m_GPUSeted[l])
			{
				SetGPU(m_DeltaAIProcess[l], 0);
				m_GPUSeted[l] = true;
			}
			m_ModelRead[l] = ReadModel(m_ModelName[l], m_AIImageSize[l]);
			if (m_ModelRead[l])
			{
				if (!ThreadRun[l])
				{
					ThreadRun[l] = true;
					RunThread(l, gPUID);
				}
				SendPredictTest(m_ModelName[l]);
				m_ModelPredictTest[l] = true;
			}
		}
		ModelACreatInst = true;
	}

	private void RunThread(int id, int GPUID)
	{
		switch (id)
		{
		case 0:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun0(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 1:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun1(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 2:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun2(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 3:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun3(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 4:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun4(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 5:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun5(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 6:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun6(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		case 7:
			AIPredictThread[id] = new Thread((ThreadStart)delegate
			{
				PredictThreadRun7(GPUID);
			});
			AIPredictThread[id].Start();
			break;
		}
		AIPredictThread[id].Start();
	}

	public void ReAssignGPU(int id, int GPUID)
	{
		if (ModelACreatInst)
		{
			if (GPUID < 0)
			{
				GPUID = 0;
			}
			m_GPUSeted[id] = false;
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
	public bool ReadModel(string ModelName, OpenCvSharp.Size ModelSize, bool ReCreatePB = false)
	{
		bool flag = true;
		try
		{
			ModelLoaded_A = false;
			if (ModelName == "")
			{
				ResultText = string.Format("[AI前處理] 尚未設定AI前處理模型。\n");
				return false;
			}
			int modelIndex = GetModelIndex(ModelName);
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
			string arg2 = Path.GetFileNameWithoutExtension(ModelName) + ".h5";
			string text3 = string.Format("{0}{1}", arg, arg2);
			H5Name = Path.GetFileNameWithoutExtension(ModelName) + ".h5d";
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
			string arg3 = Path.GetFileNameWithoutExtension(ModelName) + ".pbd";
			string text5 = string.Format("{0}{1}", arg, arg3);
			PbExist[modelIndex] = File.Exists(text5);
			if (!PbExist[modelIndex])
			{
				if (ConvertToPb(text4, text5))
				{
					PbExist[modelIndex] = File.Exists(text5);
					if (PbExist[modelIndex])
					{
						if (CheckEncrypt(text5))
						{
							if (!ReadDecryptModel(m_DeltaAIProcess[modelIndex], text5))
							{
								ModelLoaded_A = false;
								param.AIEnhanceParam.ModelExist_A = false;
								if (File.Exists(text5))
								{
									File.Delete(text5);
								}
								return false;
							}
							ModelLoaded_A = true;
							param.AIEnhanceParam.ModelExist_A = true;
						}
						else
						{
							ReadModel(m_DeltaAIProcess[modelIndex], text5, useCUDA);
						}
						ModelLoaded_A = true;
						param.AIEnhanceParam.ModelExist_A = true;
						return true;
					}
					flag = false;
					PbExist[modelIndex] = false;
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					ResultText += string.Format("[AI前處理] 找不到AI前處理模型檔案{0}。\n", H5Name);
					return false;
				}
				flag = false;
				PbExist[modelIndex] = false;
				ModelLoaded_A = false;
				param.AIEnhanceParam.ModelExist_A = false;
				ResultText += string.Format("[AI前處理] AI前處理模型檔案轉換失敗{0}。\n", text5);
				return false;
			}
			if (CheckEncrypt(text5))
			{
				if (!ReadDecryptModel(m_DeltaAIProcess[modelIndex], text5))
				{
					ModelLoaded_A = false;
					param.AIEnhanceParam.ModelExist_A = false;
					if (File.Exists(text5))
					{
						File.Delete(text5);
					}
					return false;
				}
				ModelLoaded_A = true;
				param.AIEnhanceParam.ModelExist_A = true;
			}
			else
			{
				ReadModel(m_DeltaAIProcess[modelIndex], text5, useCUDA);
			}
			PbExist[modelIndex] = true;
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
	public void SendPredictTest(string modelname)
	{
		try
		{
			int modelIndex = GetModelIndex(modelname);
			if (!m_ModelPredictTest[modelIndex])
			{
				OpenCvSharp.Size size = m_AIImageSize[modelIndex];
				if (DllExist && ModelACreatInst && size.Width > 0 && size.Height > 0)
				{
					Mat mat = new Mat(size, MatType.CV_8UC3);
					_ = mat.Width;
					_ = mat.Height;
					ResizeOrg[modelIndex] = mat.Resize(size);
					IntPtr ptr = PredictImageToImage(m_DeltaAIProcess[modelIndex], ResizeOrg[modelIndex].CvPtr);
					MatPtrTemp = new Mat(ptr);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AT01-AIEnhance-SendPredictTest()-01-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool PredictMat(Mat org, int index, OpenCvSharp.Size AIImageSize, bool Offline = false)
	{
		ResultText = "";
		string text = "GPU";
		if (!useCUDA)
		{
			text = "CPU";
		}
		try
		{
			if (!PbExist[index] || !DllExist || !ModelLoaded_A || !ModelACreatInst)
			{
				if (!PbExist[index])
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
			ResizeOrg[index] = org.Resize(AIImageSize);
			AIPredict_PauseEvent[index].Set();
			WaitAIPredict_PauseEvent[index].WaitOne(-1);
			WaitAIPredict_PauseEvent[index].Reset();
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
	public bool ReadDecryptModel(IntPtr m_DeltaAIProcess, string PbFile, int stationIndex = 0)
	{
		try
		{
			string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string text = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", 1);
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
	private bool CheckEncrypt(string _filename)
	{
		string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string text = string.Format("{0}{1}{2}.tmp", AppDomain.CurrentDomain.BaseDirectory, "Training\\Temp\\", 1);
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

	public void SetMaxWaitms(int maxms)
	{
		MaxWaitms = maxms;
	}

	public int GetMaxWaitms()
	{
		return MaxWaitms;
	}
}
