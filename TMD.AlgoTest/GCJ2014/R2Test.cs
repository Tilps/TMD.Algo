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
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Data Packing

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int x);
            test.Get(out int[] sizes);
            return () => Q1Solver(x, sizes);
        }

        private static int Q1Solver(int x, int[] sizes)
        {
            Array.Sort(sizes);
            int lo = 0;
            int hi = sizes.Length - 1;
            int discs = 0;
            while (lo <= hi)
            {
                if (lo < hi && sizes[lo] + sizes[hi] <= x)
                    lo++;
                hi--;
                discs++;
            }
            return discs;
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

        #region Question 2 - Up and Down

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] a);
            return () => Q2Solver(n, a);
        }

        private static int Q2Solver(int n, int[] a)
        {
            int swaps = 0;
            for (int i = 0; i < n; i++)
            {
                int val = a[i];
                int leftGreater = 0;
                for (int j = 0; j < i; j++)
                {
                    if (a[j] > val)
                        leftGreater++;
                }

                int rightGreater = 0;
                for (int j = i + 1; j < n; j++)
                {
                    if (a[j] > val)
                        rightGreater++;
                }

                swaps += Math.Min(leftGreater, rightGreater);
            }
            return swaps;
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

        #region Question 3 - Don't Break The Nile

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int w, out int h, out int b);
            (int X0, int Y0, int X1, int Y1)[] rects = new (int, int, int, int)[b];
            for (int i = 0; i < b; i++)
                test.Get(out rects[i]);
            return () => Q3Solver(w, rects);
        }

        private static long Q3Solver(int w, (int X0, int Y0, int X1, int Y1)[] rects)
        {
            int b = rects.Length;
            int leftBank = b;
            int rightBank = b + 1;

            long Weight(int u, int v)
            {
                if (u == v)
                    return 0L;
                if (u > v)
                    (u, v) = (v, u);

                if (v == rightBank)
                    return u == leftBank ? w : w - 1 - rects[u].X1;
                if (v == leftBank)
                    return rects[u].X0;

                var a = rects[u];
                var c = rects[v];
                int dx = Math.Max(0, Math.Max(a.X0 - c.X1, c.X0 - a.X1) - 1);
                int dy = Math.Max(0, Math.Max(a.Y0 - c.Y1, c.Y0 - a.Y1) - 1);
                return Math.Max(dx, dy);
            }

            return Search.DenseDijkstra(b + 2, leftBank, rightBank, Weight);
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
            SlowTest(Q3Solver);
        }

        #endregion

        #region Question 4 - Trie Sharding

        private const long TrieMod = 1_000_000_007L;
        private static readonly long[,] CombMod = BuildCombMod();

        private static long[,] BuildCombMod()
        {
            long[,] comb = new long[101, 101];
            for (int i = 0; i <= 100; i++)
            {
                comb[i, 0] = 1L;
                comb[i, i] = 1L;
                for (int j = 1; j < i; j++)
                    comb[i, j] = (comb[i - 1, j - 1] + comb[i - 1, j]) % TrieMod;
            }
            return comb;
        }

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int m, out int n);
            test.GetLines(m, out string[] words);
            return () => Q4Solver(n, words);
        }

        private static string Q4Solver(int n, string[] words)
        {
            int maxNodes = 1;
            foreach (string w in words)
                maxNodes += w.Length;

            int[] next = new int[maxNodes * 26];
            uint[] childMask = new uint[maxNodes];
            int[] pref = new int[maxNodes];
            bool[] isTerm = new bool[maxNodes];
            int nodeCount = 1;

            foreach (string word in words)
            {
                int node = 0;
                pref[0]++;
                for (int i = 0; i < word.Length; i++)
                {
                    int c = word[i] - 'A';
                    int edgeIdx = node * 26 + c;
                    int child = next[edgeIdx];
                    if (child == 0)
                    {
                        child = nodeCount++;
                        next[edgeIdx] = child;
                        childMask[node] |= 1u << c;
                    }
                    node = child;
                    pref[node]++;
                }
                isTerm[node] = true;
            }

            int[] kArr = new int[nodeCount];
            long[] wArr = new long[nodeCount];
            int totalNodes = 0;
            Span<int> childK = stackalloc int[27];
            Span<long> childW = stackalloc long[27];

            for (int node = nodeCount - 1; node >= 0; node--)
            {
                int kp = Math.Min(n, pref[node]);
                kArr[node] = kp;
                totalNodes += kp;

                int d = 0;
                uint mask = childMask[node];
                int nodeBase = node * 26;
                while (mask != 0)
                {
                    int c = BitOperations.TrailingZeroCount(mask);
                    int child = next[nodeBase + c];
                    childK[d] = kArr[child];
                    childW[d] = wArr[child];
                    d++;
                    mask &= mask - 1;
                }
                if (isTerm[node])
                {
                    childK[d] = 1;
                    childW[d] = 1L;
                    d++;
                }

                if (d == 0)
                {
                    wArr[node] = 1L;
                }
                else if (d == 1)
                {
                    wArr[node] = childW[0];
                }
                else
                {
                    int maxKc = 0;
                    long prodChildW = 1L;
                    for (int r = 0; r < d; r++)
                    {
                        if (childK[r] > maxKc)
                            maxKc = childK[r];
                        prodChildW = (prodChildW * childW[r]) % TrieMod;
                    }

                    long inclExcl = 0L;
                    for (int j = maxKc; j <= kp; j++)
                    {
                        long term = CombMod[kp, j];
                        for (int r = 0; r < d; r++)
                            term = (term * CombMod[j, childK[r]]) % TrieMod;

                        if (((kp - j) & 1) == 0)
                            inclExcl += term;
                        else
                            inclExcl -= term;
                        inclExcl %= TrieMod;
                    }

                    inclExcl = ((inclExcl % TrieMod) + TrieMod) % TrieMod;
                    wArr[node] = (inclExcl * prodChildW) % TrieMod;
                }
            }

            return $"{totalNodes} {wArr[0]}";
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
