

using H.Drawing3D.Drawable;
using H.Drawing3D.Drawable.Manipulators;
using H.Drawing3D.Shape;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Test.Draw3D
{

    public class ManipulatableCubeShape3D : CubeShape3D, IManipulableShape3D
    {
        public ManipulatableCubeShape3D()
        {

        }
        public IEnumerable<IManipulator> GetManipulators()
        {
            //var c = new CombinedManipulator();
            //c.valu

            TranslateManipulator translateManipulator = new()
            {
                Direction = new Vector3D(0, 1, 0),
                Color = Colors.Blue,
                Length = this.SideLength / 3,
                Offset = new Vector3D(0, this.SideLength / 2, 0),
                Position = this.Center,
                Value = this.SideLength / 2,
                Diameter = this.SideLength / 20
            };
            translateManipulator.ValueChanged += (l, k) =>
            {
                this.SideLength = k * 2;
                this.Drawing();
            };
            yield return translateManipulator;

            RotateManipulator rotateManipulatory = new()

            {
                Color = Colors.Green,
                Length = this.SideLength / 15,
                Axis = new Vector3D(0, 1, 0),
                Diameter = this.SideLength * 2,
                TargetTransform = this.ShapeObject.Transform,
                //Transform = this.ShapeObject.Transform,
                InnerDiameter = this.SideLength * 1.8,
                Position = this.Center,
                Pivot = this.Center,
            };

            //RotateManipulator rotateManipulatorx = new RotateManipulator()
            //{
            //    Color = Colors.Red,
            //    Length = this.SideLength / 15,
            //    Axis = new Vector3D(this.Center.X, 0, 0),
            //    Diameter = this.SideLength * 2,
            //    TargetTransform = this.ShapeObject.Transform,
            //    Transform = this.ShapeObject.Transform,
            //    InnerDiameter = this.SideLength * 1.8,
            //    Position = this.Center,
            //    //Pivot = this.Center,
            //};
            rotateManipulatory.ValueChanged += (l, k) =>
                {
                    //var rotateTransform = new RotateTransform3D(new AxisAngleRotation3D(this.Axis, theta), this.Pivot);
                    //this.TargetTransform = Transform3DHelper.CombineTransform(rotateTransform, this.TargetTransform);
                    //var transform = Transform3DHelper.CombineTransform(rotateManipulatory.TargetTransform, this.ShapeObject.Transform);
                    this.ShapeObject.Transform = rotateManipulatory.TargetTransform;
                    //rotateManipulatorx.Transform = rotateManipulatory.TargetTransform;
                    this.Drawing();
                };


            //      BindingOperations.SetBinding(rotateManipulatory,
            //Manipulator.TargetTransformProperty,
            //new Binding("TargetTransform") { Source = this.ShapeObject.Transform });
            yield return rotateManipulatory;



            //rotateManipulatorx.ValueChanged += (l, k) =>
            //        {
            //            //var transform = Transform3DHelper.CombineTransform(rotateManipulatorx.TargetTransform, this.ShapeObject.Transform);
            //            //this.ShapeObject.Transform = transform;
            //            this.ShapeObject.Transform = rotateManipulatorx.TargetTransform;
            //            //rotateManipulatory.Transform = rotateManipulatorx.TargetTransform;
            //            this.Drawing();
            //        };
            //yield return rotateManipulatorx;

        }
    }

    public class ManipulatableEllipsoidShape3D : EllipsoidShape3D, IManipulableShape3D
    {
        private readonly IManipulator _translateManipulator;
        public ManipulatableEllipsoidShape3D()
        {
            this._translateManipulator = new TranslateManipulator()
            {
                Direction = new Vector3D(0, 0, 1),
                Color = Colors.Blue,
                Length = 1,
                Offset = new Vector3D(0, 0, this.RadiusZ / 2),
                Position = this.Center,
                Value = this.RadiusZ / 2
            };
            this._translateManipulator.ValueChanged += (l, k) =>
            {
                this.RadiusZ = k;
                this.Drawing();
            };
        }
        public IEnumerable<IManipulator> GetManipulators()
        {

            yield return this._translateManipulator;

        }
    }
}
