// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector3DExtensions.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Extension methods for Vector3D.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Extension;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.ExtrudeText
{
    [DisplayName("立体文本")]
    public class ExtrudeTextShape3D : TextShape3DBase
    {
        public ExtrudeTextShape3D()
        {
            this.Text = this.Name;
            this.BackMaterial = this.Material;
        }
        public Point3D Point1 { get; set; } = new Point3D(0, 0, 1);

        [System.Obsolete]
        public override void Draw(IMeshGeometryDrawing3D drawing)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                return;
            }

            drawing.DrawExtrudeText(this.Text, this.FontFamily?.ToString() ?? "Arial", this.FontStyle, this.FontWeight, this.FontSize, this.TextDirection, this.Position, this.Point1);
        }

    }
}