using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace InspTools.Other;

public class RecipeListWindow : Window, IComponentConnector
{
	private List<string> RecipeSelected = new List<string>();

	internal ListBox lb_RecipeList;

	internal Button btn_RecipeListSelect;

	internal Label lb_SelectCount;

	private bool _contentLoaded;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public RecipeListWindow()
	{
		InitializeComponent();
		string[] directories = Directory.GetDirectories(Environment.CurrentDirectory + "\\recipe\\", "*", SearchOption.TopDirectoryOnly);
		ArrayList arrayList = new ArrayList();
		string[] array = directories;
		foreach (string path in array)
		{
			arrayList.Add(Path.GetFileNameWithoutExtension(path));
		}
		foreach (object item in arrayList)
		{
			lb_RecipeList.Items.Add(item.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Btn_RecipeListSelect_Click(object sender, RoutedEventArgs e)
	{
		if (lb_RecipeList.Items.Count <= 0)
		{
			MessageBox.Show("請選擇要複製的程式。", "警告");
			return;
		}
		for (int i = 0; i < lb_RecipeList.SelectedItems.Count; i++)
		{
			string item = lb_RecipeList.SelectedItems[i].ToString();
			RecipeSelected.Add(item);
		}
		Close();
	}

	private void Lb_RecipeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		lb_SelectCount.Content = lb_RecipeList.SelectedItems.Count.ToString();
	}

	public List<string> GetSelectedRecipes()
	{
		return RecipeSelected;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/InspTools;component/other/recipelistwindow.xaml", UriKind.Relative);
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
			lb_RecipeList = (ListBox)target;
			lb_RecipeList.SelectionChanged += Lb_RecipeList_SelectionChanged;
			break;
		case 2:
			btn_RecipeListSelect = (Button)target;
			btn_RecipeListSelect.Click += Btn_RecipeListSelect_Click;
			break;
		case 3:
			lb_SelectCount = (Label)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
