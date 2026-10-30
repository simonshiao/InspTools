using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SAMViewer;

public class PointAnnotation : UserControl
{
	private const int DefaultSize = 10;

	private readonly Shape _shape;

	private readonly TextBlock _textBlock;

	public SolidColorBrush m_Brush = Brushes.Red;

	public Point Position
	{
		get
		{
			return new Point(Canvas.GetLeft(this) + _shape.Width / 2.0, Canvas.GetTop(this) + _shape.Height / 2.0);
		}
		set
		{
			Canvas.SetLeft(this, value.X - _shape.Width / 2.0);
			Canvas.SetTop(this, value.Y - _shape.Height / 2.0);
		}
	}

	public string Text
	{
		get
		{
			return _textBlock.Text;
		}
		set
		{
			_textBlock.Text = value;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public PointAnnotation(SolidColorBrush brush)
	{
		m_Brush = brush;
		_shape = new Ellipse
		{
			Width = 10.0,
			Height = 10.0,
			Fill = brush,
			Stroke = Brushes.Black,
			StrokeThickness = 1.0
		};
		_textBlock = new TextBlock
		{
			Text = "Point",
			Foreground = Brushes.Black,
			FontSize = 10.0,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		base.Content = new Grid
		{
			Children = { (UIElement)_shape }
		};
	}
}
