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
using InspTools.AOI;
using InspTools.Other;
using InspTools.Station;
using OpenCvSharp;

namespace InspTools;

public class HistoryViewWindow : System.Windows.Window, IComponentConnector
{
	private const int MaxStation = 8;

	private int MIndex = -1;

	public CVInspection[] Station = new CVInspection[8];

	private TextBox[] ResultTxt;

	private Label[] LabelResult;

	private Canvas[] cnv_Image;

	private double[] xRatio = new double[8];

	private double[] yRatio = new double[8];

	private int[] ImageWidth = new int[8];

	private int[] ImageHeight = new int[8];

	private int[] ImageBand = new int[8];

	private Mat[] DrawMat = new Mat[8];

	private int LastIndex;

	private int CurrentIndex;

	private string DefaultImageID = "";

	internal Grid grd_Station1;

	internal TextBox txt_InspResult1;

	internal Canvas cnv_Image1;

	internal Label Label_Result1;

	internal Label lab_StatinoIndex;

	internal Grid grd_Station2;

	internal TextBox txt_InspResult2;

	internal Canvas cnv_Image2;

	internal Label Label_Result2;

	internal Label lab_StatinoIndex1;

	internal Grid grd_Station3;

	internal TextBox txt_InspResult3;

	internal Canvas cnv_Image3;

	internal Label Label_Result3;

	internal Label lab_StatinoIndex2;

	internal Grid grd_Station4;

	internal TextBox txt_InspResult4;

	internal Canvas cnv_Image4;

	internal Label Label_Result4;

	internal Label lab_StatinoIndex3;

	internal Grid grd_Station5;

	internal TextBox txt_InspResult5;

	internal Canvas cnv_Image5;

	internal Label Label_Result5;

	internal Label lab_StatinoIndex4;

	internal Grid grd_Station6;

	internal TextBox txt_InspResult6;

	internal Canvas cnv_Image6;

	internal Label Label_Result6;

	internal Label lab_StatinoIndex5;

	internal Grid grd_Station7;

	internal TextBox txt_InspResult7;

	internal Canvas cnv_Image7;

	internal Label Label_Result7;

	internal Label lab_StatinoIndex7_Copy;

	internal Grid grd_Station8;

	internal TextBox txt_InspResult8;

	internal Canvas cnv_Image8;

	internal Label Label_Result8;

	internal Label lab_StatinoIndex7;

	internal Button btn_ImageNum;

	internal Button btn_PrevImage;

	internal Button btn_TestAllImage;

	internal Button btn_NextImage;

	internal Label lab_StatinoIndex_Copy;

	internal Ellipse ep_Result;

	internal Label lab_TestResult;

	internal Label Label_ID;

	private bool _contentLoaded;

	public HistoryViewWindow()
	{
		InitializeComponent();
		for (int i = 0; i < 8; i++)
		{
			Station[i] = null;
			xRatio[i] = 1.0;
			yRatio[i] = 1.0;
			ImageWidth[i] = 640;
			ImageHeight[i] = 480;
			ImageBand[i] = 3;
			DrawMat[i] = new Mat();
		}
		LabelResult = new Label[8] { Label_Result1, Label_Result2, Label_Result3, Label_Result4, Label_Result5, Label_Result6, Label_Result7, Label_Result8 };
		ResultTxt = new TextBox[8] { txt_InspResult1, txt_InspResult2, txt_InspResult3, txt_InspResult4, txt_InspResult5, txt_InspResult6, txt_InspResult7, txt_InspResult8 };
		cnv_Image = new Canvas[8] { cnv_Image1, cnv_Image2, cnv_Image3, cnv_Image4, cnv_Image5, cnv_Image6, cnv_Image7, cnv_Image8 };
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetStation(CVInspection[] station, int[] width, int[] height, int[] band, int mindex = -1, int lastIndex = -1)
	{
		MIndex = mindex;
		LastIndex = lastIndex;
		try
		{
			for (int i = 0; i < 8; i++)
			{
				Station[i] = station[i];
				ImageWidth[i] = width[i];
				ImageHeight[i] = height[i];
				ImageBand[i] = band[i];
				if (Station[i].GetStationON())
				{
					LabelResult[i].Visibility = Visibility.Visible;
				}
				else
				{
					LabelResult[i].Visibility = Visibility.Hidden;
				}
				if (MIndex == -1 && Station[i].GetStationON() && Station[i].RecentBufferList != null && Station[i].RecentBufferList.Count > 0)
				{
					MIndex = i;
					LabelResult[i].Visibility = Visibility.Visible;
				}
			}
			if (MIndex < 0)
			{
				return;
			}
			if (LastIndex == -1)
			{
				if (Station[MIndex].RecentBufferList == null)
				{
					LastIndex = -1;
				}
				else if (Station[MIndex].LastIndex > 0 && Station[MIndex].LastIndex < Station[MIndex].RecentBufferList.Count)
				{
					LastIndex = Station[MIndex].LastIndex;
				}
				else
				{
					LastIndex = 0;
				}
			}
			CurrentIndex = LastIndex;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("HW01-HistoryViewWindow-SetStation()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private FLOW_BIN GetStationInspect(int index, int id)
	{
		FLOW_BIN result = FLOW_BIN.EM_FLOW_RETESET;
		if (!Station[index].GetStationON())
		{
			return result;
		}
		try
		{
			Label_ID.Content = id.ToString();
			if (Station[index].RecentBufferIDList != null)
			{
				for (int i = 0; i < Station[index].RecentBufferIDList.Count; i++)
				{
					if (Station[index].RecentBufferIDList[i] != id)
					{
						continue;
					}
					if (i < Station[index].RecentBufferList.Count)
					{
						Station[index].SetBuffers(Station[index].RecentBufferList[i], ImageWidth[index], ImageHeight[index], ImageBand[index]);
						ResultTxt[index].Text = "";
						if (Station[index].buffers.SrcReady)
						{
							result = Inspect(index, Station[index].buffers.Src.Clone());
							ResultTxt[index].Text = Station[index].ErrorMsg;
							return result;
						}
						DrawBlank(index);
						return result;
					}
					DrawBlank(index);
					return result;
				}
			}
			DrawBlank(index);
			return result;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("HW02-HistoryViewWindow-GetStationInspect()-" + ex.ToString());
			return result;
		}
	}

	private FLOW_BIN Inspect(int index, Mat mat)
	{
		INSPRESULT iNSPRESULT = Station[index].Inspect(ref mat, offLine: true);
		DrawImage(index, mat, iNSPRESULT.bin);
		return iNSPRESULT.bin;
	}

	private void DrawBlank(int index)
	{
		cnv_Image[index].Background = Brushes.White;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void DrawImage(int index, Mat mat, FLOW_BIN bin)
	{
		OpenCvSharp.Rect locateImageRect = Station[index].LocateImageRect;
		for (int i = 0; i < Station[index].NGRect.Count; i++)
		{
			if (Station[index].NGRect[i].Width > 0 && Station[index].NGRect[i].Height > 0)
			{
				mat.Rectangle(new OpenCvSharp.Rect
				{
					X = locateImageRect.X + Station[index].NGRect[i].X,
					Y = locateImageRect.Y + Station[index].NGRect[i].Y,
					Width = Station[index].NGRect[i].Width,
					Height = Station[index].NGRect[i].Height
				}, Scalar.Red);
			}
		}
		ImageSource imageSource = Station[index].buffers.MatToBitmapImage(mat);
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = imageSource;
		cnv_Image[index].Background = imageBrush;
		imageSource = null;
		switch (bin)
		{
		case FLOW_BIN.EM_FLOW_OK:
			LabelResult[index].Content = "OK";
			LabelResult[index].Background = Brushes.Lime;
			break;
		case FLOW_BIN.EM_FLOW_NG1:
			LabelResult[index].Content = "NG1";
			LabelResult[index].Background = Brushes.Red;
			break;
		case FLOW_BIN.EM_FLOW_NG2:
			LabelResult[index].Content = "NG2";
			LabelResult[index].Background = Brushes.Red;
			break;
		default:
			LabelResult[index].Content = "Retest";
			LabelResult[index].Background = Brushes.Gray;
			break;
		}
	}

	private void Btn_PrevImage_Click(object sender, RoutedEventArgs e)
	{
		if (MIndex >= 0 && LastIndex >= 0 && Station[MIndex].RecentBufferList != null && Station[MIndex].RecentBufferList.Count > 0)
		{
			if (CurrentIndex - 1 >= 0)
			{
				CurrentIndex--;
			}
			else
			{
				CurrentIndex = Station[MIndex].RecentBufferList.Count - 1;
			}
			RunInspect();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RunInspect(bool firstrun = false)
	{
		if (MIndex < 0 || CurrentIndex < 0 || Station[MIndex].RecentBufferList == null || Station[MIndex].RecentBufferList.Count <= 0)
		{
			return;
		}
		try
		{
			int num = CurrentIndex + 1;
			if (num < 0)
			{
				num = num + Station[MIndex].RecentBufferList.Count + 1;
			}
			btn_ImageNum.Content = string.Format("{0} / {1}", num, Station[MIndex].RecentBufferList.Count);
			FLOW_BIN fLOW_BIN = FLOW_BIN.EM_FLOW_OK;
			for (int i = 0; i < 8; i++)
			{
				if (!Station[i].GetStationON() || Station[i].RecentBufferList == null || Station[i].RecentBufferList.Count <= 0)
				{
					continue;
				}
				if (CurrentIndex < 0 || CurrentIndex > Station[i].RecentBufferIDList.Count)
				{
					return;
				}
				FLOW_BIN stationInspect = GetStationInspect(i, Station[MIndex].RecentBufferIDList[CurrentIndex]);
				switch (stationInspect)
				{
				case FLOW_BIN.EM_FLOW_RETESET:
					fLOW_BIN = stationInspect;
					break;
				case FLOW_BIN.EM_FLOW_NG1:
					if (fLOW_BIN != FLOW_BIN.EM_FLOW_RETESET)
					{
						fLOW_BIN = stationInspect;
					}
					break;
				case FLOW_BIN.EM_FLOW_NG2:
					if (fLOW_BIN != FLOW_BIN.EM_FLOW_RETESET && fLOW_BIN != FLOW_BIN.EM_FLOW_NG1)
					{
						fLOW_BIN = stationInspect;
					}
					break;
				}
			}
			switch (fLOW_BIN)
			{
			case FLOW_BIN.EM_FLOW_OK:
				lab_TestResult.Content = "OK";
				ep_Result.Fill = Brushes.Lime;
				break;
			case FLOW_BIN.EM_FLOW_NG1:
				lab_TestResult.Content = "NG1";
				ep_Result.Fill = Brushes.Red;
				break;
			case FLOW_BIN.EM_FLOW_NG2:
				lab_TestResult.Content = "NG2";
				ep_Result.Fill = Brushes.Red;
				break;
			default:
				lab_TestResult.Content = "Retest";
				ep_Result.Fill = Brushes.Gray;
				break;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("HW01-HistoryViewWindow-RunInspect()-" + ex.ToString());
		}
	}

	private void Btn_TestAllImage_Click(object sender, RoutedEventArgs e)
	{
		RunInspect();
	}

	private void Btn_NextImage_Click(object sender, RoutedEventArgs e)
	{
		if (MIndex >= 0 && CurrentIndex >= 0 && Station[MIndex].RecentBufferList != null && Station[MIndex].RecentBufferList.Count > 0)
		{
			if (CurrentIndex + 1 < Station[MIndex].RecentBufferList.Count)
			{
				CurrentIndex++;
			}
			else
			{
				CurrentIndex = 0;
			}
			RunInspect();
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		RunInspect(firstrun: true);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ImageNum_Click(object sender, RoutedEventArgs e)
	{
		System.Windows.Point point = new System.Windows.Point(btn_ImageNum.Margin.Left, btn_ImageNum.Margin.Top);
		HKeyboard hKeyboard = new HKeyboard();
		int count = Station[MIndex].RecentBufferList.Count;
		string title = string.Format("1~{0}", count);
		hKeyboard.SetTitle(title);
		hKeyboard.SetInput("");
		hKeyboard.SetInputNegative(state: true);
		hKeyboard.SetDouble(state: false);
		hKeyboard.WindowStartupLocation = WindowStartupLocation.Manual;
		hKeyboard.Left = point.X;
		hKeyboard.Top = 450.0;
		hKeyboard.ShowDialog();
		if (!(hKeyboard.GetInput() == ""))
		{
			int num = Convert.ToInt32(hKeyboard.GetInput());
			num--;
			if (num >= 0 && num < count)
			{
				CurrentIndex = num;
				RunInspect();
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
			Uri resourceLocator = new Uri("/InspTools;component/historyviewwindow.xaml", UriKind.Relative);
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
			((HistoryViewWindow)target).Loaded += Window_Loaded;
			break;
		case 2:
			grd_Station1 = (Grid)target;
			break;
		case 3:
			txt_InspResult1 = (TextBox)target;
			break;
		case 4:
			cnv_Image1 = (Canvas)target;
			break;
		case 5:
			Label_Result1 = (Label)target;
			break;
		case 6:
			lab_StatinoIndex = (Label)target;
			break;
		case 7:
			grd_Station2 = (Grid)target;
			break;
		case 8:
			txt_InspResult2 = (TextBox)target;
			break;
		case 9:
			cnv_Image2 = (Canvas)target;
			break;
		case 10:
			Label_Result2 = (Label)target;
			break;
		case 11:
			lab_StatinoIndex1 = (Label)target;
			break;
		case 12:
			grd_Station3 = (Grid)target;
			break;
		case 13:
			txt_InspResult3 = (TextBox)target;
			break;
		case 14:
			cnv_Image3 = (Canvas)target;
			break;
		case 15:
			Label_Result3 = (Label)target;
			break;
		case 16:
			lab_StatinoIndex2 = (Label)target;
			break;
		case 17:
			grd_Station4 = (Grid)target;
			break;
		case 18:
			txt_InspResult4 = (TextBox)target;
			break;
		case 19:
			cnv_Image4 = (Canvas)target;
			break;
		case 20:
			Label_Result4 = (Label)target;
			break;
		case 21:
			lab_StatinoIndex3 = (Label)target;
			break;
		case 22:
			grd_Station5 = (Grid)target;
			break;
		case 23:
			txt_InspResult5 = (TextBox)target;
			break;
		case 24:
			cnv_Image5 = (Canvas)target;
			break;
		case 25:
			Label_Result5 = (Label)target;
			break;
		case 26:
			lab_StatinoIndex4 = (Label)target;
			break;
		case 27:
			grd_Station6 = (Grid)target;
			break;
		case 28:
			txt_InspResult6 = (TextBox)target;
			break;
		case 29:
			cnv_Image6 = (Canvas)target;
			break;
		case 30:
			Label_Result6 = (Label)target;
			break;
		case 31:
			lab_StatinoIndex5 = (Label)target;
			break;
		case 32:
			grd_Station7 = (Grid)target;
			break;
		case 33:
			txt_InspResult7 = (TextBox)target;
			break;
		case 34:
			cnv_Image7 = (Canvas)target;
			break;
		case 35:
			Label_Result7 = (Label)target;
			break;
		case 36:
			lab_StatinoIndex7_Copy = (Label)target;
			break;
		case 37:
			grd_Station8 = (Grid)target;
			break;
		case 38:
			txt_InspResult8 = (TextBox)target;
			break;
		case 39:
			cnv_Image8 = (Canvas)target;
			break;
		case 40:
			Label_Result8 = (Label)target;
			break;
		case 41:
			lab_StatinoIndex7 = (Label)target;
			break;
		case 42:
			btn_ImageNum = (Button)target;
			btn_ImageNum.Click += Btn_ImageNum_Click;
			break;
		case 43:
			btn_PrevImage = (Button)target;
			btn_PrevImage.Click += Btn_PrevImage_Click;
			break;
		case 44:
			btn_TestAllImage = (Button)target;
			btn_TestAllImage.Click += Btn_TestAllImage_Click;
			break;
		case 45:
			btn_NextImage = (Button)target;
			btn_NextImage.Click += Btn_NextImage_Click;
			break;
		case 46:
			lab_StatinoIndex_Copy = (Label)target;
			break;
		case 47:
			ep_Result = (Ellipse)target;
			break;
		case 48:
			lab_TestResult = (Label)target;
			break;
		case 49:
			Label_ID = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
