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
    /// Works with a set of items which are ordered.  From this ordering the items can be partitioned into distinct groups.
    /// Provides O(log N) time joining of adjacent groups, splitting of a group into 2, updating ordering of an element and determining which group an element belongs too.
    /// Joining of adjacent groups is not validated for correctness except that the largest of the first must be less or equal to the smallest of the last.
    /// Updating ordering of an element is not validated to ensure that it doesn't cause overlap with another group, it is presumed that the user will join the groups, update, split, if that is what is intended.
    /// Adding a new element does not automatically join a set which straddles its ordering, it is presumed this event will never occur.
    /// </summary>
    /// <typeparam name="TElement">
    /// Type of item being tracked.
    /// </typeparam>
    /// <typeparam name="TRep">
    /// Type of the representative to store information about a given disjoint set.
    /// </typeparam>
    public class OrderedSplittableDisjointTracker<TElement, TRep>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public OrderedSplittableDisjointTracker() : this(null, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Equality comparer to use to lookup entries in the internal storage.
        /// </param>
        /// <param name="orderingComparer">
        /// Ordering comparer to order the elements for splitting/joining.
        /// </param>
        public OrderedSplittableDisjointTracker(IEqualityComparer<TElement> comparer,
            IComparer<TElement> orderingComparer)
        {
            if (comparer == null)
                comparer = EqualityComparer<TElement>.Default;
            if (orderingComparer == null)
                orderingComparer = Comparer<TElement>.Default;
            this.comparer = comparer;
            this.orderingComparer = orderingComparer;

            tracker = new Dictionary<TElement, int>(comparer);
            reps = new Dictionary<TRep, int>();
            nodes = new List<Node>();
        }

        private struct Node
        {
            public Node(TElement value, TRep represenative)
            {
                Left = -1;
                Right = -1;
                Parent = -1;
                Value = value;
                Red = false;
                Representative = represenative;
            }

            public TElement Value;
            public int Left;
            public int Right;
            public int Parent;
            public bool Red;
            public TRep Representative;
        }

        private readonly Dictionary<TElement, int> tracker;
        private readonly Dictionary<TRep, int> reps;
        private readonly List<Node> nodes;
        private int firstFree = -1;
        private IEqualityComparer<TElement> comparer;
        private readonly IComparer<TElement> orderingComparer;


        /// <summary>
        /// Adds a new item to be tracked. 
        /// </summary>
        /// <param name="value">
        /// Value to be added to the tracking.
        /// </param>
        /// <param name="representative">
        /// Initial representative for the value.
        /// </param>
        public void Add(TElement value, TRep representative)
        {
            int newSpot;
            if (firstFree < 0)
            {
                nodes.Add(new Node(value, representative));
                newSpot = nodes.Count - 1;
            }
            else
            {
                newSpot = firstFree;
                firstFree = nodes[firstFree].Left;
                nodes[newSpot] = new Node(value, representative);
            }
            tracker[value] = newSpot;
            reps[representative] = newSpot;
        }

        /// <summary>
        /// Handles a change in ordering for the given element.
        /// </summary>
        /// <param name="value">
        /// Element which has had a change in ordering.
        /// </param>
        public void Update(TElement value)
        {
            int spot = tracker[value];
            // If the tree contains a single node, we do nothing since there is nothing to fix.
            if (nodes[spot].Parent == -1 && nodes[spot].Left == -1 && nodes[spot].Right == -1)
                return;
            TRep rep = GetRepresentative(value);
            SetRep(reps[rep], default(TRep));

            // Since we handle case with single node, remove from tree there will always be a tree left.
            int root = GetRoot(spot);
            int newRoot = RemoveFromTree(spot);
            if (newRoot >= -1)
                root = newRoot;
            newRoot = AddToTree(root, spot);
            if (newRoot >= -1)
                root = newRoot;
            SetRep(root, rep);
            reps[rep] = root;
        }

        private int AddToTree(int node, int newNode)
        {
            // New nodes always start red.
            SetRed(newNode, true);
            int newRoot = -2;
            int other = -1;
            bool lastLeft = false;
            while (node != -1)
            {
                other = node;
                int order = orderingComparer.Compare(nodes[newNode].Value, nodes[node].Value);
                if (order <= 0)
                {
                    node = nodes[node].Left;
                    lastLeft = true;
                }
                else
                {
                    node = nodes[node].Right;
                    lastLeft = false;
                }
            }
            SetParent(newNode, other);
            if (other != -1)
            {
                if (lastLeft)
                    SetLeft(nodes[newNode].Parent, newNode);
                else
                    SetRight(nodes[newNode].Parent, newNode);
            }

            TNR(FixRedBlackInsert(newNode), ref newRoot);
            return newRoot;
        }

        private void SetParent(int newNode, int other)
        {
            Node newNodeNodeValue = nodes[newNode];
            newNodeNodeValue.Parent = other;
            nodes[newNode] = newNodeNodeValue;
        }

        private void SetLeft(int newNode, int other)
        {
            Node newNodeNodeValue = nodes[newNode];
            newNodeNodeValue.Left = other;
            nodes[newNode] = newNodeNodeValue;
        }

        private void SetRight(int newNode, int other)
        {
            Node newNodeNodeValue = nodes[newNode];
            newNodeNodeValue.Right = other;
            nodes[newNode] = newNodeNodeValue;
        }

        private void SetRed(int newNode, bool other)
        {
            Node newNodeNodeValue = nodes[newNode];
            newNodeNodeValue.Red = other;
            nodes[newNode] = newNodeNodeValue;
        }

        private int GetParent(int node)
        {
            return nodes[node].Parent;
        }

        private int GetLeft(int node)
        {
            return nodes[node].Left;
        }

        private int GetRight(int node)
        {
            return nodes[node].Right;
        }

        private bool GetRed(int node)
        {
            if (node < 0)
                return false;
            return nodes[node].Red;
        }

        private int FixRedBlackInsert(int newNode)
        {
            int newRoot = -2;
            while (GetParent(newNode) != -1 && nodes[GetParent(newNode)].Red)
            {
                if (GetParent(newNode) == GetLeft(GetParent(GetParent(newNode))))
                {
                    int other = GetRight(GetParent(GetParent(newNode)));
                    if (other != -1 && GetRed(other))
                    {
                        SetRed(GetParent(newNode), false);
                        SetRed(other, false);
                        SetRed(GetParent(GetParent(newNode)), true);
                        newNode = GetParent(GetParent(newNode));
                    }
                    else
                    {
                        if (newNode == GetRight(GetParent(newNode)))
                        {
                            newNode = GetParent(newNode);
                            TNR(LeftRotate(newNode), ref newRoot);
                        }
                        SetRed(GetParent(newNode), false);
                        SetRed(GetParent(GetParent(newNode)), true);
                        TNR(RightRotate(GetParent(GetParent(newNode))), ref newRoot);
                    }
                }
                else
                {
                    int other = GetLeft(GetParent(GetParent(newNode)));
                    if (other != -1 && GetRed(other))
                    {
                        SetRed(GetParent(newNode), false);
                        SetRed(other, false);
                        SetRed(GetParent(GetParent(newNode)), true);
                        newNode = GetParent(GetParent(newNode));
                    }
                    else
                    {
                        if (newNode == GetLeft(GetParent(newNode)))
                        {
                            newNode = GetParent(newNode);
                            TNR(RightRotate(newNode), ref newRoot);
                        }
                        SetRed(GetParent(newNode), false);
                        SetRed(GetParent(GetParent(newNode)), true);
                        TNR(LeftRotate(GetParent(GetParent(newNode))), ref newRoot);
                    }
                }
            }
            // Root must always be black for some extended logic to work.
            if (GetParent(newNode) == -1 && GetRed(newNode))
            {
                SetRed(newNode, false);
            }
            return newRoot;
        }

        private int LeftRotate(int newNode)
        {
            int newRoot = -2;
            int other = GetRight(newNode);
            if (other == -1)
                throw new InvalidOperationException("This should not happen.");
            SetRight(newNode, GetLeft(other));
            if (GetLeft(other) != -1)
                SetParent(GetLeft(other), newNode);
            SetParent(other, GetParent(newNode));
            if (GetParent(newNode) == -1)
                newRoot = other;
            else
            {
                if (newNode == GetLeft(GetParent(newNode)))
                    SetLeft(GetParent(newNode), other);
                else
                    SetRight(GetParent(newNode), other);
            }
            SetLeft(other, newNode);
            SetParent(newNode, other);
            return newRoot;
        }

        private int RightRotate(int newNode)
        {
            int newRoot = -2;
            int other = GetLeft(newNode);
            if (other == -1)
                throw new InvalidOperationException("This should not happen.");
            SetLeft(newNode, GetRight(other));
            if (GetRight(other) != -1)
                SetParent(GetRight(other), newNode);
            SetParent(other, GetParent(newNode));
            if (GetParent(newNode) == -1)
                newRoot = other;
            else
            {
                if (newNode == GetRight(GetParent(newNode)))
                    SetRight(GetParent(newNode), other);
                else
                    SetLeft(GetParent(newNode), other);
            }
            SetRight(other, newNode);
            SetParent(newNode, other);
            return newRoot;
        }


        private int RemoveFromTree(int node)
        {
            int newRoot = -2;
            int other;
            if (GetLeft(node) == -1 || GetRight(node) == -1)
                other = node;
            else
                other = FullLeft(GetRight(node));
            int other2;
            if (GetLeft(other) != -1)
                other2 = GetLeft(other);
            else
                other2 = GetRight(other);

            int defaultParent = GetParent(other);
            if (other2 != -1)
                SetParent(other2, defaultParent);
            if (GetParent(other) == -1)
                newRoot = other2;
            else
            {
                if (other == GetLeft(GetParent(other)))
                    SetLeft(GetParent(other), other2);
                else
                    SetRight(GetParent(other), other2);
            }
            if (other != node)
            {
                Node n = nodes[node];
                n.Value = nodes[other].Value;
                n.Representative = nodes[other].Representative;
                nodes[node] = n;
            }
            if (!GetRed(other))
                TNR(FixRedBlackDelete(other2, defaultParent), ref newRoot);
            return newRoot;
        }

        private void TNR(int potential, ref int newRoot)
        {
            if (potential >= -1)
                newRoot = potential;
        }

        private int GetParent(int node, int defaultParent)
        {
            if (node == -1)
                return defaultParent;
            return GetParent(node);
        }

        private int FixRedBlackDelete(int node, int defaultParent)
        {
            int newRoot = -2;
            while (GetParent(node, defaultParent) != -1 && !GetRed(node))
            {
                if (node == GetLeft(GetParent(node, defaultParent)))
                {
                    int other = GetRight(GetParent(node, defaultParent));
                    if (GetRed(other))
                    {
                        SetRed(other, false);
                        SetRed(GetParent(node, defaultParent), true);
                        TNR(LeftRotate(GetParent(node, defaultParent)), ref newRoot);
                        other = GetRight(GetParent(node, defaultParent));
                    }
                    if (!GetRed(GetLeft(other)) && !GetRed(GetRight(other)))
                    {
                        SetRed(other, true);
                        node = GetParent(node, defaultParent);
                    }
                    else
                    {
                        if (!GetRed(GetRight(other)))
                        {
                            SetRed(GetLeft(other), false);
                            SetRed(other, true);
                            TNR(RightRotate(other), ref newRoot);
                            other = GetRight(GetParent(node, defaultParent));
                        }
                        SetRed(other, GetRed(GetParent(node, defaultParent)));
                        SetRed(GetParent(node, defaultParent), false);
                        SetRed(GetRight(other), false);
                        TNR(LeftRotate(GetParent(node, defaultParent)), ref newRoot);
                        if (newRoot > -2)
                            node = newRoot;
                        else
                        {
                            // TODO: determine if this is too expensive (since we don't pass in the root node).
                            node = FullParent(GetParent(node, defaultParent));
                        }
                    }
                }
                else
                {
                    int other = GetLeft(GetParent(node, defaultParent));
                    if (GetRed(other))
                    {
                        SetRed(other, false);
                        SetRed(GetParent(node, defaultParent), true);
                        TNR(RightRotate(GetParent(node, defaultParent)), ref newRoot);
                        other = GetLeft(GetParent(node, defaultParent));
                    }
                    if (!GetRed(GetRight(other)) && !GetRed(GetLeft(other)))
                    {
                        SetRed(other, true);
                        node = GetParent(node, defaultParent);
                    }
                    else
                    {
                        if (!GetRed(GetLeft(other)))
                        {
                            SetRed(GetRight(other), false);
                            SetRed(other, true);
                            TNR(LeftRotate(other), ref newRoot);
                            other = GetLeft(GetParent(node, defaultParent));
                        }
                        SetRed(other, GetRed(GetParent(node, defaultParent)));
                        SetRed(GetParent(node, defaultParent), false);
                        SetRed(GetLeft(other), false);
                        TNR(RightRotate(GetParent(node, defaultParent)), ref newRoot);
                        if (newRoot > -2)
                            node = newRoot;
                        else
                        {
                            // TODO: determine if this is too expensive (since we don't pass in the root node).
                            node = FullParent(GetParent(node, defaultParent));
                        }
                    }
                }
            }
            if (node != -1)
                SetRed(node, false);
            return newRoot;
        }

        private int FullParent(int node)
        {
            while (GetParent(node) != -1)
            {
                node = GetParent(node);
            }
            return node;
        }

        private int FullLeft(int treeNode)
        {
            while (GetLeft(treeNode) != -1)
            {
                treeNode = GetLeft(treeNode);
            }
            return treeNode;
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
        /// <param name="mergeFunction">
        /// Function for </param>
        public void Union(TElement first, TElement second, Func<TRep, TRep, TRep> mergeFunction)
        {
            TRep firstRep = GetRepresentative(first);
            TRep secondRep = GetRepresentative(second);
            TRep newRep = mergeFunction(firstRep, secondRep);
            Link(firstRep, secondRep, newRep);
        }

        /// <summary>
        /// Breaks the set which contains into split point into two, one with everything up to and including it, and everything else.
        /// </summary>
        /// <param name="splitPoint">
        /// Point to control the split.</param>
        /// <param name="splitFunction">
        /// Function to create a new representative for the right half of the split. (And potentially update the current representative for the first.)
        /// </param>
        public void Split(TElement splitPoint, Func<TRep, TRep> splitFunction)
        {
            TRep splitRep = GetRepresentative(splitPoint);
            Split(splitRep, splitPoint, splitFunction(splitRep));
        }

        private void Split(TRep splitRep, TElement splitPoint, TRep newRep)
        {
            int splitRoot = reps[splitRep];
            SetRep(splitRoot, default(TRep));

            int splitRootHeight = 0;

            int newFirstRoot = -1;
            int firstPivot = -1;
            int firstPosHeight = -1;
            int firstPos = -1;

            int newSecondRoot = -1;
            int secondPivot = -1;
            int secondPos = -1;
            int secondPosHeight = -1;

            while (splitRoot != -1)
            {
                if (!GetRed(splitRoot))
                    splitRootHeight--;

                int order = orderingComparer.Compare(nodes[splitRoot].Value, splitPoint);
                if (order > 0)
                {
                    int newTree = GetRight(splitRoot);
                    if (newTree != -1)
                    {
                        int targetHeight = splitRootHeight;
                        if (GetRed(newTree))
                        {
                            SetRed(newTree, false);
                            targetHeight++;
                        }
                        if (secondPos != -1)
                        {
                            while (secondPosHeight != targetHeight)
                            {
                                if (!GetRed(secondPos))
                                    secondPosHeight--;
                                secondPos = GetLeft(secondPos);
                            }
                            while (GetRed(secondPos))
                                secondPos = GetLeft(secondPos);
                        }
                        SetParent(newTree, -1);
                        int parent = secondPos == -1 ? -1 : GetParent(secondPos);
                        TNR(SplitConcat(newTree, secondPos, secondPivot, parent, true), ref newSecondRoot);
                        if (secondPivot == -1)
                        {
                            secondPos = newSecondRoot;
                            secondPosHeight = targetHeight;
                        }
                        else
                        {
                            secondPos = secondPivot;
                            secondPosHeight = targetHeight + (GetRed(secondPivot) ? 0 : 1);
                        }
                    }
                    else if (secondPivot != -1)
                    {
                        if (secondPos == -1)
                        {
                            newSecondRoot = secondPivot;
                            SetRed(newSecondRoot, false);
                            secondPos = newSecondRoot;
                            secondPosHeight = splitRootHeight;
                        }
                        else
                        {
                            TNR(AddToTree(secondPos, secondPivot), ref newSecondRoot);
                            secondPos = secondPivot;
                            secondPosHeight = splitRootHeight;
                        }
                    }
                    secondPivot = splitRoot;
                    splitRoot = GetLeft(splitRoot);
                    SetParent(secondPivot, -1);
                    SetLeft(secondPivot, -1);
                    SetRight(secondPivot, -1);
                    SetRed(secondPivot, true);
                }
                else if (order < 0)
                {
                    int newTree = GetLeft(splitRoot);
                    if (newTree != -1)
                    {
                        int targetHeight = splitRootHeight;
                        if (GetRed(newTree))
                        {
                            SetRed(newTree, false);
                            targetHeight++;
                        }
                        if (firstPos != -1)
                        {
                            while (firstPosHeight != targetHeight)
                            {
                                if (!GetRed(firstPos))
                                    firstPosHeight--;
                                firstPos = GetRight(firstPos);
                            }
                            while (GetRed(firstPos))
                                firstPos = GetRight(firstPos);
                        }
                        SetParent(newTree, -1);
                        int parent = firstPos == -1 ? -1 : GetParent(firstPos);
                        TNR(SplitConcat(firstPos, newTree, firstPivot, parent, false), ref newFirstRoot);
                        if (firstPivot == -1)
                        {
                            firstPos = newFirstRoot;
                            firstPosHeight = targetHeight;
                        }
                        else
                        {
                            firstPos = firstPivot;
                            firstPosHeight = targetHeight + (GetRed(firstPivot) ? 0 : 1);
                        }
                    }
                    else if (firstPivot != -1)
                    {
                        if (firstPos == -1)
                        {
                            newFirstRoot = firstPivot;
                            SetRed(newFirstRoot, false);
                            firstPos = newFirstRoot;
                            firstPosHeight = splitRootHeight;
                        }
                        else
                        {
                            TNR(AddToTree(firstPos, firstPivot), ref newFirstRoot);
                            firstPos = firstPivot;
                            firstPosHeight = splitRootHeight;
                        }
                    }
                    firstPivot = splitRoot;
                    splitRoot = GetRight(splitRoot);
                    SetParent(firstPivot, -1);
                    SetLeft(firstPivot, -1);
                    SetRight(firstPivot, -1);
                    SetRed(firstPivot, true);
                }
                else
                {
                    int newTree = GetLeft(splitRoot);
                    if (newTree != -1)
                    {
                        int targetHeight = splitRootHeight;
                        if (GetRed(newTree))
                        {
                            SetRed(newTree, false);
                            targetHeight++;
                        }
                        if (firstPos != -1)
                        {
                            while (firstPosHeight != targetHeight)
                            {
                                if (!GetRed(firstPos))
                                    firstPosHeight--;
                                firstPos = GetRight(firstPos);
                            }
                            while (GetRed(firstPos))
                                firstPos = GetRight(firstPos);
                        }
                        SetParent(newTree, -1);
                        int parent = firstPos == -1 ? -1 : GetParent(firstPos);
                        TNR(SplitConcat(firstPos, newTree, firstPivot, parent, false), ref newFirstRoot);
                    }
                    else if (firstPivot != -1)
                    {
                        if (firstPos == -1)
                        {
                            newFirstRoot = firstPivot;
                            SetRed(newFirstRoot, false);
                            firstPos = newFirstRoot;
                            firstPosHeight = splitRootHeight;
                        }
                        else
                            TNR(AddToTree(firstPos, firstPivot), ref newFirstRoot);
                    }

                    newTree = GetRight(splitRoot);
                    if (newTree != -1)
                    {
                        int targetHeight = splitRootHeight;
                        if (GetRed(newTree))
                        {
                            SetRed(newTree, false);
                            targetHeight++;
                        }
                        if (secondPos != -1)
                        {
                            while (secondPosHeight != targetHeight)
                            {
                                if (!GetRed(secondPos))
                                    secondPosHeight--;
                                secondPos = GetLeft(secondPos);
                            }
                            while (GetRed(secondPos))
                                secondPos = GetLeft(secondPos);
                        }
                        SetParent(newTree, -1);
                        int parent = secondPos == -1 ? -1 : GetParent(secondPos);
                        TNR(SplitConcat(newTree, secondPos, secondPivot, parent, true), ref newSecondRoot);
                    }
                    else if (secondPivot != -1)
                    {
                        if (secondPos == -1)
                        {
                            newSecondRoot = secondPivot;
                            SetRed(newSecondRoot, false);
                            secondPos = newSecondRoot;
                            secondPosHeight = splitRootHeight;
                        }
                        else
                            TNR(AddToTree(secondPos, secondPivot), ref newSecondRoot);
                    }
                    SetRed(splitRoot, true);
                    SetParent(splitRoot, -1);
                    SetLeft(splitRoot, -1);
                    SetRight(splitRoot, -1);
                    if (newFirstRoot == -1)
                    {
                        newFirstRoot = splitRoot;
                        SetRed(newFirstRoot, false);
                    }
                    else
                        TNR(AddToTree(newFirstRoot, splitRoot), ref newFirstRoot);
                    firstPivot = -1;
                    secondPivot = -1;
                    break;
                }
            }
            if (firstPivot != -1)
            {
                if (newFirstRoot == -1)
                {
                    newFirstRoot = firstPivot;
                    SetRed(newFirstRoot, false);
                }
                else
                    TNR(AddToTree(newFirstRoot, firstPivot), ref newFirstRoot);
            }
            if (secondPivot != -1)
            {
                if (newSecondRoot == -1)
                {
                    newSecondRoot = secondPivot;
                    SetRed(newSecondRoot, false);
                }
                else
                    TNR(AddToTree(newSecondRoot, secondPivot), ref newSecondRoot);
            }
            reps[splitRep] = newFirstRoot;
            SetRep(newFirstRoot, splitRep);
            if (newSecondRoot != -1)
            {
                reps[newRep] = newSecondRoot;
                SetRep(newSecondRoot, newRep);
            }
        }

        internal void ___TESTCheckTreesTEST()
        {
            HashSet<int> seen = new HashSet<int>();
            foreach (int root in reps.Values)
            {
                if (GetRed(root))
                    throw new InvalidOperationException("Tree root must be black");
                CheckDistinct(root, seen);
                CheckRedBlack(root);
                int depth = GetBlackDepthDown(root);
                CheckDepthMatch(root, depth);
                CheckOrdering(root);
                // Ensure that every pair of roots doesn't have overlapping ends.
                foreach (int otherRoot in reps.Values)
                {
                    if (root == otherRoot)
                        continue;
                    int order = orderingComparer.Compare(nodes[root].Value, nodes[otherRoot].Value);
                    if (order < 0)
                        CheckOverlap(root, otherRoot);
                    else
                        CheckOverlap(otherRoot, root);
                }
            }
        }

        private void CheckOrdering(int root)
        {
            if (root == -1)
                return;
            if (GetLeft(root) != -1)
                CheckOrdering(GetLeft(root), nodes[root].Value, false);
            if (GetRight(root) != -1)
                CheckOrdering(GetRight(root), nodes[root].Value, true);
            CheckOrdering(GetLeft(root));
            CheckOrdering(GetRight(root));
        }

        private void CheckOrdering(int root, TElement comparePoint, bool right)
        {
            if (root == -1)
                return;
            if (right)
            {
                if (orderingComparer.Compare(nodes[root].Value, comparePoint) < 0)
                    throw new InvalidOperationException("Tree is not in order.");
            }
            else
            {
                if (orderingComparer.Compare(nodes[root].Value, comparePoint) > 0)
                    throw new InvalidOperationException("Tree is not in order.");
            }
            CheckOrdering(GetLeft(root), comparePoint, right);
            CheckOrdering(GetRight(root), comparePoint, right);
        }

        private void CheckDistinct(int root, HashSet<int> seen)
        {
            if (root == -1)
                return;
            if (seen.Contains(root))
                throw new InvalidOperationException(
                    "Every node must be in only one tree, and only appear in that tree once.");
            seen.Add(root);
            CheckDistinct(GetLeft(root), seen);
            CheckDistinct(GetRight(root), seen);
        }

        private void CheckDepthMatch(int root, int depth)
        {
            if (root == -1)
                return;
            if ((GetLeft(root) == -1 || GetRight(root) == -1) && GetBlackDepthUp(root) != depth)
                throw new InvalidOperationException(
                    "All parent path black depths from nodes without 2 children must match the left most path black depth.");
            CheckDepthMatch(GetLeft(root), depth);
            CheckDepthMatch(GetRight(root), depth);
        }

        private int GetBlackDepthUp(int root)
        {
            if (root == -1)
                return 0;
            int child = GetBlackDepthUp(GetParent(root));
            if (!GetRed(root))
                return 1 + child;
            return child;
        }

        private int GetBlackDepthDown(int root)
        {
            if (root == -1)
                return 0;
            int child = GetBlackDepthDown(GetLeft(root));
            if (!GetRed(root))
                return 1 + child;
            return child;
        }

        private void CheckRedBlack(int root)
        {
            if (root == -1)
                return;
            if (GetRed(root) && (GetRed(GetLeft(root)) || GetRed(GetRight(root))))
                throw new InvalidOperationException("Red nodes must have black children.");
            CheckRedBlack(GetLeft(root));
            CheckRedBlack(GetRight(root));
        }

        /// <summary>
        /// Gets the representative of the set which a given value belongs to.
        /// </summary>
        /// <param name="value">
        /// Value to lookup.
        /// </param>
        /// <returns>
        /// The representative of the set the element belongs to.
        /// </returns>
        public TRep GetRepresentative(TElement value)
        {
            int pos = GetRoot(value);
            return nodes[pos].Representative;
        }

        private int GetRoot(TElement value)
        {
            int pos = tracker[value];
            pos = GetRoot(pos);
            return pos;
        }

        private int GetRoot(int pos)
        {
            while (nodes[pos].Parent != -1)
                pos = nodes[pos].Parent;
            return pos;
        }

        private void Link(TRep first, TRep second, TRep newRep)
        {
            int firstRoot = reps[first];
            int secondRoot = reps[second];
            if (firstRoot == secondRoot)
                return;
            CheckOverlap(firstRoot, secondRoot);

            SetRep(firstRoot, default(TRep));
            SetRep(secondRoot, default(TRep));
            reps.Remove(first);
            reps.Remove(second);

            int newRoot = Concat(firstRoot, secondRoot);

            SetRep(newRoot, newRep);
            reps[newRep] = newRoot;
        }

        private void CheckOverlap(int firstRoot, int secondRoot)
        {
            int rightEdge = firstRoot;
            while (nodes[rightEdge].Right != -1)
                rightEdge = nodes[rightEdge].Right;
            int leftEdge = secondRoot;
            while (nodes[leftEdge].Left != -1)
                leftEdge = nodes[leftEdge].Left;

            if (orderingComparer.Compare(nodes[rightEdge].Value, nodes[leftEdge].Value) > 0)
                throw new InvalidOperationException("Cannot merge two sets which have crossing edges.");
        }

        private void SetRep(int newRoot, TRep newRep)
        {
            Node newRootNodeValue = nodes[newRoot];
            newRootNodeValue.Representative = newRep;
            nodes[newRoot] = newRootNodeValue;
        }

        private int Concat(int firstRoot, int secondRoot)
        {
            if (firstRoot == secondRoot)
                return firstRoot;
            if (secondRoot == -1)
                return firstRoot;
            if (firstRoot == -1)
                return secondRoot;

            int startingLeftEdge = FullLeft(secondRoot);
            int newRoot = RemoveFromTree(startingLeftEdge);
            if (newRoot > -2)
                secondRoot = newRoot;

            int rightDepth = 0;
            int rightEdge = firstRoot;
            if (!nodes[rightEdge].Red)
                rightDepth++;
            while (nodes[rightEdge].Right != -1)
            {
                rightEdge = nodes[rightEdge].Right;
                if (!nodes[rightEdge].Red)
                    rightDepth++;
            }

            int leftDepth = 0;
            int leftEdge = secondRoot;
            if (leftEdge != -1)
            {
                if (!nodes[leftEdge].Red)
                    leftDepth++;
                while (nodes[leftEdge].Left != -1)
                {
                    leftEdge = nodes[leftEdge].Left;
                    if (!nodes[leftEdge].Red)
                        leftDepth++;
                }
            }

            if (rightDepth == leftDepth)
            {
                int root = startingLeftEdge;
                int newRoot2 = ConcatInternal(firstRoot, secondRoot, startingLeftEdge, -1, false);
                if (newRoot2 > -2)
                    root = newRoot2;
                return root;
            }
            else if (rightDepth > leftDepth)
            {
                if (leftEdge == -1)
                {
                    int root2 = firstRoot;
                    int newRoot3 = AddToTree(rightEdge, startingLeftEdge);
                    if (newRoot3 > -2)
                        root2 = newRoot3;
                    return root2;
                }
                int equalPos = rightEdge;
                int depth = GetRed(equalPos) ? 0 : 1;
                while (depth < leftDepth)
                {
                    equalPos = GetParent(equalPos);
                    depth += GetRed(equalPos) ? 0 : 1;
                }

                int root = firstRoot;
                int newRoot2 = ConcatInternal(equalPos, secondRoot, startingLeftEdge, GetParent(equalPos), false);
                if (newRoot2 > -2)
                    root = newRoot2;
                return root;
            }
            else
            {
                int equalPos = leftEdge;
                int depth = GetRed(equalPos) ? 0 : 1;
                while (depth < rightDepth)
                {
                    equalPos = GetParent(equalPos);
                    depth += GetRed(equalPos) ? 0 : 1;
                }

                int root = secondRoot;
                int newRoot2 = ConcatInternal(firstRoot, equalPos, startingLeftEdge, GetParent(equalPos), true);
                if (newRoot2 > -2)
                    root = newRoot2;
                return root;
            }
        }

        private int SplitConcat(int leftEqualPos, int rightEqualPos, int pivot, int parent, bool leftOfParent)
        {
            if (pivot == -1)
            {
                if (leftEqualPos == -1)
                    return rightEqualPos;
                return leftEqualPos;
            }
            return ConcatInternal(leftEqualPos, rightEqualPos, pivot, parent, leftOfParent);
        }

        /// <summary>
        /// Joins two trees at a safe point using a provided pivot.
        /// </summary>
        /// <param name="leftEqualPos">
        /// Point in right edge of left tree which is 'highest', or equal to 'highest' of the right tree.
        /// </param>
        /// <param name="rightEqualPos">
        /// Point in the left edge of the right tree which is 'highest', or equal to 'highest' of the left tree.
        /// </param>
        /// <param name="pivot">
        /// Value in between both trees which can be safely used as a pivot.
        /// </param>
        /// <param name="parent">
        /// Parent to join the pivot to, may be -1 if left and right equal pos are both 'highest' for their respective trees.
        /// </param>
        /// <param name="leftOfParent">
        /// If true, the pivot will be left of the provided parent.
        /// </param>
        /// <returns>
        /// An index of the root of the joined tree if it changes, otherwise -2.
        /// </returns>
        private int ConcatInternal(int leftEqualPos, int rightEqualPos, int pivot, int parent, bool leftOfParent)
        {
            int newRoot = -2;
            SetRed(pivot, true);
            SetParent(pivot, parent);
            if (parent != -1)
            {
                if (leftOfParent)
                    SetLeft(parent, pivot);
                else
                    SetRight(parent, pivot);
            }
            else
            {
                newRoot = pivot;
            }
            SetLeft(pivot, leftEqualPos);
            SetParent(leftEqualPos, pivot);
            SetRight(pivot, rightEqualPos);
            SetParent(rightEqualPos, pivot);
            TNR(FixRedBlackInsert(pivot), ref newRoot);
            return newRoot;
        }
    }
}