using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace InspTools.Other;

public class HashTagEditWindow : Window, IComponentConnector
{
	private ThroughputParamFile ThroughputFile;

	private bool inited;

	private TextBox[] TagTxt;

	internal TextBox txt_tag1;

	internal TextBox txt_tag2;

	internal TextBox txt_tag3;

	internal TextBox txt_tag4;

	internal TextBox txt_tag5;

	internal TextBox txt_tag6;

	internal TextBox txt_tag7;

	internal Button btn_Save;

	private bool _contentLoaded;

	public HashTagEditWindow()
	{
		InitializeComponent();
		inited = true;
		TagTxt = new TextBox[7] { txt_tag1, txt_tag2, txt_tag3, txt_tag4, txt_tag5, txt_tag6, txt_tag7 };
	}

	public void SetThroughputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
		for (int i = 0; i < 7; i++)
		{
			TagTxt[i].Text = ThroughputFile.param.HashTagStr[i];
		}
	}

	private void Btn_Save_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 7; i++)
		{
			ThroughputFile.param.HashTagStr[i] = TagTxt[i].Text;
		}
		Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/hashtageditwindow.xaml", UriKind.Relative);
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
			txt_tag1 = (TextBox)target;
			break;
		case 2:
			txt_tag2 = (TextBox)target;
			break;
		case 3:
			txt_tag3 = (TextBox)target;
			break;
		case 4:
			txt_tag4 = (TextBox)target;
			break;
		case 5:
			txt_tag5 = (TextBox)target;
			break;
		case 6:
			txt_tag6 = (TextBox)target;
			break;
		case 7:
			txt_tag7 = (TextBox)target;
			break;
		case 8:
			btn_Save = (Button)target;
			btn_Save.Click += Btn_Save_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
