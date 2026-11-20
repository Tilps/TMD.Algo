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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2011
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Irregular Cakes

        private static Func<string[]> Q1Solver(GCJTestCase test)
        {
            test.Get(out int _, out int l, out int u, out int g);
            test.GetLines(l, out (int X, int Y)[] lower);
            test.GetLines(u, out (int X, int Y)[] upper);
            return () => Q1Solver(g, lower, upper);
        }

        private static string[] Q1Solver(int g, (int X, int Y)[] lower, (int X, int Y)[] upper)
        {
            List<int> xs = [with(capacity: lower.Length + upper.Length)];
            int iL = 0, iU = 0;
            while (iL < lower.Length || iU < upper.Length)
            {
                int nextX;
                if (iL < lower.Length && iU < upper.Length)
                {
                    if (lower[iL].X < upper[iU].X)
                        nextX = lower[iL++].X;
                    else if (upper[iU].X < lower[iL].X)
                        nextX = upper[iU++].X;
                    else
                    {
                        nextX = lower[iL].X;
                        iL++;
                        iU++;
                    }
                }
                else if (iL < lower.Length)
                {
                    nextX = lower[iL++].X;
                }
                else
                {
                    nextX = upper[iU++].X;
                }
                xs.Add(nextX);
            }

            int m = xs.Count;
            double[] heights = new double[m];
            int pL = 0, pU = 0;
            for (int i = 0; i < m; i++)
            {
                int x = xs[i];
                while (pL + 1 < lower.Length && lower[pL + 1].X < x)
                    pL++;
                while (pU + 1 < upper.Length && upper[pU + 1].X < x)
                    pU++;

                double yL = x == lower[pL].X
                    ? lower[pL].Y
                    : lower[pL].Y + (lower[pL + 1].Y - lower[pL].Y) * (double)(x - lower[pL].X) / (lower[pL + 1].X - lower[pL].X);

                double yU = x == upper[pU].X
                    ? upper[pU].Y
                    : upper[pU].Y + (upper[pU + 1].Y - upper[pU].Y) * (double)(x - upper[pU].X) / (upper[pU + 1].X - upper[pU].X);

                heights[i] = yU - yL;
            }

            double[] slabArea = new double[m - 1];
            double totalArea = 0;
            for (int i = 0; i < m - 1; i++)
            {
                slabArea[i] = 0.5 * (heights[i] + heights[i + 1]) * (xs[i + 1] - xs[i]);
                totalArea += slabArea[i];
            }

            double sliceArea = totalArea / g;
            string[] cuts = new string[g - 1];
            double cumArea = 0;
            int cut = 1;

            for (int i = 0; i < m - 1 && cut < g; i++)
            {
                double area = slabArea[i];
                double dx = xs[i + 1] - xs[i];
                double h0 = heights[i];
                double slope = (heights[i + 1] - h0) / dx;

                while (cut < g && (cumArea + area >= cut * sliceArea - 1e-12 || i == m - 2))
                {
                    double req = Math.Clamp(cut * sliceArea - cumArea, 0.0, area);
                    double disc = Math.Max(0.0, h0 * h0 + 2.0 * slope * req);
                    double t = req == 0.0 ? 0.0 : Math.Clamp((2.0 * req) / (h0 + Math.Sqrt(disc)), 0.0, dx);
                    cuts[cut - 1] = (xs[i] + t).ToString("F9", CultureInfo.InvariantCulture);
                    cut++;
                }

                cumArea += area;
            }

            return cuts;
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

        #region Question 2 - Dire Straights

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int[] tokens);
            return () => Q2Solver(tokens);
        }

        private static int Q2Solver(int[] tokens)
        {
            int n = tokens[0];
            if (n == 0)
                return 0;

            int maxCard = 0;
            for (int i = 1; i <= n; i++)
            {
                if (tokens[i] > maxCard)
                    maxCard = tokens[i];
            }

            int[] cnt = new int[maxCard + 2];
            for (int i = 1; i <= n; i++)
                cnt[tokens[i]]++;

            int best = n;
            Queue<int> starts = new();
            for (int v = 1; v <= maxCard + 1; v++)
            {
                int diff = cnt[v] - cnt[v - 1];
                if (diff > 0)
                {
                    for (int k = 0; k < diff; k++)
                        starts.Enqueue(v);
                }
                else if (diff < 0)
                {
                    for (int k = 0; k < -diff; k++)
                    {
                        int len = v - starts.Dequeue();
                        if (len < best)
                            best = len;
                    }
                }
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
            Test(Q2Solver);
        }

        #endregion

        #region Question 3 - Perpetual Motion

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] grid);
            return () => Q3Solver(r, c, grid);
        }

        private static long Q3Solver(int r, int c, string[] grid)
        {
            (int First, int Second)[] edges = new (int First, int Second)[r * c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    int dr1, dc1, dr2, dc2;
                    switch (grid[i][j])
                    {
                        case '|':
                            dr1 = -1; dc1 = 0; dr2 = 1; dc2 = 0;
                            break;
                        case '-':
                            dr1 = 0; dc1 = -1; dr2 = 0; dc2 = 1;
                            break;
                        case '/':
                            dr1 = -1; dc1 = 1; dr2 = 1; dc2 = -1;
                            break;
                        default:
                            dr1 = -1; dc1 = -1; dr2 = 1; dc2 = 1;
                            break;
                    }
                    int r1 = (i + dr1 + r) % r;
                    int c1 = (j + dc1 + c) % c;
                    int r2 = (i + dr2 + r) % r;
                    int c2 = (j + dc2 + c) % c;
                    edges[i * c + j] = (r1 * c + c1, r2 * c + c2);
                }
            }

            return BipartiteMatching.CountPerfectMatchingsLeftDegreeTwo(r * c, edges, 1_000_003L);
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

        #region Question 4 - Mystery Square

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out string pattern);
            return () => Q4Solver(pattern);
        }

        private static string Q4Solver(string pattern)
        {
            return SolveSquare(pattern) ?? throw new InvalidOperationException("No valid perfect square found.");
        }

        private static string FormatBinary(UInt128 value, int length)
        {
            char[] chars = new char[length];
            for (int bit = 0; bit < length; bit++)
                chars[length - 1 - bit] = ((value >> bit) & 1) != 0 ? '1' : '0';
            return new string(chars);
        }

        private static bool MatchesPattern(UInt128 sq, int len, UInt128 knownMask, UInt128 knownVal)
        {
            if (sq < ((UInt128)1 << (len - 1)))
                return false;
            if (len < 128 && sq >= ((UInt128)1 << len))
                return false;
            return (sq & knownMask) == knownVal;
        }

        private static string? SolveSquare(string pattern)
        {
            if (pattern == "1")
                return "1";

            int len = pattern.Length;
            // Branch 1: odd square (must satisfy N mod 8 == 1, i.e., ...001 for len >= 3, or ...01 for len == 2)
            if (pattern[^1] != '0' && (len < 2 || pattern[^2] != '1') && (len < 3 || pattern[^3] != '1'))
            {
                char[] oddChars = pattern.ToCharArray();
                oddChars[^1] = '1';
                if (len >= 2) oddChars[^2] = '0';
                if (len >= 3) oddChars[^3] = '0';

                int topLen = len / 2;
                int qTop = 0, qBot = 0;
                for (int i = 0; i < topLen; i++)
                {
                    if (oddChars[i] == '?')
                        qTop++;
                }
                for (int i = topLen; i < len; i++)
                {
                    if (oddChars[i] == '?')
                        qBot++;
                }

                UInt128 knownMask = 0;
                UInt128 knownVal = 0;
                for (int i = 0; i < len; i++)
                {
                    if (oddChars[i] != '?')
                    {
                        int bit = len - 1 - i;
                        UInt128 bMask = (UInt128)1 << bit;
                        knownMask |= bMask;
                        if (oddChars[i] == '1')
                            knownVal |= bMask;
                    }
                }

                if (qTop <= qBot)
                {
                    int[] topBitPositions = new int[qTop];
                    int tIdx = 0;
                    for (int i = 0; i < topLen; i++)
                    {
                        if (oddChars[i] == '?')
                            topBitPositions[tIdx++] = len - 1 - i;
                    }

                    UInt128 botAllOnes = 0;
                    for (int i = topLen; i < len; i++)
                    {
                        if (oddChars[i] == '?')
                            botAllOnes |= (UInt128)1 << (len - 1 - i);
                    }

                    int limit = 1 << qTop;
                    for (int mask = 0; mask < limit; mask++)
                    {
                        UInt128 nMax = knownVal | botAllOnes;
                        for (int b = 0; b < qTop; b++)
                        {
                            if ((mask & (1 << b)) != 0)
                                nMax |= (UInt128)1 << topBitPositions[b];
                        }

                        ulong y = MathFormulas.IntegerSqrt(nMax);
                        UInt128 sq = (UInt128)y * y;
                        if (MatchesPattern(sq, len, knownMask, knownVal))
                            return FormatBinary(sq, len);
                    }
                }
                else
                {
                    int m = (len + 1) / 2;
                    ulong msbBit = 1UL << (m - 1);

                    string? LiftDfs(int j, ulong r)
                    {
                        if (j == m - 1)
                        {
                            ulong y = r | msbBit;
                            UInt128 sq = (UInt128)y * y;
                            if (MatchesPattern(sq, len, knownMask, knownVal))
                                return FormatBinary(sq, len);
                            return null;
                        }

                        int targetBitIdx = j + 1;
                        char ch = oddChars[len - 1 - targetBitIdx];
                        UInt128 rSq = (UInt128)r * r;
                        int sqBit = (int)((rSq >> targetBitIdx) & 1);

                        if (ch != '?')
                        {
                            int targetBit = ch - '0';
                            ulong bj = (ulong)(sqBit ^ targetBit);
                            return LiftDfs(j + 1, r | (bj << j));
                        }

                        return LiftDfs(j + 1, r) ?? LiftDfs(j + 1, r | (1UL << j));
                    }

                    string? found = m == 2
                        ? LiftDfs(m - 1, 3UL)
                        : LiftDfs(2, 1UL) ?? LiftDfs(2, 3UL);
                    if (found != null)
                        return found;
                }
            }

            // Branch 2: even square (divisible by 4, strip trailing "00")
            if (len >= 3 && pattern[^1] != '1' && pattern[^2] != '1')
            {
                string? prefixAns = SolveSquare(pattern[..^2]);
                if (prefixAns != null)
                    return prefixAns + "00";
            }

            return null;
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
