// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraController.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides a control that manipulates the camera by mouse and keyboard gestures.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Base;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace H.Drawing3D.View
{
    public class GameShapesView3D : SelectableShapesView3D
    {
        private readonly Storyboard _storyboard;
        public GameShapesView3D()
        {
            this._storyboard = new Storyboard();
            Timeline.SetDesiredFrameRate(this._storyboard, this.FrameRate);
            DoubleAnimation animation = new(0, 100, TimeSpan.FromMilliseconds(100 * 1000));
            this._storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(nameof(this.Elapse)));
            this._storyboard.RepeatBehavior = RepeatBehavior.Forever;
            this._storyboard.FillBehavior = FillBehavior.Stop;
            this.Loaded += (l, k) =>
            {
                this._storyboard.Begin();
            };
        }

        public void StartGame()
        {
            this._storyboard.Begin();
        }

        public void StopGame()
        {
            this._storyboard.Pause();
        }

        public void Resume()
        {
            this._storyboard.Resume();
        }

        public int? FrameRate
        {
            get => (int?)this.GetValue(FrameRateProperty);
            set => this.SetValue(FrameRateProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(int?), typeof(GameShapesView3D), new FrameworkPropertyMetadata(1, (d, e) =>
            {
                if (d is not GameShapesView3D control)
                {
                    return;
                }

                //if (e.OldValue is int ? o)
                //{

                //}

                //if (e.NewValue is int ? n)
                //{

                //}
                control.UpdateElapsed();

            }));


        private double Elapse
        {
            get => (double)this.GetValue(ElapseProperty);
            set => this.SetValue(ElapseProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ElapseProperty =
            DependencyProperty.Register("Elapse", typeof(double), typeof(GameShapesView3D), new FrameworkPropertyMetadata(default(double), (d, e) =>
            {
                if (d is not GameShapesView3D control)
                {
                    return;
                }

                if (e.OldValue is double o)
                {

                }

                if (e.NewValue is double n)
                {

                }
                control.UpdateElapsed();
            }));


        protected virtual void UpdateElapsed()
        {
            foreach (IGameShape3D item in this.Shapes.OfType<IGameShape3D>())
            {
                item.Update();
            }
        }
    }
}
