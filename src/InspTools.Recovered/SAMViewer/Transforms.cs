using System;
using OpenCvSharp;

namespace SAMViewer;

internal class Transforms
{
	private int mTargetLength;

	public Transforms(int target_length)
	{
		mTargetLength = target_length;
	}

	public float[] ApplyImage(Mat image, int orgw, int orgh)
	{
		int neww = 0;
		int newh = 0;
		GetPreprocessShape(orgw, orgh, mTargetLength, ref neww, ref newh);
		Mat mat = new Mat();
		Cv2.Resize(image, mat, new Size(neww, newh));
		Mat mat2 = new Mat();
		mat.ConvertTo(mat2, MatType.CV_32FC3);
		Cv2.MeanStdDev(mat2, out var mean, out var stddev);
		Mat mat3 = new Mat();
		Cv2.Subtract(mat2, mean, mat3);
		Cv2.Divide(mat3, stddev, mat3);
		float[] array = new float[3 * mTargetLength * mTargetLength];
		for (int i = 0; i < neww; i++)
		{
			for (int j = 0; j < newh; j++)
			{
				int num = j * mTargetLength + i;
				array[num] = mat3.At<Vec3f>(j, i)[0];
				array[mTargetLength * mTargetLength + num] = mat3.At<Vec3f>(j, i)[1];
				array[2 * mTargetLength * mTargetLength + num] = mat3.At<Vec3f>(j, i)[2];
			}
		}
		mat.Dispose();
		mat2.Dispose();
		mat3.Dispose();
		return array;
	}

	public PointPromotion ApplyCoords(PointPromotion org_point, int orgw, int orgh)
	{
		int neww = 0;
		int newh = 0;
		GetPreprocessShape(orgw, orgh, mTargetLength, ref neww, ref newh);
		PointPromotion pointPromotion = new PointPromotion(org_point.m_Optype);
		float num = 1f * (float)neww / (float)orgw;
		float num2 = 1f * (float)newh / (float)orgh;
		pointPromotion.X = (int)((float)org_point.X * num);
		pointPromotion.Y = (int)((float)org_point.Y * num2);
		return pointPromotion;
	}

	public BoxPromotion ApplyBox(BoxPromotion org_box, int orgw, int orgh)
	{
		BoxPromotion boxPromotion = new BoxPromotion();
		PointPromotion mLeftUp = ApplyCoords(org_box.mLeftUp, orgw, orgh);
		PointPromotion mRightBottom = ApplyCoords(org_box.mRightBottom, orgw, orgh);
		boxPromotion.mLeftUp = mLeftUp;
		boxPromotion.mRightBottom = mRightBottom;
		return boxPromotion;
	}

	public void GetPreprocessShape(int oldw, int oldh, int long_side_length, ref int neww, ref int newh)
	{
		float num = (float)long_side_length * 1f / (float)Math.Max(oldh, oldw);
		float num2 = (float)oldh * num;
		float num3 = (float)oldw * num;
		neww = (int)((double)num3 + 0.5);
		newh = (int)((double)num2 + 0.5);
	}
}
