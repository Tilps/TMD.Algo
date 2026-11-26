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
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class SparseTableTest
    {
        [Test]
        public void EmptyAndSingleElementEdgeCases()
        {
            SparseTable<int, RangeMinInt32Policy> empty = new(Array.Empty<int>());
            Assert.AreEqual(0, empty.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => empty.Query(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => empty.Query(0, 1));

            SparseTable<int, RangeMinInt32Policy> single = new(new[] { 42 });
            Assert.AreEqual(1, single.Count);
            Assert.AreEqual(42, single.Query(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => single.Query(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => single.Query(1, 1));

            Assert.Throws<ArgumentNullException>(() => new SparseTable<int, RangeMinInt32Policy>((IReadOnlyList<int>)null!));
        }

        [Test]
        public void MinMaxAndGcdAgainstBruteForce()
        {
            int[] values32 = { 7, -3, 12, 0, -9, 5, 5, 14, -1, 8, 3, -4, 19, 2, -6, 11, 0 };
            long[] values64 = { 12, 18, 24, 36, 15, 25, 35, 0, 14, 21, 28, 42, -14, 7, 70, 105, 5 };

            SparseTable<int, RangeMinInt32Policy> min32 = new(values32.AsSpan());
            SparseTable<int, RangeMaxInt32Policy> max32 = new((IReadOnlyList<int>)values32);
            SparseTable<long, RangeMinInt64Policy> min64 = new(values64.AsSpan());
            SparseTable<long, RangeMaxInt64Policy> max64 = new(values64.AsSpan());
            SparseTable<long, RangeGcdInt64Policy> gcd64 = new(values64.AsSpan());

            for (int l = 0; l < values32.Length; l++)
            {
                int bruteMin32 = values32[l];
                int bruteMax32 = values32[l];
                long bruteMin64 = values64[l];
                long bruteMax64 = values64[l];
                long bruteGcd64 = Math.Abs(values64[l]);

                for (int r = l + 1; r <= values32.Length; r++)
                {
                    if (r > l + 1)
                    {
                        bruteMin32 = Math.Min(bruteMin32, values32[r - 1]);
                        bruteMax32 = Math.Max(bruteMax32, values32[r - 1]);
                        bruteMin64 = Math.Min(bruteMin64, values64[r - 1]);
                        bruteMax64 = Math.Max(bruteMax64, values64[r - 1]);
                        bruteGcd64 = MathFormulas.Gcd(bruteGcd64, values64[r - 1]);
                    }

                    Assert.AreEqual(bruteMin32, min32.Query(l, r));
                    Assert.AreEqual(bruteMax32, max32.Query(l, r));
                    Assert.AreEqual(bruteMin64, min64.Query(l, r));
                    Assert.AreEqual(bruteMax64, max64.Query(l, r));
                    Assert.AreEqual(bruteGcd64, gcd64.Query(l, r));
                }
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => min32.Query(-1, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => min32.Query(4, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => min32.Query(5, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => min32.Query(0, values32.Length + 1));
        }

        [Test]
        public void PowerOfTwoLength()
        {
            long[] values = { 100, -50, 25, 200, -75, 10, 0, 50 };
            SparseTable<long, RangeMaxInt64Policy> max64 = new(values.AsSpan());
            Assert.AreEqual(200, max64.Query(0, 8));
            Assert.AreEqual(200, max64.Query(0, 4));
            Assert.AreEqual(50, max64.Query(4, 8));
        }
    }
}
