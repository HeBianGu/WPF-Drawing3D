using H.Drawing3D.Drawing.Base;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawing
{
    public class GeometryDrawing3D : Drawing3DBase<GeometryModel3D>, IGeometryDrawing3D
    {
        public GeometryDrawing3D(GeometryModel3D model) : base(model)
        {
        }

        public void DrawGeometry3D(Geometry3D geometry3D, Material material = null, Material backMaterial = null)
        {
            this.Model.Geometry = geometry3D;
            this.Model.Material = material;
            this.Model.BackMaterial = backMaterial;
        }
    }
}