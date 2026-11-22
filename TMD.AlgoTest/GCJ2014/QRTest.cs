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
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Magic Trick

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int r1);
            int[][] g1 = new int[4][];
            for (int i = 0; i < 4; i++)
                test.Get(out g1[i]);
            test.Get(out int r2);
            int[][] g2 = new int[4][];
            for (int i = 0; i < 4; i++)
                test.Get(out g2[i]);
            return () => Q1Solver(g1[r1 - 1], g2[r2 - 1]);
        }

        private static string Q1Solver(int[] row1, int[] row2)
        {
            int mask1 = 0;
            foreach (int x in row1)
                mask1 |= 1 << x;

            int matches = 0;
            int chosen = -1;
            foreach (int x in row2)
            {
                if ((mask1 & (1 << x)) != 0)
                {
                    matches++;
                    chosen = x;
                }
            }

            if (matches == 0)
                return "Volunteer cheated!";
            if (matches == 1)
                return chosen.ToString();
            return "Bad magician!";
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

        #endregion

        #region Question 2 - Cookie Clicker Alpha

        private static Func<double> Q2Solver(GCJTestCase test)
        {
            test.Get(out double c, out double f, out double x);
            return () => Q2Solver(c, f, x);
        }

        private static double Q2Solver(double c, double f, double x)
        {
            double rate = 2.0;
            double elapsed = 0.0;
            while (c * (rate + f) < x * f)
            {
                elapsed += c / rate;
                rate += f;
            }
            return elapsed + x / rate;
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

        #region Question 3 - Minesweeper Master

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c, out int m);
            return () => Q3Solver(r, c, m);
        }

        private static string Q3Solver(int r, int c, int m)
        {
            int free = r * c - m;
            char[][] grid = new char[r][];
            for (int i = 0; i < r; i++)
            {
                grid[i] = new char[c];
                Array.Fill(grid[i], '*');
            }

            if (free == 1)
            {
                grid[0][0] = 'c';
            }
            else if (r == 1)
            {
                for (int col = 0; col < free; col++)
                    grid[0][col] = '.';
                grid[0][0] = 'c';
            }
            else if (c == 1)
            {
                for (int row = 0; row < free; row++)
                    grid[row][0] = '.';
                grid[0][0] = 'c';
            }
            else if (r == 2)
            {
                if (free < 4 || (free & 1) == 1)
                    return Environment.NewLine + "Impossible";
                int cols = free >> 1;
                for (int col = 0; col < cols; col++)
                {
                    grid[0][col] = '.';
                    grid[1][col] = '.';
                }
                grid[0][0] = 'c';
            }
            else if (c == 2)
            {
                if (free < 4 || (free & 1) == 1)
                    return Environment.NewLine + "Impossible";
                int rows = free >> 1;
                for (int row = 0; row < rows; row++)
                {
                    grid[row][0] = '.';
                    grid[row][1] = '.';
                }
                grid[0][0] = 'c';
            }
            else
            {
                if (free is 2 or 3 or 5 or 7)
                    return Environment.NewLine + "Impossible";

                int[] lengths = new int[r];
                if (free <= 2 * c + 1)
                {
                    if ((free & 1) == 0)
                    {
                        lengths[0] = lengths[1] = free >> 1;
                    }
                    else
                    {
                        lengths[0] = lengths[1] = (free - 3) >> 1;
                        lengths[2] = 3;
                    }
                }
                else
                {
                    int q = free / c;
                    int rem = free % c;
                    for (int row = 0; row < q; row++)
                        lengths[row] = c;
                    if (rem >= 2)
                    {
                        lengths[q] = rem;
                    }
                    else if (rem == 1)
                    {
                        lengths[q - 1] = c - 1;
                        lengths[q] = 2;
                    }
                }

                for (int row = 0; row < r; row++)
                {
                    for (int col = 0; col < lengths[row]; col++)
                        grid[row][col] = '.';
                }
                grid[0][0] = 'c';
            }

            string[] lines = new string[r + 1];
            lines[0] = string.Empty;
            for (int row = 0; row < r; row++)
                lines[row + 1] = new string(grid[row]);
            return string.Join(Environment.NewLine, lines);
        }

        private static readonly char[] LineSeps = ['\r', '\n'];
        private static readonly char[] TokenSeps = [' ', '\t'];

        private static void ValidateQ3(string input, string expectedOutput, List<string> results)
        {
            string[] inLines = input.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0]);
            Assert.AreEqual(t, results.Count);

            string[] expLines = expectedOutput.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            int expIdx = 0;

            for (int tc = 0; tc < t; tc++)
            {
                string[] dims = inLines[tc + 1].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                int r = int.Parse(dims[0]);
                int c = int.Parse(dims[1]);
                int m = int.Parse(dims[2]);

                Assert.IsTrue(expIdx < expLines.Length);
                Assert.AreEqual($"Case #{tc + 1}:", expLines[expIdx++].TrimEnd());
                bool expPossible = !string.Equals(expLines[expIdx].Trim(), "Impossible", StringComparison.OrdinalIgnoreCase);
                expIdx += expPossible ? r : 1;

                string[] actLines = results[tc].Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual($"Case #{tc + 1}:", actLines[0].TrimEnd(), $"Case #{tc + 1}: expected empty Case line");

                if (!expPossible)
                {
                    Assert.AreEqual(2, actLines.Length);
                    Assert.AreEqual("Impossible", actLines[1].Trim(), $"Case #{tc + 1}: expected Impossible");
                    continue;
                }

                Assert.AreEqual(r + 1, actLines.Length, $"Case #{tc + 1}: wrong grid height");
                char[][] g = new char[r][];
                int mines = 0;
                int cr = -1, cc = -1;
                for (int i = 0; i < r; i++)
                {
                    string rowStr = actLines[i + 1].Trim();
                    Assert.AreEqual(c, rowStr.Length, $"Case #{tc + 1}: wrong grid width");
                    g[i] = rowStr.ToCharArray();
                    for (int j = 0; j < c; j++)
                    {
                        char ch = g[i][j];
                        Assert.IsTrue(ch is '.' or '*' or 'c', $"Case #{tc + 1}: invalid char '{ch}'");
                        if (ch == '*')
                            mines++;
                        else if (ch == 'c')
                        {
                            Assert.AreEqual(-1, cr, $"Case #{tc + 1}: multiple clicks");
                            cr = i;
                            cc = j;
                        }
                    }
                }

                Assert.AreNotEqual(-1, cr, $"Case #{tc + 1}: missing click");
                Assert.AreEqual(m, mines, $"Case #{tc + 1}: mine count mismatch");

                bool[,] seen = new bool[r, c];
                Queue<(int R, int C)> q = new();
                seen[cr, cc] = true;
                q.Enqueue((cr, cc));
                int seenCount = 1;

                while (q.Count > 0)
                {
                    var (ur, uc) = q.Dequeue();
                    bool hasAdjMine = false;
                    for (int dr = -1; dr <= 1 && !hasAdjMine; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            int nr = ur + dr;
                            int nc = uc + dc;
                            if (nr >= 0 && nr < r && nc >= 0 && nc < c && g[nr][nc] == '*')
                            {
                                hasAdjMine = true;
                                break;
                            }
                        }
                    }
                    if (hasAdjMine)
                        continue;

                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            int nr = ur + dr;
                            int nc = uc + dc;
                            if (nr >= 0 && nr < r && nc >= 0 && nc < c && !seen[nr, nc])
                            {
                                seen[nr, nc] = true;
                                seenCount++;
                                q.Enqueue((nr, nc));
                            }
                        }
                    }
                }

                Assert.AreEqual(r * c - m, seenCount, $"Case #{tc + 1}: not all safe cells revealed ({seenCount} != {r * c - m})");
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

        #region Question 4 - Deceitful War

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out double[] naomi);
            test.Get(out double[] ken);
            return () => Q4Solver(n, naomi, ken);
        }

        private static int MaxWins(double[] winner, double[] loser)
        {
            int wins = 0;
            int j = 0;
            foreach (double w in winner)
            {
                if (w > loser[j])
                {
                    wins++;
                    j++;
                }
            }
            return wins;
        }

        private static string Q4Solver(int n, double[] naomi, double[] ken)
        {
            Array.Sort(naomi);
            Array.Sort(ken);
            int deceitful = MaxWins(naomi, ken);
            int honest = n - MaxWins(ken, naomi);
            return $"{deceitful} {honest}";
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
    }
}
