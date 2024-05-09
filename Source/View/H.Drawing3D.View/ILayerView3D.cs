using H.Drawing3D.Shape.Base;
using H.Drawing3D.ShapeLayer.Layer;
using H.Drawing3D.ShapeLayer.ShapeLayer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace H.Drawing3D.View
{
    public interface ILayerView3D : IView3D
    {
        public ObservableCollection<ILayer3D> Layers { get; }
    }

    public static class LayerView3DExtension
    {
        public static IEnumerable<IShape3D> GetShapes(this ILayerView3D layerView)
        {
            return layerView.Layers.OfType<IShapeLayer>().SelectMany(x => x.Shapes);
        }
    }
}