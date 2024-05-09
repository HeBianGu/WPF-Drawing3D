using H.Drawing3D.Shape.Extension;

namespace H.Drawing3D.Shape.Base
{
    public abstract class MeshGeometryShape3DBase : SelectableShape3DBase
    {
        public override void Drawing()
        {
            MeshGeometryDrawing3D drawing = new(this.Model3D);
            this.Draw(drawing);
            drawing.PopMeshGeometry3D(this.Material, this.BackMaterial);
        }

        public abstract void Draw(IMeshGeometryDrawing3D drawing);
    }
}