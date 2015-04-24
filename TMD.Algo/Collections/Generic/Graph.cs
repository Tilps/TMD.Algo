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
using System.Diagnostics.CodeAnalysis;
using TMD.Algo.Algorithms.Generic;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Represents a graph of vertexes and edges.
    /// </summary>
    /// <typeparam name="TVertex">
    /// Type of data associated with the vertexes.
    /// </typeparam>
    /// <typeparam name="TEdge">
    /// Type of data associated with the edges.
    /// </typeparam>
    public class Graph<TVertex, TEdge>
    {

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">
        /// Vertex to add to the graph.
        /// </param>
        /// <returns>
        /// The vertex id assigned to the vertex.
        /// </returns>
        public int AddVertex(TVertex vertex)
        {
            vertices.Add(vertex);
            adjacencyList.Add(new List<KeyValuePair<int, int>>());
            return vertices.Count - 1;
        }

        /// <summary>
        /// Adds an undirected edge to the graph.
        /// </summary>
        /// <param name="firstVertexId">
        /// Id of the vertex the edge starts at.
        /// </param>
        /// <param name="secondVertexId">
        /// Id of the vertex the edge ends at.
        /// </param>
        /// <param name="edge">
        /// Data associated with the edge.
        /// </param>
        /// <returns>
        /// The edge id assigned to the edge.
        /// </returns>
        public int AddUndirectedEdge(int firstVertexId, int secondVertexId, TEdge edge)
        {
            edges.Add(edge);
            adjacencyList[firstVertexId].Add(new KeyValuePair<int, int>(secondVertexId, edges.Count - 1));
            adjacencyList[secondVertexId].Add(new KeyValuePair<int, int>(firstVertexId, edges.Count - 1));
            return edges.Count - 1;
        }

        /// <summary>
        /// Adds an directed edge to the graph.
        /// </summary>
        /// <param name="firstVertexId">
        /// Id of the vertex the edge starts at.
        /// </param>
        /// <param name="secondVertexId">
        /// Id of the vertex the edge ends at.
        /// </param>
        /// <param name="edge">
        /// Data associated with the edge.
        /// </param>
        /// <returns>
        /// The edge id assigned to the edge.
        /// </returns>
        public int AddDirectedEdge(int firstVertexId, int secondVertexId, TEdge edge)
        {
            edges.Add(edge);
            adjacencyList[firstVertexId].Add(new KeyValuePair<int, int>(secondVertexId, edges.Count - 1));
            return edges.Count - 1;
        }

        /// <summary>
        /// Performs a breadth first search of the graph starting from a specific location.
        /// </summary>
        /// <param name="startingVertexId">
        /// Location to start searching from.
        /// </param>
        /// <param name="visitor">
        /// Visitor function to execute at each node found.
        /// </param>
        public void BreadthFirstSearch(int startingVertexId, BfsSearchVisitor visitor)
        {
            int[] searchState = new int[vertices.Count];
            int[] depth = new int[vertices.Count];
            int[] prev = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                depth[i] = int.MaxValue;
                prev[i] = -1;
            }
            Queue<int> searchQueue = new Queue<int>();
            searchQueue.Enqueue(startingVertexId);
            depth[startingVertexId] = 0;
            searchState[startingVertexId] = 1;
            while (searchQueue.Count > 0)
            {
                int next = searchQueue.Dequeue();
                if (visitor(next, prev[next], depth[next]))
                    return;
                foreach (KeyValuePair<int, int> edge in adjacencyList[next])
                {
                    if (searchState[edge.Key] == 0)
                    {
                        searchState[edge.Key] = 1;
                        depth[edge.Key] = depth[next] + 1;
                        prev[edge.Key] = next;
                        searchQueue.Enqueue(edge.Key);
                    }
                }
                searchState[next] = 2;
            }
        }

        /// <summary>
        /// Performs a depth first search of the graph starting from a specific location.
        /// </summary>
        /// <param name="startingVertexId">
        /// Location to start searching from.
        /// </param>
        /// <param name="visitor">
        /// Visitor function to execute at each node found.
        /// </param>
        public void DepthFirstSearch(int startingVertexId, DfsSearchVisitor visitor)
        {
            int[] searchState = new int[vertices.Count];
            DFSVisit(startingVertexId, visitor, searchState, -1, 0);
        }

        /// <summary>
        /// Performs a depth first search of the graph as a whole.
        /// </summary>
        /// <param name="visitor">
        /// Visitor function to execute at each node found.
        /// </param>
        public void DepthFirstSearch(DfsSearchVisitor visitor)
        {
            int[] searchState = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                if (searchState[i] == 0)
                    DFSVisit(i, visitor, searchState, -1, 0);
            }
        }

        private bool DFSVisit(int startingVertexId, DfsSearchVisitor visitor, int[] searchState, int prev, int depth)
        {
            if (visitor(startingVertexId, prev, depth, false))
                return true;
            searchState[startingVertexId] = 1;
            foreach (KeyValuePair<int, int> edge in adjacencyList[startingVertexId])
            {
                if (searchState[edge.Key] == 0)
                {
                    if (DFSVisit(edge.Key, visitor, searchState, startingVertexId, depth + 1))
                        return true;
                }
            }
            searchState[startingVertexId] = 2;
            if (visitor(startingVertexId, prev, depth, true))
                return true;
            return false;
        }

        /// <summary>
        /// Returns the list of vertices in topological order.
        /// Running time O(V+E).
        /// </summary>
        /// <returns>
        /// The list of verticies in topological order.
        /// </returns>
        public List<int> TopologicalSort()
        {
            List<int> res = new List<int>();
            DepthFirstSearch(delegate(int nodeId, int prevId, int depth, bool postOrder)
            {
                if (postOrder)
                {
                    res.Add(nodeId);
                }
                return false;
            });
            res.Reverse();
            return res;
        }

        /// <summary>
        /// Returns the verticies in clumps that are strongly connected.
        /// </summary>
        /// <returns>
        /// Vertex clumps that are strongly connected.
        /// </returns>
        public List<List<int>> GetStronglyConnectedComponents()
        {
            List<int> sorted = TopologicalSort();
            List<List<KeyValuePair<int, int>>> backupAdjacencyList = adjacencyList;
            List<List<int>> res = new List<List<int>>();
            try
            {
                // Transpose self temporarily.
                adjacencyList = GetTransposedList();
                int[] searchState = new int[vertices.Count];
                foreach (int vertex in sorted)
                {
                    if (searchState[vertex] == 0)
                    {
                        List<int> resPart = new List<int>();
                        DFSVisit(vertex, delegate(int nodeId, int prevId, int depth, bool postOrder)
                        {
                            if (postOrder)
                                resPart.Add(nodeId);
                            return false;
                        }, searchState, -1, 0);
                        res.Add(resPart);
                    }
                }
            }
            finally
            {
                // Revert transposition.
                adjacencyList = backupAdjacencyList;
            }
            return res;
        }

        /// <summary>
        /// Determines the minimum combination of edges to cover the vertices.
        /// </summary>
        /// <returns>
        /// The list of edge ids corresponding to the minimum combination which covers the verticies.
        /// </returns>
        public List<int> FindMinimumSpanningTree()
        {
            return FindMinimumSpanningTree(null);
        }

        /// <summary>
        /// Determines the minimum combination of edges to cover the verticies.
        /// </summary>
        /// <param name="edgeComparer">
        /// Comparer to use to sort the edges from lowest weight to highest weight.  Specify null to use the default comparer.
        /// </param>
        /// <returns>
        /// The list of edge ids corresponding to the minimum combination which covers the verticies.
        /// </returns>
        public List<int> FindMinimumSpanningTree(IComparer<TEdge> edgeComparer)
        {
            if (edgeComparer == null)
                edgeComparer = Comparer<TEdge>.Default;
            List<int> res = new List<int>();
            MappedDisjointTracker<int> tracker = new MappedDisjointTracker<int>(i => i, 0, vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
                tracker.Add(i);
            TEdge[] keys = edges.ToArray();
            // TODO: create a small struct for the int tripple instead of abusing key value pair.
            KeyValuePair<int, KeyValuePair<int, int>>[] cons = MakeEdgeList();
            Array.Sort(keys, cons, edgeComparer);
            foreach (KeyValuePair<int, KeyValuePair<int, int>> edge in cons)
            {
                if (tracker.GetRepresentative(edge.Value.Key) != tracker.GetRepresentative(edge.Value.Value))
                {
                    res.Add(edge.Key);
                    tracker.Union(edge.Value.Key, edge.Value.Value);
                }
            }
            return res;
        }

        private KeyValuePair<int, KeyValuePair<int, int>>[] MakeEdgeList()
        {
            KeyValuePair<int, KeyValuePair<int, int>>[] cons = new KeyValuePair<int, KeyValuePair<int, int>>[edges.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < adjacencyList[i].Count; j++)
                {
                    KeyValuePair<int, int> edgeEntry = adjacencyList[i][j];
                    cons[edgeEntry.Value] = new KeyValuePair<int, KeyValuePair<int, int>>(edgeEntry.Value, new KeyValuePair<int, int>(i, edgeEntry.Key));
                }
            }
            return cons;
        }

        private List<TVertex> vertices = new List<TVertex>();
        private List<TEdge> edges = new List<TEdge>();
        private List<List<KeyValuePair<int, int>>> adjacencyList = new List<List<KeyValuePair<int, int>>>();

        [SuppressMessage("Microsoft.Performance", "CA1814")]
        private int[,] GetAdjacencyMatrix()
        {
            int[,] res = new int[vertices.Count, vertices.Count];
            for (int i = 0; i < adjacencyList.Count; i++)
            {
                for (int j = 0; j < vertices.Count; j++)
                    res[i, j] = -1;
                for (int j = 0; j < adjacencyList[i].Count; j++)
                {
                    res[i, adjacencyList[i][j].Key] = adjacencyList[i][j].Value;
                }
            }
            return res;
        }

        private List<List<KeyValuePair<int, int>>> GetTransposedList()
        {
            List<List<KeyValuePair<int, int>>> res = new List<List<KeyValuePair<int, int>>>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                res.Add(new List<KeyValuePair<int, int>>());
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < adjacencyList[i].Count; j++)
                {
                    res[adjacencyList[i][j].Key].Add(new KeyValuePair<int, int>(i, adjacencyList[i][j].Value));
                }
            }
            return res;
        }
        // TODO: implement shortest path.
        /// <summary>
        /// Finds the shortest path info for all nodes from a single source.
        /// Assumes no negative weight edges.
        /// Running time O(V^2)
        /// </summary>
        /// <param name="startVertexId">
        /// Starting vertex.
        /// </param>
        /// <param name="edgeComparer">
        /// Comparer for comparing sums of distances of edges.
        /// </param>
        /// <param name="edgeSummer">
        /// Added for summing edge distances.
        /// </param>
        /// <returns>
        /// A single source shortest path info capable of answering questions about the shortest paths.
        /// </returns>
        public SingleShortestPathInfo ShortestPathDijkstra(int startVertexId, IComparer<TEdge> edgeComparer, IAdder<TEdge> edgeSummer)
        {
            int[] predecesors = new int[vertices.Count];
            TEdge[] distances = new TEdge[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                predecesors[i] = -1;
                distances[i] = edgeSummer.MaxValue;
            }
            distances[startVertexId] = edgeSummer.Zero;
            int count = vertices.Count;
            bool[] seen = new bool[count];

            while (count > 0)
            {
                int found = -1;
                TEdge min = edgeSummer.MaxValue;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (seen[i])
                        continue;
                    if (edgeComparer.Compare(distances[i], min) < 0)
                    {
                        min = distances[i];
                        found = i;
                    }
                }
                // If we haven't found something its because the remaining nodes are all unreachable from the source.
                if (found == -1)
                    break;
                count++;
                seen[found] = true;

                foreach (KeyValuePair<int, int> edge in adjacencyList[found])
                {
                    TEdge sum = edgeSummer.Add(distances[found], edges[edge.Value]);
                    if (edgeComparer.Compare(distances[edge.Key], sum) > 0)
                    {
                        predecesors[edge.Key] = found;
                        distances[edge.Key] = sum;
                    }
                }
            }


            SingleShortestPathInfo results = new SingleShortestPathInfo(predecesors, distances);
            return results;
        }

        /// <summary>
        /// Finds the shortest path info for all nodes from a single source.
        /// Assumes no negative weight edges.
        /// Running time O(E log V)
        /// </summary>
        /// <param name="startVertexId">
        /// Starting vertex.
        /// </param>
        /// <param name="edgeComparer">
        /// Comparer for comparing sums of distances of edges.
        /// </param>
        /// <param name="edgeSummer">
        /// Added for summing edge distances.
        /// </param>
        /// <returns>
        /// A single source shortest path info capable of answering questions about the shortest paths.
        /// </returns>
        public SingleShortestPathInfo ShortestPathModifiedDijkstra(int startVertexId, IComparer<TEdge> edgeComparer, IAdder<TEdge> edgeSummer)
        {
            int[] predecesors = new int[vertices.Count];
            TEdge[] distances = new TEdge[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                predecesors[i] = -1;
                distances[i] = edgeSummer.MaxValue;
            }
            distances[startVertexId] = edgeSummer.Zero;

            LookupHeap<KeyValuePair<TEdge, int>> priorityQueue = new LookupHeap<KeyValuePair<TEdge, int>>(new ReverseComparer<KeyValuePair<TEdge, int>>(new KeyComparer<TEdge, int>(edgeComparer)));
            for (int i = 0; i < vertices.Count; i++)
            {
                priorityQueue.Add(new KeyValuePair<TEdge, int>(distances[i], i));
            }
            while (priorityQueue.Count > 0)
            {
                KeyValuePair<TEdge, int> currentVertex = priorityQueue.PopFront();
                // If distance is max value then all remaining nodes are unreachable from the source.
                if (edgeComparer.Compare(currentVertex.Key, edgeSummer.MaxValue) == 0)
                    break;
                foreach (KeyValuePair<int, int> edge in adjacencyList[currentVertex.Value])
                {
                    TEdge sum = edgeSummer.Add(currentVertex.Key, edges[edge.Value]);
                    if (edgeComparer.Compare(distances[edge.Key], sum) > 0)
                    {
                        predecesors[edge.Key] = currentVertex.Value;
                        // TODO: change to 'update entry' once there is such a method.
                        if (priorityQueue.Remove(new KeyValuePair<TEdge, int>(distances[edge.Key], edge.Key)))
                        {
                            distances[edge.Key] = sum;
                            priorityQueue.Add(new KeyValuePair<TEdge, int>(distances[edge.Key], edge.Key));
                        }
                        else
                        {
                            distances[edge.Key] = sum;
                        }
                    }
                }
            }


            SingleShortestPathInfo results = new SingleShortestPathInfo(predecesors, distances);
            return results;
        }

        /// <summary>
        /// Finds the shortest path info for all nodes from a single source.
        /// Assumes no cycles.
        /// Running time O(E + V)
        /// </summary>
        /// <param name="startVertexId">
        /// Starting vertex.
        /// </param>
        /// <param name="edgeComparer">
        /// Comparer for comparing sums of distances of edges.
        /// </param>
        /// <param name="edgeSummer">
        /// Added for summing edge distances.
        /// </param>
        /// <returns>
        /// A single source shortest path info capable of answering questions about the shortest paths.
        /// </returns>
        public SingleShortestPathInfo ShortestPathAcyclic(int startVertexId, IComparer<TEdge> edgeComparer, IAdder<TEdge> edgeSummer)
        {
            int[] predecesors = new int[vertices.Count];
            TEdge[] distances = new TEdge[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                predecesors[i] = -1;
                distances[i] = edgeSummer.MaxValue;
            }
            distances[startVertexId] = edgeSummer.Zero;

            List<int> sortedVerticies = TopologicalSort();
            foreach (int vertex in sortedVerticies)
            {
                // The first few entries might not be reachable from the source and hence will have maximum distance.
                if (edgeComparer.Compare(distances[vertex], edgeSummer.MaxValue) == 0)
                    continue;
                foreach (KeyValuePair<int, int> edge in adjacencyList[vertex])
                {
                    TEdge sum = edgeSummer.Add(distances[vertex], edges[edge.Value]);
                    if (edgeComparer.Compare(distances[edge.Key], sum) > 0)
                    {
                        predecesors[edge.Key] = vertex;
                        distances[edge.Key] = sum;
                    }
                }
            }


            SingleShortestPathInfo results = new SingleShortestPathInfo(predecesors, distances);
            return results;
        }

        /// <summary>
        /// Finds the shortest path info for all nodes from a single source.
        /// Running time O(VE)
        /// </summary>
        /// <param name="startVertexId">
        /// Starting vertex.
        /// </param>
        /// <param name="edgeComparer">
        /// Comparer for comparing sums of distances of edges.
        /// </param>
        /// <param name="edgeSummer">
        /// Added for summing edge distances.
        /// </param>
        /// <param name="successful">
        /// Outputs true if there are no negative weight cycles.
        /// False otherwie.
        /// </param>
        /// <returns>
        /// A single source shortest path info capable of answering questions about the shortest paths.
        /// </returns>
        public SingleShortestPathInfo ShortestPathBellmanFord(int startVertexId, IComparer<TEdge> edgeComparer, IAdder<TEdge> edgeSummer, out bool successful)
        {
            int[] predecesors = new int[vertices.Count];
            TEdge[] distances = new TEdge[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                predecesors[i] = -1;
                distances[i] = edgeSummer.MaxValue;
            }
            distances[startVertexId] = edgeSummer.Zero;
            KeyValuePair<int, KeyValuePair<int, int>>[] edgeList = MakeEdgeList();

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                for (int j = 0; j < edgeList.Length; j++)
                {
                    int vertex = edgeList[j].Value.Key;
                    if (edgeComparer.Compare(distances[vertex], edgeSummer.MaxValue) == 0)
                        continue;
                    int destVertex = edgeList[j].Value.Value;
                    TEdge sum = edgeSummer.Add(distances[vertex], edges[edgeList[j].Key]);
                    if (edgeComparer.Compare(distances[destVertex], sum) > 0)
                    {
                        predecesors[destVertex] = vertex;
                        distances[destVertex] = sum;
                    }
                }
            }
            successful = true;
            for (int j = 0; j < edgeList.Length; j++)
            {
                int vertex = edgeList[j].Value.Key;
                if (edgeComparer.Compare(distances[vertex], edgeSummer.MaxValue) == 0)
                    continue;
                int destVertex = edgeList[j].Value.Value;
                TEdge sum = edgeSummer.Add(distances[vertex], edges[edgeList[j].Key]);
                if (edgeComparer.Compare(distances[destVertex], sum) > 0)
                {
                    successful = false;
                }
            }

            SingleShortestPathInfo results = new SingleShortestPathInfo(predecesors, distances);
            return results;
        }
        /// <summary>
        /// Represents a provider of information about the shorest path from a single source.
        /// </summary>
        public class SingleShortestPathInfo
        {
            internal SingleShortestPathInfo(int[] predecesors, TEdge[] distances)
            {
                this.predecesors = predecesors;
                this.distances = distances;
            }

            /// <summary>
            /// Returns the shortest distance from the original source to the specified destination.
            /// </summary>
            /// <param name="destVertexId">
            /// Destination vertex.
            /// </param>
            /// <returns>
            /// The shortest distance from the source to the specified destination.
            /// </returns>
            public TEdge GetDistance(int destVertexId)
            {
                return distances[destVertexId];
            }

            /// <summary>
            /// Returns the shortest path from the original source to the specified destination.
            /// </summary>
            /// <param name="destVertexId">
            /// Destination vertex.
            /// </param>
            /// <returns>
            /// The shortest path from the source to the specified destination.
            /// </returns>
            public IList<int> GetPath(int destVertexId)
            {
                List<int> results = new List<int>();
                results.Add(destVertexId);
                int curVertex = destVertexId;
                while (predecesors[curVertex] != -1)
                {
                    curVertex = predecesors[curVertex];
                    results.Add(curVertex);
                }
                results.Reverse();
                return results;
            }
            private int[] predecesors;
            private TEdge[] distances;
        }

        /// <summary>
        /// Finds the shortest path info for all nodes from all sources.
        /// Running time O(V^3)
        /// </summary>
        /// <param name="edgeComparer">
        /// Comparer for comparing sums of distances of edges.
        /// </param>
        /// <param name="edgeSummer">
        /// Added for summing edge distances.
        /// </param>
        /// <param name="successful">
        /// Outputs true if there are no negative weight cycles.
        /// False otherwie.
        /// </param>
        /// <returns>
        /// A single source shortest path info capable of answering questions about the shortest paths.
        /// </returns>
        public ShortestPathInfo ShortestPathFloydWarshal(IComparer<TEdge> edgeComparer, IAdder<TEdge> edgeSummer, out bool successful)
        {
            int[,] predecesors = new int[vertices.Count, vertices.Count];
            TEdge[,] distances = new TEdge[vertices.Count, vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices.Count; j++)
                {
                    predecesors[i, j] = -1;
                    distances[i, j] = edgeSummer.MaxValue;
                }
                distances[i, i] = edgeSummer.Zero;
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                foreach (KeyValuePair<int, int> edge in adjacencyList[i])
                {
                    distances[i, edge.Key] = edges[edge.Value];
                    predecesors[i, edge.Key] = i;
                }
            }
            for (int k = 0; k < vertices.Count; k++)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    for (int j = 0; j < vertices.Count; j++)
                    {
                        if (edgeComparer.Compare(distances[i, k], edgeSummer.MaxValue) == 0)
                            continue;
                        if (edgeComparer.Compare(distances[k, j], edgeSummer.MaxValue) == 0)
                            continue;
                        TEdge sum = edgeSummer.Add(distances[i, k], distances[k, j]);
                        if (edgeComparer.Compare(sum, distances[i, j]) < 0)
                        {
                            distances[i, j] = sum;
                            predecesors[i, j] = predecesors[k, j];
                        }
                    }
                }
            }
            successful = true;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (edgeComparer.Compare(distances[i, i], edgeSummer.Zero) < 0)
                {
                    successful = false;
                    break;
                }
            }

            ShortestPathInfo results = new ShortestPathInfo(predecesors, distances);
            return results;
        }
        /// <summary>
        /// Represents a provider of information about the shorest path from all sources.
        /// </summary>
        public class ShortestPathInfo
        {
            internal ShortestPathInfo(int[,] predecesors, TEdge[,] distances)
            {
                this.predecesors = predecesors;
                this.distances = distances;
            }

            /// <summary>
            /// Returns the shortest distance from the specified source to the specified destination.
            /// </summary>
            /// <param name="srcVertexId">
            /// Source vertex.
            /// </param>
            /// <param name="destVertexId">
            /// Destination vertex.
            /// </param>
            /// <returns>
            /// The shortest distance from the source to the specified destination.
            /// </returns>
            public TEdge GetDistance(int srcVertexId, int destVertexId)
            {
                return distances[srcVertexId, destVertexId];
            }

            /// <summary>
            /// Returns the shortest path from the specified source to the specified destination.
            /// </summary>
            /// <param name="srcVertexId">
            /// Source vertex.
            /// </param>
            /// <param name="destVertexId">
            /// Destination vertex.
            /// </param>
            /// <returns>
            /// The shortest path from the source to the specified destination.
            /// </returns>
            public IList<int> GetPath(int srcVertexId, int destVertexId)
            {
                List<int> results = new List<int>();
                results.Add(destVertexId);
                int curVertex = destVertexId;
                while (predecesors[srcVertexId,curVertex] != -1)
                {
                    curVertex = predecesors[srcVertexId,curVertex];
                    results.Add(curVertex);
                }
                results.Reverse();
                return results;
            }
            private int[,] predecesors;
            private TEdge[,] distances;
        }

        /// <summary>
        /// Calculates the maximum flow for a flow network graph between source and sink.
        /// Edges of the graph are treated as capacities.
        /// </summary>
        /// <param name="source">
        /// Source node where flow comes from.
        /// </param>
        /// <param name="sink">
        /// Sink node where flow goes to.
        /// </param>
        /// <param name="flowComparer">
        /// Comparer for comparing flows.
        /// </param>
        /// <param name="flowSummer">
        /// Adder for summing flows.
        /// </param>
        /// <returns>
        /// The maximum flow if a flow could be found, otherwise flowSummer.Zero.
        /// </returns>
        public TEdge MaximumFlowFordFulkersonBfs(int source, int sink, IComparer<TEdge> flowComparer, IAdder<TEdge> flowSummer)
        {
            Dictionary<KeyValuePair<int, int>, int> edgeLookup = new Dictionary<KeyValuePair<int, int>, int>();
            Graph<TVertex, TEdge> residualGraph = new Graph<TVertex, TEdge>();
            for (int i = 0; i < vertices.Count; i++)
            {
                residualGraph.AddVertex(vertices[i]);
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                foreach (KeyValuePair<int, int> edge in adjacencyList[i])
                {
                    int edgeIndex;
                    KeyValuePair<int, int> edgeEndPoints = new KeyValuePair<int, int>(i, edge.Key);
                    if (!edgeLookup.TryGetValue(edgeEndPoints, out edgeIndex))
                    {
                        edgeIndex = residualGraph.AddDirectedEdge(i, edge.Key, edges[edge.Value]);
                        edgeLookup.Add(edgeEndPoints, edgeIndex);
                    }
                    else
                    {
                        residualGraph.edges[edgeIndex] = flowSummer.Add(residualGraph.edges[edgeIndex], edges[edge.Value]);
                    }
                    int reverseEdgeIndex;
                    KeyValuePair<int, int> reverseEndPoints = new KeyValuePair<int, int>(edge.Key, i);
                    if (!edgeLookup.TryGetValue(reverseEndPoints, out reverseEdgeIndex))
                    {
                        reverseEdgeIndex = residualGraph.AddDirectedEdge(edge.Key, i, flowSummer.Zero);
                        edgeLookup.Add(reverseEndPoints, reverseEdgeIndex);
                    }
                }
            }
            TEdge sum = flowSummer.Zero;

            int[] searchState = new int[vertices.Count];
            int[] comeFrom = new int[vertices.Count];
            Queue<int> vertexQueue = new Queue<int>();
            while (true)
            {
                vertexQueue.Clear();
                Array.Clear(searchState, 0, searchState.Length);
                for (int i = 0; i < vertices.Count; i++)
                    comeFrom[i] = -1;
                bool foundSink = false;
                vertexQueue.Enqueue(source);                
                while (vertexQueue.Count > 0)
                {
                    int vertex = vertexQueue.Dequeue();
                    searchState[vertex] = 1;
                    if (vertex == sink)
                    {
                        foundSink = true;
                        break;
                    }
                    foreach (KeyValuePair<int, int> edge in residualGraph.adjacencyList[vertex])
                    {
                        if (flowComparer.Compare(residualGraph.edges[edge.Value], flowSummer.Zero) <= 0)
                            continue;
                        if (searchState[edge.Key] == 0)
                        {
                            vertexQueue.Enqueue(edge.Key);
                            searchState[edge.Key] = 1;
                            comeFrom[edge.Key] = vertex;
                        }
                    }
                    searchState[vertex] = 2;
                }
                if (foundSink)
                {
                    TEdge flow = flowSummer.MaxValue;
                    int curVertex = sink;
                    int nextVertex = comeFrom[curVertex];
                    while (nextVertex != -1)
                    {
                        int edgeIndex = edgeLookup[new KeyValuePair<int, int>(nextVertex, curVertex)];
                        TEdge flowSegment = residualGraph.edges[edgeIndex];
                        if (flowComparer.Compare(flowSegment, flow) < 0)
                            flow = flowSegment;
                        curVertex = nextVertex;
                        nextVertex = comeFrom[curVertex];
                    }
                    sum = flowSummer.Add(sum, flow);

                    // Now update.
                    curVertex = sink;
                    nextVertex = comeFrom[curVertex];
                    while (nextVertex != -1)
                    {
                        int edgeIndex = edgeLookup[new KeyValuePair<int, int>(nextVertex, curVertex)];
                        TEdge flowSegment = residualGraph.edges[edgeIndex];
                        residualGraph.edges[edgeIndex] = flowSummer.Subtract(flowSegment, flow);

                        int reverseEdgeIndex = edgeLookup[new KeyValuePair<int, int>(curVertex, nextVertex)];
                        TEdge reverseFlowSegment = residualGraph.edges[reverseEdgeIndex];
                        residualGraph.edges[reverseEdgeIndex] = flowSummer.Add(reverseFlowSegment, flow);

                        curVertex = nextVertex;
                        nextVertex = comeFrom[curVertex];
                    }
                }
                else
                {
                    break;
                }
            }
            return sum;
        }
        // TODO: implement a minimum cost flow algorithm.
        // Maximum flow min cost algorithm will do instead, can be done by reimplementing the above using shortest path algorithm over costs instead of breadth first search.

        /// <summary>
        /// Calculates the maximum flow for minimum cost for a flow network graph between source and sink.
        /// Edges of the graph are mapped to capacities and costs
        /// </summary>
        /// <param name="source">
        /// Source node where flow comes from.
        /// </param>
        /// <param name="sink">
        /// Sink node where flow goes to.
        /// </param>
        /// <param name="flowComparer">
        /// Comparer for comparing flows.
        /// </param>
        /// <param name="flowSummer">
        /// Adder for summing flows.
        /// </param>
        /// <param name="costComparer">
        /// Comparer for comparing costs.
        /// </param>
        /// <param name="costSummer">
        /// Adder for adding costs.
        /// </param>
        /// <param name="costProduct">
        /// Function to expand a cost by a flow amount.
        /// </param>
        /// <param name="capGetter">
        /// Function to map an edge to its capacity.
        /// </param>
        /// <param name="costGetter">
        /// Function to map an edge to its cost.
        /// </param>
        /// <param name="capSetter">
        /// Function to set an edges capacity for use internally.
        /// </param>
        /// <param name="costSetter">
        /// Function to set an edges cost, for use internally.
        /// </param>
        /// <param name="edgeCreator">
        /// Function to create a zero cost zero capacity edge.
        /// </param>
        /// <param name="minCost">
        /// Receives the minium cost.
        /// </param>
        /// <param name="successful">
        /// True unless there exists a negative cost cycle in the initial available capacities.
        /// </param>
        /// <returns>
        /// The maximum flow if a flow could be found, otherwise flowSummer.Zero.
        /// </returns>
        public TCapacity MaximumFlowMinCost<TCapacity, TCost>(int source, int sink, IComparer<TCapacity> flowComparer, IAdder<TCapacity> flowSummer, IComparer<TCost> costComparer, IAdder<TCost> costSummer, Func<TCost, TCapacity, TCost> costProduct, Func<TEdge, TCapacity> capGetter, Func<TEdge, TCost> costGetter, Action<TEdge, TCapacity> capSetter, Action<TEdge, TCost> costSetter, Func<TEdge> edgeCreator, out TCost minCost, out bool successful)
        {
            Dictionary<KeyValuePair<int, int>, int> edgeLookup = new Dictionary<KeyValuePair<int, int>, int>();
            Dictionary<KeyValuePair<int, int>, int> originalEdge = new Dictionary<KeyValuePair<int, int>, int>();
            Graph<TVertex, TEdge> residualGraph = new Graph<TVertex, TEdge>();
            for (int i = 0; i < vertices.Count; i++)
            {
                residualGraph.AddVertex(vertices[i]);
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                foreach (KeyValuePair<int, int> edge in adjacencyList[i])
                {
                    int edgeIndex;
                    KeyValuePair<int, int> edgeEndPoints = new KeyValuePair<int, int>(i, edge.Key);
                    if (!edgeLookup.TryGetValue(edgeEndPoints, out edgeIndex))
                    {
                        TEdge e = edgeCreator();
                        costSetter(e, costGetter(edges[edge.Value]));
                        capSetter(e, capGetter(edges[edge.Value]));
                        edgeIndex = residualGraph.AddDirectedEdge(i, edge.Key, e);
                        edgeLookup.Add(edgeEndPoints, edgeIndex);
                        originalEdge.Add(edgeEndPoints, edge.Value);
                    }
                    else
                    {
                        capSetter(residualGraph.edges[edgeIndex], flowSummer.Add(capGetter(residualGraph.edges[edgeIndex]), capGetter(edges[edge.Value])));
                    }
                    int reverseEdgeIndex;
                    KeyValuePair<int, int> reverseEndPoints = new KeyValuePair<int, int>(edge.Key, i);
                    if (!edgeLookup.TryGetValue(reverseEndPoints, out reverseEdgeIndex))
                    {
                        reverseEdgeIndex = residualGraph.AddDirectedEdge(edge.Key, i, edgeCreator());
                        costSetter(residualGraph.edges[reverseEdgeIndex], costSummer.Subtract(costSummer.Zero, costGetter(edges[edge.Value])));
                        edgeLookup.Add(reverseEndPoints, reverseEdgeIndex);
                    }
                }
            }

            // User bellman-ford to start...
            int[] predecesors = new int[vertices.Count];
            TCost[] distances = new TCost[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                predecesors[i] = -1;
                distances[i] = costSummer.MaxValue;
            }
            distances[source] = costSummer.Zero;
            KeyValuePair<int, KeyValuePair<int, int>>[] edgeList = residualGraph.MakeEdgeList();

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                for (int j = 0; j < edgeList.Length; j++)
                {
                    int vertex = edgeList[j].Value.Key;
                    if (costComparer.Compare(distances[vertex], costSummer.MaxValue) == 0)
                        continue;
                    int destVertex = edgeList[j].Value.Value;
                    TCost sum = costSummer.Add(distances[vertex], costGetter(residualGraph.edges[edgeList[j].Key]));
                    if (costComparer.Compare(distances[destVertex], sum) > 0)
                    {
                        TCapacity availableFlow = capGetter(residualGraph.edges[edgeLookup[new KeyValuePair<int, int>(vertex, destVertex)]]);
                        if (flowComparer.Compare(availableFlow, flowSummer.Zero) > 0)
                        {
                            predecesors[destVertex] = vertex;
                            distances[destVertex] = sum;
                        }
                    }
                }
            }
            successful = true;
            for (int j = 0; j < edgeList.Length; j++)
            {
                int vertex = edgeList[j].Value.Key;
                if (costComparer.Compare(distances[vertex], costSummer.MaxValue) == 0)
                    continue;
                int destVertex = edgeList[j].Value.Value;
                TCost sum = costSummer.Add(distances[vertex], costGetter(residualGraph.edges[edgeList[j].Key]));
                if (costComparer.Compare(distances[destVertex], sum) > 0)
                {
                    TCapacity availableFlow = capGetter(residualGraph.edges[edgeLookup[new KeyValuePair<int, int>(vertex, destVertex)]]);
                    if (flowComparer.Compare(availableFlow, flowSummer.Zero) > 0)
                    {
                        successful = false;
                    }
                }
            }
            minCost = costSummer.Zero;
            if (!successful)
                return flowSummer.Zero;

            TCapacity capSum = flowSummer.Zero;
            bool foundSink = costComparer.Compare(distances[sink], costSummer.MaxValue) != 0;
            if (foundSink)
            {
                TCapacity flow = flowSummer.MaxValue;
                int curVertex = sink;
                int nextVertex = predecesors[curVertex];
                while (nextVertex != -1)
                {
                    int edgeIndex = edgeLookup[new KeyValuePair<int, int>(nextVertex, curVertex)];
                    TCapacity flowSegment = capGetter(residualGraph.edges[edgeIndex]);
                    if (flowComparer.Compare(flowSegment, flow) < 0)
                        flow = flowSegment;
                    curVertex = nextVertex;
                    nextVertex = predecesors[curVertex];
                }
                capSum = flowSummer.Add(capSum, flow);

                // Now update.
                curVertex = sink;
                nextVertex = predecesors[curVertex];
                while (nextVertex != -1)
                {
                    int edgeIndex = edgeLookup[new KeyValuePair<int, int>(nextVertex, curVertex)];
                    TCapacity flowSegment = capGetter(residualGraph.edges[edgeIndex]);
                    capSetter(residualGraph.edges[edgeIndex], flowSummer.Subtract(flowSegment, flow));
                    minCost = costSummer.Add(minCost, costProduct(costGetter(residualGraph.edges[edgeIndex]), flow));
                    int reverseEdgeIndex = edgeLookup[new KeyValuePair<int, int>(curVertex, nextVertex)];
                    TCapacity reverseFlowSegment = capGetter(residualGraph.edges[reverseEdgeIndex]);
                    capSetter(residualGraph.edges[reverseEdgeIndex], flowSummer.Add(reverseFlowSegment, flow));

                    curVertex = nextVertex;
                    nextVertex = predecesors[curVertex];
                }
            }
            else
            {
                return capSum;
            }

            // Update costs.
            for (int j = 0; j < edgeList.Length; j++)
            {
                TCost oldCost = costGetter(residualGraph.edges[edgeList[j].Key]);
                costSetter(residualGraph.edges[edgeList[j].Key], costSummer.Add(oldCost, costSummer.Subtract(distances[edgeList[j].Value.Key], distances[edgeList[j].Value.Value])));
            }

            while (true)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    predecesors[i] = -1;
                    distances[i] = costSummer.MaxValue;
                }
                distances[source] = costSummer.Zero;

                LookupHeap<KeyValuePair<TCost, int>> priorityQueue = new LookupHeap<KeyValuePair<TCost, int>>(new ReverseComparer<KeyValuePair<TCost, int>>(new KeyComparer<TCost, int>(costComparer)));
                for (int i = 0; i < vertices.Count; i++)
                {
                    priorityQueue.Add(new KeyValuePair<TCost, int>(distances[i], i));
                }
                while (priorityQueue.Count > 0)
                {
                    KeyValuePair<TCost, int> currentVertex = priorityQueue.PopFront();
                    // If distance is max value then all remaining nodes are unreachable from the source.
                    if (costComparer.Compare(currentVertex.Key, costSummer.MaxValue) == 0)
                        break;
                    foreach (KeyValuePair<int, int> edge in residualGraph.adjacencyList[currentVertex.Value])
                    {
                        int destVertex = edge.Key;
                        // If it makes the destination closer.
                        TCost sum = costSummer.Add(currentVertex.Key, costGetter(residualGraph.edges[edge.Value]));
                        if (costComparer.Compare(distances[edge.Key], sum) > 0)
                        {
                            // And we have available flow.
                            TCapacity availableFlow = capGetter(residualGraph.edges[edgeLookup[new KeyValuePair<int, int>(currentVertex.Value, destVertex)]]);
                            if (flowComparer.Compare(availableFlow, flowSummer.Zero) > 0)
                            {
                                predecesors[edge.Key] = currentVertex.Value;
                                // TODO: change to 'update entry' once there is such a method.
                                if (priorityQueue.Remove(new KeyValuePair<TCost, int>(distances[edge.Key], edge.Key)))
                                {
                                    distances[edge.Key] = sum;
                                    priorityQueue.Add(new KeyValuePair<TCost, int>(distances[edge.Key], edge.Key));
                                }
                                else
                                {
                                    distances[edge.Key] = sum;
                                }
                            }
                        }
                    }
                }
                for (int j = 0; j < edgeList.Length; j++)
                {
                    TCost oldCost = costGetter(residualGraph.edges[edgeList[j].Key]);
                    costSetter(residualGraph.edges[edgeList[j].Key], costSummer.Add(oldCost, costSummer.Subtract(distances[edgeList[j].Value.Key], distances[edgeList[j].Value.Value])));
                }

                foundSink = costComparer.Compare(distances[sink], costSummer.MaxValue) != 0;
                if (foundSink)
                {
                    TCapacity flow = flowSummer.MaxValue;
                    int curVertex = sink;
                    int nextVertex = predecesors[curVertex];
                    while (nextVertex != -1)
                    {
                        int edgeIndex = edgeLookup[new KeyValuePair<int, int>(nextVertex, curVertex)];
                        TCapacity flowSegment = capGetter(residualGraph.edges[edgeIndex]);
                        if (flowComparer.Compare(flowSegment, flow) < 0)
                            flow = flowSegment;
                        curVertex = nextVertex;
                        nextVertex = predecesors[curVertex];
                    }
                    capSum = flowSummer.Add(capSum, flow);

                    // Now update.
                    curVertex = sink;
                    nextVertex = predecesors[curVertex];
                    while (nextVertex != -1)
                    {
                        int edgeIndex = edgeLookup[new KeyValuePair<int, int>(nextVertex, curVertex)];
                        TCapacity flowSegment = capGetter(residualGraph.edges[edgeIndex]);
                        capSetter(residualGraph.edges[edgeIndex], flowSummer.Subtract(flowSegment, flow));
                        TCost edgeCost;
                        int origEdgeIndex;
                        if (originalEdge.TryGetValue(new KeyValuePair<int, int>(nextVertex, curVertex), out origEdgeIndex))
                        {
                            edgeCost = costGetter(edges[origEdgeIndex]);
                        }
                        else
                        {
                            edgeCost = costSummer.Subtract(costSummer.Zero, costGetter(edges[originalEdge[new KeyValuePair<int,int>(curVertex, nextVertex)]]));
                        }
                        minCost = costSummer.Add(minCost, costProduct(edgeCost, flow));
                        int reverseEdgeIndex = edgeLookup[new KeyValuePair<int, int>(curVertex, nextVertex)];
                        TCapacity reverseFlowSegment = capGetter(residualGraph.edges[reverseEdgeIndex]);
                        capSetter(residualGraph.edges[reverseEdgeIndex], flowSummer.Add(reverseFlowSegment, flow));

                        curVertex = nextVertex;
                        nextVertex = predecesors[curVertex];
                    }
                }
                else
                {
                    break;
                }
            }


            return capSum;
        }
        /// <summary>
        /// Determines the maximum independent set assuming this graph is undirected and bipartite.
        /// </summary>
        /// <returns></returns>
        public int MaximumBipartiteIndependentSet()
        {
            return vertices.Count - MaximumBipartiteMatching();
        }
        /// <summary>
        /// Determines the maximum matching assuming this graph is undirected and bipartite.
        /// </summary>
        /// <returns>
        /// The maximum matching.
        /// </returns>
        public int MaximumBipartiteMatching()
        {
            // Create bipartition, graph is undirected so this is easy.
            bool[] secondGroup = new bool[vertices.Count];
            DepthFirstSearch(delegate(int nodeId, int prevNodeId, int depth, bool postOrder)
            {
                if (!postOrder && prevNodeId != -1)
                {
                    secondGroup[nodeId] = !secondGroup[prevNodeId];
                }
                return false;
            });

            Graph<int, int> network = new Graph<int, int>();
            for (int i = 0; i < vertices.Count; i++)
            {
                network.AddVertex(i);
            }
            int source = network.AddVertex(vertices.Count);
            int sink = network.AddVertex(vertices.Count+1);
            for (int i = 0; i < vertices.Count; i++)
            {
                if (secondGroup[i])
                {
                    network.AddDirectedEdge(i, sink, 1);
                    // Only add edges from the first group to the second group, avoids duplication issues, so long as we are bipartite.
                }
                else
                {
                    network.AddDirectedEdge(source, i, 1);
                    foreach (KeyValuePair<int, int> edge in adjacencyList[i])
                    {
                        network.AddDirectedEdge(i, edge.Key, 1);
                    }
                }
            }
            return network.MaximumFlowFordFulkersonBfs(source, sink, Comparer<int>.Default, new IntAdder());


        }


        /// <summary>
        /// Determines whether a list of degrees is possible for an undirected graph with no self-loops.
        /// </summary>
        /// <param name="degrees">
        /// List of degrees for each vertex.
        /// </param>
        /// <returns>
        /// True if the graph is possible, false otherwise.
        /// </returns>
        public static bool GraphPlausible(IEnumerable<int> degrees)
        {
            List<int> degreeList = new List<int>(degrees);
            for (int i=0; i < degreeList.Count; i++)
            {
                degreeList.Sort();
                int current = degreeList[degreeList.Count-1];
                degreeList[degreeList.Count - 1] = 0;
                if (current > degreeList.Count-1)
                    return false;
                for (int j = degreeList.Count - 2; j >= degreeList.Count - 1 - current; j--)
                {
                    if (degreeList[j] <= 0)
                        return false;
                    degreeList[j] = degreeList[j]-1;
                }

            }
            return true;
        }
    }


    /// <summary>
    /// Search visitor function used by the breadth first search.
    /// </summary>
    /// <param name="nodeId">
    /// Id of node currently being visited.
    /// </param>
    /// <param name="prevNodeId">
    /// Id of the node which this node was reached from, or -1 if nodeId is the root of the search.
    /// </param>
    /// <param name="depth">
    /// Depth of this node in the search.
    /// </param>
    /// <returns>
    /// True to stop the search, false otherwise.
    /// </returns>
    [SuppressMessage("Microsoft.Naming", "CA1704")]
    public delegate bool BfsSearchVisitor(int nodeId, int prevNodeId, int depth);

    /// <summary>
    /// Search visitor function used by the depth first search.
    /// </summary>
    /// <param name="nodeId">
    /// Id of the node currently being visited.
    /// </param>
    /// <param name="prevNodeId">
    /// Id of the node which this node was reached from, or -1 if nodeId is the root of the search.
    /// </param>
    /// <param name="depth">
    /// Depth of this node in the search.
    /// </param>
    /// <param name="postOrder">
    /// True if this visit call is post order, false if it is pre order.
    /// </param>
    /// <returns>
    /// True to stop the search, false otherwise.
    /// </returns>
    [SuppressMessage("Microsoft.Naming", "CA1704")]
    public delegate bool DfsSearchVisitor(int nodeId, int prevNodeId, int depth, bool postOrder);
}
