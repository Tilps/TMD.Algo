#region License
/*
Copyright (c) 2011, the TMD.Algo authors.
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
using System.Diagnostics.CodeAnalysis;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides a queue like data structure with amortized O(1) contains and O(1) remove as well as O(1) append.
    /// Requires that the queue only contains distinct items.
    /// </summary>
    /// <typeparam name="T">
    /// Type of data being stored.
    /// </typeparam>
    public class LookupQueue<T> : ICollection<T>
    {
        #region data structure

        private struct LinkListNode
        {
            public int Next;
            public int Prev;
            public T Value;
        }
        private LinkListNode[] linkList;
        private int linkListHead;
        private int linkListTail;
        private int freeListHead;
        private int linkListSize;
        private Dictionary<T, int> lookup;
        private int size;

        private int version;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public LookupQueue()
        {
            lookup = new Dictionary<T, int>();
            linkList = new LinkListNode[4];
            linkListHead = -1;
            linkListTail = -1;
            freeListHead = -1;
            linkListSize = 0;
        }

        #endregion 

        /// <summary>
        /// Adds an entry to the end of the queue.
        /// </summary>
        /// <param name="item">
        /// Item to enqueue.
        /// </param>
        public void Enqueue(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Removes the first element from the queue.
        /// </summary>
        /// <returns>
        /// The removed item.
        /// </returns>
        public T Dequeue()
        {
            if (size == 0)
                throw new InvalidOperationException("Queue must not be empty in order to dequeue.");
            T result = linkList[linkListHead].Value;
            // TODO: optimize this to avoid the lookup check that remove performs.
            Remove(result);
            return result;
        }

        /// <summary>
        /// Removes the first element, if there is one.
        /// </summary>
        /// <param name="value">
        /// Value retrieved, or default.
        /// </param>
        /// <returns>
        /// True if the queue was non-empty, false otherwise.
        /// </returns>
        public bool TryDequeue(out T value)
        {
            if (Count == 0)
            {
                value = default (T);
                return false;
            }
            value = Dequeue();
            return true;
        }

        /// <summary>
        /// Looks at the first element in the queue.
        /// </summary>
        /// <returns>
        /// The first element in the queue.
        /// </returns>
        public T Peek()
        {
            if (size == 0)
                throw new InvalidOperationException("Queue must not be empty in order to peek.");
            return linkList[linkListHead].Value;
        }

        /// <summary>
        /// Adds to the queue if the item isn't already in the queue.
        /// </summary>
        /// <param name="item">
        /// Item to add to the queue.
        /// </param>
        /// <returns>
        /// True if the item was added, false otherwise.
        /// </returns>
        public bool TryEnqueue(T item)
        {
            if (Contains(item)) return false;
            Add(item);
            return true;
        }

        #region ICollection<T> implementation

        /// <summary>
        /// Adds an item to the collection, specifically to the end of the queue.
        /// </summary>
        /// <param name="item">
        /// Item to add to the queue.
        /// </param>
        public void Add(T item)
        {
            if (lookup.ContainsKey(item))
                throw new ArgumentException("Provided item already exists in the queue.", "item");
            int loc = GetNextSpot();
            linkList[loc].Value = item;
            lookup[item] = loc;
            size++;
            version++;
        }

        /// <summary>
        /// Performs the task of preparing a new spot for data at the end of the queue.
        /// </summary>
        /// <returns>
        /// The position in the array of the new spot which has been prepared.
        /// </returns>
        private int GetNextSpot()
        {
            int spot;
            if (freeListHead != -1)
            {
                spot = freeListHead;
                freeListHead = linkList[freeListHead].Next;
            }
            else
            {
                if (linkListSize >= linkList.Length)
                {
                    LinkListNode[] newArray = new LinkListNode[linkList.Length * 2];
                    Array.Copy(linkList, newArray, linkList.Length);
                    linkList = newArray;
                }
                spot = linkListSize;
                linkListSize++;
            }
            if (linkListHead == -1)
                linkListHead = spot;
            if (linkListTail != -1)
            {
                linkList[linkListTail].Next = spot;
                linkList[spot].Prev = linkListTail;
            }
            else
            {
                linkList[spot].Prev = -1;
            }
            linkList[spot].Next = -1;
            linkListTail = spot;
            return spot;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            linkListHead = -1;
            linkListTail = -1;
            freeListHead = -1;
            linkListSize = 0;
            size = 0;
            lookup.Clear();
            Array.Clear(linkList, 0, linkList.Length);
            version++;
        }

        /// <summary>
        /// Checks whether an item is in the queue.
        /// </summary>
        /// <param name="item">
        /// The item to check.
        /// </param>
        /// <returns>
        /// True if the item is in the queue, false otherwise.
        /// </returns>
        public bool Contains(T item)
        {
            return lookup.ContainsKey(item);
        }

        /// <summary>
        /// Copies the content of the queue into the provided array in queue order.
        /// </summary>
        /// <param name="array">
        /// Array to receive the contents of this queue.
        /// </param>
        /// <param name="arrayIndex">
        /// Index at which to start adding values to the array.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            // TODO: check for arrayIndex + size overflow?
            if (array.Length < arrayIndex + size)
                throw new ArgumentException("Insufficient size in provided array.", "array");            
            foreach (T val in this)
            {
                array[arrayIndex] = val;
                arrayIndex++;
            }
        }

        /// <summary>
        /// Gets the count of the queued entries.
        /// </summary>
        public int Count
        {
            get { return size; }
        }

        /// <summary>
        /// Returns false.  ReadOnly mode not supported.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specific item from the queue.
        /// </summary>
        /// <param name="item">
        /// Item to remove from the queue.
        /// </param>
        /// <returns>
        /// True if the item was removed, false otherwise.
        /// </returns>
        public bool Remove(T item)
        {
            int index;
            if (!lookup.TryGetValue(item, out index))
                return false;
            lookup.Remove(item);

            int prev = linkList[index].Prev;
            int next = linkList[index].Next;
            if (prev == -1)
                linkListHead = next;
            else
                linkList[prev].Next = next;
            if (next == -1)
                linkListTail = prev;
            else
                linkList[next].Prev = prev;
            linkList[index].Value = default(T);
            linkList[index].Next = freeListHead;
            freeListHead = index;
            version++;
            size--;
            return true;
        }

        /// <summary>
        /// Gets the enumerator to enumerate the contents of the queue in queue order.
        /// </summary>
        /// <returns>
        /// An enumerator for this queue.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new LookupQueueEnumerator(this);
        }

        /// <summary>
        /// Gets the non-typed enumerator to enumerate the contents of the queue in queue order.
        /// </summary>
        /// <returns>
        /// An enumerator for this queue.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new LookupQueueEnumerator(this);
        }

        #endregion

        #region Enumerator struct

        /// <summary>
        /// Lookup queue enumerator for enumerating values in the queue in queue order.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct LookupQueueEnumerator : IEnumerator<T>
        {

            /// <summary>
            /// Internal constructor.
            /// </summary>
            /// <param name="parent">
            /// Parent which this enumerator is enumerating.
            /// </param>
            internal LookupQueueEnumerator(LookupQueue<T> parent)
            {
                this.parent = parent;
                origVersion = parent.version;
                currentIndex = -2;
            }

            private LookupQueue<T> parent;
            private int origVersion;
            private int currentIndex;

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the current item pointed to by this enumerator.
            /// </summary>
            public T Current
            {
                get
                {
                    if (origVersion != parent.version)
                        throw new InvalidOperationException("The collection being enumerated has been modified since enumerator was acquired.");
                    if (currentIndex == -1)
                        throw new InvalidOperationException("Current is not pointing to a valid location.");
                    if (currentIndex == -2)
                        return default(T);
                    else
                        return parent.linkList[currentIndex].Value;
                }
            }

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Does nothing.
            /// </summary>
            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Gets the current object this enumerator is pointing at.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (currentIndex == -2)
                        throw new InvalidOperationException("Current is not pointing to a valid location.");
                    return this.Current;
                }
            }

            /// <summary>
            /// Moves to the next entry in the heap, if available.
            /// </summary>
            /// <returns>
            /// True if there was an entry to move to, false otherwise.
            /// </returns>
            public bool MoveNext()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException("The collection being enumerated has been modified since enumerator was acquired.");
                if (currentIndex == -2)
                    currentIndex = parent.linkListHead;
                else if (currentIndex >= 0)
                    currentIndex = parent.linkList[currentIndex].Next;
                if (currentIndex == -1)
                    return false;
                return true;
            }

            /// <summary>
            /// Resets the enumerator to enumerate from the begining.
            /// </summary>
            void System.Collections.IEnumerator.Reset()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException("The collection being enumerated has been modified since enumerator was acquired.");
                currentIndex = -2;
            }

            #endregion
        }
        #endregion
    }
}
