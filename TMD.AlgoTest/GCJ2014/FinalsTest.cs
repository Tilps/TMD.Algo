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
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Checkerboard Matrix

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(2 * n, out string[] rows);
            return () => Q1Solver(n, rows);
        }

        private static string Q1Solver(int n, string[] rows)
        {
            int size = 2 * n;
            string row0 = rows[0];

            int zeroEven = 0;
            int zeroTotal = 0;
            for (int c = 0; c < size; c++)
            {
                if (row0[c] == '0')
                {
                    zeroTotal++;
                    if ((c & 1) == 0)
                        zeroEven++;
                }
            }
            if (zeroTotal != n)
                return "IMPOSSIBLE";

            int aCount = 0;
            int aEven = 0;
            for (int r = 0; r < size; r++)
            {
                string row = rows[r];
                bool matchesA = true;
                bool matchesComp = true;
                for (int c = 0; c < size; c++)
                {
                    if (row[c] == row0[c])
                        matchesComp = false;
                    else
                        matchesA = false;
                    if (!matchesA && !matchesComp)
                        return "IMPOSSIBLE";
                }

                if (matchesA)
                {
                    aCount++;
                    if ((r & 1) == 0)
                        aEven++;
                }
            }

            if (aCount != n)
                return "IMPOSSIBLE";

            int rowSwaps = Math.Min(aEven, n - aEven);
            int colSwaps = Math.Min(zeroEven, n - zeroEven);
            return (rowSwaps + colSwaps).ToString(CultureInfo.InvariantCulture);
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

        #region Question 2 - Power Swapper

        private static readonly long[] SmallFact = BuildSmallFact();

        private static long[] BuildSmallFact()
        {
            long[] f = new long[13];
            f[0] = 1L;
            for (int i = 1; i < f.Length; i++)
                f[i] = f[i - 1] * i;
            return f;
        }

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] perm);
            return () => Q2Solver(perm);
        }

        private static long Q2Solver(int[] perm)
        {
            int[] zeroBased = new int[perm.Length];
            for (int i = 0; i < perm.Length; i++)
                zeroBased[i] = perm[i] - 1;
            return SolvePowerSwap(zeroBased, 0);
        }

        private static bool IsValidPair(int x, int y) => (x & 1) == 0 && x + 1 == y;

        private static int[] CompressPairs(int[] a)
        {
            int half = a.Length >> 1;
            int[] next = new int[half];
            for (int i = 0; i < half; i++)
                next[i] = a[2 * i] >> 1;
            return next;
        }

        private static long SolvePowerSwap(int[] a, int swaps)
        {
            if (a.Length == 1)
                return SmallFact[swaps];

            int half = a.Length >> 1;
            int bad0 = -1, bad1 = -1;
            int badCount = 0;
            for (int i = 0; i < half; i++)
            {
                if (!IsValidPair(a[2 * i], a[2 * i + 1]))
                {
                    if (badCount == 0) bad0 = i;
                    else if (badCount == 1) bad1 = i;
                    else return 0L;
                    badCount++;
                }
            }

            if (badCount == 0)
                return SolvePowerSwap(CompressPairs(a), swaps);

            if (badCount == 1)
            {
                int i0 = 2 * bad0;
                int i1 = i0 + 1;
                if (!IsValidPair(a[i1], a[i0]))
                    return 0L;
                (a[i0], a[i1]) = (a[i1], a[i0]);
                long ans = SolvePowerSwap(CompressPairs(a), swaps + 1);
                (a[i0], a[i1]) = (a[i1], a[i0]);
                return ans;
            }

            long total = 0L;
            int p0 = 2 * bad0, p1 = p0 + 1;
            int q0 = 2 * bad1, q1 = q0 + 1;
            foreach (int u in new[] { p0, p1 })
            {
                foreach (int v in new[] { q0, q1 })
                {
                    (a[u], a[v]) = (a[v], a[u]);
                    if (IsValidPair(a[p0], a[p1]) && IsValidPair(a[q0], a[q1]))
                        total += SolvePowerSwap(CompressPairs(a), swaps + 1);
                    (a[u], a[v]) = (a[v], a[u]);
                }
            }
            return total;
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

        #region Question 3 - Symmetric Trees

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            int[] colors = new int[n];
            for (int i = 0; i < n; i++)
            {
                test.Get(out string c);
                colors[i] = c[0] - 'A';
            }
            (int U, int V)[] edges = new (int, int)[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                test.Get(out int u, out int v);
                edges[i] = (u - 1, v - 1);
            }
            return () => Q3Solver(n, colors, edges);
        }

        private static string Q3Solver(int n, int[] colors, (int U, int V)[] edges)
        {
            List<List<int>> adj = [with(capacity: n + 1)];
            for (int i = 0; i < n; i++)
                adj.Add([]);
            foreach (var (u, v) in edges)
            {
                adj[u].Add(v);
                adj[v].Add(u);
            }

            var (c1, c2) = RootedForest.FindTreeCenters(adj);
            int root;
            int[] labels;
            if (c2 == -1)
            {
                root = c1;
                labels = colors;
            }
            else
            {
                root = n;
                labels = new int[n + 1];
                Array.Copy(colors, labels, n);
                labels[n] = 26;
                adj[c1].Remove(c2);
                adj[c2].Remove(c1);
                adj.Add([c1, c2]);
                adj[c1].Add(root);
                adj[c2].Add(root);
            }

            int totalVerts = adj.Count;
            int[] canonical = RootedForest.ComputeCanonicalSubtreeIds(adj, root, labels);

            int[] parent = new int[totalVerts];
            Array.Fill(parent, -1);
            bool[] visited = new bool[totalVerts];
            int[] queue = new int[totalVerts];
            visited[root] = true;
            int head = 0, tail = 0;
            queue[tail++] = root;
            while (head < tail)
            {
                int u = queue[head++];
                foreach (int v in adj[u])
                {
                    if (!visited[v])
                    {
                        visited[v] = true;
                        parent[v] = u;
                        queue[tail++] = v;
                    }
                }
            }

            List<int> GetOddChildren(int u)
            {
                List<int> children = [];
                foreach (int v in adj[u])
                {
                    if (v != parent[u])
                        children.Add(v);
                }
                children.Sort((a, b) => canonical[a].CompareTo(canonical[b]));

                List<int> odd = [];
                int idx = 0;
                while (idx < children.Count)
                {
                    int end = idx + 1;
                    int cid = canonical[children[idx]];
                    while (end < children.Count && canonical[children[end]] == cid)
                        end++;
                    if (((end - idx) & 1) == 1)
                        odd.Add(children[idx]);
                    idx = end;
                }
                return odd;
            }

            bool IsChainSymmetric(int start)
            {
                int cur = start;
                while (true)
                {
                    List<int> odd = GetOddChildren(cur);
                    if (odd.Count == 0)
                        return true;
                    if (odd.Count > 1)
                        return false;
                    cur = odd[0];
                }
            }

            List<int> rootOdd = GetOddChildren(root);
            if (rootOdd.Count > 2)
                return "NOT SYMMETRIC";

            foreach (int branch in rootOdd)
            {
                if (!IsChainSymmetric(branch))
                    return "NOT SYMMETRIC";
            }
            return "SYMMETRIC";
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

        #region Question 4 - Paradox Sort

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int a);
            test.GetLines(n, out string[] grid);
            return () => Q4Solver(n, a, grid);
        }

        private static string Q4Solver(int n, int a, string[] grid)
        {
            UInt128[] preferMask = new UInt128[n];
            UInt128[] beatenByMask = new UInt128[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (grid[i][j] == 'Y')
                    {
                        preferMask[i] |= UInt128.One << j;
                        beatenByMask[j] |= UInt128.One << i;
                    }
                }
            }

            UInt128 allMask = n == 128 ? ~UInt128.Zero : (UInt128.One << n) - UInt128.One;

            UInt128 Reachable(int start, UInt128 allowed)
            {
                UInt128 reached = UInt128.One << start;
                UInt128 frontier = reached;
                while (frontier != UInt128.Zero)
                {
                    ulong lo = (ulong)frontier;
                    int v;
                    if (lo != 0UL)
                    {
                        v = BitOperations.TrailingZeroCount(lo);
                    }
                    else
                    {
                        ulong hi = (ulong)(frontier >> 64);
                        v = 64 + BitOperations.TrailingZeroCount(hi);
                    }
                    frontier ^= UInt128.One << v;

                    UInt128 next = preferMask[v] & allowed & ~reached;
                    reached |= next;
                    frontier |= next;
                }
                return reached;
            }

            if (Reachable(a, allMask) != allMask)
                return "IMPOSSIBLE";

            bool IsValid(int curWinner, UInt128 remMask)
            {
                if (curWinner == a)
                    return (preferMask[a] & remMask) == remMask;

                if ((remMask & (UInt128.One << a)) == UInt128.Zero)
                    return false;

                UInt128 reached = Reachable(a, remMask);
                if ((beatenByMask[curWinner] & reached) == UInt128.Zero)
                    return false;
                return ((reached | preferMask[curWinner]) & remMask) == remMask;
            }

            int[] result = new int[n];
            UInt128 remaining = allMask;
            int curWinner = -1;

            for (int pos = 0; pos < n; pos++)
            {
                int chosen = -1;
                for (int cand = 0; cand < n; cand++)
                {
                    UInt128 bit = UInt128.One << cand;
                    if ((remaining & bit) == UInt128.Zero)
                        continue;

                    int nextWinner = (pos == 0 || grid[cand][curWinner] == 'Y') ? cand : curWinner;
                    UInt128 nextRem = remaining ^ bit;
                    if (IsValid(nextWinner, nextRem))
                    {
                        chosen = cand;
                        curWinner = nextWinner;
                        remaining = nextRem;
                        break;
                    }
                }
                result[pos] = chosen;
            }

            return string.Join(" ", result);
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

        #region Question 5 - Allergy Testing

        private static Func<long> Q5Solver(GCJTestCase test)
        {
            test.Get(out long n, out long a, out long b);
            return () => Q5Solver(n, a, b);
        }

        private static bool CanIdentifyAllergen(long days, long n, long a, long b)
        {
            Int128 total = 0;
            Int128 n128 = n;
            int maxL = (int)(days / b);
            for (int l = 0; l <= maxL; l++)
            {
                long lb = l * b;
                long kMax = (days - lb) / a;
                long lbNext = lb + b;
                long kMin = days < lbNext ? 0L : (days - lbNext) / a + 1L;

                long cMaxPrev = Combinatorics.ChooseCapped(kMax + l, l, n);
                if (cMaxPrev >= n)
                    return true;
                Int128 upper = (Int128)cMaxPrev * (kMax + l + 1L) / (l + 1L);

                Int128 lower = 0;
                if (kMin > 0L)
                {
                    long cMinPrev = Combinatorics.ChooseCapped(kMin + l - 1L, l, n);
                    lower = (Int128)cMinPrev * (kMin + l) / (l + 1L);
                }

                total += upper - lower;
                if (total >= n128)
                    return true;
            }
            return false;
        }

        private static long Q5Solver(long n, long a, long b)
        {
            if (n <= 1L)
                return 0L;

            long lo = 0L;
            long hi = 50L * b;
            long ans = hi;
            while (lo <= hi)
            {
                long mid = lo + ((hi - lo) >> 1);
                if (CanIdentifyAllergen(mid, n, a, b))
                {
                    ans = mid;
                    hi = mid - 1L;
                }
                else
                {
                    lo = mid + 1L;
                }
            }
            return ans;
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

        #region Question 6 - ARAM

        private static Func<string> Q6Solver(GCJTestCase test)
        {
            test.Get(out int n, out int r, out int g);
            test.Get(out string[] probTokens);
            return () => Q6Solver(n, r, g, probTokens);
        }

        private static string Q6Solver(int n, int r, int g, string[] probTokens)
        {
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
                weights[i] = (int)decimal.Round(decimal.Parse(probTokens[i], CultureInfo.InvariantCulture) * 10000m);

            Array.Sort(weights, static (x, y) => y.CompareTo(x));
            long[] prefW = new long[n + 1];
            for (int i = 0; i < n; i++)
                prefW[i + 1] = prefW[i] + weights[i];

            decimal avgAll = prefW[n] / (10000m * n);
            decimal lo = avgAll;
            decimal hi = weights[0] / 10000m;
            if (lo == hi)
                return lo.ToString("F15", CultureInfo.InvariantCulture);

            MonotoneLineHull<decimal> hull = new(maximize: true);
            for (int k = n; k >= 1; k--)
            {
                decimal slope = (decimal)(n - k) / k;
                decimal intercept = prefW[k] / (10000m * k);
                hull.AddLine(slope, intercept);
            }

            int rg = r * g;
            decimal[] aArr = new decimal[rg + 1];

            bool CanAchieve(decimal q)
            {
                for (int c = 0; c < g; c++)
                    aArr[c] = avgAll - q;

                int ptr = 0;
                for (int c = g; c <= rg; c++)
                {
                    decimal sum = 0m;
                    for (int j = c - g; j < c; j++)
                        sum += aArr[j];

                    decimal val = hull.QueryMonotone(sum, ref ptr) - q;
                    if (val >= 0m)
                        return true;
                    aArr[c] = val;
                }
                return false;
            }

            for (int iter = 0; iter < 88; iter++)
            {
                decimal mid = (lo + hi) * 0.5m;
                if (CanAchieve(mid))
                    lo = mid;
                else
                    hi = mid;
            }

            return ((lo + hi) * 0.5m).ToString("F15", CultureInfo.InvariantCulture);
        }

        [Test]
        public void Q6Sample()
        {
            Test(Q6Solver, 1e-10);
        }

        [Test]
        public void Q6Small()
        {
            Test(Q6Solver, 1e-10);
        }

        [Test]
        public void Q6Large()
        {
            SlowTest(Q6Solver, 1e-10);
        }

        #endregion
    }
}
