using H.Drawing3D.Drawing;
using H.Drawing3D.Drawing.Base;

namespace H.Drawing3D.Shape.Base
{
    public abstract class GeometryShape3DBase : SelectableShape3DBase
    {
        public override void Drawing()
        {
            IGeometryDrawing3D drawing = new GeometryDrawing3D(this.Model3D);
            this.Draw(drawing);
            this.UpdateDefault();
        }

        public abstract void Draw(IGeometryDrawing3D drawing);

        public override void UpdateDefault()
        {
            base.UpdateDefault();
            this.Model3D.Material = this.Material;
            this.Model3D.BackMaterial = this.BackMaterial;
        }
    }

}