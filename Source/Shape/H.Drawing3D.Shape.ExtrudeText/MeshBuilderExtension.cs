// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector3DExtensions.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Extension methods for Vector3D.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using H.Drawing2D.Api.Triangle.Geometry;
using H.Drawing2D.Api.Triangle.Meshing;
using H.Drawing3D.Shape.Extension;
using H.Drawing3D.Shape.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using LineSegment = System.Windows.Media.LineSegment;
using Point = System.Windows.Point;
using Geometry = System.Windows.Media.Geometry;


namespace H.Drawing3D.Shape.ExtrudeText
{
    public static class MeshBuilderExtension
    {
        [Obsolete]
        public static void ExtrudeText(this IMeshBuilder builder, string text, string font, FontStyle fontStyle, FontWeight fontWeight, double fontSize, Vector3D textDirection, Point3D p0, Point3D p1)
        {
            IEnumerable<IList<Point[]>> outlineList = GetTextOutlines(text, font, fontStyle, fontWeight, fontSize);

            // Build the polygon to mesh (using Triangle.NET to triangulate)
            Drawing2D.Api.Triangle.Geometry.Polygon polygon = new();

            foreach (IList<Point[]> outlines in outlineList)
            {
                Point[] outerOutline = outlines.OrderBy(x => x.AreaOfSegment()).Last();

                for (int i = 0; i < outlines.Count; i++)
                {
                    Point[] outline = outlines[i];
                    bool isHole = i != outlines.Count - 1 && IsPointInPolygon(outerOutline, outline[0]);
                    //polygon.AddContour(outline.Select(p => new Vertex(p.X, p.Y)), marker++, isHole);

                    Contour contour = new(outline.Select(p => new Vertex(p.X, p.Y)));
                    polygon.Add(contour, isHole);
                    builder.AddExtrudedSegments(outline.ToSegments().ToList(), textDirection, p0, p1);
                }
            }

            GenericMesher mesher = new();
            ConstraintOptions options = new();
            IMesh mesh = mesher.Triangulate(polygon, options);

            Vector3D u = textDirection;
            u.Normalize();
            Vector3D z = p1 - p0;
            z.Normalize();
            Vector3D v = Vector3D.CrossProduct(z, u);

            // Convert the triangles
            foreach (Drawing2D.Api.Triangle.Topology.Triangle t in mesh.Triangles)
            {
                Vertex v0 = t.GetVertex(0);
                Vertex v1 = t.GetVertex(1);
                Vertex v2 = t.GetVertex(2);

                // Add the top triangle.
                // Project the X/Y vertices onto a plane defined by textdirection, p0 and p1.                
                builder.AddTriangle(v0.Project(p0, u, v, z, 1), v1.Project(p0, u, v, z, 1), v2.Project(p0, u, v, z, 1));

                // Add the bottom triangle.
                builder.AddTriangle(v2.Project(p0, u, v, z, 0), v1.Project(p0, u, v, z, 0), v0.Project(p0, u, v, z, 0));
            }
        }

        public static Point3D Project(this Vertex v, Point3D p0, Vector3D x, Vector3D y, Vector3D z, double h)
        {
            return p0 + (x * v.X) - (y * v.Y) + (z * h);
        }

        public static double AreaOfSegment(this Point[] segment)
        {
            return Math.Abs(segment.Take(segment.Length - 1)
                .Select((p, i) => (segment[i + 1].X - p.X) * (segment[i + 1].Y + p.Y))
                .Sum() / 2);
        }

        public static bool IsPointInPolygon(IList<Point> polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if ((polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y) || (polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y))
                {
                    if (polygon[i].X + ((testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X)) < testPoint.X)
                    {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }

        public static IEnumerable<System.Windows.Point> ToSegments(this IEnumerable<Point> input)
        {
            bool first = true;
            Point previous = default;
            foreach (Point point in input)
            {
                if (!first)
                {
                    yield return previous;
                    yield return point;
                }
                else
                {
                    first = false;
                }

                previous = point;
            }
        }

        [Obsolete]
        public static IEnumerable<IList<System.Windows.Point[]>> GetTextOutlines(string text, string fontName, FontStyle fontStyle, FontWeight fontWeight, double fontSize)
        {
            FormattedText formattedText = new(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily(fontName), fontStyle, fontWeight, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            System.Windows.Media.Geometry textGeometry = formattedText.BuildGeometry(new Point(0, 0));
            List<List<Point[]>> outlines = new();
            AppendOutlines(textGeometry, outlines);
            return outlines;
        }

        private static void AppendOutlines(System.Windows.Media.Geometry geometry, List<List<Point[]>> outlines)
        {
            if (geometry is GeometryGroup group)
            {
                foreach (System.Windows.Media.Geometry g in group.Children)
                {
                    AppendOutlines(g, outlines);
                }

                return;
            }

            if (geometry is PathGeometry pathGeometry)
            {
                List<Point[]> figures = pathGeometry.Figures.Select(figure => figure.ToPolyLine()).ToList();
                outlines.Add(figures);
                return;
            }

            throw new NotImplementedException();
        }

        public static Point[] ToPolyLine(this PathFigure figure)
        {
            List<Point> outline = new() { figure.StartPoint };
            Point previousPoint = figure.StartPoint;
            foreach (PathSegment segment in figure.Segments)
            {
                if (segment is PolyLineSegment polyline)
                {
                    outline.AddRange(polyline.Points);
                    previousPoint = polyline.Points.Last();
                    continue;
                }

                if (segment is PolyBezierSegment polybezier)
                {
                    for (int i = -1; i + 3 < polybezier.Points.Count; i += 3)
                    {
                        Point p1 = i == -1 ? previousPoint : polybezier.Points[i];
                        outline.AddRange(FlattenBezier(p1, polybezier.Points[i + 1], polybezier.Points[i + 2], polybezier.Points[i + 3], 10));
                    }

                    previousPoint = polybezier.Points.Last();
                    continue;
                }

                if (segment is LineSegment lineSegment)
                {
                    outline.Add(lineSegment.Point);
                    previousPoint = lineSegment.Point;
                    continue;
                }

                if (segment is BezierSegment bezierSegment)
                {
                    outline.AddRange(FlattenBezier(previousPoint, bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, 10));
                    previousPoint = bezierSegment.Point3;
                    continue;
                }

                throw new NotImplementedException();
            }

            return outline.ToArray();
        }

        private static IEnumerable<Point> FlattenBezier(Point p1, Point p2, Point p3, Point p4, int n)
        {
            // http://tsunami.cis.usouthal.edu/~hain/general/Publications/Bezier/bezier%20cccg04%20paper.pdf
            // http://en.wikipedia.org/wiki/De_Casteljau's_algorithm
            for (int i = 1; i <= n; i++)
            {
                double t = (double)i / n;
                double u = 1 - t;
                yield return new Point(
                    (u * u * u * p1.X) + (3 * t * u * u * p2.X) + (3 * t * t * u * p3.X) + (t * t * t * p4.X),
                    (u * u * u * p1.Y) + (3 * t * u * u * p2.Y) + (3 * t * t * u * p3.Y) + (t * t * t * p4.Y));
            }
        }
    }

    public static class MeshGeometryDrawing3DExtension
    {
        [Obsolete]
        public static void DrawExtrudeText(this IMeshGeometryDrawing3D drawing, string text, string font, FontStyle fontStyle, FontWeight fontWeight, double fontSize, Vector3D textDirection, Point3D p0, Point3D p1)
        {
            drawing.MeshBuilder.ExtrudeText(
                text,
                "Arial",
                fontStyle,
                fontWeight,
                fontSize,
                textDirection,
                p0,
                p1);

        }
    }
}