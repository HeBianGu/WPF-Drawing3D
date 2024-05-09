using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Base
{
    public static class ShapeMaterials
    {
        public static Material Default = ShapeBrushs.Default.ToMaterial();
        public static Material Select = ShapeBrushs.Select.ToMaterial();
        public static Material MouseOver = ShapeBrushs.MouseOver.ToMaterial();
    }

    public static class ShapeBrushs
    {
        public static Brush Default = new SolidColorBrush(Colors.LightGray);
        public static Brush Select = new SolidColorBrush(SystemColors.HighlightColor);
        public static Brush MouseOver = new SolidColorBrush(SystemColors.HighlightColor) { Opacity = 0.8 };
        public static Material ToMaterial(this Brush brush)
        {
            DiffuseMaterial r = new(brush);
            r.Freeze();
            return r;
        }
    }

}