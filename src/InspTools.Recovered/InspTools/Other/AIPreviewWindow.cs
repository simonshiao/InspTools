using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SAMViewer;

namespace InspTools.Other;

public class AIPreviewWindow : System.Windows.Window, IComponentConnector
{
	private string ImageXNGPath = "";

	private string ImageXOKPath = "";

	private string X_NG_Path = "";

	private string Y_NG_Path = "";

	private string[] filelist;

	private CVInspection Station;

	private string NewModel = "";

	private string OldModel = "";

	private int ImageIndex;

	public AIEnhanceTool aiEnhanceTools;

	private string[] SubDir;

	private bool UpdateUI;

	private string ReDrawFile = "";

	private int StationIndex;

	private bool UseYPath;

	private string LastFile = "";

	private Mat[] AIMatBGR = new Mat[3];

	private int PreviewMode;

	private int LanguageType;

	private ThroughputParamFile ThroughputFile;

	private bool UseNGImage = true;

	private Mat AIMatBuf = new Mat();

	private Mat EnhanceMat = new Mat();

	private Mat EnhanceMatH = new Mat();

	private Mat EnhanceMatV = new Mat();

	private Mat EnhanceMatVH = new Mat();

	private Mat DrawingMat = new Mat();

	private Mat DrawingMatH = new Mat();

	private Mat DrawingMatV = new Mat();

	private Mat DrawingMatVH = new Mat();

	private Mat DestMat1 = new Mat();

	private Mat DestMat2 = new Mat();

	private Mat DestMat3 = new Mat();

	private Mat DestMat4 = new Mat();

	private SAM mSam;

	private int ImageListSource;

	private string[] buffilelist;

	private string[] dropfilelist;

	private int dropfileIndex = -1;

	private int buffileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int bufImageIndex;

	private int bufImageCount;

	private bool DragMode;

	private bool ContinueWrite;

	private bool MarkImgMode = true;

	public string[] caption = new string[6];

	private string LastPath = "";

	private bool LastSearchALL;

	internal AIPreviewWindow wnd_Main;

	internal Canvas cnv_Image;

	internal ScaleTransform sfr;

	internal Button btn_OKImage;

	internal Button btn_NGImage;

	internal Button btn_ImageNum;

	internal Button btn_PrevImage;

	internal Button btn_NextImage;

	internal Label lab_FileName;

	internal Button btn_Delete;

	internal ComboBox cb_ImageDir;

	internal Button btn_BufferImgALL;

	internal Button btn_BufferImgOK;

	internal Button btn_BufferImgNG;

	internal Button btn_BufferImgFile;

	internal Label lab_Current;

	internal ComboBox cb_PreviewMode;

	internal Button btn_MoveFolder;

	internal Button btn_FixAllIgnore;

	internal CheckBox chk_RotatePreview;

	internal ComboBox cb_ImageDirMoveTo;

	internal Label lab_GerNG;

	internal Canvas cnv_ImageAIOld;

	internal ScaleTransform sfr1;

	internal Label lab_OldModel;

	internal Button btn_ReDraw1_Copy;

	internal Button btn_ReDraw1_Copy1;

	internal Button btn_ReDraw1_Copy2;

	internal Button btn_ReDraw1_Copy3;

	internal Button btn_NewModel1;

	internal Button btn_OldModel1;

	internal Button btn_ImageNum1;

	internal Button btn_PrevImage1;

	internal Button btn_NextImage1;

	internal Canvas cnv_ImageAI;

	internal ScaleTransform sfr2;

	internal Label lab_NewModel;

	internal Canvas cnv_ImageDraw;

	internal ScaleTransform sfr3;

	internal Label lab_FileNotExist;

	internal Button btn_Redraw;

	internal Grid grd_Enhance;

	internal CheckBox chk_Enhance;

	internal CheckBox chk_EnableVMirror;

	internal CheckBox chk_EnableHMirror;

	internal Button btn_SetPara;

	internal Button btn_SetEnhancePara;

	internal NumericUpDown nu_Hue;

	internal NumericUpDown nu_Sat;

	internal NumericUpDown nu_Val;

	internal NumericUpDown nu_Red;

	internal NumericUpDown nu_Green;

	internal NumericUpDown nu_Blue;

	internal Button btn_SaveToNGDB;

	internal Button btn_LoadOldModel;

	internal Button btn_FixIgnore;

	internal ComboBox cb_MarkColorBefore;

	internal ComboBox cb_MarkColorAfter;

	internal Button btn_ChangeMarkColor;

	internal Button btn_RotateInspectAlert;

	internal Label lab_ImageClass;

	internal CheckBox chk_ShowOldModelPreview;

	internal Grid grd_RotatePreview;

	internal Canvas cnv_ImageAI2;

	internal ScaleTransform sfr4;

	internal Canvas cnv_ImageV;

	internal ScaleTransform sfr5;

	internal Canvas cnv_ImageH;

	internal ScaleTransform sfr6;

	internal Canvas cnv_ImageVH;

	internal ScaleTransform sfr7;

	internal Label ico_ResultAlartH;

	internal Label ico_ResultAlartVH;

	internal Label ico_ResultAlartV;

	internal Button btn_SaveToNGDB_H;

	internal Button btn_SaveToNGDB_VH;

	internal Button btn_SaveToNGDB_V;

	internal Button btn_ClosePreview;

	internal Label ico_ResultAlartH_Copy;

	internal Label ico_ResultAlartH_Copy1;

	internal Label ico_ResultAlartH_Copy2;

	internal Label ico_ResultAlartH_Copy3;

	internal Button btn_GerNG;

	private bool _contentLoaded;

	public AIPreviewWindow()
	{
		InitializeComponent();
		for (int i = 0; i < 3; i++)
		{
			AIMatBGR[i] = new Mat();
		}
		for (int j = 0; j < 6; j++)
		{
			caption[j] = "";
		}
		grd_Enhance.Width = 170.0;
		grd_RotatePreview.Visibility = Visibility.Hidden;
		btn_RotateInspectAlert.Visibility = Visibility.Hidden;
		cnv_ImageAIOld.Visibility = Visibility.Hidden;
		btn_FixAllIgnore.Visibility = Visibility.Hidden;
		btn_GerNG.Visibility = Visibility.Hidden;
		lab_GerNG.Visibility = Visibility.Hidden;
	}

	public void SetStation(CVInspection s, int index)
	{
		Station = s;
		StationIndex = index;
		aiEnhanceTools = new AIEnhanceTool(Station.paramFile);
		aiEnhanceTools.SetBuffers(Station.buffers);
		if (Station.paramFile.ThroughputFile.CurrentClient == 3)
		{
			chk_RotatePreview.Visibility = Visibility.Hidden;
			cb_MarkColorBefore.Visibility = Visibility.Hidden;
			btn_ChangeMarkColor.Visibility = Visibility.Hidden;
			cb_MarkColorAfter.Visibility = Visibility.Hidden;
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

	public void SetSAM(SAM sam)
	{
		mSam = sam;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImagePath(string ngpath, string okpath, bool DefalutNg = true)
	{
		UpdateUI = false;
		try
		{
			cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
			ImageIndex = 0;
			if (cb_ImageDir.Items.Count > 0)
			{
				cb_ImageDir.Items.Clear();
			}
			cb_ImageDir.Items.Add("所有影像");
			cb_ImageDir.Items.Add("本次新增，未分類影像");
			cb_ImageDir.SelectedIndex = 0;
			cb_ImageDirMoveTo.Items.Clear();
			ImageXNGPath = ngpath;
			ImageXOKPath = okpath;
			if (DefalutNg)
			{
				UpdateList(ImageXNGPath);
				SubDir = Directory.GetDirectories(ImageXNGPath);
				UseYPath = true;
			}
			else if (okpath != "")
			{
				btn_OKImage.IsEnabled = true;
				UpdateList(ImageXOKPath);
				SubDir = Directory.GetDirectories(ImageXOKPath);
				UseYPath = false;
			}
			else
			{
				btn_OKImage.IsEnabled = false;
			}
			if (SubDir == null)
			{
				UpdateUI = true;
				return;
			}
			if (SubDir.Length != 0)
			{
				for (int i = 0; i < SubDir.Length; i++)
				{
					if (Path.GetFileName(SubDir[i]) != "ALL")
					{
						cb_ImageDir.Items.Add(Path.GetFileName(SubDir[i]));
						cb_ImageDirMoveTo.Items.Add(Path.GetFileName(SubDir[i]));
					}
					else
					{
						cb_ImageDir.Items.Add("未分類");
						cb_ImageDirMoveTo.Items.Add("未分類");
					}
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreview-SetImagePath()-01-" + ex.ToString());
			return;
		}
		UpdateUI = true;
	}

	public void SetThroughputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImageSource(int imagelistsource, string[] flist, int findex, int recentindex, int okindex, int ngindex)
	{
		try
		{
			Button button = btn_BufferImgALL;
			Button button2 = btn_BufferImgOK;
			Button button3 = btn_BufferImgNG;
			bool flag = (btn_BufferImgFile.IsEnabled = true);
			bool flag3 = (button3.IsEnabled = flag);
			bool isEnabled = (button2.IsEnabled = flag3);
			button.IsEnabled = isEnabled;
			ImageListSource = imagelistsource;
			buffilelist = flist;
			buffileIndex = findex;
			RecentBufferIndex = recentindex;
			OKBufferIndex = okindex;
			NGBufferIndex = ngindex;
			if (buffilelist == null)
			{
				btn_BufferImgFile.IsEnabled = false;
			}
			else if (buffilelist.Length < 0)
			{
				btn_BufferImgFile.IsEnabled = false;
			}
			if (Station.RecentBufferList == null)
			{
				btn_BufferImgALL.IsEnabled = false;
			}
			else if (Station.RecentBufferList.Count < 0)
			{
				btn_BufferImgALL.IsEnabled = false;
			}
			if (Station.OKBufferList == null)
			{
				btn_BufferImgOK.IsEnabled = false;
			}
			else if (Station.OKBufferList.Count < 0)
			{
				btn_BufferImgOK.IsEnabled = false;
			}
			if (Station.NGBufferList == null)
			{
				btn_BufferImgNG.IsEnabled = false;
			}
			else if (Station.NGBufferList.Count < 0)
			{
				btn_BufferImgNG.IsEnabled = false;
			}
			ImageIndex = 0;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreview-SetImageSource()-01-" + ex.ToString());
		}
	}

	public void SetPreviewMode(int mode)
	{
		PreviewMode = mode;
		bool updateUI = UpdateUI;
		UpdateUI = false;
		cb_PreviewMode.SelectedIndex = PreviewMode;
		UpdateUI = updateUI;
	}

	public void SetNGMarkedFolder(string xng, string yng)
	{
		X_NG_Path = xng;
		Y_NG_Path = yng;
	}

	private static string[] GetFiles(string sourceFolder, string filters, SearchOption searchOption)
	{
		return filters.Split('|').SelectMany((string filter) => Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateList(string path, bool SearchAll = true)
	{
		try
		{
			if (!Directory.Exists(path) || path == "")
			{
				btn_ImageNum.Content = string.Format("{0} / {1}", 0, 0);
				cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
				return;
			}
			LastSearchALL = SearchAll;
			LastPath = path;
			if (SearchAll)
			{
				string[] files = Directory.GetFiles(path, "*.bmp", SearchOption.AllDirectories);
				string[] files2 = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
				filelist = files.Concat(files2).ToArray();
			}
			else
			{
				string[] files3 = Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly);
				string[] files4 = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
				filelist = files3.Concat(files4).ToArray();
			}
			SortAsFileTime(ref filelist);
			if (filelist.Length != 0)
			{
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
				Inspect(filelist[0]);
				return;
			}
			btn_ImageNum.Content = string.Format("{0} / {1}", 0, 0);
			cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
		}
		catch (Exception ex)
		{
			string text = "AIP03-AIPreviewWindow-SetImagePath()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetModel(string newmodel, string oldmodel)
	{
		NewModel = newmodel;
		OldModel = oldmodel;
		if (NewModel != "")
		{
			lab_NewModel.Content = "AI標註(新模型) - " + NewModel.ToString();
			Station.aiEnhanceTools.ReadModel(NewModel, ReLoadModel: false);
		}
		if (OldModel != "")
		{
			lab_OldModel.Content = "AI標註(舊模型) - " + OldModel.ToString();
			aiEnhanceTools.ReadModel(OldModel, ReLoadModel: true);
		}
	}

	private void ReadModel(bool useNewModel)
	{
		if (useNewModel)
		{
			if (NewModel != "")
			{
				Station.aiEnhanceTools.ReadModel(NewModel, ReLoadModel: true);
			}
		}
		else if (OldModel != "")
		{
			Station.aiEnhanceTools.ReadModel(OldModel, ReLoadModel: true);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_PrevImage_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (MarkImgMode)
			{
				if (filelist != null && filelist.Length != 0)
				{
					ImageIndex--;
					if (ImageIndex >= filelist.Length)
					{
						ImageIndex = 0;
					}
					if (ImageIndex < 0)
					{
						ImageIndex = filelist.Length - 1;
					}
					btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
					Inspect(filelist[ImageIndex]);
				}
			}
			else if (ImageListSource != 0)
			{
				Mat mat = SwitchImage(-1);
				if (mat.Width > 0 && mat.Height > 0)
				{
					Inspect(mat);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreview-Btn_PrevImage_Click()-01-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NextImage_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (MarkImgMode)
			{
				if (filelist != null && filelist.Length != 0)
				{
					ImageIndex++;
					if (ImageIndex < 0)
					{
						ImageIndex = 0;
					}
					if (ImageIndex >= filelist.Length)
					{
						ImageIndex = 0;
					}
					btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
					Inspect(filelist[ImageIndex]);
				}
			}
			else if (ImageListSource != 0)
			{
				Mat mat = SwitchImage(1);
				if (mat.Width > 0 && mat.Height > 0)
				{
					Inspect(mat);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreview-Btn_NextImage_Click()-01-" + ex.ToString());
		}
	}

	private void ImageEnhance(ref Mat Src)
	{
		Mat[] mv = new Mat[3];
		Cv2.Split(Src, out mv);
		if (nu_Blue.Value != 1.0)
		{
			mv[0].ConvertTo(mv[0], MatType.CV_8UC1, nu_Blue.Value.Value);
		}
		if (nu_Green.Value != 1.0)
		{
			mv[1].ConvertTo(mv[1], MatType.CV_8UC1, nu_Green.Value.Value);
		}
		if (nu_Red.Value != 1.0)
		{
			mv[2].ConvertTo(mv[2], MatType.CV_8UC1, nu_Red.Value.Value);
		}
		Cv2.Merge(mv, Src);
		Mat[] mv2 = new Mat[3];
		Mat mat = new Mat();
		Cv2.CvtColor(Src, mat, ColorConversionCodes.BGR2HSV);
		Cv2.Split(mat, out mv2);
		if (nu_Hue.Value != 1.0)
		{
			mv2[0].ConvertTo(mv2[0], MatType.CV_8UC1, nu_Hue.Value.Value);
		}
		if (nu_Sat.Value != 1.0)
		{
			mv2[1].ConvertTo(mv2[1], MatType.CV_8UC1, nu_Sat.Value.Value);
		}
		if (nu_Val.Value != 1.0)
		{
			mv2[2].ConvertTo(mv2[2], MatType.CV_8UC1, nu_Val.Value.Value);
		}
		Cv2.Merge(mv2, mat);
		Cv2.CvtColor(mat, Src, ColorConversionCodes.HSV2BGR);
		if (chk_EnableVMirror.IsChecked == true)
		{
			Cv2.Flip(Src, Src, FlipMode.X);
		}
		if (chk_EnableHMirror.IsChecked == true)
		{
			Cv2.Flip(Src, Src, FlipMode.Y);
		}
		EnhanceMat = Src;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string ToGBK(string str)
	{
		Encoding encoding = Encoding.GetEncoding("GB2312");
		byte[] bytes = Encoding.Default.GetBytes(str);
		bytes = Encoding.Convert(Encoding.Default, encoding, bytes);
		return encoding.GetString(bytes);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string UTF8toGBK(string str)
	{
		Encoding encoding = Encoding.GetEncoding("UTF-8");
		Encoding encoding2 = Encoding.GetEncoding("GBK");
		byte[] bytes = encoding.GetBytes(str);
		bytes = Encoding.Convert(encoding, encoding2, bytes);
		return encoding2.GetString(bytes);
	}

	public static bool IsContainGBKString(string str)
	{
		char[] array = str.ToCharArray();
		foreach (char c in array)
		{
			if (IsGBKCode(c.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool IsGBKCode(string str)
	{
		byte[] bytes = Encoding.GetEncoding("GBK").GetBytes(str.ToString());
		if (bytes.Length <= 1)
		{
			return false;
		}
		byte b = bytes[0];
		byte b2 = bytes[1];
		if (b >= 129 && b <= 254 && b2 >= 64 && b2 <= 254)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string GBKtoUTF8(string str)
	{
		Encoding encoding = Encoding.GetEncoding("GBK");
		Encoding encoding2 = Encoding.GetEncoding("UTF-8");
		byte[] bytes = encoding.GetBytes(str);
		bytes = Encoding.Convert(encoding, encoding2, bytes);
		return encoding2.GetString(bytes);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string GBtoUTF8(string str)
	{
		Encoding encoding = Encoding.GetEncoding("GB2312");
		Encoding encoding2 = Encoding.GetEncoding("UTF-8");
		byte[] bytes = encoding.GetBytes(str);
		bytes = Encoding.Convert(encoding, encoding2, bytes);
		return encoding2.GetString(bytes);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string ToUTF8(string str)
	{
		Encoding encoding = Encoding.GetEncoding("UTF-8");
		byte[] bytes = Encoding.Default.GetBytes(str);
		bytes = Encoding.Convert(Encoding.Default, encoding, bytes);
		return encoding.GetString(bytes);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Mat ImRead(string path, ImreadModes imcolor)
	{
		Mat result = new Mat();
		try
		{
			if (IsContainGBKString(path))
			{
				Bitmap bitmap = new Bitmap(path);
				result = bitmap.ToMat();
				bitmap.Dispose();
			}
			else
			{
				result = Cv2.ImRead(path, imcolor);
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreview-ImRead()-01-" + ex.ToString());
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Inspect(string imagefile)
	{
		try
		{
			AIMatBuf = new Mat();
			EnhanceMat = new Mat();
			if (!File.Exists(imagefile))
			{
				cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
				return;
			}
			LastFile = imagefile;
			lab_FileName.Content = Path.GetFileName(imagefile);
			string text = "";
			if (imagefile.Contains("\\X\\NG\\"))
			{
				text = imagefile.Replace("\\X\\NG\\", "\\Y\\NG\\");
			}
			else if (imagefile.Contains("\\X\\OK\\"))
			{
				text = imagefile.Replace("\\X\\OK\\", "\\Y\\OK\\");
			}
			Mat Src = ImRead(imagefile, ImreadModes.Color);
			Src.CopyTo(AIMatBuf);
			Mat mat = new Mat();
			if (Src.Width <= 10 || Src.Height <= 10)
			{
				return;
			}
			if (chk_Enhance.IsChecked == true)
			{
				ImageEnhance(ref Src);
			}
			mat = Src.Clone();
			if (PreviewMode > 0)
			{
				Cv2.Split(Src, out AIMatBGR);
				switch (PreviewMode)
				{
				case 1:
					Cv2.CvtColor(AIMatBGR[2], mat, ColorConversionCodes.GRAY2BGR);
					break;
				case 2:
					Cv2.CvtColor(AIMatBGR[1], mat, ColorConversionCodes.GRAY2BGR);
					break;
				case 3:
					Cv2.CvtColor(AIMatBGR[0], mat, ColorConversionCodes.GRAY2BGR);
					break;
				default:
					Cv2.CvtColor(AIMatBGR[2], mat, ColorConversionCodes.GRAY2BGR);
					break;
				}
			}
			ImageSource imageSource = mat.ToBitmapSource();
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			double num = 320.0;
			double num2 = 476.0 / (double)Src.Width;
			double num3 = num / (double)Src.Height;
			if (num2 > num3)
			{
				num2 = num3;
			}
			else
			{
				num3 = num2;
			}
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num2);
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num3);
			if (grd_RotatePreview.Visibility == Visibility.Visible)
			{
				cnv_ImageAI2.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num2);
				cnv_ImageAI2.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num3);
				cnv_ImageH.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num2);
				cnv_ImageH.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num3);
				cnv_ImageV.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num2);
				cnv_ImageV.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num3);
				cnv_ImageVH.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num2);
				cnv_ImageVH.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num3);
			}
			if (Station.aiEnhanceTools.Inspect(Src, Offline: true, 0))
			{
				Mat mat2 = mat.Clone();
				if (Station.aiEnhanceTools.DestMatColor.Width > 0 && Station.aiEnhanceTools.DestMatColor.Height > 0)
				{
					if (Station.paramFile.AIEnhanceParam.DnetMode_A == 1 || Station.paramFile.AIEnhanceParam.DnetMode_A == 2)
					{
						Mat mat3 = new Mat();
						Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat3, ColorConversionCodes.BGR2GRAY);
						Cv2.InRange(mat3, 10.0, 255.0, mat3);
						Station.aiEnhanceTools.DestMatColor.CopyTo(mat2, mat3);
					}
					else
					{
						Station.aiEnhanceTools.DestMatColor.CopyTo(mat2);
					}
					if (File.Exists(text))
					{
						DestMat1 = ImRead(text, ImreadModes.Color);
					}
					else
					{
						Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat1);
					}
					if (mat2.Width > 10 && mat2.Height > 10)
					{
						ImageSource imageSource2 = mat2.ToBitmapSource();
						ImageBrush imageBrush2 = new ImageBrush();
						imageBrush2.ImageSource = imageSource2;
						cnv_ImageAI.Background = imageBrush2;
						if (grd_RotatePreview.Visibility == Visibility.Visible)
						{
							cnv_ImageAI2.Background = imageBrush2;
						}
						double num4 = 320.0;
						double num5 = 476.0 / (double)Src.Width;
						double num6 = num4 / (double)Src.Height;
						if (num5 > num6)
						{
							num5 = num6;
						}
						else
						{
							num6 = num5;
						}
						cnv_ImageAI.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num5);
						cnv_ImageAI.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num6);
					}
				}
			}
			else
			{
				cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
			}
			if (OldModel != "")
			{
				if (aiEnhanceTools.Inspect(Src, Offline: true, 0))
				{
					Mat mat4 = mat.Clone();
					if (aiEnhanceTools.DestMatColor.Width > 0 && aiEnhanceTools.DestMatColor.Height > 0)
					{
						if (Station.paramFile.AIEnhanceParam.DnetMode_A == 1 || Station.paramFile.AIEnhanceParam.DnetMode_A == 2)
						{
							Mat mat5 = new Mat();
							Cv2.CvtColor(aiEnhanceTools.DestMatColor, mat5, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat5, 10.0, 255.0, mat5);
							aiEnhanceTools.DestMatColor.CopyTo(mat4, mat5);
						}
						else
						{
							aiEnhanceTools.DestMatColor.CopyTo(mat4);
						}
						if (mat4.Width > 10 && mat4.Height > 10)
						{
							ImageSource imageSource3 = mat4.ToBitmapSource();
							ImageBrush imageBrush3 = new ImageBrush();
							imageBrush3.ImageSource = imageSource3;
							cnv_ImageAIOld.Background = imageBrush3;
							double num7 = 320.0;
							double num8 = 476.0 / (double)Src.Width;
							double num9 = num7 / (double)Src.Height;
							if (num8 > num9)
							{
								num8 = num9;
							}
							else
							{
								num9 = num8;
							}
							cnv_ImageAIOld.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num8);
							cnv_ImageAIOld.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num9);
						}
					}
				}
			}
			else
			{
				cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
			}
			ReDrawFile = "";
			if (File.Exists(text) && text != "")
			{
				lab_FileNotExist.Visibility = Visibility.Hidden;
				Mat mat6 = ImRead(text, ImreadModes.Color);
				Mat mat7 = new Mat();
				if (mat6.Width > 10 && mat6.Height > 10)
				{
					if (Station.paramFile.AIEnhanceParam.DnetMode_A == 1 || Station.paramFile.AIEnhanceParam.DnetMode_A == 2)
					{
						Mat mat8 = new Mat();
						Cv2.CvtColor(mat6, mat8, ColorConversionCodes.BGR2GRAY);
						Cv2.InRange(mat8, 10.0, 255.0, mat8);
						DrawingMat = new Mat();
						if (chk_Enhance.IsChecked == true)
						{
							if (chk_EnableVMirror.IsChecked == true && chk_EnableHMirror.IsChecked == true)
							{
								Cv2.Flip(mat6, mat6, FlipMode.XY);
								Cv2.Flip(mat8, mat8, FlipMode.XY);
							}
							else if (chk_EnableHMirror.IsChecked == true)
							{
								Cv2.Flip(mat6, mat6, FlipMode.Y);
								Cv2.Flip(mat8, mat8, FlipMode.Y);
							}
							else if (chk_EnableVMirror.IsChecked == true)
							{
								Cv2.Flip(mat6, mat6, FlipMode.X);
								Cv2.Flip(mat8, mat8, FlipMode.X);
							}
							DrawingMat = mat6;
						}
						Src.CopyTo(mat7);
						mat6.CopyTo(mat7, mat8);
						bool flag = false;
						Mat mat9 = Src.Clone();
						Cv2.Flip(mat9, mat9, FlipMode.XY);
						Cv2.Flip(mat6, DrawingMatVH, FlipMode.XY);
						mat9.CopyTo(EnhanceMatVH);
						if (Station.aiEnhanceTools.Inspect(mat9, Offline: true, 0) && Station.aiEnhanceTools.DestMatColor.Width > 0 && Station.aiEnhanceTools.DestMatColor.Height > 0)
						{
							mat8 = new Mat();
							Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat8, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat8, 10.0, 255.0, mat8);
							Station.aiEnhanceTools.DestMatColor.CopyTo(mat9, mat8);
							using (Mat mat10 = new Mat())
							{
								using Mat mat11 = new Mat();
								using Mat mat12 = new Mat();
								Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat2);
								Cv2.Flip(DestMat2, DestMat2, FlipMode.XY);
								Cv2.BitwiseAnd(DestMat1, DestMat2, mat10);
								Cv2.CvtColor(DestMat1, mat11, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat11, 50.0, 255.0, mat11);
								int num10 = mat11.CountNonZero();
								Cv2.CvtColor(mat10, mat12, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat12, 50.0, 255.0, mat12);
								int num11 = mat12.CountNonZero();
								if (num10 > 0)
								{
									if ((double)num11 / (double)num10 < 0.5)
									{
										ico_ResultAlartVH.Visibility = Visibility.Visible;
										flag = true;
									}
									else
									{
										ico_ResultAlartVH.Visibility = Visibility.Hidden;
									}
								}
							}
							ImageSource imageSource4 = mat9.ToBitmapSource();
							ImageBrush imageBrush4 = new ImageBrush();
							imageBrush4.ImageSource = imageSource4;
							cnv_ImageVH.Background = imageBrush4;
						}
						Mat mat13 = Src.Clone();
						Cv2.Flip(mat13, mat13, FlipMode.Y);
						Cv2.Flip(mat6, DrawingMatH, FlipMode.Y);
						mat13.CopyTo(EnhanceMatH);
						if (Station.aiEnhanceTools.Inspect(mat13, Offline: true, 0))
						{
							mat8 = new Mat();
							Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat8, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat8, 10.0, 255.0, mat8);
							Station.aiEnhanceTools.DestMatColor.CopyTo(mat13, mat8);
							using (Mat mat14 = new Mat())
							{
								using Mat mat15 = new Mat();
								using Mat mat16 = new Mat();
								Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat3);
								Cv2.Flip(DestMat3, DestMat3, FlipMode.Y);
								Cv2.BitwiseAnd(DestMat1, DestMat3, mat14);
								Cv2.CvtColor(DestMat1, mat15, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat15, 50.0, 255.0, mat15);
								int num12 = mat15.CountNonZero();
								Cv2.CvtColor(mat14, mat16, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat16, 50.0, 255.0, mat16);
								int num13 = mat16.CountNonZero();
								if (num12 > 0)
								{
									if ((double)num13 / (double)num12 < 0.5)
									{
										ico_ResultAlartH.Visibility = Visibility.Visible;
										flag = true;
									}
									else
									{
										ico_ResultAlartH.Visibility = Visibility.Hidden;
									}
								}
							}
							ImageSource imageSource5 = mat13.ToBitmapSource();
							ImageBrush imageBrush5 = new ImageBrush();
							imageBrush5.ImageSource = imageSource5;
							cnv_ImageH.Background = imageBrush5;
						}
						Mat mat17 = Src.Clone();
						Cv2.Flip(mat17, mat17, FlipMode.X);
						Cv2.Flip(mat6, DrawingMatV, FlipMode.X);
						mat17.CopyTo(EnhanceMatV);
						if (Station.aiEnhanceTools.Inspect(mat17, Offline: true, 0))
						{
							mat8 = new Mat();
							Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat8, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat8, 10.0, 255.0, mat8);
							Station.aiEnhanceTools.DestMatColor.CopyTo(mat17, mat8);
							using (Mat mat18 = new Mat())
							{
								using Mat mat19 = new Mat();
								using Mat mat20 = new Mat();
								Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat4);
								Cv2.Flip(DestMat4, DestMat4, FlipMode.X);
								Cv2.BitwiseAnd(DestMat1, DestMat4, mat18);
								Cv2.CvtColor(DestMat1, mat19, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat19, 50.0, 255.0, mat19);
								int num14 = mat19.CountNonZero();
								Cv2.CvtColor(mat18, mat20, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat20, 50.0, 255.0, mat20);
								int num15 = mat20.CountNonZero();
								if (num14 > 0)
								{
									if ((double)num15 / (double)num14 < 0.5)
									{
										ico_ResultAlartV.Visibility = Visibility.Visible;
										flag = true;
									}
									else
									{
										ico_ResultAlartV.Visibility = Visibility.Hidden;
									}
								}
							}
							ImageSource imageSource6 = mat17.ToBitmapSource();
							ImageBrush imageBrush6 = new ImageBrush();
							imageBrush6.ImageSource = imageSource6;
							cnv_ImageV.Background = imageBrush6;
						}
						if (flag && Station.paramFile.ThroughputFile.CurrentClient != 3)
						{
							btn_RotateInspectAlert.Visibility = Visibility.Visible;
						}
						else
						{
							btn_RotateInspectAlert.Visibility = Visibility.Hidden;
						}
					}
					else
					{
						mat6.CopyTo(mat7);
					}
					if (mat7.Width > 10 && mat7.Height > 10)
					{
						ImageSource imageSource7 = mat7.ToBitmapSource();
						ImageBrush imageBrush7 = new ImageBrush();
						imageBrush7.ImageSource = imageSource7;
						cnv_ImageDraw.Background = imageBrush7;
						double num16 = 320.0;
						double num17 = 476.0 / (double)Src.Width;
						double num18 = num16 / (double)Src.Height;
						if (num17 > num18)
						{
							num17 = num18;
						}
						else
						{
							num18 = num17;
						}
						cnv_ImageDraw.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num17);
						cnv_ImageDraw.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num18);
						if (grd_RotatePreview.Visibility == Visibility.Visible)
						{
							cnv_ImageAI2.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num17);
							cnv_ImageAI2.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num18);
							cnv_ImageH.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num17);
							cnv_ImageH.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num18);
							cnv_ImageV.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num17);
							cnv_ImageV.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num18);
							cnv_ImageVH.SetValue(FrameworkElement.WidthProperty, (double)Src.Width * num17);
							cnv_ImageVH.SetValue(FrameworkElement.HeightProperty, (double)Src.Height * num18);
						}
						btn_Redraw.Visibility = Visibility.Visible;
						ReDrawFile = text;
					}
				}
			}
			else
			{
				lab_FileNotExist.Visibility = Visibility.Visible;
				if (MarkImgMode && text != "")
				{
					cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
					btn_Redraw.Visibility = Visibility.Visible;
					ReDrawFile = text;
				}
				else
				{
					cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
				}
			}
			string text2 = GetUpLevelDirectory(Path.GetDirectoryName(imagefile), 1);
			if (text2 == "ALL")
			{
				text2 = "未分類";
			}
			else if (text2 == "NG" || text2 == "OK")
			{
				text2 = "本次新增，未分類影像";
			}
			lab_ImageClass.Content = text2;
		}
		catch (Exception ex)
		{
			string text3 = "AIP02-AIPreviewWindow-Inspect()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
		}
	}

	private string GetUpLevelDirectory(string path, int upLevel)
	{
		string text = (File.GetAttributes(path).HasFlag(FileAttributes.Directory) ? path : Path.GetDirectoryName(path));
		upLevel = ((upLevel >= 0) ? upLevel : 0);
		for (int i = 0; i < upLevel; i++)
		{
			text = Path.GetFileName(text);
		}
		return text;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Inspect(Mat AIMat)
	{
		try
		{
			AIMatBuf = new Mat();
			Mat mat = new Mat();
			if (AIMat.Width > 0 && AIMat.Height > 0)
			{
				if (chk_Enhance.IsChecked == true)
				{
					ImageEnhance(ref AIMat);
				}
				mat = AIMat.Clone();
				if (PreviewMode > 0)
				{
					Cv2.Split(AIMat, out AIMatBGR);
					switch (PreviewMode)
					{
					case 1:
						Cv2.CvtColor(AIMatBGR[2], mat, ColorConversionCodes.GRAY2BGR);
						break;
					case 2:
						Cv2.CvtColor(AIMatBGR[1], mat, ColorConversionCodes.GRAY2BGR);
						break;
					case 3:
						Cv2.CvtColor(AIMatBGR[0], mat, ColorConversionCodes.GRAY2BGR);
						break;
					default:
						Cv2.CvtColor(AIMatBGR[2], mat, ColorConversionCodes.GRAY2BGR);
						break;
					}
				}
				ImageSource imageSource = mat.ToBitmapSource();
				ImageBrush imageBrush = new ImageBrush();
				imageBrush.ImageSource = imageSource;
				cnv_Image.Background = imageBrush;
				AIMat.CopyTo(AIMatBuf);
				if (Station.aiEnhanceTools.Inspect(AIMat, Offline: true, 0))
				{
					Mat mat2 = mat.Clone();
					if (Station.aiEnhanceTools.DestMatColor.Width > 0 && Station.aiEnhanceTools.DestMatColor.Height > 0)
					{
						if (Station.paramFile.AIEnhanceParam.DnetMode_A == 1 || Station.paramFile.AIEnhanceParam.DnetMode_A == 2)
						{
							Mat mat3 = new Mat();
							Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat3, ColorConversionCodes.BGR2GRAY);
							Cv2.InRange(mat3, 10.0, 255.0, mat3);
							Station.aiEnhanceTools.DestMatColor.CopyTo(mat2, mat3);
						}
						else
						{
							Station.aiEnhanceTools.DestMatColor.CopyTo(mat2);
						}
						Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat1);
						if (mat2.Width > 10 && mat2.Height > 10)
						{
							ImageSource imageSource2 = mat2.ToBitmapSource();
							ImageBrush imageBrush2 = new ImageBrush();
							imageBrush2.ImageSource = imageSource2;
							cnv_ImageAI.Background = imageBrush2;
							cnv_ImageAI2.Background = imageBrush2;
						}
					}
				}
				else
				{
					cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
				}
				if (OldModel != "")
				{
					if (aiEnhanceTools.Inspect(AIMat, Offline: true, 0))
					{
						Mat mat4 = mat.Clone();
						if (aiEnhanceTools.DestMatColor.Width > 0 && aiEnhanceTools.DestMatColor.Height > 0)
						{
							if (Station.paramFile.AIEnhanceParam.DnetMode_A == 1 || Station.paramFile.AIEnhanceParam.DnetMode_A == 2)
							{
								Mat mat5 = new Mat();
								Cv2.CvtColor(aiEnhanceTools.DestMatColor, mat5, ColorConversionCodes.BGR2GRAY);
								Cv2.InRange(mat5, 10.0, 255.0, mat5);
								aiEnhanceTools.DestMatColor.CopyTo(mat4, mat5);
							}
							else
							{
								aiEnhanceTools.DestMatColor.CopyTo(mat4);
							}
							if (mat4.Width > 10 && mat4.Height > 10)
							{
								ImageSource imageSource3 = mat4.ToBitmapSource();
								ImageBrush imageBrush3 = new ImageBrush();
								imageBrush3.ImageSource = imageSource3;
								cnv_ImageAIOld.Background = imageBrush3;
							}
						}
					}
				}
				else
				{
					cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
				}
				if (grd_RotatePreview.Visibility == Visibility.Visible)
				{
					Mat mat6 = new Mat();
					Mat mat7 = AIMat.Clone();
					Cv2.Flip(mat7, mat7, FlipMode.XY);
					if (Station.aiEnhanceTools.Inspect(mat7, Offline: true, 0))
					{
						mat6 = new Mat();
						Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat6, ColorConversionCodes.BGR2GRAY);
						Cv2.InRange(mat6, 10.0, 255.0, mat6);
						Station.aiEnhanceTools.DestMatColor.CopyTo(mat7, mat6);
						using (Mat mat8 = new Mat())
						{
							using Mat mat9 = new Mat();
							Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat2);
							Cv2.Flip(DestMat2, DestMat2, FlipMode.XY);
							Cv2.BitwiseAnd(DestMat1, DestMat2, mat8);
							Cv2.CvtColor(DestMat1, mat9, ColorConversionCodes.BGR2GRAY);
							int num = mat9.CountNonZero();
							Cv2.CvtColor(mat8, mat9, ColorConversionCodes.BGR2GRAY);
							int num2 = mat9.CountNonZero();
							if (num > 0)
							{
								if ((double)num2 / (double)num < 0.5)
								{
									ico_ResultAlartVH.Visibility = Visibility.Visible;
								}
								else
								{
									ico_ResultAlartVH.Visibility = Visibility.Hidden;
								}
							}
						}
						ImageSource imageSource4 = mat7.ToBitmapSource();
						ImageBrush imageBrush4 = new ImageBrush();
						imageBrush4.ImageSource = imageSource4;
						cnv_ImageVH.Background = imageBrush4;
					}
					Mat mat10 = AIMat.Clone();
					Cv2.Flip(mat10, mat10, FlipMode.Y);
					if (Station.aiEnhanceTools.Inspect(mat10, Offline: true, 0))
					{
						mat6 = new Mat();
						Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat6, ColorConversionCodes.BGR2GRAY);
						Cv2.InRange(mat6, 10.0, 255.0, mat6);
						Station.aiEnhanceTools.DestMatColor.CopyTo(mat10, mat6);
						using (Mat mat11 = new Mat())
						{
							using Mat mat12 = new Mat();
							Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat3);
							Cv2.Flip(DestMat3, DestMat3, FlipMode.Y);
							Cv2.BitwiseAnd(DestMat1, DestMat3, mat11);
							Cv2.CvtColor(DestMat1, mat12, ColorConversionCodes.BGR2GRAY);
							int num3 = mat12.CountNonZero();
							Cv2.CvtColor(mat11, mat12, ColorConversionCodes.BGR2GRAY);
							int num4 = mat12.CountNonZero();
							if (num3 > 0)
							{
								if ((double)num4 / (double)num3 < 0.5)
								{
									ico_ResultAlartH.Visibility = Visibility.Visible;
								}
								else
								{
									ico_ResultAlartH.Visibility = Visibility.Hidden;
								}
							}
						}
						ImageSource imageSource5 = mat10.ToBitmapSource();
						ImageBrush imageBrush5 = new ImageBrush();
						imageBrush5.ImageSource = imageSource5;
						cnv_ImageH.Background = imageBrush5;
					}
					Mat mat13 = AIMat.Clone();
					Cv2.Flip(mat13, mat13, FlipMode.X);
					if (Station.aiEnhanceTools.Inspect(mat13, Offline: true, 0))
					{
						mat6 = new Mat();
						Cv2.CvtColor(Station.aiEnhanceTools.DestMatColor, mat6, ColorConversionCodes.BGR2GRAY);
						Cv2.InRange(mat6, 10.0, 255.0, mat6);
						Station.aiEnhanceTools.DestMatColor.CopyTo(mat13, mat6);
						using (Mat mat14 = new Mat())
						{
							using Mat mat15 = new Mat();
							Station.aiEnhanceTools.DestMatColor.CopyTo(DestMat4);
							Cv2.Flip(DestMat4, DestMat4, FlipMode.X);
							Cv2.BitwiseAnd(DestMat1, DestMat4, mat14);
							Cv2.CvtColor(DestMat1, mat15, ColorConversionCodes.BGR2GRAY);
							int num5 = mat15.CountNonZero();
							Cv2.CvtColor(mat14, mat15, ColorConversionCodes.BGR2GRAY);
							int num6 = mat15.CountNonZero();
							if (num5 > 0)
							{
								if ((double)num6 / (double)num5 < 0.5)
								{
									ico_ResultAlartV.Visibility = Visibility.Visible;
								}
								else
								{
									ico_ResultAlartV.Visibility = Visibility.Hidden;
								}
							}
						}
						ImageSource imageSource6 = mat13.ToBitmapSource();
						ImageBrush imageBrush6 = new ImageBrush();
						imageBrush6.ImageSource = imageSource6;
						cnv_ImageV.Background = imageBrush6;
					}
				}
				cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
			}
			else
			{
				cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
				cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
			}
		}
		catch (Exception ex)
		{
			string text = "AIP02-AIPreviewWindow-Inspect()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	private void Btn_NewModel_Click(object sender, RoutedEventArgs e)
	{
		ReadModel(useNewModel: true);
		if (filelist != null && filelist.Length != 0)
		{
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			Inspect(filelist[ImageIndex]);
		}
	}

	private void Btn_OldModel_Click(object sender, RoutedEventArgs e)
	{
		ReadModel(useNewModel: false);
		if (filelist != null && filelist.Length != 0)
		{
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			Inspect(filelist[ImageIndex]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Wnd_Main_Drop(object sender, DragEventArgs e)
	{
		try
		{
			if (cb_ImageDir.Items.Count > 0)
			{
				UpdateUI = false;
				cb_ImageDir.Items.Clear();
				UpdateUI = true;
			}
			string[] array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
			if (array.Length == 0)
			{
				btn_ImageNum.Content = string.Format("{0} / {1}", 0, filelist.Length);
				return;
			}
			string text = array[0];
			MarkImgMode = false;
			if (!string.IsNullOrEmpty(text) && File.Exists(text))
			{
				lab_Current.Content = "Drop FILE";
				ImageListSource = 0;
				Inspect(text);
				btn_ImageNum.Content = string.Format("{0} / {1}", 1, array.Length);
				return;
			}
			lab_Current.Content = "Drop FILE";
			dropfilelist = Directory.GetFiles(text, "*.png", SearchOption.AllDirectories);
			SortAsFileTime(ref dropfilelist);
			dropfileIndex = 0;
			ImageIndex = 0;
			btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, dropfilelist.Length);
			ImageListSource = 5;
			if (dropfilelist.Length != 0)
			{
				Mat mat = SwitchImage(0);
				if (mat.Width > 0 && mat.Height > 0)
				{
					Inspect(mat);
				}
			}
		}
		catch (Exception ex)
		{
			string text2 = "AIP01-AIPreviewWindow-Wnd_Main_Drop()" + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
		}
	}

	private void Wnd_Main_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta > 0)
		{
			Btn_PrevImage_Click(this, null);
		}
		else
		{
			Btn_NextImage_Click(this, null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Delete_Click(object sender, RoutedEventArgs e)
	{
		if (filelist == null || filelist.Length == 0)
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("當前的AI資料庫影像將被刪除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("当前的AI资料库影像将被删除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Delete current AI database image ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在のAIデータベースの画像が削除されます!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		try
		{
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			string text = filelist[ImageIndex];
			if (!File.Exists(text))
			{
				return;
			}
			string text2 = "";
			if (text.Contains("\\X\\NG\\"))
			{
				text2 = text.Replace("\\X\\NG\\", "\\Y\\NG\\");
			}
			else if (text.Contains("\\X\\OK\\"))
			{
				text2 = text.Replace("\\X\\OK\\", "\\Y\\OK\\");
			}
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text);
				ImageIndex--;
			}
			if (File.Exists(text2) && text2 != "")
			{
				FileInfo fileInfo2 = new FileInfo(text2);
				if (fileInfo2.IsReadOnly)
				{
					fileInfo2.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			int imageIndex = ImageIndex;
			Cb_ImageDir_SelectionChanged(this, null);
			ImageIndex = imageIndex;
			Btn_NextImage_Click(this, null);
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIPreviewWindow-Btn_Delete_Click()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
		}
	}

	private void Cb_ImageDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI || SubDir == null || SubDir.Length < 0)
		{
			return;
		}
		ImageIndex = 0;
		if (cb_ImageDir.SelectedIndex == 0)
		{
			if (UseNGImage)
			{
				UpdateList(ImageXNGPath);
			}
			else
			{
				UpdateList(ImageXOKPath);
			}
			btn_MoveFolder.IsEnabled = true;
		}
		else if (cb_ImageDir.SelectedIndex == 1)
		{
			if (UseNGImage)
			{
				UpdateList(ImageXNGPath, SearchAll: false);
			}
			else
			{
				UpdateList(ImageXOKPath, SearchAll: false);
			}
			btn_MoveFolder.IsEnabled = true;
		}
		else if (cb_ImageDir.SelectedIndex - 2 < SubDir.Length)
		{
			UpdateList(SubDir[cb_ImageDir.SelectedIndex - 2]);
			btn_MoveFolder.IsEnabled = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Redraw_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (MarkImgMode)
			{
				if (ReDrawFile == "" || filelist == null || filelist.Length == 0)
				{
					return;
				}
				if (ImageIndex >= filelist.Length)
				{
					ImageIndex = filelist.Length - 1;
				}
				if (ImageIndex <= 0)
				{
					ImageIndex = 0;
				}
				string text = filelist[ImageIndex];
				Mat mat = ImRead(text, ImreadModes.Color);
				if (mat.Width <= 0 || mat.Height <= 0)
				{
					return;
				}
				string path = text.Replace("\\X\\NG\\", "\\Y\\NG\\");
				Mat mat2 = new Mat();
				if (File.Exists(path))
				{
					mat2 = ImRead(path, ImreadModes.Color);
				}
				else
				{
					mat2 = mat.Clone();
					mat2.SetTo(0.0);
				}
				if (!File.Exists(ReDrawFile))
				{
					Mat mat3 = mat.Clone();
					mat3.SetTo(0.0);
					mat3.SaveImage(ReDrawFile);
				}
				AIMarkWindow aIMarkWindow = new AIMarkWindow();
				aIMarkWindow.SetInspection(Station, StationIndex);
				aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
				aIMarkWindow.SetSAM(mSam);
				aIMarkWindow.LoadLanguage(LanguageType);
				aIMarkWindow.SetMat(mat, Station.paramFile.ZoomMode);
				aIMarkWindow.SetSingleRedraw(ReDrawFile);
				aIMarkWindow.SetPreviewMode(Station.paramFile.PreviewMode);
				aIMarkWindow.SetColorSelect(Station.paramFile.ColorSelect);
				aIMarkWindow.SetAutoDrawIgnore(Station.paramFile.EnableAutoIgnoreDraw, Station.paramFile.AutoIgnoreSize);
				aIMarkWindow.SetPenSize(Station.paramFile.PenSize);
				aIMarkWindow.SetCaption(Station.paramFile.DrawCaition, Station.paramFile.SaveAsFolder);
				if (mat2.Width >= 0 && mat2.Height >= 0)
				{
					aIMarkWindow.DecodeTempDraw(mat2);
				}
				aIMarkWindow.ShowDialog();
				aIMarkWindow.GetCaption(ref caption, ref Station.paramFile.SaveAsFolder);
				for (int i = 0; i < 6; i++)
				{
					if (Station.paramFile.DrawCaition[i] != caption[i])
					{
						Station.paramFile.DrawCaition[i] = caption[i];
					}
				}
				Inspect(text);
				Station.paramFile.AutoIgnoreSize = aIMarkWindow.GetAutoDrawIgnoreSize();
			}
			else if (!(X_NG_Path == "") && !(Y_NG_Path == "") && AIMatBuf.Width > 0 && AIMatBuf.Height > 0)
			{
				AIMarkWindow aIMarkWindow2 = new AIMarkWindow();
				aIMarkWindow2.SetInspection(Station, StationIndex);
				aIMarkWindow2.LoadLanguage(LanguageType);
				aIMarkWindow2.SetMat(AIMatBuf, Station.paramFile.ZoomMode);
				aIMarkWindow2.SetPreviewMode(Station.paramFile.PreviewMode);
				aIMarkWindow2.SetColorSelect(Station.paramFile.ColorSelect);
				aIMarkWindow2.SetAutoDrawIgnore(Station.paramFile.EnableAutoIgnoreDraw, Station.paramFile.AutoIgnoreSize);
				aIMarkWindow2.SetPenSize(Station.paramFile.PenSize);
				aIMarkWindow2.SetNGMarkedFolder(X_NG_Path, Y_NG_Path);
				aIMarkWindow2.ShowDialog();
				Station.paramFile.AutoIgnoreSize = aIMarkWindow2.GetAutoDrawIgnoreSize();
				lab_FileNotExist.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex)
		{
			string text2 = "AIP01-AIPreviewWindow-Btn_Redraw_Click()" + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OKImage_Click(object sender, RoutedEventArgs e)
	{
		UseNGImage = false;
		lab_Current.Content = "OK標註";
		MarkImgMode = true;
		btn_Delete.IsEnabled = true;
		SetImagePath(ImageXNGPath, ImageXOKPath, DefalutNg: false);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NGImage_Click(object sender, RoutedEventArgs e)
	{
		UseNGImage = true;
		lab_Current.Content = "NG標註";
		MarkImgMode = true;
		btn_Delete.IsEnabled = true;
		SetImagePath(ImageXNGPath, ImageXOKPath);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string FindSameImage(string[] list1, string[] list2 = null)
	{
		string result = "";
		if (list1 == null)
		{
			return result;
		}
		try
		{
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			for (int i = 0; i < list1.Length; i++)
			{
				list3.Add(list1[i]);
			}
			if (list2 != null)
			{
				for (int j = 0; j < list2.Length; j++)
				{
					list3.Add(list2[j]);
				}
			}
			for (int k = 0; k < list3.Count; k++)
			{
				list4.Add(GetFileHash(list3[k]));
			}
			for (int l = 0; l < list4.Count; l++)
			{
				if (list4[l] == "")
				{
					continue;
				}
				for (int m = 0; m < list4.Count; m++)
				{
					if (!(list3[l] == list3[m]) && list4[l] == list4[m])
					{
						Path.GetFileName(list3[l]);
						Path.GetFileName(list3[m]);
						string text = list3[l];
						string oldDrawpath = "";
						bool oldstatus = true;
						if (text.Contains("\\X\\NG\\"))
						{
							oldDrawpath = text.Replace("\\X\\NG\\", "\\Y\\NG\\");
							oldstatus = false;
						}
						else if (text.Contains("\\X\\OK\\"))
						{
							oldDrawpath = text.Replace("\\X\\OK\\", "\\Y\\OK\\");
							oldstatus = true;
						}
						string text2 = list3[m];
						string newDrawpath = "";
						bool newstatus = true;
						if (text2.Contains("\\X\\NG\\"))
						{
							newDrawpath = text2.Replace("\\X\\NG\\", "\\Y\\NG\\");
							newstatus = false;
						}
						else if (text2.Contains("\\X\\OK\\"))
						{
							newDrawpath = text2.Replace("\\X\\OK\\", "\\Y\\OK\\");
							newstatus = true;
						}
						SelectImageWindow selectImageWindow = new SelectImageWindow();
						selectImageWindow.SetImage(text, oldDrawpath, oldstatus, text2, newDrawpath, newstatus);
						selectImageWindow.ShowDialog();
						switch (selectImageWindow.GetDeleteResult())
						{
						case 1:
							list4[l] = "";
							break;
						case 2:
							list4[m] = "";
							break;
						}
					}
				}
			}
			UpdateList(ImageXNGPath);
			return result;
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIPreviewWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
			return result;
		}
	}

	private string GetFileHash(string file)
	{
		string result = "";
		if (!File.Exists(file))
		{
			return result;
		}
		HashAlgorithm hashAlgorithm = HashAlgorithm.Create();
		FileStream fileStream = new FileStream(file, FileMode.Open);
		result = BitConverter.ToString(hashAlgorithm.ComputeHash(fileStream));
		fileStream.Close();
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Wnd_Main_Loaded(object sender, RoutedEventArgs e)
	{
		try
		{
			string[] files = Directory.GetFiles(ImageXNGPath, "*.bmp", SearchOption.AllDirectories);
			string[] files2 = Directory.GetFiles(ImageXNGPath, "*.png", SearchOption.AllDirectories);
			string[] arrFi = files.Concat(files2).ToArray();
			SortAsFileTime(ref arrFi);
			string[] array = null;
			string[] files3 = Directory.GetFiles(ImageXOKPath, "*.bmp", SearchOption.AllDirectories);
			files2 = Directory.GetFiles(ImageXOKPath, "*.png", SearchOption.AllDirectories);
			array = files3.Concat(files2).ToArray();
			SortAsFileTime(ref array);
			FindSameImage(arrFi, array);
		}
		catch
		{
		}
	}

	private void Nu_Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
	{
		if (MarkImgMode)
		{
			if (LastFile == "" || chk_Enhance.IsChecked == false)
			{
				return;
			}
			Inspect(LastFile);
		}
		else
		{
			if (chk_Enhance.IsChecked == false)
			{
				return;
			}
			Mat mat = SwitchImage(0);
			if (mat.Width > 0 && mat.Height > 0)
			{
				Inspect(mat);
			}
		}
		NumericUpDown numericUpDown = (NumericUpDown)sender;
		if (numericUpDown.Value == 1.0)
		{
			numericUpDown.Background = System.Windows.Media.Brushes.White;
		}
		else
		{
			numericUpDown.Background = System.Windows.Media.Brushes.Gold;
		}
	}

	private void Chk_EnableRotate_Click(object sender, RoutedEventArgs e)
	{
		if (chk_Enhance.IsChecked == true)
		{
			grd_Enhance.Width = 1024.0;
			chk_RotatePreview.IsChecked = false;
		}
		else
		{
			grd_Enhance.Width = 170.0;
			chk_RotatePreview.IsChecked = true;
		}
		if (MarkImgMode)
		{
			if (!(LastFile == "") && chk_Enhance.IsChecked != false)
			{
				Inspect(LastFile);
			}
		}
		else if (chk_Enhance.IsChecked != false)
		{
			Mat mat = SwitchImage(0);
			if (mat.Width > 0 && mat.Height > 0)
			{
				Inspect(mat);
			}
		}
	}

	private void Btn_SetPara_Click(object sender, RoutedEventArgs e)
	{
		if (chk_Enhance.IsChecked != false && ThroughputFile != null)
		{
			ThroughputFile.param.TrainingImageDoubleRate[StationIndex] = 1.0;
			if (chk_EnableHMirror.IsChecked == true)
			{
				ThroughputFile.param.Training_horizontalFlip[StationIndex] = 1.0;
			}
			else
			{
				ThroughputFile.param.Training_horizontalFlip[StationIndex] = 0.0;
			}
			if (chk_EnableVMirror.IsChecked == true)
			{
				ThroughputFile.param.Training_verticalFlip[StationIndex] = 1.0;
			}
			else
			{
				ThroughputFile.param.Training_verticalFlip[StationIndex] = 0.0;
			}
			ThroughputFile.param.Training_brightness_limit[StationIndex] = 0.0;
			ThroughputFile.param.Training_contrast_limit[StationIndex] = 0.0;
			ThroughputFile.param.Training_scale_limit[StationIndex] = 0.0;
			ThroughputFile.param.Training_rotate_limit[StationIndex] = 0.0;
			ThroughputFile.param.Training_shift_limit[StationIndex] = 0.0;
			if (nu_Hue.Value != 1.0)
			{
				ThroughputFile.param.Training_hue_shift_limit[StationIndex] = Math.Abs((int)((1.0 - nu_Hue.Value) * 100.0).Value);
			}
			else
			{
				ThroughputFile.param.Training_hue_shift_limit[StationIndex] = 0;
			}
			if (nu_Sat.Value != 1.0)
			{
				ThroughputFile.param.Training_sat_shift_limit[StationIndex] = Math.Abs((int)((1.0 - nu_Sat.Value) * 100.0).Value);
			}
			else
			{
				ThroughputFile.param.Training_sat_shift_limit[StationIndex] = 0;
			}
			if (nu_Val.Value != 1.0)
			{
				ThroughputFile.param.Training_val_shift_limit[StationIndex] = Math.Abs((int)((1.0 - nu_Val.Value) * 100.0).Value);
			}
			else
			{
				ThroughputFile.param.Training_val_shift_limit[StationIndex] = 0;
			}
			if (nu_Red.Value != 1.0)
			{
				ThroughputFile.param.Training_r_shift_limit[StationIndex] = Math.Abs((int)((1.0 - nu_Red.Value) * 100.0).Value);
			}
			else
			{
				ThroughputFile.param.Training_r_shift_limit[StationIndex] = 0;
			}
			if (nu_Green.Value != 1.0)
			{
				ThroughputFile.param.Training_g_shift_limit[StationIndex] = Math.Abs((int)((1.0 - nu_Green.Value) * 100.0).Value);
			}
			else
			{
				ThroughputFile.param.Training_g_shift_limit[StationIndex] = 0;
			}
			if (nu_Blue.Value != 1.0)
			{
				ThroughputFile.param.Training_b_shift_limit[StationIndex] = Math.Abs((int)((1.0 - nu_Blue.Value) * 100.0).Value);
			}
			else
			{
				ThroughputFile.param.Training_b_shift_limit[StationIndex] = 0;
			}
			ThroughputFile.WriteThroughputParam();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_BufferImgALL_Click(object sender, RoutedEventArgs e)
	{
		MarkImgMode = false;
		btn_Delete.IsEnabled = false;
		Button obj = (Button)sender;
		Button button = btn_SaveToNGDB_H;
		Button button2 = btn_SaveToNGDB_V;
		Visibility visibility = (btn_SaveToNGDB_VH.Visibility = Visibility.Visible);
		Visibility visibility3 = (button2.Visibility = visibility);
		button.Visibility = visibility3;
		string uid = obj.Uid;
		if (!(uid == "ALL"))
		{
			if (!(uid == "OK"))
			{
				if (!(uid == "NG"))
				{
					if (uid == "FILE")
					{
						lab_Current.Content = "FILE";
						ImageListSource = 1;
						Button button3 = btn_SaveToNGDB_H;
						Button button4 = btn_SaveToNGDB_V;
						visibility = (btn_SaveToNGDB_VH.Visibility = Visibility.Hidden);
						visibility3 = (button4.Visibility = visibility);
						button3.Visibility = visibility3;
					}
					else
					{
						ImageListSource = 1;
					}
				}
				else
				{
					lab_Current.Content = "NG";
					ImageListSource = 4;
				}
			}
			else
			{
				lab_Current.Content = "OK";
				ImageListSource = 3;
			}
		}
		else
		{
			lab_Current.Content = "ALL";
			ImageListSource = 2;
		}
		btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex, bufImageCount);
		Mat mat = SwitchImage(0);
		if (mat.Width > 0 && mat.Height > 0)
		{
			Inspect(mat);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Mat SwitchImage(int value)
	{
		Mat result = new Mat();
		try
		{
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			case 0:
				return result;
			case 1:
				if (buffilelist == null)
				{
					return result;
				}
				if (buffilelist.Length == 0)
				{
					return result;
				}
				ImageIndex = buffileIndex + value;
				if (ImageIndex < 0)
				{
					if (value < 0)
					{
						ImageIndex = buffilelist.Length - 1;
					}
					else
					{
						ImageIndex = 0;
					}
				}
				if (ImageIndex >= buffilelist.Length)
				{
					if (value >= 1)
					{
						ImageIndex = 0;
					}
					else
					{
						ImageIndex = buffilelist.Length - 1;
					}
				}
				buffileIndex = ImageIndex;
				bufImageCount = buffilelist.Length;
				btn_ImageNum.Content = string.Format("{0} / {1}", buffileIndex + 1, buffilelist.Length);
				mat = ImRead(buffilelist[buffileIndex], ImreadModes.Color);
				Station.SetBuffers(mat);
				break;
			case 2:
				if (RecentBufferIndex < 0)
				{
					return result;
				}
				if (Station.RecentBufferList.Count <= 0)
				{
					return result;
				}
				ImageIndex = RecentBufferIndex + value;
				if (ImageIndex < 0)
				{
					if (value < 0)
					{
						ImageIndex = Station.RecentBufferList.Count - 1;
					}
					else
					{
						ImageIndex = 0;
					}
				}
				if (ImageIndex >= Station.RecentBufferList.Count)
				{
					if (value >= 1)
					{
						ImageIndex = 0;
					}
					else
					{
						ImageIndex = Station.RecentBufferList.Count - 1;
					}
				}
				RecentBufferIndex = ImageIndex;
				bufImageCount = Station.RecentBufferList.Count;
				btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, Station.RecentBufferList.Count);
				Station.SetBuffers(Station.RecentBufferList[RecentBufferIndex], Station.buffers.SrcRect.Width, Station.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (OKBufferIndex < 0)
				{
					return result;
				}
				if (Station.OKBufferList.Count <= 0)
				{
					return result;
				}
				ImageIndex = OKBufferIndex + value;
				if (ImageIndex < 0)
				{
					if (value < 0)
					{
						ImageIndex = Station.OKBufferList.Count - 1;
					}
					else
					{
						ImageIndex = 0;
					}
				}
				if (ImageIndex >= Station.OKBufferList.Count)
				{
					if (value >= 1)
					{
						ImageIndex = 0;
					}
					else
					{
						ImageIndex = Station.OKBufferList.Count - 1;
					}
				}
				OKBufferIndex = ImageIndex;
				bufImageCount = Station.OKBufferList.Count;
				btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, Station.OKBufferList.Count);
				Station.SetBuffers(Station.OKBufferList[OKBufferIndex], Station.buffers.SrcRect.Width, Station.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (NGBufferIndex < 0)
				{
					return result;
				}
				if (Station.NGBufferList.Count <= 0)
				{
					return result;
				}
				ImageIndex = NGBufferIndex + value;
				if (ImageIndex < 0)
				{
					if (value < 0)
					{
						ImageIndex = Station.NGBufferList.Count - 1;
					}
					else
					{
						ImageIndex = 0;
					}
				}
				if (ImageIndex >= Station.NGBufferList.Count)
				{
					if (value >= 1)
					{
						ImageIndex = 0;
					}
					else
					{
						ImageIndex = Station.NGBufferList.Count - 1;
					}
				}
				NGBufferIndex = ImageIndex;
				bufImageCount = Station.NGBufferList.Count;
				btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, Station.NGBufferList.Count);
				Station.SetBuffers(Station.NGBufferList[NGBufferIndex], Station.buffers.SrcRect.Width, Station.buffers.SrcRect.Height, 3);
				break;
			case 5:
				if (dropfilelist == null)
				{
					return result;
				}
				if (dropfilelist.Length == 0)
				{
					return result;
				}
				ImageIndex = dropfileIndex + value;
				if (ImageIndex < 0)
				{
					if (value < 0)
					{
						ImageIndex = dropfilelist.Length - 1;
					}
					else
					{
						ImageIndex = 0;
					}
				}
				if (ImageIndex >= dropfilelist.Length)
				{
					if (value >= 1)
					{
						ImageIndex = 0;
					}
					else
					{
						ImageIndex = dropfilelist.Length - 1;
					}
				}
				dropfileIndex = ImageIndex;
				bufImageCount = dropfilelist.Length;
				btn_ImageNum.Content = string.Format("{0} / {1}", dropfileIndex + 1, dropfilelist.Length);
				result = ImRead(dropfilelist[dropfileIndex], ImreadModes.Color);
				break;
			default:
				return result;
			}
			if (ImageListSource != 5)
			{
				Station.Inspect(ref mat);
				result = Station.GetAIMat();
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreviewWindow-Btn_NextImage_Click()-01-" + ex.ToString());
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetEnhancePara_Click(object sender, RoutedEventArgs e)
	{
		if (chk_Enhance.IsChecked != false && MessageBox.Show("此功能會將目前的增強數值寫入前處理的C4(R)、C5(G)、C6(B)!(僅支援RGB色階及明度調整) 是否同意?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			if (nu_Red.Value != 1.0)
			{
				Station.paramFile.EnhanceParam[3].BrightnessValue = nu_Red.Value.Value;
				Station.paramFile.EnhanceParam[3].ChannelIndex = 0;
			}
			else
			{
				Station.paramFile.EnhanceParam[3].BrightnessValue = 1.0;
			}
			if (nu_Green.Value != 1.0)
			{
				Station.paramFile.EnhanceParam[4].BrightnessValue = nu_Green.Value.Value;
				Station.paramFile.EnhanceParam[4].ChannelIndex = 1;
			}
			else
			{
				Station.paramFile.EnhanceParam[4].BrightnessValue = 1.0;
			}
			if (nu_Blue.Value != 1.0)
			{
				Station.paramFile.EnhanceParam[5].BrightnessValue = nu_Blue.Value.Value;
				Station.paramFile.EnhanceParam[5].ChannelIndex = 2;
			}
			else
			{
				Station.paramFile.EnhanceParam[5].BrightnessValue = 1.0;
			}
			if (nu_Val.Value != 1.0)
			{
				Station.paramFile.EnhanceParam[3].BrightnessValue = nu_Val.Value.Value;
				Station.paramFile.EnhanceParam[3].ChannelIndex = 0;
				Station.paramFile.EnhanceParam[4].BrightnessValue = nu_Val.Value.Value;
				Station.paramFile.EnhanceParam[4].ChannelIndex = 1;
				Station.paramFile.EnhanceParam[5].BrightnessValue = nu_Val.Value.Value;
				Station.paramFile.EnhanceParam[5].ChannelIndex = 2;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_PreviewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!UpdateUI)
		{
			return;
		}
		PreviewMode = cb_PreviewMode.SelectedIndex;
		if (MarkImgMode)
		{
			if (filelist != null && filelist.Length != 0)
			{
				if (ImageIndex >= filelist.Length)
				{
					ImageIndex = filelist.Length - 1;
				}
				if (ImageIndex <= 0)
				{
					ImageIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
				Inspect(filelist[ImageIndex]);
			}
		}
		else
		{
			Mat mat = SwitchImage(0);
			if (mat.Width > 0 && mat.Height > 0)
			{
				Inspect(mat);
			}
		}
	}

	private void SortAsFileTime(ref string[] arrFi)
	{
		Array.Sort(arrFi, delegate(string x, string y)
		{
			string text = Path.GetFileName(x);
			string text2 = Path.GetFileName(y);
			if (text.Length > 5)
			{
				text = text.Remove(0, 5);
			}
			if (text2.Length > 5)
			{
				text2 = text2.Remove(0, 5);
			}
			return text.CompareTo(text2);
		});
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_LoadOldModel_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Model files (*.h5d) | *.h5d";
			string initialDirectory = string.Format("{0}Model\\", AppDomain.CurrentDomain.BaseDirectory);
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}
			OldModel = openFileDialog.FileName;
			aiEnhanceTools.ReadModel(OldModel, ReLoadModel: true, 0, AbsolutePath: true);
			lab_OldModel.Content = "AI標註(手動載入模型) - " + Path.GetFileNameWithoutExtension(OldModel);
			if (MarkImgMode)
			{
				if (!(LastFile == ""))
				{
					Inspect(LastFile);
				}
				return;
			}
			Mat mat = SwitchImage(0);
			if (mat.Width > 0 && mat.Height > 0)
			{
				Inspect(mat);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreviewWindow-Btn_LoadOldModel_Click()-01-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_MoveFolder_Click(object sender, RoutedEventArgs e)
	{
		string text = "";
		string text2 = "";
		try
		{
			if (cb_ImageDirMoveTo.SelectedIndex < 0)
			{
				return;
			}
			if (filelist.Length == 0)
			{
				btn_ImageNum.Content = string.Format("{0} / {1}", 0, filelist.Length);
				return;
			}
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			string text3 = filelist[ImageIndex];
			if (File.Exists(text3))
			{
				string text4 = "";
				bool flag = true;
				if (text3.Contains("\\X\\NG\\"))
				{
					text4 = text3.Replace("\\X\\NG\\", "\\Y\\NG\\");
					flag = false;
				}
				else if (text3.Contains("\\X\\OK\\"))
				{
					text4 = text3.Replace("\\X\\OK\\", "\\Y\\OK\\");
				}
				string text5 = "ALL";
				text5 = cb_ImageDirMoveTo.SelectedItem.ToString();
				_ = cb_ImageDir.SelectedItem;
				if (lab_ImageClass.Content.ToString() == text5)
				{
					if (LanguageType == 1)
					{
						MessageBox.Show("所選路徑相同!", "警告");
					}
					else if (LanguageType == 2)
					{
						MessageBox.Show("所选路径相同!", "警告");
					}
					else if (LanguageType == 3)
					{
						MessageBox.Show("選択したパスが同じです!", "警告");
					}
					else
					{
						MessageBox.Show("Selet same path!", "Alert");
					}
					return;
				}
				if (cb_ImageDirMoveTo.SelectedIndex == 0)
				{
					text5 = "ALL";
				}
				string text6 = "";
				string text7 = "";
				if (text5 == "未分類")
				{
					text5 = "ALL";
				}
				if (flag)
				{
					text7 = string.Format("{0}{1}\\{2}", ImageXOKPath, text5, Path.GetFileName(text4));
					string directoryName = Path.GetDirectoryName(text7);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					if (File.Exists(text3))
					{
						File.Move(text3, text7);
					}
					text6 = text7.Replace("\\X\\OK\\", "\\Y\\OK\\");
					string directoryName2 = Path.GetDirectoryName(text6);
					if (!Directory.Exists(directoryName2))
					{
						Directory.CreateDirectory(directoryName2);
					}
					if (File.Exists(text4))
					{
						File.Move(text4, text6);
					}
				}
				else
				{
					text2 = (text7 = string.Format("{0}{1}\\{2}", ImageXNGPath, text5, Path.GetFileName(text4)));
					string directoryName3 = Path.GetDirectoryName(text7);
					if (!Directory.Exists(directoryName3))
					{
						Directory.CreateDirectory(directoryName3);
					}
					if (File.Exists(text3))
					{
						File.Copy(text3, text7, overwrite: true);
						File.Delete(text3);
					}
					text = (text6 = text7.Replace("\\X\\NG\\", "\\Y\\NG\\"));
					string directoryName4 = Path.GetDirectoryName(text6);
					if (!Directory.Exists(directoryName4))
					{
						Directory.CreateDirectory(directoryName4);
					}
					if (File.Exists(text4))
					{
						File.Copy(text4, text6, overwrite: true);
						File.Delete(text4);
					}
				}
			}
			int imageIndex = ImageIndex;
			Cb_ImageDir_SelectionChanged(this, null);
			ImageIndex = imageIndex;
			if (LastSearchALL)
			{
				string[] files = Directory.GetFiles(LastPath, "*.bmp", SearchOption.AllDirectories);
				string[] files2 = Directory.GetFiles(LastPath, "*.png", SearchOption.AllDirectories);
				filelist = files.Concat(files2).ToArray();
			}
			else
			{
				string[] files3 = Directory.GetFiles(LastPath, "*.bmp", SearchOption.TopDirectoryOnly);
				string[] files4 = Directory.GetFiles(LastPath, "*.png", SearchOption.TopDirectoryOnly);
				filelist = files3.Concat(files4).ToArray();
			}
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
			if (filelist.Length != 0)
			{
				Inspect(filelist[ImageIndex]);
				return;
			}
			cnv_Image.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageDraw.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageAI.Background = System.Windows.Media.Brushes.LightGray;
			cnv_ImageAIOld.Background = System.Windows.Media.Brushes.LightGray;
			btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex, filelist.Length);
		}
		catch (Exception ex)
		{
			string text8 = string.Concat("AIP01-AIPreviewWindow-Btn_Delete_Click()" + text2 + " - " + text + " - ", ex.ToString());
			CVBuffer.SaveLog(text8);
			MessageBox.Show(text8, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SaveToNGDB_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("是否將增強後的影像存到NG資料庫!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("当前的AI资料库影像将被删除!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Delete current AI database image ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("強化された画像をNGデータベースに保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes && !(X_NG_Path == "") && !(Y_NG_Path == ""))
		{
			string lastFile = LastFile;
			lab_FileName.Content = Path.GetFileName(lastFile);
			string path = "";
			if (lastFile.Contains("\\X\\NG\\"))
			{
				path = lastFile.Replace("\\X\\NG\\", "\\Y\\NG\\");
			}
			else if (lastFile.Contains("\\X\\OK\\"))
			{
				path = lastFile.Replace("\\X\\OK\\", "\\Y\\OK\\");
			}
			DateTime now = DateTime.Now;
			string destFileName = string.Format("{0}\\{1}-{2}.png", Path.GetDirectoryName(lastFile), Path.GetFileNameWithoutExtension(lastFile), now.ToString("mm-ss"));
			string destFileName2 = string.Format("{0}\\{1}-{2}.png", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(lastFile), now.ToString("mm-ss"));
			string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "x.png");
			string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "y.png");
			if (EnhanceMat.Width > 0 && EnhanceMat.Height > 0)
			{
				EnhanceMat.SaveImage(text);
				File.Move(text, destFileName);
			}
			if (DrawingMat.Width > 0 && DrawingMat.Height > 0)
			{
				DrawingMat.SaveImage(text2);
				File.Move(text2, destFileName2);
			}
			int imageIndex = ImageIndex;
			Cb_ImageDir_SelectionChanged(this, null);
			ImageIndex = imageIndex;
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
			Inspect(filelist[ImageIndex]);
			if (LanguageType == 1)
			{
				MessageBox.Show("標註影像新增完成!", "注意");
			}
			else if (LanguageType == 2)
			{
				MessageBox.Show("标注影像新增完成!", "注意");
			}
			else if (LanguageType == 3)
			{
				MessageBox.Show("マーキングされた画像の追加が完了しました!", "注意");
			}
			else
			{
				MessageBox.Show("Add Marking Image to DataBase!", "Alert");
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImageNum_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (MarkImgMode)
			{
				if (filelist == null)
				{
					return;
				}
				System.Windows.Point point = new System.Windows.Point(btn_ImageNum.Margin.Left, btn_ImageNum.Margin.Top);
				HKeyboard hKeyboard = new HKeyboard();
				string title = string.Format("1~{0}", filelist.Length);
				hKeyboard.SetTitle(title);
				hKeyboard.SetInput("");
				hKeyboard.SetInputNegative(state: true);
				hKeyboard.SetDouble(state: false);
				hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
				hKeyboard.Left = point.X;
				hKeyboard.Top = 450.0;
				hKeyboard.ShowDialog();
				if (hKeyboard.GetInput() == "")
				{
					return;
				}
				int num = Convert.ToInt32(hKeyboard.GetInput());
				num--;
				if (num >= 0 && num < filelist.Length && filelist.Length != 0)
				{
					ImageIndex = num;
					if (ImageIndex >= filelist.Length)
					{
						ImageIndex = filelist.Length - 1;
					}
					if (ImageIndex <= 0)
					{
						ImageIndex = 0;
					}
					btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
					Inspect(filelist[ImageIndex]);
				}
				return;
			}
			int num2 = 0;
			switch (ImageListSource)
			{
			default:
				return;
			case 0:
				return;
			case 1:
				if (buffilelist == null || buffilelist.Length == 0)
				{
					return;
				}
				num2 = buffilelist.Length;
				break;
			case 2:
				if (RecentBufferIndex < 0 || Station.RecentBufferList.Count <= 0)
				{
					return;
				}
				num2 = Station.RecentBufferList.Count;
				break;
			case 3:
				if (OKBufferIndex < 0 || Station.OKBufferList.Count <= 0)
				{
					return;
				}
				num2 = Station.OKBufferList.Count;
				break;
			case 4:
				if (NGBufferIndex < 0 || Station.NGBufferList.Count <= 0)
				{
					return;
				}
				num2 = Station.NGBufferList.Count;
				break;
			case 5:
				if (dropfilelist == null || dropfilelist.Length == 0)
				{
					return;
				}
				num2 = dropfilelist.Length;
				break;
			}
			System.Windows.Point point2 = new System.Windows.Point(btn_ImageNum.Margin.Left, btn_ImageNum.Margin.Top);
			HKeyboard hKeyboard2 = new HKeyboard();
			string title2 = string.Format("1~{0}", num2);
			hKeyboard2.SetTitle(title2);
			hKeyboard2.SetInput("");
			hKeyboard2.SetInputNegative(state: true);
			hKeyboard2.SetDouble(state: false);
			hKeyboard2.WindowStartupLocation = WindowStartupLocation.Manual;
			hKeyboard2.Left = point2.X;
			hKeyboard2.Top = 450.0;
			hKeyboard2.ShowDialog();
			if (hKeyboard2.GetInput() == "")
			{
				return;
			}
			int num3 = Convert.ToInt32(hKeyboard2.GetInput());
			num3--;
			if (num3 >= 0 && num3 < num2 && num2 > 0)
			{
				int value = num3 - ImageIndex;
				Mat mat = SwitchImage(value);
				if (mat.Width > 0 && mat.Height > 0)
				{
					Inspect(mat);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreviewWindow-Btn_ImageNum_Click()-01-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AutoFixIgnore(string imagefile)
	{
		try
		{
			string text = imagefile.Replace("\\X\\NG\\", "\\Y\\NG\\");
			if (File.Exists(text))
			{
				Mat mat = ImRead(text, ImreadModes.Color);
				Mat mat2 = new Mat();
				Cv2.CvtColor(mat, mat2, ColorConversionCodes.BGR2GRAY);
				Cv2.InRange(mat2, 25.0, 255.0, mat2);
				if (mat2.CountNonZero() != 0)
				{
					int num = 20;
					Mat mat3 = mat.Clone();
					mat3.SetTo(Scalar.FromRgb(num, num, num));
					Mat mat4 = new Mat();
					Cv2.Dilate(mat2, mat4, new Mat(), null, Station.paramFile.AutoIgnoreSize);
					Mat mat5 = mat.Clone();
					mat5.SetTo(0.0);
					mat3.CopyTo(mat5, mat4);
					mat.CopyTo(mat5, mat2);
					mat5.SaveImage(text);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreviewWindow-AutoFixIgnore()-01-" + ex.ToString());
		}
	}

	private void Btn_FixIgnore_Click(object sender, RoutedEventArgs e)
	{
		if (ImageIndex >= filelist.Length)
		{
			ImageIndex = filelist.Length - 1;
		}
		if (ImageIndex <= 0)
		{
			ImageIndex = 0;
		}
		string imagefile = filelist[ImageIndex];
		AutoFixIgnore(imagefile);
		Inspect(LastFile);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_FixAllIgnore_Click(object sender, RoutedEventArgs e)
	{
		if (!UseNGImage)
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("當前所有NG影像的關注區域會被重新修正，是否確定!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("当前所有NG影像的关注区域会被重新修正，是否确定!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Reset all Ignore Area of image?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("現在のすべてのNG画像の注目領域が再修正されます。よろしいですか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			for (int i = 0; i < filelist.Length; i++)
			{
				AutoFixIgnore(filelist[i]);
			}
			Inspect(LastFile);
		}
	}

	private void btn_ChangeMarkColor_Click(object sender, RoutedEventArgs e)
	{
		if (cb_MarkColorBefore.SelectedIndex != cb_MarkColorAfter.SelectedIndex)
		{
			if (ImageIndex >= filelist.Length)
			{
				ImageIndex = filelist.Length - 1;
			}
			if (ImageIndex <= 0)
			{
				ImageIndex = 0;
			}
			string imagefile = filelist[ImageIndex];
			ChangeMarkColor(imagefile, cb_MarkColorBefore.SelectedIndex, cb_MarkColorAfter.SelectedIndex);
			Inspect(LastFile);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ChangeMarkColor(string imagefile, int beforeChannel, int afterChannel)
	{
		try
		{
			string text = imagefile.Replace("\\X\\NG\\", "\\Y\\NG\\");
			if (!File.Exists(text))
			{
				return;
			}
			_ = new Mat[3];
			_ = new Mat[3];
			Mat mat = ImRead(text, ImreadModes.Color);
			int[] array = new int[3];
			switch (beforeChannel)
			{
			case 0:
				array[0] = (array[1] = 0);
				array[2] = 1;
				break;
			case 1:
				array[0] = (array[2] = 0);
				array[1] = 1;
				break;
			case 2:
				array[2] = (array[1] = 0);
				array[0] = 1;
				break;
			}
			Scalar lowerb = new Scalar(160 * array[0], 160 * array[1], 160 * array[2]);
			Scalar upperb = new Scalar(255 * array[0], 255 * array[1], 255 * array[2]);
			using Mat mat2 = new Mat();
			Cv2.InRange(mat, lowerb, upperb, mat2);
			switch (afterChannel)
			{
			case 0:
				array[0] = (array[1] = 0);
				array[2] = 1;
				break;
			case 1:
				array[0] = (array[2] = 0);
				array[1] = 1;
				break;
			case 2:
				array[2] = (array[1] = 0);
				array[0] = 1;
				break;
			}
			Scalar value = new Scalar(255 * array[0], 255 * array[1], 255 * array[2]);
			mat.SetTo(value, mat2);
			mat.SaveImage(text);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreviewWindow-AutoFixIgnore()-01-" + ex.ToString());
		}
	}

	private void chk_RotatePreview_Click(object sender, RoutedEventArgs e)
	{
		if (chk_RotatePreview.IsChecked == true)
		{
			chk_Enhance.IsChecked = false;
			chk_Enhance.IsEnabled = false;
			grd_RotatePreview.Visibility = Visibility.Visible;
		}
		else
		{
			grd_RotatePreview.Visibility = Visibility.Hidden;
			chk_Enhance.IsEnabled = true;
		}
		ReInspect();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_SaveToNGDB_H_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		if (X_NG_Path == "" || Y_NG_Path == "")
		{
			return;
		}
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("是否將增強後的影像存到NG資料庫!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("是否将增强后的影像存到NG资料库!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Save Enhanec Image to DataBase?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("強化された画像をNGデータベースに保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		string lastFile = LastFile;
		lab_FileName.Content = Path.GetFileName(lastFile);
		string path = "";
		if (lastFile.Contains("\\X\\NG\\"))
		{
			path = lastFile.Replace("\\X\\NG\\", "\\Y\\NG\\");
		}
		else if (lastFile.Contains("\\X\\OK\\"))
		{
			path = lastFile.Replace("\\X\\OK\\", "\\Y\\OK\\");
		}
		string destFileName = string.Format("{0}\\{1}-{2}.png", Path.GetDirectoryName(lastFile), Path.GetFileNameWithoutExtension(lastFile), button.Uid);
		string destFileName2 = string.Format("{0}\\{1}-{2}.png", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), button.Uid);
		string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "x.png");
		string text2 = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "y.png");
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		if (File.Exists(text2))
		{
			File.Delete(text2);
		}
		if (button.Uid == "H")
		{
			if (EnhanceMatH.Width > 0 && EnhanceMatH.Height > 0)
			{
				EnhanceMatH.SaveImage(text);
				File.Copy(text, destFileName, overwrite: true);
			}
			if (DrawingMatH.Width > 0 && DrawingMatH.Height > 0)
			{
				DrawingMatH.SaveImage(text2);
				File.Copy(text2, destFileName2, overwrite: true);
			}
		}
		else if (button.Uid == "V")
		{
			if (EnhanceMatV.Width > 0 && EnhanceMatV.Height > 0)
			{
				EnhanceMatV.SaveImage(text);
				File.Copy(text, destFileName, overwrite: true);
			}
			if (DrawingMatV.Width > 0 && DrawingMatV.Height > 0)
			{
				DrawingMatV.SaveImage(text2);
				File.Copy(text2, destFileName2, overwrite: true);
			}
		}
		else
		{
			if (EnhanceMatVH.Width > 0 && EnhanceMatVH.Height > 0)
			{
				EnhanceMatVH.SaveImage(text);
				File.Copy(text, destFileName, overwrite: true);
			}
			if (DrawingMatVH.Width > 0 && DrawingMatVH.Height > 0)
			{
				DrawingMatVH.SaveImage(text2);
				File.Copy(text2, destFileName2, overwrite: true);
			}
		}
		int imageIndex = ImageIndex;
		Cb_ImageDir_SelectionChanged(this, null);
		ImageIndex = imageIndex;
		if (ImageIndex >= filelist.Length)
		{
			ImageIndex = filelist.Length - 1;
		}
		if (ImageIndex <= 0)
		{
			ImageIndex = 0;
		}
		btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, filelist.Length);
		Inspect(filelist[ImageIndex]);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ReInspect()
	{
		try
		{
			if (MarkImgMode)
			{
				if (filelist != null && filelist.Length != 0)
				{
					if (ImageIndex >= filelist.Length)
					{
						ImageIndex = filelist.Length - 1;
					}
					if (ImageIndex <= 0)
					{
						ImageIndex = 0;
					}
					Inspect(filelist[ImageIndex]);
				}
			}
			else if (ImageListSource != 0)
			{
				Mat mat = SwitchImage(0);
				if (mat.Width > 0 && mat.Height > 0)
				{
					Inspect(mat);
				}
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIPreview-ReInspect()-01-" + ex.ToString());
		}
	}

	private void btn_RotateInspectAlert_Click(object sender, RoutedEventArgs e)
	{
		chk_RotatePreview.IsChecked = true;
		grd_RotatePreview.Visibility = Visibility.Visible;
		ReInspect();
	}

	private void btn_ClosePreview_Click(object sender, RoutedEventArgs e)
	{
		chk_RotatePreview.IsChecked = false;
		grd_RotatePreview.Visibility = Visibility.Hidden;
		ReInspect();
	}

	private void wnd_Main_Closed(object sender, EventArgs e)
	{
	}

	private void chk_ShowOldModelPreview_Click(object sender, RoutedEventArgs e)
	{
		if (chk_ShowOldModelPreview.IsChecked == true)
		{
			cnv_ImageAIOld.Visibility = Visibility.Visible;
			chk_ShowOldModelPreview.Visibility = Visibility.Hidden;
		}
		else
		{
			cnv_ImageAIOld.Visibility = Visibility.Hidden;
		}
	}

	private void btn_GerNG_Click(object sender, RoutedEventArgs e)
	{
		NGCloneWindow nGCloneWindow = new NGCloneWindow();
		nGCloneWindow.LoadLanguage(LanguageType);
		nGCloneWindow.SetInspection(Station);
		nGCloneWindow.setThroughputParamFile(ThroughputFile);
		nGCloneWindow.SetSAM(mSam);
		nGCloneWindow.SetBuffer(AIMatBuf);
		nGCloneWindow.ShowDialog();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/aipreviewwindow.xaml", UriKind.Relative);
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
			wnd_Main = (AIPreviewWindow)target;
			wnd_Main.Drop += Wnd_Main_Drop;
			wnd_Main.MouseWheel += Wnd_Main_MouseWheel;
			wnd_Main.Loaded += Wnd_Main_Loaded;
			wnd_Main.Closed += wnd_Main_Closed;
			break;
		case 2:
			cnv_Image = (Canvas)target;
			break;
		case 3:
			sfr = (ScaleTransform)target;
			break;
		case 4:
			btn_OKImage = (Button)target;
			btn_OKImage.Click += Btn_OKImage_Click;
			break;
		case 5:
			btn_NGImage = (Button)target;
			btn_NGImage.Click += Btn_NGImage_Click;
			break;
		case 6:
			btn_ImageNum = (Button)target;
			btn_ImageNum.Click += Btn_ImageNum_Click;
			break;
		case 7:
			btn_PrevImage = (Button)target;
			btn_PrevImage.Click += Btn_PrevImage_Click;
			break;
		case 8:
			btn_NextImage = (Button)target;
			btn_NextImage.Click += Btn_NextImage_Click;
			break;
		case 9:
			lab_FileName = (Label)target;
			break;
		case 10:
			btn_Delete = (Button)target;
			btn_Delete.Click += Btn_Delete_Click;
			break;
		case 11:
			cb_ImageDir = (ComboBox)target;
			cb_ImageDir.SelectionChanged += Cb_ImageDir_SelectionChanged;
			break;
		case 12:
			btn_BufferImgALL = (Button)target;
			btn_BufferImgALL.Click += Btn_BufferImgALL_Click;
			break;
		case 13:
			btn_BufferImgOK = (Button)target;
			btn_BufferImgOK.Click += Btn_BufferImgALL_Click;
			break;
		case 14:
			btn_BufferImgNG = (Button)target;
			btn_BufferImgNG.Click += Btn_BufferImgALL_Click;
			break;
		case 15:
			btn_BufferImgFile = (Button)target;
			btn_BufferImgFile.Click += Btn_BufferImgALL_Click;
			break;
		case 16:
			lab_Current = (Label)target;
			break;
		case 17:
			cb_PreviewMode = (ComboBox)target;
			cb_PreviewMode.SelectionChanged += Cb_PreviewMode_SelectionChanged;
			break;
		case 18:
			btn_MoveFolder = (Button)target;
			btn_MoveFolder.Click += Btn_MoveFolder_Click;
			break;
		case 19:
			btn_FixAllIgnore = (Button)target;
			btn_FixAllIgnore.Click += Btn_FixAllIgnore_Click;
			break;
		case 20:
			chk_RotatePreview = (CheckBox)target;
			chk_RotatePreview.Click += chk_RotatePreview_Click;
			break;
		case 21:
			cb_ImageDirMoveTo = (ComboBox)target;
			break;
		case 22:
			lab_GerNG = (Label)target;
			break;
		case 23:
			cnv_ImageAIOld = (Canvas)target;
			break;
		case 24:
			sfr1 = (ScaleTransform)target;
			break;
		case 25:
			lab_OldModel = (Label)target;
			break;
		case 26:
			btn_ReDraw1_Copy = (Button)target;
			break;
		case 27:
			btn_ReDraw1_Copy1 = (Button)target;
			break;
		case 28:
			btn_ReDraw1_Copy2 = (Button)target;
			break;
		case 29:
			btn_ReDraw1_Copy3 = (Button)target;
			break;
		case 30:
			btn_NewModel1 = (Button)target;
			btn_NewModel1.Click += Btn_NewModel_Click;
			break;
		case 31:
			btn_OldModel1 = (Button)target;
			btn_OldModel1.Click += Btn_OldModel_Click;
			break;
		case 32:
			btn_ImageNum1 = (Button)target;
			break;
		case 33:
			btn_PrevImage1 = (Button)target;
			btn_PrevImage1.Click += Btn_PrevImage_Click;
			break;
		case 34:
			btn_NextImage1 = (Button)target;
			btn_NextImage1.Click += Btn_NextImage_Click;
			break;
		case 35:
			cnv_ImageAI = (Canvas)target;
			break;
		case 36:
			sfr2 = (ScaleTransform)target;
			break;
		case 37:
			lab_NewModel = (Label)target;
			break;
		case 38:
			cnv_ImageDraw = (Canvas)target;
			break;
		case 39:
			sfr3 = (ScaleTransform)target;
			break;
		case 40:
			lab_FileNotExist = (Label)target;
			break;
		case 41:
			btn_Redraw = (Button)target;
			btn_Redraw.Click += Btn_Redraw_Click;
			break;
		case 42:
			grd_Enhance = (Grid)target;
			break;
		case 43:
			chk_Enhance = (CheckBox)target;
			chk_Enhance.Click += Chk_EnableRotate_Click;
			break;
		case 44:
			chk_EnableVMirror = (CheckBox)target;
			chk_EnableVMirror.Click += Chk_EnableRotate_Click;
			break;
		case 45:
			chk_EnableHMirror = (CheckBox)target;
			chk_EnableHMirror.Click += Chk_EnableRotate_Click;
			break;
		case 46:
			btn_SetPara = (Button)target;
			btn_SetPara.Click += Btn_SetPara_Click;
			break;
		case 47:
			btn_SetEnhancePara = (Button)target;
			btn_SetEnhancePara.Click += Btn_SetEnhancePara_Click;
			break;
		case 48:
			nu_Hue = (NumericUpDown)target;
			nu_Hue.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 49:
			nu_Sat = (NumericUpDown)target;
			nu_Sat.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 50:
			nu_Val = (NumericUpDown)target;
			nu_Val.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 51:
			nu_Red = (NumericUpDown)target;
			nu_Red.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 52:
			nu_Green = (NumericUpDown)target;
			nu_Green.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 53:
			nu_Blue = (NumericUpDown)target;
			nu_Blue.ValueChanged += Nu_Red_ValueChanged;
			break;
		case 54:
			btn_SaveToNGDB = (Button)target;
			btn_SaveToNGDB.Click += Btn_SaveToNGDB_Click;
			break;
		case 55:
			btn_LoadOldModel = (Button)target;
			btn_LoadOldModel.Click += Btn_LoadOldModel_Click;
			break;
		case 56:
			btn_FixIgnore = (Button)target;
			btn_FixIgnore.Click += Btn_FixIgnore_Click;
			break;
		case 57:
			cb_MarkColorBefore = (ComboBox)target;
			cb_MarkColorBefore.SelectionChanged += Cb_PreviewMode_SelectionChanged;
			break;
		case 58:
			cb_MarkColorAfter = (ComboBox)target;
			cb_MarkColorAfter.SelectionChanged += Cb_PreviewMode_SelectionChanged;
			break;
		case 59:
			btn_ChangeMarkColor = (Button)target;
			btn_ChangeMarkColor.Click += btn_ChangeMarkColor_Click;
			break;
		case 60:
			btn_RotateInspectAlert = (Button)target;
			btn_RotateInspectAlert.Click += btn_RotateInspectAlert_Click;
			break;
		case 61:
			lab_ImageClass = (Label)target;
			break;
		case 62:
			chk_ShowOldModelPreview = (CheckBox)target;
			chk_ShowOldModelPreview.Click += chk_ShowOldModelPreview_Click;
			break;
		case 63:
			grd_RotatePreview = (Grid)target;
			break;
		case 64:
			cnv_ImageAI2 = (Canvas)target;
			break;
		case 65:
			sfr4 = (ScaleTransform)target;
			break;
		case 66:
			cnv_ImageV = (Canvas)target;
			break;
		case 67:
			sfr5 = (ScaleTransform)target;
			break;
		case 68:
			cnv_ImageH = (Canvas)target;
			break;
		case 69:
			sfr6 = (ScaleTransform)target;
			break;
		case 70:
			cnv_ImageVH = (Canvas)target;
			break;
		case 71:
			sfr7 = (ScaleTransform)target;
			break;
		case 72:
			ico_ResultAlartH = (Label)target;
			break;
		case 73:
			ico_ResultAlartVH = (Label)target;
			break;
		case 74:
			ico_ResultAlartV = (Label)target;
			break;
		case 75:
			btn_SaveToNGDB_H = (Button)target;
			btn_SaveToNGDB_H.Click += btn_SaveToNGDB_H_Click;
			break;
		case 76:
			btn_SaveToNGDB_VH = (Button)target;
			btn_SaveToNGDB_VH.Click += btn_SaveToNGDB_H_Click;
			break;
		case 77:
			btn_SaveToNGDB_V = (Button)target;
			btn_SaveToNGDB_V.Click += btn_SaveToNGDB_H_Click;
			break;
		case 78:
			btn_ClosePreview = (Button)target;
			btn_ClosePreview.Click += btn_ClosePreview_Click;
			break;
		case 79:
			ico_ResultAlartH_Copy = (Label)target;
			break;
		case 80:
			ico_ResultAlartH_Copy1 = (Label)target;
			break;
		case 81:
			ico_ResultAlartH_Copy2 = (Label)target;
			break;
		case 82:
			ico_ResultAlartH_Copy3 = (Label)target;
			break;
		case 83:
			btn_GerNG = (Button)target;
			btn_GerNG.Click += btn_GerNG_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
