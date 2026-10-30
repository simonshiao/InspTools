namespace SAMViewer;

internal class BoxPromotion : Promotion
{
	public PointPromotion mLeftUp { get; set; }

	public PointPromotion mRightBottom { get; set; }

	public BoxPromotion()
	{
		mLeftUp = new PointPromotion(OpType.ADD);
		mRightBottom = new PointPromotion(OpType.ADD);
		mType = PromotionType.Box;
	}

	public override float[] GetInput()
	{
		return new float[4] { mLeftUp.X, mLeftUp.Y, mRightBottom.X, mRightBottom.Y };
	}

	public override float[] GetLable()
	{
		return new float[2] { 2f, 3f };
	}
}
