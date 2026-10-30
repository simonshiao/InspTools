using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using HiroPChart;
using InspTools.AOI;

namespace InspTools.Other;

public class SaveParetoChartWindow : Window, IComponentConnector
{
	private HPChartPage HiroPChart = new HPChartPage();

	private DataTable dt = new DataTable("ParetoChartData");

	private string ImagePath = "";

	internal SaveParetoChartWindow Main;

	internal Canvas cnv_Image;

	internal Frame frame;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public SaveParetoChartWindow()
	{
		InitializeComponent();
		loadGridColums();
		Main.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void loadGridColums()
	{
		dt.Columns.Add("No", typeof(string));
		dt.Columns.Add("Names", typeof(string));
		dt.Columns.Add("Frequency");
		dt.Columns["Frequency"].DataType = Type.GetType("System.Int32");
	}

	public void SetImagePath(string path)
	{
		ImagePath = path;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetParetoChartData(List<ParetoChartData> data)
	{
		try
		{
			dt.Clear();
			dt.Rows.Clear();
			for (int i = 0; i < data.Count; i++)
			{
				DataRow dataRow = dt.NewRow();
				dataRow["No"] = i.ToString();
				dataRow["Names"] = data[i].Name;
				dataRow["Frequency"] = data[i].Value;
				dt.Rows.Add(dataRow);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("PCW04-ParetoChartWindow-Btn_ShowChartS1_Click()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SaveParetoChartImage()
	{
		frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
		frame.Content = HiroPChart;
		HiroPChart.ChartTitleStr = "Pareto Chart";
		HiroPChart.SetChartSize(frame.RenderSize.Width, frame.RenderSize.Height);
		HiroPChart.Bindgrid(dt);
		HiroPChart.SaveChartImage(ImagePath);
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		if (ImagePath == "")
		{
			Close();
			return;
		}
		SaveParetoChartImage();
		Close();
	}

	public void SaveCanvas()
	{
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)Main.Width, (int)Main.Height, 96.0, 96.0, PixelFormats.Default);
		renderTargetBitmap.Render(cnv_Image);
		BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
		bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
		using Stream stream = File.Create(ImagePath);
		bmpBitmapEncoder.Save(stream);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/saveparetochartwindow.xaml", UriKind.Relative);
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
			Main = (SaveParetoChartWindow)target;
			Main.Loaded += Window_Loaded;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			break;
		case 3:
			frame = (Frame)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
