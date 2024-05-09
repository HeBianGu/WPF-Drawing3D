using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{

    public abstract class SelectableShape3DBase : MouseOverableShape3DBase, ISelectableShape3D
    {
        public bool UseSelectable { get; set; } = true;
        public virtual Material SelectedMaterial { get; set; } = ShapeMaterials.Select;
        public virtual void UpdateSelect()
        {
            this.Model3D.Material = this.SelectedMaterial;
        }
    }
}