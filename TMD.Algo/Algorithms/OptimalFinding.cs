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
using TMD.Algo.Algorithms.Generic;
using TMD.Algo.Collections.Generic;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Methods for calculating optimal scenarios.
    /// </summary>
    public static class OptimalFinding
    {
        /// <summary>
        /// Computes the minimum value in every contiguous window of length <paramref name="windowSize"/>
        /// across <paramref name="values"/> in O(n) time using a monotonic deque.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="values">Input values.</param>
        /// <param name="windowSize">Window size (must be between 1 and values.Length, inclusive).</param>
        /// <param name="comparer">Element comparer, or null to use <see cref="Comparer{T}.Default"/>.</param>
        /// <returns>An array of length values.Length - windowSize + 1 containing the minimum of each window.</returns>
        public static T[] SlidingWindowMin<T>(
            ReadOnlySpan<T> values,
            int windowSize,
            IComparer<T>? comparer = null)
        {
            return SlidingWindowFixed(values, windowSize, comparer, findMax: false);
        }

        /// <summary>
        /// Computes the maximum value in every contiguous window of length <paramref name="windowSize"/>
        /// across <paramref name="values"/> in O(n) time using a monotonic deque.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="values">Input values.</param>
        /// <param name="windowSize">Window size (must be between 1 and values.Length, inclusive).</param>
        /// <param name="comparer">Element comparer, or null to use <see cref="Comparer{T}.Default"/>.</param>
        /// <returns>An array of length values.Length - windowSize + 1 containing the maximum of each window.</returns>
        public static T[] SlidingWindowMax<T>(
            ReadOnlySpan<T> values,
            int windowSize,
            IComparer<T>? comparer = null)
        {
            return SlidingWindowFixed(values, windowSize, comparer, findMax: true);
        }

        /// <summary>
        /// Computes for each index i in <paramref name="values"/> the minimum over the clamped window
        /// [max(0, i - <paramref name="leftRadius"/>) .. min(n - 1, i + <paramref name="rightRadius"/>)]
        /// in O(n) time, writing results into <paramref name="destination"/>.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="values">Input values.</param>
        /// <param name="leftRadius">Non-negative radius extending left of each index.</param>
        /// <param name="rightRadius">Non-negative radius extending right of each index.</param>
        /// <param name="destination">Output span receiving at least values.Length elements.</param>
        /// <param name="comparer">Element comparer, or null to use <see cref="Comparer{T}.Default"/>.</param>
        public static void SlidingWindowMin<T>(
            ReadOnlySpan<T> values,
            int leftRadius,
            int rightRadius,
            Span<T> destination,
            IComparer<T>? comparer = null)
        {
            SlidingWindowRadius(values, leftRadius, rightRadius, destination, comparer, findMax: false);
        }

        /// <summary>
        /// Computes for each index i in <paramref name="values"/> the maximum over the clamped window
        /// [max(0, i - <paramref name="leftRadius"/>) .. min(n - 1, i + <paramref name="rightRadius"/>)]
        /// in O(n) time, writing results into <paramref name="destination"/>.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="values">Input values.</param>
        /// <param name="leftRadius">Non-negative radius extending left of each index.</param>
        /// <param name="rightRadius">Non-negative radius extending right of each index.</param>
        /// <param name="destination">Output span receiving at least values.Length elements.</param>
        /// <param name="comparer">Element comparer, or null to use <see cref="Comparer{T}.Default"/>.</param>
        public static void SlidingWindowMax<T>(
            ReadOnlySpan<T> values,
            int leftRadius,
            int rightRadius,
            Span<T> destination,
            IComparer<T>? comparer = null)
        {
            SlidingWindowRadius(values, leftRadius, rightRadius, destination, comparer, findMax: true);
        }

        private static T[] SlidingWindowFixed<T>(
            ReadOnlySpan<T> values,
            int windowSize,
            IComparer<T>? comparer,
            bool findMax)
        {
            if (windowSize <= 0 || windowSize > values.Length)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size must be between 1 and values.Length.");

            T[] result = new T[values.Length - windowSize + 1];
            SlidingWindowCore(values, windowSize - 1, 0, windowSize - 1, result, comparer, findMax);
            return result;
        }

        private static void SlidingWindowRadius<T>(
            ReadOnlySpan<T> values,
            int leftRadius,
            int rightRadius,
            Span<T> destination,
            IComparer<T>? comparer,
            bool findMax)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(leftRadius);
            ArgumentOutOfRangeException.ThrowIfNegative(rightRadius);
            if (destination.Length < values.Length)
                throw new ArgumentException("Destination span must be at least as long as values.", nameof(destination));

            SlidingWindowCore(values, leftRadius, rightRadius, 0, destination, comparer, findMax);
        }

        private static void SlidingWindowCore<T>(
            ReadOnlySpan<T> values,
            int leftRadius,
            int rightRadius,
            int startOutputIndex,
            Span<T> destination,
            IComparer<T>? comparer,
            bool findMax)
        {
            if (values.IsEmpty)
                return;

            int n = values.Length;
            IComparer<T> cmp = comparer ?? Comparer<T>.Default;
            int[] deque = new int[n];
            int head = 0;
            int tail = 0;
            int nextPush = 0;

            for (int i = 0; i < n; i++)
            {
                int rightEnd = (int)Math.Min(n - 1, (long)i + rightRadius);
                while (nextPush <= rightEnd)
                {
                    T incoming = values[nextPush];
                    while (tail > head)
                    {
                        int c = cmp.Compare(values[deque[tail - 1]], incoming);
                        if (findMax ? c <= 0 : c >= 0)
                            tail--;
                        else
                            break;
                    }
                    deque[tail++] = nextPush++;
                }

                long leftStart = (long)i - leftRadius;
                while (head < tail && deque[head] < leftStart)
                {
                    head++;
                }

                if (i >= startOutputIndex)
                {
                    destination[i - startOutputIndex] = values[deque[head]];
                }
            }
        }
        /// <summary>
        /// Finds the set of items which has minimal cost but reaches target value.
        /// </summary>
        /// <param name="target">
        /// Target value to reach.
        /// </param>
        /// <param name="costs">
        /// Costs of each item.
        /// </param>
        /// <param name="values">
        /// Values of each item.
        /// </param>
        /// <returns></returns>
        public static int[]? OptimalCostForTarget(int target, int[] costs, int[] values)
        {
            // break in to half to allow us to double the number of things we can process.
            // we can easily generate all combinations to work out what is best, but if number of items exceeds ~26, that is going to be slow.
            // This hack lets us go to almost double that in theory.
            int firstHalf = costs.Length/2 + 1;
            int rest = costs.Length - firstHalf;
            List<KeyValuePair<int, int>> firstCombinationTotals = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> secondCombinationTotals = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < (1 << (firstHalf + 1)); i++)
            {
                int totalCost = 0;
                int totalValue = 0;
                for (int j = 0; j < firstHalf; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        totalCost += costs[j];
                        totalValue += values[j];
                    }
                }
                firstCombinationTotals.Add(new KeyValuePair<int, int>(totalValue, totalCost));
                totalCost = 0;
                totalValue = 0;
                if (i < (1 << (rest + 1)))
                {
                    for (int j = 0; j < rest; j++)
                    {
                        if ((i & (1 << j)) != 0)
                        {
                            totalCost += costs[firstHalf + j];
                            totalValue += values[firstHalf + j];
                        }
                    }
                    secondCombinationTotals.Add(new KeyValuePair<int, int>(totalValue, totalCost));
                }
            }
            firstCombinationTotals.Sort(KvpComparer<int, int>.Default);
            secondCombinationTotals.Sort(KvpComparer<int, int>.Default);

            int other = secondCombinationTotals.Count - 1;
            int best = int.MaxValue;
            int localBest = int.MaxValue;
            List<int> result = new List<int>();
            int firstBestCombo = -1;
            int secondBestComboTemp = -1;
            int secondBestCombo = -1;
            for (int i = 0; i < firstCombinationTotals.Count; i++)
            {
                while (other >= 0 && secondCombinationTotals[other].Key + firstCombinationTotals[i].Key >= target)
                {
                    if (secondCombinationTotals[other].Value < localBest)
                    {
                        localBest = secondCombinationTotals[other].Value;
                        secondBestComboTemp = other;
                    }
                    other--;
                }
                if (localBest < int.MaxValue)
                {
                    int newTotal = firstCombinationTotals[i].Value + localBest;
                    if (newTotal < best)
                    {
                        best = newTotal;
                        firstBestCombo = i;
                        secondBestCombo = secondBestComboTemp;
                    }
                }
            }
            if (best < int.MaxValue)
            {
                for (int j = 0; j < firstHalf; j++)
                {
                    if ((firstBestCombo & (1 << j)) != 0)
                    {
                        result.Add(j);
                    }
                    if ((secondBestCombo & (1 << j)) != 0)
                    {
                        result.Add(j + firstHalf);
                    }
                }
                return result.ToArray();
            }
            else
                return null;
        }

        /// <summary>
        /// Computes the length of the longest increasing (or non-decreasing) subsequence of <paramref name="values"/>
        /// in O(n log n) time using patience sorting.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="values">Input sequence.</param>
        /// <param name="strictlyIncreasing">If true, strictly increasing (&lt;); if false, non-decreasing (&lt;=).</param>
        /// <param name="comparer">Optional element comparer, or null for <see cref="Comparer{T}.Default"/>.</param>
        /// <returns>The maximum subsequence length (0 if <paramref name="values"/> is empty).</returns>
        public static int LongestIncreasingSubsequenceLength<T>(
            ReadOnlySpan<T> values,
            bool strictlyIncreasing = true,
            IComparer<T>? comparer = null)
        {
            return LongestIncreasingSubsequenceCore(values, Span<int>.Empty, writePerElement: false, strictlyIncreasing, comparer);
        }

        /// <summary>
        /// Computes for every index <c>i</c> in <paramref name="values"/> the length of the longest increasing
        /// (or non-decreasing) subsequence ending at index <c>i</c>, storing them into <paramref name="lengthsEndingAt"/>,
        /// in O(n log n) time.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="values">Input sequence.</param>
        /// <param name="lengthsEndingAt">Destination span of length at least <c>values.Length</c>.</param>
        /// <param name="strictlyIncreasing">If true, strictly increasing (&lt;); if false, non-decreasing (&lt;=).</param>
        /// <param name="comparer">Optional element comparer, or null for <see cref="Comparer{T}.Default"/>.</param>
        /// <returns>The overall maximum subsequence length (0 if <paramref name="values"/> is empty).</returns>
        public static int LongestIncreasingSubsequenceLengths<T>(
            ReadOnlySpan<T> values,
            Span<int> lengthsEndingAt,
            bool strictlyIncreasing = true,
            IComparer<T>? comparer = null)
        {
            if (lengthsEndingAt.Length < values.Length)
                throw new ArgumentException("Destination span must be at least as long as values.", nameof(lengthsEndingAt));
            return LongestIncreasingSubsequenceCore(values, lengthsEndingAt, writePerElement: true, strictlyIncreasing, comparer);
        }

        private static int LongestIncreasingSubsequenceCore<T>(
            ReadOnlySpan<T> values,
            Span<int> lengthsEndingAt,
            bool writePerElement,
            bool strictlyIncreasing,
            IComparer<T>? comparer)
        {
            if (values.IsEmpty)
                return 0;

            IComparer<T> cmp = comparer ?? Comparer<T>.Default;
            T[] tails = new T[values.Length];
            int len = 0;

            for (int i = 0; i < values.Length; i++)
            {
                T val = values[i];
                ReadOnlySpan<T> active = tails.AsSpan(0, len);
                int pos = strictlyIncreasing
                    ? SpecialtySelect.LowerBound(active, val, cmp)
                    : SpecialtySelect.UpperBound(active, val, cmp);
                tails[pos] = val;
                if (pos == len)
                    len++;
                if (writePerElement)
                    lengthsEndingAt[i] = pos + 1;
            }

            return len;
        }
    }
}