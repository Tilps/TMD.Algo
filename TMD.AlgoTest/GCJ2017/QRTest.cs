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
using System.Linq;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2017
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Oversized Pancake Flipper

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out string s, out int k);
            return () => Q1Solver(s, k);
        }

        private static string Q1Solver(string s, int k)
        {
            int n = s.Length;
            bool[] endFlip = new bool[n + 1];
            bool parity = false;
            int flips = 0;

            for (int i = 0; i < n; i++)
            {
                if (endFlip[i])
                    parity = !parity;

                bool isHappy = (s[i] == '+') ^ parity;
                if (!isHappy)
                {
                    if (i + k > n)
                        return "IMPOSSIBLE";
                    flips++;
                    parity = !parity;
                    endFlip[i + k] = !endFlip[i + k];
                }
            }

            return flips.ToString(CultureInfo.InvariantCulture);
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

        #region Question 2 - Tidy Numbers

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out string s);
            return () => Q2Solver(s);
        }

        private static string Q2Solver(string s)
        {
            char[] digits = s.ToCharArray();
            int n = digits.Length;
            int nineStart = n;

            for (int i = n - 2; i >= 0; i--)
            {
                if (digits[i] > digits[i + 1])
                {
                    digits[i]--;
                    nineStart = i + 1;
                }
            }

            for (int i = nineStart; i < n; i++)
                digits[i] = '9';

            int firstNonZero = 0;
            while (firstNonZero < n - 1 && digits[firstNonZero] == '0')
                firstNonZero++;

            return new string(digits, firstNonZero, n - firstNonZero);
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

        #region Question 3 - Bathroom Stalls

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out long n, out long k);
            return () => Q3Solver(n, k);
        }

        private static string Q3Solver(long n, long k)
        {
            SortedDictionary<long, long> counts = new() { [n] = 1 };
            long seen = 0;

            while (true)
            {
                long x = counts.Keys.Max();
                long c = counts[x];
                counts.Remove(x);

                long maxS = x / 2;
                long minS = (x - 1) / 2;
                seen += c;
                if (seen >= k)
                    return $"{maxS} {minS}";

                if (maxS > 0)
                    counts[maxS] = counts.GetValueOrDefault(maxS) + c;
                if (minS > 0)
                    counts[minS] = counts.GetValueOrDefault(minS) + c;
            }
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small1()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small2()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver);
        }

        #endregion

        #region Question 4 - Fashion Show

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.GetLines(m, out string[] types, out int[] rows, out int[] cols);
            return () => Q4Solver(n, m, types, rows, cols);
        }

        private static string Q4Solver(int n, int m, string[] types, int[] rows, int[] cols)
        {
            char[,] orig = new char[n, n];
            bool[,] rook = new bool[n, n];
            bool[,] bishop = new bool[n, n];
            bool[] usedRow = new bool[n];
            bool[] usedCol = new bool[n];
            int diagCount = 2 * n - 1;
            bool[] usedUp = new bool[diagCount];
            bool[] usedDown = new bool[diagCount];

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    orig[r, c] = '.';

            for (int i = 0; i < m; i++)
            {
                int r = rows[i] - 1;
                int c = cols[i] - 1;
                char t = types[i][0];
                orig[r, c] = t;
                if (t == 'x' || t == 'o')
                {
                    rook[r, c] = true;
                    usedRow[r] = true;
                    usedCol[c] = true;
                }
                if (t == '+' || t == 'o')
                {
                    bishop[r, c] = true;
                    usedUp[r + c] = true;
                    usedDown[r - c + n - 1] = true;
                }
            }

            // Greedy rook completion: one per free row & column.
            int nextCol = 0;
            for (int r = 0; r < n; r++)
            {
                if (usedRow[r])
                    continue;
                while (usedCol[nextCol])
                    nextCol++;
                rook[r, nextCol] = true;
                usedCol[nextCol] = true;
            }

            // Bipartite matching on diagonals for bishops.
            List<int>[] diagAdj = new List<int>[diagCount];
            for (int u = 0; u < diagCount; u++)
                diagAdj[u] = [];

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    int u = r + c;
                    int v = r - c + n - 1;
                    if (!usedUp[u] && !usedDown[v])
                        diagAdj[u].Add(v);
                }
            }

            BipartiteMatchingResult match = BipartiteMatching.FindMaximumMatching(diagCount, diagCount, diagAdj);
            for (int u = 0; u < diagCount; u++)
            {
                int v = match.MatchLeft[u];
                if (v >= 0)
                {
                    int r = (u + v - (n - 1)) / 2;
                    int c = (u - v + (n - 1)) / 2;
                    bishop[r, c] = true;
                }
            }

            int score = 0;
            List<string> mods = [];
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    char finalType = (rook[r, c], bishop[r, c]) switch
                    {
                        (true, true) => 'o',
                        (true, false) => 'x',
                        (false, true) => '+',
                        _ => '.'
                    };
                    if (finalType == 'o') score += 2;
                    else if (finalType != '.') score += 1;

                    if (finalType != orig[r, c])
                        mods.Add($"{finalType} {r + 1} {c + 1}");
                }
            }

            StringBuilder sb = new();
            sb.Append(score).Append(' ').Append(mods.Count);
            foreach (string mod in mods)
                sb.AppendLine().Append(mod);
            return sb.ToString();
        }

        private static void ValidateFashionShow(string input, string expectedOutput, List<string> result)
        {
            char[] sep = ['\r', '\n'];
            string[] inLines = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] expLines = expectedOutput.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] actLines = string.Join(Environment.NewLine, result)
                .Split(sep, StringSplitOptions.RemoveEmptyEntries);

            int t = int.Parse(inLines[0], CultureInfo.InvariantCulture);
            int inIdx = 1;
            int expIdx = 0;
            int actIdx = 0;

            for (int c = 1; c <= t; c++)
            {
                string[] nm = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int n = int.Parse(nm[0], CultureInfo.InvariantCulture);
                int m = int.Parse(nm[1], CultureInfo.InvariantCulture);

                char[,] grid = new char[n, n];
                bool[,] preplaced = new bool[n, n];
                int actualScore = 0;
                for (int r = 0; r < n; r++)
                    for (int col = 0; col < n; col++)
                        grid[r, col] = '.';

                for (int i = 0; i < m; i++)
                {
                    string[] parts = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    char mt = parts[0][0];
                    int mr = int.Parse(parts[1], CultureInfo.InvariantCulture) - 1;
                    int mc = int.Parse(parts[2], CultureInfo.InvariantCulture) - 1;
                    grid[mr, mc] = mt;
                    preplaced[mr, mc] = true;
                    actualScore += mt == 'o' ? 2 : 1;
                }

                // Parse expected score from expected output header.
                string expPrefix = $"Case #{c}:";
                Assert.IsTrue(expLines[expIdx].StartsWith(expPrefix, StringComparison.Ordinal));
                string[] expHdr = expLines[expIdx++].Substring(expPrefix.Length)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int expectedScore = int.Parse(expHdr[0], CultureInfo.InvariantCulture);
                int expModCount = int.Parse(expHdr[1], CultureInfo.InvariantCulture);
                expIdx += expModCount;

                // Parse actual header.
                Assert.Less(actIdx, actLines.Length);
                Assert.IsTrue(actLines[actIdx].StartsWith(expPrefix, StringComparison.Ordinal));
                string[] actHdr = actLines[actIdx++].Substring(expPrefix.Length)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, actHdr.Length, $"Case #{c} header format mismatch");
                int claimedScore = int.Parse(actHdr[0], CultureInfo.InvariantCulture);
                int actModCount = int.Parse(actHdr[1], CultureInfo.InvariantCulture);
                Assert.AreEqual(expectedScore, claimedScore, $"Case #{c} optimal score mismatch");

                bool[,] modified = new bool[n, n];
                for (int i = 0; i < actModCount; i++)
                {
                    string[] modParts = actLines[actIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Assert.AreEqual(3, modParts.Length);
                    char mt = modParts[0][0];
                    Assert.IsTrue(mt == '+' || mt == 'x' || mt == 'o');
                    int mr = int.Parse(modParts[1], CultureInfo.InvariantCulture) - 1;
                    int mc = int.Parse(modParts[2], CultureInfo.InvariantCulture) - 1;
                    Assert.IsTrue(mr >= 0 && mr < n && mc >= 0 && mc < n);
                    Assert.IsFalse(modified[mr, mc], $"Case #{c} duplicate modification at ({mr + 1},{mc + 1})");
                    modified[mr, mc] = true;

                    if (preplaced[mr, mc])
                    {
                        Assert.AreEqual('o', mt, $"Case #{c} preplaced cell can only upgrade to 'o'");
                        Assert.AreNotEqual('o', grid[mr, mc], $"Case #{c} cannot replace existing 'o'");
                        actualScore += 1;
                    }
                    else
                    {
                        actualScore += mt == 'o' ? 2 : 1;
                    }
                    grid[mr, mc] = mt;
                }

                Assert.AreEqual(claimedScore, actualScore, $"Case #{c} score inconsistent with grid");

                bool[] rows = new bool[n];
                bool[] colsSeen = new bool[n];
                bool[] ups = new bool[2 * n - 1];
                bool[] downs = new bool[2 * n - 1];
                for (int r = 0; r < n; r++)
                {
                    for (int col = 0; col < n; col++)
                    {
                        char ch = grid[r, col];
                        if (ch == '.') continue;
                        if (ch != '+')
                        {
                            Assert.IsFalse(rows[r], $"Case #{c} row conflict on row {r + 1}");
                            Assert.IsFalse(colsSeen[col], $"Case #{c} col conflict on col {col + 1}");
                            rows[r] = true;
                            colsSeen[col] = true;
                        }
                        if (ch != 'x')
                        {
                            Assert.IsFalse(ups[r + col], $"Case #{c} up-diag conflict at ({r + 1},{col + 1})");
                            Assert.IsFalse(downs[r - col + n - 1], $"Case #{c} down-diag conflict at ({r + 1},{col + 1})");
                            ups[r + col] = true;
                            downs[r - col + n - 1] = true;
                        }
                    }
                }
            }
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, ValidateFashionShow);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, ValidateFashionShow);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver, ValidateFashionShow);
        }

        #endregion
    }
}
