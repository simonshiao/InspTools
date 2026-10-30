using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using InspTools.AOI;
using InspTools.Resources;
using InspTools.Station;

namespace InspTools.Other;

public class LocateModelsWindow : Window, IComponentConnector
{
	public struct ModelItem
	{
		public string ModelName { get; set; }

		public string ModelUsed { get; set; }

		public string ModelTimes { get; set; }

		public string ModelFullName { get; set; }
	}

	private int LanguageType = 1;

	private CVInspection Station;

	private List<ModelItem> MItems = new List<ModelItem>();

	private List<ModelItem> SearchItems = new List<ModelItem>();

	private string SelectModelName = "";

	private string CurrentModelName = "";

	private bool SearchMode;

	private bool ModelSetuped;

	internal ListView lv_ModelList;

	internal TextBox txt_SearchBox;

	internal Button btn_ModelSearch;

	internal Button btn_CreateModel;

	internal Button btn_RemoveModel;

	internal Button btn_LoadModel;

	internal Button btn_ModelHome;

	internal TextBox txt_CurrentModel;

	internal Button btn_RenameModel;

	internal Button btn_ReSetModel;

	private bool _contentLoaded;

	public LocateModelsWindow()
	{
		InitializeComponent();
	}

	public void SetInspection(CVInspection station)
	{
		Station = station;
		UpdateModels();
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

	public void SetCurrentModelName(string name)
	{
		CurrentModelName = name;
	}

	public string GetSelectModelName()
	{
		return SelectModelName;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateModels()
	{
		try
		{
			lv_ModelList.ItemsSource = null;
			string path = string.Format("{0}{1}\\", AppDomain.CurrentDomain.BaseDirectory, "Model");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
			MItems.Clear();
			string[] array = files;
			foreach (string text in array)
			{
				string extension = Path.GetExtension(text);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				if (fileNameWithoutExtension.Contains("-TempFile"))
				{
					continue;
				}
				string text2 = ".lh5d";
				if (extension.ToLower() == text2)
				{
					ModelItem item = new ModelItem
					{
						ModelName = fileNameWithoutExtension
					};
					if (new FileInfo(text).Length < 10)
					{
						item.ModelName += "(未訓練)";
					}
					item.ModelUsed = GetModelUsed(Path.GetFileName(text));
					item.ModelFullName = Path.GetFileName(text);
					FileInfo fileInfo = new FileInfo(text);
					item.ModelTimes = fileInfo.LastWriteTime.ToString();
					MItems.Add(item);
				}
			}
			lv_ModelList.ItemsSource = MItems;
			if (!(CurrentModelName != ""))
			{
				return;
			}
			for (int j = 0; j < MItems.Count; j++)
			{
				if (CurrentModelName == MItems[j].ModelFullName)
				{
					lv_ModelList.SelectedIndex = j;
					ModelSetuped = true;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			string text3 = "LocateModelsWindows-UpdateModels-更新模型檔案錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetModelUsed(string onnxfile)
	{
		string text = "";
		for (int i = 0; i < 20; i++)
		{
			if (Station.paramFile.MatchToolON && Station.paramFile.MatchParam[i].OnnxName == onnxfile)
			{
				text += string.Format("{0}, ", i + 1);
			}
		}
		return text;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ModelSearch_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			SearchItems.Clear();
			lv_ModelList.ItemsSource = null;
			string text = txt_SearchBox.Text.ToLower();
			if (text == "")
			{
				lv_ModelList.ItemsSource = MItems;
				SearchMode = false;
				return;
			}
			for (int i = 0; i < MItems.Count; i++)
			{
				if (MItems[i].ModelName.ToLower().Contains(text))
				{
					SearchItems.Add(MItems[i]);
				}
			}
			lv_ModelList.ItemsSource = SearchItems;
			SearchMode = true;
		}
		catch (Exception ex)
		{
			string text2 = "LocateModelsWindows-Btn_ModelSearch_Click-搜尋模型檔案錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, "錯誤");
		}
	}

	private void Lv_ParetoList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		int selectedIndex = lv_ModelList.SelectedIndex;
		if (selectedIndex < 0)
		{
			return;
		}
		if (!SearchMode)
		{
			if (selectedIndex < MItems.Count)
			{
				SelectModelName = MItems[selectedIndex].ModelFullName;
			}
		}
		else if (selectedIndex < SearchItems.Count)
		{
			SelectModelName = SearchItems[selectedIndex].ModelFullName;
		}
		Close();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RemoveModel_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = lv_ModelList.SelectedIndex;
			if (selectedIndex < 0)
			{
				MessageBox.Show("尚未選擇刪除模型!", "警告");
				return;
			}
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Delete Model?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("是否删除模型档案?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("是否刪除模型檔案?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
			if (!SearchMode)
			{
				string text = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "Model", MItems[selectedIndex].ModelFullName);
				if (File.Exists(text))
				{
					FileInfo fileInfo = new FileInfo(text);
					if (fileInfo.IsReadOnly)
					{
						fileInfo.IsReadOnly = false;
					}
					File.Delete(text);
				}
				text = string.Format("{0}{1}\\{2}.lpbd", AppDomain.CurrentDomain.BaseDirectory, "Model", MItems[selectedIndex].ModelName);
				if (File.Exists(text))
				{
					FileInfo fileInfo2 = new FileInfo(text);
					if (fileInfo2.IsReadOnly)
					{
						fileInfo2.IsReadOnly = false;
					}
					File.Delete(text);
				}
				MItems.RemoveAt(selectedIndex);
				lv_ModelList.ItemsSource = null;
				lv_ModelList.ItemsSource = MItems;
				return;
			}
			string text2 = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "Model", SearchItems[selectedIndex].ModelFullName);
			if (File.Exists(text2))
			{
				FileInfo fileInfo3 = new FileInfo(text2);
				if (fileInfo3.IsReadOnly)
				{
					fileInfo3.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			text2 = string.Format("{0}{1}\\{2}.lpbd", AppDomain.CurrentDomain.BaseDirectory, "Model", SearchItems[selectedIndex].ModelName);
			if (File.Exists(text2))
			{
				FileInfo fileInfo4 = new FileInfo(text2);
				if (fileInfo4.IsReadOnly)
				{
					fileInfo4.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			SearchItems.RemoveAt(selectedIndex);
			lv_ModelList.ItemsSource = null;
			lv_ModelList.ItemsSource = SearchItems;
		}
		catch (Exception ex)
		{
			string text3 = "LocateModelsWindows-Btn_RemoveModel_Click-刪除模型錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_LoadModel_Click(object sender, RoutedEventArgs e)
	{
		if (MItems.Count <= 0)
		{
			Close();
		}
		if (SearchMode && SearchItems.Count <= 0)
		{
			Close();
		}
		int selectedIndex = lv_ModelList.SelectedIndex;
		if (selectedIndex < 0)
		{
			string text = "";
			if (ModelSetuped)
			{
				for (int i = 0; i < MItems.Count; i++)
				{
					if (txt_CurrentModel.Text == MItems[i].ModelName)
					{
						text = MItems[i].ModelFullName;
						SelectModelName = text;
						Close();
						return;
					}
				}
			}
			MessageBox.Show("尚未選擇載入模型!", "警告");
			return;
		}
		if (!SearchMode)
		{
			if (selectedIndex < MItems.Count)
			{
				SelectModelName = MItems[selectedIndex].ModelFullName;
			}
		}
		else if (selectedIndex < SearchItems.Count)
		{
			SelectModelName = SearchItems[selectedIndex].ModelFullName;
		}
		Close();
	}

	private void Btn_ModelHome_Click(object sender, RoutedEventArgs e)
	{
		lv_ModelList.ItemsSource = MItems;
		SearchMode = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Txt_SearchBox_KeyUp(object sender, KeyEventArgs e)
	{
		try
		{
			SearchItems.Clear();
			lv_ModelList.ItemsSource = null;
			string text = txt_SearchBox.Text.ToLower();
			if (text == "")
			{
				lv_ModelList.ItemsSource = MItems;
				SearchMode = false;
				return;
			}
			for (int i = 0; i < MItems.Count; i++)
			{
				if (MItems[i].ModelName.ToLower().Contains(text))
				{
					SearchItems.Add(MItems[i]);
				}
			}
			lv_ModelList.ItemsSource = SearchItems;
			SearchMode = true;
		}
		catch (Exception ex)
		{
			string text2 = "LocateModelsWindows-Txt_SearchBox_KeyUp-搜尋模型錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text2);
			MessageBox.Show(text2, "錯誤");
		}
	}

	private void Lv_ModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		int selectedIndex = lv_ModelList.SelectedIndex;
		if (selectedIndex < 0)
		{
			return;
		}
		if (SearchMode)
		{
			if (selectedIndex < SearchItems.Count)
			{
				txt_CurrentModel.Text = SearchItems[selectedIndex].ModelName;
				txt_CurrentModel.Background = Brushes.LightGreen;
				ModelSetuped = true;
			}
		}
		else if (selectedIndex < MItems.Count)
		{
			txt_CurrentModel.Text = MItems[selectedIndex].ModelName;
			txt_CurrentModel.Background = Brushes.LightGreen;
			ModelSetuped = true;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (!ModelSetuped)
		{
			MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Model setup couldn't Finish, exit?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("是尚未選擇模型，是否強制離開?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("尚未選擇模型，是否強制離開?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				e.Cancel = true;
			}
			return;
		}
		string text = "";
		for (int i = 0; i < MItems.Count; i++)
		{
			if (txt_CurrentModel.Text == MItems[i].ModelName)
			{
				text = MItems[i].ModelFullName;
				SelectModelName = text;
				break;
			}
		}
	}

	private void Txt_CurrentModel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Btn_LoadModel_Click(sender, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void btn_CreateModel_Click(object sender, RoutedEventArgs e)
	{
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.SetTitle(LanguageManager.Instance["InputNewModelName"]);
		itemInputWindow.ShowDialog();
		if (itemInputWindow.InputStr == "")
		{
			MessageBox.Show(LanguageManager.Instance["ModelNameEmptyWarning"], LanguageManager.Instance["Warning"]);
			return;
		}
		if (itemInputWindow.InputStr.Contains("\\") || itemInputWindow.InputStr.Contains("/") || itemInputWindow.InputStr.Contains(":") || itemInputWindow.InputStr.Contains("\"") || itemInputWindow.InputStr.Contains("<") || itemInputWindow.InputStr.Contains(">") || itemInputWindow.InputStr.Contains("|") || itemInputWindow.InputStr.Contains("?") || itemInputWindow.InputStr.Contains("*") || itemInputWindow.InputStr.Contains("."))
		{
			MessageBox.Show(LanguageManager.Instance["IllegalModelNameChar"] + "\\ / : * \" < > | ? .", LanguageManager.Instance["Warning"]);
			return;
		}
		if (itemInputWindow.InputStr.ToUpper() == "X" || itemInputWindow.InputStr.ToUpper() == "Y" || itemInputWindow.InputStr.ToUpper() == "OK" || itemInputWindow.InputStr.ToUpper() == "NG")
		{
			MessageBox.Show(LanguageManager.Instance["IllegalModelName"] + " X, Y, OK, NG", LanguageManager.Instance["Warning"]);
			return;
		}
		try
		{
			string arg = itemInputWindow.InputStr.TrimEnd();
			Directory.CreateDirectory(string.Format("{0}TrainingImage\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg));
			Directory.CreateDirectory(string.Format("{0}TrainingImage\\{1}\\X\\OK\\", AppDomain.CurrentDomain.BaseDirectory, arg));
			Directory.CreateDirectory(string.Format("{0}TrainingImage\\{1}\\X\\NG\\", AppDomain.CurrentDomain.BaseDirectory, arg));
			Directory.CreateDirectory(string.Format("{0}TrainingImage\\{1}\\Y\\OK\\", AppDomain.CurrentDomain.BaseDirectory, arg));
			Directory.CreateDirectory(string.Format("{0}TrainingImage\\{1}\\Y\\NG\\", AppDomain.CurrentDomain.BaseDirectory, arg));
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ModelWindow-Btn_CreateModel_Click()" + ex.ToString());
			MessageBox.Show(string.Format("建立模型資料夾錯誤，請檢查您輸入檔名是否合法。(輸入檔名:{0})", itemInputWindow.InputStr), "Warning");
			return;
		}
		string inputStr = itemInputWindow.InputStr;
		string text = "";
		text = string.Format("{0}Model\\{1}.lh5d", AppDomain.CurrentDomain.BaseDirectory, inputStr);
		if (File.Exists(text))
		{
			MessageBox.Show(string.Format("{0}{1}{2}，{3}。", LanguageManager.Instance["ModelName"], inputStr, LanguageManager.Instance["Exist"], LanguageManager.Instance["UseOtherName"]), LanguageManager.Instance["Warning"]);
		}
		else
		{
			using (StreamWriter streamWriter = new StreamWriter(text, append: true))
			{
				streamWriter.Write("{0}", "");
				streamWriter.WriteLine();
			}
			UpdateModels();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RenameModel_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int selectedIndex = lv_ModelList.SelectedIndex;
			if (selectedIndex < 0)
			{
				MessageBox.Show(LanguageManager.Instance["SelectModelFirst"], LanguageManager.Instance["Warning"]);
				return;
			}
			string text = "";
			if (!SearchMode)
			{
				if (selectedIndex < MItems.Count)
				{
					SelectModelName = MItems[selectedIndex].ModelFullName;
					text = MItems[selectedIndex].ModelName;
				}
			}
			else if (selectedIndex < SearchItems.Count)
			{
				SelectModelName = SearchItems[selectedIndex].ModelFullName;
				text = SearchItems[selectedIndex].ModelName;
			}
			if (text == "" || MessageBox.Show(LanguageManager.Instance["isChangeModelName"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
			{
				return;
			}
			ItemInputWindow itemInputWindow = new ItemInputWindow();
			itemInputWindow.SetTitle(LanguageManager.Instance["InputCopyModelName"]);
			itemInputWindow.SetText(text + "-2");
			itemInputWindow.ShowDialog();
			string text2 = itemInputWindow.InputStr.TrimEnd();
			if (itemInputWindow.InputStr == "")
			{
				MessageBox.Show(LanguageManager.Instance["ModelNameEmptyWarning"], LanguageManager.Instance["Warning"]);
				return;
			}
			if (itemInputWindow.InputStr.Contains("\\") || itemInputWindow.InputStr.Contains("/") || itemInputWindow.InputStr.Contains(":") || itemInputWindow.InputStr.Contains("\"") || itemInputWindow.InputStr.Contains("<") || itemInputWindow.InputStr.Contains(">") || itemInputWindow.InputStr.Contains("|") || itemInputWindow.InputStr.Contains("?") || itemInputWindow.InputStr.Contains("*") || itemInputWindow.InputStr.Contains("."))
			{
				MessageBox.Show(LanguageManager.Instance["IllegalModelNameChar"] + "\\ / : * \" < > | ? .", LanguageManager.Instance["Warning"]);
				return;
			}
			if (itemInputWindow.InputStr.ToUpper() == "X" || itemInputWindow.InputStr.ToUpper() == "Y" || itemInputWindow.InputStr.ToUpper() == "OK" || itemInputWindow.InputStr.ToUpper() == "NG")
			{
				MessageBox.Show(LanguageManager.Instance["IllegalModelName"] + " X, Y, OK, NG", LanguageManager.Instance["Warning"]);
				return;
			}
			if (text2 == text)
			{
				MessageBox.Show(LanguageManager.Instance["SameModelNameWaring"], LanguageManager.Instance["Warning"]);
				return;
			}
			string text3 = "";
			string text4 = "";
			string text5 = "";
			string text6 = "";
			text3 = string.Format("{0}Model\\{1}.lh5d", AppDomain.CurrentDomain.BaseDirectory, text2);
			text4 = string.Format("{0}Model\\{1}.lh5d", AppDomain.CurrentDomain.BaseDirectory, text);
			text5 = text3.Replace(".lh5d", ".lpbd");
			text6 = text4.Replace(".lh5d", ".lpbd");
			if (File.Exists(text3))
			{
				MessageBox.Show(string.Format("{0}{1}{2}，{3}。", LanguageManager.Instance["ModelName"], text2, LanguageManager.Instance["Exist"], LanguageManager.Instance["UseOtherName"]), LanguageManager.Instance["Warning"]);
				return;
			}
			if (File.Exists(text4))
			{
				File.Move(text4, text3);
				if (File.Exists(text5))
				{
					File.Delete(text5);
				}
			}
			if (File.Exists(text6))
			{
				File.Move(text6, text5);
			}
			string text7 = string.Format("{0}TrainingImage\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string text8 = string.Format("{0}TrainingImage\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text2);
			if (Directory.Exists(text7))
			{
				if (Directory.Exists(text8))
				{
					Directory.Delete(text8, recursive: true);
				}
				Directory.Move(text7, text8);
			}
			else if (!Directory.Exists(text8))
			{
				Directory.CreateDirectory(text8);
			}
			if (txt_CurrentModel.Text == Path.GetFileNameWithoutExtension(text4))
			{
				txt_CurrentModel.Text = Path.GetFileNameWithoutExtension(text3);
			}
			UpdateModels();
		}
		catch (Exception ex)
		{
			string text9 = "LocateModelsWindows-Btn_RemoveModel_Click-刪除模型錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text9);
			MessageBox.Show(text9, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_ReSetModel_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = lv_ModelList.SelectedIndex;
		if (selectedIndex < 0)
		{
			MessageBox.Show(LanguageManager.Instance["SelectDelModelFirst"], LanguageManager.Instance["AutorunFinish"]);
			return;
		}
		string text = "";
		if (!SearchMode)
		{
			if (selectedIndex < MItems.Count)
			{
				SelectModelName = MItems[selectedIndex].ModelFullName;
				text = MItems[selectedIndex].ModelName;
			}
		}
		else if (selectedIndex < SearchItems.Count)
		{
			SelectModelName = SearchItems[selectedIndex].ModelFullName;
			text = SearchItems[selectedIndex].ModelName;
		}
		if (!(text == "") && MessageBox.Show(LanguageManager.Instance["ResetModelWaring"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
		{
			string text2 = "";
			string text3 = "";
			text2 = string.Format("{0}Model\\{1}.lh5d", AppDomain.CurrentDomain.BaseDirectory, text);
			text3 = text2.Replace(".lh5d", ".lpbd");
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			using (StreamWriter streamWriter = new StreamWriter(text2, append: true))
			{
				streamWriter.Write("{0}", "");
				streamWriter.WriteLine();
			}
			UpdateModels();
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
			Uri resourceLocator = new Uri("/InspTools;component/other/locatemodelswindow.xaml", UriKind.Relative);
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
			((LocateModelsWindow)target).Closing += Window_Closing;
			break;
		case 2:
			lv_ModelList = (ListView)target;
			lv_ModelList.MouseDoubleClick += Lv_ParetoList_MouseDoubleClick;
			lv_ModelList.SelectionChanged += Lv_ModelList_SelectionChanged;
			break;
		case 3:
			txt_SearchBox = (TextBox)target;
			txt_SearchBox.KeyUp += Txt_SearchBox_KeyUp;
			break;
		case 4:
			btn_ModelSearch = (Button)target;
			btn_ModelSearch.Click += Btn_ModelSearch_Click;
			break;
		case 5:
			btn_CreateModel = (Button)target;
			btn_CreateModel.Click += btn_CreateModel_Click;
			break;
		case 6:
			btn_RemoveModel = (Button)target;
			btn_RemoveModel.Click += Btn_RemoveModel_Click;
			break;
		case 7:
			btn_LoadModel = (Button)target;
			btn_LoadModel.Click += Btn_LoadModel_Click;
			break;
		case 8:
			btn_ModelHome = (Button)target;
			btn_ModelHome.Click += Btn_ModelHome_Click;
			break;
		case 9:
			txt_CurrentModel = (TextBox)target;
			txt_CurrentModel.KeyUp += Txt_SearchBox_KeyUp;
			txt_CurrentModel.PreviewMouseLeftButtonDown += Txt_CurrentModel_PreviewMouseLeftButtonDown;
			break;
		case 10:
			btn_RenameModel = (Button)target;
			btn_RenameModel.Click += Btn_RenameModel_Click;
			break;
		case 11:
			btn_ReSetModel = (Button)target;
			btn_ReSetModel.Click += Btn_ReSetModel_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
