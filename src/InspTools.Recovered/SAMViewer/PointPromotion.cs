namespace SAMViewer;

public class PointPromotion : Promotion
{
	public OpType m_Optype;

	public int X { get; set; }

	public int Y { get; set; }

	public PointPromotion(OpType optype)
	{
		mType = PromotionType.Point;
		m_Optype = optype;
	}

	public override float[] GetInput()
	{
		return new float[2] { X, Y };
	}

	public override float[] GetLable()
	{
		if (m_Optype == OpType.ADD)
		{
			return new float[1] { 1f };
		}
		return new float[1];
	}
}
