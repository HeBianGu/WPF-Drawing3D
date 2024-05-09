using H.Drawing3D.ShapeLayer.Layer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{
    [ContentProperty("Layers")]
    public class LayerView3D : Presenter2DView3D, ILayerView3D
    {
        private readonly List<Visual3D> _v3ds = new();
        public LayerView3D()
        {
            //ModelVisual3D m3d = new ModelVisual3D();
            //m3d.Content = _group = new Model3DGroup();
            //this.Children.Add(m3d);
            this.Layers = new ObservableCollection<ILayer3D>();
        }

        public ObservableCollection<ILayer3D> Layers
        {
            get => (ObservableCollection<ILayer3D>)this.GetValue(LayersProperty);
            set => this.SetValue(LayersProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersProperty =
            DependencyProperty.Register("Layers", typeof(ObservableCollection<ILayer3D>), typeof(LayerView3D), new FrameworkPropertyMetadata(new ObservableCollection<ILayer3D>(), (d, e) =>
            {
                if (d is not LayerView3D control)
                {
                    return;
                }

                if (e.OldValue is ObservableCollection<ILayer3D> o)
                {
                    o.CollectionChanged -= control.CollectionChanged;

                }

                if (e.NewValue is ObservableCollection<ILayer3D> n)
                {
                    n.CollectionChanged -= control.CollectionChanged;
                    n.CollectionChanged += control.CollectionChanged;
                }
                control.RefreshLayers();
            }));

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RefreshLayers();
        }

        private void RefreshLayers()
        {
            this.Clear();
            foreach (ILayer3D layer in this.Layers)
            {
                layer.InitView(this);
                this._v3ds.Add(layer.ModelVisual3D);
                this.Children.Add(layer.ModelVisual3D);
            }
            this.RefreshDraw();
        }

        public void RefreshDraw()
        {
            foreach (ILayer3D layer in this.Layers)
            {
                layer.Clear();
                layer.Drawing();
            }
        }

        public override void Clear()
        {
            foreach (Visual3D item in this._v3ds)
            {
                _ = this.Children.Remove(item);
            }
            this._v3ds.Clear();
            base.Clear();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            IEnumerable<IMouseLayer3D> layers = this.Layers.OfType<IMouseLayer3D>();
            foreach (IMouseLayer3D item in layers)
            {
                item.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            IEnumerable<IMouseLayer3D> layers = this.Layers.OfType<IMouseLayer3D>();
            foreach (IMouseLayer3D item in layers)
            {
                item.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            IEnumerable<IMouseLayer3D> layers = this.Layers.OfType<IMouseLayer3D>();
            foreach (IMouseLayer3D item in layers)
            {
                item.OnMouseUp(e);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IEnumerable<IMouseLayer3D> layers = this.Layers.OfType<IMouseLayer3D>();
            foreach (IMouseLayer3D item in layers)
            {
                item.OnMouseLeave(e);
            }
        }

        protected override void OnCameraChanged()
        {
            IEnumerable<ICameraUpdateLayer> all = this.Layers.OfType<ICameraUpdateLayer>();
            foreach (ICameraUpdateLayer item in all)
            {
                item.OnCameraUpdate(this);
            }
            base.OnCameraChanged();
        }
    }
}