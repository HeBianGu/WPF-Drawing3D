// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LineSegment.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Represents a line segment in two-dimensional space.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows;

namespace H.Drawing3D.View.Privider
{
    /// <summary>
    /// Represents a line segment in two-dimensional space.
    /// </summary>
    public class LineSegment
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment"/> class.
        /// </summary>
        /// <param name="p1">The first point of the line segment.</param>
        /// <param name="p2">The second point of the line segment.</param>
        public LineSegment(Point p1, Point p2)
        {
            this.P1 = p1;
            this.P2 = p2;
        }

        /// <summary>
        /// Gets the first point of the line segment.
        /// </summary>
        /// <value>The point.</value>
        public Point P1 { get; }

        /// <summary>
        /// Gets the second point of the line segment.
        /// </summary>
        /// <value>The point.</value>
        public Point P2 { get; }

        /// <summary>
        /// Checks if there are any intersections of two line segments.
        /// </summary>
        /// <param name="a1">One vertex of line a.</param>
        /// <param name="a2">The other vertex of the line a.</param>
        /// <param name="b1">One vertex of line b.</param>
        /// <param name="b2">The other vertex of the line b.</param>
        /// <returns>
        /// <c>true</c>, if the two lines are crossed. Otherwise, it returns <c>false</c>.
        /// </returns>
        public static bool AreLineSegmentsIntersecting(Point a1, Point a2, Point b1, Point b2)
        {
            return b1 != b2 && a1 != a2
&& (((a2.X - a1.X) * (b1.Y - a1.Y)) - ((b1.X - a1.X) * (a2.Y - a1.Y)))
                * (((a2.X - a1.X) * (b2.Y - a1.Y)) - ((b2.X - a1.X) * (a2.Y - a1.Y))) <= 0
&& (((b2.X - b1.X) * (a1.Y - b1.Y)) - ((a1.X - b1.X) * (b2.Y - b1.Y)))
                * (((b2.X - b1.X) * (a2.Y - b1.Y)) - ((a2.X - b1.X) * (b2.Y - b1.Y))) <= 0;
        }

        /// <summary>
        /// Indicates whether the specified line segment intersects with the current line segment.
        /// </summary>
        /// <param name="other">The line segment to check.</param>
        /// <returns>
        /// <c>true</c> if the specified line segment intersects with the current line segment; otherwise <c>false</c>.
        /// </returns>
        public bool IntersectsWith(LineSegment other)
        {
            return AreLineSegmentsIntersecting(this.P1, this.P2, other.P1, other.P2);
        }
    }
}