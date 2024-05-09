using H.Drawing3D.Drawing.Base;
using H.Drawing3D.Shape.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;

namespace H.Drawing3D.Shape
{
    public interface IShapeGroupShape3D
    {
        ObservableCollection<IShape3D> Shapes { get; }
    }

    [DisplayName("形状组")]
    [ContentProperty("Shapes")]
    public class ShapeGroupShape3D : GroupShape3DBase, IShapeGroupShape3D
    {
        public ObservableCollection<IShape3D> Shapes { get; } = new ObservableCollection<IShape3D>();

        public override void Draw(IGroupDrawing3D drawing)
        {
            foreach (IShape3D shape in this.Shapes)
            {
                shape.Drawing();
                this.Model3D.Children.Add(shape.ShapeObject);
            }
        }
    }
}