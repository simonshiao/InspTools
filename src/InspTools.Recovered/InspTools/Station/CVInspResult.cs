using System.Collections.Generic;
using InspTools.AOI;

namespace InspTools.Station;

public class CVInspResult
{
	private const int MaxUsedTools = 1000;

	public TOOL_RESULT LocaetResult;

	public FLOW_BIN LocateBIN;

	public FLOW_BIN LocateSizeBIN;

	public int LocaetErrorCount;

	public int LocaetSizeErrorCount;

	public TOOL_RESULT[] VScanResult;

	public FLOW_BIN[] VScanBIN;

	public int[] VScanErrorCount;

	public TOOL_RESULT[] HScanResult;

	public FLOW_BIN[] HScanBIN;

	public int[] HScanErrorCount;

	public TOOL_RESULT[] AdvLocateResult;

	public FLOW_BIN[] AdvLocateBIN;

	public int[] AdvLocateErrorCount;

	public TOOL_RESULT[] MeaWidthResult;

	public FLOW_BIN[] MeaWidthBIN;

	public int[] MeaWidthErrorCount;

	public TOOL_RESULT[] MeaHeightResult;

	public FLOW_BIN[] MeaHeightBIN;

	public int[] MeaHeightErrorCount;

	public TOOL_RESULT[] AdvMeasureResult;

	public FLOW_BIN[] AdvMeasureBIN;

	public int[] AdvMeasureErrorCount;

	public TOOL_RESULT[] BinaryResult;

	public FLOW_BIN[] BinaryBIN;

	public int[] BinaryErrorCount;

	public TOOL_RESULT[] SmoothResult;

	public FLOW_BIN[] SmoothBIN;

	public int[] SmoothErrorCount;

	public TOOL_RESULT[] DifferenceResult;

	public FLOW_BIN[] DifferenceBIN;

	public int[] DifferenceErrorCount;

	public TOOL_RESULT[] BlockAvgResult;

	public FLOW_BIN[] BlockAvgBIN;

	public int[] BlockAvgErrorCount;

	public TOOL_RESULT[] LineAnalysisResult;

	public FLOW_BIN[] LineAnalysisBIN;

	public int[] LineAnalysisErrorCount;

	public TOOL_RESULT[] MatchResult;

	public FLOW_BIN[] MatchBIN;

	public int[] MatchErrorCount;

	public TOOL_RESULT FontToolResult;

	public FLOW_BIN FontToolBIN;

	public int FontToolErrorCount;

	public TOOL_RESULT IRToolResult;

	public FLOW_BIN IRToolBIN;

	public int IRToolErrorCount;

	public TOOL_RESULT AIPredictResult;

	public FLOW_BIN AIPredictBIN;

	public int AIPredictErrorCount;

	public TOOL_RESULT AIProphetResult;

	public FLOW_BIN AIProphetBIN;

	public int AIProphetErrorCount;

	public List<string> ToolNameList;

	public int ToolUsedCount;

	public int[] ToolErrorList = new int[1000];

	public double[] ToolErrorRatioAlarmList = new double[1000];

	private bool inited;

	public CVInspResult()
	{
		VScanResult = new TOOL_RESULT[20];
		VScanBIN = new FLOW_BIN[20];
		VScanErrorCount = new int[20];
		HScanResult = new TOOL_RESULT[20];
		HScanBIN = new FLOW_BIN[20];
		HScanErrorCount = new int[20];
		AdvLocateResult = new TOOL_RESULT[20];
		AdvLocateBIN = new FLOW_BIN[20];
		AdvLocateErrorCount = new int[20];
		AdvMeasureResult = new TOOL_RESULT[40];
		AdvMeasureBIN = new FLOW_BIN[40];
		AdvMeasureErrorCount = new int[40];
		MeaWidthResult = new TOOL_RESULT[20];
		MeaWidthBIN = new FLOW_BIN[20];
		MeaWidthErrorCount = new int[20];
		MeaHeightResult = new TOOL_RESULT[20];
		MeaHeightBIN = new FLOW_BIN[20];
		MeaHeightErrorCount = new int[20];
		BinaryResult = new TOOL_RESULT[30];
		BinaryBIN = new FLOW_BIN[30];
		BinaryErrorCount = new int[30];
		SmoothResult = new TOOL_RESULT[20];
		SmoothBIN = new FLOW_BIN[20];
		SmoothErrorCount = new int[20];
		DifferenceResult = new TOOL_RESULT[20];
		DifferenceBIN = new FLOW_BIN[20];
		DifferenceErrorCount = new int[20];
		BlockAvgResult = new TOOL_RESULT[20];
		BlockAvgBIN = new FLOW_BIN[20];
		BlockAvgErrorCount = new int[20];
		LineAnalysisResult = new TOOL_RESULT[20];
		LineAnalysisBIN = new FLOW_BIN[20];
		LineAnalysisErrorCount = new int[20];
		MatchResult = new TOOL_RESULT[20];
		MatchBIN = new FLOW_BIN[20];
		MatchErrorCount = new int[20];
		FontToolResult = TOOL_RESULT.EM_NOT_USE;
		FontToolBIN = FLOW_BIN.EM_FLOW_RETESET;
		FontToolErrorCount = 0;
		IRToolResult = TOOL_RESULT.EM_NOT_USE;
		IRToolBIN = FLOW_BIN.EM_FLOW_RETESET;
		IRToolErrorCount = 0;
		AIPredictResult = TOOL_RESULT.EM_NOT_USE;
		AIPredictBIN = FLOW_BIN.EM_FLOW_RETESET;
		AIPredictErrorCount = 0;
		AIProphetResult = TOOL_RESULT.EM_NOT_USE;
		AIProphetBIN = FLOW_BIN.EM_FLOW_RETESET;
		AIProphetErrorCount = 0;
		ToolNameList = new List<string>();
		ToolUsedCount = 0;
		for (int i = 0; i < 1000; i++)
		{
			ToolErrorList[i] = 0;
			ToolErrorRatioAlarmList[i] = 0.0;
		}
		LocaetErrorCount = 0;
		LocaetSizeErrorCount = 0;
		for (int j = 0; j < 20; j++)
		{
			VScanErrorCount[j] = 0;
			HScanErrorCount[j] = 0;
			AdvLocateErrorCount[j] = 0;
			MeaWidthErrorCount[j] = 0;
			MeaHeightErrorCount[j] = 0;
			SmoothErrorCount[j] = 0;
			DifferenceErrorCount[j] = 0;
			BlockAvgErrorCount[j] = 0;
			LineAnalysisErrorCount[j] = 0;
			MatchErrorCount[j] = 0;
		}
		for (int k = 0; k < 40; k++)
		{
			AdvMeasureErrorCount[k] = 0;
		}
		for (int l = 0; l < 30; l++)
		{
			BinaryErrorCount[l] = 0;
		}
		inited = true;
	}

	public void CopyResultFrom(CVInspResult SourceResult)
	{
		ToolUsedCount = SourceResult.ToolUsedCount;
		for (int i = 0; i < 1000; i++)
		{
			ToolErrorList[i] = SourceResult.ToolErrorList[i];
			ToolErrorRatioAlarmList[i] = SourceResult.ToolErrorRatioAlarmList[i];
		}
		LocaetErrorCount = SourceResult.LocaetErrorCount;
		LocaetSizeErrorCount = SourceResult.LocaetSizeErrorCount;
		FontToolErrorCount = SourceResult.FontToolErrorCount;
		AIPredictErrorCount = SourceResult.AIPredictErrorCount;
		AIProphetErrorCount = SourceResult.AIProphetErrorCount;
		IRToolErrorCount = SourceResult.IRToolErrorCount;
		for (int j = 0; j < 20; j++)
		{
			VScanErrorCount[j] = SourceResult.VScanErrorCount[j];
			HScanErrorCount[j] = SourceResult.HScanErrorCount[j];
			AdvLocateErrorCount[j] = SourceResult.AdvLocateErrorCount[j];
			MeaHeightErrorCount[j] = SourceResult.MeaHeightErrorCount[j];
			MeaWidthErrorCount[j] = SourceResult.MeaWidthErrorCount[j];
			SmoothErrorCount[j] = SourceResult.SmoothErrorCount[j];
			DifferenceErrorCount[j] = SourceResult.DifferenceErrorCount[j];
			BlockAvgErrorCount[j] = SourceResult.BlockAvgErrorCount[j];
			LineAnalysisErrorCount[j] = SourceResult.LineAnalysisErrorCount[j];
			MatchErrorCount[j] = SourceResult.MatchErrorCount[j];
		}
		for (int k = 0; k < 40; k++)
		{
			AdvMeasureErrorCount[k] = SourceResult.AdvMeasureErrorCount[k];
		}
		for (int l = 0; l < 30; l++)
		{
			BinaryErrorCount[l] = SourceResult.BinaryErrorCount[l];
		}
	}

	public void ResetResult()
	{
		if (inited)
		{
			ToolUsedCount = 0;
			for (int i = 0; i < 1000; i++)
			{
				ToolErrorList[i] = 0;
				ToolErrorRatioAlarmList[i] = 0.0;
			}
			LocaetResult = TOOL_RESULT.EM_OK;
			LocateBIN = FLOW_BIN.EM_FLOW_RETESET;
			LocaetErrorCount = 0;
			LocaetSizeErrorCount = 0;
			FontToolResult = TOOL_RESULT.EM_OK;
			IRToolResult = TOOL_RESULT.EM_OK;
			AIPredictResult = TOOL_RESULT.EM_OK;
			AIProphetResult = TOOL_RESULT.EM_OK;
			FontToolErrorCount = 0;
			AIPredictErrorCount = 0;
			AIProphetErrorCount = 0;
			IRToolErrorCount = 0;
			for (int j = 0; j < 20; j++)
			{
				VScanResult[j] = TOOL_RESULT.EM_OK;
				HScanResult[j] = TOOL_RESULT.EM_OK;
				AdvLocateResult[j] = TOOL_RESULT.EM_OK;
				MeaWidthResult[j] = TOOL_RESULT.EM_OK;
				MeaHeightResult[j] = TOOL_RESULT.EM_OK;
				SmoothResult[j] = TOOL_RESULT.EM_OK;
				DifferenceResult[j] = TOOL_RESULT.EM_OK;
				BlockAvgResult[j] = TOOL_RESULT.EM_OK;
				LineAnalysisResult[j] = TOOL_RESULT.EM_OK;
				MatchResult[j] = TOOL_RESULT.EM_OK;
				VScanErrorCount[j] = 0;
				HScanErrorCount[j] = 0;
				AdvLocateErrorCount[j] = 0;
				MeaHeightErrorCount[j] = 0;
				MeaWidthErrorCount[j] = 0;
				AdvMeasureErrorCount[j] = 0;
				SmoothErrorCount[j] = 0;
				DifferenceErrorCount[j] = 0;
				BlockAvgErrorCount[j] = 0;
				LineAnalysisErrorCount[j] = 0;
				MatchErrorCount[j] = 0;
			}
			for (int k = 0; k < 40; k++)
			{
				AdvMeasureResult[k] = TOOL_RESULT.EM_OK;
			}
			for (int l = 0; l < 30; l++)
			{
				BinaryErrorCount[l] = 0;
				BinaryResult[l] = TOOL_RESULT.EM_OK;
			}
		}
	}
}
