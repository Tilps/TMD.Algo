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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Efficient modulo operations.  Specializing the case where the modulo is prime.
    /// While the basis is specified as a long, factorials and inverts are cached in an array which has size equal to the basis.
    /// Therefore, if those methods are invoked, the basis must be of managable size.
    /// </summary>
    public class Modulo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="basis">
        /// Basis to use for the modulo. 
        /// </param>
        public Modulo(long basis)
        {
            if (basis <= 0)
                throw new ArgumentException("Basis must be positive.", "basis");
            this.basis = basis;
            basisIsPrime = IsPrime(basis);
        }

        private static bool IsPrime(long basis)
        {
            if (basis < 2)
                return false;
            if (basis == 2)
                return true;
            if (basis % 2 == 0)
                return false;
            for (int i = 3; i * i <= basis; i+=2)
            {
                if (basis % i == 0)
                    return false;
            }
            return true;
        }

        private long basis;
        private bool basisIsPrime;

        /// <summary>
        /// Adds two numbers togeather modulo the basis.
        /// Assumes that the sum will not overflow.
        /// </summary>
        /// <param name="first">
        /// First number.
        /// </param>
        /// <param name="second">
        /// Second number.
        /// </param>
        /// <returns>
        /// The sum of the two numbers modulo the basis.
        /// </returns>
        public long Add(long first, long second)
        {
            return Mod(first + second);
        }

        /// <summary>
        /// Subtracts two numbers, modulo the basis.  Assumes the subtraction will not overflow.
        /// </summary>
        /// <param name="first">
        /// First argument to the subtraction.
        /// </param>
        /// <param name="second">
        /// Second argument to the subtraction.
        /// </param>
        /// <returns>
        /// The difference between first and second, modulo the basis.
        /// </returns>
        public long Subtract(long first, long second)
        {
            return Mod(first - second);
        }

        /// <summary>
        /// Reduces a given number to the domain of the basis.
        /// </summary>
        /// <param name="number">
        /// The number for which the modulo calculation is required.
        /// </param>
        /// <returns>
        /// A number between 0 and basis-1.
        /// </returns>
        public long Mod(long number)
        {
            return ((number % basis) + basis) % basis;
        }

        /// <summary>
        /// Calculates the factorial of number under the basis.
        /// Running time is amortized O(1), with an O(basis) hidden cost in one call.
        /// </summary>
        /// <param name="number">
        /// Number to calculate the factorial of.
        /// </param>
        /// <returns>
        /// The factorial of the specified number modulo the basis.
        /// </returns>
        public long Factorial(long number)
        {
            if (number >= basis)
                return 0;
            if (factCache == null)
            {
                factCache = new long[basis];
                factCache[0] = 1;
                factCache[1] = 1;
                for (long i = 2; i < basis; i++)
                {
                    factCache[i] = (factCache[i-1] * i) % basis;
                }
            }
            return factCache[number];
        }
        private long[] factCache;

        /// <summary>
        /// Multiplies two numbers togeather under the basis.
        /// Assumes that the multiplication will not overflow.
        /// </summary>
        /// <param name="first">
        /// First number to multiply.
        /// </param>
        /// <param name="second">
        /// Second number to multiply.
        /// </param>
        /// <returns></returns>
        public long Multiply(long first, long second)
        {
            return Mod(first * second);
        }

        /// <summary>
        /// Calculates first divided by second under the modulo.
        /// Technically it finds a number which when multiplied by second is equal to first under the modulo.
        /// As there may be more than one answer, this function chooses one at random.
        /// Runs in O(log n + r) where n is the basis and r is the number of solutions.
        /// If Mod(first)=1, Invert is called, and so subsequent calls for the same second value modulo the basis, are O(1).
        /// </summary>
        /// <param name="first">
        /// First argument to the division.
        /// </param>
        /// <param name="second">
        /// Second argument to the division.
        /// </param>
        /// <returns></returns>
        public long Divide(long first, long second)
        {
            // Use the inversion cache if asking for 1.
            if (Mod(first) == 1)
                return Invert(second);
            List<long> results = SolveLinearEquation(second, first);
            if (results.Count == 0)
                throw new ArgumentException("There exists no number which multiplied by the second is equal to the first modulo the basis.");
            return results[0];
        }

        /// <summary>
        /// Calculates the inverse of the specified number under the modulo.
        /// Uses a cache, so if after reduction, the inverse has been calculated before it is O(1).
        /// Otherwise it is O(log n) where n is the basis.
        /// </summary>
        /// <param name="number">
        /// Number to invert.
        /// </param>
        /// <returns>
        /// The inverted number if there is one.
        /// </returns>
        public long Invert(long number)
        {
            number = Mod(number);
            if (number == 0)
                throw new ArgumentException("Zero has no inverse.", "number");
            if (invertCache == null)
            {
                invertCache = new long[basis];
            }
            if (invertCache[number] == 0)
            {
                long firstProduct;
                long secondProudct;
                long divisor = ExtendedGcd(number, basis, out firstProduct, out secondProudct);
                if (divisor == 1)
                    invertCache[number] = Mod(firstProduct);
                else
                    invertCache[number] = -1;
            }
            if (invertCache[number] == -1)
                throw new ArgumentException("Specified number is not coprime with the basis, it has no inverse.", "number");
            return invertCache[number];
        }
        private long[] invertCache;

        /// <summary>
        /// Solves linear modulo equation.
        /// </summary>
        /// <param name="multiplier">
        /// Multiplier of the answer.
        /// </param>
        /// <param name="result">
        /// Expected result after applying modulo.
        /// </param>
        /// <returns>
        /// List of solutions, if there are any.
        /// </returns>
        public List<long> SolveLinearEquation(long multiplier, long result)
        {
            List<long> results = new List<long>();
            long firstProduct;
            long secondProudct;
            long d = ExtendedGcd(Mod(multiplier), basis, out firstProduct, out secondProudct);
            result = Mod(result);
            if (result % d == 0)
            {
                long firstResult = Mod(firstProduct * (result / d));
                for (int i = 0; i < d; i++)
                {
                    results.Add(Mod(firstResult + i * (basis / d)));
                }
            }
            return results;
        }

        private long ExtendedGcd(long first, long second, out long firstProduct, out long secondProduct)
        {
            if (second == 0)
            {
                firstProduct = 1;
                secondProduct = 0;
                return first;
            }
            long firstTemp;
            long secondTemp;
            long divisor = ExtendedGcd(second, first % second, out firstTemp, out secondTemp);
            firstProduct = secondTemp;
            secondProduct = firstTemp - (first / second) * secondTemp;
            return divisor;
        }

        /// <summary>
        /// Calculates the number of permutations selecting k items from n total modulo the basis.
        /// This is efficient if the basis is prime.
        /// </summary>
        /// <param name="n">
        /// Number of items to choose from.
        /// </param>
        /// <param name="k">
        /// Number of items to choose.
        /// </param>
        /// <returns>
        /// Number of permutations, modulo the basis.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704")]
        public long Permutations(long n, long k)
        {
            if (k >= basis)
                return 0;
            if (n < k)
                return 0;
            if (k == 0)
                return 1;
            long first = Mod(n - k + 1);
            long last = Mod(n);
            if (first > last)
                return 0;
            if (first == 0)
                return 0;
            if (first == last)
                return first;
            if (basisIsPrime)
            {
                return Mod(Factorial(last) * Invert(Factorial(first - 1)));
            }
            else
            {
                // TODO: two dimensional cache?
                long result = 1;
                for (long i = first; i <= last; i++)
                {
                    result = Mod(result * i);
                }
                return result;
            }
        }

        /// <summary>
        /// Calculates the number of ways to choose k items from n modulo the basis.
        /// This method currently only works if the basis is prime, as otherwise the problem is tricky.
        /// Running time is O(log n * log basis).  Although a cache means it is effectively O(log n).
        /// </summary>
        /// <param name="n">
        /// Number of items to choose from.
        /// </param>
        /// <param name="k">
        /// Number of items to choose.
        /// </param>
        /// <returns>
        /// The number of ways to choose k from n, modulo the basis.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704")]
        public long Choose(long n, long k)
        {
            if (k < 0 || n < k)
                return 0;
            if (k == 0)
                return 1;
            if (basisIsPrime)
            {
                // Lucas's law.
                return Mod(Choose(n/basis, k/basis)*ChooseInternal(Mod(n), Mod(k)));
            }
            else
            {
                throw new NotImplementedException("Too hard basket.");
            }
        }

        private long ChooseInternal(long n, long k)
        {
            return Mod(Permutations(n, k) * Invert(Factorial(k)));
        }
    }
}
