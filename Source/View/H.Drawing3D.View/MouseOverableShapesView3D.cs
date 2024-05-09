// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraController.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides a control that manipulates the camera by mouse and keyboard gestures.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{
    public class MouseOverableShapesView3D : ShapesView3D
    {
        public MouseOverableShapesView3D()
        {

        }

        public bool UseMouseOver { get; set; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.DelayInvoke(() =>
            {
                Point point = e.GetPosition(this);
                List<Model3D> models = this.HitModel3Ds(point).ToList();
                this.OnMouseOverModelChanged(models);
            });
        }

        protected virtual void OnMouseOverModelChanged(IList<Model3D> models)
        {
            _ = this.Shapes.UpdateMouseOver(models);
        }
    }
}
