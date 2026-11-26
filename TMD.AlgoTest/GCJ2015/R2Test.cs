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

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Pegman

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] grid);
            return () => Q1Solver(r, c, grid);
        }

        private static string Q1Solver(int r, int c, string[] grid)
        {
            int[] minRow = new int[c];
            int[] maxRow = new int[c];
            int[] minCol = new int[r];
            int[] maxCol = new int[r];
            Array.Fill(minRow, int.MaxValue);
            Array.Fill(maxRow, -1);
            Array.Fill(minCol, int.MaxValue);
            Array.Fill(maxCol, -1);

            for (int row = 0; row < r; row++)
            {
                string line = grid[row];
                for (int col = 0; col < c; col++)
                {
                    if (line[col] != '.')
                    {
                        if (row < minRow[col]) minRow[col] = row;
                        if (row > maxRow[col]) maxRow[col] = row;
                        if (col < minCol[row]) minCol[row] = col;
                        if (col > maxCol[row]) maxCol[row] = col;
                    }
                }
            }

            int changes = 0;
            for (int row = 0; row < r; row++)
            {
                string line = grid[row];
                for (int col = 0; col < c; col++)
                {
                    char ch = line[col];
                    if (ch == '.')
                        continue;

                    bool safe = ch switch
                    {
                        '^' => row > minRow[col],
                        'v' => row < maxRow[col],
                        '<' => col > minCol[row],
                        '>' => col < maxCol[row],
                        _ => true
                    };
                    if (safe)
                        continue;

                    if (minRow[col] < maxRow[col] || minCol[row] < maxCol[row])
                        changes++;
                    else
                        return "IMPOSSIBLE";
                }
            }

            return changes.ToString(CultureInfo.InvariantCulture);
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

        #region Question 2 - Kiddie Pool

        private static long ToFixed4(double val) => (long)Math.Round(val * 10000.0);

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out double v, out double x);
            test.GetLines(n, out double[] rates, out double[] temps);
            return () => Q2Solver(n, v, x, rates, temps);
        }

        private static string Q2Solver(int n, double vDouble, double xDouble, double[] ratesDouble, double[] tempsDouble)
        {
            long v = ToFixed4(vDouble);
            long x = ToFixed4(xDouble);

            (long Rate, long Diff)[] sources = new (long, long)[n];
            bool anyLeq = false;
            bool anyGeq = false;
            Int128 totalFlux = 0;
            long totalRate = 0;

            for (int i = 0; i < n; i++)
            {
                long r = ToFixed4(ratesDouble[i]);
                long diff = ToFixed4(tempsDouble[i]) - x;
                sources[i] = (r, diff);
                if (diff <= 0) anyLeq = true;
                if (diff >= 0) anyGeq = true;
                totalFlux += (Int128)r * diff;
                totalRate += r;
            }

            if (!anyLeq || !anyGeq)
                return "IMPOSSIBLE";

            Array.Sort(sources, static (a, b) => a.Diff.CompareTo(b.Diff));

            double maxRate = totalRate;
            if (totalFlux > 0)
            {
                for (int i = n - 1; i >= 0 && totalFlux > 0; i--)
                {
                    (long r, long diff) = sources[i];
                    Int128 flux = (Int128)r * diff;
                    if (totalFlux >= flux)
                    {
                        totalFlux -= flux;
                        totalRate -= r;
                        maxRate = totalRate;
                    }
                    else
                    {
                        long remRate = totalRate - r;
                        Int128 keepFlux = flux - totalFlux;
                        maxRate = remRate + (double)keepFlux / diff;
                        totalFlux = 0;
                        break;
                    }
                }
            }
            else if (totalFlux < 0)
            {
                Int128 deficit = -totalFlux;
                for (int i = 0; i < n && deficit > 0; i++)
                {
                    (long r, long diff) = sources[i];
                    long negDiff = -diff;
                    Int128 flux = (Int128)r * negDiff;
                    if (deficit >= flux)
                    {
                        deficit -= flux;
                        totalRate -= r;
                        maxRate = totalRate;
                    }
                    else
                    {
                        long remRate = totalRate - r;
                        Int128 keepFlux = flux - deficit;
                        maxRate = remRate + (double)keepFlux / negDiff;
                        deficit = 0;
                        break;
                    }
                }
            }

            double ans = v / maxRate;
            return ans.ToString("G17", CultureInfo.InvariantCulture);
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
            Test(Q2Solver, 1e-6);
        }

        #endregion

        #region Question 3 - Bilingual

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            string[][] sentences = new string[n][];
            for (int i = 0; i < n; i++)
                test.Get(out sentences[i]);
            return () => Q3Solver(n, sentences);
        }

        private static long Q3Solver(int n, string[][] sentences)
        {
            Dictionary<string, int> wordIds = new(StringComparer.Ordinal);
            List<int>[] sentenceWordIds = new List<int>[n];
            for (int i = 0; i < n; i++)
            {
                HashSet<int> seen = [];
                List<int> list = [];
                foreach (string w in sentences[i])
                {
                    if (!wordIds.TryGetValue(w, out int id))
                    {
                        id = wordIds.Count;
                        wordIds.Add(w, id);
                    }
                    if (seen.Add(id))
                        list.Add(id);
                }
                sentenceWordIds[i] = list;
            }

            int wordCount = wordIds.Count;
            if (wordCount == 0)
                return 0;

            int inf = wordCount + 5;
            MaxFlow flow = new(n + 2 * wordCount);
            for (int w = 0; w < wordCount; w++)
            {
                int inNode = n + 2 * w;
                flow.AddDirectedEdge(inNode, inNode + 1, 1);
            }

            for (int s = 0; s < n; s++)
            {
                foreach (int w in sentenceWordIds[s])
                {
                    int inNode = n + 2 * w;
                    int outNode = inNode + 1;
                    flow.AddDirectedEdge(s, inNode, inf);
                    flow.AddDirectedEdge(outNode, s, inf);
                }
            }

            return flow.ComputeMaxFlow(0, 1);
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

        #region Question 4 - Drum Decorator

        private static readonly int[] DrumPeriods = [1, 3, 4, 6, 12];
        private static readonly int[][] DrumLcmIdx = BuildDrumLcmIdx();

        private static int[][] BuildDrumLcmIdx()
        {
            int[][] map = new int[DrumPeriods.Length][];
            for (int i = 0; i < DrumPeriods.Length; i++)
            {
                map[i] = new int[DrumPeriods.Length];
                for (int j = 0; j < DrumPeriods.Length; j++)
                {
                    long lcm = MathFormulas.Lcm(DrumPeriods[i], DrumPeriods[j]);
                    map[i][j] = Array.IndexOf(DrumPeriods, (int)lcm);
                }
            }
            return map;
        }

        private static Func<long> Q4Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            return () => Q4Solver(r, c);
        }

        private static long Q4Solver(int r, int c)
        {
            const long modBasis = 1_000_000_007L;
            Modulo mod = new(modBasis);

            // Periods indices: 0 -> 1, 1 -> 3, 2 -> 4, 3 -> 6, 4 -> 12
            List<(int Height, int PeriodIdx, long Ways)> bands12 = [(1, 0, 1)];
            if (c % 3 == 0) bands12.Add((2, 1, 3));
            if (c % 4 == 0) bands12.Add((3, 2, 4));
            if (c % 6 == 0) bands12.Add((2, 3, 6));

            // dp[row, lastWas3, lcmIdx]
            long[,,] dp = new long[r + 1, 2, DrumPeriods.Length];

            // Initial placement at row 0 can be a 3-band or any valid 1/2-band.
            if (r >= 2)
                dp[2, 1, 0] = 1;
            foreach (var (h, pIdx, ways) in bands12)
            {
                if (h <= r)
                    dp[h, 0, pIdx] = mod.Add(dp[h, 0, pIdx], ways);
            }

            for (int row = 1; row < r; row++)
            {
                // Last was 1/2-band -> next must be 3-band (h=2, pIdx=0, ways=1).
                if (row + 2 <= r)
                {
                    for (int lcmIdx = 0; lcmIdx < DrumPeriods.Length; lcmIdx++)
                    {
                        long cur = dp[row, 0, lcmIdx];
                        if (cur != 0)
                            dp[row + 2, 1, lcmIdx] = mod.Add(dp[row + 2, 1, lcmIdx], cur);
                    }
                }

                // Last was 3-band -> next must be a valid 1/2-band.
                for (int lcmIdx = 0; lcmIdx < DrumPeriods.Length; lcmIdx++)
                {
                    long cur = dp[row, 1, lcmIdx];
                    if (cur == 0)
                        continue;
                    foreach (var (h, pIdx, ways) in bands12)
                    {
                        if (row + h <= r)
                        {
                            int newLcmIdx = DrumLcmIdx[lcmIdx][pIdx];
                            dp[row + h, 0, newLcmIdx] = mod.Add(dp[row + h, 0, newLcmIdx], mod.Multiply(cur, ways));
                        }
                    }
                }
            }

            long ans = 0;
            for (int last = 0; last < 2; last++)
            {
                for (int lcmIdx = 0; lcmIdx < DrumPeriods.Length; lcmIdx++)
                {
                    long count = dp[r, last, lcmIdx];
                    if (count != 0)
                        ans = mod.Add(ans, mod.Multiply(count, mod.Invert(DrumPeriods[lcmIdx])));
                }
            }
            return ans;
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
