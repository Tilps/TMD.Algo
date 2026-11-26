using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class R1ATest : BaseGCJTest
{
    #region Q1 Waffle Choppers

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int c, out int h, out int v);
        string[] grid = new string[r];
        for (int i = 0; i < r; i++)
            testCase.Get(out grid[i]);

        return () =>
        {
            int total = 0;
            int[] rowCounts = new int[r];
            int[] colCounts = new int[c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (grid[i][j] == '@')
                    {
                        total++;
                        rowCounts[i]++;
                        colCounts[j]++;
                    }
                }
            }

            if (total == 0)
                return "POSSIBLE";

            int slices = (h + 1) * (v + 1);
            if (total % slices != 0)
                return "IMPOSSIBLE";

            int perCell = total / slices;
            int rowTarget = perCell * (v + 1);
            int colTarget = perCell * (h + 1);

            if (!TrySlice(rowCounts, h, rowTarget, out int[] rowStarts, out int[] rowEnds))
                return "IMPOSSIBLE";
            if (!TrySlice(colCounts, v, colTarget, out int[] colStarts, out int[] colEnds))
                return "IMPOSSIBLE";

            for (int ri = 0; ri <= h; ri++)
            {
                for (int ci = 0; ci <= v; ci++)
                {
                    int cnt = 0;
                    for (int i = rowStarts[ri]; i <= rowEnds[ri]; i++)
                        for (int j = colStarts[ci]; j <= colEnds[ci]; j++)
                            if (grid[i][j] == '@')
                                cnt++;
                    if (cnt != perCell)
                        return "IMPOSSIBLE";
                }
            }

            return "POSSIBLE";
        };
    }

    private static bool TrySlice(int[] counts, int cuts, int target, out int[] starts, out int[] ends)
    {
        starts = new int[cuts + 1];
        ends = new int[cuts + 1];
        int sliceIdx = 0;
        int acc = 0;
        starts[0] = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            acc += counts[i];
            if (acc > target)
                return false;
            if (acc == target && sliceIdx < cuts)
            {
                ends[sliceIdx] = i;
                sliceIdx++;
                starts[sliceIdx] = i + 1;
                acc = 0;
            }
        }

        if (sliceIdx != cuts || acc != target)
            return false;
        ends[cuts] = counts.Length - 1;
        return true;
    }

    #endregion

    #region Q2 Bit Party

    [Test]
    public void Q2Sample() => Test(Q2);

    [Test]
    public void Q2Small() => Test(Q2);

    [Test]
    public void Q2Large() => Test(Q2);

    private static Func<long> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int r, out long b, out int c);
        long[] m = new long[c];
        long[] s = new long[c];
        long[] p = new long[c];
        for (int i = 0; i < c; i++)
            testCase.Get(out m[i], out s[i], out p[i]);

        return () =>
        {
            long lo = 1;
            long hi = 2_000_000_000_000_000_000L;
            long[] cap = new long[c];
            while (lo < hi)
            {
                long mid = lo + ((hi - lo) >> 1);
                for (int i = 0; i < c; i++)
                    cap[i] = mid < p[i] ? 0 : Math.Min(m[i], (mid - p[i]) / s[i]);

                Array.Sort(cap);
                long sum = 0;
                for (int i = 0; i < r; i++)
                {
                    sum += cap[c - 1 - i];
                    if (sum >= b)
                        break;
                }

                if (sum >= b)
                    hi = mid;
                else
                    lo = mid + 1;
            }

            return lo;
        };
    }

    #endregion

    #region Q3 Edgy Baking

    [Test]
    public void Q3Sample() => Test(Q3, 1e-6);

    [Test]
    public void Q3Small() => Test(Q3, 1e-6);

    [Test]
    public void Q3Large() => Test(Q3, 1e-6);

    private static Func<double> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n, out long p);
        int[] w = new int[n];
        int[] h = new int[n];
        for (int i = 0; i < n; i++)
            testCase.Get(out w[i], out h[i]);

        return () =>
        {
            double baseP = 0.0;
            for (int i = 0; i < n; i++)
                baseP += 2.0 * (w[i] + h[i]);

            double target = p - baseP;
            List<(double L, double R)> intervals = [(0.0, 0.0)];

            for (int i = 0; i < n; i++)
            {
                double addMin = 2.0 * Math.Min(w[i], h[i]);
                double addMax = 2.0 * Math.Sqrt((double)w[i] * w[i] + (double)h[i] * h[i]);

                int existingCount = intervals.Count;
                for (int j = 0; j < existingCount; j++)
                {
                    double nl = intervals[j].L + addMin;
                    if (nl <= target + 1e-9)
                    {
                        double nr = intervals[j].R + addMax;
                        intervals.Add((nl, nr));
                    }
                }

                intervals.Sort((a, b) => a.L.CompareTo(b.L));
                int write = 0;
                for (int j = 0; j < intervals.Count; j++)
                {
                    var cur = intervals[j];
                    if (write == 0 || cur.L > intervals[write - 1].R + 1e-12)
                        intervals[write++] = cur;
                    else if (cur.R > intervals[write - 1].R)
                        intervals[write - 1] = (intervals[write - 1].L, cur.R);
                }
                if (write < intervals.Count)
                    intervals.RemoveRange(write, intervals.Count - write);
            }

            return baseP + Math.Min(target, intervals[^1].R);
        };
    }

    #endregion
}
