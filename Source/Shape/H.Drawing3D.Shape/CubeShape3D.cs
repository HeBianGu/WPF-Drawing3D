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
    [DisplayName("立方体")]
    public class CubeShape3D : Geometry3DGeometryShape3DBase
    {
        public Point3D Center { get; set; } = new Point3D();

        public double SideLength { get; set; } = 1.0;

        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder b = new(false, true);
            b.AddCubeFace(
                this.Center,
                new Vector3D(-1, 0, 0),
                new Vector3D(0, 0, 1),
                this.SideLength,
                this.SideLength,
                this.SideLength);
            b.AddCubeFace(
                this.Center,
                new Vector3D(1, 0, 0),
                new Vector3D(0, 0, -1),
                this.SideLength,
                this.SideLength,
                this.SideLength);
            b.AddCubeFace(
                this.Center,
                new Vector3D(0, -1, 0),
                new Vector3D(0, 0, 1),
                this.SideLength,
                this.SideLength,
                this.SideLength);
            b.AddCubeFace(
                this.Center,
                new Vector3D(0, 1, 0),
                new Vector3D(0, 0, -1),
                this.SideLength,
                this.SideLength,
                this.SideLength);
            b.AddCubeFace(
                this.Center,
                new Vector3D(0, 0, 1),
                new Vector3D(0, -1, 0),
                this.SideLength,
                this.SideLength,
                this.SideLength);
            b.AddCubeFace(
                this.Center,
                new Vector3D(0, 0, -1),
                new Vector3D(0, 1, 0),
                this.SideLength,
                this.SideLength,
                this.SideLength);

            return b.ToMesh();
        }
    }
}