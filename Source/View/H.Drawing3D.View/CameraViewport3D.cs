// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraController.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides a control that manipulates the camera by mouse and keyboard gestures.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Drawing;
using H.Drawing3D.View.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{
    [ContentProperty("Children")]
    public partial class CameraViewport3D : CameraController
    {
        private readonly Viewport3D _viewport3D;
        private readonly Model3DGroup lights;
        public CameraViewport3D()
        {
            this._viewport3D = new Viewport3D();
            this.AddVisualChild(this._viewport3D);
            PerspectiveCamera perspectiveCamera = new();
            this._viewport3D.Camera = perspectiveCamera;
            this.DefaultCamera = perspectiveCamera;
            this.Camera = perspectiveCamera;
            this.lights = new Model3DGroup();
            byte ab = 0x75;
            AmbientLight l = new(Color.FromArgb(0xFF, ab, ab, ab));
            this.lights.Children.Add(l);
            DirectionalLight dl = new(Colors.White, new Vector3D(-1, -1, -1));
            this.lights.Children.Add(dl);
            this._viewport3D.Children.Add(new ModelVisual3D { Content = this.lights });
            this.Loaded += (l, k) =>
            {
                this.ZoomExtents();
            };

            this.SizeChanged += (l, k) =>
            {
                this.ZoomExtents();
            };

            this.Camera.Changed += (l, k) =>
            {
                this.OnCameraChanged();
            };

            this.InitInputBindings();
            this.InitSettings();
        }

        protected virtual void InitInputBindings()
        {
            this.InitCameraInputBindings();
        }

        public void InitCameraInputBindings()
        {
            _ = this.InputBindings.Add(new MouseBinding(CameraController.RotateCommand, new MouseGesture(MouseAction.RightClick)));
            _ = this.InputBindings.Add(new MouseBinding(CameraController.PanCommand, new MouseGesture(MouseAction.MiddleClick)));
            //this.InputBindings.Add(new MouseBinding(CameraController.ZoomRectangleCommand, new MouseGesture(MouseAction.RightClick)));
            _ = this.InputBindings.Add(new MouseBinding(CameraController.ZoomExtentsCommand, new MouseGesture(MouseAction.LeftDoubleClick)));
        }
        protected virtual void InitSettings()
        {
            //  Do ：按鼠标位置
            this.ZoomAroundMouseDownPoint = true;
            this.RotateAroundMouseDownPoint = false;

            //  Do ：速度
            this.RotationSensitivity = this.RotationSensitivity;
            this.ZoomSensitivity = this.ZoomSensitivity;

            //  Do ：模式
            this.CameraRotationMode = this.CameraRotationMode;//旋转
            this.CameraMode = this.CameraMode;//缩放

            //  Do ：惯性因子
            this.InertiaFactor = this.InertiaFactor;

            // 是否是正交相机
            //this.IsOrthographicCamera = true;

            //  Do ：相机的近远景
            this.Camera.NearPlaneDistance = 0.001;
            this.Camera.FarPlaneDistance = double.MaxValue;
        }



        public override Viewport3D Viewport => this._viewport3D;
        public Visual3DCollection Children => this._viewport3D.Children;
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index)
        {
            return this._viewport3D;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._viewport3D.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }


        public Brush Background
        {
            get => (Brush)this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(CameraViewport3D), new FrameworkPropertyMetadata(Brushes.Transparent, (d, e) =>
            {
                if (d is not CameraViewport3D control)
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


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Size renderSize = this.RenderSize;
            dc.DrawRectangle(this.Background, null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
        }
    }

    public partial class CameraViewport3D
    {
        public static readonly RoutedEvent CameraChangedRoutedEvent =
            EventManager.RegisterRoutedEvent("CameraChanged", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(CameraViewport3D));

        public event RoutedEventHandler CameraChanged
        {
            add => this.AddHandler(CameraChangedRoutedEvent, value);
            remove => this.RemoveHandler(CameraChangedRoutedEvent, value);
        }

        protected virtual void OnCameraChanged()
        {
            RoutedEventArgs args = new(CameraChangedRoutedEvent, this);
            this.RaiseEvent(args);
        }
    }

    public partial class CameraViewport3D : ICameraViewport3D
    {
        public IEnumerable<Model3D> HitModel3Ds(Point position)
        {
            IList<Viewport3DHelper.HitResult> res = this.Viewport.FindHits(position);
            return res.Select(hit => hit.Model);
        }

        public IEnumerable<Visual3D> HitVisual3Ds(Point position)
        {
            IList<Viewport3DHelper.HitResult> res = this.Viewport.FindHits(position);
            return res.Select(hit => hit.Visual);
        }
    }
}
