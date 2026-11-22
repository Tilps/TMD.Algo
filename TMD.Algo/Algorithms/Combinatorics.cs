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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Methods for counting things.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Counts the number of different ways to generate all sums less than or equal to maximum given the specified number of distinct nonnegative integers.
        /// </summary>
        /// <param name="maximum">
        /// Maximum sum.
        /// </param>
        /// <param name="numberOfNumbers">
        /// Number of distinct nonnegative integers make up the sum.
        /// </param>
        /// <returns>
        /// The total number of sums.
        /// </returns>
        public static long CountSumsDistinctNumbersOrderDoesNotMatter(long maximum, long numberOfNumbers)
        {
            return CountSumsOrderDoesNotMatter(maximum - numberOfNumbers*(numberOfNumbers + 1)/2, numberOfNumbers);
        }

        /// <summary>
        /// Counts the number of different ways to generate all sums less than or equal to the maximum given the specified number of nonnegative integers.
        /// They may not be distinct.
        /// </summary>
        /// <param name="maximum">
        /// Maximum sum.
        /// </param>
        /// <param name="numberOfNumbers">
        /// Number of nonnegative integers to make up the sum.
        /// </param>
        /// <returns>
        /// The total number of sums.
        /// </returns>
        public static long CountSumsOrderDoesNotMatter(long maximum, long numberOfNumbers)
        {
            // This maps the problem to the number of accumulations to achieve the maximum.
            // a + (a+b) + (a+b+c) + (a+b+c+d) - where order does matter for a, b, c, d
            // Because of the sums being constructed this way, they are in nondecreasing order, giving the order does not matter
            // part of the original problem.
            long[,] cache = new long[2, maximum + 1];
            for (int i = 0; i <= maximum; i++)
            {
                cache[0, i] = 1;
            }
            for (int i = 1; i <= numberOfNumbers; i++)
            {
                for (int j = 0; j <= maximum; j++)
                {
                    long total = cache[1 - i%2, j];
                    if (j >= i)
                        total += cache[i%2, j - i];
                    cache[i%2, j] = total;
                }
            }
            return cache[numberOfNumbers%2, maximum];
        }

        /// <summary>
        /// Counts the number of bit patterns of length n, where exclusions of length l are defined.
        /// </summary>
        /// <param name="n">
        /// Number of bits in the overal patern.
        /// </param>
        /// <param name="l">
        /// Number of bits in each exclusion.
        /// </param>
        /// <param name="exclusions">
        /// Truth table for whether a given mask of l bits is an exclusion or not.
        /// </param>
        /// <param name="startingMask">
        /// The initial mask of bits seen so far, masked to length l.
        /// </param>
        /// <param name="startingOffset">
        /// Current number of bits in to the pattern.
        /// </param>
        /// <param name="cache">
        /// Recieves cache to use for multiple calls with same paramteres other than startingMask and startingOffset.
        /// Or passes in an existing cache.  The variable passed on the first call should be null.
        /// </param>
        /// <returns>
        /// The number of bit patterns satisfying the restrictions.
        /// </returns>
        public static long CountBitPatternsWithExclusions(int n, int l, bool[] exclusions, int startingMask,
            int startingOffset, ref long[,] cache)
        {
            if (cache == null)
            {
                cache = new long[n + 1, 1 << (l + 1)];
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j < 1 << (l + 1); j++)
                    {
                        cache[i, j] = -1;
                    }
                }
            }
            startingMask &= (1 << (l + 1)) - 1;
            return BitPatternRecurseCount(n, l, exclusions, startingMask, startingOffset, cache);
        }

        private static long BitPatternRecurseCount(int n, int l, bool[] exclusions, int startingMask, int startingOffset,
            long[,] cache)
        {
            if (startingOffset >= l && exclusions[startingMask])
                return 0;
            if (startingOffset == n)
                return 1;
            if (cache[startingOffset, startingMask] == -1)
            {
                int nextMaskBase = startingMask << 1;
                nextMaskBase &= (1 << (l + 1)) - 1;
                long total = BitPatternRecurseCount(n, l, exclusions, nextMaskBase, startingOffset + 1, cache);
                total += BitPatternRecurseCount(n, l, exclusions, nextMaskBase | 1, startingOffset + 1, cache);
                cache[startingOffset, startingMask] = total;
            }
            return cache[startingOffset, startingMask];
        }

        /// <summary>
        /// Determines the number of times the prime number p can divide n factorial.
        /// </summary>
        /// <param name="p">
        /// Prime number to test for divisibility.
        /// </param>
        /// <param name="n">
        /// The factorial which is to be divided.
        /// </param>
        /// <returns>
        /// The number of times n! can be divided by p.
        /// </returns>
        public static long CountDivisibilityInFactorial(long p, long n)
        {
            long count = 0;
            long currentPower = p;
            while (currentPower <= n)
            {
                count += n/currentPower;
                currentPower *= p;
            }
            return count;
        }

        /// <summary>
        /// Determines the number of times the prime number p can divide the number of permutations of size k from a set size n.
        /// </summary>
        /// <param name="p">
        /// Prime number to test for divisibility.
        /// </param>
        /// <param name="n">
        /// Size of the set for the permutations.
        /// </param>
        /// <param name="k">
        /// Number of elements selected from the set for the permutations.
        /// </param>
        /// <returns>
        /// The number of times p divides n perm k.
        /// </returns>
        public static long CountDivisibilityInPermutation(long p, long n, long k)
        {
            return CountDivisibilityInFactorial(p, n) - CountDivisibilityInFactorial(p, n - k);
        }

        /// <summary>
        /// Determines the number of times the prime number p can divide n choose k.
        /// </summary>
        /// <param name="p">
        /// Prime number to test for divisibility.
        /// </param>
        /// <param name="n">
        /// Size of set for choose operation.
        /// </param>
        /// <param name="k">
        /// Number of items to choose from n.
        /// </param>
        /// <returns>
        /// The number of times p divides n choose k.
        /// </returns>
        public static long CountDivisibilityInCombination(long p, long n, long k)
        {
            return CountDivisibilityInPermutation(p, n, k) - CountDivisibilityInFactorial(p, k);
        }

        /// <summary>
        /// Counts the number of ways to arrange n objects such that none of them are in their original positions.
        /// </summary>
        /// <param name="n">
        /// Number of objects n.</param>
        /// <returns>
        /// The number of derangements of n objects.  So long as n! does not overflow.
        /// </returns>
        public static long Derangements(long n)
        {
            long res = 1;
            for (int i = 1; i <= n; i++)
                res *= n;
            double approx = res/Math.E;
            return (long)Math.Round(approx);
        }

        /// <summary>
        /// Counts the number of rearrangements of n objects, where k objects are in their starting point.  If overflow may be important use the cached version.
        /// </summary>
        /// <param name="n">
        /// Number of objects.
        /// </param>
        /// <param name="k">
        /// Number of objects which don't move.
        /// </param>
        /// <returns>
        /// The number of partial derangements of n objects.
        /// </returns>
        public static long PartialDerangements(long n, long k)
        {
            if (n == k)
                return 1;
            if (n == k + 1)
                return 0;
            return Choose(n, k)*Derangements(n - k);
        }

        /// <summary>
        /// Calculates n choose k.  Avoids simple overflows, but still easy to overflow.  If overflow may be important, use cached version.
        /// Running time is proportional to n.
        /// </summary>
        /// <param name="n">
        /// Number of items to choose from.
        /// </param>
        /// <param name="k">
        /// Number of items to choose.
        /// </param>
        /// <returns>
        /// n choose k.
        /// </returns>
        public static long Choose(long n, long k)
        {
            if (k < 0 || k > n)
                return 0;
            long res = 1;
            long currentK = k;
            for (long i = n; i >= n - k + 1; i--)
            {
                res *= i;
                while (currentK > 1 && res%currentK == 0)
                {
                    res /= currentK;
                    currentK--;
                }
            }
            while (currentK > 1)
            {
                res /= currentK;
                currentK--;
            }
            return res;
        }

        /// <summary>
        /// Determines the number of unordered ways to select k objects from n objects where the same object can be selected multiple times.
        /// If overflow may be important use the version which takes a cache parameter.
        /// </summary>
        /// <param name="n">
        /// Number of objects to choose from.
        /// </param>
        /// <param name="k">
        /// Number of objects to select with possible reptition.
        /// </param>
        /// <returns>
        /// n multichoose k
        /// </returns>
        public static long MultiChoose(long n, long k)
        {
            return Choose(n + k - 1, k);
        }

        /// <summary>
        /// Determines n choose k using a cache. This improves performance for repetition and also only overflows where long is too small.
        /// Running time is n^2 if cache is insufficient.  Calling once with the maximum possible n up front will save execution time.
        /// </summary>
        /// <param name="n">
        /// Number of items to choose from.
        /// </param>
        /// <param name="k">
        /// Number of items to choose.
        /// </param>
        /// <param name="cache">
        /// Cache parameter to receive the calculated cache.  Alternatively passes in a previously received result to avoid calculating the cache again if possible.
        /// </param>
        /// <returns>
        /// The number of ways to choose k items from n without respect to ordering.
        /// </returns>
        public static long Choose(long n, long k, ref long[,] cache)
        {
            if (k < 0 || k > n)
                return 0;
            // This version only supports positive integers.
            if (n < 1 || k < 1)
                return Choose(n, k);
            if (cache == null || cache.GetLength(0) <= n)
            {
                cache = new long[n + 1, n + 1];
                cache[0, 0] = 1;
                for (int i = 1; i <= n; i++)
                {
                    cache[i, 0] = 1;
                    cache[i, i] = 1;
                    for (int j = 1; j < i; j++)
                    {
                        cache[i, j] = cache[i - 1, j] + cache[i - 1, j - 1];
                    }
                }
            }
            return cache[n, k];
        }

        /// <summary>
        /// Determines the number of unordered ways to select k objects from n objects where the same object can be selected multiple times.
        /// Uses a cache to improve performance over multiple calls and reduce overflows.  Calling once up front with the maximum possible value of n+k will avoid cache recalculation.
        /// </summary>
        /// <param name="n">
        /// Number of objects to choose from.
        /// </param>
        /// <param name="k">
        /// Number of objects to select with possible reptition.
        /// </param>
        /// <param name="cache">
        /// Cache parameter to receive the calculated cache.  Alternatively passes in a previously received result to avoid calculating the cache again if possible.
        /// </param>
        /// <returns>
        /// n multichoose k
        /// </returns>
        public static long MultiChoose(long n, long k, ref long[,] cache)
        {
            return Choose(n + k - 1, k, ref cache);
        }


        /// <summary>
        /// Counts the number of rearrangements of n objects, where k objects are in their starting point.
        /// Uses a cache to improve performance over multiple calls and reduce overflows.  Calling once up front with the maximum possible value of n will reduce cache recalculation.
        /// </summary>
        /// <param name="n">
        /// Number of objects.
        /// </param>
        /// <param name="k">
        /// Number of objects which don't move.
        /// </param>
        /// <param name="cache">
        /// Cache parameter to receive the calculated cache.  Alternatively passes in a previously received result to avoid calculating the cache again if possible.
        /// </param>
        /// <returns>
        /// The number of partial derangements of n objects.
        /// </returns>
        public static long PartialDerangements(long n, long k, ref long[,] cache)
        {
            if (n == k)
                return 1;
            if (n == k + 1)
                return 0;
            return Choose(n, k, ref cache)*Derangements(n - k);
        }

        /// <summary>
        /// Transforms <paramref name="items"/> in-place into its next lexicographical permutation according to <paramref name="comparer"/>.
        /// If <paramref name="items"/> is already the last permutation, wraps around to the smallest permutation and returns false.
        /// </summary>
        /// <typeparam name="T">Type of elements in the sequence.</typeparam>
        /// <param name="items">Span of elements to permute in-place.</param>
        /// <param name="comparer">Comparer defining element ordering, or null to use <see cref="Comparer{T}.Default"/>.</param>
        /// <returns>True if a strictly greater next permutation existed; false if it wrapped around to the first permutation.</returns>
        public static bool NextPermutation<T>(Span<T> items, IComparer<T>? comparer = null)
        {
            if (items.Length < 2)
                return false;

            comparer ??= Comparer<T>.Default;
            int pivot = items.Length - 2;
            while (pivot >= 0 && comparer.Compare(items[pivot], items[pivot + 1]) >= 0)
            {
                pivot--;
            }

            if (pivot < 0)
            {
                items.Reverse();
                return false;
            }

            int swapIdx = items.Length - 1;
            while (comparer.Compare(items[pivot], items[swapIdx]) >= 0)
            {
                swapIdx--;
            }

            (items[pivot], items[swapIdx]) = (items[swapIdx], items[pivot]);
            items[(pivot + 1)..].Reverse();
            return true;
        }

        /// <summary>
        /// Transforms <paramref name="items"/> in-place into its next lexicographical permutation according to <paramref name="comparer"/>.
        /// If <paramref name="items"/> is already the last permutation, wraps around to the smallest permutation and returns false.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="items">List of elements to permute in-place.</param>
        /// <param name="comparer">Comparer defining element ordering, or null to use <see cref="Comparer{T}.Default"/>.</param>
        /// <returns>True if a strictly greater next permutation existed; false if it wrapped around to the first permutation.</returns>
        public static bool NextPermutation<T>(IList<T> items, IComparer<T>? comparer = null)
        {
            ArgumentNullException.ThrowIfNull(items);
            if (items.Count < 2)
                return false;

            comparer ??= Comparer<T>.Default;
            int pivot = items.Count - 2;
            while (pivot >= 0 && comparer.Compare(items[pivot], items[pivot + 1]) >= 0)
            {
                pivot--;
            }

            if (pivot < 0)
            {
                ReverseRange(items, 0, items.Count - 1);
                return false;
            }

            int swapIdx = items.Count - 1;
            while (comparer.Compare(items[pivot], items[swapIdx]) >= 0)
            {
                swapIdx--;
            }

            (items[pivot], items[swapIdx]) = (items[swapIdx], items[pivot]);
            ReverseRange(items, pivot + 1, items.Count - 1);
            return true;
        }

        /// <summary>
        /// Computes the binomial coefficient C(<paramref name="n"/>, <paramref name="k"/>), saturated at
        /// <paramref name="cap"/>, in O(min(k, n - k)) time using internal <see cref="Int128"/> arithmetic
        /// without 64-bit intermediate overflow.
        /// </summary>
        /// <param name="n">Non-negative total item count.</param>
        /// <param name="k">Number of chosen items.</param>
        /// <param name="cap">Non-negative saturation cap.</param>
        /// <returns>
        /// <c>min(C(n, k), cap)</c>, or <c>0</c> if <paramref name="k"/> &lt; 0 or <paramref name="k"/> &gt; <paramref name="n"/>.
        /// </returns>
        public static long ChooseCapped(long n, long k, long cap)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n);
            ArgumentOutOfRangeException.ThrowIfNegative(cap);
            if (cap == 0 || k < 0 || k > n)
                return 0L;
            if (k == 0 || k == n)
                return Math.Min(1L, cap);
            if (k > n - k)
                k = n - k;

            Int128 res = 1;
            Int128 cap128 = cap;
            for (long i = 1; i <= k; i++)
            {
                res = res * (n - k + i) / i;
                if (res >= cap128)
                    return cap;
            }
            return (long)res;
        }

        private static void ReverseRange<T>(IList<T> items, int left, int right)
        {
            while (left < right)
            {
                (items[left], items[right]) = (items[right], items[left]);
                left++;
                right--;
            }
        }
    }
}