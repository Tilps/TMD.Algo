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
    public class AmerTest : BaseGCJTest
    {
        #region Question 1 - Mixing Bowls

        private Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out string[] lines);
            return () => Q1Solver(lines);
        }

        private int Q1Solver(string[] lines)
        {
            var childMixtures = new Dictionary<string, List<string>>();
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string name = parts[0];
                var children = new List<string>();
                for (int j = 2; j < parts.Length; j++)
                {
                    if (char.IsUpper(parts[j][0]))
                        children.Add(parts[j]);
                }
                childMixtures[name] = children;
            }

            string root = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

            var getBowls = Memotize.CreateMemotizedRecursive<string, int>((recurse, name) =>
            {
                var children = childMixtures[name];
                if (children.Count == 0)
                    return 1;

                var childBowls = new List<int>(children.Count);
                foreach (var child in children)
                    childBowls.Add(recurse(child));

                childBowls.Sort((a, b) => b.CompareTo(a));

                int maxReq = 0;
                for (int i = 0; i < childBowls.Count; i++)
                    maxReq = Math.Max(maxReq, childBowls[i] + i);

                return Math.Max(children.Count + 1, maxReq);
            });

            return getBowls(root);
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

        #region Question 2 - Code Sequence

        private const int AnyDiff = -1;
        private static readonly Modulo Mod10007 = new Modulo(10007);

        private Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out long[] s);
            return () => Q2Solver(n, s);
        }

        private string Q2Solver(int n, long[] s)
        {
            if (n <= 1)
                return "UNKNOWN";

            int diffCount = n - 1;
            int[] d = new int[diffCount];
            for (int i = 0; i < diffCount; i++)
                d[i] = (int)Mod10007.Subtract(s[i + 1], s[i]);

            var nextDiffs = GetPossibleNextDiffs(d);
            if (nextDiffs.Contains(AnyDiff) || nextDiffs.Count != 1)
                return "UNKNOWN";

            int nextDiff = nextDiffs.First();
            long nextVal = Mod10007.Add(s[n - 1], nextDiff);
            return nextVal.ToString();
        }

        private static HashSet<int> GetPossibleNextDiffs(int[] d)
        {
            int len = d.Length;
            if (len <= 1)
                return new HashSet<int> { AnyDiff };

            var result = new HashSet<int>();
            for (int p = 0; p < 2; p++)
            {
                int c = d[p];
                bool allEqual = true;
                for (int i = p + 2; i < len; i += 2)
                {
                    if (d[i] != c)
                    {
                        allEqual = false;
                        break;
                    }
                }
                if (!allEqual)
                    continue;

                int subLen = 0;
                for (int i = 1 - p; i < len; i += 2)
                    subLen++;

                int[] subD = new int[subLen];
                int idx = 0;
                for (int i = 1 - p; i < len; i += 2)
                    subD[idx++] = d[i];

                var subNext = GetPossibleNextDiffs(subD);
                if (subNext.Count == 0)
                    continue;

                int nextParity = len % 2;
                if (nextParity == p)
                {
                    result.Add(c);
                }
                else
                {
                    foreach (var val in subNext)
                        result.Add(val);
                }
            }
            return result;
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

        #region Question 3 - Test Passing Probability

        private Func<double> Q3Solver(GCJTestCase test)
        {
            test.Get(out int m, out int q);
            double[][] probs = new double[q][];
            for (int i = 0; i < q; i++)
                test.Get(out probs[i]);

            return () => Q3Solver(m, q, probs);
        }

        private double Q3Solver(int m, int q, double[][] probs)
        {
            List<double> current = new List<double> { 1.0 };
            var descComparer = Comparer<double>.Create((a, b) => b.CompareTo(a));

            for (int i = 0; i < q; i++)
            {
                var next = new List<double>(current.Count * 4);
                foreach (double p in current)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        double val = p * probs[i][j];
                        if (val > 1e-15)
                            next.Add(val);
                    }
                }

                if (next.Count > m)
                {
                    SpecialtySelect.SelectRandomizedInPlace(next, m - 1, descComparer);
                    next.RemoveRange(m, next.Count - m);
                }
                current = next;
            }

            double sum = 0;
            for (int i = 0; i < Math.Min(m, current.Count); i++)
                sum += current[i];

            return Math.Min(1.0, sum);
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

        #region Question 4 - King

        private Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] board);
            return () => Q4Solver(r, c, board);
        }

        private string Q4Solver(int r, int c, string[] board)
        {
            int gx = c;
            int gy = r;
            char[,] g;

            if (gx > gy)
            {
                gx = r;
                gy = c;
                g = new char[gy, gx];
                for (int y = 0; y < gy; y++)
                for (int x = 0; x < gx; x++)
                    g[y, x] = board[x][y];
            }
            else
            {
                g = new char[gy, gx];
                for (int y = 0; y < gy; y++)
                for (int x = 0; x < gx; x++)
                    g[y, x] = board[y][x];
            }

            int kx = -1, ky = -1;
            for (int y = 0; y < gy; y++)
            for (int x = 0; x < gx; x++)
            {
                if (g[y, x] == 'K')
                {
                    kx = x;
                    ky = y;
                }
            }

            int m1 = SolveMatching(g, gx, gy);

            g[ky, kx] = '.';
            int m2 = SolveMatching(g, gx, gy);

            return m2 > m1 ? "A" : "B";
        }

        private static int SolveMatching(char[,] g, int gx, int gy)
        {
            int stateCount = 1 << (gx + 1);
            int mask = stateCount - 1;

            int[] dp = new int[stateCount];
            int[] nextDp = new int[stateCount];
            int[] reachable = new int[stateCount];
            int[] nextReachable = new int[stateCount];

            Array.Fill(dp, -1);
            Array.Fill(nextDp, -1);

            dp[0] = 0;
            reachable[0] = 0;
            int reachableCount = 1;

            for (int y = 0; y < gy; y++)
            {
                for (int x = 0; x < gx; x++)
                {
                    char cell = g[y, x];
                    int nextCount = 0;

                    for (int i = 0; i < reachableCount; i++)
                    {
                        int b = reachable[i];
                        int val = dp[b];
                        int b2 = (b << 1) & mask;

                        if (cell != '.')
                        {
                            if (nextDp[b2] < 0)
                            {
                                nextDp[b2] = val;
                                nextReachable[nextCount++] = b2;
                            }
                            else if (val > nextDp[b2])
                            {
                                nextDp[b2] = val;
                            }
                        }
                        else
                        {
                            // 1. Leave cell (x, y) unmatched (marked available for future neighbors)
                            int nb1 = b2 | 1;
                            if (nextDp[nb1] < 0)
                            {
                                nextDp[nb1] = val;
                                nextReachable[nextCount++] = nb1;
                            }
                            else if (val > nextDp[nb1])
                            {
                                nextDp[nb1] = val;
                            }

                            // 2. Match with Left (x - 1, y)
                            if (x > 0 && (b & 1) != 0)
                            {
                                int nb = b2 & ~2;
                                if (nextDp[nb] < 0)
                                {
                                    nextDp[nb] = val + 1;
                                    nextReachable[nextCount++] = nb;
                                }
                                else if (val + 1 > nextDp[nb])
                                {
                                    nextDp[nb] = val + 1;
                                }
                            }

                            // 3. Match with Up-Left (x - 1, y - 1)
                            if (x > 0 && (b & (1 << gx)) != 0)
                            {
                                int nb = b2;
                                if (nextDp[nb] < 0)
                                {
                                    nextDp[nb] = val + 1;
                                    nextReachable[nextCount++] = nb;
                                }
                                else if (val + 1 > nextDp[nb])
                                {
                                    nextDp[nb] = val + 1;
                                }
                            }

                            // 4. Match with Up (x, y - 1)
                            if ((b & (1 << (gx - 1))) != 0)
                            {
                                int nb = b2 & ~(1 << gx);
                                if (nextDp[nb] < 0)
                                {
                                    nextDp[nb] = val + 1;
                                    nextReachable[nextCount++] = nb;
                                }
                                else if (val + 1 > nextDp[nb])
                                {
                                    nextDp[nb] = val + 1;
                                }
                            }

                            // 5. Match with Up-Right (x + 1, y - 1)
                            if (gx >= 2 && x < gx - 1 && (b & (1 << (gx - 2))) != 0)
                            {
                                int nb = b2 & ~(1 << (gx - 1));
                                if (nextDp[nb] < 0)
                                {
                                    nextDp[nb] = val + 1;
                                    nextReachable[nextCount++] = nb;
                                }
                                else if (val + 1 > nextDp[nb])
                                {
                                    nextDp[nb] = val + 1;
                                }
                            }
                        }

                        // Reset dp entry for state b
                        dp[b] = -1;
                    }

                    // Swap buffers
                    var tempArr = dp;
                    dp = nextDp;
                    nextDp = tempArr;

                    var tempReach = reachable;
                    reachable = nextReachable;
                    nextReachable = tempReach;

                    reachableCount = nextCount;
                }
            }

            int maxMatching = 0;
            for (int i = 0; i < reachableCount; i++)
            {
                if (dp[reachable[i]] > maxMatching)
                    maxMatching = dp[reachable[i]];
            }

            return maxMatching;
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

