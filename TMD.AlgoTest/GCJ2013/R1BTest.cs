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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - Osmos

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out long a, out int n);
            test.Get(out long[] motes);
            return () => Q1Solver(a, n, motes);
        }

        private static int Q1Solver(long a, int n, long[] motes)
        {
            if (a == 1)
                return n;

            Array.Sort(motes);
            int best = n;
            int ops = 0;
            long cur = a;
            for (int i = 0; i < n; i++)
            {
                while (cur <= motes[i])
                {
                    cur += cur - 1;
                    ops++;
                }
                cur += motes[i];
                int cand = ops + (n - 1 - i);
                if (cand < best)
                    best = cand;
            }
            return best;
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

        #region Question 2 - Falling Diamonds

        private static readonly double[] LogFact = BuildLogFact();

        private static double[] BuildLogFact()
        {
            double[] lf = new double[3001];
            for (int i = 1; i < lf.Length; i++)
                lf[i] = lf[i - 1] + Math.Log(i);
            return lf;
        }

        private static Func<double> Q2Solver(GCJTestCase test)
        {
            test.Get(out long n, out int x, out int y);
            return () => Q2Solver(n, x, y);
        }

        private static double Q2Solver(long n, int x, int y)
        {
            long k = ((long)Math.Abs(x) + y) >> 1;
            long prev = k * (2L * k - 1);
            long full = (k + 1) * (2L * k + 1);
            if (n >= full)
                return 1.0;
            if (n <= prev || x == 0)
                return 0.0;

            int rem = (int)(n - prev);
            int need = y + 1;
            int sideCap = (int)(2 * k);
            if (rem >= sideCap + need)
                return 1.0;
            if (rem < need)
                return 0.0;

            double logHalfPow = -rem * Math.Log(2.0);
            double prob = 0.0;
            for (int j = need; j <= rem; j++)
                prob += Math.Exp(LogFact[rem] - LogFact[j] - LogFact[rem - j] + logHalfPow);
            return Math.Min(1.0, prob);
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

        #region Question 3 - Garbled Email

        private sealed class CompactTrie
        {
            public int[] Next;
            public uint[] ChildMask;
            public bool[] IsWord;
            public int NodeCount = 1;

            public CompactTrie(int initialCapacity)
            {
                Next = new int[initialCapacity * 26];
                ChildMask = new uint[initialCapacity];
                IsWord = new bool[initialCapacity];
            }

            private void EnsureCapacity(int needed)
            {
                if (needed <= ChildMask.Length)
                    return;
                int newCap = Math.Max(needed, ChildMask.Length * 2);
                Array.Resize(ref Next, newCap * 26);
                Array.Resize(ref ChildMask, newCap);
                Array.Resize(ref IsWord, newCap);
            }

            public void AddWord(string word)
            {
                int node = 0;
                for (int i = 0; i < word.Length; i++)
                {
                    int c = word[i] - 'a';
                    int edgeIdx = node * 26 + c;
                    int child = Next[edgeIdx];
                    if (child == 0)
                    {
                        child = NodeCount++;
                        EnsureCapacity(NodeCount);
                        Next[edgeIdx] = child;
                        ChildMask[node] |= 1u << c;
                    }
                    node = child;
                }
                IsWord[node] = true;
            }
        }

        private static (int TestCount, Func<GCJTestCase, Func<int>> TestCaseHandler) Q3Setup(GCJTestCase header)
        {
            header.Get(out int w);
            int totalChars = 0;
            string[] words = new string[w];
            for (int i = 0; i < w; i++)
            {
                header.Get(out string word);
                words[i] = word;
                totalChars += word.Length;
            }

            // Deduplicating prefixes makes trie nodes well below totalChars + 1 (~1.15M vs 3.32M).
            int initCap = Math.Max(64, Math.Min(totalChars + 1, 1_300_000));
            CompactTrie trie = new(initCap);
            for (int i = 0; i < w; i++)
                trie.AddWord(words[i]);

            header.Get(out int t);
            return (t, test =>
            {
                test.Get(out string s);
                return () => Q3Solver(s, trie);
            });
        }

        private static int Q3Solver(string s, CompactTrie trie)
        {
            const int Inf = 1_000_000_000;
            int n = s.Length;
            int[] dp = new int[(n + 1) * 5];
            Array.Fill(dp, Inf);
            dp[0] = 0;

            int[] next = trie.Next;
            uint[] childMask = trie.ChildMask;
            bool[] isWord = trie.IsWord;

            void Dfs(int node, int pos, int wait, int cost)
            {
                if (isWord[node])
                {
                    int dpIdx = pos * 5 + wait;
                    if (cost < dp[dpIdx])
                        dp[dpIdx] = cost;
                }
                if (pos == n)
                    return;

                int actual = s[pos] - 'a';
                int exactChild = next[node * 26 + actual];
                if (exactChild != 0)
                {
                    int nextWait = wait > 0 ? wait - 1 : 0;
                    Dfs(exactChild, pos + 1, nextWait, cost);
                }

                if (wait == 0)
                {
                    uint mask = childMask[node] & ~(1u << actual);
                    int nodeBase = node * 26;
                    while (mask != 0)
                    {
                        int c = BitOperations.TrailingZeroCount(mask);
                        Dfs(next[nodeBase + c], pos + 1, 4, cost + 1);
                        mask &= mask - 1;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int w = 0; w < 5; w++)
                {
                    int c = dp[i * 5 + w];
                    if (c < Inf)
                        Dfs(0, i, w, c);
                }
            }

            int best = Inf;
            for (int w = 0; w < 5; w++)
            {
                int cand = dp[n * 5 + w];
                if (cand < best)
                    best = cand;
            }
            return best;
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Setup);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Setup);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Setup);
        }

        #endregion
    }
}
