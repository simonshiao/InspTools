using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InspTools.AOI;
using InspTools.Station;

namespace InspTools.Other;

internal class SizeStatistics
{
	private CVInspection[] Station;

	private CVInspection StationL;

	private CVInspection StationW;

	private CVInspection StationT;

	private CVInspection StationLPad;

	private CVInspection StationRPad;

	private CVInspection StationGap;

	private CVInspection StationC1;

	private CVInspection StationC2;

	private CVInspection StationC3;

	private CVInspection StationC4;

	private CVInspection StationC5;

	private CVInspection StationC6;

	private CVInspection StationC7;

	private CVInspection StationC8;

	private CVInspection StationC9;

	private CVInspection StationC10;

	private CVInspection StationC11;

	private CVInspection StationC12;

	private CVInspection StationC13;

	private CVInspection StationC14;

	private TOOLSIZERECORDSETUP LSize;

	private TOOLSIZERECORDSETUP WSize;

	private TOOLSIZERECORDSETUP TSize;

	private TOOLSIZERECORDSETUP LPadSize;

	private TOOLSIZERECORDSETUP RPadSize;

	private TOOLSIZERECORDSETUP GapSize;

	private TOOLSIZERECORDSETUP C1Size;

	private TOOLSIZERECORDSETUP C2Size;

	private TOOLSIZERECORDSETUP C3Size;

	private TOOLSIZERECORDSETUP C4Size;

	private TOOLSIZERECORDSETUP C5Size;

	private TOOLSIZERECORDSETUP C6Size;

	private TOOLSIZERECORDSETUP C7Size;

	private TOOLSIZERECORDSETUP C8Size;

	private TOOLSIZERECORDSETUP C9Size;

	private TOOLSIZERECORDSETUP C10Size;

	private TOOLSIZERECORDSETUP C11Size;

	private TOOLSIZERECORDSETUP C12Size;

	private TOOLSIZERECORDSETUP C13Size;

	private TOOLSIZERECORDSETUP C14Size;

	private ThroughputParamFile ThroughputFile;

	public List<OBJSIZE> OBJSizeStatistics;

	private const int MaxMeasureTools = 40;

	private const int MaxTools = 20;

	public void SetInspection(CVInspection[] station)
	{
		Station = station;
	}

	public void SetRecipe(ThroughputParamFile file)
	{
		ThroughputFile = file;
	}

	public void SetOJBSizes(List<OBJSIZE> OBJSizes)
	{
		OBJSizeStatistics = OBJSizes;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void CalcSizeStatistics(bool updateUI = false)
	{
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		int num8 = 0;
		double num9 = 0.0;
		double num10 = 0.0;
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		double num15 = 0.0;
		double num16 = 0.0;
		double num17 = 0.0;
		double num18 = 0.0;
		double num19 = 0.0;
		double num20 = 0.0;
		double num21 = 0.0;
		double num22 = 0.0;
		double num23 = 10000000.0;
		double num24 = 10000000.0;
		double num25 = 10000000.0;
		double num26 = 10000000.0;
		double num27 = 10000000.0;
		double num28 = 10000000.0;
		double num29 = 0.0;
		double num30 = 0.0;
		double num31 = 0.0;
		double num32 = 0.0;
		double num33 = 0.0;
		double num34 = 0.0;
		double num35 = 0.0;
		double num36 = 0.0;
		double num37 = 0.0;
		double num38 = 0.0;
		double num39 = 0.0;
		double num40 = 0.0;
		double num41 = 0.0;
		double num42 = 0.0;
		double num43 = 0.0;
		double num44 = 0.0;
		double num45 = 0.0;
		double num46 = 0.0;
		double num47 = 10000000.0;
		double num48 = 0.0;
		double num49 = 0.0;
		double num50 = 0.0;
		double num51 = 10000000.0;
		double num52 = 0.0;
		double num53 = 0.0;
		double num54 = 0.0;
		double num55 = 10000000.0;
		double num56 = 0.0;
		double num57 = 0.0;
		double num58 = 0.0;
		double num59 = 10000000.0;
		double num60 = 0.0;
		double num61 = 0.0;
		double num62 = 0.0;
		double num63 = 10000000.0;
		double num64 = 0.0;
		double num65 = 0.0;
		double num66 = 0.0;
		double num67 = 10000000.0;
		double num68 = 0.0;
		double num69 = 0.0;
		double num70 = 0.0;
		double num71 = 10000000.0;
		double num72 = 0.0;
		double num73 = 0.0;
		double num74 = 0.0;
		double num75 = 10000000.0;
		double num76 = 0.0;
		double num77 = 0.0;
		double num78 = 0.0;
		double num79 = 10000000.0;
		double num80 = 0.0;
		double num81 = 0.0;
		double num82 = 0.0;
		double num83 = 10000000.0;
		double num84 = 0.0;
		double num85 = 0.0;
		double num86 = 0.0;
		double num87 = 10000000.0;
		double num88 = 0.0;
		double num89 = 0.0;
		double num90 = 0.0;
		double num91 = 10000000.0;
		double num92 = 0.0;
		double num93 = 0.0;
		double num94 = 0.0;
		double num95 = 10000000.0;
		double num96 = 0.0;
		double num97 = 0.0;
		double num98 = 0.0;
		double num99 = 10000000.0;
		double num100 = 0.0;
		double num101 = 0.0;
		double num102 = 0.0;
		try
		{
			if (OBJSizeStatistics == null)
			{
				return;
			}
			if (!updateUI)
			{
				LSize.StationNo = ThroughputFile.param.LSizeStation;
				LSize.ToolSelect = ThroughputFile.param.LSizeToolSelect;
				LSize.SubSelect = ThroughputFile.param.LSizeSubSelect;
				LSize.ItemSelect = ThroughputFile.param.LSizeItemSelect;
				StationL = Station[LSize.StationNo];
				WSize.StationNo = ThroughputFile.param.WSizeStation;
				WSize.ToolSelect = ThroughputFile.param.WSizeToolSelect;
				WSize.SubSelect = ThroughputFile.param.WSizeSubSelect;
				WSize.ItemSelect = ThroughputFile.param.WSizeItemSelect;
				StationW = Station[WSize.StationNo];
				TSize.StationNo = ThroughputFile.param.TSizeStation;
				TSize.ToolSelect = ThroughputFile.param.TSizeToolSelect;
				TSize.SubSelect = ThroughputFile.param.TSizeSubSelect;
				TSize.ItemSelect = ThroughputFile.param.TSizeItemSelect;
				StationT = Station[TSize.StationNo];
				LPadSize.StationNo = ThroughputFile.param.LPadSizeStation;
				LPadSize.ToolSelect = ThroughputFile.param.LPadSizeToolSelect;
				LPadSize.SubSelect = ThroughputFile.param.LPadSizeSubSelect;
				LPadSize.ItemSelect = ThroughputFile.param.LPadSizeItemSelect;
				StationLPad = Station[LPadSize.StationNo];
				RPadSize.StationNo = ThroughputFile.param.RPadSizeStation;
				RPadSize.ToolSelect = ThroughputFile.param.RPadSizeToolSelect;
				RPadSize.SubSelect = ThroughputFile.param.RPadSizeSubSelect;
				RPadSize.ItemSelect = ThroughputFile.param.RPadSizeItemSelect;
				StationRPad = Station[RPadSize.StationNo];
				GapSize.StationNo = ThroughputFile.param.GapSizeStation;
				GapSize.ToolSelect = ThroughputFile.param.GapSizeToolSelect;
				GapSize.SubSelect = ThroughputFile.param.GapSizeSubSelect;
				GapSize.ItemSelect = ThroughputFile.param.GapSizeItemSelect;
				StationGap = Station[GapSize.StationNo];
				C1Size.StationNo = ThroughputFile.param.C1SizeStation;
				C1Size.ToolSelect = ThroughputFile.param.C1SizeToolSelect;
				C1Size.SubSelect = ThroughputFile.param.C1SizeSubSelect;
				C1Size.ItemSelect = ThroughputFile.param.C1SizeItemSelect;
				StationC1 = Station[C1Size.StationNo];
				C2Size.StationNo = ThroughputFile.param.C2SizeStation;
				C2Size.ToolSelect = ThroughputFile.param.C2SizeToolSelect;
				C2Size.SubSelect = ThroughputFile.param.C2SizeSubSelect;
				C2Size.ItemSelect = ThroughputFile.param.C2SizeItemSelect;
				StationC2 = Station[C2Size.StationNo];
				C3Size.StationNo = ThroughputFile.param.C3SizeStation;
				C3Size.ToolSelect = ThroughputFile.param.C3SizeToolSelect;
				C3Size.SubSelect = ThroughputFile.param.C3SizeSubSelect;
				C3Size.ItemSelect = ThroughputFile.param.C3SizeItemSelect;
				StationC3 = Station[C3Size.StationNo];
				C4Size.StationNo = ThroughputFile.param.C4SizeStation;
				C4Size.ToolSelect = ThroughputFile.param.C4SizeToolSelect;
				C4Size.SubSelect = ThroughputFile.param.C4SizeSubSelect;
				C4Size.ItemSelect = ThroughputFile.param.C4SizeItemSelect;
				StationC4 = Station[C4Size.StationNo];
				C5Size.StationNo = ThroughputFile.param.C5SizeStation;
				C5Size.ToolSelect = ThroughputFile.param.C5SizeToolSelect;
				C5Size.SubSelect = ThroughputFile.param.C5SizeSubSelect;
				C5Size.ItemSelect = ThroughputFile.param.C5SizeItemSelect;
				StationC5 = Station[C5Size.StationNo];
				C6Size.StationNo = ThroughputFile.param.C6SizeStation;
				C6Size.ToolSelect = ThroughputFile.param.C6SizeToolSelect;
				C6Size.SubSelect = ThroughputFile.param.C6SizeSubSelect;
				C6Size.ItemSelect = ThroughputFile.param.C6SizeItemSelect;
				StationC6 = Station[C6Size.StationNo];
				C7Size.StationNo = ThroughputFile.param.C7SizeStation;
				C7Size.ToolSelect = ThroughputFile.param.C7SizeToolSelect;
				C7Size.SubSelect = ThroughputFile.param.C7SizeSubSelect;
				C7Size.ItemSelect = ThroughputFile.param.C7SizeItemSelect;
				StationC7 = Station[C7Size.StationNo];
				C8Size.StationNo = ThroughputFile.param.C8SizeStation;
				C8Size.ToolSelect = ThroughputFile.param.C8SizeToolSelect;
				C8Size.SubSelect = ThroughputFile.param.C8SizeSubSelect;
				C8Size.ItemSelect = ThroughputFile.param.C8SizeItemSelect;
				StationC8 = Station[C8Size.StationNo];
				C9Size.StationNo = ThroughputFile.param.C9SizeStation;
				C9Size.ToolSelect = ThroughputFile.param.C9SizeToolSelect;
				C9Size.SubSelect = ThroughputFile.param.C9SizeSubSelect;
				C9Size.ItemSelect = ThroughputFile.param.C9SizeItemSelect;
				StationC9 = Station[C9Size.StationNo];
				C10Size.StationNo = ThroughputFile.param.C10SizeStation;
				C10Size.ToolSelect = ThroughputFile.param.C10SizeToolSelect;
				C10Size.SubSelect = ThroughputFile.param.C10SizeSubSelect;
				C10Size.ItemSelect = ThroughputFile.param.C10SizeItemSelect;
				StationC10 = Station[C10Size.StationNo];
				C11Size.StationNo = ThroughputFile.param.C11SizeStation;
				C11Size.ToolSelect = ThroughputFile.param.C11SizeToolSelect;
				C11Size.SubSelect = ThroughputFile.param.C11SizeSubSelect;
				C11Size.ItemSelect = ThroughputFile.param.C11SizeItemSelect;
				StationC11 = Station[C11Size.StationNo];
				C12Size.StationNo = ThroughputFile.param.C12SizeStation;
				C12Size.ToolSelect = ThroughputFile.param.C12SizeToolSelect;
				C12Size.SubSelect = ThroughputFile.param.C12SizeSubSelect;
				C12Size.ItemSelect = ThroughputFile.param.C12SizeItemSelect;
				StationC12 = Station[C12Size.StationNo];
				C13Size.StationNo = ThroughputFile.param.C13SizeStation;
				C13Size.ToolSelect = ThroughputFile.param.C13SizeToolSelect;
				C13Size.SubSelect = ThroughputFile.param.C13SizeSubSelect;
				C13Size.ItemSelect = ThroughputFile.param.C13SizeItemSelect;
				StationC13 = Station[C13Size.StationNo];
				C14Size.StationNo = ThroughputFile.param.C14SizeStation;
				C14Size.ToolSelect = ThroughputFile.param.C14SizeToolSelect;
				C14Size.SubSelect = ThroughputFile.param.C14SizeSubSelect;
				C14Size.ItemSelect = ThroughputFile.param.C14SizeItemSelect;
				StationC14 = Station[C14Size.StationNo];
			}
			OBJSizeStatistics.Clear();
			OBJSIZE item = default(OBJSIZE);
			for (int i = 0; i < StationL.SizeStatics.Count; i++)
			{
				num3 = (num7 = (num2 = -999.0));
				num4 = (num5 = (num6 = -999.0));
				num9 = (num10 = (num11 = (num12 = (num13 = (num14 = -999.0)))));
				num15 = (num16 = (num17 = (num18 = (num20 = (num20 = (num21 = (num22 = -999.0)))))));
				num2 = GetSizeValue(StationL, LSize, i);
				num = StationL.SizeStatics[i].ID;
				for (int j = 0; j < StationW.SizeStatics.Count; j++)
				{
					if (num == StationW.SizeStatics[j].ID)
					{
						num3 = GetSizeValue(StationW, WSize, j);
						break;
					}
				}
				for (int k = 0; k < StationT.SizeStatics.Count; k++)
				{
					if (num == StationT.SizeStatics[k].ID)
					{
						num7 = GetSizeValue(StationT, TSize, k);
						break;
					}
				}
				for (int l = 0; l < StationLPad.SizeStatics.Count; l++)
				{
					if (num == StationLPad.SizeStatics[l].ID)
					{
						num4 = GetSizeValue(StationLPad, LPadSize, l);
						break;
					}
				}
				for (int m = 0; m < StationRPad.SizeStatics.Count; m++)
				{
					if (num == StationRPad.SizeStatics[m].ID)
					{
						num5 = GetSizeValue(StationRPad, RPadSize, m);
						break;
					}
				}
				for (int n = 0; n < StationGap.SizeStatics.Count; n++)
				{
					if (num == StationGap.SizeStatics[n].ID)
					{
						num6 = GetSizeValue(StationGap, GapSize, n);
						break;
					}
				}
				for (int num103 = 0; num103 < StationC1.SizeStatics.Count; num103++)
				{
					if (num == StationC1.SizeStatics[num103].ID)
					{
						num9 = GetSizeValue(StationC1, C1Size, num103);
						break;
					}
				}
				for (int num104 = 0; num104 < StationC2.SizeStatics.Count; num104++)
				{
					if (num == StationC2.SizeStatics[num104].ID)
					{
						num10 = GetSizeValue(StationC2, C2Size, num104);
						break;
					}
				}
				for (int num105 = 0; num105 < StationC3.SizeStatics.Count; num105++)
				{
					if (num == StationC3.SizeStatics[num105].ID)
					{
						num11 = GetSizeValue(StationC3, C3Size, num105);
						break;
					}
				}
				for (int num106 = 0; num106 < StationC4.SizeStatics.Count; num106++)
				{
					if (num == StationC4.SizeStatics[num106].ID)
					{
						num12 = GetSizeValue(StationC4, C4Size, num106);
						break;
					}
				}
				for (int num107 = 0; num107 < StationC5.SizeStatics.Count; num107++)
				{
					if (num == StationC5.SizeStatics[num107].ID)
					{
						num13 = GetSizeValue(StationC5, C5Size, num107);
						break;
					}
				}
				for (int num108 = 0; num108 < StationC6.SizeStatics.Count; num108++)
				{
					if (num == StationC6.SizeStatics[num108].ID)
					{
						num14 = GetSizeValue(StationC6, C6Size, num108);
						break;
					}
				}
				for (int num109 = 0; num109 < StationC7.SizeStatics.Count; num109++)
				{
					if (num == StationC7.SizeStatics[num109].ID)
					{
						num15 = GetSizeValue(StationC7, C7Size, num109);
						break;
					}
				}
				for (int num110 = 0; num110 < StationC8.SizeStatics.Count; num110++)
				{
					if (num == StationC8.SizeStatics[num110].ID)
					{
						num16 = GetSizeValue(StationC8, C8Size, num110);
						break;
					}
				}
				for (int num111 = 0; num111 < StationC9.SizeStatics.Count; num111++)
				{
					if (num == StationC9.SizeStatics[num111].ID)
					{
						num17 = GetSizeValue(StationC9, C9Size, num111);
						break;
					}
				}
				for (int num112 = 0; num112 < StationC10.SizeStatics.Count; num112++)
				{
					if (num == StationC10.SizeStatics[num112].ID)
					{
						num18 = GetSizeValue(StationC10, C10Size, num112);
						break;
					}
				}
				for (int num113 = 0; num113 < StationC11.SizeStatics.Count; num113++)
				{
					if (num == StationC11.SizeStatics[num113].ID)
					{
						num19 = GetSizeValue(StationC11, C11Size, num113);
						break;
					}
				}
				for (int num114 = 0; num114 < StationC12.SizeStatics.Count; num114++)
				{
					if (num == StationC12.SizeStatics[num114].ID)
					{
						num20 = GetSizeValue(StationC12, C12Size, num114);
						break;
					}
				}
				for (int num115 = 0; num115 < StationC13.SizeStatics.Count; num115++)
				{
					if (num == StationC13.SizeStatics[num115].ID)
					{
						num21 = GetSizeValue(StationC13, C13Size, num115);
						break;
					}
				}
				for (int num116 = 0; num116 < StationC14.SizeStatics.Count; num116++)
				{
					if (num == StationC14.SizeStatics[num116].ID)
					{
						num22 = GetSizeValue(StationC14, C14Size, num116);
						break;
					}
				}
				if (num3 != -999.0 && num7 != -999.0 && num2 != -999.0 && num4 != -999.0 && num5 != -999.0 && num6 != -999.0 && num9 != -999.0 && num10 != -999.0 && num11 != -999.0 && num12 != -999.0 && num13 != -999.0 && num14 != -999.0 && num2 > 0.0 && num3 > 0.0 && num7 > 0.0)
				{
					item.ID = num;
					item.LSize = num2;
					item.WSize = num3;
					item.TSize = num7;
					item.LPadWidth = num4;
					item.RPadWidth = num5;
					item.Gap = num6;
					item.C1 = num9;
					item.C2 = num10;
					item.C3 = num11;
					item.C4 = num12;
					item.C5 = num13;
					item.C6 = num14;
					item.C7 = num15;
					item.C8 = num16;
					item.C9 = num17;
					item.C10 = num18;
					item.C11 = num19;
					item.C12 = num20;
					item.C13 = num21;
					item.C14 = num22;
					if (OBJSizeStatistics.Count >= 1000)
					{
						OBJSizeStatistics.RemoveAt(OBJSizeStatistics.Count - 1);
					}
					OBJSizeStatistics.Add(item);
					num8++;
					if (num2 > num29)
					{
						num29 = num2;
					}
					if (num2 < num23)
					{
						num23 = num2;
					}
					num35 += num2;
					if (num3 > num30)
					{
						num30 = num3;
					}
					if (num3 < num24)
					{
						num24 = num3;
					}
					num36 += num3;
					if (num7 > num31)
					{
						num31 = num7;
					}
					if (num7 < num25)
					{
						num25 = num7;
					}
					num37 += num7;
					if (num4 > num32)
					{
						num32 = num4;
					}
					if (num4 < num26)
					{
						num26 = num4;
					}
					num38 += num4;
					if (num5 > num33)
					{
						num33 = num5;
					}
					if (num5 < num27)
					{
						num27 = num5;
					}
					num39 += num5;
					if (num6 > num34)
					{
						num34 = num6;
					}
					if (num6 < num28)
					{
						num28 = num6;
					}
					num40 += num6;
					if (num9 > num48)
					{
						num48 = num9;
					}
					if (num9 < num47)
					{
						num47 = num9;
					}
					num49 += num9;
					if (num10 > num52)
					{
						num52 = num10;
					}
					if (num10 < num51)
					{
						num51 = num10;
					}
					num53 += num10;
					if (num11 > num56)
					{
						num56 = num11;
					}
					if (num11 < num55)
					{
						num55 = num11;
					}
					num57 += num11;
					if (num12 > num60)
					{
						num60 = num12;
					}
					if (num12 < num59)
					{
						num59 = num12;
					}
					num61 += num12;
					if (num13 > num64)
					{
						num64 = num13;
					}
					if (num13 < num63)
					{
						num63 = num13;
					}
					num65 += num13;
					if (num14 > num68)
					{
						num68 = num14;
					}
					if (num14 < num67)
					{
						num67 = num14;
					}
					num69 += num14;
					if (num15 > num72)
					{
						num72 = num15;
					}
					if (num15 < num71)
					{
						num71 = num15;
					}
					num73 += num15;
					if (num16 > num76)
					{
						num76 = num16;
					}
					if (num16 < num75)
					{
						num75 = num16;
					}
					num77 += num16;
					if (num17 > num80)
					{
						num80 = num17;
					}
					if (num17 < num79)
					{
						num79 = num17;
					}
					num81 += num17;
					if (num18 > num84)
					{
						num84 = num18;
					}
					if (num18 < num83)
					{
						num83 = num18;
					}
					num85 += num18;
					if (num19 > num88)
					{
						num88 = num19;
					}
					if (num19 < num87)
					{
						num87 = num19;
					}
					num89 += num19;
					if (num20 > num92)
					{
						num92 = num20;
					}
					if (num20 < num91)
					{
						num91 = num20;
					}
					num93 += num20;
					if (num21 > num96)
					{
						num96 = num21;
					}
					if (num21 < num95)
					{
						num95 = num21;
					}
					num97 += num21;
					if (num22 > num100)
					{
						num100 = num22;
					}
					if (num22 < num99)
					{
						num99 = num22;
					}
					num101 += num22;
				}
			}
			if (num8 > 0 && OBJSizeStatistics.Count > 0)
			{
				num41 = num35 / (double)num8;
				num42 = num36 / (double)num8;
				num43 = num37 / (double)num8;
				num44 = num38 / (double)num8;
				num45 = num39 / (double)num8;
				num46 = num40 / (double)num8;
				Math.Abs(num29 - num23);
				Math.Abs(num30 - num24);
				Math.Abs(num31 - num25);
				Math.Abs(num32 - num26);
				Math.Abs(num33 - num27);
				Math.Abs(num34 - num28);
				num50 = num49 / (double)num8;
				Math.Abs(num48 - num47);
				num54 = num53 / (double)num8;
				Math.Abs(num52 - num51);
				num58 = num57 / (double)num8;
				Math.Abs(num56 - num55);
				num62 = num61 / (double)num8;
				Math.Abs(num60 - num59);
				num66 = num65 / (double)num8;
				Math.Abs(num64 - num63);
				num70 = num69 / (double)num8;
				Math.Abs(num68 - num67);
				num74 = num73 / (double)num8;
				Math.Abs(num72 - num71);
				num78 = num77 / (double)num8;
				Math.Abs(num76 - num75);
				num82 = num81 / (double)num8;
				Math.Abs(num80 - num79);
				num86 = num85 / (double)num8;
				Math.Abs(num84 - num83);
				num90 = num89 / (double)num8;
				Math.Abs(num88 - num87);
				num94 = num93 / (double)num8;
				Math.Abs(num92 - num91);
				num98 = num97 / (double)num8;
				Math.Abs(num96 - num95);
				num102 = num101 / (double)num8;
				Math.Abs(num100 - num99);
				double num117 = 0.0;
				double num118 = 0.0;
				double num119 = 0.0;
				double num120 = 0.0;
				double num121 = 0.0;
				double num122 = 0.0;
				double num123 = 0.0;
				double num124 = 0.0;
				double num125 = 0.0;
				double num126 = 0.0;
				double num127 = 0.0;
				double num128 = 0.0;
				double num129 = 0.0;
				double num130 = 0.0;
				double num131 = 0.0;
				double num132 = 0.0;
				double num133 = 0.0;
				double num134 = 0.0;
				double num135 = 0.0;
				double num136 = 0.0;
				for (int num137 = 0; num137 < OBJSizeStatistics.Count; num137++)
				{
					num117 += Math.Pow(OBJSizeStatistics[num137].LSize - num41, 2.0);
					num118 += Math.Pow(OBJSizeStatistics[num137].WSize - num42, 2.0);
					num119 += Math.Pow(OBJSizeStatistics[num137].TSize - num43, 2.0);
					num120 += Math.Pow(OBJSizeStatistics[num137].LPadWidth - num44, 2.0);
					num121 += Math.Pow(OBJSizeStatistics[num137].RPadWidth - num45, 2.0);
					num122 += Math.Pow(OBJSizeStatistics[num137].Gap - num46, 2.0);
					num123 += Math.Pow(OBJSizeStatistics[num137].C1 - num50, 2.0);
					num124 += Math.Pow(OBJSizeStatistics[num137].C2 - num54, 2.0);
					num125 += Math.Pow(OBJSizeStatistics[num137].C3 - num58, 2.0);
					num126 += Math.Pow(OBJSizeStatistics[num137].C4 - num62, 2.0);
					num127 += Math.Pow(OBJSizeStatistics[num137].C5 - num66, 2.0);
					num128 += Math.Pow(OBJSizeStatistics[num137].C6 - num70, 2.0);
					num129 += Math.Pow(OBJSizeStatistics[num137].C7 - num74, 2.0);
					num130 += Math.Pow(OBJSizeStatistics[num137].C8 - num78, 2.0);
					num131 += Math.Pow(OBJSizeStatistics[num137].C9 - num82, 2.0);
					num132 += Math.Pow(OBJSizeStatistics[num137].C10 - num86, 2.0);
					num133 += Math.Pow(OBJSizeStatistics[num137].C11 - num90, 2.0);
					num134 += Math.Pow(OBJSizeStatistics[num137].C12 - num94, 2.0);
					num135 += Math.Pow(OBJSizeStatistics[num137].C13 - num98, 2.0);
					num136 += Math.Pow(OBJSizeStatistics[num137].C14 - num102, 2.0);
				}
				Math.Sqrt(num117 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num118 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num119 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num120 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num121 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num122 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num123 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num124 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num125 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num126 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num127 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num128 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num129 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num130 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num131 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num132 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num133 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num134 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num135 / (double)OBJSizeStatistics.Count);
				Math.Sqrt(num136 / (double)OBJSizeStatistics.Count);
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("SW04-SizeStatistics-CalcSizeStatistics()-" + ex.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private double GetSizeValue(CVInspection insp, TOOLSIZERECORDSETUP ss, int index)
	{
		try
		{
			switch (ss.ToolSelect)
			{
			case 0:
				if (ss.SubSelect == 0)
				{
					return insp.SizeStatics[index].LocateWidth;
				}
				return insp.SizeStatics[index].LocateHeight;
			case 1:
				if (ss.ItemSelect >= 0 && ss.ItemSelect < 20)
				{
					if (ss.SubSelect == 0)
					{
						return insp.SizeStatics[index].AdvAWidth[ss.ItemSelect];
					}
					return insp.SizeStatics[index].AdvAHeight[ss.ItemSelect];
				}
				return 0.0;
			case 2:
				if (ss.ItemSelect >= 0 && ss.ItemSelect < 40)
				{
					return insp.SizeStatics[index].AdvM[ss.ItemSelect];
				}
				return 0.0;
			case 3:
				if (ss.ItemSelect >= 0 && ss.ItemSelect < 20)
				{
					if (ss.SubSelect == 0)
					{
						return insp.SizeStatics[index].MeasureW[ss.ItemSelect];
					}
					return insp.SizeStatics[index].MeasureH[ss.ItemSelect];
				}
				return 0.0;
			default:
				return 0.0;
			}
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("SW03-SizeStatistics-GetSizeValue()-" + ex.ToString());
			return 0.0;
		}
	}
}
