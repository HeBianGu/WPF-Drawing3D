// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrowVisual3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   A visual element that shows an arrow.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Geometry;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("箭头")]
    public class ArrowShape3D : Geometry3DGeometryShape3DBase
    {
        public double Diameter { get; set; } = 1.0;
        public double HeadLength { get; set; } = 3.0;
        public Point3D Point1 { get; set; } = new Point3D();
        public Point3D Point2 { get; set; } = new Point3D(0, 0, 10);
        public int ThetaDiv { get; set; } = 36;
        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            if (this.Diameter <= 0)
            {
                return null;
            }

            MeshBuilder builder = new(true, true);
            builder.AddArrow(this.Point1, this.Point2, this.Diameter, this.HeadLength, this.ThetaDiv);
            return builder.ToMesh();
        }
    }
}