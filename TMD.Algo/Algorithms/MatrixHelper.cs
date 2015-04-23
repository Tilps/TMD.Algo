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
            int iMax = input.GetLength(0);
            int jMax = input.GetLength(1);
            if (iMax != jMax)
                throw new ArgumentException("Input matrix not square.", "input");
            long[,] result = new long[iMax, jMax];
            for (int i = 0; i < iMax; i++)
            {
                result[i, i] = 1;
            }
            long bit = 1;
            long[,] current = input;
            while (bit < power)
            {
                if ((power & bit) != 0)
                {
                    result = Product(result, current, modulo);
                }
                current = Product(current, current, modulo);
                bit <<= 1;
            }
            return result;
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

        // Fibonacii using the 1 1 0 1 square matrix powers.
    }
}
