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

public class ModelsWindow : Window, IComponentConnector
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

	private bool PredictMode;

	public bool EditHashTag;

	private ThroughputParamFile ThroughputFile;

	private Button[] HashTagBtn;

	internal ListView lv_ModelList;

	internal TextBox txt_SearchBox;

	internal Button btn_ModelSearch;

	internal Button btn_AddModel;

	internal Button btn_RemoveModel;

	internal Button btn_ModelHome;

	internal TextBox txt_CurrentModel;

	internal Button btn_CreateModel;

	internal Button btn_LoadModel;

	internal Button btn_CloneModel;

	internal Button btn_RenameModel;

	internal Button btn_ReSetModel;

	internal Button btn_HashTag1;

	internal Button btn_HashTag2;

	internal Button btn_HashTag3;

	internal Button btn_HashTag4;

	internal Button btn_HashTag5;

	internal Button btn_HashTag6;

	internal Button btn_HashTag7;

	internal Button btn_HashTagSetup;

	private bool _contentLoaded;

	public ModelsWindow()
	{
		InitializeComponent();
		HashTagBtn = new Button[7] { btn_HashTag1, btn_HashTag2, btn_HashTag3, btn_HashTag4, btn_HashTag5, btn_HashTag6, btn_HashTag7 };
	}

	public void SetInspection(CVInspection[] station, bool predictMode)
	{
		Station = station;
		PredictMode = predictMode;
		UpdateModels();
	}

	public void SetThroghputFile(ThroughputParamFile file)
	{
		ThroughputFile = file;
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
				string text2 = ((!PredictMode) ? ".h5d" : ".h5");
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
			if (CurrentModelName != "")
			{
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
			for (int k = 0; k < 7; k++)
			{
				HashTagBtn[k].Content = "#" + ThroughputFile.param.HashTagStr[k];
			}
		}
		catch (Exception ex)
		{
			string text3 = "ModelsWindows-UpdateModels-更新模型檔案錯誤!" + ex.ToString();
			CVBuffer.SaveLog(text3);
			MessageBox.Show(text3, "錯誤");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private string GetModelUsed(string h5file)
	{
		string text = "";
		for (int i = 0; i < 8; i++)
		{
			if (Station[i].paramFile.AIEnhanceToolON)
			{
				if (Station[i].paramFile.AIEnhanceParam.EnableSelectFace)
				{
					if (Station[i].paramFile.AIEnhanceParam.ModelName_A == h5file)
					{
						text += string.Format("{0}A, ", i + 1);
					}
					if (Station[i].paramFile.AIEnhanceParam.ModelName_A == h5file)
					{
						text += string.Format("{0}B, ", i + 1);
					}
				}
				else if (Station[i].paramFile.AIEnhanceParam.ModelName_A == h5file)
				{
					text += string.Format("{0}, ", i + 1);
				}
			}
			if (!Station[i].paramFile.AIPredictToolON)
			{
				continue;
			}
			if (Station[i].paramFile.AIPredictParam.EnableSelectFace)
			{
				if (Station[i].paramFile.AIPredictParam.ModelName_A == h5file)
				{
					text += string.Format("{0}A, ", i + 1);
				}
				if (Station[i].paramFile.AIPredictParam.ModelName_A == h5file)
				{
					text += string.Format("{0}B, ", i + 1);
				}
			}
			else if (Station[i].paramFile.AIPredictParam.ModelName_A == h5file)
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
			string text2 = "ModelsWindows-Btn_ModelSearch_Click-搜尋模型檔案錯誤!" + ex.ToString();
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
		if (PredictMode)
		{
			openFileDialog.Filter = "AI Model files (*.h5)|*.h5|All files (*.*)|*.*";
		}
		else
		{
			openFileDialog.Filter = "AI Model files (*.h5d)|*.h5d|All files (*.*)|*.*";
		}
		string text = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "Model\\");
		try
		{
			openFileDialog.InitialDirectory = text;
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}
			string fileName = Path.GetFileName(openFileDialog.FileName);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0}{1}", text, fileName);
			if (File.Exists(text2) && text2 != openFileDialog.FileName)
			{
				if (MessageBox.Show(LanguageManager.Instance["AutorunFinish"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
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
				text3 = ((!PredictMode) ? string.Format("{0}{1}.pbd", text, fileNameWithoutExtension) : string.Format("{0}{1}.pb", text, fileNameWithoutExtension));
				if (File.Exists(text3))
				{
					FileInfo fileInfo2 = new FileInfo(text3);
					if (fileInfo2.IsReadOnly)
					{
						fileInfo2.IsReadOnly = false;
					}
					File.Delete(text3);
				}
				for (int i = 0; i < 8; i++)
				{
					if (PredictMode && Station[i].paramFile.AIPredictToolON && Station[i].paramFile.AIPredictParam.ModelName_A.Length > 4)
					{
						if (Station[i].paramFile.AIPredictParam.ModelName_A == fileNameWithoutExtension)
						{
							Station[i].paramFile.AIPredictParam.Model_A_Changed = true;
						}
					}
					else if (!PredictMode && Station[i].paramFile.AIEnhanceToolON && Station[i].paramFile.AIEnhanceParam.ModelName_A.Length > 4 && Station[i].paramFile.AIEnhanceParam.ModelName_A.Substring(0, Station[i].paramFile.AIEnhanceParam.ModelName_A.Length - 4) == fileNameWithoutExtension)
					{
						Station[i].paramFile.AIEnhanceParam.Model_A_Changed = true;
					}
				}
			}
			if (text2 != openFileDialog.FileName)
			{
				File.Copy(openFileDialog.FileName, text2, overwrite: true);
			}
			UpdateModels();
		}
		catch (Exception ex)
		{
			string text4 = "ModelsWindows-Btn_AddModel_Click-覆蓋模型檔案錯誤!" + ex.ToString();
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
				MessageBox.Show(LanguageManager.Instance["SelectDelModelFirst"], LanguageManager.Instance["Warning"]);
			}
			else
			{
				if (MessageBox.Show(LanguageManager.Instance["DeleteModelWarning"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
				{
					return;
				}
				string modelName = MItems[selectedIndex].ModelName;
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
					text = ((!PredictMode) ? string.Format("{0}{1}\\{2}.pbd", AppDomain.CurrentDomain.BaseDirectory, "Model", MItems[selectedIndex].ModelName) : string.Format("{0}{1}\\{2}.pb", AppDomain.CurrentDomain.BaseDirectory, "Model", MItems[selectedIndex].ModelName));
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
				}
				else
				{
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
					text2 = ((!PredictMode) ? string.Format("{0}{1}\\{2}.pbd", AppDomain.CurrentDomain.BaseDirectory, "Model", SearchItems[selectedIndex].ModelName) : string.Format("{0}{1}\\{2}.pb", AppDomain.CurrentDomain.BaseDirectory, "Model", SearchItems[selectedIndex].ModelName));
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
				SelectModelName = "";
				txt_CurrentModel.Text = LanguageManager.Instance["SelectLoadModel"];
				if (MessageBox.Show(LanguageManager.Instance["DeleteModelImageWarning"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) == MessageBoxResult.Yes)
				{
					string path = string.Format("{0}TrainingImage\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, modelName);
					if (Directory.Exists(path))
					{
						Directory.Delete(path, recursive: true);
					}
				}
			}
		}
		catch (Exception ex)
		{
			string text3 = "ModelsWindows-Btn_RemoveModel_Click-刪除模型錯誤!" + ex.ToString();
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
			MessageBox.Show(LanguageManager.Instance["LoadModelFirst"], LanguageManager.Instance["Warning"]);
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
			string text2 = "ModelsWindows-Txt_SearchBox_KeyUp-搜尋模型錯誤!" + ex.ToString();
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
			if (MessageBox.Show(LanguageManager.Instance["ExistModelWarring"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes) != MessageBoxResult.Yes)
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
		text = ((!PredictMode) ? string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, inputStr) : string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, inputStr));
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
	private void Btn_CloneModel_Click(object sender, RoutedEventArgs e)
	{
		int selectedIndex = lv_ModelList.SelectedIndex;
		if (selectedIndex < 0)
		{
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
		if (text == "" || text.Contains("(未訓練)"))
		{
			return;
		}
		ItemInputWindow itemInputWindow = new ItemInputWindow();
		itemInputWindow.SetTitle(LanguageManager.Instance["InputCopyModelName"]);
		itemInputWindow.SetText(text + "-2");
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
		string text2 = itemInputWindow.InputStr.TrimEnd();
		string text3 = "";
		string text4 = "";
		string text5 = "";
		string text6 = "";
		if (PredictMode)
		{
			text3 = string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, text2);
			text4 = string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, text);
			text5 = text3.Replace(".h5", ".pb");
			text6 = text4.Replace(".h5", ".pb");
		}
		else
		{
			text3 = string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, text2);
			text4 = string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, text);
			text5 = text3.Replace(".h5d", ".pbd");
			text6 = text4.Replace(".h5d", ".pbd");
		}
		if (File.Exists(text3))
		{
			MessageBox.Show(string.Format("{0}{1}{2}，{3}。", LanguageManager.Instance["ModelName"], text2, LanguageManager.Instance["Exist"], LanguageManager.Instance["UseOtherName"]), LanguageManager.Instance["Warning"]);
			return;
		}
		File.Copy(text4, text3, overwrite: true);
		File.Copy(text6, text5, overwrite: true);
		MessageBoxResult num = MessageBox.Show(LanguageManager.Instance["CopyModelImageToo"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
		string sourcePath = string.Format("{0}TrainingImage\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text);
		string destinationPath = string.Format("{0}TrainingImage\\{1}\\", AppDomain.CurrentDomain.BaseDirectory, text2);
		if (num == MessageBoxResult.Yes)
		{
			CopyDirectory(sourcePath, destinationPath, overwriteexisting: true);
		}
		UpdateModels();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting)
	{
		bool flag = false;
		try
		{
			SourcePath = (SourcePath.EndsWith("\\") ? SourcePath : (SourcePath + "\\"));
			DestinationPath = (DestinationPath.EndsWith("\\") ? DestinationPath : (DestinationPath + "\\"));
			if (Directory.Exists(SourcePath))
			{
				if (!Directory.Exists(DestinationPath))
				{
					Directory.CreateDirectory(DestinationPath);
				}
				string[] files = Directory.GetFiles(SourcePath);
				for (int i = 0; i < files.Length; i++)
				{
					FileInfo fileInfo = new FileInfo(files[i]);
					fileInfo.CopyTo(DestinationPath + fileInfo.Name, overwriteexisting);
				}
				files = Directory.GetDirectories(SourcePath);
				foreach (string obj in files)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(obj);
					if (!CopyDirectory(obj, DestinationPath + directoryInfo.Name, overwriteexisting))
					{
						flag = false;
					}
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool ImageFileCopy(string OrgDir, string DstDir, File_Action action = File_Action.EM_Copy, string Filetype = "*.*")
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(OrgDir);
		try
		{
			if (!directoryInfo.Exists)
			{
				return false;
			}
			string[] files = Directory.GetFiles(OrgDir, Filetype, SearchOption.AllDirectories);
			if (files.Length == 0)
			{
				return false;
			}
			if (!Directory.Exists(DstDir))
			{
				Directory.CreateDirectory(DstDir);
			}
			for (int i = 0; i < files.Length; i++)
			{
				string text = Path.Combine(DstDir, Path.GetFileName(files[i]));
				if (!File.Exists(files[i]))
				{
					continue;
				}
				FileInfo fileInfo = new FileInfo(files[i]);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				if (action != File_Action.EM_Delete)
				{
					if (File.Exists(text))
					{
						FileInfo fileInfo2 = new FileInfo(text);
						if (fileInfo2.IsReadOnly)
						{
							fileInfo2.IsReadOnly = false;
						}
					}
					File.Delete(text);
				}
				switch (action)
				{
				case File_Action.EM_Move:
					File.Move(files[i], text);
					break;
				case File_Action.EM_Delete:
					File.Delete(files[i]);
					break;
				case File_Action.EM_Copy:
					File.Copy(files[i], text, overwrite: true);
					break;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			CVBuffer.SaveLog("AITrainingWindow-ImagefileCopy()" + ex.ToString());
			return false;
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
			if (PredictMode)
			{
				text3 = string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, text2);
				text4 = string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, text);
				text5 = text3.Replace(".h5", ".pb");
				text6 = text4.Replace(".h5", ".pb");
			}
			else
			{
				text3 = string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, text2);
				text4 = string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, text);
				text5 = text3.Replace(".h5d", ".pbd");
				text6 = text4.Replace(".h5d", ".pbd");
			}
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
			for (int i = 0; i < 8; i++)
			{
				if (Station[i].paramFile.AIEnhanceParam.ModelName_A == Path.GetFileName(text4))
				{
					Station[i].paramFile.AIEnhanceParam.ModelName_A = Path.GetFileName(text3);
				}
				if (Station[i].paramFile.AIPredictParam.ModelName_A == Path.GetFileName(text4))
				{
					Station[i].paramFile.AIPredictParam.ModelName_A = Path.GetFileName(text3);
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
			if (PredictMode)
			{
				text2 = string.Format("{0}Model\\{1}.h5", AppDomain.CurrentDomain.BaseDirectory, text);
				text3 = text2.Replace(".h5", ".pb");
			}
			else
			{
				text2 = string.Format("{0}Model\\{1}.h5d", AppDomain.CurrentDomain.BaseDirectory, text);
				text3 = text2.Replace(".h5d", ".pbd");
			}
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
	private void Btn_HashTagSetup_Click(object sender, RoutedEventArgs e)
	{
		HashTagEditWindow hashTagEditWindow = new HashTagEditWindow();
		hashTagEditWindow.SetThroughputFile(ThroughputFile);
		hashTagEditWindow.ShowDialog();
		for (int i = 0; i < 7; i++)
		{
			HashTagBtn[i].Content = "#" + ThroughputFile.param.HashTagStr[i];
		}
		EditHashTag = true;
	}

	private void Btn_HashTag1_Click(object sender, RoutedEventArgs e)
	{
		string text = ((Button)sender).Content.ToString();
		text = text.Substring(1);
		txt_SearchBox.Text = text;
		Btn_ModelSearch_Click(null, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/modelswindow.xaml", UriKind.Relative);
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
			((ModelsWindow)target).Closing += Window_Closing;
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
			btn_ModelHome = (Button)target;
			btn_ModelHome.Click += Btn_ModelHome_Click;
			break;
		case 8:
			txt_CurrentModel = (TextBox)target;
			txt_CurrentModel.KeyUp += Txt_SearchBox_KeyUp;
			txt_CurrentModel.PreviewMouseLeftButtonDown += Txt_CurrentModel_PreviewMouseLeftButtonDown;
			break;
		case 9:
			btn_CreateModel = (Button)target;
			btn_CreateModel.Click += Btn_CreateModel_Click;
			break;
		case 10:
			btn_LoadModel = (Button)target;
			btn_LoadModel.Click += Btn_LoadModel_Click;
			break;
		case 11:
			btn_CloneModel = (Button)target;
			btn_CloneModel.Click += Btn_CloneModel_Click;
			break;
		case 12:
			btn_RenameModel = (Button)target;
			btn_RenameModel.Click += Btn_RenameModel_Click;
			break;
		case 13:
			btn_ReSetModel = (Button)target;
			btn_ReSetModel.Click += Btn_ReSetModel_Click;
			break;
		case 14:
			btn_HashTag1 = (Button)target;
			btn_HashTag1.Click += Btn_HashTag1_Click;
			break;
		case 15:
			btn_HashTag2 = (Button)target;
			btn_HashTag2.Click += Btn_HashTag1_Click;
			break;
		case 16:
			btn_HashTag3 = (Button)target;
			btn_HashTag3.Click += Btn_HashTag1_Click;
			break;
		case 17:
			btn_HashTag4 = (Button)target;
			btn_HashTag4.Click += Btn_HashTag1_Click;
			break;
		case 18:
			btn_HashTag5 = (Button)target;
			btn_HashTag5.Click += Btn_HashTag1_Click;
			break;
		case 19:
			btn_HashTag6 = (Button)target;
			btn_HashTag6.Click += Btn_HashTag1_Click;
			break;
		case 20:
			btn_HashTag7 = (Button)target;
			btn_HashTag7.Click += Btn_HashTag1_Click;
			break;
		case 21:
			btn_HashTagSetup = (Button)target;
			btn_HashTagSetup.Click += Btn_HashTagSetup_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
