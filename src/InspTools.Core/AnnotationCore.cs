using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace InspTools.Core
{
    public enum EngineAnnotationSampleKind
    {
        Ok,
        Ng
    }

    public enum EngineAnnotationMergeMode
    {
        FillEmptyOnly,
        Overwrite,
        UnionSameClass,
        ReplaceRegion
    }

    public sealed class EngineAnnotationOptions
    {
        public string TrainingImageRoot { get; set; }
        public string ModelName { get; set; }
        public EngineAnnotationSampleKind Kind { get; set; }
        public string ImagePath { get; set; }
        public string LabelPath { get; set; }
        public string XSubfolder { get; set; }
        public string YSubfolder { get; set; }
        public bool Editable { get; set; }
        public bool AllowOverwrite { get; set; }
    }

    public sealed class EngineAnnotationImageInfo
    {
        internal EngineAnnotationImageInfo(string modelName, EngineAnnotationSampleKind kind,
            string imagePath, string labelPath, int width, int height)
        {
            ModelName = modelName;
            Kind = kind;
            ImagePath = imagePath;
            LabelPath = labelPath ?? string.Empty;
            Width = width;
            Height = height;
        }

        public string ModelName { get; private set; }
        public EngineAnnotationSampleKind Kind { get; private set; }
        public string ImagePath { get; private set; }
        public string LabelPath { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    }

    public sealed class EngineAnnotationClassMap
    {
        internal EngineAnnotationClassMap(int width, int height, byte[] classIds)
        {
            Width = width;
            Height = height;
            ClassIds = classIds;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] ClassIds { get; private set; }
    }

    public sealed class EngineAnnotationLabelImage
    {
        internal EngineAnnotationLabelImage(int width, int height, byte[] rgb24)
        {
            Width = width;
            Height = height;
            Rgb24 = rgb24;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Rgb24 { get; private set; }
    }

    public sealed class EngineAnnotationBinaryMask
    {
        internal EngineAnnotationBinaryMask(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Data { get; private set; }
    }

    public sealed class EngineAnnotationValidation
    {
        internal EngineAnnotationValidation(bool valid, bool xExists, bool yExists,
            bool dimensionsMatch, string message)
        {
            IsValid = valid;
            XExists = xExists;
            YExists = yExists;
            DimensionsMatch = dimensionsMatch;
            Message = message ?? string.Empty;
        }

        public bool IsValid { get; private set; }
        public bool XExists { get; private set; }
        public bool YExists { get; private set; }
        public bool DimensionsMatch { get; private set; }
        public string Message { get; private set; }
    }

    public sealed class EngineAnnotationSave
    {
        internal EngineAnnotationSave(string xPath, string yPath, int width, int height)
        {
            XPath = xPath;
            YPath = yPath;
            Width = width;
            Height = height;
        }

        public string XPath { get; private set; }
        public string YPath { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    }

    public sealed class AnnotationEngine : IDisposable
    {
        private readonly EngineAnnotationOptions options;
        private byte[] committedRgb;
        private byte[] workingRgb;
        private string imagePath;
        private string labelPath;
        private int width;
        private int height;
        private bool disposed;

        public AnnotationEngine(EngineAnnotationOptions source)
        {
            options = ValidateAndClone(source);
            LoadImage(options.ImagePath, options.LabelPath);
        }

        public bool Editable => options.Editable;
        public bool IsDirty => !BytesEqual(committedRgb, workingRgb);

        public EngineAnnotationImageInfo GetImageInfo()
        {
            ThrowIfDisposed();
            return new EngineAnnotationImageInfo(options.ModelName, options.Kind, imagePath, labelPath, width, height);
        }

        public void SetImage(string path)
        {
            ThrowIfDisposed();
            if (IsDirty)
                throw new InvalidOperationException("Annotation session contains unsaved changes. Save or Cancel before SetImage().");
            LoadImage(path, null);
        }

        public EngineAnnotationClassMap GetClassMap()
        {
            ThrowIfDisposed();
            var result = new byte[width * height];
            for (int i = 0; i < result.Length; i++)
            {
                int p = i * 3;
                result[i] = DecodeCommittedClass(workingRgb[p], workingRgb[p + 1], workingRgb[p + 2]);
            }
            return new EngineAnnotationClassMap(width, height, result);
        }

        public EngineAnnotationLabelImage GetLegacyLabelImage()
        {
            ThrowIfDisposed();
            return new EngineAnnotationLabelImage(width, height, CloneBytes(workingRgb));
        }

        public EngineAnnotationBinaryMask ExportBinaryMask(int threshold, bool includeMaybe, bool includeIgnore)
        {
            ThrowIfDisposed();
            if (threshold < 0 || threshold > 255) throw new ArgumentOutOfRangeException(nameof(threshold));
            var mask = new byte[width * height];
            for (int i = 0; i < mask.Length; i++)
            {
                int p = i * 3;
                byte r = workingRgb[p];
                byte g = workingRgb[p + 1];
                byte b = workingRgb[p + 2];
                bool grayIgnore = r == g && g == b && r > 0 && r <= 20;
                bool maybe = IsMaybeFamily(r, g, b);
                bool positive = Math.Max(r, Math.Max(g, b)) >= threshold && !grayIgnore;
                if (includeMaybe && maybe) positive = true;
                if (includeIgnore && grayIgnore) positive = true;
                mask[i] = positive ? (byte)255 : (byte)0;
            }
            return new EngineAnnotationBinaryMask(width, height, mask);
        }

        public void Erase(int x, int y, int rectWidth, int rectHeight)
        {
            ThrowEditable();
            if (rectWidth <= 0 || rectHeight <= 0) return;
            int left = Math.Max(0, x);
            int top = Math.Max(0, y);
            int right = Math.Min(width, x + rectWidth);
            int bottom = Math.Min(height, y + rectHeight);
            for (int yy = top; yy < bottom; yy++)
            {
                for (int xx = left; xx < right; xx++)
                {
                    int p = (yy * width + xx) * 3;
                    workingRgb[p] = 0;
                    workingRgb[p + 1] = 0;
                    workingRgb[p + 2] = 0;
                }
            }
        }

        public void ClearClass(int classId)
        {
            ThrowEditable();
            ValidateClassId(classId);
            for (int i = 0; i < width * height; i++)
            {
                int p = i * 3;
                if (DecodeColorFamily(workingRgb[p], workingRgb[p + 1], workingRgb[p + 2]) != classId)
                    continue;
                workingRgb[p] = 0;
                workingRgb[p + 1] = 0;
                workingRgb[p + 2] = 0;
            }
        }

        public void ClearAll()
        {
            ThrowEditable();
            Array.Clear(workingRgb, 0, workingRgb.Length);
        }

        public void CommitSuggestion(float[] mask, int classId, EngineAnnotationMergeMode mergeMode)
        {
            ThrowEditable();
            ValidateClassId(classId);
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            int pixels = width * height;
            if (mask.Length != pixels)
                throw new ArgumentException("Suggestion mask length must match image width * height.", nameof(mask));

            byte r, g, b;
            GetClassColor(classId, out r, out g, out b);
            for (int i = 0; i < pixels; i++)
            {
                if (!(mask[i] > 0f)) continue; // recovered AIMarkWindow mask_threshold defaults to 0
                int p = i * 3;
                bool empty = workingRgb[p] == 0 && workingRgb[p + 1] == 0 && workingRgb[p + 2] == 0;
                int existingFamily = DecodeColorFamily(workingRgb[p], workingRgb[p + 1], workingRgb[p + 2]);

                bool write;
                switch (mergeMode)
                {
                    case EngineAnnotationMergeMode.FillEmptyOnly:
                        write = empty;
                        break;
                    case EngineAnnotationMergeMode.UnionSameClass:
                        write = empty || existingFamily == classId;
                        break;
                    case EngineAnnotationMergeMode.Overwrite:
                    case EngineAnnotationMergeMode.ReplaceRegion:
                        write = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mergeMode));
                }
                if (!write) continue;
                workingRgb[p] = r;
                workingRgb[p + 1] = g;
                workingRgb[p + 2] = b;
            }
        }

        public EngineAnnotationValidation ValidateLegacyPair()
        {
            ThrowIfDisposed();
            bool xExists = File.Exists(imagePath);
            bool yExists = !string.IsNullOrWhiteSpace(labelPath) && File.Exists(labelPath);
            bool dimensionsMatch = false;
            string message;

            if (!xExists)
                message = "X image does not exist.";
            else if (!yExists)
                message = "Y label does not exist.";
            else
            {
                Size xSize = ReadImageSize(imagePath);
                Size ySize = ReadImageSize(labelPath);
                dimensionsMatch = xSize.Width == ySize.Width && xSize.Height == ySize.Height;
                message = dimensionsMatch ? "Legacy X/Y pair PASS." : "X/Y dimensions do not match.";
            }

            if (options.Kind == EngineAnnotationSampleKind.Ok && !IsBlack(workingRgb))
                return new EngineAnnotationValidation(false, xExists, yExists, dimensionsMatch,
                    "Y/OK must be a blank black label.");

            return new EngineAnnotationValidation(xExists && yExists && dimensionsMatch,
                xExists, yExists, dimensionsMatch, message);
        }

        public EngineAnnotationSave SaveLegacyTrainingPair()
        {
            ThrowEditable();
            if (options.Kind == EngineAnnotationSampleKind.Ok && !IsBlack(workingRgb))
                throw new InvalidDataException("Y/OK must remain a blank black label.");

            string fileName = Path.GetFileName(imagePath);
            EnsureLosslessLabelExtension(fileName);
            string kind = options.Kind == EngineAnnotationSampleKind.Ok ? "OK" : "NG";
            string xTarget = ResolveXSaveTarget(kind, fileName);
            string yTarget = ResolveYSaveTarget(kind, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(xTarget));
            Directory.CreateDirectory(Path.GetDirectoryName(yTarget));
            SaveX(xTarget);
            SaveY(yTarget);

            imagePath = xTarget;
            labelPath = yTarget;
            committedRgb = CloneBytes(workingRgb);
            return new EngineAnnotationSave(xTarget, yTarget, width, height);
        }

        public void Cancel()
        {
            ThrowIfDisposed();
            workingRgb = CloneBytes(committedRgb);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            committedRgb = null;
            workingRgb = null;
        }

        private void LoadImage(string sourcePath, string explicitLabelPath)
        {
            string full = RequireFile(sourcePath, "ImagePath");
            Size size = ReadImageSize(full);
            imagePath = full;
            width = size.Width;
            height = size.Height;

            labelPath = ResolveLabelPath(explicitLabelPath, Path.GetFileName(full));
            if (!string.IsNullOrEmpty(labelPath))
            {
                Size labelSize = ReadImageSize(labelPath);
                if (labelSize.Width != width || labelSize.Height != height)
                    throw new InvalidDataException("Annotation label dimensions do not match the X image: " + labelPath);
                committedRgb = ReadRgb24(labelPath, width, height);
            }
            else
            {
                committedRgb = new byte[width * height * 3];
            }
            workingRgb = CloneBytes(committedRgb);
        }

        private string ResolveLabelPath(string explicitPath, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(explicitPath))
                return RequireFile(explicitPath, "LabelPath");

            string kind = options.Kind == EngineAnnotationSampleKind.Ok ? "OK" : "NG";
            string folder = Path.Combine(options.TrainingImageRoot, options.ModelName, "Y", kind);
            if (!Directory.Exists(folder)) return string.Empty;
            string[] matches = Directory.GetFiles(folder, fileName, SearchOption.AllDirectories);
            if (matches.Length == 0) return string.Empty;
            if (matches.Length > 1)
                throw new InvalidDataException("Annotation label filename is ambiguous under Y/" + kind + ": " + fileName);
            return Path.GetFullPath(matches[0]);
        }

        private string BuildOutputFolder(string axis, string kind, string relativeSubfolder)
        {
            string baseFolder = Path.Combine(options.TrainingImageRoot, options.ModelName, axis, kind);
            string relative = NormalizeRelativeSubfolder(relativeSubfolder);
            return string.IsNullOrEmpty(relative) ? baseFolder : Path.Combine(baseFolder, relative);
        }

        private string ResolveXSaveTarget(string kind, string fileName)
        {
            if (string.IsNullOrEmpty(options.XSubfolder))
            {
                string axisRoot = BuildOutputFolder("X", kind, string.Empty);
                if (IsPathInside(imagePath, axisRoot)) return imagePath;
            }
            return Path.Combine(BuildOutputFolder("X", kind, options.XSubfolder), fileName);
        }

        private string ResolveYSaveTarget(string kind, string fileName)
        {
            if (string.IsNullOrEmpty(options.YSubfolder) && !string.IsNullOrEmpty(labelPath))
            {
                string axisRoot = BuildOutputFolder("Y", kind, string.Empty);
                if (IsPathInside(labelPath, axisRoot)) return labelPath;
            }
            return Path.Combine(BuildOutputFolder("Y", kind, options.YSubfolder), fileName);
        }

        private static bool IsPathInside(string path, string folder)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(folder)) return false;
            string fullPath = Path.GetFullPath(path);
            string fullFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(fullFolder, StringComparison.OrdinalIgnoreCase);
        }

        private void SaveX(string target)
        {
            if (PathsEqual(imagePath, target)) return;
            if (File.Exists(target))
            {
                if (FilesEqual(imagePath, target)) return;
                if (!options.AllowOverwrite) throw new IOException("X target already exists: " + target);
            }
            File.Copy(imagePath, target, true);
        }

        private void SaveY(string target)
        {
            bool targetIsLoaded = !string.IsNullOrEmpty(labelPath) && PathsEqual(labelPath, target);
            if (File.Exists(target) && !targetIsLoaded && !options.AllowOverwrite)
                throw new IOException("Y target already exists: " + target);
            if (targetIsLoaded && !IsDirty) return;
            WriteRgb24(target, width, height, workingRgb);
        }

        private static EngineAnnotationOptions ValidateAndClone(EngineAnnotationOptions source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(source.TrainingImageRoot))
                throw new ArgumentException("TrainingImageRoot is required.", nameof(source));
            if (string.IsNullOrWhiteSpace(source.ModelName))
                throw new ArgumentException("ModelName is required.", nameof(source));

            string rawModel = source.ModelName.Trim();
            if (rawModel.IndexOf(Path.DirectorySeparatorChar) >= 0 || rawModel.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
                throw new ArgumentException("ModelName must be a file/folder stem, not a path.", nameof(source));
            string model = Path.GetFileNameWithoutExtension(rawModel);
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("ModelName must be a file/folder stem.", nameof(source));

            return new EngineAnnotationOptions
            {
                TrainingImageRoot = Path.GetFullPath(source.TrainingImageRoot),
                ModelName = model,
                Kind = source.Kind,
                ImagePath = source.ImagePath,
                LabelPath = source.LabelPath,
                XSubfolder = NormalizeRelativeSubfolder(source.XSubfolder),
                YSubfolder = NormalizeRelativeSubfolder(source.YSubfolder),
                Editable = source.Editable,
                AllowOverwrite = source.AllowOverwrite
            };
        }

        private static string NormalizeRelativeSubfolder(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            string clean = value.Trim().Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (Path.IsPathRooted(clean) || clean == ".." || clean.StartsWith("..\\", StringComparison.Ordinal) ||
                clean.StartsWith("../", StringComparison.Ordinal))
                throw new ArgumentException("Annotation subfolder must stay inside X/OK|NG or Y/OK|NG.");
            return clean;
        }

        private static string RequireFile(string path, string name)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(name + " is required.", name);
            string full = Path.GetFullPath(path);
            if (!File.Exists(full)) throw new FileNotFoundException(name + " was not found.", full);
            return full;
        }

        private static Size ReadImageSize(string path)
        {
            using (var image = Image.FromFile(path)) return image.Size;
        }

        private static byte[] ReadRgb24(string path, int width, int height)
        {
            using (var source = new Bitmap(path))
            using (var converted = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                using (Graphics graphics = Graphics.FromImage(converted))
                    graphics.DrawImageUnscaled(source, 0, 0);

                var result = new byte[width * height * 3];
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = converted.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    int stride = data.Stride;
                    byte[] row = new byte[Math.Abs(stride)];
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr rowPtr = stride >= 0
                            ? IntPtr.Add(data.Scan0, y * stride)
                            : IntPtr.Add(data.Scan0, (height - 1 - y) * (-stride));
                        Marshal.Copy(rowPtr, row, 0, row.Length);
                        for (int x = 0; x < width; x++)
                        {
                            int src = x * 3;
                            int dst = (y * width + x) * 3;
                            result[dst] = row[src + 2];
                            result[dst + 1] = row[src + 1];
                            result[dst + 2] = row[src];
                        }
                    }
                }
                finally { converted.UnlockBits(data); }
                return result;
            }
        }

        private static void WriteRgb24(string path, int width, int height, byte[] rgb)
        {
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                try
                {
                    int stride = data.Stride;
                    byte[] row = new byte[Math.Abs(stride)];
                    for (int y = 0; y < height; y++)
                    {
                        Array.Clear(row, 0, row.Length);
                        for (int x = 0; x < width; x++)
                        {
                            int src = (y * width + x) * 3;
                            int dst = x * 3;
                            row[dst] = rgb[src + 2];
                            row[dst + 1] = rgb[src + 1];
                            row[dst + 2] = rgb[src];
                        }
                        IntPtr rowPtr = stride >= 0
                            ? IntPtr.Add(data.Scan0, y * stride)
                            : IntPtr.Add(data.Scan0, (height - 1 - y) * (-stride));
                        Marshal.Copy(row, 0, rowPtr, row.Length);
                    }
                }
                finally { bitmap.UnlockBits(data); }
                bitmap.Save(path, ImageFormatFor(path));
            }
        }

        private static ImageFormat ImageFormatFor(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".tif":
                case ".tiff": return ImageFormat.Tiff;
                default: return ImageFormat.Png;
            }
        }

        private static void EnsureLosslessLabelExtension(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".png" && ext != ".bmp" && ext != ".tif" && ext != ".tiff")
                throw new InvalidDataException("Legacy annotation writer requires a lossless image extension (.png/.bmp/.tif/.tiff): " + fileName);
        }

        private static byte DecodeCommittedClass(byte r, byte g, byte b)
        {
            if (r >= 128 && g <= 10 && b <= 10) return 1;
            if (g >= 128 && r <= 10 && b <= 10) return 2;
            if (b >= 128 && r <= 10 && g <= 10) return 3;
            if (r >= 128 && g >= 128 && b <= 10) return 4;
            if (r >= 128 && b >= 128 && g <= 10) return 5;
            if (g >= 128 && b >= 128 && r <= 10) return 6;
            return 0;
        }

        private static int DecodeColorFamily(byte r, byte g, byte b)
        {
            if (r > 0 && g <= 10 && b <= 10) return 1;
            if (g > 0 && r <= 10 && b <= 10) return 2;
            if (b > 0 && r <= 10 && g <= 10) return 3;
            if (r > 0 && g > 0 && b <= 10 && Math.Abs(r - g) <= 10) return 4;
            if (r > 0 && b > 0 && g <= 10 && Math.Abs(r - b) <= 10) return 5;
            if (g > 0 && b > 0 && r <= 10 && Math.Abs(g - b) <= 10) return 6;
            return 0;
        }

        private static bool IsMaybeFamily(byte r, byte g, byte b)
        {
            int family = DecodeColorFamily(r, g, b);
            int max = Math.Max(r, Math.Max(g, b));
            return family > 0 && max >= 100 && max < 128;
        }

        private static bool IsBlack(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++) if (bytes[i] != 0) return false;
            return true;
        }

        private static void ValidateClassId(int classId)
        {
            if (classId < 1 || classId > 6) throw new ArgumentOutOfRangeException(nameof(classId), "classId must be 1..6.");
        }

        private static void GetClassColor(int classId, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            switch (classId)
            {
                case 1: r = 255; break;
                case 2: g = 255; break;
                case 3: b = 255; break;
                case 4: r = 255; g = 255; break;
                case 5: r = 255; b = 255; break;
                case 6: g = 255; b = 255; break;
                default: throw new ArgumentOutOfRangeException(nameof(classId));
            }
        }

        private static bool FilesEqual(string left, string right)
        {
            var a = new FileInfo(left);
            var b = new FileInfo(right);
            if (a.Length != b.Length) return false;
            const int size = 64 * 1024;
            byte[] ba = new byte[size];
            byte[] bb = new byte[size];
            using (FileStream fa = File.OpenRead(left))
            using (FileStream fb = File.OpenRead(right))
            {
                while (true)
                {
                    int ca = fa.Read(ba, 0, ba.Length);
                    int cb = fb.Read(bb, 0, bb.Length);
                    if (ca != cb) return false;
                    if (ca == 0) return true;
                    for (int i = 0; i < ca; i++) if (ba[i] != bb[i]) return false;
                }
            }
        }

        private static bool PathsEqual(string left, string right)
            => string.Equals(Path.GetFullPath(left).TrimEnd('\\'), Path.GetFullPath(right).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);

        private static byte[] CloneBytes(byte[] source)
        {
            var result = new byte[source.Length];
            Buffer.BlockCopy(source, 0, result, 0, source.Length);
            return result;
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
            return true;
        }

        private void ThrowEditable()
        {
            ThrowIfDisposed();
            if (!Editable) throw new InvalidOperationException("Annotation session is read-only.");
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(AnnotationEngine));
        }
    }
}
