using H.Drawing3D.Drawing;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.ShapeLayer.Layer
{
    public abstract class LayerBase : ModelVisual3D, ILayer3D
    {
        public virtual ModelVisual3D ModelVisual3D => this;
        public ICameraViewport3D View { get; private set; }
        public abstract void Drawing();
        public virtual void Clear()
        {
            this.Children.Clear();
        }
        public void InitView(ICameraViewport3D view)
        {
            this.View = view;
        }
    }
}