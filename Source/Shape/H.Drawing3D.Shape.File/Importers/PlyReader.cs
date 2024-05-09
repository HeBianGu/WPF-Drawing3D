using H.Drawing3D.Shape.Geometry.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace H.Drawing3D.Shape.File.Importers
{
    /// <summary>
    /// Polygon File Format Reader.
    /// </summary>
    /// <remarks>
    /// https://www.cc.gatech.edu/projects/large_models/ply.html
    /// http://graphics.stanford.edu/data/3Dscanrep/
    /// </remarks>
    public class PlyReader : ModelReader
    {
        /// <summary>
        /// Initializes a new <see cref="PlyReader"/>.
        /// </summary>
        /// <param name="dispatcher"></param>
        public PlyReader(Dispatcher dispatcher = null) : base(dispatcher)
        {
            this.Header = new PlyHeader();
            this.Body = new List<PlyElement>();
        }

        #region Public methods
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
        /// Reads the model from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The model.</returns>
        public override Model3DGroup Read(string path)
        {
            this.Directory = Path.GetDirectoryName(path);
            this.Load(path);
            using FileStream s = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return this.Read(s);
        }

        /// <summary>
        /// Creates a mesh from the loaded file.
        /// </summary>
        /// <returns>
        /// A <see cref="Mesh3D" />.
        /// </returns>
        public Mesh3D CreateMesh()
        {
            Mesh3D mesh = new();

            PlyElement vertexElement = this.Body.Find(item => item.Name == "vertex");
            if (vertexElement != null && vertexElement.Count > 0)
            {
                foreach (PlyProperty[] vertProp in vertexElement.Instances)
                {
                    List<PlyProperty> vertPropList = vertProp.ToList();
                    PlyProperty xProp = vertPropList.Find(item => !item.IsList && item.Name == "x");
                    PlyProperty yProp = vertPropList.Find(item => !item.IsList && item.Name == "y");
                    PlyProperty zProp = vertPropList.Find(item => !item.IsList && item.Name == "z");

                    if (xProp != null && yProp != null && zProp != null)
                    {
                        double xCoord = double.Parse(xProp.Value?.ToString() ?? "0");
                        double yCoord = double.Parse(yProp.Value?.ToString() ?? "0");
                        double zCoord = double.Parse(zProp.Value?.ToString() ?? "0");
                        Point3D vertex = new(xCoord, yCoord, zCoord);
                        mesh.Vertices.Add(vertex);
                    }

                    //var sProp = vertPropList.Find(item => !item.IsList && item.Name == "s");
                    //var tProp = vertPropList.Find(item => !item.IsList && item.Name == "t");

                    //if (sProp != null && tProp != null)
                    //{
                    //    var sCoord = double.Parse(sProp.Value.ToString());
                    //    var tCoord = double.Parse(tProp.Value.ToString());
                    //    var texturePt = new Point(sCoord, tCoord);
                    //    mesh.TextureCoordinates.Add(texturePt);
                    //}
                }
            }

            PlyElement faceElement = this.Body.Find(item => item.Name == "face");
            if (faceElement != null && faceElement.Count > 0)
            {
                foreach (PlyProperty[] faceProp in faceElement.Instances)
                {
                    PlyProperty[] vertexIndicesProperties = (from item in faceProp where (item.IsList && item.Name == "vertex_indices") || item.Name == "vertex_index" select item).ToArray();
                    if (vertexIndicesProperties.Length > 0)
                    {
                        List<int> vertexIndices = new();
                        foreach (object item in vertexIndicesProperties[0].ListContentValues)
                        {
                            vertexIndices.Add(Convert.ToInt32(item ?? "0"));
                        }
                        mesh.Faces.Add(vertexIndices.ToArray());
                    }

                }
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
            Mesh3D mesh = this.CreateMesh();
            return mesh.ToMeshGeometry3D();
        }

        /// <summary>
        /// Creates a <see cref="Model3DGroup" /> from the loaded file.
        /// </summary>
        /// <returns>A <see cref="Model3DGroup" />.</returns>
        public Model3DGroup CreateModel3D()
        {
            Model3DGroup modelGroup = null;
            this.Dispatch(() =>
            {
                modelGroup = new Model3DGroup();
                MeshGeometry3D g = this.CreateMeshGeometry3D();
                GeometryModel3D gm = new()
                {
                    Geometry = g,
                    Material = this.DefaultMaterial,
                    BackMaterial = this.DefaultMaterial
                };
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
        /// Loads a ply file from the <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The stream containing the ply file.</param>
        public void Load(Stream s)
        {
            this.Header = new PlyHeader();
            this.Body = new List<PlyElement>();
            Tuple<PlyHeader, List<PlyElement>> plyFileData = this.LoadPlyFile(s);

            this.Header = plyFileData.Item1;
            this.Body = plyFileData.Item2;
            //var location = "";
            //DumpFileAsASCII(location, plyHeader, plyBody);
        }

        /// <summary>
        /// Loads a plyfile from the specified filepath.
        /// </summary>
        /// <param name="path">The filepath.</param>
        public void Load(string path)
        {
            this.Directory = Path.GetDirectoryName(path);
            using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
            this.Load(fs);
        }

        /// <summary>
        /// Loads a ply file from the stream but doesn't consume it.
        /// </summary>
        /// <param name="plyFileStream"></param>
        /// <returns></returns>
        /// <remarks>
        /// This could be useful when we have several streams of plyfiles to reconstruct
        /// into a single mesh, without updating the Header and Body properties of this reader.
        /// </remarks>
        public Tuple<PlyHeader, List<PlyElement>> LoadPlyFile(Stream plyFileStream)
        {
            List<string> headerLines = new();
            long startPosition = 0;
            StreamReader textReader = new(plyFileStream);
            plyFileStream.Position = 0;
            _ = plyFileStream.Seek(0, SeekOrigin.Begin);

            bool readingHeader = true;
            List<StringBuilder> headerLineBuilders = new() { new StringBuilder() };
            bool newLineCharMet = false;

            while (readingHeader && textReader.EndOfStream == false)
            {
                char currentChar = (char)textReader.Read();

                if (currentChar is '\r' or '\n')
                {
                    if (newLineCharMet)
                    {
                        startPosition++;
                    }
                    else
                    {
                        newLineCharMet = true;
                        headerLineBuilders.Add(new StringBuilder());
                        startPosition++;
                    }
                }
                else
                {
                    if (headerLineBuilders.Count > 2 && headerLineBuilders[^2].ToString() == "end_header")
                    {
                        headerLineBuilders.RemoveAt(headerLineBuilders.Count - 1);
                        readingHeader = false;
                    }
                    else
                    {
                        newLineCharMet = false;
                        _ = headerLineBuilders.Last().Append(currentChar.ToString());
                        startPosition++;
                    }
                }
            }

            foreach (StringBuilder headerLineBuilder in headerLineBuilders)
            {
                headerLines.Add(headerLineBuilder.ToString());
            }

            PlyHeader plyHeader = this.ReadHeader(headerLines.ToArray());
            List<PlyElement> plyBody = new();
            plyFileStream.Position = startPosition;

            switch (plyHeader.FormatType)
            {
                case PlyFormatTypes.ascii:
                    plyBody = this.ReadASCII(plyFileStream, plyHeader);
                    break;
                case PlyFormatTypes.binary_big_endian:
                    plyBody = this.ReadBinary(plyFileStream, plyHeader, true);
                    break;
                case PlyFormatTypes.binary_little_endian:
                    plyBody = this.ReadBinary(plyFileStream, plyHeader, false);
                    break;
                default:
                    break;
            }

            textReader.Dispose();
            textReader.Close();
            return new Tuple<PlyHeader, List<PlyElement>>(plyHeader, plyBody);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the header of the loaded ply file.
        /// </summary>
        public PlyHeader Header { get; private set; }

        /// <summary>
        /// Gets or sets the body of the loaded ply file.
        /// </summary>
        public List<PlyElement> Body { get; private set; }
        #endregion

        #region Data
        /// <summary>
        /// The supported version of the ply format.
        /// </summary>
        public static readonly Version SUPPORTEDVERSION = new(1, 0, 0);

        /// <summary>
        /// Specifies the types of ply model formats.
        /// </summary>
        public enum PlyFormatTypes
        {
            /// <summary>
            /// ASCII ply format.
            /// </summary>
            ascii,
            /// <summary>
            /// Binary big endian ply format.
            /// </summary>
            binary_big_endian,
            /// <summary>
            /// Binary little endian ply format.
            /// </summary>
            binary_little_endian
        }

        /// <summary>
        /// Specifies the types of ply data types.
        /// </summary>
        public enum PlyDataTypes
        {
            /// <summary>
            /// character
            /// </summary>
            _char,
            /// <summary>
            /// unsigned character
            /// </summary>
            _uchar,
            /// <summary>
            /// short integer
            /// </summary>
            _short,
            /// <summary>
            /// unsigned short integer
            /// </summary>
            _ushort,
            /// <summary>
            /// integer
            /// </summary>
            _int,
            /// <summary>
            /// integer
            /// </summary>
            _int32,
            /// <summary>
            /// unsigned integer
            /// </summary>
            _uint,
            /// <summary>
            /// unsigned integer
            /// </summary>
            _uint8,
            /// <summary>
            /// single-precision float
            /// </summary>
            _float,
            /// <summary>
            /// single-precision float
            /// </summary>
            _float32,
            /// <summary>
            /// double-precision float
            /// </summary>
            _double,
        }

        /// <summary>
        /// Specifies the types of items in a ply header.
        /// </summary>
        public enum PlyHeaderItems
        {
            /// <summary>
            /// The beginning of a ply file.
            /// </summary>
            ply,
            /// <summary>
            /// The format of a ply file.
            /// </summary>
            format,
            /// <summary>
            /// A comment in a ply file.
            /// </summary>
            comment,
            /// <summary>
            /// An object info in a ply header
            /// </summary>
            obj_info,
            /// <summary>
            /// The declaration of an element.
            /// </summary>
            element,
            /// <summary>
            /// The property to be attached to an element.
            /// </summary>
            property,
            /// <summary>
            /// The end of header declaration.
            /// </summary>
            end_header
        }

        /// <summary>
        /// Represents a ply element.
        /// </summary>
        public class PlyElement
        {
            /// <summary>
            /// Initializes a new <see cref="PlyElement"/>.
            /// </summary>
            /// <param name="name">The name of this element.</param>
            /// <param name="count">The number of instances of this element.</param>
            /// <param name="instances">The instances of this elements properties.</param>
            public PlyElement(string name, int count, List<PlyProperty[]> instances)
            {
                this.Name = name;
                this.Count = count;
                this.Instances = instances;
            }

            /// <summary>
            /// The name of this element.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// The number of times this element is expected to appear.
            /// </summary>
            public int Count { get; }

            /// <summary>
            /// The instances of this elements properties.
            /// </summary>
            /// <remarks>
            /// An element can have any number of properties and that list
            /// of properties can appear <see cref="Count"/> number of times.
            /// This property holds those values.
            /// </remarks>
            public List<PlyProperty[]> Instances { get; }
        }

        /// <summary>
        /// Represents a property of a <see cref="PlyElement"/>.
        /// </summary>
        public class PlyProperty
        {
            /// <summary>
            /// Initializes a new ply property with the specified values.
            /// </summary>
            /// <param name="name">The name of the property.</param>
            /// <param name="type">The type of the property.</param>
            /// <param name="value">The value of the property.</param>
            /// <param name="isList">Specifies whether the property is a list or not.</param>
            /// <param name="listContentType">The type of contents in the list if it is a list.</param>
            /// <param name="listContentValues">The items in the property's list.</param>
            public PlyProperty(string name, PlyDataTypes type, object value, bool isList, PlyDataTypes listContentType, object[] listContentValues)
            {
                this.Name = name;
                this.Type = type;
                this.Value = value;
                this.IsList = isList;
                this.ListContentType = listContentType;
                this.ListContentValues = listContentValues;
            }

            /// <summary>
            /// The name of this property.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// For a scalar property: the type of value it holds.<para/>
            /// For a vector property: the type of the items count value.
            /// </summary>
            /// <remarks>
            /// A scalar property is a property where <see cref="IsList"/> is false.
            /// </remarks>
            public PlyDataTypes Type { get; }

            /// <summary>
            /// For a scalar property: The value of this property.<para/>
            /// For a vector property: The number of items in the list.
            /// </summary>
            public object Value { get; }

            /// <summary>
            /// Specifies whether this property is a scalar or vector (list).
            /// </summary>
            public bool IsList { get; }

            /// <summary>
            /// The type of items in the list.
            /// </summary>
            public PlyDataTypes ListContentType { get; }

            /// <summary>
            /// The value of the items in the list.
            /// </summary>
            public object[] ListContentValues { get; }
        }

        /// <summary>
        /// Represents the header of a ply file.
        /// </summary>
        public class PlyHeader
        {
            /// <summary>
            /// Initializes a ply header with type <see cref="PlyFormatTypes.ascii"/> and no elements, comments and object infos.
            /// </summary>
            public PlyHeader()
            {
                this.FormatType = PlyFormatTypes.ascii;
                this.Version = SUPPORTEDVERSION;
                this.Comments = new string[] { };
                this.ObjectInfos = new Tuple<string, string>[] { };
                this.Elements = new PlyElement[] { };
            }

            /// <summary>
            /// Initializes a new Ply header with the given values.
            /// </summary>
            /// <param name="plyFormatType"></param>
            /// <param name="version"></param>
            /// <param name="elements"></param>
            /// <param name="objInfos"></param>
            /// <param name="comments"></param>
            public PlyHeader(PlyFormatTypes plyFormatType, Version version, PlyElement[] elements, Tuple<string, string>[] objInfos, string[] comments)
            {
                this.FormatType = plyFormatType;
                this.Version = version;
                this.ObjectInfos = objInfos;
                this.Comments = comments;
                this.Elements = elements;
            }

            /// <summary>
            /// The format of the ply file's body.
            /// </summary>
            public PlyFormatTypes FormatType { get; }

            /// <summary>
            /// The version of the ply file.
            /// </summary>
            public Version Version { get; }

            /// <summary>
            /// Gets the comments made in the file.
            /// </summary>
            public string[] Comments { get; }

            /// <summary>
            /// Gets the object informations for this file (mostly producer independent).
            /// </summary>
            public Tuple<string, string>[] ObjectInfos { get; }

            /// <summary>
            /// Gets the elements declared in the header.
            /// </summary>
            public PlyElement[] Elements { get; }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Reads and validates the header lines of a ply file.
        /// </summary>
        /// <param name="headerLines">The lines to read.</param>
        /// <returns></returns>
        private PlyHeader ReadHeader(string[] headerLines)
        {
            if (headerLines.Length > 2 && (PlyHeaderItems)Enum.Parse(typeof(PlyHeaderItems), headerLines[0]) == PlyHeaderItems.ply
                && (PlyHeaderItems)Enum.Parse(typeof(PlyHeaderItems), headerLines[^1]) == PlyHeaderItems.end_header)
            {
                string[] formatSpecLineParts = headerLines[1].Split(' ');
                string formatStr = formatSpecLineParts[0];
                string formatTypeStr = formatSpecLineParts[1];
                Version fileVersion = Version.Parse(formatSpecLineParts[2]);

                if ((PlyHeaderItems)Enum.Parse(typeof(PlyHeaderItems), formatStr) == PlyHeaderItems.format
                    && Enum.TryParse(formatTypeStr, out PlyFormatTypes formatType) && fileVersion <= SUPPORTEDVERSION)
                {
                    List<string> comments = new();
                    List<Tuple<string, string>> objInfos = new();
                    List<PlyElement> elements = new();

                    for (int i = 2; i < headerLines.Length - 1; i++)
                    {
                        string[] lineParts = headerLines[i].Split(' ');
                        if (Enum.TryParse(lineParts[0], out PlyHeaderItems headerItemType))
                        {
                            switch (headerItemType)
                            {
                                case PlyHeaderItems.element:
                                    {
                                        if (lineParts.Length == 3)
                                        {
                                            string elementName = lineParts[1];
                                            int elementCount = int.Parse(lineParts[2]);
                                            PlyElement element = new(elementName, elementCount, new List<PlyProperty[]> { new PlyProperty[] { } });
                                            elements.Add(element);
                                        }
                                        break;
                                    }
                                case PlyHeaderItems.property:
                                    {
                                        if (lineParts.Length >= 3 && elements.Count > 0)
                                        {
                                            if (lineParts[1] != "list" && lineParts.Length == 3)
                                            {
                                                if (Enum.TryParse($"_{lineParts[1]}", out PlyDataTypes propertyType))
                                                {
                                                    string propertyName = lineParts[2];

                                                    PlyProperty property = new(propertyName, propertyType, null, false, PlyDataTypes._char, null);

                                                    List<PlyProperty> newPropertyList = new();
                                                    for (int j = 0; j < elements.Last().Instances[0].Length; j++)
                                                    {
                                                        newPropertyList.Add(elements.Last().Instances[0][j]);
                                                    }
                                                    newPropertyList.Add(property);
                                                    elements.Last().Instances[0] = newPropertyList.ToArray();
                                                }
                                                else
                                                {
                                                    throw new InvalidDataException($"Invalid data type, {lineParts[1]}.");
                                                }
                                            }
                                            else if (lineParts[1] == "list" && lineParts.Length == 5)
                                            {
                                                //array property
                                                if (Enum.TryParse($"_{lineParts[2]}", out PlyDataTypes propertyType) && Enum.TryParse($"_{lineParts[3]}", out PlyDataTypes listContentType))
                                                {
                                                    string propertyName = lineParts[4];

                                                    PlyProperty property = new(propertyName, propertyType, null, true, listContentType, null);

                                                    List<PlyProperty> newPropertyList = new();
                                                    for (int j = 0; j < elements.Last().Instances[0].Length; j++)
                                                    {
                                                        newPropertyList.Add(elements.Last().Instances[0][j]);
                                                    }
                                                    newPropertyList.Add(property);
                                                    elements.Last().Instances[0] = newPropertyList.ToArray();

                                                }
                                                else
                                                {
                                                    throw new InvalidDataException($"Invalid data type, {lineParts[1]}.");
                                                }
                                            }
                                            else
                                            {
                                                throw new InvalidDataException("Invalid property definition.");
                                            }
                                        }
                                        break;
                                    }
                                case PlyHeaderItems.obj_info:
                                    {
                                        if (lineParts.Length == 3)
                                        {
                                            objInfos.Add(new Tuple<string, string>(lineParts[1], lineParts[2]));
                                        }
                                        else
                                        {
                                            objInfos.Add(new Tuple<string, string>($"htk_info_{objInfos.Count}", headerLines[i][(lineParts[0].Length + 1)..]));
                                        }
                                        break;
                                    }
                                case PlyHeaderItems.comment:
                                    {
                                        comments.Add(headerLines[i][(lineParts[0].Length + 1)..]);
                                        break;
                                    }
                                default:
                                    {
                                        throw new InvalidDataException($"Unknown header item, {lineParts[0]}.");
                                    }
                            }
                        }
                        else
                        {
                            throw new InvalidDataException($"Unknown header item, {lineParts[0]}.");
                        }
                    }

                    PlyHeader plyHeader = new(formatType, fileVersion, elements.ToArray(), objInfos.ToArray(), comments.ToArray());
                    return plyHeader;
                }
                else
                {
                    throw new InvalidDataException("Invalid format specification.");
                }
            }
            else
            {
                throw new InvalidDataException("Invalid ply file.");
            }
        }

        /// <summary>
        /// Converts the value of a property to the specified data type.
        /// </summary>
        /// <param name="plyDataType">The type to convert to.</param>
        /// <param name="propValue">The value to convert.</param>
        /// <returns></returns>
        private object ConvertPropValueASCII(PlyDataTypes plyDataType, string propValue)
        {
            object result = null;
            switch (plyDataType)
            {
                case PlyDataTypes._char:
                    result = Convert.ToChar(propValue);
                    break;
                case PlyDataTypes._uint8:
                case PlyDataTypes._uchar:
                    result = Convert.ToByte(propValue);
                    break;
                case PlyDataTypes._short:
                    result = short.Parse(propValue);
                    break;
                case PlyDataTypes._ushort:
                    result = ushort.Parse(propValue);
                    break;
                case PlyDataTypes._int:
                case PlyDataTypes._int32:
                    result = int.Parse(propValue);
                    break;
                case PlyDataTypes._uint:
                    result = uint.Parse(propValue);
                    break;
                case PlyDataTypes._float:
                case PlyDataTypes._float32:
                    result = float.Parse(propValue);
                    break;
                case PlyDataTypes._double:
                    result = double.Parse(propValue);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Reads the value of a property in the specified data type.
        /// </summary>
        /// <param name="plyDataType"></param>
        /// <param name="reader"></param>
        /// <param name="bigEndian"></param>
        /// <returns></returns>
        private object ConvertPropValueBinary(PlyDataTypes plyDataType, BinaryReader reader, bool bigEndian)
        {
            bool reverseBytes = bigEndian && BitConverter.IsLittleEndian;
            object result;
            switch (plyDataType)
            {
                case PlyDataTypes._char:
                    {
                        result = reverseBytes ? BitConverter.ToChar(reader.ReadBytes(1).Reverse().ToArray(), 0) : reader.ReadChar();
                        break;
                    }
                case PlyDataTypes._uint8:
                case PlyDataTypes._uchar:
                    {
                        result = reader.ReadByte();
                        break;
                    }
                case PlyDataTypes._short:
                    {
                        result = reverseBytes ? BitConverter.ToInt16(reader.ReadBytes(2).Reverse().ToArray(), 0) : reader.ReadInt16();
                        break;
                    }
                case PlyDataTypes._ushort:
                    {
                        result = reverseBytes ? BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0) : reader.ReadUInt16();
                        break;
                    }
                case PlyDataTypes._int:
                case PlyDataTypes._int32:
                    {
                        result = reverseBytes ? BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0) : reader.ReadInt32();
                        break;
                    }
                case PlyDataTypes._uint:
                    {
                        result = reverseBytes ? BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0) : reader.ReadUInt32();
                        break;
                    }
                case PlyDataTypes._float:
                case PlyDataTypes._float32:
                    {
                        result = reverseBytes ? BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0) : reader.ReadSingle();
                        break;
                    }
                case PlyDataTypes._double:
                    {
                        result = reverseBytes ? BitConverter.ToDouble(reader.ReadBytes(8).Reverse().ToArray(), 0) : reader.ReadDouble();
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unimplemented data conversion.");
            }

            return result;
        }

        /// <summary>
        /// Reads a ply file in an ascii format.
        /// </summary>
        /// <param name="s">The stream to read from.</param>
        /// <param name="plyHeader">The header of the ply file.</param>
        private List<PlyElement> ReadASCII(Stream s, PlyHeader plyHeader)
        {
            List<PlyElement> plyBody = new();
            using (StreamReader reader = new(s, Encoding.ASCII))
            {
                //The index of the element being read from the header.
                int currentElementIdx = 0;
                //The index of the instances of the current element.
                int currentIdx = 0;
                List<PlyProperty[]> currentPlyElementProperties = new();
                PlyElement currentHeadElement = plyHeader.Elements[currentElementIdx];
                int debLineNo = 0;
                while (!reader.EndOfStream)
                {
                    debLineNo++;
                    if (currentElementIdx < plyHeader.Elements.Length)
                    {
                        string currentLine = reader.ReadLine()?.Trim();
                        string[] lineDataArr = currentLine?.Split(' ');
                    readElementInstance:
                        if (currentIdx < currentHeadElement.Count)
                        {
                            PlyProperty[] plyHeadProperties = currentHeadElement.Instances[0];
                            //The number of items from the current lineDataArr start position(0)
                            //This allows an element to contain multiple lists and properties
                            int idxOffset = 0;
                            List<PlyProperty> plyBodyProperties = new();
                            for (int i = 0; i < plyHeadProperties.Length; i++)
                            {
                                PlyProperty currentPlyHeadProp = plyHeadProperties[i];
                                if (currentPlyHeadProp.IsList)
                                {
                                    object itemsNumStr = this.ConvertPropValueASCII(currentPlyHeadProp.Type, lineDataArr[idxOffset]);
                                    if (int.TryParse(itemsNumStr.ToString(), out int itemsNum))
                                    {
                                        idxOffset++;

                                        List<object> listContentItems = new();
                                        for (int j = 0; j < itemsNum; j++)
                                        {
                                            object listContentItem = this.ConvertPropValueASCII(currentPlyHeadProp.ListContentType, lineDataArr[idxOffset]);
                                            listContentItems.Add(listContentItem);
                                            idxOffset++;
                                        }
                                        PlyProperty plyBodyProp = new(currentPlyHeadProp.Name, currentPlyHeadProp.Type,
                                            itemsNum, currentPlyHeadProp.IsList, currentPlyHeadProp.ListContentType, listContentItems.ToArray());
                                        plyBodyProperties.Add(plyBodyProp);
                                    }
                                    else
                                    {
                                        throw new InvalidDataException("Invalid list items count.");
                                    }
                                }
                                else
                                {
                                    PlyProperty plyBodyProp = new(currentPlyHeadProp.Name, currentPlyHeadProp.Type,
                                        this.ConvertPropValueASCII(currentPlyHeadProp.Type, lineDataArr[idxOffset]), currentPlyHeadProp.IsList, currentPlyHeadProp.ListContentType, null);
                                    plyBodyProperties.Add(plyBodyProp);
                                    idxOffset++;
                                }
                            }

                            currentPlyElementProperties.Add(plyBodyProperties.ToArray());
                            currentIdx++;
                        }
                        else if (currentIdx == currentHeadElement.Count)
                        {
                            PlyElement plyBodyElement = new(currentHeadElement.Name, currentHeadElement.Count, currentPlyElementProperties);
                            plyBody.Add(plyBodyElement);

                            currentElementIdx++;
                            currentIdx = 0;
                            currentPlyElementProperties = new List<PlyProperty[]>();
                            currentHeadElement = plyHeader.Elements[currentElementIdx];
                            goto readElementInstance;
                        }
                        else
                        {
                            throw new InvalidOperationException("Index was pushed too far out.");
                        }
                    }
                    else if (currentElementIdx == plyHeader.Elements.Length)
                    {

                    }
                }

                PlyElement lastPlyBodyElement = new(currentHeadElement.Name, currentHeadElement.Count, currentPlyElementProperties);
                plyBody.Add(lastPlyBodyElement);
            }
            return plyBody;
        }

        /// <summary>
        /// Reads a ply file in a binary big endian format or in a binary little endian format.
        /// </summary>
        /// <param name="s">The stream to read from.</param>
        /// <param name="plyHeader">The header of the ply file.</param>
        /// <param name="bigEndian">Specifies whether the byte order is big endian or little endian.</param>
        /// <returns>
        /// The list of Ply elements declared in the header.
        /// </returns>
        private List<PlyElement> ReadBinary(Stream s, PlyHeader plyHeader, bool bigEndian)
        {
            List<PlyElement> plyBody = new();
            Encoding streamEncoding = bigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;
            using (BinaryReader reader = new(s, streamEncoding))
            {
                for (int i = 0; i < plyHeader.Elements.Length; i++)
                {
                    PlyElement currentHeadElement = plyHeader.Elements[i];
                    List<PlyProperty[]> currentElementInstanceProperties = new();

                    for (int j = 0; j < currentHeadElement.Count; j++)
                    {
                        List<PlyProperty> currentInstanceProperties = new();

                        for (int k = 0; k < currentHeadElement.Instances[0].Length; k++)
                        {
                            PlyProperty currentHeadProp = currentHeadElement.Instances[0][k];
                            if (currentHeadProp.IsList)
                            {
                                object itemsNumStr = this.ConvertPropValueBinary(currentHeadProp.Type, reader, bigEndian);
                                if (int.TryParse(itemsNumStr.ToString(), out int itemsNum))
                                {
                                    List<object> listContentItems = new();
                                    for (int l = 0; l < itemsNum; l++)
                                    {
                                        object listContentItem = this.ConvertPropValueBinary(currentHeadProp.ListContentType, reader, bigEndian);
                                        listContentItems.Add(listContentItem);
                                    }
                                    PlyProperty plyProp = new(currentHeadProp.Name, currentHeadProp.Type,
                                        itemsNum, currentHeadProp.IsList, currentHeadProp.ListContentType, listContentItems.ToArray());
                                    currentInstanceProperties.Add(plyProp);
                                }
                                else
                                {
                                    throw new InvalidDataException("Invalid list items count.");
                                }
                            }
                            else
                            {
                                PlyProperty newProperty = new(currentHeadProp.Name, currentHeadProp.Type,
                                    this.ConvertPropValueBinary(currentHeadProp.Type, reader, bigEndian), currentHeadProp.IsList, currentHeadProp.ListContentType, currentHeadProp.ListContentValues);
                                currentInstanceProperties.Add(newProperty);
                            }
                        }

                        currentElementInstanceProperties.Add(currentInstanceProperties.ToArray());
                    }

                    PlyElement plyElement = new(currentHeadElement.Name, currentHeadElement.Count, currentElementInstanceProperties);
                    plyBody.Add(plyElement);
                }
            }


            return plyBody;
        }

        /// <summary>
        /// Writes the ply header and body to a ply file in an ASCII format.
        /// </summary>
        /// <param name="dumpPath"></param>
        /// <param name="plyHeader"></param>
        /// <param name="plyBody"></param>
        private void DumpAsASCII(string dumpPath, PlyHeader plyHeader, List<PlyElement> plyBody)
        {
            using FileStream fs = new(dumpPath, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            #region Header
            sw.WriteLine("ply");
            sw.WriteLine("format ascii 1.0");
            foreach (string comment in plyHeader.Comments)
            {
                sw.WriteLine($"comment {comment}");
            }

            foreach (Tuple<string, string> objInfo in plyHeader.ObjectInfos)
            {
                sw.WriteLine($"obj_info {objInfo.Item1} {objInfo.Item2}");
            }

            foreach (PlyElement element in plyHeader.Elements)
            {
                sw.WriteLine($"element {element.Name} {element.Count}");
                foreach (PlyProperty propertyTemplate in element.Instances[0])
                {
                    if (propertyTemplate.IsList)
                    {
                        sw.WriteLine($"property list {propertyTemplate.Type.ToString()[1..]} {propertyTemplate.ListContentType.ToString()[1..]} {propertyTemplate.Name}");
                    }
                    else
                    {
                        sw.WriteLine($"property {propertyTemplate.Type.ToString()[1..]} {propertyTemplate.Name}");
                    }
                }
            }
            sw.WriteLine("end_header");
            #endregion

            #region Body
            foreach (PlyElement element in plyBody)
            {
                foreach (PlyProperty[] instances in element.Instances)
                {
                    StringBuilder instanceBuilder = new();
                    foreach (PlyProperty property in instances)
                    {
                        if (property.IsList)
                        {
                            _ = instanceBuilder.Append($" {property.ListContentValues.Length}");
                            for (int i = 0; i < property.ListContentValues.Length; i++)
                            {
                                _ = instanceBuilder.Append($" {property.ListContentValues[i]}");
                            }
                        }
                        else
                        {
                            _ = instanceBuilder.Append($" {property.Value?.ToString()}");
                        }
                    }
                    sw.WriteLine(instanceBuilder.ToString().Trim());
                }
            }
            #endregion

        }

        #endregion

    }
}
