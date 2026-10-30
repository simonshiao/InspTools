using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.Other;

public class SelectImageWindow : System.Windows.Window, IComponentConnector
{
	private string OldImgPath = "";

	private string OldDrawImgPath = "";

	private string NewImgPath = "";

	private string NewDrawImgPath = "";

	private int DeleteResult;

	internal Label lab_FileName;

	internal Canvas cnv_ImageOld;

	internal ScaleTransform sfr1;

	internal Label lab_StatusOld;

	internal Canvas cnv_ImageDrawOld;

	internal ScaleTransform sfr2;

	internal Label lab_ImageDrawOld;

	internal Button btn_DeleteOld;

	internal Label ico_ResultNGFont1;

	internal Canvas cnv_ImageNew;

	internal ScaleTransform sfr3;

	internal Label lab_StatusNew;

	internal Canvas cnv_ImageDrawNew;

	internal ScaleTransform sfr4;

	internal Label lab_ImageDrawNew;

	internal Button btn_DeleteNew;

	internal Label ico_ResultNGFont1_Copy;

	private bool _contentLoaded;

	public SelectImageWindow()
	{
		InitializeComponent();
	}

	public void SetMarkImageHide()
	{
		lab_ImageDrawOld.Visibility = Visibility.Hidden;
		cnv_ImageDrawOld.Visibility = Visibility.Hidden;
		lab_ImageDrawNew.Visibility = Visibility.Hidden;
		cnv_ImageDrawNew.Visibility = Visibility.Hidden;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetClassicMode(string a, string b)
	{
		lab_StatusOld.Visibility = Visibility.Hidden;
		lab_StatusNew.Visibility = Visibility.Hidden;
		cnv_ImageOld.Visibility = Visibility.Hidden;
		cnv_ImageNew.Visibility = Visibility.Hidden;
		lab_ImageDrawOld.Content = "分類 " + a;
		lab_ImageDrawNew.Content = "分類 " + b;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImage(string Oldpath, string OldDrawpath, bool oldstatus, string Newpath, string NewDrawpath, bool newstatus)
	{
		Mat mat = new Mat();
		Mat mat2 = new Mat();
		Mat mat3 = new Mat();
		Mat mat4 = new Mat();
		ImageSource imageSource = null;
		ImageBrush imageBrush = null;
		OldImgPath = Oldpath;
		OldDrawImgPath = OldDrawpath;
		NewImgPath = Newpath;
		NewDrawImgPath = NewDrawpath;
		if (File.Exists(OldImgPath))
		{
			mat = CVBuffer.ImRead(OldImgPath);
		}
		if (mat.Width > 0 && mat.Height > 0)
		{
			imageSource = mat.ToBitmapSource();
			imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageOld.Background = imageBrush;
		}
		if (File.Exists(OldDrawImgPath))
		{
			mat2 = CVBuffer.ImRead(OldDrawImgPath);
		}
		if (mat2.Width > 0 && mat2.Height > 0)
		{
			imageSource = mat2.ToBitmapSource();
			imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageDrawOld.Background = imageBrush;
		}
		if (File.Exists(NewImgPath))
		{
			mat3 = CVBuffer.ImRead(NewImgPath);
		}
		if (mat3.Width > 0 && mat3.Height > 0)
		{
			imageSource = mat3.ToBitmapSource();
			imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageNew.Background = imageBrush;
		}
		if (File.Exists(NewDrawImgPath))
		{
			mat4 = CVBuffer.ImRead(NewDrawImgPath);
		}
		if (mat4.Width > 0 && mat4.Height > 0)
		{
			imageSource = mat4.ToBitmapSource();
			imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageDrawNew.Background = imageBrush;
		}
		if (!oldstatus)
		{
			lab_StatusOld.Content = "NG影像";
		}
		if (!newstatus)
		{
			lab_StatusNew.Content = "NG影像";
		}
	}

	public void SetClassicImage(string OldDrawpath, string NewDrawpath)
	{
		Mat mat = new Mat();
		Mat mat2 = new Mat();
		ImageSource imageSource = null;
		ImageBrush imageBrush = null;
		OldDrawImgPath = OldDrawpath;
		NewDrawImgPath = NewDrawpath;
		if (File.Exists(OldDrawImgPath))
		{
			mat = CVBuffer.ImRead(OldDrawImgPath, ImreadModes.Grayscale);
		}
		if (mat.Width > 0 && mat.Height > 0)
		{
			imageSource = mat.ToBitmapSource();
			imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageDrawOld.Background = imageBrush;
		}
		if (File.Exists(NewDrawImgPath))
		{
			mat2 = CVBuffer.ImRead(NewDrawImgPath, ImreadModes.Grayscale);
		}
		if (mat2.Width > 0 && mat2.Height > 0)
		{
			imageSource = mat2.ToBitmapSource();
			imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageDrawNew.Background = imageBrush;
		}
	}

	public int GetDeleteResult()
	{
		return DeleteResult;
	}

	private void Btn_DeleteOld_Click(object sender, RoutedEventArgs e)
	{
		if (File.Exists(OldImgPath))
		{
			FileInfo fileInfo = new FileInfo(OldImgPath);
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
			File.Delete(OldImgPath);
		}
		if (File.Exists(OldDrawImgPath))
		{
			FileInfo fileInfo2 = new FileInfo(OldDrawImgPath);
			if (fileInfo2.IsReadOnly)
			{
				fileInfo2.IsReadOnly = false;
			}
			File.Delete(OldDrawImgPath);
		}
		DeleteResult = 1;
		Close();
	}

	private void Btn_DeleteNew_Click(object sender, RoutedEventArgs e)
	{
		if (File.Exists(NewImgPath))
		{
			FileInfo fileInfo = new FileInfo(NewImgPath);
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
			File.Delete(NewImgPath);
		}
		if (File.Exists(NewDrawImgPath))
		{
			FileInfo fileInfo2 = new FileInfo(NewDrawImgPath);
			if (fileInfo2.IsReadOnly)
			{
				fileInfo2.IsReadOnly = false;
			}
			File.Delete(NewDrawImgPath);
		}
		DeleteResult = 2;
		Close();
	}

	private void ico_ResultNGFont1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Btn_DeleteOld_Click(sender, null);
	}

	private void ico_ResultNGFont1_Copy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Btn_DeleteNew_Click(sender, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/selectimagewindow.xaml", UriKind.Relative);
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
			lab_FileName = (Label)target;
			break;
		case 2:
			cnv_ImageOld = (Canvas)target;
			break;
		case 3:
			sfr1 = (ScaleTransform)target;
			break;
		case 4:
			lab_StatusOld = (Label)target;
			break;
		case 5:
			cnv_ImageDrawOld = (Canvas)target;
			break;
		case 6:
			sfr2 = (ScaleTransform)target;
			break;
		case 7:
			lab_ImageDrawOld = (Label)target;
			break;
		case 8:
			btn_DeleteOld = (Button)target;
			btn_DeleteOld.Click += Btn_DeleteOld_Click;
			break;
		case 9:
			ico_ResultNGFont1 = (Label)target;
			ico_ResultNGFont1.MouseLeftButtonDown += ico_ResultNGFont1_MouseLeftButtonDown;
			break;
		case 10:
			cnv_ImageNew = (Canvas)target;
			break;
		case 11:
			sfr3 = (ScaleTransform)target;
			break;
		case 12:
			lab_StatusNew = (Label)target;
			break;
		case 13:
			cnv_ImageDrawNew = (Canvas)target;
			break;
		case 14:
			sfr4 = (ScaleTransform)target;
			break;
		case 15:
			lab_ImageDrawNew = (Label)target;
			break;
		case 16:
			btn_DeleteNew = (Button)target;
			btn_DeleteNew.Click += Btn_DeleteNew_Click;
			break;
		case 17:
			ico_ResultNGFont1_Copy = (Label)target;
			ico_ResultNGFont1_Copy.MouseLeftButtonDown += ico_ResultNGFont1_Copy_MouseLeftButtonDown;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
