using H.Drawing3D.Drawing.Base;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawing
{
    public class GroupDrawing3D : Drawing3DBase<Model3DGroup>, IGroupDrawing3D
    {
        public GroupDrawing3D(Model3DGroup model) : base(model)
        {
        }
        public void DrawGeometry3D(Geometry3D geometry3D, Material material)
        {
            GeometryModel3D geometryModel = new(geometry3D, material);
            this.Model.Children.Add(geometryModel);
        }
    }
}