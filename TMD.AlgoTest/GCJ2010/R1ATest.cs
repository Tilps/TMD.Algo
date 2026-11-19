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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Rotate

        private static readonly (int Dr, int Dc)[] WinDirections = [(0, 1), (1, 0), (1, 1), (1, -1)];

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.GetLines(n, out string[] rows);
            return () => Q1Solver(n, k, rows);
        }

        private static string Q1Solver(int n, int k, string[] rows)
        {
            char[,] board = new char[n, n];
            for (int r = 0; r < n; r++)
            {
                int write = n - 1;
                for (int c = n - 1; c >= 0; c--)
                {
                    if (rows[r][c] != '.')
                        board[r, write--] = rows[r][c];
                }

                while (write >= 0)
                    board[r, write--] = '.';
            }

            bool red = false;
            bool blue = false;
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    char ch = board[r, c];
                    if (ch == '.')
                        continue;
                    if (ch == 'R' && red || ch == 'B' && blue)
                        continue;

                    foreach (var (dr, dc) in WinDirections)
                    {
                        int er = r + (k - 1) * dr;
                        int ec = c + (k - 1) * dc;
                        if ((uint)er >= (uint)n || (uint)ec >= (uint)n)
                            continue;

                        bool match = true;
                        for (int step = 1; step < k; step++)
                        {
                            if (board[r + step * dr, c + step * dc] != ch)
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            if (ch == 'R')
                                red = true;
                            else
                                blue = true;
                        }
                    }
                }
            }

            return (red, blue) switch
            {
                (true, true) => "Both",
                (true, false) => "Red",
                (false, true) => "Blue",
                _ => "Neither"
            };
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

        #region Question 2 - Make it Smooth

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int delCost, out int insCost, out int m, out int n);
            test.Get(out int[] pixels);
            return () => Q2Solver(delCost, insCost, m, n, pixels);
        }

        private static int Q2Solver(int delCost, int insCost, int m, int n, int[] pixels)
        {
            const int Values = 256;
            int[] dp = new int[Values];
            int[] afterIns = new int[Values];
            int[] bestPrev = new int[Values];
            int[] nextDp = new int[Values];

            for (int i = 0; i < n; i++)
            {
                int pixel = pixels[i];
                if (m == 0)
                {
                    Array.Copy(dp, bestPrev, Values);
                }
                else
                {
                    Array.Copy(dp, afterIns, Values);
                    for (int v = m; v < Values; v++)
                    {
                        int cand = afterIns[v - m] + insCost;
                        if (cand < afterIns[v])
                            afterIns[v] = cand;
                    }

                    for (int v = Values - 1 - m; v >= 0; v--)
                    {
                        int cand = afterIns[v + m] + insCost;
                        if (cand < afterIns[v])
                            afterIns[v] = cand;
                    }

                    OptimalFinding.SlidingWindowMin<int>(afterIns, m, m, bestPrev);
                }

                for (int v = 0; v < Values; v++)
                {
                    int deleteOption = dp[v] + delCost;
                    int keepOption = bestPrev[v] + Math.Abs(pixel - v);
                    nextDp[v] = Math.Min(deleteOption, keepOption);
                }

                (dp, nextDp) = (nextDp, dp);
            }

            int best = int.MaxValue;
            for (int v = 0; v < Values; v++)
            {
                if (dp[v] < best)
                    best = dp[v];
            }
            return best;
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

        #region Question 3 - Number Game

        private static bool IsGreaterThanPhiTimes(long x, long y)
        {
            // x > y * (1 + sqrt(5)) / 2  <=>  2*x - y > sqrt(5)*y.
            long lhs = 2 * x - y;
            return lhs > 0 && lhs * lhs > 5 * y * y;
        }

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out long a1, out long a2, out long b1, out long b2);
            return () => Q3Solver(a1, a2, b1, b2);
        }

        private static long Q3Solver(long a1, long a2, long b1, long b2)
        {
            double phi = MathFormulas.GoldenRatio;
            long total = 0;

            for (long b = b1; b <= b2; b++)
            {
                // Count a in [a1..a2] with a > phi * b.
                long lo = (long)Math.Floor(b * phi);
                while (lo > 1 && IsGreaterThanPhiTimes(lo - 1, b))
                    lo--;
                while (!IsGreaterThanPhiTimes(lo, b))
                    lo++;
                if (lo <= a2)
                {
                    long startA = Math.Max(a1, lo);
                    total += a2 - startA + 1;
                }

                // Count a in [a1..a2] with b > phi * a (i.e. a < b / phi).
                long hi = (long)Math.Ceiling(b / phi);
                while (IsGreaterThanPhiTimes(b, hi + 1))
                    hi++;
                while (hi >= 0 && !IsGreaterThanPhiTimes(b, hi))
                    hi--;
                if (hi >= a1)
                {
                    long endA = Math.Min(a2, hi);
                    total += endA - a1 + 1;
                }
            }

            return total;
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
    }
}
