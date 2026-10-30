using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace InspTools.Standalone
{
    /// <summary>
    /// Mirrors Chao8Side's first-party AI Prophet UI bridge for an explicitly
    /// marked portable research candidate. It never edits authorization files
    /// or the InspTools assembly; only the loaded candidate's in-memory station
    /// permission fields are changed after the self-contained trainer is staged.
    /// </summary>
    internal static class Chao8SideAiProphetUiBridge
    {
        private const string MarkerName = "chao8side-ai-prophet-ui.marker";
        private const string MarkerValue = "INSPTOOLS_CHAO8SIDE_AI_PROPHET_UI_V1";

        internal static bool Attach(Window toolsWindow)
        {
            if (toolsWindow == null || !NativeR3RecipeSession.IsPortableReview())
                return false;

            string root = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string marker = Path.Combine(root, MarkerName);
            if (!File.Exists(marker) ||
                !string.Equals(File.ReadAllText(marker).Trim(), MarkerValue,
                    StringComparison.Ordinal))
                return false;

            string[] required = {
                @"PModel\train\dist\AIProphet\AIProphet.exe",
                @"PModel\train\AIProphetRebuild\aiprophet.py",
                @"PModel\train\weights\backbone.pth",
                @"Runtime\AIRuntime\ChaoAIRuntime.exe",
                @"Runtime\AIProphet\Trainer\AIProphet.exe"
            };
            foreach (string relative in required)
            {
                if (!File.Exists(Path.Combine(root, relative)))
                {
                    WriteLog(root, "bridge skipped; missing " + relative);
                    return false;
                }
            }

            Type windowType = toolsWindow.GetType();
            FieldInfo stationField = FindField(windowType, "Station");
            MethodInfo setVisibility = windowType.GetMethod("SetAIProphetON",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo unlockField = FindField(windowType, "btn_UnLockAuth");
            if (stationField == null || setVisibility == null)
            {
                WriteLog(root, "bridge skipped; original station field or SetAIProphetON missing");
                return false;
            }

            int changed = 0;
            Array stations = stationField.GetValue(toolsWindow) as Array;
            if (stations == null)
            {
                WriteLog(root, "bridge skipped; original station array is not initialized");
                return false;
            }

            foreach (object station in stations)
            {
                if (station == null) continue;
                FieldInfo paramField = FindField(station.GetType(), "paramFile");
                object param = paramField == null ? null : paramField.GetValue(station);
                if (param == null) continue;
                FieldInfo allowField = FindField(param.GetType(), "AllowEnableAIProphet");
                if (allowField == null || allowField.FieldType != typeof(bool)) continue;
                if (!Convert.ToBoolean(allowField.GetValue(param)))
                {
                    allowField.SetValue(param, true);
                    changed++;
                }
            }

            object unlock = unlockField == null ? null : unlockField.GetValue(toolsWindow);
            FrameworkElement unlockElement = unlock as FrameworkElement;
            if (unlockElement != null)
            {
                unlockElement.Visibility = Visibility.Hidden;
                unlockElement.IsHitTestVisible = false;
            }

            try
            {
                setVisibility.Invoke(toolsWindow, new object[] { true });
                toolsWindow.UpdateLayout();
                WriteLog(root,
                    "original AI Prophet controls enabled by isolated Chao8Side bridge; " +
                    "in-memory station flags changed=" + changed +
                    "; authorization files and InspTools.dll untouched");
                return true;
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException ?? ex;
                WriteLog(root, "SetAIProphetON failed: " + inner.GetType().Name + " " + inner.Message);
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(root, "bridge failed: " + ex.GetType().Name + " " + ex.Message);
                return false;
            }
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name,
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }

        private static void WriteLog(string root, string message)
        {
            try
            {
                File.AppendAllText(Path.Combine(root, "chao8side-ai-prophet-ui.log"),
                    DateTime.Now.ToString("O") + " " + message + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }
    }
}
