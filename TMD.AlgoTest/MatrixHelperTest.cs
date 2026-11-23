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
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class MatrixHelperTest
    {
        [Test]
        public void PowerComputesFibonacci()
        {
            long[,] fib = { { 1, 1 }, { 1, 0 } };
            Modulo mod = new(1_000_000_007);
            long[,] res = MatrixHelper.Power(fib, 10, mod);
            Assert.AreEqual(55, res[0, 1]);
            long[,] id = MatrixHelper.Power(fib, 0, mod);
            Assert.AreEqual(1, id[0, 0]);
            Assert.AreEqual(0, id[0, 1]);
            Assert.AreEqual(1, id[1, 1]);
        }

        [Test]
        public void DeterminantExactAndMod()
        {
            Assert.AreEqual(BigInteger.One, MatrixHelper.Determinant(new long[0, 0]));
            Assert.AreEqual(new BigInteger(-7), MatrixHelper.Determinant(new long[,] { { -7 } }));

            // Row swap after first elimination step to stress prevPivot in Bareiss.
            long[,] swapTest =
            {
                { 2, 1, 3, 4 },
                { 4, 2, 1, 5 },
                { 1, 3, 2, 6 },
                { 3, 5, 7, 2 }
            };
            BigInteger expected = new BigInteger(-267);
            Assert.AreEqual(expected, MatrixHelper.Determinant(swapTest));
            Assert.AreEqual(1_000_000_007L - 267L, MatrixHelper.DeterminantMod(swapTest, 1_000_000_007L));

            long[,] singular =
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 }
            };
            Assert.AreEqual(BigInteger.Zero, MatrixHelper.Determinant(singular));
            Assert.AreEqual(0L, MatrixHelper.DeterminantMod(singular, 1_000_000_007L));

            Assert.Throws<ArgumentException>(() => MatrixHelper.Determinant(new long[2, 3]));
            Assert.Throws<ArgumentException>(() => MatrixHelper.DeterminantMod(singular, 12));
        }

        [Test]
        public void CountSpanningTrees()
        {
            // Single vertex has 1 spanning tree.
            Assert.AreEqual(BigInteger.One, MatrixHelper.CountSpanningTrees(new bool[1, 1]));

            // Disconnected 3 vertices has 0 spanning trees.
            bool[,] disconnected = new bool[3, 3];
            disconnected[0, 1] = disconnected[1, 0] = true;
            Assert.AreEqual(BigInteger.Zero, MatrixHelper.CountSpanningTrees(disconnected));

            // Cycle C5 has 5 spanning trees.
            bool[,] cycle5 = new bool[5, 5];
            for (int i = 0; i < 5; i++)
            {
                int j = (i + 1) % 5;
                cycle5[i, j] = cycle5[j, i] = true;
            }
            Assert.AreEqual(new BigInteger(5), MatrixHelper.CountSpanningTrees(cycle5));

            // Complete graph K_22 has 22^20 spanning trees (exceeds 64-bit integer).
            bool[,] k22 = new bool[22, 22];
            for (int i = 0; i < 22; i++)
                for (int j = 0; j < 22; j++)
                    if (i != j)
                        k22[i, j] = true;
            Assert.AreEqual(BigInteger.Pow(22, 20), MatrixHelper.CountSpanningTrees(k22));

            // Sample K=8 from GCJ 2017 Finals Spanning Planning.
            bool[,] sample8 =
            {
                { false, true, true, true },
                { true, false, false, true },
                { true, false, false, true },
                { true, true, true, false }
            };
            Assert.AreEqual(new BigInteger(8), MatrixHelper.CountSpanningTrees(sample8));
        }
    }
}
