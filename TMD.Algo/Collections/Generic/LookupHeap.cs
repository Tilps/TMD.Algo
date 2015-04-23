#region License
/*
Copyright (c) 2008, Gareth Pearce (www.themissingdocs.net)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the www.themissingdocs.net nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides an implementation of a Heap, useful for priority queues.
    /// This version is augmented with a lookup to allow fast contains and entry update.
    /// </summary>
    /// <typeparam name="T">
    /// Type of data stored in the heap.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public class LookupHeap<T> : IHeap<T>
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public LookupHeap() : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="startList">
        /// List to initialy populate the heap with.  Construction runs in O(n) time.
        /// </param>
        public LookupHeap(IEnumerable<T> startList) : this(startList, Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Comparer to use to compare items.  Specify null to use the default comparer.
        /// </param>
        public LookupHeap(IComparer<T> comparer) : this(comparer, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Comparer to use to compare items.  Specify null to use the default comparer.
        /// </param>
        /// <param name="hashComparer">
        /// Comparer to use for the lookup.  Specify null to use the default hash comparer.
        /// </param>
        public LookupHeap(IComparer<T> comparer, IEqualityComparer<T> hashComparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            this.comparer = comparer;
            lookup = new Dictionary<T, int>(hashComparer);
            array = emptyArray;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="startList">
        /// Initial list to populate the heap with.  Population occurs in O(n) time.
        /// </param>
        /// <param name="comparer">
        /// Comparer to use to compare items.  Specify null to use the default comparer.
        /// </param>
        public LookupHeap(IEnumerable<T> startList, IComparer<T> comparer) : this(startList, comparer, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="startList">
        /// Initial list to populate the heap with.  Population occurs in O(n) time.
        /// </param>
        /// <param name="comparer">
        /// Comparer to use to compare items.  Specify null to use the default comparer.
        /// </param>
        /// <param name="hashComparer">
        /// Comparer to use for the lookup.  Specify null to use the default hash comparer.
        /// </param>
        public LookupHeap(IEnumerable<T> startList, IComparer<T> comparer, IEqualityComparer<T> hashComparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            this.comparer = comparer;
            count = startList.Count();
            lookup = new Dictionary<T, int>(hashComparer);
            array = new T[count + 1];
            int index = 1;
            foreach (T value in startList)
            {
                array[index] = value;
                index++;
            }
            for (int i = 0; i < count; i++)
            {
                lookup[array[i + 1]] = i + 1;
            }
            for (int i = Parent(count); i >= 1; i--)
            {
                Heapify(i);
            }
        }

        /// <summary>
        /// Test method for validating heap structure.  DO NOT USE.
        /// </summary>
        internal void ___TESTValidateTEST()
        {
            for (int i = 2; i <= count; i++)
            {
                if (comparer.Compare(array[Parent(i)], array[i]) < 0)
                    throw new InvalidOperationException("Heap is corrupt.");
                if (lookup[array[i]] != i)
                    throw new InvalidOperationException("Heap lookup is corrupt.");
            }
            if (count > 0 && lookup[array[1]] != 1)
                throw new InvalidOperationException("Heap lookup is corrupt.");
            if (lookup.Count != count)
                throw new InvalidOperationException("Heap lookup is corrupt.");
        }

        /// <summary>
        /// Gets the first item in the heap, the largest item.
        /// </summary>
        public T Front
        {
            get
            {
                if (count <= 0)
                    throw new InvalidOperationException("The heap is currently empty.");
                return array[1];
            }
        }

        /// <summary>
        /// Gets the first item in the heap, if the heap isn't empty.
        /// </summary>
        /// <param name="var">
        /// Receives the first item in the heap, if there is one.
        /// </param>
        /// <returns>
        /// True if the heap was non-empty, false otherwise.
        /// </returns>
        public bool TryGetFront(out T var)
        {
            if (count <= 0)
            {
                var = default(T);
                return false;
            }
            else
            {
                var = array[1];
                return true;
            }
        }

        /// <summary>
        /// Gets and removes the first element in the heap.
        /// </summary>
        /// <returns>
        /// The first element in the heap, if the heap was not empty.
        /// </returns>
        public T PopFront()
        {
            if (count <= 0)
                throw new InvalidOperationException("The heap is currently empty.");
            T max = array[1];
            lookup.Remove(max);
            if (count > 1)
            {
                array[1] = array[count];
                lookup[array[1]] = 1;
            }
            array[count] = default(T); // Clear the entry incase it references something.
            count--;
            Heapify(1);
            version++;
            return max;
        }

        private int version;
        private T[] array;
        private int count;
        private IComparer<T> comparer;
        private Dictionary<T, int> lookup;
        private static T[] emptyArray = new T[1];

        private static int Parent(int index)
        {
            return index >> 1;
        }

        private static int LeftChild(int index)
        {
            return index << 1;
        }

        private static int RightChild(int index)
        {
            return (index << 1) | 1;
        }

        private void Heapify(int index)
        {
            bool done = true;
            do
            {
                done = true;
                int l = LeftChild(index);
                int r = RightChild(index);
                int largest;
                if (l <= count && comparer.Compare(array[l], array[index]) > 0)
                    largest = l;
                else
                    largest = index;
                if (r <= count && comparer.Compare(array[r], array[largest]) > 0)
                    largest = r;
                if (largest != index)
                {
                    T tmp = array[index];
                    array[index] = array[largest];
                    lookup[array[index]] = index;
                    array[largest] = tmp;
                    lookup[array[largest]] = largest;
                    done = false;
                    index = largest;
                }
            } while (!done);
        }

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the heap.
        /// </summary>
        /// <param name="item">
        /// Item to add.
        /// </param>
        public void Add(T item)
        {
            count++;
            if (count >= array.Length)
            {
                int newLength = array.Length << 1;
                if (newLength < 4)
                    newLength = 4;
                T[] newArray = new T[newLength];
                Array.Copy(array, newArray, count);
                array = newArray;
            }
            int i = count;
            while (i > 1 && comparer.Compare(array[Parent(i)], item) < 0)
            {
                array[i] = array[Parent(i)];
                lookup[array[i]] = i;
                i = Parent(i);
            }
            array[i] = item;
            lookup[array[i]] = i;
            version++;
        }

        /// <summary>
        /// Clears the heap.
        /// </summary>
        public void Clear()
        {
            Array.Clear(array, 1, count);
            lookup.Clear();
            count = 0;
            version++;
        }

        /// <summary>
        /// Returns true if the heap contains the specified item.
        /// </summary>
        /// <param name="item">
        /// Item to search for.
        /// </param>
        /// <returns>
        /// True if item exists in the heap, false otherwise.
        /// </returns>
        public bool Contains(T item)
        {
            return lookup.ContainsKey(item);
        }

        /// <summary>
        /// Copies the elements in this heap to an array, in heap order.
        /// </summary>
        /// <param name="array">
        /// Array to receive the copied results.
        /// </param>
        /// <param name="arrayIndex">
        /// Offset in to target array to copy to.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this.array, 1, array, arrayIndex, count);
        }

        /// <summary>
        /// Gets the count of the number of elements in the heap.
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified item from the heap if it exists.
        /// </summary>
        /// <param name="item">
        /// Item to remove from the heap.
        /// </param>
        /// <returns>
        /// True if the item was in the heap and removed, false otherwise.
        /// </returns>
        public bool Remove(T item)
        {
            int i;
            if (lookup.TryGetValue(item, out i))
            {
                lookup.Remove(item);
                array[i] = array[count];
                lookup[array[i]] = i;
                count--;
                if (i == 1)
                    Heapify(1);
                else if (comparer.Compare(array[Parent(i)], array[i]) >= 0)
                {
                    Heapify(i);
                }
                else
                {
                    while (i > 1 && comparer.Compare(array[Parent(i)], array[count + 1]) < 0)
                    {
                        array[i] = array[Parent(i)];
                        lookup[array[i]] = i;
                        i = Parent(i);
                    }
                    array[i] = array[count + 1];
                    lookup[array[i]] = i;
                }
                array[count + 1] = default(T); // Clear the entry incase it references something.
                version++;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Gets the strongly typed enumerator.
        /// </summary>
        /// <returns>
        /// An enumerator to list all the values in the heap.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new HeapEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the old style enumerator.
        /// </summary>
        /// <returns>
        /// An old style enumerator to return all the values in the heap.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new HeapEnumerator(this);
        }

        #endregion

        /// <summary>
        /// Heap enumerator for enumerating values in the heap.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct HeapEnumerator : IEnumerator<T>
        {

            /// <summary>
            /// Internal constructor.
            /// </summary>
            /// <param name="parent">
            /// Parent which this enumerator is enumerating.
            /// </param>
            internal HeapEnumerator(LookupHeap<T> parent)
            {
                this.parent = parent;
                origVersion = parent.version;
                currentIndex = 0;
            }

            private LookupHeap<T> parent;
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
                    if (currentIndex > parent.count)
                        throw new InvalidOperationException("Current is not pointing to a valid location.");
                    if (currentIndex == 0)
                        return default(T);
                    else
                        return parent.array[currentIndex];
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
                    if (currentIndex == 0)
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
                currentIndex++;
                if (currentIndex > parent.count)
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
                currentIndex = 0;
            }

            #endregion
        }
    }
}
