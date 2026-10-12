#region License

/*
Copyright (c) 2016, the TMD.Algo authors.
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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2016
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Rather Perplexing Showdown

        private Func<string> Q1Solver(GCJTestCase test)
        {
            int n, r, p, s;
            test.Get(out n, out r, out p, out s);
            return () => Q1Solver(n, r, p, s);
        }

        private string Q1Solver(int n, int r, int p, int s)
        {
            string best = null;
            char[] candidates = { 'P', 'R', 'S' };
            foreach (char winner in candidates)
            {
                string lineup = BuildLineup(winner, n);
                int countR = 0, countP = 0, countS = 0;
                foreach (char c in lineup)
                {
                    if (c == 'R') countR++;
                    else if (c == 'P') countP++;
                    else if (c == 'S') countS++;
                }

                if (countR == r && countP == p && countS == s)
                {
                    if (best == null || string.CompareOrdinal(lineup, best) < 0)
                        best = lineup;
                }
            }

            return best ?? "IMPOSSIBLE";
        }

        private string BuildLineup(char winner, int depth)
        {
            if (depth == 0)
                return winner.ToString();

            char c1 = winner;
            char c2;
            switch (winner)
            {
                case 'P': c2 = 'R'; break;
                case 'R': c2 = 'S'; break;
                case 'S': c2 = 'P'; break;
                default: throw new ArgumentException("Unknown move: " + winner);
            }

            string s1 = BuildLineup(c1, depth - 1);
            string s2 = BuildLineup(c2, depth - 1);
            return string.CompareOrdinal(s1, s2) <= 0 ? s1 + s2 : s2 + s1;
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

        #region Question 2 - Red Tape Committee

        private Func<double> Q2Solver(GCJTestCase test)
        {
            int n, k;
            test.Get(out n, out k);
            double[] p;
            test.Get(out p);
            return () => Q2Solver(n, k, p);
        }

        private double Q2Solver(int n, int k, double[] p)
        {
            Array.Sort(p);
            double maxTieProb = 0.0;
            double[] chosen = new double[k];
            int half = k / 2;

            for (int m = 0; m <= k; m++)
            {
                // Take m elements from the left, k - m from the right
                for (int i = 0; i < m; i++)
                    chosen[i] = p[i];
                for (int i = 0; i < k - m; i++)
                    chosen[m + i] = p[n - (k - m) + i];

                double[] pmf = MathFormulas.PoissonBinomialPmf(chosen, maxSuccesses: half);
                if (pmf[half] > maxTieProb)
                    maxTieProb = pmf[half];
            }

            return maxTieProb;
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

        #region Question 3 - The Gardener of Seville

        private Func<IEnumerable<string>> Q3Solver(GCJTestCase test)
        {
            int r, c;
            test.Get(out r, out c);
            int[] permutation;
            test.Get(out permutation);
            return () => Q3Solver(r, c, permutation);
        }

        private IEnumerable<string> Q3Solver(int r, int c, int[] permutation)
        {
            char?[,] board = new char?[r, c];
            int size = 2 * (r + c);
            Modulo modPerimeter = new Modulo(size);
            Modulo modDir = new Modulo(4);
            int pairCount = r + c;
            var pairs = new List<(int start, int end)>(pairCount);
            for (int i = 0; i < pairCount; i++)
            {
                int a = permutation[2 * i];
                int b = permutation[2 * i + 1];
                pairs.Add((a, b));
            }

            pairs.Sort((p1, p2) =>
            {
                long dist1 = modPerimeter.Dist(p1.end, p1.start);
                long dist2 = modPerimeter.Dist(p2.end, p2.start);
                return dist1.CompareTo(dist2);
            });

            int[] dx = { 0, -1, 0, 1 };
            int[] dy = { 1, 0, -1, 0 };

            foreach (var pair in pairs)
            {
                int start = pair.start;
                int end = pair.end;
                if (modPerimeter.Subtract(start, end) > r + c)
                {
                    int tmp = start;
                    start = end;
                    end = tmp;
                }

                var (dir, x, y) = GetPosition(start, r, c);
                var (_, targetX, targetY) = GetPosition(end, r, c);

                x += dx[dir];
                y += dy[dir];

                while (x >= 0 && x < c && y >= 0 && y < r)
                {
                    if (!board[y, x].HasValue)
                    {
                        board[y, x] = (dir & 1) == 0 ? '/' : '\\';
                    }

                    char hedge = board[y, x].Value;
                    long turn = (hedge == '/') ? ((dir & 1) == 0 ? 1 : 3) : ((dir & 1) == 0 ? 3 : 1);
                    dir = (int)modDir.Add(dir, turn);
                    x += dx[dir];
                    y += dy[dir];
                }

                if (x != targetX || y != targetY)
                {
                    return new[] { "IMPOSSIBLE" };
                }
            }

            string[] result = new string[r];
            for (int i = 0; i < r; i++)
            {
                char[] row = new char[c];
                for (int j = 0; j < c; j++)
                {
                    row[j] = board[i, j] ?? '/';
                }
                result[i] = new string(row);
            }
            return result;
        }

        private static (int direction, int x, int y) GetPosition(int v, int r, int c)
        {
            if (v <= c) return (0, v - 1, -1);
            v -= c;
            if (v <= r) return (1, c, v - 1);
            v -= r;
            if (v <= c) return (2, c - v, r);
            v -= c;
            return (3, -1, r - v);
        }

        private void ValidateGardener(string input, string expectedOutput, List<string> result)
        {
            string[] inLines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0]);
            int inIdx = 1;

            List<string> resLines = new List<string>();
            foreach (var r in result)
            {
                resLines.AddRange(r.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }

            int resIdx = 0;
            for (int caseNum = 1; caseNum <= t; caseNum++)
            {
                string header = $"Case #{caseNum}:";
                Assert.AreEqual(header, resLines[resIdx++].TrimEnd());

                string[] rc = inLines[inIdx++].Split(' ');
                int r = int.Parse(rc[0]);
                int c = int.Parse(rc[1]);
                int[] perm = inLines[inIdx++].Split(' ').Select(int.Parse).ToArray();

                if (resLines[resIdx] == "IMPOSSIBLE")
                {
                    resIdx++;
                    continue;
                }

                string[] grid = new string[r];
                for (int i = 0; i < r; i++)
                {
                    grid[i] = resLines[resIdx++];
                }

                Assert.IsTrue(CheckGrid(r, c, perm, grid), $"Case #{caseNum} produced an invalid hedge maze.");
            }
        }

        private static bool CheckGrid(int r, int c, int[] permutation, string[] gridRows)
        {
            if (gridRows.Length != r) return false;
            foreach (var row in gridRows)
            {
                if (row.Length != c) return false;
                foreach (char ch in row)
                    if (ch != '/' && ch != '\\') return false;
            }

            int numLovers = 2 * (r + c);
            int[] connections = new int[numLovers + 1];
            for (int i = 0; i < r + c; i++)
            {
                int a = permutation[2 * i];
                int b = permutation[2 * i + 1];
                connections[a] = b;
                connections[b] = a;
            }

            int[,] grid = new int[r + 2, c + 2];
            var loverStats = new (int row, int col, int entrySide)[numLovers + 1];

            int loverNum = 1;
            for (int n = 0; n < c; n++)
            {
                grid[0, n + 1] = loverNum;
                loverStats[loverNum] = (1, 1 + n, 1);
                loverNum++;
            }
            for (int n = 0; n < r; n++)
            {
                grid[1 + n, c + 1] = loverNum;
                loverStats[loverNum] = (1 + n, c, 2);
                loverNum++;
            }
            for (int n = 0; n < c; n++)
            {
                grid[r + 1, c - n] = loverNum;
                loverStats[loverNum] = (r, c - n, 3);
                loverNum++;
            }
            for (int n = 0; n < r; n++)
            {
                grid[r - n, 0] = loverNum;
                loverStats[loverNum] = (r - n, 1, 0);
                loverNum++;
            }

            for (int i = 1; i <= numLovers; i++)
            {
                var (currR, currC, currSide) = loverStats[i];
                while (currR >= 1 && currR <= r && currC >= 1 && currC <= c)
                {
                    char hedge = gridRows[currR - 1][currC - 1];
                    int rOff, cOff, nextSide;
                    if (hedge == '/')
                    {
                        switch (currSide)
                        {
                            case 0: rOff = -1; cOff = 0; nextSide = 3; break;
                            case 1: rOff = 0; cOff = -1; nextSide = 2; break;
                            case 2: rOff = 1; cOff = 0; nextSide = 1; break;
                            case 3: rOff = 0; cOff = 1; nextSide = 0; break;
                            default: return false;
                        }
                    }
                    else
                    {
                        switch (currSide)
                        {
                            case 0: rOff = 1; cOff = 0; nextSide = 1; break;
                            case 1: rOff = 0; cOff = 1; nextSide = 0; break;
                            case 2: rOff = -1; cOff = 0; nextSide = 3; break;
                            case 3: rOff = 0; cOff = -1; nextSide = 2; break;
                            default: return false;
                        }
                    }
                    currR += rOff;
                    currC += cOff;
                    currSide = nextSide;
                }

                int outlet = grid[currR, currC];
                if (outlet != connections[i])
                    return false;
            }

            return true;
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, ValidateGardener);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, ValidateGardener);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver, ValidateGardener);
        }

        #endregion

        #region Question 4 - Freeform Factory

        private Func<int> Q4Solver(GCJTestCase test)
        {
            int n;
            test.Get(out n);
            string[] grid = new string[n];
            for (int i = 0; i < n; i++)
                test.Get(out grid[i]);
            return () => Q4Solver(n, grid);
        }

        private int Q4Solver(int n, string[] grid)
        {
            int initialEdges = 0;
            DisjointTracker<int> dsu = new DisjointTracker<int>();
            for (int i = 0; i < 2 * n; i++)
                dsu.Add(i);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (grid[i][j] == '1')
                    {
                        initialEdges++;
                        dsu.Union(i, n + j);
                    }
                }
            }

            var compWorkers = new Dictionary<int, int>();
            var compMachines = new Dictionary<int, int>();

            for (int i = 0; i < n; i++)
            {
                int root = dsu.GetRepresentative(i);
                compWorkers[root] = compWorkers.GetValueOrDefault(root) + 1;
            }
            for (int j = 0; j < n; j++)
            {
                int root = dsu.GetRepresentative(n + j);
                compMachines[root] = compMachines.GetValueOrDefault(root) + 1;
            }

            var allRoots = new HashSet<int>(compWorkers.Keys.Concat(compMachines.Keys));
            int balancedCost = 0;
            int w0 = 0;
            var compEdges = new List<(int p, int q)>();

            foreach (int root in allRoots)
            {
                int p = compWorkers.GetValueOrDefault(root, 0);
                int q = compMachines.GetValueOrDefault(root, 0);
                if (p == q)
                {
                    balancedCost += p * p;
                }
                else if (p > 0 && q == 0)
                {
                    w0 += p;
                }
                else if (p == 0 && q > 0)
                {
                    // Isolated machines are accounted for via balance
                }
                else
                {
                    compEdges.Add((p, q));
                }
            }

            int m = compEdges.Count;
            int numMasks = 1 << m;
            int[] sumP = new int[numMasks];
            int[] sumQ = new int[numMasks];
            int[] baseCost = new int[numMasks];
            int[] baseW = new int[numMasks];
            int[] baseU = new int[numMasks];

            for (int mask = 1; mask < numMasks; mask++)
            {
                int lowestBit = BitTricks.KeepLeastSignificantSetBit(mask);
                int bitIdx = BitTricks.LeastSignificantSetBitPos(mask);
                int prev = mask ^ lowestBit;
                sumP[mask] = sumP[prev] + compEdges[bitIdx].p;
                sumQ[mask] = sumQ[prev] + compEdges[bitIdx].q;

                int p = sumP[mask];
                int q = sumQ[mask];
                int r = Math.Max(p, q);
                baseCost[mask] = r * r;
                baseW[mask] = Math.Max(0, q - p);
                baseU[mask] = Math.Max(0, p - q);
            }

            var search = Memotize.CreateMemotizedRecursive<int>(
                0, numMasks - 1,
                0, w0,
                (recurse, remMask, remW) =>
                {
                    if (remMask == 0)
                    {
                        return remW;
                    }

                    int remM = remW - (sumQ[remMask] - sumP[remMask]);
                    int lowestBit = BitTricks.KeepLeastSignificantSetBit(remMask);
                    int other = remMask ^ lowestBit;
                    int best = int.MaxValue;

                    for (int sub = other; ; sub = BitTricks.PrevSubset(sub, other))
                    {
                        int candidate = sub | lowestBit;
                        int reqW = baseW[candidate];
                        int reqU = baseU[candidate];
                        if (reqW <= remW && reqU <= remM)
                        {
                            int res = recurse(remMask ^ candidate, remW - reqW);
                            if (res != int.MaxValue)
                            {
                                int total = baseCost[candidate] + res;
                                if (total < best) best = total;
                            }
                        }
                        if (sub == 0) break;
                    }

                    return best;
                });

            int minSquares = balancedCost + search((1 << m) - 1, w0);
            return minSquares - initialEdges;
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
