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
    /// Implements IList using a tree.
    /// </summary>
    /// <remarks>
    /// Compared to List, general performance is slower by a constant factor.
    /// Exceptions are Insert and RemoveAt are O(log n) instead of O(n), Indexer is O(log n) instead of O(1) and Add is O(log n) instead of O(1)
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public class TreeList<T> : IList<T>
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public TreeList()
        {
            treeRoot = sentinal;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialList">
        /// Initial values to populate the list with.
        /// </param>
        public TreeList(IEnumerable<T> initialList) : this()
        {
            foreach (T val in initialList)
            {
                this.Add(val);
            }
        }

        #region internal tree

        private class TreeNode
        {
            public TreeNode Parent;
            public TreeNode Left;
            public TreeNode Right;
            public int Count;
            public T Value;
            public bool Red;
        }


        private TreeNode sentinal = new TreeNode();
        private TreeNode treeRoot;

        private static void UpdateCount(TreeNode newNode)
        {
            newNode.Count = 1 + newNode.Left.Count + newNode.Right.Count;
        }

        private int TreeIndex(TreeNode node, T item)
        {
            if (node == sentinal)
                return -1;

            if (node.Value.Equals(item))
                return node.Left.Count;

            int index = TreeIndex(node.Left, item);
            if (index != -1)
                return index;

            int right = TreeIndex(node.Right, item);
            if (right != -1)
                return right + node.Left.Count + 1;

            return -1;
        }

        private void TreeInsert(TreeNode node, TreeNode newNode, int index)
        {
            TreeNode other = null;
            bool lastLeft = false;
            while (node != sentinal)
            {
                other = node;
                if (index - 1 < SubTreeIndex(node))
                {
                    node = node.Left;
                    lastLeft = true;
                }
                else
                {
                    index -= SubTreeIndex(node) + 1;
                    node = node.Right;
                    lastLeft = false;
                }
            }
            newNode.Parent = other;
            if (lastLeft)
                newNode.Parent.Left = newNode;
            else
                newNode.Parent.Right = newNode;
            TreeNode backWalk = newNode.Parent;
            while (backWalk != sentinal)
            {
                backWalk.Count++;
                backWalk = backWalk.Parent;
            }
            FixRedBlackInsert(newNode);
        }

        private void FixRedBlackInsert(TreeNode newNode)
        {
            while (newNode != treeRoot && newNode.Parent.Red)
            {
                if (newNode.Parent == newNode.Parent.Parent.Left)
                {
                    TreeNode other = newNode.Parent.Parent.Right;
                    if (other != sentinal && other.Red)
                    {
                        newNode.Parent.Red = false;
                        other.Red = false;
                        newNode.Parent.Parent.Red = true;
                        newNode = newNode.Parent.Parent;
                    }
                    else
                    {
                        if (newNode == newNode.Parent.Right)
                        {
                            newNode = newNode.Parent;
                            LeftRotate(newNode);
                        }
                        newNode.Parent.Red = false;
                        newNode.Parent.Parent.Red = true;
                        RightRotate(newNode.Parent.Parent);
                    }
                }
                else
                {
                    TreeNode other = newNode.Parent.Parent.Left;
                    if (other != sentinal && other.Red)
                    {
                        newNode.Parent.Red = false;
                        other.Red = false;
                        newNode.Parent.Parent.Red = true;
                        newNode = newNode.Parent.Parent;
                    }
                    else
                    {
                        if (newNode == newNode.Parent.Left)
                        {
                            newNode = newNode.Parent;
                            RightRotate(newNode);
                        }
                        newNode.Parent.Red = false;
                        newNode.Parent.Parent.Red = true;
                        LeftRotate(newNode.Parent.Parent);
                    }
                }
            }
        }

        private void LeftRotate(TreeNode newNode)
        {
            TreeNode other = newNode.Right;
            if (other == sentinal)
                throw new InvalidOperationException("This should not happen.");
            newNode.Right = other.Left;
            if (other.Left != sentinal)
                other.Left.Parent = newNode;
            other.Parent = newNode.Parent;
            if (newNode.Parent == sentinal)
                treeRoot = other;
            else
            {
                if (newNode == newNode.Parent.Left)
                    newNode.Parent.Left = other;
                else
                    newNode.Parent.Right = other;
            }
            other.Left = newNode;
            newNode.Parent = other;
            UpdateCount(newNode);
            UpdateCount(other);
        }

        private void RightRotate(TreeNode newNode)
        {
            TreeNode other = newNode.Left;
            if (other == sentinal)
                throw new InvalidOperationException("This should not happen.");
            newNode.Left = other.Right;
            if (other.Right != sentinal)
                other.Right.Parent = newNode;
            other.Parent = newNode.Parent;
            if (newNode.Parent == sentinal)
                treeRoot = other;
            else
            {
                if (newNode == newNode.Parent.Right)
                    newNode.Parent.Right = other;
                else
                    newNode.Parent.Left = other;
            }
            other.Right = newNode;
            newNode.Parent = other;
            UpdateCount(newNode);
            UpdateCount(other);
        }

        private static int SubTreeIndex(TreeNode node)
        {
            return node.Count - 1 - node.Right.Count;
        }


        private void RemoveNode(TreeNode node)
        {
            TreeNode other;
            if (node.Left == sentinal || node.Right == sentinal)
                other = node;
            else
                other = FullLeft(node.Right);
            TreeNode other2;
            if (other.Left != sentinal)
                other2 = other.Left;
            else
                other2 = other.Right;
            other2.Parent = other.Parent;
            if (other.Parent == sentinal)
                treeRoot = other2;
            else
            {
                if (other == other.Parent.Left)
                    other.Parent.Left = other2;
                else
                    other.Parent.Right = other2;
            }
            if (other != node)
            {
                node.Value = other.Value;
            }
            TreeNode backWalk = other.Parent;
            while (backWalk != sentinal)
            {
                backWalk.Count--;
                backWalk = backWalk.Parent;
            }
            if (!other.Red)
                FixRedBlackDelete(other2);
        }

        private void FixRedBlackDelete(TreeNode node)
        {
            while (node != treeRoot && !node.Red)
            {
                if (node == node.Parent.Left)
                {
                    TreeNode other = node.Parent.Right;
                    if (other.Red)
                    {
                        other.Red = false;
                        node.Parent.Red = true;
                        LeftRotate(node.Parent);
                        other = node.Parent.Right;
                    }
                    if (!other.Left.Red && !other.Right.Red)
                    {
                        other.Red = true;
                        node = node.Parent;
                    }
                    else
                    {
                        if (!other.Right.Red)
                        {
                            other.Left.Red = false;
                            other.Red = true;
                            RightRotate(other);
                            other = node.Parent.Right;
                        }
                        other.Red = node.Parent.Red;
                        node.Parent.Red = false;
                        other.Right.Red = false;
                        LeftRotate(node.Parent);
                        node = treeRoot;
                    }
                }
                else
                {
                    TreeNode other = node.Parent.Left;
                    if (other.Red)
                    {
                        other.Red = false;
                        node.Parent.Red = true;
                        RightRotate(node.Parent);
                        other = node.Parent.Left;
                    }
                    if (!other.Right.Red && !other.Left.Red)
                    {
                        other.Red = true;
                        node = node.Parent;
                    }
                    else
                    {
                        if (!other.Left.Red)
                        {
                            other.Right.Red = false;
                            other.Red = true;
                            LeftRotate(other);
                            other = node.Parent.Left;
                        }
                        other.Red = node.Parent.Red;
                        node.Parent.Red = false;
                        other.Left.Red = false;
                        RightRotate(node.Parent);
                        node = treeRoot;
                    }
                }
            }
            node.Red = false;
        }

        private TreeNode FullLeft(TreeNode treeNode)
        {
            while (treeNode.Left != sentinal)
            {
                treeNode = treeNode.Left;
            }
            return treeNode;
        }

        private TreeNode TreeIndexer(TreeNode node, int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must not be negative.");
            else if (index >= Count)
                throw new ArgumentException("Index must not be beyond the end of the list.");
            while (node != null)
            {
                int subTreeIndex = SubTreeIndex(node);
                if (index < subTreeIndex)
                    node = node.Left;
                else if (index == subTreeIndex)
                    return node;
                else
                {
                    index -= subTreeIndex + 1;
                    node = node.Right;
                }
            }
            throw new InvalidOperationException("This code should never be reached.");
        }

        #endregion

        private int version;

        #region IList<T> Members

        /// <summary>
        /// Gets the index of the specified item in the overall list.
        /// </summary>
        /// <param name="item">
        /// Item to search for.
        /// </param>
        /// <returns>
        /// The index of the item if it is in the list, otherwise -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return TreeIndex(treeRoot, item);
        }

        /// <summary>
        /// Inserts the specified item at the specified location in the overall list.
        /// </summary>
        /// <param name="index">
        /// Index to insert at.
        /// </param>
        /// <param name="item">
        /// Item to be inserted.
        /// </param>
        public void Insert(int index, T item)
        {
            if (index < 0)
                throw new ArgumentException("Index is less than zero.");
            if (index > Count)
               throw new ArgumentException("Index is greater than the size of the list.");
            TreeNode newNode = new TreeNode();
            newNode.Count = 1;
            newNode.Value = item;
            newNode.Red = true;
            newNode.Parent = sentinal;
            newNode.Left = sentinal;
            newNode.Right = sentinal;
            if (treeRoot == sentinal)
                treeRoot = newNode;
            else
                TreeInsert(treeRoot, newNode, index);
            treeRoot.Red = false;
            version++;
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index to remove at.
        /// </param>
        public void RemoveAt(int index)
        {
            TreeNode node = TreeIndexer(treeRoot, index);
            RemoveNode(node);
        }

        /// <summary>
        /// Gets or sets the value at a specified index.
        /// </summary>
        /// <param name="index">
        /// Index to perform the get or set operation at.
        /// </param>
        /// <returns>
        /// The value at the specified index.
        /// </returns>
        public T this[int index]
        {
            get
            {
                TreeNode node = TreeIndexer(treeRoot, index);
                return node.Value;
            }
            set
            {
                TreeNode node = TreeIndexer(treeRoot, index);
                node.Value = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Adds the specified item to the end of the list.
        /// </summary>
        /// <param name="item">
        /// Item to be added to the end of the list.
        /// </param>
        public void Add(T item)
        {
            Insert(Count, item);
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            treeRoot = sentinal;
            version++;
        }

        /// <summary>
        /// Checks whether the list contains the specified item.
        /// </summary>
        /// <param name="item">
        /// Item to search for.
        /// </param>
        /// <returns>
        /// True if the list contains the specified item, false otherwise.
        /// </returns>
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary>
        /// Copies the contents of this list to the specified array.
        /// </summary>
        /// <param name="array">
        /// Array to copy to.
        /// </param>
        /// <param name="arrayIndex">
        /// Offset into target array to start copying to.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Count + arrayIndex > array.Length)
                throw new ArgumentException("Specified array and array index not sufficient to contain this list.");
            TreeCopy(treeRoot, array, ref arrayIndex);
        }

        private void TreeCopy(TreeNode node, T[] array, ref int arrayIndex)
        {
            if (node == sentinal)
                return;
            TreeCopy(node.Left, array, ref arrayIndex);
            array[arrayIndex] = node.Value;
            arrayIndex++;
            TreeCopy(node.Right, array, ref arrayIndex);
        }

        /// <summary>
        /// Gets the count of the number of items in the list.
        /// </summary>
        public int Count
        {
            get
            {
                return treeRoot.Count;
            }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified item from the list if it exists.
        /// </summary>
        /// <param name="item">
        /// Item to try and remove from the list.
        /// </param>
        /// <returns>
        /// True if the item was found and removed from the list, false otherwise.
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
        /// Gets the enumerator for this list.
        /// </summary>
        /// <returns>
        /// An enumerator to enumerate the values stored in this list.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new TreeListEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the untyped enumerator for this list.
        /// </summary>
        /// <returns>
        /// An enumerator to enumerate the objects stored in this list.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new TreeListEnumerator(this);
        }

        #endregion

        /// <summary>
        /// Enumerator for tree lists.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct TreeListEnumerator : IEnumerator<T>
        {

            /// <summary>
            /// Internal constructor.
            /// </summary>
            /// <param name="parent">
            /// Parent tree list to enumerate.
            /// </param>
            internal TreeListEnumerator(TreeList<T> parent)
            {
                this.parent = parent;
                origVersion = parent.version;
                currentNode = null;
                offEnd = false;
            }

            private TreeList<T> parent;
            private int origVersion;
            private TreeNode currentNode;
            private bool offEnd;

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the current item this enumerator points to.
            /// </summary>
            public T Current
            {
                get
                {
                    if (currentNode == null)
                        return default(T);
                    else
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
            /// Gets the current object the enumerator points to.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (currentNode == null)
                        throw new InvalidOperationException("Current is not pointing to a valid node.");
                    return this.Current;
                }
            }

            /// <summary>
            /// Moves to the next node if possible.
            /// </summary>
            /// <returns>
            /// True if moved to the next node, false otherwise.
            /// </returns>
            public bool MoveNext()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException("The collection being enumerated has been modified since enumerator was acquired.");
                if (!offEnd && currentNode == null)
                {
                    if (parent.treeRoot == parent.sentinal)
                    {
                        offEnd = true;
                        return false;
                    }
                    else
                    {
                        currentNode = parent.FullLeft(parent.treeRoot);
                        return true;
                    }
                }
                else
                {
                    if (currentNode.Right != parent.sentinal)
                    {
                        currentNode = parent.FullLeft(currentNode.Right);
                        return true;
                    }
                    else
                    {
                        TreeNode other = currentNode.Parent;
                        while (other != parent.sentinal && currentNode == other.Right)
                        {
                            currentNode = other;
                            other = other.Parent;
                        }
                        if (other != parent.sentinal)
                        {
                            currentNode = other;
                            return true;
                        }
                        else
                        {
                            currentNode = null;
                            offEnd = true;
                            return false;
                        }
                    }
                }
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            void System.Collections.IEnumerator.Reset()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException("The collection being enumerated has been modified since enumerator was acquired.");
                currentNode = null;
                offEnd = false;
            }

            #endregion
        }

    }
}
