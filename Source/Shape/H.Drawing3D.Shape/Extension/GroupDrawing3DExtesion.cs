using H.Drawing3D.Drawing.Base;
using H.Drawing3D.Shape.Geometry;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Extension
{
    public static class GroupDrawing3DExtension
    {
        public static void DrawArrow(this IGroupDrawing3D groupDrawing, Point3D point1, Point3D point2, Material material, double diameter = 1, double headLength = 3, int ThetaDiv = 36)
        {
            if (diameter <= 0)
            {
                return;
            }

            MeshBuilder builder = new(true, true);
            builder.AddArrow(point1, point2, diameter, headLength, ThetaDiv);
            MeshGeometry3D geo = builder.ToMesh();
            groupDrawing.DrawGeometry3D(geo, material);
        }
    }
}