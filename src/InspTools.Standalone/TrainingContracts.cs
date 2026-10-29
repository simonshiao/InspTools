using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InspTools.Standalone
{
    // This is a source-data contract, NOT a simulated Train success.
    // The recovered DAVS/Locate/Prophet runners will be attached separately.
    internal enum TrainingFamily { General, Locate, Prophet }

    internal sealed class TrainingDatasetReport
    {
        internal TrainingFamily Family;
        internal int SourceImages;
        internal int Labels;
        internal int OkPairs;
        internal int NgPairs;
        internal bool StructureValid;
        internal string Message;
    }

    internal static class TrainingContracts
    {
        internal static TrainingDatasetReport Check(string appBase, string datasetName,
            TrainingFamily family)
        {
            if (string.IsNullOrWhiteSpace(datasetName) ||
                datasetName == "." || datasetName == ".." ||
                datasetName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                datasetName.Contains("/") || datasetName.Contains("\\"))
                throw new ArgumentException("Dataset must be a single local folder name");
            string dataRoot = Path.Combine(appBase, "TrainingImage", datasetName);
            if (!Directory.Exists(dataRoot))
                throw new DirectoryNotFoundException("TrainingImage dataset missing: " + datasetName);

            var result = new TrainingDatasetReport { Family = family };
            if (family == TrainingFamily.Prophet)
            {
                string[] groups = { "OK", "PASS", "NOPASS" };
                foreach (string group in groups)
                {
                    int count = Images(Path.Combine(dataRoot, group)).Count;
                    if (count == 0)
                        throw new InvalidDataException("預分類缺少資料組：" + group);
                    result.SourceImages += count;
                }
                result.Labels = 0; // No Y segmentation mask required
            }
            else
            {
                int populatedGroups = 0;
                foreach (string group in new[] { "OK", "NG" })
                {
                    var x = IndexedImages(Path.Combine(dataRoot, "X", group));
                    var y = IndexedImages(Path.Combine(dataRoot, "Y", group));
                    // Recovered DAVS pairs files under X/Y recursively and does
                    // not require both OK and NG categories to be populated.
                    if (x.Count == 0 && y.Count == 0) continue;
                    if (x.Count == 0 || y.Count == 0)
                        throw new InvalidDataException("X/Y training pair missing for " + group);
                    // Recovered DAVS recursively pairs X/Y by exact relative
                    // image paths, including ALL/ and the file extension.
                    if (x.Count != y.Count ||
                        x.Keys.Except(y.Keys, StringComparer.OrdinalIgnoreCase).Any() ||
                        y.Keys.Except(x.Keys, StringComparer.OrdinalIgnoreCase).Any())
                        throw new InvalidDataException("X/Y relative image-mask paths mismatch: " + group);
                    result.SourceImages += x.Count;
                    result.Labels += y.Count;
                    if (group == "OK") result.OkPairs += x.Count;
                    else result.NgPairs += x.Count;
                    populatedGroups++;
                }
                if (populatedGroups == 0)
                    throw new InvalidDataException("No X/Y training pairs found");
            }
            result.StructureValid = true;
            result.Message = "DATASET_STRUCTURE=PASS; TRAIN_NOT_STARTED";
            return result;
        }

        private static List<string> Images(string directory)
        {
            if (!Directory.Exists(directory)) return new List<string>();
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(f => new[] { ".png", ".jpg", ".jpeg", ".bmp" }
                    .Contains(Path.GetExtension(f).ToLowerInvariant())).ToList();
        }

        private static Dictionary<string, string> IndexedImages(string directory)
        {
            var images = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(directory)) return images;
            string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar);
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                foreach (string entry in Directory.EnumerateFileSystemEntries(stack.Pop()))
                {
                    FileAttributes attributes = File.GetAttributes(entry);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                        throw new InvalidDataException("Dataset reparse points forbidden: " + entry);
                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        stack.Push(entry);
                        continue;
                    }
                    string extension = Path.GetExtension(entry).ToLowerInvariant();
                    if (extension != ".png" && extension != ".jpg" &&
                        extension != ".jpeg" && extension != ".bmp") continue;
                    string rel = entry.Substring(root.Length + 1).Replace('/', '\\');
                    if (images.ContainsKey(rel))
                        throw new InvalidDataException("Duplicate relative image name: " + rel);
                    images.Add(rel, entry);
                }
            }
            return images;
        }

        internal static bool TryRun(string[] args, out int exit)
        {
            exit = 0;
            if (args == null || args.Length == 0 ||
                !args[0].StartsWith("--train-contract-", StringComparison.OrdinalIgnoreCase))
                return false;
            string testRoot = null;
            try
            {
                Console.OutputEncoding = new UTF8Encoding(false);
                if (args.Length == 1 && args[0] == "--train-contract-selftest")
                {
                    testRoot = Path.Combine(Path.GetTempPath(),
                        "InspToolsTrainContract-" + Guid.NewGuid().ToString("N"));
                    foreach (string path in new[] {
                        "general/X/OK", "general/X/NG", "general/Y/OK", "general/Y/NG",
                        "prophet/OK", "prophet/PASS", "prophet/NOPASS"
                    })
                    {
                        string folder = Path.Combine(testRoot, "TrainingImage",
                            path.Replace('/', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(folder);
                        File.WriteAllBytes(Path.Combine(folder, "item.png"),
                            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                    }
                    var general = Check(testRoot, "general", TrainingFamily.General);
                    var locate = Check(testRoot, "general", TrainingFamily.Locate);
                    var prophet = Check(testRoot, "prophet", TrainingFamily.Prophet);
                    if (!general.StructureValid || general.Labels != 2 ||
                        !locate.StructureValid || !prophet.StructureValid ||
                        prophet.Labels != 0)
                        throw new InvalidOperationException("Dataset family contract failure");
                    bool rejected = false;
                    File.Delete(Path.Combine(testRoot, "TrainingImage", "general", "Y",
                        "NG", "item.png"));
                    try { Check(testRoot, "general", TrainingFamily.General); }
                    catch (InvalidDataException) { rejected = true; }
                    if (!rejected) throw new InvalidOperationException("Missing mask not rejected");
                    File.WriteAllBytes(Path.Combine(testRoot, "TrainingImage", "general",
                        "Y", "NG", "item.png"),
                        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                    // The recovered trainer looks into NG/ALL recursively.
                    foreach (string branch in new[] { "X", "Y" })
                    {
                        string all = Path.Combine(testRoot, "TrainingImage", "general", branch, "NG", "ALL");
                        Directory.CreateDirectory(all);
                        File.WriteAllBytes(Path.Combine(all, "aug.png"),
                            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                    }
                    if (Check(testRoot, "general", TrainingFamily.General).NgPairs != 2)
                        throw new InvalidOperationException("Nested NG/ALL pairs not recognized");
                    string yAll = Path.Combine(testRoot, "TrainingImage", "general", "Y", "NG", "ALL");
                    File.Move(Path.Combine(yAll, "aug.png"), Path.Combine(yAll, "other.png"));
                    rejected = false;
                    try { Check(testRoot, "general", TrainingFamily.General); }
                    catch (InvalidDataException) { rejected = true; }
                    if (!rejected) throw new InvalidOperationException("Mismatched ALL path accepted");
                    rejected = false;
                    try { Check(testRoot, "..", TrainingFamily.General); }
                    catch (ArgumentException) { rejected = true; }
                    if (!rejected) throw new InvalidOperationException("Dataset traversal not rejected");
                    Console.WriteLine("TRAIN_CONTRACT=PASS FAMILIES=3 MASK_GUARD=PASS" +
                        " NESTED_ALL=PASS RELATIVE_PATH_GUARD=PASS" +
                        " LOCAL_PATH_GUARD=PASS TRAIN_EXECUTED=False");
                }
                else if (args.Length == 3 && args[0] == "--train-contract-check")
                {
                    TrainingFamily family;
                    if (!Enum.TryParse(args[1], true, out family) ||
                        !Enum.IsDefined(typeof(TrainingFamily), family))
                        throw new ArgumentException("Family must be General, Locate or Prophet");
                    TrainingDatasetReport result = Check(
                        AppDomain.CurrentDomain.BaseDirectory, args[2], family);
                    Console.WriteLine("TRAIN_CONTRACT=PASS FAMILY=" + family +
                        " IMAGES=" + result.SourceImages + " MASKS=" + result.Labels +
                        " OK_PAIRS=" + result.OkPairs + " NG_PAIRS=" + result.NgPairs +
                        " TRAIN_EXECUTED=False");
                }
                else throw new ArgumentException(
                    "Usage: --train-contract-check General|Locate|Prophet DATASET_NAME");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("TRAIN_CONTRACT=FAIL " + ex.GetBaseException().Message);
                exit = 71;
            }
            finally
            {
                if (testRoot != null && Directory.Exists(testRoot))
                    Directory.Delete(testRoot, true);
            }
            return true;
        }
    }
}
