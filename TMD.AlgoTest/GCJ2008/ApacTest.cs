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
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class ApacTest : BaseGCJTest
    {
        #region Question 1 - What Are Birds?

        private Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            string[] knownLines;
            test.GetLines(n, out knownLines);
            test.Get(out int m);
            string[] queryLines;
            test.GetLines(m, out queryLines);
            return () => Q1Solver(knownLines, queryLines);
        }

        private string Q1Solver(string[] knownLines, string[] queryLines)
        {
            var birds = new List<(int H, int W)>();
            var nonBirds = new List<(int H, int W)>();

            foreach (var line in knownLines)
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int h = int.Parse(parts[0]);
                int w = int.Parse(parts[1]);
                bool isBird = parts[2] == "BIRD";
                if (isBird)
                    birds.Add((h, w));
                else
                    nonBirds.Add((h, w));
            }

            int hMin = int.MaxValue, hMax = int.MinValue;
            int wMin = int.MaxValue, wMax = int.MinValue;

            foreach (var b in birds)
            {
                if (b.H < hMin) hMin = b.H;
                if (b.H > hMax) hMax = b.H;
                if (b.W < wMin) wMin = b.W;
                if (b.W > wMax) wMax = b.W;
            }

            var results = new List<string>(queryLines.Length);

            foreach (var q in queryLines)
            {
                var parts = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int qh = int.Parse(parts[0]);
                int qw = int.Parse(parts[1]);

                if (birds.Count == 0)
                {
                    // No known birds. If point coincides with a known non-bird, it's NOT BIRD, else UNKNOWN.
                    bool isKnownNonBird = nonBirds.Any(nb => nb.H == qh && nb.W == qw);
                    results.Add(isKnownNonBird ? "NOT BIRD" : "UNKNOWN");
                }
                else
                {
                    if (qh >= hMin && qh <= hMax && qw >= wMin && qw <= wMax)
                    {
                        results.Add("BIRD");
                    }
                    else
                    {
                        int expHMin = Math.Min(hMin, qh);
                        int expHMax = Math.Max(hMax, qh);
                        int expWMin = Math.Min(wMin, qw);
                        int expWMax = Math.Max(wMax, qw);

                        bool containsNonBird = nonBirds.Any(nb =>
                            nb.H >= expHMin && nb.H <= expHMax &&
                            nb.W >= expWMin && nb.W <= expWMax);

                        results.Add(containsNonBird ? "NOT BIRD" : "UNKNOWN");
                    }
                }
            }

            return Environment.NewLine + string.Join(Environment.NewLine, results);
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

        #region Question 2 - Apocalypse Soon

        private Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int cCount, out int rCount, out int myC, out int myR);
            int[,] grid = new int[rCount, cCount];
            for (int r = 0; r < rCount; r++)
            {
                test.Get(out int[] row);
                for (int c = 0; c < cCount; c++)
                    grid[r, c] = row[c];
            }

            return () => Q2Solver(cCount, rCount, myC - 1, myR - 1, grid);
        }

        private static readonly int[] DrQ2 = { -1, 0, 0, 1 };
        private static readonly int[] DcQ2 = { 0, -1, 1, 0 };

        private string Q2Solver(int cCount, int rCount, int myC, int myR, int[,] grid)
        {
            int maxDays = 0;

            bool HasLivingNeighbor(int[,] g, int r, int c)
            {
                for (int i = 0; i < 4; i++)
                {
                    int nr = r + DrQ2[i], nc = c + DcQ2[i];
                    if (nr >= 0 && nr < rCount && nc >= 0 && nc < cCount && g[nr, nc] > 0)
                        return true;
                }
                return false;
            }

            bool Dfs(int[,] curGrid, int day)
            {
                if (!HasLivingNeighbor(curGrid, myR, myC))
                    return true;

                // Determine targets for all computer-controlled nations
                (int tr, int tc)[,] targets = new (int, int)[rCount, cCount];
                for (int r = 0; r < rCount; r++)
                {
                    for (int c = 0; c < cCount; c++)
                    {
                        if (curGrid[r, c] <= 0 || (r == myR && c == myC))
                        {
                            targets[r, c] = (-1, -1);
                            continue;
                        }

                        int maxS = 0;
                        int btr = -1, btc = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            int nr = r + DrQ2[i], nc = c + DcQ2[i];
                            if (nr >= 0 && nr < rCount && nc >= 0 && nc < cCount && curGrid[nr, nc] > 0)
                            {
                                if (curGrid[nr, nc] > maxS)
                                {
                                    maxS = curGrid[nr, nc];
                                    btr = nr;
                                    btc = nc;
                                }
                            }
                        }
                        targets[r, c] = (btr, btc);
                    }
                }

                // Gather our choices
                var choices = new List<(int tr, int tc)>();
                for (int i = 0; i < 4; i++)
                {
                    int nr = myR + DrQ2[i], nc = myC + DcQ2[i];
                    if (nr >= 0 && nr < rCount && nc >= 0 && nc < cCount && curGrid[nr, nc] > 0)
                        choices.Add((nr, nc));
                }
                choices.Add((-1, -1)); // pass

                foreach (var choice in choices)
                {
                    int[,] nextGrid = (int[,])curGrid.Clone();

                    // Apply attacks
                    for (int r = 0; r < rCount; r++)
                    {
                        for (int c = 0; c < cCount; c++)
                        {
                            if (curGrid[r, c] <= 0) continue;
                            (int tr, int tc) = (r == myR && c == myC) ? choice : targets[r, c];
                            if (tr >= 0 && tc >= 0)
                            {
                                nextGrid[tr, tc] -= curGrid[r, c];
                            }
                        }
                    }

                    // Zero out destroyed nations
                    for (int r = 0; r < rCount; r++)
                    for (int c = 0; c < cCount; c++)
                        if (nextGrid[r, c] < 0) nextGrid[r, c] = 0;

                    if (nextGrid[myR, myC] <= 0)
                    {
                        // We died on day
                        if (day - 1 > maxDays)
                            maxDays = day - 1;
                    }
                    else
                    {
                        if (day > maxDays)
                            maxDays = day;

                        if (Dfs(nextGrid, day + 1))
                            return true;
                    }
                }

                return false;
            }

            if (Dfs(grid, 1))
                return "forever";

            return maxDays + " day(s)";
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

        #region Question 3 - Millionaire

        private Func<double> Q3Solver(GCJTestCase test)
        {
            test.Get(out int m, out double p, out int x);
            return () => Q3Solver(m, p, x);
        }

        private double Q3Solver(int m, double p, int x)
        {
            if (x >= 1_000_000)
                return 1.0;
            if (x <= 0)
                return 0.0;

            double[] dp = { 0.0, 1.0 };

            for (int round = 1; round <= m; round++)
            {
                int curCount = 1 << round;
                double[] nextDp = new double[curCount + 1];

                for (int i = 0; i <= curCount; i++)
                {
                    double maxP = 0.0;
                    int minA = Math.Max(0, i - (1 << (round - 1)));
                    int maxA = i / 2;

                    for (int a = minA; a <= maxA; a++)
                    {
                        int b = i - a;
                        double val = p * dp[b] + (1.0 - p) * dp[a];
                        if (val > maxP)
                            maxP = val;
                    }

                    nextDp[i] = maxP;
                }

                dp = nextDp;
            }

            int index = (int)((long)x * (1 << m) / 1_000_000);
            return dp[index];
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, 1e-6);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, 1e-6);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver, 1e-6);
        }

        #endregion

        #region Question 4 - Modern Art Plagiarism

        private Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            int[][] edges1 = new int[n - 1][];
            for (int i = 0; i < n - 1; i++)
            {
                test.Get(out int u, out int v);
                edges1[i] = new[] { u - 1, v - 1 };
            }

            test.Get(out int m);
            int[][] edges2 = new int[m - 1][];
            for (int i = 0; i < m - 1; i++)
            {
                test.Get(out int u, out int v);
                edges2[i] = new[] { u - 1, v - 1 };
            }

            return () => Q4Solver(n, edges1, m, edges2);
        }

        private string Q4Solver(int n, int[][] edges1, int m, int[][] edges2)
        {
            if (m == 1)
                return "YES";

            var adj1 = new List<int>[n];
            for (int i = 0; i < n; i++) adj1[i] = new List<int>();
            foreach (var e in edges1)
            {
                adj1[e[0]].Add(e[1]);
                adj1[e[1]].Add(e[0]);
            }

            var adj2 = new List<int>[m];
            for (int i = 0; i < m; i++) adj2[i] = new List<int>();
            foreach (var e in edges2)
            {
                adj2[e[0]].Add(e[1]);
                adj2[e[1]].Add(e[0]);
            }

            // Root tree 2 at node 0
            var children2 = new List<int>[m];
            for (int i = 0; i < m; i++) children2[i] = new List<int>();

            void BuildTree2(int u, int p)
            {
                foreach (int v in adj2[u])
                {
                    if (v == p) continue;
                    children2[u].Add(v);
                    BuildTree2(v, u);
                }
            }
            BuildTree2(0, -1);

            // Memoized fits(u, parentU, v)
            // parentU can be -1 (encoded as n)
            int[,,] memo = new int[n, n + 1, m]; // 0: uncomputed, 1: true, 2: false

            bool Fits(int u, int parentU, int v)
            {
                int pIdx = parentU == -1 ? n : parentU;
                if (memo[u, pIdx, v] != 0)
                    return memo[u, pIdx, v] == 1;

                var ch2 = children2[v];
                if (ch2.Count == 0)
                {
                    memo[u, pIdx, v] = 1;
                    return true;
                }

                var ch1 = new List<int>();
                foreach (int w in adj1[u])
                {
                    if (w == parentU) continue;
                    ch1.Add(w);
                }

                if (ch1.Count < ch2.Count)
                {
                    memo[u, pIdx, v] = 2;
                    return false;
                }

                // Construct bipartite graph between ch2 and ch1
                var bg = new List<int>[ch2.Count];
                for (int i = 0; i < ch2.Count; i++)
                {
                    bg[i] = new List<int>();
                    for (int j = 0; j < ch1.Count; j++)
                    {
                        if (Fits(ch1[j], u, ch2[i]))
                            bg[i].Add(j);
                    }
                }

                bool matchAll = BipartiteMatching.CanMatchAllLeft(ch2.Count, ch1.Count, bg);
                memo[u, pIdx, v] = matchAll ? 1 : 2;
                return matchAll;
            }

            for (int r = 0; r < n; r++)
            {
                if (Fits(r, -1, 0))
                    return "YES";
            }

            return "NO";
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

