using H.Drawing3D.Drawing.Base;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{
    public abstract class Geometry3DGeometryShape3DBase : GeometryShape3DBase
    {
        protected virtual Geometry3D Geometry3D { get; }
        public override void Draw(IGeometryDrawing3D drawing)
        {
            drawing.DrawGeometry3D(this.Geometry3D, this.Material);
        }
    }
}