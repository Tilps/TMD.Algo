#region License
/*
Copyright (c) 2008, the TMD.Algo authors.
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
using System.Linq;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class GraphTest
    {

        [Test]
        public void Basic()
        {
            int[,] dists = new int[50, 50];
            Random rnd = new Random();
            for (int i = 0; i <= 100; i++)
            {
                int a = rnd.Next(50);
                int b = rnd.Next(50);
                if (a == b)
                    continue;
                dists[a, b] = rnd.Next(1000);
            }
            Graph<int, int> g = new Graph<int, int>();
            for (int i = 0; i < 50; i++)
            {
                g.AddVertex(i);
            }
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    if (dists[i,j] > 0)
                        g.AddDirectedEdge(i, j, dists[i, j]);
                }
            }
            bool success;
            Graph<int, int>.ShortestPathInfo spi = g.ShortestPathFloydWarshal(Comparer<int>.Default, new IntAdder(), out success);
            Assert.IsTrue(success);
            for (int i = 0; i < 50; i++)
            {
                Graph<int, int>.SingleShortestPathInfo sspi1 = g.ShortestPathBellmanFord(i, Comparer<int>.Default, new IntAdder(), out success);
                Assert.IsTrue(success);
                Graph<int, int>.SingleShortestPathInfo sspi2 = g.ShortestPathDijkstra(i, Comparer<int>.Default, new IntAdder());
                Graph<int, int>.SingleShortestPathInfo sspi3 = g.ShortestPathModifiedDijkstra(i, Comparer<int>.Default, new IntAdder());
                for (int j = 0; j < 50; j++)
                {
                    Assert.AreEqual(spi.GetDistance(i, j), sspi1.GetDistance(j));
                    Assert.AreEqual(spi.GetDistance(i, j), sspi2.GetDistance(j));
                    Assert.AreEqual(spi.GetDistance(i, j), sspi3.GetDistance(j));
                }
            }

        }


        [Test]
        public void Flow()
        {
            Graph<int, int> g = new Graph<int, int>();
            g.AddVertex(0);
            g.AddVertex(1);
            g.AddVertex(2);
            g.AddVertex(3);
            g.AddDirectedEdge(0, 1, 1000000);
            g.AddDirectedEdge(1, 3, 1000000);
            g.AddDirectedEdge(1, 2, 1);
            g.AddDirectedEdge(0, 2, 1000000);
            g.AddDirectedEdge(2, 3, 1000000);
            Assert.AreEqual(2000000, g.MaximumFlowFordFulkersonBfs(0, 3, Comparer<int>.Default, new IntAdder()));
        }
        [Test]
        public void Flow2()
        {
            Graph<int, int> g = new Graph<int, int>();
            g.AddVertex(0);
            g.AddVertex(1);
            g.AddVertex(2);
            g.AddVertex(3);
            g.AddDirectedEdge(0, 1, 1000000);
            g.AddDirectedEdge(1, 3, 999999);
            g.AddDirectedEdge(1, 2, 1);
            g.AddDirectedEdge(0, 2, 1);
            g.AddDirectedEdge(2, 3, 1000000);
            Assert.AreEqual(1000001, g.MaximumFlowFordFulkersonBfs(0, 3, Comparer<int>.Default, new IntAdder()));
        }

        [Test]
        public void Matching()
        {
            Graph<int, int> g = new Graph<int, int>();
            g.AddVertex(0);
            g.AddVertex(1);
            g.AddVertex(2);
            g.AddVertex(3);
            g.AddUndirectedEdge(0, 2, 1);
            g.AddUndirectedEdge(1, 2, 1);
            Assert.AreEqual(1, g.MaximumBipartiteMatching());
            g.AddUndirectedEdge(0, 3, 1);
            Assert.AreEqual(2, g.MaximumBipartiteMatching());
        }

        private class Pair<K, V>
        {
            public Pair(K k, V v)
            {
                Key = k;
                Value = v;
            }
            public K Key;
            public V Value;
        }

        [Test]
        public void MaxFlowMinCost()
        {
            Graph<int, Pair<int, int>> g = new Graph<int, Pair<int, int>>();
            g.AddVertex(0);
            g.AddVertex(1);
            g.AddVertex(2);
            g.AddVertex(3);
            g.AddDirectedEdge(0, 1, new Pair<int, int>(1000000, 1));
            g.AddDirectedEdge(1, 3, new Pair<int, int>(1000000, 2));
            g.AddDirectedEdge(1, 2, new Pair<int, int>(1, 5));
            g.AddDirectedEdge(0, 2, new Pair<int, int>(1000000, 3));
            g.AddDirectedEdge(2, 3, new Pair<int, int>(1000000, 4));
            int minCost;
            bool successful;
            int result = g.MaximumFlowMinCost(0, 3,
                Comparer<int>.Default, new IntAdder(),
                Comparer<int>.Default, new IntAdder(),
                delegate(int cost, int flow) { return cost * flow; },
                delegate(Pair<int, int> kvp) { return kvp.Key; },
                delegate(Pair<int, int> kvp) { return kvp.Value; },
                delegate(Pair<int, int> p, int cap) { p.Key = cap; },
                delegate(Pair<int, int> p, int cap) { p.Value = cap; },
                delegate() { return new Pair<int, int>(0, 0); },
                out minCost,
                out successful);
            Assert.AreEqual(2000000, result);
            Assert.AreEqual(10000000, minCost);
        }
        [Test]
        public void MaxFlowMinCost2()
        {
            Graph<int, Pair<int, int>> g = new Graph<int, Pair<int, int>>();
            g.AddVertex(0);
            g.AddVertex(1);
            g.AddVertex(2);
            g.AddVertex(3);
            g.AddDirectedEdge(0, 1, new Pair<int, int>(1000000, 1));
            g.AddDirectedEdge(1, 3, new Pair<int, int>(999999, 2));
            g.AddDirectedEdge(1, 2, new Pair<int, int>(1, 5));
            g.AddDirectedEdge(0, 2, new Pair<int, int>(1, 3));
            g.AddDirectedEdge(2, 3, new Pair<int, int>(1000000, 4));
            int minCost;
            bool successful;
            int result = g.MaximumFlowMinCost(0, 3,
                Comparer<int>.Default, new IntAdder(),
                Comparer<int>.Default, new IntAdder(),
                delegate(int cost, int flow) { return cost * flow; },
                delegate(Pair<int, int> kvp) { return kvp.Key; },
                delegate(Pair<int, int> kvp) { return kvp.Value; },
                delegate(Pair<int, int> p, int cap) { p.Key = cap; },
                delegate(Pair<int, int> p, int cap) { p.Value = cap; },
                delegate() { return new Pair<int, int>(0, 0); },
                out minCost,
                out successful);
            Assert.AreEqual(1000001, result);
            Assert.AreEqual(1000000 + 999999 * 2 + 5 + 3 + 2 * 4, minCost);
        }
        [Test]
        public void MaxFlowMinCost3()
        {
            Graph<int, Pair<int, int>> g = new Graph<int, Pair<int, int>>();
            g.AddVertex(0);
            g.AddVertex(1);
            g.AddVertex(2);
            g.AddVertex(3);
            g.AddDirectedEdge(0, 1, new Pair<int, int>(1000000, 2));
            g.AddDirectedEdge(1, 3, new Pair<int, int>(1000000, 4));
            g.AddDirectedEdge(1, 2, new Pair<int, int>(1000000, 1));
            g.AddDirectedEdge(0, 2, new Pair<int, int>(1, 3));
            g.AddDirectedEdge(2, 3, new Pair<int, int>(1000000, 1));
            int minCost;
            bool successful;
            int result = g.MaximumFlowMinCost(0, 3,
                Comparer<int>.Default, new IntAdder(),
                Comparer<int>.Default, new IntAdder(),
                delegate(int cost, int flow) { return cost * flow; },
                delegate(Pair<int, int> kvp) { return kvp.Key; },
                delegate(Pair<int, int> kvp) { return kvp.Value; },
                delegate(Pair<int, int> p, int cap) { p.Key = cap; },
                delegate(Pair<int, int> p, int cap) { p.Value = cap; },
                delegate() { return new Pair<int, int>(0, 0); },
                out minCost,
                out successful);
            Assert.AreEqual(1000001, result);
            Assert.AreEqual(1000000*2 + 4 + 999999*1 + 3 + 1000000, minCost);
        }
    }
}
