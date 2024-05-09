using H.Drawing3D.Drawing.Base;
using H.Drawing3D.Shape.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Shape.Extension
{
    public interface IMeshGeometryDrawing3D : IGeometryDrawing3D
    {
        IMeshBuilder MeshBuilder { get; }
    }

    public static class MeshGeometryDrawing3DExtension
    {
        public static void DrawDodecahedron(this IMeshGeometryDrawing3D drawing, Point3D center, Vector3D forward, Vector3D up, double sideLength)
        {
            drawing.MeshBuilder.AddDodecahedron(center, forward, up, sideLength);
        }

        /// <summary>
        /// 绘制多线
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="diameter"></param>
        /// <param name="thetaDiv"></param>
        /// <param name="points"></param>
        public static void DrawPolyLine(this IMeshGeometryDrawing3D drawing, double diameter = 0.1, int thetaDiv = 10, params Point3D[] points)
        {
            for (int i = 0; i < points.Count(); i++)
            {
                if (i == 0)
                {
                    continue;
                }

                drawing.MeshBuilder.AddCylinder(points[i - 1], points[i], diameter, thetaDiv, true, true);
            }
        }

        /// <summary>
        /// 绘制矩形框
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="boundingBox"></param>
        /// <param name="diameter"></param>
        public static void DrawBoundingBox(this IMeshGeometryDrawing3D drawing, Rect3D boundingBox, double diameter = 0.1)
        {
            drawing.MeshBuilder.AddBoundingBox(boundingBox, diameter);
        }

        /// <summary>
        /// 绘制箭头
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="diameter"></param>
        /// <param name="headLength"></param>
        /// <param name="ThetaDiv"></param>
        public static void DrawArrow(this IMeshGeometryDrawing3D drawing, Point3D point1, Point3D point2, double diameter = 1, double headLength = 3, int ThetaDiv = 36)
        {
            if (diameter <= 0)
            {
                return;
            }

            drawing.MeshBuilder.AddArrow(point1, point2, diameter, headLength, ThetaDiv);
        }

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="radius"></param>
        /// <param name="thetaDiv"></param>
        /// <param name="cap1"></param>
        /// <param name="cap2"></param>
        public static void DrawCylinder(this IMeshGeometryDrawing3D drawing, Point3D p1, Point3D p2, double radius = 1, int thetaDiv = 32, bool cap1 = true, bool cap2 = true)
        {
            drawing.MeshBuilder.AddCylinder(p1, p2, radius, thetaDiv, cap1, cap2);
        }

        /// <summary>
        /// 转换成Geometry3D
        /// </summary>
        /// <param name="drawing"></param>
        /// <returns></returns>
        public static Geometry3D ToMesh(this IMeshGeometryDrawing3D drawing)
        {
            return drawing.MeshBuilder.ToMesh();
        }

        /// <summary>
        /// 绘制坐标轴矩形框
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="bound"></param>
        /// <param name="lineThickness"></param>
        /// <param name="intervalX"></param>
        /// <param name="intervalY"></param>
        /// <param name="intervalZ"></param>
        public static void DrawAxisBoundingBox(this IMeshGeometryDrawing3D drawing, Rect3D bound, double lineThickness = 0.01, double intervalX = 1, double intervalY = 1, double intervalZ = 1)
        {
            double minX = bound.X;
            double maxX = bound.X + bound.SizeX;
            double minY = bound.Y;
            double maxY = bound.Y + bound.SizeY;
            double minZ = bound.Z;
            double maxZ = bound.Z + bound.SizeZ;
            drawing.DrawBoundingBox(bound, lineThickness);
            for (double x = minX; x <= maxX; x += intervalX)
            {
                {
                    //Y
                    List<Point3D> path = new()
                    {
                        new Point3D(x, minY, minZ),
                        new Point3D(x, maxY, minZ)
                    };
                    drawing.DrawPolyLine(lineThickness, 9, path.ToArray());
                }

                {
                    //Z
                    List<Point3D> path = new()
                    {
                        new Point3D(x, minY, minZ),
                        new Point3D(x, minY, maxZ)
                    };
                    drawing.DrawPolyLine(lineThickness, 9, path.ToArray());
                }
                ////  Do ：对面
                //{
                //    var path = new List<Point3D> { new Point3D(x, maxY, maxZ) };
                //    path.Add(new Point3D(x, minY, maxZ));
                //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
                //}

                //{
                //    var path = new List<Point3D> { new Point3D(x, maxY, maxZ) };
                //    path.Add(new Point3D(x, maxY, minZ));
                //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
                //}
            }

            for (double y = minY; y <= maxY; y += intervalY)
            {
                {
                    //  Do ：X
                    List<Point3D> path = new()
                    {
                        new Point3D(minX, y, minZ),
                        new Point3D(maxX, y, minZ)
                    };
                    drawing.DrawPolyLine(lineThickness, 9, path.ToArray());
                }

                {
                    //  Do ：Z
                    List<Point3D> path = new()
                    {
                        new Point3D(minX, y, minZ),
                        new Point3D(minX, y, maxZ)
                    };
                    drawing.DrawPolyLine(lineThickness, 9, path.ToArray());
                }

                //{
                //    var path = new List<Point3D> { new Point3D(maxX, y, maxX) };
                //    path.Add(new Point3D(maxX, y, minZ));
                //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
                //}

                //{
                //    var path = new List<Point3D> { new Point3D(minX, y, maxX) };
                //    path.Add(new Point3D(maxX, y, maxX));
                //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
                //}
            }


            for (double z = minZ; z <= maxZ; z += intervalZ)
            {
                {
                    //  Do ：X
                    List<Point3D> path = new()
                    {
                        new Point3D(minX, minY, z),
                        new Point3D(maxX, minY, z)
                    };
                    drawing.DrawPolyLine(lineThickness, 9, path.ToArray());
                }

                {
                    //  Do ：Y
                    List<Point3D> path = new()
                    {
                        new Point3D(minX, minY, z),
                        new Point3D(minX, maxY, z)
                    };
                    drawing.DrawPolyLine(lineThickness, 9, path.ToArray());
                }

                //{
                //    var path = new List<Point3D> { new Point3D(maxX, maxY, z) };
                //    path.Add(new Point3D(minX, maxY, z));
                //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
                //}

                //{
                //    var path = new List<Point3D> { new Point3D(maxX, maxY, z) };
                //    path.Add(new Point3D(maxX, minY, z));
                //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
                //}
            }

            //for (double x = minX; x <= maxX; x += intervalX)
            //{
            //    {
            //        var path = new List<Point3D> { new Point3D(x, minY, minZ) };
            //        path.Add(new Point3D(x, maxY, minZ));
            //        axesMeshBuilder.AddTube(path, lineThickness, 9, false);
            //        //GeometryModel3D label = TextCreator.CreateTextLabelModel3D(x.ToString(), Brushes.Black, true, FontSize,
            //        //                                                           new Point3D(x, minY - FontSize * 2.5, minZ),
            //        //                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
            //        //plotModel.Children.Add(label);
            //    }

            //}

            //for (double x = minX; x <= maxX; x += intervalX)
            //{
            //    {
            //        var path = new List<Point3D> { new Point3D(x, minY, minZ) };
            //        path.Add(new Point3D(x, minY, minZ));
            //        axesMeshBuilder.AddTube(path, lineThickness, 9, false);
            //        //GeometryModel3D label = TextCreator.CreateTextLabelModel3D(x.ToString(), Brushes.Black, true, FontSize,
            //        //                                                           new Point3D(x, minY - FontSize * 2.5, minZ),
            //        //                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
            //        //plotModel.Children.Add(label);
            //    }
            //}

            //for (double x = minX; x <= maxX; x += intervalX)
            //{

            //    {
            //        var path = new List<Point3D> { new Point3D(x, minY, minZ) };
            //        path.Add(new Point3D(x, maxY, maxZ));
            //        axesMeshBuilder.AddTube(path, lineThickness, 9, false);
            //        //GeometryModel3D label = TextCreator.CreateTextLabelModel3D(x.ToString(), Brushes.Black, true, FontSize,
            //        //                                                           new Point3D(x, minY - FontSize * 2.5, minZ),
            //        //                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
            //        //plotModel.Children.Add(label);
            //    }
            //}
            //for (double y = minY; y <= maxY; y += intervalY)
            //{
            //    var path = new List<Point3D> { new Point3D(minX, y, minZ) };
            //    path.Add(new Point3D(maxX, y, minZ));
            //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
            //    //GeometryModel3D label = TextCreator.CreateTextLabelModel3D(y.ToString(), Brushes.Black, true, FontSize,
            //    //                                                           new Point3D(minX - FontSize * 3, y, minZ),
            //    //                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
            //    //plotModel.Children.Add(label);
            //}



            //for (double z = minZ; z <= maxZ; z += intervalZ)
            //{
            //    var path = new List<Point3D> { new Point3D(minX, minY, z) };
            //    path.Add(new Point3D(maxX, minY, z));

            //    axesMeshBuilder.AddTube(path, lineThickness, 9, false);
            //    //GeometryModel3D label = TextCreator.CreateTextLabelModel3D(y.ToString(), Brushes.Black, true, FontSize,
            //    //                                                           new Point3D(minX - FontSize * 3, y, minZ),
            //    //                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
            //    //plotModel.Children.Add(label);
            //}
        }
    }
}