// Strictly offline 1-epoch ONNX model contract smoke; not production AI.
// Compiled separately with the pre-existing ShuntVC OnnxRuntime assembly.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public static class ChaoProphetOnnxSmoke
{
    public static string Run(string modelPath, string sourceImage, string outputPng)
    {
        if (!File.Exists(modelPath) || !File.Exists(sourceImage))
            throw new FileNotFoundException("Missing ONNX or real source image");
        byte[] file = File.ReadAllBytes(modelPath);
        if (file.Length < 16 || file[0] != 8 || file[1] != 7)
            throw new InvalidDataException("Model must be unwrapped ONNX protobuf");

        var input = new DenseTensor<byte>(new int[] { 1, 256, 256, 3 });
        using (var original = new Bitmap(sourceImage))
        using (var scaled = new Bitmap(256, 256))
        {
            using (var graphics = Graphics.FromImage(scaled))
                graphics.DrawImage(original, 0, 0, 256, 256);
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                Color color = scaled.GetPixel(x, y);
                // Preliminary RGB uint8 normalization-free contract smoke.
                // Must confirm training-side BGR/RGB and resize strategy
                // before comparing anomaly quality or integrating Inspect.
                input[0, y, x, 0] = color.R;
                input[0, y, x, 1] = color.G;
                input[0, y, x, 2] = color.B;
            }
        }
        using (var session = new InferenceSession(modelPath))
        using (var result = session.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor<byte>("input", input)
        }))
        {
            Tensor<byte> mask = null;
            byte score2 = 0, score3 = 0;
            foreach (var item in result)
            {
                if (item.Name == "output1") mask = item.AsTensor<byte>();
                else if (item.Name == "output2") score2 = item.AsTensor<byte>()[0];
                else if (item.Name == "output3") score3 = item.AsTensor<byte>()[0];
            }
            if (mask == null || mask.Rank != 3 || mask.Length != 256 * 256)
                throw new InvalidOperationException("Unexpected anomaly-mask output");
            int nonZero = 0;
            byte min = byte.MaxValue, max = byte.MinValue;
            using (var output = new Bitmap(256, 256))
            {
                for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                {
                    byte value = mask[0, y, x];
                    if (value > 0) nonZero++;
                    if (value < min) min = value;
                    if (value > max) max = value;
                    output.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
                Directory.CreateDirectory(Path.GetDirectoryName(outputPng));
                output.Save(outputPng, ImageFormat.Png);
            }
            return "INFERENCE_SMOKE=PASS" + Environment.NewLine +
                "SOURCE_IMAGE=" + sourceImage + Environment.NewLine +
                "MASK_PNG=" + outputPng + Environment.NewLine +
                "MASK_NONZERO=" + nonZero + Environment.NewLine +
                "MASK_MIN=" + min + " MASK_MAX=" + max + Environment.NewLine +
                "RAW_OUTPUT2=" + score2 + " RAW_OUTPUT3=" + score3 +
                Environment.NewLine + "QUALITY_GATE=NOT_TESTED" +
                Environment.NewLine + "MODEL_PROVENANCE=real-e1, one epoch; NOT DEPLOYABLE";
        }
    }
}
