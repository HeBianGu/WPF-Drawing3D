using H.Drawing3D.Shape.Base;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{

    [ContentProperty("Shape3D")]
    public class ShapeView3D : Presenter2DView3D, IView3D
    {
        private readonly ModelVisual3D _m3d;
        public ShapeView3D()
        {
            this._m3d = new ModelVisual3D();
            this.Children.Add(this._m3d);
        }
        public IShape3D Shape3D
        {
            get => (IShape3D)this.GetValue(Shape3DProperty);
            set => this.SetValue(Shape3DProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Shape3DProperty =
            DependencyProperty.Register("Shape3D", typeof(IShape3D), typeof(ShapeView3D), new FrameworkPropertyMetadata(default(IShape3D), (d, e) =>
            {
                if (d is not ShapeView3D control)
                {
                    return;
                }

                if (e.OldValue is IShape3D o)
                {

                }

                if (e.NewValue is IShape3D n)
                {
                    control.RefreshDraw();
                }
            }));

        public virtual void RefreshDraw()
        {
            this.Clear();
            if (this.Shape3D == null)
            {
                return;
            }

            this.Shape3D.Drawing();

            if (this.Shape3D is IPresenter2DShape3D presenterShape)
            {
                object presenter = presenterShape.GetPresenter2D();
                if (presenter != null)
                {
                    this.Add(presenter);
                }
            }
            this._m3d.Content = this.Shape3D.ShapeObject;
        }


        public override void Clear()
        {
            this._m3d.Content = null;
        }

        protected override void OnCameraChanged()
        {
            base.OnCameraChanged();
            this.Shape3D?.CameraUpdate(this.Camera);

            if (this.Shape3D is IPresenter2DShape3D item)
            {
                item.UpdatePresenter2D(this);
            }
        }
    }
}