using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InspTools.Other;

public class UISetupWindow : Window, IComponentConnector
{
	private ThroughputParamFile ThroughputFile;

	private bool inited;

	internal ComboBox cb_ColorSelectA;

	internal ComboBox cb_ThicknessA;

	internal Border bor_TrackerA;

	internal TextBlock txt_TrackerALabel;

	internal Rectangle rt_ColorSelectA;

	internal ComboBox cb_ColorSelectB;

	internal ComboBox cb_ThicknessB;

	internal Border bor_TrackerB;

	internal TextBlock txt_TrackerBLabel;

	internal Rectangle rt_ColorSelectB;

	private bool _contentLoaded;

	public UISetupWindow()
	{
		InitializeComponent();
		inited = true;
	}

	public void SetThroughputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
		CopyValueToUI();
	}

	private void CopyValueToUI()
	{
		cb_ColorSelectA.SelectedIndex = ThroughputFile.param.TrackerAColorIndex;
		cb_ColorSelectB.SelectedIndex = ThroughputFile.param.TrackerBColorIndex;
		SetColor(bor_TrackerA, txt_TrackerALabel, rt_ColorSelectA, ThroughputFile.param.TrackerAColorIndex);
		SetColor(bor_TrackerB, txt_TrackerBLabel, rt_ColorSelectB, ThroughputFile.param.TrackerBColorIndex);
		cb_ThicknessA.SelectedIndex = ThroughputFile.param.TrackerAThickness - 1;
		cb_ThicknessB.SelectedIndex = ThroughputFile.param.TrackerBThickness - 1;
		bor_TrackerA.BorderThickness = new Thickness(ThroughputFile.param.TrackerAThickness);
		bor_TrackerB.BorderThickness = new Thickness(ThroughputFile.param.TrackerBThickness);
	}

	private void SetColor(Border Tracker, TextBlock text, Rectangle rect, int index)
	{
		switch (index)
		{
		case 0:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Blue);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		case 1:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Cyan);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		case 2:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Magenta);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		case 3:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Lime);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		case 4:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Yellow);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		case 5:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Red);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		default:
		{
			Brush brush = (Tracker.BorderBrush = Brushes.Blue);
			Brush fill = (text.Foreground = brush);
			rect.Fill = fill;
			break;
		}
		}
	}

	private void WriteUItoParamFile()
	{
		ThroughputFile.param.TrackerAColorIndex = cb_ColorSelectA.SelectedIndex;
		ThroughputFile.param.TrackerBColorIndex = cb_ColorSelectB.SelectedIndex;
		ThroughputFile.param.TrackerAThickness = cb_ThicknessA.SelectedIndex + 1;
		ThroughputFile.param.TrackerBThickness = cb_ThicknessB.SelectedIndex + 1;
	}

	private void Cb_ColorSelectA_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			SetColor(bor_TrackerA, txt_TrackerALabel, rt_ColorSelectA, cb_ColorSelectA.SelectedIndex);
		}
	}

	private void Cb_ColorSelectB_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			SetColor(bor_TrackerB, txt_TrackerBLabel, rt_ColorSelectB, cb_ColorSelectB.SelectedIndex);
		}
	}

	private void Cb_ThicknessA_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			bor_TrackerA.BorderThickness = new Thickness(cb_ThicknessA.SelectedIndex + 1);
			bor_TrackerB.BorderThickness = new Thickness(cb_ThicknessB.SelectedIndex + 1);
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		WriteUItoParamFile();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/uisetupwindow.xaml", UriKind.Relative);
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
			((UISetupWindow)target).Closing += Window_Closing;
			break;
		case 2:
			cb_ColorSelectA = (ComboBox)target;
			cb_ColorSelectA.SelectionChanged += Cb_ColorSelectA_SelectionChanged;
			break;
		case 3:
			cb_ThicknessA = (ComboBox)target;
			cb_ThicknessA.SelectionChanged += Cb_ThicknessA_SelectionChanged;
			break;
		case 4:
			bor_TrackerA = (Border)target;
			break;
		case 5:
			txt_TrackerALabel = (TextBlock)target;
			break;
		case 6:
			rt_ColorSelectA = (Rectangle)target;
			break;
		case 7:
			cb_ColorSelectB = (ComboBox)target;
			cb_ColorSelectB.SelectionChanged += Cb_ColorSelectB_SelectionChanged;
			break;
		case 8:
			cb_ThicknessB = (ComboBox)target;
			cb_ThicknessB.SelectionChanged += Cb_ThicknessA_SelectionChanged;
			break;
		case 9:
			bor_TrackerB = (Border)target;
			break;
		case 10:
			txt_TrackerBLabel = (TextBlock)target;
			break;
		case 11:
			rt_ColorSelectB = (Rectangle)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
