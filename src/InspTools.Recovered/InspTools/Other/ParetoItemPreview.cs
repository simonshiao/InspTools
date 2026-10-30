namespace InspTools.Other;

public class ParetoItemPreview
{
	public string ToolsName { get; set; }

	public int ItemNo { get; set; }

	public string ItemName { get; set; }

	public ParetoItemPreview(string name, int no, string item)
	{
		ToolsName = name;
		ItemNo = no;
		ItemName = item;
	}
}
