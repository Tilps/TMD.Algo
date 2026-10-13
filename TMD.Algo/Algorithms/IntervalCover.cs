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
    /// Provides methods for solving the discrete interval cover problem.
    /// Given a target range [targetStart, targetEnd] and a collection of candidate intervals,
    /// finds the minimum number of intervals needed to completely cover the target range.
    /// </summary>
    public static class IntervalCover
    {
        /// <summary>
        /// Finds the minimum number of discrete intervals needed to cover [targetStart, targetEnd].
        /// </summary>
        /// <typeparam name="T">Type of the interval items.</typeparam>
        /// <param name="targetStart">Start of the target range (inclusive).</param>
        /// <param name="targetEnd">End of the target range (inclusive).</param>
        /// <param name="intervals">Candidate intervals.</param>
        /// <param name="getStart">Function extracting the start coordinate (inclusive).</param>
        /// <param name="getEnd">Function extracting the end coordinate (inclusive).</param>
        /// <returns>The minimum count of intervals, or -1 if the target range cannot be covered.</returns>
        public static int MinIntervalsToCover<T>(
            int targetStart,
            int targetEnd,
            IEnumerable<T> intervals,
            Func<T, int> getStart,
            Func<T, int> getEnd)
        {
            var cover = FindMinimumCover(targetStart, targetEnd, intervals, getStart, getEnd);
            return cover?.Count ?? -1;
        }

        /// <summary>
        /// Finds the minimum number of discrete intervals needed to cover [targetStart, targetEnd].
        /// </summary>
        /// <param name="targetStart">Start of the target range (inclusive).</param>
        /// <param name="targetEnd">End of the target range (inclusive).</param>
        /// <param name="intervals">Candidate intervals as (Start, End) pairs.</param>
        /// <returns>The minimum count of intervals, or -1 if the target range cannot be covered.</returns>
        public static int MinIntervalsToCover(
            int targetStart,
            int targetEnd,
            IEnumerable<(int Start, int End)> intervals)
        {
            return MinIntervalsToCover(targetStart, targetEnd, intervals, x => x.Start, x => x.End);
        }

        /// <summary>
        /// Finds the minimum number of discrete intervals needed to cover [targetStart, targetEnd].
        /// </summary>
        /// <param name="targetStart">Start of the target range (inclusive).</param>
        /// <param name="targetEnd">End of the target range (inclusive).</param>
        /// <param name="intervals">Candidate intervals as KeyValuePair&lt;int, int&gt; where Key is start and Value is end.</param>
        /// <returns>The minimum count of intervals, or -1 if the target range cannot be covered.</returns>
        public static int MinIntervalsToCover(
            int targetStart,
            int targetEnd,
            IEnumerable<KeyValuePair<int, int>> intervals)
        {
            return MinIntervalsToCover(targetStart, targetEnd, intervals, x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Finds a minimal subset of discrete intervals that completely covers [targetStart, targetEnd].
        /// </summary>
        /// <param name="targetStart">Start of the target range (inclusive).</param>
        /// <param name="targetEnd">End of the target range (inclusive).</param>
        /// <param name="intervals">Candidate intervals as (Start, End) pairs.</param>
        /// <returns>A list of chosen intervals, or null if the target range cannot be covered.</returns>
        public static List<(int Start, int End)>? FindMinimumCover(
            int targetStart,
            int targetEnd,
            IEnumerable<(int Start, int End)> intervals)
        {
            return FindMinimumCover(targetStart, targetEnd, intervals, x => x.Start, x => x.End);
        }

        /// <summary>
        /// Finds a minimal subset of discrete intervals that completely covers [targetStart, targetEnd].
        /// </summary>
        /// <param name="targetStart">Start of the target range (inclusive).</param>
        /// <param name="targetEnd">End of the target range (inclusive).</param>
        /// <param name="intervals">Candidate intervals as KeyValuePair&lt;int, int&gt; where Key is start and Value is end.</param>
        /// <returns>A list of chosen intervals, or null if the target range cannot be covered.</returns>
        public static List<KeyValuePair<int, int>>? FindMinimumCover(
            int targetStart,
            int targetEnd,
            IEnumerable<KeyValuePair<int, int>> intervals)
        {
            return FindMinimumCover(targetStart, targetEnd, intervals, x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Finds a minimal subset of discrete intervals that completely covers [targetStart, targetEnd].
        /// </summary>
        /// <typeparam name="T">Type of the interval items.</typeparam>
        /// <param name="targetStart">Start of the target range (inclusive).</param>
        /// <param name="targetEnd">End of the target range (inclusive).</param>
        /// <param name="intervals">Candidate intervals.</param>
        /// <param name="getStart">Function extracting the start coordinate (inclusive).</param>
        /// <param name="getEnd">Function extracting the end coordinate (inclusive).</param>
        /// <returns>A list of chosen intervals, or null if the target range cannot be covered.</returns>
        public static List<T>? FindMinimumCover<T>(
            int targetStart,
            int targetEnd,
            IEnumerable<T> intervals,
            Func<T, int> getStart,
            Func<T, int> getEnd)
        {
            if (intervals == null) throw new ArgumentNullException(nameof(intervals));
            if (getStart == null) throw new ArgumentNullException(nameof(getStart));
            if (getEnd == null) throw new ArgumentNullException(nameof(getEnd));

            if (targetStart > targetEnd)
                return new List<T>();

            var list = new List<T>();
            foreach (var item in intervals)
            {
                // Discard intervals entirely outside the target range
                if (getEnd(item) >= targetStart && getStart(item) <= targetEnd)
                    list.Add(item);
            }

            list.Sort((a, b) =>
            {
                int cmp = getStart(a).CompareTo(getStart(b));
                return cmp != 0 ? cmp : getEnd(b).CompareTo(getEnd(a));
            });

            var result = new List<T>();
            int covered = targetStart - 1;
            int idx = 0;

            while (covered < targetEnd)
            {
                int bestEnd = covered;
                int bestIdx = -1;

                while (idx < list.Count && getStart(list[idx]) <= covered + 1)
                {
                    int end = getEnd(list[idx]);
                    if (end > bestEnd)
                    {
                        bestEnd = end;
                        bestIdx = idx;
                    }
                    idx++;
                }

                if (bestIdx < 0)
                    return null; // Cannot cover next point

                result.Add(list[bestIdx]);
                covered = bestEnd;
            }

            return result;
        }
    }
}

