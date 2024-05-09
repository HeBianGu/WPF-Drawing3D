using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Geometry;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("圆环")]
    public class TorusShape3D : Geometry3DGeometryShape3DBase
    {
        public int ThetaDiv { get; set; } = 36;
        public int PhiDiv { get; set; } = 24;
        public double TubeDiameter { get; set; } = 1.0;
        public double TorusDiameter { get; set; } = 3.0;
        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder builder = new(false, true);
            builder.AddTorus(this.TorusDiameter, this.TubeDiameter, this.ThetaDiv, this.PhiDiv);
            return builder.ToMesh();
        }
    }
}