using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;

namespace InspTools.Other;

public class QuickSetupWindow : Window, IComponentConnector
{
	private ComboBox[] cbe;

	private ComboBox[] ParetoChart;

	private CheckBox[] cb;

	private CheckBox[] cbl;

	private TextBox[] txt;

	private Label[] lab;

	private bool Inited;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private int LanguageType = 1;

	internal Button btn_QuickSetupSave;

	internal ComboBox cb_ToolSelect;

	internal CheckBox cb_SelectAll;

	internal CheckBox cb_UnSelectAll;

	internal Button btn_ChannelPreview1;

	internal Button btn_QuickSetupSave_Copy;

	internal Label lab_01;

	internal CheckBox cb01;

	internal TextBox txt01;

	internal ComboBox cbe01;

	internal Label lab_Pareto;

	internal ComboBox cb_PC01;

	internal CheckBox cbl01;

	internal Label lab_02;

	internal CheckBox cb02;

	internal TextBox txt02;

	internal ComboBox cbe02;

	internal Label lab_Pareto_Copy;

	internal ComboBox cb_PC02;

	internal CheckBox cbl02;

	internal Label lab_03;

	internal CheckBox cb03;

	internal TextBox txt03;

	internal ComboBox cbe03;

	internal Label lab_Pareto_Copy1;

	internal ComboBox cb_PC03;

	internal CheckBox cbl03;

	internal Label lab_04;

	internal CheckBox cb04;

	internal TextBox txt04;

	internal ComboBox cbe04;

	internal Label lab_Pareto_Copy2;

	internal ComboBox cb_PC04;

	internal CheckBox cbl04;

	internal Label lab_05;

	internal CheckBox cb05;

	internal TextBox txt05;

	internal ComboBox cbe05;

	internal Label lab_Pareto_Copy3;

	internal ComboBox cb_PC05;

	internal CheckBox cbl05;

	internal Label lab_06;

	internal CheckBox cb06;

	internal TextBox txt06;

	internal ComboBox cbe06;

	internal Label lab_Pareto_Copy4;

	internal ComboBox cb_PC06;

	internal CheckBox cbl06;

	internal Label lab_07;

	internal CheckBox cb07;

	internal TextBox txt07;

	internal ComboBox cbe07;

	internal Label lab_Pareto_Copy5;

	internal ComboBox cb_PC07;

	internal CheckBox cbl07;

	internal Label lab_08;

	internal CheckBox cb08;

	internal TextBox txt08;

	internal ComboBox cbe08;

	internal Label lab_Pareto_Copy6;

	internal ComboBox cb_PC08;

	internal CheckBox cbl08;

	internal Label lab_09;

	internal CheckBox cb09;

	internal TextBox txt09;

	internal ComboBox cbe09;

	internal Label lab_Pareto_Copy7;

	internal ComboBox cb_PC09;

	internal CheckBox cbl09;

	internal Label lab_10;

	internal CheckBox cb10;

	internal TextBox txt10;

	internal ComboBox cbe10;

	internal Label lab_Pareto_Copy8;

	internal ComboBox cb_PC10;

	internal CheckBox cbl10;

	internal Label lab_11;

	internal CheckBox cb11;

	internal TextBox txt11;

	internal ComboBox cbe11;

	internal Label lab_Pareto_Copy9;

	internal ComboBox cb_PC11;

	internal CheckBox cbl11;

	internal Label lab_12;

	internal CheckBox cb12;

	internal TextBox txt12;

	internal ComboBox cbe12;

	internal Label lab_Pareto_Copy10;

	internal ComboBox cb_PC12;

	internal CheckBox cbl12;

	internal Label lab_13;

	internal CheckBox cb13;

	internal TextBox txt13;

	internal ComboBox cbe13;

	internal Label lab_Pareto_Copy11;

	internal ComboBox cb_PC13;

	internal CheckBox cbl13;

	internal Label lab_14;

	internal CheckBox cb14;

	internal TextBox txt14;

	internal ComboBox cbe14;

	internal Label lab_Pareto_Copy12;

	internal ComboBox cb_PC14;

	internal CheckBox cbl14;

	internal Label lab_15;

	internal CheckBox cb15;

	internal TextBox txt15;

	internal ComboBox cbe15;

	internal Label lab_Pareto_Copy13;

	internal ComboBox cb_PC15;

	internal CheckBox cbl15;

	internal Label lab_16;

	internal CheckBox cb16;

	internal TextBox txt16;

	internal ComboBox cbe16;

	internal Label lab_Pareto_Copy14;

	internal ComboBox cb_PC16;

	internal CheckBox cbl16;

	internal Label lab_17;

	internal CheckBox cb17;

	internal TextBox txt17;

	internal ComboBox cbe17;

	internal Label lab_Pareto_Copy15;

	internal ComboBox cb_PC17;

	internal CheckBox cbl17;

	internal Label lab_18;

	internal CheckBox cb18;

	internal TextBox txt18;

	internal ComboBox cbe18;

	internal Label lab_Pareto_Copy16;

	internal ComboBox cb_PC18;

	internal CheckBox cbl18;

	internal Label lab_19;

	internal CheckBox cb19;

	internal TextBox txt19;

	internal ComboBox cbe19;

	internal Label lab_Pareto_Copy17;

	internal ComboBox cb_PC19;

	internal CheckBox cbl19;

	internal Label lab_20;

	internal CheckBox cb20;

	internal TextBox txt20;

	internal ComboBox cbe20;

	internal Label lab_Pareto_Copy18;

	internal ComboBox cb_PC20;

	internal CheckBox cbl20;

	internal Label lab_21;

	internal CheckBox cb21;

	internal TextBox txt21;

	internal ComboBox cbe21;

	internal Label lab_Pareto_Copy19;

	internal ComboBox cb_PC21;

	internal CheckBox cbl21;

	internal Label lab_22;

	internal CheckBox cb22;

	internal TextBox txt22;

	internal ComboBox cbe22;

	internal Label lab_Pareto_Copy20;

	internal ComboBox cb_PC22;

	internal CheckBox cbl22;

	internal Label lab_23;

	internal CheckBox cb23;

	internal TextBox txt23;

	internal ComboBox cbe23;

	internal Label lab_Pareto_Copy21;

	internal ComboBox cb_PC23;

	internal CheckBox cbl23;

	internal Label lab_24;

	internal CheckBox cb24;

	internal TextBox txt24;

	internal ComboBox cbe24;

	internal Label lab_Pareto_Copy22;

	internal ComboBox cb_PC24;

	internal CheckBox cbl24;

	internal Label lab_25;

	internal CheckBox cb25;

	internal TextBox txt25;

	internal ComboBox cbe25;

	internal Label lab_Pareto_Copy23;

	internal ComboBox cb_PC25;

	internal CheckBox cbl25;

	internal Label lab_26;

	internal CheckBox cb26;

	internal TextBox txt26;

	internal ComboBox cbe26;

	internal Label lab_Pareto_Copy24;

	internal ComboBox cb_PC26;

	internal CheckBox cbl26;

	internal Label lab_27;

	internal CheckBox cb27;

	internal TextBox txt27;

	internal ComboBox cbe27;

	internal Label lab_Pareto_Copy25;

	internal ComboBox cb_PC27;

	internal CheckBox cbl27;

	internal Label lab_28;

	internal CheckBox cb28;

	internal TextBox txt28;

	internal ComboBox cbe28;

	internal Label lab_Pareto_Copy26;

	internal ComboBox cb_PC28;

	internal CheckBox cbl28;

	internal Label lab_29;

	internal CheckBox cb29;

	internal TextBox txt29;

	internal ComboBox cbe29;

	internal Label lab_Pareto_Copy27;

	internal ComboBox cb_PC29;

	internal CheckBox cbl29;

	internal Label lab_30;

	internal CheckBox cb30;

	internal TextBox txt30;

	internal ComboBox cbe30;

	internal Label lab_Pareto_Copy28;

	internal ComboBox cb_PC30;

	internal CheckBox cbl30;

	private bool _contentLoaded;

	public QuickSetupWindow()
	{
		InitializeComponent();
		cb = new CheckBox[30]
		{
			cb01, cb02, cb03, cb04, cb05, cb06, cb07, cb08, cb09, cb10,
			cb11, cb12, cb13, cb14, cb15, cb16, cb17, cb18, cb19, cb20,
			cb21, cb22, cb23, cb24, cb25, cb26, cb27, cb28, cb29, cb30
		};
		cbl = new CheckBox[30]
		{
			cbl01, cbl02, cbl03, cbl04, cbl05, cbl06, cbl07, cbl08, cbl09, cbl10,
			cbl11, cbl12, cbl13, cbl14, cbl15, cbl16, cbl17, cbl18, cbl19, cbl20,
			cbl21, cbl22, cbl23, cbl24, cbl25, cbl26, cbl27, cbl28, cbl29, cbl30
		};
		cbe = new ComboBox[30]
		{
			cbe01, cbe02, cbe03, cbe04, cbe05, cbe06, cbe07, cbe08, cbe09, cbe10,
			cbe11, cbe12, cbe13, cbe14, cbe15, cbe16, cbe17, cbe18, cbe19, cbe20,
			cbe21, cbe22, cbe23, cbe24, cbe25, cbe26, cbe27, cbe28, cbe29, cbe30
		};
		txt = new TextBox[30]
		{
			txt01, txt02, txt03, txt04, txt05, txt06, txt07, txt08, txt09, txt10,
			txt11, txt12, txt13, txt14, txt15, txt16, txt17, txt18, txt19, txt20,
			txt21, txt22, txt23, txt24, txt25, txt26, txt27, txt28, txt29, txt30
		};
		ParetoChart = new ComboBox[30]
		{
			cb_PC01, cb_PC02, cb_PC03, cb_PC04, cb_PC05, cb_PC06, cb_PC07, cb_PC08, cb_PC09, cb_PC10,
			cb_PC11, cb_PC12, cb_PC13, cb_PC14, cb_PC15, cb_PC16, cb_PC17, cb_PC18, cb_PC19, cb_PC20,
			cb_PC21, cb_PC22, cb_PC23, cb_PC24, cb_PC25, cb_PC26, cb_PC27, cb_PC28, cb_PC29, cb_PC30
		};
		lab = new Label[30]
		{
			lab_01, lab_02, lab_03, lab_04, lab_05, lab_06, lab_07, lab_08, lab_09, lab_10,
			lab_11, lab_12, lab_13, lab_14, lab_15, lab_16, lab_17, lab_18, lab_19, lab_20,
			lab_21, lab_22, lab_23, lab_24, lab_25, lab_26, lab_27, lab_28, lab_29, lab_30
		};
		Inited = true;
		for (int i = 0; i < 30; i++)
		{
			cbl[i].Visibility = Visibility.Visible;
			cbl[i].IsChecked = false;
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

	public void SetInspection(CVInspection station)
	{
		inspection = station;
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
		for (int i = 0; i < 30; i++)
		{
			ParetoChart[i].Items.Clear();
		}
		for (int j = 0; j < 30; j++)
		{
			if (ThroughputFile.param.AllParetoItems[j] != "")
			{
				for (int k = 0; k < 30; k++)
				{
					ParetoChart[k].Items.Add(ThroughputFile.param.AllParetoItems[j]);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_ToolSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!Inited || cb_ToolSelect.SelectedIndex < 0)
		{
			return;
		}
		for (int i = 0; i < 30; i++)
		{
			lab[i].Background = Brushes.White;
		}
		for (int j = 0; j < 20; j++)
		{
			cb[j].IsChecked = false;
			cbe[j].SelectedIndex = -1;
			txt[j].Text = "";
			cb[j].IsEnabled = true;
			cbe[j].IsEnabled = true;
			txt[j].IsEnabled = true;
			ParetoChart[j].SelectedIndex = -1;
		}
		for (int k = 0; k < 30; k++)
		{
			cbl[k].Visibility = Visibility.Visible;
			cbl[k].IsChecked = false;
		}
		switch (cb_ToolSelect.SelectedIndex)
		{
		case 0:
		{
			cb[0].IsChecked = true;
			cb[1].IsChecked = true;
			if (LanguageType == 0)
			{
				txt[0].Text = "Locate Left";
				txt[1].Text = "Locate Right";
			}
			else if (LanguageType == 1)
			{
				txt[0].Text = "定位左邊";
				txt[1].Text = "定位右邊";
			}
			else
			{
				txt[0].Text = "定位左边";
				txt[1].Text = "定位右边";
			}
			cb[0].IsEnabled = false;
			cbe[0].IsEnabled = false;
			txt[0].IsEnabled = false;
			cb[1].IsEnabled = false;
			cbe[1].IsEnabled = false;
			txt[1].IsEnabled = false;
			for (int num9 = 2; num9 < 20; num9++)
			{
				cb[num9].IsChecked = inspection.paramFile.VSLocateParam[num9].TrackerON;
				cbe[num9].SelectedIndex = (int)inspection.paramFile.VSLocateParam[num9].ErrorBin;
				txt[num9].Text = inspection.paramFile.VSLocateParam[num9].TrackerName;
				ParetoChart[num9].SelectedIndex = inspection.paramFile.VSLocateParam[num9].ParetoIndex;
				if (inspection.paramFile.VSLocateParam[num9].ValueChangeFlag)
				{
					lab[num9].Background = Brushes.Lime;
				}
				cbl[num9].IsChecked = inspection.paramFile.VSLocateParam[num9].Locked;
			}
			break;
		}
		case 1:
		{
			cb[0].IsChecked = true;
			cb[1].IsChecked = true;
			if (LanguageType == 0)
			{
				txt[0].Text = "Locate Top";
				txt[1].Text = "Locate Bottom";
			}
			else if (LanguageType == 1)
			{
				txt[0].Text = "定位上邊";
				txt[1].Text = "定位下邊";
			}
			else
			{
				txt[0].Text = "定位上边";
				txt[1].Text = "定位下边";
			}
			cb[0].IsEnabled = false;
			cbe[0].IsEnabled = false;
			txt[0].IsEnabled = false;
			cb[1].IsEnabled = false;
			cbe[1].IsEnabled = false;
			txt[1].IsEnabled = false;
			for (int num5 = 2; num5 < 20; num5++)
			{
				cb[num5].IsChecked = inspection.paramFile.HSLocateParam[num5].TrackerON;
				cbe[num5].SelectedIndex = (int)inspection.paramFile.HSLocateParam[num5].ErrorBin;
				txt[num5].Text = inspection.paramFile.HSLocateParam[num5].TrackerName;
				ParetoChart[num5].SelectedIndex = inspection.paramFile.HSLocateParam[num5].ParetoIndex;
				if (inspection.paramFile.HSLocateParam[num5].ValueChangeFlag)
				{
					lab[num5].Background = Brushes.Lime;
				}
				cbl[num5].IsChecked = inspection.paramFile.HSLocateParam[num5].Locked;
			}
			break;
		}
		case 2:
		{
			cb[0].IsChecked = true;
			if (LanguageType == 0)
			{
				txt[0].Text = "Locate Rect";
			}
			else if (LanguageType == 1)
			{
				txt[0].Text = "定位框";
			}
			else
			{
				txt[0].Text = "定位框";
			}
			cb[0].IsEnabled = false;
			cbe[0].IsEnabled = false;
			txt[0].IsEnabled = false;
			for (int num = 1; num < 20; num++)
			{
				cb[num].IsChecked = inspection.paramFile.AdvLocateParam[num].TrackerON;
				cbe[num].SelectedIndex = (int)inspection.paramFile.AdvLocateParam[num].ErrorBin;
				txt[num].Text = inspection.paramFile.AdvLocateParam[num].TrackerName;
				ParetoChart[num].SelectedIndex = inspection.paramFile.AdvLocateParam[num].ParetoIndex;
				if (inspection.paramFile.AdvLocateParam[num].ValueChangeFlag)
				{
					lab[num].Background = Brushes.Lime;
				}
				cbl[num].IsChecked = inspection.paramFile.AdvLocateParam[num].Locked;
			}
			break;
		}
		case 3:
		{
			for (int num7 = 0; num7 < 30; num7++)
			{
				cb[num7].IsChecked = inspection.paramFile.AdvLocateMParam[num7].TrackerON;
				cbe[num7].SelectedIndex = (int)inspection.paramFile.AdvLocateMParam[num7].ErrorBin;
				txt[num7].Text = inspection.paramFile.AdvLocateMParam[num7].TrackerName;
				ParetoChart[num7].SelectedIndex = inspection.paramFile.AdvLocateMParam[num7].ParetoIndex;
				if (inspection.paramFile.AdvLocateMParam[num7].ValueChangeFlag)
				{
					lab[num7].Background = Brushes.Lime;
				}
				cbl[num7].IsChecked = inspection.paramFile.AdvLocateMParam[num7].Locked;
			}
			break;
		}
		case 4:
		{
			for (int num3 = 0; num3 < 20; num3++)
			{
				cb[num3].IsChecked = inspection.paramFile.MeaWidthParam[num3].TrackerON;
				cbe[num3].SelectedIndex = (int)inspection.paramFile.MeaWidthParam[num3].ErrorBin;
				txt[num3].Text = inspection.paramFile.MeaWidthParam[num3].TrackerName;
				ParetoChart[num3].SelectedIndex = inspection.paramFile.MeaWidthParam[num3].ParetoIndex;
				if (inspection.paramFile.MeaWidthParam[num3].ValueChangeFlag)
				{
					lab[num3].Background = Brushes.Lime;
				}
				cbl[num3].IsChecked = inspection.paramFile.MeaWidthParam[num3].Locked;
			}
			break;
		}
		case 5:
		{
			for (int m = 0; m < 20; m++)
			{
				cb[m].IsChecked = inspection.paramFile.MeaHeightParam[m].TrackerON;
				cbe[m].SelectedIndex = (int)inspection.paramFile.MeaHeightParam[m].ErrorBin;
				txt[m].Text = inspection.paramFile.MeaHeightParam[m].TrackerName;
				ParetoChart[m].SelectedIndex = inspection.paramFile.MeaHeightParam[m].ParetoIndex;
				if (inspection.paramFile.MeaHeightParam[m].ValueChangeFlag)
				{
					lab[m].Background = Brushes.Lime;
				}
				cbl[m].IsChecked = inspection.paramFile.MeaHeightParam[m].Locked;
			}
			break;
		}
		case 6:
		{
			for (int num8 = 0; num8 < 30; num8++)
			{
				cb[num8].IsChecked = inspection.paramFile.BinaryParam[num8].TrackerON;
				cbe[num8].SelectedIndex = (int)inspection.paramFile.BinaryParam[num8].ErrorBin;
				txt[num8].Text = inspection.paramFile.BinaryParam[num8].TrackerName;
				ParetoChart[num8].SelectedIndex = inspection.paramFile.BinaryParam[num8].ParetoIndex;
				if (inspection.paramFile.BinaryParam[num8].ValueChangeFlag)
				{
					lab[num8].Background = Brushes.Lime;
				}
				cbl[num8].Visibility = Visibility.Visible;
				cbl[num8].IsChecked = inspection.paramFile.BinaryParam[num8].Locked;
			}
			break;
		}
		case 7:
		{
			for (int num6 = 0; num6 < 20; num6++)
			{
				cb[num6].IsChecked = inspection.paramFile.SmoothParam[num6].TrackerON;
				cbe[num6].SelectedIndex = (int)inspection.paramFile.SmoothParam[num6].ErrorBin;
				txt[num6].Text = inspection.paramFile.SmoothParam[num6].TrackerName;
				ParetoChart[num6].SelectedIndex = inspection.paramFile.SmoothParam[num6].ParetoIndex;
				if (inspection.paramFile.SmoothParam[num6].ValueChangeFlag)
				{
					lab[num6].Background = Brushes.Lime;
				}
				cbl[num6].Visibility = Visibility.Visible;
				cbl[num6].IsChecked = inspection.paramFile.SmoothParam[num6].Locked;
			}
			break;
		}
		case 8:
		{
			for (int num4 = 0; num4 < 20; num4++)
			{
				cb[num4].IsChecked = inspection.paramFile.BlockAvgParam[num4].TrackerON;
				cbe[num4].SelectedIndex = (int)inspection.paramFile.BlockAvgParam[num4].ErrorBin;
				txt[num4].Text = inspection.paramFile.BlockAvgParam[num4].TrackerName;
				ParetoChart[num4].SelectedIndex = inspection.paramFile.BlockAvgParam[num4].ParetoIndex;
				if (inspection.paramFile.BlockAvgParam[num4].ValueChangeFlag)
				{
					lab[num4].Background = Brushes.Lime;
				}
				cbl[num4].Visibility = Visibility.Visible;
				cbl[num4].IsChecked = inspection.paramFile.BlockAvgParam[num4].Locked;
			}
			break;
		}
		case 9:
		{
			for (int num2 = 0; num2 < 20; num2++)
			{
				cb[num2].IsChecked = inspection.paramFile.DifferenceParam[num2].TrackerON;
				cbe[num2].SelectedIndex = (int)inspection.paramFile.DifferenceParam[num2].ErrorBin;
				txt[num2].Text = inspection.paramFile.DifferenceParam[num2].TrackerName;
				ParetoChart[num2].SelectedIndex = inspection.paramFile.DifferenceParam[num2].ParetoIndex;
				if (inspection.paramFile.DifferenceParam[num2].ValueChangeFlag)
				{
					lab[num2].Background = Brushes.Lime;
				}
				cbl[num2].Visibility = Visibility.Visible;
				cbl[num2].IsChecked = inspection.paramFile.DifferenceParam[num2].Locked;
			}
			break;
		}
		case 10:
		{
			for (int n = 0; n < 20; n++)
			{
				cb[n].IsChecked = inspection.paramFile.LineAnalysisParam[n].TrackerON;
				cbe[n].SelectedIndex = (int)inspection.paramFile.LineAnalysisParam[n].ErrorBin;
				txt[n].Text = inspection.paramFile.LineAnalysisParam[n].TrackerName;
				ParetoChart[n].SelectedIndex = inspection.paramFile.LineAnalysisParam[n].ParetoIndex;
				if (inspection.paramFile.LineAnalysisParam[n].ValueChangeFlag)
				{
					lab[n].Background = Brushes.Lime;
				}
				cbl[n].IsChecked = inspection.paramFile.LineAnalysisParam[n].Locked;
			}
			break;
		}
		case 11:
		{
			for (int l = 0; l < 20; l++)
			{
				cb[l].IsChecked = inspection.paramFile.MatchParam[l].TrackerON;
				cbe[l].SelectedIndex = (int)inspection.paramFile.MatchParam[l].ErrorBin;
				txt[l].Text = inspection.paramFile.MatchParam[l].TrackerName;
				ParetoChart[l].SelectedIndex = inspection.paramFile.MatchParam[l].ParetoIndex;
				if (inspection.paramFile.MatchParam[l].ValueChangeFlag)
				{
					lab[l].Background = Brushes.Lime;
				}
				cbl[l].IsChecked = inspection.paramFile.MatchParam[l].Locked;
			}
			break;
		}
		}
	}

	private void Btn_QuickSetupSave_Click(object sender, RoutedEventArgs e)
	{
		if (!Inited || cb_ToolSelect.SelectedIndex < 0)
		{
			return;
		}
		switch (cb_ToolSelect.SelectedIndex)
		{
		case 0:
		{
			for (int num2 = 2; num2 < 20; num2++)
			{
				if (inspection.paramFile.VSLocateParam[num2].TrackerON != cb[num2].IsChecked || inspection.paramFile.VSLocateParam[num2].ErrorBin != (FLOW_BIN)cbe[num2].SelectedIndex || inspection.paramFile.VSLocateParam[num2].ParetoIndex != ParetoChart[num2].SelectedIndex || inspection.paramFile.VSLocateParam[num2].TrackerName != txt[num2].Text || inspection.paramFile.VSLocateParam[num2].Locked != cbl[num2].IsChecked.Value)
				{
					inspection.paramFile.VSLocateParam[num2].TrackerON = cb[num2].IsChecked.Value;
					inspection.paramFile.VSLocateParam[num2].ErrorBin = (FLOW_BIN)cbe[num2].SelectedIndex;
					inspection.paramFile.VSLocateParam[num2].TrackerName = txt[num2].Text;
					inspection.paramFile.VSLocateParam[num2].ValueChangeFlag = true;
					inspection.paramFile.VSLocateParam[num2].ParetoIndex = ParetoChart[num2].SelectedIndex;
					inspection.paramFile.VSLocateParam[num2].Locked = cbl[num2].IsChecked.Value;
				}
			}
			break;
		}
		case 1:
		{
			for (int num6 = 2; num6 < 20; num6++)
			{
				if (inspection.paramFile.HSLocateParam[num6].TrackerON != cb[num6].IsChecked || inspection.paramFile.HSLocateParam[num6].ErrorBin != (FLOW_BIN)cbe[num6].SelectedIndex || inspection.paramFile.HSLocateParam[num6].ParetoIndex != ParetoChart[num6].SelectedIndex || inspection.paramFile.HSLocateParam[num6].TrackerName != txt[num6].Text || inspection.paramFile.HSLocateParam[num6].Locked != cbl[num6].IsChecked.Value)
				{
					inspection.paramFile.HSLocateParam[num6].TrackerON = cb[num6].IsChecked.Value;
					inspection.paramFile.HSLocateParam[num6].ErrorBin = (FLOW_BIN)cbe[num6].SelectedIndex;
					inspection.paramFile.HSLocateParam[num6].TrackerName = txt[num6].Text;
					inspection.paramFile.HSLocateParam[num6].ValueChangeFlag = true;
					inspection.paramFile.HSLocateParam[num6].ParetoIndex = ParetoChart[num6].SelectedIndex;
					inspection.paramFile.HSLocateParam[num6].Locked = cbl[num6].IsChecked.Value;
				}
			}
			break;
		}
		case 2:
		{
			for (int l = 1; l < 20; l++)
			{
				if (inspection.paramFile.AdvLocateParam[l].TrackerON != cb[l].IsChecked || inspection.paramFile.AdvLocateParam[l].ErrorBin != (FLOW_BIN)cbe[l].SelectedIndex || inspection.paramFile.AdvLocateParam[l].ParetoIndex != ParetoChart[l].SelectedIndex || inspection.paramFile.AdvLocateParam[l].TrackerName != txt[l].Text || inspection.paramFile.AdvLocateParam[l].Locked != cbl[l].IsChecked.Value)
				{
					inspection.paramFile.AdvLocateParam[l].TrackerON = cb[l].IsChecked.Value;
					inspection.paramFile.AdvLocateParam[l].ErrorBin = (FLOW_BIN)cbe[l].SelectedIndex;
					inspection.paramFile.AdvLocateParam[l].TrackerName = txt[l].Text;
					inspection.paramFile.AdvLocateParam[l].ValueChangeFlag = true;
					inspection.paramFile.AdvLocateParam[l].ParetoIndex = ParetoChart[l].SelectedIndex;
					inspection.paramFile.AdvLocateParam[l].Locked = cbl[l].IsChecked.Value;
				}
			}
			break;
		}
		case 3:
		{
			for (int num4 = 0; num4 < 30; num4++)
			{
				if (inspection.paramFile.AdvLocateMParam[num4].TrackerON != cb[num4].IsChecked || inspection.paramFile.AdvLocateMParam[num4].ErrorBin != (FLOW_BIN)cbe[num4].SelectedIndex || inspection.paramFile.AdvLocateMParam[num4].ParetoIndex != ParetoChart[num4].SelectedIndex || inspection.paramFile.AdvLocateMParam[num4].TrackerName != txt[num4].Text || inspection.paramFile.AdvLocateMParam[num4].Locked != cbl[num4].IsChecked.Value)
				{
					inspection.paramFile.AdvLocateMParam[num4].TrackerON = cb[num4].IsChecked.Value;
					inspection.paramFile.AdvLocateMParam[num4].ErrorBin = (FLOW_BIN)cbe[num4].SelectedIndex;
					inspection.paramFile.AdvLocateMParam[num4].TrackerName = txt[num4].Text;
					inspection.paramFile.AdvLocateMParam[num4].ValueChangeFlag = true;
					inspection.paramFile.AdvLocateMParam[num4].ParetoIndex = ParetoChart[num4].SelectedIndex;
					inspection.paramFile.AdvLocateMParam[num4].Locked = cbl[num4].IsChecked.Value;
				}
			}
			break;
		}
		case 4:
		{
			for (int n = 0; n < 20; n++)
			{
				if (inspection.paramFile.MeaWidthParam[n].TrackerON != cb[n].IsChecked || inspection.paramFile.MeaWidthParam[n].ErrorBin != (FLOW_BIN)cbe[n].SelectedIndex || inspection.paramFile.MeaWidthParam[n].ParetoIndex != ParetoChart[n].SelectedIndex || inspection.paramFile.MeaWidthParam[n].TrackerName != txt[n].Text || inspection.paramFile.MeaWidthParam[n].Locked != cbl[n].IsChecked.Value)
				{
					inspection.paramFile.MeaWidthParam[n].TrackerON = cb[n].IsChecked.Value;
					inspection.paramFile.MeaWidthParam[n].ErrorBin = (FLOW_BIN)cbe[n].SelectedIndex;
					inspection.paramFile.MeaWidthParam[n].TrackerName = txt[n].Text;
					inspection.paramFile.MeaWidthParam[n].ValueChangeFlag = true;
					inspection.paramFile.MeaWidthParam[n].ParetoIndex = ParetoChart[n].SelectedIndex;
					inspection.paramFile.MeaWidthParam[n].Locked = cbl[n].IsChecked.Value;
				}
			}
			break;
		}
		case 5:
		{
			for (int j = 0; j < 20; j++)
			{
				if (inspection.paramFile.MeaHeightParam[j].TrackerON != cb[j].IsChecked || inspection.paramFile.MeaHeightParam[j].ErrorBin != (FLOW_BIN)cbe[j].SelectedIndex || inspection.paramFile.MeaHeightParam[j].ParetoIndex != ParetoChart[j].SelectedIndex || inspection.paramFile.MeaHeightParam[j].TrackerName != txt[j].Text || inspection.paramFile.MeaHeightParam[j].Locked != cbl[j].IsChecked.Value)
				{
					inspection.paramFile.MeaHeightParam[j].TrackerON = cb[j].IsChecked.Value;
					inspection.paramFile.MeaHeightParam[j].ErrorBin = (FLOW_BIN)cbe[j].SelectedIndex;
					inspection.paramFile.MeaHeightParam[j].TrackerName = txt[j].Text;
					inspection.paramFile.MeaHeightParam[j].ValueChangeFlag = true;
					inspection.paramFile.MeaHeightParam[j].ParetoIndex = ParetoChart[j].SelectedIndex;
					inspection.paramFile.MeaHeightParam[j].Locked = cbl[j].IsChecked.Value;
				}
			}
			break;
		}
		case 6:
		{
			for (int num5 = 0; num5 < 30; num5++)
			{
				if (inspection.paramFile.BinaryParam[num5].TrackerON != cb[num5].IsChecked || inspection.paramFile.BinaryParam[num5].ErrorBin != (FLOW_BIN)cbe[num5].SelectedIndex || inspection.paramFile.BinaryParam[num5].ParetoIndex != ParetoChart[num5].SelectedIndex || inspection.paramFile.BinaryParam[num5].TrackerName != txt[num5].Text || inspection.paramFile.BinaryParam[num5].Locked != cbl[num5].IsChecked.Value)
				{
					inspection.paramFile.BinaryParam[num5].TrackerON = cb[num5].IsChecked.Value;
					inspection.paramFile.BinaryParam[num5].ErrorBin = (FLOW_BIN)cbe[num5].SelectedIndex;
					inspection.paramFile.BinaryParam[num5].TrackerName = txt[num5].Text;
					inspection.paramFile.BinaryParam[num5].ValueChangeFlag = true;
					inspection.paramFile.BinaryParam[num5].ParetoIndex = ParetoChart[num5].SelectedIndex;
					inspection.paramFile.BinaryParam[num5].Locked = cbl[num5].IsChecked.Value;
				}
			}
			break;
		}
		case 7:
		{
			for (int num3 = 0; num3 < 20; num3++)
			{
				if (inspection.paramFile.SmoothParam[num3].TrackerON != cb[num3].IsChecked || inspection.paramFile.SmoothParam[num3].ErrorBin != (FLOW_BIN)cbe[num3].SelectedIndex || inspection.paramFile.SmoothParam[num3].ParetoIndex != ParetoChart[num3].SelectedIndex || inspection.paramFile.SmoothParam[num3].TrackerName != txt[num3].Text || inspection.paramFile.SmoothParam[num3].Locked != cbl[num3].IsChecked.Value)
				{
					inspection.paramFile.SmoothParam[num3].TrackerON = cb[num3].IsChecked.Value;
					inspection.paramFile.SmoothParam[num3].ErrorBin = (FLOW_BIN)cbe[num3].SelectedIndex;
					inspection.paramFile.SmoothParam[num3].TrackerName = txt[num3].Text;
					inspection.paramFile.SmoothParam[num3].ValueChangeFlag = true;
					inspection.paramFile.SmoothParam[num3].ParetoIndex = ParetoChart[num3].SelectedIndex;
					inspection.paramFile.SmoothParam[num3].Locked = cbl[num3].IsChecked.Value;
				}
			}
			break;
		}
		case 8:
		{
			for (int num = 0; num < 20; num++)
			{
				if (inspection.paramFile.BlockAvgParam[num].TrackerON != cb[num].IsChecked || inspection.paramFile.BlockAvgParam[num].ErrorBin != (FLOW_BIN)cbe[num].SelectedIndex || inspection.paramFile.BlockAvgParam[num].ParetoIndex != ParetoChart[num].SelectedIndex || inspection.paramFile.BlockAvgParam[num].TrackerName != txt[num].Text || inspection.paramFile.BlockAvgParam[num].Locked != cbl[num].IsChecked.Value)
				{
					inspection.paramFile.BlockAvgParam[num].TrackerON = cb[num].IsChecked.Value;
					inspection.paramFile.BlockAvgParam[num].ErrorBin = (FLOW_BIN)cbe[num].SelectedIndex;
					inspection.paramFile.BlockAvgParam[num].TrackerName = txt[num].Text;
					inspection.paramFile.BlockAvgParam[num].ValueChangeFlag = true;
					inspection.paramFile.BlockAvgParam[num].ParetoIndex = ParetoChart[num].SelectedIndex;
					inspection.paramFile.BlockAvgParam[num].Locked = cbl[num].IsChecked.Value;
				}
			}
			break;
		}
		case 9:
		{
			for (int m = 0; m < 20; m++)
			{
				if (inspection.paramFile.DifferenceParam[m].TrackerON != cb[m].IsChecked || inspection.paramFile.DifferenceParam[m].ErrorBin != (FLOW_BIN)cbe[m].SelectedIndex || inspection.paramFile.DifferenceParam[m].ParetoIndex != ParetoChart[m].SelectedIndex || inspection.paramFile.DifferenceParam[m].TrackerName != txt[m].Text || inspection.paramFile.DifferenceParam[m].Locked != cbl[m].IsChecked.Value)
				{
					inspection.paramFile.DifferenceParam[m].TrackerON = cb[m].IsChecked.Value;
					inspection.paramFile.DifferenceParam[m].ErrorBin = (FLOW_BIN)cbe[m].SelectedIndex;
					inspection.paramFile.DifferenceParam[m].TrackerName = txt[m].Text;
					inspection.paramFile.DifferenceParam[m].ValueChangeFlag = true;
					inspection.paramFile.DifferenceParam[m].ParetoIndex = ParetoChart[m].SelectedIndex;
					inspection.paramFile.DifferenceParam[m].Locked = cbl[m].IsChecked.Value;
				}
			}
			break;
		}
		case 10:
		{
			for (int k = 0; k < 20; k++)
			{
				if (inspection.paramFile.LineAnalysisParam[k].TrackerON != cb[k].IsChecked || inspection.paramFile.LineAnalysisParam[k].ErrorBin != (FLOW_BIN)cbe[k].SelectedIndex || inspection.paramFile.LineAnalysisParam[k].ParetoIndex != ParetoChart[k].SelectedIndex || inspection.paramFile.LineAnalysisParam[k].TrackerName != txt[k].Text || inspection.paramFile.LineAnalysisParam[k].Locked != cbl[k].IsChecked.Value)
				{
					inspection.paramFile.LineAnalysisParam[k].TrackerON = cb[k].IsChecked.Value;
					inspection.paramFile.LineAnalysisParam[k].ErrorBin = (FLOW_BIN)cbe[k].SelectedIndex;
					inspection.paramFile.LineAnalysisParam[k].TrackerName = txt[k].Text;
					inspection.paramFile.LineAnalysisParam[k].ValueChangeFlag = true;
					inspection.paramFile.LineAnalysisParam[k].ParetoIndex = ParetoChart[k].SelectedIndex;
					inspection.paramFile.LineAnalysisParam[k].Locked = cbl[k].IsChecked.Value;
				}
			}
			break;
		}
		case 11:
		{
			for (int i = 0; i < 20; i++)
			{
				if (inspection.paramFile.MatchParam[i].TrackerON != cb[i].IsChecked || inspection.paramFile.MatchParam[i].ErrorBin != (FLOW_BIN)cbe[i].SelectedIndex || inspection.paramFile.MatchParam[i].ParetoIndex != ParetoChart[i].SelectedIndex || inspection.paramFile.MatchParam[i].TrackerName != txt[i].Text || inspection.paramFile.MatchParam[i].Locked != cbl[i].IsChecked.Value)
				{
					inspection.paramFile.MatchParam[i].TrackerON = cb[i].IsChecked.Value;
					inspection.paramFile.MatchParam[i].ErrorBin = (FLOW_BIN)cbe[i].SelectedIndex;
					inspection.paramFile.MatchParam[i].TrackerName = txt[i].Text;
					inspection.paramFile.MatchParam[i].ValueChangeFlag = true;
					inspection.paramFile.MatchParam[i].ParetoIndex = ParetoChart[i].SelectedIndex;
					inspection.paramFile.MatchParam[i].Locked = cbl[i].IsChecked.Value;
				}
			}
			break;
		}
		}
	}

	private void Cb_SelectAll_Click(object sender, RoutedEventArgs e)
	{
		if (cb_ToolSelect.SelectedIndex == 3 || cb_ToolSelect.SelectedIndex == 6)
		{
			for (int i = 0; i < 30; i++)
			{
				cb[i].IsChecked = true;
			}
		}
		else
		{
			for (int j = 0; j < 20; j++)
			{
				cb[j].IsChecked = true;
			}
		}
		if (cb_SelectAll.IsChecked == true)
		{
			cb_UnSelectAll.IsChecked = false;
		}
	}

	private void Cb_UnSelectAll_Click(object sender, RoutedEventArgs e)
	{
		if (cb_ToolSelect.SelectedIndex == 3 || cb_ToolSelect.SelectedIndex == 6)
		{
			for (int i = 0; i < 30; i++)
			{
				cb[i].IsChecked = false;
			}
		}
		else
		{
			for (int j = 0; j < 20; j++)
			{
				cb[j].IsChecked = false;
			}
		}
		if (cb_UnSelectAll.IsChecked == true)
		{
			cb_SelectAll.IsChecked = false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ChannelPreview1_Click(object sender, RoutedEventArgs e)
	{
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.Title = "請輸入密碼";
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr == "7678")
		{
			for (int i = 0; i < 30; i++)
			{
				cbl[i].Visibility = Visibility.Visible;
			}
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
			Uri resourceLocator = new Uri("/InspTools;component/other/quicksetupwindow.xaml", UriKind.Relative);
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
			btn_QuickSetupSave = (Button)target;
			btn_QuickSetupSave.Click += Btn_QuickSetupSave_Click;
			break;
		case 2:
			cb_ToolSelect = (ComboBox)target;
			cb_ToolSelect.SelectionChanged += Cb_ToolSelect_SelectionChanged;
			break;
		case 3:
			cb_SelectAll = (CheckBox)target;
			cb_SelectAll.Click += Cb_SelectAll_Click;
			break;
		case 4:
			cb_UnSelectAll = (CheckBox)target;
			cb_UnSelectAll.Click += Cb_UnSelectAll_Click;
			break;
		case 5:
			btn_ChannelPreview1 = (Button)target;
			btn_ChannelPreview1.Click += Btn_ChannelPreview1_Click;
			break;
		case 6:
			btn_QuickSetupSave_Copy = (Button)target;
			btn_QuickSetupSave_Copy.Click += Btn_QuickSetupSave_Click;
			break;
		case 7:
			((Button)target).Click += Cb_UnSelectAll_Click;
			break;
		case 8:
			lab_01 = (Label)target;
			break;
		case 9:
			cb01 = (CheckBox)target;
			break;
		case 10:
			txt01 = (TextBox)target;
			break;
		case 11:
			cbe01 = (ComboBox)target;
			break;
		case 12:
			lab_Pareto = (Label)target;
			break;
		case 13:
			cb_PC01 = (ComboBox)target;
			break;
		case 14:
			cbl01 = (CheckBox)target;
			break;
		case 15:
			lab_02 = (Label)target;
			break;
		case 16:
			cb02 = (CheckBox)target;
			break;
		case 17:
			txt02 = (TextBox)target;
			break;
		case 18:
			cbe02 = (ComboBox)target;
			break;
		case 19:
			lab_Pareto_Copy = (Label)target;
			break;
		case 20:
			cb_PC02 = (ComboBox)target;
			break;
		case 21:
			cbl02 = (CheckBox)target;
			break;
		case 22:
			lab_03 = (Label)target;
			break;
		case 23:
			cb03 = (CheckBox)target;
			break;
		case 24:
			txt03 = (TextBox)target;
			break;
		case 25:
			cbe03 = (ComboBox)target;
			break;
		case 26:
			lab_Pareto_Copy1 = (Label)target;
			break;
		case 27:
			cb_PC03 = (ComboBox)target;
			break;
		case 28:
			cbl03 = (CheckBox)target;
			break;
		case 29:
			lab_04 = (Label)target;
			break;
		case 30:
			cb04 = (CheckBox)target;
			break;
		case 31:
			txt04 = (TextBox)target;
			break;
		case 32:
			cbe04 = (ComboBox)target;
			break;
		case 33:
			lab_Pareto_Copy2 = (Label)target;
			break;
		case 34:
			cb_PC04 = (ComboBox)target;
			break;
		case 35:
			cbl04 = (CheckBox)target;
			break;
		case 36:
			lab_05 = (Label)target;
			break;
		case 37:
			cb05 = (CheckBox)target;
			break;
		case 38:
			txt05 = (TextBox)target;
			break;
		case 39:
			cbe05 = (ComboBox)target;
			break;
		case 40:
			lab_Pareto_Copy3 = (Label)target;
			break;
		case 41:
			cb_PC05 = (ComboBox)target;
			break;
		case 42:
			cbl05 = (CheckBox)target;
			break;
		case 43:
			lab_06 = (Label)target;
			break;
		case 44:
			cb06 = (CheckBox)target;
			break;
		case 45:
			txt06 = (TextBox)target;
			break;
		case 46:
			cbe06 = (ComboBox)target;
			break;
		case 47:
			lab_Pareto_Copy4 = (Label)target;
			break;
		case 48:
			cb_PC06 = (ComboBox)target;
			break;
		case 49:
			cbl06 = (CheckBox)target;
			break;
		case 50:
			lab_07 = (Label)target;
			break;
		case 51:
			cb07 = (CheckBox)target;
			break;
		case 52:
			txt07 = (TextBox)target;
			break;
		case 53:
			cbe07 = (ComboBox)target;
			break;
		case 54:
			lab_Pareto_Copy5 = (Label)target;
			break;
		case 55:
			cb_PC07 = (ComboBox)target;
			break;
		case 56:
			cbl07 = (CheckBox)target;
			break;
		case 57:
			lab_08 = (Label)target;
			break;
		case 58:
			cb08 = (CheckBox)target;
			break;
		case 59:
			txt08 = (TextBox)target;
			break;
		case 60:
			cbe08 = (ComboBox)target;
			break;
		case 61:
			lab_Pareto_Copy6 = (Label)target;
			break;
		case 62:
			cb_PC08 = (ComboBox)target;
			break;
		case 63:
			cbl08 = (CheckBox)target;
			break;
		case 64:
			lab_09 = (Label)target;
			break;
		case 65:
			cb09 = (CheckBox)target;
			break;
		case 66:
			txt09 = (TextBox)target;
			break;
		case 67:
			cbe09 = (ComboBox)target;
			break;
		case 68:
			lab_Pareto_Copy7 = (Label)target;
			break;
		case 69:
			cb_PC09 = (ComboBox)target;
			break;
		case 70:
			cbl09 = (CheckBox)target;
			break;
		case 71:
			lab_10 = (Label)target;
			break;
		case 72:
			cb10 = (CheckBox)target;
			break;
		case 73:
			txt10 = (TextBox)target;
			break;
		case 74:
			cbe10 = (ComboBox)target;
			break;
		case 75:
			lab_Pareto_Copy8 = (Label)target;
			break;
		case 76:
			cb_PC10 = (ComboBox)target;
			break;
		case 77:
			cbl10 = (CheckBox)target;
			break;
		case 78:
			lab_11 = (Label)target;
			break;
		case 79:
			cb11 = (CheckBox)target;
			break;
		case 80:
			txt11 = (TextBox)target;
			break;
		case 81:
			cbe11 = (ComboBox)target;
			break;
		case 82:
			lab_Pareto_Copy9 = (Label)target;
			break;
		case 83:
			cb_PC11 = (ComboBox)target;
			break;
		case 84:
			cbl11 = (CheckBox)target;
			break;
		case 85:
			lab_12 = (Label)target;
			break;
		case 86:
			cb12 = (CheckBox)target;
			break;
		case 87:
			txt12 = (TextBox)target;
			break;
		case 88:
			cbe12 = (ComboBox)target;
			break;
		case 89:
			lab_Pareto_Copy10 = (Label)target;
			break;
		case 90:
			cb_PC12 = (ComboBox)target;
			break;
		case 91:
			cbl12 = (CheckBox)target;
			break;
		case 92:
			lab_13 = (Label)target;
			break;
		case 93:
			cb13 = (CheckBox)target;
			break;
		case 94:
			txt13 = (TextBox)target;
			break;
		case 95:
			cbe13 = (ComboBox)target;
			break;
		case 96:
			lab_Pareto_Copy11 = (Label)target;
			break;
		case 97:
			cb_PC13 = (ComboBox)target;
			break;
		case 98:
			cbl13 = (CheckBox)target;
			break;
		case 99:
			lab_14 = (Label)target;
			break;
		case 100:
			cb14 = (CheckBox)target;
			break;
		case 101:
			txt14 = (TextBox)target;
			break;
		case 102:
			cbe14 = (ComboBox)target;
			break;
		case 103:
			lab_Pareto_Copy12 = (Label)target;
			break;
		case 104:
			cb_PC14 = (ComboBox)target;
			break;
		case 105:
			cbl14 = (CheckBox)target;
			break;
		case 106:
			lab_15 = (Label)target;
			break;
		case 107:
			cb15 = (CheckBox)target;
			break;
		case 108:
			txt15 = (TextBox)target;
			break;
		case 109:
			cbe15 = (ComboBox)target;
			break;
		case 110:
			lab_Pareto_Copy13 = (Label)target;
			break;
		case 111:
			cb_PC15 = (ComboBox)target;
			break;
		case 112:
			cbl15 = (CheckBox)target;
			break;
		case 113:
			lab_16 = (Label)target;
			break;
		case 114:
			cb16 = (CheckBox)target;
			break;
		case 115:
			txt16 = (TextBox)target;
			break;
		case 116:
			cbe16 = (ComboBox)target;
			break;
		case 117:
			lab_Pareto_Copy14 = (Label)target;
			break;
		case 118:
			cb_PC16 = (ComboBox)target;
			break;
		case 119:
			cbl16 = (CheckBox)target;
			break;
		case 120:
			lab_17 = (Label)target;
			break;
		case 121:
			cb17 = (CheckBox)target;
			break;
		case 122:
			txt17 = (TextBox)target;
			break;
		case 123:
			cbe17 = (ComboBox)target;
			break;
		case 124:
			lab_Pareto_Copy15 = (Label)target;
			break;
		case 125:
			cb_PC17 = (ComboBox)target;
			break;
		case 126:
			cbl17 = (CheckBox)target;
			break;
		case 127:
			lab_18 = (Label)target;
			break;
		case 128:
			cb18 = (CheckBox)target;
			break;
		case 129:
			txt18 = (TextBox)target;
			break;
		case 130:
			cbe18 = (ComboBox)target;
			break;
		case 131:
			lab_Pareto_Copy16 = (Label)target;
			break;
		case 132:
			cb_PC18 = (ComboBox)target;
			break;
		case 133:
			cbl18 = (CheckBox)target;
			break;
		case 134:
			lab_19 = (Label)target;
			break;
		case 135:
			cb19 = (CheckBox)target;
			break;
		case 136:
			txt19 = (TextBox)target;
			break;
		case 137:
			cbe19 = (ComboBox)target;
			break;
		case 138:
			lab_Pareto_Copy17 = (Label)target;
			break;
		case 139:
			cb_PC19 = (ComboBox)target;
			break;
		case 140:
			cbl19 = (CheckBox)target;
			break;
		case 141:
			lab_20 = (Label)target;
			break;
		case 142:
			cb20 = (CheckBox)target;
			break;
		case 143:
			txt20 = (TextBox)target;
			break;
		case 144:
			cbe20 = (ComboBox)target;
			break;
		case 145:
			lab_Pareto_Copy18 = (Label)target;
			break;
		case 146:
			cb_PC20 = (ComboBox)target;
			break;
		case 147:
			cbl20 = (CheckBox)target;
			break;
		case 148:
			lab_21 = (Label)target;
			break;
		case 149:
			cb21 = (CheckBox)target;
			break;
		case 150:
			txt21 = (TextBox)target;
			break;
		case 151:
			cbe21 = (ComboBox)target;
			break;
		case 152:
			lab_Pareto_Copy19 = (Label)target;
			break;
		case 153:
			cb_PC21 = (ComboBox)target;
			break;
		case 154:
			cbl21 = (CheckBox)target;
			break;
		case 155:
			lab_22 = (Label)target;
			break;
		case 156:
			cb22 = (CheckBox)target;
			break;
		case 157:
			txt22 = (TextBox)target;
			break;
		case 158:
			cbe22 = (ComboBox)target;
			break;
		case 159:
			lab_Pareto_Copy20 = (Label)target;
			break;
		case 160:
			cb_PC22 = (ComboBox)target;
			break;
		case 161:
			cbl22 = (CheckBox)target;
			break;
		case 162:
			lab_23 = (Label)target;
			break;
		case 163:
			cb23 = (CheckBox)target;
			break;
		case 164:
			txt23 = (TextBox)target;
			break;
		case 165:
			cbe23 = (ComboBox)target;
			break;
		case 166:
			lab_Pareto_Copy21 = (Label)target;
			break;
		case 167:
			cb_PC23 = (ComboBox)target;
			break;
		case 168:
			cbl23 = (CheckBox)target;
			break;
		case 169:
			lab_24 = (Label)target;
			break;
		case 170:
			cb24 = (CheckBox)target;
			break;
		case 171:
			txt24 = (TextBox)target;
			break;
		case 172:
			cbe24 = (ComboBox)target;
			break;
		case 173:
			lab_Pareto_Copy22 = (Label)target;
			break;
		case 174:
			cb_PC24 = (ComboBox)target;
			break;
		case 175:
			cbl24 = (CheckBox)target;
			break;
		case 176:
			lab_25 = (Label)target;
			break;
		case 177:
			cb25 = (CheckBox)target;
			break;
		case 178:
			txt25 = (TextBox)target;
			break;
		case 179:
			cbe25 = (ComboBox)target;
			break;
		case 180:
			lab_Pareto_Copy23 = (Label)target;
			break;
		case 181:
			cb_PC25 = (ComboBox)target;
			break;
		case 182:
			cbl25 = (CheckBox)target;
			break;
		case 183:
			lab_26 = (Label)target;
			break;
		case 184:
			cb26 = (CheckBox)target;
			break;
		case 185:
			txt26 = (TextBox)target;
			break;
		case 186:
			cbe26 = (ComboBox)target;
			break;
		case 187:
			lab_Pareto_Copy24 = (Label)target;
			break;
		case 188:
			cb_PC26 = (ComboBox)target;
			break;
		case 189:
			cbl26 = (CheckBox)target;
			break;
		case 190:
			lab_27 = (Label)target;
			break;
		case 191:
			cb27 = (CheckBox)target;
			break;
		case 192:
			txt27 = (TextBox)target;
			break;
		case 193:
			cbe27 = (ComboBox)target;
			break;
		case 194:
			lab_Pareto_Copy25 = (Label)target;
			break;
		case 195:
			cb_PC27 = (ComboBox)target;
			break;
		case 196:
			cbl27 = (CheckBox)target;
			break;
		case 197:
			lab_28 = (Label)target;
			break;
		case 198:
			cb28 = (CheckBox)target;
			break;
		case 199:
			txt28 = (TextBox)target;
			break;
		case 200:
			cbe28 = (ComboBox)target;
			break;
		case 201:
			lab_Pareto_Copy26 = (Label)target;
			break;
		case 202:
			cb_PC28 = (ComboBox)target;
			break;
		case 203:
			cbl28 = (CheckBox)target;
			break;
		case 204:
			lab_29 = (Label)target;
			break;
		case 205:
			cb29 = (CheckBox)target;
			break;
		case 206:
			txt29 = (TextBox)target;
			break;
		case 207:
			cbe29 = (ComboBox)target;
			break;
		case 208:
			lab_Pareto_Copy27 = (Label)target;
			break;
		case 209:
			cb_PC29 = (ComboBox)target;
			break;
		case 210:
			cbl29 = (CheckBox)target;
			break;
		case 211:
			lab_30 = (Label)target;
			break;
		case 212:
			cb30 = (CheckBox)target;
			break;
		case 213:
			txt30 = (TextBox)target;
			break;
		case 214:
			cbe30 = (ComboBox)target;
			break;
		case 215:
			lab_Pareto_Copy28 = (Label)target;
			break;
		case 216:
			cb_PC30 = (ComboBox)target;
			break;
		case 217:
			cbl30 = (CheckBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
