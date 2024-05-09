using H.Drawing3D.Drawing;
using H.Drawing3D.Shape.Base;
using System.Linq;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.ShapeLayer.Layer
{
    public abstract class ShapeLayerBase : LayerBase
    {
        public void AddShape(IShape3D shape)
        {
            ModelVisual3D visual3D = new()
            {
                Content = shape.ShapeObject
            };
            if (shape is IPresenter2DShape3D presenterShape && this.View is IPresenter2DView3D presenter2DView3D)
            {
                object presenter = presenterShape.GetPresenter2D();
                if (presenter != null)
                {
                    presenter2DView3D.Add(presenter);
                }
            }
            this.Children.Add(visual3D);
        }

        public void RemoveShape(IShape3D shape)
        {
            System.Collections.Generic.List<ModelVisual3D> vs = this.Children.OfType<ModelVisual3D>().Where(x => x.Content == shape.ShapeObject).ToList();
            foreach (ModelVisual3D item in vs)
            {
                _ = this.Children.Remove(item);
            }

            if (shape is IPresenter2DShape3D presenterShape && this.View is IPresenter2DView3D presenter2DView3D)
            {
                object presenter = presenterShape.GetPresenter2D();
                if (presenter != null)
                {
                    presenter2DView3D.Delete(presenter);
                }
            }
        }
    }
}