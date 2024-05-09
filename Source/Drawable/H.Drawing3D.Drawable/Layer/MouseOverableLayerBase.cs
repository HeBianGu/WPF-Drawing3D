using H.Drawing3D.ShapeLayer.Layer;
using System.Windows.Input;

namespace H.Drawing3D.Drawable.Layer
{
    public abstract class MouseOverableLayerBase : ShapeLayerBase, IMouseLayer3D
    {
        public virtual void OnMouseDown(MouseButtonEventArgs e)
        {

        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {

        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {

        }

        public virtual void OnMouseUp(MouseButtonEventArgs e)
        {

        }
    }
}