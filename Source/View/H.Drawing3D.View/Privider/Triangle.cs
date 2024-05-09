// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Triangle.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Represents a triangle in two-dimensional space.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows;

namespace H.Drawing3D.View.Privider
{
    /// <summary>
    /// Represents a triangle in two-dimensional space.
    /// </summary>
    public class Triangle
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        /// <param name="a">The first point of the triangle.</param>
        /// <param name="b">The second point of the triangle.</param>
        /// <param name="c">The third point of the triangle.</param>
        public Triangle(Point a, Point b, Point c)
        {
            this.P1 = a;
            this.P2 = b;
            this.P3 = c;
        }

        /// <summary>
        /// Gets the first point of the triangle.
        /// </summary>
        /// <value>The point.</value>
        public Point P1 { get; }

        /// <summary>
        /// Gets the second point of the triangle.
        /// </summary>
        /// <value>The point.</value>
        public Point P2 { get; }

        /// <summary>
        /// Gets the third point of the triangle.
        /// </summary>
        /// <value>The point.</value>
        public Point P3 { get; }

        /// <summary>
        /// Checks whether the specified rectangle is completely inside the current triangle.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <returns>
        /// <c>true</c> if the specified rectangle is inside the current triangle; otherwise <c>false</c>.
        /// </returns>
        public bool IsCompletelyInside(Rect rect)
        {
            return rect.Contains(this.P2) && rect.Contains(this.P3) && rect.Contains(this.P3);
        }

        /// <summary>
        /// Checks whether the specified rectangle is completely inside the current triangle.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>
        /// <c>true</c> if the specified rectangle is inside the current triangle; otherwise <c>false</c>.
        /// </returns>
        public bool IsRectCompletelyInside(Rect rect)
        {
            return this.IsPointInside(rect.TopLeft) && this.IsPointInside(rect.TopRight)
                   && this.IsPointInside(rect.BottomLeft) && this.IsPointInside(rect.BottomRight);
        }

        /// <summary>
        /// Checks whether the specified point is inside the triangle. 
        /// </summary>
        /// <param name="p">The point to be checked.</param>
        /// <returns>
        /// <c>true</c> if the specified point is inside the current triangle; otherwise <c>false</c>.
        /// </returns>
        public bool IsPointInside(Point p)
        {
            // http://stackoverflow.com/questions/2049582/how-to-determine-a-point-in-a-triangle
            double s = (this.P1.Y * this.P3.X) - (this.P1.X * this.P3.Y) + ((this.P3.Y - this.P1.Y) * p.X) + ((this.P1.X - this.P3.X) * p.Y);
            double t = (this.P1.X * this.P2.Y) - (this.P1.Y * this.P2.X) + ((this.P1.Y - this.P2.Y) * p.X) + ((this.P2.X - this.P1.X) * p.Y);

            if ((s < 0) != (t < 0))
            {
                return false;
            }

            double a = (-this.P2.Y * this.P3.X) + (this.P1.Y * (this.P3.X - this.P2.X)) + (this.P1.X * (this.P2.Y - this.P3.Y)) + (this.P2.X * this.P3.Y);
            if (a < 0.0)
            {
                s = -s;
                t = -t;
                a = -a;
            }

            return s > 0 && t > 0 && s + t < a;
        }

        /// <summary>
        /// Indicates whether the specified rectangle intersects with the current triangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns>
        /// <c>true</c> if the specified rectangle intersects with the current triangle; otherwise <c>false</c>.
        /// </returns>
        public bool IntersectsWith(Rect rect)
        {
            return LineSegment.AreLineSegmentsIntersecting(this.P1, this.P2, rect.BottomLeft, rect.BottomRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P1, this.P2, rect.BottomLeft, rect.TopLeft)
                   || LineSegment.AreLineSegmentsIntersecting(this.P1, this.P2, rect.TopLeft, rect.TopRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P1, this.P2, rect.TopRight, rect.BottomRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P2, this.P3, rect.BottomLeft, rect.BottomRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P2, this.P3, rect.BottomLeft, rect.TopLeft)
                   || LineSegment.AreLineSegmentsIntersecting(this.P2, this.P3, rect.TopLeft, rect.TopRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P2, this.P3, rect.TopRight, rect.BottomRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P3, this.P1, rect.BottomLeft, rect.BottomRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P3, this.P1, rect.BottomLeft, rect.TopLeft)
                   || LineSegment.AreLineSegmentsIntersecting(this.P3, this.P1, rect.TopLeft, rect.TopRight)
                   || LineSegment.AreLineSegmentsIntersecting(this.P3, this.P1, rect.TopRight, rect.BottomRight);
        }
    }
}