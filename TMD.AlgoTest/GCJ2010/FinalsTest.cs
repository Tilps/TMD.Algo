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
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Letter Stamper

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out string s);
            return () => Q1Solver(s);
        }

        private static int Q1Solver(string s)
        {
            List<int> runs = [s[0] - 'A'];
            for (int i = 1; i < s.Length; i++)
            {
                int ch = s[i] - 'A';
                if (ch != runs[^1])
                    runs.Add(ch);
            }

            int m = runs.Count;
            const int Inf = 1_000_000_000;
            int[] dp0 = new int[m + 3];
            int[] dp1 = new int[m + 3];
            int[] next0 = new int[m + 3];
            int[] next1 = new int[m + 3];
            Array.Fill(dp0, Inf);
            Array.Fill(dp1, Inf);
            dp0[1] = 1;
            dp1[1] = 1;

            int curMaxH = 1;
            for (int i = 1; i < m; i++)
            {
                int prev = runs[i - 1];
                int cur = runs[i];
                bool isFwdZero = cur == (prev + 1) % 3;

                // Free 3-pops reduce height modulo 3 without new pushes.
                for (int h = curMaxH; h >= 4; h--)
                {
                    if (dp0[h] < dp0[h - 3]) dp0[h - 3] = dp0[h];
                    if (dp1[h] < dp1[h - 3]) dp1[h - 3] = dp1[h];
                }

                int minAll = Inf;
                for (int h = 1; h <= curMaxH; h++)
                {
                    if (dp0[h] < minAll) minAll = dp0[h];
                    if (dp1[h] < minAll) minAll = dp1[h];
                }

                int nextMaxH = curMaxH + 1;
                for (int h = 0; h <= nextMaxH + 1; h++)
                {
                    next0[h] = Inf;
                    next1[h] = Inf;
                }

                // Pop entire stack and push cur cleanly as [cur] at height 1.
                next0[1] = minAll + 1;
                next1[1] = minAll + 1;

                int[] fwd = isFwdZero ? dp0 : dp1;
                int[] bwd = isFwdZero ? dp1 : dp0;
                int[] nextFwd = isFwdZero ? next0 : next1;
                int[] nextBwd = isFwdZero ? next1 : next0;

                // Switch cycle orientation at height 2 by popping top and pushing cur.
                if (curMaxH >= 2)
                    nextBwd[2] = Math.Min(nextBwd[2], fwd[2] + 1);

                for (int h = 1; h <= curMaxH; h++)
                {
                    int val = fwd[h];
                    if (val < Inf)
                    {
                        if (val + 1 < nextFwd[h + 1])
                            nextFwd[h + 1] = val + 1;
                        if (h >= 3 && val < nextFwd[h - 2])
                            nextFwd[h - 2] = val;
                    }
                }

                for (int h = 2; h <= curMaxH; h++)
                {
                    int val = bwd[h];
                    if (val < Inf && val < nextBwd[h - 1])
                        nextBwd[h - 1] = val;
                }

                int minH1 = Math.Min(next0[1], next1[1]);
                next0[1] = minH1;
                next1[1] = minH1;

                (dp0, next0) = (next0, dp0);
                (dp1, next1) = (next1, dp1);
                curMaxH = nextMaxH;
            }

            int minPushes = Inf;
            for (int h = 1; h <= curMaxH; h++)
            {
                if (dp0[h] < minPushes) minPushes = dp0[h];
                if (dp1[h] < minPushes) minPushes = dp1[h];
            }

            return s.Length + 2 * minPushes;
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

        #region Question 2 - City Tour

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n - 3, out (int A, int B)[] attach);
            return () => Q2Solver(n, attach);
        }

        private static int Q2Solver(int n, (int A, int B)[] attach)
        {
            Dictionary<(int U, int V), List<int>> children = [];
            List<(int U, int V)> createdEdges = [(1, 2), (1, 3), (2, 3)];
            foreach (var e in createdEdges)
                children[e] = [];

            // Treat vertex 3 as attached to initial edge (1, 2) so all downward cycles roll up to (1, 2).
            children[(1, 2)].Add(3);

            for (int w = 4; w <= n; w++)
            {
                var (rawA, rawB) = attach[w - 4];
                var parentEdge = rawA < rawB ? (rawA, rawB) : (rawB, rawA);
                children[parentEdge].Add(w);

                var e1 = (rawA, w);
                var e2 = (rawB, w);
                children[e1] = [];
                children[e2] = [];
                createdEdges.Add(e1);
                createdEdges.Add(e2);
            }

            int[,] bestDown = new int[n + 1, n + 1];
            int bestCycle = 3;

            for (int idx = createdEdges.Count - 1; idx >= 0; idx--)
            {
                var (u, v) = createdEdges[idx];
                int max1 = 2;
                int max2 = -1;

                foreach (int w in children[(u, v)])
                {
                    int len1 = u < w ? bestDown[u, w] : bestDown[w, u];
                    int len2 = v < w ? bestDown[v, w] : bestDown[w, v];
                    int len = len1 + len2 - 1;
                    if (len > max1)
                    {
                        max2 = max1;
                        max1 = len;
                    }
                    else if (len > max2)
                    {
                        max2 = len;
                    }
                }

                bestDown[u, v] = max1;
                if (max1 > bestCycle)
                    bestCycle = max1;
                if (max2 >= 2 && max1 + max2 - 2 > bestCycle)
                    bestCycle = max1 + max2 - 2;
            }

            return bestCycle;
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

        #region Question 3 - Candy Store

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out long k, out long c);
            return () => Q3Solver(k, c);
        }

        private static long Q3Solver(long k, long c)
        {
            long target = k * c;
            long sum = 0;
            long boxes = 0;
            while (sum < target)
            {
                boxes++;
                sum += sum / k + 1;
            }
            return boxes;
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

        #region Question 4 - Travel Plan

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out long[] coords);
            test.Get(out long fuel);
            return () => Q4Solver(n, coords, fuel);
        }

        private static long[] MergeThreeShifted(long[]? a0, long[]? a1, long[]? a2, long offset, long maxAllowed)
        {
            int n0 = a0?.Length ?? 0;
            int n1 = a1?.Length ?? 0;
            int n2 = a2?.Length ?? 0;
            int cap = n0 + n1 + n2;
            if (cap == 0)
                return [];

            List<long> res = [with(capacity: cap)];
            int i0 = 0, i1 = 0, i2 = 0;
            long last = long.MinValue;

            while (i0 < n0 || i1 < n1 || i2 < n2)
            {
                long v0 = i0 < n0 ? a0![i0] : long.MaxValue;
                long v1 = i1 < n1 ? a1![i1] : long.MaxValue;
                long v2 = i2 < n2 ? a2![i2] : long.MaxValue;

                long minRaw = Math.Min(v0, Math.Min(v1, v2));
                if (minRaw > maxAllowed - offset)
                    break;

                if (v0 == minRaw) i0++;
                if (v1 == minRaw) i1++;
                if (v2 == minRaw) i2++;

                long sum = minRaw + offset;
                if (sum != last)
                {
                    res.Add(sum);
                    last = sum;
                }
            }

            return [.. res];
        }

        private static string Q4Solver(int n, long[] coords, long fuel)
        {
            Array.Sort(coords);
            long span = coords[n - 1] - coords[0];
            if (2 * span > fuel)
                return "NO SOLUTION";

            int m = n - 1;
            long[] d = new long[m];
            for (int i = 0; i < m; i++)
                d[i] = coords[i + 1] - coords[i];

            if (m == 1)
                return (2 * d[0]).ToString();

            long halfF = fuel / 2;
            int mid = (m - 1) / 2;

            // Prefix/suffix min sums remaining if all future crossings are >= 1.
            long[] suffixMinFromLeft = new long[m + 1];
            for (int i = m - 1; i >= 0; i--)
                suffixMinFromLeft[i] = suffixMinFromLeft[i + 1] + d[i];

            long[] prefixMinFromRight = new long[m];
            prefixMinFromRight[0] = d[0];
            for (int i = 1; i < m; i++)
                prefixMinFromRight[i] = prefixMinFromRight[i - 1] + d[i];

            long[][] left = new long[m + 2][];
            left[1] = [d[0]];
            int maxLeftC = 1;

            for (int j = 1; j <= mid; j++)
            {
                int nextMax = Math.Min(j + 1, m - j);
                long[][] next = new long[nextMax + 2][];
                long cap = halfF - suffixMinFromLeft[j + 1];
                for (int c = 1; c <= nextMax; c++)
                {
                    long[]? p0 = c - 1 >= 1 && c - 1 <= maxLeftC ? left[c - 1] : null;
                    long[]? p1 = c <= maxLeftC ? left[c] : null;
                    long[]? p2 = c + 1 <= maxLeftC ? left[c + 1] : null;
                    next[c] = MergeThreeShifted(p0, p1, p2, c * d[j], cap);
                }
                left = next;
                maxLeftC = nextMax;
            }

            long[][] right = new long[m + 2][];
            right[1] = [d[m - 1]];
            int maxRightC = 1;

            for (int j = m - 2; j >= mid + 1; j--)
            {
                int nextMax = Math.Min(m - j, j + 1);
                long[][] next = new long[nextMax + 2][];
                long cap = halfF - prefixMinFromRight[j - 1];
                for (int c = 1; c <= nextMax; c++)
                {
                    long[]? p0 = c - 1 >= 1 && c - 1 <= maxRightC ? right[c - 1] : null;
                    long[]? p1 = c <= maxRightC ? right[c] : null;
                    long[]? p2 = c + 1 <= maxRightC ? right[c + 1] : null;
                    next[c] = MergeThreeShifted(p0, p1, p2, c * d[j], cap);
                }
                right = next;
                maxRightC = nextMax;
            }

            long bestHalf = span;
            for (int cL = 1; cL <= maxLeftC; cL++)
            {
                long[]? a = left[cL];
                if (a == null || a.Length == 0)
                    continue;

                for (int cR = Math.Max(1, cL - 1); cR <= Math.Min(maxRightC, cL + 1); cR++)
                {
                    long[]? b = right[cR];
                    if (b == null || b.Length == 0)
                        continue;

                    int ia = 0;
                    int ib = b.Length - 1;
                    while (ia < a.Length && ib >= 0)
                    {
                        long total = a[ia] + b[ib];
                        if (total <= halfF)
                        {
                            if (total > bestHalf)
                                bestHalf = total;
                            if (bestHalf == halfF)
                                return (2 * bestHalf).ToString();
                            ia++;
                        }
                        else
                        {
                            ib--;
                        }
                    }
                }
            }

            return (2 * bestHalf).ToString();
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver);
        }

        #endregion

        #region Question 5 - Ninjutsu

        private readonly record struct RayTarget(int TargetId, double Dist);

        private sealed class RayGroup
        {
            public long Dx { get; }
            public long Dy { get; }
            public RayTarget[] Targets { get; }
            public double MinDist { get; }

            public RayGroup(long dx, long dy, RayTarget[] targets)
            {
                Dx = dx;
                Dy = dy;
                Targets = targets;
                MinDist = targets[0].Dist;
            }
        }

        private sealed class AnchorRays
        {
            public RayGroup[] Rays { get; }
            private readonly double[] treeMin;
            private readonly int treeSize;

            public AnchorRays(RayGroup[] rays)
            {
                Rays = rays;
                int count = rays.Length;
                int p = 1;
                while (p < count)
                    p <<= 1;
                treeSize = p;
                treeMin = new double[2 * p];
                Array.Fill(treeMin, double.PositiveInfinity);
                for (int i = 0; i < count; i++)
                    treeMin[p + i] = rays[i].MinDist;
                for (int i = p - 1; i >= 1; i--)
                    treeMin[i] = Math.Min(treeMin[i << 1], treeMin[(i << 1) | 1]);
            }

            public int FindFirstInRange(int ql, int qr, double maxAllowed)
            {
                if (ql > qr || ql >= Rays.Length)
                    return -1;
                return FindFirstRecursive(1, 0, treeSize - 1, ql, Math.Min(qr, Rays.Length - 1), maxAllowed);
            }

            private int FindFirstRecursive(int node, int nl, int nr, int ql, int qr, double maxAllowed)
            {
                if (nr < ql || nl > qr || treeMin[node] > maxAllowed)
                    return -1;
                if (nl == nr)
                    return nl;
                int mid = (nl + nr) >> 1;
                int leftRes = FindFirstRecursive(node << 1, nl, mid, ql, qr, maxAllowed);
                return leftRes >= 0 ? leftRes : FindFirstRecursive((node << 1) | 1, mid + 1, nr, ql, qr, maxAllowed);
            }

            public int FindNextRay(long dx, long dy, double maxAllowed, out bool exactFound, out int exactIdx)
            {
                int lo = 0;
                int hi = Rays.Length;
                while (lo < hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (ComparePolarDir(Rays[mid].Dx, Rays[mid].Dy, dx, dy) < 0)
                        lo = mid + 1;
                    else
                        hi = mid;
                }

                exactFound = lo < Rays.Length && ComparePolarDir(Rays[lo].Dx, Rays[lo].Dy, dx, dy) == 0;
                exactIdx = exactFound ? lo : -1;

                int firstStart = exactFound ? lo + 1 : lo;
                int res = FindFirstInRange(firstStart, Rays.Length - 1, maxAllowed);
                if (res >= 0)
                    return res;
                return FindFirstInRange(0, lo - 1, maxAllowed);
            }

            public static int ComparePolarDir(long ax, long ay, long bx, long by)
            {
                bool ua = ay > 0 || (ay == 0 && ax > 0);
                bool ub = by > 0 || (by == 0 && bx > 0);
                if (ua != ub)
                    return ua ? -1 : 1;

                Int128 cross = (Int128)ax * by - (Int128)ay * bx;
                if (cross > 0)
                    return -1;
                if (cross < 0)
                    return 1;
                return 0;
            }
        }

        private static Func<long> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n, out long r);
            test.GetLines(n, out (long X, long Y)[] pts);
            return () => Q5Solver(n, r, pts);
        }

        private static long Q5Solver(int n, long r, (long X, long Y)[] pts)
        {
            if (n <= 1)
                return 0;

            AnchorRays[] anchors = new AnchorRays[n];
            var rawEntries = new (long Dx, long Dy, UInt128 DistSq, int Target)[n - 1];

            for (int p = 0; p < n; p++)
            {
                int idx = 0;
                for (int q = 0; q < n; q++)
                {
                    if (q == p)
                        continue;
                    long dx = pts[q].X - pts[p].X;
                    long dy = pts[q].Y - pts[p].Y;
                    UInt128 distSq = (UInt128)(dx * (Int128)dx + dy * (Int128)dy);
                    rawEntries[idx++] = (dx, dy, distSq, q);
                }

                Array.Sort(rawEntries, static (a, b) =>
                {
                    int cmp = AnchorRays.ComparePolarDir(a.Dx, a.Dy, b.Dx, b.Dy);
                    return cmp != 0 ? cmp : a.DistSq.CompareTo(b.DistSq);
                });

                List<RayGroup> groups = [];
                int start = 0;
                while (start < rawEntries.Length)
                {
                    int end = start + 1;
                    while (end < rawEntries.Length &&
                           AnchorRays.ComparePolarDir(rawEntries[start].Dx, rawEntries[start].Dy, rawEntries[end].Dx, rawEntries[end].Dy) == 0)
                    {
                        end++;
                    }

                    RayTarget[] targets = new RayTarget[end - start];
                    for (int t = start; t < end; t++)
                    {
                        double dist = Math.Sqrt((double)rawEntries[t].DistSq);
                        targets[t - start] = new RayTarget(rawEntries[t].Target, dist);
                    }
                    groups.Add(new RayGroup(rawEntries[start].Dx, rawEntries[start].Dy, targets));
                    start = end;
                }

                anchors[p] = new AnchorRays([.. groups]);
            }

            const double HitMargin = 0.5;
            Dictionary<int, long> cutMemo = [];

            static int PickFarthestReachable(RayTarget[] targets, double maxAllowed, int upperExclusive)
            {
                int lo = 0;
                int hi = upperExclusive - 1;
                int ans = -1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (targets[mid].Dist <= maxAllowed)
                    {
                        ans = mid;
                        lo = mid + 1;
                    }
                    else
                    {
                        hi = mid - 1;
                    }
                }
                return ans;
            }

            long SolveCut(int p, int q)
            {
                int key = p * n + q;
                if (cutMemo.TryGetValue(key, out long cached))
                    return cached;

                long dx = pts[q].X - pts[p].X;
                long dy = pts[q].Y - pts[p].Y;
                double cutLen = Math.Sqrt((double)((Int128)dx * dx + (Int128)dy * dy));
                long res = Simulate(p, dx, dy, cutLen, isCutStart: true, cutTarget: q);
                cutMemo[key] = res;
                return res;
            }

            long Simulate(int startP, long startDx, long startDy, double startLen, bool isCutStart, int cutTarget)
            {
                int curP = startP;
                long curDx = startDx;
                long curDy = startDy;
                double consumed = 0.0;
                long bends = 0;
                long best = 0;

                Dictionary<int, int> seenStep = [];
                List<double> stepLengths = [];
                bool checkSameRayFirst = isCutStart;

                while (true)
                {
                    double remLen = startLen - consumed;
                    double maxAllowed = remLen - HitMargin;
                    if (maxAllowed <= 0.0)
                        break;

                    AnchorRays anchor = anchors[curP];
                    int nextRayIdx = anchor.FindNextRay(curDx, curDy, maxAllowed, out bool exactFound, out int exactIdx);
                    int pickedTargetIdx = -1;
                    int chosenRayIdx = nextRayIdx;

                    if (checkSameRayFirst)
                    {
                        checkSameRayFirst = false;
                        if (exactFound)
                        {
                            RayTarget[] sameTargets = anchor.Rays[exactIdx].Targets;
                            int cutIdx = -1;
                            for (int i = 0; i < sameTargets.Length; i++)
                            {
                                if (sameTargets[i].TargetId == cutTarget)
                                {
                                    cutIdx = i;
                                    break;
                                }
                            }
                            if (cutIdx > 0)
                            {
                                int innerIdx = PickFarthestReachable(sameTargets, maxAllowed, cutIdx);
                                if (innerIdx >= 0)
                                {
                                    chosenRayIdx = exactIdx;
                                    pickedTargetIdx = innerIdx;
                                }
                            }
                        }
                    }

                    if (pickedTargetIdx < 0)
                    {
                        if (nextRayIdx < 0)
                            break;
                        chosenRayIdx = nextRayIdx;
                        RayTarget[] targets = anchor.Rays[chosenRayIdx].Targets;
                        pickedTargetIdx = PickFarthestReachable(targets, maxAllowed, targets.Length);
                        if (pickedTargetIdx < 0)
                            break;
                    }

                    RayTarget hit = anchor.Rays[chosenRayIdx].Targets[pickedTargetIdx];
                    int nextQ = hit.TargetId;
                    double segLen = hit.Dist;

                    long withCut = bends + SolveCut(curP, nextQ);
                    if (withCut > best)
                        best = withCut;

                    int edgeKey = curP * n + nextQ;
                    if (seenStep.TryGetValue(edgeKey, out int prevStep))
                    {
                        int loopBends = stepLengths.Count - prevStep;
                        double loopPerim = 0.0;
                        for (int s = prevStep; s < stepLengths.Count; s++)
                            loopPerim += stepLengths[s];

                        long laps = (long)Math.Floor((remLen - HitMargin) / loopPerim) - 1;
                        if (laps > 0)
                        {
                            bends += laps * loopBends;
                            consumed += laps * loopPerim;
                            seenStep.Clear();
                            stepLengths.Clear();
                            continue;
                        }
                    }

                    seenStep[edgeKey] = stepLengths.Count;
                    stepLengths.Add(segLen);

                    consumed += segLen;
                    bends++;
                    if (bends > best)
                        best = bends;

                    curDx = pts[nextQ].X - pts[curP].X;
                    curDy = pts[nextQ].Y - pts[curP].Y;
                    curP = nextQ;
                }

                return best;
            }

            return Simulate(0, -1, 0, r, isCutStart: false, cutTarget: -1);
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
            SlowTest(Q5Solver);
        }

        #endregion

        #region Question 6 - The Paths of Yin Yang

        private static Func<long> Q6Solver(GCJTestCase test)
        {
            test.Get(out int rows, out int cols);
            return () => Q6Solver(rows, cols);
        }

        private static long Q6Solver(int rows, int cols)
        {
            int perim = 2 * rows + 2 * cols - 4;
            (int R, int C)[] perimCoords = new (int, int)[perim];
            int[,] perimIndex = new int[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    perimIndex[r, c] = -1;

            int pIdx = 0;
            for (int c = 0; c < cols; c++) { perimCoords[pIdx] = (0, c); perimIndex[0, c] = pIdx++; }
            for (int r = 1; r < rows; r++) { perimCoords[pIdx] = (r, cols - 1); perimIndex[r, cols - 1] = pIdx++; }
            for (int c = cols - 2; c >= 0; c--) { perimCoords[pIdx] = (rows - 1, c); perimIndex[rows - 1, c] = pIdx++; }
            for (int r = rows - 2; r >= 1; r--) { perimCoords[pIdx] = (r, 0); perimIndex[r, 0] = pIdx++; }

            int TransformEdge(int edgeIdx, int sym)
            {
                var (r1, c1) = perimCoords[edgeIdx];
                var (r2, c2) = perimCoords[(edgeIdx + 1) % perim];

                (int, int) Apply((int R, int C) pt)
                {
                    int rr = pt.R;
                    int cc = pt.C;
                    if ((sym & 1) != 0) rr = rows - 1 - rr;
                    if ((sym & 2) != 0) cc = cols - 1 - cc;
                    if ((sym & 4) != 0) (rr, cc) = (cc, rr);
                    return (rr, cc);
                }

                var t1 = Apply((r1, c1));
                var t2 = Apply((r2, c2));
                int i1 = perimIndex[t1.Item1, t1.Item2];
                int i2 = perimIndex[t2.Item1, t2.Item2];
                return (i2 - i1 + perim) % perim == 1 ? i1 : i2;
            }

            int symCount = rows == cols ? 8 : 4;
            Dictionary<(int E1, int E2), int> orbits = [];
            for (int e1 = 0; e1 < perim; e1++)
            {
                for (int e2 = e1 + 1; e2 < perim; e2++)
                {
                    (int, int) canon = (e1, e2);
                    for (int sym = 1; sym < symCount; sym++)
                    {
                        int m1 = TransformEdge(e1, sym);
                        int m2 = TransformEdge(e2, sym);
                        var pair = m1 < m2 ? (m1, m2) : (m2, m1);
                        if (pair.Item1 < canon.Item1 || (pair.Item1 == canon.Item1 && pair.Item2 < canon.Item2))
                            canon = pair;
                    }
                    orbits[canon] = orbits.TryGetValue(canon, out int cnt) ? cnt + 1 : 1;
                }
            }

            int totalCells = rows * cols;
            byte[] grid = new byte[totalCells];
            bool[] validD = new bool[rows + cols + 4];
            bool[] validS = new bool[rows + cols + 4];
            int dOffset = cols + 1;
            int[] bndDegreeOffset = [0, 0];

            long totalWays = 0;
            foreach (var kv in orbits)
            {
                long ways = SolveBoundary(kv.Key.E1, kv.Key.E2);
                totalWays += 2L * kv.Value * ways;
            }
            return totalWays;

            long SolveBoundary(int e1, int e2)
            {
                for (int idx = 0; idx < perim; idx++)
                {
                    var (r, c) = perimCoords[idx];
                    grid[r * cols + c] = (byte)(idx > e1 && idx <= e2 ? 1 : 0);
                }

                // Validate corners and accumulate forced corner endpoint counts.
                bndDegreeOffset[0] = 0;
                bndDegreeOffset[1] = 0;

                ReadOnlySpan<int> cornerIndices = [0, cols - 1, cols + rows - 2, 2 * cols + rows - 3];
                foreach (int ci in cornerIndices)
                {
                    var (cr, cc) = perimCoords[ci];
                    byte col = grid[cr * cols + cc];
                    var (pr, pc) = perimCoords[(ci - 1 + perim) % perim];
                    var (nr, nc) = perimCoords[(ci + 1) % perim];
                    int deg = (grid[pr * cols + pc] == col ? 1 : 0) + (grid[nr * cols + nc] == col ? 1 : 0);
                    if (deg == 0)
                        return 0;
                    if (deg == 1)
                    {
                        if (++bndDegreeOffset[col] > 2)
                            return 0;
                    }
                }

                Array.Clear(validD);
                Array.Clear(validS);

                // 4 corners' inward diagonals
                validD[0 + dOffset] = true;
                validD[(rows - 1) - (cols - 1) + dOffset] = true;
                validS[0 + cols - 1] = true;
                validS[rows - 1 + 0] = true;

                // 4 transition vertices around edges e1 and e2
                void AddTransition(int edgeIdx)
                {
                    int idxA = edgeIdx;
                    int idxB = (edgeIdx + 1) % perim;
                    AddOne(perimCoords[idxA], perimCoords[idxB]);
                    AddOne(perimCoords[idxB], perimCoords[idxA]);
                }

                void AddOne((int R, int C) pt, (int R, int C) opp)
                {
                    bool isCorner = (pt.R == 0 || pt.R == rows - 1) && (pt.C == 0 || pt.C == cols - 1);
                    if (isCorner)
                        return;

                    int inR = pt.R == 0 ? 1 : (pt.R == rows - 1 ? -1 : 0);
                    int inC = pt.C == 0 ? 1 : (pt.C == cols - 1 ? -1 : 0);
                    int dr = inR + (pt.R - opp.R);
                    int dc = inC + (pt.C - opp.C);
                    if (dr == dc)
                        validD[pt.R - pt.C + dOffset] = true;
                    else
                        validS[pt.R + pt.C] = true;
                }

                AddTransition(e1);
                AddTransition(e2);

                bool IsGoodSquare(int r, int c)
                {
                    int baseIdx = (r - 1) * cols + (c - 1);
                    byte a = grid[baseIdx];
                    byte b = grid[baseIdx + 1];
                    byte d = grid[baseIdx + cols];
                    byte e = grid[baseIdx + cols + 1];
                    int sum = a + b + d + e;
                    return sum != 0 && sum != 4 && (a != e || b != d);
                }

                bool CheckBoundaryNeighbor(int bndPos, int n1, int n2, byte placedVal, ref int end0, ref int end1)
                {
                    byte bndCol = grid[bndPos];
                    int deg = (grid[n1] == bndCol ? 1 : 0) +
                              (grid[n2] == bndCol ? 1 : 0) +
                              (placedVal == bndCol ? 1 : 0);
                    if (deg != 1 && deg != 2)
                        return false;
                    if (deg == 1)
                    {
                        if (bndCol == 0) { if (++end0 > 2) return false; }
                        else { if (++end1 > 2) return false; }
                    }
                    return true;
                }

                bool TryPlaceCell(int r, int c, byte val, ref int end0, ref int end1)
                {
                    grid[r * cols + c] = val;
                    if (!IsGoodSquare(r, c))
                        return false;
                    if (c == cols - 2 && !IsGoodSquare(r, cols - 1))
                        return false;

                    int rowStart = r * cols;
                    if (c == 1 && !CheckBoundaryNeighbor(rowStart, rowStart - cols, rowStart + cols, val, ref end0, ref end1))
                        return false;
                    if (c == cols - 2 && !CheckBoundaryNeighbor(rowStart + cols - 1, rowStart - 1, rowStart + 2 * cols - 1, val, ref end0, ref end1))
                        return false;

                    if (r == rows - 2)
                    {
                        if (!IsGoodSquare(rows - 1, c))
                            return false;
                        if (c == cols - 2 && !IsGoodSquare(rows - 1, cols - 1))
                            return false;
                        int botPos = (rows - 1) * cols + c;
                        if (!CheckBoundaryNeighbor(botPos, botPos - 1, botPos + 1, val, ref end0, ref end1))
                            return false;
                    }

                    return true;
                }

                long BranchCell(int r, int c, byte aboveCol, int end0, int end1)
                {
                    int nr = c == cols - 2 ? r + 1 : r;
                    int nc = c == cols - 2 ? 1 : c + 1;
                    long total = 0;

                    for (byte tryVal = 0; tryVal <= 1; tryVal++)
                    {
                        int ne0 = end0;
                        int ne1 = end1;
                        if (tryVal != aboveCol)
                        {
                            if (aboveCol == 0) { if (++ne0 > 2) continue; }
                            else { if (++ne1 > 2) continue; }
                        }

                        if (TryPlaceCell(r, c, tryVal, ref ne0, ref ne1))
                            total += SearchFrom(nr, nc, ne0, ne1);
                    }

                    return total;
                }

                bool ValidateSolution(int end0, int end1)
                {
                    if (end0 != 2 || end1 != 2)
                        return false;

                    int count0 = 0;
                    int count1 = 0;
                    int start0 = -1;
                    int start1 = -1;

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            int pos = r * cols + c;
                            byte color = grid[pos];
                            if (color == 0) count0++;
                            else count1++;

                            int deg = 0;
                            if (r > 0 && grid[pos - cols] == color) deg++;
                            if (r + 1 < rows && grid[pos + cols] == color) deg++;
                            if (c > 0 && grid[pos - 1] == color) deg++;
                            if (c + 1 < cols && grid[pos + 1] == color) deg++;

                            if (deg != 1 && deg != 2)
                                return false;
                            if (deg == 1)
                            {
                                if (color == 0 && start0 < 0) start0 = pos;
                                else if (color == 1 && start1 < 0) start1 = pos;
                            }
                        }
                    }

                    bool CheckConnected(int startPos, int expectedCount, byte color)
                    {
                        int cur = startPos;
                        int prev = -1;
                        int visited = 0;
                        while (cur >= 0)
                        {
                            visited++;
                            if (visited > expectedCount)
                                return false;
                            int r = cur / cols;
                            int c = cur % cols;
                            int next = -1;
                            if (r > 0 && cur - cols != prev && grid[cur - cols] == color) next = cur - cols;
                            else if (r + 1 < rows && cur + cols != prev && grid[cur + cols] == color) next = cur + cols;
                            else if (c > 0 && cur - 1 != prev && grid[cur - 1] == color) next = cur - 1;
                            else if (c + 1 < cols && cur + 1 != prev && grid[cur + 1] == color) next = cur + 1;
                            prev = cur;
                            cur = next;
                        }
                        return visited == expectedCount;
                    }

                    return CheckConnected(start0, count0, 0) && CheckConnected(start1, count1, 1);
                }

                long SearchFrom(int startR, int startC, int end0, int end1)
                {
                    int r = startR;
                    int c = startC;

                    while (r <= rows - 2)
                    {
                        byte forced;

                        if (r == 1)
                        {
                            byte topCol = grid[c];
                            int topBndSame = (grid[c - 1] == topCol ? 1 : 0) + (grid[c + 1] == topCol ? 1 : 0);
                            if (topBndSame == 2)
                            {
                                forced = (byte)(1 - topCol);
                            }
                            else if (topBndSame == 0)
                            {
                                if (topCol == 0) { if (++end0 > 2) return 0; }
                                else { if (++end1 > 2) return 0; }
                                forced = topCol;
                            }
                            else
                            {
                                return BranchCell(r, c, topCol, end0, end1);
                            }
                        }
                        else
                        {
                            int upPos = (r - 1) * cols + c;
                            byte upCol = grid[upPos];
                            int upDeg = (grid[upPos - cols] == upCol ? 1 : 0) +
                                        (grid[upPos - 1] == upCol ? 1 : 0) +
                                        (grid[upPos + 1] == upCol ? 1 : 0);

                            if (upDeg > 2)
                                return 0;
                            if (upDeg == 2)
                            {
                                forced = (byte)(1 - upCol);
                            }
                            else if (upDeg == 0)
                            {
                                if (!validD[r - 1 - c + dOffset] || !validS[r - 1 + c])
                                    return 0;
                                if (upCol == 0) { if (++end0 > 2) return 0; }
                                else { if (++end1 > 2) return 0; }
                                forced = upCol;
                            }
                            else
                            {
                                if (validD[r - 1 - c + dOffset] && validS[r - 1 + c])
                                    return BranchCell(r, c, upCol, end0, end1);
                                forced = upCol;
                            }
                        }

                        if (!TryPlaceCell(r, c, forced, ref end0, ref end1))
                            return 0;

                        if (c == cols - 2)
                        {
                            r++;
                            c = 1;
                        }
                        else
                        {
                            c++;
                        }
                    }

                    // Validate degrees of final interior row now that its lower boundary neighbors also count.
                    int finalRow = rows - 2;
                    for (int cc = 1; cc <= cols - 2; cc++)
                    {
                        int p = finalRow * cols + cc;
                        byte col = grid[p];
                        int deg = (grid[p - cols] == col ? 1 : 0) +
                                  (grid[p + cols] == col ? 1 : 0) +
                                  (grid[p - 1] == col ? 1 : 0) +
                                  (grid[p + 1] == col ? 1 : 0);
                        if (deg != 1 && deg != 2)
                            return 0;
                        if (deg == 1)
                        {
                            if (!validD[finalRow - cc + dOffset] || !validS[finalRow + cc])
                                return 0;
                            if (col == 0) { if (++end0 > 2) return 0; }
                            else { if (++end1 > 2) return 0; }
                        }
                    }

                    return ValidateSolution(end0, end1) ? 1 : 0;
                }

                return SearchFrom(1, 1, bndDegreeOffset[0], bndDegreeOffset[1]);
            }
        }

        [Test]
        public void Q6Sample()
        {
            Test(Q6Solver);
        }

        [Test]
        public void Q6Small()
        {
            Test(Q6Solver);
        }

        [Test]
        public void Q6Large()
        {
            SlowTest(Q6Solver);
        }

        #endregion
    }
}
