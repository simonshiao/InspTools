using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using InspTools.SDK;

namespace InspTools.Agent
{
    /// <summary>
    /// Transport-neutral JSON command surface for Agent/MCP/HTTP/Pipe hosts.
    /// The dispatcher owns command state only; it does not own any transport.
    /// </summary>
    public sealed class AnnotationAgentJsonDispatcher : IDisposable
    {
        public const string Protocol = "InspTools.AnnotationAgent/1";

        private readonly object sync = new object();
        private readonly JavaScriptSerializer json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        private readonly AnnotationAgentController controller;
        private readonly List<AnnotationPrompt> prompts = new List<AnnotationPrompt>();
        private bool disposed;

        public AnnotationAgentJsonDispatcher(Func<IAnnotationSuggestionBackend> backendFactory = null)
        {
            controller = new AnnotationAgentController(backendFactory);
        }

        public string Dispatch(string requestJson)
        {
            lock (sync)
            {
                string action = string.Empty;
                try
                {
                    ThrowIfDisposed();
                    var request = json.DeserializeObject(requestJson ?? string.Empty) as IDictionary<string, object>;
                    if (request == null) return Error(string.Empty, "Request must be a JSON object.");

                    action = GetString(request, "action").Trim().ToLowerInvariant();
                    if (action.Length == 0) return Error(action, "action is required.");

                    switch (action)
                    {
                        case "annotation.capabilities":
                            return Ok(action, "Capabilities read.", new Dictionary<string, object>
                            {
                                { "protocol", Protocol },
                                { "capabilities", controller.Capabilities.ToArray() }
                            });

                        case "annotation.open":
                            prompts.Clear();
                            return From(action, controller.Open(new AnnotationAgentOpenRequest
                            {
                                TrainingImageRoot = RequireString(request, "trainingImageRoot"),
                                ModelName = RequireString(request, "modelName"),
                                Kind = ParseEnum(GetString(request, "kind", "Ng"), AnnotationSampleKind.Ng),
                                ImagePath = RequireString(request, "imagePath"),
                                LabelPath = GetString(request, "labelPath"),
                                XSubfolder = GetString(request, "xSubfolder"),
                                YSubfolder = GetString(request, "ySubfolder"),
                                Editable = GetBool(request, "editable", true),
                                AllowOverwrite = GetBool(request, "allowOverwrite", false)
                            }), StatusPayload);

                        case "annotation.status":
                        case "annotation.sam.status":
                            return From(action, controller.Status(), StatusPayload);

                        case "annotation.image.set":
                            prompts.Clear();
                            return From(action, controller.SetImage(RequireString(request, "path")), StatusPayload);

                        case "annotation.sam.point.add":
                            return AddPoint(action, request, AnnotationPromptType.PositivePoint);

                        case "annotation.sam.point.remove":
                            return AddPoint(action, request, AnnotationPromptType.NegativePoint);

                        case "annotation.sam.box":
                            prompts.Add(new AnnotationPrompt
                            {
                                Type = AnnotationPromptType.Box,
                                X = GetInt(request, "x"),
                                Y = GetInt(request, "y"),
                                X2 = GetInt(request, "x2"),
                                Y2 = GetInt(request, "y2")
                            });
                            return Ok(action, "Box prompt added.", PromptPayload());

                        case "annotation.prompts.clear":
                            prompts.Clear();
                            return Ok(action, "Prompts cleared.", PromptPayload());

                        case "annotation.suggestion.get":
                            return Suggest(action);

                        case "annotation.suggestion.commit":
                            return From(action, controller.CommitSuggestion(
                                GetInt(request, "classId"),
                                ParseEnum(GetString(request, "mergeMode", "FillEmptyOnly"), AnnotationMergeMode.FillEmptyOnly)),
                                StatusPayload);

                        case "annotation.auto.propose":
                            return From(action, controller.AutoPropose(new AnnotationAutoProposalOptions
                            {
                                PointsPerSide = GetInt(request, "pointsPerSide", 4),
                                PredIouThreshold = GetDouble(request, "predIouThreshold", 0.88),
                                StabilityThreshold = GetDouble(request, "stabilityThreshold", 0.95),
                                StabilityOffset = GetDouble(request, "stabilityOffset", 1.0),
                                MaskThreshold = GetDouble(request, "maskThreshold", 0.0),
                                MaxCoverageRatio = GetDouble(request, "maxCoverageRatio", 0.75)
                            }), AutoProposalPayload);

                        case "annotation.erase":
                            return From(action, controller.Erase(new AnnotationRect
                            {
                                X = GetInt(request, "x"),
                                Y = GetInt(request, "y"),
                                Width = GetInt(request, "width"),
                                Height = GetInt(request, "height")
                            }), StatusPayload);

                        case "annotation.class_map.get":
                            return From(action, controller.GetClassSummary(), ClassSummaryPayload);

                        case "annotation.legacy.validate":
                            return From(action, controller.ValidateLegacy(), ValidationPayload);

                        case "annotation.legacy.save":
                            return From(action, controller.SaveLegacy(), SavePayload);

                        case "annotation.cancel":
                            prompts.Clear();
                            return From(action, controller.Cancel(), StatusPayload);

                        case "annotation.close":
                            prompts.Clear();
                            return From(action, controller.Close(), value => new Dictionary<string, object> { { "closed", value } });

                        default:
                            return Error(action, "Unsupported annotation action: " + action);
                    }
                }
                catch (Exception ex)
                {
                    return Error(action, ex.GetBaseException().Message);
                }
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                prompts.Clear();
                controller.Dispose();
            }
        }

        private string AddPoint(string action, IDictionary<string, object> request, AnnotationPromptType type)
        {
            prompts.Add(new AnnotationPrompt
            {
                Type = type,
                X = GetInt(request, "x"),
                Y = GetInt(request, "y")
            });
            return Ok(action,
                type == AnnotationPromptType.PositivePoint ? "Positive point added." : "Negative point added.",
                PromptPayload());
        }

        private string Suggest(string action)
        {
            if (prompts.Count == 0)
                return Error(action, "At least one Point or Box prompt is required before suggestion.get.");
            AgentResult<AnnotationSuggestion> result = controller.Suggest(prompts);
            return From(action, result, suggestion =>
            {
                int positives = suggestion != null && suggestion.Mask != null
                    ? suggestion.Mask.Count(x => x > 0f)
                    : 0;
                return new Dictionary<string, object>
                {
                    { "success", suggestion != null && suggestion.Success },
                    { "width", suggestion == null ? 0 : suggestion.Width },
                    { "height", suggestion == null ? 0 : suggestion.Height },
                    { "score", suggestion == null ? 0d : suggestion.Score },
                    { "positivePixels", positives },
                    { "promptCount", prompts.Count },
                    { "detail", suggestion == null ? string.Empty : suggestion.Message ?? string.Empty }
                };
            });
        }

        private Dictionary<string, object> PromptPayload()
        {
            return new Dictionary<string, object>
            {
                { "promptCount", prompts.Count },
                { "positivePoints", prompts.Count(x => x.Type == AnnotationPromptType.PositivePoint) },
                { "negativePoints", prompts.Count(x => x.Type == AnnotationPromptType.NegativePoint) },
                { "boxes", prompts.Count(x => x.Type == AnnotationPromptType.Box) }
            };
        }

        private static Dictionary<string, object> StatusPayload(AnnotationAgentStatus status)
        {
            var result = new Dictionary<string, object>
            {
                { "sessionOpen", status != null && status.SessionOpen },
                { "editable", status != null && status.Editable },
                { "dirty", status != null && status.IsDirty },
                { "samReady", status != null && status.SamReady },
                { "samState", status == null ? string.Empty : status.SamState ?? string.Empty }
            };
            if (status != null && status.Image != null)
            {
                result["image"] = new Dictionary<string, object>
                {
                    { "modelName", status.Image.ModelName },
                    { "kind", status.Image.Kind.ToString() },
                    { "imagePath", status.Image.ImagePath },
                    { "labelPath", status.Image.LabelPath },
                    { "width", status.Image.Width },
                    { "height", status.Image.Height }
                };
            }
            return result;
        }

        private static Dictionary<string, object> ClassSummaryPayload(AnnotationAgentClassSummary summary)
        {
            return new Dictionary<string, object>
            {
                { "width", summary.Width },
                { "height", summary.Height },
                { "backgroundPixels", summary.BackgroundPixels },
                { "classPixelCounts", summary.ClassPixelCounts }
            };
        }

        private static Dictionary<string, object> AutoProposalPayload(AnnotationAutoProposalResult result)
        {
            var proposals = new List<Dictionary<string, object>>();
            if (result != null && result.Proposals != null)
            {
                foreach (AnnotationAutoProposal item in result.Proposals)
                {
                    proposals.Add(new Dictionary<string, object>
                    {
                        { "pointIndex", item.PointIndex },
                        { "pointX", item.PointX },
                        { "pointY", item.PointY },
                        { "maskIndex", item.MaskIndex },
                        { "score", item.Score },
                        { "stability", item.Stability },
                        { "positivePixels", item.PositivePixels },
                        { "coverageRatio", item.CoverageRatio },
                        { "box", new[] { item.X1, item.Y1, item.X2, item.Y2 } },
                        { "rejectedBySafety", item.RejectedBySafety }
                    });
                }
            }
            return new Dictionary<string, object>
            {
                { "success", result != null && result.Success },
                { "width", result == null ? 0 : result.Width },
                { "height", result == null ? 0 : result.Height },
                { "pointCount", result == null ? 0 : result.PointCount },
                { "candidateCount", result == null ? 0 : result.CandidateCount },
                { "recoveredAcceptedCount", result == null ? 0 : result.RecoveredAcceptedCount },
                { "safeProposalCount", result == null ? 0 : result.SafeProposalCount },
                { "proposals", proposals },
                { "detail", result == null ? string.Empty : result.Message ?? string.Empty }
            };
        }

        private static Dictionary<string, object> ValidationPayload(AnnotationValidationResult value)
        {
            return new Dictionary<string, object>
            {
                { "valid", value.IsValid },
                { "xExists", value.XExists },
                { "yExists", value.YExists },
                { "dimensionsMatch", value.DimensionsMatch },
                { "detail", value.Message ?? string.Empty }
            };
        }

        private static Dictionary<string, object> SavePayload(AnnotationSaveResult value)
        {
            return new Dictionary<string, object>
            {
                { "xPath", value.XPath },
                { "yPath", value.YPath },
                { "width", value.Width },
                { "height", value.Height }
            };
        }

        private string From<T>(string action, AgentResult<T> result, Func<T, object> map)
        {
            if (result == null) return Error(action, "Agent controller returned no result.");
            object payload = ReferenceEquals(result.Value, null) ? null : map(result.Value);
            if (!result.Success) return Error(action, result.Message ?? string.Empty, payload);
            return Ok(action, result.Message ?? string.Empty, payload);
        }

        private string Ok(string action, string message, object value)
        {
            return json.Serialize(new Dictionary<string, object>
            {
                { "ok", true },
                { "protocol", Protocol },
                { "action", action ?? string.Empty },
                { "message", message ?? string.Empty },
                { "value", value }
            });
        }

        private string Error(string action, string message, object value = null)
        {
            return json.Serialize(new Dictionary<string, object>
            {
                { "ok", false },
                { "protocol", Protocol },
                { "action", action ?? string.Empty },
                { "message", message ?? string.Empty },
                { "value", value }
            });
        }

        private static string RequireString(IDictionary<string, object> source, string key)
        {
            string value = GetString(source, key);
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(key + " is required.");
            return value;
        }

        private static string GetString(IDictionary<string, object> source, string key, string fallback = "")
        {
            object value;
            return source != null && source.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value)
                : fallback;
        }

        private static bool GetBool(IDictionary<string, object> source, string key, bool fallback)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || value == null) return fallback;
            bool result;
            if (bool.TryParse(Convert.ToString(value), out result)) return result;
            int number;
            if (int.TryParse(Convert.ToString(value), out number)) return number != 0;
            return fallback;
        }

        private static int GetInt(IDictionary<string, object> source, string key)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || value == null)
                throw new ArgumentException(key + " is required.");
            int result;
            if (!int.TryParse(Convert.ToString(value), out result))
                throw new ArgumentException(key + " must be an integer.");
            return result;
        }

        private static int GetInt(IDictionary<string, object> source, string key, int fallback)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || value == null) return fallback;
            int result;
            if (!int.TryParse(Convert.ToString(value), out result))
                throw new ArgumentException(key + " must be an integer.");
            return result;
        }

        private static double GetDouble(IDictionary<string, object> source, string key, double fallback)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || value == null) return fallback;
            double result;
            if (!double.TryParse(Convert.ToString(value), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out result))
                throw new ArgumentException(key + " must be a number.");
            return result;
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            T result;
            return Enum.TryParse(value ?? string.Empty, true, out result) ? result : fallback;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(AnnotationAgentJsonDispatcher));
        }
    }
}
