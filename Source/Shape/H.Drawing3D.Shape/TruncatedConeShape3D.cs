using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Geometry;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{

    [DisplayName("圆锥")]
    public class TruncatedConeShape3D : Geometry3DGeometryShape3DBase
    {
        public bool TopCap { get; set; } = true;
        public bool BaseCap { get; set; } = true;

        public Vector3D Normal { get; set; } = new Vector3D(0, 0, 1);
        public double BaseRadius { get; set; } = 1.0;
        public double Height { get; set; } = 2.0;

        public Point3D Origin { get; set; } = new Point3D(0, 0, 0);
        public int ThetaDiv { get; set; } = 35;
        public double TopRadius { get; set; } = 0.0;
        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder builder = new(false, true);
            builder.AddCone(
                this.Origin,
                this.Normal,
                this.BaseRadius,
                this.TopRadius,
                this.Height,
                this.BaseCap,
                this.TopCap,
                this.ThetaDiv);
            return builder.ToMesh();
        }
    }

}