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
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Runs

        private static readonly Modulo RunsMod = CreateRunsModulo();

        private static Modulo CreateRunsModulo()
        {
            Modulo mod = new(1_000_003L);
            mod.PrecomputeFactorials(450_000);
            return mod;
        }

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out string s);
            return () => Q1Solver(s);
        }

        private static long FastChoose(int n, int k)
        {
            if (k < 0 || k > n)
                return 0L;
            return RunsMod.Multiply(
                RunsMod.Multiply(RunsMod.Factorial(n), RunsMod.InverseFactorial(k)),
                RunsMod.InverseFactorial(n - k));
        }

        private static long Q1Solver(string s)
        {
            const long Mod = 1_000_003L;
            int targetRuns = 1;
            int[] counts = new int[26];
            counts[s[0] - 'a']++;
            for (int i = 1; i < s.Length; i++)
            {
                if (s[i] != s[i - 1])
                    targetRuns++;
                counts[s[i] - 'a']++;
            }

            List<int> freqs = [with(capacity: 26)];
            for (int c = 0; c < 26; c++)
            {
                if (counts[c] > 0)
                    freqs.Add(counts[c]);
            }

            long[] dp = new long[targetRuns + 1];
            long[] next = new long[targetRuns + 1];
            dp[1] = 1;
            int placed = freqs[0];

            for (int fi = 1; fi < freqs.Count; fi++)
            {
                int nc = freqs[fi];
                Array.Clear(next);

                for (int r0 = 1; r0 < targetRuns; r0++)
                {
                    long curWays = dp[r0];
                    if (curWays == 0)
                        continue;

                    int maxR = targetRuns;
                    int badSlots = placed - r0;
                    int goodSlots = r0 + 1;

                    for (int r = r0 + 1; r <= maxR; r++)
                    {
                        int dr = r - r0;
                        int maxY = Math.Min(dr / 2, Math.Min(badSlots, nc));
                        int minY = Math.Max(0, Math.Max((dr - goodSlots + 1) / 2, dr - nc));

                        long trans = 0;
                        for (int y = minY; y <= maxY; y++)
                        {
                            int x = dr - 2 * y;
                            int parts = x + y;
                            if (parts < 1 || parts > nc)
                                continue;
                            long cGood = FastChoose(goodSlots, x);
                            long cBad = FastChoose(badSlots, y);
                            long cSplit = FastChoose(nc - 1, parts - 1);
                            long term = cGood * cBad % Mod * cSplit % Mod;
                            trans += term;
                        }

                        if (trans != 0)
                            next[r] = (next[r] + curWays * (trans % Mod)) % Mod;
                    }
                }

                (dp, next) = (next, dp);
                placed += nc;
            }

            return dp[targetRuns];
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

        #region Question 2 - Rains Over Atlantis

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int h, out int w, out long m);
            long[,] heights = new long[h, w];
            for (int r = 0; r < h; r++)
            {
                test.Get(out long[] row);
                for (int c = 0; c < w; c++)
                    heights[r, c] = row[c];
            }
            return () => Q2Solver(h, w, m, heights);
        }

        private static long Q2Solver(int rows, int cols, long erodeMax, long[,] h)
        {
            long[,] w = new long[rows, cols];
            long[,] wMin = new long[rows, cols];
            PriorityQueue<(int R, int C), long> pq = new();
            ReadOnlySpan<int> dr = [-1, 1, 0, 0];
            ReadOnlySpan<int> dc = [0, 0, -1, 1];

            long days = 0;
            while (true)
            {
                pq.Clear();
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (r == 0 || r == rows - 1 || c == 0 || c == cols - 1)
                        {
                            w[r, c] = h[r, c];
                            pq.Enqueue((r, c), h[r, c]);
                        }
                        else
                        {
                            w[r, c] = long.MaxValue;
                        }
                    }
                }

                while (pq.TryDequeue(out var cell, out long d))
                {
                    if (d > w[cell.R, cell.C])
                        continue;
                    for (int k = 0; k < 4; k++)
                    {
                        int nr = cell.R + dr[k];
                        int nc = cell.C + dc[k];
                        if ((uint)nr >= (uint)rows || (uint)nc >= (uint)cols)
                            continue;
                        long nd = Math.Max(d, h[nr, nc]);
                        if (nd < w[nr, nc])
                        {
                            w[nr, nc] = nd;
                            pq.Enqueue((nr, nc), nd);
                        }
                    }
                }

                long maxH = 0;
                bool hasSubmerged = false;
                long minSubmergedDiff = long.MaxValue;
                bool canMultiStep = true;

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        long curH = h[r, c];
                        if (curH == 0)
                            continue;
                        if (curH > maxH)
                            maxH = curH;

                        long minN = long.MaxValue;
                        for (int k = 0; k < 4; k++)
                        {
                            int nr = r + dr[k];
                            int nc = c + dc[k];
                            long nw = ((uint)nr >= (uint)rows || (uint)nc >= (uint)cols) ? 0L : w[nr, nc];
                            if (nw < minN)
                                minN = nw;
                        }
                        wMin[r, c] = minN;

                        long water = w[r, c];
                        if (water > curH)
                        {
                            hasSubmerged = true;
                            long diff = water - curH;
                            if (diff < minSubmergedDiff)
                                minSubmergedDiff = diff;
                        }
                        else if (minN > 0 && water - minN < erodeMax)
                        {
                            canMultiStep = false;
                        }
                    }
                }

                if (maxH == 0)
                    break;

                if (canMultiStep && !hasSubmerged)
                {
                    days += MathFormulas.DivCeil(maxH, erodeMax);
                    break;
                }

                if (canMultiStep)
                {
                    long steps = MathFormulas.DivCeil(minSubmergedDiff, erodeMax);
                    long drop = steps * erodeMax;
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            if (h[r, c] > 0 && w[r, c] == h[r, c])
                                h[r, c] = Math.Max(0L, h[r, c] - drop);
                        }
                    }
                    days += steps;
                }
                else
                {
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            if (h[r, c] > 0)
                                h[r, c] -= Math.Min(w[r, c] - wMin[r, c], erodeMax);
                        }
                    }
                    days++;
                }
            }

            return days;
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
            SlowTest(Q2Solver);
        }

        #endregion

        #region Question 3 - Program within a Program

        private static readonly string[] FixedTmLoopRules =
        [
            "9 0 -> R",
            "9 1 -> W 9 3",
            "9 2 -> W 10 1",
            "9 3 -> W 10 2",
            "10 0 -> E 11 0",
            "10 1 -> W 10 1",
            "10 2 -> W 10 2",
            "10 3 -> W 10 3",
            "11 1 -> E 12 0",
            "12 0 -> W 9 3",
            "12 1 -> E 12 1",
            "12 2 -> E 13 1",
            "12 3 -> E 14 1",
            "13 0 -> W 10 1",
            "13 1 -> E 12 2",
            "13 2 -> E 13 2",
            "13 3 -> E 14 2",
            "14 0 -> W 10 2",
            "14 1 -> E 12 3",
            "14 2 -> E 13 3",
            "14 3 -> E 14 3"
        ];

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            return () => Q3Solver(n);
        }

        private static string Q3Solver(int n)
        {
            int k = n + 1;
            int[] digits = new int[9];
            for (int i = 0; i < 9; i++)
            {
                digits[i] = k % 3;
                k /= 3;
            }

            List<string> rules = [with(capacity: 30)];
            for (int i = 0; i < 8; i++)
                rules.Add($"{i} 0 -> E {i + 1} {digits[8 - i] + 1}");

            rules.Add(digits[0] switch
            {
                0 => "8 0 -> W 9 3",
                1 => "8 0 -> W 10 1",
                _ => "8 0 -> W 10 2"
            });

            rules.AddRange(FixedTmLoopRules);
            return $"30\n{string.Join('\n', rules)}";
        }

        private static bool TryParseStrictInt(string token, int min, int max, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(token))
                return false;
            if ((token.Length > 1 && token[0] == '0') || token.StartsWith("-0", StringComparison.Ordinal))
                return false;
            if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return false;
            return value >= min && value <= max;
        }

        private static void ValidateQ3(string input, string expectedOutput, List<string> result)
        {
            string[] expLines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            string[] limitToks = expLines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int maxInstructions = int.Parse(limitToks[0], CultureInfo.InvariantCulture);
            int maxMoves = int.Parse(limitToks[1], CultureInfo.InvariantCulture);
            int minMem = int.Parse(limitToks[2], CultureInfo.InvariantCulture);
            int maxMem = int.Parse(limitToks[3], CultureInfo.InvariantCulture);

            string[] inToks = input.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            int numCases = int.Parse(inToks[0], CultureInfo.InvariantCulture);
            Assert.AreEqual(numCases, result.Count);

            for (int caseIdx = 0; caseIdx < numCases; caseIdx++)
            {
                int targetPos = int.Parse(inToks[1 + caseIdx], CultureInfo.InvariantCulture);
                string[] lines = result[caseIdx].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                Assert.IsTrue(lines.Length >= 1);

                string[] header = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, header.Length);
                Assert.AreEqual("Case", header[0]);
                Assert.AreEqual($"#{caseIdx + 1}:", header[1]);
                Assert.IsTrue(TryParseStrictInt(header[2], 0, maxInstructions, out int numRules));
                Assert.AreEqual(numRules + 1, lines.Length);

                Dictionary<(int State, int Mark), (char Action, int NextState, int NextMark)> rules = [];
                for (int r = 1; r <= numRules; r++)
                {
                    string[] tok = lines[r].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Assert.IsTrue(tok.Length == 4 || tok.Length == 6);
                    Assert.IsTrue(TryParseStrictInt(tok[0], minMem, maxMem, out int st));
                    Assert.IsTrue(TryParseStrictInt(tok[1], minMem, maxMem, out int mk));
                    Assert.AreEqual("->", tok[2]);
                    char action = char.ToUpperInvariant(tok[3][0]);
                    Assert.AreEqual(1, tok[3].Length);
                    Assert.IsTrue(action == 'W' || action == 'E' || action == 'R');

                    int ns = 0, nm = 0;
                    if (action == 'R')
                    {
                        Assert.AreEqual(4, tok.Length);
                    }
                    else
                    {
                        Assert.AreEqual(6, tok.Length);
                        Assert.IsTrue(TryParseStrictInt(tok[4], minMem, maxMem, out ns));
                        Assert.IsTrue(TryParseStrictInt(tok[5], minMem, maxMem, out nm));
                    }

                    Assert.IsTrue(rules.TryAdd((st, mk), (action, ns, nm)), $"Duplicate rule ({st}, {mk}) in case {caseIdx + 1}");
                }

                Dictionary<int, int> tape = [];
                int state = 0;
                int pos = 0;
                int moves = 0;
                bool halted = false;
                while (moves < maxMoves)
                {
                    moves++;
                    int mark = tape.GetValueOrDefault(pos, 0);
                    Assert.IsTrue(rules.TryGetValue((state, mark), out var instr),
                        $"Missing rule ({state}, {mark}) in case {caseIdx + 1}");
                    tape[pos] = instr.NextMark;
                    state = instr.NextState;
                    if (instr.Action == 'R')
                    {
                        Assert.AreEqual(targetPos, pos, $"Case #{caseIdx + 1}: halted at {pos} instead of {targetPos}");
                        halted = true;
                        break;
                    }
                    pos += instr.Action == 'E' ? 1 : -1;
                }

                Assert.IsTrue(halted, $"Case #{caseIdx + 1}: exceeded {maxMoves} moves without halting.");
            }
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver, ValidateQ3);
        }

        #endregion

        #region Question 4 - Ace in the Hole

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] order);
            return () => Q4Solver(n, order);
        }

        private static string Q4Solver(int n, int[] order)
        {
            List<int> remPos = [with(capacity: n)];
            List<int> remVal = [with(capacity: n)];
            for (int i = 1; i <= n; i++)
            {
                remPos.Add(i);
                remVal.Add(i);
            }

            int[] ans = new int[n + 1];
            int[] posOfVal = new int[n + 1];

            foreach (int g in order)
            {
                int idx = remPos.BinarySearch(g);
                int m = remPos.Count;
                int chosenIdx;

                if (idx < m - 1)
                {
                    chosenIdx = idx + 1;
                }
                else if (m <= 2)
                {
                    chosenIdx = m - 1;
                }
                else
                {
                    int vPrev = remVal[m - 2];
                    int vMax = remVal[m - 1];
                    bool blocked = false;
                    for (int v = vPrev + 1; v < vMax; v++)
                    {
                        int p = posOfVal[v];
                        if (p > 0 && p < g)
                        {
                            blocked = true;
                            break;
                        }
                    }
                    chosenIdx = blocked ? m - 1 : m - 2;
                }

                int chosenVal = remVal[chosenIdx];
                ans[g] = chosenVal;
                posOfVal[chosenVal] = g;
                remPos.RemoveAt(idx);
                remVal.RemoveAt(chosenIdx);
            }

            return string.Join(" ", ans.AsSpan(1).ToArray());
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

        #region Question 5 - Google Royale

        private static Func<string> Q5Solver(GCJTestCase test)
        {
            test.Get(out long a, out long m, out long v);
            return () => Q5Solver(a, m, v);
        }

        private static string Q5Solver(long a, long m, long v)
        {
            List<long> strictS = [];
            List<long> strictL = [];
            HashSet<long> optionalPoints = [];
            long bestL = 1;
            long halfV = v / 2;

            for (int i = 53; i >= 0; i--)
            {
                long mi = m >> i;
                if (mi == 0)
                    continue;
                if (mi > halfV)
                    break;

                long l = -2L * mi * ((1L << i) - 1);
                if (l < bestL)
                {
                    bestL = l;
                    strictS.Add(mi);
                    strictL.Add(l);
                }
                else if (l == bestL)
                {
                    optionalPoints.Add(mi);
                }
            }

            strictS.Add(v);

            int j = 0;
            while (j + 1 < strictS.Count && strictS[j + 1] <= a)
                j++;

            double prob = (double)(a - strictL[j]) / (strictS[j + 1] - strictL[j]);
            for (int t = j + 1; t < strictL.Count; t++)
                prob *= (double)(strictS[t] - strictL[t]) / (strictS[t + 1] - strictL[t]);

            long maxBet = (a == strictS[j] || optionalPoints.Contains(a))
                ? a
                : Math.Min(Math.Min(a - strictS[j], strictS[j + 1] - a), m);

            return $"{prob.ToString("F9", CultureInfo.InvariantCulture)} {maxBet}";
        }

        private static bool ApproxEqual(double expected, double actual, double tol)
        {
            double diff = Math.Abs(expected - actual);
            if (diff <= tol)
                return true;
            if (Math.Abs(expected) <= tol || Math.Abs(actual) <= tol)
                return false;
            return diff / Math.Abs(expected) <= tol || diff / Math.Abs(actual) <= tol;
        }

        private static void ValidateQ5(string _, string expectedOutput, List<string> result)
        {
            string[] expLines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expLines.Length, result.Count);

            for (int i = 0; i < expLines.Length; i++)
            {
                string[] expToks = expLines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] resToks = result[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(4, expToks.Length);
                Assert.AreEqual(4, resToks.Length);
                Assert.AreEqual(expToks[0], resToks[0]);
                Assert.AreEqual(expToks[1], resToks[1]);

                double expProb = double.Parse(expToks[2], CultureInfo.InvariantCulture);
                double resProb = double.Parse(resToks[2], CultureInfo.InvariantCulture);
                Assert.IsTrue(ApproxEqual(expProb, resProb, 1e-5),
                    $"Case #{i + 1}: probability mismatch (expected {expProb}, got {resProb})");

                long expBet = long.Parse(expToks[3], CultureInfo.InvariantCulture);
                long resBet = long.Parse(resToks[3], CultureInfo.InvariantCulture);
                Assert.AreEqual(expBet, resBet, $"Case #{i + 1}: first bet mismatch");
            }
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver, ValidateQ5);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver, ValidateQ5);
        }

        [Test]
        public void Q5Large()
        {
            Test(Q5Solver, ValidateQ5);
        }

        #endregion
    }
}
