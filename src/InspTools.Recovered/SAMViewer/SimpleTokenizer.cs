using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SAMViewer;

internal class SimpleTokenizer
{
	private static SimpleTokenizer theSingleton;

	private Dictionary<int, string> byte_encoder;

	private Dictionary<string, int> byte_decoder;

	private Dictionary<string, int> encoder = new Dictionary<string, int>();

	private Dictionary<int, string> decoder = new Dictionary<int, string>();

	private Dictionary<(string, string), int> bpe_ranks = new Dictionary<(string, string), int>();

	private Dictionary<string, string> cache = new Dictionary<string, string>();

	private Regex pat;

	private int contextLength = 77;

	public static SimpleTokenizer Instance()
	{
		if (theSingleton == null)
		{
			theSingleton = new SimpleTokenizer();
		}
		return theSingleton;
	}

	protected SimpleTokenizer()
	{
		Init();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Init()
	{
		byte_encoder = BytesToUnicode();
		byte_decoder = byte_encoder.ToDictionary((KeyValuePair<int, string> kvp) => kvp.Value, (KeyValuePair<int, string> kvp) => kvp.Key);
		List<(string, string)> list = LoadBPEMerges(default_bpe());
		List<string> list2 = byte_encoder.Values.ToList();
		foreach (string item in byte_encoder.Values.ToList())
		{
			list2.Add(item + "</w>");
		}
		foreach (var (text, text2) in list)
		{
			list2.Add(text + text2);
		}
		list2.AddRange(new List<string>
		{
			"<|startoftext|>",
			"<|endoftext|>"
		});
		for (int num = 0; num < list2.Count; num++)
		{
			encoder[list2[num]] = num;
		}
		decoder = encoder.ToDictionary((KeyValuePair<string, int> kvp) => kvp.Value, (KeyValuePair<string, int> kvp) => kvp.Key);
		bpe_ranks = list.Select(((string, string) merge, int index) => new { merge, index }).ToDictionary(item => item.merge, item => item.index);
		cache = new Dictionary<string, string>
		{
			{
				"<|startoftext|>",
				"<|startoftext|>"
			},
			{
				"<|endoftext|>",
				"<|endoftext|>"
			}
		};
		pat = new Regex(" <\\| startoftext\\|>|<\\| endoftext\\|>| 's|'t | 're|'ve | 'm|'ll | 'd|[\\p{L}]+|[\\p{N}]|[^\\s\\p{L}\\p{N}]+", RegexOptions.IgnoreCase);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public List<long> tolikenlize(string textpromot)
	{
		int num = encoder["<|startoftext|>"];
		int num2 = encoder["<|endoftext|>"];
		List<string> obj = new List<string> { textpromot };
		List<long> list = new List<long>();
		foreach (string item in obj)
		{
			list.Add(num);
			list.AddRange(Encode(item));
			list.Add(num2);
		}
		if (list.Count > contextLength)
		{
			list.RemoveRange(contextLength, list.Count - contextLength);
			list[contextLength - 1] = num2;
		}
		else
		{
			long[] collection = new long[contextLength - list.Count];
			list.AddRange(collection);
		}
		return list;
	}

	private List<long> Encode(string text)
	{
		List<long> list = new List<long>();
		text = whitespace_clean(basic_clean(text)).ToLower();
		foreach (Match item in Regex.Matches(text, pat.ToString()))
		{
			string token = string.Join("", item.Value.Select((char c) => byte_encoder[c]));
			string[] array = bpe(token).Split(' ');
			foreach (string key in array)
			{
				list.Add(encoder[key]);
			}
		}
		return list;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string Decode(List<int> tokens)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int token in tokens)
		{
			stringBuilder.Append(decoder[token]);
		}
		string text = stringBuilder.ToString();
		List<byte> list = new List<byte>();
		string text2 = text;
		foreach (char c in text2)
		{
			list.Add((byte)byte_decoder[c.ToString()]);
		}
		byte[] bytes = list.ToArray();
		return Encoding.UTF8.GetString(bytes).Replace("</w>", " ");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string bpe(string token)
	{
		if (cache.ContainsKey(token))
		{
			return cache[token];
		}
		List<string> list = new List<string>();
		for (int i = 0; i < token.Length - 1; i++)
		{
			list.Add(token[i].ToString());
		}
		list.Add(token[token.Length - 1] + "</w>");
		HashSet<(string, string)> pairs = GetPairs(list);
		if (pairs.Count == 0)
		{
			return token + "</w>";
		}
		while (true)
		{
			var (text, text2) = pairs.OrderBy(((string, string) pair) => (!bpe_ranks.ContainsKey(pair)) ? double.PositiveInfinity : ((double)bpe_ranks[pair])).First();
			if (!bpe_ranks.ContainsKey((text, text2)))
			{
				break;
			}
			List<string> list2 = new List<string>();
			int num = 0;
			while (num < list.Count)
			{
				try
				{
					int num2 = list.IndexOf(text, num);
					list2.AddRange(list.GetRange(num, num2 - num));
					num = num2;
				}
				catch
				{
					list2.AddRange(list.GetRange(num, list.Count - num));
					break;
				}
				if (list[num] == text && num < list.Count - 1 && list[num + 1] == text2)
				{
					list2.Add(text + text2);
					num += 2;
				}
				else
				{
					list2.Add(list[num]);
					num++;
				}
			}
			list = list2;
			if (list.Count == 1)
			{
				break;
			}
			pairs = GetPairs(list2);
		}
		string text3 = string.Join(" ", list);
		cache[token] = text3;
		return text3;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string default_bpe()
	{
		return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bpe_simple_vocab_16e6.txt";
	}

	private List<(string, string)> LoadBPEMerges(string bpePath)
	{
		List<(string, string)> list = new List<(string, string)>();
		using FileStream stream = File.OpenRead(bpePath);
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		string[] array = streamReader.ReadToEnd().Split('\n');
		int num = 1;
		int num2 = 48895;
		foreach (string item in (IEnumerable<string>)new ArraySegment<string>(array, num, num2 - num)/*cast due to constrained. prefix*/)
		{
			string[] array2 = item.Split();
			list.Add((array2[0], array2[1]));
		}
		return list;
	}

	private Dictionary<int, string> BytesToUnicode()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 33; i <= 126; i++)
		{
			list.Add(i);
			list2.Add(i);
		}
		for (int j = 161; j <= 172; j++)
		{
			list.Add(j);
			list2.Add(j);
		}
		for (int k = 174; k <= 255; k++)
		{
			list.Add(k);
			list2.Add(k);
		}
		int num = 0;
		for (int l = 0; l < 256; l++)
		{
			if (!list.Contains(l))
			{
				list.Add(l);
				list2.Add(256 + num);
				num++;
			}
		}
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		for (int m = 0; m < list.Count; m++)
		{
			dictionary.Add(list[m], ((char)list2[m]).ToString());
		}
		return dictionary;
	}

	private HashSet<(string, string)> GetPairs(List<string> word)
	{
		HashSet<(string, string)> hashSet = new HashSet<(string, string)>();
		string item = word[0];
		for (int i = 1; i < word.Count; i++)
		{
			string text = word[i].ToString();
			hashSet.Add((item, text));
			item = text;
		}
		return hashSet;
	}

	private string HtmlDecode(string text)
	{
		return WebUtility.HtmlDecode(text);
	}

	private string basic_clean(string text)
	{
		text = HtmlDecode(text);
		return text.Trim();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string whitespace_clean(string text)
	{
		text = Regex.Replace(text, "\\s+", " ");
		text = text.Trim();
		return text;
	}
}
