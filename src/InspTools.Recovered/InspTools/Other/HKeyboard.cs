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

public class HKeyboard : Window, IComponentConnector
{
	private string Input = "";

	private bool InputInt;

	private bool UseDouble = true;

	private bool UseNegative = true;

	private bool closeing;

	public bool UseCommonDigits;

	public int CommonDigitsIndex = -1;

	private bool UpdateCommonDigits;

	private ThroughputParamFile Tfile;

	private TextBox txbox;

	private string MainTitle = "";

	internal HKeyboard MainW;

	internal Button btn_1;

	internal Button btn_2;

	internal Button btn_3;

	internal Button btn_4;

	internal Button btn_5;

	internal Button btn_6;

	internal Button btn_7;

	internal Button btn_8;

	internal Button btn_9;

	internal Button btn_0;

	internal Button btn_point;

	internal Button btn_Finish;

	internal Button btn_C;

	internal Button btn_Negative;

	internal TextBox txt_Input;

	internal Button btn_M1;

	internal Button btn_M5;

	internal Button btn_M10;

	internal Button btn_M100;

	internal Button btn_P1;

	internal Button btn_P5;

	internal Button btn_P10;

	internal Button btn_P100;

	internal ComboBox cb_CommonDigits;

	private bool _contentLoaded;

	public HKeyboard()
	{
		InitializeComponent();
	}

	public void SetTitle(string title)
	{
		MainTitle = title;
		MainW.Title = MainTitle;
	}

	public void setTxtbox(TextBox t)
	{
		txbox = t;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetInput(string str)
	{
		Input = str;
		if (str.Contains(":") && Tfile != null)
		{
			int length = str.IndexOf(":");
			int num = Convert.ToInt32(str.Substring(0, length));
			txt_Input.Text = string.Format("{0}:{1}:{2}", num, Tfile.ComDigitsName[num - 1], Tfile.ComDigits[num - 1]);
			cb_CommonDigits.SelectedIndex = num;
		}
	}

	public string GetInput()
	{
		return Input;
	}

	public void SetInputInt()
	{
		InputInt = true;
		btn_point.IsEnabled = false;
	}

	public void SetInputNegative(bool state)
	{
		btn_Negative.IsEnabled = state;
		UseNegative = state;
	}

	public void SetDouble(bool state)
	{
		btn_point.IsEnabled = state;
		UseDouble = state;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void SetComDigits(ThroughputParamFile tfile)
	{
		if (tfile != null)
		{
			cb_CommonDigits.Visibility = Visibility.Visible;
			Tfile = tfile;
			cb_CommonDigits.Items.Clear();
			cb_CommonDigits.Items.Add("-----");
			for (int i = 0; i < 24; i++)
			{
				cb_CommonDigits.Items.Add(Tfile.ComDigitsName[i]);
			}
			cb_CommonDigits.SelectedIndex = 0;
			UpdateCommonDigits = true;
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Window_Activated(object sender, EventArgs e)
	{
		if (MainTitle == "")
		{
			MainW.Title = string.Format("Last:{0}", Input);
		}
		if (!UpdateCommonDigits)
		{
			cb_CommonDigits.Visibility = Visibility.Hidden;
		}
		else
		{
			cb_CommonDigits.Visibility = Visibility.Visible;
		}
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
		Input = "";
		txt_Input.Text = Input;
		if (txbox != null)
		{
			txbox.Text = Input;
		}
	}

	private void Button_Click_2(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Negative_Click(object sender, RoutedEventArgs e)
	{
		if (InputInt)
		{
			Input = txt_Input.Text;
			if (Input == "")
			{
				txt_Input.Text = "-";
				return;
			}
			int num = Convert.ToInt32(Input);
			num *= -1;
			Input = string.Format("{0:D}", num);
			txt_Input.Text = Input;
			if (txbox != null)
			{
				txbox.Text = Input;
			}
			return;
		}
		Input = txt_Input.Text;
		if (Input == "")
		{
			txt_Input.Text = "-";
			return;
		}
		double num2 = Convert.ToDouble(Input);
		num2 *= -1.0;
		Input = string.Format("{0:F}", num2);
		txt_Input.Text = Input;
		if (txbox != null)
		{
			txbox.Text = Input;
		}
	}

	private void Button_Click_4(object sender, RoutedEventArgs e)
	{
		Input = txt_Input.Text;
		if (Input.Length != 0)
		{
			Input = Input.Remove(Input.Length - 1, 1);
			txt_Input.Text = Input;
			if (txbox != null)
			{
				txbox.Text = Input;
			}
		}
	}

	private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		Input = txt_Input.Text;
		if (!(button.Name == "btn_point") || Input.IndexOf(".") < 0)
		{
			Input += button.Content;
			txt_Input.Text = Input;
			if (txbox != null)
			{
				txbox.Text = Input;
			}
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Finish_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (UseCommonDigits && cb_CommonDigits.SelectedIndex >= 0)
			{
				CommonDigitsIndex = cb_CommonDigits.SelectedIndex - 1;
				txt_Input.Text = cb_CommonDigits.SelectedIndex.ToString();
			}
			else if (!InputInt)
			{
				if (txt_Input.Text == "")
				{
					Hide();
					txbox = null;
					Close();
					return;
				}
				double result = 0.0;
				if (double.TryParse(txt_Input.Text, out result))
				{
					Input = txt_Input.Text;
					Hide();
					txbox = null;
				}
				else
				{
					MessageBox.Show("請輸入合法數值");
				}
			}
			else
			{
				if (txt_Input.Text == "")
				{
					Hide();
					txbox = null;
					Close();
					return;
				}
				int result2 = 0;
				if (int.TryParse(txt_Input.Text, out result2))
				{
					Input = txt_Input.Text;
					Hide();
					txbox = null;
				}
				else
				{
					MessageBox.Show("請輸入合法數值");
				}
			}
			txbox = null;
			Close();
		}
		catch
		{
			txbox = null;
			Close();
		}
	}

	private void Button_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
	{
		Button button = (Button)sender;
		try
		{
			if (!UseDouble)
			{
				int num = ((!(txt_Input.Text == "")) ? Convert.ToInt32(txt_Input.Text) : ((!(Input == "")) ? Convert.ToInt32(Input) : 0));
				switch (button.Uid)
				{
				case "0":
					num--;
					break;
				case "1":
					num++;
					break;
				case "2":
					num -= 5;
					break;
				case "3":
					num += 5;
					break;
				case "4":
					num -= 10;
					break;
				case "5":
					num += 10;
					break;
				case "6":
					num -= 100;
					break;
				case "7":
					num += 100;
					break;
				}
				if (!UseNegative)
				{
					num = Math.Abs(num);
				}
				txt_Input.Text = num.ToString();
				if (txbox != null)
				{
					txbox.Text = txt_Input.Text;
				}
			}
			else
			{
				double num2 = ((!(txt_Input.Text == "")) ? Convert.ToDouble(txt_Input.Text) : Convert.ToDouble(Input));
				switch (button.Uid)
				{
				case "0":
					num2--;
					break;
				case "1":
					num2++;
					break;
				case "2":
					num2 -= 5.0;
					break;
				case "3":
					num2 += 5.0;
					break;
				case "4":
					num2 -= 10.0;
					break;
				case "5":
					num2 += 10.0;
					break;
				case "6":
					num2 -= 100.0;
					break;
				case "7":
					num2 += 100.0;
					break;
				}
				if (!UseNegative)
				{
					num2 = Math.Abs(num2);
				}
				txt_Input.Text = num2.ToString();
				if (txbox != null)
				{
					txbox.Text = txt_Input.Text;
				}
			}
		}
		catch
		{
		}
	}

	private void MainW_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			Close();
		}
	}

	private void MainW_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Return)
		{
			Close();
		}
	}

	private void MainW_Deactivated(object sender, EventArgs e)
	{
		if (!closeing)
		{
			Close();
		}
	}

	private void MainW_Closing(object sender, CancelEventArgs e)
	{
		closeing = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Cb_CommonDigits_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (UpdateCommonDigits)
		{
			bool flag = true;
			if (cb_CommonDigits.SelectedIndex <= 0)
			{
				flag = true;
				txt_Input.Text = "0";
				Input = "0";
				UseCommonDigits = false;
			}
			else
			{
				flag = false;
				UseCommonDigits = true;
				CommonDigitsIndex = cb_CommonDigits.SelectedIndex - 1;
				txt_Input.Text = string.Format("{0}:{1}={2}", CommonDigitsIndex + 1, Tfile.ComDigitsName[CommonDigitsIndex], Tfile.ComDigits[CommonDigitsIndex]);
			}
			ChangeComdigitsBtnState(flag);
		}
	}

	private void ChangeComdigitsBtnState(bool ON)
	{
		Button button = btn_1;
		Button button2 = btn_2;
		Button button3 = btn_3;
		Button button4 = btn_4;
		Button button5 = btn_5;
		Button button6 = btn_6;
		Button button7 = btn_7;
		bool flag = (btn_8.IsEnabled = ON);
		bool flag3 = (button7.IsEnabled = flag);
		bool flag5 = (button6.IsEnabled = flag3);
		bool flag7 = (button5.IsEnabled = flag5);
		bool flag9 = (button4.IsEnabled = flag7);
		bool flag11 = (button3.IsEnabled = flag9);
		bool isEnabled = (button2.IsEnabled = flag11);
		button.IsEnabled = isEnabled;
		Button button8 = btn_9;
		Button button9 = btn_0;
		Button button10 = btn_M1;
		Button button11 = btn_P1;
		Button button12 = btn_M5;
		Button button13 = btn_P5;
		Button button14 = btn_M10;
		flag = (btn_P10.IsEnabled = ON);
		flag3 = (button14.IsEnabled = flag);
		flag5 = (button13.IsEnabled = flag3);
		flag7 = (button12.IsEnabled = flag5);
		flag9 = (button11.IsEnabled = flag7);
		flag11 = (button10.IsEnabled = flag9);
		isEnabled = (button9.IsEnabled = flag11);
		button8.IsEnabled = isEnabled;
		Button button15 = btn_M100;
		Button button16 = btn_P100;
		flag11 = (btn_C.IsEnabled = ON);
		isEnabled = (button16.IsEnabled = flag11);
		button15.IsEnabled = isEnabled;
		if (UseNegative)
		{
			btn_point.IsEnabled = ON;
		}
		if (UseNegative)
		{
			btn_Negative.IsEnabled = ON;
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
			Uri resourceLocator = new Uri("/InspTools;component/other/keyboard.xaml", UriKind.Relative);
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
			MainW = (HKeyboard)target;
			MainW.Activated += Window_Activated;
			MainW.KeyDown += MainW_KeyDown;
			MainW.PreviewKeyDown += MainW_PreviewKeyDown;
			MainW.Deactivated += MainW_Deactivated;
			MainW.Closing += MainW_Closing;
			break;
		case 2:
			btn_1 = (Button)target;
			btn_1.Click += Button_Click;
			btn_1.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 3:
			btn_2 = (Button)target;
			btn_2.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 4:
			btn_3 = (Button)target;
			btn_3.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 5:
			btn_4 = (Button)target;
			btn_4.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 6:
			btn_5 = (Button)target;
			btn_5.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 7:
			btn_6 = (Button)target;
			btn_6.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 8:
			btn_7 = (Button)target;
			btn_7.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 9:
			btn_8 = (Button)target;
			btn_8.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 10:
			btn_9 = (Button)target;
			btn_9.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 11:
			btn_0 = (Button)target;
			btn_0.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 12:
			btn_point = (Button)target;
			btn_point.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			break;
		case 13:
			btn_Finish = (Button)target;
			btn_Finish.Click += btn_Finish_Click;
			break;
		case 14:
			btn_C = (Button)target;
			btn_C.Click += Button_Click_1;
			break;
		case 15:
			btn_Negative = (Button)target;
			btn_Negative.Click += btn_Negative_Click;
			break;
		case 16:
			txt_Input = (TextBox)target;
			break;
		case 17:
			btn_M1 = (Button)target;
			btn_M1.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 18:
			btn_M5 = (Button)target;
			btn_M5.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 19:
			btn_M10 = (Button)target;
			btn_M10.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 20:
			btn_M100 = (Button)target;
			btn_M100.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 21:
			btn_P1 = (Button)target;
			btn_P1.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 22:
			btn_P5 = (Button)target;
			btn_P5.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 23:
			btn_P10 = (Button)target;
			btn_P10.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 24:
			btn_P100 = (Button)target;
			btn_P100.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown_1;
			break;
		case 25:
			cb_CommonDigits = (ComboBox)target;
			cb_CommonDigits.SelectionChanged += Cb_CommonDigits_SelectionChanged;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
