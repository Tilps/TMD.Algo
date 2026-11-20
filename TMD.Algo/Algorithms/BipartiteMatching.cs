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

        /// <summary>
        /// Counts the number of independent cycles remaining after degree-1 peeling in a balanced bipartite graph
        /// where every left vertex connects to two distinct right vertices. Each remaining cycle doubles the number
        /// of perfect matchings.
        /// </summary>
        /// <param name="count">Number of vertices in each partition.</param>
        /// <param name="edges">Span of length <paramref name="count"/> giving each left vertex's two distinct right neighbors.</param>
        /// <returns>
        /// The cycle count K &gt;= 0 (meaning 2^K perfect matchings), or -1 if no perfect matching exists.
        /// Returns 0 for <paramref name="count"/> == 0.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> &lt; 0 or a vertex index is out of range.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="edges"/> length mismatches <paramref name="count"/> or a left vertex has duplicate endpoints.</exception>
        public static int CountPerfectMatchingsLeftDegreeTwoCycles(
            int count,
            ReadOnlySpan<(int First, int Second)> edges)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (edges.Length != count)
                throw new ArgumentException("Edges length must equal count.", nameof(edges));
            if (count == 0)
                return 0;

            List<int>[] rightAdj = new List<int>[count];
            for (int v = 0; v < count; v++)
                rightAdj[v] = [];

            for (int u = 0; u < count; u++)
            {
                var (v0, v1) = edges[u];
                if ((uint)v0 >= (uint)count || (uint)v1 >= (uint)count)
                    throw new ArgumentOutOfRangeException(nameof(edges), "Right vertex index is out of range.");
                if (v0 == v1)
                    throw new ArgumentException("Each left vertex must connect to two distinct right vertices.", nameof(edges));
                rightAdj[v0].Add(u);
                rightAdj[v1].Add(u);
            }

            int[] deg = new int[count];
            bool[] leftRemoved = new bool[count];
            bool[] rightRemoved = new bool[count];
            Queue<int> queue = new();

            for (int v = 0; v < count; v++)
            {
                deg[v] = rightAdj[v].Count;
                if (deg[v] == 0)
                    return -1;
                if (deg[v] == 1)
                    queue.Enqueue(v);
            }

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                if (rightRemoved[v])
                    continue;
                if (deg[v] == 0)
                    return -1;

                int matchedU = -1;
                foreach (int u in rightAdj[v])
                {
                    if (!leftRemoved[u])
                    {
                        matchedU = u;
                        break;
                    }
                }
                if (matchedU < 0)
                    return -1;

                leftRemoved[matchedU] = true;
                rightRemoved[v] = true;

                var (first, second) = edges[matchedU];
                int otherV = first == v ? second : first;
                if (!rightRemoved[otherV])
                {
                    deg[otherV]--;
                    if (deg[otherV] == 0)
                        return -1;
                    if (deg[otherV] == 1)
                        queue.Enqueue(otherV);
                }
            }

            // Every remaining active vertex on both sides now has degree exactly 2, forming disjoint cycles.
            int cycles = 0;
            Queue<int> bfs = new();
            for (int v = 0; v < count; v++)
            {
                if (rightRemoved[v])
                    continue;

                cycles++;
                rightRemoved[v] = true;
                bfs.Enqueue(v);
                while (bfs.Count > 0)
                {
                    int curV = bfs.Dequeue();
                    foreach (int u in rightAdj[curV])
                    {
                        if (leftRemoved[u])
                            continue;
                        leftRemoved[u] = true;
                        var (first, second) = edges[u];
                        int nextV = first == curV ? second : first;
                        if (!rightRemoved[nextV])
                        {
                            rightRemoved[nextV] = true;
                            bfs.Enqueue(nextV);
                        }
                    }
                }
            }

            return cycles;
        }

        /// <summary>
        /// Counts the number of independent cycles remaining after degree-1 peeling in a balanced bipartite graph
        /// where every left vertex connects to two distinct right vertices.
        /// </summary>
        /// <param name="count">Number of vertices in each partition.</param>
        /// <param name="edges">List of length <paramref name="count"/> giving each left vertex's two distinct right neighbors.</param>
        /// <returns>The cycle count K &gt;= 0 (meaning 2^K perfect matchings), or -1 if no perfect matching exists.</returns>
        public static int CountPerfectMatchingsLeftDegreeTwoCycles(
            int count,
            IReadOnlyList<(int First, int Second)> edges)
        {
            ArgumentNullException.ThrowIfNull(edges);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (edges.Count != count)
                throw new ArgumentException("Edges count must equal count.", nameof(edges));
            var arr = new (int First, int Second)[count];
            for (int i = 0; i < count; i++)
                arr[i] = edges[i];
            return CountPerfectMatchingsLeftDegreeTwoCycles(count, arr.AsSpan());
        }

        /// <summary>
        /// Counts the number of perfect matchings modulo <paramref name="modulus"/> in a balanced bipartite graph
        /// where every left vertex connects to two distinct right vertices, in O(V) time.
        /// </summary>
        /// <param name="count">Number of vertices in each partition.</param>
        /// <param name="edges">Span of length <paramref name="count"/> giving each left vertex's two distinct right neighbors.</param>
        /// <param name="modulus">Positive modulus.</param>
        /// <returns>Number of perfect matchings modulo <paramref name="modulus"/>.</returns>
        public static long CountPerfectMatchingsLeftDegreeTwo(
            int count,
            ReadOnlySpan<(int First, int Second)> edges,
            long modulus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulus);
            int cycles = CountPerfectMatchingsLeftDegreeTwoCycles(count, edges);
            return cycles < 0 ? 0 : Modulo.Pow(2, cycles, modulus);
        }

        /// <summary>
        /// Counts the number of perfect matchings modulo <paramref name="modulus"/> in a balanced bipartite graph
        /// where every left vertex connects to two distinct right vertices, in O(V) time.
        /// </summary>
        /// <param name="count">Number of vertices in each partition.</param>
        /// <param name="edges">List of length <paramref name="count"/> giving each left vertex's two distinct right neighbors.</param>
        /// <param name="modulus">Positive modulus.</param>
        /// <returns>Number of perfect matchings modulo <paramref name="modulus"/>.</returns>
        public static long CountPerfectMatchingsLeftDegreeTwo(
            int count,
            IReadOnlyList<(int First, int Second)> edges,
            long modulus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulus);
            int cycles = CountPerfectMatchingsLeftDegreeTwoCycles(count, edges);
            return cycles < 0 ? 0 : Modulo.Pow(2, cycles, modulus);
        }
    }
}

