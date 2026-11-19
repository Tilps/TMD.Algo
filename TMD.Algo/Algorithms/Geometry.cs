#region License

/*
Copyright (c) 2008, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Represents an immutable point in two-dimensional Euclidean space.
    /// </summary>
    public readonly struct Point2D : IEquatable<Point2D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the Y coordinate.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Computes the squared Euclidean distance between this point and another point.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>The squared Euclidean distance.</returns>
        public double DistanceSquaredTo(Point2D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Computes the Euclidean distance between this point and another point.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>The Euclidean distance.</returns>
        public double DistanceTo(Point2D other) => Math.Sqrt(DistanceSquaredTo(other));

        /// <inheritdoc />
        public bool Equals(Point2D other) => X.Equals(other.X) && Y.Equals(other.Y);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Point2D other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Determines whether two <see cref="Point2D"/> instances have equal coordinates.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns>True if both coordinates are equal; otherwise, false.</returns>
        public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="Point2D"/> instances do not have equal coordinates.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns>True if coordinates differ; otherwise, false.</returns>
        public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() =>
            $"({X.ToString(CultureInfo.InvariantCulture)}, {Y.ToString(CultureInfo.InvariantCulture)})";
    }

    /// <summary>
    /// Provides geometry methods.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Gets the cross product of two vectors.
        /// </summary>
        /// <param name="x1">
        /// X component of first vector.
        /// </param>
        /// <param name="y1">
        /// Y component of first vector.
        /// </param>
        /// <param name="x2">
        /// X component of Second vector.
        /// </param>
        /// <param name="y2">
        /// Y component of Second vector.
        /// </param>
        /// <returns>
        /// The signed cross product of vector 1 against vector 2.
        /// </returns>
        public static long CrossProduct(long x1, long y1, long x2, long y2)
        {
            return x1*y2 - x2*y1;
        }

        // right hand rule - two consecutive segments turn clockwise if seg1 cross seg2 is negative.


        /// <summary>
        /// Given a set of points, determines the convex hull which contains them.
        /// </summary>
        /// <param name="points">
        /// Points to construct hull for.
        /// </param>
        /// <returns>
        /// The list of points in the convex hull.
        /// </returns>
        public static List<KeyValuePair<long, long>> ConvexHull(List<KeyValuePair<long, long>> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            if (points.Count < 3)
                throw new ArgumentException("Points were insufficient to form a polygon", nameof(points));

            KeyValuePair<long, long> p0 = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].Value < p0.Value)
                    p0 = points[i];
                else if (points[i].Value == p0.Value && points[i].Key < p0.Key)
                    p0 = points[i];
            }
            List<KeyValuePair<long, long>> otherPoints = [with(capacity: points.Count)];
            foreach (KeyValuePair<long, long> point in points)
            {
                if (point.Key != p0.Key || point.Value != p0.Value)
                    otherPoints.Add(point);
            }
            AngleComparer comparer = new AngleComparer(p0);
            otherPoints.Sort(comparer);
            List<KeyValuePair<long, long>> equalityFilteredPoints = new List<KeyValuePair<long, long>>();
            KeyValuePair<long, long> cur = otherPoints[0];
            long curSqDist = SqDist(cur.Key, cur.Value, p0.Key, p0.Value);
            for (int i = 1; i < otherPoints.Count; i++)
            {
                if (comparer.Compare(cur, otherPoints[i]) != 0)
                {
                    equalityFilteredPoints.Add(cur);
                    cur = otherPoints[i];
                }
                else
                {
                    long nextDist = SqDist(otherPoints[i].Key, otherPoints[i].Value, p0.Key, p0.Value);
                    if (nextDist > curSqDist)
                    {
                        cur = otherPoints[i];
                        curSqDist = nextDist;
                    }
                }
            }
            equalityFilteredPoints.Add(cur);
            if (equalityFilteredPoints.Count < 2)
                throw new ArgumentException("Points were colinear and/or coincident.", nameof(points));

            List<KeyValuePair<long, long>> stack = new List<KeyValuePair<long, long>>
            {
                p0,
                equalityFilteredPoints[0],
                equalityFilteredPoints[1]
            };
            for (int i = 2; i < equalityFilteredPoints.Count; i++)
            {
                KeyValuePair<long, long> top = stack[stack.Count - 1];
                KeyValuePair<long, long> nextToTop = stack[stack.Count - 2];
                KeyValuePair<long, long> trial = equalityFilteredPoints[i];
                while (
                    CrossProduct(top.Key - nextToTop.Key, top.Value - nextToTop.Value, trial.Key - top.Key,
                        trial.Value - top.Value) < 0)
                {
                    stack.RemoveAt(stack.Count - 1);
                    top = stack[stack.Count - 1];
                    nextToTop = stack[stack.Count - 2];
                }
                stack.Add(trial);
            }
            return stack;
        }

        private static long SqDist(long x1, long y1, long x2, long y2)
        {
            long dx = x1 - x2;
            long dy = y1 - y2;
            return dx*dx + dy*dy;
        }

        private const double GeometryEpsilon = 1e-12;

        private static double SignedPolygonArea(IReadOnlyList<Point2D> polygon)
        {
            if (polygon.Count < 3)
                return 0.0;

            double sum = 0.0;
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2D a = polygon[i];
                Point2D b = polygon[(i + 1) % polygon.Count];
                sum += a.X * b.Y - a.Y * b.X;
            }
            return 0.5 * sum;
        }

        private static double Cross(Point2D a, Point2D b, Point2D c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        /// <summary>
        /// Computes the area of a simple polygon using the shoelace formula.
        /// </summary>
        /// <param name="polygon">Ordered polygon vertices.</param>
        /// <returns>The non-negative area of the polygon.</returns>
        public static double PolygonArea(IReadOnlyList<Point2D> polygon)
        {
            ArgumentNullException.ThrowIfNull(polygon);
            return Math.Abs(SignedPolygonArea(polygon));
        }

        /// <summary>
        /// Computes the intersection polygon of two convex polygons using the Sutherland-Hodgman algorithm.
        /// </summary>
        /// <param name="subjectPolygon">Vertices of the subject convex polygon.</param>
        /// <param name="clipPolygon">Vertices of the clipping convex polygon.</param>
        /// <returns>Vertices of the intersection polygon, or an empty list if the intersection has negligible area.</returns>
        public static List<Point2D> ConvexPolygonIntersection(
            IReadOnlyList<Point2D> subjectPolygon,
            IReadOnlyList<Point2D> clipPolygon)
        {
            ArgumentNullException.ThrowIfNull(subjectPolygon);
            ArgumentNullException.ThrowIfNull(clipPolygon);

            if (subjectPolygon.Count < 3 || clipPolygon.Count < 3)
                return [];

            double clipOrientation = SignedPolygonArea(clipPolygon) >= 0.0 ? 1.0 : -1.0;
            List<Point2D> output = [.. subjectPolygon];
            List<Point2D> input = [];

            for (int e = 0; e < clipPolygon.Count && output.Count > 0; e++)
            {
                Point2D clipA = clipPolygon[e];
                Point2D clipB = clipPolygon[(e + 1) % clipPolygon.Count];

                (input, output) = (output, input);
                output.Clear();

                Point2D prev = input[^1];
                double prevSide = clipOrientation * Cross(clipA, clipB, prev);
                bool prevInside = prevSide >= -GeometryEpsilon;

                for (int i = 0; i < input.Count; i++)
                {
                    Point2D curr = input[i];
                    double currSide = clipOrientation * Cross(clipA, clipB, curr);
                    bool currInside = currSide >= -GeometryEpsilon;

                    if (currInside)
                    {
                        if (!prevInside)
                        {
                            double denom = prevSide - currSide;
                            if (Math.Abs(denom) > GeometryEpsilon)
                            {
                                double t = prevSide / denom;
                                output.Add(new Point2D(
                                    prev.X + t * (curr.X - prev.X),
                                    prev.Y + t * (curr.Y - prev.Y)));
                            }
                        }
                        output.Add(curr);
                    }
                    else if (prevInside)
                    {
                        double denom = prevSide - currSide;
                        if (Math.Abs(denom) > GeometryEpsilon)
                        {
                            double t = prevSide / denom;
                            output.Add(new Point2D(
                                prev.X + t * (curr.X - prev.X),
                                prev.Y + t * (curr.Y - prev.Y)));
                        }
                    }

                    prev = curr;
                    prevSide = currSide;
                    prevInside = currInside;
                }
            }

            if (output.Count < 3)
                return [];

            // Remove near-duplicate consecutive vertices.
            const double epsSq = 1e-22;
            List<Point2D> deduped = [with(capacity: output.Count)];
            foreach (Point2D p in output)
            {
                if (deduped.Count == 0 || deduped[^1].DistanceSquaredTo(p) > epsSq)
                {
                    deduped.Add(p);
                }
            }
            if (deduped.Count > 1 && deduped[0].DistanceSquaredTo(deduped[^1]) <= epsSq)
            {
                deduped.RemoveAt(deduped.Count - 1);
            }

            if (deduped.Count < 3 || PolygonArea(deduped) <= 1e-14)
                return [];

            return deduped;
        }

        /// <summary>
        /// Computes the area of intersection between a disk and a simple polygon.
        /// </summary>
        /// <param name="center">Center of the circle.</param>
        /// <param name="radius">Radius of the circle (must be non-negative).</param>
        /// <param name="polygon">Ordered vertices of the simple polygon.</param>
        /// <returns>The non-negative area inside both the circle and the polygon.</returns>
        public static double CirclePolygonIntersectionArea(
            Point2D center,
            double radius,
            IReadOnlyList<Point2D> polygon)
        {
            ArgumentNullException.ThrowIfNull(polygon);
            ArgumentOutOfRangeException.ThrowIfNegative(radius);

            if (radius == 0.0 || polygon.Count < 3)
                return 0.0;

            double r2 = radius * radius;
            double signedSum = 0.0;
            Span<double> cuts = stackalloc double[4];

            for (int i = 0; i < polygon.Count; i++)
            {
                Point2D rawA = polygon[i];
                Point2D rawB = polygon[(i + 1) % polygon.Count];

                double ax = rawA.X - center.X;
                double ay = rawA.Y - center.Y;
                double bx = rawB.X - center.X;
                double by = rawB.Y - center.Y;

                double dx = bx - ax;
                double dy = by - ay;
                double a = dx * dx + dy * dy;
                if (a <= 0.0)
                    continue;

                double b = ax * dx + ay * dy;
                double c = ax * ax + ay * ay - r2;
                double disc = b * b - a * c;

                int cutCount = 2;
                cuts[0] = 0.0;
                cuts[1] = 1.0;

                if (disc > 0.0)
                {
                    double sqrtD = Math.Sqrt(disc);
                    double t1 = (-b - sqrtD) / a;
                    double t2 = (-b + sqrtD) / a;
                    if (t1 > GeometryEpsilon && t1 < 1.0 - GeometryEpsilon)
                        cuts[cutCount++] = t1;
                    if (t2 > GeometryEpsilon && t2 < 1.0 - GeometryEpsilon && Math.Abs(t2 - t1) > GeometryEpsilon)
                        cuts[cutCount++] = t2;
                    cuts[..cutCount].Sort();
                }

                for (int s = 0; s + 1 < cutCount; s++)
                {
                    double ts = cuts[s];
                    double te = cuts[s + 1];
                    double px = ax + ts * dx;
                    double py = ay + ts * dy;
                    double qx = ax + te * dx;
                    double qy = ay + te * dy;
                    double mx = 0.5 * (px + qx);
                    double my = 0.5 * (py + qy);

                    double cross = px * qy - py * qx;
                    if (mx * mx + my * my <= r2)
                    {
                        signedSum += 0.5 * cross;
                    }
                    else
                    {
                        double dot = px * qx + py * qy;
                        signedSum += 0.5 * r2 * Math.Atan2(cross, dot);
                    }
                }
            }

            return Math.Abs(signedSum);
        }

        private static int CompareByX(Point2D a, Point2D b)
        {
            int cmp = a.X.CompareTo(b.X);
            return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
        }

        private static int CompareByY(Point2D a, Point2D b)
        {
            int cmp = a.Y.CompareTo(b.Y);
            return cmp != 0 ? cmp : a.X.CompareTo(b.X);
        }

        private static void PrepareSortedPoints(
            IReadOnlyList<Point2D> points,
            out Point2D[] byX,
            out Point2D[] levelBuffer,
            out Point2D[] strip)
        {
            int n = points.Count;
            byX = new Point2D[n];
            Point2D[] byY = new Point2D[n];
            for (int i = 0; i < n; i++)
            {
                Point2D p = points[i];
                byX[i] = p;
                byY[i] = p;
            }
            Array.Sort(byX, CompareByX);
            Array.Sort(byY, CompareByY);

            levelBuffer = new Point2D[3 * n + 64];
            Array.Copy(byY, 0, levelBuffer, 0, n);
            strip = new Point2D[n];
        }

        private static void PartitionByX(
            Point2D[] levelBuffer,
            int curStart,
            int len,
            Point2D split,
            int eqTargetInLeft,
            int leftOutStart,
            int rightOutStart)
        {
            int li = 0;
            int ri = 0;
            int eqInLeft = 0;
            for (int i = 0; i < len; i++)
            {
                Point2D p = levelBuffer[curStart + i];
                int cmp = CompareByX(p, split);
                if (cmp < 0)
                {
                    levelBuffer[leftOutStart + li++] = p;
                }
                else if (cmp == 0 && eqInLeft < eqTargetInLeft)
                {
                    eqInLeft++;
                    levelBuffer[leftOutStart + li++] = p;
                }
                else
                {
                    levelBuffer[rightOutStart + ri++] = p;
                }
            }
        }

        private static int CountEqualToSplitInLeft(Point2D[] byX, int l, int mid, Point2D split)
        {
            int idx = mid - 1;
            while (idx >= l && CompareByX(byX[idx], split) == 0)
                idx--;
            return (mid - 1) - idx;
        }

        /// <summary>
        /// Computes the minimum Euclidean distance between any pair of distinct points in <paramref name="points"/> in O(n log n) time.
        /// </summary>
        /// <param name="points">Collection of at least two points.</param>
        /// <returns>The smallest Euclidean distance between two points in the collection.</returns>
        public static double ClosestPairDistance(IReadOnlyList<Point2D> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            if (points.Count < 2)
                throw new ArgumentException("At least two points are required.", nameof(points));

            PrepareSortedPoints(points, out Point2D[] byX, out Point2D[] levelBuffer, out Point2D[] strip);
            for (int i = 1; i < byX.Length; i++)
            {
                if (byX[i].Equals(byX[i - 1]))
                    return 0.0;
            }

            double Solve(int l, int r, int curStart, int nextFree)
            {
                int len = r - l;
                if (len <= 32)
                {
                    double bestLocal = double.PositiveInfinity;
                    for (int i = l; i < r; i++)
                    {
                        for (int j = i + 1; j < r; j++)
                        {
                            double d = byX[i].DistanceTo(byX[j]);
                            if (d < bestLocal)
                                bestLocal = d;
                        }
                    }
                    return bestLocal;
                }

                int mid = l + (len >> 1);
                int leftLen = mid - l;
                int leftStart = nextFree;
                int rightStart = nextFree + leftLen;
                int childFree = nextFree + len;
                Point2D split = byX[mid];
                int eqTargetInLeft = CountEqualToSplitInLeft(byX, l, mid, split);

                PartitionByX(levelBuffer, curStart, len, split, eqTargetInLeft, leftStart, rightStart);

                double best = Solve(l, mid, leftStart, childFree);
                double bestRight = Solve(mid, r, rightStart, childFree);
                if (bestRight < best)
                    best = bestRight;

                double splitX = split.X;
                int stripCount = 0;
                for (int i = 0; i < len; i++)
                {
                    Point2D p = levelBuffer[curStart + i];
                    if (Math.Abs(p.X - splitX) < best)
                    {
                        strip[stripCount++] = p;
                    }
                }

                for (int i = 0; i < stripCount; i++)
                {
                    Point2D p = strip[i];
                    for (int j = i + 1; j < stripCount && strip[j].Y - p.Y < best; j++)
                    {
                        double d = p.DistanceTo(strip[j]);
                        if (d < best)
                            best = d;
                    }
                }

                return best;
            }

            return Solve(0, points.Count, 0, points.Count);
        }

        /// <summary>
        /// Computes the minimum perimeter of a triangle formed by three distinct points in <paramref name="points"/> in O(n log n) time.
        /// Degenerate (collinear) triangles are permitted.
        /// </summary>
        /// <param name="points">Collection of at least three points.</param>
        /// <returns>The smallest perimeter among all triples of distinct points.</returns>
        public static double MinimumTrianglePerimeter(IReadOnlyList<Point2D> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            if (points.Count < 3)
                throw new ArgumentException("At least three points are required.", nameof(points));

            PrepareSortedPoints(points, out Point2D[] byX, out Point2D[] levelBuffer, out Point2D[] strip);

            static double TrianglePerimeter(Point2D a, Point2D b, Point2D c) =>
                a.DistanceTo(b) + b.DistanceTo(c) + c.DistanceTo(a);

            double Solve(int l, int r, int curStart, int nextFree)
            {
                int len = r - l;
                if (len < 3)
                    return double.PositiveInfinity;

                if (len <= 32)
                {
                    double bestLocal = double.PositiveInfinity;
                    for (int i = l; i < r; i++)
                    {
                        for (int j = i + 1; j < r; j++)
                        {
                            double dij = byX[i].DistanceTo(byX[j]);
                            if (dij * 2.0 >= bestLocal)
                                continue;
                            for (int k = j + 1; k < r; k++)
                            {
                                double p = dij + byX[j].DistanceTo(byX[k]) + byX[i].DistanceTo(byX[k]);
                                if (p < bestLocal)
                                    bestLocal = p;
                            }
                        }
                    }
                    return bestLocal;
                }

                int mid = l + (len >> 1);
                int leftLen = mid - l;
                int leftStart = nextFree;
                int rightStart = nextFree + leftLen;
                int childFree = nextFree + len;
                Point2D split = byX[mid];
                int eqTargetInLeft = CountEqualToSplitInLeft(byX, l, mid, split);

                PartitionByX(levelBuffer, curStart, len, split, eqTargetInLeft, leftStart, rightStart);

                double best = Solve(l, mid, leftStart, childFree);
                double bestRight = Solve(mid, r, rightStart, childFree);
                if (bestRight < best)
                    best = bestRight;

                double halfBest = best * 0.5;
                double splitX = 0.5 * (byX[mid - 1].X + split.X);
                int stripCount = 0;
                int windowStart = 0;

                for (int idx = 0; idx < len; idx++)
                {
                    Point2D p = levelBuffer[curStart + idx];
                    if (Math.Abs(p.X - splitX) > halfBest)
                        continue;

                    while (windowStart < stripCount && p.Y - strip[windowStart].Y > halfBest)
                    {
                        windowStart++;
                    }

                    for (int i = windowStart; i < stripCount; i++)
                    {
                        double pi = p.DistanceTo(strip[i]);
                        if (pi >= best)
                            continue;
                        for (int j = i + 1; j < stripCount; j++)
                        {
                            double perim = TrianglePerimeter(p, strip[i], strip[j]);
                            if (perim < best)
                            {
                                best = perim;
                                halfBest = best * 0.5;
                            }
                        }
                    }

                    strip[stripCount++] = p;
                }

                return best;
            }

            return Solve(0, points.Count, 0, points.Count);
        }
    }

    /// <summary>
    /// Compares points by their rotation around another point.
    /// </summary>
    public class AngleComparer : IComparer<KeyValuePair<long, long>>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="centre">
        /// Point to compare angles relative to.
        /// </param>
        public AngleComparer(KeyValuePair<long, long> centre)
        {
            this.centre = centre;
        }

        private KeyValuePair<long, long> centre;

        #region IComparer<KeyValuePair<long,long>> Members

        /// <summary>
        /// Compares two points relative to the centre to see whether they are clockwise or anticlockwise from each other.
        /// </summary>
        /// <param name="x">
        /// First point.
        /// </param>
        /// <param name="y">
        /// Second points.
        /// </param>
        /// <returns>
        /// A positive number if x is anticlockwise from y, otherwise a negative number.
        /// </returns>
        public int Compare(KeyValuePair<long, long> x, KeyValuePair<long, long> y)
        {
            long res = Geometry.CrossProduct(x.Key - centre.Key, x.Value - centre.Value, y.Key - centre.Key,
                y.Value - centre.Value);
            if (res > 0)
                return -1;
            else if (res < 0)
                return 1;
            return 0;
        }

        #endregion
    }
}