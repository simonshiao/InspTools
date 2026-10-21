using System;
using System.IO;
using System.Text;
using System.Windows;

namespace InspTools.Standalone
{
    internal static class StandaloneEntry
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                int recipeExitCode;
                if (StandaloneRecipeCli.TryRun(args, out recipeExitCode))
                    return recipeExitCode;
                int result = DecodedToolsWindowHost.Run(args);
                // Compiled InspTools starts non-background worker threads. WPF
                // Application.Run has already ended; do not leave a headless
                // process alive after the only standalone window has closed.
                Environment.Exit(result);
                return result;
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspToolsStandalone.error.txt"),
                        ex.ToString(),
                        new UTF8Encoding(false));
                }
                catch
                {
                }

                MessageBox.Show(
                    ex.GetBaseException().Message,
                    "InspTools Standalone 啟動失敗",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 31;
            }
        }
    }
}
