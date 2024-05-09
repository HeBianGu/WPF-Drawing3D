using H.Drawing3D.Drawing;
using H.Drawing3D.Shape;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.View;
using System.Windows.Controls;

namespace H.Test.Draw3D
{
    public class Presenter2DCubeShape3D : CubeShape3D, IPresenter2DShape3D
    {
        public object GetPresenter2D()
        {
            return this;
        }

        public void UpdatePresenter2D(IPresenter2DView3D view)
        {
            object p = this.GetPresenter2D();
            ContentPresenter contentPresenter = view.GetContentPresenter(p);
            System.Windows.Point p2d = view.ToView(this.ShapeObject.Bounds.Location);
            Canvas.SetLeft(contentPresenter, p2d.X);
            Canvas.SetTop(contentPresenter, p2d.Y);
        }
    }
}
