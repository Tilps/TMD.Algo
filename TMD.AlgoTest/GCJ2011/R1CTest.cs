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
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Square Tiles

        private static Func<string[]> Q1Solver(GCJTestCase test)
        {
            test.Get(out int r, out int _);
            test.GetLines(r, out string[] rows);
            return () => Q1Solver(rows);
        }

        private static string[] Q1Solver(string[] rows)
        {
            int r = rows.Length;
            int c = rows[0].Length;
            char[][] grid = new char[r][];
            for (int i = 0; i < r; i++)
                grid[i] = rows[i].ToCharArray();

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (grid[i][j] == '#')
                    {
                        if (i + 1 >= r || j + 1 >= c ||
                            grid[i][j + 1] != '#' ||
                            grid[i + 1][j] != '#' ||
                            grid[i + 1][j + 1] != '#')
                        {
                            return ["Impossible"];
                        }
                        grid[i][j] = '/';
                        grid[i][j + 1] = '\\';
                        grid[i + 1][j] = '\\';
                        grid[i + 1][j + 1] = '/';
                    }
                }
            }

            string[] output = new string[r];
            for (int i = 0; i < r; i++)
                output[i] = new string(grid[i]);
            return output;
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

        #region Question 2 - Space Emergency

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out string[] tokens);
            return () => Q2Solver(tokens);
        }

        private static long Q2Solver(string[] tokens)
        {
            long boosters = long.Parse(tokens[0], CultureInfo.InvariantCulture);
            long t = long.Parse(tokens[1], CultureInfo.InvariantCulture);
            long n = long.Parse(tokens[2], CultureInfo.InvariantCulture);
            int c = int.Parse(tokens[3], CultureInfo.InvariantCulture);

            int[] a = new int[c];
            long periodSum = 0;
            for (int i = 0; i < c; i++)
            {
                a[i] = int.Parse(tokens[4 + i], CultureInfo.InvariantCulture);
                periodSum += a[i];
            }

            long fullTotalPeriods = n / c;
            int remTotal = (int)(n % c);
            long totalDist = fullTotalPeriods * periodSum;
            for (int i = 0; i < remTotal; i++)
                totalDist += a[i];

            long d0 = t / 2;
            if (d0 >= totalDist || boosters == 0)
                return 2 * totalDist;

            long startPeriod = d0 / periodSum;
            long remD = d0 % periodSum;
            int j = 0;
            while (remD >= a[j])
            {
                remD -= a[j];
                j++;
            }

            long firstRemainingIdx = startPeriod * c + j;
            long fullSuffixEdges = (n - 1) - firstRemainingIdx; // edges after straddling edge
            long q = fullSuffixEdges / c;
            int r = (int)(fullSuffixEdges % c);

            List<(long Saving, long Count)> savings = [with(capacity: c + 1)];
            savings.Add((a[j] - remD, 1));
            for (int offset = 1; offset <= c; offset++)
            {
                int residue = (j + offset) % c;
                long count = q + (offset <= r ? 1 : 0);
                if (count > 0)
                    savings.Add((a[residue], count));
            }

            savings.Sort((x, y) => y.Saving.CompareTo(x.Saving));
            long saved = 0;
            long remBoosters = boosters;
            foreach (var (saving, count) in savings)
            {
                long take = Math.Min(remBoosters, count);
                saved += take * saving;
                remBoosters -= take;
                if (remBoosters == 0)
                    break;
            }

            return 2 * totalDist - saved;
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

        #region Question 3 - Perfect Harmony

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int _, out long l, out long h);
            test.Get(out long[] f);
            return () => Q3Solver(l, h, f);
        }

        private static string Q3Solver(long l, long h, long[] fRaw)
        {
            Array.Sort(fRaw);
            List<long> f = [with(capacity: fRaw.Length + 1)];
            if (fRaw[0] != 1)
                f.Add(1);
            foreach (long val in fRaw)
            {
                if (f.Count == 0 || val != f[^1])
                    f.Add(val);
            }

            int m = f.Count;
            long[] prefLcm = new long[m];
            prefLcm[0] = f[0];
            for (int i = 1; i < m; i++)
            {
                if (prefLcm[i - 1] < 0 || !MathFormulas.TryLcm(prefLcm[i - 1], f[i], h, out long nextLcm))
                    prefLcm[i] = -1;
                else
                    prefLcm[i] = nextLcm;
            }

            long[] sufGcd = new long[m];
            sufGcd[m - 1] = f[m - 1];
            for (int i = m - 2; i >= 0; i--)
                sufGcd[i] = MathFormulas.Gcd(sufGcd[i + 1], f[i]);

            for (int k = 0; k < m - 1; k++)
            {
                long c = prefLcm[k];
                if (c < 0)
                    break;
                long d = sufGcd[k + 1];
                if (d % c != 0 || d < l)
                    continue;
                if (c >= l)
                    return c.ToString(CultureInfo.InvariantCulture);

                long q = d / c;
                long lo = MathFormulas.DivCeil(l, c);
                long hi = h / c;
                foreach (long x in MathFormulas.GetDivisors(q))
                {
                    if (x >= lo && x <= hi)
                        return (c * x).ToString(CultureInfo.InvariantCulture);
                }
            }

            long cAll = prefLcm[m - 1];
            if (cAll > 0)
            {
                long cand = MathFormulas.DivCeil(l, cAll) * cAll;
                if (cand <= h)
                    return cand.ToString(CultureInfo.InvariantCulture);
            }

            return "NO";
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
