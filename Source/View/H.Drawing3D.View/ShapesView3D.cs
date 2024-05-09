using H.Drawing3D.Shape.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{

    [ContentProperty("Shapes")]
    public class ShapesView3D : Presenter2DView3D, IShapesView3D
    {
        private readonly ModelVisual3D _m3d;
        public ShapesView3D()
        {
            this._m3d = new ModelVisual3D();
            this.Children.Add(this._m3d);
            this.Shapes = new ObservableCollection<IShape3D>();
        }

        public ObservableCollection<IShape3D> Shapes
        {
            get => (ObservableCollection<IShape3D>)this.GetValue(ShapesProperty);
            set => this.SetValue(ShapesProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShapesProperty =
            DependencyProperty.Register("Shapes", typeof(ObservableCollection<IShape3D>), typeof(ShapesView3D), new FrameworkPropertyMetadata(new ObservableCollection<IShape3D>(), (d, e) =>
            {
                if (d is not ShapesView3D control)
                {
                    return;
                }

                if (e.OldValue is ObservableCollection<IShape3D> o)
                {
                    o.CollectionChanged -= control.CollectionChanged;
                }

                if (e.NewValue is ObservableCollection<IShape3D> n)
                {
                    n.CollectionChanged -= control.CollectionChanged;
                    n.CollectionChanged += control.CollectionChanged;
                }
                control.RefreshDraw();
            }));

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RefreshDraw();
        }

        public virtual void RefreshDraw()
        {
            this.Clear();
            if (this.Shapes == null)
            {
                return;
            }

            Model3DGroup group = new();
            foreach (IShape3D item in this.Shapes)
            {
                item.Drawing();
                group.Children.Add(item.ShapeObject);

                if (item is IPresenter2DShape3D presenterShape)
                {
                    object presenter = presenterShape.GetPresenter2D();
                    if (presenter != null)
                    {
                        this.Add(presenter);
                    }
                }
            }
            this._m3d.Content = group;
        }

        public override void Clear()
        {
            this._m3d.Content = null;
            base.Clear();
        }

        protected override void OnCameraChanged()
        {
            IEnumerable<ISelectableShape3D> all = this.Shapes.OfType<ISelectableShape3D>();
            foreach (ISelectableShape3D item in all)
            {
                item.CameraUpdate(this.Camera);
            }

            foreach (IPresenter2DShape3D item in this.Shapes.OfType<IPresenter2DShape3D>())
            {
                item.UpdatePresenter2D(this);
            }
            base.OnCameraChanged();
        }
    }
}