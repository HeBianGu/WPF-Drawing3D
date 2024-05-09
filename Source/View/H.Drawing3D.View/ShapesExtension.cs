using H.Drawing3D.Shape.Base;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{
    public static class ShapesExtension
    {
        public static IEnumerable<IShape3D> UpdateMouseOver(this IEnumerable<IShape3D> shapes, IEnumerable<Model3D> models, IEnumerable<IShape3D> exceptShapes = null)
        {
            IEnumerable<IMouseOverableShape3D> all = shapes.OfType<IMouseOverableShape3D>().Where(x => x.UseMouseOverable);
            IEnumerable<IMouseOverableShape3D> finds = all.Where(x => models.Contains(x.ShapeObject));
            exceptShapes ??= new List<IShape3D>();
            foreach (IShape3D item in all.Except(finds).Except(exceptShapes))
            {
                item.UpdateDefault();
            }
            foreach (IMouseOverableShape3D item in finds.Except(exceptShapes).OfType<IMouseOverableShape3D>())
            {
                item.UpdateMouseOver();
            }
            return finds;
        }

        public static IEnumerable<IShape3D> UpdateSelect(this IEnumerable<IShape3D> shapes, IEnumerable<Model3D> models)
        {
            IEnumerable<ISelectableShape3D> all = shapes.OfType<ISelectableShape3D>().Where(x => x.UseSelectable);
            IEnumerable<ISelectableShape3D> finds = all.Where(x => models.Contains(x.ShapeObject));
            shapes.UpdateSelect(finds);
            return finds;
        }

        public static void UpdateSelect(this IEnumerable<IShape3D> shapes, IEnumerable<ISelectableShape3D> selects)
        {
            IEnumerable<ISelectableShape3D> all = shapes.OfType<ISelectableShape3D>().Where(x => x.UseSelectable);
            foreach (ISelectableShape3D item in all.Except(selects))
            {
                item.UpdateDefault();
            }
            foreach (ISelectableShape3D item in selects)
            {
                item.UpdateSelect();
            }
        }
    }
}