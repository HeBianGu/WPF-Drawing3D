// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CubeVisual3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   A visual element that displays a cube.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.ExtensionMethods;
using H.Drawing3D.Shape.Geometry;
using System;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("网格线")]
    public class GridLinesShape3D : Geometry3DGeometryShape3DBase
    {
        public GridLinesShape3D()
        {
            this.UseSelectable = false;
            this.UseMouseOverable = false;
            this.BackMaterial = this.Material;
        }
        public Point3D Center { get; set; } = new Point3D();
        public double MinorDistance { get; set; } = 2.5;
        public double Length { get; set; } = 200.0;
        public double MajorDistance { get; set; } = 10.0;
        public double Thickness { get; set; } = 0.08;
        public double Width { get; set; } = 200.0;
        public Vector3D LengthDirection { get; set; } = new Vector3D(1, 0, 0);
        public Vector3D Normal { get; set; } = new Vector3D(0, 0, 1);

        protected override Geometry3D Geometry3D => this.Tessellate();

        /// <summary>
        /// The length direction.
        /// </summary>
        private Vector3D lengthDirection;

        /// <summary>
        /// The width direction.
        /// </summary>
        private Vector3D widthDirection;
        /// <summary>
        /// Do the tessellation and return the <see cref="MeshGeometry3D" />.
        /// </summary>
        /// <returns>
        /// A triangular mesh geometry.
        /// </returns>
        protected MeshGeometry3D Tessellate()
        {
            this.lengthDirection = this.LengthDirection;
            this.lengthDirection.Normalize();

            // #136, chrkon, 2015-03-26
            // if NormalVector and LenghtDirection are not perpendicular then overwrite LengthDirection
            if (Vector3D.DotProduct(this.Normal, this.LengthDirection) != 0.0)
            {
                this.lengthDirection = this.Normal.FindAnyPerpendicular();
                this.lengthDirection.Normalize();
            }

            // create WidthDirection by rotating lengthDirection vector 90° around normal vector
            RotateTransform3D rotate = new(new AxisAngleRotation3D(this.Normal, 90.0));
            this.widthDirection = rotate.Transform(this.lengthDirection);
            this.widthDirection.Normalize();
            // #136 

            MeshBuilder mesh = new(true, false);
            double minX = -this.Width / 2;
            double minY = -this.Length / 2;
            double maxX = this.Width / 2;
            double maxY = this.Length / 2;

            double x = minX;
            double eps = this.MinorDistance / 10;
            while (x <= maxX + eps)
            {
                double t = this.Thickness;
                if (IsMultipleOf(x, this.MajorDistance))
                {
                    t *= 2;
                }

                this.AddLineX(mesh, x, minY, maxY, t);
                x += this.MinorDistance;
            }

            double y = minY;
            while (y <= maxY + eps)
            {
                double t = this.Thickness;
                if (IsMultipleOf(y, this.MajorDistance))
                {
                    t *= 2;
                }

                this.AddLineY(mesh, y, minX, maxX, t);
                y += this.MinorDistance;
            }

            MeshGeometry3D m = mesh.ToMesh();
            m.Freeze();
            return m;
        }

        /// <summary>
        /// Determines whether y is a multiple of d.
        /// </summary>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <returns>
        /// The is multiple of.
        /// </returns>
        private static bool IsMultipleOf(double y, double d)
        {
            double y2 = d * (int)(y / d);
            return Math.Abs(y - y2) < 1e-3;
        }

        /// <summary>
        /// The add line x.
        /// </summary>
        /// <param name="mesh">
        /// The mesh.
        /// </param>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="minY">
        /// The min y.
        /// </param>
        /// <param name="maxY">
        /// The max y.
        /// </param>
        /// <param name="thickness">
        /// The thickness.
        /// </param>
        private void AddLineX(MeshBuilder mesh, double x, double minY, double maxY, double thickness)
        {
            int i0 = mesh.Positions.Count;
            mesh.Positions.Add(this.GetPoint(x - (thickness / 2), minY));
            mesh.Positions.Add(this.GetPoint(x - (thickness / 2), maxY));
            mesh.Positions.Add(this.GetPoint(x + (thickness / 2), maxY));
            mesh.Positions.Add(this.GetPoint(x + (thickness / 2), minY));
            mesh.Normals.Add(this.Normal);
            mesh.Normals.Add(this.Normal);
            mesh.Normals.Add(this.Normal);
            mesh.Normals.Add(this.Normal);
            mesh.TriangleIndices.Add(i0);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 3);
            mesh.TriangleIndices.Add(i0);
        }

        /// <summary>
        /// The add line y.
        /// </summary>
        /// <param name="mesh">
        /// The mesh.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <param name="minX">
        /// The min x.
        /// </param>
        /// <param name="maxX">
        /// The max x.
        /// </param>
        /// <param name="thickness">
        /// The thickness.
        /// </param>
        private void AddLineY(MeshBuilder mesh, double y, double minX, double maxX, double thickness)
        {
            int i0 = mesh.Positions.Count;
            mesh.Positions.Add(this.GetPoint(minX, y + (thickness / 2)));
            mesh.Positions.Add(this.GetPoint(maxX, y + (thickness / 2)));
            mesh.Positions.Add(this.GetPoint(maxX, y - (thickness / 2)));
            mesh.Positions.Add(this.GetPoint(minX, y - (thickness / 2)));
            mesh.Normals.Add(this.Normal);
            mesh.Normals.Add(this.Normal);
            mesh.Normals.Add(this.Normal);
            mesh.Normals.Add(this.Normal);
            mesh.TriangleIndices.Add(i0);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 3);
            mesh.TriangleIndices.Add(i0);
        }

        /// <summary>
        /// Gets a point on the plane.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>A <see cref="Point3D"/>.</returns>
        private Point3D GetPoint(double x, double y)
        {
            return this.Center + (this.widthDirection * x) + (this.lengthDirection * y);
        }
    }

}