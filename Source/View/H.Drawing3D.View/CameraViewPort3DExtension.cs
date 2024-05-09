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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View
{
    public static class CameraViewPort3DExtension
    {
        public static void ZoomExtents(this ICameraViewport3D view, Rect3D bounds, double animationTime = 200)
        {
            view.Camera.ZoomExtents(view.Viewport, bounds, animationTime);
        }

        public static void ZoomToFit(this ICameraViewport3D view, double animationTime = 200)
        {
            view.Camera.ZoomExtents(view.Viewport, animationTime);
        }

        public static void LookAt(this ICameraViewport3D view, Point3D target, double animationTime = 200)
        {
            view.Camera.LookAt(target, animationTime);
        }

        public static void Reset(this ICameraViewport3D view)
        {
            view.Camera.Reset();
        }

        public static Rect ToView(this ICameraViewport3D view, Rect3D rect3D)
        {
            IEnumerable<Point> points = rect3D.ToPoints().Select(view.ToView);
            double mx = points.Min(x => x.X);
            double my = points.Min(x => x.Y);
            double max = points.Min(x => x.X);
            double may = points.Min(x => x.Y);
            return new Rect(mx, my, max - mx, may - my);
        }

        /// <summary>
        /// HitTest一个Model3D上的点
        /// </summary>
        /// <param name="view"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point3D? ToGeo(this ICameraViewport3D view, Point position)
        {
            return view.Viewport.FindNearestPoint(position);
        }

        /// <summary>
        /// 将三维中的点转换为屏幕坐标
        /// </summary>
        /// <param name="view"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point ToView(this ICameraViewport3D view, Point3D point)
        {
            return view.Viewport.Point3DtoPoint2D(point);
        }

        /// <summary>
        /// 将三维中的点转换为屏幕坐标
        /// </summary>
        /// <param name="view"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double ToGeo(this ICameraViewport3D view, double len)
        {
            Point p = new();
            Point p1 = new(0, len);
            Point3D? p2 = view.Viewport.UnProject(p);
            Point3D? p3 = view.Viewport.UnProject(p1);
            return p2 == null || p3 == null ? len : (p3.Value - p2.Value).Length;
        }
    }

    public static class Rect3DExtension
    {
        public static IEnumerable<Point3D> ToPoints(this Rect3D view)
        {
            yield return view.Location;
            yield return view.Location + new Vector3D(view.SizeX, 0, 0);
            yield return view.Location + new Vector3D(0, view.SizeY, 0);
            yield return view.Location + new Vector3D(0, 0, view.SizeZ);

            yield return view.Location + new Vector3D(view.SizeX, view.SizeY, view.SizeZ);
            yield return view.Location + new Vector3D(view.SizeX, view.SizeY, 0);
            yield return view.Location + new Vector3D(0, view.SizeY, view.SizeZ);
            yield return view.Location + new Vector3D(view.SizeX, 0, view.SizeZ);
        }

        public static IEnumerable<Point3D> ToPoints(params Point3D[] points)
        {
            return points;
        }
    }
}
