// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CubeVisual3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   A visual element that displays a cube.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Geometry;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("椭圆体")]
    public class EllipsoidShape3D : Geometry3DGeometryShape3DBase
    {

        public Point3D Center { get; set; } = new Point3D();
        public int ThetaDiv { get; set; } = 30;
        public double RadiusZ { get; set; } = 1.0;
        public double RadiusY { get; set; } = 1.0;
        public double RadiusX { get; set; } = 1.0;
        public int PhiDiv { get; set; } = 30;

        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder builder = new(false, true);
            builder.AddEllipsoid(this.Center, this.RadiusX, this.RadiusY, this.RadiusZ, this.ThetaDiv, this.PhiDiv);
            return builder.ToMesh();
        }
    }

}