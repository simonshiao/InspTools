using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace InspTools.Other;

public class PasswordWindow : Window, IComponentConnector
{
	private bool LoginSuccess;

	private string DefaultPassWord = "1111";

	internal PasswordBox txt_Password;

	internal Button btn_Login;

	internal Button btn_1;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public PasswordWindow()
	{
		InitializeComponent();
	}

	public bool GetLoginStatus()
	{
		return LoginSuccess;
	}

	public void SetPassWord(string password)
	{
		DefaultPassWord = password;
	}

	private void Txt_Password_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_Login_Click(object sender, RoutedEventArgs e)
	{
		if (txt_Password.Password == DefaultPassWord)
		{
			LoginSuccess = true;
			Close();
		}
		else
		{
			MessageBox.Show(string.Format("輸入密碼錯誤({0})，請重新輸入", txt_Password.Password), "警告");
			txt_Password.Password = "";
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		txt_Password.Password += button.Uid;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/passwordwindow.xaml", UriKind.Relative);
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
			txt_Password = (PasswordBox)target;
			txt_Password.PreviewMouseLeftButtonDown += Txt_Password_PreviewMouseLeftButtonDown;
			break;
		case 2:
			btn_Login = (Button)target;
			btn_Login.Click += Btn_Login_Click;
			break;
		case 3:
			btn_1 = (Button)target;
			btn_1.Click += Button_Click;
			break;
		case 4:
			((Button)target).Click += Button_Click;
			break;
		case 5:
			((Button)target).Click += Button_Click;
			break;
		case 6:
			((Button)target).Click += Button_Click;
			break;
		case 7:
			((Button)target).Click += Button_Click;
			break;
		case 8:
			((Button)target).Click += Button_Click;
			break;
		case 9:
			((Button)target).Click += Button_Click;
			break;
		case 10:
			((Button)target).Click += Button_Click;
			break;
		case 11:
			((Button)target).Click += Button_Click;
			break;
		case 12:
			((Button)target).Click += Button_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
