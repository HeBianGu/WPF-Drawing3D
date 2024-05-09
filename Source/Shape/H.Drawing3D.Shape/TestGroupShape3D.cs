using H.Drawing3D.Drawing.Base;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Extension;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("测试组")]
    public class TestGroupShape3D : GroupShape3DBase
    {
        public override void Draw(IGroupDrawing3D drawing)
        {
            drawing.DrawArrow(new Point3D(), new Point3D(0, 0, 10), this.Material);
            drawing.DrawArrow(new Point3D(), new Point3D(10, 0, 0), this.Material);
            drawing.DrawArrow(new Point3D(), new Point3D(0, 10, 0), this.Material);
        }
    }
}