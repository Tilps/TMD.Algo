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
using System.Numerics;

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
    /// Represents an immutable point or vector with exact 64-bit integer coordinates.
    /// </summary>
    public readonly struct Point2L : IEquatable<Point2L>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point2L"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Point2L(long x, long y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public long X { get; }

        /// <summary>
        /// Gets the Y coordinate.
        /// </summary>
        public long Y { get; }

        /// <summary>
        /// Returns the exact squared Euclidean norm as a <see cref="BigInteger"/>.
        /// </summary>
        public BigInteger LengthSquared()
        {
            BigInteger bx = X;
            BigInteger by = Y;
            return bx * bx + by * by;
        }

        /// <summary>
        /// Returns the exact squared Euclidean norm as an <see cref="Int128"/>.
        /// </summary>
        [CLSCompliant(false)]
        public Int128 LengthSquared128() => (Int128)X * X + (Int128)Y * Y;

        /// <summary>
        /// Returns the Euclidean norm as a double without intermediate 64-bit integer overflow.
        /// </summary>
        public double Length() => Math.Sqrt((double)LengthSquared128());

        /// <summary>
        /// Returns the Euclidean distance from this point to <paramref name="other"/> without intermediate 64-bit integer overflow.
        /// </summary>
        /// <param name="other">The other point.</param>
        public double DistanceTo(Point2L other) => (this - other).Length();

        /// <summary>
        /// Adds two points/vectors coordinate-wise.
        /// </summary>
        public static Point2L operator +(Point2L left, Point2L right) => new(left.X + right.X, left.Y + right.Y);

        /// <summary>
        /// Subtracts <paramref name="right"/> from <paramref name="left"/> coordinate-wise.
        /// </summary>
        public static Point2L operator -(Point2L left, Point2L right) => new(left.X - right.X, left.Y - right.Y);

        /// <summary>
        /// Scales a vector by an integer scalar.
        /// </summary>
        public static Point2L operator *(Point2L point, long scalar) => new(point.X * scalar, point.Y * scalar);

        /// <summary>
        /// Scales a vector by an integer scalar.
        /// </summary>
        public static Point2L operator *(long scalar, Point2L point) => new(scalar * point.X, scalar * point.Y);

        /// <inheritdoc />
        public bool Equals(Point2L other) => X == other.X && Y == other.Y;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Point2L other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Determines whether two <see cref="Point2L"/> instances have equal coordinates.
        /// </summary>
        public static bool operator ==(Point2L left, Point2L right) => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="Point2L"/> instances do not have equal coordinates.
        /// </summary>
        public static bool operator !=(Point2L left, Point2L right) => !left.Equals(right);

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

        /// <summary>
        /// Computes the area of a circular segment of radius <paramref name="radius"/>
        /// subtended by a central angle of <paramref name="centralAngle"/> radians.
        /// </summary>
        /// <param name="radius">Non-negative radius of the circle.</param>
        /// <param name="centralAngle">Central angle in radians in [0, 2*pi].</param>
        /// <returns>The area of the circular segment.</returns>
        public static double CircularSegmentArea(double radius, double centralAngle)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(radius);
            if (centralAngle < -1e-12 || centralAngle > 2.0 * Math.PI + 1e-12)
                throw new ArgumentOutOfRangeException(nameof(centralAngle), "Central angle must be in [0, 2*pi].");

            double angle = Math.Clamp(centralAngle, 0.0, 2.0 * Math.PI);
            if (radius == 0.0 || angle == 0.0)
                return 0.0;

            if (angle < 1e-3)
            {
                double a2 = angle * angle;
                double diff = angle * a2 * (1.0 / 6.0 - a2 * (1.0 / 120.0 - a2 / 5040.0));
                return 0.5 * radius * radius * diff;
            }

            return 0.5 * radius * radius * (angle - Math.Sin(angle));
        }

        /// <summary>
        /// Computes the intersection area in O(N log N) time of disks centered at <paramref name="centers"/>,
        /// where each disk passes through <paramref name="commonPoint"/>.
        /// </summary>
        /// <param name="commonPoint">The point lying on the boundary of every disk.</param>
        /// <param name="centers">Centers of the disks.</param>
        /// <returns>The intersection area of all disks.</returns>
        public static double CommonPointCirclesIntersectionArea(
            Point2D commonPoint,
            IReadOnlyList<Point2D> centers)
        {
            ArgumentNullException.ThrowIfNull(centers);
            if (centers.Count == 0)
                return 0.0;

            Point2D[] buffer = new Point2D[centers.Count];
            for (int i = 0; i < centers.Count; i++)
                buffer[i] = centers[i];
            return CommonPointCirclesIntersectionArea(commonPoint, (ReadOnlySpan<Point2D>)buffer);
        }

        /// <summary>
        /// Computes the intersection area in O(N log N) time of disks centered at <paramref name="centers"/>,
        /// where each disk passes through <paramref name="commonPoint"/>.
        /// </summary>
        /// <param name="commonPoint">The point lying on the boundary of every disk.</param>
        /// <param name="centers">Centers of the disks.</param>
        /// <returns>The intersection area of all disks.</returns>
        public static double CommonPointCirclesIntersectionArea(
            Point2D commonPoint,
            ReadOnlySpan<Point2D> centers)
        {
            if (centers.IsEmpty)
                return 0.0;

            int n = centers.Length;
            Point2D[] shifted = new Point2D[n];
            for (int i = 0; i < n; i++)
            {
                double x = centers[i].X - commonPoint.X;
                double y = centers[i].Y - commonPoint.Y;
                if (x == 0.0 && y == 0.0)
                    return 0.0;
                shifted[i] = new Point2D(x, y);
            }

            Array.Sort(shifted, ComparePolarCCW);

            // Deduplicate collinear rays, retaining the smallest radius (smallest disk).
            int uniqueCount = 0;
            for (int i = 1; i < n; i++)
            {
                Point2D prev = shifted[uniqueCount];
                Point2D cur = shifted[i];
                double lenSqPrev = prev.X * prev.X + prev.Y * prev.Y;
                double lenSqCur = cur.X * cur.X + cur.Y * cur.Y;
                double cross = prev.X * cur.Y - prev.Y * cur.X;
                bool sameHalf = IsUpperHalf(prev) == IsUpperHalf(cur);
                if (sameHalf && Math.Abs(cross) <= 1e-12 * Math.Sqrt(lenSqPrev * lenSqCur))
                {
                    if (lenSqCur < lenSqPrev)
                        shifted[uniqueCount] = cur;
                }
                else
                {
                    shifted[++uniqueCount] = cur;
                }
            }

            int m = uniqueCount + 1;
            if (m == 1)
            {
                Point2D p0 = shifted[0];
                return Math.PI * (p0.X * p0.X + p0.Y * p0.Y);
            }

            // Locate the unique angular gap > pi around the origin.
            int gapIdx = -1;
            for (int i = 0; i < m; i++)
            {
                Point2D a = shifted[i];
                Point2D b = shifted[(i + 1) % m];
                double cross = a.X * b.Y - a.Y * b.X;
                double scale = Math.Sqrt((a.X * a.X + a.Y * a.Y) * (b.X * b.X + b.Y * b.Y));
                if (cross < -1e-15 * scale)
                {
                    gapIdx = i;
                    break;
                }
            }

            if (gapIdx < 0)
                return 0.0;

            Point2D[] hull = new Point2D[m];
            int k = 0;
            for (int offset = 1; offset <= m; offset++)
            {
                Point2D pt = shifted[(gapIdx + offset) % m];
                while (k >= 2 && Cross(hull[k - 2], hull[k - 1], pt) >= 0.0)
                {
                    k--;
                }
                hull[k++] = pt;
            }

            Point2D[] poly = new Point2D[k];
            poly[0] = new Point2D(0.0, 0.0);
            for (int j = 1; j < k; j++)
            {
                Point2D a = hull[k - 1 - j];
                Point2D b = hull[k - j];
                double det = a.X * b.Y - a.Y * b.X;
                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double invDistSq = 1.0 / (dx * dx + dy * dy);
                double factor = 2.0 * det * invDistSq;
                poly[j] = new Point2D(factor * dy, -factor * dx);
            }

            double area = PolygonArea(poly);
            for (int e = 0; e < k; e++)
            {
                Point2D c = hull[k - 1 - e];
                Point2D p1 = poly[e];
                Point2D p2 = poly[(e + 1) % k];

                double r = Math.Sqrt(c.X * c.X + c.Y * c.Y);
                double ux = p1.X - c.X;
                double uy = p1.Y - c.Y;
                double vx = p2.X - c.X;
                double vy = p2.Y - c.Y;
                double arcCross = ux * vy - uy * vx;
                double arcDot = ux * vx + uy * vy;
                double theta = Math.Atan2(arcCross, arcDot);
                if (theta < 0.0)
                {
                    theta = arcDot > 0.0 && p1.DistanceSquaredTo(p2) <= 1e-24 * (c.X * c.X + c.Y * c.Y)
                        ? 0.0
                        : theta + 2.0 * Math.PI;
                }
                area += CircularSegmentArea(r, theta);
            }

            return area;
        }

        private static bool IsUpperHalf(Point2D p) => p.Y > 0.0 || (p.Y == 0.0 && p.X > 0.0);

        private static int ComparePolarCCW(Point2D a, Point2D b)
        {
            bool ua = IsUpperHalf(a);
            bool ub = IsUpperHalf(b);
            if (ua != ub)
                return ua ? -1 : 1;

            double cross = a.X * b.Y - a.Y * b.X;
            if (cross > 0.0)
                return -1;
            if (cross < 0.0)
                return 1;

            double lenA = a.X * a.X + a.Y * a.Y;
            double lenB = b.X * b.X + b.Y * b.Y;
            return lenA.CompareTo(lenB);
        }

        private static Int128 Cross128(Point2L o, Point2L a, Point2L b) =>
            (Int128)(a.X - o.X) * (b.Y - o.Y) - (Int128)(a.Y - o.Y) * (b.X - o.X);

        /// <summary>
        /// Compares two integer direction vectors by counterclockwise polar angle around the origin in <c>[0, 2*pi)</c>
        /// starting from the positive X axis, using exact <see cref="Int128"/> cross products.
        /// Collinear vectors pointing in the same direction (or two zero vectors) compare as <c>0</c>, and a zero vector
        /// orders before any non-zero vector.
        /// </summary>
        /// <param name="u">First direction vector.</param>
        /// <param name="v">Second direction vector.</param>
        /// <returns>
        /// Negative if <paramref name="u"/> has smaller counterclockwise angle than <paramref name="v"/>,
        /// positive if larger, or <c>0</c> if both share the same ray direction (or both are zero).
        /// </returns>
        public static int CompareDirectionCCW(Point2L u, Point2L v)
        {
            bool zu = u.X == 0 && u.Y == 0;
            bool zv = v.X == 0 && v.Y == 0;
            if (zu || zv)
                return zu == zv ? 0 : (zu ? -1 : 1);

            bool hu = u.Y > 0 || (u.Y == 0 && u.X > 0);
            bool hv = v.Y > 0 || (v.Y == 0 && v.X > 0);
            if (hu != hv)
                return hu ? -1 : 1;
            Int128 cross = (Int128)u.X * v.Y - (Int128)u.Y * v.X;
            return cross > 0 ? -1 : cross < 0 ? 1 : 0;
        }

        /// <summary>
        /// Computes the strict counterclockwise convex hull of <paramref name="points"/> using Andrew's monotone chain.
        /// Handles 0, 1, duplicate, and all-collinear point sets without throwing.
        /// </summary>
        /// <param name="points">Input points.</param>
        /// <returns>List of strict convex hull vertices in counterclockwise order.</returns>
        public static List<Point2L> ConvexHull(ReadOnlySpan<Point2L> points)
        {
            List<Point2L> unique = SortAndDeduplicate(points);
            if (unique.Count <= 2)
                return unique;

            var (lower, upper) = BuildMonotoneHulls(unique, includeCollinear: false);
            List<Point2L> hull = new List<Point2L>(lower.Count + upper.Count);
            hull.AddRange(lower);
            for (int i = upper.Count - 2; i >= 1; i--)
                hull.Add(upper[i]);
            return hull;
        }

        /// <summary>
        /// Computes the lower and upper convex hull chains of <paramref name="points"/> ordered left-to-right
        /// by <c>(X, Y)</c>.
        /// </summary>
        /// <param name="points">Input points.</param>
        /// <param name="includeCollinear">
        /// If true, retains collinear points lying along the lower and upper hull boundaries; otherwise excludes them.
        /// </param>
        /// <returns>
        /// A tuple <c>(Lower, Upper)</c> where both chains start at the lexicographically smallest unique point
        /// and end at the lexicographically largest unique point.
        /// </returns>
        public static (List<Point2L> Lower, List<Point2L> Upper) MonotoneChainHulls(
            ReadOnlySpan<Point2L> points,
            bool includeCollinear = false)
        {
            List<Point2L> unique = SortAndDeduplicate(points);
            if (unique.Count == 0)
                return ([], []);
            if (unique.Count == 1)
                return ([unique[0]], [unique[0]]);
            return BuildMonotoneHulls(unique, includeCollinear);
        }

        /// <summary>
        /// Computes the minimum vertical strip width <c>min_K (max_i(y_i - K*x_i) - min_i(y_i - K*x_i))</c>
        /// over all real slopes <c>K</c> in O(n log n) time (or O(n) when <paramref name="points"/> are already sorted by X).
        /// For 1D linear Chebyshev regression <c>min_{a, K} max_i |y_i - (a + K*x_i)|</c>, the minimum error is half this width.
        /// </summary>
        /// <param name="points">Input integer points.</param>
        /// <returns>The minimum vertical width between two parallel lines enclosing all points.</returns>
        public static double MinimumVerticalStripWidth(ReadOnlySpan<Point2L> points)
        {
            if (points.Length <= 1)
                return 0.0;

            Point2L[] sorted = points.ToArray();
            Array.Sort(sorted, CompareLexXY);

            List<Point2L> lowPts = [with(capacity: sorted.Length)];
            List<Point2L> highPts = [with(capacity: sorted.Length)];
            for (int i = 0; i < sorted.Length;)
            {
                long x = sorted[i].X;
                long minY = sorted[i].Y;
                long maxY = sorted[i].Y;
                i++;
                while (i < sorted.Length && sorted[i].X == x)
                {
                    if (sorted[i].Y < minY) minY = sorted[i].Y;
                    if (sorted[i].Y > maxY) maxY = sorted[i].Y;
                    i++;
                }
                lowPts.Add(new Point2L(x, minY));
                highPts.Add(new Point2L(x, maxY));
            }

            if (lowPts.Count == 1)
                return (double)((Int128)highPts[0].Y - lowPts[0].Y);

            List<Point2L> lower = BuildHalfChain(lowPts, keepLeftTurns: true, includeCollinear: false);
            List<Point2L> upper = BuildHalfChain(highPts, keepLeftTurns: false, includeCollinear: false);

            int u = upper.Count - 1;
            int l = 0;
            double best = double.PositiveInfinity;

            while (u > 0 || l < lower.Count - 1)
            {
                bool chooseUpper;
                if (u == 0)
                {
                    chooseUpper = false;
                }
                else if (l == lower.Count - 1)
                {
                    chooseUpper = true;
                }
                else
                {
                    long dxU = upper[u].X - upper[u - 1].X;
                    long dyU = upper[u].Y - upper[u - 1].Y;
                    long dxL = lower[l + 1].X - lower[l].X;
                    long dyL = lower[l + 1].Y - lower[l].Y;
                    chooseUpper = (Int128)dyU * dxL <= (Int128)dyL * dxU;
                }

                long dx, dy;
                if (chooseUpper)
                {
                    dx = upper[u].X - upper[u - 1].X;
                    dy = upper[u].Y - upper[u - 1].Y;
                }
                else
                {
                    dx = lower[l + 1].X - lower[l].X;
                    dy = lower[l + 1].Y - lower[l].Y;
                }

                Int128 num = (Int128)(upper[u].Y - lower[l].Y) * dx - (Int128)dy * (upper[u].X - lower[l].X);
                double width = (double)num / dx;
                if (width < best)
                    best = width;

                if (chooseUpper)
                    u--;
                else
                    l++;
            }

            return best;
        }

        private static int CompareLexXY(Point2L a, Point2L b)
        {
            int cmpX = a.X.CompareTo(b.X);
            return cmpX != 0 ? cmpX : a.Y.CompareTo(b.Y);
        }

        private static List<Point2L> SortAndDeduplicate(ReadOnlySpan<Point2L> points)
        {
            if (points.Length == 0)
                return [];

            Point2L[] sorted = points.ToArray();
            Array.Sort(sorted, CompareLexXY);

            List<Point2L> unique = [sorted[0]];
            for (int i = 1; i < sorted.Length; i++)
            {
                if (sorted[i] != sorted[i - 1])
                    unique.Add(sorted[i]);
            }
            return unique;
        }

        private static (List<Point2L> Lower, List<Point2L> Upper) BuildMonotoneHulls(
            List<Point2L> unique,
            bool includeCollinear)
        {
            return (
                BuildHalfChain(unique, keepLeftTurns: true, includeCollinear),
                BuildHalfChain(unique, keepLeftTurns: false, includeCollinear));
        }

        private static List<Point2L> BuildHalfChain(
            List<Point2L> sortedPoints,
            bool keepLeftTurns,
            bool includeCollinear)
        {
            List<Point2L> chain = [with(capacity: sortedPoints.Count)];
            foreach (Point2L p in sortedPoints)
            {
                while (chain.Count >= 2)
                {
                    Int128 cross = Cross128(chain[^2], chain[^1], p);
                    bool shouldPop = keepLeftTurns
                        ? (includeCollinear ? cross < 0 : cross <= 0)
                        : (includeCollinear ? cross > 0 : cross >= 0);
                    if (!shouldPop)
                        break;
                    chain.RemoveAt(chain.Count - 1);
                }
                chain.Add(p);
            }
            return chain;
        }

        /// <summary>
        /// Computes the strict counterclockwise convex hull of <paramref name="points"/> using Andrew's monotone chain.
        /// </summary>
        /// <param name="points">Input points.</param>
        /// <returns>List of strict convex hull vertices in counterclockwise order.</returns>
        public static List<Point2L> ConvexHull(IReadOnlyList<Point2L> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            Point2L[] copy = new Point2L[points.Count];
            for (int i = 0; i < points.Count; i++)
                copy[i] = points[i];
            return ConvexHull(copy.AsSpan());
        }

        private static List<Point2L> BuildCcwEdges(ReadOnlySpan<Point2L> poly, out Point2L bottomLeft)
        {
            int n = poly.Length;
            int start = 0;
            for (int i = 1; i < n; i++)
            {
                if (poly[i].Y < poly[start].Y || (poly[i].Y == poly[start].Y && poly[i].X < poly[start].X))
                    start = i;
            }
            bottomLeft = poly[start];
            List<Point2L> edges = [with(capacity: n)];
            for (int i = 0; i < n; i++)
            {
                Point2L edge = poly[(start + i + 1) % n] - poly[(start + i) % n];
                if (edge.X != 0 || edge.Y != 0)
                    edges.Add(edge);
            }
            return edges;
        }

        /// <summary>
        /// Computes the Minkowski sum <c>A + B = { a + b : a in A, b in B }</c> of two counterclockwise convex polygons
        /// (including 1-point and 2-point degenerate convex sets) in O(|A| + |B|) time using exact <see cref="Int128"/> cross products.
        /// </summary>
        /// <param name="a">First counterclockwise convex polygon (or point/segment).</param>
        /// <param name="b">Second counterclockwise convex polygon (or point/segment).</param>
        /// <returns>Counterclockwise convex hull vertices of the Minkowski sum.</returns>
        public static List<Point2L> MinkowskiSum(ReadOnlySpan<Point2L> a, ReadOnlySpan<Point2L> b)
        {
            if (a.Length == 0 || b.Length == 0)
                return [];
            if (a.Length == 1)
            {
                Point2L offset = a[0];
                List<Point2L> shifted = [with(capacity: b.Length)];
                for (int i = 0; i < b.Length; i++)
                    shifted.Add(b[i] + offset);
                return shifted;
            }
            if (b.Length == 1)
            {
                Point2L offset = b[0];
                List<Point2L> shifted = [with(capacity: a.Length)];
                for (int i = 0; i < a.Length; i++)
                    shifted.Add(a[i] + offset);
                return shifted;
            }

            List<Point2L> ea = BuildCcwEdges(a, out Point2L startA);
            List<Point2L> eb = BuildCcwEdges(b, out Point2L startB);
            Point2L cur = startA + startB;
            if (ea.Count == 0 && eb.Count == 0)
                return [cur];

            List<Point2L> result = [with(capacity: ea.Count + eb.Count)];
            int ia = 0, ib = 0;
            while (ia < ea.Count || ib < eb.Count)
            {
                result.Add(cur);
                if (ia == ea.Count)
                {
                    cur += eb[ib++];
                }
                else if (ib == eb.Count)
                {
                    cur += ea[ia++];
                }
                else
                {
                    int cmp = CompareDirectionCCW(ea[ia], eb[ib]);
                    if (cmp < 0)
                    {
                        cur += ea[ia++];
                    }
                    else if (cmp > 0)
                    {
                        cur += eb[ib++];
                    }
                    else
                    {
                        cur += ea[ia++] + eb[ib++];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the Minkowski sum <c>A + B</c> of two counterclockwise convex polygons in O(|A| + |B|) time.
        /// </summary>
        /// <param name="a">First counterclockwise convex polygon.</param>
        /// <param name="b">Second counterclockwise convex polygon.</param>
        /// <returns>Counterclockwise convex hull vertices of the Minkowski sum.</returns>
        public static List<Point2L> MinkowskiSum(IReadOnlyList<Point2L> a, IReadOnlyList<Point2L> b)
        {
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);
            Point2L[] arrA = new Point2L[a.Count];
            for (int i = 0; i < a.Count; i++) arrA[i] = a[i];
            Point2L[] arrB = new Point2L[b.Count];
            for (int i = 0; i < b.Count; i++) arrB[i] = b[i];
            return MinkowskiSum(arrA.AsSpan(), arrB.AsSpan());
        }

        /// <summary>
        /// Extracts all faces (cycles of directed half-edges) from a planar straight-line graph using exact
        /// counterclockwise angular ordering around each vertex via <see cref="CompareDirectionCCW"/>.
        /// Bounded interior faces are returned in counterclockwise order (<see cref="PlanarFace.TwiceSignedArea"/> &gt; 0),
        /// unbounded outer faces in clockwise order (&lt; 0), and bridge/tree components have area <c>0</c>.
        /// </summary>
        /// <param name="points">Coordinates of the graph vertices indexed <c>0 .. points.Length - 1</c>.</param>
        /// <param name="edges">Undirected edges between distinct vertices.</param>
        /// <returns>List of extracted planar faces partitioning all directed half-edges.</returns>
        public static List<PlanarFace> ExtractPlanarFaces(
            ReadOnlySpan<Point2L> points,
            ReadOnlySpan<(int U, int V)> edges)
        {
            int n = points.Length;
            int m = edges.Length;
            if (m == 0)
                return [];

            int halfCount = m * 2;
            int[] from = new int[halfCount];
            int[] to = new int[halfCount];
            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = [];

            for (int i = 0; i < m; i++)
            {
                var (u, v) = edges[i];
                if ((uint)u >= (uint)n)
                    throw new ArgumentOutOfRangeException(nameof(edges), $"Vertex index {u} at edge {i} is out of range [0, {n - 1}].");
                if ((uint)v >= (uint)n)
                    throw new ArgumentOutOfRangeException(nameof(edges), $"Vertex index {v} at edge {i} is out of range [0, {n - 1}].");
                if (u == v)
                    throw new ArgumentException($"Self-loop at vertex {u} (edge {i}) is not allowed.", nameof(edges));

                int heFwd = i * 2;
                int heRev = heFwd + 1;
                from[heFwd] = u;
                to[heFwd] = v;
                from[heRev] = v;
                to[heRev] = u;
                adj[u].Add(heFwd);
                adj[v].Add(heRev);
            }

            Point2L[] pts = points.ToArray();
            int[] posInAdj = new int[halfCount];
            for (int u = 0; u < n; u++)
            {
                List<int> list = adj[u];
                if (list.Count > 1)
                {
                    Point2L origin = pts[u];
                    list.Sort((a, b) =>
                    {
                        int cmp = CompareDirectionCCW(pts[to[a]] - origin, pts[to[b]] - origin);
                        return cmp != 0 ? cmp : a.CompareTo(b);
                    });
                }

                for (int i = 0; i < list.Count; i++)
                    posInAdj[list[i]] = i;
            }

            bool[] visited = new bool[halfCount];
            List<PlanarFace> faces = [];
            for (int start = 0; start < halfCount; start++)
            {
                if (visited[start])
                    continue;

                List<int> verts = [];
                List<int> faceEdges = [];
                Int128 twiceArea = 0;
                int cur = start;
                while (!visited[cur])
                {
                    visited[cur] = true;
                    int u = from[cur];
                    int v = to[cur];
                    verts.Add(u);
                    faceEdges.Add(cur >> 1);
                    twiceArea += (Int128)pts[u].X * pts[v].Y - (Int128)pts[u].Y * pts[v].X;

                    int twin = cur ^ 1;
                    List<int> list = adj[v];
                    int nextPos = posInAdj[twin] - 1;
                    if (nextPos < 0)
                        nextPos += list.Count;
                    cur = list[nextPos];
                }

                if (cur != start)
                    throw new InvalidOperationException("Planar face walk failed to close cleanly on start half-edge.");

                faces.Add(new PlanarFace(verts, faceEdges, twiceArea));
            }

            return faces;
        }
    }

    /// <summary>
    /// Represents a face extracted from a planar straight-line graph by <see cref="Geometry.ExtractPlanarFaces"/>.
    /// </summary>
    public sealed class PlanarFace
    {
        internal PlanarFace(IReadOnlyList<int> vertices, IReadOnlyList<int> edges, Int128 twiceSignedArea)
        {
            Vertices = vertices;
            Edges = edges;
            TwiceSignedArea = twiceSignedArea;
        }

        /// <summary>
        /// Gets the ordered vertex indices along the face boundary cycle.
        /// </summary>
        public IReadOnlyList<int> Vertices { get; }

        /// <summary>
        /// Gets the ordered undirected edge indices connecting consecutive vertices in <see cref="Vertices"/>.
        /// </summary>
        public IReadOnlyList<int> Edges { get; }

        /// <summary>
        /// Gets twice the exact signed area of the face boundary using <see cref="Int128"/> arithmetic.
        /// Positive for counterclockwise bounded interior faces, negative for clockwise outer faces,
        /// and <c>0</c> for degenerate bridge-only faces.
        /// </summary>
        public Int128 TwiceSignedArea { get; }
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

    /// <summary>
    /// Compares <see cref="Point2L"/> points by full 360-degree counterclockwise polar angle in <c>[0, 2*pi)</c>
    /// around a specified origin using exact <see cref="Int128"/> cross products.
    /// </summary>
    public readonly struct PolarAngleComparer : IComparer<Point2L>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolarAngleComparer"/> struct.
        /// </summary>
        /// <param name="origin">Origin point relative to which polar angles are measured.</param>
        public PolarAngleComparer(Point2L origin)
        {
            Origin = origin;
        }

        /// <summary>
        /// Gets the origin point relative to which polar angles are measured.
        /// </summary>
        public Point2L Origin { get; }

        /// <inheritdoc />
        public int Compare(Point2L x, Point2L y) => Geometry.CompareDirectionCCW(x - Origin, y - Origin);
    }

    /// <summary>
    /// Immutable 3D point or vector with 64-bit integer coordinates and overflow-safe
    /// <see cref="Int128"/> dot and scalar triple product helpers.
    /// </summary>
    public readonly struct Point3L : IEquatable<Point3L>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point3L"/> struct.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Point3L(long x, long y, long z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public long X { get; }

        /// <summary>
        /// Gets the Y coordinate.
        /// </summary>
        public long Y { get; }

        /// <summary>
        /// Gets the Z coordinate.
        /// </summary>
        public long Z { get; }

        /// <summary>
        /// Computes the exact dot product with <paramref name="other"/> using <see cref="Int128"/> arithmetic.
        /// </summary>
        /// <param name="other">Other vector.</param>
        /// <returns>Exact dot product <c>X * other.X + Y * other.Y + Z * other.Z</c>.</returns>
        public Int128 Dot128(Point3L other) =>
            (Int128)X * other.X + (Int128)Y * other.Y + (Int128)Z * other.Z;

        /// <summary>
        /// Computes the 3D cross product <c>this x other</c>.
        /// </summary>
        /// <param name="other">Right-hand vector.</param>
        /// <returns>Cross product vector.</returns>
        public Point3L Cross(Point3L other) => new(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X);

        /// <summary>
        /// Computes the exact scalar triple product <c>(a x b) . c</c> using <see cref="Int128"/> intermediates
        /// for coordinates whose pairwise products may approach 64-bit limits.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="c">Third vector.</param>
        /// <returns>Exact scalar triple product <c>(a x b) . c</c>.</returns>
        public static Int128 TripleProduct128(Point3L a, Point3L b, Point3L c)
        {
            Int128 cx = (Int128)a.Y * b.Z - (Int128)a.Z * b.Y;
            Int128 cy = (Int128)a.Z * b.X - (Int128)a.X * b.Z;
            Int128 cz = (Int128)a.X * b.Y - (Int128)a.Y * b.X;
            return cx * c.X + cy * c.Y + cz * c.Z;
        }

        /// <summary>
        /// Computes the L1 (Manhattan) distance to <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Target point.</param>
        /// <returns><c>|X - other.X| + |Y - other.Y| + |Z - other.Z|</c>.</returns>
        public long ManhattanDistanceTo(Point3L other) =>
            Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);

        /// <summary>
        /// Negates all coordinates of a vector.
        /// </summary>
        public static Point3L operator -(Point3L point) => new(-point.X, -point.Y, -point.Z);

        /// <summary>
        /// Adds two 3D vectors componentwise.
        /// </summary>
        public static Point3L operator +(Point3L left, Point3L right) =>
            new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        /// <summary>
        /// Subtracts two 3D vectors componentwise.
        /// </summary>
        public static Point3L operator -(Point3L left, Point3L right) =>
            new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        /// <summary>
        /// Scales a 3D vector by a scalar.
        /// </summary>
        public static Point3L operator *(Point3L point, long scalar) =>
            new(point.X * scalar, point.Y * scalar, point.Z * scalar);

        /// <summary>
        /// Scales a 3D vector by a scalar.
        /// </summary>
        public static Point3L operator *(long scalar, Point3L point) =>
            new(scalar * point.X, scalar * point.Y, scalar * point.Z);

        /// <inheritdoc />
        public bool Equals(Point3L other) => X == other.X && Y == other.Y && Z == other.Z;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Point3L other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        /// <summary>
        /// Returns whether two <see cref="Point3L"/> values are equal.
        /// </summary>
        public static bool operator ==(Point3L left, Point3L right) => left.Equals(right);

        /// <summary>
        /// Returns whether two <see cref="Point3L"/> values are not equal.
        /// </summary>
        public static bool operator !=(Point3L left, Point3L right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}