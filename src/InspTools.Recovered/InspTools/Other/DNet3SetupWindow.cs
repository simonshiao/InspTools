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
using InspTools.Resources;
using InspTools.Station;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.Other;

public class DNet3SetupWindow : System.Windows.Window, IComponentConnector
{
	private int LanguageType = 1;

	private const int MaxAreaCount = 8;

	private bool UpdateUI;

	private int FaceIndex;

	private CVInspection inspection;

	private ComboBox[] OKArea;

	private Mat Buffer = new Mat();

	private Mat BufferDraw = new Mat();

	internal Canvas cnv_draw;

	internal ComboBox cmb_Area1;

	internal ComboBox cmb_Area2;

	internal ComboBox cmb_Area3;

	internal ComboBox cmb_Area4;

	internal ComboBox cmb_Area5;

	internal ComboBox cmb_Area6;

	internal ComboBox cmb_Area7;

	internal ComboBox cmb_Area8;

	private bool _contentLoaded;

	public DNet3SetupWindow()
	{
		InitializeComponent();
		OKArea = new ComboBox[8] { cmb_Area1, cmb_Area2, cmb_Area3, cmb_Area4, cmb_Area5, cmb_Area6, cmb_Area7, cmb_Area8 };
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(CVInspection station, int abIndex)
	{
		try
		{
			inspection = station;
			UpdateUI = false;
			for (int i = 0; i <= 20; i++)
			{
				if (i == 0)
				{
					for (int j = 0; j < 8; j++)
					{
						OKArea[j].Items.Add("-----");
					}
					continue;
				}
				if (inspection.paramFile.AdvLocateParam[i - 1].TrackerON)
				{
					for (int k = 0; k < 8; k++)
					{
						OKArea[k].Items.Add(i + " : " + inspection.paramFile.AdvLocateParam[i - 1].TrackerName);
					}
					continue;
				}
				for (int l = 0; l < 8; l++)
				{
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = i + " : " + inspection.paramFile.AdvLocateParam[i - 1].TrackerName;
					comboBoxItem.IsEnabled = false;
					OKArea[l].Items.Add(comboBoxItem);
				}
			}
			FaceIndex = abIndex;
			if (abIndex == 0)
			{
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A1 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A1].TrackerON)
					{
						OKArea[0].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A1 + 1;
					}
					else
					{
						OKArea[0].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A2 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A2].TrackerON)
					{
						OKArea[1].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A2 + 1;
					}
					else
					{
						OKArea[1].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A3 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A3].TrackerON)
					{
						OKArea[2].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A3 + 1;
					}
					else
					{
						OKArea[2].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A4 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A4].TrackerON)
					{
						OKArea[3].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A4 + 1;
					}
					else
					{
						OKArea[3].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A5 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A5].TrackerON)
					{
						OKArea[4].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A5 + 1;
					}
					else
					{
						OKArea[4].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A6 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A6].TrackerON)
					{
						OKArea[5].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A6 + 1;
					}
					else
					{
						OKArea[5].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A7 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A7].TrackerON)
					{
						OKArea[6].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A7 + 1;
					}
					else
					{
						OKArea[6].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_A8 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_A8].TrackerON)
					{
						OKArea[7].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_A8 + 1;
					}
					else
					{
						OKArea[7].SelectedIndex = 0;
					}
				}
			}
			else
			{
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B1 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B1].TrackerON)
					{
						OKArea[0].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B1 + 1;
					}
					else
					{
						OKArea[0].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B2 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B2].TrackerON)
					{
						OKArea[1].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B2 + 1;
					}
					else
					{
						OKArea[1].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B3 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B3].TrackerON)
					{
						OKArea[2].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B3 + 1;
					}
					else
					{
						OKArea[2].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B4 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B4].TrackerON)
					{
						OKArea[3].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B4 + 1;
					}
					else
					{
						OKArea[3].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B5 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B5].TrackerON)
					{
						OKArea[4].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B5 + 1;
					}
					else
					{
						OKArea[4].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B6 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B6].TrackerON)
					{
						OKArea[5].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B6 + 1;
					}
					else
					{
						OKArea[5].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B7 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B7].TrackerON)
					{
						OKArea[6].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B7 + 1;
					}
					else
					{
						OKArea[6].SelectedIndex = 0;
					}
				}
				if (inspection.paramFile.AIEnhanceParam.DnetROIIndex_B8 > 0)
				{
					if (inspection.paramFile.AdvLocateParam[inspection.paramFile.AIEnhanceParam.DnetROIIndex_B8].TrackerON)
					{
						OKArea[7].SelectedIndex = inspection.paramFile.AIEnhanceParam.DnetROIIndex_B8 + 1;
					}
					else
					{
						OKArea[7].SelectedIndex = 0;
					}
				}
			}
			UpdateUI = true;
			UpdateImage();
		}
		catch
		{
		}
	}

	public void UpdateImage()
	{
		if (!UpdateUI)
		{
			return;
		}
		Buffer.CopyTo(BufferDraw);
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		OpenCvSharp.Rect rect2 = new OpenCvSharp.Rect(0, 0, Buffer.Width, Buffer.Height);
		inspection.maskTool.GetInspeRect(0, ref rect);
		if (rect.Width <= 0 && rect.Height <= 0)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			int num = OKArea[i].SelectedIndex - 1;
			if (num < 0)
			{
				continue;
			}
			for (int j = i + 1; j < 8; j++)
			{
				if (OKArea[j].SelectedIndex - 1 == num && OKArea[j].SelectedIndex >= 0)
				{
					OKArea[j].SelectedIndex = 0;
				}
			}
			OpenCvSharp.Rect insideRect = inspection.paramFile.AdvLocateParam[num].InsideRect;
			insideRect.Left -= rect.Left;
			insideRect.Top -= rect.Top;
			insideRect &= rect2;
			BufferDraw.Rectangle(insideRect, Scalar.LimeGreen);
		}
		ImageSource imageSource = BufferDraw.ToBitmapSource();
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_draw.Background = imageBrush;
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

	public void SetBuffer(Mat mat)
	{
		mat.CopyTo(Buffer);
		mat.CopyTo(BufferDraw);
		ImageSource imageSource = mat.ToBitmapSource();
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_draw.Background = imageBrush;
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		if (FaceIndex == 0)
		{
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A1 = OKArea[0].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A2 = OKArea[1].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A3 = OKArea[2].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A4 = OKArea[3].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A5 = OKArea[4].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A6 = OKArea[5].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A7 = OKArea[6].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_A8 = OKArea[7].SelectedIndex - 1;
		}
		else
		{
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B1 = OKArea[0].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B2 = OKArea[1].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B3 = OKArea[2].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B4 = OKArea[3].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B5 = OKArea[4].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B6 = OKArea[5].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B7 = OKArea[6].SelectedIndex - 1;
			inspection.paramFile.AIEnhanceParam.DnetROIIndex_B8 = OKArea[7].SelectedIndex - 1;
		}
	}

	private void Cmb_Area1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		UpdateImage();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/dnet3setupwindow.xaml", UriKind.Relative);
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
			((DNet3SetupWindow)target).Closed += Window_Closed;
			break;
		case 2:
			cnv_draw = (Canvas)target;
			break;
		case 3:
			cmb_Area1 = (ComboBox)target;
			cmb_Area1.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 4:
			cmb_Area2 = (ComboBox)target;
			cmb_Area2.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 5:
			cmb_Area3 = (ComboBox)target;
			cmb_Area3.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 6:
			cmb_Area4 = (ComboBox)target;
			cmb_Area4.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 7:
			cmb_Area5 = (ComboBox)target;
			cmb_Area5.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 8:
			cmb_Area6 = (ComboBox)target;
			cmb_Area6.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 9:
			cmb_Area7 = (ComboBox)target;
			cmb_Area7.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		case 10:
			cmb_Area8 = (ComboBox)target;
			cmb_Area8.SelectionChanged += Cmb_Area1_SelectionChanged;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
