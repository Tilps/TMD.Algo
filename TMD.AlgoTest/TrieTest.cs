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
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class TrieTest
    {
        private sealed class CaseInsensitiveCharComparer : IEqualityComparer<char>
        {
            public bool Equals(char x, char y) => char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
            public int GetHashCode(char obj) => char.ToUpperInvariant(obj).GetHashCode();
        }

        [Test]
        public void EmptyTrieState()
        {
            var trie = new Trie<char>();
            Assert.AreEqual(0, trie.Count);
            Assert.AreEqual(0, trie.UniqueCount);
            Assert.AreEqual(1, trie.NodeCount);
            Assert.IsFalse(trie.Contains("".AsSpan()));
            Assert.IsFalse(trie.Contains("a".AsSpan()));
            Assert.IsFalse(trie.StartsWith("".AsSpan()));
            Assert.IsFalse(trie.StartsWith("a".AsSpan()));
            Assert.AreEqual(0, trie.CountExact("".AsSpan()));
            Assert.AreEqual(0, trie.CountPrefix("".AsSpan()));
            Assert.IsFalse(trie.Remove("a".AsSpan()));
            Assert.AreEqual(0, trie.CountMatches([]));
        }

        [Test]
        public void AddDuplicatesPrefixAndRemove()
        {
            var trie = Trie.FromWords(["apple", "app", "apple", "apricot", "bat", ""]);

            Assert.AreEqual(6, trie.Count);
            Assert.AreEqual(5, trie.UniqueCount);
            Assert.AreEqual(14, trie.NodeCount);

            Assert.IsTrue(trie.Contains("".AsSpan()));
            Assert.AreEqual(1, trie.CountExact("".AsSpan()));
            Assert.AreEqual(6, trie.CountPrefix("".AsSpan()));

            Assert.IsTrue(trie.Contains("apple".AsSpan()));
            Assert.AreEqual(2, trie.CountExact("apple".AsSpan()));
            Assert.AreEqual(1, trie.CountExact("app".AsSpan()));
            Assert.AreEqual(0, trie.CountExact("ap".AsSpan()));
            Assert.IsTrue(trie.StartsWith("ap".AsSpan()));
            Assert.AreEqual(4, trie.CountPrefix("ap".AsSpan()));
            Assert.AreEqual(3, trie.CountPrefix("app".AsSpan()));
            Assert.AreEqual(1, trie.CountPrefix("bat".AsSpan()));
            Assert.IsFalse(trie.StartsWith("cat".AsSpan()));

            // Removing a non-existent word or prefix-only sequence returns false.
            Assert.IsFalse(trie.Remove("ap".AsSpan()));
            Assert.IsFalse(trie.Remove("zebra".AsSpan()));

            // Remove one duplicate of "apple": UniqueCount remains 5, NodeCount unchanged.
            Assert.IsTrue(trie.Remove("apple".AsSpan()));
            Assert.AreEqual(5, trie.Count);
            Assert.AreEqual(5, trie.UniqueCount);
            Assert.AreEqual(14, trie.NodeCount);
            Assert.AreEqual(1, trie.CountExact("apple".AsSpan()));
            Assert.AreEqual(2, trie.CountPrefix("app".AsSpan()));

            // Remove second "apple": UniqueCount drops to 4, "le" branch pruned (NodeCount 14 -> 12).
            Assert.IsTrue(trie.Remove("apple".AsSpan()));
            Assert.AreEqual(4, trie.Count);
            Assert.AreEqual(4, trie.UniqueCount);
            Assert.AreEqual(12, trie.NodeCount);
            Assert.IsFalse(trie.Contains("apple".AsSpan()));
            Assert.IsTrue(trie.Contains("app".AsSpan()));
            Assert.AreEqual(1, trie.CountPrefix("app".AsSpan()));

            // Remove empty string.
            Assert.IsTrue(trie.Remove((IEnumerable<char>)""));
            Assert.IsFalse(trie.Contains((IEnumerable<char>)""));
            Assert.AreEqual(3, trie.Count);
            Assert.AreEqual(3, trie.UniqueCount);
            Assert.AreEqual(12, trie.NodeCount);

            trie.Clear();
            Assert.AreEqual(0, trie.Count);
            Assert.AreEqual(0, trie.UniqueCount);
            Assert.AreEqual(1, trie.NodeCount);
            Assert.IsFalse(trie.StartsWith("a".AsSpan()));
        }

        [Test]
        public void EnumerableAndCustomComparerAndIntSequences()
        {
            var ciTrie = Trie.FromWords(["Abc", "aBd", "XYZ"], new CaseInsensitiveCharComparer());
            Assert.AreEqual(3, ciTrie.Count);
            Assert.IsTrue(ciTrie.Contains("ABC".AsSpan()));
            Assert.AreEqual(2, ciTrie.CountPrefix("ab".AsSpan()));
            Assert.AreEqual(2, ciTrie.CountMatches([c => char.ToLowerInvariant(c) == 'a', c => char.ToLowerInvariant(c) == 'b', _ => true]));

            int[][] numberSequences =
            [
                [1, 2, 3],
                [1, 2, 4],
                [1, 5, 3],
                [2, 2, 3],
                [1, 2]
            ];
            var intTrie = new Trie<int>(numberSequences);
            Assert.AreEqual(5, intTrie.Count);
            Assert.AreEqual(5, intTrie.UniqueCount);
            Assert.AreEqual(3, intTrie.CountPrefix((IEnumerable<int>)[1, 2]));
            Assert.IsTrue(intTrie.Contains((IEnumerable<int>)[1, 2]));

            Func<int, bool>[] pattern =
            [
                x => x == 1,
                x => x % 2 == 0 || x == 5,
                x => x == 3
            ];
            int expected = numberSequences.Count(seq =>
                seq.Length == pattern.Length && pattern.Where((p, idx) => p(seq[idx])).Count() == pattern.Length);
            Assert.AreEqual(expected, intTrie.CountMatches(pattern));
        }

        [Test]
        public void CountMatchesVerifiedAgainstBruteForceFilter()
        {
            string[] words = ["abc", "bca", "dac", "dbc", "cba", "abd", "abcd", "ab", "abc"];
            var trie = Trie.FromWords(words);

            Func<char, bool>[][] patterns =
            [
                [c => c == 'a' || c == 'b', c => c == 'b' || c == 'c', c => c == 'c' || c == 'a'],
                [c => c == 'a', c => c == 'b', c => c == 'c'],
                [c => c is 'a' or 'b' or 'c', c => c is 'a' or 'b' or 'c', c => c is 'a' or 'b' or 'c'],
                [c => c is 'z' or 'y' or 'x', c => c == 'b', c => c == 'c'],
                [c => c == 'a', c => c == 'b'],
                [c => c == 'a', c => c == 'b', c => c == 'c', c => c == 'd'],
                []
            ];

            foreach (var pattern in patterns)
            {
                int expected = words.Count(w =>
                    w.Length == pattern.Length &&
                    Enumerable.Range(0, pattern.Length).All(i => pattern[i](w[i])));
                Assert.AreEqual(expected, trie.CountMatches(pattern));
            }
        }

        [Test]
        public void NullArgumentValidation()
        {
            var trie = new Trie<char>();
            Assert.Throws<ArgumentNullException>(() => new Trie<char>((IEnumerable<IEnumerable<char>>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.Add((IEnumerable<char>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.AddRange(null!));
            Assert.Throws<ArgumentNullException>(() => trie.Remove((IEnumerable<char>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.Contains((IEnumerable<char>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.CountExact((IEnumerable<char>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.StartsWith((IEnumerable<char>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.CountPrefix((IEnumerable<char>)null!));
            Assert.Throws<ArgumentNullException>(() => trie.CountMatches(null!));
            Assert.Throws<ArgumentNullException>(() => trie.CountMatches([c => true, null!]));
            Assert.Throws<ArgumentNullException>(() => Trie.FromWords(null!));
            Assert.Throws<ArgumentNullException>(() => Trie.FromWords(["ok", null!]));
        }
    }
}
