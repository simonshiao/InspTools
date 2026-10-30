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

public class ProphetOnnxModelsWindow : Window, IComponentConnector
{
	public struct ModelItem
	{
		public string ModelName { get; set; }

		public string ModelUsed { get; set; }

		public string ModelTimes { get; set; }

		public string ModelFullName { get; set; }
	}

	private int LanguageType = 1;

	private CVInspection[] Station;

	private List<ModelItem> MItems = new List<ModelItem>();

	private List<ModelItem> SearchItems = new List<ModelItem>();

	private string SelectModelName = "";

	private string CurrentModelName = "";

	private bool SearchMode;

	private bool ModelSetuped;

	internal ListView lv_ModelList;

	internal TextBox txt_SearchBox;

	internal Button btn_ModelSearch;

	internal Button btn_ModelHome;

	internal Button btn_RemoveModel_Copy;

	internal Button btn_CreateModel;

	internal Button btn_RenameModel;

	internal Button btn_ReSetModel;

	internal Button btn_LoadModel;

	internal TextBox txt_CurrentModel;

	private bool _contentLoaded;

	public ProphetOnnxModelsWindow()
	{
		InitializeComponent();
	}

	public void SetInspection(CVInspection[] station)
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
		return Path.GetFileNameWithoutExtension(SelectModelName);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void UpdateModels()
	{
		try
		{
			lv_ModelList.ItemsSource = null;
			string[] files = Directory.GetFiles(string.Format("{0}{1}\\", AppDomain.CurrentDomain.BaseDirectory, "PModel"), "*.*", SearchOption.TopDirectoryOnly);
			MItems.Clear();
			string[] array = files;
			foreach (string text in array)
			{
				string extension = Path.GetExtension(text);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				string text2 = ".onnx";
				if (extension.ToLower() == text2)
				{
					ModelItem item = new ModelItem
					{
						ModelName = fileNameWithoutExtension,
						ModelUsed = GetModelUsed(fileNameWithoutExtension),
						ModelFullName = Path.GetFileName(text)
					};
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
			string text3 = "ProphetOnnxModelsWindows-UpdateModels-更新模型檔案錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetModelUsed(string onnxfile)
	{
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIProphetToolON && Station[i].paramFile.AIProphetParam.ModelName == onnxfile)
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
			string text2 = "ProphetOnnxModelsWindows-Btn_ModelSearch_Click-搜尋模型檔案錯誤!" + ex.ToString();
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
			string text = "";
			if (!SearchMode)
			{
				string text2 = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "PModel", MItems[selectedIndex].ModelFullName);
				text = MItems[selectedIndex].ModelName;
				if (File.Exists(text2))
				{
					FileInfo fileInfo = new FileInfo(text2);
					if (fileInfo.IsReadOnly)
					{
						fileInfo.IsReadOnly = false;
					}
					File.Delete(text2);
				}
				text2 = string.Format("{0}{1}\\{2}.onnx", AppDomain.CurrentDomain.BaseDirectory, "PModel", MItems[selectedIndex].ModelName);
				if (File.Exists(text2))
				{
					FileInfo fileInfo2 = new FileInfo(text2);
					if (fileInfo2.IsReadOnly)
					{
						fileInfo2.IsReadOnly = false;
					}
					File.Delete(text2);
				}
				MItems.RemoveAt(selectedIndex);
				lv_ModelList.ItemsSource = null;
				lv_ModelList.ItemsSource = MItems;
			}
			else
			{
				string text3 = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "PModel", SearchItems[selectedIndex].ModelFullName);
				text = MItems[selectedIndex].ModelName;
				if (File.Exists(text3))
				{
					FileInfo fileInfo3 = new FileInfo(text3);
					if (fileInfo3.IsReadOnly)
					{
						fileInfo3.IsReadOnly = false;
					}
					File.Delete(text3);
				}
				text3 = string.Format("{0}{1}\\{2}.onnx", AppDomain.CurrentDomain.BaseDirectory, "PModel", SearchItems[selectedIndex].ModelName);
				if (File.Exists(text3))
				{
					FileInfo fileInfo4 = new FileInfo(text3);
					if (fileInfo4.IsReadOnly)
					{
						fileInfo4.IsReadOnly = false;
					}
					File.Delete(text3);
				}
				SearchItems.RemoveAt(selectedIndex);
				lv_ModelList.ItemsSource = null;
				lv_ModelList.ItemsSource = SearchItems;
			}
			string path = string.Format("{0}PModel\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text);
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		catch (Exception ex)
		{
			string text4 = "ProphetOnnxModelsWindows-Btn_RemoveModel_Click-刪除模型錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text4);
			MessageBox.Show(text4, "錯誤");
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
				SelectModelName = MItems[selectedIndex].ModelName;
			}
		}
		else if (selectedIndex < SearchItems.Count)
		{
			SelectModelName = SearchItems[selectedIndex].ModelName;
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
			string text2 = "ProphetOnnxModelsWindows-Txt_SearchBox_KeyUp-搜尋模型錯誤!" + ex.ToString();
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
	private void Btn_CreateModel_Click(object sender, RoutedEventArgs e)
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
			Directory.CreateDirectory(string.Format("{0}PModel\\{1}", AppDomain.CurrentDomain.BaseDirectory, arg));
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("ModelWindow-Btn_CreateModel_Click()" + ex.ToString());
			MessageBox.Show(string.Format("建立模型資料夾錯誤，請檢查您輸入檔名是否合法。(輸入檔名:{0})", itemInputWindow.InputStr), "Warning");
			return;
		}
		string inputStr = itemInputWindow.InputStr;
		string path = string.Format("{0}PModel\\{1}.onnx", AppDomain.CurrentDomain.BaseDirectory, inputStr);
		if (File.Exists(path))
		{
			MessageBox.Show(string.Format("{0}{1}{2}，{3}。", LanguageManager.Instance["ModelName"], inputStr, LanguageManager.Instance["Exist"], LanguageManager.Instance["UseOtherName"]), LanguageManager.Instance["Warning"]);
		}
		else
		{
			using (StreamWriter streamWriter = new StreamWriter(path, append: true))
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
			text3 = string.Format("{0}PModel\\{1}.onnx", AppDomain.CurrentDomain.BaseDirectory, text2);
			text4 = string.Format("{0}PModel\\{1}.onnx", AppDomain.CurrentDomain.BaseDirectory, text);
			text5 = text3.Replace(".onnx", ".pt");
			text6 = text4.Replace(".onnx", ".pt");
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
			string text7 = string.Format("{0}PModel\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text);
			string text8 = string.Format("{0}PModel\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text2);
			if (Directory.Exists(text7))
			{
				if (Directory.Exists(text8))
				{
					Directory.Delete(text8, recursive: true);
				}
				DirectoryCopy(text7, text8);
				if (Directory.Exists(text7))
				{
					Directory.Delete(text7, recursive: true);
				}
			}
			else if (!Directory.Exists(text8))
			{
				Directory.CreateDirectory(text8);
			}
			for (int i = 0; i < 8; i++)
			{
				if (Station[i].paramFile.AIProphetParam.ModelName == Path.GetFileName(text4))
				{
					Station[i].paramFile.AIProphetParam.ModelName = Path.GetFileName(text3);
				}
			}
			if (txt_CurrentModel.Text == Path.GetFileNameWithoutExtension(text4))
			{
				txt_CurrentModel.Text = Path.GetFileNameWithoutExtension(text3);
			}
			UpdateModels();
		}
		catch (Exception ex)
		{
			string text9 = "ModelsWindows-Btn_RemoveModel_Click-刪除模型錯誤!" + ex.ToString();
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
			text2 = string.Format("{0}PModel\\{1}.onnx", AppDomain.CurrentDomain.BaseDirectory, text);
			text3 = text2.Replace(".onnx", ".pt");
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

	private void DirectoryCopy(string sourceDir, string targetDir)
	{
		if (!Directory.Exists(sourceDir))
		{
			return;
		}
		if (!Directory.Exists(targetDir))
		{
			Directory.CreateDirectory(targetDir);
			if (!Directory.Exists(targetDir))
			{
				return;
			}
		}
		string text = sourceDir.Replace(Directory.GetParent(sourceDir).ToString(), "").Replace(Path.DirectorySeparatorChar.ToString(), "");
		if (!(sourceDir == targetDir + text))
		{
			string text2 = targetDir + Path.DirectorySeparatorChar + text;
			string[] files = Directory.GetFiles(sourceDir);
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i], text2 + Path.DirectorySeparatorChar + Path.GetFileName(files[i]), overwrite: true);
			}
			string[] directories = Directory.GetDirectories(sourceDir);
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryCopy(directories[j], text2);
			}
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
			Uri resourceLocator = new Uri("/InspTools;component/other/prophetonnxmodelswindow.xaml", UriKind.Relative);
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
			((ProphetOnnxModelsWindow)target).Closing += Window_Closing;
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
			btn_ModelHome = (Button)target;
			btn_ModelHome.Click += Btn_ModelHome_Click;
			break;
		case 6:
			btn_RemoveModel_Copy = (Button)target;
			btn_RemoveModel_Copy.Click += Btn_RemoveModel_Click;
			break;
		case 7:
			btn_CreateModel = (Button)target;
			btn_CreateModel.Click += Btn_CreateModel_Click;
			break;
		case 8:
			btn_RenameModel = (Button)target;
			btn_RenameModel.Click += Btn_RenameModel_Click;
			break;
		case 9:
			btn_ReSetModel = (Button)target;
			btn_ReSetModel.Click += Btn_ReSetModel_Click;
			break;
		case 10:
			btn_LoadModel = (Button)target;
			btn_LoadModel.Click += Btn_LoadModel_Click;
			break;
		case 11:
			txt_CurrentModel = (TextBox)target;
			txt_CurrentModel.KeyUp += Txt_SearchBox_KeyUp;
			txt_CurrentModel.PreviewMouseLeftButtonDown += Txt_CurrentModel_PreviewMouseLeftButtonDown;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
