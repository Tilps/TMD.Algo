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
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class BipartiteMatchingTest
    {
        [Test]
        public void EmptyGraph_ReturnsZero()
        {
            var result = BipartiteMatching.FindMaximumMatching(0, 0, new List<int>[0]);
            Assert.AreEqual(0, result.Count);
            Assert.IsTrue(result.CanMatchAllLeft);
            Assert.IsEmpty(result.Edges);

            var result2 = BipartiteMatching.FindMaximumMatching(3, 0, u => Enumerable.Empty<int>());
            Assert.AreEqual(0, result2.Count);
            Assert.IsFalse(result2.CanMatchAllLeft);

            var result3 = BipartiteMatching.FindMaximumMatching(0, 3, new List<int>[0]);
            Assert.AreEqual(0, result3.Count);
            Assert.IsTrue(result3.CanMatchAllLeft);
        }

        [Test]
        public void SingleEdge_MatchesCorrectly()
        {
            var adj = new[] { new List<int> { 0 } };
            var result = BipartiteMatching.FindMaximumMatching(1, 1, adj);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result.MatchLeft[0]);
            Assert.AreEqual(0, result.MatchRight[0]);
            Assert.IsTrue(result.CanMatchAllLeft);

            var edges = result.Edges.ToList();
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual((0, 0), edges[0]);
        }

        [Test]
        public void DisconnectedMatching_FindsMaximum()
        {
            // Left 0 -> Right 0
            // Left 1 -> Right 1
            // Left 2 -> Right 0, 1
            // Left 3 has no edges
            // Right 2 has no edges
            var adj = new[]
            {
                new List<int> { 0 },
                new List<int> { 1 },
                new List<int> { 0, 1 },
                new List<int>()
            };

            var result = BipartiteMatching.FindMaximumMatching(4, 3, adj);
            Assert.AreEqual(2, result.Count);
            Assert.IsFalse(result.CanMatchAllLeft);

            // Verify matching is valid
            for (int u = 0; u < 4; u++)
            {
                if (result.MatchLeft[u] >= 0)
                {
                    int v = result.MatchLeft[u];
                    Assert.AreEqual(u, result.MatchRight[v]);
                    Assert.Contains(v, adj[u]);
                }
            }
        }

        [Test]
        public void CompleteBipartiteGraph_MatchesMinOfSides()
        {
            int leftCount = 4;
            int rightCount = 6;
            var edges = new List<(int, int)>();
            for (int u = 0; u < leftCount; u++)
            for (int v = 0; v < rightCount; v++)
                edges.Add((u, v));

            var result = BipartiteMatching.FindMaximumMatching(leftCount, rightCount, edges);
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.CanMatchAllLeft);

            // Every left vertex must be matched to a distinct right vertex
            var matchedRights = new HashSet<int>();
            for (int u = 0; u < leftCount; u++)
            {
                Assert.GreaterOrEqual(result.MatchLeft[u], 0);
                Assert.IsTrue(matchedRights.Add(result.MatchLeft[u]));
            }
        }

        [Test]
        public void CrownGraph_MatchesAll()
        {
            // Crown graph with 5 vertices on each side (all edges except u == v)
            int n = 5;
            var result = BipartiteMatching.FindMaximumMatching(n, n, u =>
            {
                var neighbors = new List<int>();
                for (int v = 0; v < n; v++)
                {
                    if (u != v) neighbors.Add(v);
                }
                return neighbors;
            });

            Assert.AreEqual(n, result.Count);
            Assert.IsTrue(result.CanMatchAllLeft);
        }

        [Test]
        public void CanMatchAllLeft_AdjacencyList()
        {
            // Solvable case:
            // 0 -> 0, 1
            // 1 -> 1, 2
            // 2 -> 2
            var solvable = new[]
            {
                new List<int> { 0, 1 },
                new List<int> { 1, 2 },
                new List<int> { 2 }
            };
            Assert.IsTrue(BipartiteMatching.CanMatchAllLeft(3, 3, solvable));

            // Unsolvable case (Hall's condition violated: 2 vertices share only 1 neighbor)
            var unsolvable = new[]
            {
                new List<int> { 0 },
                new List<int> { 0 },
                new List<int> { 1, 2 }
            };
            Assert.IsFalse(BipartiteMatching.CanMatchAllLeft(3, 3, unsolvable));
        }

        [Test]
        public void CanMatchAllLeft_Predicate()
        {
            // Left nodes must be <= right nodes
            // Left: {0, 1, 2}, Right: {0, 1, 2}
            Assert.IsTrue(BipartiteMatching.CanMatchAllLeft(3, 3, (u, v) => u <= v));

            // Left: {0, 1, 2, 3}, Right: {0, 1}
            Assert.IsFalse(BipartiteMatching.CanMatchAllLeft(4, 2, (u, v) => true));
        }

        [Test]
        public void CanMatchAllLeft_LeftLargerThanRight_ReturnsFalseImmediately()
        {
            Assert.IsFalse(BipartiteMatching.CanMatchAllLeft(5, 4, (u, v) => true));
        }

        [Test]
        public void CanMatchAllLeft_EmptyLeft_ReturnsTrue()
        {
            Assert.IsTrue(BipartiteMatching.CanMatchAllLeft(0, 5, (u, v) => true));
        }
    }
}

