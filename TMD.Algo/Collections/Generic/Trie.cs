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
    /// Prefix tree (trie) storing multiset sequences of symbols with exact, prefix, and positional-predicate pattern queries.
    /// </summary>
    /// <typeparam name="T">Type of symbol in each sequence.</typeparam>
    public class Trie<T> where T : notnull
    {
        private struct Node
        {
            public Dictionary<T, int> Children;
            public int PrefixCount;
            public int TerminalCount;
        }

        private readonly IEqualityComparer<T> comparer;
        private readonly List<Node> nodes = [];

        /// <summary>
        /// Initializes an empty <see cref="Trie{T}"/> using the default equality comparer for <typeparamref name="T"/>.
        /// </summary>
        public Trie() : this((IEqualityComparer<T>?)null)
        {
        }

        /// <summary>
        /// Initializes an empty <see cref="Trie{T}"/> using the specified equality comparer for symbol comparisons.
        /// </summary>
        /// <param name="comparer">Equality comparer for symbols, or null to use <see cref="EqualityComparer{T}.Default"/>.</param>
        public Trie(IEqualityComparer<T>? comparer)
        {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            nodes.Add(CreateNode());
        }

        /// <summary>
        /// Initializes a <see cref="Trie{T}"/> populated with the specified sequences.
        /// </summary>
        /// <param name="sequences">Initial sequences to insert.</param>
        /// <param name="comparer">Equality comparer for symbols, or null to use <see cref="EqualityComparer{T}.Default"/>.</param>
        public Trie(IEnumerable<IEnumerable<T>> sequences, IEqualityComparer<T>? comparer = null)
            : this(comparer)
        {
            ArgumentNullException.ThrowIfNull(sequences);
            AddRange(sequences);
        }

        /// <summary>
        /// Gets the total number of sequence insertions currently stored (including duplicates).
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the number of distinct sequences currently stored in the trie.
        /// </summary>
        public int UniqueCount { get; private set; }

        /// <summary>
        /// Gets the number of active nodes currently in the trie (including the root node).
        /// </summary>
        public int NodeCount { get; private set; } = 1;

        /// <summary>
        /// Inserts a sequence of symbols into the trie.
        /// </summary>
        /// <param name="sequence">The symbol sequence to insert.</param>
        public void Add(ReadOnlySpan<T> sequence)
        {
            int nodeIdx = 0;
            Node root = nodes[0];
            root.PrefixCount++;
            nodes[0] = root;

            for (int i = 0; i < sequence.Length; i++)
            {
                nodeIdx = GetOrCreateChild(nodeIdx, sequence[i]);
                Node child = nodes[nodeIdx];
                child.PrefixCount++;
                nodes[nodeIdx] = child;
            }

            Node terminal = nodes[nodeIdx];
            if (terminal.TerminalCount == 0)
                UniqueCount++;
            terminal.TerminalCount++;
            nodes[nodeIdx] = terminal;
            Count++;
        }

        /// <summary>
        /// Inserts a sequence of symbols into the trie.
        /// </summary>
        /// <param name="sequence">The symbol sequence to insert.</param>
        public void Add(IEnumerable<T> sequence)
        {
            ArgumentNullException.ThrowIfNull(sequence);
            int nodeIdx = 0;
            Node root = nodes[0];
            root.PrefixCount++;
            nodes[0] = root;

            foreach (T symbol in sequence)
            {
                nodeIdx = GetOrCreateChild(nodeIdx, symbol);
                Node child = nodes[nodeIdx];
                child.PrefixCount++;
                nodes[nodeIdx] = child;
            }

            Node terminal = nodes[nodeIdx];
            if (terminal.TerminalCount == 0)
                UniqueCount++;
            terminal.TerminalCount++;
            nodes[nodeIdx] = terminal;
            Count++;
        }

        /// <summary>
        /// Inserts all sequences in the specified collection into the trie.
        /// </summary>
        /// <param name="sequences">The collection of symbol sequences to insert.</param>
        public void AddRange(IEnumerable<IEnumerable<T>> sequences)
        {
            ArgumentNullException.ThrowIfNull(sequences);
            foreach (IEnumerable<T> sequence in sequences)
            {
                Add(sequence);
            }
        }

        /// <summary>
        /// Removes one occurrence of the specified sequence from the trie, if present.
        /// </summary>
        /// <param name="sequence">The symbol sequence to remove.</param>
        /// <returns>True if an occurrence was removed; false if the sequence was not present.</returns>
        public bool Remove(ReadOnlySpan<T> sequence)
        {
            int[] path = new int[sequence.Length + 1];
            path[0] = 0;
            int nodeIdx = 0;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (!nodes[nodeIdx].Children.TryGetValue(sequence[i], out int nextIdx))
                    return false;
                nodeIdx = nextIdx;
                path[i + 1] = nodeIdx;
            }

            if (nodes[nodeIdx].TerminalCount == 0)
                return false;

            Node terminal = nodes[nodeIdx];
            terminal.TerminalCount--;
            if (terminal.TerminalCount == 0)
                UniqueCount--;
            nodes[nodeIdx] = terminal;

            for (int i = 0; i < path.Length; i++)
            {
                int currentIdx = path[i];
                Node current = nodes[currentIdx];
                current.PrefixCount--;
                nodes[currentIdx] = current;
            }

            for (int i = 0; i < sequence.Length; i++)
            {
                int childIdx = path[i + 1];
                if (nodes[childIdx].PrefixCount == 0 && nodes[path[i]].Children.Remove(sequence[i]))
                {
                    NodeCount--;
                }
            }

            Count--;
            return true;
        }

        /// <summary>
        /// Removes one occurrence of the specified sequence from the trie, if present.
        /// </summary>
        /// <param name="sequence">The symbol sequence to remove.</param>
        /// <returns>True if an occurrence was removed; false if the sequence was not present.</returns>
        public bool Remove(IEnumerable<T> sequence)
        {
            ArgumentNullException.ThrowIfNull(sequence);
            T[] materialized = [.. sequence];
            return Remove(materialized.AsSpan());
        }

        /// <summary>
        /// Removes all sequences from the trie.
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            nodes.Add(CreateNode());
            Count = 0;
            UniqueCount = 0;
            NodeCount = 1;
        }

        /// <summary>
        /// Determines whether the trie contains at least one occurrence of the specified sequence.
        /// </summary>
        /// <param name="sequence">The symbol sequence to locate.</param>
        /// <returns>True if present; otherwise, false.</returns>
        public bool Contains(ReadOnlySpan<T> sequence) => CountExact(sequence) > 0;

        /// <summary>
        /// Determines whether the trie contains at least one occurrence of the specified sequence.
        /// </summary>
        /// <param name="sequence">The symbol sequence to locate.</param>
        /// <returns>True if present; otherwise, false.</returns>
        public bool Contains(IEnumerable<T> sequence) => CountExact(sequence) > 0;

        /// <summary>
        /// Returns how many times the exact specified sequence has been inserted.
        /// </summary>
        /// <param name="sequence">The symbol sequence to count.</param>
        /// <returns>Number of occurrences of <paramref name="sequence"/> in the trie.</returns>
        public int CountExact(ReadOnlySpan<T> sequence)
        {
            int nodeIdx = FindNode(sequence);
            return nodeIdx >= 0 ? nodes[nodeIdx].TerminalCount : 0;
        }

        /// <summary>
        /// Returns how many times the exact specified sequence has been inserted.
        /// </summary>
        /// <param name="sequence">The symbol sequence to count.</param>
        /// <returns>Number of occurrences of <paramref name="sequence"/> in the trie.</returns>
        public int CountExact(IEnumerable<T> sequence)
        {
            int nodeIdx = FindNode(sequence);
            return nodeIdx >= 0 ? nodes[nodeIdx].TerminalCount : 0;
        }

        /// <summary>
        /// Determines whether at least one stored sequence starts with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to locate.</param>
        /// <returns>True if any stored sequence begins with <paramref name="prefix"/>; otherwise, false.</returns>
        public bool StartsWith(ReadOnlySpan<T> prefix) => CountPrefix(prefix) > 0;

        /// <summary>
        /// Determines whether at least one stored sequence starts with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to locate.</param>
        /// <returns>True if any stored sequence begins with <paramref name="prefix"/>; otherwise, false.</returns>
        public bool StartsWith(IEnumerable<T> prefix) => CountPrefix(prefix) > 0;

        /// <summary>
        /// Counts how many stored sequence insertions begin with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to count.</param>
        /// <returns>Number of stored sequence insertions starting with <paramref name="prefix"/>.</returns>
        public int CountPrefix(ReadOnlySpan<T> prefix)
        {
            int nodeIdx = FindNode(prefix);
            return nodeIdx >= 0 ? nodes[nodeIdx].PrefixCount : 0;
        }

        /// <summary>
        /// Counts how many stored sequence insertions begin with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to count.</param>
        /// <returns>Number of stored sequence insertions starting with <paramref name="prefix"/>.</returns>
        public int CountPrefix(IEnumerable<T> prefix)
        {
            int nodeIdx = FindNode(prefix);
            return nodeIdx >= 0 ? nodes[nodeIdx].PrefixCount : 0;
        }

        /// <summary>
        /// Counts how many stored sequence insertions have length equal to <paramref name="pattern"/>.Count
        /// and satisfy <paramref name="pattern"/>[i] at each position i, pruning branches that fail early.
        /// </summary>
        /// <param name="pattern">Sequence of per-position predicates.</param>
        /// <returns>Total count of matching sequence insertions.</returns>
        public int CountMatches(IReadOnlyList<Func<T, bool>> pattern)
        {
            ArgumentNullException.ThrowIfNull(pattern);
            for (int i = 0; i < pattern.Count; i++)
            {
                ArgumentNullException.ThrowIfNull(pattern[i], nameof(pattern));
            }

            return CountMatchesCore(0, 0, pattern);
        }

        private int CountMatchesCore(int nodeIdx, int depth, IReadOnlyList<Func<T, bool>> pattern)
        {
            Node node = nodes[nodeIdx];
            if (node.PrefixCount == 0)
                return 0;

            if (depth == pattern.Count)
                return node.TerminalCount;

            Func<T, bool> predicate = pattern[depth];
            int total = 0;
            foreach (KeyValuePair<T, int> child in node.Children)
            {
                if (nodes[child.Value].PrefixCount > 0 && predicate(child.Key))
                {
                    total += CountMatchesCore(child.Value, depth + 1, pattern);
                }
            }
            return total;
        }

        private int FindNode(ReadOnlySpan<T> sequence)
        {
            int nodeIdx = 0;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (!nodes[nodeIdx].Children.TryGetValue(sequence[i], out int nextIdx))
                    return -1;
                nodeIdx = nextIdx;
            }
            return nodeIdx;
        }

        private int FindNode(IEnumerable<T> sequence)
        {
            ArgumentNullException.ThrowIfNull(sequence);
            int nodeIdx = 0;
            foreach (T symbol in sequence)
            {
                if (!nodes[nodeIdx].Children.TryGetValue(symbol, out int nextIdx))
                    return -1;
                nodeIdx = nextIdx;
            }
            return nodeIdx;
        }

        private int GetOrCreateChild(int parentIdx, T symbol)
        {
            Dictionary<T, int> children = nodes[parentIdx].Children;
            if (!children.TryGetValue(symbol, out int childIdx))
            {
                childIdx = nodes.Count;
                nodes.Add(CreateNode());
                children[symbol] = childIdx;
                NodeCount++;
            }
            return childIdx;
        }

        private Node CreateNode()
        {
            return new Node
            {
                Children = [with(comparer)],
                PrefixCount = 0,
                TerminalCount = 0
            };
        }
    }

    /// <summary>
    /// Factory helpers for creating <see cref="Trie{T}"/> instances.
    /// </summary>
    public static class Trie
    {
        /// <summary>
        /// Builds a character <see cref="Trie{T}"/> from a collection of words.
        /// </summary>
        /// <param name="words">Words to insert into the trie.</param>
        /// <param name="comparer">Optional character equality comparer.</param>
        /// <returns>A populated <see cref="Trie{T}"/> containing all provided words.</returns>
        public static Trie<char> FromWords(IEnumerable<string> words, IEqualityComparer<char>? comparer = null)
        {
            ArgumentNullException.ThrowIfNull(words);
            Trie<char> trie = new(comparer);
            foreach (string word in words)
            {
                ArgumentNullException.ThrowIfNull(word, nameof(words));
                trie.Add(word.AsSpan());
            }
            return trie;
        }
    }
}
