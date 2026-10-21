using System;
using System.Windows;
using InspTools.SDK;

namespace InspTools.Wpf
{
    public static class InspToolsEditor
    {
        public static bool? ShowDialog(Window owner, IInspToolsSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var editor = new InspToolsEditorControl { Session = session };
            var window = new Window
            {
                Title = $"InspTools · {session.RecipeName} · S{session.Station}",
                Width = 1240,
                Height = 760,
                MinWidth = 1040,
                MinHeight = 640,
                Content = editor,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            editor.SaveRequested += delegate { window.DialogResult = true; window.Close(); };
            editor.CancelRequested += delegate { window.DialogResult = false; window.Close(); };
            window.Closing += delegate
            {
                if (window.DialogResult != true && session.IsDirty) session.Cancel();
            };
            return window.ShowDialog();
        }
    }
}
