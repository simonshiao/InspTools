using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using OpenCvSharp;

namespace InspTools;

internal class MM
{
	private static bool bPrintToConsole = false;

	public static int AllocCallCnt = 0;

	public static int AllocActualCnt = 0;

	private static Mutex mut_mm = new Mutex(initiallyOwned: false, "MemoryManager");

	public static List<Queue<Mat>> IngQueueList = new List<Queue<Mat>>();

	public static Dictionary<string, int> ImgQueueIdxBySize = new Dictionary<string, int>();

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static int FindQueueIndex(int rows, int cols, MatType typ)
	{
		string key = string.Format("{0}_{1}_{2}", rows, cols, typ);
		int num = -1;
		int value = -1;
		if (ImgQueueIdxBySize.TryGetValue(key, out value))
		{
			num = value;
		}
		else
		{
			IngQueueList.Add(new Queue<Mat>());
			num = IngQueueList.Count - 1;
			ImgQueueIdxBySize.Add(key, num);
		}
		return num;
	}

	public static Mat Alloc(int rows, int cols, MatType typ)
	{
		mut_mm.WaitOne();
		Mat result = null;
		try
		{
			AllocCallCnt++;
			int num = FindQueueIndex(rows, cols, typ);
			if (num >= 0)
			{
				Queue<Mat> queue = IngQueueList[num];
				if (queue.Count == 0)
				{
					result = new Mat(rows, cols, typ);
					AllocActualCnt++;
				}
				else
				{
					result = queue.Dequeue();
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		mut_mm.ReleaseMutex();
		return result;
	}

	public static void GiveBack(Mat img)
	{
		if (img != null && img.Width != 0 && img.Height != 0)
		{
			mut_mm.WaitOne();
			int num = FindQueueIndex(img.Rows, img.Cols, img.Type());
			if (num >= 0)
			{
				IngQueueList[num].Enqueue(img);
			}
			mut_mm.ReleaseMutex();
		}
	}

	public static Mat CloneFrom(Mat org, Mat TryGiveBackImg = null)
	{
		if (TryGiveBackImg != null)
		{
			GiveBack(TryGiveBackImg);
		}
		if (org == null)
		{
			org = new Mat();
			return org;
		}
		Mat mat = Alloc(org.Rows, org.Cols, org.Type());
		org.CopyTo(mat);
		return mat;
	}

	public static MyMat newMat(Mat org, Rect rect)
	{
		return new MyMat(org, rect);
	}

	public static Mat new_Mat(int Rows, int Cols, MatType Typ)
	{
		return Alloc(Rows, Cols, Typ);
	}
}
