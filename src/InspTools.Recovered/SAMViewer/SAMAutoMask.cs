using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using OpenCvSharp;

namespace SAMViewer;

internal class SAMAutoMask
{
	public int points_per_side = 4;

	private int points_per_batch = 64;

	public float pred_iou_thresh = 0.88f;

	public float stability_score_thresh = 0.95f;

	private float stability_score_offset = 1f;

	private float box_nms_thresh = 0.7f;

	private int crop_n_layers;

	private float crop_nms_thresh = 0.7f;

	private float crop_overlap_ratio = 0.34133333f;

	private int crop_n_points_downscale_factor = 1;

	private List<double[,]> point_grids;

	private int min_mask_region_area;

	private string output_mode = "binary_mask";

	private Mat mImage;

	public SAM mSAM;

	public float[] mImgEmbedding;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public SAMAutoMask(int points_per_side = 4, int points_per_batch = 64, float pred_iou_thresh = 0.88f, float stability_score_thresh = 0.95f, float stability_score_offset = 1f, float box_nms_thresh = 0.7f, int crop_n_layers = 0, float crop_nms_thresh = 0.7f, float crop_overlap_ratio = 0.34133333f, int crop_n_points_downscale_factor = 1, List<double[,]> point_grids = null, int min_mask_region_area = 0, string output_mode = "binary_mask")
	{
		this.points_per_side = points_per_side;
		this.points_per_batch = points_per_batch;
		this.pred_iou_thresh = pred_iou_thresh;
		this.stability_score_thresh = stability_score_thresh;
		this.stability_score_offset = stability_score_offset;
		this.box_nms_thresh = box_nms_thresh;
		this.crop_n_layers = crop_n_layers;
		this.crop_nms_thresh = crop_nms_thresh;
		this.crop_overlap_ratio = crop_overlap_ratio;
		this.crop_n_points_downscale_factor = crop_n_points_downscale_factor;
		this.point_grids = point_grids;
		this.min_mask_region_area = min_mask_region_area;
		this.output_mode = output_mode;
		if (points_per_side == 0 && (point_grids == null || point_grids.Count == 0))
		{
			MessageBox.Show("Exactly one of points_per_side or point_grid must be provided.");
		}
		else if (points_per_side != 0)
		{
			this.point_grids = build_all_layer_point_grids(points_per_side, crop_n_layers, crop_n_points_downscale_factor);
		}
	}

	private List<double[,]> build_all_layer_point_grids(int n_per_side, int n_layers, int scale_per_layer)
	{
		List<double[,]> list = new List<double[,]>();
		for (int i = 0; i <= n_layers; i++)
		{
			int n_per_side2 = (int)((double)n_per_side / Math.Pow(scale_per_layer, i));
			list.Add(BuildPointGrid(n_per_side2));
		}
		return list;
	}

	private double[,] BuildPointGrid(int n_per_side)
	{
		double offset = 1.0 / (double)(2 * n_per_side);
		double[] array = (from i in Enumerable.Range(0, n_per_side)
			select offset + (double)i * (1.0 - 2.0 * offset) / (double)(n_per_side - 1)).ToArray();
		double[,] array2 = new double[n_per_side * n_per_side, 2];
		for (int num = 0; num < n_per_side; num++)
		{
			for (int num2 = 0; num2 < n_per_side; num2++)
			{
				int num3 = num * n_per_side + num2;
				array2[num3, 0] = array[num];
				array2[num3, 1] = array[num2];
			}
		}
		return array2;
	}

	private void generateCropBoxes(int orgWid, int orgHei, int n_layers, float overlap_ratio, ref List<List<int>> crop_boxes, ref List<int> layer_idxs)
	{
		int num = Math.Min(orgHei, orgWid);
		crop_boxes.Add(new List<int> { 0, 0, orgWid, orgHei });
		layer_idxs.Add(0);
		for (int i = 0; i < n_layers; i++)
		{
			int num2 = (int)Math.Pow(2.0, i + 1);
			int num3 = (int)(overlap_ratio * (float)num * (float)(2 / num2));
			int num4 = crop_len(orgWid, num2, num3);
			int num5 = crop_len(orgHei, num2, num3);
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			for (int j = 0; j < num2; j++)
			{
				list.Add((num4 - num3) * j);
				list2.Add((num5 - num3) * j);
			}
			foreach (int item2 in list)
			{
				foreach (int item3 in list2)
				{
					List<int> item = new List<int>
					{
						item2,
						item3,
						Math.Min(item2 + num4, orgWid),
						Math.Min(item3 + num5, orgHei)
					};
					crop_boxes.Add(item);
					layer_idxs.Add(i + 1);
				}
			}
		}
	}

	private int crop_len(int orig_len, int n_crops, int overlap)
	{
		return (int)Math.Ceiling((double)(overlap * (n_crops - 1) + orig_len) / (double)n_crops);
	}

	private IEnumerable<List<object>> BatchIterator(int batch_size, params object[] args)
	{
		int n_batches = ((Array)args[0]).Length / batch_size + ((((Array)args[0]).Length % batch_size != 0) ? 1 : 0);
		for (int b = 0; b < n_batches; b++)
		{
			List<object> list = new List<object>();
			for (int i = 0; i < args.Length; i++)
			{
				Array array = (Array)args[i];
				int num = b * batch_size;
				int num2 = (b + 1) * batch_size;
				if (num2 > array.Length)
				{
					num2 = array.Length;
				}
				Array array2 = Array.CreateInstance(array.GetType().GetElementType(), num2 - num);
				Array.Copy(array, num, array2, 0, num2 - num);
				list.Add(array2);
			}
			yield return list;
		}
	}

	public MaskData Generate(string imgfile)
	{
		if (mImage != null)
		{
			mImage.Dispose();
		}
		mImage = Cv2.ImRead(imgfile);
		if (points_per_side != 0)
		{
			point_grids = build_all_layer_point_grids(points_per_side, crop_n_layers, crop_n_points_downscale_factor);
		}
		return _generate_masks(mImage);
	}

	private MaskData _generate_masks(Mat img)
	{
		MaskData maskData = new MaskData();
		List<List<int>> crop_boxes = new List<List<int>>();
		List<int> layer_idxs = new List<int>();
		generateCropBoxes(img.Cols, img.Rows, crop_n_layers, crop_overlap_ratio, ref crop_boxes, ref layer_idxs);
		for (int i = 0; i < crop_boxes.Count; i++)
		{
			MaskData md = _process_crop(crop_boxes[i], layer_idxs[i]);
			maskData.Cat(md);
		}
		return maskData;
	}

	private MaskData _process_crop(List<int> crop_box, int crop_layer_idx)
	{
		MaskData maskData = new MaskData();
		int x = crop_box[0];
		int y = crop_box[1];
		int width = crop_box[2];
		int height = crop_box[3];
		Mat mat = new Mat(roi: new OpenCvSharp.Rect(x, y, width, height), m: mImage);
		double[,] array = point_grids[crop_layer_idx];
		PointPromotion[] array2 = new PointPromotion[array.GetLength(0)];
		Transforms transforms = new Transforms(1024);
		for (int i = 0; i < array.GetLength(0); i++)
		{
			Promotion promotion = new PointPromotion(OpType.ADD);
			(promotion as PointPromotion).X = (int)(array[i, 0] * (double)mat.Cols);
			(promotion as PointPromotion).Y = (int)(array[i, 1] * (double)mat.Rows);
			PointPromotion pointPromotion = transforms.ApplyCoords(promotion as PointPromotion, mat.Cols, mat.Rows);
			array2[i] = pointPromotion;
		}
		object[] args = new object[1] { array2 };
		foreach (List<object> item in BatchIterator(points_per_batch, args))
		{
			MaskData md = _process_batch(item.ToList(), mat.Cols, mat.Rows, crop_box, mImage.Cols, mImage.Rows);
			maskData.Cat(md);
		}
		mat.Dispose();
		return maskData;
	}

	private MaskData _process_batch(List<object> points, int cropimgwid, int cropimghei, List<int> cropbox, int orgimgwid, int orgimghei)
	{
		MaskData maskData = new MaskData();
		new List<float[]>();
		foreach (object point in points)
		{
			PointPromotion[] array = point as PointPromotion[];
			for (int i = 0; i < array.Length; i++)
			{
				MaskData maskData2 = mSAM.Decode(new List<Promotion> { array[i] }, mImgEmbedding, cropimgwid, cropimghei);
				maskData2.mStalibility = maskData2.CalculateStabilityScore(mSAM.mask_threshold, stability_score_offset).ToList();
				maskData2.Filter(pred_iou_thresh, stability_score_thresh);
				maskData.Cat(maskData2);
			}
		}
		maskData.mBox = maskData.batched_mask_to_box().ToList();
		return maskData;
	}
}
