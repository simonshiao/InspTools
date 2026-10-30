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
using Microsoft.Win32;

namespace InspTools.Other;

public class OnnxModelsWindow : Window, IComponentConnector
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

	internal Button btn_AddModel;

	internal Button btn_RemoveModel;

	internal Button btn_LoadModel;

	internal Button btn_ModelHome;

	internal TextBox txt_CurrentModel;

	private bool _contentLoaded;

	public OnnxModelsWindow()
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
			string[] files = Directory.GetFiles(string.Format("{0}{1}\\", AppDomain.CurrentDomain.BaseDirectory, "PublicDB"), "*.*", SearchOption.TopDirectoryOnly);
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
						ModelUsed = GetModelUsed(Path.GetFileName(text)),
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
			string text3 = "OnnxModelsWindows-UpdateModels-更新模型檔案錯誤!" + ex.ToString();
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
			string text2 = "OnnxModelsWindows-Btn_ModelSearch_Click-搜尋模型檔案錯誤!" + ex.ToString();
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
	private void Btn_AddModel_Click(object sender, RoutedEventArgs e)
	{
		_ = (Button)sender;
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "AI Model files (*.onnx)|*.onnx|All files (*.*)|*.*";
		string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB\\");
		try
		{
			openFileDialog.InitialDirectory = text;
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}
			string fileName = Path.GetFileName(openFileDialog.FileName);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}{1}", text, fileName);
			if (File.Exists(text2) && text2 != openFileDialog.FileName)
			{
				MessageBoxResult messageBoxResult = ((LanguageType == 0) ? MessageBox.Show("Replace Exist Model?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : ((LanguageType != 1) ? MessageBox.Show("模型档名重复，是否覆盖模型档案?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) : MessageBox.Show("模型檔名重複，是否覆蓋模型檔案?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes)));
				if (messageBoxResult != MessageBoxResult.Yes)
				{
					return;
				}
				FileInfo fileInfo = new FileInfo(text2);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				File.Delete(text2);
				string text3 = "";
				text3 = string.Format("{0}", text, fileName);
				if (File.Exists(text3))
				{
					FileInfo fileInfo2 = new FileInfo(text3);
					if (fileInfo2.IsReadOnly)
					{
						fileInfo2.IsReadOnly = false;
					}
					File.Delete(text3);
				}
				for (int i = 0; i < 20; i++)
				{
					if (Station.paramFile.MatchParam[i].OnnxName == fileName)
					{
						Station.matchTool[i].ModelAReaded = false;
					}
				}
			}
			if (text2 != openFileDialog.FileName)
			{
				if (File.Exists(text2) && File.Exists(text2))
				{
					FileInfo fileInfo3 = new FileInfo(text2);
					if (fileInfo3.IsReadOnly)
					{
						fileInfo3.IsReadOnly = false;
					}
					File.Delete(text2);
				}
				File.Copy(openFileDialog.FileName, text2, overwrite: true);
			}
			UpdateModels();
		}
		catch (Exception ex)
		{
			string text4 = "OnnxModelsWindows-Btn_AddModel_Click-覆蓋模型檔案錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text4);
			MessageBox.Show(text4, "錯誤");
		}
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
				string text = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB", MItems[selectedIndex].ModelFullName);
				if (File.Exists(text))
				{
					FileInfo fileInfo = new FileInfo(text);
					if (fileInfo.IsReadOnly)
					{
						fileInfo.IsReadOnly = false;
					}
					File.Delete(text);
				}
				text = string.Format("{0}{1}\\{2}.onnx", AppDomain.CurrentDomain.BaseDirectory, "PublicDB", MItems[selectedIndex].ModelName);
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
			string text2 = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "PublicDB", SearchItems[selectedIndex].ModelFullName);
			if (File.Exists(text2))
			{
				FileInfo fileInfo3 = new FileInfo(text2);
				if (fileInfo3.IsReadOnly)
				{
					fileInfo3.IsReadOnly = false;
				}
				File.Delete(text2);
			}
			text2 = string.Format("{0}{1}\\{2}.onnx", AppDomain.CurrentDomain.BaseDirectory, "PublicDB", SearchItems[selectedIndex].ModelName);
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
			string text3 = "OnnxModelsWindows-Btn_RemoveModel_Click-刪除模型錯誤!" + ex.ToString();
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
			string text2 = "OnnxModelsWindows-Txt_SearchBox_KeyUp-搜尋模型錯誤!" + ex.ToString();
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
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/onnxmodelswindow.xaml", UriKind.Relative);
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
			((OnnxModelsWindow)target).Closing += Window_Closing;
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
			btn_AddModel = (Button)target;
			btn_AddModel.Click += Btn_AddModel_Click;
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
		default:
			_contentLoaded = true;
			break;
		}
	}
}
