using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace InspTools.Standalone
{
    /// <summary>
    /// Isolated R2-only test of the original ParamFile reader/writer.
    /// Avoids CVInspection construction, ReadModel, Inspect, PLC, camera and GUI.
    /// Native ParamFile can write INI; probe accepts only isolated recipe clones.
    /// </summary>
    internal static class NativeRecipeProbe
    {
        internal static bool TryRun(string[] args, out int code)
        {
            code = 0;
            if (args == null || args.Length < 1 ||
                !string.Equals(args[0], "--recipe-native-probe", StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                if (args.Length < 2 || args.Length > 3)
                    throw new ArgumentException("Usage: --recipe-native-probe R2PROBE-xxx [--roundtrip]");
                string name = args[1];
                if (!name.StartsWith("R2PROBE-", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("原生探測只允許 R2PROBE- 前綴的隔離副本");
                bool roundtrip = args.Length == 3 &&
                    string.Equals(args[2], "--roundtrip", StringComparison.OrdinalIgnoreCase);
                if (args.Length == 3 && !roundtrip) throw new ArgumentException("Invalid option");
                StandaloneRecipeStore.Validate(name);

                string dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll");
                Assembly asm = Assembly.LoadFrom(dll);
                Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
                Type iniType = asm.GetType("InspTools.HiniFile", true);
                FieldInfo locate = paramType.GetField("LocateToolON",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo binary = paramType.GetField("BinaryToolON",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (locate == null || binary == null)
                    throw new MissingMemberException("原廠 LocateToolON / BinaryToolON 欄位不存在");

                int loaded = 0;
                for (int index = 0; index < 8; index++)
                {
                    object parameters = OpenNativeParamFile(paramType, iniType, name, index);
                    bool locateValue = Convert.ToBoolean(locate.GetValue(parameters));
                    bool binaryValue = Convert.ToBoolean(binary.GetValue(parameters));
                    Console.WriteLine("S" + (index + 1) + " READ=PASS LocateToolON=" +
                        locateValue + " BinaryToolON=" + binaryValue);
                    loaded++;

                    if (roundtrip && index == 0)
                    {
                        // Verify the original writer, not a hand-written INI patch.
                        // Only the R2PROBE local copy is affected.
                        MethodInfo writer = paramType.GetMethod("WriteAllParam",
                            BindingFlags.Public | BindingFlags.Instance);
                        if (writer == null) throw new MissingMethodException("WriteAllParam");
                        binary.SetValue(parameters, !binaryValue);
                        if (!Convert.ToBoolean(writer.Invoke(parameters, null)))
                            throw new InvalidDataException("S1 WriteAllParam 回傳失敗");

                        object reread = OpenNativeParamFile(paramType, iniType, name, index);
                        bool actual = Convert.ToBoolean(binary.GetValue(reread));
                        if (actual != !binaryValue)
                            throw new InvalidDataException("S1 WriteAllParam 後重新載入結果不一致");

                        // Revert the property using the same original writer.
                        binary.SetValue(reread, binaryValue);
                        if (!Convert.ToBoolean(writer.Invoke(reread, null)))
                            throw new InvalidDataException("S1 恢復原值寫入失敗");
                        object verified = OpenNativeParamFile(paramType, iniType, name, index);
                        if (Convert.ToBoolean(binary.GetValue(verified)) != binaryValue)
                            throw new InvalidDataException("S1 無法恢復原值");
                        Console.WriteLine("S1 WRITE_REREAD_RESTORE=PASS");
                    }
                }
                Console.WriteLine("NATIVE_RECIPE_PROBE=PASS STATIONS=" + loaded);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("NATIVE_RECIPE_PROBE=FAIL " + ex.GetBaseException());
                code = 4;
            }
            return true;
        }

        internal static object OpenNativeParamFile(Type paramType, Type iniType, string name, int index)
        {
            string stationPath = Path.Combine(StandaloneRecipeStore.Validate(name),
                "station" + (index + 1) + ".ini");
            object param = Activator.CreateInstance(paramType);
            object ini = Activator.CreateInstance(iniType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new object[] { stationPath, ";" }, null);
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            paramType.GetMethod("SetINI", flags).Invoke(param, new[] { ini });
            paramType.GetMethod("SetIniFile", flags).Invoke(param,
                new object[] { stationPath, index, System.Text.Encoding.Default });
            paramType.GetMethod("SetRecipeName", flags).Invoke(param, new object[] { name });
            paramType.GetMethod("SetStationIndex", flags).Invoke(param, new object[] { index });
            MethodInfo reader = paramType.GetMethod("ReadAllParam", flags);
            if (reader == null || !Convert.ToBoolean(reader.Invoke(param, null)))
                throw new InvalidDataException("S" + (index + 1) + " 原版 ReadAllParam 回傳失敗");
            return param;
        }

        /// <summary>
        /// GUI read-only Recipe overview. Uses the exact same native ParamFile
        /// as the probe, but never calls WriteAllParam or creates CVInspection.
        /// </summary>
        internal static string ReadOnlySummary(string name)
        {
            StandaloneRecipeStore.Validate(name);
            string dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll");
            Assembly asm = Assembly.LoadFrom(dll);
            Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
            Type iniType = asm.GetType("InspTools.HiniFile", true);
            var report = new StringBuilder();
            report.AppendLine("Recipe：" + name);
            report.AppendLine("來源：本程式 Recipe 目錄（唯讀）");
            report.AppendLine();
            for (int i = 0; i < 8; i++)
            {
                object param = OpenNativeParamFile(paramType, iniType, name, i);
                report.AppendLine("S" + (i + 1)
                    + "  定位=" + ReadFlag(paramType, param, "LocateToolON")
                    + "  二值化=" + ReadFlag(paramType, param, "BinaryToolON")
                    + "  AI前處理=" + ReadFlag(paramType, param, "AIEnhanceToolON")
                    + "  預分類=" + ReadFlag(paramType, param, "AIProphetToolON"));
            }
            report.AppendLine();
            report.Append("僅讀取原生 ParamFile；未啟動 AI 模型／檢測。");
            return report.ToString();
        }

        private static string ReadFlag(Type type, object param, string fieldName)
        {
            FieldInfo flag = type.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return flag == null ? "未知" : (Convert.ToBoolean(flag.GetValue(param)) ? "開" : "關");
        }

        private static readonly string[] EditableFlags =
        {
            "LocateToolON", "BinaryToolON", "AIEnhanceToolON", "AIProphetToolON"
        };

        internal static Dictionary<string, bool> ReadEditableFlags(string recipeName, int station)
        {
            if (station < 1 || station > 8) throw new ArgumentOutOfRangeException(nameof(station));
            var asm = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll"));
            Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
            Type iniType = asm.GetType("InspTools.HiniFile", true);
            object param = OpenNativeParamFile(paramType, iniType, recipeName, station - 1);
            var values = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (string key in EditableFlags)
            {
                FieldInfo field = RequireEditableField(paramType, key);
                values.Add(key, Convert.ToBoolean(field.GetValue(param)));
            }
            return values;
        }

        internal static string WriteEditableFlags(string recipeName, int station,
            IDictionary<string, bool> values)
        {
            if (station < 1 || station > 8) throw new ArgumentOutOfRangeException(nameof(station));
            if (values == null || values.Count == 0) throw new ArgumentException("沒有要儲存的設定");
            string recipe = StandaloneRecipeStore.Validate(recipeName);
            string ini = Path.Combine(recipe, "station" + station + ".ini");

            var asm = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InspTools.dll"));
            Type paramType = asm.GetType("InspTools.AOI.ParamFile", true);
            Type iniType = asm.GetType("InspTools.HiniFile", true);
            object param = OpenNativeParamFile(paramType, iniType, recipeName, station - 1);
            var expected = new Dictionary<string, bool>(StringComparer.Ordinal);
            int changed = 0;
            foreach (var edit in values)
            {
                FieldInfo field = RequireEditableField(paramType, edit.Key);
                bool previous = Convert.ToBoolean(field.GetValue(param));
                if (previous != edit.Value) changed++;
                field.SetValue(param, edit.Value);
                expected.Add(edit.Key, edit.Value);
            }
            if (changed == 0) return "UNCHANGED";

            // A separate local backup is taken BEFORE invoking the vendor writer.
            // The vendor ParamFile writer may rewrite many sections, not only flags.
            string backupFolder = Path.Combine(StandaloneRecipeStore.Root,
                "_BackupRecipe", recipeName);
            Directory.CreateDirectory(backupFolder);
            string backup = Path.Combine(backupFolder, "station" + station + "-" +
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + ".ini");
            byte[] prior = File.ReadAllBytes(ini);
            File.WriteAllBytes(backup, prior);

            try
            {
                MethodInfo writer = paramType.GetMethod("WriteAllParam",
                    BindingFlags.Public | BindingFlags.Instance);
                if (writer == null || !Convert.ToBoolean(writer.Invoke(param, null)))
                    throw new InvalidDataException("原版 WriteAllParam 回傳失敗");
                object reread = OpenNativeParamFile(paramType, iniType, recipeName, station - 1);
                foreach (var edit in expected)
                {
                    FieldInfo field = RequireEditableField(paramType, edit.Key);
                    if (Convert.ToBoolean(field.GetValue(reread)) != edit.Value)
                        throw new InvalidDataException(edit.Key + " 寫入後重讀不一致");
                }
                return backup;
            }
            catch
            {
                // Restore precisely the original bytes on failed validation.
                File.WriteAllBytes(ini, prior);
                throw;
            }
        }

        private static FieldInfo RequireEditableField(Type paramType, string key)
        {
            bool permitted = false;
            foreach (string allowed in EditableFlags)
                if (string.Equals(allowed, key, StringComparison.Ordinal)) permitted = true;
            if (!permitted) throw new ArgumentException("不允許變更設定：" + key);
            FieldInfo field = paramType.GetField(key,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(bool))
                throw new MissingFieldException("InspTools.AOI.ParamFile", key);
            return field;
        }
    }
}
