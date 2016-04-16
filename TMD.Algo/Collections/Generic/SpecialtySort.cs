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
using System.Linq;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides helper methods for sorting data using methods other than QuickSort or equivelent which is built-in.
    /// </summary>
    public static class SpecialtySort
    {
        /// <summary>
        /// Provides a bucket sort, an expected O(n) time sort assuming the input is uniformly distributed by the partitioning function.
        /// This sort is not in-place, it is stable.
        /// </summary>
        /// <typeparam name="T">
        /// Type of element in the input list.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable set of items to sort.
        /// </param>
        /// <param name="part">
        /// The partitioning function which distributes the input uniformly over the range 0 to n-1 in the same order as the sort.
        /// </param>
        /// <returns>
        /// A new list which contains the sorted results.
        /// </returns>
        public static List<T> BucketSort<T>(this IEnumerable<T> input, PartitionFunction<T> part)
        {
            return BucketSort(input, part, null);
        }

        /// <summary>
        /// Provides a bucket sort, an expected O(n) time sort assuming the input is uniformly distributed by the partitioning function.
        /// This sort is not in-place, it is stable.
        /// </summary>
        /// <typeparam name="T">
        /// Type of element in the input list.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable set of items to sort.
        /// </param>
        /// <param name="part">
        /// The partitioning function which distributes the input uniformly over the range 0 to n-1 in the same order as the sort.
        /// </param>
        /// <param name="comparer">
        /// Comparer used to compare items which are partitioned in to the same bucket.  Specify null to use the default comparer.
        /// </param>
        /// <returns>
        /// A new list which contains the sorted results.
        /// </returns>
        public static List<T> BucketSort<T>(this IEnumerable<T> input, PartitionFunction<T> part, IComparer<T> comparer)
        {
            int count = input.Count();
            List<T>[] parts = new List<T>[count];
            for (int i = 0; i < count; i++)
                parts[i] = new List<T>();
            foreach (T value in input)
            {
                parts[part(value, count)].Add(value);
            }
            List<T> results = new List<T>();
            for (int i = 0; i < count; i++)
            {
                parts[i].Sort(comparer);
                results.AddRange(parts[i]);
            }
            return results;
        }

        /// <summary>
        /// Provides a basic O(n) counting sort for integers.
        /// This sort is not inplace, it is stable.
        /// </summary>
        /// <param name="input">
        /// Enumerable set of integers to sort.
        /// </param>
        /// <param name="min">
        /// Minimum value in the input.
        /// </param>
        /// <param name="max">
        /// Maximum value in the input.
        /// </param>
        /// <returns>
        /// A sorted list.
        /// </returns>
        public static List<int> CountingSort(this IEnumerable<int> input, int min, int max)
        {
            int[] counts = new int[max - min + 1];
            int entries = 0;
            foreach (int value in input)
            {
                counts[value - min]++;
                entries++;
            }
            List<int> results = new List<int>(entries);
            for (int i = 0; i < counts.Length; i++)
            {
                for (int j = 0; j < counts[i]; j++)
                {
                    results.Add(i + min);
                }
            }
            return results;
        }

        /// <summary>
        /// Provides a generic O(n) counting sort.
        /// This sort is not inplace, it is stable.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable set of values to be sorted.
        /// </param>
        /// <param name="map">
        /// Mapping function to map values to a sort conforming integer range.
        /// </param>
        /// <param name="min">
        /// Minimum value to be returned from the map function.
        /// </param>
        /// <param name="max">
        /// Maximum value to be returned from the map function.
        /// </param>
        /// <returns>
        /// A new array containing the sorted items.
        /// </returns>
        public static T[] CountingSort<T>(this IEnumerable<T> input, IntMapFunction<T> map, int min, int max)
        {
            int[] counts = new int[max - min + 1];
            int entries = 0;
            foreach (T value in input)
            {
                counts[map(value) - min]++;
                entries++;
            }
            // Accumulate.
            for (int i = 1; i < counts.Length; i++)
            {
                counts[i] += counts[i - 1];
            }
            // Shift backwards by segment total, so we can do a stable sort with forward walk, rather than having to enumerate backwards.
            for (int i = counts.Length - 1; i >= 1; i--)
            {
                counts[i] -= counts[i] - counts[i - 1];
            }
            counts[0] = 0;
            T[] resultsArray = new T[entries];
            foreach (T value in input)
            {
                resultsArray[counts[map(value) - min]] = value;
                counts[map(value) - min]++;
            }
            return resultsArray;
        }

        /// <summary>
        /// Provides an O(n)*radixCount radix sort based on the counting sort.
        /// This sort is not inplace, it is stable.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable set of values to be sorted.
        /// </param>
        /// <param name="radixMap">
        /// Radix function which maps values to sort conforming integer ranges dependent on the radix.
        /// </param>
        /// <param name="min">
        /// Minimum value expected from the radix function.
        /// </param>
        /// <param name="max">
        /// Maximum value expected from the radix function.
        /// </param>
        /// <param name="radixCount">
        /// Number of iterations of the radix sort required.  The radix function will receive 0 to radixCount-1 for its second argument.
        /// </param>
        /// <returns>
        /// An array containing the items in sorted order.
        /// </returns>
        public static T[] RadixSort<T>(this IEnumerable<T> input, RadixFunction<T> radixMap, int min, int max,
            int radixCount)
        {
            if (radixCount <= 0)
                throw new ArgumentException("Value must be greater than zero.", nameof(radixCount));
            IEnumerable<T> current = input;
            for (int i = 0; i < radixCount; i++)
            {
                current = CountingSort(current, value => radixMap(value, i), min, max);
            }
            return (T[])current;
        }

        /// <summary>
        /// Performs an inplace O(n^2) sort, efficient for very small lists.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// List to be sorted.
        /// </param>
        public static void InsertionSort<T>(this IList<T> input)
        {
            InsertionSort(input, null);
        }

        /// <summary>
        /// Performs an inplace O(n^2) sort, efficient for very small lists.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// List to be sorted.
        /// </param>
        /// <param name="comparer">
        /// Comparer for comparing values.  Specify null to use the default comparer.
        /// </param>
        public static void InsertionSort<T>(this IList<T> input, IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            for (int i = 1; i < input.Count; i++)
            {
                T current = input[i];
                int j = i - 1;
                while (j >= 0 && comparer.Compare(input[j], current) > 0)
                {
                    input[j + 1] = input[j];
                    j--;
                }
                input[j + 1] = current;
            }
        }

        /// <summary>
        /// Performs an inplace O(n log n) sort, gauranteed efficient for big lists, but uses recursion and requires additional storage.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// List to be sorted.
        /// </param>
        public static void MergeSort<T>(this IList<T> input)
        {
            MergeSort(input, null);
        }

        /// <summary>
        /// Performs an inplace O(n log n) sort, gauranteed efficient for big lists, but uses recursion and requires additional storage.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// List to be sorted.
        /// </param>
        /// <param name="comparer">
        /// Comparer for comparing values.  Specify null to use the default comparer.
        /// </param>
        public static void MergeSort<T>(this IList<T> input, IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            T[] tmp = new T[input.Count];
            MergeSortInner(input, comparer, 0, input.Count - 1, tmp);
        }

        private static void MergeSortInner<T>(IList<T> input, IComparer<T> comparer, int first, int last, T[] tmp)
        {
            if (first < last)
            {
                int mid = first + ((last - first) >> 1);
                MergeSortInner(input, comparer, first, mid, tmp);
                MergeSortInner(input, comparer, mid + 1, last, tmp);
                Merge(input, comparer, first, mid, last, tmp);
            }
        }

        private static void Merge<T>(IList<T> input, IComparer<T> comparer, int first, int mid, int last, T[] tmp)
        {
            int i = first;
            int j = mid + 1;
            int o = first;
            while (i <= mid && j <= last)
            {
                if (comparer.Compare(input[i], input[j]) <= 0)
                {
                    tmp[o] = input[i];
                    o++;
                    i++;
                }
                else
                {
                    tmp[o] = input[j];
                    o++;
                    j++;
                }
            }
            while (i <= mid)
            {
                tmp[o] = input[i];
                o++;
                i++;
            }
            while (j <= last)
            {
                tmp[o] = input[j];
                o++;
                j++;
            }
            for (int l = first; l <= last; l++)
            {
                input[l] = tmp[l];
            }
        }

        /// <summary>
        /// Performs an inplace O(n log n) sort, gauranteed efficient for big lists, but requires additional storage.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// List to be sorted.
        /// </param>
        public static void MergeSort2<T>(this IList<T> input)
        {
            MergeSort2(input, null);
        }

        /// <summary>
        /// Performs an inplace O(n log n) sort, gauranteed efficient for big lists, but requires additional storage.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to be sorted.
        /// </typeparam>
        /// <param name="input">
        /// List to be sorted.
        /// </param>
        /// <param name="comparer">
        /// Comparer for comparing values.  Specify null to use the default comparer.
        /// </param>
        public static void MergeSort2<T>(this IList<T> input, IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            T[] tmp = new T[input.Count];
            int size = 2;
            while ((size >> 1) < input.Count)
            {
                int start = 0;
                while (start < input.Count)
                {
                    int end = Math.Min(start + size - 1, input.Count - 1);
                    int mid = start + (size >> 1) - 1;
                    if (end > mid)
                        Merge(input, comparer, start, mid, end, tmp);
                    start += size;
                }
                size <<= 1;
            }
        }

        /// <summary>
        /// Given a list of input values, returns an array of integers that have the same sorting characteristics but have minimal range.  Smallest element in return set will be 0.
        /// </summary>
        /// <param name="input">
        /// Input list to be relabeled.
        /// </param>
        /// <typeparam name="T">
        /// Type of element in the input, must be comparable and equality comparable.
        /// </typeparam>
        /// <returns>
        /// An array of integers with the same sorting characteristics as the input.
        /// </returns>
        public static int[] Relabel<T>(this IEnumerable<T> input)
        {
            return Relabel(input, null, null);
        }

        /// <summary>
        /// Given a list of input values, returns an array of integers that have the same sorting characteristics but have minimal range.  Smallest element in return set will be 0.
        /// </summary>
        /// <param name="input">
        /// Input list to be relabeled.
        /// </param>
        /// <param name="comparer">
        /// Comparer used to determine sorting characteristics of the input.
        /// </param>
        /// <param name="equalityComparer">
        /// Equality comparer used for quick look up when relabeling.
        /// </param>
        /// <typeparam name="T">
        /// Type of element in the input.
        /// </typeparam>
        /// <returns>
        /// An array of integers with the same sorting characteristics as the input.
        /// </returns>
        public static int[] Relabel<T>(this IEnumerable<T> input, IComparer<T> comparer,
            IEqualityComparer<T> equalityComparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            if (equalityComparer == null)
                equalityComparer = EqualityComparer<T>.Default;
            T[] sorted = input.Distinct(equalityComparer).ToArray();
            Array.Sort(sorted, comparer);
            Dictionary<T, int> indexes = new Dictionary<T, int>(equalityComparer);
            for (int i = 0; i < sorted.Length; i++)
            {
                indexes.Add(sorted[i], i);
            }
            return input.Select(a => indexes[a]).ToArray();
        }

        /// <summary>
        /// Converts the input to an array, then sorts that array and returns it.
        /// </summary>
        /// <param name="input">
        /// Input to be sorted.
        /// </param>
        /// <typeparam name="T">
        /// Type of elements in the input.
        /// </typeparam>
        /// <returns>
        /// An array containing the input in sorted order.
        /// </returns>
        public static T[] ToSortedArray<T>(this IEnumerable<T> input)
        {
            return ToSortedArray<T>(input, null);
        }

        /// <summary>
        /// Converts the input to an array, then sorts that array and returns it.
        /// </summary>
        /// <param name="input">
        /// Input to be sorted.
        /// </param>
        /// <param name="comparer">
        /// Comparer to use to sort the input, or null for default comparison.
        /// </param>
        /// <typeparam name="T">
        /// Type of elements in the input.
        /// </typeparam>
        /// <returns>
        /// An array containing the input in sorted order.
        /// </returns>
        public static T[] ToSortedArray<T>(this IEnumerable<T> input, IComparer<T> comparer)
        {
            T[] result = input.ToArray();
            Array.Sort(result, comparer);
            return result;
        }
    }

    /// <summary>
    /// Maps a value to an integer.
    /// </summary>
    /// <typeparam name="T">
    /// Type of value.
    /// </typeparam>
    /// <param name="value">
    /// Value to map.
    /// </param>
    /// <remarks>
    /// When used in a counting sort, the result must sort the same value, and the mapping must be distinct for all inputs.
    /// </remarks>
    /// <returns>
    /// An integer corresponding to the value.
    /// </returns>
    public delegate int IntMapFunction<T>(T value);

    /// <summary>
    /// Maps a value to an integer in a specified range.
    /// </summary>
    /// <typeparam name="T">
    /// Type of values to be mapped.
    /// </typeparam>
    /// <param name="value">
    /// Value to be mapped.
    /// </param>
    /// <param name="parts">
    /// Specifies the upper exclusive bound of the results of a given call.
    /// </param>
    /// <remarks>
    /// When used for a bucket sort, the results must sort in the same order as the values, but they need not be distinct.
    /// If they do not distribute evenly, sort performance will be affected.
    /// </remarks>
    /// <returns>
    /// A number between 0 and parts-1 inclusive.
    /// </returns>
    public delegate int PartitionFunction<T>(T value, int parts);

    /// <summary>
    /// Maps values to integers for a radix sort.
    /// </summary>
    /// <typeparam name="T">
    /// Type of values to be mapped.
    /// </typeparam>
    /// <param name="value">
    /// Value to be mapped.
    /// </param>
    /// <param name="radix">
    /// Current radix pass.
    /// </param>
    /// <remarks>
    /// When used for a radix counting sort, the radix function should be equivelent to a counting sort function performed on the 'radix'th least significant part of the input.
    /// Radix will range from 0 to the number of passes - 1.
    /// </remarks>
    /// <returns>
    /// An integer which corresponds to the pass.
    /// </returns>
    public delegate int RadixFunction<T>(T value, int radix);
}