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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Diamond Inheritance

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            int[][] parents = new int[n][];
            for (int i = 0; i < n; i++)
            {
                test.Get(out int[] line);
                int m = line[0];
                parents[i] = new int[m];
                for (int j = 0; j < m; j++)
                    parents[i][j] = line[j + 1] - 1;
            }
            return () => Q1Solver(n, parents);
        }

        private static string Q1Solver(int n, int[][] parents)
        {
            int[] seen = new int[n];
            int stamp = 0;
            Queue<int> queue = new();

            for (int start = 0; start < n; start++)
            {
                stamp++;
                seen[start] = stamp;
                queue.Clear();
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    int cur = queue.Dequeue();
                    foreach (int nxt in parents[cur])
                    {
                        if (seen[nxt] == stamp)
                            return "Yes";
                        seen[nxt] = stamp;
                        queue.Enqueue(nxt);
                    }
                }
            }

            return "No";
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

        #region Question 2 - Out of Gas

        private static Func<string[]> Q2Solver(GCJTestCase test)
        {
            test.Get(out double d, out int n, out int _);
            test.GetLines(n, out (double T, double X)[] pts);
            test.Get(out double[] accels);
            return () => Q2Solver(d, pts, accels);
        }

        private static string[] Q2Solver(double d, (double T, double X)[] pts, double[] accels)
        {
            List<(double T, double X)> clipped = [with(capacity: pts.Length)];
            if (pts[0].X < d)
            {
                for (int i = 0; i < pts.Length; i++)
                {
                    if (pts[i].X >= d)
                    {
                        var (tPrev, xPrev) = pts[i - 1];
                        var (tCur, xCur) = pts[i];
                        double tD = tPrev + (tCur - tPrev) * (d - xPrev) / (xCur - xPrev);
                        clipped.Add((tD, d));
                        break;
                    }
                    clipped.Add(pts[i]);
                }
            }

            string[] output = new string[accels.Length];
            for (int i = 0; i < accels.Length; i++)
            {
                double a = accels[i];
                double wait = 0.0;
                foreach (var (t, x) in clipped)
                {
                    double candWait = t - Math.Sqrt(2.0 * x / a);
                    if (candWait > wait)
                        wait = candWait;
                }
                double total = wait + Math.Sqrt(2.0 * d / a);
                output[i] = total.ToString("F7", CultureInfo.InvariantCulture);
            }

            return output;
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

        #region Question 3 - Box Factory

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.Get(out long[] boxRaw);
            test.Get(out long[] toyRaw);
            return () => Q3Solver(n, m, boxRaw, toyRaw);
        }

        private static long Q3Solver(int n, int m, long[] boxRaw, long[] toyRaw)
        {
            long[] aCnt = new long[n + 1];
            int[] aType = new int[n + 1];
            for (int i = 1; i <= n; i++)
            {
                aCnt[i] = boxRaw[2 * (i - 1)];
                aType[i] = (int)boxRaw[2 * (i - 1) + 1];
            }

            long[] bCnt = new long[m + 1];
            int[] bType = new int[m + 1];
            for (int j = 1; j <= m; j++)
            {
                bCnt[j] = toyRaw[2 * (j - 1)];
                bType[j] = (int)toyRaw[2 * (j - 1) + 1];
            }

            long[,] dp = new long[n + 1, m + 1];
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    long best = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    if (aType[i] == bType[j])
                    {
                        int type = aType[i];
                        int ia = i;
                        int jb = j;
                        long sumA = aCnt[ia];
                        long sumB = bCnt[jb];

                        while (true)
                        {
                            long cand = dp[ia - 1, jb - 1] + Math.Min(sumA, sumB);
                            if (cand > best)
                                best = cand;

                            if (sumA <= sumB)
                            {
                                ia--;
                                while (ia >= 1 && aType[ia] != type)
                                    ia--;
                                if (ia < 1)
                                    break;
                                sumA += aCnt[ia];
                            }
                            else
                            {
                                jb--;
                                while (jb >= 1 && bType[jb] != type)
                                    jb--;
                                if (jb < 1)
                                    break;
                                sumB += bCnt[jb];
                            }
                        }
                    }
                    dp[i, j] = best;
                }
            }

            return dp[n, m];
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
