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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2011
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Bot Trust

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out string[] tokens);
            return () => Q1Solver(tokens);
        }

        private static int Q1Solver(string[] tokens)
        {
            int n = int.Parse(tokens[0], CultureInfo.InvariantCulture);
            int posO = 1, timeO = 0;
            int posB = 1, timeB = 0;
            int total = 0;

            for (int i = 0; i < n; i++)
            {
                char robot = tokens[1 + 2 * i][0];
                int button = int.Parse(tokens[2 + 2 * i], CultureInfo.InvariantCulture);
                if (robot == 'O')
                {
                    total = Math.Max(total, timeO + Math.Abs(button - posO)) + 1;
                    timeO = total;
                    posO = button;
                }
                else
                {
                    total = Math.Max(total, timeB + Math.Abs(button - posB)) + 1;
                    timeB = total;
                    posB = button;
                }
            }

            return total;
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

        #region Question 2 - Magicka

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out string[] tokens);
            return () => Q2Solver(tokens);
        }

        private static string Q2Solver(string[] tokens)
        {
            int idx = 0;
            int cCount = int.Parse(tokens[idx++], CultureInfo.InvariantCulture);
            char[,] combine = new char[26, 26];
            for (int i = 0; i < cCount; i++)
            {
                string rule = tokens[idx++];
                int a = rule[0] - 'A';
                int b = rule[1] - 'A';
                combine[a, b] = rule[2];
                combine[b, a] = rule[2];
            }

            int dCount = int.Parse(tokens[idx++], CultureInfo.InvariantCulture);
            int[] opposedMask = new int[26];
            for (int i = 0; i < dCount; i++)
            {
                string rule = tokens[idx++];
                int a = rule[0] - 'A';
                int b = rule[1] - 'A';
                opposedMask[a] |= 1 << b;
                opposedMask[b] |= 1 << a;
            }

            idx++; // N
            string invoke = tokens[idx];
            List<char> list = [with(capacity: invoke.Length)];

            foreach (char ch in invoke)
            {
                if (list.Count > 0)
                {
                    char comb = combine[list[^1] - 'A', ch - 'A'];
                    if (comb != '\0')
                    {
                        list[^1] = comb;
                        continue;
                    }
                }

                int chBit = opposedMask[ch - 'A'];
                bool cleared = false;
                if (chBit != 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if ((chBit & (1 << (list[i] - 'A'))) != 0)
                        {
                            list.Clear();
                            cleared = true;
                            break;
                        }
                    }
                }

                if (!cleared)
                    list.Add(ch);
            }

            return "[" + string.Join(", ", list) + "]";
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

        #region Question 3 - Candy Splitting

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int _);
            test.Get(out int[] c);
            return () => Q3Solver(c);
        }

        private static string Q3Solver(int[] c)
        {
            int xor = 0;
            long sum = 0;
            int min = int.MaxValue;
            for (int i = 0; i < c.Length; i++)
            {
                xor ^= c[i];
                sum += c[i];
                if (c[i] < min)
                    min = c[i];
            }

            return xor != 0 ? "NO" : (sum - min).ToString(CultureInfo.InvariantCulture);
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
        public void Q3Large()
        {
            Test(Q3Solver);
        }

        #endregion

        #region Question 4 - GoroSort

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] a);
            return () => Q4Solver(n, a);
        }

        private static string Q4Solver(int n, int[] a)
        {
            int misplaced = 0;
            for (int i = 0; i < n; i++)
            {
                if (a[i] != i + 1)
                    misplaced++;
            }

            return ((double)misplaced).ToString("F6", CultureInfo.InvariantCulture);
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, 1e-6);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, 1e-6);
        }

        [Test]
        public void Q4Large()
        {
            Test(Q4Solver, 1e-6);
        }

        #endregion
    }
}
