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

namespace TMD.AlgoTest.GCJ2011
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Airport Walkways

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int x, out int s, out int r, out int t, out int n);
            test.GetLines(n, out (int B, int E, int W)[] walkways);
            return () => Q1Solver(x, s, r, t, walkways);
        }

        private static string Q1Solver(int x, int s, int r, int t, (int B, int E, int W)[] walkways)
        {
            long[] speedDist = new long[101];
            long walkwayLen = 0;
            foreach (var (b, e, w) in walkways)
            {
                int len = e - b;
                speedDist[w] += len;
                walkwayLen += len;
            }
            speedDist[0] = x - walkwayLen;

            double remT = t;
            double totalTime = 0;
            for (int v = 0; v <= 100; v++)
            {
                long dist = speedDist[v];
                if (dist == 0)
                    continue;

                double runTime = Math.Min(remT, dist / (double)(v + r));
                double remDist = dist - runTime * (v + r);
                double walkTime = remDist / (v + s);
                totalTime += runTime + walkTime;
                remT -= runTime;
            }

            return totalTime.ToString("F12", CultureInfo.InvariantCulture);
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

        #region Question 2 - Spinning Blade

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c, out int _);
            test.GetLines(r, out string[] rows);
            return () => Q2Solver(r, c, rows);
        }

        private static string Q2Solver(int r, int c, string[] rows)
        {
            PrefixSum2D<long> psM = new(r, c, (i, j) => rows[i][j] - '0');
            PrefixSum2D<long> psMR = new(r, c, (i, j) => (long)(rows[i][j] - '0') * i);
            PrefixSum2D<long> psMC = new(r, c, (i, j) => (long)(rows[i][j] - '0') * j);

            for (int k = Math.Min(r, c); k >= 3; k--)
            {
                for (int r1 = 0; r1 + k <= r; r1++)
                {
                    int r2 = r1 + k - 1;
                    for (int c1 = 0; c1 + k <= c; c1++)
                    {
                        int c2 = c1 + k - 1;

                        long w00 = rows[r1][c1] - '0';
                        long w01 = rows[r1][c2] - '0';
                        long w10 = rows[r2][c1] - '0';
                        long w11 = rows[r2][c2] - '0';

                        long m = psM.Query(r1, c1, r2, c2) - w00 - w01 - w10 - w11;
                        long mr = psMR.Query(r1, c1, r2, c2) - (w00 + w01) * r1 - (w10 + w11) * r2;
                        long mc = psMC.Query(r1, c1, r2, c2) - (w00 + w10) * c1 - (w01 + w11) * c2;

                        if (2L * mr == (long)(r1 + r2) * m && 2L * mc == (long)(c1 + c2) * m)
                            return k.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            return "IMPOSSIBLE";
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
            SlowTest(Q2Solver);
        }

        #endregion

        #region Question 3 - Expensive Dinner

        private static readonly int[] Primes = MathFormulas.PrimesLessThan(1_000_001);

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out long n);
            return () => Q3Solver(n);
        }

        private static long Q3Solver(long n)
        {
            if (n == 1)
                return 0L;

            long ans = 1;
            foreach (int p in Primes)
            {
                long pp = (long)p * p;
                if (pp > n)
                    break;
                long val = pp;
                while (true)
                {
                    ans++;
                    if (val > n / p)
                        break;
                    val *= p;
                }
            }
            return ans;
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

        #region Question 4 - A.I. War

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int p, out int _);
            test.Get(out string[] edges);
            return () => Q4Solver(p, edges);
        }

        private static string Q4Solver(int p, string[] edges)
        {
            List<int>[] adj = new List<int>[p];
            for (int i = 0; i < p; i++)
                adj[i] = [];

            int words = (p + 63) / 64;
            ulong[,] mask = new ulong[p, words];

            foreach (string token in edges)
            {
                int comma = token.IndexOf(',');
                int u = int.Parse(token.AsSpan(0, comma), CultureInfo.InvariantCulture);
                int v = int.Parse(token.AsSpan(comma + 1), CultureInfo.InvariantCulture);
                adj[u].Add(v);
                adj[v].Add(u);
                mask[u, v >> 6] |= 1UL << (v & 63);
                mask[v, u >> 6] |= 1UL << (u & 63);
            }

            int[] dist = new int[p];
            Array.Fill(dist, -1);
            Search.Bfs(0, (cur, _, d) => dist[cur] = d, u => adj[u]);

            int targetDist = dist[1];
            if (targetDist == 1)
                return $"0 {adj[0].Count}";

            List<int>[] levels = new List<int>[targetDist];
            for (int i = 0; i < targetDist; i++)
                levels[i] = [];
            for (int v = 0; v < p; v++)
            {
                if (dist[v] >= 0 && dist[v] < targetDist)
                    levels[dist[v]].Add(v);
            }

            int[,] dp = new int[p, p];
            for (int i = 0; i < p; i++)
                for (int j = 0; j < p; j++)
                    dp[i, j] = -1;

            foreach (int b in levels[1])
            {
                int unionSize = 0;
                for (int w = 0; w < words; w++)
                    unionSize += BitOperations.PopCount(mask[0, w] | mask[b, w]);
                dp[0, b] = unionSize;
            }

            for (int d = 1; d <= targetDist - 2; d++)
            {
                foreach (int b in levels[d])
                {
                    foreach (int c in adj[b])
                    {
                        if (dist[c] != d + 1)
                            continue;
                        int best = -1;
                        foreach (int a in adj[b])
                        {
                            if (dist[a] != d - 1 || dp[a, b] < 0)
                                continue;
                            int gain = 0;
                            for (int w = 0; w < words; w++)
                                gain += BitOperations.PopCount(mask[c, w] & ~(mask[a, w] | mask[b, w]));
                            int cand = dp[a, b] + gain;
                            if (cand > best)
                                best = cand;
                        }
                        if (best > dp[b, c])
                            dp[b, c] = best;
                    }
                }
            }

            int maxUnion = 0;
            foreach (int b in adj[1])
            {
                if (dist[b] != targetDist - 1)
                    continue;
                foreach (int a in adj[b])
                {
                    if (dist[a] == targetDist - 2 && dp[a, b] > maxUnion)
                        maxUnion = dp[a, b];
                }
            }

            int conquered = targetDist - 1;
            int threatened = maxUnion - targetDist;
            return $"{conquered} {threatened}";
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
