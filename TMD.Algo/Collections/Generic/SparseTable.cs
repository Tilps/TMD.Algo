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
using System.Collections.Generic;
using System.Numerics;
using TMD.Algo.Algorithms;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Defines an associative, idempotent binary operation for <see cref="SparseTable{TValue, TPolicy}"/>.
    /// Implement as a value type (struct) for zero-overhead specialization.
    /// </summary>
    /// <typeparam name="TValue">Element and aggregate value type.</typeparam>
    public interface ISparseTableOperation<TValue>
    {
        /// <summary>
        /// Combines the aggregate values of two overlapping or adjacent subranges idempotently.
        /// </summary>
        /// <param name="left">Aggregate value from the left subrange.</param>
        /// <param name="right">Aggregate value from the right subrange.</param>
        /// <returns>The combined aggregate value.</returns>
        TValue Combine(TValue left, TValue right);
    }

    /// <summary>
    /// Policy supporting O(1) 32-bit integer range minimum queries on <see cref="SparseTable{TValue, TPolicy}"/>.
    /// </summary>
    public readonly struct RangeMinInt32Policy : ISparseTableOperation<int>
    {
        /// <inheritdoc />
        public int Combine(int left, int right) => Math.Min(left, right);
    }

    /// <summary>
    /// Policy supporting O(1) 32-bit integer range maximum queries on <see cref="SparseTable{TValue, TPolicy}"/>.
    /// </summary>
    public readonly struct RangeMaxInt32Policy : ISparseTableOperation<int>
    {
        /// <inheritdoc />
        public int Combine(int left, int right) => Math.Max(left, right);
    }

    /// <summary>
    /// Policy supporting O(1) 64-bit integer range minimum queries on <see cref="SparseTable{TValue, TPolicy}"/>.
    /// </summary>
    public readonly struct RangeMinInt64Policy : ISparseTableOperation<long>
    {
        /// <inheritdoc />
        public long Combine(long left, long right) => Math.Min(left, right);
    }

    /// <summary>
    /// Policy supporting O(1) 64-bit integer range maximum queries on <see cref="SparseTable{TValue, TPolicy}"/>.
    /// </summary>
    public readonly struct RangeMaxInt64Policy : ISparseTableOperation<long>
    {
        /// <inheritdoc />
        public long Combine(long left, long right) => Math.Max(left, right);
    }

    /// <summary>
    /// Policy supporting O(1) 64-bit greatest common divisor range queries on <see cref="SparseTable{TValue, TPolicy}"/>.
    /// </summary>
    public readonly struct RangeGcdInt64Policy : ISparseTableOperation<long>
    {
        /// <inheritdoc />
        public long Combine(long left, long right) => MathFormulas.Gcd(left, right);
    }

    /// <summary>
    /// Static sparse table supporting O(1) idempotent range queries after O(n log n) preprocessing.
    /// </summary>
    /// <typeparam name="TValue">Element type.</typeparam>
    /// <typeparam name="TPolicy">Idempotent operation policy value type implementing <see cref="ISparseTableOperation{TValue}"/>.</typeparam>
    public class SparseTable<TValue, TPolicy>
        where TPolicy : struct, ISparseTableOperation<TValue>
    {
        private static readonly TPolicy Policy = default;
        private readonly TValue[][] table;

        /// <summary>
        /// Gets the number of elements in the sparse table.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Initializes a <see cref="SparseTable{TValue, TPolicy}"/> from <paramref name="values"/> in O(n log n) time.
        /// </summary>
        /// <param name="values">Input elements.</param>
        public SparseTable(ReadOnlySpan<TValue> values)
        {
            Count = values.Length;
            if (Count == 0)
            {
                table = [];
                return;
            }

            int levels = BitOperations.Log2((uint)Count) + 1;
            table = new TValue[levels][];
            table[0] = values.ToArray();

            for (int k = 1; k < levels; k++)
            {
                int span = 1 << k;
                int half = span >> 1;
                int len = Count - span + 1;
                TValue[] prev = table[k - 1];
                TValue[] cur = new TValue[len];
                for (int i = 0; i < len; i++)
                    cur[i] = Policy.Combine(prev[i], prev[i + half]);
                table[k] = cur;
            }
        }

        /// <summary>
        /// Initializes a <see cref="SparseTable{TValue, TPolicy}"/> from <paramref name="values"/> in O(n log n) time.
        /// </summary>
        /// <param name="values">Input list of elements.</param>
        public SparseTable(IReadOnlyList<TValue> values) : this((ReadOnlySpan<TValue>)CopyToArray(values))
        {
        }

        private static TValue[] CopyToArray(IReadOnlyList<TValue> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            TValue[] arr = new TValue[values.Count];
            for (int i = 0; i < values.Count; i++)
                arr[i] = values[i];
            return arr;
        }

        /// <summary>
        /// Queries the combined idempotent aggregate value over the non-empty half-open range [<paramref name="start"/>, <paramref name="endExclusive"/>) in O(1) time.
        /// </summary>
        /// <param name="start">Inclusive 0-based start index.</param>
        /// <param name="endExclusive">Exclusive 0-based end index (must be strictly greater than <paramref name="start"/>).</param>
        /// <returns>The combined aggregate value over the range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the range is empty or lies outside [0, <see cref="Count"/>].
        /// </exception>
        public TValue Query(int start, int endExclusive)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(endExclusive, Count);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, endExclusive);

            int length = endExclusive - start;
            int k = BitOperations.Log2((uint)length);
            TValue[] row = table[k];
            return Policy.Combine(row[start], row[endExclusive - (1 << k)]);
        }
    }
}
