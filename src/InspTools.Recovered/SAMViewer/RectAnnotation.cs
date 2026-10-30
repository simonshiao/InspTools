using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SAMViewer;

public class RectAnnotation : UserControl
{
	private readonly Shape _shape;

	private readonly TextBlock _textBlock;

	public Point StartPosition
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

	public Point LeftUP { get; set; }

	public Point RightBottom { get; set; }

	public new double Width
	{
		get
		{
			return _shape.Width;
		}
		set
		{
			_shape.Width = value;
		}
	}

	public new double Height
	{
		get
		{
			return _shape.Height;
		}
		set
		{
			_shape.Height = value;
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
	public RectAnnotation()
	{
		_shape = new Rectangle
		{
			Width = 50.0,
			Height = 50.0,
			Fill = Brushes.Transparent,
			Stroke = Brushes.Blue,
			StrokeThickness = 1.0
		};
		_textBlock = new TextBlock
		{
			Text = "Rect",
			Foreground = Brushes.Blue,
			FontSize = 10.0,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		base.Content = new Grid
		{
			Children = { (UIElement)_shape }
		};
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
	}
}
