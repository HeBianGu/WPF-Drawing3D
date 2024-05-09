// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RotateManipulator.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Represents a visual element containing a manipulator that can rotate around an axis.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Geometry;
using H.Drawing3D.View.Extensions;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawable.Manipulators
{
    /// <summary>
    /// Represents a visual element containing a manipulator that can rotate around an axis.
    /// </summary>
    public class RotateManipulator : Manipulator
    {
        /// <summary>
        /// Identifies the <see cref="Axis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisProperty = DependencyProperty.Register(
            "Axis",
            typeof(Vector3D),
            typeof(RotateManipulator),
            new UIPropertyMetadata(new Vector3D(0, 0, 1), UpdateGeometry));

        /// <summary>
        /// Identifies the <see cref="Diameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
            "Diameter", typeof(double), typeof(RotateManipulator), new UIPropertyMetadata(3.0, UpdateGeometry));

        /// <summary>
        /// Identifies the <see cref="InnerDiameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerDiameterProperty = DependencyProperty.Register(
            "InnerDiameter", typeof(double), typeof(RotateManipulator), new UIPropertyMetadata(2.5, UpdateGeometry));

        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
            "Length", typeof(double), typeof(RotateManipulator), new UIPropertyMetadata(0.1, UpdateGeometry));

        /// <summary>
        /// Identifies the <see cref="Pivot"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PivotProperty = DependencyProperty.Register(
            "Pivot", typeof(Point3D), typeof(Manipulator), new PropertyMetadata(new Point3D()));

        /// <summary>
        /// The last point.
        /// </summary>
        private Point3D lastPoint;

        /// <summary>
        /// Gets or sets the rotation axis.
        /// </summary>
        /// <value>The axis.</value>
        public Vector3D Axis
        {
            get => (Vector3D)this.GetValue(AxisProperty);

            set => this.SetValue(AxisProperty, value);
        }

        /// <summary>
        /// Gets or sets the outer diameter of the manipulator.
        /// </summary>
        /// <value>The outer diameter.</value>
        public double Diameter
        {
            get => (double)this.GetValue(DiameterProperty);

            set => this.SetValue(DiameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the inner diameter of the manipulator.
        /// </summary>
        /// <value>The inner diameter.</value>
        public double InnerDiameter
        {
            get => (double)this.GetValue(InnerDiameterProperty);

            set => this.SetValue(InnerDiameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the length (thickness) of the manipulator.
        /// </summary>
        /// <value>The length.</value>
        public double Length
        {
            get => (double)this.GetValue(LengthProperty);

            set => this.SetValue(LengthProperty, value);
        }

        /// <summary>
        /// Gets or sets the pivot point of the manipulator.
        /// </summary>
        /// <value> The position. </value>
        public Point3D Pivot
        {
            get => (Point3D)this.GetValue(PivotProperty);

            set => this.SetValue(PivotProperty, value);
        }

        /// <summary>
        /// Updates the geometry.
        /// </summary>
        protected override void UpdateGeometry()
        {
            MeshBuilder mb = new MeshBuilder(false, false);
            Point3D p0 = new(0, 0, 0);
            Vector3D d = this.Axis;
            d.Normalize();
            Point3D p1 = p0 - (d * this.Length * 0.5);
            Point3D p2 = p0 + (d * this.Length * 0.5);
            mb.AddPipe(p1, p2, this.InnerDiameter, this.Diameter, 60);
            this.Model.Geometry = mb.ToMesh();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Point3D hitPlaneOrigin = this.ToWorld(this.Position);
            Vector3D hitPlaneNormal = this.ToWorld(this.Axis);
            Point p = e.GetPosition(this.ParentViewport);

            Point3D? hitPoint = this.GetHitPlanePoint(p, hitPlaneOrigin, hitPlaneNormal);
            if (hitPoint != null)
            {
                this.lastPoint = this.ToLocal(hitPoint.Value);
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.IsMouseCaptured)
            {
                Point3D hitPlaneOrigin = this.ToWorld(this.Position);
                Vector3D hitPlaneNormal = this.ToWorld(this.Axis);

                Point position = e.GetPosition(this.ParentViewport);
                Point3D? hitPoint = this.GetHitPlanePoint(position, hitPlaneOrigin, hitPlaneNormal);
                if (hitPoint == null)
                {
                    return;
                }

                Point3D currentPoint = this.ToLocal(hitPoint.Value);

                Vector3D v = this.lastPoint - this.Position;
                Vector3D u = currentPoint - this.Position;
                v.Normalize();
                u.Normalize();

                Vector3D currentAxis = Vector3D.CrossProduct(u, v);
                double sign = -Vector3D.DotProduct(this.Axis, currentAxis);
                double theta = Math.Sign(sign) * Math.Asin(currentAxis.Length) / Math.PI * 180;
                this.Value += theta;

                if (this.TargetTransform != null)
                {
                    RotateTransform3D rotateTransform = new(new AxisAngleRotation3D(this.Axis, theta), this.Pivot);
                    this.TargetTransform = Transform3DHelper.CombineTransform(rotateTransform, this.TargetTransform);
                }

                hitPoint = this.GetHitPlanePoint(position, hitPlaneOrigin, hitPlaneNormal);
                if (hitPoint != null)
                {
                    this.lastPoint = this.ToLocal(hitPoint.Value);
                }
            }
        }
    }
}