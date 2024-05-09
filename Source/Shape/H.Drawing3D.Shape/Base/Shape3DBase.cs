using System.ComponentModel;
using System.Reflection;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{

    public abstract class Shape3DBase<T> : IShape3D where T : Model3D, new()
    {
        public T Model3D { get; set; } = new T();

        public Shape3DBase()
        {
            this.Name = this.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
        }
        public string Name { get; set; }
        public Model3D ShapeObject => this.Model3D;
        public virtual Material Material { get; set; } = ShapeMaterials.Default;
        public virtual Material BackMaterial { get; set; }
        public abstract void Drawing();
        public virtual void UpdateDefault()
        {

        }
        public virtual void CameraUpdate(ProjectionCamera vierw)
        {

        }
    }

}