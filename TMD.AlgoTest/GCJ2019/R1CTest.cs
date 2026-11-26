using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class R1CTest : BaseGCJTest
{
    #region Q1 Robot Programming Strategy

    [Test]
    public void Q1Sample() => Test(Q1, ValidateRps);

    [Test]
    public void Q1Small() => Test(Q1, ValidateRps);

    [Test]
    public void Q1Large() => Test(Q1, ValidateRps);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int a);
        testCase.GetLines(a, out string[] opponents);
        return () =>
        {
            bool[] defeated = new bool[a];
            StringBuilder program = new();

            for (int step = 0; step < 500; step++)
            {
                int mask = 0;
                for (int i = 0; i < a; i++)
                {
                    if (defeated[i])
                        continue;
                    char mv = opponents[i][step % opponents[i].Length];
                    mask |= mv switch
                    {
                        'R' => 1,
                        'P' => 2,
                        'S' => 4,
                        _ => 0
                    };
                }

                if (mask == 7)
                    return "IMPOSSIBLE";

                if (BitOperations.PopCount((uint)mask) == 1)
                {
                    char winMove = mask switch
                    {
                        1 => 'P',
                        2 => 'S',
                        _ => 'R'
                    };
                    program.Append(winMove);
                    return program.ToString();
                }

                char chosen;
                char beats;
                if (mask == (1 | 2))
                {
                    chosen = 'P';
                    beats = 'R';
                }
                else if (mask == (2 | 4))
                {
                    chosen = 'S';
                    beats = 'P';
                }
                else
                {
                    chosen = 'R';
                    beats = 'S';
                }

                program.Append(chosen);
                for (int i = 0; i < a; i++)
                {
                    if (!defeated[i] && opponents[i][step % opponents[i].Length] == beats)
                        defeated[i] = true;
                }
            }

            return "IMPOSSIBLE";
        };
    }

    private static void ValidateRps(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        bool[] expectedPossible = ParsePossibleFlags(expectedOutput, numCases);
        int ptr = 1;
        for (int tc = 0; tc < numCases; tc++)
        {
            int a = int.Parse(inputLines[ptr++].Trim(), CultureInfo.InvariantCulture);
            string[] opps = new string[a];
            for (int i = 0; i < a; i++)
                opps[i] = inputLines[ptr++].Trim().ToUpperInvariant();

            string[] lines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, lines.Length);
            string prefix = $"Case #{tc + 1}:";
            Assert.IsTrue(lines[0].StartsWith(prefix, StringComparison.Ordinal));
            string[] tokens = lines[0][prefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, tokens.Length);
            string ans = tokens[0].ToUpperInvariant();

            if (!expectedPossible[tc])
            {
                Assert.AreEqual("IMPOSSIBLE", ans, $"Case #{tc + 1}: expected IMPOSSIBLE.");
                continue;
            }

            Assert.AreNotEqual("IMPOSSIBLE", ans, $"Case #{tc + 1}: claimed IMPOSSIBLE when valid exists.");
            Assert.LessOrEqual(ans.Length, 500, $"Case #{tc + 1}: program too long.");
            foreach (char ch in ans)
                Assert.IsTrue(ch is 'R' or 'P' or 'S', $"Case #{tc + 1}: invalid move '{ch}'.");

            foreach (string opp in opps)
                Assert.IsTrue(IsProgramWin(ans, opp), $"Case #{tc + 1}: loses/ties to opponent '{opp}'.");
        }
    }

    private static bool IsProgramWin(string a, string b)
    {
        StringBuilder cSb = new(a.Length + b.Length);
        while (cSb.Length < a.Length)
            cSb.Append(b);
        string c = cSb.ToString();
        string combined = a + c + c;
        int[] z = Pattern.ComputeZArray(combined.AsSpan());

        int nowB = 0;
        while (true)
        {
            int lcp = z[a.Length + nowB];
            if (lcp < a.Length)
            {
                char ma = a[lcp];
                char mb = b[(lcp + nowB) % b.Length];
                return (ma == 'R' && mb == 'S') || (ma == 'S' && mb == 'P') || (ma == 'P' && mb == 'R');
            }
            nowB = (nowB + a.Length) % b.Length;
            if (nowB == 0)
                return false;
        }
    }

    private static bool[] ParsePossibleFlags(string expectedOutput, int numCases)
    {
        bool[] flags = new bool[numCases];
        foreach (string line in expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("Case #", StringComparison.Ordinal))
                continue;
            int colonIdx = line.IndexOf(':');
            int caseNum = int.Parse(line[6..colonIdx], CultureInfo.InvariantCulture);
            string word = line[(colonIdx + 1)..].Trim();
            flags[caseNum - 1] = !string.Equals(word, "IMPOSSIBLE", StringComparison.OrdinalIgnoreCase);
        }
        return flags;
    }

    #endregion

    #region Q2 Power Arrangers

    [Test]
    public void Q2Small() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 0));

    [Test]
    public void Q2Large() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 1));

    private static void Q2Solve(GCJInteractiveChannel io)
    {
        string[] header = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(header[0], CultureInfo.InvariantCulture);
        int[] targetCounts = [23, 5, 1];

        for (int tc = 0; tc < t; tc++)
        {
            List<int> cand = new(119);
            for (int i = 0; i < 119; i++)
                cand.Add(i);

            char[] missing = new char[5];
            bool[] used = new bool[5];

            for (int pos = 0; pos < 3; pos++)
            {
                List<int>[] buckets = new List<int>[5];
                for (int d = 0; d < 5; d++)
                    buckets[d] = [];

                foreach (int setIdx in cand)
                {
                    io.WriteLine((setIdx * 5 + pos + 1).ToString(CultureInfo.InvariantCulture));
                    string ch = io.ReadRequiredLine();
                    Assert.AreNotEqual("N", ch);
                    buckets[ch[0] - 'A'].Add(setIdx);
                }

                for (int d = 0; d < 5; d++)
                {
                    if (!used[d] && buckets[d].Count == targetCounts[pos])
                    {
                        used[d] = true;
                        missing[pos] = (char)('A' + d);
                        cand = buckets[d];
                        break;
                    }
                }
            }

            io.WriteLine((cand[0] * 5 + 4).ToString(CultureInfo.InvariantCulture));
            char present4Th = io.ReadRequiredLine()[0];
            used[present4Th - 'A'] = true;
            missing[4] = present4Th;
            for (int d = 0; d < 5; d++)
            {
                if (!used[d])
                {
                    missing[3] = (char)('A' + d);
                    break;
                }
            }

            io.WriteLine(new string(missing));
            string verdict = io.ReadRequiredLine();
            Assert.AreEqual("Y", verdict, $"Case #{tc + 1}: wrong answer {new string(missing)}.");
        }
    }

    private static void Q2Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int numCases = 50;
        int maxInspection = testNumber == 0 ? 475 : 150;
        Random rng = new(514514 + testNumber);

        List<string> allPerms = [];
        GeneratePermutations("ABCDE".ToCharArray(), 0, allPerms);

        io.WriteLine($"{numCases} {maxInspection}");
        for (int tc = 1; tc <= numCases; tc++)
        {
            string[] shuffled = [.. allPerms];
            rng.Shuffle(shuffled);
            string answer = shuffled[^1];
            string figures = string.Concat(shuffled.AsSpan(0, 119));

            bool solved = false;
            for (int i = 0; i <= maxInspection; i++)
            {
                string input = io.ReadRequiredLine();
                Assert.IsFalse(input.Contains('\n') || input.Contains('\r'));
                if (int.TryParse(input, CultureInfo.InvariantCulture, out int pos))
                {
                    Assert.Less(i, maxInspection, $"Case #{tc}: inspection limit {maxInspection} exceeded.");
                    Assert.That(pos, Is.InRange(1, figures.Length));
                    io.WriteLine(figures[pos - 1].ToString());
                }
                else
                {
                    Assert.AreEqual(5, input.Length, $"Case #{tc}: invalid answer '{input}'.");
                    char[] sorted = input.ToCharArray();
                    Array.Sort(sorted);
                    Assert.AreEqual("ABCDE", new string(sorted));
                    Assert.AreEqual(answer, input, $"Case #{tc}: wrong answer '{input}', expected '{answer}'.");
                    io.WriteLine("Y");
                    solved = true;
                    break;
                }
            }
            Assert.IsTrue(solved);
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    private static void GeneratePermutations(char[] arr, int idx, List<string> result)
    {
        if (idx == arr.Length)
        {
            result.Add(new string(arr));
            return;
        }
        for (int i = idx; i < arr.Length; i++)
        {
            (arr[idx], arr[i]) = (arr[i], arr[idx]);
            GeneratePermutations(arr, idx + 1, result);
            (arr[idx], arr[i]) = (arr[i], arr[idx]);
        }
    }

    #endregion

    #region Q3 Bacterial Tactics

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int c);
        testCase.GetLines(r, out string[] grid);
        return () =>
        {
            int[,] badPref = new int[r + 1, c + 1];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    badPref[i + 1, j + 1] =
                        badPref[i, j + 1] + badPref[i + 1, j] - badPref[i, j] + (grid[i][j] == '#' ? 1 : 0);
                }
            }

            bool HasHash(int r1, int r2, int c1, int c2) =>
                badPref[r2, c2] - badPref[r1, c2] - badPref[r2, c1] + badPref[r1, c1] > 0;

            int[,,,] memo = new int[r + 1, r + 1, c + 1, c + 1];
            for (int a = 0; a <= r; a++)
                for (int b = 0; b <= r; b++)
                    for (int x = 0; x <= c; x++)
                        for (int y = 0; y <= c; y++)
                            memo[a, b, x, y] = -1;

            int Grundy(int r1, int r2, int c1, int c2)
            {
                if (r1 >= r2 || c1 >= c2)
                    return 0;
                int cached = memo[r1, r2, c1, c2];
                if (cached >= 0)
                    return cached;

                ulong seen = 0;
                for (int row = r1; row < r2; row++)
                {
                    if (!HasHash(row, row + 1, c1, c2))
                    {
                        int g = Grundy(r1, row, c1, c2) ^ Grundy(row + 1, r2, c1, c2);
                        if (g < 64)
                            seen |= 1UL << g;
                    }
                }
                for (int col = c1; col < c2; col++)
                {
                    if (!HasHash(r1, r2, col, col + 1))
                    {
                        int g = Grundy(r1, r2, c1, col) ^ Grundy(r1, r2, col + 1, c2);
                        if (g < 64)
                            seen |= 1UL << g;
                    }
                }

                int mex = BitOperations.TrailingZeroCount(~seen);
                return memo[r1, r2, c1, c2] = mex;
            }

            int winningMoves = 0;
            for (int row = 0; row < r; row++)
            {
                if (!HasHash(row, row + 1, 0, c) && (Grundy(0, row, 0, c) ^ Grundy(row + 1, r, 0, c)) == 0)
                    winningMoves += c;
            }
            for (int col = 0; col < c; col++)
            {
                if (!HasHash(0, r, col, col + 1) && (Grundy(0, r, 0, col) ^ Grundy(0, r, col + 1, c)) == 0)
                    winningMoves += r;
            }

            return winningMoves.ToString(CultureInfo.InvariantCulture);
        };
    }

    #endregion
}
