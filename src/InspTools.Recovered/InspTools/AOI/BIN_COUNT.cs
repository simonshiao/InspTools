namespace InspTools.AOI;

internal class BIN_COUNT
{
	private const int Maxbin = 6;

	public int[] Count;

	public BIN_COUNT()
	{
		Count = new int[6];
		Reset();
	}

	public void Reset()
	{
		for (int i = 0; i < 6; i++)
		{
			Count[i] = 0;
		}
	}
}
