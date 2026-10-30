using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;

namespace InspTools.Other;

public class InspSurvWindow : Window, IComponentConnector
{
	private const int MaxInspSurvItems = 20;

	private int LanguageType = 1;

	private CVInspection[] inspection;

	private ThroughputParamFile ThroughputFile;

	public List<INSPSURVITEM> IsItems = new List<INSPSURVITEM>
	{
		new INSPSURVITEM
		{
			ID = 0,
			Station = 0,
			InspToolID = 0,
			InspToolName = "",
			TrackID = 0,
			Status = true,
			StatusStr = "",
			TrackName = "",
			AlarmRatio = 100.0
		}
	};

	private bool UpdateUI;

	internal Button btn_Add;

	internal ComboBox cb_InspToolName;

	internal ComboBox cb_TrackName;

	internal ComboBox cb_Station;

	internal TextBox txt_AlarmRatio;

	internal ListView lv_IsList;

	internal Button btn_Delete;

	private bool _contentLoaded;

	public InspSurvWindow()
	{
		InitializeComponent();
		UpdateUI = true;
	}

	public void SetInspection(CVInspection[] station)
	{
		inspection = station;
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
		CopyValueToUI();
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

	private void CopyValueToUI()
	{
		int num = 1;
		IsItems.Clear();
		for (int i = 0; i < 20; i++)
		{
			string text = ThroughputFile.param.InspSurvItems[i];
			double alarmRatio = ThroughputFile.param.InspSurvItemsAlarmRatio[i];
			if (text != "")
			{
				IsItems.Add(ProcessString(text, num, alarmRatio));
				num++;
			}
		}
		ThroughputFile.IsItems = IsItems.ToList();
		if (IsItems.Count > 0)
		{
			lv_IsList.ItemsSource = null;
			lv_IsList.ItemsSource = IsItems;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteUIValueToParam()
	{
		if (IsItems.Count < 0 || ThroughputFile == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < 20; i++)
		{
			string text = "";
			if (IsItems.Count > i)
			{
				num = (IsItems[i].Status ? 1 : 0);
				text = string.Format("{0}{1:D2}{2:D2}{3:D1}", IsItems[i].Station, IsItems[i].InspToolID, IsItems[i].TrackID, num);
				ThroughputFile.param.InspSurvItemsAlarmRatio[i] = IsItems[i].AlarmRatio;
			}
			ThroughputFile.param.InspSurvItems[i] = text;
		}
		ThroughputFile.IsItems = IsItems.ToList();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private INSPSURVITEM ProcessString(string str, int count, double AlarmRatio)
	{
		INSPSURVITEM iNSPSURVITEM = new INSPSURVITEM();
		try
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			iNSPSURVITEM.ID = count;
			num = Convert.ToInt32(str.Substring(0, 1));
			num2 = Convert.ToInt32(str.Substring(1, 2));
			num3 = Convert.ToInt32(str.Substring(3, 2));
			iNSPSURVITEM.Station = num;
			iNSPSURVITEM.InspToolID = num2;
			iNSPSURVITEM.InspToolName = GetInspToolName(num2);
			iNSPSURVITEM.TrackID = num3;
			bool status = true;
			iNSPSURVITEM.TrackName = GetTrackName(num, num2, num3, ref status);
			iNSPSURVITEM.Status = status;
			if (status)
			{
				iNSPSURVITEM.StatusStr = LanguageManager.Instance["Open"];
			}
			else
			{
				iNSPSURVITEM.StatusStr = LanguageManager.Instance["Close"];
			}
			iNSPSURVITEM.StationStr = GetStationStr(num);
			iNSPSURVITEM.AlarmRatio = AlarmRatio;
			return iNSPSURVITEM;
		}
		catch (Exception ex)
		{
			string text = "ISW03-InspSurvWindow-Btn_Delete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return iNSPSURVITEM;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetStationStr(int station)
	{
		return station switch
		{
			0 => LanguageManager.Instance["Station1"], 
			1 => LanguageManager.Instance["Station2"], 
			2 => LanguageManager.Instance["Station3"], 
			3 => LanguageManager.Instance["Station4"], 
			4 => LanguageManager.Instance["Station5"], 
			5 => LanguageManager.Instance["Station6"], 
			6 => LanguageManager.Instance["Station7"], 
			7 => LanguageManager.Instance["Station8"], 
			_ => LanguageManager.Instance["Station1"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetInspToolName(int toolid)
	{
		try
		{
			string result = "";
			switch (toolid)
			{
			case 0:
				result = LanguageManager.Instance["Adv.Inside"];
				break;
			case 1:
				result = LanguageManager.Instance["Adv.Measure"];
				break;
			case 2:
				result = LanguageManager.Instance["MeasureTool1"];
				break;
			case 3:
				result = LanguageManager.Instance["MeasureTool2"];
				break;
			case 4:
				result = LanguageManager.Instance["BinaryTool"];
				break;
			case 5:
				result = LanguageManager.Instance["SmoothTool"];
				break;
			case 6:
				result = LanguageManager.Instance["BlockAvgTool"];
				break;
			case 7:
				result = LanguageManager.Instance["DifferenceTool"];
				break;
			case 8:
				result = LanguageManager.Instance["LineAnalysisTool"];
				break;
			case 9:
				result = LanguageManager.Instance["MatchTool"];
				break;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "ISW02-InspSurvWindow-Btn_Delete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
			return text;
		}
	}

	private string GetTrackName(int station, int toolid, int trackno, ref bool status)
	{
		string result = "";
		switch (toolid)
		{
		case 0:
			result = inspection[station].paramFile.AdvLocateParam[trackno].TrackerName;
			if (!inspection[station].paramFile.AdvLocateToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.AdvLocateParam[trackno].TrackerON;
			}
			break;
		case 1:
			result = inspection[station].paramFile.AdvLocateMParam[trackno].TrackerName;
			if (!inspection[station].paramFile.AdvLocateToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.AdvLocateMParam[trackno].TrackerON;
			}
			break;
		case 2:
			result = inspection[station].paramFile.MeaWidthParam[trackno].TrackerName;
			if (!inspection[station].paramFile.MeasureToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.MeaWidthParam[trackno].TrackerON;
			}
			break;
		case 3:
			result = inspection[station].paramFile.MeaHeightParam[trackno].TrackerName;
			if (!inspection[station].paramFile.MeasureToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.MeaHeightParam[trackno].TrackerON;
			}
			break;
		case 4:
			result = inspection[station].paramFile.BinaryParam[trackno].TrackerName;
			if (!inspection[station].paramFile.BinaryToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.BinaryParam[trackno].TrackerON;
			}
			break;
		case 5:
			result = inspection[station].paramFile.SmoothParam[trackno].TrackerName;
			if (!inspection[station].paramFile.SmoothToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.SmoothParam[trackno].TrackerON;
			}
			break;
		case 6:
			result = inspection[station].paramFile.BlockAvgParam[trackno].TrackerName;
			if (!inspection[station].paramFile.BlockAvgToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.BlockAvgParam[trackno].TrackerON;
			}
			break;
		case 7:
			result = inspection[station].paramFile.DifferenceParam[trackno].TrackerName;
			if (!inspection[station].paramFile.DifferenceToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.DifferenceParam[trackno].TrackerON;
			}
			break;
		case 8:
			result = inspection[station].paramFile.LineAnalysisParam[trackno].TrackerName;
			if (!inspection[station].paramFile.LineAnalysisToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.LineAnalysisParam[trackno].TrackerON;
			}
			break;
		case 9:
			result = inspection[station].paramFile.MatchParam[trackno].TrackerName;
			if (!inspection[station].paramFile.MaskToolON)
			{
				status = false;
			}
			else
			{
				status = inspection[station].paramFile.MatchParam[trackno].TrackerON;
			}
			break;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Add_Click(object sender, RoutedEventArgs e)
	{
		if (cb_TrackName.SelectedIndex == -1)
		{
			return;
		}
		if (IsItems.Count >= 20)
		{
			MessageBox.Show("超過監測限制筆數(20)", "警告");
			return;
		}
		try
		{
			for (int i = 0; i < IsItems.Count; i++)
			{
				if (IsItems[i].Station == cb_Station.SelectedIndex && IsItems[i].InspToolID == cb_InspToolName.SelectedIndex && IsItems[i].TrackID == cb_TrackName.SelectedIndex)
				{
					IsItems[i].AlarmRatio = Convert.ToDouble(txt_AlarmRatio.Text);
					lv_IsList.ItemsSource = null;
					lv_IsList.ItemsSource = IsItems;
					return;
				}
			}
			INSPSURVITEM iNSPSURVITEM = new INSPSURVITEM();
			iNSPSURVITEM.ID = IsItems.Count + 1;
			iNSPSURVITEM.Station = cb_Station.SelectedIndex;
			iNSPSURVITEM.InspToolID = cb_InspToolName.SelectedIndex;
			iNSPSURVITEM.TrackID = cb_TrackName.SelectedIndex;
			iNSPSURVITEM.InspToolName = GetInspToolName(iNSPSURVITEM.InspToolID);
			bool status = true;
			iNSPSURVITEM.TrackName = GetTrackName(iNSPSURVITEM.Station, iNSPSURVITEM.InspToolID, iNSPSURVITEM.TrackID, ref status);
			iNSPSURVITEM.Status = status;
			iNSPSURVITEM.AlarmRatio = Convert.ToDouble(txt_AlarmRatio.Text);
			if (status)
			{
				iNSPSURVITEM.StatusStr = LanguageManager.Instance["Open"];
			}
			else
			{
				iNSPSURVITEM.StatusStr = LanguageManager.Instance["Close"];
			}
			iNSPSURVITEM.StationStr = GetStationStr(iNSPSURVITEM.Station);
			IsItems.Add(iNSPSURVITEM);
			lv_IsList.ItemsSource = null;
			lv_IsList.ItemsSource = IsItems;
		}
		catch (Exception ex)
		{
			string text = "ISW03-InspSurvWindow-Btn_Add_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_InspToolName_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		try
		{
			int selectedIndex = cb_Station.SelectedIndex;
			int selectedIndex2 = cb_InspToolName.SelectedIndex;
			string text = "";
			cb_TrackName.Items.Clear();
			switch (selectedIndex2)
			{
			case 0:
			{
				for (int l = 0; l < 20; l++)
				{
					text = string.Format("{0}:{1}", l + 1, inspection[selectedIndex].paramFile.AdvLocateParam[l].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 1:
			{
				for (int num2 = 0; num2 < 40; num2++)
				{
					text = string.Format("{0}:{1}", num2 + 1, inspection[selectedIndex].paramFile.AdvLocateMParam[num2].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 2:
			{
				for (int num4 = 0; num4 < 20; num4++)
				{
					text = string.Format("{0}:{1}", num4 + 1, inspection[selectedIndex].paramFile.MeaWidthParam[num4].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 3:
			{
				for (int n = 0; n < 20; n++)
				{
					text = string.Format("{0}:{1}", n + 1, inspection[selectedIndex].paramFile.MeaHeightParam[n].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 4:
			{
				for (int j = 0; j < 30; j++)
				{
					text = string.Format("{0}:{1}", j + 1, inspection[selectedIndex].paramFile.BinaryParam[j].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 5:
			{
				for (int num3 = 0; num3 < 20; num3++)
				{
					text = string.Format("{0}:{1}", num3 + 1, inspection[selectedIndex].paramFile.SmoothParam[num3].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 6:
			{
				for (int num = 0; num < 20; num++)
				{
					text = string.Format("{0}:{1}", num + 1, inspection[selectedIndex].paramFile.BlockAvgParam[num].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 7:
			{
				for (int m = 0; m < 20; m++)
				{
					text = string.Format("{0}:{1}", m + 1, inspection[selectedIndex].paramFile.DifferenceParam[m].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 8:
			{
				for (int k = 0; k < 20; k++)
				{
					text = string.Format("{0}:{1}", k + 1, inspection[selectedIndex].paramFile.LineAnalysisParam[k].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			case 9:
			{
				for (int i = 0; i < 20; i++)
				{
					text = string.Format("{0}:{1}", i + 1, inspection[selectedIndex].paramFile.MatchParam[i].TrackerName);
					cb_TrackName.Items.Add(text);
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			string text2 = "ISW01-InspSurvWindow-Cb_InspToolName_SelectionChanged()-" + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, "錯誤");
		}
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		WriteUIValueToParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Delete_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = lv_IsList.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < IsItems.Count)
			{
				MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Selected item will be delete！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("选取的项目即将删除！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("選取的項目即將刪除！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
				if (messageBoxResult == MessageBoxResult.Yes && messageBoxResult == MessageBoxResult.Yes)
				{
					lv_IsList.ItemsSource = null;
					IsItems.RemoveAt(selectedIndex);
					ResetIsListID();
					lv_IsList.ItemsSource = IsItems;
				}
			}
		}
		catch (Exception ex)
		{
			string text = "ISW02-InspSurvWindow-Btn_Delete_Click()-" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void ResetIsListID()
	{
		for (int i = 0; i < IsItems.Count; i++)
		{
			IsItems[i].ID = i + 1;
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
			Uri resourceLocator = new Uri("/InspTools;component/other/inspsurvwindow.xaml", UriKind.Relative);
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
			((InspSurvWindow)target).Closed += Window_Closed;
			break;
		case 2:
			btn_Add = (Button)target;
			btn_Add.Click += Btn_Add_Click;
			break;
		case 3:
			cb_InspToolName = (ComboBox)target;
			cb_InspToolName.SelectionChanged += Cb_InspToolName_SelectionChanged;
			break;
		case 4:
			cb_TrackName = (ComboBox)target;
			break;
		case 5:
			cb_Station = (ComboBox)target;
			cb_Station.SelectionChanged += Cb_InspToolName_SelectionChanged;
			break;
		case 6:
			txt_AlarmRatio = (TextBox)target;
			break;
		case 7:
			lv_IsList = (ListView)target;
			break;
		case 8:
			btn_Delete = (Button)target;
			btn_Delete.Click += Btn_Delete_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
