using H.Drawing3D.Drawing;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace H.Drawing3D.View
{
    public class Presenter2DView3D : CameraViewport3D, IPresenter2DView3D
    {
        private readonly Canvas _canvas;
        public Presenter2DView3D()
        {
            this._canvas = new Canvas();
            this.AddVisualChild(this._canvas);
        }

        protected override int VisualChildrenCount => base.VisualChildrenCount + 1;
        protected override Visual GetVisualChild(int index)
        {
            return index == 0 ? base.GetVisualChild(index) : this._canvas;
        }

        public void Add(object presenter, Action<ContentPresenter> action = null)
        {
            ContentPresenter control = new() { Content = presenter };
            action?.Invoke(control);
            _ = this._canvas.Children.Add(control);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._canvas.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }

        public virtual void Clear()
        {
            this._canvas.Children.Clear();
        }

        public void Delete(object presenter)
        {
            ContentPresenter control = this.GetContentPresenter(presenter);
            this._canvas.Children.Remove(control);
        }

        public ContentPresenter GetContentPresenter(object presenter)
        {
            return this._canvas.Children.OfType<ContentPresenter>().Where(x => x.Content == presenter).FirstOrDefault();
        }
    }
}