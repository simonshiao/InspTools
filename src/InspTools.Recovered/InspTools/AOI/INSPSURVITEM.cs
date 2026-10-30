namespace InspTools.AOI;

public class INSPSURVITEM
{
	public int ID { get; set; }

	public int Station { get; set; }

	public string StationStr { get; set; }

	public int InspToolID { get; set; }

	public string InspToolName { get; set; }

	public int TrackID { get; set; }

	public string TrackName { get; set; }

	public string StatusStr { get; set; }

	public bool Status { get; set; }

	public double AlarmRatio { get; set; }
}
