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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Zombie Smash

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int z);
            test.GetLines(z, out (int X, int Y, int M)[] zombies);
            return () => Q1Solver(zombies);
        }

        private static int Q1Solver((int X, int Y, int M)[] zombies)
        {
            int n = zombies.Length;
            const long Inf = long.MaxValue / 4;
            long[] dp = new long[n];
            long[] next = new long[n];
            bool any = false;

            for (int i = 0; i < n; i++)
            {
                long travel = 100L * Math.Max(Math.Abs(zombies[i].X), Math.Abs(zombies[i].Y));
                if (travel <= (long)zombies[i].M + 1000)
                {
                    dp[i] = Math.Max(travel, zombies[i].M);
                    any = true;
                }
                else
                {
                    dp[i] = Inf;
                }
            }

            if (!any)
                return 0;

            int killed = 1;
            while (killed < n)
            {
                Array.Fill(next, Inf);
                bool nextAny = false;
                for (int u = 0; u < n; u++)
                {
                    long curT = dp[u];
                    if (curT >= Inf)
                        continue;

                    for (int v = 0; v < n; v++)
                    {
                        if (u == v)
                            continue;
                        long dist = 100L * Math.Max(Math.Abs(zombies[u].X - zombies[v].X), Math.Abs(zombies[u].Y - zombies[v].Y));
                        long step = Math.Max(750L, dist);
                        long arr = curT + step;
                        if (arr <= (long)zombies[v].M + 1000)
                        {
                            long smashTime = Math.Max(arr, zombies[v].M);
                            if (smashTime < next[v])
                            {
                                next[v] = smashTime;
                                nextAny = true;
                            }
                        }
                    }
                }

                if (!nextAny)
                    break;
                (dp, next) = (next, dp);
                killed++;
            }

            return killed;
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
            Test(Q1Solver);
        }

        #endregion

        #region Question 2 - Upstairs/Downstairs

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.GetLines(n, out string[] lines);
            return () => Q2Solver(k, lines);
        }

        private static string Q2Solver(int k, string[] lines)
        {
            if (k <= 1)
                return 0.0.ToString("F9", CultureInfo.InvariantCulture);

            int n = lines.Length;
            (long A, long B, long C)[] acts = new (long, long, long)[n];
            for (int i = 0; i < n; i++)
            {
                string[] parts = lines[i].Split(['/', ' '], StringSplitOptions.RemoveEmptyEntries);
                acts[i] = (
                    long.Parse(parts[0], CultureInfo.InvariantCulture),
                    long.Parse(parts[1], CultureInfo.InvariantCulture),
                    long.Parse(parts[2], CultureInfo.InvariantCulture));
            }

            Array.Sort(acts, static (x, y) => (y.A * x.B).CompareTo(x.A * y.B));

            double[] leftP = new double[k];
            double[] leftQ = new double[k];
            int filled = 0;
            for (int i = 0; i < n && filled < k; i++)
            {
                double p = (double)acts[i].A / acts[i].B;
                double q = (double)(acts[i].B - acts[i].A) / acts[i].B;
                long take = Math.Min(k - filled, acts[i].C);
                for (int t = 0; t < take; t++)
                {
                    leftP[filled] = p;
                    leftQ[filled] = q;
                    filled++;
                }
            }

            double[] rightP = new double[k];
            double[] rightQ = new double[k];
            filled = k;
            for (int i = n - 1; i >= 0 && filled > 0; i--)
            {
                double p = (double)acts[i].A / acts[i].B;
                double q = (double)(acts[i].B - acts[i].A) / acts[i].B;
                long take = Math.Min(filled, acts[i].C);
                for (int t = 0; t < take; t++)
                {
                    filled--;
                    rightP[filled] = p;
                    rightQ[filled] = q;
                }
            }

            double[] prefA = new double[k + 1];
            double[] prefS = new double[k + 1];
            double[] prefW = new double[k + 1];
            prefA[0] = 1.0;
            for (int i = 0; i < k; i++)
            {
                double p = leftP[i];
                double qs = leftQ[i];
                prefW[i + 1] = prefW[i] + prefS[i] * p;
                prefS[i + 1] = (prefA[i] + prefS[i]) * qs;
                prefA[i + 1] = prefA[i] * p;
            }

            double[] sufA = new double[k + 1];
            double[] sufS = new double[k + 1];
            for (int q = 0; q < k; q++)
            {
                double p = rightP[k - 1 - q];
                double qs = rightQ[k - 1 - q];
                sufA[q + 1] = p * sufA[q] + qs * sufS[q];
                sufS[q + 1] = p + qs * sufS[q];
            }

            double best = 1.0;
            for (int q = 0; q <= k; q++)
            {
                int t = k - q;
                double probWake = prefW[t] + prefA[t] * sufA[q] + prefS[t] * sufS[q];
                if (probWake < best)
                    best = probWake;
            }

            return best.ToString("F9", CultureInfo.InvariantCulture);
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, 1e-6);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, 1e-6);
        }

        [Test]
        public void Q2Large()
        {
            SlowTest(Q2Solver, 1e-6);
        }

        #endregion

        #region Question 3 - Xeno-archaeology

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out long[] xs, out long[] ys, out string[] colorTokens);
            char[] colors = new char[n];
            for (int i = 0; i < n; i++)
                colors[i] = colorTokens[i][0];
            return () => Q3Solver(n, xs, ys, colors);
        }

        private static long CeilMod4(long a, int r) => a + ((r - (int)(a & 3)) & 3);
        private static long FloorMod4(long a, int r) => a - (((int)(a & 3) - r) & 3);

        private static long MinAbsMod4(long lo, long hi, int r)
        {
            if (lo >= 0) return lo;
            if (hi <= 0) return -hi;
            return Math.Min(r, 4 - r);
        }

        private static string Q3Solver(int n, long[] xs, long[] ys, char[] colors)
        {
            const long Inf = 4_000_000_000_000_000_000L;
            const long BigBase = 4_000_000_000_000_000L;

            bool found = false;
            long bestDist = long.MaxValue;
            long bestX = 0, bestY = 0;

            void ConsiderCandidate(long candX, long candY)
            {
                long dist = Math.Abs(candX) + Math.Abs(candY);
                if (!found || dist < bestDist ||
                    (dist == bestDist && (candX > bestX || (candX == bestX && candY > bestY))))
                {
                    found = true;
                    bestDist = dist;
                    bestX = candX;
                    bestY = candY;
                }
            }

            List<(long U, long V, bool IsTypeX)> constraints = [with(capacity: n)];

            for (int px = 0; px <= 1; px++)
            {
                for (int py = 0; py <= 1; py++)
                {
                    constraints.Clear();
                    bool impossible = false;

                    for (int i = 0; i < n; i++)
                    {
                        int dxp = px ^ (int)(xs[i] & 1);
                        int dyp = py ^ (int)(ys[i] & 1);
                        int col = colors[i] == '#' ? 1 : 0;

                        if (dxp == col && dyp == col)
                            continue;
                        if (dxp != col && dyp != col)
                        {
                            impossible = true;
                            break;
                        }

                        long u = xs[i] + ys[i];
                        long v = xs[i] - ys[i];
                        constraints.Add((u, v, dxp == col));
                    }

                    if (impossible)
                        continue;

                    int pUv = (px + py) & 1;
                    long big = BigBase + pUv;

                    constraints.Sort(static (a, b) => a.U.CompareTo(b.U));

                    List<long> uniqueU = [-Inf];
                    List<(long MaxX, long MinY, long MinX, long MaxY)> groups = [(-Inf, Inf, Inf, -Inf)];
                    for (int i = 0; i < constraints.Count;)
                    {
                        long curU = constraints[i].U;
                        long maxX = -Inf, minY = Inf, minX = Inf, maxY = -Inf;
                        while (i < constraints.Count && constraints[i].U == curU)
                        {
                            long v = constraints[i].V;
                            if (constraints[i].IsTypeX)
                            {
                                if (v > maxX) maxX = v;
                                if (v < minX) minX = v;
                            }
                            else
                            {
                                if (v < minY) minY = v;
                                if (v > maxY) maxY = v;
                            }
                            i++;
                        }
                        uniqueU.Add(curU);
                        groups.Add((maxX, minY, minX, maxY));
                    }
                    uniqueU.Add(Inf);
                    groups.Add((-Inf, Inf, Inf, -Inf));

                    int gCount = uniqueU.Count - 2;
                    long[] prefMaxX = new long[gCount + 2];
                    long[] prefMinY = new long[gCount + 2];
                    prefMaxX[0] = -Inf;
                    prefMinY[0] = Inf;
                    for (int k = 1; k <= gCount; k++)
                    {
                        prefMaxX[k] = Math.Max(prefMaxX[k - 1], groups[k].MaxX);
                        prefMinY[k] = Math.Min(prefMinY[k - 1], groups[k].MinY);
                    }

                    long[] suffMinX = new long[gCount + 2];
                    long[] suffMaxY = new long[gCount + 2];
                    suffMinX[gCount + 1] = Inf;
                    suffMaxY[gCount + 1] = -Inf;
                    for (int k = gCount; k >= 1; k--)
                    {
                        suffMinX[k] = Math.Min(suffMinX[k + 1], groups[k].MinX);
                        suffMaxY[k] = Math.Max(suffMaxY[k + 1], groups[k].MaxY);
                    }

                    for (int k = 0; k <= gCount; k++)
                    {
                        long uLo = uniqueU[k];
                        long uHi = uniqueU[k + 1];
                        long vLo = Math.Max(prefMaxX[k], suffMaxY[k + 1]);
                        long vHi = Math.Min(prefMinY[k], suffMinX[k + 1]);

                        long uMin = uLo == -Inf ? -big : uLo + 1;
                        long uMax = uHi == Inf ? big : uHi - 1;
                        long vMin = vLo == -Inf ? -big : vLo + 1;
                        long vMax = vHi == Inf ? big : vHi - 1;
                        if (uMin > uMax || vMin > vMax)
                            continue;

                        for (int add = 0; add <= 2; add += 2)
                        {
                            int ru = pUv + add;
                            int rv = (2 * px - ru) & 3;
                            long uL = CeilMod4(uMin, ru);
                            long uR = FloorMod4(uMax, ru);
                            if (uL > uR)
                                continue;

                            long vL = CeilMod4(vMin, rv);
                            long vR = FloorMod4(vMax, rv);
                            if (vL > vR)
                                continue;

                            long d = Math.Max(MinAbsMod4(uL, uR, ru), MinAbsMod4(vL, vR, rv));
                            long optU = FloorMod4(Math.Min(uR, d), ru);
                            long optV = FloorMod4(Math.Min(vR, d), rv);
                            ConsiderCandidate((optU + optV) >> 1, (optU - optV) >> 1);
                        }
                    }
                }
            }

            return found ? $"{bestX} {bestY}" : "Too damaged";
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

        #region Question 4 - Twirling Towards Freedom

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int m);
            Point2L[] stars = new Point2L[n];
            for (int i = 0; i < n; i++)
            {
                test.Get(out long x, out long y);
                stars[i] = new Point2L(x, y);
            }
            return () => Q4Solver(m, stars);
        }

        private static Point2L ApplyRotationWeight(Point2L p, int r, long c)
        {
            (long wx, long wy) = r switch
            {
                0 => (p.X - p.Y, p.X + p.Y),
                1 => (p.X + p.Y, p.Y - p.X),
                2 => (p.Y - p.X, -p.X - p.Y),
                _ => (-p.X - p.Y, p.X - p.Y)
            };
            return new Point2L(c * wx, c * wy);
        }

        private static string Q4Solver(int m, Point2L[] stars)
        {
            List<Point2L> hull = Geometry.ConvexHull(stars);
            Int128 bestSq = 0;

            int minSteps = Math.Max(1, m - 3);
            for (int steps = minSteps; steps <= m; steps++)
            {
                ReadOnlySpan<int> counts = [(steps + 3) / 4, (steps + 2) / 4, (steps + 1) / 4, steps / 4];
                List<Point2L>? acc = null;

                for (int r = 0; r < 4; r++)
                {
                    if (counts[r] == 0)
                        continue;

                    List<Point2L> poly = [with(capacity: hull.Count)];
                    foreach (Point2L p in hull)
                        poly.Add(ApplyRotationWeight(p, r, counts[r]));

                    // Each w_r is a positive similarity, so poly already has strict CCW vertex order.
                    acc = acc == null ? poly : Geometry.MinkowskiSum(acc, poly);
                }

                if (acc != null)
                {
                    foreach (Point2L v in acc)
                    {
                        Int128 sq = v.LengthSquared128();
                        if (sq > bestSq)
                            bestSq = sq;
                    }
                }
            }

            return Math.Sqrt((double)bestSq).ToString("F10", CultureInfo.InvariantCulture);
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
            Test(Q4Solver, 1e-6);
        }

        #endregion

        #region Question 5 - Shifting Paths

        private static Func<string> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n - 1, out (int L, int R)[] edges);
            return () => Q5Solver(n, edges);
        }

        private static string Q5Solver(int n, (int L, int R)[] edges)
        {
            int[] left = new int[n + 1];
            int[] right = new int[n + 1];
            List<int>[] rev = new List<int>[n + 1];
            for (int i = 1; i <= n; i++)
                rev[i] = [];

            for (int i = 1; i < n; i++)
            {
                left[i] = edges[i - 1].L;
                right[i] = edges[i - 1].R;
                rev[left[i]].Add(i);
                if (right[i] != left[i])
                    rev[right[i]].Add(i);
            }

            int[] dist = new int[n + 1];
            Array.Fill(dist, -1);
            dist[n] = 0;
            Queue<int> bfs = new();
            bfs.Enqueue(n);
            while (bfs.Count > 0)
            {
                int u = bfs.Dequeue();
                foreach (int v in rev[u])
                {
                    if (dist[v] < 0)
                    {
                        dist[v] = dist[u] + 1;
                        bfs.Enqueue(v);
                    }
                }
            }

            if (dist[1] < 0)
                return "Infinity";

            List<int> reachable = [];
            for (int i = 1; i < n; i++)
            {
                if (dist[i] >= 0)
                    reachable.Add(i);
            }
            reachable.Sort((a, b) => dist[a].CompareTo(dist[b]));

            int numB = (reachable.Count + 1) / 2;
            int numA = reachable.Count - numB;

            int[] aIdx = new int[n + 1];
            int[] bIdx = new int[n + 1];
            Array.Fill(aIdx, -1);
            Array.Fill(bIdx, -1);
            int[] aNodes = new int[numA];

            for (int i = 0; i < numB; i++)
                bIdx[reachable[i]] = i;
            for (int i = 0; i < numA; i++)
            {
                int node = reachable[numB + i];
                aIdx[node] = i;
                aNodes[i] = node;
            }

            int[] dpSteps = [];
            int[] dpExitAndMask = [];
            int[] stack = [];
            if (numA > 0)
            {
                int totalStates = numA << numA;
                dpSteps = new int[totalStates];
                dpExitAndMask = new int[totalStates];
                Array.Fill(dpSteps, -1);
                stack = new int[totalStates];
            }

            const int MaskBits = 19;
            const int MaskLow = (1 << MaskBits) - 1;

            int cur = 1;
            int maskA = 0;
            int stateB = 0;
            long totalSteps = 0;

            while (cur != n)
            {
                if (dist[cur] < 0)
                    return "Infinity";

                int ai = aIdx[cur];
                if (ai >= 0)
                {
                    int initKey = (ai << numA) | maskA;
                    int exitNode;
                    int exitMaskA;
                    int sp = 0;
                    int cNode = cur;
                    int cMask = maskA;

                    while (true)
                    {
                        int cAi = aIdx[cNode];
                        if (cAi < 0)
                        {
                            exitNode = cNode;
                            exitMaskA = cMask;
                            break;
                        }

                        int key = (cAi << numA) | cMask;
                        if (dpSteps[key] >= 0)
                        {
                            int packed = dpExitAndMask[key];
                            exitNode = packed >> MaskBits;
                            exitMaskA = packed & MaskLow;
                            break;
                        }

                        stack[sp++] = key;
                        bool isRight = ((cMask >> cAi) & 1) != 0;
                        cMask ^= 1 << cAi;
                        cNode = isRight ? right[cNode] : left[cNode];
                    }

                    int steps = sp > 0 && aIdx[cNode] >= 0
                        ? dpSteps[(aIdx[cNode] << numA) | cMask]
                        : 0;
                    int packedOut = (exitNode << MaskBits) | exitMaskA;
                    for (int p = sp - 1; p >= 0; p--)
                    {
                        steps++;
                        int stKey = stack[p];
                        dpSteps[stKey] = steps;
                        dpExitAndMask[stKey] = packedOut;
                    }

                    totalSteps += dpSteps[initKey];
                    cur = exitNode;
                    maskA = exitMaskA;
                }
                else
                {
                    int bi = bIdx[cur];
                    bool isRight = ((stateB >> bi) & 1) != 0;
                    stateB ^= 1 << bi;
                    cur = isRight ? right[cur] : left[cur];
                    totalSteps++;
                }
            }

            return totalSteps.ToString(CultureInfo.InvariantCulture);
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Large()
        {
            Test(Q5Solver);
        }

        #endregion
    }
}
