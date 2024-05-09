// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CubeVisual3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   A visual element that displays a cube.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Extension;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("边界框")]
    public class BoundingBoxShape3D : MeshGeometryShape3DBase
    {
        public Rect3D BoundingBox { get; set; } = new Rect3D();
        public double Diameter { get; set; } = 0.1;

        public override void Draw(IMeshGeometryDrawing3D drawing)
        {
            if (this.BoundingBox.IsEmpty)
            {
                return;
            }

            Rect3D bb = this.BoundingBox;
            Point3D p0 = new(bb.X, bb.Y, bb.Z);
            Point3D p1 = new(bb.X, bb.Y + bb.SizeY, bb.Z);
            Point3D p2 = new(bb.X + bb.SizeX, bb.Y + bb.SizeY, bb.Z);
            Point3D p3 = new(bb.X + bb.SizeX, bb.Y, bb.Z);
            Point3D p4 = new(bb.X, bb.Y, bb.Z + bb.SizeZ);
            Point3D p5 = new(bb.X, bb.Y + bb.SizeY, bb.Z + bb.SizeZ);
            Point3D p6 = new(bb.X + bb.SizeX, bb.Y + bb.SizeY, bb.Z + bb.SizeZ);
            Point3D p7 = new(bb.X + bb.SizeX, bb.Y, bb.Z + bb.SizeZ);

            double r = this.Diameter / 2;
            int t = 10;
            drawing.DrawCylinder(p0, p1, r, t);
            drawing.DrawCylinder(p1, p2, r, t);
            drawing.DrawCylinder(p2, p3, r, t);
            drawing.DrawCylinder(p3, p0, r, t);

            drawing.DrawCylinder(p4, p5, r, t);
            drawing.DrawCylinder(p5, p6, r, t);
            drawing.DrawCylinder(p6, p7, r, t);
            drawing.DrawCylinder(p7, p4, r, t);

            drawing.DrawCylinder(p0, p4, r, t);
            drawing.DrawCylinder(p1, p5, r, t);
            drawing.DrawCylinder(p2, p6, r, t);
            drawing.DrawCylinder(p3, p7, r, t);
        }

        //private void AddEdge(Point3D p1, Point3D p2)
        //{
        //    var fv = new PipeVisual3D();
        //    fv.BeginEdit();
        //    fv.Diameter = this.Diameter;
        //    fv.ThetaDiv = 10;
        //    fv.Fill = this.Fill;
        //    fv.Point1 = p1;
        //    fv.Point2 = p2;
        //    fv.EndEdit();
        //    this.Children.Add(fv);
        //}
    }
}