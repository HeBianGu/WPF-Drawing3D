using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawable
{
    public class Sphere : UIElement3D
    {
        // OnUpdateModel is called in response to InvalidateModel and provides
        // a place to set the Visual3DModel property.
        // 
        // Setting Visual3DModel does not provide parenting information, which
        // is needed for data binding, styling, and other features. Similarly, creating render data
        // in 2-D does not provide the connections either.
        // 
        // To get around this, we create a Model dependency property which
        // sets this value.  The Model DP then causes the correct connections to occur
        // and the above features to work correctly.
        // 
        // In this update model we retessellate the sphere based on the current
        // dependency property values, and then set it as the model.  The brush
        // color is blue by default, but the code can easily be updated to let
        // this be set by the user.

        protected override void OnUpdateModel()
        {
            GeometryModel3D model = new()
            {
                Geometry = Tessellate(this.ThetaDiv, this.PhiDiv, this.Radius),
                Material = new DiffuseMaterial(this.MaterialBrush),
                BackMaterial = new DiffuseMaterial(this.BackMaterialBrush)
            };
            this.Model = model;
        }


        public Brush BackMaterialBrush
        {
            get => (Brush)this.GetValue(BackMaterialBrushProperty);
            set => this.SetValue(BackMaterialBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackMaterialBrushProperty =
            DependencyProperty.Register("BackMaterialBrush", typeof(Brush), typeof(Sphere), new FrameworkPropertyMetadata(default(Brush), (d, e) =>
            {
                if (d is not Sphere control)
                {
                    return;
                }

                if (e.OldValue is Brush o)
                {

                }

                if (e.NewValue is Brush n)
                {

                }

            }));


        public Brush MaterialBrush
        {
            get => (Brush)this.GetValue(MaterialBrushProperty);
            set => this.SetValue(MaterialBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaterialBrushProperty =
            DependencyProperty.Register("MaterialBrush", typeof(Brush), typeof(Sphere), new PropertyMetadata(new SolidColorBrush(Colors.Red), (d, e) =>
            {
                if (d is not Sphere control)
                {
                    return;
                }

                Brush config = e.NewValue as Brush;

            }));


        // The Model property for the sphere
        private static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model",
                                        typeof(Model3D),
                                        typeof(Sphere),
                                        new PropertyMetadata(ModelPropertyChanged));

        private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sphere s = (Sphere)d;
            s.Visual3DModel = s.Model;
        }

        private Model3D Model
        {
            get => (Model3D)this.GetValue(ModelProperty);

            set => this.SetValue(ModelProperty, value);
        }

        // The number of divisions to make in the theta direction on the sphere
        public static readonly DependencyProperty ThetaDivProperty =
            DependencyProperty.Register("ThetaDiv",
                                        typeof(int),
                                        typeof(Sphere),
                                        new PropertyMetadata(15, ThetaDivPropertyChanged));

        private static void ThetaDivPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sphere s = (Sphere)d;
            s.InvalidateModel();
        }

        public int ThetaDiv
        {
            get => (int)this.GetValue(ThetaDivProperty);

            set => this.SetValue(ThetaDivProperty, value);
        }

        // The number of divisions to make in the phi direction on the sphere
        public static readonly DependencyProperty PhiDivProperty =
            DependencyProperty.Register("PhiDiv",
                                        typeof(int),
                                        typeof(Sphere),
                                        new PropertyMetadata(15, PhiDivPropertyChanged));

        private static void PhiDivPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sphere s = (Sphere)d;
            s.InvalidateModel();
        }

        public int PhiDiv
        {
            get => (int)this.GetValue(PhiDivProperty);

            set => this.SetValue(PhiDivProperty, value);
        }

        // The radius of the sphere
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius",
                                        typeof(double),
                                        typeof(Sphere),
                                        new PropertyMetadata(1.0, RadiusPropertyChanged));

        private static void RadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sphere s = (Sphere)d;
            s.InvalidateModel();
        }

        public double Radius
        {
            get => (double)this.GetValue(RadiusProperty);

            set => this.SetValue(RadiusProperty, value);
        }

        // Private helper methods
        private static Point3D GetPosition(double theta, double phi, double radius)
        {
            double x = radius * Math.Sin(theta) * Math.Sin(phi);
            double y = radius * Math.Cos(phi);
            double z = radius * Math.Cos(theta) * Math.Sin(phi);

            return new Point3D(x, y, z);
        }

        private static Vector3D GetNormal(double theta, double phi)
        {
            return (Vector3D)GetPosition(theta, phi, 1.0);
        }

        private static double DegToRad(double degrees)
        {
            return degrees / 180.0 * Math.PI;
        }

        private static System.Windows.Point GetTextureCoordinate(double theta, double phi)
        {
            System.Windows.Point p = new(theta / (2 * Math.PI),
                                phi / Math.PI);

            return p;
        }

        // Tesselates the sphere and returns a MeshGeometry3D representing the 
        // tessellation based on the given parameters
        internal static MeshGeometry3D Tessellate(int tDiv, int pDiv, double radius)
        {
            double dt = DegToRad(360.0) / tDiv;
            double dp = DegToRad(180.0) / pDiv;

            MeshGeometry3D mesh = new();

            for (int pi = 0; pi <= pDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    mesh.Positions.Add(GetPosition(theta, phi, radius));
                    mesh.Normals.Add(GetNormal(theta, phi));
                    mesh.TextureCoordinates.Add(GetTextureCoordinate(theta, phi));
                }
            }

            for (int pi = 0; pi < pDiv; pi++)
            {
                for (int ti = 0; ti < tDiv; ti++)
                {
                    int x0 = ti;
                    int x1 = ti + 1;
                    int y0 = pi * (tDiv + 1);
                    int y1 = (pi + 1) * (tDiv + 1);

                    mesh.TriangleIndices.Add(x0 + y0);
                    mesh.TriangleIndices.Add(x0 + y1);
                    mesh.TriangleIndices.Add(x1 + y0);

                    mesh.TriangleIndices.Add(x1 + y0);
                    mesh.TriangleIndices.Add(x0 + y1);
                    mesh.TriangleIndices.Add(x1 + y1);
                }
            }

            mesh.Freeze();
            return mesh;
        }
    }
}
