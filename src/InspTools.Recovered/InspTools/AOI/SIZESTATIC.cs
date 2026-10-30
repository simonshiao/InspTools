namespace InspTools.AOI;

public class SIZESTATIC
{
	public int ID;

	public double LocateWidth;

	public double LocateHeight;

	public double LocateAngle;

	public double AIProphetScore;

	public int LocateCenterX;

	public int LocateCenterY;

	public double[] AdvAWidth = new double[20];

	public double[] AdvAHeight = new double[20];

	public double[] AdvM = new double[40];

	public double[] MeasureH = new double[20];

	public double[] MeasureW = new double[20];

	public SIZESTATIC()
	{
		ID = 0;
		LocateWidth = 0.0;
		LocateHeight = 0.0;
		LocateAngle = 0.0;
		AIProphetScore = 0.0;
		for (int i = 0; i < 20; i++)
		{
			AdvAWidth[i] = 0.0;
			AdvAHeight[i] = 0.0;
			MeasureH[i] = 0.0;
			MeasureW[i] = 0.0;
		}
		for (int j = 0; j < 40; j++)
		{
			AdvM[j] = 0.0;
		}
	}
}
