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

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Fairland

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int d);
            test.Get(out long s0, out long aS, out long cS, out long rS);
            test.Get(out long m0, out long aM, out long cM, out long rM);
            return () => Q1Solver(n, d, s0, aS, cS, rS, m0, aM, cM, rM);
        }

        private static int Q1Solver(int n, int d, long s0, long aS, long cS, long rS, long m0, long aM, long cM, long rM)
        {
            int[] lo = new int[n];
            int[] hi = new int[n];
            int[] diff = new int[d + 2];

            int baseX = (int)(s0 - d);
            lo[0] = baseX;
            hi[0] = (int)s0;
            diff[0]++;
            diff[d + 1]--;

            long curS = s0;
            long curM = m0;
            for (int i = 1; i < n; i++)
            {
                curS = (curS * aS + cS) % rS;
                curM = (curM * aM + cM) % rM;
                int mgr = (int)(curM % i);

                int l = Math.Max(lo[mgr], (int)(curS - d));
                int h = Math.Min(hi[mgr], (int)curS);
                lo[i] = l;
                hi[i] = h;
                if (l <= h)
                {
                    diff[l - baseX]++;
                    diff[h - baseX + 1]--;
                }
            }

            int best = 0;
            int active = 0;
            for (int i = 0; i <= d; i++)
            {
                active += diff[i];
                if (active > best)
                    best = active;
            }
            return best;
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

        #region Question 2 - Smoothing Window

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.Get(out long[] sums);
            return () => Q2Solver(n, k, sums);
        }

        private static long Q2Solver(int n, int k, long[] sums)
        {
            long[] cur = new long[k];
            long[] lo = new long[k];
            long[] hi = new long[k];

            cur[k - 1] = sums[0];
            lo[k - 1] = sums[0];
            hi[k - 1] = sums[0];

            int steps = n - k;
            for (int i = 0; i < steps; i++)
            {
                int r = i % k;
                cur[r] += sums[i + 1] - sums[i];
                if (cur[r] < lo[r]) lo[r] = cur[r];
                if (cur[r] > hi[r]) hi[r] = cur[r];
            }

            long maxSpan = 0;
            long sumLo = 0;
            for (int r = 0; r < k; r++)
            {
                long span = hi[r] - lo[r];
                if (span > maxSpan) maxSpan = span;
                sumLo += lo[r];
            }

            long slack = 0;
            for (int r = 0; r < k; r++)
                slack += maxSpan - (hi[r] - lo[r]);

            long rem = ((sumLo % k) + k) % k;
            return rem <= slack ? maxSpan : maxSpan + 1;
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

        #region Question 3 - Runaway Quail

        private static Func<double> Q3Solver(GCJTestCase test)
        {
            test.Get(out int y, out int _);
            test.Get(out int[] p);
            test.Get(out int[] s);
            return () => Q3Solver(y, p, s);
        }

        private static (double[] A, double[] B) PrepareSide(int y, int[] p, int[] s, bool positiveSide)
        {
            List<(int Dist, int Speed)> list = [];
            for (int i = 0; i < p.Length; i++)
            {
                if (positiveSide ? p[i] > 0 : p[i] < 0)
                    list.Add((Math.Abs(p[i]), s[i]));
            }

            if (list.Count == 0)
                return ([], []);

            list.Sort(static (u, v) =>
            {
                int cmp = v.Speed.CompareTo(u.Speed);
                return cmp != 0 ? cmp : v.Dist.CompareTo(u.Dist);
            });

            List<(int Dist, int Speed)> filtered = [list[0]];
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].Speed != list[i - 1].Speed)
                    filtered.Add(list[i]);
            }

            double[] a = new double[filtered.Count];
            double[] b = new double[filtered.Count];
            for (int i = 0; i < filtered.Count; i++)
            {
                double denom = y - filtered[i].Speed;
                a[i] = filtered[i].Dist / denom;
                b[i] = filtered[i].Speed / denom;
            }
            return (a, b);
        }

        private static double Q3Solver(int y, int[] p, int[] s)
        {
            var (aR, bR) = PrepareSide(y, p, s, positiveSide: true);
            var (aL, bL) = PrepareSide(y, p, s, positiveSide: false);
            int mR = aR.Length;
            int mL = aL.Length;

            double[,] dp = new double[mR + 1, mL + 1];
            for (int i = 0; i <= mR; i++)
                for (int j = 0; j <= mL; j++)
                    dp[i, j] = double.PositiveInfinity;
            dp[0, 0] = 0.0;

            double best = double.PositiveInfinity;
            for (int i = 0; i <= mR; i++)
            {
                for (int j = 0; j <= mL; j++)
                {
                    double t = dp[i, j];
                    if (double.IsPositiveInfinity(t))
                        continue;

                    if (i < mR)
                    {
                        double dt = 0.0;
                        for (int k = i; k < mR; k++)
                        {
                            double c = aR[k] + bR[k] * t;
                            if (c > dt)
                            {
                                dt = c;
                                int newI = k + 1;
                                while (newI < mR && aR[newI] + bR[newI] * t <= dt)
                                    newI++;

                                if (newI == mR && j == mL)
                                {
                                    double fin = t + dt;
                                    if (fin < best) best = fin;
                                }
                                else
                                {
                                    double nt = t + 2.0 * dt;
                                    if (nt < dp[newI, j]) dp[newI, j] = nt;
                                }
                            }
                        }
                    }

                    if (j < mL)
                    {
                        double dt = 0.0;
                        for (int k = j; k < mL; k++)
                        {
                            double c = aL[k] + bL[k] * t;
                            if (c > dt)
                            {
                                dt = c;
                                int newJ = k + 1;
                                while (newJ < mL && aL[newJ] + bL[newJ] * t <= dt)
                                    newJ++;

                                if (i == mR && newJ == mL)
                                {
                                    double fin = t + dt;
                                    if (fin < best) best = fin;
                                }
                                else
                                {
                                    double nt = t + 2.0 * dt;
                                    if (nt < dp[i, newJ]) dp[i, newJ] = nt;
                                }
                            }
                        }
                    }
                }
            }

            return best;
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

        #region Question 4 - Log Set

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int p);
            test.Get(out long[] e);
            test.Get(out long[] f);
            return () => Q4Solver(p, e, f);
        }

        private static long[] MergeUniqueSortedPlus(long[] sorted, long add)
        {
            int n = sorted.Length;
            long[] res = new long[2 * n];
            int i = 0;
            int j = 0;
            int outLen = 0;

            while (i < n || j < n)
            {
                long val;
                if (j == n || (i < n && sorted[i] < sorted[j] + add))
                {
                    val = sorted[i++];
                }
                else if (i == n || sorted[j] + add < sorted[i])
                {
                    val = sorted[j++] + add;
                }
                else
                {
                    val = sorted[i];
                    i++;
                    j++;
                }

                if (outLen == 0 || res[outLen - 1] != val)
                    res[outLen++] = val;
            }

            if (outLen == res.Length)
                return res;
            Array.Resize(ref res, outLen);
            return res;
        }

        private static string Q4Solver(int p, long[] e, long[] f)
        {
            long[] curE = (long[])e.Clone();
            long[] curF = (long[])f.Clone();
            int curLen = p;

            int zeroCount = 0;
            long lastFreq = curF[curLen - 1];
            while ((lastFreq & 1L) == 0 && lastFreq > 1)
            {
                zeroCount++;
                lastFreq >>= 1;
            }
            if (zeroCount > 0)
            {
                for (int i = 0; i < curLen; i++)
                    curF[i] >>= zeroCount;
            }

            List<long> mags = [];
            while (curLen > 1)
            {
                long d = curE[curLen - 1] - curE[curLen - 2];
                mags.Add(d);

                int nextLen = 0;
                int j = 0;
                for (int i = 0; i < curLen; i++)
                {
                    long freq = curF[i];
                    if (freq == 0)
                        continue;

                    long target = curE[i] + d;
                    while (curE[j] < target)
                        j++;
                    curF[j] -= freq;

                    curE[nextLen] = curE[i];
                    curF[nextLen] = freq;
                    nextLen++;
                }
                curLen = nextLen;
            }

            long negTarget = -curE[0];
            int m = mags.Count;
            mags.Sort(static (a, b) => b.CompareTo(a));

            long[][] can = new long[m + 1][];
            can[m] = [0L];
            for (int k = m - 1; k >= 0; k--)
                can[k] = MergeUniqueSortedPlus(can[k + 1], mags[k]);

            List<long> ans = [with(capacity: m + zeroCount)];
            for (int k = 0; k < m; k++)
            {
                long mag = mags[k];
                if (negTarget >= mag && Array.BinarySearch(can[k + 1], negTarget - mag) >= 0)
                {
                    ans.Add(-mag);
                    negTarget -= mag;
                }
                else
                {
                    ans.Add(mag);
                }
            }
            for (int z = 0; z < zeroCount; z++)
                ans.Add(0L);

            ans.Sort();
            return string.Join(' ', ans);
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

        #region Question 5 - River Flow

        private static Func<string> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n, out int d);
            test.Get(out int[] flows);
            return () => Q5Solver(n, d, flows);
        }

        private static string Q5Solver(int n, int d, int[] flows)
        {
            int m = 2 * d;
            for (int i = m; i < n; i++)
            {
                if (flows[i] != flows[i % m])
                    return "CHEATERS!";
            }

            int[] x = new int[m];
            for (int i = 0; i < m; i++)
                x[i] = flows[i] - flows[(i - 1 + m) % m];

            int totalFarmers = 0;
            int sim0 = 0;
            int[] xSim = new int[m];

            for (int p = 1; p <= d; p <<= 1)
            {
                int q = m / p;
                for (int t = 0; t < p; t++)
                {
                    int f = 0;
                    int sign = 1;
                    for (int idx = t; idx < m; idx += p)
                    {
                        f += sign * x[idx];
                        sign = -sign;
                    }

                    if (f % q != 0)
                        return "CHEATERS!";

                    int sc = f / q;
                    if (sc == 0)
                        continue;

                    int cnt = Math.Abs(sc);
                    totalFarmers += cnt;
                    if ((t == 0 && sc > 0) || (t > 0 && sc < 0))
                        sim0 += cnt;

                    int cur = sc;
                    for (int idx = t; idx < m; idx += p)
                    {
                        xSim[idx] += cur;
                        cur = -cur;
                    }
                }
            }

            int baseFlow = flows[0] - sim0;
            if (baseFlow < 0)
                return "CHEATERS!";

            int curSim = sim0;
            if (xSim[0] != x[0])
                return "CHEATERS!";
            for (int i = 1; i < m; i++)
            {
                if (xSim[i] != x[i])
                    return "CHEATERS!";
                curSim += xSim[i];
                if (curSim + baseFlow != flows[i])
                    return "CHEATERS!";
            }
            if (sim0 - curSim != xSim[0])
                return "CHEATERS!";

            return totalFarmers.ToString(CultureInfo.InvariantCulture);
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Large()
        {
            Test(Q5Solver);
        }

        #endregion
    }
}
