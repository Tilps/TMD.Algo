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
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Cheaters

        private static Func<double> Q1Solver(GCJTestCase test)
        {
            test.Get(out long b, out int n);
            test.Get(out long[] raw);
            return () => Q1Solver(b, n, raw);
        }

        private static double Q1Solver(long b, int n, long[] raw)
        {
            long[] x = new long[37];
            for (int i = 0; i < n; i++)
                x[i] = raw[i];
            Array.Sort(x);

            long Cost(int k, long h)
            {
                long total = 0;
                for (int i = 0; i < k; i++)
                    total += h - x[i];
                for (int i = k; i < 37; i++)
                {
                    if (x[i] <= h)
                        total += h + 1 - x[i];
                }
                return total;
            }

            double best = 0.0;
            for (int k = 1; k <= 36; k++)
            {
                long minH = x[k - 1];
                if (Cost(k, minH) > b)
                    continue;

                long lo = minH;
                long hi = minH + b;
                long maxH = minH;
                while (lo <= hi)
                {
                    long mid = lo + ((hi - lo) >> 1);
                    if (Cost(k, mid) <= b)
                    {
                        maxH = mid;
                        lo = mid + 1;
                    }
                    else
                    {
                        hi = mid - 1;
                    }
                }

                // Profit decreases linearly once h + 1 >= x[36], so cap at max(minH, x[36] - 1).
                long optH = Math.Min(maxH, Math.Max(minH, x[36] - 1));
                long winBet = 0;
                for (int i = 0; i < k; i++)
                    winBet += optH - x[i];
                long totalBet = Cost(k, optH);
                double profit = (double)(36L * winBet - (long)k * totalBet) / k;
                if (profit > best)
                    best = profit;
            }

            return best;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-6);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-6);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, 1e-6);
        }

        #endregion

        #region Question 2 - Rural Planning

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            Point2L[] pts = new Point2L[n];
            for (int i = 0; i < n; i++)
            {
                test.Get(out long x, out long y);
                pts[i] = new Point2L(x, y);
            }
            return () => Q2Solver(pts);
        }

        private static long ShoelaceTimes2(List<Point2L> poly)
        {
            long sum = 0;
            int n = poly.Count;
            for (int i = 0; i < n; i++)
            {
                Point2L a = poly[i];
                Point2L b = poly[(i + 1) % n];
                sum += a.X * b.Y - a.Y * b.X;
            }
            return sum;
        }

        private static string Q2Solver(Point2L[] pts)
        {
            int n = pts.Length;
            Dictionary<Point2L, int> indexMap = [with(capacity: n)];
            for (int i = 0; i < n; i++)
                indexMap[pts[i]] = i;

            Point2L[] sorted = (Point2L[])pts.Clone();
            Array.Sort(sorted, static (a, b) =>
            {
                int cmpX = a.X.CompareTo(b.X);
                return cmpX != 0 ? cmpX : a.Y.CompareTo(b.Y);
            });

            var (lower, upper) = Geometry.MonotoneChainHulls(sorted, includeCollinear: true);

            List<Point2L> BuildCandidate(List<Point2L> chain)
            {
                HashSet<Point2L> inChain = [with(capacity: chain.Count)];
                foreach (Point2L p in chain)
                    inChain.Add(p);

                List<Point2L> poly = [with(capacity: n)];
                poly.AddRange(chain);
                for (int i = sorted.Length - 1; i >= 0; i--)
                {
                    if (!inChain.Contains(sorted[i]))
                        poly.Add(sorted[i]);
                }
                return poly;
            }

            List<Point2L> polyUpper = BuildCandidate(upper);
            List<Point2L> polyLower = BuildCandidate(lower);

            long areaU = Math.Abs(ShoelaceTimes2(polyUpper));
            long areaL = Math.Abs(ShoelaceTimes2(polyLower));
            List<Point2L> chosen = areaU >= areaL ? polyUpper : polyLower;

            int[] res = new int[n];
            for (int i = 0; i < n; i++)
                res[i] = indexMap[chosen[i]];
            return string.Join(" ", res);
        }

        private static int Orient(Point2L a, Point2L b, Point2L c)
        {
            long cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return cross > 0 ? 1 : cross < 0 ? -1 : 0;
        }

        private static bool OnSegment(Point2L a, Point2L b, Point2L p) =>
            Orient(a, b, p) == 0 &&
            p.X >= Math.Min(a.X, b.X) && p.X <= Math.Max(a.X, b.X) &&
            p.Y >= Math.Min(a.Y, b.Y) && p.Y <= Math.Max(a.Y, b.Y);

        private static bool SegmentsIntersect(Point2L a, Point2L b, Point2L c, Point2L d)
        {
            int o1 = Orient(a, b, c);
            int o2 = Orient(a, b, d);
            int o3 = Orient(c, d, a);
            int o4 = Orient(c, d, b);
            if (o1 * o2 < 0 && o3 * o4 < 0)
                return true;
            return (o1 == 0 && OnSegment(a, b, c))
                || (o2 == 0 && OnSegment(a, b, d))
                || (o3 == 0 && OnSegment(c, d, a))
                || (o4 == 0 && OnSegment(c, d, b));
        }

        private static readonly char[] LineSeps = ['\r', '\n'];
        private static readonly char[] TokenSeps = [' ', '\t'];

        private static void ValidateQ2(string input, string expectedOutput, List<string> results)
        {
            string[] inLines = input.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            int lineIdx = 0;
            int t = int.Parse(inLines[lineIdx++]);
            Assert.AreEqual(t, results.Count);

            for (int tc = 0; tc < t; tc++)
            {
                int n = int.Parse(inLines[lineIdx++]);
                Point2L[] pts = new Point2L[n];
                for (int i = 0; i < n; i++)
                {
                    string[] tok = inLines[lineIdx++].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                    pts[i] = new Point2L(long.Parse(tok[0]), long.Parse(tok[1]));
                }

                string[] actTok = results[tc].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(n + 2, actTok.Length, $"Case #{tc + 1} wrong token count");

                bool[] seen = new bool[n];
                List<Point2L> poly = [with(capacity: n)];
                for (int i = 0; i < n; i++)
                {
                    int idx = int.Parse(actTok[i + 2]);
                    Assert.IsTrue(idx >= 0 && idx < n && !seen[idx], $"Case #{tc + 1} invalid/duplicate index {idx}");
                    seen[idx] = true;
                    poly.Add(pts[idx]);
                }

                // Verify adjacent edges do not fold back 180 degrees and non-adjacent edges never intersect.
                for (int i = 0; i < n; i++)
                {
                    Point2L a = poly[i];
                    Point2L b = poly[(i + 1) % n];
                    Point2L c = poly[(i + 2) % n];
                    if (Orient(a, b, c) == 0)
                    {
                        long dot = (b.X - a.X) * (c.X - b.X) + (b.Y - a.Y) * (c.Y - b.Y);
                        Assert.IsTrue(dot > 0, $"Case #{tc + 1} has 180-degree foldback at vertex {i + 1}");
                    }

                    for (int j = i + 2; j < n; j++)
                    {
                        if (i == 0 && j == n - 1)
                            continue;
                        Assert.IsFalse(
                            SegmentsIntersect(a, b, poly[j], poly[(j + 1) % n]),
                            $"Case #{tc + 1} edges {i} and {j} intersect");
                    }
                }

                long polyArea2 = Math.Abs(ShoelaceTimes2(poly));
                List<Point2L> hull = Geometry.ConvexHull(pts);
                long hullArea2 = Math.Abs(ShoelaceTimes2(hull));
                Assert.IsTrue(2L * polyArea2 > hullArea2, $"Case #{tc + 1} area {polyArea2}/2 <= hull {hullArea2}/4");
            }
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, ValidateQ2);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, ValidateQ2);
        }

        [Test]
        public void Q2Large()
        {
            SlowTest(Q2Solver, ValidateQ2);
        }

        #endregion

        #region Question 3 - Are We Lost Yet?

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m, out int p);
            int[] u = new int[m + 1];
            int[] v = new int[m + 1];
            int[] a = new int[m + 1];
            int[] b = new int[m + 1];
            for (int i = 1; i <= m; i++)
                test.Get(out u[i], out v[i], out a[i], out b[i]);
            test.Get(out int[] path);
            return () => Q3Solver(n, m, p, u, v, a, b, path);
        }

        private static string Q3Solver(int n, int m, int p, int[] u, int[] v, int[] a, int[] b, int[] path)
        {
            List<int>[] adj = new List<int>[n + 1];
            for (int i = 1; i <= n; i++)
                adj[i] = [];
            for (int eid = 1; eid <= m; eid++)
                adj[u[eid]].Add(eid);

            bool[] inPrefix = new bool[m + 1];
            long[] distKey = new long[n + 1];
            PriorityQueue<int, long> pq = new();

            bool IsFeasible(int prefixLen)
            {
                if (prefixLen == 0)
                    return true;

                Array.Clear(inPrefix);
                long lowSum = 0;
                for (int i = 0; i < prefixLen; i++)
                {
                    int eid = path[i];
                    inPrefix[eid] = true;
                    lowSum += a[eid];
                }
                int target = v[path[prefixLen - 1]];

                const long Inf = long.MaxValue / 4;
                Array.Fill(distKey, Inf);
                pq.Clear();

                distKey[1] = 1L;
                pq.Enqueue(1, 1L);

                long goodKey = 2L * lowSum;
                if (goodKey < distKey[target])
                {
                    distKey[target] = goodKey;
                    pq.Enqueue(target, goodKey);
                }

                while (pq.TryDequeue(out int city, out long key))
                {
                    if (key != distKey[city])
                        continue;
                    if (city == 2)
                        return (key & 1L) == 0;

                    bool isGood = (key & 1L) == 0;
                    foreach (int eid in adj[city])
                    {
                        int next = v[eid];
                        long w = (isGood || inPrefix[eid]) ? a[eid] : b[eid];
                        long nextKey = key + 2L * w;
                        if (nextKey < distKey[next])
                        {
                            distKey[next] = nextKey;
                            pq.Enqueue(next, nextKey);
                        }
                    }
                }

                return false;
            }

            int lo = 0;
            int hi = p;
            int bestP = 0;
            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (IsFeasible(mid))
                {
                    bestP = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return bestP == p ? "Looks Good To Me" : path[bestP].ToString(CultureInfo.InvariantCulture);
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
            Test(Q3Solver);
        }

        #endregion

        #region Question 4 - Observation Wheel

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out string s);
            return () => Q4Solver(s).ToString("G17", CultureInfo.InvariantCulture);
        }

        private static double Q4Solver(string s)
        {
            int n = s.Length;
            int[] emptyPref = new int[2 * n + 1];
            for (int i = 0; i < 2 * n; i++)
                emptyPref[i + 1] = emptyPref[i] + (s[i % n] == '.' ? 1 : 0);

            int EmptyCount(int start, int len) => emptyPref[start + len] - emptyPref[start];

            double[,] comb = new double[n + 1, n + 1];
            for (int i = 0; i <= n; i++)
            {
                comb[i, 0] = 1.0;
                comb[i, i] = 1.0;
                for (int j = 1; j < i; j++)
                    comb[i, j] = comb[i - 1, j - 1] + comb[i - 1, j];
            }

            double[,] prob = new double[n, n];
            double[,] sumE = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int len = 0; len < n; len++)
                {
                    if (EmptyCount(i, len) == 0)
                        prob[i, len] = 1.0;
                }
            }

            for (int len = 1; len < n; len++)
            {
                double invDenom = 1.0 / (len + 1);
                for (int i = 0; i < n; i++)
                {
                    int end = (i + len) % n;
                    if (s[end] != '.' || EmptyCount(i, len) == 0)
                        continue;

                    double accP = 0.0;
                    double accE = 0.0;
                    for (int k = 0; k < len; k++)
                    {
                        int m = (i + k) % n;
                        if (s[m] != '.')
                            continue;

                        int a = EmptyCount(i, k);
                        int b = EmptyCount(m + 1, len - 1 - k);
                        double w = comb[a + b, a]
                            * Math.Pow((k + 1) * invDenom, a + 1)
                            * Math.Pow((len - k) * invDenom, b);

                        int nextStart = (m + 1) % n;
                        int rightLen = len - 1 - k;
                        double pL = prob[i, k];
                        double sL = sumE[i, k];
                        double pR = prob[nextStart, rightLen];
                        double sR = sumE[nextStart, rightLen];

                        double pHere = w * pL * pR;
                        accP += pHere;
                        accE += w * (sL * pR + pL * sR) + pHere * (n - k * 0.5);
                    }

                    prob[i, len] = accP;
                    sumE[i, len] = accE;
                }
            }

            double ans = 0.0;
            for (int i = 0; i < n; i++)
            {
                if (s[i] == '.')
                {
                    int next = (i + 1) % n;
                    ans += sumE[next, n - 1] + prob[next, n - 1] * ((n + 1) * 0.5);
                }
            }

            return ans;
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, 1e-9);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, 1e-9);
        }

        [Test]
        public void Q4Large()
        {
            Test(Q4Solver, 1e-9);
        }

        #endregion
    }
}
