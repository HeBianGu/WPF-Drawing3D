using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Extension;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("多线")]
    public class PolyLineShape3D : MeshGeometryShape3DBase
    {
        public double Diameter { get; set; } = 0.05;
        public int ThetaDiv { get; set; } = 10;
        public Point3DCollection Points { get; set; } = new Point3DCollection();

        public override void Draw(IMeshGeometryDrawing3D drawing)
        {
            drawing.DrawPolyLine(this.Diameter, this.ThetaDiv, this.Points.ToArray());
        }
    }
}