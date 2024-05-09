using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{
    public interface IShape3D
    {
        Model3D ShapeObject { get; }
        void Drawing();
        void CameraUpdate(ProjectionCamera vierw);
        void UpdateDefault();
    }
}