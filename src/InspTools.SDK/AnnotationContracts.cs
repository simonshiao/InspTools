using System;
using System.Collections.Generic;

namespace InspTools.SDK
{
    public enum AnnotationSampleKind
    {
        Ok,
        Ng
    }

    public enum AnnotationPromptType
    {
        PositivePoint,
        NegativePoint,
        Box
    }

    public enum AnnotationMergeMode
    {
        FillEmptyOnly,
        Overwrite,
        UnionSameClass,
        ReplaceRegion
    }

    public sealed class AnnotationSessionOptions
    {
        public string TrainingImageRoot { get; set; }
        public string ModelName { get; set; }
        public AnnotationSampleKind Kind { get; set; }
        public string ImagePath { get; set; }
        public string LabelPath { get; set; }
        public string XSubfolder { get; set; }
        public string YSubfolder { get; set; }
        public bool Editable { get; set; }
        public bool AllowOverwrite { get; set; }
        public IAnnotationSuggestionBackend SuggestionBackend { get; set; }
    }

    public sealed class AnnotationImageInfo
    {
        public string ModelName { get; internal set; }
        public AnnotationSampleKind Kind { get; internal set; }
        public string ImagePath { get; internal set; }
        public string LabelPath { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    public sealed class AnnotationPrompt
    {
        public AnnotationPromptType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
    }

    public sealed class AnnotationSuggestion
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float[] Mask { get; set; }
        public double Score { get; set; }
    }

    public sealed class AnnotationSuggestionRequest
    {
        public string ImagePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public IReadOnlyList<AnnotationPrompt> Prompts { get; set; }
    }

    public interface IAnnotationSuggestionBackend : IDisposable
    {
        bool IsReady { get; }
        string State { get; }
        AnnotationSuggestion Suggest(AnnotationSuggestionRequest request);
        void ResetImageCache();
    }

    // Optional capability implemented by suggestion backends that can enumerate
    // recovered SAM automatic-mask proposals.  This surface is intentionally
    // read-only: it returns proposal metadata only and never exposes/commits a
    // dense mask.
    public interface IAnnotationAutoProposalBackend
    {
        AnnotationAutoProposalResult Propose(AnnotationAutoProposalRequest request);
    }

    public sealed class AnnotationAutoProposalOptions
    {
        public int PointsPerSide { get; set; } = 4;
        public double PredIouThreshold { get; set; } = 0.88;
        public double StabilityThreshold { get; set; } = 0.95;
        public double StabilityOffset { get; set; } = 1.0;
        public double MaskThreshold { get; set; } = 0.0;
        // Safety policy added by the rebuilt Agent contract, not recovered
        // vendor parity.  Giant near-full-frame masks remain observable but are
        // explicitly rejected from the safe proposal count.
        public double MaxCoverageRatio { get; set; } = 0.75;
    }

    public sealed class AnnotationAutoProposalRequest
    {
        public string ImagePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public AnnotationAutoProposalOptions Options { get; set; }
    }

    public sealed class AnnotationAutoProposal
    {
        public int PointIndex { get; set; }
        public int PointX { get; set; }
        public int PointY { get; set; }
        public int MaskIndex { get; set; }
        public double Score { get; set; }
        public double Stability { get; set; }
        public int PositivePixels { get; set; }
        public double CoverageRatio { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public bool RejectedBySafety { get; set; }
    }

    public sealed class AnnotationAutoProposalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PointCount { get; set; }
        public int CandidateCount { get; set; }
        public int RecoveredAcceptedCount { get; set; }
        public int SafeProposalCount { get; set; }
        public IReadOnlyList<AnnotationAutoProposal> Proposals { get; set; }
    }

    public sealed class AnnotationRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public sealed class AnnotationClassMap
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public byte[] ClassIds { get; internal set; }
    }

    public sealed class AnnotationLabelImage
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public byte[] Rgb24 { get; internal set; }
    }

    public sealed class AnnotationBinaryExportOptions
    {
        public int Threshold { get; set; } = 220;
        public bool IncludeMaybe { get; set; }
        public bool IncludeIgnore { get; set; }
    }

    public sealed class AnnotationBinaryMask
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public byte[] Data { get; internal set; }
    }

    public sealed class AnnotationValidationResult
    {
        public bool IsValid { get; internal set; }
        public bool XExists { get; internal set; }
        public bool YExists { get; internal set; }
        public bool DimensionsMatch { get; internal set; }
        public string Message { get; internal set; }
    }

    public sealed class AnnotationSaveResult
    {
        public string XPath { get; internal set; }
        public string YPath { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    public interface IAnnotationSession : IDisposable
    {
        AnnotationImageInfo Image { get; }
        bool Editable { get; }
        bool IsDirty { get; }
        bool SamReady { get; }
        string SamState { get; }

        void SetImage(string path);
        AnnotationSuggestion Suggest(IReadOnlyList<AnnotationPrompt> prompts);
        void CommitSuggestion(int classId, AnnotationMergeMode mergeMode);

        void Erase(AnnotationRect rect);
        void ClearClass(int classId);
        void ClearAll();

        AnnotationClassMap GetClassMap();
        AnnotationLabelImage GetLegacyLabelImage();
        AnnotationBinaryMask ExportBinaryMask(AnnotationBinaryExportOptions options);

        AnnotationValidationResult ValidateLegacyPair();
        AnnotationSaveResult SaveLegacyTrainingPair();
        void Cancel();
    }
}
