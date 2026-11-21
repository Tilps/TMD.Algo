#region License

/*
Copyright (c) 2012, the TMD.Algo authors.
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
    /// Tracks which set each item belongs to after a set of union operations to merge disjoint sets.
    /// This is very efficient, it can be consider that each operation takes close to O(1) time on average.
    /// This variant has additional amortized O(1) overhead per operation in order to add the revert function.
    /// The revert function returns the class's visible state to before the last modification.
    /// This allows for use in trial based algorithms where we need to revert to before the trial.
    /// </summary>
    /// <typeparam name="T">
    /// Type of item being tracked.
    /// </typeparam>
    public class RevertibleDisjointTracker<T>
        where T : notnull
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RevertibleDisjointTracker() : this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Equality comparer to use to lookup entries in the internal storage.
        /// </param>
        public RevertibleDisjointTracker(IEqualityComparer<T>? comparer)
        {
            comparer ??= EqualityComparer<T>.Default;
            this.comparer = comparer;
            tracker = [with(comparer)];
            ranker = [with(comparer)];
        }


        private readonly Dictionary<T, T> tracker;
        private readonly Dictionary<T, int> ranker;
        private readonly IEqualityComparer<T> comparer;
        private readonly List<List<Action>> undoLists = [];

        private List<Action> CurrentUndo => undoLists[^1];

        /// <summary>
        /// Undoes all changes to this data structure since the start of the last call to Add or Link.
        /// </summary>
        public void Revert()
        {
            List<Action> toUndo = CurrentUndo;
            for (int i = toUndo.Count - 1; i >= 0; i--)
                toUndo[i]();
        }


        /// <summary>
        /// Adds a new item to be tracked.  Initially it is in a set of its own.
        /// </summary>
        /// <param name="value">
        /// Value to be added to the tracking.
        /// </param>
        public void Add(T value)
        {
            undoLists.Add([]);
            CurrentUndo.Add(() => { undoLists.RemoveAt(undoLists.Count - 1); });
            tracker[value] = value;
            ranker[value] = 0;
            CurrentUndo.Add(() =>
            {
                tracker.Remove(value);
                ranker.Remove(value);
            });
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
            undoLists.Add(new List<Action>());
            CurrentUndo.Add(() => { undoLists.RemoveAt(undoLists.Count - 1); });
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
        /// True if <paramref name="first"/> and <paramref name="second"/> have the same representative; otherwise false.
        /// </returns>
        public bool AreConnected(T first, T second)
        {
            return comparer.Equals(GetRepresentative(first), GetRepresentative(second));
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
            T parent = tracker[value];
            if (comparer.Equals(parent, value))
                return parent;
            T realParent = GetRepresentative(parent);
            if (!comparer.Equals(realParent, parent))
            {
                tracker[value] = realParent;
                CurrentUndo.Add(() => { tracker[value] = parent; });
            }
            return realParent;
        }

        private bool Link(T first, T second, out T survivingRoot, out T absorbedRoot)
        {
            if (comparer.Equals(first, second))
            {
                survivingRoot = first;
                absorbedRoot = first;
                return false;
            }
            int firstRank = ranker[first];
            int secondRank = ranker[second];
            if (firstRank > secondRank)
            {
                bool exists = tracker.TryGetValue(second, out var oldTracker);
                tracker[second] = first;
                CurrentUndo.Add(() =>
                {
                    if (exists) tracker[second] = oldTracker!;
                    else tracker.Remove(second);
                });
                survivingRoot = first;
                absorbedRoot = second;
            }
            else
            {
                if (firstRank == secondRank)
                {
                    ranker[second] = secondRank + 1;
                    CurrentUndo.Add(() => { ranker[second] = secondRank; });
                }
                bool exists = tracker.TryGetValue(first, out var oldTracker);
                tracker[first] = second;
                CurrentUndo.Add(() =>
                {
                    if (exists) tracker[first] = oldTracker!;
                    else tracker.Remove(first);
                });
                survivingRoot = second;
                absorbedRoot = first;
            }
            return true;
        }
    }
}