using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Extension;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("测试Mesh")]
    public class TestMeshGeometryShape3D : MeshGeometryShape3DBase
    {
        public override void Draw(IMeshGeometryDrawing3D drawing)
        {
            //drawing.DrawArrow(new Point3D(0, 0, 0), new Point3D(0, 0, 8));
            //drawing.DrawArrow(new Point3D(0, 0, 0), new Point3D(8, 0, 0));
            //drawing.DrawArrow(new Point3D(0, 0, 0), new Point3D(0, 8, 0));
            //drawing.DrawArrow(new Point3D(0, 0, 0), new Point3D(0, 0, -8));
            //drawing.DrawArrow(new Point3D(0, 0, 0), new Point3D(-8, 0, 0));
            //drawing.DrawArrow(new Point3D(0, 0, 0), new Point3D(0, -8, 0));

            //drawing.DrawCylinder(new Point3D(0, 0, -3), new Point3D(0, 0, 3));
            drawing.DrawDodecahedron(new Point3D(0, 0, 0), new Vector3D(0, 0, 3), new Vector3D(0, 3, 0), 1);

        }
    }
}