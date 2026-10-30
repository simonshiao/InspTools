using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace InspTools.Other;

public class ItemInputWindow : Window, IComponentConnector
{
	internal ItemInputWindow wnd_Main;

	internal TextBox txt_Value1;

	internal Button btn_Input;

	private bool _contentLoaded;

	public string InputStr { get; protected set; }

	public ItemInputWindow()
	{
		InitializeComponent();
		txt_Value1.Focus();
		InputStr = "";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Input_Click(object sender, RoutedEventArgs e)
	{
		string text;
		try
		{
			text = txt_Value1.Text;
		}
		catch (OverflowException)
		{
			MessageBox.Show("格式輸入錯誤，請重新輸入！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			txt_Value1.Text = "";
			return;
		}
		if (text == "")
		{
			MessageBox.Show("項目名稱為空，請重新輸入！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			txt_Value1.Text = "";
		}
		else
		{
			InputStr = text;
			Close();
		}
	}

	public void SetTitle(string str)
	{
		wnd_Main.Title = str;
	}

	public void SetText(string str)
	{
		txt_Value1.Text = str;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/iteminputwindow.xaml", UriKind.Relative);
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
			wnd_Main = (ItemInputWindow)target;
			break;
		case 2:
			txt_Value1 = (TextBox)target;
			break;
		case 3:
			btn_Input = (Button)target;
			btn_Input.Click += Btn_Input_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
