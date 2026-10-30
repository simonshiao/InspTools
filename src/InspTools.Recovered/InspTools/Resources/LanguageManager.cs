using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace InspTools.Resources;

public class LanguageManager : INotifyPropertyChanged
{
	private readonly ResourceManager _resourceManager;

	private static readonly Lazy<LanguageManager> _lazy = new Lazy<LanguageManager>(() => new LanguageManager());

	public static LanguageManager Instance => _lazy.Value;

	public string this[string name]
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return _resourceManager.GetString(name);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public LanguageManager()
	{
		_resourceManager = new ResourceManager("InspTools.Resources.Lang", typeof(LanguageManager).Assembly);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void ChangeLanguage(CultureInfo cultureInfo)
	{
		CultureInfo.CurrentCulture = cultureInfo;
		CultureInfo.CurrentUICulture = cultureInfo;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("item[]"));
	}
}
