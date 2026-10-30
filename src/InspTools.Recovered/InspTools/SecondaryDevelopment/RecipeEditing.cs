using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace InspTools.SecondaryDevelopment;

public sealed class SecondaryDevelopmentOptions
{
	public const string RecipeWriteGuardFileName = ".insptools-secondarydev-recipe-write";
	public const string RecipeWriteGuardContent = "INSPTOOLS_SECONDARY_DEVELOPMENT_ISOLATED_WRITE_V1";

	public bool AllowRecipeWrites { get; set; }
}

public sealed class RecipeParameterEntry
{
	internal RecipeParameterEntry(string section, string key, string value, int order)
	{
		Section = section ?? string.Empty;
		Key = key ?? string.Empty;
		Value = value ?? string.Empty;
		Order = order;
	}

	public string Section { get; }
	public string Key { get; }
	public string Value { get; }
	public int Order { get; }
}

public sealed class RecipeParameterSnapshot
{
	internal RecipeParameterSnapshot(int stationNumber, string recipeName, string sourcePath,
		string sourceEncoding, string sha256, IReadOnlyList<RecipeParameterEntry> entries)
	{
		StationNumber = stationNumber;
		RecipeName = recipeName ?? string.Empty;
		SourcePath = sourcePath ?? string.Empty;
		SourceEncoding = sourceEncoding ?? string.Empty;
		Sha256 = sha256 ?? string.Empty;
		Entries = entries ?? throw new ArgumentNullException(nameof(entries));
	}

	public int StationNumber { get; }
	public string RecipeName { get; }
	public string SourcePath { get; }
	public string SourceEncoding { get; }
	public string Sha256 { get; }
	public IReadOnlyList<RecipeParameterEntry> Entries { get; }
}

public sealed class RecipeParameterPatch
{
	public RecipeParameterPatch(string section, string key, string value)
	{
		Section = section ?? string.Empty;
		Key = key ?? string.Empty;
		Value = value ?? string.Empty;
	}

	public string Section { get; }
	public string Key { get; }
	public string Value { get; }
}

public sealed class RecipeParameterDiff
{
	internal RecipeParameterDiff(string section, string key, string oldValue, string newValue, bool changed)
	{
		Section = section ?? string.Empty;
		Key = key ?? string.Empty;
		OldValue = oldValue ?? string.Empty;
		NewValue = newValue ?? string.Empty;
		Changed = changed;
	}

	public string Section { get; }
	public string Key { get; }
	public string OldValue { get; }
	public string NewValue { get; }
	public bool Changed { get; }
}

public sealed class RecipePatchPreview
{
	internal RecipePatchPreview(int stationNumber, string recipeName, string sourcePath,
		string sourceEncoding, string preSha256, IReadOnlyList<RecipeParameterDiff> differences)
	{
		StationNumber = stationNumber;
		RecipeName = recipeName ?? string.Empty;
		SourcePath = sourcePath ?? string.Empty;
		SourceEncoding = sourceEncoding ?? string.Empty;
		PreSha256 = preSha256 ?? string.Empty;
		Differences = differences ?? throw new ArgumentNullException(nameof(differences));
	}

	public int StationNumber { get; }
	public string RecipeName { get; }
	public string SourcePath { get; }
	public string SourceEncoding { get; }
	public string PreSha256 { get; }
	public IReadOnlyList<RecipeParameterDiff> Differences { get; }
	public bool HasChanges => Differences.Any(d => d.Changed);
}

public sealed class RecipePatchResult
{
	internal RecipePatchResult(string transactionId, int stationNumber, string recipeName,
		string sourcePath, string preSha256, string postSha256, string backupPath,
		bool reloaded, IReadOnlyList<RecipeParameterDiff> differences)
	{
		TransactionId = transactionId ?? string.Empty;
		StationNumber = stationNumber;
		RecipeName = recipeName ?? string.Empty;
		SourcePath = sourcePath ?? string.Empty;
		PreSha256 = preSha256 ?? string.Empty;
		PostSha256 = postSha256 ?? string.Empty;
		BackupPath = backupPath ?? string.Empty;
		Reloaded = reloaded;
		Differences = differences ?? throw new ArgumentNullException(nameof(differences));
	}

	public string TransactionId { get; }
	public int StationNumber { get; }
	public string RecipeName { get; }
	public string SourcePath { get; }
	public string PreSha256 { get; }
	public string PostSha256 { get; }
	public string BackupPath { get; }
	public bool Reloaded { get; }
	public IReadOnlyList<RecipeParameterDiff> Differences { get; }
	public bool Changed => !string.Equals(PreSha256, PostSha256, StringComparison.OrdinalIgnoreCase);
}

public sealed class RecipeRollbackResult
{
	internal RecipeRollbackResult(string transactionId, int stationNumber, string recipeName,
		string restoredSha256, bool reloaded)
	{
		TransactionId = transactionId ?? string.Empty;
		StationNumber = stationNumber;
		RecipeName = recipeName ?? string.Empty;
		RestoredSha256 = restoredSha256 ?? string.Empty;
		Reloaded = reloaded;
	}

	public string TransactionId { get; }
	public int StationNumber { get; }
	public string RecipeName { get; }
	public string RestoredSha256 { get; }
	public bool Reloaded { get; }
}

internal sealed class RecipeEditTransactionState
{
	public string TransactionId;
	public int StationNumber;
	public string RecipeName;
	public string StationPath;
	public string BackupPath;
	public string PreSha256;
	public string PostSha256;
	public bool RolledBack;
}

internal sealed class RecipeIniDocument
{
	private readonly List<RecipeIniLine> lines;

	private RecipeIniDocument(byte[] sourceBytes, Encoding encoding, bool emitBom, List<RecipeIniLine> lines)
	{
		SourceBytes = sourceBytes;
		Encoding = encoding;
		EmitBom = emitBom;
		this.lines = lines;
	}

	public byte[] SourceBytes { get; }
	public Encoding Encoding { get; }
	public bool EmitBom { get; }
	public string EncodingName => Encoding is UnicodeEncoding ? "UTF-16LE" : Encoding.WebName;

	public static RecipeIniDocument Load(string path)
	{
		byte[] bytes = File.ReadAllBytes(path);
		Encoding encoding;
		bool emitBom;
		int offset;
		if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
		{
			encoding = new UnicodeEncoding(false, true, true);
			emitBom = true;
			offset = 2;
		}
		else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
		{
			encoding = new UTF8Encoding(true, true);
			emitBom = true;
			offset = 3;
		}
		else
		{
			encoding = new UTF8Encoding(false, true);
			emitBom = false;
			offset = 0;
		}

		string text = encoding.GetString(bytes, offset, bytes.Length - offset);
		return new RecipeIniDocument(bytes, encoding, emitBom, ParseLines(text));
	}

	public IReadOnlyList<RecipeParameterEntry> GetEntries()
	{
		var result = new List<RecipeParameterEntry>();
		string section = string.Empty;
		int order = 0;
		foreach (RecipeIniLine line in lines)
		{
			string content = line.Content;
			string trimmed = content.Trim();
			if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal) && trimmed.Length >= 2)
			{
				section = trimmed.Substring(1, trimmed.Length - 2).Trim();
				continue;
			}
			if (trimmed.Length == 0 || trimmed.StartsWith(";", StringComparison.Ordinal) || trimmed.StartsWith("#", StringComparison.Ordinal)) continue;
			int equalsIndex = content.IndexOf('=');
			if (equalsIndex <= 0) continue;
			string key = content.Substring(0, equalsIndex).Trim();
			if (key.Length == 0) continue;
			string rawValue = content.Substring(equalsIndex + 1);
			result.Add(new RecipeParameterEntry(section, key, rawValue.Trim(), order++));
		}
		return new ReadOnlyCollection<RecipeParameterEntry>(result);
	}

	public IReadOnlyList<RecipeParameterDiff> Preview(IReadOnlyList<RecipeParameterPatch> patches)
	{
		var entries = GetEntries();
		var diffs = new List<RecipeParameterDiff>(patches.Count);
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (RecipeParameterPatch patch in patches)
		{
			ValidatePatch(patch);
			string identity = patch.Section + "\u001f" + patch.Key;
			if (!seen.Add(identity)) throw new ArgumentException("Duplicate Recipe patch: [" + patch.Section + "] " + patch.Key + ".", nameof(patches));
			List<RecipeParameterEntry> matches = entries
				.Where(e => string.Equals(e.Section, patch.Section, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(e.Key, patch.Key, StringComparison.OrdinalIgnoreCase))
				.ToList();
			if (matches.Count == 0) throw new KeyNotFoundException("Recipe parameter was not found: [" + patch.Section + "] " + patch.Key + ".");
			if (matches.Count != 1) throw new InvalidDataException("Recipe parameter is ambiguous because it appears more than once: [" + patch.Section + "] " + patch.Key + ".");
			RecipeParameterEntry current = matches[0];
			diffs.Add(new RecipeParameterDiff(current.Section, current.Key, current.Value, patch.Value,
				!string.Equals(current.Value, patch.Value, StringComparison.Ordinal)));
		}
		return new ReadOnlyCollection<RecipeParameterDiff>(diffs);
	}

	public byte[] Apply(IReadOnlyList<RecipeParameterPatch> patches)
	{
		Preview(patches);
		var patchMap = patches.ToDictionary(p => p.Section + "\u001f" + p.Key, p => p, StringComparer.OrdinalIgnoreCase);
		string section = string.Empty;
		foreach (RecipeIniLine line in lines)
		{
			string content = line.Content;
			string trimmed = content.Trim();
			if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal) && trimmed.Length >= 2)
			{
				section = trimmed.Substring(1, trimmed.Length - 2).Trim();
				continue;
			}
			int equalsIndex = content.IndexOf('=');
			if (equalsIndex <= 0) continue;
			string key = content.Substring(0, equalsIndex).Trim();
			RecipeParameterPatch patch;
			if (!patchMap.TryGetValue(section + "\u001f" + key, out patch)) continue;
			string rawValue = content.Substring(equalsIndex + 1);
			int leadingCount = 0;
			while (leadingCount < rawValue.Length && char.IsWhiteSpace(rawValue[leadingCount])) leadingCount++;
			int trailingStart = rawValue.Length;
			while (trailingStart > leadingCount && char.IsWhiteSpace(rawValue[trailingStart - 1])) trailingStart--;
			string leading = rawValue.Substring(0, leadingCount);
			string trailing = rawValue.Substring(trailingStart);
			line.Content = content.Substring(0, equalsIndex + 1) + leading + patch.Value + trailing;
		}

		string rendered = string.Concat(lines.Select(l => l.Content + l.NewLine));
		byte[] payload = Encoding.GetBytes(rendered);
		if (!EmitBom) return payload;
		byte[] preamble = Encoding.GetPreamble();
		var result = new byte[preamble.Length + payload.Length];
		Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
		Buffer.BlockCopy(payload, 0, result, preamble.Length, payload.Length);
		return result;
	}

	private static void ValidatePatch(RecipeParameterPatch patch)
	{
		if (patch == null) throw new ArgumentNullException(nameof(patch));
		if (string.IsNullOrWhiteSpace(patch.Section)) throw new ArgumentException("Recipe patch section is required.");
		if (string.IsNullOrWhiteSpace(patch.Key)) throw new ArgumentException("Recipe patch key is required.");
		if (patch.Section.IndexOfAny(new[] { '\r', '\n', '[', ']' }) >= 0) throw new ArgumentException("Recipe patch section contains invalid characters.");
		if (patch.Key.IndexOfAny(new[] { '\r', '\n', '=' }) >= 0) throw new ArgumentException("Recipe patch key contains invalid characters.");
		if (patch.Value.IndexOfAny(new[] { '\r', '\n' }) >= 0) throw new ArgumentException("Recipe patch value must be a single line.");
		if (patch.Value.Length > 4096) throw new ArgumentException("Recipe patch value is too long.");
	}

	private static List<RecipeIniLine> ParseLines(string text)
	{
		var result = new List<RecipeIniLine>();
		int start = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '\r' && text[i] != '\n') continue;
			string content = text.Substring(start, i - start);
			string newLine;
			if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
			{
				newLine = "\r\n";
				i++;
			}
			else newLine = text[i].ToString();
			result.Add(new RecipeIniLine(content, newLine));
			start = i + 1;
		}
		if (start < text.Length) result.Add(new RecipeIniLine(text.Substring(start), string.Empty));
		else if (text.Length == 0) result.Add(new RecipeIniLine(string.Empty, string.Empty));
		return result;
	}
}

internal sealed class RecipeIniLine
{
	public RecipeIniLine(string content, string newLine)
	{
		Content = content ?? string.Empty;
		NewLine = newLine ?? string.Empty;
	}

	public string Content { get; set; }
	public string NewLine { get; }
}

internal static class SecondaryDevelopmentHash
{
	public static string Sha256(byte[] bytes)
	{
		using (SHA256 sha = SHA256.Create())
			return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
	}

	public static string Sha256(string path) => Sha256(File.ReadAllBytes(path));
}

