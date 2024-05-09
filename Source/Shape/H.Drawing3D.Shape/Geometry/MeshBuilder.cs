// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeshBuilder.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Builds MeshGeometry3D objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#if SHARPDX
    using System.Linq;
    using global::SharpDX;
#if NETFX_CORE
#if CORE
    using HelixToolkit.SharpDX.Core;
#else
    using HelixToolkit.UWP;
#endif
    using HelixToolkit.Wpf;
#else
#endif

#if !NETFX_CORE
    using Rect3D = System.Windows.Media.Media3D.Rect3D;
#endif
    using Point = global::SharpDX.Vector2;
    using Point3D = global::SharpDX.Vector3;
    using Vector3D = global::SharpDX.Vector3;
    using Vector3DCollection = Vector3Collection;
    using Point3DCollection = Vector3Collection;
    using PointCollection = Vector2Collection;
    using Int32Collection = IntCollection;
    using DoubleOrSingle = System.Single;
#else
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using DoubleOrSingle = System.Double;
using H.Drawing3D.Shape.ExtensionMethods;
using H.Drawing3D.Shape.Geometry.Geometry;

#if SHARPDX
#if NETFX_CORE
#if CORE
namespace HelixToolkit.SharpDX.Core
#else
namespace HelixToolkit.UWP
#endif
#else
namespace HelixToolkit.Wpf.SharpDX
#endif
#else
namespace H.Drawing3D.Shape.Geometry
#endif
{
#endif
    /// <summary>
    /// Box face enumeration.
    /// </summary>
    [Flags]
    public enum BoxFaces
    {
        /// <summary>
        /// The top.
        /// </summary>
        PositiveZ = 0x1,
        /// <summary>
        /// The top.
        /// </summary>
        Top = PositiveZ,

        /// <summary>
        /// The bottom.
        /// </summary>
        NegativeZ = 0x2,
        /// <summary>
        /// The bottom.
        /// </summary>
        Bottom = NegativeZ,

        /// <summary>
        /// The left side.
        /// </summary>
        NegativeY = 0x4,
        /// <summary>
        /// The left side.
        /// </summary>
        Left = NegativeY,

        /// <summary>
        /// The right side.
        /// </summary>
        PositiveY = 0x8,
        /// <summary>
        /// The right side.
        /// </summary>
        Right = PositiveY,

        /// <summary>
        /// The front side.
        /// </summary>
        PositiveX = 0x10,
        /// <summary>
        /// The front side.
        /// </summary>
        Front = PositiveX,

        /// <summary>
        /// The back side.
        /// </summary>
        NegativeX = 0x20,
        /// <summary>
        /// The back side.
        /// </summary>
        Back = NegativeX,

        /// <summary>
        /// All sides.
        /// </summary>
        All = PositiveZ | NegativeZ | NegativeY | PositiveY | PositiveX | NegativeX
    }
    /// <summary>
    /// MeshFaces.
    /// </summary>
    public enum MeshFaces
    {
        /// <summary>
        /// Normal Face (3 Points).
        /// </summary>
        Default,
        /// <summary>
        /// Face with 4 Points.
        /// </summary>
        QuadPatches,
    }

    /// <summary>
    /// Builds MeshGeometry3D objects.
    /// </summary>
    /// <remarks>
    /// Performance tips for MeshGeometry3D (http://msdn.microsoft.com/en-us/library/bb613553.aspx)
    /// <para>
    /// High impact:
    /// Mesh animation—changing the individual vertices of a mesh on a per-frame basis—is not always efficient in
    /// Windows Presentation Foundation (WPF).  To minimize the performance impact of change notifications when
    /// each vertex is modified, detach the mesh from the visual tree before performing per-vertex modification.
    /// Once the mesh has been modified, reattach it to the visual tree.  Also, try to minimize the size of meshes
    /// that will be animated in this way.
    /// </para>
    /// <para>
    /// Medium impact:
    /// When a mesh is defined as abutting triangles with shared vertices and those vertices have the same position,
    /// normal, and texture coordinates, define each shared vertex only once and then define your triangles by
    /// index with TriangleIndices.
    /// </para>
    /// <para>
    /// Low impact:
    /// To minimize the construction time of large collections in Windows Presentation Foundation (WPF),
    /// such as a MeshGeometry3D’s Positions, Normals, TextureCoordinates, and TriangleIndices, pre-size
    /// the collections before value population. If possible, pass the collections’ constructors prepopulated
    /// data structures such as arrays or Lists.
    /// </para>
    /// </remarks>
    public class MeshBuilder : IMeshBuilder
    {
        #region Static and Const
        /// <summary>
        /// 'All curves should have the same number of points' exception message.
        /// </summary>
        private const string AllCurvesShouldHaveTheSameNumberOfPoints =
            "All curves should have the same number of points";
        /// <summary>
        /// 'Source mesh normals should not be null' exception message.
        /// </summary>
        private const string SourceMeshNormalsShouldNotBeNull = "Source mesh normals should not be null.";
        /// <summary>
        /// 'Source mesh texture coordinates should not be null' exception message.
        /// </summary>
        private const string SourceMeshTextureCoordinatesShouldNotBeNull =
            "Source mesh texture coordinates should not be null.";
        /// <summary>
        /// 'Wrong number of diameters' exception message.
        /// </summary>
        private const string WrongNumberOfDiameters = "Wrong number of diameters.";
        /// <summary>
        /// 'Wrong number of positions' exception message.
        /// </summary>
        private const string WrongNumberOfPositions = "Wrong number of positions.";
        /// <summary>
        /// 'Wrong number of normals' exception message.
        /// </summary>
        private const string WrongNumberOfNormals = "Wrong number of normals.";
        /// <summary>
        /// 'Wrong number of texture coordinates' exception message.
        /// </summary>
        private const string WrongNumberOfTextureCoordinates = "Wrong number of texture coordinates.";
        /// <summary>
        /// 'Wrong number of angles' exception message.
        /// </summary>
        private const string WrongNumberOfAngles = "Wrong number of angles.";
        /// <summary>
        /// The circle cache.
        /// </summary>
        private static readonly ThreadLocal<Dictionary<int, IList<Point>>> CircleCache = new(() => new Dictionary<int, IList<Point>>());
        /// <summary>
        /// The closed circle cache.
        /// </summary>
        private static readonly ThreadLocal<Dictionary<int, IList<Point>>> ClosedCircleCache = new(() => new Dictionary<int, IList<Point>>());
#if !NETFX_CORE
        /// <summary>
        /// The unit sphere cache.
        /// </summary>
        private static readonly ThreadLocal<Dictionary<int, MeshGeometry3D>> UnitSphereCache = new(() => new Dictionary<int, MeshGeometry3D>());
#endif
        #endregion Static and Const

        #region Variables and Properties
        /// <summary>
        /// The positions.
        /// </summary>
        /// <summary>
        /// Gets the positions collection of the mesh.
        /// </summary>
        /// <value> The positions. </value>
        public Point3DCollection Positions { get; private set; }

        /// <summary>
        /// Gets the triangle indices.
        /// </summary>
        /// <value>The triangle indices.</value>
        public Int32Collection TriangleIndices { get; private set; }
        /// <summary>
        /// The normal vectors.
        /// </summary>
        private Vector3DCollection normals;
        /// <summary>
        /// Gets the normal vectors of the mesh.
        /// </summary>
        /// <value>The normal vectors.</value>
        public Vector3DCollection Normals { get => this.normals; set => this.normals = value; }

        /// <summary>
        /// Gets the texture coordinates of the mesh.
        /// </summary>
        /// <value>The texture coordinates.</value>
        public PointCollection TextureCoordinates { get; set; }

        /// <summary>
        /// Gets and sets the tangents of the mesh.
        /// </summary>
        /// <value>The tangents.</value>
        public Vector3DCollection Tangents { get; set; }

        /// <summary>
        /// Gets and sets the bi-tangents of the mesh.
        /// </summary>
        /// <value>The bi-tangents.</value>
        public Vector3DCollection BiTangents { get; set; }
        /// <summary>
        /// Do we have Normals or not.
        /// </summary>
        public bool HasNormals => this.normals != null;
        /// <summary>
        /// Do we have Texture Coordinates or not.
        /// </summary>
        public bool HasTexCoords => this.TextureCoordinates != null;
        /// <summary>
        /// Do we have Tangents or not.
        /// </summary>
        public bool HasTangents => this.Tangents != null;
        /// <summary>
        /// Gets or sets a value indicating whether to create normal vectors.
        /// </summary>
        /// <value>
        /// <c>true</c> if normal vectors should be created; otherwise, <c>false</c>.
        /// </value>
        public bool CreateNormals
        {
            get => this.normals != null;
            set
            {
                if (value && this.normals == null)
                {
                    this.normals = new Vector3DCollection();
                }
                if (!value)
                {
                    this.normals = null;
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether to create texture coordinates.
        /// </summary>
        /// <value>
        /// <c>true</c> if texture coordinates should be created; otherwise, <c>false</c>.
        /// </value>
        public bool CreateTextureCoordinates
        {
            get => this.TextureCoordinates != null;
            set
            {
                if (value && this.TextureCoordinates == null)
                {
                    this.TextureCoordinates = new PointCollection();
                }
                if (!value)
                {
                    this.TextureCoordinates = null;
                }
            }
        }
        #endregion Variables and Properties

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshBuilder"/> class.
        /// </summary>
        /// <remarks>
        /// Normal and texture coordinate generation are included.
        /// </remarks>
        public MeshBuilder()
            : this(true, true)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshBuilder"/> class.
        /// </summary>
        /// <param name="generateNormals">
        /// Generate normal vectors.
        /// </param>
        /// <param name="generateTexCoords">
        /// Generate texture coordinates.
        /// </param>
        /// <param name="tangentSpace">
        /// Generate tangents.
        /// </param>
        public MeshBuilder(bool generateNormals = true, bool generateTexCoords = true, bool tangentSpace = false)
        {
            this.Positions = new Point3DCollection();
            this.TriangleIndices = new Int32Collection();
            if (generateNormals)
            {
                this.normals = new Vector3DCollection();
            }
            if (generateTexCoords)
            {
                this.TextureCoordinates = new PointCollection();
            }
            if (tangentSpace)
            {
                this.Tangents = new Vector3DCollection();
                this.BiTangents = new Vector3DCollection();
            }
        }
        #endregion Constructors

        #region Geometric Base Functions
        /// <summary>
        /// Gets a circle section (cached).
        /// </summary>
        /// <param name="thetaDiv">
        /// The number of division.
        /// </param>
        /// <param name="closed">
        /// Is the circle closed?
        /// If true, the last point will not be at the same position than the first one.
        /// </param>
        /// <returns>
        /// A circle.
        /// </returns>
        public static IList<Point> GetCircle(int thetaDiv, bool closed = false)
        {
            IList<Point> circle = null;
            // If the circle can't be found in one of the two caches
            if ((!closed && !CircleCache.Value.TryGetValue(thetaDiv, out circle)) ||
                (closed && !ClosedCircleCache.Value.TryGetValue(thetaDiv, out circle)))
            {
                circle = new PointCollection();
                // Add to the cache
                if (!closed)
                {
                    CircleCache.Value.Add(thetaDiv, circle);
                }
                else
                {
                    ClosedCircleCache.Value.Add(thetaDiv, circle);
                }
                // Determine the angle steps
                int num = closed ? thetaDiv : thetaDiv - 1;
                for (int i = 0; i < thetaDiv; i++)
                {
                    double theta = Math.PI * 2 * ((DoubleOrSingle)i / num);
                    circle.Add(new Point((DoubleOrSingle)Math.Cos(theta), -(DoubleOrSingle)Math.Sin(theta)));
                }
            }
            // Since Vector2Collection is not Freezable,
            // return new IList<Vector> to avoid manipulation of the Cached Values
            IList<Point> result = new List<Point>();
            foreach (Point point in circle)
            {
                result.Add(new Point(point.X, point.Y));
            }
            return result;
        }
        /// <summary>
        /// Gets a circle segment section.
        /// </summary>
        /// <param name="thetaDiv">The number of division.</param>
        /// <param name="totalAngle">The angle of the circle segment.</param>
        /// <param name="angleOffset">The angle-offset to use.</param>
        /// <returns>
        /// A circle segment.
        /// </returns>
        public static IList<Point> GetCircleSegment(int thetaDiv, double totalAngle = 2 * Math.PI, double angleOffset = 0)
        {
            IList<Point> circleSegment;
            circleSegment = new PointCollection();
            for (int i = 0; i < thetaDiv; i++)
            {
                double theta = ((DoubleOrSingle)totalAngle * ((DoubleOrSingle)i / (thetaDiv - 1))) + (DoubleOrSingle)angleOffset;
                circleSegment.Add(new Point((DoubleOrSingle)Math.Cos(theta), (DoubleOrSingle)Math.Sin(theta)));
            }

            return circleSegment;
        }

#if !NETFX_CORE
        /// <summary>
        /// Gets a unit sphere from the cache.
        /// </summary>
        /// <param name="subdivisions">
        /// The number of subdivisions.
        /// </param>
        /// <returns>
        /// A unit sphere mesh.
        /// </returns>
        private static MeshGeometry3D GetUnitSphere(int subdivisions)
        {
            if (UnitSphereCache.Value.ContainsKey(subdivisions))
            {
                return UnitSphereCache.Value[subdivisions];
            }

            MeshBuilder mb = new(false, false);
            mb.AddRegularIcosahedron(new Point3D(), 1, false);
            for (int i = 0; i < subdivisions; i++)
            {
                mb.SubdivideLinear();
            }

            for (int i = 0; i < mb.Positions.Count; i++)
            {
                Vector3D v = mb.Positions[i].ToVector3D();
                v.Normalize();
                mb.Positions[i] = SharedFunctions.ToPoint3D(ref v);
            }
            MeshGeometry3D mesh = mb.ToMesh();
            UnitSphereCache.Value[subdivisions] = mesh;
            return mesh;
        }
#endif

        /// <summary>
        /// Calculate the Mesh's Normals
        /// </summary>
        /// <param name="positions">The Positions.</param>
        /// <param name="triangleIndices">The TriangleIndices.</param>
        /// <param name="normals">The calcualted Normals.</param>
        private static void ComputeNormals(Point3DCollection positions, Int32Collection triangleIndices, out Vector3DCollection normals)
        {
            normals = new Vector3DCollection(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                normals.Add(new Vector3D(0, 0, 0));
            }
            for (int t = 0; t < triangleIndices.Count; t += 3)
            {
                int i1 = triangleIndices[t];
                int i2 = triangleIndices[t + 1];
                int i3 = triangleIndices[t + 2];
                Point3D v1 = positions[i1];
                Point3D v2 = positions[i2];
                Point3D v3 = positions[i3];
                Vector3D p1 = v2 - v1;
                Vector3D p2 = v3 - v1;
                Vector3D n = SharedFunctions.CrossProduct(ref p1, ref p2);
                // angle
                p1.Normalize();
                p2.Normalize();
                float a = (float)Math.Acos(SharedFunctions.DotProduct(ref p1, ref p2));
                n.Normalize();
                normals[i1] += a * n;
                normals[i2] += a * n;
                normals[i3] += a * n;
            }
            for (int i = 0; i < normals.Count; i++)
            {
                //Cannot use normals[i].normalize() if using Media3D.Vector3DCollection. Does not change the internal value in Vector3DCollection.
                Vector3D n = normals[i];
                n.Normalize();
                normals[i] = n;
            }
        }
        /// <summary>
        /// Calculate the Mesh's Tangents
        /// </summary>
        /// <param name="meshFaces">The Faces of the Mesh</param>
        public void ComputeTangents(MeshFaces meshFaces)
        {
            switch (meshFaces)
            {
                case MeshFaces.Default:
                    if (this.Positions != null & this.TriangleIndices != null & this.normals != null & this.TextureCoordinates != null)
                    {
                        ComputeTangents(this.Positions, this.normals, this.TextureCoordinates, this.TriangleIndices, out Vector3DCollection t1, out Vector3DCollection t2);
                        this.Tangents = t1;
                        this.BiTangents = t2;
                    }
                    break;
                case MeshFaces.QuadPatches:
                    if (this.Positions != null & this.TriangleIndices != null & this.normals != null & this.TextureCoordinates != null)
                    {
                        ComputeTangentsQuads(this.Positions, this.normals, this.TextureCoordinates, this.TriangleIndices, out Vector3DCollection t1, out Vector3DCollection t2);
                        this.Tangents = t1;
                        this.BiTangents = t2;
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Tangent Space computation for IndexedTriangle meshes
        /// Based on:
        /// http://www.terathon.com/code/tangent.html
        /// </summary>
        public static void ComputeTangents(Point3DCollection positions, Vector3DCollection normals, PointCollection textureCoordinates, Int32Collection triangleIndices,
            out Vector3DCollection tangents, out Vector3DCollection bitangents)
        {
            Vector3D[] tan1 = new Vector3D[positions.Count];
            for (int t = 0; t < triangleIndices.Count; t += 3)
            {
                int i1 = triangleIndices[t];
                int i2 = triangleIndices[t + 1];
                int i3 = triangleIndices[t + 2];
                Point3D v1 = positions[i1];
                Point3D v2 = positions[i2];
                Point3D v3 = positions[i3];
                Point w1 = textureCoordinates[i1];
                Point w2 = textureCoordinates[i2];
                Point w3 = textureCoordinates[i3];
                double x1 = v2.X - v1.X;
                double x2 = v3.X - v1.X;
                double y1 = v2.Y - v1.Y;
                double y2 = v3.Y - v1.Y;
                double z1 = v2.Z - v1.Z;
                double z2 = v3.Z - v1.Z;
                double s1 = w2.X - w1.X;
                double s2 = w3.X - w1.X;
                double t1 = w2.Y - w1.Y;
                double t2 = w3.Y - w1.Y;
                double r = 1.0f / ((s1 * t2) - (s2 * t1));
                Vector3D udir = new(((t2 * x1) - (t1 * x2)) * r, ((t2 * y1) - (t1 * y2)) * r, ((t2 * z1) - (t1 * z2)) * r);
                tan1[i1] += udir;
                tan1[i2] += udir;
                tan1[i3] += udir;
            }
            tangents = new Vector3DCollection(positions.Count);
            bitangents = new Vector3DCollection(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3D n = normals[i];
                Vector3D t = tan1[i];
                t -= n * SharedFunctions.DotProduct(ref n, ref t);
                t.Normalize();
                Vector3D b = SharedFunctions.CrossProduct(ref n, ref t);
                tangents.Add(t);
                bitangents.Add(b);
            }
        }
        /// <summary>
        /// Calculate the Tangents for a Quad.
        /// </summary>
        /// <param name="positions">The Positions.</param>
        /// <param name="normals">The Normals.</param>
        /// <param name="textureCoordinates">The TextureCoordinates.</param>
        /// <param name="indices">The Indices.</param>
        /// <param name="tangents">The calculated Tangens.</param>
        /// <param name="bitangents">The calculated Bi-Tangens.</param>
        public static void ComputeTangentsQuads(Point3DCollection positions, Vector3DCollection normals, PointCollection textureCoordinates, Int32Collection indices,
            out Vector3DCollection tangents, out Vector3DCollection bitangents)
        {
            Vector3D[] tan1 = new Vector3D[positions.Count];
            for (int t = 0; t < indices.Count; t += 4)
            {
                int i1 = indices[t];
                int i2 = indices[t + 1];
                int i3 = indices[t + 2];
                int i4 = indices[t + 3];
                Point3D v1 = positions[i1];
                Point3D v2 = positions[i2];
                _ = positions[i3];
                Point3D v4 = positions[i4];
                Point w1 = textureCoordinates[i1];
                Point w2 = textureCoordinates[i2];
                _ = textureCoordinates[i3];
                Point w4 = textureCoordinates[i4];
                double x1 = v2.X - v1.X;
                double x2 = v4.X - v1.X;
                double y1 = v2.Y - v1.Y;
                double y2 = v4.Y - v1.Y;
                double z1 = v2.Z - v1.Z;
                double z2 = v4.Z - v1.Z;
                double s1 = w2.X - w1.X;
                double s2 = w4.X - w1.X;
                double t1 = w2.Y - w1.Y;
                double t2 = w4.Y - w1.Y;
                double r = 1.0f / ((s1 * t2) - (s2 * t1));
                Vector3D udir = new(((t2 * x1) - (t1 * x2)) * r, ((t2 * y1) - (t1 * y2)) * r, ((t2 * z1) - (t1 * z2)) * r);
                tan1[i1] += udir;
                tan1[i2] += udir;
                tan1[i3] += udir;
                tan1[i4] += udir;
            }
            tangents = new Vector3DCollection(positions.Count);
            bitangents = new Vector3DCollection(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3D n = normals[i];
                Vector3D t = tan1[i];
                t -= n * SharedFunctions.DotProduct(ref n, ref t);
                t.Normalize();
                Vector3D b = SharedFunctions.CrossProduct(ref n, ref t);
                tangents.Add(t);
                bitangents.Add(b);
            }
        }

#if !NETFX_CORE
        /// <summary>
        /// Calculate the Tangents for a MeshGeometry3D.
        /// </summary>
        /// <param name="meshGeometry">The MeshGeometry3D.</param>
        public static void ComputeTangents(MeshGeometry3D meshGeometry)
        {
            ComputeTangents(meshGeometry.Positions, meshGeometry.Normals, meshGeometry.TextureCoordinates, meshGeometry.TriangleIndices, out Vector3DCollection t1, out Vector3DCollection t2);
#if SHARPDX
            meshGeometry.Tangents = new Vector3DCollection(t1);
            meshGeometry.BiTangents = new Vector3DCollection(t2);
#endif
        }
#endif

        /// <summary>
        /// Calculate the Normals and Tangents for all MeshFaces.
        /// </summary>
        /// <param name="meshFaces">The MeshFaces.</param>
        /// <param name="tangents">Also calculate the Tangents or not.</param>
        public void ComputeNormalsAndTangents(MeshFaces meshFaces, bool tangents = false)
        {
            if (!this.HasNormals & this.Positions != null & this.TriangleIndices != null)
            {
                ComputeNormals(this.Positions, this.TriangleIndices, out this.normals);
            }
            switch (meshFaces)
            {
                case MeshFaces.Default:
                    if (tangents & this.HasNormals & this.TextureCoordinates != null)
                    {
                        ComputeTangents(this.Positions, this.normals, this.TextureCoordinates, this.TriangleIndices, out Vector3DCollection t1, out Vector3DCollection t2);
                        this.Tangents = t1;
                        this.BiTangents = t2;
                    }
                    break;
                case MeshFaces.QuadPatches:
                    if (tangents & this.HasNormals & this.TextureCoordinates != null)
                    {
                        ComputeTangentsQuads(this.Positions, this.normals, this.TextureCoordinates, this.TriangleIndices, out Vector3DCollection t1, out Vector3DCollection t2);
                        this.Tangents = t1;
                        this.BiTangents = t2;
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion Geometric Base Functions

        #region Add Geometry
        /// <summary>
        /// Adds an arrow to the mesh.
        /// </summary>
        /// <param name="point1">
        /// The start point.
        /// </param>
        /// <param name="point2">
        /// The end point.
        /// </param>
        /// <param name="diameter">
        /// The diameter of the arrow cylinder.
        /// </param>
        /// <param name="headLength">
        /// Length of the head (relative to diameter).
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the arrow.
        /// </param>
        public void AddArrow(Point3D point1, Point3D point2, double diameter, double headLength = 3, int thetaDiv = 18)
        {
            Vector3D dir = point2 - point1;
            double length = SharedFunctions.Length(ref dir);
            double r = (DoubleOrSingle)diameter / 2;

            PointCollection pc = new()
            {
                    new Point(0, 0),
                    new Point(0, r),
                    new Point(length - (DoubleOrSingle)(diameter * headLength), r),
                    new Point(length - (DoubleOrSingle)(diameter * headLength), r * 2),
                    new Point(length, 0)
                };

            this.AddRevolvedGeometry(pc, null, point1, dir, thetaDiv);
        }

#if !NETFX_CORE
        /// <summary>
        /// Adds the edges of a bounding box as cylinders.
        /// </summary>
        /// <param name="boundingBox">
        /// The bounding box.
        /// </param>
        /// <param name="diameter">
        /// The diameter of the cylinders.
        /// </param>
        public void AddBoundingBox(Rect3D boundingBox, double diameter)
        {
            Point3D p0 = new((DoubleOrSingle)boundingBox.X, (DoubleOrSingle)boundingBox.Y, (DoubleOrSingle)boundingBox.Z);
            Point3D p1 = new((DoubleOrSingle)boundingBox.X, (DoubleOrSingle)boundingBox.Y + (DoubleOrSingle)boundingBox.SizeY, (DoubleOrSingle)boundingBox.Z);
            Point3D p2 = new((DoubleOrSingle)boundingBox.X + (DoubleOrSingle)boundingBox.SizeX, (DoubleOrSingle)boundingBox.Y + (DoubleOrSingle)boundingBox.SizeY, (DoubleOrSingle)boundingBox.Z);
            Point3D p3 = new((DoubleOrSingle)boundingBox.X + (DoubleOrSingle)boundingBox.SizeX, (DoubleOrSingle)boundingBox.Y, (DoubleOrSingle)boundingBox.Z);
            Point3D p4 = new((DoubleOrSingle)boundingBox.X, (DoubleOrSingle)boundingBox.Y, (DoubleOrSingle)boundingBox.Z + (DoubleOrSingle)boundingBox.SizeZ);
            Point3D p5 = new((DoubleOrSingle)boundingBox.X, (DoubleOrSingle)boundingBox.Y + (DoubleOrSingle)boundingBox.SizeY, (DoubleOrSingle)boundingBox.Z + (DoubleOrSingle)boundingBox.SizeZ);
            Point3D p6 = new((DoubleOrSingle)boundingBox.X + (DoubleOrSingle)boundingBox.SizeX, (DoubleOrSingle)boundingBox.Y + (DoubleOrSingle)boundingBox.SizeY, (DoubleOrSingle)boundingBox.Z + (DoubleOrSingle)boundingBox.SizeZ);
            Point3D p7 = new((DoubleOrSingle)boundingBox.X + (DoubleOrSingle)boundingBox.SizeX, (DoubleOrSingle)boundingBox.Y, (DoubleOrSingle)boundingBox.Z + (DoubleOrSingle)boundingBox.SizeZ);

            void addEdge(Point3D c1, Point3D c2)
            {
                this.AddCylinder(c1, c2, diameter, 10);
            }

            addEdge(p0, p1);
            addEdge(p1, p2);
            addEdge(p2, p3);
            addEdge(p3, p0);

            addEdge(p4, p5);
            addEdge(p5, p6);
            addEdge(p6, p7);
            addEdge(p7, p4);

            addEdge(p0, p4);
            addEdge(p1, p5);
            addEdge(p2, p6);
            addEdge(p3, p7);
        }
#endif

        /// <summary>
        /// Adds a box aligned with the X, Y and Z axes.
        /// </summary>
        /// <param name="center">
        /// The center point of the box.
        /// </param>
        /// <param name="xlength">
        /// The length of the box along the X axis.
        /// </param>
        /// <param name="ylength">
        /// The length of the box along the Y axis.
        /// </param>
        /// <param name="zlength">
        /// The length of the box along the Z axis.
        /// </param>
        public void AddBox(Point3D center, double xlength, double ylength, double zlength)
        {
            this.AddBox(center, xlength, ylength, zlength, BoxFaces.All);
        }

#if !NETFX_CORE
        /// <summary>
        /// Adds a box aligned with the X, Y and Z axes.
        /// </summary>
        /// <param name="rectangle">
        /// The 3-D "rectangle".
        /// </param>
        /// <param name="faces">The faces to include.</param>
        public void AddBox(Rect3D rectangle, BoxFaces faces = BoxFaces.All)
        {
            this.AddBox(
                new Point3D((DoubleOrSingle)(rectangle.X + (rectangle.SizeX * 0.5f)), (DoubleOrSingle)(rectangle.Y + (rectangle.SizeY * 0.5f)), (DoubleOrSingle)(rectangle.Z + (rectangle.SizeZ * 0.5f))),
                (DoubleOrSingle)rectangle.SizeX, (DoubleOrSingle)rectangle.SizeY, (DoubleOrSingle)rectangle.SizeZ, faces);
        }
#endif

        /// <summary>
        /// Adds a box with the specified faces, aligned with the X, Y and Z axes.
        /// </summary>
        /// <param name="center">
        /// The center point of the box.
        /// </param>
        /// <param name="xlength">
        /// The length of the box along the X axis.
        /// </param>
        /// <param name="ylength">
        /// The length of the box along the Y axis.
        /// </param>
        /// <param name="zlength">
        /// The length of the box along the Z axis.
        /// </param>
        /// <param name="faces">
        /// The faces to include.
        /// </param>
        public void AddBox(Point3D center, double xlength, double ylength, double zlength, BoxFaces faces)
        {
            this.AddBox(center, new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), xlength, ylength, zlength, faces);
        }
        /// <summary>
        /// Adds a box with the specified faces, aligned with the specified axes.
        /// </summary>
        /// <param name="center">The center point of the box.</param>
        /// <param name="x">The x axis.</param>
        /// <param name="y">The y axis.</param>
        /// <param name="xlength">The length of the box along the X axis.</param>
        /// <param name="ylength">The length of the box along the Y axis.</param>
        /// <param name="zlength">The length of the box along the Z axis.</param>
        /// <param name="faces">The faces to include.</param>
        public void AddBox(Point3D center, Vector3D x, Vector3D y, double xlength, double ylength, double zlength, BoxFaces faces = BoxFaces.All)
        {
            Vector3D z = SharedFunctions.CrossProduct(ref x, ref y);
            if ((faces & BoxFaces.Front) == BoxFaces.Front)
            {
                this.AddCubeFace(center, x, z, xlength, ylength, zlength);
            }

            if ((faces & BoxFaces.Back) == BoxFaces.Back)
            {
                this.AddCubeFace(center, -x, z, xlength, ylength, zlength);
            }

            if ((faces & BoxFaces.Left) == BoxFaces.Left)
            {
                this.AddCubeFace(center, -y, z, ylength, xlength, zlength);
            }

            if ((faces & BoxFaces.Right) == BoxFaces.Right)
            {
                this.AddCubeFace(center, y, z, ylength, xlength, zlength);
            }

            if ((faces & BoxFaces.Top) == BoxFaces.Top)
            {
                this.AddCubeFace(center, z, y, zlength, xlength, ylength);
            }

            if ((faces & BoxFaces.Bottom) == BoxFaces.Bottom)
            {
                this.AddCubeFace(center, -z, y, zlength, xlength, ylength);
            }
        }
        /// <summary>
        /// Adds a (possibly truncated) cone.
        /// </summary>
        /// <param name="origin">
        /// The origin.
        /// </param>
        /// <param name="direction">
        /// The direction (normalization not required).
        /// </param>
        /// <param name="baseRadius">
        /// The base radius.
        /// </param>
        /// <param name="topRadius">
        /// The top radius.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="baseCap">
        /// Include a base cap if set to <c>true</c> .
        /// </param>
        /// <param name="topCap">
        /// Include the top cap if set to <c>true</c> .
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the cone.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Cone_(geometry).
        /// </remarks>
        public void AddCone(Point3D origin, Vector3D direction,
            double baseRadius, double topRadius, double height,
            bool baseCap, bool topCap, int thetaDiv)
        {
            PointCollection pc = new();
            List<double> tc = new();
            if (baseCap)
            {
                pc.Add(new Point(0, 0));
                tc.Add(0);
            }

            pc.Add(new Point(0, (DoubleOrSingle)baseRadius));
            tc.Add(1);
            pc.Add(new Point((DoubleOrSingle)height, (DoubleOrSingle)topRadius));
            tc.Add(0);
            if (topCap)
            {
                pc.Add(new Point((DoubleOrSingle)height, 0));
                tc.Add(1);
            }

            this.AddRevolvedGeometry(pc, tc, origin, direction, thetaDiv);
        }
        /// <summary>
        /// Adds a cone.
        /// </summary>
        /// <param name="origin">The origin point.</param>
        /// <param name="apex">The apex point.</param>
        /// <param name="baseRadius">The base radius.</param>
        /// <param name="baseCap">
        /// Include a base cap if set to <c>true</c> .
        /// </param>
        /// <param name="thetaDiv">The theta div.</param>
        public void AddCone(Point3D origin, Point3D apex, double baseRadius, bool baseCap, int thetaDiv)
        {
            Vector3D dir = apex - origin;
            this.AddCone(origin, dir, baseRadius, 0, SharedFunctions.Length(ref dir), baseCap, false, thetaDiv);
        }
        /// <summary>
        /// Adds a cube face.
        /// </summary>
        /// <param name="center">
        /// The center of the cube.
        /// </param>
        /// <param name="normal">
        /// The normal vector for the face.
        /// </param>
        /// <param name="up">
        /// The up vector for the face.
        /// </param>
        /// <param name="dist">
        /// The distance from the center of the cube to the face.
        /// </param>
        /// <param name="width">
        /// The width of the face.
        /// </param>
        /// <param name="height">
        /// The height of the face.
        /// </param>
        public void AddCubeFace(Point3D center, Vector3D normal, Vector3D up, double dist, double width, double height)
        {
            Vector3D right = SharedFunctions.CrossProduct(ref normal, ref up);
            Vector3D n = normal * (DoubleOrSingle)dist / 2;
            up *= (DoubleOrSingle)height / 2;
            right *= (DoubleOrSingle)width / 2;
            Point3D p1 = center + n - up - right;
            Point3D p2 = center + n - up + right;
            Point3D p3 = center + n + up + right;
            Point3D p4 = center + n + up - right;

            int i0 = this.Positions.Count;
            this.Positions.Add(p1);
            this.Positions.Add(p2);
            this.Positions.Add(p3);
            this.Positions.Add(p4);
            if (this.normals != null)
            {
                this.normals.Add(normal);
                this.normals.Add(normal);
                this.normals.Add(normal);
                this.normals.Add(normal);
            }

            if (this.TextureCoordinates != null)
            {
                this.TextureCoordinates.Add(new Point(1, 1));
                this.TextureCoordinates.Add(new Point(0, 1));
                this.TextureCoordinates.Add(new Point(0, 0));
                this.TextureCoordinates.Add(new Point(1, 0));
            }

            this.TriangleIndices.Add(i0 + 2);
            this.TriangleIndices.Add(i0 + 1);
            this.TriangleIndices.Add(i0 + 0);
            this.TriangleIndices.Add(i0 + 0);
            this.TriangleIndices.Add(i0 + 3);
            this.TriangleIndices.Add(i0 + 2);
        }
        /// <summary>
        /// Add a Cube, only with specified Faces.
        /// </summary>
        /// <param name="faces">The Faces to create (default all Faces)</param>
        public void AddCube(BoxFaces faces = BoxFaces.All)
        {
            if ((faces & BoxFaces.PositiveX) == BoxFaces.PositiveX)
            {
                this.AddFacePX();
            }
            if ((faces & BoxFaces.NegativeX) == BoxFaces.NegativeX)
            {
                this.AddFaceNX();
            }
            if ((faces & BoxFaces.NegativeY) == BoxFaces.NegativeY)
            {
                this.AddFaceNY();
            }
            if ((faces & BoxFaces.PositiveY) == BoxFaces.PositiveY)
            {
                this.AddFacePY();
            }
            if ((faces & BoxFaces.PositiveZ) == BoxFaces.PositiveZ)
            {
                this.AddFacePZ();
            }
            if ((faces & BoxFaces.NegativeZ) == BoxFaces.NegativeZ)
            {
                this.AddFaceNZ();
            }
        }
        /// <summary>
        /// Adds a cylinder to the mesh.
        /// </summary>
        /// <param name="p1">
        /// The first point.
        /// </param>
        /// <param name="p2">
        /// The second point.
        /// </param>
        /// <param name="diameter">
        /// The diameters.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the cylinder.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Cylinder_(geometry).
        /// </remarks>
        public void AddCylinder(Point3D p1, Point3D p2, double diameter, int thetaDiv)
        {
            Vector3D n = p2 - p1;
            double l = SharedFunctions.Length(ref n);
            n.Normalize();
            this.AddCone(p1, n, diameter / 2, diameter / 2, l, false, false, thetaDiv);
        }
        /// <summary>
        /// Adds a cylinder to the mesh.
        /// </summary>
        /// <param name="p1">
        /// The first point.
        /// </param>
        /// <param name="p2">
        /// The second point.
        /// </param>
        /// <param name="radius">
        /// The diameters.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the cylinder.
        /// </param>
        /// <param name="cap1">
        /// The first Cap.
        /// </param>
        /// <param name="cap2">
        /// The second Cap.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Cylinder_(geometry).
        /// </remarks>
        public void AddCylinder(Point3D p1, Point3D p2, double radius = 1, int thetaDiv = 32, bool cap1 = true, bool cap2 = true)
        {
            Vector3D n = p2 - p1;
            double l = SharedFunctions.Length(ref n);
            n.Normalize();
            this.AddCone(p1, n, radius, radius, l, cap1, cap2, thetaDiv);
        }
        /// <summary>
        /// Generate a Dodecahedron
        /// </summary>
        /// <param name="center">The Center of the Dodecahedron</param>
        /// <param name="forward">The Direction to the first Point (normalized).</param>
        /// <param name="up">The Up-Dirextion (normalized, perpendicular to the forward Direction)</param>
        /// <param name="sideLength">Length of the Edges of the Dodecahedron</param>
        /// <remarks>
        /// See:
        /// https://en.wikipedia.org/wiki/Dodecahedron
        /// https://en.wikipedia.org/wiki/Pentagon
        /// https://en.wikipedia.org/wiki/Isosceles_triangle
        /// </remarks>
        public void AddDodecahedron(Point3D center, Vector3D forward, Vector3D up, double sideLength)
        {
            // If points already exist in the MeshBuilder
            int positionsCount = this.Positions.Count;

            Vector3D right = SharedFunctions.CrossProduct(ref up, ref forward);
            // Distance from the Center to the Dodekaeder-Points
            double radiusSphere = 0.25f * (DoubleOrSingle)Math.Sqrt(3) * (1 + (DoubleOrSingle)Math.Sqrt(5)) * (DoubleOrSingle)sideLength;
            double radiusFace = 0.1f * (DoubleOrSingle)Math.Sqrt(50 + (10 * (DoubleOrSingle)Math.Sqrt(5))) * (DoubleOrSingle)sideLength;
            double vectorDown = (DoubleOrSingle)Math.Sqrt((radiusSphere * radiusSphere) - (radiusFace * radiusFace));

            // Add Points
            Point3D baseCenter = center - (up * vectorDown);
            IList<Point> pentagonPoints = GetCircle(5, true);
            // Base Points
            List<Point3D> basePoints = new();
            foreach (Point point in pentagonPoints)
            {
                Point3D newPoint = baseCenter + (forward * point.X * radiusFace) + (right * point.Y * radiusFace);
                basePoints.Add(newPoint);
                this.Positions.Add(newPoint);
            }
            // Angle of Projected Isosceles triangle
            double gamma = (DoubleOrSingle)Math.Acos(1 - (sideLength * sideLength / (2 * radiusSphere * radiusSphere)));
            // Base Upper Points
            foreach (Point3D point in basePoints)
            {
                Vector3D baseCenterToPoint = point - baseCenter;
                baseCenterToPoint.Normalize();
                Vector3D centerToPoint = point - center;
                centerToPoint.Normalize();
                Vector3D tempRight = SharedFunctions.CrossProduct(ref up, ref baseCenterToPoint);
                Point3D newPoint = new(radiusSphere * (DoubleOrSingle)Math.Cos(gamma), 0, radiusSphere * (DoubleOrSingle)Math.Sin(gamma));
                Vector3D tempUp = SharedFunctions.CrossProduct(ref centerToPoint, ref tempRight);
                this.Positions.Add(center + (centerToPoint * newPoint.X) + (tempUp * newPoint.Z));
            }

            // Top Points
            Point3D topCenter = center + (up * vectorDown);
            List<Point3D> topPoints = new();
            foreach (Point point in pentagonPoints)
            {
                Point3D newPoint = topCenter - (forward * point.X * radiusFace) + (right * point.Y * radiusFace);
                topPoints.Add(newPoint);
            }
            // Top Lower Points
            foreach (Point3D point in topPoints)
            {
                Vector3D topCenterToPoint = point - topCenter;
                topCenterToPoint.Normalize();
                Vector3D centerToPoint = point - center;
                centerToPoint.Normalize();
                Vector3D tempRight = SharedFunctions.CrossProduct(ref up, ref topCenterToPoint);
                Point3D newPoint = new(radiusSphere * (DoubleOrSingle)Math.Cos(gamma), 0, radiusSphere * (DoubleOrSingle)Math.Sin(gamma));
                Vector3D tempUp = SharedFunctions.CrossProduct(ref tempRight, ref centerToPoint);
                this.Positions.Add(center + (centerToPoint * newPoint.X) + (tempUp * newPoint.Z));
            }
            // Add top Points at last
            foreach (Point3D point in topPoints)
            {
                this.Positions.Add(point);
            }

            // Add Normals if wanted
            if (this.normals != null)
            {
                for (int i = positionsCount; i < this.Positions.Count; i++)
                {
                    Vector3D centerToPoint = this.Positions[i] - center;
                    centerToPoint.Normalize();
                    this.normals.Add(centerToPoint);
                }
            }

            // Add Texture Coordinates
            if (this.TextureCoordinates != null)
            {
                for (int i = positionsCount; i < this.Positions.Count; i++)
                {
                    Vector3D centerToPoint = this.Positions[i] - center;
                    centerToPoint.Normalize();
                    double cTPUpValue = SharedFunctions.DotProduct(ref centerToPoint, ref up);
                    Vector3D planeCTP = centerToPoint - (up * cTPUpValue);
                    planeCTP.Normalize();
                    double u = (DoubleOrSingle)Math.Atan2(SharedFunctions.DotProduct(ref planeCTP, ref forward), SharedFunctions.DotProduct(ref planeCTP, ref right));
                    double v = (cTPUpValue * 0.5f) + 0.5f;
                    this.TextureCoordinates.Add(new Point(u, v));
                }
            }

            // Add Faces
            // Base Polygon
            this.AddPolygonByTriangulation(this.Positions.Skip(positionsCount).Take(5).Select((p, i) => i).ToList());
            // Top Polygon
            this.AddPolygonByTriangulation(this.Positions.Skip(positionsCount + 15).Select((p, i) => 15 + i).ToList());
            // SidePolygons
            for (int i = 0; i < 5; i++)
            {
                // Polygon one
                List<int> pIndices = new() {
                    ((i + 1) % 5) + positionsCount,
                    i, i + 5 + positionsCount,
                    ((5 - i + 2) % 5) + 10 + positionsCount,
                    ((i + 1) % 5) + 5 + positionsCount
                };
                this.AddPolygonByTriangulation(pIndices);

                // Polygon two
                pIndices = new List<int>() {
                    i + 15 + positionsCount,
                    i + 10 + positionsCount,
                    ((5 - i + 2) % 5) + 5 + positionsCount,
                    ((i + 1) % 5) + 10 + positionsCount,
                    ((i + 1) % 5) + 15 + positionsCount
                };
                this.AddPolygonByTriangulation(pIndices);
            }
        }
        /// <summary>
        /// Adds a collection of edges as cylinders.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <param name="edges">
        /// The edge indices.
        /// </param>
        /// <param name="diameter">
        /// The diameter of the cylinders.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the cylinders.
        /// </param>
        public void AddEdges(IList<Point3D> points, IList<int> edges, double diameter, int thetaDiv)
        {
            for (int i = 0; i < edges.Count - 1; i += 2)
            {
                this.AddCylinder(points[edges[i]], points[edges[i + 1]], diameter, thetaDiv);
            }
        }
        /// <summary>
        /// Adds an ellipsoid.
        /// </summary>
        /// <param name="center">
        /// The center of the ellipsoid.
        /// </param>
        /// <param name="radiusx">
        /// The x radius of the ellipsoid.
        /// </param>
        /// <param name="radiusy">
        /// The y radius of the ellipsoid.
        /// </param>
        /// <param name="radiusz">
        /// The z radius of the ellipsoid.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the ellipsoid.
        /// </param>
        /// <param name="phiDiv">
        /// The number of divisions from top to bottom of the ellipsoid.
        /// </param>
        public void AddEllipsoid(Point3D center, double radiusx, double radiusy, double radiusz, int thetaDiv = 20, int phiDiv = 10)
        {
            int index0 = this.Positions.Count;
            double dt = 2 * Math.PI / thetaDiv;
            double dp = Math.PI / phiDiv;

            for (int pi = 0; pi <= phiDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= thetaDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    // Spherical coordinates
                    // http://mathworld.wolfram.com/SphericalCoordinates.html
                    double x = (DoubleOrSingle)Math.Cos(theta) * (DoubleOrSingle)Math.Sin(phi);
                    double y = (DoubleOrSingle)Math.Sin(theta) * (DoubleOrSingle)Math.Sin(phi);
                    double z = (DoubleOrSingle)Math.Cos(phi);

                    Point3D p = new(center.X + (DoubleOrSingle)(radiusx * x), center.Y + (DoubleOrSingle)(radiusy * y), center.Z + (DoubleOrSingle)(radiusz * z));
                    this.Positions.Add(p);

                    if (this.normals != null)
                    {
                        Vector3D n = new(x, y, z);
                        this.normals.Add(n);
                    }

                    if (this.TextureCoordinates != null)
                    {
                        Point uv = new(theta / (2 * Math.PI), phi / Math.PI);
                        this.TextureCoordinates.Add(uv);
                    }
                }
            }

            this.AddRectangularMeshTriangleIndices(index0, phiDiv + 1, thetaDiv + 1, true);
        }
        /// <summary>
        /// Adds an extruded surface of the specified curve.
        /// </summary>
        /// <param name="points">
        /// The 2D points describing the curve to extrude.
        /// </param>
        /// <param name="xaxis">
        /// The x-axis.
        /// </param>
        /// <param name="p0">
        /// The start origin of the extruded surface.
        /// </param>
        /// <param name="p1">
        /// The end origin of the extruded surface.
        /// </param>
        /// <remarks>
        /// The y-axis is determined by the cross product between the specified x-axis and the p1-origin vector.
        /// </remarks>
        public void AddExtrudedGeometry(IList<Point> points, Vector3D xaxis, Point3D p0, Point3D p1)
        {
            Vector3D p10 = p1 - p0;
            Vector3D ydirection = SharedFunctions.CrossProduct(ref xaxis, ref p10);
            ydirection.Normalize();
            xaxis.Normalize();

            int index0 = this.Positions.Count;
            int np = 2 * points.Count;
            foreach (Point p in points)
            {
                Vector3D v = (xaxis * p.X) + (ydirection * p.Y);
                this.Positions.Add(p0 + v);
                this.Positions.Add(p1 + v);
                v.Normalize();
                if (this.normals != null)
                {
                    this.normals.Add(v);
                    this.normals.Add(v);
                }

                if (this.TextureCoordinates != null)
                {
                    this.TextureCoordinates.Add(new Point(0, 0));
                    this.TextureCoordinates.Add(new Point(1, 0));
                }

                int i1 = index0 + 1;
                int i2 = (index0 + 2) % np;
                int i3 = ((index0 + 2) % np) + 1;

                this.TriangleIndices.Add(i1);
                this.TriangleIndices.Add(i2);
                this.TriangleIndices.Add(index0);

                this.TriangleIndices.Add(i1);
                this.TriangleIndices.Add(i3);
                this.TriangleIndices.Add(i2);
            }
            ComputeNormals(this.Positions, this.TriangleIndices, out this.normals);
        }
        /// <summary>
        /// Add a Face in positive Z-Direction.
        /// </summary>
        public void AddFacePZ()
        {
            Point3D[] positions = new Point3D[]
            {
                new(0,0,1),
                new(0,1,1),
                new(1,1,1),
                new(1,0,1),
            };
            Vector3D[] normals = new Vector3D[]
            {
                new(0,0,1),
                new(0,0,1),
                new(0,0,1),
                new(0,0,1),
            };
            int i0 = this.Positions.Count;
            int[] indices = new int[]
            {
                i0+0,i0+3,i0+2,
                i0+0,i0+2,i0+1,
            };
            Point[] texcoords = new Point[]
            {
                new(0,1),
                new(1,1),
                new(1,0),
                new(0,0),
            };

            foreach (Point3D position in positions)
            {
                this.Positions.Add(position);
            }
            foreach (Vector3D normal in normals)
            {
                this.normals.Add(normal);
            }
            foreach (int index in indices)
            {
                this.TriangleIndices.Add(index);
            }
            foreach (Point texCoord in texcoords)
            {
                this.TextureCoordinates.Add(texCoord);
            }
        }
        /// <summary>
        /// Add a Face in negative Z-Direction.
        /// </summary>
        public void AddFaceNZ()
        {
            Point3D[] positions = new Point3D[]
            {
                new(0,1,0), //p1
                new(0,0,0), //p0                
                new(1,0,0), //p3
                new(1,1,0), //p2
            };
            Vector3D[] normals = new Vector3D[]
            {
                -new Vector3D(0,0,1),
                -new Vector3D(0,0,1),
                -new Vector3D(0,0,1),
                -new Vector3D(0,0,1),
            };

            int i0 = this.Positions.Count;
            int[] indices = new int[]
            {
                i0+0,i0+3,i0+2,
                i0+0,i0+2,i0+1,
            };
            Point[] texcoords = new Point[]
            {
                new(0,1),
                new(1,1),
                new(1,0),
                new(0,0),
            };

            foreach (Point3D position in positions)
            {
                this.Positions.Add(position);
            }
            foreach (Vector3D normal in normals)
            {
                this.normals.Add(normal);
            }
            foreach (int index in indices)
            {
                this.TriangleIndices.Add(index);
            }
            foreach (Point texCoord in texcoords)
            {
                this.TextureCoordinates.Add(texCoord);
            }
        }
        /// <summary>
        /// Add a Face in positive X-Direction.
        /// </summary>
        public void AddFacePX()
        {
            Point3D[] positions = new Point3D[]
            {
                new(1,0,0), //p0
                new(1,0,1), //p1
                new(1,1,1), //p2   
                new(1,1,0), //p3                             
            };
            Vector3D[] normals = new Vector3D[]
            {
                new(1,0,0),
                new(1,0,0),
                new(1,0,0),
                new(1,0,0),
            };

            int i0 = this.Positions.Count;
            int[] indices = new int[]
            {
                i0+0,i0+3,i0+2,
                i0+0,i0+2,i0+1,
            };
            Point[] texcoords = new Point[]
            {
                new(0,1),
                new(1,1),
                new(1,0),
                new(0,0),
            };

            foreach (Point3D position in positions)
            {
                this.Positions.Add(position);
            }
            foreach (Vector3D normal in normals)
            {
                this.normals.Add(normal);
            }
            foreach (int index in indices)
            {
                this.TriangleIndices.Add(index);
            }
            foreach (Point texCoord in texcoords)
            {
                this.TextureCoordinates.Add(texCoord);
            }
        }
        /// <summary>
        /// Add a Face in negative X-Direction.
        /// </summary>
        public void AddFaceNX()
        {
            Point3D[] positions = new Point3D[]
            {
                new(0,0,1), //p1
                new(0,0,0), //p0                
                new(0,1,0), //p3 
                new(0,1,1), //p2               
            };
            Vector3D[] normals = new Vector3D[]
            {
                -new Vector3D(1,0,0),
                -new Vector3D(1,0,0),
                -new Vector3D(1,0,0),
                -new Vector3D(1,0,0),
            };

            int i0 = this.Positions.Count;
            int[] indices = new int[]
            {
                i0+0,i0+3,i0+2,
                i0+0,i0+2,i0+1,
            };
            Point[] texcoords = new Point[]
            {
                new(0,1),
                new(1,1),
                new(1,0),
                new(0,0),
            };

            foreach (Point3D position in positions)
            {
                this.Positions.Add(position);
            }
            foreach (Vector3D normal in normals)
            {
                this.normals.Add(normal);
            }
            foreach (int index in indices)
            {
                this.TriangleIndices.Add(index);
            }
            foreach (Point texCoord in texcoords)
            {
                this.TextureCoordinates.Add(texCoord);
            }
        }
        /// <summary>
        /// Add a Face in positive Y-Direction.
        /// </summary>
        public void AddFacePY()
        {
            Point3D[] positions = new Point3D[]
            {
                new(1,1,0), //p3  
                new(1,1,1), //p2  
                new(0,1,1), //p1
                new(0,1,0), //p0
            };
            Vector3D[] normals = new Vector3D[]
            {
                new(0,1,0),
                new(0,1,0),
                new(0,1,0),
                new(0,1,0),
            };

            int i0 = this.Positions.Count;
            int[] indices = new int[]
            {
                i0+0,i0+3,i0+2,
                i0+0,i0+2,i0+1,
            };
            Point[] texcoords = new Point[]
            {
                new(0,1),
                new(1,1),
                new(1,0),
                new(0,0),
            };

            foreach (Point3D position in positions)
            {
                this.Positions.Add(position);
            }
            foreach (Vector3D normal in normals)
            {
                this.normals.Add(normal);
            }
            foreach (int index in indices)
            {
                this.TriangleIndices.Add(index);
            }
            foreach (Point texCoord in texcoords)
            {
                this.TextureCoordinates.Add(texCoord);
            }
        }
        /// <summary>
        /// Add a Face in negative Y-Direction.
        /// </summary>
        public void AddFaceNY()
        {
            Point3D[] positions = new Point3D[]
            {
                new(0,0,0), //p0
                new(0,0,1), //p1
                new(1,0,1), //p2
                new(1,0,0), //p3
            };
            Vector3D[] normals = new Vector3D[]
            {
                -new Vector3D(0,1,0),
                -new Vector3D(0,1,0),
                -new Vector3D(0,1,0),
                -new Vector3D(0,1,0),
            };

            int i0 = this.Positions.Count;
            int[] indices = new int[]
            {
                i0+0,i0+3,i0+2,
                i0+0,i0+2,i0+1,
            };
            Point[] texcoords = new Point[]
            {
                new(0,1),
                new(1,1),
                new(1,0),
                new(0,0),
            };

            foreach (Point3D position in positions)
            {
                this.Positions.Add(position);
            }
            foreach (Vector3D normal in normals)
            {
                this.normals.Add(normal);
            }
            foreach (int index in indices)
            {
                this.TriangleIndices.Add(index);
            }
            foreach (Point texCoord in texcoords)
            {
                this.TextureCoordinates.Add(texCoord);
            }
        }
        /// <summary>
        /// Adds an extruded surface of the specified line segments.
        /// </summary>
        /// <param name="points">The 2D points describing the line segments to extrude. The number of points must be even.</param>
        /// <param name="axisX">The x-axis.</param>
        /// <param name="p0">The start origin of the extruded surface.</param>
        /// <param name="p1">The end origin of the extruded surface.</param>
        /// <remarks>The y-axis is determined by the cross product between the specified x-axis and the p1-origin vector.</remarks>
        public void AddExtrudedSegments(IList<Point> points, Vector3D axisX, Point3D p0, Point3D p1)
        {
            if (points.Count % 2 != 0)
            {
                throw new InvalidOperationException("The number of points should be even.");
            }
            Vector3D p10 = p1 - p0;
            Vector3D axisY = SharedFunctions.CrossProduct(ref axisX, ref p10);
            axisY.Normalize();
            axisX.Normalize();
            int index0 = this.Positions.Count;

            for (int i = 0; i < points.Count; i++)
            {
                Point p = points[i];
                Vector3D d = (axisX * p.X) + (axisY * p.Y);
                this.Positions.Add(p0 + d);
                this.Positions.Add(p1 + d);

                if (this.normals != null)
                {
                    d.Normalize();
                    this.normals.Add(d);
                    this.normals.Add(d);
                }

                if (this.TextureCoordinates != null)
                {
                    double v = (DoubleOrSingle)i / (points.Count - 1);
                    this.TextureCoordinates.Add(new Point(0, v));
                    this.TextureCoordinates.Add(new Point(1, v));
                }
            }

            int n = points.Count - 1;
            for (int i = 0; i < n; i++)
            {
                int i0 = index0 + (i * 2);
                int i1 = i0 + 1;
                int i2 = i0 + 3;
                int i3 = i0 + 2;

                this.TriangleIndices.Add(i0);
                this.TriangleIndices.Add(i1);
                this.TriangleIndices.Add(i2);

                this.TriangleIndices.Add(i2);
                this.TriangleIndices.Add(i3);
                this.TriangleIndices.Add(i0);
            }
        }
        /// <summary>
        /// Adds a lofted surface.
        /// </summary>
        /// <param name="positionsList">
        /// List of lofting sections.
        /// </param>
        /// <param name="normalList">
        /// The normal list.
        /// </param>
        /// <param name="textureCoordinateList">
        /// The texture coordinate list.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Loft_(3D).
        /// </remarks>
        public void AddLoftedGeometry(
            IList<IList<Point3D>> positionsList,
            IList<IList<Vector3D>> normalList,
            IList<IList<Point>> textureCoordinateList)
        {
            int index0 = this.Positions.Count;
            int n = -1;
            for (int i = 0; i < positionsList.Count; i++)
            {
                IList<Point3D> pc = positionsList[i];

                // check that all curves have same number of points
                if (n == -1)
                {
                    n = pc.Count;
                }

                if (pc.Count != n)
                {
                    throw new InvalidOperationException(AllCurvesShouldHaveTheSameNumberOfPoints);
                }

                // add the points
                foreach (Point3D p in pc)
                {
                    this.Positions.Add(p);
                }

                // add normals
                if (this.normals != null && normalList != null)
                {
                    IList<Vector3D> nc = normalList[i];
                    foreach (Vector3D normal in nc)
                    {
                        this.normals.Add(normal);
                    }
                }

                // add texcoords
                if (this.TextureCoordinates != null && textureCoordinateList != null)
                {
                    IList<Point> tc = textureCoordinateList[i];
                    foreach (Point t in tc)
                    {
                        this.TextureCoordinates.Add(t);
                    }
                }
            }

            for (int i = 0; i + 1 < positionsList.Count; i++)
            {
                for (int j = 0; j + 1 < n; j++)
                {
                    int i0 = index0 + (i * n) + j;
                    int i1 = i0 + n;
                    int i2 = i1 + 1;
                    int i3 = i0 + 1;
                    this.TriangleIndices.Add(i0);
                    this.TriangleIndices.Add(i1);
                    this.TriangleIndices.Add(i2);

                    this.TriangleIndices.Add(i2);
                    this.TriangleIndices.Add(i3);
                    this.TriangleIndices.Add(i0);
                }
            }
        }
        /// <summary>
        /// Adds a single node.
        /// </summary>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <param name="normal">
        /// The normal.
        /// </param>
        /// <param name="textureCoordinate">
        /// The texture coordinate.
        /// </param>
        public void AddNode(Point3D position, Vector3D normal, Point textureCoordinate)
        {
            this.Positions.Add(position);

            this.normals?.Add(normal);

            this.TextureCoordinates?.Add(textureCoordinate);
        }
        /// <summary>
        /// Adds an octahedron.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="forward">The normal vector.</param>
        /// <param name="up">The up vector.</param>
        /// <param name="sideLength">Length of the side.</param>
        /// <param name="height">The half height of the octahedron.</param>
        /// <remarks>See <a href="http://en.wikipedia.org/wiki/Octahedron">Octahedron</a>.</remarks>
        public void AddOctahedron(Point3D center, Vector3D forward, Vector3D up, double sideLength, double height)
        {
            Vector3D right = SharedFunctions.CrossProduct(ref forward, ref up);
            Vector3D n = forward * (DoubleOrSingle)sideLength / 2;
            up *= (DoubleOrSingle)height / 2;
            right *= (DoubleOrSingle)sideLength / 2;

            Point3D p1 = center - n - up - right;
            Point3D p2 = center - n - up + right;
            Point3D p3 = center + n - up + right;
            Point3D p4 = center + n - up - right;
            Point3D p5 = center + up;
            Point3D p6 = center - up;

            this.AddTriangle(p1, p2, p5);
            this.AddTriangle(p2, p3, p5);
            this.AddTriangle(p3, p4, p5);
            this.AddTriangle(p4, p1, p5);

            this.AddTriangle(p2, p1, p6);
            this.AddTriangle(p3, p2, p6);
            this.AddTriangle(p4, p3, p6);
            this.AddTriangle(p1, p4, p6);
        }
        /// <summary>
        /// Adds a (possibly hollow) pipe.
        /// </summary>
        /// <param name="point1">
        /// The start point.
        /// </param>
        /// <param name="point2">
        /// The end point.
        /// </param>
        /// <param name="innerDiameter">
        /// The inner diameter.
        /// </param>
        /// <param name="diameter">
        /// The outer diameter.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the pipe.
        /// </param>
        public void AddPipe(Point3D point1, Point3D point2, double innerDiameter, double diameter, int thetaDiv)
        {
            Vector3D dir = point2 - point1;

            double height = SharedFunctions.Length(ref dir);
            dir.Normalize();

            PointCollection pc = new()
            {
                    new Point(0, (DoubleOrSingle)innerDiameter / 2),
                    new Point(0, (DoubleOrSingle)diameter / 2),
                    new Point(height, (DoubleOrSingle)diameter / 2),
                    new Point(height, (DoubleOrSingle)innerDiameter / 2)
                };

            List<double> tc = new() { 1, 0, 1, 0 };

            if (innerDiameter > 0)
            {
                // Add the inner surface
                pc.Add(new Point(0, (DoubleOrSingle)innerDiameter / 2));
                tc.Add(1);
            }

            this.AddRevolvedGeometry(pc, tc, point1, dir, thetaDiv);
        }
        /// <summary>
        /// Adds a collection of edges as cylinders.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <param name="edges">
        /// The edge indices.
        /// </param>
        /// <param name="diameter">
        /// The diameter of the cylinders.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the cylinders.
        /// </param>
        public void AddPipes(IList<Vector3D> points, IList<int> edges, double diameter = 1, int thetaDiv = 32)
        {
            for (int i = 0; i < edges.Count - 1; i += 2)
            {
                this.AddCylinder((Point3D)points[edges[i]], (Point3D)points[edges[i + 1]], diameter, thetaDiv);
            }
        }
        /// <summary>
        /// Adds a polygon.
        /// </summary>
        /// <param name="points">The 2D points defining the polygon.</param>
        /// <param name="axisX">The x axis.</param>
        /// <param name="axisY">The y axis.</param>
        /// <param name="origin">The origin.</param>
        public void AddPolygon(IList<Point> points, Vector3D axisX, Vector3D axisY, Point3D origin)
        {
            Int32Collection indices = SweepLinePolygonTriangulator.Triangulate(points);
            if (indices != null)
            {
                int index0 = this.Positions.Count;
                foreach (Point p in points)
                {
                    this.Positions.Add(origin + (axisX * p.X) + (axisY * p.Y));
                }

                foreach (int i in indices)
                {
                    this.TriangleIndices.Add(index0 + i);
                }
            }
        }
        /// <summary>
        /// Adds a polygon.
        /// </summary>
        /// <param name="points">
        /// The points of the polygon.
        /// </param>
        /// <remarks>
        /// If the number of points is greater than 4, a triangle fan is used.
        /// </remarks>
        public void AddPolygon(IList<Point3D> points)
        {
            switch (points.Count)
            {
                case 3:
                    this.AddTriangle(points[0], points[1], points[2]);
                    break;
                case 4:
                    this.AddQuad(points[0], points[1], points[2], points[3]);
                    break;
                default:
                    this.AddTriangleFan(points);
                    break;
            }
        }
        /// <summary>
        /// Adds a polygon specified by vertex index (uses a triangle fan).
        /// </summary>
        /// <param name="vertexIndices">The vertex indices.</param>
        public void AddPolygon(IList<int> vertexIndices)
        {
            int n = vertexIndices.Count;
            for (int i = 0; i + 2 < n; i++)
            {
                this.TriangleIndices.Add(vertexIndices[0]);
                this.TriangleIndices.Add(vertexIndices[i + 1]);
                this.TriangleIndices.Add(vertexIndices[i + 2]);
            }
        }
        /// <summary>
        /// Adds a polygon defined by vertex indices (uses the cutting ears algorithm).
        /// </summary>
        /// <param name="vertexIndices">The vertex indices.</param>
        [Obsolete("Please use the faster version AddPolygon instead")]
        public void AddPolygonByCuttingEars(IList<int> vertexIndices)
        {
            List<Point3D> points = vertexIndices.Select(vi => this.Positions[vi]).ToList();
            Polygon3D poly3D = new(points);
            // Transform the polygon to 2D
            Polygon poly2D = poly3D.Flatten();

            // Triangulate
            Int32Collection triangulatedIndices = CuttingEarsTriangulator.Triangulate(poly2D.Points);
            if (triangulatedIndices != null)
            {
                foreach (int i in triangulatedIndices)
                {
                    this.TriangleIndices.Add(vertexIndices[i]);
                }
            }
        }
        /// <summary>
        /// Adds a polygon defined by vertex indices (uses the sweep line algorithm).
        /// </summary>
        /// <param name="vertexIndices">The vertex indices.</param>
        public void AddPolygonByTriangulation(IList<int> vertexIndices)
        {
            List<Point3D> points = vertexIndices.Select(vi => this.Positions[vi]).ToList();
            Polygon3D poly3D = new(points);
            // Transform the polygon to 2D
            Polygon poly2D = poly3D.Flatten();

            // Triangulate
            Int32Collection triangulatedIndices = poly2D.Triangulate();
            if (triangulatedIndices != null)
            {
                foreach (int i in triangulatedIndices)
                {
                    this.TriangleIndices.Add(vertexIndices[i]);
                }
            }
        }
        /// <summary>
        /// Adds a pyramid.
        /// </summary>
        /// <param name="center">
        /// The center.
        /// </param>
        /// <param name="sideLength">
        /// Length of the sides of the pyramid.
        /// </param>
        /// <param name="height">
        /// The height of the pyramid.
        /// </param>
        /// <param name="closeBase">
        /// Add triangles to the base of the pyramid or not.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Pyramid_(geometry).
        /// </remarks>
        public void AddPyramid(Point3D center, double sideLength, double height, bool closeBase = false)
        {
            this.AddPyramid(center, new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), sideLength, height, closeBase);
        }
        /// <summary>
        /// Adds a pyramid.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="forward">The normal vector (normalized).</param>
        /// <param name="up">The 'up' vector (normalized).</param>
        /// <param name="sideLength">Length of the sides of the pyramid.</param>
        /// <param name="height">The height of the pyramid.</param>
        /// <param name="closeBase">Add triangles to the base of the pyramid or not.</param>
        public void AddPyramid(Point3D center, Vector3D forward, Vector3D up, double sideLength, double height, bool closeBase = false)
        {
            Vector3D right = SharedFunctions.CrossProduct(ref forward, ref up);
            Vector3D n = forward * (DoubleOrSingle)sideLength / 2;
            up *= (DoubleOrSingle)height;
            right *= (DoubleOrSingle)sideLength / 2;

            Vector3D down = -up * 1f / 3;
            Vector3D realup = up * 2f / 3;

            Point3D p1 = center - n - right + down;
            Point3D p2 = center - n + right + down;
            Point3D p3 = center + n + right + down;
            Point3D p4 = center + n - right + down;
            Point3D p5 = center + realup;

            this.AddTriangle(p1, p2, p5);
            this.AddTriangle(p2, p3, p5);
            this.AddTriangle(p3, p4, p5);
            this.AddTriangle(p4, p1, p5);
            if (closeBase)
            {
                this.AddTriangle(p1, p3, p2);
                this.AddTriangle(p3, p1, p4);
            }
        }
        /// <summary>
        /// Adds a quad (exactely 4 indices)
        /// </summary>
        /// <param name="vertexIndices">The vertex indices.</param>
        public void AddQuad(IList<int> vertexIndices)
        {
            for (int i = 0; i < 4; i++)
            {
                this.TriangleIndices.Add(vertexIndices[i]);
            }
        }
        /// <summary>
        /// Adds a quadrilateral polygon.
        /// </summary>
        /// <param name="p0">
        /// The first point.
        /// </param>
        /// <param name="p1">
        /// The second point.
        /// </param>
        /// <param name="p2">
        /// The third point.
        /// </param>
        /// <param name="p3">
        /// The fourth point.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Quadrilateral.
        /// </remarks>
        public void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            //// The nodes are arranged in counter-clockwise order
            //// p3               p2
            //// +---------------+
            //// |               |
            //// |               |
            //// +---------------+
            //// origin               p1
            Point uv0 = new(0, 0);
            Point uv1 = new(1, 0);
            Point uv2 = new(1, 1);
            Point uv3 = new(0, 1);
            this.AddQuad(p0, p1, p2, p3, uv0, uv1, uv2, uv3);
        }
        /// <summary>
        /// Adds a quadrilateral polygon.
        /// </summary>
        /// <param name="p0">
        /// The first point.
        /// </param>
        /// <param name="p1">
        /// The second point.
        /// </param>
        /// <param name="p2">
        /// The third point.
        /// </param>
        /// <param name="p3">
        /// The fourth point.
        /// </param>
        /// <param name="uv0">
        /// The first texture coordinate.
        /// </param>
        /// <param name="uv1">
        /// The second texture coordinate.
        /// </param>
        /// <param name="uv2">
        /// The third texture coordinate.
        /// </param>
        /// <param name="uv3">
        /// The fourth texture coordinate.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Quadrilateral.
        /// </remarks>
        public void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point uv0, Point uv1, Point uv2, Point uv3)
        {
            //// The nodes are arranged in counter-clockwise order
            //// p3               p2
            //// +---------------+
            //// |               |
            //// |               |
            //// +---------------+
            //// origin               p1
            int i0 = this.Positions.Count;

            this.Positions.Add(p0);
            this.Positions.Add(p1);
            this.Positions.Add(p2);
            this.Positions.Add(p3);

            if (this.TextureCoordinates != null)
            {
                this.TextureCoordinates.Add(uv0);
                this.TextureCoordinates.Add(uv1);
                this.TextureCoordinates.Add(uv2);
                this.TextureCoordinates.Add(uv3);
            }

            if (this.normals != null)
            {
                Vector3D p10 = p1 - p0;
                Vector3D p30 = p3 - p0;
                Vector3D w = SharedFunctions.CrossProduct(ref p10, ref p30);
                w.Normalize();
                this.normals.Add(w);
                this.normals.Add(w);
                this.normals.Add(w);
                this.normals.Add(w);
            }

            this.TriangleIndices.Add(i0 + 0);
            this.TriangleIndices.Add(i0 + 1);
            this.TriangleIndices.Add(i0 + 2);

            this.TriangleIndices.Add(i0 + 2);
            this.TriangleIndices.Add(i0 + 3);
            this.TriangleIndices.Add(i0 + 0);
        }
        /// <summary>
        /// Adds a list of quadrilateral polygons.
        /// </summary>
        /// <param name="quadPositions">
        /// The points.
        /// </param>
        /// <param name="quadNormals">
        /// The normal vectors.
        /// </param>
        /// <param name="quadTextureCoordinates">
        /// The texture coordinates.
        /// </param>
        public void AddQuads(
            IList<Point3D> quadPositions, IList<Vector3D> quadNormals, IList<Point> quadTextureCoordinates)
        {
            if (quadPositions == null)
            {
                throw new ArgumentNullException(nameof(quadPositions));
            }

            if (this.normals != null && quadNormals == null)
            {
                throw new ArgumentNullException(nameof(quadNormals));
            }

            if (this.TextureCoordinates != null && quadTextureCoordinates == null)
            {
                throw new ArgumentNullException(nameof(quadTextureCoordinates));
            }

            if (quadNormals != null && quadNormals.Count != quadPositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (quadTextureCoordinates != null && quadTextureCoordinates.Count != quadPositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            Debug.Assert(quadPositions.Count > 0 && quadPositions.Count % 4 == 0, "Wrong number of positions.");

            int index0 = this.Positions.Count;
            foreach (Point3D p in quadPositions)
            {
                this.Positions.Add(p);
            }

            if (this.TextureCoordinates != null && quadTextureCoordinates != null)
            {
                foreach (Point tc in quadTextureCoordinates)
                {
                    this.TextureCoordinates.Add(tc);
                }
            }

            if (this.normals != null && quadNormals != null)
            {
                foreach (Vector3D n in quadNormals)
                {
                    this.normals.Add(n);
                }
            }

            int indexEnd = this.Positions.Count;
            for (int i = index0; i + 3 < indexEnd; i++)
            {
                this.TriangleIndices.Add(i);
                this.TriangleIndices.Add(i + 1);
                this.TriangleIndices.Add(i + 2);

                this.TriangleIndices.Add(i + 2);
                this.TriangleIndices.Add(i + 3);
                this.TriangleIndices.Add(i);
            }
        }
        /// <summary>
        /// Adds a rectangular mesh (m x n points).
        /// </summary>
        /// <param name="points">
        /// The one-dimensional array of points. The points are stored row-by-row.
        /// </param>
        /// <param name="columns">
        /// The number of columns in the rectangular mesh.
        /// </param>
        public void AddRectangularMesh(IList<Point3D> points, int columns)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            int index0 = this.Positions.Count;

            foreach (Point3D pt in points)
            {
                this.Positions.Add(pt);
            }

            int rows = points.Count / columns;

            this.AddRectangularMeshTriangleIndices(index0, rows, columns);
            if (this.normals != null)
            {
                this.AddRectangularMeshNormals(index0, rows, columns);
            }

            if (this.TextureCoordinates != null)
            {
                this.AddRectangularMeshTextureCoordinates(rows, columns);
            }
        }
        /// <summary>
        /// Adds a rectangular mesh defined by a two-dimensional array of points.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <param name="texCoords">
        /// The texture coordinates (optional).
        /// </param>
        /// <param name="closed0">
        /// set to <c>true</c> if the mesh is closed in the first dimension.
        /// </param>
        /// <param name="closed1">
        /// set to <c>true</c> if the mesh is closed in the second dimension.
        /// </param>
        public void AddRectangularMesh(
            Point3D[,] points, Point[,] texCoords = null, bool closed0 = false, bool closed1 = false)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            int rows = points.GetUpperBound(0) + 1;
            int columns = points.GetUpperBound(1) + 1;
            int index0 = this.Positions.Count;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    this.Positions.Add(points[i, j]);
                }
            }

            this.AddRectangularMeshTriangleIndices(index0, rows, columns, closed0, closed1);

            if (this.normals != null)
            {
                this.AddRectangularMeshNormals(index0, rows, columns);
            }

            if (this.TextureCoordinates != null)
            {
                if (texCoords != null)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            this.TextureCoordinates.Add(texCoords[i, j]);
                        }
                    }
                }
                else
                {
                    this.AddRectangularMeshTextureCoordinates(rows, columns);
                }
            }
        }
        /// <summary>
        /// Adds a rectangular mesh (m x n points).
        /// </summary>
        /// <param name="points">
        /// The one-dimensional array of points. The points are stored row-by-row.
        /// </param>
        /// <param name="columns">
        /// The number of columns in the rectangular mesh.
        /// </param>
        /// <param name="flipTriangles">
        /// Flip the Triangles.
        /// </param>
        public void AddRectangularMesh(IList<Point3D> points, int columns, bool flipTriangles = false)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            int index0 = this.Positions.Count;

            foreach (Point3D pt in points)
            {
                this.Positions.Add(pt);
            }

            int rows = points.Count / columns;

            if (flipTriangles)
            {
                this.AddRectangularMeshTriangleIndicesFlipped(index0, rows, columns);
            }
            else
            {
                this.AddRectangularMeshTriangleIndices(index0, rows, columns);
            }

            if (this.normals != null)
            {
                this.AddRectangularMeshNormals(index0, rows, columns);
            }

            if (this.TextureCoordinates != null)
            {
                this.AddRectangularMeshTextureCoordinates(rows, columns);
            }
        }
        /// <summary>
        /// Generates a rectangles mesh on the axis-aligned plane given by the box-face.
        /// </summary>
        /// <param name="plane">Box face which determines the plane the grid lies on.</param>
        /// <param name="columns">width of the grid, i.e. horizontal resolution </param>
        /// <param name="rows">height of the grid, i.e. vertical resolution</param>
        /// <param name="width">total size in horizontal </param>
        /// <param name="height">total vertical size</param>
        /// <param name="flipTriangles">flips the triangle faces</param>
        /// <param name="flipTexCoordsUAxis">flips the u-axis (horizontal) of the texture coords.</param>
        /// <param name="flipTexCoordsVAxis">flips the v-axis (vertical) of the tex.coords.</param>
        public void AddRectangularMesh(BoxFaces plane, int columns, int rows, double width, double height, bool flipTriangles = false, bool flipTexCoordsUAxis = false, bool flipTexCoordsVAxis = false)
        {
            // checks
            if (columns < 2 || rows < 2)
            {
                throw new ArgumentNullException("columns or rows too small");
            }
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentNullException("width or height too small");
            }

            // index0
            int index0 = this.Positions.Count;

            // positions
            double stepy = (DoubleOrSingle)height / (rows - 1);
            double stepx = (DoubleOrSingle)width / (columns - 1);
            //rows++;
            //columns++;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    this.Positions.Add(new Point3D(x * stepx, y * stepy, 0));
                }
            }

            // indices
            if (flipTriangles)
            {
                this.AddRectangularMeshTriangleIndicesFlipped(index0, rows, columns);
            }
            else
            {
                this.AddRectangularMeshTriangleIndices(index0, rows, columns);
            }

            // normals
            if (this.normals != null)
            {
                this.AddRectangularMeshNormals(index0, rows, columns);
            }

            // texcoords
            if (this.TextureCoordinates != null)
            {
                this.AddRectangularMeshTextureCoordinates(rows, columns, flipTexCoordsVAxis, flipTexCoordsUAxis);
            }
        }
        /// <summary>
        /// Adds normal vectors for a rectangular mesh.
        /// </summary>
        /// <param name="index0">
        /// The index 0.
        /// </param>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        private void AddRectangularMeshNormals(int index0, int rows, int columns)
        {
            for (int i = 0; i < rows; i++)
            {
                int i1 = i + 1;
                if (i1 == rows)
                {
                    i1--;
                }

                int i0 = i1 - 1;
                for (int j = 0; j < columns; j++)
                {
                    int j1 = j + 1;
                    if (j1 == columns)
                    {
                        j1--;
                    }

                    int j0 = j1 - 1;
                    Vector3D u = Point3D.Subtract(
                        this.Positions[index0 + (i1 * columns) + j0], this.Positions[index0 + (i0 * columns) + j0]);
                    Vector3D v = Point3D.Subtract(
                        this.Positions[index0 + (i0 * columns) + j1], this.Positions[index0 + (i0 * columns) + j0]);
                    Vector3D normal = SharedFunctions.CrossProduct(ref u, ref v);
                    normal.Normalize();
                    this.normals.Add(normal);
                }
            }
        }
        /// <summary>
        /// Adds texture coordinates for a rectangular mesh.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="flipRowsAxis">
        /// Flip the Rows.
        /// </param>
        /// <param name="flipColumnsAxis">
        /// Flip the Columns.
        /// </param>
        private void AddRectangularMeshTextureCoordinates(int rows, int columns, bool flipRowsAxis = false, bool flipColumnsAxis = false)
        {
            for (int i = 0; i < rows; i++)
            {
                double v = flipRowsAxis ? 1 - ((DoubleOrSingle)i / (rows - 1)) : (DoubleOrSingle)i / (rows - 1);

                for (int j = 0; j < columns; j++)
                {
                    double u = flipColumnsAxis ? 1 - ((DoubleOrSingle)j / (columns - 1)) : (DoubleOrSingle)j / (columns - 1);
                    this.TextureCoordinates.Add(new Point(u, v));
                }
            }
        }
        /// <summary>
        /// Add triangle indices for a rectangular mesh.
        /// </summary>
        /// <param name="index0">
        /// The index offset.
        /// </param>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="isSpherical">
        /// set the flag to true to create a sphere mesh (triangles at top and bottom).
        /// </param>
        public void AddRectangularMeshTriangleIndices(int index0, int rows, int columns, bool isSpherical = false)
        {
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < columns - 1; j++)
                {
                    int ij = (i * columns) + j;
                    if (!isSpherical || i > 0)
                    {
                        this.TriangleIndices.Add(index0 + ij);
                        this.TriangleIndices.Add(index0 + ij + 1 + columns);
                        this.TriangleIndices.Add(index0 + ij + 1);
                    }

                    if (!isSpherical || i < rows - 2)
                    {
                        this.TriangleIndices.Add(index0 + ij + 1 + columns);
                        this.TriangleIndices.Add(index0 + ij);
                        this.TriangleIndices.Add(index0 + ij + columns);
                    }
                }
            }
        }
        /// <summary>
        /// Adds triangular indices for a rectangular mesh.
        /// </summary>
        /// <param name="index0">
        /// The index 0.
        /// </param>
        /// <param name="rows">
        /// The rows.
        /// </param>
        /// <param name="columns">
        /// The columns.
        /// </param>
        /// <param name="rowsClosed">
        /// True if rows are closed.
        /// </param>
        /// <param name="columnsClosed">
        /// True if columns are closed.
        /// </param>
        public void AddRectangularMeshTriangleIndices(
            int index0, int rows, int columns, bool rowsClosed, bool columnsClosed)
        {
            int m2 = rows - 1;
            int n2 = columns - 1;
            if (columnsClosed)
            {
                m2++;
            }

            if (rowsClosed)
            {
                n2++;
            }

            for (int i = 0; i < m2; i++)
            {
                for (int j = 0; j < n2; j++)
                {
                    int i00 = index0 + (i * columns) + j;
                    int i01 = index0 + (i * columns) + ((j + 1) % columns);
                    int i10 = index0 + ((i + 1) % rows * columns) + j;
                    int i11 = index0 + ((i + 1) % rows * columns) + ((j + 1) % columns);
                    this.TriangleIndices.Add(i00);
                    this.TriangleIndices.Add(i11);
                    this.TriangleIndices.Add(i01);

                    this.TriangleIndices.Add(i11);
                    this.TriangleIndices.Add(i00);
                    this.TriangleIndices.Add(i10);
                }
            }
        }
        /// <summary>
        /// Add triangle indices for a rectangular mesh with flipped triangles.
        /// </summary>
        /// <param name="index0">
        /// The index offset.
        /// </param>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="isSpherical">
        /// set the flag to true to create a sphere mesh (triangles at top and bottom).
        /// </param>
        private void AddRectangularMeshTriangleIndicesFlipped(int index0, int rows, int columns, bool isSpherical = false)
        {
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < columns - 1; j++)
                {
                    int ij = (i * columns) + j;
                    if (!isSpherical || i > 0)
                    {
                        this.TriangleIndices.Add(index0 + ij);
                        this.TriangleIndices.Add(index0 + ij + 1);
                        this.TriangleIndices.Add(index0 + ij + 1 + columns);
                    }

                    if (!isSpherical || i < rows - 2)
                    {
                        this.TriangleIndices.Add(index0 + ij + 1 + columns);
                        this.TriangleIndices.Add(index0 + ij + columns);
                        this.TriangleIndices.Add(index0 + ij);
                    }
                }
            }
        }
        /// <summary>
        /// Adds a regular icosahedron.
        /// </summary>
        /// <param name="center">
        /// The center.
        /// </param>
        /// <param name="radius">
        /// The radius.
        /// </param>
        /// <param name="shareVertices">
        /// Share vertices if set to <c>true</c> .
        /// </param>
        /// <remarks>
        /// See <a href="http://en.wikipedia.org/wiki/Icosahedron">Wikipedia</a> and <a href="http://www.gamedev.net/community/forums/topic.asp?topic_id=283350">link</a>.
        /// </remarks>
        public void AddRegularIcosahedron(Point3D center, double radius, bool shareVertices)
        {
            double a = (DoubleOrSingle)Math.Sqrt(2.0 / (5.0 + Math.Sqrt(5.0)));
            double b = (DoubleOrSingle)Math.Sqrt(2.0 / (5.0 - Math.Sqrt(5.0)));

            int[] icosahedronIndices = new[]
                {
                    1, 4, 0, 4, 9, 0, 4, 5, 9, 8, 5, 4, 1, 8, 4, 1, 10, 8, 10, 3, 8, 8, 3, 5, 3, 2, 5, 3, 7, 2, 3, 10, 7,
                    10, 6, 7, 6, 11, 7, 6, 0, 11, 6, 1, 0, 10, 1, 6, 11, 0, 9, 2, 11, 9, 5, 2, 9, 11, 2, 7
                };

            Vector3D[] icosahedronVertices = new[]
                {
                    new Vector3D(-a, 0, b), new Vector3D(a, 0, b), new Vector3D(-a, 0, -b), new Vector3D(a, 0, -b),
                    new Vector3D(0, b, a), new Vector3D(0, b, -a), new Vector3D(0, -b, a), new Vector3D(0, -b, -a),
                    new Vector3D(b, a, 0), new Vector3D(-b, a, 0), new Vector3D(b, -a, 0), new Vector3D(-b, -a, 0)
                };

            if (shareVertices)
            {
                int index0 = this.Positions.Count;
                foreach (Vector3D v in icosahedronVertices)
                {
                    this.Positions.Add(center + (v * (DoubleOrSingle)radius));
                }

                foreach (int i in icosahedronIndices)
                {
                    this.TriangleIndices.Add(index0 + i);
                }
            }
            else
            {
                for (int i = 0; i + 2 < icosahedronIndices.Length; i += 3)
                {
                    this.AddTriangle(
                        center + (icosahedronVertices[icosahedronIndices[i]] * (DoubleOrSingle)radius),
                        center + (icosahedronVertices[icosahedronIndices[i + 1]] * (DoubleOrSingle)radius),
                        center + (icosahedronVertices[icosahedronIndices[i + 2]] * (DoubleOrSingle)radius));
                }
            }
        }
        /// <summary>
        /// Adds a surface of revolution.
        /// </summary>
        /// <param name="points">The points (x coordinates are distance from the origin along the axis of revolution, y coordinates are radius, )</param>
        /// <param name="textureValues">The v texture coordinates, one for each point in the <paramref name="points" /> list.</param>
        /// <param name="origin">The origin of the revolution axis.</param>
        /// <param name="direction">The direction of the revolution axis.</param>
        /// <param name="thetaDiv">The number of divisions around the mesh.</param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Surface_of_revolution.
        /// </remarks>
        public void AddRevolvedGeometry(IList<Point> points, IList<double> textureValues, Point3D origin, Vector3D direction, int thetaDiv)
        {
            direction.Normalize();

            // Find two unit vectors orthogonal to the specified direction
            Vector3D u = direction.FindAnyPerpendicular();
            Vector3D v = SharedFunctions.CrossProduct(ref direction, ref u);
            u.Normalize();
            v.Normalize();

            IList<Point> circle = GetCircle(thetaDiv);

            int index0 = this.Positions.Count;
            int n = points.Count;

            int totalNodes = (points.Count - 1) * 2 * thetaDiv;
            int rowNodes = (points.Count - 1) * 2;

            for (int i = 0; i < thetaDiv; i++)
            {
                Vector3D w = (v * circle[i].X) + (u * circle[i].Y);

                for (int j = 0; j + 1 < n; j++)
                {
                    // Add segment
                    Point3D q1 = origin + (direction * points[j].X) + (w * points[j].Y);
                    Point3D q2 = origin + (direction * points[j + 1].X) + (w * points[j + 1].Y);

                    // TODO: should not add segment if q1==q2 (corner point)
                    // const double eps = 1e-6;
                    // if (Point3D.Subtract(q1, q2).LengthSquared < eps)
                    // continue;
                    this.Positions.Add(q1);
                    this.Positions.Add(q2);

                    if (this.normals != null)
                    {
                        double tx = points[j + 1].X - points[j].X;
                        double ty = points[j + 1].Y - points[j].Y;
                        Vector3D normal = (-direction * ty) + (w * tx);
                        normal.Normalize();
                        this.normals.Add(normal);
                        this.normals.Add(normal);
                    }

                    if (this.TextureCoordinates != null)
                    {
                        this.TextureCoordinates.Add(new Point((DoubleOrSingle)i / (thetaDiv - 1), textureValues == null ? (DoubleOrSingle)j / (n - 1) : (DoubleOrSingle)textureValues[j]));
                        this.TextureCoordinates.Add(new Point((DoubleOrSingle)i / (thetaDiv - 1), textureValues == null ? (DoubleOrSingle)(j + 1) / (n - 1) : (DoubleOrSingle)textureValues[j + 1]));
                    }

                    int i0 = index0 + (i * rowNodes) + (j * 2);
                    int i1 = i0 + 1;
                    int i2 = index0 + ((((i + 1) * rowNodes) + (j * 2)) % totalNodes);
                    int i3 = i2 + 1;

                    this.TriangleIndices.Add(i1);
                    this.TriangleIndices.Add(i0);
                    this.TriangleIndices.Add(i2);

                    this.TriangleIndices.Add(i1);
                    this.TriangleIndices.Add(i2);
                    this.TriangleIndices.Add(i3);
                }
            }
        }
        /// <summary>
        /// Adds a sphere.
        /// </summary>
        /// <param name="center">
        /// The center of the sphere.
        /// </param>
        /// <param name="radius">
        /// The radius of the sphere.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the sphere.
        /// </param>
        /// <param name="phiDiv">
        /// The number of divisions from top to bottom of the sphere.
        /// </param>
        public void AddSphere(Point3D center, double radius = 1, int thetaDiv = 32, int phiDiv = 32)
        {
            this.AddEllipsoid(center, radius, radius, radius, thetaDiv, phiDiv);
        }

#if !NETFX_CORE
        /// <summary>
        /// Adds a sphere (by subdividing a regular icosahedron).
        /// </summary>
        /// <param name="center">
        /// The center of the sphere.
        /// </param>
        /// <param name="radius">
        /// The radius of the sphere.
        /// </param>
        /// <param name="subdivisions">
        /// The number of triangular subdivisions of the original icosahedron.
        /// </param>
        /// <remarks>
        /// See <a href="http://www.fho-emden.de/~hoffmann/ikos27042002.pdf">link</a>.
        /// </remarks>
        public void AddSubdivisionSphere(Point3D center, double radius, int subdivisions)
        {
            int p0 = this.Positions.Count;
            this.Append(GetUnitSphere(subdivisions));
            int p1 = this.Positions.Count;
            for (int i = p0; i < p1; i++)
            {
                Vector3D pVec = (Vector3D)this.Positions[i];
                this.Positions[i] = center + ((DoubleOrSingle)radius * pVec);
            }
        }
#endif

        /// <summary>
        /// Adds a surface of revolution.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="section">The points defining the curve to revolve.</param>
        /// <param name="sectionIndices">The indices of the line segments of the section.</param>
        /// <param name="thetaDiv">The number of divisions.</param>
        /// <param name="textureValues">The texture values.</param>
        public void AddSurfaceOfRevolution(
            Point3D origin, Vector3D axis, IList<Point> section, IList<int> sectionIndices,
            int thetaDiv = 37, IList<double> textureValues = null)
        {
            if (this.TextureCoordinates != null && textureValues == null)
            {
                throw new ArgumentNullException(nameof(textureValues));
            }

            if (textureValues != null && textureValues.Count != section.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            axis.Normalize();

            // Find two unit vectors orthogonal to the specified direction
            Vector3D u = axis.FindAnyPerpendicular();
            Vector3D v = SharedFunctions.CrossProduct(ref axis, ref u);
            IList<Point> circle = GetCircle(thetaDiv);
            int n = section.Count;
            int index0 = this.Positions.Count;
            for (int i = 0; i < thetaDiv; i++)
            {
                Vector3D w = (v * circle[i].X) + (u * circle[i].Y);
                for (int j = 0; j < n; j++)
                {
                    Point3D q1 = origin + (axis * section[j].Y) + (w * section[j].X);
                    this.Positions.Add(q1);
                    if (this.normals != null)
                    {
                        double tx = section[j + 1].X - section[j].X;
                        double ty = section[j + 1].Y - section[j].Y;
                        Vector3D normal = (-axis * ty) + (w * tx);
                        normal.Normalize();
                        this.normals.Add(normal);
                    }

                    this.TextureCoordinates?.Add(new Point((DoubleOrSingle)i / (thetaDiv - 1), textureValues == null ? (DoubleOrSingle)j / (n - 1) : (DoubleOrSingle)textureValues[j]));
                }
            }
            for (int i = 0; i < thetaDiv; i++)
            {
                int ii = (i + 1) % thetaDiv;
                for (int j = 0; j + 1 < sectionIndices.Count; j += 2)
                {
                    int j0 = sectionIndices[j];
                    int j1 = sectionIndices[j + 1];

                    int i0 = index0 + (i * n) + j0;
                    int i1 = index0 + (ii * n) + j0;
                    int i2 = index0 + (i * n) + j1;
                    int i3 = index0 + (ii * n) + j1;

                    this.TriangleIndices.Add(i0);
                    this.TriangleIndices.Add(i1);
                    this.TriangleIndices.Add(i3);

                    this.TriangleIndices.Add(i3);
                    this.TriangleIndices.Add(i2);
                    this.TriangleIndices.Add(i0);
                }
            }
        }
        /// <summary>
        /// Add a tetrahedron.
        /// </summary>
        /// <param name="center">The Center of Mass.</param>
        /// <param name="forward">Direction to first Base-Point (in Base-Plane).</param>
        /// <param name="up">Up Vector.</param>
        /// <param name="sideLength">The Sidelength.</param>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Tetrahedron and
        /// https://en.wikipedia.org/wiki/Equilateral_triangle.
        /// </remarks>
        public void AddTetrahedron(Point3D center, Vector3D forward, Vector3D up, double sideLength)
        {
            // Helper Variables
            Vector3D right = SharedFunctions.CrossProduct(ref up, ref forward);
            double heightSphere = (DoubleOrSingle)Math.Sqrt(6) / 3 * (DoubleOrSingle)sideLength;
            double radiusSphere = (DoubleOrSingle)Math.Sqrt(6) / 4 * (DoubleOrSingle)sideLength;
            double heightFace = (DoubleOrSingle)Math.Sqrt(3) / 2 * (DoubleOrSingle)sideLength;
            double radiusFace = (DoubleOrSingle)Math.Sqrt(3) / 3 * (DoubleOrSingle)sideLength;
            double smallHeightSphere = heightSphere - radiusSphere;
            double smallHeightFace = heightFace - radiusFace;
            double halfLength = (DoubleOrSingle)sideLength * 0.5f;

            // The Vertex Positions
            Point3D p1 = center + (forward * radiusFace) - (up * smallHeightSphere);
            Point3D p2 = center - (forward * smallHeightFace) - (right * halfLength) - (up * smallHeightSphere);
            Point3D p3 = center - (forward * smallHeightFace) + (right * halfLength) - (up * smallHeightSphere);
            Point3D p4 = center + (up * radiusSphere);

            // Triangles
            this.AddTriangle(p1, p2, p3);
            this.AddTriangle(p1, p4, p2);
            this.AddTriangle(p2, p4, p3);
            this.AddTriangle(p3, p4, p1);
        }
        /// <summary>
        /// Adds a torus.
        /// </summary>
        /// <param name="torusDiameter">The diameter of the torus.</param>
        /// <param name="tubeDiameter">The diameter of the torus "tube".</param>
        /// <param name="thetaDiv">The number of subdivisions around the torus.</param>
        /// <param name="phiDiv">The number of subdividions of the torus' "tube.</param>
        public void AddTorus(double torusDiameter, double tubeDiameter, int thetaDiv = 36, int phiDiv = 24)
        {
            int positionsCount = this.Positions.Count;
            // No Torus Diameter means we treat the Visual3D like a Sphere
            if (torusDiameter == 0.0)
            {
                this.AddSphere(new Point3D(), tubeDiameter, thetaDiv, phiDiv);
            }
            // If the second Diameter is zero, we can't build out torus
            else if (tubeDiameter == 0.0)
            {
                throw new Exception("Torus must have a Diameter bigger than 0");
            }
            // Values result in a Torus
            else
            {
                // Points of the Cross-Section of the torus "tube"
                IList<Point> crossSectionPoints;
                // Self-intersecting Torus, if the "Tube" Diameter is bigger than the Torus Diameter
                bool selfIntersecting = tubeDiameter > torusDiameter;
                if (selfIntersecting)
                {
                    // Angle-Calculations for Circle Segment https://de.wikipedia.org/wiki/Gleichschenkliges_Dreieck
                    double angleIcoTriangle = (DoubleOrSingle)Math.Acos(1 - (torusDiameter * torusDiameter / (2 * (tubeDiameter * tubeDiameter * .25))));
                    double circleAngle = Math.PI + angleIcoTriangle;
                    double offset = -circleAngle / 2;
                    // The Cross-Section is defined by only a Segment of a Circle
                    crossSectionPoints = GetCircleSegment(phiDiv, circleAngle, offset);
                }
                // "normal" Torus (with a Circle as Cross-Section of the Torus
                else
                {
                    crossSectionPoints = GetCircle(phiDiv, true);
                }
                // Transform Crosssection to real Size
                crossSectionPoints = crossSectionPoints.Select(p => new Point((DoubleOrSingle)p.X * (DoubleOrSingle)tubeDiameter * .5f, (DoubleOrSingle)p.Y * (DoubleOrSingle)tubeDiameter * .5f)).ToList();
                // Transform the Cross-Section Points to 3D Space
                List<Point3D> crossSection3DPoints = crossSectionPoints.Select(p => new Point3D(p.X, 0, p.Y)).ToList();

                // Add the needed Vertex-Positions of the Torus
                for (int i = 0; i < thetaDiv; i++)
                {
                    // Angle of the current Cross-Section in the XY-Plane
                    double angle = Math.PI * 2 * ((double)i / thetaDiv);
                    // Rotate the Cross-Section around the Origin by using the angle and the defined torusDiameter
                    List<Point3D> rotatedPoints = crossSection3DPoints.Select(p3D => new Point3D((DoubleOrSingle)Math.Cos(angle) * (DoubleOrSingle)(p3D.X + (torusDiameter * .5f)), (DoubleOrSingle)Math.Sin(angle) * (DoubleOrSingle)(p3D.X + (torusDiameter * .5f)), p3D.Z)).ToList();
                    for (int j = 0; j < phiDiv; j++)
                    {
                        // If selfintersecting Torus, skip the first and last Point of the Cross-Sections, when not the first Cross Section.
                        // We only need the first and last Point of the first Cross-Section once!
                        if (selfIntersecting && i > 0 && (j == 0 || j == phiDiv - 1))
                        {
                            continue;
                        }
                        // Add the Position
                        this.Positions.Add(rotatedPoints[j]);
                    }
                }
                // Add all Normals, if they need to be calculated
                if (this.normals != null)
                {
                    for (int i = 0; i < thetaDiv; i++)
                    {
                        // Transform the Cross-Section as well as the Origin of the Cross-Section
                        double angle = Math.PI * 2 * ((double)i / thetaDiv);
                        List<Point3D> rotatedPoints = crossSection3DPoints.Select(p3D => new Point3D((DoubleOrSingle)Math.Cos(angle) * (DoubleOrSingle)(p3D.X + (torusDiameter * .5f)), (DoubleOrSingle)Math.Sin(angle) * (DoubleOrSingle)(p3D.X + (torusDiameter * .5f)), p3D.Z)).ToList();
                        // We don't need the first and last Point of the rotated Points, if we are not in the first Cross-Section
                        if (selfIntersecting && i > 0)
                        {
                            rotatedPoints.RemoveAt(0);
                            rotatedPoints.RemoveAt(rotatedPoints.Count - 1);
                        }
                        // Transform the Center of the Cross-Section
                        Point3D rotatedOrigin = new((DoubleOrSingle)Math.Cos(angle) * (DoubleOrSingle)torusDiameter * .5f, (DoubleOrSingle)Math.Sin(angle) * (DoubleOrSingle)torusDiameter * .5f, 0);
                        // Add the Normal of the Vertex
                        for (int j = 0; j < rotatedPoints.Count; j++)
                        {
                            // The default Normal has the same Direction as the Vector from the Center to the Vertex
                            Vector3D normal = rotatedPoints[j] - rotatedOrigin;
                            normal.Normalize();
                            // If self-intersecting Torus and first Point of first Cross-Section,
                            // modify Normal
                            if (selfIntersecting && i == 0 && j == 0)
                            {
                                normal = new Vector3D(0, 0, -1);
                            }
                            // If self-intersecting Torus and last Point of first Cross-Section
                            // modify Normal
                            else if (selfIntersecting && i == 0 && j == phiDiv - 1)
                            {
                                normal = new Vector3D(0, 0, 1);
                            }
                            // Add the Normal
                            this.normals.Add(normal);
                        }
                    }
                }
                // Add all Texture Coordinates, if they need to be calculated
                if (this.TextureCoordinates != null)
                {
                    // For all Points, calculate a simple uv Coordinate
                    for (int i = 0; i < thetaDiv; i++)
                    {
                        // Determine the Number of Vertices of this Cross-Section present in the positions Collection
                        int numCS = selfIntersecting && i > 0 ? phiDiv - 2 : phiDiv;
                        for (int j = 0; j < numCS; j++)
                        {
                            // Calculate u- and v- Coordinates for the Points
                            double u = (DoubleOrSingle)i / thetaDiv;
                            DoubleOrSingle v = i > 0 && selfIntersecting ? (DoubleOrSingle)(j + 1) / phiDiv : (DoubleOrSingle)j / phiDiv;
                            // Add the Texture-Coordinate
                            this.TextureCoordinates.Add(new Point(u, v));
                        }
                    }
                }
                // Add Triangle-Indices
                for (int i = 0; i < thetaDiv; i++)
                {
                    // Normal non-selfintersecting Torus
                    // Just add Triangle-Strips between all neighboring Cross-Sections
                    if (!selfIntersecting)
                    {
                        int firstPointIdx = i * phiDiv;
                        int firstPointIdxNextCircle = (i + 1) % thetaDiv * phiDiv;
                        for (int j = 0; j < phiDiv; j++)
                        {
                            int jNext = (j + 1) % phiDiv;
                            this.TriangleIndices.Add(firstPointIdx + j + positionsCount);
                            this.TriangleIndices.Add(firstPointIdx + jNext + positionsCount);
                            this.TriangleIndices.Add(firstPointIdxNextCircle + j + positionsCount);

                            this.TriangleIndices.Add(firstPointIdxNextCircle + j + positionsCount);
                            this.TriangleIndices.Add(firstPointIdx + jNext + positionsCount);
                            this.TriangleIndices.Add(firstPointIdxNextCircle + jNext + positionsCount);
                        }
                    }
                    // Selfintersecting Torus
                    else
                    {
                        // Add intermediate Triangles like for the non-selfintersecting Torus
                        // Skip the first and last Triangles, the "Caps" will be added later
                        // Determine the Index of the first Point of the first Cross-Section
                        int firstPointIdx = (i * (phiDiv - 2)) + 1;
                        firstPointIdx += i > 0 ? 1 : 0;
                        // Determine the Index of the first Point of the next Cross-Section
                        int firstPointIdxNextCircle = phiDiv + firstPointIdx - 1;
                        firstPointIdxNextCircle -= i > 0 ? 1 : 0;
                        if (firstPointIdxNextCircle >= this.Positions.Count)
                        {
                            firstPointIdxNextCircle %= this.Positions.Count;
                            firstPointIdxNextCircle++;
                        }
                        // Add Triangles between the "middle" Parts of the neighboring Cross-Sections
                        for (int j = 1; j < phiDiv - 2; j++)
                        {
                            this.TriangleIndices.Add(firstPointIdx + j - 1 + positionsCount);
                            this.TriangleIndices.Add(firstPointIdxNextCircle + j - 1 + positionsCount);
                            this.TriangleIndices.Add(firstPointIdx + j + positionsCount);

                            this.TriangleIndices.Add(firstPointIdxNextCircle + j - 1 + positionsCount);
                            this.TriangleIndices.Add(firstPointIdxNextCircle + j + positionsCount);
                            this.TriangleIndices.Add(firstPointIdx + j + positionsCount);
                        }
                    }
                }
                // For selfintersecting Tori
                if (selfIntersecting)
                {
                    // Add bottom Cap by creating a List of Vertex-Indices
                    // and using them to create a Triangle-Fan
                    List<int> verts = new()
                    {
                        0
                    };
                    for (int i = 0; i < thetaDiv; i++)
                    {
                        if (i == 0)
                        {
                            verts.Add(1 + positionsCount);
                        }
                        else
                        {
                            verts.Add(phiDiv + ((i - 1) * (phiDiv - 2)) + positionsCount);
                        }
                    }
                    verts.Add(1 + positionsCount);
                    verts.Reverse();
                    this.AddTriangleFan(verts);

                    // Add top Cap by creating a List of Vertex-Indices
                    // and using them to create a Triangle-Fan
                    verts = new List<int>
                    {
                        phiDiv - 1 + positionsCount
                    };
                    for (int i = 0; i < thetaDiv; i++)
                    {
                        if (i == 0)
                        {
                            verts.Add(phiDiv - 2 + positionsCount);
                        }
                        else
                        {
                            verts.Add(phiDiv + (i * (phiDiv - 2)) - 1 + positionsCount);
                        }
                    }
                    verts.Add(phiDiv - 2 + positionsCount);
                    this.AddTriangleFan(verts);
                }
            }
        }
        /// <summary>
        /// Adds a triangle (exactely 3 indices)
        /// </summary>
        /// <param name="vertexIndices">The vertex indices.</param>
        public void AddTriangle(IList<int> vertexIndices)
        {
            for (int i = 0; i < 3; i++)
            {
                this.TriangleIndices.Add(vertexIndices[i]);
            }
        }
        /// <summary>
        /// Adds a triangle.
        /// </summary>
        /// <param name="p0">
        /// The first point.
        /// </param>
        /// <param name="p1">
        /// The second point.
        /// </param>
        /// <param name="p2">
        /// The third point.
        /// </param>
        public void AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            Point uv0 = new(0, 0);
            Point uv1 = new(1, 0);
            Point uv2 = new(0, 1);
            this.AddTriangle(p0, p1, p2, uv0, uv1, uv2);
        }
        /// <summary>
        /// Adds a triangle.
        /// </summary>
        /// <param name="p0">
        /// The first point.
        /// </param>
        /// <param name="p1">
        /// The second point.
        /// </param>
        /// <param name="p2">
        /// The third point.
        /// </param>
        /// <param name="uv0">
        /// The first texture coordinate.
        /// </param>
        /// <param name="uv1">
        /// The second texture coordinate.
        /// </param>
        /// <param name="uv2">
        /// The third texture coordinate.
        /// </param>
        public void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Point uv0, Point uv1, Point uv2)
        {
            int i0 = this.Positions.Count;

            this.Positions.Add(p0);
            this.Positions.Add(p1);
            this.Positions.Add(p2);

            if (this.TextureCoordinates != null)
            {
                this.TextureCoordinates.Add(uv0);
                this.TextureCoordinates.Add(uv1);
                this.TextureCoordinates.Add(uv2);
            }

            if (this.normals != null)
            {
                Vector3D p10 = p1 - p0;
                Vector3D p20 = p2 - p0;
                Vector3D w = SharedFunctions.CrossProduct(ref p10, ref p20);
                w.Normalize();
                this.normals.Add(w);
                this.normals.Add(w);
                this.normals.Add(w);
            }

            this.TriangleIndices.Add(i0 + 0);
            this.TriangleIndices.Add(i0 + 1);
            this.TriangleIndices.Add(i0 + 2);
        }
        /// <summary>
        /// Adds a triangle fan.
        /// </summary>
        /// <param name="vertices">
        /// The vertex indices of the triangle fan.
        /// </param>
        public void AddTriangleFan(IList<int> vertices)
        {
            for (int i = 0; i + 2 < vertices.Count; i++)
            {
                this.TriangleIndices.Add(vertices[0]);
                this.TriangleIndices.Add(vertices[i + 1]);
                this.TriangleIndices.Add(vertices[i + 2]);
            }
        }
        /// <summary>
        /// Adds a triangle fan to the mesh
        /// </summary>
        /// <param name="fanPositions">
        /// The points of the triangle fan.
        /// </param>
        /// <param name="fanNormals">
        /// The normal vectors of the triangle fan.
        /// </param>
        /// <param name="fanTextureCoordinates">
        /// The texture coordinates of the triangle fan.
        /// </param>
        public void AddTriangleFan(
            IList<Point3D> fanPositions, IList<Vector3D> fanNormals = null, IList<Point> fanTextureCoordinates = null)
        {
            if (this.Positions == null)
            {
                throw new ArgumentNullException(nameof(fanPositions));
            }

            if (this.normals != null && fanNormals == null)
            {
                throw new ArgumentNullException(nameof(fanNormals));
            }

            if (this.TextureCoordinates != null && fanTextureCoordinates == null)
            {
                throw new ArgumentNullException(nameof(fanTextureCoordinates));
            }

            if (fanPositions.Count < 3)
            {
                return;
            }

            int index0 = this.Positions.Count;
            foreach (Point3D p in fanPositions)
            {
                this.Positions.Add(p);
            }

            if (this.TextureCoordinates != null && fanTextureCoordinates != null)
            {
                foreach (Point tc in fanTextureCoordinates)
                {
                    this.TextureCoordinates.Add(tc);
                }
            }

            if (this.normals != null && fanNormals != null)
            {
                foreach (Vector3D n in fanNormals)
                {
                    this.normals.Add(n);
                }
            }

            int indexEnd = this.Positions.Count;
            for (int i = index0; i + 2 < indexEnd; i++)
            {
                this.TriangleIndices.Add(index0);
                this.TriangleIndices.Add(i + 1);
                this.TriangleIndices.Add(i + 2);
            }
        }
        /// <summary>
        /// Adds a list of triangles.
        /// </summary>
        /// <param name="trianglePositions">
        /// The points (the number of points must be a multiple of 3).
        /// </param>
        /// <param name="triangleNormals">
        /// The normal vectors (corresponding to the points).
        /// </param>
        /// <param name="triangleTextureCoordinates">
        /// The texture coordinates (corresponding to the points).
        /// </param>
        public void AddTriangles(
            IList<Point3D> trianglePositions, IList<Vector3D> triangleNormals = null, IList<Point> triangleTextureCoordinates = null)
        {
            if (trianglePositions == null)
            {
                throw new ArgumentNullException(nameof(trianglePositions));
            }

            if (this.normals != null && triangleNormals == null)
            {
                throw new ArgumentNullException(nameof(triangleNormals));
            }

            if (this.TextureCoordinates != null && triangleTextureCoordinates == null)
            {
                throw new ArgumentNullException(nameof(triangleTextureCoordinates));
            }

            if (trianglePositions.Count % 3 != 0)
            {
                throw new InvalidOperationException(WrongNumberOfPositions);
            }

            if (triangleNormals != null && triangleNormals.Count != trianglePositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (triangleTextureCoordinates != null && triangleTextureCoordinates.Count != trianglePositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            int index0 = this.Positions.Count;
            foreach (Point3D p in trianglePositions)
            {
                this.Positions.Add(p);
            }

            if (this.TextureCoordinates != null && triangleTextureCoordinates != null)
            {
                foreach (Point tc in triangleTextureCoordinates)
                {
                    this.TextureCoordinates.Add(tc);
                }
            }

            if (this.normals != null && triangleNormals != null)
            {
                foreach (Vector3D n in triangleNormals)
                {
                    this.normals.Add(n);
                }
            }

            int indexEnd = this.Positions.Count;
            for (int i = index0; i < indexEnd; i++)
            {
                this.TriangleIndices.Add(i);
            }
        }
        /// <summary>
        /// Adds a triangle strip to the mesh.
        /// </summary>
        /// <param name="stripPositions">
        /// The points of the triangle strip.
        /// </param>
        /// <param name="stripNormals">
        /// The normal vectors of the triangle strip.
        /// </param>
        /// <param name="stripTextureCoordinates">
        /// The texture coordinates of the triangle strip.
        /// </param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Triangle_strip.
        /// </remarks>
        public void AddTriangleStrip(
            IList<Point3D> stripPositions, IList<Vector3D> stripNormals = null, IList<Point> stripTextureCoordinates = null)
        {
            if (stripPositions == null)
            {
                throw new ArgumentNullException(nameof(stripPositions));
            }

            if (this.normals != null && stripNormals == null)
            {
                throw new ArgumentNullException(nameof(stripNormals));
            }

            if (this.TextureCoordinates != null && stripTextureCoordinates == null)
            {
                throw new ArgumentNullException(nameof(stripTextureCoordinates));
            }

            if (stripNormals != null && stripNormals.Count != stripPositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (stripTextureCoordinates != null && stripTextureCoordinates.Count != stripPositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            int index0 = this.Positions.Count;
            for (int i = 0; i < stripPositions.Count; i++)
            {
                this.Positions.Add(stripPositions[i]);
                if (this.normals != null && stripNormals != null)
                {
                    this.normals.Add(stripNormals[i]);
                }

                if (this.TextureCoordinates != null && stripTextureCoordinates != null)
                {
                    this.TextureCoordinates.Add(stripTextureCoordinates[i]);
                }
            }

            int indexEnd = this.Positions.Count;
            for (int i = index0; i + 2 < indexEnd; i += 2)
            {
                this.TriangleIndices.Add(i);
                this.TriangleIndices.Add(i + 1);
                this.TriangleIndices.Add(i + 2);

                if (i + 3 < indexEnd)
                {
                    this.TriangleIndices.Add(i + 1);
                    this.TriangleIndices.Add(i + 3);
                    this.TriangleIndices.Add(i + 2);
                }
            }
        }
        /// <summary>
        /// Adds a tube.
        /// </summary>
        /// <param name="path">
        /// A list of points defining the centers of the tube.
        /// </param>
        /// <param name="values">
        /// The texture coordinate X-values.
        /// </param>
        /// <param name="diameters">
        /// The diameters.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the tube.
        /// </param>
        /// <param name="isTubeClosed">
        /// Set to true if the tube path is closed.
        /// </param>
        /// <param name="frontCap">
        /// Create a front Cap or not.
        /// </param>
        /// <param name="backCap">
        /// Create a back Cap or not.
        /// </param>
        public void AddTube(IList<Point3D> path, double[] values, double[] diameters, int thetaDiv, bool isTubeClosed, bool frontCap = false, bool backCap = false)
        {
            IList<Point> circle = GetCircle(thetaDiv);
            this.AddTube(path, values, diameters, circle, isTubeClosed, true, frontCap, backCap);
        }
        /// <summary>
        /// Adds a tube.
        /// </summary>
        /// <param name="path">
        /// A list of points defining the centers of the tube.
        /// </param>
        /// <param name="diameter">
        /// The diameter of the tube.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the tube.
        /// </param>
        /// <param name="isTubeClosed">
        /// Set to true if the tube path is closed.
        /// </param>
        /// <param name="frontCap">
        /// Generate front Cap.
        /// </param>
        /// <param name="backCap">
        /// Generate back Cap.
        /// </param>
        public void AddTube(IList<Point3D> path, double diameter, int thetaDiv, bool isTubeClosed, bool frontCap = false, bool backCap = false)
        {
            this.AddTube(path, null, new[] { diameter }, thetaDiv, isTubeClosed, frontCap, backCap);
        }
        /// <summary>
        /// Adds a tube with a custom section.
        /// </summary>
        /// <param name="path">
        /// A list of points defining the centers of the tube.
        /// </param>
        /// <param name="values">
        /// The texture coordinate X values (optional).
        /// </param>
        /// <param name="diameters">
        /// The diameters (optional).
        /// </param>
        /// <param name="section">
        /// The section to extrude along the tube path.
        /// </param>
        /// <param name="isTubeClosed">
        /// If the tube is closed set to <c>true</c> .
        /// </param>
        /// <param name="isSectionClosed">
        /// if set to <c>true</c> [is section closed].
        /// </param>
        /// <param name="frontCap">
        /// Create a front Cap or not.
        /// </param>
        /// <param name="backCap">
        /// Create a back Cap or not.
        /// </param>
        public void AddTube(
            IList<Point3D> path, IList<double> values, IList<double> diameters,
            IList<Point> section, bool isTubeClosed, bool isSectionClosed, bool frontCap = false, bool backCap = false)
        {
            if (values != null && values.Count == 0)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            if (diameters != null && diameters.Count == 0)
            {
                throw new InvalidOperationException(WrongNumberOfDiameters);
            }

            int index0 = this.Positions.Count;
            int pathLength = path.Count;
            int sectionLength = section.Count;
            if (pathLength < 2 || sectionLength < 2)
            {
                return;
            }

            Vector3D up = (path[1] - path[0]).FindAnyPerpendicular();

            int diametersCount = diameters != null ? diameters.Count : 0;
            int valuesCount = values != null ? values.Count : 0;

            //*******************************
            //*** PROPOSED SOLUTION *********
            Vector3D lastUp = new();
            Vector3D lastForward = new();
            //*** PROPOSED SOLUTION *********
            //*******************************

            for (int i = 0; i < pathLength; i++)
            {
                double r = diameters != null ? (DoubleOrSingle)diameters[i % diametersCount] / 2 : 1;
                int i0 = i > 0 ? i - 1 : i;
                int i1 = i + 1 < pathLength ? i + 1 : i;
                Vector3D forward = path[i1] - path[i0];
                Vector3D right = SharedFunctions.CrossProduct(ref up, ref forward);

                up = SharedFunctions.CrossProduct(ref forward, ref right);
                up.Normalize();
                right.Normalize();
                Vector3D u = right;
                Vector3D v = up;

                //*******************************
                //*** PROPOSED SOLUTION *********
                // ** I think this will work because if path[n-1] is same point, 
                // ** it is always a reflection of the current move
                // ** so reversing the last move vector should work?
                //*******************************
                if (u.IsUndefined() || v.IsUndefined())
                {
                    forward = lastForward;
                    forward *= -1;
                    up = lastUp;
                    //** Please verify that negation of "up" is correct here
                    up *= -1;
                    right = SharedFunctions.CrossProduct(ref up, ref forward);
                    up.Normalize();
                    right.Normalize();
                    u = right;
                    v = up;
                }
                lastForward = forward;
                lastUp = up;

                //*** PROPOSED SOLUTION *********
                //*******************************
                for (int j = 0; j < sectionLength; j++)
                {
                    Vector3D w = (section[j].X * u * r) + (section[j].Y * v * r);
                    Point3D q = path[i] + w;
                    this.Positions.Add(q);
                    if (this.normals != null)
                    {
                        w.Normalize();
                        this.normals.Add(w);
                    }

                    this.TextureCoordinates?.Add(
                            values != null
                                ? new Point((DoubleOrSingle)values[i % valuesCount], (DoubleOrSingle)j / (sectionLength - 1))
                                : new Point());
                }
            }

            this.AddRectangularMeshTriangleIndices(index0, pathLength, sectionLength, isSectionClosed, isTubeClosed);

            if (frontCap || (backCap && path.Count > 1))
            {
                Vector3D[] normals = new Vector3D[section.Count];
                Point[] fanTextures = new Point[section.Count];
                int count = path.Count;
                if (backCap)
                {
                    Point3D[] circleBack = this.Positions.Skip(this.Positions.Count - section.Count).Take(section.Count).Reverse().ToArray();
                    Vector3D normal = path[count - 1] - path[count - 2];
                    normal.Normalize();
                    for (int i = 0; i < normals.Length; ++i)
                    {
                        normals[i] = normal;
                    }
                    this.AddTriangleFan(circleBack, normals, fanTextures);
                }
                if (frontCap)
                {
                    Point3D[] circleFront = this.Positions.Take(section.Count).ToArray();
                    Vector3D normal = path[0] - path[1];
                    normal.Normalize();

                    for (int i = 0; i < normals.Length; ++i)
                    {
                        normals[i] = normal;
                    }
                    this.AddTriangleFan(circleFront, normals, fanTextures);
                }
            }
        }
        /// <summary>
        /// Adds a tube with a custom section.
        /// </summary>
        /// <param name="path">A list of points defining the centers of the tube.</param>
        /// <param name="angles">The rotation of the section as it moves along the path</param>
        /// <param name="values">The texture coordinate X values (optional).</param>
        /// <param name="diameters">The diameters (optional).</param>
        /// <param name="section">The section to extrude along the tube path.</param>
        /// <param name="sectionXAxis">The initial alignment of the x-axis of the section into the
        /// 3D viewport</param>
        /// <param name="isTubeClosed">If the tube is closed set to <c>true</c> .</param>
        /// <param name="isSectionClosed">if set to <c>true</c> [is section closed].</param>
        /// <param name="frontCap">
        /// Create a front Cap or not.
        /// </param>
        /// <param name="backCap">
        /// Create a back Cap or not.
        /// </param>
        public void AddTube(
            IList<Point3D> path, IList<double> angles, IList<double> values, IList<double> diameters,
            IList<Point> section, Vector3D sectionXAxis, bool isTubeClosed, bool isSectionClosed, bool frontCap = false, bool backCap = false)
        {
            if (values != null && values.Count == 0)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            if (diameters != null && diameters.Count == 0)
            {
                throw new InvalidOperationException(WrongNumberOfDiameters);
            }

            if (angles != null && angles.Count == 0)
            {
                throw new InvalidOperationException(WrongNumberOfAngles);
            }

            int index0 = this.Positions.Count;
            int pathLength = path.Count;
            int sectionLength = section.Count;
            if (pathLength < 2 || sectionLength < 2)
            {
                return;
            }

            Vector3D forward = path[1] - path[0];
            Vector3D right = sectionXAxis;
            Vector3D up = SharedFunctions.CrossProduct(ref forward, ref right);
            up.Normalize();
            right.Normalize();

            int diametersCount = diameters != null ? diameters.Count : 0;
            int valuesCount = values != null ? values.Count : 0;
            int anglesCount = angles != null ? angles.Count : 0;

            for (int i = 0; i < pathLength; i++)
            {
                double radius = diameters != null ? (DoubleOrSingle)diameters[i % diametersCount] / 2 : 1;
                double theta = angles != null ? (DoubleOrSingle)angles[i % anglesCount] : 0.0;

                double ct = (DoubleOrSingle)Math.Cos(theta);
                double st = (DoubleOrSingle)Math.Sin(theta);

                int i0 = i > 0 ? i - 1 : i;
                int i1 = i + 1 < pathLength ? i + 1 : i;

                forward = path[i1] - path[i0];
                right = SharedFunctions.CrossProduct(ref up, ref forward);
                if (SharedFunctions.LengthSquared(ref right) > 1e-6f)
                {
                    up = SharedFunctions.CrossProduct(ref forward, ref right);
                }

                up.Normalize();
                right.Normalize();
                for (int j = 0; j < sectionLength; j++)
                {
                    double x = (section[j].X * ct) - (section[j].Y * st);
                    double y = (section[j].X * st) + (section[j].Y * ct);

                    Vector3D w = (x * right * radius) + (y * up * radius);
                    Point3D q = path[i] + w;
                    this.Positions.Add(q);
                    if (this.normals != null)
                    {
                        w.Normalize();
                        this.normals.Add(w);
                    }

                    this.TextureCoordinates?.Add(
                            values != null
                                ? new Point((DoubleOrSingle)values[i % valuesCount], (DoubleOrSingle)j / (sectionLength - 1))
                                : new Point());
                }
            }

            this.AddRectangularMeshTriangleIndices(index0, pathLength, sectionLength, isSectionClosed, isTubeClosed);
            if (frontCap || (backCap && path.Count > 1))
            {
                Vector3D[] normals = new Vector3D[section.Count];
                Point[] fanTextures = new Point[section.Count];
                int count = path.Count;
                if (backCap)
                {
                    Point3D[] circleBack = this.Positions.Skip(this.Positions.Count - section.Count).Take(section.Count).Reverse().ToArray();
                    Vector3D normal = path[count - 1] - path[count - 2];
                    normal.Normalize();
                    for (int i = 0; i < normals.Length; ++i)
                    {
                        normals[i] = normal;
                    }
                    this.AddTriangleFan(circleBack, normals, fanTextures);
                }
                if (frontCap)
                {
                    Point3D[] circleFront = this.Positions.Take(section.Count).ToArray();
                    Vector3D normal = path[0] - path[1];
                    normal.Normalize();

                    for (int i = 0; i < normals.Length; ++i)
                    {
                        normals[i] = normal;
                    }
                    this.AddTriangleFan(circleFront, normals, fanTextures);
                }
            }
        }
        #endregion Add Geometry

        #region Helper Functions
        /// <summary>
        /// Appends the specified mesh.
        /// </summary>
        /// <param name="mesh">
        /// The mesh.
        /// </param>
        public void Append(MeshBuilder mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            this.Append(mesh.Positions, mesh.TriangleIndices, mesh.normals, mesh.TextureCoordinates);
        }

#if !NETFX_CORE
        /// <summary>
        /// Appends the specified mesh.
        /// </summary>
        /// <param name="mesh">
        /// The mesh.
        /// </param>
        public void Append(MeshGeometry3D mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            this.Append(mesh.Positions, mesh.TriangleIndices, this.normals != null ? mesh.Normals : null, this.TextureCoordinates != null ? mesh.TextureCoordinates : null);
        }
#endif

        /// <summary>
        /// Appends the specified points and triangles.
        /// </summary>
        /// <param name="positionsToAppend">
        /// The points to append.
        /// </param>
        /// <param name="triangleIndicesToAppend">
        /// The triangle indices to append.
        /// </param>
        /// <param name="normalsToAppend">
        /// The normal vectors to append.
        /// </param>
        /// <param name="textureCoordinatesToAppend">
        /// The texture coordinates to append.
        /// </param>
        public void Append(
            IList<Point3D> positionsToAppend, IList<int> triangleIndicesToAppend,
            IList<Vector3D> normalsToAppend = null, IList<Point> textureCoordinatesToAppend = null)
        {
            if (positionsToAppend == null)
            {
                throw new ArgumentNullException(nameof(positionsToAppend));
            }

            if (this.normals != null && normalsToAppend == null)
            {
                throw new InvalidOperationException(SourceMeshNormalsShouldNotBeNull);
            }

            if (this.TextureCoordinates != null && textureCoordinatesToAppend == null)
            {
                throw new InvalidOperationException(SourceMeshTextureCoordinatesShouldNotBeNull);
            }

            if (normalsToAppend != null && normalsToAppend.Count != positionsToAppend.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (textureCoordinatesToAppend != null && textureCoordinatesToAppend.Count != positionsToAppend.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            int index0 = this.Positions.Count;
            foreach (Point3D p in positionsToAppend)
            {
                this.Positions.Add(p);
            }

            if (this.normals != null && normalsToAppend != null)
            {
                foreach (Vector3D n in normalsToAppend)
                {
                    this.normals.Add(n);
                }
            }

            if (this.TextureCoordinates != null && textureCoordinatesToAppend != null)
            {
                foreach (Point t in textureCoordinatesToAppend)
                {
                    this.TextureCoordinates.Add(t);
                }
            }

            foreach (int i in triangleIndicesToAppend)
            {
                this.TriangleIndices.Add(index0 + i);
            }
        }
        /// <summary>
        /// Chamfers the specified corner (experimental code).
        /// </summary>
        /// <param name="p">
        /// The corner point.
        /// </param>
        /// <param name="d">
        /// The chamfer distance.
        /// </param>
        /// <param name="eps">
        /// The corner search limit distance.
        /// </param>
        /// <param name="chamferPoints">
        /// If this parameter is provided, the collection will be filled with the generated chamfer points.
        /// </param>
        public void ChamferCorner(Point3D p, double d, double eps = 1e-6f, IList<Point3D> chamferPoints = null)
        {
            this.NoSharedVertices();

            this.normals = null;
            this.TextureCoordinates = null;

            Vector3D cornerNormal = this.FindCornerNormal(p, eps);

            Point3D newCornerPoint = p - (cornerNormal * (DoubleOrSingle)d);
            int index0 = this.Positions.Count;
            this.Positions.Add(newCornerPoint);

#if SHARPDX
            var plane = new Plane(newCornerPoint, cornerNormal);
#else
            Plane3D plane = new(newCornerPoint, cornerNormal);
#endif

            int ntri = this.TriangleIndices.Count;

            for (int i = 0; i < ntri; i += 3)
            {
                int i0 = i;
                int i1 = i + 1;
                int i2 = i + 2;
                Point3D p0 = this.Positions[this.TriangleIndices[i0]];
                Point3D p1 = this.Positions[this.TriangleIndices[i1]];
                Point3D p2 = this.Positions[this.TriangleIndices[i2]];
                Vector3D pp0 = p - p0;
                Vector3D pp1 = p - p1;
                Vector3D pp2 = p - p2;
                double d0 = SharedFunctions.LengthSquared(ref pp0);
                double d1 = SharedFunctions.LengthSquared(ref pp1);
                double d2 = SharedFunctions.LengthSquared(ref pp2);
                double mind = Math.Min(d0, Math.Min(d1, d2));
                if (mind > eps)
                {
                    continue;
                }

                if (d1 < eps)
                {
                    i0 = i + 1;
                    i1 = i + 2;
                    i2 = i;
                }

                if (d2 < eps)
                {
                    i0 = i + 2;
                    i1 = i;
                    i2 = i + 1;
                }

                p0 = this.Positions[this.TriangleIndices[i0]];
                p1 = this.Positions[this.TriangleIndices[i1]];
                p2 = this.Positions[this.TriangleIndices[i2]];

                // origin is the corner vertex (at index i0)
                // find the intersections between the chamfer plane and the two edges connected to the corner
                Point3D? p01 = plane.LineIntersection(p0, p1);
                Point3D? p02 = plane.LineIntersection(p0, p2);

                if (p01 == null)
                {
                    continue;
                }

                if (p02 == null)
                {
                    continue;
                }

                if (chamferPoints != null)
                {
                    // add the chamfered points
                    if (!chamferPoints.Contains(p01.Value))
                    {
                        chamferPoints.Add(p01.Value);
                    }

                    if (!chamferPoints.Contains(p02.Value))
                    {
                        chamferPoints.Add(p02.Value);
                    }
                }

                int i01 = i0;

                // change the original triangle to use the first chamfer point
                this.Positions[this.TriangleIndices[i01]] = p01.Value;

                int i02 = this.Positions.Count;
                this.Positions.Add(p02.Value);

                // add a new triangle for the other chamfer point
                this.TriangleIndices.Add(i01);
                this.TriangleIndices.Add(i2);
                this.TriangleIndices.Add(i02);

                // add a triangle connecting the chamfer points and the new corner point
                this.TriangleIndices.Add(index0);
                this.TriangleIndices.Add(i01);
                this.TriangleIndices.Add(i02);
            }

            this.NoSharedVertices();
        }

#if !NETFX_CORE
        /// <summary>
        /// Checks the performance limits.
        /// </summary>
        /// <remarks>
        /// See <a href="https://msdn.microsoft.com/en-us/library/bb613553(v=vs.100).aspx">MSDN</a>.
        /// Try to keep mesh sizes under these limits:
        /// Positions : 20,001 point instances
        /// TriangleIndices : 60,003 integer instances
        /// </remarks>
        public void CheckPerformanceLimits()
        {
            if (this.Positions.Count > 20000)
            {
                Trace.WriteLine(string.Format("Too many positions ({0}).", this.Positions.Count));
            }

            if (this.TriangleIndices.Count > 60002)
            {
                Trace.WriteLine(string.Format("Too many triangle indices ({0}).", this.TriangleIndices.Count));
            }
        }
#endif

        /// <summary>
        /// Finds the average normal to the specified corner (experimental code).
        /// </summary>
        /// <param name="p">
        /// The corner point.
        /// </param>
        /// <param name="eps">
        /// The corner search limit distance.
        /// </param>
        /// <returns>
        /// The normal.
        /// </returns>
        private Vector3D FindCornerNormal(Point3D p, double eps)
        {
            Vector3D sum = new();
            int count = 0;
            HashSet<Vector3D> addedNormals = new();
            for (int i = 0; i < this.TriangleIndices.Count; i += 3)
            {
                int i0 = i;
                int i1 = i + 1;
                int i2 = i + 2;
                Point3D p0 = this.Positions[this.TriangleIndices[i0]];
                Point3D p1 = this.Positions[this.TriangleIndices[i1]];
                Point3D p2 = this.Positions[this.TriangleIndices[i2]];

                // check if any of the vertices are on the corner
                Vector3D pp0 = p - p0;
                Vector3D pp1 = p - p1;
                Vector3D pp2 = p - p2;
                double d0 = SharedFunctions.LengthSquared(ref pp0);
                double d1 = SharedFunctions.LengthSquared(ref pp1);
                double d2 = SharedFunctions.LengthSquared(ref pp2);
                double mind = Math.Min(d0, Math.Min(d1, d2));
                if (mind > eps)
                {
                    continue;
                }

                // calculate the triangle normal and check if this face is already added
                Vector3D p10 = p1 - p0;
                Vector3D p20 = p2 - p0;
                Vector3D normal = SharedFunctions.CrossProduct(ref p10, ref p20);
                normal.Normalize();

                // todo: need to use the epsilon value to compare the normals?
                if (addedNormals.Contains(normal))
                {
                    continue;
                }

                // todo: this does not work yet
                // double dp = 1;
                // foreach (var n in addedNormals)
                // {
                // dp = Math.Abs(Vector3D.DotProduct(n, normal) - 1);
                // if (dp < eps)
                // continue;
                // }
                // if (dp < eps)
                // {
                // continue;
                // }
                count++;
                sum += normal;
                _ = addedNormals.Add(normal);
            }

            return count == 0 ? new Vector3D() : sum * (1f / count);
        }
        /// <summary>
        /// Makes sure no triangles share the same vertex.
        /// </summary>
        private void NoSharedVertices()
        {
            Point3DCollection p = new();
            Int32Collection ti = new();
            Vector3DCollection n = null;
            if (this.normals != null)
            {
                n = new Vector3DCollection();
            }

            PointCollection tc = null;
            if (this.TextureCoordinates != null)
            {
                tc = new PointCollection();
            }

            for (int i = 0; i < this.TriangleIndices.Count; i += 3)
            {
                int i0 = i;
                int i1 = i + 1;
                int i2 = i + 2;
                int index0 = this.TriangleIndices[i0];
                int index1 = this.TriangleIndices[i1];
                int index2 = this.TriangleIndices[i2];
                Point3D p0 = this.Positions[index0];
                Point3D p1 = this.Positions[index1];
                Point3D p2 = this.Positions[index2];
                p.Add(p0);
                p.Add(p1);
                p.Add(p2);
                ti.Add(i0);
                ti.Add(i1);
                ti.Add(i2);
                if (n != null)
                {
                    n.Add(this.normals[index0]);
                    n.Add(this.normals[index1]);
                    n.Add(this.normals[index2]);
                }

                if (tc != null)
                {
                    tc.Add(this.TextureCoordinates[index0]);
                    tc.Add(this.TextureCoordinates[index1]);
                    tc.Add(this.TextureCoordinates[index2]);
                }
            }

            this.Positions = p;
            this.TriangleIndices = ti;
            this.normals = n;
            this.TextureCoordinates = tc;
        }
        /// <summary>
        /// Scales the positions (and normal vectors).
        /// </summary>
        /// <param name="scaleX">
        /// The X scale factor.
        /// </param>
        /// <param name="scaleY">
        /// The Y scale factor.
        /// </param>
        /// <param name="scaleZ">
        /// The Z scale factor.
        /// </param>
        public void Scale(double scaleX, double scaleY, double scaleZ)
        {
            for (int i = 0; i < this.Positions.Count; i++)
            {
                this.Positions[i] = new Point3D(
                    this.Positions[i].X * (DoubleOrSingle)scaleX, this.Positions[i].Y * (DoubleOrSingle)scaleY, this.Positions[i].Z * (DoubleOrSingle)scaleZ);
            }

            if (this.Normals != null)
            {
                for (int i = 0; i < this.Normals.Count; i++)
                {
                    Vector3D v = new(
                        this.Normals[i].X * (DoubleOrSingle)scaleX, this.Normals[i].Y * (DoubleOrSingle)scaleY, this.Normals[i].Z * (DoubleOrSingle)scaleZ);
                    v.Normalize();
                    this.Normals[i] = v;
                }
            }
        }
        /// <summary>
        /// Subdivides each triangle into four sub-triangles.
        /// </summary>
        private void Subdivide4()
        {
            // Each triangle is divided into four subtriangles, adding new vertices in the middle of each edge.
            int ip = this.Positions.Count;
            int ntri = this.TriangleIndices.Count;
            for (int i = 0; i < ntri; i += 3)
            {
                int i0 = this.TriangleIndices[i];
                int i1 = this.TriangleIndices[i + 1];
                int i2 = this.TriangleIndices[i + 2];
                Point3D p0 = this.Positions[i0];
                Point3D p1 = this.Positions[i1];
                Point3D p2 = this.Positions[i2];
                Vector3D v01 = p1 - p0;
                Vector3D v12 = p2 - p1;
                Vector3D v20 = p0 - p2;
                Point3D p01 = p0 + (v01 * 0.5f);
                Point3D p12 = p1 + (v12 * 0.5f);
                Point3D p20 = p2 + (v20 * 0.5f);

                int i01 = ip++;
                int i12 = ip++;
                int i20 = ip++;

                this.Positions.Add(p01);
                this.Positions.Add(p12);
                this.Positions.Add(p20);

                if (this.normals != null)
                {
                    Vector3D n = this.Normals[i0];
                    this.Normals.Add(n);
                    this.Normals.Add(n);
                    this.Normals.Add(n);
                }

                if (this.TextureCoordinates != null)
                {
                    Point uv0 = this.TextureCoordinates[i0];
                    Point uv1 = this.TextureCoordinates[i0 + 1];
                    Point uv2 = this.TextureCoordinates[i0 + 2];
                    Vector t01 = uv1 - uv0;
                    Vector t12 = uv2 - uv1;
                    Vector t20 = uv0 - uv2;
                    Point u01 = uv0 + (t01 * 0.5f);
                    Point u12 = uv1 + (t12 * 0.5f);
                    Point u20 = uv2 + (t20 * 0.5f);
                    this.TextureCoordinates.Add(u01);
                    this.TextureCoordinates.Add(u12);
                    this.TextureCoordinates.Add(u20);
                }

                // TriangleIndices[i ] = i0;
                this.TriangleIndices[i + 1] = i01;
                this.TriangleIndices[i + 2] = i20;

                this.TriangleIndices.Add(i01);
                this.TriangleIndices.Add(i1);
                this.TriangleIndices.Add(i12);

                this.TriangleIndices.Add(i12);
                this.TriangleIndices.Add(i2);
                this.TriangleIndices.Add(i20);

                this.TriangleIndices.Add(i01);
                this.TriangleIndices.Add(i12);
                this.TriangleIndices.Add(i20);
            }
        }
        /// <summary>
        /// Subdivides each triangle into six triangles. Adds a vertex at the midpoint of each triangle.
        /// </summary>
        /// <remarks>
        /// See <a href="http://en.wikipedia.org/wiki/Barycentric_subdivision">wikipedia</a>.
        /// </remarks>
        private void SubdivideBarycentric()
        {
            // The BCS of a triangle S divides it into six triangles; each part has one vertex v2 at the
            // barycenter of S, another one v1 at the midpoint of some side, and the last one v0 at one
            // of the original vertices.
            int im = this.Positions.Count;
            int ntri = this.TriangleIndices.Count;
            for (int i = 0; i < ntri; i += 3)
            {
                int i0 = this.TriangleIndices[i];
                int i1 = this.TriangleIndices[i + 1];
                int i2 = this.TriangleIndices[i + 2];
                Point3D p0 = this.Positions[i0];
                Point3D p1 = this.Positions[i1];
                Point3D p2 = this.Positions[i2];
                Vector3D v01 = p1 - p0;
                Vector3D v12 = p2 - p1;
                Vector3D v20 = p0 - p2;
                Point3D p01 = p0 + (v01 * 0.5f);
                Point3D p12 = p1 + (v12 * 0.5f);
                Point3D p20 = p2 + (v20 * 0.5f);
                Point3D m = new((p0.X + p1.X + p2.X) / 3, (p0.Y + p1.Y + p2.Y) / 3, (p0.Z + p1.Z + p2.Z) / 3);

                int i01 = im + 1;
                int i12 = im + 2;
                int i20 = im + 3;

                this.Positions.Add(m);
                this.Positions.Add(p01);
                this.Positions.Add(p12);
                this.Positions.Add(p20);

                if (this.normals != null)
                {
                    Vector3D n = this.Normals[i0];
                    this.Normals.Add(n);
                    this.Normals.Add(n);
                    this.Normals.Add(n);
                    this.Normals.Add(n);
                }

                if (this.TextureCoordinates != null)
                {
                    Point uv0 = this.TextureCoordinates[i0];
                    Point uv1 = this.TextureCoordinates[i0 + 1];
                    Point uv2 = this.TextureCoordinates[i0 + 2];
                    Vector t01 = uv1 - uv0;
                    Vector t12 = uv2 - uv1;
                    Vector t20 = uv0 - uv2;
                    Point u01 = uv0 + (t01 * 0.5f);
                    Point u12 = uv1 + (t12 * 0.5f);
                    Point u20 = uv2 + (t20 * 0.5f);
                    Point uvm = new((uv0.X + uv1.X) * 0.5f, (uv0.Y + uv1.Y) * 0.5f);
                    this.TextureCoordinates.Add(uvm);
                    this.TextureCoordinates.Add(u01);
                    this.TextureCoordinates.Add(u12);
                    this.TextureCoordinates.Add(u20);
                }

                // TriangleIndices[i ] = i0;
                this.TriangleIndices[i + 1] = i01;
                this.TriangleIndices[i + 2] = im;

                this.TriangleIndices.Add(i01);
                this.TriangleIndices.Add(i1);
                this.TriangleIndices.Add(im);

                this.TriangleIndices.Add(i1);
                this.TriangleIndices.Add(i12);
                this.TriangleIndices.Add(im);

                this.TriangleIndices.Add(i12);
                this.TriangleIndices.Add(i2);
                this.TriangleIndices.Add(im);

                this.TriangleIndices.Add(i2);
                this.TriangleIndices.Add(i20);
                this.TriangleIndices.Add(im);

                this.TriangleIndices.Add(i20);
                this.TriangleIndices.Add(i0);
                this.TriangleIndices.Add(im);

                im += 4;
            }
        }
        /// <summary>
        /// Performs a linear subdivision of the mesh.
        /// </summary>
        /// <param name="barycentric">
        /// Add a vertex in the center if set to <c>true</c> .
        /// </param>
        public void SubdivideLinear(bool barycentric = false)
        {
            if (barycentric)
            {
                this.SubdivideBarycentric();
            }
            else
            {
                this.Subdivide4();
            }
        }
        #endregion Helper Functions

        #region Exporter Functions
#if SHARPDX
        /// <summary>
        /// Generate a MeshGeometry3D from the generated Data.
        /// </summary>
        /// <returns>The MeshGeometry3D.</returns>
        public MeshGeometry3D ToMesh()
        {
            return ToMeshGeometry3D();
        }
        /// <summary>
        /// Converts the geometry to a <see cref="MeshGeometry3D"/> .        
        /// </summary>
        public MeshGeometry3D ToMeshGeometry3D()
        {
            if (this.HasTangents && this.tangents.Count == 0)
            {
                Vector3DCollection tan, bitan;
                ComputeTangents(this.positions, this.normals, this.textureCoordinates, this.triangleIndices, out tan, out bitan);
                this.tangents.AddRange(tan);
                this.bitangents.AddRange(bitan);
            }

            return new MeshGeometry3D()
            {
                Positions = this.positions,
                Indices = this.triangleIndices,
                Normals = (this.HasNormals) ? this.normals : null,
                TextureCoordinates = (this.HasTexCoords) ? this.textureCoordinates : null,
                Tangents = (this.HasTangents) ? this.tangents : null,
                BiTangents = (this.HasTangents) ? this.bitangents : null,
            };
        }
#if Sphere
        private static Vector3 GetPosition(double theta, double phi, double radius)
        {
            double x = radius * Math.Sin(theta) * Math.Sin(phi);
            double y = radius * Math.Cos(phi);
            double z = radius * Math.Cos(theta) * Math.Sin(phi);
            return new Vector3((float)x, (float)y, (float)z);
        }

        private static Vector3 GetNormal(double theta, double phi)
        {
            return (Vector3)GetPosition(theta, phi, 1.0);
        }

        private static double DegToRad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        private static Vector2 GetTextureCoordinate(double theta, double phi)
        {
            return new Vector2((float)(theta / (2 * Math.PI)), (float)(phi / (Math.PI)));
        }

        /// <summary>
        /// Tesselates the element and returns a MeshGeometry3D representing the 
        /// tessellation based on the parameters given 
        /// </summary>        
        public void AppendSphere(Vector3 center, double radius = 1, int thetaSteps = 64, int phiSteps = 64)
        {
            Vector3Collection pos, nor;
            Vector2Collection tcoord;
            IntCollection tind;

            AppendSphere(center, radius, thetaSteps, phiSteps, out pos, out nor, out tcoord, out tind);

            int i0 = positions.Count;
            this.positions.AddRange(pos);
            this.normals.AddRange(nor);
            this.textureCoordinates.AddRange(tcoord);
            this.triangleIndices.AddRange(tind.Select(x => x + i0));
        }

        private static void AppendSphere(Vector3 center, double radius, int thetaSteps, int phiSteps,
            out Vector3Collection positions, out Vector3Collection normals, out Vector2Collection textureCoordinates, out IntCollection triangleIndices)
        {
            positions = new Vector3Collection();
            normals = new Vector3Collection();
            textureCoordinates = new Vector2Collection();
            triangleIndices = new IntCollection();

            double dt = DegToRad(360.0) / thetaSteps;
            double dp = DegToRad(180.0) / phiSteps;

            for (int pi = 0; pi <= phiSteps; pi++)
            {
                double phi = pi * dp;
                for (int ti = 0; ti <= thetaSteps; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    positions.Add(GetPosition(theta, phi, radius) + center);
                    normals.Add(GetNormal(theta, phi));
                    textureCoordinates.Add(GetTextureCoordinate(theta, phi));
                }
            }

            for (int pi = 0; pi < phiSteps; pi++)
            {
                for (int ti = 0; ti < thetaSteps; ti++)
                {
                    int x0 = ti;
                    int x1 = ti + 1;
                    int y0 = pi * (thetaSteps + 1);
                    int y1 = (pi + 1) * (thetaSteps + 1);

                    triangleIndices.Add(x0 + y0);
                    triangleIndices.Add(x0 + y1);
                    triangleIndices.Add(x1 + y0);

                    triangleIndices.Add(x1 + y0);
                    triangleIndices.Add(x0 + y1);
                    triangleIndices.Add(x1 + y1);
                }
            }
        }

#endif
#else
        /// <summary>
        /// Converts the geometry to a <see cref="MeshGeometry3D"/> .
        /// </summary>
        /// <param name="freeze">
        /// freeze the mesh if set to <c>true</c> .
        /// </param>
        /// <returns>
        /// A mesh geometry.
        /// </returns>
        public MeshGeometry3D ToMesh(bool freeze = false)
        {
            if (this.TriangleIndices.Count == 0)
            {
                MeshGeometry3D emptyGeometry = new();
                if (freeze)
                {
                    emptyGeometry.Freeze();
                }

                return emptyGeometry;
            }

            if (this.normals != null && this.Positions.Count != this.normals.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (this.TextureCoordinates != null && this.Positions.Count != this.TextureCoordinates.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            MeshGeometry3D mg = new()
            {
                Positions = new Point3DCollection(this.Positions),
                TriangleIndices = new Int32Collection(this.TriangleIndices)
            };
            if (this.normals != null)
            {
                mg.Normals = new Vector3DCollection(this.normals);
            }

            if (this.TextureCoordinates != null)
            {
                mg.TextureCoordinates = new PointCollection(this.TextureCoordinates);
            }

            if (freeze)
            {
                mg.Freeze();
            }

            return mg;
        }
#endif
        #endregion Exporter Functions
    }
}
