using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class R1CTest : BaseGCJTest
{
    #region Q1 A Whole New Word

    [Test]
    public void Q1Sample() => Test(Q1, ValidateWholeNewWord);

    [Test]
    public void Q1Small() => Test(Q1, ValidateWholeNewWord);

    [Test]
    public void Q1Large() => Test(Q1, ValidateWholeNewWord);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int n, out int l);
        string[] words = new string[n];
        for (int i = 0; i < n; i++)
            testCase.Get(out words[i]);

        return () =>
        {
            HashSet<string> existing = new(words, StringComparer.Ordinal);
            List<char>[] letters = new List<char>[l];
            for (int col = 0; col < l; col++)
            {
                bool[] seen = new bool[26];
                letters[col] = [];
                for (int i = 0; i < n; i++)
                {
                    char ch = words[i][col];
                    if (!seen[ch - 'A'])
                    {
                        seen[ch - 'A'] = true;
                        letters[col].Add(ch);
                    }
                }
            }

            char[] buf = new char[l];
            string? Dfs(int pos)
            {
                if (pos == l)
                {
                    string candidate = new(buf);
                    return existing.Contains(candidate) ? null : candidate;
                }

                foreach (char ch in letters[pos])
                {
                    buf[pos] = ch;
                    string? found = Dfs(pos + 1);
                    if (found is not null)
                        return found;
                }

                return null;
            }

            return Dfs(0) ?? "-";
        };
    }

    private static void ValidateWholeNewWord(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        string[] expLines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(inLines[0]);
        Assert.AreEqual(t, expLines.Length);
        Assert.AreEqual(t, actualCases.Count);

        int idx = 1;
        for (int tc = 0; tc < t; tc++)
        {
            string[] nl = inLines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int n = int.Parse(nl[0]);
            int l = int.Parse(nl[1]);

            HashSet<string> words = new(StringComparer.OrdinalIgnoreCase);
            HashSet<char>[] avail = new HashSet<char>[l];
            for (int c = 0; c < l; c++)
                avail[c] = [];

            for (int i = 0; i < n; i++)
            {
                string w = inLines[idx++].Trim();
                words.Add(w);
                for (int c = 0; c < l; c++)
                    avail[c].Add(char.ToLowerInvariant(w[c]));
            }

            string expPrefix = $"Case #{tc + 1}: ";
            Assert.IsTrue(expLines[tc].StartsWith(expPrefix, StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(actualCases[tc].StartsWith(expPrefix, StringComparison.Ordinal));
            string expAns = expLines[tc].Substring(expPrefix.Length).Trim();
            string actAns = actualCases[tc].Substring(expPrefix.Length).Trim();

            if (expAns == "-")
            {
                Assert.AreEqual("-", actAns, $"Case #{tc + 1}: expected '-'.");
            }
            else
            {
                Assert.AreNotEqual("-", actAns, $"Case #{tc + 1}: claimed '-' when valid word '{expAns}' exists.");
                Assert.AreEqual(l, actAns.Length, $"Case #{tc + 1}: wrong word length.");
                Assert.IsFalse(words.Contains(actAns), $"Case #{tc + 1}: '{actAns}' already in input.");
                for (int c = 0; c < l; c++)
                {
                    Assert.IsTrue(avail[c].Contains(char.ToLowerInvariant(actAns[c])),
                        $"Case #{tc + 1}: char '{actAns[c]}' at position {c} not available.");
                }
            }
        }
    }

    #endregion

    #region Q2 Lollipop Shop

    [Test]
    public void Q2Small() => InteractiveTest(Q2Solve, Q2Judge);

    private static void Q2Solve(GCJInteractiveChannel io)
    {
        int t = int.Parse(io.ReadRequiredLine());
        Random rng = new(42);
        for (int tc = 0; tc < t; tc++)
        {
            int n = int.Parse(io.ReadRequiredLine());
            if (n == -1)
                throw new InvalidOperationException("Judge reported wrong answer.");

            int[] seenCount = new int[n];
            bool[] sold = new bool[n];

            for (int c = 0; c < n; c++)
            {
                string line = io.ReadRequiredLine();
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int d = int.Parse(parts[0]);
                if (d == -1)
                    throw new InvalidOperationException("Judge reported wrong answer.");

                int bestFlavor = -1;
                int bestSeen = int.MaxValue;
                int ties = 0;
                for (int j = 1; j <= d; j++)
                {
                    int flavor = int.Parse(parts[j]);
                    seenCount[flavor]++;
                    if (sold[flavor])
                        continue;

                    if (seenCount[flavor] < bestSeen)
                    {
                        bestSeen = seenCount[flavor];
                        bestFlavor = flavor;
                        ties = 1;
                    }
                    else if (seenCount[flavor] == bestSeen)
                    {
                        ties++;
                        if (rng.Next(ties) == 0)
                            bestFlavor = flavor;
                    }
                }

                if (bestFlavor >= 0)
                    sold[bestFlavor] = true;
                io.WriteLine(bestFlavor.ToString());
            }
        }
    }

    private static void Q2Judge(GCJInteractiveChannel io)
    {
        const int totalCases = 50;
        const int n = 200;
        Random rng = new(126);

        io.WriteLine(totalCases.ToString());
        for (int tc = 1; tc <= totalCases; tc++)
        {
            io.WriteLine(n.ToString());
            double[] probs = new double[n];
            for (int f = 0; f < n; f++)
                probs[f] = 0.005 + 0.095 * rng.NextDouble();

            int[][] prefs = new int[n][];
            int[][] adj = new int[n][];
            for (int cust = 0; cust < n; cust++)
            {
                List<int> liked = [];
                for (int f = 0; f < n; f++)
                {
                    if (rng.NextDouble() < probs[f])
                        liked.Add(f);
                }
                prefs[cust] = [.. liked];
                adj[cust] = prefs[cust];
            }

            int opt = BipartiteMatching.FindMaximumMatching(n, n, adj).Count;
            bool[] used = new bool[n];
            int score = 0;

            for (int cust = 0; cust < n; cust++)
            {
                int[] line = prefs[cust];
                io.WriteLine(line.Length == 0 ? "0" : $"{line.Length} {string.Join(' ', line)}");
                string resp = io.ReadRequiredLine();
                Assert.IsFalse(resp.Contains('\n') || resp.Contains('\r'));
                string[] tokens = resp.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(1, tokens.Length, $"Case #{tc}: expected 1 token, got '{resp}'.");
                Assert.IsTrue(int.TryParse(tokens[0], out int guess), $"Case #{tc}: invalid token '{tokens[0]}'.");
                Assert.That(guess, Is.InRange(-1, n - 1), $"Case #{tc}: guess {guess} out of range.");

                if (guess == -1)
                    continue;
                Assert.IsFalse(used[guess], $"Case #{tc}: flavor {guess} already sold.");
                used[guess] = true;
                Assert.IsTrue(Array.IndexOf(line, guess) >= 0, $"Case #{tc}: flavor {guess} disliked by customer {cust}.");
                score++;
            }

            int required = opt - opt / 10;
            Assert.GreaterOrEqual(score, required,
                $"Case #{tc}: sold {score}, below required {required} (opt={opt}).");
        }

        Assert.IsNull(io.ReadLine(), "Additional output after all cases finish.");
    }

    #endregion

    #region Q3 Ant Stack

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<int> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.Get(out long[] w);

        return () =>
        {
            const long inf = long.MaxValue / 4;
            long[] dp = new long[145];
            Array.Fill(dp, inf);
            dp[0] = 0;
            int best = 0;

            for (int i = 0; i < n; i++)
            {
                long weight = w[i];
                long limit = 6L * weight;
                for (int k = best; k >= 0; k--)
                {
                    if (dp[k] <= limit)
                    {
                        long nextWeight = dp[k] + weight;
                        if (nextWeight < dp[k + 1])
                        {
                            dp[k + 1] = nextWeight;
                            if (k + 1 > best)
                                best = k + 1;
                        }
                    }
                }
            }

            return best;
        };
    }

    #endregion
}
