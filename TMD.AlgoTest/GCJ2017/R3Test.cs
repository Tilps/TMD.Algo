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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2017
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Googlements

        private static readonly ConcurrentDictionary<string, long> GooglementCache = new(StringComparer.Ordinal);
        private static readonly long[] Fact9 = [1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880];

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out string g);
            return () => Q1Solver(g);
        }

        private static long Q1Solver(string g)
        {
            if (GooglementCache.TryGetValue(g, out long cached))
                return cached;

            int len = g.Length;
            int[] counts = new int[len + 1];
            int ancSum = 0;
            for (int i = 0; i < len; i++)
            {
                int digit = g[i] - '0';
                counts[i + 1] = digit;
                ancSum += digit;
            }

            if (ancSum > len)
            {
                GooglementCache[g] = 1;
                return 1;
            }

            counts[0] = len - ancSum;

            int nextSum = 0;
            for (int d = 1; d <= len; d++)
                nextSum += d * counts[d];

            if (nextSum > len)
            {
                long ways = Fact9[len];
                for (int d = 0; d <= len; d++)
                    ways /= Fact9[counts[d]];
                long ans = 1 + ways;
                GooglementCache[g] = ans;
                return ans;
            }

            char[] buf = new char[len];
            long total = 1;

            void EnumPerms(int pos)
            {
                if (pos == len)
                {
                    string anc = new(buf);
                    if (!string.Equals(anc, g, StringComparison.Ordinal))
                        total += Q1Solver(anc);
                    return;
                }

                for (int d = 0; d <= len; d++)
                {
                    if (counts[d] > 0)
                    {
                        counts[d]--;
                        buf[pos] = (char)('0' + d);
                        EnumPerms(pos + 1);
                        counts[d]++;
                    }
                }
            }

            EnumPerms(0);
            GooglementCache[g] = total;
            return total;
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

        #region Question 2 - Good News and Bad News

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int f, out int p);
            test.GetLines(p, out int[] u, out int[] v);
            return () => Q2Solver(f, p, u, v);
        }

        private static string Q2Solver(int f, int p, int[] edgeU, int[] edgeV)
        {
            List<(int To, int EdgeId, int DirSign)>[] adj = new List<(int, int, int)>[f + 1];
            for (int i = 1; i <= f; i++)
                adj[i] = [];

            for (int e = 0; e < p; e++)
            {
                int a = edgeU[e];
                int b = edgeV[e];
                adj[a].Add((b, e, 1));  // original a -> b
                adj[b].Add((a, e, -1)); // reversed b -> a
            }

            int[] tin = new int[f + 1];
            int[] parentNode = new int[f + 1];
            int[] parentEdge = new int[f + 1];
            int[] parentSign = new int[f + 1];
            int[] order = new int[f];
            int timer = 0, orderLen = 0;

            bool[] isTreeEdge = new bool[p];
            int[] ans = new int[p];

            // Iterative DFS to build rooted spanning forest without recursion depth limits.
            int[] stackNode = new int[f + 1];
            int[] stackIdx = new int[f + 1];

            for (int root = 1; root <= f; root++)
            {
                if (tin[root] != 0)
                    continue;

                tin[root] = ++timer;
                order[orderLen++] = root;
                int top = 0;
                stackNode[0] = root;
                stackIdx[0] = 0;

                while (top >= 0)
                {
                    int u = stackNode[top];
                    int idx = stackIdx[top];
                    if (idx < adj[u].Count)
                    {
                        stackIdx[top] = idx + 1;
                        var (v, eId, dirSign) = adj[u][idx];
                        if (tin[v] == 0)
                        {
                            tin[v] = ++timer;
                            order[orderLen++] = v;
                            isTreeEdge[eId] = true;
                            parentNode[v] = u;
                            parentEdge[v] = eId;
                            parentSign[v] = dirSign;
                            top++;
                            stackNode[top] = v;
                            stackIdx[top] = 0;
                        }
                    }
                    else
                    {
                        top--;
                    }
                }
            }

            int[] netIn = new int[f + 1]; // received - given
            for (int e = 0; e < p; e++)
            {
                if (isTreeEdge[e])
                    continue;
                int a = edgeU[e];
                int b = edgeV[e];
                // Send +1 along ancestor -> descendant; flip sign if original edge goes descendant -> ancestor.
                if (tin[a] < tin[b])
                {
                    ans[e] = 1;
                    netIn[a]--;
                    netIn[b]++;
                }
                else
                {
                    ans[e] = -1;
                    netIn[b]--;
                    netIn[a]++;
                }
            }

            for (int i = orderLen - 1; i >= 0; i--)
            {
                int v = order[i];
                if (parentNode[v] == 0)
                    continue;

                int flowNeededToParent = netIn[v]; // flow along v -> parentNode[v]
                if (flowNeededToParent == 0)
                    return "IMPOSSIBLE";

                // parentSign[v] == 1 means original edge was parentNode[v] -> v;
                // -1 means original edge was v -> parentNode[v].
                ans[parentEdge[v]] = parentSign[v] == -1 ? flowNeededToParent : -flowNeededToParent;
                netIn[parentNode[v]] += flowNeededToParent;
                netIn[v] = 0;
            }

            return string.Join(" ", ans);
        }

        private static void ValidateGoodNewsBadNews(string input, string expectedOutput, List<string> result)
        {
            char[] sep = ['\r', '\n'];
            string[] inLines = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] expLines = expectedOutput.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0], CultureInfo.InvariantCulture);
            int inIdx = 1;
            Assert.AreEqual(t, result.Count);

            for (int c = 1; c <= t; c++)
            {
                string[] fp = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int f = int.Parse(fp[0], CultureInfo.InvariantCulture);
                int p = int.Parse(fp[1], CultureInfo.InvariantCulture);
                (int A, int B)[] pairs = new (int, int)[p];
                for (int i = 0; i < p; i++)
                {
                    string[] ab = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    pairs[i] = (int.Parse(ab[0], CultureInfo.InvariantCulture) - 1,
                                int.Parse(ab[1], CultureInfo.InvariantCulture) - 1);
                }

                string prefix = $"Case #{c}: ";
                string expVal = expLines[c - 1].Substring(prefix.Length).Trim();
                Assert.IsTrue(result[c - 1].StartsWith(prefix, StringComparison.Ordinal));
                string actVal = result[c - 1].Substring(prefix.Length).Trim();

                if (expVal == "IMPOSSIBLE")
                {
                    Assert.AreEqual("IMPOSSIBLE", actVal, $"Case #{c} expected IMPOSSIBLE");
                    continue;
                }

                Assert.AreNotEqual("IMPOSSIBLE", actVal, $"Case #{c} expected valid assignment");
                string[] tokens = actVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(p, tokens.Length, $"Case #{c} wrong value count");

                long limit = (long)f * f;
                long[] bal = new long[f];
                for (int i = 0; i < p; i++)
                {
                    long v = long.Parse(tokens[i], CultureInfo.InvariantCulture);
                    Assert.IsTrue(v != 0 && Math.Abs(v) <= limit, $"Case #{c} out-of-range value {v}");
                    bal[pairs[i].A] -= v;
                    bal[pairs[i].B] += v;
                }

                for (int friend = 0; friend < f; friend++)
                    Assert.AreEqual(0L, bal[friend], $"Case #{c} friend {friend + 1} unbalanced");
            }
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, ValidateGoodNewsBadNews);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, ValidateGoodNewsBadNews);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, ValidateGoodNewsBadNews);
        }

        #endregion

        #region Question 3 - Mountain Tour

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int c);
            test.GetLines(2 * c, out int[] e, out int[] l, out int[] d);
            return () => Q3Solver(c, e, l, d);
        }

        private static long Q3Solver(int c, int[] endCamp, int[] leaveHour, int[] duration)
        {
            int m = 2 * c;
            int[][] inTours = new int[c][];
            int[] inCount = new int[c];
            for (int i = 0; i < c; i++)
                inTours[i] = new int[2];

            long totalDur = 0;
            int[] arrHour = new int[m];
            for (int t = 0; t < m; t++)
            {
                totalDur += duration[t];
                arrHour[t] = (leaveHour[t] + duration[t]) % 24;
                int dst = endCamp[t] - 1;
                inTours[dst][inCount[dst]++] = t;
            }

            static int WaitTime(int arr, int leave)
            {
                int w = leave - arr;
                return w < 0 ? w + 24 : w;
            }

            long best = long.MaxValue;
            // Camp 0 starts at hour 0 on one departing tour and passes through camp 0 once midway.
            for (int startSlot = 0; startSlot < 2; startSlot++)
            {
                int startOut = startSlot;
                int otherOut = 1 - startSlot;
                int startWait = leaveHour[startOut]; // starting at hour 0

                for (int midSlot = 0; midSlot < 2; midSlot++)
                {
                    int midIn = inTours[0][midSlot];
                    int endIn = inTours[0][1 - midSlot];
                    int midWait = WaitTime(arrHour[midIn], leaveHour[otherOut]);
                    long camp0Cost = startWait + midWait;

                    DisjointTracker<int> dsu = new();
                    for (int t = 0; t < m; t++)
                        dsu.Add(t);

                    dsu.Union(midIn, otherOut);
                    dsu.Union(endIn, startOut);

                    long waitCost = camp0Cost;
                    List<(int A, int B)> switchable = [];

                    for (int camp = 1; camp < c; camp++)
                    {
                        int in0 = inTours[camp][0];
                        int in1 = inTours[camp][1];
                        int out0 = 2 * camp;
                        int out1 = 2 * camp + 1;

                        int cost0 = WaitTime(arrHour[in0], leaveHour[out0]) + WaitTime(arrHour[in1], leaveHour[out1]);
                        int cost1 = WaitTime(arrHour[in0], leaveHour[out1]) + WaitTime(arrHour[in1], leaveHour[out0]);

                        if (cost0 == cost1)
                        {
                            waitCost += cost0;
                            dsu.Union(in0, out0);
                            dsu.Union(in1, out1);
                            dsu.Union(in0, in1);
                        }
                        else if (cost0 < cost1)
                        {
                            waitCost += cost0;
                            dsu.Union(in0, out0);
                            dsu.Union(in1, out1);
                            switchable.Add((in0, in1));
                        }
                        else
                        {
                            waitCost += cost1;
                            dsu.Union(in0, out1);
                            dsu.Union(in1, out0);
                            switchable.Add((in0, in1));
                        }
                    }

                    foreach (var (a, b) in switchable)
                    {
                        if (dsu.Union(a, b))
                            waitCost += 24;
                    }

                    bool singleCycle = true;
                    int root = dsu.GetRepresentative(0);
                    for (int t = 1; t < m; t++)
                    {
                        if (dsu.GetRepresentative(t) != root)
                        {
                            singleCycle = false;
                            break;
                        }
                    }

                    if (singleCycle)
                    {
                        long cand = totalDur + waitCost;
                        if (cand < best)
                            best = cand;
                    }
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

        #region Question 4 - Slate Modern

        private const long SlateMod = 1_000_000_007L;
        private const long Inv2 = 500_000_004L;
        private const long Inv6 = 166_666_668L;

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out long r, out long c, out int n, out long d);
            test.GetLines(n, out long[] ri, out long[] ci, out long[] bi);
            return () => Q4Solver(r, c, n, d, ri, ci, bi);
        }

        private static long FloorDiv(Int128 a, long b)
        {
            Int128 q = a / b;
            Int128 r = a % b;
            if (r < 0) q--;
            return (long)q;
        }

        private static long CeilDiv(Int128 a, long b) => -FloorDiv(-a, b);

        private static long Sum1(long x)
        {
            if (x <= 0) return 0;
            long a = x % SlateMod;
            long b = (x + 1) % SlateMod;
            return a * b % SlateMod * Inv2 % SlateMod;
        }

        private static long Sum2(long x)
        {
            if (x <= 0) return 0;
            long a = x % SlateMod;
            long b = (x + 1) % SlateMod;
            long c = (2 * x + 1) % SlateMod;
            return a * b % SlateMod * c % SlateMod * Inv6 % SlateMod;
        }

        // Sums (c0 + c1 * t) * (v0 + v1 * t) mod SlateMod for t in [lo, hi], working cleanly via shifted u = t - lo.
        private static long SumProdRangeMod(long lo, long hi, long c0, long c1, Int128 v0, long v1, long maxVal)
        {
            if (lo > hi) return 0;
            long len = hi - lo + 1;
            long n = len - 1;
            long cntBase = (c0 + c1 * lo) % SlateMod;
            if (cntBase < 0) cntBase += SlateMod;
            long cntSlope = ((c1 % SlateMod) + SlateMod) % SlateMod;

            long valBase = (long)((v0 + (Int128)v1 * lo) % SlateMod);
            if (valBase < 0) valBase += SlateMod;
            long valSlope = ((v1 % SlateMod) + SlateMod) % SlateMod;

            long coef0 = cntBase * valBase % SlateMod;
            long coef1 = (cntBase * valSlope + cntSlope * valBase) % SlateMod;
            long coef2 = cntSlope * valSlope % SlateMod;

            long res = coef0 * (len % SlateMod) % SlateMod;
            res = (res + coef1 * Sum1(n)) % SlateMod;
            res = (res + coef2 * Sum2(n)) % SlateMod;
            return res;
        }

        private static long SumSegmentOpen(long len, long vLeft, long vRight, long d)
        {
            if (len <= 0) return 0;
            long width = len + 1;
            // k in [1..len]: min(vLeft + k * D, vRight + (width - k) * D)
            long kSplit = Math.Clamp(FloorDiv((Int128)vRight - vLeft + (Int128)width * d, 2 * d), 0, len);
            long ans = 0;

            if (kSplit >= 1)
            {
                long a0 = vLeft % SlateMod;
                long dm = d % SlateMod;
                ans = (ans + (kSplit % SlateMod) * a0 % SlateMod + dm * Sum1(kSplit)) % SlateMod;
            }

            long rem = len - kSplit;
            if (rem >= 1)
            {
                // m = width - k ranges over [1 .. rem]
                long b0 = vRight % SlateMod;
                long dm = d % SlateMod;
                ans = (ans + (rem % SlateMod) * b0 % SlateMod + dm * Sum1(rem)) % SlateMod;
            }

            return ans;
        }

        // Sums min(baseTL + (i+j)*D, baseTR + (i + W1 - j)*D, baseBL + (H1 - i + j)*D, baseBR + (H1 - i + W1 - j)*D)
        // over i in [1..h], j in [1..w] where H1 = h + 1, W1 = w + 1.
        private static long SumInteriorRectangle(long h, long w, long tl, long tr, long bl, long br, long d)
        {
            if (h <= 0 || w <= 0) return 0;
            long h1 = h + 1;
            long w1 = w + 1;

            long jTop = Math.Clamp(FloorDiv((Int128)tr - tl + (Int128)w1 * d, 2 * d), 0, w);
            long jBot = Math.Clamp(FloorDiv((Int128)br - bl + (Int128)w1 * d, 2 * d), 0, w);
            long iLeft = Math.Clamp(FloorDiv((Int128)bl - tl + (Int128)h1 * d, 2 * d), 0, h);
            long iRight = Math.Clamp(FloorDiv((Int128)br - tr + (Int128)h1 * d, 2 * d), 0, h);

            long[] rowCuts = [0, iLeft, iRight, h];
            long[] colCuts = [0, jTop, jBot, w];
            Array.Sort(rowCuts);
            Array.Sort(colCuts);

            long total = 0;
            for (int ri = 0; ri < 3; ri++)
            {
                long iA = rowCuts[ri] + 1;
                long iB = rowCuts[ri + 1];
                if (iA > iB) continue;

                for (int ci = 0; ci < 3; ci++)
                {
                    long jA = colCuts[ci] + 1;
                    long jB = colCuts[ci + 1];
                    if (jA > jB) continue;

                    bool tlLeTr = jB <= jTop;
                    bool blLeBr = jB <= jBot;
                    bool tlLeBl = iB <= iLeft;
                    bool trLeBr = iB <= iRight;

                    // Test whether a single corner dominates inside this sub-rectangle.
                    if (tlLeTr && tlLeBl && (trLeBr || blLeBr))
                    {
                        total = (total + SumSingleCorner(iB - iA + 1, jB - jA + 1, tl + (Int128)(iA + jA) * d, d)) % SlateMod;
                    }
                    else if (!tlLeTr && trLeBr && (tlLeBl || !blLeBr))
                    {
                        total = (total + SumSingleCorner(iB - iA + 1, jB - jA + 1, tr + (Int128)(iA + w1 - jB) * d, d)) % SlateMod;
                    }
                    else if (!tlLeBl && blLeBr && (tlLeTr || !trLeBr))
                    {
                        total = (total + SumSingleCorner(iB - iA + 1, jB - jA + 1, bl + (Int128)(h1 - iB + jA) * d, d)) % SlateMod;
                    }
                    else if (!blLeBr && !trLeBr && (!tlLeTr || !tlLeBl))
                    {
                        total = (total + SumSingleCorner(iB - iA + 1, jB - jA + 1, br + (Int128)(h1 - iB + w1 - jB) * d, d)) % SlateMod;
                    }
                    else if (tlLeTr && tlLeBl && !trLeBr && !blLeBr)
                    {
                        // Opposite pair TL vs BR.
                        total = (total + SumOppositePair(iA, iB, jA, jB, h1, w1, tl, br, d)) % SlateMod;
                    }
                    else
                    {
                        // Opposite pair TR vs BL: reflect columns j -> w1 - j to reuse TL vs BR formula.
                        total = (total + SumOppositePair(iA, iB, w1 - jB, w1 - jA, h1, w1, tr, bl, d)) % SlateMod;
                    }
                }
            }

            return total;
        }

        private static long SumSingleCorner(long rows, long cols, Int128 minCornerVal, long d)
        {
            // Sum_{u=0..rows-1, v=0..cols-1} (minCornerVal + (u + v) * d)
            long rcMod = (rows % SlateMod) * (cols % SlateMod) % SlateMod;
            long baseMod = (long)(minCornerVal % SlateMod);
            long dMod = d % SlateMod;

            long rowPart = (cols % SlateMod) * Sum1(rows - 1) % SlateMod;
            long colPart = (rows % SlateMod) * Sum1(cols - 1) % SlateMod;
            long stepSum = (rowPart + colPart) % SlateMod * dMod % SlateMod;
            return (rcMod * baseMod + stepSum) % SlateMod;
        }

        private static long SumOppositePair(
            long iA, long iB, long jA, long jB,
            long h1, long w1, long baseA, long baseB, long d)
        {
            long sh = iB - iA + 1;
            long sw = jB - jA + 1;
            if (sh > sw)
                return SumOppositePair(jA, jB, iA, iB, w1, h1, baseA, baseB, d);

            // For u in [0..sh-1], v in [0..sw-1], let t = u + v in [0 .. sh + sw - 2].
            // ValueA(t) = baseA + (iA + jA + t) * d
            // ValueB(t) = baseB + (h1 + w1 - iA - jA - t) * d
            long tMax = sh + sw - 2;
            long sSplit = FloorDiv((Int128)baseB - baseA + (Int128)(h1 + w1) * d, 2 * d);
            long tSplit = sSplit - (iA + jA);

            Int128 vA0 = baseA + (Int128)(iA + jA) * d;
            Int128 vB0 = baseB + (Int128)(h1 + w1 - iA - jA) * d;

            long SumInterval(long segLo, long segHi, long c0, long c1)
            {
                if (segLo > segHi) return 0;
                long aHi = Math.Min(segHi, tSplit);
                long bLo = Math.Max(segLo, tSplit + 1);
                long s = 0;
                if (segLo <= aHi)
                    s = (s + SumProdRangeMod(segLo, aHi, c0, c1, vA0, d, 0)) % SlateMod;
                if (bLo <= segHi)
                    s = (s + SumProdRangeMod(bLo, segHi, c0, c1, vB0, -d, 0)) % SlateMod;
                return s;
            }

            long total = 0;
            // Region 1: 0 <= t <= sh - 2 -> count(t) = t + 1
            total = (total + SumInterval(0, Math.Min(tMax, sh - 2), 1, 1)) % SlateMod;
            // Region 2: sh - 1 <= t <= sw - 1 -> count(t) = sh
            total = (total + SumInterval(Math.Max(0, sh - 1), Math.Min(tMax, sw - 1), sh, 0)) % SlateMod;
            // Region 3: sw <= t <= sh + sw - 2 -> count(t) = sh + sw - 1 - t
            total = (total + SumInterval(Math.Max(0, sw), tMax, sh + sw - 1, -1)) % SlateMod;
            return total;
        }

        private static string Q4Solver(long r, long c, int n, long d, long[] ri, long[] ci, long[] bi)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    long dist = Math.Abs(ri[i] - ri[j]) + Math.Abs(ci[i] - ci[j]);
                    if ((Int128)Math.Abs(bi[i] - bi[j]) > (Int128)dist * d)
                        return "IMPOSSIBLE";
                }
            }

            SortedSet<long> rSet = [1, r];
            SortedSet<long> cSet = [1, c];
            for (int i = 0; i < n; i++)
            {
                rSet.Add(ri[i]);
                cSet.Add(ci[i]);
            }

            long[] rows = rSet.ToArray();
            long[] cols = cSet.ToArray();
            int nr = rows.Length;
            int nc = cols.Length;

            const long inf = long.MaxValue / 4;
            long[,] val = new long[nr, nc];
            for (int i = 0; i < nr; i++)
                for (int j = 0; j < nc; j++)
                    val[i, j] = inf;

            for (int k = 0; k < n; k++)
            {
                int rIdx = Array.BinarySearch(rows, ri[k]);
                int cIdx = Array.BinarySearch(cols, ci[k]);
                val[rIdx, cIdx] = bi[k];
            }

            // 4-directional sweep computes exact L1 shortest path DP on compressed grid in O(nr * nc).
            for (int i = 0; i < nr; i++)
            {
                for (int j = 0; j < nc; j++)
                {
                    if (i > 0) val[i, j] = Math.Min(val[i, j], val[i - 1, j] + (rows[i] - rows[i - 1]) * d);
                    if (j > 0) val[i, j] = Math.Min(val[i, j], val[i, j - 1] + (cols[j] - cols[j - 1]) * d);
                }
                for (int j = nc - 2; j >= 0; j--)
                    val[i, j] = Math.Min(val[i, j], val[i, j + 1] + (cols[j + 1] - cols[j]) * d);
            }
            for (int i = nr - 1; i >= 0; i--)
            {
                for (int j = 0; j < nc; j++)
                {
                    if (i + 1 < nr) val[i, j] = Math.Min(val[i, j], val[i + 1, j] + (rows[i + 1] - rows[i]) * d);
                    if (j > 0) val[i, j] = Math.Min(val[i, j], val[i, j - 1] + (cols[j] - cols[j - 1]) * d);
                }
                for (int j = nc - 2; j >= 0; j--)
                    val[i, j] = Math.Min(val[i, j], val[i, j + 1] + (cols[j + 1] - cols[j]) * d);
            }

            long ans = 0;
            // 1. Vertices.
            for (int i = 0; i < nr; i++)
                for (int j = 0; j < nc; j++)
                    ans = (ans + val[i, j] % SlateMod) % SlateMod;

            // 2. Open horizontal segments.
            for (int i = 0; i < nr; i++)
            {
                for (int j = 0; j + 1 < nc; j++)
                {
                    long len = cols[j + 1] - cols[j] - 1;
                    ans = (ans + SumSegmentOpen(len, val[i, j], val[i, j + 1], d)) % SlateMod;
                }
            }

            // 3. Open vertical segments.
            for (int i = 0; i + 1 < nr; i++)
            {
                long len = rows[i + 1] - rows[i] - 1;
                for (int j = 0; j < nc; j++)
                    ans = (ans + SumSegmentOpen(len, val[i, j], val[i + 1, j], d)) % SlateMod;
            }

            // 4. Open interior rectangles.
            for (int i = 0; i + 1 < nr; i++)
            {
                long h = rows[i + 1] - rows[i] - 1;
                if (h <= 0) continue;
                for (int j = 0; j + 1 < nc; j++)
                {
                    long w = cols[j + 1] - cols[j] - 1;
                    if (w <= 0) continue;
                    ans = (ans + SumInteriorRectangle(h, w, val[i, j], val[i, j + 1], val[i + 1, j], val[i + 1, j + 1], d)) % SlateMod;
                }
            }

            return ans.ToString(CultureInfo.InvariantCulture);
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
    }
}
