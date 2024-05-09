// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraController.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides a control that manipulates the camera by mouse and keyboard gestures.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Base;
using H.Drawing3D.View.SelectionCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{
    public class SelectableShapesView3D : MouseOverableShapesView3D
    {
        public bool UseSelect { get; set; } = true;

        public SelectableShapesView3D()
        {
            this.InitSelectionInputBindings();
        }
        protected virtual void InitSelectionInputBindings()
        {
            PointSelectionCommand pointSelectionCommand = new(this.Viewport, new EventHandler<ModelsSelectedEventArgs>((s, e) =>
            {
                if (this.UseSelect == false)
                {
                    return;
                }

                this.OnSelectionModelChanged(e.SelectedModels);
            }));
            _ = this.InputBindings.Add(new MouseBinding(pointSelectionCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control)));

            RectangleSelectionCommand rectangleSelectionCommand = new(this.Viewport, new EventHandler<ModelsSelectedEventArgs>((s, e) =>
            {
                if (this.UseSelect == false)
                {
                    return;
                }

                this.OnSelectionModelChanged(e.SelectedModels);
            }));
            _ = this.InputBindings.Add(new MouseBinding(pointSelectionCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control)));
        }

        public IEnumerable<IShape3D> SelectedShapes
        {
            get => (IEnumerable<IShape3D>)this.GetValue(SelectedShapesProperty);
            set => this.SetValue(SelectedShapesProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedShapesProperty =
            DependencyProperty.Register("SelectedShapes", typeof(IEnumerable<IShape3D>), typeof(SelectableShapesView3D), new FrameworkPropertyMetadata(default(IEnumerable<IShape3D>), (d, e) =>
            {
                if (d is not SelectableShapesView3D control)
                {
                    return;
                }

                if (e.OldValue is IEnumerable<IShape3D> o)
                {

                }

                if (e.NewValue is IEnumerable<IShape3D> n)
                {

                }
                control.RefreshSelection();
            }));

        public static readonly RoutedEvent SelectionModelChangedRoutedEvent =
            EventManager.RegisterRoutedEvent("SelectionModelChanged", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(CameraViewport3D));

        public event RoutedEventHandler SelectionModelChanged
        {
            add => this.AddHandler(SelectionModelChangedRoutedEvent, value);
            remove => this.RemoveHandler(SelectionModelChangedRoutedEvent, value);
        }

        protected virtual void OnSelectionModelChanged(IList<Model3D> models)
        {
            RoutedEventArgs args = new(SelectionModelChangedRoutedEvent, this);
            this.RaiseEvent(args);
            this.SelectedShapes = this.Shapes.UpdateSelect(models);
        }

        private void RefreshSelection()
        {
            this.Shapes.UpdateSelect(this.SelectedShapes.OfType<ISelectableShape3D>());
        }
    }
}
