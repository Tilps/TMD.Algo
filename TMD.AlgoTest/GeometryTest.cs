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
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class GeometryTest
    {
        [Test]
        public void Basic()
        {
            List<KeyValuePair<long, long>> points = new List<KeyValuePair<long, long>>
            {
                new KeyValuePair<long, long>(0, 0),
                new KeyValuePair<long, long>(int.MaxValue, 0),
                new KeyValuePair<long, long>(0, int.MaxValue),
                new KeyValuePair<long, long>(int.MaxValue, int.MaxValue)
            };
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                points.Add(new KeyValuePair<long, long>(rnd.Next(1, int.MaxValue - 1), rnd.Next(1, int.MaxValue - 1)));
            }
            for (int i = 0; i < 1000; i++)
            {
                int a = rnd.Next(points.Count);
                int b = rnd.Next(points.Count);
                KeyValuePair<long, long> tmp = points[a];
                points[a] = points[b];
                points[b] = tmp;
            }
            List<KeyValuePair<long, long>> hull = Geometry.ConvexHull(points);
            Assert.AreEqual(4, hull.Count);
            Assert.AreEqual(0, hull[0].Key);
            Assert.AreEqual(0, hull[0].Value);
            Assert.AreEqual(int.MaxValue, hull[1].Key);
            Assert.AreEqual(0, hull[1].Value);
            Assert.AreEqual(int.MaxValue, hull[2].Key);
            Assert.AreEqual(int.MaxValue, hull[2].Value);
            Assert.AreEqual(0, hull[3].Key);
            Assert.AreEqual(int.MaxValue, hull[3].Value);
        }

        [Test]
        public void CollinearPoints()
        {
            var points = new List<KeyValuePair<long, long>>
            {
                new KeyValuePair<long, long>(0, 0),
                new KeyValuePair<long, long>(5, 0),
                new KeyValuePair<long, long>(10, 0),
                new KeyValuePair<long, long>(5, 5),
                new KeyValuePair<long, long>(10, 10),
                new KeyValuePair<long, long>(0, 10)
            };
            var hull = Geometry.ConvexHull(points);
            Assert.AreEqual(4, hull.Count);
            Assert.AreEqual(new KeyValuePair<long, long>(0, 0), hull[0]);
            Assert.AreEqual(new KeyValuePair<long, long>(10, 0), hull[1]);
            Assert.AreEqual(new KeyValuePair<long, long>(10, 10), hull[2]);
            Assert.AreEqual(new KeyValuePair<long, long>(0, 10), hull[3]);
        }

        [Test]
        public void PolygonAreaAndConvexIntersectionAndCircleIntersection()
        {
            Point2D[] square =
            [
                new(0, 0),
                new(4, 0),
                new(4, 4),
                new(0, 4)
            ];
            Assert.AreEqual(16.0, Geometry.PolygonArea(square), 1e-12);

            Point2D[] shiftedSquare =
            [
                new(2, 2),
                new(6, 2),
                new(6, 6),
                new(2, 6)
            ];
            List<Point2D> intersection = Geometry.ConvexPolygonIntersection(square, shiftedSquare);
            Assert.AreEqual(4.0, Geometry.PolygonArea(intersection), 1e-12);

            Point2D[] disjoint =
            [
                new(10, 10),
                new(12, 10),
                new(12, 12),
                new(10, 12)
            ];
            Assert.AreEqual(0, Geometry.ConvexPolygonIntersection(square, disjoint).Count);

            // Unit square [0,1]^2 intersected with unit disk centered at (0,0) = pi/4.
            Point2D[] unitSquare = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)];
            Assert.AreEqual(Math.PI * 0.25, Geometry.CirclePolygonIntersectionArea(new Point2D(0, 0), 1.0, unitSquare), 1e-12);

            // Disk completely inside square -> circle area.
            Assert.AreEqual(Math.PI, Geometry.CirclePolygonIntersectionArea(new Point2D(2, 2), 1.0, square), 1e-12);

            // Disk completely containing square -> square area.
            Assert.AreEqual(16.0, Geometry.CirclePolygonIntersectionArea(new Point2D(2, 2), 10.0, square), 1e-12);
        }

        [Test]
        public void ClosestPairAndMinimumTrianglePerimeterAgainstBruteForce()
        {
            Random rnd = new(42);
            Point2D[] pts = new Point2D[45];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = new Point2D(rnd.Next(-500, 500), rnd.Next(-500, 500));
            }

            double expectedPair = double.PositiveInfinity;
            double expectedPerim = double.PositiveInfinity;
            for (int i = 0; i < pts.Length; i++)
            {
                for (int j = i + 1; j < pts.Length; j++)
                {
                    double dij = pts[i].DistanceTo(pts[j]);
                    if (dij < expectedPair)
                        expectedPair = dij;
                    for (int k = j + 1; k < pts.Length; k++)
                    {
                        double p = dij + pts[j].DistanceTo(pts[k]) + pts[i].DistanceTo(pts[k]);
                        if (p < expectedPerim)
                            expectedPerim = p;
                    }
                }
            }

            Assert.AreEqual(expectedPair, Geometry.ClosestPairDistance(pts), 1e-9);
            Assert.AreEqual(expectedPerim, Geometry.MinimumTrianglePerimeter(pts), 1e-9);

            // Collinear sample from GCJ 2009 Finals Min Perimeter.
            Point2D[] collinear = new Point2D[10];
            for (int i = 0; i < 10; i++)
                collinear[i] = new Point2D(i, i);
            Assert.AreEqual(4.0 * Math.Sqrt(2.0), Geometry.MinimumTrianglePerimeter(collinear), 1e-9);
        }

        [Test]
        public void CircularSegmentAndCommonPointCirclesIntersection()
        {
            // Full circle and semicircle segments.
            Assert.AreEqual(0.0, Geometry.CircularSegmentArea(5.0, 0.0), 1e-12);
            Assert.AreEqual(Math.PI * 0.5 * 4.0, Geometry.CircularSegmentArea(2.0, Math.PI), 1e-12);
            Assert.AreEqual(Math.PI * 9.0, Geometry.CircularSegmentArea(3.0, 2.0 * Math.PI), 1e-12);
            // Tiny angle avoids cancellation.
            Assert.AreEqual(0.5 * 100.0 * Math.Pow(1e-5, 3) / 6.0, Geometry.CircularSegmentArea(10.0, 1e-5), 1e-22);

            // Empty and single circle.
            Assert.AreEqual(0.0, Geometry.CommonPointCirclesIntersectionArea(new Point2D(0, 0), ReadOnlySpan<Point2D>.Empty));
            Assert.AreEqual(Math.PI * 25.0, Geometry.CommonPointCirclesIntersectionArea(new Point2D(1, 2), [new Point2D(4, 6)]), 1e-9);

            // Collinear same-ray circles: intersection equals smallest circle.
            Assert.AreEqual(Math.PI * 4.0, Geometry.CommonPointCirclesIntersectionArea(
                new Point2D(0, 0),
                [new Point2D(2, 0), new Point2D(5, 0), new Point2D(10, 0)]), 1e-9);

            // Common point inside convex hull of centers -> zero area.
            Assert.AreEqual(0.0, Geometry.CommonPointCirclesIntersectionArea(
                new Point2D(10, 5),
                [new Point2D(0, 0), new Point2D(10, 10), new Point2D(20, 0)]), 1e-9);

            // Two equal orthogonal unit disks centered at (1, 0) and (0, 1) through (0, 0): pi/2 - 1.
            Assert.AreEqual(Math.PI * 0.5 - 1.0, Geometry.CommonPointCirclesIntersectionArea(
                new Point2D(0, 0),
                [new Point2D(1, 0), new Point2D(0, 1)]), 1e-11);

            // GCJ 2010 R2 Grazing Google Goats Sample #1 & #2 (tests reflex interior arc > pi).
            Point2D[] poles1 = [new(0, 20), new(20, 0)];
            Assert.AreEqual(1264.9865911, Geometry.CommonPointCirclesIntersectionArea(new Point2D(-20, 10), poles1), 1e-6);
            Assert.AreEqual(1713.2741229, Geometry.CommonPointCirclesIntersectionArea(new Point2D(40, 20), poles1), 1e-6);
            Assert.AreEqual(0.2939440, Geometry.CommonPointCirclesIntersectionArea(new Point2D(0, 19), poles1), 1e-6);

            Point2D[] poles2 = [new(0, 0), new(100, 100), new(300, 0), new(380, 90)];
            Assert.AreEqual(1518.9063729, Geometry.CommonPointCirclesIntersectionArea(new Point2D(400, 100), poles2), 1e-6);
            Assert.AreEqual(1193932.9692206, Geometry.CommonPointCirclesIntersectionArea(new Point2D(1000, 5), poles2), 1e-6);

            // Multi-circle asymmetric case checked against Monte-Carlo / grid midpoint quadrature.
            Point2D q = new(0, 0);
            Point2D[] multi = [new(3, 0), new(2, 1), new(1, 2), new(0, 4), new(5, 2)];
            double computed = Geometry.CommonPointCirclesIntersectionArea(q, multi);
            // Notice (2,1) and (1,2) keep the intersection strictly bounded and include 3+ active arcs.
            const int steps = 800;
            double minX = -2.0, maxX = 4.0, minY = -2.0, maxY = 4.0;
            double dx = (maxX - minX) / steps;
            double dy = (maxY - minY) / steps;
            int inside = 0;
            for (int ix = 0; ix < steps; ix++)
            {
                double x = minX + (ix + 0.5) * dx;
                for (int iy = 0; iy < steps; iy++)
                {
                    double y = minY + (iy + 0.5) * dy;
                    bool ok = true;
                    foreach (Point2D c in multi)
                    {
                        double r2 = c.X * c.X + c.Y * c.Y;
                        if ((x - c.X) * (x - c.X) + (y - c.Y) * (y - c.Y) > r2)
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok) inside++;
                }
            }
            double approx = inside * dx * dy;
            Assert.AreEqual(approx, computed, 0.02);
        }

        [Test]
        public void Point2LConvexHullAndMinkowskiSum()
        {
            Point2L p0 = new(1_000_000_000_000L, -2_000_000_000_000L);
            Assert.IsTrue(p0.Length() > 2.2e12);
            Assert.AreEqual(5_000_000_000_000_000_000_000_000.0, (double)p0.LengthSquared128(), 1e12);
            Assert.AreEqual((new Point2L(1, 2) + new Point2L(3, 4)) * 2L - new Point2L(1, 0), new Point2L(7, 12));

            // Degenerate hulls: 0, 1, duplicates, collinear
            Assert.IsEmpty(Geometry.ConvexHull(ReadOnlySpan<Point2L>.Empty));
            CollectionAssert.AreEqual(new[] { new Point2L(5, 5) }, Geometry.ConvexHull([new Point2L(5, 5), new Point2L(5, 5)]));
            CollectionAssert.AreEqual(
                new[] { new Point2L(0, 0), new Point2L(4, 2) },
                Geometry.ConvexHull([new Point2L(2, 1), new Point2L(0, 0), new Point2L(4, 2), new Point2L(2, 1)]));

            // Square with interior and collinear boundary points
            List<Point2L> sqHull = Geometry.ConvexHull(
            [
                new Point2L(0, 0), new Point2L(1, 0), new Point2L(2, 0),
                new Point2L(2, 1), new Point2L(2, 2), new Point2L(1, 2),
                new Point2L(0, 2), new Point2L(0, 1), new Point2L(1, 1)
            ]);
            CollectionAssert.AreEqual(
                new[] { new Point2L(0, 0), new Point2L(2, 0), new Point2L(2, 2), new Point2L(0, 2) },
                sqHull);

            // Minkowski sum degenerate cases: point + poly, segment + segment, segment + triangle
            CollectionAssert.AreEqual(
                new[] { new Point2L(10, 20), new Point2L(12, 20), new Point2L(12, 22), new Point2L(10, 22) },
                Geometry.MinkowskiSum([new Point2L(10, 20)], sqHull));

            List<Point2L> seg1 = [new(0, 0), new(3, 0)];
            List<Point2L> seg2 = [new(0, 0), new(0, 4)];
            CollectionAssert.AreEqual(
                new[] { new Point2L(0, 0), new Point2L(3, 0), new Point2L(3, 4), new Point2L(0, 4) },
                Geometry.MinkowskiSum(seg1, seg2));

            List<Point2L> tri = Geometry.ConvexHull([new Point2L(0, 0), new Point2L(2, 0), new Point2L(1, 3)]);
            List<Point2L> sumHull = Geometry.MinkowskiSum(sqHull, tri);
            List<Point2L> bruteSums = [];
            foreach (Point2L a in sqHull)
                foreach (Point2L b in tri)
                    bruteSums.Add(a + b);
            List<Point2L> expected = Geometry.ConvexHull(bruteSums);
            CollectionAssert.AreEqual(expected, sumHull);
        }

        [Test]
        public void MonotoneHullsAndMinimumVerticalStripWidth()
        {
            // Collinear retention vs exclusion on square with boundary points.
            Point2L[] sq =
            [
                new(0, 0), new(1, 0), new(2, 0),
                new(2, 1), new(2, 2), new(1, 2),
                new(0, 2), new(0, 1), new(1, 1)
            ];
            var (strictLow, strictHigh) = Geometry.MonotoneChainHulls(sq, includeCollinear: false);
            CollectionAssert.AreEqual(new[] { new Point2L(0, 0), new Point2L(2, 0), new Point2L(2, 2) }, strictLow);
            CollectionAssert.AreEqual(new[] { new Point2L(0, 0), new Point2L(0, 2), new Point2L(2, 2) }, strictHigh);

            var (colLow, colHigh) = Geometry.MonotoneChainHulls(sq, includeCollinear: true);
            CollectionAssert.AreEqual(
                new[] { new Point2L(0, 0), new Point2L(1, 0), new Point2L(2, 0), new Point2L(2, 1), new Point2L(2, 2) },
                colLow);
            CollectionAssert.AreEqual(
                new[] { new Point2L(0, 0), new Point2L(0, 1), new Point2L(0, 2), new Point2L(1, 2), new Point2L(2, 2) },
                colHigh);

            // Minimum vertical strip width degenerate and drummer samples.
            Assert.AreEqual(0.0, Geometry.MinimumVerticalStripWidth(ReadOnlySpan<Point2L>.Empty));
            Assert.AreEqual(0.0, Geometry.MinimumVerticalStripWidth([new Point2L(5, 10)]));
            Assert.AreEqual(7.0, Geometry.MinimumVerticalStripWidth([new Point2L(3, 2), new Point2L(3, 9), new Point2L(3, 5)]), 1e-12);
            Assert.AreEqual(0.0, Geometry.MinimumVerticalStripWidth([new Point2L(0, 10), new Point2L(1, 70)]), 1e-12);
            Assert.AreEqual(1.0, Geometry.MinimumVerticalStripWidth([new Point2L(0, 0), new Point2L(1, 10), new Point2L(2, 19), new Point2L(3, 30)]), 1e-12);
            Assert.AreEqual(1.5, Geometry.MinimumVerticalStripWidth([new Point2L(0, 2), new Point2L(1, 5), new Point2L(2, 10), new Point2L(3, 15), new Point2L(4, 20), new Point2L(5, 24)]), 1e-12);

            // Duplicate X coordinates (square + interior): best vertical strip is horizontal K=0 -> height 2.
            Assert.AreEqual(2.0, Geometry.MinimumVerticalStripWidth(sq), 1e-12);
        }

        [Test]
        public void PolarAngleComparerAndDirectionCCW()
        {
            Assert.AreEqual(0, Geometry.CompareDirectionCCW(default, default));
            Assert.Less(Geometry.CompareDirectionCCW(default, new Point2L(1, 0)), 0);
            Assert.Greater(Geometry.CompareDirectionCCW(new Point2L(1, 0), default), 0);
            Assert.AreEqual(0, Geometry.CompareDirectionCCW(new Point2L(2, 4), new Point2L(3, 6)));
            Assert.Less(Geometry.CompareDirectionCCW(new Point2L(1, 0), new Point2L(-1, 0)), 0);

            Point2L origin = new(100, -50);
            Point2L[] pts =
            [
                new(100, -51),  // -y axis (3pi/2)
                new(99, -51),   // Q3 (5pi/4)
                new(102, -50),  // +x axis (0)
                new(101, -49),  // Q1 (pi/4)
                new(99, -50),   // -x axis (pi)
                new(100, -49),  // +y axis (pi/2)
                new(99, -49),   // Q2 (3pi/4)
                new(101, -51),  // Q4 (7pi/4)
                origin          // zero vector first
            ];
            Array.Sort(pts, new PolarAngleComparer(origin));

            Point2L[] expected =
            [
                origin,
                new(102, -50),
                new(101, -49),
                new(100, -49),
                new(99, -49),
                new(99, -50),
                new(99, -51),
                new(100, -51),
                new(101, -51)
            ];
            CollectionAssert.AreEqual(expected, pts);

            // Large coordinates ~10^12 requiring Int128 cross product without overflow.
            Point2L largeA = new(1_000_000_000_001L, 1_000_000_000_000L);
            Point2L largeB = new(1_000_000_000_000L, 1_000_000_000_000L);
            Assert.Less(Geometry.CompareDirectionCCW(largeA, largeB), 0);
            Assert.Greater(Geometry.CompareDirectionCCW(largeB, largeA), 0);
        }
    }
}