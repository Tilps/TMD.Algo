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
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class MaxFlowTest
    {
        [Test]
        public void SimpleFlowNetwork()
        {
            // S=0, A=1, B=2, T=3
            var mf = new MaxFlow(4);
            mf.AddDirectedEdge(0, 1, 10);
            mf.AddDirectedEdge(0, 2, 10);
            mf.AddDirectedEdge(1, 2, 2);
            mf.AddDirectedEdge(1, 3, 4);
            mf.AddDirectedEdge(2, 3, 9);

            int flow = mf.ComputeMaxFlow(0, 3);
            Assert.That(flow, Is.EqualTo(13));

            bool[] minCut = mf.GetMinCut(0);
            Assert.That(minCut[0], Is.True);
            Assert.That(minCut[3], Is.False);
        }

        [Test]
        public void BottleneckFlow()
        {
            var mf = new MaxFlow(3);
            mf.AddDirectedEdge(0, 1, 100);
            mf.AddDirectedEdge(1, 2, 5);

            int flow = mf.ComputeMaxFlow(0, 2);
            Assert.That(flow, Is.EqualTo(5));

            bool[] minCut = mf.GetMinCut(0);
            Assert.That(minCut[0], Is.True);
            Assert.That(minCut[1], Is.True);
            Assert.That(minCut[2], Is.False);
        }

        [Test]
        public void DisconnectedGraphReturnsZero()
        {
            var mf = new MaxFlow(4);
            mf.AddDirectedEdge(0, 1, 10);
            mf.AddDirectedEdge(2, 3, 10);

            int flow = mf.ComputeMaxFlow(0, 3);
            Assert.That(flow, Is.EqualTo(0));

            bool[] minCut = mf.GetMinCut(0);
            Assert.That(minCut[0], Is.True);
            Assert.That(minCut[1], Is.True);
            Assert.That(minCut[2], Is.False);
            Assert.That(minCut[3], Is.False);
        }

        [Test]
        public void UndirectedEdges()
        {
            var mf = new MaxFlow(4);
            mf.AddUndirectedEdge(0, 1, 5);
            mf.AddUndirectedEdge(1, 2, 3);
            mf.AddUndirectedEdge(2, 3, 5);

            int flow = mf.ComputeMaxFlow(0, 3);
            Assert.That(flow, Is.EqualTo(3));
        }

        [Test]
        public void GraphDinicIntegration()
        {
            var g = new Graph<string, int>();
            int s = g.AddVertex("S");
            int a = g.AddVertex("A");
            int b = g.AddVertex("B");
            int t = g.AddVertex("T");

            g.AddDirectedEdge(s, a, 10);
            g.AddDirectedEdge(s, b, 5);
            g.AddDirectedEdge(a, t, 7);
            g.AddDirectedEdge(b, t, 8);
            g.AddDirectedEdge(a, b, 4);

            int flow = g.MaximumFlowDinic(s, t, cap => cap);
            Assert.That(flow, Is.EqualTo(15));

            int flowDefault = g.MaximumFlowDinic(s, t);
            Assert.That(flowDefault, Is.EqualTo(15));
        }

        [Test]
        public void GraphGetVertexAndEdge()
        {
            var g = new Graph<string, int>();
            int v0 = g.AddVertex("Source");
            int v1 = g.AddVertex("Target");
            int e0 = g.AddDirectedEdge(v0, v1, 42);

            Assert.That(g.VertexCount, Is.EqualTo(2));
            Assert.That(g.EdgeCount, Is.EqualTo(1));
            Assert.That(g.GetVertex(v0), Is.EqualTo("Source"));
            Assert.That(g.GetVertex(v1), Is.EqualTo("Target"));
            Assert.That(g.GetEdge(e0), Is.EqualTo(42));
        }

        [Test]
        public void InvalidArgumentsThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MaxFlow(0));
            var mf = new MaxFlow(3);
            Assert.Throws<ArgumentOutOfRangeException>(() => mf.AddDirectedEdge(-1, 0, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => mf.AddDirectedEdge(0, 3, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => mf.AddDirectedEdge(0, 1, -1));
            Assert.Throws<ArgumentException>(() => mf.ComputeMaxFlow(0, 0));
        }
    }
}

