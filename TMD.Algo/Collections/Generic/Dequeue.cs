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
using System.Diagnostics.CodeAnalysis;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides a double ended queue.
    /// </summary>
    /// <typeparam name="T">
    /// Type of elements in the queue.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public class Dequeue<T> : IList<T>
    {
        private int count;
        private int version;
        private T[] array = Array.Empty<T>();
        private int start;
        private int end = -1;

        /// <summary>
        /// Gets the count of the number of members.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Gets the element at the front of the queue.
        /// </summary>
        public T Front
        {
            get
            {
                if (count <= 0)
                    throw new InvalidOperationException("There is no front when the dequeue is empty.");
                return array[start];
            }
        }

        /// <summary>
        /// Gets the element at the back of the queue.
        /// </summary>
        public T Back
        {
            get
            {
                if (count <= 0)
                    throw new InvalidOperationException("There is no back when the dequeue is empty.");
                return array[end];
            }
        }

        /// <summary>
        /// Adds an element to the back of the queue.
        /// </summary>
        /// <param name="value">
        /// Element to add.
        /// </param>
        [SuppressMessage("Microsoft.Naming", "CA1702")]
        public void PushBack(T value)
        {
            if (count >= array.Length)
            {
                Resize();
            }
            end = (end + 1)%array.Length;
            array[end] = value;
            count++;
            version++;
        }

        private void Resize()
        {
            int newSize = Math.Max(array.Length << 1, 4);
            T[] newArray = new T[newSize];
            if (count > 0)
            {
                if (start <= end)
                {
                    Array.Copy(array, start, newArray, 0, count);
                }
                else if (start > end)
                {
                    Array.Copy(array, start, newArray, 0, array.Length - start);
                    Array.Copy(array, 0, newArray, array.Length - start, end + 1);
                }
            }
            array = newArray;
            start = 0;
            end = count - 1;
        }

        /// <summary>
        /// Adds an element to the front of the queue.
        /// </summary>
        /// <param name="value">
        /// Value to add.
        /// </param>
        public void PushFront(T value)
        {
            if (count >= array.Length)
            {
                Resize();
            }
            start--;
            if (start < 0)
                start = array.Length - 1;
            // special case
            if (end < 0)
                end = start;
            array[start] = value;
            count++;
            version++;
        }

        /// <summary>
        /// Removes and returns the element at the back of the queue.
        /// </summary>
        /// <returns>
        /// The element which was at the back of the queue.
        /// </returns>
        public T PopBack()
        {
            if (count <= 0)
                throw new InvalidOperationException("There is no back when the dequeue is empty.");
            T result = array[end];
            array[end] = default(T);
            end--;
            if (end < 0)
                end = array.Length - 1;
            count--;
            version++;
            return result;
        }

        /// <summary>
        /// Removes and returns the element at the front of the queue.
        /// </summary>
        /// <returns>
        /// The element which was at the front of the queue.
        /// </returns>
        public T PopFront()
        {
            if (count <= 0)
                throw new InvalidOperationException("There is no front when the dequeue is empty.");
            T result = array[start];
            array[start] = default(T);
            start = (start + 1)%array.Length;
            count--;
            version++;
            return result;
        }

        /// <summary>
        /// Clears the dequeue.
        /// </summary>
        public void Clear()
        {
            Array.Clear(array, 0, array.Length);
            count = 0;
            start = 0;
            end = -1;
            version++;
        }

        #region IList<T> Members

        /// <summary>
        /// Gets the index of the specified item in the dequeue, if it exists.
        /// </summary>
        /// <param name="item">
        /// Item to search for.
        /// </param>
        /// <returns>
        /// The index of the item in the list if found, otherwise -1.
        /// </returns>
        public int IndexOf(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(array[(start + i)%array.Length], item))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index in to the queue at which to insert.
        /// </param>
        /// <param name="item">
        /// Item which to insert in the dequeue.
        /// </param>
        public void Insert(int index, T item)
        {
            // TODO: replace loops with array copies.
            if (index < 0 || index > count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range to insert an element.");
            if (index == 0)
                PushFront(item);
            else if (index == count)
                PushBack(item);
            else if (index < count/2)
            {
                PushFront(Front);
                for (int i = 1; i < index; i++)
                {
                    this[i] = this[i + 1];
                }
                this[index] = item;
            }
            else
            {
                PushBack(Back);
                for (int i = count - 2; i > index; i--)
                {
                    this[i] = this[i - 1];
                }
                this[index] = item;
            }
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index of the item to remove.
        /// </param>
        public void RemoveAt(int index)
        {
            // TODO: replace loops with array copies.
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range to insert an element.");
            if (index == 0)
                PopFront();
            else if (index == count - 1)
                PopBack();
            else if (index < count/2)
            {
                for (int i = index; i > 0; i--)
                {
                    this[i] = this[i - 1];
                }
                PopFront();
            }
            else
            {
                for (int i = index; i < count - 1; i++)
                {
                    this[i] = this[i + 1];
                }
                PopBack();
            }
        }

        /// <summary>
        /// Gets or sets the specified location in the dequeue.
        /// </summary>
        /// <param name="index">
        /// Index in the dequeue to retrieve or modify.
        /// </param>
        /// <returns>
        /// The value stored at the given offset in to the dequeue.
        /// </returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must point to a position in the queue.");
                return array[(start + index)%array.Length];
            }
            set
            {
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must point to a position in the queue.");
                array[(start + index)%array.Length] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the end of the queue.
        /// </summary>
        /// <param name="item">
        /// Item to add.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1033")]
        void ICollection<T>.Add(T item)
        {
            PushBack(item);
        }

        /// <summary>
        /// Determines whether the item exists in the collection.
        /// </summary>
        /// <param name="item">
        /// Item to check for existance.
        /// </param>
        /// <returns>
        /// True if the item exists in the collection, false otherwise.
        /// </returns>
        public bool Contains(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(array[(start + i)%array.Length], item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Copys the contents of this queue in to another array.
        /// </summary>
        /// <param name="array">
        /// Array to copy to.
        /// </param>
        /// <param name="arrayIndex">
        /// Array index to start at in the target array.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (count > 0)
            {
                if (start <= end)
                {
                    Array.Copy(this.array, start, array, arrayIndex, count);
                }
                else if (start > end)
                {
                    Array.Copy(this.array, start, array, arrayIndex, this.array.Length - start);
                    Array.Copy(this.array, 0, array, arrayIndex + this.array.Length - start, end + 1);
                }
            }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Removes the specified item from the list if it exists.
        /// </summary>
        /// <param name="item">
        /// Item to remove from the list.
        /// </param>
        /// <returns>
        /// True if the item was found and removed, false otherwise.
        /// </returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;
            RemoveAt(index);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Gets a strongly typed enumerator to enumerate the values in the dequeue.
        /// </summary>
        /// <returns>
        /// An enumerator for this dequeue.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new DequeueEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an old style enumerator to enumerate the values in the dequeue.
        /// </summary>
        /// <returns>
        /// An old style enumerator for this dequeue.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new DequeueEnumerator(this);
        }

        #endregion

        /// <summary>
        /// Enumerator for the Dequeue.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct DequeueEnumerator : IEnumerator<T>
        {
            /// <summary>
            /// Internal constructor.
            /// </summary>
            /// <param name="parent">
            /// Parent dequeue to enumerate.
            /// </param>
            internal DequeueEnumerator(Dequeue<T> parent)
            {
                this.parent = parent;
                currentIndex = -1;
                origVersion = parent.version;
            }

            private readonly Dequeue<T> parent;
            private int currentIndex;
            private readonly int origVersion;

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the current value the enumerator is pointing to.
            /// </summary>
            public T Current
            {
                get
                {
                    if (origVersion != parent.version)
                        throw new InvalidOperationException(
                            "The collection being enumerated has been modified since enumerator was acquired.");
                    if (currentIndex >= parent.count)
                        throw new InvalidOperationException("Current is not pointing to a valid location.");
                    if (currentIndex == -1)
                        return default(T);
                    else
                        return parent.array[(parent.start + currentIndex)%parent.array.Length];
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
            /// Gets the object the enumerator is currently pointing to.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (currentIndex == -1)
                        throw new InvalidOperationException("Current is not pointing to a valid location.");
                    return this.Current;
                }
            }

            /// <summary>
            /// Moves the enumerator to the next value, if there is one.
            /// </summary>
            /// <returns>
            /// True if the enumerator now points to a valid location, false otherwise.
            /// </returns>
            public bool MoveNext()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException(
                        "The collection being enumerated has been modified since enumerator was acquired.");
                currentIndex++;
                return currentIndex < parent.count;
            }

            /// <summary>
            /// Resets the enumerator to start again.
            /// </summary>
            public void Reset()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException(
                        "The collection being enumerated has been modified since enumerator was acquired.");
                currentIndex = -1;
            }

            #endregion
        }
    }
}