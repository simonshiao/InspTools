using System;
using System.Collections.Generic;
using System.Linq;
using InspTools.SDK;

namespace InspTools.Agent
{
    public sealed class AgentResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Value { get; set; }
    }

    public sealed class AnnotationAgentOpenRequest
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
    }

    public sealed class AnnotationAgentStatus
    {
        public bool SessionOpen { get; set; }
        public bool Editable { get; set; }
        public bool IsDirty { get; set; }
        public bool SamReady { get; set; }
        public string SamState { get; set; }
        public AnnotationImageInfo Image { get; set; }
    }

    public sealed class AnnotationAgentClassSummary
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] ClassPixelCounts { get; set; }
        public int BackgroundPixels { get; set; }
    }

    public sealed class AnnotationAgentController : IDisposable
    {
        private static readonly string[] capabilityNames =
        {
            "annotation.capabilities",
            "annotation.open",
            "annotation.status",
            "annotation.close",
            "annotation.image.set",
            "annotation.sam.status",
            "annotation.sam.point.add",
            "annotation.sam.point.remove",
            "annotation.sam.box",
            "annotation.prompts.clear",
            "annotation.suggestion.get",
            "annotation.suggestion.commit",
            "annotation.auto.propose",
            "annotation.erase",
            "annotation.class_map.get",
            "annotation.legacy.validate",
            "annotation.legacy.save",
            "annotation.cancel"
        };

        private readonly Func<IAnnotationSuggestionBackend> backendFactory;
        private IAnnotationSuggestionBackend backend;
        private IAnnotationSession session;
        private bool disposed;

        public AnnotationAgentController(Func<IAnnotationSuggestionBackend> backendFactory = null)
        {
            this.backendFactory = backendFactory;
        }

        public IReadOnlyList<string> Capabilities => capabilityNames;

        public AgentResult<AnnotationAgentStatus> Open(AnnotationAgentOpenRequest request)
        {
            return Guard(() =>
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                CloseSessionOnly();
                if (backend == null && backendFactory != null) backend = backendFactory();
                session = InspToolsAnnotation.OpenSession(new AnnotationSessionOptions
                {
                    TrainingImageRoot = request.TrainingImageRoot,
                    ModelName = request.ModelName,
                    Kind = request.Kind,
                    ImagePath = request.ImagePath,
                    LabelPath = request.LabelPath,
                    XSubfolder = request.XSubfolder,
                    YSubfolder = request.YSubfolder,
                    Editable = request.Editable,
                    AllowOverwrite = request.AllowOverwrite,
                    SuggestionBackend = backend
                });
                return StatusCore();
            }, "Annotation session opened.");
        }

        public AgentResult<AnnotationAgentStatus> Status()
        {
            return Guard(StatusCore, "Annotation status read.");
        }

        public AgentResult<AnnotationAgentStatus> SetImage(string path)
        {
            return Guard(() =>
            {
                RequireSession().SetImage(path);
                return StatusCore();
            }, "Annotation image changed.");
        }

        public AgentResult<AnnotationSuggestion> Suggest(IReadOnlyList<AnnotationPrompt> prompts)
        {
            return Guard(() => RequireSession().Suggest(prompts), "Annotation suggestion completed.");
        }

        public AgentResult<AnnotationAgentStatus> CommitSuggestion(int classId, AnnotationMergeMode mergeMode)
        {
            return Guard(() =>
            {
                RequireSession().CommitSuggestion(classId, mergeMode);
                return StatusCore();
            }, "Annotation suggestion committed.");
        }

        public AgentResult<AnnotationAutoProposalResult> AutoPropose(AnnotationAutoProposalOptions options = null)
        {
            return Guard(() =>
            {
                IAnnotationSession active = RequireSession();
                var autoBackend = backend as IAnnotationAutoProposalBackend;
                if (autoBackend == null)
                {
                    return new AnnotationAutoProposalResult
                    {
                        Success = false,
                        Message = "The configured suggestion backend does not support automatic proposals.",
                        Width = active.Image.Width,
                        Height = active.Image.Height,
                        Proposals = new List<AnnotationAutoProposal>().AsReadOnly()
                    };
                }
                AnnotationImageInfo image = active.Image;
                return autoBackend.Propose(new AnnotationAutoProposalRequest
                {
                    ImagePath = image.ImagePath,
                    Width = image.Width,
                    Height = image.Height,
                    Options = options ?? new AnnotationAutoProposalOptions()
                });
            }, "Automatic annotation proposals scanned.");
        }

        public AgentResult<AnnotationAgentStatus> Erase(AnnotationRect rect)
        {
            return Guard(() =>
            {
                RequireSession().Erase(rect);
                return StatusCore();
            }, "Annotation region erased.");
        }

        public AgentResult<AnnotationAgentClassSummary> GetClassSummary()
        {
            return Guard(() =>
            {
                AnnotationClassMap map = RequireSession().GetClassMap();
                var counts = new int[7];
                foreach (byte id in map.ClassIds)
                {
                    if (id <= 6) counts[id]++;
                }
                return new AnnotationAgentClassSummary
                {
                    Width = map.Width,
                    Height = map.Height,
                    ClassPixelCounts = counts,
                    BackgroundPixels = counts[0]
                };
            }, "Annotation class summary read.");
        }

        public AgentResult<AnnotationClassMap> GetClassMap()
        {
            return Guard(() => RequireSession().GetClassMap(), "Annotation class map read.");
        }

        public AgentResult<AnnotationValidationResult> ValidateLegacy()
        {
            return Guard(() => RequireSession().ValidateLegacyPair(), "Legacy annotation pair validated.");
        }

        public AgentResult<AnnotationSaveResult> SaveLegacy()
        {
            return Guard(() => RequireSession().SaveLegacyTrainingPair(), "Legacy annotation pair saved.");
        }

        public AgentResult<AnnotationAgentStatus> Cancel()
        {
            return Guard(() =>
            {
                RequireSession().Cancel();
                return StatusCore();
            }, "Annotation changes cancelled.");
        }

        public AgentResult<bool> Close()
        {
            return Guard(() =>
            {
                CloseSessionOnly();
                return true;
            }, "Annotation session closed.");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            CloseSessionOnly();
            if (backend != null) backend.Dispose();
            backend = null;
        }

        private AnnotationAgentStatus StatusCore()
        {
            ThrowIfDisposed();
            if (session == null)
            {
                return new AnnotationAgentStatus
                {
                    SessionOpen = false,
                    SamReady = backend != null && backend.IsReady,
                    SamState = backend == null ? "No suggestion backend configured." : backend.State
                };
            }

            return new AnnotationAgentStatus
            {
                SessionOpen = true,
                Editable = session.Editable,
                IsDirty = session.IsDirty,
                SamReady = session.SamReady,
                SamState = session.SamState,
                Image = session.Image
            };
        }

        private IAnnotationSession RequireSession()
        {
            ThrowIfDisposed();
            if (session == null) throw new InvalidOperationException("No annotation session is open.");
            return session;
        }

        private AgentResult<T> Guard<T>(Func<T> action, string successMessage)
        {
            try
            {
                ThrowIfDisposed();
                T value = action();
                bool semanticSuccess = true;
                var suggestion = value as AnnotationSuggestion;
                if (suggestion != null) semanticSuccess = suggestion.Success;
                var validation = value as AnnotationValidationResult;
                if (validation != null) semanticSuccess = validation.IsValid;
                var autoProposal = value as AnnotationAutoProposalResult;
                if (autoProposal != null) semanticSuccess = autoProposal.Success;
                return new AgentResult<T>
                {
                    Success = semanticSuccess,
                    Message = semanticSuccess ? successMessage : ExtractMessage(value),
                    Value = value
                };
            }
            catch (Exception ex)
            {
                return new AgentResult<T>
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    Value = default(T)
                };
            }
        }

        private static string ExtractMessage<T>(T value)
        {
            var suggestion = value as AnnotationSuggestion;
            if (suggestion != null) return suggestion.Message ?? string.Empty;
            var validation = value as AnnotationValidationResult;
            if (validation != null) return validation.Message ?? string.Empty;
            var autoProposal = value as AnnotationAutoProposalResult;
            if (autoProposal != null) return autoProposal.Message ?? string.Empty;
            return string.Empty;
        }

        private void CloseSessionOnly()
        {
            if (session != null) session.Dispose();
            session = null;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(AnnotationAgentController));
        }
    }
}
