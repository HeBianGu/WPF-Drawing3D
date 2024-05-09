using H.Drawing3D.Shape.Base;
using H.Drawing3D.ShapeLayer.Layer;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Markup;

namespace H.Drawing3D.ShapeLayer.ShapeLayer
{
    [ContentProperty("Shapes")]
    public abstract class ShapesLayerBase : ShapeLayerBase, IShapeLayer
    {
        public ShapesLayerBase()
        {
            this.Shapes = new ObservableCollection<IShape3D>();
        }
        public ObservableCollection<IShape3D> Shapes
        {
            get => (ObservableCollection<IShape3D>)this.GetValue(ShapesProperty);
            set => this.SetValue(ShapesProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShapesProperty =
            DependencyProperty.Register("Shapes", typeof(ObservableCollection<IShape3D>), typeof(ShapesLayerBase), new FrameworkPropertyMetadata(new ObservableCollection<IShape3D>(), (d, e) =>
            {
                if (d is not ShapesLayerBase control)
                {
                    return;
                }

                if (e.OldValue is ObservableCollection<IShape3D> o)
                {

                }

                if (e.NewValue is ObservableCollection<IShape3D> n)
                {

                }

            }));

    }
}