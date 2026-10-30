using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

public class FolderSelectDialog
{
	private OpenFileDialog ofd;

	public string InitialDirectory
	{
		get
		{
			return ofd.InitialDirectory;
		}
		set
		{
			ofd.InitialDirectory = ((value == null || value.Length == 0) ? Environment.CurrentDirectory : value);
		}
	}

	public string Title
	{
		get
		{
			return ofd.Title;
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		set
		{
			ofd.Title = ((value == null) ? "Select a folder" : value);
		}
	}

	public string FileName => ofd.FileName;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public FolderSelectDialog()
	{
		ofd = new OpenFileDialog();
		ofd.Filter = "Folders|\n";
		ofd.AddExtension = false;
		ofd.CheckFileExists = false;
		ofd.DereferenceLinks = true;
		ofd.Multiselect = false;
	}

	public bool ShowDialog()
	{
		return ShowDialog(IntPtr.Zero);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public bool ShowDialog(IntPtr hWndOwner)
	{
		bool result = false;
		if (Environment.OSVersion.Version.Major >= 6)
		{
			Reflector reflector = new Reflector("System.Windows.Forms");
			uint num = 0u;
			Type type = reflector.GetType("FileDialogNative.IFileDialog");
			object obj = reflector.Call(ofd, "CreateVistaDialog");
			reflector.Call(ofd, "OnBeforeVistaDialog", obj);
			uint num2 = (uint)reflector.CallAs(typeof(FileDialog), ofd, "GetOptions");
			num2 |= (uint)reflector.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
			reflector.CallAs(type, obj, "SetOptions", num2);
			object obj2 = reflector.New("FileDialog.VistaDialogEvents", ofd);
			object[] array = new object[2] { obj2, num };
			reflector.CallAs2(type, obj, "Advise", array);
			num = (uint)array[1];
			try
			{
				int num3 = (int)reflector.CallAs(type, obj, "Show", hWndOwner);
				result = num3 == 0;
			}
			finally
			{
				reflector.CallAs(type, obj, "Unadvise", num);
				GC.KeepAlive(obj2);
			}
		}
		else
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = Title;
			folderBrowserDialog.SelectedPath = InitialDirectory;
			folderBrowserDialog.ShowNewFolderButton = false;
			if (folderBrowserDialog.ShowDialog(new WindowWrapper(hWndOwner)) != DialogResult.OK)
			{
				return false;
			}
			ofd.FileName = folderBrowserDialog.SelectedPath;
			result = true;
		}
		return result;
	}
}
