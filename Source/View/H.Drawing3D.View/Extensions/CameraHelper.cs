// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraHelper.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides extension methods for Camera derived classes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.View.Privider;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Camera"/> derived classes.
    /// </summary>
    public static class CameraHelper
    {
        /// <summary>
        /// Animates the camera position and directions.
        /// </summary>
        /// <param name="camera">
        /// The camera to animate.
        /// </param>
        /// <param name="newPosition">
        /// The position to animate to.
        /// </param>
        /// <param name="newDirection">
        /// The direction to animate to.
        /// </param>
        /// <param name="newUpDirection">
        /// The up direction to animate to.
        /// </param>
        /// <param name="animationTime">
        /// Animation time in milliseconds.
        /// </param>
        public static void AnimateTo(this ProjectionCamera camera, Point3D newPosition, Vector3D newDirection, Vector3D newUpDirection, double animationTime)
        {
            Point3D fromPosition = camera.Position;
            Vector3D fromDirection = camera.LookDirection;
            Vector3D fromUpDirection = camera.UpDirection;

            camera.Position = newPosition;
            camera.LookDirection = newDirection;
            camera.UpDirection = newUpDirection;

            if (animationTime > 0)
            {
                Point3DAnimation a1 = new(
                    fromPosition, newPosition, new Duration(TimeSpan.FromMilliseconds(animationTime)))
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                    FillBehavior = FillBehavior.Stop
                };
                a1.Completed += (s, a) => camera.BeginAnimation(ProjectionCamera.PositionProperty, null);
                camera.BeginAnimation(ProjectionCamera.PositionProperty, a1);

                Vector3DAnimation a2 = new(
                    fromDirection, newDirection, new Duration(TimeSpan.FromMilliseconds(animationTime)))
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                    FillBehavior = FillBehavior.Stop
                };
                a2.Completed += (s, a) => camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, null);
                camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, a2);

                Vector3DAnimation a3 = new(
                    fromUpDirection, newUpDirection, new Duration(TimeSpan.FromMilliseconds(animationTime)))
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                    FillBehavior = FillBehavior.Stop
                };
                a3.Completed += (s, a) => camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, null);
                camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, a3);
            }
        }

        /// <summary>
        /// Animates the orthographic width.
        /// </summary>
        /// <param name="camera">
        /// An orthographic camera.
        /// </param>
        /// <param name="newWidth">
        /// The width to animate to.
        /// </param>
        /// <param name="animationTime">
        /// Animation time in milliseconds
        /// </param>
        public static void AnimateWidth(this OrthographicCamera camera, double newWidth, double animationTime)
        {
            double fromWidth = camera.Width;

            camera.Width = newWidth;

            if (animationTime > 0)
            {
                DoubleAnimation a1 = new(
                    fromWidth, newWidth, new Duration(TimeSpan.FromMilliseconds(animationTime)))
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                    FillBehavior = FillBehavior.Stop
                };
                camera.BeginAnimation(OrthographicCamera.WidthProperty, a1);
            }
        }

        /// <summary>
        /// Changes the direction of a camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="newLookDirection">
        /// The new look direction.
        /// </param>
        /// <param name="newUpDirection">
        /// The new up direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ChangeDirection(this ProjectionCamera camera, Vector3D newLookDirection, Vector3D newUpDirection, double animationTime)
        {
            Point3D target = camera.Position + camera.LookDirection;
            double length = camera.LookDirection.Length;
            newLookDirection.Normalize();
            camera.LookAt(target, newLookDirection * length, newUpDirection, animationTime);
        }

        /// <summary>
        /// Copies the specified camera, converts field of view/width if necessary.
        /// </summary>
        /// <param name="source">The source camera.</param>
        /// <param name="dest">The destination camera.</param>
        /// <param name="copyNearFarPlaneDistances">Copy near and far plane distances if set to <c>true</c>.</param>
        public static void Copy(this ProjectionCamera source, ProjectionCamera dest, bool copyNearFarPlaneDistances = true)
        {
            if (source == null || dest == null)
            {
                return;
            }

            dest.LookDirection = source.LookDirection;
            dest.Position = source.Position;
            dest.UpDirection = source.UpDirection;

            if (copyNearFarPlaneDistances)
            {
                dest.NearPlaneDistance = source.NearPlaneDistance;
                dest.FarPlaneDistance = source.FarPlaneDistance;
            }

            PerspectiveCamera psrc = source as PerspectiveCamera;
            OrthographicCamera osrc = source as OrthographicCamera;
            if (dest is PerspectiveCamera pdest)
            {
                double fov = 45;
                if (psrc != null)
                {
                    fov = psrc.FieldOfView;
                }

                if (osrc != null)
                {
                    double dist = source.LookDirection.Length;
                    fov = Math.Atan(osrc.Width / 2 / dist) * 180 / Math.PI * 2;
                }

                pdest.FieldOfView = fov;
            }

            if (dest is OrthographicCamera odest)
            {
                double width = 100;
                if (psrc != null)
                {
                    double dist = source.LookDirection.Length;
                    width = Math.Tan(psrc.FieldOfView / 180 * Math.PI / 2) * dist * 2;
                }

                if (osrc != null)
                {
                    width = osrc.Width;
                }

                odest.Width = width;
            }
        }

        /// <summary>
        /// Copy the direction of the source <see cref="Camera"/>. Used for the CoordinateSystem view.
        /// </summary>
        /// <param name="source">
        /// The source camera.
        /// </param>
        /// <param name="dest">
        /// The destination camera.
        /// </param>
        /// <param name="distance">
        /// New length of the LookDirection vector.
        /// </param>
        public static void CopyDirectionOnly(this ProjectionCamera source, ProjectionCamera dest, double distance)
        {
            if (source == null || dest == null)
            {
                return;
            }

            Vector3D dir = source.LookDirection;
            dir.Normalize();
            dir *= distance;

            dest.LookDirection = dir;
            dest.Position = new Point3D(-dest.LookDirection.X, -dest.LookDirection.Y, -dest.LookDirection.Z);
            dest.UpDirection = source.UpDirection;
        }

        /// <summary>
        /// Creates a default perspective camera.
        /// </summary>
        /// <returns>A perspective camera.</returns>
        public static PerspectiveCamera CreateDefaultCamera()
        {
            PerspectiveCamera camera = new();
            camera.Reset();
            return camera;
        }

        /// <summary>
        /// Gets an information string about the specified camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <returns>
        /// The get info.
        /// </returns>
        public static string GetInfo(this Camera camera)
        {
            StringBuilder sb = new();
            _ = sb.AppendLine(camera.GetType().Name);
            if (camera is ProjectionCamera projectionCamera)
            {
                _ = sb.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "LookDirection:\t{0:0.000},{1:0.000},{2:0.000}",
                        projectionCamera.LookDirection.X,
                        projectionCamera.LookDirection.Y,
                        projectionCamera.LookDirection.Z));
                _ = sb.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "UpDirection:\t{0:0.000},{1:0.000},{2:0.000}",
                        projectionCamera.UpDirection.X,
                        projectionCamera.UpDirection.Y,
                        projectionCamera.UpDirection.Z));
                _ = sb.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Position:\t\t{0:0.000},{1:0.000},{2:0.000}",
                        projectionCamera.Position.X,
                        projectionCamera.Position.Y,
                        projectionCamera.Position.Z));
                Point3D target = projectionCamera.Position + projectionCamera.LookDirection;
                _ = sb.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Target:\t\t{0:0.000},{1:0.000},{2:0.000}",
                        target.X,
                        target.Y,
                        target.Z));
                _ = sb.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture, "NearPlaneDist:\t{0}", projectionCamera.NearPlaneDistance));
                _ = sb.AppendLine(
                    string.Format(CultureInfo.InvariantCulture, "FarPlaneDist:\t{0}", projectionCamera.FarPlaneDistance));
            }

            if (camera is PerspectiveCamera perspectiveCamera)
            {
                _ = sb.AppendLine(
                    string.Format(CultureInfo.InvariantCulture, "FieldOfView:\t{0:0.#}°", perspectiveCamera.FieldOfView));
            }

            if (camera is OrthographicCamera orthographicCamera)
            {
                _ = sb.AppendLine(
                    string.Format(CultureInfo.InvariantCulture, "Width:\t{0:0.###}", orthographicCamera.Width));
            }

            if (camera is MatrixCamera matrixCamera)
            {
                _ = sb.AppendLine("ProjectionMatrix:");
                _ = sb.AppendLine(matrixCamera.ProjectionMatrix.ToString(CultureInfo.InvariantCulture));
                _ = sb.AppendLine("ViewMatrix:");
                _ = sb.AppendLine(matrixCamera.ViewMatrix.ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Set the camera target point without changing the look direction.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(this ProjectionCamera camera, Point3D target, double animationTime)
        {
            camera.LookAt(target, camera.LookDirection, animationTime);
        }

        /// <summary>
        /// Set the camera target point and look direction
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="newLookDirection">
        /// The new look direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(this ProjectionCamera camera, Point3D target, Vector3D newLookDirection, double animationTime)
        {
            camera.LookAt(target, newLookDirection, camera.UpDirection, animationTime);
        }

        /// <summary>
        /// Set the camera target point and directions
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="newLookDirection">
        /// The new look direction.
        /// </param>
        /// <param name="newUpDirection">
        /// The new up direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(this ProjectionCamera camera, Point3D target, Vector3D newLookDirection, Vector3D newUpDirection, double animationTime)
        {
            Point3D newPosition = target - newLookDirection;

            if (camera is PerspectiveCamera perspectiveCamera)
            {
                perspectiveCamera.AnimateTo(newPosition, newLookDirection, newUpDirection, animationTime);
                return;
            }

            OrthographicCamera orthographicCamera = camera as OrthographicCamera;
            orthographicCamera?.AnimateTo(newPosition, newLookDirection, newUpDirection, animationTime);
        }

        /// <summary>
        /// Set the camera target point and camera distance.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="target">
        /// The target point.
        /// </param>
        /// <param name="distance">
        /// The distance to the camera.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void LookAt(this ProjectionCamera camera, Point3D target, double distance, double animationTime)
        {
            Vector3D d = camera.LookDirection;
            d.Normalize();
            camera.LookAt(target, d * distance, animationTime);
        }

        /// <summary>
        /// Resets the specified camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        public static void Reset(this Camera camera)
        {
            PerspectiveCamera pcamera = camera as PerspectiveCamera;
            pcamera?.Reset();

            OrthographicCamera ocamera = camera as OrthographicCamera;
            ocamera?.Reset();
        }

        /// <summary>
        /// Resets the specified perspective camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        public static void Reset(this PerspectiveCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.Position = new Point3D(2, 16, 20);
            camera.LookDirection = new Vector3D(-2, -16, -20);
            camera.UpDirection = new Vector3D(0, 0, 1);
            camera.FieldOfView = 45;
            camera.NearPlaneDistance = 0.1;
            camera.FarPlaneDistance = double.PositiveInfinity;
        }

        /// <summary>
        /// Resets the specified orthographic camera.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        public static void Reset(this OrthographicCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.Position = new Point3D(2, 16, 20);
            camera.LookDirection = new Vector3D(-2, -16, -20);
            camera.UpDirection = new Vector3D(0, 0, 1);
            camera.Width = 40;
            camera.NearPlaneDistance = 0.1;
            camera.FarPlaneDistance = double.PositiveInfinity;
        }

        /// <summary>
        /// Obtains the view transform matrix for a camera. (see page 327)
        /// </summary>
        /// <param name="camera">
        /// Camera to obtain the ViewMatrix for
        /// </param>
        /// <returns>
        /// A Matrix3D object with the camera view transform matrix, or a Matrix3D with all zeros if the "camera" is null.
        /// </returns>
        public static Matrix3D GetViewMatrix(this Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (camera is MatrixCamera matrixCamera)
            {
                return matrixCamera.ViewMatrix;
            }

            if (camera is ProjectionCamera projectionCamera)
            {
                Vector3D zaxis = -projectionCamera.LookDirection;
                zaxis.Normalize();

                Vector3D xaxis = Vector3D.CrossProduct(projectionCamera.UpDirection, zaxis);
                xaxis.Normalize();

                Vector3D yaxis = Vector3D.CrossProduct(zaxis, xaxis);
                Vector3D pos = (Vector3D)projectionCamera.Position;

                return new Matrix3D(
                    xaxis.X,
                    yaxis.X,
                    zaxis.X,
                    0,
                    xaxis.Y,
                    yaxis.Y,
                    zaxis.Y,
                    0,
                    xaxis.Z,
                    yaxis.Z,
                    zaxis.Z,
                    0,
                    -Vector3D.DotProduct(xaxis, pos),
                    -Vector3D.DotProduct(yaxis, pos),
                    -Vector3D.DotProduct(zaxis, pos),
                    1);
            }

            throw new HelixToolkitException("Unknown camera type.");
        }

        /// <summary>
        /// Gets the projection matrix for the specified camera.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <returns>The projection matrix.</returns>
        public static Matrix3D GetProjectionMatrix(this Camera camera, double aspectRatio)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (camera is PerspectiveCamera perspectiveCamera)
            {
                // The angle-to-radian formula is a little off because only
                // half the angle enters the calculation.
                double xscale = 1 / Math.Tan(Math.PI * perspectiveCamera.FieldOfView / 360);
                double yscale = xscale * aspectRatio;
                double znear = perspectiveCamera.NearPlaneDistance;
                double zfar = perspectiveCamera.FarPlaneDistance;
                double zscale = double.IsPositiveInfinity(zfar) ? -1 : zfar / (znear - zfar);
                double zoffset = znear * zscale;

                return new Matrix3D(xscale, 0, 0, 0, 0, yscale, 0, 0, 0, 0, zscale, -1, 0, 0, zoffset, 0);
            }

            if (camera is OrthographicCamera orthographicCamera)
            {
                double xscale = 2.0 / orthographicCamera.Width;
                double yscale = xscale * aspectRatio;
                double znear = orthographicCamera.NearPlaneDistance;
                double zfar = orthographicCamera.FarPlaneDistance;

                if (double.IsPositiveInfinity(zfar))
                {
                    zfar = znear * 1e5;
                }

                double dzinv = 1.0 / (znear - zfar);

                Matrix3D m = new(xscale, 0, 0, 0, 0, yscale, 0, 0, 0, 0, dzinv, 0, 0, 0, znear * dzinv, 1);
                return m;
            }

            return camera is MatrixCamera matrixCamera ? matrixCamera.ProjectionMatrix : throw new HelixToolkitException("Unknown camera type.");
        }

        /// <summary>
        /// Gets the combined view and projection transform.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <returns>The total view and projection transform.</returns>
        public static Matrix3D GetTotalTransform(this Camera camera, double aspectRatio)
        {
            Matrix3D m = Matrix3D.Identity;

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (camera.Transform != null)
            {
                Matrix3D cameraTransform = camera.Transform.Value;

                if (!cameraTransform.HasInverse)
                {
                    throw new HelixToolkitException("Camera transform has no inverse.");
                }

                cameraTransform.Invert();
                m.Append(cameraTransform);
            }

            m.Append(camera.GetViewMatrix());
            m.Append(camera.GetProjectionMatrix(aspectRatio));
            return m;
        }

        /// <summary>
        /// Gets the inverse camera transform.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="aspectRatio">
        /// The aspect ratio.
        /// </param>
        /// <returns>
        /// The inverse transform.
        /// </returns>
        public static Matrix3D GetInverseTransform(this Camera camera, double aspectRatio)
        {
            Matrix3D m = camera.GetTotalTransform(aspectRatio);

            if (!m.HasInverse)
            {
                throw new HelixToolkitException("Camera transform has no inverse.");
            }

            m.Invert();
            return m;
        }

        /// <summary>
        /// Fits the current scene in the current view.
        /// </summary>
        /// <param name="camera">The actual camera.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="animationTime">The animation time.</param>
        public static void FitView(
            this ProjectionCamera camera,
            Viewport3D viewport,
            double animationTime = 0)
        {
            if (camera is PerspectiveCamera perspectiveCamera)
            {
                camera.FitView(viewport, perspectiveCamera.LookDirection, perspectiveCamera.UpDirection, animationTime);
                return;
            }

            if (camera is OrthographicCamera orthoCamera)
            {
                camera.FitView(viewport, orthoCamera.LookDirection, orthoCamera.UpDirection, animationTime);
            }
        }

        /// <summary>
        /// Fits the current scene in the current view.
        /// </summary>
        /// <param name="camera">The actual camera.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="lookDirection">The look direction.</param>
        /// <param name="upDirection">The up direction.</param>
        /// <param name="animationTime">The animation time.</param>
        public static void FitView(this ProjectionCamera camera, Viewport3D viewport, Vector3D lookDirection, Vector3D upDirection, double animationTime = 0)
        {
            Rect3D bounds = viewport.Children.FindBounds();
            Vector3D diagonal = new(bounds.SizeX, bounds.SizeY, bounds.SizeZ);

            if (bounds.IsEmpty || diagonal.LengthSquared < double.Epsilon)
            {
                return;
            }

            camera.FitView(viewport, bounds, lookDirection, upDirection, animationTime);
        }

        /// <summary>
        /// Zooms to fit the extents of the specified viewport.
        /// </summary>
        /// <param name="camera">
        /// The actual camera.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ZoomExtents(this ProjectionCamera camera, Viewport3D viewport, double animationTime = 0)
        {
            Rect3D bounds = viewport.Children.FindBounds();
            Vector3D diagonal = new(bounds.SizeX, bounds.SizeY, bounds.SizeZ);

            if (bounds.IsEmpty || diagonal.LengthSquared < double.Epsilon)
            {
                return;
            }

            camera.ZoomExtents(viewport, bounds, animationTime);
        }

        /// <summary>
        /// Zooms to fit the specified bounding rectangle.
        /// </summary>
        /// <param name="camera">
        /// The camera to change.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="bounds">
        /// The bounding rectangle.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ZoomExtents(
            this ProjectionCamera camera,
            Viewport3D viewport,
            Rect3D bounds,
            double animationTime = 0)
        {
            if (camera is PerspectiveCamera perspectiveCamera)
            {
                camera.FitView(viewport, bounds, perspectiveCamera.LookDirection, perspectiveCamera.UpDirection, animationTime);
                return;
            }

            if (camera is OrthographicCamera orthoCamera)
            {
                camera.FitView(viewport, bounds, orthoCamera.LookDirection, orthoCamera.UpDirection, animationTime);
            }
        }

        /// <summary>
        /// Fits the specified bounding rectangle in the current view.
        /// </summary>
        /// <param name="camera">The camera to change.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="bounds">The bounding rectangle.</param>
        /// <param name="lookDirection">The look direction.</param>
        /// <param name="upDirection">The up direction.</param>
        /// <param name="animationTime">The animation time.</param>
        public static void FitView(this ProjectionCamera camera, Viewport3D viewport, Rect3D bounds, Vector3D lookDirection, Vector3D upDirection, double animationTime = 0)
        {
            Vector3D diagonal = new(bounds.SizeX, bounds.SizeY, bounds.SizeZ);
            Point3D center = bounds.Location + (diagonal * 0.5);
            double radius = diagonal.Length * 0.5;
            FitView(camera, viewport, center, radius, lookDirection, upDirection, animationTime);
        }

        /// <summary>
        /// Zooms to fit the specified sphere.
        /// </summary>
        /// <param name="camera">
        /// The camera to change.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="center">
        /// The center of the sphere.
        /// </param>
        /// <param name="radius">
        /// The radius of the sphere.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void ZoomExtents(
            ProjectionCamera camera,
            Viewport3D viewport,
            Point3D center,
            double radius,
            double animationTime = 0)
        {
            if (camera is PerspectiveCamera perspectiveCamera)
            {
                FitView(camera, viewport, center, radius, perspectiveCamera.LookDirection, perspectiveCamera.UpDirection, animationTime);
                return;
            }

            if (camera is OrthographicCamera orthoCamera)
            {
                FitView(camera, viewport, center, radius, orthoCamera.LookDirection, orthoCamera.UpDirection, animationTime);
            }
        }

        /// <summary>
        /// Fits the specified bounding sphere to the view.
        /// </summary>
        /// <param name="camera">The camera to change.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="lookDirection">The look direction.</param>
        /// <param name="upDirection">The up direction.</param>
        /// <param name="animationTime">The animation time.</param>
        public static void FitView(
            ProjectionCamera camera,
            Viewport3D viewport,
            Point3D center,
            double radius,
            Vector3D lookDirection,
            Vector3D upDirection,
            double animationTime = 0)
        {
            if (camera is PerspectiveCamera perspectiveCamera)
            {
                PerspectiveCamera pcam = perspectiveCamera;
                double disth = radius / Math.Tan(0.5 * pcam.FieldOfView * Math.PI / 180);
                double vfov = pcam.FieldOfView;
                if (viewport.ActualWidth > 0 && viewport.ActualHeight > 0)
                {
                    vfov *= viewport.ActualHeight / viewport.ActualWidth;
                }

                double distv = radius / Math.Tan(0.5 * vfov * Math.PI / 180);
                double dist = Math.Max(disth, distv);
                Vector3D dir = lookDirection;
                dir.Normalize();
                perspectiveCamera.LookAt(center, dir * dist, upDirection, animationTime);
                return;
            }

            if (camera is OrthographicCamera orthographicCamera)
            {
                Vector3D dir = lookDirection;
                dir.Normalize();
                orthographicCamera.LookAt(center, dir, upDirection, animationTime);
                double newWidth = radius * 2;

                if (viewport.ActualWidth > viewport.ActualHeight)
                {
                    newWidth = radius * 2 * viewport.ActualWidth / viewport.ActualHeight;
                }

                orthographicCamera.AnimateWidth(newWidth, animationTime);
            }
        }

        /// <summary>
        /// Zooms the camera to the specified rectangle.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="zoomRectangle">
        /// The zoom rectangle.
        /// </param>
        public static void ZoomToRectangle(this ProjectionCamera camera, Viewport3D viewport, Rect zoomRectangle)
        {
            Privider.Ray3D topLeftRay = viewport.Point2DtoRay3D(zoomRectangle.TopLeft);
            Privider.Ray3D topRightRay = viewport.Point2DtoRay3D(zoomRectangle.TopRight);
            Privider.Ray3D centerRay = viewport.Point2DtoRay3D(
                new Point(
                    (zoomRectangle.Left + zoomRectangle.Right) * 0.5, (zoomRectangle.Top + zoomRectangle.Bottom) * 0.5));

            if (topLeftRay == null || topRightRay == null || centerRay == null)
            {
                // could not invert camera matrix
                return;
            }

            Vector3D u = topLeftRay.Direction;
            Vector3D v = topRightRay.Direction;
            Vector3D w = centerRay.Direction;
            u.Normalize();
            v.Normalize();
            w.Normalize();
            if (camera is PerspectiveCamera perspectiveCamera)
            {
                double distance = camera.LookDirection.Length;

                // option 1: change distance
                double newDistance = distance * zoomRectangle.Width / viewport.ActualWidth;
                Vector3D newLookDirection = newDistance * w;
                Point3D newPosition = perspectiveCamera.Position + ((distance - newDistance) * w);
                Point3D newTarget = newPosition + newLookDirection;
                camera.LookAt(newTarget, newLookDirection, 200);

                // option 2: change fov
                //    double newFieldOfView = Math.Acos(Vector3D.DotProduct(u, v));
                //    var newTarget = camera.Position + distance * w;
                //    pcamera.FieldOfView = newFieldOfView * 180 / Math.PI;
                //    LookAt(camera, newTarget, distance * w, 0);
            }

            if (camera is OrthographicCamera orthographicCamera)
            {
                orthographicCamera.Width *= zoomRectangle.Width / viewport.ActualWidth;
                Point3D oldTarget = camera.Position + camera.LookDirection;
                double distance = camera.LookDirection.Length;
                if (centerRay.PlaneIntersection(oldTarget, w, out Point3D newTarget))
                {
                    orthographicCamera.LookDirection = w * distance;
                    orthographicCamera.Position = newTarget - orthographicCamera.LookDirection;
                }
            }
        }
    }
}