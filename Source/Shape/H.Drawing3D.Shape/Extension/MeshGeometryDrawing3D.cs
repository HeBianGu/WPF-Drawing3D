using H.Drawing3D.Drawing;
using H.Drawing3D.Shape.Geometry;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Extension
{
    public class MeshGeometryDrawing3D : GeometryDrawing3D, IMeshGeometryDrawing3D
    {
        private readonly MeshBuilder _builder = new(false, true);
        public MeshGeometryDrawing3D(GeometryModel3D model) : base(model)
        {

        }
        public IMeshBuilder MeshBuilder => this._builder;
        public void PopMeshGeometry3D(Material material, Material bm = null)
        {
            this.Model.Geometry = this.ToMesh();
            this.Model.Material = material;
            this.Model.BackMaterial = bm;
        }


    }
}