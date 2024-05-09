using H.Drawing3D.Drawable.Layer;
using H.Drawing3D.Drawing;
using H.Drawing3D.Shape;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.ShapeLayer.Layer;
using H.Drawing3D.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawable
{
    public abstract class DrawableSelectableShapeLayer : MouseOverableLayerBase, ICameraUpdateLayer
    {
        private readonly List<IShape3D> _boundBoxShapes = new();

        public ObservableCollection<IShape3D> SelectedShapes
        {
            get => (ObservableCollection<IShape3D>)this.GetValue(SelectedShapesProperty);
            set => this.SetValue(SelectedShapesProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedShapesProperty =
            DependencyProperty.Register("SelectedShapes", typeof(ObservableCollection<IShape3D>), typeof(DrawableSelectableShapeLayer), new FrameworkPropertyMetadata(new ObservableCollection<IShape3D>(), (d, e) =>
            {
                if (d is not DrawableSelectableShapeLayer control)
                {
                    return;
                }

                if (e.OldValue is ObservableCollection<IShape3D> o)
                {

                }

                if (e.NewValue is ObservableCollection<IShape3D> n)
                {
                    control.OnSelectionChanged();
                }
            }));

        public virtual void OnSelectionChanged()
        {
            this.ClearBoundBoxShapes();
            foreach (IShape3D item in this.SelectedShapes)
            {
                Rect3D bounds = item.ShapeObject.Bounds;
                double l = new Vector3D(bounds.Size.X, bounds.Y, bounds.Z).Length;
                BoundingBoxShape3D shape = new() { BoundingBox = bounds, Diameter = l / 50, Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray) { Opacity = 0.5 }) };
                shape.Drawing();
                this._boundBoxShapes.Add(shape);
                this.AddShape(shape);
            }
        }

        public void ClearBoundBoxShapes()
        {
            foreach (IShape3D v in this._boundBoxShapes)
            {
                this.RemoveShape(v);
            }
            this._boundBoxShapes.Clear();
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point p = e.GetPosition(this.View as UIElement);
            List<Model3D> models = this.View.HitModel3Ds(p).ToList();
            if (this.View is LayerView3D layerView)
            {
                IEnumerable<IShape3D> all = layerView.GetShapes();
                _ = all.UpdateMouseOver(models, this.SelectedShapes);
            }
        }

        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Point p = e.GetPosition(this.View as UIElement);
            List<Model3D> models = this.View.HitModel3Ds(p).ToList();
            if (this.View is LayerView3D layerView)
            {
                IEnumerable<IShape3D> all = layerView.GetShapes();
                this.SelectedShapes = new ObservableCollection<IShape3D>(all.UpdateSelect(models));
                this.Drawing();
            }
        }

        //public override void Clear()
        //{
        //    foreach (var item in this.SelectedShapes)
        //    {
        //        this.RemoveShape(item);
        //    }
        //    base.Clear();
        //}


        //public void ClearSelection()
        //{
        //    说明
        //}


        public override void Drawing()
        {
            ////  Do ：根据屏幕尺寸计算geo尺寸绘制线的粗细
            //double l = this.View.ToGeo(100);
            //foreach (var item in this.SelectedShapes)
            //{
            //    Rect3D bounds = item.ShapeObject.Bounds;
            //    //double l = new Vector3D(bounds.Size.X, bounds.Y, bounds.Z).Length;
            //    var shape = new BoundingBoxShape3D { BoundingBox = bounds, Diameter = l / 50, Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray) { Opacity = 0.5 }) };
            //    shape.Drawing();
            //    this.AddShape(shape);
            //}

            //foreach (var item in this._boundBoxShapes)
            //{
            //    item.Drawing();
            //}

        }

        public virtual void OnCameraUpdate(ICameraViewport3D vierw)
        {
            this.Drawing();
        }
    }
}