using H.Drawing3D.Shape.Base;
using System.Collections.ObjectModel;

namespace H.Drawing3D.ShapeLayer.ShapeLayer
{
    public interface IShapeLayer
    {
        ObservableCollection<IShape3D> Shapes { get; set; }
    }

}