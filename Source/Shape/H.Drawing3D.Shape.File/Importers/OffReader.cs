﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OffReader.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides an Object File Format (OFF) reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Geometry;
using H.Drawing3D.Shape.Geometry.Geometry;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace H.Drawing3D.Shape.File.Importers
{
    /// <summary>
    /// Provides an Object File Format (OFF) reader.
    /// </summary>
    /// <remarks>
    /// The reader does not parse colors, normal vectors and texture coordinates.
    /// Only 3 dimensional vertices are supported.
    /// Homogeneous coordinates are not supported.
    /// </remarks>
    public class OffReader : ModelReader
    {
        private static readonly char[] Delimiters = new char[] { ' ', '\t' };
        /// <summary>
        /// Initializes a new instance of the <see cref="OffReader" /> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public OffReader(Dispatcher dispatcher = null)
            : base(dispatcher)
        {
            //// http://www.geomview.org/
            //// http://people.sc.fsu.edu/~jburkardt/data/off/off.html
            //// http://people.sc.fsu.edu/~jburkardt/html/off_format.html
            //// http://segeval.cs.princeton.edu/public/off_format.html
            //// http://paulbourke.net/dataformats/off/

            this.Vertices = new List<Point3D>();

            // this.VertexColors = new List<Color>();
            // this.TexCoords = new PointCollection();
            // this.Normals = new Vector3DCollection();
            this.Faces = new List<int[]>();
        }

        /// <summary>
        /// Gets the faces.
        /// </summary>
        public IList<int[]> Faces { get; private set; }

        /// <summary>
        /// Gets the vertices.
        /// </summary>
        public IList<Point3D> Vertices { get; private set; }

        /// <summary>
        /// Creates a mesh from the loaded file.
        /// </summary>
        /// <returns>
        /// A <see cref="Mesh3D" />.
        /// </returns>
        public Mesh3D CreateMesh()
        {
            Mesh3D mesh = new();
            foreach (Point3D v in this.Vertices)
            {
                mesh.Vertices.Add(v);
            }

            foreach (int[] face in this.Faces)
            {
                mesh.Faces.Add((int[])face.Clone());
            }

            return mesh;
        }

        /// <summary>
        /// Creates a <see cref="MeshGeometry3D" /> object from the loaded file. Polygons are triangulated using triangle fans.
        /// </summary>
        /// <returns>
        /// A <see cref="MeshGeometry3D" />.
        /// </returns>
        public MeshGeometry3D CreateMeshGeometry3D()
        {
            MeshBuilder mb = new(false, false);
            foreach (Point3D p in this.Vertices)
            {
                mb.Positions.Add(p);
            }

            foreach (int[] face in this.Faces)
            {
                mb.AddTriangleFan(face);
            }

            return mb.ToMesh();
        }

        /// <summary>
        /// Creates a <see cref="Model3DGroup" /> from the loaded file.
        /// </summary>
        /// <returns>A <see cref="Model3DGroup" />.</returns>
        public Model3DGroup CreateModel3D()
        {
            Model3DGroup modelGroup = null;
            this.Dispatch(
                () =>
                {
                    modelGroup = new Model3DGroup();
                    MeshGeometry3D g = this.CreateMeshGeometry3D();
                    GeometryModel3D gm = new() { Geometry = g, Material = this.DefaultMaterial };
                    gm.BackMaterial = gm.Material;
                    if (this.Freeze)
                    {
                        gm.Freeze();
                    }

                    modelGroup.Children.Add(gm);
                    if (this.Freeze)
                    {
                        modelGroup.Freeze();
                    }
                });

            return modelGroup;
        }

        /// <summary>
        /// Loads the model from the specified stream.
        /// </summary>
        /// <param name="s">
        /// The stream.
        /// </param>
        public void Load(Stream s)
        {
            using StreamReader reader = new(s);
            bool containsNormals = false;
            bool containsTextureCoordinates = false;
            bool containsColors = false;
            bool containsHomogeneousCoordinates = false;
            int vertexDimension = 3;
            bool nextLineContainsVertexDimension = false;
            bool nextLineContainsNumberOfVertices = false;
            int numberOfVertices = 0;
            int numberOfFaces = 0;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                line = line.Trim();
                if (line.StartsWith("#") || line.Length == 0)
                {
                    continue;
                }

                if (nextLineContainsVertexDimension)
                {
                    int[] values = GetIntValues(line);
                    vertexDimension = values[0];
                    nextLineContainsVertexDimension = false;
                    continue;
                }

                if (line.Contains("OFF"))
                {
                    containsNormals = line.Contains("N");
                    containsColors = line.Contains("C");
                    containsTextureCoordinates = line.Contains("ST");
                    if (line.Contains("4"))
                    {
                        containsHomogeneousCoordinates = true;
                    }

                    if (line.Contains("n"))
                    {
                        nextLineContainsVertexDimension = true;
                    }

                    nextLineContainsNumberOfVertices = true;
                    continue;
                }

                if (nextLineContainsNumberOfVertices)
                {
                    int[] values = GetIntValues(line);
                    numberOfVertices = values[0];
                    numberOfFaces = values[1];

                    /* numberOfEdges = values[2]; */
                    nextLineContainsNumberOfVertices = false;
                    continue;
                }

                if (this.Vertices.Count < numberOfVertices)
                {
                    double[] x = new double[vertexDimension];
                    double[] values = GetValues(line);
                    int i = 0;
                    for (int j = 0; j < vertexDimension; j++)
                    {
                        x[j] = values[i++];
                    }

                    double[] n = new double[vertexDimension];
                    double[] uv = new double[2];
                    if (containsHomogeneousCoordinates)
                    {
                        // ReSharper disable once RedundantAssignment
                        _ = values[i++];
                    }

                    if (containsNormals)
                    {
                        for (int j = 0; j < vertexDimension; j++)
                        {
                            n[j] = values[i++];
                        }
                    }

                    if (containsColors)
                    {
                        // read color
                    }

                    if (containsTextureCoordinates)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            uv[j] = values[i++];
                        }
                    }

                    this.Vertices.Add(new Point3D(x[0], x[1], x[2]));

                    continue;
                }

                if (this.Faces.Count < numberOfFaces)
                {
                    int[] values = GetIntValues(line);
                    int nv = values[0];
                    int[] vertices = new int[nv];
                    for (int i = 0; i < nv; i++)
                    {
                        vertices[i] = values[i + 1];
                    }

                    if (containsColors)
                    {
                        // read colorspec
                    }

                    this.Faces.Add(vertices);
                }
            }
        }

        /// <summary>
        /// Reads the model from the specified stream.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <returns>A <see cref="Model3DGroup" />.</returns>
        public override Model3DGroup Read(Stream s)
        {
            this.Load(s);
            return this.CreateModel3D();
        }

        /// <summary>
        /// Parses integer values from a string.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <returns>
        /// Array of integer values.
        /// </returns>
        private static int[] GetIntValues(string input)
        {
            string[] fields = RemoveComments(input).Split(Delimiters, System.StringSplitOptions.RemoveEmptyEntries);
            int[] result = new int[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                result[i] = (int)double.Parse(fields[i], CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Parses double values from a string.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <returns>
        /// Array of double values.
        /// </returns>
        private static double[] GetValues(string input)
        {
            string[] fields = RemoveComments(input).Split(Delimiters, System.StringSplitOptions.RemoveEmptyEntries);
            double[] result = new double[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                result[i] = double.Parse(fields[i], CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Removes comments from the line.
        /// </summary>
        /// <param name="input">
        /// The line.
        /// </param>
        /// <returns>
        /// A line without comments.
        /// </returns>
        private static string RemoveComments(string input)
        {
            int commentIndex = input.IndexOf('#');
            return commentIndex >= 0 ? input[..commentIndex] : input;
        }
    }
}