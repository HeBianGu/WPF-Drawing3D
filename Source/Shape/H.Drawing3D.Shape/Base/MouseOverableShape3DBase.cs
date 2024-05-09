using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{
    public abstract class MouseOverableShape3DBase : Shape3DBase<GeometryModel3D>, IMouseOverableShape3D
    {
        public bool UseMouseOverable { get; set; } = true;
        public virtual Material MouseOverMaterial { get; set; } = ShapeMaterials.MouseOver;
        public virtual void UpdateMouseOver()
        {
            this.Model3D.Material = this.MouseOverMaterial;
        }
    }
}