using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using InspTools.SDK;

namespace InspTools.SamOnnx
{
    public enum SamMaskSelection
    {
        FirstMaskLegacy,
        BestIoU
    }

    public sealed class SamOnnxOptions
    {
        public string EncoderModelPath { get; set; }
        public string DecoderModelPath { get; set; }
        public bool PreferCuda { get; set; }
        public int CudaDeviceId { get; set; }
        public SamMaskSelection MaskSelection { get; set; } = SamMaskSelection.FirstMaskLegacy;
    }

    public sealed class SamOnnxSuggestionBackend : IAnnotationSuggestionBackend, IAnnotationAutoProposalBackend
    {
        private readonly object sync = new object();
        private readonly SamOnnxOptions options;
        private InferenceSession encoder;
        private InferenceSession decoder;
        private float[] embedding;
        private string embeddingKey = string.Empty;
        private bool disposed;
        private string state;

        public SamOnnxSuggestionBackend(SamOnnxOptions options)
        {
            this.options = ValidateAndClone(options);
            LoadModels();
        }

        public bool IsReady => !disposed && encoder != null && decoder != null;
        public string State => state ?? string.Empty;
        public string ProviderUsed { get; private set; } = string.Empty;
        public string LastProviderError { get; private set; } = string.Empty;
        public int EncodeCount { get; private set; }
        public int DecodeCount { get; private set; }
        public double LastEncodeMilliseconds { get; private set; }
        public double LastDecodeMilliseconds { get; private set; }

        public AnnotationSuggestion Suggest(AnnotationSuggestionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!IsReady)
                return Failed(request, State);
            if (string.IsNullOrWhiteSpace(request.ImagePath) || !File.Exists(request.ImagePath))
                return Failed(request, "SAM input image does not exist: " + request.ImagePath);
            if (request.Width <= 0 || request.Height <= 0)
                return Failed(request, "SAM input dimensions are invalid.");
            if (request.Prompts == null || request.Prompts.Count == 0)
                return Failed(request, "At least one SAM prompt is required.");

            lock (sync)
            {
                ThrowIfDisposed();
                EnsureEmbedding(request.ImagePath, request.Width, request.Height);
                return Decode(request);
            }
        }

        public AnnotationAutoProposalResult Propose(AnnotationAutoProposalRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!IsReady) return FailedAuto(request, State);
            if (string.IsNullOrWhiteSpace(request.ImagePath) || !File.Exists(request.ImagePath))
                return FailedAuto(request, "SAM input image does not exist: " + request.ImagePath);
            if (request.Width <= 0 || request.Height <= 0)
                return FailedAuto(request, "SAM input dimensions are invalid.");

            AnnotationAutoProposalOptions auto = ValidateAutoOptions(request.Options);
            lock (sync)
            {
                ThrowIfDisposed();
                EnsureEmbedding(request.ImagePath, request.Width, request.Height);

                int newWidth, newHeight;
                GetPreprocessShape(request.Width, request.Height, 1024, out newWidth, out newHeight);
                int pixels = request.Width * request.Height;
                var proposals = new List<AnnotationAutoProposal>();
                int candidateCount = 0;
                int recoveredAccepted = 0;
                int safeCount = 0;
                double gridOffset = 1.0 / (2.0 * auto.PointsPerSide);
                double gridStep = (1.0 - 2.0 * gridOffset) / (auto.PointsPerSide - 1);
                int pointIndex = 0;

                for (int row = 0; row < auto.PointsPerSide; row++)
                {
                    double nx = gridOffset + row * gridStep;
                    for (int col = 0; col < auto.PointsPerSide; col++, pointIndex++)
                    {
                        double ny = gridOffset + col * gridStep;
                        int x = (int)(nx * request.Width);
                        int y = (int)(ny * request.Height);
                        int tx = (int)(x * (double)newWidth / request.Width);
                        int ty = (int)(y * (double)newHeight / request.Height);
                        DecoderCandidates decoded = DecodeSinglePositivePoint(request.Width, request.Height, tx, ty);
                        candidateCount += decoded.MaskCount;

                        int maxMasks = Math.Min(decoded.MaskCount, decoded.Iou.Length);
                        for (int maskIndex = 0; maskIndex < maxMasks; maskIndex++)
                        {
                            int start = maskIndex * pixels;
                            if (decoded.AllMasks.Length < start + pixels)
                                throw new InvalidDataException("SAM decoder auto-proposal mask output is smaller than expected.");

                            double stable = CalculateStability(decoded.AllMasks, start, pixels,
                                auto.MaskThreshold, auto.StabilityOffset);
                            double score = decoded.Iou[maskIndex];
                            if (!(score > auto.PredIouThreshold && stable > auto.StabilityThreshold)) continue;

                            recoveredAccepted++;
                            int positive, x1, y1, x2, y2;
                            CalculateMaskSummary(decoded.AllMasks, start, request.Width, request.Height,
                                auto.MaskThreshold, out positive, out x1, out y1, out x2, out y2);
                            double coverage = pixels == 0 ? 0d : (double)positive / pixels;
                            bool rejected = coverage > auto.MaxCoverageRatio;
                            if (!rejected) safeCount++;
                            proposals.Add(new AnnotationAutoProposal
                            {
                                PointIndex = pointIndex,
                                PointX = x,
                                PointY = y,
                                MaskIndex = maskIndex,
                                Score = score,
                                Stability = stable,
                                PositivePixels = positive,
                                CoverageRatio = coverage,
                                X1 = x1,
                                Y1 = y1,
                                X2 = x2,
                                Y2 = y2,
                                RejectedBySafety = rejected
                            });
                        }
                    }
                }

                return new AnnotationAutoProposalResult
                {
                    Success = true,
                    Message = "Recovered SAMAutoMask proposal scan complete; read-only, no auto commit.",
                    Width = request.Width,
                    Height = request.Height,
                    PointCount = auto.PointsPerSide * auto.PointsPerSide,
                    CandidateCount = candidateCount,
                    RecoveredAcceptedCount = recoveredAccepted,
                    SafeProposalCount = safeCount,
                    Proposals = proposals.AsReadOnly()
                };
            }
        }

        public void ResetImageCache()
        {
            lock (sync)
            {
                embedding = null;
                embeddingKey = string.Empty;
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                embedding = null;
                embeddingKey = string.Empty;
                if (encoder != null) encoder.Dispose();
                if (decoder != null) decoder.Dispose();
                encoder = null;
                decoder = null;
                state = "Disposed";
            }
        }

        private void LoadModels()
        {
            if (!File.Exists(options.EncoderModelPath) || !File.Exists(options.DecoderModelPath))
            {
                state = "SAM encoder/decoder model file is missing.";
                return;
            }

            if (options.PreferCuda)
            {
                try
                {
                    using (SessionOptions so = CreateSessionOptions())
                    {
                        so.AppendExecutionProvider_CUDA(options.CudaDeviceId);
                        encoder = new InferenceSession(options.EncoderModelPath, so);
                        decoder = new InferenceSession(options.DecoderModelPath, so);
                    }
                    ProviderUsed = "CUDA";
                    state = "Ready (CUDA, device " + options.CudaDeviceId + ")";
                    return;
                }
                catch (Exception ex)
                {
                    DisposeSessionsOnly();
                    LastProviderError = "CUDA provider load failed: " + ex.GetBaseException().Message;
                    state = "CUDA unavailable; falling back to CPU: " + LastProviderError;
                }
            }

            try
            {
                using (SessionOptions so = CreateSessionOptions())
                {
                    encoder = new InferenceSession(options.EncoderModelPath, so);
                    decoder = new InferenceSession(options.DecoderModelPath, so);
                }
                ProviderUsed = "CPU";
                state = options.PreferCuda ? "Ready (CPU fallback); " + LastProviderError : "Ready (CPU)";
            }
            catch (Exception ex)
            {
                DisposeSessionsOnly();
                ProviderUsed = string.Empty;
                LastProviderError = "CPU provider load failed: " + ex.GetBaseException().Message;
                state = "SAM model load failed: " + ex.GetBaseException().Message;
            }
        }

        private static SessionOptions CreateSessionOptions()
        {
            var result = new SessionOptions();
            result.EnableMemoryPattern = false;
            result.EnableCpuMemArena = false;
            return result;
        }

        private void EnsureEmbedding(string imagePath, int width, int height)
        {
            var file = new FileInfo(imagePath);
            string key = Path.GetFullPath(imagePath) + "|" + file.Length + "|" + file.LastWriteTimeUtc.Ticks + "|" + width + "x" + height;
            if (embedding != null && string.Equals(key, embeddingKey, StringComparison.Ordinal)) return;

            byte[] encoded = File.ReadAllBytes(imagePath); // Unicode-safe path handling for OpenCV 3.x
            using (Mat image = Cv2.ImDecode(encoded, ImreadModes.Color))
            {
                if (image.Empty()) throw new InvalidDataException("OpenCV could not decode SAM input image: " + imagePath);
                if (image.Width != width || image.Height != height)
                    throw new InvalidDataException("SAM image dimensions differ from annotation session dimensions.");

                float[] input = ApplyRecoveredPreprocess(image, width, height);
                var tensor = new DenseTensor<float>(input, new[] { 1, 3, 1024, 1024 });
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("x", tensor) };
                Stopwatch sw = Stopwatch.StartNew();
                using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs = encoder.Run(inputs))
                {
                    DisposableNamedOnnxValue value = outputs.First(v => v.Name == "image_embeddings");
                    embedding = value.AsTensor<float>().ToArray();
                }
                sw.Stop();
                LastEncodeMilliseconds = sw.Elapsed.TotalMilliseconds;
                EncodeCount++;
                embeddingKey = key;
            }
        }

        private AnnotationSuggestion Decode(AnnotationSuggestionRequest request)
        {
            int newWidth, newHeight;
            GetPreprocessShape(request.Width, request.Height, 1024, out newWidth, out newHeight);
            var coords = new List<float>();
            var labels = new List<float>();
            float sx = (float)newWidth / request.Width;
            float sy = (float)newHeight / request.Height;

            foreach (AnnotationPrompt prompt in request.Prompts)
            {
                if (prompt == null) throw new ArgumentException("Prompt list contains null.", nameof(request));
                switch (prompt.Type)
                {
                    case AnnotationPromptType.PositivePoint:
                    case AnnotationPromptType.NegativePoint:
                        coords.Add((int)(prompt.X * sx));
                        coords.Add((int)(prompt.Y * sy));
                        labels.Add(prompt.Type == AnnotationPromptType.PositivePoint ? 1f : 0f);
                        break;
                    case AnnotationPromptType.Box:
                        int left = Math.Min(prompt.X, prompt.X2);
                        int top = Math.Min(prompt.Y, prompt.Y2);
                        int right = Math.Max(prompt.X, prompt.X2);
                        int bottom = Math.Max(prompt.Y, prompt.Y2);
                        coords.Add((int)(left * sx));
                        coords.Add((int)(top * sy));
                        coords.Add((int)(right * sx));
                        coords.Add((int)(bottom * sy));
                        labels.Add(2f);
                        labels.Add(3f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(prompt.Type));
                }
            }

            int pointCount = labels.Count;
            var embeddingTensor = new DenseTensor<float>(embedding, new[] { 1, 256, 64, 64 });
            var coordsTensor = new DenseTensor<float>(coords.ToArray(), new[] { 1, pointCount, 2 });
            var labelsTensor = new DenseTensor<float>(labels.ToArray(), new[] { 1, pointCount });
            var maskInputTensor = new DenseTensor<float>(new float[256 * 256], new[] { 1, 1, 256, 256 });
            var hasMaskTensor = new DenseTensor<float>(new float[1], new[] { 1 });
            var originalSizeTensor = new DenseTensor<float>(new[] { (float)request.Height, (float)request.Width }, new[] { 2 });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image_embeddings", embeddingTensor),
                NamedOnnxValue.CreateFromTensor("point_coords", coordsTensor),
                NamedOnnxValue.CreateFromTensor("point_labels", labelsTensor),
                NamedOnnxValue.CreateFromTensor("mask_input", maskInputTensor),
                NamedOnnxValue.CreateFromTensor("has_mask_input", hasMaskTensor),
                NamedOnnxValue.CreateFromTensor("orig_im_size", originalSizeTensor)
            };

            Stopwatch sw = Stopwatch.StartNew();
            float[] allMasks;
            float[] iou;
            int maskCount;
            using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs = decoder.Run(inputs))
            {
                Tensor<float> masksTensor = outputs.First(v => v.Name == "masks").AsTensor<float>();
                Tensor<float> iouTensor = outputs.First(v => v.Name == "iou_predictions").AsTensor<float>();
                allMasks = masksTensor.ToArray();
                iou = iouTensor.ToArray();
                int[] shape = masksTensor.Dimensions.ToArray();
                maskCount = shape.Length >= 2 ? shape[1] : 1;
            }
            sw.Stop();
            LastDecodeMilliseconds = sw.Elapsed.TotalMilliseconds;
            DecodeCount++;

            int selected = 0;
            if (options.MaskSelection == SamMaskSelection.BestIoU && iou.Length > 0)
            {
                int max = Math.Min(maskCount, iou.Length);
                for (int i = 1; i < max; i++) if (iou[i] > iou[selected]) selected = i;
            }

            int pixels = request.Width * request.Height;
            if (allMasks.Length < (selected + 1) * pixels)
                throw new InvalidDataException("SAM decoder mask output is smaller than the requested image dimensions.");
            var mask = new float[pixels];
            Array.Copy(allMasks, selected * pixels, mask, 0, pixels);
            double score = selected < iou.Length ? iou[selected] : 0d;
            return new AnnotationSuggestion
            {
                Success = true,
                Message = "SAM suggestion PASS; provider=" + ProviderUsed + "; mask=" + selected,
                Width = request.Width,
                Height = request.Height,
                Mask = mask,
                Score = score
            };
        }

        private DecoderCandidates DecodeSinglePositivePoint(int width, int height, int transformedX, int transformedY)
        {
            var embeddingTensor = new DenseTensor<float>(embedding, new[] { 1, 256, 64, 64 });
            var coordsTensor = new DenseTensor<float>(new[] { (float)transformedX, (float)transformedY }, new[] { 1, 1, 2 });
            var labelsTensor = new DenseTensor<float>(new[] { 1f }, new[] { 1, 1 });
            var maskInputTensor = new DenseTensor<float>(new float[256 * 256], new[] { 1, 1, 256, 256 });
            var hasMaskTensor = new DenseTensor<float>(new float[1], new[] { 1 });
            var originalSizeTensor = new DenseTensor<float>(new[] { (float)height, (float)width }, new[] { 2 });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image_embeddings", embeddingTensor),
                NamedOnnxValue.CreateFromTensor("point_coords", coordsTensor),
                NamedOnnxValue.CreateFromTensor("point_labels", labelsTensor),
                NamedOnnxValue.CreateFromTensor("mask_input", maskInputTensor),
                NamedOnnxValue.CreateFromTensor("has_mask_input", hasMaskTensor),
                NamedOnnxValue.CreateFromTensor("orig_im_size", originalSizeTensor)
            };

            Stopwatch sw = Stopwatch.StartNew();
            float[] masks;
            float[] iou;
            int maskCount;
            using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs = decoder.Run(inputs))
            {
                Tensor<float> masksTensor = outputs.First(v => v.Name == "masks").AsTensor<float>();
                masks = masksTensor.ToArray();
                iou = outputs.First(v => v.Name == "iou_predictions").AsTensor<float>().ToArray();
                int[] shape = masksTensor.Dimensions.ToArray();
                maskCount = shape.Length >= 2 ? shape[1] : 1;
            }
            sw.Stop();
            LastDecodeMilliseconds = sw.Elapsed.TotalMilliseconds;
            DecodeCount++;
            return new DecoderCandidates(masks, iou, maskCount);
        }

        private static double CalculateStability(float[] masks, int start, int pixels,
            double maskThreshold, double stabilityOffset)
        {
            int upper = 0;
            int lower = 0;
            double high = maskThreshold + stabilityOffset;
            double low = maskThreshold - stabilityOffset;
            for (int i = 0; i < pixels; i++)
            {
                float value = masks[start + i];
                if (value > high) upper++;
                if (value > low) lower++;
            }
            return lower == 0 ? 0d : (double)upper / lower;
        }

        private static void CalculateMaskSummary(float[] masks, int start, int width, int height,
            double threshold, out int positive, out int x1, out int y1, out int x2, out int y2)
        {
            positive = 0;
            x1 = width;
            y1 = height;
            x2 = 0;
            y2 = 0;
            for (int y = 0; y < height; y++)
            {
                int row = start + y * width;
                for (int x = 0; x < width; x++)
                {
                    if (!(masks[row + x] > threshold)) continue;
                    positive++;
                    if (x < x1) x1 = x;
                    if (x > x2) x2 = x;
                    if (y < y1) y1 = y;
                    if (y > y2) y2 = y;
                }
            }
            if (positive == 0) x1 = y1 = x2 = y2 = 0;
        }

        private static AnnotationAutoProposalOptions ValidateAutoOptions(AnnotationAutoProposalOptions source)
        {
            source = source ?? new AnnotationAutoProposalOptions();
            if (source.PointsPerSide < 2 || source.PointsPerSide > 64)
                throw new ArgumentOutOfRangeException(nameof(source.PointsPerSide), "PointsPerSide must be 2..64.");
            if (source.PredIouThreshold < 0 || source.PredIouThreshold > 1)
                throw new ArgumentOutOfRangeException(nameof(source.PredIouThreshold));
            if (source.StabilityThreshold < 0 || source.StabilityThreshold > 1)
                throw new ArgumentOutOfRangeException(nameof(source.StabilityThreshold));
            if (source.StabilityOffset < 0 || double.IsNaN(source.StabilityOffset) || double.IsInfinity(source.StabilityOffset))
                throw new ArgumentOutOfRangeException(nameof(source.StabilityOffset));
            if (double.IsNaN(source.MaskThreshold) || double.IsInfinity(source.MaskThreshold))
                throw new ArgumentOutOfRangeException(nameof(source.MaskThreshold));
            if (!(source.MaxCoverageRatio > 0 && source.MaxCoverageRatio <= 1))
                throw new ArgumentOutOfRangeException(nameof(source.MaxCoverageRatio), "MaxCoverageRatio must be > 0 and <= 1.");
            return new AnnotationAutoProposalOptions
            {
                PointsPerSide = source.PointsPerSide,
                PredIouThreshold = source.PredIouThreshold,
                StabilityThreshold = source.StabilityThreshold,
                StabilityOffset = source.StabilityOffset,
                MaskThreshold = source.MaskThreshold,
                MaxCoverageRatio = source.MaxCoverageRatio
            };
        }

        private static float[] ApplyRecoveredPreprocess(Mat image, int originalWidth, int originalHeight)
        {
            int newWidth, newHeight;
            GetPreprocessShape(originalWidth, originalHeight, 1024, out newWidth, out newHeight);
            using (var resized = new Mat())
            using (var f32 = new Mat())
            using (var normalized = new Mat())
            {
                Cv2.Resize(image, resized, new Size(newWidth, newHeight));
                resized.ConvertTo(f32, MatType.CV_32FC3);
                Scalar mean, stddev;
                Cv2.MeanStdDev(f32, out mean, out stddev);
                Cv2.Subtract(f32, mean, normalized);
                Cv2.Divide(normalized, stddev, normalized);

                var result = new float[3 * 1024 * 1024];
                int plane = 1024 * 1024;
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        Vec3f value = normalized.At<Vec3f>(y, x);
                        int index = y * 1024 + x;
                        result[index] = value.Item0;
                        result[plane + index] = value.Item1;
                        result[2 * plane + index] = value.Item2;
                    }
                }
                return result;
            }
        }

        private static void GetPreprocessShape(int oldWidth, int oldHeight, int target, out int newWidth, out int newHeight)
        {
            float scale = (float)target / Math.Max(oldHeight, oldWidth);
            newWidth = (int)(oldWidth * scale + 0.5f);
            newHeight = (int)(oldHeight * scale + 0.5f);
        }

        private static SamOnnxOptions ValidateAndClone(SamOnnxOptions source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(source.EncoderModelPath)) throw new ArgumentException("EncoderModelPath is required.");
            if (string.IsNullOrWhiteSpace(source.DecoderModelPath)) throw new ArgumentException("DecoderModelPath is required.");
            return new SamOnnxOptions
            {
                EncoderModelPath = Path.GetFullPath(source.EncoderModelPath),
                DecoderModelPath = Path.GetFullPath(source.DecoderModelPath),
                PreferCuda = source.PreferCuda,
                CudaDeviceId = source.CudaDeviceId,
                MaskSelection = source.MaskSelection
            };
        }

        private static AnnotationSuggestion Failed(AnnotationSuggestionRequest request, string message)
        {
            return new AnnotationSuggestion
            {
                Success = false,
                Message = message ?? string.Empty,
                Width = request == null ? 0 : request.Width,
                Height = request == null ? 0 : request.Height,
                Mask = new float[0],
                Score = 0d
            };
        }

        private static AnnotationAutoProposalResult FailedAuto(AnnotationAutoProposalRequest request, string message)
        {
            return new AnnotationAutoProposalResult
            {
                Success = false,
                Message = message ?? string.Empty,
                Width = request == null ? 0 : request.Width,
                Height = request == null ? 0 : request.Height,
                Proposals = new List<AnnotationAutoProposal>().AsReadOnly()
            };
        }

        private sealed class DecoderCandidates
        {
            internal DecoderCandidates(float[] masks, float[] iou, int maskCount)
            {
                AllMasks = masks;
                Iou = iou;
                MaskCount = maskCount;
            }

            internal float[] AllMasks { get; private set; }
            internal float[] Iou { get; private set; }
            internal int MaskCount { get; private set; }
        }

        private void DisposeSessionsOnly()
        {
            if (encoder != null) encoder.Dispose();
            if (decoder != null) decoder.Dispose();
            encoder = null;
            decoder = null;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SamOnnxSuggestionBackend));
        }
    }
}
