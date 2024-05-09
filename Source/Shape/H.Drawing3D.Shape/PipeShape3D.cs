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
    [DisplayName("管道")]
    public class PipeShape3D : Geometry3DGeometryShape3DBase
    {
        public double Diameter { get; set; } = 1.0;
        public double InnerDiameter { get; set; } = 0.5;
        public Point3D Point1 { get; set; } = new Point3D(0, 0, 0);
        public Point3D Point2 { get; set; } = new Point3D(0, 0, 10);
        public int ThetaDiv { get; set; } = 36;
        protected override Geometry3D Geometry3D => this.Tessellate();

        protected MeshGeometry3D Tessellate()
        {
            MeshBuilder builder = new(false, true);
            builder.AddPipe(this.Point1, this.Point2, this.InnerDiameter, this.Diameter, this.ThetaDiv);
            return builder.ToMesh();
        }
    }

}