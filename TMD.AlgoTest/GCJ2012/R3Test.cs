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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Perfect Game

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] lengths);
            test.Get(out int[] probs);
            return () => Q1Solver(n, lengths, probs);
        }

        private static string Q1Solver(int n, int[] lengths, int[] probs)
        {
            int[] order = new int[n];
            for (int i = 0; i < n; i++)
                order[i] = i;

            Array.Sort(order, (a, b) =>
            {
                int cmp = (lengths[a] * probs[b]).CompareTo(lengths[b] * probs[a]);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });

            return string.Join(" ", order);
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

        #region Question 2 - Havannah

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int s, out int m);
            test.GetLines(m, out (int X, int Y)[] moves);
            return () => Q2Solver(s, moves);
        }

        private static int GetBoundaryMask(int s, int x, int y)
        {
            int maxCoord = 2 * s - 1;
            if (x == 1 && y == 1) return 1 << 0;
            if (x == 1 && y == s) return 1 << 1;
            if (x == s && y == maxCoord) return 1 << 2;
            if (x == maxCoord && y == maxCoord) return 1 << 3;
            if (x == maxCoord && y == s) return 1 << 4;
            if (x == s && y == 1) return 1 << 5;

            if (x == 1) return 1 << 6;
            if (y - x == s - 1) return 1 << 7;
            if (y == maxCoord) return 1 << 8;
            if (x == maxCoord) return 1 << 9;
            if (x - y == s - 1) return 1 << 10;
            if (y == 1) return 1 << 11;
            return 0;
        }

        private static string Q2Solver(int s, (int X, int Y)[] moves)
        {
            int m = moves.Length;
            int maxCoord = 2 * s - 1;
            Dictionary<long, int> occupied = [with(capacity: m)];
            MappedDisjointTracker<int> dsu = new(static i => i, 0, m - 1);
            int[] masks = new int[m];

            ReadOnlySpan<int> dx = [1, 0, -1, -1, 0, 1];
            ReadOnlySpan<int> dy = [1, 1, 0, -1, -1, 0];
            int[] comp = new int[6];

            for (int idx = 0; idx < m; idx++)
            {
                var (x, y) = moves[idx];
                for (int d = 0; d < 6; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx >= 1 && nx <= maxCoord && ny >= 1 && ny <= maxCoord &&
                        Math.Abs(nx - ny) <= s - 1 &&
                        occupied.TryGetValue(((long)nx << 20) | (uint)ny, out int nid))
                    {
                        comp[d] = dsu.GetRepresentative(nid);
                    }
                    else
                    {
                        comp[d] = -1;
                    }
                }

                bool isRing = false;
                for (int d = 0; d < 6 && !isRing; d++)
                {
                    int c = comp[d];
                    if (c < 0 || c == comp[(d + 5) % 6])
                        continue;
                    for (int prev = 0; prev < d; prev++)
                    {
                        int pc = comp[prev];
                        if (pc == c && pc != comp[(prev + 5) % 6])
                        {
                            isRing = true;
                            break;
                        }
                    }
                }

                dsu.Add(idx);
                masks[idx] = GetBoundaryMask(s, x, y);
                occupied[((long)x << 20) | (uint)y] = idx;
                for (int d = 0; d < 6; d++)
                {
                    if (comp[d] >= 0 && dsu.Union(idx, comp[d], out int root, out int absorbed))
                        masks[root] |= masks[absorbed];
                }

                int mask = masks[dsu.GetRepresentative(idx)];
                bool isBridge = BitOperations.PopCount((uint)(mask & 0x3F)) >= 2;
                bool isFork = BitOperations.PopCount((uint)(mask >> 6)) >= 3;

                if (isBridge || isFork || isRing)
                {
                    List<string> types = [];
                    if (isBridge) types.Add("bridge");
                    if (isFork) types.Add("fork");
                    if (isRing) types.Add("ring");
                    return $"{string.Join("-", types)} in move {idx + 1}";
                }
            }

            return "none";
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

        #region Question 3 - Quality Food

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out long m, out long f, out int n);
            test.GetLines(n, out (long Price, long Stale)[] foods);
            return () => Q3Solver(m, f, foods);
        }

        private static long Q3Solver(long m, long f, (long Price, long Stale)[] foods)
        {
            Array.Sort(foods, static (a, b) =>
            {
                int cmp = a.Price.CompareTo(b.Price);
                return cmp != 0 ? cmp : b.Stale.CompareTo(a.Stale);
            });

            List<(long P, Int128 DaysLimit)> hull = [];
            long bestStale = -1;
            foreach (var (p, s) in foods)
            {
                if (s > bestStale)
                {
                    bestStale = s;
                    hull.Add((p, (Int128)s + 1));
                }
            }

            if (hull.Count == 0 || m < f + hull[0].P)
                return 0L;

            List<Int128> pieceCost = [];
            List<Int128> pieceDays = [];
            Int128 curCost = 0;
            Int128 prevDays = 0;
            foreach (var (p, daysLimit) in hull)
            {
                Int128 deltaDays = daysLimit - prevDays;
                curCost += deltaDays * p;
                pieceCost.Add(curCost);
                pieceDays.Add(daysLimit);
                prevDays = daysLimit;
                if (f + curCost > m)
                    break;
            }

            Int128 EvalDays(Int128 deliveries)
            {
                if (deliveries <= 0 || deliveries * f > m)
                    return 0;
                Int128 budget = m - deliveries * f;
                Int128 days = 0;
                Int128 prevD = 0;
                for (int i = 0; i < hull.Count; i++)
                {
                    Int128 tierCap = (hull[i].DaysLimit - prevD) * deliveries;
                    Int128 maxAffordable = budget / hull[i].P;
                    if (maxAffordable < tierCap)
                        return days + maxAffordable;
                    days += tierCap;
                    budget -= tierCap * hull[i].P;
                    prevD = hull[i].DaysLimit;
                }
                return days;
            }

            Int128 maxDeliveries = m / f;
            List<Int128> candidates = [1, maxDeliveries];
            foreach (Int128 c in pieceCost)
            {
                Int128 single = f + c;
                if (single <= m)
                {
                    Int128 x = (Int128)m / single;
                    candidates.Add(x);
                    if (x + 1 <= maxDeliveries)
                        candidates.Add(x + 1);
                }
            }

            Int128 bestDays = 0;
            foreach (Int128 x in candidates)
            {
                Int128 d = EvalDays(x);
                if (d > bestDays)
                    bestDays = d;
            }
            return (long)bestDays;
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

        #region Question 4 - Lost Password

        private static Func<long> Q4Solver(GCJTestCase test)
        {
            test.Get(out int k);
            test.Get(out string s);
            return () => Q4Solver(k, s);
        }

        private static int LeetMultiplier(char ch) => ch switch
        {
            'o' or 'i' or 'e' or 'a' or 's' or 't' or 'b' or 'g' => 2,
            _ => 1
        };

        private static long Q4Solver(int k, string s)
        {
            int n = s.Length;
            SuffixArray sa = SuffixArray.Build(s);
            IReadOnlyList<int> order = sa.Order;
            IReadOnlyList<int> lcp = sa.Lcp;

            long[] pCnt = new long[n + 1];
            long[] sCnt = new long[n + 1];
            long[] nextP = new long[n + 1];
            long[] nextS = new long[n + 1];

            long totalC = 0;
            for (int idx = 0; idx < n;)
            {
                int end = idx + 1;
                while (end < n && lcp[end - 1] >= k)
                    end++;

                int pos = order[idx];
                if (pos + k <= n)
                {
                    long v = 1;
                    for (int t = 0; t < k; t++)
                        v *= LeetMultiplier(s[pos + t]);
                    totalC += v;
                    pCnt[pos] += v;
                    sCnt[pos + 1] += v;
                }
                idx = end;
            }

            long totalCost = 0;
            long lastMatchedDist = 0;
            long remEmpty = 0;

            for (int dist = 0; dist <= k - 2; dist++)
            {
                int overlap = k - 1 - dist;
                Array.Clear(nextP);
                Array.Clear(nextS);

                for (int idx = 0; idx < n;)
                {
                    int end = idx + 1;
                    while (end < n && lcp[end - 1] >= overlap)
                        end++;

                    long sumP = 0, sumS = 0;
                    int rep = -1;
                    for (int j = idx; j < end; j++)
                    {
                        int pos = order[j];
                        long pv = pCnt[pos];
                        long sv = sCnt[pos];
                        if (pv != 0 || sv != 0)
                        {
                            sumP += pv;
                            sumS += sv;
                            if (rep < 0)
                                rep = pos;
                        }
                    }

                    if (rep >= 0)
                    {
                        long matched = Math.Min(sumP, sumS);
                        if (matched > 0)
                        {
                            totalCost += (long)dist * matched;
                            lastMatchedDist = dist;
                        }

                        if (overlap > 1)
                        {
                            nextP[rep] += sumP - matched;
                            nextS[rep + 1] += sumS - matched;
                        }
                        else
                        {
                            remEmpty += sumP - matched;
                        }
                    }

                    idx = end;
                }

                (pCnt, nextP) = (nextP, pCnt);
                (sCnt, nextS) = (nextS, sCnt);
            }

            if (remEmpty > 0)
            {
                totalCost += (long)(k - 1) * remEmpty;
                lastMatchedDist = k - 1;
            }

            return totalC + k - 1 + totalCost - lastMatchedDist;
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
            Test(Q4Solver);
        }

        #endregion
    }
}
