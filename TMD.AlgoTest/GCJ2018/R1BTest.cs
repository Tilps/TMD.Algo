using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class R1BTest : BaseGCJTest
{
    #region Q1 Rounding Error

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small1() => Test(Q1);

    [Test]
    public void Q1Small2() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<long> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int n, out int l);
        testCase.Get(out int[] c);

        return () =>
        {
            long sumC = 0;
            for (int i = 0; i < l; i++)
                sumC += c[i];
            int rem = (int)(n - sumC);

            bool RoundsUp(int x) => (100L * x) % n * 2 >= n;
            long RoundPct(int x) => (200L * x + n) / (2L * n);

            int[] need = new int[n + 1];
            need[n] = int.MaxValue;
            for (int x = n - 1; x >= 0; x--)
            {
                if (RoundsUp(x))
                    need[x] = 0;
                else
                    need[x] = need[x + 1] == int.MaxValue ? int.MaxValue : need[x + 1] + 1;
            }

            List<int> candidates = [];
            for (int i = 0; i < l; i++)
            {
                if (need[c[i]] > 0 && need[c[i]] < need[0])
                    candidates.Add(i);
            }

            candidates.Sort((i, j) => need[c[i]].CompareTo(need[c[j]]));
            int[] votes = (int[])c.Clone();
            foreach (int idx in candidates)
            {
                int cost = need[votes[idx]];
                if (rem >= cost)
                {
                    votes[idx] += cost;
                    rem -= cost;
                }
            }

            long totalPct = 0;
            for (int i = 0; i < l; i++)
                totalPct += RoundPct(votes[i]);

            int costNew = need[0];
            while (rem >= costNew)
            {
                totalPct += RoundPct(costNew);
                rem -= costNew;
            }

            if (rem > 0)
                totalPct += RoundPct(rem);

            return totalPct;
        };
    }

    #endregion

    #region Q2 Mysterious Road Signs

    [Test]
    public void Q2Sample() => Test(Q2);

    [Test]
    public void Q2Small() => Test(Q2);

    [Test]
    public void Q2Large() => Test(Q2);

    private static Func<string> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int s);
        long[] m = new long[s];
        long[] n = new long[s];
        for (int i = 0; i < s; i++)
        {
            testCase.Get(out long d, out long a, out long b);
            m[i] = d + a;
            n[i] = d - b;
        }

        return () =>
        {
            const long unset = long.MinValue;
            (long M, long N, int Start, int XStart) mc = (m[0], unset, 0, 0);
            (long M, long N, int Start, int XStart) nc = (unset, n[0], 0, 0);

            int maxLen = 1;
            int count = 1;

            for (int i = 1; i < s; i++)
            {
                (long M, long N, int Start, int XStart) newMc =
                    m[i] == mc.M ? (m[i], mc.N, mc.Start, mc.XStart) :
                    (nc.M == unset || m[i] == nc.M) ? (m[i], nc.N, nc.Start, i) :
                    (m[i], nc.N, nc.XStart, i);

                (long M, long N, int Start, int XStart) newNc =
                    n[i] == nc.N ? (nc.M, n[i], nc.Start, nc.XStart) :
                    (mc.N == unset || n[i] == mc.N) ? (mc.M, n[i], mc.Start, i) :
                    (mc.M, n[i], mc.XStart, i);

                mc = newMc;
                nc = newNc;

                int len = i - Math.Min(mc.Start, nc.Start) + 1;
                if (len > maxLen)
                {
                    maxLen = len;
                    count = 1;
                }
                else if (len == maxLen)
                {
                    count++;
                }
            }

            return $"{maxLen} {count}";
        };
    }

    #endregion

    #region Q3 Transmutation

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small1() => Test(Q3);

    [Test]
    public void Q3Small2() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<long> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int m);
        int[] r1 = new int[m];
        int[] r2 = new int[m];
        for (int i = 0; i < m; i++)
        {
            testCase.Get(out int a, out int b);
            r1[i] = a - 1;
            r2[i] = b - 1;
        }
        testCase.Get(out long[] g0);

        return () =>
        {
            long totalG = 0;
            for (int i = 0; i < m; i++)
                totalG += g0[i];

            long lo = g0[0];
            long hi = totalG;
            long cap = totalG * 2L + 2L;

            bool CanMake(long target)
            {
                Int128[] g = new Int128[m];
                for (int i = 0; i < m; i++)
                    g[i] = g0[i];
                g[0] -= target;

                long[,] rec = new long[m, m];
                for (int i = 0; i < m; i++)
                {
                    rec[i, r1[i]]++;
                    rec[i, r2[i]]++;
                }

                while (true)
                {
                    int neg = -1;
                    for (int i = 0; i < m; i++)
                    {
                        if (g[i] < 0)
                        {
                            neg = i;
                            break;
                        }
                    }

                    if (neg < 0)
                        return true;
                    if (rec[neg, neg] > 0)
                        return false;

                    Int128 need = -g[neg];
                    g[neg] = 0;

                    for (int j = 0; j < m; j++)
                    {
                        long c = rec[neg, j];
                        if (c > 0)
                        {
                            g[j] -= need * c;
                            if (g[j] < -totalG)
                                return false;
                        }
                    }

                    for (int row = 0; row < m; row++)
                    {
                        if (row == neg)
                            continue;
                        long mult = rec[row, neg];
                        if (mult == 0)
                            continue;

                        rec[row, neg] = 0;
                        for (int j = 0; j < m; j++)
                        {
                            long c = rec[neg, j];
                            if (c > 0)
                            {
                                Int128 val = rec[row, j] + (Int128)mult * c;
                                rec[row, j] = val > cap ? cap : (long)val;
                            }
                        }
                    }
                }
            }

            while (lo < hi)
            {
                long mid = lo + ((hi - lo + 1) >> 1);
                if (CanMake(mid))
                    lo = mid;
                else
                    hi = mid - 1;
            }

            return lo;
        };
    }

    #endregion
}
