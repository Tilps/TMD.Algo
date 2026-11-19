#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Elegant Diamond

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int k);
            test.GetLines(2 * k - 1, out string[] lines);
            return () => Q1Solver(k, lines);
        }

        private static int Q1Solver(int k, string[] lines)
        {
            int size = 2 * k - 1;
            int[,] grid = new int[size, size];
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    grid[r, c] = -1;
                }

                string line = lines[r];
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c] is >= '0' and <= '9')
                        grid[r, c] = line[c] - '0';
                }
            }

            bool IsSymmetric(int cr, int cc)
            {
                for (int r = 0; r < size; r++)
                {
                    int rr = 2 * cr - r;
                    for (int c = 0; c < size; c++)
                    {
                        int val = grid[r, c];
                        if (val < 0)
                            continue;

                        int cc2 = 2 * cc - c;
                        if ((uint)rr < (uint)size && grid[rr, c] >= 0 && grid[rr, c] != val)
                            return false;
                        if ((uint)cc2 < (uint)size && grid[r, cc2] >= 0 && grid[r, cc2] != val)
                            return false;
                        if ((uint)rr < (uint)size && (uint)cc2 < (uint)size && grid[rr, cc2] >= 0 && grid[rr, cc2] != val)
                            return false;
                    }
                }
                return true;
            }

            int center = k - 1;
            int maxShift = 2 * (k - 1);
            for (int shift = 0; shift <= maxShift; shift++)
            {
                for (int dr = -shift; dr <= shift; dr++)
                {
                    int cr = center + dr;
                    if ((uint)cr >= (uint)size)
                        continue;

                    int rem = shift - Math.Abs(dr);
                    if (rem == 0)
                    {
                        if (IsSymmetric(cr, center))
                            return (k + shift) * (k + shift) - k * k;
                    }
                    else
                    {
                        int ccLeft = center - rem;
                        if ((uint)ccLeft < (uint)size && IsSymmetric(cr, ccLeft))
                            return (k + shift) * (k + shift) - k * k;

                        int ccRight = center + rem;
                        if ((uint)ccRight < (uint)size && IsSymmetric(cr, ccRight))
                            return (k + shift) * (k + shift) - k * k;
                    }
                }
            }

            return (k + maxShift) * (k + maxShift) - k * k;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Large()
        {
            SlowTest(Q1Solver);
        }

        #endregion

        #region Question 2 - World Cup 2010

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int p);
            test.Get(out int[] maxMiss);
            long[][] prices = new long[p + 1][];
            for (int level = 1; level <= p; level++)
            {
                test.Get(out prices[level]);
            }
            return () => Q2Solver(p, maxMiss, prices);
        }

        private static long Q2Solver(int p, int[] maxMiss, long[][] prices)
        {
            const long Inf = 1L << 50;
            int teamCount = 1 << p;
            long[,] cur = new long[teamCount, p + 1];

            for (int i = 0; i < teamCount; i++)
            {
                int needed = p - maxMiss[i];
                for (int b = 0; b <= p; b++)
                {
                    cur[i, b] = b >= needed ? 0L : Inf;
                }
            }

            for (int level = 1; level <= p; level++)
            {
                int matchCount = 1 << (p - level);
                int maxAbove = p - level;
                long[,] next = new long[matchCount, maxAbove + 1];
                long[] levelPrices = prices[level];

                for (int idx = 0; idx < matchCount; idx++)
                {
                    int left = idx * 2;
                    int right = left + 1;
                    long price = levelPrices[idx];
                    for (int b = 0; b <= maxAbove; b++)
                    {
                        long skip = cur[left, b] + cur[right, b];
                        long buy = price + cur[left, b + 1] + cur[right, b + 1];
                        next[idx, b] = Math.Min(skip, buy);
                    }
                }

                cur = next;
            }

            return cur[0, 0];
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver);
        }

        #endregion

        #region Question 3 - Bacteria

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r);
            test.GetLines(r, out (int X1, int Y1, int X2, int Y2)[] rects);
            return () => Q3Solver(r, rects);
        }

        private static bool RectsConnected((int X1, int Y1, int X2, int Y2) a, (int X1, int Y1, int X2, int Y2) b)
        {
            // Horizontal adjacency or overlap
            if (a.X1 <= b.X2 + 1 && b.X1 <= a.X2 + 1 && a.Y1 <= b.Y2 && b.Y1 <= a.Y2)
                return true;
            // Vertical adjacency or overlap
            if (a.X1 <= b.X2 && b.X1 <= a.X2 && a.Y1 <= b.Y2 + 1 && b.Y1 <= a.Y2 + 1)
                return true;
            // NE/SW type-1 diagonal: (xa + 1, ya - 1) in b or (xb + 1, yb - 1) in a
            if (a.X1 + 1 <= b.X2 && b.X1 <= a.X2 + 1 && a.Y1 - 1 <= b.Y2 && b.Y1 <= a.Y2 - 1)
                return true;
            if (b.X1 + 1 <= a.X2 && a.X1 <= b.X2 + 1 && b.Y1 - 1 <= a.Y2 && a.Y1 <= b.Y2 - 1)
                return true;
            return false;
        }

        private static long Q3Solver(int r, (int X1, int Y1, int X2, int Y2)[] rects)
        {
            DisjointTracker<int> dsu = new();
            for (int i = 0; i < r; i++)
                dsu.Add(i);

            for (int i = 0; i < r; i++)
            {
                for (int j = i + 1; j < r; j++)
                {
                    if (RectsConnected(rects[i], rects[j]))
                        dsu.Union(i, j);
                }
            }

            long[] maxX = new long[r];
            long[] maxY = new long[r];
            long[] minDiag = new long[r];
            Array.Fill(maxX, long.MinValue);
            Array.Fill(maxY, long.MinValue);
            Array.Fill(minDiag, long.MaxValue);

            for (int i = 0; i < r; i++)
            {
                int root = dsu.GetRepresentative(i);
                var rect = rects[i];
                if (rect.X2 > maxX[root]) maxX[root] = rect.X2;
                if (rect.Y2 > maxY[root]) maxY[root] = rect.Y2;
                long diag = (long)rect.X1 + rect.Y1;
                if (diag < minDiag[root]) minDiag[root] = diag;
            }

            long best = 0;
            for (int i = 0; i < r; i++)
            {
                if (minDiag[i] != long.MaxValue)
                {
                    long time = maxX[i] + maxY[i] - minDiag[i] + 1;
                    if (time > best)
                        best = time;
                }
            }

            return best;
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver);
        }

        #endregion

        #region Question 4 - Grazing Google Goats

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            Point2D[] poles = new Point2D[n];
            for (int i = 0; i < n; i++)
            {
                test.Get(out double x, out double y);
                poles[i] = new Point2D(x, y);
            }

            Point2D[] buckets = new Point2D[m];
            for (int i = 0; i < m; i++)
            {
                test.Get(out double x, out double y);
                buckets[i] = new Point2D(x, y);
            }

            return () => Q4Solver(poles, buckets);
        }

        private static string Q4Solver(Point2D[] poles, Point2D[] buckets)
        {
            string[] answers = new string[buckets.Length];
            for (int i = 0; i < buckets.Length; i++)
            {
                double area = Geometry.CommonPointCirclesIntersectionArea(buckets[i], poles);
                answers[i] = area.ToString("F7", CultureInfo.InvariantCulture);
            }
            return string.Join(' ', answers);
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, 1e-6);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, 1e-6);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver, 1e-6);
        }

        #endregion
    }
}
