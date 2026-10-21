using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InspTools.Core
{
    public sealed class EngineRecipeEntry
    {
        internal EngineRecipeEntry(string section, string key, string value, int order)
        {
            Section = section ?? string.Empty;
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
            Order = order;
        }

        public string Section { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
        public int Order { get; private set; }
    }

    public sealed class EngineRecipeSnapshot
    {
        internal EngineRecipeSnapshot(
            string recipeName,
            int station,
            string sourcePath,
            string sourceEncoding,
            string sha256,
            IReadOnlyList<EngineRecipeEntry> entries)
        {
            RecipeName = recipeName ?? string.Empty;
            Station = station;
            SourcePath = sourcePath ?? string.Empty;
            SourceEncoding = sourceEncoding ?? string.Empty;
            Sha256 = sha256 ?? string.Empty;
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public string RecipeName { get; private set; }
        public int Station { get; private set; }
        public string SourcePath { get; private set; }
        public string SourceEncoding { get; private set; }
        public string Sha256 { get; private set; }
        public IReadOnlyList<EngineRecipeEntry> Entries { get; private set; }
    }

    internal sealed class RecipeDocument
    {
        private readonly List<EngineRecipeEntry> entries;

        private RecipeDocument(byte[] sourceBytes, Encoding encoding, List<EngineRecipeEntry> entries)
        {
            SourceBytes = sourceBytes;
            Encoding = encoding;
            this.entries = entries;
        }

        internal byte[] SourceBytes { get; private set; }
        internal Encoding Encoding { get; private set; }
        internal string EncodingName
        {
            get
            {
                if (Encoding is UnicodeEncoding) return "UTF-16LE";
                return Encoding.WebName;
            }
        }

        internal IReadOnlyList<EngineRecipeEntry> Entries
            => new ReadOnlyCollection<EngineRecipeEntry>(entries);

        internal static RecipeDocument Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Recipe station path is required.", nameof(path));
            string fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath)) throw new FileNotFoundException("Recipe station file was not found.", fullPath);

            byte[] bytes = File.ReadAllBytes(fullPath);
            Encoding encoding;
            int offset;

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                encoding = new UnicodeEncoding(false, true, true);
                offset = 2;
            }
            else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                encoding = new UTF8Encoding(true, true);
                offset = 3;
            }
            else
            {
                encoding = DetectNoBomEncoding(bytes);
                offset = 0;
            }

            string text = encoding.GetString(bytes, offset, bytes.Length - offset);
            return new RecipeDocument(bytes, encoding, ParseEntries(text));
        }

        private static Encoding DetectNoBomEncoding(byte[] bytes)
        {
            var utf8 = new UTF8Encoding(false, true);
            try
            {
                utf8.GetString(bytes);
                return utf8;
            }
            catch (DecoderFallbackException)
            {
                // 舊機台 Recipe 若不是 UTF-8，Windows 常見為 CP950/Big5。
                return Encoding.GetEncoding(950, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            }
        }

        private static List<EngineRecipeEntry> ParseEntries(string text)
        {
            var result = new List<EngineRecipeEntry>();
            string section = string.Empty;
            int order = 0;

            using (var reader = new StringReader(text ?? string.Empty))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith(";", StringComparison.Ordinal) || trimmed.StartsWith("#", StringComparison.Ordinal))
                        continue;

                    if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal) && trimmed.Length >= 2)
                    {
                        section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        continue;
                    }

                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex <= 0) continue;

                    string key = line.Substring(0, equalsIndex).Trim();
                    if (key.Length == 0) continue;

                    string value = line.Substring(equalsIndex + 1).Trim();
                    result.Add(new EngineRecipeEntry(section, key, value, order++));
                }
            }

            return result;
        }
    }

    internal static class CoreHash
    {
        internal static string Sha256(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes ?? Array.Empty<byte>());
                var builder = new StringBuilder(hash.Length * 2);
                foreach (byte value in hash) builder.Append(value.ToString("X2"));
                return builder.ToString();
            }
        }
    }
}
