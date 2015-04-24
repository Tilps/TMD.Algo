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
using System.Runtime.InteropServices;

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
        public static int Bfs<T>(T first, T last, Func<T, IEnumerable<T>> neighbourFunc, Action<T> pathObserver, IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (equalityComparer.Equals(first, last))
            {
                if (pathObserver != null)
                {
                    pathObserver(first);
                }
                return 0;
            }
            Queue<T> toSearch = new Queue<T>();
            toSearch.Enqueue(first);
            Dictionary<T, KeyValuePair<T, int>> seen = new Dictionary<T, KeyValuePair<T, int>>(equalityComparer);
            seen.Add(first, new KeyValuePair<T, int>(default(T), 0));
            while (toSearch.Count != 0)
            {
                T cur = toSearch.Dequeue();
                int depth = seen[cur].Value;
                foreach (T next in neighbourFunc(cur))
                {
                    if (!seen.ContainsKey(next))
                    {
                        if (equalityComparer.Equals(cur, last))
                        {
                            if (pathObserver != null)
                            {
                                List<T> path = new List<T>();
                                path.Add(last);
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
    }
}
