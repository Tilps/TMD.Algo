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
using System.Numerics;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Binary Indexed Tree (Fenwick Tree) supporting O(log n) point updates and prefix/range sum queries
    /// over a 0-based sequence of 64-bit integers.
    /// </summary>
    public class FenwickTree
    {
        private readonly long[] tree;

        /// <summary>
        /// Initializes a zeroed <see cref="FenwickTree"/> with the specified length.
        /// </summary>
        /// <param name="length">Number of elements (must be non-negative).</param>
        public FenwickTree(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            Length = length;
            tree = new long[length + 1];
        }

        /// <summary>
        /// Initializes a <see cref="FenwickTree"/> from initial element values in O(n) time.
        /// </summary>
        /// <param name="initialValues">Initial values for indices 0 through length - 1.</param>
        public FenwickTree(ReadOnlySpan<long> initialValues)
        {
            Length = initialValues.Length;
            tree = new long[Length + 1];
            for (int i = 1; i <= Length; i++)
            {
                tree[i] += initialValues[i - 1];
                int parent = i + (i & -i);
                if (parent <= Length)
                    tree[parent] += tree[i];
            }
        }

        /// <summary>
        /// Gets the number of elements in this tree.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Adds <paramref name="delta"/> to the element at 0-based <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Zero-based element index.</param>
        /// <param name="delta">Value to add.</param>
        public void Add(int index, long delta)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length);

            for (int i = index + 1; i <= Length; i += i & -i)
            {
                tree[i] += delta;
            }
        }

        /// <summary>
        /// Sets the element at 0-based <paramref name="index"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="index">Zero-based element index.</param>
        /// <param name="value">New value to assign.</param>
        public void Set(int index, long value)
        {
            Add(index, value - Get(index));
        }

        /// <summary>
        /// Returns the current value of the element at 0-based <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Zero-based element index.</param>
        /// <returns>The value at <paramref name="index"/>.</returns>
        public long Get(int index) => RangeSum(index, index);

        /// <summary>
        /// Computes the inclusive prefix sum over indices [0 .. <paramref name="index"/>].
        /// </summary>
        /// <param name="index">Inclusive 0-based upper bound.</param>
        /// <returns>Sum of elements from 0 to <paramref name="index"/>.</returns>
        public long PrefixSum(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length);

            long sum = 0;
            for (int i = index + 1; i > 0; i -= i & -i)
            {
                sum += tree[i];
            }
            return sum;
        }

        /// <summary>
        /// Computes the inclusive range sum over indices [<paramref name="left"/> .. <paramref name="right"/>].
        /// Returns 0 if <paramref name="left"/> &gt; <paramref name="right"/>.
        /// </summary>
        /// <param name="left">Inclusive 0-based lower bound (must be &gt;= 0).</param>
        /// <param name="right">Inclusive 0-based upper bound (must be &lt; <see cref="Length"/>).</param>
        /// <returns>Sum over the specified range.</returns>
        public long RangeSum(int left, int right)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(left);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(right, Length);
            if (left > right)
                return 0;
            return left == 0 ? PrefixSum(right) : PrefixSum(right) - PrefixSum(left - 1);
        }

        /// <summary>
        /// Returns the total sum of all elements in the tree.
        /// </summary>
        /// <returns>The sum of all elements, or 0 if <see cref="Length"/> is 0.</returns>
        public long TotalSum() => Length == 0 ? 0 : PrefixSum(Length - 1);

        /// <summary>
        /// Finds the smallest 0-based index i such that <see cref="PrefixSum(int)"/>(i) &gt;= <paramref name="targetSum"/>,
        /// assuming all stored element values are non-negative.
        /// </summary>
        /// <param name="targetSum">Target cumulative sum.</param>
        /// <returns>
        /// 0 if <paramref name="targetSum"/> &lt;= 0, the smallest valid index if reachable,
        /// or <see cref="Length"/> if <see cref="TotalSum()"/> &lt; <paramref name="targetSum"/>.
        /// </returns>
        public int LowerBound(long targetSum)
        {
            if (targetSum <= 0)
                return 0;

            int idx = 0;
            long accumulated = 0;
            int bitMask = Length == 0 ? 0 : 1 << BitOperations.Log2((uint)Length);

            while (bitMask > 0)
            {
                int next = idx + bitMask;
                if (next <= Length && (Int128)accumulated + tree[next] < targetSum)
                {
                    idx = next;
                    accumulated += tree[next];
                }
                bitMask >>= 1;
            }

            return idx;
        }

        /// <summary>
        /// Resets all elements in the tree to zero.
        /// </summary>
        public void Clear() => Array.Clear(tree);
    }

    /// <summary>
    /// Two-dimensional Binary Indexed Tree (Fenwick Tree) supporting O(log R * log C) point updates
    /// and 2D prefix/submatrix sum queries over 0-based coordinates.
    /// </summary>
    public class FenwickTree2D
    {
        private readonly long[,] tree;

        /// <summary>
        /// Initializes a zeroed <see cref="FenwickTree2D"/> with the specified grid dimensions.
        /// </summary>
        /// <param name="rows">Number of rows (must be non-negative).</param>
        /// <param name="columns">Number of columns (must be non-negative).</param>
        public FenwickTree2D(int rows, int columns)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(rows);
            ArgumentOutOfRangeException.ThrowIfNegative(columns);
            Rows = rows;
            Columns = columns;
            tree = new long[rows + 1, columns + 1];
        }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Adds <paramref name="delta"/> to the cell at 0-based (<paramref name="row"/>, <paramref name="column"/>).
        /// </summary>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="delta">Value to add.</param>
        public void Add(int row, int column, long delta)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Columns);

            for (int r = row + 1; r <= Rows; r += r & -r)
            {
                for (int c = column + 1; c <= Columns; c += c & -c)
                {
                    tree[r, c] += delta;
                }
            }
        }

        /// <summary>
        /// Computes the inclusive 2D prefix sum over [0 .. <paramref name="row"/>] x [0 .. <paramref name="column"/>].
        /// </summary>
        /// <param name="row">Inclusive 0-based row bound.</param>
        /// <param name="column">Inclusive 0-based column bound.</param>
        /// <returns>The sum over the prefix rectangle.</returns>
        public long PrefixSum(int row, int column)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Columns);

            long sum = 0;
            for (int r = row + 1; r > 0; r -= r & -r)
            {
                for (int c = column + 1; c > 0; c -= c & -c)
                {
                    sum += tree[r, c];
                }
            }
            return sum;
        }

        /// <summary>
        /// Computes the inclusive sum over the submatrix [<paramref name="topRow"/> .. <paramref name="bottomRow"/>]
        /// x [<paramref name="leftColumn"/> .. <paramref name="rightColumn"/>].
        /// Returns 0 if the rectangle is empty.
        /// </summary>
        /// <param name="topRow">Inclusive 0-based top row (&gt;= 0).</param>
        /// <param name="leftColumn">Inclusive 0-based left column (&gt;= 0).</param>
        /// <param name="bottomRow">Inclusive 0-based bottom row (&lt; <see cref="Rows"/>).</param>
        /// <param name="rightColumn">Inclusive 0-based right column (&lt; <see cref="Columns"/>).</param>
        /// <returns>The sum over the specified rectangle.</returns>
        public long RangeSum(int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(topRow);
            ArgumentOutOfRangeException.ThrowIfNegative(leftColumn);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bottomRow, Rows);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rightColumn, Columns);

            if (topRow > bottomRow || leftColumn > rightColumn)
                return 0;

            long res = PrefixSum(bottomRow, rightColumn);
            if (topRow > 0)
                res -= PrefixSum(topRow - 1, rightColumn);
            if (leftColumn > 0)
                res -= PrefixSum(bottomRow, leftColumn - 1);
            if (topRow > 0 && leftColumn > 0)
                res += PrefixSum(topRow - 1, leftColumn - 1);
            return res;
        }

        /// <summary>
        /// Resets all cells in the 2D tree to zero.
        /// </summary>
        public void Clear() => Array.Clear(tree);
    }
}
