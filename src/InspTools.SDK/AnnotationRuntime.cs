using System;
using System.Collections.Generic;
using System.IO;
using InspTools.Core;

namespace InspTools.SDK
{
    public static class InspToolsAnnotation
    {
        public static IAnnotationSession OpenSession(AnnotationSessionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            return new AnnotationSessionAdapter(new AnnotationEngine(new EngineAnnotationOptions
            {
                TrainingImageRoot = options.TrainingImageRoot,
                ModelName = options.ModelName,
                Kind = options.Kind == AnnotationSampleKind.Ok ? EngineAnnotationSampleKind.Ok : EngineAnnotationSampleKind.Ng,
                ImagePath = options.ImagePath,
                LabelPath = options.LabelPath,
                XSubfolder = options.XSubfolder,
                YSubfolder = options.YSubfolder,
                Editable = options.Editable,
                AllowOverwrite = options.AllowOverwrite
            }), options.SuggestionBackend);
        }
    }

    internal sealed class AnnotationSessionAdapter : IAnnotationSession
    {
        private readonly AnnotationEngine engine;
        private readonly IAnnotationSuggestionBackend backend;
        private AnnotationSuggestion lastSuggestion;

        public AnnotationSessionAdapter(AnnotationEngine engine, IAnnotationSuggestionBackend backend)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this.backend = backend;
        }

        public AnnotationImageInfo Image
        {
            get
            {
                EngineAnnotationImageInfo source = engine.GetImageInfo();
                return new AnnotationImageInfo
                {
                    ModelName = source.ModelName,
                    Kind = source.Kind == EngineAnnotationSampleKind.Ok ? AnnotationSampleKind.Ok : AnnotationSampleKind.Ng,
                    ImagePath = source.ImagePath,
                    LabelPath = source.LabelPath,
                    Width = source.Width,
                    Height = source.Height
                };
            }
        }

        public bool Editable => engine.Editable;
        public bool IsDirty => engine.IsDirty;
        public bool SamReady => backend != null && backend.IsReady;
        public string SamState => backend == null
            ? "Annotation data Core is active; no suggestion backend is configured."
            : backend.State;

        public void SetImage(string path)
        {
            engine.SetImage(path);
            lastSuggestion = null;
            if (backend != null) backend.ResetImageCache();
        }

        public AnnotationSuggestion Suggest(IReadOnlyList<AnnotationPrompt> prompts)
        {
            if (backend == null || !backend.IsReady)
            {
                lastSuggestion = null;
                return new AnnotationSuggestion
                {
                    Success = false,
                    Message = SamState,
                    Width = Image.Width,
                    Height = Image.Height,
                    Mask = new float[0],
                    Score = 0
                };
            }
            if (prompts == null || prompts.Count == 0)
                throw new ArgumentException("At least one annotation prompt is required.", nameof(prompts));

            AnnotationImageInfo image = Image;
            AnnotationSuggestion suggestion = backend.Suggest(new AnnotationSuggestionRequest
            {
                ImagePath = image.ImagePath,
                Width = image.Width,
                Height = image.Height,
                Prompts = prompts
            });
            if (suggestion == null)
                throw new InvalidOperationException("Suggestion backend returned null.");
            if (suggestion.Success && (suggestion.Mask == null || suggestion.Mask.Length != image.Width * image.Height))
                throw new InvalidDataException("Suggestion backend returned a mask with invalid dimensions.");
            lastSuggestion = suggestion.Success ? CloneSuggestion(suggestion) : null;
            return CloneSuggestion(suggestion);
        }

        public void CommitSuggestion(int classId, AnnotationMergeMode mergeMode)
        {
            if (lastSuggestion == null || !lastSuggestion.Success)
                throw new InvalidOperationException("No successful suggestion is available to commit.");
            engine.CommitSuggestion(lastSuggestion.Mask, classId, (EngineAnnotationMergeMode)mergeMode);
        }

        public void Erase(AnnotationRect rect)
        {
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            engine.Erase(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void ClearClass(int classId) => engine.ClearClass(classId);
        public void ClearAll() => engine.ClearAll();

        public AnnotationClassMap GetClassMap()
        {
            EngineAnnotationClassMap source = engine.GetClassMap();
            return new AnnotationClassMap { Width = source.Width, Height = source.Height, ClassIds = Clone(source.ClassIds) };
        }

        public AnnotationLabelImage GetLegacyLabelImage()
        {
            EngineAnnotationLabelImage source = engine.GetLegacyLabelImage();
            return new AnnotationLabelImage { Width = source.Width, Height = source.Height, Rgb24 = Clone(source.Rgb24) };
        }

        public AnnotationBinaryMask ExportBinaryMask(AnnotationBinaryExportOptions options)
        {
            options = options ?? new AnnotationBinaryExportOptions();
            EngineAnnotationBinaryMask source = engine.ExportBinaryMask(options.Threshold, options.IncludeMaybe, options.IncludeIgnore);
            return new AnnotationBinaryMask { Width = source.Width, Height = source.Height, Data = Clone(source.Data) };
        }

        public AnnotationValidationResult ValidateLegacyPair()
        {
            EngineAnnotationValidation source = engine.ValidateLegacyPair();
            return new AnnotationValidationResult
            {
                IsValid = source.IsValid,
                XExists = source.XExists,
                YExists = source.YExists,
                DimensionsMatch = source.DimensionsMatch,
                Message = source.Message
            };
        }

        public AnnotationSaveResult SaveLegacyTrainingPair()
        {
            EngineAnnotationSave source = engine.SaveLegacyTrainingPair();
            return new AnnotationSaveResult
            {
                XPath = source.XPath,
                YPath = source.YPath,
                Width = source.Width,
                Height = source.Height
            };
        }

        public void Cancel()
        {
            engine.Cancel();
            lastSuggestion = null;
        }

        public void Dispose()
        {
            engine.Dispose();
            lastSuggestion = null;
        }

        private static byte[] Clone(byte[] source)
        {
            var result = new byte[source.Length];
            Buffer.BlockCopy(source, 0, result, 0, source.Length);
            return result;
        }

        private static AnnotationSuggestion CloneSuggestion(AnnotationSuggestion source)
        {
            float[] mask = source.Mask == null ? new float[0] : (float[])source.Mask.Clone();
            return new AnnotationSuggestion
            {
                Success = source.Success,
                Message = source.Message,
                Width = source.Width,
                Height = source.Height,
                Mask = mask,
                Score = source.Score
            };
        }
    }
}
