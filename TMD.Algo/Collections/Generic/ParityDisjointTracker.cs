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

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Dynamic integer disjoint-set (Union-Find) maintaining modulo-2 parity (bipartite side) constraints
    /// between elements, detecting odd-cycle contradictions online in amortized near-O(1) time.
    /// </summary>
    public class ParityDisjointTracker
    {
        private readonly List<int> parent;
        private readonly List<int> parity;
        private readonly List<int> size;

        /// <summary>
        /// Initializes a <see cref="ParityDisjointTracker"/> with <paramref name="initialCount"/> isolated vertices.
        /// </summary>
        /// <param name="initialCount">Initial number of elements (0-indexed).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCount"/> &lt; 0.</exception>
        public ParityDisjointTracker(int initialCount = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(initialCount);
            parent = [with(capacity: initialCount)];
            parity = [with(capacity: initialCount)];
            size = [with(capacity: initialCount)];
            for (int i = 0; i < initialCount; i++)
            {
                parent.Add(i);
                parity.Add(0);
                size.Add(1);
            }
        }

        /// <summary>
        /// Gets the total number of tracked elements.
        /// </summary>
        public int Count => parent.Count;

        /// <summary>
        /// Adds a new isolated element and returns its 0-based index.
        /// </summary>
        /// <returns>The index of the newly added element.</returns>
        public int Add()
        {
            int id = parent.Count;
            parent.Add(id);
            parity.Add(0);
            size.Add(1);
            return id;
        }

        private (int Root, int Parity) FindInternal(int x)
        {
            int p = parent[x];
            if (p == x)
                return (x, 0);
            var (root, pPar) = FindInternal(p);
            parent[x] = root;
            parity[x] ^= pPar;
            return (root, parity[x]);
        }

        private void ValidateIndex(int x, string paramName)
        {
            if ((uint)x >= (uint)parent.Count)
                throw new ArgumentOutOfRangeException(paramName, "Element index is out of range.");
        }

        /// <summary>
        /// Finds the canonical root representative of <paramref name="x"/> together with its parity relative to that root.
        /// </summary>
        /// <param name="x">0-based element index.</param>
        /// <returns>A tuple containing the root index and relative parity bit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is out of range.</exception>
        public (int Root, bool Parity) Find(int x)
        {
            ValidateIndex(x, nameof(x));
            var (root, par) = FindInternal(x);
            return (root, par != 0);
        }

        /// <summary>
        /// Enforces the constraint <c>parity(a) ^ parity(b) == opposite</c>.
        /// If inconsistent with existing constraints, leaves the structure unchanged and returns <c>false</c>.
        /// </summary>
        /// <param name="a">First element index.</param>
        /// <param name="b">Second element index.</param>
        /// <param name="opposite">True to force different sides; false to force the same side.</param>
        /// <returns>True if the constraint is consistent and applied; false if it introduces a parity contradiction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="a"/> or <paramref name="b"/> is out of range.</exception>
        public bool Union(int a, int b, bool opposite)
        {
            ValidateIndex(a, nameof(a));
            ValidateIndex(b, nameof(b));

            var (ra, pa) = FindInternal(a);
            var (rb, pb) = FindInternal(b);
            int rel = opposite ? 1 : 0;
            if (ra == rb)
                return (pa ^ pb) == rel;

            if (size[ra] < size[rb])
                (ra, rb) = (rb, ra);

            parent[rb] = ra;
            parity[rb] = pa ^ pb ^ rel;
            size[ra] += size[rb];
            return true;
        }

        /// <summary>
        /// Enforces that <paramref name="a"/> and <paramref name="b"/> belong to the same parity side.
        /// </summary>
        /// <param name="a">First element index.</param>
        /// <param name="b">Second element index.</param>
        /// <returns>True if consistent; false on contradiction.</returns>
        public bool UnionSame(int a, int b) => Union(a, b, false);

        /// <summary>
        /// Enforces that <paramref name="a"/> and <paramref name="b"/> belong to opposite parity sides.
        /// </summary>
        /// <param name="a">First element index.</param>
        /// <param name="b">Second element index.</param>
        /// <returns>True if consistent; false on contradiction.</returns>
        public bool UnionOpposite(int a, int b) => Union(a, b, true);

        /// <summary>
        /// Determines whether <paramref name="a"/> and <paramref name="b"/> are currently constrained in the same component.
        /// </summary>
        /// <param name="a">First element index.</param>
        /// <param name="b">Second element index.</param>
        /// <returns>True if connected in the same component; otherwise false.</returns>
        public bool AreConnected(int a, int b)
        {
            ValidateIndex(a, nameof(a));
            ValidateIndex(b, nameof(b));
            return FindInternal(a).Root == FindInternal(b).Root;
        }

        /// <summary>
        /// Attempts to determine the forced parity relationship between <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">First element index.</param>
        /// <param name="b">Second element index.</param>
        /// <param name="sameSide">True if forced to the same side; false if forced to opposite sides or disconnected.</param>
        /// <returns>True if <paramref name="a"/> and <paramref name="b"/> are in the same component; otherwise false.</returns>
        public bool TryGetRelation(int a, int b, out bool sameSide)
        {
            ValidateIndex(a, nameof(a));
            ValidateIndex(b, nameof(b));
            var (ra, pa) = FindInternal(a);
            var (rb, pb) = FindInternal(b);
            if (ra != rb)
            {
                sameSide = false;
                return false;
            }
            sameSide = pa == pb;
            return true;
        }
    }
}
