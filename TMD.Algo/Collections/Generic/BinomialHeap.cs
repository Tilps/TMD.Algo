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
    /// Provides an implementation of a Binomial Heap, useful for mergeable priority queues.
    /// </summary>
    /// <typeparam name="T">
    /// Type of data stored in the heap.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public class BinomialHeap<T> : IHeap<T>
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public BinomialHeap() : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="startList">
        /// List to initialy populate the heap with.  Construction runs in O(n) time.
        /// </param>
        public BinomialHeap(IEnumerable<T> startList) : this(startList, Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Comparer to use to compare items.  Specify null to use the default comparer.
        /// </param>
        public BinomialHeap(IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            this.comparer = comparer;
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
        public BinomialHeap(IEnumerable<T> startList, IComparer<T> comparer)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            this.comparer = comparer;
            foreach (T value in startList)
            {
                Add(value);
            }
        }

        private class BinomialNode
        {
            public T Value;
            public BinomialNode Parent;
            public BinomialNode Child;
            public BinomialNode Sibling;
            public int Degree;
        }

        /// <summary>
        /// Test method for validating heap structure.  DO NOT USE.
        /// </summary>
        internal void ___TESTValidateTEST()
        {
            if (head == null)
                return;
            int maxLoop = 33;
            BinomialNode current = head;
            int lastDegree = -1;
            while (current != null)
            {
                maxLoop--;
                if (maxLoop < 0)
                    throw new InvalidOperationException("Internal list is improbably long.");
                if (((1 << current.Degree) & count) == 0)
                    throw new InvalidOperationException("Internal list doesn't correspond to bitset of total count.");
                if (current.Degree <= lastDegree)
                    throw new InvalidOperationException("Internal list not sorted or has duplicates.");
                if (current.Parent != null)
                    throw new InvalidOperationException("Top level nodes must be parent less.");
                lastDegree = current.Degree;
                Validate(current);
                current = current.Sibling;
            }
        }

        private void Validate(BinomialNode current)
        {
            Validate(current, true);
        }

        private void Validate(BinomialNode current, bool top)
        {
            if (current.Child != null)
            {
                if (current.Child.Degree != current.Degree - 1)
                    throw new InvalidOperationException("Degrees are not marked correctly");
                if (current.Child.Degree < 0)
                    throw new InvalidOperationException("Negative degrees not allowed on children.");
                if (comparer.Compare(current.Child.Value, current.Value) > 0)
                    throw new InvalidOperationException("Heap not in heap order.");
                Validate(current.Child, false);
            }
            else if (current.Degree != 0)
                throw new InvalidOperationException("Leaf has wrong degree.");
            if (!top)
            {
                if (current.Sibling != null)
                {
                    if (current.Sibling.Degree != current.Degree - 1)
                        throw new InvalidOperationException("Sibling Degrees are not marked correctly");
                    if (current.Sibling.Degree < 0)
                        throw new InvalidOperationException("Negative degrees not allowed on children.");
                    Validate(current.Sibling, false);
                }
            }
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
                BinomialNode y = head;
                T maxValue = y.Value;
                BinomialNode x = head.Sibling;
                while (x != null)
                {
                    if (comparer.Compare(x.Value, maxValue) > 0)
                    {
                        maxValue = x.Value;
                        y = x;
                    }
                    x = x.Sibling;
                }
                return y.Value;
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
                var = Front;
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
                throw new InvalidOperationException("The healp is currently empty.");
            BinomialNode prevY = null;
            BinomialNode y = head;
            T maxValue = y.Value;
            BinomialNode prevX = head;
            BinomialNode x = head.Sibling;
            while (x != null)
            {
                if (comparer.Compare(x.Value, maxValue) > 0)
                {
                    maxValue = x.Value;
                    prevY = prevX;
                    y = x;
                }
                prevX = x;
                x = x.Sibling;
            }
            RemoveTopLevelNode(prevY, y);
            return y.Value;
        }

        private void RemoveTopLevelNode(BinomialNode prevY, BinomialNode y)
        {
            BinomialNode head2 = null;
            BinomialNode current = y.Child;
            while (current != null)
            {
                current.Parent = null;
                BinomialNode tmp = current.Sibling;
                current.Sibling = head2;
                head2 = current;
                current = tmp;
            }
            if (prevY == null)
                head = y.Sibling;
            else
                prevY.Sibling = y.Sibling;
            UnionWith(head2);
            count--;
            version++;
        }

        /// <summary>
        /// Unions one binomial heap in to this one.  Destroys the other heaps contents.
        /// </summary>
        /// <param name="other">
        /// Other heap to union in to this one.
        /// </param>
        public void UnionWith(BinomialHeap<T> other)
        {
            this.count += other.count;
            version++;
            BinomialNode otherHead = other.head;
            other.Clear();
            UnionWith(otherHead);
        }

        private void UnionWith(BinomialNode otherHead)
        {
            head = MergeTopLevels(this.head, otherHead);
            if (head == null)
                return;
            BinomialNode prevX = null;
            BinomialNode x = head;
            BinomialNode nextX = x.Sibling;
            while (nextX != null)
            {
                if (x.Degree != nextX.Degree || (nextX.Sibling != null && nextX.Sibling.Degree == x.Degree))
                {
                    prevX = x;
                    x = nextX;
                }
                else
                {
                    if (comparer.Compare(x.Value, nextX.Value) >= 0)
                    {
                        x.Sibling = nextX.Sibling;
                        UnionTopLevelNodes(x, nextX);
                    }
                    else
                    {
                        if (prevX == null)
                            head = nextX;
                        else
                            prevX.Sibling = nextX;
                        UnionTopLevelNodes(nextX, x);
                        x = nextX;
                    }
                }
                nextX = x.Sibling;
            }
        }

        private static BinomialNode MergeTopLevels(BinomialNode firstList, BinomialNode secondList)
        {
            BinomialNode result = null;
            BinomialNode current = null;
            while (firstList != null && secondList != null)
            {
                if (firstList.Degree <= secondList.Degree)
                {
                    if (current == null)
                    {
                        current = firstList;
                        result = current;
                    }
                    else
                    {
                        current.Sibling = firstList;
                        current = firstList;
                    }
                    firstList = firstList.Sibling;
                }
                else
                {
                    if (current == null)
                    {
                        current = secondList;
                        result = current;
                    }
                    else
                    {
                        current.Sibling = secondList;
                        current = secondList;
                    }
                    secondList = secondList.Sibling;
                }
            }
            while (firstList != null)
            {
                if (current == null)
                {
                    current = firstList;
                    result = current;
                }
                else
                {
                    current.Sibling = firstList;
                    current = firstList;
                }
                firstList = firstList.Sibling;
            }
            while (secondList != null)
            {
                if (current == null)
                {
                    current = secondList;
                    result = current;
                }
                else
                {
                    current.Sibling = secondList;
                    current = secondList;
                }
                secondList = secondList.Sibling;
            }
            return result;
        }

        private static void UnionTopLevelNodes(BinomialNode first, BinomialNode second)
        {
            second.Parent = first;
            second.Sibling = first.Child;
            first.Child = second;
            first.Degree++;
        }

        private int version;
        private int count;
        private IComparer<T> comparer;
        private BinomialNode head;


        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the heap.
        /// </summary>
        /// <param name="item">
        /// Item to add.
        /// </param>
        public void Add(T item)
        {
            BinomialNode newNode = new BinomialNode();
            newNode.Value = item;
            UnionWith(newNode);
            count++;
            version++;
        }

        /// <summary>
        /// Clears the heap.
        /// </summary>
        public void Clear()
        {
            count = 0;
            head = null;
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
            foreach (T value in this)
            {
                if (comparer.Compare(value, item) == 0)
                    return true;
            }
            return false;
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
            if (arrayIndex < 0 || arrayIndex + count > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex", "Specified array index is either negative or does not allow for enough space to store the data.");
            foreach (T value in this)
            {
                array[arrayIndex] = value;
                arrayIndex++;
            }
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
            BinomialNode node = Find(item);
            if (node == null)
                return false;
            while (node.Parent != null)
            {
                T tmp = node.Parent.Value;
                node.Parent.Value = node.Value;
                node.Value = tmp;
                node = node.Parent;
            }
            BinomialNode prev = null;
            if (head != node)
            {
                prev = head;
                while (prev != null)
                {
                    if (prev.Sibling == node)
                    {
                        break;
                    }
                    prev = prev.Sibling;
                }
            }
            RemoveTopLevelNode(prev, node);
            return true;
        }

        private BinomialNode Find(T item)
        {
            if (head == null)
                return null;
            if (comparer.Compare(head.Value, item) == 0)
                return head;
            BinomialNode currentNode = head;
            bool lastWasChild = false;
            while (true)
            {
                if (!lastWasChild && currentNode.Child != null)
                {
                    currentNode = currentNode.Child;
                }
                else if (currentNode.Sibling != null)
                {
                    currentNode = currentNode.Sibling;
                    lastWasChild = false;
                }
                else if (currentNode.Parent != null)
                {
                    currentNode = currentNode.Parent;
                    lastWasChild = true;
                }
                else
                {
                    return null;
                }
                if (comparer.Compare(currentNode.Value, item) == 0)
                    return currentNode;
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
            return new BinomialHeapEnumerator(this);
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
            return new BinomialHeapEnumerator(this);
        }

        #endregion

        /// <summary>
        /// Heap enumerator for enumerating values in the heap.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct BinomialHeapEnumerator : IEnumerator<T>
        {

            /// <summary>
            /// Internal constructor.
            /// </summary>
            /// <param name="parent">
            /// Parent which this enumerator is enumerating.
            /// </param>
            internal BinomialHeapEnumerator(BinomialHeap<T> parent)
            {
                this.parent = parent;
                origVersion = parent.version;
                currentNode = null;
                finished = false;
                lastWasChild = false;
            }

            private BinomialHeap<T> parent;
            private int origVersion;
            private BinomialNode currentNode;
            private bool finished;
            private bool lastWasChild;

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
                    if (currentNode == null)
                        throw new InvalidOperationException("The enumerator is currently not pointing to a valid node.");
                    return currentNode.Value;
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
                    return Current;
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
                if (finished)
                    return false;
                if (currentNode == null)
                {
                    currentNode = parent.head;
                    if (currentNode == null)
                    {
                        finished = true;
                        return false;
                    }
                }
                else
                {
                    if (!lastWasChild && currentNode.Child != null)
                    {
                        currentNode = currentNode.Child;
                    }
                    else if (currentNode.Sibling != null)
                    {
                        currentNode = currentNode.Sibling;
                        lastWasChild = false;
                    }
                    else if (currentNode.Parent != null)
                    {
                        currentNode = currentNode.Parent;
                        lastWasChild = true;
                        // We've already been here, need to move to next spot.
                        return MoveNext();
                    }
                    else
                    {
                        finished = true;
                        currentNode = null;
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Resets the enumerator to enumerate from the begining.
            /// </summary>
            void System.Collections.IEnumerator.Reset()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException("The collection being enumerated has been modified since enumerator was acquired.");
                currentNode = null;
                finished = false;
                lastWasChild = false;
            }

            #endregion
        }
    }
}
