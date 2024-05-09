using H.Drawing3D.Shape;
using H.Drawing3D.Shape.Base;
using H.Drawing3D.View;
using H.Drawing3D.View.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Test.Draw3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem is ISelectableShape3D modelShape)
                {
                    Rect3D bound = modelShape.ShapeObject.Bounds;
                    this.sv1.ZoomExtents(bound, 200);

                    IEnumerable<ISelectableShape3D> shapes = this.sv1.Shapes.OfType<ISelectableShape3D>();
                    foreach (ISelectableShape3D item in shapes)
                    {
                        item.UpdateDefault();
                    }
                    modelShape.UpdateSelect();
                }
            }
        }

        private void sv2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(this.sv2);
            System.Diagnostics.Debug.WriteLine("MousePoint:" + point);

            //{
            //    //  Do ：测试1
            //    var v = this.sv2.Viewport.FindNearestVisual(point);
            //    var res = this.sv2.Viewport.FindHits(point);
            //    var selectedModels = res.Select(hit => hit.Model).ToList();
            //    foreach (var item in selectedModels)
            //    {
            //        var points = item.Bounds.ToPoints();
            //        var points1 = item.Bounds.ToPoints().Select(x => this.sv2.ToView(x));
            //        var points2 = points1.Select(x => this.sv2.Viewport.UnProject(x));
            //        foreach (var p in points2)
            //        {
            //            var s = new SphereShape3D() { Center = p.Value, Radius = 0.2, Material = new DiffuseMaterial(new SolidColorBrush(Colors.Pink)) };
            //            this.sv2.Shapes.Add(s);
            //        }
            //        //this.sv2.ToView(item.Bounds);
            //    }
            //}

            {
                //  Do ：hit上的点
                Point3D? p = this.sv2.Viewport.FindNearestPoint(point);
                if (p != null)
                {
                    SphereShape3D s = new() { Center = p.Value, Radius = 0.2, Material = new DiffuseMaterial(new SolidColorBrush(Colors.Pink)) };
                    this.sv2.Shapes.Add(s);
                    Point viewPoint = this.sv2.Viewport.Point3DtoPoint2D(p.Value);
                    System.Diagnostics.Debug.WriteLine("viewPoint:" + viewPoint);
                }
            }

            //{
            //    //  Do ：hit上的点
            //    var p = this.sv2.Viewport.FindNearestPoint(point);
            //    if (p != null)
            //    {
            //        var s = new SphereShape3D() { Center = p.Value };
            //        this.sv2.Shapes.Add(s);
            //    }
            //}

            //{
            //    var p = this.sv2.Viewport.UnProject(point);
            //    if (p != null)
            //    {
            //        //相机与平面平行的交点
            //        var s = new SphereShape3D() { Center = p.Value };
            //        this.sv2.Shapes.Add(s);
            //    }
            //}
        }
    }
}