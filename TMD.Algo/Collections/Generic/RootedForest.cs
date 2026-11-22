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
using System.Numerics;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Represents a forest of rooted trees (where each node has at most one parent, and no cycles exist).
    /// Provides subtree structures, top-down topological ordering, lowest common ancestor (LCA) queries,
    /// counting linear extensions, and uniform random topological sorting.
    /// </summary>
    public class RootedForest
    {
        private readonly int[,] up;
        private readonly int[] tin;
        private readonly int[] tout;
        private readonly int[] treeId;
        private readonly int maxUp;

        /// <summary>
        /// Gets the total number of nodes in the forest.
        /// </summary>
        public int NodeCount { get; }

        /// <summary>
        /// Gets the list of root nodes in the forest.
        /// </summary>
        public IReadOnlyList<int> Roots { get; }

        /// <summary>
        /// Gets the parent index for each node (-1 for roots).
        /// </summary>
        public IReadOnlyList<int> Parents { get; }

        /// <summary>
        /// Gets the list of children for each node.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> Children { get; }

        /// <summary>
        /// Gets the size of the subtree rooted at each node (including the node itself).
        /// </summary>
        public IReadOnlyList<int> SubtreeSizes { get; }

        /// <summary>
        /// Gets the list of all node indices in the subtree rooted at each node.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> SubtreeNodes { get; }

        /// <summary>
        /// Gets a top-down pre-order topological ordering of all nodes in the forest.
        /// </summary>
        public IReadOnlyList<int> TopDownOrder { get; }

        /// <summary>
        /// Gets the depth of each node from its respective root (roots have depth 0).
        /// </summary>
        public IReadOnlyList<int> Depths { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootedForest"/> class from an array of parent indices.
        /// </summary>
        /// <param name="parents">
        /// Array where parents[i] is the parent of node i. A value &lt; 0 or equal to i indicates a root.
        /// </param>
        public RootedForest(int[] parents)
        {
            ArgumentNullException.ThrowIfNull(parents);

            NodeCount = parents.Length;
            int[] normalizedParents = new int[NodeCount];
            List<int>[] children = new List<int>[NodeCount];
            List<int> roots = [];

            for (int i = 0; i < NodeCount; i++)
            {
                children[i] = [];
                int p = parents[i];
                if (p < 0 || p == i || p >= NodeCount)
                {
                    normalizedParents[i] = -1;
                    roots.Add(i);
                }
                else
                {
                    normalizedParents[i] = p;
                }
            }

            for (int i = 0; i < NodeCount; i++)
            {
                int p = normalizedParents[i];
                if (p != -1)
                {
                    children[p].Add(i);
                }
            }

            Parents = normalizedParents;
            Children = children;
            Roots = roots;

            int[] depths = new int[NodeCount];
            List<int> topDownOrder = new List<int>(NodeCount);
            tin = new int[NodeCount];
            tout = new int[NodeCount];
            treeId = new int[NodeCount];

            int timer = 0;
            int[] state = new int[NodeCount]; // 0 = unvisited, 1 = visiting, 2 = visited

            for (int rIdx = 0; rIdx < roots.Count; rIdx++)
            {
                int root = roots[rIdx];
                DfsVisit(root, 0, rIdx, ref timer, state, depths, topDownOrder);
            }

            if (topDownOrder.Count != NodeCount)
                throw new ArgumentException("The provided parents array contains a cycle or unreachable nodes.");

            Depths = depths;
            TopDownOrder = topDownOrder;

            // Subtrees computation (bottom-up from reverse top-down order)
            List<int>[] subtreeNodes = new List<int>[NodeCount];
            int[] subtreeSizes = new int[NodeCount];
            for (int i = 0; i < NodeCount; i++)
                subtreeNodes[i] = new List<int>();

            for (int i = NodeCount - 1; i >= 0; i--)
            {
                int u = topDownOrder[i];
                subtreeNodes[u].Add(u);
                foreach (int v in children[u])
                {
                    subtreeNodes[u].AddRange(subtreeNodes[v]);
                }
                subtreeSizes[u] = subtreeNodes[u].Count;
            }

            SubtreeNodes = subtreeNodes;
            SubtreeSizes = subtreeSizes;

            // Binary lifting table initialization
            maxUp = 1;
            while ((1 << maxUp) <= NodeCount) maxUp++;
            up = new int[NodeCount, maxUp];

            for (int i = 0; i < NodeCount; i++)
            {
                up[i, 0] = normalizedParents[i];
            }

            for (int k = 1; k < maxUp; k++)
            {
                for (int i = 0; i < NodeCount; i++)
                {
                    int p = up[i, k - 1];
                    up[i, k] = p == -1 ? -1 : up[p, k - 1];
                }
            }
        }

        private void DfsVisit(
            int u,
            int depth,
            int currentTreeId,
            ref int timer,
            int[] state,
            int[] depths,
            List<int> topDownOrder)
        {
            if (state[u] == 1)
                throw new ArgumentException("The provided parents array contains a cycle.");
            if (state[u] == 2)
                return;

            state[u] = 1;
            depths[u] = depth;
            treeId[u] = currentTreeId;
            tin[u] = ++timer;
            topDownOrder.Add(u);

            foreach (int v in Children[u])
            {
                DfsVisit(v, depth + 1, currentTreeId, ref timer, state, depths, topDownOrder);
            }

            tout[u] = ++timer;
            state[u] = 2;
        }

        /// <summary>
        /// Creates a <see cref="RootedForest"/> from an array of parents.
        /// </summary>
        /// <param name="parents">Parent indices for each node.</param>
        /// <returns>A new <see cref="RootedForest"/> instance.</returns>
        public static RootedForest FromParents(int[] parents)
        {
            return new RootedForest(parents);
        }

        /// <summary>
        /// Generates a valid topological sort (linear extension) of the forest chosen uniformly at random.
        /// </summary>
        /// <param name="rng">Random number generator.</param>
        /// <returns>An array containing the node indices in topological order.</returns>
        public int[] GenerateRandomTopologicalSort(Random rng)
        {
            ArgumentNullException.ThrowIfNull(rng);
            if (NodeCount == 0) return [];

            double[] values = new double[NodeCount];
            for (int i = 0; i < NodeCount; i++)
                values[i] = rng.NextDouble();

            // Propagate minimum value in each subtree up to the root of that subtree (top-down)
            for (int i = 0; i < NodeCount; i++)
            {
                int u = TopDownOrder[i];
                int minNode = u;
                double minVal = values[u];
                IReadOnlyList<int> st = SubtreeNodes[u];
                for (int j = 0; j < st.Count; j++)
                {
                    int v = st[j];
                    if (values[v] < minVal)
                    {
                        minVal = values[v];
                        minNode = v;
                    }
                }
                if (minNode != u)
                {
                    (values[u], values[minNode]) = (values[minNode], values[u]);
                }
            }

            int[] order = new int[NodeCount];
            for (int i = 0; i < NodeCount; i++) order[i] = i;
            Array.Sort(order, (a, b) => values[a].CompareTo(values[b]));
            return order;
        }

        /// <summary>
        /// Counts the exact number of valid topological sorts (linear extensions) of the forest
        /// using the tree hook-length formula: N! / prod(SubtreeSize[v]).
        /// </summary>
        /// <returns>The exact number of linear extensions as a <see cref="BigInteger"/>.</returns>
        public BigInteger CountTopologicalSorts()
        {
            if (NodeCount == 0) return BigInteger.One;

            BigInteger factorial = BigInteger.One;
            for (int i = 2; i <= NodeCount; i++)
                factorial *= i;

            BigInteger denom = BigInteger.One;
            for (int i = 0; i < NodeCount; i++)
                denom *= SubtreeSizes[i];

            return factorial / denom;
        }

        /// <summary>
        /// Finds the lowest common ancestor (LCA) of two nodes using binary lifting in O(log N) time.
        /// </summary>
        /// <param name="u">First node index.</param>
        /// <param name="v">Second node index.</param>
        /// <returns>The LCA node index, or -1 if the nodes are in different trees.</returns>
        public int GetLowestCommonAncestor(int u, int v)
        {
            if (u < 0 || u >= NodeCount || v < 0 || v >= NodeCount)
                throw new ArgumentOutOfRangeException("Node indices must be within [0, NodeCount - 1].");

            if (treeId[u] != treeId[v])
                return -1;

            if (Depths[u] < Depths[v])
            {
                (u, v) = (v, u);
            }

            int diff = Depths[u] - Depths[v];
            for (int k = 0; k < maxUp; k++)
            {
                if ((diff & (1 << k)) != 0)
                {
                    u = up[u, k];
                }
            }

            if (u == v)
                return u;

            for (int k = maxUp - 1; k >= 0; k--)
            {
                if (up[u, k] != up[v, k])
                {
                    u = up[u, k];
                    v = up[v, k];
                }
            }

            return up[u, 0];
        }

        /// <summary>
        /// Gets the tree path distance between two nodes in O(log N) time.
        /// </summary>
        /// <param name="u">First node index.</param>
        /// <param name="v">Second node index.</param>
        /// <returns>The distance in edges, or -1 if the nodes are in different trees.</returns>
        public int GetDistance(int u, int v)
        {
            int lca = GetLowestCommonAncestor(u, v);
            if (lca == -1) return -1;
            return Depths[u] + Depths[v] - 2 * Depths[lca];
        }

        /// <summary>
        /// Determines whether <paramref name="ancestor"/> is an ancestor of <paramref name="descendant"/>.
        /// </summary>
        /// <param name="ancestor">Potential ancestor node.</param>
        /// <param name="descendant">Potential descendant node.</param>
        /// <returns>True if ancestor is equal to or an ancestor of descendant; false otherwise.</returns>
        public bool IsAncestor(int ancestor, int descendant)
        {
            if (ancestor < 0 || ancestor >= NodeCount || descendant < 0 || descendant >= NodeCount)
                return false;

            return treeId[ancestor] == treeId[descendant] &&
                   tin[ancestor] <= tin[descendant] &&
                   tout[ancestor] >= tout[descendant];
        }

        /// <summary>
        /// Finds the one or two centers (midpoints of a longest path) of an undirected tree in O(N) time and O(N) space
        /// iteratively, safe for arbitrarily deep paths without recursion.
        /// </summary>
        /// <param name="adjacency">Undirected tree adjacency list over vertices <c>0 .. N - 1</c>.</param>
        /// <returns>
        /// <c>(Center1, Center2)</c>, where <c>Center2</c> is <c>-1</c> if the tree has a unique center (even diameter)
        /// or the second adjacent center vertex if the diameter is odd.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="adjacency"/> is empty or disconnected.</exception>
        public static (int Center1, int Center2) FindTreeCenters(IReadOnlyList<IReadOnlyList<int>> adjacency)
        {
            ArgumentNullException.ThrowIfNull(adjacency);
            int n = adjacency.Count;
            if (n == 0)
                throw new ArgumentException("Tree must contain at least one vertex.", nameof(adjacency));
            if (n == 1)
                return (0, -1);

            int[] queue = new int[n];
            int[] parent = new int[n];
            int[] dist = new int[n];

            (int Farthest, int Visited) Bfs(int start)
            {
                Array.Fill(dist, -1);
                dist[start] = 0;
                parent[start] = -1;
                int head = 0;
                int tail = 0;
                queue[tail++] = start;
                int farthest = start;
                while (head < tail)
                {
                    int u = queue[head++];
                    if (dist[u] > dist[farthest])
                        farthest = u;
                    IReadOnlyList<int> neighbors = adjacency[u];
                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        int v = neighbors[i];
                        if (dist[v] == -1)
                        {
                            dist[v] = dist[u] + 1;
                            parent[v] = u;
                            queue[tail++] = v;
                        }
                    }
                }
                return (farthest, tail);
            }

            var (uEnd, visited1) = Bfs(0);
            if (visited1 != n)
                throw new ArgumentException("Tree is disconnected.", nameof(adjacency));

            var (vEnd, _) = Bfs(uEnd);
            int diameter = dist[vEnd];
            int mid = vEnd;
            int steps = diameter >> 1;
            for (int i = 0; i < steps; i++)
                mid = parent[mid];

            return (diameter & 1) == 0 ? (mid, -1) : (mid, parent[mid]);
        }

        /// <summary>
        /// Computes exact collision-free canonical subtree isomorphism IDs (AHU algorithm via trie interning)
        /// for an unlabeled tree rooted at <paramref name="root"/> in O(N log N) time and O(N) space iteratively.
        /// Two vertices have equal canonical IDs iff their rooted unlabeled subtrees are isomorphic.
        /// </summary>
        /// <param name="adjacency">Undirected tree adjacency list over vertices <c>0 .. N - 1</c>.</param>
        /// <param name="root">Root vertex index in [0, N - 1].</param>
        /// <returns>Array of canonical positive integer IDs indexed by vertex.</returns>
        public static int[] ComputeCanonicalSubtreeIds(IReadOnlyList<IReadOnlyList<int>> adjacency, int root)
        {
            return ComputeCanonicalSubtreeIdsCore(adjacency, root, ReadOnlySpan<int>.Empty, hasLabels: false);
        }

        /// <summary>
        /// Computes exact collision-free canonical subtree isomorphism IDs (AHU algorithm via trie interning)
        /// for a vertex-labeled tree rooted at <paramref name="root"/> in O(N log N) time and O(N) space iteratively.
        /// Two vertices have equal canonical IDs iff their rooted labeled subtrees are isomorphic.
        /// </summary>
        /// <param name="adjacency">Undirected tree adjacency list over vertices <c>0 .. N - 1</c>.</param>
        /// <param name="root">Root vertex index in [0, N - 1].</param>
        /// <param name="nodeLabels">Label for each vertex (length must equal <c>adjacency.Count</c>).</param>
        /// <returns>Array of canonical positive integer IDs indexed by vertex.</returns>
        public static int[] ComputeCanonicalSubtreeIds(
            IReadOnlyList<IReadOnlyList<int>> adjacency,
            int root,
            ReadOnlySpan<int> nodeLabels)
        {
            ArgumentNullException.ThrowIfNull(adjacency);
            if (nodeLabels.Length != adjacency.Count)
                throw new ArgumentException("nodeLabels length must match vertex count.", nameof(nodeLabels));
            return ComputeCanonicalSubtreeIdsCore(adjacency, root, nodeLabels, hasLabels: true);
        }

        private static int[] ComputeCanonicalSubtreeIdsCore(
            IReadOnlyList<IReadOnlyList<int>> adjacency,
            int root,
            ReadOnlySpan<int> nodeLabels,
            bool hasLabels)
        {
            ArgumentNullException.ThrowIfNull(adjacency);
            int n = adjacency.Count;
            if (n == 0)
                throw new ArgumentException("Tree must contain at least one vertex.", nameof(adjacency));
            ArgumentOutOfRangeException.ThrowIfNegative(root);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(root, n);

            int[] parent = new int[n];
            Array.Fill(parent, -1);
            bool[] visited = new bool[n];
            int[] order = new int[n];

            visited[root] = true;
            int head = 0;
            int tail = 0;
            order[tail++] = root;

            while (head < tail)
            {
                int u = order[head++];
                IReadOnlyList<int> neighbors = adjacency[u];
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int v = neighbors[i];
                    if (!visited[v])
                    {
                        visited[v] = true;
                        parent[v] = u;
                        order[tail++] = v;
                    }
                }
            }

            if (tail != n)
                throw new ArgumentException("Tree is disconnected.", nameof(adjacency));

            Dictionary<(int State, int Key), int> trie = [with(capacity: 2 * n)];
            int nextState = 1;
            int StepTrie(int state, int key)
            {
                if (!trie.TryGetValue((state, key), out int next))
                {
                    next = nextState++;
                    trie[(state, key)] = next;
                }
                return next;
            }

            int[] ids = new int[n];
            List<int> childIds = [];
            for (int idx = n - 1; idx >= 0; idx--)
            {
                int u = order[idx];
                childIds.Clear();
                IReadOnlyList<int> neighbors = adjacency[u];
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int v = neighbors[i];
                    if (v != parent[u])
                        childIds.Add(ids[v]);
                }
                childIds.Sort();

                int state = StepTrie(0, hasLabels ? nodeLabels[u] : 0);
                for (int i = 0; i < childIds.Count; i++)
                    state = StepTrie(state, childIds[i]);
                ids[u] = state;
            }

            return ids;
        }
    }
}

