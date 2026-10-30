using OpenCvSharp;

namespace InspTools.AOI;

public struct INSP_MATCHTOOL_PARAM
{
	public bool ValueChangeFlag;

	public bool NameChangeFlag;

	public bool TrackerON;

	public bool Locked;

	public string TrackerName;

	public Rect SearchRect1;

	public bool Tracker1UseAutoSize;

	public int Tracker1AligmentXIndex;

	public int Tracker1AligmentYIndex;

	public int Tracker1AligmentX;

	public int Tracker1AligmentY;

	public int PatternSelect;

	public int PatternSelect2;

	public int PatternSelect3;

	public int PatternSelect4;

	public int AutoSize1_SelectIndex;

	public int Track1_T;

	public int Track1_B;

	public int Track1_L;

	public int Track1_R;

	public int MatchScoreMin;

	public int MatchScoreMax;

	public int ShapeMatchScoreMin;

	public bool UseTextMatch;

	public int TextPosDiff;

	public int TextAreaDiff;

	public int TextWidthDiff;

	public int TextHeightDiff;

	public double MinColorMatchScore;

	public bool TextisWhite;

	public bool UseFoundToMask;

	public bool MaskInFound;

	public bool EnableMatchShape;

	public bool ShapeTextisWhite;

	public int MinMatchShapoeScore;

	public int DEStart;

	public int DEStartCount;

	public Mat TextMat;

	public Rect TextRect;

	public bool TextMatResult;

	public int ParetoIndex;

	public bool EnableSelectFace;

	public int FaceIndex;

	public bool EnableRotateInspect;

	public int FindCloseArea;

	public int FindTextArea;

	public double ClosedArea;

	public double MaxClosedArea;

	public double TextArea;

	public double MaxTextArea;

	public double MinTextHeight;

	public double MinTextWidth;

	public double MaxTextHeight;

	public double MaxTextWidth;

	public bool EnableManualThd;

	public int TxtBinaryThdMin;

	public int TxtBinaryThdMax;

	public bool EnableAISimilarityCheck;

	public int AISimilarityScore;

	public string OnnxName;

	public bool UseOCR;

	public string OCRText;

	public int OCRImageChannel;

	public bool OCRTextisWhite;

	public bool UsePublicDB;

	public bool QuickSetupFinish;

	public int QuickSetupIndex;

	public string QuickSetupName;

	public bool AlignToMatchText;

	public FLOW_BIN ErrorBin;
}
