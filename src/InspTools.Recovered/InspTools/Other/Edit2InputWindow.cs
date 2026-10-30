using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace InspTools.Other;

public class Edit2InputWindow : Window, IComponentConnector
{
	private int Value1Min;

	private int Value1Max;

	private int Value2Min;

	private int Value2Max;

	internal Label lab_Value1;

	internal Label lab_Value2;

	internal TextBox txt_Value1;

	internal TextBox txt_Value2;

	internal Button btn_Input;

	private bool _contentLoaded;

	public int Value1 { get; protected set; }

	public int Value2 { get; protected set; }

	public bool SetSuccess { get; protected set; }

	public Edit2InputWindow()
	{
		InitializeComponent();
		SetSuccess = false;
	}

	public void SetMinMaxValue(int v1min, int v1max, int v2min, int v2max)
	{
		Value1Min = v1min;
		Value1Max = v1max;
		Value2Min = v2min;
		Value2Max = v2max;
	}

	public void SetValue(int v1, int v2)
	{
		txt_Value1.Text = v1.ToString();
		txt_Value2.Text = v2.ToString();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetValueLabel(string str1, string str2)
	{
		lab_Value1.Content = str1 + ":";
		lab_Value2.Content = str2 + ":";
		base.Content = "輸入" + str1 + str2 + ":";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Input_Click(object sender, RoutedEventArgs e)
	{
		int num;
		try
		{
			num = Convert.ToInt32(txt_Value1.Text);
		}
		catch (OverflowException)
		{
			MessageBox.Show("格式輸入錯誤，請重新輸入！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			txt_Value1.Text = "";
			return;
		}
		int num2;
		try
		{
			num2 = Convert.ToInt32(txt_Value2.Text);
		}
		catch (OverflowException)
		{
			MessageBox.Show("格式輸入錯誤，請重新輸入！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			txt_Value1.Text = "";
			return;
		}
		if (num < Value1Min || num > Value1Max)
		{
			MessageBox.Show("超過設定值範圍，請重新輸入！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			txt_Value1.Text = "";
			return;
		}
		if (num2 < Value2Min || num2 > Value2Max)
		{
			MessageBox.Show("超過設定值範圍，請重新輸入！", "警告", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
			txt_Value2.Text = "";
			return;
		}
		Value1 = num;
		Value2 = num2;
		SetSuccess = true;
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
			Uri resourceLocator = new Uri("/InspTools;component/other/edit2inputwindow.xaml", UriKind.Relative);
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
			lab_Value1 = (Label)target;
			break;
		case 2:
			lab_Value2 = (Label)target;
			break;
		case 3:
			txt_Value1 = (TextBox)target;
			break;
		case 4:
			txt_Value2 = (TextBox)target;
			break;
		case 5:
			btn_Input = (Button)target;
			btn_Input.Click += Btn_Input_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
