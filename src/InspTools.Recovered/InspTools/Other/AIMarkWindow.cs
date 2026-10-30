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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SAMViewer;

namespace InspTools.Other;

public class AIMarkWindow : System.Windows.Window, IComponentConnector
{
	private const int MaxMarkCount = 8;

	private const int MaxNGMarkCount = 12;

	private const int MaxIgnoreCount = 4;

	private const int IgnoreColor = 20;

	private const int MaybeColor = 125;

	private int AutoDrawExtend = 1;

	private bool AutoDrawByRect;

	private int x0;

	private int y0;

	private int x1;

	private int y1;

	private int x01;

	private int y01;

	private double xRatio;

	private double yRatio;

	private Mat OrgBitmap = new Mat();

	private Mat SamBitmap = new Mat();

	public Mat OrgImage = new Mat();

	public Mat FinalMat = new Mat();

	private Mat AutoDrawMat = new Mat();

	private Mat AutoDrawMask = new Mat();

	private bool AutoDrawRun;

	private Mat DrawingMat = new Mat();

	private Mat DrawingMat2 = new Mat();

	private Mat IgnoreMat = new Mat();

	private Mat[] OrgBitmapBGR = new Mat[3];

	private Mat Binary = new Mat();

	private int StationIndex;

	private int LanguageType = 1;

	private int ColorSelect;

	private int PreviewMode;

	private int ZoomMode;

	private int linewidth = 6;

	private string NGMarkedFolder = "";

	private string NGFolder = "";

	private int CurrentClient;

	private int CurrentTrack = -1;

	private System.Windows.Media.Color DrawColor = Colors.Red;

	private System.Windows.Point renderedLocation;

	private DrawingAttributes drawingAttributes;

	private Button[] TrackBtn;

	private Border[] IgnoreTrack;

	private int[] IgnoreTrackColor = new int[4];

	private bool[] IgnoreTrackON = new bool[4];

	private OpenCvSharp.Rect[] OKTrack = new OpenCvSharp.Rect[8];

	private bool[] OKTrackON = new bool[8];

	private Border[] IgnoreRect;

	private int FunctionIndex;

	private int DNetMode = 1;

	public bool Saved;

	private bool PenMode = true;

	private int PenModeIndex;

	private bool FillHoleStatus;

	private RadioButton[] ColorRadBtn;

	private TextBox[] CaptionText;

	private System.Windows.Shapes.Rectangle[] ColorRect;

	private bool inited;

	private bool SingleReDrawMode;

	private string ReDrawFilePath = "";

	private int ImageListSource;

	private string[] filelist;

	private int fileIndex = -1;

	private int RecentBufferIndex = -1;

	private int OKBufferIndex = -1;

	private int NGBufferIndex = -1;

	private int ImageIndex;

	private int ImageCount;

	public bool CropedMode;

	private bool ContinueWrite;

	private bool EnableAutoDrawIgnore = true;

	private CVInspection inspection;

	private System.Drawing.Point LineStart = new System.Drawing.Point(-1, -1);

	private System.Drawing.Point LineEnd = new System.Drawing.Point(-1, -1);

	private const int MAXCOLOR = 6;

	private SAM mSam;

	private List<Promotion> mPromotionList = new List<Promotion>();

	private float[] mImgEmbedding;

	private RectAnnotation mCurRectAnno;

	private System.Windows.Point _startPoint;

	private int mOrgwid;

	private int mOrghei;

	private Stack<Promotion> mUndoStack = new Stack<Promotion>();

	private Stack<Promotion> mRedoStack = new Stack<Promotion>();

	private Dispatcher UI;

	private SAMAutoMask mAutoMask;

	private MaskData mAutoMaskData;

	private Operation mCurOp;

	private float[] mData;

	private List<float> mDataList = new List<float>();

	private List<float> mDataListSave = new List<float>();

	private System.Windows.Media.Color DrawingColor;

	private int ColorID = 1;

	private System.Windows.Media.Color[] ColorSet = new System.Windows.Media.Color[6];

	private bool ModelLoad;

	private Thread LoadDrawAIThread;

	internal AIMarkWindow wnd_main;

	internal Label lab_Message;

	internal TabControl tab_Main;

	internal TabItem tab_Draw;

	internal Button btn_Clear;

	internal Grid grd_IgnoreTrack;

	internal Button btn_IGN01;

	internal Button btn_IGN02;

	internal Button btn_IGN03;

	internal Button btn_IGN04;

	internal System.Windows.Shapes.Rectangle rt_Red;

	internal System.Windows.Shapes.Rectangle rt_Lime;

	internal System.Windows.Shapes.Rectangle rt_Blue;

	internal System.Windows.Shapes.Rectangle rt_Yellow;

	internal System.Windows.Shapes.Rectangle rt_Fuchsia;

	internal System.Windows.Shapes.Rectangle rt_Aqua;

	internal TextBox txt_Caption1;

	internal TextBox txt_Caption2;

	internal TextBox txt_Caption3;

	internal TextBox txt_Caption4;

	internal TextBox txt_Caption5;

	internal TextBox txt_Caption6;

	internal Label lab_DNet3;

	internal RadioButton rad_Red;

	internal ComboBox cb_PreviewMode;

	internal TextBox txt_AutoIgnoreSize;

	internal Label lab_AutoIgnoreSize;

	internal RadioButton rad_Lime;

	internal RadioButton rad_Blue;

	internal RadioButton rad_Yellow;

	internal RadioButton rad_Fuchsia;

	internal RadioButton rad_Aqua;

	internal CheckBox chk_AutoIgnoreDraw;

	internal Label lab_UsedTool;

	internal Label lab_UsedColor;

	internal TabItem tab_MarkNG;

	internal TabItem tab_MarkOK;

	internal Button btn_AddImage;

	internal Button btn_ImageNum;

	internal Button btn_NextImage;

	internal Button btn_PrevImage;

	internal Grid grd_Ink;

	internal Canvas cnv_Draw;

	internal System.Windows.Controls.Image img_BG;

	internal System.Windows.Controls.Image mMask;

	internal InkCanvas cnv_Ink;

	internal Border bor_RET1;

	internal TextBlock txt_Search1Label;

	internal Border bor_RET2;

	internal TextBlock txt_Search1Labe2;

	internal Border bor_RET3;

	internal TextBlock txt_Search1Labe3;

	internal Border bor_RET4;

	internal TextBlock txt_Search1Labe4;

	internal Ellipse es_LineStart;

	internal Border bor_01;

	internal Border bor_02;

	internal Border bor_03;

	internal Border bor_04;

	internal CheckBox chk_ContinueWrite;

	internal Grid grd_InkDraw;

	internal Label lab_AILoad;

	internal Button btn_PenMode;

	internal Button btn_EraserMode;

	internal Button btn_TrackMode;

	internal Button btn_Line;

	internal Button btn_AutoDraw;

	internal Button btn_Magnify;

	internal Button btn_FillHole;

	internal Grid grd_AIDraw;

	internal Button btn_AddPT;

	internal Button btn_AddAIRect;

	internal Button btn_AIColorEraser;

	internal Button btn_SaveAIDraw;

	internal Button btn_AIUndo;

	internal Button btn_AIReset;

	internal Grid grd_AIDraw_Copy1;

	internal Button btn_AddPT1;

	internal Button btn_MaskErode;

	internal Button btn_AIColorEraser1;

	internal Button btn_SaveAIDraw2;

	internal Button btn_AIUndo1;

	internal Button btn_AIReset1;

	internal Button btn_AIDelete;

	internal Button btn_EyesOff;

	internal Button btn_Opacity;

	internal CheckBox chk_SavebyFolder;

	internal Label lab_classic;

	internal Grid grd_Icons;

	internal Button icon_PenMode;

	internal Button icon_EraserMode;

	internal Button icon_TrackMode;

	internal Button icon_Line;

	internal Grid grd_AIDraw_Copy;

	internal Button icon_AddPT;

	internal Button icon_AddAIRect;

	internal Button icon_AIColorEraser;

	internal Button btn_SaveAIDraw1;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public AIMarkWindow()
	{
		InitializeComponent();
		inited = true;
		TrackBtn = new Button[4] { btn_IGN01, btn_IGN02, btn_IGN03, btn_IGN04 };
		IgnoreTrack = new Border[4] { bor_RET1, bor_RET2, bor_RET3, bor_RET4 };
		IgnoreRect = new Border[4] { bor_01, bor_02, bor_03, bor_04 };
		ColorRadBtn = new RadioButton[6] { rad_Red, rad_Lime, rad_Blue, rad_Yellow, rad_Fuchsia, rad_Aqua };
		ColorRect = new System.Windows.Shapes.Rectangle[6] { rt_Red, rt_Lime, rt_Blue, rt_Yellow, rt_Fuchsia, rt_Aqua };
		CaptionText = new TextBox[6] { txt_Caption1, txt_Caption2, txt_Caption3, txt_Caption4, txt_Caption5, txt_Caption6 };
		for (int i = 0; i < 4; i++)
		{
			IgnoreTrack[i].IsEnabled = true;
			IgnoreTrack[i].Visibility = Visibility.Hidden;
			IgnoreRect[i].Visibility = Visibility.Hidden;
			IgnoreTrackON[i] = false;
			IgnoreTrackColor[i] = 0;
		}
		for (int j = 0; j < 3; j++)
		{
			OrgBitmapBGR[j] = new Mat();
		}
		lab_Message.Opacity = 0.0;
		lab_DNet3.Visibility = Visibility.Hidden;
		btn_PenMode.Background = System.Windows.Media.Brushes.LimeGreen;
		es_LineStart.Visibility = Visibility.Hidden;
		if (!Directory.Exists("D:\\PythonTest"))
		{
			btn_AutoDraw.Visibility = Visibility.Hidden;
		}
		DrawingColor = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, 0, 0);
		mCurOp = Operation.Draw;
		ColorSet[0] = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, 0, 0);
		ColorSet[1] = System.Windows.Media.Color.FromArgb(100, 0, byte.MaxValue, 0);
		ColorSet[2] = System.Windows.Media.Color.FromArgb(100, 0, 0, byte.MaxValue);
		ColorSet[3] = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, byte.MaxValue, 0);
		ColorSet[4] = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, 0, byte.MaxValue);
		ColorSet[5] = System.Windows.Media.Color.FromArgb(100, 0, byte.MaxValue, byte.MaxValue);
		grd_Icons.Visibility = Visibility.Hidden;
		LoadDrawAIThread = new Thread(LoadDrawAIRun);
		grd_AIDraw.Visibility = Visibility.Hidden;
		btn_PrevImage.Visibility = Visibility.Hidden;
		btn_ImageNum.Visibility = Visibility.Hidden;
		btn_NextImage.Visibility = Visibility.Hidden;
		chk_ContinueWrite.Visibility = Visibility.Hidden;
		chk_ContinueWrite.IsChecked = false;
	}

	private void LoadDrawAIRun()
	{
		OpenAIModelAndImage();
		Action method = delegate
		{
			grd_AIDraw.Visibility = Visibility.Visible;
		};
		base.Dispatcher.BeginInvoke(method);
	}

	public void SetOrgImage(Mat mat)
	{
		mat.CopyTo(OrgImage);
	}

	public void SetMat(Mat mat, int zoom)
	{
		ZoomMode = zoom;
		mat.CopyTo(OrgBitmap);
		mat.CopyTo(SamBitmap);
		if (OrgBitmap.Channels() == 3)
		{
			Cv2.Split(OrgBitmap, out OrgBitmapBGR);
		}
		else
		{
			OrgBitmapBGR[0] = OrgBitmap.Clone();
			OrgBitmapBGR[1] = OrgBitmap.Clone();
			OrgBitmapBGR[2] = OrgBitmap.Clone();
		}
		if (mSam != null && CurrentClient != 3)
		{
			if (LoadDrawAIThread.ThreadState == System.Threading.ThreadState.Unstarted)
			{
				LoadDrawAIThread.Start();
			}
		}
		else
		{
			lab_AILoad.Visibility = Visibility.Hidden;
		}
		ZoomWindows();
	}

	public void SetInspection(CVInspection station, int stationIndex)
	{
		inspection = station;
		StationIndex = stationIndex;
		DNetMode = 1;
		tab_MarkNG.Visibility = Visibility.Hidden;
		tab_MarkOK.Visibility = Visibility.Hidden;
		PenMode = true;
		btn_TrackMode.Visibility = Visibility.Visible;
		grd_IgnoreTrack.Visibility = Visibility.Visible;
		chk_AutoIgnoreDraw.Visibility = Visibility.Hidden;
		txt_AutoIgnoreSize.Visibility = Visibility.Hidden;
		lab_AutoIgnoreSize.Visibility = Visibility.Hidden;
		btn_AutoDraw.Visibility = Visibility.Hidden;
		if (inspection.paramFile.ThroughputFile.CurrentClient == 3)
		{
			btn_Opacity.Visibility = Visibility.Hidden;
			btn_FillHole.Visibility = Visibility.Hidden;
			lab_classic.Visibility = Visibility.Hidden;
			chk_SavebyFolder.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetSAM(SAM sam)
	{
		string path = string.Format("{0}{1}", "C:\\Temp\\", "decoder.onnx");
		string path2 = string.Format("{0}{1}", "C:\\Temp\\", "encoder.onnx");
		if (File.Exists(path) && File.Exists(path2))
		{
			mSam = sam;
		}
		else
		{
			lab_AILoad.Visibility = Visibility.Hidden;
		}
	}

	public void SetNGMarkedFolder(string ngpath, string markpath)
	{
		NGFolder = ngpath;
		NGMarkedFolder = markpath;
	}

	public void SetClientCode(int code)
	{
		CurrentClient = code;
		if (CurrentClient == 3)
		{
			grd_AIDraw.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetImageSource(int imagelistsource, string[] flist, int findex, int recentindex, int okindex, int ngindex)
	{
		try
		{
			ImageListSource = imagelistsource;
			filelist = flist;
			fileIndex = findex;
			RecentBufferIndex = recentindex;
			OKBufferIndex = okindex;
			NGBufferIndex = ngindex;
			switch (ImageListSource)
			{
			case 0:
				btn_ImageNum.Content = string.Format("1 / 1");
				ImageIndex = (ImageCount = 1);
				break;
			case 1:
				if (filelist != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
					ImageIndex = fileIndex + 1;
					ImageCount = filelist.Length;
				}
				break;
			case 2:
				if (inspection.RecentBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
					ImageIndex = RecentBufferIndex + 1;
					ImageCount = inspection.RecentBufferList.Count;
				}
				break;
			case 3:
				if (inspection.OKBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
					ImageIndex = OKBufferIndex + 1;
					ImageCount = inspection.OKBufferList.Count;
				}
				break;
			case 4:
				if (inspection.NGBufferList != null)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
					ImageIndex = NGBufferIndex + 1;
					ImageCount = inspection.NGBufferList.Count;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIMarkWindow-SetImageSource()-01-" + ex.ToString());
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

	public void SetSingleRedraw(string path)
	{
		SingleReDrawMode = true;
		ReDrawFilePath = path;
		chk_ContinueWrite.Visibility = Visibility.Hidden;
	}

	private void ZoomWindows()
	{
		Mat orgBitmap = OrgBitmap;
		double num = 0.0;
		double num2 = 0.0;
		if (ZoomMode == 0)
		{
			if (orgBitmap.Width > orgBitmap.Height)
			{
				xRatio = 600.0 / (double)orgBitmap.Width;
				yRatio = xRatio;
			}
			else
			{
				yRatio = 600.0 / (double)orgBitmap.Height;
				xRatio = yRatio;
			}
		}
		else
		{
			double num3 = 1200.0 / (double)orgBitmap.Width;
			double num4 = 700.0 / (double)orgBitmap.Height;
			if (num3 < num4)
			{
				xRatio = num3;
				yRatio = num3;
			}
			else
			{
				xRatio = num4;
				yRatio = num4;
			}
		}
		num = (double)orgBitmap.Width * xRatio + 100.0;
		num2 = (double)orgBitmap.Height * yRatio + 300.0;
		base.Width = num;
		base.Height = num2;
		base.Left = (1920.0 - num) / 2.0;
		base.Top = (1080.0 - num2) / 2.0;
		grd_Ink.SetValue(FrameworkElement.WidthProperty, (double)orgBitmap.Width * xRatio);
		cnv_Ink.SetValue(FrameworkElement.WidthProperty, (double)orgBitmap.Width * xRatio);
		img_BG.SetValue(FrameworkElement.WidthProperty, (double)orgBitmap.Width * xRatio);
		grd_Ink.SetValue(FrameworkElement.HeightProperty, (double)orgBitmap.Height * yRatio);
		cnv_Ink.SetValue(FrameworkElement.HeightProperty, (double)orgBitmap.Height * yRatio);
		img_BG.SetValue(FrameworkElement.HeightProperty, (double)orgBitmap.Height * yRatio);
		ImageSource imageSource = OrgBitmap.ToBitmapSource();
		imageSource = ((PreviewMode <= 0) ? OrgBitmap.ToBitmapSource() : (PreviewMode switch
		{
			1 => OrgBitmapBGR[2].ToBitmapSource(), 
			2 => OrgBitmapBGR[1].ToBitmapSource(), 
			3 => OrgBitmapBGR[0].ToBitmapSource(), 
			_ => OrgBitmapBGR[2].ToBitmapSource(), 
		}));
		mMask.SetValue(FrameworkElement.WidthProperty, (double)orgBitmap.Width * xRatio);
		mMask.SetValue(FrameworkElement.HeightProperty, (double)orgBitmap.Height * yRatio);
		new ImageBrush().ImageSource = imageSource;
		img_BG.Source = imageSource;
	}

	private void Btn_Clear_Click(object sender, RoutedEventArgs e)
	{
		ResetAndDelete();
		DrawingMat.SetTo(0.0);
		DrawingMat.CopyTo(FinalMat);
		cnv_Ink.Children.Clear();
		while (cnv_Ink.Strokes.Count > 0)
		{
			cnv_Ink.Strokes.RemoveAt(cnv_Ink.Strokes.Count - 1);
		}
		ImageSource imageSource = OrgBitmap.ToBitmapSource();
		new ImageBrush().ImageSource = imageSource;
		img_BG.Source = imageSource;
	}

	private void SortAsFileTime(ref string[] arrFi)
	{
		Array.Sort(arrFi, delegate(string x, string y)
		{
			string text = System.IO.Path.GetFileName(x);
			string text2 = System.IO.Path.GetFileName(y);
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
						System.IO.Path.GetFileName(list3[l]);
						System.IO.Path.GetFileName(list3[m]);
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
			return result;
		}
		catch (Exception ex)
		{
			string text3 = "AIP01-AIMarkWindow-FindSameImage()" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, LanguageManager.Instance["Error"]);
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void StartFindSameImage()
	{
		try
		{
			string[] files = Directory.GetFiles(NGFolder, "*.bmp", SearchOption.AllDirectories);
			string[] files2 = Directory.GetFiles(NGFolder, "*.png", SearchOption.AllDirectories);
			string[] arrFi = files.Concat(files2).ToArray();
			SortAsFileTime(ref arrFi);
			FindSameImage(arrFi);
		}
		catch
		{
		}
	}

	private void Btn_AddImage_Click(object sender, RoutedEventArgs e)
	{
		if (CropedMode || ContinueWrite)
		{
			btn_SaveAIDraw_Click(this, null);
			SaveAIMarkedImage();
			if (CropedMode)
			{
				if (!SwitchDragImage(1))
				{
					Saved = true;
					StartFindSameImage();
					Close();
				}
			}
			else if (!SwitchImage(1))
			{
				Saved = true;
				StartFindSameImage();
				Close();
			}
			cnv_Ink.Children.Clear();
			while (cnv_Ink.Strokes.Count > 0)
			{
				cnv_Ink.Strokes.RemoveAt(cnv_Ink.Strokes.Count - 1);
			}
			for (int i = 0; i < 4; i++)
			{
				IgnoreTrackON[i] = false;
				IgnoreTrack[i].Visibility = Visibility.Hidden;
				IgnoreRect[i].Visibility = Visibility.Hidden;
			}
			btn_IGN01.Background = System.Windows.Media.Brushes.Orange;
			btn_IGN02.Background = System.Windows.Media.Brushes.Orange;
			btn_IGN03.Background = System.Windows.Media.Brushes.Orange;
			btn_IGN04.Background = System.Windows.Media.Brushes.Orange;
			if (PenModeIndex != 0 && PenModeIndex != 3)
			{
				Btn_Pen_Click(this, null);
			}
		}
		else
		{
			btn_SaveAIDraw_Click(this, null);
			SaveAIMarkedImage();
			SwitchImage(1);
			Saved = true;
			StartFindSameImage();
			Close();
		}
	}

	private void FillIgnoreTrackArea()
	{
		IgnoreMat.SetTo(0.0);
		OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 0, 0);
		for (int i = 0; i < 4; i++)
		{
			if (!IgnoreTrackON[i])
			{
				continue;
			}
			rect = GetIgnoreTracker(i);
			if (rect.Width > 0 && rect.Height > 0)
			{
				if (IgnoreTrackColor[i] == 0)
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(125, 0, 0), -1);
				}
				else if (IgnoreTrackColor[i] == 1)
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(0, 125, 0), -1);
				}
				else if (IgnoreTrackColor[i] == 2)
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(0, 0, 125), -1);
				}
				else if (IgnoreTrackColor[i] == 3)
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(125, 125, 0), -1);
				}
				else if (IgnoreTrackColor[i] == 4)
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(125, 0, 125), -1);
				}
				else if (IgnoreTrackColor[i] == 5)
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(0, 125, 125), -1);
				}
				else
				{
					IgnoreMat.Rectangle(rect, Scalar.FromRgb(125, 0, 0), -1);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AutoSetIgnoreArea()
	{
		if (cnv_Ink == null)
		{
			return;
		}
		try
		{
			BitmapImage bitmapImage = new BitmapImage();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)cnv_Ink.ActualWidth, (int)cnv_Ink.ActualHeight, 96.0, 96.0, PixelFormats.Default);
			renderTargetBitmap.Render(cnv_Ink);
			BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
			bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Bitmap src;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmapEncoder.Save(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				src = new Bitmap(memoryStream);
			}
			Mat mat = src.ToMat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			Mat mat5 = new Mat();
			Cv2.CvtColor(mat, mat5, ColorConversionCodes.BGR2GRAY);
			Cv2.InRange(mat5, 20.0, 255.0, mat5);
			int num = Cv2.ConnectedComponentsWithStats(mat5, mat2, mat3, mat4, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat3.GetGenericIndexer<int>();
			OpenCvSharp.Rect rect = new OpenCvSharp.Rect(0, 0, 50, 50);
			for (int i = 1; i < num; i++)
			{
				rect = new OpenCvSharp.Rect(genericIndexer[i, 0] - 15, genericIndexer[i, 1] - 15, genericIndexer[i, 2] + 30, genericIndexer[i, 3] + 30);
				rect &= new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);
			}
			bor_RET1.SetValue(Canvas.LeftProperty, (double)rect.X);
			bor_RET1.SetValue(Canvas.TopProperty, (double)rect.Y);
			bor_RET1.SetValue(FrameworkElement.WidthProperty, (double)rect.Width);
			bor_RET1.SetValue(FrameworkElement.HeightProperty, (double)rect.Height);
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIMarkWindow-AutoSetIgnoreArea()" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AutoDrawIgnoreArea(Mat src, ref Mat dst)
	{
		Mat mat = new Mat();
		Mat mat2 = new Mat();
		Mat mat3 = new Mat();
		try
		{
			AutoDrawExtend = 1;
		}
		catch
		{
			AutoDrawExtend = 1;
			txt_AutoIgnoreSize.Text = "1";
		}
		if (AutoDrawByRect)
		{
			int num = Cv2.ConnectedComponentsWithStats(src, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			for (int i = 1; i < num; i++)
			{
				OpenCvSharp.Rect rect = new OpenCvSharp.Rect(genericIndexer[i, 0] - AutoDrawExtend, genericIndexer[i, 1] - AutoDrawExtend, genericIndexer[i, 2] + AutoDrawExtend * 2, genericIndexer[i, 3] + AutoDrawExtend * 2);
				rect &= new OpenCvSharp.Rect(0, 0, src.Width, src.Height);
				dst.Rectangle(rect, Scalar.FromRgb(20, 20, 20), -1);
			}
		}
		else
		{
			Mat mat4 = dst.Clone();
			mat4.SetTo(Scalar.FromRgb(20, 20, 20));
			Mat mat5 = new Mat();
			Cv2.Dilate(src, mat5, new Mat(), null, AutoDrawExtend);
			mat4.CopyTo(dst, mat5);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SaveAIMarkedImage()
	{
		try
		{
			FinalMat = OrgBitmap.Clone();
			bool num = ConvertInkToMat();
			bool flag = ConvertMaskToMat();
			if (!num && !flag)
			{
				return;
			}
			OpenCvSharp.Size dsize = new OpenCvSharp.Size(FinalMat.Width, FinalMat.Height);
			FinalMat.SetTo(0.0);
			FillIgnoreTrackArea();
			Mat mat = new Mat();
			Cv2.CvtColor(IgnoreMat, mat, ColorConversionCodes.BGR2GRAY);
			Cv2.InRange(mat, 5.0, 255.0, mat);
			mat = mat.Resize(dsize);
			IgnoreMat = IgnoreMat.Resize(dsize);
			IgnoreMat.CopyTo(FinalMat, mat);
			if (EnableAutoDrawIgnore)
			{
				Cv2.CvtColor(DrawingMat, mat, ColorConversionCodes.BGR2GRAY);
				Cv2.InRange(mat, 5.0, 255.0, mat);
				mat = mat.Resize(dsize);
				Mat dst = FinalMat.Clone();
				dst.SetTo(0.0);
				AutoDrawIgnoreArea(mat, ref dst);
				Cv2.CvtColor(dst, mat, ColorConversionCodes.BGR2GRAY);
				Cv2.InRange(mat, 5.0, 255.0, mat);
				dst.CopyTo(FinalMat, mat);
			}
			Cv2.CvtColor(DrawingMat, mat, ColorConversionCodes.BGR2GRAY);
			Cv2.InRange(mat, 5.0, 255.0, mat);
			mat = mat.Resize(dsize);
			DrawingMat = DrawingMat.Resize(dsize);
			DrawingMat.CopyTo(FinalMat, mat);
			if (SingleReDrawMode)
			{
				ImWrite(ReDrawFilePath, FinalMat);
			}
			else
			{
				if (FinalMat.Width <= 0 || FinalMat.Height <= 0)
				{
					return;
				}
				string pathY = NGMarkedFolder;
				string pathX = NGFolder;
				if (chk_SavebyFolder.IsChecked == true)
				{
					SaveFolderWindow saveFolderWindow = new SaveFolderWindow();
					saveFolderWindow.SetFolder(pathY, pathX);
					saveFolderWindow.ShowDialog();
					saveFolderWindow.GetFolder(ref pathY, ref pathX);
				}
				string str = "";
				if (pathY == "")
				{
					str = ((LanguageType == 1) ? string.Format("尚未設定AI影像資料夾!") : ((LanguageType == 2) ? string.Format("尚未设定AI影像资料夹!") : ((LanguageType != 3) ? string.Format("Save AI image folder first!") : string.Format("AI画像フォルダが設定されていません!"))));
					MessageBox.Show(str);
					return;
				}
				if (!Directory.Exists(pathY))
				{
					Directory.CreateDirectory(pathY);
				}
				if (!Directory.Exists(pathY))
				{
					return;
				}
				DateTime now = DateTime.Now;
				string path = string.Format("{0}NG-{1}-{2}.png", pathY, StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
				ImWrite(path, FinalMat);
				if (pathX == "")
				{
					str = ((LanguageType == 1) ? string.Format("尚未設定AI影像資料夾!") : ((LanguageType == 2) ? string.Format("尚未设定AI影像资料夹!") : ((LanguageType != 3) ? string.Format("Save AI image folder first!") : string.Format("AI画像フォルダが設定されていません!"))));
					MessageBox.Show(str);
					return;
				}
				if (!Directory.Exists(pathX))
				{
					Directory.CreateDirectory(pathX);
				}
				if (!Directory.Exists(pathX))
				{
					return;
				}
				path = string.Format("{0}NG-{1}-{2}.png", pathX, StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
				string path2 = string.Format("{0}NG-{1}-{2}.png", pathX, StationIndex + 1, now.ToString("yyyy-MM-dd-HH-mm-ss"));
				if (!File.Exists(path))
				{
					ImWrite(path, OrgBitmap);
				}
				if (!CropedMode)
				{
					string modelName_A = inspection.paramFile.AIEnhanceParam.ModelName_A;
					modelName_A = System.IO.Path.GetFileNameWithoutExtension(modelName_A);
					string text = string.Format("{0}TrainingImage\\{1}\\Origional\\{2}\\NG\\", AppDomain.CurrentDomain.BaseDirectory, modelName_A, StationIndex + 1);
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
					if (Directory.Exists(text))
					{
						string path3 = string.Format("{0}{1}", text, System.IO.Path.GetFileName(path2));
						ImWrite(path3, OrgImage);
					}
				}
				if (LanguageType == 0)
				{
					str = string.Format("{0} Save AI Image Finish", path);
				}
				else if (LanguageType == 1)
				{
					str = string.Format("{0} AI 影像儲存成功", path);
				}
				else if (LanguageType == 2)
				{
					str = string.Format("{0} AI 影像储存成功!", path);
				}
				ShowLabMessage(str);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AIMarkWindow-SaveAIMarkedImage()" + ex.ToString());
		}
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

	private Mat ImWrite(string path, Mat mat)
	{
		if (IsContainGBKString(path))
		{
			Bitmap bitmap = mat.ToBitmap();
			bitmap.Save(path);
			bitmap.Dispose();
		}
		else
		{
			mat.SaveImage(path);
		}
		return mat;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_NextImage_Click(object sender, RoutedEventArgs e)
	{
		Button obj = (Button)sender;
		int num = 1;
		num = ((obj.Uid == "next") ? 1 : (-1));
		if (CropedMode)
		{
			SwitchDragImage(num);
		}
		else
		{
			SwitchImage(num);
		}
		cnv_Ink.Children.Clear();
		while (cnv_Ink.Strokes.Count > 0)
		{
			cnv_Ink.Strokes.RemoveAt(cnv_Ink.Strokes.Count - 1);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool SwitchDragImage(int value)
	{
		if (filelist == null)
		{
			return false;
		}
		if (filelist.Length == 0)
		{
			return false;
		}
		if (fileIndex < 0)
		{
			fileIndex = 0;
		}
		if (fileIndex + value < filelist.Length && fileIndex + value >= 0)
		{
			fileIndex += value;
		}
		else
		{
			if (fileIndex + value >= 0)
			{
				return false;
			}
			fileIndex = filelist.Length - 1;
		}
		ImageIndex = fileIndex + 1;
		ImageCount = filelist.Length;
		btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
		Mat mat = CVBuffer.ImRead(filelist[fileIndex]);
		SetOrgImage(mat);
		SetMat(mat, ZoomMode);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool SwitchImage(int value)
	{
		try
		{
			ResetAndDelete();
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			case 0:
				return false;
			case 1:
				if (filelist == null)
				{
					return false;
				}
				if (filelist.Length == 0)
				{
					return false;
				}
				if (fileIndex < 0)
				{
					fileIndex = 0;
				}
				if (fileIndex + value == filelist.Length)
				{
					return false;
				}
				if (fileIndex + value < filelist.Length && fileIndex + value >= 0)
				{
					fileIndex += value;
				}
				else if (fileIndex + value < 0)
				{
					fileIndex = filelist.Length - 1;
				}
				else
				{
					fileIndex = 0;
				}
				ImageIndex = fileIndex + 1;
				ImageCount = filelist.Length;
				btn_ImageNum.Content = string.Format("{0} / {1}", fileIndex + 1, filelist.Length);
				mat = CVBuffer.ImRead(filelist[fileIndex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (RecentBufferIndex < 0)
				{
					return false;
				}
				if (inspection.RecentBufferList.Count <= 0)
				{
					return false;
				}
				if (RecentBufferIndex + value == inspection.RecentBufferList.Count)
				{
					return false;
				}
				if (RecentBufferIndex + value < inspection.RecentBufferList.Count && RecentBufferIndex + value >= 0)
				{
					RecentBufferIndex += value;
				}
				else if (RecentBufferIndex + value < 0)
				{
					RecentBufferIndex = inspection.RecentBufferList.Count - 1;
				}
				else
				{
					RecentBufferIndex = 0;
				}
				ImageIndex = RecentBufferIndex + 1;
				ImageCount = inspection.RecentBufferList.Count;
				btn_ImageNum.Content = string.Format("{0} / {1}", RecentBufferIndex + 1, inspection.RecentBufferList.Count);
				inspection.SetBuffers(inspection.RecentBufferList[RecentBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (OKBufferIndex < 0)
				{
					return false;
				}
				if (inspection.OKBufferList.Count <= 0)
				{
					return false;
				}
				if (OKBufferIndex + value == inspection.OKBufferList.Count)
				{
					return false;
				}
				if (OKBufferIndex + value < inspection.OKBufferList.Count && OKBufferIndex + value >= 0)
				{
					OKBufferIndex += value;
				}
				else if (RecentBufferIndex + value < 0)
				{
					OKBufferIndex = inspection.OKBufferList.Count - 1;
				}
				else
				{
					OKBufferIndex = 0;
				}
				ImageIndex = OKBufferIndex + 1;
				ImageCount = inspection.OKBufferList.Count;
				btn_ImageNum.Content = string.Format("{0} / {1}", OKBufferIndex + 1, inspection.OKBufferList.Count);
				inspection.SetBuffers(inspection.OKBufferList[OKBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (NGBufferIndex < 0)
				{
					return false;
				}
				if (inspection.NGBufferList.Count <= 0)
				{
					return false;
				}
				if (NGBufferIndex + value == inspection.NGBufferList.Count)
				{
					return false;
				}
				if (NGBufferIndex + value < inspection.NGBufferList.Count && NGBufferIndex + value >= 0)
				{
					NGBufferIndex += value;
				}
				else if (RecentBufferIndex + value < 0)
				{
					NGBufferIndex = inspection.NGBufferList.Count - 1;
				}
				else
				{
					NGBufferIndex = 0;
				}
				ImageIndex = NGBufferIndex + 1;
				ImageCount = inspection.NGBufferList.Count;
				btn_ImageNum.Content = string.Format("{0} / {1}", NGBufferIndex + 1, inspection.NGBufferList.Count);
				inspection.SetBuffers(inspection.NGBufferList[NGBufferIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			default:
				return false;
			}
			inspection.Inspect(ref mat);
			inspection.buffers.Dst.CopyTo(OrgImage);
			if (!inspection.LocaetResult || !inspection.AdvlocateResult)
			{
				cnv_Ink.Background = System.Windows.Media.Brushes.LightGray;
				btn_AddImage.IsEnabled = false;
			}
			else
			{
				btn_AddImage.IsEnabled = true;
				SetMat(inspection.GetAIMat(), ZoomMode);
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIMarkWindow-SwitchImage()-01-" + ex.ToString());
			return false;
		}
	}

	private void Wnd_main_PreviewKeyDown(object sender, KeyEventArgs e)
	{
	}

	private void Btn_EyesOff_MouseMove(object sender, MouseEventArgs e)
	{
		cnv_Ink.Visibility = Visibility.Hidden;
		mMask.Visibility = Visibility.Hidden;
	}

	private void Btn_EyesOff_MouseLeave(object sender, MouseEventArgs e)
	{
		cnv_Ink.Visibility = Visibility.Visible;
		mMask.Visibility = Visibility.Visible;
	}

	private void Tab_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
	}

	private void Btn_NGT01_Click(object sender, RoutedEventArgs e)
	{
		Convert.ToInt32(((Button)sender).Uid);
	}

	private OpenCvSharp.Rect GetIgnoreTracker(int TrackerID)
	{
		OpenCvSharp.Rect result = new OpenCvSharp.Rect(0, 0, 0, 0);
		result.X = (int)(Canvas.GetLeft(IgnoreTrack[TrackerID]) / xRatio);
		result.Y = (int)(Canvas.GetTop(IgnoreTrack[TrackerID]) / yRatio);
		result.Width = (int)(IgnoreTrack[TrackerID].Width / xRatio);
		result.Height = (int)(IgnoreTrack[TrackerID].Height / yRatio);
		return result;
	}

	private OpenCvSharp.Rect GetNGTracker(int TrackerID)
	{
		return new OpenCvSharp.Rect(0, 0, 0, 0);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Wnd_main_Closing(object sender, CancelEventArgs e)
	{
		if (!Saved)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("是否儲存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("是否储存！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("save Image?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("保存しますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				SaveAIMarkedImage();
			}
		}
	}

	private void SetTrackColor(int id)
	{
		int num = 0;
		IgnoreRect[id].Opacity = 0.35;
		IgnoreTrack[id].Opacity = 0.35;
		IgnoreTrack[id].BorderBrush = System.Windows.Media.Brushes.Black;
		IgnoreRect[id].BorderBrush = System.Windows.Media.Brushes.Black;
		if (rad_Red.IsChecked == true)
		{
			num = 0;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Red;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Red;
		}
		else if (rad_Lime.IsChecked == true)
		{
			num = 1;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Lime;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Lime;
		}
		else if (rad_Blue.IsChecked == true)
		{
			num = 2;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Blue;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Blue;
		}
		else if (rad_Yellow.IsChecked == true)
		{
			num = 3;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Yellow;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Yellow;
		}
		else if (rad_Fuchsia.IsChecked == true)
		{
			num = 4;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Fuchsia;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Fuchsia;
		}
		else if (rad_Aqua.IsChecked == true)
		{
			num = 5;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Aqua;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Aqua;
		}
		else
		{
			num = 0;
			IgnoreRect[id].Background = System.Windows.Media.Brushes.Red;
			IgnoreTrack[id].Background = System.Windows.Media.Brushes.Red;
			rad_Red.IsChecked = true;
		}
		if (IgnoreTrackON[id])
		{
			TrackBtn[id].Background = System.Windows.Media.Brushes.Orange;
			IgnoreTrackON[id] = false;
			IgnoreTrack[id].Visibility = Visibility.Hidden;
			IgnoreTrackColor[id] = num;
			SwitchPenMode(0);
			return;
		}
		TrackBtn[id].Background = System.Windows.Media.Brushes.LimeGreen;
		IgnoreTrackON[id] = true;
		IgnoreTrack[id].Visibility = Visibility.Visible;
		IgnoreTrackColor[id] = num;
		if (id == 0)
		{
			AutoSetIgnoreArea();
		}
		SwitchPenMode(2);
	}

	private void Btn_IGN01_Click(object sender, RoutedEventArgs e)
	{
		SetTrackColor(CurrentTrack = Convert.ToInt32(((Button)sender).Uid));
	}

	private void ChangeToolStatus()
	{
		if (mCurOp == Operation.Draw)
		{
			lab_UsedTool.Content = icon_PenMode.Content;
		}
		else if (mCurOp == Operation.DrawErase)
		{
			lab_UsedTool.Content = icon_EraserMode.Content;
		}
		else if (mCurOp == Operation.Rect)
		{
			lab_UsedTool.Content = icon_TrackMode.Content;
		}
		else if (mCurOp == Operation.Line)
		{
			lab_UsedTool.Content = icon_Line.Content;
		}
		else if (mCurOp == Operation.Box)
		{
			lab_UsedTool.Content = icon_AddAIRect.Content;
		}
		else if (mCurOp == Operation.Erase)
		{
			lab_UsedTool.Content = icon_AIColorEraser.Content;
		}
		else if (mCurOp == Operation.Point)
		{
			lab_UsedTool.Content = icon_AddPT.Content;
		}
		SaveAITempDraw();
	}

	private void Btn_Pen_Click(object sender, RoutedEventArgs e)
	{
		SwitchPenMode(0);
		cnv_Ink.EditingMode = InkCanvasEditingMode.Ink;
		cnv_Ink.IsEnabled = true;
	}

	private void Btn_EraserMode_Click(object sender, RoutedEventArgs e)
	{
		SwitchPenMode(1);
		cnv_Ink.EditingMode = InkCanvasEditingMode.EraseByPoint;
	}

	private void Btn_TrackMode_Click(object sender, RoutedEventArgs e)
	{
		SwitchPenMode(2);
		cnv_Ink.EditingMode = InkCanvasEditingMode.None;
	}

	private void Wnd_main_Drop(object sender, DragEventArgs e)
	{
		CropedMode = true;
		string[] array = e.Data.GetData(DataFormats.FileDrop, autoConvert: true) as string[];
		if (array.Length >= 1)
		{
			string text = array[0];
			if (string.IsNullOrEmpty(text) || !File.Exists(text))
			{
				CheckFileList(text);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CheckFileList(string path)
	{
		try
		{
			if (path == "" || path == null)
			{
				return;
			}
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				if (!Directory.Exists(path))
				{
					return;
				}
			}
			filelist = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			if (filelist.Length == 0)
			{
				return;
			}
			for (int i = 0; i < filelist.Length; i++)
			{
				string extension = System.IO.Path.GetExtension(filelist[i]);
				extension = extension.ToLower();
				if (extension != ".bmp" && extension != ".jpg" && extension != ".tif" && extension != ".png")
				{
					filelist = filelist.Take(i).Concat(filelist.Skip(i + 1)).ToArray();
					i--;
				}
			}
			if (filelist.Length != 0)
			{
				if (filelist.Length != 0)
				{
					fileIndex = 0;
					string content = string.Format("{0} / {1}", 1, filelist.Length);
					btn_ImageNum.Content = content;
				}
				string text = LanguageManager.Instance["FileNotFound"];
				if (!File.Exists(filelist[fileIndex]))
				{
					MessageBox.Show(filelist[fileIndex] + text);
					return;
				}
				Mat mat = CVBuffer.ImRead(filelist[fileIndex]);
				SetOrgImage(mat);
				SetMat(mat, ZoomMode);
			}
		}
		catch (Exception ex)
		{
			string text2 = "TW01-AIMarkWindow-CheckFileList()" + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, LanguageManager.Instance["Error"]);
		}
	}

	public int GetPreviewMode()
	{
		PreviewMode = cb_PreviewMode.SelectedIndex;
		return PreviewMode;
	}

	public int GetZoomMode()
	{
		return ZoomMode;
	}

	public int GetColorSelect()
	{
		return ColorSelect;
	}

	private void cnv_Ink_Copy_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void cnv_Ink_Copy_MouseMove(object sender, MouseEventArgs e)
	{
	}

	public void SetPreviewMode(int mode)
	{
		PreviewMode = mode;
		cb_PreviewMode.SelectedIndex = mode;
	}

	public void SetColorSelect(int select)
	{
		ColorSelect = select;
		for (int i = 0; i < 6; i++)
		{
			if (select == i)
			{
				CaptionText[i].Background = System.Windows.Media.Brushes.Orange;
			}
			else
			{
				CaptionText[i].Background = System.Windows.Media.Brushes.White;
			}
		}
		switch (ColorSelect)
		{
		case 0:
			rad_Red.IsChecked = true;
			DrawColor = Colors.Red;
			break;
		case 1:
			rad_Lime.IsChecked = true;
			DrawColor = Colors.Lime;
			break;
		case 2:
			rad_Blue.IsChecked = true;
			DrawColor = Colors.Blue;
			break;
		case 3:
			rad_Yellow.IsChecked = true;
			DrawColor = Colors.Yellow;
			break;
		case 4:
			rad_Fuchsia.IsChecked = true;
			DrawColor = Colors.Fuchsia;
			break;
		case 5:
			rad_Aqua.IsChecked = true;
			DrawColor = Colors.Aqua;
			break;
		default:
			rad_Red.IsChecked = true;
			DrawColor = Colors.Red;
			break;
		}
		SetPenInfo();
	}

	public int GetPenSize()
	{
		return linewidth;
	}

	public void SetPenSize(int size)
	{
		linewidth = size;
		if (linewidth < 3)
		{
			linewidth = 3;
		}
		if (linewidth > 200)
		{
			linewidth = 200;
		}
		SetPenInfo();
	}

	public void SetContinueWrite(bool enabled)
	{
	}

	public bool GetContinueWrite()
	{
		return ContinueWrite;
	}

	public void SetAutoDrawIgnore(bool enabled, int IgnoreSize)
	{
		EnableAutoDrawIgnore = true;
		chk_AutoIgnoreDraw.IsChecked = true;
		AutoDrawExtend = 1;
		txt_AutoIgnoreSize.Text = IgnoreSize.ToString();
	}

	public void SetCaption(string[] s, bool EnbleSaveFolder)
	{
		txt_Caption1.Text = s[0];
		txt_Caption2.Text = s[1];
		txt_Caption3.Text = s[2];
		txt_Caption4.Text = s[3];
		txt_Caption5.Text = s[4];
		txt_Caption6.Text = s[5];
		txt_Caption1.ToolTip = s[0];
		txt_Caption2.ToolTip = s[1];
		txt_Caption3.ToolTip = s[2];
		txt_Caption4.ToolTip = s[3];
		txt_Caption5.ToolTip = s[4];
		txt_Caption6.ToolTip = s[5];
		rt_Red.ToolTip = s[0];
		rt_Lime.ToolTip = s[1];
		rt_Blue.ToolTip = s[2];
		rt_Yellow.ToolTip = s[3];
		rt_Fuchsia.ToolTip = s[4];
		rt_Aqua.ToolTip = s[5];
		chk_SavebyFolder.IsChecked = EnbleSaveFolder;
	}

	public void GetCaption(ref string[] s, ref bool EnbleSaveFolder)
	{
		s[0] = txt_Caption1.Text;
		s[1] = txt_Caption2.Text;
		s[2] = txt_Caption3.Text;
		s[3] = txt_Caption4.Text;
		s[4] = txt_Caption5.Text;
		s[5] = txt_Caption6.Text;
		EnbleSaveFolder = chk_SavebyFolder.IsChecked.Value;
	}

	public bool GetEnableAutoDrawIgnore()
	{
		return EnableAutoDrawIgnore;
	}

	public int GetAutoDrawIgnoreSize()
	{
		try
		{
			AutoDrawExtend = 1;
			return AutoDrawExtend;
		}
		catch
		{
			return 7;
		}
	}

	private void Cnv_Ink_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Cnv_Ink_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
	}

	private void SwitchColor(bool Forworkd = true)
	{
		if (Forworkd)
		{
			ColorSelect++;
		}
		else
		{
			ColorSelect--;
		}
		if (ColorSelect < 0)
		{
			ColorSelect = 5;
		}
		if (ColorSelect > 5)
		{
			ColorSelect = 0;
		}
		for (int i = 0; i < 6; i++)
		{
			if (ColorSelect == i)
			{
				CaptionText[i].Background = System.Windows.Media.Brushes.Orange;
			}
			else
			{
				CaptionText[i].Background = System.Windows.Media.Brushes.White;
			}
		}
		switch (ColorSelect)
		{
		case 0:
			rad_Red.IsChecked = true;
			break;
		case 1:
			rad_Lime.IsChecked = true;
			break;
		case 2:
			rad_Blue.IsChecked = true;
			break;
		case 3:
			rad_Yellow.IsChecked = true;
			break;
		case 4:
			rad_Fuchsia.IsChecked = true;
			break;
		case 5:
			rad_Aqua.IsChecked = true;
			break;
		default:
			rad_Red.IsChecked = true;
			break;
		}
		if (PenModeIndex != 0 && PenModeIndex != 3)
		{
			SwitchPenMode(0);
			cnv_Ink.EditingMode = InkCanvasEditingMode.Ink;
		}
		SetPenInfo();
	}

	private void Rad_Red_Checked(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((RadioButton)sender).Uid);
		ColorID = num + 1;
		for (int i = 0; i < 6; i++)
		{
			if (num == i)
			{
				CaptionText[i].Background = System.Windows.Media.Brushes.Gold;
			}
			else
			{
				CaptionText[i].Background = System.Windows.Media.Brushes.White;
			}
		}
		switch (num)
		{
		case 0:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Red;
			DrawColor = Colors.Red;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, 0, 0);
			break;
		case 1:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Lime;
			DrawColor = Colors.Lime;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, 0, byte.MaxValue, 0);
			break;
		case 2:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Blue;
			DrawColor = Colors.Blue;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, 0, 0, byte.MaxValue);
			break;
		case 3:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Yellow;
			DrawColor = Colors.Yellow;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, byte.MaxValue, 0);
			break;
		case 4:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Fuchsia;
			DrawColor = Colors.Fuchsia;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, 0, byte.MaxValue);
			break;
		case 5:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Aqua;
			DrawColor = Colors.Aqua;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, 0, byte.MaxValue, byte.MaxValue);
			break;
		default:
			lab_UsedColor.Background = System.Windows.Media.Brushes.Yellow;
			DrawColor = Colors.Yellow;
			DrawingColor = System.Windows.Media.Color.FromArgb(100, byte.MaxValue, byte.MaxValue, 0);
			break;
		}
		if (CurrentTrack != -1)
		{
			IgnoreRect[CurrentTrack].Opacity = 0.35;
			IgnoreTrack[CurrentTrack].Opacity = 0.35;
			if (IgnoreTrack[CurrentTrack].IsVisible)
			{
				if (num == 0)
				{
					IgnoreTrack[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreRect[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Red;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Red;
					IgnoreTrackColor[CurrentTrack] = 0;
				}
				switch (num)
				{
				case 1:
					IgnoreTrack[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreRect[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Lime;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Lime;
					IgnoreTrackColor[CurrentTrack] = 1;
					break;
				case 2:
					IgnoreTrack[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreRect[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Blue;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Blue;
					IgnoreTrackColor[CurrentTrack] = 2;
					break;
				case 3:
					IgnoreTrack[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreRect[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Yellow;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Yellow;
					IgnoreTrackColor[CurrentTrack] = 3;
					break;
				case 4:
					IgnoreTrack[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreRect[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Fuchsia;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Fuchsia;
					IgnoreTrackColor[CurrentTrack] = 4;
					break;
				case 5:
					IgnoreTrack[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreRect[CurrentTrack].BorderBrush = System.Windows.Media.Brushes.Black;
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Aqua;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Aqua;
					IgnoreTrackColor[CurrentTrack] = 5;
					break;
				default:
					IgnoreTrack[CurrentTrack].Background = System.Windows.Media.Brushes.Red;
					IgnoreRect[CurrentTrack].Background = System.Windows.Media.Brushes.Red;
					IgnoreTrackColor[CurrentTrack] = 0;
					break;
				}
			}
		}
		SetPenInfo();
		ColorSelect = num;
	}

	private void Chk_ContinueWrite_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Cnv_Ink_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (mCurOp <= Operation.Erase)
		{
			return;
		}
		if (e.Delta > 0)
		{
			linewidth += 3;
			if (linewidth >= 200)
			{
				linewidth = 200;
			}
		}
		else
		{
			linewidth -= 3;
			if (linewidth < 3)
			{
				linewidth = 3;
			}
		}
		SetPenInfo();
	}

	private void Rt_Red_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		int num = Convert.ToInt32(((System.Windows.Shapes.Rectangle)sender).Uid);
		ColorRadBtn[num].IsChecked = true;
	}

	private void Chk_AutoIgnoreDraw_Click(object sender, RoutedEventArgs e)
	{
		EnableAutoDrawIgnore = true;
	}

	private void Cb_PreviewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (inited)
		{
			PreviewMode = cb_PreviewMode.SelectedIndex;
			ImageSource imageSource = OrgBitmap.ToBitmapSource();
			imageSource = ((PreviewMode <= 0) ? OrgBitmap.ToBitmapSource() : (PreviewMode switch
			{
				1 => OrgBitmapBGR[2].ToBitmapSource(), 
				2 => OrgBitmapBGR[1].ToBitmapSource(), 
				3 => OrgBitmapBGR[0].ToBitmapSource(), 
				_ => OrgBitmapBGR[2].ToBitmapSource(), 
			}));
			new ImageBrush().ImageSource = imageSource;
			img_BG.Source = imageSource;
		}
	}

	private void Cnv_Ink_PreviewMouseMove(object sender, MouseEventArgs e)
	{
	}

	private void Btn_Line_Click(object sender, RoutedEventArgs e)
	{
		SwitchPenMode(3);
	}

	private void Cnv_Ink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (mCurOp <= Operation.Erase || PenModeIndex != 3)
		{
			return;
		}
		if (LineStart.X == -1 && LineStart.Y == -1)
		{
			System.Windows.Point position = e.GetPosition(this);
			LineStart.X = (int)(position.X - grd_Ink.Margin.Left);
			LineStart.Y = (int)(position.Y - grd_Ink.Margin.Top);
			es_LineStart.Visibility = Visibility.Visible;
			es_LineStart.SetValue(Canvas.LeftProperty, (double)(LineStart.X - linewidth / 2));
			es_LineStart.SetValue(Canvas.TopProperty, (double)(LineStart.Y - linewidth / 2));
			es_LineStart.SetValue(FrameworkElement.WidthProperty, (double)linewidth);
			es_LineStart.SetValue(FrameworkElement.HeightProperty, (double)linewidth);
			if (DrawColor == Colors.Red)
			{
				es_LineStart.Stroke = System.Windows.Media.Brushes.Red;
			}
			else if (DrawColor == Colors.Blue)
			{
				es_LineStart.Stroke = System.Windows.Media.Brushes.Blue;
			}
			else if (DrawColor == Colors.Lime)
			{
				es_LineStart.Stroke = System.Windows.Media.Brushes.Lime;
			}
			else if (DrawColor == Colors.Yellow)
			{
				es_LineStart.Stroke = System.Windows.Media.Brushes.Yellow;
			}
			else if (DrawColor == Colors.Fuchsia)
			{
				es_LineStart.Stroke = System.Windows.Media.Brushes.Fuchsia;
			}
			else
			{
				es_LineStart.Stroke = System.Windows.Media.Brushes.Aqua;
			}
		}
		else if (LineEnd.X == -1 && LineEnd.Y == -1)
		{
			System.Windows.Point position2 = e.GetPosition(this);
			LineEnd.X = (int)(position2.X - grd_Ink.Margin.Left);
			LineEnd.Y = (int)(position2.Y - grd_Ink.Margin.Top);
			Line line = new Line();
			if (DrawColor == Colors.Red)
			{
				line.Stroke = System.Windows.Media.Brushes.Red;
			}
			else if (DrawColor == Colors.Blue)
			{
				line.Stroke = System.Windows.Media.Brushes.Blue;
			}
			else if (DrawColor == Colors.Lime)
			{
				line.Stroke = System.Windows.Media.Brushes.Lime;
			}
			else if (DrawColor == Colors.Yellow)
			{
				line.Stroke = System.Windows.Media.Brushes.Yellow;
			}
			else if (DrawColor == Colors.Fuchsia)
			{
				line.Stroke = System.Windows.Media.Brushes.Fuchsia;
			}
			else
			{
				line.Stroke = System.Windows.Media.Brushes.Aqua;
			}
			line.X1 = LineStart.X;
			line.Y1 = LineStart.Y;
			line.X2 = LineEnd.X;
			line.Y2 = LineEnd.Y;
			line.StrokeThickness = linewidth;
			cnv_Ink.Children.Add(line);
			ref System.Drawing.Point lineStart = ref LineStart;
			ref System.Drawing.Point lineStart2 = ref LineStart;
			ref System.Drawing.Point lineEnd = ref LineEnd;
			int num = (LineEnd.Y = -1);
			int num3 = (lineEnd.X = num);
			int x = (lineStart2.Y = num3);
			lineStart.X = x;
			es_LineStart.Visibility = Visibility.Hidden;
		}
	}

	private void Wnd_main_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Left)
		{
			SwitchColor(Forworkd: false);
		}
		else if (e.Key == Key.Right)
		{
			SwitchColor();
		}
		else if (e.Key == Key.Up)
		{
			linewidth += 3;
			if (linewidth >= 200)
			{
				linewidth = 200;
			}
			SetPenInfo();
		}
		else if (e.Key == Key.Down)
		{
			linewidth -= 3;
			if (linewidth < 3)
			{
				linewidth = 3;
			}
			SetPenInfo();
		}
		else if (e.Key == Key.Return || e.Key == Key.Space)
		{
			Btn_AddImage_Click(this, null);
		}
		else if (e.Key == Key.LeftCtrl)
		{
			btn_SaveAIDraw_Click(this, null);
		}
	}

	private void Btn_Magnify_Click(object sender, RoutedEventArgs e)
	{
		if (ZoomMode == 0)
		{
			ZoomMode = 1;
		}
		else
		{
			ZoomMode = 0;
		}
		Btn_Clear_Click(this, null);
		ZoomWindows();
	}

	private void btn_AutoDraw_Click(object sender, RoutedEventArgs e)
	{
	}

	private void btn_Opacity_Click(object sender, RoutedEventArgs e)
	{
		cnv_Draw.Opacity = 0.65;
	}

	private void ShowLabMessage(string str)
	{
		lab_Message.Visibility = Visibility.Visible;
		DoubleAnimation doubleAnimation = new DoubleAnimation();
		doubleAnimation.From = 0.0;
		doubleAnimation.To = 1.0;
		doubleAnimation.AutoReverse = true;
		doubleAnimation.RepeatBehavior = new RepeatBehavior(1.0);
		lab_Message.Content = str;
		doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.0));
		lab_Message.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
	}

	private void UpdateMarkedMat()
	{
		_ = inited;
	}

	private System.Windows.Point Window2Image(System.Windows.Point clickPoint)
	{
		double actualWidth = img_BG.ActualWidth;
		double actualHeight = img_BG.ActualHeight;
		double num = actualWidth / (double)mOrgwid;
		double num2 = actualHeight / (double)mOrghei;
		double num3 = (actualWidth - num * (double)mOrgwid) / 2.0;
		double num4 = (actualHeight - num2 * (double)mOrghei) / 2.0;
		double num5 = (clickPoint.X - num3) / num;
		double num6 = (clickPoint.Y - num4) / num2;
		return new System.Windows.Point
		{
			X = (int)num5,
			Y = (int)num6
		};
	}

	private System.Windows.Point Image2Window(System.Windows.Point image)
	{
		double actualWidth = img_BG.ActualWidth;
		double actualHeight = img_BG.ActualHeight;
		double num = actualWidth / (double)mOrgwid;
		double num2 = actualHeight / (double)mOrghei;
		double num3 = (actualWidth - num * (double)mOrgwid) / 2.0;
		_ = (actualHeight - num2 * (double)mOrghei) / 2.0;
		double num4 = image.X * num + num3;
		double num5 = image.Y * num2 + num3;
		return new System.Windows.Point
		{
			X = (int)num4,
			Y = (int)num5
		};
	}

	private void btn_FillHole_Click(object sender, RoutedEventArgs e)
	{
		Mat mat = new Mat();
		ConvertInkToMat(mat);
	}

	private void image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (mCurOp != Operation.Box)
		{
			return;
		}
		img_BG.ReleaseMouseCapture();
		if (mCurRectAnno != null)
		{
			System.Windows.Point position = e.GetPosition(img_BG);
			System.Windows.Point rightBottom = Window2Image(position);
			mCurRectAnno.RightBottom = rightBottom;
			int num = 0;
			int num2 = 0;
			if ((int)mCurRectAnno.LeftUP.X > (int)mCurRectAnno.RightBottom.X)
			{
				num = (int)mCurRectAnno.RightBottom.X;
				num2 = (int)mCurRectAnno.LeftUP.X;
			}
			else
			{
				num2 = (int)mCurRectAnno.RightBottom.X;
				num = (int)mCurRectAnno.LeftUP.X;
			}
			int num3 = 0;
			int num4 = 0;
			if ((int)mCurRectAnno.LeftUP.Y > (int)mCurRectAnno.RightBottom.Y)
			{
				num3 = (int)mCurRectAnno.RightBottom.Y;
				num4 = (int)mCurRectAnno.LeftUP.Y;
			}
			else
			{
				num4 = (int)mCurRectAnno.RightBottom.Y;
				num3 = (int)mCurRectAnno.LeftUP.Y;
			}
			new BoxPromotion
			{
				mLeftUp = 
				{
					X = num,
					Y = num3
				},
				mRightBottom = 
				{
					X = num2,
					Y = num4
				}
			};
			EraserRect(rt: new OpenCvSharp.Rect(num, num3, Math.Abs(num2 - num), Math.Abs(num4 - num3)), color: DrawingColor);
			ClearAnation();
		}
	}

	private void ShowMask(float[] mask, System.Windows.Media.Color color)
	{
		if (mSam == null)
		{
			return;
		}
		float[] array = null;
		bool flag = false;
		if (mDataListSave.Count > 0)
		{
			array = mDataListSave.ToArray();
			flag = true;
		}
		WriteableBitmap writeableBitmap = new WriteableBitmap(mOrgwid, mOrghei, 96.0, 96.0, PixelFormats.Pbgra32, null);
		byte[] array2 = new byte[mOrgwid * mOrghei * 4];
		Array.Clear(array2, 0, array2.Length);
		for (int i = 0; i < mOrghei; i++)
		{
			for (int j = 0; j < mOrgwid; j++)
			{
				int num = i * mOrgwid + j;
				if (flag)
				{
					if (array[num] > 0f)
					{
						array2[4 * num] = ColorSet[(int)array[num] - 1].B;
						array2[4 * num + 1] = ColorSet[(int)array[num] - 1].G;
						array2[4 * num + 2] = ColorSet[(int)array[num] - 1].R;
						array2[4 * num + 3] = 100;
					}
					else if (mask[num] > mSam.mask_threshold)
					{
						array2[4 * num] = color.B;
						array2[4 * num + 1] = color.G;
						array2[4 * num + 2] = color.R;
						array2[4 * num + 3] = 100;
					}
				}
				else if (mask[num] > mSam.mask_threshold)
				{
					array2[4 * num] = color.B;
					array2[4 * num + 1] = color.G;
					array2[4 * num + 2] = color.R;
					array2[4 * num + 3] = 100;
				}
			}
		}
		writeableBitmap.WritePixels(new Int32Rect(0, 0, mOrgwid, mOrghei), array2, mOrgwid * 4, 0);
		mMask.Source = writeableBitmap;
		ShowAnimation();
	}

	private void EraserRect(System.Windows.Media.Color color, OpenCvSharp.Rect rt)
	{
		if (mSam == null || mDataList.Count <= 0)
		{
			return;
		}
		float[] array = mDataList.ToArray();
		float[] array2 = null;
		bool flag = false;
		if (mDataListSave.Count > 0)
		{
			array2 = mDataListSave.ToArray();
			flag = true;
		}
		WriteableBitmap writeableBitmap = new WriteableBitmap(mOrgwid, mOrghei, 96.0, 96.0, PixelFormats.Pbgra32, null);
		byte[] array3 = new byte[mOrgwid * mOrghei * 4];
		Array.Clear(array3, 0, array3.Length);
		OpenCvSharp.Rect rect = rt;
		for (int i = 0; i < mOrghei; i++)
		{
			for (int j = 0; j < mOrgwid; j++)
			{
				int num = i * mOrgwid + j;
				if (rect.Contains(j, i))
				{
					mDataList[num] = 0f;
					if (flag)
					{
						mDataListSave[num] = 0f;
					}
				}
				else if (flag)
				{
					if (array2[num] > 0f)
					{
						array3[4 * num] = ColorSet[(int)array2[num] - 1].B;
						array3[4 * num + 1] = ColorSet[(int)array2[num] - 1].G;
						array3[4 * num + 2] = ColorSet[(int)array2[num] - 1].R;
						array3[4 * num + 3] = 100;
					}
					else if (array[num] > mSam.mask_threshold)
					{
						array3[4 * num] = color.B;
						array3[4 * num + 1] = color.G;
						array3[4 * num + 2] = color.R;
						array3[4 * num + 3] = 100;
					}
				}
				else if (array[num] > mSam.mask_threshold)
				{
					array3[4 * num] = color.B;
					array3[4 * num + 1] = color.G;
					array3[4 * num + 2] = color.R;
					array3[4 * num + 3] = 100;
				}
			}
		}
		writeableBitmap.WritePixels(new Int32Rect(0, 0, mOrgwid, mOrghei), array3, mOrgwid * 4, 0);
		mMask.Source = writeableBitmap;
	}

	private void ShowAnimation()
	{
		DoubleAnimation doubleAnimation = new DoubleAnimation();
		doubleAnimation.From = 0.0;
		doubleAnimation.To = 1.0;
		doubleAnimation.AutoReverse = false;
		doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
		mMask.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
	}

	private void SetPenInfo()
	{
		if (inited)
		{
			if (PenModeIndex == 3)
			{
				drawingAttributes = new DrawingAttributes
				{
					Color = DrawColor,
					Width = linewidth,
					Height = linewidth,
					StylusTip = StylusTip.Rectangle,
					FitToCurve = false,
					IsHighlighter = false,
					IgnorePressure = true
				};
			}
			else
			{
				drawingAttributes = new DrawingAttributes
				{
					Color = DrawColor,
					Width = linewidth,
					Height = linewidth,
					StylusTip = StylusTip.Ellipse,
					FitToCurve = false,
					IsHighlighter = false,
					IgnorePressure = true
				};
			}
			cnv_Ink.DefaultDrawingAttributes = drawingAttributes;
		}
	}

	private void SwitchPenMode(int mode)
	{
		cnv_Ink.IsEnabled = true;
		switch (mode)
		{
		case 0:
		{
			mCurOp = Operation.Draw;
			PenModeIndex = 0;
			cnv_Ink.EditingMode = InkCanvasEditingMode.Ink;
			btn_PenMode.Background = System.Windows.Media.Brushes.LimeGreen;
			btn_EraserMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_TrackMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_Line.Background = System.Windows.Media.Brushes.LightGray;
			es_LineStart.Visibility = Visibility.Hidden;
			for (int j = 0; j < 4; j++)
			{
				IgnoreTrack[j].IsEnabled = false;
				IgnoreTrack[j].Visibility = Visibility.Hidden;
				if (IgnoreTrackON[j])
				{
					IgnoreRect[j].Visibility = Visibility.Visible;
					IgnoreRect[j].Margin = new Thickness(Canvas.GetLeft(IgnoreTrack[j]), Canvas.GetTop(IgnoreTrack[j]), 10.0, 10.0);
					IgnoreRect[j].SetValue(FrameworkElement.WidthProperty, IgnoreTrack[j].Width);
					IgnoreRect[j].SetValue(FrameworkElement.HeightProperty, IgnoreTrack[j].Height);
				}
			}
			break;
		}
		case 1:
		{
			mCurOp = Operation.DrawErase;
			PenModeIndex = 1;
			cnv_Ink.EditingMode = InkCanvasEditingMode.EraseByStroke;
			btn_PenMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_EraserMode.Background = System.Windows.Media.Brushes.LimeGreen;
			btn_TrackMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_Line.Background = System.Windows.Media.Brushes.LightGray;
			es_LineStart.Visibility = Visibility.Hidden;
			for (int k = 0; k < 4; k++)
			{
				IgnoreTrack[k].IsEnabled = false;
				IgnoreTrack[k].Visibility = Visibility.Hidden;
				if (IgnoreTrackON[k])
				{
					IgnoreRect[k].Visibility = Visibility.Visible;
					IgnoreRect[k].Margin = new Thickness(Canvas.GetLeft(IgnoreTrack[k]), Canvas.GetTop(IgnoreTrack[k]), 10.0, 10.0);
					IgnoreRect[k].SetValue(FrameworkElement.WidthProperty, IgnoreTrack[k].Width);
					IgnoreRect[k].SetValue(FrameworkElement.HeightProperty, IgnoreTrack[k].Height);
				}
			}
			break;
		}
		case 2:
		{
			mCurOp = Operation.Rect;
			PenModeIndex = 2;
			cnv_Ink.EditingMode = InkCanvasEditingMode.None;
			PenMode = false;
			btn_PenMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_EraserMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_TrackMode.Background = System.Windows.Media.Brushes.LimeGreen;
			btn_Line.Background = System.Windows.Media.Brushes.LightGray;
			es_LineStart.Visibility = Visibility.Hidden;
			for (int i = 0; i < 4; i++)
			{
				IgnoreRect[i].Visibility = Visibility.Hidden;
				if (IgnoreTrackON[i])
				{
					IgnoreTrack[i].IsEnabled = true;
					IgnoreTrack[i].Visibility = Visibility.Visible;
				}
			}
			break;
		}
		default:
		{
			mCurOp = Operation.Line;
			PenModeIndex = 3;
			cnv_Ink.EditingMode = InkCanvasEditingMode.GestureOnly;
			PenMode = false;
			btn_PenMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_EraserMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_TrackMode.Background = System.Windows.Media.Brushes.LightGray;
			btn_Line.Background = System.Windows.Media.Brushes.LimeGreen;
			es_LineStart.Visibility = Visibility.Hidden;
			ref System.Drawing.Point lineStart = ref LineStart;
			ref System.Drawing.Point lineStart2 = ref LineStart;
			ref System.Drawing.Point lineEnd = ref LineEnd;
			int num = (LineEnd.Y = -1);
			int num3 = (lineEnd.X = num);
			int x = (lineStart2.Y = num3);
			lineStart.X = x;
			break;
		}
		}
		SetPenInfo();
		ChangeToolStatus();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ConvertMaskToMat()
	{
		if (cnv_Ink == null)
		{
			return false;
		}
		es_LineStart.Visibility = Visibility.Hidden;
		if ((int)mMask.ActualWidth <= 0 || (int)mMask.ActualHeight <= 0)
		{
			return false;
		}
		try
		{
			BitmapImage bitmapImage = new BitmapImage();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)mMask.ActualWidth, (int)mMask.ActualHeight, 96.0, 96.0, PixelFormats.Default);
			renderTargetBitmap.Render(mMask);
			BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
			bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Bitmap src;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmapEncoder.Save(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				src = new Bitmap(memoryStream);
			}
			DrawingMat2 = src.ToMat();
			OpenCvSharp.Size dsize = new OpenCvSharp.Size(OrgBitmap.Width, OrgBitmap.Height);
			DrawingMat2 = DrawingMat2.Resize(dsize);
			if (AutoDrawRun)
			{
				AutoDrawMat.CopyTo(DrawingMat2, AutoDrawMask);
				AutoDrawRun = false;
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat[] mv = new Mat[3];
			Cv2.Split(DrawingMat, out mv);
			Cv2.InRange(mv[0], 225.0, 255.0, mat);
			Cv2.InRange(mv[1], 225.0, 255.0, mat2);
			Cv2.InRange(mv[2], 225.0, 255.0, mat3);
			if (mat.CountNonZero() + mat2.CountNonZero() + mat3.CountNonZero() <= 0)
			{
				DrawingMat2.CopyTo(DrawingMat);
				return true;
			}
			DrawingMat.CopyTo(DrawingMat2, mat);
			DrawingMat.CopyTo(DrawingMat2, mat2);
			DrawingMat.CopyTo(DrawingMat2, mat3);
			DrawingMat2.CopyTo(DrawingMat);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIMarkWindow-ConvertInkToMat()-01-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ConvertMaskToMatOLD()
	{
		if (cnv_Ink == null)
		{
			return false;
		}
		es_LineStart.Visibility = Visibility.Hidden;
		if ((int)mMask.ActualWidth <= 0 || (int)mMask.ActualHeight <= 0)
		{
			return false;
		}
		try
		{
			BitmapImage bitmapImage = new BitmapImage();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)mMask.ActualWidth, (int)mMask.ActualHeight, 96.0, 96.0, PixelFormats.Default);
			renderTargetBitmap.Render(mMask);
			BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
			bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Bitmap src;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmapEncoder.Save(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				src = new Bitmap(memoryStream);
			}
			DrawingMat2 = src.ToMat();
			OpenCvSharp.Size dsize = new OpenCvSharp.Size(OrgBitmap.Width, OrgBitmap.Height);
			DrawingMat2 = DrawingMat2.Resize(dsize);
			if (AutoDrawRun)
			{
				AutoDrawMat.CopyTo(DrawingMat2, AutoDrawMask);
				AutoDrawRun = false;
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat[] mv = new Mat[3];
			Cv2.CvtColor(DrawingMat, mat2, ColorConversionCodes.BGR2HSV);
			Cv2.Split(mat2, out mv);
			Cv2.InRange(mv[1], 245.0, 255.0, mat);
			if (mat.CountNonZero() <= 0)
			{
				return false;
			}
			Cv2.Erode(mat, mat, new Mat());
			DrawingMat.CopyTo(DrawingMat2, mat);
			DrawingMat2.CopyTo(DrawingMat);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIMarkWindow-ConvertInkToMat()-01-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ConvertInkToMat()
	{
		if (cnv_Ink == null)
		{
			return false;
		}
		es_LineStart.Visibility = Visibility.Hidden;
		try
		{
			if ((int)cnv_Ink.ActualWidth <= 0 || (int)cnv_Ink.ActualHeight <= 0)
			{
				return false;
			}
			BitmapImage bitmapImage = new BitmapImage();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)cnv_Ink.ActualWidth, (int)cnv_Ink.ActualHeight, 96.0, 96.0, PixelFormats.Default);
			renderTargetBitmap.Render(cnv_Ink);
			BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
			bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Bitmap src;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmapEncoder.Save(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				src = new Bitmap(memoryStream);
			}
			DrawingMat = src.ToMat();
			int num = 0;
			using (Mat mat = new Mat())
			{
				Cv2.CvtColor(DrawingMat, mat, ColorConversionCodes.BGR2GRAY);
				num = mat.CountNonZero();
			}
			int num2 = 0;
			for (int i = 0; i < 4; i++)
			{
				if (IgnoreTrackON[i])
				{
					num2++;
				}
			}
			OpenCvSharp.Size dsize = new OpenCvSharp.Size(OrgBitmap.Width, OrgBitmap.Height);
			DrawingMat = DrawingMat.Resize(dsize);
			if (AutoDrawRun)
			{
				AutoDrawMat.CopyTo(DrawingMat, AutoDrawMask);
				AutoDrawRun = false;
			}
			if (FillHoleStatus)
			{
				Mat[] mv = new Mat[3];
				Cv2.Split(DrawingMat, out mv);
				FillHole(ref mv[0]);
				FillHole(ref mv[1]);
				FillHole(ref mv[2]);
				Cv2.Merge(mv, DrawingMat);
			}
			DrawingMat.CopyTo(IgnoreMat);
			IgnoreMat.SetTo(0.0);
			if (num <= 0 && num2 <= 0)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIMarkWindow-ConvertInkToMat()-01-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ConvertInkToMat(Mat mat)
	{
		if (cnv_Ink == null)
		{
			return false;
		}
		try
		{
			if ((int)cnv_Ink.ActualWidth <= 0 || (int)cnv_Ink.ActualHeight <= 0)
			{
				return false;
			}
			BitmapImage bitmapImage = new BitmapImage();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)cnv_Ink.ActualWidth, (int)cnv_Ink.ActualHeight, 96.0, 96.0, PixelFormats.Default);
			renderTargetBitmap.Render(cnv_Ink);
			BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
			bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			Bitmap src;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmapEncoder.Save(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				src = new Bitmap(memoryStream);
			}
			mat = src.ToMat();
			OpenCvSharp.Size dsize = new OpenCvSharp.Size(OrgBitmap.Width, OrgBitmap.Height);
			mat = mat.Resize(dsize);
			Mat[] mv = new Mat[3];
			Cv2.Split(mat, out mv);
			FillHole(ref mv[0]);
			FillHole(ref mv[1]);
			FillHole(ref mv[2]);
			Cv2.Merge(mv, mat);
			Mat mat2 = new Mat();
			Cv2.CvtColor(mat, mat2, ColorConversionCodes.BGR2GRAY);
			Cv2.InRange(mat2, 10.0, 255.0, mat2);
			Mat mat3 = new Mat();
			mat.CopyTo(mat3, mat2);
			Cv2.BitwiseNot(mat2, mat2);
			OrgBitmap.CopyTo(mat3, mat2);
			FillHoleStatus = true;
			ImageSource imageSource = mat3.ToBitmapSource();
			new ImageBrush().ImageSource = imageSource;
			img_BG.Source = imageSource;
			img_BG.Source = imageSource;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-AIMarkWindow-ConvertInkToMat(Mat mat)-02-" + ex.ToString());
			return false;
		}
	}

	private void FillHole(ref Mat Src)
	{
		using Mat mat = new Mat(Src.Height + 2, Src.Width + 2, MatType.CV_8UC1);
		mat.SetTo(0.0);
		Mat mat2 = new Mat(mat, new OpenCvSharp.Rect(1, 1, Src.Width, Src.Height));
		Src.CopyTo(mat2);
		Cv2.FloodFill(mat, new OpenCvSharp.Point(0, 0), new Scalar(255.0));
		Cv2.BitwiseNot(mat2, mat2);
		Cv2.BitwiseOr(Src, mat2, Src);
	}

	private void btn_AddPT_Click(object sender, RoutedEventArgs e)
	{
		Convert.ToInt32((sender as Button).Uid);
		mCurOp = Operation.Point;
		cnv_Ink.IsEnabled = false;
		ChangeToolStatus();
	}

	private void btn_AddAIRect_Click(object sender, RoutedEventArgs e)
	{
		Convert.ToInt32((sender as Button).Uid);
		mCurOp = Operation.Box;
		cnv_Ink.IsEnabled = false;
		ChangeToolStatus();
	}

	private void btn_AIColorEraser_Click(object sender, RoutedEventArgs e)
	{
		Convert.ToInt32((sender as Button).Uid);
		mCurOp = Operation.Erase;
		cnv_Ink.IsEnabled = false;
		ChangeToolStatus();
	}

	private void SaveAITempDraw()
	{
		if (mDataListSave.Count <= 0)
		{
			for (int i = 0; i < mDataList.Count; i++)
			{
				if (mDataList[i] > 0f)
				{
					mDataListSave.Add(ColorID);
				}
				else
				{
					mDataListSave.Add(0f);
				}
			}
		}
		else
		{
			for (int j = 0; j < mDataList.Count; j++)
			{
				if (mDataList[j] > 0f && mDataListSave.Count > j && mDataListSave[j] <= 0f)
				{
					mDataListSave[j] = ColorID;
				}
			}
		}
		Reset();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe void DecodeTempDraw(Mat mat)
	{
		if (mat == null || mSam == null || mat.Width <= 0 || mat.Height <= 0)
		{
			return;
		}
		try
		{
			mDataListSave.Clear();
			mDataList.Clear();
			for (int i = 0; i < mat.Width * mat.Height; i++)
			{
				mDataList.Add(0f);
				mDataListSave.Add(0f);
			}
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			Mat mat4 = new Mat();
			Mat mat5 = new Mat();
			Mat mat6 = new Mat();
			Mat mat7 = new Mat();
			Cv2.InRange(mat, new Scalar(0.0, 0.0, 128.0), new Scalar(10.0, 10.0, 255.0), mat2);
			Cv2.InRange(mat, new Scalar(0.0, 128.0, 0.0), new Scalar(10.0, 255.0, 10.0), mat3);
			Cv2.InRange(mat, new Scalar(128.0, 0.0, 0.0), new Scalar(255.0, 10.0, 10.0), mat4);
			Cv2.InRange(mat, new Scalar(0.0, 128.0, 128.0), new Scalar(10.0, 255.0, 255.0), mat5);
			Cv2.InRange(mat, new Scalar(128.0, 0.0, 128.0), new Scalar(255.0, 10.0, 255.0), mat7);
			Cv2.InRange(mat, new Scalar(128.0, 128.0, 0.0), new Scalar(255.0, 255.0, 10.0), mat6);
			IntPtr data = mat2.Data;
			IntPtr data2 = mat3.Data;
			IntPtr data3 = mat4.Data;
			IntPtr data4 = mat5.Data;
			IntPtr data5 = mat7.Data;
			IntPtr data6 = mat6.Data;
			int width = mat.Width;
			int height = mat.Height;
			WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32, null);
			byte[] array = new byte[width * height * 4];
			Array.Clear(array, 0, array.Length);
			for (int j = 0; j < mat.Height; j++)
			{
				for (int k = 0; k < mat.Width; k++)
				{
					int num = j * width + k;
					byte* ptr = (byte*)data.ToPointer();
					int num2 = width * j + k;
					if (ptr[num2] == byte.MaxValue)
					{
						array[4 * num] = 0;
						array[4 * num + 1] = 0;
						array[4 * num + 2] = byte.MaxValue;
						array[4 * num + 3] = 100;
						mDataListSave[num] = 1f;
					}
					byte* ptr2 = (byte*)data2.ToPointer();
					int num3 = width * j + k;
					if (ptr2[num3] == byte.MaxValue)
					{
						array[4 * num] = 0;
						array[4 * num + 1] = byte.MaxValue;
						array[4 * num + 2] = 0;
						array[4 * num + 3] = 100;
						mDataListSave[num] = 2f;
					}
					byte* ptr3 = (byte*)data3.ToPointer();
					int num4 = width * j + k;
					if (ptr3[num4] == byte.MaxValue)
					{
						array[4 * num] = byte.MaxValue;
						array[4 * num + 1] = 0;
						array[4 * num + 2] = 0;
						array[4 * num + 3] = 100;
						mDataListSave[num] = 3f;
					}
					byte* ptr4 = (byte*)data4.ToPointer();
					int num5 = width * j + k;
					if (ptr4[num5] == byte.MaxValue)
					{
						array[4 * num] = 0;
						array[4 * num + 1] = byte.MaxValue;
						array[4 * num + 2] = byte.MaxValue;
						array[4 * num + 3] = 100;
						mDataListSave[num] = 4f;
					}
					byte* ptr5 = (byte*)data5.ToPointer();
					int num6 = width * j + k;
					if (ptr5[num6] == byte.MaxValue)
					{
						array[4 * num] = byte.MaxValue;
						array[4 * num + 1] = 0;
						array[4 * num + 2] = byte.MaxValue;
						array[4 * num + 3] = 100;
						mDataListSave[num] = 5f;
					}
					byte* ptr6 = (byte*)data6.ToPointer();
					int num7 = width * j + k;
					if (ptr6[num7] == byte.MaxValue)
					{
						array[4 * num] = byte.MaxValue;
						array[4 * num + 1] = byte.MaxValue;
						array[4 * num + 2] = 0;
						array[4 * num + 3] = 100;
						mDataListSave[num] = 6f;
					}
				}
			}
			writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), array, width * 4, 0);
			mMask.Source = writeableBitmap;
		}
		catch (Exception ex)
		{
			string text = "TW01-AIMarkWindow-DecodeTempDraw()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe void ErodeTempDraw()
	{
		if (mDataListSave.Count <= 0)
		{
			return;
		}
		try
		{
			Mat mat = new Mat(mOrghei, mOrgwid, MatType.CV_8UC1);
			IntPtr data = mat.Data;
			for (int i = 0; i < mOrghei; i++)
			{
				for (int j = 0; j < mOrgwid; j++)
				{
					byte* ptr = (byte*)data.ToPointer();
					int num = mOrgwid * i + j;
					if (mDataListSave[num] > 0f)
					{
						ptr[num] = byte.MaxValue;
					}
					else
					{
						ptr[num] = 0;
					}
				}
			}
			Cv2.Erode(mat, mat, new Mat());
			for (int k = 0; k < mOrghei; k++)
			{
				for (int l = 0; l < mOrgwid; l++)
				{
					byte* ptr2 = (byte*)data.ToPointer();
					int num2 = mOrgwid * k + l;
					if (ptr2[num2] == 0)
					{
						mDataListSave[num2] = 0f;
					}
				}
			}
			Reset();
		}
		catch (Exception ex)
		{
			string text = "TW01-AIMarkWindow-ErodeTempDraw()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	private void btn_SaveAIDraw_Click(object sender, RoutedEventArgs e)
	{
		cnv_Ink.IsEnabled = false;
		SaveAITempDraw();
	}

	private void btn_AIUndo_Click(object sender, RoutedEventArgs e)
	{
		if (mSam == null)
		{
			return;
		}
		cnv_Ink.IsEnabled = false;
		Convert.ToInt32((sender as Button).Uid);
		if (mUndoStack.Count == 1)
		{
			Reset();
		}
		else if (mUndoStack.Count > 0)
		{
			Promotion promotion = mUndoStack.Pop();
			mRedoStack.Push(promotion);
			RemoveAnation(promotion);
			mPromotionList.Clear();
			mPromotionList.AddRange(mUndoStack.ToArray());
			MaskData maskData = mSam.Decode(mPromotionList, mImgEmbedding, mOrgwid, mOrghei);
			mDataList = maskData.mMask.ToList();
			if (mDataList.Count > 0)
			{
				ShowMask(maskData.mMask.ToArray(), DrawingColor);
			}
		}
	}

	private void btn_AIReset_Click(object sender, RoutedEventArgs e)
	{
		cnv_Ink.IsEnabled = false;
		Convert.ToInt32((sender as Button).Uid);
		Reset();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AIDelete_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 1) ? MessageBox.Show("是否清除所有標註！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType == 2) ? MessageBox.Show("是否清除所有标注！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 3) ? MessageBox.Show("Clear all mark image?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("すべてのマーキングをクリアしますか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes))));
		if (messageBoxResult != MessageBoxResult.No)
		{
			ResetAndDelete();
			DrawingMat.SetTo(0.0);
			DrawingMat.CopyTo(FinalMat);
			cnv_Ink.Children.Clear();
			while (cnv_Ink.Strokes.Count > 0)
			{
				cnv_Ink.Strokes.RemoveAt(cnv_Ink.Strokes.Count - 1);
			}
			ImageSource imageSource = OrgBitmap.ToBitmapSource();
			new ImageBrush().ImageSource = imageSource;
			img_BG.Source = imageSource;
			for (int i = 0; i < 4; i++)
			{
				IgnoreTrackON[i] = false;
			}
		}
	}

	private void Reset()
	{
		ClearAnation();
		mPromotionList.Clear();
		mUndoStack.Clear();
		mMask.Source = null;
		for (int i = 0; i < mDataList.Count; i++)
		{
			mDataList[i] = 0f;
		}
		if (mDataListSave.Count > 0)
		{
			ShowMask(mDataListSave.ToArray(), DrawingColor);
		}
	}

	private void cnv_Draw_MouseLeave(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.Arrow;
	}

	private void cnv_Draw_MouseMove(object sender, MouseEventArgs e)
	{
		if (mCurOp > Operation.Erase)
		{
			return;
		}
		if (e.LeftButton == MouseButtonState.Pressed && mCurRectAnno != null)
		{
			System.Windows.Point position = e.GetPosition(cnv_Draw);
			double width = Math.Abs(position.X - _startPoint.X);
			double height = Math.Abs(position.Y - _startPoint.Y);
			mCurRectAnno.Width = width;
			mCurRectAnno.Height = height;
			Canvas.SetLeft(mCurRectAnno, Math.Min(_startPoint.X, position.X));
			Canvas.SetTop(mCurRectAnno, Math.Min(_startPoint.Y, position.Y));
		}
		else if (e.RightButton == MouseButtonState.Pressed && mCurRectAnno != null)
		{
			System.Windows.Point position2 = e.GetPosition(cnv_Draw);
			double width2 = Math.Abs(position2.X - _startPoint.X);
			double height2 = Math.Abs(position2.Y - _startPoint.Y);
			mCurRectAnno.Width = width2;
			mCurRectAnno.Height = height2;
			Canvas.SetLeft(mCurRectAnno, Math.Min(_startPoint.X, position2.X));
			Canvas.SetTop(mCurRectAnno, Math.Min(_startPoint.Y, position2.Y));
		}
		if (mCurOp == Operation.Point)
		{
			base.Cursor = Cursors.Hand;
		}
		else if (mCurOp == Operation.Box)
		{
			if (e.RightButton == MouseButtonState.Pressed)
			{
				base.Cursor = Cursors.No;
			}
			else
			{
				base.Cursor = Cursors.Cross;
			}
		}
		else if (mCurOp == Operation.Erase)
		{
			base.Cursor = Cursors.No;
		}
	}

	private void cnv_Draw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (mCurOp > Operation.Erase || !ModelLoad)
		{
			return;
		}
		img_BG.CaptureMouse();
		if (mCurOp == Operation.Point)
		{
			PointAnnotation pointAnnotation = new PointAnnotation((0 == 0) ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black);
			System.Windows.Point position = e.GetPosition(cnv_Draw);
			pointAnnotation.Position = position;
			cnv_Draw.Children.Add(pointAnnotation);
			Promotion promotion = new PointPromotion(OpType.ADD);
			System.Windows.Point position2 = e.GetPosition(img_BG);
			System.Windows.Point point = Window2Image(position2);
			(promotion as PointPromotion).X = (int)point.X;
			(promotion as PointPromotion).Y = (int)point.Y;
			PointPromotion pointPromotion = new Transforms(1024).ApplyCoords(promotion as PointPromotion, mOrgwid, mOrghei);
			pointPromotion.mAnation = pointAnnotation;
			mUndoStack.Push(pointPromotion);
			mPromotionList.Add(pointPromotion);
			MaskData maskData = mSam.Decode(mPromotionList, mImgEmbedding, mOrgwid, mOrghei);
			mDataList = maskData.mMask.ToList();
			if (mDataList.Count > 0)
			{
				ShowMask(maskData.mMask.ToArray(), DrawingColor);
			}
		}
		else if (mCurOp == Operation.Box)
		{
			_startPoint = e.GetPosition(cnv_Draw);
			mCurRectAnno = new RectAnnotation
			{
				Width = 0.0,
				Height = 0.0,
				StartPosition = _startPoint
			};
			Reset();
			cnv_Draw.Children.Add(mCurRectAnno);
			System.Windows.Point position3 = e.GetPosition(img_BG);
			System.Windows.Point leftUP = Window2Image(position3);
			mCurRectAnno.LeftUP = leftUP;
		}
		else if (mCurOp == Operation.Erase)
		{
			_startPoint = e.GetPosition(cnv_Draw);
			mCurRectAnno = new RectAnnotation
			{
				Width = 0.0,
				Height = 0.0,
				StartPosition = _startPoint
			};
			Reset();
			cnv_Draw.Children.Add(mCurRectAnno);
			System.Windows.Point position4 = e.GetPosition(img_BG);
			System.Windows.Point leftUP2 = Window2Image(position4);
			mCurRectAnno.LeftUP = leftUP2;
		}
	}

	private void cnv_Draw_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (mSam == null || mCurOp > Operation.Erase || !ModelLoad)
		{
			return;
		}
		if (mCurOp == Operation.Point)
		{
			PointAnnotation pointAnnotation = new PointAnnotation((1 == 0) ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black);
			System.Windows.Point position = e.GetPosition(cnv_Draw);
			pointAnnotation.Position = position;
			cnv_Draw.Children.Add(pointAnnotation);
			Promotion promotion = new PointPromotion(OpType.REMOVE);
			System.Windows.Point position2 = e.GetPosition(img_BG);
			System.Windows.Point point = Window2Image(position2);
			(promotion as PointPromotion).X = (int)point.X;
			(promotion as PointPromotion).Y = (int)point.Y;
			PointPromotion pointPromotion = new Transforms(1024).ApplyCoords(promotion as PointPromotion, mOrgwid, mOrghei);
			pointPromotion.mAnation = pointAnnotation;
			mUndoStack.Push(pointPromotion);
			mPromotionList.Add(pointPromotion);
			MaskData maskData = mSam.Decode(mPromotionList, mImgEmbedding, mOrgwid, mOrghei);
			mDataList = maskData.mMask.ToList();
			if (mDataList.Count > 0)
			{
				ShowMask(maskData.mMask.ToArray(), DrawingColor);
			}
		}
		else if (mCurOp == Operation.Box)
		{
			SaveAITempDraw();
			_startPoint = e.GetPosition(cnv_Draw);
			mCurRectAnno = new RectAnnotation
			{
				Width = 0.0,
				Height = 0.0,
				StartPosition = _startPoint
			};
			Reset();
			cnv_Draw.Children.Add(mCurRectAnno);
			System.Windows.Point position3 = e.GetPosition(img_BG);
			System.Windows.Point leftUP = Window2Image(position3);
			mCurRectAnno.LeftUP = leftUP;
		}
	}

	public void SetSamAIRect(OpenCvSharp.Rect rect)
	{
		if (mSam == null || !ModelLoad)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		num = rect.Left;
		num2 = rect.Right;
		num3 = rect.Top;
		num4 = rect.Bottom;
		BoxPromotion boxPromotion = new BoxPromotion();
		boxPromotion.mLeftUp.X = num;
		boxPromotion.mLeftUp.Y = num3;
		boxPromotion.mRightBottom.X = num2;
		boxPromotion.mRightBottom.Y = num4;
		BoxPromotion boxPromotion2 = new Transforms(1024).ApplyBox(boxPromotion, mOrgwid, mOrghei);
		boxPromotion2.mAnation = mCurRectAnno;
		mUndoStack.Push(boxPromotion2);
		mPromotionList.Add(boxPromotion2);
		MaskData maskData = mSam.Decode(mPromotionList, mImgEmbedding, mOrgwid, mOrghei);
		mDataList = maskData.mMask.ToList();
		if (mDataListSave.Count > 0)
		{
			return;
		}
		for (int i = 0; i < mDataList.Count; i++)
		{
			if (mDataList[i] > 0f)
			{
				mDataListSave.Add(ColorID);
			}
			else
			{
				mDataListSave.Add(0f);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe Mat OutPutAIDrawMat()
	{
		Mat mat = new Mat();
		if (mDataListSave.Count <= 0)
		{
			return mat;
		}
		try
		{
			mat = new Mat(mOrghei, mOrgwid, MatType.CV_8UC1);
			IntPtr data = mat.Data;
			for (int i = 0; i < mOrghei; i++)
			{
				for (int j = 0; j < mOrgwid; j++)
				{
					byte* ptr = (byte*)data.ToPointer();
					int num = mOrgwid * i + j;
					if (mDataListSave[num] > 0f)
					{
						ptr[num] = byte.MaxValue;
					}
					else
					{
						ptr[num] = 0;
					}
				}
			}
			Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat());
			for (int k = 0; k < mOrghei; k++)
			{
				for (int l = 0; l < mOrgwid; l++)
				{
					byte* ptr2 = (byte*)data.ToPointer();
					int num2 = mOrgwid * k + l;
					if (ptr2[num2] == 0)
					{
						mDataListSave[num2] = 0f;
					}
				}
			}
			mDataListSave.Clear();
			return mat;
		}
		catch (Exception ex)
		{
			string text = "TW01-AIMarkWindow-OutPutAIDrawMat()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
			return mat;
		}
	}

	private void cnv_Draw_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (mSam == null || mCurOp > Operation.Erase || !ModelLoad)
		{
			return;
		}
		img_BG.ReleaseMouseCapture();
		if (mCurRectAnno == null)
		{
			return;
		}
		System.Windows.Point position = e.GetPosition(img_BG);
		System.Windows.Point rightBottom = Window2Image(position);
		mCurRectAnno.RightBottom = rightBottom;
		int num = 0;
		int num2 = 0;
		if ((int)mCurRectAnno.LeftUP.X > (int)mCurRectAnno.RightBottom.X)
		{
			num = (int)mCurRectAnno.RightBottom.X;
			num2 = (int)mCurRectAnno.LeftUP.X;
		}
		else
		{
			num2 = (int)mCurRectAnno.RightBottom.X;
			num = (int)mCurRectAnno.LeftUP.X;
		}
		int num3 = 0;
		int num4 = 0;
		if ((int)mCurRectAnno.LeftUP.Y > (int)mCurRectAnno.RightBottom.Y)
		{
			num3 = (int)mCurRectAnno.RightBottom.Y;
			num4 = (int)mCurRectAnno.LeftUP.Y;
		}
		else
		{
			num4 = (int)mCurRectAnno.RightBottom.Y;
			num3 = (int)mCurRectAnno.LeftUP.Y;
		}
		BoxPromotion boxPromotion = new BoxPromotion();
		boxPromotion.mLeftUp.X = num;
		boxPromotion.mLeftUp.Y = num3;
		boxPromotion.mRightBottom.X = num2;
		boxPromotion.mRightBottom.Y = num4;
		if (mCurOp == Operation.Box)
		{
			BoxPromotion boxPromotion2 = new Transforms(1024).ApplyBox(boxPromotion, mOrgwid, mOrghei);
			boxPromotion2.mAnation = mCurRectAnno;
			mUndoStack.Push(boxPromotion2);
			mPromotionList.Add(boxPromotion2);
			MaskData maskData = mSam.Decode(mPromotionList, mImgEmbedding, mOrgwid, mOrghei);
			mDataList = maskData.mMask.ToList();
			if (mDataList.Count > 0)
			{
				ShowMask(maskData.mMask.ToArray(), DrawingColor);
			}
			mCurRectAnno = null;
		}
		else if (mCurOp == Operation.Erase)
		{
			EraserRect(rt: new OpenCvSharp.Rect(num, num3, Math.Abs(num2 - num), Math.Abs(num4 - num3)), color: DrawingColor);
			ClearAnation();
		}
	}

	private void cnv_Draw_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (mSam == null || mCurOp > Operation.Erase || !ModelLoad || mCurOp != Operation.Box)
		{
			return;
		}
		img_BG.ReleaseMouseCapture();
		if (mCurRectAnno != null)
		{
			System.Windows.Point position = e.GetPosition(img_BG);
			System.Windows.Point rightBottom = Window2Image(position);
			mCurRectAnno.RightBottom = rightBottom;
			int num = 0;
			int num2 = 0;
			if ((int)mCurRectAnno.LeftUP.X > (int)mCurRectAnno.RightBottom.X)
			{
				num = (int)mCurRectAnno.RightBottom.X;
				num2 = (int)mCurRectAnno.LeftUP.X;
			}
			else
			{
				num2 = (int)mCurRectAnno.RightBottom.X;
				num = (int)mCurRectAnno.LeftUP.X;
			}
			int num3 = 0;
			int num4 = 0;
			if ((int)mCurRectAnno.LeftUP.Y > (int)mCurRectAnno.RightBottom.Y)
			{
				num3 = (int)mCurRectAnno.RightBottom.Y;
				num4 = (int)mCurRectAnno.LeftUP.Y;
			}
			else
			{
				num4 = (int)mCurRectAnno.RightBottom.Y;
				num3 = (int)mCurRectAnno.LeftUP.Y;
			}
			new BoxPromotion
			{
				mLeftUp = 
				{
					X = num,
					Y = num3
				},
				mRightBottom = 
				{
					X = num2,
					Y = num4
				}
			};
			EraserRect(rt: new OpenCvSharp.Rect(num, num3, Math.Abs(num2 - num), Math.Abs(num4 - num3)), color: DrawingColor);
			ClearAnation();
		}
	}

	private void bor_RET1_GotFocus(object sender, RoutedEventArgs e)
	{
		Border border = (Border)sender;
		CurrentTrack = Convert.ToInt32(border.Uid);
	}

	private void btn_MaskErode_Click(object sender, RoutedEventArgs e)
	{
		SaveAITempDraw();
		ErodeTempDraw();
	}

	private void ResetAndDelete()
	{
		mDataListSave.Clear();
		ClearAnation();
		mPromotionList.Clear();
		mUndoStack.Clear();
		mMask.Source = null;
		for (int i = 0; i < mDataList.Count; i++)
		{
			mDataList[i] = 0f;
		}
	}

	private void ClearAnation()
	{
		List<UserControl> list = new List<UserControl>();
		foreach (object child in cnv_Draw.Children)
		{
			if (child is PointAnnotation || child is RectAnnotation)
			{
				list.Add(child as UserControl);
			}
		}
		list.ForEach(delegate(UserControl e)
		{
			cnv_Draw.Children.Remove(e);
		});
	}

	private void RemoveAnation(Promotion pt)
	{
		if (cnv_Draw.Children.Contains(pt.mAnation))
		{
			cnv_Draw.Children.Remove(pt.mAnation);
		}
	}

	private void AddAnation(Promotion pt)
	{
		if (!cnv_Draw.Children.Contains(pt.mAnation))
		{
			cnv_Draw.Children.Add(pt.mAnation);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void OpenAIModelAndImage()
	{
		if (mSam == null)
		{
			return;
		}
		try
		{
			mOrgwid = SamBitmap.Width;
			mOrghei = SamBitmap.Height;
			Mat mat = SamBitmap.Clone();
			mImgEmbedding = mSam.Encode(mat, mOrgwid, mOrghei);
			mAutoMask = new SAMAutoMask(4, 64, 0.88f, 0.95f, 1f, 0.7f, 0, 0.7f, 0.34133333f, 1, null, 0, "binary_mask");
			mAutoMask.mImgEmbedding = mImgEmbedding;
			mAutoMask.mSAM = mSam;
			ModelLoad = true;
			mat.Dispose();
		}
		catch (Exception ex)
		{
			string text = "TW01-AIMarkWindow-OpenAIModelAndImage()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void OpenAIModelAndImage(Mat bufimage)
	{
		if (mSam == null)
		{
			return;
		}
		try
		{
			mOrgwid = bufimage.Width;
			mOrghei = bufimage.Height;
			Mat mat = bufimage.Clone();
			mImgEmbedding = mSam.Encode(mat, mOrgwid, mOrghei);
			mAutoMask = new SAMAutoMask(4, 64, 0.88f, 0.95f, 1f, 0.7f, 0, 0.7f, 0.34133333f, 1, null, 0, "binary_mask");
			mAutoMask.mImgEmbedding = mImgEmbedding;
			mAutoMask.mSAM = mSam;
			ModelLoad = true;
			mat.Dispose();
		}
		catch (Exception ex)
		{
			string text = "TW01-AIMarkWindow-OpenAIModelAndImage()" + ex.ToString();
			CVBuffer.SaveLog(text);
			MessageBox.Show(text, LanguageManager.Instance["Error"]);
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
			Uri resourceLocator = new Uri("/InspTools;component/other/aimarkwindow.xaml", UriKind.Relative);
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
			wnd_main = (AIMarkWindow)target;
			wnd_main.PreviewKeyDown += Wnd_main_PreviewKeyDown;
			wnd_main.Closing += Wnd_main_Closing;
			wnd_main.Drop += Wnd_main_Drop;
			wnd_main.KeyDown += Wnd_main_KeyDown;
			break;
		case 2:
			lab_Message = (Label)target;
			break;
		case 3:
			tab_Main = (TabControl)target;
			tab_Main.SelectionChanged += Tab_Main_SelectionChanged;
			break;
		case 4:
			tab_Draw = (TabItem)target;
			break;
		case 5:
			btn_Clear = (Button)target;
			btn_Clear.Click += Btn_Clear_Click;
			break;
		case 6:
			grd_IgnoreTrack = (Grid)target;
			break;
		case 7:
			btn_IGN01 = (Button)target;
			btn_IGN01.Click += Btn_IGN01_Click;
			break;
		case 8:
			btn_IGN02 = (Button)target;
			btn_IGN02.Click += Btn_IGN01_Click;
			break;
		case 9:
			btn_IGN03 = (Button)target;
			btn_IGN03.Click += Btn_IGN01_Click;
			break;
		case 10:
			btn_IGN04 = (Button)target;
			btn_IGN04.Click += Btn_IGN01_Click;
			break;
		case 11:
			rt_Red = (System.Windows.Shapes.Rectangle)target;
			rt_Red.PreviewMouseLeftButtonDown += Rt_Red_PreviewMouseLeftButtonDown;
			break;
		case 12:
			rt_Lime = (System.Windows.Shapes.Rectangle)target;
			rt_Lime.PreviewMouseLeftButtonDown += Rt_Red_PreviewMouseLeftButtonDown;
			break;
		case 13:
			rt_Blue = (System.Windows.Shapes.Rectangle)target;
			rt_Blue.PreviewMouseLeftButtonDown += Rt_Red_PreviewMouseLeftButtonDown;
			break;
		case 14:
			rt_Yellow = (System.Windows.Shapes.Rectangle)target;
			rt_Yellow.PreviewMouseLeftButtonDown += Rt_Red_PreviewMouseLeftButtonDown;
			break;
		case 15:
			rt_Fuchsia = (System.Windows.Shapes.Rectangle)target;
			rt_Fuchsia.PreviewMouseLeftButtonDown += Rt_Red_PreviewMouseLeftButtonDown;
			break;
		case 16:
			rt_Aqua = (System.Windows.Shapes.Rectangle)target;
			rt_Aqua.PreviewMouseLeftButtonDown += Rt_Red_PreviewMouseLeftButtonDown;
			break;
		case 17:
			txt_Caption1 = (TextBox)target;
			break;
		case 18:
			txt_Caption2 = (TextBox)target;
			break;
		case 19:
			txt_Caption3 = (TextBox)target;
			break;
		case 20:
			txt_Caption4 = (TextBox)target;
			break;
		case 21:
			txt_Caption5 = (TextBox)target;
			break;
		case 22:
			txt_Caption6 = (TextBox)target;
			break;
		case 23:
			lab_DNet3 = (Label)target;
			break;
		case 24:
			rad_Red = (RadioButton)target;
			rad_Red.Checked += Rad_Red_Checked;
			break;
		case 25:
			cb_PreviewMode = (ComboBox)target;
			cb_PreviewMode.SelectionChanged += Cb_PreviewMode_SelectionChanged;
			break;
		case 26:
			txt_AutoIgnoreSize = (TextBox)target;
			break;
		case 27:
			lab_AutoIgnoreSize = (Label)target;
			break;
		case 28:
			rad_Lime = (RadioButton)target;
			rad_Lime.Checked += Rad_Red_Checked;
			break;
		case 29:
			rad_Blue = (RadioButton)target;
			rad_Blue.Checked += Rad_Red_Checked;
			break;
		case 30:
			rad_Yellow = (RadioButton)target;
			rad_Yellow.Checked += Rad_Red_Checked;
			break;
		case 31:
			rad_Fuchsia = (RadioButton)target;
			rad_Fuchsia.Checked += Rad_Red_Checked;
			break;
		case 32:
			rad_Aqua = (RadioButton)target;
			rad_Aqua.Checked += Rad_Red_Checked;
			break;
		case 33:
			chk_AutoIgnoreDraw = (CheckBox)target;
			chk_AutoIgnoreDraw.Click += Chk_AutoIgnoreDraw_Click;
			break;
		case 34:
			lab_UsedTool = (Label)target;
			break;
		case 35:
			lab_UsedColor = (Label)target;
			break;
		case 36:
			tab_MarkNG = (TabItem)target;
			break;
		case 37:
			tab_MarkOK = (TabItem)target;
			break;
		case 38:
			btn_AddImage = (Button)target;
			btn_AddImage.Click += Btn_AddImage_Click;
			break;
		case 39:
			btn_ImageNum = (Button)target;
			break;
		case 40:
			btn_NextImage = (Button)target;
			btn_NextImage.Click += Btn_NextImage_Click;
			break;
		case 41:
			btn_PrevImage = (Button)target;
			btn_PrevImage.Click += Btn_NextImage_Click;
			break;
		case 42:
			grd_Ink = (Grid)target;
			break;
		case 43:
			cnv_Draw = (Canvas)target;
			cnv_Draw.MouseLeave += cnv_Draw_MouseLeave;
			cnv_Draw.MouseMove += cnv_Draw_MouseMove;
			cnv_Draw.PreviewMouseLeftButtonDown += cnv_Draw_PreviewMouseLeftButtonDown;
			cnv_Draw.PreviewMouseRightButtonDown += cnv_Draw_PreviewMouseRightButtonDown;
			cnv_Draw.PreviewMouseLeftButtonUp += cnv_Draw_PreviewMouseLeftButtonUp;
			cnv_Draw.PreviewMouseRightButtonUp += cnv_Draw_PreviewMouseRightButtonUp;
			break;
		case 44:
			img_BG = (System.Windows.Controls.Image)target;
			break;
		case 45:
			mMask = (System.Windows.Controls.Image)target;
			break;
		case 46:
			cnv_Ink = (InkCanvas)target;
			cnv_Ink.PreviewMouseRightButtonDown += Cnv_Ink_PreviewMouseRightButtonDown;
			cnv_Ink.PreviewMouseRightButtonUp += Cnv_Ink_PreviewMouseRightButtonUp;
			cnv_Ink.MouseWheel += Cnv_Ink_MouseWheel;
			cnv_Ink.PreviewMouseMove += Cnv_Ink_PreviewMouseMove;
			cnv_Ink.PreviewMouseLeftButtonDown += Cnv_Ink_PreviewMouseLeftButtonDown;
			break;
		case 47:
			bor_RET1 = (Border)target;
			bor_RET1.GotFocus += bor_RET1_GotFocus;
			break;
		case 48:
			txt_Search1Label = (TextBlock)target;
			break;
		case 49:
			bor_RET2 = (Border)target;
			bor_RET2.GotFocus += bor_RET1_GotFocus;
			break;
		case 50:
			txt_Search1Labe2 = (TextBlock)target;
			break;
		case 51:
			bor_RET3 = (Border)target;
			bor_RET3.GotFocus += bor_RET1_GotFocus;
			break;
		case 52:
			txt_Search1Labe3 = (TextBlock)target;
			break;
		case 53:
			bor_RET4 = (Border)target;
			bor_RET4.GotFocus += bor_RET1_GotFocus;
			break;
		case 54:
			txt_Search1Labe4 = (TextBlock)target;
			break;
		case 55:
			es_LineStart = (Ellipse)target;
			break;
		case 56:
			bor_01 = (Border)target;
			break;
		case 57:
			bor_02 = (Border)target;
			break;
		case 58:
			bor_03 = (Border)target;
			break;
		case 59:
			bor_04 = (Border)target;
			break;
		case 60:
			chk_ContinueWrite = (CheckBox)target;
			chk_ContinueWrite.Click += Chk_ContinueWrite_Click;
			break;
		case 61:
			grd_InkDraw = (Grid)target;
			break;
		case 62:
			lab_AILoad = (Label)target;
			break;
		case 63:
			btn_PenMode = (Button)target;
			btn_PenMode.Click += Btn_Pen_Click;
			break;
		case 64:
			btn_EraserMode = (Button)target;
			btn_EraserMode.Click += Btn_EraserMode_Click;
			break;
		case 65:
			btn_TrackMode = (Button)target;
			btn_TrackMode.Click += Btn_TrackMode_Click;
			break;
		case 66:
			btn_Line = (Button)target;
			btn_Line.Click += Btn_Line_Click;
			break;
		case 67:
			btn_AutoDraw = (Button)target;
			btn_AutoDraw.Click += btn_AutoDraw_Click;
			break;
		case 68:
			btn_Magnify = (Button)target;
			btn_Magnify.Click += Btn_Magnify_Click;
			break;
		case 69:
			btn_FillHole = (Button)target;
			btn_FillHole.Click += btn_FillHole_Click;
			break;
		case 70:
			grd_AIDraw = (Grid)target;
			break;
		case 71:
			btn_AddPT = (Button)target;
			btn_AddPT.Click += btn_AddPT_Click;
			break;
		case 72:
			btn_AddAIRect = (Button)target;
			btn_AddAIRect.Click += btn_AddAIRect_Click;
			break;
		case 73:
			btn_AIColorEraser = (Button)target;
			btn_AIColorEraser.Click += btn_AIColorEraser_Click;
			break;
		case 74:
			btn_SaveAIDraw = (Button)target;
			btn_SaveAIDraw.Click += btn_SaveAIDraw_Click;
			break;
		case 75:
			btn_AIUndo = (Button)target;
			btn_AIUndo.Click += btn_AIUndo_Click;
			break;
		case 76:
			btn_AIReset = (Button)target;
			btn_AIReset.Click += btn_AIReset_Click;
			break;
		case 77:
			grd_AIDraw_Copy1 = (Grid)target;
			break;
		case 78:
			btn_AddPT1 = (Button)target;
			btn_AddPT1.Click += btn_AddPT_Click;
			break;
		case 79:
			btn_MaskErode = (Button)target;
			btn_MaskErode.Click += btn_MaskErode_Click;
			break;
		case 80:
			btn_AIColorEraser1 = (Button)target;
			btn_AIColorEraser1.Click += btn_AIColorEraser_Click;
			break;
		case 81:
			btn_SaveAIDraw2 = (Button)target;
			btn_SaveAIDraw2.Click += btn_SaveAIDraw_Click;
			break;
		case 82:
			btn_AIUndo1 = (Button)target;
			btn_AIUndo1.Click += btn_AIUndo_Click;
			break;
		case 83:
			btn_AIReset1 = (Button)target;
			btn_AIReset1.Click += btn_AIReset_Click;
			break;
		case 84:
			btn_AIDelete = (Button)target;
			btn_AIDelete.Click += btn_AIDelete_Click;
			break;
		case 85:
			btn_EyesOff = (Button)target;
			btn_EyesOff.MouseMove += Btn_EyesOff_MouseMove;
			btn_EyesOff.MouseLeave += Btn_EyesOff_MouseLeave;
			break;
		case 86:
			btn_Opacity = (Button)target;
			btn_Opacity.Click += btn_Opacity_Click;
			break;
		case 87:
			chk_SavebyFolder = (CheckBox)target;
			break;
		case 88:
			lab_classic = (Label)target;
			break;
		case 89:
			grd_Icons = (Grid)target;
			break;
		case 90:
			icon_PenMode = (Button)target;
			icon_PenMode.Click += Btn_Pen_Click;
			break;
		case 91:
			icon_EraserMode = (Button)target;
			icon_EraserMode.Click += Btn_EraserMode_Click;
			break;
		case 92:
			icon_TrackMode = (Button)target;
			icon_TrackMode.Click += Btn_TrackMode_Click;
			break;
		case 93:
			icon_Line = (Button)target;
			icon_Line.Click += Btn_Line_Click;
			break;
		case 94:
			grd_AIDraw_Copy = (Grid)target;
			break;
		case 95:
			icon_AddPT = (Button)target;
			icon_AddPT.Click += btn_AddPT_Click;
			break;
		case 96:
			icon_AddAIRect = (Button)target;
			icon_AddAIRect.Click += btn_AddAIRect_Click;
			break;
		case 97:
			icon_AIColorEraser = (Button)target;
			icon_AIColorEraser.Click += btn_AIColorEraser_Click;
			break;
		case 98:
			btn_SaveAIDraw1 = (Button)target;
			btn_SaveAIDraw1.Click += btn_SaveAIDraw_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
