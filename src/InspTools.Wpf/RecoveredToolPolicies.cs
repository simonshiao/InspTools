using System;
using System.Collections.Generic;

namespace InspTools.Wpf
{
    internal enum RecoveredFieldKind
    {
        Text,
        Boolean
    }

    internal sealed class RecoveredToolField
    {
        internal RecoveredToolField(string key, string label, RecoveredFieldKind kind = RecoveredFieldKind.Text)
        {
            Key = key;
            Label = label;
            Kind = kind;
        }

        internal string Key { get; private set; }
        internal string Label { get; private set; }
        internal RecoveredFieldKind Kind { get; private set; }
    }

    internal sealed class RecoveredToolFieldGroup
    {
        internal RecoveredToolFieldGroup(string title, params RecoveredToolField[] fields)
        {
            Title = title;
            Fields = fields ?? Array.Empty<RecoveredToolField>();
        }

        internal string Title { get; private set; }
        internal IReadOnlyList<RecoveredToolField> Fields { get; private set; }
    }

    internal sealed class RecoveredToolPolicy
    {
        private readonly HashSet<string> editableKeys;

        internal RecoveredToolPolicy(
            string family,
            string recoveredPage,
            string evidence,
            string warning,
            bool dedicatedEditor,
            IEnumerable<string> editableKeys,
            params RecoveredToolFieldGroup[] groups)
        {
            Family = family ?? string.Empty;
            RecoveredPage = recoveredPage ?? string.Empty;
            Evidence = evidence ?? string.Empty;
            Warning = warning ?? string.Empty;
            DedicatedEditor = dedicatedEditor;
            this.editableKeys = new HashSet<string>(editableKeys ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            Groups = groups ?? Array.Empty<RecoveredToolFieldGroup>();
        }

        internal string Family { get; private set; }
        internal string RecoveredPage { get; private set; }
        internal string Evidence { get; private set; }
        internal string Warning { get; private set; }
        internal bool DedicatedEditor { get; private set; }
        internal IReadOnlyList<RecoveredToolFieldGroup> Groups { get; private set; }
        internal bool HasEditableKeys => editableKeys.Count > 0;

        internal bool CanEdit(string key)
            => !string.IsNullOrWhiteSpace(key) && editableKeys.Contains(key.Trim());
    }

    /// <summary>
    /// IR2-04 parity policy distilled from phase-1 recovered binders.
    /// Keep this list deliberately narrower than the raw INI surface: existence of a key is not
    /// sufficient evidence that its original editor lifecycle has been recovered.
    /// </summary>
    internal static class RecoveredToolPolicies
    {
        private static readonly RecoveredToolPolicy Locate = BuildLocate();
        private static readonly RecoveredToolPolicy Binary = BuildBinary();
        private static readonly RecoveredToolPolicy Mask = BuildMask();
        private static readonly RecoveredToolPolicy MatchProtected = new RecoveredToolPolicy(
            "Match",
            "matchtoolwindow.xaml",
            "verified_runtime safe scalar binder; Pattern DB / ROI / OCR aliases protected",
            "Pattern DB、public DB、ROI、OCR alias lifecycle 沒有完整 parity；IR2-04 目前保持 Raw parameters 只讀。",
            false,
            Array.Empty<string>());
        private static readonly RecoveredToolPolicy FontProtected = new RecoveredToolPolicy(
            "Font",
            "fonttoolwindow.xaml",
            "phase-1 capability = READ_ONLY+TEST",
            "Font/OCR DB、Training、Pattern lifecycle 未開放寫入；IR2-04 保持唯讀。",
            false,
            Array.Empty<string>());

        internal static RecoveredToolPolicy Resolve(string family, string section)
        {
            string f = family ?? string.Empty;
            if (string.Equals(f, "Locate", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(section, "Locate", StringComparison.OrdinalIgnoreCase)) return Locate;
            if (string.Equals(f, "Binary", StringComparison.OrdinalIgnoreCase)) return Binary;
            if (string.Equals(f, "Mask", StringComparison.OrdinalIgnoreCase)) return Mask;
            if (string.Equals(f, "Match", StringComparison.OrdinalIgnoreCase)) return MatchProtected;
            if (string.Equals(f, "Font", StringComparison.OrdinalIgnoreCase)) return FontProtected;
            return null;
        }

        private static RecoveredToolPolicy BuildLocate()
        {
            string[] keys =
            {
                "LocateAngleLimit", "LocateWidthMin", "LocateWidthMax", "LocateHeightMin", "LocateHeightMax",
                "LocateErrorBin", "LocateSizeErrorBin", "ParetoIndex", "LocateSpeedUp", "doRotateMinAngle"
            };

            return new RecoveredToolPolicy(
                "Locate",
                "locatewindow.xaml",
                "LocateDialogBinder read/write scalar verified_runtime",
                "只有第一期 LocateDialogBinder 明確綁定的 10 個 scalar keys 可由本專用頁修改；其餘 206-key raw surface 僅供查閱。",
                true,
                keys,
                new RecoveredToolFieldGroup("定位尺寸 / 旋轉",
                    T("LocateAngleLimit", "旋轉角度限制"),
                    T("LocateWidthMin", "寬度最小"),
                    T("LocateWidthMax", "寬度最大"),
                    T("LocateHeightMin", "高度最小"),
                    T("LocateHeightMax", "高度最大"),
                    T("doRotateMinAngle", "執行旋轉最小角度")),
                new RecoveredToolFieldGroup("結果 / 效能",
                    T("LocateErrorBin", "定位錯誤 Bin"),
                    T("LocateSizeErrorBin", "尺寸錯誤 Bin"),
                    T("ParetoIndex", "Pareto Index"),
                    B("LocateSpeedUp", "定位加速")));
        }

        private static RecoveredToolPolicy BuildBinary()
        {
            var groups = new List<RecoveredToolFieldGroup>();
            var editable = new List<string>();

            AddGroup(groups, editable, "Tracker / 判定",
                T("TrackerName", "Tracker 名稱"),
                B("TrackerON", "Tracker 啟用"),
                T("ErrorBin", "錯誤 Bin"),
                T("ParetoIndex", "Pareto Index"),
                T("MaskSelectIndex", "Mask Index"),
                B("UseMaskAsInspArea", "Mask 作為檢測區"),
                T("ColorReferenceIndex", "顏色參考 Index"),
                B("THD_ORMode", "Threshold OR 模式"),
                B("TrackErrorSetNG", "追蹤失敗設 NG"),
                B("SearchRect2ON", "第二搜尋區啟用"),
                B("Tracker1UseAutoSize", "Tracker1 Auto Size"),
                B("Tracker2UseAutoSize", "Tracker2 Auto Size"),
                T("AutoSize1_SelectIndex", "Tracker1 Auto Size Index"),
                T("AutoSize2_SelectIndex", "Tracker2 Auto Size Index"),
                T("Tracker1AligmentXIndex", "Tracker1 Align X"),
                T("Tracker1AligmentYIndex", "Tracker1 Align Y"),
                T("Tracker2AligmentXIndex", "Tracker2 Align X"),
                T("Tracker2AligmentYIndex", "Tracker2 Align Y"),
                B("EnableSoftCorrection", "Soft Correction"),
                B("UseFontMask", "使用 Font Mask"),
                B("EnableSelectFace", "Select Face"),
                B("UsePixelValue", "Blob 使用 Pixel Value"));

            for (int i = 1; i <= 4; i++)
            {
                var threshold = new[]
                {
                    T("THD" + i + "_ChannelIndex", "Channel"),
                    B("THD" + i + "_ChannelON", "啟用"),
                    T("THD" + i + "_Min", "Min"),
                    T("THD" + i + "_Max", "Max"),
                    T("DE_Start" + i + "_Index", "DE Start"),
                    T("DE_End" + i + "_Index", "DE End"),
                    T("DE_Start" + i + "_Count", "DE Start Count"),
                    T("DE_End" + i + "_Count", "DE End Count")
                };
                AddGroup(groups, editable, "Threshold " + i, threshold);
            }

            AddGroup(groups, editable, "Blob / Line",
                T("BlobAnalysisSelect", "Blob Analysis"),
                T("BlobLineSort", "Blob Line Sort"),
                T("BlobLineNum", "Blob Line Num"),
                T("BlobLineMin", "Blob Line Min"),
                T("BlobLineMax", "Blob Line Max"),
                T("BlobLineWidthMin", "Blob Width Min"),
                T("BlobLineWidthMax", "Blob Width Max"),
                T("BlobLineHeightMin", "Blob Height Min"),
                T("BlobLineHeightMax", "Blob Height Max"),
                T("BlobLineAreaMin", "Blob Area Min"),
                T("BlobLineAreaMax", "Blob Area Max"),
                T("LineWidthMinW", "Line Width Min W"),
                T("LineWidthMinH", "Line Width Min H"),
                T("LineWidthMinArea", "Line Width Min Area"),
                T("AllowLineWidth", "Allow Line Width"),
                T("AllowLineHeight", "Allow Line Height"),
                T("AllowLineError", "Allow Line Error"),
                T("AllowErrorW", "Allow Error W"),
                T("AllowErrorH", "Allow Error H"),
                B("EnableLineWidthA", "Enable Line Area"),
                B("EnableLineWidthW", "Enable Line Width"),
                B("EnableLineWidthH", "Enable Line Height"));

            var ruleFields = new List<RecoveredToolField>
            {
                B("InspRuleAND", "Inspection Rule AND"),
                B("MB_InspRuleAND", "Multi-Blob Rule AND"),
                T("MB_BlobSort", "Multi-Blob Sort"),
                T("MB_BlobNum", "Multi-Blob Num"),
                T("MB_AllowNGCount", "Multi-Blob Allow NG Count")
            };
            for (int i = 1; i <= 4; i++)
            {
                ruleFields.Add(T("InspRule" + i + "_Index", "Rule " + i + " Index"));
                ruleFields.Add(T("InspRule" + i + "_Min", "Rule " + i + " Min"));
                ruleFields.Add(T("InspRule" + i + "_Max", "Rule " + i + " Max"));
                ruleFields.Add(T("MB_InspRule" + i + "_Index", "MB Rule " + i + " Index"));
                ruleFields.Add(T("MB_InspRule" + i + "_Min", "MB Rule " + i + " Min"));
                ruleFields.Add(T("MB_InspRule" + i + "_Max", "MB Rule " + i + " Max"));
            }
            AddGroup(groups, editable, "Inspection rules", ruleFields.ToArray());

            var relationFields = new List<RecoveredToolField>();
            for (int i = 0; i < 4; i++)
            {
                relationFields.Add(T("T1FillletCount" + i, "Tracker1 Fillet " + i));
                relationFields.Add(T("T2FillletCount" + i, "Tracker2 Fillet " + i));
            }
            for (int i = 1; i <= 3; i++)
            {
                relationFields.Add(T("UseMatchTextMask" + i, "Match Text Mask " + i));
                relationFields.Add(T("MatchTextDE_Start" + i + "_Index", "Match Text DE " + i + " Start"));
                relationFields.Add(T("MatchTextDE_Start" + i + "_Count", "Match Text DE " + i + " Count"));
            }
            AddGroup(groups, editable, "Tracker relation / text mask", relationFields.ToArray());

            return new RecoveredToolPolicy(
                "Binary",
                "binarywindow.xaml",
                "BinaryDialogBinder representable scalar set verified_runtime",
                "本頁只開放第一期 BinaryDialogBinder 可 round-trip 的 scalar set；Raw 140-key surface 只讀。",
                true,
                editable,
                groups.ToArray());
        }

        private static RecoveredToolPolicy BuildMask()
        {
            var editable = new List<string> { "TrackerON", "TrackerName" };
            var groups = new List<RecoveredToolFieldGroup>
            {
                new RecoveredToolFieldGroup("Mask Tracker",
                    T("TrackerName", "Tracker 名稱"),
                    B("TrackerON", "Tracker 啟用"))
            };

            for (int i = 1; i <= 4; i++)
            {
                var threshold = new[]
                {
                    T("THD" + i + "_ChannelIndex", "Channel"),
                    B("THD" + i + "_ChannelON", "啟用"),
                    T("THD" + i + "_Min", "Min"),
                    T("THD" + i + "_Max", "Max"),
                    T("DE_Start" + i + "_Index", "DE Start"),
                    T("DE_End" + i + "_Index", "DE End"),
                    T("DE_Start" + i + "_Count", "DE Start Count"),
                    T("DE_End" + i + "_Count", "DE End Count")
                };
                groups.Add(new RecoveredToolFieldGroup("Mask Threshold " + i, threshold));
                foreach (RecoveredToolField field in threshold) editable.Add(field.Key);
            }

            RecoveredToolField[] morphology =
            {
                B("FillHole", "填洞"),
                B("doConvexHull", "Convex Hull"),
                T("DE_FinalStart_Index", "Final DE Start"),
                T("DE_FinalStart_Count", "Final DE Start Count"),
                T("DE_FinalEnd_Index", "Final DE End"),
                T("DE_FinalEnd_Count", "Final DE End Count"),
                T("MaxBlob", "Max Blob")
            };
            groups.Add(new RecoveredToolFieldGroup("Mask Morphology", morphology));
            foreach (RecoveredToolField field in morphology) editable.Add(field.Key);

            return new RecoveredToolPolicy(
                "Mask",
                "maskwindow.xaml",
                "MaskDialogBinder scalar read/write verified_runtime; bitmap/ROI read-only",
                "只開放第一期 MaskDialogBinder 已驗證的 threshold / morphology scalar。Mask bitmap、Mask path、complex ROI 與任何未列入 binder Capture 的 key 一律唯讀。",
                true,
                editable,
                groups.ToArray());
        }

        private static RecoveredToolField T(string key, string label)
            => new RecoveredToolField(key, label, RecoveredFieldKind.Text);

        private static RecoveredToolField B(string key, string label)
            => new RecoveredToolField(key, label, RecoveredFieldKind.Boolean);

        private static void AddGroup(
            IList<RecoveredToolFieldGroup> groups,
            IList<string> editable,
            string title,
            params RecoveredToolField[] fields)
        {
            groups.Add(new RecoveredToolFieldGroup(title, fields));
            foreach (RecoveredToolField field in fields) editable.Add(field.Key);
        }
    }
}
