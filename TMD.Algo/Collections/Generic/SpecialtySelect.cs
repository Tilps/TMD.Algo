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
using System.Diagnostics;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides helper methods for selecting the nth smallest item in an unordered list.
    /// </summary>
    public static class SpecialtySelect
    {

        /// <summary>
        /// Finds the minimum element in the input.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data in the list.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable list of items to find the minimum.
        /// </param>
        /// <returns>
        /// The smallest item if the list contains any items.
        /// </returns>
        public static T FindMinimum<T>(this IEnumerable<T> input)
        {
            return FindMinimum(input, null);
        }

        /// <summary>
        /// Finds the minimum element in the input.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data in the list.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable list of items to find the minimum.
        /// </param>
        /// <param name="comparer">
        /// Comparer to use to compare items in the input.  Specify null to use the default comparer.
        /// </param>
        /// <returns>
        /// The smallest item if the list contains any items.
        /// </returns>
        public static T FindMinimum<T>(this IEnumerable<T> input, IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            T result = default(T);
            bool first = true;
            foreach (T value in input)
            {
                if (first)
                {
                    result = value;
                    first = false;
                }
                else if (comparer.Compare(result, value) > 0)
                    result = value;
            }
            if (first)
                throw new ArgumentException("'this' must not be an empty list.", "input");
            return result;
        }

        /// <summary>
        /// Finds the maximum element in the input.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data in the list.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable list of items to find the maximum.
        /// </param>
        /// <returns>
        /// The largest item if the list contains any items.
        /// </returns>
        public static T FindMaximum<T>(this IEnumerable<T> input)
        {
            return FindMaximum(input, null);
        }

        /// <summary>
        /// Finds the maximum element in the input.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data in the list.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable list of items to find the maximum.
        /// </param>
        /// <param name="comparer">
        /// Comparer to use to compare items in the input.  Specify null to use the default comparer.
        /// </param>
        /// <returns>
        /// The largest item if the list contains any items.
        /// </returns>
        public static T FindMaximum<T>(this IEnumerable<T> input, IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            T result = default(T);
            bool first = true;
            foreach (T value in input)
            {
                if (first)
                {
                    result = value;
                    first = false;
                }
                else if (comparer.Compare(result, value) < 0)
                    result = value;
            }
            if (first)
                throw new ArgumentException("'this' must not be an empty list.", "input");
            return result;
        }

        /// <summary>
        /// Selects the nth smallest item from an unordered list.
        /// This action reorders the specified input partially.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to select from.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable list of items to select from.
        /// </param>
        /// <param name="selection">
        /// Provides the choice of which smallest item to choose.  0 based.
        /// </param>
        /// <returns>
        /// The nth smallest item from the given input.
        /// </returns>
        public static T SelectRandomizedInPlace<T>(this IList<T> input, int selection)
        {
            return SelectRandomizedInPlace(input, selection, null);
        }

        /// <summary>
        /// Selects the nth smallest item from an unordered list.
        /// This action reorders the specified input partially.
        /// </summary>
        /// <typeparam name="T">
        /// Type of data to select from.
        /// </typeparam>
        /// <param name="input">
        /// Enumerable list of items to select from.
        /// </param>
        /// <param name="selection">
        /// Provides the choice of which smallest item to choose.  0 based.
        /// </param>
        /// <param name="comparer">
        /// Specifies the comparer to determine order in the unordered list.  Specify null to use the default comparer.
        /// </param>
        /// <returns>
        /// The nth smallest item from the given input.
        /// </returns>
        public static T SelectRandomizedInPlace<T>(this IList<T> input, int selection, IComparer<T> comparer)
        {
            if (selection < 0 || selection >= input.Count)
                throw new ArgumentOutOfRangeException("selection", "Selection must be non-negative and less than the size of the input.");
            if (comparer == null)
                comparer = Comparer<T>.Default;
            Random rnd = new Random();
            int currentStart = 0;
            int currentEnd = input.Count - 1;
            while (currentStart < currentEnd)
            {
                int endPoint = RandomizedPartition<T>(input, comparer, rnd, currentStart, currentEnd);
                int pivotOffset = endPoint-currentStart+1;
                if (selection < pivotOffset)
                {
                    currentEnd = endPoint;
                }
                else 
                {
                    currentStart = endPoint+1;
                    selection -= pivotOffset;
                }
            }

            return input[currentStart+selection];
        }

        private static int RandomizedPartition<T>(IList<T> input, IComparer<T> comparer, Random rnd, int currentStart, int currentEnd)
        {
            int randomPos = currentStart + rnd.Next(currentEnd - currentStart + 1);
            T tmp = input[randomPos];
            input[randomPos] = input[currentStart];
            input[currentStart] = tmp;
            int startPoint = currentStart - 1;
            int endPoint = currentEnd + 1;
            while (true)
            {
                do
                {
                    endPoint--;
                } while (comparer.Compare(input[endPoint], tmp) > 0);
                do
                {
                    startPoint++;
                } while (comparer.Compare(input[startPoint], tmp) < 0);
                if (startPoint < endPoint)
                {
                    T tmp2 = input[startPoint];
                    input[startPoint] = input[endPoint];
                    input[endPoint] = tmp2;
                }
                else
                {
                    break;
                }
            }
            return endPoint;
        }

        /// <summary>
        /// Partitions the given list arround the first entry, all items less than it are listed first,greater than it are listed last.
        /// Equal items can be listed anywhere.
        /// </summary>
        /// <typeparam name="T">
        /// The type of items in the input list.
        /// </typeparam>
        /// <param name="input">
        /// List to partition.
        /// </param>
        /// <param name="comparer">
        /// Comparer to compare items in the list, specify null to use the default comparer.
        /// </param>
        /// <param name="currentStart">
        /// First entry in the list to participate in the partition.
        /// </param>
        /// <param name="currentEnd">
        /// Last entry in the list to participate in the partition.
        /// </param>
        /// <returns>
        /// The index of the last node in the less than or equal to section of the partition.
        /// Note: There may still be more equal items after this index.
        /// </returns>
        public static int Partition<T>(IList<T> input, IComparer<T> comparer, int currentStart, int currentEnd)
        {
            if (currentStart < 0 || currentStart >= input.Count)
                throw new ArgumentOutOfRangeException("currentStart", "Start index must be in the input.");
            if (currentEnd < 0 || currentEnd >= input.Count)
                throw new ArgumentOutOfRangeException("currentEnd", "End index must be in the input.");
            T tmp = input[currentStart];
            int startPoint = currentStart - 1;
            int endPoint = currentEnd + 1;
            while (true)
            {
                do
                {
                    endPoint--;
                } while (comparer.Compare(input[endPoint], tmp) > 0);
                do
                {
                    startPoint++;
                } while (comparer.Compare(input[startPoint], tmp) < 0);
                if (startPoint < endPoint)
                {
                    T tmp2 = input[startPoint];
                    input[startPoint] = input[endPoint];
                    input[endPoint] = tmp2;
                }
                else
                {
                    break;
                }
            }
            return endPoint;
        }

        /// <summary>
        /// Performs a generic binary search over an integer function.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="comparer">
        /// Search function.
        /// </param>
        /// <returns>
        /// The value which satisfies comparer, or a number less than min if none exists.
        /// </returns>
        public static long BinarySearch(long min, long max, Func<long, int> comparer)
        {
            long origMin = min;
            while (min <= max)
            {
                long mid = min + (max - min) / 2;
                int comparison = comparer(mid);
                if (comparison == 0)
                    return mid;
                if (comparison < 0)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }
            return origMin - 1;
        }

        /// <summary>
        /// Performs a generic binary search over a real function.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="comparer">
        /// Search function.
        /// </param>
        /// <returns>
        /// The value which satisfies comparer, NaN if none exists.
        /// </returns>
        public static double BinarySearch(double min, double max, Func<double, int> comparer)
        {
            bool earlyOut;
            // double has 64 bits, each repition halves the range, so 64 reps should always reduce min/max to an empty range.
            // Add a few more to handle potential corner cases.
            double result = BinarySearch(min, max, comparer, 70, out earlyOut);
            Debug.Assert(!earlyOut);
            return result;
        }

        /// <summary>
        /// Performs a generic binary search over a real function.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="comparer">
        /// Search function.
        /// </param>
        /// <param name="maxDepth">
        /// Maximum number of iterations before giving up.
        /// </param>
        /// <param name="earlyOut">
        /// Returns whether maxDepth was reached.
        /// </param>
        /// <returns>
        /// The value which satisfies comparer, NaN if none exists, or the current range midpoint if maxDepth is reached.
        /// </returns>
        public static double BinarySearch(double min, double max, Func<double, int> comparer, int maxDepth, out bool earlyOut)
        {
            earlyOut = false;
            while (min <= max)
            {
                double mid = min + (max - min) / 2;
                maxDepth--;
                if (maxDepth < 0)
                {
                    earlyOut = true;
                    return mid;
                }
                int comparison = comparer(mid);
                if (comparison == 0)
                    return mid;
                // If there is no range left, and we've checked the remaining point, no point to continue searching.
                if (max == min)
                {
                    return double.NaN;
                }
                if (comparison < 0)
                {
                    // If adding half the difference has made it max, the only value that needs checking is min, there is nothing left in between.
                    if (max == mid)
                    {
                        max = min;
                    }
                    else
                    {
                        max = mid;
                    }
                }
                else
                {
                    // If adding half the difference has left it as min, the only value that needs checking is max, there is nothing left in between.
                    if (min == mid)
                    {
                        min = max;
                    }
                    else
                    {
                        min = mid;
                    }
                }
            }
            return double.NaN;
        }

        /// <summary>
        /// Performs a generic binary search over a real function.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="delta">
        /// Smallest detail level considered.  If the width of the comparison function zero output is less than delta, it may not be found.
        /// Max +/- delta must != max and min +/- delta must != min or behavior is undefined.
        /// </param>
        /// <param name="comparer">
        /// Search function.
        /// </param>
        /// <returns>
        /// The value which satisfies comparer, NaN if none can be found given the delta.
        /// </returns>
        public static double BinarySearch(double min, double max, double delta, Func<double, int> comparer)
        {
            while (min <= max)
            {
                double mid = min + (max - min) / 2;
                int comparison = comparer(mid);
                if (comparison == 0)
                    return mid;
                if (comparison < 0)
                {
                    max = mid - delta;
                }
                else
                {
                    min = mid + delta;
                }
            }
            return double.NaN;
        }

        /// <summary>
        /// Assuming the input function has a single transition point from false to true, or true to false in the given range, returns x where f(x) != f(x+1).
        /// Performs a variant of binary search.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="stepFunc">
        /// Function which has a single transition point.
        /// </param>
        /// <returns>
        /// The value location of the start of the transition.
        /// </returns>
        public static long FindTransition(long min, long max, Func<long, bool> stepFunc)
        {
            bool start = stepFunc(min);
            bool end = stepFunc(max);
            if (start == end) throw new ArgumentException("Step function is flat, or has multiple transitions.");
            // TODO: consider whether the chance of early out is better worth the extra 50% extra evaluations a true binary search brings vs a custom search which keeps going till min/max meet.
            return BinarySearch(min, max - 1, value =>
            {
                bool first = stepFunc(value);
                if (first == end) return 1;
                bool second = stepFunc(value + 1);
                if (second == start) return -1;
                return 0;
            });
        }

        /// <summary>
        /// Assuming the input function has a single transition point from false to true, or true to false in the given range, returns an x where f(x) != f(x+delta).
        /// Performs a variant of binary search.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="stepFunc">
        /// Function which has a single transition point.
        /// </param>
        /// <param name="delta">
        /// Accuracy for the identification of transition.  Must be significant enough that min +/- delta != min and max +/- delta != max.
        /// </param>
        /// <returns>
        /// The value location of the start of the transition.
        /// </returns>
        public static double FindTransition(double min, double max, double delta, Func<double, bool> stepFunc)
        {
            bool start = stepFunc(min);
            bool end = stepFunc(max);
            if (start == end) throw new ArgumentException("Step function is flat, or has multiple transitions.");
            // TODO: consider whether the chance of early out is better worth the extra 50% extra evaluations a true binary search brings vs a custom search which keeps going till min/max meet.
            return BinarySearch(min, max, delta, value =>
            {
                bool first = stepFunc(value);
                if (first == end) return 1;
                bool second = stepFunc(value + delta);
                if (second == start) return -1;
                return 0;
            });
        }

        /// <summary>
        /// Assuming the input function has a single transition point from false to true, or true to false in the given range, returns an x where f(x) != f(x+delta) for minimal delta.
        /// Performs a variant of binary search.
        /// </summary>
        /// <param name="min">
        /// Minimum value to search over.
        /// </param>
        /// <param name="max">
        /// Maximum value to search over.
        /// </param>
        /// <param name="stepFunc">
        /// Function which has a single transition point.
        /// </param>
        /// <returns>
        /// The value location of the start of the transition.
        /// </returns>
        public static double FindTransition(double min, double max, Func<double, bool> stepFunc)
        {
            bool start = stepFunc(min);
            bool end = stepFunc(max);
            if (start == end) throw new ArgumentException("Step function is flat, or has multiple transitions.");
            while (min < max)
            {
                double mid = min + (max - min) / 2;
                if (mid == min || mid == max) return min;
                bool midValue = stepFunc(mid);
                if (midValue == end) max = mid;
                else min = mid;
            }
            // This should never be reached.
            Debug.Assert(false);
            return min;
        }

        /// <summary>
        /// Finds the minimum or maximum of a function at integer locations under the assumption that evaluator is a smooth function with at most one turning point over the range.
        /// If the function is flat, the result is undefined and a number less than min will be returned.
        /// </summary>
        /// <param name="min">
        /// Minimum value for the range to search.
        /// </param>
        /// <param name="max">
        /// Maximum value for the range to search.
        /// </param>
        /// <param name="evaluator">
        /// Evaluator function to compare values.
        /// </param>
        /// <param name="findMax">
        /// If true the maximum is found, otherwise the minimum.
        /// </param>
        /// <returns>
        /// The location of the minimum or maximum if one exists, otherwise a number less than min.
        /// </returns>
        public static long TernarySearch(long min, long max, Func<long, double> evaluator, bool findMax)
        {
            long origMin = min;
            long origMax = max;
            double bestEdge = findMax ? Math.Max(evaluator(min), evaluator(max)) : Math.Min(evaluator(min), evaluator(max));
            while (min +3 <= max)
            {
                long mid1 = min + (max - min) / 3;
                long mid2 = min + (max - min) * 2 / 3;
                double first = evaluator(mid1);
                double second = evaluator(mid2);
                if (findMax)
                {
                    if (first > second)
                    {
                        max = mid2;
                    }
                    else if (second > first)
                    {
                        min = mid1;
                    }
                    else if (first > bestEdge)
                    {
                        max = mid2;
                        min = mid1;
                    }
                    else if (bestEdge > first)
                    {
                        // TODO: if evaluator(min) == evaluator(max) return min - 1 or not?
                        return evaluator(origMin) > evaluator(origMax) ? origMin : origMax;
                    }
                    else
                    {
                        return origMin - 1;
                    }
                }
                else
                {
                    if (first > second)
                    {
                        min = mid1;
                    }
                    else if (second > first)
                    {
                        max = mid2;
                    }
                    else if (first < bestEdge)
                    {
                        max = mid2;
                        min = mid1;
                    }
                    else if (bestEdge < first)
                    {
                        // TODO: if evaluator(min) == evaluator(max) return min - 1 or not?
                        return evaluator(origMin) < evaluator(origMax) ? origMin : origMax;
                    }
                    else
                    {
                        return origMin - 1;
                    }
                }
            }
            // We are down to 4 or less values, all that remains is to return the min/max of those values.
            long bestIndex = -1;
            double bestValue = findMax ? double.MinValue : double.MaxValue;
            for (long i = min; i <= max; i++)
            {
                if (findMax)
                {
                    if (evaluator(i) > bestValue)
                    {
                        bestValue = evaluator(i);
                        bestIndex = i;
                    }
                }
                else
                {
                    if (evaluator(i) < bestValue)
                    {
                        bestValue = evaluator(i);
                        bestIndex = i;
                    }
                }
            }
            return bestIndex;
        }

        /// <summary>
        /// Finds the minimum or maximum of a function over reals under the assumption that evaluator is a smooth function with at most one turning point over the range.
        /// If the function is flat, the result is undefined and NaN will be returned
        /// </summary>
        /// <param name="min">
        /// Minimum value for the range to search.
        /// </param>
        /// <param name="max">
        /// Maximum value for the range to search.
        /// </param>
        /// <param name="evaluator">
        /// Evaluator function to compare values.
        /// </param>
        /// <param name="findMax">
        /// If true the maximum is found, otherwise the minimum.
        /// </param>
        /// <param name="maxDepth">
        /// Maximum number of repetitions to try when looking for the extrema.
        /// </param>
        /// <param name="reachedLimit">
        /// Outputs true if the maximum number of repetitions was reached without convergence, false otherwise.
        /// </param>
        /// <returns>
        /// The location of the minimum or maximum if one exists, otherwise NaN.
        /// </returns>
        public static double TernarySearch(double min, double max, Func<double, double> evaluator, bool findMax, int maxDepth, out bool reachedLimit)
        {
            reachedLimit = false;
            double origMin = min;
            double origMax = max;
            double bestEdge = findMax ? Math.Max(evaluator(min), evaluator(max)) : Math.Min(evaluator(min), evaluator(max));
            while (min < max && maxDepth > 0)
            {
                maxDepth--;
                double mid1 = min + (max - min) / 3;
                double mid2 = min + (max - min) * 2 / 3;
                double first = evaluator(mid1);
                double second = evaluator(mid2);
                if (findMax)
                {
                    if (first > second)
                    {
                        max = mid2;
                    }
                    else if (second > first)
                    {
                        min = mid1;
                    }
                    else if (first > bestEdge)
                    {
                        max = mid2;
                        min = mid1;
                    }
                    else if (bestEdge > first)
                    {
                        // TODO: if evaluator(min) == evaluator(max) return NaN or not?
                        return evaluator(origMin) > evaluator(origMax) ? origMin : origMax;
                    }
                    else
                    {
                        return double.NaN;
                    }
                }
                else
                {
                    if (first > second)
                    {
                        min = mid1;
                    }
                    else if (second > first)
                    {
                        max = mid2;
                    }
                    else if (first < bestEdge)
                    {
                        max = mid2;
                        min = mid1;
                    }
                    else if (bestEdge < first)
                    {
                        // TODO: if evaluator(min) == evaluator(max) return NaN or not?
                        return evaluator(origMin) < evaluator(origMax) ? origMin : origMax;
                    }
                    else
                    {
                        return double.NaN;
                    }
                }
            }
            reachedLimit = min != max;
            return min + (max - min) / 2;
        }
    }
}
