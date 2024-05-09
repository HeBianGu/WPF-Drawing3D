using H.Drawing3D.Drawing;
using H.Drawing3D.Drawing.Base;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{
    public abstract class GroupShape3DBase : Shape3DBase<Model3DGroup>
    {
        public override void Drawing()
        {
            IGroupDrawing3D drawing = new GroupDrawing3D(this.Model3D);
            this.Draw(drawing);
            this.UpdateDefault();
        }

        public abstract void Draw(IGroupDrawing3D drawing);
    }
}