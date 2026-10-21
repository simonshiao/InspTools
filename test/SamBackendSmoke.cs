using System;
using System.IO;
using System.Linq;
using InspTools.SDK;
using InspTools.SamOnnx;

internal static class SamBackendSmoke
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3)
                throw new ArgumentException("Usage: SamBackendSmoke.exe <TrainingImageRoot> <encoder.onnx> <decoder.onnx>");

            string root = Path.GetFullPath(args[0]);
            string encoder = Path.GetFullPath(args[1]);
            string decoder = Path.GetFullPath(args[2]);
            string model = "R002-CCD1";
            string name = "NG-1-2025-06-27-16-55-12-H.png";
            string x = Path.Combine(root, model, "X", "NG", "ALL", name);
            string y = Path.Combine(root, model, "Y", "NG", "ALL", name);
            RequireFile(x);
            RequireFile(y);
            RequireFile(encoder);
            RequireFile(decoder);

            using (var backend = new SamOnnxSuggestionBackend(new SamOnnxOptions
            {
                EncoderModelPath = encoder,
                DecoderModelPath = decoder,
                PreferCuda = false,
                MaskSelection = SamMaskSelection.FirstMaskLegacy
            }))
            {
                Require(backend.IsReady, "SAM backend is not ready: " + backend.State);
                Require(backend.ProviderUsed == "CPU", "CPU smoke unexpectedly used: " + backend.ProviderUsed);

                using (IAnnotationSession session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
                {
                    TrainingImageRoot = root,
                    ModelName = model,
                    Kind = AnnotationSampleKind.Ng,
                    ImagePath = x,
                    LabelPath = y,
                    Editable = true,
                    SuggestionBackend = backend
                }))
                {
                    Require(session.SamReady, "Session does not report SAM ready: " + session.SamState);
                    var prompts = new[]
                    {
                        new AnnotationPrompt
                        {
                            Type = AnnotationPromptType.Box,
                            X = 10,
                            Y = 16,
                            X2 = 151,
                            Y2 = 684
                        }
                    };

                    AnnotationSuggestion first = session.Suggest(prompts);
                    Require(first.Success, "First SAM suggestion failed: " + first.Message);
                    int positive = first.Mask.Count(v => v > 0f);
                    Require(Math.Abs(positive - 11961) <= 300,
                        "Legacy first-mask positive count diverged from Python reference. actual=" + positive);
                    Require(Math.Abs(first.Score - 0.748689591884613) <= 0.02,
                        "Legacy first-mask IoU score diverged from Python reference. actual=" + first.Score);
                    Require(backend.EncodeCount == 1 && backend.DecodeCount == 1,
                        "Unexpected first inference lifecycle. encode=" + backend.EncodeCount + " decode=" + backend.DecodeCount);

                    AnnotationSuggestion second = session.Suggest(prompts);
                    Require(second.Success, "Second SAM suggestion failed: " + second.Message);
                    Require(backend.EncodeCount == 1 && backend.DecodeCount == 2,
                        "Embedding cache failed. encode=" + backend.EncodeCount + " decode=" + backend.DecodeCount);

                    AnnotationSuggestion point = session.Suggest(new[]
                    {
                        new AnnotationPrompt { Type = AnnotationPromptType.PositivePoint, X = 80, Y = 350 }
                    });
                    Require(point.Success && point.Mask.Length == session.Image.Width * session.Image.Height,
                        "Positive-point SAM suggestion failed.");
                    AnnotationSuggestion pointWithNegative = session.Suggest(new[]
                    {
                        new AnnotationPrompt { Type = AnnotationPromptType.PositivePoint, X = 80, Y = 350 },
                        new AnnotationPrompt { Type = AnnotationPromptType.NegativePoint, X = 300, Y = 350 }
                    });
                    Require(pointWithNegative.Success && pointWithNegative.Mask.Length == session.Image.Width * session.Image.Height,
                        "Positive+negative SAM suggestion failed.");
                    Require(backend.EncodeCount == 1 && backend.DecodeCount == 4,
                        "Point prompt path broke embedding cache. encode=" + backend.EncodeCount + " decode=" + backend.DecodeCount);

                    byte[] before = session.GetLegacyLabelImage().Rgb24;
                    int beforeClasses = session.GetClassMap().ClassIds.Count(v => v > 0);
                    session.CommitSuggestion(1, AnnotationMergeMode.FillEmptyOnly);
                    int afterClasses = session.GetClassMap().ClassIds.Count(v => v > 0);
                    Require(afterClasses >= beforeClasses, "Commit unexpectedly removed class pixels.");
                    Require(session.IsDirty || afterClasses == beforeClasses,
                        "Commit changed class state without marking session dirty.");
                    session.Cancel();
                    Require(!session.IsDirty, "Cancel failed after SAM commit.");
                    Require(BytesEqual(before, session.GetLegacyLabelImage().Rgb24), "Cancel did not restore pre-SAM label bytes.");

                    Console.WriteLine("SAM_CPU_PARITY=PASS provider=" + backend.ProviderUsed +
                        " positive=" + positive +
                        " score=" + first.Score.ToString("0.000000") +
                        " encode_ms=" + backend.LastEncodeMilliseconds.ToString("0.0") +
                        " decode_ms=" + backend.LastDecodeMilliseconds.ToString("0.0"));
                    Console.WriteLine("SAM_POINT_PROMPTS=PASS positive_score=" + point.Score.ToString("0.000000") +
                        " posneg_score=" + pointWithNegative.Score.ToString("0.000000"));
                    Console.WriteLine("SAM_EMBEDDING_CACHE=PASS encode=" + backend.EncodeCount + " decode=" + backend.DecodeCount);
                    Console.WriteLine("SAM_COMMIT_ROLLBACK=PASS before_classes=" + beforeClasses + " after_classes=" + afterClasses);
                }
            }

            TestCudaFallback(encoder, decoder);
            TestMissingModelsFailClosed(encoder, decoder);

            Console.WriteLine("SAM_BACKEND_SMOKE=PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("SAM_BACKEND_SMOKE=FAIL");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void TestCudaFallback(string encoder, string decoder)
    {
        using (var backend = new SamOnnxSuggestionBackend(new SamOnnxOptions
        {
            EncoderModelPath = encoder,
            DecoderModelPath = decoder,
            PreferCuda = true,
            CudaDeviceId = 0,
            MaskSelection = SamMaskSelection.FirstMaskLegacy
        }))
        {
            Require(backend.IsReady, "PreferCuda backend did not recover to a usable provider: " + backend.State);
            Require(backend.ProviderUsed == "CPU" || backend.ProviderUsed == "CUDA",
                "PreferCuda selected an unknown provider: " + backend.ProviderUsed);
            Console.WriteLine("SAM_CUDA_CAPABILITY=PASS provider=" + backend.ProviderUsed +
                " error=" + backend.LastProviderError);
        }
    }

    private static void TestMissingModelsFailClosed(string encoder, string decoder)
    {
        using (var backend = new SamOnnxSuggestionBackend(new SamOnnxOptions
        {
            EncoderModelPath = encoder + ".missing",
            DecoderModelPath = decoder + ".missing",
            PreferCuda = true
        }))
        {
            Require(!backend.IsReady, "Missing-model backend must not report ready.");
            Require(backend.State.IndexOf("missing", StringComparison.OrdinalIgnoreCase) >= 0,
                "Missing-model backend state is not explicit: " + backend.State);
            Console.WriteLine("SAM_MISSING_MODEL_FAIL_CLOSED=PASS state=" + backend.State);
        }
    }

    private static void RequireFile(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Smoke fixture not found.", path);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static bool BytesEqual(byte[] left, byte[] right)
    {
        if (left == null || right == null || left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
        return true;
    }
}
