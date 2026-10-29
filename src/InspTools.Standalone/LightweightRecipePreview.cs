using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace InspTools.Standalone
{
    /// <summary>
    /// Experimental no-model binder: reuse the original ParamFile objects in
    /// the compiled ToolsWindow visual tree, but DO NOT instantiate CVInspection.
    /// This is a read-only GUI preview, not a working AOI/AI/recipe editor.
    /// </summary>
    internal sealed class LightweightRecipePreview
    {
        private readonly Window _window;
        private readonly string _recipeName;
        private readonly MethodInfo _selectStation;
        private readonly FieldInfo _selectedBinary;
        private readonly bool[] _expectedBinary;

        private LightweightRecipePreview(Window window, string recipe,
            MethodInfo selectStation, FieldInfo selectedBinary, bool[] expectedBinary)
        {
            _window = window;
            _recipeName = recipe;
            _selectStation = selectStation;
            _selectedBinary = selectedBinary;
            _expectedBinary = expectedBinary;
        }

        internal static LightweightRecipePreview Load(Window window, string recipe, int station)
        {
            if (station < 1 || station > 8) throw new ArgumentOutOfRangeException(nameof(station));
            StandaloneRecipeStore.Validate(recipe);

            Type toolsType = window.GetType();
            Assembly asm = toolsType.Assembly;
            Type cvType = asm.GetType("InspTools.Station.CVInspection", true);
            Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
            Type iniType = asm.GetType("InspTools.HiniFile", true);
            FieldInfo param = cvType.GetField("paramFile",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (param == null) throw new MissingFieldException("CVInspection", "paramFile");
            Array stations = Array.CreateInstance(cvType, 8);
            var binaryValues = new bool[8];
            FieldInfo binaryField = paramType.GetField("BinaryToolON",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (binaryField == null) throw new MissingFieldException("ParamFile", "BinaryToolON");
            for (int i = 0; i < 8; i++)
            {
                object holder = FormatterServices.GetUninitializedObject(cvType);
                object parameters = NativeRecipeProbe.OpenNativeParamFile(paramType, iniType, recipe, i);
                binaryValues[i] = Convert.ToBoolean(binaryField.GetValue(parameters));
                param.SetValue(holder, parameters);
                stations.SetValue(holder, i);
            }

            MethodInfo setStation = toolsType.GetMethod("SetStation",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo selectStation = toolsType.GetMethod("SetStationIndex",
                BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
            FieldInfo binaryCheckbox = toolsType.GetField("chk_BinaryToolON",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (setStation == null || selectStation == null || binaryCheckbox == null)
                throw new MissingMemberException("原版視窗缺少 SetStation / SetStationIndex / chk_BinaryToolON");
            setStation.Invoke(window, new object[] { stations });
            var preview = new LightweightRecipePreview(window, recipe, selectStation, binaryCheckbox, binaryValues);
            preview.SelectStation(station);
            return preview;
        }

        internal void SelectStation(int station)
        {
            if (station < 1 || station > 8) throw new ArgumentOutOfRangeException(nameof(station));
            _selectStation.Invoke(_window, new object[] { station - 1 });
            // Fail rather than silently show the wrong station state.
            CheckBox checkbox = _selectedBinary.GetValue(_window) as CheckBox;
            if (checkbox == null || !checkbox.IsChecked.HasValue)
                throw new InvalidDataException("原版站別二值化 Checkbox 未能同步");
            if (checkbox.IsChecked.Value != _expectedBinary[station - 1])
                throw new InvalidDataException("S" + station +
                    " 原版 Checkbox 與原生 ParamFile 值不一致");
            _window.Title = "INSPECTION TOOLS - [參數預覽／不可檢測] " +
                _recipeName + " / S" + station;
        }

        internal void AttachStationMenu()
        {
            ContextMenu menu = _window.ContextMenu;
            if (menu == null) return;
            for (int i = menu.Items.Count - 1; i >= 0; i--)
            {
                var previous = menu.Items[i] as MenuItem;
                if (previous != null && Equals(previous.Tag, "R2_PARAM_STATIONS"))
                    menu.Items.RemoveAt(i);
            }
            var stations = new MenuItem
            {
                Header = "預覽切換 S1～S8（唯讀）",
                Tag = "R2_PARAM_STATIONS"
            };
            for (int i = 1; i <= 8; i++)
            {
                int index = i;
                var item = new MenuItem { Header = "S" + index };
                item.Click += delegate
                {
                    try { SelectStation(index); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(_window, ex.GetBaseException().Message,
                            "站別切換失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };
                stations.Items.Add(item);
            }
            menu.Items.Insert(0, stations);
        }

        internal static void RestrictOriginalEditing(DependencyObject window)
        {
            // Original child dialogs call into CVInspection tool objects that
            // are deliberately absent in preview mode. Leave read-only native
            // checkbox state visible while disabling original tool operations.
            DisableInputs(window);
        }

        private static void DisableInputs(DependencyObject node)
        {
            int n = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < n; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                var check = child as ToggleButton;
                if (check != null) check.IsHitTestVisible = false;
                var button = child as Button;
                if (button != null) button.IsHitTestVisible = false;
                DisableInputs(child);
            }
        }
    }
}
