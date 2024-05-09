

using H.Drawing3D.Shape;
using H.Drawing3D.Shape.Base;
using System;
using System.Windows.Media.Media3D;

namespace H.Test.Draw3D
{
    public class GameArrowShape3D : ArrowShape3D, IGameShape3D
    {
        public virtual void Update()
        {
            System.Diagnostics.Debug.WriteLine("GameArrowShape3D Update:" + Environment.TickCount);
            _ = this.ShapeObject.Transform.Transform(new Vector3D(1, 1, 1));

            this.Point1 += new Vector3D(1, 1, 1);
            this.Point2 += new Vector3D(1, 1, 1);
            this.Drawing();
        }
    }
}
