// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CubeVisual3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   A visual element that displays a cube.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Drawing.Base;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Extension;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{
    [DisplayName("坐标系")]
    public class CoordinateSystemShape3D : GroupShape3DBase
    {
        public double ArrowLengths { get; set; } = 1.0;
        public Brush XAxisBrush { get; set; } = Brushes.Blue;
        public Brush YAxisBrush { get; set; } = Brushes.Red;
        public Brush ZAxisBrush { get; set; } = Brushes.Orange;


        public override void Draw(IGroupDrawing3D drawing)
        {
            double l = this.ArrowLengths;
            double d = l * 0.1;
            drawing.DrawArrow(new Point3D(), new Point3D(l, 0, 0), this.XAxisBrush.ToMaterial(), d);
            drawing.DrawArrow(new Point3D(), new Point3D(0, l, 0), this.YAxisBrush.ToMaterial(), d);
            drawing.DrawArrow(new Point3D(), new Point3D(0, 0, l), this.ZAxisBrush.ToMaterial(), d);
        }
    }
}