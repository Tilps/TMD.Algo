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
using System.Collections.Generic;
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
                throw new ArgumentException("Basis must be positive.", nameof(basis));
            this.basis = basis;
            basisIsPrime = MathFormulas.IsPrime(basis);
        }

        private readonly long basis;
        private readonly bool basisIsPrime;

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
        /// Calculates the minimum circular distance between two numbers under the modulo.
        /// </summary>
        /// <param name="first">
        /// First number.
        /// </param>
        /// <param name="second">
        /// Second number.
        /// </param>
        /// <returns>
        /// The circular distance between first and second, in the range [0, basis / 2].
        /// </returns>
        public long Dist(long first, long second)
        {
            long diff = Math.Abs(Mod(first) - Mod(second));
            return Math.Min(diff, basis - diff);
        }

        /// <summary>
        /// Calculates the minimum circular distance between two numbers under the modulo.
        /// Equivalent to <see cref="Dist(long, long)"/>.
        /// </summary>
        /// <param name="first">
        /// First number.
        /// </param>
        /// <param name="second">
        /// Second number.
        /// </param>
        /// <returns>
        /// The circular distance between first and second, in the range [0, basis / 2].
        /// </returns>
        public long Distance(long first, long second)
        {
            return Dist(first, second);
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
            return ((number%basis) + basis)%basis;
        }

        /// <summary>
        /// Calculates the factorial of number under the basis.
        /// </summary>
        /// <param name="number">
        /// Number to calculate the factorial of.
        /// </param>
        /// <returns>
        /// The factorial of the specified number modulo the basis.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> &lt; 0.</exception>
        public long Factorial(long number)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(number);
            if (number >= basis)
                return 0;
            if (factCache != null && number < factCache.Length)
                return factCache[number];
            long prevLen = factCache?.Length ?? 0;
            long desired = Math.Max(number + 1, Math.Max(prevLen * 2, basis <= 100_000 ? basis : number + 1));
            int targetLen = (int)Math.Min(basis, desired);
            EnsureFactorialCache(targetLen);
            return factCache![number];
        }

        /// <summary>
        /// Precomputes factorials up to <paramref name="maxN"/> (and inverse factorials up to
        /// <c>min(maxN, basis - 1)</c> when the basis is prime) in O(maxN + log basis) time.
        /// Enables lock-free O(1) <see cref="Choose"/>, <see cref="Permutations"/>, and <see cref="InverseFactorial"/> lookups.
        /// </summary>
        /// <param name="maxN">Maximum factorial index to precompute.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxN"/> &lt; 0.</exception>
        public void PrecomputeFactorials(int maxN)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxN);
            EnsureFactorialCache(maxN + 1);
            if (basisIsPrime)
            {
                int invMax = (int)Math.Min(maxN, basis - 1);
                if (invFactCache == null || invFactCache.Length <= invMax)
                {
                    long[] newInv = new long[invMax + 1];
                    newInv[invMax] = Invert(factCache![invMax], basis);
                    for (int i = invMax; i >= 1; i--)
                    {
                        newInv[i - 1] = Multiply(newInv[i], i);
                    }
                    invFactCache = newInv;
                }
            }
        }

        /// <summary>
        /// Returns the modular inverse of <paramref name="number"/>! modulo the basis (which must be prime).
        /// Uses O(1) lookup when within the range prepared by <see cref="PrecomputeFactorials"/>.
        /// </summary>
        /// <param name="number">Non-negative factorial argument strictly less than basis.</param>
        /// <returns>Modular multiplicative inverse of <paramref name="number"/>!.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="number"/> &lt; 0.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the basis is not prime.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="number"/> &gt;= basis.</exception>
        public long InverseFactorial(int number)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(number);
            if (!basisIsPrime)
                throw new InvalidOperationException("Modular inverse of factorial requires a prime basis.");
            if (number >= basis)
                throw new ArgumentException("Specified number is not coprime with the basis, its factorial has no inverse.", nameof(number));
            if (invFactCache != null && number < invFactCache.Length)
                return invFactCache[number];
            return Invert(Factorial(number));
        }

        private void EnsureFactorialCache(int targetLength)
        {
            int prevLen = factCache?.Length ?? 0;
            if (prevLen >= targetLength)
                return;
            if (factCache == null)
            {
                factCache = new long[targetLength];
                factCache[0] = 1 % basis;
                prevLen = 1;
            }
            else
            {
                Array.Resize(ref factCache, targetLength);
            }
            for (int i = prevLen; i < targetLength; i++)
            {
                factCache[i] = Multiply(factCache[i - 1], i);
            }
        }

        private long[]? factCache;
        private long[]? invFactCache;

        /// <summary>
        /// Computes <paramref name="value"/> raised to <paramref name="exponent"/> modulo <paramref name="basis"/>
        /// in O(log exponent) time without overflow.
        /// </summary>
        /// <param name="value">Base integer.</param>
        /// <param name="exponent">Non-negative exponent.</param>
        /// <param name="basis">Positive modulus.</param>
        /// <returns><paramref name="value"/>^<paramref name="exponent"/> mod <paramref name="basis"/> in [0, basis - 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="basis"/> &lt;= 0 or <paramref name="exponent"/> &lt; 0.</exception>
        public static long Pow(long value, long exponent, long basis)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(basis);
            ArgumentOutOfRangeException.ThrowIfNegative(exponent);
            if (basis == 1)
                return 0;
            long result = 1;
            long baseVal = ((value % basis) + basis) % basis;
            while (exponent > 0)
            {
                if ((exponent & 1) != 0)
                    result = (long)((UInt128)(ulong)result * (UInt128)(ulong)baseVal % (UInt128)(ulong)basis);
                baseVal = (long)((UInt128)(ulong)baseVal * (UInt128)(ulong)baseVal % (UInt128)(ulong)basis);
                exponent >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Computes <paramref name="value"/> raised to <paramref name="exponent"/> modulo this basis
        /// in O(log exponent) time without overflow.
        /// </summary>
        /// <param name="value">Base integer.</param>
        /// <param name="exponent">Non-negative exponent.</param>
        /// <returns><paramref name="value"/>^<paramref name="exponent"/> mod basis in [0, basis - 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="exponent"/> &lt; 0.</exception>
        public long Pow(long value, long exponent)
        {
            return Pow(value, exponent, basis);
        }

        /// <summary>
        /// Multiplies two numbers together under the basis without overflow.
        /// </summary>
        /// <param name="first">
        /// First number to multiply.
        /// </param>
        /// <param name="second">
        /// Second number to multiply.
        /// </param>
        /// <returns>The product modulo the basis.</returns>
        public long Multiply(long first, long second)
        {
            return Multiply(first, second, basis);
        }

        /// <summary>
        /// Multiplies two numbers together modulo <paramref name="basis"/> without overflow.
        /// </summary>
        /// <param name="first">First number to multiply.</param>
        /// <param name="second">Second number to multiply.</param>
        /// <param name="basis">Positive modulus.</param>
        /// <returns>The product modulo <paramref name="basis"/> in [0, basis - 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="basis"/> &lt;= 0.</exception>
        public static long Multiply(long first, long second, long basis)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(basis);
            ulong a = (ulong)first < (ulong)basis ? (ulong)first : (ulong)(((first % basis) + basis) % basis);
            ulong b = (ulong)second < (ulong)basis ? (ulong)second : (ulong)(((second % basis) + basis) % basis);
            return (long)((UInt128)a * b % (ulong)basis);
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
                throw new ArgumentException(
                    "There exists no number which multiplied by the second is equal to the first modulo the basis.");
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
                throw new ArgumentException("Zero has no inverse.", nameof(number));
            if (basis <= 100_000)
            {
                if (invertCache == null)
                {
                    invertCache = new long[basis];
                }
                if (invertCache[number] == 0)
                {
                    long divisor = MathFormulas.ExtendedGcd(number, basis, out long firstProduct, out _);
                    if (divisor == 1)
                        invertCache[number] = Mod(firstProduct);
                    else
                        invertCache[number] = -1;
                }
                if (invertCache[number] == -1)
                    throw new ArgumentException("Specified number is not coprime with the basis, it has no inverse.",
                        nameof(number));
                return invertCache[number];
            }
            else
            {
                if (invertDict == null)
                {
                    invertDict = new Dictionary<long, long>();
                }
                if (!invertDict.TryGetValue(number, out long inv))
                {
                    long divisor = MathFormulas.ExtendedGcd(number, basis, out long firstProduct, out _);
                    if (divisor == 1)
                    {
                        inv = Mod(firstProduct);
                        invertDict[number] = inv;
                    }
                    else
                    {
                        invertDict[number] = -1;
                        inv = -1;
                    }
                }
                if (inv == -1)
                    throw new ArgumentException("Specified number is not coprime with the basis, it has no inverse.",
                        nameof(number));
                return inv;
            }
        }

        /// <summary>
        /// Calculates the modular multiplicative inverse of <paramref name="number"/> modulo <paramref name="basis"/>
        /// in the range [0, <paramref name="basis"/> - 1] without allocating a cache or constructing a <see cref="Modulo"/> instance.
        /// Runs in O(log basis) time.
        /// </summary>
        /// <param name="number">Number to invert.</param>
        /// <param name="basis">Positive modulus.</param>
        /// <returns>An integer inv in [0, basis - 1] such that (number * inv) % basis == 1 % basis.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="basis"/> &lt;= 0.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="number"/> is not coprime with <paramref name="basis"/>.</exception>
        public static long Invert(long number, long basis)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(basis);
            long v = ((number % basis) + basis) % basis;
            long gcd = MathFormulas.ExtendedGcd(v, basis, out long x, out _);
            if (gcd != 1)
                throw new ArgumentException("Specified number is not coprime with the basis, it has no inverse.", nameof(number));
            return ((x % basis) + basis) % basis;
        }

        private long[]? invertCache;
        private Dictionary<long, long>? invertDict;

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
            long d = MathFormulas.ExtendedGcd(Mod(multiplier), basis, out long firstProduct, out _);
            result = Mod(result);
            if (result%d == 0)
            {
                long firstResult = Mod(firstProduct*(result/d));
                for (int i = 0; i < d; i++)
                {
                    results.Add(Mod(firstResult + i*(basis/d)));
                }
            }
            return results;
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
                if (invFactCache != null && factCache != null && last < invFactCache.Length && last < factCache.Length)
                    return Multiply(factCache[last], invFactCache[first - 1]);
                return Multiply(Factorial(last), Invert(Factorial(first - 1)));
            }
            else
            {
                // TODO: two dimensional cache?
                long result = 1;
                for (long i = first; i <= last; i++)
                {
                    result = Multiply(result, i);
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
                return Multiply(Choose(n/basis, k/basis), ChooseInternal(Mod(n), Mod(k)));
            }
            else
            {
                throw new NotImplementedException("Too hard basket.");
            }
        }

        private long ChooseInternal(long n, long k)
        {
            if (k > n)
                return 0;
            if (invFactCache != null && factCache != null && n < invFactCache.Length && n < factCache.Length)
                return Multiply(Multiply(factCache[n], invFactCache[k]), invFactCache[n - k]);
            return Multiply(Permutations(n, k), Invert(Factorial(k)));
        }
    }
}