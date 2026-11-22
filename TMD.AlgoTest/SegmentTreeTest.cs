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

namespace TMD.AlgoTest
{
    [TestFixture]
    public class SegmentTreeTest
    {
        private readonly struct IntSumMonoid : ISegmentTreeMonoid<int>
        {
            public int Identity => 0;
            public int Combine(int left, int right) => left + right;
        }

        [Test]
        public void PointSegmentTreeRandomizedAndEdgeCases()
        {
            SegmentTree<int, IntSumMonoid> empty = new(0);
            Assert.AreEqual(0, empty.Count);
            Assert.AreEqual(0, empty.Query(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => empty.Get(0));

            SegmentTree<int, IntSumMonoid> single = new([42]);
            Assert.AreEqual(42, single.Get(0));
            Assert.AreEqual(42, single.Query(0, 1));
            single.Set(0, -7);
            Assert.AreEqual(-7, single.Query(0, 1));

            const int n = 37;
            int[] brute = new int[n];
            Random rng = new(201301);
            for (int i = 0; i < n; i++)
                brute[i] = rng.Next(-100, 101);

            SegmentTree<int, IntSumMonoid> st = new(brute);
            for (int step = 0; step < 300; step++)
            {
                if (step % 3 == 0)
                {
                    int idx = rng.Next(n);
                    int val = rng.Next(-500, 501);
                    brute[idx] = val;
                    st.Set(idx, val);
                }
                else
                {
                    int l = rng.Next(n + 1);
                    int r = rng.Next(l, n + 1);
                    int expected = 0;
                    for (int i = l; i < r; i++)
                        expected += brute[i];
                    Assert.AreEqual(expected, st.Query(l, r));
                }
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => st.Query(-1, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => st.Query(5, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => st.Query(0, n + 1));
        }

        [Test]
        public void LazySegmentTreeSumAddAndMinAdd()
        {
            LazySegmentTree<long, long, RangeSumAddInt64Policy> empty = new(0);
            Assert.AreEqual(0L, empty.Query(0, 0));
            empty.Update(0, 0, 5L);

            const int n = 45;
            long[] initial = new long[n];
            Random rng = new(201302);
            for (int i = 0; i < n; i++)
                initial[i] = rng.Next(-20, 50);

            long[] sumBrute = (long[])initial.Clone();
            long[] minBrute = (long[])initial.Clone();
            LazySegmentTree<long, long, RangeSumAddInt64Policy> sumTree = new(initial);
            LazySegmentTree<long, long, RangeMinAddInt64Policy> minTree = new(initial);

            for (int step = 0; step < 400; step++)
            {
                int l = rng.Next(n + 1);
                int r = rng.Next(l, n + 1);
                if (step % 2 == 0)
                {
                    long delta = rng.Next(-30, 31);
                    for (int i = l; i < r; i++)
                    {
                        sumBrute[i] += delta;
                        minBrute[i] += delta;
                    }
                    sumTree.Update(l, r, delta);
                    minTree.Update(l, r, delta);
                }
                else
                {
                    long expSum = 0L;
                    long expMin = long.MaxValue;
                    for (int i = l; i < r; i++)
                    {
                        expSum += sumBrute[i];
                        if (minBrute[i] < expMin)
                            expMin = minBrute[i];
                    }
                    Assert.AreEqual(expSum, sumTree.Query(l, r));
                    Assert.AreEqual(expMin, minTree.Query(l, r));
                }
            }

            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(sumBrute[i], sumTree.Get(i));
                Assert.AreEqual(minBrute[i], minTree.Get(i));
            }
        }

        [Test]
        public void LazySegmentTreeMinChmax()
        {
            const int n = 35;
            int[] brute = new int[n];
            LazySegmentTree<int, int, RangeMinChmaxInt32Policy> tree = new(n, 0);
            Random rng = new(201303);

            for (int step = 0; step < 300; step++)
            {
                int l = rng.Next(n + 1);
                int r = rng.Next(l, n + 1);
                if (step % 2 == 0)
                {
                    int h = rng.Next(1, 200);
                    for (int i = l; i < r; i++)
                        brute[i] = Math.Max(brute[i], h);
                    tree.Update(l, r, h);
                }
                else
                {
                    int expMin = int.MaxValue;
                    for (int i = l; i < r; i++)
                        expMin = Math.Min(expMin, brute[i]);
                    Assert.AreEqual(expMin, tree.Query(l, r));
                }
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => tree.Update(-1, 2, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => tree.Get(n));
        }
    }
}
