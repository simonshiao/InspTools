using System;
using System.Collections.Generic;

namespace SAMViewer;

public class MaskData
{
	public int[] mShape;

	public List<float> mMask;

	public List<float> mIoU;

	public List<float> mStalibility;

	public List<int> mBox;

	public List<List<float>> mfinalMask;

	public MaskData()
	{
		mShape = new int[4];
		mMask = new List<float>();
		mIoU = new List<float>();
		mStalibility = new List<float>();
		mBox = new List<int>();
		mfinalMask = new List<List<float>>();
	}

	public void Filter(float pred_iou_thresh, float stability_score_thresh)
	{
		new List<float>();
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		int num = 0;
		for (int i = 0; i < mShape[1]; i++)
		{
			if (mIoU[i] > pred_iou_thresh && mStalibility[i] > stability_score_thresh)
			{
				mfinalMask.Add(mMask.GetRange(i * mShape[2] * mShape[3], mShape[2] * mShape[3]));
				list.Add(mIoU[i]);
				list2.Add(mStalibility[i]);
				num++;
			}
		}
		mShape[1] = num;
		mStalibility.Clear();
		mMask.Clear();
		mIoU.Clear();
		mIoU.AddRange(list);
		mStalibility.AddRange(list2);
	}

	public float[] CalculateStabilityScore(float maskThreshold, float thresholdOffset)
	{
		int num = mShape[1];
		int num2 = mShape[3];
		int num3 = mShape[2];
		float[] array = new float[num];
		float[] array2 = new float[num];
		for (int i = 0; i < num; i++)
		{
			float num4 = 0f;
			float num5 = 0f;
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					int index = i * num2 * num3 + k * num2 + j;
					if (mMask[index] > maskThreshold + thresholdOffset)
					{
						num4++;
					}
					if (mMask[index] > maskThreshold - thresholdOffset)
					{
						num5++;
					}
				}
			}
			array[i] = num4;
			array2[i] = num5;
		}
		float[] array3 = new float[num];
		for (int l = 0; l < num; l++)
		{
			array3[l] = array[l] / array2[l];
		}
		return array3;
	}

	public void Cat(MaskData md)
	{
		mShape[0] = md.mShape[0];
		mShape[1] += md.mShape[1];
		mShape[2] = md.mShape[2];
		mShape[3] = md.mShape[3];
		mBox.AddRange(md.mBox);
		mMask.AddRange(md.mMask);
		mStalibility.AddRange(md.mStalibility);
		mIoU.AddRange(md.mIoU);
		mfinalMask.AddRange(md.mfinalMask);
	}

	public int[] batched_mask_to_box()
	{
		int num = mShape[1];
		int num2 = mShape[3];
		int num3 = mShape[2];
		int[] array = new int[num * 4];
		for (int i = 0; i < num; i++)
		{
			bool flag = true;
			int num4 = num3;
			int num5 = 0;
			int num6 = num2;
			int num7 = 0;
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					int index = k * num2 + j;
					if (mfinalMask[i][index] > 0f)
					{
						flag = false;
						num4 = Math.Min(num4, k);
						num5 = Math.Max(num5, k);
						num6 = Math.Min(num6, j);
						num7 = Math.Max(num7, j);
					}
				}
			}
			if (flag)
			{
				array[i * 4] = 0;
				array[i * 4 + 1] = 0;
				array[i * 4 + 2] = 0;
				array[i * 4 + 3] = 0;
			}
			else
			{
				array[i * 4] = num6;
				array[i * 4 + 1] = num4;
				array[i * 4 + 2] = num7;
				array[i * 4 + 3] = num5;
			}
		}
		return array;
	}
}
