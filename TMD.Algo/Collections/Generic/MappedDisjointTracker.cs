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

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Tracks which set each item belongs to after a set of union operations to merge disjoint sets.
    /// This is very efficient, it can be consider that each operation takes close to O(1) time on average.
    /// This variation uses an int mapping function to allow for more efficient storage.
    /// The mapping function must be a bijection and should be very fast to calculate.
    /// </summary>
    /// <typeparam name="T">
    /// Type of item being tracked.
    /// </typeparam>
    public class MappedDisjointTracker<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MappedDisjointTracker(IntMapFunction<T> map, int min, int max)
        {
            ArgumentNullException.ThrowIfNull(map);
            tracker = new T[max - min + 1];
            ranker = new int[max - min + 1];
            this.min = min;
            this.map = map;
        }

        private readonly T[] tracker;
        private readonly int[] ranker;
        private readonly IntMapFunction<T> map;
        private readonly int min;

        /// <summary>
        /// Adds a new item to be tracked.  Initially it is in a set of its own.
        /// </summary>
        /// <param name="value">
        /// Value to be added to the tracking.
        /// </param>
        public void Add(T value)
        {
            tracker[map(value) - min] = value;
            ranker[map(value) - min] = 0;
        }

        /// <summary>
        /// Unions the two sets which contain the specified items.  Does nothing if the items are already in the same set.
        /// </summary>
        /// <param name="first">
        /// First item to find set to union.
        /// </param>
        /// <param name="second">
        /// Second item to find set to union.
        /// </param>
        /// <returns>
        /// True if the two items belonged to different sets and were merged; false if they were already in the same set.
        /// </returns>
        public bool Union(T first, T second)
        {
            return Union(first, second, out _, out _);
        }

        /// <summary>
        /// Unions the two sets which contain the specified items and outputs the surviving and absorbed representatives.
        /// </summary>
        /// <param name="first">
        /// First item to find set to union.
        /// </param>
        /// <param name="second">
        /// Second item to find set to union.
        /// </param>
        /// <param name="survivingRoot">
        /// When this method returns, contains the representative of the resulting set.
        /// </param>
        /// <param name="absorbedRoot">
        /// When this method returns, contains the former representative that was merged into <paramref name="survivingRoot"/>,
        /// or <paramref name="survivingRoot"/> if the two items were already in the same set.
        /// </param>
        /// <returns>
        /// True if the two items belonged to different sets and were merged; false if they were already in the same set.
        /// </returns>
        public bool Union(T first, T second, out T survivingRoot, out T absorbedRoot)
        {
            return Link(GetRepresentative(first), GetRepresentative(second), out survivingRoot, out absorbedRoot);
        }

        /// <summary>
        /// Determines whether two tracked items currently belong to the same set.
        /// </summary>
        /// <param name="first">
        /// First item to check.
        /// </param>
        /// <param name="second">
        /// Second item to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="first"/> and <paramref name="second"/> have the same mapped representative; otherwise false.
        /// </returns>
        public bool AreConnected(T first, T second)
        {
            return map(GetRepresentative(first)) == map(GetRepresentative(second));
        }

        /// <summary>
        /// Gets the primary member of the set which a given value belongs to.
        /// </summary>
        /// <param name="value">
        /// Value to lookup.
        /// </param>
        /// <returns>
        /// The primary member of the set that the value is currently a member of.
        /// </returns>
        public T GetRepresentative(T value)
        {
            int valuePoint = map(value) - min;
            T parent = tracker[valuePoint];
            int parentPoint = map(parent) - min;
            if (valuePoint == parentPoint)
                return parent;
            T realParent = GetRepresentative(parent);
            tracker[valuePoint] = realParent;
            return realParent;
        }

        private bool Link(T first, T second, out T survivingRoot, out T absorbedRoot)
        {
            int firstPoint = map(first) - min;
            int secondPoint = map(second) - min;
            if (secondPoint == firstPoint)
            {
                survivingRoot = first;
                absorbedRoot = first;
                return false;
            }
            int firstRank = ranker[firstPoint];
            int secondRank = ranker[secondPoint];
            if (firstRank > secondRank)
            {
                tracker[secondPoint] = first;
                survivingRoot = first;
                absorbedRoot = second;
            }
            else
            {
                if (firstRank == secondRank)
                    ranker[secondPoint] = secondRank + 1;
                tracker[firstPoint] = second;
                survivingRoot = second;
                absorbedRoot = first;
            }
            return true;
        }
    }
}