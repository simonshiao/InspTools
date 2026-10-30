using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.Other;

public class AutoMatchWindow : System.Windows.Window, IComponentConnector
{
	private const int MaxPublicPattern = 210;

	private const int MaxFonts = 5;

	private const int Max9Fonts = 9;

	private int LanguageType = 1;

	private CVInspection inspection;

	private Label[] LabText;

	private Label[] Lab9Text;

	private Label[] LabResult;

	private Label[] Lab9Result;

	public bool FontToolMode;

	private MatchPattern[] PublicPatternDB;

	private MatchPattern[] PrivatePatternDB;

	private string RecipeName = "";

	private int StartIndex;

	private int TotalFound;

	private int CurrentFound;

	private bool LastPatternFound;

	private int LastFoundIndex = -1;

	internal AutoMatchWindow wnd_main;

	internal TextBox txt_Input;

	internal Button btn_AutoSetup;

	internal CheckBox chk_FontAlignVertical;

	internal CheckBox chk_AutoLoadAI;

	internal CheckBox chk_AutoCombineFont;

	internal Button btn_Save;

	internal Image img_Found;

	internal Button btn_dash;

	internal Button btn_R;

	internal Button btn_Clear;

	internal Grid grd_Font5;

	internal Label lab_Result1;

	internal Label lab_Result2;

	internal Label lab_Result3;

	internal Label lab_Result4;

	internal Label lab_Result5;

	internal Label lab_Text1;

	internal Label lab_Text2;

	internal Label lab_Text3;

	internal Label lab_Text4;

	internal Label lab_Text5;

	internal Label lab_LastText;

	internal Grid grd_Font9;

	internal Label lab_Result91;

	internal Label lab_Result92;

	internal Label lab_Result93;

	internal Label lab_Text91;

	internal Label lab_Text92;

	internal Label lab_Text93;

	internal Label lab_Result94;

	internal Label lab_Result95;

	internal Label lab_Result96;

	internal Label lab_Text94;

	internal Label lab_Text95;

	internal Label lab_Text96;

	internal Label lab_Result97;

	internal Label lab_Result98;

	internal Label lab_Result99;

	internal Label lab_Text97;

	internal Label lab_Text98;

	internal Label lab_Text99;

	internal Label lab_found;

	private bool _contentLoaded;

	public AutoMatchWindow()
	{
		InitializeComponent();
		LabText = new Label[5] { lab_Text1, lab_Text2, lab_Text3, lab_Text4, lab_Text5 };
		Lab9Text = new Label[9] { lab_Text91, lab_Text92, lab_Text93, lab_Text94, lab_Text95, lab_Text96, lab_Text97, lab_Text98, lab_Text99 };
		LabResult = new Label[5] { lab_Result1, lab_Result2, lab_Result3, lab_Result4, lab_Result5 };
		Lab9Result = new Label[9] { lab_Result91, lab_Result92, lab_Result93, lab_Result94, lab_Result95, lab_Result96, lab_Result97, lab_Result98, lab_Result99 };
		for (int i = 0; i < 5; i++)
		{
			LabText[i].Content = "";
			LabResult[i].Content = "";
			LabText[i].Background = Brushes.LightGray;
		}
		for (int j = 0; j < 9; j++)
		{
			Lab9Text[j].Content = "";
			Lab9Result[j].Content = "";
			Lab9Text[j].Background = Brushes.LightGray;
		}
		txt_Input.Focus();
	}

	public void SetInspection(CVInspection station, string recipeName)
	{
		inspection = station;
		RecipeName = recipeName;
		if (inspection.paramFile.FontToolParam.FontAlignVertical)
		{
			chk_FontAlignVertical.IsChecked = true;
		}
		if (inspection.paramFile.FontToolParam.AutoCombineFontToDBSet)
		{
			chk_AutoCombineFont.Visibility = Visibility.Visible;
			chk_AutoCombineFont.IsChecked = inspection.paramFile.FontToolParam.doAutoCombineFontToDB;
		}
		else
		{
			chk_AutoCombineFont.Visibility = Visibility.Hidden;
		}
	}

	public void SetPublicDB(MatchPattern[] db, MatchPattern[] Privatedb)
	{
		PublicPatternDB = db;
		PrivatePatternDB = Privatedb;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LoadLanguage(int language)
	{
		switch (language)
		{
		case 1:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-TW"));
			break;
		case 2:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-CN"));
			break;
		case 3:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("ja-JP"));
			break;
		default:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("en-US"));
			break;
		}
		_ = AppDomain.CurrentDomain.BaseDirectory;
		LanguageType = language;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetLastText(string str)
	{
		if (str != "")
		{
			lab_LastText.Content = "目前設定 :  " + str;
		}
		else
		{
			lab_LastText.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_AutoSetup_Click(object sender, RoutedEventArgs e)
	{
		string text = txt_Input.Text;
		for (int i = 0; i < 5; i++)
		{
			LabText[i].Content = "";
			LabResult[i].Content = "";
			LabText[i].Background = Brushes.LightGray;
			LabText[i].Foreground = Brushes.Black;
		}
		for (int j = 0; j < 9; j++)
		{
			Lab9Text[j].Content = "";
			Lab9Result[j].Content = "";
			Lab9Text[j].Background = Brushes.LightGray;
			Lab9Text[j].Foreground = Brushes.Black;
		}
		if (!FontToolMode)
		{
			text.Replace(" ", string.Empty);
			string text2 = text.ToLower();
			if (text2 == "")
			{
				return;
			}
			for (int k = 0; k < text2.Length; k++)
			{
				if (k > 9)
				{
					continue;
				}
				bool flag = false;
				for (int l = 0; l < 210; l++)
				{
					if (inspection.paramFile.MatchQuickSetupParam[l].QuickSetupFinish && inspection.paramFile.MatchQuickSetupParam[l].QuickSetupName.ToLower().Contains(text2[k]))
					{
						inspection.paramFile.MatchParam[k].TrackerON = true;
						inspection.paramFile.MatchParam[k].QuickSetupIndex = l;
						inspection.paramFile.MatchParam[k].ValueChangeFlag = true;
						LabText[k].Content = text[k];
						LabText[k].Background = Brushes.LightYellow;
						LabResult[k].Content = "成功";
						LabResult[k].Foreground = Brushes.Black;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					LabText[k].Content = "";
					LabText[k].Background = Brushes.Fuchsia;
					LabResult[k].Content = "失敗";
					LabResult[k].Foreground = Brushes.Red;
				}
			}
			return;
		}
		string input = text.ToUpper();
		input = Regex.Replace(input, "\\s", "");
		txt_Input.Text = input;
		for (int m = 0; m <= 9; m++)
		{
			inspection.fontTool.FontModelReaded[m] = false;
			inspection.fontTool.FontModelOnnxType[m] = 0;
		}
		if (input.Length > 9)
		{
			MessageBox.Show("字元字數超過限制(9)", "警告");
			return;
		}
		if (input.Length <= 0)
		{
			MessageBox.Show("請輸入檢測字元", "警告");
			return;
		}
		for (int n = 0; n < input.Length; n++)
		{
			if (inspection.fontTool.GetFontIndex(input.Substring(n, 1)) == -1)
			{
				MessageBox.Show(string.Format("輸入字元包含資料庫不支援的字元({0})", input.Substring(n, 1)), "警告");
				return;
			}
		}
		if (chk_AutoCombineFont.IsChecked == false)
		{
			bool flag2 = false;
			int num = -1;
			MatchPattern[] array = ((!inspection.paramFile.FontToolParam.PublicDBUsePublicMode) ? PrivatePatternDB : PublicPatternDB);
			if (StartIndex == 0)
			{
				TotalFound = 0;
				CurrentFound = 0;
				for (int num2 = StartIndex; num2 < 210; num2++)
				{
					if (array[num2].PatternName.ToUpper() == input)
					{
						TotalFound++;
					}
				}
			}
			for (int num3 = StartIndex; num3 < 210; num3++)
			{
				if (array[num3].PatternName.ToUpper() == input)
				{
					flag2 = true;
					num = num3;
					StartIndex = num3 + 1;
					CurrentFound++;
					lab_found.Content = string.Format("{0} / {1}", CurrentFound, TotalFound);
					btn_AutoSetup.Content = "下一個";
					if (StartIndex >= 210)
					{
						btn_AutoSetup.Content = "自動模式";
						StartIndex = 0;
						LastPatternFound = false;
					}
					break;
				}
			}
			if (!flag2 && LastPatternFound)
			{
				btn_AutoSetup.Content = "自動模式";
				StartIndex = 0;
				MessageBox.Show(string.Format("已到達最後一筆", input), "警告");
				return;
			}
			if (!flag2)
			{
				btn_AutoSetup.Content = "自動模式";
				StartIndex = 0;
				MessageBox.Show(string.Format("在共用資料庫找不到輸入的字碼({0})，請先到共用資料庫設定該字碼。", input), "警告");
				return;
			}
			img_Found.Source = array[num].PatternImage.ToBitmapSource();
			if (!flag2 || num < 0)
			{
				return;
			}
			if (input.Length > 5)
			{
				grd_Font5.Visibility = Visibility.Hidden;
				grd_Font9.Visibility = Visibility.Visible;
			}
			else
			{
				grd_Font5.Visibility = Visibility.Visible;
				grd_Font9.Visibility = Visibility.Hidden;
			}
			LastPatternFound = true;
			LastFoundIndex = num;
			inspection.fontTool.PatternRectSeted = false;
			INSP_FONTTOOL_PARAM fontToolParam = inspection.paramFile.FontToolParam;
			fontToolParam.ValueChangeFlag = true;
			fontToolParam.PatternSelect = num;
			fontToolParam.FontChar1 = "";
			fontToolParam.FontChar2 = "";
			fontToolParam.FontChar3 = "";
			fontToolParam.FontChar4 = "";
			fontToolParam.FontChar5 = "";
			fontToolParam.FontChar6 = "";
			fontToolParam.FontChar7 = "";
			fontToolParam.FontChar8 = "";
			fontToolParam.FontChar9 = "";
			fontToolParam.FontAlignVertical = chk_FontAlignVertical.IsChecked.Value;
			string text3 = "";
			bool autoSetFontImage = fontToolParam.AutoSetFontImage;
			if (autoSetFontImage && fontToolParam.FontChar1.Contains("-"))
			{
				MessageBox.Show("自動切割共用庫資料庫字元功能，不支援 - 字元");
			}
			if (autoSetFontImage && !fontToolParam.FontChar1.Contains("-"))
			{
				Mat patternImage = array[num].PatternImage;
				if (patternImage != null && patternImage.Width > 0 && patternImage.Height > 0)
				{
					inspection.fontTool.AutoSetFontImage(patternImage.Clone(), input, fontToolParam.FontAlignVertical);
				}
			}
			fontToolParam.AutoLoadAI = chk_AutoLoadAI.IsChecked.Value;
			for (int num4 = 0; num4 < input.Length; num4++)
			{
				text3 = "";
				switch (num4)
				{
				case 0:
					fontToolParam.FontChar1 = input.Substring(num4, 1);
					fontToolParam.FontIndex1 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar1);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar1 + ".onnx";
					}
					fontToolParam.AIModel_Font1 = text3;
					break;
				case 1:
					fontToolParam.FontChar2 = input.Substring(num4, 1);
					fontToolParam.FontIndex2 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar2);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar2 + ".onnx";
					}
					fontToolParam.AIModel_Font2 = text3;
					break;
				case 2:
					fontToolParam.FontChar3 = input.Substring(num4, 1);
					fontToolParam.FontIndex3 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar3);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar3 + ".onnx";
					}
					fontToolParam.AIModel_Font3 = text3;
					break;
				case 3:
					fontToolParam.FontChar4 = input.Substring(num4, 1);
					fontToolParam.FontIndex4 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar4);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar4 + ".onnx";
					}
					fontToolParam.AIModel_Font4 = text3;
					break;
				case 4:
					fontToolParam.FontChar5 = input.Substring(num4, 1);
					fontToolParam.FontIndex5 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar5);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar5 + ".onnx";
					}
					fontToolParam.AIModel_Font5 = text3;
					break;
				case 5:
					fontToolParam.FontChar6 = input.Substring(num4, 1);
					fontToolParam.FontIndex6 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar6);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar6 + ".onnx";
					}
					fontToolParam.AIModel_Font6 = text3;
					break;
				case 6:
					fontToolParam.FontChar7 = input.Substring(num4, 1);
					fontToolParam.FontIndex7 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar7);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar7 + ".onnx";
					}
					fontToolParam.AIModel_Font7 = text3;
					break;
				case 7:
					fontToolParam.FontChar8 = input.Substring(num4, 1);
					fontToolParam.FontIndex8 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar8);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar8 + ".onnx";
					}
					fontToolParam.AIModel_Font8 = text3;
					break;
				case 8:
					fontToolParam.FontChar9 = input.Substring(num4, 1);
					fontToolParam.FontIndex9 = inspection.fontTool.GetFontIndex(fontToolParam.FontChar9);
					if (fontToolParam.AutoLoadAI)
					{
						text3 = fontToolParam.FontChar9 + ".onnx";
					}
					fontToolParam.AIModel_Font9 = text3;
					break;
				}
				inspection.fontTool.FontModelReaded[num4] = false;
				inspection.fontTool.FontModelOnnxType[num4] = 0;
				if (input.Length > 5)
				{
					Lab9Text[num4].Content = input.Substring(num4, 1);
				}
				else
				{
					LabText[num4].Content = input.Substring(num4, 1);
				}
				if (fontToolParam.AutoLoadAI)
				{
					if (GetAIModelIndex(text3) == 0)
					{
						if (input.Length > 5)
						{
							Lab9Result[num4].Content = "缺少模型";
						}
						else
						{
							LabResult[num4].Content = "缺少模型";
						}
					}
					else if (input.Length > 5)
					{
						Lab9Result[num4].Content = "成功";
					}
					else
					{
						LabResult[num4].Content = "成功";
					}
					continue;
				}
				Mat mat = new Mat();
				switch (num4)
				{
				case 0:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex1);
					break;
				case 1:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex2);
					break;
				case 2:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex3);
					break;
				case 3:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex4);
					break;
				case 4:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex5);
					break;
				case 5:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex6);
					break;
				case 6:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex7);
					break;
				case 7:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex8);
					break;
				case 8:
					mat = GetFontImageFromIndex(fontToolParam.FontIndex9);
					break;
				}
				if (mat.Width <= 0 || mat.Height <= 0 || mat.Channels() != 1)
				{
					if (input.Length > 5)
					{
						Lab9Result[num4].Content = "尚未設定";
						Lab9Text[num4].Foreground = Brushes.Red;
						Lab9Text[num4].Background = Brushes.LightPink;
					}
					else
					{
						LabResult[num4].Content = "尚未設定";
						LabText[num4].Foreground = Brushes.Red;
						LabText[num4].Background = Brushes.LightPink;
					}
				}
				else if (input.Length > 5)
				{
					Lab9Result[num4].Content = "成功";
				}
				else
				{
					LabResult[num4].Content = "成功";
				}
			}
			fontToolParam.AllFont = input;
			inspection.paramFile.FontToolParam = fontToolParam;
			return;
		}
		if (input.Length < 9)
		{
			MessageBox.Show("自動拼字只支援9個字!");
			chk_AutoCombineFont.IsChecked = false;
			return;
		}
		if (!inspection.paramFile.FontToolParam.AutoCombineFontToDBSet)
		{
			MessageBox.Show("自動拼字尚未設定完成!");
			chk_AutoCombineFont.IsChecked = false;
			return;
		}
		for (int num5 = 0; num5 < input.Length; num5++)
		{
			string text4 = input.Substring(num5, 1);
			Mat[] pattern = new Mat[5];
			inspection.fontTool.GetFontPattern(inspection.fontTool.GetFontIndex(text4), ref pattern);
			if (pattern[0].Width <= 0 || pattern[0].Height <= 0)
			{
				MessageBox.Show(string.Format("{0}字元尚未設定，無法使用自動拼字功能!", text4));
				chk_AutoCombineFont.IsChecked = false;
				return;
			}
		}
		inspection.paramFile.FontToolParam.AutoCombineFontCreate = false;
		int num6 = 208;
		grd_Font5.Visibility = Visibility.Hidden;
		grd_Font9.Visibility = Visibility.Visible;
		TOOL_RESULT iresult = TOOL_RESULT.EM_OK;
		inspection.paramFile.FontToolParam.AllFont = input;
		inspection.paramFile.FontToolParam.PatternSelect = num6;
		inspection.fontTool.InspectF9(ref iresult);
		MatchPattern[] array2 = ((!inspection.paramFile.FontToolParam.PublicDBUsePublicMode) ? PrivatePatternDB : PublicPatternDB);
		img_Found.Source = array2[num6].PatternImage.ToBitmapSource();
		LastPatternFound = true;
		LastFoundIndex = num6;
		inspection.fontTool.PatternRectSeted = false;
		INSP_FONTTOOL_PARAM fontToolParam2 = inspection.paramFile.FontToolParam;
		fontToolParam2.ValueChangeFlag = true;
		fontToolParam2.PatternSelect = num6;
		fontToolParam2.FontChar1 = "";
		fontToolParam2.FontChar2 = "";
		fontToolParam2.FontChar3 = "";
		fontToolParam2.FontChar4 = "";
		fontToolParam2.FontChar5 = "";
		fontToolParam2.FontChar6 = "";
		fontToolParam2.FontChar7 = "";
		fontToolParam2.FontChar8 = "";
		fontToolParam2.FontChar9 = "";
		fontToolParam2.FontAlignVertical = chk_FontAlignVertical.IsChecked.Value;
		string text5 = "";
		bool autoSetFontImage2 = fontToolParam2.AutoSetFontImage;
		if (autoSetFontImage2 && fontToolParam2.FontChar1.Contains("-"))
		{
			MessageBox.Show("自動切割共用庫資料庫字元功能，不支援 - 字元");
		}
		if (autoSetFontImage2 && !fontToolParam2.FontChar1.Contains("-"))
		{
			Mat patternImage2 = array2[208].PatternImage;
			if (patternImage2 != null && patternImage2.Width > 0 && patternImage2.Height > 0)
			{
				inspection.fontTool.AutoSetFontImage(patternImage2.Clone(), input, fontToolParam2.FontAlignVertical);
			}
		}
		fontToolParam2.AutoLoadAI = chk_AutoLoadAI.IsChecked.Value;
		for (int num7 = 0; num7 < input.Length; num7++)
		{
			text5 = "";
			switch (num7)
			{
			case 0:
				fontToolParam2.FontChar1 = input.Substring(num7, 1);
				fontToolParam2.FontIndex1 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar1);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar1 + ".onnx";
				}
				fontToolParam2.AIModel_Font1 = text5;
				break;
			case 1:
				fontToolParam2.FontChar2 = input.Substring(num7, 1);
				fontToolParam2.FontIndex2 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar2);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar2 + ".onnx";
				}
				fontToolParam2.AIModel_Font2 = text5;
				break;
			case 2:
				fontToolParam2.FontChar3 = input.Substring(num7, 1);
				fontToolParam2.FontIndex3 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar3);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar3 + ".onnx";
				}
				fontToolParam2.AIModel_Font3 = text5;
				break;
			case 3:
				fontToolParam2.FontChar4 = input.Substring(num7, 1);
				fontToolParam2.FontIndex4 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar4);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar4 + ".onnx";
				}
				fontToolParam2.AIModel_Font4 = text5;
				break;
			case 4:
				fontToolParam2.FontChar5 = input.Substring(num7, 1);
				fontToolParam2.FontIndex5 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar5);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar5 + ".onnx";
				}
				fontToolParam2.AIModel_Font5 = text5;
				break;
			case 5:
				fontToolParam2.FontChar6 = input.Substring(num7, 1);
				fontToolParam2.FontIndex6 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar6);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar6 + ".onnx";
				}
				fontToolParam2.AIModel_Font6 = text5;
				break;
			case 6:
				fontToolParam2.FontChar7 = input.Substring(num7, 1);
				fontToolParam2.FontIndex7 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar7);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar7 + ".onnx";
				}
				fontToolParam2.AIModel_Font7 = text5;
				break;
			case 7:
				fontToolParam2.FontChar8 = input.Substring(num7, 1);
				fontToolParam2.FontIndex8 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar8);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar8 + ".onnx";
				}
				fontToolParam2.AIModel_Font8 = text5;
				break;
			case 8:
				fontToolParam2.FontChar9 = input.Substring(num7, 1);
				fontToolParam2.FontIndex9 = inspection.fontTool.GetFontIndex(fontToolParam2.FontChar9);
				if (fontToolParam2.AutoLoadAI)
				{
					text5 = fontToolParam2.FontChar9 + ".onnx";
				}
				fontToolParam2.AIModel_Font9 = text5;
				break;
			}
			inspection.fontTool.FontModelReaded[num7] = false;
			inspection.fontTool.FontModelOnnxType[num7] = 0;
			if (input.Length > 5)
			{
				Lab9Text[num7].Content = input.Substring(num7, 1);
			}
			else
			{
				LabText[num7].Content = input.Substring(num7, 1);
			}
			if (fontToolParam2.AutoLoadAI)
			{
				if (GetAIModelIndex(text5) == 0)
				{
					if (input.Length > 5)
					{
						Lab9Result[num7].Content = "缺少模型";
					}
					else
					{
						LabResult[num7].Content = "缺少模型";
					}
				}
				else if (input.Length > 5)
				{
					Lab9Result[num7].Content = "成功";
				}
				else
				{
					LabResult[num7].Content = "成功";
				}
				continue;
			}
			Mat mat2 = new Mat();
			switch (num7)
			{
			case 0:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex1);
				break;
			case 1:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex2);
				break;
			case 2:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex3);
				break;
			case 3:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex4);
				break;
			case 4:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex5);
				break;
			case 5:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex6);
				break;
			case 6:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex7);
				break;
			case 7:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex8);
				break;
			case 8:
				mat2 = GetFontImageFromIndex(fontToolParam2.FontIndex9);
				break;
			}
			if (mat2.Width <= 0 || mat2.Height <= 0 || mat2.Channels() != 1)
			{
				if (input.Length > 5)
				{
					Lab9Result[num7].Content = "尚未設定";
					Lab9Text[num7].Foreground = Brushes.Red;
					Lab9Text[num7].Background = Brushes.LightPink;
				}
				else
				{
					LabResult[num7].Content = "尚未設定";
					LabText[num7].Foreground = Brushes.Red;
					LabText[num7].Background = Brushes.LightPink;
				}
			}
			else if (input.Length > 5)
			{
				Lab9Result[num7].Content = "成功";
			}
			else
			{
				LabResult[num7].Content = "成功";
			}
		}
		fontToolParam2.AllFont = input;
		inspection.paramFile.FontToolParam = fontToolParam2;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int GetAIModelIndex(string onnxfile)
	{
		try
		{
			string text = "";
			text = ((!inspection.paramFile.FontToolParam.FontDBUsePublicMode) ? string.Format("{0}Recipe\\{1}\\Font\\", AppDomain.CurrentDomain.BaseDirectory, RecipeName) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\Font\\"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string[] files = Directory.GetFiles(text, "*.onnx", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				if (Path.GetFileName(files[i]) == onnxfile)
				{
					return i + 1;
				}
			}
			return 0;
		}
		catch (Exception ex)
		{
			string text2 = "FTW07-FontToolWindow-GetAIModelIndex()";
			CVBuffer.SaveLog(text2);
			_ = text2 + ex.ToString();
			return 0;
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_dash_Click(object sender, RoutedEventArgs e)
	{
		txt_Input.Text += "-";
	}

	private void Btn_R_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		txt_Input.Text += button.Content;
	}

	private void Btn_Clear_Click(object sender, RoutedEventArgs e)
	{
		txt_Input.Text = "";
	}

	private Mat GetFontImageFromIndex(int index)
	{
		return index switch
		{
			0 => inspection.fontTool.FontMatR[0], 
			1 => inspection.fontTool.FontMatM[0], 
			2 => inspection.fontTool.FontMatC[0], 
			3 => inspection.fontTool.FontMatU[0], 
			4 => inspection.fontTool.FontMatDash[0], 
			5 => inspection.fontTool.FontMat0[0], 
			6 => inspection.fontTool.FontMat1[0], 
			7 => inspection.fontTool.FontMat2[0], 
			8 => inspection.fontTool.FontMat3[0], 
			9 => inspection.fontTool.FontMat4[0], 
			10 => inspection.fontTool.FontMat5[0], 
			11 => inspection.fontTool.FontMat6[0], 
			12 => inspection.fontTool.FontMat7[0], 
			13 => inspection.fontTool.FontMat8[0], 
			14 => inspection.fontTool.FontMat9[0], 
			15 => inspection.fontTool.FontMatX[0], 
			16 => inspection.fontTool.FontMatB[0], 
			17 => inspection.fontTool.FontMatE[0], 
			18 => inspection.fontTool.FontMatD[0], 
			19 => inspection.fontTool.FontMatA[0], 
			20 => inspection.fontTool.FontMatOther[0][0], 
			21 => inspection.fontTool.FontMatOther[1][0], 
			22 => inspection.fontTool.FontMatOther[2][0], 
			23 => inspection.fontTool.FontMatOther[3][0], 
			24 => inspection.fontTool.FontMatOther[4][0], 
			25 => inspection.fontTool.FontMatOther[5][0], 
			26 => inspection.fontTool.FontMatOther[6][0], 
			27 => inspection.fontTool.FontMatOther[7][0], 
			28 => inspection.fontTool.FontMatOther[8][0], 
			29 => inspection.fontTool.FontMatOther[9][0], 
			30 => inspection.fontTool.FontMatOther[10][0], 
			31 => inspection.fontTool.FontMatOther[11][0], 
			32 => inspection.fontTool.FontMatOther[12][0], 
			33 => inspection.fontTool.FontMatOther[13][0], 
			34 => inspection.fontTool.FontMatOther[14][0], 
			35 => inspection.fontTool.FontMatOther[15][0], 
			36 => inspection.fontTool.FontMatOther[16][0], 
			_ => inspection.fontTool.FontMat0[0], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void chk_AutoCombineFont_Checked(object sender, RoutedEventArgs e)
	{
		btn_AutoSetup.Content = "自動拼字";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/automatchwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	void IComponentConnector.InitializeComponent()
	{
		//ILSpy generated this explicit interface implementation from .override directive in InitializeComponent
		this.InitializeComponent();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			wnd_main = (AutoMatchWindow)target;
			break;
		case 2:
			txt_Input = (TextBox)target;
			break;
		case 3:
			btn_AutoSetup = (Button)target;
			btn_AutoSetup.Click += Btn_AutoSetup_Click;
			break;
		case 4:
			chk_FontAlignVertical = (CheckBox)target;
			break;
		case 5:
			chk_AutoLoadAI = (CheckBox)target;
			break;
		case 6:
			chk_AutoCombineFont = (CheckBox)target;
			chk_AutoCombineFont.Checked += chk_AutoCombineFont_Checked;
			break;
		case 7:
			btn_Save = (Button)target;
			btn_Save.Click += Button_Click;
			break;
		case 8:
			img_Found = (Image)target;
			break;
		case 9:
			btn_dash = (Button)target;
			btn_dash.Click += Btn_dash_Click;
			break;
		case 10:
			btn_R = (Button)target;
			btn_R.Click += Btn_R_Click;
			break;
		case 11:
			((Button)target).Click += Btn_R_Click;
			break;
		case 12:
			((Button)target).Click += Btn_R_Click;
			break;
		case 13:
			((Button)target).Click += Btn_R_Click;
			break;
		case 14:
			((Button)target).Click += Btn_R_Click;
			break;
		case 15:
			((Button)target).Click += Btn_R_Click;
			break;
		case 16:
			((Button)target).Click += Btn_R_Click;
			break;
		case 17:
			((Button)target).Click += Btn_R_Click;
			break;
		case 18:
			((Button)target).Click += Btn_R_Click;
			break;
		case 19:
			((Button)target).Click += Btn_R_Click;
			break;
		case 20:
			((Button)target).Click += Btn_R_Click;
			break;
		case 21:
			((Button)target).Click += Btn_R_Click;
			break;
		case 22:
			((Button)target).Click += Btn_R_Click;
			break;
		case 23:
			((Button)target).Click += Btn_R_Click;
			break;
		case 24:
			((Button)target).Click += Btn_R_Click;
			break;
		case 25:
			btn_Clear = (Button)target;
			btn_Clear.Click += Btn_Clear_Click;
			break;
		case 26:
			((Button)target).Click += Btn_R_Click;
			break;
		case 27:
			((Button)target).Click += Btn_R_Click;
			break;
		case 28:
			((Button)target).Click += Btn_R_Click;
			break;
		case 29:
			((Button)target).Click += Btn_R_Click;
			break;
		case 30:
			((Button)target).Click += Btn_R_Click;
			break;
		case 31:
			((Button)target).Click += Btn_R_Click;
			break;
		case 32:
			((Button)target).Click += Btn_R_Click;
			break;
		case 33:
			((Button)target).Click += Btn_R_Click;
			break;
		case 34:
			((Button)target).Click += Btn_R_Click;
			break;
		case 35:
			((Button)target).Click += Btn_R_Click;
			break;
		case 36:
			((Button)target).Click += Btn_R_Click;
			break;
		case 37:
			((Button)target).Click += Btn_R_Click;
			break;
		case 38:
			((Button)target).Click += Btn_R_Click;
			break;
		case 39:
			((Button)target).Click += Btn_R_Click;
			break;
		case 40:
			((Button)target).Click += Btn_R_Click;
			break;
		case 41:
			((Button)target).Click += Btn_R_Click;
			break;
		case 42:
			((Button)target).Click += Btn_R_Click;
			break;
		case 43:
			((Button)target).Click += Btn_R_Click;
			break;
		case 44:
			((Button)target).Click += Btn_R_Click;
			break;
		case 45:
			((Button)target).Click += Btn_R_Click;
			break;
		case 46:
			((Button)target).Click += Btn_R_Click;
			break;
		case 47:
			((Button)target).Click += Btn_R_Click;
			break;
		case 48:
			((Button)target).Click += Btn_R_Click;
			break;
		case 49:
			((Button)target).Click += Btn_R_Click;
			break;
		case 50:
			((Button)target).Click += Btn_R_Click;
			break;
		case 51:
			((Button)target).Click += Btn_R_Click;
			break;
		case 52:
			((Button)target).Click += Btn_R_Click;
			break;
		case 53:
			((Button)target).Click += Btn_R_Click;
			break;
		case 54:
			((Button)target).Click += Btn_R_Click;
			break;
		case 55:
			grd_Font5 = (Grid)target;
			break;
		case 56:
			lab_Result1 = (Label)target;
			break;
		case 57:
			lab_Result2 = (Label)target;
			break;
		case 58:
			lab_Result3 = (Label)target;
			break;
		case 59:
			lab_Result4 = (Label)target;
			break;
		case 60:
			lab_Result5 = (Label)target;
			break;
		case 61:
			lab_Text1 = (Label)target;
			break;
		case 62:
			lab_Text2 = (Label)target;
			break;
		case 63:
			lab_Text3 = (Label)target;
			break;
		case 64:
			lab_Text4 = (Label)target;
			break;
		case 65:
			lab_Text5 = (Label)target;
			break;
		case 66:
			lab_LastText = (Label)target;
			break;
		case 67:
			grd_Font9 = (Grid)target;
			break;
		case 68:
			lab_Result91 = (Label)target;
			break;
		case 69:
			lab_Result92 = (Label)target;
			break;
		case 70:
			lab_Result93 = (Label)target;
			break;
		case 71:
			lab_Text91 = (Label)target;
			break;
		case 72:
			lab_Text92 = (Label)target;
			break;
		case 73:
			lab_Text93 = (Label)target;
			break;
		case 74:
			lab_Result94 = (Label)target;
			break;
		case 75:
			lab_Result95 = (Label)target;
			break;
		case 76:
			lab_Result96 = (Label)target;
			break;
		case 77:
			lab_Text94 = (Label)target;
			break;
		case 78:
			lab_Text95 = (Label)target;
			break;
		case 79:
			lab_Text96 = (Label)target;
			break;
		case 80:
			lab_Result97 = (Label)target;
			break;
		case 81:
			lab_Result98 = (Label)target;
			break;
		case 82:
			lab_Result99 = (Label)target;
			break;
		case 83:
			lab_Text97 = (Label)target;
			break;
		case 84:
			lab_Text98 = (Label)target;
			break;
		case 85:
			lab_Text99 = (Label)target;
			break;
		case 86:
			lab_found = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
