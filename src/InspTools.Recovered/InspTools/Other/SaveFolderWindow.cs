using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.Resources;

namespace InspTools.Other;

public class SaveFolderWindow : Window, IComponentConnector
{
	private Button[] BTNSAVE;

	private string PathX;

	private string PathY;

	private const int MaxFolder = 32;

	internal SaveFolderWindow wnd_main;

	internal Button btn_Save2Folder_1;

	internal Button btn_AddNewFolder;

	internal Button btn_Save2Folder_2;

	internal Button btn_Save2Folder_3;

	internal Button btn_Save2Folder_4;

	internal Button btn_Save2Folder_5;

	internal Button btn_Save2Folder_6;

	internal Button btn_Save2Folder_7;

	internal Button btn_Save2Folder_8;

	internal Button btn_Save2Folder_9;

	internal Button btn_Save2Folder_10;

	internal Button btn_Save2Folder_11;

	internal Button btn_Save2Folder_12;

	internal Button btn_Save2Folder_13;

	internal Button btn_Save2Folder_14;

	internal Button btn_Save2Folder_16;

	internal Button btn_Save2Folder_17;

	internal Button btn_Save2Folder_18;

	internal Button btn_Save2Folder_15;

	internal CheckBox chk_ChangeName;

	internal Button btn_Save2Folder_19;

	internal Button btn_Save2Folder_20;

	internal Button btn_Save2Folder_21;

	internal Button btn_Save2Folder_23;

	internal Button btn_Save2Folder_24;

	internal Button btn_Save2Folder_25;

	internal Button btn_Save2Folder_22;

	internal Button btn_Save2Folder_26;

	internal Button btn_Save2Folder_27;

	internal Button btn_Save2Folder_28;

	internal Button btn_Save2Folder_30;

	internal Button btn_Save2Folder_31;

	internal Button btn_Save2Folder_32;

	internal Button btn_Save2Folder_29;

	internal Button btn_Save2Folder_33;

	private bool _contentLoaded;

	public SaveFolderWindow()
	{
		InitializeComponent();
		BTNSAVE = new Button[32]
		{
			btn_Save2Folder_2, btn_Save2Folder_3, btn_Save2Folder_4, btn_Save2Folder_5, btn_Save2Folder_6, btn_Save2Folder_7, btn_Save2Folder_8, btn_Save2Folder_9, btn_Save2Folder_10, btn_Save2Folder_11,
			btn_Save2Folder_12, btn_Save2Folder_13, btn_Save2Folder_14, btn_Save2Folder_15, btn_Save2Folder_16, btn_Save2Folder_17, btn_Save2Folder_18, btn_Save2Folder_19, btn_Save2Folder_20, btn_Save2Folder_21,
			btn_Save2Folder_22, btn_Save2Folder_23, btn_Save2Folder_24, btn_Save2Folder_25, btn_Save2Folder_26, btn_Save2Folder_27, btn_Save2Folder_28, btn_Save2Folder_29, btn_Save2Folder_30, btn_Save2Folder_31,
			btn_Save2Folder_32, btn_Save2Folder_33
		};
	}

	public void SetFolder(string pathY, string pathX)
	{
		PathX = pathX;
		PathY = pathY;
		UpdateFolder();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateFolder()
	{
		string[] directories = Directory.GetDirectories(PathX);
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			BTNSAVE[i].IsEnabled = false;
			BTNSAVE[i].Content = "";
			BTNSAVE[i].Visibility = Visibility.Hidden;
		}
		for (int j = 0; j < directories.Length; j++)
		{
			if (j < 32 && !(Path.GetFileName(directories[j]) == "ALL"))
			{
				BTNSAVE[num].IsEnabled = true;
				BTNSAVE[num].Content = Path.GetFileName(directories[j]);
				BTNSAVE[num].Visibility = Visibility.Visible;
				BTNSAVE[num].Background = Brushes.Yellow;
				num++;
			}
		}
	}

	public void GetFolder(ref string pathY, ref string pathX)
	{
		pathX = PathX;
		pathY = PathY;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_AddNewFolder_Click(object sender, RoutedEventArgs e)
	{
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.SetTitle(LanguageManager.Instance["InputNewModelName"]);
		itemInputWindow.SetTitle("請輸入資料夾名稱");
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr == "")
		{
			return;
		}
		if (itemInputWindow.InputStr.Contains("\\") || itemInputWindow.InputStr.Contains("/") || itemInputWindow.InputStr.Contains(":") || itemInputWindow.InputStr.Contains("\"") || itemInputWindow.InputStr.Contains("<") || itemInputWindow.InputStr.Contains(">") || itemInputWindow.InputStr.Contains("|") || itemInputWindow.InputStr.Contains("?") || itemInputWindow.InputStr.Contains("*") || itemInputWindow.InputStr.Contains("."))
		{
			MessageBox.Show("資料夾名稱包含不合法字元 \\ / : * \" < > | ? .", LanguageManager.Instance["Warning"]);
			return;
		}
		string[] directories = Directory.GetDirectories(PathX);
		for (int i = 0; i < directories.Length; i++)
		{
			if (itemInputWindow.InputStr == Path.GetFileName(directories[i]))
			{
				MessageBox.Show(string.Format("[{0}] 資料夾名稱已經存在", itemInputWindow.InputStr), "警告");
				return;
			}
		}
		string path = string.Format("{0}{1}\\", PathX, itemInputWindow.InputStr);
		string path2 = string.Format("{0}{1}\\", PathY, itemInputWindow.InputStr);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		if (!Directory.Exists(path2))
		{
			Directory.CreateDirectory(path2);
		}
		UpdateFolder();
	}

	private void btn_Save2Folder_Default(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void chk_ChangeName_Checked(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 32; i++)
		{
			BTNSAVE[i].Background = Brushes.OrangeRed;
		}
	}

	private void chk_ChangeName_Unchecked(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < 32; i++)
		{
			BTNSAVE[i].Background = Brushes.Yellow;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_Save2Folder_1_Click(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		Convert.ToInt32(button.Uid);
		if (chk_ChangeName.IsChecked == false)
		{
			PathX = PathX + button.Content?.ToString() + "\\";
			PathY = PathY + button.Content?.ToString() + "\\";
			Close();
			return;
		}
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.SetTitle(LanguageManager.Instance["InputNewModelName"]);
		itemInputWindow.SetTitle("請輸入資料夾名稱");
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr == "")
		{
			return;
		}
		if (itemInputWindow.InputStr.Contains("\\") || itemInputWindow.InputStr.Contains("/") || itemInputWindow.InputStr.Contains(":") || itemInputWindow.InputStr.Contains("\"") || itemInputWindow.InputStr.Contains("<") || itemInputWindow.InputStr.Contains(">") || itemInputWindow.InputStr.Contains("|") || itemInputWindow.InputStr.Contains("?") || itemInputWindow.InputStr.Contains("*") || itemInputWindow.InputStr.Contains("."))
		{
			MessageBox.Show("資料夾名稱包含不合法字元 \\ / : * \" < > | ? .", LanguageManager.Instance["Warning"]);
			return;
		}
		string[] directories = Directory.GetDirectories(PathX);
		for (int i = 0; i < directories.Length; i++)
		{
			if (itemInputWindow.InputStr == Path.GetFileName(directories[i]))
			{
				MessageBox.Show(string.Format("[{0}] 資料夾名稱已經存在", itemInputWindow.InputStr), "警告");
				return;
			}
		}
		string sourceDirName = string.Format("{0}{1}\\", PathX, button.Content);
		string destDirName = string.Format("{0}{1}\\", PathX, itemInputWindow.InputStr);
		Directory.Move(sourceDirName, destDirName);
		string sourceDirName2 = string.Format("{0}{1}\\", PathY, button.Content);
		destDirName = string.Format("{0}{1}\\", PathY, itemInputWindow.InputStr);
		Directory.Move(sourceDirName2, destDirName);
		UpdateFolder();
		chk_ChangeName.IsChecked = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/savefolderwindow.xaml", UriKind.Relative);
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
			wnd_main = (SaveFolderWindow)target;
			break;
		case 2:
			btn_Save2Folder_1 = (Button)target;
			btn_Save2Folder_1.Click += btn_Save2Folder_Default;
			break;
		case 3:
			btn_AddNewFolder = (Button)target;
			btn_AddNewFolder.Click += btn_AddNewFolder_Click;
			break;
		case 4:
			btn_Save2Folder_2 = (Button)target;
			btn_Save2Folder_2.Click += btn_Save2Folder_1_Click;
			break;
		case 5:
			btn_Save2Folder_3 = (Button)target;
			btn_Save2Folder_3.Click += btn_Save2Folder_1_Click;
			break;
		case 6:
			btn_Save2Folder_4 = (Button)target;
			btn_Save2Folder_4.Click += btn_Save2Folder_1_Click;
			break;
		case 7:
			btn_Save2Folder_5 = (Button)target;
			btn_Save2Folder_5.Click += btn_Save2Folder_1_Click;
			break;
		case 8:
			btn_Save2Folder_6 = (Button)target;
			btn_Save2Folder_6.Click += btn_Save2Folder_1_Click;
			break;
		case 9:
			btn_Save2Folder_7 = (Button)target;
			btn_Save2Folder_7.Click += btn_Save2Folder_1_Click;
			break;
		case 10:
			btn_Save2Folder_8 = (Button)target;
			btn_Save2Folder_8.Click += btn_Save2Folder_1_Click;
			break;
		case 11:
			btn_Save2Folder_9 = (Button)target;
			btn_Save2Folder_9.Click += btn_Save2Folder_1_Click;
			break;
		case 12:
			btn_Save2Folder_10 = (Button)target;
			btn_Save2Folder_10.Click += btn_Save2Folder_1_Click;
			break;
		case 13:
			btn_Save2Folder_11 = (Button)target;
			btn_Save2Folder_11.Click += btn_Save2Folder_1_Click;
			break;
		case 14:
			btn_Save2Folder_12 = (Button)target;
			btn_Save2Folder_12.Click += btn_Save2Folder_1_Click;
			break;
		case 15:
			btn_Save2Folder_13 = (Button)target;
			btn_Save2Folder_13.Click += btn_Save2Folder_1_Click;
			break;
		case 16:
			btn_Save2Folder_14 = (Button)target;
			btn_Save2Folder_14.Click += btn_Save2Folder_1_Click;
			break;
		case 17:
			btn_Save2Folder_16 = (Button)target;
			btn_Save2Folder_16.Click += btn_Save2Folder_1_Click;
			break;
		case 18:
			btn_Save2Folder_17 = (Button)target;
			btn_Save2Folder_17.Click += btn_Save2Folder_1_Click;
			break;
		case 19:
			btn_Save2Folder_18 = (Button)target;
			btn_Save2Folder_18.Click += btn_Save2Folder_1_Click;
			break;
		case 20:
			btn_Save2Folder_15 = (Button)target;
			btn_Save2Folder_15.Click += btn_Save2Folder_1_Click;
			break;
		case 21:
			chk_ChangeName = (CheckBox)target;
			chk_ChangeName.Checked += chk_ChangeName_Checked;
			chk_ChangeName.Unchecked += chk_ChangeName_Unchecked;
			break;
		case 22:
			btn_Save2Folder_19 = (Button)target;
			btn_Save2Folder_19.Click += btn_Save2Folder_1_Click;
			break;
		case 23:
			btn_Save2Folder_20 = (Button)target;
			btn_Save2Folder_20.Click += btn_Save2Folder_1_Click;
			break;
		case 24:
			btn_Save2Folder_21 = (Button)target;
			btn_Save2Folder_21.Click += btn_Save2Folder_1_Click;
			break;
		case 25:
			btn_Save2Folder_23 = (Button)target;
			btn_Save2Folder_23.Click += btn_Save2Folder_1_Click;
			break;
		case 26:
			btn_Save2Folder_24 = (Button)target;
			btn_Save2Folder_24.Click += btn_Save2Folder_1_Click;
			break;
		case 27:
			btn_Save2Folder_25 = (Button)target;
			btn_Save2Folder_25.Click += btn_Save2Folder_1_Click;
			break;
		case 28:
			btn_Save2Folder_22 = (Button)target;
			btn_Save2Folder_22.Click += btn_Save2Folder_1_Click;
			break;
		case 29:
			btn_Save2Folder_26 = (Button)target;
			btn_Save2Folder_26.Click += btn_Save2Folder_1_Click;
			break;
		case 30:
			btn_Save2Folder_27 = (Button)target;
			btn_Save2Folder_27.Click += btn_Save2Folder_1_Click;
			break;
		case 31:
			btn_Save2Folder_28 = (Button)target;
			btn_Save2Folder_28.Click += btn_Save2Folder_1_Click;
			break;
		case 32:
			btn_Save2Folder_30 = (Button)target;
			btn_Save2Folder_30.Click += btn_Save2Folder_1_Click;
			break;
		case 33:
			btn_Save2Folder_31 = (Button)target;
			btn_Save2Folder_31.Click += btn_Save2Folder_1_Click;
			break;
		case 34:
			btn_Save2Folder_32 = (Button)target;
			btn_Save2Folder_32.Click += btn_Save2Folder_1_Click;
			break;
		case 35:
			btn_Save2Folder_29 = (Button)target;
			btn_Save2Folder_29.Click += btn_Save2Folder_1_Click;
			break;
		case 36:
			btn_Save2Folder_33 = (Button)target;
			btn_Save2Folder_33.Click += btn_Save2Folder_1_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
