#region License

/*
Copyright (c) 2015, the TMD.Algo authors.
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
    /// Algorithms which search on an implicit graph.
    /// </summary>
    public static class Search
    {
        /// <summary>
        /// Determines the shortest path between first and last using breadth first search.
        /// </summary>
        /// <param name="first">
        /// Starting state.
        /// </param>
        /// <param name="last">
        /// Target state.
        /// </param>
        /// <param name="neighbourFunc">
        /// Function for determining potential next states for a given state.
        /// </param>
        /// <param name="pathObserver">
        /// Optional action to observe the path from first to last.
        /// </param>
        /// <param name="equalityComparer">
        /// Equality comparer to determine when last is found or states have been seen.
        /// </param>
        /// <typeparam name="T">
        /// Type of state being searched over.
        /// </typeparam>
        /// <returns>
        /// The number of steps to get from first to last. -1 if no path exists.
        /// </returns>
        public static int Bfs<T>(T first, T last, Func<T, IEnumerable<T>> neighbourFunc, Action<T> pathObserver,
            IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            return Bfs(first, other => equalityComparer.Equals(last, other), neighbourFunc, pathObserver,
                equalityComparer);
        }

        /// <summary>
        /// Determines the shortest path between first and 'last' using breadth first search.
        /// </summary>
        /// <param name="first">
        /// Starting state.
        /// </param>
        /// <param name="lastFoundFunc">
        /// Function for determing whether the search has ended.
        /// </param>
        /// <param name="neighbourFunc">
        /// Function for determining potential next states for a given state.
        /// </param>
        /// <param name="pathObserver">
        /// Optional action to observe the path from first to last.
        /// </param>
        /// <param name="equalityComparer">
        /// Equality comparer to determine when last is found or states have been seen.
        /// </param>
        /// <typeparam name="T">
        /// Type of state being searched over.
        /// </typeparam>
        /// <returns>
        /// The number of steps to get from first to last. -1 if no path exists.
        /// </returns>
        public static int Bfs<T>(T first, Func<T, bool> lastFoundFunc, Func<T, IEnumerable<T>> neighbourFunc,
            Action<T> pathObserver, IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (lastFoundFunc(first))
            {
                pathObserver?.Invoke(first);
                return 0;
            }
            Queue<T> toSearch = new Queue<T>();
            toSearch.Enqueue(first);
            Dictionary<T, KeyValuePair<T, int>> seen = new Dictionary<T, KeyValuePair<T, int>>(equalityComparer)
            {
                {first, new KeyValuePair<T, int>(default(T), 0)}
            };
            while (toSearch.Count != 0)
            {
                T cur = toSearch.Dequeue();
                int depth = seen[cur].Value;
                foreach (T next in neighbourFunc(cur))
                {
                    if (!seen.ContainsKey(next))
                    {
                        if (lastFoundFunc(next))
                        {
                            if (pathObserver != null)
                            {
                                List<T> path = new List<T> {next};
                                while (true)
                                {
                                    path.Add(cur);
                                    if (equalityComparer.Equals(cur, first))
                                    {
                                        break;
                                    }
                                    cur = seen[cur].Key;
                                }
                                path.Reverse();
                                path.ForEach(pathObserver);
                            }
                            return depth + 1;
                        }
                        toSearch.Enqueue(next);
                        seen.Add(next, new KeyValuePair<T, int>(cur, depth + 1));
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Performs a breadth first search traversal of an implicit graph starting from a specified state.
        /// </summary>
        /// <param name="first">Starting state.</param>
        /// <param name="visitor">Action to execute for each reachable state.</param>
        /// <param name="neighbourFunc">Function for determining potential next states for a given state.</param>
        /// <param name="equalityComparer">Equality comparer to determine when states have been seen.</param>
        /// <typeparam name="T">Type of state being searched over.</typeparam>
        public static void Bfs<T>(T first, Action<T> visitor, Func<T, IEnumerable<T>> neighbourFunc,
            IEqualityComparer<T> equalityComparer = null)
        {
            Bfs(new[] { first }, visitor, neighbourFunc, equalityComparer);
        }

        /// <summary>
        /// Performs a breadth first search traversal of an implicit graph starting from a collection of states.
        /// </summary>
        /// <param name="initialStates">Starting states.</param>
        /// <param name="visitor">Action to execute for each reachable state.</param>
        /// <param name="neighbourFunc">Function for determining potential next states for a given state.</param>
        /// <param name="equalityComparer">Equality comparer to determine when states have been seen.</param>
        /// <typeparam name="T">Type of state being searched over.</typeparam>
        public static void Bfs<T>(IEnumerable<T> initialStates, Action<T> visitor, Func<T, IEnumerable<T>> neighbourFunc,
            IEqualityComparer<T> equalityComparer = null)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            Queue<T> toSearch = new Queue<T>();
            HashSet<T> seen = new HashSet<T>(equalityComparer);
            if (initialStates != null)
            {
                foreach (T state in initialStates)
                {
                    if (seen.Add(state))
                    {
                        toSearch.Enqueue(state);
                    }
                }
            }
            while (toSearch.Count != 0)
            {
                T cur = toSearch.Dequeue();
                visitor(cur);
                foreach (T next in neighbourFunc(cur))
                {
                    if (seen.Add(next))
                    {
                        toSearch.Enqueue(next);
                    }
                }
            }
        }

        /// <summary>
        /// Processes items in a FIFO worklist queue. Duplicate entries currently pending in the worklist are prevented.
        /// When an item is processed, if the processor returns true, triggered dependent items are added to the worklist.
        /// </summary>
        /// <param name="initialItems">Initial items to add to the worklist.</param>
        /// <param name="processFunc">
        /// Function to process an item. Returns true if the item was successfully processed and its triggers should run,
        /// or false if skipped.
        /// </param>
        /// <param name="triggerFunc">Function returning next items to enqueue when an item is processed.</param>
        /// <param name="equalityComparer">Equality comparer for items.</param>
        /// <typeparam name="T">Type of item being processed.</typeparam>
        public static void Worklist<T>(
            IEnumerable<T> initialItems,
            Func<T, bool> processFunc,
            Func<T, IEnumerable<T>> triggerFunc,
            IEqualityComparer<T> equalityComparer = null)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            Queue<T> queue = new Queue<T>();
            HashSet<T> inQueue = new HashSet<T>(equalityComparer);

            void Enqueue(T item)
            {
                if (inQueue.Add(item))
                {
                    queue.Enqueue(item);
                }
            }

            if (initialItems != null)
            {
                foreach (T item in initialItems)
                    Enqueue(item);
            }

            while (queue.Count > 0)
            {
                T cur = queue.Dequeue();
                inQueue.Remove(cur);

                if (processFunc(cur))
                {
                    foreach (T next in triggerFunc(cur))
                    {
                        Enqueue(next);
                    }
                }
            }
        }

        /// <summary>
        /// Determines the shortest path between first and last using priority first search.
        /// </summary>
        /// <param name="first">
        /// Starting state.
        /// </param>
        /// <param name="last">
        /// Target state.
        /// </param>
        /// <param name="neighbourFunc">
        /// Function for determining potential next states for a given state.  Also the distance to each new state.
        /// </param>
        /// <param name="pathObserver">
        /// Optional action to observe the path from first to last.
        /// </param>
        /// <param name="equalityComparer">
        /// Equality comparer to determine when last is found or states have been seen.
        /// </param>
        /// <param name="comparer">
        /// Comparer to determine the ordering of distances.
        /// </param>
        /// <param name="distanceSummer">
        /// Adder to accumulate distances for return.
        /// </param>
        /// <typeparam name="T">
        /// Type of state being searched over.
        /// </typeparam>
        /// <typeparam name="TD">
        /// Type of distance being accumulated.
        /// </typeparam>
        /// <returns>
        /// The distance get from first to last. distanceSummer.MinValue if no path exists.
        /// </returns>
        public static TD Pfs<T, TD>(T first, T last, Func<T, IEnumerable<KeyValuePair<T, TD>>> neighbourFunc,
            Action<T> pathObserver, IEqualityComparer<T> equalityComparer, IComparer<TD> comparer,
            IAdder<TD> distanceSummer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            return Pfs(first, other => equalityComparer.Equals(last, other), neighbourFunc, pathObserver,
                equalityComparer, comparer, distanceSummer);
        }

        /// <summary>
        /// Determines the shortest path between first and last using priority first search.
        /// </summary>
        /// <param name="first">
        /// Starting state.
        /// </param>
        /// <param name="lastFoundFunc">
        /// Function for determing whether the search has ended.
        /// </param>
        /// <param name="neighbourFunc">
        /// Function for determining potential next states for a given state.  Also the distance to each new state.
        /// </param>
        /// <param name="pathObserver">
        /// Optional action to observe the path from first to last.
        /// </param>
        /// <param name="equalityComparer">
        /// Equality comparer to determine when last is found or states have been seen.
        /// </param>
        /// <param name="comparer">
        /// Comparer to determine the ordering of distances.
        /// </param>
        /// <param name="distanceSummer">
        /// Adder to accumulate distances for return.
        /// </param>
        /// <typeparam name="T">
        /// Type of state being searched over.
        /// </typeparam>
        /// <typeparam name="TD">
        /// Type of distance being accumulated.
        /// </typeparam>
        /// <returns>
        /// The distance get from first to last. distanceSummer.MinValue if no path exists.
        /// </returns>
        public static TD Pfs<T, TD>(T first, Func<T, bool> lastFoundFunc,
            Func<T, IEnumerable<KeyValuePair<T, TD>>> neighbourFunc,
            Action<T> pathObserver, IEqualityComparer<T> equalityComparer, IComparer<TD> comparer,
            IAdder<TD> distanceSummer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (comparer == null)
            {
                comparer = Comparer<TD>.Default;
            }
            LookupHeap<Pair<TD, T>> priorityQueue =
                new LookupHeap<Pair<TD, T>>(
                    new ReverseComparer<Pair<TD, T>>(new Item1Comparer<TD, T>(comparer)))
                {
                    new Pair<TD, T>(distanceSummer.Zero, first)
                };
            Dictionary<T, KeyValuePair<T, TD>> seen = new Dictionary<T, KeyValuePair<T, TD>>(equalityComparer)
            {
                {first, new KeyValuePair<T, TD>(default(T), distanceSummer.Zero)}
            };
            while (priorityQueue.Count != 0)
            {
                var cur = priorityQueue.PopFront();
                if (lastFoundFunc(cur.Item2))
                {
                    if (pathObserver != null)
                    {
                        List<T> path = new List<T>();
                        T walkValue = cur.Item2;
                        while (true)
                        {
                            path.Add(walkValue);
                            if (equalityComparer.Equals(walkValue, first))
                            {
                                break;
                            }
                            walkValue = seen[walkValue].Key;
                        }
                        path.Reverse();
                        path.ForEach(pathObserver);
                    }
                    return cur.Item1;
                }
                foreach (KeyValuePair<T, TD> next in neighbourFunc(cur.Item2))
                {
                    TD newDist = distanceSummer.Add(next.Value, cur.Item1);
                    KeyValuePair<T, TD> existingEntry;
                    if (!seen.TryGetValue(next.Key, out existingEntry))
                    {
                        priorityQueue.Add(new Pair<TD, T>(newDist, next.Key));
                        seen.Add(next.Key, new KeyValuePair<T, TD>(cur.Item2, newDist));
                    }
                    else if (comparer.Compare(existingEntry.Value, newDist) > 0)
                    {
                        // TODO use priorityQueue updateValue once it exists.
                        priorityQueue.Remove(new Pair<TD, T>(existingEntry.Value, next.Key));
                        priorityQueue.Add(new Pair<TD, T>(newDist, next.Key));
                        seen.Remove(next.Key);
                        seen.Add(next.Key, new KeyValuePair<T, TD>(cur.Item2, newDist));
                    }
                }
            }
            return distanceSummer.MinValue;
        }

        /// <summary>
        /// Attempts to find the shortest path from any of the initial states to a target state using priority first search.
        /// Supports generalized/time-dependent transitions where the transition function receives both current state and current distance.
        /// </summary>
        /// <param name="initialStates">The starting states and their initial distances.</param>
        /// <param name="targetFoundFunc">Predicate that tests whether a reached state is the target.</param>
        /// <param name="transitionFunc">Function that yields next states and their resulting distances from the current state and distance.</param>
        /// <param name="targetDistance">The distance to the target if reached, or default if not reached.</param>
        /// <param name="comparer">Comparer for distance ordering (defaults to Comparer&lt;TD&gt;.Default).</param>
        /// <param name="equalityComparer">Equality comparer for states (defaults to EqualityComparer&lt;T&gt;.Default).</param>
        /// <typeparam name="T">Type of state being searched over.</typeparam>
        /// <typeparam name="TD">Type of distance being accumulated.</typeparam>
        /// <returns>True if a target state was reached, false otherwise.</returns>
        public static bool TryPfs<T, TD>(
            IEnumerable<KeyValuePair<T, TD>> initialStates,
            Func<T, bool> targetFoundFunc,
            Func<T, TD, IEnumerable<KeyValuePair<T, TD>>> transitionFunc,
            out TD targetDistance,
            IComparer<TD> comparer = null,
            IEqualityComparer<T> equalityComparer = null)
        {
            if (equalityComparer == null) equalityComparer = EqualityComparer<T>.Default;
            if (comparer == null) comparer = Comparer<TD>.Default;

            LookupHeap<Pair<TD, T>> priorityQueue =
                new LookupHeap<Pair<TD, T>>(
                    new ReverseComparer<Pair<TD, T>>(new Item1Comparer<TD, T>(comparer)));
            Dictionary<T, TD> best = new Dictionary<T, TD>(equalityComparer);

            if (initialStates != null)
            {
                foreach (KeyValuePair<T, TD> init in initialStates)
                {
                    if (!best.TryGetValue(init.Key, out TD existing) || comparer.Compare(init.Value, existing) < 0)
                    {
                        best[init.Key] = init.Value;
                        priorityQueue.Add(new Pair<TD, T>(init.Value, init.Key));
                    }
                }
            }

            while (priorityQueue.Count != 0)
            {
                Pair<TD, T> cur = priorityQueue.PopFront();
                TD dist = cur.Item1;
                T state = cur.Item2;

                if (comparer.Compare(dist, best[state]) > 0)
                    continue;

                if (targetFoundFunc(state))
                {
                    targetDistance = dist;
                    return true;
                }

                foreach (KeyValuePair<T, TD> next in transitionFunc(state, dist))
                {
                    T nextState = next.Key;
                    TD nextDist = next.Value;
                    if (!best.TryGetValue(nextState, out TD curBest) || comparer.Compare(nextDist, curBest) < 0)
                    {
                        best[nextState] = nextDist;
                        priorityQueue.Add(new Pair<TD, T>(nextDist, nextState));
                    }
                }
            }

            targetDistance = default(TD);
            return false;
        }

        /// <summary>
        /// Attempts to find the shortest path from an initial state to a target state using priority first search.
        /// Supports generalized/time-dependent transitions where the transition function receives both current state and current distance.
        /// </summary>
        /// <param name="initialState">The starting state.</param>
        /// <param name="initialDistance">The initial distance of the starting state.</param>
        /// <param name="targetFoundFunc">Predicate that tests whether a reached state is the target.</param>
        /// <param name="transitionFunc">Function that yields next states and their resulting distances from the current state and distance.</param>
        /// <param name="targetDistance">The distance to the target if reached, or default if not reached.</param>
        /// <param name="comparer">Comparer for distance ordering (defaults to Comparer&lt;TD&gt;.Default).</param>
        /// <param name="equalityComparer">Equality comparer for states (defaults to EqualityComparer&lt;T&gt;.Default).</param>
        /// <typeparam name="T">Type of state being searched over.</typeparam>
        /// <typeparam name="TD">Type of distance being accumulated.</typeparam>
        /// <returns>True if a target state was reached, false otherwise.</returns>
        public static bool TryPfs<T, TD>(
            T initialState,
            TD initialDistance,
            Func<T, bool> targetFoundFunc,
            Func<T, TD, IEnumerable<KeyValuePair<T, TD>>> transitionFunc,
            out TD targetDistance,
            IComparer<TD> comparer = null,
            IEqualityComparer<T> equalityComparer = null)
        {
            return TryPfs(
                new[] { new KeyValuePair<T, TD>(initialState, initialDistance) },
                targetFoundFunc,
                transitionFunc,
                out targetDistance,
                comparer,
                equalityComparer);
        }
    }
}