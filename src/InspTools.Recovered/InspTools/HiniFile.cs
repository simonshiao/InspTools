using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IniParser.Model;
using OpenCvSharp;

namespace InspTools;

public class HiniFile
{
	private string IniFilePath;

	private IniData iData;

	private string theFile;

	private Dictionary<string, string> dictionary = new Dictionary<string, string>();

	public string CommentDelimiter { get; set; }

	public string TheFile
	{
		get
		{
			return theFile;
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		set
		{
			theFile = null;
			dictionary.Clear();
			if (!File.Exists(value))
			{
				return;
			}
			theFile = value;
			using StreamReader streamReader = new StreamReader(theFile, Encoding.Default);
			string arg = "";
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				text = text.Trim();
				if (text.Length == 0 || (!string.IsNullOrEmpty(CommentDelimiter) && text.StartsWith(CommentDelimiter)))
				{
					continue;
				}
				if (text.StartsWith("[") && text.Contains("]"))
				{
					int num = text.IndexOf(']');
					arg = text.Substring(1, num - 1).Trim();
				}
				else
				{
					if (!text.Contains("="))
					{
						continue;
					}
					int num2 = text.IndexOf('=');
					string arg2 = text.Substring(0, num2).Trim();
					string text2 = text.Substring(num2 + 1).Trim();
					string text3 = string.Format("[{0}]{1}", arg, arg2).ToLower();
					if (text2.StartsWith("\"") && text2.EndsWith("\""))
					{
						text2 = text2.Substring(1, text2.Length - 2);
					}
					if (dictionary.ContainsKey(text3))
					{
						num2 = 1;
						string key;
						do
						{
							key = string.Format("{0}~{1}", text3, ++num2);
						}
						while (dictionary.ContainsKey(key));
						dictionary.Add(key, text2);
					}
					else
					{
						dictionary.Add(text3, text2);
					}
				}
			}
		}
	}

	public string this[string section, string key] => GetValue(section, key);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

	public HiniFile(string file, string commentDelimiter = ";")
	{
		CommentDelimiter = commentDelimiter;
		TheFile = file;
		IniFilePath = file;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public HiniFile()
	{
		CommentDelimiter = ";";
	}

	public void SetiData(IniData idata)
	{
		iData = idata;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool TryGetValue(string section, string key, out string value)
	{
		string text = ((!section.StartsWith("[")) ? string.Format("[{0}]{1}", section, key) : string.Format("{0}{1}", section, key));
		return dictionary.TryGetValue(text.ToLower(), out value);
	}

	public string GetValue(string section, string key, string defaultValue = "")
	{
		if (!TryGetValue(section, key, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public int GetInteger(string section, string key, int defaultValue = 0, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		if (!TryGetValue(section, key, out var value))
		{
			return defaultValue;
		}
		if (!int.TryParse(value, out var result))
		{
			if (!double.TryParse(value, out var result2))
			{
				return defaultValue;
			}
			result = (int)result2;
		}
		if (result < minValue)
		{
			result = minValue;
		}
		if (result > maxValue)
		{
			result = maxValue;
		}
		return result;
	}

	public double GetDouble(string section, string key, double defaultValue = 0.0, double minValue = double.MinValue, double maxValue = double.MaxValue)
	{
		if (!TryGetValue(section, key, out var value))
		{
			return defaultValue;
		}
		if (!double.TryParse(value, out var result))
		{
			return defaultValue;
		}
		if (result < minValue)
		{
			result = minValue;
		}
		if (result > maxValue)
		{
			result = maxValue;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool GetBoolean(string section, string key, bool defaultValue = false)
	{
		if (!TryGetValue(section, key, out var value))
		{
			return defaultValue;
		}
		if (value != "0")
		{
			return !value.StartsWith("f", ignoreCase: true, null);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string[] GetAllValues(string section, string key)
	{
		string text = ((!section.StartsWith("[")) ? string.Format("[{0}]{1}", section, key).ToLower() : string.Format("{0}{1}", section, key).ToLower());
		if (!dictionary.TryGetValue(text, out var value))
		{
			return null;
		}
		List<string> list = new List<string>();
		list.Add(value);
		int num = 1;
		while (true)
		{
			string key2 = string.Format("{0}~{1}", text, ++num);
			if (!dictionary.TryGetValue(key2, out value))
			{
				break;
			}
			list.Add(value);
		}
		return list.ToArray();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Rect ReadRect(string section, string key, int DefaultValue = 0)
	{
		new Rect(0, 0, 50, 50);
		int result = 0;
		int result2 = 0;
		int result3 = 50;
		int result4 = 50;
		string key2 = key + "_X";
		if (!TryGetValue(section, key2, out var value))
		{
			result = 0;
		}
		else if (!int.TryParse(value, out result))
		{
			result = (double.TryParse(value, out var result5) ? ((int)result5) : 0);
		}
		key2 = key + "_Y";
		if (!TryGetValue(section, key2, out value))
		{
			result2 = 0;
		}
		else if (!int.TryParse(value, out result2))
		{
			result2 = (double.TryParse(value, out var result6) ? ((int)result6) : 0);
		}
		key2 = key + "_W";
		if (!TryGetValue(section, key2, out value))
		{
			result3 = 0;
		}
		else if (!int.TryParse(value, out result3))
		{
			result3 = (double.TryParse(value, out var result7) ? ((int)result7) : 0);
		}
		key2 = key + "_H";
		if (!TryGetValue(section, key2, out value))
		{
			result4 = 0;
		}
		else if (!int.TryParse(value, out result4))
		{
			result4 = (double.TryParse(value, out var result8) ? ((int)result8) : 0);
		}
		return new Rect(result, result2, result3, result4);
	}

	public void WriteBool(string section, string key, bool value)
	{
		if (iData == null)
		{
			string value2 = Convert.ToString(value);
			WritePrivateProfileString(section, key, value2, IniFilePath);
		}
		else
		{
			iData[section][key] = value.ToString();
		}
	}

	public void WriteString(string section, string key, string value)
	{
		if (iData == null)
		{
			WritePrivateProfileString(section, key, value, IniFilePath);
		}
		else
		{
			iData[section][key] = value;
		}
	}

	public void WriteInt(string section, string key, int value)
	{
		if (iData == null)
		{
			string value2 = Convert.ToString(value);
			WritePrivateProfileString(section, key, value2, IniFilePath);
		}
		else
		{
			iData[section][key] = value.ToString();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void WriteRect(string section, string key, Rect rect)
	{
		string text = "";
		if (iData == null)
		{
			string key2 = key + "_X";
			text = Convert.ToString(rect.X);
			WritePrivateProfileString(section, key2, text, IniFilePath);
			key2 = key + "_Y";
			text = Convert.ToString(rect.Y);
			WritePrivateProfileString(section, key2, text, IniFilePath);
			key2 = key + "_W";
			text = Convert.ToString(rect.Width);
			WritePrivateProfileString(section, key2, text, IniFilePath);
			key2 = key + "_H";
			text = Convert.ToString(rect.Height);
			WritePrivateProfileString(section, key2, text, IniFilePath);
		}
		else
		{
			string key2 = key + "_X";
			text = Convert.ToString(rect.X);
			iData[section][key2] = text;
			key2 = key + "_Y";
			text = Convert.ToString(rect.Y);
			iData[section][key2] = text;
			key2 = key + "_W";
			text = Convert.ToString(rect.Width);
			iData[section][key2] = text;
			key2 = key + "_H";
			text = Convert.ToString(rect.Height);
			iData[section][key2] = text;
		}
	}

	public void WriteDouble(string section, string key, double value)
	{
		if (iData == null)
		{
			string value2 = Convert.ToString(value);
			WritePrivateProfileString(section, key, value2, IniFilePath);
		}
		else
		{
			iData[section][key] = value.ToString();
		}
	}

	public long DeleteSection(string section)
	{
		return WritePrivateProfileString(section, null, null, IniFilePath);
	}

	public long DeleteKey(string section, string key)
	{
		return WritePrivateProfileString(section, key, null, IniFilePath);
	}

	public void ReSetiData()
	{
		iData = null;
	}
}
