// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeshBuilder.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Builds MeshGeometry3D objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Geometry
{
    public interface IMeshBuilder
    {
        void AddArrow(Point3D point1, Point3D point2, double diameter, double headLength = 3, int thetaDiv = 18);
        void AddBoundingBox(Rect3D boundingBox, double diameter);
        void AddBox(Point3D center, double xlength, double ylength, double zlength);
        void AddBox(Point3D center, double xlength, double ylength, double zlength, BoxFaces faces);
        void AddBox(Point3D center, Vector3D x, Vector3D y, double xlength, double ylength, double zlength, BoxFaces faces = BoxFaces.All);
        void AddBox(Rect3D rectangle, BoxFaces faces = BoxFaces.All);
        void AddCone(Point3D origin, Point3D apex, double baseRadius, bool baseCap, int thetaDiv);
        void AddCone(Point3D origin, Vector3D direction, double baseRadius, double topRadius, double height, bool baseCap, bool topCap, int thetaDiv);
        void AddCube(BoxFaces faces = BoxFaces.All);
        void AddCubeFace(Point3D center, Vector3D normal, Vector3D up, double dist, double width, double height);
        void AddCylinder(Point3D p1, Point3D p2, double radius = 1, int thetaDiv = 32, bool cap1 = true, bool cap2 = true);
        void AddCylinder(Point3D p1, Point3D p2, double diameter, int thetaDiv);
        void AddDodecahedron(Point3D center, Vector3D forward, Vector3D up, double sideLength);
        void AddEdges(IList<Point3D> points, IList<int> edges, double diameter, int thetaDiv);
        void AddEllipsoid(Point3D center, double radiusx, double radiusy, double radiusz, int thetaDiv = 20, int phiDiv = 10);
        void AddExtrudedGeometry(IList<Point> points, Vector3D xaxis, Point3D p0, Point3D p1);
        void AddExtrudedSegments(IList<Point> points, Vector3D axisX, Point3D p0, Point3D p1);
        void AddFaceNX();
        void AddFaceNY();
        void AddFaceNZ();
        void AddFacePX();
        void AddFacePY();
        void AddFacePZ();
        void AddLoftedGeometry(IList<IList<Point3D>> positionsList, IList<IList<Vector3D>> normalList, IList<IList<Point>> textureCoordinateList);
        void AddNode(Point3D position, Vector3D normal, Point textureCoordinate);
        void AddOctahedron(Point3D center, Vector3D forward, Vector3D up, double sideLength, double height);
        void AddPipe(Point3D point1, Point3D point2, double innerDiameter, double diameter, int thetaDiv);
        void AddPipes(IList<Vector3D> points, IList<int> edges, double diameter = 1, int thetaDiv = 32);
        void AddPolygon(IList<int> vertexIndices);
        void AddPolygon(IList<Point> points, Vector3D axisX, Vector3D axisY, Point3D origin);
        void AddPolygon(IList<Point3D> points);
        void AddPolygonByCuttingEars(IList<int> vertexIndices);
        void AddPolygonByTriangulation(IList<int> vertexIndices);
        void AddPyramid(Point3D center, double sideLength, double height, bool closeBase = false);
        void AddPyramid(Point3D center, Vector3D forward, Vector3D up, double sideLength, double height, bool closeBase = false);
        void AddQuad(IList<int> vertexIndices);
        void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3);
        void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point uv0, Point uv1, Point uv2, Point uv3);
        void AddQuads(IList<Point3D> quadPositions, IList<Vector3D> quadNormals, IList<Point> quadTextureCoordinates);
        void AddRectangularMesh(BoxFaces plane, int columns, int rows, double width, double height, bool flipTriangles = false, bool flipTexCoordsUAxis = false, bool flipTexCoordsVAxis = false);
        void AddRectangularMesh(IList<Point3D> points, int columns);
        void AddRectangularMesh(IList<Point3D> points, int columns, bool flipTriangles = false);
        void AddRectangularMesh(Point3D[,] points, Point[,] texCoords = null, bool closed0 = false, bool closed1 = false);
        void AddRectangularMeshTriangleIndices(int index0, int rows, int columns, bool isSpherical = false);
        void AddRectangularMeshTriangleIndices(int index0, int rows, int columns, bool rowsClosed, bool columnsClosed);
        void AddRegularIcosahedron(Point3D center, double radius, bool shareVertices);
        void AddRevolvedGeometry(IList<Point> points, IList<double> textureValues, Point3D origin, Vector3D direction, int thetaDiv);
        void AddSphere(Point3D center, double radius = 1, int thetaDiv = 32, int phiDiv = 32);
        void AddSubdivisionSphere(Point3D center, double radius, int subdivisions);
        void AddSurfaceOfRevolution(Point3D origin, Vector3D axis, IList<Point> section, IList<int> sectionIndices, int thetaDiv = 37, IList<double> textureValues = null);
        void AddTetrahedron(Point3D center, Vector3D forward, Vector3D up, double sideLength);
        void AddTorus(double torusDiameter, double tubeDiameter, int thetaDiv = 36, int phiDiv = 24);
        void AddTriangle(IList<int> vertexIndices);
        void AddTriangle(Point3D p0, Point3D p1, Point3D p2);
        void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Point uv0, Point uv1, Point uv2);
        void AddTriangleFan(IList<int> vertices);
        void AddTriangleFan(IList<Point3D> fanPositions, IList<Vector3D> fanNormals = null, IList<Point> fanTextureCoordinates = null);
        void AddTriangles(IList<Point3D> trianglePositions, IList<Vector3D> triangleNormals = null, IList<Point> triangleTextureCoordinates = null);
        void AddTriangleStrip(IList<Point3D> stripPositions, IList<Vector3D> stripNormals = null, IList<Point> stripTextureCoordinates = null);
        void AddTube(IList<Point3D> path, double diameter, int thetaDiv, bool isTubeClosed, bool frontCap = false, bool backCap = false);
        void AddTube(IList<Point3D> path, double[] values, double[] diameters, int thetaDiv, bool isTubeClosed, bool frontCap = false, bool backCap = false);
        void AddTube(IList<Point3D> path, IList<double> angles, IList<double> values, IList<double> diameters, IList<Point> section, Vector3D sectionXAxis, bool isTubeClosed, bool isSectionClosed, bool frontCap = false, bool backCap = false);
        void AddTube(IList<Point3D> path, IList<double> values, IList<double> diameters, IList<Point> section, bool isTubeClosed, bool isSectionClosed, bool frontCap = false, bool backCap = false);
        void Append(IList<Point3D> positionsToAppend, IList<int> triangleIndicesToAppend, IList<Vector3D> normalsToAppend = null, IList<Point> textureCoordinatesToAppend = null);
        void Append(MeshGeometry3D mesh);
        void Scale(double scaleX, double scaleY, double scaleZ);
        void SubdivideLinear(bool barycentric = false);
        MeshGeometry3D ToMesh(bool freeze = false);
    }
}