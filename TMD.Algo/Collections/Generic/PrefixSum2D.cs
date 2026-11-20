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
    /// 2D prefix sum table supporting O(R * C) construction and O(1) rectangular submatrix sum queries.
    /// </summary>
    /// <typeparam name="T">Numeric element type.</typeparam>
    public class PrefixSum2D<T> where T : INumberBase<T>
    {
        private readonly T[] prefix;

        /// <summary>
        /// Gets the number of rows in the underlying grid.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Gets the number of columns in the underlying grid.
        /// </summary>
        public int Cols { get; }

        /// <summary>
        /// Initializes a <see cref="PrefixSum2D{T}"/> from a 2D array.
        /// </summary>
        /// <param name="grid">Source 2D grid of values.</param>
        public PrefixSum2D(T[,] grid)
        {
            ArgumentNullException.ThrowIfNull(grid);
            Rows = grid.GetLength(0);
            Cols = grid.GetLength(1);
            prefix = BuildTable(Rows, Cols, (r, c) => grid[r, c]);
        }

        /// <summary>
        /// Initializes a <see cref="PrefixSum2D{T}"/> from dimensions and a cell value selector.
        /// </summary>
        /// <param name="rows">Number of rows (must be non-negative).</param>
        /// <param name="cols">Number of columns (must be non-negative).</param>
        /// <param name="valueSelector">Function returning the value at (row, col).</param>
        public PrefixSum2D(int rows, int cols, Func<int, int, T> valueSelector)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(rows);
            ArgumentOutOfRangeException.ThrowIfNegative(cols);
            ArgumentNullException.ThrowIfNull(valueSelector);
            Rows = rows;
            Cols = cols;
            prefix = BuildTable(rows, cols, valueSelector);
        }

        private static T[] BuildTable(int rows, int cols, Func<int, int, T> valueSelector)
        {
            int stride = cols + 1;
            T[] table = new T[(rows + 1) * stride];
            Array.Fill(table, T.Zero);

            for (int r = 0; r < rows; r++)
            {
                int rowOffset = r * stride;
                int nextRowOffset = (r + 1) * stride;
                for (int c = 0; c < cols; c++)
                {
                    table[nextRowOffset + c + 1] =
                        valueSelector(r, c)
                        + table[rowOffset + c + 1]
                        + table[nextRowOffset + c]
                        - table[rowOffset + c];
                }
            }

            return table;
        }

        /// <summary>
        /// Returns the sum over the prefix rectangle [0, rowExclusive) x [0, colExclusive).
        /// </summary>
        /// <param name="rowExclusive">Exclusive ending row index in [0, Rows].</param>
        /// <param name="colExclusive">Exclusive ending column index in [0, Cols].</param>
        /// <returns>The sum of all elements in [0, rowExclusive) x [0, colExclusive).</returns>
        public T Prefix(int rowExclusive, int colExclusive)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(rowExclusive);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(rowExclusive, Rows);
            ArgumentOutOfRangeException.ThrowIfNegative(colExclusive);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(colExclusive, Cols);
            return prefix[rowExclusive * (Cols + 1) + colExclusive];
        }

        /// <summary>
        /// Returns the sum over the inclusive rectangle [r1, r2] x [c1, c2] in O(1) time.
        /// Returns zero if r1 &gt; r2 or c1 &gt; c2.
        /// </summary>
        /// <param name="r1">Inclusive starting row index.</param>
        /// <param name="c1">Inclusive starting column index.</param>
        /// <param name="r2">Inclusive ending row index.</param>
        /// <param name="c2">Inclusive ending column index.</param>
        /// <returns>The sum over [r1, r2] x [c1, c2], or zero if the range is empty.</returns>
        public T Query(int r1, int c1, int r2, int c2)
        {
            if (r1 > r2 || c1 > c2)
                return T.Zero;
            ArgumentOutOfRangeException.ThrowIfNegative(r1);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(r2, Rows);
            ArgumentOutOfRangeException.ThrowIfNegative(c1);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(c2, Cols);

            int stride = Cols + 1;
            int off1 = r1 * stride;
            int off2 = (r2 + 1) * stride;
            return prefix[off2 + c2 + 1]
                 - prefix[off1 + c2 + 1]
                 - prefix[off2 + c1]
                 + prefix[off1 + c1];
        }
    }
}
