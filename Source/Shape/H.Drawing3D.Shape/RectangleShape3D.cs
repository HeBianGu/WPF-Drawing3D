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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("长方形")]
    public class RectangleShape3D : Geometry3DGeometryShape3DBase
    {
        public RectangleShape3D()
        {
            this.BackMaterial = this.Material;
        }
        public Point3D Origin { get; set; } = new Point3D(0, 0, 0);
        public int DivWidth { get; set; } = 10;
        public double Width { get; set; } = 10.0;
        public double Length { get; set; } = 10.0;
        public Vector3D LengthDirection { get; set; } = new Vector3D(1, 0, 0);
        public Vector3D Normal { get; set; } = new Vector3D(0, 0, 1);
        public int DivLength { get; set; } = 10;

        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            Vector3D u = this.LengthDirection;
            Vector3D w = this.Normal;
            Vector3D v = Vector3D.CrossProduct(w, u);
            u = Vector3D.CrossProduct(v, w);

            u.Normalize();
            v.Normalize();
            w.Normalize();

            double le = this.Length;
            double wi = this.Width;

            List<Point3D> pts = new();
            for (int i = 0; i < this.DivLength; i++)
            {
                double fi = -0.5 + ((double)i / (this.DivLength - 1));
                for (int j = 0; j < this.DivWidth; j++)
                {
                    double fj = -0.5 + ((double)j / (this.DivWidth - 1));
                    pts.Add(this.Origin + (u * le * fi) + (v * wi * fj));
                }
            }

            MeshBuilder builder = new(false, true);
            builder.AddRectangularMesh(pts, this.DivWidth);

            return builder.ToMesh();
        }
    }


}