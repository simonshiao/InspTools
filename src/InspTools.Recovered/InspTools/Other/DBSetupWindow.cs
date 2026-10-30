using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;

namespace InspTools.Other;

public class DBSetupWindow : Window, IComponentConnector
{
	private string[] okPath = new string[8];

	private string[] ngPath = new string[8];

	private string[] okPath_AI = new string[8];

	private string[] ngPath_AI = new string[8];

	private string[] okPath_B = new string[8];

	private string[] ngPath_B = new string[8];

	private string[] okPath_AI_B = new string[8];

	private string[] ngPath_AI_B = new string[8];

	private string CopyPathOK = "";

	private string CopyPathNG = "";

	private int LanguageType = 1;

	private TextBox[] OKBox;

	private TextBox[] NGBox;

	private Button[] OKBtn;

	private Button[] NGBtn;

	private Button[] CopyBtn;

	private Button[] PasteBtn;

	private string RootPath = "";

	private bool[] EnableSelectFace = new bool[8];

	private AI_MODE[] Mode = new AI_MODE[8];

	internal DBSetupWindow wnd_Main;

	internal TextBox txt_PathAll;

	internal Button btn_SetAll;

	internal TextBox txt_OKPathS1;

	internal Button btn_OpenOKS1;

	internal Label lab_StatinoIndex;

	internal TextBox txt_NGPathS1;

	internal Button btn_OpenNGS1;

	internal Button btn_RecipeCopy1;

	internal Button btn_RecipePaste1;

	internal TextBox txt_OKPathS2;

	internal Button btn_OpenOKS2;

	internal Label lab_StatinoIndex1;

	internal TextBox txt_NGPathS2;

	internal Button btn_OpenNGS2;

	internal Button btn_RecipeCopy2;

	internal Button btn_RecipePaste2;

	internal TextBox txt_OKPathS3;

	internal Button btn_OpenOKS3;

	internal Label lab_StatinoIndex2;

	internal TextBox txt_NGPathS3;

	internal Button btn_OpenNGS3;

	internal Button btn_RecipeCopy3;

	internal Button btn_RecipePaste3;

	internal TextBox txt_OKPathS4;

	internal Button btn_OpenOKS4;

	internal Label lab_StatinoIndex3;

	internal TextBox txt_NGPathS4;

	internal Button btn_OpenNGS4;

	internal Button btn_RecipeCopy4;

	internal Button btn_RecipePaste4;

	internal TextBox txt_OKPathS5;

	internal Button btn_OpenOKS5;

	internal Label lab_StatinoIndex4;

	internal TextBox txt_NGPathS5;

	internal Button btn_OpenNGS5;

	internal Button btn_RecipeCopy5;

	internal Button btn_RecipePaste5;

	internal TextBox txt_OKPathS6;

	internal Button btn_OpenOKS6;

	internal Label lab_StatinoIndex5;

	internal TextBox txt_NGPathS6;

	internal Button btn_OpenNGS6;

	internal Button btn_RecipeCopy6;

	internal Button btn_RecipePaste6;

	internal TextBox txt_OKPathS7;

	internal Button btn_OpenOKS7;

	internal Label lab_StatinoIndex6;

	internal TextBox txt_NGPathS7;

	internal Button btn_OpenNGS7;

	internal Button btn_RecipeCopy7;

	internal Button btn_RecipePaste7;

	internal TextBox txt_OKPathS8;

	internal Button btn_OpenOKS8;

	internal Label lab_StatinoIndex7;

	internal TextBox txt_NGPathS8;

	internal Button btn_OpenNGS8;

	internal Button btn_RecipeCopy8;

	internal Button btn_RecipePaste8;

	internal TextBox txt_OKPathS9;

	internal Button btn_OpenOKS9;

	internal Label lab_StatinoIndex8;

	internal TextBox txt_NGPathS9;

	internal Button btn_OpenNGS9;

	internal Button btn_RecipeCopy9;

	internal Button btn_RecipePaste9;

	internal TextBox txt_OKPathS10;

	internal Button btn_OpenOKS10;

	internal Label lab_StatinoIndex9;

	internal TextBox txt_NGPathS10;

	internal Button btn_OpenNGS10;

	internal Button btn_RecipeCopy10;

	internal Button btn_RecipePaste10;

	private bool _contentLoaded;

	public DBSetupWindow()
	{
		InitializeComponent();
		OKBox = new TextBox[8] { txt_OKPathS1, txt_OKPathS2, txt_OKPathS3, txt_OKPathS4, txt_OKPathS5, txt_OKPathS6, txt_OKPathS7, txt_OKPathS8 };
		NGBox = new TextBox[8] { txt_NGPathS1, txt_NGPathS2, txt_NGPathS3, txt_NGPathS4, txt_NGPathS5, txt_NGPathS6, txt_NGPathS7, txt_NGPathS8 };
		OKBtn = new Button[8] { btn_OpenOKS1, btn_OpenOKS2, btn_OpenOKS3, btn_OpenOKS4, btn_OpenOKS5, btn_OpenOKS6, btn_OpenOKS7, btn_OpenOKS8 };
		NGBtn = new Button[8] { btn_OpenNGS1, btn_OpenNGS2, btn_OpenNGS3, btn_OpenNGS4, btn_OpenNGS5, btn_OpenNGS6, btn_OpenNGS7, btn_OpenNGS8 };
		CopyBtn = new Button[8] { btn_RecipeCopy1, btn_RecipeCopy2, btn_RecipeCopy3, btn_RecipeCopy4, btn_RecipeCopy5, btn_RecipeCopy6, btn_RecipeCopy7, btn_RecipeCopy8 };
		PasteBtn = new Button[8] { btn_RecipePaste1, btn_RecipePaste2, btn_RecipePaste3, btn_RecipePaste4, btn_RecipePaste5, btn_RecipePaste6, btn_RecipePaste7, btn_RecipePaste8 };
		for (int i = 0; i < 8; i++)
		{
			Mode[i] = AI_MODE.EM_NoUse;
			EnableSelectFace[i] = false;
		}
	}

	public void SetFunctionName(string str)
	{
		wnd_Main.Content = str;
	}

	public void SetMode(AI_MODE[] mode, bool[] enableSelectFace)
	{
		Mode = mode;
		for (int i = 0; i < 8; i++)
		{
			EnableSelectFace[i] = enableSelectFace[i];
			if (mode[i] == AI_MODE.EM_Enhance)
			{
				OKBtn[i].Visibility = Visibility.Hidden;
				NGBtn[i].Visibility = Visibility.Hidden;
				CopyBtn[i].Visibility = Visibility.Hidden;
				PasteBtn[i].Visibility = Visibility.Hidden;
			}
		}
	}

	public void SetPath(string[] OKPath_A, string[] NGPath_A, string[] OKPath_B, string[] NGPath_B)
	{
		for (int i = 0; i < 8; i++)
		{
			okPath[i] = OKPath_A[i];
			ngPath[i] = NGPath_A[i];
			okPath_B[i] = OKPath_B[i];
			ngPath_B[i] = NGPath_B[i];
			OKBox[i].Text = OKPath_A[i];
			NGBox[i].Text = NGPath_A[i];
		}
	}

	public void SetPath(string[] OKPath, string[] NGPath)
	{
		for (int i = 0; i < 8; i++)
		{
			okPath[i] = OKPath[i];
			ngPath[i] = NGPath[i];
			OKBox[i].Text = OKPath[i];
			NGBox[i].Text = NGPath[i];
		}
	}

	public void SetPath(int index, string OKPath, string NGPath)
	{
		okPath[index] = OKPath;
		ngPath[index] = NGPath;
		OKBox[index].Text = OKPath;
		NGBox[index].Text = NGPath;
	}

	public void SetPath(int index, string OKPath_A, string NGPath_A, string OKPath_B, string NGPath_B)
	{
		okPath[index] = OKPath_A;
		ngPath[index] = NGPath_A;
		okPath_B[index] = OKPath_B;
		ngPath_B[index] = NGPath_B;
		OKBox[index].Text = OKPath_A;
		NGBox[index].Text = NGPath_A;
	}

	public void GetPath(ref string[] OKPath, ref string[] NGPath)
	{
		for (int i = 0; i < 8; i++)
		{
			OKPath[i] = okPath[i];
			NGPath[i] = ngPath[i];
		}
	}

	public void GetPath(int index, ref string OKPath, ref string NGPath)
	{
		OKPath = okPath[index];
		NGPath = ngPath[index];
	}

	public void GetPath(int index, ref string OKPath, ref string NGPath, ref string OKPathAI, ref string NGPathAI)
	{
		OKPath = okPath[index];
		NGPath = ngPath[index];
		OKPathAI = okPath_AI[index];
		NGPathAI = ngPath_AI[index];
	}

	public void GetPath(int index, ref string OKPath_A, ref string NGPath_A, ref string OKPathAI_A, ref string NGPathAI_A, ref string OKPath_B, ref string NGPath_B, ref string OKPathAI_B, ref string NGPathAI_B)
	{
		OKPath_A = okPath[index];
		NGPath_A = ngPath[index];
		OKPathAI_A = okPath_AI[index];
		NGPathAI_A = ngPath_AI[index];
		OKPath_B = okPath_B[index];
		NGPath_B = ngPath_B[index];
		OKPathAI_B = okPath_AI_B[index];
		NGPathAI_B = ngPath_AI_B[index];
	}

	public void SetRootPath(string str)
	{
		string rootPath = (txt_PathAll.Text = str);
		RootPath = rootPath;
	}

	public string GetRootPath()
	{
		return RootPath;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenOKS1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (folderSelectDialog.ShowDialog())
		{
			okPath[num] = folderSelectDialog.FileName + "\\";
			OKBox[num].Text = folderSelectDialog.FileName + "\\";
			if (!Directory.Exists(okPath[num]))
			{
				Directory.CreateDirectory(okPath[num]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_OpenNGS1_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (folderSelectDialog.ShowDialog())
		{
			ngPath[num] = folderSelectDialog.FileName + "\\";
			NGBox[num].Text = folderSelectDialog.FileName + "\\";
			if (!Directory.Exists(ngPath[num]))
			{
				Directory.CreateDirectory(ngPath[num]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_SetAll_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Will Reset All OK and NG Database path！", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("将会重设各站的OK及NG资料库路径！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("將會重設各站的OK及NG資料庫路徑！", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
		if (!folderSelectDialog.ShowDialog())
		{
			return;
		}
		RootPath = folderSelectDialog.FileName;
		for (int i = 0; i < 8; i++)
		{
			string arg = "";
			string text = "";
			string text2;
			if (Mode[i] == AI_MODE.EM_Enhance)
			{
				if (EnableSelectFace[i])
				{
					arg = "A\\";
					text = "B\\";
					text2 = string.Format("{0}\\{1}\\{1}X\\", folderSelectDialog.FileName, i + 1, text);
					okPath_B[i] = text2;
					if (!Directory.Exists(okPath_B[i]))
					{
						Directory.CreateDirectory(okPath_B[i]);
					}
					text2 = string.Format("{0}\\{1}\\{1}Y\\", folderSelectDialog.FileName, i + 1, text);
					ngPath_B[i] = text2;
					if (!Directory.Exists(ngPath_B[i]))
					{
						Directory.CreateDirectory(ngPath_B[i]);
					}
					text2 = string.Format("{0}\\{1}\\{1}OK\\", folderSelectDialog.FileName, i + 1, text);
					okPath_AI_B[i] = text2;
					if (!Directory.Exists(okPath_AI_B[i]))
					{
						Directory.CreateDirectory(okPath_AI_B[i]);
					}
					text2 = string.Format("{0}\\{1}\\{1}NG\\", folderSelectDialog.FileName, i + 1, text);
					ngPath_AI_B[i] = text2;
					if (!Directory.Exists(ngPath_AI_B[i]))
					{
						Directory.CreateDirectory(ngPath_AI_B[i]);
					}
				}
				text2 = string.Format("{0}\\{1}\\{2}X\\", folderSelectDialog.FileName, i + 1, arg);
				okPath[i] = text2;
				OKBox[i].Text = text2;
				if (!Directory.Exists(okPath[i]))
				{
					Directory.CreateDirectory(okPath[i]);
				}
				text2 = string.Format("{0}\\{1}\\{2}Y\\", folderSelectDialog.FileName, i + 1, arg);
				ngPath[i] = text2;
				NGBox[i].Text = text2;
				if (!Directory.Exists(ngPath[i]))
				{
					Directory.CreateDirectory(ngPath[i]);
				}
				text2 = string.Format("{0}\\{1}\\{2}OK\\", folderSelectDialog.FileName, i + 1, arg);
				okPath_AI[i] = text2;
				if (!Directory.Exists(okPath_AI[i]))
				{
					Directory.CreateDirectory(okPath_AI[i]);
				}
				text2 = string.Format("{0}\\{1}\\{2}NG\\", folderSelectDialog.FileName, i + 1, arg);
				ngPath_AI[i] = text2;
				if (!Directory.Exists(ngPath_AI[i]))
				{
					Directory.CreateDirectory(ngPath_AI[i]);
				}
				continue;
			}
			if (EnableSelectFace[i])
			{
				arg = "A\\";
				text = "B\\";
				text2 = string.Format("{0}\\{1}\\{2}OK\\", folderSelectDialog.FileName, i + 1, text);
				okPath_B[i] = text2;
				if (!Directory.Exists(okPath_B[i]))
				{
					Directory.CreateDirectory(okPath_B[i]);
				}
				text2 = string.Format("{0}\\{1}\\{2}NG\\", folderSelectDialog.FileName, i + 1, text);
				ngPath_B[i] = text2;
				if (!Directory.Exists(ngPath_B[i]))
				{
					Directory.CreateDirectory(ngPath_B[i]);
				}
			}
			text2 = string.Format("{0}\\{1}\\{2}OK\\", folderSelectDialog.FileName, i + 1, arg);
			okPath[i] = text2;
			OKBox[i].Text = text2;
			if (!Directory.Exists(okPath[i]))
			{
				Directory.CreateDirectory(okPath[i]);
			}
			text2 = string.Format("{0}\\{1}\\{2}NG\\", folderSelectDialog.FileName, i + 1, arg);
			ngPath[i] = text2;
			NGBox[i].Text = text2;
			if (!Directory.Exists(ngPath[i]))
			{
				Directory.CreateDirectory(ngPath[i]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LoadLanguage(int language)
	{
		switch (language)
		{
		case 1:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-TW"));
			break;
		case 2:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-CN"));
			break;
		case 3:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("ja-JP"));
			break;
		default:
			LanguageManager.Instance.ChangeLanguage(new CultureInfo("en-US"));
			break;
		}
		_ = AppDomain.CurrentDomain.BaseDirectory;
		LanguageType = language;
	}

	private void Btn_RecipeCopy_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (!(OKBox[num].Text == "") && Directory.Exists(OKBox[num].Text) && !(NGBox[num].Text == "") && Directory.Exists(NGBox[num].Text))
		{
			for (int i = 0; i < 8; i++)
			{
				CopyBtn[i].Background = Brushes.White;
				PasteBtn[num].IsEnabled = true;
			}
			CopyBtn[num].Background = Brushes.LightYellow;
			CopyPathOK = OKBox[num].Text;
			CopyPathNG = NGBox[num].Text;
		}
	}

	private void Btn_RecipePaste_Click(object sender, RoutedEventArgs e)
	{
		int num = Convert.ToInt32(((Button)sender).Uid);
		if (CopyPathOK != "" && CopyPathNG != "")
		{
			OKBox[num].Text = CopyPathOK;
			NGBox[num].Text = CopyPathNG;
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
			Uri resourceLocator = new Uri("/InspTools;component/other/dbsetupwindow.xaml", UriKind.Relative);
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
			wnd_Main = (DBSetupWindow)target;
			break;
		case 2:
			txt_PathAll = (TextBox)target;
			break;
		case 3:
			btn_SetAll = (Button)target;
			btn_SetAll.Click += Btn_SetAll_Click;
			break;
		case 4:
			txt_OKPathS1 = (TextBox)target;
			break;
		case 5:
			btn_OpenOKS1 = (Button)target;
			btn_OpenOKS1.Click += Btn_OpenOKS1_Click;
			break;
		case 6:
			lab_StatinoIndex = (Label)target;
			break;
		case 7:
			txt_NGPathS1 = (TextBox)target;
			break;
		case 8:
			btn_OpenNGS1 = (Button)target;
			btn_OpenNGS1.Click += Btn_OpenNGS1_Click;
			break;
		case 9:
			btn_RecipeCopy1 = (Button)target;
			btn_RecipeCopy1.Click += Btn_RecipeCopy_Click;
			break;
		case 10:
			btn_RecipePaste1 = (Button)target;
			btn_RecipePaste1.Click += Btn_RecipePaste_Click;
			break;
		case 11:
			txt_OKPathS2 = (TextBox)target;
			break;
		case 12:
			btn_OpenOKS2 = (Button)target;
			btn_OpenOKS2.Click += Btn_OpenOKS1_Click;
			break;
		case 13:
			lab_StatinoIndex1 = (Label)target;
			break;
		case 14:
			txt_NGPathS2 = (TextBox)target;
			break;
		case 15:
			btn_OpenNGS2 = (Button)target;
			btn_OpenNGS2.Click += Btn_OpenNGS1_Click;
			break;
		case 16:
			btn_RecipeCopy2 = (Button)target;
			btn_RecipeCopy2.Click += Btn_RecipeCopy_Click;
			break;
		case 17:
			btn_RecipePaste2 = (Button)target;
			btn_RecipePaste2.Click += Btn_RecipePaste_Click;
			break;
		case 18:
			txt_OKPathS3 = (TextBox)target;
			break;
		case 19:
			btn_OpenOKS3 = (Button)target;
			btn_OpenOKS3.Click += Btn_OpenOKS1_Click;
			break;
		case 20:
			lab_StatinoIndex2 = (Label)target;
			break;
		case 21:
			txt_NGPathS3 = (TextBox)target;
			break;
		case 22:
			btn_OpenNGS3 = (Button)target;
			btn_OpenNGS3.Click += Btn_OpenNGS1_Click;
			break;
		case 23:
			btn_RecipeCopy3 = (Button)target;
			btn_RecipeCopy3.Click += Btn_RecipeCopy_Click;
			break;
		case 24:
			btn_RecipePaste3 = (Button)target;
			btn_RecipePaste3.Click += Btn_RecipePaste_Click;
			break;
		case 25:
			txt_OKPathS4 = (TextBox)target;
			break;
		case 26:
			btn_OpenOKS4 = (Button)target;
			btn_OpenOKS4.Click += Btn_OpenOKS1_Click;
			break;
		case 27:
			lab_StatinoIndex3 = (Label)target;
			break;
		case 28:
			txt_NGPathS4 = (TextBox)target;
			break;
		case 29:
			btn_OpenNGS4 = (Button)target;
			btn_OpenNGS4.Click += Btn_OpenNGS1_Click;
			break;
		case 30:
			btn_RecipeCopy4 = (Button)target;
			btn_RecipeCopy4.Click += Btn_RecipeCopy_Click;
			break;
		case 31:
			btn_RecipePaste4 = (Button)target;
			btn_RecipePaste4.Click += Btn_RecipePaste_Click;
			break;
		case 32:
			txt_OKPathS5 = (TextBox)target;
			break;
		case 33:
			btn_OpenOKS5 = (Button)target;
			btn_OpenOKS5.Click += Btn_OpenOKS1_Click;
			break;
		case 34:
			lab_StatinoIndex4 = (Label)target;
			break;
		case 35:
			txt_NGPathS5 = (TextBox)target;
			break;
		case 36:
			btn_OpenNGS5 = (Button)target;
			btn_OpenNGS5.Click += Btn_OpenNGS1_Click;
			break;
		case 37:
			btn_RecipeCopy5 = (Button)target;
			btn_RecipeCopy5.Click += Btn_RecipeCopy_Click;
			break;
		case 38:
			btn_RecipePaste5 = (Button)target;
			btn_RecipePaste5.Click += Btn_RecipePaste_Click;
			break;
		case 39:
			txt_OKPathS6 = (TextBox)target;
			break;
		case 40:
			btn_OpenOKS6 = (Button)target;
			btn_OpenOKS6.Click += Btn_OpenOKS1_Click;
			break;
		case 41:
			lab_StatinoIndex5 = (Label)target;
			break;
		case 42:
			txt_NGPathS6 = (TextBox)target;
			break;
		case 43:
			btn_OpenNGS6 = (Button)target;
			btn_OpenNGS6.Click += Btn_OpenNGS1_Click;
			break;
		case 44:
			btn_RecipeCopy6 = (Button)target;
			btn_RecipeCopy6.Click += Btn_RecipeCopy_Click;
			break;
		case 45:
			btn_RecipePaste6 = (Button)target;
			btn_RecipePaste6.Click += Btn_RecipePaste_Click;
			break;
		case 46:
			txt_OKPathS7 = (TextBox)target;
			break;
		case 47:
			btn_OpenOKS7 = (Button)target;
			btn_OpenOKS7.Click += Btn_OpenOKS1_Click;
			break;
		case 48:
			lab_StatinoIndex6 = (Label)target;
			break;
		case 49:
			txt_NGPathS7 = (TextBox)target;
			break;
		case 50:
			btn_OpenNGS7 = (Button)target;
			btn_OpenNGS7.Click += Btn_OpenNGS1_Click;
			break;
		case 51:
			btn_RecipeCopy7 = (Button)target;
			btn_RecipeCopy7.Click += Btn_RecipeCopy_Click;
			break;
		case 52:
			btn_RecipePaste7 = (Button)target;
			btn_RecipePaste7.Click += Btn_RecipePaste_Click;
			break;
		case 53:
			txt_OKPathS8 = (TextBox)target;
			break;
		case 54:
			btn_OpenOKS8 = (Button)target;
			btn_OpenOKS8.Click += Btn_OpenOKS1_Click;
			break;
		case 55:
			lab_StatinoIndex7 = (Label)target;
			break;
		case 56:
			txt_NGPathS8 = (TextBox)target;
			break;
		case 57:
			btn_OpenNGS8 = (Button)target;
			btn_OpenNGS8.Click += Btn_OpenNGS1_Click;
			break;
		case 58:
			btn_RecipeCopy8 = (Button)target;
			btn_RecipeCopy8.Click += Btn_RecipeCopy_Click;
			break;
		case 59:
			btn_RecipePaste8 = (Button)target;
			btn_RecipePaste8.Click += Btn_RecipePaste_Click;
			break;
		case 60:
			txt_OKPathS9 = (TextBox)target;
			break;
		case 61:
			btn_OpenOKS9 = (Button)target;
			btn_OpenOKS9.Click += Btn_OpenOKS1_Click;
			break;
		case 62:
			lab_StatinoIndex8 = (Label)target;
			break;
		case 63:
			txt_NGPathS9 = (TextBox)target;
			break;
		case 64:
			btn_OpenNGS9 = (Button)target;
			btn_OpenNGS9.Click += Btn_OpenNGS1_Click;
			break;
		case 65:
			btn_RecipeCopy9 = (Button)target;
			btn_RecipeCopy9.Click += Btn_RecipeCopy_Click;
			break;
		case 66:
			btn_RecipePaste9 = (Button)target;
			btn_RecipePaste9.Click += Btn_RecipePaste_Click;
			break;
		case 67:
			txt_OKPathS10 = (TextBox)target;
			break;
		case 68:
			btn_OpenOKS10 = (Button)target;
			btn_OpenOKS10.Click += Btn_OpenOKS1_Click;
			break;
		case 69:
			lab_StatinoIndex9 = (Label)target;
			break;
		case 70:
			txt_NGPathS10 = (TextBox)target;
			break;
		case 71:
			btn_OpenNGS10 = (Button)target;
			btn_OpenNGS10.Click += Btn_OpenNGS1_Click;
			break;
		case 72:
			btn_RecipeCopy10 = (Button)target;
			btn_RecipeCopy10.Click += Btn_RecipeCopy_Click;
			break;
		case 73:
			btn_RecipePaste10 = (Button)target;
			btn_RecipePaste10.Click += Btn_RecipePaste_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
