using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using InspTools.Resources;

namespace InspTools.Other;

public class ModifyLogFun : Window, IComponentConnector
{
	private bool _contentLoaded;

	public ModifyLogFun()
	{
		InitializeComponent();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetMorphModifyText(int Now, int Backup, int NowCount, int BackupCount, int Mindex1, int Mindex2, string Str, string SubStr = "")
	{
		string text = Str;
		string text2 = Str;
		bool flag = false;
		bool flag2 = false;
		if (Backup != Now)
		{
			text += string.Format(" ({0}{1})[{2}-{3}] {4} {5} {6}\n", SubStr, LanguageManager.Instance["Dilate"] + LanguageManager.Instance["Erode"], Mindex1, Mindex2, GetMorphText(Backup), LanguageManager.Instance["ChangedTo"], GetMorphText(Now));
			flag = true;
		}
		if (BackupCount != NowCount)
		{
			text2 += string.Format(" ({0}{1})[{2}-{3}] {4} {5} {6}\n", SubStr, LanguageManager.Instance["Dilate"] + LanguageManager.Instance["Erode"] + LanguageManager.Instance["Count"], Mindex1, Mindex2, BackupCount, LanguageManager.Instance["ChangedTo"], NowCount);
			flag2 = true;
		}
		if (flag & flag2)
		{
			return text + text2;
		}
		if (flag)
		{
			return text;
		}
		if (flag2)
		{
			return text2;
		}
		return "";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetMorphText(int index)
	{
		return index switch
		{
			0 => "-----", 
			1 => LanguageManager.Instance["Dilate"], 
			2 => LanguageManager.Instance["Erode"], 
			3 => LanguageManager.Instance["OpenMorph"], 
			4 => LanguageManager.Instance["CloseMorph"], 
			5 => LanguageManager.Instance["DilateV"], 
			6 => LanguageManager.Instance["ErodeV"], 
			7 => LanguageManager.Instance["DilateH"], 
			8 => LanguageManager.Instance["ErodeH"], 
			_ => "-----", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetChannelText(int index)
	{
		return index switch
		{
			0 => "R", 
			1 => "G", 
			2 => "B", 
			3 => "H", 
			4 => "S", 
			5 => "V", 
			6 => "Gray", 
			7 => "C1", 
			8 => "C2", 
			9 => "C3", 
			_ => "R", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetLocateTypeText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["EasyLocate"], 
			1 => LanguageManager.Instance["QuadLocate"], 
			2 => LanguageManager.Instance["PadLocate"], 
			_ => LanguageManager.Instance["EasyLocate"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetErrorBinText(int index)
	{
		return index switch
		{
			0 => "Retest", 
			1 => "OK", 
			2 => "NG1", 
			3 => "NG2", 
			4 => "NG3", 
			5 => "NG4", 
			_ => "Retest", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetSelectFaceText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["Unused"], 
			1 => "A", 
			2 => "B", 
			3 => "C", 
			4 => "D", 
			_ => LanguageManager.Instance["Unused"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetRotateAngleUseText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["UseAngleAuto"], 
			1 => LanguageManager.Instance["UseAngleAutoLeft"], 
			2 => LanguageManager.Instance["UseAngleAutoRight"], 
			3 => LanguageManager.Instance["UseAngleAutoTop"], 
			4 => LanguageManager.Instance["UseAngleAutoBottom"], 
			_ => LanguageManager.Instance["UseAngleAuto"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetBlobAnalysisText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["InspBlobs"], 
			1 => LanguageManager.Instance["InspLines"], 
			2 => LanguageManager.Instance["InspMultiBlob"], 
			3 => LanguageManager.Instance["InspLineWidth"], 
			_ => LanguageManager.Instance["InspBlobs"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetBlobRuleText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["Disable"], 
			1 => LanguageManager.Instance["Width"], 
			2 => LanguageManager.Instance["Height"], 
			3 => LanguageManager.Instance["Area"], 
			4 => LanguageManager.Instance["WHRatio"], 
			5 => LanguageManager.Instance["AreaRatio"], 
			6 => LanguageManager.Instance["ABAreaRatio"], 
			7 => LanguageManager.Instance["ABHDiff"], 
			8 => LanguageManager.Instance["ABWDiff"], 
			9 => LanguageManager.Instance["ABWRatio"], 
			10 => LanguageManager.Instance["ABHRatio"], 
			11 => LanguageManager.Instance["AllArea"], 
			12 => LanguageManager.Instance["AllAreaRatio"], 
			13 => LanguageManager.Instance["ABAllAreaDiff"], 
			_ => "-----", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetAvgBinaryText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["AvgThd"], 
			1 => LanguageManager.Instance["HoriThd"], 
			2 => LanguageManager.Instance["VeriThd"], 
			_ => LanguageManager.Instance["AvgThd"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetAvgCountText(int index)
	{
		return index switch
		{
			0 => "1", 
			1 => "3", 
			2 => "5", 
			3 => "8", 
			4 => "12", 
			5 => "15", 
			_ => "1", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetScanRuleText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["Average"], 
			1 => LanguageManager.Instance["Min"], 
			2 => LanguageManager.Instance["Max"], 
			_ => LanguageManager.Instance["Average"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetSecanDirectionText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["Left2Right"], 
			1 => LanguageManager.Instance["Right2Left"], 
			2 => LanguageManager.Instance["Top2Bottom"], 
			3 => LanguageManager.Instance["Bottom2Top"], 
			4 => LanguageManager.Instance["ScanLeftToRightHole"], 
			5 => LanguageManager.Instance["ScanRightToLeftHole"], 
			_ => LanguageManager.Instance["Left2Right"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetMeasureFunctionText(int index)
	{
		return index switch
		{
			0 => LanguageManager.Instance["Width3"], 
			1 => LanguageManager.Instance["Height3"], 
			2 => LanguageManager.Instance["Distance"], 
			3 => LanguageManager.Instance["KValue"], 
			4 => LanguageManager.Instance["Subtract"], 
			_ => LanguageManager.Instance["Width3"], 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public string GetInsideLocatePtText(int index)
	{
		return index switch
		{
			0 => "A", 
			1 => "B", 
			2 => "C", 
			3 => "D", 
			4 => "E", 
			5 => "F", 
			6 => "H", 
			7 => "H", 
			8 => "O", 
			_ => "A", 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/modifylogfun.xaml", UriKind.Relative);
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
		_contentLoaded = true;
	}
}
