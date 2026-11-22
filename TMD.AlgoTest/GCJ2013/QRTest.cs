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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Tic-Tac-Toe-Tomek

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.GetLines(4, out string[] board);
            return () => Q1Solver(board);
        }

        private static bool HasWon(string[] board, char player)
        {
            for (int r = 0; r < 4; r++)
            {
                bool rowWin = true;
                bool colWin = true;
                for (int c = 0; c < 4; c++)
                {
                    char rc = board[r][c];
                    if (rc != player && rc != 'T')
                        rowWin = false;
                    char cc = board[c][r];
                    if (cc != player && cc != 'T')
                        colWin = false;
                }
                if (rowWin || colWin)
                    return true;
            }

            bool d1 = true;
            bool d2 = true;
            for (int i = 0; i < 4; i++)
            {
                char c1 = board[i][i];
                if (c1 != player && c1 != 'T')
                    d1 = false;
                char c2 = board[i][3 - i];
                if (c2 != player && c2 != 'T')
                    d2 = false;
            }
            return d1 || d2;
        }

        private static string Q1Solver(string[] board)
        {
            if (HasWon(board, 'X'))
                return "X won";
            if (HasWon(board, 'O'))
                return "O won";

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (board[r][c] == '.')
                        return "Game has not completed";
                }
            }
            return "Draw";
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

        #region Question 2 - Lawnmower

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            int[][] grid = new int[n][];
            for (int i = 0; i < n; i++)
                test.Get(out grid[i]);
            return () => Q2Solver(n, m, grid);
        }

        private static string Q2Solver(int n, int m, int[][] grid)
        {
            int[] rowMax = new int[n];
            int[] colMax = new int[m];
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < m; c++)
                {
                    int h = grid[r][c];
                    if (h > rowMax[r])
                        rowMax[r] = h;
                    if (h > colMax[c])
                        colMax[c] = h;
                }
            }

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < m; c++)
                {
                    int h = grid[r][c];
                    if (h < rowMax[r] && h < colMax[c])
                        return "NO";
                }
            }
            return "YES";
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

        #region Question 3 - Fair and Square

        private static readonly BigInteger[] FairAndSquares = BuildFairAndSquares();

        private static BigInteger[] BuildFairAndSquares()
        {
            List<BigInteger> results = [1, 4, 9];
            char[] buf = new char[50];

            void AddRoot(int len)
            {
                BigInteger y = BigInteger.Parse(buf.AsSpan(0, len));
                results.Add(y * y);
            }

            for (int len = 2; len <= 50; len++)
            {
                Array.Fill(buf, '0', 0, len);
                buf[0] = '2';
                buf[len - 1] = '2';
                AddRoot(len);
                if ((len & 1) == 1)
                {
                    buf[len >> 1] = '1';
                    AddRoot(len);
                }
            }

            for (int len = 2; len <= 50; len++)
            {
                Array.Fill(buf, '0', 0, len);
                buf[0] = '1';
                buf[len - 1] = '1';
                int half = (len - 2) >> 1;
                bool isOdd = (len & 1) == 1;
                int mid = len >> 1;

                void AddPairsAndCenter(char center)
                {
                    if (isOdd)
                        buf[mid] = center;

                    AddRoot(len);
                    for (int i = 1; i <= half; i++)
                    {
                        buf[i] = '1';
                        buf[len - 1 - i] = '1';
                        AddRoot(len);
                        if (center != '2')
                        {
                            for (int j = i + 1; j <= half; j++)
                            {
                                buf[j] = '1';
                                buf[len - 1 - j] = '1';
                                AddRoot(len);
                                for (int k = j + 1; k <= half; k++)
                                {
                                    buf[k] = '1';
                                    buf[len - 1 - k] = '1';
                                    AddRoot(len);
                                    buf[k] = '0';
                                    buf[len - 1 - k] = '0';
                                }
                                buf[j] = '0';
                                buf[len - 1 - j] = '0';
                            }
                        }
                        buf[i] = '0';
                        buf[len - 1 - i] = '0';
                    }
                }

                AddPairsAndCenter('0');
                if (isOdd)
                {
                    AddPairsAndCenter('1');
                    AddPairsAndCenter('2');
                }
            }

            results.Sort();
            return [.. results];
        }

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out BigInteger a, out BigInteger b);
            return () => SpecialtySelect.UpperBound<BigInteger>(FairAndSquares, b)
                - SpecialtySelect.LowerBound<BigInteger>(FairAndSquares, a);
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Large1()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Large2()
        {
            Test(Q3Solver);
        }

        #endregion

        #region Question 4 - Treasure

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int k, out int n);
            test.Get(out int[] startKeys);
            int[] needKey = new int[n];
            int[][] insideKeys = new int[n][];
            int maxType = 200;

            foreach (int key in startKeys)
            {
                if (key > maxType)
                    maxType = key;
            }

            for (int i = 0; i < n; i++)
            {
                test.Get(out int[] row);
                needKey[i] = row[0];
                if (row[0] > maxType)
                    maxType = row[0];
                int cnt = row[1];
                int[] keys = new int[cnt];
                for (int j = 0; j < cnt; j++)
                {
                    int key = row[2 + j];
                    keys[j] = key;
                    if (key > maxType)
                        maxType = key;
                }
                insideKeys[i] = keys;
            }

            return () => Q4Solver(n, maxType, startKeys, needKey, insideKeys);
        }

        private static string Q4Solver(int n, int maxType, int[] startKeys, int[] needKey, int[][] insideKeys)
        {
            int[] hand = new int[maxType + 1];
            int[] remNeed = new int[maxType + 1];
            int[] remInside = new int[maxType + 1];
            List<int>[] chestsByKey = new List<int>[maxType + 1];
            for (int t = 0; t <= maxType; t++)
                chestsByKey[t] = [];

            foreach (int key in startKeys)
                hand[key]++;

            bool[] containsSelf = new bool[n];
            for (int c = 0; c < n; c++)
            {
                int req = needKey[c];
                remNeed[req]++;
                chestsByKey[req].Add(c);
                foreach (int key in insideKeys[c])
                {
                    remInside[key]++;
                    if (key == req)
                        containsSelf[c] = true;
                }
            }

            for (int t = 1; t <= maxType; t++)
            {
                if (hand[t] + remInside[t] < remNeed[t])
                    return "IMPOSSIBLE";
            }

            bool[] opened = new bool[n];
            bool[] visited = new bool[maxType + 1];
            int[] queue = new int[maxType + 1];

            bool IsReachable()
            {
                Array.Clear(visited);
                int head = 0;
                int tail = 0;
                for (int t = 1; t <= maxType; t++)
                {
                    if (hand[t] > 0)
                    {
                        visited[t] = true;
                        queue[tail++] = t;
                    }
                }

                while (head < tail)
                {
                    int t = queue[head++];
                    foreach (int c in chestsByKey[t])
                    {
                        if (opened[c])
                            continue;
                        foreach (int nextKey in insideKeys[c])
                        {
                            if (!visited[nextKey])
                            {
                                visited[nextKey] = true;
                                queue[tail++] = nextKey;
                            }
                        }
                    }
                }

                for (int t = 1; t <= maxType; t++)
                {
                    if (remNeed[t] > 0 && !visited[t])
                        return false;
                }
                return true;
            }

            if (!IsReachable())
                return "IMPOSSIBLE";

            int[] order = new int[n];
            for (int step = 0; step < n; step++)
            {
                int chosen = -1;
                for (int c = 0; c < n; c++)
                {
                    if (opened[c])
                        continue;
                    int req = needKey[c];
                    int prevHand = hand[req];
                    if (prevHand == 0)
                        continue;

                    opened[c] = true;
                    hand[req]--;
                    remNeed[req]--;
                    foreach (int key in insideKeys[c])
                        hand[key]++;

                    if (prevHand >= 2 || containsSelf[c] || IsReachable())
                    {
                        chosen = c;
                        break;
                    }

                    opened[c] = false;
                    hand[req]++;
                    remNeed[req]++;
                    foreach (int key in insideKeys[c])
                        hand[key]--;
                }

                if (chosen < 0)
                    return "IMPOSSIBLE";
                order[step] = chosen + 1;
            }

            return string.Join(" ", order);
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
