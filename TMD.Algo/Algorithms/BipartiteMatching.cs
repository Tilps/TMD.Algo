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
    /// Represents the result of a maximum bipartite matching computation.
    /// </summary>
    public class BipartiteMatchingResult
    {
        /// <summary>
        /// Gets the cardinality of the maximum matching.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets the right vertex index matched to each left vertex, or -1 if unmatched.
        /// </summary>
        public IReadOnlyList<int> MatchLeft { get; }

        /// <summary>
        /// Gets the left vertex index matched to each right vertex, or -1 if unmatched.
        /// </summary>
        public IReadOnlyList<int> MatchRight { get; }

        /// <summary>
        /// Gets a value indicating whether every left vertex is matched.
        /// </summary>
        public bool CanMatchAllLeft => Count == MatchLeft.Count;

        /// <summary>
        /// Gets the matched edges as pairs of (Left, Right) vertex indices.
        /// </summary>
        public IEnumerable<(int Left, int Right)> Edges
        {
            get
            {
                for (int u = 0; u < MatchLeft.Count; u++)
                {
                    if (MatchLeft[u] >= 0)
                        yield return (u, MatchLeft[u]);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipartiteMatchingResult"/> class.
        /// </summary>
        /// <param name="count">The maximum matching cardinality.</param>
        /// <param name="matchLeft">Matched right vertex for each left vertex.</param>
        /// <param name="matchRight">Matched left vertex for each right vertex.</param>
        public BipartiteMatchingResult(int count, int[] matchLeft, int[] matchRight)
        {
            Count = count;
            MatchLeft = matchLeft;
            MatchRight = matchRight;
        }
    }

    /// <summary>
    /// Provides methods for maximum bipartite matching using the Hopcroft-Karp algorithm
    /// and augmenting path methods.
    /// </summary>
    public static class BipartiteMatching
    {
        private const int Nil = -1;
        private const int Infinity = int.MaxValue;

        /// <summary>
        /// Computes the maximum bipartite matching using the Hopcroft-Karp algorithm.
        /// Time complexity: O(|E| * sqrt(|V|)).
        /// </summary>
        /// <param name="leftCount">Number of vertices in the left partition (0-indexed: 0 .. leftCount - 1).</param>
        /// <param name="rightCount">Number of vertices in the right partition (0-indexed: 0 .. rightCount - 1).</param>
        /// <param name="adj">
        /// Adjacency list mapping each left vertex to an enumerable of connected right vertices.
        /// </param>
        /// <returns>A <see cref="BipartiteMatchingResult"/> describing the maximum matching.</returns>
        public static BipartiteMatchingResult FindMaximumMatching(
            int leftCount,
            int rightCount,
            IReadOnlyList<IReadOnlyList<int>> adj)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftCount);
            ArgumentOutOfRangeException.ThrowIfNegative(rightCount);
            ArgumentNullException.ThrowIfNull(adj);

            int[] pairU = new int[leftCount];
            int[] pairV = new int[rightCount];
            Array.Fill(pairU, Nil);
            Array.Fill(pairV, Nil);

            if (leftCount == 0 || rightCount == 0)
                return new BipartiteMatchingResult(0, pairU, pairV);

            int[] dist = new int[leftCount];
            int nilDist;
            Queue<int> queue = new(leftCount);

            bool Bfs()
            {
                nilDist = Infinity;
                for (int u = 0; u < leftCount; u++)
                {
                    if (pairU[u] == Nil)
                    {
                        dist[u] = 0;
                        queue.Enqueue(u);
                    }
                    else
                    {
                        dist[u] = Infinity;
                    }
                }

                while (queue.Count > 0)
                {
                    int u = queue.Dequeue();
                    if (dist[u] < nilDist)
                    {
                        var neighbors = adj[u];
                        int count = neighbors.Count;
                        for (int i = 0; i < count; i++)
                        {
                            int v = neighbors[i];
                            int nextU = pairV[v];
                            if (nextU == Nil)
                            {
                                nilDist = dist[u] + 1;
                            }
                            else if (dist[nextU] == Infinity)
                            {
                                dist[nextU] = dist[u] + 1;
                                queue.Enqueue(nextU);
                            }
                        }
                    }
                }

                return nilDist != Infinity;
            }

            bool Dfs(int u)
            {
                var neighbors = adj[u];
                int count = neighbors.Count;
                for (int i = 0; i < count; i++)
                {
                    int v = neighbors[i];
                    int nextU = pairV[v];
                    int nextDist = nextU == Nil ? nilDist : dist[nextU];

                    if (nextDist == dist[u] + 1)
                    {
                        if (nextU == Nil || Dfs(nextU))
                        {
                            pairV[v] = u;
                            pairU[u] = v;
                            return true;
                        }
                    }
                }

                dist[u] = Infinity;
                return false;
            }

            int matching = 0;
            while (Bfs())
            {
                for (int u = 0; u < leftCount; u++)
                {
                    if (pairU[u] == Nil && Dfs(u))
                        matching++;
                }
            }

            return new BipartiteMatchingResult(matching, pairU, pairV);
        }

        /// <summary>
        /// Computes the maximum bipartite matching using the Hopcroft-Karp algorithm.
        /// </summary>
        /// <param name="leftCount">Number of vertices in the left partition.</param>
        /// <param name="rightCount">Number of vertices in the right partition.</param>
        /// <param name="getNeighbors">Function returning the right neighbors for a given left vertex.</param>
        /// <returns>A <see cref="BipartiteMatchingResult"/> describing the maximum matching.</returns>
        public static BipartiteMatchingResult FindMaximumMatching(
            int leftCount,
            int rightCount,
            Func<int, IEnumerable<int>> getNeighbors)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftCount);
            ArgumentOutOfRangeException.ThrowIfNegative(rightCount);
            ArgumentNullException.ThrowIfNull(getNeighbors);
            var adj = new List<int>[leftCount];
            for (int u = 0; u < leftCount; u++)
                adj[u] = [.. getNeighbors(u)];
            return FindMaximumMatching(leftCount, rightCount, adj);
        }

        /// <summary>
        /// Computes the maximum bipartite matching from an edge list.
        /// </summary>
        /// <param name="leftCount">Number of vertices in the left partition.</param>
        /// <param name="rightCount">Number of vertices in the right partition.</param>
        /// <param name="edges">List of (Left, Right) edges.</param>
        /// <returns>A <see cref="BipartiteMatchingResult"/> describing the maximum matching.</returns>
        public static BipartiteMatchingResult FindMaximumMatching(
            int leftCount,
            int rightCount,
            IEnumerable<(int Left, int Right)> edges)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftCount);
            ArgumentOutOfRangeException.ThrowIfNegative(rightCount);
            ArgumentNullException.ThrowIfNull(edges);
            var adj = new List<int>[leftCount];
            for (int i = 0; i < leftCount; i++)
                adj[i] = [];
            foreach (var (u, v) in edges)
            {
                if (u >= 0 && u < leftCount && v >= 0 && v < rightCount)
                    adj[u].Add(v);
            }
            return FindMaximumMatching(leftCount, rightCount, adj);
        }

        /// <summary>
        /// Determines whether all left vertices can be matched in the bipartite graph.
        /// Uses early termination via augmenting paths: returns false as soon as any left vertex cannot be matched.
        /// </summary>
        /// <param name="leftCount">Number of left vertices to match.</param>
        /// <param name="rightCount">Number of right vertices.</param>
        /// <param name="adj">Adjacency list mapping each left vertex to connected right vertices.</param>
        /// <returns>True if every left vertex is matched; otherwise, false.</returns>
        public static bool CanMatchAllLeft(
            int leftCount,
            int rightCount,
            IReadOnlyList<IReadOnlyList<int>> adj)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftCount);
            ArgumentOutOfRangeException.ThrowIfNegative(rightCount);
            ArgumentNullException.ThrowIfNull(adj);
            if (leftCount > rightCount) return false;
            if (leftCount == 0) return true;

            int[] matchRight = new int[rightCount];
            Array.Fill(matchRight, Nil);
            bool[] visited = new bool[rightCount];

            bool TryAugment(int u)
            {
                var neighbors = adj[u];
                int count = neighbors.Count;
                for (int i = 0; i < count; i++)
                {
                    int v = neighbors[i];
                    if (visited[v]) continue;
                    visited[v] = true;

                    if (matchRight[v] == Nil || TryAugment(matchRight[v]))
                    {
                        matchRight[v] = u;
                        return true;
                    }
                }
                return false;
            }

            for (int u = 0; u < leftCount; u++)
            {
                Array.Clear(visited);
                if (!TryAugment(u))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether all left vertices can be matched in the bipartite graph defined by a neighbor function.
        /// </summary>
        /// <param name="leftCount">Number of left vertices to match.</param>
        /// <param name="rightCount">Number of right vertices.</param>
        /// <param name="getNeighbors">Function returning the right neighbors for a given left vertex.</param>
        /// <returns>True if every left vertex is matched; otherwise, false.</returns>
        public static bool CanMatchAllLeft(
            int leftCount,
            int rightCount,
            Func<int, IEnumerable<int>> getNeighbors)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftCount);
            ArgumentOutOfRangeException.ThrowIfNegative(rightCount);
            ArgumentNullException.ThrowIfNull(getNeighbors);
            if (leftCount > rightCount) return false;
            if (leftCount == 0) return true;

            var adj = new List<int>[leftCount];
            for (int u = 0; u < leftCount; u++)
                adj[u] = [.. getNeighbors(u)];

            return CanMatchAllLeft(leftCount, rightCount, adj);
        }

        /// <summary>
        /// Determines whether all left vertices can be matched in the bipartite graph defined by an edge predicate.
        /// </summary>
        /// <param name="leftCount">Number of left vertices to match.</param>
        /// <param name="rightCount">Number of right vertices.</param>
        /// <param name="hasEdge">Predicate indicating whether an edge exists between left vertex u and right vertex v.</param>
        /// <returns>True if every left vertex is matched; otherwise, false.</returns>
        public static bool CanMatchAllLeft(
            int leftCount,
            int rightCount,
            Func<int, int, bool> hasEdge)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftCount);
            ArgumentOutOfRangeException.ThrowIfNegative(rightCount);
            ArgumentNullException.ThrowIfNull(hasEdge);
            if (leftCount > rightCount) return false;
            if (leftCount == 0) return true;

            var adj = new List<int>[leftCount];
            for (int u = 0; u < leftCount; u++)
            {
                adj[u] = [];
                for (int v = 0; v < rightCount; v++)
                {
                    if (hasEdge(u, v))
                        adj[u].Add(v);
                }
            }

            return CanMatchAllLeft(leftCount, rightCount, adj);
        }
    }
}

