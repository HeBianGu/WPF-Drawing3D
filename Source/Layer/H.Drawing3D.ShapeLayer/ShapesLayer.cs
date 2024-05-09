using H.Drawing3D.Drawing;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.ShapeLayer.Layer;
using H.Drawing3D.ShapeLayer.ShapeLayer;
using System.Linq;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.ShapeLayer
{
    public class ShapesLayer : ShapesLayerBase, ICameraUpdateLayer
    {
        public override void Drawing()
        {
            foreach (IShape3D shape in this.Shapes)
            {
                shape.Drawing();
                if (shape is IPresenter2DShape3D presenterShape && this.View is IPresenter2DView3D presenter2DView3D)
                {
                    object presenter = presenterShape.GetPresenter2D();
                    if (presenter != null)
                    {
                        presenter2DView3D.Add(presenter);
                    }
                }
                ModelVisual3D m3d = new()
                {
                    Content = shape.ShapeObject
                };
                this.Children.Add(m3d);
            }
        }

        public void OnCameraUpdate(ICameraViewport3D vierw)
        {
            if (vierw is IPresenter2DView3D view)
            {
                foreach (IPresenter2DShape3D item in this.Shapes.OfType<IPresenter2DShape3D>())
                {
                    item.UpdatePresenter2D(view);
                }
            }
        }
    }
}