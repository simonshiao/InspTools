namespace SAMViewer;

internal class MaskPromotion
{
	private float[,] mMask { get; set; }

	public int mWidth { get; set; }

	public int mHeight { get; set; }

	public MaskPromotion(int wid, int hei)
	{
		mWidth = wid;
		mHeight = hei;
		mMask = new float[mWidth, mHeight];
	}
}
