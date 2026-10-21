using System;
using System.Windows;
using InspTools.SDK;

namespace InspTools.Wpf
{
    public static class AnnotationEditor
    {
        public static bool? ShowDialog(Window owner, IAnnotationSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var editor = new AnnotationEditorControl { Session = session };
            var window = new Window
            {
                Title = "InspTools · AI Annotation · " + session.Image.ModelName,
                Width = 1280,
                Height = 820,
                MinWidth = 1040,
                MinHeight = 680,
                Content = editor,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            editor.Saved += delegate { window.DialogResult = true; window.Close(); };
            editor.Cancelled += delegate { window.DialogResult = false; window.Close(); };
            window.Closing += delegate
            {
                if (window.DialogResult != true && session.IsDirty) session.Cancel();
            };
            return window.ShowDialog();
        }
    }
}
