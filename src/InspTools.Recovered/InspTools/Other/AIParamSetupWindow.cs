using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;

namespace InspTools.Other;

public class AIParamSetupWindow : Window, IComponentConnector
{
	private int LanguageType = 1;

	private CVInspection Inspection;

	private int StationIndex;

	private ThroughputParamFile Throughput;

	internal NumericUpDown nu_ImageDoubleRate;

	internal NumericUpDown nu_HorizontalFlip;

	internal NumericUpDown nu_VerticalFlip;

	internal NumericUpDown nu_Brightness;

	internal NumericUpDown nu_Contrast;

	internal NumericUpDown nu_Scale;

	internal NumericUpDown nu_Rotate;

	internal NumericUpDown nu_Shift;

	internal NumericUpDown nu_Hue;

	internal NumericUpDown nu_Sat;

	internal NumericUpDown nu_Val;

	internal NumericUpDown nu_Red;

	internal NumericUpDown nu_Green;

	internal NumericUpDown nu_Blue;

	internal ComboBox cb_BatchSize;

	internal ComboBox cb_DnetDiv;

	private bool _contentLoaded;

	public AIParamSetupWindow()
	{
		InitializeComponent();
	}

	public void SetInspection(CVInspection station, ThroughputParamFile throughput, int stationIndex)
	{
		Inspection = station;
		StationIndex = stationIndex;
		Throughput = throughput;
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
		nu_ImageDoubleRate.Value = Throughput.param.TrainingImageDoubleRate[StationIndex];
		nu_HorizontalFlip.Value = Throughput.param.Training_horizontalFlip[StationIndex];
		nu_VerticalFlip.Value = Throughput.param.Training_verticalFlip[StationIndex];
		nu_Brightness.Value = Throughput.param.Training_brightness_limit[StationIndex];
		nu_Contrast.Value = Throughput.param.Training_contrast_limit[StationIndex];
		nu_Scale.Value = Throughput.param.Training_scale_limit[StationIndex];
		nu_Rotate.Value = Throughput.param.Training_rotate_limit[StationIndex];
		nu_Shift.Value = Throughput.param.Training_shift_limit[StationIndex];
		nu_Hue.Value = Throughput.param.Training_hue_shift_limit[StationIndex];
		nu_Sat.Value = Throughput.param.Training_sat_shift_limit[StationIndex];
		nu_Val.Value = Throughput.param.Training_val_shift_limit[StationIndex];
		nu_Red.Value = Throughput.param.Training_r_shift_limit[StationIndex];
		nu_Green.Value = Throughput.param.Training_g_shift_limit[StationIndex];
		nu_Blue.Value = Throughput.param.Training_b_shift_limit[StationIndex];
		cb_BatchSize.SelectedIndex = GetComboxIndex(Throughput.param.BatchSize[StationIndex]);
		cb_DnetDiv.SelectedIndex = GetComboxIndex(Throughput.param.DnetDiv[StationIndex]);
	}

	private int GetComboxValue(int index)
	{
		return index switch
		{
			0 => 1, 
			1 => 2, 
			2 => 4, 
			3 => 8, 
			4 => 16, 
			5 => 32, 
			_ => 8, 
		};
	}

	private int GetComboxIndex(int value)
	{
		return value switch
		{
			1 => 0, 
			2 => 1, 
			4 => 2, 
			8 => 3, 
			16 => 4, 
			32 => 5, 
			_ => 3, 
		};
	}

	private void WriteUIValueToParam()
	{
		Throughput.param.TrainingImageDoubleRate[StationIndex] = nu_ImageDoubleRate.Value.Value;
		Throughput.param.Training_horizontalFlip[StationIndex] = nu_HorizontalFlip.Value.Value;
		Throughput.param.Training_verticalFlip[StationIndex] = nu_VerticalFlip.Value.Value;
		Throughput.param.Training_brightness_limit[StationIndex] = nu_Brightness.Value.Value;
		Throughput.param.Training_contrast_limit[StationIndex] = nu_Contrast.Value.Value;
		Throughput.param.Training_scale_limit[StationIndex] = nu_Scale.Value.Value;
		Throughput.param.Training_rotate_limit[StationIndex] = nu_Rotate.Value.Value;
		Throughput.param.Training_shift_limit[StationIndex] = nu_Shift.Value.Value;
		Throughput.param.Training_hue_shift_limit[StationIndex] = (int)nu_Hue.Value.Value;
		Throughput.param.Training_sat_shift_limit[StationIndex] = (int)nu_Sat.Value.Value;
		Throughput.param.Training_val_shift_limit[StationIndex] = (int)nu_Val.Value.Value;
		Throughput.param.Training_r_shift_limit[StationIndex] = (int)nu_Red.Value.Value;
		Throughput.param.Training_g_shift_limit[StationIndex] = (int)nu_Green.Value.Value;
		Throughput.param.Training_b_shift_limit[StationIndex] = (int)nu_Blue.Value.Value;
		Throughput.param.BatchSize[StationIndex] = GetComboxValue(cb_BatchSize.SelectedIndex);
		Throughput.param.DnetDiv[StationIndex] = GetComboxValue(cb_DnetDiv.SelectedIndex);
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		WriteUIValueToParam();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/aiparamsetupwindow.xaml", UriKind.Relative);
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
			((AIParamSetupWindow)target).Closing += Window_Closing;
			break;
		case 2:
			nu_ImageDoubleRate = (NumericUpDown)target;
			break;
		case 3:
			nu_HorizontalFlip = (NumericUpDown)target;
			break;
		case 4:
			nu_VerticalFlip = (NumericUpDown)target;
			break;
		case 5:
			nu_Brightness = (NumericUpDown)target;
			break;
		case 6:
			nu_Contrast = (NumericUpDown)target;
			break;
		case 7:
			nu_Scale = (NumericUpDown)target;
			break;
		case 8:
			nu_Rotate = (NumericUpDown)target;
			break;
		case 9:
			nu_Shift = (NumericUpDown)target;
			break;
		case 10:
			nu_Hue = (NumericUpDown)target;
			break;
		case 11:
			nu_Sat = (NumericUpDown)target;
			break;
		case 12:
			nu_Val = (NumericUpDown)target;
			break;
		case 13:
			nu_Red = (NumericUpDown)target;
			break;
		case 14:
			nu_Green = (NumericUpDown)target;
			break;
		case 15:
			nu_Blue = (NumericUpDown)target;
			break;
		case 16:
			cb_BatchSize = (ComboBox)target;
			break;
		case 17:
			cb_DnetDiv = (ComboBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
