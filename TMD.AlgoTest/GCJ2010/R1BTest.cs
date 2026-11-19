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
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - File Fix-it

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.GetLines(n, out string[] existing);
            test.GetLines(m, out string[] desired);
            return () => Q1Solver(existing, desired);
        }

        private static int Q1Solver(string[] existing, string[] desired)
        {
            Trie<string> trie = new();
            foreach (string path in existing)
            {
                trie.Add(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            }

            int before = trie.NodeCount;
            foreach (string path in desired)
            {
                trie.Add(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            }

            return trie.NodeCount - before;
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

        #region Question 2 - Picking Up Chicks

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k, out long b, out long t);
            test.Get(out long[] positions);
            test.Get(out long[] speeds);
            return () => Q2Solver(n, k, b, t, positions, speeds);
        }

        private static string Q2Solver(int n, int k, long b, long t, long[] positions, long[] speeds)
        {
            int fast = 0;
            int slow = 0;
            long swaps = 0;

            for (int i = n - 1; i >= 0 && fast < k; i--)
            {
                if (positions[i] + t * speeds[i] >= b)
                {
                    fast++;
                    swaps += slow;
                }
                else
                {
                    slow++;
                }
            }

            return fast >= k ? swaps.ToString() : "IMPOSSIBLE";
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

        #region Question 3 - Your Rank is Pure

        private static readonly int[] PureAnswers = BuildPureAnswers();

        private static int[] BuildPureAnswers()
        {
            const int MaxN = 500;
            const int Mod = 100003;

            int[,] choose = new int[MaxN + 1, MaxN + 1];
            for (int i = 0; i <= MaxN; i++)
            {
                choose[i, 0] = 1;
                for (int j = 1; j <= i; j++)
                {
                    choose[i, j] = (choose[i - 1, j - 1] + choose[i - 1, j]) % Mod;
                }
            }

            int[,] dp = new int[MaxN + 1, MaxN + 1];
            int[] ans = new int[MaxN + 1];

            for (int n = 2; n <= MaxN; n++)
            {
                dp[n, 1] = 1;
                long total = 1;
                for (int k = 2; k < n; k++)
                {
                    int avail = n - k - 1;
                    long ways = 0;
                    for (int kp = 1; kp < k; kp++)
                    {
                        int need = k - kp - 1;
                        if (need <= avail)
                        {
                            ways = (ways + (long)dp[k, kp] * choose[avail, need]) % Mod;
                        }
                    }

                    dp[n, k] = (int)ways;
                    total = (total + ways) % Mod;
                }

                ans[n] = (int)total;
            }

            return ans;
        }

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            return () => PureAnswers[n];
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
    }
}
