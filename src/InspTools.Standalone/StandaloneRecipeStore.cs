using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InspTools.Standalone
{
    /// <summary>
    /// Chao8Side-compatible Recipe directory store. This manages *whole recipes*,
    /// not individual AOI tool parameters (those are owned by compiled InspTools).
    /// The only destination root is this executable's local Recipe directory.
    /// </summary>
    internal static class StandaloneRecipeStore
    {
        internal static string Root
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipe"); }
        }

        internal static string[] List()
        {
            return ListFromRoot(Root);
        }

        // Browsing a source Recipe root must never create or write files there.
        internal static string[] ListFromRoot(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return new string[0];
            string root = Path.GetFullPath(directory);
            if (!Directory.Exists(root) || IsLink(root)) return new string[0];
            return Directory.GetDirectories(root)
                .Where(path => !IsLink(path) &&
                    File.Exists(Path.Combine(path, "Setting.ini")) &&
                    Enumerable.Range(1, 8).All(i =>
                        File.Exists(Path.Combine(path, "station" + i + ".ini"))))
                .Select(Path.GetFileName)
                .Where(name => name.IndexOf("_TempRecipe", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("_BackupRecipe", StringComparison.OrdinalIgnoreCase) < 0)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal static string LocalPath(string name)
        {
            CheckName(name);
            return Path.Combine(Root, name);
        }

        internal static string Validate(string name)
        {
            string path = LocalPath(name);
            if (!Directory.Exists(path) || IsLink(path))
                throw new DirectoryNotFoundException("Recipe 不存在或為連結：" + name);
            CheckStructure(path);
            return path;
        }

        internal static string Import(string sourceDirectory, string destinationName)
        {
            CheckName(destinationName);
            if (string.IsNullOrWhiteSpace(sourceDirectory))
                throw new ArgumentException("來源 Recipe 路徑不可為空");
            string source = Path.GetFullPath(sourceDirectory);
            if (!Directory.Exists(source) || IsLink(source))
                throw new DirectoryNotFoundException("來源 Recipe 不存在或是連結：" + source);
            CheckStructure(source);

            Directory.CreateDirectory(Root);
            string dest = LocalPath(destinationName);
            if (Directory.Exists(dest) || File.Exists(dest))
                throw new IOException("Recipe 已存在，為避免覆蓋不允許直接匯入：" + destinationName);
            if (Path.GetFullPath(source).TrimEnd('\\').Equals(
                Path.GetFullPath(dest).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                throw new IOException("來源與目的地不能相同");

            // Fail closed: reject reparse points in the input before any copy.
            RejectLinksRecursive(source);
            try
            {
                CopyDirectory(source, dest);
                RewriteRecipeReferences(dest, Path.GetFileName(source), destinationName);
                CheckStructure(dest);
                return dest;
            }
            catch
            {
                // The destination did not exist before this operation.
                if (Directory.Exists(dest)) Directory.Delete(dest, true);
                throw;
            }
        }

        internal static string Duplicate(string existingName, string newName)
        {
            return Import(Validate(existingName), newName);
        }

        private static void CheckStructure(string path)
        {
            if (!File.Exists(Path.Combine(path, "Setting.ini")))
                throw new InvalidDataException("Recipe 缺少 Setting.ini：" + path);
            for (int i = 1; i <= 8; i++)
            {
                if (!File.Exists(Path.Combine(path, "station" + i + ".ini")))
                    throw new InvalidDataException("Recipe 缺少 station" + i + ".ini：" + path);
            }
        }

        private static void CheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name != name.Trim() ||
                name == "." || name == ".." || name.EndsWith(".") ||
                name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                name.IndexOfAny(new[] { '/', '\\', ':' }) >= 0)
                throw new ArgumentException("無效的 Recipe 名稱：" + name);
        }

        private static bool IsLink(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }

        private static void RejectLinksRecursive(string dir)
        {
            if (IsLink(dir)) throw new IOException("不允許 Recipe 連結：" + dir);
            foreach (string item in Directory.GetFileSystemEntries(dir))
            {
                if (IsLink(item)) throw new IOException("不允許 Recipe 內含連結：" + item);
                if (Directory.Exists(item)) RejectLinksRecursive(item);
            }
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), false);
            foreach (string dir in Directory.GetDirectories(source))
                CopyDirectory(dir, Path.Combine(target, Path.GetFileName(dir)));
        }

        private static void RewriteRecipeReferences(string dest, string oldName, string newName)
        {
            if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase)) return;
            foreach (string ini in Directory.GetFiles(dest, "*.ini", SearchOption.AllDirectories))
            {
                // Chao8Side-compatible naming convention: rewrite only path segments,
                // preserve all other INI keys and existing text encoding.
                byte[] bytes = File.ReadAllBytes(ini);
                Encoding encoding = DetectEncoding(bytes);
                string original;
                using (var reader = new StreamReader(ini, encoding, true))
                    original = reader.ReadToEnd();
                string revised = ReplaceIgnoreCase(original, "\\Recipe\\" + oldName + "\\",
                    "\\Recipe\\" + newName + "\\");
                revised = ReplaceIgnoreCase(revised, "/Recipe/" + oldName + "/",
                    "/Recipe/" + newName + "/");
                if (!string.Equals(original, revised, StringComparison.Ordinal))
                    File.WriteAllText(ini, revised, encoding);
            }
        }

        private static Encoding DetectEncoding(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(true);
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) return Encoding.Unicode;
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) return Encoding.BigEndianUnicode;
            return Encoding.Default;
        }

        private static string ReplaceIgnoreCase(string input, string find, string replace)
        {
            int start = 0;
            var text = new StringBuilder();
            while (start < input.Length)
            {
                int at = input.IndexOf(find, start, StringComparison.OrdinalIgnoreCase);
                if (at < 0) break;
                text.Append(input, start, at - start).Append(replace);
                start = at + find.Length;
            }
            text.Append(input, start, input.Length - start);
            return text.ToString();
        }
    }
}
