using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class R1BTest : BaseGCJTest
{
    #region Q1 Manhattan Crepe Cart

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int p, out int q);
        testCase.GetLines(p, out int[] xs, out int[] ys, out string[] dirs);
        return () =>
        {
            int[] xDiff = new int[q + 2];
            int[] yDiff = new int[q + 2];
            for (int i = 0; i < p; i++)
            {
                int x = xs[i];
                int y = ys[i];
                switch (dirs[i][0])
                {
                    case 'N':
                        yDiff[y + 1]++;
                        break;
                    case 'S':
                        yDiff[0]++;
                        yDiff[y]--;
                        break;
                    case 'E':
                        xDiff[x + 1]++;
                        break;
                    case 'W':
                        xDiff[0]++;
                        xDiff[x]--;
                        break;
                }
            }

            int bestX = BestCoord(xDiff, q);
            int bestY = BestCoord(yDiff, q);
            return $"{bestX} {bestY}";
        };

        static int BestCoord(int[] diff, int maxCoord)
        {
            int best = 0;
            int bestVotes = -1;
            int cur = 0;
            for (int i = 0; i <= maxCoord; i++)
            {
                cur += diff[i];
                if (cur > bestVotes)
                {
                    bestVotes = cur;
                    best = i;
                }
            }
            return best;
        }
    }

    #endregion

    #region Q2 Draupnir

    [Test]
    public void Q2Small() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 0));

    [Test]
    public void Q2Large() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 1));

    private static void Q2Solve(GCJInteractiveChannel io)
    {
        string[] header = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(header[0], CultureInfo.InvariantCulture);

        const ulong mask63 = (1UL << 63) - 1;
        for (int tc = 0; tc < t; tc++)
        {
            io.WriteLine("200");
            ulong day200 = ulong.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
            ulong r4 = (day200 >> 50) & 127;
            ulong r5 = (day200 >> 40) & 127;
            ulong r6 = (day200 >> 33) & 127;

            io.WriteLine("56");
            ulong day56 = ulong.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
            ulong rem = (day56 - (r4 << 14) - (r5 << 11) - (r6 << 9)) & mask63;
            ulong r1 = (rem >> 56) & 127;
            ulong r2 = (rem >> 28) & 127;
            ulong r3 = (rem >> 18) & 127;

            io.WriteLine($"{r1} {r2} {r3} {r4} {r5} {r6}");
            string verdict = io.ReadRequiredLine();
            Assert.AreEqual("1", verdict, $"Case #{tc + 1}: wrong guess.");
        }
    }

    private static void Q2Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int maxDay = 500;
        const ulong mod = 1UL << 63;
        int w = testNumber == 0 ? 6 : 2;
        Random rng = new(926153 + testNumber);

        List<int[]> cases = [[1, 1, 1, 1, 1, 1], [100, 100, 100, 100, 100, 100]];
        for (int i = 0; i < 6; i++)
        {
            int delta = 1 + ((i & 1) ^ testNumber);
            int[] lo = new int[6];
            lo[i] = delta;
            cases.Add(lo);
            int[] hi = [100, 100, 100, 100, 100, 100];
            hi[i] -= delta;
            cases.Add(hi);
        }

        // All 20 distinct permutations of (0,0,0,1,1,1)
        for (int mask = 0; mask < 64; mask++)
        {
            if (int.PopCount(mask) != 3)
                continue;
            int[] c = new int[6];
            for (int i = 0; i < 6; i++)
                c[i] = rng.Next(0, 5) + (((mask >> i) & 1) == 1 ? 96 : 0);
            cases.Add(c);
        }

        for (int k = 0; k < 5; k++)
        {
            int[] c = new int[6];
            for (int i = 0; i < 6; i++) c[i] = rng.Next(1, 5);
            cases.Add(c);
        }
        for (int k = 0; k < 5; k++)
        {
            int[] c = new int[6];
            for (int i = 0; i < 6; i++) c[i] = rng.Next(96, 101);
            cases.Add(c);
        }
        while (cases.Count < 50)
        {
            int[] c = new int[6];
            for (int i = 0; i < 6; i++) c[i] = rng.Next(20, 81);
            cases.Add(c);
        }

        int[][] order = [.. cases];
        rng.Shuffle(order);

        io.WriteLine($"{order.Length} {w}");
        for (int tc = 0; tc < order.Length; tc++)
        {
            int[] ringCase = order[tc];
            bool solved = false;
            for (int ex = 0; ex <= w; ex++)
            {
                string line = io.ReadRequiredLine();
                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.IsTrue(tokens.Length is 1 or 6, $"Case #{tc + 1}: wrong token count.");

                if (tokens.Length == 1)
                {
                    Assert.Less(ex, w, $"Case #{tc + 1}: exceeded {w} queries.");
                    int day = int.Parse(tokens[0], CultureInfo.InvariantCulture);
                    Assert.That(day, Is.InRange(1, maxDay));
                    ulong total = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        int shift = day / (i + 1);
                        if (shift < 63)
                            total = (total + ((ulong)ringCase[i] << shift)) & (mod - 1);
                    }
                    io.WriteLine(total.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    int[] guess = new int[6];
                    for (int i = 0; i < 6; i++)
                    {
                        guess[i] = int.Parse(tokens[i], CultureInfo.InvariantCulture);
                        Assert.GreaterOrEqual(guess[i], 0);
                    }
                    CollectionAssert.AreEqual(ringCase, guess, $"Case #{tc + 1}: wrong guess.");
                    io.WriteLine("1");
                    solved = true;
                    break;
                }
            }
            Assert.IsTrue(solved);
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    #endregion

    #region Q3 Fair Fight

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n, out int k);
        testCase.Get(out int[] c);
        testCase.Get(out int[] d);
        return () =>
        {
            int[] leftBound = new int[n];
            int[] rightBound = new int[n];
            Stack<int> stack = new();

            for (int i = 0; i < n; i++)
            {
                while (stack.Count > 0 && c[stack.Peek()] < c[i])
                    stack.Pop();
                leftBound[i] = stack.Count == 0 ? 0 : stack.Peek() + 1;
                stack.Push(i);
            }

            stack.Clear();
            for (int i = n - 1; i >= 0; i--)
            {
                while (stack.Count > 0 && c[stack.Peek()] <= c[i])
                    stack.Pop();
                rightBound[i] = stack.Count == 0 ? n - 1 : stack.Peek() - 1;
                stack.Push(i);
            }

            SparseTable<int, RangeMaxInt32Policy> stD = new(d.AsSpan());
            long total = 0;
            for (int i = 0; i < n; i++)
            {
                long upper = CountWithMaxAtMost(i, leftBound[i], rightBound[i], c[i] + k, d, stD);
                long lower = CountWithMaxAtMost(i, leftBound[i], rightBound[i], c[i] - k - 1, d, stD);
                total += upper - lower;
            }

            return total.ToString(CultureInfo.InvariantCulture);
        };

        static long CountWithMaxAtMost(
            int idx,
            int minL,
            int maxR,
            int limit,
            int[] d,
            SparseTable<int, RangeMaxInt32Policy> stD)
        {
            if (d[idx] > limit)
                return 0;

            int lo = minL;
            int hi = idx;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (stD.Query(mid, idx + 1) <= limit)
                    hi = mid;
                else
                    lo = mid + 1;
            }
            int leftmost = lo;

            lo = idx;
            hi = maxR;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo + 1) >> 1);
                if (stD.Query(idx, mid + 1) <= limit)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            int rightmost = lo;

            return (long)(idx - leftmost + 1) * (rightmost - idx + 1);
        }
    }

    #endregion
}
