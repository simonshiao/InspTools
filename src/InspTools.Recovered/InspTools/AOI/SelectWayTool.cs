using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace InspTools.AOI;

public class SelectWayTool
{
	private const int MaxThdTool = 3;

	private Mat[] Binary = new Mat[3];

	private Mat FinalBinary = new Mat();

	private Mat FinalBinary2 = new Mat();

	private CVBuffer buffers;

	public BlobStruct Blobs;

	public BlobStruct Blobs2;

	public bool TwoWayFlip;

	public Rect FoundRect;

	private Mat SearchMat = new Mat();

	private Mat MatchImage = new Mat();

	private Mat FlipMat = new Mat();

	public Mat FoundMat = new Mat();

	public int MatchScore;

	public double AllArea;

	public Rect NGRect;

	private ParamFile pfile;

	public Rect InspeRect;

	public Rect InspeRect2;

	public Rect FoundTextRect = new Rect(0, 0, 0, 0);

	private Stopwatch sw = new Stopwatch();

	private bool OffLine = true;

	public bool SelectWaySuccess { get; set; }

	public string Tracker1ResultText { get; set; }

	public Rect LocateRect { get; set; }

	public Rect LocateImageRect { get; set; }

	public bool PatternFound { get; set; }

	public string ErrorMsg { get; set; }

	public SelectWayTool(ParamFile paramfile)
	{
		pfile = paramfile;
		InspeRect = new Rect(0, 0, 0, 0);
		InspeRect2 = new Rect(0, 0, 0, 0);
		for (int i = 0; i < 3; i++)
		{
			Binary[i] = new Mat();
		}
	}

	public void SetNumThreads(int num)
	{
		Cv2.SetNumThreads(num);
	}

	public void SetUseOptimized(bool enable)
	{
		Cv2.SetUseOptimized(enable);
	}

	public void SetBuffers(CVBuffer bufs)
	{
		buffers = bufs;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool Inspect(bool offline = true)
	{
		OffLine = offline;
		if (pfile.SelectWayParam.SelelctWayFunction == 0)
		{
			try
			{
				if (!pfile.SelectWayParam.Use2WayMode)
				{
					Tracker1ResultText = "";
					TwoWayFlip = false;
					DoThdImage(pfile.SelectWayParam, FinalBinary, isRect2: false);
					return DoFlipCheck(result: SelectWaySuccess = FindBlob(FinalBinary), SuccessDoFlip: pfile.SelectWayParam.SuccessDoFlip, offline: offline, UseRect2: false);
				}
				Tracker1ResultText = "";
				TwoWayFlip = false;
				DoThdImage(pfile.SelectWayParam, FinalBinary, isRect2: false);
				Tracker1ResultText += "[A] ";
				int num = FindBlob2(FinalBinary, isRect2: false);
				if (num == pfile.SelectWayParam.SuccessDoFlip)
				{
					bool result = false;
					if (num == 1)
					{
						result = true;
					}
					TwoWayFlip = false;
					return DoFlipCheck(pfile.SelectWayParam.SuccessDoFlip, result, offline, UseRect2: false);
				}
				DoThdImage(pfile.SelectWayParam, FinalBinary2, isRect2: true);
				Tracker1ResultText += "[B] ";
				int num2 = FindBlob2(FinalBinary2, isRect2: true);
				bool result2 = false;
				if (num2 == 1)
				{
					result2 = true;
				}
				TwoWayFlip = true;
				return DoFlipCheck(pfile.SelectWayParam.SuccessDoFlipB, result2, offline, UseRect2: true);
			}
			catch (Exception ex)
			{
				string text = "SWT03-SelectWayTool-Inspect()";
				text += ex.ToString();
				if (!OffLine)
				{
					CVBuffer.SaveLog(text);
				}
				else
				{
					CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-001");
				}
				SelectWaySuccess = false;
				return false;
			}
		}
		return MatchInspect(offline);
	}

	public bool DoFlipCheck(int SuccessDoFlip, bool result, bool offline, bool UseRect2)
	{
		bool flag = false;
		if (SuccessDoFlip == 0)
		{
			if (result)
			{
				return false;
			}
			if (!UseRect2)
			{
				buffers.SetSelectWayFlip(pfile.SelectWayParam.RotateMethod, offline);
			}
			else
			{
				buffers.SetSelectWayFlip(pfile.SelectWayParam.RotateMethodB, offline);
			}
			return true;
		}
		if (result)
		{
			if (!UseRect2)
			{
				buffers.SetSelectWayFlip(pfile.SelectWayParam.RotateMethod, offline);
			}
			else
			{
				buffers.SetSelectWayFlip(pfile.SelectWayParam.RotateMethodB, offline);
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool MatchInspect(bool offline = false)
	{
		Tracker1ResultText = "";
		bool result = false;
		MatchScore = 0;
		PatternFound = false;
		try
		{
			if (pfile.SelectWayParam.PatternSelect < 0)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("選面工具使用圖形比對錯誤，圖形資料庫索引錯誤，請重新選擇比對圖形。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("选面工具使用图形比对错误，图形资料库索引错误，请重新选择比对图形。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("選択面ツールによる画像比較エラー、画像データベースインデックスエラーが発生しました。比較画像を再度選択してください。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("SelectWay tool Match Error, Pattern file index Error, Select Pattern file again.\n");
				}
				SelectWaySuccess = false;
				return result;
			}
			if (pfile.patternDB[pfile.SelectWayParam.PatternSelect].PatternImage.Empty() || !pfile.patternDB[pfile.SelectWayParam.PatternSelect].PatternON)
			{
				if (buffers.LanguageType == 0)
				{
					Tracker1ResultText += string.Format("SelectWay tool Match Error, Missing Pattern file.\n");
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("選面工具使用圖形比對錯誤，遺失比對圖形。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("选面工具使用图形比对错误，遗失比对图形。\n");
				}
				SelectWaySuccess = false;
				return result;
			}
		}
		catch (Exception ex)
		{
			string text = "MAT01-SelectWayTool-MatchInspect()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-002");
			}
			SelectWaySuccess = false;
			return false;
		}
		try
		{
			GetInspeRect(pfile.SelectWayParam);
			pfile.SelectWayParam.FoundTextRectL = 0;
			pfile.SelectWayParam.FoundTextRectT = 0;
			pfile.SelectWayParam.FoundTextRectW = 0;
			pfile.SelectWayParam.FoundTextRectH = 0;
			int patternSelect = pfile.SelectWayParam.PatternSelect;
			int patternChannel = pfile.patternDB[patternSelect].PatternChannel;
			Mat patternImage = pfile.patternDB[pfile.SelectWayParam.PatternSelect].PatternImage;
			InspeRect &= new Rect(1, 1, buffers.GetChannelMat(patternChannel).Cols - 1, buffers.GetChannelMat(patternChannel).Rows - 1);
			new Mat(buffers.GetChannelMat(patternChannel), InspeRect).CopyTo(SearchMat);
			if (SearchMat.Width < patternImage.Width || SearchMat.Height < patternImage.Height)
			{
				FoundTextRect.Width = (FoundTextRect.Height = 0);
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("選面工具使用圖形比對錯誤，搜尋區域寬高小於比對圖形。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("选面工具使用图形比对错误，搜寻区域宽高小于比对图形。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("選択面ツールで画像比較エラーが発生しました。検索領域の幅または高さが比較画像よりも小さいです。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("SelectWay tool Match Error，Search area is smaller than pattern.\n");
				}
				SelectWaySuccess = false;
				return false;
			}
			Cv2.MatchTemplate(SearchMat, patternImage, MatchImage, TemplateMatchModes.CCoeffNormed);
			Cv2.MinMaxLoc(MatchImage, out var minVal, out var maxVal, out var minLoc, out var maxLoc);
			Rect rect = new Rect(maxLoc.X, maxLoc.Y, patternImage.Cols, patternImage.Rows);
			rect &= new Rect(0, 0, buffers.GetChannelMat(patternChannel).Cols, buffers.GetChannelMat(patternChannel).Rows);
			if (rect.Width != patternImage.Width || rect.Height != patternImage.Height)
			{
				FoundTextRect.Width = (FoundTextRect.Height = 0);
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("選面工具使用圖形比對錯誤，找不到符合的圖形。\n");
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("选面工具使用图形比对错误，找不到符合的图形。\n");
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("選択面ツールがグラフィック比較でエラー。一致するグラフィックが見つかりません。\n");
				}
				else
				{
					Tracker1ResultText += string.Format("SelectWay tool Match Error，Can't find any match area.\n");
				}
				SelectWaySuccess = false;
				return DoFlipCheck(pfile.SelectWayParam.SuccessDoFlip, result: false, offline, UseRect2: false);
			}
			new Mat(SearchMat, rect).CopyTo(FoundMat);
			MatchScore = (int)(maxVal * 100.0);
			if (MatchScore > 100)
			{
				MatchScore = 100;
			}
			if (MatchScore < 0)
			{
				MatchScore = 0;
			}
			Cv2.Flip(patternImage, FlipMat, FlipMode.XY);
			Cv2.MatchTemplate(SearchMat, FlipMat, MatchImage, TemplateMatchModes.CCoeffNormed);
			Cv2.MinMaxLoc(MatchImage, out minVal, out maxVal, out minLoc, out maxLoc);
			Rect rect2 = default(Rect);
			rect2 = new Rect(maxLoc.X, maxLoc.Y, patternImage.Cols, patternImage.Rows);
			rect2 &= new Rect(0, 0, buffers.GetChannelMat(patternChannel).Cols, buffers.GetChannelMat(patternChannel).Rows);
			int num = (int)(maxVal * 100.0);
			if (num > 100)
			{
				num = 100;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num < pfile.SelectWayParam.MatchScoreMin && MatchScore < pfile.SelectWayParam.MatchScoreMin)
			{
				if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("選面工具使用圖形比對失敗，圖形比對分數={0}。\n", MatchScore);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("选面工具使用图形比对失败，图形比对分数={0}。\n", MatchScore);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("選択面ツールによる画像比較に失敗しました。画像比較スコア={0}。\n", MatchScore);
				}
				else
				{
					Tracker1ResultText += string.Format("SelectWay tool Match error，Match score ={0}。\n", MatchScore);
				}
				SelectWaySuccess = false;
				return DoFlipCheck(pfile.SelectWayParam.SuccessDoFlip, result: false, offline, UseRect2: false);
			}
			if (num > MatchScore)
			{
				MatchScore = num;
				SelectWaySuccess = true;
				FoundRect = rect2;
				FoundRect.Left = rect2.Left + InspeRect.Left;
				FoundRect.Top = rect2.Top + InspeRect.Top;
				FoundTextRect = FoundRect;
				pfile.SelectWayParam.FoundTextRectL = FoundTextRect.Left;
				pfile.SelectWayParam.FoundTextRectT = FoundTextRect.Top;
				pfile.SelectWayParam.FoundTextRectW = FoundTextRect.Width;
				pfile.SelectWayParam.FoundTextRectH = FoundTextRect.Height;
				PatternFound = true;
				return DoFlipCheck(pfile.SelectWayParam.SuccessDoFlip, result: false, offline, UseRect2: false);
			}
			FoundRect = rect;
			FoundRect.Left = rect.Left + InspeRect.Left;
			FoundRect.Top = rect.Top + InspeRect.Top;
			FoundTextRect = FoundRect;
			pfile.SelectWayParam.FoundTextRectL = FoundTextRect.Left;
			pfile.SelectWayParam.FoundTextRectT = FoundTextRect.Top;
			pfile.SelectWayParam.FoundTextRectW = FoundTextRect.Width;
			pfile.SelectWayParam.FoundTextRectH = FoundTextRect.Height;
			PatternFound = true;
			SelectWaySuccess = true;
			return DoFlipCheck(pfile.SelectWayParam.SuccessDoFlip, result: true, offline, UseRect2: false);
		}
		catch (Exception ex2)
		{
			string text2 = "MAT02-SelectWayTool-MatchInspect()-";
			text2 += ex2.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text2);
			}
			else
			{
				CVBuffer.SaveLog(text2, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-003");
			}
			SelectWaySuccess = false;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool DoThdImage(INSP_SELECTWAYTOOL_PARAM param, Mat finalbinary, bool isRect2)
	{
		try
		{
			bool[] array = new bool[3];
			Rect roi;
			if (isRect2)
			{
				GetInspeRect2(param);
				roi = InspeRect2;
			}
			else
			{
				GetInspeRect(param);
				roi = InspeRect;
			}
			roi &= new Rect(1, 1, buffers.GetChannelMat(0).Cols - 1, buffers.GetChannelMat(0).Rows - 1);
			if (roi.Width <= 0 || roi.Height <= 0)
			{
				return true;
			}
			int num = 0;
			if (param.ColorReferenceIndex <= 0)
			{
				if (param.THD1_ChannelON)
				{
					new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), roi).CopyTo(Binary[0]);
					Cv2.InRange(Binary[0], param.THD1_Min, param.THD1_Max, Binary[0]);
					DoMorph(ref Binary[0], (int)param.DE_Start1_Index, param.DE_Start1_Count);
					DoMorph(ref Binary[0], (int)param.DE_End1_Index, param.DE_End1_Count);
					num++;
					array[0] = true;
				}
				else
				{
					array[0] = false;
				}
				if (param.THD2_ChannelON)
				{
					new Mat(buffers.GetChannelMat((int)param.THD2_ChannelIndex), roi).CopyTo(Binary[1]);
					Cv2.InRange(Binary[1], param.THD2_Min, param.THD2_Max, Binary[1]);
					DoMorph(ref Binary[1], (int)param.DE_Start2_Index, param.DE_Start2_Count);
					DoMorph(ref Binary[1], (int)param.DE_End2_Index, param.DE_End2_Count);
					num++;
					array[1] = true;
				}
				else
				{
					array[1] = false;
				}
				if (param.THD3_ChannelON)
				{
					new Mat(buffers.GetChannelMat((int)param.THD3_ChannelIndex), roi).CopyTo(Binary[2]);
					Cv2.InRange(Binary[2], param.THD3_Min, param.THD3_Max, Binary[2]);
					DoMorph(ref Binary[2], (int)param.DE_Start3_Index, param.DE_Start3_Count);
					DoMorph(ref Binary[2], (int)param.DE_End3_Index, param.DE_End3_Count);
					num++;
					array[2] = true;
				}
				else
				{
					array[2] = false;
				}
			}
			else
			{
				int num2 = param.ColorReferenceIndex - 1;
				INSP_COLORREFERENCE_PARAM iNSP_COLORREFERENCE_PARAM = pfile.ColorReferenceParam[num2];
				if (iNSP_COLORREFERENCE_PARAM.THD1_ChannelON)
				{
					new Mat(buffers.GetChannelMat((int)iNSP_COLORREFERENCE_PARAM.THD1_ChannelIndex), roi).CopyTo(Binary[0]);
					Cv2.InRange(Binary[0], iNSP_COLORREFERENCE_PARAM.THD1_Min, iNSP_COLORREFERENCE_PARAM.THD1_Max, Binary[0]);
					DoMorph(ref Binary[0], (int)iNSP_COLORREFERENCE_PARAM.DE_Start1_Index, iNSP_COLORREFERENCE_PARAM.DE_Start1_Count);
					DoMorph(ref Binary[0], (int)iNSP_COLORREFERENCE_PARAM.DE_End1_Index, iNSP_COLORREFERENCE_PARAM.DE_End1_Count);
					num++;
					array[0] = true;
				}
				else
				{
					array[0] = false;
				}
				if (iNSP_COLORREFERENCE_PARAM.THD2_ChannelON)
				{
					new Mat(buffers.GetChannelMat((int)iNSP_COLORREFERENCE_PARAM.THD2_ChannelIndex), roi).CopyTo(Binary[1]);
					Cv2.InRange(Binary[1], iNSP_COLORREFERENCE_PARAM.THD2_Min, iNSP_COLORREFERENCE_PARAM.THD2_Max, Binary[1]);
					DoMorph(ref Binary[1], (int)iNSP_COLORREFERENCE_PARAM.DE_Start2_Index, iNSP_COLORREFERENCE_PARAM.DE_Start2_Count);
					DoMorph(ref Binary[1], (int)iNSP_COLORREFERENCE_PARAM.DE_End2_Index, iNSP_COLORREFERENCE_PARAM.DE_End2_Count);
					num++;
					array[1] = true;
				}
				else
				{
					array[1] = false;
				}
				if (iNSP_COLORREFERENCE_PARAM.THD3_ChannelON)
				{
					new Mat(buffers.GetChannelMat((int)iNSP_COLORREFERENCE_PARAM.THD3_ChannelIndex), roi).CopyTo(Binary[2]);
					Cv2.InRange(Binary[2], iNSP_COLORREFERENCE_PARAM.THD3_Min, iNSP_COLORREFERENCE_PARAM.THD3_Max, Binary[2]);
					DoMorph(ref Binary[2], (int)iNSP_COLORREFERENCE_PARAM.DE_Start3_Index, iNSP_COLORREFERENCE_PARAM.DE_Start3_Count);
					DoMorph(ref Binary[2], (int)iNSP_COLORREFERENCE_PARAM.DE_End3_Index, iNSP_COLORREFERENCE_PARAM.DE_End3_Count);
					num++;
					array[2] = true;
				}
				else
				{
					array[2] = false;
				}
			}
			if (num == 0)
			{
				return true;
			}
			new Mat(buffers.GetChannelMat((int)param.THD1_ChannelIndex), roi).CopyTo(finalbinary);
			finalbinary.SetTo(255.0);
			for (int i = 0; i < 3; i++)
			{
				if (array[i])
				{
					Cv2.BitwiseAnd(finalbinary, Binary[i], finalbinary);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			string text = "SWT04-SelectWayTool-DoThdImage()";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-004");
			}
			return false;
		}
	}

	public void GetInspeRect(INSP_SELECTWAYTOOL_PARAM param)
	{
		LocateRect = buffers.LocatedRectTmp;
		InspeRect.X = LocateRect.X + param.SearchRectL;
		InspeRect.Y = LocateRect.Y + param.SearchRectT;
		if (param.UseAutoSize)
		{
			InspeRect.Width = LocateRect.Width - (param.SearchRectL + param.SearchRectR);
			InspeRect.Height = LocateRect.Height - (param.SearchRectT + param.SearchRectB);
		}
		else
		{
			InspeRect.Width = param.SearchRectWidth;
			InspeRect.Height = param.SearchRectHeight;
		}
	}

	public void GetInspeRect2(INSP_SELECTWAYTOOL_PARAM param)
	{
		LocateRect = buffers.LocatedRectTmp;
		InspeRect2.X = LocateRect.X + param.SearchRect2L;
		InspeRect2.Y = LocateRect.Y + param.SearchRect2T;
		if (param.UseAutoSize)
		{
			InspeRect2.Width = LocateRect.Width - (param.SearchRect2L + param.SearchRect2R);
			InspeRect2.Height = LocateRect.Height - (param.SearchRect2T + param.SearchRect2B);
		}
		else
		{
			InspeRect2.Width = param.SearchRect2Width;
			InspeRect2.Height = param.SearchRect2Height;
		}
	}

	private void DoMorph(ref Mat mat, int index, int count)
	{
		if (count != 0)
		{
			switch (index)
			{
			case 1:
				Cv2.Dilate(mat, mat, new Mat(), null, count);
				break;
			case 2:
				Cv2.Erode(mat, mat, new Mat(), null, count);
				break;
			case 3:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Open, new Mat(), null, count);
				break;
			case 4:
				Cv2.MorphologyEx(mat, mat, MorphTypes.Close, new Mat(), null, count);
				break;
			case 5:
			{
				Mat structuringElement4 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, count));
				Cv2.Dilate(mat, mat, structuringElement4, null, count);
				break;
			}
			case 6:
			{
				Mat structuringElement3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, count));
				Cv2.Erode(mat, mat, structuringElement3, null, count);
				break;
			}
			case 7:
			{
				Mat structuringElement2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(count, 1));
				Cv2.Dilate(mat, mat, structuringElement2, null, count);
				break;
			}
			case 8:
			{
				Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(count, 1));
				Cv2.Erode(mat, mat, structuringElement, null, count);
				break;
			}
			case 0:
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool FindBlob(Mat binary)
	{
		try
		{
			AllArea = 0.0;
			int num = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			if (binary.CountNonZero() <= 3)
			{
				num = 0;
				Blobs.Width = rect.Width;
				Blobs.Height = rect.Height;
				Blobs.WHRatio = 0.0;
				Blobs.Area = 0.0;
				Blobs.NGRect = rect;
				Blobs.NGRect.X += InspeRect.X;
				Blobs.NGRect.Y += InspeRect.Y;
				NGRect = Blobs.NGRect;
				return CheckResult(rect, num, 0.0);
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num2 = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			AllArea = (double)binary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num3 = 0;
			int i = 0;
			if (num2 > 1)
			{
				for (int j = 1; j < num2; j++)
				{
					if (genericIndexer[j, 4] > num3)
					{
						i = j;
						num3 = genericIndexer[j, 4];
					}
				}
				rect = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
				num = num3;
			}
			else
			{
				rect = new Rect(0, 0, 0, 0);
				num = num3;
			}
			double num4 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			Blobs.Width = rect.Width;
			Blobs.Height = rect.Height;
			Blobs.WHRatio = num4;
			Blobs.Area = num3;
			Blobs.NGRect = rect;
			Blobs.NGRect.X += InspeRect.X;
			Blobs.NGRect.Y += InspeRect.Y;
			NGRect = Blobs.NGRect;
			return CheckResult(rect, num, num4);
		}
		catch (Exception ex)
		{
			string text = "BT02-SelectWayTool-FindBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-005");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int FindBlob2(Mat binary, bool isRect2)
	{
		try
		{
			AllArea = 0.0;
			int num = 0;
			Rect rect = new Rect(0, 0, 0, 0);
			Rect rect2 = new Rect(0, 0, 0, 0);
			rect2 = ((!isRect2) ? InspeRect : InspeRect2);
			if (binary.CountNonZero() <= 3)
			{
				num = 0;
				if (!isRect2)
				{
					Blobs.Width = rect.Width;
					Blobs.Height = rect.Height;
					Blobs.WHRatio = 0.0;
					Blobs.Area = 0.0;
					Blobs.NGRect = rect;
					Blobs.NGRect.X += rect2.X;
					Blobs.NGRect.Y += rect2.Y;
					NGRect = Blobs.NGRect;
				}
				else
				{
					Blobs2.Width = rect.Width;
					Blobs2.Height = rect.Height;
					Blobs2.WHRatio = 0.0;
					Blobs2.Area = 0.0;
					Blobs2.NGRect = rect;
					Blobs2.NGRect.X += rect2.X;
					Blobs2.NGRect.Y += rect2.Y;
					NGRect = Blobs2.NGRect;
				}
				return CheckResultTwo(rect, num, 0.0, isRect2);
			}
			Mat mat = new Mat();
			Mat mat2 = new Mat();
			Mat mat3 = new Mat();
			int num2 = Cv2.ConnectedComponentsWithStats(binary, mat, mat2, mat3, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
			AllArea = (double)binary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
			Mat.Indexer<int> genericIndexer = mat2.GetGenericIndexer<int>();
			int num3 = 0;
			int i = 0;
			if (num2 > 1)
			{
				for (int j = 1; j < num2; j++)
				{
					if (genericIndexer[j, 4] > num3)
					{
						i = j;
						num3 = genericIndexer[j, 4];
					}
				}
				rect = new Rect(genericIndexer[i, 0], genericIndexer[i, 1], genericIndexer[i, 2], genericIndexer[i, 3]);
				num = num3;
			}
			else
			{
				rect = new Rect(0, 0, 0, 0);
				num = num3;
			}
			double num4 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
			if (!isRect2)
			{
				Blobs.Width = rect.Width;
				Blobs.Height = rect.Height;
				Blobs.WHRatio = num4;
				Blobs.Area = num3;
				Blobs.NGRect = rect;
				Blobs.NGRect.X += rect2.X;
				Blobs.NGRect.Y += rect2.Y;
				NGRect = Blobs.NGRect;
			}
			else
			{
				Blobs2.Width = rect.Width;
				Blobs2.Height = rect.Height;
				Blobs2.WHRatio = num4;
				Blobs2.Area = num3;
				Blobs2.NGRect = rect;
				Blobs2.NGRect.X += rect2.X;
				Blobs2.NGRect.Y += rect2.Y;
				NGRect = Blobs2.NGRect;
			}
			return CheckResultTwo(rect, num, num4, isRect2);
		}
		catch (Exception ex)
		{
			string text = "BT02-SelectWayTool-FindBlob()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-006");
			}
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CheckResult(Rect rect, int area, double whratio)
	{
		try
		{
			bool[] array = new bool[4];
			bool flag = true;
			array[0] = GetRule((int)pfile.SelectWayParam.InspRule1_Index, pfile.SelectWayParam.InspRule1_Min, pfile.SelectWayParam.InspRule1_Max, rect, area);
			array[1] = GetRule((int)pfile.SelectWayParam.InspRule2_Index, pfile.SelectWayParam.InspRule2_Min, pfile.SelectWayParam.InspRule2_Max, rect, area);
			array[2] = GetRule((int)pfile.SelectWayParam.InspRule3_Index, pfile.SelectWayParam.InspRule3_Min, pfile.SelectWayParam.InspRule3_Max, rect, area);
			array[3] = GetRule((int)pfile.SelectWayParam.InspRule4_Index, pfile.SelectWayParam.InspRule4_Min, pfile.SelectWayParam.InspRule4_Max, rect, area);
			if (pfile.SelectWayParam.InspRuleAND)
			{
				flag = ((array[0] && array[1] && array[2] && array[3]) ? true : false);
			}
			else
			{
				flag = false;
				if (array[0] && pfile.SelectWayParam.InspRule1_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[1] && pfile.SelectWayParam.InspRule2_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[2] && pfile.SelectWayParam.InspRule3_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
				if (array[3] && pfile.SelectWayParam.InspRule4_Index > BlobCondition.EM_NOT_USE)
				{
					flag = true;
				}
			}
			Blobs.result = flag;
			if (area == 0 || rect.Width <= 0 || rect.Height <= 0)
			{
				Blobs.AreaRatio = 0.0;
			}
			else
			{
				Blobs.AreaRatio = (double)area / (double)(rect.Width * rect.Height);
			}
			if (pfile.MeasureUnit == 0)
			{
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText = string.Format("寬:{0:N3}um, 高:{1:N3}um, 面積:{2:N3}um, 長寬比:{3:N3}, 面積比:{4:N3}, 所有面積:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText = string.Format("宽:{0:N3}um, 高:{1:N3}um, 面积:{2:N3}um, 长宽比:{3:N3}, 面积比:{4:N3}, 所有面积:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText = string.Format("幅:{0:N3}um, 高:{1:N3}um, 面積:{2:N3}um, アスペクト比:{3:N3}, 面積比:{4:N3}, 全面積:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
					else
					{
						Tracker1ResultText = string.Format("Width:{0:N3}um, Height:{1:N3}um, Area:{2:N3}um, WH Ratio:{3:N3}, Area Ratio:{4:N3}, AllArea:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText = string.Format("寬:{0}um, 高:{1}um, 面積:{2}um, 長寬比:{3:N3}, 面積比:{4:N3}, 所有面積:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText = string.Format("宽:{0}um, 高:{1}um, 面积:{2}um, 长宽比:{3:N3}, 面积比:{4:N3}, 所有面积:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText = string.Format("幅:{0}um, 高:{1}um, 面積:{2}um, アスペクト比:{3:N3}, 面積比:{4:N3}, 全面積:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
				else
				{
					Tracker1ResultText = string.Format("Width:{0}um, Height:{1}um, Area:{2}um, WH Ratio:{3:N3}, Area Ratio:{4:N3}, AllArea:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, Blobs.AreaRatio, (int)AllArea);
				}
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker1ResultText = string.Format("寬:{0:N3}mm, 高:{1:N3}mm, 面積:{2:N3}mm, 長寬比:{3:N3}, 面積比:{4:N3}, 所有面積:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText = string.Format("宽:{0:N3}mm, 高:{1:N3}mm, 面积:{2:N3}mm, 长宽比:{3:N3}, 面积比:{4:N3}, 所有面积:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText = string.Format("幅:{0:N3}mm, 高:{1:N3}mm, 面積:{2:N3}mm², アスペクト比:{3:N3}, 面積比:{4:N3}, 総面積:{5:N3}mm² \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			else
			{
				Tracker1ResultText = string.Format("Width:{0:N3}mm, Height:{1:N3}mm, Area:{2:N3}mm, WH Ratio:{3:N3}, Area Ratio:{4:N3}, AllArea:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, Blobs.AreaRatio, AllArea);
			}
			return flag;
		}
		catch (Exception ex)
		{
			string text = "BT03-SelectWayTool-CheckResult()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-007");
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private int CheckResultTwo(Rect rect, int area, double whratio, bool isRect2)
	{
		try
		{
			bool[] array = new bool[2];
			bool flag = true;
			BlobStruct blobStruct = default(BlobStruct);
			if (!isRect2)
			{
				array[0] = GetRule((int)pfile.SelectWayParam.InspRule1_Index, pfile.SelectWayParam.InspRule1_Min, pfile.SelectWayParam.InspRule1_Max, rect, area);
				array[1] = GetRule((int)pfile.SelectWayParam.InspRule2_Index, pfile.SelectWayParam.InspRule2_Min, pfile.SelectWayParam.InspRule2_Max, rect, area);
				blobStruct = Blobs;
			}
			else
			{
				array[0] = GetRule((int)pfile.SelectWayParam.InspRule3_Index, pfile.SelectWayParam.InspRule3_Min, pfile.SelectWayParam.InspRule3_Max, rect, area);
				array[1] = GetRule((int)pfile.SelectWayParam.InspRule4_Index, pfile.SelectWayParam.InspRule4_Min, pfile.SelectWayParam.InspRule4_Max, rect, area);
				blobStruct = Blobs2;
			}
			if (pfile.SelectWayParam.InspRuleAND)
			{
				flag = ((array[0] && array[1]) ? true : false);
			}
			else
			{
				flag = false;
				if (!isRect2)
				{
					if (array[0] && pfile.SelectWayParam.InspRule1_Index > BlobCondition.EM_NOT_USE)
					{
						flag = true;
					}
					if (array[1] && pfile.SelectWayParam.InspRule2_Index > BlobCondition.EM_NOT_USE)
					{
						flag = true;
					}
				}
				else
				{
					if (array[0] && pfile.SelectWayParam.InspRule3_Index > BlobCondition.EM_NOT_USE)
					{
						flag = true;
					}
					if (array[1] && pfile.SelectWayParam.InspRule4_Index > BlobCondition.EM_NOT_USE)
					{
						flag = true;
					}
				}
			}
			blobStruct.result = flag;
			if (area == 0 || rect.Width <= 0 || rect.Height <= 0)
			{
				blobStruct.AreaRatio = 0.0;
			}
			else
			{
				blobStruct.AreaRatio = (double)area / (double)(rect.Width * rect.Height);
			}
			if (pfile.MeasureUnit == 0)
			{
				if (pfile.UseFloat)
				{
					if (buffers.LanguageType == 1)
					{
						Tracker1ResultText += string.Format("寬:{0:N3}um, 高:{1:N3}um, 面積:{2:N3}um, 長寬比:{3:N3}, 面積比:{4:N3}, 所有面積:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
					}
					else if (buffers.LanguageType == 2)
					{
						Tracker1ResultText += string.Format("宽:{0:N3}um, 高:{1:N3}um, 面积:{2:N3}um, 长宽比:{3:N3}, 面积比:{4:N3}, 所有面积:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
					}
					else if (buffers.LanguageType == 3)
					{
						Tracker1ResultText += string.Format("幅:{0:N3}um, 高:{1:N3}um, 面積:{2:N3}um, アスペクト比:{3:N3}, 面積比:{4:N3}, 全面積:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
					}
					else
					{
						Tracker1ResultText += string.Format("Width:{0:N3}um, Height:{1:N3}um, Area:{2:N3}um, WH Ratio:{3:N3}, Area Ratio:{4:N3}, AllArea:{5:N3}um \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
					}
				}
				else if (buffers.LanguageType == 1)
				{
					Tracker1ResultText += string.Format("寬:{0}um, 高:{1}um, 面積:{2}um, 長寬比:{3:N3}, 面積比:{4:N3}, 所有面積:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, blobStruct.AreaRatio, (int)AllArea);
				}
				else if (buffers.LanguageType == 2)
				{
					Tracker1ResultText += string.Format("宽:{0}um, 高:{1}um, 面积:{2}um, 长宽比:{3:N3}, 面积比:{4:N3}, 所有面积:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, blobStruct.AreaRatio, (int)AllArea);
				}
				else if (buffers.LanguageType == 3)
				{
					Tracker1ResultText += string.Format("幅:{0}um, 高:{1}um, 面積:{2}um, アスペクト比:{3:N3}, 面積比:{4:N3}, 全面積:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, blobStruct.AreaRatio, (int)AllArea);
				}
				else
				{
					Tracker1ResultText += string.Format("Width:{0}um, Height:{1}um, Area:{2}um, WH Ratio:{3:N3}, Area Ratio:{4:N3}, AllArea:{5}um \n", (int)((double)rect.Width * pfile.PixelPerUM_X), (int)((double)rect.Height * pfile.PixelPerUM_Y), (int)((double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y), whratio, blobStruct.AreaRatio, (int)AllArea);
				}
			}
			else if (buffers.LanguageType == 1)
			{
				Tracker1ResultText += string.Format("寬:{0:N3}mm, 高:{1:N3}mm, 面積:{2:N3}mm, 長寬比:{3:N3}, 面積比:{4:N3}, 所有面積:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
			}
			else if (buffers.LanguageType == 2)
			{
				Tracker1ResultText += string.Format("宽:{0:N3}mm, 高:{1:N3}mm, 面积:{2:N3}mm, 长宽比:{3:N3}, 面积比:{4:N3}, 所有面积:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
			}
			else if (buffers.LanguageType == 3)
			{
				Tracker1ResultText += string.Format("幅:{0:N3}mm, 高:{1:N3}mm, 面積:{2:N3}mm, アスペクト比:{3:N3}, 面積比:{4:N3}, 全面積:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
			}
			else
			{
				Tracker1ResultText += string.Format("Width:{0:N3}mm, Height:{1:N3}mm, Area:{2:N3}mm, WH Ratio:{3:N3}, Area Ratio:{4:N3}, AllArea:{5:N3}mm \n", (double)rect.Width * pfile.PixelPerUM_X, (double)rect.Height * pfile.PixelPerUM_Y, (double)area * pfile.PixelPerUM_X * pfile.PixelPerUM_Y, whratio, blobStruct.AreaRatio, AllArea);
			}
			if (!isRect2)
			{
				Blobs = blobStruct;
			}
			else
			{
				Blobs2 = blobStruct;
			}
			if (flag)
			{
				return 1;
			}
			return 0;
		}
		catch (Exception ex)
		{
			string text = "BT03-SelectWayTool-CheckResult()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-008");
			}
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool GetRule(int rule, double min, double max, Rect rect, double area)
	{
		try
		{
			bool result = true;
			switch (rule)
			{
			case 0:
				return true;
			case 1:
			{
				double num = (double)rect.Width * pfile.PixelPerUM_X;
				if (num < min || num > max)
				{
					result = false;
				}
				break;
			}
			case 2:
			{
				double num4 = (double)rect.Height * pfile.PixelPerUM_Y;
				if (num4 < min || num4 > max)
				{
					result = false;
				}
				break;
			}
			case 3:
			{
				double num2 = area * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				if (num2 < min || num2 > max)
				{
					result = false;
				}
				break;
			}
			case 4:
			{
				double num3 = ((rect.Width == 0 || rect.Height == 0) ? 0.0 : ((rect.Width <= rect.Height) ? ((double)rect.Height / (double)rect.Width) : ((double)rect.Width / (double)rect.Height)));
				if (num3 < min || num3 > max)
				{
					result = false;
				}
				break;
			}
			case 5:
			{
				double areaRatio = 0.0;
				Blobs.AreaRatio = areaRatio;
				if ((double)rect.Width * (double)rect.Height <= 0.0)
				{
					Blobs.AreaRatio = areaRatio;
					result = true;
				}
				else
				{
					areaRatio = area / (double)(rect.Width * rect.Height);
					Blobs.AreaRatio = areaRatio;
					result = ((!(areaRatio < min) && !(areaRatio > max)) ? true : false);
				}
				break;
			}
			case 6:
				AllArea = (double)FinalBinary.CountNonZero() * pfile.PixelPerUM_Y * pfile.PixelPerUM_X;
				result = ((!(AllArea < min) && !(AllArea > max)) ? true : false);
				break;
			default:
				return true;
			}
			return result;
		}
		catch (Exception ex)
		{
			string text = "BT04-SelectWayTool-GetRule()-";
			text += ex.ToString();
			if (!OffLine)
			{
				CVBuffer.SaveLog(text);
			}
			else
			{
				CVBuffer.SaveLog(text, pfile.GetStationIndex(), pfile.GetRecipeName(), buffers.Dst, "SelectWay-009");
			}
			return false;
		}
	}
}
