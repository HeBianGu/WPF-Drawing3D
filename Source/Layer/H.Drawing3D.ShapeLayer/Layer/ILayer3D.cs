using H.Drawing3D.Drawing;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.ShapeLayer.Layer
{
    public interface ILayer3D
    {
        void InitView(ICameraViewport3D view);
        ModelVisual3D ModelVisual3D { get; }
        void Drawing();
        void Clear();
    }

    public static class Layer3DExtension
    {
        public static void RefreshDrawing(this ILayer3D layer)
        {
            layer.Clear();
            layer.Drawing();
        }
    }

}