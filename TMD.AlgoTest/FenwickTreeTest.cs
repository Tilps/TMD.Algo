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
    public class FenwickTreeTest
    {
        [Test]
        public void OneDimensionalBasicAndLowerBound()
        {
            long[] initial = [2, 0, 3, 1, 2];
            var bit = new FenwickTree(initial.AsSpan());
            Assert.AreEqual(5, bit.Length);
            Assert.AreEqual(8, bit.TotalSum());
            for (int i = 0; i < initial.Length; i++)
            {
                Assert.AreEqual(initial[i], bit.Get(i));
            }
            Assert.AreEqual(4, bit.RangeSum(1, 3));
            Assert.AreEqual(0, bit.RangeSum(3, 1));
            Assert.AreEqual(0, bit.RangeSum(0, -1));
            Assert.AreEqual(0, bit.RangeSum(5, 4));

            Assert.AreEqual(0, bit.LowerBound(-5));
            Assert.AreEqual(0, bit.LowerBound(0));
            Assert.AreEqual(0, bit.LowerBound(1));
            Assert.AreEqual(0, bit.LowerBound(2));
            Assert.AreEqual(2, bit.LowerBound(3));
            Assert.AreEqual(2, bit.LowerBound(5));
            Assert.AreEqual(3, bit.LowerBound(6));
            Assert.AreEqual(4, bit.LowerBound(8));
            Assert.AreEqual(5, bit.LowerBound(9));

            bit.Set(1, 4);
            Assert.AreEqual(4, bit.Get(1));
            Assert.AreEqual(12, bit.TotalSum());
            Assert.AreEqual(1, bit.LowerBound(3));

            bit.Clear();
            Assert.AreEqual(0, bit.TotalSum());
            Assert.AreEqual(5, bit.LowerBound(1));

            var empty = new FenwickTree(0);
            Assert.AreEqual(0, empty.TotalSum());
            Assert.AreEqual(0, empty.LowerBound(0));
            Assert.AreEqual(0, empty.LowerBound(1));
            Assert.AreEqual(0, empty.RangeSum(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => empty.Get(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FenwickTree(-1));
        }

        [Test]
        public void OneDimensionalRandomMatchesBruteForce()
        {
            const int n = 64;
            long[] baseline = new long[n];
            var bit = new FenwickTree(n);
            Random rnd = new(1234);

            for (int step = 0; step < 300; step++)
            {
                int idx = rnd.Next(n);
                long delta = rnd.Next(-100, 101);
                baseline[idx] += delta;
                bit.Add(idx, delta);

                int l = rnd.Next(n);
                int r = rnd.Next(n);
                long expected = 0;
                for (int i = l; i <= r; i++)
                    expected += baseline[i];
                Assert.AreEqual(expected, bit.RangeSum(l, r));
            }
        }

        [Test]
        public void TwoDimensionalMatchesBruteForce()
        {
            const int rows = 12;
            const int cols = 15;
            long[,] baseline = new long[rows, cols];
            var bit = new FenwickTree2D(rows, cols);
            Random rnd = new(5678);

            for (int step = 0; step < 250; step++)
            {
                int r = rnd.Next(rows);
                int c = rnd.Next(cols);
                long delta = rnd.Next(-50, 51);
                baseline[r, c] += delta;
                bit.Add(r, c, delta);

                int r1 = rnd.Next(rows);
                int c1 = rnd.Next(cols);
                int r2 = rnd.Next(rows);
                int c2 = rnd.Next(cols);
                long expected = 0;
                for (int rr = r1; rr <= r2; rr++)
                {
                    for (int cc = c1; cc <= c2; cc++)
                    {
                        expected += baseline[rr, cc];
                    }
                }
                Assert.AreEqual(expected, bit.RangeSum(r1, c1, r2, c2));
            }

            bit.Clear();
            Assert.AreEqual(0, bit.RangeSum(0, 0, rows - 1, cols - 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FenwickTree2D(-1, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => bit.Add(rows, 0, 1));
        }
    }
}
