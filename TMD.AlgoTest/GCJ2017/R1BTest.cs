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
using System.Text;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2017
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - Steed 2: Cruise Control

        private static Func<double> Q1Solver(GCJTestCase test)
        {
            test.Get(out double d, out int n);
            test.GetLines(n, out double[] k, out double[] s);
            return () => Q1Solver(d, n, k, s);
        }

        private static double Q1Solver(double d, int n, double[] k, double[] s)
        {
            double maxTime = 0.0;
            for (int i = 0; i < n; i++)
            {
                double t = (d - k[i]) / s[i];
                if (t > maxTime)
                    maxTime = t;
            }
            return d / maxTime;
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

        #region Question 2 - Stable Neigh-bors

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int r, out int o, out int y, out int g, out int b, out int v);
            return () => Q2Solver(n, r, o, y, g, b, v);
        }

        private static string Q2Solver(int n, int r, int o, int y, int g, int b, int v)
        {
            static string? CheckTwoColorOnly(int total, int primary, int secondary, string unit)
            {
                if (secondary == 0 || primary != secondary)
                    return null;
                if (primary + secondary != total)
                    return "IMPOSSIBLE";
                StringBuilder sb = new(total);
                for (int i = 0; i < secondary; i++)
                    sb.Append(unit);
                return sb.ToString();
            }

            if ((o > 0 && b < o) || (g > 0 && r < g) || (v > 0 && y < v))
                return "IMPOSSIBLE";

            string? twoO = CheckTwoColorOnly(n, b, o, "BO");
            if (twoO != null) return twoO;
            string? twoG = CheckTwoColorOnly(n, r, g, "RG");
            if (twoG != null) return twoG;
            string? twoV = CheckTwoColorOnly(n, y, v, "YV");
            if (twoV != null) return twoV;

            int pr = r - g;
            int py = y - v;
            int pb = b - o;
            int rem = pr + py + pb;

            if (2 * Math.Max(pr, Math.Max(py, pb)) > rem)
                return "IMPOSSIBLE";

            (int Count, char Color)[] groups = [(pr, 'R'), (py, 'Y'), (pb, 'B')];
            Array.Sort(groups, static (a, b) => b.Count.CompareTo(a.Count));

            char[] ring = new char[rem];
            int idx = 0;
            foreach (var (count, color) in groups)
            {
                for (int i = 0; i < count; i++)
                {
                    ring[idx] = color;
                    idx += 2;
                    if (idx >= rem)
                        idx = 1;
                }
            }

            StringBuilder result = new(n);
            bool splicedG = false;
            bool splicedV = false;
            bool splicedO = false;

            for (int i = 0; i < rem; i++)
            {
                char c = ring[i];
                result.Append(c);
                if (c == 'R' && !splicedG && g > 0)
                {
                    splicedG = true;
                    for (int k = 0; k < g; k++)
                        result.Append("GR");
                }
                else if (c == 'Y' && !splicedV && v > 0)
                {
                    splicedV = true;
                    for (int k = 0; k < v; k++)
                        result.Append("VY");
                }
                else if (c == 'B' && !splicedO && o > 0)
                {
                    splicedO = true;
                    for (int k = 0; k < o; k++)
                        result.Append("OB");
                }
            }

            return result.ToString();
        }

        private static bool IsValidNeighborPair(char a, char b) => a switch
        {
            'R' => b == 'Y' || b == 'G' || b == 'B',
            'O' => b == 'B',
            'Y' => b == 'R' || b == 'B' || b == 'V',
            'G' => b == 'R',
            'B' => b == 'R' || b == 'O' || b == 'Y',
            'V' => b == 'Y',
            _ => false
        };

        private static void ValidateStableNeighbors(string input, string expectedOutput, List<string> result)
        {
            char[] sep = ['\r', '\n'];
            string[] inLines = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] expLines = expectedOutput.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0], CultureInfo.InvariantCulture);
            Assert.AreEqual(t, result.Count);

            for (int i = 0; i < t; i++)
            {
                string[] nums = inLines[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int n = int.Parse(nums[0], CultureInfo.InvariantCulture);
                int r = int.Parse(nums[1], CultureInfo.InvariantCulture);
                int o = int.Parse(nums[2], CultureInfo.InvariantCulture);
                int y = int.Parse(nums[3], CultureInfo.InvariantCulture);
                int g = int.Parse(nums[4], CultureInfo.InvariantCulture);
                int b = int.Parse(nums[5], CultureInfo.InvariantCulture);
                int v = int.Parse(nums[6], CultureInfo.InvariantCulture);

                string prefix = $"Case #{i + 1}: ";
                string expVal = expLines[i].Substring(prefix.Length).Trim();
                Assert.IsTrue(result[i].StartsWith(prefix, StringComparison.Ordinal));
                string actVal = result[i].Substring(prefix.Length).Trim();

                if (expVal == "IMPOSSIBLE")
                {
                    Assert.AreEqual("IMPOSSIBLE", actVal, $"Case #{i + 1} expected IMPOSSIBLE");
                    continue;
                }

                Assert.AreNotEqual("IMPOSSIBLE", actVal, $"Case #{i + 1} expected valid arrangement");
                Assert.AreEqual(n, actVal.Length, $"Case #{i + 1} wrong length");

                int cr = 0, co = 0, cy = 0, cg = 0, cb = 0, cv = 0;
                foreach (char ch in actVal)
                {
                    switch (ch)
                    {
                        case 'R': cr++; break;
                        case 'O': co++; break;
                        case 'Y': cy++; break;
                        case 'G': cg++; break;
                        case 'B': cb++; break;
                        case 'V': cv++; break;
                        default: Assert.Fail($"Case #{i + 1} invalid char '{ch}'"); break;
                    }
                }
                Assert.AreEqual((r, o, y, g, b, v), (cr, co, cy, cg, cb, cv), $"Case #{i + 1} color counts mismatch");

                for (int j = 0; j < n; j++)
                {
                    char cur = actVal[j];
                    char nxt = actVal[(j + 1) % n];
                    Assert.IsTrue(IsValidNeighborPair(cur, nxt), $"Case #{i + 1} invalid neighbor {cur}{nxt} at {j}");
                }
            }
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, ValidateStableNeighbors);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, ValidateStableNeighbors);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, ValidateStableNeighbors);
        }

        #endregion

        #region Question 3 - Pony Express

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int q);
            test.GetLines(n, out long[] endurance, out long[] speed);
            test.GetMatrix(n, out long[][] distMatrix);
            test.GetLines(q, out int[] u, out int[] v);
            return () => Q3Solver(n, q, endurance, speed, distMatrix, u, v);
        }

        private static string Q3Solver(int n, int q, long[] endurance, long[] speed, long[][] rawDist, int[] qs, int[] qt)
        {
            const long infDist = 1_000_000_000_000_000L;
            long[,] dist = new long[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    dist[i, j] = i == j ? 0 : (rawDist[i][j] < 0 ? infDist : rawDist[i][j]);
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    long dik = dist[i, k];
                    if (dik >= infDist) continue;
                    for (int j = 0; j < n; j++)
                    {
                        long cand = dik + dist[k, j];
                        if (cand < dist[i, j])
                            dist[i, j] = cand;
                    }
                }
            }

            double[,] time = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                        time[i, j] = 0.0;
                    else if (dist[i, j] <= endurance[i])
                        time[i, j] = (double)dist[i, j] / speed[i];
                    else
                        time[i, j] = double.PositiveInfinity;
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    double tik = time[i, k];
                    if (double.IsInfinity(tik)) continue;
                    for (int j = 0; j < n; j++)
                    {
                        double cand = tik + time[k, j];
                        if (cand < time[i, j])
                            time[i, j] = cand;
                    }
                }
            }

            string[] answers = new string[q];
            for (int i = 0; i < q; i++)
                answers[i] = time[qs[i] - 1, qt[i] - 1].ToString("G17", CultureInfo.InvariantCulture);
            return string.Join(" ", answers);
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
    }
}
