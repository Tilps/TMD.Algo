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
    public class SearchTest
    {
        [Test]
        public void TestTryPfsSingleSource()
        {
            // Grid shortest path from (0,0) to (2,2) with edge costs: right=1, down=2
            bool reached = Search.TryPfs(
                (0, 0),
                0,
                target => target == (2, 2),
                (state, dist) =>
                {
                    List<KeyValuePair<(int, int), int>> next = new List<KeyValuePair<(int, int), int>>();
                    if (state.Item1 < 2) next.Add(new KeyValuePair<(int, int), int>((state.Item1 + 1, state.Item2), dist + 1));
                    if (state.Item2 < 2) next.Add(new KeyValuePair<(int, int), int>((state.Item1, state.Item2 + 1), dist + 2));
                    return next;
                },
                out int finalDist);

            Assert.IsTrue(reached);
            // 2 steps right (1+1) and 2 steps down (2+2) = 6
            Assert.AreEqual(6, finalDist);
        }

        [Test]
        public void TestTryPfsMultipleSourcesAndTimeDependent()
        {
            // Two starting buses departing at t=2 and t=5
            var starts = new[]
            {
                new KeyValuePair<string, double>("StopA", 2.0),
                new KeyValuePair<string, double>("StopB", 5.0)
            };

            // StopA -> StopC arrives at max(t, 4.0) + 1.0 (earliest departure is 4.0, travel takes 1.0)
            // StopB -> StopC arrives at max(t, 5.0) + 0.5 (earliest departure is 5.0, travel takes 0.5)
            bool reached = Search.TryPfs(
                starts,
                state => state == "StopC",
                (state, t) =>
                {
                    List<KeyValuePair<string, double>> next = new List<KeyValuePair<string, double>>();
                    if (state == "StopA")
                        next.Add(new KeyValuePair<string, double>("StopC", System.Math.Max(t, 4.0) + 1.0));
                    if (state == "StopB")
                        next.Add(new KeyValuePair<string, double>("StopC", System.Math.Max(t, 5.0) + 0.5));
                    return next;
                },
                out double arrival);

            Assert.IsTrue(reached);
            // From StopA: max(2, 4) + 1 = 5.0
            // From StopB: max(5, 5) + 0.5 = 5.5
            Assert.AreEqual(5.0, arrival);
        }

        [Test]
        public void TestTryPfsUnreachable()
        {
            bool reached = Search.TryPfs(
                0,
                0,
                state => state == 10,
                (state, dist) => new[] { new KeyValuePair<int, int>(state + 1, dist + 1) },
                out int result);

            // Reaches 10
            Assert.IsTrue(reached);
            Assert.AreEqual(10, result);

            bool unreachable = Search.TryPfs(
                0,
                0,
                state => state < 0,
                (state, dist) => state > 5 ? System.Array.Empty<KeyValuePair<int, int>>() : new[] { new KeyValuePair<int, int>(state + 1, dist + 1) },
                out int noResult);

            Assert.IsFalse(unreachable);
            Assert.AreEqual(0, noResult);
        }
    }
}

