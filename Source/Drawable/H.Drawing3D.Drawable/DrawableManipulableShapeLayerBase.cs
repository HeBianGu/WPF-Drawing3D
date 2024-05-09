using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using IManipulator = H.Drawing3D.Drawable.Manipulators.IManipulator;

namespace H.Drawing3D.Drawable
{
    public abstract class DrawableManipulableShapeLayerBase : DrawableSelectableShapeLayer
    {
        private List<IManipulator> _manipulatorsCache = new();
        public override void OnSelectionChanged()
        {
            base.OnSelectionChanged();
            this.ClearManipulators();
            this._manipulatorsCache = this.SelectedShapes.OfType<IManipulableShape3D>().SelectMany(x => x.GetManipulators()).ToList();
        }

        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this.View as UIElement);
            List<Visual3D> models = this.View.HitVisual3Ds(p).ToList();
            if (this._manipulatorsCache.OfType<Visual3D>().Any(models.Contains))
            {
                return;
            }

            base.OnMouseDown(e);
        }

        public override void Clear()
        {
            base.Clear();
            this.ClearManipulators();
        }

        public void ClearManipulators()
        {
            foreach (Visual3D v in this._manipulatorsCache.OfType<Visual3D>())
            {
                _ = this.View.Viewport.Children.Remove(v);
            }
        }
        public override void Drawing()
        {
            base.Drawing();
            this.ClearManipulators();
            foreach (Visual3D v in this._manipulatorsCache.OfType<Visual3D>())
            {
                this.View.Viewport.Children.Add(v);
            }
        }
    }
}