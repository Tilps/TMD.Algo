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

namespace TMD.AlgoTest
{
    [TestFixture]
    public class OptimalFindingTest
    {
        [Test]
        public void SlidingWindowFixedWidthMatchesBruteForce()
        {
            int[] values = [4, 2, 12, 3, 8, 8, 1, 9, 5];
            CollectionAssert.AreEqual(values, OptimalFinding.SlidingWindowMin<int>(values, 1));
            CollectionAssert.AreEqual(values, OptimalFinding.SlidingWindowMax<int>(values, 1));

            for (int w = 1; w <= values.Length; w++)
            {
                int[] mins = OptimalFinding.SlidingWindowMin<int>(values, w);
                int[] maxs = OptimalFinding.SlidingWindowMax<int>(values, w);
                Assert.AreEqual(values.Length - w + 1, mins.Length);
                Assert.AreEqual(values.Length - w + 1, maxs.Length);

                for (int i = 0; i < mins.Length; i++)
                {
                    int expectedMin = int.MaxValue;
                    int expectedMax = int.MinValue;
                    for (int k = 0; k < w; k++)
                    {
                        expectedMin = Math.Min(expectedMin, values[i + k]);
                        expectedMax = Math.Max(expectedMax, values[i + k]);
                    }
                    Assert.AreEqual(expectedMin, mins[i]);
                    Assert.AreEqual(expectedMax, maxs[i]);
                }
            }

            // Custom comparer flips Min/Max semantics.
            var reverse = Comparer<int>.Create((a, b) => b.CompareTo(a));
            CollectionAssert.AreEqual(
                OptimalFinding.SlidingWindowMax<int>(values, 3),
                OptimalFinding.SlidingWindowMin<int>(values, 3, reverse));

            Assert.Throws<ArgumentOutOfRangeException>(() => OptimalFinding.SlidingWindowMin<int>(values, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => OptimalFinding.SlidingWindowMax<int>(values, values.Length + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => OptimalFinding.SlidingWindowMin<int>(ReadOnlySpan<int>.Empty, 1));
        }

        [Test]
        public void SlidingWindowRadiusMatchesBruteForceAndValidatesEdgeCases()
        {
            Random rnd = new(321);
            int[] values = new int[50];
            for (int i = 0; i < values.Length; i++)
                values[i] = rnd.Next(-100, 100);

            int[] actualMin = new int[values.Length];
            int[] actualMax = new int[values.Length];
            foreach ((int left, int right) in new[] { (0, 0), (3, 3), (0, 5), (4, 1), (100, 100), (int.MaxValue, int.MaxValue) })
            {
                OptimalFinding.SlidingWindowMin<int>(values, left, right, actualMin);
                OptimalFinding.SlidingWindowMax<int>(values, left, right, actualMax);

                for (int i = 0; i < values.Length; i++)
                {
                    int l = (int)Math.Max(0, (long)i - left);
                    int r = (int)Math.Min(values.Length - 1, (long)i + right);
                    int expectedMin = int.MaxValue;
                    int expectedMax = int.MinValue;
                    for (int j = l; j <= r; j++)
                    {
                        expectedMin = Math.Min(expectedMin, values[j]);
                        expectedMax = Math.Max(expectedMax, values[j]);
                    }
                    Assert.AreEqual(expectedMin, actualMin[i]);
                    Assert.AreEqual(expectedMax, actualMax[i]);
                }
            }

            // Empty input succeeds with empty destination.
            OptimalFinding.SlidingWindowMin<int>(ReadOnlySpan<int>.Empty, 2, 2, Span<int>.Empty);

            Assert.Throws<ArgumentOutOfRangeException>(() => OptimalFinding.SlidingWindowMin<int>(values, -1, 1, actualMin));
            Assert.Throws<ArgumentOutOfRangeException>(() => OptimalFinding.SlidingWindowMax<int>(values, 1, -1, actualMax));
            Assert.Throws<ArgumentException>(() => OptimalFinding.SlidingWindowMin<int>(values, 1, 1, new int[values.Length - 1]));
        }

        [Test]
        public void LongestIncreasingSubsequenceTests()
        {
            Assert.AreEqual(0, OptimalFinding.LongestIncreasingSubsequenceLength<int>(ReadOnlySpan<int>.Empty));
            Assert.AreEqual(0, OptimalFinding.LongestIncreasingSubsequenceLengths<int>(ReadOnlySpan<int>.Empty, Span<int>.Empty));
            Assert.AreEqual(1, OptimalFinding.LongestIncreasingSubsequenceLength<int>([7]));

            int[] seq = [3, 1, 2, 2, 4, 3, 5];
            int[] strictEnds = new int[seq.Length];
            int[] weakEnds = new int[seq.Length];

            Assert.AreEqual(4, OptimalFinding.LongestIncreasingSubsequenceLength<int>(seq, strictlyIncreasing: true));
            Assert.AreEqual(5, OptimalFinding.LongestIncreasingSubsequenceLength<int>(seq, strictlyIncreasing: false));
            Assert.AreEqual(4, OptimalFinding.LongestIncreasingSubsequenceLengths<int>(seq, strictEnds, strictlyIncreasing: true));
            Assert.AreEqual(5, OptimalFinding.LongestIncreasingSubsequenceLengths<int>(seq, weakEnds, strictlyIncreasing: false));

            CollectionAssert.AreEqual(new[] { 1, 1, 2, 2, 3, 3, 4 }, strictEnds);
            CollectionAssert.AreEqual(new[] { 1, 1, 2, 3, 4, 4, 5 }, weakEnds);

            // Custom descending comparer checks longest decreasing subsequence.
            Comparer<int> desc = Comparer<int>.Create((a, b) => b.CompareTo(a));
            int[] decEnds = new int[seq.Length];
            Assert.AreEqual(3, OptimalFinding.LongestIncreasingSubsequenceLengths<int>(seq, decEnds, strictlyIncreasing: false, desc));
            CollectionAssert.AreEqual(new[] { 1, 2, 2, 3, 1, 2, 1 }, decEnds);

            Assert.Throws<ArgumentException>(() =>
                OptimalFinding.LongestIncreasingSubsequenceLengths<int>(seq, new int[seq.Length - 1]));
        }
    }
}
