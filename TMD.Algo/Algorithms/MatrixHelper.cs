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
using System.Numerics;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Provides methods for performing matrix operations.
    /// </summary>
    public static class MatrixHelper
    {
        /// <summary>
        /// Given a square matrix, calcualtes its nth power efficiently.
        /// </summary>
        /// <param name="input">
        /// Input matrix.
        /// </param>
        /// <param name="power">
        /// Power to raise it to.
        /// </param>
        /// <param name="modulo">
        /// Modulo to apply.
        /// </param>
        /// <returns>
        /// The result of raising the input matrix to the specified power.
        /// </returns>
        public static long[,] Power(long[,] input, long power, Modulo modulo)
        {
            ArgumentNullException.ThrowIfNull(input);
            int iMax = input.GetLength(0);
            int jMax = input.GetLength(1);
            if (iMax != jMax)
                throw new ArgumentException("Input matrix not square.", nameof(input));

            if (power == 0)
            {
                long[,] result = new long[iMax, jMax];
                for (int i = 0; i < iMax; i++)
                {
                    result[i, i] = 1;
                }
                return result;
            }
            return MathFormulas.FastExponent(input, power, (a, b) => Product(a, b, modulo));
        }

        private static long[,] Product(long[,] result, long[,] input, Modulo modulo)
        {
            int iMax = result.GetLength(0);
            long[,] output = new long[iMax, iMax];
            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < iMax; j++)
                {
                    for (int k = 0; k < iMax; k++)
                    {
                        output[i, j] = modulo.Add(output[i, j], modulo.Multiply(result[i, k], input[k, j]));
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Computes the exact determinant of an integer square matrix using fraction-free Bareiss elimination
        /// in O(N^3) arithmetic operations without rounding errors.
        /// </summary>
        /// <param name="matrix">Square integer matrix.</param>
        /// <returns>Exact determinant as a <see cref="BigInteger"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="matrix"/> is not square.</exception>
        public static BigInteger Determinant(long[,] matrix)
        {
            ArgumentNullException.ThrowIfNull(matrix);
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Input matrix must be square.", nameof(matrix));
            if (n == 0)
                return BigInteger.One;
            if (n == 1)
                return new BigInteger(matrix[0, 0]);

            BigInteger[,] a = new BigInteger[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    a[i, j] = new BigInteger(matrix[i, j]);

            BigInteger prevPivot = BigInteger.One;
            int sign = 1;

            for (int k = 0; k < n - 1; k++)
            {
                int pivotRow = -1;
                for (int r = k; r < n; r++)
                {
                    if (!a[r, k].IsZero)
                    {
                        pivotRow = r;
                        break;
                    }
                }
                if (pivotRow == -1)
                    return BigInteger.Zero;

                if (pivotRow != k)
                {
                    for (int c = k; c < n; c++)
                        (a[k, c], a[pivotRow, c]) = (a[pivotRow, c], a[k, c]);
                    sign = -sign;
                }

                BigInteger pivot = a[k, k];
                for (int i = k + 1; i < n; i++)
                {
                    BigInteger aik = a[i, k];
                    for (int j = k + 1; j < n; j++)
                    {
                        a[i, j] = (pivot * a[i, j] - aik * a[k, j]) / prevPivot;
                    }
                }
                prevPivot = pivot;
            }

            BigInteger det = a[n - 1, n - 1];
            return sign < 0 ? -det : det;
        }

        /// <summary>
        /// Computes the determinant of a square integer matrix modulo a prime in O(N^3) time.
        /// </summary>
        /// <param name="matrix">Square integer matrix.</param>
        /// <param name="primeModulus">Prime modulus greater than 1.</param>
        /// <returns>The determinant reduced modulo <paramref name="primeModulus"/> in [0, primeModulus - 1].</returns>
        /// <exception cref="ArgumentException">Thrown when the matrix is not square or the modulus is not prime.</exception>
        public static long DeterminantMod(long[,] matrix, long primeModulus)
        {
            ArgumentNullException.ThrowIfNull(matrix);
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Input matrix must be square.", nameof(matrix));
            if (primeModulus <= 1 || !MathFormulas.IsPrime(primeModulus))
                throw new ArgumentException("Modulus must be a prime greater than 1.", nameof(primeModulus));
            if (n == 0)
                return 1 % primeModulus;

            long[,] a = new long[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    long v = matrix[i, j] % primeModulus;
                    a[i, j] = v < 0 ? v + primeModulus : v;
                }
            }

            long det = 1;
            for (int k = 0; k < n; k++)
            {
                int pivotRow = -1;
                for (int r = k; r < n; r++)
                {
                    if (a[r, k] != 0)
                    {
                        pivotRow = r;
                        break;
                    }
                }
                if (pivotRow == -1)
                    return 0;

                if (pivotRow != k)
                {
                    for (int c = k; c < n; c++)
                        (a[k, c], a[pivotRow, c]) = (a[pivotRow, c], a[k, c]);
                    det = det == 0 ? 0 : primeModulus - det;
                }

                long pivot = a[k, k];
                det = Modulo.Multiply(det, pivot, primeModulus);
                long invPivot = Modulo.Invert(pivot, primeModulus);
                for (int i = k + 1; i < n; i++)
                {
                    if (a[i, k] == 0)
                        continue;
                    long factor = Modulo.Multiply(a[i, k], invPivot, primeModulus);
                    for (int j = k + 1; j < n; j++)
                    {
                        long sub = Modulo.Multiply(factor, a[k, j], primeModulus);
                        long val = a[i, j] - sub;
                        a[i, j] = val < 0 ? val + primeModulus : val;
                    }
                }
            }

            return det;
        }

        /// <summary>
        /// Counts the exact number of spanning trees in an undirected simple graph given its adjacency matrix
        /// using Kirchhoff's Matrix-Tree Theorem.
        /// </summary>
        /// <param name="adjacency">Symmetric <c>N x N</c> adjacency matrix where <c>adjacency[i, j]</c> is true if edge (i, j) exists.</param>
        /// <returns>Exact number of distinct spanning trees.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="adjacency"/> is empty, non-square, or non-symmetric.</exception>
        public static BigInteger CountSpanningTrees(bool[,] adjacency)
        {
            ArgumentNullException.ThrowIfNull(adjacency);
            int n = adjacency.GetLength(0);
            if (n != adjacency.GetLength(1) || n == 0)
                throw new ArgumentException("Adjacency matrix must be non-empty and square.", nameof(adjacency));

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (adjacency[i, j] != adjacency[j, i])
                        throw new ArgumentException("Adjacency matrix must be symmetric.", nameof(adjacency));
                }
            }

            if (n == 1)
                return BigInteger.One;

            long[,] laplacianMinor = new long[n - 1, n - 1];
            for (int i = 1; i < n; i++)
            {
                int deg = 0;
                for (int j = 0; j < n; j++)
                {
                    if (i != j && adjacency[i, j])
                    {
                        deg++;
                        if (j > 0)
                            laplacianMinor[i - 1, j - 1] = -1;
                    }
                }
                laplacianMinor[i - 1, i - 1] = deg;
            }

            return Determinant(laplacianMinor);
        }

        // Fibonacii using the 1 1 0 1 square matrix powers.
    }
}