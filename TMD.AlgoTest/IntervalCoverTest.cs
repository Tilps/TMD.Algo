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

using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class IntervalCoverTest
    {
        [Test]
        public void TestEmptyTargetRange()
        {
            var intervals = new List<(int S, int E)> { (1, 5) };
            var result = IntervalCover.FindMinimumCover(5, 4, intervals, x => x.S, x => x.E);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result!.Count);
            Assert.AreEqual(0, IntervalCover.MinIntervalsToCover(5, 4, intervals, x => x.S, x => x.E));
        }

        [Test]
        public void TestSingleIntervalExact()
        {
            var intervals = new List<(int S, int E)> { (1, 10) };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals, x => x.S, x => x.E);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count);
            Assert.AreEqual((1, 10), result![0]);
        }

        [Test]
        public void TestImpossibleGap()
        {
            var intervals = new List<(int S, int E)>
            {
                (1, 4),
                (6, 10)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals, x => x.S, x => x.E);
            Assert.IsNull(result);
            Assert.AreEqual(-1, IntervalCover.MinIntervalsToCover(1, 10, intervals, x => x.S, x => x.E));
        }

        [Test]
        public void TestImpossibleStartMissing()
        {
            var intervals = new List<(int S, int E)>
            {
                (2, 10)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals, x => x.S, x => x.E);
            Assert.IsNull(result);
        }

        [Test]
        public void TestImpossibleEndMissing()
        {
            var intervals = new List<(int S, int E)>
            {
                (1, 9)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals, x => x.S, x => x.E);
            Assert.IsNull(result);
        }

        [Test]
        public void TestGreedySelection()
        {
            // Intervals: [1, 3], [1, 5], [2, 6], [4, 10], [5, 8], [7, 10]
            // Optimal cover of [1, 10]: [1, 5] and [4, 10] (2 intervals)
            var intervals = new List<(int S, int E)>
            {
                (1, 3),
                (1, 5),
                (2, 6),
                (5, 8),
                (4, 10),
                (7, 10)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals, x => x.S, x => x.E);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual((1, 5), result![0]);
            Assert.AreEqual((4, 10), result![1]);
            Assert.AreEqual(2, IntervalCover.MinIntervalsToCover(1, 10, intervals, x => x.S, x => x.E));
        }

        [Test]
        public void TestDiscreteAdjacency()
        {
            // For discrete intervals: [1, 5] and [6, 10] can cover [1, 10] because 6 == 5 + 1
            var intervals = new List<(int S, int E)>
            {
                (1, 5),
                (6, 10)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals, x => x.S, x => x.E);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual(2, IntervalCover.MinIntervalsToCover(1, 10, intervals, x => x.S, x => x.E));
        }

        [Test]
        public void TestTupleOverloads()
        {
            var intervals = new List<(int Start, int End)>
            {
                (1, 4),
                (3, 8),
                (7, 10)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result!.Count);
            Assert.AreEqual(3, IntervalCover.MinIntervalsToCover(1, 10, intervals));
        }

        [Test]
        public void TestKeyValuePairOverloads()
        {
            var intervals = new List<KeyValuePair<int, int>>
            {
                new KeyValuePair<int, int>(1, 6),
                new KeyValuePair<int, int>(5, 10)
            };
            var result = IntervalCover.FindMinimumCover(1, 10, intervals);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual(2, IntervalCover.MinIntervalsToCover(1, 10, intervals));
        }
    }
}

