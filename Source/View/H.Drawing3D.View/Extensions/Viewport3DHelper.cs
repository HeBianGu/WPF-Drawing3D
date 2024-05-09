// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Viewport3DHelper.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides extension methods for Viewport3D.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.View.Extensions;
using H.Drawing3D.View.Privider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace H.Drawing3D.View.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Viewport3D"/>.
    /// </summary>
    /// <remarks>
    /// See "3D programming for Windows" (Charles Petzold book) and <a hef="http://www.ericsink.com/wpf3d/index.html">Twelve Days of WPF 3D</a>.
    /// </remarks>
    public static class Viewport3DHelper
    {
        /// <summary>
        /// Copies the specified viewport to the clipboard.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="m">The oversampling multiplier.</param>
        public static void Copy(this Viewport3D view, int m = 1)
        {
            Clipboard.SetImage(view.RenderBitmap(Brushes.White, m));
        }

        /// <summary>
        /// Copies the specified viewport to the clipboard.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="background">The background.</param>
        /// <param name="m">The oversampling multiplier.</param>
        public static void Copy(this Viewport3D view, double width, double height, Brush background, int m = 1)
        {
            Clipboard.SetImage(view.RenderBitmap(width, height, background));
        }

        /// <summary>
        /// Copies the viewport as <code>xaml</code> to the clipboard.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        public static void CopyXaml(this Viewport3D viewport)
        {
            Clipboard.SetText(XamlWriter.Save(viewport));
        }

        /// <summary>
        /// Exports the specified viewport.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="background">The background brush.</param>
        public static void Export(this Viewport3D viewport, string fileName, Brush background = null)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (ext != null)
            {
                _ = ext.ToLower();
            }

            //switch (ext)
            //{
            //    case ".jpg":
            //    case ".png":
            //        SaveBitmap(viewport, fileName, background, 2);
            //        break;
            //    case ".xaml":
            //        ExportXaml(viewport, fileName);
            //        break;
            //    case ".xml":
            //        ExportKerkythea(viewport, fileName, background);
            //        break;
            //    case ".obj":
            //        ExportObj(viewport, fileName);
            //        break;
            //    case ".x3d":
            //        ExportX3D(viewport, fileName);
            //        break;
            //    case ".dae":
            //        ExportCollada(viewport, fileName);
            //        break;
            //    case ".stl":
            //        ExportStl(viewport, fileName);
            //        break;
            //    default:
            //        throw new HelixToolkitException("Not supported file format.");
            //}
        }

        ///// <summary>
        ///// Exports the specified viewport.
        ///// </summary>
        ///// <param name="viewport">The viewport.</param>
        ///// <param name="fileName">Name of the file.</param>
        ///// <param name="stereoBase">The stereo base.</param>
        ///// <param name="background">The background brush.</param>
        ///// <exception cref="HelixToolkitException">Not supported file format.</exception>
        //public static void ExportStereo(this Viewport3D viewport, string fileName, double stereoBase, Brush background = null)
        //{
        //    string ext = System.IO.Path.GetExtension(fileName);
        //    if (ext != null)
        //    {
        //        ext = ext.ToLower();
        //    }

        //    switch (ext)
        //    {
        //        case ".jpg":
        //        case ".png":
        //            SaveStereoBitmap(viewport, fileName, stereoBase, background, 2);
        //            break;
        //        case ".mpo":
        //            throw new HelixToolkitException("MPO is not yet supported.");
        //        default:
        //            throw new HelixToolkitException("Not supported file format.");
        //    }
        //}

        /// <summary>
        /// Finds the hits for the specified position.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <returns>
        /// List of hits, sorted with the nearest hit first.
        /// </returns>
        public static IList<HitResult> FindHits(this Viewport3D viewport, Point position)
        {
            if (viewport.Camera is not ProjectionCamera camera)
            {
                return null;
            }

            List<HitResult> result = new();
            HitTestResultBehavior callback(HitTestResult hit)
            {
                if (hit is RayMeshGeometry3DHitTestResult rayHit)
                {
                    if (rayHit.MeshHit != null)
                    {
                        Point3D p = GetGlobalHitPosition(rayHit, viewport);
                        Vector3D? nn = GetNormalHit(rayHit);
                        Vector3D n = nn.HasValue ? nn.Value : new Vector3D(0, 0, 1);

                        result.Add(
                            new HitResult
                            {
                                Distance = (camera.Position - p).Length,
                                RayHit = rayHit,
                                Normal = n,
                                Position = p
                            });
                    }
                }

                return HitTestResultBehavior.Continue;
            }

            PointHitTestParameters htp = new(position);
            VisualTreeHelper.HitTest(viewport, null, callback, htp);

            return result.OrderBy(k => k.Distance).ToList();
        }

        /// <summary>
        /// Finds the hits for the specified rectangle.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="rectangle">
        /// The rectangle.
        /// </param>
        /// <param name="mode">
        /// The mode of selection.
        /// </param>
        /// <returns>
        /// The list of the hits.
        /// </returns>
        public static IEnumerable<RectangleHitResult> FindHits(this Viewport3D viewport, Rect rectangle, SelectionHitMode mode)
        {
            const double Tolerance = 1e-10;

            if (viewport.Camera is not ProjectionCamera camera)
            {
                throw new InvalidOperationException("No projection camera defined. Cannot find rectangle hits.");
            }

            if (rectangle.Width < Tolerance && rectangle.Height < Tolerance)
            {
                IList<HitResult> hitResults = viewport.FindHits(rectangle.BottomLeft);
                return hitResults.Select(x => new RectangleHitResult(x.Model, x.Visual));
            }

            List<RectangleHitResult> results = new();
            viewport.Children.Traverse<GeometryModel3D>(
                (model, visual, transform) =>
                {
                    if (model.Geometry is not MeshGeometry3D geometry || geometry.Positions == null || geometry.TriangleIndices == null)
                    {
                        return;
                    }

                    bool status = mode == SelectionHitMode.Inside;

                    // transform the positions of the mesh to screen coordinates
                    Point[] point2Ds = geometry.Positions.Select(transform.Transform).Select(viewport.Point3DtoPoint2D).ToArray();

                    // evaluate each triangle
                    for (int i = 0; i < geometry.TriangleIndices.Count / 3; i++)
                    {
                        Triangle triangle = new(
                            point2Ds[geometry.TriangleIndices[i * 3]],
                            point2Ds[geometry.TriangleIndices[(i * 3) + 1]],
                            point2Ds[geometry.TriangleIndices[(i * 3) + 2]]);
                        switch (mode)
                        {
                            case SelectionHitMode.Inside:
                                status = status && triangle.IsCompletelyInside(rectangle);
                                break;
                            case SelectionHitMode.Touch:
                                status = status
                                         || triangle.IsCompletelyInside(rectangle)
                                         || triangle.IntersectsWith(rectangle)
                                         || triangle.IsRectCompletelyInside(rectangle);
                                break;
                        }

                        if (mode == SelectionHitMode.Touch && status)
                        {
                            break;
                        }
                    }

                    if (status)
                    {
                        results.Add(new RectangleHitResult(model, visual));
                    }
                });

            return results;
        }

        /// <summary>
        /// Finds the nearest visual, hit point and its normal at the specified position.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <param name="point">
        /// The 3D hit point.
        /// </param>
        /// <param name="normal">
        /// The normal of the mesh at the hit point.
        /// </param>
        /// <param name="visual">
        /// The hit visual.
        /// </param>
        /// <returns>
        /// <c>true</c> if a visual was found at the specified position.
        /// </returns>
        public static bool FindNearest(this Viewport3D viewport, Point position, out Point3D point, out Vector3D normal, out DependencyObject visual)
        {
            if (viewport.Camera is not ProjectionCamera camera)
            {
                point = new Point3D();
                normal = new Vector3D();
                visual = null;
                return false;
            }

            PointHitTestParameters htp = new(position);

            double minimumDistance = double.MaxValue;
            Point3D nearestPoint = new();
            Vector3D nearestNormal = new();
            DependencyObject nearestObject = null;

            VisualTreeHelper.HitTest(
                viewport,
                null,
                delegate (HitTestResult hit)
                {
                    if (hit is RayMeshGeometry3DHitTestResult rayHit)
                    {
                        MeshGeometry3D mesh = rayHit.MeshHit;
                        if (mesh != null)
                        {
                            Point3D p1 = mesh.Positions[rayHit.VertexIndex1];
                            Point3D p2 = mesh.Positions[rayHit.VertexIndex2];
                            Point3D p3 = mesh.Positions[rayHit.VertexIndex3];
                            double x = (p1.X * rayHit.VertexWeight1) + (p2.X * rayHit.VertexWeight2) + (p3.X * rayHit.VertexWeight3);
                            double y = (p1.Y * rayHit.VertexWeight1) + (p2.Y * rayHit.VertexWeight2) + (p3.Y * rayHit.VertexWeight3);
                            double z = (p1.Z * rayHit.VertexWeight1) + (p2.Z * rayHit.VertexWeight2) + (p3.Z * rayHit.VertexWeight3);

                            // point in local coordinates
                            Point3D p = new(x, y, z);

                            // transform to global coordinates

                            // first transform the Model3D hierarchy
                            GeneralTransform3D t2 = rayHit.VisualHit.GetTransformTo(rayHit.ModelHit);
                            if (t2 != null)
                            {
                                p = t2.Transform(p);
                            }

                            // then transform the Visual3D hierarchy up to the Viewport3D ancestor
                            GeneralTransform3D t = viewport.GetTransform(rayHit.VisualHit);
                            if (t != null)
                            {
                                p = t.Transform(p);
                            }

                            double distance = (camera.Position - p).LengthSquared;
                            if (distance < minimumDistance)
                            {
                                minimumDistance = distance;
                                nearestPoint = p;
                                nearestNormal = Vector3D.CrossProduct(p2 - p1, p3 - p1);
                                nearestObject = hit.VisualHit;
                            }
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                htp);

            point = nearestPoint;
            visual = nearestObject;
            normal = nearestNormal;

            if (minimumDistance >= double.MaxValue)
            {
                return false;
            }

            normal.Normalize();
            return true;
        }

        /// <summary>
        /// Finds the coordinates of the nearest point at the specified position.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="position">The position.</param>
        /// <returns>The nearest point, or null if no point was found.</returns>
        public static Point3D? FindNearestPoint(this Viewport3D viewport, Point position)
        {
            return viewport.FindNearest(position, out Point3D p, out _, out _) ? p : null;
        }

        /// <summary>
        /// Finds the Visual3D that is nearest the specified position.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="position">The position.</param>
        /// <returns>
        /// The nearest visual, or <c>null</c> if no visual was found.
        /// </returns>
        public static Visual3D FindNearestVisual(this Viewport3D viewport, Point position)
        {
            return viewport.FindNearest(position, out _, out _, out DependencyObject obj) ? obj as Visual3D : null;
        }

        /// <summary>
        /// Gets the camera transform.
        /// </summary>
        /// <param name="viewport3DVisual">The viewport visual.</param>
        /// <returns>The camera transform.</returns>
        public static Matrix3D GetCameraTransform(this Viewport3DVisual viewport3DVisual)
        {
            return viewport3DVisual.Camera.GetTotalTransform(viewport3DVisual.Viewport.Size.Width / viewport3DVisual.Viewport.Size.Height);
        }

        /// <summary>
        /// Gets the camera transform (viewport and projection).
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>
        /// A <see cref="Matrix3D"/>.
        /// </returns>
        public static Matrix3D GetCameraTransform(this Viewport3D viewport)
        {
            return viewport.Camera.GetTotalTransform(viewport.ActualWidth / viewport.ActualHeight);
        }

        /// <summary>
        /// Gets all lights.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>A sequence of <see cref="Light"/> objects.</returns>
        public static IEnumerable<Light> GetLights(this Viewport3D viewport)
        {
            IList<Model3D> models = viewport.Children.SearchFor<Light>();
            return models.Select(m => m as Light);
        }

        /// <summary>
        /// Gets the ray at the specified position.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="position">
        /// A 2D point.
        /// </param>
        /// <returns>
        /// A <see cref="Ray3D"/>.
        /// </returns>
        public static Ray3D GetRay(this Viewport3D viewport, Point position)
        {
            bool ok = viewport.Point2DtoPoint3D(position, out Point3D point1, out Point3D point2);
            return !ok ? null : new Ray3D { Origin = point1, Direction = point2 - point1 };
        }

        /// <summary>
        /// Gets the total transform (camera and viewport).
        /// </summary>
        /// <param name="viewport3DVisual">The viewport visual.</param>
        /// <returns>The total transform.</returns>
        public static Matrix3D GetTotalTransform(this Viewport3DVisual viewport3DVisual)
        {
            Matrix3D m = viewport3DVisual.GetCameraTransform();
            m.Append(viewport3DVisual.GetViewportTransform());
            return m;
        }

        /// <summary>
        /// Gets the total transform (camera and viewport).
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>The total transform.</returns>
        public static Matrix3D GetTotalTransform(this Viewport3D viewport)
        {
            Matrix3D transform = viewport.GetCameraTransform();
            transform.Append(viewport.GetViewportTransform());
            return transform;
        }

        /// <summary>
        /// Gets the total transform of the specified visual.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="visual">The visual.</param>
        /// <returns>The transform.</returns>
        public static GeneralTransform3D GetTransform(this Viewport3D viewport, Visual3D visual)
        {
            if (visual == null)
            {
                return null;
            }

            foreach (Visual3D ancestor in viewport.Children)
            {
                if (visual.IsDescendantOf(ancestor))
                {
                    GeneralTransform3DGroup g = new();

                    // this includes the visual.Transform
                    GeneralTransform3D ta = visual.TransformToAncestor(ancestor);
                    if (ta != null)
                    {
                        g.Children.Add(ta);
                    }

                    // add the transform of the top-level ancestor
                    g.Children.Add(ancestor.Transform);

                    return g;
                }
            }

            return visual.Transform;
        }

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>A <see cref="Matrix3D"/>.</returns>
        public static Matrix3D GetViewMatrix(this Viewport3D viewport)
        {
            return viewport.Camera.GetViewMatrix();
        }

        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>A <see cref="Matrix3D"/>.</returns>
        public static Matrix3D GetProjectionMatrix(this Viewport3D viewport)
        {
            return viewport.Camera.GetProjectionMatrix(viewport.ActualHeight / viewport.ActualWidth);
        }

        /// <summary>
        /// Gets the viewport transform.
        /// </summary>
        /// <param name="viewport3DVisual">The viewport3DVisual.</param>
        /// <returns>The transform.</returns>
        public static Matrix3D GetViewportTransform(this Viewport3DVisual viewport3DVisual)
        {
            return new Matrix3D(
                viewport3DVisual.Viewport.Width / 2,
                0,
                0,
                0,
                0,
                -viewport3DVisual.Viewport.Height / 2,
                0,
                0,
                0,
                0,
                1,
                0,
                viewport3DVisual.Viewport.X + (viewport3DVisual.Viewport.Width / 2),
                viewport3DVisual.Viewport.Y + (viewport3DVisual.Viewport.Height / 2),
                0,
                1);
        }

        /// <summary>
        /// Gets the viewport transform.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>The transform.</returns>
        public static Matrix3D GetViewportTransform(this Viewport3D viewport)
        {
            return new Matrix3D(
                viewport.ActualWidth / 2,
                0,
                0,
                0,
                0,
                -viewport.ActualHeight / 2,
                0,
                0,
                0,
                0,
                1,
                0,
                viewport.ActualWidth / 2,
                viewport.ActualHeight / 2,
                0,
                1);
        }

        /// <summary>
        /// Transforms a position to Point3D at the near and far clipping planes.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="pointIn">The point to transform.</param>
        /// <param name="pointNear">The point at the near clipping plane.</param>
        /// <param name="pointFar">The point at the far clipping plane.</param>
        /// <returns>True if points were found.</returns>
        public static bool Point2DtoPoint3D(this Viewport3D viewport, Point pointIn, out Point3D pointNear, out Point3D pointFar)
        {
            pointNear = new Point3D();
            pointFar = new Point3D();

            Point3D pointIn3D = new(pointIn.X, pointIn.Y, 0);
            Matrix3D matrixViewport = viewport.GetViewportTransform();
            Matrix3D matrixCamera = viewport.GetCameraTransform();

            if (!matrixViewport.HasInverse)
            {
                return false;
            }

            if (!matrixCamera.HasInverse)
            {
                return false;
            }

            matrixViewport.Invert();
            matrixCamera.Invert();

            Point3D pointNormalized = matrixViewport.Transform(pointIn3D);
            pointNormalized.Z = 0.01;
            pointNear = matrixCamera.Transform(pointNormalized);
            pointNormalized.Z = 0.99;
            pointFar = matrixCamera.Transform(pointNormalized);

            return true;
        }

        /// <summary>
        /// Transforms a 2D point to a ray.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="pointIn">The point.</param>
        /// <returns>The ray.</returns>
        public static Ray3D Point2DtoRay3D(this Viewport3D viewport, Point pointIn)
        {
            return !viewport.Point2DtoPoint3D(pointIn, out Point3D pointNear, out Point3D pointFar) ? null : new Ray3D(pointNear, pointFar);
        }

        /// <summary>
        /// Transforms the Point3D to a Point2D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="point">The 3D point.</param>
        /// <returns>The point.</returns>
        public static Point Point3DtoPoint2D(this Viewport3D viewport, Point3D point)
        {
            Matrix3D matrix = viewport.GetTotalTransform();
            Point3D pointTransformed = matrix.Transform(point);
            Point pt = new(pointTransformed.X, pointTransformed.Y);
            return pt;
        }

        /// <summary>
        /// Transforms the set of Point3D to a set of Point2D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="points">The set of 3D points.</param>
        /// <returns>The transformed points.</returns>
        public static IEnumerable<Point> Point3DtoPoint2D(this Viewport3D viewport, IEnumerable<Point3D> points)
        {
            Matrix3D matrix = viewport.GetTotalTransform();
            IEnumerable<Point3D> pointsTransformed = points.Select(matrix.Transform);
            return pointsTransformed.Select(point => new Point(point.X, point.Y));
        }

        /// <summary>
        /// Prints the specified viewport.
        /// </summary>
        /// <param name="vp">
        /// The viewport.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public static void Print(this Viewport3D vp, string description)
        {
            PrintDialog dlg = new();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                dlg.PrintVisual(vp, description);
            }
        }

        /// <summary>
        /// Renders the viewport to a bitmap.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="background">The background.</param>
        /// <param name="m">The oversampling multiplier.</param>
        /// <returns>A bitmap.</returns>
        public static BitmapSource RenderBitmap(this Viewport3D view, Brush background, int m = 1)
        {
            WriteableBitmap target = new((int)view.ActualWidth * m, (int)view.ActualHeight * m, 96, 96, PixelFormats.Pbgra32, null);

            System.Windows.Media.Media3D.Camera originalCamera = view.Camera;
            Matrix3D vm = originalCamera.GetViewMatrix();
            double ar = view.ActualWidth / view.ActualHeight;

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    // change the camera viewport and scaling
                    Matrix3D pm = originalCamera.GetProjectionMatrix(ar);
                    if (originalCamera is OrthographicCamera)
                    {
                        pm.OffsetX = m - 1 - (i * 2);
                        pm.OffsetY = -(m - 1 - (j * 2));
                    }

                    if (originalCamera is PerspectiveCamera)
                    {
                        pm.M31 = -(m - 1 - (i * 2));
                        pm.M32 = m - 1 - (j * 2);
                    }

                    pm.M11 *= m;
                    pm.M22 *= m;

                    MatrixCamera mc = new(vm, pm);
                    view.Camera = mc;

                    RenderTargetBitmap partialBitmap = new((int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32);

                    // render background
                    Rectangle backgroundRectangle = new() { Width = partialBitmap.Width, Height = partialBitmap.Height, Fill = background };
                    backgroundRectangle.Arrange(new Rect(0, 0, backgroundRectangle.Width, backgroundRectangle.Height));
                    partialBitmap.Render(backgroundRectangle);

                    // render 3d
                    partialBitmap.Render(view);

                    // copy to the target bitmap
                    CopyBitmap(partialBitmap, target, (int)(i * view.ActualWidth), (int)(j * view.ActualHeight));
                }
            }

            // restore the camera
            view.Camera = originalCamera;
            return target;
        }

        /// <summary>
        /// Renders the viewport to a bitmap.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="background">The background.</param>
        /// <param name="m">The oversampling multiplier.</param>
        /// <returns>A bitmap.</returns>
        public static BitmapSource RenderBitmap(this Viewport3D view, double width, double height, Brush background, int m = 1)
        {
            double w = view.Width;
            double h = view.Height;
            view.ResizeAndArrange(width, height);
            BitmapSource rtb = view.RenderBitmap(background, m);
            view.ResizeAndArrange(w, h);
            return rtb;
        }

        /// <summary>
        /// Resizes and arranges the viewport.
        /// </summary>
        /// <param name="view">
        /// The viewport.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        public static void ResizeAndArrange(this Viewport3D view, double width, double height)
        {
            view.Width = width;
            view.Height = height;
            if (double.IsNaN(width) || double.IsNaN(height))
            {
                return;
            }

            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
        }

        ///// <summary>
        ///// Saves the viewport to a file.
        ///// </summary>
        ///// <param name="view">The viewport.</param>
        ///// <param name="fileName">Name of the file.</param>
        ///// <param name="background">The background brush.</param>
        ///// <param name="m">The oversampling multiplier.</param>
        ///// <param name="format">The output format.</param>
        //public static void SaveBitmap(this Viewport3D view, string fileName, Brush background = null, int m = 1, BitmapExporter.OutputFormat format = BitmapExporter.OutputFormat.Png)
        //{
        //    using (var stream = File.Create(fileName))
        //    {
        //        SaveBitmap(view, stream, background, m, format);
        //    }
        //}

        ///// <summary>
        ///// Saves the <see cref="Viewport3D"/> to left/right bitmap files.
        ///// </summary>
        ///// <param name="view">The viewport.</param>
        ///// <param name="fileName">Name of the file. "_L" and "_R" will be appended to the file name.</param>
        ///// <param name="stereoBase">The stereo base.</param>
        ///// <param name="background">The background brush.</param>
        ///// <param name="m">The oversampling multiplier.</param>
        //public static void SaveStereoBitmap(this Viewport3D view, string fileName, double stereoBase, Brush background = null, int m = 1)
        //{
        //    var extension = System.IO.Path.GetExtension(fileName);
        //    var directory = System.IO.Path.GetDirectoryName(fileName) ?? string.Empty;
        //    var name = System.IO.Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
        //    var leftFileName = System.IO.Path.Combine(directory, name) + "_L" + extension;
        //    var rightFileName = System.IO.Path.Combine(directory, name) + "_R" + extension;

        //    var centerCamera = view.Camera as PerspectiveCamera;
        //    var leftCamera = new PerspectiveCamera();
        //    var rightCamera = new PerspectiveCamera();
        //    StereoHelper.UpdateStereoCameras(centerCamera, leftCamera, rightCamera, stereoBase);

        //    // save the left image
        //    using (var stream = File.Create(leftFileName))
        //    {
        //        view.Camera = leftCamera;
        //        view.SaveBitmap(stream, background, m);
        //    }

        //    // save the right image
        //    using (var stream = File.Create(rightFileName))
        //    {
        //        view.Camera = rightCamera;
        //        view.SaveBitmap(stream, background, m);
        //    }

        //    // restore original camera
        //    view.Camera = centerCamera;
        //}

        ///// <summary>
        ///// Saves the <see cref="Viewport3D" /> to a bitmap.
        ///// </summary>
        ///// <param name="view">The view.</param>
        ///// <param name="stream">The output stream.</param>
        ///// <param name="background">The background brush.</param>
        ///// <param name="m">The oversampling multiplier.</param>
        ///// <param name="format">The output format.</param>
        //public static void SaveBitmap(this Viewport3D view, Stream stream, Brush background = null, int m = 1, BitmapExporter.OutputFormat format = BitmapExporter.OutputFormat.Png)
        //{
        //    var exporter = new BitmapExporter { Background = background, OversamplingMultiplier = m, Format = format };
        //    exporter.Export(view, stream);
        //}

        /// <summary>
        /// Recursive search in a Visual3D collection for objects of given type T
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>A list of models.</returns>
        public static IList<Model3D> SearchFor<T>(this IEnumerable<Visual3D> collection)
        {
            List<Model3D> output = new();
            SearchFor(collection, typeof(T), output);
            return output;
        }

        /// <summary>
        /// Transforms a point from the screen (2D) to a point on plane (3D) 平行于相机的平面上的交点
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="p">
        /// The 2D point.
        /// </param>
        /// <param name="position">
        /// A point in the plane.
        /// </param>
        /// <param name="normal">
        /// The plane normal.
        /// </param>
        /// <returns>
        /// A 3D point.
        /// </returns>
        /// <remarks>
        /// Maps window coordinates to object coordinates like <code>gluUnProject</code>.
        /// </remarks>
        public static Point3D? UnProject(this Viewport3D viewport, Point p, Point3D position, Vector3D normal)
        {
            Ray3D ray = viewport.GetRay(p);
            return ray == null ? null : ray.PlaneIntersection(position, normal, out Point3D i) ? i : null;
        }

        /// <summary>
        /// Transforms a point from the screen (2D) to a point on the plane trough the camera target point.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="p">
        /// The 2D point.
        /// </param>
        /// <returns>
        /// A 3D point.
        /// </returns>
        public static Point3D? UnProject(this Viewport3D viewport, Point p)
        {
            return viewport.Camera is not ProjectionCamera pc ? null : viewport.UnProject(p, pc.Position + pc.LookDirection, pc.LookDirection);
        }

        /// <summary>
        /// Gets the total number of triangles in the viewport.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>The total number of triangles</returns>
        public static int GetTotalNumberOfTriangles(this Viewport3D viewport)
        {
            int count = 0;
            viewport.Children.Traverse<GeometryModel3D>((m, t) =>
                {
                    if (m.Geometry is MeshGeometry3D geometry && geometry.TriangleIndices != null)
                    {
                        count += geometry.TriangleIndices.Count / 3;
                    }
                });
            return count;
        }

        /// <summary>
        /// Copies the bitmap.
        /// </summary>
        /// <param name="source">The source bitmap.</param>
        /// <param name="target">The target bitmap.</param>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        private static void CopyBitmap(BitmapSource source, WriteableBitmap target, int x, int y)
        {
            // Calculate stride of source
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);

            // Create data array to hold source pixel data
            byte[] data = new byte[stride * source.PixelHeight];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Write the pixel data to the bitmap.
            target.WritePixels(new Int32Rect(x, y, source.PixelWidth, source.PixelHeight), data, stride, 0);
        }

        ///// <summary>
        ///// Exports the model to a Kerkythea file.
        ///// </summary>
        ///// <param name="view">The viewport.</param>
        ///// <param name="fileName">Name of the file.</param>
        ///// <param name="background">The background.</param>
        //private static void ExportKerkythea(this Viewport3D view, string fileName, Brush background)
        //{
        //    ExportKerkythea(view, fileName, background, (int)view.ActualWidth, (int)view.ActualHeight);
        //}

        ///// <summary>
        ///// Exports the model to a Kerkythea file.
        ///// </summary>
        ///// <param name="view">
        ///// The viewport.
        ///// </param>
        ///// <param name="fileName">
        ///// Name of the file.
        ///// </param>
        ///// <param name="background">
        ///// The background.
        ///// </param>
        ///// <param name="width">
        ///// The width.
        ///// </param>
        ///// <param name="height">
        ///// The height.
        ///// </param>
        //private static void ExportKerkythea(this Viewport3D view, string fileName, Brush background, int width, int height)
        //{
        //    var scb = background as SolidColorBrush;
        //    var backgroundColor = scb != null ? scb.Color : Colors.White;
        //    var e = new KerkytheaExporter
        //    {
        //        Width = width,
        //        Height = height,
        //        BackgroundColor = backgroundColor,
        //        TexturePath = System.IO.Path.GetDirectoryName(fileName)
        //    };
        //    using (var stream = File.Create(fileName))
        //    {
        //        e.Export(view, stream);
        //    }
        //}

        ///// <summary>
        ///// Exports to an obj file.
        ///// </summary>
        ///// <param name="view">
        ///// The viewport.
        ///// </param>
        ///// <param name="path">
        ///// Name of the file.
        ///// </param>
        //private static void ExportObj(this Viewport3D view, string path)
        //{
        //    var dir = System.IO.Path.GetDirectoryName(path) ?? ".";
        //    var filename = System.IO.Path.GetFileName(path);
        //    var e = new ObjExporter
        //    {
        //        TextureFolder = dir,
        //        FileCreator = f => File.Create(System.IO.Path.Combine(dir, f))
        //    };
        //    using (var stream = File.Create(path))
        //    {
        //        e.MaterialsFile = System.IO.Path.ChangeExtension(filename, ".mtl");
        //        e.Export(view, stream);
        //    }
        //}

        ///// <summary>
        ///// Exports to an X3D file.
        ///// </summary>
        ///// <param name="view">
        ///// The viewport.
        ///// </param>
        ///// <param name="fileName">
        ///// Name of the file.
        ///// </param>
        //private static void ExportX3D(this Viewport3D view, string fileName)
        //{
        //    var e = new X3DExporter();
        //    using (var stream = File.Create(fileName))
        //    {
        //        e.Export(view, stream);
        //    }
        //}

        ///// <summary>
        ///// Exports to a COLLADA file.
        ///// </summary>
        ///// <param name="view">The viewport.</param>
        ///// <param name="fileName">Name of the file.</param>
        //private static void ExportCollada(this Viewport3D view, string fileName)
        //{
        //    var e = new ColladaExporter();
        //    using (var stream = File.Create(fileName))
        //    {
        //        e.Export(view, stream);
        //    }
        //}

        ///// <summary>
        ///// Exports to a STL file.
        ///// </summary>
        ///// <param name="view">The viewport.</param>
        ///// <param name="fileName">Name of the file.</param>
        //private static void ExportStl(this Viewport3D view, string fileName)
        //{
        //    var e = new StlExporter();
        //    using (var stream = File.Create(fileName))
        //    {
        //        e.Export(view, stream);
        //    }
        //}

        ///// <summary>
        ///// Exports to xaml.
        ///// </summary>
        ///// <param name="view">
        ///// The viewport.
        ///// </param>
        ///// <param name="fileName">
        ///// Name of the file.
        ///// </param>
        //private static void ExportXaml(this Viewport3D view, string fileName)
        //{
        //    var e = new XamlExporter();
        //    using (var stream = File.Create(fileName))
        //    {
        //        e.Export(view, stream);
        //    }
        //}

        /// <summary>
        /// Gets the hit position transformed to global (viewport) coordinates.
        /// </summary>
        /// <param name="rayHit">
        /// The hit structure.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>
        /// The 3D position of the hit.
        /// </returns>
        private static Point3D GetGlobalHitPosition(RayHitTestResult rayHit, Viewport3D viewport)
        {
            // PointHit is in Visual3D space
            Point3D p = rayHit.PointHit;

            // transform the Visual3D hierarchy up to the Viewport3D ancestor
            GeneralTransform3D t = viewport.GetTransform(rayHit.VisualHit);
            if (t != null)
            {
                p = t.Transform(p);
            }

            return p;
        }

        /// <summary>
        /// Gets the normal for a hit test result.
        /// </summary>
        /// <param name="rayHit">
        /// The ray hit.
        /// </param>
        /// <returns>
        /// The normal.
        /// </returns>
        private static Vector3D? GetNormalHit(RayMeshGeometry3DHitTestResult rayHit)
        {
            return rayHit.MeshHit.Normals == null || rayHit.MeshHit.Normals.Count < 1
                ? null
                : (rayHit.MeshHit.Normals[rayHit.VertexIndex1] * rayHit.VertexWeight1)
                   + (rayHit.MeshHit.Normals[rayHit.VertexIndex2] * rayHit.VertexWeight2)
                   + (rayHit.MeshHit.Normals[rayHit.VertexIndex3] * rayHit.VertexWeight3);
        }

        /// <summary>
        /// Recursive search for an object of a given type
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="type">The type.</param>
        /// <param name="output">The output.</param>
        private static void SearchFor(IEnumerable<Visual3D> collection, Type type, IList<Model3D> output)
        {
            // TODO: change to use Stack/Queue
            foreach (Visual3D visual in collection)
            {
                if (visual is ModelVisual3D modelVisual)
                {
                    Model3D model = modelVisual.Content;
                    if (model != null)
                    {
                        if (type.IsInstanceOfType(model))
                        {
                            output.Add(model);
                        }

                        // recursive
                        SearchFor(modelVisual.Children, type, output);
                    }

                    if (model is Model3DGroup modelGroup)
                    {
                        SearchFor(modelGroup.Children, type, output);
                    }
                }
            }
        }

        /// <summary>
        /// Searches for models of the specified type.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        private static void SearchFor(IEnumerable<Model3D> collection, Type type, IList<Model3D> output)
        {
            foreach (Model3D model in collection)
            {
                if (type.IsInstanceOfType(model))
                {
                    output.Add(model);
                }

                if (model is Model3DGroup group)
                {
                    SearchFor(group.Children, type, output);
                }
            }
        }

        /// <summary>
        /// Represents a rectangle hit result.
        /// </summary>
        public class RectangleHitResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RectangleHitResult" /> class.
            /// </summary>
            /// <param name="model">The hit model.</param>
            /// <param name="visual">The hit visual.</param>
            public RectangleHitResult(Model3D model, Visual3D visual)
            {
                this.Model = model;
                this.Visual = visual;
            }

            /// <summary>
            /// Gets the hit model.
            /// </summary>
            public Model3D Model { get; private set; }

            /// <summary>
            /// Gets the hit visual.
            /// </summary>
            public Visual3D Visual { get; private set; }
        }

        /// <summary>
        /// A hit result.
        /// </summary>
        public class HitResult
        {
            /// <summary>
            /// Gets or sets the distance.
            /// </summary>
            /// <value>The distance.</value>
            public double Distance { get; set; }

            /// <summary>
            /// Gets the mesh.
            /// </summary>
            /// <value>The mesh.</value>
            public MeshGeometry3D Mesh => this.RayHit.MeshHit;

            /// <summary>
            /// Gets the model.
            /// </summary>
            /// <value>The model.</value>
            public Model3D Model => this.RayHit.ModelHit;

            /// <summary>
            /// Gets or sets the normal.
            /// </summary>
            /// <value>The normal.</value>
            public Vector3D Normal { get; set; }

            /// <summary>
            /// Gets or sets the position.
            /// </summary>
            /// <value>The position.</value>
            public Point3D Position { get; set; }

            /// <summary>
            /// Gets or sets the ray hit.
            /// </summary>
            /// <value>The ray hit.</value>
            public RayMeshGeometry3DHitTestResult RayHit { get; set; }

            /// <summary>
            /// Gets the visual.
            /// </summary>
            /// <value>The visual.</value>
            public Visual3D Visual => this.RayHit.VisualHit;
        }
    }
}