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
using System.Text;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - The Repeater

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out string[] words);
            return () => Q1Solver(n, words);
        }

        private static (string Signature, int[] Counts) RunLengthEncode(string s)
        {
            StringBuilder sb = new();
            List<int> counts = [];
            int i = 0;
            while (i < s.Length)
            {
                char ch = s[i];
                int start = i;
                while (i < s.Length && s[i] == ch)
                    i++;
                sb.Append(ch);
                counts.Add(i - start);
            }
            return (sb.ToString(), [.. counts]);
        }

        private static string Q1Solver(int n, string[] words)
        {
            var (sig0, c0) = RunLengthEncode(words[0]);
            int runs = c0.Length;
            int[][] table = new int[runs][];
            for (int r = 0; r < runs; r++)
            {
                table[r] = new int[n];
                table[r][0] = c0[r];
            }

            for (int i = 1; i < n; i++)
            {
                var (sig, ci) = RunLengthEncode(words[i]);
                if (!string.Equals(sig, sig0, StringComparison.Ordinal))
                    return "Fegla Won";
                for (int r = 0; r < runs; r++)
                    table[r][i] = ci[r];
            }

            int moves = 0;
            for (int r = 0; r < runs; r++)
            {
                int[] col = table[r];
                Array.Sort(col);
                int med = col[n >> 1];
                for (int i = 0; i < n; i++)
                    moves += Math.Abs(col[i] - med);
            }
            return moves.ToString();
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

        #region Question 2 - New Lottery Game

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int a, out int b, out int k);
            return () => Q2Solver(a, b, k);
        }

        private static long Q2Solver(int a, int b, int k)
        {
            long[,] dp = new long[31, 8];
            dp[0, 7] = 1L; // (lessA=true, lessB=true, lessK=true) at base.

            for (int step = 1; step <= 30; step++)
            {
                int bit = step - 1;
                int bitA = (a >> bit) & 1;
                int bitB = (b >> bit) & 1;
                int bitK = (k >> bit) & 1;

                for (int mask = 0; mask < 8; mask++)
                {
                    bool lessA = (mask & 4) != 0;
                    bool lessB = (mask & 2) != 0;
                    bool lessK = (mask & 1) != 0;

                    int maxA = lessA ? 1 : bitA;
                    int maxB = lessB ? 1 : bitB;
                    int maxK = lessK ? 1 : bitK;

                    long ways = 0;
                    for (int ba = 0; ba <= maxA; ba++)
                    {
                        for (int bb = 0; bb <= maxB; bb++)
                        {
                            int bk = ba & bb;
                            if (bk > maxK)
                                continue;

                            int nextMask = ((lessA || ba < bitA) ? 4 : 0)
                                | ((lessB || bb < bitB) ? 2 : 0)
                                | ((lessK || bk < bitK) ? 1 : 0);
                            ways += dp[step - 1, nextMask];
                        }
                    }
                    dp[step, mask] = ways;
                }
            }

            return dp[30, 0];
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

        #region Question 3 - The Bored Traveling Salesman

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.GetLines(n, out string[] zips);
            (int U, int V)[] edges = new (int, int)[m];
            for (int i = 0; i < m; i++)
            {
                test.Get(out int u, out int v);
                edges[i] = (u - 1, v - 1);
            }
            return () => Q3Solver(n, zips, edges);
        }

        private static string Q3Solver(int n, string[] zips, (int U, int V)[] edges)
        {
            ulong[] adj = new ulong[n];
            foreach (var (u, v) in edges)
            {
                adj[u] |= 1UL << v;
                adj[v] |= 1UL << u;
            }

            int root = 0;
            for (int i = 1; i < n; i++)
            {
                if (string.CompareOrdinal(zips[i], zips[root]) < 0)
                    root = i;
            }

            ulong allMask = n == 64 ? ulong.MaxValue : (1UL << n) - 1UL;
            ulong visited = 1UL << root;
            ulong dead = 0UL;
            List<int> active = [root];
            StringBuilder tour = new(5 * n);
            tour.Append(zips[root]);

            bool CanReachAllUnvisited(int start, ulong tempDead, ulong unvisitedMask)
            {
                if (unvisitedMask == 0UL)
                    return true;
                ulong allowed = (~tempDead) & allMask;
                ulong reached = 1UL << start;
                ulong frontier = reached;
                while (frontier != 0UL)
                {
                    int u = BitOperations.TrailingZeroCount(frontier);
                    frontier &= frontier - 1;
                    ulong next = adj[u] & allowed & ~reached;
                    reached |= next;
                    frontier |= next;
                }
                return (reached & unvisitedMask) == unvisitedMask;
            }

            for (int step = 1; step < n; step++)
            {
                ulong unvisitedMask = (~visited) & allMask;
                ulong tempDead = dead;
                int best = -1;
                int bestK = -1;

                for (int k = active.Count - 1; k >= 0; k--)
                {
                    if (!CanReachAllUnvisited(active[0], tempDead, unvisitedMask))
                        break;

                    ulong candidates = adj[active[k]] & unvisitedMask;
                    while (candidates != 0UL)
                    {
                        int v = BitOperations.TrailingZeroCount(candidates);
                        candidates &= candidates - 1;
                        if (best == -1 || string.CompareOrdinal(zips[v], zips[best]) < 0)
                        {
                            best = v;
                            bestK = k;
                        }
                    }

                    tempDead |= 1UL << active[k];
                }

                while (active.Count - 1 > bestK)
                {
                    int popped = active[^1];
                    active.RemoveAt(active.Count - 1);
                    dead |= 1UL << popped;
                }

                active.Add(best);
                visited |= 1UL << best;
                tour.Append(zips[best]);
            }

            return tour.ToString();
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
    }
}
