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
using System.Windows;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("QuadS")]
    public class QuadShape3D : Geometry3DGeometryShape3DBase
    {
        public QuadShape3D()
        {
            this.BackMaterial = this.Material;
        }
        public Point3D Point1 { get; set; } = new Point3D(0, 0, 0);
        public Point3D Point2 { get; set; } = new Point3D(1, 0, 0);
        public Point3D Point3 { get; set; } = new Point3D(1, 1, 0);
        public Point3D Point4 { get; set; } = new Point3D(0, 1, 0);
        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder builder = new(false, true);
            builder.AddQuad(
                this.Point1,
                this.Point2,
                this.Point3,
                this.Point4,
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(0, 0));
            return builder.ToMesh();
        }
    }
}