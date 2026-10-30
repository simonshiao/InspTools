using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace SAMViewer;

public class SAM
{
	public static SAM theSingleton;

	private InferenceSession mEncoder;

	private InferenceSession mDecoder;

	public float mask_threshold;

	private bool mReady;

	protected SAM()
	{
	}

	public static SAM Instance()
	{
		if (theSingleton == null)
		{
			theSingleton = new SAM();
		}
		return theSingleton;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LoadONNXModel(bool UseCuda = true)
	{
		if (mEncoder != null)
		{
			mEncoder.Dispose();
		}
		if (mDecoder != null)
		{
			mDecoder.Dispose();
		}
		SessionOptions sessionOptions = new SessionOptions();
		sessionOptions.EnableMemoryPattern = false;
		sessionOptions.EnableCpuMemArena = false;
		if (UseCuda)
		{
			sessionOptions.AppendExecutionProvider_CUDA();
		}
		Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		string text = string.Format("{0}{1}", "C:\\Temp\\", "encoder.onnx");
		if (File.Exists(text))
		{
			mEncoder = new InferenceSession(text, sessionOptions);
			string text2 = string.Format("{0}{1}", "C:\\Temp\\", "decoder.onnx");
			if (File.Exists(text2))
			{
				mDecoder = new InferenceSession(text2, sessionOptions);
				sessionOptions.Dispose();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public float[] Encode(Mat image, int orgWid, int orgHei)
	{
		DenseTensor<float> value = new DenseTensor<float>(new Transforms(1024).ApplyImage(image, orgWid, orgHei), new ReadOnlySpan<int>(global::_003CPrivateImplementationDetails_003E._19BF79AC3845FF1493B86C4781EB54889EA5BFAA1C33B458D5FBEB6A75F06A7C_A6 ?? (global::_003CPrivateImplementationDetails_003E._19BF79AC3845FF1493B86C4781EB54889EA5BFAA1C33B458D5FBEB6A75F06A7C_A6 = new int[] { 1, 3, 1024, 1024 })));
		List<NamedOnnxValue> inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("x", value) };
		float[] result = mEncoder.Run(inputs).First().AsTensor<float>()
			.ToArray();
		mReady = true;
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public MaskData Decode(List<Promotion> promotions, float[] embedding, int orgWid, int orgHei)
	{
		if (!mReady)
		{
			MessageBox.Show("Image Embedding is not done!");
			return null;
		}
		DenseTensor<float> value = new DenseTensor<float>(embedding, new ReadOnlySpan<int>(global::_003CPrivateImplementationDetails_003E._14266AAEE6150834ADB23B113163ADB0778134E325E1A63ED78A8C34D239D43E_A6 ?? (global::_003CPrivateImplementationDetails_003E._14266AAEE6150834ADB23B113163ADB0778134E325E1A63ED78A8C34D239D43E_A6 = new int[] { 1, 256, 64, 64 })));
		List<Promotion> list = promotions.FindAll((Promotion e) => e.mType == PromotionType.Box);
		List<Promotion> list2 = promotions.FindAll((Promotion e) => e.mType == PromotionType.Point);
		int num = promotions.FindAll((Promotion e) => e.mType == PromotionType.Box).Count();
		int num2 = promotions.FindAll((Promotion e) => e.mType == PromotionType.Point).Count();
		float[] array = new float[2 * (num * 2 + num2)];
		float[] array2 = new float[num * 2 + num2];
		for (int num3 = 0; num3 < num; num3++)
		{
			float[] input = list[num3].GetInput();
			for (int num4 = 0; num4 < input.Count(); num4++)
			{
				array[4 * num3 + num4] = input[num4];
			}
			float[] lable = list[num3].GetLable();
			for (int num5 = 0; num5 < lable.Count(); num5++)
			{
				array2[2 * num3 + num5] = lable[num5];
			}
		}
		for (int num6 = 0; num6 < num2; num6++)
		{
			float[] input2 = list2[num6].GetInput();
			for (int num7 = 0; num7 < input2.Count(); num7++)
			{
				array[num * 4 + 2 * num6 + num7] = input2[num7];
			}
			float[] lable2 = list2[num6].GetLable();
			for (int num8 = 0; num8 < lable2.Count(); num8++)
			{
				array2[num * 2 + num6 + num8] = lable2[num8];
			}
		}
		DenseTensor<float> value2 = new DenseTensor<float>(array, new int[3]
		{
			1,
			num * 2 + num2,
			2
		});
		DenseTensor<float> value3 = new DenseTensor<float>(array2, new int[2]
		{
			1,
			num * 2 + num2
		});
		float[] array3 = new float[65536];
		for (int num9 = 0; num9 < array3.Count(); num9++)
		{
			array3[num9] = 0f;
		}
		DenseTensor<float> value4 = new DenseTensor<float>(array3, new ReadOnlySpan<int>(global::_003CPrivateImplementationDetails_003E.F04CA24E9A0567EEA0CF60548D77CC0D6BBD462D8DDA902D0E0EB18BABCA3B16_A6 ?? (global::_003CPrivateImplementationDetails_003E.F04CA24E9A0567EEA0CF60548D77CC0D6BBD462D8DDA902D0E0EB18BABCA3B16_A6 = new int[] { 1, 1, 256, 256 })));
		DenseTensor<float> value5 = new DenseTensor<float>(new float[1], new ReadOnlySpan<int>(global::_003CPrivateImplementationDetails_003E._67ABDD721024F0FF4E0B3F4C2FC13BC5BAD42D0B7851D456D88D203D15AAA450_A6 ?? (global::_003CPrivateImplementationDetails_003E._67ABDD721024F0FF4E0B3F4C2FC13BC5BAD42D0B7851D456D88D203D15AAA450_A6 = new int[] { 1 })));
		DenseTensor<float> value6 = new DenseTensor<float>(new float[2] { orgHei, orgWid }, new ReadOnlySpan<int>(global::_003CPrivateImplementationDetails_003E._26B25D457597A7B0463F9620F666DD10AA2C4373A505967C7C8D70922A2D6ECE_A6 ?? (global::_003CPrivateImplementationDetails_003E._26B25D457597A7B0463F9620F666DD10AA2C4373A505967C7C8D70922A2D6ECE_A6 = new int[] { 2 })));
		List<NamedOnnxValue> inputs = new List<NamedOnnxValue>
		{
			NamedOnnxValue.CreateFromTensor("image_embeddings", value),
			NamedOnnxValue.CreateFromTensor("point_coords", value2),
			NamedOnnxValue.CreateFromTensor("point_labels", value3),
			NamedOnnxValue.CreateFromTensor("mask_input", value4),
			NamedOnnxValue.CreateFromTensor("has_mask_input", value5),
			NamedOnnxValue.CreateFromTensor("orig_im_size", value6)
		};
		MaskData maskData = new MaskData();
		List<DisposableNamedOnnxValue> list3 = mDecoder.Run(inputs).ToList();
		maskData.mMask = list3[0].AsTensor<float>().ToArray().ToList();
		maskData.mShape = list3[0].AsTensor<float>().Dimensions.ToArray();
		maskData.mIoU = list3[1].AsTensor<float>().ToList();
		return maskData;
	}
}
