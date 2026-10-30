namespace InspTools.Other;

public class ParetoSatisicsItem
{
	public int[] Value = new int[8];

	public string Name { get; set; }

	public string Station1 { get; set; }

	public string Station2 { get; set; }

	public string Station3 { get; set; }

	public string Station4 { get; set; }

	public string Station5 { get; set; }

	public string Station6 { get; set; }

	public string Station7 { get; set; }

	public string Station8 { get; set; }

	public ParetoSatisicsItem(string name, int[] value, string[] summary)
	{
		Name = name;
		for (int i = 0; i < 8; i++)
		{
			Value[i] = value[i];
		}
		Station1 = summary[0];
		Station2 = summary[1];
		Station3 = summary[2];
		Station4 = summary[3];
		Station5 = summary[4];
		Station6 = summary[5];
		Station7 = summary[6];
		Station8 = summary[7];
	}
}
