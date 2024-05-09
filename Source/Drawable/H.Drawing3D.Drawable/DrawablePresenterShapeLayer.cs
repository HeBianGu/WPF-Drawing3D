using H.Drawing3D.Drawing;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.View;
using System.Collections.Generic;
using System.Windows.Controls;

namespace H.Drawing3D.Drawable
{
    public class DrawablePresenterShapeLayer : DrawableSelectableShapeLayer
    {
        private readonly Dictionary<IShape3D, ContentPresenter> _presenters = new();
        public override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            if (this.View is IPresenter2DView3D presenterView)
            {
                foreach (KeyValuePair<IShape3D, ContentPresenter> item in this._presenters)
                {
                    presenterView.Delete(item.Key);
                }
                this._presenters.Clear();
                foreach (IShape3D item in this.SelectedShapes)
                {
                    presenterView.Add(item, x =>
                    {
                        this._presenters.Add(item, x);
                    });
                }
            }
        }
        public override void Drawing()
        {
            base.Drawing();
            if (this.View is IPresenter2DView3D)
            {
                foreach (KeyValuePair<IShape3D, ContentPresenter> item in this._presenters)
                {
                    System.Windows.Point p2d = this.View.ToView(item.Key.ShapeObject.Bounds.Location);
                    Canvas.SetLeft(item.Value, p2d.X);
                    Canvas.SetTop(item.Value, p2d.Y);
                }
            }
        }
    }
}