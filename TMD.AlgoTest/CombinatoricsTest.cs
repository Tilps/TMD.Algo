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
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class CombinatoricsTest
    {
        private static List<string> GenerateDistinctPermutationsBruteForce(char[] chars)
        {
            var set = new HashSet<string>();
            bool[] used = new bool[chars.Length];
            char[] cur = new char[chars.Length];

            void Backtrack(int depth)
            {
                if (depth == chars.Length)
                {
                    set.Add(new string(cur));
                    return;
                }
                for (int i = 0; i < chars.Length; i++)
                {
                    if (used[i]) continue;
                    used[i] = true;
                    cur[depth] = chars[i];
                    Backtrack(depth + 1);
                    used[i] = false;
                }
            }

            Backtrack(0);
            List<string> list = [.. set];
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        [Test]
        public void NextPermutationSpanAndListWithDuplicates()
        {
            char[] seed = ['0', '1', '1', '5'];
            List<string> expected = GenerateDistinctPermutationsBruteForce(seed);

            char[] spanWork = [.. seed];
            List<char> listWork = [.. seed];
            var observedSpan = new List<string>();
            var observedList = new List<string>();

            do
            {
                observedSpan.Add(new string(spanWork));
            } while (Combinatorics.NextPermutation(spanWork.AsSpan()));

            do
            {
                observedList.Add(new string([.. listWork]));
            } while (Combinatorics.NextPermutation(listWork));

            Assert.AreEqual(expected, observedSpan);
            Assert.AreEqual(expected, observedList);
            // Wrapping returns to initial ascending permutation.
            Assert.AreEqual("0115", new string(spanWork));
            Assert.AreEqual("0115", new string([.. listWork]));
        }

        [Test]
        public void NextPermutationEdgeCasesAndCustomComparer()
        {
            int[] empty = [];
            Assert.IsFalse(Combinatorics.NextPermutation(empty.AsSpan()));

            int[] single = [42];
            Assert.IsFalse(Combinatorics.NextPermutation(single.AsSpan()));
            Assert.AreEqual(42, single[0]);

            var descComparer = Comparer<int>.Create((a, b) => b.CompareTo(a));
            List<int> items = [3, 2, 1];
            var sequence = new List<string>();
            do
            {
                sequence.Add(string.Join(",", items));
            } while (Combinatorics.NextPermutation(items, descComparer));

            Assert.AreEqual(new[] { "3,2,1", "3,1,2", "2,3,1", "2,1,3", "1,3,2", "1,2,3" }, sequence);
            Assert.Throws<ArgumentNullException>(() => Combinatorics.NextPermutation((IList<int>)null!));
        }

        [Test]
        public void ChooseCachedMatchesUncachedAndPascalIdentities()
        {
            long[,] cache = null!;
            Combinatorics.Choose(20, 10, ref cache);
            for (int n = 0; n <= 20; n++)
            {
                for (int k = 0; k <= n; k++)
                {
                    long cached = Combinatorics.Choose(n, k, ref cache);
                    long direct = Combinatorics.Choose(n, k);
                    Assert.AreEqual(direct, cached, $"Mismatch at ({n}, {k})");
                }
                Assert.AreEqual(0, Combinatorics.Choose(n, n + 1, ref cache));
            }
            Assert.AreEqual(184756L, Combinatorics.Choose(20, 10, ref cache));
        }

        [Test]
        public void ChooseCappedTests()
        {
            long cap = 1_000_000_000_000_000L;
            Assert.AreEqual(0L, Combinatorics.ChooseCapped(10, -1, cap));
            Assert.AreEqual(0L, Combinatorics.ChooseCapped(10, 11, cap));
            Assert.AreEqual(0L, Combinatorics.ChooseCapped(10, 3, 0L));
            Assert.AreEqual(1L, Combinatorics.ChooseCapped(0, 0, cap));
            Assert.AreEqual(1L, Combinatorics.ChooseCapped(100, 100, 1L));

            for (int n = 0; n <= 20; n++)
            {
                for (int k = 0; k <= n; k++)
                    Assert.AreEqual(Combinatorics.Choose(n, k), Combinatorics.ChooseCapped(n, k, long.MaxValue));
            }

            // Very large n without intermediate overflow and saturating at cap.
            Assert.AreEqual(cap, Combinatorics.ChooseCapped(1_000_000_000_000_000L, 2, cap));
            Assert.AreEqual(cap, Combinatorics.ChooseCapped(100_000L, 50, cap));
            Assert.AreEqual(499_999_999_500_000_000L, Combinatorics.ChooseCapped(1_000_000_000L, 2, long.MaxValue));

            Assert.Throws<ArgumentOutOfRangeException>(() => Combinatorics.ChooseCapped(-1, 0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => Combinatorics.ChooseCapped(5, 2, -1));
        }
    }
}
