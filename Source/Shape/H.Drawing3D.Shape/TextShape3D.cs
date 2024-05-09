using H.Drawing3D.Shape.Base;
using H.Drawing3D.Shape.Extension;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape
{

    public abstract class TextShape3DBase : MeshGeometryShape3DBase
    {
        public TextShape3DBase()
        {
            this.Text = this.Name;
        }
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; } = 15;
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;
        public Brush Foreground { get; set; } = Brushes.Black;
        public Point3D Position { get; set; }
        public string Text { get; set; }
        public Vector3D TextDirection { get; set; } = new Vector3D(1, 0, 0);
    }

    [DisplayName("文本")]
    public class TextShape3D : TextShape3DBase
    {
        public bool IsFlipped { get; set; }
        public Brush Background { get; set; }
        public Brush BorderBrush { get; set; }
        public Thickness BorderThickness { get; set; } = new Thickness(1);
        public double Height { get; set; } = 11.0;
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;
        public bool IsDoubleSided { get; set; } = true;
        public Thickness Padding { get; set; }
        public Vector3D UpDirection { get; set; } = new Vector3D(0, 0, 1);
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
        public override void Draw(IMeshGeometryDrawing3D drawing)
        {
            Tuple<MeshGeometry3D, Material> tuple = this.GetTuple();
            if (tuple == null)
            {
                return;
            }

            drawing.DrawGeometry3D(tuple.Item1, tuple.Item2);
        }

        public override void UpdateDefault()
        {

        }

        private Tuple<MeshGeometry3D, Material> GetTuple()
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                return null;
            }

            // First we need a textblock containing the text of our label
            TextBlock textBlock = new(new Run(this.Text))
            {
                Foreground = this.Foreground,
                Background = this.Background,
                FontWeight = this.FontWeight,
                Padding = this.Padding
            };
            if (this.FontFamily != null)
            {
                textBlock.FontFamily = this.FontFamily;
            }
            if (this.FontSize > 0)
            {
                textBlock.FontSize = this.FontSize;
            }

            FrameworkElement element = this.BorderBrush != null
                              ?
                                new Border
                                {
                                    BorderBrush = this.BorderBrush,
                                    BorderThickness = this.BorderThickness,
                                    Child = textBlock
                                }
                              : textBlock;

            element.Measure(new Size(1000, 1000));
            element.Arrange(new Rect(element.DesiredSize));

            Material material;
            if (this.FontSize > 0)
            {
                RenderTargetBitmap rtb = new(
                    (int)element.ActualWidth + 1, (int)element.ActualHeight + 1, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(element);
                rtb.Freeze();
                material = new DiffuseMaterial(new ImageBrush(rtb));
            }
            else
            {
                material = new DiffuseMaterial { Brush = new VisualBrush(element) };
            }

            double width = element.ActualWidth / element.ActualHeight * this.Height;

            Point3D position = this.Position;
            Vector3D textDirection = this.TextDirection;
            Vector3D updirection = this.UpDirection;
            double height = this.Height;

            // Set horizontal alignment factor
            double xa = -0.5;
            if (this.HorizontalAlignment == HorizontalAlignment.Left)
            {
                xa = 0;
            }
            if (this.HorizontalAlignment == HorizontalAlignment.Right)
            {
                xa = -1;
            }

            // Set vertical alignment factor
            double ya = -0.5;
            if (this.VerticalAlignment == VerticalAlignment.Top)
            {
                ya = -1;
            }
            if (this.VerticalAlignment == VerticalAlignment.Bottom)
            {
                ya = 0;
            }

            // Since the parameter coming in was the center of the label,
            // we need to find the four corners
            // p0 is the lower left corner
            // p1 is the upper left
            // p2 is the lower right
            // p3 is the upper right
            Point3D p0 = position + (xa * width * textDirection) + (ya * height * updirection);
            Point3D p1 = p0 + (updirection * height);
            Point3D p2 = p0 + (textDirection * width);
            Point3D p3 = p0 + (updirection * height) + (textDirection * width);

            // Now build the geometry for the sign.  It's just a
            // rectangle made of two triangles, on each side.
            MeshGeometry3D mg = new() { Positions = new Point3DCollection { p0, p1, p2, p3 } };

            bool isDoubleSided = this.IsDoubleSided;
            if (isDoubleSided)
            {
                mg.Positions.Add(p0); // 4
                mg.Positions.Add(p1); // 5
                mg.Positions.Add(p2); // 6
                mg.Positions.Add(p3); // 7
            }

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            if (isDoubleSided)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }

            double u0 = this.IsFlipped ? 1 : 0;
            double u1 = this.IsFlipped ? 0 : 1;

            // These texture coordinates basically stretch the
            // TextBox brush to cover the full side of the label.
            mg.TextureCoordinates.Add(new Point(u0, 1));
            mg.TextureCoordinates.Add(new Point(u0, 0));
            mg.TextureCoordinates.Add(new Point(u1, 1));
            mg.TextureCoordinates.Add(new Point(u1, 0));

            if (isDoubleSided)
            {
                mg.TextureCoordinates.Add(new Point(u1, 1));
                mg.TextureCoordinates.Add(new Point(u1, 0));
                mg.TextureCoordinates.Add(new Point(u0, 1));
                mg.TextureCoordinates.Add(new Point(u0, 0));
            }

            return Tuple.Create(mg, material);
        }
    }
}