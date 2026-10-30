using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace InspTools.AOI;

internal class CVImageBase
{
	private Mat Src = new Mat();

	private Mat Bayer = new Mat();

	private Mat hsv = new Mat();

	private Mat Dst = new Mat();

	private Mat[] SrcRGB = new Mat[3];

	private Mat[] SrcHSV = new Mat[3];

	private Mat Binary = new Mat();

	private Mat Gray = new Mat();

	private Mat roihsv = new Mat();

	private Mat[] ROI_HSV = new Mat[3];

	private Mat[] ROI_RGB = new Mat[3];

	private Mat ROI_Src = new Mat();

	private Mat ROI_Binary = new Mat();

	private Mat ROI_Gray = new Mat();

	private Stopwatch sw = new Stopwatch();

	public bool BufferSeted;

	public bool ROIBufferSeted;

	public void SetMat(Mat mat)
	{
		if (!mat.Empty())
		{
			Src.Release();
			hsv.Release();
			Dst.Release();
			Src = mat.Clone();
			hsv = Src.Clone();
			Cv2.CvtColor(Src, hsv, ColorConversionCodes.BGR2HSV);
			Cv2.CvtColor(Src, Gray, ColorConversionCodes.BGR2GRAY);
			Cv2.Split(Src, out SrcRGB);
			Cv2.Split(hsv, out SrcHSV);
			BufferSeted = true;
		}
	}

	public void SetMat(Bitmap bitmap)
	{
		Src.Release();
		hsv.Release();
		Dst.Release();
		Mat mat = Bitmap2Mat(bitmap);
		Src = mat.Clone();
		hsv = Src.Clone();
		Cv2.CvtColor(Src, hsv, ColorConversionCodes.BGR2HSV);
		Cv2.CvtColor(Src, Gray, ColorConversionCodes.BGR2GRAY);
		Cv2.Split(Src, out SrcRGB);
		Cv2.Split(hsv, out SrcHSV);
		BufferSeted = true;
	}

	public void SetMat(byte[] buffer, int w, int h, int band)
	{
		Src.Release();
		hsv.Release();
		Dst.Release();
		Src = buffer2Mat(buffer, w, h, band);
		if (band == 3)
		{
			hsv = Src.Clone();
			Cv2.CvtColor(Src, hsv, ColorConversionCodes.BGR2HSV);
			Cv2.CvtColor(Src, Gray, ColorConversionCodes.BGR2GRAY);
			Cv2.Split(Src, out SrcRGB);
			Cv2.Split(hsv, out SrcHSV);
		}
		else
		{
			Gray = Src.Clone();
		}
		BufferSeted = true;
	}

	public Mat buffer2Mat(byte[] buffer, int w, int h, int band)
	{
		MatType type = ((band != 3) ? MatType.CV_8UC1 : MatType.CV_8UC3);
		return new Mat(h, w, type, buffer, 0L);
	}

	public void ROIMat_Release()
	{
		for (int i = 0; i < 3; i++)
		{
			if (!ROI_RGB[i].Empty())
			{
				ROI_RGB[i].Release();
			}
			if (!ROI_HSV[i].Empty())
			{
				ROI_HSV[i].Release();
			}
		}
		ROI_Binary.Release();
		ROI_Gray.Release();
		ROI_Src.Release();
	}

	public void SetRotatedImage(Mat mat)
	{
		ROI_Src = mat.Clone();
		if (mat.Channels() == 3)
		{
			Cv2.CvtColor(ROI_Src, roihsv, ColorConversionCodes.BGR2HSV);
			Cv2.CvtColor(ROI_Src, ROI_Gray, ColorConversionCodes.BGR2GRAY);
			Cv2.Split(ROI_Src, out SrcRGB);
			Cv2.Split(roihsv, out SrcHSV);
		}
		else
		{
			ROI_Gray = ROI_Src.Clone();
		}
		ROIBufferSeted = true;
	}

	public Mat GetSrcMat()
	{
		return Src;
	}

	public Mat GetGrayMat()
	{
		return Gray;
	}

	public Mat GetRGBMat(int index)
	{
		return SrcRGB[index];
	}

	public Mat GetHSVMat(int index)
	{
		return SrcHSV[index];
	}

	public Mat GetROISrcMat()
	{
		return ROI_Src;
	}

	public bool Bitmap2Mat(Bitmap bitmap, ref Mat dst)
	{
		dst = bitmap.ToMat();
		return true;
	}

	public Mat Bitmap2Mat(Bitmap bitmap)
	{
		return bitmap.ToMat();
	}

	public ImageSource Mat2ImageSource(Mat image)
	{
		BitmapImage bitmapImage = new BitmapImage();
		Bitmap bitmap = Mat2Bitmap(image);
		using MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, bitmap.RawFormat);
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		return bitmapImage;
	}

	public bool Mat2Bitmap(Mat src, ref Bitmap bitmap)
	{
		bitmap = src.ToBitmap();
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Bitmap Mat2Bitmap(Mat src)
	{
		using MemoryStream stream = src.ToMemoryStream(".png");
		return (Bitmap)Image.FromStream(stream);
	}

	public byte[] BmpToBytes(Bitmap bmp)
	{
		Bitmap bitmap = VerticalFlip(bmp);
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Bmp);
		return memoryStream.GetBuffer();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public byte[] Mat2Byte(Mat mat)
	{
		return mat.ToBytes(".png");
	}

	public Mat byte2Mat(byte[] buf, int band)
	{
		if (band == 3)
		{
			return Mat.FromImageData(buf);
		}
		return Mat.FromImageData(buf, ImreadModes.Grayscale);
	}

	public static Bitmap VerticalFlip(Bitmap bmp)
	{
		try
		{
			int width = bmp.Width;
			int height = bmp.Height;
			Graphics graphics = Graphics.FromImage(bmp);
			Rectangle rect = new Rectangle(0, 0, width, height);
			bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
			graphics.DrawImage(bmp, rect);
			return bmp;
		}
		catch
		{
			return bmp;
		}
	}

	public static ImageSource BitmapToBitmapImage(Bitmap bitmap)
	{
		BitmapImage bitmapImage = new BitmapImage();
		using MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, bitmap.RawFormat);
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		return bitmapImage;
	}

	public ImageSource MatToBitmapImage(Mat image)
	{
		BitmapImage bitmapImage = new BitmapImage();
		Bitmap bitmap = Mat2Bitmap(image);
		using MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, bitmap.RawFormat);
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		return bitmapImage;
	}
}
