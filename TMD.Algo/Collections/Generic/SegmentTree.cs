#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
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
    /// Defines an associative binary monoid operation and identity element for <see cref="SegmentTree{TValue, TPolicy}"/>.
    /// Implement as a value type (struct) for zero-overhead specialization.
    /// </summary>
    /// <typeparam name="TValue">Element and aggregate value type.</typeparam>
    public interface ISegmentTreeMonoid<TValue>
    {
        /// <summary>
        /// Gets the identity element for <see cref="Combine"/>.
        /// </summary>
        TValue Identity { get; }

        /// <summary>
        /// Associatively combines the aggregate value of the left segment with that of the right segment.
        /// </summary>
        /// <param name="left">Aggregate value from the left subsegment.</param>
        /// <param name="right">Aggregate value from the right subsegment.</param>
        /// <returns>The combined aggregate value.</returns>
        TValue Combine(TValue left, TValue right);
    }

    /// <summary>
    /// Defines a monoid with a composable lazy range-update action for <see cref="LazySegmentTree{TValue, TLazy, TPolicy}"/>.
    /// Implement as a value type (struct) for zero-overhead specialization.
    /// </summary>
    /// <typeparam name="TValue">Element and aggregate value type.</typeparam>
    /// <typeparam name="TLazy">Lazy tag type representing deferred range updates.</typeparam>
    public interface ILazySegmentTreePolicy<TValue, TLazy> : ISegmentTreeMonoid<TValue>
    {
        /// <summary>
        /// Gets the no-op identity value for lazy tags.
        /// </summary>
        TLazy LazyIdentity { get; }

        /// <summary>
        /// Determines whether <paramref name="lazy"/> represents a pending update rather than <see cref="LazyIdentity"/>.
        /// </summary>
        /// <param name="lazy">Lazy tag to inspect.</param>
        /// <returns>True if <paramref name="lazy"/> must be pushed/applied; otherwise false.</returns>
        bool HasLazy(TLazy lazy);

        /// <summary>
        /// Applies a lazy update tag to an aggregate node value covering <paramref name="segmentLength"/> leaves.
        /// </summary>
        /// <param name="value">Current aggregate value of the node.</param>
        /// <param name="lazy">Update tag to apply.</param>
        /// <param name="segmentLength">Number of leaves covered by the node.</param>
        /// <returns>The updated aggregate value.</returns>
        TValue Apply(TValue value, TLazy lazy, int segmentLength);

        /// <summary>
        /// Composes an existing pending lazy tag with an incoming update tag.
        /// </summary>
        /// <param name="existing">Previously queued tag (or <see cref="LazyIdentity"/>).</param>
        /// <param name="incoming">Newly applied tag.</param>
        /// <returns>The combined lazy tag equivalent to applying <paramref name="existing"/> then <paramref name="incoming"/>.</returns>
        TLazy Compose(TLazy existing, TLazy incoming);
    }

    /// <summary>
    /// Policy supporting 64-bit range sum queries and range additive updates.
    /// </summary>
    public readonly struct RangeSumAddInt64Policy : ILazySegmentTreePolicy<long, long>
    {
        /// <inheritdoc />
        public long Identity => 0L;

        /// <inheritdoc />
        public long Combine(long left, long right) => left + right;

        /// <inheritdoc />
        public long LazyIdentity => 0L;

        /// <inheritdoc />
        public bool HasLazy(long lazy) => lazy != 0L;

        /// <inheritdoc />
        public long Apply(long value, long lazy, int segmentLength) => value + lazy * segmentLength;

        /// <inheritdoc />
        public long Compose(long existing, long incoming) => existing + incoming;
    }

    /// <summary>
    /// Policy supporting 64-bit range minimum queries and range additive updates.
    /// </summary>
    public readonly struct RangeMinAddInt64Policy : ILazySegmentTreePolicy<long, long>
    {
        /// <inheritdoc />
        public long Identity => long.MaxValue;

        /// <inheritdoc />
        public long Combine(long left, long right) => Math.Min(left, right);

        /// <inheritdoc />
        public long LazyIdentity => 0L;

        /// <inheritdoc />
        public bool HasLazy(long lazy) => lazy != 0L;

        /// <inheritdoc />
        public long Apply(long value, long lazy, int segmentLength) => value == long.MaxValue ? long.MaxValue : value + lazy;

        /// <inheritdoc />
        public long Compose(long existing, long incoming) => existing + incoming;
    }

    /// <summary>
    /// Policy supporting 32-bit range minimum queries and range point-wise maximum (chmax) updates.
    /// Leaves constructed without an initial value default to <see cref="int.MaxValue"/>; construct with
    /// initial leaf values (for example <c>0</c>) when tracking finite lower-bounded arrays.
    /// </summary>
    public readonly struct RangeMinChmaxInt32Policy : ILazySegmentTreePolicy<int, int>
    {
        /// <inheritdoc />
        public int Identity => int.MaxValue;

        /// <inheritdoc />
        public int Combine(int left, int right) => Math.Min(left, right);

        /// <inheritdoc />
        public int LazyIdentity => int.MinValue;

        /// <inheritdoc />
        public bool HasLazy(int lazy) => lazy != int.MinValue;

        /// <inheritdoc />
        public int Apply(int value, int lazy, int segmentLength) => Math.Max(value, lazy);

        /// <inheritdoc />
        public int Compose(int existing, int incoming) => Math.Max(existing, incoming);
    }

    /// <summary>
    /// Iterative segment tree supporting O(log n) point updates and associative monoid range queries.
    /// </summary>
    /// <typeparam name="TValue">Element and aggregate value type.</typeparam>
    /// <typeparam name="TPolicy">Monoid policy value type implementing <see cref="ISegmentTreeMonoid{TValue}"/>.</typeparam>
    public class SegmentTree<TValue, TPolicy>
        where TPolicy : struct, ISegmentTreeMonoid<TValue>
    {
        private static readonly TPolicy Policy = default;
        private readonly TValue[] tree;
        private readonly int size;

        /// <summary>
        /// Gets the number of elements in the tree.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Initializes a <see cref="SegmentTree{TValue, TPolicy}"/> of <paramref name="count"/> elements set to <see cref="ISegmentTreeMonoid{TValue}.Identity"/>.
        /// </summary>
        /// <param name="count">Number of elements (must be non-negative).</param>
        public SegmentTree(int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            Count = count;
            size = ComputePowerOfTwo(count);
            tree = new TValue[2 * size];
            Array.Fill(tree, Policy.Identity);
        }

        /// <summary>
        /// Initializes a <see cref="SegmentTree{TValue, TPolicy}"/> from <paramref name="initialValues"/> in O(n) time.
        /// </summary>
        /// <param name="initialValues">Initial element values.</param>
        public SegmentTree(ReadOnlySpan<TValue> initialValues)
        {
            Count = initialValues.Length;
            size = ComputePowerOfTwo(Count);
            tree = new TValue[2 * size];
            Array.Fill(tree, Policy.Identity);
            initialValues.CopyTo(tree.AsSpan(size));
            for (int i = size - 1; i >= 1; i--)
                tree[i] = Policy.Combine(tree[2 * i], tree[2 * i + 1]);
        }

        private static int ComputePowerOfTwo(int count)
        {
            int s = 1;
            while (s < count)
                s <<= 1;
            return s;
        }

        /// <summary>
        /// Gets the value at <paramref name="index"/> in O(1) time.
        /// </summary>
        /// <param name="index">0-based element index.</param>
        /// <returns>The element value at <paramref name="index"/>.</returns>
        public TValue Get(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return tree[size + index];
        }

        /// <summary>
        /// Replaces the value at <paramref name="index"/> with <paramref name="value"/> in O(log n) time.
        /// </summary>
        /// <param name="index">0-based element index.</param>
        /// <param name="value">New value.</param>
        public void Set(int index, TValue value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            int pos = size + index;
            tree[pos] = value;
            for (pos >>= 1; pos >= 1; pos >>= 1)
                tree[pos] = Policy.Combine(tree[2 * pos], tree[2 * pos + 1]);
        }

        /// <summary>
        /// Queries the combined aggregate value over the half-open range [<paramref name="start"/>, <paramref name="endExclusive"/>) in O(log n) time.
        /// </summary>
        /// <param name="start">Inclusive 0-based start index.</param>
        /// <param name="endExclusive">Exclusive 0-based end index.</param>
        /// <returns>The combined aggregate value, or <see cref="ISegmentTreeMonoid{TValue}.Identity"/> when the range is empty.</returns>
        public TValue Query(int start, int endExclusive)
        {
            ValidateRange(start, endExclusive, Count);
            if (start == endExclusive)
                return Policy.Identity;

            TValue leftAcc = Policy.Identity;
            TValue rightAcc = Policy.Identity;
            int l = start + size;
            int r = endExclusive + size;
            while (l < r)
            {
                if ((l & 1) != 0)
                    leftAcc = Policy.Combine(leftAcc, tree[l++]);
                if ((r & 1) != 0)
                    rightAcc = Policy.Combine(tree[--r], rightAcc);
                l >>= 1;
                r >>= 1;
            }
            return Policy.Combine(leftAcc, rightAcc);
        }

        internal static void ValidateRange(int start, int endExclusive, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(endExclusive, count);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, endExclusive);
        }
    }

    /// <summary>
    /// Lazy-propagation segment tree supporting O(log n) range updates and range queries
    /// for any monoid and composable lazy action defined by <typeparamref name="TPolicy"/>.
    /// </summary>
    /// <typeparam name="TValue">Element and aggregate value type.</typeparam>
    /// <typeparam name="TLazy">Lazy tag type representing deferred range updates.</typeparam>
    /// <typeparam name="TPolicy">Policy value type implementing <see cref="ILazySegmentTreePolicy{TValue, TLazy}"/>.</typeparam>
    public class LazySegmentTree<TValue, TLazy, TPolicy>
        where TPolicy : struct, ILazySegmentTreePolicy<TValue, TLazy>
    {
        private static readonly TPolicy Policy = default;
        private readonly TValue[] tree;
        private readonly TLazy[] lazy;
        private readonly int size;

        /// <summary>
        /// Gets the number of elements in the tree.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Initializes a <see cref="LazySegmentTree{TValue, TLazy, TPolicy}"/> of <paramref name="count"/> leaves initialized to <see cref="ISegmentTreeMonoid{TValue}.Identity"/>.
        /// </summary>
        /// <param name="count">Number of elements (must be non-negative).</param>
        public LazySegmentTree(int count) : this(count, Policy.Identity)
        {
        }

        /// <summary>
        /// Initializes a <see cref="LazySegmentTree{TValue, TLazy, TPolicy}"/> with <paramref name="count"/> active leaves set to <paramref name="initialLeafValue"/>.
        /// </summary>
        /// <param name="count">Number of elements (must be non-negative).</param>
        /// <param name="initialLeafValue">Initial value for every leaf in [0, count).</param>
        public LazySegmentTree(int count, TValue initialLeafValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            Count = count;
            size = ComputePowerOfTwo(count);
            tree = new TValue[2 * size];
            lazy = new TLazy[2 * size];
            Array.Fill(lazy, Policy.LazyIdentity);
            Array.Fill(tree, Policy.Identity);
            if (count > 0)
            {
                Array.Fill(tree, initialLeafValue, size, count);
                for (int i = size - 1; i >= 1; i--)
                    tree[i] = Policy.Combine(tree[2 * i], tree[2 * i + 1]);
            }
        }

        /// <summary>
        /// Initializes a <see cref="LazySegmentTree{TValue, TLazy, TPolicy}"/> from <paramref name="initialValues"/> in O(n) time.
        /// </summary>
        /// <param name="initialValues">Initial element values.</param>
        public LazySegmentTree(ReadOnlySpan<TValue> initialValues)
        {
            Count = initialValues.Length;
            size = ComputePowerOfTwo(Count);
            tree = new TValue[2 * size];
            lazy = new TLazy[2 * size];
            Array.Fill(lazy, Policy.LazyIdentity);
            Array.Fill(tree, Policy.Identity);
            if (Count > 0)
            {
                initialValues.CopyTo(tree.AsSpan(size));
                for (int i = size - 1; i >= 1; i--)
                    tree[i] = Policy.Combine(tree[2 * i], tree[2 * i + 1]);
            }
        }

        private static int ComputePowerOfTwo(int count)
        {
            int s = 1;
            while (s < count)
                s <<= 1;
            return s;
        }

        /// <summary>
        /// Applies the update <paramref name="delta"/> to every element in the half-open range [<paramref name="start"/>, <paramref name="endExclusive"/>) in O(log n) time.
        /// </summary>
        /// <param name="start">Inclusive 0-based start index.</param>
        /// <param name="endExclusive">Exclusive 0-based end index.</param>
        /// <param name="delta">Lazy update tag to apply.</param>
        public void Update(int start, int endExclusive, TLazy delta)
        {
            SegmentTree<TValue, TPolicy>.ValidateRange(start, endExclusive, Count);
            if (start == endExclusive || !Policy.HasLazy(delta))
                return;
            UpdateInternal(1, 0, size, start, endExclusive, delta);
        }

        /// <summary>
        /// Queries the combined aggregate value over the half-open range [<paramref name="start"/>, <paramref name="endExclusive"/>) in O(log n) time.
        /// </summary>
        /// <param name="start">Inclusive 0-based start index.</param>
        /// <param name="endExclusive">Exclusive 0-based end index.</param>
        /// <returns>The combined aggregate value, or <see cref="ISegmentTreeMonoid{TValue}.Identity"/> when the range is empty.</returns>
        public TValue Query(int start, int endExclusive)
        {
            SegmentTree<TValue, TPolicy>.ValidateRange(start, endExclusive, Count);
            if (start == endExclusive)
                return Policy.Identity;
            return QueryInternal(1, 0, size, start, endExclusive);
        }

        /// <summary>
        /// Gets the current value of the single element at <paramref name="index"/> in O(log n) time.
        /// </summary>
        /// <param name="index">0-based element index.</param>
        /// <returns>The current element value.</returns>
        public TValue Get(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return QueryInternal(1, 0, size, index, index + 1);
        }

        private void ApplyNode(int node, TLazy delta, int segLen)
        {
            tree[node] = Policy.Apply(tree[node], delta, segLen);
            if (node < size)
                lazy[node] = Policy.Compose(lazy[node], delta);
        }

        private void PushDown(int node, int segLen)
        {
            TLazy pending = lazy[node];
            if (!Policy.HasLazy(pending))
                return;
            int childLen = segLen >> 1;
            ApplyNode(2 * node, pending, childLen);
            ApplyNode(2 * node + 1, pending, childLen);
            lazy[node] = Policy.LazyIdentity;
        }

        private void UpdateInternal(int node, int nl, int nr, int ql, int qr, TLazy delta)
        {
            if (ql <= nl && nr <= qr)
            {
                ApplyNode(node, delta, nr - nl);
                return;
            }
            PushDown(node, nr - nl);
            int mid = nl + ((nr - nl) >> 1);
            if (ql < mid)
                UpdateInternal(2 * node, nl, mid, ql, qr, delta);
            if (qr > mid)
                UpdateInternal(2 * node + 1, mid, nr, ql, qr, delta);
            tree[node] = Policy.Combine(tree[2 * node], tree[2 * node + 1]);
        }

        private TValue QueryInternal(int node, int nl, int nr, int ql, int qr)
        {
            if (ql <= nl && nr <= qr)
                return tree[node];
            PushDown(node, nr - nl);
            int mid = nl + ((nr - nl) >> 1);
            if (qr <= mid)
                return QueryInternal(2 * node, nl, mid, ql, qr);
            if (ql >= mid)
                return QueryInternal(2 * node + 1, mid, nr, ql, qr);
            return Policy.Combine(
                QueryInternal(2 * node, nl, mid, ql, qr),
                QueryInternal(2 * node + 1, mid, nr, ql, qr));
        }
    }
}
