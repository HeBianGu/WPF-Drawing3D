// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubchainSamplingPolygonTriangulation.cs" company="Helix Toolkit">
//   Copyright (c) 2016 Franz Spitaler
// </copyright>
// <summary>
//   A polygon triangulator for simple polygons with no holes. Expected runtime is O(n log n)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
#if SHARPDX
    using Point = global::SharpDX.Vector2;
    using Int32Collection = System.Collections.Generic.List<int>;
    using DoubleOrSingle = System.Single;
#else
using System.Windows;
using System.Windows.Media;
using DoubleOrSingle = System.Double;

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
    /// Triangulate a simple Polygon with the Sweep-Line Algorithm
    /// </summary>
    /// <remarks>
    /// Based on http://www.cs.uu.nl/docs/vakken/ga/slides3.pdf
    /// References
    /// https://www.cs.ucsb.edu/~suri/cs235/Triangulation.pdf
    /// </remarks>
    public static class SweepLinePolygonTriangulator
    {
        /// <summary>
        /// Range Extension when searching for the Helper and Edge
        /// </summary>
        public static float Epsilon = 0.0000001f;

        /// <summary>
        /// Perform the Triangulation of the Input.
        /// </summary>
        /// <param name="polygon">The Input Polygon</param>
        /// <param name="holes">The Input Polygon</param>
        /// <returns>List of Indices representing the Triangulation of the Polygon</returns>
        public static Int32Collection Triangulate(IList<Point> polygon, List<List<Point>> holes = null)
        {
            // Allocate and initialize List of Indices in Polygon
            Int32Collection result = new();

            // Point-List from Input
            // (we don't want the first and last Point to be present two times)
            List<Point> points = polygon.ToList();
            if (points[0] == points[points.Count - 1])
            {
                points.RemoveAt(points.Count - 1);
            }
            int count = points.Count;

            // Sort the Input and create the Datastructures
            // Make the Polygon CounterClockWise
            bool didReverse = false;
            if (!IsCCW(polygon))
            {
                points.Reverse();
                didReverse = true;
            }

            // Skip Polygons that don't need Triangulation
            if (count < 3)
            {
                return null;
            }
            else if (count == 3)
            {
                return !didReverse ? new Int32Collection { 0, 1, 2 } : new Int32Collection { 0, 2, 1 };
            }

            PolygonData poly = new(points);

            if (holes != null)
            {
                foreach (List<Point> hole in holes)
                {
                    poly.AddHole(hole);
                }
            }
            // Sort Points from highest y to lowest y
            // and if two or more Points have the same y Value from lowest x to highest x Value
            List<PolygonPoint> events = new(poly.Points);
            events.Sort();

            // Calculate the Diagonals in the Down Sweep
            List<Tuple<int, int>> diagonals = CalculateDiagonals(events);
            // Reverse the Order of the Events
            events.Reverse();
            // Add the Diagonals in the Up Sweep (and remove duplicates)
            diagonals.AddRange(CalculateDiagonals(events, false));
            diagonals = diagonals.Distinct().ToList();

            // Use Diagonals to split into nonotone Polygons
            List<PolygonData> monotonePolygons = SplitIntoPolygons(poly, diagonals);

            // y-Monotone Polygons
            // Triangulate
            foreach (PolygonData monoton in monotonePolygons.Where(m => m != null))
            {
                Int32Collection indices = TriangulateMonotone(monoton);
                foreach (int index in indices)
                {
                    result.Add(index);
                }
            }

            // If we reversed the Polygon,
            // we need to reverse the result also to get a correct Triangulation
            if (didReverse)
            {
                // Transform back every calculated Index
                for (int i = 0; i < result.Count; i++)
                {
                    result[i] = count - result[i] - 1;
                }
            }

            // Return all calculated Triangleindices
            return result;
        }

        /// <summary>
        /// Triangulate the y-Monotone Polygons.
        /// </summary>
        /// <param name="monoton">The y-Monotone Polygon to triangle</param>
        /// <returns>Index-List of Polygon Points (Indices from the original Polygon)</returns>
        private static Int32Collection TriangulateMonotone(PolygonData monoton)
        {
            // Collection to return
            Int32Collection result = new();

            // Sort the Events
            List<PolygonPoint> events = new(monoton.Points);
            events.Sort();

            // Stack of Events to push to and pop from
            Stack<PolygonPoint> pointStack = new();

            // Push the first two Events
            pointStack.Push(events[0]);
            pointStack.Push(events[1]);

            // Left- and right Chain for Triangulation
            PolygonPoint left = events[0].Next == events[1] ? events[1] : events[0];
            PolygonPoint right = events[0].Last == events[1] ? events[1] : events[0];

            // Count of Points
            int pointCnt = monoton.Points.Count;

            // Handle the 3rd...n-th Point to triangle
            for (int i = 2; i < pointCnt; i++)
            {
                // The current Point
                PolygonPoint newPoint = events[i];
                PolygonPoint top = pointStack.Peek();
                // If the new Point is not on the same side as the last Point on the Stack
                //if (!(leftChain.Contains(top) && leftChain.Contains(newPoint) || rightChain.Contains(top) && rightChain.Contains(newPoint)))
                if (!(top.Last == newPoint || top.Next == newPoint))
                {
                    // Determine this Point's Chain (left or right)
                    if (left.Next == newPoint)
                    {
                        left = newPoint;
                    }
                    else if (right.Last == newPoint)
                    {
                        right = newPoint;
                    }
                    // While there is a Point on the Stack
                    while (pointStack.Count != 0)
                    {
                        // Pop and set the third Point
                        top = pointStack.Pop();
                        // Third triangle Point
                        PolygonPoint p2 = top;
                        if (pointStack.Count != 0)
                        {
                            // Pop again
                            top = pointStack.Pop();

                            // Add to the result. The Order is depending on the Side
                            if (left == newPoint)
                            {
                                result.Add(newPoint.Index);
                                result.Add(p2.Index);
                                result.Add(top.Index);
                            }
                            else
                            {
                                result.Add(newPoint.Index);
                                result.Add(top.Index);
                                result.Add(p2.Index);
                            }
                        }
                        // If more Points are on the Stack,
                        // Push the Point back again, to be able to form the Triangles
                        if (pointStack.Count != 0)
                        {
                            pointStack.Push(top);
                        }
                    }
                    // Push the last to Points on the Stack
                    pointStack.Push(events[i - 1]);
                    pointStack.Push(newPoint);
                }
                // If the newPoint is on the same Side (i.e. Chain)
                else
                {
                    // Get to Point on the Stack
                    top = pointStack.Pop();
                    PolygonPoint p2 = top;

                    // Determine this Point's Chain (left or right)
                    if (left.Next == newPoint && right.Last == newPoint)
                    {
                        if (top.Last == newPoint)
                        {
                            right = newPoint;
                        }
                        else
                        {
                            left = top.Next == newPoint ? newPoint : throw new Exception("Triangulation error");
                        }
                    }
                    else if (left.Next == newPoint)
                    {
                        left = newPoint;
                    }
                    else if (right.Last == newPoint)
                    {
                        right = newPoint;
                    }

                    while (pointStack.Count != 0)
                    {
                        // If the Triangle is possible, add it to the result (Point Order depends on the Side)
                        if (right == newPoint && IsCCW(new List<Point> { newPoint.Point, p2.Point, pointStack.Peek().Point }))
                        {
                            top = pointStack.Pop();
                            result.Add(newPoint.Index);
                            result.Add(p2.Index);
                            result.Add(top.Index);
                            p2 = top;
                        }
                        else if (left == newPoint && !IsCCW(new List<Point> { newPoint.Point, p2.Point, pointStack.Peek().Point }))
                        {
                            top = pointStack.Pop();
                            result.Add(newPoint.Index);
                            result.Add(top.Index);
                            result.Add(p2.Index);
                            p2 = top;
                        }
                        // No Triangle possible, just leave the Loop
                        else
                        {
                            break;
                        }
                    }
                    // Push the last two Points on the Stack
                    pointStack.Push(p2);
                    pointStack.Push(newPoint);
                }
            }
            // Return the Triangulation
            return result;
        }

        /// <summary>
        /// Calculate the Diagonals to add inside the Polygon.
        /// </summary>
        /// <param name="events">The Events in sorted Form</param>
        /// <param name="sweepDown">True in the first Stage (sweeping down), false in the following Stages (sweeping up)</param>
        /// <returns></returns>
        private static List<Tuple<int, int>> CalculateDiagonals(List<PolygonPoint> events, bool sweepDown = true)
        {
            // Diagonals to add to the Polygon to make it monotone after the Down- and Up-Sweeps
            List<Tuple<int, int>> diagonals = new();

            // Construct Status and Helper, a List of Edges left of every Point of the Polygon
            // by shooting a Ray from the Vertex to the left.
            // The Helper Point of that Edge will be used to create Monotone Polygons
            // by adding Diagonals from/to Split- and Merge-Points
            StatusHelper statusAndHelper = new();

            // Sweep through the Polygon using the sorted Polygon Points
            for (int i = 0; i < events.Count; i++)
            {
                PolygonPoint ev = events[i];
                // Get the Class of this event (depending on the sweeping direction)
                PolygonPointClass evClass = ev.PointClass(!sweepDown);

                // Temporary StatusHelperElement
                StatusHelperElement she;

                // Handle the different Point-Classes
                switch (evClass)
                {
                    case PolygonPointClass.Start:
                        // Just add the left Edge (depending on the sweeping direction)
                        statusAndHelper.Add(new StatusHelperElement(sweepDown ? ev.EdgeTwo : ev.EdgeOne, ev));
                        break;
                    case PolygonPointClass.Stop:
                        // Just remove the left Edge (depending on the sweeping direction)
                        statusAndHelper.Remove(sweepDown ? ev.EdgeOne : ev.EdgeTwo);
                        break;
                    case PolygonPointClass.Regular:
                        // If the Polygon is positioned on the right Side of this Event
                        if (ev.Last > ev.Next)
                        {
                            // Replace the corresponding (old) StatusHelperElement with the new one
                            statusAndHelper.Remove(sweepDown ? ev.EdgeOne : ev.EdgeTwo);
                            statusAndHelper.Add(new StatusHelperElement(sweepDown ? ev.EdgeTwo : ev.EdgeOne, ev));
                        }
                        else
                        {
                            // Search Edge left of the Event and set Event as it's Helper
                            she = statusAndHelper.SearchLeft(ev);
                            if (she != null)
                            {
                                she.Helper = ev;
                            }
                        }
                        break;
                    case PolygonPointClass.Merge:
                        // Just remove the left Edge (depending on the sweeping direction)
                        statusAndHelper.Remove(sweepDown ? ev.EdgeOne : ev.EdgeTwo);
                        // Search Edge left of the Event and set Event as it's Helper
                        she = statusAndHelper.SearchLeft(ev);
                        if (she != null)
                        {
                            she.Helper = ev;
                        }

                        break;
                    case PolygonPointClass.Split:
                        // Search Edge left of the Event
                        she = statusAndHelper.SearchLeft(ev);
                        if (she != null)
                        {
                            // Chose diagonal from Helper of Edge to Event.
                            int minP = Math.Min(she.Helper.Index, ev.Index);
                            int maxP = Math.Max(she.Helper.Index, ev.Index);
                            Tuple<int, int> diagonal = new(minP, maxP);
                            diagonals.Add(diagonal);

                            // Replace the Helper of the StatusHelperElement by Event
                            she.Helper = ev;
                            // Insert the right Edge from Event
                            statusAndHelper.Add(new StatusHelperElement(sweepDown ? ev.EdgeTwo : ev.EdgeOne, ev));
                        }
                        break;
                }
            }
            return diagonals;
        }

        /// <summary>
        /// Split Polygon into subpolagons using the calculated Diagonals
        /// </summary>
        /// <param name="poly">The Base-Polygon</param>
        /// <param name="diagonals">The Split-Diagonals</param>
        /// <returns>List of Subpolygons</returns>
        private static List<PolygonData> SplitIntoPolygons(PolygonData poly, List<Tuple<int, int>> diagonals)
        {
            if (diagonals.Count == 0)
            {
                return new List<PolygonData>() { poly };
            }

            diagonals = diagonals.OrderBy(d => d.Item1).ThenBy(d => d.Item2).ToList();
            SortedDictionary<int, List<PolygonEdge>> edges = new();
            foreach (PolygonEdge edge in poly.Points.Select(p => p.EdgeTwo)
                .Union(diagonals.Select(d => new PolygonEdge(poly.Points[d.Item1], poly.Points[d.Item2])))
                .Union(diagonals.Select(d => new PolygonEdge(poly.Points[d.Item2], poly.Points[d.Item1]))))
            {
                if (!edges.ContainsKey(edge.PointOne.Index))
                {
                    edges.Add(edge.PointOne.Index, new List<PolygonEdge>() { edge });
                }
                else
                {
                    edges[edge.PointOne.Index].Add(edge);
                }
            }

            List<PolygonData> subPolygons = new();

            int cnt = 0;
            foreach (KeyValuePair<int, List<PolygonEdge>> edge in edges)
            {
                cnt += edge.Value.Count;
            }

            // For each Diagonal
            while (edges.Count > 0)
            {
                // Start at first Diagonal Point
                PolygonPoint currentPoint = edges.First().Value.First().PointOne;
                PolygonEdge nextEdge = new(null, null);
                List<PolygonPoint> subPolygonPoints = new();
                // March along the edges to form a monotone Polygon
                // Until the current Point equals the StartPoint
                do
                {
                    // Add the current Point
                    subPolygonPoints.Add(currentPoint);
                    // Select the next Edge
                    List<PolygonEdge> possibleEdges = edges[currentPoint.Index].ToList();
                    nextEdge = BestEdge(currentPoint, nextEdge, possibleEdges);
                    // Remove Edge from possible Edges
                    _ = edges[currentPoint.Index].Remove(nextEdge);
                    if (edges[currentPoint.Index].Count == 0)
                    {
                        _ = edges.Remove(currentPoint.Index);
                    }

                    // Move to the next Point
                    currentPoint = nextEdge.PointTwo;
                }
                while (subPolygonPoints[0].Index != currentPoint.Index);
                // Add the new SubPolygon
                subPolygons.Add(new PolygonData(subPolygonPoints));
            }

            return subPolygons;
        }

        /// <summary>
        /// For a Point, last used Edge and possible Edges, retrieve the best next Edge
        /// </summary>
        /// <param name="point">The current Point</param>
        /// <param name="lastEdge">The last used Edge</param>
        /// <param name="possibleEdges">The possible next Edges</param>
        /// <returns>Best next Edge</returns>
        internal static PolygonEdge BestEdge(PolygonPoint point, PolygonEdge lastEdge, List<PolygonEdge> possibleEdges)
        {
            // If just Starting, return the first possible Edge of the Point
            // If only one possibility, return that
            if ((lastEdge.PointOne == null && lastEdge.PointTwo == null) || possibleEdges.Count == 1)
            {
                return possibleEdges[0];
            }

            // Variables needed to determine the next Edge
            PolygonEdge bestEdge = possibleEdges[0];
            float bestAngle = (float)Math.PI * 2;
            // Vector from last Point to current Point
            Vector lastVector = lastEdge.PointTwo.Point - lastEdge.PointOne.Point;
            lastVector.Normalize();
            // Using CCW Point Order, so the left Vector always points towards the Polygon Center
            Point insideVector = new(-lastVector.Y, lastVector.X);
            // Check all possible Edges
            foreach (PolygonEdge possibleEdge in possibleEdges)
            {
                // Next Edge Vector
                Vector edgeVector = possibleEdge.PointTwo.Point - possibleEdge.PointOne.Point;
                edgeVector.Normalize();
                // Dot determines if the Vector also points towards the Polygon Center or not (> 0, yes, < 0, no)
                double dot = (insideVector.X * edgeVector.X) + (insideVector.Y * edgeVector.Y);
                // Cos represents the Angle between the last Edge and the next Edge
                double cos = (lastVector.X * edgeVector.X) + (lastVector.Y * edgeVector.Y);
                float angle = (insideVector.X * edgeVector.X) + (insideVector.Y * edgeVector.Y) > 0
                    ? (float)Math.PI - (float)Math.Acos(cos)
                    : (float)Math.PI + (float)Math.Acos(cos);
                // Depending on the Dot-Value, calculate the actual "inner" Angle
                // Replace the old Values if a better Edge was found
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestEdge = possibleEdge;
                }
            }
            return bestEdge;
        }

        /// <summary>
        /// Calculates the Orientation of a Polygon by usings it's (double-) Area as an Indicator.
        /// </summary>
        /// <param name="polygon">The Polygon.</param>
        /// <returns>True if the Polygon is present in a CCW manner.</returns>
        internal static bool IsCCW(IList<Point> polygon)
        {
            int n = polygon.Count;
            double area = 0.0;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                area += (polygon[p].X * polygon[q].Y) - (polygon[q].X * polygon[p].Y);
            }
            return area > 0.0f;
        }
    }

    /// <summary>
    /// Enumeration of PolygonPoint - Classes
    /// </summary>
    internal enum PolygonPointClass : byte
    {
        Start,
        Stop,
        Split,
        Merge,
        Regular
    }

    /// <summary>
    /// Helper Class that is used in the calculation Process of the Diagonals.
    /// </summary>
    internal class StatusHelper
    {
        /// <summary>
        /// List of StatusHelperElements that are currently present at the Sweeper's Position
        /// </summary>
        internal List<StatusHelperElement> EdgesHelpers { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        internal StatusHelper()
        {
            this.EdgesHelpers = new List<StatusHelperElement>();
        }

        /// <summary>
        /// Adds a StatusHelperElement to the List
        /// </summary>
        /// <param name="element"></param>
        internal void Add(StatusHelperElement element)
        {
            this.EdgesHelpers.Add(element);
        }

        /// <summary>
        /// Removes all StatusHelperElements with a specific Edge
        /// </summary>
        /// <param name="edge"></param>
        internal void Remove(PolygonEdge edge)
        {
            _ = this.EdgesHelpers.RemoveAll(she => she.Edge == edge);
        }

        /// <summary>
        /// Searches the nearest StatusHelperElement from the given Point
        /// </summary>
        /// <param name="point">The Point to search a StatusHelperElement for</param>
        /// <returns>The nearest StatusHelperElement that is positioned left of the Poin</returns>
        internal StatusHelperElement SearchLeft(PolygonPoint point)
        {
            // The found StatusHelperElement and the Distance Variables
            StatusHelperElement result = null;
            double dist = DoubleOrSingle.PositiveInfinity;

            double px = point.X;
            double py = point.Y;
            // Search for the right StatusHelperElement
            foreach (StatusHelperElement she in this.EdgesHelpers)
            {
                // No need to calculate the X-Value
                if (she.MinX > px)
                {
                    continue;
                }

                // Calculate the x-Coordinate of the Intersection between
                // a horizontal Line from the Point to the Left and the Edge of the StatusHelperElement
                double xValue = she.Edge.PointOne.X + ((py - she.Edge.PointOne.Y) * she.Factor);

                // If the xValue is smaller than or equal to the Point's x-Coordinate
                // (i.e. it lies on the left Side of it - allows a small Error)
                if (xValue <= px + SweepLinePolygonTriangulator.Epsilon)
                {
                    // Calculate the Distance
                    double sheDist = px - xValue;

                    // Update, if the Distance is smaller than a previously found Result
                    if (sheDist < dist)
                    {
                        dist = sheDist;
                        result = she;
                    }
                }
            }

            // Return the nearest found StatusHelperElement
            return result;
        }
    }

    /// <summary>
    /// Helper Class that is used in the calculation Process of the Diagonals.
    /// </summary>
    internal class StatusHelperElement
    {
        /// <summary>
        /// The Edge of the StatusHelperElement
        /// </summary>
        public PolygonEdge Edge { get; set; }

        /// <summary>
        /// The Helper of the Edge is a Polygon Point
        /// </summary>
        public PolygonPoint Helper { get; set; }

        /// <summary>
        /// Accessor for the Factor
        /// </summary>
        public double Factor { get; }

        /// <summary>
        /// Used to early-skip the Search for the right Status and Helper
        /// </summary>
        public double MinX
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor taking an Edge and a Helper
        /// </summary>
        /// <param name="edge">The Edge of the StatusHelperElement</param>
        /// <param name="point">The Helper for the Edge of the StatusHelperElement</param>
        internal StatusHelperElement(PolygonEdge edge, PolygonPoint point)
        {
            this.Edge = edge;
            this.Helper = point;
            Vector vector = edge.PointTwo.Point - edge.PointOne.Point;
            this.Factor = vector.X / vector.Y;
            this.MinX = Math.Min(edge.PointOne.X, edge.PointTwo.X);
        }
    }

    /// <summary>
    /// Helper Class for the PolygonData Object.
    /// </summary>
    internal class PolygonPoint : IComparable<PolygonPoint>
    {
        /// <summary>
        /// The actual Point of this PolygonPoint
        /// </summary>
        private Point mPoint;

        /// <summary>
        /// Accessor for the Point-Data
        /// </summary>
        public Point Point
        {
            get => this.mPoint;
            set => this.mPoint = value;
        }

        /// <summary>
        /// Accessor for the X-Coordinate of the Point
        /// </summary>
        public DoubleOrSingle X { get => this.mPoint.X; set => this.mPoint.X = value; }

        /// <summary>
        /// Accessor for the Y-Coordinate of the Point
        /// </summary>
        public DoubleOrSingle Y { get => this.mPoint.Y; set => this.mPoint.Y = value; }

        /// <summary>
        /// Accessor for the incoming Edge
        /// </summary>
        public PolygonEdge EdgeOne { get; set; }

        /// <summary>
        /// Accessor for the outgoing Edge
        /// </summary>
        public PolygonEdge EdgeTwo { get; set; }

        /// <summary>
        /// Accessor for the iriginal Point-Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The "last" neighboring Point, which is connected throught the incoming Edge
        /// </summary>
        public PolygonPoint Last => this.EdgeOne != null && this.EdgeOne.PointOne != null ? this.EdgeOne.PointOne : null;

        /// <summary>
        /// The "next" neighboring Point, which is connected throught the outgoing Edge
        /// </summary>
        public PolygonPoint Next => this.EdgeTwo != null && this.EdgeTwo.PointTwo != null ? this.EdgeTwo.PointTwo : null;

        /// <summary>
        /// Comparison Operator, that is used to determine the Class of the PolygonPoints
        /// </summary>
        /// <param name="first">The first PolygonPoint</param>
        /// <param name="second">The second PolygonPoint</param>
        /// <returns>Returns true if the first PolygonPoint is smaller, compared to the second PolygonPoint, false otherwise</returns>
        public static bool operator <(PolygonPoint first, PolygonPoint second)
        {
            return first.CompareTo(second) == 1;
        }

        /// <summary>
        /// Comparison Operator, that is used to determine the Class of the PolygonPoints
        /// </summary>
        /// <param name="first">The first PolygonPoint</param>
        /// <param name="second">The second PolygonPoint</param>
        /// <returns>Returns true if the first PolygonPoint is bigger, compared to the second PolygonPoint, false otherwise</returns>
        public static bool operator >(PolygonPoint first, PolygonPoint second)
        {
            return first.CompareTo(second) == -1;
        }

        /// <summary>
        /// Constructor using a Point
        /// </summary>
        /// <param name="p">The Point-Data to use</param>
        internal PolygonPoint(Point p)
        {
            // Set the Point-Data, the Index must be set later
            this.mPoint = p;
            this.Index = -1;
        }

        /// <summary>
        /// Detrmines the Class of the PolygonPoint, depending on the sweeping Direction
        /// </summary>
        /// <param name="reverse">The Sweeping direction, top-to-bottom if false, bottom-to-top otherwise</param>
        /// <returns>The Class of the PolygonPoint</returns>
        internal PolygonPointClass PointClass(bool reverse = false)
        {
            // If the Point has no Next- and Last-PolygonPoint, there's an Error
            if (this.Next == null || this.Last == null)
            {
                throw new Exception("No closed Polygon");
            }

            // If we use the normal Order (top-to-bottom)
            if (!reverse)
            {
                // Both neighboring PolygonPoints are below this Point and the Point is concave
                return this.Last < this && this.Next < this && this.isConvexPoint()
                    ? PolygonPointClass.Start
                    : this.Last > this && this.Next > this && this.isConvexPoint()
                    ? PolygonPointClass.Stop
                    : this.Last < this && this.Next < this
                    ? PolygonPointClass.Split
                    : this.Last > this && this.Next > this ? PolygonPointClass.Merge : PolygonPointClass.Regular;
            }
            else
            {
                // Both neighboring PolygonPoints are below this Point and the Point is concave
                return this.Last < this && this.Next < this && this.isConvexPoint()
                    ? PolygonPointClass.Stop
                    : this.Last > this && this.Next > this && this.isConvexPoint()
                    ? PolygonPointClass.Start
                    : this.Last < this && this.Next < this
                    ? PolygonPointClass.Merge
                    : this.Last > this && this.Next > this ? PolygonPointClass.Split : PolygonPointClass.Regular;
            }
        }

        /// <summary>
        /// Calculates for a Point, if it is a convex Point or not
        /// (the assumption is, that we are dealing with a CCW Polygon orientation!)
        /// </summary>
        /// <returns>Returns true, if convex, false if concave (or "reflex" Vertex)</returns>
        private bool isConvexPoint()
        {
            // If the Point has no Next- and Last-PolygonPoint, there's an Error
            if (this.Next == null || this.Last == null)
            {
                throw new Exception("No closed Polygon");
            }
            // Calculate the necessary Vectors
            // From-last-to-this Vector
            Vector vecFromLast = this.Point - this.Last.Point;
            vecFromLast.Normalize();
            // "Left" Vector (pointing "inward")
            Point vecLeft = new(-vecFromLast.Y, vecFromLast.X);
            // From-this-to-next Vector
            Vector vecToNext = this.Next.Point - this.Point;
            vecToNext.Normalize();
            // If the next Vector is pointing to the left Vector's direction,
            // the current Point is a convex Point (Dot-Product bigger than 0)
            return (vecLeft.X * vecToNext.X) + (vecLeft.Y * vecToNext.Y) >= 0;
        }

        /// <summary>
        /// Override the ToString (for Debugging Purposes)
        /// </summary>
        /// <returns>String representing this Point</returns>
        public override string ToString()
        {
            return this.Index + " X:" + this.X + " Y:" + this.Y;
        }

        /// <summary>
        /// Comparison of two Points, used to sort the Polygons from top to bottom (left to right)
        /// </summary>
        /// <param name="second">Other Point to compare to</param>
        /// <returns>-1 if this Point is bigger, 0 if the same, 1 if smaller</returns>
        public int CompareTo(PolygonPoint second)
        {
            return this == null || second == null
                ? 0
                : this.Y > second.Y || (this.Y == second.Y && this.X < second.X) ? -1 : this.Y == second.Y && this.X == second.X ? 0 : 1;
        }
    }

    /// <summary>
    /// Helper Class for the PolygonData Object.
    /// </summary>
    internal class PolygonEdge
    {
        /// <summary>
        /// Accessor to the Startpoint of this Edge
        /// </summary>
        public PolygonPoint PointOne { get; set; }

        /// <summary>
        /// Accessor to the Endpoint of this Edge
        /// </summary>
        public PolygonPoint PointTwo { get; set; }

        /// <summary>
        /// The "last" neighboring Edge, which both share the Startpoint of this Edge
        /// </summary>
        public PolygonEdge Last => this.PointOne != null && this.PointOne.EdgeOne != null ? this.PointOne.EdgeOne : null;

        /// <summary>
        /// The "next" neighboring Edge, which both share the Endpoint of this Edge
        /// </summary>
        public PolygonEdge Next => this.PointTwo != null && this.PointTwo.EdgeTwo != null ? this.PointTwo.EdgeTwo : null;

        /// <summary>
        /// Constructor that takes both Points of the Edge
        /// </summary>
        /// <param name="one">The Startpoint</param>
        /// <param name="two">The Endpoint</param>
        internal PolygonEdge(PolygonPoint one, PolygonPoint two)
        {
            this.PointOne = one;
            this.PointTwo = two;
        }

        /// <summary>
        /// Override the ToString (for Debugging Purposes)
        /// </summary>
        /// <returns>String representing this Edge</returns>
        public override string ToString()
        {
            return "From: {" + this.PointOne + "} To: {" + this.PointTwo + "}";
        }
    }

    /// <summary>
    /// Helper Class for the Polygon-Triangulation.
    /// </summary>
    internal class PolygonData
    {
        /// <summary>
        /// Accessor to the List of PolygonPoints
        /// </summary>
        public List<PolygonPoint> Points { get; set; }

        /// <summary>
        /// Are there Holes present
        /// </summary>
        public bool HasHoles => this.Holes.Count > 0;

        /// <summary>
        /// Access to the Holes
        /// </summary>
        public List<List<PolygonPoint>> Holes { get; }

        /// <summary>
        /// Number of initial Points on the Polygon Boundary
        /// </summary>
        private readonly int mNumBoundaryPoints;

        /// <summary>
        /// Constructor that uses a List of Points and an optional List of Point-Indices
        /// </summary>
        /// <param name="points">The Polygon-Defining Points</param>
        /// <param name="indices">Optional List of Point-Indices</param>
        public PolygonData(List<Point> points, List<int> indices = null)
        {
            // Initialize
            this.Points = new List<PolygonPoint>(points.Select(p => new PolygonPoint(p)));
            this.Holes = new List<List<PolygonPoint>>();
            this.mNumBoundaryPoints = this.Points.Count;

            // If no Indices were specified, add them manually
            if (indices == null)
            {
                for (int i = 0; i < this.Points.Count; i++)
                {
                    this.Points[i].Index = i;
                }
            }
            // If there were Indices specified, use them to set the PolygonPoint's Index Property
            else
            {
                for (int i = 0; i < this.Points.Count; i++)
                {
                    this.Points[i].Index = indices[i];
                }
            }

            // Add Edges between the Points (to be able to navigate along the Polygon easily later)
            int cnt = this.Points.Count;
            for (int i = 0; i < cnt; i++)
            {
                int lastIdx = (i + cnt - 1) % cnt;
                PolygonEdge edge = new(this.Points[lastIdx], this.Points[i]);
                this.Points[lastIdx].EdgeTwo = edge;
                this.Points[i].EdgeOne = edge;
            }
        }

        /// <summary>
        /// Constructor that takes a List of PolygonPoints
        /// Calls the first Constructor by splitting the Input-Information (Points and Indices)
        /// </summary>
        /// <param name="points">The PolygonPoints</param>
        public PolygonData(List<PolygonPoint> points)
            : this(points.Select(p => p.Point).ToList(), points.Select(p => p.Index).ToList())
        { }

        /// <summary>
        /// Add Points of a Hole to the PolygonData
        /// </summary>
        /// <param name="points">The Points that define the Hole in the Polygon</param>
        internal void AddHole(List<Point> points)
        {
            // Make Hole Clockwise
            if (SweepLinePolygonTriangulator.IsCCW(points))
            {
                points.Reverse();
            }
            // The Hole Points
            List<PolygonPoint> polyPoints = points.Select(p => new PolygonPoint(p)).ToList();
            // If Endpoint equals Startpoint
            if (polyPoints[0].Equals(polyPoints[polyPoints.Count - 1]))
                polyPoints.RemoveAt(polyPoints.Count - 1);
            this.Holes.Add(polyPoints);

            int cntBefore = this.Points.Count;
            int pointCount = points.Count;
            // Add the PolygonPoints for this Polygon Object
            this.Points.AddRange(polyPoints);

            // Add the Indices
            for (int i = cntBefore; i < this.Points.Count; i++)
            {
                polyPoints[i - cntBefore].Index = i;
            }

            // Add Edges between the Points (to be able to navigate along the Polygon easily later)
            int cnt = this.Points.Count;
            for (int i = 0; i < pointCount; i++)
            {
                int lastIdx = (i + pointCount - 1) % pointCount;
                PolygonEdge edge = new(polyPoints[lastIdx], polyPoints[i]);
                polyPoints[lastIdx].EdgeTwo = edge;
                polyPoints[i].EdgeOne = edge;
            }
        }
    }
}
