using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using Microsoft.Win32;
using OpenCvSharp;

namespace InspTools.Other;

public class ParamCopyWindow : System.Windows.Window, IComponentConnector
{
	private bool inited;

	public CVInspection[] Station = new CVInspection[8];

	private int LanguageType = 1;

	private ParamFile OtherIni = new ParamFile();

	private ThroughputParamFile Settingini = new ThroughputParamFile();

	private HiniFile StationINI;

	private HiniFile SettingINI;

	private bool OtherIniOK;

	public bool UpdateComDigits;

	public double[] ComDigits = new double[24];

	public string[] ComDigitsName = new string[24];

	private User_Permissions User = User_Permissions.USER_Administrator;

	public bool ReloadAI;

	private List<string> SelectedRecipe = new List<string>();

	private CheckBox[] StationSelect;

	private CheckBox[] RCStation;

	private DispatcherTimer CopyTimer = new DispatcherTimer();

	internal ComboBox cb_StationCopyTo;

	internal Button btn_ReDraw;

	internal ComboBox cb_StationCopyFrom;

	internal Button btn_StationCopy;

	internal Button btn_ReDraw_Copy;

	internal CheckBox cb_SCopy_Locate;

	internal CheckBox cb_SCopy_SelectWay;

	internal CheckBox cb_SCopy_HLocate;

	internal CheckBox cb_SCopy_AdvLocate;

	internal CheckBox cb_SCopy_MeasureH;

	internal CheckBox cb_SCopy_VLocate;

	internal CheckBox cb_SCopy_Smooth;

	internal CheckBox cb_SCopy_Mask;

	internal CheckBox cb_SCopy_Binary;

	internal CheckBox cb_SCopy_Difference;

	internal CheckBox cb_SCopy_BlockAvg;

	internal CheckBox cb_SCopy_LineAnalysis;

	internal CheckBox cb_SCopy_MeasureW;

	internal CheckBox cb_SCopy_ColorReference;

	internal CheckBox cb_SCopy_SelectAll;

	internal CheckBox cb_SCopy_UnSelectAll;

	internal CheckBox cb_SCopy_PatternMatch;

	internal CheckBox cb_SCopy_PatternDB;

	internal CheckBox cb_SCopy_SelectFace;

	internal CheckBox cb_SCopy_AdvMeasure;

	internal CheckBox cb_SCopy_EnhanceTool;

	internal CheckBox cb_CommonPara;

	internal CheckBox cb_SCopy_AIEnhanceTool;

	internal GroupBox gb_TrackerCopy;

	internal ComboBox cb_ToolCopyTo;

	internal Button btn_ReDraw1;

	internal ComboBox cb_ToolCopyFrom;

	internal Button btn_ReDraw_Copy1;

	internal ComboBox cb_ToolCopySelect;

	internal ComboBox cb_TCopyToolIndexTo;

	internal ComboBox cb_TCopyToolIndexFrom;

	internal Button btn_ToolsCopy;

	internal Button btn_ReDraw1_Copy;

	internal Button btn_ReDraw_Copy2;

	internal Button btn_ReDraw1_Copy1;

	internal ComboBox cb_SwitchToolIndex2;

	internal ComboBox cb_SwitchToolIndex1;

	internal Button btn_ToolsSwitch;

	internal Button btn_ReDraw1_Copy2;

	internal Button btn_ReDraw_Copy3;

	internal Button btn_ReDraw1_Copy3;

	internal GroupBox gb_StationCopy;

	internal ComboBox cb_RecipeCopyTo;

	internal Button btn_ReDraw2;

	internal ComboBox cb_RecipeCopyFrom;

	internal Button btn_ReDraw_Copy4;

	internal Button btn_RecipeCopy;

	internal CheckBox cb_Station1;

	internal CheckBox cb_Station2;

	internal CheckBox cb_Station3;

	internal CheckBox cb_Station4;

	internal CheckBox cb_Station5;

	internal CheckBox cb_Station6;

	internal CheckBox cb_Station7;

	internal CheckBox cb_Station8;

	internal RadioButton rad_CopyAll;

	internal RadioButton rad_CopyName;

	internal Button btn_ReDraw2_Copy;

	internal Button btn_ReDraw_Copy5;

	internal CheckBox cb_Station9;

	internal CheckBox cb_Station10;

	internal GroupBox gb_RecipeCopy;

	internal ComboBox cb_RC_Sour;

	internal TabItem tab_PublicPara;

	internal CheckBox cb_RC_StationPos;

	internal CheckBox cb_RC_FlowPos;

	internal CheckBox cb_RC_PublicPara;

	internal Button btn_PublicPara_Copy;

	internal TabItem tab_StationPara;

	internal CheckBox cb_RC_S1;

	internal CheckBox cb_RC_S2;

	internal CheckBox cb_RC_S3;

	internal CheckBox cb_RC_S4;

	internal CheckBox cb_RC_S5;

	internal CheckBox cb_RC_S6;

	internal CheckBox cb_RC_S7;

	internal CheckBox cb_RC_S8;

	internal CheckBox cb_RC_S9;

	internal CheckBox cb_RC_S10;

	internal Button btn_StationPara_Copy;

	internal CheckBox cb_RC_LightPara;

	internal CheckBox cb_RC_CameraPara;

	internal CheckBox cb_RC_PixelPerUm;

	internal CheckBox cb_RC_LocateTool;

	internal ComboBox cb_RC_Tools;

	internal ComboBox cb_RC_ToolIndex;

	internal TabItem tab_CustomPara;

	internal TextBox txt_RC_TagName;

	internal TextBox txt_RC_ItemName;

	internal ComboBox cb_RC_Station;

	internal Button btn_CustomPara_Copy;

	internal Button btn_RecipeCopyDest;

	internal TextBox txt_RC_Dest;

	internal Grid grd_Loading;

	internal Label lab_Loading;

	private bool _contentLoaded;

	public ParamCopyWindow(User_Permissions permissions)
	{
		InitializeComponent();
		inited = true;
		User = permissions;
		UpdateComDigits = false;
		UpdateRecipeCombobox();
		StationSelect = new CheckBox[8] { cb_Station1, cb_Station2, cb_Station3, cb_Station4, cb_Station5, cb_Station6, cb_Station7, cb_Station8 };
		RCStation = new CheckBox[8] { cb_RC_S1, cb_RC_S2, cb_RC_S3, cb_RC_S4, cb_RC_S5, cb_RC_S6, cb_RC_S7, cb_RC_S8 };
		if (User < User_Permissions.USER_Administrator)
		{
			gb_StationCopy.Visibility = Visibility.Hidden;
		}
		CopyTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
		CopyTimer.Tick += CopyTimer_Tick;
		CopyTimer.Stop();
		grd_Loading.Visibility = Visibility.Hidden;
		cb_Station9.Visibility = Visibility.Hidden;
		cb_Station10.Visibility = Visibility.Hidden;
		cb_RC_S9.Visibility = Visibility.Hidden;
		cb_RC_S10.Visibility = Visibility.Hidden;
	}

	public void SetStation(CVInspection[] station)
	{
		for (int i = 0; i < 8; i++)
		{
			Station[i] = station[i];
		}
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
	private void UpdateRecipeCombobox()
	{
		string[] directories = Directory.GetDirectories(Environment.CurrentDirectory + "\\recipe\\");
		ArrayList arrayList = new ArrayList();
		string[] array = directories;
		foreach (string path in array)
		{
			arrayList.Add(Path.GetFileNameWithoutExtension(path));
		}
		cb_RecipeCopyFrom.Items.Clear();
		cb_RecipeCopyTo.Items.Clear();
		cb_RC_Sour.Items.Clear();
		foreach (object item in arrayList)
		{
			cb_RecipeCopyFrom.Items.Add(item.ToString());
			cb_RecipeCopyTo.Items.Add(item.ToString());
			cb_RC_Sour.Items.Add(item.ToString());
		}
		cb_RecipeCopyFrom.SelectedIndex = -1;
		cb_RecipeCopyTo.SelectedIndex = -1;
		cb_RC_Sour.SelectedIndex = -1;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_StationCopy_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = cb_StationCopyFrom.SelectedIndex;
		int selectedIndex2 = cb_StationCopyTo.SelectedIndex;
		string text = (cb_StationCopyFrom.Items[selectedIndex] as ComboBoxItem).Content.ToString();
		string text2 = (cb_StationCopyTo.Items[selectedIndex2] as ComboBoxItem).Content.ToString();
		if (cb_StationCopyFrom.SelectedIndex == cb_StationCopyTo.SelectedIndex)
		{
			if (LanguageType == 0)
			{
				MessageBox.Show("Select same Station!", "Warning - " + text + "Copy to" + text2, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("選擇相同站別!", "警告 - " + text + "拷貝到" + text2, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("选择相同站别!", "警告 - " + text + "拷贝到" + text2, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
			}
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected tools will be overwrite!", "Warning - " + text + "Copy to" + text2, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("所有选择的工具将被覆盖", "警告 - " + text + "拷贝到" + text2, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : MessageBox.Show("所有選擇的工具將被覆蓋", "警告 - " + text + "拷貝到" + text2, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		if (selectedIndex < 8)
		{
			if (cb_SCopy_Locate.IsChecked == true)
			{
				Station[selectedIndex2].paramFile.LocateParam = Station[selectedIndex].paramFile.LocateParam;
				Station[selectedIndex2].paramFile.LocateToolON = Station[selectedIndex].paramFile.LocateToolON;
				Station[selectedIndex2].paramFile.LocateParam.ValueChangeFlag = true;
			}
			if (cb_SCopy_SelectWay.IsChecked == true)
			{
				Station[selectedIndex2].paramFile.SelectWayParam = Station[selectedIndex].paramFile.SelectWayParam;
				Station[selectedIndex2].paramFile.SelectWayON = Station[selectedIndex].paramFile.SelectWayON;
				Station[selectedIndex2].paramFile.SelectWayParam.ValueChangeFlag = true;
			}
			if (cb_SCopy_SelectFace.IsChecked == true)
			{
				Station[selectedIndex2].paramFile.SelectFaceON = Station[selectedIndex].paramFile.SelectFaceON;
				for (int i = 0; i < 4; i++)
				{
					Station[selectedIndex2].paramFile.SelectFaceParam[i] = Station[selectedIndex].paramFile.SelectFaceParam[i];
					Station[selectedIndex2].paramFile.SelectFaceParam[i].ValueChangeFlag = true;
				}
			}
			for (int j = 0; j < 6; j++)
			{
				if (cb_SCopy_EnhanceTool.IsChecked == true)
				{
					Station[selectedIndex2].paramFile.EnhanceParam[j] = Station[selectedIndex].paramFile.EnhanceParam[j];
					Station[selectedIndex2].paramFile.EnhanceParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_AIEnhanceTool.IsChecked == true)
			{
				Station[selectedIndex2].paramFile.AIEnhanceParam = Station[selectedIndex].paramFile.AIEnhanceParam;
				Station[selectedIndex2].aiEnhanceTools.ResetModelRead();
				Station[selectedIndex2].paramFile.AIEnhanceParam.ValueChangeFlag = true;
				ReloadAI = true;
			}
			for (int k = 0; k < 20; k++)
			{
				if (cb_SCopy_VLocate.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.AdvLocateToolON = true;
					}
					if (k >= 2 && Station[selectedIndex].paramFile.VSLocateParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.VSLocateParam[k] = Station[selectedIndex].paramFile.VSLocateParam[k];
						Station[selectedIndex2].paramFile.VSLocateParam[k].ValueChangeFlag = true;
					}
					else if (k >= 2 && !Station[selectedIndex].paramFile.VSLocateParam[k].TrackerON && Station[selectedIndex].paramFile.VSLocateParam[k].TrackerON != Station[selectedIndex2].paramFile.VSLocateParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.VSLocateParam[k] = Station[selectedIndex].paramFile.VSLocateParam[k];
						Station[selectedIndex2].paramFile.VSLocateParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_HLocate.IsChecked == true)
				{
					if (k >= 2 && Station[selectedIndex].paramFile.HSLocateParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.HSLocateParam[k] = Station[selectedIndex].paramFile.HSLocateParam[k];
						Station[selectedIndex2].paramFile.HSLocateParam[k].ValueChangeFlag = true;
					}
					else if (k >= 2 && !Station[selectedIndex].paramFile.HSLocateParam[k].TrackerON && Station[selectedIndex].paramFile.HSLocateParam[k].TrackerON != Station[selectedIndex2].paramFile.HSLocateParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.HSLocateParam[k] = Station[selectedIndex].paramFile.HSLocateParam[k];
						Station[selectedIndex2].paramFile.HSLocateParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_AdvLocate.IsChecked == true)
				{
					if (k >= 1 && Station[selectedIndex].paramFile.AdvLocateParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.AdvLocateParam[k] = Station[selectedIndex].paramFile.AdvLocateParam[k];
						Station[selectedIndex2].paramFile.AdvLocateParam[k].ValueChangeFlag = true;
					}
					else if (k >= 1 && !Station[selectedIndex].paramFile.AdvLocateParam[k].TrackerON && Station[selectedIndex].paramFile.AdvLocateParam[k].TrackerON != Station[selectedIndex2].paramFile.AdvLocateParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.AdvLocateParam[k] = Station[selectedIndex].paramFile.AdvLocateParam[k];
						Station[selectedIndex2].paramFile.AdvLocateParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_AdvMeasure.IsChecked == true)
				{
					if (Station[selectedIndex].paramFile.AdvLocateMParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.AdvLocateMParam[k] = Station[selectedIndex].paramFile.AdvLocateMParam[k];
						Station[selectedIndex2].paramFile.AdvLocateMParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.AdvLocateMParam[k].TrackerON != Station[selectedIndex2].paramFile.AdvLocateMParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.AdvLocateMParam[k] = Station[selectedIndex].paramFile.AdvLocateMParam[k];
						Station[selectedIndex2].paramFile.AdvLocateMParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_MeasureW.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.MeasureToolON = Station[selectedIndex].paramFile.MeasureToolON;
					}
					if (Station[selectedIndex].paramFile.MeaWidthParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MeaWidthParam[k] = Station[selectedIndex].paramFile.MeaWidthParam[k];
						Station[selectedIndex2].paramFile.MeaWidthParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.MeaWidthParam[k].TrackerON != Station[selectedIndex2].paramFile.MeaWidthParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MeaWidthParam[k] = Station[selectedIndex].paramFile.MeaWidthParam[k];
						Station[selectedIndex2].paramFile.MeaWidthParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_MeasureH.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.MeasureToolON = Station[selectedIndex].paramFile.MeasureToolON;
					}
					if (Station[selectedIndex].paramFile.MeaHeightParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MeaHeightParam[k] = Station[selectedIndex].paramFile.MeaHeightParam[k];
						Station[selectedIndex2].paramFile.MeaHeightParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.MeaHeightParam[k].TrackerON != Station[selectedIndex2].paramFile.MeaHeightParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MeaHeightParam[k] = Station[selectedIndex].paramFile.MeaHeightParam[k];
						Station[selectedIndex2].paramFile.MeaHeightParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_Mask.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.MaskToolON = Station[selectedIndex].paramFile.MaskToolON;
					}
					if (Station[selectedIndex].paramFile.MaskParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MaskParam[k] = Station[selectedIndex].paramFile.MaskParam[k];
						Station[selectedIndex2].paramFile.MaskParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.MaskParam[k].TrackerON != Station[selectedIndex2].paramFile.MaskParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MaskParam[k] = Station[selectedIndex].paramFile.MaskParam[k];
						Station[selectedIndex2].paramFile.MaskParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_ColorReference.IsChecked == true)
				{
					Station[selectedIndex2].paramFile.ColorReferenceParam[k] = Station[selectedIndex].paramFile.ColorReferenceParam[k];
					Station[selectedIndex2].paramFile.ColorReferenceParam[k].ValueChangeFlag = true;
				}
				if (cb_SCopy_Smooth.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.SmoothToolON = Station[selectedIndex].paramFile.SmoothToolON;
					}
					if (Station[selectedIndex].paramFile.SmoothParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.SmoothParam[k] = Station[selectedIndex].paramFile.SmoothParam[k];
						Station[selectedIndex2].paramFile.SmoothParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.SmoothParam[k].TrackerON != Station[selectedIndex2].paramFile.SmoothParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.SmoothParam[k] = Station[selectedIndex].paramFile.SmoothParam[k];
						Station[selectedIndex2].paramFile.SmoothParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_BlockAvg.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.BlockAvgToolON = Station[selectedIndex].paramFile.BlockAvgToolON;
					}
					if (Station[selectedIndex].paramFile.BlockAvgParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.BlockAvgParam[k] = Station[selectedIndex].paramFile.BlockAvgParam[k];
						Station[selectedIndex2].paramFile.BlockAvgParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.BlockAvgParam[k].TrackerON != Station[selectedIndex2].paramFile.BlockAvgParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.BlockAvgParam[k] = Station[selectedIndex].paramFile.BlockAvgParam[k];
						Station[selectedIndex2].paramFile.BlockAvgParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_Difference.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.DifferenceToolON = Station[selectedIndex].paramFile.DifferenceToolON;
					}
					if (Station[selectedIndex].paramFile.DifferenceParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.DifferenceParam[k] = Station[selectedIndex].paramFile.DifferenceParam[k];
						Station[selectedIndex2].paramFile.DifferenceParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.DifferenceParam[k].TrackerON != Station[selectedIndex2].paramFile.DifferenceParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.DifferenceParam[k] = Station[selectedIndex].paramFile.DifferenceParam[k];
						Station[selectedIndex2].paramFile.DifferenceParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_LineAnalysis.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.LineAnalysisToolON = Station[selectedIndex].paramFile.LineAnalysisToolON;
					}
					if (Station[selectedIndex].paramFile.LineAnalysisParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.LineAnalysisParam[k] = Station[selectedIndex].paramFile.LineAnalysisParam[k];
						Station[selectedIndex2].paramFile.LineAnalysisParam[k].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.LineAnalysisParam[k].TrackerON != Station[selectedIndex2].paramFile.LineAnalysisParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.LineAnalysisParam[k] = Station[selectedIndex].paramFile.LineAnalysisParam[k];
						Station[selectedIndex2].paramFile.LineAnalysisParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_PatternMatch.IsChecked == true)
				{
					if (k == 0)
					{
						Station[selectedIndex2].paramFile.MatchToolON = Station[selectedIndex].paramFile.MatchToolON;
					}
					if (Station[selectedIndex].paramFile.MatchParam[k].TrackerON)
					{
						Station[selectedIndex2].paramFile.MatchParam[k] = Station[selectedIndex].paramFile.MatchParam[k];
						Station[selectedIndex2].paramFile.MatchParam[k].ValueChangeFlag = true;
					}
				}
				if (cb_SCopy_PatternDB.IsChecked == true)
				{
					Station[selectedIndex2].paramFile.patternDB[k] = Station[selectedIndex].paramFile.patternDB[k];
					if (!Station[selectedIndex2].paramFile.patternDB[k].PatternImage.Empty())
					{
						string text3 = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + Station[selectedIndex].RecipeName + "\\Pattern\\";
						Station[selectedIndex2].paramFile.patternDB[k].PatternPath = string.Format("{0}S{1}P{2}-{3}.bmp", text3, selectedIndex2 + 1, k + 1, Station[selectedIndex2].paramFile.patternDB[k].PatternChannel);
						Cv2.ImWrite(Station[selectedIndex2].paramFile.patternDB[k].PatternPath, Station[selectedIndex2].paramFile.patternDB[k].PatternImage);
						Station[selectedIndex2].paramFile.patternDB[k].ValueChangeFlag = true;
					}
				}
			}
			for (int l = 0; l < 30; l++)
			{
				if (cb_SCopy_Binary.IsChecked == true)
				{
					if (l == 0)
					{
						Station[selectedIndex2].paramFile.BinaryToolON = Station[selectedIndex].paramFile.BinaryToolON;
					}
					if (Station[selectedIndex].paramFile.BinaryParam[l].TrackerON)
					{
						Station[selectedIndex2].paramFile.BinaryParam[l] = Station[selectedIndex].paramFile.BinaryParam[l];
						Station[selectedIndex2].paramFile.BinaryParam[l].ValueChangeFlag = true;
					}
					else if (Station[selectedIndex].paramFile.BinaryParam[l].TrackerON != Station[selectedIndex2].paramFile.BinaryParam[l].TrackerON)
					{
						Station[selectedIndex2].paramFile.BinaryParam[l] = Station[selectedIndex].paramFile.BinaryParam[l];
						Station[selectedIndex2].paramFile.BinaryParam[l].ValueChangeFlag = true;
					}
				}
			}
			if (cb_CommonPara.IsChecked == true)
			{
				Station[selectedIndex2].paramFile.ThroughputFile.GetComDigis(ref ComDigits, ref ComDigitsName);
			}
			if (LanguageType == 0)
			{
				MessageBox.Show("Copy Finish!", "Finish - " + text + "Copy to" + text2, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("站別拷貝完成!", "完成 - " + text + "拷貝到" + text2, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("站別拷貝完成!", "完成 - " + text + "拷貝到" + text2, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
		}
		else if (OtherIniOK)
		{
			CopyFromOtherRecipe(selectedIndex2);
		}
		if (cb_CommonPara.IsChecked == true)
		{
			UpdateComDigits = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyFromOtherRecipe(int dst)
	{
		if (cb_SCopy_Locate.IsChecked == true)
		{
			Station[dst].paramFile.LocateParam = OtherIni.LocateParam;
			Station[dst].paramFile.LocateToolON = OtherIni.LocateToolON;
			Station[dst].paramFile.LocateParam.ValueChangeFlag = true;
		}
		if (cb_SCopy_SelectWay.IsChecked == true)
		{
			Station[dst].paramFile.SelectWayParam = OtherIni.SelectWayParam;
			Station[dst].paramFile.SelectWayON = OtherIni.SelectWayON;
			Station[dst].paramFile.SelectWayParam.ValueChangeFlag = true;
		}
		for (int i = 0; i < 6; i++)
		{
			if (cb_SCopy_EnhanceTool.IsChecked == true)
			{
				Station[dst].paramFile.EnhanceParam[i] = OtherIni.EnhanceParam[i];
				Station[dst].paramFile.EnhanceParam[i].ValueChangeFlag = true;
			}
		}
		if (cb_SCopy_AIEnhanceTool.IsChecked == true)
		{
			Station[dst].paramFile.AIEnhanceParam = OtherIni.AIEnhanceParam;
			Station[dst].aiEnhanceTools.ResetModelRead();
			Station[dst].paramFile.AIEnhanceParam.ValueChangeFlag = true;
			ReloadAI = true;
		}
		for (int j = 0; j < 20; j++)
		{
			if (cb_SCopy_VLocate.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.AdvLocateToolON = true;
				}
				if (j >= 2 && OtherIni.VSLocateParam[j].TrackerON)
				{
					Station[dst].paramFile.VSLocateParam[j] = OtherIni.VSLocateParam[j];
					Station[dst].paramFile.VSLocateParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_HLocate.IsChecked == true && j >= 2 && OtherIni.HSLocateParam[j].TrackerON)
			{
				Station[dst].paramFile.HSLocateParam[j] = OtherIni.HSLocateParam[j];
				Station[dst].paramFile.HSLocateParam[j].ValueChangeFlag = true;
			}
			if (cb_SCopy_AdvLocate.IsChecked == true && j >= 1 && OtherIni.AdvLocateParam[j].TrackerON)
			{
				Station[dst].paramFile.AdvLocateParam[j] = OtherIni.AdvLocateParam[j];
				Station[dst].paramFile.AdvLocateParam[j].ValueChangeFlag = true;
			}
			if (cb_SCopy_AdvMeasure.IsChecked == true && OtherIni.AdvLocateMParam[j].TrackerON)
			{
				Station[dst].paramFile.AdvLocateMParam[j] = OtherIni.AdvLocateMParam[j];
				Station[dst].paramFile.AdvLocateMParam[j].ValueChangeFlag = true;
			}
			if (cb_SCopy_MeasureW.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.MeasureToolON = OtherIni.MeasureToolON;
				}
				if (OtherIni.MeaWidthParam[j].TrackerON)
				{
					Station[dst].paramFile.MeaWidthParam[j] = OtherIni.MeaWidthParam[j];
					Station[dst].paramFile.MeaWidthParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_MeasureH.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.MeasureToolON = OtherIni.MeasureToolON;
				}
				if (OtherIni.MeaHeightParam[j].TrackerON)
				{
					Station[dst].paramFile.MeaHeightParam[j] = OtherIni.MeaHeightParam[j];
					Station[dst].paramFile.MeaHeightParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_Mask.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.MaskToolON = OtherIni.MaskToolON;
				}
				if (OtherIni.MaskParam[j].TrackerON)
				{
					Station[dst].paramFile.MaskParam[j] = OtherIni.MaskParam[j];
					Station[dst].paramFile.MaskParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_ColorReference.IsChecked == true)
			{
				Station[dst].paramFile.ColorReferenceParam[j] = OtherIni.ColorReferenceParam[j];
				Station[dst].paramFile.ColorReferenceParam[j].ValueChangeFlag = true;
			}
			if (cb_SCopy_Smooth.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.SmoothToolON = OtherIni.SmoothToolON;
				}
				if (OtherIni.SmoothParam[j].TrackerON)
				{
					Station[dst].paramFile.SmoothParam[j] = OtherIni.SmoothParam[j];
					Station[dst].paramFile.SmoothParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_BlockAvg.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.BlockAvgToolON = OtherIni.BlockAvgToolON;
				}
				if (OtherIni.BlockAvgParam[j].TrackerON)
				{
					Station[dst].paramFile.BlockAvgParam[j] = OtherIni.BlockAvgParam[j];
					Station[dst].paramFile.BlockAvgParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_Difference.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.DifferenceToolON = OtherIni.DifferenceToolON;
				}
				if (OtherIni.DifferenceParam[j].TrackerON)
				{
					Station[dst].paramFile.DifferenceParam[j] = OtherIni.DifferenceParam[j];
					Station[dst].paramFile.DifferenceParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_LineAnalysis.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.LineAnalysisToolON = OtherIni.LineAnalysisToolON;
				}
				if (OtherIni.LineAnalysisParam[j].TrackerON)
				{
					Station[dst].paramFile.LineAnalysisParam[j] = OtherIni.LineAnalysisParam[j];
					Station[dst].paramFile.LineAnalysisParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_PatternMatch.IsChecked == true)
			{
				if (j == 0)
				{
					Station[dst].paramFile.MatchToolON = OtherIni.MatchToolON;
				}
				if (OtherIni.MatchParam[j].TrackerON)
				{
					Station[dst].paramFile.MatchParam[j] = OtherIni.MatchParam[j];
					Station[dst].paramFile.MatchParam[j].ValueChangeFlag = true;
				}
			}
			if (cb_SCopy_PatternDB.IsChecked == true)
			{
				Station[dst].paramFile.patternDB[j] = OtherIni.patternDB[j];
				if (!Station[dst].paramFile.patternDB[j].PatternImage.Empty())
				{
					string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + Station[dst].RecipeName + "\\Pattern\\";
					Station[dst].paramFile.patternDB[j].PatternPath = string.Format("{0}S{1}P{2}-{3}.bmp", text, dst + 1, j + 1, Station[dst].paramFile.patternDB[j].PatternChannel);
					Cv2.ImWrite(Station[dst].paramFile.patternDB[j].PatternPath, Station[dst].paramFile.patternDB[j].PatternImage);
					Station[dst].paramFile.patternDB[j].ValueChangeFlag = true;
				}
			}
		}
		for (int k = 0; k < 30; k++)
		{
			if (cb_SCopy_Binary.IsChecked == true)
			{
				if (k == 0)
				{
					Station[dst].paramFile.BinaryToolON = OtherIni.BinaryToolON;
				}
				if (OtherIni.BinaryParam[k].TrackerON)
				{
					Station[dst].paramFile.BinaryParam[k] = OtherIni.BinaryParam[k];
					Station[dst].paramFile.BinaryParam[k].ValueChangeFlag = true;
				}
			}
		}
		if (LanguageType == 0)
		{
			MessageBox.Show("Copy Finish!", "Finish", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
		}
		else if (LanguageType == 1)
		{
			MessageBox.Show("站別拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
		}
		else
		{
			MessageBox.Show("站別拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_ToolCopyFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!inited)
		{
			return;
		}
		ComboBox comboBox = (ComboBox)sender;
		if (cb_ToolCopyFrom.SelectedIndex == 8)
		{
			if (comboBox.Uid == "0")
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Ini files (*.ini,*.INI)|*.ini;*.INI|All files (*.*)|*.*";
				if (openFileDialog.ShowDialog() == true)
				{
					string fileName = Path.GetFileName(openFileDialog.FileName);
					string text = string.Format(arg0: Path.GetDirectoryName(openFileDialog.FileName), format: "{0}\\setting.ini");
					if (File.Exists(text))
					{
						SettingINI = new HiniFile(text, ";");
						Settingini.SetINI(SettingINI);
						Settingini.ReadSizeRecordSetupParam();
						Settingini.GetComDigis(ref ComDigits, ref ComDigitsName);
					}
					if (!fileName.Contains("station"))
					{
						OtherIniOK = false;
						if (LanguageType == 0)
						{
							MessageBox.Show("Open Ini file Error!", "Warring", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
						}
						else if (LanguageType == 1)
						{
							MessageBox.Show("INI檔案格式錯誤!", "Warring", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
						}
						else
						{
							MessageBox.Show("INI档案格式错误!", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
						}
						return;
					}
					StationINI = new HiniFile(openFileDialog.FileName, ";");
					OtherIni.SetINI(StationINI);
					OtherIni.ReadAllParam();
					OtherIniOK = true;
				}
			}
			int selectedIndex = cb_ToolCopyTo.SelectedIndex;
			if (selectedIndex != -1)
			{
				int selectedIndex2 = cb_ToolCopySelect.SelectedIndex;
				UpdateCopyToolSelectItem(8, selectedIndex, selectedIndex2);
				UpdateSwitchToolSelectItem(8, selectedIndex, selectedIndex2);
			}
		}
		else
		{
			int selectedIndex3 = cb_ToolCopyFrom.SelectedIndex;
			int selectedIndex4 = cb_ToolCopyTo.SelectedIndex;
			if (selectedIndex3 != -1 && selectedIndex4 != -1)
			{
				int selectedIndex5 = cb_ToolCopySelect.SelectedIndex;
				UpdateCopyToolSelectItem(selectedIndex3, selectedIndex4, selectedIndex5);
				UpdateSwitchToolSelectItem(selectedIndex3, selectedIndex4, selectedIndex5);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateSwitchToolSelectItem(int src, int dst, int tool)
	{
		if (src == 8)
		{
			return;
		}
		int selectedIndex = cb_SwitchToolIndex1.SelectedIndex;
		int selectedIndex2 = cb_SwitchToolIndex2.SelectedIndex;
		cb_SwitchToolIndex1.Items.Clear();
		cb_SwitchToolIndex2.Items.Clear();
		if (tool <= 1)
		{
			for (int i = 2; i < 20; i++)
			{
				if (tool == 0)
				{
					cb_SwitchToolIndex1.Items.Add(i + 1 + " : " + Station[src].paramFile.VSLocateParam[i].TrackerName);
					cb_SwitchToolIndex2.Items.Add(i + 1 + " : " + Station[dst].paramFile.VSLocateParam[i].TrackerName);
				}
				else
				{
					cb_SwitchToolIndex1.Items.Add(i + 1 + " : " + Station[src].paramFile.HSLocateParam[i].TrackerName);
					cb_SwitchToolIndex2.Items.Add(i + 1 + " : " + Station[dst].paramFile.HSLocateParam[i].TrackerName);
				}
			}
		}
		else if (tool == 2)
		{
			for (int j = 1; j < 20; j++)
			{
				cb_SwitchToolIndex1.Items.Add(j + 1 + " : " + Station[src].paramFile.AdvLocateParam[j].TrackerName);
				cb_SwitchToolIndex2.Items.Add(j + 1 + " : " + Station[dst].paramFile.AdvLocateParam[j].TrackerName);
			}
		}
		else
		{
			for (int k = 0; k < 20; k++)
			{
				if (tool == 3)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.MeaWidthParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.MeaWidthParam[k].TrackerName);
				}
				if (tool == 4)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.MeaHeightParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.MeaHeightParam[k].TrackerName);
				}
				if (tool == 6)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.ColorReferenceParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.ColorReferenceParam[k].TrackerName);
				}
				if (tool == 7)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.SmoothParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.SmoothParam[k].TrackerName);
				}
				if (tool == 8)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.BlockAvgParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.BlockAvgParam[k].TrackerName);
				}
				if (tool == 9)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.DifferenceParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.DifferenceParam[k].TrackerName);
				}
				if (tool == 10)
				{
					cb_SwitchToolIndex1.Items.Add(k + 1 + " : " + Station[src].paramFile.LineAnalysisParam[k].TrackerName);
					cb_SwitchToolIndex2.Items.Add(k + 1 + " : " + Station[dst].paramFile.LineAnalysisParam[k].TrackerName);
				}
			}
			for (int l = 0; l < 30; l++)
			{
				if (tool == 5)
				{
					cb_SwitchToolIndex1.Items.Add(l + 1 + " : " + Station[src].paramFile.BinaryParam[l].TrackerName);
					cb_SwitchToolIndex2.Items.Add(l + 1 + " : " + Station[dst].paramFile.BinaryParam[l].TrackerName);
				}
				if (tool == 12)
				{
					cb_SwitchToolIndex1.Items.Add(l + 1 + " : " + Station[src].paramFile.MaskParam[l].TrackerName);
					cb_SwitchToolIndex2.Items.Add(l + 1 + " : " + Station[dst].paramFile.MaskParam[l].TrackerName);
				}
			}
			for (int m = 0; m < 40; m++)
			{
				if (tool == 11)
				{
					cb_SwitchToolIndex1.Items.Add(m + 1 + " : " + Station[src].paramFile.AdvLocateMParam[m].TrackerName);
					cb_SwitchToolIndex2.Items.Add(m + 1 + " : " + Station[dst].paramFile.AdvLocateMParam[m].TrackerName);
				}
			}
		}
		if (selectedIndex < cb_SwitchToolIndex1.Items.Count)
		{
			cb_SwitchToolIndex1.SelectedIndex = selectedIndex;
		}
		else
		{
			cb_SwitchToolIndex1.SelectedIndex = 0;
		}
		if (selectedIndex2 < cb_SwitchToolIndex2.Items.Count)
		{
			cb_SwitchToolIndex2.SelectedIndex = selectedIndex2;
		}
		else
		{
			cb_SwitchToolIndex2.SelectedIndex = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateCopyToolSelectItem(int src, int dst, int tool)
	{
		int selectedIndex = cb_TCopyToolIndexFrom.SelectedIndex;
		int selectedIndex2 = cb_TCopyToolIndexTo.SelectedIndex;
		CVInspection cVInspection = null;
		if (src == 8)
		{
			cVInspection = new CVInspection();
			cVInspection.paramFile = OtherIni;
		}
		else
		{
			cVInspection = Station[src];
		}
		cb_TCopyToolIndexFrom.Items.Clear();
		cb_TCopyToolIndexTo.Items.Clear();
		if (tool <= 1)
		{
			for (int i = 2; i < 20; i++)
			{
				if (tool == 0)
				{
					cb_TCopyToolIndexFrom.Items.Add(i + 1 + " : " + cVInspection.paramFile.VSLocateParam[i].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(i + 1 + " : " + Station[dst].paramFile.VSLocateParam[i].TrackerName);
				}
				else
				{
					cb_TCopyToolIndexFrom.Items.Add(i + 1 + " : " + cVInspection.paramFile.HSLocateParam[i].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(i + 1 + " : " + Station[dst].paramFile.HSLocateParam[i].TrackerName);
				}
			}
		}
		else if (tool == 2)
		{
			for (int j = 1; j < 20; j++)
			{
				cb_TCopyToolIndexFrom.Items.Add(j + 1 + " : " + cVInspection.paramFile.AdvLocateParam[j].TrackerName);
				cb_TCopyToolIndexTo.Items.Add(j + 1 + " : " + Station[dst].paramFile.AdvLocateParam[j].TrackerName);
			}
		}
		else
		{
			for (int k = 0; k < 20; k++)
			{
				if (tool == 3)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.MeaWidthParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.MeaWidthParam[k].TrackerName);
				}
				if (tool == 4)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.MeaHeightParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.MeaHeightParam[k].TrackerName);
				}
				if (tool == 6)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.ColorReferenceParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.ColorReferenceParam[k].TrackerName);
				}
				if (tool == 7)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.SmoothParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.SmoothParam[k].TrackerName);
				}
				if (tool == 8)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.BlockAvgParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.BlockAvgParam[k].TrackerName);
				}
				if (tool == 9)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.DifferenceParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.DifferenceParam[k].TrackerName);
				}
				if (tool == 10)
				{
					cb_TCopyToolIndexFrom.Items.Add(k + 1 + " : " + cVInspection.paramFile.LineAnalysisParam[k].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(k + 1 + " : " + Station[dst].paramFile.LineAnalysisParam[k].TrackerName);
				}
			}
			for (int l = 0; l < 30; l++)
			{
				if (tool == 5)
				{
					cb_TCopyToolIndexFrom.Items.Add(l + 1 + " : " + cVInspection.paramFile.BinaryParam[l].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(l + 1 + " : " + Station[dst].paramFile.BinaryParam[l].TrackerName);
				}
				if (tool == 12)
				{
					cb_TCopyToolIndexFrom.Items.Add(l + 1 + " : " + cVInspection.paramFile.MaskParam[l].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(l + 1 + " : " + Station[dst].paramFile.MaskParam[l].TrackerName);
				}
			}
			for (int m = 0; m < 40; m++)
			{
				if (tool == 11)
				{
					cb_TCopyToolIndexFrom.Items.Add(m + 1 + " : " + cVInspection.paramFile.AdvLocateMParam[m].TrackerName);
					cb_TCopyToolIndexTo.Items.Add(m + 1 + " : " + Station[dst].paramFile.AdvLocateMParam[m].TrackerName);
				}
			}
		}
		if (selectedIndex < cb_TCopyToolIndexFrom.Items.Count)
		{
			cb_TCopyToolIndexFrom.SelectedIndex = selectedIndex;
		}
		else
		{
			cb_TCopyToolIndexFrom.SelectedIndex = 0;
		}
		if (selectedIndex2 < cb_TCopyToolIndexTo.Items.Count)
		{
			cb_TCopyToolIndexTo.SelectedIndex = selectedIndex2;
		}
		else
		{
			cb_TCopyToolIndexTo.SelectedIndex = 0;
		}
	}

	private void Btn_ToolsCopy_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = cb_ToolCopyFrom.SelectedIndex;
		int selectedIndex2 = cb_ToolCopyTo.SelectedIndex;
		if (selectedIndex == -1 || selectedIndex2 == -1)
		{
			return;
		}
		CVInspection cVInspection = null;
		if (selectedIndex == 8)
		{
			cVInspection = new CVInspection();
			cVInspection.paramFile = OtherIni;
		}
		else
		{
			cVInspection = Station[selectedIndex];
		}
		int selectedIndex3 = cb_ToolCopySelect.SelectedIndex;
		int selectedIndex4 = cb_TCopyToolIndexFrom.SelectedIndex;
		int selectedIndex5 = cb_TCopyToolIndexTo.SelectedIndex;
		if (selectedIndex4 >= 0 && selectedIndex5 >= 0)
		{
			switch (selectedIndex3)
			{
			case 0:
				Station[selectedIndex2].paramFile.VSLocateParam[selectedIndex5 + 2] = cVInspection.paramFile.VSLocateParam[selectedIndex4 + 2];
				Station[selectedIndex2].paramFile.VSLocateParam[selectedIndex5 + 2].ValueChangeFlag = true;
				break;
			case 1:
				Station[selectedIndex2].paramFile.HSLocateParam[selectedIndex5 + 2] = cVInspection.paramFile.HSLocateParam[selectedIndex4 + 2];
				Station[selectedIndex2].paramFile.HSLocateParam[selectedIndex5 + 2].ValueChangeFlag = true;
				break;
			case 2:
				Station[selectedIndex2].paramFile.AdvLocateParam[selectedIndex5 + 1] = cVInspection.paramFile.AdvLocateParam[selectedIndex4 + 1];
				Station[selectedIndex2].paramFile.AdvLocateParam[selectedIndex5 + 1].ValueChangeFlag = true;
				break;
			case 3:
				Station[selectedIndex2].paramFile.MeaWidthParam[selectedIndex5] = cVInspection.paramFile.MeaWidthParam[selectedIndex4];
				Station[selectedIndex2].paramFile.MeaWidthParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 4:
				Station[selectedIndex2].paramFile.MeaHeightParam[selectedIndex5] = cVInspection.paramFile.MeaHeightParam[selectedIndex4];
				Station[selectedIndex2].paramFile.MeaHeightParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 5:
				Station[selectedIndex2].paramFile.BinaryParam[selectedIndex5] = cVInspection.paramFile.BinaryParam[selectedIndex4];
				Station[selectedIndex2].paramFile.BinaryParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 6:
				Station[selectedIndex2].paramFile.ColorReferenceParam[selectedIndex5] = cVInspection.paramFile.ColorReferenceParam[selectedIndex4];
				Station[selectedIndex2].paramFile.ColorReferenceParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 7:
				Station[selectedIndex2].paramFile.SmoothParam[selectedIndex5] = cVInspection.paramFile.SmoothParam[selectedIndex4];
				Station[selectedIndex2].paramFile.SmoothParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 8:
				Station[selectedIndex2].paramFile.BlockAvgParam[selectedIndex5] = cVInspection.paramFile.BlockAvgParam[selectedIndex4];
				Station[selectedIndex2].paramFile.BlockAvgParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 9:
				Station[selectedIndex2].paramFile.DifferenceParam[selectedIndex5] = cVInspection.paramFile.DifferenceParam[selectedIndex4];
				Station[selectedIndex2].paramFile.DifferenceParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 10:
				Station[selectedIndex2].paramFile.LineAnalysisParam[selectedIndex5] = cVInspection.paramFile.LineAnalysisParam[selectedIndex4];
				Station[selectedIndex2].paramFile.LineAnalysisParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 11:
				Station[selectedIndex2].paramFile.AdvLocateMParam[selectedIndex5] = cVInspection.paramFile.AdvLocateMParam[selectedIndex4];
				Station[selectedIndex2].paramFile.AdvLocateMParam[selectedIndex5].ValueChangeFlag = true;
				break;
			case 12:
				Station[selectedIndex2].paramFile.MaskParam[selectedIndex5] = cVInspection.paramFile.MaskParam[selectedIndex4];
				Station[selectedIndex2].paramFile.MaskParam[selectedIndex5].ValueChangeFlag = true;
				break;
			}
			UpdateCopyToolSelectItem(selectedIndex, selectedIndex2, selectedIndex3);
			UpdateSwitchToolSelectItem(selectedIndex, selectedIndex2, selectedIndex3);
			cb_TCopyToolIndexFrom.SelectedIndex = selectedIndex4;
			cb_TCopyToolIndexTo.SelectedIndex = selectedIndex5;
		}
	}

	private void Cb_SCopy_SelectAll_Click(object sender, RoutedEventArgs e)
	{
		cb_SCopy_Locate.IsChecked = true;
		cb_SCopy_SelectWay.IsChecked = true;
		cb_SCopy_SelectFace.IsChecked = true;
		cb_SCopy_VLocate.IsChecked = true;
		cb_SCopy_HLocate.IsChecked = true;
		cb_SCopy_AdvLocate.IsChecked = true;
		cb_SCopy_MeasureW.IsChecked = true;
		cb_SCopy_MeasureH.IsChecked = true;
		cb_SCopy_Mask.IsChecked = true;
		cb_SCopy_ColorReference.IsChecked = true;
		cb_SCopy_Smooth.IsChecked = true;
		cb_SCopy_Difference.IsChecked = true;
		cb_SCopy_BlockAvg.IsChecked = true;
		cb_SCopy_Binary.IsChecked = true;
		cb_SCopy_LineAnalysis.IsChecked = true;
		cb_SCopy_PatternMatch.IsChecked = true;
		cb_SCopy_PatternDB.IsChecked = true;
		cb_SCopy_AdvMeasure.IsChecked = true;
		cb_SCopy_EnhanceTool.IsChecked = true;
		cb_SCopy_AIEnhanceTool.IsChecked = true;
		cb_CommonPara.IsChecked = true;
	}

	private void Cb_SCopy_UnSelectAll_Click(object sender, RoutedEventArgs e)
	{
		cb_SCopy_Locate.IsChecked = false;
		cb_SCopy_SelectWay.IsChecked = false;
		cb_SCopy_SelectFace.IsChecked = false;
		cb_SCopy_VLocate.IsChecked = false;
		cb_SCopy_HLocate.IsChecked = false;
		cb_SCopy_AdvLocate.IsChecked = false;
		cb_SCopy_MeasureW.IsChecked = false;
		cb_SCopy_MeasureH.IsChecked = false;
		cb_SCopy_Mask.IsChecked = false;
		cb_SCopy_ColorReference.IsChecked = false;
		cb_SCopy_Smooth.IsChecked = false;
		cb_SCopy_Binary.IsChecked = false;
		cb_SCopy_Difference.IsChecked = false;
		cb_SCopy_BlockAvg.IsChecked = false;
		cb_SCopy_LineAnalysis.IsChecked = false;
		cb_SCopy_PatternMatch.IsChecked = false;
		cb_SCopy_PatternDB.IsChecked = false;
		cb_SCopy_AdvMeasure.IsChecked = false;
		cb_SCopy_EnhanceTool.IsChecked = false;
		cb_SCopy_AIEnhanceTool.IsChecked = false;
		cb_CommonPara.IsChecked = false;
	}

	private void Cb_TCopyToolIndexFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		cb_TCopyToolIndexTo.SelectedIndex = cb_TCopyToolIndexFrom.SelectedIndex;
	}

	private void Btn_ToolsSwitch_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = cb_ToolCopyFrom.SelectedIndex;
		int selectedIndex2 = cb_ToolCopyTo.SelectedIndex;
		if (selectedIndex == -1 || selectedIndex2 == -1)
		{
			return;
		}
		int selectedIndex3 = cb_ToolCopySelect.SelectedIndex;
		int selectedIndex4 = cb_SwitchToolIndex1.SelectedIndex;
		int selectedIndex5 = cb_SwitchToolIndex2.SelectedIndex;
		if (selectedIndex4 >= 0 && selectedIndex5 >= 0)
		{
			switch (selectedIndex3)
			{
			case 0:
			{
				INSP_SCANLINETOOL_PARAM iNSP_SCANLINETOOL_PARAM2 = Station[selectedIndex].paramFile.VSLocateParam[selectedIndex4 + 2];
				Station[selectedIndex].paramFile.VSLocateParam[selectedIndex4 + 2] = Station[selectedIndex2].paramFile.VSLocateParam[selectedIndex5 + 2];
				Station[selectedIndex2].paramFile.VSLocateParam[selectedIndex5 + 2] = iNSP_SCANLINETOOL_PARAM2;
				Station[selectedIndex2].paramFile.VSLocateParam[selectedIndex5 + 2].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.VSLocateParam[selectedIndex4 + 2].ValueChangeFlag = true;
				break;
			}
			case 1:
			{
				INSP_SCANLINETOOL_PARAM iNSP_SCANLINETOOL_PARAM = Station[selectedIndex].paramFile.HSLocateParam[selectedIndex4 + 2];
				Station[selectedIndex].paramFile.HSLocateParam[selectedIndex4 + 2] = Station[selectedIndex2].paramFile.HSLocateParam[selectedIndex5 + 2];
				Station[selectedIndex2].paramFile.HSLocateParam[selectedIndex5 + 2] = iNSP_SCANLINETOOL_PARAM;
				Station[selectedIndex2].paramFile.HSLocateParam[selectedIndex5 + 2].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.HSLocateParam[selectedIndex4 + 2].ValueChangeFlag = true;
				break;
			}
			case 2:
			{
				INSP_ADVLOCATETOOL_PARAM iNSP_ADVLOCATETOOL_PARAM = Station[selectedIndex].paramFile.AdvLocateParam[selectedIndex4 + 1];
				Station[selectedIndex].paramFile.AdvLocateParam[selectedIndex4 + 1] = Station[selectedIndex2].paramFile.AdvLocateParam[selectedIndex5 + 1];
				Station[selectedIndex2].paramFile.AdvLocateParam[selectedIndex5 + 1] = iNSP_ADVLOCATETOOL_PARAM;
				Station[selectedIndex2].paramFile.AdvLocateParam[selectedIndex5 + 1].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.AdvLocateParam[selectedIndex4 + 1].ValueChangeFlag = true;
				break;
			}
			case 3:
			{
				INSP_MEASURETOOL_PARAM iNSP_MEASURETOOL_PARAM2 = Station[selectedIndex].paramFile.MeaWidthParam[selectedIndex4];
				Station[selectedIndex].paramFile.MeaWidthParam[selectedIndex4] = Station[selectedIndex2].paramFile.MeaWidthParam[selectedIndex5];
				Station[selectedIndex2].paramFile.MeaWidthParam[selectedIndex5] = iNSP_MEASURETOOL_PARAM2;
				Station[selectedIndex2].paramFile.MeaWidthParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.MeaWidthParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 4:
			{
				INSP_MEASURETOOL_PARAM iNSP_MEASURETOOL_PARAM = Station[selectedIndex].paramFile.MeaHeightParam[selectedIndex4];
				Station[selectedIndex].paramFile.MeaHeightParam[selectedIndex4] = Station[selectedIndex2].paramFile.MeaHeightParam[selectedIndex5];
				Station[selectedIndex2].paramFile.MeaHeightParam[selectedIndex5] = iNSP_MEASURETOOL_PARAM;
				Station[selectedIndex2].paramFile.MeaHeightParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.MeaHeightParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 5:
			{
				INSP_BINARYTOOL_PARAM iNSP_BINARYTOOL_PARAM = Station[selectedIndex].paramFile.BinaryParam[selectedIndex4];
				Station[selectedIndex].paramFile.BinaryParam[selectedIndex4] = Station[selectedIndex2].paramFile.BinaryParam[selectedIndex5];
				Station[selectedIndex2].paramFile.BinaryParam[selectedIndex5] = iNSP_BINARYTOOL_PARAM;
				Station[selectedIndex2].paramFile.BinaryParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.BinaryParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 6:
			{
				INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = Station[selectedIndex].paramFile.ColorReferenceParam[selectedIndex4];
				Station[selectedIndex].paramFile.ColorReferenceParam[selectedIndex4] = Station[selectedIndex2].paramFile.ColorReferenceParam[selectedIndex5];
				Station[selectedIndex2].paramFile.ColorReferenceParam[selectedIndex5] = iNSP_COLORREFERENCE_PARAM;
				Station[selectedIndex2].paramFile.ColorReferenceParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.ColorReferenceParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 7:
			{
				INSP_SMOOTHTOOL_PARAM iNSP_SMOOTHTOOL_PARAM = Station[selectedIndex].paramFile.SmoothParam[selectedIndex4];
				Station[selectedIndex].paramFile.SmoothParam[selectedIndex4] = Station[selectedIndex2].paramFile.SmoothParam[selectedIndex5];
				Station[selectedIndex2].paramFile.SmoothParam[selectedIndex5] = iNSP_SMOOTHTOOL_PARAM;
				Station[selectedIndex2].paramFile.SmoothParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.SmoothParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 8:
			{
				INSP_DIFFERENCETOOL_PARAM iNSP_DIFFERENCETOOL_PARAM2 = Station[selectedIndex].paramFile.BlockAvgParam[selectedIndex4];
				Station[selectedIndex].paramFile.BlockAvgParam[selectedIndex4] = Station[selectedIndex2].paramFile.BlockAvgParam[selectedIndex5];
				Station[selectedIndex2].paramFile.BlockAvgParam[selectedIndex5] = iNSP_DIFFERENCETOOL_PARAM2;
				Station[selectedIndex2].paramFile.BlockAvgParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.BlockAvgParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 9:
			{
				INSP_DIFFERENCETOOL_PARAM iNSP_DIFFERENCETOOL_PARAM = Station[selectedIndex].paramFile.DifferenceParam[selectedIndex4];
				Station[selectedIndex].paramFile.DifferenceParam[selectedIndex4] = Station[selectedIndex2].paramFile.DifferenceParam[selectedIndex5];
				Station[selectedIndex2].paramFile.DifferenceParam[selectedIndex5] = iNSP_DIFFERENCETOOL_PARAM;
				Station[selectedIndex2].paramFile.DifferenceParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.DifferenceParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 10:
			{
				INSP_LINEANALYSISTOOL_PARAM iNSP_LINEANALYSISTOOL_PARAM = Station[selectedIndex].paramFile.LineAnalysisParam[selectedIndex4];
				Station[selectedIndex].paramFile.LineAnalysisParam[selectedIndex4] = Station[selectedIndex2].paramFile.LineAnalysisParam[selectedIndex5];
				Station[selectedIndex2].paramFile.LineAnalysisParam[selectedIndex5] = iNSP_LINEANALYSISTOOL_PARAM;
				Station[selectedIndex2].paramFile.LineAnalysisParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.LineAnalysisParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 11:
			{
				INSP_ADVMEASURETOOL_PARAM iNSP_ADVMEASURETOOL_PARAM = Station[selectedIndex].paramFile.AdvLocateMParam[selectedIndex4];
				Station[selectedIndex].paramFile.AdvLocateMParam[selectedIndex4] = Station[selectedIndex2].paramFile.AdvLocateMParam[selectedIndex5];
				Station[selectedIndex2].paramFile.AdvLocateMParam[selectedIndex5] = iNSP_ADVMEASURETOOL_PARAM;
				Station[selectedIndex2].paramFile.AdvLocateMParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.AdvLocateMParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			case 12:
			{
				INSP_MASKTOOL_PARAM iNSP_MASKTOOL_PARAM = Station[selectedIndex].paramFile.MaskParam[selectedIndex4];
				Station[selectedIndex].paramFile.MaskParam[selectedIndex4] = Station[selectedIndex2].paramFile.MaskParam[selectedIndex5];
				Station[selectedIndex2].paramFile.MaskParam[selectedIndex5] = iNSP_MASKTOOL_PARAM;
				Station[selectedIndex2].paramFile.MaskParam[selectedIndex5].ValueChangeFlag = true;
				Station[selectedIndex].paramFile.MaskParam[selectedIndex4].ValueChangeFlag = true;
				break;
			}
			}
			UpdateCopyToolSelectItem(selectedIndex, selectedIndex2, selectedIndex3);
			UpdateSwitchToolSelectItem(selectedIndex, selectedIndex2, selectedIndex3);
			cb_SwitchToolIndex1.SelectedIndex = selectedIndex4;
			cb_SwitchToolIndex2.SelectedIndex = selectedIndex5;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_StationCopyFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (cb_StationCopyFrom.SelectedIndex != 8)
		{
			return;
		}
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Ini files (station*.ini,station*.INI)|station*.ini;station*.INI|All files (*.*)|*.*";
		openFileDialog.InitialDirectory = string.Format("{0}Recipe\\", AppDomain.CurrentDomain.BaseDirectory);
		if (openFileDialog.ShowDialog() != true)
		{
			return;
		}
		string fileName = Path.GetFileName(openFileDialog.FileName);
		string text = string.Format(arg0: Path.GetDirectoryName(openFileDialog.FileName), format: "{0}\\setting.ini");
		if (File.Exists(text))
		{
			SettingINI = new HiniFile(text, ";");
			Settingini.SetINI(SettingINI);
			Settingini.ReadSizeRecordSetupParam();
			Settingini.GetComDigis(ref ComDigits, ref ComDigitsName);
		}
		if (fileName.Contains("station"))
		{
			StationINI = new HiniFile(openFileDialog.FileName, ";");
			OtherIni.SetINI(StationINI);
			OtherIni.ReadAllParam();
			OtherIniOK = true;
			return;
		}
		OtherIniOK = false;
		if (LanguageType == 0)
		{
			MessageBox.Show("Open Ini file Error!", "Warring", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
		}
		else if (LanguageType == 1)
		{
			MessageBox.Show("INI檔案格式錯誤!", "Warring", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
		}
		else
		{
			MessageBox.Show("INI档案格式错误!", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetRecipe(ParamFile paramFile, string recipename, int index)
	{
		string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + recipename + "\\";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string file = text + "station" + (index + 1) + ".ini";
		StationINI = new HiniFile(file, ";");
		paramFile.SetINI(StationINI);
		paramFile.ReadAllParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CopyToRecipe(ParamFile paramFile, string recipename, int index)
	{
		string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + recipename + "\\";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string file = text + "station" + (index + 1) + ".ini";
		StationINI = new HiniFile(file, ";");
		paramFile.SetINI(StationINI);
		paramFile.WriteAllParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		if (cb_RecipeCopyTo.SelectedIndex == -1 || cb_RecipeCopyFrom.SelectedIndex == -1 || cb_RecipeCopyTo.SelectedIndex == cb_RecipeCopyFrom.SelectedIndex)
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected Recipe will be overwrite!", "Warning - " + cb_RecipeCopyFrom.Text + "Copy to" + cb_RecipeCopyTo.Text, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("所有选择的程式将被覆盖", "警告 - " + cb_RecipeCopyFrom.Text + "拷贝到" + cb_RecipeCopyTo.Text, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : MessageBox.Show("所有選擇的程式將被覆蓋", "警告 - " + cb_RecipeCopyFrom.Text + "拷貝到" + cb_RecipeCopyTo.Text, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		ParamFile paramFile = new ParamFile();
		ParamFile paramFile2 = new ParamFile();
		try
		{
			if (rad_CopyAll.IsChecked == true)
			{
				string text = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + cb_RecipeCopyFrom.Text + "\\";
				string text2 = AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + cb_RecipeCopyTo.Text + "\\";
				for (int i = 0; i < 8; i++)
				{
					if (StationSelect[i].IsChecked == true)
					{
						string sourceFileName = text + "station" + (i + 1) + ".ini";
						string destFileName = text2 + "station" + (i + 1) + ".ini";
						File.Copy(sourceFileName, destFileName, overwrite: true);
					}
				}
			}
			else
			{
				for (int j = 0; j < 8; j++)
				{
					if (StationSelect[j].IsChecked != true)
					{
						continue;
					}
					SetRecipe(paramFile, cb_RecipeCopyFrom.Text, j);
					SetRecipe(paramFile2, cb_RecipeCopyTo.Text, j);
					for (int k = 0; k < 20; k++)
					{
						if (paramFile.HSLocateParam[k].TrackerName != "Unused")
						{
							paramFile2.HSLocateParam[k].NameChangeFlag = true;
							paramFile2.HSLocateParam[k].TrackerName = paramFile.HSLocateParam[k].TrackerName;
						}
						if (paramFile.VSLocateParam[k].TrackerName != "Unused")
						{
							paramFile2.VSLocateParam[k].NameChangeFlag = true;
							paramFile2.VSLocateParam[k].TrackerName = paramFile.VSLocateParam[k].TrackerName;
						}
						if (paramFile.AdvLocateParam[k].TrackerName != "Unused")
						{
							paramFile2.AdvLocateParam[k].NameChangeFlag = true;
							paramFile2.AdvLocateParam[k].TrackerName = paramFile.AdvLocateParam[k].TrackerName;
						}
						if (paramFile.MeaWidthParam[k].TrackerName != "Unused")
						{
							paramFile2.MeaWidthParam[k].NameChangeFlag = true;
							paramFile2.MeaWidthParam[k].TrackerName = paramFile.MeaWidthParam[k].TrackerName;
						}
						if (paramFile.MeaHeightParam[k].TrackerName != "Unused")
						{
							paramFile2.MeaHeightParam[k].NameChangeFlag = true;
							paramFile2.MeaHeightParam[k].TrackerName = paramFile.MeaHeightParam[k].TrackerName;
						}
						if (paramFile.MaskParam[k].TrackerName != "Unused")
						{
							paramFile2.MaskParam[k].NameChangeFlag = true;
							paramFile2.MaskParam[k].TrackerName = paramFile.MaskParam[k].TrackerName;
						}
						if (paramFile.SmoothParam[k].TrackerName != "Unused")
						{
							paramFile2.SmoothParam[k].NameChangeFlag = true;
							paramFile2.SmoothParam[k].TrackerName = paramFile.SmoothParam[k].TrackerName;
						}
						if (paramFile.DifferenceParam[k].TrackerName != "Unused")
						{
							paramFile2.DifferenceParam[k].NameChangeFlag = true;
							paramFile2.DifferenceParam[k].TrackerName = paramFile.DifferenceParam[k].TrackerName;
						}
						if (paramFile.BlockAvgParam[k].TrackerName != "Unused")
						{
							paramFile2.BlockAvgParam[k].NameChangeFlag = true;
							paramFile2.BlockAvgParam[k].TrackerName = paramFile.BlockAvgParam[k].TrackerName;
						}
						if (paramFile.LineAnalysisParam[k].TrackerName != "Unused")
						{
							paramFile2.LineAnalysisParam[k].NameChangeFlag = true;
							paramFile2.LineAnalysisParam[k].TrackerName = paramFile.LineAnalysisParam[k].TrackerName;
						}
						if (paramFile.MatchParam[k].TrackerName != "Unused")
						{
							paramFile2.MatchParam[k].NameChangeFlag = true;
							paramFile2.MatchParam[k].TrackerName = paramFile.MatchParam[k].TrackerName;
						}
					}
					for (int l = 0; l < 30; l++)
					{
						if (paramFile.BinaryParam[l].TrackerName != "Unused")
						{
							paramFile2.BinaryParam[l].NameChangeFlag = true;
							paramFile2.BinaryParam[l].TrackerName = paramFile.BinaryParam[l].TrackerName;
						}
					}
					for (int m = 0; m < 40; m++)
					{
						if (paramFile.AdvLocateMParam[m].TrackerName != "Unused")
						{
							paramFile2.AdvLocateMParam[m].NameChangeFlag = true;
							paramFile2.AdvLocateMParam[m].TrackerName = paramFile.AdvLocateMParam[m].TrackerName;
						}
					}
					CopyToRecipe(paramFile2, cb_RecipeCopyTo.Text, j);
				}
			}
			if (LanguageType == 0)
			{
				MessageBox.Show("Copy Finish!", "Finish", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
		}
		catch (Exception ex)
		{
			string text3 = "PCW01-ParamCOpyWIndow-Btn_RecipeCopy_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_PublicPara_Copy_Click(object sender, RoutedEventArgs e)
	{
		if (cb_RC_Sour.SelectedIndex == -1 || SelectedRecipe.Count <= 0)
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected Recipe will be overwrite!", "Warning - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("所有选择的程式将被覆盖", "警告 - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : MessageBox.Show("所有選擇的程式將被覆蓋", "警告 - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		try
		{
			HiniFile hiniFile = new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + cb_RC_Sour.Text + "\\SoterPara.ini", ";");
			HiniFile iNI = new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + cb_RC_Sour.Text + "\\Setting.ini", ";");
			ThroughputParamFile throughputParamFile = new ThroughputParamFile();
			throughputParamFile.SetINI(iNI);
			throughputParamFile.ReadSizeRecordSetupParam();
			for (int i = 0; i < SelectedRecipe.Count; i++)
			{
				HiniFile hiniFile2 = new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + SelectedRecipe[i] + "\\SoterPara.ini", ";");
				for (int j = 0; j < 15; j++)
				{
					int num = -1;
					if (cb_RC_StationPos.IsChecked == true && j < 8)
					{
						string key = string.Format("Station_{0}", j);
						num = hiniFile.GetInteger("OffsetPara", key, -1);
						if (num != -1)
						{
							hiniFile2.WriteInt("OffsetPara", key, num);
						}
					}
					if (cb_RC_FlowPos.IsChecked == true && j >= 8)
					{
						string key2 = string.Format("Station_{0}", j);
						num = hiniFile.GetInteger("OffsetPara", key2, -1);
						if (num != -1)
						{
							hiniFile2.WriteInt("OffsetPara", key2, num);
						}
					}
				}
				HiniFile hiniFile3 = new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + SelectedRecipe[i] + "\\Setting.ini", ";");
				if (cb_RC_PublicPara.IsChecked == true)
				{
					hiniFile3.WriteDouble("Throughput", "ComDigits0", throughputParamFile.param.ComDigits0);
					hiniFile3.WriteDouble("Throughput", "ComDigits1", throughputParamFile.param.ComDigits1);
					hiniFile3.WriteDouble("Throughput", "ComDigits2", throughputParamFile.param.ComDigits2);
					hiniFile3.WriteDouble("Throughput", "ComDigits3", throughputParamFile.param.ComDigits3);
					hiniFile3.WriteDouble("Throughput", "ComDigits4", throughputParamFile.param.ComDigits4);
					hiniFile3.WriteDouble("Throughput", "ComDigits5", throughputParamFile.param.ComDigits5);
					hiniFile3.WriteDouble("Throughput", "ComDigits6", throughputParamFile.param.ComDigits6);
					hiniFile3.WriteDouble("Throughput", "ComDigits7", throughputParamFile.param.ComDigits7);
					hiniFile3.WriteDouble("Throughput", "ComDigits8", throughputParamFile.param.ComDigits8);
					hiniFile3.WriteDouble("Throughput", "ComDigits9", throughputParamFile.param.ComDigits9);
					hiniFile3.WriteDouble("Throughput", "ComDigits10", throughputParamFile.param.ComDigits10);
					hiniFile3.WriteDouble("Throughput", "ComDigits11", throughputParamFile.param.ComDigits11);
					hiniFile3.WriteDouble("Throughput", "ComDigits12", throughputParamFile.param.ComDigits12);
					hiniFile3.WriteDouble("Throughput", "ComDigits13", throughputParamFile.param.ComDigits13);
					hiniFile3.WriteDouble("Throughput", "ComDigits14", throughputParamFile.param.ComDigits14);
					hiniFile3.WriteDouble("Throughput", "ComDigits15", throughputParamFile.param.ComDigits15);
					hiniFile3.WriteDouble("Throughput", "ComDigits16", throughputParamFile.param.ComDigits16);
					hiniFile3.WriteDouble("Throughput", "ComDigits17", throughputParamFile.param.ComDigits17);
					hiniFile3.WriteDouble("Throughput", "ComDigits18", throughputParamFile.param.ComDigits18);
					hiniFile3.WriteDouble("Throughput", "ComDigits19", throughputParamFile.param.ComDigits19);
					hiniFile3.WriteDouble("Throughput", "ComDigits20", throughputParamFile.param.ComDigits20);
					hiniFile3.WriteDouble("Throughput", "ComDigits21", throughputParamFile.param.ComDigits21);
					hiniFile3.WriteDouble("Throughput", "ComDigits22", throughputParamFile.param.ComDigits22);
					hiniFile3.WriteDouble("Throughput", "ComDigits23", throughputParamFile.param.ComDigits23);
					hiniFile3.WriteString("Throughput", "ComDigitsName0", throughputParamFile.param.ComDigitsName0);
					hiniFile3.WriteString("Throughput", "ComDigitsName1", throughputParamFile.param.ComDigitsName1);
					hiniFile3.WriteString("Throughput", "ComDigitsName2", throughputParamFile.param.ComDigitsName2);
					hiniFile3.WriteString("Throughput", "ComDigitsName3", throughputParamFile.param.ComDigitsName3);
					hiniFile3.WriteString("Throughput", "ComDigitsName4", throughputParamFile.param.ComDigitsName4);
					hiniFile3.WriteString("Throughput", "ComDigitsName5", throughputParamFile.param.ComDigitsName5);
					hiniFile3.WriteString("Throughput", "ComDigitsName6", throughputParamFile.param.ComDigitsName6);
					hiniFile3.WriteString("Throughput", "ComDigitsName7", throughputParamFile.param.ComDigitsName7);
					hiniFile3.WriteString("Throughput", "ComDigitsName8", throughputParamFile.param.ComDigitsName8);
					hiniFile3.WriteString("Throughput", "ComDigitsName9", throughputParamFile.param.ComDigitsName9);
					hiniFile3.WriteString("Throughput", "ComDigitsName10", throughputParamFile.param.ComDigitsName10);
					hiniFile3.WriteString("Throughput", "ComDigitsName11", throughputParamFile.param.ComDigitsName11);
					hiniFile3.WriteString("Throughput", "ComDigitsName12", throughputParamFile.param.ComDigitsName12);
					hiniFile3.WriteString("Throughput", "ComDigitsName13", throughputParamFile.param.ComDigitsName13);
					hiniFile3.WriteString("Throughput", "ComDigitsName14", throughputParamFile.param.ComDigitsName14);
					hiniFile3.WriteString("Throughput", "ComDigitsName15", throughputParamFile.param.ComDigitsName15);
					hiniFile3.WriteString("Throughput", "ComDigitsName16", throughputParamFile.param.ComDigitsName16);
					hiniFile3.WriteString("Throughput", "ComDigitsName17", throughputParamFile.param.ComDigitsName17);
					hiniFile3.WriteString("Throughput", "ComDigitsName18", throughputParamFile.param.ComDigitsName18);
					hiniFile3.WriteString("Throughput", "ComDigitsName19", throughputParamFile.param.ComDigitsName19);
					hiniFile3.WriteString("Throughput", "ComDigitsName20", throughputParamFile.param.ComDigitsName20);
					hiniFile3.WriteString("Throughput", "ComDigitsName21", throughputParamFile.param.ComDigitsName21);
					hiniFile3.WriteString("Throughput", "ComDigitsName22", throughputParamFile.param.ComDigitsName22);
					hiniFile3.WriteString("Throughput", "ComDigitsName23", throughputParamFile.param.ComDigitsName23);
				}
			}
			if (LanguageType == 0)
			{
				MessageBox.Show("Copy Finish!", "Finish", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
		}
		catch (Exception ex)
		{
			string text = "PCW01-ParamCOpyWIndow-Btn_PublicPara_Copy_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_StationPara_Copy_Click(object sender, RoutedEventArgs e)
	{
		if (cb_RC_Sour.SelectedIndex == -1 || SelectedRecipe.Count <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			if (RCStation[i].IsChecked == true)
			{
				num++;
			}
		}
		if (num == 0)
		{
			MessageBox.Show("請選擇要拷貝的站別。", "注意");
			return;
		}
		if (cb_RC_LightPara.IsChecked == false && cb_RC_CameraPara.IsChecked == false && cb_RC_PixelPerUm.IsChecked == false && cb_RC_LocateTool.IsChecked == false && cb_RC_Tools.SelectedIndex == 0 && cb_RC_ToolIndex.SelectedIndex == 0)
		{
			MessageBox.Show("請選擇要拷貝的站別參數或檢測工具參數。", "注意");
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected Recipe will be overwrite!", "Warning - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("所有选择的程式将被覆盖", "警告 - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : MessageBox.Show("所有選擇的程式將被覆蓋", "警告 - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			grd_Loading.Visibility = Visibility.Visible;
			lab_Loading.Content = string.Format("程式拷貝中...");
			CopyTimer.Start();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CopyTimer_Tick(object sender, EventArgs e)
	{
		ParamFile paramFile = new ParamFile();
		ParamFile paramFile2 = new ParamFile();
		CopyTimer.Stop();
		try
		{
			HiniFile hiniFile = new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + cb_RC_Sour.Text + "\\Setting.ini", ";");
			int r;
			for (r = 0; r < SelectedRecipe.Count; r++)
			{
				HiniFile hiniFile2 = new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + SelectedRecipe[r] + "\\Setting.ini", ";");
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)([MethodImpl(MethodImplOptions.NoInlining)] () =>
				{
					lab_Loading.Content = string.Format("程式拷貝中...{0}/{1}", r + 1, SelectedRecipe.Count);
				}));
				for (int num = 0; num < 8; num++)
				{
					if (RCStation[num].IsChecked == false)
					{
						continue;
					}
					string text = string.Format("{0}Recipe\\{1}\\station{2}.ini", AppDomain.CurrentDomain.BaseDirectory, cb_RC_Sour.Text, num + 1);
					string text2 = string.Format("{0}Recipe\\{1}\\station{2}.ini", AppDomain.CurrentDomain.BaseDirectory, SelectedRecipe[r], num + 1);
					if (!File.Exists(text))
					{
						MessageBox.Show("找不到來源檔案 " + text, "警告");
						return;
					}
					if (!File.Exists(text2))
					{
						MessageBox.Show("找不到目標檔案 " + text2, "警告");
						return;
					}
					HiniFile hiniFile3 = new HiniFile(text, ";");
					HiniFile hiniFile4 = new HiniFile(text2, ";");
					if (cb_RC_LightPara.IsChecked == true)
					{
						double num2 = -1.0;
						for (int num3 = 0; num3 < 4; num3++)
						{
							string key = string.Format("LightVoltage_{0}{1}", num + 1, num3 + 1);
							num2 = hiniFile.GetDouble("Main", key, -1.0);
							if (num2 != -1.0)
							{
								hiniFile2.WriteDouble("Main", key, num2);
							}
						}
					}
					if (cb_RC_CameraPara.IsChecked == true)
					{
						double num4 = -1.0;
						int num5 = -1;
						string key2 = string.Format("ImageWidth_{0}", num + 1);
						num5 = hiniFile.GetInteger("Camera Info", key2, 640);
						if (num5 > 0)
						{
							hiniFile2.WriteInt("Camera Info", key2, num5);
						}
						key2 = string.Format("ImageHeight_{0}", num + 1);
						num5 = hiniFile.GetInteger("Camera Info", key2, 480);
						if (num5 > 0)
						{
							hiniFile2.WriteInt("Camera Info", key2, num5);
						}
						key2 = string.Format("ImageOffsetX_{0}", num + 1);
						num5 = hiniFile.GetInteger("Camera Info", key2);
						if (num5 > 0)
						{
							hiniFile2.WriteInt("Camera Info", key2, num5);
						}
						key2 = string.Format("ImageOffsetY_{0}", num + 1);
						num5 = hiniFile.GetInteger("Camera Info", key2);
						if (num5 > 0)
						{
							hiniFile2.WriteInt("Camera Info", key2, num5);
						}
						string key3 = string.Format("Saturation_{0}", num + 1);
						num4 = hiniFile.GetDouble("Camera Info", key3, -1.0);
						if (num4 != -1.0)
						{
							hiniFile2.WriteDouble("Camera Info", key3, num4);
						}
						key3 = string.Format("WhitebalanceBlue_{0}", num + 1);
						num4 = hiniFile.GetDouble("Camera Info", key3, -1.0);
						if (num4 != -1.0)
						{
							hiniFile2.WriteDouble("Camera Info", key3, num4);
						}
						key3 = string.Format("WhitebalanceRed_{0}", num + 1);
						num4 = hiniFile.GetDouble("Camera Info", key3, -1.0);
						if (num4 != -1.0)
						{
							hiniFile2.WriteDouble("Camera Info", key3, num4);
						}
						key3 = string.Format("Gain_{0}", num + 1);
						num5 = hiniFile.GetInteger("Camera Info", key3, -1);
						if ((double)num5 != -1.0)
						{
							hiniFile2.WriteInt("Camera Info", key3, num5);
						}
						key3 = string.Format("ExposureTime_{0}", num + 1);
						num5 = hiniFile.GetInteger("Camera Info", key3, -1);
						if ((double)num5 != -1.0)
						{
							hiniFile2.WriteInt("Camera Info", key3, num5);
						}
					}
					if (cb_RC_PixelPerUm.IsChecked == true)
					{
						double num6 = -1.0;
						string key4 = string.Format("PixelPerUM_X");
						num6 = hiniFile3.GetDouble("Main", key4, -1.0);
						if (num6 != -1.0)
						{
							hiniFile4.WriteDouble("Main", key4, num6);
						}
						key4 = string.Format("PixelPerUM_Y");
						num6 = hiniFile3.GetDouble("Main", key4, -1.0);
						if (num6 != -1.0)
						{
							hiniFile4.WriteDouble("Main", key4, num6);
						}
						key4 = string.Format("UseFloat");
						hiniFile4.WriteBool("Main", key4, hiniFile3.GetBoolean("Main", key4));
					}
					if (cb_RC_LocateTool.IsChecked == true)
					{
						paramFile.SetINI(hiniFile3);
						paramFile.ReadAllParam();
						paramFile2.SetINI(hiniFile4);
						paramFile2.ReadAllParam();
						paramFile2.LocateParam = paramFile.LocateParam;
						paramFile2.LocateParam.ValueChangeFlag = true;
						paramFile2.WriteLocateParam();
						if (Station[num].RecipeName == SelectedRecipe[r])
						{
							Station[num].paramFile.LocateParam = paramFile.LocateParam;
						}
					}
					if (cb_RC_Tools.SelectedIndex > 0 && cb_RC_ToolIndex.SelectedIndex >= 0)
					{
						paramFile.SetINI(hiniFile3);
						paramFile.ReadAllParam();
						paramFile2.SetINI(hiniFile4);
						paramFile2.ReadAllParam();
						int selectedIndex = cb_RC_ToolIndex.SelectedIndex;
						switch (cb_RC_Tools.SelectedIndex)
						{
						case 1:
							paramFile2.VSLocateParam[selectedIndex] = paramFile.VSLocateParam[selectedIndex];
							paramFile2.VSLocateParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteAdvLocaeParam(selectedIndex);
							break;
						case 2:
							paramFile2.HSLocateParam[selectedIndex] = paramFile.HSLocateParam[selectedIndex];
							paramFile2.HSLocateParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteAdvLocaeParam(selectedIndex);
							break;
						case 3:
							paramFile2.AdvLocateParam[selectedIndex] = paramFile.AdvLocateParam[selectedIndex];
							paramFile2.AdvLocateParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteAdvLocaeParam(selectedIndex);
							break;
						case 4:
							paramFile2.MeaWidthParam[selectedIndex] = paramFile.MeaWidthParam[selectedIndex];
							paramFile2.MeaWidthParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteMeasureParam(selectedIndex);
							break;
						case 5:
							paramFile2.MeaHeightParam[selectedIndex] = paramFile.MeaHeightParam[selectedIndex];
							paramFile2.MeaHeightParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteMeasureParam(selectedIndex);
							break;
						case 6:
							paramFile2.BinaryParam[selectedIndex] = paramFile.BinaryParam[selectedIndex];
							paramFile2.BinaryParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteBinaryParam(paramFile2.BinaryParam[selectedIndex], selectedIndex, "BinaryTools");
							break;
						case 7:
							paramFile2.ColorReferenceParam[selectedIndex] = paramFile.ColorReferenceParam[selectedIndex];
							paramFile2.ColorReferenceParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteColorReferenceParam(selectedIndex);
							break;
						case 8:
							paramFile2.SmoothParam[selectedIndex] = paramFile.SmoothParam[selectedIndex];
							paramFile2.SmoothParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteSmoothParam(selectedIndex);
							break;
						case 9:
							paramFile2.BlockAvgParam[selectedIndex] = paramFile.BlockAvgParam[selectedIndex];
							paramFile2.BlockAvgParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteBlockAvgParam(selectedIndex);
							break;
						case 10:
							paramFile2.DifferenceParam[selectedIndex] = paramFile.DifferenceParam[selectedIndex];
							paramFile2.DifferenceParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteDifferenceParam(selectedIndex);
							break;
						case 11:
							paramFile2.LineAnalysisParam[selectedIndex] = paramFile.LineAnalysisParam[selectedIndex];
							paramFile2.LineAnalysisParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteLineAnalysisParam(selectedIndex);
							break;
						case 12:
							paramFile2.AdvLocateMParam[selectedIndex] = paramFile.AdvLocateMParam[selectedIndex];
							paramFile2.AdvLocateMParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteAdvMeasureParam(selectedIndex);
							break;
						case 13:
							paramFile2.MaskParam[selectedIndex] = paramFile.MaskParam[selectedIndex];
							paramFile2.MaskParam[selectedIndex].ValueChangeFlag = true;
							paramFile2.WriteMaskParam(selectedIndex);
							break;
						}
					}
				}
			}
			if (LanguageType == 0)
			{
				MessageBox.Show("Copy Finish!", "Finish", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			grd_Loading.Visibility = Visibility.Hidden;
		}
		catch (Exception ex)
		{
			grd_Loading.Visibility = Visibility.Hidden;
			string text3 = "PCW01-ParamCOpyWIndow-Btn_StationPara_Copy_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_CustomPara_Copy_Click(object sender, RoutedEventArgs e)
	{
		if (cb_RC_Sour.SelectedIndex == -1 || SelectedRecipe.Count <= 0)
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected Recipe will be overwrite!", "Warning - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("所有选择的程式将被覆盖", "警告 - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) : MessageBox.Show("所有選擇的程式將被覆蓋", "警告 - ", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		int num = cb_RC_Station.SelectedIndex - 1;
		if (num < 0 || txt_RC_TagName.Text == "" || txt_RC_ItemName.Text == "")
		{
			return;
		}
		try
		{
			new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + cb_RC_Sour.Text + "\\Setting.ini", ";");
			string text = "";
			for (int i = 0; i < SelectedRecipe.Count; i++)
			{
				new HiniFile(AppDomain.CurrentDomain.BaseDirectory + "Recipe\\" + SelectedRecipe[i] + "\\Setting.ini", ";");
				string text2 = string.Format("{0}Recipe\\{1}\\station{2}.ini", AppDomain.CurrentDomain.BaseDirectory, cb_RC_Sour.Text, num + 1);
				string text3 = string.Format("{0}Recipe\\{1}\\station{2}.ini", AppDomain.CurrentDomain.BaseDirectory, SelectedRecipe[i], num + 1);
				if (!File.Exists(text2))
				{
					MessageBox.Show("找不到來源檔案 " + text2, "警告");
					return;
				}
				if (!File.Exists(text3))
				{
					MessageBox.Show("找不到目標檔案 " + text3, "警告");
					return;
				}
				HiniFile hiniFile = new HiniFile(text2, ";");
				HiniFile hiniFile2 = new HiniFile(text3, ";");
				string value = hiniFile.GetValue(txt_RC_TagName.Text, txt_RC_ItemName.Text);
				if (value != "")
				{
					hiniFile2.WriteString(txt_RC_TagName.Text, txt_RC_ItemName.Text, value);
				}
				else
				{
					text = text + txt_RC_TagName.Text + " 找不到來源參數 " + txt_RC_ItemName.Text;
				}
			}
			if (text != "")
			{
				MessageBox.Show(text, "警告");
			}
			if (LanguageType == 0)
			{
				MessageBox.Show("Copy Finish!", "Finish", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else if (LanguageType == 1)
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
			else
			{
				MessageBox.Show("程式拷貝完成!", "完成", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
			}
		}
		catch (Exception ex)
		{
			string text4 = "PCW01-ParamCOpyWIndow-Btn_CustomPara_Copy_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text4);
			MessageBox.Show(text4, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipeCopyDest_Click(object sender, RoutedEventArgs e)
	{
		SelectedRecipe.Clear();
		RecipeListWindow recipeListWindow = new RecipeListWindow();
		recipeListWindow.ShowDialog();
		SelectedRecipe = recipeListWindow.GetSelectedRecipes().ToList();
		if (SelectedRecipe.Count > 0)
		{
			txt_RC_Dest.Text = string.Format("已選擇 {0} 個程式。", SelectedRecipe.Count);
			txt_RC_Dest.Background = Brushes.LightGreen;
		}
		else
		{
			txt_RC_Dest.Text = "尚未選擇程式。";
			txt_RC_Dest.Background = Brushes.LightPink;
		}
	}

	private void Cb_RC_Tools_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			int num = 20;
			num = cb_RC_Tools.SelectedIndex switch
			{
				6 => 30, 
				12 => 40, 
				_ => 20, 
			};
			cb_RC_ToolIndex.Items.Clear();
			for (int i = 0; i < num; i++)
			{
				cb_RC_ToolIndex.Items.Add(i + 1);
			}
			cb_RC_ToolIndex.SelectedIndex = 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/paramcopywindow.xaml", UriKind.Relative);
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
			cb_StationCopyTo = (ComboBox)target;
			break;
		case 2:
			btn_ReDraw = (Button)target;
			break;
		case 3:
			cb_StationCopyFrom = (ComboBox)target;
			cb_StationCopyFrom.SelectionChanged += Cb_StationCopyFrom_SelectionChanged;
			break;
		case 4:
			btn_StationCopy = (Button)target;
			btn_StationCopy.Click += Btn_StationCopy_Click;
			break;
		case 5:
			btn_ReDraw_Copy = (Button)target;
			break;
		case 6:
			cb_SCopy_Locate = (CheckBox)target;
			break;
		case 7:
			cb_SCopy_SelectWay = (CheckBox)target;
			break;
		case 8:
			cb_SCopy_HLocate = (CheckBox)target;
			break;
		case 9:
			cb_SCopy_AdvLocate = (CheckBox)target;
			break;
		case 10:
			cb_SCopy_MeasureH = (CheckBox)target;
			break;
		case 11:
			cb_SCopy_VLocate = (CheckBox)target;
			break;
		case 12:
			cb_SCopy_Smooth = (CheckBox)target;
			break;
		case 13:
			cb_SCopy_Mask = (CheckBox)target;
			break;
		case 14:
			cb_SCopy_Binary = (CheckBox)target;
			break;
		case 15:
			cb_SCopy_Difference = (CheckBox)target;
			break;
		case 16:
			cb_SCopy_BlockAvg = (CheckBox)target;
			break;
		case 17:
			cb_SCopy_LineAnalysis = (CheckBox)target;
			break;
		case 18:
			cb_SCopy_MeasureW = (CheckBox)target;
			break;
		case 19:
			cb_SCopy_ColorReference = (CheckBox)target;
			break;
		case 20:
			cb_SCopy_SelectAll = (CheckBox)target;
			cb_SCopy_SelectAll.Click += Cb_SCopy_SelectAll_Click;
			break;
		case 21:
			cb_SCopy_UnSelectAll = (CheckBox)target;
			cb_SCopy_UnSelectAll.Click += Cb_SCopy_UnSelectAll_Click;
			break;
		case 22:
			cb_SCopy_PatternMatch = (CheckBox)target;
			break;
		case 23:
			cb_SCopy_PatternDB = (CheckBox)target;
			break;
		case 24:
			cb_SCopy_SelectFace = (CheckBox)target;
			break;
		case 25:
			cb_SCopy_AdvMeasure = (CheckBox)target;
			break;
		case 26:
			cb_SCopy_EnhanceTool = (CheckBox)target;
			break;
		case 27:
			cb_CommonPara = (CheckBox)target;
			break;
		case 28:
			cb_SCopy_AIEnhanceTool = (CheckBox)target;
			break;
		case 29:
			((Button)target).Click += Cb_SCopy_SelectAll_Click;
			break;
		case 30:
			((Button)target).Click += Cb_SCopy_UnSelectAll_Click;
			break;
		case 31:
			gb_TrackerCopy = (GroupBox)target;
			break;
		case 32:
			cb_ToolCopyTo = (ComboBox)target;
			cb_ToolCopyTo.SelectionChanged += Cb_ToolCopyFrom_SelectionChanged;
			break;
		case 33:
			btn_ReDraw1 = (Button)target;
			break;
		case 34:
			cb_ToolCopyFrom = (ComboBox)target;
			cb_ToolCopyFrom.SelectionChanged += Cb_ToolCopyFrom_SelectionChanged;
			break;
		case 35:
			btn_ReDraw_Copy1 = (Button)target;
			break;
		case 36:
			cb_ToolCopySelect = (ComboBox)target;
			cb_ToolCopySelect.SelectionChanged += Cb_ToolCopyFrom_SelectionChanged;
			break;
		case 37:
			cb_TCopyToolIndexTo = (ComboBox)target;
			break;
		case 38:
			cb_TCopyToolIndexFrom = (ComboBox)target;
			cb_TCopyToolIndexFrom.SelectionChanged += Cb_TCopyToolIndexFrom_SelectionChanged;
			break;
		case 39:
			btn_ToolsCopy = (Button)target;
			btn_ToolsCopy.Click += Btn_ToolsCopy_Click;
			break;
		case 40:
			btn_ReDraw1_Copy = (Button)target;
			break;
		case 41:
			btn_ReDraw_Copy2 = (Button)target;
			break;
		case 42:
			btn_ReDraw1_Copy1 = (Button)target;
			break;
		case 43:
			cb_SwitchToolIndex2 = (ComboBox)target;
			break;
		case 44:
			cb_SwitchToolIndex1 = (ComboBox)target;
			break;
		case 45:
			btn_ToolsSwitch = (Button)target;
			btn_ToolsSwitch.Click += Btn_ToolsSwitch_Click;
			break;
		case 46:
			btn_ReDraw1_Copy2 = (Button)target;
			break;
		case 47:
			btn_ReDraw_Copy3 = (Button)target;
			break;
		case 48:
			btn_ReDraw1_Copy3 = (Button)target;
			break;
		case 49:
			gb_StationCopy = (GroupBox)target;
			break;
		case 50:
			cb_RecipeCopyTo = (ComboBox)target;
			cb_RecipeCopyTo.SelectionChanged += Cb_ToolCopyFrom_SelectionChanged;
			break;
		case 51:
			btn_ReDraw2 = (Button)target;
			break;
		case 52:
			cb_RecipeCopyFrom = (ComboBox)target;
			cb_RecipeCopyFrom.SelectionChanged += Cb_ToolCopyFrom_SelectionChanged;
			break;
		case 53:
			btn_ReDraw_Copy4 = (Button)target;
			break;
		case 54:
			btn_RecipeCopy = (Button)target;
			btn_RecipeCopy.Click += Btn_RecipeCopy_Click;
			break;
		case 55:
			cb_Station1 = (CheckBox)target;
			break;
		case 56:
			cb_Station2 = (CheckBox)target;
			break;
		case 57:
			cb_Station3 = (CheckBox)target;
			break;
		case 58:
			cb_Station4 = (CheckBox)target;
			break;
		case 59:
			cb_Station5 = (CheckBox)target;
			break;
		case 60:
			cb_Station6 = (CheckBox)target;
			break;
		case 61:
			cb_Station7 = (CheckBox)target;
			break;
		case 62:
			cb_Station8 = (CheckBox)target;
			break;
		case 63:
			rad_CopyAll = (RadioButton)target;
			break;
		case 64:
			rad_CopyName = (RadioButton)target;
			break;
		case 65:
			btn_ReDraw2_Copy = (Button)target;
			break;
		case 66:
			btn_ReDraw_Copy5 = (Button)target;
			break;
		case 67:
			cb_Station9 = (CheckBox)target;
			break;
		case 68:
			cb_Station10 = (CheckBox)target;
			break;
		case 69:
			gb_RecipeCopy = (GroupBox)target;
			break;
		case 70:
			cb_RC_Sour = (ComboBox)target;
			break;
		case 71:
			tab_PublicPara = (TabItem)target;
			break;
		case 72:
			cb_RC_StationPos = (CheckBox)target;
			break;
		case 73:
			cb_RC_FlowPos = (CheckBox)target;
			break;
		case 74:
			cb_RC_PublicPara = (CheckBox)target;
			break;
		case 75:
			btn_PublicPara_Copy = (Button)target;
			btn_PublicPara_Copy.Click += Btn_PublicPara_Copy_Click;
			break;
		case 76:
			tab_StationPara = (TabItem)target;
			break;
		case 77:
			cb_RC_S1 = (CheckBox)target;
			break;
		case 78:
			cb_RC_S2 = (CheckBox)target;
			break;
		case 79:
			cb_RC_S3 = (CheckBox)target;
			break;
		case 80:
			cb_RC_S4 = (CheckBox)target;
			break;
		case 81:
			cb_RC_S5 = (CheckBox)target;
			break;
		case 82:
			cb_RC_S6 = (CheckBox)target;
			break;
		case 83:
			cb_RC_S7 = (CheckBox)target;
			break;
		case 84:
			cb_RC_S8 = (CheckBox)target;
			break;
		case 85:
			cb_RC_S9 = (CheckBox)target;
			break;
		case 86:
			cb_RC_S10 = (CheckBox)target;
			break;
		case 87:
			btn_StationPara_Copy = (Button)target;
			btn_StationPara_Copy.Click += Btn_StationPara_Copy_Click;
			break;
		case 88:
			cb_RC_LightPara = (CheckBox)target;
			break;
		case 89:
			cb_RC_CameraPara = (CheckBox)target;
			break;
		case 90:
			cb_RC_PixelPerUm = (CheckBox)target;
			break;
		case 91:
			cb_RC_LocateTool = (CheckBox)target;
			break;
		case 92:
			cb_RC_Tools = (ComboBox)target;
			cb_RC_Tools.SelectionChanged += Cb_RC_Tools_SelectionChanged;
			break;
		case 93:
			cb_RC_ToolIndex = (ComboBox)target;
			break;
		case 94:
			tab_CustomPara = (TabItem)target;
			break;
		case 95:
			txt_RC_TagName = (TextBox)target;
			break;
		case 96:
			txt_RC_ItemName = (TextBox)target;
			break;
		case 97:
			cb_RC_Station = (ComboBox)target;
			cb_RC_Station.SelectionChanged += Cb_RC_Tools_SelectionChanged;
			break;
		case 98:
			btn_CustomPara_Copy = (Button)target;
			btn_CustomPara_Copy.Click += Btn_CustomPara_Copy_Click;
			break;
		case 99:
			btn_RecipeCopyDest = (Button)target;
			btn_RecipeCopyDest.Click += Btn_RecipeCopyDest_Click;
			break;
		case 100:
			txt_RC_Dest = (TextBox)target;
			break;
		case 101:
			grd_Loading = (Grid)target;
			break;
		case 102:
			lab_Loading = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
