using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;
using Microsoft.Win32;
using OpenCvSharp;
using SAMViewer;

namespace InspTools.Other;

public class NGCloneWindow : System.Windows.Window, IComponentConnector
{
	private string OldImgPath = "";

	private string OldDrawImgPath = "";

	private string NewImgPath = "";

	private string NewDrawImgPath = "";

	private int DeleteResult;

	private double xRatio = 1.0;

	private double yRatio = 1.0;

	private int ImageWidth;

	private int ImageHeight;

	private int ImageBand;

	private double CanvasWidth;

	private double CanvasHeight;

	private double DstxRatio = 1.0;

	private double DstyRatio = 1.0;

	private int DstImageWidth;

	private int DstImageHeight;

	private int DstImageBand;

	private double DstCanvasWidth;

	private double DstCanvasHeight;

	private int ImageListSource;

	private string[] OKfilelist;

	private string[] NGfilelist;

	private int ImageIndex;

	private Button[] PathBtn;

	private int LanguageType = 1;

	private CVInspection inspection;

	private ThroughputParamFile ThroughputFile;

	private SAM mSam;

	private OpenCvSharp.Rect LastRect = new OpenCvSharp.Rect(0, 0, 0, 0);

	private OpenCvSharp.Rect NGBlobRect;

	private OpenCvSharp.Rect OrgAIRect;

	private OpenCvSharp.Rect DstAIRect;

	private Mat OrgBitmap = new Mat();

	private Mat OrgCloneBitmap = new Mat();

	private Mat DstBitmap = new Mat();

	private Mat DstBitmapResult = new Mat();

	private Mat DstDrawResult = new Mat();

	private Mat DstOpenBitmap = new Mat();

	private Mat DstCloneBitmap = new Mat();

	private Mat DstAIDrawBitmap = new Mat();

	private Mat ResultBitmap = new Mat();

	private Mat OpenMat = new Mat();

	private Mat OrgDrawMat = new Mat();

	private Mat DstNGDrawMat = new Mat();

	private OpenCvSharp.Point DstCenter = new OpenCvSharp.Point(0, 0);

	private bool UpdateUI;

	internal Canvas cnv_ImageClone;

	internal Border bor_NGDestScope;

	internal TextBlock txt_Search1Label;

	internal Button btn_Draw;

	internal Button btn_Eye;

	internal Button btn_AI;

	internal Button btn_AIOutSide;

	internal Button btn_Save;

	internal CheckBox chk_EnableRealtimeAI;

	internal CheckBox chk_EnableRealtimeAIOutside;

	internal TextBox Label_ResultError;

	internal Canvas cnv_Image;

	internal Border bor_NGScope;

	internal TextBlock txt_Search1Label1;

	internal CheckBox chk_EnableHMirror;

	internal CheckBox chk_EnableVMirror;

	internal TextBox txt_Angle;

	internal ComboBox cmb_SeamlessMethod;

	internal CheckBox chk_EnableAutoMark;

	internal TextBox txt_DilateCount;

	internal CheckBox chk_EnableRandom;

	internal CheckBox chk_EnabledRadomH;

	internal CheckBox chk_EnabledRadomV;

	internal CheckBox chk_EnabledRadomA;

	internal Button btn_ALL;

	internal Button btn_OK;

	internal Button btn_NG;

	internal Button btn_OKDB;

	internal Button btn_NGDB;

	internal Button btn_Open;

	internal Button btn_ImageNum;

	internal Button btn_ImagePrev;

	internal Button btn_ImageNext;

	private bool _contentLoaded;

	public NGCloneWindow()
	{
		InitializeComponent();
		CanvasWidth = cnv_Image.Width;
		CanvasHeight = cnv_Image.Height;
		DstCanvasWidth = cnv_ImageClone.Width;
		DstCanvasHeight = cnv_ImageClone.Height;
		btn_AI.IsEnabled = false;
		btn_AIOutSide.IsEnabled = false;
		btn_Draw.IsEnabled = false;
		bor_NGDestScope.Visibility = Visibility.Hidden;
		PathBtn = new Button[6] { btn_OKDB, btn_NGDB, btn_ALL, btn_OK, btn_NG, btn_Open };
		Label_ResultError.Visibility = Visibility.Hidden;
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

	public void setThroughputParamFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
	}

	public void SetSAM(SAM sam)
	{
		mSam = sam;
	}

	private void UpdateBtnColor(int uid)
	{
		for (int i = 0; i < PathBtn.Length; i++)
		{
			if (i == uid)
			{
				PathBtn[i].Background = Brushes.GreenYellow;
			}
			else
			{
				PathBtn[i].Background = Brushes.White;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Open_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			UpdateBtnColor(5);
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.tif;*.png)|*.bmp;*.jpg;*.tif;*.png|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}
			OpenMat = CVBuffer.ImRead(openFileDialog.FileName);
			if (OpenMat.Channels() == 4)
			{
				Cv2.CvtColor(OpenMat, OpenMat, ColorConversionCodes.BGRA2BGR);
			}
			inspection.SetBuffers(OpenMat);
			DstBitmap = new Mat();
			Mat mat = new Mat();
			inspection.Inspect(ref mat, offLine: true);
			if (!inspection.LocaetResult || !inspection.AdvlocateResult || inspection.maskTool.AIMat.Width <= 0 || inspection.maskTool.AIMat.Height <= 0)
			{
				cnv_Image.Background = Brushes.LightGray;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				SetResult(0, "");
				DstAIRect = new OpenCvSharp.Rect(inspection.buffers.LocateImageRect.X + inspection.maskTool.AIRect.Left, inspection.buffers.LocateImageRect.Y + inspection.maskTool.AIRect.Top, inspection.maskTool.AIRect.Width, inspection.maskTool.AIRect.Height);
				DstBitmap = inspection.maskTool.AIMat;
				SetDstBuffer(DstBitmap);
				DstOpenBitmap = OpenMat.Clone();
			}
			ImageListSource = -1;
			btn_ImageNum.Content = "1 / 1";
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-NGCloneWindow-btn_Open_Click()-01-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private OpenCvSharp.Rect findBlob(Mat binary)
	{
		OpenCvSharp.Rect result = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
			if (binary.CountNonZero() <= 3)
			{
				return result;
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num2 = 0;
			int i = 0;
			if (num >= 1)
			{
				_ = genericIndexer[0, 4];
				for (int j = 1; j < num; j++)
				{
					_ = genericIndexer[j, 4];
					_ = genericIndexer[j, 2];
					_ = genericIndexer[j, 3];
					if (genericIndexer[j, 4] > num2)
					{
						i = j;
						num2 = genericIndexer[j, 4];
					}
				}
				result = new OpenCvSharp.Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
			}
			else
			{
				result = new OpenCvSharp.Rect(0, 0, 0, 0);
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT02-NGCloneWindow-findBlob()-" + ex.ToString());
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Mat GetAIMatDraw(Mat mat, out OpenCvSharp.Rect BlobRect)
	{
		Mat result = new Mat();
		new Mat();
		BlobRect = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
			mat.CopyTo(inspection.maskTool.AIMat);
			inspection.aiEnhanceTools.Inspect(mat, Offline: true, 0);
			if (inspection.aiEnhanceTools.bgr.Width <= 0 || inspection.aiEnhanceTools.bgr.Height <= 0)
			{
				return result;
			}
			Mat mat2 = inspection.aiEnhanceTools.bgr.Clone().Resize(new OpenCvSharp.Size(mat.Width, mat.Height));
			mat2.CopyTo(OrgDrawMat);
			Cv2.CvtColor(mat2, mat2, ColorConversionCodes.BGR2GRAY);
			Cv2.InRange(mat2, 20.0, 255.0, mat2);
			int iterations = Convert.ToInt32(txt_DilateCount.Text);
			Cv2.Dilate(mat2, mat2, new Mat(), null, iterations);
			BlobRect = findBlob(mat2);
			if (BlobRect.Width > 0 && BlobRect.Height > 0)
			{
				result = new Mat(mat2, BlobRect).Clone();
			}
			DstNGDrawMat = new Mat(OrgDrawMat, BlobRect).Clone();
			NGBlobRect = BlobRect;
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-NGCloneWindow-GetAIDraw()-01-" + ex.ToString());
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Mat GetAIDraw(Mat mat, out OpenCvSharp.Rect BlobRect)
	{
		Mat result = new Mat();
		Mat mat2 = new Mat();
		BlobRect = new OpenCvSharp.Rect(0, 0, 0, 0);
		try
		{
			inspection.SetBuffers(mat);
			inspection.Inspect(ref mat2, offLine: true);
			if (inspection.buffers.GetEnhanceAiMatColor().Width <= 0 || inspection.buffers.GetEnhanceAiMatColor().Height <= 0)
			{
				return result;
			}
			using (Mat mat3 = inspection.buffers.GetEnhanceAiMatColor().Clone())
			{
				if (mat3.Width > 0 && mat3.Height > 0)
				{
					Mat mat4 = new Mat(mat3, inspection.maskTool.AIRect).Clone();
					mat4.CopyTo(OrgDrawMat);
					Cv2.CvtColor(mat4, mat4, ColorConversionCodes.BGR2GRAY);
					Cv2.InRange(mat4, 20.0, 255.0, mat4);
					int iterations = Convert.ToInt32(txt_DilateCount.Text);
					Cv2.Dilate(mat4, mat4, new Mat(), null, iterations);
					BlobRect = findBlob(mat4);
					if (BlobRect.Width > 0 && BlobRect.Height > 0)
					{
						result = new Mat(mat4, BlobRect).Clone();
					}
					DstNGDrawMat = new Mat(OrgDrawMat, BlobRect).Clone();
					OrgAIRect = new OpenCvSharp.Rect(inspection.buffers.LocateImageRect.X + inspection.maskTool.AIRect.Left, inspection.buffers.LocateImageRect.Y + inspection.maskTool.AIRect.Top, inspection.maskTool.AIRect.Width, inspection.maskTool.AIRect.Height);
					NGBlobRect = BlobRect;
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-NGCloneWindow-GetAIDraw()-01-" + ex.ToString());
			return result;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool CloneNG(double x, double y)
	{
		try
		{
			if (OrgBitmap.Width <= 0 || DstBitmap.Width <= 0)
			{
				return false;
			}
			OpenCvSharp.Rect BlobRect = new OpenCvSharp.Rect(0, 0, 0, 0);
			Mat mat = new Mat();
			if (chk_EnableAutoMark.IsChecked != false)
			{
				mat = ((OrgCloneBitmap.Width > 0 && OrgCloneBitmap.Height > 0) ? GetAIDraw(OrgCloneBitmap, out BlobRect).Clone() : GetAIMatDraw(OrgBitmap, out BlobRect));
			}
			else
			{
				BlobRect = GetTrackRect(bor_NGScope, xRatio, yRatio);
				mat = new Mat(BlobRect.Height, BlobRect.Width, MatType.CV_8UC1);
				mat.SetTo(255.0);
				OpenCvSharp.Rect BlobRect2 = default(OpenCvSharp.Rect);
				if (OrgCloneBitmap.Width <= 0 || OrgCloneBitmap.Height <= 0)
				{
					GetAIMatDraw(OrgBitmap, out BlobRect2);
				}
				else
				{
					GetAIDraw(OrgCloneBitmap, out BlobRect2).Clone();
				}
			}
			if (BlobRect.Width <= 0 || BlobRect.Height <= 0)
			{
				return false;
			}
			OpenCvSharp.Point point = new OpenCvSharp.Point(x, y);
			Mat mat2 = new Mat(OrgBitmap, BlobRect).Clone();
			if (chk_EnableAutoMark.IsChecked == true)
			{
				Mat mat3 = new Mat();
				Cv2.BitwiseNot(mat, mat3);
				mat2.SetTo(0.0, mat3);
			}
			bool value = chk_EnableRandom.IsChecked.Value;
			int num = new Random().Next(360);
			if (value)
			{
				switch (num % 4)
				{
				case 0:
					if (chk_EnabledRadomH.IsChecked.Value)
					{
						chk_EnableHMirror.IsChecked = false;
					}
					if (chk_EnabledRadomV.IsChecked.Value)
					{
						chk_EnableVMirror.IsChecked = false;
					}
					break;
				case 1:
					if (chk_EnabledRadomH.IsChecked.Value)
					{
						chk_EnableHMirror.IsChecked = false;
					}
					if (chk_EnabledRadomV.IsChecked.Value)
					{
						chk_EnableVMirror.IsChecked = true;
					}
					break;
				case 2:
					if (chk_EnabledRadomH.IsChecked.Value)
					{
						chk_EnableHMirror.IsChecked = true;
					}
					if (chk_EnabledRadomV.IsChecked.Value)
					{
						chk_EnableVMirror.IsChecked = false;
					}
					break;
				case 3:
					if (chk_EnabledRadomH.IsChecked.Value)
					{
						chk_EnableHMirror.IsChecked = true;
					}
					if (chk_EnabledRadomV.IsChecked.Value)
					{
						chk_EnableVMirror.IsChecked = true;
					}
					break;
				}
			}
			if (chk_EnableVMirror.IsChecked == true)
			{
				Cv2.Flip(mat2, mat2, FlipMode.X);
				Cv2.Flip(mat, mat, FlipMode.X);
				if (DstNGDrawMat.Width > 0 && DstNGDrawMat.Height > 0)
				{
					Cv2.Flip(DstNGDrawMat, DstNGDrawMat, FlipMode.X);
				}
			}
			if (chk_EnableHMirror.IsChecked == true)
			{
				Cv2.Flip(mat2, mat2, FlipMode.Y);
				Cv2.Flip(mat, mat, FlipMode.Y);
				if (DstNGDrawMat.Width > 0 && DstNGDrawMat.Height > 0)
				{
					Cv2.Flip(DstNGDrawMat, DstNGDrawMat, FlipMode.Y);
				}
			}
			if (value && chk_EnabledRadomA.IsChecked.Value)
			{
				txt_Angle.Text = num.ToString();
			}
			double num2 = ((txt_Angle.Text == "") ? 0.0 : Convert.ToDouble(txt_Angle.Text));
			if (num2 != 0.0)
			{
				Mat rotationMatrix2D = Cv2.GetRotationMatrix2D(new OpenCvSharp.Point(mat2.Width / 2, mat2.Height / 2), num2, 1.0);
				OpenCvSharp.Size dsize = default(OpenCvSharp.Size);
				dsize.Width = mat2.Cols;
				dsize.Height = mat2.Rows;
				Cv2.WarpAffine(mat2, mat2, rotationMatrix2D, dsize, InterpolationFlags.Linear, BorderTypes.Transparent, Scalar.Black);
				Cv2.WarpAffine(mat, mat, rotationMatrix2D, dsize, InterpolationFlags.Linear, BorderTypes.Replicate, Scalar.Black);
				if (DstNGDrawMat.Width > 0 && DstNGDrawMat.Height > 0)
				{
					Cv2.WarpAffine(DstNGDrawMat, DstNGDrawMat, rotationMatrix2D, dsize, InterpolationFlags.Linear, BorderTypes.Replicate, Scalar.Black);
				}
			}
			OpenCvSharp.Rect roi = findBlob(mat);
			if ((roi.Width != mat.Width || roi.Height != mat.Height) && roi.Width > 0 && roi.Height > 0)
			{
				using Mat mat4 = new Mat(mat, roi).Clone();
				using Mat mat5 = new Mat(mat2, roi).Clone();
				using Mat mat6 = new Mat(DstNGDrawMat, roi).Clone();
				mat4.CopyTo(mat);
				mat5.CopyTo(mat2);
				mat6.CopyTo(DstNGDrawMat);
			}
			if (point.X + mat2.Width / 2 >= DstBitmap.Width)
			{
				point.X = DstBitmap.Width - mat2.Width / 2 - 1;
			}
			if (point.Y + mat2.Height / 2 >= DstBitmap.Height)
			{
				point.Y = DstBitmap.Height - mat2.Height / 2 - 1;
			}
			if (point.X < mat2.Width / 2)
			{
				point.X = mat2.Width / 2;
			}
			if (point.Y < mat2.Height / 2)
			{
				point.Y = mat2.Height / 2;
			}
			DstCenter = point;
			SeamlessCloneMethods flags = (SeamlessCloneMethods)0;
			switch (cmb_SeamlessMethod.SelectedIndex)
			{
			case 0:
				flags = SeamlessCloneMethods.MixedClone;
				break;
			case 1:
				flags = SeamlessCloneMethods.NormalClone;
				break;
			case 2:
				flags = SeamlessCloneMethods.MonochromeTransfer;
				break;
			}
			Cv2.SeamlessClone(mat2, DstBitmap, mat, point, DstBitmapResult, flags);
			DstBitmapResult.CopyTo(DstDrawResult);
			DstDrawResult.SetTo(0.0);
			Mat m = new Mat(roi: new OpenCvSharp.Rect(point.X - mat2.Width / 2, point.Y - mat2.Height / 2, mat2.Width, mat2.Height), m: DstDrawResult);
			DstNGDrawMat.CopyTo(m);
			SetDstBuffer(DstBitmapResult);
			btn_AI.IsEnabled = true;
			btn_AIOutSide.IsEnabled = true;
			btn_Draw.IsEnabled = true;
			DstCloneBitmap = DstOpenBitmap.Clone();
			Mat m2 = new Mat(DstCloneBitmap, DstAIRect);
			DstBitmapResult.CopyTo(m2);
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AMW01-NGCloneWindow-CloneNG()-01-" + ex.ToString());
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInspection(CVInspection station)
	{
		try
		{
			inspection = station;
			UpdateUI = false;
			chk_EnableAutoMark.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableAutoMark;
			if (inspection.paramFile.AIEnhanceParam.NGClone_EnableAutoMark)
			{
				bor_NGScope.Visibility = Visibility.Hidden;
			}
			txt_DilateCount.Text = inspection.paramFile.AIEnhanceParam.NGClone_AutoMarkDilate.ToString();
			cmb_SeamlessMethod.SelectedIndex = inspection.paramFile.AIEnhanceParam.NGClone_ImageMixMode;
			chk_EnableRandom.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableRandom;
			chk_EnabledRadomH.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableRandomH;
			chk_EnabledRadomV.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableRandomV;
			chk_EnabledRadomA.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableRandomA;
			chk_EnableHMirror.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableFlipH;
			chk_EnableVMirror.IsChecked = inspection.paramFile.AIEnhanceParam.NGClone_EnableFlipV;
			txt_Angle.Text = inspection.paramFile.AIEnhanceParam.NGClone_RotateAngle.ToString();
			UpdateUI = true;
		}
		catch (Exception ex)
		{
			string text = "BW05-NGCloneWindow-SetInspection()-" + ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void btn_Eye_Click(object sender, RoutedEventArgs e)
	{
		SetDstBuffer(DstBitmapResult);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetDstBuffer(Mat mat)
	{
		try
		{
			DstImageWidth = mat.Cols;
			DstImageHeight = mat.Rows;
			DstImageBand = mat.Channels();
			DstxRatio = DstCanvasWidth / (double)DstImageWidth;
			DstyRatio = DstCanvasHeight / (double)DstImageHeight;
			if (DstxRatio > DstyRatio)
			{
				DstxRatio = DstyRatio;
			}
			else
			{
				DstyRatio = DstxRatio;
			}
			cnv_ImageClone.SetValue(FrameworkElement.WidthProperty, (double)DstImageWidth * DstxRatio);
			cnv_ImageClone.SetValue(FrameworkElement.HeightProperty, (double)DstImageHeight * DstyRatio);
			ImageSource imageSource = inspection.buffers.MatToBitmapImage(mat);
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_ImageClone.Background = imageBrush;
			imageSource = null;
		}
		catch
		{
			string text = "BW01-NGCloneWindow-SetBuffer()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void WriteParam()
	{
		try
		{
			inspection.paramFile.AIEnhanceParam.NGClone_EnableAutoMark = chk_EnableAutoMark.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_AutoMarkDilate = Convert.ToInt32(txt_DilateCount.Text);
			inspection.paramFile.AIEnhanceParam.NGClone_ImageMixMode = cmb_SeamlessMethod.SelectedIndex;
			inspection.paramFile.AIEnhanceParam.NGClone_EnableRandom = chk_EnableRandom.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_EnableRandomH = chk_EnabledRadomH.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_EnableRandomV = chk_EnabledRadomV.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_EnableRandomA = chk_EnabledRadomA.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_EnableFlipH = chk_EnableHMirror.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_EnableFlipV = chk_EnableVMirror.IsChecked.Value;
			inspection.paramFile.AIEnhanceParam.NGClone_RotateAngle = Convert.ToDouble(txt_Angle.Text);
		}
		catch (Exception ex)
		{
			string text = "BW05-NGCloneWindow-WriteParam()-" + ex.ToString();
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void btn_AI_Click(object sender, RoutedEventArgs e)
	{
		Inspect(DstCloneBitmap.Clone(), drawOutside: false);
	}

	private void btn_AIOutSide_Click(object sender, RoutedEventArgs e)
	{
		Inspect(DstCloneBitmap.Clone(), drawOutside: true);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Draw_Click(object sender, RoutedEventArgs e)
	{
		if (Inspect(DstCloneBitmap.Clone(), drawOutside: false))
		{
			string ngpath = string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(inspection.paramFile.AIEnhanceParam.ModelName_A));
			string markpath = string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(inspection.paramFile.AIEnhanceParam.ModelName_A));
			AIMarkWindow aIMarkWindow = new AIMarkWindow();
			aIMarkWindow.SetInspection(inspection, inspection.GetStationIndex());
			aIMarkWindow.SetClientCode(ThroughputFile.CurrentClient);
			aIMarkWindow.SetSAM(mSam);
			aIMarkWindow.LoadLanguage(LanguageType);
			aIMarkWindow.SetOrgImage(inspection.buffers.Dst);
			aIMarkWindow.SetMat(DstBitmapResult, inspection.paramFile.ZoomMode);
			aIMarkWindow.SetNGMarkedFolder(ngpath, markpath);
			aIMarkWindow.SetColorSelect(inspection.paramFile.ColorSelect);
			aIMarkWindow.SetPreviewMode(inspection.paramFile.PreviewMode);
			aIMarkWindow.SetPenSize(inspection.paramFile.PenSize);
			aIMarkWindow.SetContinueWrite(inspection.paramFile.EnableContinueWrite);
			aIMarkWindow.SetAutoDrawIgnore(inspection.paramFile.EnableAutoIgnoreDraw, inspection.paramFile.AutoIgnoreSize);
			aIMarkWindow.SetCaption(inspection.paramFile.DrawCaition, inspection.paramFile.SaveAsFolder);
			if (DstDrawResult.Width > 0 && DstDrawResult.Height > 0)
			{
				aIMarkWindow.DecodeTempDraw(DstDrawResult);
			}
			aIMarkWindow.ShowDialog();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Save_Click(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Image files (*.png)|*.png|All files (*.*)|*.*";
		if (saveFileDialog.ShowDialog() == true)
		{
			Cv2.ImWrite(saveFileDialog.FileName, DstCloneBitmap);
			MessageBox.Show(string.Format("Image saved to {0}", saveFileDialog.FileName), "Information");
		}
	}

	private void cnv_ImageClone_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (DstOpenBitmap.Width > 0 && DstOpenBitmap.Height > 0)
		{
			System.Windows.Point position = e.GetPosition(cnv_ImageClone);
			double x = position.X;
			double y = position.Y;
			CloneNG(x / DstxRatio, y / DstyRatio);
			if (chk_EnableRealtimeAI.IsChecked == true)
			{
				Inspect(DstCloneBitmap, drawOutside: false);
			}
			if (chk_EnableRealtimeAIOutside.IsChecked == true)
			{
				Inspect(DstCloneBitmap, drawOutside: true);
			}
			WriteParam();
		}
	}

	private void chk_EnableRealtimeAI_Checked(object sender, RoutedEventArgs e)
	{
		chk_EnableRealtimeAIOutside.IsChecked = false;
	}

	private void chk_EnableRealtimeAIOutside_Checked(object sender, RoutedEventArgs e)
	{
		chk_EnableRealtimeAI.IsChecked = false;
	}

	private void chk_EnableAutoMark_Click(object sender, RoutedEventArgs e)
	{
		if (chk_EnableAutoMark.IsChecked == false)
		{
			bor_NGScope.Visibility = Visibility.Visible;
		}
		else
		{
			bor_NGScope.Visibility = Visibility.Hidden;
		}
		SetBuffer(OrgBitmap, OrgCloneBitmap);
	}

	private void chk_EnableRandom_Click(object sender, RoutedEventArgs e)
	{
		if (chk_EnabledRadomH.IsChecked == true)
		{
			chk_EnabledRadomH.Visibility = Visibility.Visible;
			chk_EnabledRadomV.Visibility = Visibility.Visible;
			chk_EnabledRadomA.Visibility = Visibility.Visible;
		}
		else
		{
			chk_EnabledRadomH.Visibility = Visibility.Hidden;
			chk_EnabledRadomV.Visibility = Visibility.Hidden;
			chk_EnabledRadomA.Visibility = Visibility.Hidden;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffer(Mat mat)
	{
		try
		{
			ImageWidth = mat.Cols;
			ImageHeight = mat.Rows;
			ImageBand = mat.Channels();
			xRatio = CanvasWidth / (double)ImageWidth;
			yRatio = CanvasHeight / (double)ImageHeight;
			if (xRatio > yRatio)
			{
				xRatio = yRatio;
			}
			else
			{
				yRatio = xRatio;
			}
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)ImageWidth * xRatio);
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)ImageHeight * yRatio);
			OrgCloneBitmap = new Mat();
			OrgBitmap = mat.Clone();
			OpenCvSharp.Rect BlobRect = default(OpenCvSharp.Rect);
			ImageSource imageSource;
			if (chk_EnableAutoMark.IsChecked == true)
			{
				Mat mat2 = GetAIMatDraw(mat, out BlobRect).Clone();
				Mat mat3 = new Mat();
				if (mat2.Width > 0 && mat2.Height > 0 && inspection.aiEnhanceTools.bgr.Width > 0 && inspection.aiEnhanceTools.bgr.Height > 0)
				{
					Mat mat4 = inspection.aiEnhanceTools.bgr.Clone();
					mat4 = mat4.Resize(new OpenCvSharp.Size(mat.Width, mat.Height));
					Cv2.CvtColor(mat4, mat4, ColorConversionCodes.BGR2GRAY);
					Cv2.InRange(mat4, 20.0, 255.0, mat4);
					Cv2.BitwiseAnd(OrgBitmap, OrgDrawMat, mat3, mat4);
					Cv2.BitwiseNot(mat4, mat4);
					OrgBitmap.CopyTo(mat3, mat4);
					imageSource = inspection.buffers.MatToBitmapImage(mat3);
				}
				else
				{
					imageSource = inspection.buffers.MatToBitmapImage(mat);
				}
			}
			else
			{
				imageSource = inspection.buffers.MatToBitmapImage(mat);
			}
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
		}
		catch
		{
			string text = "BW01-NGCloneWindow-SetBuffer()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetBuffer(Mat mat, Mat orgmat)
	{
		try
		{
			ImageWidth = mat.Cols;
			ImageHeight = mat.Rows;
			ImageBand = mat.Channels();
			xRatio = CanvasWidth / (double)ImageWidth;
			yRatio = CanvasHeight / (double)ImageHeight;
			if (xRatio > yRatio)
			{
				xRatio = yRatio;
			}
			else
			{
				yRatio = xRatio;
			}
			cnv_Image.SetValue(FrameworkElement.WidthProperty, (double)ImageWidth * xRatio);
			cnv_Image.SetValue(FrameworkElement.HeightProperty, (double)ImageHeight * yRatio);
			if (inspection.buffers.LocatedSrc.Channels() == 3)
			{
				OrgBitmap = mat.Clone();
			}
			else
			{
				Cv2.CvtColor(mat, OrgBitmap, ColorConversionCodes.GRAY2BGR);
			}
			OrgCloneBitmap = orgmat.Clone();
			OpenCvSharp.Rect BlobRect = default(OpenCvSharp.Rect);
			ImageSource imageSource;
			if (chk_EnableAutoMark.IsChecked == true)
			{
				GetAIDraw(OrgCloneBitmap, out BlobRect).Clone();
				Mat mat2 = OrgBitmap.Clone();
				if (OrgDrawMat.Width > 0 && OrgDrawMat.Height > 0 && BlobRect.Width > 0 && BlobRect.Height > 0)
				{
					Mat mat3 = OrgDrawMat.Clone();
					Cv2.CvtColor(mat3, mat3, ColorConversionCodes.BGR2GRAY);
					Cv2.InRange(mat3, 20.0, 255.0, mat3);
					Cv2.BitwiseAnd(OrgBitmap, OrgDrawMat, mat2, mat3);
					imageSource = inspection.buffers.MatToBitmapImage(mat2);
				}
				else
				{
					imageSource = inspection.buffers.MatToBitmapImage(OrgBitmap);
				}
			}
			else
			{
				imageSource = inspection.buffers.MatToBitmapImage(OrgBitmap);
			}
			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = imageSource;
			cnv_Image.Background = imageBrush;
			imageSource = null;
		}
		catch
		{
			string text = "BW01-NGCloneWindow-SetBuffer()";
			CVBuffer.SaveLog(text, inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			MessageBox.Show(text, "錯誤");
		}
	}

	private void btn_ALL_Click(object sender, RoutedEventArgs e)
	{
		ImageListSource = 2;
		ImageIndex = 0;
		SwitchImage();
	}

	private void btn_ImagePrev_Click(object sender, RoutedEventArgs e)
	{
		SwitchImage(-1);
	}

	private void btn_OK_Click(object sender, RoutedEventArgs e)
	{
		ImageListSource = 3;
		ImageIndex = 0;
		SwitchImage();
	}

	private void btn_NG_Click(object sender, RoutedEventArgs e)
	{
		ImageListSource = 4;
		ImageIndex = 0;
		SwitchImage();
	}

	private void btn_OKDB_Click(object sender, RoutedEventArgs e)
	{
		ImageListSource = 0;
		ImageIndex = 0;
		SwitchImage();
	}

	private void btn_NGDB_Click(object sender, RoutedEventArgs e)
	{
		ImageListSource = 1;
		ImageIndex = 0;
		SwitchImage();
	}

	private void btn_ImageNext_Click(object sender, RoutedEventArgs e)
	{
		SwitchImage(1);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SwitchImage(int plus = 0)
	{
		try
		{
			UpdateBtnColor(ImageListSource);
			Mat mat = new Mat();
			switch (ImageListSource)
			{
			default:
				return;
			case 0:
				if (OKfilelist == null)
				{
					OKfilelist = Directory.GetFiles(inspection.paramFile.OKPath, "*.*", SearchOption.TopDirectoryOnly);
				}
				if (OKfilelist.Length == 0)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", 0, OKfilelist.Length);
					return;
				}
				if (ImageIndex + plus < 0)
				{
					ImageIndex = 0;
				}
				else if (ImageIndex + plus < OKfilelist.Length)
				{
					ImageIndex += plus;
				}
				else
				{
					ImageIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, OKfilelist.Length);
				mat = CVBuffer.ImRead(OKfilelist[ImageIndex]);
				inspection.SetBuffers(mat);
				break;
			case 1:
				if (NGfilelist == null)
				{
					NGfilelist = Directory.GetFiles(inspection.paramFile.NGPath, "*.*", SearchOption.TopDirectoryOnly);
				}
				if (NGfilelist.Length == 0)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", 0, NGfilelist.Length);
					return;
				}
				if (ImageIndex + plus < 0)
				{
					ImageIndex = 0;
				}
				else if (ImageIndex + plus < NGfilelist.Length)
				{
					ImageIndex += plus;
				}
				else
				{
					ImageIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, NGfilelist.Length);
				mat = CVBuffer.ImRead(NGfilelist[ImageIndex]);
				inspection.SetBuffers(mat);
				break;
			case 2:
				if (inspection.RecentBufferList == null)
				{
					return;
				}
				if (inspection.RecentBufferList.Count <= 0)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", 0, inspection.RecentBufferList.Count);
					return;
				}
				if (ImageIndex + plus < 0)
				{
					ImageIndex = 0;
				}
				else if (ImageIndex + plus < inspection.RecentBufferList.Count)
				{
					ImageIndex += plus;
				}
				else
				{
					ImageIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, inspection.RecentBufferList.Count);
				inspection.SetBuffers(inspection.RecentBufferList[ImageIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 3:
				if (inspection.OKBufferList == null)
				{
					return;
				}
				if (inspection.OKBufferList.Count <= 0)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", 0, inspection.OKBufferList.Count);
					return;
				}
				if (ImageIndex + plus < 0)
				{
					ImageIndex = 0;
				}
				else if (ImageIndex + plus < inspection.OKBufferList.Count)
				{
					ImageIndex += plus;
				}
				else
				{
					ImageIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, inspection.OKBufferList.Count);
				inspection.SetBuffers(inspection.OKBufferList[ImageIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			case 4:
				if (inspection.NGBufferList == null)
				{
					return;
				}
				if (inspection.NGBufferList.Count <= 0)
				{
					btn_ImageNum.Content = string.Format("{0} / {1}", 0, inspection.NGBufferList.Count);
					return;
				}
				if (ImageIndex + plus < 0)
				{
					ImageIndex = 0;
				}
				else if (ImageIndex + plus < inspection.NGBufferList.Count)
				{
					ImageIndex += plus;
				}
				else
				{
					ImageIndex = 0;
				}
				btn_ImageNum.Content = string.Format("{0} / {1}", ImageIndex + 1, inspection.NGBufferList.Count);
				inspection.SetBuffers(inspection.NGBufferList[ImageIndex], inspection.buffers.SrcRect.Width, inspection.buffers.SrcRect.Height, 3);
				break;
			}
			inspection.Inspect(ref mat, offLine: true);
			if (inspection.maskTool.AIMat.Width <= 0 || inspection.maskTool.AIMat.Height <= 0)
			{
				cnv_Image.Background = Brushes.LightGray;
				if (!inspection.LocaetResult)
				{
					SetResult(2, LanguageManager.Instance["LocateFail"]);
				}
				else if (!inspection.AdvlocateResult)
				{
					SetResult(2, LanguageManager.Instance["AdvLocateFail"]);
				}
			}
			else
			{
				SetResult(0, "");
				OpenMat = inspection.buffers.Dst;
				DstBitmap = inspection.maskTool.AIMat;
				SetDstBuffer(inspection.maskTool.AIMat);
				DstOpenBitmap = OpenMat.Clone();
				DstAIRect = new OpenCvSharp.Rect(inspection.buffers.LocateImageRect.X + inspection.maskTool.AIRect.Left, inspection.buffers.LocateImageRect.Y + inspection.maskTool.AIRect.Top, inspection.maskTool.AIRect.Width, inspection.maskTool.AIRect.Height);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-NGCloneWindow-Btn_SwitchImage_Click()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
		}
	}

	private void Btn_TestNextImg_Click(object sender, RoutedEventArgs e)
	{
		ImageIndex++;
		SwitchImage(1);
	}

	private void SetResult(int result, string msg)
	{
		if (result == 2)
		{
			Label_ResultError.Visibility = Visibility.Visible;
			Label_ResultError.Text = msg;
			Label_ResultError.Background = Brushes.Red;
		}
		else
		{
			Label_ResultError.Visibility = Visibility.Hidden;
		}
	}

	private OpenCvSharp.Rect GetTrackRect(Border bor_track, double xratio, double yratio)
	{
		OpenCvSharp.Rect result = default(OpenCvSharp.Rect);
		result.X = (int)(Canvas.GetLeft(bor_track) / xratio);
		result.Y = (int)(Canvas.GetTop(bor_track) / yratio);
		result.Width = (int)(bor_track.Width / xratio);
		result.Height = (int)(bor_track.Height / yratio);
		return result;
	}

	private void UpdateTrack(Border bor_track, OpenCvSharp.Rect rect, double xratio, double yratio)
	{
		bor_track.SetValue(FrameworkElement.WidthProperty, (double)rect.Width * xratio);
		bor_track.SetValue(FrameworkElement.HeightProperty, (double)rect.Height * yratio);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(Mat org, bool drawOutside)
	{
		if (!inspection.paramFile.AIEnhanceToolON)
		{
			return false;
		}
		if (org.Width <= 0 || org.Height <= 0)
		{
			return false;
		}
		try
		{
			inspection.SetBuffers(org);
			Mat mat = new Mat();
			inspection.Inspect(ref mat, offLine: true);
			inspection.buffers.Dst.CopyTo(mat);
			if (mat.Width <= 0 || mat.Height <= 0)
			{
				return false;
			}
			using (Mat mat2 = inspection.buffers.GetEnhanceAiMatColor().Clone())
			{
				using Mat m = new Mat(mat, inspection.buffers.LocateImageRect);
				if (mat2.Width > 0 && mat2.Height > 0)
				{
					Mat mat3 = new Mat();
					Cv2.CvtColor(mat2, mat3, ColorConversionCodes.BGR2GRAY);
					Cv2.InRange(mat3, 20.0, 255.0, mat3);
					if (drawOutside)
					{
						Cv2.FindContours(mat3, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
						Mat mat4 = mat2.Clone();
						mat4.SetTo(0.0);
						for (int i = 0; i < contours.Length; i++)
						{
							Cv2.DrawContours(mat4, contours, i, Scalar.White, 3);
						}
						Cv2.CvtColor(mat4, mat4, ColorConversionCodes.BGR2GRAY);
						mat3.SetTo(0.0);
						mat4.CopyTo(mat3);
					}
					mat2.CopyTo(m, mat3);
				}
				SetDstBuffer(new Mat(m, inspection.maskTool.AIRect));
			}
			OpenCvSharp.Rect trackRect = GetTrackRect(bor_NGDestScope, DstxRatio, DstyRatio);
			LastRect = trackRect;
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("BT01-NGCloneWindow-Inspect()-01-" + ex.ToString(), inspection.GetStationIndex(), inspection.RecipeName, inspection.buffers.Dst);
			return false;
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
			Uri resourceLocator = new Uri("/InspTools;component/other/ngclonewindow.xaml", UriKind.Relative);
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
			cnv_ImageClone = (Canvas)target;
			cnv_ImageClone.PreviewMouseLeftButtonDown += cnv_ImageClone_PreviewMouseLeftButtonDown;
			break;
		case 2:
			bor_NGDestScope = (Border)target;
			break;
		case 3:
			txt_Search1Label = (TextBlock)target;
			break;
		case 4:
			btn_Draw = (Button)target;
			btn_Draw.Click += btn_Draw_Click;
			break;
		case 5:
			btn_Eye = (Button)target;
			btn_Eye.Click += btn_Eye_Click;
			break;
		case 6:
			btn_AI = (Button)target;
			btn_AI.Click += btn_AI_Click;
			break;
		case 7:
			btn_AIOutSide = (Button)target;
			btn_AIOutSide.Click += btn_AIOutSide_Click;
			break;
		case 8:
			btn_Save = (Button)target;
			btn_Save.Click += btn_Save_Click;
			break;
		case 9:
			chk_EnableRealtimeAI = (CheckBox)target;
			chk_EnableRealtimeAI.Checked += chk_EnableRealtimeAI_Checked;
			break;
		case 10:
			chk_EnableRealtimeAIOutside = (CheckBox)target;
			chk_EnableRealtimeAIOutside.Checked += chk_EnableRealtimeAIOutside_Checked;
			break;
		case 11:
			Label_ResultError = (TextBox)target;
			break;
		case 12:
			cnv_Image = (Canvas)target;
			break;
		case 13:
			bor_NGScope = (Border)target;
			break;
		case 14:
			txt_Search1Label1 = (TextBlock)target;
			break;
		case 15:
			chk_EnableHMirror = (CheckBox)target;
			break;
		case 16:
			chk_EnableVMirror = (CheckBox)target;
			break;
		case 17:
			txt_Angle = (TextBox)target;
			break;
		case 18:
			cmb_SeamlessMethod = (ComboBox)target;
			break;
		case 19:
			chk_EnableAutoMark = (CheckBox)target;
			chk_EnableAutoMark.Click += chk_EnableAutoMark_Click;
			break;
		case 20:
			txt_DilateCount = (TextBox)target;
			break;
		case 21:
			chk_EnableRandom = (CheckBox)target;
			chk_EnableRandom.Click += chk_EnableRandom_Click;
			break;
		case 22:
			chk_EnabledRadomH = (CheckBox)target;
			break;
		case 23:
			chk_EnabledRadomV = (CheckBox)target;
			break;
		case 24:
			chk_EnabledRadomA = (CheckBox)target;
			break;
		case 25:
			btn_ALL = (Button)target;
			btn_ALL.Click += btn_ALL_Click;
			break;
		case 26:
			btn_OK = (Button)target;
			btn_OK.Click += btn_OK_Click;
			break;
		case 27:
			btn_NG = (Button)target;
			btn_NG.Click += btn_NG_Click;
			break;
		case 28:
			btn_OKDB = (Button)target;
			btn_OKDB.Click += btn_OKDB_Click;
			break;
		case 29:
			btn_NGDB = (Button)target;
			btn_NGDB.Click += btn_NGDB_Click;
			break;
		case 30:
			btn_Open = (Button)target;
			btn_Open.Click += btn_Open_Click;
			break;
		case 31:
			btn_ImageNum = (Button)target;
			btn_ImageNum.Click += btn_Open_Click;
			break;
		case 32:
			btn_ImagePrev = (Button)target;
			btn_ImagePrev.Click += btn_ImagePrev_Click;
			break;
		case 33:
			btn_ImageNext = (Button)target;
			btn_ImageNext.Click += btn_ImageNext_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
