using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Geometry;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("球体")]
    public class SphereShape3D : Geometry3DGeometryShape3DBase
    {
        public Point3D Center { get; set; } = new Point3D(0, 0, 0);
        public int ThetaDiv { get; set; } = 20;
        public int PhiDiv { get; set; } = 20;
        public double Radius { get; set; } = 7;
        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder builder = new(true, true);
            builder.AddSphere(this.Center, this.Radius, this.ThetaDiv, this.PhiDiv);
            return builder.ToMesh();
        }
    }

    public class SSShape3D : Geometry3DGeometryShape3DBase
    {
        protected override Geometry3D Geometry3D
        {
            get
            {
                MeshGeometry3D mesh = new()
                {
                    Positions = new Point3DCollection(){ new Point3D(-1, 30, 1),
    new Point3D(1, 1, 1),
    new Point3D(-1, -1, 1),
    new Point3D(1, -1, 1),
    new Point3D(-1, 1, -1),
    new Point3D(1, 1, -1),
    new Point3D(-1, -1, -1),
    new Point3D(1, -1, -1),
},
                    TriangleIndices = new Int32Collection()
{

    0, 1, 2,
    1, 3, 2,
    1, 5, 3,
    5, 7, 3,
    5, 4, 7,
    4, 6, 7,
    4, 0, 6,
    0, 2, 6,
    2, 3, 6,
    3, 7, 6,
    4, 5, 0,
    5, 1, 0,
}
                };
                return mesh;
            }

        }
    }

}