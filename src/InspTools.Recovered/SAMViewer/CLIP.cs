using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace SAMViewer;

internal class CLIP
{
	private InferenceSession mTxtEncoder;

	private InferenceSession mImgEncoder;

	public static CLIP theSingleton;

	private int contextLength = 77;

	protected CLIP()
	{
		new Thread((ThreadStart)delegate
		{
			LoadONNXModel();
		}).Start();
	}

	public static CLIP Instance()
	{
		if (theSingleton == null)
		{
			theSingleton = new CLIP();
		}
		return theSingleton;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LoadONNXModel()
	{
		if (mTxtEncoder != null)
		{
			mTxtEncoder.Dispose();
		}
		if (mImgEncoder != null)
		{
			mImgEncoder.Dispose();
		}
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		string text = directoryName + "\\textual.onnx";
		if (!File.Exists(text))
		{
			MessageBox.Show(text + " not exist!");
			return;
		}
		mTxtEncoder = new InferenceSession(text);
		string text2 = directoryName + "\\visual.onnx";
		if (!File.Exists(text2))
		{
			MessageBox.Show(text2 + " not exist!");
		}
		else
		{
			mImgEncoder = new InferenceSession(text2);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<float> TxtEncoder(string txt)
	{
		DenseTensor<long> value = new DenseTensor<long>(SimpleTokenizer.Instance().tolikenlize(txt).ToArray(), new int[2] { 1, contextLength });
		List<NamedOnnxValue> inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", value) };
		return mTxtEncoder.Run(inputs).First().AsTensor<float>()
			.ToArray()
			.ToList();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<float> ImgEncoder(float[] img)
	{
		DenseTensor<float> value = new DenseTensor<float>(img, new ReadOnlySpan<int>(global::_003CPrivateImplementationDetails_003E._84293761A8E206B6F9D173D5D3FFF4D07DB900259751A57C60FC31BA4C51EB59_A6 ?? (global::_003CPrivateImplementationDetails_003E._84293761A8E206B6F9D173D5D3FFF4D07DB900259751A57C60FC31BA4C51EB59_A6 = new int[] { 1, 3, 224, 224 })));
		List<NamedOnnxValue> inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", value) };
		return mImgEncoder.Run(inputs).First().AsTensor<float>()
			.ToArray()
			.ToList();
	}
}
