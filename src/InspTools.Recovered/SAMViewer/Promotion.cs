using System.Windows.Controls;

namespace SAMViewer;

public abstract class Promotion
{
	public UserControl mAnation;

	public PromotionType mType;

	public abstract float[] GetInput();

	public abstract float[] GetLable();
}
