using H.Drawing3D.Shape.Base;
using System.Collections.ObjectModel;

namespace H.Drawing3D.View
{
    public interface IShapesView3D
    {
        ObservableCollection<IShape3D> Shapes { get; set; }
    }
}