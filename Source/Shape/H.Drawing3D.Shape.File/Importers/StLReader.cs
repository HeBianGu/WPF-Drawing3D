// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StLReader.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides an importer for StereoLithography .StL files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing3D.Shape.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace H.Drawing3D.Shape.File.Importers
{
    /// <summary>
    /// Provides an importer for StereoLithography .StL files.
    /// </summary>
    /// <remarks>
    /// The format is documented on <a href="http://en.wikipedia.org/wiki/STL_(file_format)">Wikipedia</a>.
    /// </remarks>
    public class StLReader : ModelReader
    {
        /// <summary>
        /// The regular expression used to parse normal vectors.
        /// </summary>
        private static readonly Regex NormalRegex = new(@"normal\s*(\S*)\s*(\S*)\s*(\S*)", RegexOptions.Compiled);

        /// <summary>
        /// The regular expression used to parse vertices.
        /// </summary>
        private static readonly Regex VertexRegex = new(@"vertex\s*(\S*)\s*(\S*)\s*(\S*)", RegexOptions.Compiled);

        /// <summary>
        /// The index.
        /// </summary>
        private int index;

        /// <summary>
        /// The last color.
        /// </summary>
        private Color lastColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StLReader" /> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public StLReader(Dispatcher dispatcher = null)
            : base(dispatcher)
        {
            this.Meshes = new List<MeshBuilder>();
            this.Materials = new List<Material>();
        }

        /// <summary>
        /// Gets the file header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public string Header { get; private set; }

        /// <summary>
        /// Gets the materials.
        /// </summary>
        /// <value> The materials. </value>
        public IList<Material> Materials { get; private set; }

        /// <summary>
        /// Gets the meshes.
        /// </summary>
        /// <value> The meshes. </value>
        public IList<MeshBuilder> Meshes { get; private set; }

        /// <summary>
        /// Reads the model from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The model.</returns>
        public override Model3DGroup Read(Stream stream)
        {
            // Try to read in BINARY format
            bool success = this.TryReadBinary(stream);
            if (!success)
            {
                // Reset position of stream
                stream.Position = 0;

                // Read in ASCII format
                success = this.TryReadAscii(stream);
            }

            return success ? this.ToModel3D() : null;
        }

        /// <summary>
        /// Builds the model.
        /// </summary>
        /// <returns>The model.</returns>
        public Model3DGroup ToModel3D()
        {
            Model3DGroup modelGroup = null;
            this.Dispatch(
                () =>
                {
                    modelGroup = new Model3DGroup();
                    int i = 0;
                    foreach (MeshBuilder mesh in this.Meshes)
                    {
                        GeometryModel3D gm = new()
                        {
                            Geometry = mesh.ToMesh(),
                            Material = this.Materials[i],
                            BackMaterial = this.Materials[i]
                        };
                        if (this.Freeze)
                        {
                            gm.Freeze();
                        }

                        modelGroup.Children.Add(gm);
                        i++;
                    }

                    if (this.Freeze)
                    {
                        modelGroup.Freeze();
                    }
                });
            return modelGroup;
        }

        /// <summary>
        /// Parses the ID and values from the specified line.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        private static void ParseLine(string line, out string id, out string values)
        {
            line = line.Trim();
            int idx = line.IndexOf(' ');
            if (idx == -1)
            {
                id = line.ToLowerInvariant();
                values = string.Empty;
            }
            else
            {
                id = line[..idx].ToLowerInvariant();
                values = line[(idx + 1)..];
            }
        }

        /// <summary>
        /// Parses a normal string.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <returns>
        /// The normal vector.
        /// </returns>
        private static Vector3D ParseNormal(string input)
        {
            input = input.ToLowerInvariant();
            input = input.Replace("nan", "NaN");
            Match match = NormalRegex.Match(input);
            if (!match.Success)
            {
                throw new FileFormatException("Unexpected line.");
            }

            double x = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            double y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            double z = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            return new Vector3D(x, y, z);
        }

        /// <summary>
        /// Reads a float (4 byte)
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <returns>
        /// The float.
        /// </returns>
        private static float ReadFloat(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Reads a line from the stream reader.
        /// </summary>
        /// <param name="reader">
        /// The stream reader.
        /// </param>
        /// <param name="token">
        /// The expected token ID.
        /// </param>
        /// <exception cref="FileFormatException">
        /// The expected token ID was not matched.
        /// </exception>
        private static void ReadLine(StreamReader reader, string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            string line = reader.ReadLine();
            ParseLine(line, out string id, out _);

            if (!string.Equals(token, id, StringComparison.OrdinalIgnoreCase))
            {
                throw new FileFormatException("Unexpected line.");
            }
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <returns>
        /// The unsigned integer.
        /// </returns>
        private static ushort ReadUInt16(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <returns>
        /// The unsigned integer.
        /// </returns>
        private static uint ReadUInt32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Tries to parse a vertex from a string.
        /// </summary>
        /// <param name="line">
        /// The input string.
        /// </param>
        /// <param name="point">
        /// The vertex point.
        /// </param>
        /// <returns>
        /// True if parsing was successful.
        /// </returns>
        private static bool TryParseVertex(string line, out Point3D point)
        {
            line = line.ToLowerInvariant();
            Match match = VertexRegex.Match(line);
            if (!match.Success)
            {
                point = new Point3D();
                return false;
            }

            double x = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            double y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            double z = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            point = new Point3D(x, y, z);
            return true;
        }

        /// <summary>
        /// Reads a facet.
        /// </summary>
        /// <param name="reader">
        /// The stream reader.
        /// </param>
        /// <param name="normal">
        /// The normal. 
        /// </param>
        private void ReadFacet(StreamReader reader, string normal)
        {
            _ = ParseNormal(normal);
            List<Point3D> points = new();
            ReadLine(reader, "outer");
            while (true)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                line = line.Trim();

                if (TryParseVertex(line, out Point3D point))
                {
                    points.Add(point);
                    continue;
                }

                ParseLine(line, out string id, out string _);

                if (id == "endloop")
                {
                    break;
                }
            }

            ReadLine(reader, "endfacet");

            if (this.Materials.Count < this.index + 1)
            {
                this.Materials.Add(this.DefaultMaterial);
            }

            if (this.Meshes.Count < this.index + 1)
            {
                this.Meshes.Add(new MeshBuilder(true, true));
            }

            this.Meshes[this.index].AddPolygon(points);

            // todo: add normals
        }

        /// <summary>
        /// Reads a triangle from a binary STL file.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        private void ReadTriangle(BinaryReader reader)
        {
            float ni = ReadFloat(reader);
            float nj = ReadFloat(reader);
            float nk = ReadFloat(reader);
            _ = new Vector3D(ni, nj, nk);
            float x1 = ReadFloat(reader);
            float y1 = ReadFloat(reader);
            float z1 = ReadFloat(reader);
            Point3D v1 = new(x1, y1, z1);

            float x2 = ReadFloat(reader);
            float y2 = ReadFloat(reader);
            float z2 = ReadFloat(reader);
            Point3D v2 = new(x2, y2, z2);

            float x3 = ReadFloat(reader);
            float y3 = ReadFloat(reader);
            float z3 = ReadFloat(reader);
            Point3D v3 = new(x3, y3, z3);

            char[] attrib = Convert.ToString(ReadUInt16(reader), 2).PadLeft(16, '0').ToCharArray();
            bool hasColor = attrib[0].Equals('1');

            if (hasColor)
            {
                int blue = attrib[15].Equals('1') ? 1 : 0;
                blue = attrib[14].Equals('1') ? blue + 2 : blue;
                blue = attrib[13].Equals('1') ? blue + 4 : blue;
                blue = attrib[12].Equals('1') ? blue + 8 : blue;
                blue = attrib[11].Equals('1') ? blue + 16 : blue;
                int b = blue * 8;

                int green = attrib[10].Equals('1') ? 1 : 0;
                green = attrib[9].Equals('1') ? green + 2 : green;
                green = attrib[8].Equals('1') ? green + 4 : green;
                green = attrib[7].Equals('1') ? green + 8 : green;
                green = attrib[6].Equals('1') ? green + 16 : green;
                int g = green * 8;

                int red = attrib[5].Equals('1') ? 1 : 0;
                red = attrib[4].Equals('1') ? red + 2 : red;
                red = attrib[3].Equals('1') ? red + 4 : red;
                red = attrib[2].Equals('1') ? red + 8 : red;
                red = attrib[1].Equals('1') ? red + 16 : red;
                int r = red * 8;

                Color currentColor = Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));

                if (!Color.Equals(this.lastColor, currentColor))
                {
                    this.lastColor = currentColor;
                    this.index++;
                }

                if (this.Materials.Count < this.index + 1)
                {
                    this.Materials.Add(new DiffuseMaterial(new SolidColorBrush(currentColor)));
                }
            }
            else
            {
                if (this.Materials.Count < this.index + 1)
                {
                    this.Materials.Add(this.DefaultMaterial);
                }
            }

            if (this.Meshes.Count < this.index + 1)
            {
                this.Meshes.Add(new MeshBuilder(true, true));
            }

            this.Meshes[this.index].AddTriangle(v1, v2, v3);

            // todo: add normal
        }

        /// <summary>
        /// Reads the model in ASCII format from the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// True if the model was loaded successfully.
        /// </returns>
        private bool TryReadAscii(Stream stream)
        {
            StreamReader reader = new(stream);
            this.Meshes.Add(new MeshBuilder(true, true));
            this.Materials.Add(this.DefaultMaterial);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    continue;
                }

                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("\0") || line.StartsWith("#") || line.StartsWith("!")
                    || line.StartsWith("$"))
                {
                    continue;
                }

                ParseLine(line, out string id, out string values);
                switch (id)
                {
                    case "solid":
                        this.Header = values.Trim();
                        break;
                    case "facet":
                        this.ReadFacet(reader, values);
                        break;
                    case "endsolid":
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads the model from the specified binary stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// True if the file was read successfully.
        /// </returns>
        /// <exception cref="FileFormatException">
        /// Incomplete file
        /// </exception>
        private bool TryReadBinary(Stream stream)
        {
            long length = stream.Length;
            if (length < 84)
            {
                throw new FileFormatException("Incomplete file");
            }

            BinaryReader reader = new(stream);
            this.Header = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(80)).Trim();
            uint numberTriangles = ReadUInt32(reader);

            if (length - 84 != numberTriangles * 50)
            {
                return false;
            }

            this.index = 0;
            this.Meshes.Add(new MeshBuilder(true, true));
            this.Materials.Add(this.DefaultMaterial);

            for (int i = 0; i < numberTriangles; i++)
            {
                this.ReadTriangle(reader);
            }

            return true;
        }
    }
}