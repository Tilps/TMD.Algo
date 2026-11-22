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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Computes maximum flow and minimum cut on directed and undirected flow networks using Dinic's algorithm.
    /// Runs in O(V^2 * E) on general networks and O(E * sqrt(V)) on unit networks and bipartite graphs.
    /// </summary>
    public class MaxFlow
    {
        /// <summary>
        /// Represents a directed edge in the residual flow network.
        /// </summary>
        public class Edge
        {
            /// <summary>
            /// Gets the source node index.
            /// </summary>
            public int From { get; }

            /// <summary>
            /// Gets the target node index.
            /// </summary>
            public int To { get; }

            /// <summary>
            /// Gets the total capacity of the edge.
            /// </summary>
            public int Capacity { get; }

            /// <summary>
            /// Gets the current flow along the edge.
            /// </summary>
            public int Flow { get; internal set; }

            /// <summary>
            /// Gets the index of the reverse edge in the target node's adjacency list.
            /// </summary>
            internal int ReverseIndex { get; }

            internal Edge(int from, int to, int capacity, int reverseIndex)
            {
                From = from;
                To = to;
                Capacity = capacity;
                Flow = 0;
                ReverseIndex = reverseIndex;
            }
        }

        private int nodeCount;
        private readonly List<List<Edge>> graph;
        private int[] level;
        private int[] nextEdgePtr;
        private readonly Queue<int> bfsQueue = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxFlow"/> class with the specified number of nodes.
        /// </summary>
        /// <param name="nodeCount">The number of nodes in the flow network.</param>
        public MaxFlow(int nodeCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nodeCount);

            this.nodeCount = nodeCount;
            graph = new List<List<Edge>>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
                graph.Add([]);

            level = new int[nodeCount];
            nextEdgePtr = new int[nodeCount];
        }

        /// <summary>
        /// Gets the total number of nodes in the flow network.
        /// </summary>
        public int NodeCount => nodeCount;

        /// <summary>
        /// Adds a single new node to the flow network while preserving all existing nodes, edges, and residual flows.
        /// </summary>
        /// <returns>The index of the newly added node.</returns>
        public int AddNode()
        {
            int id = nodeCount;
            nodeCount++;
            graph.Add([]);
            return id;
        }

        /// <summary>
        /// Adds <paramref name="count"/> new nodes to the flow network while preserving all existing nodes, edges, and residual flows.
        /// </summary>
        /// <param name="count">Number of nodes to add (must be non-negative).</param>
        /// <returns>The index of the first newly added node (or current <see cref="NodeCount"/> if <paramref name="count"/> is zero).</returns>
        public int AddNodes(int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            int firstId = nodeCount;
            if (count == 0)
                return firstId;

            graph.EnsureCapacity(nodeCount + count);
            for (int i = 0; i < count; i++)
                graph.Add([]);
            nodeCount += count;
            return firstId;
        }

        /// <summary>
        /// Adds a directed edge from <paramref name="from"/> to <paramref name="to"/> with the specified capacity.
        /// </summary>
        /// <param name="from">The source node index.</param>
        /// <param name="to">The destination node index.</param>
        /// <param name="capacity">The edge capacity, which must be non-negative.</param>
        public void AddDirectedEdge(int from, int to, int capacity)
        {
            ValidateNode(from, nameof(from));
            ValidateNode(to, nameof(to));
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            int forwardIndex = graph[from].Count;
            int reverseIndex = graph[to].Count;

            Edge forward = new Edge(from, to, capacity, reverseIndex);
            Edge backward = new Edge(to, from, 0, forwardIndex);

            graph[from].Add(forward);
            graph[to].Add(backward);
        }

        /// <summary>
        /// Adds an undirected edge between <paramref name="u"/> and <paramref name="v"/> with the specified capacity in both directions.
        /// </summary>
        /// <param name="u">The first node index.</param>
        /// <param name="v">The second node index.</param>
        /// <param name="capacity">The edge capacity, which must be non-negative.</param>
        public void AddUndirectedEdge(int u, int v, int capacity)
        {
            ValidateNode(u, nameof(u));
            ValidateNode(v, nameof(v));
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            int forwardIndex = graph[u].Count;
            int reverseIndex = graph[v].Count;

            Edge forward = new Edge(u, v, capacity, reverseIndex);
            Edge backward = new Edge(v, u, capacity, forwardIndex);

            graph[u].Add(forward);
            graph[v].Add(backward);
        }

        /// <summary>
        /// Computes the maximum flow from <paramref name="source"/> to <paramref name="sink"/> using Dinic's algorithm.
        /// If called repeatedly after adding nodes or edges, augments along residual paths incrementally.
        /// </summary>
        /// <param name="source">The source node index.</param>
        /// <param name="sink">The sink node index.</param>
        /// <returns>The additional flow pushed during this call.</returns>
        public int ComputeMaxFlow(int source, int sink)
        {
            return ComputeMaxFlow(source, sink, int.MaxValue);
        }

        /// <summary>
        /// Computes up to <paramref name="flowLimit"/> units of additional flow from <paramref name="source"/> to
        /// <paramref name="sink"/> using Dinic's algorithm while preserving existing residual flow.
        /// </summary>
        /// <param name="source">The source node index.</param>
        /// <param name="sink">The sink node index.</param>
        /// <param name="flowLimit">Maximum amount of flow to push during this call (must be non-negative).</param>
        /// <returns>The amount of flow pushed during this call (at most <paramref name="flowLimit"/>).</returns>
        public int ComputeMaxFlow(int source, int sink, int flowLimit)
        {
            ValidateNode(source, nameof(source));
            ValidateNode(sink, nameof(sink));
            if (source == sink)
                throw new ArgumentException("Source and sink nodes must be distinct.", nameof(sink));
            ArgumentOutOfRangeException.ThrowIfNegative(flowLimit);

            if (flowLimit == 0)
                return 0;

            EnsureScratchCapacity();
            int totalFlow = 0;
            while (totalFlow < flowLimit && Bfs(source, sink))
            {
                Array.Clear(nextEdgePtr, 0, nodeCount);
                while (totalFlow < flowLimit)
                {
                    int pushed = Dfs(source, sink, flowLimit - totalFlow);
                    if (pushed == 0)
                        break;
                    totalFlow += pushed;
                }
            }
            return totalFlow;
        }

        /// <summary>
        /// Determines the minimum cut partition reachable from the source in the residual graph.
        /// Must be called after <see cref="ComputeMaxFlow(int, int)"/>.
        /// </summary>
        /// <param name="source">The source node index.</param>
        /// <returns>A boolean array where true indicates the node belongs to the source component of the minimum cut.</returns>
        public bool[] GetMinCut(int source)
        {
            ValidateNode(source, nameof(source));

            bool[] inSource = new bool[nodeCount];
            bfsQueue.Clear();
            inSource[source] = true;
            bfsQueue.Enqueue(source);

            while (bfsQueue.Count > 0)
            {
                int u = bfsQueue.Dequeue();
                foreach (Edge edge in graph[u])
                {
                    if (edge.Capacity - edge.Flow > 0 && !inSource[edge.To])
                    {
                        inSource[edge.To] = true;
                        bfsQueue.Enqueue(edge.To);
                    }
                }
            }
            return inSource;
        }

        private void EnsureScratchCapacity()
        {
            if (level.Length < nodeCount)
            {
                int newCap = Math.Max(nodeCount, level.Length * 2);
                level = new int[newCap];
                nextEdgePtr = new int[newCap];
            }
        }

        private bool Bfs(int source, int sink)
        {
            Array.Fill(level, -1, 0, nodeCount);
            level[source] = 0;
            bfsQueue.Clear();
            bfsQueue.Enqueue(source);

            while (bfsQueue.Count > 0)
            {
                int u = bfsQueue.Dequeue();
                foreach (Edge edge in graph[u])
                {
                    if (edge.Capacity - edge.Flow > 0 && level[edge.To] < 0)
                    {
                        level[edge.To] = level[u] + 1;
                        bfsQueue.Enqueue(edge.To);
                    }
                }
            }
            return level[sink] >= 0;
        }

        private int Dfs(int u, int sink, int pushed)
        {
            if (pushed == 0 || u == sink)
                return pushed;

            for (; nextEdgePtr[u] < graph[u].Count; nextEdgePtr[u]++)
            {
                Edge edge = graph[u][nextEdgePtr[u]];
                int v = edge.To;
                if (level[u] + 1 != level[v] || edge.Capacity - edge.Flow == 0)
                    continue;

                int trPushed = Dfs(v, sink, Math.Min(pushed, edge.Capacity - edge.Flow));
                if (trPushed == 0)
                    continue;

                edge.Flow += trPushed;
                graph[v][edge.ReverseIndex].Flow -= trPushed;
                return trPushed;
            }
            return 0;
        }

        private void ValidateNode(int node, string paramName)
        {
            if (node < 0 || node >= nodeCount)
                throw new ArgumentOutOfRangeException(paramName, $"Node index {node} is out of range [0, {nodeCount - 1}].");
        }
    }
}

