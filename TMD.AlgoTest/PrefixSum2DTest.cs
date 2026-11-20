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
    public class PrefixSum2DTest
    {
        [Test]
        public void IntAndLongGridQueriesMatchBruteForce()
        {
            int[,] grid =
            {
                { 1, -2, 3, 4 },
                { 5, 0, -1, 2 },
                { -3, 7, 8, -4 }
            };

            PrefixSum2D<int> psInt = new(grid);
            PrefixSum2D<long> psLong = new(3, 4, (r, c) => grid[r, c]);

            Assert.AreEqual(3, psInt.Rows);
            Assert.AreEqual(4, psInt.Cols);

            for (int r1 = 0; r1 < 3; r1++)
            for (int c1 = 0; c1 < 4; c1++)
            for (int r2 = r1; r2 < 3; r2++)
            for (int c2 = c1; c2 < 4; c2++)
            {
                int expected = 0;
                for (int r = r1; r <= r2; r++)
                for (int c = c1; c <= c2; c++)
                    expected += grid[r, c];

                Assert.AreEqual(expected, psInt.Query(r1, c1, r2, c2));
                Assert.AreEqual((long)expected, psLong.Query(r1, c1, r2, c2));
            }

            Assert.AreEqual(0, psInt.Query(2, 0, 1, 3));
            Assert.AreEqual(0, psInt.Query(0, 2, 2, 1));
            Assert.AreEqual(0, psInt.Prefix(0, 0));
            Assert.AreEqual(grid[0, 0], psInt.Prefix(1, 1));
        }

        [Test]
        public void DoubleAndBoundaryValidation()
        {
            double[,] dGrid =
            {
                { 1.5, 2.25 },
                { -0.5, 4.75 }
            };
            PrefixSum2D<double> ps = new(dGrid);
            Assert.AreEqual(8.0, ps.Query(0, 0, 1, 1), 1e-12);
            Assert.AreEqual(7.0, ps.Query(0, 1, 1, 1), 1e-12);
        }

        [Test]
        public void ThrowsOnInvalidBounds()
        {
            PrefixSum2D<int> empty = new(0, 0, (_, _) => 0);
            Assert.AreEqual(0, empty.Prefix(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => empty.Query(0, 0, 0, 0));

            PrefixSum2D<int> ps = new(new int[2, 3]);
            Assert.Throws<ArgumentOutOfRangeException>(() => ps.Prefix(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ps.Prefix(3, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ps.Query(-1, 0, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ps.Query(0, 0, 2, 1));
        }
    }
}
