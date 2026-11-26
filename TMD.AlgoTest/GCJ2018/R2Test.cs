using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class R2Test : BaseGCJTest
{
    #region Q1 Falling Balls

    [Test]
    public void Q1Sample() => Test(Q1, ValidateFallingBalls);

    [Test]
    public void Q1Small() => Test(Q1, ValidateFallingBalls);

    [Test]
    public void Q1Large() => Test(Q1, ValidateFallingBalls);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int c);
        testCase.Get(out int[] b);

        return () =>
        {
            if (b[0] == 0 || b[c - 1] == 0)
                return "IMPOSSIBLE";

            int[] target = new int[c];
            int src = 0;
            for (int col = 0; col < c; col++)
            {
                for (int k = 0; k < b[col]; k++)
                    target[src++] = col;
            }

            int maxShift = 0;
            for (int i = 0; i < c; i++)
                maxShift = Math.Max(maxShift, Math.Abs(target[i] - i));

            int rows = maxShift + 1;
            char[,] grid = new char[rows, c];
            for (int r = 0; r < rows; r++)
                for (int col = 0; col < c; col++)
                    grid[r, col] = '.';

            for (int i = 0; i < c; i++)
            {
                if (target[i] > i)
                {
                    int steps = target[i] - i;
                    for (int k = 0; k < steps; k++)
                        grid[k, i + k] = '\\';
                }
                else if (target[i] < i)
                {
                    int steps = i - target[i];
                    for (int k = 0; k < steps; k++)
                        grid[k, i - k] = '/';
                }
            }

            string[] lines = new string[rows + 1];
            lines[0] = rows.ToString();
            char[] rowBuf = new char[c];
            for (int r = 0; r < rows; r++)
            {
                for (int col = 0; col < c; col++)
                    rowBuf[col] = grid[r, col];
                lines[r + 1] = new string(rowBuf);
            }

            return string.Join('\n', lines);
        };
    }

    private static void ValidateFallingBalls(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(inLines[0]);
        Assert.AreEqual(t, actualCases.Count);

        List<List<string>> expectedPerCase = SplitOutputCases(expectedOutput, t);

        int idx = 1;
        for (int tc = 0; tc < t; tc++)
        {
            int c = int.Parse(inLines[idx++].Trim());
            string[] bTokens = inLines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int[] target = new int[c];
            for (int i = 0; i < c; i++)
                target[i] = int.Parse(bTokens[i]);

            int? expectedRows = VerifyFallingBallsOutput(expectedPerCase[tc], c, target);
            int? actualRows = VerifyFallingBallsOutput(StripCaseHeader(actualCases[tc], tc + 1), c, target);
            Assert.AreEqual(expectedRows, actualRows, $"Case #{tc + 1}: row count mismatch.");
        }
    }

    private static List<List<string>> SplitOutputCases(string text, int numCases)
    {
        string[] lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        List<List<string>> cases = new(numCases);
        foreach (string line in lines)
        {
            if (line.StartsWith("Case #", StringComparison.OrdinalIgnoreCase))
            {
                int colon = line.IndexOf(':');
                Assert.Greater(colon, 0);
                string rest = line.Substring(colon + 1).Trim();
                cases.Add(string.IsNullOrEmpty(rest) ? [] : [rest]);
            }
            else
            {
                cases[^1].Add(line.Trim());
            }
        }

        Assert.AreEqual(numCases, cases.Count);
        return cases;
    }

    private static List<string> StripCaseHeader(string caseText, int caseNum)
    {
        string[] lines = caseText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        string prefix = $"Case #{caseNum}:";
        Assert.IsTrue(lines[0].StartsWith(prefix, StringComparison.Ordinal));
        List<string> res = [];
        string first = lines[0].Substring(prefix.Length).Trim();
        if (!string.IsNullOrEmpty(first))
            res.Add(first);
        for (int i = 1; i < lines.Length; i++)
            res.Add(lines[i].Trim());
        return res;
    }

    private static int VerifyFallingBallsOutput(List<string> lines, int c, int[] expectedBalls)
    {
        Assert.GreaterOrEqual(lines.Count, 1);
        if (lines.Count == 1 && string.Equals(lines[0], "IMPOSSIBLE", StringComparison.OrdinalIgnoreCase))
            return int.MaxValue;

        Assert.IsTrue(int.TryParse(lines[0], out int rows) && rows >= 1);
        Assert.AreEqual(rows + 1, lines.Count, "Row count mismatch with layout lines.");

        int[] balls = new int[c];
        Array.Fill(balls, 1);

        for (int r = 1; r <= rows; r++)
        {
            string s = lines[r];
            Assert.AreEqual(c, s.Length);
            Assert.AreEqual('.', s[0], "Leftmost column must be empty.");
            Assert.AreEqual('.', s[c - 1], "Rightmost column must be empty.");
            for (int j = 0; j < c; j++)
            {
                Assert.IsTrue(s[j] is '.' or '\\' or '/');
                if (r == rows)
                    Assert.AreEqual('.', s[j], "Bottommost row must be empty.");
                if (s[j] == '\\')
                    Assert.AreNotEqual('/', s[j + 1], "Invalid \\/ adjacent ramps.");
            }

            int[] nextBalls = new int[c];
            for (int j = 0; j < c; j++)
            {
                if (s[j] == '.')
                    nextBalls[j] += balls[j];
                else if (s[j] == '\\')
                    nextBalls[j + 1] += balls[j];
                else
                    nextBalls[j - 1] += balls[j];
            }
            balls = nextBalls;
        }

        CollectionAssert.AreEqual(expectedBalls, balls, "Resulting ball distribution mismatch.");
        return rows;
    }

    #endregion

    #region Q2 Graceful Chainsaw Jugglers

    private static readonly Lazy<int[,]> ChainsawDp = new(BuildChainsawDp);

    [Test]
    public void Q2Sample() => Test(Q2);

    [Test]
    public void Q2Small() => Test(Q2);

    [Test]
    public void Q2Large() => Test(Q2);

    private static int[,] BuildChainsawDp()
    {
        const int maxVal = 500;
        int[,] dp = new int[maxVal + 1, maxVal + 1];
        for (int r = 0; r <= 32; r++)
        {
            for (int b = 0; b <= 32; b++)
            {
                if (r == 0 && b == 0)
                    continue;
                if ((b + 1) * r * (r + 1) / 2 > maxVal || (r + 1) * b * (b + 1) / 2 > maxVal)
                    continue;

                for (int i = maxVal; i >= r; i--)
                {
                    for (int j = maxVal; j >= b; j--)
                    {
                        int cand = dp[i - r, j - b] + 1;
                        if (cand > dp[i, j])
                            dp[i, j] = cand;
                    }
                }
            }
        }

        return dp;
    }

    private static Func<int> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int b);
        return () => ChainsawDp.Value[r, b];
    }

    #endregion

    #region Q3 Costume Change

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<int> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        int[,] grid = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            testCase.Get(out int[] row);
            for (int j = 0; j < n; j++)
                grid[i, j] = row[j];
        }

        return () =>
        {
            Dictionary<int, List<(int R, int C)>> byValue = [];
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    int val = grid[r, c];
                    if (!byValue.TryGetValue(val, out var list))
                    {
                        list = [];
                        byValue[val] = list;
                    }
                    list.Add((r, c));
                }
            }

            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = [];

            int kept = 0;
            foreach (var list in byValue.Values)
            {
                for (int i = 0; i < n; i++)
                    adj[i].Clear();
                foreach (var (r, c) in list)
                    adj[r].Add(c);

                kept += BipartiteMatching.FindMaximumMatching(n, n, adj).Count;
            }

            return n * n - kept;
        };
    }

    #endregion

    #region Q4 Gridception

    [Test]
    public void Q4Sample() => Test(Q4);

    [Test]
    public void Q4Small() => Test(Q4);

    [Test]
    public void Q4Large() => Test(Q4);

    private static Func<int> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int c);
        string[] lines = new string[r];
        for (int i = 0; i < r; i++)
            testCase.Get(out lines[i]);

        return () =>
        {
            int[,] grid = new int[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    grid[i, j] = lines[i][j] == 'W' ? 1 : 0;

            bool[] hasPattern = new bool[16];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    int v = grid[i, j];
                    hasPattern[(v << 3) | (v << 2) | (v << 1) | v] = true;

                    if (j + 1 < c)
                    {
                        int vR = grid[i, j + 1];
                        hasPattern[(v << 3) | (vR << 2) | (v << 1) | vR] = true;
                    }
                    if (i + 1 < r)
                    {
                        int vD = grid[i + 1, j];
                        hasPattern[(v << 3) | (v << 2) | (vD << 1) | vD] = true;
                    }
                    if (i + 1 < r && j + 1 < c)
                    {
                        int pat = (v << 3) | (grid[i, j + 1] << 2) | (grid[i + 1, j] << 1) | grid[i + 1, j + 1];
                        hasPattern[pat] = true;
                    }
                }
            }

            int[,] seenStamp = new int[r, c];
            int stamp = 0;
            int[] qR = new int[r * c];
            int[] qC = new int[r * c];
            int best = 1;

            for (int pat = 0; pat < 16; pat++)
            {
                if (!hasPattern[pat])
                    continue;

                int c00 = (pat >> 3) & 1;
                int c01 = (pat >> 2) & 1;
                int c10 = (pat >> 1) & 1;
                int c11 = pat & 1;

                for (int splitR = 0; splitR <= r; splitR++)
                {
                    for (int splitC = 0; splitC <= c; splitC++)
                    {
                        stamp++;
                        for (int sr = 0; sr < r; sr++)
                        {
                            for (int sc = 0; sc < c; sc++)
                            {
                                if (seenStamp[sr, sc] == stamp)
                                    continue;

                                int target = sr < splitR
                                    ? (sc < splitC ? c00 : c01)
                                    : (sc < splitC ? c10 : c11);
                                if (grid[sr, sc] != target)
                                    continue;

                                int head = 0, tail = 0;
                                seenStamp[sr, sc] = stamp;
                                qR[tail] = sr;
                                qC[tail] = sc;
                                tail++;

                                while (head < tail)
                                {
                                    int cr = qR[head];
                                    int cc = qC[head];
                                    head++;

                                    TryPush(cr - 1, cc);
                                    TryPush(cr + 1, cc);
                                    TryPush(cr, cc - 1);
                                    TryPush(cr, cc + 1);
                                }

                                if (tail > best)
                                    best = tail;

                                void TryPush(int nr, int nc)
                                {
                                    if ((uint)nr >= (uint)r || (uint)nc >= (uint)c || seenStamp[nr, nc] == stamp)
                                        return;
                                    int exp = nr < splitR
                                        ? (nc < splitC ? c00 : c01)
                                        : (nc < splitC ? c10 : c11);
                                    if (grid[nr, nc] != exp)
                                        return;
                                    seenStamp[nr, nc] = stamp;
                                    qR[tail] = nr;
                                    qC[tail] = nc;
                                    tail++;
                                }
                            }
                        }
                    }
                }
            }

            return best;
        };
    }

    #endregion
}
