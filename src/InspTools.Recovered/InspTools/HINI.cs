using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp;

namespace InspTools;

public class HINI
{
	public string IniFilePath = "setup.ini";

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

	public void SetIniPath(string path)
	{
		IniFilePath = path;
	}

	public string GetIniPath()
	{
		return IniFilePath;
	}

	public bool ReadBool(string section, string key, bool DefaultValue = false)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = Convert.ToString(DefaultValue);
		GetPrivateProfileString(section, key, def, stringBuilder, 255, IniFilePath);
		if (bool.TryParse(stringBuilder.ToString(), out var result))
		{
			return result;
		}
		return DefaultValue;
	}

	public string ReadString(string section, string key, string Defalut = "")
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		GetPrivateProfileString(section, key, Defalut, stringBuilder, 255, IniFilePath);
		return stringBuilder.ToString();
	}

	public int ReadInt(string section, string key, int DefaultValue = 0)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = Convert.ToString(DefaultValue);
		GetPrivateProfileString(section, key, def, stringBuilder, 255, IniFilePath);
		if (int.TryParse(stringBuilder.ToString(), out var result))
		{
			return result;
		}
		return DefaultValue;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Rect ReadRect(string section, string key, int DefaultValue = 0)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		bool[] array = new bool[4];
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = "0";
		string key2 = key + "_X";
		GetPrivateProfileString(section, key2, def, stringBuilder, 255, IniFilePath);
		array[0] = int.TryParse(stringBuilder.ToString(), out var result);
		num = result;
		key2 = key + "_Y";
		GetPrivateProfileString(section, key2, def, stringBuilder, 255, IniFilePath);
		array[1] = int.TryParse(stringBuilder.ToString(), out result);
		num2 = result;
		def = Convert.ToString(DefaultValue);
		key2 = key + "_W";
		GetPrivateProfileString(section, key2, def, stringBuilder, 255, IniFilePath);
		array[2] = int.TryParse(stringBuilder.ToString(), out result);
		num3 = result;
		key2 = key + "_H";
		num4 = GetPrivateProfileString(section, key2, def, stringBuilder, 255, IniFilePath);
		array[3] = int.TryParse(stringBuilder.ToString(), out result);
		num4 = result;
		Rect result2 = default(Rect);
		if (!array[0] || !array[1] || !array[2] || !array[3])
		{
			result2.X = 0;
			result2.Y = 0;
			result2.Width = DefaultValue;
			result2.Height = DefaultValue;
			return result2;
		}
		result2.X = num;
		result2.Y = num2;
		result2.Width = num3;
		result2.Height = num4;
		return result2;
	}

	public double ReadDouble(string section, string key, double DefaultValue = 0.0)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = Convert.ToString(DefaultValue);
		GetPrivateProfileString(section, key, def, stringBuilder, 255, IniFilePath);
		if (double.TryParse(stringBuilder.ToString(), out var result))
		{
			return result;
		}
		return DefaultValue;
	}

	public void WriteBool(string section, string key, bool value)
	{
		string value2 = Convert.ToString(value);
		WritePrivateProfileString(section, key, value2, IniFilePath);
	}

	public void WriteString(string section, string key, string value)
	{
		WritePrivateProfileString(section, key, value, IniFilePath);
	}

	public void WriteInt(string section, string key, int value)
	{
		string value2 = Convert.ToString(value);
		WritePrivateProfileString(section, key, value2, IniFilePath);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void WriteRect(string section, string key, Rect rect)
	{
		string key2 = key + "_X";
		string value = Convert.ToString(rect.X);
		WritePrivateProfileString(section, key2, value, IniFilePath);
		key2 = key + "_Y";
		value = Convert.ToString(rect.Y);
		WritePrivateProfileString(section, key2, value, IniFilePath);
		key2 = key + "_W";
		value = Convert.ToString(rect.Width);
		WritePrivateProfileString(section, key2, value, IniFilePath);
		key2 = key + "_H";
		value = Convert.ToString(rect.Height);
		WritePrivateProfileString(section, key2, value, IniFilePath);
	}

	public void WriteDouble(string section, string key, double value)
	{
		string value2 = Convert.ToString(value);
		WritePrivateProfileString(section, key, value2, IniFilePath);
	}

	public long DeleteSection(string section)
	{
		return WritePrivateProfileString(section, null, null, IniFilePath);
	}

	public long DeleteKey(string section, string key)
	{
		return WritePrivateProfileString(section, key, null, IniFilePath);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public HINI()
	{
	}
}
