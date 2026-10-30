namespace InspTools.Other;

public class ParetoChartData
{
	public string Name { get; set; }

	public int Value { get; set; }

	public ParetoChartData(string name, int value)
	{
		Name = name;
		Value = value;
	}
}
