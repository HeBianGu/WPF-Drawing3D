using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawing.Base
{
    public interface IGroupDrawing3D : IDrawing3D
    {
        void DrawGeometry3D(Geometry3D geometry3D, Material material);
    }
}