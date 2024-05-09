using System.Windows.Input;

namespace H.Drawing3D.ShapeLayer.Layer
{
    public interface IMouseLayer3D
    {
        public void OnMouseDown(MouseButtonEventArgs e);
        public void OnMouseMove(MouseEventArgs e);
        public void OnMouseUp(MouseButtonEventArgs e);
        public void OnMouseLeave(MouseEventArgs e);
    }
}